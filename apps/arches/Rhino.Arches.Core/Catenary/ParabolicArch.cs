using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Catenary
{
    public class ParabolicArch : RhinoArchBase
    {
        public static ParabolicArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var result = ArcGeometry.GetParabolicArc(startPt, endPt, apexPoint, normal);
            if (result is null)
                return null;
            return new ParabolicArch()
            {
                StartPoint = startPt,
                EndPoint = endPt,
                ApexPoint = apexPoint,
                CenterLineResult = result.CenterLineResult,
                Curves = new List<Curve> { result.Cv }
                
            }; // You might want to initialize the ParabolicArch with the curve or other properties.
        }

        public override double GetMaximumPossibleRise()
        {
            return int.MaxValue; // Parabolic arches can theoretically rise indefinitely, so we return a very large number.
        }

        public override double GetMinimumPossibleRise()
        {
            return 0; // The minimum rise for a parabolic arch can be zero (a straight line), so we return 0.
        }
    }
}
