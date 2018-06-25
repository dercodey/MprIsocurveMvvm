using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RenderModule.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ComplexGeometry : List<LineSegments>
    {
        // stores the dictionary
        Dictionary<Point, LineSegments> _segments =
            new Dictionary<Point, LineSegments>();

        /// <summary>
        /// creates or adds a segment to the collections
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="startOffset"></param>
        /// <param name="endOffset"></param>
        public void CreateOrAddSegment(int x, int y, Vector startOffset, Vector endOffset)
        {
            Point ptMiddle = new Point(x, y);
            Point startPoint = ptMiddle + startOffset;
            Point endPoint = ptMiddle + endOffset;

            if (_segments.ContainsKey(startPoint))
            {
                var pc = _segments[startPoint];
                _segments.Remove(startPoint);

                if (pc.First() == startPoint)
                {
                    pc.Insert(0, endPoint);
                }
                else if (pc.Last() == startPoint)
                {
                    pc.Add(endPoint);
                }

                if (!_segments.ContainsKey(endPoint))
                    _segments.Add(endPoint, pc);
            }
            else if (_segments.ContainsKey(endPoint))
            {
                var pc = _segments[endPoint];
                _segments.Remove(endPoint);

                if (pc.First() == endPoint)
                {
                    pc.Insert(0, startPoint);
                }
                else if (pc.Last() == endPoint)
                {
                    pc.Add(startPoint);
                }

                if (!_segments.ContainsKey(startPoint))
                    _segments.Add(startPoint, pc);
            }
            else
            {
                var pc = new LineSegments();
                pc.Add(startPoint);
                pc.Add(endPoint);
                _segments.Add(startPoint, pc);
                _segments.Add(endPoint, pc);

                this.Add(pc);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LineSegments : List<Point> { }
}
