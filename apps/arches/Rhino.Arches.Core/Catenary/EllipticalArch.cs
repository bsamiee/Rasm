using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Catenary
{
    public class EllipticalArch : RhinoArchBase
    {
        public static EllipticalArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var result = ArcGeometry.GetEllipticalArc(startPt, endPt, apexPoint, normal);
            if (result is null)
                return null;
            return new EllipticalArch()
            {
                StartPoint = startPt,
                EndPoint = endPt,
                ApexPoint = apexPoint,
                CenterLineResult = result.CenterLineResult,
                Curves = new List<Curve> { result.Cv }

            }; // You might want to initialize the EllipticalArch with the curve or other properties.
        }

        public override double GetMaximumPossibleRise()
        {
            return StartPoint.DistanceTo(EndPoint) / 2;
        }

        public override double GetMinimumPossibleRise()
        {
            return 0;
        }
    }
}
