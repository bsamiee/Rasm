
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Rhino.Arches.Core.Extensions
{
    public static partial class ArcGeometry
    {

        public static TwoArcsPartResult Get_ThreeCentered_Ogee_V1(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var spring = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (spring is null)
                return null;
            var r = ln.Length / 2;
            apexPoint = spring.ConstrianArcApex(r, apexPoint, ArcQuadType.GreaterThan180);

            //draw semi circle in apex direction
            var cir = new Circle(new Plane(spring.SpringCenter, ln.Direction, apexPoint - spring.SpringCenter), r);
            var quadArc = cir.GetArc(0, 90);
            

            var apexDir = apexPoint - spring.SpringCenter;
            apexDir.Unitize();

            //minimum rise required is semicircle height
            if (apexPoint.DistanceTo(spring.SpringCenter) < r)
            {
                apexPoint = spring.SpringCenter + apexDir * r;
            }

            // connect end point with apex point & get intersection with semi circle
            var endToApexLine = new Line(endPt, apexPoint);
 
            var iX = Intersection.LineCircle(endToApexLine, cir,
                out double tA, out Point3d pA, out double tB, out Point3d pB);

            if (iX == LineCircleIntersection.None)
                return null;
            var C = pB;

            var angle = RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, C - spring.SpringCenter));
            // limit maximum rise of apex point
            if (angle < 45)
            {
                C = quadArc.MidPoint;
                var EC = new Line(endPt, C);
                Intersection.LineLine(spring.CenterLine, EC,
                    out double a1, out double b1, RhinoMath.DefaultDistanceToleranceMillimeters, false);
                apexPoint = EC.PointAt(b1);
            }



            var MC = new Line(spring.SpringCenter, C);
            var apexPerp = new Line(apexPoint, ln.Direction, int.MaxValue);

            var iL = Intersection.LineLine(MC, apexPerp,
                 out double a, out double b, RhinoMath.DefaultDistanceToleranceMillimeters, false);
            if (!iL)
                return null;
            var B = apexPerp.PointAt(b);

            var circle_B = new Circle(new Plane(B, ln.Direction, apexDir),
                 B.DistanceTo(C));

            double minAngle = 360 - RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, C - B));
            double maxAngle = 180; 
            var endArc = circle_B.GetArc(minAngle, maxAngle);

            maxAngle = RhinoMath.ToDegrees(Vector3d.VectorAngle(ln.Direction, C - spring.SpringCenter));
            var startArc = cir.GetArc(0, maxAngle);

            return new TwoArcsPartResult(spring, startArc, endArc, apexPoint);
        }

        public static TwoArcsPartResult Get_ThreeCentered_Basket(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            if (Validate(startPt, endPt, out Line ln) == false)
                return null;

            var result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (result is null)
                return null;

            //find now apex point geometrically
            //take start side

            //divde line 4 parts
            var cPlane = new Plane(Point3d.Origin, ln.Direction, result.PerpendicularDir);
            var divLength = ln.Length / 4;

            var d1 = ln.PointAtLength(divLength);
            cPlane.Origin = d1;
            var c1 = new Circle(cPlane, divLength);

            //now draw circle from d1 to d3
            var d3 = ln.PointAtLength(divLength * 3);
            var c2 = new Circle(cPlane, d1.DistanceTo(d3));
            var centerLine = result.CenterLine;
            centerLine.Extend(1000, 1000);
            var cIX = Intersection.LineCircle(centerLine, c2,
                out double t1, out Point3d p1, out double t2, out Point3d p2);

            //check which point is in direction of refDirection

            bool isP1Valid = Vector3d.VectorAngle(result.PerpendicularDir, p1 - result.SpringCenter) > RhinoMath.DefaultAngleTolerance;
            Point3d basketArcCenter = isP1Valid ? p1 : p2;

            var quadDir = d1 - basketArcCenter;
            quadDir.Unitize();
            var c1Quad = c1.GetCircleQuad(quadDir);

            //double angle = Vector3d.VectorAngle(quadDir,ln.Direction);
            //double min_angle = 90 - RhinoMath.ToDegrees(angle);
            double min = RhinoMath.ToRadians(120);
            double max = RhinoMath.ToRadians(180);

            var startArc
             = new Arc(c1, new Interval(min, max));

            cPlane.Origin = basketArcCenter;
            double min_basket = RhinoMath.ToRadians(90);
            double max_basket = RhinoMath.ToRadians(120);

            var basketCircle = new Circle(cPlane, basketArcCenter.DistanceTo(c1Quad));
            var endArc = new Arc(basketCircle, new Interval(min_basket, max_basket));


            return new TwoArcsPartResult(result, startArc
            , endArc, startArc.EndPoint);
        }


        public static TwoArcsPartResult Get_ThreeCentered_Basket(Point3d startPt,
            Point3d endPt, Point3d apexPoint, Vector3d normal)
        {

            if (Validate(startPt, endPt, out Line ln) == false)
                return null;

            var spring = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (spring is null)
                return null;

            apexPoint = spring.CenterLine.ClosestPoint(apexPoint, false);
            var r = ln.Length / 4;
            double limitDistance = apexPoint.DistanceTo(spring.SpringCenter);

            var yDir = apexPoint - spring.SpringCenter;
            yDir.Unitize();

            var xDir = ln.Direction;

            if (limitDistance <= r)
            {
                apexPoint = spring.SpringCenter + yDir * (r + 0.001);
            }
            else if (limitDistance >= ln.Length / 2)
            {
                apexPoint = spring.SpringCenter + yDir * (ln.Length / 2 - 0.001);
            }


            var D = spring.SpringCenter.DistanceTo(apexPoint);
            var d_squared = Math.Pow(D, 2);
            var dNom = 2 * (D - r);
            var R = d_squared / dNom;

            var basketCenter = apexPoint - yDir * R;
            var basketPlane = new Plane(basketCenter, xDir, yDir);
            var basketCircle = new Circle(basketPlane, R);
            // RhinoDoc.ActiveDoc.Objects.AddPoint(basketCenter);

            var segCenter = ln.PointAtLength(r);

            var segPlane = new Plane(segCenter, xDir, yDir);
            var c1 = new Circle(segPlane, r);
            //RhinoDoc.ActiveDoc.Objects.AddCircle(c1);

            var quadDir = c1.Center - basketCenter;
            quadDir.Unitize();
            var c1Quad = c1.GetCircleQuad(quadDir);
            // RhinoDoc.ActiveDoc.Objects.AddPoint(c1Quad);

            double maxAngle = Math.PI; //180 degrees
            double minAngle = Vector3d.VectorAngle(xDir, quadDir);
            var endArc = new Arc(c1, new Interval(minAngle, maxAngle));

            var max_basketAngle = minAngle;
            var min_basketAngle = Math.PI / 2; //90 degrees

            var startArc = new Arc(basketCircle, new Interval(min_basketAngle, max_basketAngle));

            return new TwoArcsPartResult(spring, startArc, endArc, apexPoint);
        }


        public static TwoArcsPartResult Get_ThreeCentered_Depressed(Point3d startPt,
            Point3d endPt, Point3d apexPoint, Vector3d normal)
        {

            if (Validate(startPt, endPt, out Line springLine) == false)
                return null;

            var spring = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, springLine.Length * 2, normal);
            if (spring is null)
                return null;

            apexPoint = spring.CenterLine.ClosestPoint(apexPoint, false);

            double limitDistance = spring.SpringCenter.DistanceTo(apexPoint);
            var yDir = apexPoint - spring.SpringCenter;
            yDir.Unitize();
            var xDir = springLine.Direction;


            if (limitDistance >= springLine.Length / 2)
            {
                apexPoint = spring.SpringCenter + yDir * ((springLine.Length / 2) - RhinoMath.DefaultDistanceToleranceMillimeters);
            }
            else if (limitDistance < RhinoMath.DefaultDistanceToleranceMillimeters)
            {
                apexPoint = spring.SpringCenter + yDir * RhinoMath.DefaultDistanceToleranceMillimeters;
            }

            //draw semi circle arch
            var semicircular_arc = GetSemiCircularArc(startPt, endPt, normal);
            if (!semicircular_arc.HasValue)
                return null;


            var semiCircleQuad = semicircular_arc.Value.GetSArcQuad(yDir);

            var helperCircleRadi = apexPoint.DistanceTo(semiCircleQuad);
            var helperCircle = new Circle(new Plane(apexPoint, xDir, yDir), helperCircleRadi);

            var dir = startPt - apexPoint;
            dir.Unitize();
            var x = helperCircle.GetCircleQuad(dir);
            var SX = new Line(startPt, x);

            var mid_SX = SX.PointAt(0.5);
            var helper_perp = GetPerpendicular(startPt, x, normal);
            var perpLine = new Line(mid_SX, helper_perp, 1);
            perpLine.Extend(int.MaxValue, int.MaxValue);

            if (!Intersection.LineLine(perpLine, springLine,
               out double a, out double b, RhinoMath.DefaultDistanceToleranceMillimeters, false))
                return null;

            var sideCircle_Center = springLine.PointAt(b);

            if (!Intersection.LineLine(perpLine, spring.CenterLine,
                out a, out b, RhinoMath.DefaultDistanceToleranceMillimeters, false))
                return null;

            var basketCircle_Center = perpLine.PointAt(a);
            var basketCircle = new Circle(new Plane(basketCircle_Center, xDir, yDir),
                basketCircle_Center.DistanceTo(apexPoint));

            var basketQuad_dir = mid_SX - basketCircle_Center;
            basketQuad_dir.Unitize();


            double min_basketAngle = Math.PI / 2;//Vector3d.VectorAngle(xDir, basketQuad_dir);
            double max_basketAngle = Vector3d.VectorAngle(xDir, basketQuad_dir); //

            var endArc = new Arc(basketCircle, new Interval(min_basketAngle, max_basketAngle));

            double min_segAngle = Vector3d.VectorAngle(xDir, basketQuad_dir);
            double max_segAngle = Math.PI; //180 degrees


            var sideCircle = new Circle(new Plane(sideCircle_Center, xDir, yDir),
                sideCircle_Center.DistanceTo(startPt));

            var startArc = new Arc(sideCircle, new Interval(min_segAngle, max_segAngle));

            return new TwoArcsPartResult(spring, startArc, endArc, apexPoint);
        }

    }
}
