
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;


namespace Rhino.Arches.Core.Extensions
{
    public static partial class ArcGeometry
    {

        public static TwoArcsPartResult Get_FourCenterArc_PersianV1(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var xDir = ln.Direction;
            xDir.Unitize();
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;
            var div_len = ln.Length / 4;
            var C1 = ln.PointAtLength(div_len);
            var C3 = ln.PointAtLength(div_len * 3);

            //evaluating for end side of span 
            //mirror arc outside the method if necessary

            //A,B,C (hytopthetical point) forms equilateral triangle
            var traingle_height = ((Math.Sqrt(3) * div_len * 2) / 2);
            var I = springResult.SpringCenter - springResult.PerpendicularDir * traingle_height;

            var C3_I = new Line(C3, I);


            var c3_plane = new Plane(C3, xDir, springResult.PerpendicularDir);
            var C3_Cir = new Circle(c3_plane, div_len);
            var B_Perp_Line = springResult.CenterLine;
            B_Perp_Line.Transform(Transform.Translation(C1 - springResult.SpringCenter));

            Intersection.LineLine(C3_I, B_Perp_Line, out double t0, out double t1,
                RhinoMath.DefaultDistanceToleranceMillimeters, false);

            var C4 = B_Perp_Line.PointAt(t1);
            var c4c3_dir = C3 - C4;
            c4c3_dir.Unitize();

            var X = C3_Cir.GetCircleQuad(c4c3_dir);
            var min_angle = 0.0;
            var max_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, X - C3));

            var startArc = C3_Cir.GetArc(min_angle, max_angle);

            var tempLn = new Line(springResult.SpringCenter, springResult.PerpendicularDir, ln.Length);

            var c4_plane = new Plane(C4, xDir, springResult.PerpendicularDir);
            var C4_Cir = new Circle(c4_plane, C4.DistanceTo(X));

            var iX = Intersection.LineCircle(tempLn, C4_Cir, out double tL, out Point3d p1, out double tC, out Point3d p2);
            if (iX == LineCircleIntersection.None)
                return null;

            var Y = p2;


            min_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, X - C4));
            max_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, Y - C4));

            var endArc = C4_Cir.GetArc(min_angle, max_angle);

            return new TwoArcsPartResult(springResult, startArc, endArc, endArc.EndPoint);
        }


        public static TwoArcsPartResult Get_FourCenterArc_PrsianV2(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var xDir = ln.Direction;
            xDir.Unitize();
            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;
            Point3d apexPoint = springResult.SpringCenter + springResult.PerpendicularDir * ln.Length / 2;
            //apexPoint = springResult.CenterLine.ClosestPoint(apexPoint, false);   

            var centerLn = new Line(springResult.SpringCenter, apexPoint);
            var yDir = centerLn.Direction;
            yDir.Unitize();

            var three_parts_dist = centerLn.Length / 3.0;
            var first_pt = centerLn.PointAtLength(three_parts_dist);
            double dist_A = three_parts_dist * 2;
            //var second_pt = centerLn.PointAtLength(dist_A);

            var end_B = endPt + xDir * dist_A;
            var small_circle = new Circle(new Plane(endPt, xDir, yDir), dist_A);


            var C = end_B + yDir * dist_A;
            var C3 = new Line(C, apexPoint);

            var c3_perp_ln = C3;

            c3_perp_ln.Transform(Transform.Rotation(RhinoMath.ToRadians(90), normal, apexPoint));
            var c3_perp_dir = c3_perp_ln.Direction;
            c3_perp_dir.Unitize();

            var c3_radi = apexPoint.DistanceTo(first_pt);
            var D = apexPoint + c3_perp_dir * c3_radi;

            var D_endPt = new Line(D, endPt);
            var d_end_mid = D_endPt.PointAt(0.5);
            var DB_perp_ln = new Line(d_end_mid, endPt);
            DB_perp_ln.Transform(Transform.Rotation(RhinoMath.ToRadians(90), normal, d_end_mid));

            var iX = Intersection.LineLine(c3_perp_ln, DB_perp_ln, out double a, out double b);
            if (!iX)
            {
                return null;
            }

            var E = c3_perp_ln.PointAt(a);
            var EB_dir = endPt - E;
            EB_dir.Unitize();
            //var quad_A = end_circle.GetCircleQuad(EB_dir);

            var small_arc_min_angle = 0;
            var small_arc_max_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, EB_dir));


            var startArc = small_circle.GetArc(small_arc_min_angle, small_arc_max_angle);


            var large_arc_min_angle = small_arc_max_angle;
            var large_arc_max_angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, apexPoint - E));

            var large_circle = new Circle(new Plane(E, xDir, yDir), E.DistanceTo(apexPoint));

            var endArc = large_circle.GetArc(large_arc_min_angle, large_arc_max_angle);
            //RhinoDoc.ActiveDoc.Objects.AddPoint(apexPoint);
            return new TwoArcsPartResult(springResult, startArc, endArc, endArc.EndPoint);
        }


        public static TwoArcsPartResult Get_FourCenterArc_Tudor(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;

            var xDir = ln.Direction;
            xDir.Unitize();

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            var yDir = springResult.PerpendicularDir;
            double span = ln.Length;
            double span_fourth = span / 4.0;
            // Square construction: top-right corner of square (above endPt)
            var bottomLeft = startPt - yDir * span / 2;
            bottomLeft += xDir * span_fourth;


            var C = startPt + xDir * (3 * span_fourth);

            var diagonal = C - bottomLeft;
            diagonal.Unitize();

            var circleA_plane = new Plane(C, xDir, yDir);
            var circle_A = new Circle(circleA_plane, span_fourth);
            var quad_A = circle_A.GetCircleQuad(diagonal);

            var circleB_plane = new Plane(bottomLeft, xDir, yDir);
            var circle_B = new Circle(circleB_plane, bottomLeft.DistanceTo(quad_A));

            //get arcs
            var startArc = circle_A.GetArc(0, RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, quad_A - C)));

            var mr_ln = new Line(springResult.SpringCenter, yDir, ln.Length);

            if (Intersection.LineCircle(mr_ln, circle_B, out _, out _, out _, out Point3d cPt) == LineCircleIntersection.None)
                return null;

            var min_ang = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, quad_A - bottomLeft));
            var maxAng = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, cPt - bottomLeft));
            var endArc = circle_B.GetArc(min_ang, maxAng);

            return new TwoArcsPartResult(springResult, startArc, endArc, endArc.EndPoint);
        }

        public static TwoArcsPartResult Get_FourCenterArc_Keel(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;

            var xDir = ln.Direction;
            xDir.Unitize();

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            int partCount = 4;
            var partLength = ln.Length / partCount;

            var start_up = startPt + springResult.PerpendicularDir * partLength;
            var C = start_up + xDir * partLength;
            var C_Plane = new Plane(C, xDir, springResult.PerpendicularDir);
            var C_Circle = new Circle(C_Plane, partLength * 2);
            var endArc = C_Circle.GetArc(0, 60);

            var end_Up = endPt + springResult.PerpendicularDir * partLength;
            var D = end_Up;
            var D_Plane = new Plane(D, xDir, springResult.PerpendicularDir);
            var D_Circle = new Circle(D_Plane, partLength);
            var startArc = D_Circle.GetArc(180, 270);

            return new TwoArcsPartResult(springResult, startArc, endArc, endArc.EndPoint);
        }

        public static TwoArcsPartResult Get_FourCerterArc_Ogee(
            Point3d startPt,
            Point3d endPt,
            Point3d apexPoint,
            Vector3d normal

        )
        {
            int partCount = 6;
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;
            var spring = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (spring is null)
                return null;
            var r = ln.Length / 2;
            apexPoint = spring.ConstrianArcApex(r, apexPoint, ArcQuadType.GreaterThan180);

            var xDir = new Vector3d(ln.Direction);
            xDir.Unitize();
            var yDir = apexPoint - spring.SpringCenter;
            yDir.Unitize();
            //minimum rise required is semicircle height
            if (apexPoint.DistanceTo(spring.SpringCenter) < r)
            {
                apexPoint = spring.SpringCenter + yDir * r;
            }

            var partLength = ln.Length / partCount;
            var C = spring.SpringCenter + xDir * partLength;
            var C_Radius = C.DistanceTo(endPt);
            var C_Plane = new Plane(C, xDir, yDir);
            var C_Circle = new Circle(C_Plane, C_Radius);
            //RhinoDoc.ActiveDoc.Objects.AddCircle(C_Circle);

            var E_APEX = new Line(endPt, apexPoint);
            var lCX = Intersection.LineCircle(E_APEX, C_Circle, out _, out _, out _, out Point3d C_Quad);
            if (lCX == LineCircleIntersection.None)
                return null;

            var maxAng = RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, C_Quad - C));

            var startArc = C_Circle.GetArc(0, maxAng);

            var B_Perp_Line = new Line(apexPoint
                , xDir, int.MaxValue);

            var C_QUAD_LINE = new Line(C, C_Quad);

            var iX = Intersection.LineLine(B_Perp_Line, C_QUAD_LINE, out double t0, out double t1,
                RhinoMath.DefaultDistanceToleranceMillimeters, false);
            if (!iX)
                return null;
            var D = B_Perp_Line.PointAt(t0);

            var D_Plane = new Plane(D, xDir, yDir);
            var D_Circle = new Circle(D_Plane, D.DistanceTo(apexPoint));
            var minAng = 360 - RhinoMath.ToDegrees(Vector3d.VectorAngle(xDir, C_Quad - D));
            maxAng = 180;
            var endArc = D_Circle.GetArc(minAng, maxAng);

            return new TwoArcsPartResult(spring, startArc, endArc, apexPoint);
        }

        public static TwoArcsPartResult Get_FourCenterArc_ReverseOgee(Point3d startPt,
            Point3d endPt, Vector3d normal)
        {
            //0.3114754098360656
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;

            var xDir = ln.Direction;
            xDir.Unitize();

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            var apexLen = ln.Length * 0.3114754098360656; //rise proportion of span
            var apexPoint = springResult.SpringCenter + springResult.PerpendicularDir * apexLen;

            var EA = new Line(endPt, apexPoint);
            int partCount = 4;
            var partLength = EA.Length / partCount;

            var EC = new Line(endPt, endPt + springResult.PerpendicularDir * ln.Length);

            var EA_MID = EA.PointAtLength(partLength * 2);
            var C = EA.PointAtLength(partLength);
            //Perpendicular direction to EA line
            var perp_dir = new Vector3d(EA.Direction);
            perp_dir.Unitize();
            perp_dir.Rotate(RhinoMath.ToRadians(90), normal);

            var C_Ln = new Line(C, perp_dir);
            C_Ln.Extend(ln.Length, ln.Length);

            var CX = Intersection.LineLine(EC, C_Ln, out double t0, out double t1,
                RhinoMath.DefaultDistanceToleranceMillimeters, false);

            if (CX == false)
                return null;

            var D = EC.PointAt(t0);
            var D_Plane = new Plane(D, xDir, springResult.PerpendicularDir);
            var Start_Circle = new Circle(D_Plane, D.DistanceTo(endPt));


            var F = EA.PointAtLength(partLength * 3);
            var F_Ln = new Line(F, perp_dir);
            F_Ln.Extend(ln.Length, ln.Length);
            var FX = Intersection.LineLine(springResult.CenterLine, F_Ln,
                out double t2, out double t3,
                RhinoMath.DefaultDistanceToleranceMillimeters, false);

            if (FX == false)
                return null;

            var K = springResult.CenterLine.PointAt(t2);
            var K_Plane = new Plane(K, xDir, springResult.PerpendicularDir);
            var End_Circle = new Circle(K_Plane, K.DistanceTo(apexPoint));

            var minAng = 270;
            var maxAng = 180 + RhinoMath.ToDegrees(
                Vector3d.VectorAngle(-xDir, EA_MID - D)
                );
            var startArc = Start_Circle.GetArc(minAng, maxAng);

            var minAng_End = 90;
            var maxAng_End = RhinoMath.ToDegrees(
                Vector3d.VectorAngle(xDir, EA_MID - K)
                );

            var endArc = End_Circle.GetArc(minAng_End, maxAng_End);

            return new TwoArcsPartResult(springResult, startArc, endArc, apexPoint);
        }

        public static TwoArcsPartResult Get_TentedArchArcs(Point3d startPt,
            Point3d endPt, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var springResult = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            var xDir = ln.Direction;
            xDir.Unitize();

            //startPt to endPt is a side of equilateral traingel based on that find heihgt of apex
            var triangle_height = (Math.Sqrt(3) / 2) * ln.Length;
            var apexPoint = springResult.SpringCenter + springResult.PerpendicularDir * triangle_height;
            var yDire = apexPoint - springResult.SpringCenter;
            yDire.Unitize();
            var partLength = ln.Length / 4;

            var end_to_apex = new Line(endPt, apexPoint);
            var endApexDir = end_to_apex.Direction;
            endApexDir.Unitize();

            var B = endPt + endApexDir * partLength;
            var D = endPt + endApexDir * partLength * 2;
            var C2 = endPt + yDire * triangle_height;
            var ED = new Line(endPt, D);

            var ED_spring = ArcGeometry.GetCenterLineFromEndPoints(endPt, D, partLength * 2, normal);
            if (ED_spring is null)
                return null;
            var ED_traingle_height = ED.Length * (Math.Sqrt(3) / 2);
            var apex_ED = ED_spring.SpringCenter - ED_spring.PerpendicularDir * ED_traingle_height;
            var C1 = apex_ED;

            var C1_Plane = new Plane(C1, xDir, yDire);
            var C1_Circle = new Circle(C1_Plane, C1.DistanceTo(endPt));
            var startArc = C1_Circle.GetArc(180, 240); //60 degrees difference
            var C2_Plane = new Plane(C2, xDir, yDire);
            var C2_Circle = new Circle(C2_Plane, C2.DistanceTo(apexPoint));
            var endArc = C2_Circle.GetArc(180, 240); //60 degrees

            return new TwoArcsPartResult(springResult, startArc, endArc, apexPoint);
        }
    }
}
