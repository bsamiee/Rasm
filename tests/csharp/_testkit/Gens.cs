using CsCheck;
using Rhino.Geometry;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Gens {
    public static readonly Gen<double> Finite = Gen.Double[start: -1.0e6, finish: 1.0e6];
    public static readonly Gen<double> Positive = Gen.Double[start: 1.0e-6, finish: 1.0e6];
    public static readonly Gen<double> Tolerance = Gen.Double[start: 1.0e-12, finish: 1.0e-3];
    public static readonly Gen<double> UnitAngle = Gen.Double[start: 0.0, finish: 2.0 * Math.PI];
    public static readonly Gen<Point3d> Point = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Point3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> Vec = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Vector3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> NonZeroVec = Vec.Where(predicate: static v => v.Length > 1.0e-6);
    public static readonly Gen<Vector3d> UnitVec = NonZeroVec.Select(static (Vector3d v) => v / v.Length);
    public static readonly Gen<BoundingBox> Bbox = Point.Select(Point, static (Point3d a, Point3d b) => new BoundingBox(
        min: new Point3d(x: Math.Min(val1: a.X, val2: b.X), y: Math.Min(val1: a.Y, val2: b.Y), z: Math.Min(val1: a.Z, val2: b.Z)),
        max: new Point3d(x: Math.Max(val1: a.X, val2: b.X), y: Math.Max(val1: a.Y, val2: b.Y), z: Math.Max(val1: a.Z, val2: b.Z))));
    public static readonly Gen<BoundingBox> NonEmptyBbox = Bbox.Where(predicate: static b => b.IsValid && b.Diagonal.Length > 1.0e-6);
}
