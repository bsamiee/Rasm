using CsCheck;
using Rhino.Geometry;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Gens {
    public static readonly Gen<double> Finite = Gen.Double[start: -1.0e6, finish: 1.0e6];
    public static readonly Gen<double> Positive = Gen.Double[start: 1.0e-6, finish: 1.0e6];
    public static readonly Gen<double> PositiveFinite = Finite.Where(predicate: static x => x > 0.0);
    public static readonly Gen<double> Tolerance = Gen.Double[start: 1.0e-12, finish: 1.0e-3];
    public static readonly Gen<double> UnitAngle = Gen.Double[start: 0.0, finish: 2.0 * Math.PI];
    public static readonly Gen<double> Probability = Gen.Double.Unit;
    public static readonly Gen<Point3d> Point = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Point3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> Vec = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Vector3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> NonZeroVec = Vec.Where(predicate: static v => v.Length > 1.0e-6);
    public static readonly Gen<Vector3d> UnitVec = NonZeroVec.Select(static (Vector3d v) => v / v.Length);
    public static readonly Gen<Plane> Plane = Point.Select(UnitVec, static (Point3d origin, Vector3d normal) => new Plane(origin: origin, normal: normal));
    public static readonly Gen<Line> Line = Point.Select(Point, static (Point3d a, Point3d b) => new Line(from: a, to: b));
    public static readonly Gen<BoundingBox> Bbox = Point.Select(Point, static (Point3d a, Point3d b) => new BoundingBox(
        min: new Point3d(x: Math.Min(val1: a.X, val2: b.X), y: Math.Min(val1: a.Y, val2: b.Y), z: Math.Min(val1: a.Z, val2: b.Z)),
        max: new Point3d(x: Math.Max(val1: a.X, val2: b.X), y: Math.Max(val1: a.Y, val2: b.Y), z: Math.Max(val1: a.Z, val2: b.Z))));
    public static readonly Gen<BoundingBox> NonEmptyBbox = Bbox.Where(predicate: static b => b.IsValid && b.Diagonal.Length > 1.0e-6);
    public static Func<double, double, bool> Approx(double relativeTolerance = 1.0e-9) =>
        (a, b) => Math.Abs(value: a - b) <= relativeTolerance * Math.Max(val1: 1.0, val2: Math.Abs(value: a) + Math.Abs(value: b));
    public static Gen<T[]> SmallArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[0, 32];
    public static Gen<T[]> NonEmptyArray<T>(Gen<T> element, int max = 256) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[1, max];
    public static Gen<T[]> LargeArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[1_000, 10_000];
    public static Gen<T[]> UniqueArray<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).ArrayUnique[1, 64];
    public static Gen<T[]> SortedArray<T>(Gen<T> element) where T : IComparable<T> =>
        SmallArray(element: element ?? throw new ArgumentNullException(nameof(element))).Select(static a => a.OrderBy(static x => x).ToArray());
    public static Gen<(T Lo, T Hi)> OrderedPair<T>(Gen<T> element) where T : IComparable<T> =>
        (element ?? throw new ArgumentNullException(nameof(element))).Select(element, static (T a, T b) => a.CompareTo(b) <= 0 ? (Lo: a, Hi: b) : (Lo: b, Hi: a));
    public static Gen<Seq<T>> NonEmptySeq<T>(Gen<T> element, int max = 256) =>
        NonEmptyArray(element: element ?? throw new ArgumentNullException(nameof(element)), max: max).Select(static (T[] xs) => toSeq(xs));
}
