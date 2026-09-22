

using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.TwoCentered
{
    public class DepressedArch : TwoCenteredBase
    {
        public static DepressedArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var arc_1 = ArcGeometry.Get_TwoCentered_Depressed(startPt, endPt, apexPoint, normal, out ArcCenterLineResult _result);
            if (arc_1 is null)
                return null;

            var arch = new DepressedArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = apexPoint;
            arch.MirrorBuild(startPt, endPt, new List<Arc>() { arc_1.Value }, _result);

            return arch;

        }


        public override double GetMaximumPossibleRise()
        {
            //this is the height of equilateral triangle: (Math.Sqrt(3) / 2)
            // this arch can go up to equilateral triangle height
            return this.StartPoint.DistanceTo(this.EndPoint) * (Math.Sqrt(3) / 2);
        }

        public override double GetMinimumPossibleRise()
        {
            //Minimum is semicircle
            return this.StartPoint.DistanceTo(this.EndPoint) / 2;
        }
    }
}
