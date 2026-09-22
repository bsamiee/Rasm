

using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;


namespace Rhino.Arches.Core.Extensions
{
    public enum ArcQuadType
    {
        LesserThan180,
        GreaterThan180
    }

    public static partial class ArcGeometry
    {

        static Arc GetArc(this Circle cir, double minAngleInDegrees, double maxAngleInDegrees)
        {
            return new Arc(cir, new Interval(RhinoMath.ToRadians(minAngleInDegrees), RhinoMath.ToRadians(maxAngleInDegrees)));
        }
        static bool Validate(Point3d startPt, Point3d endPt, out Line ln)
        {
            ln = new Line(startPt, endPt);
            if (ln.Length < RhinoMath.ZeroTolerance)
                return false;

            return true;
        }


        public static Arc? GetHorseShoePointed_SideArc(Point3d startPt,
            Point3d endPt,
            Point3d apexPt,
            Vector3d normal, out ArcCenterLineResult result)
        {
            result = null;
            var ln = new Line(startPt, endPt);
            if (ln.Length < RhinoMath.ZeroTolerance)
                return null;

            result = GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (result == null)
                return null;
            double L = ln.Length;
            var mid = result.SpringCenter;

            apexPt = result.ConstrianArcApex(ln.Length / 2, apexPt, ArcQuadType.GreaterThan180);
            //RhinoDoc.ActiveDoc.Objects.AddPoint(apexPt);

            /*
Pointed Horseshoe Arch – Base Circle Radius (R0)

Given:
S  = spring line start point
E  = spring line end point
A  = apex point (centered above spring midpoint)

Definitions:
L = distance(S, E)                 // total spring line length
M = (S + E) * 0.5                  // midpoint of spring line
H = distance(A, M)                 // apex height above spring line

D1 = S + (1.0 / 3.0) * (E - S)     // left division point (fixed)
D2 = S + (2.0 / 3.0) * (E - S)     // right division point (fixed)

R0 = radius of the first two circles centered at D1 and D2
     such that their top quadrant points (Q1, Q2) generate
     the final arch circles which pass through the apex.

Formula:
R0 = (H*H - (5.0 / 12.0) * L*L) / (2.0 * H)

Validity condition:
H*H > (5.0 / 12.0) * L*L
*/
            double hMin = L * Math.Sqrt(5.0 / 12.0);
            double H = apexPt.DistanceTo(mid);

            var apexDir = apexPt - mid;
            apexDir.Unitize();

            if (H < hMin)
            {
                apexPt = mid + apexDir * hMin;
                H = hMin;
            }

            double hSquared = Math.Pow(H, 2);
            double fractionTerm = (5.0 / 12.0) * Math.Pow(L, 2);
            double subtract = Math.Abs(hSquared - fractionTerm);
            double circleRadius = subtract / (2 * H);

            //divide line 3 parts
            var divLength = ln.Length / 3;

            var d1 = ln.PointAtLength(divLength);
            var pln = new Plane(d1, ln.Direction, apexDir);
            //draw circle for d1
            var c1 = new Circle(pln, d1, circleRadius);

            //c1 top quadrant
            var c1Top = c1.GetCircleQuad(apexDir);

            //draw arc from endPoint to apexPoint using c1 top quad as center
            var tempPlane = new Plane(c1Top, ln.Direction, apexDir);

            double minAngle = Vector3d.VectorAngle(ln.Direction, endPt - c1Top);
            double maxAngle = Vector3d.VectorAngle(ln.Direction, apexPt - c1Top);
            var arcCircle = new Circle(tempPlane, tempPlane.Origin.DistanceTo(endPt));
            var angleInterval = new Interval(-minAngle, maxAngle);
            var arc = new Arc(arcCircle, angleInterval);

            return arc;

        }

        public static Arc? GetSemiCircularArc(Point3d arcStartPt,
            Point3d arcEndPt, Vector3d normal)
        {
            var dia = arcStartPt.DistanceTo(arcEndPt);
            if (dia < RhinoMath.ZeroTolerance)
                return null;

            var radi = dia / 2;
            var xAxis = arcEndPt - arcStartPt;
            xAxis.Unitize();

            // ydir = perpendicular
            Vector3d ydir = Vector3d.CrossProduct(normal, xAxis);
            ydir.Unitize();

            var mid = arcStartPt + xAxis * radi;
            Plane arcPlane = new(mid, xAxis, ydir);

            var arc = new Arc(arcPlane, dia / 2, Math.PI);

            return arc;
        }

        public static Vector3d GetBisector(Point3d arcStart, Point3d arcEnd, Point3d arcCenter)
        {
            var dir1 = arcStart - arcCenter;
            dir1.Unitize();
            var dir2 = arcEnd - arcCenter;
            dir2.Unitize();
            var bisector = (dir1 + dir2) / 2;
            bisector.Unitize();
            return bisector;
        }


        public static Vector3d GetPerpendicular(Point3d startPt, Point3d endPt, Vector3d normal)
        {
            var dir = endPt - startPt;
            dir.Unitize();
            var perpDir = Vector3d.CrossProduct(normal, dir);
            perpDir.Unitize();

            return perpDir;
        }

        public static ArcCenterLineResult GetCenterLineFromEndPoints(Point3d startPt, Point3d endPt,
    double lineLength, Vector3d normal)
        {
            //mid point
            var midPt = (startPt + endPt) / 2;

            //draw perpendicular line at mid point
            var perpDir = GetPerpendicular(startPt, endPt, normal);
            perpDir.Unitize();
            var lineStart = midPt - perpDir * lineLength / 2;
            var lineEnd = midPt + perpDir * lineLength / 2;

            var midLine = new Line(lineStart, lineEnd);

            return new ArcCenterLineResult(midLine, midPt, perpDir);
        }

        public static Point3d ConstrianArcApex(this ArcCenterLineResult result,
    double radi, Point3d possibleQuad, ArcQuadType quadType)
        {
            var quad = result.CenterLine.ClosestPoint(possibleQuad, false);

            var maxChordPt = result.SpringCenter + result.PerpendicularDir * radi;
            var minChordPt = result.SpringCenter - result.PerpendicularDir * radi;

            bool condition = quadType == ArcQuadType.LesserThan180
                ? quad.DistanceTo(result.SpringCenter) > radi
                : quad.DistanceTo(result.SpringCenter) < radi;

            if (condition)
            {
                if ((quad - result.SpringCenter) * result.PerpendicularDir > 0)
                {
                    return maxChordPt;
                }
                else
                {
                    return minChordPt;
                }
            }

            return quad;
        }
    }
}
