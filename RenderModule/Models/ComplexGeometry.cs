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
    }

    /// <summary>
    /// 
    /// </summary>
    public class LineSegments : List<Point> { }
}
