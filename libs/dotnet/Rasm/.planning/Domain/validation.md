# [RASM_VALIDATION]

`Rasm.Domain` validation owns the kernel's one acceptance, readiness, capability-vocabulary, and admission-projection authority; every value proving its validity routes through it. `OpAcceptance.ValidityOf` is the corpus's single validity oracle, and `CapabilitySet<TCapability>` the single combinable-capability column every stratum instantiates.

Validation composes `normalization.md`'s `Kind` roster, `Capability` rows, and `GeometryForm` lease recovery for topology-driven readiness dispatch, and `results.md`'s `Fault` family, type-not-reference evidence law, `ValidityClaim` predicate rows, and the `Op.Demand` scalar guard for every verdict. Thinktecture factory validation and LanguageExt's `Validation` applicative fold carry generated admission and independent-fault accumulation.

## [01]-[INDEX]

- [02]-[READINESS_ALGEBRA]: `Requirement` + `Check` — the composable readiness matrix, `ForKind` topology dispatch, lease-aware execution.
- [03]-[ACCEPTANCE_ORACLE]: `OpAcceptance` — the single validity oracle and result-acceptance gate, `IValidityEvidence` registration law.
- [04]-[FACTORY_BRIDGE]: `OpExtensions` + `AdmissionProjection` — generated factory admission, optional-key resolution, bidirectional projection.
- [05]-[ADMISSION_SLOTS]: `AdmissionSlots` — the reusable applicative slot fold, scalar-band gates, and recursive accumulated-error reader.
- [06]-[PAIR_COMBINATOR]: `RequirementContext.Pair` — the two-operand kind-resolve-then-validate combinator.
- [07]-[CAPABILITY]: `ICapability` + `CapabilitySet` + `CapabilityLaw` — the one combinable-capability column and its legal-corner admission.
- [08]-[VERDICT_CARRIERS]: `Quality` + `Masked` + `Evidence<T>` — the foreign-measurement quality verdict, the changed-or-not transform verdict, and the three-state probe verdict.
- [09]-[ADMISSION_VOCABULARY]: `Admit` — the shape and collection input-guard vocabulary above a scalar.
- [10]-[DENSITY_BAR]: one owner per concern.

## [02]-[READINESS_ALGEBRA]

- Owner: `Requirement` folds a delegate-backed `Check` matrix into readiness rows — readiness is data, a requirement is a set of check rows, never a method family. `Set<Check>` stores the rows, so `+` is set union under the row key's ordinal comparer and no fold re-scans for duplicates.
- Entry: `Apply<T>` is the ONE readiness gate; an empty requirement admits straight through the oracle as input, so a readiness rejection is `KernelFault.InvalidInput`, never a result fault. `ForKind` dispatches topology to requirement through the exhaustive generated `Topology.Map`, so a new `Topology` row breaks dispatch loudly at compile time and no caller hand-picks rows, and `Continuous` widens any dispatched requirement with the derivative-grade continuity row.
- Auto: `RunChecks` folds every row applicatively over one `Validation` result, so independent failures accumulate into one verdict and each row self-skips through its `Applies` column. `UsableBounds` passes any box computing short of full invalidity (`IsDegenerate < 4`), so flat and point geometry clears the readiness floor. Non-`GeometryBase` values run lease-aware through one gate: `Capability.Form` admits them, `GeometryForm` leases the native, the checks run inside the lease, and owned conversions dispose on exit.
- Law: every check failure carries the value's `Type`, never the live reference (`results.md` evidence law), and a direct cancellation poll pre-empts every row as `Errors.Cancelled`. `Demand` is the one verdict constructor, `MeshReport`'s lazy guard the named exemption where its `TextLog` materializes only on failure.
- Law: `Check` rows are a closed, row-owned matrix — a new readiness concern is one row and its membership in the requirements that need it, never a call-site validator and never a whole-matrix requirement standing in for the memberships a row is owed, which enrols every later row silently and leaves the concern reachable only through a constant nothing dispatches to.
- Law: readiness stays validation's — a `Requirement` column on `Topology` seats a readiness policy inside the taxonomy, and the generated `Topology.Map` already proves total coverage at compile time, so `ForKind` is the dispatch and the roster is not mirrored.
- Exemption: `MeshReport` and `CurveSelfIntersectionReport` are the statement forms; `Analysis/inspect.md` composes `MeshReport` for its defect surface.
- Packages: Thinktecture.Runtime.Extensions and LanguageExt.Core drive the smart-enum delegate rows and the applicative fold; RhinoCommon carries the check-matrix members.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed partial record Requirement {
    private static readonly Op Operand = Op.Of(name: nameof(Operand));
    private readonly Set<Check> checks;
    private Requirement(Set<Check> checks) => this.checks = checks;
    internal bool IsEmpty => checks.IsEmpty;
    private static Requirement Single(Check check) => new(checks: Set(check));
    public static readonly Requirement None = new(checks: Set<Check>());
    public static readonly Requirement Basic = new(checks: Set(Check.Validity, Check.UsableBounds));
    public static readonly Requirement CurveLength = Basic + Single(check: Check.CurveLengthReadiness) + Single(check: Check.PolycurveStructure);
    public static readonly Requirement AreaMass = Basic + Single(check: Check.CurveAreaReadiness) + Single(check: Check.CurveSelfIntersection) + Single(check: Check.PolycurveStructure);
    public static readonly Requirement MeshCheck = Basic + Single(check: Check.MeshRhinoCheck);
    public static readonly Requirement SolidTopology = Basic + Single(check: Check.BrepIntegrity) + Single(check: Check.MeshManifoldReadiness) + Single(check: Check.BrepSolidReadiness) + Single(check: Check.MeshRhinoCheck);
    public static readonly Requirement VolumeMass = SolidTopology + Single(check: Check.SurfaceSolidReadiness);
    public static readonly Requirement SurfaceEvaluation = Basic + Single(check: Check.SurfaceDomainReadiness);
    public static Requirement operator +(Requirement left, Requirement right) => Add(left: left, right: right);
    public static Requirement Add(Requirement left, Requirement right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(checks: left.checks.Union(right.checks));
    }
    public Requirement Continuous => this + Single(check: Check.ContinuityReadiness);
    public Validation<Error, T> Apply<T>(Context? context, T? value, CancellationToken cancel = default) where T : notnull =>
        (value, context, this) switch {
            (null, _, _) => Fin.Fail<T>(error: new KernelFault.MissingGeometry()).ToValidation(),
            (T candidate, _, Requirement { IsEmpty: true }) => Operand.AcceptInput(value: candidate).ToValidation(),
            (T candidate, Context ctx, Requirement req) => RunChecks(checks: req.checks, context: ctx, original: candidate, cancel: cancel),
            _ => Fin.Fail<T>(error: new KernelFault.MissingContext(Key: Operand)).ToValidation(),
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
    internal static Fin<MeshCheckParameters> MeshReport(Mesh mesh, string check) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return guard(mesh.Check(textLog: textLog, parameters: ref parameters), () => (Error)new KernelFault.InvalidGeometry(Shape: mesh.GetType(), Check: check, Log: textLog.ToString()))
            .ToFin()
            .Map(_ => parameters);
    }
    private static Validation<Error, T> RunChecks<T>(Set<Check> checks, Context context, T original, CancellationToken cancel) where T : notnull =>
        original switch {
            GeometryBase geometry => RunChecks(checks: checks, context: context, geometry: geometry, original: original, cancel: cancel),
            object value when Capability.Form.Admits(type: value.GetType()) =>
                RunLeaseChecks(lease: value.GeometryForm(key: Operand), checks: checks, context: context, original: original, cancel: cancel),
            _ => Operand.AcceptInput(value: original).ToValidation(),
        };
    private static Validation<Error, T> RunChecks<T>(Set<Check> checks, Context context, GeometryBase geometry, T original, CancellationToken cancel) where T : notnull =>
        toSeq(checks)
            .Traverse(check => check.Apply(context: context, geometry: geometry, cancel: cancel).ToValidation())
            .As()
            .Map(_ => original);
    private static Validation<Error, T> RunLeaseChecks<T>(Fin<Lease<GeometryBase>> lease, Set<Check> checks, Context context, T original, CancellationToken cancel)
        where T : notnull =>
        lease.ToValidation()
            .Bind(native => native.Use(geometry => RunChecks(checks: checks, context: context, geometry: geometry, original: original, cancel: cancel)));
    private static Fin<Unit> CurveSelfIntersectionReport(Check check, Curve curve, double tolerance) {
        using CurveIntersections? hits = Intersection.CurveSelf(curve: curve, tolerance: tolerance);
        return Optional(hits).Match(
            Some: found => check.Demand(
                geometry: curve,
                condition: found.Count == 0,
                log: found.Count == 0 ? string.Empty : string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {found.Count} curve self-intersection event(s).")),
            None: () => check.Demand(geometry: curve, condition: false, log: "Rhino curve self-intersection computation failed."));
    }
    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
    private sealed partial class Check {
        public static readonly Check Validity = new(key: "rhino-validity", applies: static _ => true, run: static (check, _, g) => check.Demand(geometry: g, condition: g.IsValidWithLog(log: out string log), log: log));
        public static readonly Check UsableBounds = new(key: "usable-bounds", applies: static _ => true, run: static (check, ctx, g) => check.Demand(geometry: g, condition: g.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: ctx.Absolute.Value) < 4, log: "Rhino could not compute a usable accurate bounding box."));
        public static readonly Check BrepIntegrity = new(key: "brep-integrity", applies: static g => g is Brep, run: static (check, _, g) => ((Brep)g) switch {
            Brep b => (b.IsValidTopology(log: out string tLog), b.IsValidGeometry(log: out string gLog), b.IsValidTolerancesAndFlags(log: out string toLog)) switch {
                (false, _, _) => check.Demand(geometry: b, condition: false, log: $"Brep topology: {tLog}"),
                (_, false, _) => check.Demand(geometry: b, condition: false, log: $"Brep geometry: {gLog}"),
                (_, _, false) => check.Demand(geometry: b, condition: false, log: $"Brep tolerances and flags: {toLog}"),
                _ => check.Demand(geometry: b, condition: true, log: string.Empty),
            },
        });
        public static readonly Check MeshRhinoCheck = new(key: "mesh-rhino-check", applies: static g => g is Mesh, run: static (check, _, g) => MeshReport(mesh: (Mesh)g, check: check.Key).Map(static _ => unit));
        public static readonly Check MeshManifoldReadiness = new(key: "mesh-manifold-readiness", applies: static g => g is Mesh, run: static (check, _, g) => check.Demand(geometry: g, condition: ((Mesh)g).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
        public static readonly Check BrepSolidReadiness = new(key: "brep-solid-readiness", applies: static g => g is Brep, run: static (check, _, g) => check.Demand(geometry: g, condition: ((Brep)g).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check SurfaceSolidReadiness = new(key: "surface-solid-readiness", applies: static g => g is Surface, run: static (check, _, g) => check.Demand(geometry: g, condition: ((Surface)g).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check CurveLengthReadiness = new(key: "curve-length-readiness", applies: static g => g is Curve, run: static (check, ctx, g) =>
            check.Demand(geometry: g, condition: !((Curve)g).IsShort(tolerance: ctx.Absolute.Value) && ((Curve)g).GetLength(fractionalTolerance: ctx.Fractional) > ctx.Absolute.Value, log: "Curve is valid Rhino geometry but is below model-length tolerance."));
        public static readonly Check CurveAreaReadiness = new(key: "curve-area-readiness", applies: static g => g is Curve, run: static (check, ctx, g) =>
            check.Demand(geometry: g, condition: ((Curve)g).IsClosed && ((Curve)g).TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value), log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."));
        public static readonly Check SurfaceDomainReadiness = new(key: "surface-domain-readiness", applies: static g => g is Surface, run: static (check, ctx, g) => check.Demand(geometry: g, condition: HasUsableDomain(surface: (Surface)g, context: ctx), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
        public static readonly Check ContinuityReadiness = new(key: "continuity-readiness", applies: static g => g is Curve or Surface, run: static (check, ctx, g) => g switch {
            Surface surface => check.Demand(
                geometry: surface,
                condition: HasUsableDomain(surface: surface, context: ctx)
                    && !surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _)
                    && !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _),
                log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
            Curve curve => check.Demand(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
        });
        public static readonly Check PolycurveStructure = new(key: "polycurve-structure", applies: static g => g is PolyCurve, run: static (check, _, g) => check.Demand(geometry: g, condition: !((PolyCurve)g).HasGap, log: "PolyCurve has gaps between segments."));
        public static readonly Check CurveSelfIntersection = new(key: "curve-self-intersection", applies: static g => g is Curve, run: static (check, ctx, g) => CurveSelfIntersectionReport(check: check, curve: (Curve)g, tolerance: ctx.Absolute.Value));
        [UseDelegateFromConstructor]
        private partial bool Applies(GeometryBase geometry);
        [UseDelegateFromConstructor]
        private partial Fin<Unit> Run(Check check, Context context, GeometryBase geometry);
        internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) =>
            condition switch {
                true => Fin.Succ(unit),
                false => Fin.Fail<Unit>(error: new KernelFault.InvalidGeometry(Shape: geometry.GetType(), Check: Key, Log: log)),
            };
        internal Fin<Unit> Apply(Context context, GeometryBase geometry, CancellationToken cancel) =>
            cancel.IsCancellationRequested switch {
                true => Fin.Fail<Unit>(error: Errors.Cancelled),
                false => Applies(geometry: geometry) ? Run(check: this, context: context, geometry: geometry) : Fin.Succ(unit),
            };
    }
    private static bool HasUsableDomain(Surface surface, Context context) =>
        Evaluation.SurfaceDomain(surface: surface, context: context).IsSome;
}
```

## [03]-[ACCEPTANCE_ORACLE]

- Owner: `OpAcceptance` internal static is the validity oracle and result-acceptance gate; `Op` fronts it publicly and `Analysis/query.md` routes it directly. Its name is frozen, keyed by the repository analyzer's docID.
- Entry: `AcceptValue`/`AcceptInput`/`Accept`/`AcceptResults` gate one value, re-label the rejection, lift into `Seq`, and bridge a same-type sequence; heterogeneous raw-to-typed projection is `Numerics/atoms.md`'s `ProjectionRow`, never a `typeof` ladder here. `OutputBinding` is the RUNTIME-typed sibling of `AcceptResults` — a roster row declaring its published output as a `Type` column carries the test and the unbox on one value, so no consumer re-spells `typeof(TOut) == Output`.
- Law: `ValidityOf(object?)` is the single validity authority — it instruments only foreign material it cannot reach otherwise (Rhino geometry, host scalars screened against the unset sentinel, the Rhino value shapes) and routes every kernel-owned result through one `IValidityEvidence` arm.
- Law: a kernel type reaches the oracle by implementing `IValidityEvidence` with a `ValidityClaim.All` fold (`results.md`), never by adding an oracle arm; that arm is probed AHEAD of every category default, so a result also inhabiting a blanket-admitted category answers through its own fold rather than the category, and an unknown type is rejected by `AcceptValue` — admitting a new result type is exactly one interface implementation.
- Law: the value-shape table has ONE authority — every Rhino shape carrying a `Kind` row derives from `Kind.Items`, and the residual roster names only the value shapes no geometry kind claims. Shapes added to `Kind` reach the oracle with no edit here.
- Boundary: `OpAcceptance` is internal; the oracle never crosses the package boundary, and the assembly-public gates are `Op`'s acceptance members and the readiness algebra.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using System.Linq.Expressions;
using Rhino;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct OutputBinding(Type Declared) {
    public static OutputBinding Of<TDeclared>() => new(Declared: typeof(TDeclared));
    public bool Serves<TOut>() => typeof(TOut) == Declared;
    internal Fin<Seq<TOut>> Admit<TOut>(Seq<object> values, Op key) =>
        typeof(TOut) == Declared
            ? values.TraverseM(value => value is TOut projected ? key.AcceptValue(value: projected) : Fin.Fail<TOut>(key.InvalidResult())).As()
            : Fin.Fail<Seq<TOut>>(key.Unsupported(inputType: Declared, outputType: typeof(TOut)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class OpAcceptance {
    private static readonly Lazy<FrozenDictionary<Type, Func<object, bool>>> ValueValidity = new(static () =>
        Kind.Items
            .Select(static row => row.Type)
            .Where(static type => !typeof(GeometryBase).IsAssignableFrom(c: type))
            .Concat(ValueShapes)
            .Distinct()
            .ToFrozenDictionary(static type => type, static type => {
                ParameterExpression parameter = Expression.Parameter(type: typeof(object));
                return Expression.Lambda<Func<object, bool>>(Expression.Property(Expression.Convert(parameter, type), propertyName: nameof(IValidityEvidence.IsValid)), parameter).Compile();
            }));
    private static readonly FrozenSet<Type> ValueShapes = new[] {
        typeof(Point2d), typeof(Vector3d), typeof(Transform), typeof(Rectangle3d), typeof(Interval),
    }.ToFrozenSet();
    extension(Op key) {
        internal Fin<Seq<TValue>> Accept<TValue>(TValue value) =>
            key.AcceptValue(value: value).Map(static candidate => Seq(candidate));
        public Fin<Seq<TValue>> Accept<TValue>(params ReadOnlySpan<TValue> values) => key.Accept(values: Iterable<TValue>.FromSpan(values).ToSeq());
        internal Fin<Seq<TValue>> Accept<TValue>(IEnumerable<TValue> values) =>
            Optional(values).ToFin(key.InvalidResult()).Bind(candidates => candidates.AsIterable().ToSeq().Traverse(value => key.AcceptValue(value: value)).As());
        internal Fin<Seq<TOut>> AcceptResults<TValue, TOut>(IEnumerable<TValue> values) => typeof(TValue).Equals(typeof(TOut)) switch {
            true => key.Accept(values: values).Bind(candidates =>
                candidates.TraverseM(candidate => candidate is TOut projected
                    ? Fin.Succ(projected)
                    : Fin.Fail<TOut>(key.InvalidResult())).As()),
            false => Fin.Fail<Seq<TOut>>(key.Unsupported(inputType: typeof(TValue), outputType: typeof(TOut))),
        };
        internal Fin<T> AcceptInput<T>(T value) =>
            key.AcceptValue(value: value).MapFail(_ => key.InvalidInput());
        internal Fin<T> AcceptValue<T>(T value) =>
            value switch {
                null => Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
                Enum => Fin.Succ(value),
                _ => ValidityOf(source: value).Case switch {
                    bool ok => key.Demand(condition: ok, value: value),
                    _ => Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key)),
                },
            };
        private Fin<T> Demand<T>(bool condition, T value) =>
            condition ? Fin.Succ(value) : Fin.Fail<T>(error: new KernelFault.InvalidResult(Key: key));
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
            IValidityEvidence evidence => Some(evidence.IsValid),
            bool or int or Enum or SurfaceCurvature or MeshCheckParameters or ISmartEnum<int> or ISmartEnum<string> => Some(value: true),
            MeshPoint m => Some(m.Point.IsValid && m.FaceIndex >= 0 && m.ComponentIndex is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 } && m.T.All(static t => RhinoMath.IsValidDouble(t))),
            ComponentIndex c => Some(c is { ComponentIndexType: not ComponentIndexType.InvalidType } ci && ci.Index >= 0),
            ValueTuple<double, double> t => Some(t is (double x, double y) && RhinoMath.IsValidDouble(x) && RhinoMath.IsValidDouble(y)),
            ValueTuple<double, Vector3d> t => Some(t is (double m, Vector3d a) && RhinoMath.IsValidDouble(m) && m >= 0.0 && a.IsValid && Band.Positive.Admits(value: a.Length)),
            _ => ValueValidity.Value.GetValueOrDefault(source.GetType()) is Func<object, bool> fn ? Some(fn(source)) : Option<bool>.None,
        };
}
```

## [04]-[FACTORY_BRIDGE]

- Owner: `OpExtensions` carries `OrDefault`, the optional-key resolver of the `results.md` threading law, and the `AcceptValidated<TVO>` key-shaped receivers.
- Owner: `AdmissionProjection<TRaw, TModel>` holds one admitted `Op`, a model-to-raw render delegate, and a `Fin`-gated raw-to-model admit delegate; `Render` and `Admit` run through the held key's `Catch` funnel, and `SmartEnum`'s false or nullable lookup lands a typed refusal there.
- Law: ONE `Validate` body serves every admission tier — the refusal is a policy value the caller hands in, so the numeric tier lands a `KernelFault.OutOfRange` carrying the saturated scalar and the general tier a `KernelFault.InvalidValue`, and both stamp the demanding `Op` at construction rather than rewriting a fault after the fact.
- Law: one generic-math body over `TRaw : struct, INumber<TRaw>` admits every numeric width; `Validate` runs under `CultureInfo.InvariantCulture`.
- Law: `AcceptValidated` spans two admission tiers and one type-erased outcome lifter, selected by input shape; the lifter exists because multi-member `[ComplexValueObject]` admission has no static `Validate` contract spanning its arities, so the caller spells `Validate` and this row owns the lift.
- Law: `OrDefault` resolves a null `Op` to `Op.Of(callerMemberName)`, so public polymorphic surfaces stay knob-free while internal kernels demand the key.
- Exemption: `AdmissionProjection` carries `render`/`admit` as runtime delegates and refuses the Mapperly `[Mapper]` rung — there is no member-to-member DTO correspondence to generate, only two opaque total functions between a raw carrier and a generated owner that already proves its own admission; a `[Mapper]` here generates nothing and proves nothing.
- Boundary: `AdmissionProjection` owns pure render/admit conversion; refusal posture, held state, fallback, and presentation compose after its `Fin`, and every bidirectional boundary composes it rather than re-minting generated validation.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
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
    public static Fin<AdmissionProjection<TRaw, TModel>> Of(
        Func<TModel, TRaw>? render,
        Func<TRaw, Fin<TModel>>? admit,
        Op? key = null) {
        Op op = key.OrDefault();
        return from renderArm in Optional(render).ToFin(op.InvalidInput())
               from admitArm in Optional(admit).ToFin(op.InvalidInput())
               select new AdmissionProjection<TRaw, TModel>(operation: op, render: renderArm, admit: admitArm);
    }
    public Fin<TRaw> Render(TModel model) =>
        operation.Catch(body: () => Optional(render(arg: model)).ToFin(operation.InvalidResult()));
    public Fin<TModel> Admit(TRaw raw) =>
        operation.Catch(body: () => Optional(admit(arg: raw)).ToFin(operation.InvalidResult()).Bind(static fin => fin));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AdmissionProjection {
    public delegate bool SmartEnumLookup<TRaw, TModel>(TRaw? key, out TModel? item)
        where TRaw : notnull
        where TModel : class, ISmartEnum<TRaw>;

    public static Fin<AdmissionProjection<TRaw, TModel>> Generated<TRaw, TModel>(Op? key = null)
        where TRaw : struct, INumber<TRaw>
        where TModel : notnull, IObjectFactory<TModel, TRaw, ValidationError>, IConvertible<TRaw> {
        Op op = key.OrDefault();
        return AdmissionProjection<TRaw, TModel>.Of(
            render: static model => model.ToValue(),
            admit: raw => OpExtensions.AcceptValidated<TRaw, TModel>(op: op, candidate: raw),
            key: op);
    }

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

public readonly record struct ValidationClause(string Requirement);

public static class FactoryValidation {
    public static Seq<ValidationClause> Violated(params ReadOnlySpan<(bool Broken, Func<ValidationClause> Mint)> clauses) =>
        toSeq(clauses.ToArray()).Choose(static clause => clause.Broken ? Some(clause.Mint()) : None);

    public static ValidationError? Of(Seq<ValidationClause> clauses) => clauses.IsEmpty
        ? null
        : new ValidationError(string.Join(" | ", clauses.Map(static clause => clause.Requirement)));

    public static Fin<Unit> Admit(Seq<ValidationClause> clauses) => clauses.IsEmpty
        ? Fin.Succ(unit)
        : Fin.Fail<Unit>(new KernelFault.InvalidValue("factory", string.Join(" | ", clauses.Map(static clause => clause.Requirement))));
}

public static class OpExtensions {
    extension(Op? key) {
        public Op OrDefault([CallerMemberName] string name = "") => key ?? Op.Of(name: name);
    }
    extension(Op op) {
        public Fin<TVO> AcceptValidated<TVO>(double candidate) where TVO : IObjectFactory<TVO, double, ValidationError> =>
            AcceptValidated<double, TVO>(op: op, candidate: candidate);
        public Fin<TVO> AcceptValidated<TVO>(int candidate) where TVO : IObjectFactory<TVO, int, ValidationError> =>
            AcceptValidated<int, TVO>(op: op, candidate: candidate);
        public Fin<TVO> AcceptValidated<TVO>(uint candidate) where TVO : IObjectFactory<TVO, uint, ValidationError> =>
            AcceptValidated<uint, TVO>(op: op, candidate: candidate);
        public Fin<TVO> AcceptValidated<TVO>(string? candidate) where TVO : IObjectFactory<TVO, string, ValidationError> =>
            Admitted<string, TVO>(op: op, candidate: candidate, refuse: refusal => InvalidValueOf<TVO>(op: op, refusal: refusal));
        public Fin<TVO> AcceptValidated<TVO>(bool candidate) where TVO : IObjectFactory<TVO, bool, ValidationError> =>
            Admitted<bool, TVO>(op: op, candidate: candidate, refuse: refusal => InvalidValueOf<TVO>(op: op, refusal: refusal));
        public Fin<TVO> AcceptValidated<TVO>(Guid candidate) where TVO : IObjectFactory<TVO, Guid, ValidationError> =>
            Admitted<Guid, TVO>(op: op, candidate: candidate, refuse: refusal => InvalidValueOf<TVO>(op: op, refusal: refusal));
        public Fin<TVO> AcceptValidated<TVO, TRaw>(TRaw? candidate)
            where TRaw : notnull
            where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
            Admitted<TRaw, TVO>(op: op, candidate: candidate, refuse: refusal => InvalidValueOf<TVO>(op: op, refusal: refusal));
        public Fin<TRow> Row<TKey, TRow>(TKey candidate) where TRow : class, ISmartEnum<TKey, TRow, ValidationError> =>
            TRow.TryGet(candidate, out TRow? row)
                ? Fin.Succ(value: row)
                : Fin.Fail<TRow>(error: op.InvalidResult(detail: $"{typeof(TRow).Name} {candidate}"));
        public Fin<TRow> Row<TKey, TRow>(TKey candidate, Func<TRow, TKey> column) where TRow : class, ISmartEnum<TKey, TRow, ValidationError> =>
            toSeq(TRow.Items).Find(row => EqualityComparer<TKey>.Default.Equals(column(arg: row), candidate))
                .ToFin(op.InvalidResult(detail: $"{typeof(TRow).Name} column {candidate}"));
        public Fin<TRow> Row<TColumn, TKey, TRow>(TColumn candidate, Func<TRow, TColumn> column, Option<IEqualityComparer<TColumn>> match = default)
            where TRow : class, ISmartEnum<TKey, TRow, ValidationError> =>
            toSeq(TRow.Items).Find(row => match.IfNone(EqualityComparer<TColumn>.Default).Equals(column(arg: row), candidate))
                .ToFin(op.InvalidResult(detail: $"{typeof(TRow).Name} column {candidate}"));
        public Fin<TRow> Row<THostEnum, TRow>(THostEnum candidate, Func<THostEnum, int> ordinal)
            where THostEnum : struct, Enum
            where TRow : class, ISmartEnum<int, TRow, ValidationError> =>
            Enum.IsDefined(candidate) ? op.Row<int, TRow>(ordinal(arg: candidate)) : Fin.Fail<TRow>(error: op.InvalidResult(detail: $"{typeof(THostEnum).Name} {candidate}"));
        public Fin<TVO> AcceptValidated<TVO>(ValidationError? fault, object? admitted) where TVO : notnull =>
            (fault, admitted) switch {
                (null, TVO owner) => Fin.Succ(value: owner),
                (ValidationError refusal, _) => Fin.Fail<TVO>(error: InvalidValueOf<TVO>(op: op, refusal: refusal)),
                _ => Fin.Fail<TVO>(error: op.InvalidResult()),
            };
        public Fin<TVO> AcceptValidated<TVO>(ValidationError? fault, TVO? admitted) where TVO : class =>
            (fault, admitted) switch {
                (null, TVO owner) => Fin.Succ(value: owner),
                (ValidationError refusal, _) => Fin.Fail<TVO>(error: InvalidValueOf<TVO>(op: op, refusal: refusal)),
                _ => Fin.Fail<TVO>(error: op.InvalidResult()),
            };
    }
    internal static Fin<TVO> AcceptValidated<TRaw, TVO>(Op op, TRaw candidate)
        where TRaw : struct, INumber<TRaw>
        where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
        Admitted<TRaw, TVO>(
            op: op,
            candidate: candidate,
            refuse: refusal => new KernelFault.OutOfRange(Label: typeof(TVO).Name, Scalar: double.CreateSaturating(candidate), Requirement: refusal.Message, Key: Some(op)));
    internal static Error InvalidValueOf<TVO>(Op op, ValidationError refusal) =>
        new KernelFault.InvalidValue(Label: typeof(TVO).Name, Requirement: refusal.Message, Key: Some(op));
    internal static Fin<TVO> Admitted<TRaw, TVO>(Op op, TRaw? candidate, Func<ValidationError, Error> refuse)
        where TRaw : notnull
        where TVO : IObjectFactory<TVO, TRaw, ValidationError> =>
        (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
            (null, TVO owner) => Fin.Succ(value: owner),
            (ValidationError refusal, _) => Fin.Fail<TVO>(error: refuse(arg: refusal)),
            _ => Fin.Fail<TVO>(error: op.InvalidResult()),
        };
}
```

## [05]-[ADMISSION_SLOTS]

- Owner: `AdmissionSlots` is the kernel's one reusable applicative admission fold. It accumulates independent `Validation<Error, Unit>` slots, mints universal scalar and representation refusals as `KernelFault`, and defers package-semantic refusal construction to the package's typed family.
- Entry: `Gate` lifts an already-owned refusal or invokes a typed package minter only on failure; `Accumulate` joins concrete and `K`-typed runs; `Indexed`, `In`, `InRange`, `Optional`, `Finite`, and `Bounded` cover universal scalar/shape admission; `Unpack` recursively flattens `ManyErrors` membership.
- Law: no package re-cases finite, range, or bounded-interval refusals into its local fault family. A package-semantic rule supplies its typed fault through `Gate`; the kernel never accepts a detail string from which it would invent package meaning.
- Packages: LanguageExt.Core (`Validation<Error,_>`, `K<F,A>`, `ManyErrors`), NodaTime (`Interval`), and the kernel `Band` vocabulary.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Numerics;

namespace Rasm.Domain;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AdmissionSlots {
    public static Validation<Error, Unit> Gate(bool holds, Error refusal) =>
        holds ? unit : refusal;

    public static Validation<Error, Unit> Gate<TConcern, TDetail>(
        bool holds,
        TConcern concern,
        TDetail detail,
        Func<TConcern, TDetail, Error> refuse) =>
        holds ? unit : refuse(concern, detail);

    public static Validation<Error, Unit> Accumulate(Seq<Validation<Error, Unit>> slots) =>
        slots.Traverse(identity).As().Map(static _ => unit);

    public static Validation<Error, Unit> Accumulate(Seq<K<Validation<Error>, Unit>> slots) =>
        slots.Traverse(identity).As().Map(static _ => unit);

    public static Validation<Error, Unit> Indexed(
        ReadOnlySpan<double> values,
        Func<double, bool> holds,
        Op key,
        string label) {
        Validation<Error, Unit> scan = Success<Error, Unit>(unit);
        for (int index = 0; index < values.Length; index++) {
            if (holds(values[index])) { continue; }
            Validation<Error, Unit> miss = new KernelFault.OutOfRange(
                Label: $"{label}[{index}]",
                Scalar: values[index],
                Requirement: "satisfy the declared scalar predicate",
                Key: Some(key));
            scan = (scan, miss).Apply(static (_, _) => unit).As();
        }
        return scan;
    }

    public static Validation<Error, double> In(double value, Band band, string label, Op key) =>
        band.Admits(value)
            ? value
            : new KernelFault.OutOfRange(label, value, $"fall inside {band.Key}", Some(key));

    public static Validation<Error, double> InRange(
        double value,
        double floor,
        double ceiling,
        string label,
        Op key) =>
        value >= floor && value <= ceiling
            ? value
            : new KernelFault.OutOfRange(
                label,
                value,
                string.Create(CultureInfo.InvariantCulture, $"fall inside [{floor:R},{ceiling:R}]"),
                Some(key));

    public static Validation<Error, Option<double>> Optional(
        Option<double> value,
        Band band,
        string label,
        Op key) =>
        value.TraverseM(scalar => In(scalar, band, label, key)).As();

    public static Validation<Error, Unit> Finite(
        Op key,
        params ReadOnlySpan<(string Label, double Value)> ordinates) {
        Validation<Error, Unit> scan = Success<Error, Unit>(unit);
        foreach ((string label, double value) in ordinates) {
            if (Band.Parameter.Admits(value)) { continue; }
            Validation<Error, Unit> miss = new KernelFault.OutOfRange(
                Label: label,
                Scalar: value,
                Requirement: "be finite",
                Key: Some(key));
            scan = (scan, miss).Apply(static (_, _) => unit).As();
        }
        return scan;
    }

    public static Validation<Error, Interval> Bounded(Interval window, Op key) =>
        window is { HasStart: true, HasEnd: true }
            ? window
            : new KernelFault.InvalidValue(
                Label: nameof(Interval),
                Requirement: "bounded start and end",
                Key: Some(key));

    public static Seq<Error> Unpack(Error fault) =>
        fault is ManyErrors many
            ? many.Errors.Bind(static member => Unpack(member)).Strict()
            : Seq(fault);
}
```

## [06]-[PAIR_COMBINATOR]

- Owner: `RequirementContext` internal static — one `extension(Context)` block carrying `Pair<TA, TB>`, the two-operand kind-resolve-then-validate combinator every pairwise operation composes receiver-style (`context.Pair(a, b, op, requirements)`).
- Auto: `Pair` returns `(A, B, KindA, KindB)`, so the operation dispatches on the resolved kinds without re-deriving them.
- Law: pairwise readiness is policy-driven — the `requirements` delegate is the caller's policy row — and the combinator owns the resolve-then-validate order so no pair operation re-spells it.
- Boundary: consumers are `Analysis/measure.md` conformance pairs and `Analysis/relations.md` intersection, classification, and deviation pairs; `Kind` and `KindOf` are `normalization.md`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Domain;

// --- [OPERATIONS] ----------------------------------------------------------------------
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

## [07]-[CAPABILITY]

- Owner: `ICapability<TSelf>` is the capability-vocabulary floor — a closed roster whose rows carry a stable wire `Key`; `CapabilitySet<TCapability>` is the ONE combinable-capability column at every stratum, backed by `FrozenSet`; `CapabilityLaw<TCapability>` carries the legal-corner law a boolean product cannot state.
- Entry: `Of` mints a set from a span, `Admits(TCapability)` is the interior containment read, `Admits(string)` the boundary arm resolving an untrusted token against the vocabulary before any membership test, `AdmitsAll` the consumer-side requirement with `Missing` its evidence complement — the required rows this set lacks, so a refusing owner names WHICH capabilities failed instead of re-deriving the diff — `Require(demanded, refuse)` the ONE refusal door folding demand + evidence + typed mint, in two arms — the `Fin` result arm and the `Option<TFault>` typed-fault twin whose Some IS the refusal, serving `Seq<TFault>` clause accumulators and generated `TFault?` hook slots without an Error downcast (the refuse arm of both receives the missing set, so an evidence-free refusal is unspellable through either — and it stays the SUPERSET door alone: an exact-correspondence demand is `held == demanded` under the value equality this struct already carries, with `held.Missing(demanded)` and `demanded.Missing(held)` as its two evidence wires minting ONE fault, never a chained double-`Require` whose two refusals split the verdict), `With`/`Without` the set edits, and `CapabilityLaw.Admit` the construction-time corner gate.
- Law: two or more adjacent `bool` columns answering "what can this row do" are the deleted form — the shape is a boolean product where only a subset of the corners is legal, and the corner law is what a bool pair cannot carry. Genuinely independent bool axes with no legal-corner law STAY bool pairs and say so at the owner.
- Law: the NAMED LOSS is per-capability compile-time exhaustiveness. Renamed and retired vocabulary rows still break every reader, while NARROWING a producer's held set is a data edit no consumer's compile catches. It is bought back twice: `CapabilityLaw.Admit` refuses an illegal corner at construction, and every consumer owner states the set it needs as a value through `AdmitsAll(required)`, so a narrowed producer fails admission at its own boundary instead of mis-answering at a call site.
- Law: `Wire` is key-ordered under ordinal comparison, so a result, a filter, and a decoder render one set byte-identically and a digest over the column is stable without the container publishing an order.
- Law: a capability read's FAILURE POSTURE is the consumer boundary's own fact, never a column on the capability row — the same lane capability is lawfully refusal-shaped at one owner and fold-out-shaped at another. Refuse-at-admission takes `Require` (evidence mandatory); fold-out-for-absence is a bare `Admits` ternary answering the neutral; lawful absence versus an illegal empty corner is the LAW row's construction-time verdict (`None` a `Legal` row here, a `Forbidden` row there); and rank/restart semantics ride their own rows (`Retriability` on the fault, `RedrivePolicy` on the schedule, the delivery-ack union at the wire) — a `(FailureRank, RestartClass)` pair on `ICapability` is the refused form: zero corpus sites read policy off a capability member.
- Growth: a new capability is one vocabulary row and its membership on the subjects that hold it; a new legal corner is one `CapabilityLaw` row; neither touches a consumer.
- Boundary: `Admits(string)` is the only text-shaped arm and it resolves through the vocabulary index first, so text no row names can never match; every interior path is set containment with no string compare.
- Packages: BCL frozen collections carry the membership store and the vocabulary index; the vocabulary rows themselves are Thinktecture `[SmartEnum<string>]` owners at each instantiating page.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ICapability<TSelf> where TSelf : ICapability<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    string Key { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public readonly partial record struct CapabilitySet<TCapability>([property: UnorderedEquality] FrozenSet<TCapability> Held)
    where TCapability : notnull, ICapability<TCapability> {
    public static readonly CapabilitySet<TCapability> None = new(FrozenSet<TCapability>.Empty);
    public static CapabilitySet<TCapability> All => Whole.Value;
    private static readonly Lazy<CapabilitySet<TCapability>> Whole = new(static () => new(TCapability.Items.ToFrozenSet()));
    public static CapabilitySet<TCapability> Of(params ReadOnlySpan<TCapability> held) => new(held.ToArray().ToFrozenSet());
    public bool Admits(TCapability capability) => Held.Contains(capability);
    public bool Admits(string key) => Lookup.Value.TryGetValue(key, out TCapability? row) && Held.Contains(row);
    public bool AdmitsAll(CapabilitySet<TCapability> required) => required.Held.IsSubsetOf(Held);
    public CapabilitySet<TCapability> Missing(CapabilitySet<TCapability> required) => new(required.Held.Except(Held).ToFrozenSet());
    public Fin<CapabilitySet<TCapability>> Require(CapabilitySet<TCapability> demanded, Func<CapabilitySet<TCapability>, Error> refuse) =>
        AdmitsAll(demanded) ? Fin.Succ(this) : Fin.Fail<CapabilitySet<TCapability>>(refuse(Missing(demanded)));
    public CapabilitySet<TCapability> With(TCapability capability) => new(Held.Append(capability).ToFrozenSet());
    public CapabilitySet<TCapability> Without(TCapability capability) => new(Held.Where(row => !row.Equals(capability)).ToFrozenSet());
    public string Wire => string.Join(',', Held.OrderBy(static row => row.Key, StringComparer.Ordinal).Select(static row => row.Key));
    public static Fin<CapabilitySet<TCapability>> OfMask(int mask, Func<TCapability, int> bit, Op key) =>
        TCapability.Items.Aggregate(seed: 0, func: (known, row) => known | bit(arg: row)) is int rostered && (mask & ~rostered) == 0
            ? Fin.Succ(new CapabilitySet<TCapability>(TCapability.Items.Where(row => (mask & bit(arg: row)) != 0).ToFrozenSet()))
            : Fin.Fail<CapabilitySet<TCapability>>(key.InvalidInput());
    public int Mask(Func<TCapability, int> bit) => Held.Aggregate(seed: 0, func: (word, row) => word | bit(arg: row));
    private static readonly Lazy<FrozenDictionary<string, TCapability>> Lookup =
        new(static () => TCapability.Items.ToFrozenDictionary(static row => row.Key, StringComparer.Ordinal));
}

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record CapabilityLaw<TCapability>(Seq<CapabilitySet<TCapability>> Legal, Seq<CapabilitySet<TCapability>> Barred)
    where TCapability : notnull, ICapability<TCapability> {
    public static readonly CapabilityLaw<TCapability> Open = new(Seq<CapabilitySet<TCapability>>(), Seq<CapabilitySet<TCapability>>());
    public CapabilityLaw(Seq<CapabilitySet<TCapability>> Legal) : this(Legal: Legal, Barred: Seq<CapabilitySet<TCapability>>()) { }
    public static CapabilityLaw<TCapability> Forbidden(Seq<CapabilitySet<TCapability>> barred) =>
        new(Legal: Seq<CapabilitySet<TCapability>>(), Barred: barred);
    public Fin<CapabilitySet<TCapability>> Admit(CapabilitySet<TCapability> held) =>
        (Legal.IsEmpty || Legal.Exists(row => row.Held.SetEquals(held.Held)))
        && !Barred.Exists(row => row.Held.Count == 0 ? held.Held.Count == 0 : row.Held.IsSubsetOf(held.Held))
            ? Fin.Succ(held)
            : Fin.Fail<CapabilitySet<TCapability>>(new KernelFault.InvalidValue(Label: typeof(TCapability).Name, Requirement: $"an admitted capability set; got <{held.Wire}>"));
}
```

## [08]-[VERDICT_CARRIERS]

- Owner: `Quality` is the ONE three-state verdict on an externally-measured value — `Good`, `Uncertain(Symbol)`, `Bad(Symbol)` — and `Symbol` its admitted reason token; `Masked` is the ONE verdict for a transform that must report whether it changed its input — `Unchanged(Value)` or `Redacted(Value)` — so "did redaction touch this" is a case read, never a length compare or a `(string, bool)` tuple; `Evidence<T>` is the ONE three-state probe verdict on a measurement column — `Measured(T)` a probe ran and answered, `Refused(Error)` a probe ran and rejected carrying its own cause, `Absent` no probe ran — and `Evidence` its static mint folding a probe's `Fin<T>` outcome or a scan's `Option<T>` presence onto the cases.
- Entry: producers mint the case at the boundary where the foreign status is in hand (an OPC-UA `StatusCode`, a decode reason, a status-flag word, a parse refusal) and consumers discriminate on the generated total `Switch`; `Symbol.Validate` admits the reason token once; `Evidence.Of(Fin<T>)` admits a probe that ran (failure IS refusal), `Evidence.Of(Option<T>)` admits a presence scan (absence IS absent), and `evidence.Value()` is the one stated collapse onto `Option<T>` for a boundary column admitting only presence.
- Law: a measurement quality crushed to one `bool Good` erases WHICH degradation admitted — four independent protocol boundaries proved the loss — and a `0d`-sentinel fill on the not-good arm forges a reading; the value column beside a `Quality` rides `Option<T>` where `Bad` carries no reading.
- Law: `Masked` carries the VALUE on both arms so a consumer never re-derives change by comparing texts whose equality is not the question; the transform's verdict is authored where the transform ran.
- Law: `Evidence<T>` and `Quality` split on WHAT is graded — `Evidence` carries whether a measurement HAPPENED, the value riding inside `Measured` and a refusal carrying the probe's own `Error`, while `Quality` grades the trustworthiness a foreign status word declares on a PRESENT reading — so neither absorbs the other; an `Option<T>` measurement column that lets a refused probe and a never-run probe both read `None` is the deleted form (the `FORGED_ZERO` boundary at result grain), and `ValidityClaim.Evidence` (`results.md`) stays the distinct validity fold over a nested result, never this carrier.
- Growth: a new quality regime is a new reason `Symbol` at the producing boundary, never a fourth case; a new masking transform reuses `Masked` whole; a new probe whose refusal and absence differ is one `Evidence<T>` column at its own result, never a presence flag beside an error string and never a fourth case.
- Packages: Thinktecture.Runtime.Extensions generates the closed unions and the `Symbol` admission; LanguageExt.Core carries the `Fin`/`Option` probe outcomes and the `Error` a refusal parks.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
public sealed partial class Symbol {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) validationError = new ValidationError("a non-empty reason token");
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record Quality {
    private Quality() { }
    public sealed record Good : Quality;
    public sealed record Uncertain(Symbol Reason) : Quality;
    public sealed record Bad(Symbol Reason) : Quality;
}

[Union]
public abstract partial record Masked {
    private Masked() { }
    public sealed record Unchanged(string Value) : Masked;
    public sealed record Redacted(string Value) : Masked;
    public string Value => Switch(unchanged: static row => row.Value, redacted: static row => row.Value);
}

[Union]
public abstract partial record Evidence<T> {
    private Evidence() { }

    public sealed record Measured(T Value) : Evidence<T>;
    public sealed record Refused(Error Cause) : Evidence<T>;
    public sealed record Absent : Evidence<T>;
}

public static class Evidence {
    public static Evidence<T> Of<T>(Fin<T> probe) =>
        probe.Match(Succ: static value => (Evidence<T>)new Evidence<T>.Measured(value), Fail: static cause => new Evidence<T>.Refused(cause));

    public static Evidence<T> Of<T>(Option<T> probe) =>
        probe.Match(Some: static value => (Evidence<T>)new Evidence<T>.Measured(value), None: static () => new Evidence<T>.Absent());

    public static Option<T> Value<T>(this Evidence<T> evidence) =>
        evidence.Switch(measured: static m => Some(m.Value), refused: static _ => Option<T>.None, absent: static _ => Option<T>.None);
}
```

## [09]-[ADMISSION_VOCABULARY]

- Owner: `Admit` internal static owns SHAPE and COLLECTION admission above a scalar — count agreement, span and sequence gates, frames, cones, and the kernel-input guards, plus the ONE accumulating clause combinator `Claims` every multi-column entry across the kernel composes.
- Law: predicate policy has ONE owner and one guard per result. `ValidityClaim` rows (`results.md`) state every predicate; `Op.Demand(claim, value, requirement)` is the ONE scalar guard, refusing as keyed `KernelFault.OutOfRange` with the rejected number and its requirement on the payload, and `Op.Finite`/`Op.Positive` are its named rows. `Admit` declares no scalar predicate and no bound of its own — it composes claim rows over shapes and collections and `Band` rows over every range, the ONE range guard, and refuses as `KernelFault.InvalidInput`. `Fin<Unit>` and `bool` restatements of a claim row are the deleted form.
- Law: emptiness is a count floor, never a flag — `All` takes the floor as data and composes `ValidityClaim.CountAtLeast`, so a caller admitting an empty sequence and one demanding at least one element differ by a number, not by a body.
- Law: `HermitianDiagonalReal` derives its tolerance from the diagonal's own scale, never an absolute literal.
- Law: a module spells its input gate as one `Admit` composition at its boundary, and a new admission shape is one member here composed everywhere.
- Law: a MULTI-COLUMN entry accumulates through `Admit.Claims`, each clause naming its own axis, so a caller learns a rank mismatch and a non-finite sample together rather than whichever the ladder tested first; a single-value coherence probe keeps `Fin` first-refusal, and a boolean AND-ladder collapsing N independent claims onto one bare `InvalidInput` is the deleted form.
- Exemption: the `Complex` span predicates live here because the claim vocabulary carries no `Complex` arm — interleaved real and imaginary parts have no vectorized finiteness member, so the span fold is the named kernel exemption and `Holds` is its one body.
- Boundary: `Numerics`, `Spatial`, and `Meshing` owners compose these gates at their boundaries; their value objects admit through the `[04]` bridge.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Domain;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Admit {
    internal static Fin<Unit> Claims(Op key, params (bool Held, string Axis)[] clauses) =>
        AdmissionSlots.Accumulate(
            toSeq(clauses).Map(clause => AdmissionSlots.Gate(clause.Held, key.InvalidInput(axis: clause.Axis))))
            .ToFin();

    internal static Fin<T> NotNull<T>(T? value, Op key) where T : class => Optional(value).ToFin(key.InvalidInput());
    internal static Fin<T> NotNull<T>(T? value, Error error) where T : class => Optional(value).ToFin(error);
    internal static Fin<Unit> CountAtLeast(int count, int minimum, Op key) => guard(ValidityClaim.CountAtLeast(count: count, floor: minimum), key.InvalidInput()).ToFin();
    internal static Fin<Unit> SameCount(int expected, Op key, params ReadOnlySpan<int> counts) =>
        guard(Holds(values: counts, claim: count => ValidityClaim.CountExactly(count: count, expected: expected)), key.InvalidInput()).ToFin();
    internal static Fin<Seq<T>> All<T>(Seq<T> values, Func<T, ValidityClaim> claim, int floor, Op key) =>
        guard(ValidityClaim.All(ValidityClaim.CountAtLeast(count: values.Count, floor: floor), values.ForAll(value => claim(arg: value))), key.InvalidInput()).ToFin().Map(_ => values);
    internal static Fin<Unit> AllFinite(ReadOnlySpan<double> values, Op key) => guard(ValidityClaim.Finite(values), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Seq<Point3d> points, Op key) => guard(points.ForAll(static point => ValidityClaim.Finite(point)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Op key, params ReadOnlySpan<Point3d> points) =>
        guard(Holds(values: points, claim: static point => ValidityClaim.Finite(point)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Op key, params ReadOnlySpan<Vector3d> vectors) =>
        guard(Holds(values: vectors, claim: static vector => ValidityClaim.Finite(vector)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> PositiveFiniteWeights(ReadOnlySpan<double> weights, int count, Op key) =>
        guard(ValidityClaim.All(ValidityClaim.CountExactly(count: weights.Length, expected: count), ValidityClaim.Finite(weights), weights.IsEmpty || TensorPrimitives.Min(weights) > 0.0), key.InvalidInput()).ToFin();
    internal static bool FiniteComplexSpan(ReadOnlySpan<Complex> values) =>
        Holds(values: values, claim: static value => ValidityClaim.All(ValidityClaim.Finite(value: value.Real), ValidityClaim.Finite(value: value.Imaginary)));
    internal static bool HermitianDiagonalRealSpan(ReadOnlySpan<Complex> diagonal) {
        double scale = 0.0;
        foreach (Complex entry in diagonal) {
            if (!ValidityClaim.All(ValidityClaim.Finite(value: entry.Real), ValidityClaim.Finite(value: entry.Imaginary))) { return false; }
            scale = Math.Max(val1: scale, val2: Math.Abs(value: entry.Real));
        }
        double tolerance = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: scale * EpsilonPolicy.SqrtEpsilon);
        return Holds(values: diagonal, claim: entry => Math.Abs(value: entry.Imaginary) <= tolerance);
    }
    internal static ValidityClaim Frame(Plane basis) =>
        ValidityClaim.All(
            basis.IsValid,
            ValidityClaim.Finite(basis.Origin),
            ValidityClaim.Finite(basis.XAxis),
            ValidityClaim.Finite(basis.YAxis),
            ValidityClaim.Finite(basis.ZAxis),
            Vector3d.AreOrthonormal(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis),
            Vector3d.AreRighthanded(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis));
    internal static Fin<Plane> Plane(Plane basis, Op key) => guard(Frame(basis: basis), key.InvalidInput()).ToFin().Map(_ => basis);
    internal static Fin<Vector3d> Directional(Vector3d value, double tolerance, Op key) =>
        guard(ValidityClaim.All(ValidityClaim.Finite(value), value.Length > tolerance), key.InvalidInput()).ToFin().Map(_ => value);
    internal static Fin<Unit> Cone(Point3d apex, Vector3d axis, double halfAngle, Op key) =>
        from _ in AllFinite(key, apex)
        from direction in Directional(value: axis, tolerance: EpsilonPolicy.ZeroTolerance, key: key)
        from angle in guard(Band.HalfTurn.Admits(value: halfAngle), key.InvalidInput()).ToFin()
        select unit;
    internal static Fin<Unit> KernelInput(double distance, double radius, Op key) =>
        guard(ValidityClaim.All(ValidityClaim.Nonnegative(value: distance), Band.Positive.Admits(value: radius)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> FalloffInput(double distance, double distanceSquared, double tolerance, Op key) =>
        guard(ValidityClaim.All(ValidityClaim.Nonnegative(value: distance), ValidityClaim.Nonnegative(value: distanceSquared), ValidityClaim.Nonnegative(value: tolerance)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> NoiseInput(int octaves, double persistence, double lacunarity, double frequency, Op key) =>
        guard(ValidityClaim.All(Band.Octave.Admits(value: octaves), ValidityClaim.Positive(value: frequency), Band.Ratio.Admits(value: persistence), Band.Growth.Admits(value: lacunarity)), key.InvalidInput()).ToFin();

    private static bool Holds<T>(ReadOnlySpan<T> values, Func<T, ValidityClaim> claim) {
        foreach (T value in values) {
            if (!claim(arg: value)) { return false; }
        }
        return true;
    }
}
```

## [10]-[DENSITY_BAR]

One owner per concern, each extended by a row.

| [INDEX] | [CONCERN]            | [OWNER]               | [KIND]                       | [RESULT]                             |
| :-----: | :------------------- | :-------------------- | :--------------------------- | :----------------------------------- |
|  [01]   | Readiness rows       | `Requirement`         | record + `Set<Check>` monoid | `Validation<Error, T>`               |
|  [02]   | Check matrix         | `Requirement.Check`   | smart-enum delegate rows     | `Fin<Unit>`                          |
|  [03]   | Validity oracle      | `OpAcceptance`        | derived frozen table         | `Fin<T>` / `Option<bool>`            |
|  [04]   | Factory bridge       | `OpExtensions`        | generated admission methods  | `Validation<Error, TVO>` / `Fin`     |
|  [05]   | Admission projection | `AdmissionProjection` | sealed bidirectional owner   | `Render` / `Admit → Fin<T>`          |
|  [06]   | Pair readiness       | `RequirementContext`  | context extension combinator | `Validation<Error, (A,B,Kind,Kind)>` |
|  [07]   | Capability column    | `CapabilitySet`       | frozen-set membership owner  | `bool` / `Fin<CapabilitySet<T>>`     |
|  [08]   | Quality verdict      | `Quality`             | closed three-case union      | case read via total `Switch`         |
|  [09]   | Masked verdict       | `Masked`              | closed two-case union        | case read via total `Switch`         |
|  [10]   | Probe evidence       | `Evidence<T>`         | closed generic union + mint  | `Of(Fin<T>)` / `Of(Option<T>)`       |
|  [11]   | Admission vocabulary | `Admit`               | shape and collection guards  | `Fin<Unit>` / `Fin<T>`               |

## [11]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
