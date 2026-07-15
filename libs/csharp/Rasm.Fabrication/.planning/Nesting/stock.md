# [RASM_FABRICATION_STOCK]

`StockNest` is the rectangular cutting-stock and yield owner. `Pack` accepts one `NestRun`, expands physical instances, explores bounded per-part orientation assignments, respects the finite sheet limit, dispatches one `NestStrategy`, verifies every provider placement, and returns one complete `NestPlan`. A partial provider result rails `StockOverflow`; it never masquerades as a successful plan.

`NestPlacement` distinguishes source definition identity from physical instance identity and separates true part extents from padded solver extents. `SheetYield` preserves the complete provider free-rectangle geometry as material-keyed `StockOffcut` rows and derives each reuse verdict from `MinReusableAreaMm2`. Area, part-perimeter, utilization, and waste use true part dimensions; collision and containment verification use padded dimensions.

The `RectangleBinPacking` implementations and heuristic vocabularies collapse behind `NestStrategy`. `Best` exhausts a `NestFamily`; `MassCut` respects `MaxPartsPerSheet`; `CutPart.AllowRotation` controls only its own instance. The `int` provider domain remains an admission boundary and never crosses the portable receipt.

Wire posture: HOST-LOCAL. `NestPlan` crosses only through `FabricationInput.Plan`, and `NestYield` remains scalar in-process evidence.

## [01]-[INDEX]

- [01]-[NEST_STRATEGY]: owns the closed provider-dispatch family and bounded heuristic sweeps.
- [02]-[STOCK_MODEL]: owns cut definitions, physical instances, stock, placements, offcuts, and yield receipts.
- [03]-[STOCK_NEST]: owns validation, orientation generation, multi-sheet driving, provider verification, and receipt projection.

## [02]-[NEST_STRATEGY]

- Owner: `NestStrategy` `[Union]` the packer-algorithm axis; `NestFamily` `[SmartEnum<string>]` the sweepable packer family.
- Cases: `MaxRects`, `Skyline`, `Guillotine`, and `Shelf` carry provider-typed heuristics; `MassCut` carries `MaxPartsPerSheet`; `Best` carries the `NestFamily` whose heuristic space it exhausts. Rotation is carried by `PackPart`, never by a strategy-wide Boolean.
- Growth: a new packing algorithm is one `NestStrategy` `[Union]` case binding its packer + heuristic and one `Drive` switch arm; a new sweepable family is one `NestFamily` row plus one `BestOf` arm.

## [03]-[STOCK_MODEL]

- Owner: `CutPart` is a repeatable source definition; `PackPart.Instance` is its unique physical instance; `StockSheet` owns material, usable extent, kerf, trim, provider resolution, and minimum reusable area; `NestRun` owns finite sheet and orientation budgets; `NestPlacement` records source, instance, true extent, packed extent, sheet, position, and orientation; `SheetYield` and `NestYield` own offcut and aggregate evidence.
- Cases: `CutPart.AllowRotation` applies per instance; `MarginMm` and `KerfMm` enlarge only the solver footprint; `TrimMm` shrinks only the usable sheet. Quantity expansion preserves `PartId` and assigns a monotonic `Instance` before orientation search.
- Receipt: `NestYield` aggregates placed count, unplaced count, sheet count, true-part utilization, offcut area, waste, exact rectangular part perimeter, and `SheetYield` rows. Each `SheetYield` preserves every exposed material-keyed `StockOffcut`, its reuse verdict, and its largest reusable area.
- Growth: a new part attribute (a defect map) is one `CutPart` column the fit gate reads; a new sheet attribute (a defect zone) is one `StockSheet` column the packable region subtracts; a new utilization metric is one `NestYield`/`SheetYield` field — never a parallel nesting owner.

## [04]-[STOCK_NEST]

- Entry: `Pack(NestRun) -> Fin<NestPlan>` validates quantities, expanded identity cardinality, geometry, material consistency, finite sheet and orientation budgets, strategy policy, and maximum part extent. It evaluates bounded orientation assignments, keeps the highest true-part yield, rejects incomplete results, verifies provider containment and non-overlap, and projects the final receipts.
- Packages: `RectangleBinPacking`, `Rasm`, `Rasm.Element`, `Thinktecture.Runtime.Extensions`, and `LanguageExt.Core`.
- Law: `Drive` confines the provider's stateful `Insert` protocol to one boundary kernel. Orientation enumeration, heuristic sweeps, best-run selection, verification, and receipt projection remain expression-shaped. Provider `Rect` values do not escape the kernel.
- Boundary: `Rect.Height == 0` is the provider infeasibility sentinel; a provider run is accepted only after total placement, `Rect.Contains`, and pairwise `Rect.Intersects` verification. `NestPlacement.PartWidthMm`/`PartHeightMm` remain distinct from `PackedWidthMm`/`PackedHeightMm`. A physical `Instance` never collapses into its source `PartId`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using System.Numerics;
using Thinktecture;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rasm.Element.Composition;
using RectangleBinPacking;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] -------------------------------------------------------------------------------
// One NestStrategy [Union] collapses the five RectangleBinPack packer classes PLUS the heuristic-sweep best-of, each
// interactive case carrying its OWN packer's correctly-typed heuristic enum — never magic ordinals, never six parallel
// packer surfaces. A new algorithm is a case; a Best sweep is a case carrying which packer Family to exhaust.
[Union]
public abstract partial record NestStrategy {
    private NestStrategy() { }
    public sealed record MaxRects(FreeRectChoiceHeuristic Choice) : NestStrategy;
    public sealed record Skyline(SkylineBinPack.LevelChoiceHeuristic Level, bool UseWasteMap) : NestStrategy;
    public sealed record Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic Choice, GuillotineBinPack.GuillotineSplitHeuristic Split, bool Merge) : NestStrategy;
    public sealed record Shelf(ShelfBinPack.ShelfChoiceHeuristic Choice, bool UseWasteMap) : NestStrategy;
    public sealed record MassCut(int MaxPartsPerSheet) : NestStrategy;
    // The heuristic SWEEP: drive ONE packer family once per heuristic in its vocabulary and keep the highest-yield run.
    // The suite ships no built-in best-of, and a single fixed heuristic leaves yield on the table — so the sweep is the
    // cutting-stock "best fit" the production cut-shop wants when the part mix is unknown ahead of time.
    public sealed record Best(NestFamily Family) : NestStrategy;

    // The intent shortcuts — a job names its algorithm, not its heuristic ordinal.
    public static readonly NestStrategy Density = new MaxRects(FreeRectChoiceHeuristic.RectBestShortSideFit);
    public static readonly NestStrategy PanelSaw = new Guillotine(GuillotineBinPack.FreeRectChoiceHeuristic.RectBestAreaFit, GuillotineBinPack.GuillotineSplitHeuristic.SplitMinimizeArea, Merge: true);
    public static readonly NestStrategy Optimal = new Best(NestFamily.MaxRects);
}

// The packer family the Best sweep exhausts — names which packer's heuristic vocabulary the sweep drives, never a
// second packer surface (the Switch routes each family to the SAME DriveStream the interactive cases use).
[SmartEnum<string>]
public sealed partial class NestFamily {
    public static readonly NestFamily MaxRects = new("max-rects");
    public static readonly NestFamily Skyline = new("skyline");
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

file readonly record struct PackPart(int PartId, int Instance, MaterialId Material, double WidthMm, double HeightMm, double MarginMm, bool AllowRotation, bool Rotated) {
    public double AreaMm2 => WidthMm * HeightMm;
}

// The stock extent and the saw geometry. KerfMm is the blade width reserved per cut; TrimMm is the unusable edge inset
// (the gripper/clamp margin or mill-edge defect band) the packable area shrinks by on every side, so the usable extent
// is (Width − 2·Trim, Height − 2·Trim) and a placement reads back in the full-sheet frame shifted by TrimMm.
public readonly record struct StockSheet(
    MaterialId Material,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeightMm,
    double KerfMm,
    double TrimMm = 0.0,
    double ResolutionMm = 0.1,
    double MinReusableAreaMm2 = 0.0) {
    public double AreaMm2 => WidthMm.Value * HeightMm.Value;
    public double UsableWidthMm => Math.Max(0.0, WidthMm.Value - 2.0 * TrimMm);
    public double UsableHeightMm => Math.Max(0.0, HeightMm.Value - 2.0 * TrimMm);
    public double UsableAreaMm2 => UsableWidthMm * UsableHeightMm;
}

// The host-neutral placed cut — PartId + sheet index + scalar position + rotation flag, never a Rhino transform. The
// sibling Nest.Honor fold maps this straight to a PartTransform (rotate by the flag, offset to the lower-left).
public readonly record struct NestPlacement(int PartId, int Instance, MaterialId Material, int SheetIndex, double XMm, double YMm,
    double PartWidthMm, double PartHeightMm, double PackedWidthMm, double PackedHeightMm, bool Rotated) {
    // The cut perimeter the cut-length receipt sums — the saw travel to free this part from the stock.
    public double AreaMm2 => PartWidthMm * PartHeightMm;
    public double PerimeterMm => 2.0 * (PartWidthMm + PartHeightMm);
}

// The per-sheet ledger row — the cutting-stock leftover ledger is PER SHEET, not last-sheet-only: a cut-shop schedules
// each sheet, re-stocks each sheet's largest offcut, and the procurement rollup reads each sheet's waste. MaxRects and
// merged Guillotine free rectangles are MAXIMAL and may overlap one another — candidate offcuts, never a disjoint
// partition — so LargestRemnantMm2 is the restockable ledger value and the row set stays diagnostic evidence.
public readonly record struct StockOffcut(int SheetIndex, MaterialId Material, double XMm, double YMm, double WidthMm, double HeightMm, bool Reusable) {
    public double AreaMm2 => WidthMm * HeightMm;
}

public readonly record struct SheetYield(int SheetIndex, int PlacedCount, double UtilizationRatio, Seq<StockOffcut> Offcuts,
    double LargestRemnantMm2, double PartPerimeterMm);

// The typed material-utilization receipt — algorithm evidence over the per-sheet ledger, never a generic IReceipt. The
// aggregate columns (UtilizationRatio/PlacedCount/UnplacedCount/SheetCount/WasteAreaMm2) are what Nest.Honor reads onto
// FabricationResult.Placement; the receipt retains the full per-sheet ledger, never one terminal free rectangle.
public readonly record struct NestYield(double UtilizationRatio, int PlacedCount, int UnplacedCount, int SheetCount, double RemnantAreaMm2,
    double WasteAreaMm2, double PartPerimeterMm, Seq<SheetYield> Sheets);

public sealed record NestRun(Seq<CutPart> Parts, StockSheet Sheet, NestStrategy Strategy, int SheetLimit, int OrientationBudget) {
    public static NestRun Of(Seq<CutPart> parts, StockSheet sheet) => new(parts, sheet, NestStrategy.Density, SheetLimit: 1, OrientationBudget: 4096);
}

public sealed record NestPlan(NestRun Run, Seq<NestPlacement> Placements, NestYield Yield);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StockNest {
    // The six input gates are INDEPENDENT facts of one NestRun, so they accumulate on Validation and cross to Fin
    // once; provider-domain, material, fit, and homogeneity gates depend on the accumulated facts and stay monadic.
    public static Fin<NestPlan> Pack(NestRun run) =>
        from admitted in Seq(
                Gate(!run.Parts.IsEmpty, FabricationFault.Nest(NestFault.EmptyCutList, 0).ToError()),
                Gate(run.SheetLimit > 0 && run.OrientationBudget > 0 && run.Parts.ForAll(static part =>
                        part.PartId >= 0 && part.Quantity > 0 && double.IsFinite(part.MarginMm) && part.MarginMm >= 0.0) &&
                    run.Parts.GroupBy(static part => part.PartId).ForAll(static group => group.Count() == 1) &&
                    run.Parts.Fold(BigInteger.Zero, static (count, part) => count + part.Quantity) <= int.MaxValue,
                    GeometryFault.DegenerateInput("stock:invalid-cut-list").ToError()),
                Gate(run.Strategy is not NestStrategy.MassCut massCut || massCut.MaxPartsPerSheet > 0,
                    GeometryFault.DegenerateInput("stock:invalid-mass-cut-policy").ToError()),
                Gate(double.IsFinite(run.Sheet.KerfMm) && run.Sheet.KerfMm >= 0.0 && double.IsFinite(run.Sheet.TrimMm) && run.Sheet.TrimMm >= 0.0 &&
                    double.IsFinite(run.Sheet.ResolutionMm) && run.Sheet.ResolutionMm > 0.0 &&
                    double.IsFinite(run.Sheet.MinReusableAreaMm2) && run.Sheet.MinReusableAreaMm2 >= 0.0,
                    GeometryFault.DegenerateInput("stock:invalid-sheet-policy").ToError()),
                Gate(run.Sheet.WidthMm.Value > 0.0 && run.Sheet.HeightMm.Value > 0.0, GeometryFault.DegenerateInput("stock:degenerate-sheet").ToError()),
                Gate(run.Sheet.UsableWidthMm > 0.0 && run.Sheet.UsableHeightMm > 0.0, GeometryFault.DegenerateInput("stock:trim-exceeds-sheet").ToError()))
            .Traverse(static gate => gate).As().ToFin().Map(static _ => unit)
        from providerDomain in guard(run.Sheet.UsableWidthMm / run.Sheet.ResolutionMm <= int.MaxValue &&
            run.Sheet.UsableHeightMm / run.Sheet.ResolutionMm <= int.MaxValue,
            GeometryFault.DegenerateInput("stock:provider-domain-overflow").ToError())
        let sheetW = (int)Math.Floor(run.Sheet.UsableWidthMm / run.Sheet.ResolutionMm)
        let sheetH = (int)Math.Floor(run.Sheet.UsableHeightMm / run.Sheet.ResolutionMm)
        let parts = Expand(run.Parts).OrderByDescending(static p => p.AreaMm2).ToSeq()
        from material in parts.Find(p => p.Material != run.Sheet.Material).Match(
            Some: p => Fin.Fail<Unit>(FabricationFault.Nest(NestFault.MaterialMismatch, p.PartId).ToError()),
            None: static () => Fin.Succ(unit))
        from fits in run.Parts.Find(p => !FitsSheet(p, run.Sheet.KerfMm, run.Sheet.ResolutionMm, sheetW, sheetH)).Match(
            Some: p => Fin.Fail<Unit>(FabricationFault.Nest(NestFault.PartExceedsStock, p.PartId).ToError()),
            None: static () => Fin.Succ(unit))
        // MassCut drives SingleBinPack over ONE footprint × quantity, so a heterogeneous cut-list under the MassCut
        // strategy silently mis-cuts the tail as copies of the head absent a guard. The strategy NAMES a homogeneous
        // yield query, so a non-uniform footprint is a malformed job railed HERE rather than a silent drop — the
        // interactive packers place each distinct part and impose no homogeneity. The MassCut test is a case pattern
        // (one hop, no parallel total Switch a new strategy must grow an arm in).
        from uniform in run.Strategy is NestStrategy.MassCut && !Homogeneous(parts)
            ? Fin.Fail<Unit>(FabricationFault.Nest(
                NestFault.HeterogeneousMassCut,
                parts.Find(p => parts.Head.Map(h => p.WidthMm != h.WidthMm || p.HeightMm != h.HeightMm).IfNone(false)).Map(static p => p.PartId).IfNone(0)).ToError())
            : Fin.Succ(unit)
        let nest = Orientations(run.Parts, run.OrientationBudget, run.Strategy)
            .Map(oriented => Drive(run.Strategy, oriented, run.Sheet, sheetW, sheetH, run.SheetLimit))
            .Fold(Seq<(NestPlacement Placement, Seq<Rect> Free)>(), static (best, candidate) => Yield(candidate) > Yield(best) ? candidate : best)
        let sheetsUsed = nest.IsEmpty ? 0 : nest.Max(static row => row.Placement.SheetIndex) + 1
        from complete in guard(nest.Count == parts.Count,
            FabricationFault.StockOverflow(parts.Count - nest.Count, sheetsUsed).ToError())
        from verified in Verify(nest, run.Sheet)
        select new NestPlan(run, nest.Map(static d => d.Placement), YieldOf(nest, parts.Count, run.Sheet));

    static Validation<Error, Unit> Gate(bool holds, Error fault) =>
        holds ? Fin.Succ(unit).ToValidation() : Fin.Fail<Unit>(fault).ToValidation();

    // A homogeneous cut-list — every part shares the head's footprint (the PositiveMagnitude [ValueObject] gives
    // value-equality), so SingleBinPack's one-footprint × quantity yield is the correct cut for the whole list. Guards
    // the MassCut strategy alone (Pack tests `run.Strategy is not NestStrategy.MassCut || Homogeneous(parts)`).
    static bool Homogeneous(Seq<PackPart> parts) =>
        parts.Head.Match(
            Some: head => parts.ForAll(p => p.WidthMm == head.WidthMm && p.HeightMm == head.HeightMm && p.MarginMm == head.MarginMm),
            None: static () => true);

    // The int packing footprint — the fractional-mm extent plus the saw kerf AND the part's finish margin, ceiled;
    // both reserves clamp non-negative so a mis-entered saw geometry can never shrink a footprint below its extent.
    static int Ceil(double mm, double kerfMm, double marginMm, double resolutionMm) =>
        (int)Math.Ceiling((mm + Math.Max(0.0, kerfMm) + Math.Max(0.0, marginMm)) / resolutionMm);

    // A part fits in its given orientation, or — when the run admits rotation AND the part is not grain-locked —
    // rotated 90°; the int packing domain reserves the saw kerf and the finish margin on each footprint before the test.
    static bool FitsSheet(CutPart p, double kerfMm, double resolutionMm, int sheetW, int sheetH) {
        if ((p.WidthMm.Value + kerfMm + p.MarginMm) / resolutionMm > int.MaxValue ||
            (p.HeightMm.Value + kerfMm + p.MarginMm) / resolutionMm > int.MaxValue) return false;
        int w = Ceil(p.WidthMm.Value, kerfMm, p.MarginMm, resolutionMm), h = Ceil(p.HeightMm.Value, kerfMm, p.MarginMm, resolutionMm);
        return (w <= sheetW && h <= sheetH) || (p.AllowRotation && h <= sheetW && w <= sheetH);
    }

    // Each CutPart of Quantity n expands to n unit parts the packer places one at a time; PartId copies through, so
    // N placements of one part map N transforms of ONE profile at the Nest.Honor seam.
    static Seq<PackPart> Expand(Seq<CutPart> parts) =>
        parts.Fold((Next: 0, Rows: Seq<PackPart>()), static (state, part) => (
            state.Next + part.Quantity,
            state.Rows.Concat(toSeq(Enumerable.Range(state.Next, part.Quantity)).Map(instance =>
                new PackPart(part.PartId, instance, part.Material, part.WidthMm.Value, part.HeightMm.Value, part.MarginMm, part.AllowRotation, Rotated: false))))).Rows;

    static Seq<Seq<PackPart>> Orientations(Seq<CutPart> parts, int budget, NestStrategy strategy) {
        Seq<PackPart> expanded = Expand(parts).OrderByDescending(static part => part.AreaMm2).ToSeq();
        if (strategy is NestStrategy.MassCut)
            return Seq1(expanded).Concat(expanded.ForAll(static part => part.AllowRotation) &&
                    expanded.Exists(static part => part.WidthMm != part.HeightMm)
                ? Seq1(expanded.Map(static part => part with {
                    WidthMm = part.HeightMm,
                    HeightMm = part.WidthMm,
                    Rotated = true
                }))
                : Seq<Seq<PackPart>>());
        Arr<int> rotatable = expanded.Map((part, index) => (part, index)).Filter(static row => row.part.AllowRotation).Map(static row => row.index).ToArr();
        BigInteger combinations = BigInteger.One << rotatable.Count;
        int samples = (int)BigInteger.Min(combinations, budget);
        return toSeq(Enumerable.Range(0, samples)).Map(sample => {
            BigInteger mask = combinations <= budget
                ? sample
                : (sample * (combinations - BigInteger.One)) / Math.Max(1, samples - 1);
            Set<int> rotated = toSet(rotatable.Map((index, bit) => (mask & (BigInteger.One << bit)) != BigInteger.Zero ? index : -1)
                .Filter(static index => index >= 0));
            return expanded.Map((part, index) => rotated.Contains(index)
                ? part with { WidthMm = part.HeightMm, HeightMm = part.WidthMm, Rotated = true }
                : part);
        });
    }

    // The largest reusable offcut on one sheet's surfaced free-rect list, scaled out of the int packing domain — the
    // cutting-stock leftover-ledger value, distinct from total scrap.
    static double LargestRemnantMm2(Seq<StockOffcut> offcuts) =>
        offcuts.Filter(static offcut => offcut.Reusable).OrderByDescending(static offcut => offcut.AreaMm2)
            .Head.Map(static offcut => offcut.AreaMm2).IfNone(0.0);

    static Fin<Unit> Verify(Seq<(NestPlacement Placement, Seq<Rect> Free)> driven, StockSheet sheet) {
        Rect bin = new(0, 0, (int)Math.Floor(sheet.UsableWidthMm / sheet.ResolutionMm), (int)Math.Floor(sheet.UsableHeightMm / sheet.ResolutionMm));
        Option<int> invalidSheet = driven.GroupBy(static row => row.Placement.SheetIndex).Select(group => {
            Arr<Rect> used = group.Select(row => new Rect(
                (int)Math.Round((row.Placement.XMm - sheet.TrimMm) / sheet.ResolutionMm),
                (int)Math.Round((row.Placement.YMm - sheet.TrimMm) / sheet.ResolutionMm),
                (int)Math.Round(row.Placement.PackedWidthMm / sheet.ResolutionMm),
                (int)Math.Round(row.Placement.PackedHeightMm / sheet.ResolutionMm))).ToArr();
            bool contained = used.ForAll(bin.Contains);
            bool disjoint = toSeq(Enumerable.Range(0, used.Count)).ForAll(i =>
                toSeq(Enumerable.Range(i + 1, used.Count - i - 1)).ForAll(j => !used[i].Intersects(used[j])));
            bool conserved = used.Sum(static rect => rect.Area) <= bin.Area;
            return (group.Key, Valid: contained && disjoint && conserved);
        }).ToSeq().Find(static row => !row.Valid).Map(static row => row.Key);
        return invalidSheet.Match(
            Some: sheetIndex => Fin.Fail<Unit>(GeometryFault.DegenerateInput($"stock:invalid-pack:{sheetIndex}").ToError()),
            None: static () => Fin.Succ(unit));
    }

    // Utilization and waste use true part area, while the provider verification retains padded solver extents.
    static NestYield YieldOf(Seq<(NestPlacement Placement, Seq<Rect> Free)> driven, int totalParts, StockSheet sheet) {
        Seq<SheetYield> sheets = SheetLedger(driven, sheet);
        int sheetCount = sheets.Count;
        double usedArea = driven.Sum(static d => d.Placement.AreaMm2);
        double usableArea = sheet.UsableAreaMm2 * Math.Max(1, sheetCount);
        return new NestYield(
            UtilizationRatio: usableArea > 0.0 ? Math.Clamp(usedArea / usableArea, 0.0, 1.0) : 0.0,
            PlacedCount: driven.Count,
            UnplacedCount: Math.Max(0, totalParts - driven.Count),
            SheetCount: sheetCount,
            RemnantAreaMm2: sheets.Sum(static s => s.LargestRemnantMm2),
            WasteAreaMm2: Math.Max(0.0, usableArea - usedArea),
            PartPerimeterMm: sheets.Sum(static s => s.PartPerimeterMm),
            Sheets: sheets);
    }

    // Every placement on one sheet carries the same final provider free-rectangle census.
    static Seq<SheetYield> SheetLedger(Seq<(NestPlacement Placement, Seq<Rect> Free)> driven, StockSheet sheet) =>
        driven.GroupBy(static d => d.Placement.SheetIndex)
            .OrderBy(static g => g.Key)
            .Select(g => {
                Seq<(NestPlacement Placement, Seq<Rect> Free)> rows = g.ToSeq();
                double sheetUsed = rows.Sum(static d => d.Placement.AreaMm2);
                Seq<StockOffcut> offcuts = rows.Head.Map(row => row.Free.Map(rect => new StockOffcut(g.Key, sheet.Material,
                    rect.X * sheet.ResolutionMm + sheet.TrimMm, rect.Y * sheet.ResolutionMm + sheet.TrimMm,
                    rect.Width * sheet.ResolutionMm, rect.Height * sheet.ResolutionMm,
                    rect.Width * rect.Height * sheet.ResolutionMm * sheet.ResolutionMm >= sheet.MinReusableAreaMm2))).IfNone(Seq<StockOffcut>());
                return new SheetYield(
                    SheetIndex: g.Key,
                    PlacedCount: rows.Count,
                    UtilizationRatio: sheet.UsableAreaMm2 > 0.0 ? Math.Clamp(sheetUsed / sheet.UsableAreaMm2, 0.0, 1.0) : 0.0,
                    Offcuts: offcuts,
                    LargestRemnantMm2: LargestRemnantMm2(offcuts),
                    PartPerimeterMm: rows.Sum(static d => d.Placement.PerimeterMm));
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
    static Seq<(NestPlacement Placement, Seq<Rect> Free)> Drive(
        NestStrategy strategy, Seq<PackPart> parts, StockSheet sheet, int sheetW, int sheetH, int sheetLimit) =>
        strategy.Switch(
            maxRects:   m => DriveStream(parts, sheet, sheetLimit, () => new MaxRectsBinPack(sheetW, sheetH, allowRotations: false), (pk, w, h) => pk.Insert(w, h, m.Choice), static pk => pk.FreeRectangles.ToSeq()),
            skyline:    s => DriveStream(parts, sheet, sheetLimit, () => new SkylineBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Level), static _ => Seq<Rect>()),
            guillotine: g => DriveStream(parts, sheet, sheetLimit, () => new GuillotineBinPack(sheetW, sheetH), (pk, w, h) => pk.Insert(w, h, g.Merge, g.Choice, g.Split), static pk => { pk.MergeFreeRectangles(); return pk.FreeRectangles.ToSeq(); }),
            shelf:      s => DriveStream(parts, sheet, sheetLimit, () => new ShelfBinPack(sheetW, sheetH, s.UseWasteMap), (pk, w, h) => pk.Insert(w, h, s.Choice), static _ => Seq<Rect>()),
            massCut:    m => MassCut(parts, sheet, sheetW, sheetH, m.MaxPartsPerSheet, sheetLimit),
            best:       b => BestOf(b.Family, parts, sheet, sheetW, sheetH, sheetLimit));

    // The Best sweep: drive the named family once per heuristic in its vocabulary, keep the run whose placed area is
    // greatest (the highest material yield). Each inner run is its own confined DriveStream kernel; the sweep itself is a
    // pure Fold over the candidate runs — so the [EXPRESSION_SPINE] exemption stays the single-sheet driver, never grows.
    static Seq<(NestPlacement Placement, Seq<Rect> Free)> BestOf(
        NestFamily family, Seq<PackPart> parts, StockSheet sheet, int sheetW, int sheetH, int sheetLimit) =>
        family.Switch(
            maxRects: _ => Best(toSeq(Enum.GetValues<FreeRectChoiceHeuristic>())
                .Map(choice => DriveStream(parts, sheet, sheetLimit, () => new MaxRectsBinPack(sheetW, sheetH, allowRotations: false), (pk, w, h) => pk.Insert(w, h, choice), static pk => pk.FreeRectangles.ToSeq()))),
            skyline: _ => Best(toSeq(Enum.GetValues<SkylineBinPack.LevelChoiceHeuristic>())
                .Bind(level => Seq(false, true).Map(waste => DriveStream(parts, sheet,
                    sheetLimit, () => new SkylineBinPack(sheetW, sheetH, waste), (pk, w, h) => pk.Insert(w, h, level), static _ => Seq<Rect>())))),
            guillotine: _ => Best(toSeq(Enum.GetValues<GuillotineBinPack.FreeRectChoiceHeuristic>())
                .Bind(choice => toSeq(Enum.GetValues<GuillotineBinPack.GuillotineSplitHeuristic>())
                    .Bind(split => Seq(false, true).Map(merge => DriveStream(parts, sheet,
                        sheetLimit, () => new GuillotineBinPack(sheetW, sheetH), (pk, w, h) => pk.Insert(w, h, merge, choice, split),
                        pk => { if (merge) pk.MergeFreeRectangles(); return pk.FreeRectangles.ToSeq(); }))))),
            shelf: _ => Best(toSeq(Enum.GetValues<ShelfBinPack.ShelfChoiceHeuristic>())
                .Bind(choice => Seq(false, true).Map(waste => DriveStream(parts, sheet,
                    sheetLimit, () => new ShelfBinPack(sheetW, sheetH, waste), (pk, w, h) => pk.Insert(w, h, choice), static _ => Seq<Rect>())))));

    // The yield comparator: the run placing the most total area wins. A Fold over the candidate runs (LanguageExt Seq has
    // no MaxBy) keeps the higher-yield accumulator, the placed-rectangle areas summed in scalar millimetres — a pure fold.
    static Seq<(NestPlacement Placement, Seq<Rect> Free)> Best(Seq<Seq<(NestPlacement Placement, Seq<Rect> Free)>> candidates) =>
        candidates.Fold(Seq<(NestPlacement, Seq<Rect>)>(), static (best, run) => Yield(run) > Yield(best) ? run : best);

    static double Yield(Seq<(NestPlacement Placement, Seq<Rect> Free)> run) => run.Sum(static r => r.Placement.AreaMm2);

    // One generic loop over any RectangleBinPack packer (type inferred from `open`) — no boxing: the stateful single-sheet
    // packer is opened, fed one part at a time, and re-opened on a Rect.Height == 0 overflow for the next sheet. Each
    // placement is paired with its CURRENT sheet's largest-remnant reader so the per-sheet ledger reads a true offcut; the
    // remnant is re-read whenever a sheet closes (an overflow) and once more at the stream end for the final partial sheet.
    static Seq<(NestPlacement Placement, Seq<Rect> Free)> DriveStream<TPacker>(Seq<PackPart> parts, StockSheet sheet, int sheetLimit, Func<TPacker> open,
        Func<TPacker, int, int, Rect> insert, Func<TPacker, Seq<Rect>> free) {
        List<(NestPlacement, Seq<Rect>)> placements = new();
        TPacker packer = open();
        int sheetIndex = 0;
        int sheetStart = 0;   // index into `placements` where the current sheet's rows begin (for the per-sheet remnant stamp)
        foreach (PackPart part in parts) {
            int w = Ceil(part.WidthMm, sheet.KerfMm, part.MarginMm, sheet.ResolutionMm);
            int h = Ceil(part.HeightMm, sheet.KerfMm, part.MarginMm, sheet.ResolutionMm);
            Rect placed = insert(packer, w, h);
            if (placed.Height == 0) {
                if (sheetIndex + 1 >= sheetLimit) continue;
                StampSheet(placements, sheetStart, free(packer));
                packer = open(); sheetIndex++; sheetStart = placements.Count;
                placed = insert(packer, w, h);
            }
            if (placed.Height == 0) { continue; }
            placements.Add((new NestPlacement(part.PartId, part.Instance, part.Material, sheetIndex,
                placed.X * sheet.ResolutionMm + sheet.TrimMm, placed.Y * sheet.ResolutionMm + sheet.TrimMm,
                part.WidthMm, part.HeightMm, placed.Width * sheet.ResolutionMm, placed.Height * sheet.ResolutionMm, part.Rotated), Seq<Rect>()));
        }
        StampSheet(placements, sheetStart, free(packer));
        return placements.ToSeq();
    }

    // Stamp the closing sheet's largest-remnant onto every placement of that sheet (the SheetLedger reads it back).
    static void StampSheet(List<(NestPlacement Placement, Seq<Rect> Free)> rows, int from, Seq<Rect> free) {
        for (int i = from; i < rows.Count; i++) { rows[i] = (rows[i].Placement, free); }
    }

    // The homogeneous mass-cut yield, MULTI-SHEET: SingleBinPack.Insert(w, h, quantity) returns the List<Rect> of
    // as-many-as-fit identical parts on ONE sheet, so the driver opens a fresh SingleBinPack per sheet and re-feeds
    // the remaining quantity until it exhausts — the one-sheet truncation that recorded every part past sheet 0 as
    // unplaced was the named partial-success defect. Pack's Homogeneous guard has already proved every part shares
    // the head's footprint, so driving the head footprint × remaining is exact, never a silent tail mis-cut. The
    // orientation resolves ONCE on the first sheet (a square part never rotates; otherwise the rotated footprint is
    // driven when it yields more) and holds for the run. A sheet placing zero terminates with the remainder counted
    // unplaced by the Drive return. SingleBinPack surfaces no free-rect list, so every mass-cut sheet reports a 0
    // remnant. The per-sheet loop is the same [PACKER_DRIVE] statement exemption DriveStream carries.
    static Seq<(NestPlacement Placement, Seq<Rect> Free)> MassCut(
        Seq<PackPart> parts, StockSheet sheet, int sheetW, int sheetH, int maxPartsPerSheet, int sheetLimit) =>
        parts.Head.Match(
            Some: part => {
                int w = Ceil(part.WidthMm, sheet.KerfMm, part.MarginMm, sheet.ResolutionMm);
                int h = Ceil(part.HeightMm, sheet.KerfMm, part.MarginMm, sheet.ResolutionMm);
                Seq<Rect> first = toSeq(new SingleBinPack(sheetW, sheetH).Insert(w, h, Math.Min(parts.Count, maxPartsPerSheet)));
                List<(NestPlacement, Seq<Rect>)> placements = new();
                int remaining = parts.Count, consumed = 0, sheetIndex = 0;
                while (remaining > 0 && sheetIndex < sheetLimit) {
                    Seq<Rect> placed = sheetIndex == 0
                        ? first
                        : toSeq(new SingleBinPack(sheetW, sheetH).Insert(w, h, Math.Min(remaining, maxPartsPerSheet)));
                    if (placed.IsEmpty) { break; }
                    Seq<PackPart> batch = parts.Skip(consumed).Take(placed.Count);
                    placements.AddRange(placed.Zip(batch, (rect, item) => (
                        new NestPlacement(item.PartId, item.Instance, item.Material, sheetIndex,
                            rect.X * sheet.ResolutionMm + sheet.TrimMm, rect.Y * sheet.ResolutionMm + sheet.TrimMm,
                            item.WidthMm, item.HeightMm, rect.Width * sheet.ResolutionMm, rect.Height * sheet.ResolutionMm, item.Rotated), Seq<Rect>())));
                    int accepted = Math.Min(remaining, placed.Count);
                    consumed += accepted;
                    remaining -= accepted;
                    sheetIndex++;
                }
                return placements.ToSeq();
            },
            None: () => Seq<(NestPlacement, Seq<Rect>)>());
}
```
