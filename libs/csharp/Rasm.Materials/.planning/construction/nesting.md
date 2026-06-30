# [MATERIALS_NESTING]

THE CUTTING-STOCK / SHEET-YIELD OWNER and THE ONE NESTING FOLD. One `NestRun` is a parameterized cut job — a `Seq<CutPart>` rectangular cut-list, a `StockSheet` stock extent, and a `NestStrategy` — and one `NestPlan` is the resolved multi-sheet layout the run folds to through the single `StockNest.Pack` fold over the `RectangleBinPack.CSharp` academic 2D bin-packing suite, with a typed `NestYield` material-utilization receipt carrying the per-sheet `SheetYield` ledger. This is the AABB-rectangle cutting-stock engine: given a cut-list and stock-sheet dimensions, place every part to maximize material yield, drive across as many sheets as the parts require, and report the per-sheet utilization/waste/remnant/cut-length evidence — the strata-acyclic placement pin an AEC-DOMAIN peer requires, since `Rasm.Materials` cannot reach sideways into `Rasm.Fabrication`'s `RectpackSharp`/`Clipper2` true-shape irregular-polygon nesting kernel (AEC peers depend only UPWARD to the `Rasm` kernel). The owner is HOST-NEUTRAL: a `NestPlacement` carries a sheet-index plus scalar `X`/`Y` position and a rotation flag, never a `Rhino.Geometry` transform — the host materializes the cutting plan, and the resolved `NestPlan` crosses to `Rasm.Fabrication` ACROSS THE STRATA WALL (peers never reference each other) as its scalar `CuttingPlan` mirror: an app-stratum mediator above both peers maps each `NestPlacement` tuple onto a `Rasm.Fabrication` `PlannedPlacement` (the field-for-field sheet-index/position/rotation mirror that carries no `MaterialId`), and the `Rasm.Fabrication` `Nesting/nfp#NESTING` `Nest.Honor` fold then consumes that `CuttingPlan` on `FabricationInput.Plan`, mapping each `PlannedPlacement` straight to a `PartTransform` by the rotation flag and lower-left offset, re-packing nothing; the appearance engine shades through the seam `Material`/`Appearance` nodes. The SIX academic packer surfaces (`MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack` plus the heuristic-sweep best-of) and their distinct heuristic enums collapse into ONE `NestStrategy` `[Union]` whose `Switch` the `Pack` fold dispatches — a new algorithm is one `NestStrategy` case, never a parallel packer surface. The page composes `RectangleBinPack.CSharp` as the packing kernel, the seam `Rasm.Element` `MaterialId` for the material a part is cut from, the `Rasm` kernel `PositiveMagnitude` for every dimension, and `Construction/assembly#PLACEMENT_MODEL` `ConstructionFault` band 2350 for the `Nest` fault rail; `Construction/layout#ASSEMBLY_FOLD` is the DISTINCT architectural-coursing owner (where units go along a structural run), this page the distinct cutting-stock owner (how parts are cut from stock with minimum waste), and the embodied-carbon/material-cost rollup the `NestYield.WasteAreaMm2` feeds is the `Rasm.Compute` `AggregateEnvironmental`/`AggregateCost` fold reading the seam `Material` node's `Environmental`/`Cost` cases, never a direct sustainability read.

## [01]-[INDEX]

- [01]-[STOCK_NEST]: the `CutPart` cut-list part, the `StockSheet` stock extent with its trim inset, the `NestStrategy` `[Union]` collapsing the five packers plus the heuristic-sweep best-of, the `NestPlacement` host-neutral placement, the `SheetYield` per-sheet ledger row, the `NestPlan` resolved layout, the `NestYield` material-utilization receipt, and the one `StockNest.Pack` multi-sheet fold.

## [02]-[STOCK_NEST]

- Owner: `NestRun` the parameterized cut job; `NestPlan` the resolved multi-sheet layout; `NestStrategy` `[Union]` the packer-algorithm axis; `SheetYield` the per-sheet ledger row; `NestYield` the typed material-utilization receipt; the multi-sheet `StockNest.Pack` fold over the `RectangleBinPack.CSharp` `Insert` stream.
- Cases: strategy {max-rects (densest general nesting, exposes `UsedRectangles`/`FreeRectangles` remnants), skyline (fast streaming for many uniform parts), guillotine (the PANEL-SAW straight-cut constraint, every cut edge-to-edge, exposes post-merge remnants), shelf (row-band), mass-cut (`SingleBinPack` homogeneous identical-part sheet-yield), best (a heuristic SWEEP — the same packer driven once per heuristic in its vocabulary, the highest-yield run kept)} — the closed `NestStrategy` `[Union]`, each interactive case carrying its OWN packer's correctly-typed heuristic enum (`FreeRectChoiceHeuristic` for max-rects, the DISTINCT nested `GuillotineBinPack.FreeRectChoiceHeuristic` + `GuillotineSplitHeuristic` for guillotine, `LevelChoiceHeuristic` for skyline, `ShelfChoiceHeuristic` for shelf) and the sweep case carrying a `NestStrategy.Family` discriminant naming which packer to exhaust; a new packing algorithm is one `NestStrategy` case, never a parallel packer class.
- Entry: `public static Fin<NestPlan> Pack(NestRun run, Op key)` — the host-neutral cutting-stock fold: validate the cut-list and sheet, ceil every part footprint and the trim-inset sheet extent to the `int` packing domain (the `RectangleBinPack` coordinate type) adding the saw `KerfMm` and the per-part `MarginMm`, sort the parts descending by area (the placement quality is order-sensitive), then `Drive` the strategy's packer `Insert` stream across as many sheets as the parts require — a `Rect.Height == 0` return is the per-sheet infeasibility sentinel that opens a fresh sheet (`Init`) and re-feeds the unplaced part, a part larger than any sheet railing `ConstructionFault.Nest`; the returned `Rect.X`/`Y` offset back into scalar `NestPlacement` millimetres and the `NestYield` derives material-utilization from the per-sheet placed-area sums against the usable sheet area. `Fin<T>` aborts on an empty cut-list (`ConstructionFault.Nest`), a degenerate sheet (`ConstructionFault.Path`), a part exceeding the usable sheet (`ConstructionFault.Nest`), or a heterogeneous cut-list under the `MassCut` strategy (`ConstructionFault.Nest` — `MassCut` is a one-footprint homogeneous yield, so a non-uniform footprint is rejected rather than silently mis-cut as copies of the head).
- Packages: RectangleBinPack.CSharp (the `MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack` packers, the `Rect` value struct with the `Height == 0` sentinel and the `Width`/`Height`/`X`/`Y` int fields, the five heuristic enums; assembly id `RectangleBinPacking`; `.api/api-rectanglebinpack-csharp.md`), Rasm.Element (project — `MaterialId` the cut part is cut from), Rasm (project — `PositiveMagnitude` for the dimensions), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new packing algorithm is one `NestStrategy` `[Union]` case binding its packer + heuristic and one `Drive` switch arm; a new part attribute (grain direction is the `AllowRotation` flag, edge-banding/finish oversize the `MarginMm` column, a defect map a future `CutPart` column) is one `CutPart` column the `Ceil` footprint reads; a new utilization metric is one `NestYield`/`SheetYield` field; a new sheet attribute (trim margin is the `TrimMm` inset, a defect zone a future column) is one `StockSheet` column — never a parallel nesting owner, never a per-algorithm `Pack` method. The int-rect packing domain crosses to scalar `NestPlacement` millimetres at the ONE `Drive` boundary, the `Rect`/heuristic-enum types never escaping into an unrelated signature.
- Law: `Drive` is the page's `[EXPRESSION_SPINE]` kernel exemption — the stateful `RectangleBinPack` packer is a MUTABLE single-sheet state machine (`Insert` mutates its free/used lists), so the multi-sheet placement loop carries the page's only statements (the per-part `Insert` call, the `Height == 0` fresh-sheet open, the per-sheet `SheetYield` close, the cursor advance); the cut-list sort, the part expansion, the `NestPlacement` projection, the `NestYield` fold, and every other surface are expression-bodied, and the placement accumulation crosses to the immutable `Seq` only at the kernel egress, never a mutable list mutated in domain code. The `Best` sweep is itself expression-shaped — a `Fold` over the per-heuristic `DriveStream` results keeping the higher-yield run, each inner run its own confined kernel — so the sweep adds NO statement: the exemption stays the single-sheet driver, the best-of selection a pure fold.
- Boundary: `StockNest.Pack` is the ONE cutting-stock fold — a per-algorithm packer surface is the deleted form; the five `RectangleBinPack` packer classes, their five heuristic enums, and the heuristic SWEEP collapse into the one `NestStrategy` `[Union]` the `Drive` `Switch` dispatches, so a max-rects nest, a guillotine panel-saw plan, a homogeneous mass-cut yield, and a best-of-sweep are the SAME `Pack` fold differing only in the strategy case; the `int` packing domain is the BOUNDARY_ADMISSION edge — part footprints (with the saw `KerfMm` AND the per-part `MarginMm` added) and the trim-inset sheet extent (`WidthMm − 2·TrimMm`, `HeightMm − 2·TrimMm`) ceil to `int` ON THE WAY IN and the returned `Rect.X`/`Y` offset back to scalar `NestPlacement` millimetres (shifted by `TrimMm` so the placement reads in the full-sheet frame) ON THE WAY OUT, the `Rect` value struct never crossing into an interior signature; the `Rect.Height == 0` sentinel is the ONLY infeasibility signal the suite emits (it throws no typed packing exception), so `Drive` maps the zero-height return to the typed multi-sheet open or the `ConstructionFault.Nest` rail rather than catching, and `Fin<NestPlan>` carries the outcome; rotation is read from the packer return ROBUSTLY — a `Rect` whose placed-width equals the rotated footprint (and differs from the un-rotated one) is a 90° flip, the square-part ambiguity resolved by `w == h ? false` so a square never reports a meaningless rotation; `max-rects` and `guillotine` alone surface their `FreeRectangles` remnant geometry, so the per-sheet `SheetYield.LargestRemnantMm2` reads the largest single reusable offcut on THAT sheet from `MaxRectsBinPack.FreeRectangles` / `GuillotineBinPack` post-`MergeFreeRectangles` (distinct from `WasteAreaMm2`, the total scrap), the `skyline`/`shelf`/`mass-cut` strategies whose free lists stay private reporting a 0 remnant while the utilization ratio derives from the placed-area sum either way; the resolved `NestPlan` is portable scalar data (a `Seq<NestPlacement>` of sheet-index + position + rotation tuples + the per-sheet `SheetYield` ledger) the host materializes into a cutting plan, and an app-stratum mediator maps it onto the `Rasm.Fabrication` `CuttingPlan`/`PlannedPlacement` mirror the `Rasm.Fabrication` `Nest.Honor` fold consumes across the [WIRE] seam (mapping each `PlannedPlacement` to a `PartTransform`, the rectangular offcuts riding the `NestYield` receipt the mediator carries, never crossing as a Materials reference), and the embodied-carbon/material-cost rollup reads the `NestYield.WasteAreaMm2` through the seam `Material` node — never a `Rhino.Geometry` type and never a reach into `Rasm.Fabrication`'s true-shape nesting kernel (the strata wall forbids the AEC peer crossing — this in-folder rectangle suite IS the placement owner precisely so no sideways dependency is minted); a `CutPart` references the seam `MaterialId` it is cut from so the cutting plan ties to the material catalogue, the per-sheet yield feeding the procurement and embodied-carbon seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Thinktecture;                  // [Union] (NestStrategy), [SmartEnum<string>] (NestFamily)
using Rasm.Domain;                   // PositiveMagnitude (the kernel dimensional value-object)
using Rasm.Element;                  // MaterialId (the seam-carried material identity a cut part is cut from)
using RectangleBinPacking;           // MaxRectsBinPack/SkylineBinPack/GuillotineBinPack/ShelfBinPack/SingleBinPack, Rect, the heuristic enums
using Op = Rasm.Domain.Op;           // the kernel op-key (the sibling assembly#PLACEMENT_MODEL alias convention)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Construction;

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
// A rectangular cut-list part keyed to the seam material it is cut from. AllowRotation IS the grain-lock axis (a
// grain-locked part forbids the 90° flip); MarginMm is the edge-banding / finish-oversize the footprint reserves
// alongside the saw kerf (a veneered panel cut oversize then trimmed), 0 for a raw cut.
public readonly record struct CutPart(MaterialId Material, PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, int Quantity, bool AllowRotation = true, double MarginMm = 0.0) {
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

// The host-neutral placed cut — a sheet index plus a scalar position and a rotation flag, never a Rhino transform. The
// Rasm.Fabrication Nest.Honor fold maps this straight to a PartTransform (rotate by the flag, offset to the lower-left).
public readonly record struct NestPlacement(MaterialId Material, int SheetIndex, double XMm, double YMm, double WidthMm, double HeightMm, bool Rotated) {
    // The cut perimeter the cut-length receipt sums — the saw travel to free this part from the stock.
    public double PerimeterMm => 2.0 * (WidthMm + HeightMm);
}

// The per-sheet ledger row — the cutting-stock leftover ledger is PER SHEET, not last-sheet-only: a cut-shop schedules
// each sheet, re-stocks each sheet's largest offcut, and the procurement rollup reads each sheet's waste. Utilization is
// the placed area against THIS sheet's usable area; LargestRemnantMm2 the re-stockable offcut the FreeRectangles-surfacing
// packers report on this sheet (0 for the others); CutLengthMm the saw travel on this sheet.
public readonly record struct SheetYield(int SheetIndex, int PlacedCount, double UtilizationRatio, double LargestRemnantMm2, double CutLengthMm);

// The typed material-utilization receipt — algorithm evidence over the per-sheet ledger, never a generic IReceipt. The
// aggregate columns (UtilizationRatio/PlacedCount/UnplacedCount/SheetCount/WasteAreaMm2) are the wire shape the
// Rasm.Fabrication CuttingPlan mirror reads; RemnantAreaMm2 is the total re-stockable offcut summed across sheets, and
// CutLengthMm the total saw travel; Sheets is the per-sheet detail a re-stocking inventory and a cut-shop schedule read.
public readonly record struct NestYield(double UtilizationRatio, int PlacedCount, int UnplacedCount, int SheetCount, double RemnantAreaMm2, double WasteAreaMm2, double CutLengthMm, Seq<SheetYield> Sheets);

public sealed record NestRun(Seq<CutPart> Parts, StockSheet Sheet, NestStrategy Strategy) {
    public static NestRun Of(Seq<CutPart> parts, StockSheet sheet) => new(parts, sheet, NestStrategy.Density);
}

public sealed record NestPlan(NestRun Run, Seq<NestPlacement> Placements, NestYield Yield);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StockNest {
    private const double ResolutionMm = 1.0;   // ceil the fractional-mm footprints to the int packing domain at 1 mm

    public static Fin<NestPlan> Pack(NestRun run, Op key) =>
        from nonEmpty in guard(!run.Parts.IsEmpty, ConstructionFault.Nest(key, "<empty-cut-list>"))
        from sheetOk in guard(run.Sheet.WidthMm.Value > 0.0 && run.Sheet.HeightMm.Value > 0.0, ConstructionFault.Path(key, "<degenerate-stock-sheet>"))
        from trimOk in guard(run.Sheet.UsableWidthMm > 0.0 && run.Sheet.UsableHeightMm > 0.0, ConstructionFault.Path(key, "<trim-margin-exceeds-stock-sheet>"))
        let sheetW = (int)Math.Floor(run.Sheet.UsableWidthMm / ResolutionMm)
        let sheetH = (int)Math.Floor(run.Sheet.UsableHeightMm / ResolutionMm)
        let parts = Expand(run.Parts).OrderByDescending(static p => p.AreaMm2).ToSeq()   // order-sensitive: descending area
        // Rotation is a per-run policy the strategy carries; a single grain-locked CutPart disables rotation for the run
        // (AllowRotation is load-bearing), and the fit gate admits a part that only fits rotated when the run rotates.
        let allowRotation = StrategyRotates(run.Strategy) && parts.ForAll(static p => p.AllowRotation)
        from fits in guard(parts.ForAll(p => FitsSheet(p, run.Sheet.KerfMm, sheetW, sheetH, allowRotation)),
            ConstructionFault.Nest(key, "<part-exceeds-stock-sheet>"))
        // MassCut drives SingleBinPack over ONE footprint × quantity (the head), so a heterogeneous cut-list under the
        // MassCut strategy would silently mis-cut the tail as copies of the head. The strategy NAMES a homogeneous yield
        // query, so a non-uniform footprint is a malformed job railed HERE rather than a silent drop — the interactive
        // packers (max-rects/skyline/guillotine/shelf/best) place each distinct part and impose no homogeneity. The
        // MassCut test is a case pattern (one hop, no parallel total Switch a new strategy would have to grow an arm in).
        from uniform in guard(run.Strategy is not NestStrategy.MassCut || Homogeneous(parts),
            ConstructionFault.Nest(key, "<mass-cut-heterogeneous-cut-list>"))
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

    // Each CutPart of Quantity n expands to n unit parts the packer places one at a time.
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
            if (placed.Height == 0) { continue; }   // unfittable on a fresh sheet → counted unplaced by the Drive return
            // Robust rotation: a 90° flip iff the placed width equals the rotated footprint AND differs from the
            // un-rotated one; a square footprint (w == h) reports no rotation rather than a meaningless flip.
            bool rotated = rotate && w != h && placed.Width == h;
            placements.Add((new NestPlacement(part.Material, sheetIndex, placed.X * ResolutionMm + sheet.TrimMm, placed.Y * ResolutionMm + sheet.TrimMm, placed.Width * ResolutionMm, placed.Height * ResolutionMm, rotated), 0.0));
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
    // ConstructionFault.Nest), so driving the head × parts.Count is exact, never a silent tail mis-cut. When the run
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
                    new NestPlacement(part.Material, 0, r.X * ResolutionMm + sheet.TrimMm, r.Y * ResolutionMm + sheet.TrimMm, r.Width * ResolutionMm, r.Height * ResolutionMm, Rotated: useRotated),
                    0.0));
            },
            None: () => Seq<(NestPlacement, double)>());
}
```

## [03]-[RESEARCH]

- [PACKER_COLLAPSE]: REALIZED — the five `RectangleBinPack.CSharp` stateful packer classes (`MaxRectsBinPack` densest-general, `SkylineBinPack` fast-streaming, `GuillotineBinPack` panel-saw straight-cut, `ShelfBinPack` row-band, `SingleBinPack` homogeneous mass-cut) and their five DISTINCT heuristic enums collapse into ONE `NestStrategy` `[Union]` whose `Switch` the `Drive` kernel dispatches, each interactive case carrying its own packer's correctly-typed heuristic so the two non-interchangeable `FreeRectChoiceHeuristic` enums (the top-level one `MaxRects` reads and the nested `GuillotineBinPack.FreeRectChoiceHeuristic`) never cross. A nesting job picks its algorithm by intent — `NestStrategy.Density` (max-rects best-short-side-fit) for general nest density, `NestStrategy.PanelSaw` (guillotine minimize-area split) for saw-cut sheet goods where every cut must span edge-to-edge, `MassCut` for an identical-part yield query, `NestStrategy.Optimal` for the heuristic sweep — and the one `Pack` fold runs all six; a new packing algorithm is one `NestStrategy` case plus one `Drive` arm, never a parallel packer surface.
- [HEURISTIC_SWEEP]: REALIZED — `NestStrategy.Best(NestFamily, AllowRotation)` is the cutting-stock best-of the suite ships no built-in driver for: each packer exposes a vocabulary of placement heuristics (the `MaxRectsBinPack.Insert` five `FreeRectChoiceHeuristic` values, the `GuillotineBinPack` six `FreeRectChoiceHeuristic` × the split heuristics, the `ShelfBinPack` seven `ShelfChoiceHeuristic` values per the `.api` catalogue), and a single fixed heuristic leaves yield on the table when the part mix is unknown — so `BestOf` drives the named `NestFamily` packer ONCE per heuristic in its enumerated vocabulary (`Enum.GetValues<T>()`) and keeps the run placing the most total area (`Best`'s `Fold` over the candidate runs' summed placed area, since LanguageExt `Seq` exposes no `MaxBy`). The sweep is a pure fold over confined `DriveStream` kernels, so the `[EXPRESSION_SPINE]` exemption stays the single-sheet driver and the best-of selection adds no statement; the `NestFamily` `[SmartEnum<string>]` names which packer to exhaust, the `Switch` routing each family to the SAME `DriveStream` the interactive cases use, never a second packer surface — a new sweepable family is one `NestFamily` row plus one `BestOf` arm.
- [STATEFUL_KERNEL_EXEMPTION]: REALIZED — the `RectangleBinPack` packers are mutable single-sheet state machines (`Insert` mutates the packer's free/used lists, no method throws on infeasibility — a `Rect.Height == 0` is the universal placement-failure sentinel), so `DriveStream`/`MassCut` are the page's named `[EXPRESSION_SPINE]` kernel exemptions the way `Construction/layout#ASSEMBLY_FOLD` names its `StepCursor`/`Voussoirs` enumerators: the multi-sheet placement loop and the local mutable `List<(NestPlacement, double)>` accumulator are confined to the kernel, and the surrounding `Pack` fold, the cut-list sort, the part expansion, the `Best` sweep, and the `NestYield`/`SheetYield` projections are expression-bodied immutable `Seq` folds. The multi-sheet driver opens a fresh packer (`Init`/new) on a `Height == 0` failure and re-feeds the unplaced part — the suite ships no multi-bin driver, so the one-sheet-overflow → next-sheet loop is this owner's, and a part that fails on a fresh empty sheet is counted unplaced by the `Drive` return rather than throwing. The rotation flag is read ROBUSTLY from the packer return (`w != h && placed.Width == h`), curing the prior `placed.Width != w` square-part ambiguity where a square footprint reported a meaningless rotation the `Rasm.Fabrication` `Nest.Honor` 90°-flag mapping would have honored.
- [INT_DOMAIN_ADMISSION]: REALIZED — `RectangleBinPack` is `int`-domain (contrast `RectpackSharp`'s `uint`), so the BOUNDARY_ADMISSION edge ceils every fractional-millimetre `CutPart` footprint and the trim-inset `StockSheet` extent to `int` at the one `Ceil`/`Drive` boundary (adding the saw `KerfMm` AND the per-part `MarginMm` so both the blade width and the finish oversize are reserved), drives the packer in the int domain, and offsets the returned `Rect.X`/`Y` back to scalar `NestPlacement` millimetres SHIFTED by `TrimMm` so the placement reads in the full-sheet frame — the `Rect` value struct never crossing into an interior signature, the int/scalar conversion confined to the one fold. The `ResolutionMm` constant fixes the 1 mm packing granularity; a finer cut tolerance scales the constant without touching the fold. The `StockSheet.TrimMm` edge inset (the gripper/clamp margin or mill-edge defect band) shrinks the packable extent to `(WidthMm − 2·TrimMm, HeightMm − 2·TrimMm)` so a flatbed router's unusable border is reserved, and the per-sheet `Pack` trim guard rails `ConstructionFault.Path` when the trim exceeds the sheet.
- [MATERIAL_UTILIZATION_RECEIPT]: REALIZED — the typed `NestYield` receipt carries the algorithm evidence over the per-sheet `SheetYield` ledger rather than a generic `IReceipt` or a bare ratio, since the suite ships no built-in occupancy metric — the consumer derives utilization from the summed `Rect.Area`. The cutting-stock leftover ledger is PER SHEET, not last-sheet-only: a cut-shop schedules each sheet, re-stocks each sheet's largest offcut, and the procurement rollup reads each sheet's waste, so `SheetYield(SheetIndex, PlacedCount, UtilizationRatio, LargestRemnantMm2, CutLengthMm)` is a per-sheet row the `DriveStream` stamps its closing-sheet remnant onto and `SheetLedger` re-folds. `MaxRectsBinPack` and `GuillotineBinPack` alone surface their `FreeRectangles` remnant geometry (the `.api`-catalogued contract — `SkylineBinPack`/`ShelfBinPack`/`SingleBinPack` keep their free lists private), so the per-sheet `LargestRemnantMm2` reads the largest reusable offcut on THAT sheet from `MaxRectsBinPack.FreeRectangles` / `GuillotineBinPack` post-`MergeFreeRectangles` (the panel-saw defragmentation that recovers contiguous remnant area) and the `skyline`/`shelf`/`mass-cut` arms report a 0 remnant; the aggregate `NestYield.RemnantAreaMm2` sums every sheet's offcut (the whole re-stockable inventory), distinct from `WasteAreaMm2` (the total `usableArea − usedArea` scrap), and `CutLengthMm` sums the per-placement perimeters (the saw travel — the cutting-stock time/cost driver). The `NestYield.WasteAreaMm2` is the source the embodied-carbon/material-cost rollup reads through the seam `Material` node — the `Rasm.Compute` `AggregateEnvironmental`/`AggregateCost` fold reading the material's `Environmental`/`Cost` cases, not a direct `Properties/sustainability` read — the cutting plan crossing to `Rasm.Fabrication`'s process owner as portable scalar data (the [WIRE] seam — Materials produces the plan, Fabrication's `Nest.Honor` consumes it, the acyclic strata forbidding the AEC peer a `Fabrication` reference).
- [PART_AND_SHEET_GROWTH]: REALIZED — the prose's promised part/sheet attributes are now load-bearing columns, not unbacked claims: `CutPart.AllowRotation` IS the grain-lock axis (a grain-locked panel forbids the 90° flip, and the run rotates only when every part admits it), `CutPart.MarginMm` the edge-banding/finish oversize the `Ceil` footprint reserves alongside the saw kerf (a veneered panel cut oversize then trimmed to its finished dimension), and `StockSheet.TrimMm` the usable-area edge inset every sheet shrinks by. A defect map is a future `CutPart` column the fit gate reads, a defect zone a future `StockSheet` column the packable region subtracts — each one column on the existing record, never a parallel nesting owner. The `MassCut` arm honors the same rotation policy: when the run rotates and the rotated footprint yields more identical parts on one sheet, it drives the rotated orientation and flags every placement, so a homogeneous yield query is not silently denied the 90° flip the interactive `MaxRects` path takes. `MassCut` drives `SingleBinPack` over the head footprint × `parts.Count`, so a heterogeneous cut-list would silently mis-cut the tail as copies of the head — `Pack`'s `Homogeneous` guard rails `ConstructionFault.Nest` on a non-uniform `MassCut` footprint (the `run.Strategy is not NestStrategy.MassCut` case pattern scopes the guard to `MassCut` alone — one hop, never a parallel total `Switch` a new strategy would have to grow an arm in; the interactive packers and the sweep place each distinct part and impose no homogeneity), so the prose's "homogeneous identical-part" claim is now a guarded invariant rather than an unchecked assumption.
- [STRATA_DISJOINT_NESTING]: the `RectangleBinPack.CSharp` AABB rectangle cutting-stock suite and `Rasm.Fabrication`'s `RectpackSharp`/`Clipper2` true-shape irregular-polygon nesting kernel own DISJOINT geometry across the strata wall: this owner is the academic MaxRects/Skyline/Guillotine/Shelf family plus the homogeneous mass-cut and the heuristic sweep, AABB-only (irregular-polygon feasibility has no owner here); Fabrication owns the true-shape arm AND the from-scratch degenerate-AABB CAM nest for parts arriving without a material plan. Neither type crosses the strata wall — `Rasm.Materials` mints its OWN in-folder rectangle packing pin precisely so no sideways `Materials → Fabrication` dependency is created, the AEC peers depending only UPWARD to the `Rasm` kernel. The seam to Fabrication is the [WIRE], mediated never direct (the acyclic strata forbids a `Materials → Fabrication` peer reference): an app-stratum mediator above both peers maps the resolved `NestPlan` onto the `Rasm.Fabrication` `CuttingPlan` (a `Seq<PlannedPlacement>` field-for-field mirror of the `NestPlacement` tuples), and `Rasm.Fabrication`'s `Nesting/nfp#NESTING` `Nest.Honor` fold admits that `CuttingPlan` on `FabricationInput.Plan` and maps each `PlannedPlacement` straight to a `PartTransform` (rotating by the `PlannedPlacement` rotation flag, offsetting to the placed lower-left), Materials owning the rectangular cutting-stock YIELD (the procurement/sustainability concern — minimum sheets, per-sheet offcuts, embodied-carbon waste) and Fabrication the true-shape irregular NEST (the CAM concern — cut-program transforms), neither re-packing the other; the `NestPlacement` sheet-index + position + rotation tuple is the wire-stable shape the `CuttingPlan`/`PlannedPlacement` mirror reflects, the `SheetYield`/`CutLengthMm` enrichment additive on the Materials side and never altering the tuple the mirror reflects.
