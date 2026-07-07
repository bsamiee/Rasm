# [RASM_FABRICATION_STOCK]

THE CUTTING-STOCK / SHEET-YIELD OWNER and THE ONE RECTANGULAR NESTING FOLD. One `NestRun` is a parameterized cut job — a `Seq<CutPart>` rectangular cut-list, a `StockSheet` stock extent, and a `NestStrategy` — and one `NestPlan` is the resolved multi-sheet layout the run folds to through the single `StockNest.Pack` fold over the `RectangleBinPack.CSharp` academic 2D bin-packing suite, with a typed `NestYield` material-utilization receipt carrying the per-sheet `SheetYield` ledger. This is the AABB-rectangle cutting-stock engine: given a cut-list and stock-sheet dimensions, place every part to maximize material yield, drive across as many sheets as the parts require, and report the per-sheet utilization/waste/remnant/cut-length evidence — the material-planning YIELD concern (minimum sheets, re-stockable offcuts, embodied-carbon waste), DISTINCT from the sibling `Nesting/nfp#NESTING` true-shape irregular NEST (the CAM concern: cut-program transforms). The owner is HOST-NEUTRAL: a `NestPlacement` carries a `PartId` indexing the `FabricationInput.Profiles`, a sheet-index, a scalar `X`/`Y` position, and a rotation flag, never a `Rhino.Geometry` transform — the host materializes the cutting plan, and the resolved `NestPlan` rides `FabricationInput.Plan` straight into the sibling `Nest.Honor` fold (same package, no wire, no mediator), which maps each `NestPlacement` to a `PartTransform` by the rotation flag and lower-left offset, re-packing nothing. The SIX academic packer surfaces (`MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack` plus the heuristic-sweep best-of) and their distinct heuristic enums collapse into ONE `NestStrategy` `[Union]` whose `Switch` the `Pack` fold dispatches — a new algorithm is one `NestStrategy` case, never a parallel packer surface. The page composes `RectangleBinPack.CSharp` as the packing kernel, the seam `Rasm.Element` `MaterialId` for the material a part is cut from, the `Rasm` kernel `PositiveMagnitude` for every dimension, the `Process/faults#FAULT_BAND` `FabricationFault.Nest` rail for the malformed cutting-stock JOB (empty cut list, part exceeds stock, heterogeneous mass-cut), and the kernel `GeometryFault.DegenerateInput` for the degenerate/trim-exceeded sheet per the folder band-ownership law; the embodied-carbon/material-cost rollup the `NestYield.WasteAreaMm2` feeds is the `Rasm.Compute` `AggregateEnvironmental`/`AggregateCost` fold reading the seam `Material` node's `Environmental`/`Cost` cases, never a direct sustainability read.

Wire posture: HOST-LOCAL. The `NestPlan` crosses only the in-package seam to `Nesting/nfp#NESTING` `Nest.Honor` on `FabricationInput.Plan`; the `NestYield` waste evidence feeds the `Rasm.Compute` rollup through the seam `Material` node. No type on this page sits between wire and rail.

## [01]-[INDEX]

- [01]-[NEST_STRATEGY]: the `NestStrategy` `[Union]` collapsing the five packers plus the heuristic-sweep best-of, each case carrying its own packer's correctly-typed heuristic enum, and the `NestFamily` `[SmartEnum<string>]` naming which packer a sweep exhausts.
- [02]-[STOCK_MODEL]: the `CutPart` cut-list part (`PartId`-keyed to the `FabricationInput.Profiles`), the `StockSheet` stock extent with its trim inset, the `NestPlacement` host-neutral placement, the `SheetYield` per-sheet ledger row, the `NestYield` material-utilization receipt, and the `NestRun`/`NestPlan` job/layout pair.
- [03]-[STOCK_NEST]: the one `StockNest.Pack` multi-sheet fold — validation rails, the int-domain admission, the `Drive` kernel exemption, the heuristic sweep, and the per-sheet ledger projection.

## [02]-[NEST_STRATEGY]

- Owner: `NestStrategy` `[Union]` the packer-algorithm axis; `NestFamily` `[SmartEnum<string>]` the sweepable packer family.
- Cases: strategy {max-rects (densest general nesting, exposes `UsedRectangles`/`FreeRectangles` remnants), skyline (fast streaming for many uniform parts), guillotine (the PANEL-SAW straight-cut constraint, every cut edge-to-edge, exposes post-merge remnants), shelf (row-band), mass-cut (`SingleBinPack` homogeneous identical-part sheet-yield), best (a heuristic SWEEP — the same packer driven once per heuristic in its vocabulary, the highest-yield run kept)} — the closed `NestStrategy` `[Union]`, each interactive case carrying its OWN packer's correctly-typed heuristic enum (`FreeRectChoiceHeuristic` for max-rects, the DISTINCT nested `GuillotineBinPack.FreeRectChoiceHeuristic` + `GuillotineSplitHeuristic` for guillotine, `LevelChoiceHeuristic` for skyline, `ShelfChoiceHeuristic` for shelf) and the sweep case carrying a `NestFamily` discriminant naming which packer to exhaust; a new packing algorithm is one `NestStrategy` case, never a parallel packer class.
- Growth: a new packing algorithm is one `NestStrategy` `[Union]` case binding its packer + heuristic and one `Drive` switch arm; a new sweepable family is one `NestFamily` row plus one `BestOf` arm.

## [03]-[STOCK_MODEL]

- Owner: `CutPart` the rectangular cut-list part keyed by `PartId` to the `FabricationInput.Profiles` and by the `Rasm.Element` `MaterialId` seam to the material it is cut from; `StockSheet` the stock extent and saw geometry (`KerfMm` blade width, `TrimMm` unusable edge inset); `NestPlacement` the host-neutral placed cut; `SheetYield` the per-sheet ledger row; `NestYield` the typed material-utilization receipt; `NestRun` the parameterized cut job; `NestPlan` the resolved multi-sheet layout.
- Cases: `CutPart.AllowRotation` IS the grain-lock axis (a grain-locked part forbids the 90° flip); `CutPart.MarginMm` the edge-banding/finish oversize the footprint reserves alongside the saw kerf; `StockSheet.TrimMm` the gripper/clamp margin or mill-edge defect band shrinking the packable extent to `(WidthMm − 2·TrimMm, HeightMm − 2·TrimMm)`; quantity expansion copies `PartId`, so N placements of one part map N transforms of ONE profile at the `Nest.Honor` seam.
- Receipt: the typed `NestYield` carries algorithm evidence over the per-sheet `SheetYield` ledger, never a generic `IReceipt` — the aggregate columns (`UtilizationRatio`/`PlacedCount`/`UnplacedCount`/`SheetCount`/`WasteAreaMm2`) are what `Nest.Honor` reads onto the honored `Placement`, and `WasteAreaMm2` is the Compute out-seam value for material-cost and embodied-carbon rollups through the seam material node; `RemnantAreaMm2` the total re-stockable offcut summed across sheets; `CutLengthMm` the total saw travel; `Sheets` the per-sheet detail a re-stocking inventory and a cut-shop schedule read.
- Growth: a new part attribute (a defect map) is one `CutPart` column the fit gate reads; a new sheet attribute (a defect zone) is one `StockSheet` column the packable region subtracts; a new utilization metric is one `NestYield`/`SheetYield` field — never a parallel nesting owner.

## [04]-[STOCK_NEST]

- Entry: `public static Fin<NestPlan> Pack(NestRun run)` — the host-neutral cutting-stock fold: validate the cut-list and sheet, ceil every part footprint and the trim-inset sheet extent to the `int` packing domain (the `RectangleBinPack` coordinate type) adding the saw `KerfMm` and the per-part `MarginMm`, sort the parts descending by area (the placement quality is order-sensitive), then `Drive` the strategy's packer `Insert` stream across as many sheets as the parts require — a `Rect.Height == 0` return is the per-sheet infeasibility sentinel that opens a fresh sheet and re-feeds the unplaced part; the returned `Rect.X`/`Y` offset back into scalar `NestPlacement` millimetres and the `NestYield` derives material-utilization from the per-sheet placed-area sums against the usable sheet area. `Fin<T>` aborts on an empty cut-list (`FabricationFault.Nest`), a degenerate or trim-exceeded sheet (the kernel `GeometryFault.DegenerateInput` — a degenerate primitive set is the kernel's per the folder band-ownership law), a part exceeding the usable sheet (`FabricationFault.Nest`), or a heterogeneous cut-list under the `MassCut` strategy (`FabricationFault.Nest` — `MassCut` is a one-footprint homogeneous yield, so a non-uniform footprint is rejected rather than silently mis-cut as copies of the head).
- Packages: RectangleBinPack.CSharp (the `MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack` packers, the `Rect` value struct with the `Height == 0` sentinel and the `Width`/`Height`/`X`/`Y` int fields, the five heuristic enums; assembly id `RectangleBinPacking`; `.api/api-rectanglebinpack-csharp.md`), Rasm.Element (project — `MaterialId` the cut part is cut from), Rasm (project — `PositiveMagnitude` for the dimensions, `GeometryFault` for the degenerate-input rail), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Law: `Drive` is the page's `[EXPRESSION_SPINE]` kernel exemption — the stateful `RectangleBinPack` packer is a MUTABLE single-sheet state machine (`Insert` mutates its free/used lists), so the multi-sheet placement loop carries the page's only statements (the per-part `Insert` call, the `Height == 0` fresh-sheet open, the per-sheet `SheetYield` close, the cursor advance); the cut-list sort, the part expansion, the `NestPlacement` projection, the `NestYield` fold, and every other surface are expression-bodied, and the placement accumulation crosses to the immutable `Seq` only at the kernel egress, never a mutable list mutated in domain code. The `Best` sweep is itself expression-shaped — a `Fold` over the per-heuristic `DriveStream` results keeping the higher-yield run, each inner run its own confined kernel — so the sweep adds NO statement: the exemption stays the single-sheet driver, the best-of selection a pure fold.
- Boundary: `StockNest.Pack` is the ONE cutting-stock fold, never a per-algorithm packer surface; the five `RectangleBinPack` packer classes, their five heuristic enums, and the heuristic SWEEP collapse into the one `NestStrategy` `[Union]` the `Drive` `Switch` dispatches, so a max-rects nest, a guillotine panel-saw plan, a homogeneous mass-cut yield, and a best-of-sweep are the SAME `Pack` fold differing only in the strategy case; the `int` packing domain is the BOUNDARY_ADMISSION edge — part footprints (with the saw `KerfMm` AND the per-part `MarginMm` added) and the trim-inset sheet extent ceil to `int` ON THE WAY IN and the returned `Rect.X`/`Y` offset back to scalar `NestPlacement` millimetres (shifted by `TrimMm` so the placement reads in the full-sheet frame) ON THE WAY OUT, the `Rect` value struct never crossing into an interior signature; the `Rect.Height == 0` sentinel is the ONLY infeasibility signal the suite emits (it throws no typed packing exception), so `Drive` maps the zero-height return to the typed multi-sheet open or the `FabricationFault.Nest` rail rather than catching, and `Fin<NestPlan>` carries the outcome; rotation is read from the packer return ROBUSTLY — a `Rect` whose placed-width equals the rotated footprint (and differs from the un-rotated one) is a 90° flip, the square-part ambiguity resolved by `w == h ? false` so a square never reports a meaningless rotation; `max-rects` and `guillotine` alone surface their `FreeRectangles` remnant geometry, so the per-sheet `SheetYield.LargestRemnantMm2` reads the largest single reusable offcut on THAT sheet from `MaxRectsBinPack.FreeRectangles` / `GuillotineBinPack` post-`MergeFreeRectangles` (distinct from `WasteAreaMm2`, the total scrap), the `skyline`/`shelf`/`mass-cut` strategies whose free lists stay private reporting a 0 remnant while the utilization ratio derives from the placed-area sum either way; the resolved `NestPlan` is portable scalar data (a `Seq<NestPlacement>` of `PartId` + sheet-index + position + rotation tuples + the per-sheet `SheetYield` ledger) the sibling `Nest.Honor` fold consumes DIRECTLY on `FabricationInput.Plan` — same package, no wire mirror, no app-stratum mediator — mapping each `NestPlacement` to a `PartTransform`, this owner keeping the rectangular cutting-stock YIELD (procurement/sustainability: minimum sheets, per-sheet offcuts, embodied-carbon waste) and `Nesting/nfp` the true-shape irregular NEST (CAM: cut-program transforms), neither re-packing the other; `MaterialId` binds the material in-seam, and `NestYield.WasteAreaMm2` binds the Compute out-seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;                  // [Union] (NestStrategy), [SmartEnum<string>] (NestFamily)
using Rasm.Numerics;                  // PositiveMagnitude (the kernel dimensional value-object) + GeometryFault (the kernel band-2400 degenerate-input rail)
using Rasm.Element;                  // MaterialId (the seam-carried material identity a cut part is cut from)
using RectangleBinPacking;           // MaxRectsBinPack/SkylineBinPack/GuillotineBinPack/ShelfBinPack/SingleBinPack, Rect, the heuristic enums
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] -------------------------------------------------------------------------------
// One NestStrategy [Union] collapses the five RectangleBinPack packer classes PLUS the heuristic-sweep best-of, each
// interactive case carrying its OWN packer's correctly-typed heuristic enum — never magic ordinals, never six parallel
// packer surfaces. A new algorithm is a case; a Best sweep is a case carrying which packer Family to exhaust.
[Union]
public abstract partial record NestStrategy {
    private NestStrategy() { }
    public sealed record MaxRects(FreeRectChoiceHeuristic Choice, bool AllowRotation) : NestStrategy;
    public sealed record Skyline(SkylineBinPack.LevelChoiceHeuristic Level, bool UseWasteMap) : NestStrategy;
    public sealed record Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic Choice, GuillotineBinPack.GuillotineSplitHeuristic Split, bool Merge) : NestStrategy;
    public sealed record Shelf(ShelfBinPack.ShelfChoiceHeuristic Choice, bool UseWasteMap) : NestStrategy;
    public sealed record MassCut(bool AllowRotation) : NestStrategy;
    // The heuristic SWEEP: drive ONE packer family once per heuristic in its vocabulary and keep the highest-yield run.
    // The suite ships no built-in best-of, and a single fixed heuristic leaves yield on the table — so the sweep is the
    // cutting-stock "best fit" the production cut-shop wants when the part mix is unknown ahead of time.
    public sealed record Best(NestFamily Family, bool AllowRotation) : NestStrategy;

    // The intent shortcuts — a job names its algorithm, not its heuristic ordinal.
    public static readonly NestStrategy Density = new MaxRects(FreeRectChoiceHeuristic.RectBestShortSideFit, AllowRotation: true);
    public static readonly NestStrategy PanelSaw = new Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic.RectBestAreaFit, GuillotineBinPack.GuillotineSplitHeuristic.SplitMinimizeArea, Merge: true);
    public static readonly NestStrategy Optimal = new Best(NestFamily.MaxRects, AllowRotation: true);
}

// The packer family the Best sweep exhausts — names which packer's heuristic vocabulary the sweep drives, never a
// second packer surface (the Switch routes each family to the SAME DriveStream the interactive cases use).
[SmartEnum<string>]
public sealed partial class NestFamily {
    public static readonly NestFamily MaxRects = new("max-rects");
    public static readonly NestFamily Guillotine = new("guillotine");
    public static readonly NestFamily Shelf = new("shelf");
}

// --- [MODELS] ------------------------------------------------------------------------------
// A rectangular cut-list part: PartId indexes FabricationInput.Profiles (the sibling Nest.Honor alignment column —
// quantity expansion copies it, so N placements of one part map N transforms of one profile), Material keys the seam
// catalogue row the part is cut from. AllowRotation IS the grain-lock axis (a grain-locked part forbids the 90° flip);
// MarginMm is the edge-banding / finish-oversize the footprint reserves alongside the saw kerf, 0 for a raw cut.
public readonly record struct CutPart(int PartId, MaterialId Material, PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, int Quantity, bool AllowRotation = true, double MarginMm = 0.0) {
    public double AreaMm2 => WidthMm.Value * HeightMm.Value;
}

// The stock extent and the saw geometry. KerfMm is the blade width reserved per cut; TrimMm is the unusable edge inset
// (the gripper/clamp margin or mill-edge defect band) the packable area shrinks by on every side, so the usable extent
// is (Width − 2·Trim, Height − 2·Trim) and a placement reads back in the full-sheet frame shifted by TrimMm.
public readonly record struct StockSheet(MaterialId Material, PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, double KerfMm, double TrimMm = 0.0) {
    public double AreaMm2 => WidthMm.Value * HeightMm.Value;
    public double UsableWidthMm => Math.Max(0.0, WidthMm.Value - 2.0 * TrimMm);
    public double UsableHeightMm => Math.Max(0.0, HeightMm.Value - 2.0 * TrimMm);
    public double UsableAreaMm2 => UsableWidthMm * UsableHeightMm;
}

// The host-neutral placed cut — PartId + sheet index + scalar position + rotation flag, never a Rhino transform. The
// sibling Nest.Honor fold maps this straight to a PartTransform (rotate by the flag, offset to the lower-left).
public readonly record struct NestPlacement(int PartId, MaterialId Material, int SheetIndex, double XMm, double YMm, double WidthMm, double HeightMm, bool Rotated) {
    // The cut perimeter the cut-length receipt sums — the saw travel to free this part from the stock.
    public double PerimeterMm => 2.0 * (WidthMm + HeightMm);
}

// The per-sheet ledger row — the cutting-stock leftover ledger is PER SHEET, not last-sheet-only: a cut-shop schedules
// each sheet, re-stocks each sheet's largest offcut, and the procurement rollup reads each sheet's waste. Utilization is
// the placed area against THIS sheet's usable area; LargestRemnantMm2 the re-stockable offcut the FreeRectangles-surfacing
// packers report on this sheet (0 for the others); CutLengthMm the saw travel on this sheet.
public readonly record struct SheetYield(int SheetIndex, int PlacedCount, double UtilizationRatio, double LargestRemnantMm2, double CutLengthMm);

// The typed material-utilization receipt — algorithm evidence over the per-sheet ledger, never a generic IReceipt. The
// aggregate columns (UtilizationRatio/PlacedCount/UnplacedCount/SheetCount/WasteAreaMm2) are what Nest.Honor reads onto
// the honored Placement; RemnantAreaMm2 is the total re-stockable offcut summed across sheets, CutLengthMm the total saw
// travel; Sheets is the per-sheet detail a re-stocking inventory and a cut-shop schedule read.
public readonly record struct NestYield(double UtilizationRatio, int PlacedCount, int UnplacedCount, int SheetCount, double RemnantAreaMm2, double WasteAreaMm2, double CutLengthMm, Seq<SheetYield> Sheets);

public sealed record NestRun(Seq<CutPart> Parts, StockSheet Sheet, NestStrategy Strategy) {
    public static NestRun Of(Seq<CutPart> parts, StockSheet sheet) => new(parts, sheet, NestStrategy.Density);
}

public sealed record NestPlan(NestRun Run, Seq<NestPlacement> Placements, NestYield Yield);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StockNest {
    private const double ResolutionMm = 1.0;   // ceil the fractional-mm footprints to the int packing domain at 1 mm

    public static Fin<NestPlan> Pack(NestRun run) =>
        from nonEmpty in guard(!run.Parts.IsEmpty, FabricationFault.Nest("<empty-cut-list>").ToError())
        from sheetOk in guard(run.Sheet.WidthMm.Value > 0.0 && run.Sheet.HeightMm.Value > 0.0, GeometryFault.DegenerateInput("stock:degenerate-sheet").ToError())
        from trimOk in guard(run.Sheet.UsableWidthMm > 0.0 && run.Sheet.UsableHeightMm > 0.0, GeometryFault.DegenerateInput("stock:trim-exceeds-sheet").ToError())
        let sheetW = (int)Math.Floor(run.Sheet.UsableWidthMm / ResolutionMm)
        let sheetH = (int)Math.Floor(run.Sheet.UsableHeightMm / ResolutionMm)
        let parts = Expand(run.Parts).OrderByDescending(static p => p.AreaMm2).ToSeq()   // order-sensitive: descending area
        // Rotation is a per-run policy the strategy carries; a single grain-locked CutPart disables rotation for the run
        // (AllowRotation is load-bearing), and the fit gate admits a part that only fits rotated when the run rotates.
        let allowRotation = StrategyRotates(run.Strategy) && parts.ForAll(static p => p.AllowRotation)
        from fits in guard(parts.ForAll(p => FitsSheet(p, run.Sheet.KerfMm, sheetW, sheetH, allowRotation)),
            FabricationFault.Nest("<part-exceeds-stock-sheet>").ToError())
        // MassCut drives SingleBinPack over ONE footprint × quantity (the head), so a heterogeneous cut-list under the
        // MassCut strategy silently mis-cuts the tail as copies of the head absent a guard. The strategy NAMES a
        // homogeneous yield query, so a non-uniform footprint is a malformed job railed HERE rather than a silent drop —
        // the interactive packers (max-rects/skyline/guillotine/shelf/best) place each distinct part and impose no
        // homogeneity. The MassCut test is a case pattern (one hop, no parallel total Switch a new strategy must grow an arm in).
        from uniform in guard(run.Strategy is not NestStrategy.MassCut || Homogeneous(parts),
            FabricationFault.Nest("<mass-cut-heterogeneous-cut-list>").ToError())
        let nest = Drive(run.Strategy, parts, run.Sheet, sheetW, sheetH, allowRotation)   // Seq<(NestPlacement, double Remnant)>
        select new NestPlan(run, nest.Map(static d => d.Placement), YieldOf(nest, parts.Count, run.Sheet));

    // The per-run rotation policy each strategy carries: only MaxRects/MassCut/Best expose the 90° flip (Skyline/Guillotine/
    // Shelf are axis-fixed in this suite), so the run rotates iff its strategy admits it AND every part is rotatable.
    static bool StrategyRotates(NestStrategy strategy) => strategy.Switch(
        maxRects:   static m => m.AllowRotation,
        skyline:    static _ => false,
        guillotine: static _ => false,
        shelf:      static _ => false,
        massCut:    static m => m.AllowRotation,
        best:       static b => b.AllowRotation && b.Family == NestFamily.MaxRects);   // only the MaxRects sweep rotates

    // A homogeneous cut-list — every part shares the head's footprint (the PositiveMagnitude [ValueObject] gives
    // value-equality), so SingleBinPack's one-footprint × quantity yield is the correct cut for the whole list. Guards
    // the MassCut strategy alone (Pack tests `run.Strategy is not NestStrategy.MassCut || Homogeneous(parts)`).
    static bool Homogeneous(Seq<CutPart> parts) =>
        parts.Head.Match(
            Some: head => parts.ForAll(p => p.WidthMm == head.WidthMm && p.HeightMm == head.HeightMm),
            None: static () => true);

    // The int packing footprint — the fractional-mm extent plus the saw kerf AND the part's finish margin, ceiled.
    static int Ceil(double mm, double kerfMm, double marginMm) => (int)Math.Ceiling((mm + kerfMm + marginMm) / ResolutionMm);

    // A part fits in its given orientation, or — when the run admits rotation AND the part is not grain-locked —
    // rotated 90°; the int packing domain reserves the saw kerf and the finish margin on each footprint before the test.
    static bool FitsSheet(CutPart p, double kerfMm, int sheetW, int sheetH, bool allowRotation) {
        int w = Ceil(p.WidthMm.Value, kerfMm, p.MarginMm), h = Ceil(p.HeightMm.Value, kerfMm, p.MarginMm);
        return (w <= sheetW && h <= sheetH) || (allowRotation && p.AllowRotation && h <= sheetW && w <= sheetH);
    }

    // Each CutPart of Quantity n expands to n unit parts the packer places one at a time; PartId copies through, so
    // N placements of one part map N transforms of ONE profile at the Nest.Honor seam.
    static Seq<CutPart> Expand(Seq<CutPart> parts) =>
        parts.Bind(static p => toSeq(Enumerable.Repeat(p with { Quantity = 1 }, Math.Max(1, p.Quantity))));

    // The largest reusable offcut on one sheet's surfaced free-rect list, scaled out of the int packing domain — the
    // cutting-stock leftover-ledger value, distinct from total scrap.
    static double LargestRemnantMm2(IReadOnlyList<Rect> free) =>
        free.Count == 0 ? 0.0 : free.Max(static r => (double)r.Area) * ResolutionMm * ResolutionMm;

    // The aggregate receipt folds the per-sheet SheetYield ledger: utilization is the total placed area against the
    // total usable sheet area, RemnantAreaMm2 the SUM of every sheet's largest offcut (the whole re-stockable inventory),
    // WasteAreaMm2 the total scrap, CutLengthMm the total saw travel, and UnplacedCount the expanded parts that fit no
    // sheet (the total minus the placed — a part that fails on a fresh empty sheet is dropped by Drive and counted here).
    static NestYield YieldOf(Seq<(NestPlacement Placement, double Remnant)> driven, int totalParts, StockSheet sheet) {
        Seq<SheetYield> sheets = SheetLedger(driven, sheet);
        int sheetCount = sheets.Count;
        double usedArea = driven.Sum(static d => d.Placement.WidthMm * d.Placement.HeightMm);
        double usableArea = sheet.UsableAreaMm2 * Math.Max(1, sheetCount);
        return new NestYield(
            UtilizationRatio: usableArea > 0.0 ? Math.Clamp(usedArea / usableArea, 0.0, 1.0) : 0.0,
            PlacedCount: driven.Count,
            UnplacedCount: Math.Max(0, totalParts - driven.Count),
            SheetCount: sheetCount,
            RemnantAreaMm2: sheets.Sum(static s => s.LargestRemnantMm2),
            WasteAreaMm2: Math.Max(0.0, usableArea - usedArea),
            CutLengthMm: sheets.Sum(static s => s.CutLengthMm),
            Sheets: sheets);
    }

    // The per-sheet ledger projection — group the driven (placement, sheet-remnant) pairs by sheet index, and for each
    // sheet derive its utilization against the usable area, its cut length from the placement perimeters, and its largest
    // offcut from the per-sheet remnant the DriveStream paired onto every placement of that sheet (uniform within a sheet,
    // so the Head read is exact; 0 for the packers that surface no free-rect list).
    static Seq<SheetYield> SheetLedger(Seq<(NestPlacement Placement, double Remnant)> driven, StockSheet sheet) =>
        driven.GroupBy(static d => d.Placement.SheetIndex)
            .OrderBy(static g => g.Key)
            .Select(g => {
                Seq<(NestPlacement Placement, double Remnant)> rows = g.ToSeq();
                double sheetUsed = rows.Sum(static d => d.Placement.WidthMm * d.Placement.HeightMm);
                return new SheetYield(
                    SheetIndex: g.Key,
                    PlacedCount: rows.Count,
                    UtilizationRatio: sheet.UsableAreaMm2 > 0.0 ? Math.Clamp(sheetUsed / sheet.UsableAreaMm2, 0.0, 1.0) : 0.0,
                    LargestRemnantMm2: rows.Head.Map(static r => r.Remnant).IfNone(0.0),   // the per-sheet offcut the DriveStream stamped uniformly onto this sheet's rows
                    CutLengthMm: rows.Sum(static d => d.Placement.PerimeterMm));
            })
            .ToSeq();

    // [PACKER_DRIVE] — the [EXPRESSION_SPINE] exemption: the RectangleBinPack packers are MUTABLE single-sheet state
    // machines, so the multi-sheet Insert loop carries the page's only statements. The Switch dispatches the strategy
    // case onto its packer + heuristic + free-rect reader; a Rect.Height == 0 opens a fresh sheet and re-feeds the part.
    // Only MaxRects/Guillotine surface FreeRectangles, so the other arms pass a 0-remnant reader (the api-catalogued
    // contract — Skyline/Shelf/SingleBin keep their free lists private), guillotine merging before the remnant read. The
    // Best sweep drives the family's whole heuristic vocabulary and keeps the highest-yield run — a pure Fold, no
    // statement of its own. The driver returns each placement PAIRED with its sheet's largest-remnant, so YieldOf's
    // per-sheet ledger reads a true offcut per sheet (the pairing threads straight through to SheetLedger, not discarded).
    static Seq<(NestPlacement Placement, double Remnant)> Drive(NestStrategy strategy, Seq<CutPart> parts, StockSheet sheet, int sheetW, int sheetH, bool allowRotation) =>
        strategy.Switch(
            maxRects:   m => DriveStream(parts, sheet, allowRotation, () => new MaxRectsBinPack(sheetW, sheetH, allowRotation), (pk, w, h) => pk.Insert(w, h, m.Choice), static pk => LargestRemnantMm2(pk.FreeRectangles)),
            skyline:    s => DriveStream(parts, sheet, false, () => new SkylineBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Level), static _ => 0.0),
            guillotine: g => DriveStream(parts, sheet, false, () => new GuillotineBinPack(sheetW, sheetH), (pk, w, h) => pk.Insert(w, h, g.Merge, g.Choice, g.Split), static pk => { pk.MergeFreeRectangles(); return LargestRemnantMm2(pk.FreeRectangles); }),
            shelf:      s => DriveStream(parts, sheet, false, () => new ShelfBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Choice), static _ => 0.0),
            massCut:    m => MassCut(parts, sheet, sheetW, sheetH, m.AllowRotation && parts.ForAll(static p => p.AllowRotation)),
            best:       b => BestOf(b.Family, parts, sheet, sheetW, sheetH, allowRotation));

    // The Best sweep: drive the named family once per heuristic in its vocabulary, keep the run whose placed area is
    // greatest (the highest material yield). Each inner run is its own confined DriveStream kernel; the sweep itself is a
    // pure Fold over the candidate runs — so the [EXPRESSION_SPINE] exemption stays the single-sheet driver, never grows.
    static Seq<(NestPlacement Placement, double Remnant)> BestOf(NestFamily family, Seq<CutPart> parts, StockSheet sheet, int sheetW, int sheetH, bool allowRotation) =>
        family.Switch(
            maxRects: _ => Best(toSeq(Enum.GetValues<FreeRectChoiceHeuristic>())
                .Map(choice => DriveStream(parts, sheet, allowRotation, () => new MaxRectsBinPack(sheetW, sheetH, allowRotation), (pk, w, h) => pk.Insert(w, h, choice), static pk => LargestRemnantMm2(pk.FreeRectangles)))),
            guillotine: _ => Best(toSeq(Enum.GetValues<GuillotineBinPack.FreeRectChoiceHeuristic>())
                .Map(choice => DriveStream(parts, sheet, false, () => new GuillotineBinPack(sheetW, sheetH), (pk, w, h) => pk.Insert(w, h, true, choice, GuillotineBinPack.GuillotineSplitHeuristic.SplitMinimizeArea), static pk => { pk.MergeFreeRectangles(); return LargestRemnantMm2(pk.FreeRectangles); }))),
            shelf: _ => Best(toSeq(Enum.GetValues<ShelfBinPack.ShelfChoiceHeuristic>())
                .Map(choice => DriveStream(parts, sheet, false, () => new ShelfBinPack(sheetW, sheetH, true), (pk, w, h) => pk.Insert(w, h, choice), static _ => 0.0))));

    // The yield comparator: the run placing the most total area wins. A Fold over the candidate runs (LanguageExt Seq has
    // no MaxBy) keeps the higher-yield accumulator, the placed-rectangle areas summed in the int domain — a pure fold.
    static Seq<(NestPlacement Placement, double Remnant)> Best(Seq<Seq<(NestPlacement Placement, double Remnant)>> candidates) =>
        candidates.Fold(Seq<(NestPlacement, double)>(), static (best, run) => Yield(run) > Yield(best) ? run : best);

    static double Yield(Seq<(NestPlacement Placement, double Remnant)> run) => run.Sum(static r => r.Placement.WidthMm * r.Placement.HeightMm);

    // One generic loop over any RectangleBinPack packer (type inferred from `open`) — no boxing: the stateful single-sheet
    // packer is opened, fed one part at a time, and re-opened on a Rect.Height == 0 overflow for the next sheet. Each
    // placement is paired with its CURRENT sheet's largest-remnant reader so the per-sheet ledger reads a true offcut; the
    // remnant is re-read whenever a sheet closes (an overflow) and once more at the stream end for the final partial sheet.
    static Seq<(NestPlacement Placement, double Remnant)> DriveStream<TPacker>(Seq<CutPart> parts, StockSheet sheet, bool rotate, Func<TPacker> open, Func<TPacker, int, int, Rect> insert, Func<TPacker, double> remnant) {
        var placements = new List<(NestPlacement, double)>();   // local mutable accumulator inside the named kernel exemption
        TPacker packer = open();
        int sheetIndex = 0;
        int sheetStart = 0;   // index into `placements` where the current sheet's rows begin (for the per-sheet remnant stamp)
        foreach (CutPart part in parts) {
            int w = Ceil(part.WidthMm.Value, sheet.KerfMm, part.MarginMm), h = Ceil(part.HeightMm.Value, sheet.KerfMm, part.MarginMm);
            Rect placed = insert(packer, w, h);
            if (placed.Height == 0) {
                StampSheet(placements, sheetStart, remnant(packer));   // close the full sheet's remnant before re-opening
                packer = open(); sheetIndex++; sheetStart = placements.Count;
                placed = insert(packer, w, h);
            }
            if (placed.Height == 0) { continue; }   // unfittable on a fresh sheet, counted unplaced by the Drive return
            // Robust rotation: a 90° flip iff the placed width equals the rotated footprint AND differs from the
            // un-rotated one; a square footprint (w == h) reports no rotation rather than a meaningless flip.
            bool rotated = rotate && w != h && placed.Width == h;
            placements.Add((new NestPlacement(part.PartId, part.Material, sheetIndex, placed.X * ResolutionMm + sheet.TrimMm, placed.Y * ResolutionMm + sheet.TrimMm, placed.Width * ResolutionMm, placed.Height * ResolutionMm, rotated), 0.0));
        }
        StampSheet(placements, sheetStart, remnant(packer));   // close the final partial sheet's remnant
        return placements.ToSeq();
    }

    // Stamp the closing sheet's largest-remnant onto every placement of that sheet (the SheetLedger reads it back).
    static void StampSheet(List<(NestPlacement Placement, double Remnant)> rows, int from, double remnantMm2) {
        for (int i = from; i < rows.Count; i++) { rows[i] = (rows[i].Placement, remnantMm2); }
    }

    // The homogeneous mass-cut yield: SingleBinPack.Insert(w, h, quantity) returns the List<Rect> of as-many-as-fit
    // identical parts on one sheet — the sheet-yield count for a homogeneous cut list, the positions not just the count.
    // Pack's Homogeneous guard has already proved every part shares the head's footprint (a heterogeneous MassCut railed
    // FabricationFault.Nest), so driving the head × parts.Count is exact, never a silent tail mis-cut. When the run
    // rotates and the upright orientation fits fewer than the rotated one, the rotated footprint is driven and every
    // placement flagged rotated. SingleBinPack surfaces no free-rect list, so the mass-cut sheet reports a 0 remnant.
    // The whole result is one sheet (index 0) — SingleBinPack is a single-sheet yield query, not multi-sheet.
    static Seq<(NestPlacement Placement, double Remnant)> MassCut(Seq<CutPart> parts, StockSheet sheet, int sheetW, int sheetH, bool allowRotation) =>
        parts.Head.Match(
            Some: part => {
                int w = Ceil(part.WidthMm.Value, sheet.KerfMm, part.MarginMm), h = Ceil(part.HeightMm.Value, sheet.KerfMm, part.MarginMm);
                int qty = Math.Max(1, parts.Count);
                Seq<Rect> upright = toSeq(new SingleBinPack(sheetW, sheetH).Insert(w, h, qty));
                // Honor rotation: a square part never rotates, otherwise drive the rotated footprint when it yields more.
                Seq<Rect> rotated = allowRotation && w != h ? toSeq(new SingleBinPack(sheetW, sheetH).Insert(h, w, qty)) : Seq<Rect>();
                bool useRotated = rotated.Count > upright.Count;
                return (useRotated ? rotated : upright).Map(r => (
                    new NestPlacement(part.PartId, part.Material, 0, r.X * ResolutionMm + sheet.TrimMm, r.Y * ResolutionMm + sheet.TrimMm, r.Width * ResolutionMm, r.Height * ResolutionMm, Rotated: useRotated),
                    0.0));
            },
            None: () => Seq<(NestPlacement, double)>());
}
```
