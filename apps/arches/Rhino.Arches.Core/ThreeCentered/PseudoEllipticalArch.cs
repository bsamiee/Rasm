using Rhino.Geometry;

namespace Rhino.Arches.Core.ThreeCentered
{
    public class PseudoEllipticalArch : ThreeCenteredArchBase
    {
        public static PseudoEllipticalArch Build(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            throw new NotImplementedException();
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
