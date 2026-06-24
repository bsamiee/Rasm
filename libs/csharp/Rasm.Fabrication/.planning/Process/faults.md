# [RASM_FABRICATION_FAULTS]

The fabrication fault rail: `FabricationFault` a `[Union]` band for the fabrication-specific failures no shared geometry fault covers — the original four `NoFit` (a part cannot be placed on the sheet under every rotation), `Unreachable` (an IK target lies outside the chain's reachable workspace), `KerfCollision` (a kerf offset collapses a feature narrower than the cut width), `OpenLoop` (a fabrication boundary the kernel admitted but a cut/toolpath/nest demands closed), plus the widened-axis and new-sub-domain arms `InadmissiblePair` (a `(RemovalModality, CutStrategy)` pair the `Admits` relation rejects — turning's `radial-sweep` on a cartesian-gantry laser), `Gouge` (a swept tool envelope intersects the part-keep geometry), `Collision` (a swept tool-plus-holder envelope intersects a stock/fixture keep-out), `NonManifoldSlice` (an additive planar section yields a non-closed or self-crossing contour the slicer cannot emit), and `StockOverflow` (a part batch exceeds the supplied `Seq<Stock>` inventory after the multi-sheet spill exhausts) — composing the kernel band-2400 `Rasm.Geometry/Numerics/faults#FAULT_BAND` `GeometryFault` for the shared `DegenerateInput` (empty or non-finite primitive set). The band-ownership resolution: a degenerate or non-finite primitive set is a geometry failure that routes the kernel `GeometryFault.DegenerateInput`; a non-closed boundary is a fabrication contract (the kernel admits an open loop, the cut/toolpath/nest concern rejects it), so it mints a `FabricationFault.OpenLoop` arm rather than a kernel case the kernel union never declared. Each new arm is a genuinely fabrication-specific rejection the kernel union never declares — a non-finite slice vertex stays `GeometryFault.DegenerateInput` (the slicer routes `NonManifoldSlice` only for a topologically-broken closed section, never a coordinate defect), and `Gouge`/`Collision` stay distinct from `KerfCollision` (the kerf-offset collapse), the swept-envelope intersection a separate safety concern from the sub-kerf feature. The folder carries no string-comparer accessor: every fabrication `[SmartEnum<string>]` (`ToolpathKind`, `ClipOp`, `OffsetEnds`, `GCommand`, `LeadStyle`) dispatches through its generated total `Switch` or a behavior column and keys no `FrozenDictionary`, and the `Process/owner#FABRICATION_OWNER` `Run` dispatch is the generated `FabricationPolicy` union `Switch`, so no owner needs a `[KeyMemberComparer]` and a comparer accessor with zero referencing owners is the deleted form.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; it never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: owns the `FabricationFault` `[Union]` band-2500 (`NoFit`/`Unreachable`/`KerfCollision`/`OpenLoop`/`InadmissiblePair`/`Gouge`/`Collision`/`NonManifoldSlice`/`StockOverflow`) with `Code`/`ToError`, composing the kernel band-2400 `GeometryFault` for the shared `DegenerateInput`.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` the closed `[Union]` fault band (band 2500) for fabrication-specific failures, mirroring the kernel `GeometryFault` shape — one case per failure carrying its detail, a `Code`/`Message` state-threaded `Switch`, and a `ToError()` that lowers the case into the `Fin<T>` failure channel.
- Cases: `FabricationFault` arms `NoFit` (a part cannot be placed on the sheet, produced by `Nesting/nfp#NESTING`) · `Unreachable` (an IK target outside the reachable workspace, produced by `Toolpath/motion#CAM_MOTION` when a reach-strict `IkPolicy` flags a non-converged solve) · `KerfCollision` (a kerf offset collapses a sub-kerf feature, produced by `Posting/program#CUT_PROGRAM`) · `OpenLoop` (a non-closed boundary a cut/toolpath/nest demands closed, produced by `Toolpath/motion`·`Nesting/nfp`·`Toolpath/skeleton`·`Posting/program`) · `InadmissiblePair` (a `(RemovalModality, CutStrategy)` pair the `Cam.Generate` `Admits` relation rejects, produced by `Toolpath/motion#CAM_MOTION`) · `Gouge` (a swept tool envelope intersects the part-keep geometry, produced by `Toolpath/guard#GUARD`) · `Collision` (a swept tool-plus-holder envelope intersects a stock/fixture keep-out, produced by `Toolpath/guard#GUARD`) · `NonManifoldSlice` (a planar section yields a non-closed or self-crossing contour, produced by `Toolpath/slicing#SLICING`) · `StockOverflow` (the multi-sheet spill exhausts the `Seq<Stock>` inventory with parts unplaced, produced by `Nesting/nfp#NESTING`) (9); the shared `DegenerateInput` (empty or non-finite primitive set) routes the kernel `GeometryFault`, never re-cased here.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries; both `FabricationFault` and the kernel `GeometryFault` are union values lowered onto the rail through `<Fault>.<Case>(...).ToError()` — `FabricationFault.NoFit(detail).ToError()` and `GeometryFault.DegenerateInput(detail).ToError()` build the band-coded `Error` the `Fin<T>` failure channel carries — so one lowering idiom serves both families on the single `Fin<FabricationResult>` rail every fabrication entrypoint returns, and a kernel route and a fabrication route compose without a second rail.
- Auto: each fabrication owner routes the most specific fault, lowering it with `.ToError()` — `Nest.Solve` routes `FabricationFault.NoFit(...).ToError()` when no placement fits the single feasibility set, `FabricationFault.StockOverflow(...).ToError()` when the multi-sheet spill exhausts the `Seq<Stock>` inventory with parts still unplaced, and `FabricationFault.OpenLoop(...).ToError()` when a part outline is non-closed; `Cam.Generate` routes `FabricationFault.InadmissiblePair(...).ToError()` when the `Admits` relation rejects the `(RemovalModality, CutStrategy)` pair rather than emitting an empty move set; `Ik.Solve` stamps the residual and a false reached flag, and `Cam.Solve` reads that stamp — under a permissive `IkPolicy` it threads the residual onto the `Motion` receipt unfailed, under a reach-strict `IkPolicy.ReachStrict` it routes `FabricationFault.Unreachable(...).ToError()` when the solve does not converge inside tolerance; `Guard.Check` routes `FabricationFault.Gouge(...).ToError()` when the swept envelope intersects the part-keep geometry and `FabricationFault.Collision(...).ToError()` against a stock/fixture keep-out; `Slice.Section` routes `FabricationFault.NonManifoldSlice(...).ToError()` on a topologically-broken closed contour; `Posting.Kerf` routes `FabricationFault.KerfCollision(...).ToError()` on a collapsed offset; an empty or non-finite primitive set lowers `GeometryFault.DegenerateInput(...).ToError()`.
- Receipt: `FabricationFault` is the typed fault evidence on the `Fin<T>` failure rail; no generic `IFault`/error-code abstraction, the union cases stay typed per fabrication concern.
- Packages: `Rasm.Geometry` (the kernel `GeometryFault` band-2400 — composed for the shared degenerate-input failure), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Error`/`Fin`), BCL inbox.
- Growth: a new fabrication-specific failure is one `FabricationFault` arm carrying the next ordinal in the fabrication band (the next free ordinal is 2510, the `TOOLPATH_STRATEGY_MODALITY_FACTOR`/`GUARD`/`SLICING`/`NESTING` producers having claimed 2505-2509); a shared geometry failure is never re-cased here and routes the kernel `GeometryFault`; zero new band.
- Boundary: `FabricationFault` mints ONLY the fabrication-specific cases and composes the kernel band-2400 `GeometryFault` for the shared geometry failure — a parallel band that re-cases `DegenerateInput`, or a synthesized `GeometryFault.OpenLoop` case the kernel union never declares, is the deleted form; both families lower onto the rail through `.ToError()` and a case passed bare into `Fin.Fail<T>(...)` without that lowering is the named seam defect; faults route through the `Fin`/`Validation`/`Eff` rails and exception-style control flow in domain logic is the named defect; the folder keeps no string-comparer accessor because every smart-enum dispatches through the generated `Switch` and keys no dictionary, so a `[KeyMemberComparer]` accessor with zero referencing owners is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Fabrication;

// --- [ERRORS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FabricationFault {
    private FabricationFault() { }

    public sealed record NoFit(string Detail) : FabricationFault;
    public sealed record Unreachable(string Detail) : FabricationFault;
    public sealed record KerfCollision(string Detail) : FabricationFault;
    public sealed record OpenLoop(string Detail) : FabricationFault;
    public sealed record InadmissiblePair(string Detail) : FabricationFault;
    public sealed record Gouge(string Detail) : FabricationFault;
    public sealed record Collision(string Detail) : FabricationFault;
    public sealed record NonManifoldSlice(string Detail) : FabricationFault;
    public sealed record StockOverflow(string Detail) : FabricationFault;

    public int Code =>
        Switch(
            noFit:            static _ => 2501,
            unreachable:      static _ => 2502,
            kerfCollision:    static _ => 2503,
            openLoop:         static _ => 2504,
            inadmissiblePair: static _ => 2505,
            gouge:            static _ => 2506,
            collision:        static _ => 2507,
            nonManifoldSlice: static _ => 2508,
            stockOverflow:    static _ => 2509);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            noFit:            static f => $"fabrication:no-fit:{f.Detail}",
            unreachable:      static f => $"fabrication:unreachable:{f.Detail}",
            kerfCollision:    static f => $"fabrication:kerf-collision:{f.Detail}",
            openLoop:         static f => $"fabrication:open-loop:{f.Detail}",
            inadmissiblePair: static f => $"fabrication:inadmissible-pair:{f.Detail}",
            gouge:            static f => $"fabrication:gouge:{f.Detail}",
            collision:        static f => $"fabrication:collision:{f.Detail}",
            nonManifoldSlice: static f => $"fabrication:non-manifold-slice:{f.Detail}",
            stockOverflow:    static f => $"fabrication:stock-overflow:{f.Detail}");
}
```
