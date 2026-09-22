using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Ogee
{
    public class ThreeCenteredOgeeArch : RhinoArchBase
    {
        public static ThreeCenteredOgeeArch Build(Point3d startPt, Point3d endPt,Point3d apexPoint, Vector3d normal)
        {
            //startPt = new Point3d(0, 0, 0);
            //endPt = new Point3d(10, 0, 0);
            //apexPoint = new Point3d(5, 10, 0);
            var partResult = ArcGeometry.Get_ThreeCentered_Ogee_V1(startPt, endPt,
                apexPoint, normal);


            if (partResult is null)
                return null;

            var arch = new ThreeCenteredOgeeArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = partResult.ApexPoint;
            arch.MirrorBuild(startPt, endPt,
                new List<Arc> { partResult.StartArc, partResult.EndArc },
                partResult.CenterLineResult);

            return arch;

        }

        public override double GetMaximumPossibleRise()
        {
            //TODO:: calculate possible max heihgt
            // come up with formaula later
            return int.MaxValue;
        }

        public override double GetMinimumPossibleRise()
        {
            //minimum is semicircle height
            return this.StartPoint.DistanceTo(this.EndPoint)/2;
        }
    }
}
