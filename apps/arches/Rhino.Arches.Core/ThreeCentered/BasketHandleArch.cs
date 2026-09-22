using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.ThreeCentered
{
    public class BasketHandleArch : ThreeCenteredArchBase
    {


        public static double GetMinimumPossibleHeight(Point3d start, Point3d end)
        {
            var d = start.DistanceTo(end) / 4;
            return d;
        }
        public static BasketHandleArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            var result = ArcGeometry.Get_ThreeCentered_Basket(startPt, endPt, normal);
            if (result == null)
                return null;

            var arch = new BasketHandleArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = result.ApexPoint;
            arch.MirrorBuild(startPt, endPt,
                new List<Arc> { result.StartArc, result.EndArc }, result.CenterLineResult);


            return arch;
        }
        public static BasketHandleArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var result = ArcGeometry.Get_ThreeCentered_Basket(startPt, endPt, apexPoint, normal);
            if (result == null)
                return null;

            var arch = new BasketHandleArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = result.ApexPoint;
            arch.MirrorBuild(startPt, endPt,
             new List<Arc> { result.StartArc, result.EndArc }, result.CenterLineResult);


            return arch;
        }

        public override double GetMinimumPossibleRise()
        {
            //can become a flat line
            return 0;
        }

        public override double GetMaximumPossibleRise()
        {
            //can go up semi circular height
            return StartPoint.DistanceTo(EndPoint) / 2.0;
        }
    }
}
