using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.FourCentered
{
    public class TudorArch : RhinoArchBase
    {

        public static TudorArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            var partResult = ArcGeometry.Get_FourCenterArc_Tudor(startPt, endPt, normal);

            if (partResult is null)
                return null;

            var arch = new TudorArch();
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
