using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;


namespace Rhino.Arches.Core.HorseShoe
{
    public class PointedArch : RhinoArchBase
    {

        public Arc StartSide_Arc { get; private set; }
        public Arc EndSide_Arc { get; private set; }

        public Circle ApexCircle_1 { get; set; }

        public Circle C1 { get; set; }



        public PointedArch(Arc startSide_Arc, Arc endSide_Arc, Point3d apexPoint)
        {
            StartSide_Arc = startSide_Arc;
            EndSide_Arc = endSide_Arc;
            ApexPoint = apexPoint;

            this.Arcs.Add(StartSide_Arc);
            this.Arcs.Add(EndSide_Arc);
        }

        public static double GetMinimumRequiredHeight(Point3d start, Point3d endPt)
        {

            return start.DistanceTo(endPt) * Math.Sqrt(5.0 / 12.0);
        }

        static PointedArch Create(Point3d startPt,
            Point3d endPt,
            Point3d apexPt,
            Vector3d normal)
        {
            var arc_1 = ArcGeometry.GetHorseShoePointed_SideArc(startPt,
                endPt,
                apexPt,
                normal,
                out ArcCenterLineResult _result);

            if (arc_1 is null)
                return null;

            var arc_2 = arc_1.Value.MirrorArc(_result.SpringCenter, startPt, endPt);

            if (arc_2 is not null)
            {
                var arch = new PointedArch(arc_1.Value, arc_2.Value, arc_2.Value.EndPoint)
                {
                    StartPoint = startPt,
                    EndPoint = endPt,
                    ApexPoint = apexPt,
                    CenterLineResult = _result
                };

                return arch;
            }

            return null;
        }
        public static PointedArch Build(Point3d startPt,
            Point3d endPt,
            Point3d apexPt,
            Vector3d normal)
        {
            // apexPt = new Point3d(0, 5, 0);
            return Create(startPt, endPt, apexPt, normal);
        }

        public override double GetMinimumPossibleRise()
        {
            //Minimum possible rise is  sqrt(5/12) * span
            return this.StartPoint.DistanceTo(EndPoint) * Math.Sqrt(5.0 / 12.0);
        }

        public override double GetMaximumPossibleRise()
        {
            return int.MaxValue;
        }
    }
}
