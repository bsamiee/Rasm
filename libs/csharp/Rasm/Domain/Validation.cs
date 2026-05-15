using System.Collections.Frozen;
using System.Linq.Expressions;
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
public sealed partial record Requirement {
    private readonly Seq<Check> checks;
    private Requirement(Seq<Check> checks) => this.checks = checks;
    internal bool IsEmpty => checks.IsEmpty;
    private Requirement With(params Check[] add) => new(checks: checks.Concat(toSeq(add)).Distinct().ToSeq());
    public Validation<Error, T> Apply<T>(Context context, T? value, CancellationToken cancel = default) where T : notnull =>
        (value, context, this) switch {
            (T candidate, Context ctx, Requirement req) when candidate is GeometryBase g => RunChecks(checks: req.checks, context: ctx, geometry: g, original: candidate, cancel: cancel),
            (T candidate, _, _) => Op.Of(name: "Operand").AcceptValue(value: candidate).ToValidation(),
            _ => Fin.Fail<T>(error: new Fault.MissingGeometry()).ToValidation(),
        };
    private static Validation<Error, T> RunChecks<T>(Seq<Check> checks, Context context, GeometryBase geometry, T original, CancellationToken cancel) where T : notnull =>
        checks
            .Fold(
                initialState: (Value: Fin.Succ(unit).ToValidation(), Context: context, Geometry: geometry, Original: original, Cancel: cancel),
                f: static (rail, check) => rail with { Value = (rail.Value, check.Apply(context: rail.Context, geometry: rail.Geometry, cancel: rail.Cancel).ToValidation()).Apply(static (_, _) => unit).As() })
            switch {
                (Validation<Error, Unit> validation, _, _, T value, _) => (validation, Fin.Succ(value).ToValidation()).Apply(static (_, item) => item).As(),
            };
    public static readonly Requirement None = new(checks: Seq<Check>());
    public static readonly Requirement Basic = new(checks: Seq(Check.Validity, Check.UsableBounds));
    public static readonly Requirement CurveLength = Basic.With(add: [Check.CurveLengthReadiness]);
    public static readonly Requirement AreaMass = Basic.With(add: [Check.CurveAreaReadiness, Check.CurveSelfIntersection]);
    public static readonly Requirement MeshCheck = Basic.With(add: [Check.MeshRhinoCheck]);
    public static readonly Requirement SolidTopology = Basic.With(add: [Check.BrepIntegrity, Check.MeshManifoldReadiness, Check.BrepSolidReadiness, Check.MeshRhinoCheck]);
    public static readonly Requirement VolumeMass = SolidTopology.With(add: [Check.SurfaceSolidReadiness]);
    public static readonly Requirement SurfaceEvaluation = Basic.With(add: [Check.SurfaceDomainReadiness]);
    public static readonly Requirement StrictStructure = SurfaceEvaluation.With(add: [Check.ContinuityReadiness, Check.PolycurveStructure]);
    public static readonly Requirement Strict = new(checks: toSeq(Check.Items));
    private static bool HasUsableDomain(Surface surface, Context context) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) is (Interval u, Interval v)
        && u.IsValid && v.IsValid && u.Length > context.Absolute.Value && v.Length > context.Absolute.Value;
    [BoundaryAdapter]
    private static Fin<Unit> RunContinuity(Check check, Context context, GeometryBase geometry, CancellationToken cancel) =>
        geometry switch {
            Surface surface => check.Demand(geometry: surface, condition: HasUsableDomain(surface: surface, context: context) && !surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _), log: "Surface is valid Rhino geometry but contains a C1 discontinuity.")
                .Bind(_ => cancel.IsCancellationRequested
                    ? Fin.Fail<Unit>(new Fault.Cancelled())
                    : check.Demand(geometry: surface, condition: !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _), log: "Surface is valid Rhino geometry but contains a C1 discontinuity.")),
            Curve curve => check.Demand(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
            _ => check.Pass(),
        };
    [BoundaryAdapter]
    private static Fin<Unit> RunMeshCheck(Check check, Context context, GeometryBase geometry, CancellationToken cancel) {
        return geometry is Mesh mesh
            ? MeshReport(mesh: mesh, check: check.Key).Map(static _ => unit)
            : check.Reject(geometry: geometry, log: "Mesh check requires a Rhino mesh.");
    }
    [BoundaryAdapter]
    internal static Fin<MeshCheckParameters> MeshReport(Mesh mesh, string check) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return mesh.Check(textLog: textLog, parameters: ref parameters)
            ? Fin.Succ(parameters)
            : Fin.Fail<MeshCheckParameters>(error: new Fault.InvalidGeometry(Geometry: mesh, Check: check, Log: textLog.ToString()));
    }
    [BoundaryAdapter]
    private static Fin<Unit> RunCurveSelfIntersection(Check check, Context context, GeometryBase geometry, CancellationToken cancel) {
        return cancel.IsCancellationRequested switch {
            true => Fin.Fail<Unit>(new Fault.Cancelled()),
            false => Probe(check: check, geometry: geometry, tolerance: context.Absolute.Value),
        };
        static Fin<Unit> Probe(Check check, GeometryBase geometry, double tolerance) {
            using CurveIntersections? intersections = geometry is Curve curve ? Intersection.CurveSelf(curve: curve, tolerance: tolerance) : null;
            return (intersections, geometry) switch {
                (CurveIntersections hits, _) when hits.Count == 0 => check.Pass(),
                (CurveIntersections hits, _) => check.Reject(geometry: geometry, log: string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {hits.Count} curve self-intersection event(s).")),
                (null, Curve _) => check.Reject(geometry: geometry, log: "Rhino curve self-intersection computation failed."),
                _ => check.Pass(),
            };
        }
    }
    [SmartEnum<string>]
    private sealed partial class Check {
        public static readonly Check Validity = new(key: "rhino-validity", applies: static _ => true, run: static (check, _, g, _) => check.Demand(geometry: g, condition: g.IsValidWithLog(log: out string log), log: log));
        public static readonly Check UsableBounds = new(key: "usable-bounds", applies: static _ => true, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: g.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: ctx.Absolute.Value) < 4, log: "Rhino could not compute a usable accurate bounding box."));
        public static readonly Check BrepIntegrity = new(key: "brep-integrity", applies: static g => g is Brep, run: static (check, _, g, _) => g is Brep b ? (b.IsValidTopology(log: out string tLog), b.IsValidGeometry(log: out string gLog), b.IsValidTolerancesAndFlags(log: out string toLog)) switch { (false, _, _) => check.Reject(geometry: b, log: $"Brep topology: {tLog}"), (_, false, _) => check.Reject(geometry: b, log: $"Brep geometry: {gLog}"), (_, _, false) => check.Reject(geometry: b, log: $"Brep tolerances and flags: {toLog}"), _ => check.Pass() } : check.Pass());
        public static readonly Check MeshRhinoCheck = new(key: "mesh-rhino-check", applies: static g => g is Mesh, run: RunMeshCheck);
        public static readonly Check MeshManifoldReadiness = new(key: "mesh-manifold-readiness", applies: static g => g is Mesh, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Mesh)g).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
        public static readonly Check BrepSolidReadiness = new(key: "brep-solid-readiness", applies: static g => g is Brep, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Brep)g).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check SurfaceSolidReadiness = new(key: "surface-solid-readiness", applies: static g => g is Surface, run: static (check, _, g, _) => check.Demand(geometry: g, condition: ((Surface)g).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
        public static readonly Check CurveLengthReadiness = new(key: "curve-length-readiness", applies: static g => g is Curve, run: static (check, ctx, g, _) => g is Curve c && !c.IsShort(tolerance: ctx.Absolute.Value) && c.GetLength(fractionalTolerance: ctx.Fractional) > ctx.Absolute.Value ? check.Pass() : check.Reject(geometry: g, log: "Curve is valid Rhino geometry but is below model-length tolerance."));
        public static readonly Check CurveAreaReadiness = new(key: "curve-area-readiness", applies: static g => g is Curve, run: static (check, ctx, g, _) => g is Curve c && c.IsClosed && c.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) ? check.Pass() : check.Reject(geometry: g, log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."));
        public static readonly Check SurfaceDomainReadiness = new(key: "surface-domain-readiness", applies: static g => g is Surface, run: static (check, ctx, g, _) => check.Demand(geometry: g, condition: HasUsableDomain(surface: (Surface)g, context: ctx), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
        public static readonly Check ContinuityReadiness = new(key: "continuity-readiness", applies: static g => g is Curve or Surface, run: RunContinuity);
        public static readonly Check PolycurveStructure = new(key: "polycurve-structure", applies: static g => g is PolyCurve, run: static (check, _, g, _) => g is PolyCurve p ? check.Demand(geometry: p, condition: !p.HasGap, log: "PolyCurve has gaps between segments.") : check.Pass());
        public static readonly Check CurveSelfIntersection = new(key: "curve-self-intersection", applies: static g => g is Curve, run: RunCurveSelfIntersection);
        private readonly Func<GeometryBase, bool> applies;
        private readonly Func<Check, Context, GeometryBase, CancellationToken, Fin<Unit>> run;
        internal Fin<Unit> Pass() => Key switch { _ => Fin.Succ(unit) };
        [BoundaryAdapter]
        internal Fin<Unit> Reject(GeometryBase geometry, string log) =>
            Fin.Fail<Unit>(error: new Fault.InvalidGeometry(Geometry: geometry, Check: Key, Log: log));
        [BoundaryAdapter]
        internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) =>
            condition ? Pass() : Reject(geometry: geometry, log: log);
        internal Fin<Unit> Apply(Context context, GeometryBase geometry, CancellationToken cancel) =>
            cancel.IsCancellationRequested switch {
                true => Fin.Fail<Unit>(error: new Fault.Cancelled()),
                false => applies(arg: geometry) ? run(arg1: this, arg2: context, arg3: geometry, arg4: cancel) : Pass(),
            };
    }
}

public static class RequirementContext {
    public static Validation<Error, (TA A, TB B)> Pair<TA, TB>(this Context context, TA a, TB b, Requirement? requirementA = null, Requirement? requirementB = null, CancellationToken cancel = default) where TA : notnull where TB : notnull =>
        ((requirementA ?? Requirement.Strict).Apply(context: context, value: a, cancel: cancel),
         (requirementB ?? Requirement.Strict).Apply(context: context, value: b, cancel: cancel))
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
        values
            .Fold(
                initialState: (Value: Fin.Succ(Seq<T>()).ToValidation(), Context: context, Requirement: requirement ?? Requirement.Strict, Cancel: cancel),
                f: static (rail, value) => rail with { Value = (rail.Value, rail.Requirement.Apply(context: rail.Context, value: value, cancel: rail.Cancel)).Apply(static (items, item) => items.Add(item)).As() })
            .Value;
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
    public sealed record MissingOperation : Fault { public override string Message => "Geometry operation is required."; public override string Category => "Operation"; }
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
    public sealed record InvalidGeometry(GeometryBase Geometry, string Check, string Log) : Fault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log)
            ? $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check}'."
            : $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check}': {Log}";
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
[BoundaryAdapter]
internal static class OpAcceptance {
    private static readonly FrozenDictionary<Type, Func<object, bool>> ValueValidity = new Type[] {
        typeof(Point2d), typeof(Point3d), typeof(Vector3d), typeof(Plane), typeof(BoundingBox), typeof(Box), typeof(Sphere),
        typeof(Cylinder), typeof(Cone), typeof(Torus), typeof(Arc), typeof(Circle), typeof(Ellipse), typeof(Rectangle3d), typeof(Interval), typeof(Line), typeof(Polyline),
    }.ToFrozenDictionary(static t => t, static t => {
        ParameterExpression p = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, bool>>(Expression.Property(Expression.Convert(p, t), "IsValid"), p).Compile();
    });
    internal static Fin<Seq<TValue>> Accept<TValue>(this Op key, TValue value) =>
        key.AcceptValue(value: value).Map(static candidate => Seq(candidate));
    internal static Fin<Seq<TValue>> Accept<TValue>(this Op key, IEnumerable<TValue> values) =>
        Optional(values).ToFin(key.InvalidResult()).Bind(candidates => candidates.AsIterable().ToSeq().Traverse(value => key.AcceptValue(value: value)).As());
    internal static Fin<Seq<TValue>> AcceptOptional<TValue>(this Op key, IEnumerable<TValue>? values) =>
        Optional(values).Case switch {
            IEnumerable<TValue> candidates => key.Accept(values: candidates),
            _ => Fin.Succ(Seq<TValue>()),
        };
    internal static Fin<Seq<TValue>> AcceptSolved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch { true => key.Accept(value: value), false => Fin.Fail<Seq<TValue>>(key.InvalidResult()) };
    internal static Fin<Seq<TOut>> AcceptResults<TValue, TOut>(this Op key, IEnumerable<TValue> values) => typeof(TValue).Equals(typeof(TOut)) switch {
        true => key.Accept(values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
        false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut))),
    };
    [BoundaryAdapter]
    internal static Validation<Error, TVO> TryCreateValidated<TVO>(this double candidate) where TVO : IObjectFactory<TVO, double, ValidationError> =>
        (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
            (null, TVO ok) => Fin.Succ(ok).ToValidation(),
            (ValidationError err, _) => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: err.Message)).ToValidation(),
            _ => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: "validation failed")).ToValidation(),
        };
    [BoundaryAdapter]
    internal static Fin<T> AcceptValue<T>(this Op key, T value) =>
        value switch {
            null => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            Enum => Fin.Succ(value),
            _ => ValidityOf(source: value!).Case switch {
                bool ok => key.Demand(condition: ok, value: value),
                _ => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            },
        };
    internal static Option<bool> ValidityOf(object? source) =>
        source switch {
            null => Option<bool>.None,
            GeometryBase geometry => Some(geometry.IsValid),
            double scalar => Some(RhinoMath.IsValidDouble(scalar)),
            bool or int or Enum or SurfaceCurvature or MeshCheckParameters => Some(true),
            global::Rasm.Domain.Kind => Some(true),
            CurveFeature => Some(true),
            ClosestHit h => Some(h.Point.IsValid
                && h.Distance.Map(static d => RhinoMath.IsValidDouble(d) && d >= 0.0).IfNone(true)
                && h.Normal.Map(static n => n.IsValid && n.Length > RhinoMath.ZeroTolerance).IfNone(true)
                && h.Component.Map(static c => c is { ComponentIndexType: not ComponentIndexType.InvalidType } && c.Index >= 0).IfNone(true)
                && h.MeshPoint.Map(static m => m.Point.IsValid).IfNone(true)),
            TopologyProjection p => Some(p switch {
                { Value: Curve { IsValid: true } } => true,
                { Value: BrepFace { IsValid: true } face, Source: { ComponentIndexType: ComponentIndexType.BrepFace, Index: int f } } => f >= 0 && f == face.FaceIndex,
                { Value: Brep { IsValid: true, Faces.Count: > 0 }, Source: { ComponentIndexType: ComponentIndexType.BrepFace, Index: >= 0 } } => true,
                { Value: Mesh { IsValid: true } m, Source: { ComponentIndexType: ComponentIndexType.MeshFace, Index: int f } } => f >= 0 && f < m.Faces.Count,
                _ => false,
            }),
            ResidualSample r => Some(r is { Index: >= 0, Location.IsValid: true, Distance: double d, Tolerance: double t, WithinTolerance: bool w } && RhinoMath.IsValidDouble(d) && d >= 0.0 && RhinoMath.IsValidDouble(t) && t >= 0.0 && w == (d <= t)),
            Stats s => Some(s is { Count: > 0, Minimum: double mn, Maximum: double mx, Mean: double me, Variance: double va, Rms: double rm } && RhinoMath.IsValidDouble(mn) && RhinoMath.IsValidDouble(mx) && RhinoMath.IsValidDouble(me) && RhinoMath.IsValidDouble(va) && RhinoMath.IsValidDouble(rm) && mn <= mx && va >= 0.0 && rm >= 0.0),
            Hit h => Some(h is { Id: >= 0 }),
            Couple c => Some(c is { A: >= 0, B: >= 0 }),
            CurveDeviation c => Some(c is { MinimumDistance: double mn, MaximumDistance: double mx, MinimumA.IsValid: true, MinimumB.IsValid: true, MaximumA.IsValid: true, MaximumB.IsValid: true, Tolerance: double t, WithinTolerance: bool w } && RhinoMath.IsValidDouble(mn) && mn >= 0.0 && RhinoMath.IsValidDouble(mx) && mx >= mn && RhinoMath.IsValidDouble(t) && t >= 0.0 && w == (mx <= t)),
            MeshPoint m => Some(m.Point.IsValid),
            ComponentIndex c => Some(c is { ComponentIndexType: not ComponentIndexType.InvalidType } ci && ci.Index >= 0),
            IntersectionHit h => Some(h.IsValid),
            ValueTuple<double, Vector3d> t => Some(t is (double m, Vector3d a) && RhinoMath.IsValidDouble(m) && a.IsValid),
            _ => ValueValidity.GetValueOrDefault(source.GetType()) is Func<object, bool> fn ? Some(fn(source)) : Option<bool>.None,
        };
    private static Fin<T> Demand<T>(this Op key, bool condition, T value) =>
        condition ? Fin.Succ(value) : Fin.Fail<T>(error: new Fault.InvalidResult(Key: key));
}
