using System.Collections.Immutable;
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
    // Option-routed Where+Select preserves CsCheck shrinking AND eliminates the unreachable throw — TryCreate
    // executes once per candidate; failing values become Option<TVo>.None and are filtered out by IsSome.
    private delegate bool TryFn<TIn, TVo>(TIn value, out TVo result);
    private static Gen<TVo> ValueObject<TIn, TVo>(Gen<TIn> source, TryFn<TIn, TVo> tryCreate) =>
        source.Select(v => tryCreate(v, out TVo r) ? Some(r) : Option<TVo>.None)
              .Where(o => o.IsSome).Select(o => o.IfNone(default(TVo)!));
    public static readonly Gen<Dim> Dimension = ValueObject<int, Dim>(SmallDimension, Dim.TryCreate);
    public static readonly Gen<PositiveMagnitude> PositiveMagnitude = ValueObject<double, PositiveMagnitude>(PositiveMagnitudeScalar, Vectors.PositiveMagnitude.TryCreate);
    public static readonly Gen<UnitInterval> UnitInterval = ValueObject<double, UnitInterval>(UnitClosed, Vectors.UnitInterval.TryCreate);
    public static readonly Gen<Context> Context = Tolerance.Select(Gen.Double[start: 0.0, finish: 1.0e-3], Tolerance, Gen.OneOfConst(UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches),
        static (double absolute, double relative, double angle, UnitSystem units) => Domain.Context.Of(absolute: absolute, relative: relative, angle: angle, units: units).ToFin().ToOption())
        .Where(static o => o.IsSome).Select(static o => o.IfNone(default(Context)!));
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
    public static readonly Gen<BoundingBox> NonEmptyBbox = Point.Select(
        Gen.Double[start: 1.0e-3, finish: 1.0e3],
        Gen.Double[start: 1.0e-3, finish: 1.0e3],
        Gen.Double[start: 1.0e-3, finish: 1.0e3],
        static (Point3d origin, double dx, double dy, double dz) => new BoundingBox(
            min: origin,
            max: new Point3d(x: origin.X + dx, y: origin.Y + dy, z: origin.Z + dz)));
    public static readonly Seq<Point3d> UnitSegment3 = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0));
    public static readonly Seq<Point3d> UnitTriangle3 = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0));
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

    // --- [DOMAIN_SMARTENUMS] ---------------------------------------------------------------
    // Plural field names avoid collision with the SmartEnum type (also `SdfKind`). Default mm Context lives on
    // VectorsContextFixture (constructor-injected); generators that need a Context use Gens.Context (Gen<Context>).
    // Consumers: Field SDF primitive Theory + Intent.Descriptor SDF-by-kind dispatch.
    public static readonly Gen<SdfKind> SdfKinds = Gen.OneOfConst([.. SdfKind.Items]);
    // Consumers: Field Rbf/Mls kernel Theory + Mesh.Project<KernelProfile> kernel projection table.
    public static readonly Gen<KernelKind> KernelKinds = Gen.OneOfConst([.. KernelKind.Items]);
    // Consumers: Flow integrator Theory (9 cases) + Intent.Streamline integrator dispatch.
    public static readonly Gen<IntegratorKind> IntegratorKinds = Gen.OneOfConst([.. IntegratorKind.Items]);
    // Consumers: Sample explicit/Poisson/Lloyd/Welsh case sweep + Intent.Sample case dispatch + Extraction sample integration.
    public static readonly Gen<SampleAlgorithmKind> SampleAlgorithmKinds = Gen.OneOfConst([.. SampleAlgorithmKind.Items]);

    // --- [FIELD_FIXTURES] ------------------------------------------------------------------
    // Consumers: Field SDF primitive admission + Intent.Descriptor primitive case factory.
    public static readonly Gen<(SdfKind Kind, ImmutableDictionary<string, double> Parameters, Plane Pose)> PrimitiveFixture =
        SdfKinds.Select(Plane, static (SdfKind kind, Plane pose) =>
            (Kind: kind, Parameters: kind.RequiredKeys.Fold(ImmutableDictionary<string, double>.Empty, static (acc, key) => acc.Add(key, 1.0)), Pose: pose));
    // Consumers: Field iso-surface bounding-box admission + Sample volume sampling within box.
    public static readonly Gen<BoundingBox> AdmissibleBoundingBox = NonEmptyBbox;
    // Consumers: Field iso-surface resolution + Sample adaptive grid resolution.
    public static readonly Gen<(int Nx, int Ny, int Nz)> GridResolution = Gen.Int[2, 32].Select(Gen.Int[2, 32], Gen.Int[2, 32],
        static (int x, int y, int z) => (Nx: x, Ny: y, Nz: z));
    // Consumers: Field MLS smoothing+radius pair + Sample density+oversample pair.
    public static readonly Gen<(double A, double B)> PositivePair = Positive.Select(Positive, static (double a, double b) => (A: a, B: b));

    // --- [FLOW_FIXTURES] -------------------------------------------------------------------
    // Consumers: Flow streamline state init + Field flow integration step seeding.
    public static readonly Gen<(Point3d Position, double Time, double Step)> StreamlineState =
        Point.Select(Gen.Double[0.0, 100.0], Positive, static (Point3d p, double t, double h) => (Position: p, Time: t, Step: h));

    // --- [GEOMETRY_FIXTURES] ---------------------------------------------------------------
    // Consumers: Extraction surface-iso + Mesh planar fixture (4-corner quadrilateral).
    public static readonly Seq<Point3d> UnitSquare3 = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0), new Point3d(x: 0.0, y: 1.0, z: 0.0));
    // Pure-data mesh fixture (vertex coords + triangle indices) — avoids native Mesh handle ownership in generators.
    // Consumers: Mesh topology Euler V-E+F=2 + Extraction triangle-mesh iso-surface.
    public sealed record MeshFixture(Seq<Point3d> Vertices, Seq<(int A, int B, int C)> Triangles);
    public static readonly MeshFixture UnitTetrahedronFixture = new(
        Vertices: Seq(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0), new Point3d(0, 0, 1)),
        Triangles: Seq((0, 1, 2), (0, 1, 3), (1, 2, 3), (0, 2, 3)));
    public static readonly MeshFixture UnitCubeFixture = new(
        Vertices: Seq(
            new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(1, 1, 0), new Point3d(0, 1, 0),
            new Point3d(0, 0, 1), new Point3d(1, 0, 1), new Point3d(1, 1, 1), new Point3d(0, 1, 1)),
        Triangles: Seq(
            (0, 1, 2), (0, 2, 3), (4, 6, 5), (4, 7, 6),
            (0, 4, 5), (0, 5, 1), (1, 5, 6), (1, 6, 2),
            (2, 6, 7), (2, 7, 3), (3, 7, 4), (3, 4, 0)));
    public static readonly Gen<MeshFixture> MeshFixtureKinds = Gen.OneOfConst(UnitTetrahedronFixture, UnitCubeFixture);
    public static Gen<MeshFixture> PlanarTriangleMesh(int rows = 4, int cols = 4) =>
        Gen.Const(value: new MeshFixture(
            Vertices: toSeq(Enumerable.Range(start: 0, count: rows * cols).Select(idx => new Point3d(x: idx % cols, y: idx / cols, z: 0.0))),
            Triangles: toSeq(Enumerable.Range(start: 0, count: (rows - 1) * (cols - 1)).SelectMany(idx => {
                int r = idx / (cols - 1), c = idx % (cols - 1), v0 = (r * cols) + c, v1 = v0 + 1, v2 = v0 + cols, v3 = v2 + 1;
                return new[] { (v0, v1, v3), (v0, v3, v2) };
            }))));

    // --- [POINT_CLUSTERS] -----------------------------------------------------------------
    // Consumers: Cloud ring-based centroid + isoperimetric compactness invariants.
    public static Gen<Seq<Point3d>> RingPoints(int count, double radius) =>
        Gen.Const(value: toSeq(Enumerable.Range(start: 0, count: count).Select(i => {
            double theta = 2.0 * Math.PI * i / count;
            return new Point3d(x: radius * Math.Cos(d: theta), y: radius * Math.Sin(a: theta), z: 0.0);
        })));
    // Consumers: Cloud cluster fixtures + Sample uniform-cluster generation.
    public static Gen<Seq<Point3d>> ClusterPoints(int count, double spread) =>
        Point.SelectMany(center => UnitVec.Array[count].SelectMany(dirs => Gen.Double[0.0, spread].Array[count].Select(rads =>
            toSeq(dirs.Zip(rads, (d, r) => center + (d * r))))));
    // Consumers: Cloud Mesh validity rejection + Mesh AcceptResults non-finite guard.
    public static readonly Gen<Point3d> NonFiniteCoordinatePoint = Gen.OneOfConst(
        new Point3d(x: double.NaN, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: double.PositiveInfinity, z: 0.0),
        new Point3d(x: 0.0, y: 0.0, z: double.NegativeInfinity));
    // Consumers: Align ICP fixture builder + Cloud Bridson seed + Sample Lloyd init + Spectral basis row count.
    public static Gen<Seq<Point3d>> PointCluster3(int min = 3, int max = 32, double spread = 1.0) =>
        Point.Array[min, max].SelectMany(centers => Vec.Array[centers.Length].Select(jitters =>
            toSeq(centers.Zip(jitters, (c, j) => c + (j * spread)))));
    // Consumers: Align Procrustes mass + Cloud Sinkhorn mass + Sample Lloyd weight.
    public static Gen<Seq<double>> UniformMass(int count) =>
        Gen.Const(value: toSeq(Enumerable.Repeat(element: 1.0 / count, count: count)));
    // Consumers: Intent.Sample (Domain × Kind tuple) + Cloud.Of(domain) + Align ICP seed cluster.
    public static Gen<(Seq<Point3d> Points, Seq<double> Mass)> Cluster(int min = 3, int max = 32) =>
        PointCluster3(min: min, max: max).SelectMany(p => UniformMass(p.Count).Select(m => (Points: p, Mass: m)));
    // Consumers: Sample CloudDomain local + Extraction CloudDomain dispatch.
    public static readonly Gen<Seq<Point3d>> CloudDomain = NonEmptySeq(Point);

    // --- [SPECTRAL_FIXTURES] --------------------------------------------------------------
    // Consumers: Spectral basis eigenpair tests + future Field/Cloud spectral consumers (path graph Laplacian).
    public static Gen<(int N, Arr<double> Eigenvalues, VectorMatrix Eigenvectors)> SpectralBasis(int n) =>
        Gen.Const(value: (
            N: n,
            Eigenvalues: new Arr<double>([.. toSeq(Enumerable.Range(start: 0, count: n)).Map(k => 2.0 - (2.0 * Math.Cos(d: k * Math.PI / Math.Max(val1: n - 1, val2: 1))))]),
            Eigenvectors: VectorMatrix.Identity(dim: Dim.TryCreate(value: n, obj: out Dim d) ? d : throw new InvalidOperationException("SpectralBasis dimension"))));

    // --- [SPACE_FIXTURES] -----------------------------------------------------------------
    // Consumers: Space.Sample (hitPoint sample) + Intent.Support direction-from-hit projection.
    public static Gen<ClosestHit> ClosestHit(Gen<Point3d>? point = null) =>
        (point ?? Point).Select(Point, static (Point3d target, Point3d hit) =>
            new ClosestHit(Point: hit, Distance: Some(value: target.DistanceTo(other: hit)), Parameter: Option<double>.None,
                Uv: Option<Point2d>.None, Normal: Option<Vector3d>.None, Component: Option<ComponentIndex>.None,
                MeshPoint: Option<MeshPoint>.None, Tangent: Option<Vector3d>.None, Frame: Option<Plane>.None));
    // Consumers: Space.spec local SupportProjection sweep + Intent.spec output-projection matrix.
    public static readonly Gen<SupportProjection> SupportProjections = Gen.OneOfConst(
        SupportProjection.Closest, SupportProjection.Direction, SupportProjection.Normal,
        SupportProjection.Distance, SupportProjection.Parameter, SupportProjection.Uv,
        SupportProjection.Component, SupportProjection.MeshPoint, SupportProjection.SignedDistance,
        SupportProjection.ContainmentDistance, SupportProjection.Tangent, SupportProjection.Frame);

    // --- [MATRIX_FIXTURES] -----------------------------------------------------------------
    // Shared matrix-from-flat-row-major builder. Inputs caller-validated (rows/cols ≥ 1); throws on invariant break.
    private static VectorMatrix BuildMatrix(int rows, int cols, double[] entries) =>
        VectorMatrix.Of(
            rows: Dim.TryCreate(rows, out Dim r) ? r : throw new InvalidOperationException($"BuildMatrix rows: {rows}"),
            cols: Dim.TryCreate(cols, out Dim c) ? c : throw new InvalidOperationException($"BuildMatrix cols: {cols}"),
            entries: new Arr<double>(entries))
            .Match(Succ: static m => m, Fail: static e => throw new InvalidOperationException($"BuildMatrix invariant: {e.Message}"));
    // Consumers: Align Procrustes 2D rotation oracle + Atoms FrameOf 2D pose check.
    public static Gen<(double Theta, VectorMatrix R)> RotationMatrix2D(Gen<double>? angle = null) =>
        (angle ?? UnitAngle).Select(static theta => (
            Theta: theta,
            R: BuildMatrix(rows: 2, cols: 2, entries: [Math.Cos(d: theta), -Math.Sin(a: theta), Math.Sin(a: theta), Math.Cos(d: theta)])));
    // Consumers: Align reflection oracle + Atoms.MirrorCase reflection plane.
    public static Gen<(Vector3d Normal, VectorMatrix M)> Reflection3D(Gen<Vector3d>? normal = null) =>
        (normal ?? UnitVec).Select(static n => (Normal: n, M: BuildMatrix(rows: 3, cols: 3, entries: [
            1 - (2 * n.X * n.X), -2 * n.X * n.Y, -2 * n.X * n.Z,
            -2 * n.X * n.Y, 1 - (2 * n.Y * n.Y), -2 * n.Y * n.Z,
            -2 * n.X * n.Z, -2 * n.Y * n.Z, 1 - (2 * n.Z * n.Z)])));
    // Consumers: Matrix near-multiplicity eigenvalue stress + Spectral assembly conditioning test.
    public static Gen<VectorMatrix> SpdWithSpectrum(int n, Gen<Arr<double>> eigenvalues) =>
        eigenvalues.Select(spec => BuildMatrix(rows: n, cols: n,
            entries: [.. toSeq(Enumerable.Range(0, n * n)).Map(idx => idx / n == idx % n ? spec[idx / n] : 0.0)]));
    // Consumers: Matrix conditioning-aware tolerance + Spectral filter conditioning fuzz.
    public static Gen<VectorMatrix> IllConditioned(int n, double kappa) =>
        SpdWithSpectrum(n: n, eigenvalues: Gen.Const(value: new Arr<double>([.. toSeq(Enumerable.Range(0, n)).Map(k => 1.0 + ((kappa - 1.0) * k / Math.Max(val1: n - 1, val2: 1)))])));

    // --- [POLYMORPHIC_TUPLES] --------------------------------------------------------------
    // Consumers: Spec.Distributive (T,T,T) requirement + Spec.MetamorphicOps multi-param chain.
    public static Gen<(T A, T B, T C)> DistinctTriple<T>(Gen<T> element) =>
        (element ?? throw new ArgumentNullException(nameof(element))).Select(element, element, static (T a, T b, T c) => (A: a, B: b, C: c));

    // --- [DIRECTION_AND_FRAME] -------------------------------------------------------------
    // Consumers: Align ICP normal alignment + Field angular direction sampling.
    public static readonly Gen<Vector3d> AngularDirection = UnitAngle.Select(UnitClosed,
        static (double azimuth, double polar) => {
            double phi = polar * Math.PI;
            return new Vector3d(x: Math.Sin(a: phi) * Math.Cos(d: azimuth), y: Math.Sin(a: phi) * Math.Sin(a: azimuth), z: Math.Cos(d: phi));
        });
    // Consumers: Spectral basis Plane reconstruction + Atoms VectorFrame round-trip.
    public static readonly Gen<Plane> OrthonormalFrame = Point.Select(UnitVecPair,
        static (Point3d origin, (Vector3d A, Vector3d B) axes) => new Plane(origin: origin, xDirection: axes.A, yDirection: axes.B));

    // --- [CLOUD_FIXTURES] -----------------------------------------------------------------
    // Consumers: Cloud cluster classification + Sample density clustering.
    public static Gen<Seq<Point3d>> PointCloudClustered(int clusters = 3, int perCluster = 10, double spread = 0.1) =>
        Point.Array[clusters].Select(Vec.Array[clusters * perCluster], (Point3d[] centers, Vector3d[] jitters) =>
            toSeq(Enumerable.Range(start: 0, count: centers.Length * perCluster).Select(idx =>
                centers[idx / perCluster] + (jitters[idx] * spread))));

    // --- [METAMORPHIC_HELPERS] ------------------------------------------------------------
    // Consumers: Cloud centroid translation MR + Field SDF sample translation MR.
    public static Gen<Vector3d> Translate(double maxMagnitude = 100.0) =>
        Vec.Where(v => v.Length <= maxMagnitude);
    // Consumers: Cloud isoperimetric scaling MR + Field SDF scale MR.
    public static readonly Gen<double> Scale = Gen.Frequency(
        (80, Positive),
        (20, Gen.OneOfConst(0.5, 1.0, 2.0, 10.0)));
}
