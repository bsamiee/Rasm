using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Circular
{
    public class SemiCircularArch : CircularArchBase
    {
        public SemiCircularArch(Arc _arc, ArcCenterLineResult _result) : base(_arc, _result)
        {
        }

        public static SemiCircularArch BuildByCenter(Point3d center,
        Point3d arcEndPt, Vector3d normal)
        {
            var radius = center.DistanceTo(arcEndPt);
            var xAxis = arcEndPt - center;
            xAxis.Unitize();

            var startPt = center + xAxis * radius;
            var endPt = center - xAxis * radius;


            var result = ArcGeometry.GetCenterLineFromEndPoints(
                startPt, endPt,
                radius * 2,
                normal);

            var arc = ArcGeometry.GetSemiCircularArc(startPt, endPt, normal);

            if (arc == null)
                return null;

            var arch = new SemiCircularArch(arc.Value, result)
            {
                StartPoint = startPt,
                EndPoint = endPt
            };

            return arch;
        }



        public static SemiCircularArch BuildByEndPoints(Point3d arcStartPt, Point3d arcEndPt, Vector3d normal)
        {

            var arc = ArcGeometry.GetSemiCircularArc(arcStartPt, arcEndPt, normal);
            if (arc == null)
                return null;

            var result = ArcGeometry.GetCenterLineFromEndPoints(
                        arcStartPt,
                        arcEndPt,
                    arc.Value.Diameter, normal);

            var arch = new SemiCircularArch(arc.Value, result);
            arch.StartPoint = arcStartPt;
            arch.EndPoint = arcEndPt;
            return arch;
        }

        public override double GetMaximumPossibleRise()
        {
            return this.StartPoint.DistanceTo(this.EndPoint) / 2.0;
        }

        public override double GetMinimumPossibleRise()
        {
            return this.StartPoint.DistanceTo(this.EndPoint) / 2.0;
        }
    }
}
