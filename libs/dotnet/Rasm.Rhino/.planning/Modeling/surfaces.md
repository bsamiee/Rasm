# [RASM_RHINO_MODELING_SURFACES]

`HostSurfaces.Build` owns freeform surface construction. One `FreeformOp` union carries network fitting, rail revolve, point-grid interpolation, ruled and corner surfaces, curve-on-surface fitting, subd-friendly rebuilds, analytic seeding, compatibility reparameterization, iso-edge matching, extrusion, periodic closure, soft editing, rolling-ball fillets, tween sampling, sum surfaces, bounded planes, and value-semantic fit/rebuild. Input shape selects each overload family. Kernel NURBS evaluation, division, tessellation, and analysis stay kernel-owned; `Context` supplies every tolerance.

Kernel `Rasm.Parametric` owns the bare names `Surfaces` and `SurfaceOp`, so this boundary spells `HostSurfaces` and `FreeformOp` exactly as the curve rail spells `HostCurves`. `ModelGate` comes from `Modeling/solids.md`; `ModelClaim` and `PairPosture` come from `Modeling/curves.md`.

## [01]-[INDEX]

- [02]-[FIT_POLICY]: `NetContinuity`, `ParametricAxis`, `SurfaceForm`, the construction-discriminant unions, and the four admitted law carriers.
- [03]-[OPERATION_RAIL]: `FreeformOp` and the `HostSurfaces.Build` entry.

## [02]-[FIT_POLICY]

- Owner: `NetContinuity` and `ParametricAxis` — the native continuity and parametric-direction vocabularies; `SurfaceForm` — the analytic constructor rows; `NetworkLaw`, `GridFit`, `CornerSeed`, `SurfaceFitLaw`, `ExtrudeTerminal`, `RollingSeed`, `AnalyticSeed`, `PlaneFrame`, `RevolveProfile`, and `SumExtent` — the construction discriminants; `SurfaceDegrees` and `SurfaceGrid` — the host-bounded degree pair and the point-count grid built on it; `MatchEdgeLaw`, `SoftEditLaw`, and `VariableOffsetLaw` — the three multi-scalar policies admitted at construction.
- Law: the continuity code never travels bare — `NetContinuity` keys the native integer so a network arm reads `(int)row`, and an out-of-vocabulary code is unconstructible.
- Law: `ParametricAxis` carries its own `Native` column and every host direction argument reads it — `Surface.CreatePeriodicSurface`, `Surface.RebuildOneDirection`, and the solid rail's `Brep.ChangeSeam` all take the host's declared `0 = U, 1 = V` encoding off the row, so a key renumber can never silently mis-drive a native and the encoding is declared once instead of riding an ordinal cast at three call sites.
- Law: a closed-axis choice is a `CapabilitySet`, so membership needs no probe — a `[SmartEnum]` value is one of its own declared rows by construction, which made every `Items.Contains` gate on this page a guard answering true on every reachable input, while a raw `FrozenSet` column compares by REFERENCE under record equality. `CapabilitySet` closes both: the roster IS the type and the held set compares by value.
- Law: the degree band is an admitted value, not a pair of free ints — RhinoCommon clamps each degree to `Math.Min(degree, 11)` inside `CreateFromPoints` and `CreateThroughPoints`, so `SurfaceDegrees` admits at the host's own ceiling and refuses what the host otherwise clamps in silence; `SurfaceGrid` carries the point counts beside the degrees they must exceed, so the grid, the plane grid, and the grid rebuild read ONE owner and no arm re-derives the count-versus-degree relation.
- Law: one analytic vocabulary serves two representations — `AnalyticSeed.Build` dispatches the primitive once, while each `SurfaceForm` row supplies the four constructor delegates through `[UseDelegateFromConstructor]`; neither axis reconstructs the other.
- Law: the rolling-ball flip pair is ONE `PairPosture` value — the host reads two adjacent side bools and a row-valued pair makes them untransposable at the call, while `Auto` stays its own case because the automatic overload picks the sides itself rather than passing `false` twice.
- Law: finiteness is not non-degeneracy — a direction that is finite can still be the zero vector, so every direction axis reads the kernel's `ValidityClaim.Direction` row and no owner re-spells `IsValid && !IsZero`.
- Law: every owner answers ONE admission fold through `IValidityEvidence` — the generated factory hook and `IsValid` read the same static `Admits`, so an invalid law is unconstructible and `FreeformOp.Admitted` proves presence rather than re-testing content.
- Growth: a new construction discriminant is one union case with its claim; a new scalar policy is one column on its owning law with the claim beside it.
- Packages: RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — the nurbs-surface build roster `:63-80` incl. `CreateNetworkSurface`, `CreateFromPoints`, `CreateThroughPoints`, `CreateFromCorners`, `CreateRuledSurface`, `CreateSubDFriendly`, `CreateRailRevolvedSurface`, `MakeCompatible`, `MatchToCurve`, `CreateCurveOnSurface`, `CreateFromPlane`, `CreateFromCone`/`Cylinder`/`Sphere`/`Torus`; the surface build roster `:82-103` incl. `CreateExtrusion`, `CreateExtrusionToPoint`, `CreatePeriodicSurface`, `CreateSoftEditSurface`, `CreateRollingBallFillet`, `CreateTweenSurfacesWithSampling`, `Fit`, `Rebuild`, `RebuildOneDirection`, `VariableOffset`, `RevSurface.Create`, `SumSurface.Create`, `PlaneSurface.CreateThroughBox`), kernel `Domain/rails` (`Op`, `ValidityClaim`, `IValidityEvidence`, `Fin`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Modeling/curves.md` (`PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NetContinuity {
    public static readonly NetContinuity Loose = new(key: 0);
    public static readonly NetContinuity Position = new(key: 1);
    public static readonly NetContinuity Tangent = new(key: 2);
    public static readonly NetContinuity Curvature = new(key: 3);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParametricAxis : ICapability<ParametricAxis> {
    public static readonly ParametricAxis U = new(key: "u", native: 0);
    public static readonly ParametricAxis V = new(key: "v", native: 1);

    internal int Native { get; }
}

[SmartEnum<int>]
public sealed partial class SurfaceForm {
    public static readonly SurfaceForm Nurbs = new(
        key: 0,
        buildCone: static value => NurbsSurface.CreateFromCone(cone: value),
        buildCylinder: static value => NurbsSurface.CreateFromCylinder(cylinder: value),
        buildSphere: static value => NurbsSurface.CreateFromSphere(sphere: value),
        buildTorus: static value => NurbsSurface.CreateFromTorus(torus: value));
    public static readonly SurfaceForm Revolved = new(
        key: 1,
        buildCone: static value => RevSurface.CreateFromCone(cone: value),
        buildCylinder: static value => RevSurface.CreateFromCylinder(cylinder: value),
        buildSphere: static value => RevSurface.CreateFromSphere(sphere: value),
        buildTorus: static value => RevSurface.CreateFromTorus(torus: value));

    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildCone(Cone value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildCylinder(Cylinder value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildSphere(Sphere value);
    [UseDelegateFromConstructor]
    internal partial GeometryBase? BuildTorus(Torus value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NetworkLaw : IValidityEvidence {
    private NetworkLaw() { }
    public sealed record Auto(Seq<GeometryHandle> Curves, NetContinuity Continuity) : NetworkLaw;
    public sealed record Uv(
        Seq<GeometryHandle> UCurves, NetContinuity UStart, NetContinuity UEnd,
        Seq<GeometryHandle> VCurves, NetContinuity VStart, NetContinuity VEnd) : NetworkLaw;

    public bool IsValid => Switch(
        auto: static law => ValidityClaim.All(
            ModelClaim.Handles(handles: law.Curves), law.Continuity is not null),
        uv: static law => ValidityClaim.All(
            ModelClaim.Handles(handles: law.UCurves), ModelClaim.Handles(handles: law.VCurves),
            law.UStart is not null, law.UEnd is not null, law.VStart is not null, law.VEnd is not null));

    internal Fin<NurbsSurface> Build(Context domain, Op key) =>
        Switch(
            state: (Domain: domain, Op: key),
            auto: static (ctx, law) => ModelGate.BorrowMany<Curve, NurbsSurface>(
                handles: law.Curves, key: ctx.Op, body: curves => Captured(ctx.Op, () => {
                    NurbsSurface? product = NurbsSurface.CreateNetworkSurface(
                        curves: curves.AsIterable(), continuity: (int)law.Continuity,
                        edgeTolerance: ctx.Domain.Absolute.Value, interiorTolerance: ctx.Domain.Absolute.Value,
                        angleTolerance: ctx.Domain.Angle.Value, error: out int error);
                    return (Product: product, Error: error);
                })),
            uv: static (ctx, law) => ModelGate.BorrowMany<Curve, NurbsSurface>(
                handles: law.UCurves, key: ctx.Op, body: uCurves =>
                ModelGate.BorrowMany<Curve, NurbsSurface>(handles: law.VCurves, key: ctx.Op, body: vCurves =>
                    Captured(ctx.Op, () => {
                        NurbsSurface? product = NurbsSurface.CreateNetworkSurface(
                            uCurves: uCurves.AsIterable(), uContinuityStart: (int)law.UStart, uContinuityEnd: (int)law.UEnd,
                            vCurves: vCurves.AsIterable(), vContinuityStart: (int)law.VStart, vContinuityEnd: (int)law.VEnd,
                            edgeTolerance: ctx.Domain.Absolute.Value, interiorTolerance: ctx.Domain.Absolute.Value,
                            angleTolerance: ctx.Domain.Angle.Value, error: out int error);
                        return (Product: product, Error: error);
                    }))));

    private static Fin<NurbsSurface> Captured(
        Op key,
        Func<(NurbsSurface? Product, int Error)> build) =>
        key.Catch(() => {
            (NurbsSurface? product, int error) = build();
            return product is not null && error == 0
                ? Fin.Succ(value: product)
                : Fin.Fail<NurbsSurface>(
                    error: key.InvalidResult(detail: error.ToString(CultureInfo.InvariantCulture))).Rollback(product);
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridFit : IValidityEvidence {
    private GridFit() { }
    public sealed record Control : GridFit;
    public sealed record Through(CapabilitySet<ParametricAxis> ClosedAxes) : GridFit;

    public bool IsValid => Switch(
        control: static () => (ValidityClaim)true,
        through: static _ => (ValidityClaim)true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CornerSeed : IValidityEvidence {
    private CornerSeed() { }
    public sealed record Triangle(Point3d A, Point3d B, Point3d C) : CornerSeed;
    public sealed record Quad(Point3d A, Point3d B, Point3d C, Point3d D) : CornerSeed;

    public bool IsValid => Switch(
        triangle: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.A), ValidityClaim.Finite(value: seed.B), ValidityClaim.Finite(value: seed.C)),
        quad: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.A), ValidityClaim.Finite(value: seed.B),
            ValidityClaim.Finite(value: seed.C), ValidityClaim.Finite(value: seed.D)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceFitLaw : IValidityEvidence {
    private SurfaceFitLaw() { }
    public sealed record ToTolerance(SurfaceDegrees Degrees) : SurfaceFitLaw;
    public sealed record ToGrid(SurfaceGrid Shape) : SurfaceFitLaw;
    public sealed record InDirection(ParametricAxis Axis, int PointCount, LoftType Kind) : SurfaceFitLaw;

    public bool IsValid => Switch(
        toTolerance: static law => (ValidityClaim)law.Degrees.IsValid,
        toGrid: static law => (ValidityClaim)law.Shape.IsValid,
        inDirection: static law => ValidityClaim.All(
            law.Axis is not null,
            ValidityClaim.CountAtLeast(count: law.PointCount, floor: 1),
            Enum.IsDefined(law.Kind)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrudeTerminal : IValidityEvidence {
    private ExtrudeTerminal() { }
    public sealed record Along(Vector3d Direction) : ExtrudeTerminal;
    public sealed record ToApex(Point3d Apex) : ExtrudeTerminal;

    public bool IsValid => Switch(
        along: static terminal => ValidityClaim.Direction(value: terminal.Direction),
        toApex: static terminal => ValidityClaim.Finite(value: terminal.Apex));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RollingSeed : IValidityEvidence {
    private RollingSeed() { }
    public sealed record Auto : RollingSeed;
    public sealed record Flipped(PairPosture Flip) : RollingSeed;
    public sealed record AtUv(Point2d First, Point2d Second) : RollingSeed;

    public bool IsValid => Switch(
        auto: static () => (ValidityClaim)true,
        flipped: static seed => (ValidityClaim)(seed.Flip is not null),
        atUv: static seed => ValidityClaim.All(seed.First.IsValid, seed.Second.IsValid));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticSeed : IValidityEvidence {
    private AnalyticSeed() { }
    public sealed record OfCone(Cone Value) : AnalyticSeed;
    public sealed record OfCylinder(Cylinder Value) : AnalyticSeed;
    public sealed record OfSphere(Sphere Value) : AnalyticSeed;
    public sealed record OfTorus(Torus Value) : AnalyticSeed;

    public bool IsValid => Switch(
        ofCone: static seed => (ValidityClaim)seed.Value.IsValid,
        ofCylinder: static seed => (ValidityClaim)seed.Value.IsValid,
        ofSphere: static seed => (ValidityClaim)seed.Value.IsValid,
        ofTorus: static seed => (ValidityClaim)seed.Value.IsValid);

    internal GeometryBase? Build(SurfaceForm form) => Switch(
        state: form,
        ofCone: static (surfaceForm, seed) => surfaceForm.BuildCone(seed.Value),
        ofCylinder: static (surfaceForm, seed) => surfaceForm.BuildCylinder(seed.Value),
        ofSphere: static (surfaceForm, seed) => surfaceForm.BuildSphere(seed.Value),
        ofTorus: static (surfaceForm, seed) => surfaceForm.BuildTorus(seed.Value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlaneFrame : IValidityEvidence {
    private PlaneFrame() { }
    public sealed record OfPlane(Plane Value) : PlaneFrame;
    public sealed record OfLine(Line LineInPlane, Vector3d VectorInPlane) : PlaneFrame;

    public bool IsValid => Switch(
        ofPlane: static frame => (ValidityClaim)frame.Value.IsValid,
        ofLine: static frame => ValidityClaim.All(
            frame.LineInPlane.IsValid, ValidityClaim.Direction(value: frame.VectorInPlane)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RevolveProfile : IValidityEvidence {
    private RevolveProfile() { }
    public sealed record OfCurve(GeometryHandle Value) : RevolveProfile;
    public sealed record OfLine(Line Value) : RevolveProfile;

    public bool IsValid => Switch(
        ofCurve: static profile => ModelClaim.Handle(handle: profile.Value),
        ofLine: static profile => (ValidityClaim)profile.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SumExtent : IValidityEvidence {
    private SumExtent() { }
    public sealed record ByDirection(Vector3d Direction) : SumExtent;
    public sealed record ByCurve(GeometryHandle Second) : SumExtent;

    public bool IsValid => Switch(
        byDirection: static extent => ValidityClaim.Direction(value: extent.Direction),
        byCurve: static extent => ModelClaim.Handle(handle: extent.Second));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SurfaceDegrees : IValidityEvidence {
    internal const int Ceiling = 11;

    public int U { get; }
    public int V { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int u, ref int v) {
        if (!Admits(u, v)) {
            validationError = new ValidationError("Surface degrees fall outside the host's admitted band.");
        }
    }

    public bool IsValid => Admits(U, V);

    private static bool Admits(int u, int v) =>
        ValidityClaim.All(u is > 0 and <= Ceiling, v is > 0 and <= Ceiling);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SurfaceGrid : IValidityEvidence {
    public SurfaceDegrees Degrees { get; }
    public int UPoints { get; }
    public int VPoints { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref SurfaceDegrees degrees, ref int uPoints, ref int vPoints) {
        if (!Admits(degrees, uPoints, vPoints)) {
            validationError = new ValidationError("Surface grid counts do not exceed their degrees.");
        }
    }

    public bool IsValid => Admits(Degrees, UPoints, VPoints);

    internal long Count => (long)UPoints * VPoints;

    private static bool Admits(SurfaceDegrees degrees, int uPoints, int vPoints) =>
        ValidityClaim.All(
            degrees.IsValid,
            ValidityClaim.CountAtLeast(count: uPoints, floor: degrees.U + 1),
            ValidityClaim.CountAtLeast(count: vPoints, floor: degrees.V + 1));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct MatchEdgeLaw : IValidityEvidence {
    public IsoStatus Side { get; }
    public double MaxEndDistance { get; }
    public double MaxInteriorDistance { get; }
    public int MaxLevel { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref IsoStatus side,
        ref double maxEndDistance, ref double maxInteriorDistance, ref int maxLevel) {
        if (!Admits(side, maxEndDistance, maxInteriorDistance, maxLevel)) {
            validationError = new ValidationError("Iso-edge match policy carries a negative bound or an unrostered side.");
        }
    }

    public bool IsValid => Admits(Side, MaxEndDistance, MaxInteriorDistance, MaxLevel);

    private static bool Admits(IsoStatus side, double maxEndDistance, double maxInteriorDistance, int maxLevel) =>
        ValidityClaim.All(
            Enum.IsDefined(side),
            ValidityClaim.Nonnegative(value: maxEndDistance),
            ValidityClaim.Nonnegative(value: maxInteriorDistance),
            ValidityClaim.CountAtLeast(count: maxLevel, floor: 0));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SoftEditLaw : IValidityEvidence {
    public Point2d Uv { get; }
    public Vector3d Delta { get; }
    public double ULength { get; }
    public double VLength { get; }
    public bool FixEnds { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Point2d uv, ref Vector3d delta,
        ref double uLength, ref double vLength, ref bool fixEnds) {
        if (!Admits(uv, delta, uLength, vLength)) {
            validationError = new ValidationError("Soft-edit falloff lengths must be positive at a valid uv.");
        }
    }

    public bool IsValid => Admits(Uv, Delta, ULength, VLength);

    private static bool Admits(Point2d uv, Vector3d delta, double uLength, double vLength) =>
        ValidityClaim.All(
            uv.IsValid,
            ValidityClaim.Finite(value: delta),
            ValidityClaim.Positive(value: uLength),
            ValidityClaim.Positive(value: vLength));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct VariableOffsetLaw : IValidityEvidence {
    public double UMinVMin { get; }
    public double UMinVMax { get; }
    public double UMaxVMin { get; }
    public double UMaxVMax { get; }
    public Seq<(Point2d Uv, double Distance)> Interior { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double uMinVMin, ref double uMinVMax,
        ref double uMaxVMin, ref double uMaxVMax, ref Seq<(Point2d Uv, double Distance)> interior) {
        if (!Admits(uMinVMin, uMinVMax, uMaxVMin, uMaxVMax, interior)) {
            validationError = new ValidationError("Variable-offset distances are not finite.");
        }
    }

    public bool IsValid => Admits(UMinVMin, UMinVMax, UMaxVMin, UMaxVMax, Interior);

    private static bool Admits(
        double uMinVMin, double uMinVMax, double uMaxVMin, double uMaxVMax,
        Seq<(Point2d Uv, double Distance)> interior) =>
        ValidityClaim.All(
            ValidityClaim.Finite(values: [uMinVMin, uMinVMax, uMaxVMin, uMaxVMax]),
            ModelClaim.Rows(
                rows: interior,
                claim: static row => ValidityClaim.All(row.Uv.IsValid, ValidityClaim.Finite(value: row.Distance)),
                allowEmpty: true));
}
```

## [03]-[OPERATION_RAIL]

- Owner: `FreeformOp` `[Union]` `[GenerateUnionOps]` — the whole verified freeform-construction verb roster, each case carrying its own generated `SelfOp`; `HostSurfaces` — the one entry folding any operation spread into one owned geometry sequence.
- Law: the entry class holds the entry ALONE — the degree ceiling, the degree predicate, the grid-shape predicate, and the roster-membership probe that once sat beside `Build` are now the `SurfaceDegrees`/`SurfaceGrid` admission and the `CapabilitySet` type, so this page's static class matches every sibling rail's one-member shape.
- Law: `NetworkLaw.Build` captures both native topologies and refuses a null product or a nonzero error code before ownership.
- Law: admission NAMES its axis — `Admitted` dispatches the generated `Switch` into the spine's `ModelClaim.Admits`, so a request breaching several constraints answers one keyed fault per breached axis, and a new case breaks the compile instead of falling through a catch-all to a silent refusal.
- Law: `HostSurfaces.Build` is `ModelGate.Entry` — the folder spine owns capture, the non-empty guard, accumulating admission, and the product fold.
- Law: minting is spine-owned — every arm reaches its products through `ModelGate.Single`/`Many`/`Staged`, including arms that already hold a built native.
- Law: geodesic fitting returns the fitted `NurbsCurve`; intermediate uv samples remain internal to the native algorithm.
- Law: fit and rebuild are value-semantic constructions — `SurfaceFitLaw` selects tolerance fit, grid rebuild, or directional rebuild inside one `Fit` arm, and each member owns the returned surface without mutating the input handle.
- Law: compatibility answers pairs — `MakeCompatible` confirms and crosses both reparameterized surfaces inside one guarded custody scope, so a half-crossed failure releases both.
- Law: variable offsetting is corner-driven construction — the four corner distances and the optional interior rows are one admitted law, the row spread selects the host overload, and the offset tolerance derives from the domain absolute tolerance, never a payload literal.
- Law: solitary independent bits stay bools — `ScaleHeight`, `Periodic`, and `Smooth` each carry one axis with no legal-corner law and no host projection column, so a capability set over them buys a wire name and loses the compile-time reading.
- Growth: a new freeform constructor is one `FreeformOp` case with its arm.
- Packages: RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — nurbs-surface build `:63-80`, surface build `:82-103`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), `Modeling/solids.md` (`ModelGate`), kernel `Domain/rails` (`Op`, `[GenerateUnionOps]` + generated `SelfOp`, `ValidityClaim`, `Fin`), kernel `Domain/context` (`Context.Absolute`, `Context.Angle`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
[GenerateUnionOps]
public abstract partial record FreeformOp {
    private FreeformOp() { }
    public sealed record Network(NetworkLaw Law) : FreeformOp;
    public sealed record RailRevolve(GeometryHandle Profile, GeometryHandle Rail, Line Axis, bool ScaleHeight = false) : FreeformOp;
    public sealed record Grid(Seq<Point3d> Points, SurfaceGrid Shape, GridFit Fit) : FreeformOp;
    public sealed record Corners(CornerSeed Seed) : FreeformOp;
    public sealed record Ruled(GeometryHandle First, GeometryHandle Second) : FreeformOp;
    public sealed record GeodesicCurve(GeometryHandle Surface, Seq<Point2d> Points, bool Periodic = false) : FreeformOp;
    public sealed record SubDFriendly(GeometryHandle Surface) : FreeformOp;
    public sealed record Seed(AnalyticSeed Value, SurfaceForm Form) : FreeformOp;
    public sealed record PlaneGrid(Plane Frame, Interval U, Interval V, SurfaceGrid Shape) : FreeformOp;
    public sealed record Compatible(GeometryHandle First, GeometryHandle Second) : FreeformOp;
    public sealed record MatchEdge(GeometryHandle Surface, GeometryHandle TargetCurve, MatchEdgeLaw Law) : FreeformOp;
    public sealed record Extruded(GeometryHandle Profile, ExtrudeTerminal Terminal) : FreeformOp;
    public sealed record Periodic(GeometryHandle Surface, ParametricAxis Axis, bool Smooth = true) : FreeformOp;
    public sealed record SoftEdit(GeometryHandle Surface, SoftEditLaw Law) : FreeformOp;
    public sealed record RollingBall(GeometryHandle First, GeometryHandle Second, double Radius, RollingSeed At) : FreeformOp;
    public sealed record Tween(GeometryHandle First, GeometryHandle Second, int Count, int Samples) : FreeformOp;
    public sealed record Sum(GeometryHandle Profile, SumExtent Extent) : FreeformOp;
    public sealed record BoundedPlane(PlaneFrame Frame, BoundingBox Box) : FreeformOp;
    public sealed record Revolve(RevolveProfile Profile, Line Axis, Option<(double StartRadians, double EndRadians)> Sweep = default) : FreeformOp;
    public sealed record Fit(GeometryHandle Surface, SurfaceFitLaw Law) : FreeformOp;
    public sealed record VariableOffset(GeometryHandle Surface, VariableOffsetLaw Law) : FreeformOp;

    internal Fin<FreeformOp> Admitted(Op key) =>
        Switch(
            context: key,
            network: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Law), row.Law is { IsValid: true })),
            railRevolve: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)), (nameof(row.Axis), row.Axis.IsValid)),
            grid: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Shape), row.Shape.IsValid), (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Points), ValidityClaim.All(
                    ModelClaim.Points(points: row.Points), row.Points.Count == row.Shape.Count))),
            corners: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Seed), row.Seed is { IsValid: true })),
            ruled: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second))),
            geodesicCurve: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)),
                (nameof(row.Points), ModelClaim.Rows(rows: row.Points, claim: static point => (ValidityClaim)point.IsValid))),
            subDFriendly: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface))),
            seed: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value is { IsValid: true }), (nameof(row.Form), row.Form is not null)),
            planeGrid: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Frame), row.Frame.IsValid),
                (nameof(row.U), row.U.IsValid), (nameof(row.V), row.V.IsValid),
                (nameof(row.Shape), row.Shape.IsValid)),
            compatible: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second))),
            matchEdge: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)),
                (nameof(row.TargetCurve), ModelClaim.Handle(handle: row.TargetCurve)),
                (nameof(row.Law), row.Law.IsValid)),
            extruded: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Terminal), row.Terminal is { IsValid: true })),
            periodic: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)), (nameof(row.Axis), row.Axis is not null)),
            softEdit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)), (nameof(row.Law), row.Law.IsValid)),
            rollingBall: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Radius), ValidityClaim.Positive(value: row.Radius)),
                (nameof(row.At), row.At is { IsValid: true })),
            tween: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Count), ValidityClaim.CountAtLeast(count: row.Count, floor: 1)),
                (nameof(row.Samples), ValidityClaim.CountAtLeast(count: row.Samples, floor: 1))),
            sum: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), ModelClaim.Handle(handle: row.Profile)),
                (nameof(row.Extent), row.Extent is { IsValid: true })),
            boundedPlane: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Frame), row.Frame is { IsValid: true }), (nameof(row.Box), row.Box.IsValid)),
            revolve: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Profile), row.Profile is { IsValid: true }), (nameof(row.Axis), row.Axis.IsValid),
                (nameof(row.Sweep), ValidityClaim.WhenPresent(
                    facet: row.Sweep,
                    claim: static sweep => ValidityClaim.Finite(values: [sweep.StartRadians, sweep.EndRadians])))),
            fit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)), (nameof(row.Law), row.Law is { IsValid: true })),
            variableOffset: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)), (nameof(row.Law), row.Law.IsValid)));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain) =>
        Switch(
            context: domain,
            network: static (model, edit) => edit.Law.Build(domain: model, key: Network.SelfOp)
                .Bind(result => ModelGate.Single(Network.SelfOp, () => result)),
            railRevolve: static (_, edit) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                handle: edit.Profile, key: RailRevolve.SelfOp, body: profile =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Rail, key: RailRevolve.SelfOp, body: rail =>
                        ModelGate.Single(RailRevolve.SelfOp, () => NurbsSurface.CreateRailRevolvedSurface(
                            profile: profile, rail: rail, axis: edit.Axis, scaleHeight: edit.ScaleHeight)))),
            grid: static (_, edit) => edit.Fit.Switch(
                state: edit,
                control: static row => ModelGate.Single(Grid.SelfOp, () => NurbsSurface.CreateFromPoints(
                    points: row.Points.AsIterable(), uCount: row.Shape.UPoints, vCount: row.Shape.VPoints,
                    uDegree: row.Shape.Degrees.U, vDegree: row.Shape.Degrees.V)),
                through: static (row, fit) => ModelGate.Single(Grid.SelfOp, () => NurbsSurface.CreateThroughPoints(
                    points: row.Points.AsIterable(), uCount: row.Shape.UPoints, vCount: row.Shape.VPoints,
                    uDegree: row.Shape.Degrees.U, vDegree: row.Shape.Degrees.V,
                    uClosed: fit.ClosedAxes.Admits(capability: ParametricAxis.U),
                    vClosed: fit.ClosedAxes.Admits(capability: ParametricAxis.V)))),
            corners: static (model, edit) => edit.Seed.Switch(
                state: model,
                triangle: static (_, seed) => ModelGate.Single(Corners.SelfOp, () => NurbsSurface.CreateFromCorners(
                    corner1: seed.A, corner2: seed.B, corner3: seed.C)),
                quad: static (regime, seed) => ModelGate.Single(Corners.SelfOp, () => NurbsSurface.CreateFromCorners(
                    corner1: seed.A, corner2: seed.B, corner3: seed.C, corner4: seed.D,
                    tolerance: regime.Absolute.Value))),
            ruled: static (_, edit) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                handle: edit.First, key: Ruled.SelfOp, body: first =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Second, key: Ruled.SelfOp, body: second =>
                        ModelGate.Single(Ruled.SelfOp,
                            () => NurbsSurface.CreateRuledSurface(curveA: first, curveB: second)))),
            geodesicCurve: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: GeodesicCurve.SelfOp, body: surface =>
                    ModelGate.Single(GeodesicCurve.SelfOp, () => NurbsSurface.CreateCurveOnSurface(
                        surface: surface, points: edit.Points.AsIterable(),
                        tolerance: model.Absolute.Value, periodic: edit.Periodic))),
            subDFriendly: static (_, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: SubDFriendly.SelfOp, body: surface =>
                    ModelGate.Single(SubDFriendly.SelfOp,
                        () => NurbsSurface.CreateSubDFriendly(surface: surface))),
            seed: static (_, edit) => ModelGate.Single(Seed.SelfOp,
                () => edit.Value.Build(form: edit.Form)),
            planeGrid: static (_, edit) => ModelGate.Single(PlaneGrid.SelfOp,
                () => NurbsSurface.CreateFromPlane(
                    plane: edit.Frame, uInterval: edit.U, vInterval: edit.V,
                    uDegree: edit.Shape.Degrees.U, vDegree: edit.Shape.Degrees.V,
                    uPointCount: edit.Shape.UPoints, vPointCount: edit.Shape.VPoints)),
            compatible: static (_, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.First, key: Compatible.SelfOp, body: first =>
                    ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: edit.Second, key: Compatible.SelfOp, body: second =>
                        Compatible.SelfOp.Catch(() =>
                            ModelGate.Staged(op: Compatible.SelfOp, success: NurbsSurface.MakeCompatible(
                                surface0: first, surface1: second, nurb0: out NurbsSurface nurb0, nurb1: out NurbsSurface nurb1),
                                ((GeometryBase[])[nurb0, nurb1], false))))),
            matchEdge: static (model, edit) => ModelGate.Borrow<NurbsSurface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: MatchEdge.SelfOp, body: surface =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.TargetCurve, key: MatchEdge.SelfOp, body: target =>
                        ModelGate.Single(MatchEdge.SelfOp, () => surface.MatchToCurve(
                            side: edit.Law.Side, targetCurve: target, maxEndDistance: edit.Law.MaxEndDistance,
                            maxInteriorDistance: edit.Law.MaxInteriorDistance, matchTolerance: model.Absolute.Value,
                            maxLevel: edit.Law.MaxLevel)))),
            extruded: static (_, edit) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                handle: edit.Profile, key: Extruded.SelfOp, body: profile =>
                    edit.Terminal.Switch(
                        state: profile,
                        along: static (curve, terminal) => ModelGate.Single(Extruded.SelfOp,
                            () => Surface.CreateExtrusion(profile: curve, direction: terminal.Direction)),
                        toApex: static (curve, terminal) => ModelGate.Single(Extruded.SelfOp,
                            () => Surface.CreateExtrusionToPoint(profile: curve, apexPoint: terminal.Apex)))),
            periodic: static (_, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: Periodic.SelfOp, body: surface =>
                    ModelGate.Single(Periodic.SelfOp, () => Surface.CreatePeriodicSurface(
                        surface: surface, direction: edit.Axis.Native, bSmooth: edit.Smooth))),
            softEdit: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: SoftEdit.SelfOp, body: surface =>
                    ModelGate.Single(SoftEdit.SelfOp, () => Surface.CreateSoftEditSurface(
                        surface: surface, uv: edit.Law.Uv, delta: edit.Law.Delta,
                        uLength: edit.Law.ULength, vLength: edit.Law.VLength,
                        tolerance: model.Absolute.Value, fixEnds: edit.Law.FixEnds))),
            rollingBall: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.First, key: RollingBall.SelfOp, body: first =>
                    ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: edit.Second, key: RollingBall.SelfOp, body: second =>
                        edit.At.Switch(
                            state: (First: first, Second: second, Radius: edit.Radius, Tolerance: model.Absolute.Value),
                            auto: static ctx => ModelGate.Many(RollingBall.SelfOp,
                                () => Surface.CreateRollingBallFillet(
                                    surfaceA: ctx.First, surfaceB: ctx.Second, radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            flipped: static (ctx, seed) => ModelGate.Many(RollingBall.SelfOp,
                                () => Surface.CreateRollingBallFillet(
                                    surfaceA: ctx.First, flipA: seed.Flip.First, surfaceB: ctx.Second, flipB: seed.Flip.Second,
                                    radius: ctx.Radius, tolerance: ctx.Tolerance)),
                            atUv: static (ctx, seed) => ModelGate.Many(RollingBall.SelfOp,
                                () => Surface.CreateRollingBallFillet(
                                    surfaceA: ctx.First, uvA: seed.First, surfaceB: ctx.Second, uvB: seed.Second,
                                    radius: ctx.Radius, tolerance: ctx.Tolerance))))),
            tween: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.First, key: Tween.SelfOp, body: first =>
                    ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: edit.Second, key: Tween.SelfOp, body: second =>
                        ModelGate.Many(Tween.SelfOp, () => Surface.CreateTweenSurfacesWithSampling(
                            surface0: first, surface1: second, numSurfaces: edit.Count,
                            numSamples: edit.Samples, tolerance: model.Absolute.Value)))),
            sum: static (_, edit) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                handle: edit.Profile, key: Sum.SelfOp, body: profile =>
                    edit.Extent.Switch(
                        state: profile,
                        byDirection: static (curve, extent) => ModelGate.Single(Sum.SelfOp,
                            () => SumSurface.Create(curve: curve, extrusionDirection: extent.Direction)),
                        byCurve: static (curve, extent) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                            handle: extent.Second, key: Sum.SelfOp, body: second => ModelGate.Single(
                                Sum.SelfOp, () => SumSurface.Create(curveA: curve, curveB: second))))),
            boundedPlane: static (_, edit) => edit.Frame.Switch(
                state: edit.Box,
                ofPlane: static (box, frame) => ModelGate.Single(BoundedPlane.SelfOp,
                    () => PlaneSurface.CreateThroughBox(plane: frame.Value, box: box)),
                ofLine: static (box, frame) => ModelGate.Single(BoundedPlane.SelfOp,
                    () => PlaneSurface.CreateThroughBox(
                        lineInPlane: frame.LineInPlane, vectorInPlane: frame.VectorInPlane, box: box))),
            revolve: static (_, edit) => edit.Profile.Switch(
                state: edit,
                ofCurve: static (row, profile) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(
                    handle: profile.Value, key: Revolve.SelfOp, body: revolute =>
                        ModelGate.Single(Revolve.SelfOp, () => row.Sweep.Match(
                            Some: sweep => RevSurface.Create(
                                revoluteCurve: revolute, axisOfRevolution: row.Axis,
                                startAngleRadians: sweep.StartRadians, endAngleRadians: sweep.EndRadians),
                            None: () => RevSurface.Create(revoluteCurve: revolute, axisOfRevolution: row.Axis)))),
                ofLine: static (row, profile) => ModelGate.Single(Revolve.SelfOp,
                    () => row.Sweep.Match(
                        Some: sweep => RevSurface.Create(
                            revoluteLine: profile.Value, axisOfRevolution: row.Axis,
                            startAngleRadians: sweep.StartRadians, endAngleRadians: sweep.EndRadians),
                        None: () => RevSurface.Create(revoluteLine: profile.Value, axisOfRevolution: row.Axis)))),
            fit: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: Fit.SelfOp, body: surface =>
                    edit.Law.Switch(
                        state: (Surface: surface, Domain: model),
                        toTolerance: static (ctx, law) => ModelGate.Single(Fit.SelfOp, () => ctx.Surface.Fit(
                            uDegree: law.Degrees.U, vDegree: law.Degrees.V, fitTolerance: ctx.Domain.Absolute.Value)),
                        toGrid: static (ctx, law) => ModelGate.Single(Fit.SelfOp, () => ctx.Surface.Rebuild(
                            uDegree: law.Shape.Degrees.U, vDegree: law.Shape.Degrees.V,
                            uPointCount: law.Shape.UPoints, vPointCount: law.Shape.VPoints)),
                        inDirection: static (ctx, law) => ModelGate.Single(Fit.SelfOp, () => ctx.Surface.RebuildOneDirection(
                            direction: law.Axis.Native, pointCount: law.PointCount, loftType: law.Kind,
                            refitTolerance: ctx.Domain.Absolute.Value)))),
            variableOffset: static (model, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Surface, key: VariableOffset.SelfOp, body: surface =>
                    ModelGate.Single(VariableOffset.SelfOp, () => edit.Law.Interior.IsEmpty
                        ? surface.VariableOffset(
                            uMinvMin: edit.Law.UMinVMin, uMinvMax: edit.Law.UMinVMax,
                            uMaxvMin: edit.Law.UMaxVMin, uMaxvMax: edit.Law.UMaxVMax,
                            tolerance: model.Absolute.Value)
                        : surface.VariableOffset(
                            uMinvMin: edit.Law.UMinVMin, uMinvMax: edit.Law.UMinVMax,
                            uMaxvMin: edit.Law.UMaxVMin, uMaxvMax: edit.Law.UMaxVMax,
                            interiorParameters: edit.Law.Interior.Map(static row => row.Uv).AsEnumerable(),
                            interiorDistances: edit.Law.Interior.Map(static row => row.Distance).AsEnumerable(),
                            tolerance: model.Absolute.Value))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostSurfaces {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<FreeformOp> operations) {
        Seq<FreeformOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(key: key),
                apply: static (operation, model) => operation.Apply(domain: model)).ToEff());
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
