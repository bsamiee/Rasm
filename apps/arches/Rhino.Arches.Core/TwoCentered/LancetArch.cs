

using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.TwoCentered
{
    public class LancetArch : TwoCenteredBase
    {
        public LancetArch()
        {
        }

        public static double GetMinimumPossibleHeight(Point3d startPt, Point3d endPt)
        {

            var equilateralTriangleHeight = (Math.Sqrt(3) * startPt.DistanceTo(endPt) / 2);

            return equilateralTriangleHeight;
        }

        public static LancetArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var arc_1 = ArcGeometry.Get_TwoCentered_Lancet(startPt, endPt, apexPoint, normal, out ArcCenterLineResult _result);
            if (arc_1 is null)
                return null;

            var arch = new LancetArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = apexPoint;

            arch.MirrorBuild(startPt, endPt, new List<Arc>() { arc_1.Value }, _result);

            return arch;
        }

        public override double GetMinimumPossibleRise()
        {
            //this is the height of equilateral triangle: (Math.Sqrt(3) / 2)
            // this arch can not go below equilateral triangle height
            return this.StartPoint.DistanceTo(this.EndPoint) * (Math.Sqrt(3) / 2);
        }

        public override double GetMaximumPossibleRise()
        {
            return int.MaxValue;
        }
    }
}
