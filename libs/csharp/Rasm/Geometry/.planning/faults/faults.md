# [RASM_FAULTS]

The consolidated geometry fault family (band 2400) and the one ordinal geometry key policy. The page owns `GeometryFault` — the single `Error`-derived `[Union]` every `Fin`/`Validation`/`Eff` rail in the geometry sub-domains routes through, one case per reachable domain failure with its typed payload and its band-2400 ordinal code — and `GeometryKeyPolicy`, the one ordinal string-key comparer the string-keyed smart enums in the domain accept. Every sibling cluster (`spatial`, `topology`, `healing`, `constraints`) names its relevant fault cases inline by shape and routes them through this union by `GeometryFault.<Case>(...).ToError()`; this page is the single owner of the union itself, so the cases live in one closed family and never as per-page error types.

`GeometryFault` is the domain failure rail, not a wire payload — it crosses no transport; the consuming `Fin<T>` channel lowers it at the in-process seam. The ordinal band (2400) is the geometry domain's slice of the package fault space, contiguous so a reader scans the family by code.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                                          |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | FAULT_BAND  | `GeometryFault` `[Union]` (band 2400) — one closed `Error`-derived family over every geometry failure; `GeometryKeyPolicy` ordinal key accessor |

## [2]-[FAULT_BAND]

- Owner: `GeometryFault` the closed `[Union]` derived from the LanguageExt `Error` rail, one case per reachable domain failure carrying its typed payload and its band-2400 ordinal `Code`; `GeometryKeyPolicy` the ordinal string-key comparer accessor the domain's string-keyed smart enums (`HealKind`, `SpatialKind`) bind through `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, so the one comparer policy is named once and never re-minted per enum.
- Cases: the band-2400 family is the union of the cases the sibling clusters route — `DegenerateInput` (2401, empty/non-finite primitive set, `spatial`) · `IndexMismatch` (2402, refit primitive-count mismatch, `spatial`) · `NameCollision` (2410, non-injective re-anchor, `topology`) · `HashMismatch` (2411, dangling reconciliation reference, `topology`) · `UnrepairableMesh` (2420, a heal kernel cannot satisfy its post-condition within budget, `healing`) · `NativeAssetMissing` (2421, the boolean op invoked without its tier-3 native asset, `healing`) · `OverConstrained` (2430, redundant + inconsistent system, `constraints`) · `SingularSystem` (2431, damped normal matrix rank-deficient through the ladder, `constraints`). The codes are sub-banded by sibling so a reader maps a code to its owning cluster: 2401–2409 spatial, 2410–2419 topology, 2420–2429 healing, 2430–2439 constraints.
- Entry: each case is a static factory on the union (`GeometryFault.DegenerateInput(string detail)`, `GeometryFault.NameCollision(UInt128 name, int kind)`, `GeometryFault.OverConstrained(int redundantRows, double residual)`, and so on) returning the union value; `public Error ToError()` lowers the union value into the LanguageExt `Error` the `Fin<T>` failure channel carries, threading the `Code` and a rendered message, so a sibling routes a failure as `GeometryFault.<Case>(...).ToError()` and the case payload is preserved on the rail. `public int Code` reads the ordinal band-2400 code the case carries.
- Auto: the union is `Error`-derived so the `Code` and message project into the `Error` rail directly — `ToError` reads the case payload and builds the `Error` with the band-2400 ordinal `Code` and the rendered detail, so no separate exception type or error-code enum sits beside the union; a rail consumer matches on the `GeometryFault` case to recover the typed payload or reads the `Code` to band the failure for telemetry.
- Receipt: none — `GeometryFault` is the failure rail itself, the terminal value a `Fin<T>` carries on the failure side; it carries no residual receipt because a fault IS the residual.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`), LanguageExt.Core (`Error`, `Fin`), BCL inbox (`UInt128`).
- Growth: a new reachable domain failure is one `GeometryFault` case carrying its typed payload and the next ordinal code in its sibling's sub-band — never a parallel error type, never an `int` error-code constant inlined at the throw site; a new sibling sub-band claims its 2440+ decade. The string-key comparer is `GeometryKeyPolicy` for every string-keyed enum and a second ordinal comparer is the deleted form.
- Boundary: `GeometryFault` is the ONE fault union for the geometry domain and a per-cluster `SpatialFault`/`TopologyFault`/`HealFault`/`ConstraintFault` family is the named density defect collapsed here onto one closed `Error`-derived union — the cluster is the sub-band, not a parallel union; an exception thrown from domain logic is forbidden, every failure routes the `Fin`/`Validation`/`Eff` rail as `GeometryFault.<Case>(...).ToError()`, and a `try`/`catch` in domain logic (rather than at the one host-numeric boundary in `constraints/solver#CONSTRAINT_SOLVER` where MathNet may throw) is the deleted form; the union is NOT a generic `IFault`/`IError`/reported-value abstraction — each case is typed to its failure with its real payload (`NameCollision` carries the colliding name and kind, `OverConstrained` carries the redundant-row count and the residual), so a generic erasing carrier is the deleted form; `GeometryKeyPolicy` is the one ordinal string-key comparer and a per-enum `StringComparer.Ordinal` field is the duplicated form collapsed onto the one accessor.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Geometry;

// --- [TYPES] ------------------------------------------------------------------------------
// The one ordinal string-key comparer every string-keyed geometry smart enum binds through
// [KeyMemberEqualityComparer]/[KeyMemberComparer] — named once, never a per-enum StringComparer field.
public sealed class GeometryKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The one closed Error-derived fault family for the geometry domain, band 2400 sub-banded by sibling cluster
// (2401-2409 spatial, 2410-2419 topology, 2420-2429 healing, 2430-2439 constraints). One case per reachable
// domain failure carrying its typed payload; ToError lowers a case into the Fin<T> failure channel with its Code.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryFault {
    private GeometryFault() { }

    // spatial — 2401..2409
    public sealed record DegenerateInput(string Detail) : GeometryFault;                  // 2401 — empty/non-finite primitive set
    public sealed record IndexMismatch(string Detail) : GeometryFault;                     // 2402 — refit primitive-count mismatch

    // topology — 2410..2419
    public sealed record NameCollision(UInt128 Name, int Kind) : GeometryFault;            // 2410 — non-injective re-anchor
    public sealed record HashMismatch(UInt128 Name, int Kind) : GeometryFault;             // 2411 — dangling reconciliation reference

    // healing — 2420..2429
    public sealed record UnrepairableMesh(string Detail) : GeometryFault;                  // 2420 — kernel post-condition unmet within budget
    public sealed record NativeAssetMissing(string Detail) : GeometryFault;                // 2421 — boolean invoked without its tier-3 native asset

    // constraints — 2430..2439
    public sealed record OverConstrained(int RedundantRows, double Residual) : GeometryFault;  // 2430 — redundant + inconsistent system
    public sealed record SingularSystem(int Rank, int Parameters) : GeometryFault;             // 2431 — damped normal matrix rank-deficient through the ladder

    public int Code =>
        Switch(
            degenerateInput:    static _ => 2401,
            indexMismatch:      static _ => 2402,
            nameCollision:      static _ => 2410,
            hashMismatch:       static _ => 2411,
            unrepairableMesh:   static _ => 2420,
            nativeAssetMissing: static _ => 2421,
            overConstrained:    static _ => 2430,
            singularSystem:     static _ => 2431);

    // Lower the union value into the Fin<T> failure channel: the band-2400 Code plus the rendered detail.
    // A sibling routes a failure as GeometryFault.<Case>(...).ToError(); the payload is preserved on the rail.
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
            singularSystem:     static f => $"geometry:singular-system:rank={f.Rank}:parameters={f.Parameters}");
}
```

## [3]-[DENSITY_BAR]

One owner for the whole geometry fault rail; a new failure is a case in its sibling's sub-band, never a sibling union. The `[RAIL]` cell names the channel each owner serves.

| [INDEX] | [AXIS/CONCERN]   | [OWNER]             | [KIND]                                                              | [RAIL]                                  | [CASES] |
| :-----: | :--------------- | :------------------ | :----------------------------------------------------------------- | :-------------------------------------- | :-----: |
|   [1]   | Fault family     | `GeometryFault`     | `[Union]` `Error`-derived, band 2400 sub-banded by sibling + `Code`/`ToError` | `GeometryFault.<Case>(...).ToError() → Error` (the `Fin<T>` failure channel) |    8    |
|   [2]   | Key policy       | `GeometryKeyPolicy` | ordinal string-key comparer accessor                               | comparer (read by string-keyed smart enums) |    —    |
