# [RASM_VALIDATION]

The one acceptance/readiness oracle (`Rasm.Domain`). This page owns five surfaces with one law between them — everything that admits, gates, or proves a value in the kernel routes here, and no second acceptance rail exists anywhere in the corpus. `Requirement`/`Check` is the geometry-readiness algebra: composable requirement rows folded over a delegate-backed check matrix, dispatched from `Kind` topology, lease-aware over coercible primitives. `OpAcceptance` is the ONE validity oracle: compiled-lambda `IsValid` capture for the Rhino value shapes, foreign-material arms, and a single `IValidityEvidence` arm through which every kernel receipt registers — the mature `AnalysisAcceptance` parallel rail is dead, absorbed here. `TryCreateValidated`/`AcceptValidated` bridge Thinktecture factory validation into the fault rail. `RequirementContext.Pair` is the two-operand kind-resolve-then-validate combinator every pairwise operation composes. `Admit` is the canonical admission vocabulary — the shape- and collection-level input guards every module composes instead of re-spelling.

## [01]-[INDEX]

- [02]-[READINESS_ALGEBRA]: `Requirement` + `Check` — composable readiness requirements over the full Rhino check matrix, `ForKind` topology dispatch, lease-aware execution.
- [03]-[ACCEPTANCE_ORACLE]: `OpAcceptance` — the one validity oracle and result-acceptance gate; the `IValidityEvidence` registration law.
- [04]-[FACTORY_BRIDGE]: `TryCreateValidated` + `OpExtensions` — the generic Thinktecture admission bridge and the optional-key resolver.
- [05]-[PAIR_COMBINATOR]: `RequirementContext.Pair` — the two-operand kind-resolve-then-validate combinator.
- [06]-[ADMISSION_VOCABULARY]: `Admit` — the corpus-wide input-guard vocabulary (counts, finiteness, weights, spectra, frames, directions).

## [02]-[READINESS_ALGEBRA]

- Owner: `Requirement` sealed record — an immutable `Seq<Check>` with monoid `+` (concat-distinct) — plus the private nested `Check` `[SmartEnum<string>]` whose rows carry their applicability predicate and check body as `[UseDelegateFromConstructor]` columns. Readiness is data: a requirement is a set of check rows, never a method family.
- Cases: `Requirement` rows — `None` · `Basic` (validity + usable bounds) · `CurveLength` · `AreaMass` (closed-planar + self-intersection) · `MeshCheck` · `SolidTopology` (brep integrity + mesh manifold + brep solid + mesh check) · `VolumeMass` (solid topology + surface solid) · `SurfaceEvaluation` (usable UV domain) · `Strict` (every row). `Check` rows — `Validity` (`IsValidWithLog`) · `UsableBounds` (accurate box computes and is valid — `IsDegenerate < 4`, so flat and point boxes pass: a point's box is its point) · `BrepIntegrity` (`IsValidTopology`/`IsValidGeometry`/`IsValidTolerancesAndFlags`, each log captured) · `MeshRhinoCheck` (`Mesh.Check` full defect report) · `MeshManifoldReadiness` (`IsSolid`) · `BrepSolidReadiness` · `SurfaceSolidReadiness` · `CurveLengthReadiness` (not short, length above model tolerance) · `CurveAreaReadiness` (closed + planar) · `SurfaceDomainReadiness` · `ContinuityReadiness` (C1 discontinuity scan, both surface directions) · `PolycurveStructure` (`HasGap`) · `CurveSelfIntersection` (`Intersection.CurveSelf` event count).
- Entry: `Apply<T>(Context?, T?, CancellationToken)` → `Validation<Error, T>` — the ONE readiness gate: null value → `Fault.MissingGeometry`; empty requirement → straight input admission through the oracle (`AcceptInput` — a readiness rejection is `Fault.InvalidInput`, never a result fault); a null `Context` under a non-empty requirement → `Fault.MissingContext`; otherwise the check fold. `ForKind(Kind)` dispatches topology → requirement through the exhaustive generated `Topology.Map` so callers never hand-pick rows and a new `Topology` row breaks this dispatch loudly at compile time: Curve → `CurveLength`, Surface → `SurfaceEvaluation`, Brep/Extrusion → `SolidTopology`, Mesh/SubD → `MeshCheck`, Point/PointCloud/Hatch → `None`, Unknown → `Basic`.
- Auto: `RunChecks` folds every row applicatively over one `Validation` rail — independent check failures ACCUMULATE (a brep reports its bad topology and its degenerate bounds in one verdict), and each row self-skips through its `Applies` column. Non-`GeometryBase` values run lease-aware: `Domain/normalization.md`'s owners — the `Capability` row vocabulary (`Capability.CurveForm.Admits`/`Capability.SurfaceForm.Admits`/`Capability.Coercible`) and the `Normalization` `Lease`-returning `CurveForm`/`SurfaceForm`/`BrepForm` recoveries — lift the primitive to native geometry, the checks run inside `Lease.Use`, and owned conversions dispose on exit.
- Law: every check failure is `Fault.InvalidGeometry(shape, checkKey, log)` — the failing `Type` names WHAT, the row key names WHICH readiness failed, and the host log says WHY (the payload is the type, never the live reference — the `rails.md` evidence law); cancellation pre-empts every row as `Fault.Cancelled`. `Demand` is the one verdict constructor — `MeshReport`'s lazy guard is the named exemption (the `TextLog` string materializes only on failure); a check body building its own fault is the deleted form.
- Law: the matrix is closed and row-owned — a new readiness concern is one `Check` row (key + `applies` + `run` columns) and membership in the requirement rows that need it; a boolean parameter, a per-type validator class, or an inline readiness `if` at a call site is the deleted form.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Validation` applicative fold, `Seq`), RhinoCommon (the check matrix members — `Mesh.Check`/`MeshCheckParameters`/`TextLog`, `Brep.IsValid*`, `Surface.GetNextDiscontinuity`/`IsSolid`/`Domain`, `Curve.IsShort`/`GetLength`/`IsClosed`/`TryGetPlane`/`GetNextDiscontinuity`, `PolyCurve.HasGap`, `Intersection.CurveSelf`, `GeometryBase.IsValidWithLog`/`GetBoundingBox`, `BoundingBox.IsDegenerate`).
- Boundary: `MeshReport` (the `Mesh.Check` capture returning the populated `MeshCheckParameters`) and `CurveSelfIntersectionReport` (the disposing `CurveIntersections` probe) are the two `[BoundaryAdapter]` statement seams; `Analysis/inspect.md` composes `MeshReport` for its defect surface.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed partial record Requirement {
    private static readonly Op Operand = Op.Of(name: nameof(Operand));
    private readonly Seq<Check> checks;
    private Requirement(Seq<Check> checks) => this.checks = checks;
    internal bool IsEmpty => checks.IsEmpty;
    private static Requirement Single(Check check) => new(checks: Seq(check));
    public static readonly Requirement None = new(checks: Seq<Check>());
    public static readonly Requirement Basic = new(checks: Seq(Check.Validity, Check.UsableBounds));
    public static readonly Requirement CurveLength = Basic + Single(check: Check.CurveLengthReadiness);
    public static readonly Requirement AreaMass = Basic + Single(check: Check.CurveAreaReadiness) + Single(check: Check.CurveSelfIntersection);
    public static readonly Requirement MeshCheck = Basic + Single(check: Check.MeshRhinoCheck);
    public static readonly Requirement SolidTopology = Basic + Single(check: Check.BrepIntegrity) + Single(check: Check.MeshManifoldReadiness) + Single(check: Check.BrepSolidReadiness) + Single(check: Check.MeshRhinoCheck);
    public static readonly Requirement VolumeMass = SolidTopology + Single(check: Check.SurfaceSolidReadiness);
    public static readonly Requirement SurfaceEvaluation = Basic + Single(check: Check.SurfaceDomainReadiness);
    public static readonly Requirement Strict = new(checks: toSeq(Check.Items));
    public static Requirement operator +(Requirement left, Requirement right) => Add(left: left, right: right);
    public static Requirement Add(Requirement left, Requirement right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(checks: left.checks.Concat(right.checks).Distinct().ToSeq());
    }
    public Validation<Error, T> Apply<T>(Context? context, T? value, CancellationToken cancel = default) where T : notnull =>
        (value, context, this) switch {
            (null, _, _) => Fin.Fail<T>(error: new Fault.MissingGeometry()).ToValidation(),
            (T candidate, _, Requirement { IsEmpty: true }) => Operand.AcceptInput(value: candidate).ToValidation(),
            (T candidate, Context ctx, Requirement req) => RunChecks(checks: req.checks, context: ctx, original: candidate, cancel: cancel),
            _ => Fin.Fail<T>(error: new Fault.MissingContext(Key: Operand)).ToValidation(),
        };
    public static Requirement ForKind(Kind kind) {
        ArgumentNullException.ThrowIfNull(argument: kind);
        return kind.Topology.Map(
            unknown: Basic,
            point: None,
            curve: CurveLength,
            surface: SurfaceEvaluation,
            brep: SolidTopology,
            mesh: MeshCheck,
            subD: MeshCheck,
            pointCloud: None,
            hatch: None,
            extrusion: SolidTopology);
    }
    private static bool HasUsableDomain(Surface surface, Context context) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) is (Interval u, Interval v)
        && u.IsValid && v.IsValid && u.Length > context.Absolute.Value && v.Length > context.Absolute.Value;
    [BoundaryAdapter]
    internal static Fin<MeshCheckParameters> MeshReport(Mesh mesh, string check) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return guard(mesh.Check(textLog: textLog, parameters: ref parameters), () => (Error)new Fault.InvalidGeometry(Shape: mesh.GetType(), Check: check, Log: textLog.ToString()))
            .ToFin()
            .Map(_ => parameters);
    }
    private static Validation<Error, T> RunChecks<T>(Seq<Check> checks, Context context, T original, CancellationToken cancel) where T : notnull =>
        original switch {
            GeometryBase geometry => RunChecks(checks: checks, context: context, geometry: geometry, original: original, cancel: cancel),
            object curveLike when Capability.CurveForm.Admits(type: curveLike.GetType()) => RunLeaseChecks(lease: Normalization.CurveForm(source: curveLike, key: Operand), checks: checks, context: context, original: original, cancel: cancel),
            object surfaceLike when Capability.SurfaceForm.Admits(type: surfaceLike.GetType()) => RunLeaseChecks(lease: Normalization.SurfaceForm(source: surfaceLike, key: Operand), checks: checks, context: context, original: original, cancel: cancel),
            object brepLike when Capability.Coercible(source: brepLike.GetType(), target: typeof(Brep)) => RunLeaseChecks(lease: Normalization.BrepForm(source: brepLike, key: Operand), checks: checks, context: context, original: original, cancel: cancel),
            _ => Operand.AcceptInput(value: original).ToValidation(),
        };
    private static Validation<Error, T> RunChecks<T>(Seq<Check> checks, Context context, GeometryBase geometry, T original, CancellationToken cancel) where T : notnull =>
        checks
            .Fold(
                initialState: (Value: Fin.Succ(unit).ToValidation(), Context: context, Geometry: geometry, Original: original, Cancel: cancel),
                f: static (rail, check) => rail with { Value = (rail.Value, check.Apply(context: rail.Context, geometry: rail.Geometry, cancel: rail.Cancel).ToValidation()).Apply(static (_, _) => unit).As() })
            switch {
                (Validation<Error, Unit> validation, _, _, T value, _) => (validation, Fin.Succ(value).ToValidation()).Apply(static (_, item) => item).As(),
            };
    private static Validation<Error, T> RunLeaseChecks<T, TGeometry>(Fin<Lease<TGeometry>> lease, Seq<Check> checks, Context context, T original, CancellationToken cancel)
        where T : notnull
        where TGeometry : GeometryBase =>
        (Fin.Succ((Checks: checks, Context: context, Original: original, Cancel: cancel)).ToValidation(),
         lease.ToValidation())
            .Apply(static (state, native) => native.Use(
                state: state,
                project: static (state, geometry) => RunChecks(checks: state.Checks, context: state.Context, geometry: geometry, original: state.Original, cancel: state.Cancel)))
            .Bind(static validation => validation)
            .As();
    [BoundaryAdapter]
    private static Fin<Unit> CurveSelfIntersectionReport(Check check, Curve curve, double tolerance) {
        using CurveIntersections? hits = Intersection.CurveSelf(curve: curve, tolerance: tolerance);
        return hits switch {
            null => check.Demand(geometry: curve, condition: false, log: "Rhino curve self-intersection computation failed."),
            { Count: 0 } => check.Demand(geometry: curve, condition: true, log: string.Empty),
            CurveIntersections found => check.Demand(geometry: curve, condition: false, log: string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {found.Count} curve self-intersection event(s).")),
        };
    }
    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
    private sealed partial class Check {
        public static readonly Check Validity = new(key: "rhino-validity", applies: static _ => true, run: static (check, _, g, _) => check.Demand(geometry: g, condition: g.IsValidWithLog(log: out string log), log: log));
        public static readonly Check UsableBounds = new(key: "usable-bounds", applies: static _ => true, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: g.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: ctx.Absolute.Value) < 4, log: "Rhino could not compute a usable accurate bounding box."));
        public static readonly Check BrepIntegrity = new(key: "brep-integrity", applies: static g => g is Brep, run: static (check, _, g, _) => g is Brep b ? (b.IsValidTopology(log: out string tLog), b.IsValidGeometry(log: out string gLog), b.IsValidTolerancesAndFlags(log: out string toLog)) switch { (false, _, _) => check.Demand(geometry: b, condition: false, log: $"Brep topology: {tLog}"), (_, false, _) => check.Demand(geometry: b, condition: false, log: $"Brep geometry: {gLog}"), (_, _, false) => check.Demand(geometry: b, condition: false, log: $"Brep tolerances and flags: {toLog}"), _ => check.Demand(geometry: b, condition: true, log: string.Empty) } : check.Demand(geometry: g, condition: true, log: string.Empty));
        public static readonly Check MeshRhinoCheck = new(key: "mesh-rhino-check", applies: static g => g is Mesh, run: static (check, _, g, _) => g is Mesh mesh ? MeshReport(mesh: mesh, check: check.Key).Map(static _ => unit) : check.Demand(geometry: g, condition: true, log: string.Empty));
        public static readonly Check MeshManifoldReadiness = new(key: "mesh-manifold-readiness", applies: static g => g is Mesh, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Mesh)g).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
        public static readonly Check BrepSolidReadiness = new(key: "brep-solid-readiness", applies: static g => g is Brep, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Brep)g).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check SurfaceSolidReadiness = new(key: "surface-solid-readiness", applies: static g => g is Surface, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Surface)g).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check CurveLengthReadiness = new(key: "curve-length-readiness", applies: static g => g is Curve, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: g is Curve c && !c.IsShort(tolerance: ctx.Absolute.Value) && c.GetLength(fractionalTolerance: ctx.Fractional) > ctx.Absolute.Value, log: "Curve is valid Rhino geometry but is below model-length tolerance."));
        public static readonly Check CurveAreaReadiness = new(key: "curve-area-readiness", applies: static g => g is Curve, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: g is Curve c && c.IsClosed && c.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value), log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."));
        public static readonly Check SurfaceDomainReadiness = new(key: "surface-domain-readiness", applies: static g => g is Surface, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: HasUsableDomain(surface: (Surface)g, context: ctx), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
        public static readonly Check ContinuityReadiness = new(key: "continuity-readiness", applies: static g => g is Curve or Surface, run: static (check, ctx, g, _) => g switch {
            Surface surface => check.Demand(
                geometry: surface,
                condition: HasUsableDomain(surface: surface, context: ctx)
                    && !surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _)
                    && !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _),
                log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
            Curve curve => check.Demand(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
            _ => check.Demand(geometry: g, condition: true, log: string.Empty),
        });
        public static readonly Check PolycurveStructure = new(key: "polycurve-structure", applies: static g => g is PolyCurve, run: static (check, _, g, _) => g is PolyCurve p ? check.Demand(geometry: p, condition: !p.HasGap, log: "PolyCurve has gaps between segments.") : check.Demand(geometry: g, condition: true, log: string.Empty));
        public static readonly Check CurveSelfIntersection = new(key: "curve-self-intersection", applies: static g => g is Curve, run: static (check, ctx, g, _) => g switch {
            Curve curve => CurveSelfIntersectionReport(check: check, curve: curve, tolerance: ctx.Absolute.Value),
            _ => check.Demand(geometry: g, condition: true, log: string.Empty),
        });
        [UseDelegateFromConstructor]
        private partial bool Applies(GeometryBase geometry);
        [UseDelegateFromConstructor]
        private partial Fin<Unit> Run(Check check, Context context, GeometryBase geometry, CancellationToken cancel);
        [BoundaryAdapter]
        internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) =>
            condition switch {
                true => Fin.Succ(unit),
                false => Fin.Fail<Unit>(error: new Fault.InvalidGeometry(Shape: geometry.GetType(), Check: Key, Log: log)),
            };
        internal Fin<Unit> Apply(Context context, GeometryBase geometry, CancellationToken cancel) =>
            cancel.IsCancellationRequested switch {
                true => Fin.Fail<Unit>(error: new Fault.Cancelled()),
                false => Applies(geometry: geometry) ? Run(check: this, context: context, geometry: geometry, cancel: cancel) : Demand(geometry: geometry, condition: true, log: string.Empty),
            };
    }
}
```

## [03]-[ACCEPTANCE_ORACLE]

- Owner: `OpAcceptance` internal static — THE validity oracle and the result-acceptance gate, its acceptance members riding one `extension(Op)` receiver block. `Op` fronts it publicly (`key.AcceptValue`/`AcceptInput` delegate through the lowered static form); the Analysis output projection (`Analysis/query.md`) routes it directly. The name is frozen — the repository analyzer's vocabulary names `OpAcceptance` by docID.
- Entry: `AcceptValue<T>` gates one value through the oracle (null → `InvalidResult`, `Enum` → pass, oracle-false or oracle-unknown → `InvalidResult`); `AcceptInput<T>` re-labels the rejection as `InvalidInput`; `Accept` (single + `IEnumerable`) lifts into `Seq` with per-element gating; `AcceptResults<TValue, TOut>` is the same-type sequence bridge — heterogeneous raw→typed projection is `Numerics/atoms.md`'s `ProjectionRow` dispatch, never a `typeof` ladder here.
- Law: the ONE-ORACLE law — `ValidityOf(object?)` is the single validity authority in the corpus. The mature `AnalysisAcceptance.ValidityOf` fork is dead: its five result arms register through `IValidityEvidence` instead, and `AnalysisOutput` calls this oracle. The oracle keeps arms ONLY for foreign material it cannot instrument — `GeometryBase`, scalars (`RhinoMath.IsValidDouble` / `RhinoMath.IsValidSingle`, both screening the host unset sentinel), `Guid`, `string`, `Ray3d`, `MeshPoint`, `ComponentIndex`, the pass-through set (`bool`/`int`/`Enum`/`SurfaceCurvature`/`MeshCheckParameters`/smart enums), scalar tuples — plus ONE `IValidityEvidence` arm for every kernel-owned receipt and carrier.
- Law: the registration law — a kernel type reaches the oracle by implementing `IValidityEvidence` with a `ValidityClaim.All` fold (`rails.md`), never by adding an oracle arm. `ClosestHit` (`evaluation.md`), `TopologyProjection` (`normalization.md`), `Stat`/`Distribution` (`stats.md`), `IntersectionHit`/`CurveDeviation` (`Analysis/relations.md`), `ResidualSample` (`Analysis/measure.md`), `NeighborHit`/`NeighborPair` (`Spatial/neighbors.md`), and every kernel receipt register this way; the mature per-type arms for those owners are the deleted form. An unknown type is REJECTED by `AcceptValue` — admission of a new result type is exactly one interface implementation.
- Law: the eighteen Rhino value shapes reach `IsValid` through compiled `Expression` lambdas cached in one `FrozenDictionary` — built once, allocation-free thereafter, no per-call reflection; the table is the last probe before `None`.
- Boundary: `OpAcceptance` is internal — the assembly-public gates are `Op`'s acceptance members and the readiness algebra above; the oracle never crosses the package seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq.Expressions;
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static partial class OpAcceptance {
    private static readonly FrozenDictionary<Type, Func<object, bool>> ValueValidity = new Type[] {
        typeof(Point2d), typeof(Point3d), typeof(Vector3d), typeof(Plane), typeof(Transform), typeof(BoundingBox), typeof(Box), typeof(Sphere),
        typeof(Cylinder), typeof(Cone), typeof(Torus), typeof(Arc), typeof(Circle), typeof(Ellipse), typeof(Rectangle3d), typeof(Interval), typeof(Line), typeof(Polyline),
    }.ToFrozenDictionary(static t => t, static t => {
        ParameterExpression p = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, bool>>(Expression.Property(Expression.Convert(p, t), "IsValid"), p).Compile();
    });
    extension(Op key) {
        internal Fin<Seq<TValue>> Accept<TValue>(TValue value) =>
            key.AcceptValue(value: value).Map(static candidate => Seq(candidate));
        internal Fin<Seq<TValue>> Accept<TValue>(IEnumerable<TValue> values) =>
            Optional(values).ToFin(key.InvalidResult()).Bind(candidates => candidates.AsIterable().ToSeq().Traverse(value => key.AcceptValue(value: value)).As());
        internal Fin<Seq<TOut>> AcceptResults<TValue, TOut>(IEnumerable<TValue> values) => typeof(TValue).Equals(typeof(TOut)) switch {
            true => key.Accept(values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
            false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut))),
        };
        internal Fin<T> AcceptInput<T>(T value) =>
            key.AcceptValue(value: value).MapFail(_ => key.InvalidInput());
        [BoundaryAdapter]
        internal Fin<T> AcceptValue<T>(T value) =>
            value switch {
                null => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
                Enum => Fin.Succ(value),
                _ => ValidityOf(source: value!).Case switch {
                    bool ok => key.Demand(condition: ok, value: value),
                    _ => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
                },
            };
        private Fin<T> Demand<T>(bool condition, T value) =>
            condition ? Fin.Succ(value) : Fin.Fail<T>(error: new Fault.InvalidResult(Key: key));
    }
    internal static Option<bool> ValidityOf(object? source) =>
        source switch {
            null => Option<bool>.None,
            GeometryBase geometry => Some(geometry.IsValid),
            double scalar => Some(RhinoMath.IsValidDouble(scalar)),
            float scalar => Some(RhinoMath.IsValidSingle(x: scalar)),
            Guid id => Some(id != Guid.Empty),
            string text => Some(!string.IsNullOrWhiteSpace(value: text)),
            Ray3d ray => Some(ray.Position.IsValid && ray.Direction.IsValid && !ray.Direction.IsTiny()),
            bool or int or Enum or SurfaceCurvature or MeshCheckParameters or ISmartEnum<int> or ISmartEnum<string> => Some(value: true),
            MeshPoint m => Some(m.Point.IsValid && m.FaceIndex >= 0 && m.ComponentIndex is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 } && m.T.All(static t => RhinoMath.IsValidDouble(t))),
            ComponentIndex c => Some(c is { ComponentIndexType: not ComponentIndexType.InvalidType } ci && ci.Index >= 0),
            ValueTuple<double, double> t => Some(t is (double x, double y) && RhinoMath.IsValidDouble(x) && RhinoMath.IsValidDouble(y)),
            ValueTuple<double, Vector3d> t => Some(t is (double m, Vector3d a) && RhinoMath.IsValidDouble(m) && m >= 0.0 && a.IsValid && a.Length > RhinoMath.ZeroTolerance),
            IValidityEvidence evidence => Some(evidence.IsValid),
            _ => ValueValidity.GetValueOrDefault(source.GetType()) is Func<object, bool> fn ? Some(fn(source)) : Option<bool>.None,
        };
}
```

## [04]-[FACTORY_BRIDGE]

- Owner: the receiver-generic `TryCreateValidated<TVO>` bridge — an `extension<TRaw>(TRaw)` block on `OpAcceptance` — plus `OpExtensions` — `OrDefault` (the optional-key resolver of the `rails.md` threading law) and `AcceptValidated<TVO>` (the key-shaped receiver overloads).
- Law: ONE bridge body for every numeric value object — the block constraint `TRaw : struct, INumber<TRaw>` with member constraint `TVO : IObjectFactory<TVO, TRaw, ValidationError>` collapses the mature double/int twin bodies into one generic-math entry. The invocation spelling is fixed by the lowering: an extension-form call supplies the COMBINED type-argument list, receiver parameter first (`absolute.TryCreateValidated<double, AbsoluteTolerance>()`) — C# has no partial type-argument inference, so the member-only spelling `absolute.TryCreateValidated<AbsoluteTolerance>()` does not compile; the width-elided one-type-argument call is exactly what the key-fronted `AcceptValidated<TVO>` receivers exist for. The generated `Validate` runs under `CultureInfo.InvariantCulture`, and a rejection lands as `Fault.OutOfRange` carrying the owner name, `double.CreateChecked` of the rejected scalar, and the generated requirement text. Bridging through `Create`/`TryCreate` discards the evidence `Validate` carries and is the rejected form.
- Law: `OrDefault` resolves `Op? key = null` to `Op.Of(callerMemberName)` — public polymorphic surfaces stay knob-free while internal kernels demand the key; this is the one spelling of optional-key resolution.
- Law: `AcceptValidated` re-keys — the raw-width receiver overloads (double, int) forward to the one bridge and stamp the demanding `Op` onto the rejection's `OutOfRange.Key`, so a key-fronted admission failure names its operation like every other key-fronted acceptance member; the keyless bridge form leaves `Key` `None` by design (factory admission has no operation).
- Boundary: consumers — the `context.md` tolerance triad admits through this bridge; `Numerics/atoms.md` value objects (`Dimension`/`PositiveMagnitude`/`UnitInterval`/`VectorAngle`) admit through `AcceptValidated` (`key.AcceptValidated<VectorAngle>(candidate)` — one type argument, the raw width resolved by overload); the mature `WithPositive`/`WithPositivePair`/`Positive(PositiveMagnitude)`/`Dimension(Dimension)` wrapper quartet is absorbed by this one generic entry.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static partial class OpAcceptance {
    extension<TRaw>(TRaw candidate) where TRaw : struct, INumber<TRaw> {
        [BoundaryAdapter]
        internal Validation<Error, TVO> TryCreateValidated<TVO>() where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
            (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
                (null, TVO ok) => Fin.Succ(ok).ToValidation(),
                (ValidationError err, _) => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: double.CreateChecked(candidate), Requirement: err.Message)).ToValidation(),
                _ => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: double.CreateChecked(candidate), Requirement: "validation failed")).ToValidation(),
            };
    }
}

public static class OpExtensions {
    extension(Op? key) {
        [BoundaryAdapter]
        public Op OrDefault([CallerMemberName] string name = "") => key ?? Op.Of(name: name);
    }
    extension(Op op) {
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(double candidate) where TVO : IObjectFactory<TVO, double, ValidationError> =>
            Rekey(op: op, admitted: candidate.TryCreateValidated<double, TVO>());
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(int candidate) where TVO : IObjectFactory<TVO, int, ValidationError> =>
            Rekey(op: op, admitted: candidate.TryCreateValidated<int, TVO>());
    }
    private static Fin<TVO> Rekey<TVO>(Op op, Validation<Error, TVO> admitted) =>
        admitted.ToFin().MapFail(error => error is Fault.OutOfRange range ? range with { Key = Some(op) } : error);
}
```

## [05]-[PAIR_COMBINATOR]

- Owner: `RequirementContext` internal static — one `extension(Context)` block carrying `Pair<TA, TB>`, the two-operand kind-resolve-then-validate combinator every pairwise operation composes receiver-style (`context.Pair(a, b, op, requirements)`).
- Auto: the pipeline is fixed — admit both operands under `Requirement.None` (null/oracle gate only), resolve both `Kind`s through the coercion owner's `KindOf`, ask the caller's `requirements(op, kindA, kindB)` policy for the per-operand `Requirement` pair, re-validate both under the resolved requirements accumulating faults applicatively, and return `(A, B, KindA, KindB)` so the operation dispatches on the resolved kinds without re-deriving them.
- Law: pairwise readiness is policy-driven — the `requirements` delegate is the caller's policy row (an intersection pair demands different readiness than a conformance pair), and the combinator owns the resolve-validate order so no pair operation re-spells it.
- Boundary: consumers — `Analysis/measure.md` conformance pairs, `Analysis/relations.md` intersection/classification/deviation pairs; `Kind` and `KindOf` are `Domain/normalization.md`'s.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class RequirementContext {
    extension(Context context) {
        internal Validation<Error, (TA A, TB B, Kind KindA, Kind KindB)> Pair<TA, TB>(
            TA a,
            TB b,
            Op op,
            Func<Op, Kind, Kind, Fin<(Requirement A, Requirement B)>> requirements,
            CancellationToken cancel = default) where TA : notnull where TB : notnull =>
            (from pair in context.Validate(a: a, b: b, requirementA: Requirement.None, requirementB: Requirement.None, cancel: cancel)
             from kindA in pair.A.KindOf(context: context).ToValidation()
             from kindB in pair.B.KindOf(context: context).ToValidation()
             from required in requirements(arg1: op, arg2: kindA, arg3: kindB).ToValidation()
             from validated in context.Validate(a: pair.A, b: pair.B, requirementA: required.A, requirementB: required.B, cancel: cancel)
             select (validated.A, validated.B, KindA: kindA, KindB: kindB)).As();
        private Validation<Error, (TA A, TB B)> Validate<TA, TB>(TA a, TB b, Requirement requirementA, Requirement requirementB, CancellationToken cancel = default) where TA : notnull where TB : notnull =>
            (requirementA.Apply(context: context, value: a, cancel: cancel),
             requirementB.Apply(context: context, value: b, cancel: cancel))
                .Apply(static (left, right) => (A: left, B: right)).As();
    }
}
```

## [06]-[ADMISSION_VOCABULARY]

- Owner: `Admit` internal static — the canonical shape- and collection-level input-guard vocabulary, absorbed from the mature field kernel's private guard half and promoted to serve every module. `Op`'s `Finite`/`Positive` own the per-scalar key-bound form (`rails.md`); `Admit` owns everything above a scalar: counts, spans, sequences, weighted sets, complex spectra, frames, directions, periods, and the parameterized kernel/falloff/noise input gates.
- Cases: null/count — `NotNull` (Op- and Error-keyed) · `CountAtLeast` · `SameCount` (params span); finiteness — the `bool` predicate floor (`Finite(Point3d)`/`Finite(Vector3d)`/`FiniteSpan`/`FiniteComplexSpan`/`HermitianDiagonalRealSpan`/`Frame(Plane)` — the orthonormal right-handed plane predicate stated once for both plane gates) and the `Fin` gates (`Finite` ×3 · `NonnegativeFinite` · `Ordered` (finite ordered pair) · `AllFinite` span/Seq/params · `AllValid` · `FinitePositive` · `FinitePoints` · `FiniteScalars` · `AllFiniteComplex` · `HermitianDiagonalReal`); structured — `WeightedPoints` (paired counts + positive finite weights) · `PositiveFiniteWeights` · `WithDivisor` (reciprocal guard) · `NonnegativeExtent` · `Plane` (the `Frame` gate) · `PlaneSequence` · `Directional` (finite, above-tolerance magnitude) · `Cone` (finite apex + directional axis + half-angle in `(0, π]`) · `Period` (finite, all components non-degenerate); parameterized inputs — `KernelInput` · `FalloffInput` · `NoiseInput` (octaves 1–32, positive frequency, persistence `(0,1]`, lacunarity `> 1`).
- Law: predicate policy has ONE owner — where a `ValidityClaim` row states the predicate (`rails.md`), the gate composes the row: `Finite`/`Nonnegative`/`Positive`/`CountAtLeast`/`Ordered` lift claims into `Op`-keyed faults, and `Admit` spells only what no claim states — complex spectra, vectorized span positivity, and the composite shape gates. The Hermitian-diagonal gate derives its tolerance from the diagonal's own scale (`max(SqrtEpsilon, scale × SqrtEpsilon)`), never an absolute literal.
- Law: a module spells its input gate as one `Admit` composition at its boundary — a private per-module guard set, a re-derived finiteness loop, or an inline `if` chain before a kernel is the deleted form. A new admission shape is one member here, composed everywhere.
- Boundary: `Numerics/atoms.md` admits its value objects through the `[04]` generic bridge and composes `Directional`/`Cone` for its `Direction`/`VectorCone` owners; `Numerics/calculus.md` composes `KernelInput`/`FalloffInput`/`NoiseInput`; `Spatial/fields.md` and `Meshing/reconstruct.md` compose the point/weight/span gates; `Numerics/matrix.md` composes the complex-spectrum gates. The span loops are the named kernel exemption.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using Rhino;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class Admit {
    internal static Fin<T> NotNull<T>(T? value, Op key) where T : class => Optional(value).ToFin(key.InvalidInput());
    internal static Fin<T> NotNull<T>(T? value, Error error) where T : class => Optional(value).ToFin(error);
    internal static Fin<Unit> CountAtLeast(int count, int minimum, Op key) => guard(ValidityClaim.CountAtLeast(count: count, floor: minimum), key.InvalidInput()).ToFin();
    internal static Fin<Unit> SameCount(int expected, Op key, params ReadOnlySpan<int> counts) {
        foreach (int count in counts) {
            if (count != expected) { return Fin.Fail<Unit>(key.InvalidInput()); }
        }
        return Fin.Succ(unit);
    }
    internal static bool Finite(Point3d point) => ValidityClaim.Finite(point: point);
    internal static bool Finite(Vector3d vector) => ValidityClaim.Finite(vector: vector);
    internal static bool FiniteSpan(ReadOnlySpan<double> values) => ValidityClaim.Finite(values);
    internal static bool FiniteComplexSpan(ReadOnlySpan<Complex> values) {
        foreach (Complex value in values) {
            if (!ValidityClaim.Finite(value: value.Real) || !ValidityClaim.Finite(value: value.Imaginary)) { return false; }
        }
        return true;
    }
    internal static bool HermitianDiagonalRealSpan(ReadOnlySpan<Complex> diagonal) {
        double scale = 0.0;
        foreach (Complex entry in diagonal) {
            if (!ValidityClaim.Finite(value: entry.Real) || !ValidityClaim.Finite(value: entry.Imaginary)) { return false; }
            scale = Math.Max(val1: scale, val2: Math.Abs(value: entry.Real));
        }
        double tolerance = Math.Max(val1: RhinoMath.SqrtEpsilon, val2: scale * RhinoMath.SqrtEpsilon);
        foreach (Complex entry in diagonal) {
            if (Math.Abs(value: entry.Imaginary) > tolerance) { return false; }
        }
        return true;
    }
    internal static Fin<Unit> Finite(double value, Op key) => guard(ValidityClaim.Finite(value: value), key.InvalidInput()).ToFin();
    internal static Fin<Unit> Finite(Point3d point, Op key) => guard(Finite(point: point), key.InvalidInput()).ToFin();
    internal static Fin<Unit> Finite(Vector3d vector, Op key) => guard(Finite(vector: vector), key.InvalidInput()).ToFin();
    internal static Fin<double> NonnegativeFinite(double value, Op key) => ValidityClaim.Nonnegative(value: value) ? Fin.Succ(value) : Fin.Fail<double>(key.InvalidInput());
    internal static Fin<Unit> Ordered(double lower, double upper, Op key) => guard(ValidityClaim.Ordered(lower: lower, upper: upper), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(ReadOnlySpan<double> values, Op key) => FiniteSpan(values) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static Fin<Unit> AllFinite(Seq<Point3d> points, Op key) => guard(points.ForAll(static point => Finite(point: point)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Op key, params ReadOnlySpan<Point3d> points) {
        foreach (Point3d point in points) {
            if (!Finite(point: point)) { return Fin.Fail<Unit>(key.InvalidInput()); }
        }
        return Fin.Succ(unit);
    }
    internal static Fin<Unit> AllValid(Op key, params ReadOnlySpan<Vector3d> vectors) {
        foreach (Vector3d vector in vectors) {
            if (!Finite(vector: vector)) { return Fin.Fail<Unit>(key.InvalidInput()); }
        }
        return Fin.Succ(unit);
    }
    internal static Fin<Unit> FinitePositive(ReadOnlySpan<double> values, Op key) =>
        !values.IsEmpty && FiniteSpan(values) && TensorPrimitives.Min(values) > 0.0 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static Fin<Seq<Point3d>> FinitePoints(Seq<Point3d> points, bool allowEmpty, Op key) =>
        (allowEmpty || !points.IsEmpty) && points.ForAll(static point => Finite(point: point)) ? Fin.Succ(points) : Fin.Fail<Seq<Point3d>>(key.InvalidInput());
    internal static Fin<Seq<double>> FiniteScalars(Seq<double> values, bool allowEmpty, Op key) =>
        (allowEmpty || !values.IsEmpty) && values.ForAll(static value => ValidityClaim.Finite(value: value)) ? Fin.Succ(values) : Fin.Fail<Seq<double>>(key.InvalidInput());
    internal static Fin<(Seq<Point3d> Points, Seq<double> Weights)> WeightedPoints(Seq<Point3d> points, Seq<double> weights, Op key) =>
        points.Count == weights.Count && !points.IsEmpty && points.ForAll(static point => Finite(point: point)) && weights.ForAll(static weight => ValidityClaim.Positive(value: weight))
            ? Fin.Succ((Points: points, Weights: weights))
            : Fin.Fail<(Seq<Point3d> Points, Seq<double> Weights)>(key.InvalidInput());
    internal static Fin<Unit> PositiveFiniteWeights(ReadOnlySpan<double> weights, int count, Op key) =>
        weights.Length == count && FiniteSpan(weights) && (weights.IsEmpty || TensorPrimitives.Min(weights) > 0.0) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static Fin<Unit> AllFiniteComplex(ReadOnlySpan<Complex> values, Op key) => FiniteComplexSpan(values) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static Fin<Unit> HermitianDiagonalReal(ReadOnlySpan<Complex> diagonal, Op key) => HermitianDiagonalRealSpan(diagonal) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static Fin<TResult> WithDivisor<TResult>(double divisor, Func<double, TResult> make, Op key) =>
        Math.Abs(value: divisor) > RhinoMath.ZeroTolerance ? Fin.Succ(make(arg: 1.0 / divisor)) : Fin.Fail<TResult>(key.InvalidInput());
    internal static Fin<Unit> KernelInput(double distance, double radius, Op key) =>
        guard(ValidityClaim.Nonnegative(value: distance) && ValidityClaim.Finite(value: radius) && radius > RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin();
    internal static Fin<Unit> FalloffInput(double distance, double distanceSquared, double tolerance, Op key) =>
        guard(ValidityClaim.Nonnegative(value: distance) && ValidityClaim.Nonnegative(value: distanceSquared) && ValidityClaim.Nonnegative(value: tolerance), key.InvalidInput()).ToFin();
    internal static Fin<Unit> NoiseInput(int octaves, double persistence, double lacunarity, double frequency, Op key) =>
        guard(octaves is >= 1 and <= 32 && ValidityClaim.Positive(value: frequency) && ValidityClaim.Positive(value: persistence) && persistence <= 1.0 && ValidityClaim.Finite(value: lacunarity) && lacunarity > 1.0, key.InvalidInput()).ToFin();
    internal static Fin<Vector3d> NonnegativeExtent(Vector3d extent, Op key) =>
        Finite(vector: extent) && extent.X >= 0.0 && extent.Y >= 0.0 && extent.Z >= 0.0 ? Fin.Succ(extent) : Fin.Fail<Vector3d>(key.InvalidInput());
    internal static bool Frame(Plane basis) =>
        basis.IsValid
        && Finite(point: basis.Origin)
        && Finite(vector: basis.XAxis)
        && Finite(vector: basis.YAxis)
        && Finite(vector: basis.ZAxis)
        && Vector3d.AreOrthonormal(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis)
        && Vector3d.AreRighthanded(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis);
    internal static Fin<Plane> Plane(Plane basis, Op key) =>
        Frame(basis: basis) ? Fin.Succ(basis) : Fin.Fail<Plane>(key.InvalidInput());
    internal static Fin<Seq<Plane>> PlaneSequence(Seq<Plane> planes, bool allowEmpty, Op key) =>
        (allowEmpty || !planes.IsEmpty) && planes.ForAll(static plane => Frame(basis: plane))
            ? Fin.Succ(planes)
            : Fin.Fail<Seq<Plane>>(key.InvalidInput());
    internal static Fin<Vector3d> Directional(Vector3d value, double tolerance, Op key) =>
        Finite(vector: value) && value.Length > tolerance ? Fin.Succ(value) : Fin.Fail<Vector3d>(key.InvalidInput());
    internal static Fin<Unit> Cone(Point3d apex, Vector3d axis, double halfAngle, Op key) =>
        from _ in Finite(point: apex, key: key)
        from direction in Directional(value: axis, tolerance: RhinoMath.ZeroTolerance, key: key)
        from angle in guard(ValidityClaim.Positive(value: halfAngle) && halfAngle <= Math.PI, key.InvalidInput()).ToFin()
        select unit;
    internal static Fin<Vector3d> Period(Vector3d period, Op key) =>
        Finite(vector: period) && Math.Abs(value: period.X) > RhinoMath.ZeroTolerance && Math.Abs(value: period.Y) > RhinoMath.ZeroTolerance && Math.Abs(value: period.Z) > RhinoMath.ZeroTolerance ? Fin.Succ(period) : Fin.Fail<Vector3d>(key.InvalidInput());
}
```

## [07]-[DENSITY_BAR]

One readiness algebra, one oracle, one bridge, one pair combinator, one admission vocabulary; a new check is a row, a new receipt is an evidence registration, a new guard is a member — never a sibling rail.

| [INDEX] | [AXIS/CONCERN]        | [OWNER]                      | [KIND]                                                        | [RAIL]                                          | [CASES] |
| :-----: | :-------------------- | :--------------------------- | :------------------------------------------------------------ | :----------------------------------------------- | :-----: |
|  [01]   | Readiness rows        | `Requirement`                | sealed record, `Seq<Check>` monoid, `ForKind` dispatch        | `Apply → Validation<Error, T>` (accumulating)   |    9    |
|  [02]   | Check matrix          | `Requirement.Check`          | `[SmartEnum<string>]` + `[UseDelegateFromConstructor]` columns | `Apply → Fin<Unit>` → `Fault.InvalidGeometry`   |   13    |
|  [03]   | Validity oracle       | `OpAcceptance`               | internal static, frozen compiled-lambda table + evidence arm  | `AcceptValue → Fin<T>` / `ValidityOf → Option<bool>` |  14+18  |
|  [04]   | Factory bridge        | `TryCreateValidated`/`OpExtensions` | `extension<TRaw>(TRaw)` admission + optional-key resolver + re-keyed receivers | `Validate → Validation<Error, TVO>` / `Fin<TVO>` |    4    |
|  [05]   | Pair readiness        | `RequirementContext`         | `extension(Context)` kind-resolve-then-validate combinator     | `Pair → Validation<Error, (A, B, KindA, KindB)>` |    1    |
|  [06]   | Admission vocabulary  | `Admit`                      | internal static guard vocabulary over the claim rows, span kernels | `Fin<Unit>`/`Fin<T>` per shape               |   36    |
