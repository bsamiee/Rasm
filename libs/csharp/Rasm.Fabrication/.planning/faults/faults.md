# [RASM_FABRICATION_FAULTS]

The fabrication fault rail: `FabricationFault` a small `[Union]` band for the fabrication-specific failures no shared geometry fault covers — `NoFit` (a part cannot be placed on the sheet under every rotation), `Unreachable` (an IK target lies outside the chain's reachable workspace), `KerfCollision` (a kerf offset collapses a feature narrower than the cut width), `OpenLoop` (a fabrication boundary the kernel admitted but a cut/toolpath/nest demands closed) — composing the kernel band-2400 `Rasm/Geometry/geometry-kernel#FAULT_BAND` `GeometryFault` for the shared `DegenerateInput` (empty or non-finite primitive set). The band-ownership resolution: a degenerate or non-finite primitive set is a geometry failure that routes the kernel `GeometryFault.DegenerateInput`; a non-closed boundary is a fabrication contract (the kernel admits an open loop, the cut/toolpath/nest concern rejects it), so it mints a `FabricationFault.OpenLoop` arm rather than a kernel case the kernel union never declared. The page also owns `FabricationKeyPolicy`, the ordinal comparer accessor the dictionary-keyed `[SmartEnum<string>]` owners — `FrontierKind` (the `Builders` table key) and `ToolpathKind` (the `Generators` table key) — reference for their key-member equality and comparison; a string smart-enum that is never a `FrozenDictionary` key carries no comparer attribute.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; it never sits between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                                          |
| :-----: | :--------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | FAULT_BAND | `FabricationFault` `[Union]` band-2500 (`NoFit`/`Unreachable`/`KerfCollision`/`OpenLoop`) with `Code`/`ToError`, composing the kernel band-2400 `GeometryFault`; `FabricationKeyPolicy` ordinal accessor |

## [2]-[FAULT_BAND]

- Owner: `FabricationFault` the closed `[Union]` fault band (band 2500) for fabrication-specific failures, mirroring the kernel `GeometryFault` shape — one case per failure carrying its detail, a `Code`/`Message` state-threaded `Switch`, and a `ToError()` that lowers the case into the `Fin<T>` failure channel; `FabricationKeyPolicy` the static ordinal comparer accessor the smart-enum owners reference for key-member equality/comparison.
- Cases: `FabricationFault` arms `NoFit` (a part cannot be placed on the sheet, produced by `nesting/nfp#NESTING`) · `Unreachable` (an IK target outside the reachable workspace, produced by `toolpath/motion#CAM_MOTION` when a reach-strict `IkPolicy` flags a non-converged solve) · `KerfCollision` (a kerf offset collapses a sub-kerf feature, produced by `posting/program#CUT_PROGRAM`) · `OpenLoop` (a non-closed boundary a cut/toolpath/nest demands closed, produced by `toolpath/motion`·`nesting/nfp`·`toolpath/skeleton`·`posting/program`) (4); the shared `DegenerateInput` (empty or non-finite primitive set) routes the kernel `GeometryFault`, never re-cased here.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries; both `FabricationFault` and the kernel `GeometryFault` are union values lowered onto the rail through `<Fault>.<Case>(...).ToError()` — `FabricationFault.NoFit(detail).ToError()` and `GeometryFault.DegenerateInput(detail).ToError()` build the band-coded `Error` the `Fin<T>` failure channel carries — so one lowering idiom serves both families on the single `Fin<FrontierResult>` rail every fabrication entrypoint returns, and a kernel route and a fabrication route compose without a second rail.
- Auto: each fabrication owner routes the most specific fault, lowering it with `.ToError()` — `Nest.Solve` routes `FabricationFault.NoFit(...).ToError()` when no placement fits and `FabricationFault.OpenLoop(...).ToError()` when a part outline is non-closed; `Ik.Solve` stamps the residual and a false reached flag, and `Cam.Solve` reads that stamp — under a permissive `IkPolicy` it threads the residual onto the `Motion` receipt unfailed, under a reach-strict `IkPolicy.ReachStrict` it routes `FabricationFault.Unreachable(...).ToError()` when the solve does not converge inside tolerance; `Posting.Kerf` routes `FabricationFault.KerfCollision(...).ToError()` on a collapsed offset; an empty or non-finite primitive set lowers `GeometryFault.DegenerateInput(...).ToError()`.
- Receipt: `FabricationFault` is the typed fault evidence on the `Fin<T>` failure rail; no generic `IFault`/error-code abstraction, the union cases stay typed per fabrication concern.
- Packages: `Rasm.Geometry` (the kernel `GeometryFault` band-2400 — composed for the shared degenerate-input failure), Thinktecture.Runtime.Extensions (`[Union]`, key-member comparer), LanguageExt.Core (`Error`/`Fin`), BCL inbox.
- Growth: a new fabrication-specific failure is one `FabricationFault` arm carrying the next ordinal in the fabrication band; a shared geometry failure is never re-cased here and routes the kernel `GeometryFault`; zero new band.
- Boundary: `FabricationFault` mints ONLY the fabrication-specific cases and composes the kernel band-2400 `GeometryFault` for the shared geometry failure — a parallel band that re-cases `DegenerateInput`, or a synthesized `GeometryFault.OpenLoop` case the kernel union never declares, is the deleted form; both families lower onto the rail through `.ToError()` and a case passed bare into `Fin.Fail<T>(...)` without that lowering is the named seam defect; faults route through the `Fin`/`Validation`/`Eff` rails and exception-style control flow in domain logic is the named defect; `FabricationKeyPolicy` is the one ordinal comparer accessor the smart-enum owners reference and a per-enum bespoke comparer is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Fabrication;

// --- [TYPES] ------------------------------------------------------------------------------
public sealed class FabricationKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault {
    private FabricationFault() { }

    public sealed record NoFit(string Detail) : FabricationFault;
    public sealed record Unreachable(string Detail) : FabricationFault;
    public sealed record KerfCollision(string Detail) : FabricationFault;
    public sealed record OpenLoop(string Detail) : FabricationFault;

    public int Code =>
        Switch(
            noFit:        static _ => 2501,
            unreachable:  static _ => 2502,
            kerfCollision: static _ => 2503,
            openLoop:     static _ => 2504);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            noFit:        static f => $"fabrication:no-fit:{f.Detail}",
            unreachable:  static f => $"fabrication:unreachable:{f.Detail}",
            kerfCollision: static f => $"fabrication:kerf-collision:{f.Detail}",
            openLoop:     static f => $"fabrication:open-loop:{f.Detail}");
}
```
