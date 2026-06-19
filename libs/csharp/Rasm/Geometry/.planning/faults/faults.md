# [RASM_FAULTS]

The consolidated geometry fault family (band 2400) and the one ordinal geometry key policy. The page owns `GeometryFault` — the single `[Union]` every `Fin`/`Validation`/`Eff` rail in the geometry sub-domains routes through, one case per reachable domain failure with its typed payload and its band-2400 ordinal code, lowered into the LanguageExt `Error` rail through its `ToError()` member — and `GeometryKeyPolicy`, the one ordinal string-key comparer the string-keyed smart enums in the domain accept. Every sibling cluster (`spatial`, `topology`, `healing`, `constraints`) names its relevant fault cases inline by shape and routes them through this union by `GeometryFault.<Case>(...).ToError()`; this page is the single owner of the union itself, so the cases live in one closed family and never as per-page error types.

`GeometryFault` is the domain failure rail, not a wire payload — it crosses no transport; the consuming `Fin<T>` channel lowers it at the in-process seam. The ordinal band (2400) is the geometry domain's slice of the package fault space, contiguous so a reader scans the family by code.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                                                                                                                             |
| :-----: | :--------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | FAULT_BAND | `GeometryFault` `[Union]` (band 2400) — one closed family over every geometry failure, lowered into the `Error` rail through `ToError()`; `GeometryKeyPolicy` ordinal key accessor |

## [2]-[FAULT_BAND]

- Owner: `GeometryFault` the closed `[Union]` lowered into the LanguageExt `Error` rail through its `ToError()` member, one case per reachable domain failure carrying its typed payload and its band-2400 ordinal `Code`; `GeometryKeyPolicy` the ordinal string-key comparer accessor the domain's string-keyed smart enums (`HealKind`, `SpatialKind`) bind through `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, so the one comparer policy is named once and never re-minted per enum.
- Cases: the band-2400 family is the union of the cases the sibling clusters route — `DegenerateInput` (2401, empty/non-finite primitive set, `spatial`) · `IndexMismatch` (2402, refit primitive-count mismatch, `spatial`) · `NameCollision` (2410, non-injective re-anchor, `topology`) · `HashMismatch` (2411, dangling reconciliation reference, `topology`) · `UnrepairableMesh` (2420, a heal kernel cannot satisfy its post-condition within budget, `healing`) · `NativeAssetMissing` (2421, the boolean op invoked without its tier-3 native asset, `healing`) · `OverConstrained` (2430, redundant + inconsistent system, `constraints`) · `SingularSystem` (2431, damped normal matrix rank-deficient through the ladder, `constraints`) · `DegenerateOffset` (2440, a self-intersecting or zero-area offset input the wavefront cannot propagate, `offsetting`) · `SkeletonStalled` (2441, the straight-skeleton event queue stalls with pending events past the time budget, `offsetting`). The codes are sub-banded by sibling so a reader maps a code to its owning cluster: 2401–2409 spatial, 2410–2419 topology, 2420–2429 healing, 2430–2439 constraints, 2440–2449 offsetting.
- Entry: each case is a static factory on the union (`GeometryFault.DegenerateInput(string detail)`, `GeometryFault.NameCollision(UInt128 name, int kind)`, `GeometryFault.OverConstrained(int redundantRows, double residual)`, and so on) returning the union value; `public Error ToError()` lowers the union value into the LanguageExt `Error` the `Fin<T>` failure channel carries, threading the `Code` and a rendered message, so a sibling routes a failure as `GeometryFault.<Case>(...).ToError()` and the case payload is preserved on the rail. `public int Code` reads the ordinal band-2400 code the case carries.
- Auto: the union projects into the `Error` rail through `ToError` — it reads the case payload and builds the `Error` with the band-2400 ordinal `Code` (`Error.New(int, string)`) and the rendered detail, so no separate exception type or error-code enum sits beside the union; a rail consumer matches on the `GeometryFault` case to recover the typed payload or reads the `Code` to band the failure for telemetry.
- Receipt: none — `GeometryFault` is the failure rail itself, the terminal value a `Fin<T>` carries on the failure side; it carries no residual receipt because a fault IS the residual.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), LanguageExt.Core (`Error`, `Fin`), BCL inbox (`UInt128`).
- Growth: a new reachable domain failure is one `GeometryFault` case carrying its typed payload and the next ordinal code in its sibling's sub-band — never a parallel error type, never an `int` error-code constant inlined at the throw site; a new sibling sub-band claims its next free decade (offsetting holds 2440–2449, a further sibling claims 2450+). The string-key comparer is `GeometryKeyPolicy` for every string-keyed enum and a second ordinal comparer is the deleted form.
- Boundary: `GeometryFault` is the ONE fault union for the geometry domain and a per-cluster `SpatialFault`/`TopologyFault`/`HealFault`/`ConstraintFault` family is the named density defect collapsed here onto one closed union lowered into the `Error` rail through `ToError()` — the cluster is the sub-band, not a parallel union; an exception thrown from domain logic is forbidden, every failure routes the `Fin`/`Validation`/`Eff` rail as `GeometryFault.<Case>(...).ToError()`, and a `try`/`catch` in domain logic (rather than at the one host-numeric boundary in `constraints/solver#CONSTRAINT_SOLVER` where MathNet may throw) is the deleted form; the union is NOT a generic `IFault`/`IError`/reported-value abstraction — each case is typed to its failure with its real payload (`NameCollision` carries the colliding name and kind, `OverConstrained` carries the redundant-row count and the residual), so a generic erasing carrier is the deleted form; `GeometryKeyPolicy` is the one ordinal string-key comparer and a per-enum `StringComparer.Ordinal` field is the duplicated form collapsed onto the one accessor.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Geometry;

// --- [TYPES] ------------------------------------------------------------------------------
public sealed class GeometryKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

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

    public int Code =>
        Switch(
            degenerateInput:    static _ => 2401,
            indexMismatch:      static _ => 2402,
            nameCollision:      static _ => 2410,
            hashMismatch:       static _ => 2411,
            unrepairableMesh:   static _ => 2420,
            nativeAssetMissing: static _ => 2421,
            overConstrained:    static _ => 2430,
            singularSystem:     static _ => 2431,
            degenerateOffset:   static _ => 2440,
            skeletonStalled:    static _ => 2441);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            degenerateInput:    static f => $"geometry:degenerate-input:{f.Detail}",
            indexMismatch:      static f => $"geometry:index-mismatch:{f.Detail}",
            nameCollision:      static f => $"geometry:name-collision:name={f.Name}:kind={f.Kind}",
            hashMismatch:       static f => $"geometry:hash-mismatch:name={f.Name}:kind={f.Kind}",
            unrepairableMesh:   static f => $"geometry:unrepairable-mesh:{f.Detail}",
            nativeAssetMissing: static f => $"geometry:native-asset-missing:{f.Detail}",
            overConstrained:    static f => $"geometry:over-constrained:redundant={f.RedundantRows}:residual={f.Residual}",
            singularSystem:     static f => $"geometry:singular-system:rank={f.Rank}:parameters={f.Parameters}",
            degenerateOffset:   static f => $"geometry:degenerate-offset:{f.Detail}",
            skeletonStalled:    static f => $"geometry:skeleton-stalled:pending={f.PendingEvents}:time={f.Time}");
}
```

## [3]-[DENSITY_BAR]

One owner for the whole geometry fault rail; a new failure is a case in its sibling's sub-band, never a sibling union. The `[RAIL]` cell names the channel each owner serves.

| [INDEX] | [AXIS/CONCERN] | [OWNER]             | [KIND]                                                                                      | [RAIL]                                                                       | [CASES] |
| :-----: | :------------- | :------------------ | :------------------------------------------------------------------------------------------ | :--------------------------------------------------------------------------- | :-----: |
|   [1]   | Fault family   | `GeometryFault`     | `[Union]` band 2400 sub-banded by sibling + `Code`/`ToError` lowering into the `Error` rail | `GeometryFault.<Case>(...).ToError() → Error` (the `Fin<T>` failure channel) |   10    |
|   [2]   | Key policy     | `GeometryKeyPolicy` | ordinal string-key comparer accessor                                                        | comparer (read by string-keyed smart enums)                                  |    —    |
