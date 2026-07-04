# [RASM_FAULTS]

The consolidated geometry fault family (band 2400-2449). The page owns `GeometryFault` — the single `[Union]` every `Fin`/`Validation`/`Eff` rail in the geometry sub-domains routes through, one case per reachable domain failure carrying its TYPED discriminant payload and its band-2400 ordinal code, lowered into the LanguageExt `Error` rail through `ToError()` — plus `FaultCluster`, the 13-row cluster taxonomy the band arithmetic derives a code's owner from, and the two faults-minted stage vocabularies `ParametricStage`/`DevelopmentStage`. The union lives at the `Rasm.Numerics` ROOT namespace; the 13 fault-routing TIER-2 namespaces map BIJECTIVELY onto the 13 clusters covering exactly 2400-2449, and every sibling routes a failure as `GeometryFault.<Case>(...).ToError()` — the cases live in one closed family, never as per-page error types.

Two fault families exist by explicit decision and neither absorbs the other: `Rasm.Domain` owns the kernel-substrate `Expected`/`Fault` family (admission, validation, host-boundary faults on the generated-owner weave), and this page owns the band-2400 `GeometryFault` union (robust-core geometry failures on the `Fin` rail). `Domain/rails.md` states the seam from its side; this page states it from the geometry side: a geometry owner never mints a `Rasm.Domain` `Fault` case for a robust-core failure, a substrate owner never claims a band-2400 code, and the ordinal band keeps a telemetry reader banding by code from ever conflating the two — 2400-2449 sits strictly below the AEC `MaterialFault` band 2450.

Every case payload is a typed discriminant owned by the failure's own sibling — never an erased `string Detail` carrier where a vocabulary row, an index, or a measure states the cause precisely. A `string` survives only as a WITNESS field beside typed discriminants (the free-text evidence a fold cannot type), never as the sole payload.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: `GeometryFault` `[Union]` (25 cases, band 2400-2449) — one closed family over every geometry failure, typed discriminant payloads, `Code`/`Message`/`Cluster` derived projections, `ToError()` lowering into the `Error` rail; `FaultCluster` the 13-row `[SmartEnum<int>]` cluster taxonomy keyed by stride base; `ParametricStage`/`DevelopmentStage` the two faults-minted stage vocabularies; the two-family seam against `Rasm.Domain` `Expected`/`Fault`.

## [02]-[FAULT_BAND]

- Owner: `GeometryFault` the closed `[Union]` at the `Rasm.Numerics` root, one case per reachable domain failure carrying its typed payload and its band-2400 ordinal `Code`, lowered into the LanguageExt `Error` rail through `ToError()` (`Error.New(Code, Message)`); `FaultCluster` the `[SmartEnum<int>]` cluster taxonomy — 13 rows keyed by stride base, each carrying its cluster name and owning TIER-2 namespace, resolved from a code by the stride arithmetic `Items[(code - 2400) >> 2]` (the 12 four-wide strides index 0-11; the two-code `parametric` tail 2448-2449 both resolve to index 12) so telemetry bands a code to its owner with zero lookup table beside the vocabulary; `ParametricStage` (`construction`/`evaluation`/`station`/`offset`/`encode`) and `DevelopmentStage` (`subdivision`/`strip`/`panel`/`pattern`) the string-keyed `[SmartEnum<string>]` stage vocabularies minted HERE because their consumers (`ParametricFault`/`DevelopmentFault`) span the whole Parametric tier and no single tier page owns the stage axis — string-keyed, never keyless, because the stage renders into the wire-bound `Message` and rendered identity requires a key; every string-keyed enum binds the shipped `ComparerAccessors.StringOrdinal` accessor through `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, so the ordinal comparer is named once by the runtime library and never re-minted per enum.
- Cases: 25, sub-banded by cluster on the ratified 13-cluster partition covering exactly 2400-2449 (12 × 4-wide + the 2-wide `parametric` tail — the century closes arithmetically, zero headroom remains):
  - 2400-2403 `spatial` (`Rasm.Spatial`): `DegenerateInput(Kind, int, string)` 2400 — empty, non-finite, or kind-invalid primitive set; the ONE cross-cutting admission case any namespace routes (the recorded exception to cluster-locality) · `IndexMismatch(EntityKind, int, int)` 2401 — refit entity-count mismatch, expected vs actual · `KindMismatch(SpatialKind, QueryKind)` 2402 — a query modality the built index kind cannot answer (the silent-empty die).
  - 2404-2407 `naming` (`Rasm.Spatial`): `NameCollision(UInt128, int)` 2404 · `HashMismatch(UInt128, int)` 2405.
  - 2408-2411 `healing` (`Rasm.Processing`): `UnrepairableMesh(HealStage, int, int)` 2408 — the failing heal stage, iterations spent, defects remaining. `NativeAssetMissing` RE-CODES OUT of this cluster to 2423 — the tier-3 gate lives with the boolean owner.
  - 2412-2415 `constraints` (`Rasm.Solving`): `OverConstrained(int, double)` 2412 · `SingularSystem(int, int)` 2413 (both rich, kept verbatim).
  - 2416-2419 `offsetting` (`Rasm.Meshing`): `DegenerateOffset(int, double)` 2416 — the wavefront vertex the propagation dies at and the event time · `SkeletonStalled(int, double)` 2417 (rich, stays) · `CollapseStalled(int, double)` 2418 — a wavefront/MCF collapse iteration stalling with its residual.
  - 2420-2423 `arrangement` (`Rasm.Meshing` — delaunay homed here; the `Tessellation` TYPE keeps its name inside this namespace): `DegenerateArrangement(int, string)` 2420 — cell count plus the manifold witness · `ConstraintUnrecoverable(int, int)` 2421 — the constraint id whose recovery exhausts its Steiner budget · `DegenerateTessellation(int, string)` 2422 — the simplex id plus the degeneracy witness · `NativeAssetMissing(string, string, long)` 2423 — engine, RID, and the `ScaleCeiling` the tier-3 route was gated behind (RE-CODED from 2409).
  - 2424-2427 `intersection` (`Rasm.Meshing`): `IntersectionFault(PrimitiveKind, PrimitiveKind)` 2424 — the degenerate primitive pair by kind · `SectionFault(int, double, int)` 2425 — layer index, elevation, and the open-chain count of a non-watertight section.
  - 2428-2431 `fitting` (`Rasm.Solving`): `FitFault(double, double)` 2428 (rich, stays).
  - 2432-2435 `parameterization` (`Rasm.Processing`): `ParameterizationFault(ChartId, double)` 2432 — the diverging chart and its distortion; the curve borrow ENDS (parametric-tier failures route 2448).
  - 2436-2439 `projection` (`Rasm.Drawing`): `ProjectionFault(EdgeKind, int)` 2436 — the projected-edge kind and segment index.
  - 2440-2443 `simplification` (`Rasm.Processing` — the widened mesh-rewrite-under-budget charter): `DecimationFault(int, int)` 2440 (rich, stays) · `RemeshStalled(double, double, int)` 2441 — target edge length, achieved length, iterations spent.
  - 2444-2447 `encoding` (`Rasm.Drawing`): `EncodingFault(EncodingChannel, ChannelDtype, string)` 2444 — the failing channel row, its quantization row, and the witness detail.
  - 2448-2449 `parametric` (`Rasm.Parametric` — the deliberate two-code headroom spend forming the 13th cluster): `ParametricFault(ParametricStage, string, string)` 2448 — the failing stage, the carrier name, the witness · `DevelopmentFault(DevelopmentStage, int, double)` 2449 — the failing stage, the unit index, and the per-concern measure (refinement level · isometry error · panel defect · instance defect by stage).
- Entry: each case is a positional record constructor on the union (`new GeometryFault.KindMismatch(index, query)`, `new GeometryFault.NativeAssetMissing(engine, rid, ceiling)`) returning the union value; `public Error ToError()` lowers it into the LanguageExt `Error` the `Fin<T>` failure channel carries (`Error.New(int, string)` threading the band-2400 `Code` and the rendered `Message`), so a sibling routes a failure as `GeometryFault.<Case>(...).ToError()` — the union value is matched and its typed payload read BEFORE lowering, and the lowering projects every discriminant into the banded `Code` plus the machine-parseable `Message` grammar, the two facts the `Error` carries; `public int Code` reads the ordinal; `public FaultCluster Cluster` derives the owning cluster row from `Code` by the stride arithmetic — telemetry reads cluster name and owning namespace off the code with no second map.
- Auto: `Code` and `Message` are two total generated `Switch` folds over the 25 cases in code order — a new case breaks both folds loudly at compile time, never a runtime-silent `_` arm; `Message` renders `geometry:<case>:<field>=<value>` with every keyed discriminant — the stage vocabularies included — projected through its `Key` (recoverable to the vocabulary row); `Cluster` is pure stride arithmetic over the `FaultCluster` declaration order (`Items` order = code order), so the taxonomy has exactly one authoritative declaration.
- Receipt: none — `GeometryFault` is the failure rail itself, the terminal value a `Fin<T>` carries on the failure side; a fault IS the residual.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[SmartEnum<int>]`, `ComparerAccessors.StringOrdinal`), LanguageExt.Core (`Error.New(int, string)`, the `Fin`/`Validation`/`Eff` failure channel), BCL inbox (`UInt128`).
- Growth: a new reachable domain failure is one `GeometryFault` case carrying its typed payload and the next free ordinal in its sibling's sub-band — never a parallel error type, never an `int` error-code constant inlined at the routing site; the century 2400-2449 is FULLY ALLOCATED across the 13 clusters (the `parametric` 2448-2449 spend closed the last headroom), so a genuinely new cluster is a federation re-plan against the AEC 2450 boundary, never a silent squeeze; a new stage on `ParametricStage`/`DevelopmentStage` is one `static readonly` row every stage-reading `Switch` re-proves at compile time; the string-key comparer is `ComparerAccessors.StringOrdinal` for every string-keyed enum and a second ordinal comparer is the deleted form.
- Boundary: `GeometryFault` is the ONE fault union for the geometry domain and a per-cluster `SpatialFault`/`NamingFault`/`HealFault` family is the named density defect collapsed onto this one closed union — the cluster is the sub-band, not a parallel union; an exception thrown from domain logic is forbidden, every failure routes the `Fin`/`Validation`/`Eff` rail as `GeometryFault.<Case>(...).ToError()`, and a `try`/`catch` in domain logic (rather than at an owning host-numeric or native boundary) is the deleted form; the union is NOT a generic `IFault`/erased-detail carrier — an erased `string Detail` standing where a sibling vocabulary row, an index, or a measure types the cause is the named defect and the deleted form; the two-family seam holds — `GeometryFault` (band 2400, robust core) ⟂ `Rasm.Domain` `Expected`/`Fault` (kernel substrate), neither absorbs, each page states its side; every payload discriminant is composed from its owning sibling, never re-minted here: `Kind` (`Rasm.Domain` normalization taxonomy), `EntityKind` (naming), `SpatialKind`/`QueryKind` (index), `HealStage` (repair), `PrimitiveKind` (intersect), `ChartId` (flatten), `EdgeKind` (view), `EncodingChannel`/`ChannelDtype` (pack), while `ParametricStage`/`DevelopmentStage` mint here because no single Parametric page owns the tier-wide stage axis.

```csharp
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
// The 13-cluster taxonomy as data: Items order = code order, so `(code - 2400) >> 2` indexes the
// owning row — total over the band, the two-code parametric tail 2448-2449 both landing on index 12.
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

// Stage vocabularies minted HERE: their consumers span the whole Parametric tier, so no tier page owns
// the axis. String-keyed because the stage renders into the wire-bound Message — keyless reference rows
// carry no verifiable rendered identity.
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
// Declaration order = code order; Code and Message are total generated folds, so a new case
// breaks every dispatch site at compile time. Payload discriminants compose their owning
// sibling's vocabulary: Kind (Rasm.Domain), EntityKind (naming), SpatialKind/QueryKind (index),
// HealStage (repair), PrimitiveKind (intersect), ChartId (flatten), EdgeKind (view),
// EncodingChannel/ChannelDtype (pack).
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
            unrepairableMesh:        static f => $"geometry:unrepairable-mesh:stage={f.Stage}:iterations={f.Iterations}:remaining={f.Remaining}",
            overConstrained:         static f => $"geometry:over-constrained:redundant={f.RedundantRows}:residual={f.Residual}",
            singularSystem:          static f => $"geometry:singular-system:rank={f.Rank}:parameters={f.Parameters}",
            degenerateOffset:        static f => $"geometry:degenerate-offset:vertex={f.WavefrontVertex}:time={f.Time}",
            skeletonStalled:         static f => $"geometry:skeleton-stalled:pending={f.PendingEvents}:time={f.Time}",
            collapseStalled:         static f => $"geometry:collapse-stalled:iteration={f.Iteration}:residual={f.Residual}",
            degenerateArrangement:   static f => $"geometry:degenerate-arrangement:cells={f.CellCount}:{f.ManifoldWitness}",
            constraintUnrecoverable: static f => $"geometry:constraint-unrecoverable:constraint={f.Constraint}:budget={f.Budget}",
            degenerateTessellation:  static f => $"geometry:degenerate-tessellation:simplex={f.Simplex}:{f.Witness}",
            nativeAssetMissing:      static f => $"geometry:native-asset-missing:engine={f.Engine}:rid={f.Rid}:ceiling={f.Ceiling}",
            intersectionFault:       static f => $"geometry:intersection-fault:a={f.A}:b={f.B}",
            sectionFault:            static f => $"geometry:section-fault:layer={f.Layer}:elevation={f.Elevation}:open={f.OpenChains}",
            fitFault:                static f => $"geometry:fit-fault:inliers={f.AchievedInlierFraction}:floor={f.Floor}",
            parameterizationFault:   static f => $"geometry:parameterization-fault:chart={f.Chart}:distortion={f.Distortion}",
            projectionFault:         static f => $"geometry:projection-fault:kind={f.Kind}:segment={f.Segment}",
            decimationFault:         static f => $"geometry:decimation-fault:budget={f.FaceBudget}:achieved={f.Achieved}",
            remeshStalled:           static f => $"geometry:remesh-stalled:target={f.TargetLength}:achieved={f.Achieved}:iterations={f.Iterations}",
            encodingFault:           static f => $"geometry:encoding-fault:channel={f.Channel.Key}:dtype={f.Dtype.Key}:{f.Detail}",
            parametricFault:         static f => $"geometry:parametric-fault:stage={f.Stage.Key}:carrier={f.Carrier}:{f.Witness}",
            developmentFault:        static f => $"geometry:development-fault:stage={f.Stage.Key}:unit={f.Unit}:witness={f.Witness}");
}
```

## [03]-[DENSITY_BAR]

One owner for the whole geometry fault rail; a new failure is a case in its sibling's sub-band, never a sibling union. The `[RAIL]` cell names the channel each owner serves.

| [INDEX] | [AXIS/CONCERN]     | [OWNER]            | [KIND]                                                                                         | [RAIL]                                                                       | [CASES] |
| :-----: | :----------------- | :----------------- | :--------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------- | :-----: |
|  [01]   | Fault family       | `GeometryFault`    | `[Union]` band 2400-2449, typed discriminant payloads + `Code`/`Message`/`Cluster` derived folds + `ToError` lowering | `GeometryFault.<Case>(...).ToError() → Error` (the `Fin<T>` failure channel) |   25    |
|  [02]   | Cluster taxonomy   | `FaultCluster`     | `[SmartEnum<int>]` keyed by stride base, `Name`/`Namespace` columns + `OfCode` stride arithmetic | `FaultCluster.OfCode(code)` (pure, total over the band)                      |   13    |
|  [03]   | Parametric stages  | `ParametricStage`  | `[SmartEnum<string>]` `StringOrdinal`-keyed stage vocabulary (`ParametricFault` discriminant)   | payload row on the union                                                     |    5    |
|  [04]   | Development stages | `DevelopmentStage` | `[SmartEnum<string>]` `StringOrdinal`-keyed stage vocabulary (`DevelopmentFault` discriminant)  | payload row on the union                                                     |    4    |
