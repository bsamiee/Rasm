using System.Runtime.CompilerServices;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct Op {
    [BoundaryAdapter]
    public static Op Of([CallerMemberName] string name = "") => Op.Create(value: name);
}

[BoundaryAdapter]
public interface ICategorized {
    public string Category { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Requirement(Seq<Rule> Checks) {
    internal bool IsEmpty => Checks.IsEmpty;
    public Requirement With(Rule single) => new(Checks: Checks.Add(value: single).Distinct().ToSeq());
    public Requirement With(Seq<Rule> more) => new(Checks: Checks.Concat(more).Distinct().ToSeq());
    public static readonly Requirement None = new(Checks: Seq<Rule>());
    public static readonly Requirement Basic = new(Checks: Seq(Rule.Validity, Rule.UsableBounds));
    public static readonly Requirement CurveLength = Basic.With(single: Rule.CurveLengthReadiness);
    public static readonly Requirement AreaMass = Basic.With(more: Seq(Rule.CurveAreaReadiness, Rule.CurveSelfIntersection));
    public static readonly Requirement MeshCheck = Basic.With(single: Rule.MeshRhinoCheck);
    public static readonly Requirement SolidTopology = Basic.With(more: Seq(Rule.BrepIntegrity, Rule.MeshManifoldReadiness, Rule.BrepSolidReadiness, Rule.MeshRhinoCheck));
    public static readonly Requirement VolumeMass = SolidTopology.With(single: Rule.SurfaceSolidReadiness);
    public static readonly Requirement SurfaceEvaluation = Basic.With(single: Rule.SurfaceDomainReadiness);
    public static readonly Requirement StrictStructure = SurfaceEvaluation.With(more: Seq(Rule.ContinuityReadiness, Rule.PolycurveStructure));
    public static readonly Requirement Strict = new(Checks: toSeq(Rule.Items));
}

// --- [CONSTANTS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Rule {
    public static readonly Rule Validity = Define(key: "rhino-validity", applies: static _ => true, check: static (rule, _, g, _) => rule.Demand(geometry: g, condition: g.IsValidWithLog(log: out string log), log: log));
    public static readonly Rule UsableBounds = Define(key: "usable-bounds", applies: static _ => true, check: static (rule, ctx, g, _) => rule.Demand(geometry: g, condition: g.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: ctx.Absolute.Value) < 4, log: "Rhino could not compute a usable accurate bounding box."));
    public static readonly Rule BrepIntegrity = Define(key: "brep-integrity", applies: static g => g is Brep, check: static (rule, _, g, _) => g is Brep b ? (b.IsValidTopology(log: out string tLog), b.IsValidGeometry(log: out string gLog), b.IsValidTolerancesAndFlags(log: out string toLog)) switch { (false, _, _) => rule.Reject(geometry: b, log: $"Brep topology: {tLog}"), (_, false, _) => rule.Reject(geometry: b, log: $"Brep geometry: {gLog}"), (_, _, false) => rule.Reject(geometry: b, log: $"Brep tolerances and flags: {toLog}"), _ => rule.Pass() } : rule.Pass());
    public static readonly Rule MeshRhinoCheck = Define(key: "mesh-rhino-check", applies: static g => g is Mesh, check: RunMeshCheck);
    public static readonly Rule MeshManifoldReadiness = Define(key: "mesh-manifold-readiness", applies: static g => g is Mesh, check: static (rule, _, g, _) => rule.Demand(geometry: g, condition: ((Mesh)g).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
    public static readonly Rule BrepSolidReadiness = Define(key: "brep-solid-readiness", applies: static g => g is Brep, check: static (rule, _, g, _) => rule.Demand(geometry: g, condition: ((Brep)g).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule SurfaceSolidReadiness = Define(key: "surface-solid-readiness", applies: static g => g is Surface, check: static (rule, _, g, _) => rule.Demand(geometry: g, condition: ((Surface)g).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule CurveLengthReadiness = Define(key: "curve-length-readiness", applies: static g => g is Curve, check: static (rule, ctx, g, _) => g is Curve c && !c.IsShort(tolerance: ctx.Absolute.Value) && c.GetLength(fractionalTolerance: ctx.Fractional) > ctx.Absolute.Value ? rule.Pass() : rule.Reject(geometry: g, log: "Curve is valid Rhino geometry but is below model-length tolerance."));
    public static readonly Rule CurveAreaReadiness = Define(key: "curve-area-readiness", applies: static g => g is Curve, check: static (rule, ctx, g, _) => g is Curve c && c.IsClosed && c.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) ? rule.Pass() : rule.Reject(geometry: g, log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."));
    public static readonly Rule SurfaceDomainReadiness = Define(key: "surface-domain-readiness", applies: static g => g is Surface, check: static (rule, ctx, g, _) => rule.Demand(geometry: g, condition: HasUsableDomain(surface: (Surface)g, context: ctx), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
    public static readonly Rule ContinuityReadiness = Define(key: "continuity-readiness", applies: static g => g is Curve or Surface, check: RunContinuity);
    public static readonly Rule PolycurveStructure = Define(key: "polycurve-structure", applies: static g => g is PolyCurve, check: static (rule, _, g, _) => g is PolyCurve p ? rule.Demand(geometry: p, condition: !p.HasGap, log: "PolyCurve has gaps between segments.") : rule.Pass());
    public static readonly Rule CurveSelfIntersection = Define(key: "curve-self-intersection", applies: static g => g is Curve, check: RunCurveSelfIntersection);
    internal Func<GeometryBase, bool> Applies { get; }
    internal Func<Rule, Context, GeometryBase, CancellationToken, Fin<Unit>> Check { get; }
    [BoundaryAdapter]
    private static Rule Define(string key, Func<GeometryBase, bool> applies, Func<Rule, Context, GeometryBase, CancellationToken, Fin<Unit>> check) => new(key: key, applies: applies, check: check);
    private static bool HasUsableDomain(Surface surface, Context context) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) is (Interval u, Interval v)
        && u.IsValid && v.IsValid && u.Length > context.Absolute.Value && v.Length > context.Absolute.Value;
    [BoundaryAdapter]
    private static Fin<Unit> RunContinuity(Rule rule, Context context, GeometryBase geometry, CancellationToken cancel) =>
        // BOUNDARY ADAPTER — Rhino 9 RhinoCommon exposes no cancellation-aware GetNextDiscontinuity overload; pre-check between direction probes is the only bail-out point.
        geometry switch {
            Surface surface => rule.Demand(geometry: surface, condition: HasUsableDomain(surface: surface, context: context) && !surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _), log: "Surface is valid Rhino geometry but contains a C1 discontinuity.")
                .Bind(_ => cancel.IsCancellationRequested
                    ? Fin.Fail<Unit>(new Fault.Cancelled())
                    : rule.Demand(geometry: surface, condition: !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _), log: "Surface is valid Rhino geometry but contains a C1 discontinuity.")),
            Curve curve => rule.Demand(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
            _ => rule.Pass(),
        };
    [BoundaryAdapter]
    private static Fin<Unit> RunMeshCheck(Rule rule, Context context, GeometryBase geometry, CancellationToken cancel) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry is Mesh mesh && mesh.Check(textLog: textLog, parameters: ref parameters)
            ? rule.Pass()
            : rule.Reject(geometry: geometry, log: textLog.ToString());
    }
    [BoundaryAdapter]
    private static Fin<Unit> RunCurveSelfIntersection(Rule rule, Context context, GeometryBase geometry, CancellationToken cancel) {
        // BOUNDARY ADAPTER — Rhino 9 RhinoCommon Intersection.CurveSelf has no cancellation overload; pre-check token to short-circuit the per-rule loop.
        return cancel.IsCancellationRequested switch {
            true => Fin.Fail<Unit>(new Fault.Cancelled()),
            false => Probe(rule: rule, geometry: geometry, tolerance: context.Absolute.Value),
        };
        static Fin<Unit> Probe(Rule rule, GeometryBase geometry, double tolerance) {
            using CurveIntersections? intersections = geometry is Curve curve ? Intersection.CurveSelf(curve: curve, tolerance: tolerance) : null;
            return (intersections, geometry) switch {
                (CurveIntersections hits, _) when hits.Count == 0 => rule.Pass(),
                (CurveIntersections hits, _) => rule.Reject(geometry: geometry, log: string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {hits.Count} curve self-intersection event(s).")),
                (null, Curve _) => rule.Reject(geometry: geometry, log: "Rhino curve self-intersection computation failed."),
                _ => rule.Pass(),
            };
        }
    }
}

public static class RuleRole {
    public static Fin<Unit> Pass(this Rule rule) => Fin.Succ(unit);
    [BoundaryAdapter] public static Fin<Unit> Reject(this Rule rule, GeometryBase geometry, string log) => Fin.Fail<Unit>(error: new Fault.InvalidGeometry(Geometry: geometry, Check: rule, Log: log));
    [BoundaryAdapter] public static Fin<Unit> Demand(this Rule rule, GeometryBase geometry, bool condition, string log) => condition ? rule.Pass() : rule.Reject(geometry: geometry, log: log);
    public static Fin<Unit> Apply(this Rule? rule, Context context, GeometryBase geometry, CancellationToken cancel) =>
        (rule, cancel.IsCancellationRequested) switch {
            (null, _) => Fin.Fail<Unit>(error: new Fault.MissingOperation()),
            (_, true) => Fin.Fail<Unit>(error: new Fault.Cancelled()),
            (Rule active, _) => active.Applies(arg: geometry) ? active.Check(arg1: active, arg2: context, arg3: geometry, arg4: cancel) : active.Pass(),
        };
}

// --- [ERRORS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public abstract record Expected : Error, ICategorized {
    protected Expected() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    public abstract string Category { get; }
}

[Union]
public abstract partial record Fault : Expected {
    private Fault() : base() { }
    internal const int UnsupportedCode = 9104;
    public sealed record MissingOperation : Fault { public override string Message => "Geometry operation requires a query."; public override string Category => "Operation"; }
    public sealed record MissingContext(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' requires a model context."; public override string Category => "Operation"; }
    public sealed record InvalidInput(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' received invalid Rhino input."; public override string Category => "Input"; }
    public sealed record InvalidResult(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' produced no valid Rhino result."; public override string Category => "Result"; }
    public sealed record Cancelled : Fault { public override string Message => "Geometry operation was cancelled."; public override string Category => "Cancelled"; }
    public sealed record Unsupported(Op Key, Type GeometryType, Type OutputType) : Fault {
        public override string Message => $"Geometry operation '{Key}' does not support geometry '{GeometryType.Name}' with output '{OutputType.Name}'.";
        public override int Code => UnsupportedCode;
        public override string Category => "Unsupported";
    }
    public sealed record ComputationFailed(string Label) : Fault { public override string Message => $"Rhino {Label} computation failed."; public override string Category => "Computation"; }
    public sealed record ComputationUnsupported(string Label, Type GeometryType) : Fault { public override string Message => $"Rhino {Label} computation does not support geometry '{GeometryType.Name}'."; public override string Category => "Unsupported"; }
    public sealed record MissingGeometry : Fault { public override string Message => "Geometry input is required."; public override string Category => "Geometry"; }
    public sealed record InvalidGeometry(GeometryBase Geometry, Rule Check, string Log) : Fault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log)
            ? $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}'."
            : $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}': {Log}";
        public override string Category => "Geometry";
    }
    public sealed record NonFiniteScalar(string Label, double Scalar) : Fault { public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be finite; actual={Scalar:R}."); public override string Category => "Tolerance"; }
    public sealed record OutOfRange(string Label, double Scalar, string Requirement) : Fault { public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be {Requirement}; actual={Scalar:R}."); public override string Category => "Tolerance"; }
    public sealed record InvalidUnitSystem(UnitSystem Units, string Requirement) : Fault { public override string Message => $"Model unit system must be {Requirement}; actual={Units}."; public override string Category => "Context"; }
    public sealed record MissingDocument : Fault { public override string Message => "Rhino document context is required."; public override string Category => "Context"; }
}

[BoundaryAdapter]
public static class FaultExtensions {
    [BoundaryAdapter]
    public static string Category(this Error error) => error switch {
        ICategorized categorized => categorized.Category,
        _ => "Fault",
    };
    [BoundaryAdapter] public static Error MissingContext(this Op key) => new Fault.MissingContext(Key: key);
    [BoundaryAdapter] public static Error InvalidInput(this Op key) => new Fault.InvalidInput(Key: key);
    [BoundaryAdapter] public static Error InvalidResult(this Op key) => new Fault.InvalidResult(Key: key);
    [BoundaryAdapter] public static Error Unsupported(this Op key, Type geometryType, Type outputType) => new Fault.Unsupported(Key: key, GeometryType: geometryType, OutputType: outputType);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Verify {
    public static Validation<Error, T> Apply<T>(this Context context, T? value, Requirement? requirement = null, CancellationToken cancel = default) where T : notnull =>
        (value, context, requirement ?? Requirement.Strict) switch {
            (T candidate, Context ctx, Requirement req) when candidate is GeometryBase g => RunChecks(checks: req.Checks, context: ctx, geometry: g, original: candidate, cancel: cancel),
            (T candidate, _, _) => Op.Of(name: "Operand").RequireValid(value: candidate).ToValidation(),
            _ => Fin.Fail<T>(error: new Fault.MissingGeometry()).ToValidation(),
        };
    private static Validation<Error, T> RunChecks<T>(Seq<Rule> checks, Context context, GeometryBase geometry, T original, CancellationToken cancel) where T : notnull =>
        checks.Fold(
            initialState: (Acc: Fin.Succ(original).ToValidation(), Ctx: context, Geometry: geometry, Cancel: cancel),
            f: static (folder, rule) => folder with {
                Acc = (folder.Acc, rule.Apply(context: folder.Ctx, geometry: folder.Geometry, cancel: folder.Cancel).ToValidation()).Apply(static (kept, _) => kept).As(),
            }).Acc;
    public static Validation<Error, (TA A, TB B)> Pair<TA, TB>(this Context context, TA a, TB b, Requirement? requirementA = null, Requirement? requirementB = null, CancellationToken cancel = default) where TA : notnull where TB : notnull =>
        (context.Apply(value: a, requirement: requirementA ?? Requirement.Strict, cancel: cancel),
         context.Apply(value: b, requirement: requirementB ?? Requirement.Strict, cancel: cancel))
            .Apply(static (left, right) => (A: left, B: right)).As();
    public static Validation<Error, (TA A, TB B, Kind KindA, Kind KindB)> Pair<TA, TB>(
        this Context context,
        TA a,
        TB b,
        Op op,
        Func<Op, Kind, Kind, Fin<(Requirement A, Requirement B)>> requirements,
        CancellationToken cancel = default) where TA : notnull where TB : notnull =>
        Fin.Succ(new PairRail<TA, TB>(Context: context, A: a, B: b, Op: op, Requirements: requirements, Cancel: cancel)).ToValidation()
            .Bind(static rail => (rail.Context.Pair(a: rail.A, b: rail.B, requirementA: Requirement.None, requirementB: Requirement.None, cancel: rail.Cancel), Fin.Succ(rail).ToValidation())
                .Apply(static (pair, state) => state with { A = pair.A, B = pair.B }).As())
            .Bind(static rail => (((object)rail.A).Kind(context: rail.Context).ToValidation(), ((object)rail.B).Kind(context: rail.Context).ToValidation(), Fin.Succ(rail).ToValidation())
                .Apply(static (kindA, kindB, state) => state with { KindA = kindA, KindB = kindB }).As())
            .Bind(static rail => (rail.Requirements(arg1: rail.Op, arg2: rail.KindA!, arg3: rail.KindB!).ToValidation(), Fin.Succ(rail).ToValidation())
                .Apply(static (required, state) => (Required: required, State: state)).As())
            .Bind(static resolved => (resolved.State.Context.Pair(a: resolved.State.A, b: resolved.State.B, requirementA: resolved.Required.A, requirementB: resolved.Required.B, cancel: resolved.State.Cancel), Fin.Succ(resolved.State).ToValidation())
                .Apply(static (pair, state) => (pair.A, pair.B, state.KindA!, state.KindB!)).As());
    public static Validation<Error, Seq<T>> All<T>(this Context context, Seq<T> values, Requirement? requirement = null, CancellationToken cancel = default) where T : notnull =>
        values.Fold(
            initialState: (Acc: Fin.Succ(Seq<T>()).ToValidation(), Ctx: context, Req: requirement, Cancel: cancel),
            f: static (folder, item) => folder with {
                Acc = (folder.Acc, folder.Ctx.Apply(value: item, requirement: folder.Req, cancel: folder.Cancel)).Apply(static (acc, v) => acc.Add(value: v)).As(),
            }).Acc;
    [BoundaryAdapter]
    internal static Validation<Error, TVO> TryCreateValidated<TVO>(this double candidate) where TVO : IObjectFactory<TVO, double, ValidationError> =>
        (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
            (null, TVO ok) => Fin.Succ(ok).ToValidation(),
            (ValidationError err, _) => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: err.Message)).ToValidation(),
            _ => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: "validation failed")).ToValidation(),
        };
    [BoundaryAdapter]
    internal static Fin<T> RequireValid<T>(this Op key, T value) =>
        value switch {
            null => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            Enum => Fin.Succ(value),
            _ => Dispatch.ValidityOf(source: value!).Case switch {
                bool ok => key.Demand(condition: ok, value: value),
                _ => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            },
        };
    private static Fin<T> Demand<T>(this Op key, bool condition, T value) =>
        condition ? Fin.Succ(value) : Fin.Fail<T>(error: new Fault.InvalidResult(Key: key));
    private readonly record struct PairRail<TA, TB>(
        Context Context,
        TA A,
        TB B,
        Op Op,
        Func<Op, Kind, Kind, Fin<(Requirement A, Requirement B)>> Requirements,
        CancellationToken Cancel,
        Kind? KindA = null,
        Kind? KindB = null) where TA : notnull where TB : notnull;
}
