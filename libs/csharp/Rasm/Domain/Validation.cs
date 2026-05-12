using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct Op {
    [BoundaryAdapter]
    public static Op Of([CallerMemberName] string name = "") => OpCache.GetOrCreate(name: name);
}

[BoundaryAdapter]
internal static class OpCache {
    private static readonly ConcurrentDictionary<string, Op> Cache = new(comparer: StringComparer.Ordinal);
    internal static Op GetOrCreate(string name) => Cache.GetOrAdd(key: name, valueFactory: static n => Op.Create(value: n));
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
    public static readonly Rule Validity = Define(key: "rhino-validity", applies: static _ => true, check: static (rule, _, g) => rule.Demand(geometry: g, condition: g.IsValidWithLog(log: out string log), log: log));
    public static readonly Rule UsableBounds = Define(key: "usable-bounds", applies: static _ => true, check: static (rule, ctx, g) => rule.Demand(geometry: g, condition: g.GetBoundingBox(accurate: true) is { IsValid: true } box && box.IsDegenerate(tolerance: ctx.Absolute.Value) < 4, log: "Rhino could not compute a usable accurate bounding box."));
    public static readonly Rule BrepIntegrity = Define(key: "brep-integrity", applies: static g => g is Brep, check: static (rule, _, g) => g is Brep b ? (b.IsValidTopology(log: out string tLog), b.IsValidGeometry(log: out string gLog), b.IsValidTolerancesAndFlags(log: out string toLog)) switch { (false, _, _) => rule.Reject(geometry: b, log: $"Brep topology: {tLog}"), (_, false, _) => rule.Reject(geometry: b, log: $"Brep geometry: {gLog}"), (_, _, false) => rule.Reject(geometry: b, log: $"Brep tolerances and flags: {toLog}"), _ => rule.Pass() } : rule.Pass());
    public static readonly Rule MeshRhinoCheck = Define(key: "mesh-rhino-check", applies: static g => g is Mesh, check: RunMeshCheck);
    public static readonly Rule MeshManifoldReadiness = Define(key: "mesh-manifold-readiness", applies: static g => g is Mesh, check: static (rule, _, g) => rule.Demand(geometry: g, condition: ((Mesh)g).IsSolid, log: "Mesh is valid Rhino geometry but is not closed and solid enough for volume operations."));
    public static readonly Rule BrepSolidReadiness = Define(key: "brep-solid-readiness", applies: static g => g is Brep, check: static (rule, _, g) => rule.Demand(geometry: g, condition: ((Brep)g).IsSolid, log: "Brep is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule SurfaceSolidReadiness = Define(key: "surface-solid-readiness", applies: static g => g is Surface, check: static (rule, _, g) => rule.Demand(geometry: g, condition: ((Surface)g).IsSolid, log: "Surface is valid Rhino geometry but is not solid enough for volume operations."));
    public static readonly Rule CurveLengthReadiness = Define(key: "curve-length-readiness", applies: static g => g is Curve, check: static (rule, ctx, g) => g is Curve c && !c.IsShort(tolerance: ctx.Absolute.Value) && c.GetLength(fractionalTolerance: ctx.Relative.Value) > ctx.Absolute.Value ? rule.Pass() : rule.Reject(geometry: g, log: "Curve is valid Rhino geometry but is below model-length tolerance."));
    public static readonly Rule CurveAreaReadiness = Define(key: "curve-area-readiness", applies: static g => g is Curve, check: static (rule, ctx, g) => g is Curve c && c.IsClosed && c.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) ? rule.Pass() : rule.Reject(geometry: g, log: "Curve is valid Rhino geometry but is not closed and planar enough for area operations."));
    public static readonly Rule SurfaceDomainReadiness = Define(key: "surface-domain-readiness", applies: static g => g is Surface, check: static (rule, ctx, g) => rule.Demand(geometry: g, condition: HasUsableDomain(surface: (Surface)g, context: ctx), log: "Surface is valid Rhino geometry but has an unusable UV domain."));
    public static readonly Rule ContinuityReadiness = Define(key: "continuity-readiness", applies: static g => g is Curve or Surface, check: RunContinuity);
    public static readonly Rule PolycurveStructure = Define(key: "polycurve-structure", applies: static g => g is PolyCurve, check: static (rule, _, g) => g is PolyCurve p ? (p.HasGap, p.IsNested) switch { (false, false) => rule.Pass(), (true, true) => rule.Reject(geometry: p, log: "PolyCurve has gaps between segments and nested polycurves."), (true, false) => rule.Reject(geometry: p, log: "PolyCurve has gaps between segments."), _ => rule.Reject(geometry: p, log: "PolyCurve contains nested polycurves.") } : rule.Pass());
    public static readonly Rule CurveSelfIntersection = Define(key: "curve-self-intersection", applies: static g => g is Curve, check: RunCurveSelfIntersection);
    internal Func<GeometryBase, bool> Applies { get; }
    internal Func<Rule, Context, GeometryBase, Fin<Unit>> Check { get; }
    [BoundaryAdapter]
    private static Rule Define(string key, Func<GeometryBase, bool> applies, Func<Rule, Context, GeometryBase, Fin<Unit>> check) => new(key: key, applies: applies, check: check);
    private static bool HasUsableDomain(Surface surface, Context context) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) is (Interval u, Interval v)
        && u.IsValid && v.IsValid && u.Length > context.Absolute.Value && v.Length > context.Absolute.Value;
    [BoundaryAdapter]
    private static Fin<Unit> RunContinuity(Rule rule, Context context, GeometryBase geometry) =>
        geometry switch {
            Surface surface => rule.Demand(geometry: surface,
                condition: !HasUsableDomain(surface: surface, context: context)
                    || (!surface.GetNextDiscontinuity(direction: 0, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 0).T0, t1: surface.Domain(direction: 0).T1, t: out double _)
                        && !surface.GetNextDiscontinuity(direction: 1, continuityType: Continuity.C1_continuous, t0: surface.Domain(direction: 1).T0, t1: surface.Domain(direction: 1).T1, t: out double _)),
                log: "Surface is valid Rhino geometry but contains a C1 discontinuity."),
            Curve curve => rule.Demand(geometry: curve, condition: !curve.GetNextDiscontinuity(continuityType: Continuity.C1_continuous, t0: curve.Domain.T0, t1: curve.Domain.T1, t: out double _), log: "Curve is valid Rhino geometry but contains a C1 discontinuity."),
            _ => rule.Pass(),
        };
    [BoundaryAdapter]
    private static Fin<Unit> RunMeshCheck(Rule rule, Context context, GeometryBase geometry) {
        using TextLog textLog = new();
        MeshCheckParameters parameters = MeshCheckParameters.Defaults();
        return geometry is Mesh mesh && mesh.Check(textLog: textLog, parameters: ref parameters) && mesh.Faces.Count > 0
            ? rule.Pass()
            : rule.Reject(geometry: geometry, log: textLog.ToString());
    }
    [BoundaryAdapter]
    private static Fin<Unit> RunCurveSelfIntersection(Rule rule, Context context, GeometryBase geometry) {
        using CurveIntersections? intersections = geometry is Curve curve ? Intersection.CurveSelf(curve: curve, tolerance: context.Absolute.Value) : null;
        return (intersections, geometry) switch {
            (CurveIntersections hits, _) when hits.Count == 0 => rule.Pass(),
            (CurveIntersections hits, _) => rule.Reject(geometry: geometry, log: string.Create(provider: CultureInfo.InvariantCulture, $"Rhino found {hits.Count} curve self-intersection event(s).")),
            (null, Curve _) => rule.Reject(geometry: geometry, log: "Rhino curve self-intersection computation failed."),
            _ => rule.Pass(),
        };
    }
}

[BoundaryAdapter]
public static class RuleRole {
    [BoundaryAdapter]
    public static Fin<Unit> Pass(this Rule rule) => Fin.Succ(unit);
    [BoundaryAdapter]
    public static Fin<Unit> Reject(this Rule rule, GeometryBase geometry, string log) => Fin.Fail<Unit>(error: new Fault.InvalidGeometry(Geometry: geometry, Check: rule, Log: log));
    [BoundaryAdapter]
    public static Fin<Unit> Demand(this Rule rule, GeometryBase geometry, bool condition, string log) => condition ? rule.Pass() : rule.Reject(geometry: geometry, log: log);
    [BoundaryAdapter]
    public static Fin<Unit> Apply(this Rule rule, Context context, GeometryBase geometry) =>
        rule is null ? Fin.Fail<Unit>(error: new Fault.MissingOperation()) : rule.Applies(arg: geometry) ? rule.Check(arg1: rule, arg2: context, arg3: geometry) : rule.Pass();
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record Fault : Error {
    private Fault() { }
    internal const int UnsupportedCode = 9104;
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    public sealed record MissingOperation : Fault { public override string Message => "Geometry operation requires a query."; }
    public sealed record MissingContext(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' requires a model context."; }
    public sealed record InvalidInput(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' received invalid Rhino input."; }
    public sealed record InvalidResult(Op Key) : Fault { public override string Message => $"Geometry operation '{Key}' produced no valid Rhino result."; }
    public sealed record Cancelled : Fault { public override string Message => "Geometry operation was cancelled."; }
    public sealed record Unsupported(Op Key, Type GeometryType, Type OutputType) : Fault {
        public override string Message => $"Geometry operation '{Key}' does not support geometry '{GeometryType.Name}' with output '{OutputType.Name}'.";
        public override int Code => UnsupportedCode;
    }
    public sealed record ComputationFailed(string Label) : Fault { public override string Message => $"Rhino {Label} computation failed."; }
    public sealed record ComputationUnsupported(string Label, Type GeometryType) : Fault { public override string Message => $"Rhino {Label} computation does not support geometry '{GeometryType.Name}'."; }
    public sealed record PrimitiveRejected(Op Key, string Primitive, string Reason) : Fault { public override string Message => $"Geometry operation '{Key}' rejects '{Primitive}' primitive: {Reason}."; }
    public sealed record MissingGeometry : Fault { public override string Message => "Geometry input is required."; }
    public sealed record InvalidGeometry(GeometryBase Geometry, Rule Check, string Log) : Fault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log)
            ? $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}'."
            : $"Geometry validation failed for {Geometry.GetType().Name} under check '{Check.Key}': {Log}";
    }
    public sealed record NonFiniteScalar(string Label, double Scalar) : Fault { public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be finite; actual={Scalar:R}."); }
    public sealed record OutOfRange(string Label, double Scalar, string Requirement) : Fault { public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be {Requirement}; actual={Scalar:R}."); }
    public sealed record InvalidUnitSystem(UnitSystem Units, string Requirement) : Fault { public override string Message => $"Model unit system must be {Requirement}; actual={Units}."; }
    public sealed record MissingDocument : Fault { public override string Message => "Rhino document context is required."; }
    public sealed record MissingCustomUnitScale : Fault { public override string Message => "Rhino document custom model unit scale is required."; }
}

[BoundaryAdapter]
public static class FaultExtensions {
    [BoundaryAdapter]
    public static string Category(this Error error) => error switch {
        Fault.MissingOperation or Fault.MissingContext => "Operation",
        Fault.InvalidInput => "Input",
        Fault.InvalidResult => "Result",
        Fault.Cancelled => "Cancelled",
        Fault.Unsupported or Fault.ComputationUnsupported => "Unsupported",
        Fault.ComputationFailed => "Computation",
        Fault.PrimitiveRejected => "Primitive",
        Fault.MissingGeometry or Fault.InvalidGeometry => "Geometry",
        Fault.NonFiniteScalar or Fault.OutOfRange => "Tolerance",
        Fault.InvalidUnitSystem or Fault.MissingDocument or Fault.MissingCustomUnitScale => "Context",
        _ => "Fault",
    };
    [BoundaryAdapter] public static Error MissingContext(this Op key) => new Fault.MissingContext(Key: key);
    [BoundaryAdapter] public static Error InvalidInput(this Op key) => new Fault.InvalidInput(Key: key);
    [BoundaryAdapter] public static Error InvalidResult(this Op key) => new Fault.InvalidResult(Key: key);
    [BoundaryAdapter] public static Error Unsupported(this Op key, Type geometryType, Type outputType) => new Fault.Unsupported(Key: key, GeometryType: geometryType, OutputType: outputType);
    [BoundaryAdapter] public static Error PrimitiveRejected(this Op key, string primitive, string reason) => new Fault.PrimitiveRejected(Key: key, Primitive: primitive, Reason: reason);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Verify {
    public static Validation<Error, T> Apply<T>(this Context context, T? value, Requirement? requirement = null) where T : notnull =>
        (value, context, requirement ?? Requirement.Strict) switch {
            (T candidate, Context ctx, Requirement req) when candidate is GeometryBase g => RunChecks(checks: req.Checks, context: ctx, geometry: g, original: candidate),
            (T candidate, _, _) => Op.Of(name: "Operand").RequireValid(value: candidate).ToValidation(),
            _ => Fin.Fail<T>(error: new Fault.MissingGeometry()).ToValidation(),
        };
    private static Validation<Error, T> RunChecks<T>(Seq<Rule> checks, Context context, GeometryBase geometry, T original) where T : notnull =>
        checks.Fold(
            initialState: (Acc: Fin.Succ(original).ToValidation(), Ctx: context, Geometry: geometry),
            f: static (folder, rule) => folder with {
                Acc = (folder.Acc, rule.Apply(context: folder.Ctx, geometry: folder.Geometry).ToValidation()).Apply(static (kept, _) => kept).As(),
            }).Acc;
    public static Validation<Error, (TA A, TB B)> Pair<TA, TB>(this Context context, TA a, TB b, Requirement? requirementA = null, Requirement? requirementB = null) where TA : notnull where TB : notnull =>
        (context.Apply(value: a, requirement: requirementA ?? Requirement.Strict),
         context.Apply(value: b, requirement: requirementB ?? Requirement.Strict))
            .Apply(static (left, right) => (A: left, B: right)).As();
    public static Validation<Error, Seq<T>> All<T>(this Context context, Seq<T> values, Requirement? requirement = null) where T : notnull =>
        values.Fold(
            initialState: (Acc: Fin.Succ(Seq<T>()).ToValidation(), Ctx: context, Req: requirement),
            f: static (folder, item) => folder with {
                Acc = (folder.Acc, folder.Ctx.Apply(value: item, requirement: folder.Req)).Apply(static (acc, v) => acc.Add(value: v)).As(),
            }).Acc;
    [BoundaryAdapter]
    internal static Validation<Error, TVO> TryCreateValidated<TVO>(this double candidate) where TVO : IObjectFactory<TVO, double, ValidationError> =>
        (TVO.Validate(value: candidate, provider: CultureInfo.InvariantCulture, item: out TVO? value), value) switch {
            (null, TVO ok) => Fin.Succ(ok).ToValidation(),
            (ValidationError err, _) => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: err.Message)).ToValidation(),
            _ => Fin.Fail<TVO>(error: new Fault.OutOfRange(Label: typeof(TVO).Name, Scalar: candidate, Requirement: "validation failed")).ToValidation(),
        };
    // Rhino value structs each expose `IsValid` but share no base type; a frozen lookup keyed by
    // runtime Type collapses 17 boxing-dispatch arms into one polymorphic surface.
    private static readonly FrozenDictionary<Type, Func<object, bool>> StructValidators = BuildStructValidators();
    [BoundaryAdapter]
    private static FrozenDictionary<Type, Func<object, bool>> BuildStructValidators() =>
        new Dictionary<Type, Func<object, bool>> {
            [typeof(Point2d)] = static v => ((Point2d)v).IsValid,
            [typeof(Point3d)] = static v => ((Point3d)v).IsValid,
            [typeof(Vector3d)] = static v => ((Vector3d)v).IsValid,
            [typeof(Plane)] = static v => ((Plane)v).IsValid,
            [typeof(BoundingBox)] = static v => ((BoundingBox)v).IsValid,
            [typeof(Box)] = static v => ((Box)v).IsValid,
            [typeof(Sphere)] = static v => ((Sphere)v).IsValid,
            [typeof(Cylinder)] = static v => ((Cylinder)v).IsValid,
            [typeof(Cone)] = static v => ((Cone)v).IsValid,
            [typeof(Torus)] = static v => ((Torus)v).IsValid,
            [typeof(Arc)] = static v => ((Arc)v).IsValid,
            [typeof(Circle)] = static v => ((Circle)v).IsValid,
            [typeof(Ellipse)] = static v => ((Ellipse)v).IsValid,
            [typeof(Rectangle3d)] = static v => ((Rectangle3d)v).IsValid,
            [typeof(Interval)] = static v => ((Interval)v).IsValid,
            [typeof(Line)] = static v => ((Line)v).IsValid,
            [typeof(Polyline)] = static v => ((Polyline)v).IsValid,
        }.ToFrozenDictionary();
    [BoundaryAdapter]
    internal static Fin<T> RequireValid<T>(this Op key, T value) =>
        value switch {
            null => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            GeometryBase g => key.Demand(condition: g.IsValid, value: value),
            double d => key.Demand(condition: RhinoMath.IsValidDouble(x: d), value: value),
            bool or int or Enum or SurfaceCurvature or MeshCheckParameters or Kind => Fin.Succ(value),
            MeshPoint mp => key.Demand(condition: mp.Point.IsValid, value: value),
            ComponentIndex ci => key.Demand(condition: ci.ComponentIndexType != ComponentIndexType.InvalidType && ci.Index >= 0, value: value),
            IntersectionEvent ie => key.Demand(condition: (ie.IsPoint || ie.IsOverlap) && ie.PointA.IsValid && ie.PointB.IsValid, value: value),
            ValueTuple<double, Vector3d> p => key.Demand(condition: RhinoMath.IsValidDouble(x: p.Item1) && p.Item2.IsValid, value: value),
            _ => StructValidators.GetValueOrDefault(key: value.GetType()) switch {
                Func<object, bool> validate => key.Demand(condition: validate(arg: value), value: value),
                _ => Fin.Fail<T>(error: new Fault.InvalidResult(Key: key)),
            },
        };
    private static Fin<T> Demand<T>(this Op key, bool condition, T value) =>
        condition ? Fin.Succ(value) : Fin.Fail<T>(error: new Fault.InvalidResult(Key: key));
}
