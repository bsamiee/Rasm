using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.ThreeCentered
{
    public class DepreesedArch : ThreeCenteredArchBase
    {
        public static double GetMaxPossibleHeight(Point3d start, Point3d end)
        {
            var d = start.DistanceTo(end) / 2;
            return d;
        }


        public static DepreesedArch Build(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            var result = ArcGeometry.Get_ThreeCentered_Depressed(startPt, endPt, apexPoint, normal);
            if (result == null)
                return null;

            var arch = new DepreesedArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = result.ApexPoint;
            arch.MirrorBuild(startPt, endPt,
                new List<Arc> { result.StartArc, result.EndArc }, result.CenterLineResult);

            return arch;
        }

        public override double GetMinimumPossibleRise()
        {
            //TODO:: Calculate minimum possible rise for basket handle arch
            return 0;
        }

        public override double GetMaximumPossibleRise()
        {
            //TODO:: Calculate maximum possible rise for basket handle arch
            return StartPoint.DistanceTo(EndPoint) / 4.0;
        }
    }
}
