# [M1]-[KERNEL_FOUNDATIONS] — LANDED-TRUTH INVENTORY

Lane: `libs/csharp/Rasm/.planning/{Domain,Numerics,Spatial,Analysis}/**` + `libs/csharp/Rasm/{README,ARCHITECTURE}.md`. All 27 pages read in full; NO page showed mid-edit corruption (all fences closed, DENSITY_BAR tables present) — the concurrent redteam left every mined page stable. Anchors: `file#[NN]-[SECTION]` / `file:Symbol`. All paths relative to `libs/csharp/Rasm/.planning/` unless rooted. The kernel compiles as ONE assembly (`Rasm.csproj`); namespace mirrors folder (`Rasm.Domain`, `Rasm.Numerics`, `Rasm.Spatial`, `Rasm.Analysis`); internal members cross the nine namespaces with no build edge (`ARCHITECTURE.md#[03]`).

---

## [01]-[DOMAIN] — the substrate floor every sibling composes

### ROP spine (`Domain/rails.md`)

| Owner | Exact surface | Anchor |
|---|---|---|
| `Op` `[ValueObject<string>]` ordinal eq+cmp | `public static Op Of([CallerMemberName] string name = "")`; fault factories `MissingContext()`/`InvalidInput()`/`InvalidResult(string? detail=null)`/`Unsupported(Type geometryType, Type outputType)`/`Caution(string concern)`; acceptance bridges `Fin<T> AcceptInput<T>(T)`/`AcceptValue<T>(T)`/`Fin<string> AcceptText`/`Fin<Unit> Confirm(bool)`/`Fin<T> Need<T>(T?)`/`Need<T>(Option<T>)`; scalar guards `Fin<double> Finite(double)`/`Positive(double)` | `rails.md:Op` §[02] |
| Exception rail | `public Fin<T> Catch<T>(Func<Fin<T>> body)` — `Try.lift`, `OperationCanceledException`→`Fault.Cancelled`, else `InvalidResult(detail)`; `public static Unit Side(Action)` / `SideWhen(bool, Action)` | `rails.md:Op.Catch`, `rails.md:Op.Side` |
| Generator marker | `[GenerateUnionOps]` metadata name `Rasm.Domain.GenerateUnionOpsAttribute` (frozen, `ForAttributeWithMetadataName`); emits per sealed-record case `internal static readonly global::Rasm.Domain.Op SelfOp = global::Rasm.Domain.Op.Of(name: nameof(<Case>));`. Opt-in only, no `[SkipUnionOps]` | `rails.md:GenerateUnionOpsAttribute` §[03] |
| `Expected` bridge | `public abstract record Expected : Error { IsExpected=>true; IsExceptional=>false; virtual string Category=>"Fault"; }` | `rails.md:Expected` §[04] |
| `Fault` `[Union]` 12 cases (UNMARKED — no `[GenerateUnionOps]`) | `MissingOperation` · `MissingContext(Op Key)` · `InvalidInput(Op Key)` · `InvalidResult(Op Key, Option<string> Detail=default)` · `Cancelled` · `Unsupported(Op Key, Type GeometryType, Type OutputType)` with `internal const int UnsupportedCode = 9104;` (ONLY coded case — the Grasshopper drain discriminant) · `ComputationFailed(string Label)` · `MissingGeometry` · `InvalidGeometry(Type Shape, string Check, string Log)` · `OutOfRange(string Label, double Scalar, string Requirement, Option<Op> Key=default)` · `InvalidUnitSystem(UnitSystem Units, string Requirement)` · `Caution(Op Key, string Concern)` | `rails.md:Fault` §[04] |
| `Lease<T>` `[Union]` | `Owned(T Value)` / `Borrowed(T Value)` where `T:class,IDisposable`; `TResult Use<TResult>(Func<T,TResult>)` · `Use<TState,TResult>` · `T Resource` · `Unit Dispose()` | `rails.md:Lease` §[05] |
| Validity floor | `public interface IValidityEvidence { public bool IsValid { get; } }`; `ValidityClaim` `[BoundaryAdapter]` readonly record struct rows `Of(bool)` · `Finite(double)`=`RhinoMath.IsValidDouble` (screens `UnsetValue`) · `Finite(Point3d/Vector3d)` · `Finite(ReadOnlySpan<double>)`=`TensorPrimitives.IsFiniteAll` · `Nonnegative`/`Positive`/`UnitInterval`/`Ordered`/`CountAtLeast`/`CountExactly`/`Evidence(IValidityEvidence?)`/`All(params ReadOnlySpan<ValidityClaim>)` + `implicit operator bool` | `rails.md:IValidityEvidence`, `rails.md:ValidityClaim` §[06] |

THREADING_LAW (`rails.md#[07]`): `Op` is an explicit VALUE (mint once via `Op.Of()`/`SelfOp`; `Op key` tail on internal kernels, `Op? key=null` + `OrDefault()` on public entries); `Eff<Env>` is the runtime carriage — `Env` carries `Context` + `IProgress<double>?` + `CancellationToken` ONLY, never `Op`; below the `Eff` floor synchronous rails thread `Context`+`CancellationToken` explicitly. One op = one paradigm. `Env` is `Analysis/query.md`'s record.

Two-family error seam (`ARCHITECTURE.md#[03]`, `rails.md` + `Numerics/faults.md` each state it): `Rasm.Domain.Fault` (kernel substrate, `Expected`-derived) vs band-2400 `GeometryFault` (robust core, ordinal-coded, `ToError()`-lowered) — two families by explicit decision, NEVER merged; both already `Error`, a composing page converts nothing.

### Identity — the federation content key (`Domain/identity.md`)

The single most load-bearing contract in the lane. EXACT, single member, single overload:

```csharp
public static class ContentHash {
    [BoundaryAdapter] public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(canonicalBytes);
}
```

- Seed = `HashToUInt128` default ZERO — never overridden, never parameterized (`identity.md#[02]`). Package `System.IO.Hashing`. `UInt128` is the identity currency; hex/`ulong`-lane/byte-order projections are boundary-owned elsewhere.
- Declared growth (NOT yet a member): streaming `XxHash128.Append`+`GetCurrentHashAsUInt128` seed-zero lands as ONE additional member HERE, never a sibling hasher.
- `Deterministic` splitmix64 owner (`identity.md:Deterministic` §[03]): `Gamma = 0x9E3779B97F4A7C15UL`; public family of 6 — `NextUnit(ref ulong)` · `NextSignedUnit(ref ulong)` · `NextSignedComplexUnit(ref ulong)` · `OrderKey(Point3d, int seed=0)` · `OrderKey(ReadOnlySpan<double>, int seed=0)` · `UnitInterval(Point3d, int salt, int seed=0)`; `Mix`/`Advance`/`Bits` (`-0.0`→`+0.0` normalize) private. LAW: derivation ≠ identity — minting a `ContentHash` from `OrderKey`, or seeding a sampler from a content hash, is REJECTED.

### Context / tolerance (`Domain/context.md`)

- Triad `[ValueObject<double>](KeyMemberName="Value", KeyMemberAccessModifier=Public)`: `AbsoluteTolerance` (finite, `> RhinoMath.ZeroTolerance`) · `RelativeTolerance` (finite, `[0,1)`) · `AngleTolerance` (finite, `(RhinoMath.Epsilon, RhinoMath.TwoPI]`) — generated `Create`/`TryCreate`/`Validate` (`context.md#[02]`).
- `Context` sealed record, private ctor (`context.md:Context` §[03]): `Validation<Error,Context> Of(double absolute, double relative, double angle, UnitSystem units)` · `Millimeters()` · `Of(UnitSystem)` (CustomUnits/Unset/None → fail) · `[BoundaryAdapter] Of(RhinoDoc? doc)` — THE ONE doc seam (absent doc → `Fault.MissingContext`). Slots `Absolute`/`Relative`/`Angle`/`Units`; derivations `Fractional => Relative.Value>0.0 ? Relative.Value : 1.0e-8` and `MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient` — derived ONCE here.
- Only 2 corpus members name `RhinoDoc`: `Context.Of(RhinoDoc?)` + the `Analyze.From/In` forwarder (`Analysis/query.md`).

### Acceptance / validation (`Domain/validation.md`)

- `Requirement` sealed record, `Seq<Check>` monoid, `operator+`=concat-distinct; 9 rows `None`·`Basic`·`CurveLength`·`AreaMass`·`MeshCheck`·`SolidTopology`·`VolumeMass`·`SurfaceEvaluation`·`Strict`; entry `public Validation<Error,T> Apply<T>(Context? context, T? value, CancellationToken cancel=default)`; dispatch `public static Requirement ForKind(Kind kind)` (`validation.md:Requirement` §[02]). Private nested `Check` `[SmartEnum<string>]` 13 rows with `[UseDelegateFromConstructor]` `Applies`/`Run` columns → `Fault.InvalidGeometry(Shape,Check,Log)`.
- `OpAcceptance` INTERNAL static (frozen docID name): `ValidityOf(object?) : Option<bool>` = THE one validity authority — foreign arms + ONE `IValidityEvidence` arm + 18-type `ValueValidity` `FrozenDictionary<Type,Func<object,bool>>` compiled-Expression table (`validation.md:OpAcceptance` §[03]). Registration law: any kernel receipt reaches the oracle by implementing `IValidityEvidence` with a `ValidityClaim.All` fold — ZERO oracle edits. Registered: `ClosestHit`, `TopologyProjection`, `Stat`/`Distribution`/`SampleMoment`; named future: `IntersectionHit`/`CurveDeviation` (relations), `ResidualSample` (measure), `NeighborHit`/`NeighborPair` (neighbors). The `AnalysisAcceptance.ValidityOf` fork is DEAD.
- Factory bridge (`validation.md#[04]`): `internal Validation<Error,TVO> TryCreateValidated<TVO>()` on `extension<TRaw>(TRaw)` — invocation is combined-type-arg `absolute.TryCreateValidated<double,AbsoluteTolerance>()`; member-only form does NOT compile. Public: `OpExtensions.OrDefault([CallerMemberName])` on `extension(Op? key)`; `Fin<TVO> AcceptValidated<TVO>(double|int candidate)` on `extension(Op op)` (re-keys `OutOfRange.Key`).
- `Admit` INTERNAL static, 36 members (`validation.md:Admit` §[06]): bool floor `Finite(Point3d/Vector3d)`/`FiniteSpan`/`FiniteComplexSpan`/`HermitianDiagonalRealSpan`/`Frame(Plane)`; `Fin` gates `NotNull`×2 · `CountAtLeast` · `SameCount` · `Finite`×3 · `NonnegativeFinite` · `Ordered` · `AllFinite`(span/Seq/params) · `AllValid` · `FinitePositive` · `FinitePoints` · `FiniteScalars` · `WeightedPoints` · `PositiveFiniteWeights` · `AllFiniteComplex` · `HermitianDiagonalReal` · `WithDivisor` · `KernelInput` · `FalloffInput` · `NoiseInput`(octaves 1–32) · `NonnegativeExtent` · `Plane` · `PlaneSequence` · `Directional` · `Cone` · `Period`.

### Normalization taxonomy (`Domain/normalization.md`)

- `Topology` `[SmartEnum<int>]` 10 rows: `Unknown/Point/Curve/Surface/Brep/Mesh/SubD/PointCloud/Hatch/Extrusion` (`normalization.md:Topology`).
- `Kind` `[SmartEnum<int>]` 21 rows (`Point,Line,Polyline,Circle,Arc,Ellipse,Curve,Surface,Plane,Sphere,Cylinder,Cone,Torus,Brep,Box,BoundingBox,Mesh,SubD,PointCloud,Extrusion,Hatch`) with `Type`/`Topology` columns, 8 `FrozenSet` tables, `public static Option<Kind> Of(Type)` (base-walk), `internal static FrozenDictionary<Rhino.DocObjects.ObjectType,Kind> ByObjectType`; predicates `CanBound`·`CanOrientedBound`·`CanReadVertices`·`CanReadControlPoints`·`CanReadEdges`·`CanCoerceTo(Type)` (`normalization.md:Kind`).
- `Capability` keyless `[SmartEnum]` INTERNAL 13 rows: `CurveForm·SurfaceForm·BrepForm·Bound·OrientedBound·DecomposeFaces·EvaluateTopology·Closest·ClosestNormal·ClosestTangent·ClosestFrame·SignedDistance·ReadVertices`; non-row arities `Coercible(Type,Type)` / `Native(...)` / `Universal(Type)` (`normalization.md:Capability`).
- `CurveForm` `[Union]` 6 cases: `LineCase(Line)`·`CircleCase(Circle)`·`ArcCase(Arc)`·`EllipseCase(Ellipse)`·`PolylineCase(Polyline,bool IsClosed)`·`NurbsCase(int Degree,bool IsClosed,bool IsPlanar,bool IsPeriodic,int SpanCount,int Dimension)` (`normalization.md:CurveForm`).
- `TopologyProjection` sealed record `IValidityEvidence,IDisposable` — the ONE disposable projection carrier: `Lease<GeometryBase>` + `ComponentIndex Source` + `bool Reversed` + memo `Option<Lease<Brep>>`; factories `Of(Curve,ComponentIndex)`/`Of(BrepFace)`/`Of(Lease<GeometryBase>,ComponentIndex,bool)`/`Fin<TopologyProjection> FromMesh(Mesh?,ComponentIndex)`; protocol `As<T>()`/`As<T>(Op)`/`DetachFrom(GeometryBase)`/`Transfers(...)`/`Dispose()` (`normalization.md:TopologyProjection` §[03]).
- `Normalization` INTERNAL `[BoundaryAdapter]`: `extension(object? geometry)` → `Fin<Kind> KindOf(Context)` · `Fin<BoundingBox> BoundsOf(Op)` · `Fin<TTarget> CoerceTo<TTarget>(Context,Op)`; recoveries `Fin<Lease<Curve>> CurveForm(object?,Op)` / `SurfaceForm` / `BrepForm` / `Fin<CurveForm> CurveFormOf(Curve,Context)` / `Option<object> PrimitiveOf(Kind,object,Context,Op)`.
- Frozen contract names (rename = corpus break, bound by generator + 10+ settled pages): `Topology`·`Kind`·`CurveForm`·`TopologyProjection`. `GeometryRequest` lives in `Analysis/query` — NOT re-minted here.

### Evaluation lattice (`Domain/evaluation.md`)

- `ClosestHit` public readonly record struct `IValidityEvidence`, 9 fields: `Point3d Point, Option<double> Distance, Option<double> Parameter, Option<Point2d> Uv, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint, Option<Vector3d> Tangent, Option<Plane> Frame`; factory `internal static ClosestHit At(Point3d target, Point3d point, ...)` — `Distance` computed AT factory, never caller-supplied; `internal Fin<TOut> Project<TOut>(Op key)` via `AtomProjection.Rows<ClosestHit,TOut>` with 8 `ProjectionRow`s (self·`Point3d`·`double`=Distance·`Point2d`=Uv·`Vector3d`=Normal·`Plane`=Frame·`ComponentIndex`·`MeshPoint`); `internal Fin<double> SignedDistanceFrom(Point3d, Op)` (`evaluation.md:ClosestHit` §[02]).
- `Evaluation` INTERNAL `[BoundaryAdapter]` `extension(object? geometry)`: `Fin<ClosestHit> ClosestOf(Point3d target, Op key)` (18 arms) · `SignedDistanceOf(ClosestHit, Point3d, Op)` (6 arms) · `Fin<Seq<Point3d>> SamplePoints(int, Context, Op)` · `VerticesOf(Op)` (15 arms); members `CurveSampleParameters`/`SurfaceUv`/`SurfaceSampleUv`/`SurfaceSamplePoints`/`NormalAt` (orientation flip)/`FrameAt` (re-hand) (`evaluation.md:Evaluation`).

### Stats (`Domain/stats.md`)

- `ScalarMetric` `[SmartEnum<int>]` 3: `Magnitude(0)`·`Gaussian(1)`·`Mean(2)`, `internal Fin<double> Of(Vector3d,Op)` + `Of(SurfaceCurvature,Op)`. `ExtremumDirection` `[SmartEnum<int>]` `Maximum(+1)`·`Minimum(-1)` — key IS the fold sign. `StatContext` `[Union]` `NoneCase`·`MetricCase(ScalarMetric)`·`ToleranceCase(double, bool WithinTolerance)` (`stats.md:ScalarMetric`, `:ExtremumDirection`, `:StatContext`).
- `Stat` readonly record struct `IValidityEvidence` `(int Count, double Minimum, double Maximum, double Mean, double Variance, StatContext Context)` + `Rms`/`WithinTolerance`; `public static Fin<Stat> Of(Seq<double> values, Op key, Option<StatContext> context=default)` — Welford single-pass, fused `AllFinite`; `internal static Seq<TItem> Extrema<TItem>(Seq<TItem>, Func<TItem,double>, double tolerance, ExtremumDirection)` — the ONE extremum fold (`stats.md:Stat`).
- `Distribution` readonly record struct `(Stat Summary, double Median, double Iqr, Seq<(double Percentile,double Value)> Percentiles)`; `internal static Fin<Distribution> Of(Seq<double>, Seq<double> percentiles, Op, Option<StatContext>)` (`stats.md:Distribution`).
- `SampleMoment` INTERNAL readonly record struct `(int Dimension, Arr<double> Mean, Arr<double> UpperCovariance)` — packed symmetric `this[int,int]` offset `i·d − i(i−1)/2 + (j−i)`; `internal static Fin<SampleMoment> Of(Seq<Arr<double>> rows, int dimension, Op key, Option<Arr<double>> weights=default)` — the ONE covariance/moment owner (`stats.md:SampleMoment`). Consumers: `Spatial/cloud` `CloudKernel` PCA, `Analysis/select` principal frames, `Solving/fit` (carries no covariance re-mint).

---

## [02]-[NUMERICS] — exact-predicate floor + host-neutral-shaped algebra

All 7 pages stable. Host-neutral floor law: coordinate carrier = RhinoCommon value structs; finiteness = `double.IsFinite`; epsilons = `EpsilonPolicy`; full turn = `Math.Tau`; NO `RhinoMath` on the floor.

### The predicate/`Implicit` exact-arithmetic spine (`Numerics/predicates.md`)

Direct `Predicate` static family — exact `Sign` verdict, NO rail (`predicates.md:Predicate`):

```csharp
public static Sign Orient2D(Point3d a, Point3d b, Point3d c)
public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d)
public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d)
public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e)
```

Implicit constructed-point family (interval-filtered, `Expansion`-exact, `Fraction`-adjudicated):

```csharp
public static Sign Orient2D(in Implicit a, in Implicit b, in Implicit c, Axis axis)
public static Sign Compare(in Implicit a, in Implicit b, Axis axis)   // Negative = a<b on axis coord — THE exact order key
public static Sign InCircle(Point3d a, Point3d b, Point3d c, in Implicit d, Axis axis)
public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, in Implicit e)
```

- Each direct member walks the inline ladder `ErrorBound.<Kind>.Of(det, permanent) ?? Refine<Kind>(…) ?? <Kind>Exact(…)`; `PrecisionTier` `[SmartEnum<int>]` 5 tiers `Double→DoubleDouble→Interval→Expansion→Rational` (`predicates.md:PrecisionTier`). Multi-implicit in-circum + 3D multi-implicit are recorded CDTet-gated GROWTH arms on existing members — never new siblings.
- `Sign` `[SmartEnum<int>]` (`Negative -1`/`Zero 0`/`Positive 1`) with `Of(double)`/`Of(int)`/`Flip`/`Times(Sign)`; `Axis` `[SmartEnum<int>]` X/Y/Z with `U`/`V` columns + `Coord(Point3d,int)` (`predicates.md:Sign`, `:Axis`).
- `Implicit` `[Union<Point3d, Ssi, Lpi, Tpi>]` — `public (T X,T Y,T Z,T Lambda) Homogeneous<T>() where T : struct, IExact<T>`; `public Point3d Round()` is the ONE emission-seam materialization. Carriers: `Ssi(Point3d P,Q,R,S, Axis Plane)` · `Lpi(Point3d P,Q,A,B,C)` · `Tpi(9× Point3d)` — defining points, never pre-rounded normals (`predicates.md:Implicit`, `:Ssi`/`:Lpi`/`:Tpi`).
- `IExact<TSelf>` static-abstract algebra (`Of`/`Diff`/`Prod` + `Add/Sub/Mul/Scale`, `Sign? Verdict`); `Expansion : IExact<Expansion>` (`TwoSum`/`TwoProduct` FMA-vs-Dekker gated on `NumericsPolicy.HardwareFma`, `SignOf`, `ToFraction`); `Interval : IExact<Interval>` (directed-rounding 53-bit `EFloat` under frozen Down/Up contexts); `ErrorBound(double Coefficient, double RefineCoefficient)` per-kind filter/refine rows; `RationalOracle` — `Fraction` exact adjudicator + independent `EFloat` `Sign? BinaryOf(Expansion)` cross-check; `NumericsPolicy` (`Epsilon`=2⁻⁵³, `DoubleDoubleEpsilon`=2⁻¹⁰⁷, `Splitter`, `HardwareFma`, 8 bound consts) (`predicates.md:IExact`, `:Expansion`, `:Interval`, `:ErrorBound`, `:RationalOracle`, `:NumericsPolicy`).

### Fault band (`Numerics/faults.md`)

- `GeometryFault` `[Union]` root-homed, 25 cases, band 2400–2449; `public int Code` (total `Switch`), `public string Message` (grammar `geometry:<case>:<field>=<value>`), `public FaultCluster Cluster => FaultCluster.OfCode(Code)`, `public Error ToError() => Error.New(Code, Message)`; routing form `GeometryFault.<Case>(...).ToError()` (`faults.md:GeometryFault`).
- All 25 (ctor · code): `DegenerateInput(Kind,int,string)` 2400 · `IndexMismatch(EntityKind,int,int)` 2401 · `KindMismatch(SpatialKind,QueryKind)` 2402 · `NameCollision(UInt128,int)` 2404 · `HashMismatch(UInt128,int)` 2405 · `UnrepairableMesh(HealStage,int,int)` 2408 · `OverConstrained(int,double)` 2412 · `SingularSystem(int,int)` 2413 · `DegenerateOffset(int,double)` 2416 · `SkeletonStalled(int,double)` 2417 · `CollapseStalled(int,double)` 2418 · `DegenerateArrangement(int,string)` 2420 · `ConstraintUnrecoverable(int,int)` 2421 · `DegenerateTessellation(int,string)` 2422 · `NativeAssetMissing(string,string,long)` 2423 · `IntersectionFault(PrimitiveKind,PrimitiveKind)` 2424 · `SectionFault(int,double,int)` 2425 · `FitFault(double,double)` 2428 · `ParameterizationFault(ChartId,double)` 2432 · `ProjectionFault(EdgeKind,int)` 2436 · `DecimationFault(int,int)` 2440 · `RemeshStalled(double,double,int)` 2441 · `EncodingFault(EncodingChannel,ChannelDtype,string)` 2444 · `ParametricFault(ParametricStage,string,string)` 2448 · `DevelopmentFault(DevelopmentStage,int,double)` 2449 (`faults.md#[02]-[FAULT_BAND]`).
- `FaultCluster` `[SmartEnum<int>]` 13 rows, `public static FaultCluster OfCode(int code) => Items[(code - 2400) >> 2];` (`faults.md:FaultCluster`). `ParametricStage` (5) / `DevelopmentStage` (4) `[SmartEnum<string>]` mint HERE; all other payload discriminants (`Kind`/`EntityKind`/`SpatialKind`/`QueryKind`/`HealStage`/`PrimitiveKind`/`ChartId`/`EdgeKind`/`EncodingChannel`/`ChannelDtype`) are COMPOSED from sibling owners, never re-minted. Band sits strictly below AEC `MaterialFault` 2450.

### Atoms — the typed vector-algebra floor (`Numerics/atoms.md`)

- `EpsilonPolicy`: `public const double SqrtEpsilon = 1.4901161193847656e-8;` (2⁻²⁶) · `public const double ZeroTolerance = 2.3283064365386963e-10;` (2⁻³²) (`atoms.md:EpsilonPolicy`).
- Value objects (the Materials-facing vocabulary): `Dimension` `[ValueObject<int>]` (`value >= 1`) · `PositiveMagnitude` `[ValueObject<double>]` (finite `&& > ZeroTolerance`) · `UnitInterval` `[ValueObject<double>]` (finite `[0,1]`) · `VectorAngle` `[ValueObject<double>]` (`[0, Math.Tau]`) with `internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot, Op key)` + raw overload + `Project<TOut>` (`atoms.md:Dimension`…`:VectorAngle`).
- Enums: `BoundarySense` (`Toward(+1)`/`Away(-1)`, `Sign` col) · `SignedAxis` (6, `Vector3d World`, `Of(Option<Plane>)`, `Cardinal(bool planar)`) · `VectorRelation` (4, `public static Fin<VectorRelation> Of(Vector3d a, Vector3d b, Context context, Op? key = null)` — `Analysis/relations` `TangencyAt` binds `Parallel`/`AntiParallel`) · `AnglePivot` `[Union]` (`WorldCase`·`FrameCase(Plane)`·`NormalCase(Direction)`) (`atoms.md:BoundarySense`…`:AnglePivot`).
- Vector algebra: `Direction` — `public static Fin<Direction> Of(Vector3d value, Context context, Op? key = null)`; `Reflect(Direction normal)` · `Refract(Direction, Direction, double etaIncident, double etaTransmitted, Op)` · `ParallelTransport(Seq<Plane> frames, Op?)` (application-only; generator lives in neighbors); unit gate `Math.Abs(Value.Length - 1.0) <= EpsilonPolicy.SqrtEpsilon`. `VectorSpan.Of(Point3d anchor, Vector3d vector, Context, Op?)` + `Components(Plane, Op)`. `VectorFrame.Of(Point3d origin, Vector3d normal, Option<Vector3d> xHint, Context, Op?)` + `public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool closed, Context context, Op? key = null)` → composes `NeighborKernel.BishopChain`. `VectorCone.Of(apex, axis, halfAngleRadians, Context, Op?)` + `SolidAngle`/`Contains`/`Enclose`/`PartitionBy` (`atoms.md:Direction`…`:VectorCone`).
- THE projection rail — the ONE type-directed dispatch site every `.Project<TOut>` routes through: `internal readonly record struct ProjectionRow(Type Output, Func<Fin<object>> Make)` + `internal static class AtomProjection` 7 members (`Rows<TSelf,TOut>(TSelf self, Op key, Type? owner, params ReadOnlySpan<ProjectionRow> rows)` · `Self`/`Value`/`SelfOrValue`/`Values`/`Custom` · `Raw<TOut>(object raw, Option<Context>, Op, Type owner, bool admitsVectorMagnitude=false)` with 11 lattice arms). Stays INTERNAL (`atoms.md:ProjectionRow`, `:AtomProjection`).

### Matrix — the linear-algebra substrate (`Numerics/matrix.md`)

- `Matrix(Dimension Rows, Dimension Cols, Arr<double> Entries)`: `static Fin<Matrix> Of(...)` · `Identity`/`Transpose`/`Multiply`/`Inverse`/`PseudoInverse` · `Fin<LuResult> DecomposeLu` · `Fin<QrResult> DecomposeQr` · `Fin<SvdResult> DecomposeSvd` · `Fin<EigenSolveReceipt<Complex,Arr<Complex>>> DecomposeEigenDetailed` · `Norm(MatrixNormKind)`/`Trace`/`Determinant`/`Spectral`/`Rank` · `Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op?)` · `LeastSquaresDetailed` (`matrix.md:Matrix`).
- `SymmetricMatrix(Dimension Dimension, Arr<double> Upper)`: `Of`/`ToDense`/`DecomposeEigenDetailed`→`EigenSolveReceipt<double,Arr<double>>`/`DecomposeEigen`/`DecomposeCholesky` (`matrix.md:SymmetricMatrix`).
- `SparseMatrix(Dimension Rows, Cols, Arr<int> RowPtr, ColInd, Arr<double> Values)` CSR: `FromTriplets` · `Multiply`/`ToDense` · `SolveDetailed` (BiCgStab+fallback) · `SingularSolveDetailed(Arr<double> rhs, GaugePolicy gauge, Context context, Op?)` · `SolveIndefiniteDetailed(rhs, double pivotTolerance=1.0, Op?)` · `SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations=200, Op?)` (LOBPCG) · **`GeneralizedEigenpairsDetailed(SparseMatrix mass, int k, Op?)` — the `Meshing/dec` spectral-basis entry** (`matrix.md:SparseMatrix`).
- `SparseHermitian(Dimension Order, …, Arr<Complex> Values)` upper-only, real-diagonal gate; Hermitian LOBPCG → `EigenSolveReceipt<double,Arr<Complex>>` (`matrix.md:SparseHermitian`).
- `CholeskySparse` sealed record: `static Fin<CholeskySparse> Of(SparseMatrix symmetric, Op?)`; `Lock`-guarded `SolveDetailed` — the Lock is LOAD-BEARING (shared scratch); `Meshing/mesh` `LaplacianCache` caches these factor objects (`matrix.md:CholeskySparse`).
- Vocabulary: `EigenSolvePath`(5)/`EigenSolveStop`(3)/`SolvePath`(7)/`SolveStop`(7)/`MatrixNormKind`(4)/`GaugeSolverKind`(3)/`GaugeShift`(4); `GaugePolicy` `[Union]` (`Pin`/`MeanZeroDeflation`/`LagrangeKKT`) with presets `PinConstant`/`Pinned`/`MeanZeroConstant`/`KktConstant` (`matrix.md:GaugePolicy`).
- Receipt shapes Compute's solver plane consumes (`matrix.md#[05]-[RECEIPTS]`):
  - `SolveReceipt(Arr<double> Solution, SolvePath Path, SolveStop Stop, Dimension Rows, Cols, int RhsLength, Option<int> Iterations, MaxIterations, Option<double> Tolerance, double Residual, Option<bool> FullRank, Option<int> InputNonZeros, FactorNonZeros, Option<GaugeReceipt> Gauge = default) : IValidityEvidence`
  - `EigenSolveReceipt<TEigen,TVector>(Seq<(TEigen,TVector)> Pairs, EigenSolvePath Path, EigenSolveStop Stop, int RequestedPairs, ReturnedPairs, Option<int> Iterations, MaxIterations, Option<double> Tolerance, double MaxResidual, Option<int> FactorNonZeros = default) : IValidityEvidence`
  - `GaugeReceipt(GaugeSolverKind Solver, int NullspaceDim, … bool NumericalBreakdown) : IValidityEvidence` — 19 fields.
- `MatrixKernel` = the ONE MathNet+CSparse access path (managed provider only, no native); residual witness folds in 106-bit `ddouble` (`CompensatedNorm`); LOBPCG basis seeding BORROWS `Domain/identity` `Deterministic` (seeds 17 real / 19 Hermitian) — Numerics mints NO identity, NO hash (`matrix.md:MatrixKernel`).

### Spectral — DEC carriers + descriptor algebra (`Numerics/spectral.md`)

- **The frozen Compute adjoint contract**: `DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Star1, Star2, SpectralAssemblyReceipt Receipt, Option<SignpostTransportReceipt> Transport = default, Option<HarmonicOneFormBasis> Harmonic = default)` — `IsValid` cross-couples star counts to operator shapes; `internal Fin<TOut> Project<TOut>(Op key)` via `ProjectionRow`s. Constructed by `Meshing/dec`; consumed by `Rasm.Compute` / `Processing/geodesics` / `Processing/segment` / `Spatial/fields` (`spectral.md:DiscreteCalculus`).
- `SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors)`: `SpectralRadius`; **`ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(SpectralRadius, EpsilonPolicy.ZeroTolerance)` — THE one scale-relative zero band**; `Truncate(int k)` (`spectral.md:SpectralBasis`).
- `SpectralFilter` `[Union]` 6 (`Heat(PositiveMagnitude)`/`Wave(PositiveMagnitude,PositiveMagnitude)`/`Biharmonic`/`Diffusion(PositiveMagnitude)`/`CommuteTime`/`Identity`): `internal double Weight(double eigenvalue)`; partial monoid `Option<SpectralFilter> Compose(SpectralFilter)`; `internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralDescriptorPolicy policy, Op key)` (`spectral.md:SpectralFilter`).
- Receipts: `SpectralAssemblyReceipt` / `HarmonicOneFormReceipt(… EigenSolveReceipt<double,Arr<double>> Eigen …)` / `HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicOneFormReceipt Receipt)` — all `IValidityEvidence` scale-relative gates. Descriptor algebra: `SpectralDescriptor(Arr<double> Values, SpectralDescriptorReceipt Receipt)` with `Normalize(policy)`/`Rank(candidates, policy)`; 5 policy SmartEnums (`SpectralAssemblyKind`/`SpectralScaleNormalization`/`SpectralEnergyNormalization`/`SpectralZeroModePolicy`/`SpectralDistanceKind`); `SpectralKernel` dense-buffer statement kernel, `WaveBandwidthFloor=1e-9` (`spectral.md#[04]-[DESCRIPTOR_ALGEBRA]`).

### Integrate — carrier-generic ODE/RK floor, zero geometry (`Numerics/integrate.md`)

- `IntegratorKind` `[SmartEnum<int>]` 9 tableaux (`Euler/Heun/Midpoint/Ralston/RK4/RK38/BogackiShampine/CashKarp/DormandPrince`) with `ButcherTableau Tableau` column; `ButcherTableau(Seq<Seq<double>> Coupling, Seq<double> Abscissae, Weights, Option<Seq<double>> EmbeddedWeights, int MethodOrder, Option<int> EmbeddedOrder)` — order-condition moment validation in `ddouble` (`Σbᵢcᵢ=1/2` …), abscissae derived as coupling row sums; `DenseWeightsAt(double theta, Op)`; `DenseOutputCoefficientFamily` (3) — generic route solves via `Matrix.SolveDetailed`/`LeastSquaresDetailed`, leaving a `SolveReceipt` inside `DenseOutputReceipt` (`integrate.md:IntegratorKind`, `:ButcherTableau`, `:ButcherDenseOutput`).
- Stepper: `IntegrationModule<TState,TDelta>(Add, Scale, Sum, Norm, Zero)` — THE one `Combine(Seq<double>, Seq<TDelta>)` fold, `Scalar` instance; `FieldIntegrator` `[Union]` (`FixedCase`/`AdaptiveCase`) — `Fixed(IntegratorKind, Op?)` / `Adaptive(IntegratorKind, double tolerance, int maxRejects=3, StepControl?, Op?)`; generic `Fin<IntegrationStep<TState,TDelta>> Step<TState,TDelta>(IntegrationModule, Func<TState,Fin<TDelta>> sample, TState state, double h, Op key)`; `DenseOutputSpan<TState,TDelta>.PointAt(double theta, Op)`. The geometry module (`<Point3d,Vector3d>`) is SUPPLIED by `Processing/flow`, which owns the reject-loop budget (`integrate.md:IntegrationModule`, `:FieldIntegrator`).

### Calculus — sampler-generic analytic floor (`Numerics/calculus.md`)

- `Nabla` static, 8 members over ONE `SampleAxes` stencil: `GradientAt`/`CurlAt`/`CurlNoiseAt`/`DivergenceAt`/`LaplacianAt`/`StrainMagnitudeAt(sampler, Point3d point, double eps, Op key)` + `ToroidalWrap(Point3d, Vector3d period)`; samplers `Func<Point3d, Fin<double>>` / `Func<Point3d, Fin<Vector3d>>` (`calculus.md:Nabla`).
- `KernelKind` `[SmartEnum<int>]` 6 (`Wendland/Quintic/Cosine/Cubic/Linear/Epanechnikov`) with `DerivativeSupremum` col; `Fin<KernelProfile> Profile(double distance, double radius, Op key)` → `KernelProfile(double Value, FirstDerivative, SecondDerivative, KernelProfileStatus Status)` — derivatives read OFF the profile, never re-diffed. `WeightKernelFamily` (5, `Interpolating` col) — the MLS/APSS/Levin window family `Meshing/reconstruct` composes (`calculus.md:KernelKind`, `:WeightKernelFamily`).
- `Falloff` `[Union]` 6 (`Constant/Inverse/InverseSquare/Gaussian(PositiveMagnitude)/Kernel(KernelKind,PositiveMagnitude)/Metric(KernelKind, Func<Point3d,Fin<SymmetricMatrix>>, PositiveMagnitude)`) with `abstract Option<double> SlopeBound` — `Spatial/fields` absorbs `SlopeBound` into `LipschitzBound`; SPD gate `SpdByMinors(SymmetricMatrix)` (`calculus.md:Falloff`).
- `FieldNoise` internal static: `PerlinAt`/`WorleyAt`/`SkewedSimplexAt(Point3d point, int seed, double frequency[, bool smooth])`, canonical `PermTable`; `Spatial/fields` owns `NoiseKind`/octave policy (`calculus.md:FieldNoise`).

---

## [03]-[SPATIAL] — proximity, identity grammar, acceleration, fields

### The four ONE-Apply entries

| Entry (verbatim) | Op union → cases | Answer | Anchor |
|---|---|---|---|
| `public static Fin<SpatialAnswer> Spatial.Apply(SpatialOp op, Op? key = null)` | `SpatialOp` = `Build(SpatialKind Kind, BoundingBox[] Primitives, BuildPolicy Policy)` · `Refit(SpatialIndex Index, BoundingBox[] Updated)` · `Query(SpatialIndex Index, SpatialQuery Probe)` · `Wire(SpatialIndex Index)` | `SpatialAnswer` = `Index(SpatialIndex)` · `Result(QueryResult)` · `Wire(float[] Bounds, long[] Nodes)` | `Spatial/index.md:Spatial.Apply`, `:SpatialOp` |
| `public static Fin<ReconcileAnswer> Reconciliation.Apply(ReconcileOp op, Op? key = null)` | `ReconcileOp` = `Encode(EncodeForm Form)` · `Reconcile(NameTable Prior, CanonicalTopology Rebuilt)` · `BuildEntities(MeshSpace Space)` | `ReconcileAnswer` = `Digest(GeometryHash)` · `Reconciled(NamingHash)` · `Topology(CanonicalTopology)` | `Spatial/reconciliation.md:Reconciliation.Apply`, `:ReconcileOp` |
| `public static Fin<NameTable> Naming.Apply(NamingOp op, Op? key = null)` | `NamingOp` = `Track(NameTable Prior, CanonicalTopology Rebuilt, Option<NamingPolicy> Policy = default)` · `Resolve(CanonicalTopology Boundary)` | `NameTable` (registry IS receipt) | `Spatial/naming.md:Naming.Apply`, `:NamingOp` |
| `public static Fin<TOut> CloudTransport.Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null)` | single op; `TOut` selects projection row (cluster×cluster only) | `SinkhornPlan.Project<TOut>` → `double`/`SinkhornReceipt`/`CloudCorrespondenceSet`/`Matrix`/`VectorCloud` | `Spatial/transport.md:CloudTransport.Sinkhorn` |

### Query algebra — the batched winding/field shape

`Spatial/index.md:SpatialQuery` `[Union]`, `QueryKind Kind` discriminant (`[SmartEnum<string>]` rows `range·ray·nearest·overlap·winding`):
- `Range(BoundingBox Box, Option<Sphere> Ball)` → `QueryResult.Hits(Seq<int> Ids)`
- `Ray(Ray3d Probe, double MaxT)` → `QueryResult.RayHit(Option<int> Id, double T)`
- `Nearest(Point3d Query, int K)` → `QueryResult.Nearest(Seq<int> Ordered)`
- `Overlap(SpatialIndex Other, double Tolerance)` → `QueryResult.Pairs(Seq<(int Left, int Right)> Overlaps)`
- **`Winding(Point3d[] Queries, Point3d[] Triangles, double BetaSquared)` → `QueryResult.Field(double[] Values)`** — batched GWN; a single point is a 1-length `Queries` array. ONE bottom-up `Moments` pass caches node moments; every query point reads O(1) (`Spatial/index.md:Winding`, `:WindingAt`, `:Moments`, `#GENERALIZED_WINDING`).

### Index owners

- `SpatialIndex` `[Union] : IValidityEvidence`: `Bvh(NodeStore Store, BoundingBox[] Primitives, int LeafSize, double BuildCost, BuildPolicy Policy, SpatialKind Builder)` · `LinearOctree(NodeStore Store, BoundingBox[] Primitives, Point3d[] Centroids, BoundingBox Root, BuildPolicy Policy)` (`Spatial/index.md:SpatialIndex`). Build kernels `BuildBvh` (SAH) / `BuildOctree` (Morton, `MortonDepth = 10`) / `BuildAgglomerative` (PLOC + `Compact` BFS); `SpatialKind` `[SmartEnum<string>]` `Bvh·Octree·Agglomerative` with `[UseDelegateFromConstructor] Build(...)` column.
- `NodeStore` frozen SoA record `(int Count, float[] BoundsMin, float[] BoundsMax, int[] FirstChild, int[] ChildCount, int[] LeafStart, int[] LeafCount, int[] Order)` (`Spatial/index.md:NodeStore`). `BuildPolicy(int LeafSize, int MaxDepth, int SahBuckets, double RefitDegradationLimit, int ParallelFloor)`, `.Canonical = (4, 32, 12, 1.6, 4096)`.
- Persistent refit `internal Fin<SpatialIndex> Refit(BoundingBox[] revised)` — fresh bound arrays over SHARED topology arrays, degradation-keyed (`Spatial/index.md:SpatialIndex.Refit`, `:Rebound`).
- Outward rounding at the ONE arena write seam: `static float Down(double v) => float.BitDecrement((float)v)` / `Up => float.BitIncrement` (`Spatial/index.md:Down`/`Up`).

### Neighbor substrate (point tier — DIFFERENT altitude from index.md by standing decision)

- `NeighborIndex` `[Union]`, `public static Fin<NeighborIndex> Of(NeighborSource source, Op? key = null)`; cases `CloudCase(VectorCloud.ClusterCase)` · `PointsCase(Point3d[] Hay, RTree Tree)` · `MeshFacesCase(Mesh, RTree)` · `BoundsCase(RTree, int Count)` · `StaticCase(KDTree<double,double,int> Tree, Point3d[] Points)` (`Spatial/neighbors.md:NeighborIndex`).
- `NeighborQuery` `[Union]`: `NearestCase(int K)` · `RadiusCase(PositiveMagnitude R, Option<Dimension> Cap)` · `BoxCase(BoundingBox)` · `BallCase(Sphere)` · `OverlapsCase(NeighborIndex Other, double Tolerance)` · `PairsCase(Seq<Point3d> Needles, NeighborQuery Probe)`. `NeighborAnswer` `[Union]`: `Hits(Seq<NeighborHit>)` · `PairsFound(Seq<NeighborPair>)` · `Graph(NeighborhoodGraph)` (`Spatial/neighbors.md:NeighborQuery`, `:NeighborAnswer`).
- Batch spine `NeighborKernel.GraphOf(...) → Fin<NeighborhoodGraph>`; carrier `NeighborhoodGraph(int[][] Ids, NeighborhoodReceipt Receipt)`. Folds: `PcaOf` · `EstimateNormals` · `OrientNormals` (Hoppe-DeRose via QuikGraph `MinimumSpanningTreePrim`) · `PrincipalCurvatures` · `Curvedness` · `ShapeIndex` (`Spatial/neighbors.md#NEIGHBORHOOD_FOLDS`).
- RMF single owner: `internal static Fin<Seq<Plane>> NeighborKernel.BishopChain(VectorCloud cloud, Op key)` (+ point-form overload) — the ONE double-reflection generator; `Direction.ParallelTransport` is application-only (`Spatial/neighbors.md#BISHOP_CHAIN`).

### Fields, cloud, support, naming, transport vocabularies

- `ScalarField` `[Union]` ~35 cases, `internal Fin<double> SampleScalar(Point3d, Context, Op)`; PUBLIC tagged rails `Fin<FieldSample> SampleDetailed(...)` / `Fin<SdfSample> SampleSdfDetailed(...)` — `Drawing/pack.md` binds `SampleDetailed` by name. `SignedDistanceFromMeshCase(MeshSpace, SdfMeshPolicy)` delegates `MeshSdf.SignedDistanceDetailed` → composes `Spatial.Apply`/`SpatialQuery.Winding` (`Spatial/fields.md:ScalarField`, `:ScalarField.SampleDetailed`).
- `SdfKind` `[Union]` 12 exact analytic primitives with `abstract double Lipschitz` column; `VectorField` `[Union]` ~25; `TensorField` `[Union]` 6 → `Fin<SymmetricMatrix>`; vocab `BlendKind`(8, `ErosionFactor` case column)/`CsgKind`(3)/`NoiseKind`(4)/`RayPolicy`/`BouncePolicy`/`SdfStatus` (`Spatial/fields.md:SdfKind`, `#VECTOR_FIELD`, `#TENSOR_FIELD`, `#FIELD_VOCAB`).
- `VectorCloud` `[Union]` `RingCase`·`PolylineCase`·`ClusterCase` (mass = `Option<Arr<double>>` column, never a 4th case); `ClusterCase.Indexed` lazy `PointCloud` via `ConditionalWeakTable`, probes `ClosestVertex`/`WithinRadius`. `VectorCloudMetric` `[SmartEnum<int>]` 30 rows `internal Fin<TOut> Project<TOut>(...)`; `CloudKernel` `CovarianceOf`/`PrincipalStatsOf`/`PlanarWindingOf` (name-distinct from 3D GWN); `CloudHullKind` `[SmartEnum<int>]` 5 over `MIConvexHull` `Triangulation.CreateDelaunay`; `VectorCloudShape` 17-Option-column omni-projection embedded by `Analysis/inspect.md` in `MeshFaceShape` (`Spatial/cloud.md:VectorCloud`, `:VectorCloudMetric`, `#HULL`, `:VectorCloudShape`).
- `SupportProjection` `[SmartEnum<int>]` 14 rows, `internal Fin<TOut> Project<TOut>(SupportSpace, ClosestHit, Point3d, Context, Op)`; `SupportSpace` `[BoundaryAdapter]` `public static Fin<SupportSpace> Of(object? value, Op? key = null)` + `Closest`/`SignedDistance`/`ContainmentDistance` (`Spatial/support.md:SupportProjection`, `:SupportSpace`).
- `EntityKind` `[SmartEnum<int>]` `Vertex`(0)·`Edge`(2)·`Face`(-1) with `SignatureArity` — the shared vertex/edge/face discriminant IFC/graph consumers key on and `Solving/solver` re-anchors against (`Spatial/naming.md:EntityKind`). `TopoName`/`TopoSignature` `[ValueObject<UInt128>]`; `NameTable` `IValidityEvidence`; `TrackOutcome`(3); `NamingPolicy` (`MigrationOverlap` fraction).
- Sinkhorn: log-domain flat `double[]` kernel, `LogUnderflowFloor = -708.3964185322641`; balanced/unbalanced/debiased as POLICY COLUMNS on `CloudTransportPolicy`, coupling exits as `Numerics/matrix` `Matrix`; receipts `SinkhornReceipt`/`CloudCorrespondenceSet`; declared growth `Barycenter`/`GromovWasserstein` (`Spatial/transport.md:CloudTransport.Solve`, `:LogUnderflowFloor`).

### THE IDENTITY GRAMMAR — every content-key mint path (Persistence keys on ALL of this)

ONE hash in the geometry domain: `ContentHash.Of` (seed-zero XxHash128, two-64-bit-half order). Every mint path routes through it — no second hasher exists anywhere:

| Mint path (verbatim) | Produces | Anchor |
|---|---|---|
| `static GeometryHash Digest(EncodeForm form)` = `GeometryHash.Create(ContentHash.Of(Stream(form).WrittenSpan))` | `GeometryHash` `[ValueObject<UInt128>]` — the CONTENT axis | `Spatial/reconciliation.md:Reconciliation.Digest`, `:GeometryHash` |
| `EncodeForm.Of(MeshSpace)` / `Of(CanonicalTopology)` / `Of(VectorCloud)` / `Of(Arr<Direction>, Arr<double>, Arr<Point3d>, Op?)` | admitted `EncodeForm` {Mesh · Cloud · Parametric}; digests travel as `(form, digest)` pairs — a digest NEVER crosses a form boundary | `Spatial/reconciliation.md:EncodeForm.Of` |
| `ReconcileAnswer.Digest` → `NamingHash(GeometryHash Whole, HashMap<TopoName,NameAddress> Addresses)` | whole-topology digest + per-name content addresses — Persistence structural merge consumes `NamingHash` PER NODE | `Spatial/reconciliation.md:NamingHash`, `:Reconciliation.Addresses` |
| `ContentHash.Of(entry.CanonicalBytes)` inside the `Addresses` traverse | per-entity `UInt128` matched against rebuilt `Set<UInt128>`; dangling refs accumulate `Validation` (`HashMismatch` 2405) | `Spatial/reconciliation.md:Reconciliation.Addresses` |
| `TopoName.Mint(EntityKind kind, ReadOnlySpan<byte> canonicalBytes, Generation born)` = `ContentHash.Of(kind‖born‖canonicalBytes)` | `TopoName` `[ValueObject<UInt128>]` — the REFERENCE axis; orthogonal to `GeometryHash`, cross-axis compare is a TYPE ERROR | `Spatial/naming.md:TopoName.Mint` |
| `TopoSignature.Of(EntityKind, ReadOnlySpan<TopoName>, ReadOnlySpan<int>)` = `ContentHash.Of(kind‖sortedNames‖histogram)` | `TopoSignature` `[ValueObject<UInt128>]` re-anchor bucket fingerprint | `Spatial/naming.md:TopoSignature.Of` |

FROZEN canonical byte layouts — sole owner `Spatial/reconciliation.md#RECONCILIATION_BRIDGE` + `:Reconciliation.MeshStream`/`CloudStream`/`ParametricStream`. All little-endian, contiguous, no padding; doubles raw IEEE-754-LE with `-0.0 → +0.0`; NaN inadmissible upstream:
- **Mesh** (= `CANONICAL_BYTE_IDENTITY` fixture): `int32 VertexCount` · `int32 EdgeCount` · `(int32 Min,int32 Max)` per lex-sorted edge · `int32 FaceCount` · per least-rotation cycle in lex face order `(int32 CycleLength, int32 Vertex…)`.
- **Cloud**: `int32 KindOrdinal` (cluster 0 · polyline 1 · ring 2) · `int32 VertexCount` · `(double X,Y,Z)` per vertex · `int32 MassCount` (0 or VertexCount) · `double Mass…`.
- **Parametric** (NORMALIZED form): `int32 DirectionCount` · per direction `(int32 Degree, int32 KnotCount, double Knot…)` · `int32 WeightCount` · `double Weight…` · `int32 ControlCount` · `(double X,Y,Z)` per control point. Weight-scale canonicalization is the PROJECTING OWNER's proof (`Domain/identity.md` law); curve/surface producers bind `(form, digest)` feeding Persistence `Element.ToCanonicalBytes` as a COMPONENT, never a sibling `SpineRef` key.

Golden fixture: single-triangle 52-byte stream `03 00 00 00 03 00 00 00 …`, digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70` (harness-confirmed-once) — `Spatial/reconciliation.md#CANONICAL_BYTE_IDENTITY`. Persistence reads the IDENTICAL mesh layout before its own `XxHash128` (cross-package byte-identity proof). Cross-package parity corpus: `ONE_WIRE_FIXTURE_CORPUS` — 7 fixtures, one seed; [1] CANONICAL_BYTE_IDENTITY (REAL), [2] CLASH_GOLDEN (REAL), [3]–[7] DESIGN-PIN owned by Compute/Persistence/AppHost/Element (`Spatial/reconciliation.md#ONE_WIRE_FIXTURE_CORPUS`).

`CanonicalTopology.OfMesh(MeshSpace)` is the ONE native adjacency admission; a brep face-edge-vertex factory is declared GROWTH under the same canonical-order law — today a brep patch encodes via meshed patch (`Spatial/reconciliation.md:CanonicalTopology.OfMesh`). `RebuiltEntity` carries explicit `Self` index (the `VertexNames` re-key), TRUE incidence, `[vertex,edge,face]` histogram (`Spatial/reconciliation.md:CanonicalTopology.Entities`).

### The Compute wire seam (clash)

`SpatialAnswer.Wire(float[] Bounds, long[] Nodes)` from `internal static (float[] Bounds, long[] Nodes) NodeLinkProjection(NodeStore store)`. Layout: `Bounds` = `6·NodeCount` LE float32 `[minX,minY,minZ,maxX,maxY,maxZ]`; `Nodes` = `NodeCount + primitiveCount` LE int64, descriptor `(FirstChild<<21)|ChildCount` internal / `-(((LeafStart'<<21)|LeafCount))-1` leaf with tail-relative `LeafStart'`, `ChildShift = 21`. `Rasm.Compute` `ClashScale.NodeLinkPairs` decodes over contiguous `[FirstChild, FirstChild+ChildCount)`; a Compute-owned `AccelerationStructure` return was RETIRED (`Spatial/index.md:SpatialIndex.NodeLinkProjection`, `#CLASH_SEAM`, `#CLASH_GOLDEN`).

### Spatial fault band

`GeometryFault.DegenerateInput(Kind, int index, string witness)` 2400 (`Spatial/index.md:SpatialIndex.Admit`) · `IndexMismatch(EntityKind, int expected, int actual)` 2401 (`:Refit`) · `KindMismatch(SpatialKind, QueryKind)` 2402 (`:Query` winding arm) · `NameCollision(UInt128 name, int kind)` 2404 — accumulated `Validation<Error,NameTable>`, never fail-fast (`Spatial/naming.md:Naming.Anchor`) · `HashMismatch(UInt128 name, int kind)` 2405 (`Spatial/reconciliation.md:Reconciliation.Addresses`); band `naming` 2404–2407 shared reconciliation↔naming. Admission channel is the Domain two-family seam: `key.InvalidInput()`/`InvalidResult()`/`Unsupported(...)`/`MissingContext()`; `Fault.Cancelled()` on neighbor capsule cancel.

---

## [04]-[ANALYSIS] — the measured-query public entry

### Entry contract (`Analysis/query.md`)

- `AnalysisQuery` `[Union]` public abstract partial record — 4 bands, 25 cases, private ctor, NO `[GenerateUnionOps]`; arity dispatch 3 `internal virtual`: `Single<TGeometry,TOut>(Op key)` / `Pair<TA,TB,TOut>(Op key)` / `Service<TOut>(Op key)`, each defaulting `key.Unsupported<…>()` (`Analysis/query.md:AnalysisQuery`, `#[02]-[REQUEST_ALGEBRA]`).
  - GEOMETRY (7, Single): `CoerceCase(Type Output)` · `CurveFormCase` · `VerticesCase` · `SamplePointsCase(int Count)` · `SurfaceUvCase(Point2d Uv)` · `ClosestCase(Point3d Target)` · `SignedDistanceCase(Point3d Sample, ClosestHit Hit)` — absorbs the geometry-request band; no second `GeometryRequest` ADT.
  - FAMILY (8, Single, each forwards the owning union's `internal Operation<TGeometry,TOut> Operation<TGeometry,TOut>()`): `BoundsCase(Bounds)` · `MeasureCase(Measure)` · `LocationCase(Location)` · `CurvesCase(Curves)` · `FacesCase(Faces)` · `TopologyCase(Topologies)` · `MeshesCase(Meshes)` · `PointsCase(Points)`.
  - RELATION (6): `IntersectionsCase`/`ClassificationCase`/`CurveDeviationCase`/`ConformanceCase(ConformanceMetric,int,Seq<double>)` (Pair) + `SelfIntersectionCase`/`RayCase(RayQuery)` (Single).
  - SPATIAL (4, Service over Unit): `SearchBoxCase(NeighborIndex,BoundingBox)` · `SearchSphereCase(NeighborIndex,Sphere)` · `OverlapCase(NeighborIndex,NeighborIndex,double)` · `PointPairsCase(Seq<Point3d>,Seq<Point3d>,NeighborQuery)` — composes `Spatial/neighbors` directly, no query-side RTree wrapper.
- Frozen factory spellings (renames = breaking edits vs dormant host consumers): `Kind` · `Coerce(Type)` · `CurveForm` · `SurfaceForm` · `BrepForm` · `Vertices` · `SamplePoints(int)` · `SurfaceUv(Point2d)` · `Closest(Point3d)` · `SignedDistance(Point3d,ClosestHit)` · `Bounds(Bounds?=null)` · `Measure(Measure)` · `Location(Location)` · `Selection(Curves|Faces|Topologies)` · `MeshPointSpatial(Meshes|Points)` · `Intersections` · `Classification` · `CurveDeviation` · `SelfIntersection` · `Ray(RayQuery)` · `Conformance(ConformanceMetric,int,params double[])` · `Search(…,BoundingBox|Sphere)` · `Overlaps(…,double=0.0)` · `PointPairs(ReadOnlySpan<Point3d>,ReadOnlySpan<Point3d>,NeighborQuery)`.
- `Operation<TGeometry,TOut>` public sealed partial record — private `Body [Union]` (`Rejected`/`PerItem`/`Aggregate`/`Service`); ctors `Build`/`Aggregate`/`Reject`/`Service`; `Apply(Seq<TGeometry>) → Eff<Env,Seq<TOut>>`; `Prepare` gate. Build-time rejection is DATA (`Body.Rejected`) until `Apply`.
- `[BoundaryAdapter] public sealed record Env(Context Context, IProgress<double>? Progress, CancellationToken Cancellation)` — positional record shape HOST-FROZEN; GH binding constructs it directly (`Analysis/query.md:Env`).
- `Analyze` facade: `Scope` record (`Fin<Context> Context`, `With(IProgress<double>)`, `With(CancellationToken)`, `Run<TGeometry,TOut>(Operation?, params ReadOnlySpan<TGeometry>)`); `From(RhinoDoc?)` — THE one doc-coupled adapter beside `Context.Of(RhinoDoc)`; `In(UnitSystem)` / `In(double,double,double,UnitSystem)` / `In(Context)`; `Query<TGeometry,TOut>` / `Query<TA,TB,TOut>` / `Query<TOut>`; 4×`Run` → ALL return `Validation<Error,Seq<TOut>>` — no dedicated receipt rail, `Validation<Error,Seq<TOut>>` IS the public result (`Analysis/query.md:Analyze`, `#[03]-[OPERATION_RUNTIME]`).

### Measure (`Analysis/measure.md`)

- `Measure` `[Union]` 3 cases `LengthCase`·`SpatialMidpointCase`·`MassPropertyCase(MassKind, MassProperty)`; 11 factories `Length`·`SpatialMidpoint`·`Area`·`Volume`·`MassError(MassKind)`·`Centroid`·`CentroidError`·`Radii`·`PrincipalAxes`·`Inertia`·`InertiaProducts` — mass ops are the `(MassKind × MassProperty)` coordinate, never a verb family (`measure.md:Measure`).
- `MassKind` `[SmartEnum<int>]` 4 (`None/Length/Area/Volume`) binding `Requirement` + compute/aggregate delegates, `KindOf(GeometryBase)`, `PrincipalFrameOf`; `MassProperty` `[SmartEnum<int>]` 8 (`Magnitude·MagnitudeError·Centroid·CentroidError·Radii·PrincipalAxes·Inertia·InertiaProducts`) (`measure.md:MassKind`, `:MassProperty`).
- `Bounds` `[Union]` 15 cases: `AxisAlignedCase`·`InPlaneCase(Plane)`·`TransformedCase(Transform)`·`PrincipalFrameCase`·`CenterCase`·`CornersCase(bool Unique)`·`EdgesCase`·`AreaCase`·`VolumeCase`·`DiagonalCase`·`AspectRatioCase`·`TightnessCase`·`EnclosingSphereCase(int Count=64)`·`EnclosingCircleCase(Plane,int=64)`·`EnclosingCylinderCase(Vector3d Axis,int=64)`; `EnclosingSampleCount=64`; Ritter two-pass fold `measure.md:RitterFit` (`measure.md:Bounds`).
- `ConformanceMetric` `[SmartEnum<int>]` 8 rows with typed outputs: `Distance`/`Rms`→`double`, `WithinTolerance`→`bool`, `Summary`→`Stat`, `Maximum`/`SignedResidual`/`Containment`→`ResidualSample`, `Distribution`→`Distribution`; `ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance) : IValidityEvidence` (`measure.md:ConformanceMetric`, `:ResidualSample`). Conformance closest-point routes through `Spatial/support` `SupportProjection`, never a local switch.

### Inspect (`Analysis/inspect.md`)

- `Topologies` `[Union]` 6 cases (`KindCase`·`DomainsCase`·`SolidOrientationCase`·`ComponentsCase`·`ContainsPointCase(Point3d)`·`ScalarCase(TopologyScalar)`), 13 factories; `TopologyScalar` `[SmartEnum<int>]` 8 rows `Manifold`(bool)·`Euler`·`BoundaryLoops`·`Genus`·`HoleCount`·`FaceCount`·`EdgeCount`·`VertexCount` with `[UseDelegateFromConstructor] Extract(GeometryBase,Op)` (`inspect.md:Topologies`, `:TopologyScalar`).
- Genus is DERIVED, never stored: `g = (2C − χ − B) / 2` applicative `(EulerOf, BoundaryLoopsOf, ComponentCountOf).Apply(…)`, oriented-manifold-gated; `HoleCountOf = max(0, B − C)`; the type gate is `OnGeometry<TGeometry,TResult>(…, onMesh, onBrep)` (`inspect.md:GenusOf`).
- `Meshes` `[Union]` 7 cases (`SamplesCase(MeshSampleGroup)`·`FaceQualityCase(MeshMetric)`·`FaceShapeCase`·`AtVisiblePolygonCase(Option<int>)`·`VisiblePolygonCountCase`·`NakedEdgesCase`·`OutlineCase(Plane)`); `MeshSampleGroup` 5 bands (`None/Validity/Count/Defect/Quality`); `MeshSampleKind` `[SmartEnum<int>]` 32 rows keys 0–44 incl. 13 defect counters — ONE `MeshCheck` capture threaded to all 13, never per-row re-runs; `MeshMetric` 6 rows over VISIBLE POLYGONS (ngon-aware `ComponentIndex`), ring metrics via `VectorCloud.Ring`+`VectorCloudMetric` (`inspect.md:Meshes`, `:MeshSampleKind`, `:MeshMetric`, `:MeshSamples`).
- Receipts: `MeshSample(MeshSampleKind Kind, int Value)` · `MeshMetricSample(ComponentIndex Source, double Value)` · `MeshFaceShape(ComponentIndex Source, VectorCloudShape Shape)` — all `IValidityEvidence` (`inspect.md#[03]-[MESH]`).

### Select (`Analysis/select.md`)

- `CurveFeature` `[SmartEnum<int>]` 14 provenance rows (`Input`(0)…`Draft`(13)); `EdgeDescriptor` internal `[Union]` (`OfBrep(EdgeAdjacency, Seq<BrepLoopType>)`·`OfMesh(int ConnectedFaces)`·`OfLoop(BrepLoopType)`) — data-driven `Features` derivation, no per-source if-ladders (`select.md:CurveFeature`, `:EdgeDescriptor`).
- `Curves` `[Union]` 6 cases / 14 factories; `Faces` `[Union]` 3 cases (`AllCase`·`RankedCase(Vector3d Axis, ExtremumDirection)`·`AtCase(int?)`) with 8 typed projections on ONE `FaceOperation` (`Brep`·`TopologyProjection`·`Plane`·`Point3d`·`Vector3d`·`ComponentIndex`·`int`·`Interval`); `Points` `[Union]` 5 cases / 6 factories (`Quadrants`·`Extrema(Seq<Vector3d>)`·`EdgeMidpoints` — composes the `Curves` rail, no second edge walker — ·`Vertices`·`ControlPoints`·`Spread(SpreadAspect)`); `SpreadAspect` 5 rows (`Frame`/`PrincipalFrame`→Plane · `Distribution`→Stat · `Collinear`/`Coplanar`→bool) (`select.md:Curves`, `:Faces`, `:Points`, `:SpreadAspect`).
- PCA spread: eigenpair selected via `Stat.Extrema` (never first-returned order coupling), `SymmetricMatrix.Of`/`DecomposeEigen` from `Numerics/matrix`; abort-on-failure, no zero-row substitution (`select.md:PrincipalAngle`).

### Relations (`Analysis/relations.md`)

- `IntersectionKind` (`Unknown/Point/Overlap/Curve`) + `IntersectionTangency` (`Unknown/Transversal/Tangent`) `[SmartEnum<int>]`; `RayQuery(Ray3d Ray, int MaxReflections=1) : IValidityEvidence` with `ReflectionCeiling=1000` — NAME FROZEN (cs-analyzer docID); `CurveDeviation` 8-field readonly record struct (`MinimumDistance,MinimumA,MinimumB,MaximumDistance,MaximumA,MaximumB,Tolerance,WithinTolerance`) : `IValidityEvidence` — exact `Curve.GetDistancesBetweenCurves`, never sampled (`relations.md:RayQuery`, `:CurveDeviation`).
- `IntersectionHit` `[BoundaryAdapter][Union] : IValidityEvidence` — NAME FROZEN (cs-analyzer docID) — 3 cases `PointCase(Point3d,IntersectionTangency)`·`CurveCase(Curve,IntersectionKind)`·`OverlapCase(Point3d,Point3d,Interval,Interval,Option<Curve>)`; frozen `Projectable` set {`IntersectionHit`,`Curve`,`Point3d`,`Interval`,`IntersectionKind`,`IntersectionTangency`}; batch gate `Project<TOut>(Seq<IntersectionHit>,Op)` with the disposal law (curve transfers ONLY under `Curve`; all else + failure `DropCurves`) (`relations.md:IntersectionHit`).
- 25-row intersection lattice as `Seq<IntersectionCase>` DATA rows, declaration order = scan order: value-primitive band 7 (`Line/Line`·`Line/Plane`·`Plane/Plane`·`Line/Circle`·`Line/Sphere`·`Line/BoundingBox`·`Line/Box`), curve band 7 (`Line/Curve`·`Curve/Curve`·`Curve/Plane`·`Curve/Line`·`Curve/BrepFace`·`Curve/Brep`(partial)·`Curve/Surface`), solid band 4 (`Surface/Surface`·`Brep/Plane`·`Brep/Surface`·`Brep/Brep`), mesh band 3 (`Mesh/Line`·`Mesh/Plane`·`Mesh/Mesh`), ray band 2, lowering band 2 (curve-form recovery re-enters the ordered scan ONCE) (`relations.md:IntersectionCases`).
- 5 relation builders over ONE `PairOp` spine: `RelationIntersection`/`RelationClassification` (out ∈ {`IntersectionHit`,`IntersectionTangency`})/`RelationDeviation` (out=`CurveDeviation`)/`RelationSelfIntersection`/`RelationRay(RayQuery,Op)`; `IntersectionOf<TL,TR>` is unordered — `Fault.Unsupported` (9104) is the ONLY flip-retry trigger; tangency is an enrichment fold (`ClassifiedIntersectionOf` → `VectorIntent.Relation`), never a second intersector (`relations.md#[04]-[RELATION_OPERATIONS]`, `:EnrichTangency`).
- Altitude coexistence: this Rhino-parametric lattice coexists with predicate-exact `Meshing/intersect`; `select.md` `Silhouette` capture coexists with `Drawing/view` — neither side calls or re-implements the other (`relations.md#[06]` [ALTITUDE_COEXISTENCE], `select.md#[06]` [HOST_CAPTURE_SEAMS]).

---

## [05]-[CONSUMER_COMPOSITION_CONTRACTS]

### Persistence — keys on the FULL identity grammar

- `ContentHash.Of(ReadOnlySpan<byte>) → UInt128` seed-zero XxHash128 — the ONE federation hasher; `ContentAddress` composes it at the codec, no second hasher (`Domain/identity.md#[02]`; `ARCHITECTURE.md#[02]` row 88).
- `NamingHash(GeometryHash Whole, HashMap<TopoName,NameAddress> Addresses)` per node for structural merge; adjacency-derived `GeometryHash` over the frozen mesh byte layout; geometry crosses the seam by content-hash ONLY, read never re-mints (`Spatial/reconciliation.md:NamingHash`; `ARCHITECTURE.md#[02]` row 90).
- Byte-identity proof: Persistence re-encodes the IDENTICAL frozen mesh layout before its own `XxHash128`; golden 52-byte fixture digest `0x9462A71A5DD13DCFA3B1D6D225FCBE70`; parity via `ONE_WIRE_FIXTURE_CORPUS` fixtures [3]–[7] (DESIGN-PIN, Persistence among owners).
- `(form, digest)` pair law: Parametric producers feed Persistence `Element.ToCanonicalBytes` as a COMPONENT; a digest never crosses an `EncodeForm` boundary.
- Federation reproduction: python `runtime/evidence/identity` and typescript `core/value/contentKey` reproduce the one seed-zero XxHash128 (`ARCHITECTURE.md#[02]` rows 91–92).

### Compute — tensor/solver planes

- `SpatialAnswer.Wire(float[] Bounds, long[] Nodes)` — `ClashScale.NodeLinkPairs` decodes the `ChildShift=21` descriptor pack; fixture [2] CLASH_GOLDEN (`Spatial/index.md#CLASH_SEAM`; `ARCHITECTURE.md#[02]` row 97).
- `ModelIdentity.Checksum` composes `ContentHash.Of` — never a per-call-site XxHash128 (`ARCHITECTURE.md#[02]` row 89).
- `Numerics/Spectral` `DiscreteCalculus` DEC operator bundle — the frozen adjoint-carrier SHAPE Compute consumes: fields `(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0/Star1/Star2, SpectralAssemblyReceipt, Option<SignpostTransportReceipt>, Option<HarmonicOneFormBasis>)` + `ProjectionRow`-routed `Project<TOut>` (`spectral.md:DiscreteCalculus`; `ARCHITECTURE.md#[02]` row 93). Solver receipts Compute keys on: `SolveReceipt`/`EigenSolveReceipt<TEigen,TVector>`/`GaugeReceipt` — full field lists in [02]-[NUMERICS].
- `Numerics/Predicates` `Predicate.Orient3D`/`InSphere` public verdicts satisfy Compute `CDTet` exact gates BY SHAPE — never a Compute-side predicate mint (`ARCHITECTURE.md#[02]` row 106).
- Matrix floor: `Matrix`/`SymmetricMatrix`/`SparseMatrix`/`SparseHermitian` over MathNet+CSparse, `CholeskySparse`, `GaugePolicy`, `MatrixKernel` LOBPCG real+Hermitian — Sinkhorn couplings, PCA covariance, spectral cuts all EXIT as these types (README `[NUMERICS]` router rows 20–23).

### Materials — the atoms value-object vocabulary

- `Numerics/atoms` dimension/magnitude value objects (`Dimension`/`PositiveMagnitude`/`UnitInterval`/`VectorAngle`), `Direction`/`VectorSpan`/`VectorFrame`/`VectorCone`, and the promoted `AtomProjection`/`ProjectionRow` dispatch every `.Project<TOut>` routes through (README router row 19). Admission through the Domain bridge: `key.AcceptValidated<VectorAngle>(candidate)` / `TryCreateValidated<double,TVO>()` — never bare `Create` (discards `Validate` evidence).
- Context tolerance triad (`AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance`, public `.Value`) + `UnitSystem` + the two derived tolerances (`Fractional`, `MeshIntersectionTolerance`) — bind these, never re-derive.

### Element / Bim — footprint, representation keys, graph seams

- `ContentHash.Of` composed for every `NodeId`/`ContentAddress` at the Element projection/address seam (`ARCHITECTURE.md#[02]` row 87).
- `EntityKind` (`Vertex`/`Edge`/`Face` + `SignatureArity`) — the shared topology discriminant graph/IFC consumers key on; `RebuiltEntity.Self` + incidence + histogram is the re-key contract (`Spatial/naming.md:EntityKind`, `Spatial/reconciliation.md:CanonicalTopology.Entities`).
- `TopoName` (reference axis) vs `GeometryHash` (content axis) — orthogonal `[ValueObject<UInt128>]` promotions; Element addressing composes BOTH without conflating.
- Representation: brep footprints encode via meshed patch through `CanonicalTopology.OfMesh` until the declared brep-factory GROWTH lands; `VectorCloudShape` 17-column projection is a cross-package field-set contract (embedded in `MeshFaceShape`).

### Host re-entry (dormant, names frozen)

- `Analyze`/`AnalysisQuery`/`Env`/`Operation` — `Rasm.Rhino/Commands` + Grasshopper bind: `Analyze.Query<object,TOut>(query)` then `Operation.Apply(…).Run(env: new Env(Context:…, Progress:…, Cancellation:…))`; overlay probe `Analyze.Run<object,BoundingBox>(AnalysisQuery.Bounds(Bounds.AxisAligned), …)`; `Fault.Unsupported` code 9104 is the host drain discriminant (`Analysis/query.md#[05]` [HOST_CONTRACT_FREEZE]).
- 11 GH sources `using Op = Rasm.Domain.Op`; `Directory.Build.props` injects the `Rasm.Domain` global using.

---

## [06]-[HANDROLL_BAIT] — the naive re-derivations briefs must FORBID

| # | Contract | Tempting-wrong form (name it in the brief) | Compose instead |
|---|---|---|---|
| H1 | Federation identity | `XxHash128.HashToUInt128(bytes)` inline at a call site; `XxHash64`; `HashCode.Combine`; any `System.Security.Cryptography.*`; a seed or width change | `ContentHash.Of` (`Domain/identity.md#[02]`) |
| H2 | Canonical bytes | a second mesh/cloud/NURBS byte encoder with drifted field order before hashing | `EncodeForm.Of` + frozen `MeshStream`/`CloudStream`/`ParametricStream` (`Spatial/reconciliation.md`) |
| H3 | Identity axes | comparing `TopoName.Value == NameAddress.ContentHash`; binding a name AS a content hash | orthogonal `[ValueObject<UInt128>]` axes — cross-axis compare is a type error |
| H4 | Determinism | `System.Random`; a private splitmix64; hand-rolled coordinate hash; `Guid.NewGuid()` for ordering; minting identity from `OrderKey` | `Deterministic` 6-member family (`Domain/identity.md#[03]`) |
| H5 | Error rail | `Error.New(...)` in domain flow; a parallel `Result<T>`; a sibling error record; a bare `try/catch`; a `Fault` case for a robust-core concern (belongs in `GeometryFault` 2400) | `Op` factories + `Fault` union + `Op.Catch` (`Domain/rails.md`) |
| H6 | Resources | raw `IDisposable` field; scattered `using`; a bool `owned` flag; a parallel owned/borrowed wrapper pair | `Lease<T>` (`Domain/rails.md#[05]`) |
| H7 | Validity | private `IsValid => a && b && c` chain; `double.IsFinite` (admits `UnsetValue`); adding an `OpAcceptance` arm; reviving `AnalysisAcceptance.ValidityOf`; a `typeof(T)` validity ladder | implement `IValidityEvidence` with a `ValidityClaim.All` fold (`Domain/rails.md#[06]`, `Domain/validation.md#[03]`) |
| H8 | Tolerances | inline epsilon literals; `RhinoDoc.ActiveDoc.ModelAbsoluteTolerance` at a call site; re-computing `Fractional`/`MeshIntersectionTolerance`; an optional tolerance-tail parameter | `Context` + triad (`Domain/context.md`) |
| H9 | Doc coupling | any member beyond `Context.Of(RhinoDoc?)` + `Analyze.From(RhinoDoc?)` reading `RhinoDoc`/`RhinoApp`/view/display | the two adapters ONLY |
| H10 | Readiness | inline readiness `if`; a bool `validate` knob; scattered `geometry.IsValid`; a per-type validator class | `Requirement.ForKind(...).Apply(...)` (`Domain/validation.md#[02]`) |
| H11 | Value-object admission | bare `Create`/`TryCreate` (discards evidence); the killed `WithPositive`/`WithPositivePair` wrapper quartet | `TryCreateValidated<TRaw,TVO>` / `key.AcceptValidated<TVO>` (`Domain/validation.md#[04]`) |
| H12 | Taxonomy | `Can*`-prefixed sibling predicate families; a `Kind`/`Topology`/`CurveForm` rename; per-target `CoerceToBrep` siblings; `ObjectType` leaking past `ByObjectType` | `Kind`/`Capability` columns + `Normalization.CoerceTo<T>` (`Domain/normalization.md`) |
| H13 | Closest-point | `ClosestOfCurve`/`ClosestOfMesh` per-form siblings; caller-supplied `Distance`; NaN/`Point2d.Unset` sentinels; per-form `CurveHit`/`MeshHit` receipts; a `typeof(TOut)` switch in `Project` | `ClosestHit` + `ClosestOf` + `AtomProjection.Rows` (`Domain/evaluation.md`) |
| H14 | Stats | naive `Σx² − (Σx)²/n` variance; best-so-far argmax loops instead of `Stat.Extrema`; a covariance re-mint beside `SampleMoment`; consumer re-derivation of the packed-triangle offset | `Stat`/`Distribution`/`SampleMoment` (`Domain/stats.md`) |
| H15 | Broad-phase | naive O(n²) neighbor scan; consumer-local `RTree.CreateFromPointArray`; wrapping `SpatialIndex` for point k-NN; re-implementing AABB broad-phase in neighbors | primitive tier `Spatial.Apply`; point tier `NeighborKernel.GraphOf`/`NeighborIndex` — the two altitudes never re-implement each other |
| H16 | Winding/SDF | re-deriving winding sign per point; a second GWN or SDF evaluator beside fields; per-visit subtree re-walks in the GWN descent | batched `SpatialQuery.Winding` → `QueryResult.Field`; `fields.md` delegates `MeshSdf` (ONE distance-field lane) |
| H17 | Float bounds | round-to-nearest `(float)` cast on node bounds (false-negative prune) | `Down`/`Up` = `BitDecrement`/`BitIncrement` at the ONE arena write seam |
| H18 | Wire | a Compute-owned `AccelerationStructure` return; a domain-local record duplicating the wire; in-place refit mutation of a published index | `NodeLinkProjection` raw `(float[], long[])`; persistent `Refit` |
| H19 | Graph lanes | hand-rolled Prim/Kruskal MST; a stored graph field crossing a seam | QuikGraph in-computation lanes, result exits as kernel-owned SoA wire (README `[GRAPH_ALGORITHM]`) |
| H20 | Frames | a second RMF/double-reflection body | `NeighborKernel.BishopChain` — the ONE generator |
| H21 | PCA | local weighted-covariance loop; raw MathNet reach; first-returned eigenpair convention | `CloudKernel.CovarianceOf` → `SampleMoment` → `SymmetricMatrix`; `Stat.Extrema` eigen-selection |
| H22 | Proximity egress | per-consumer `PlaneSpace`/`ClusterSpace` wrapper family; `typeof` output ladders; a second native `PointCloud` index beside `ClusterCase.Indexed` | `SupportSpace.Of` + `SupportProjection.Project<TOut>`; `ClusterCase.ClosestVertex` |
| H23 | Transport | raw MathNet dense Sinkhorn; `Math.Exp` on unfloored exponent (silent denormal); ad-hoc correspondence re-thresholding | log-domain kernel + `LogUnderflowFloor`; policy `CouplingCutoff` |
| H24 | Naming | fail-fast first-collision fold; keying `VertexNames` by `IncidentVertices[0]`; strict-subset migration predicate | accumulate `Validation<Error,NameTable>`; key by `RebuiltEntity.Self`; `TopoSignature.Overlap` fraction |
| H25 | Request ADT | a second `GeometryRequest` union; `RunMany`/`RunPair`/`RunService` verb siblings; `Get`/`GetMany`/`GetBy<Key>` families | ONE `AnalysisQuery` + 3 arity dispatches on `Analyze.Run` |
| H26 | Bounding | ad-hoc foreach min/max bbox loop; hand-rolled Welzl; vertex-average centroid | `Bounds` union → `BoundsOf`/`RitterFit`; mass-backed `CentroidOf` |
| H27 | Mass ops | `MeasureLength`/`MeasureArea`/`MeasureVolume`/`MeasureCentroid` verb family; escaping `AreaMassProperties.Compute` handle | `(MassKind × MassProperty)` coordinate under `Lease.Owned(...).Use(...)` |
| H28 | Topology scalars | stored/re-enumerated genus; per-op `is Mesh`/`is Brep` switches; per-row `Mesh.Check` re-runs (N-fold host cost) | derived `GenusOf` over `OnGeometry`; ONE `MeshCheck` capture threaded to 13 rows |
| H29 | Intersection | `switch` over type pairs / `IntersectXxYy` method family; direct `Rhino.Geometry.Intersect.Intersection.*` bypassing disposal/tolerance/cancel; a second tangency intersector; sampled deviation | 25-row `IntersectionCases` table; `Analyze.IntersectionOf`; `EnrichTangency` fold; `Curve.GetDistancesBetweenCurves` |
| H30 | Robust re-implement | a hand-rolled predicate-exact or hidden-line kernel inside Analysis | coexist by altitude with `Meshing/intersect` + `Drawing/view` — never call, never re-implement |
| H31 | Orientation sign | raw double-determinant `(b-a)×(c-a)` test; epsilon-inflated `Math.Abs(det) < tol` | `Predicate.Orient2D/Orient3D/InCircle/InSphere` (`Numerics/predicates.md:Predicate`) |
| H32 | Constructed-point order | materialize SSI/LPI/TPI crossing as rounded `Point3d` then sort by coordinate; pre-rounded `Plane` normals into a solve | `Predicate.Compare(in Implicit, in Implicit, Axis)`; `Tpi` nine-point `Homogeneous<T>`; `Implicit.Round()` ONLY at emission |
| H33 | EFT primitives | a free-function `TwoSum`/`TwoProduct` set; per-call-site FMA probe | `Expansion` + the one `NumericsPolicy.HardwareFma` gate |
| H34 | Linear algebra reach | direct MathNet `.Evd()`/`.Cholesky()`/`DenseMatrix` in a sibling; hand-rolled Gram-Schmidt; `.Inverse()` in a hot loop | `Matrix.DecomposeQr`/held factor solves → `MatrixKernel` — the ONE MathNet+CSparse path |
| H35 | Singular/generalized solves | pin-a-row hack or ad-hoc mean subtraction on a singular Laplacian; mass-lumping + dense EVD or explicit `M⁻¹A` for `Az=λMz` | `SparseMatrix.SingularSolveDetailed(rhs, GaugePolicy, Context, key)`; `GeneralizedEigenpairsDetailed(mass, k, key)` |
| H36 | Spectral weights | inline `Math.Exp(-t*λ)` HKS loop per consumer; absolute `λ < SqrtEpsilon` zero-mode gate (misclassifies mm-scale spectra) | `SpectralFilter.Weight`/`ApplyDetailed`; `SpectralBasis.ZeroBand` |
| H37 | Typed projection | inline `typeof(TOut)` switch on a consumer surface; direct `Vector3d.VectorAngle` from a consumer | `AtomProjection.Rows` + declared `ProjectionRow`; `VectorAngle.Of` over `AnglePivot.Compute` |
| H38 | RK stepping | duplicating the `Combine` linear-combination fold; chord interpolation of a trajectory; asserting Butcher coefficients by comment | `IntegrationModule.Combine`; `DenseOutputSpan.PointAt`; `ButcherTableau` moment validation |
| H39 | Fault minting | a page-local `SpatialFault`/`HealFault`; inline `int` error codes; re-minting a payload discriminant the sibling owns | ONE `GeometryFault` case in the owning sub-band, lowered via `.ToError()` (`Numerics/faults.md`) |
| H40 | Field math | re-diffing a kernel-windowed quantity; hard-wiring `∇`/`∇×` to a field union; a second noise/perm table | `Nabla.*` over a sampler; derivatives off `KernelProfile`; `FieldNoise` canonical `PermTable` |

---

## [07]-[SEAM_ROWS] — ARCHITECTURE `[02]` rows offered outward from this lane (VERBATIM)

```text seams
Domain/Identity.cs        →  csharp:Rasm.Element/Projection/address       # [CONTENT_KEY]: the kernel seed-zero XxHash128 ContentHash.Of entry the Rasm.Element seam composes for every NodeId/ContentAddress — ONE hasher, no second hasher
Domain/Identity.cs        →  csharp:Rasm.Persistence/Element/codec        # [CONTENT_KEY]: ContentAddress composes the kernel seed-zero XxHash128 entry — no second hasher at the codec
Domain/Identity.cs        →  csharp:Rasm.Compute/Model/identity           # [CONTENT_KEY]: ModelIdentity.Checksum composes ContentHash.Of — the ONE federation hasher, never a per-call-site XxHash128
Spatial/Reconciliation.cs →  csharp:Rasm.Persistence/Query/topology       # [CONTENT_KEY]: adjacency-derived GeometryHash canonical-byte content-identity hashed through the kernel Domain/Identity seed-zero entry; geometry crosses the seam by content-hash ONLY, read never re-minted
Spatial/Reconciliation.cs ⇄  python:runtime/evidence/identity             # [CONTENT_KEY]: canonical-byte content-identity reproducing the one Domain/Identity seed (XxHash128 seed-zero)
Spatial/Reconciliation.cs ⇄  typescript:core/value/contentKey             # [CONTENT_KEY]: content-hashing wasm reproducing the one Domain/Identity seed (XxHash128 seed-zero)
Numerics/Spectral.cs      ⇄  csharp:Rasm.Compute                          # [SHAPE]: DiscreteCalculus DEC operator bundle — the frozen adjoint-carrier shape
Spatial/Index.cs          ⇄  csharp:Rasm.Fabrication/Toolpath/guard       # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
Spatial/Index.cs          ⇄  csharp:Rasm.Fabrication/Posting/projection   # [SHAPE]: SpatialIndex BVH broad-phase
Spatial/Index.cs          →  csharp:Rasm.Compute                          # [WIRE]: Spatial.Apply Wire case emits; Compute decodes
Numerics/Predicates.cs    ←  csharp:Rasm.Fabrication/Posting              # [WIRE]: Predicate.Orient2D/Orient3D exact verdict
Numerics/Predicates.cs    ←  csharp:Rasm.Compute/Solver/discretization    # [SHAPE]: CDTet exact gates — the public Predicate.Orient3D/InSphere verdicts satisfy by shape, never a Compute-side predicate mint
Analysis/Query.cs         →  csharp:Rasm.Rhino/Commands                   # [BOUNDARY]: Analyze/AnalysisQuery/Env frozen-name contract (Commands + Overlay bind the same entry); dormant host edge until host-boundary re-entry
*                         ←  csharp:Rasm.Fabrication                      # [SHAPE]: Matrix / Point3d / Vector3d
```

Counterpart-status flags (verified against disk):
- [FLAGGED-DORMANT] `Analysis/Query.cs → Rasm.Rhino/Commands` — the ARCHITECTURE row itself marks the host edge dormant until host-boundary re-entry; `Rasm.Rhino` exists as a folder but the Commands binding is a frozen-name contract, not a landed counterpart. Same status applies to the adjacent `Processing/Intent.cs → Rasm.Rhino/Camera` row (out of lane but shares the freeze).
- [PRESENT] All csharp counterparts exist as package folders: `Rasm.Element`, `Rasm.Persistence`, `Rasm.Compute`, `Rasm.Fabrication`.
- [PRESENT] `python:runtime` (`libs/python/runtime/.planning/`) and `typescript:core/value` (`libs/typescript/core/.planning/value/`) counterpart folders exist; the reconciliation rows' reproduce-the-seed obligations are DESIGN-PIN until fixtures [3]–[7] of `ONE_WIRE_FIXTURE_CORPUS` land on those sides.
- [NOTE] `ONE_WIRE_FIXTURE_CORPUS` fixtures [3]–[7] are DESIGN-PIN owned by Compute/Persistence/AppHost/Element — the counterpart sides of the [CONTENT_KEY]/[WIRE] rows are declared but not yet harness-proven; only [1] CANONICAL_BYTE_IDENTITY and [2] CLASH_GOLDEN are REAL.
