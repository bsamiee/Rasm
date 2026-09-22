using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Multifoil
{
    public class TrefoilArch : RhinoArchBase
    {
        public static TrefoilArch Build(Point3d startPt, Point3d endPt, Vector3d normal,bool isPointed)
        {

           var partResult = isPointed ? ArcGeometry.Get_PointedTrefoilArch(startPt, endPt, normal) :
                ArcGeometry.Get_TrefoilArch(startPt, endPt, normal);

            if (partResult is null)
                return null;

            var arch = new TrefoilArch
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
