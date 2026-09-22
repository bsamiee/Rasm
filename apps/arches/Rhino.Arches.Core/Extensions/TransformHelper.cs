

using Rhino.Geometry;


namespace Rhino.Arches.Core.Extensions
{
    public static class TransformHelper
    {
        public static Arc? MirrorArc(this Arc arc, Point3d springCenter, Point3d springStart, Point3d springEnd)
        {
            var _mirrorPlane = new Plane(springCenter, springEnd - springStart);
            var _xForm = Transform.Mirror(_mirrorPlane);

            if (arc.Transform(_xForm))
                return arc;

            return null;
        }
    }
}
