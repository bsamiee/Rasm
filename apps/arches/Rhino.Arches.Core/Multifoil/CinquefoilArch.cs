using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Multifoil
{
    public class CinquefoilArch : RhinoArchBase
    {
        public static CinquefoilArch Build(Point3d startPt, Point3d endPt, Vector3d normal, bool isPointed)
        {
            //startPt = new Point3d(0, 0, 0);
            //endPt = new Point3d(5, 0, 0);
            var partResult = isPointed
                ? ArcGeometry.Get_CinquefoilArcs_Pointed(startPt, endPt, normal)
                : ArcGeometry.Get_CinquefoilArcs_Rounded(startPt, endPt, normal, numberOfFoils: 3);
            if (partResult is null)
                return null;

            var arch = new CinquefoilArch
            {
                StartPoint = startPt,
                EndPoint = endPt,
                ApexPoint = partResult.ApexPoint
            };
            arch.MirrorBuild(startPt, endPt,
                partResult.Arcs,
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
