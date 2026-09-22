
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Extensions
{
    public static partial class ArcGeometry
    {

        public static Arc? Get_TwoCentered_EquilateralArc(Point3d startPt, Point3d endPt, Vector3d normal, out ArcCenterLineResult result)
        {
            var ln = new Line(startPt, endPt);
            result = null;
            if (ln.Length < RhinoMath.ZeroTolerance)
                return null;

            // var mid = ln.PointAt(0.5);

            result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (result is null)
                return null;

            var apexPt = result.SpringCenter + result.PerpendicularDir * (ln.Length);

            var tempPlane = new Plane(startPt, ln.Direction, apexPt - result.SpringCenter);

            var arcCircle = new Circle(tempPlane, tempPlane.Origin.DistanceTo(endPt));
            var angleInterval = new Interval(0, RhinoMath.ToRadians(60));
            var arc = new Arc(arcCircle, angleInterval);

            return arc;

        }

        public static Arc? Get_TwoCentered_Depressed(Point3d startPt, Point3d endPt, Point3d apexPt, Vector3d normal,
            out ArcCenterLineResult result)
        {
            var ln = new Line(startPt, endPt);
            result = null;
            if (ln.Length < RhinoMath.ZeroTolerance)
                return null;

            result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (result is null)
                return null;

            apexPt = result.CenterLine.ClosestPoint(apexPt, false);

            var apexDir = apexPt - result.SpringCenter;
            apexDir.Unitize();

            var equilateralTriangleHeight = (Math.Sqrt(3) * ln.Length / 2);
            var apexToCenterDistance = apexPt.DistanceTo(result.SpringCenter);
            if (apexToCenterDistance > equilateralTriangleHeight)
            {
                apexPt = result.SpringCenter + apexDir * equilateralTriangleHeight;
            }
            else if (apexToCenterDistance < ln.Length / 2)
            {
                //its semicircle
                apexPt = result.SpringCenter + apexDir * ln.Length / 2;
            }


            double H = result.SpringCenter.DistanceTo(apexPt);


            // Radius
            double R = (ln.Length * ln.Length / 4.0 + H * H) / ln.Length;
            var springDir = ln.Direction;
            springDir.Unitize();

            Point3d centerLeft = endPt - springDir * R;

            //draw arc from endPoint to apexPoint using c1 top quad as center
            var tempPlane = new Plane(centerLeft, ln.Direction, apexDir);

            double minAngle = Vector3d.VectorAngle(ln.Direction, endPt - centerLeft);
            double maxAngle = Vector3d.VectorAngle(ln.Direction, apexPt - centerLeft);
            var arcCircle = new Circle(tempPlane, tempPlane.Origin.DistanceTo(endPt));
            var angleInterval = new Interval(-minAngle, maxAngle);
            var arc = new Arc(arcCircle, angleInterval);

            return arc;

        }

        public static Arc? Get_TwoCentered_Lancet(Point3d startPt, Point3d endPt, Point3d apexPt, Vector3d normal, out ArcCenterLineResult result)
        {
            var ln = new Line(startPt, endPt);
            result = null;
            if (ln.Length < RhinoMath.ZeroTolerance)
                return null;

            result = ArcGeometry.GetCenterLineFromEndPoints(startPt, endPt, ln.Length * 2, normal);
            if (result is null)
                return null;

            apexPt = result.CenterLine.ClosestPoint(apexPt, false);

            var apexDir = apexPt - result.SpringCenter;
            apexDir.Unitize();

            var equilateralTriangleHeight = (Math.Sqrt(3) * ln.Length / 2);
            if (apexPt.DistanceTo(result.SpringCenter) < equilateralTriangleHeight)
            {
                apexPt = result.SpringCenter + apexDir * equilateralTriangleHeight;
            }

            //double H_Min = (Math.Sqrt(3) / 2.0) * ln.Length;
            double H = result.SpringCenter.DistanceTo(apexPt);


            // Radius
            double R = (ln.Length * ln.Length / 4.0 + H * H) / ln.Length;
            var springDir = ln.Direction;
            springDir.Unitize();

            Point3d centerLeft = endPt - springDir * R;

            //draw arc from endPoint to apexPoint using c1 top quad as center
            var tempPlane = new Plane(centerLeft, ln.Direction, apexDir);

            double minAngle = Vector3d.VectorAngle(ln.Direction, endPt - centerLeft);
            double maxAngle = Vector3d.VectorAngle(ln.Direction, apexPt - centerLeft);
            var arcCircle = new Circle(tempPlane, tempPlane.Origin.DistanceTo(endPt));
            var angleInterval = new Interval(-minAngle, maxAngle);
            var arc = new Arc(arcCircle, angleInterval);

            return arc;
        }


    }
}
