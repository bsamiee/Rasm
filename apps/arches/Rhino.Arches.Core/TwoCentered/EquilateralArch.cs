
using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.TwoCentered
{
    public class EquilateralArch : TwoCenteredBase
    {

        public static EquilateralArch Build(
         Point3d startPt,
         Point3d endPt, Vector3d normal)
        {
            var arc_1 = ArcGeometry.Get_TwoCentered_EquilateralArc(startPt, endPt, normal, out ArcCenterLineResult _result);
            if (arc_1 is null)
                return null;

            var arch = new EquilateralArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = arc_1.Value.EndPoint;
            arch.MirrorBuild(startPt, endPt, new List<Arc>() { arc_1.Value }, _result);

            return arch;
        }

        public override double GetMaximumPossibleRise()
        {
            return this.StartPoint.DistanceTo(this.EndPoint) * (Math.Sqrt(3) / 2);
        }

        public override double GetMinimumPossibleRise()
        {
            return this.StartPoint.DistanceTo(this.EndPoint) * (Math.Sqrt(3) / 2);
        }
    }
}
