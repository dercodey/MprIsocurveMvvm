using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FsRenderModule.Interfaces;
using RenderModule.Interfaces;
using FsRenderModule.Interfaces;
using RenderModule.Models;

namespace RenderModule.Services
{
    using LineSegments = List<Point>;
    using ComplexGeometry = List<List<Point>>;

#if USE_ISOCURVE_CS
    /// <summary>
    /// 
    /// </summary>
    public class IsocurveFunction : IIsocurveFunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromImage"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public async Task<ComplexGeometry> GenerateIsocurveAsync(MprImageModelBase fromImage, double threshold)
        {
            var geometry = new ComplexGeometry();

            // stores the dictionary of segments
            var segments =
                new Dictionary<Point, LineSegments>();

            var pixels = await fromImage.GetPixelsAsync();

            if (pixels == null)
                return geometry;

            int height = pixels.GetLength(0);
            int width = pixels.GetLength(1);

            byte byteThreshold = (byte)threshold;

            // Y is increasing as we move down the image
            //      Upper is -Y
            //      Lower is +Y
            // for (int y = -height / 2; y < height / 2 - 1; y++)
            Parallel.For(-height / 2, height / 2 - 1,
                y =>
                {
                    for (int x = -width / 2; x < width / 2 - 1; x++)
                    {
                        // UL---UR    Bit order:
                        //  |/ \|     (UL) (UR) (LR) (LL)
                        //  |\ /|
                        // LL---LR

                        byte ul = pixels[y + height / 2, x + width / 2];
                        byte ur = pixels[y + height / 2, x + width / 2 + 1];
                        byte lr = pixels[y + height / 2 + 1, x + width / 2 + 1];
                        byte ll = pixels[y + height / 2 + 1, x + width / 2];

                        uint index = 0;
                        index |= (uint)((ul < byteThreshold) ? 0 : 1) << 3; // 1000
                        index |= (uint)((ur < byteThreshold) ? 0 : 1) << 2; // 0100
                        index |= (uint)((lr < byteThreshold) ? 0 : 1) << 1; // 0010
                        index |= (uint)((ll < byteThreshold) ? 0 : 1) << 0; // 0001

                        System.Diagnostics.Trace.Assert(index <= 0xF);
                        System.Diagnostics.Trace.Assert(index >= 0x0);

                        lock (geometry)
                        {
                            switch (index)
                            {
                                // UL---UR
                                //  |   |
                                //  |\  |
                                // LL---LR
                                case 0x1:  // 0001
                                case 0xE:  // 1110
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(-0.5, 0.0),
                                        new Vector(0.0, 0.5));
                                    break;

                                // UL---UR
                                //  |   |
                                //  |  /|
                                // LL---LR
                                case 0x2:  // 0010
                                case 0xD:  // 1101
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(0.0, 0.5),
                                        new Vector(0.5, 0.0));
                                    break;

                                // UL---UR
                                //  |___|
                                //  |   |
                                // LL---LR
                                case 0x3:  // 0011
                                case 0xC:  // 1100
                                    CreateOrAddSegment(geometry, segments, x, y,
                                       new Vector(-0.5, 0.0),
                                       new Vector(0.5, 0.0));
                                    break;

                                // UL---UR
                                //  |  \|
                                //  |   |
                                // LL---LR
                                case 0x4:   // 0100
                                case 0xB:   // 1011
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(0.0, -0.5),
                                        new Vector(0.5, 0.0));
                                    break;

                                // UL---UR
                                //  |  \|
                                //  |\  |
                                // LL---LR
                                case 0x5:   // 0101
                                case 0xA:   // 1010
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(0.0, -0.5),
                                        new Vector(0.5, 0.0));

                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(-0.5, 0.0),
                                        new Vector(0.0, 0.5));
                                    break;

                                // UL---UR
                                //  | | |
                                //  | | |
                                // LL---LR
                                case 0x6:   // 0110
                                case 0x9:   // 1001
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(0.0, -0.5),
                                        new Vector(0.0, 0.5));
                                    break;

                                // UL---UR
                                //  |/  |
                                //  |   |
                                // LL---LR
                                case 0x7:
                                case 0x8:
                                    CreateOrAddSegment(geometry, segments, x, y,
                                        new Vector(-0.5, 0.0),
                                        new Vector(0.0, -0.5));
                                    break;

                                case 0xF:
                                case 0x0:
                                default:
                                    // all on same side - do nothing
                                    break;

                            }
                        }
                    }
                });

            return geometry;
        }

        /// <summary>
        /// creates or adds a segment to the collections
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="startOffset"></param>
        /// <param name="endOffset"></param>
        public void CreateOrAddSegment(ComplexGeometry geometry,
            Dictionary<Point, LineSegments> segments, 
            int x, int y, Vector startOffset, Vector endOffset)
        {
            Point ptMiddle = new Point(x, y);
            Point startPoint = ptMiddle + startOffset;
            Point endPoint = ptMiddle + endOffset;

            if (segments.ContainsKey(startPoint))
            {
                var pc = segments[startPoint];
                segments.Remove(startPoint);

                if (pc.First() == startPoint)
                {
                    pc.Insert(0, endPoint);
                }
                else if (pc.Last() == startPoint)
                {
                    pc.Add(endPoint);
                }

                if (!segments.ContainsKey(endPoint))
                    segments.Add(endPoint, pc);
            }
            else if (segments.ContainsKey(endPoint))
            {
                var pc = segments[endPoint];
                segments.Remove(endPoint);

                if (pc.First() == endPoint)
                {
                    pc.Insert(0, startPoint);
                }
                else if (pc.Last() == endPoint)
                {
                    pc.Add(startPoint);
                }

                if (!segments.ContainsKey(startPoint))
                    segments.Add(startPoint, pc);
            }
            else
            {
                var pc = new LineSegments();
                pc.Add(startPoint);
                pc.Add(endPoint);
                segments.Add(startPoint, pc);
                segments.Add(endPoint, pc);

                geometry.Add(pc);
            }
        }
    }
#endif
}
