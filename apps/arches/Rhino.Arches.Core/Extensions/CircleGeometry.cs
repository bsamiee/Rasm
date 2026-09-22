using Rhino.Geometry;

namespace Rhino.Arches.Core.Extensions
{
    public static class CircleGeomGetry
    {
        public static Point3d GetCircleQuad(this Circle cir, Vector3d quadDir)
        {
            double radius = cir.Radius;
            var center = cir.Center;
            return center + (quadDir * radius);
        }

        public static Point3d GetSArcQuad(this Arc arc, Vector3d quadDir)
        {
            double radius = arc.Radius;
            var center = arc.Center;
            return center + (quadDir * radius);
        }
    }
}