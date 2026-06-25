# [MATERIALS_NESTING]

THE CUTTING-STOCK / SHEET-YIELD OWNER and THE ONE NESTING FOLD. One `NestRun` is a parameterized cut job — a `Seq<CutPart>` rectangular cut-list, a `StockSheet` stock extent, and a `NestStrategy` — and one `NestPlan` is the resolved sheet layout the run folds to through the single `StockNest.Pack` fold over the `RectangleBinPack.CSharp` academic 2D bin-packing suite, with a typed `NestYield` material-utilization receipt. This is the AABB-rectangle cutting-stock engine: given a cut-list and stock-sheet dimensions, place every part to maximize material yield, drive across as many sheets as the parts require, and report the utilization/waste evidence — the strata-acyclic placement pin an AEC-DOMAIN peer requires, since `Rasm.Materials` cannot reach sideways into `Rasm.Fabrication`'s `RectpackSharp`/`Clipper2` true-shape irregular-polygon nesting kernel (AEC peers depend only UPWARD to the `Rasm` kernel). The owner is HOST-NEUTRAL: a `NestPlacement` carries a sheet-index plus scalar `X`/`Y` position and a rotation flag, never a `Rhino.Geometry` transform — the host materializes the cutting plan and the appearance engine shades through `Appearance/graph#MATERIAL_LIBRARY`. The FIVE academic packer classes (`MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack`) and their five distinct heuristic enums collapse into ONE `NestStrategy` `[Union]` whose `Switch` the `Pack` fold dispatches — a new algorithm is one `NestStrategy` case, never a parallel packer surface. The page composes `RectangleBinPack.CSharp` as the packing kernel, `profile#PROFILE_OWNER` `Profile`/`MaterialId` for the material a part is cut from, the `Rasm` kernel `PositiveMagnitude` for every dimension, and `Construction/assembly#ELEMENT_MODEL` `ConstructionFault` band 2350 for the `Nest` fault rail; `Construction/layout#ASSEMBLY_FOLD` is the DISTINCT architectural-coursing owner (where units go along a structural run), this page the distinct cutting-stock owner (how parts are cut from stock with minimum waste).

## [01]-[INDEX]

- [01]-[STOCK_NEST]: the `CutPart` cut-list part, the `StockSheet` stock extent, the `NestStrategy` `[Union]` collapsing the five packers, the `NestPlacement` host-neutral placement, the `NestPlan` resolved layout, the `NestYield` material-utilization receipt, and the one `StockNest.Pack` multi-sheet fold.

## [02]-[STOCK_NEST]

- Owner: `NestRun` the parameterized cut job; `NestPlan` the resolved sheet layout; `NestStrategy` `[Union]` the five-packer algorithm axis; `NestYield` the typed material-utilization receipt; the multi-sheet `StockNest.Pack` fold over the `RectangleBinPack.CSharp` `Insert` stream.
- Cases: strategy {max-rects (densest general nesting, exposes `UsedRectangles`/`FreeRectangles` remnants), skyline (fast streaming for many uniform parts), guillotine (the PANEL-SAW straight-cut constraint, every cut edge-to-edge, exposes post-merge remnants), shelf (row-band), mass-cut (`SingleBinPack` homogeneous identical-part sheet-yield)} — the closed `NestStrategy` `[Union]`, each case carrying its OWN packer's correctly-typed heuristic enum (`FreeRectChoiceHeuristic` for max-rects, the DISTINCT nested `GuillotineBinPack.FreeRectChoiceHeuristic` + `GuillotineSplitHeuristic` for guillotine, `LevelChoiceHeuristic` for skyline, `ShelfChoiceHeuristic` for shelf); a new packing algorithm is one `NestStrategy` case, never a parallel packer class.
- Entry: `public static Fin<NestPlan> Pack(NestRun run, Op key)` — the host-neutral cutting-stock fold: validate the cut-list and sheet, ceil every part footprint and the sheet extent to the `int` packing domain (the `RectangleBinPack` coordinate type) adding the saw `KerfMm`, sort the parts descending by area (the placement quality is order-sensitive), then `Drive` the strategy's packer `Insert` stream across as many sheets as the parts require — a `Rect.Height == 0` return is the per-sheet infeasibility sentinel that opens a fresh sheet (`Init`) and re-feeds the unplaced part, a part larger than any sheet railing `ConstructionFault.Nest`; the returned `Rect.X`/`Y` offset back into scalar `NestPlacement` millimetres and the `NestYield` derives material-utilization from the summed `Rect.Area` against the sheet area. `Fin<T>` aborts on an empty cut-list (`ConstructionFault.Nest`), a degenerate sheet (`ConstructionFault.Path`), or a part exceeding the sheet (`ConstructionFault.Nest`).
- Packages: RectangleBinPack.CSharp (the `MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack` packers, the `Rect` value struct with the `Height == 0` sentinel, the five heuristic enums; assembly id `RectangleBinPacking`; `.api/api-rectanglebinpack-csharp.md`), Rasm (project — `PositiveMagnitude` for the dimensions), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new packing algorithm is one `NestStrategy` `[Union]` case binding its packer + heuristic and one `Drive` switch arm; a new part attribute (grain direction, edge-banding margin, defect map) is one `CutPart` column; a new utilization metric is one `NestYield` field; a new sheet attribute (trim margin, defect zone) is one `StockSheet` column — never a parallel nesting owner, never a per-algorithm `Pack` method. The int-rect packing domain crosses to scalar `NestPlacement` millimetres at the ONE `Drive` boundary, the `Rect`/heuristic-enum types never escaping into an unrelated signature.
- Law: `Drive` is the page's `[EXPRESSION_SPINE]` kernel exemption — the stateful `RectangleBinPack` packer is a MUTABLE single-sheet state machine (`Insert` mutates its free/used lists), so the multi-sheet placement loop carries the page's only statements (the per-part `Insert` call, the `Height == 0` fresh-sheet open, the cursor advance), exactly the measured imperative-kernel exemption `Construction/layout#ASSEMBLY_FOLD` `StepCursor`/`Voussoirs` name; the cut-list sort, the `NestPlacement` projection, the `NestYield` fold, and every other surface are expression-bodied, and the placement accumulation is the immutable `Seq` `Fold`, never a mutable list mutated in domain code.
- Boundary: `StockNest.Pack` is the ONE cutting-stock fold — a per-algorithm packer surface is the deleted form; the five `RectangleBinPack` packer classes and their five heuristic enums collapse into the one `NestStrategy` `[Union]` the `Drive` `Switch` dispatches, so a max-rects nest, a guillotine panel-saw plan, and a homogeneous mass-cut yield are the SAME `Pack` fold differing only in the strategy case; the `int` packing domain is the BOUNDARY_ADMISSION edge — part footprints and the sheet extent ceil to `int` (the `RectangleBinPack` coordinate type, with the saw `KerfMm` added to each part) ON THE WAY IN and the returned `Rect.X`/`Y` offset back to scalar `NestPlacement` millimetres ON THE WAY OUT, the `Rect` value struct never crossing into an interior signature; the `Rect.Height == 0` sentinel is the ONLY infeasibility signal the suite emits (it throws no typed packing exception), so `Drive` maps the zero-height return to the typed multi-sheet open or the `ConstructionFault.Nest` rail rather than catching, and `Fin<NestPlan>` carries the outcome; `max-rects` and `guillotine` alone surface their `FreeRectangles` remnant geometry, so the `NestYield.RemnantAreaMm2` reads the largest single reusable offcut on the final sheet from `MaxRectsBinPack.FreeRectangles` / `GuillotineBinPack` post-`MergeFreeRectangles` (distinct from `WasteAreaMm2`, the total scrap), the `skyline`/`shelf`/`mass-cut` strategies whose free lists stay private reporting a 0 remnant while the utilization ratio derives from the placed-area sum either way; the resolved `NestPlan` is portable scalar data (a `Seq<NestPlacement>` of sheet-index + position + rotation tuples) the host materializes into a cutting plan and the `Properties/sustainability#SUSTAINABILITY` offcut-waste rollup reads, never a `Rhino.Geometry` type and never a reach into `Rasm.Fabrication`'s true-shape nesting kernel (the strata wall forbids the AEC peer crossing — this in-folder rectangle suite IS the placement owner precisely so no sideways dependency is minted); a `CutPart` references the `profile#PROFILE_OWNER` `MaterialId` it is cut from so the cutting plan ties to the material catalogue, the yield feeding the embodied-carbon/material-cost seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using RectangleBinPacking;   // MaxRectsBinPack/SkylineBinPack/GuillotineBinPack/ShelfBinPack/SingleBinPack, Rect, the heuristic enums

// --- [TYPES] -------------------------------------------------------------------------------
// One NestStrategy [Union] collapses the five RectangleBinPack packer classes, each case carrying its OWN packer's
// correctly-typed heuristic enum — never magic ordinals, never five parallel packer surfaces. A new algorithm is a case.
[Union]
public abstract partial record NestStrategy {
    private NestStrategy() { }
    public sealed record MaxRects(FreeRectChoiceHeuristic Choice, bool AllowRotation) : NestStrategy;
    public sealed record Skyline(SkylineBinPack.LevelChoiceHeuristic Level, bool UseWasteMap) : NestStrategy;
    public sealed record Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic Choice, GuillotineBinPack.GuillotineSplitHeuristic Split, bool Merge) : NestStrategy;
    public sealed record Shelf(ShelfBinPack.ShelfChoiceHeuristic Choice, bool UseWasteMap) : NestStrategy;
    public sealed record MassCut : NestStrategy;

    public static readonly NestStrategy Density = new MaxRects(FreeRectChoiceHeuristic.RectBestShortSideFit, AllowRotation: true);
    public static readonly NestStrategy PanelSaw = new Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic.RectBestAreaFit, GuillotineBinPack.GuillotineSplitHeuristic.SplitMinimizeArea, Merge: true);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct CutPart(MaterialId Material, PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, int Quantity, bool AllowRotation = true) {
    public double AreaMm2 => WidthMm.Value * HeightMm.Value;
}

public readonly record struct StockSheet(MaterialId Material, PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, double KerfMm) {
    public double AreaMm2 => WidthMm.Value * HeightMm.Value;
}

// The host-neutral placed cut — a sheet index plus a scalar position and a rotation flag, never a Rhino transform.
public readonly record struct NestPlacement(MaterialId Material, int SheetIndex, double XMm, double YMm, double WidthMm, double HeightMm, bool Rotated);

// The typed material-utilization receipt — algorithm evidence (yield/waste/remnant), never a generic IReceipt.
public readonly record struct NestYield(double UtilizationRatio, int PlacedCount, int UnplacedCount, int SheetCount, double RemnantAreaMm2, double WasteAreaMm2);

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
        let sheetW = (int)Math.Floor(run.Sheet.WidthMm.Value / ResolutionMm)
        let sheetH = (int)Math.Floor(run.Sheet.HeightMm.Value / ResolutionMm)
        let parts = Expand(run.Parts).OrderByDescending(p => p.AreaMm2).ToSeq()   // order-sensitive: descending area
        // Only MaxRects rotates (a global per-packer flag, never per-Insert), so a single grain-locked CutPart disables
        // rotation for the run — AllowRotation is load-bearing, and the fit gate admits a part that only fits rotated.
        let allowRotation = run.Strategy is NestStrategy.MaxRects mr && mr.AllowRotation && parts.ForAll(static p => p.AllowRotation)
        from fits in guard(parts.ForAll(p => FitsSheet(p, run.Sheet.KerfMm, sheetW, sheetH, allowRotation)),
            ConstructionFault.Nest(key, "<part-exceeds-stock-sheet>"))
        let nest = Drive(run.Strategy, parts, run.Sheet, sheetW, sheetH, allowRotation)
        select new NestPlan(run, nest.Placements, YieldOf(nest.Placements, parts.Count, run.Sheet, nest.LargestRemnantMm2));

    static int Ceil(double mm, double kerfMm) => (int)Math.Ceiling((mm + kerfMm) / ResolutionMm);

    // A part fits in its given orientation, or — when the run admits rotation AND the part is not grain-locked —
    // rotated 90°; the int packing domain reserves the saw kerf on each footprint before the comparison.
    static bool FitsSheet(CutPart p, double kerfMm, int sheetW, int sheetH, bool allowRotation) {
        int w = Ceil(p.WidthMm.Value, kerfMm), h = Ceil(p.HeightMm.Value, kerfMm);
        return (w <= sheetW && h <= sheetH) || (allowRotation && p.AllowRotation && h <= sheetW && w <= sheetH);
    }

    // Each CutPart of Quantity n expands to n unit parts the packer places one at a time.
    static Seq<CutPart> Expand(Seq<CutPart> parts) =>
        parts.Bind(p => toSeq(Enumerable.Repeat(p with { Quantity = 1 }, Math.Max(1, p.Quantity))));

    // The largest reusable offcut on the final (partial) sheet, read from a packer's surfaced free-rect list and
    // scaled out of the int packing domain — the cutting-stock leftover-ledger value, distinct from total scrap.
    static double LargestRemnantMm2(IReadOnlyList<Rect> free) =>
        free.Count == 0 ? 0.0 : free.Max(static r => (double)r.Area) * ResolutionMm * ResolutionMm;

    // RemnantAreaMm2 is the largest single reusable offcut (re-stockable leftover) the FreeRectangles-surfacing
    // packers report; WasteAreaMm2 is the total scrap (sheet area minus placed area). The two are distinct
    // cutting-stock metrics — a packer that surfaces no free-rect list reports a 0 remnant, never the waste value.
    static NestYield YieldOf(Seq<NestPlacement> placed, int total, StockSheet sheet, double largestRemnantMm2) {
        int sheets = placed.IsEmpty ? 0 : placed.Map(p => p.SheetIndex).Max() + 1;
        double usedArea = placed.Sum(p => p.WidthMm * p.HeightMm);
        double sheetArea = sheet.AreaMm2 * Math.Max(1, sheets);
        return new NestYield(
            UtilizationRatio: sheetArea > 0.0 ? Math.Clamp(usedArea / sheetArea, 0.0, 1.0) : 0.0,
            PlacedCount: placed.Count,
            UnplacedCount: Math.Max(0, total - placed.Count),
            SheetCount: sheets,
            RemnantAreaMm2: largestRemnantMm2,
            WasteAreaMm2: Math.Max(0.0, sheetArea - usedArea));
    }

    // [PACKER_DRIVE] — the [EXPRESSION_SPINE] exemption: the RectangleBinPack packers are MUTABLE single-sheet state
    // machines, so the multi-sheet Insert loop carries the page's only statements. The Switch dispatches the strategy
    // case onto its packer + heuristic + free-rect reader; a Rect.Height == 0 opens a fresh sheet and re-feeds the part.
    // Only MaxRects/Guillotine surface FreeRectangles, so the other arms pass a 0-remnant reader (the api-catalogued
    // contract — Skyline/Shelf/SingleBin keep their free lists private), guillotine merging before the remnant read.
    static (Seq<NestPlacement> Placements, double LargestRemnantMm2) Drive(NestStrategy strategy, Seq<CutPart> parts, StockSheet sheet, int sheetW, int sheetH, bool allowRotation) =>
        strategy.Switch(
            maxRects:   m => DriveStream(parts, sheet, allowRotation, () => new MaxRectsBinPack(sheetW, sheetH, allowRotation), (pk, w, h) => pk.Insert(w, h, m.Choice), static pk => LargestRemnantMm2(pk.FreeRectangles)),
            skyline:    s => DriveStream(parts, sheet, false, () => new SkylineBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Level), static _ => 0.0),
            guillotine: g => DriveStream(parts, sheet, false, () => new GuillotineBinPack(sheetW, sheetH), (pk, w, h) => pk.Insert(w, h, g.Merge, g.Choice, g.Split), static pk => { pk.MergeFreeRectangles(); return LargestRemnantMm2(pk.FreeRectangles); }),
            shelf:      s => DriveStream(parts, sheet, false, () => new ShelfBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Choice), static _ => 0.0),
            massCut:    _ => (MassCut(parts, sheet, sheetW, sheetH), 0.0));

    // One generic loop over any RectangleBinPack packer (type inferred from `open`) — no boxing: the stateful single-sheet
    // packer is opened, fed one part at a time, and re-opened on a Rect.Height == 0 overflow for the next sheet. After
    // the stream the final (partial) packer's free-rect reader yields the largest reusable offcut.
    static (Seq<NestPlacement> Placements, double LargestRemnantMm2) DriveStream<TPacker>(Seq<CutPart> parts, StockSheet sheet, bool rotate, Func<TPacker> open, Func<TPacker, int, int, Rect> insert, Func<TPacker, double> remnant) {
        var placements = new List<NestPlacement>();   // local mutable accumulator inside the named kernel exemption
        TPacker packer = open();
        int sheetIndex = 0;
        foreach (CutPart part in parts) {
            int w = Ceil(part.WidthMm.Value, sheet.KerfMm), h = Ceil(part.HeightMm.Value, sheet.KerfMm);
            Rect placed = insert(packer, w, h);
            if (placed.Height == 0) { packer = open(); sheetIndex++; placed = insert(packer, w, h); }
            if (placed.Height == 0) { continue; }   // unfittable on a fresh sheet → counted unplaced by YieldOf
            bool rotated = rotate && placed.Width != w;
            placements.Add(new NestPlacement(part.Material, sheetIndex, placed.X * ResolutionMm, placed.Y * ResolutionMm, placed.Width * ResolutionMm, placed.Height * ResolutionMm, rotated));
        }
        return (placements.ToSeq(), remnant(packer));
    }

    // The homogeneous mass-cut yield: SingleBinPack.Insert(w, h, quantity) returns the List<Rect> of as-many-as-fit
    // identical parts on one sheet — the sheet-yield count for a homogeneous cut list, the positions not just the count.
    // SingleBinPack surfaces no free-rect list, so the mass-cut arm reports a 0 remnant.
    static Seq<NestPlacement> MassCut(Seq<CutPart> parts, StockSheet sheet, int sheetW, int sheetH) =>
        parts.HeadOrNone().Match(
            Some: part => toSeq(new SingleBinPack(sheetW, sheetH).Insert(Ceil(part.WidthMm.Value, sheet.KerfMm), Ceil(part.HeightMm.Value, sheet.KerfMm), Math.Max(1, parts.Count)))
                .Map(r => new NestPlacement(part.Material, 0, r.X * ResolutionMm, r.Y * ResolutionMm, r.Width * ResolutionMm, r.Height * ResolutionMm, Rotated: false)),
            None: () => Seq<NestPlacement>());
}
```

## [03]-[RESEARCH]

- [PACKER_COLLAPSE]: REALIZED — the five `RectangleBinPack.CSharp` stateful packer classes (`MaxRectsBinPack` densest-general, `SkylineBinPack` fast-streaming, `GuillotineBinPack` panel-saw straight-cut, `ShelfBinPack` row-band, `SingleBinPack` homogeneous mass-cut) and their five DISTINCT heuristic enums collapse into ONE `NestStrategy` `[Union]` whose `Switch` the `Drive` kernel dispatches, each case carrying its own packer's correctly-typed heuristic so the two non-interchangeable `FreeRectChoiceHeuristic` enums (the top-level one `MaxRects` reads and the nested `GuillotineBinPack.FreeRectChoiceHeuristic`) never cross. A nesting job picks its algorithm by intent — `NestStrategy.Density` (max-rects best-short-side-fit) for general nest density, `NestStrategy.PanelSaw` (guillotine minimize-area split) for saw-cut sheet goods where every cut must span edge-to-edge, `MassCut` for an identical-part yield query — and the one `Pack` fold runs all five; a new packing algorithm is one `NestStrategy` case plus one `Drive` arm, never a parallel packer surface.
- [STATEFUL_KERNEL_EXEMPTION]: REALIZED — the `RectangleBinPack` packers are mutable single-sheet state machines (`Insert` mutates the packer's free/used lists, no method throws on infeasibility — a `Rect.Height == 0` is the universal placement-failure sentinel), so `DriveStream`/`MassCut` are the page's named `[EXPRESSION_SPINE]` kernel exemptions the way `Construction/layout#ASSEMBLY_FOLD` names its `StepCursor`/`Voussoirs` enumerators: the multi-sheet placement loop and the local mutable `List<NestPlacement>` accumulator are confined to the kernel, and the surrounding `Pack` fold, the cut-list sort, and the `NestYield` projection are expression-bodied immutable `Seq` folds. The multi-sheet driver opens a fresh packer (`Init`/new) on a `Height == 0` failure and re-feeds the unplaced part — the suite ships no multi-bin driver, so the one-sheet-overflow → next-sheet loop is this owner's, and a part that fails on a fresh empty sheet is counted unplaced by `YieldOf` rather than throwing.
- [INT_DOMAIN_ADMISSION]: REALIZED — `RectangleBinPack` is `int`-domain (contrast `RectpackSharp`'s `uint`), so the BOUNDARY_ADMISSION edge ceils every fractional-millimetre `CutPart` footprint and the `StockSheet` extent to `int` at the one `Ceil`/`Drive` boundary (adding the saw `KerfMm` to each part so the blade width is reserved), drives the packer in the int domain, and offsets the returned `Rect.X`/`Y` back to scalar `NestPlacement` millimetres — the `Rect` value struct never crossing into an interior signature, the int/scalar conversion confined to the one fold. The `ResolutionMm` constant fixes the 1 mm packing granularity; a finer cut tolerance scales the constant without touching the fold.
- [MATERIAL_UTILIZATION_RECEIPT]: REALIZED — the typed `NestYield` receipt carries the algorithm evidence (`UtilizationRatio` = placed area / total sheet area, `PlacedCount`/`UnplacedCount`, `SheetCount`, `RemnantAreaMm2`, `WasteAreaMm2`) rather than a generic `IReceipt` or a bare ratio, since the suite ships no built-in occupancy metric — the consumer derives utilization from the summed `Rect.Area`. `MaxRectsBinPack` and `GuillotineBinPack` alone surface their `FreeRectangles` remnant geometry, so the `LargestRemnantMm2` reader the `Drive` arm threads to `DriveStream` reads the largest reusable offcut on the final partial sheet from `MaxRectsBinPack.FreeRectangles` / `GuillotineBinPack` post-`MergeFreeRectangles` (the panel-saw defragmentation that recovers contiguous remnant area) and sets `RemnantAreaMm2` to it — distinct from `WasteAreaMm2` (the total `sheetArea − usedArea` scrap); the `Skyline`/`Shelf`/`SingleBinPack` strategies keep their free lists private so their arms thread a 0-remnant reader, the utilization ratio deriving from the accumulated placement areas for every strategy alike. The `NestYield.WasteAreaMm2` is the source the `Properties/sustainability#SUSTAINABILITY` offcut-waste embodied-carbon/material-cost rollup reads by `MaterialId`, the cutting plan crossing to `Rasm.Fabrication`'s process owner as portable scalar data (a [WIRE] seam — Materials produces the plan, Fabrication consumes it, the acyclic strata forbidding the AEC peer a `Fabrication` reference).
- [STRATA_DISJOINT_NESTING]: the `RectangleBinPack.CSharp` AABB rectangle cutting-stock suite and `Rasm.Fabrication`'s `RectpackSharp`/`Clipper2` true-shape irregular-polygon nesting kernel own DISJOINT geometry across the strata wall: this owner is the academic MaxRects/Skyline/Guillotine/Shelf family plus the homogeneous mass-cut, AABB-only (irregular-polygon feasibility has no owner here); Fabrication owns the true-shape arm. Neither type crosses the strata wall — `Rasm.Materials` mints its OWN in-folder rectangle packing pin precisely so no sideways `Materials → Fabrication` dependency is created, the AEC peers depending only UPWARD to the `Rasm` kernel. The seam to Fabrication is the produced cutting plan as wire data, never a project reference.
