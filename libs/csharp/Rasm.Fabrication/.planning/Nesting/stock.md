# [RASM_FABRICATION_STOCK]

`StockNest` owns rectangular cutting-stock assignment across finite heterogeneous inventory. One admitted `NestRun` expands physical part instances, derives rectangular stock frames, proves eligibility, evaluates the complete provider family, and admits one geometry- and conservation-proved `NestPlan` with stable evidence identity.

`Pack` remains the single rectangular entry. `NestRun`, `NestPlan`, `NestPlacement`, `NestYield`, and `SheetIndex` preserve the true-shape seam, while provider rectangles and heuristic enums remain private to this owner.

## [01]-[INDEX]

- [02]-[DOMAIN]: generated strategy, part, stock-frame, cut, receipt, and run owners.
- [03]-[ADMISSION]: profile expansion, quarter-turn orientation, stock-frame derivation, and eligibility graph.
- [04]-[PACKING]: bounded strategy evaluation over every `RectangleBinPack.CSharp` provider.
- [05]-[PROOF]: containment, overlap, cardinality, area, cut-pattern, and content-identity receipts.

## [02]-[DOMAIN]

- Owner: `NestRun` admits expanded parts, finite inventory, a bounded materialized strategy family, sheet budget, orientation budget, kerf, and edge allowance once.
- Owner: `StockFrame` retains stock identity, source index, coordinate origin, integer provider extent, true area, material, grain, and cost.
- Cases: `NestStrategy` carries maximal-rectangle, skyline, guillotine, shelf, homogeneous mass-cut, and parameterized sweep policies.
- Law: `CutAxis` and `CutProof` carry the ordinal every digest and validator reads, so no consumer re-derives a discriminant by case test.
- Packages: `Thinktecture` supplies the generated strategy, axis, and proof families; `LanguageExt` supplies admission and the `Fin` rail; `UnitsNet` supplies material quantities; `Rasm` supplies `ContentHash` for the snapshot digest.
- Growth: a rectangular provider or heuristic lands as one `NestStrategy` case consumed by `Evaluate`; a stock modality remains a `Stock` case consumed by `Frame`.

## [03]-[ADMISSION]

- Law: `NestRun.ResolutionMm` floors stock frames and ceils kerf-bearing part envelopes once; placement egress restores physical coordinates, extents, origins, and rotations.
- Law: `StockNest.Frames` admits rectangular frames before applying `StockLimit`, so the budget never buys frames the packer rejects.
- Law: `EligibilityGraph` joins each part instance only to stock frames satisfying material, extent, grain, and exclusion policy, then refuses any component whose part-area demand exceeds its reachable stock supply — Hall's condition BY AREA, exact for a relation where one frame carries many parts. `MaximumBipartiteMatchingAlgorithm` is refused by name: a matching saturates a one-to-one assignment, so on this relation it is vacuous where the area bound is decisive; the graph's own `AdjacentDegree` answers the orphan question the part-by-orientation cross-product re-derived, and supply indexes by component once.
- Entry: `NestRun.FromProfiles` expands every `PartRule.Quantity` into stable `(PartId, Instance)` identities, clamps the stock budget to real inventory, and narrows each rotation family to its quarter-turn subset; `RectangularBudget` and `RectangularGrid` carry the eight ceilings and grid scalars as two admitted values, so no caller transposes two adjacent positional ints.
- Packages: `QuikGraph` supplies the eligibility components; `LanguageExt` supplies the applicative admission and traversal; the `Geometry2D` owner supplies the profile measure every part envelope reads.
- Boundary: nonrectangular regions, interior exclusions, exhausted roll or coil length, unsupported stock modalities, oblique-only rotation families, orphan parts, and strategy expansion beyond its count or depth budget remain typed failures; the bounded queue walk is the strategy-materialization kernel.

## [04]-[PACKING]

- Entry: `StockNest.Pack` evaluates strategy cases and bounded orientation assignments, then selects by placed cardinality, true-part yield, and stock cost.
- Law: an `Evaluate` arm supplies only what its provider TYPE cannot share — construction, insertion, and the sheet geometry that provider publishes — so the drive fold, the option lift, and the opaque-sheet shape are hoisted once and a new provider adds an arm, never a fold. Strategy expansion folds an immutable frontier: a mutable queue beside a captured leaf list and a captured counter made the budget a predicate over three variables no expression could state.
- Auto: `Orientations` enumerates the exact mixed-radix assignment space while it fits `OrientationBudget` and draws decorrelated assignments beyond it, so no part is pinned to its first rotation.
- Auto: `MaxRectsBinPack`, `SkylineBinPack`, `GuillotineBinPack`, and `ShelfBinPack` share one multi-stock `Drive` fold; `SingleBinPack` owns homogeneous mass-cut.
- Law: the rectangular provider family is a genuine fast path, not a degenerate case of the true-shape algebra — `Pack` is reached only through `SearchOp.Rectangular`, so `Nest` holds no second provider switch and the integer-frame lane never competes with the configuration-space walk for a nonrectangular profile. The kernel offers no packing algebra that subsumes it: `OffsetOp.Minkowski` and `ArrangementOp.PlanarOverlay` build a configuration space, and a search over that space is this branch's own concern.
- Auto: `ParallelHelper.For2D` isolates each strategy-orientation evaluation; inapplicable providers return `Option.None`, provider faults abort the evaluation rail, and capacity-limited winners retain explicit unplaced cardinality in `NestPlan`.
- Auto: one `ProviderSheet` per stock frame carries the packer's own geometry; `MaxRectsBinPack` and `GuillotineBinPack` publish their placement lists, and the max-rects row admits its list only while `BinWidth`/`BinHeight` still match the frame that constructed it.
- Packages: `RectangleBinPack` supplies `MaxRectsBinPack` (`UsedRectangles`, `FreeRectangles`, `BinWidth`, `BinHeight`), `SkylineBinPack`, `GuillotineBinPack` (`MergeFreeRectangles`, `UsedRectangles`, `FreeRectangles`), `ShelfBinPack`, `SingleBinPack`, and `Rect.Area`; `Rasm` supplies the `Deterministic` lanes the beyond-budget orientation draw reads; `CommunityToolkit.HighPerformance` supplies the per-evaluation partition.
- Boundary: each packer `Insert` mutates its own free geometry, so `Drive` folds eligible frames and stops at the first fit; each `Height == 0` sentinel becomes a rejected stock candidate and no provider type crosses `ProviderRun`.

## [05]-[PROOF]

- Owner: `CutPattern` projects stock-local cut spans, provider-feasibility posture, and free rectangles without inventing a provider cut tree or claiming true-shape feasibility.
- Law: `CutAxis` rows own the trim projection, so `Pattern` generates every span and `NestPlan.Validate` re-proves it through one correspondence.
- Law: `NestPlan.Validate` proves unique instance subset coverage, source containment, pairwise non-overlap, stock eligibility, finite coordinates, yield cardinality, and the stock-minus-placement area balance; provider free rectangles remain bounded evidence, not an exact complement claim.
- Output: `NestPlan.Evidence` retains the `StockSnapshot` kind beside the digest over canonical placements, indexed stock identities, cut spans, free rectangles, run policy, and yield scalars; process-random hashes never enter identity.
- Packages: `Rasm` supplies `ContentHash` for the placement preimage; `Thinktecture` supplies the generated proof families; `LanguageExt` supplies the validation applicative.
- Receipt: `NestYield` retains requested and placed cardinality, stock count, true and rectangular areas, stock area, utilization, waste, cost, and the continuous sheet lower bound the placed count is proved against. Rectangular area reads the packer's own placed set wherever every used sheet publishes one, and `NestPlan.Validate` re-derives it from the projected placements, so the two sources disagreeing is an admission failure rather than a silent drift.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using RectangleBinPacking;
using Rhino.Geometry;
using System.Collections.Frozen;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NestStrategy {
    private NestStrategy() { }

    public sealed record MaxRects(RectChoice Choice) : NestStrategy;
    public sealed record Skyline(SkylineChoice Level, WastePosture Waste) : NestStrategy;
    public sealed record Guillotine(MergePosture Merge, GuillotineChoice Choice, GuillotineSplit Split) : NestStrategy;
    public sealed record Shelf(ShelfChoice Choice, WastePosture Waste) : NestStrategy;
    public sealed record MassCut : NestStrategy;
    public sealed record Sweep(Seq<NestStrategy> Cases) : NestStrategy;

    // Only `Sweep` carries children, so expansion is a one-column read rather than a six-arm dispatch, and the
    // frontier folds IMMUTABLY: a mutable queue beside a captured leaf list and a captured visit counter made the
    // budget check depend on three variables no single expression could state.
    internal Fin<Seq<NestStrategy>> Expand(int countBudget, int depthBudget) =>
        countBudget < 1 || depthBudget < 0
            ? Budget()
            : Walk(Seq((Strategy: this, Depth: 0)), Seq<NestStrategy>(), visited: 0, countBudget, depthBudget);

    private Option<Seq<NestStrategy>> Children => Switch(
        maxRects: static _ => Option<Seq<NestStrategy>>.None,
        skyline: static _ => Option<Seq<NestStrategy>>.None,
        guillotine: static _ => Option<Seq<NestStrategy>>.None,
        shelf: static _ => Option<Seq<NestStrategy>>.None,
        massCut: static _ => Option<Seq<NestStrategy>>.None,
        sweep: static row => Some(row.Cases));

    private static Fin<Seq<NestStrategy>> Walk(
        Seq<(NestStrategy Strategy, int Depth)> frontier,
        Seq<NestStrategy> leaves,
        int visited,
        int countBudget,
        int depthBudget) =>
        frontier.Head.Match(
            None: () => leaves.IsEmpty
                ? Fin.Fail<Seq<NestStrategy>>(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:strategy-empty"))
                : Fin.Succ(leaves),
            Some: head => visited + 1 > countBudget || head.Depth > depthBudget
                ? Budget()
                : head.Strategy.Children.Match(
                    None: () => Walk(frontier.Tail, leaves.Add(head.Strategy), visited + 1, countBudget, depthBudget),
                    Some: cases => head.Depth == depthBudget
                        || visited + 1 + frontier.Tail.Count + cases.Count > countBudget
                            ? Budget()
                            : Walk(frontier.Tail + cases.Map(child => (child, head.Depth + 1)),
                                leaves, visited + 1, countBudget, depthBudget)));

    private static Fin<Seq<NestStrategy>> Budget() =>
        Fin.Fail<Seq<NestStrategy>>(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:strategy-budget"));

    internal UInt128 Identity => StockNest.StrategyKey(this);

    internal CutProof Proof => Switch(
        maxRects: static _ => CutProof.LayoutProjection,
        skyline: static _ => CutProof.LayoutProjection,
        guillotine: static _ => CutProof.ProviderFeasible,
        shelf: static _ => CutProof.LayoutProjection,
        massCut: static _ => CutProof.ProviderFeasible,
        sweep: static _ => CutProof.LayoutProjection);
}

[SmartEnum<string>]
public sealed partial class WastePosture {
    public static readonly WastePosture Disabled = new("disabled", false);
    public static readonly WastePosture Recycled = new("recycled", true);
    internal bool Native { get; }
}

[SmartEnum<string>]
public sealed partial class MergePosture {
    public static readonly MergePosture Fragmented = new("fragmented", false);
    public static readonly MergePosture Coalesced = new("coalesced", true);
    internal bool Native { get; }
}

[SmartEnum<string>]
public sealed partial class RectChoice {
    public static readonly RectChoice ShortSide = new("short-side", FreeRectChoiceHeuristic.RectBestShortSideFit);
    public static readonly RectChoice LongSide = new("long-side", FreeRectChoiceHeuristic.RectBestLongSideFit);
    public static readonly RectChoice Area = new("area", FreeRectChoiceHeuristic.RectBestAreaFit);
    public static readonly RectChoice BottomLeft = new("bottom-left", FreeRectChoiceHeuristic.RectBottomLeftRule);
    public static readonly RectChoice Contact = new("contact", FreeRectChoiceHeuristic.RectContactPointRule);
    internal FreeRectChoiceHeuristic Native { get; }
}

[SmartEnum<string>]
public sealed partial class SkylineChoice {
    public static readonly SkylineChoice BottomLeft = new("bottom-left", LevelChoiceHeuristic.LevelBottomLeft);
    public static readonly SkylineChoice Waste = new("waste", LevelChoiceHeuristic.LevelMinWasteFit);
    internal LevelChoiceHeuristic Native { get; }
}

[SmartEnum<string>]
public sealed partial class GuillotineChoice {
    public static readonly GuillotineChoice Area = new("area", GuillotineBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
    public static readonly GuillotineChoice ShortSide = new("short-side", GuillotineBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit);
    public static readonly GuillotineChoice LongSide = new("long-side", GuillotineBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit);
    public static readonly GuillotineChoice WorstArea = new("worst-area", GuillotineBinPack.FreeRectChoiceHeuristic.RectWorstAreaFit);
    public static readonly GuillotineChoice WorstShortSide = new("worst-short-side", GuillotineBinPack.FreeRectChoiceHeuristic.RectWorstShortSideFit);
    public static readonly GuillotineChoice WorstLongSide = new("worst-long-side", GuillotineBinPack.FreeRectChoiceHeuristic.RectWorstLongSideFit);
    internal GuillotineBinPack.FreeRectChoiceHeuristic Native { get; }
}

[SmartEnum<string>]
public sealed partial class GuillotineSplit {
    public static readonly GuillotineSplit ShorterLeftover = new("shorter-leftover", GuillotineBinPack.GuillotineSplitHeuristic.SplitShorterLeftoverAxis);
    public static readonly GuillotineSplit LongerLeftover = new("longer-leftover", GuillotineBinPack.GuillotineSplitHeuristic.SplitLongerLeftoverAxis);
    public static readonly GuillotineSplit MinArea = new("min-area", GuillotineBinPack.GuillotineSplitHeuristic.SplitMinimizeArea);
    public static readonly GuillotineSplit MaxArea = new("max-area", GuillotineBinPack.GuillotineSplitHeuristic.SplitMaximizeArea);
    public static readonly GuillotineSplit ShorterAxis = new("shorter-axis", GuillotineBinPack.GuillotineSplitHeuristic.SplitShorterAxis);
    public static readonly GuillotineSplit LongerAxis = new("longer-axis", GuillotineBinPack.GuillotineSplitHeuristic.SplitLongerAxis);
    internal GuillotineBinPack.GuillotineSplitHeuristic Native { get; }
}

[SmartEnum<string>]
public sealed partial class ShelfChoice {
    public static readonly ShelfChoice Next = new("next", ShelfChoiceHeuristic.ShelfNextFit);
    public static readonly ShelfChoice First = new("first", ShelfChoiceHeuristic.ShelfFirstFit);
    public static readonly ShelfChoice BestArea = new("best-area", ShelfChoiceHeuristic.ShelfBestAreaFit);
    public static readonly ShelfChoice WorstArea = new("worst-area", ShelfChoiceHeuristic.ShelfWorstAreaFit);
    public static readonly ShelfChoice BestHeight = new("best-height", ShelfChoiceHeuristic.ShelfBestHeightFit);
    public static readonly ShelfChoice BestWidth = new("best-width", ShelfChoiceHeuristic.ShelfBestWidthFit);
    public static readonly ShelfChoice WorstWidth = new("worst-width", ShelfChoiceHeuristic.ShelfWorstWidthFit);
    internal ShelfChoiceHeuristic Native { get; }
}

[SmartEnum<string>]
public sealed partial class CutAxis {
    public static readonly CutAxis X = new("x", Ordinal: 0,
        static row => (row.XMm + row.WidthMm, row.YMm, row.YMm + row.HeightMm));
    public static readonly CutAxis Y = new("y", Ordinal: 1,
        static row => (row.YMm + row.HeightMm, row.XMm, row.XMm + row.WidthMm));

    public int Ordinal { get; }
    internal Func<NestPlacement, (double Coordinate, double Start, double End)> Trim { get; }
}

[SmartEnum<string>]
public sealed partial class CutProof {
    public static readonly CutProof ProviderFeasible = new("provider-feasible", Ordinal: 1);
    public static readonly CutProof LayoutProjection = new("layout-projection", Ordinal: 0);

    public int Ordinal { get; }
}
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CutPart {
    public int PartId { get; }
    public int Instance { get; }
    public double WidthMm { get; }
    public double HeightMm { get; }
    public double TrueAreaMm2 { get; }
    public double LinearTolerance { get; }
    public double AngularTolerance { get; }
    public Option<MaterialId> Material { get; }
    public Option<double> GrainAxis { get; }
    public Seq<double> Rotations { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref int partId, ref int instance,
        ref double widthMm, ref double heightMm, ref double trueAreaMm2, ref double linearTolerance,
        ref double angularTolerance, ref Option<MaterialId> material, ref Option<double> grainAxis, ref Seq<double> rotations) {
        double rectangleArea = widthMm * heightMm;
        double areaTolerance = linearTolerance * Math.Max(widthMm, heightMm);
        validationError = partId < 0 || instance < 0 || !double.IsFinite(widthMm) || widthMm <= 0.0
            || !double.IsFinite(heightMm) || heightMm <= 0.0 || !double.IsFinite(trueAreaMm2) || trueAreaMm2 <= 0.0
            || !double.IsFinite(linearTolerance) || linearTolerance <= 0.0
            || !double.IsFinite(rectangleArea) || !double.IsFinite(areaTolerance) || trueAreaMm2 - rectangleArea > areaTolerance
            || !double.IsFinite(angularTolerance) || angularTolerance <= 0.0
            || rotations.IsEmpty || rotations.Exists(static angle => !double.IsFinite(angle))
            || rotations.Distinct().Count != rotations.Count
            || rotations.Exists(angle => Math.Abs(Math.IEEERemainder(angle, Math.PI / 2.0)) > angularTolerance)
            || grainAxis.Exists(static angle => !double.IsFinite(angle))
                ? new GeometryFault.DegenerateInput(Kind.Polyline, partId, "stock:part").ToFabrication()
                : null;
        if (validationError is null) {
            trueAreaMm2 = Math.Min(trueAreaMm2, rectangleArea);
            rotations = toSeq(rotations.Order());
        }
    }
}

// The rectangular search's own ceilings, and the integer grid every frame and envelope quantizes onto: two
// admitted values instead of eight positional scalars whose adjacency a caller could transpose silently.
public readonly record struct RectangularBudget(
    int StrategyBudget,
    int StrategyDepth,
    int StockLimit,
    int OrientationBudget,
    int BatchFloor) {
    public bool Valid =>
        StrategyBudget >= 1 && StrategyDepth >= 0 && StockLimit >= 1 && OrientationBudget >= 1 && BatchFloor >= 1
        && (long)StrategyBudget * OrientationBudget <= int.MaxValue;
}

public readonly record struct RectangularGrid(double ResolutionMm, double KerfMm, double EdgeAllowanceMm) {
    public bool Valid =>
        Witness.Positive(ResolutionMm)
        && double.IsFinite(KerfMm) && KerfMm >= 0.0
        && double.IsFinite(EdgeAllowanceMm) && EdgeAllowanceMm >= 0.0;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class NestRun {
    public Seq<CutPart> Parts { get; }
    public Seq<Stock> Inventory { get; }
    public NestStrategy Strategy { get; }
    public Seq<NestStrategy> Strategies { get; }
    public RectangularBudget Budget { get; }
    public RectangularGrid Grid { get; }

    public double ResolutionMm => Grid.ResolutionMm;
    public double KerfMm => Grid.KerfMm;
    public double EdgeAllowanceMm => Grid.EdgeAllowanceMm;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref Seq<CutPart> parts,
        ref Seq<Stock> inventory, ref NestStrategy strategy, ref Seq<NestStrategy> strategies,
        ref RectangularBudget budget, ref RectangularGrid grid) {
        Option<Seq<NestStrategy>> expanded = budget.Valid
            ? strategy.Expand(budget.StrategyBudget, budget.StrategyDepth).ToOption()
            : None;
        RectangularGrid held = grid;
        validationError = parts.IsEmpty || inventory.IsEmpty
            || toSeq(parts.GroupBy(static part => (part.PartId, part.Instance))).Exists(static group => group.Count() != 1)
            || inventory.Exists(static stock => !stock.Physical)
            || toSeq(inventory.GroupBy(static stock => stock.Identity)).Exists(static group => group.Count() != 1)
            || budget.StockLimit > inventory.Count || !budget.Valid || !grid.Valid || expanded.IsNone
            || parts.Exists(part => Math.Ceiling((Math.Sqrt((part.WidthMm * part.WidthMm) + (part.HeightMm * part.HeightMm))
                + held.KerfMm) / held.ResolutionMm) > int.MaxValue)
                ? new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:run")
                : null;
        if (validationError is null) {
            parts = toSeq(parts.OrderBy(static part => part.PartId).ThenBy(static part => part.Instance));
            strategies = expanded.IfNone(Seq<NestStrategy>());
        }
    }

    internal static Fin<NestRun> FromProfiles(
        Arr<Loop> profiles,
        Seq<Stock> inventory,
        Seq<PartRule> rules,
        NestStrategy strategy,
        RectangularBudget budget,
        RectangularGrid grid) =>
        rules.Bind(rule => toSeq(Enumerable.Range(0, rule.Quantity)).Map(instance => (rule, instance)))
            .TraverseM(row => row.rule.PartId >= 0 && row.rule.PartId < profiles.Count
                ? Part(profiles[row.rule.PartId], row.rule, row.instance)
                : Fin.Fail<CutPart>(new FabricationFault.NoFit(row.rule.PartId, row.rule.Angles).ToError())).As()
            .Bind(parts => Validate(
                    parts,
                    inventory,
                    strategy,
                    Seq<NestStrategy>(),
                    // The stock budget clamps to REAL inventory here rather than refusing: a caller asking for more
                    // sheets than the shop holds is asking for the shop, not for a rejection.
                    budget with { StockLimit = Math.Clamp(budget.StockLimit, 1, Math.Max(inventory.Count, 1)) },
                    grid,
                    out NestRun run)
                .Admitted(run));

    // Rectangular admission retains the quarter-turn subset of a rule's rotation family; an oblique-only rule has no
    // rectangular realization and rails, never silently packing the part at an angle the provider cannot express.
    static Fin<CutPart> Part(Loop profile, PartRule rule, int instance) {
        BoundingBox bounds = profile.Bound();
        double angular = profile.Tolerance.Angular.Value;
        Seq<double> quarters = rule.Angles
            .Filter(angle => Math.Abs(Math.IEEERemainder(angle, Math.PI / 2.0)) <= angular)
            .Distinct().ToSeq();
        if (quarters.IsEmpty)
            return Fin.Fail<CutPart>(new FabricationFault.NoFit(rule.PartId, rule.Angles).ToError());
        return CutPart.Validate(rule.PartId, instance, bounds.Max.X - bounds.Min.X, bounds.Max.Y - bounds.Min.Y,
            Math.Abs(profile.Area()), profile.Tolerance.Absolute.Value, angular, rule.Material, rule.GrainAxis, quarters,
            out CutPart part).Admitted(part);
    }
}

internal sealed record StockFrame(int Index, Stock Source, double OriginX, double OriginY, int Width, int Height,
    double Resolution, double TrueArea, MaterialId Material, Option<double> GrainAxis, double Cost) {
    public UInt128 Identity => Source.Identity;
    public Rect Bounds => new(0, 0, Width, Height);
}

internal sealed record OrientedPart(PartInstance Instance, CutPart Source, int Width, int Height, double Rotation);
internal sealed record ProviderPlacement(PartInstance Instance, int StockIndex, Rect Rect, double Rotation);
// One row per stock frame carrying the packer's own geometry: what it left free and what it placed. Only the packers
// that publish a placement list fill `Placed`, and it fills only where the packer's own bin extent still matches the
// frame that constructed it, so a receipt read from it cannot disagree with the placement that produced it.
internal sealed record ProviderSheet(int Stock, Option<Seq<Rect>> Free, Option<Seq<Rect>> Placed);
internal sealed record ProviderRun(NestStrategy Strategy, Seq<ProviderPlacement> Placements, Seq<ProviderSheet> Sheets);
public sealed record NestPlacement(int PartId, int Instance, int SheetIndex, double XMm, double YMm,
    double RotationRadians, double WidthMm, double HeightMm);
public sealed record CutSpan(int PlacementIndex, CutAxis Axis, double CoordinateMm, double StartMm, double EndMm, double KerfMm);
public sealed record CutPattern(int SheetIndex, NestStrategy Strategy, CutProof Proof, Seq<CutSpan> Spans,
    Option<Seq<(double X, double Y, double Width, double Height)>> Free);
public sealed record NestYield(int RequestedCount, int PlacedCount, int UnplacedCount, int SheetCount,
    double TruePartAreaMm2, double RectangleAreaMm2, double StockAreaMm2, double UtilizationRatio, double WasteAreaMm2,
    double StockCost, int SheetLowerBound);
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class NestPlan {
    public NestRun Run { get; }
    public Seq<Stock> Stock { get; }
    public Seq<NestPlacement> Placements { get; }
    public Seq<CutPattern> Patterns { get; }
    public NestYield Yield { get; }
    public ContentKey Evidence { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref NestRun run, ref Seq<Stock> stock,
        ref Seq<NestPlacement> placements, ref Seq<CutPattern> patterns, ref NestYield yield, ref ContentKey evidence) {
        FrozenSet<PartInstance> requested = run.Parts.Map(static row => new PartInstance(row.PartId, row.Instance)).ToFrozenSet();
        FrozenSet<PartInstance> actual = placements.Map(static row => new PartInstance(row.PartId, row.Instance)).ToFrozenSet();
        FrozenDictionary<PartInstance, CutPart> parts = run.Parts
            .ToFrozenDictionary(static row => new PartInstance(row.PartId, row.Instance));
        Option<Seq<StockFrame>> admittedFrames = StockNest.Frames(run).ToOption();
        Seq<StockFrame> frames = admittedFrames.IfNone(Seq<StockFrame>());
        bool inventory = admittedFrames.Exists(derived => derived.Count == stock.Count
            && derived.Map((frame, index) => frame.Identity == stock[index].Identity).ForAll(identity));
        Seq<IGrouping<int, NestPlacement>> sheets = toSeq(placements.GroupBy(static row => row.SheetIndex));
        FrozenSet<int> used = placements.Map(static row => row.SheetIndex).ToFrozenSet();
        bool indexed = placements.ForAll(row => row.SheetIndex >= 0 && row.SheetIndex < stock.Count
            && parts.ContainsKey(new PartInstance(row.PartId, row.Instance)));
        bool placementDomain = placements.ForAll(row => row.PartId >= 0 && row.Instance >= 0
            && row.SheetIndex >= 0 && row.SheetIndex < stock.Count && double.IsFinite(row.XMm) && double.IsFinite(row.YMm)
            && double.IsFinite(row.RotationRadians) && double.IsFinite(row.WidthMm) && row.WidthMm > 0.0
            && double.IsFinite(row.HeightMm) && row.HeightMm > 0.0);
        bool eligible = inventory && indexed && placements.ForAll(row => {
            CutPart part = parts[new PartInstance(row.PartId, row.Instance)]; Stock source = frames[row.SheetIndex].Source;
            return part.Material.ForAll(material => material == source.Material)
                && part.GrainAxis.ForAll(grain => source.GrainAxis.Exists(axis =>
                    Math.Abs(Math.IEEERemainder((grain + row.RotationRadians) - axis, Math.PI)) <= part.AngularTolerance));
        });
        bool envelopes = indexed && placements.ForAll(row => {
            CutPart part = parts[new PartInstance(row.PartId, row.Instance)];
            (int Width, int Height) expected = StockNest.Envelope(part, run.ResolutionMm, run.KerfMm, row.RotationRadians);
            return part.Rotations.Exists(angle => angle == row.RotationRadians)
                && Math.Abs(row.WidthMm - (expected.Width * run.ResolutionMm)) <= run.ResolutionMm
                && Math.Abs(row.HeightMm - (expected.Height * run.ResolutionMm)) <= run.ResolutionMm;
        });
        bool geometry = inventory && indexed && sheets.ForAll(group => {
            StockFrame frame = frames[group.Key];
            Seq<BoundingBox> placed = toSeq(group).Map(row => new BoundingBox(new Point3d(row.XMm, row.YMm, 0.0),
                new Point3d(row.XMm + row.WidthMm, row.YMm + row.HeightMm, 0.0)));
            return placed.ForAll(box => frame.OriginX <= box.Min.X && frame.OriginY <= box.Min.Y
                && frame.OriginX + (frame.Width * frame.Resolution) >= box.Max.X
                && frame.OriginY + (frame.Height * frame.Resolution) >= box.Max.Y)
                && !placed.Map((left, index) => placed.Skip(index + 1).Exists(right =>
                    left.Min.X < right.Max.X && left.Max.X > right.Min.X
                    && left.Min.Y < right.Max.Y && left.Max.Y > right.Min.Y)).Exists(identity);
        });
        double slack = run.ResolutionMm * run.ResolutionMm;
        Option<NestYield> expectedYield = inventory && indexed && placementDomain
            ? Some(StockNest.YieldOf(run, frames, placements, Seq<ProviderSheet>()))
            : None;
        bool yieldDomain = expectedYield.Exists(expected => YieldMatches(yield, expected, slack));
        Seq<UInt128> selectedStrategies = patterns.Choose(pattern => Optional(pattern.Strategy).Map(strategy => strategy.Identity));
        bool oneStrategy = selectedStrategies.Count == patterns.Count && selectedStrategies.Distinct().Count == 1;
        bool cutDomain = patterns.ForAll(pattern => {
            Seq<NestPlacement> sheet = placements.Filter(row => row.SheetIndex == pattern.SheetIndex);
            Option<StockFrame> frame = inventory && pattern.SheetIndex >= 0 && pattern.SheetIndex < frames.Count
                ? Some(frames[pattern.SheetIndex])
                : None;
            bool spans = !sheet.IsEmpty && pattern.Spans.Count == CutAxis.Items.Count * sheet.Count
                && toSeq(pattern.Spans.GroupBy(static span => span.PlacementIndex)).Map(static group => toSeq(group))
                    .ForAll(static rows => rows.Map(static span => span.Axis).Distinct().Count == CutAxis.Items.Count)
                && pattern.Spans.ForAll(span => span.PlacementIndex >= 0 && span.PlacementIndex < sheet.Count
                    && double.IsFinite(span.CoordinateMm)
                    && double.IsFinite(span.StartMm) && double.IsFinite(span.EndMm) && span.StartMm <= span.EndMm
                    && double.IsFinite(span.KerfMm) && span.KerfMm >= 0.0
                    && SpanMatches(span, sheet[span.PlacementIndex], run));
            bool free = frame.Exists(bounds => pattern.Free.ForAll(rows => rows.ForAll(row => double.IsFinite(row.X)
                && double.IsFinite(row.Y) && double.IsFinite(row.Width) && row.Width > 0.0
                && double.IsFinite(row.Height) && row.Height > 0.0 && row.X >= bounds.OriginX && row.Y >= bounds.OriginY
                && row.X + row.Width <= bounds.OriginX + (bounds.Width * bounds.Resolution)
                && row.Y + row.Height <= bounds.OriginY + (bounds.Height * bounds.Resolution))));
            bool proof = pattern.Strategy is not null
                && run.Strategies.Exists(candidate => candidate.Identity == pattern.Strategy.Identity)
                && pattern.Proof == pattern.Strategy.Proof;
            return proof && spans && free;
        });
        validationError = stock.IsEmpty || placements.IsEmpty || !inventory || !placementDomain
            || toSeq(placements.GroupBy(static row => (row.PartId, row.Instance))).Exists(static group => group.Count() != 1)
            || !actual.IsSubsetOf(requested) || !eligible || !envelopes || !geometry || !yieldDomain
            || !oneStrategy || !cutDomain
            || patterns.Count != used.Count
            || patterns.Map(static pattern => pattern.SheetIndex).ToFrozenSet().SetEquals(used) is false
            || evidence is null || evidence.Kind != EgressKind.StockSnapshot || evidence.Digest == UInt128.Zero
            || evidence != StockNest.Digest(run, stock, placements, patterns, yield)
                ? new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:plan")
                : null;
    }

    static bool YieldMatches(NestYield actual, NestYield expected, double tolerance) =>
        expected.SheetLowerBound > 0 && expected.SheetCount >= expected.SheetLowerBound
        && actual.RequestedCount == expected.RequestedCount && actual.PlacedCount == expected.PlacedCount
        && actual.UnplacedCount == expected.UnplacedCount && actual.SheetCount == expected.SheetCount
        && Math.Abs(actual.TruePartAreaMm2 - expected.TruePartAreaMm2) <= tolerance
        && Math.Abs(actual.RectangleAreaMm2 - expected.RectangleAreaMm2) <= tolerance
        && Math.Abs(actual.StockAreaMm2 - expected.StockAreaMm2) <= tolerance
        && Math.Abs(actual.UtilizationRatio - expected.UtilizationRatio) <= tolerance
        && Math.Abs(actual.WasteAreaMm2 - expected.WasteAreaMm2) <= tolerance
        && Math.Abs(actual.StockCost - expected.StockCost) <= tolerance
        && actual.SheetLowerBound == expected.SheetLowerBound;

    static bool SpanMatches(CutSpan span, NestPlacement placement, NestRun run) {
        (double Coordinate, double Start, double End) trim = span.Axis.Trim(placement);
        return Math.Abs(span.CoordinateMm - trim.Coordinate) <= run.ResolutionMm
            && Math.Abs(span.StartMm - trim.Start) <= run.ResolutionMm
            && Math.Abs(span.EndMm - trim.End) <= run.ResolutionMm
            && Math.Abs(span.KerfMm - run.KerfMm) <= run.ResolutionMm;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record EligibilityNode {
    private EligibilityNode() { }
    public sealed record Part(PartInstance Value) : EligibilityNode;
    public sealed record Orientation(PartInstance Value, double Rotation) : EligibilityNode;
    public sealed record Stock(int Index) : EligibilityNode;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class StockNest {
    public static Fin<NestPlan> Pack(NestRun run) =>
        from frames in Frames(run)
        from eligibility in EligibilityGraph.Admit(run.Parts, frames, run.KerfMm)
        let orientations = Orientations(run.Parts, run.Budget.OrientationBudget, run.ResolutionMm, run.KerfMm)
        from candidates in Evaluate(run, frames, eligibility, orientations)
        from best in candidates.Fold(Option<ProviderRun>.None, (held, row) => held
                .Filter(seat => (-seat.Placements.Count,
                        -TrueArea(seat, run.Parts) / UsedStockArea(seat, frames), UsedCost(seat, frames))
                    .CompareTo((-row.Placements.Count,
                        -TrueArea(row, run.Parts) / UsedStockArea(row, frames), UsedCost(row, frames))) <= 0)
                .IfNone(row))
            .ToFin(new FabricationFault.StockOverflow(run.Parts.Count, frames.Count).ToError())
        from proved in VerifyProvider(best, frames)
        from plan in Project(run, frames, proved)
        select plan;

    // Rectangular admission precedes the stock budget: taking first would spend the budget on frames the packer rejects.
    internal static Fin<Seq<StockFrame>> Frames(NestRun run) {
        Seq<StockFrame> frames = run.Inventory.Map((stock, index) => (stock, index))
            .Choose(row => Frame(row.stock, row.index, run.EdgeAllowanceMm, run.ResolutionMm))
            .Take(run.Budget.StockLimit).Map((frame, index) => frame with { Index = index }).ToSeq();
        return frames.IsEmpty
            ? Fin.Fail<Seq<StockFrame>>(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:no-rectangular-inventory"))
            : Fin.Succ(frames);
    }

    internal static int Bound(NestRun run, Seq<StockFrame> frames) =>
        frames.Map(static frame => frame.TrueArea).Fold(0.0, Math.Max) is double largest && largest > 0.0
            ? (int)Math.Ceiling(run.Parts.Sum(static part => part.TrueAreaMm2) / largest)
            : run.Parts.Count;

    internal static Option<StockFrame> Frame(Stock stock, int index, double allowance, double resolution) {
        if (!stock.Nestable || stock.Exclusions.Count > 0 || stock.Region.Count != 1) return None;
        return stock.Region.Head.Bind(region => {
            BoundingBox box = region.Bound();
            double width = box.Max.X - box.Min.X - (2.0 * allowance), height = box.Max.Y - box.Min.Y - (2.0 * allowance);
            if (!FitsExtent(stock, box) || width <= 0.0 || height <= 0.0
                || Math.Abs(Math.Abs(region.Area()) - ((box.Max.X - box.Min.X) * (box.Max.Y - box.Min.Y)))
                > region.Tolerance.Absolute.Value * Math.Max(box.Max.X - box.Min.X, box.Max.Y - box.Min.Y)) return None;
            double columnCount = Math.Floor(width / resolution), rowCount = Math.Floor(height / resolution);
            return columnCount is < 1.0 or > int.MaxValue || rowCount is < 1.0 or > int.MaxValue
                ? None
                : Some(new StockFrame(index, stock, box.Min.X + allowance, box.Min.Y + allowance,
                    (int)columnCount, (int)rowCount, resolution, Math.Abs(region.Area()), stock.Material, stock.GrainAxis, stock.Cost));
        });
    }

    static bool FitsExtent(Stock stock, BoundingBox bounds) {
        double width = bounds.Max.X - bounds.Min.X, height = bounds.Max.Y - bounds.Min.Y;
        return stock.Switch(
            sheet: static _ => true, plate: static _ => true,
            roll: row => width <= row.Width && height <= row.AvailableLength,
            coil: row => width <= row.Width && height <= row.AvailableLength,
            barStock: static _ => false, tubeStock: static _ => false,
            billet: static _ => true, filament: static _ => false, fromRemnant: static _ => true);
    }

    // Exact mixed-radix enumeration while the assignment space fits the budget; beyond it a decorrelated per-part draw,
    // because a saturating stride freezes every part past the prefix at its first rotation and samples one corner only.
    static Seq<Seq<OrientedPart>> Orientations(Seq<CutPart> parts, int budget, double resolution, double kerf) {
        Seq<long> strides = parts.Fold(Seq(1L), static (acc, part) =>
            acc.Add(Saturated(acc.Last.IfNone(1L), part.Rotations.Count))).Init;
        long space = Saturated(strides.Last.IfNone(1L), parts.Last.Map(static part => part.Rotations.Count).IfNone(1));
        bool exhaustive = space <= budget;
        return toSeq(Enumerable.Range(0, exhaustive ? (int)space : budget))
            .Map(seed => toSeq(parts.Map((part, index) => Oriented(part, resolution, kerf, part.Rotations[
                (int)((exhaustive
                    ? (ulong)seed / (ulong)strides[index]
                    : (ulong)Rotation(seed, index, part.Rotations.Count)) % (ulong)part.Rotations.Count)]))
                .OrderByDescending(static row => (long)row.Width * row.Height)))
            .Distinct();
    }

    static OrientedPart Oriented(CutPart part, double resolution, double kerf, double rotation) =>
        Envelope(part, resolution, kerf, rotation).Apply(extent => new OrientedPart(
            new PartInstance(part.PartId, part.Instance), part, extent.Width, extent.Height, rotation));

    internal static (int Width, int Height) Envelope(CutPart part, double resolution, double kerf, double rotation) {
        double cosine = Math.Abs(Math.Cos(rotation));
        double sine = Math.Abs(Math.Sin(rotation));
        return (
            checked((int)Math.Ceiling(((part.WidthMm * cosine) + (part.HeightMm * sine) + kerf) / resolution)),
            checked((int)Math.Ceiling(((part.WidthMm * sine) + (part.HeightMm * cosine) + kerf) / resolution)));
    }

    static long Saturated(long prior, int radix) =>
        radix < 1 || prior > long.MaxValue / radix ? long.MaxValue : prior * radix;

    // Beyond the exhaustive budget the assignment is DRAWN, and a raw stream modulo would bias a non-power-of-two
    // rotation count toward its low rows; one threaded state through the kernel's rejection draw removes it.
    static int Rotation(int seed, int index, int count) {
        ulong stream = Deterministic.Stream(lanes: [seed, index]);
        return Deterministic.NextBelow(ref stream, count);
    }

    static Fin<Seq<ProviderRun>> Evaluate(NestRun run, Seq<StockFrame> frames, EligibilityGraph eligibility,
        Seq<Seq<OrientedPart>> orientations) {
        NestStrategy[] strategies = run.Strategies.ToArray();
        Seq<OrientedPart>[] assignments = orientations.ToArray();
        Fin<Option<ProviderRun>>[] results = new Fin<Option<ProviderRun>>[checked(strategies.Length * assignments.Length)];
        EvaluationAction action = new(strategies, assignments, frames, eligibility, results);
        ParallelHelper.For2D(0..strategies.Length, 0..assignments.Length, in action, run.Budget.BatchFloor);
        return results.ToSeq().TraverseM(identity).As()
            .Map(static attempts => attempts.Choose(identity).Filter(static row => row.Placements.Count > 0))
            .Bind(found => !found.IsEmpty
                ? Fin.Succ(found)
                : Fin.Fail<Seq<ProviderRun>>(new FabricationFault.StockOverflow(run.Parts.Count, frames.Count).ToError()));
    }

    // Each arm supplies only what its provider type cannot share: how to construct the packer, how to insert a
    // part, and which of its own geometry the sheet receipt retains. The drive fold, the option lift, and the
    // sheet-shape trio are hoisted, so an arm is one expression and a new provider adds no fold.
    static Fin<Option<ProviderRun>> Evaluate(NestStrategy strategy, Seq<OrientedPart> parts, Seq<StockFrame> frames,
        EligibilityGraph eligibility) =>
        strategy.Switch(
            maxRects: row => Drive(strategy, parts, frames, eligibility,
                static frame => new MaxRectsBinPack(frame.Width, frame.Height, allowRotations: false),
                (packer, part) => packer.Insert(part.Width, part.Height, row.Choice.Native),
                static (packer, frame) => new ProviderSheet(
                    frame.Index,
                    Some(packer.FreeRectangles.ToSeq()),
                    // The used list belongs to the frame that constructed the packer; a resized bin publishes a
                    // roster addressing extents this frame never had.
                    packer.BinWidth == frame.Width && packer.BinHeight == frame.Height
                        ? Some(packer.UsedRectangles.ToSeq())
                        : None)),
            skyline: row => Drive(strategy, parts, frames, eligibility,
                frame => new SkylineBinPack(frame.Width, frame.Height, row.Waste.Native),
                (packer, part) => packer.Insert(part.Width, part.Height, row.Level.Native),
                Opaque<SkylineBinPack>()),
            guillotine: row => Drive(strategy, parts, frames, eligibility,
                static frame => new GuillotineBinPack(frame.Width, frame.Height),
                (packer, part) => packer.Insert(part.Width, part.Height, row.Merge.Native, row.Choice.Native, row.Split.Native),
                static (packer, frame) => {
                    packer.MergeFreeRectangles();
                    return new ProviderSheet(frame.Index, Some(packer.FreeRectangles.ToSeq()), Some(packer.UsedRectangles.ToSeq()));
                }),
            shelf: row => Drive(strategy, parts, frames, eligibility,
                frame => new ShelfBinPack(frame.Width, frame.Height, row.Waste.Native),
                (packer, part) => packer.Insert(part.Width, part.Height, row.Choice.Native),
                Opaque<ShelfBinPack>()),
            massCut: _ => MassCut(parts, frames, eligibility),
            sweep: static _ => Fin.Fail<Option<ProviderRun>>(
                new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:nested-sweep")));

    // A packer publishing neither its free nor its used geometry retains its frame index alone; the receipt states
    // the absence rather than inventing an empty roster a validator would read as a proved complement.
    static Func<TPacker, StockFrame, ProviderSheet> Opaque<TPacker>() where TPacker : class =>
        static (_, frame) => new ProviderSheet(frame.Index, None, None);

    static Fin<Option<ProviderRun>> Drive<TPacker>(NestStrategy strategy, Seq<OrientedPart> parts, Seq<StockFrame> frames,
        EligibilityGraph eligibility, Func<StockFrame, TPacker> create, Func<TPacker, OrientedPart, Rect> insert,
        Func<TPacker, StockFrame, ProviderSheet> state) where TPacker : class {
        TPacker[] packers = frames.Map(create).ToArray();
        // Each Insert mutates its packer, so the frame walk short-circuits on the first fit; a lazy filter-map would
        // insert the part into every eligible packer and keep one, silently consuming free area in all the others.
        Seq<ProviderPlacement> placements = parts.Fold(Seq<ProviderPlacement>(), (accepted, part) => frames
            .Filter(frame => eligibility.Allows(part.Instance, part.Rotation, frame.Index))
            .Fold(Option<ProviderPlacement>.None, (found, frame) => found.IsSome
                ? found
                : insert(packers[frame.Index], part) is Rect rect && rect.Height != 0
                    ? Some(new ProviderPlacement(part.Instance, frame.Index, rect, part.Rotation))
                    : found)
            .Map(placement => accepted.Add(placement)).IfNone(accepted));
        return Fin.Succ(Some(new ProviderRun(strategy, placements, frames.Map(frame => state(packers[frame.Index], frame)))));
    }

    static Fin<Option<ProviderRun>> MassCut(Seq<OrientedPart> parts, Seq<StockFrame> frames, EligibilityGraph eligibility) {
        Seq<(int Width, int Height, Option<MaterialId> Material, Option<double> Grain, double Rotation, double AngularTolerance)> extents = parts
            .Map(static row => (row.Width, row.Height, row.Source.Material, row.Source.GrainAxis, row.Rotation,
                row.Source.AngularTolerance)).Distinct();
        if (extents.Count != 1) return Fin.Succ(Option<ProviderRun>.None);
        return parts.Head.ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:mass-cut-empty")).Bind(prototype => {
            (Seq<ProviderPlacement> Placements, Seq<OrientedPart> Remaining) packedRun = frames.Fold(
                (Placements: Seq<ProviderPlacement>(), Remaining: parts), (state, frame) => {
                Seq<OrientedPart> eligible = state.Remaining
                    .Filter(part => eligibility.Allows(part.Instance, part.Rotation, frame.Index));
                if (eligible.IsEmpty) return state;
                Seq<Rect> packed = toSeq(new SingleBinPack(frame.Width, frame.Height)
                    .Insert(prototype.Width, prototype.Height, eligible.Count));
                Seq<OrientedPart> placed = eligible.Take(packed.Count);
                FrozenSet<PartInstance> assigned = placed.Map(static part => part.Instance).ToFrozenSet();
                return (state.Placements.Concat(packed.Map((rect, offset) => new ProviderPlacement(
                    placed[offset].Instance, frame.Index, rect, placed[offset].Rotation))),
                    state.Remaining.Filter(part => !assigned.Contains(part.Instance)));
            });
            return Fin.Succ(Some(new ProviderRun(new NestStrategy.MassCut(), packedRun.Placements, Seq<ProviderSheet>())));
        });
    }

    static Fin<NestPlan> Project(NestRun run, Seq<StockFrame> frames, ProviderRun provider) {
        Seq<NestPlacement> placements = provider.Placements.Map(row => {
            StockFrame frame = frames[row.StockIndex];
            return new NestPlacement(row.Instance.PartId, row.Instance.Ordinal, row.StockIndex,
                frame.OriginX + (row.Rect.X * run.ResolutionMm), frame.OriginY + (row.Rect.Y * run.ResolutionMm),
                row.Rotation, row.Rect.Width * run.ResolutionMm, row.Rect.Height * run.ResolutionMm);
        });
        Seq<int> used = provider.Placements.Map(static row => row.StockIndex).Distinct();
        NestYield yield = YieldOf(run, frames, placements, provider.Sheets);
        Seq<CutPattern> patterns = used.Map(index => Pattern(index, provider,
            placements.Filter(row => row.SheetIndex == index), frames[index], run.ResolutionMm, run.KerfMm));
        Seq<Stock> stock = frames.Map(static row => row.Source);
        ContentKey evidence = Digest(run, stock, placements, patterns, yield);
        return NestPlan.Validate(run, stock, placements, patterns, yield, evidence, out NestPlan plan).Admitted(plan);
    }

    // Each packer holds the placed set it mutated, so where every used sheet publishes one the rectangle area is
    // READ rather than re-accumulated from the projected footprints; the placement derivation stays the
    // independent recomputation plan admission proves the receipt against.
    internal static NestYield YieldOf(NestRun run, Seq<StockFrame> frames, Seq<NestPlacement> placements,
        Seq<ProviderSheet> sheets) {
        FrozenDictionary<PartInstance, CutPart> parts = run.Parts
            .ToFrozenDictionary(static row => new PartInstance(row.PartId, row.Instance));
        Seq<int> used = placements.Map(static row => row.SheetIndex).Distinct();
        double trueArea = placements.Sum(row => parts[new PartInstance(row.PartId, row.Instance)].TrueAreaMm2);
        double resolutionArea = run.ResolutionMm * run.ResolutionMm;
        Seq<ProviderSheet> placedSheets = sheets.Filter(sheet => used.Contains(sheet.Stock) && sheet.Placed.IsSome);
        double rectangleArea = placedSheets.Count == used.Count
            ? placedSheets.Sum(static sheet => sheet.Placed.Map(static rows => rows.Sum(static rect => (double)rect.Area)).IfNone(0.0)) * resolutionArea
            : placements.Sum(static row => row.WidthMm * row.HeightMm);
        double stockArea = used.Sum(index => frames[index].TrueArea);
        return new NestYield(
            run.Parts.Count,
            placements.Count,
            run.Parts.Count - placements.Count,
            used.Count,
            trueArea,
            rectangleArea,
            stockArea,
            stockArea > 0.0 ? trueArea / stockArea : 0.0,
            stockArea - rectangleArea,
            used.Sum(index => frames[index].Cost),
            Bound(run, frames));
    }

    static Fin<ProviderRun> VerifyProvider(ProviderRun provider, Seq<StockFrame> frames) {
        bool contained = provider.Placements.ForAll(row => frames[row.StockIndex].Bounds.Contains(row.Rect));
        bool disjoint = toSeq(provider.Placements.GroupBy(static row => row.StockIndex))
            .Map(static group => toSeq(group).Map(static row => row.Rect))
            .ForAll(static rects => rects.Map((left, index) => rects.Skip(index + 1)
                .ForAll(right => !left.Intersects(right))).ForAll(identity));
        return contained && disjoint
            ? Fin.Succ(provider)
            : Fin.Fail<ProviderRun>(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:provider-proof"));
    }

    static CutPattern Pattern(int stock, ProviderRun provider, Seq<NestPlacement> sheet, StockFrame frame,
        double resolution, double kerf) {
        Seq<CutSpan> spans = sheet.Map((row, placementIndex) => toSeq(CutAxis.Items).Map(axis => {
            (double Coordinate, double Start, double End) trim = axis.Trim(row);
            return new CutSpan(placementIndex, axis, trim.Coordinate, trim.Start, trim.End, kerf);
        })).Bind(identity);
        Option<Seq<(double, double, double, double)>> free = provider.Sheets
            .Find(row => row.Stock == stock)
            .Bind(static row => row.Free)
            .Map(rows => rows.Map(rect => (frame.OriginX + (rect.X * resolution), frame.OriginY + (rect.Y * resolution),
                rect.Width * resolution, rect.Height * resolution)));
        return new CutPattern(stock, provider.Strategy, provider.Strategy.Proof, spans, free);
    }

    static double TrueArea(ProviderRun run, Seq<CutPart> parts) {
        FrozenDictionary<PartInstance, CutPart> index = parts.ToFrozenDictionary(static row => new PartInstance(row.PartId, row.Instance));
        return run.Placements.Sum(row => index[row.Instance].TrueAreaMm2);
    }

    static double UsedStockArea(ProviderRun run, Seq<StockFrame> frames) => run.Placements.Map(static row => row.StockIndex)
        .Distinct().Sum(index => frames[index].TrueArea);
    static double UsedCost(ProviderRun run, Seq<StockFrame> frames) => run.Placements.Map(static row => row.StockIndex)
        .Distinct().Sum(index => frames[index].Cost);

    // The ONE byte codec is the `Rasm.Element` `CanonicalWriter` INSTANCE: it folds `-0.0`, collapses every NaN
    // payload to one pattern, frames each string by its UTF-8 byte count, and writes a presence bit before an
    // optional value, so a rectangular digest and a true-shape digest can never fork on framing. Field ORDER is
    // the page's and stays; the static twin this page once composed is deleted, not aliased. Unit identity rides
    // the same freeze: every digested scalar is a bare millimetre-basis double named by its own `Mm`/`Mm2` suffix,
    // and lifting one to a typed quantity moves the preimage bytes — forking every `StockSnapshot` key already
    // minted and the `NestYield` projection `Rasm.Compute` decodes off it.
    // Every preimage composes `FabricationCanon` over the ONE `Rasm.Element` `CanonicalWriter`: `Rows` writes the
    // count ahead of its rows, `Maybe` frames absence as a presence bit ahead of the payload, and `Double`
    // normalizes `-0.0` and every NaN pattern — so a hand-counted collection prologue, a sentinel ordinal for an
    // absent free-rectangle list, and a page-local presence writer are all the deleted form. Field ORDER is this
    // page's and stays; only the framing moves onto the codec.
    internal static ContentKey Digest(NestRun run, Seq<Stock> stock, Seq<NestPlacement> placements,
        Seq<CutPattern> patterns, NestYield yield) =>
        ContentKey.Of(EgressKind.StockSnapshot, new CanonicalWriter(run.ResolutionMm)
            .U128(run.Strategy.Identity)
            .Ordinal(run.Budget.StrategyBudget).Ordinal(run.Budget.StrategyDepth)
            .Rows(run.Strategies, static (held, strategy) => held.U128(strategy.Identity))
            .Ordinal(run.Budget.StockLimit).Ordinal(run.Budget.OrientationBudget).Ordinal(run.Budget.BatchFloor)
            .Double(run.Grid.ResolutionMm).Double(run.Grid.KerfMm).Double(run.Grid.EdgeAllowanceMm)
            .Rows(run.Inventory, static (held, item) => held.U128(item.Identity))
            .Rows(run.Parts, static (held, part) => held
                .Ordinal(part.PartId).Ordinal(part.Instance)
                .Double(part.WidthMm).Double(part.HeightMm).Double(part.TrueAreaMm2)
                .Double(part.LinearTolerance).Double(part.AngularTolerance)
                .Maybe(part.Material, static (row, material) => row.String(material.Value))
                .Maybe(part.GrainAxis, static (row, grain) => row.Double(grain))
                .Rows(part.Rotations, static (row, rotation) => row.Double(rotation)))
            .Rows(toSeq(placements.OrderBy(static row => row.SheetIndex).ThenBy(static row => row.PartId)
                    .ThenBy(static row => row.Instance)),
                static (held, row) => held
                    .Ordinal(row.PartId).Ordinal(row.Instance).Ordinal(row.SheetIndex)
                    .Double(row.XMm).Double(row.YMm).Double(row.RotationRadians)
                    .Double(row.WidthMm).Double(row.HeightMm))
            .Rows(stock, static (held, item) => held.U128(item.Identity))
            .Rows(toSeq(patterns.OrderBy(static row => row.SheetIndex)), static (held, pattern) => held
                .Ordinal(pattern.SheetIndex).U128(pattern.Strategy.Identity).Ordinal(pattern.Proof.Ordinal)
                .Rows(toSeq(pattern.Spans.OrderBy(static row => row.PlacementIndex).ThenBy(static row => row.Axis.Ordinal)),
                    static (row, span) => row
                        .Ordinal(span.PlacementIndex).Ordinal(span.Axis.Ordinal)
                        .Double(span.CoordinateMm).Double(span.StartMm).Double(span.EndMm).Double(span.KerfMm))
                .Maybe(pattern.Free, static (row, rows) => row.Rows(
                    toSeq(rows.OrderBy(static free => free.X).ThenBy(static free => free.Y)
                        .ThenBy(static free => free.Width).ThenBy(static free => free.Height)),
                    static (leaf, free) => leaf.Double(free.X).Double(free.Y).Double(free.Width).Double(free.Height))))
            .Ordinal(yield.RequestedCount).Ordinal(yield.PlacedCount).Ordinal(yield.UnplacedCount).Ordinal(yield.SheetCount)
            .Double(yield.TruePartAreaMm2).Double(yield.RectangleAreaMm2).Double(yield.StockAreaMm2)
            .Double(yield.UtilizationRatio).Double(yield.WasteAreaMm2).Double(yield.StockCost)
            .Ordinal(yield.SheetLowerBound)
            .ToBytes().Span);

    // A strategy key digests a closed row vocabulary and writes no `Measure`, so the writer's quantization grid
    // never reaches the bytes; the anchor names that inertness rather than borrowing a run's geometry grid.
    const double MeasureFreeGrid = 1.0;

    internal static UInt128 StrategyKey(NestStrategy strategy) =>
        ContentKey.Of(
            EgressKind.StockSnapshot,
            WriteStrategy(new CanonicalWriter(MeasureFreeGrid).String(nameof(NestStrategy)), strategy).ToBytes().Span).Digest;

    // A strategy preimage is its own case token then its own closed rows: `Discriminant` frames each generated key
    // length-prefixed, so a provider heuristic reordering its enum can never re-key a plan.
    static CanonicalWriter WriteStrategy(CanonicalWriter writer, NestStrategy strategy) => strategy.Switch(
        state: writer,
        maxRects: static (held, row) => held.String(nameof(NestStrategy.MaxRects)).Discriminant(row.Choice),
        skyline: static (held, row) => held.String(nameof(NestStrategy.Skyline))
            .Discriminant(row.Level).Discriminant(row.Waste),
        guillotine: static (held, row) => held.String(nameof(NestStrategy.Guillotine))
            .Discriminant(row.Merge).Discriminant(row.Choice).Discriminant(row.Split),
        shelf: static (held, row) => held.String(nameof(NestStrategy.Shelf))
            .Discriminant(row.Choice).Discriminant(row.Waste),
        massCut: static (held, _) => held.String(nameof(NestStrategy.MassCut)),
        sweep: static (held, row) => held.String(nameof(NestStrategy.Sweep))
            .Rows(row.Cases, static (leaf, item) => WriteStrategy(leaf, item)));

    readonly struct EvaluationAction(NestStrategy[] strategies, Seq<OrientedPart>[] assignments, Seq<StockFrame> frames,
        EligibilityGraph eligibility, Fin<Option<ProviderRun>>[] results) : IAction2D {
        public void Invoke(int i, int j) => results[(i * assignments.Length) + j] =
            Try.lift<Fin<Option<ProviderRun>>>(f: () => Evaluate(strategies[i], assignments[j], frames, eligibility)).Run()
                .MapFail(error => new GeometryFault.DegenerateInput(Kind.Polyline, None, "stock:provider").ToError() + error).Bind(identity);
    }
}

// Connected components over the bipartite eligibility relation are independent cutting sub-problems, so a component whose
// part-area demand exceeds its own reachable stock supply is provably infeasible before any provider runs.
internal sealed class EligibilityGraph {
    readonly UndirectedGraph<EligibilityNode, SEdge<EligibilityNode>> graph;
    readonly HashMap<EligibilityNode, int> components;

    EligibilityGraph(UndirectedGraph<EligibilityNode, SEdge<EligibilityNode>> graph,
        HashMap<EligibilityNode, int> components) => (this.graph, this.components) = (graph, components);

    public static Fin<EligibilityGraph> Admit(Seq<CutPart> parts, Seq<StockFrame> frames, double kerf) {
        UndirectedGraph<EligibilityNode, SEdge<EligibilityNode>> graph = new(allowParallelEdges: false);
        Seq<EligibilityNode.Part> partNodes = parts.Map(row => new EligibilityNode.Part(new PartInstance(row.PartId, row.Instance)));
        Seq<EligibilityNode.Orientation> orientationNodes = parts.Bind(part => part.Rotations.Map(rotation =>
            new EligibilityNode.Orientation(new PartInstance(part.PartId, part.Instance), rotation)));
        graph.AddVertexRange(partNodes.Cast<EligibilityNode>());
        graph.AddVertexRange(orientationNodes.Cast<EligibilityNode>());
        graph.AddVertexRange(frames.Map(static row => new EligibilityNode.Stock(row.Index)).Cast<EligibilityNode>());
        parts.Iter(part => part.Rotations.Iter(rotation => {
            PartInstance instance = new(part.PartId, part.Instance);
            EligibilityNode.Part partNode = new(instance);
            EligibilityNode.Orientation orientationNode = new(instance, rotation);
            graph.AddEdge(new SEdge<EligibilityNode>(partNode, orientationNode));
            frames.Filter(frame => Fits(part, frame, kerf, rotation)).Iter(frame =>
                graph.AddEdge(new SEdge<EligibilityNode>(orientationNode, new EligibilityNode.Stock(frame.Index))));
        }));
        // An orphan is a part whose every orientation reaches no frame — the graph's own degree answers it, so the
        // cross-product of parts against orientations this replaces re-derived membership the container already holds.
        if (orientationNodes.GroupBy(static row => row.Value)
            .Any(group => group.All(orientation => graph.AdjacentDegree(orientation) == 1)))
            return Fin.Fail<EligibilityGraph>(new FabricationFault.PolicyInadmissible(FabConcern.Nesting, "stock:orphan-part"));
        Dictionary<EligibilityNode, int> labels = new();
        _ = graph.ConnectedComponents(labels);
        HashMap<EligibilityNode, int> components = toHashMap(toSeq(labels).Map(static row => (row.Key, row.Value)));
        return Starved(parts, frames, components).Match(
            Some: count => Fin.Fail<EligibilityGraph>(new FabricationFault.StockOverflow(count, frames.Count).ToError()),
            None: () => Fin.Succ(new EligibilityGraph(graph, components)));
    }

    public bool Allows(PartInstance part, double rotation, int stock) =>
        graph.TryGetEdge(new EligibilityNode.Orientation(part, rotation), new EligibilityNode.Stock(stock), out _);

    // Each component is an independent cutting sub-problem, so its own part-area demand against its own reachable
    // stock supply is the admissible feasibility relaxation — Hall's condition by AREA over a many-to-one relation.
    // `MaximumBipartiteMatchingAlgorithm` is REFUSED by name here: a matching saturates a one-to-one assignment, and
    // one frame carries many parts, so a perfect matching neither exists nor implies feasibility on this relation;
    // the area bound is exact where a matching would be vacuous. Supply indexes by component ONCE, so the frame
    // roster is walked once rather than once per component.
    static Option<int> Starved(Seq<CutPart> parts, Seq<StockFrame> frames, HashMap<EligibilityNode, int> components) {
        Map<int, double> supply = frames.Fold(Map<int, double>(), (index, frame) =>
            index.AddOrUpdate(components.Find(new EligibilityNode.Stock(frame.Index)).IfNone(-1),
                held => held + frame.TrueArea, () => frame.TrueArea));
        return toSeq(parts
                .Map(part => (Label: components
                    .Find(new EligibilityNode.Part(new PartInstance(part.PartId, part.Instance))).IfNone(-1), Part: part))
                .GroupBy(static row => row.Label))
            .Map(static group => toSeq(group))
            .Choose(rows => rows.Head.Map(head => (
                head.Label,
                Demand: rows.Sum(static row => row.Part.TrueAreaMm2),
                Supply: supply.Find(head.Label).IfNone(0.0),
                rows.Count)))
            .Find(static group => group.Demand > group.Supply)
            .Map(static group => group.Count);
    }

    static bool Fits(CutPart part, StockFrame stock, double kerf, double rotation) {
        (int Width, int Height) extent = StockNest.Envelope(part, stock.Resolution, kerf, rotation);
        return part.Material.ForAll(material => material == stock.Material)
            && extent.Width <= stock.Width && extent.Height <= stock.Height
            && part.GrainAxis.ForAll(grain => stock.GrainAxis.Exists(axis =>
                Math.Abs(Math.IEEERemainder((grain + rotation) - axis, Math.PI)) <= part.AngularTolerance));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
