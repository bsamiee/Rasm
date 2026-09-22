using Rhino.Arches.Core.Extensions;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Multifoil
{
    public class MultifoilArch : RhinoArchBase
    {
        public static MultifoilArch Build(Point3d startPt, Point3d endPt, Vector3d normal, bool isPointed, int foilCount = 7)
        {
            var partResult = isPointed
                ? ArcGeometry.Get_MultifoilPointedArcs(startPt, endPt, normal, foilCount)
                : ArcGeometry.Get_MultifoilRoundedArcs(startPt, endPt, normal, foilCount);

            if (partResult is null)
                return null;

            var arch = new MultifoilArch
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
