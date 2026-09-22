using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Rhino.Arches.Core.Extensions
{
    public static partial class ArcGeometry
    {
        public static ArchCurveResult GetParabolicArc(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var springResult = GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);

            if (springResult is null)
                return null;

            // Constrain apex to the perpendicular centerline
            apexPoint = springResult.CenterLine.ClosestPoint(apexPoint, false);

            // Ensure apex has a minimum height above the spring line
            if (apexPoint.DistanceTo(springResult.SpringCenter) < RhinoMath.ZeroTolerance)
                apexPoint = springResult.SpringCenter + springResult.PerpendicularDir * (ln.Length / 2);

            // A degree-2 Bezier curve is a parabola.
            // C(t) = (1-t)^2 * P0 + 2t(1-t) * P1 + t^2 * P2
            // At t=0.5: C(0.5) = (P0 + 2*P1 + P2) / 4
            // Solving for P1 so the curve passes through apexPoint at t=0.5:
            // P1 = 2 * apexPoint - (P0 + P2) / 2
            var midBase = springResult.SpringCenter;
            var controlPt = apexPoint + (apexPoint - midBase);

            var curve = Curve.CreateControlPointCurve(new List<Point3d> { startPt, controlPt, endPt }, 2);
            //RhinoDoc.ActiveDoc.Objects.AddCurve(curve);
            return new ArchCurveResult(curve, springResult);
        }

        public static ArchCurveResult GetEllipticalArc(Point3d startPt, Point3d endPt, Point3d apexPoint, Vector3d normal)
        {
            if (!Validate(startPt, endPt, out Line ln))
                return null;

            var springResult = GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (springResult is null)
                return null;

            //pull apex to center line
            apexPoint = springResult.CenterLine.ClosestPoint(apexPoint, false);
            var r1 = ln.Length / 2; // Semi-major axis
            var r2 = apexPoint.DistanceTo(springResult.SpringCenter); // Semi-minor axis

            if (r2 < RhinoMath.ZeroTolerance)
                return null; // Apex is too close to the spring line, can't form a valid ellipse
            if (r2 > r1)
            {
                r2 = r1; // Limit the semi-minor axis to the length of the semi-major axis
            }
            var yDir = apexPoint - springResult.SpringCenter;
            var pln = new Plane(springResult.SpringCenter, ln.Direction, yDir);

            var elips = new Ellipse(pln, r1, r2);
            var elipseCv = elips.ToNurbsCurve();
            //split elips into 2 curves at the start and end points
            elipseCv.ClosestPoint(startPt, out double startParam);
            elipseCv.ClosestPoint(endPt, out double endParam);
            var splitCurves = elipseCv.Split(new List<double> { startParam, endParam });

            //find the only split curve that intersects with springResult.PerpendicularLine
            Curve selectedCurve = null;

            var perpendicularLine = new Line(springResult.SpringCenter, springResult.SpringCenter + yDir * ln.Length);
            foreach (var cv in splitCurves)
            {
                var intersect = Intersection.CurveLine(cv, perpendicularLine, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (intersect != null && intersect.Count > 0)
                {
                    selectedCurve = cv;
                    break;
                }
            }

            return new ArchCurveResult(selectedCurve, springResult);
        }
    }
}