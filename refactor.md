# Rasm C# Domain Unification + Analysis Cascade

## Context

The greenfield Rhino/Grasshopper2 monorepo carries 534 LOC of `libs/csharp/Rasm/Domain/` (Geometry 147, Validation 244, Context 143) and 2147 LOC of `libs/csharp/Rasm/Analysis/` (Analyze 84, Bounds 322, Evaluate 365, Intersect 190, Query 466, Spatial 153, Topology 567). Of those 2147 Analysis lines, ~540–600 (25–28 %) are dispatch ceremony: 79+ inline `(typeof(TGeometry), typeof(TOut)) switch` sites, 8 Analysis-local unions/enums duplicating Domain concepts (`Topology.Curves` ↔ `Domain.CurveFeature` is a textbook DUAL_CANONICAL_SHAPE), 50+ hand-maintained `Op` constants, and seven sibling `*Role.Apply` files mirroring their unions. Forward growth — dozens of additional analysis folders and Grasshopper plugins — will compound this proportionally unless the Domain trio absorbs the polymorphism and the Analysis surface drops to a thin caller layer.

The intent is a tabula-rasa rewrite of the Domain trio into one polymorphic `Kind` surface that carries classification (axes), validation (rule-bound), bounds, vertices, centroid, closest, coercion, curves, components, and intersection — so every Analysis function that today re-derives "what is this geometry?" delegates to `Kind.X(value, ctx, op)`. The `Geometry.cs` ceiling is 225–250 LOC by user mandate; `Context.cs` ~85; `Validation.cs` ~140. Analysis collapses to ~1500–1600 LOC (-25 to -30 %) via method-bearing unions and Domain-delegated dispatch. Aggressive API breaks are welcome; no legacy stubs, no compatibility shims, no deferred work. `pnpm check:cs` is the only validation gate.

## Architectural Overview

```
┌─────────────────────────────────────────────────────────────┐
│ Domain (≈ 475 LOC, was 534)                                 │
│ ┌───────────────────────────┐                               │
│ │ Geometry.cs (≈ 245)       │ Kind smart enum,              │
│ │  - Topology/Primitive/    │ axis enums, Coercion table,   │
│ │    Closure/Solidity enums │ extension blocks for all      │
│ │  - Kind smart enum (21)   │ polymorphic ops, CurveFeature │
│ │  - CurveFeature smart enum│ promoted, CurveProjection +   │
│ │  - Coercion table         │ ITopologyProjection lifted    │
│ │  - extensions on Kind     │ from Analysis to Domain       │
│ └───────────────────────────┘                               │
│ ┌───────────────────────────┐ ┌──────────────────────────┐  │
│ │ Context.cs (≈ 85)         │ │ Validation.cs (≈ 140)    │  │
│ │  4 ValueObject<double>    │ │ Op, Requirement, Rule,    │  │
│ │  tolerances, UnitScale,   │ │ Verify (Apply/Pair/All),  │  │
│ │  Context record, factories│ │ unified Fault union (15)  │  │
│ │                           │ │ — absorbs ContextFault    │  │
│ └───────────────────────────┘ └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                                ▲
                                │ Kind.X(value, ctx, op) → Fin<T>
                                │
┌─────────────────────────────────────────────────────────────┐
│ Analysis (≈ 1560 LOC, was 2147)                              │
│  Method-bearing public unions (Bounds, Measure, Location,    │
│  Curves, Faces, Meshes, Conformance) replace *Role files.    │
│  Query<TGeom,TOut> stays here (owns Analyze.Env runtime).    │
│  IntersectionResult relocates to Domain (Kind.Intersect      │
│  return type).                                               │
└─────────────────────────────────────────────────────────────┘
```

## 1. The Unified Domain — Code Specification

### 1.1 `libs/csharp/Rasm/Domain/Geometry.cs` (target ~245 LOC, hard ceiling 250)

Canonical section order (omit unused): `TYPES → MODELS → CONSTANTS → SERVICES → OPERATIONS → COMPOSITION`. Separator at column 90.

```csharp
namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology  : byte { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum Primitive : byte { None, Line, Polyline, Circle, Arc, Ellipse, Plane, Sphere, Cylinder, Cone, Torus, Box, BoundingBox }
public enum Closure   : byte { Unknown, Open, Closed }
public enum Solidity  : byte { Unknown, Open, Solid }

public interface ITopologyProjection {
    ComponentIndex Source { get; }
    Unit Dispose();
    bool SameAs(ITopologyProjection other);
}

public readonly record struct CurveProjection(Curve Curve, CurveFeature Feature, ComponentIndex Source) : ITopologyProjection {
    public CurveProjection(Curve curve, CurveFeature feature, ComponentIndexType type, int index)
        : this(curve, feature, new ComponentIndex(type, index)) { }
    public Unit Dispose() { Curve.Dispose(); return Unit.Default; }
    public bool SameAs(ITopologyProjection other) =>
        other is CurveProjection c && ReferenceEquals(Curve, c.Curve);
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveFeature {
    public static readonly CurveFeature Input        = new(0,  "input");
    public static readonly CurveFeature Segment      = new(1,  "segment");
    public static readonly CurveFeature Edge         = new(2,  "edge");
    public static readonly CurveFeature Boundary     = new(3,  "boundary");
    public static readonly CurveFeature NakedOuter   = new(4,  "naked-outer");
    public static readonly CurveFeature NakedInner   = new(5,  "naked-inner");
    public static readonly CurveFeature Interior     = new(6,  "interior");
    public static readonly CurveFeature NonManifold  = new(7,  "non-manifold");
    public static readonly CurveFeature OuterLoop    = new(8,  "outer-loop");
    public static readonly CurveFeature InnerLoop    = new(9,  "inner-loop");
    public static readonly CurveFeature IsoU         = new(10, "iso-u");
    public static readonly CurveFeature IsoV         = new(11, "iso-v");
    public static readonly CurveFeature Silhouette   = new(12, "silhouette");
    public static readonly CurveFeature SubCurve     = new(13, "sub-curve");
    public static readonly CurveFeature Draft        = new(14, "draft");
    public string Label { get; }
}

extension(CurveFeature feature) {
    public bool InputCurve => feature == CurveFeature.Input
                            || feature == CurveFeature.Segment
                            || feature == CurveFeature.SubCurve
                            || feature == CurveFeature.Boundary;
    public bool InputBoundary => feature == CurveFeature.Input
                              || feature == CurveFeature.Boundary;
}

// --- [SERVICES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point     = new(0,  typeof(Point3d),     Topology.Point,      Primitive.None,        Closure.Open,    Solidity.Open);
    public static readonly Kind Line      = new(1,  typeof(Line),        Topology.Curve,      Primitive.Line,        Closure.Open,    Solidity.Open);
    public static readonly Kind Polyline  = new(2,  typeof(Polyline),    Topology.Curve,      Primitive.Polyline,    Closure.Unknown, Solidity.Open);
    public static readonly Kind Circle    = new(3,  typeof(Circle),      Topology.Curve,      Primitive.Circle,      Closure.Closed,  Solidity.Open);
    public static readonly Kind Arc       = new(4,  typeof(Arc),         Topology.Curve,      Primitive.Arc,         Closure.Open,    Solidity.Open);
    public static readonly Kind Ellipse   = new(5,  typeof(Ellipse),     Topology.Curve,      Primitive.Ellipse,     Closure.Closed,  Solidity.Open);
    public static readonly Kind Curve     = new(6,  typeof(Curve),       Topology.Curve,      Primitive.None,        Closure.Unknown, Solidity.Open);
    public static readonly Kind Surface   = new(7,  typeof(Surface),     Topology.Surface,    Primitive.None,        Closure.Unknown, Solidity.Open);
    public static readonly Kind Plane     = new(8,  typeof(Plane),       Topology.Surface,    Primitive.Plane,       Closure.Open,    Solidity.Open);
    public static readonly Kind Sphere    = new(9,  typeof(Sphere),      Topology.Surface,    Primitive.Sphere,      Closure.Closed,  Solidity.Solid);
    public static readonly Kind Cylinder  = new(10, typeof(Cylinder),    Topology.Surface,    Primitive.Cylinder,    Closure.Unknown, Solidity.Unknown);
    public static readonly Kind Cone      = new(11, typeof(Cone),        Topology.Surface,    Primitive.Cone,        Closure.Unknown, Solidity.Unknown);
    public static readonly Kind Torus     = new(12, typeof(Torus),       Topology.Surface,    Primitive.Torus,       Closure.Closed,  Solidity.Solid);
    public static readonly Kind Brep      = new(13, typeof(Brep),        Topology.Brep,       Primitive.None,        Closure.Unknown, Solidity.Unknown);
    public static readonly Kind Box       = new(14, typeof(Box),         Topology.Brep,       Primitive.Box,         Closure.Closed,  Solidity.Solid);
    public static readonly Kind BBox      = new(15, typeof(BoundingBox), Topology.Brep,       Primitive.BoundingBox, Closure.Closed,  Solidity.Solid);
    public static readonly Kind Mesh      = new(16, typeof(Mesh),        Topology.Mesh,       Primitive.None,        Closure.Unknown, Solidity.Unknown);
    public static readonly Kind SubD      = new(17, typeof(SubD),        Topology.SubD,       Primitive.None,        Closure.Unknown, Solidity.Unknown);
    public static readonly Kind Cloud     = new(18, typeof(PointCloud),  Topology.PointCloud, Primitive.None,        Closure.Unknown, Solidity.Open);
    public static readonly Kind Extrusion = new(19, typeof(Extrusion),   Topology.Extrusion,  Primitive.None,        Closure.Unknown, Solidity.Unknown);
    public static readonly Kind Hatch     = new(20, typeof(Hatch),       Topology.Hatch,      Primitive.None,        Closure.Closed,  Solidity.Open);

    public Type Type { get; }
    public Topology Topology { get; }
    public Primitive Primitive { get; }
    public Closure NominalClosure { get; }
    public Solidity NominalSolidity { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = BuildByType();
    internal static Option<Kind> For(Type type) =>
        Optional(ByType.GetValueOrDefault(type)) | Inherits(type);
    private static Option<Kind> Inherits(Type t) =>
        typeof(Extrusion).IsAssignableFrom(t)  ? Some(Kind.Extrusion) :
        typeof(Brep).IsAssignableFrom(t)       ? Some(Kind.Brep)      :
        typeof(Surface).IsAssignableFrom(t)    ? Some(Kind.Surface)   :
        typeof(Curve).IsAssignableFrom(t)      ? Some(Kind.Curve)     :
        typeof(Mesh).IsAssignableFrom(t)       ? Some(Kind.Mesh)      :
        typeof(SubD).IsAssignableFrom(t)       ? Some(Kind.SubD)      :
        typeof(PointCloud).IsAssignableFrom(t) ? Some(Kind.Cloud)     :
        typeof(Hatch).IsAssignableFrom(t)      ? Some(Kind.Hatch)     :
        Option<Kind>.None;
    private static FrozenDictionary<Type, Kind> BuildByType() =>
        Kind.Items.ToFrozenDictionary(static k => k.Type);
}

internal static class Coercion {
    private static readonly FrozenDictionary<(Type Source, Type Target), Func<Context, object, Op, Fin<object>>> Table = Build();
    internal static Fin<TTarget> Of<TTarget>(object source, Context ctx, Op op) =>
        Table.TryGetValue((source.GetType(), typeof(TTarget)), out Func<Context, object, Op, Fin<object>>? fn) switch {
            true  => fn(ctx, source, op).Map(static (object v) => (TTarget)v),
            false => Fin.Fail<TTarget>(Fault.Unsupported.For(op, source.GetType(), typeof(TTarget))),
        };
    [BoundaryAdapter] private static FrozenDictionary<(Type, Type), Func<Context, object, Op, Fin<object>>> Build() =>
        // 11–14 entries: Surface→{Plane,Sphere,Cylinder,Cone,Torus}, Curve→{Line,Polyline,Circle,Arc,Ellipse}, Brep→{Box}
        // each uses Rhino's TryGet*(out, tolerance) and returns Fin<object>
        ...;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
extension(Type type) {
    public Option<Kind> AsKind() => KindLookup.For(type);
}

extension(object value) {
    public Fin<Kind> Kind(Context ctx) => value switch {
        Brep b when b.IsBox(ctx.Absolute.Value)                            => Fin.Succ(Domain.Kind.Box),
        Brep { IsSurface: true } single                                    => SurfaceKind(single.Surfaces[0], ctx, brep: true),
        Surface s                                                          => SurfaceKind(s, ctx, brep: false),
        Curve c when c.TryGetPolyline(out _)                               => Fin.Succ(Domain.Kind.Polyline),
        Curve c when c.TryGetCircle(out _, ctx.Absolute.Value)             => Fin.Succ(Domain.Kind.Circle),
        Curve c when c.TryGetArc(out _, ctx.Absolute.Value)                => Fin.Succ(Domain.Kind.Arc),
        Curve c when c.TryGetEllipse(out _, ctx.Absolute.Value)            => Fin.Succ(Domain.Kind.Ellipse),
        Curve c when c.IsLinear(ctx.Absolute.Value)                        => Fin.Succ(Domain.Kind.Line),
        _ => KindLookup.For(value.GetType()).ToFin(Fault.InvalidInput.Anonymous),
    };
    private static Fin<Kind> SurfaceKind(Surface s, Context ctx, bool brep) =>
        s.TryGetPlane(out _, ctx.Absolute.Value)    ? Fin.Succ(brep ? Domain.Kind.Surface : Domain.Kind.Plane) :
        s.TryGetSphere(out _, ctx.Absolute.Value)   ? Fin.Succ(Domain.Kind.Sphere)   :
        s.TryGetCylinder(out _, ctx.Absolute.Value) ? Fin.Succ(Domain.Kind.Cylinder) :
        s.TryGetCone(out _, ctx.Absolute.Value)     ? Fin.Succ(Domain.Kind.Cone)     :
        s.TryGetTorus(out _, ctx.Absolute.Value)    ? Fin.Succ(Domain.Kind.Torus)    :
        Fin.Succ(Domain.Kind.Surface);
}

extension(Kind kind) {
    // -- Bounds (replaces Analysis SpatialMidpoint 13-arm + BoundsRole 8-arm)
    public Fin<BoundingBox> Bounds(object value, Op op);
    // -- Validation (delegates to Verify.Apply; bridge to Validation.cs)
    public Fin<T> Validate<T>(Context ctx, T value, Requirement req) where T : notnull;
    // -- Coercion (replaces GeometryClassifier.ExtractPrimitive)
    public Fin<TTarget> Coerce<TTarget>(object value, Context ctx, Op op);
    // -- Vertices (replaces Topology.Vertices 10-arm + EdgeMidpoints/Closest reuse)
    public Fin<Seq<Point3d>> Vertices(object value, Context ctx, Op op);
    // -- Centroid (replaces Bounds.SpatialMidpoint + Evaluate.Mid mass-centroid fallback)
    public Fin<Point3d> Centroid(object value, Context ctx, Op op);
    // -- Closest (replaces Evaluate.Closest 14-arm)
    public Fin<ClosestHit> Closest(object value, Point3d target, Context ctx, Op op);
    // -- Curves (replaces Topology.CurvesOf 8-arm + ExtractCurveProjections 13-arm)
    public Fin<Seq<CurveProjection>> Curves(object value, CurveSelector selector, Context ctx, Op op);
    // -- Components (replaces Topology.Components 3-arm)
    public Fin<Seq<GeometryBase>> Components(object value, Context ctx, Op op);
    // -- Domain interval(s) (replaces Topology.Domain 3-arm)
    public Fin<Seq<Interval>> Domains(object value, Op op);
    // -- IsoCurves (replaces Topology.IsoCurves/IsoCurveValues 3+3-arm)
    public Fin<Seq<Curve>> IsoCurves(object value, IsoStatus direction, double normalized, Op op);
    // -- ControlPoints (replaces Evaluate.ControlPoints 6-arm)
    public Fin<Seq<Point3d>> ControlPoints(object value, Op op);
    // -- Closure/Solidity refinement (tolerance-aware where applicable)
    public Closure ClosureOf(object value, Context ctx);
    public Solidity SolidityOf(object value, Context ctx);
    // -- Intersect (binary, replaces Intersect.cs 17-arm typeof-pair switch)
    public Fin<IntersectionResult> Intersect(Kind other, object valueA, object valueB, Context ctx, Op op);
}

public readonly record struct ClosestHit(
    Point3d Point,
    Option<double> Distance,
    Option<Vector3d> Normal,
    Option<ComponentIndex> Component,
    Option<MeshPoint> MeshPoint);

public readonly record struct CurveSelector(
    CurveFeature Feature,
    Option<Vector3d> Direction,   // Silhouette/Draft
    Option<double>   Angle,       // Draft
    Option<int>      Index,       // At-index variant
    Option<double>   Normalized   // Iso parameter
);

// --- [COMPOSITION] ------------------------------------------------------------------------
// IntersectionResult moves here from Analysis (Domain owns Kind.Intersect return shape).
[Union]
public abstract partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values)                        : IntersectionResult;
    public sealed record Lines(Seq<Line> Values)                          : IntersectionResult;
    public sealed record Circles(Seq<Circle> Values)                      : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values)                      : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values)                  : IntersectionResult;
    public sealed record Polylines(Seq<Polyline> Values)                  : IntersectionResult;
    public sealed record Events(Seq<IntersectionEvent> Values)            : IntersectionResult;
    public sealed record Mixed(Seq<object> Values)                        : IntersectionResult;
}
```

**LOC budget allocation:**

| Section | Lines |
| --- | --- |
| using/namespace | 5 |
| TYPES (enums × 4 + 2 records + interface) | 22 |
| MODELS (CurveFeature smart enum + 2 extension predicates) | 28 |
| SERVICES (Kind smart enum w/ 21 instances + 5 properties) | 50 |
| CONSTANTS (KindLookup + Coercion table) | 32 |
| OPERATIONS (extension blocks on Type/object/Kind, 13 methods) | 88 |
| COMPOSITION (IntersectionResult union + ClosestHit + CurveSelector) | 22 |
| **TOTAL** | **~247** |

Internal method bodies branch on `kind.Topology` (≤ 6 arms) then forward to private statics that call Rhino's native polymorphism (`GeometryBase.GetBoundingBox`, `Brep.DuplicateEdgeCurves`, `Surface.IsoCurve`, etc.). No nested type-switches.

### 1.2 `libs/csharp/Rasm/Domain/Context.cs` (target ~85 LOC)

```csharp
namespace Rasm.Domain;

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value")]
public readonly partial struct AbsoluteTolerance {
    static partial void ValidateFactoryArguments(ref ValidationError? error, ref double value) =>
        error = double.IsFinite(value) switch {
            false => new ValidationError($"AbsoluteTolerance must be finite (got {value})."),
            true  => value > RhinoMath.ZeroTolerance switch {
                true  => null,
                false => new ValidationError($"AbsoluteTolerance must be > {RhinoMath.ZeroTolerance}."),
            },
        };
}
[ValueObject<double>(KeyMemberName = "Value")]
public readonly partial struct RelativeTolerance {
    static partial void ValidateFactoryArguments(ref ValidationError? error, ref double value) =>
        error = (double.IsFinite(value) && value is >= 0.0 and < 1.0) switch {
            true  => null,
            false => new ValidationError($"RelativeTolerance must lie in [0,1) (got {value})."),
        };
}
[ValueObject<double>(KeyMemberName = "Value")]
public readonly partial struct AngleTolerance { /* validate (eps, 2π] */ }

public readonly record struct UnitScale(UnitSystem Units, double MetersPerUnit) {
    public static Fin<UnitScale> Create(UnitSystem units) =>
        units switch {
            UnitSystem.CustomUnits     => Fin.Fail<UnitScale>(Fault.MissingCustomUnitScale.Instance),
            UnitSystem.Unset or UnitSystem.None
                                       => Fin.Fail<UnitScale>(Fault.InvalidUnitSystem.For(units, "must be a concrete unit system")),
            _ when RhinoMath.MetersPerUnit(units) is double m and > 0.0
                                       => Fin.Succ(new UnitScale(units, m)),
            _                          => Fin.Fail<UnitScale>(Fault.InvalidUnitSystem.For(units, "must yield positive meters-per-unit")),
        };
    public static Fin<UnitScale> FromModelUnits(UnitSystem units, double metersPerUnit) => /* … */;
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record Context(
    AbsoluteTolerance Absolute,
    RelativeTolerance Relative,
    AngleTolerance Angle,
    UnitScale Scale)
{
    public UnitSystem Units => Scale.Units;
    public double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;

    public static Validation<Error, Context> Create(double abs, double rel, double ang, UnitSystem units) =>
        (AbsoluteTolerance.TryCreateValidated(abs),
         RelativeTolerance.TryCreateValidated(rel),
         AngleTolerance.TryCreateValidated(ang),
         UnitScale.Create(units).ToValidation())
        .Apply(static (a, r, n, s) => new Context(a, r, n, s));

    public static Fin<Context> CreateDefault(UnitSystem units);
    public static Fin<Context> FromDocument(RhinoDoc? candidate);
    public static Fin<Context> FromKnownUnits(double abs, double rel, double ang, UnitSystem units, double metersPerUnit);
}
```

`Fault` (including former `ContextFault` variants) lives in `Validation.cs`. Each Thinktecture `ValueObject<double>` exposes the auto-generated `TryCreate` / `Validate` factories; `TryCreateValidated` is a tiny extension defined once in `Validation.cs` that maps `ValidationError` → `Fault.NonFiniteScalar` / `Fault.OutOfRange`.

### 1.3 `libs/csharp/Rasm/Domain/Validation.cs` (target ~140 LOC)

```csharp
namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value")]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct Op {
    private static readonly ConcurrentDictionary<string, Op> Cache = new(StringComparer.Ordinal);
    public static Op Of([CallerMemberName] string name = "") =>
        Cache.GetOrAdd(name, static n => Create(n));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Requirement(Seq<Rule> Checks) {
    public static readonly Requirement None              = new(Seq.empty<Rule>());
    public static readonly Requirement Basic             = new(Seq(Rule.Validity, Rule.UsableBounds));
    public static readonly Requirement CurveLength       = Basic.With(Rule.CurveLengthReadiness);
    public static readonly Requirement AreaMass          = Basic.With(Rule.CurveAreaReadiness);
    public static readonly Requirement MeshCheck         = Basic.With(Rule.MeshRhinoCheck);
    public static readonly Requirement SolidTopology     = Basic.With(Rule.BrepIntegrity, Rule.MeshManifoldReadiness);
    public static readonly Requirement VolumeMass        = Basic.With(Rule.BrepSolidReadiness, Rule.MeshManifoldReadiness, Rule.SurfaceSolidReadiness);
    public static readonly Requirement SurfaceEvaluation = Basic.With(Rule.SurfaceDomainReadiness, Rule.ContinuityReadiness);
    public static readonly Requirement StrictStructure   = Basic.With(Rule.PolycurveStructure, Rule.CurveSelfIntersection);
    public static readonly Requirement Strict            = StrictStructure.With(VolumeMass.Checks);
    public Requirement With(params Rule[] rules)   => this with { Checks = Checks.Concat(toSeq(rules)).Distinct() };
    public Requirement With(Seq<Rule> rules)       => this with { Checks = Checks.Concat(rules).Distinct() };
}

// --- [CONSTANTS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Rule {
    public static readonly Rule Validity              = Define(key: nameof(Validity),              applies: static _ => true,            check: RunValidity);
    public static readonly Rule UsableBounds          = Define(key: nameof(UsableBounds),          applies: static _ => true,            check: RunBounds);
    public static readonly Rule BrepIntegrity         = Define(key: nameof(BrepIntegrity),         applies: static g => g is Brep,        check: RunBrepIntegrity);
    public static readonly Rule MeshRhinoCheck        = Define(key: nameof(MeshRhinoCheck),        applies: static g => g is Mesh,        check: RunMeshCheck);
    public static readonly Rule MeshManifoldReadiness = Define(key: nameof(MeshManifoldReadiness), applies: static g => g is Mesh,        check: static (r, _, g) => r.Demand(g, ((Mesh)g).IsSolid, "mesh must be solid"));
    public static readonly Rule BrepSolidReadiness    = Define(key: nameof(BrepSolidReadiness),    applies: static g => g is Brep,        check: static (r, _, g) => r.Demand(g, ((Brep)g).IsSolid, "brep must be solid"));
    public static readonly Rule SurfaceSolidReadiness = Define(key: nameof(SurfaceSolidReadiness), applies: static g => g is Surface,     check: static (r, _, g) => r.Demand(g, ((Surface)g).IsSolid, "surface must be solid"));
    public static readonly Rule CurveLengthReadiness  = Define(...);
    public static readonly Rule CurveAreaReadiness    = Define(...);
    public static readonly Rule SurfaceDomainReadiness= Define(...);
    public static readonly Rule ContinuityReadiness   = Define(...);
    public static readonly Rule PolycurveStructure    = Define(...);
    public static readonly Rule CurveSelfIntersection = Define(...);

    private static Rule Define(string key, Func<GeometryBase, bool> applies, Func<Rule, Context, GeometryBase, Fin<Unit>> check)
        => new Rule(key) with { Applies = applies, Check = check };
}

extension(Rule rule) {
    internal Fin<Unit> Pass()                                                    => Fin.Succ(unit);
    internal Fin<Unit> Reject(GeometryBase geometry, string log)                 => Fin.Fail<Unit>(Fault.InvalidGeometry.For(geometry, rule, log));
    internal Fin<Unit> Demand(GeometryBase geometry, bool condition, string log) => condition ? rule.Pass() : rule.Reject(geometry, log);
    internal Fin<Unit> Apply(Context ctx, GeometryBase geometry)                 => rule.Applies(geometry) ? rule.Check(rule, ctx, geometry) : rule.Pass();
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record Fault : Error {
    private Fault() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;

    // Operation-rail variants
    public sealed record MissingOperation                                                : Fault;
    public sealed record MissingContext(Op Key)                                          : Fault;
    public sealed record InvalidInput(Op Key)                                            : Fault;
    public sealed record InvalidResult(Op Key)                                           : Fault;
    public sealed record Cancelled                                                       : Fault;
    public sealed record Unsupported(Op Key, Type GeometryType, Type OutputType)         : Fault;
    public sealed record ComputationFailed(string Label)                                 : Fault;
    public sealed record ComputationUnsupported(string Label, Type GeometryType)         : Fault;
    public sealed record PrimitiveRejected(Op Key, string Primitive, string Reason)      : Fault;
    public sealed record MissingGeometry                                                 : Fault;
    public sealed record InvalidGeometry(GeometryBase Geometry, Rule Check, string Log)  : Fault;
    // Context-rail variants (formerly ContextFault — absorbed)
    public sealed record NonFiniteScalar(string Label, double Scalar)                    : Fault;
    public sealed record OutOfRange(string Label, double Scalar, string Requirement)     : Fault;
    public sealed record InvalidUnitSystem(UnitSystem Units, string Requirement)         : Fault;
    public sealed record MissingDocument                                                 : Fault;
    public sealed record MissingCustomUnitScale                                          : Fault;
}

extension(Error error) {
    public string Category => error switch {
        Fault.MissingOperation or Fault.MissingContext       => "Operation",
        Fault.InvalidInput                                   => "Input",
        Fault.InvalidResult                                  => "Result",
        Fault.Cancelled                                      => "Cancelled",
        Fault.Unsupported or Fault.ComputationUnsupported    => "Unsupported",
        Fault.ComputationFailed                              => "Computation",
        Fault.PrimitiveRejected                              => "Primitive",
        Fault.MissingGeometry or Fault.InvalidGeometry       => "Geometry",
        Fault.NonFiniteScalar or Fault.OutOfRange            => "Tolerance",
        Fault.InvalidUnitSystem or Fault.MissingDocument
            or Fault.MissingCustomUnitScale                  => "Context",
        _                                                    => "Unknown",
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Verify {
    public static Validation<Error, T> Apply<T>(this Context ctx, T value, Requirement req) where T : notnull =>
        value switch {
            null              => Fail<Error, T>(Fault.MissingGeometry.Instance),
            GeometryBase g    => req.Checks.Fold(Success<Error, Unit>(unit),
                                     (acc, rule) => acc.Bind(_ => rule.Apply(ctx, g).ToValidation()))
                                  .Map(_ => value),
            _                 => Success<Error, T>(value),   // value primitives have no rules
        };

    public static Validation<Error, (TA A, TB B)> Pair<TA, TB>(this Context ctx, TA a, TB b, Requirement req)
        where TA : notnull where TB : notnull =>
        (ctx.Apply(a, req), ctx.Apply(b, req)).Apply(static (TA aa, TB bb) => (aa, bb));

    public static Validation<Error, Seq<T>> All<T>(this Context ctx, Seq<T> values, Requirement req) where T : notnull =>
        values.Map(v => ctx.Apply(v, req)).Sequence();

    internal static Validation<Error, TVO> TryCreateValidated<TVO, T>(this T candidate)
        where TVO : IValueObjectFactory<TVO, T, ValidationError> =>
        TVO.TryCreate(candidate, out TVO value, out ValidationError? err) switch {
            true  => Success<Error, TVO>(value),
            false => Fail<Error, TVO>(Fault.OutOfRange.For(typeof(TVO).Name, Convert.ToDouble(candidate), err?.Message ?? "validation failed")),
        };
}
```

**Section budget:**

| Section | Lines |
| --- | --- |
| using + namespace | 4 |
| TYPES (`Op` ValueObject + `[CallerMemberName]` cache) | 12 |
| MODELS (`Requirement` record + 10 factories + 2 `With` overloads) | 18 |
| CONSTANTS (`Rule` SmartEnum, 13 instances + `Define` private factory + extension helpers) | 35 |
| ERRORS (`Fault` union, 16 variants + `Category` extension) | 38 |
| OPERATIONS (`Verify.Apply/Pair/All/TryCreateValidated`) | 28 |
| **TOTAL** | **~135** |

## 2. The Analysis Cascade — Per-File Specification

Every public method named `Query.X<TG,TOut>(...)` on `Rasm.Analysis.Query` keeps its name and signature shape (Output.cs + Bridge.cs + Radyab depend on these). Internal dispatch trees collapse onto `Domain.Kind.X(value, ctx, op)`. All seven aspect unions (Bounds, Measure, Location, Curves, Faces, Meshes, Conformance) stay public — they're the Grasshopper selector surface — but become **method-bearing** (`.Apply<TG, TOut>()` lives on the union itself). The seven `*Role` sibling helpers are deleted. The 50+ static `Op` fields are deleted; every call site uses `Op.Of()` resolved via `[CallerMemberName]`.

### 2.1 `Analyze.cs` (84 → ~70 LOC)

- `Validate` shim deletes; route through `ctx.Apply(value, req)` directly.
- `Scope.Run` collapses into one Eff factory; `MissingOperation` guard becomes single expression.
- `Analyze.Env(Context, IProgress?, CancellationToken)` runtime record kept verbatim.

### 2.2 `Bounds.cs` (322 → ~100 LOC)

- `Bounds` union: method-bearing.
  ```csharp
  [Union] public partial record Bounds {
      public sealed record Box                          : Bounds;
      public sealed record Oriented(Plane Plane)        : Bounds;
      public sealed record Transformed(Transform Xform) : Bounds;
      public sealed record Center                       : Bounds;
      public sealed record Corners                      : Bounds;
      public sealed record Edges                        : Bounds;
      public sealed record Area                         : Bounds;
      public sealed record Volume                       : Bounds;
      internal Query<TG, TOut> Apply<TG, TOut>() where TG : notnull => this switch {
          Box                  => Query.FromKind<TG, TOut, BoundingBox>(Op.Of(), static (k, v, ctx) => k.Bounds(v, Op.Of())),
          Oriented o           => Query.FromKind<TG, TOut, Box>(Op.Of(), (k, v, ctx) => k.Bounds(v, Op.Of()).Map(bb => new Box(o.Plane, bb))),
          Transformed t        => Query.FromKind<TG, TOut, BoundingBox>(Op.Of(), (k, v, ctx) => k.Bounds(v, Op.Of()).Map(bb => Transformed(bb, t.Xform))),
          Center               => Query.FromKind<TG, TOut, Point3d>(Op.Of(),     static (k, v, ctx) => k.Bounds(v, Op.Of()).Map(static bb => bb.Center)),
          Corners              => Query.FromKind<TG, TOut, Point3d>(Op.Of(),     static (k, v, ctx) => k.Bounds(v, Op.Of()).Map(static bb => toSeq(bb.GetCorners()))),
          Edges                => Query.FromKind<TG, TOut, Line>(Op.Of(),        static (k, v, ctx) => k.Bounds(v, Op.Of()).Map(static bb => toSeq(bb.GetEdges()))),
          Area                 => Query.FromKind<TG, TOut, double>(Op.Of(),      static (k, v, ctx) => k.Bounds(v, Op.Of()).Map(static bb => bb.Area)),
          Volume               => Query.FromKind<TG, TOut, double>(Op.Of(),      static (k, v, ctx) => k.Bounds(v, Op.Of()).Map(static bb => bb.Volume)),
      };
  }
  ```
- `BoundsRole.Apply` (Bounds.cs:276-287) — deleted.
- `Query.SpatialMidpoint<T, TOut>` (13-arm switch, Bounds.cs:68-104) — one-line forward to `kind.Centroid(value, ctx, op)`.
- `Query.UniqueCorners` / `BoundingCorners` survive as small wrappers around `kind.Bounds`.

### 2.3 `Evaluate.cs` (365 → ~250 LOC)

- `Location` union: method-bearing. 20 variants → 20 `.Apply<TG, TOut>()` arms, each ≤ 3 LOC.
- `LocationRole.Apply` (Evaluate.cs:308-364) — deleted.
- `Query.Closest` (14-arm `(TGeometry, TOut)` switch, Evaluate.cs:200-237) — replaced by:
  ```csharp
  internal static Query<TG, TOut> Closest<TG, TOut>(Point3d target) where TG : notnull =>
      Query<TG, TOut>.Build(key: Op.Of(), state: target, requiresContext: true,
          evaluator: static (Point3d t, TG g) =>
              from env  in Analyze.AsksEnv
              from kind in g.Kind(env.Context).ToEff()
              from hit  in kind.Closest(g, t, env.Context, Op.Of()).ToEff()
              from typed in ProjectClosest<TOut>(hit, Op.Of()).ToEff()
              select typed);
  ```
  `ProjectClosest<TOut>` is a small typed extractor (~15 LOC) that pulls Point3d / double / Vector3d / ComponentIndex / MeshPoint out of the `ClosestHit` payload.
- `Query.ControlPoints` (6-arm switch, Evaluate.cs:69-103) — one-line forward to `kind.ControlPoints(value, op)`.
- `Query.Mid` / `TangentAtMiddle` / `DividePoly` retain their bodies (genuinely curve-specific, not type-spam).
- `Query.CurvatureProfile` (8-branch, Evaluate.cs:131-141) loses its `(TGeometry, TOut)` switch but keeps its (count, scalar, geometry) parameterization — those are real semantic axes.

### 2.4 `Intersect.cs` (190 → ~130 LOC)

- 17-arm `(typeof(TA), typeof(TB), typeof(TOut))` switch (Intersect.cs:7-65) — deleted. Replaced by:
  ```csharp
  public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
      Query<(TA, TB), TOut>.Build(Op.Of(), requiresContext: true,
          evaluator: static ((TA A, TB B) pair) =>
              from env in Analyze.AsksEnv
              from kA  in pair.A.Kind(env.Context).ToEff()
              from kB  in pair.B.Kind(env.Context).ToEff()
              from res in kA.Intersect(kB, pair.A, pair.B, env.Context, Op.Of()).ToEff()
              from typed in IntersectionProjection.Project<TOut>(res, Op.Of()).ToEff()
              select typed);
  ```
- `IntersectionResult` union moves to `Domain.Geometry.cs` (already shown above — it's the return type of `Kind.Intersect`).
- `IntersectionKind` enum (4 values) stays here — it's a public summary tag for Grasshopper consumers.
- `IntersectionResultRole.Project` (Intersect.cs:152-189) — relocates to `IntersectionProjection.Project<TOut>` (one method, ~25 LOC) that fans the union into a typed `Seq<TOut>`.
- `Query.Deviation` (Intersect.cs:67-83) survives unchanged — it's not really intersection.

### 2.5 `Query.cs` (466 → ~285 LOC)

- `Query<TGeom, TOut>` record slimmed from 99 → ~55 LOC by:
  - Primary constructor everywhere (`public sealed record Query<TGeom, TOut>(Op Key, Requirement Requirement, bool RequiresContext, Option<Error> Rejection, Func<Seq<TGeom>, Eff<Analyze.Env, Seq<TOut>>> Evaluate, Option<Func<Seq<TGeom>, Eff<Analyze.Env, Seq<TOut>>>> AggregatePlan = default)`).
  - Two `Build` overloads collapse into one via `Unit`-state passthrough.
  - `Ready` + `Validate` helpers merge into one `Prepare` private static.
  - `Contramap` / `Aggregate` / `Reject` reshaped via `with` expressions.
- The 50+ static `Op` fields block (Query.cs:347-366) — deleted. Each Query factory calls `Op.Of()` which resolves to its `[CallerMemberName]`.
- `MassKind` smart enum stays in Query.cs — it's an Analysis-layer effect strategy (Length/Area/Volume mass-property compute), not Domain classification.
- `Measure` / `Conformance` / `Faces` unions — method-bearing (same pattern as Bounds).
- `Curves` / `Meshes` unions also method-bearing but live in `Topology.cs`.
- `MeshFaceMetrics` / `MeshCheckCounts` static helpers stay — pure projection.
- `One`, `Many`, `Solved`, `Bracket`, `BracketEach`, `Cast`, `Native`, `Results` survive as private statics (~60 LOC total).
- New helper: `Query.FromKind<TG, TOut, TVal>(Op key, Func<Kind, object, Context, Fin<Seq<TVal>>> extract, Requirement? req = null)` — the universal bridge that all method-bearing union arms feed into. Absorbs the typed-cast ceremony in one place.

### 2.6 `Spatial.cs` (153 → ~145 LOC)

Lean already. Only changes:
- `Tree.Bounds<T>` leverages `kind.Bounds(value, op)` for the box accumulation.
- `private static Fin<Point3d[]> ValidatePoints` (Spatial.cs:109-116) replaced by `ctx.Apply(point, Requirement.Basic).ToFin()`.

### 2.7 `Topology.cs` (567 → ~380 LOC) — biggest single-file cut

- `Curves` union: method-bearing; **14 variants** (down from 15: IsoU/IsoV merge into `Iso(IsoStatus, double Normalized)`).
  ```csharp
  [Union] public partial record Curves {
      public sealed record All        : Curves;
      public sealed record Segments   : Curves;
      public sealed record Boundary   : Curves;
      public sealed record NakedOuter : Curves;
      public sealed record NakedInner : Curves;
      public sealed record Interior   : Curves;
      public sealed record NonManifold: Curves;
      public sealed record OuterLoop  : Curves;
      public sealed record InnerLoop  : Curves;
      public sealed record SubCurves  : Curves;
      public sealed record Iso(IsoStatus Direction, double Normalized)         : Curves;
      public sealed record Silhouette(Option<Vector3d> Direction)              : Curves;
      public sealed record Draft(Option<Vector3d> Direction, Option<double> Angle) : Curves;
      public sealed record At(Option<int> Index)                               : Curves;

      internal CurveSelector Selector => /* maps variant → Domain.CurveSelector */;
      internal Query<TG, TOut> Apply<TG, TOut>() where TG : notnull =>
          Query.FromKind<TG, TOut, CurveProjection>(Op.Of(), (k, v, ctx) => k.Curves(v, Selector, ctx, Op.Of()));
  }
  ```
  The previous `Curves.Edge` accessor returning `(CurveFeature, Predicate)` tuples (Topology.cs:321-330) vanishes — Domain's `Kind.Curves` carries the predicate dispatch internally.
- `Faces` union (4 variants): method-bearing. `Top` / `Bottom` rank-by-centroid-axis logic moves into the `.Apply<TG, TOut>()` body (~12 LOC), no helper file.
- `Meshes` union (5 variants): method-bearing. Bundle constructors stay.
- `FacesRole.Apply` (527-542), `CurvesRole.Apply` (545-553), `MeshesRole.Apply` (557-567) — all deleted.
- `Query.Vertices` (Topology.cs:166-194, 10-arm) — one-line `kind.Vertices(value, ctx, Op.Of())`.
- `Query.Components` (207-213, 3-arm) — `kind.Components(value, ctx, Op.Of())`.
- `Query.Domain` (71-75, 3-arm) — `kind.Domains(value, Op.Of())`.
- `Query.EdgeMidpoints` (82-108, 6-arm) — composes `kind.Curves(...).Map(c => c.PointAtNormalized(0.5))`.
- `Query.NakedEdges` (109-113, 3-arm) — `kind.Curves(value, new CurveSelector(CurveFeature.NakedOuter, ...), ctx, op)`.
- `Query.IsoCurves` / `IsoCurveValues` (372-411) — `kind.IsoCurves(value, direction, normalized, op)`.
- `Query.IsPointInside` (151-165, 3-arm) — small wrapper around `kind.Closest(value, target, ctx, op)` with sign test, or a new `kind.Contains(value, target, ctx, op)` if the dispatch is cheap enough — defer to implementation.
- Private helpers `BrepLeaves`, `CurvesOf`, `ExtractCurveProjections`, `ProjectCurve`, `IsoCurves`, `IsoCurveValues`, `BrepEdgeCurves`, `MeshEdgeCurves`, `LoopCurves`, `SilhouetteCurves`, `DraftCurves`, `SilhouetteProjections`, `CurvePieces`, `IndexedCurves`, `OneCurve`, `SurfaceBoundaryCurves` — **all removed**; their logic moves into `Domain.Kind.Curves` extension body (Domain.Geometry.cs SERVICES/COMPOSITION).
- Topology projections `FaceProjection`, `MeshFaceProjection` stay; `CurveProjection` + `ITopologyProjection` move to Domain.
- Public surface kept name-stable: `Query.Domain`, `Query.Segments`, `Query.Edges`, `Query.EdgeMidpoints`, `Query.NakedEdges`, `Query.Outlines`, `Query.Iso`, `Query.Primitive`, `Query.Kind`, `Query.SolidOrientation`, `Query.IsPointInside`, `Query.Vertices`, `Query.Components`, `Query.IsManifold`, `Query.NakedPointStatus`, `Query.MeshCheck`, `Query.MeshCheckCount`, `Query.MeshFaceMetric`, `Query.MeshValidityBundle/StatsBundle/DefectsBundle`, `Query.MeshAtFace`, `Query.Meshes`, `Query.Faces`, `Query.Curves`, `Query.SelfIntersections`, `Query.CurveProjections`, `Query.FaceProjections`, `Query.FrameAtCentroid`, `Query.FaceCentroid`, `Query.FaceDomains`.

### 2.8 Global totals

| Layer | Old LOC | New LOC | Δ |
| --- | --- | --- | --- |
| Domain trio | 534 | ~470 | -12 % |
| Analysis 7 files | 2147 | ~1560 | -27 % |
| **Repository totals** | **2681** | **~2030** | **-24 %** |

`Rasm.Grasshopper/*` and `apps/grasshopper/Radyab/*` see **zero source changes** — every public symbol they import keeps its name and shape. Smoke-validated against `Output.cs:17,20-22`, `Bridge.cs:48,56`, and the four `Extract*.cs` components.

## 3. Critical files to be modified

```
libs/csharp/Rasm/Domain/Geometry.cs        ← rewritten (Kind + axes + Coercion + extensions)
libs/csharp/Rasm/Domain/Context.cs         ← rewritten (4 tolerance ValueObjects)
libs/csharp/Rasm/Domain/Validation.cs      ← rewritten (slim Rule, unified Fault, Verify+All)
libs/csharp/Rasm/Analysis/Analyze.cs       ← compressed (~70 LOC)
libs/csharp/Rasm/Analysis/Bounds.cs        ← method-bearing Bounds union; BoundsRole deleted
libs/csharp/Rasm/Analysis/Evaluate.cs      ← method-bearing Location union; LocationRole deleted
libs/csharp/Rasm/Analysis/Intersect.cs     ← Kind.Intersect dispatch; IntersectionResult moves to Domain
libs/csharp/Rasm/Analysis/Query.cs         ← slim Query<>, Op.Of(), method-bearing Measure/Conformance/Faces; *Role files deleted
libs/csharp/Rasm/Analysis/Spatial.cs       ← minor; Tree.Bounds via Kind
libs/csharp/Rasm/Analysis/Topology.cs      ← method-bearing Curves/Faces/Meshes; CurvesRole/FacesRole/MeshesRole deleted; 16 private helpers removed
```

Files **not** modified: `libs/csharp/Rasm.Grasshopper/*` (all 5), `apps/grasshopper/Radyab/*` (all 6). Their public API contract is preserved.

## 4. Existing primitives we reuse (never reinvent)

| Primitive | Today's location | Reuse strategy |
| --- | --- | --- |
| `Op` ValueObject + comparer strategies | `Validation.cs:6-9` | Keep; extend with `[CallerMemberName]` static factory `Op.Of()`. |
| `Sourced<T>` Meta propagation, Plain/One/Many slot trio | (External — bridge layer per commit `3303161`) | Untouched; remain Output's responsibility. |
| `Bracket` / `BracketEach` resource scope | `Query.cs` private statics | Keep verbatim. |
| `FrozenDictionary<Type, _>` workaround for LanguageExt v5 Map<Type, _> Rhino reflection bug | `Geometry.cs:99` (comment) | Preserve verbatim in `KindLookup` and `Coercion`. |
| `[Union]` source generator + generated `Switch`/`Map` | Thinktecture 10.2.0 — used 8× today | Used for every aspect union + `Fault` + `IntersectionResult`. |
| `[ValueObject<T>]` source generator | Thinktecture 10.2.0 — used 1× today (`Op`) | Extended to 4× tolerance variants (Absolute/Relative/Angle plus `CustomUnitScale`). |
| `[SmartEnum<T>]` source generator + extension blocks | Thinktecture 10.2.0 — used 5× today | Used for `Kind`, `CurveFeature` (promoted from enum), and existing `Rule`, `MassKind`, `PortKind`. |
| `Fin<T>` / `Validation<Error, T>` / `Eff<Env, T>` / `Seq<T>` / `Option<T>` | LanguageExt 5.0.0-beta-77 | Pervasive. The Domain extension surface is `Fin<T>`-typed; Analysis lifts to `Eff<Analyze.Env, T>` at call sites via `.ToEff()`. |

No new NuGet packages or new global usings are introduced.

## 5. Execution Strategy — Sequential Phases with Mandatory Critique

Six phases, each a single coordinated commit that leaves the build green (`pnpm check:cs` passes). Each phase is executed by a paired **Implementation agent → Review agent** cycle. Both agents are spawned with `subagent_type: general-purpose`, both load the `coding-csharp` skill content in full at start, both read this plan in full, both are explicitly told that they may not introduce TODOs, deprecation stubs, dual encodings, or split-brain states. The review agent **fixes issues itself** by direct edits — it does not produce a report for the orchestrator to action. After both agents complete, the orchestrator inspects the diff and the build output and either approves the phase or re-launches the review agent with a tightened brief.

### Phase 1 — Domain Tabula Rasa + Analysis Compilation Bridge

**Scope:** rewrite `Domain/Geometry.cs`, `Domain/Context.cs`, `Domain/Validation.cs` per §1; touch every Analysis file minimally so the build stays green. Internal Analysis dispatch trees may **remain** in place (BoundsRole, LocationRole, CurvesRole, FacesRole, MeshesRole, etc. survive this phase) — they just consume the new Domain surface. The Op-statics block (Query.cs:347-366) is **kept** for now to minimize churn; replacement happens in Phase 6. The seven aspect unions are **kept** in their current non-method-bearing shape; method-bearing migration happens in Phases 2–5.

**Concretely:**
- Add `Topology`/`Primitive`/`Closure`/`Solidity` enums, `Kind` SmartEnum (21 instances), `CurveFeature` SmartEnum (promoted from plain enum, same 15 members), `KindLookup`, `Coercion`, all extension blocks per §1.1.
- Promote `CurveProjection` + `ITopologyProjection` from `Analysis/Topology.cs` to `Domain/Geometry.cs`.
- Promote `IntersectionResult` from `Analysis/Intersect.cs` to `Domain/Geometry.cs`.
- Replace tolerance struct with 4 `[ValueObject<double>]` types (Absolute/Relative/Angle + CustomUnitScale); update `Context` factories accordingly.
- Merge `ContextFault` variants into `Fault`. Update every call site (`Context.cs:122-143`, plus any `ContextFault.X` ref in Analysis) to `Fault.X`.
- Compress `Rule` smart enum via private `Define` factory + extension helpers (`Pass`, `Reject`, `Demand`).
- Add `Verify.All<T>` extension.
- Across all Analysis files: replace `GeometryClassifier.KindOf(g, ctx)` → `g.Kind(ctx).IfFail(Kind.Unknown).Topology`-style usage (or new `Kind.Of(...)` calls), `GeometryClassifier.For(...)` → `Type.AsKind()`, `GeometryClassifier.BoundingBoxOf(g, k)` → `g.Kind(ctx).Bind(k => k.Bounds(g, op))`, `GeometryClassifier.ExtractPrimitive<T>(g, ctx, k)` → `g.Kind(ctx).Bind(k => k.Coerce<T>(g, ctx, op))`, `GeometryKind.Curve` → `kind.Topology == Topology.Curve`, `Fault.PrimitiveNoEdges` / `PrimitiveNoVertices` → `Fault.PrimitiveRejected`.
- No method renames on the public Analysis surface.

**Implementation agent prompt template** — see §6 below. Validation: `pnpm check:cs` passes.

**Review agent must specifically check:**
- All `GeometryKind`/`GeometryClassifier`/`ContextFault`/`Tolerance` legacy references **deleted**, not aliased.
- `Geometry.cs` LOC ≤ 250.
- Coercion table covers ≥ 11 Source→Target pairs (Curve→Line/Polyline/Circle/Arc/Ellipse, Surface→Plane/Sphere/Cylinder/Cone/Torus, Brep→Box).
- Every `extension(Kind kind)` method dispatches on at most 6 `kind.Topology` arms.
- No `if`/`else`/`for`/`while`/`switch` *statements* in Domain (switch *expressions* only).
- `Op.Of()` factory available even if not yet adopted.

### Phase 2 — Bounds.cs cascade

**Scope:** `Bounds` union becomes method-bearing per §2.2. `BoundsRole.Apply` deleted. `Query.SpatialMidpoint` collapses to a single-line forward. `Query.UniqueCorners` / `Query.BoundingCorners` reshape around `kind.Bounds`. Target file LOC ~100.

**Validation:** `pnpm check:cs` passes.

**Review agent checks:**
- `BoundsRole` class fully removed.
- `Bounds.Apply<TG, TOut>` exhaustive switch (Thinktecture-generated) — no missing variants, no `_ => default`.
- Each variant arm ≤ 3 LOC.
- `SpatialMidpoint` body ≤ 3 LOC.

### Phase 3 — Evaluate.cs cascade

**Scope:** `Location` union becomes method-bearing. `LocationRole.Apply` deleted. `Query.Closest` collapses to the 6-line Eff pipeline shown in §2.3. `Query.ControlPoints` becomes one-line forward. `Query.Mid` / `TangentAtMiddle` / `DividePoly` retained as-is. Target file LOC ~250.

**Review agent checks:**
- `LocationRole` fully removed.
- `Location.Apply<TG, TOut>` exhaustive over all 20 variants.
- No `(typeof(TGeometry), typeof(TOut))` switches survive.
- `ProjectClosest<TOut>` typed extractor ≤ 18 LOC.

### Phase 4 — Topology.cs cascade (largest cut)

**Scope:** `Curves` union slimmed (14 variants, IsoU+IsoV merged). `Faces` and `Meshes` unions method-bearing. `CurvesRole`, `FacesRole`, `MeshesRole` deleted. 16 private helpers (`CurvesOf`, `ExtractCurveProjections`, `BrepLeaves`, `BrepEdgeCurves`, `MeshEdgeCurves`, `LoopCurves`, `SilhouetteCurves`, `DraftCurves`, `IsoCurves`, `IsoCurveValues`, `SilhouetteProjections`, `ProjectCurve`, `CurvePieces`, `IndexedCurves`, `OneCurve`, `SurfaceBoundaryCurves`) deleted. `Query.Vertices`, `Query.Components`, `Query.Domain`, `Query.EdgeMidpoints`, `Query.NakedEdges`, `Query.IsoCurves`, `Query.IsPointInside`, `Query.SolidOrientation`, `Query.Primitive`, `Query.Kind` collapse to single-expression bodies through `kind.X(...)` extensions. Target file LOC ~380.

**Review agent checks:**
- All 16 private helpers gone (no compromise — they must be inlined or moved to `Domain.Kind.Curves` body).
- No second `CurveFeature`-mirroring shape exists (`Curves.Edge` accessor gone).
- `Curves.Apply<TG, TOut>` arms ≤ 3 LOC each.
- `Faces` Top/Bottom ranking moves into `.Apply<TG, TOut>` (no helper).
- File LOC ≤ 400.

### Phase 5 — Intersect.cs cascade

**Scope:** 17-arm `(typeof, typeof, typeof)` switch deleted. `Query.Intersect<TA, TB, TOut>` replaced by the 6-line Eff pipeline in §2.4. `IntersectionResult` location confirmed in Domain (was relocated in Phase 1; this phase ratifies callers). `IntersectionProjection.Project<TOut>` extractor (~25 LOC). `IntersectionResultRole.Project` deleted. `Pair*` helpers (Intersect.cs:104-132) deleted; their logic absorbed by the typed projection. Target file LOC ~130.

**Review agent checks:**
- No `(typeof, typeof, typeof)` switch survives anywhere in Analysis.
- `IntersectionResult` referenced by full qualified name from `Domain.Geometry.cs` or via using.
- `IntersectionKind` enum stays (it's a public summary tag).
- `Query.Deviation` unchanged.

### Phase 6 — Query.cs cascade + Op cleanup

**Scope:** `Query<TGeom, TOut>` slimmed to ~55 LOC per §2.5. `Build` overloads collapsed. `Measure` / `Conformance` / `Faces` (and `Faces`-only-living-in-Query if any) become method-bearing. `MeasureRole`, `ConformanceRole`, and any remaining `*Role` artefacts deleted. The 50+ static `Op` constants block (Query.cs:347-366) deleted; every Query factory uses `Op.Of()` via `[CallerMemberName]`. `Query.FromKind<TG, TOut, TVal>` private helper added. Target file LOC ~285.

**Review agent checks:**
- Zero static `Op` field declarations remain in `Analysis/Query.cs`.
- `Query<TG, TOut>` record body ≤ 60 LOC.
- All seven aspect unions are method-bearing.
- All seven `*Role` files are deleted.
- `MassKind` stays in `Query.cs` (it's an Analysis effect strategy).
- `pnpm check:cs` passes.
- Final repository LOC inspected: Domain ≤ 475 (target 470), Analysis ≤ 1620 (target 1560).

## 6. Sub-Agent Briefing Templates

Each implementation and review agent is spawned with `subagent_type: general-purpose` (so it has Edit/Write/Bash/Read access). The prompts below are templates — the orchestrator fills `<PHASE_NAME>`, `<SCOPE_SUMMARY>`, `<REVIEW_CHECKLIST>`, and `<EXTRA_CONSTRAINTS>` per phase.

### 6.1 Implementation agent prompt

```
You are executing Phase <N> of the Rasm domain unification + analysis cascade refactor in a greenfield C# Rhino/Grasshopper2 monorepo. There is no legacy to preserve; aggressive API breaks are welcome within the scope below; the prior phases have already landed.

MANDATORY READING (do this first, in this order, no skipping):
1. /Users/bardiasamiee/.claude/plans/you-are-an-orchestrator-serialized-flask.md  (the full plan; read every section)
2. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/SKILL.md
3. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/patterns.md
4. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/objects.md
5. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/effects.md
6. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/validation.md
7. /Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md and every applicable file under .claude/rules/
8. Every source file in your phase scope (full reads, no skimming)

PHASE <N>: <PHASE_NAME>
SCOPE: <SCOPE_SUMMARY>

HARD RULES:
- Zero `if`/`else`/`while`/`for`/`foreach`/`try`/`catch`/`throw` in domain logic; switch expressions only; LINQ comprehension for monadic chains.
- Zero `var`. Every type explicit.
- Named parameters at every domain call site (single-arg LINQ lambdas exempt).
- One canonical shape per concept. No dual encodings, no legacy aliases.
- No TODO comments, no deprecation stubs, no compatibility shims, no deferred work.
- No new helper files, no single-caller utilities. Inline at the call site or attach as an extension on the owning type.
- No new packages. Use what is already in `Directory.Build.props`.
- File organization: canonical section order (TYPES → MODELS → CONSTANTS → ERRORS → SERVICES → OPERATIONS → COMPOSITION → EXPORTS), separator at column 90 per project standard.

PUBLIC API CONTRACT (immovable):
- Every `Rasm.Analysis.Query.X<TG, TOut>(...)` and `Rasm.Analysis.Query.Y(...)` entry point keeps its exact name and signature shape (Output.cs, Bridge.cs, and Radyab components depend on these). You may change internals freely.
- `Analyze.In(...)` factories keep their signatures.
- `IntersectionKind` enum stays public; `Curves`/`Faces`/`Meshes`/`Bounds`/`Measure`/`Location`/`Conformance` unions stay public (variants may be renamed/merged per the plan).

VALIDATION: run `pnpm check:cs` after the change. The build must pass with zero warnings (TreatWarningsAsErrors is set). If a check fails, fix the root cause — never disable a check, never swallow a warning.

OUTPUT: Make the edits directly. Do not write a plan or a report. At the end, report: (a) which files you touched with line-delta counts, (b) the final `pnpm check:cs` exit status, (c) any deviations from the phase plan and why.

<EXTRA_CONSTRAINTS>
```

### 6.2 Review agent prompt

```
You are reviewing Phase <N> of the Rasm domain unification refactor. The implementation agent has just landed changes. Your job is to read the plan in full, read the coding-csharp skill in full, audit the diff and the final state, and FIX any deviations directly — do not produce a report and stop.

MANDATORY READING (this order, no skipping):
1. /Users/bardiasamiee/.claude/plans/you-are-an-orchestrator-serialized-flask.md  (full)
2. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/SKILL.md
3. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/patterns.md  (anti-pattern codex)
4. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/objects.md
5. /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/validation.md
6. /Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md
7. `git diff main -- <files_in_scope>` and read every changed file in full

PHASE <N> SCOPE: <SCOPE_SUMMARY>

REVIEW CHECKLIST — every item below must be true. Fix any failure by editing the source directly. Do not file a TODO; do not ask the orchestrator; do not leave the issue for "next phase".

GENERAL (apply to every phase):
- No `if`/`else`/`for`/`while`/`foreach`/`try`/`catch`/`throw` in domain logic.
- No `var` anywhere except inside boundary adapters explicitly marked `// BOUNDARY ADAPTER — reason`.
- Every domain method has named parameters at its call sites (LINQ single-arg lambdas exempt).
- Zero new helper files (`*Helper`, `*Util`, `Helpers/`, `Utils/`, `Common/`, `Handlers/`, `Config/`, `Misc/`, `Dispatch_Tables/`).
- Zero single-call private extracted functions — inline at site.
- Zero deprecated/legacy stubs, zero compatibility shims, zero parallel encodings of the same concept.
- Zero TODO/FIXME/HACK/XXX comments introduced by this phase.
- Zero new package references not already in Directory.Build.props.
- Section headers in canonical order with col-90 separator format.
- Every modified file complies with the LOC budget in the plan (treat as a strong signal, not a hard wall — exceed by <20% only with a one-line justification comment).
- No `private`/`internal` declaration that is consumed from exactly one call site survives.

ANTI-PATTERN HUNT (skill `patterns.md` codex — flag and fix):
- `GOD_FUNCTION`: any switch >6 arms in domain code; any `(typeof, typeof) switch` outside a single boundary adapter.
- `DUAL_CANONICAL_SHAPE`: two encodings/representations of the same concept (e.g. a Curves variant whose label duplicates a CurveFeature enum value).
- `API_SURFACE_INFLATION`: sibling methods that differ only by overload arity or by typed-output parameter.
- `DENSITY_OVER_VOLUME`: repetitive switch arms with near-identical bodies — collapse via a fold or extension.
- `HELPER_SPAM`: any `private static Fin<T>` called from exactly one site.
- `INTERFACE_POLLUTION`: any `I*` interface with a single implementation introduced this phase.
- `OVERLOAD_SPAM`: more than two overloads of the same conceptual operation.
- `VARIABLE_REASSIGNMENT`: any rebound local in pipeline code.
- `EXCEPTION_CONTROL_FLOW`: any `throw` outside a boundary adapter.
- `PHANTOM_BYPASS`: any value object/struct with a public constructor bypassing TryCreate/Validate.
- `EARLY_RETURN_GUARDS`: any sequential null/range/type guard expressed as `if (x) return Fin.Fail; if (y) return Fin.Fail; ...` — replace with applicative `Validation`.
- `LINQ_HOT_PATH`: any `.Where().Sum()`-style chain inside an inner loop — for now, only flag if introduced this phase; do not pre-optimise.
- `PREMATURE_MATCH_COLLAPSE`: any `.Match(Succ, Fail)` mid-pipeline — must be replaced by `.Map`/`.Bind`/`.BiMap`.
- `INTERFACE_POLLUTION`: see above.
- `CLOSURE_CAPTURE_HOT_PATH`: any non-static lambda inside an Eff/Fin chain that captures an outer variable — replace with `static` lambda + tuple state.

PHASE-SPECIFIC CHECKS: <REVIEW_CHECKLIST>

FINAL GATE: `pnpm check:cs` must pass with zero warnings. Re-run it after every fix. If it fails, diagnose root cause; never `--no-verify`, never lower analyzer severity, never silence a diagnostic.

When everything is green, report exactly: (a) the number of issues you found and fixed, by category; (b) the final `pnpm check:cs` exit status; (c) the final LOC of each touched file. Nothing else.
```

## 7. Verification

The only verification gate is `pnpm check:cs`, run after each phase. Per `Directory.Build.props:31` the project sets `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-all`, so any analyzer warning fails the build. The local C# analyzer (`tools/cs-analyzer/CsAnalyzer.csproj`) plus the workspace boundary contracts (`BoundaryContracts.cs`) are automatically wired by `Directory.Build.props`.

End-to-end smoke (manual, post-Phase 6, optional):
1. `pnpm exec nx run-many -t build --projects=Rasm,Rasm.Grasshopper,Radyab` (or the equivalent dotnet build if Nx targets are not yet wired — `Workspace.slnx` will still validate).
2. Open RhinoWIP, load the Radyab `.rhp`, drag each of the four Extract components onto a Grasshopper2 canvas, wire a Brep, Mesh, Curve, and Surface respectively, confirm outputs flow without errors.

No new test files are added (none exist in the project today; testing infrastructure is out of scope for this refactor per the user's explicit "Keep validation simple, just run quality check for cs" directive).

## 8. Risks and Mitigations

| Risk | Likelihood | Mitigation |
| --- | --- | --- |
| Domain.Geometry.cs exceeds 250 LOC after Coercion table grows. | Medium | Table is data, not code; if > 250 hit, split `Coercion` into its own internal class (~30 LOC) inside the same file. Implementation agent has explicit budget tolerance in §5 footnote. |
| LanguageExt v5 beta drift breaks `[Union].Match` codegen. | Low | The current codebase already depends on this surface (8 unions today); we are not adopting any newer pre-release. |
| Method-bearing union arms incur boxing under hot paths. | Low-Med | The Eff/Fin pipelines already boxe via `object value` parameters. Hot paths exist only in Spatial.cs (RTree fold) — untouched. Performance-tune later if profiling demands. |
| Coercion table's reflection-based `(Type, Type)` lookup regresses for inherited Rhino types. | Medium | The Rhino reflection bug already discovered for `LanguageExt v5 Map<Type, _>` — `FrozenDictionary` is the established workaround and preserved. Inheritance fallback is explicit in `KindLookup.Inherits`. |
| Implementation agent introduces split-brain by leaving a `BoundsRole.Apply` stub that delegates to the new union method. | High at first contact | Review agent's `HELPER_SPAM` + `DUAL_CANONICAL_SHAPE` checks fail the phase; review agent fixes by deletion. |
| Hidden Grasshopper output-shape dependency on a now-renamed `Op` constant string. | Low | The `Op.Of([CallerMemberName])` factory preserves the *method name* as the Op key. Method names are unchanged. |

## 9. Further Considerations

- **Source-generator interaction**: Thinktecture's `[SmartEnum<T>]`, `[Union]`, and `[ValueObject<T>]` all emit additional partial members at compile time. The 245-LOC budget for `Geometry.cs` is **hand-written lines only** — generated code does not count and does not appear in `wc -l`. The implementation agent should verify generated `Map`/`Switch` overloads cover every variant arm, not lean on default cases.
- **Rhino 9 WIP type coverage**: SubD / PointCloud / Extrusion / Hatch are now first-class `Kind` instances. Today's Analysis layer ignores them silently (everything falls through to `Native`). With the new design, Grasshopper components that wire any of these geometry types into an Extract component will succeed instead of returning empty `Sourced.Vacant` — a behavioral upgrade, not a break. Verify with manual smoke if/when those types become Radyab inputs.
- **Forward expansion cost**: Adding `Analysis/Optimization/Optimization.cs` (say, gradient-descent objectives over points sampled from `Kind.Centroid`) costs **one** new method-bearing `[Union]`, **one** new `public static Query<TG, TOut> Optimize<TG, TOut>(Optimization aspect)` factory, and **zero** new `Op` constants, **zero** new `*Role` files. The orchestrator should confirm this O(1) growth invariant by re-running the LOC delta after the first new analysis folder lands; if a sibling planner ever proposes a new `*Role` file or a new static `Op` block, that is a regression signal.
