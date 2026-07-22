# [RASM_FAULTS]

`GeometryFault` owns the geometry domain's one failure rail — a closed `[Union]` at the `Rasm.Numerics` root that every `Fin`/`Validation`/`Eff` path across the geometry sub-domains routes through, each case carrying its typed payload and a band-2400 ordinal `Code` lowered into the LanguageExt `Error` rail by `ToError()`. `FaultCluster` resolves a code's owning cluster and namespace by stride arithmetic, and `ParametricStage`/`DevelopmentStage` mint the stage vocabularies here; a reachable domain failure lands as one case in its sibling's sub-band, never a per-page error type.

Each payload discriminant composes from its owning sibling's vocabulary — the fault mints none of its own domain axes — while `ParametricStage`/`DevelopmentStage` mint here because no single Parametric page owns the tier-wide stage axis, every string key binding the shipped `ComparerAccessors.StringOrdinal` accessor once. Band 2400-2449 sits strictly below the AEC `MaterialFault` band 2450, so a telemetry reader banding by code never conflates a geometry failure with the `Rasm.Domain` `Expected`/`Fault` substrate family that neither union absorbs.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: `GeometryFault` closed `[Union]`, `FaultCluster` stride taxonomy, and the `ParametricStage`/`DevelopmentStage` stage vocabularies — typed payloads, `Code`/`Message`/`Cluster` folds, `ToError()` lowering.

## [02]-[FAULT_BAND]

- Owner: `GeometryFault` the closed `[Union]` at the `Rasm.Numerics` root, one case per reachable failure carrying its typed payload and band-2400 `Code`, lowered to the `Error` rail through `ToError()`; `FaultCluster` the `[SmartEnum<int>]` taxonomy resolving a code's cluster name and owning namespace by stride arithmetic with no lookup table beside the vocabulary; `ParametricStage`/`DevelopmentStage` the `StringOrdinal`-keyed stage vocabularies, string-keyed because the stage renders into the wire-bound `Message`.
- Cases: cases sub-band by cluster across the 2400-2449 century — each sibling's cluster owns a four-wide stride, the `parametric` tail spending the final two codes; `DegenerateInput` at the band base is the one cross-cutting admission case every namespace routes, the recorded exception to cluster-locality, and the fence carries the case, code, and payload roster.
- Entry: each case is a positional record constructor returning the union; a sibling routes a failure as `GeometryFault.<Case>(...).ToError()`, the payload matched and read before lowering, `ToError` projecting the band `Code` and the parseable `Message` into the `Error` the `Fin<T>` failure channel carries.
- Auto: `Code`, `Message`, and `Cluster` are total generated folds — a new case breaks every site at compile time, never a silent `_` arm; `Message` renders the `geometry:<case>:<field>=<value>` wire grammar with every keyed discriminant projected through its `Key`; `Cluster` is stride arithmetic over the single `FaultCluster` declaration.
- Receipt: none — `GeometryFault` is the failure rail itself, the terminal value a `Fin<T>` carries; a fault is the residual.
- Packages: Thinktecture.Runtime.Extensions for `[Union]`/`[SmartEnum]` and the `StringOrdinal` accessor, LanguageExt.Core for the `Error`/`Fin` failure channel, BCL `UInt128`.
- Growth: a new reachable failure is one `GeometryFault` case carrying its typed payload and the next free ordinal in its sibling's sub-band; the 2400-2449 century is fully allocated across its clusters, so a genuinely new cluster is a federation re-plan against the AEC materials boundary, never a silent squeeze, and a new stage is one `static readonly` row every stage-reading fold re-proves at compile time.
- Boundary: `GeometryFault` is the one fault union for geometry — a per-cluster `SpatialFault`/`NamingFault` family is the density defect collapsed onto it, the cluster a sub-band not a parallel union; the payload is never a generic `IFault` or an erased `string Detail` where a sibling vocabulary row, index, or measure types the cause, a `string` surviving only as a `Witness` field beside typed discriminants; `try`/`catch` is legal only at a host-numeric or native boundary, never in domain logic; the upward namespace references are legal by the one-assembly law — the kernel compiles as one `Rasm.csproj`, namespaces are cluster routing vocabulary with no build edge, and this root-consolidated union is the recorded exception to strata direction.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Meshing;
using Rasm.Processing;
using Rasm.Spatial;
using Thinktecture;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
// Items order MUST equal Code fold order: (code-2400)>>2 indexes the owning row; the 2-wide parametric tail 2448-2449 both land on index 12.
[SmartEnum<int>]
public sealed partial class FaultCluster {
    public static readonly FaultCluster Spatial          = new(2400, "spatial",          "Rasm.Spatial");
    public static readonly FaultCluster Naming           = new(2404, "naming",           "Rasm.Spatial");
    public static readonly FaultCluster Healing          = new(2408, "healing",          "Rasm.Processing");
    public static readonly FaultCluster Constraints      = new(2412, "constraints",      "Rasm.Solving");
    public static readonly FaultCluster Offsetting       = new(2416, "offsetting",       "Rasm.Meshing");
    public static readonly FaultCluster Arrangement      = new(2420, "arrangement",      "Rasm.Meshing");
    public static readonly FaultCluster Intersection     = new(2424, "intersection",     "Rasm.Meshing");
    public static readonly FaultCluster Fitting          = new(2428, "fitting",          "Rasm.Solving");
    public static readonly FaultCluster Parameterization = new(2432, "parameterization", "Rasm.Processing");
    public static readonly FaultCluster Projection       = new(2436, "projection",       "Rasm.Drawing");
    public static readonly FaultCluster Simplification   = new(2440, "simplification",   "Rasm.Processing");
    public static readonly FaultCluster Encoding         = new(2444, "encoding",         "Rasm.Drawing");
    public static readonly FaultCluster Parametric       = new(2448, "parametric",       "Rasm.Parametric");

    public string Name { get; }
    public string Namespace { get; }

    public static FaultCluster OfCode(int code) => Items[(code - 2400) >> 2];
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParametricStage {
    public static readonly ParametricStage Construction = new("construction");
    public static readonly ParametricStage Evaluation   = new("evaluation");
    public static readonly ParametricStage Station      = new("station");
    public static readonly ParametricStage Offset       = new("offset");
    public static readonly ParametricStage Encode       = new("encode");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DevelopmentStage {
    public static readonly DevelopmentStage Subdivision = new("subdivision");
    public static readonly DevelopmentStage Strip       = new("strip");
    public static readonly DevelopmentStage Panel       = new("panel");
    public static readonly DevelopmentStage Pattern     = new("pattern");
}

// --- [ERRORS] -----------------------------------------------------------------------------
// Record declaration order = Code/Message fold order; both folds total over the union, no silent _ arm.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryFault {
    private GeometryFault() { }

    public sealed record DegenerateInput(Kind Kind, int Index, string Witness) : GeometryFault;
    public sealed record IndexMismatch(EntityKind Kind, int Expected, int Actual) : GeometryFault;
    public sealed record KindMismatch(SpatialKind Index, QueryKind Query) : GeometryFault;

    public sealed record NameCollision(UInt128 Name, int Kind) : GeometryFault;
    public sealed record HashMismatch(UInt128 Name, int Kind) : GeometryFault;

    public sealed record UnrepairableMesh(HealStage Stage, int Iterations, int Remaining) : GeometryFault;

    public sealed record OverConstrained(int RedundantRows, double Residual) : GeometryFault;
    public sealed record SingularSystem(int Rank, int Parameters) : GeometryFault;

    public sealed record DegenerateOffset(int WavefrontVertex, double Time) : GeometryFault;
    public sealed record SkeletonStalled(int PendingEvents, double Time) : GeometryFault;
    public sealed record CollapseStalled(int Iteration, double Residual) : GeometryFault;

    public sealed record DegenerateArrangement(int CellCount, string ManifoldWitness) : GeometryFault;
    public sealed record ConstraintUnrecoverable(int Constraint, int Budget) : GeometryFault;
    public sealed record DegenerateTessellation(int Simplex, string Witness) : GeometryFault;
    public sealed record NativeAssetMissing(string Engine, string Rid, long Ceiling) : GeometryFault;

    public sealed record IntersectionFault(PrimitiveKind A, PrimitiveKind B) : GeometryFault;
    public sealed record SectionFault(int Layer, double Elevation, int OpenChains) : GeometryFault;

    public sealed record FitFault(double AchievedInlierFraction, double Floor) : GeometryFault;

    public sealed record ParameterizationFault(ChartId Chart, double Distortion) : GeometryFault;

    public sealed record ProjectionFault(EdgeKind Kind, int Segment) : GeometryFault;

    public sealed record DecimationFault(int FaceBudget, int Achieved) : GeometryFault;
    public sealed record RemeshStalled(double TargetLength, double Achieved, int Iterations) : GeometryFault;

    public sealed record EncodingFault(EncodingChannel Channel, ChannelDtype Dtype, string Detail) : GeometryFault;

    public sealed record ParametricFault(ParametricStage Stage, string Carrier, string Witness) : GeometryFault;
    public sealed record DevelopmentFault(DevelopmentStage Stage, int Unit, double Witness) : GeometryFault;

    public int Code =>
        Switch(
            degenerateInput:         static _ => 2400,
            indexMismatch:           static _ => 2401,
            kindMismatch:            static _ => 2402,
            nameCollision:           static _ => 2404,
            hashMismatch:            static _ => 2405,
            unrepairableMesh:        static _ => 2408,
            overConstrained:         static _ => 2412,
            singularSystem:          static _ => 2413,
            degenerateOffset:        static _ => 2416,
            skeletonStalled:         static _ => 2417,
            collapseStalled:         static _ => 2418,
            degenerateArrangement:   static _ => 2420,
            constraintUnrecoverable: static _ => 2421,
            degenerateTessellation:  static _ => 2422,
            nativeAssetMissing:      static _ => 2423,
            intersectionFault:       static _ => 2424,
            sectionFault:            static _ => 2425,
            fitFault:                static _ => 2428,
            parameterizationFault:   static _ => 2432,
            projectionFault:         static _ => 2436,
            decimationFault:         static _ => 2440,
            remeshStalled:           static _ => 2441,
            encodingFault:           static _ => 2444,
            parametricFault:         static _ => 2448,
            developmentFault:        static _ => 2449);

    public FaultCluster Cluster => FaultCluster.OfCode(Code);

    public Error ToError() => Error.New(Code, Message);

    public string Message =>
        Switch(
            degenerateInput:         static f => $"geometry:degenerate-input:kind={f.Kind.Key}:index={f.Index}:{f.Witness}",
            indexMismatch:           static f => $"geometry:index-mismatch:kind={f.Kind.Key}:expected={f.Expected}:actual={f.Actual}",
            kindMismatch:            static f => $"geometry:kind-mismatch:index={f.Index.Key}:query={f.Query.Key}",
            nameCollision:           static f => $"geometry:name-collision:name={f.Name}:kind={f.Kind}",
            hashMismatch:            static f => $"geometry:hash-mismatch:name={f.Name}:kind={f.Kind}",
            unrepairableMesh:        static f => $"geometry:unrepairable-mesh:stage={f.Stage.Key}:iterations={f.Iterations}:remaining={f.Remaining}",
            overConstrained:         static f => $"geometry:over-constrained:redundant={f.RedundantRows}:residual={f.Residual}",
            singularSystem:          static f => $"geometry:singular-system:rank={f.Rank}:parameters={f.Parameters}",
            degenerateOffset:        static f => $"geometry:degenerate-offset:vertex={f.WavefrontVertex}:time={f.Time}",
            skeletonStalled:         static f => $"geometry:skeleton-stalled:pending={f.PendingEvents}:time={f.Time}",
            collapseStalled:         static f => $"geometry:collapse-stalled:iteration={f.Iteration}:residual={f.Residual}",
            degenerateArrangement:   static f => $"geometry:degenerate-arrangement:cells={f.CellCount}:{f.ManifoldWitness}",
            constraintUnrecoverable: static f => $"geometry:constraint-unrecoverable:constraint={f.Constraint}:budget={f.Budget}",
            degenerateTessellation:  static f => $"geometry:degenerate-tessellation:simplex={f.Simplex}:{f.Witness}",
            nativeAssetMissing:      static f => $"geometry:native-asset-missing:engine={f.Engine}:rid={f.Rid}:ceiling={f.Ceiling}",
            intersectionFault:       static f => $"geometry:intersection-fault:a={f.A.Key}:b={f.B.Key}",
            sectionFault:            static f => $"geometry:section-fault:layer={f.Layer}:elevation={f.Elevation}:open={f.OpenChains}",
            fitFault:                static f => $"geometry:fit-fault:inliers={f.AchievedInlierFraction}:floor={f.Floor}",
            parameterizationFault:   static f => $"geometry:parameterization-fault:chart={f.Chart.Value}:distortion={f.Distortion}",
            projectionFault:         static f => $"geometry:projection-fault:kind={f.Kind.Key}:segment={f.Segment}",
            decimationFault:         static f => $"geometry:decimation-fault:budget={f.FaceBudget}:achieved={f.Achieved}",
            remeshStalled:           static f => $"geometry:remesh-stalled:target={f.TargetLength}:achieved={f.Achieved}:iterations={f.Iterations}",
            encodingFault:           static f => $"geometry:encoding-fault:channel={f.Channel.Key}:dtype={f.Dtype.Key}:{f.Detail}",
            parametricFault:         static f => $"geometry:parametric-fault:stage={f.Stage.Key}:carrier={f.Carrier}:{f.Witness}",
            developmentFault:        static f => $"geometry:development-fault:stage={f.Stage.Key}:unit={f.Unit}:witness={f.Witness}");
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
