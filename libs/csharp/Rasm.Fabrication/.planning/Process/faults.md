# [RASM_FABRICATION_FAULTS]

The fabrication fault rail: `FabricationFault` a `[Union]` band for the fabrication-specific failures no shared geometry fault covers — the original four `NoFit` (a part cannot be placed on the sheet under every rotation), `Unreachable` (a robot-cell target lies outside the serial chain's reachable workspace, joint limits, or through a singularity — the `Robots` `Program.Errors` diagnostic the cell solve folds), `KerfCollision` (a kerf offset collapses a feature narrower than the cut width), `OpenLoop` (a fabrication boundary the kernel admitted but a cut/toolpath/nest demands closed), plus the widened-axis and new-sub-domain arms `InadmissiblePair` (a `(RemovalModality, CutStrategy)` pair the `Admits` relation rejects — turning's `radial-sweep` on a cartesian-gantry laser), `Gouge` (a swept tool envelope intersects the part-keep geometry), `Collision` (a swept tool-plus-holder envelope intersects a stock/fixture keep-out), `NonManifoldSlice` (an additive planar section yields a non-closed or self-crossing contour the slicer cannot emit), `StockOverflow` (a part batch exceeds the supplied `Seq<Stock>` inventory after the multi-sheet spill exhausts), and the re-homed `Nest` (a malformed cutting-stock JOB — empty cut list, part exceeds stock, heterogeneous mass-cut — produced by `Nesting/stock#STOCK_NEST` `StockNest.Pack`; distinct from `NoFit`/`StockOverflow` by PRODUCER: `Nest` rejects the job, `NoFit`/`StockOverflow` are true-shape placement outcomes from `Nest.Solve`) — composing the kernel band-2400 `Rasm/Numerics/faults#FAULT_BAND` `GeometryFault` for the shared `DegenerateInput` (empty or non-finite primitive set). The band rides the federation `FaultBand` registry (`Rasm.Element/Projection/fault#FAULT_TABLES`): `FaultBand.Fabrication` reserves 2700 — re-banded off the live 2501-2509 collision inside Element's 2500 band, the persisted-boundary probe confirming no 25xx Fabrication code crosses a persisted or wire boundary (the page's HOST-LOCAL posture) — and every per-case code derives `FaultBand.Fabrication + n`, the nine wire-visible offsets preserved. The band-ownership resolution: a degenerate or non-finite primitive set is a geometry failure that routes the kernel `GeometryFault.DegenerateInput`; a non-closed boundary is a fabrication contract (the kernel admits an open loop, the cut/toolpath/nest concern rejects it), so it mints a `FabricationFault.OpenLoop` arm rather than a kernel case the kernel union never declared. Each new arm is a genuinely fabrication-specific rejection the kernel union never declares — a non-finite slice vertex stays `GeometryFault.DegenerateInput` (the slicer routes `NonManifoldSlice` only for a topologically-broken closed section, never a coordinate defect), and `Gouge`/`Collision` stay distinct from `KerfCollision` (the kerf-offset collapse), the swept-envelope intersection a separate safety concern from the sub-kerf feature. The folder carries no string-comparer accessor: every fabrication `[SmartEnum<string>]` (`CutStrategy`, `ClipOp`, `OffsetEnds`, `GCommand`, `LeadStyle`, `Magazine`, `PlacementMode`) dispatches through its generated total `Switch` or a behavior column and keys no `FrozenDictionary`, and the `Process/owner#FABRICATION_OWNER` `Run` dispatch is the generated `FabricationPolicy` union `Switch`, so no owner needs a `[KeyMemberComparer]` and a comparer accessor with zero referencing owners is the deleted form.

Wire posture: HOST-LOCAL. `FabricationFault` rides the `Fin<T>` rail every fabrication entrypoint returns; it never sits between wire and rail.

## [01]-[INDEX]

- [01]-[FAULT_BAND]: owns the `FabricationFault` `[Union]` on the registry band `FaultBand.Fabrication` = 2700 (`NoFit`/`Unreachable`/`KerfCollision`/`OpenLoop`/`InadmissiblePair`/`Gouge`/`Collision`/`NonManifoldSlice`/`StockOverflow`/`Nest`) with offset-derived `Code`/`ToError`, composing the kernel band-2400 `GeometryFault` for the shared `DegenerateInput`.

## [02]-[FAULT_BAND]

- Owner: `FabricationFault` the closed `[Union]` fault band on the registry row `FaultBand.Fabrication` (2700) for fabrication-specific failures, mirroring the kernel `GeometryFault` shape — one case per failure carrying its detail, a `Code` `Switch` deriving `FaultBand.Fabrication + n` per case, a `Message` `Switch`, and a `ToError()` that lowers the case into the `Fin<T>` failure channel.
- Cases: `FabricationFault` arms `NoFit` (a part cannot be placed on the sheet, produced by `Nesting/nfp#NESTING`) · `Unreachable` (a robot-cell target outside the reachable workspace / joint limits / through a singularity, produced by the `Toolpath/kinematics#ROBOT_CELL` `RobotProgram.Solve` folding a non-empty `Robots` `Program.Errors` under a reach-strict `CellPolicy`, surfaced through `Toolpath/motion#CAM_MOTION`) · `KerfCollision` (a kerf offset collapses a sub-kerf feature, produced by `Posting/program#CUT_PROGRAM`) · `OpenLoop` (a non-closed boundary a cut/toolpath/nest demands closed, produced by `Toolpath/motion`·`Nesting/nfp`·`Toolpath/skeleton`·`Posting/program`) · `InadmissiblePair` (a `(RemovalModality, CutStrategy)` pair the `Cam.Generate` `Admits` relation rejects, produced by `Toolpath/motion#CAM_MOTION`) · `Gouge` (a swept tool envelope intersects the part-keep geometry, produced by `Toolpath/guard#GUARD`) · `Collision` (a swept tool-plus-holder envelope intersects a stock/fixture keep-out, produced by `Toolpath/guard#GUARD`) · `NonManifoldSlice` (a planar section yields a non-closed or self-crossing contour, produced by `Toolpath/slicing#SLICING`) · `StockOverflow` (the multi-sheet spill exhausts the `Seq<Stock>` inventory with parts unplaced, produced by `Nesting/nfp#NESTING`) · `Nest` (a malformed cutting-stock job — empty cut list, part exceeds stock, heterogeneous mass-cut — produced by `Nesting/stock#STOCK_NEST` `StockNest.Pack`) (10); the shared `DegenerateInput` (empty or non-finite primitive set) routes the kernel `GeometryFault`, never re-cased here.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries; both `FabricationFault` and the kernel `GeometryFault` are union values lowered onto the rail through `<Fault>.<Case>(...).ToError()` — `FabricationFault.NoFit(detail).ToError()` and `GeometryFault.DegenerateInput(detail).ToError()` build the band-coded `Error` the `Fin<T>` failure channel carries — so one lowering idiom serves both families on the single `Fin<FabricationResult>` rail every fabrication entrypoint returns, and a kernel route and a fabrication route compose without a second rail.
- Auto: each fabrication owner routes the most specific fault, lowering it with `.ToError()` — `Nest.Solve` routes `FabricationFault.NoFit(...).ToError()` when no placement fits the single feasibility set, `FabricationFault.StockOverflow(...).ToError()` when the multi-sheet spill exhausts the `Seq<Stock>` inventory with parts still unplaced, and `FabricationFault.OpenLoop(...).ToError()` when a part outline is non-closed; `Cam.Generate` routes `FabricationFault.InadmissiblePair(...).ToError()` when the `Admits` relation rejects the `(RemovalModality, CutStrategy)` pair rather than emitting an empty move set; `RobotProgram.Solve` reads the `Robots` `Program.Errors`/`Warnings` after the look-ahead compile — under a permissive `CellPolicy` it threads the `Duration`/`Warnings` onto the `Motion` receipt unfailed, under a reach-strict `CellPolicy` it routes `FabricationFault.Unreachable(...).ToError()` when `Program.Errors` is non-empty (the cell's joint-limit/singularity/reach diagnostics), the `Cam.Solve` propagating that `Fin`; `Guard.Check` routes `FabricationFault.Gouge(...).ToError()` when the swept envelope intersects the part-keep geometry and `FabricationFault.Collision(...).ToError()` against a stock/fixture keep-out; `Slice.Section` routes `FabricationFault.NonManifoldSlice(...).ToError()` on a topologically-broken closed contour; `Posting.Kerf` routes `FabricationFault.KerfCollision(...).ToError()` on a collapsed offset; an empty or non-finite primitive set lowers `GeometryFault.DegenerateInput(...).ToError()`.
- Receipt: `FabricationFault` is the typed fault evidence on the `Fin<T>` failure rail; no generic `IFault`/error-code abstraction, the union cases stay typed per fabrication concern.
- Packages: `Rasm.Numerics` (the kernel `GeometryFault` band-2400 — composed for the shared degenerate-input failure), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Error`/`Fin`), BCL inbox.
- Growth: a new fabrication-specific failure is one `FabricationFault` arm carrying the next offset on the registry row (the next free offset is `FaultBand.Fabrication + 11`, the cutting-stock `Nest` case having claimed `+ 10`); a shared geometry failure is never re-cased here and routes the kernel `GeometryFault`; zero new band — band allocation lives in the ONE federation `FaultBand` registry, never a page-local integer.
- Boundary: `FabricationFault` mints ONLY the fabrication-specific cases and composes the kernel band-2400 `GeometryFault` for the shared geometry failure — a parallel band that re-cases `DegenerateInput`, or a synthesized `GeometryFault.OpenLoop` case the kernel union never declares, is the deleted form; both families lower onto the rail through `.ToError()` and a case passed bare into `Fin.Fail<T>(...)` without that lowering is the named seam defect; faults route through the `Fin`/`Validation`/`Eff` rails and exception-style control flow in domain logic is the named defect; the folder keeps no string-comparer accessor because every smart-enum dispatches through the generated `Switch` and keys no dictionary, so a `[KeyMemberComparer]` accessor with zero referencing owners is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;                  // FaultBand — the federation band-allocation registry (Projection/fault#FAULT_TABLES)
using Thinktecture;

namespace Rasm.Fabrication;

// --- [ERRORS] -----------------------------------------------------------------------------
// The nine wire-visible per-case codes are PRESERVED as offsets under the re-banded registry row
// (FaultBand.Fabrication = 2700, off the live 2501-2509 collision inside Element's 2500 band),
// plus the re-homed cutting-stock Nest case; message strings are unchanged.
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
    public sealed record Nest(string Detail) : FabricationFault;   // the cutting-stock job fault (Nesting/stock StockNest.Pack)

    public int Code =>
        Switch(
            noFit:            static _ => FaultBand.Fabrication + 1,
            unreachable:      static _ => FaultBand.Fabrication + 2,
            kerfCollision:    static _ => FaultBand.Fabrication + 3,
            openLoop:         static _ => FaultBand.Fabrication + 4,
            inadmissiblePair: static _ => FaultBand.Fabrication + 5,
            gouge:            static _ => FaultBand.Fabrication + 6,
            collision:        static _ => FaultBand.Fabrication + 7,
            nonManifoldSlice: static _ => FaultBand.Fabrication + 8,
            stockOverflow:    static _ => FaultBand.Fabrication + 9,
            nest:             static _ => FaultBand.Fabrication + 10);

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
            stockOverflow:    static f => $"fabrication:stock-overflow:{f.Detail}",
            nest:             static f => $"fabrication:nest:{f.Detail}");
}
```
