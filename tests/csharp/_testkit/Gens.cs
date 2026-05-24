using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Gens {
    // --- [SCALARS] -------------------------------------------------------------------------
    public static readonly Gen<double> Finite = Gen.Double[start: -1.0e6, finish: 1.0e6];
    public static readonly Gen<double> NonFinite = Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity);
    public static readonly Gen<double> NonPositive = Finite.Where(predicate: static x => x <= 0.0);
    public static readonly Gen<double> Positive = Gen.Double[start: 1.0e-6, finish: 1.0e6];
    public static readonly Gen<double> PositiveFinite = Finite.Where(predicate: static x => x > 0.0);
    public static readonly Gen<double> Tolerance = Gen.Frequency(
        (90, Gen.Double[start: 1.0e-12, finish: 1.0e-3]),
        (10, Gen.OneOfConst(RhinoMath.ZeroTolerance, RhinoMath.ZeroTolerance * 2.0, RhinoMath.SqrtEpsilon)));
    public static readonly Gen<double> UnitClosed = Gen.Frequency(
        (94, Gen.Double.Unit),
        (2, Gen.Const(value: 0.0)),
        (2, Gen.Const(value: 1.0)),
        (1, Gen.Const(value: RhinoMath.ZeroTolerance)),
        (1, Gen.Const(value: 1.0 - RhinoMath.ZeroTolerance)));
    public static readonly Gen<double> UnitInterior = Gen.Double[start: double.Epsilon, finish: 1.0 - double.Epsilon];
    public static readonly Gen<double> UnitAngle = Gen.Frequency(
        (90, Gen.Double[start: 0.0, finish: RhinoMath.TwoPI]),
        (10, Gen.OneOfConst(0.0, RhinoMath.Epsilon * 2.0, Math.PI, RhinoMath.TwoPI - RhinoMath.ZeroTolerance, RhinoMath.TwoPI)));
    public static readonly Gen<double> Probability = Gen.Double.Unit;
    public static readonly Gen<int> SmallDimension = Gen.Int[start: 1, finish: 8];
    public static readonly Gen<double> PositiveMagnitudeScalar = Gen.Frequency(
        (90, Positive),
        (10, Gen.OneOfConst(RhinoMath.ZeroTolerance * 2.0, RhinoMath.SqrtEpsilon, 1.0, 1.0e3)));
    public static readonly Gen<Vectors.Dimension> Dimension = SmallDimension.Select(static value =>
        Vectors.Dimension.TryCreate(value: value, obj: out Vectors.Dimension dimension) ? dimension : throw new InvalidOperationException(message: "generator invariant broken: Dimension"));
    public static readonly Gen<PositiveMagnitude> PositiveMagnitude = PositiveMagnitudeScalar.Select(static value =>
        Vectors.PositiveMagnitude.TryCreate(value: value, obj: out PositiveMagnitude magnitude) ? magnitude : throw new InvalidOperationException(message: "generator invariant broken: PositiveMagnitude"));
    public static readonly Gen<UnitInterval> UnitInterval = UnitClosed.Select(static value =>
        Vectors.UnitInterval.TryCreate(value: value, obj: out UnitInterval unit) ? unit : throw new InvalidOperationException(message: "generator invariant broken: UnitInterval"));
    public static readonly Gen<Context> Context = Tolerance.Select(Gen.Double[start: 0.0, finish: 1.0e-3], Tolerance, Gen.OneOfConst(UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches),
        static (double absolute, double relative, double angle, UnitSystem units) => Domain.Context.Of(absolute: absolute, relative: relative, angle: angle, units: units)
            .Match(Succ: static context => context, Fail: static error => throw new InvalidOperationException(message: $"generator invariant broken: Context {error.Message}")));
    public static Gen<Seq<double>> Simplex(int count) =>
        count switch {
            <= 0 => Gen.Const(value: Seq<double>()),
            _ => Positive.Array[count].Select(static values => {
                double total = values.Sum();
                return toSeq(values.Select(value => value / total));
            }),
        };

    // --- [GEOMETRY] ------------------------------------------------------------------------
    public static readonly Gen<Point3d> Point = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Point3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> Vec = Finite.Select(Finite, Finite, static (double x, double y, double z) => new Vector3d(x: x, y: y, z: z));
    public static readonly Gen<Vector3d> NonZeroVec = Vec.Where(predicate: static v => v.Length > 1.0e-6);
    public static readonly Gen<Vector3d> UnitVec = NonZeroVec.Select(static (Vector3d v) => v / v.Length);
    public static readonly Gen<(Vector3d A, Vector3d B)> VecPair = NonZeroVec.Select(NonZeroVec, static (Vector3d a, Vector3d b) => (A: a, B: b));
    public static readonly Gen<(Vector3d A, Vector3d B)> UnitVecPair = UnitVec.Select(UnitVec, static (Vector3d a, Vector3d b) => (A: a, B: b));
    public static readonly Gen<Plane> Plane = Point.Select(UnitVec, static (Point3d origin, Vector3d normal) => new Plane(origin: origin, normal: normal));
    public static readonly Gen<Line> Line = Point.Select(Point, static (Point3d a, Point3d b) => new Line(from: a, to: b));
    public static readonly Gen<BoundingBox> Bbox = Point.Select(Point, static (Point3d a, Point3d b) => new BoundingBox(
        min: new Point3d(x: Math.Min(val1: a.X, val2: b.X), y: Math.Min(val1: a.Y, val2: b.Y), z: Math.Min(val1: a.Z, val2: b.Z)),
        max: new Point3d(x: Math.Max(val1: a.X, val2: b.X), y: Math.Max(val1: a.Y, val2: b.Y), z: Math.Max(val1: a.Z, val2: b.Z))));
    public static readonly Gen<BoundingBox> NonEmptyBbox = Bbox.Where(predicate: static b => b.IsValid && b.Diagonal.Length > 1.0e-6);
    public static Func<double, double, bool> Approx(double relativeTolerance = 1.0e-9) =>
        (a, b) => Math.Abs(value: a - b) <= relativeTolerance * Math.Max(val1: 1.0, val2: Math.Abs(value: a) + Math.Abs(value: b));

    // --- [COLLECTIONS] ---------------------------------------------------------------------
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
    public static Gen<Seq<T>> SeqOf<T>(Gen<T> element, int max = 256) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Array[0, max].Select(static (T[] xs) => toSeq(xs));

    // --- [RAIL] ----------------------------------------------------------------------------
    public static readonly Op TestKey = Op.Of(name: "testkit");
    public static readonly Gen<Op> OpKey = Gen.String[1, 32].Select(static (string name) => Op.Of(name: name));
    public static readonly Gen<Error> Fault = Gen.OneOfConst<Error>(
        new Fault.MissingGeometry(),
        new Fault.Cancelled(),
        new Fault.MissingOperation());
    public static Gen<Fin<T>> FinOf<T>(Gen<T> succ, Gen<Error>? fail = null, int succWeight = 80) =>
        Gen.Frequency(
            (succWeight, (succ ?? throw new ArgumentNullException(nameof(succ))).Select(static (T v) => Fin.Succ(value: v))),
            (100 - succWeight, (fail ?? Fault).Select(static (Error e) => Fin.Fail<T>(error: e))));
    public static Gen<Option<T>> OptionOf<T>(Gen<T> some, int someWeight = 80) =>
        Gen.Frequency(
            (someWeight, (some ?? throw new ArgumentNullException(nameof(some))).Select(static (T v) => Some(value: v))),
            (100 - someWeight, Gen.Const(value: Option<T>.None)));
    public static Gen<Validation<Error, T>> ValidationOf<T>(Gen<T> succ, Gen<Error>? fail = null) =>
        Gen.OneOf(
            (succ ?? throw new ArgumentNullException(nameof(succ))).Select(static (T v) => Success<Error, T>(value: v)),
            (fail ?? Fault).Select(static (Error e) => Fail<Error, T>(value: e)));
}
