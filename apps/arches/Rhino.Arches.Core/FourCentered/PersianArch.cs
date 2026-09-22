

using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.FourCentered
{
    public class PersianArch : RhinoArchBase
    {

        public static PersianArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            //startPt = new Point3d(0, 0, 0);
            //endPt = new Point3d(10, 0, 0);
            var partResult = ArcGeometry.Get_FourCenterArc_PersianV1(startPt, endPt, normal);

            if (partResult is null)
                return null;


            var arch = new PersianArch();
            arch.StartPoint = startPt;
            arch.EndPoint = endPt;
            arch.ApexPoint = partResult.ApexPoint;
            arch.MirrorBuild(startPt, endPt,
                new List<Arc> { partResult.StartArc, partResult.EndArc }, partResult.CenterLineResult);

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
