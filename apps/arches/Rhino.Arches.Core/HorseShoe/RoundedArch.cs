using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.HorseShoe
{
    public class RoundedArch : RhinoArchBase
    {
        public RoundedArch(Arc _arc, ArcCenterLineResult _result)
        {
            this.Arcs.Add(_arc);
            CenterLineResult = _result;
        }

        public static double GetMinimumPossibleHeight(Point3d startPt, Point3d endPt)
        {
            //get maximum possible height for segmental arch

            return startPt.DistanceTo(endPt) / 2.0;
        }

        public static RoundedArch Build(Point3d startPt, Point3d endPt,
            Point3d apexPoint, Vector3d normal)
        {
            var dia = startPt.DistanceTo(endPt);
            if (dia < RhinoMath.ZeroTolerance)
                return null;

            var result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, dia * 2, normal);

            var quad = result.ConstrianArcApex(dia / 2, apexPoint, ArcQuadType.GreaterThan180);

            var arc = new Arc(startPt, quad, endPt);

            if (arc.IsValid == false)
                return null;

            var arch = new RoundedArch(arc, result);
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = apexPoint;

            return arch;
        }

        public override double GetMinimumPossibleRise()
        {
            //should be at least semicircle, can not go below semicircle
            return this.StartPoint.DistanceTo(this.EndPoint) / 2.0;
        }

        public override double GetMaximumPossibleRise()
        {
            return int.MaxValue;
        }
    }
}
