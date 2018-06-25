using System.Linq;
using System.Windows.Media;

using Prism.Mvvm;

namespace RenderModule.Models
{
    /// <summary>
    /// Generates and update isocurves bindable to the Geometry path
    /// </summary>
    public class IsocurveLevel : BindableBase
    {
        /// <summary>
        /// bindable brush for curve color
        /// </summary>
        public Brush CurveColor
        {
            get { return _curveColor; }
            set { SetProperty(ref _curveColor, value); }
        }
        Brush _curveColor;

        /// <summary>
        /// determines the threshold of the isocurve
        /// </summary>
        public float Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }
        float _threshold;

        /// <summary>
        /// current isocurve geometry that can be bound to a Path element
        /// </summary>
        public PathGeometry Geometry
        {
            get { return _geometry; }
            set { SetProperty(ref _geometry, value); }
        }
        PathGeometry _geometry;

        /// <summary>
        /// set up the geometry based on the collection of line segments
        /// </summary>
        /// <param name="lineSegments"></param>
        public void UpdateGeometry(ComplexGeometry complexGeometry)
        {
            var pathFigures =
                from startSegment in complexGeometry
                select FromPointCollection(startSegment);

            PathGeometry path = new PathGeometry();
            path.Figures = new PathFigureCollection(pathFigures);
            Geometry = path;
        }

        // construct a pathfigure from the line segments
        static PathFigure FromPointCollection(LineSegments ls)
        {
            var pc = new PointCollection(ls);
            var pls = new PolyLineSegment(pc, true);
            var psc = new PathSegmentCollection();
            psc.Add(pls);
            return new PathFigure(pc.First(), psc, false);
        }
    }
}
