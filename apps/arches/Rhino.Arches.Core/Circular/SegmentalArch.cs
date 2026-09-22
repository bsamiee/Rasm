using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Circular
{
    public class SegmentalArch : CircularArchBase
    {

        public SegmentalArch(Arc arc, ArcCenterLineResult result) : base(arc, result)
        {
        }

        public static SegmentalArch Build(Point3d startPt, Point3d endPt, Point3d possibleQuad, Vector3d normal)
        {
            var dia = startPt.DistanceTo(endPt);
            if (dia < RhinoMath.ZeroTolerance)
                return null;

            var result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, dia * 2, normal);

            var quad = result.ConstrianArcApex(dia / 2, possibleQuad, ArcQuadType.LesserThan180);

            var arc = new Arc(startPt, quad, endPt);

            if (arc.IsValid == false)
                return null;

            var arch = new SegmentalArch(arc, result)
            {
                StartPoint = startPt,
                EndPoint = endPt,
                ApexPoint = possibleQuad
            };
            return arch;
        }

        public static double GetMaximumPossibleHeight(Point3d startPt, Point3d endPt)
        {
            //get maximum possible height for segmental arch
            return startPt.DistanceTo(endPt) / 2.0;
        }

        public override double GetMaximumPossibleRise()
        {
            //max can go up to semicircle
            return StartPoint.DistanceTo(EndPoint) / 2.0;
        }

        public override double GetMinimumPossibleRise()
        {
            //it can become a flat line
            return 0;
        }
    }
}
