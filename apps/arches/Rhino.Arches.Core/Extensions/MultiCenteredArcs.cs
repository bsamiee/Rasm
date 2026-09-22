
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Extensions
{
    public static partial class ArcGeometry
    {
        public static MultiArcsPartResult Get_MultifoilRoundedArcs(Point3d startPt, Point3d endPt, Vector3d normal, int numberOfFoils)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;
            //divide arc into segments
            var circle = new Circle(new Plane(springResult.SpringCenter, ln.Direction, springResult.PerpendicularDir), ln.Length / 2);

            var rightSideArc = circle.GetArc(0, 90);
            // RhinoDoc.ActiveDoc.Objects.AddArc(rightSideArc);

            int divCount = (int)Math.Round((double)numberOfFoils / 2, MidpointRounding.ToPositiveInfinity) * 2;
            var ts = rightSideArc.ToNurbsCurve().DivideByCount(divCount, true, out Point3d[] divisionPts);

            if (ts is null || divisionPts is null || divisionPts.Length < 2)
                return null;


            var arcs = new List<Arc>();

            var s = divisionPts[1];
            var e = divisionPts[3];

            var m = divisionPts[2];
            var t = rightSideArc.ClosestParameter(m);
            var tanget = rightSideArc.TangentAt(t);
            //find perpendicular to tangent at mid pt
            var perp = Vector3d.CrossProduct(tanget, normal);
            perp.Unitize();
            //create apex point
            var apexPt = m + perp * e.DistanceTo(s) / 2;

            var hs_arch = HorseShoe.RoundedArch.Build(s, e, apexPt, normal);
            if (hs_arch is null)
                return null;

            arcs.AddRange(hs_arch.Arcs);

            var temp_dir = m - springResult.SpringCenter;
            for (int i = 4; i < divisionPts.Length - 1; i += 2)
            {
                //var angle = Vector3d.VectorAngle(temp_dir, );
                var endDir = divisionPts[i] - springResult.SpringCenter;
                foreach (var a in hs_arch.Arcs)
                {
                    var success = a.Transform(Transform.Rotation(temp_dir, endDir, springResult.SpringCenter));
                    if (success)
                    {
                        arcs.Add(a);
                    }
                }
            }

            //render top arc
            var topPt = divisionPts[^1];
            var prv_Pt = divisionPts[^2];


            var cTop_Plane = new Plane(topPt, ln.Direction, springResult.PerpendicularDir);
            var cTopCircle = new Circle(cTop_Plane, topPt.DistanceTo(prv_Pt));
            var minAngle = 0 - RhinoMath.ToDegrees(Vector3d.VectorAngle(cTop_Plane.XAxis, prv_Pt - topPt));
            double maxAngle = 90;
            var top_arc = cTopCircle.GetArc(minAngle, maxAngle);
            arcs.Add(top_arc);

            //render bottom arc
            //bottom arc is a segmental arc
            var cBottom_pln = new Plane(endPt, ln.Direction, springResult.PerpendicularDir);
            var bottomCircle = new Circle(cBottom_pln, endPt.DistanceTo(divisionPts[1]));
            minAngle = 0;
            maxAngle = RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, divisionPts[1] - endPt));
            var bottomArc = bottomCircle.GetArc(minAngle, maxAngle);
            arcs.Add(bottomArc);
            //scale down
            var scl_startPt = bottomArc.StartPoint;

            var scl_factor = (ln.Length * 0.5) / springResult.SpringCenter.DistanceTo(scl_startPt);

            var scaledArcs = new List<Arc>();
            foreach (var a in arcs)
            {
                //var tempArc = a;
                a.Transform(Transform.Scale(springResult.SpringCenter, scl_factor));

                scaledArcs.Add(a);
            }

            return new MultiArcsPartResult(scaledArcs, springResult, top_arc.EndPoint);
        }

        public static MultiArcsPartResult Get_MultifoilPointedArcs(Point3d startPt, Point3d endPt, Vector3d normal, int numberOfFoils)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;
            //divide arc into segments
            var circle = new Circle(new Plane(startPt, ln.Direction, springResult.PerpendicularDir), ln.Length);

            var rightSideArc = circle.GetArc(0, 60);
            // RhinoDoc.ActiveDoc.Objects.AddArc(rightSideArc);

            int divCount = (int)Math.Round((double)numberOfFoils / 2, MidpointRounding.ToPositiveInfinity) * 2;
            var ts = rightSideArc.ToNurbsCurve().DivideByCount(divCount, true, out Point3d[] divisionPts);

            if (ts is null || divisionPts is null || divisionPts.Length < 2)
                return null;


            var arcs = new List<Arc>();

            //build series of horse shoe arches on the right side arc
            for (int i = 1; i < divisionPts.Length - 2; i += 2)
            {
                var s = divisionPts[i];
                var e = divisionPts[i + 2];

                var m = divisionPts[i + 1];
                var t = rightSideArc.ClosestParameter(m);
                var tanget = rightSideArc.TangentAt(t);
                //find perpendicular to tangent at mid pt
                var perp = Vector3d.CrossProduct(tanget, normal);
                perp.Unitize();
                //create apex point
                var apexPt = m + perp * e.DistanceTo(s) / 2;

                var hs_arch = HorseShoe.RoundedArch.Build(s, e, apexPt, normal);
                if (hs_arch is null)
                    return null;

                arcs.AddRange(hs_arch.Arcs);
            }

            // //render top arc
            var topPt = divisionPts[divisionPts.Length - 1];
            var prv_Pt = divisionPts[divisionPts.Length - 2];


            var cTop_Plane = new Plane(topPt, ln.Direction, springResult.PerpendicularDir);
            var cTopCircle = new Circle(cTop_Plane, topPt.DistanceTo(prv_Pt));
            var x_to_top_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(cTop_Plane.XAxis, prv_Pt - topPt));
            var minAngle = 0 - x_to_top_angle;
            double maxAngle = 90;
            var top_arc = cTopCircle.GetArc(minAngle, maxAngle);
            arcs.Add(top_arc);

            //render bottom arc
            //bottom arc is a segmental arc
            var cBottom_pln = new Plane(endPt, ln.Direction, springResult.PerpendicularDir);
            var bottomCircle = new Circle(cBottom_pln, endPt.DistanceTo(divisionPts[1]));
            minAngle = 0;
            maxAngle = RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, divisionPts[1] - endPt));
            var bottomArc = bottomCircle.GetArc(minAngle, maxAngle);
            arcs.Add(bottomArc);
            //scale down
            var scl_startPt = bottomArc.StartPoint;

            var scl_factor = (ln.Length * 0.5) / springResult.SpringCenter.DistanceTo(scl_startPt);

            var scaledArcs = new List<Arc>();
            foreach (var a in arcs)
            {
                //var tempArc = a;
                a.Transform(Transform.Scale(springResult.SpringCenter, scl_factor));

                scaledArcs.Add(a);
            }

            return new MultiArcsPartResult(scaledArcs, springResult, top_arc.EndPoint);
        }


        public static MultiArcsPartResult Get_CinquefoilArcs_Rounded(Point3d startPt, Point3d endPt, Vector3d normal, int numberOfFoils)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;
            //divide arc into segments
            var circle = new Circle(new Plane(springResult.SpringCenter, ln.Direction, springResult.PerpendicularDir), ln.Length / 2);

            var rightSideArc = circle.GetArc(0, 90);
            //RhinoDoc.ActiveDoc.Objects.AddArc(rightSideArc);

            int divCount = (int)Math.Round((double)numberOfFoils / 2, MidpointRounding.ToPositiveInfinity) * 2;
            var ts = rightSideArc.ToNurbsCurve().DivideByCount(divCount + 1, true, out Point3d[] divisionPts);

            if (ts is null || divisionPts is null || divisionPts.Length < 2)
                return null;


            var arcs = new List<Arc>();
            var tempArcs = new List<Arc>();
            for (int i = 1; i < divisionPts.Length; i += 2)
            {
                var c = divisionPts[i];

                var t = rightSideArc.ClosestParameter(c);
                var y = rightSideArc.TangentAt(t);
                //find perpendicular to tangent at mid pt
                var x = Vector3d.CrossProduct(y, normal);
                x.Unitize();

                var pln = new Plane(c, x, y);

                var cC = new Circle(pln, c.DistanceTo(divisionPts[i - 1]));

                var minAng = RhinoMath.ToDegrees(Vector3d.VectorAngle(x, divisionPts[i - 1] - c));
                var maxAng = -minAng;

                var arc = cC.GetArc(minAng, maxAng);
                tempArcs.Add(arc);

            }
            //first arc
            var firstArc = tempArcs[0];
            var quadPt = firstArc.GetSArcQuad(ln.Direction);
            var x_dir = firstArc.Plane.XAxis;
            var quadDir = quadPt - firstArc.Center;
            var start_angle = Vector3d.VectorAngle(x_dir, quadDir);
            firstArc.EndAngle = start_angle;

            //last arc 
            var lastArc = tempArcs[^1];
            var last_quadPt = lastArc.GetSArcQuad(springResult.PerpendicularDir);
            var last_quadDir = last_quadPt - lastArc.Center;
            var last_start_angle = Vector3d.VectorAngle(lastArc.Plane.XAxis, last_quadDir);
            lastArc.StartAngle = last_start_angle;

            //add tp arcs in correct order
            arcs.Add(firstArc);
            for (int i = 1; i < tempArcs.Count - 1; i++)
            {
                arcs.Add(tempArcs[i]);
            }
            arcs.Add(lastArc);


            var results = new List<Arc>();
            //move & scale arc to actualy span line

            var sclPt = ln.ClosestPoint(firstArc.EndPoint, false);

            var dist = springResult.SpringCenter.DistanceTo(sclPt);
            var scl_factor = (ln.Length * 0.5) / dist;
            var move_vec = sclPt - firstArc.EndPoint;

            arcs.ForEach(a =>
            {

                if (a.Transform(Transform.Translation(move_vec)))
                {

                    if (a.Transform(Transform.Scale(springResult.SpringCenter, scl_factor)))
                    {
                        results.Add(a);
                        //RhinoDoc.ActiveDoc.Objects.AddArc(a);
                    }
                }
            });


            return new MultiArcsPartResult(results, springResult, lastArc.StartPoint);
        }

        public static MultiArcsPartResult Get_CinquefoilArcs_Pointed(Point3d startPt, Point3d endPt,
            Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            //find heihgt of equlateral trinagle based on ln
            double height = ln.Length * Math.Sqrt(3) / 2;
            var apexPoint = springResult.SpringCenter + springResult.PerpendicularDir * height;
            var circle = new Circle(new Plane(startPt, ln.Direction, springResult.PerpendicularDir), ln.Length);
            var arc = circle.GetArc(0, RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, apexPoint - startPt)));

            //divide arc into 6 segments including end points
            int divCount = 6;
            arc.ToNurbsCurve().DivideByCount(divCount, true, out Point3d[] divisionPts);

            var circ_dist = endPt.DistanceTo(divisionPts[1]);
            //drow half foli for start
            var start_cir = new Circle(new Plane(endPt, ln.Direction, springResult.PerpendicularDir), circ_dist);
            var start_arc = start_cir.GetArc(0, RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, divisionPts[1] - endPt)));

            //draw mid arch
            var mid_cir = new Circle(new Plane(divisionPts[2], ln.Direction, springResult.PerpendicularDir), circ_dist);
            var mid_arc = mid_cir.GetArc(-RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, divisionPts[1] - divisionPts[2])),
                RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, divisionPts[3] - divisionPts[2])));

           //draw equlateral pointed arch
           var pointed_end = divisionPts[3];

            //mirror on perp line to get other side of arch
            var pointed_start = pointed_end;

            pointed_start.Transform(Transform.Mirror(apexPoint,ln.Direction));

            var pointed_arc = ArcGeometry.Get_TwoCentered_EquilateralArc(pointed_start, pointed_end, normal, out ArcCenterLineResult eqArc);
            if(pointed_arc is null)
                return null;

            //now scale all 3 arcs
            var large_end = start_arc.StartPoint;
            var scale_factor = (ln.Length *0.5) / springResult.SpringCenter.DistanceTo(large_end);

            start_arc.Transform(Transform.Scale(springResult.SpringCenter, scale_factor));
            mid_arc.Transform(Transform.Scale(springResult.SpringCenter, scale_factor));

            var p_arc = pointed_arc.Value;
            p_arc.Transform(Transform.Scale(springResult.SpringCenter, scale_factor));

            

            return new MultiArcsPartResult(new List<Arc>() {
                start_arc, mid_arc, p_arc
            }, springResult, p_arc.EndPoint);
        }


        public static MultiArcsPartResult Get_TrefoilArch(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;


            //get 4th part length of span line
            var part_length = ln.Length / 4;
            var xDir = ln.Direction;
            xDir.Unitize();
            var yDir = springResult.PerpendicularDir;
            var c = springResult.SpringCenter + xDir * part_length;
            var pln = new Plane(c, xDir, yDir);
            var cC = new Circle(pln, part_length);
            var baseArc = cC.GetArc(0, 90);
            c = springResult.SpringCenter + yDir * part_length;
            pln = new Plane(c, xDir, yDir);
            cC = new Circle(pln, part_length);
            var topArc = cC.GetArc(0, 90);

            var arcs = new List<Arc> { baseArc, topArc };
            return new MultiArcsPartResult(arcs, springResult, topArc.EndPoint);
        }



        public static MultiArcsPartResult Get_PointedTrefoilArch(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;


            //get 4th part length of span line
            var part_length = ln.Length / 4;
            var xDir = ln.Direction;
            xDir.Unitize();
            var yDir = springResult.PerpendicularDir;
            var c = springResult.SpringCenter + xDir * part_length;
            var pln = new Plane(c, xDir, yDir);
            var cC = new Circle(pln, part_length);
            var baseArc = cC.GetArc(0, 90);

            var pointedEnd = cC.GetCircleQuad(springResult.PerpendicularDir);
            var pointed_start = pointedEnd - xDir * part_length * 2;

            var topArc = ArcGeometry.Get_TwoCentered_EquilateralArc(pointed_start, pointedEnd, normal, out _);
            if (topArc is null)
                return null;

            var arcs = new List<Arc> { baseArc, topArc.Value };
            return new MultiArcsPartResult(arcs, springResult, topArc.Value.EndPoint);
        }

    }
}
