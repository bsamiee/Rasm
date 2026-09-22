using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.FourCentered
{
    public class KeelArch : RhinoArchBase
    {
        public static KeelArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
           var partResult = ArcGeometry.Get_FourCenterArc_Keel(startPt, endPt,
                normal);

            if (partResult is null)
                return null;

            var arch = new KeelArch
            {
                StartPoint = startPt,
                EndPoint = endPt,
                CenterLineResult = partResult.CenterLineResult,
                ApexPoint = partResult.ApexPoint
            };

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
