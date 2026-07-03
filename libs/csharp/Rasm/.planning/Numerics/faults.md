# [RASM_FAULTS]

The consolidated geometry fault family (band 2400). The page owns `GeometryFault` — the single `[Union]` every `Fin`/`Validation`/`Eff` rail in the geometry sub-domains routes through, one case per reachable domain failure with its typed payload and its band-2400 ordinal code, lowered into the LanguageExt `Error` rail through its `ToError()` member; the domain's string-keyed smart enums bind the shipped `ComparerAccessors.StringOrdinal` accessor, so the ordinal comparer is named once by the runtime library and never re-minted per enum. Every sibling cluster (`spatial`, `topology`, `healing`, `constraints`, `offsetting`, `arrangement`, `intersection`, `fitting`, `parameterization`, `projection`, `simplification`, `encoding`) names its relevant fault cases inline by shape and routes them through this union by `GeometryFault.<Case>(...).ToError()`; this page is the single owner of the union itself, so the cases live in one closed family and never as per-page error types.

`GeometryFault` is the domain failure rail, not a wire payload — it crosses no transport; the consuming `Fin<T>` channel lowers it at the in-process seam. The ordinal band (2400) is the geometry domain's slice of the package fault space, contiguous so a reader scans the family by code.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: `GeometryFault` `[Union]` (band 2400) — one closed family over every geometry failure, lowered into the `Error` rail through `ToError()`; geometry string-enums bind the shipped `ComparerAccessors.StringOrdinal` accessor.

## [02]-[FAULT_BAND]

- Owner: `GeometryFault` the closed `[Union]` lowered into the LanguageExt `Error` rail through its `ToError()` member, one case per reachable domain failure carrying its typed payload and its band-2400 ordinal `Code`; the domain's string-keyed smart enums (`HealKind`, `SpatialKind`) bind the shipped `ComparerAccessors.StringOrdinal` accessor through `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, so the ordinal comparer is named once by the library and never re-minted per enum.
- Cases: the band-2400 family is the union of the cases the sibling clusters route — `DegenerateInput` (2400, empty/non-finite primitive set, `spatial`) · `IndexMismatch` (2401, refit primitive-count mismatch, `spatial`) · `NameCollision` (2404, non-injective re-anchor, `topology`) · `HashMismatch` (2405, dangling reconciliation reference, `topology`) · `UnrepairableMesh` (2408, a heal kernel cannot satisfy its post-condition within budget, `healing`) · `NativeAssetMissing` (2409, the boolean op invoked without its tier-3 native asset, `healing`) · `OverConstrained` (2412, redundant + inconsistent system, `constraints`) · `SingularSystem` (2413, damped normal matrix rank-deficient through the ladder, `constraints`) · `DegenerateOffset` (2416, a self-intersecting or zero-area offset input the wavefront cannot propagate, `offsetting`) · `SkeletonStalled` (2417, the straight-skeleton event queue stalls with pending events past the time budget, `offsetting`) · `DegenerateArrangement` (2420, two triangle soups whose intersection is non-manifold or whose cell classification is ambiguous, `arrangement`) · `IntersectionFault` (2424, a narrow-phase exact test on a degenerate primitive pair, `intersection`) · `FitFault` (2428, a RANSAC consensus never reaching the inlier-fraction floor within the sample budget, carrying the achieved fraction vs the floor, `fitting`) · `ParameterizationFault` (2432, a flattening solve that diverges or a non-disk-topology chart, `parameterization`) · `ProjectionFault` (2436, a BSP partition stall or a silhouette extraction over a non-manifold view, `projection`) · `DecimationFault` (2440, a QEM collapse that cannot satisfy the topology-preservation gate within the face budget, carrying the budget vs the achieved count, `simplification`) · `EncodingFault` (2444, a channel pack whose round-trip witness fails the content-hash identity, carrying the failing channel, `encoding`). The whole band fits inside the geometry century 2400–2449 — strictly below the AEC `MaterialFault` band 2450 so a telemetry reader banding by code never conflates a geometry fault with a Material/Element/Bim fault — and the codes are sub-banded by sibling on a 4-wide stride so a reader maps a code to its owning cluster: 2400–2403 spatial, 2404–2407 topology, 2408–2411 healing, 2412–2415 constraints, 2416–2419 offsetting, 2420–2423 arrangement, 2424–2427 intersection, 2428–2431 fitting, 2432–2435 parameterization, 2436–2439 projection, 2440–2443 simplification, 2444–2447 encoding.
- Entry: each case is a static factory on the union (`GeometryFault.DegenerateInput(string detail)`, `GeometryFault.NameCollision(UInt128 name, int kind)`, `GeometryFault.OverConstrained(int redundantRows, double residual)`, and so on) returning the union value; `public Error ToError()` lowers the union value into the LanguageExt `Error` the `Fin<T>` failure channel carries, threading the `Code` and a rendered message, so a sibling routes a failure as `GeometryFault.<Case>(...).ToError()` and the case payload is preserved on the rail. `public int Code` reads the ordinal band-2400 code the case carries.
- Auto: the union projects into the `Error` rail through `ToError` — it reads the case payload and builds the `Error` with the band-2400 ordinal `Code` (`Error.New(int, string)`) and the rendered detail, so no separate exception type or error-code enum sits beside the union; a rail consumer matches on the `GeometryFault` case to recover the typed payload or reads the `Code` to band the failure for telemetry.
- Receipt: none — `GeometryFault` is the failure rail itself, the terminal value a `Fin<T>` carries on the failure side; it carries no residual receipt because a fault IS the residual.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), LanguageExt.Core (`Error`, `Fin`), BCL inbox (`UInt128`).
- Growth: a new reachable domain failure is one `GeometryFault` case carrying its typed payload and the next free ordinal in its sibling's 4-wide sub-band — never a parallel error type, never an `int` error-code constant inlined at the throw site; a new sibling sub-band claims the next free 4-wide stride above encoding (the band now runs through encoding 2444–2447, so a further sibling claims 2448–2449 — the last headroom inside the geometry century — and the band must stay below the AEC `MaterialFault` 2450 boundary, so an outright federation re-plan rather than a 13th cluster is the move once the century fills). The string-key comparer is `ComparerAccessors.StringOrdinal` for every string-keyed enum and a second ordinal comparer is the deleted form.
- Boundary: `GeometryFault` is the ONE fault union for the geometry domain and a per-cluster `SpatialFault`/`TopologyFault`/`HealFault`/`ConstraintFault` family is the named density defect collapsed here onto one closed union lowered into the `Error` rail through `ToError()` — the cluster is the sub-band, not a parallel union; an exception thrown from domain logic is forbidden, every failure routes the `Fin`/`Validation`/`Eff` rail as `GeometryFault.<Case>(...).ToError()`, and a `try`/`catch` in domain logic (rather than at the one host-numeric boundary in `Processing/solver#CONSTRAINT_SOLVER` where MathNet may throw) is the deleted form; the union is NOT a generic `IFault`/`IError`/reported-value abstraction — each case is typed to its failure with its real payload (`NameCollision` carries the colliding name and kind, `OverConstrained` carries the redundant-row count and the residual), so a generic erasing carrier is the deleted form; `ComparerAccessors.StringOrdinal` is the one ordinal string-key comparer and a per-enum `StringComparer.Ordinal` field is the duplicated form collapsed onto the one accessor.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Geometry;

// --- [TYPES] ------------------------------------------------------------------------------

// --- [ERRORS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryFault {
    private GeometryFault() { }

    public sealed record DegenerateInput(string Detail) : GeometryFault;
    public sealed record IndexMismatch(string Detail) : GeometryFault;

    public sealed record NameCollision(UInt128 Name, int Kind) : GeometryFault;
    public sealed record HashMismatch(UInt128 Name, int Kind) : GeometryFault;

    public sealed record UnrepairableMesh(string Detail) : GeometryFault;
    public sealed record NativeAssetMissing(string Detail) : GeometryFault;

    public sealed record OverConstrained(int RedundantRows, double Residual) : GeometryFault;
    public sealed record SingularSystem(int Rank, int Parameters) : GeometryFault;

    public sealed record DegenerateOffset(string Detail) : GeometryFault;
    public sealed record SkeletonStalled(int PendingEvents, double Time) : GeometryFault;

    public sealed record DegenerateArrangement(string Detail) : GeometryFault;
    public sealed record IntersectionFault(string Detail) : GeometryFault;
    public sealed record FitFault(double AchievedInlierFraction, double Floor) : GeometryFault;
    public sealed record ParameterizationFault(string Detail) : GeometryFault;
    public sealed record ProjectionFault(string Detail) : GeometryFault;
    public sealed record DecimationFault(int FaceBudget, int Achieved) : GeometryFault;
    public sealed record EncodingFault(string Channel, string Detail) : GeometryFault;

    public int Code =>
        Switch(
            degenerateInput:       static _ => 2400,
            indexMismatch:         static _ => 2401,
            nameCollision:         static _ => 2404,
            hashMismatch:          static _ => 2405,
            unrepairableMesh:      static _ => 2408,
            nativeAssetMissing:    static _ => 2409,
            overConstrained:       static _ => 2412,
            singularSystem:        static _ => 2413,
            degenerateOffset:      static _ => 2416,
            skeletonStalled:       static _ => 2417,
            degenerateArrangement: static _ => 2420,
            intersectionFault:     static _ => 2424,
            fitFault:              static _ => 2428,
            parameterizationFault: static _ => 2432,
            projectionFault:       static _ => 2436,
            decimationFault:       static _ => 2440,
            encodingFault:         static _ => 2444);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            degenerateInput:       static f => $"geometry:degenerate-input:{f.Detail}",
            indexMismatch:         static f => $"geometry:index-mismatch:{f.Detail}",
            nameCollision:         static f => $"geometry:name-collision:name={f.Name}:kind={f.Kind}",
            hashMismatch:          static f => $"geometry:hash-mismatch:name={f.Name}:kind={f.Kind}",
            unrepairableMesh:      static f => $"geometry:unrepairable-mesh:{f.Detail}",
            nativeAssetMissing:    static f => $"geometry:native-asset-missing:{f.Detail}",
            overConstrained:       static f => $"geometry:over-constrained:redundant={f.RedundantRows}:residual={f.Residual}",
            singularSystem:        static f => $"geometry:singular-system:rank={f.Rank}:parameters={f.Parameters}",
            degenerateOffset:      static f => $"geometry:degenerate-offset:{f.Detail}",
            skeletonStalled:       static f => $"geometry:skeleton-stalled:pending={f.PendingEvents}:time={f.Time}",
            degenerateArrangement: static f => $"geometry:degenerate-arrangement:{f.Detail}",
            intersectionFault:     static f => $"geometry:intersection-fault:{f.Detail}",
            fitFault:              static f => $"geometry:fit-fault:inliers={f.AchievedInlierFraction}:floor={f.Floor}",
            parameterizationFault: static f => $"geometry:parameterization-fault:{f.Detail}",
            projectionFault:       static f => $"geometry:projection-fault:{f.Detail}",
            decimationFault:       static f => $"geometry:decimation-fault:budget={f.FaceBudget}:achieved={f.Achieved}",
            encodingFault:         static f => $"geometry:encoding-fault:channel={f.Channel}:{f.Detail}");
}
```

## [03]-[DENSITY_BAR]

One owner for the whole geometry fault rail; a new failure is a case in its sibling's sub-band, never a sibling union. The `[RAIL]` cell names the channel each owner serves.

| [INDEX] | [AXIS/CONCERN] | [OWNER]             | [KIND]                                                                                      | [RAIL]                                                                       | [CASES] |
| :-----: | :------------- | :------------------ | :------------------------------------------------------------------------------------------ | :--------------------------------------------------------------------------- | :-----: |
|  [01]   | Fault family   | `GeometryFault`     | `[Union]` band 2400 sub-banded by sibling + `Code`/`ToError` lowering into the `Error` rail | `GeometryFault.<Case>(...).ToError() → Error` (the `Fin<T>` failure channel) |   17    |
