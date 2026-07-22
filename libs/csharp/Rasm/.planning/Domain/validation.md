# [RASM_VALIDATION]

`Rasm.Domain` validation owns the kernel's one acceptance, readiness, and admission-projection authority; every value proving its validity routes through it. `OpAcceptance.ValidityOf` is the corpus's single validity oracle.

Validation composes `normalization.md`'s `Kind`/`KindOf` coercion and exhaustive `Topology.Map` for topology-driven readiness dispatch, and `rails.md`'s `Fault` rail, type-not-reference evidence law, and `ValidityClaim` predicate rows for every verdict. Thinktecture factory validation and LanguageExt's `Validation` applicative fold carry generated admission and independent-fault accumulation.

## [01]-[INDEX]

- [02]-[READINESS_ALGEBRA]: `Requirement` + `Check` — the composable readiness matrix, `ForKind` topology dispatch, lease-aware execution.
- [03]-[ACCEPTANCE_ORACLE]: `OpAcceptance` — the single validity oracle and result-acceptance gate, `IValidityEvidence` registration law.
- [04]-[FACTORY_BRIDGE]: `TryCreateValidated` + `OpExtensions` + `AdmissionProjection` — generated factory admission, optional-key resolution, bidirectional projection.
- [05]-[PAIR_COMBINATOR]: `RequirementContext.Pair` — the two-operand kind-resolve-then-validate combinator.
- [06]-[ADMISSION_VOCABULARY]: `Admit` — the input-guard vocabulary above a scalar.

## [02]-[READINESS_ALGEBRA]

- Owner: `Requirement` folds a delegate-backed `Check` matrix into readiness rows — readiness is data, a requirement is a set of check rows, never a method family.
- Entry: `Apply<T>` is the ONE readiness gate; an empty requirement admits straight through the oracle as input, so a readiness rejection is `Fault.InvalidInput`, never a result fault. `ForKind` dispatches topology to requirement through the exhaustive generated `Topology.Map`, so a new `Topology` row breaks dispatch loudly at compile time and no caller hand-picks rows.
- Auto: `RunChecks` folds every row applicatively over one `Validation` rail, so independent failures accumulate into one verdict and each row self-skips through its `Applies` column. `UsableBounds` passes any box computing short of full invalidity (`IsDegenerate < 4`), so flat and point geometry clears the readiness floor. A non-`GeometryBase` value runs lease-aware: `normalization.md`'s `Capability` rows and `Lease`-returning recoveries lift it to native geometry, the checks run inside the lease, and owned conversions dispose on exit.
- Law: every check failure carries the value's `Type`, never the live reference (`rails.md` evidence law), and cancellation pre-empts every row as `Fault.Cancelled`. `Demand` is the one verdict constructor, `MeshReport`'s lazy guard the named exemption where its `TextLog` materializes only on failure.
- Law: `Check` rows are a closed, row-owned matrix — a new readiness concern is one row and its membership in the requirements that need it, never a call-site validator.
- Packages: Thinktecture.Runtime.Extensions and LanguageExt.Core drive the smart-enum delegate rows and the applicative fold; RhinoCommon carries the check-matrix members.
- Boundary: `MeshReport` and `CurveSelfIntersectionReport` are the `[BoundaryAdapter]` statement seams; `Analysis/inspect.md` composes `MeshReport` for its defect surface.

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

- Owner: `OpAcceptance` internal static is the validity oracle and result-acceptance gate; `Op` fronts it publicly and `Analysis/query.md` routes it directly. Its name is frozen, keyed by the repository analyzer's docID.
- Entry: `AcceptValue`/`AcceptInput`/`Accept`/`AcceptResults` gate one value, re-label the rejection, lift into `Seq`, and bridge a same-type sequence; heterogeneous raw-to-typed projection is `Numerics/atoms.md`'s `ProjectionRow`, never a `typeof` ladder here.
- Law: `ValidityOf(object?)` is the single validity authority — it instruments only foreign material it cannot reach otherwise (Rhino geometry, host scalars screened against the unset sentinel, the Rhino value shapes) and routes every kernel-owned receipt through one `IValidityEvidence` arm.
- Law: a kernel type reaches the oracle by implementing `IValidityEvidence` with a `ValidityClaim.All` fold (`rails.md`), never by adding an oracle arm; an unknown type is rejected by `AcceptValue`, so admitting a new result type is exactly one interface implementation.
- Law: Rhino value-shape validity rides compiled `Expression` lambdas in one `FrozenDictionary`, reflection-free per call and probed last before `None`.
- Boundary: `OpAcceptance` is internal; the oracle never crosses the package seam, and the assembly-public gates are `Op`'s acceptance members and the readiness algebra.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq.Expressions;
using Rasm.Csp;
using Rhino;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
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
            true => key.Accept(values: values).Bind(candidates =>
                candidates.TraverseM(candidate => candidate is TOut projected
                    ? Fin.Succ(projected)
                    : Fin.Fail<TOut>(key.InvalidResult())).As()),
            false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut))),
        };
        internal Fin<T> AcceptInput<T>(T value) =>
            key.AcceptValue(value: value).MapFail(_ => key.InvalidInput());
        [BoundaryAdapter]
        internal Fin<T> AcceptValue<T>(T value) =>
            value switch {
                null => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
                Enum => Fin.Succ(value),
                _ => ValidityOf(source: value).Case switch {
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

- Owner: the receiver-generic `TryCreateValidated<TVO>` bridge sits on `OpAcceptance`; `OpExtensions` carries `OrDefault`, the optional-key resolver of the `rails.md` threading law, and the `AcceptValidated<TVO>` key-shaped receivers.
- Owner: `AdmissionProjection<TRaw, TModel>` holds one admitted `Op`, a model-to-raw render delegate, and a `Fin`-gated raw-to-model admit delegate; `Render` and `Admit` run through the held key's `Catch` rail, both directions null-gated inside that window, and `SmartEnum`'s false or nullable lookup lands a typed refusal there.
- Law: one generic-math body over `TRaw : struct, INumber<TRaw>` admits every numeric width; `Validate` runs under `CultureInfo.InvariantCulture` and a rejection lands as `Fault.OutOfRange`.
- Law: lowering fixes the invocation spelling — the extension-form call supplies the combined type-argument list receiver-first (`absolute.TryCreateValidated<double, AbsoluteTolerance>()`); C# has no partial type-argument inference, so the member-only `absolute.TryCreateValidated<AbsoluteTolerance>()` cannot compile, and the width-elided one-argument call is what the key-fronted `AcceptValidated<TVO>` receivers exist for.
- Law: `AcceptValidated` spans two admission tiers and one type-erased outcome lifter, selected by input shape; the lifter exists because multi-member `[ComplexValueObject]` admission has no static `Validate` contract spanning its arities, so the caller spells `Validate` and this row owns the lift.
- Law: `OrDefault` resolves a null `Op` to `Op.Of(callerMemberName)`, so public polymorphic surfaces stay knob-free while internal kernels demand the key.
- Law: `AcceptValidated` stamps the demanding `Op` onto every rejection, so a key-fronted admission failure names its operation; the keyless bridge form leaves `Key` `None`, since factory admission has no operation.
- Boundary: `AdmissionProjection` owns pure render/admit conversion; refusal posture, held state, fallback, and presentation compose after its `Fin`, and every bidirectional boundary composes it rather than re-minting generated validation.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed class AdmissionProjection<TRaw, TModel>
    where TRaw : notnull
    where TModel : notnull {
    private readonly Op operation;
    private readonly Func<TModel, TRaw> render;
    private readonly Func<TRaw, Fin<TModel>> admit;
    private AdmissionProjection(Op operation, Func<TModel, TRaw> render, Func<TRaw, Fin<TModel>> admit) {
        this.operation = operation;
        this.render = render;
        this.admit = admit;
    }
    [BoundaryAdapter]
    public static Fin<AdmissionProjection<TRaw, TModel>> Of(
        Func<TModel, TRaw>? render,
        Func<TRaw, Fin<TModel>>? admit,
        Op? key = null) {
        Op op = key.OrDefault();
        return from renderArm in Optional(render).ToFin(op.InvalidInput())
               from admitArm in Optional(admit).ToFin(op.InvalidInput())
               select new AdmissionProjection<TRaw, TModel>(operation: op, render: renderArm, admit: admitArm);
    }
    [BoundaryAdapter]
    public Fin<TRaw> Render(TModel model) =>
        Optional(model)
            .ToFin(operation.InvalidInput())
            .Bind(valid => operation.Catch(body: () => Optional(render(arg: valid)).ToFin(operation.InvalidResult())));
    [BoundaryAdapter]
    public Fin<TModel> Admit(TRaw raw) =>
        Optional(raw)
            .ToFin(operation.InvalidInput())
            .Bind(valid => operation.Catch(body: () => Optional(admit(arg: valid)).ToFin(operation.InvalidResult()).Bind(static rail => rail)))
            .Bind(admitted => Optional(admitted).ToFin(operation.InvalidResult()));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static partial class OpAcceptance {
    extension<TRaw>(TRaw candidate) where TRaw : struct, INumber<TRaw> {
        [BoundaryAdapter]
        internal Validation<Error, TVO> TryCreateValidated<TVO>() where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
            (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
                (null, TVO ok) => Fin.Succ(ok).ToValidation(),
                (ValidationError err, _) => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: double.CreateSaturating(candidate), Requirement: err.Message)).ToValidation(),
                _ => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: double.CreateSaturating(candidate), Requirement: "validation failed")).ToValidation(),
            };
    }
}

public static class AdmissionProjection {
    public delegate bool SmartEnumLookup<TRaw, TModel>(TRaw? key, out TModel? item)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw>;

    [BoundaryAdapter]
    public static Fin<AdmissionProjection<TRaw, TModel>> Generated<TRaw, TModel>(Op? key = null)
        where TRaw : struct, INumber<TRaw>
        where TModel : notnull, IObjectFactory<TModel, TRaw, ValidationError>, IConvertible<TRaw> {
        Op op = key.OrDefault();
        return AdmissionProjection<TRaw, TModel>.Of(
            render: static model => model.ToValue(),
            admit: raw => OpExtensions.AcceptValidated<TRaw, TModel>(op: op, candidate: raw),
            key: op);
    }

    [BoundaryAdapter]
    public static Fin<AdmissionProjection<TRaw, TModel>> SmartEnum<TRaw, TModel>(
        SmartEnumLookup<TRaw, TModel>? lookup,
        Op? key = null)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw> {
        Op op = key.OrDefault();
        return Optional(lookup).ToFin(op.InvalidInput()).Bind(valid =>
            AdmissionProjection<TRaw, TModel>.Of(
                render: static model => model.ToValue(),
                admit: raw => valid(key: raw, item: out TModel? item) && item is TModel admitted
                    ? Fin.Succ(admitted)
                    : Fin.Fail<TModel>(error: op.InvalidInput()),
                key: op));
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
            AcceptValidated<double, TVO>(op: op, candidate: candidate);
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(int candidate) where TVO : IObjectFactory<TVO, int, ValidationError> =>
            AcceptValidated<int, TVO>(op: op, candidate: candidate);
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(uint candidate) where TVO : IObjectFactory<TVO, uint, ValidationError> =>
            AcceptValidated<uint, TVO>(op: op, candidate: candidate);
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(string? candidate) where TVO : IObjectFactory<TVO, string, ValidationError> =>
            Validated<string, TVO>(op: op, candidate: candidate);
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(bool candidate) where TVO : IObjectFactory<TVO, bool, ValidationError> =>
            Validated<bool, TVO>(op: op, candidate: candidate);
        [BoundaryAdapter]
        public Fin<TVO> AcceptValidated<TVO>(ValidationError? fault, object? admitted) where TVO : notnull =>
            (fault, admitted) switch {
                (null, TVO owner) => Fin.Succ(value: owner),
                (ValidationError refusal, _) => Fin.Fail<TVO>(error: new Fault.InvalidValue(Label: typeof(TVO).Name, Requirement: refusal.Message, Key: Some(op))),
                _ => Fin.Fail<TVO>(error: op.InvalidResult()),
            };
    }
    [BoundaryAdapter]
    internal static Fin<TVO> AcceptValidated<TRaw, TVO>(Op op, TRaw candidate)
        where TRaw : struct, INumber<TRaw>
        where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
        Rekey(op: op, admitted: candidate.TryCreateValidated<TRaw, TVO>());
    private static Fin<TVO> Validated<TRaw, TVO>(Op op, TRaw? candidate)
        where TRaw : notnull
        where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
        (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
            (null, TVO owner) => Fin.Succ(value: owner),
            (ValidationError refusal, _) => Fin.Fail<TVO>(error: new Fault.InvalidValue(Label: typeof(TVO).Name, Requirement: refusal.Message, Key: Some(op))),
            _ => Fin.Fail<TVO>(error: op.InvalidResult()),
        };
    private static Fin<TVO> Rekey<TVO>(Op op, Validation<Error, TVO> admitted) =>
        admitted.ToFin().MapFail(error => error is Fault.OutOfRange range ? range with { Key = Some(op) } : error);
}
```

## [05]-[PAIR_COMBINATOR]

- Owner: `RequirementContext` internal static — one `extension(Context)` block carrying `Pair<TA, TB>`, the two-operand kind-resolve-then-validate combinator every pairwise operation composes receiver-style (`context.Pair(a, b, op, requirements)`).
- Auto: `Pair` returns `(A, B, KindA, KindB)`, so the operation dispatches on the resolved kinds without re-deriving them.
- Law: pairwise readiness is policy-driven — the `requirements` delegate is the caller's policy row — and the combinator owns the resolve-then-validate order so no pair operation re-spells it.
- Boundary: consumers are `Analysis/measure.md` conformance pairs and `Analysis/relations.md` intersection, classification, and deviation pairs; `Kind` and `KindOf` are `normalization.md`'s.

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

- Owner: `Admit` internal static owns every input guard above a scalar; `Op`'s `Finite`/`Positive` own the per-scalar key-bound form (`rails.md`).
- Law: predicate policy has ONE owner — where a `ValidityClaim` row states the predicate (`rails.md`), the gate composes the row, and `Admit` spells only what no claim states. `HermitianDiagonalReal` derives its tolerance from the diagonal's own scale, never an absolute literal.
- Law: a module spells its input gate as one `Admit` composition at its boundary, and a new admission shape is one member here composed everywhere.
- Boundary: `Numerics`, `Spatial`, and `Meshing` owners compose these gates at their boundaries; their value objects admit through the `[04]` bridge, and the internal span loops are the named kernel exemption.

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

One owner per concern, each extended by a row.

| [INDEX] | [CONCERN]            | [OWNER]               | [KIND]                       | [RAIL]                               |
| :-----: | :------------------- | :-------------------- | :--------------------------- | :----------------------------------- |
|  [01]   | Readiness rows       | `Requirement`         | record + `Seq<Check>` monoid | `Validation<Error, T>`               |
|  [02]   | Check matrix         | `Requirement.Check`   | smart-enum delegate rows     | `Fin<Unit>`                          |
|  [03]   | Validity oracle      | `OpAcceptance`        | frozen table + evidence arm  | `Fin<T>` / `Option<bool>`            |
|  [04]   | Factory bridge       | `OpAcceptance`        | generic generated admission  | `Validation<Error, TVO>` / `Fin`     |
|  [05]   | Admission projection | `AdmissionProjection` | sealed bidirectional owner   | `Render` / `Admit → Fin<T>`          |
|  [06]   | Pair readiness       | `RequirementContext`  | context extension combinator | `Validation<Error, (A,B,Kind,Kind)>` |
|  [07]   | Admission vocabulary | `Admit`               | guard vocabulary             | `Fin<Unit>` / `Fin<T>`               |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
