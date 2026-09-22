using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Ogee
{
    public class ReverseOgeeArch : RhinoArchBase
    {
        public static ReverseOgeeArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            //startPt = new Point3d(0, 0, 0);
            //endPt = new Point3d(10, 0, 0);
            //apexPoint = new Point3d(5, 10, 0);
            var partResult = ArcGeometry.Get_FourCenterArc_ReverseOgee(startPt, endPt, normal);

            if (partResult is null)
                return null;

            var arch = new ReverseOgeeArch();
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
            throw new NotImplementedException();
        }

        public override double GetMinimumPossibleRise()
        {
            throw new NotImplementedException();
        }
    }
}
