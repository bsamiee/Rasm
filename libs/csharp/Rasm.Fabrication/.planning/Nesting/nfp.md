# [RASM_FABRICATION_NFP]

The 2D true-shape nesting owner: `Nest` the static placement fold packing part outlines across a `Seq<Stock>` inventory through a no-fit-polygon (NFP) feasibility test, with a bottom-left greedy, a genetic placement heuristic, and an axis-aligned-rectangle `RectpackSharp` fast-path all over the same NFP feasibility set, spilling parts that do not fit one stock onto the next and minting the leftover of each consumed stock as a content-keyed lineage-tracked `Remnant`. The NFP construction routes the `Polygon/clipper#POLYGON_ALGEBRA` `MinkowskiSum` — the Minkowski sum of the fixed part with the reflected orbiting part — never a hand-rolled angle-sorted edge merge; Clipper2 owns the polygon construction at integer-robust precision, and the irregular/non-convex NFP (convex decomposition + per-piece Minkowski union) is one arm on the same Geometry2D owner. The inner-fit-polygon containment locus — whether a part lies fully INSIDE an irregular container — is the dual `NoFitPolygon.InnerFit` factory over the settled `PolygonAlgebra.MinkowskiDiff` primitive, the exact non-rectangular `Stock.Contains` arm the `FromRemnant`/`Plate` cases read. The stock the parts pack onto is the `Stock` `[Union]` (`Sheet`/`Plate`/`BarStock`/`TubeStock`/`Billet`/`Filament`/`FromRemnant`) — one closed family with a single `Contains`/`Area`/`Of` total fold the feasibility kernel reads, collapsing the old virgin-rectangle-vs-remnant `Option<Remnant>` discriminant and the new plate/bar/tube/billet/filament cases into one owner: a planar `Sheet`/`Plate`/`Billet` packs in its 2D bounds (the `RectpackSharp` maxrects/guillotine fast-path the panel-saw and flatbed cases route through), a `BarStock`/`TubeStock` packs onto its 1-axis-revolved envelope (the unrolled circumference × length), the `Filament` is the additive feedstock budget, and a `FromRemnant`/non-rectangular case routes the exact `Remnant.Holds`/`InnerFit` IFP containment. The feasibility verdict — whether a reference point lies inside or outside the NFP — stays the kernel `Rasm/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact point-in-polygon sign. The bottom-left, genetic, and rect-fastpath modes are ONE fold over the `NestPolicy` placement discriminant, never three packer classes; `RectpackSharp` admits ONLY as the axis-aligned-rectangle arm coexisting with the NFP true-shape kernel and never displaces the `BarStock`/`TubeStock`/`Remnant` cases. When a sibling `Nesting/stock#STOCK_NEST` `StockNest.Pack` cutting-stock solve has already resolved a rectangular sheet-goods layout, the `Nest.Honor` fold CONSUMES that `NestPlan` directly on `FabricationInput.Plan` and maps each `NestPlacement` straight to a `PartTransform` — the stock owner keeps the rectangular cutting-stock YIELD (the procurement/sustainability concern: minimum sheets, offcut remnants, embodied-carbon waste) and this owner the true-shape irregular NEST (the CAM concern: cut-program transforms), so the `rect-fastpath` `RectpackSharp` arm is the from-scratch degenerate-AABB nest for parts arriving WITHOUT a material plan and never a second rectangular cutting-stock packer beside the stock owner; the seam is in-package and direct, neither duplicating the other. A DRL-guided placement policy is a `NestPolicy` column carrying an injected `Func<NoFitPolygon, PartTransform, double>` placement-score delegate (defaulted to the heuristic lowest-then-leftmost score) the app-platform consumer wires from the `Rasm.Compute/Model/inference#INFERENCE_MODES` ONNX lane (Fabrication is AEC-domain and never references the app-platform `Rasm.Compute`; the strata law forbids the downward edge), the NFP primitive unchanged. The kernel composes the `Process/owner#FABRICATION_OWNER` `Loop`/`PartTransform`/`FabricationPolicy.Nest`/`FabricationResult.Placement` shared vocabulary; it computes no hash beyond the `System.IO.Hashing` `XxHash128` content identity and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `Placement` transforms cross only the in-process seam to the `Posting/program#CUT_PROGRAM` emitter — never a browser or peer wire. The `Stock`/`NestPolicy`/`NoFitPolygon` records are host-local types that never sit between wire and rail; the `NestPlan` on `FabricationInput.Plan` is IN-PACKAGE data from `Nesting/stock#STOCK_NEST` — admitted ONCE at the `Nest.Solve` plan-honor boundary and mapped straight to `PartTransform`, re-validated nowhere in the interior; no wire mirror exists.

## [01]-[INDEX]

- [01]-[NESTING]: owns the `Stock` union, the `NestPolicy`/`NoFitPolygon` records, and the `Nest` fold — no-fit-polygon feasibility (Minkowski via Geometry2D) and inner-fit-polygon containment (MinkowskiDiff) with the bottom-left, genetic, and `RectpackSharp` rect-fastpath placement modes over the one feasibility set, the multi-sheet `Seq<Stock>` scheduler spilling parts forward, the kerf-inflated Boolean-difference `Remnant` lineage producer, the injected placement-score delegate slot, and the `Nest.Honor` consumption of a pre-resolved sibling `NestPlan` (the `Nesting/stock#STOCK_NEST` rectangular cutting-stock yield) the plan-present path maps straight to `PartTransform` rather than re-packing.

## [02]-[NESTING]

- Owner: `Remnant` the leftover-stock polygon carrying its boundary `Loop`, its `XxHash128`-derived `UInt128` content identity, an `Option<UInt128>` `Parent` lineage column (the consumed-stock identity a difference-minted child descends from, `None` for a virgin remnant), the `Holds` exact point-in-polygon containment, the `Of` content-address mint over the kernel `System.IO.Hashing` `XxHash128`, and the `From` kerf-inflated Boolean-difference fold minting each leftover region as a content-keyed lineage-stamped child; `Stock` `[Union]` the stock the parts pack onto — `Sheet` (virgin rectangle) · `Plate` (thick rectangle with a cut-depth column) · `BarStock`/`TubeStock` (a revolved envelope unrolled to its circumference × length planar bounds) · `Billet` (a solid block) · `Filament` (additive feedstock length) · `FromRemnant` (wrapping the content-keyed leftover `Remnant` polygon) — with one `Contains`/`Area`/`Of`/`Planar` total fold every feasibility check reads, the `Switch` discriminating the planar rectangle bounds (the `RectpackSharp` fast-path eligible cases the `Planar` projection flags) from the revolved-envelope bounds from the remnant `Holds`/`InnerFit` polygon containment, never a parallel per-stock packer; `NestPolicy` the placement knobs (rotation step count, GA population, generations, mutation rate, the `Mode` `[SmartEnum<string>]` `nfp-true-shape`/`rect-fastpath` placement discriminant, the `Guillotine` straight-cut and `GrainDirection` constraint columns the panel-saw fast-path reads, the `Kerf` inflation width the remnant difference reads, and the `Score` placement-rank delegate); `NoFitPolygon` the sliding-locus polygon — the set of reference positions where the orbiting part touches but never overlaps the fixed part, the canonical primitive every 2D true-shape nesting heuristic reads, its outer boundary built through the Geometry2D Minkowski SUM and its `InnerFit` inner-fit-polygon locus (the dual: the exact set of reference positions where the part lies fully inside an irregular container) built through the Geometry2D Minkowski DIFFERENCE, with `PairKey` the `XxHash128` content digest over the rotation-quantized part-pair vertex span keying the precompute memo; the sibling `Nesting/stock#STOCK_NEST` `NestPlan` — the resolved rectangular layout (a `Seq<NestPlacement>` of `PartId` + sheet-index + position + rotation tuples plus the `NestYield` receipt) — the `Nest.Honor` fold consumes directly when `FabricationInput.Plan` is present; `Nest` the static placement fold building each part-pair NFP, scheduling the parts across the `Seq<Stock>` inventory sheet-by-sheet, folding the parts onto each `Stock` envelope by the bottom-left, genetic-ordered, or `RectpackSharp` rect-fastpath heuristic, or — when a pre-resolved `NestPlan` rides `FabricationInput.Plan` — honoring that rectangular layout directly through `Nest.Honor` without running a packer.
- Cases: placement modes `bottom-left` (a deterministic greedy lowest-then-leftmost feasible position per part) · `genetic` (a GA over the part ordering + rotation, the bottom-left decode scoring each chromosome by utilization) · `rect-fastpath` (the `RectpackSharp` maxrects/guillotine axis-aligned-rectangle packer over the part bounding boxes, routed only for the `Planar` `Sheet`/`Plate`/`Billet` cases under the `NestPolicy.Mode` discriminant) (3), the orthogonal plan-honor path consuming a pre-resolved sibling `NestPlan` from `FabricationInput.Plan` bypassing the packer entirely; the `Stock` union cases `Sheet` · `Plate` · `BarStock` · `TubeStock` · `Billet` · `Filament` · `FromRemnant` (7), the one `Contains`/`Area`/`Planar` fold discriminating the planar bounds (`Planar` true) from the revolved envelope from the remnant `Holds`/`InnerFit` containment (`Planar` false), the remnant re-entering the same NFP feasibility set as the next stock; the NFP outer boundary is the convex Minkowski merge through Geometry2D, the irregular/non-convex NFP (convex decomposition + per-piece union) the one widening arm on `NoFitPolygon.Of`, and the inner-fit locus the settled `MinkowskiDiff` arm on `NoFitPolygon.InnerFit`.
- Entry: `public static Fin<FabricationResult> Solve(FabricationPolicy.Nest policy, FabricationInput input)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed part outline, `FabricationFault.NoFit` when a part cannot be placed within any one stock under every rotation, and `FabricationFault.StockOverflow` when the multi-sheet spill exhausts the `Seq<Stock>` inventory with parts still unplaced, each lowered with `.ToError()`; the body builds the pairwise NFPs once, then runs the sheet-by-sheet scheduler folding each stock's placement through the bottom-left, GA, or rect-fastpath mode, spilling the unplaced parts forward, minting each consumed stock's `Remnant`, and emitting the `Placement` transforms with their `SheetIndex` partition, the cross-sheet utilization scalar, and the produced `Remnant` set.
- Auto: `NoFitPolygon.Of` reflects the orbiting part through the origin and runs the `Polygon/clipper#POLYGON_ALGEBRA` `MinkowskiSum` of the fixed part and the reflected orbiting part, taking the result boundary as the NFP outer locus; `NoFitPolygon.InnerFit` runs the dual `PolygonAlgebra.MinkowskiDiff` of the container loop and the reflected part at the fixed rotation, taking the result as the inner-fit-polygon locus the exact non-rectangular `Stock.Contains` reads (a reference point INSIDE the IFP places the part fully inside the container, the exact dual of the NFP overlap test, keyed per rotation the same way the NFP precompute is); the irregular/non-convex NFP (convex decomposition into sub-pieces, per-piece Minkowski sum, locus union) is the one widening arm on this owner over the same Geometry2D substrate. `Nest.Solve` precomputes the ordered pairwise NFPs into a frozen memo keyed by the `NoFitPolygon.PairKey` content digest — the `XxHash128` `UInt128` hash over the rotation count and the canonicalized vertex span of the ordered part-pair loops — so two part-pairs with identical geometry share one Minkowski result across the bottom-left, genetic, and across the multi-sheet fold, and the `NestPolicy.Rotations` discretization enters the key so a rotated instance hits its own digest. The MULTI-SHEET scheduler folds the `input.Inventory` `Seq<Stock>` sheet-by-sheet: it places as many parts as fit the head stock (the per-stock placement run a `bottom-left`/`genetic`/`rect-fastpath` fold over the unplaced set), stamps each placed `PartTransform.SheetIndex` with the stock's inventory index, spills the unplaced parts onto the next stock, mints the consumed stock's `Remnant` back into the inventory tail, and exhausts when the inventory empties — `StockOverflow` routing if parts remain unplaced after the last stock, never a per-sheet `Solve` call. A candidate placement is feasible when the part reference point lies OUTSIDE every already-placed part's NFP (no overlap, the NFP fetched by the part-pair digest) and `Stock.Contains` holds — for a planar `Sheet`/`Plate`/`Billet` the axis-aligned bounds, for a `BarStock`/`TubeStock` the unrolled circumference × length bounds, for a `FromRemnant`/non-rectangular case the exact `Remnant.Holds` (the part fully inside the remnant `Loop`) composing the `NoFitPolygon.InnerFit` IFP locus over the same `Orient2D` point-in-polygon — so a partially-consumed stock's remnant re-enters the feasibility set and the next nest packs onto the real leftover polygon rather than a virgin sheet. The PER-STOCK placement mode dispatches on `NestPolicy.Mode`: `nfp-true-shape` runs the `bottom-left`/`genetic` NFP fold (the irregular owner), and `rect-fastpath` — gated to `Stock.Planar` planar cases only — runs `RectpackSharp.RectanglePacker.Pack` over the part bounding boxes (`PackingRectangle` per part keyed by `PartId`, the `PackingHints.FindBest` maxrects/guillotine heuristics, the `Guillotine` column forcing `PackingHints` to the straight-cut subset for a panel-saw job, the `GrainDirection` column locking the rotation), the in-place `Pack` mutation wrapped in `Try` so the plain-`Exception` infeasibility throw the `.api` `[IN_PLACE_PACK]` law documents ("Failed to find a solution" under the `maxBounds` cap) lowers to an empty placement that spills the whole pending set forward through the multi-sheet `Consume` fold — never an uncaught exception escaping the `Fin` rail — folding the packed `X`/`Y` back to `PartTransform`, the AABB packer the dominant flatbed laser/plasma/waterjet case the GA cannot match; `bottom-left` mode folds the parts in descending-area order, sliding each to its lowest feasible NFP-boundary position scored by `NestPolicy.Score` (defaulted to the lowest-then-leftmost heuristic, overridable by the injected DRL delegate); `genetic` mode evolves a population of (order, rotation) chromosomes, decoding each through the same bottom-left placement and scoring by packed-area utilization against `Stock.Area`, the GA fold running tournament selection, order crossover, and swap mutation for `Generations` and returning the best decode. After a stock's placement, `Remnant.From` folds the kerf-inflated placed-part outlines (each outline `Offset` outward by half the `NestPolicy.Kerf`) against the stock boundary through the `Polygon/clipper#POLYGON_ALGEBRA` `Clip` `ClipOp.Difference`, takes each resulting disjoint region as a candidate `Remnant`, content-addresses it through `Remnant.Of`'s `XxHash128`, and stamps each child's `Parent` with the consumed stock's `Of()` identity so the leftover of cutting sheet A is a real lineage-tracked polygon (not a virgin rectangle) the next nest's inventory carries forward. `Remnant.Of` content-addresses the remnant boundary through the kernel `XxHash128` so a remnant is keyed by the one content identity, never a second tag. When `FabricationInput.Plan` carries a pre-resolved `NestPlan`, `Nest.Solve` SHORT-CIRCUITS the packer path entirely — `Nest.Honor` maps each `NestPlacement` to a `PartTransform` by rotating the part about the origin by the 90° flag and offsetting its rotated bounding-box min to the plan's placed lower-left (the exact dual of the `rect-fastpath` arm's bbox-min anchor, reusing the one `Transform` fold), passing the `NestYield`'s utilization and unplaced count straight into the `Placement` and minting no `Remnant` (the rectangular offcuts stay yield evidence on the `Nesting/stock` receipt), so no rectangular cutting-stock packer is duplicated beside the stock owner.
- Receipt: the `Placement` carries the per-part `PartTransform` (translation + rotation), the stock utilization fraction, and the unplaced count — the typed nesting evidence the posting emitter consumes; no generic nesting ledger.
- Packages: `Rhino.Geometry` (`Point3d`/`Vector3d`/`BoundingBox` — composed), the `Process/owner#FABRICATION_OWNER` `Loop.Covers` (point-in-polygon feasibility, composing the kernel `Predicate.Orient2D` transitively), Clipper2 (via `Polygon/clipper#POLYGON_ALGEBRA` — the NFP `MinkowskiSum`, the IFP `MinkowskiDiff`, the remnant `Clip` difference and the kerf-inflation `Offset`), `RectpackSharp` (`RectanglePacker.Pack(Span<PackingRectangle>, out PackingRectangle, PackingHints, double, uint, uint?, uint?)` — the sole `net5.0`-binding overload the consumer feeds its `PackingRectangle[]` as a span and re-reads in place — and the `PackingRectangle` `Id`/`X`/`Y`/`Width`/`Height`/`Right`/`Bottom` members — the axis-aligned-rectangle maxrects/guillotine fast-path arm, pure-managed AnyCPU zero native deps, the `Pack` infeasibility throw lowered through `LanguageExt` `Try` at the one call site, the `.api/api-rectpacksharp.md` catalogue, this folder its first admitter), `System.IO.Hashing` (`XxHash128` — the Nesting content-identity owner minting the `Stock`/`Remnant` `UInt128` content address, the `Remnant.Parent` lineage key, and the `NoFitPolygon.PairKey` precompute-memo digest), `System.Buffers.Binary` (`BinaryPrimitives`), LanguageExt.Core, BCL inbox.
- Growth: a full irregular-shape NFP with rotation search is one decomposition arm on `NoFitPolygon.Of` over the same Geometry2D Minkowski owner; the inner-fit containment for an irregular remnant is the settled `NoFitPolygon.InnerFit` `MinkowskiDiff` arm the `Stock.Contains` `FromRemnant` case reads, never a hand-rolled bounds test; a new stock kind is one `Stock` union case carrying its `Contains`/`Area`/`Planar` arm, never a second nesting owner; a new placement mode is one `NestPolicy.Mode` row plus one `PlaceStock` `Switch` arm, the `rect-fastpath` `RectpackSharp` arm coexisting with the NFP true-shape kernel; a DRL-guided placement is the `NestPolicy.Score` delegate column the app-platform consumer fills from the `Rasm.Compute/Model/inference#INFERENCE_MODES` ONNX lane (never a Fabrication-side `Rasm.Compute` reference — the AEC→app-platform edge is forbidden), the NFP and heuristic folds unchanged; the multi-sheet schedule is the one `Seq<Stock>` fold spilling parts forward, never a per-sheet `Solve`; the remnant inventory grows automatically through the `Remnant.From` difference producer the multi-sheet fold runs per consumed stock; a cross-run NFP cache is the same `PairKey` content digest the precompute already keys, never a second memo; an existing rectangular cutting-stock layout is HONORED through the `Nest.Honor` fold reading the sibling `NestPlan` on `FabricationInput.Plan`, never a second rectangular cutting-stock packer minted beside the true-shape kernel (`Nesting/stock#STOCK_NEST` owns the rectangular yield); zero new surface.
- Boundary: nesting is the ONE author-kernel placement owner and the NFP construction routes the one `Polygon/clipper#POLYGON_ALGEBRA` Minkowski owner — a hand-rolled angle-sorted edge merge is the deleted form; the NFP is the canonical placement primitive and a per-heuristic bespoke overlap test is the deleted form, every feasibility check reading the same NFP and the exact `Orient2D` inside/outside; the inner-fit containment is the `NoFitPolygon.InnerFit` `MinkowskiDiff` dual over the same one owner and a hand-rolled inner-fit bounds test is the deleted form — the IFP is the exact containment locus the non-rectangular `Stock.Contains` reads, keyed per rotation the same way the NFP is; the bottom-left, genetic, and rect-fastpath modes are ONE fold over the `NestPolicy.Mode` discriminant and a `NfpPacker`/`RectPacker` parallel packer class pair is the deleted form — `RectpackSharp` admits ONLY as the `rect-fastpath` axis-aligned-rectangle arm over the `Planar` `Sheet`/`Plate`/`Billet` cases and NEVER displaces the `BarStock`/`TubeStock`/`Remnant` true-shape cases (those route the NFP owner), the AABB packer coexisting with the irregular kernel, never replacing it, and the dropped `MaxRect`/`BinPack.NET` candidates owning the same AABB concern with no maintained edge are the rejected siblings; the `RectanglePacker.Pack` infeasibility throw is admitted at the one call site through `LanguageExt` `Try` lowering to an empty placement the multi-sheet fold spills forward and an unguarded `Pack` whose "Failed to find a solution" `Exception` escapes the `Fin` rail is the named boundary defect — the in-place packer's throw converts at this owning boundary, never the interior, exactly as the `.api` `[IN_PLACE_PACK]` law prescribes; the multi-sheet schedule is ONE `Seq<Stock>` fold spilling unplaced parts forward and a per-sheet `Solve` call (the `Map(Solve)` over the inventory) is the deleted form — the scheduler partitions the placement by `SheetIndex`, mints each consumed stock's remnant back into the inventory, and routes `StockOverflow` when the inventory exhausts with parts unplaced; the remnant is the kerf-inflated Boolean DIFFERENCE of the placed outlines from the stock through the one `Polygon/clipper#POLYGON_ALGEBRA` `Clip` `ClipOp.Difference` and an axis-aligned leftover rectangle is the deleted form — the producer is a fold over the difference's disjoint regions, each minting its own content-keyed `Remnant` stamped with the same `Parent` lineage, so the leftover of a finished nest is a real polygon the next nest carries; the stock rides the one `Stock` union with one `Contains`/`Area`/`Planar` fold and a `SheetPacker`/`BarPacker`/`RemnantPacker` per-stock packer triple is the deleted form — the `Contains` `Switch` discriminates the planar bounds from the revolved envelope from the remnant `Holds`/`InnerFit` containment over one owner, and the old `Option<Remnant>` discriminant collapses INTO the union closedness; the placement-score delegate is the one `NestPolicy.Score` column and a parallel learned-vs-heuristic packer split is the deleted form — the injected delegate ranks placements over the unchanged NFP, the heuristic score the default arm; the remnant identity AND its parent lineage are the kernel `XxHash128` content address (one federation hash, never a second tag) and the geometry domain mints no parallel digest; the NFP precompute memo is content-keyed by the `NoFitPolygon.PairKey` part-pair digest (the same `XxHash128` the `Remnant` mints, over the rotation-quantized ordered-pair vertex span) and an `(int, int)` index-tuple key is the deleted form — a repeated part shape reuses its NFP across genetic generations, stocks, and the multi-sheet fold, never rebuilding an identical Minkowski result per index pair; the point-in-polygon side test reads the shared `Process/owner#FABRICATION_OWNER` `Loop.Covers` exact-`Orient2D` containment (`Remnant.Holds` and `NoFitPolygon.Feasible`/`InnerFeasible` compose it, never a per-page re-rolled containment loop) and a naive `double` cross is the named robustness defect; the polygon area metric is the one `Polygon/clipper#POLYGON_ALGEBRA` `Area` projection — `Stock.Area` for a remnant and `Utilization` for the packed-part fraction both read `PolygonAlgebra.Area`, and a hand-inlined shoelace loop (the old `SignedArea`/`Stock.Area fromRemnant` triplicate) is the deleted form the metric owner subsumes; the `RectpackSharp` `PackingRectangle[]` is the boundary-mapped fast-path input and a `PackingRectangle`/`PackingHints` type in a sibling-kernel signature is the seam violation — the AABB packer's int-rect domain crosses to `PartTransform` at the one `rect-fastpath` arm and never travels the interior; the per-generation GA population evolution and the in-place `Crossover`/`Shuffle` chromosome scratch are the named measured-kernel statement exemption — the order-crossover and Fisher-Yates index permutation over `int[]` arrays below the dense-collection crossover, never an immutable rebuild per swap; the strata law forbids the AEC→app-platform downward edge and a `Rasm.Compute` reference in this folder is the rejected form — the DRL score crosses as a raw `double` through the injected delegate, the upstream owner never named; a second rectangular cutting-stock packer duplicating the sibling `Nesting/stock#STOCK_NEST` `StockNest`/`RectangleBinPack` yield engine is the deleted form — the `rect-fastpath` `RectpackSharp` arm is the from-scratch degenerate-AABB CAM nest (cut-program transforms from raw outlines), DISTINCT in concern from the consumed material-planning plan (the procurement yield receipt), so when a `StockNest.Pack` solve has already resolved a sheet-goods layout the `Nest.Honor` fold HONORS the `NestPlan` on `FabricationInput.Plan` (admitted once, mapped straight to `PartTransform`, re-validated nowhere) rather than re-deriving a lower-yield layout — the stock owner keeping the rectangular yield and this owner the true-shape irregular nest, neither duplicating the other.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using RectpackSharp;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PlacementMode {
    public static readonly PlacementMode NfpTrueShape = new("nfp-true-shape");
    public static readonly PlacementMode RectFastpath = new("rect-fastpath");
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Remnant(Loop Boundary, UInt128 Identity, Option<UInt128> Parent) {
    public static Remnant Of(Loop boundary, Option<UInt128> parent = default) {
        Loop ccw = boundary.AsCcw();
        var buffer = new ArrayBufferWriter<byte>();
        foreach (Point3d v in ccw.Vertices) {
            Span<byte> slot = buffer.GetSpan(16);
            BinaryPrimitives.WriteDoubleLittleEndian(slot[..8], v.X);
            BinaryPrimitives.WriteDoubleLittleEndian(slot[8..16], v.Y);
            buffer.Advance(16);
        }
        return new Remnant(ccw, XxHash128.HashToUInt128(buffer.WrittenSpan), parent);
    }

    // Kerf-inflated Boolean difference of the placed outlines from the stock: each disjoint
    // leftover region mints its own content-keyed child stamped with the consumed stock's lineage.
    public static Seq<Remnant> From(Stock stock, Seq<Loop> placed, double kerf) {
        Seq<Loop> inflated = placed.Bind(p => PolygonAlgebra.Offset(Seq(p), 0.5 * Math.Abs(kerf), OffsetEnds.Polygon).IfFail(Seq(p)));
        return PolygonAlgebra.Clip(Seq(stock.Outline()), inflated, ClipOp.Difference).IfFail(Seq<Loop>())
            .Filter(r => Math.Abs(PolygonAlgebra.Area(r)) > 1e-6)
            .Map(r => Of(r, Some(stock.Of())));
    }

    public bool Holds(Loop part, double tx, double ty) =>
        part.Vertices.ForAll(v => Boundary.Covers(new Point3d(v.X + tx, v.Y + ty, 0.0)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Stock {
    private Stock() { }

    public sealed record Sheet(double Width, double Height) : Stock;
    public sealed record Plate(double Width, double Height, double Depth) : Stock;
    public sealed record BarStock(double Diameter, double Length) : Stock;
    public sealed record TubeStock(double OuterDiameter, double WallThickness, double Length) : Stock;
    public sealed record Billet(double Width, double Height, double Depth) : Stock;
    public sealed record Filament(double Diameter, double SpoolLength) : Stock;
    public sealed record FromRemnant(Remnant Remnant) : Stock;

    public UInt128 Of() =>
        this is FromRemnant fr ? fr.Remnant.Identity : XxHash128.HashToUInt128(MemoryMarshal.AsBytes<double>(new[] { Area }));

    // Planar cases admit the RectpackSharp AABB fast-path; revolved/remnant cases stay NFP-only.
    public bool Planar =>
        Switch(sheet: static _ => true, plate: static _ => true, billet: static _ => true,
            barStock: static _ => false, tubeStock: static _ => false, filament: static _ => false, fromRemnant: static _ => false);

    public (double Width, double Height) Extent =>
        Switch(
            sheet:       static s => (s.Width, s.Height),
            plate:       static s => (s.Width, s.Height),
            barStock:    static s => (Math.PI * s.Diameter, s.Length),
            tubeStock:   static s => (Math.PI * s.OuterDiameter, s.Length),
            billet:      static s => (s.Width, s.Height),
            filament:    static s => (s.Diameter, s.SpoolLength),
            fromRemnant: static r => (r.Remnant.Boundary.Bound().Diagonal.X, r.Remnant.Boundary.Bound().Diagonal.Y));

    public Loop Outline() {
        if (this is FromRemnant fr) return fr.Remnant.Boundary;
        var (w, h) = Extent;
        return new Loop(Arr(new Point3d(0, 0, 0), new Point3d(w, 0, 0), new Point3d(w, h, 0), new Point3d(0, h, 0)), Closed: true).AsCcw();
    }

    public double Area =>
        Switch(
            sheet:       static s => s.Width * s.Height,
            plate:       static s => s.Width * s.Height,
            barStock:    static s => Math.PI * s.Diameter * s.Length,
            tubeStock:   static s => Math.PI * s.OuterDiameter * s.Length,
            billet:      static s => s.Width * s.Height,
            filament:    static s => s.Diameter * s.SpoolLength,
            fromRemnant: static r => Math.Abs(PolygonAlgebra.Area(r.Remnant.Boundary)));

    public bool Contains(Loop part, double tx, double ty) =>
        Switch(
            state:       (part, tx, ty),
            sheet:       static (k, s) => InRect(k.part, k.tx, k.ty, s.Width, s.Height),
            plate:       static (k, s) => InRect(k.part, k.tx, k.ty, s.Width, s.Height),
            barStock:    static (k, s) => InRect(k.part, k.tx, k.ty, Math.PI * s.Diameter, s.Length),
            tubeStock:   static (k, s) => InRect(k.part, k.tx, k.ty, Math.PI * s.OuterDiameter, s.Length),
            billet:      static (k, s) => InRect(k.part, k.tx, k.ty, s.Width, s.Height),
            filament:    static (k, s) => InRect(k.part, k.tx, k.ty, s.Diameter, s.SpoolLength),
            fromRemnant: static (k, r) => r.Remnant.Holds(k.part, k.tx, k.ty));

    static bool InRect(Loop part, double tx, double ty, double width, double height) =>
        part.Vertices.ForAll(v => v.X + tx >= 0.0 && v.X + tx <= width && v.Y + ty >= 0.0 && v.Y + ty <= height);
}

public sealed record NestPolicy(PlacementMode Mode, bool Genetic, int Rotations, int Population, int Generations, double MutationRate, double Kerf, bool Guillotine, double GrainDirection, int Seed, Func<NoFitPolygon, PartTransform, double> Score) {
    static double Heuristic(NoFitPolygon nfp, PartTransform t) => t.Ty * 1e6 + t.Tx;
    public static readonly NestPolicy BottomLeft = new(PlacementMode.NfpTrueShape, Genetic: false, Rotations: 4, Population: 0, Generations: 0, MutationRate: 0.0, Kerf: 0.2, Guillotine: false, GrainDirection: double.NaN, Seed: 1, Heuristic);
    public static readonly NestPolicy GeneticDefault = new(PlacementMode.NfpTrueShape, Genetic: true, Rotations: 4, Population: 40, Generations: 60, MutationRate: 0.15, Kerf: 0.2, Guillotine: false, GrainDirection: double.NaN, Seed: 1, Heuristic);
    public static readonly NestPolicy RectFlatbed = new(PlacementMode.RectFastpath, Genetic: false, Rotations: 1, Population: 0, Generations: 0, MutationRate: 0.0, Kerf: 0.2, Guillotine: false, GrainDirection: double.NaN, Seed: 1, Heuristic);
}

public sealed record NoFitPolygon(Loop Boundary, Option<Loop> InnerFitLocus) {
    public static Fin<NoFitPolygon> Of(Loop fixedPart, Loop orbiting) {
        Loop a = fixedPart.AsCcw();
        Loop b = Reflect(orbiting);
        return PolygonAlgebra.MinkowskiSum(a, b).Map(loops => new NoFitPolygon(loops.Head.AsCcw(), None));
    }

    // The inner-fit-polygon dual: the exact set of reference positions placing the part fully
    // inside the container, via the precision-bearing MinkowskiDiff (the NFP overlap test's dual).
    public static Fin<Loop> InnerFit(Loop container, Loop part) =>
        PolygonAlgebra.MinkowskiDiff(container.AsCcw(), Reflect(part))
            .Bind(loops => loops.HeadOrNone().Match(
                Some: l => Fin.Succ(l.AsCcw()),
                None: () => Fin.Fail<Loop>(FabricationFault.NoFit("ifp:empty-locus").ToError())));

    public static UInt128 PairKey(Loop fixedPart, Loop orbiting, int rotations) {
        var buffer = new ArrayBufferWriter<byte>();
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), rotations);
        buffer.Advance(4);
        foreach (Loop loop in Seq(fixedPart.AsCcw(), orbiting.AsCcw()))
            foreach (Point3d v in loop.Vertices) {
                Span<byte> slot = buffer.GetSpan(16);
                BinaryPrimitives.WriteDoubleLittleEndian(slot[..8], v.X);
                BinaryPrimitives.WriteDoubleLittleEndian(slot[8..16], v.Y);
                buffer.Advance(16);
            }
        return XxHash128.HashToUInt128(buffer.WrittenSpan);
    }

    static Loop Reflect(Loop loop) =>
        new Loop(loop.Vertices.Map(v => Point3d.Origin - (v - Point3d.Origin)).ToArr(), Closed: true).AsCcw();

    public bool Feasible(double tx, double ty) => !Boundary.Covers(new Point3d(tx, ty, 0.0));

    // True only when the reference point lies INSIDE the inner-fit locus (the part fits the container).
    public bool InnerFeasible(double tx, double ty) =>
        InnerFitLocus.Match(Some: l => l.Covers(new Point3d(tx, ty, 0.0)), None: () => true);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Nest {
    public static Fin<FabricationResult> Solve(FabricationPolicy.Nest policy, FabricationInput input) =>
        input.Profiles.IsEmpty
            ? Fin.Fail<FabricationResult>(GeometryFault.DegenerateInput("nest:no-parts").ToError())
            : input.Plan.Match(
                // A pre-resolved Nesting/stock cutting-stock plan is present: HONOR it (the stock owner owns the
                // rectangular yield) rather than re-deriving a lower-yield layout — the packer path runs only with no
                // plan, the per-stock PlaceStock.Switch the standing placement-mode totality gate (unchanged here).
                Some: plan => Honor(input.Profiles.Map(static l => l.AsCcw()), plan),
                None: () => input.Inventory.IsEmpty
                    ? Fin.Fail<FabricationResult>(GeometryFault.DegenerateInput("nest:no-stock").ToError())
                    : input.Profiles.Find(static l => !l.Closed).Match(
                        Some: _ => Fin.Fail<FabricationResult>(FabricationFault.OpenLoop("nest:open-outline").ToError()),
                        None: () => Schedule(input.Profiles.Map(static l => l.AsCcw()), input.Inventory, policy.Nesting)));

    // Consume the sibling Nesting/stock NestPlan DIRECTLY (same package, no wire mirror): map each NestPlacement
    // straight to a PartTransform — the stock owner keeps the rectangular cutting-stock yield, this fold HONORS the
    // resolved layout rather than minting a second rectangular packer. The placed (XMm, YMm) is the part footprint's
    // lower-left, so each transform rotates the part about the origin by the 90° flag then offsets its rotated
    // bbox-min to that corner (the exact dual of the rect-fastpath arm's bbox-min anchoring, reusing the one Transform
    // fold, never a second rotation kernel). The rectangular remnants stay yield evidence on the NestYield receipt, so
    // the honored Placement mints no Fabrication Remnant; an out-of-range PartId is dropped, an all-dropped plan
    // railing NoFit, the NestYield utilization/unplaced passed straight through.
    static Fin<FabricationResult> Honor(Arr<Loop> parts, NestPlan plan) {
        Seq<PartTransform> placed = plan.Placements
            .Filter(np => np.PartId >= 0 && np.PartId < parts.Count)
            .Map(np => {
                double rot = np.Rotated ? Math.PI / 2.0 : 0.0;
                BoundingBox b = Transform(parts[np.PartId], new PartTransform(np.PartId, 0.0, 0.0, rot)).Bound();
                return new PartTransform(np.PartId, np.XMm - b.Min.X, np.YMm - b.Min.Y, rot, np.SheetIndex);
            });
        return placed.IsEmpty
            ? Fin.Fail<FabricationResult>(FabricationFault.NoFit("nest:empty-plan").ToError())
            : Fin.Succ((FabricationResult)new FabricationResult.Placement(placed, plan.Yield.UtilizationRatio, plan.Yield.UnplacedCount, Seq<Remnant>()));
    }

    // Multi-sheet scheduler over a GROWING stock queue: pop the head stock, place the pending parts,
    // stamp each SheetIndex, mint the consumed stock's kerf-difference remnant, and re-inject a
    // usable remnant onto the queue TAIL so the next pending parts pack the real leftover before a
    // virgin sheet opens — the inter-sheet feasibility loop the card needs, the Sheet count the
    // re-injection termination bound (a remnant that places nothing is not re-queued).
    static Fin<FabricationResult> Schedule(Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy) {
        FrozenDictionary<UInt128, NoFitPolygon> nfp = Precompute(parts, policy);
        var seed = (Queue: inventory, Placed: Seq<PartTransform>(), Remnants: Seq<Remnant>(),
                    Pending: toSeq(Enumerable.Range(0, parts.Count)), Sheet: 0);
        var run = Consume(parts, policy, nfp, seed);
        return run.Placed.IsEmpty
            ? Fin.Fail<FabricationResult>(FabricationFault.NoFit($"nest:none-placed:{parts.Count}").ToError())
            : run.Pending.IsEmpty
                ? Fin.Succ((FabricationResult)new FabricationResult.Placement(run.Placed, Utilization(run.Placed, parts, inventory), 0, run.Remnants))
                : Fin.Fail<FabricationResult>(FabricationFault.StockOverflow($"nest:overflow:{run.Pending.Count}-unplaced/{inventory.Count}-stock").ToError());
    }

    static (Seq<Stock> Queue, Seq<PartTransform> Placed, Seq<Remnant> Remnants, Seq<int> Pending, int Sheet) Consume(
        Arr<Loop> parts, NestPolicy policy, FrozenDictionary<UInt128, NoFitPolygon> nfp,
        (Seq<Stock> Queue, Seq<PartTransform> Placed, Seq<Remnant> Remnants, Seq<int> Pending, int Sheet) st) {
        if (st.Pending.IsEmpty || st.Queue.IsEmpty) return st;
        Stock stock = st.Queue.Head;
        Seq<PartTransform> here = PlaceStock(parts, stock, policy, st.Pending.ToArray(), nfp).Map(t => t with { SheetIndex = st.Sheet });
        Set<int> done = toSet(here.Map(static t => t.PartId));
        Seq<Remnant> minted = Remnant.From(stock, here.Map(t => Transform(parts[t.PartId], t)), policy.Kerf);
        Seq<Stock> reinject = here.IsEmpty ? Seq<Stock>() : minted.Filter(r => Math.Abs(PolygonAlgebra.Area(r.Boundary)) > policy.Kerf * policy.Kerf).Map(r => (Stock)new Stock.FromRemnant(r));
        return Consume(parts, policy, nfp, (
            st.Queue.Tail.Concat(reinject),
            st.Placed.Concat(here),
            st.Remnants.Concat(minted),
            st.Pending.Filter(id => !done.Contains(id)),
            st.Sheet + 1));
    }

    static FrozenDictionary<UInt128, NoFitPolygon> Precompute(Arr<Loop> parts, NestPolicy policy) =>
        Enumerable.Range(0, parts.Count)
            .SelectMany(f => Enumerable.Range(0, parts.Count).Where(o => o != f).Select(o => (f, o)))
            .GroupBy(pair => NoFitPolygon.PairKey(parts[pair.f], parts[pair.o], policy.Rotations))
            .ToFrozenDictionary(g => g.Key, g => NoFitPolygon.Of(parts[g.First().f], parts[g.First().o]).IfFail(new NoFitPolygon(parts[g.First().f], None)));

    static Seq<PartTransform> PlaceStock(Arr<Loop> parts, Stock stock, NestPolicy policy, int[] pending, FrozenDictionary<UInt128, NoFitPolygon> nfp) =>
        policy.Mode.Switch(
            state:        (parts, stock, policy, pending, nfp),
            rectFastpath: static s => s.stock.Planar ? RectPack(s.parts, s.stock, s.pending) : Nfp(s.parts, s.stock, s.policy, s.pending, s.nfp),
            nfpTrueShape: static s => Nfp(s.parts, s.stock, s.policy, s.pending, s.nfp));

    static Seq<PartTransform> Nfp(Arr<Loop> parts, Stock stock, NestPolicy policy, int[] pending, FrozenDictionary<UInt128, NoFitPolygon> nfp) =>
        policy.Genetic ? Genetic(parts, stock, policy, pending, nfp) : BottomLeft(parts, stock, policy, pending, nfp);

    // RectpackSharp axis-aligned-rectangle fast-path: pack the part bounding boxes by the
    // maxrects/guillotine heuristics, fold the int-rect placement back to PartTransform. Pack THROWS
    // plain Exception ("Failed to find a solution") when no layout fits the maxBounds cap (the api
    // [IN_PLACE_PACK] law) — the in-place mutation is wrapped in Try so the infeasibility throw lowers
    // to an empty placement that spills the whole pending set forward to the next stock through the
    // Consume fold, never an uncaught exception escaping the Fin rail; the Right/Bottom post-filter
    // then drops any partially-placed rect the bounded search left past the sheet edge.
    static Seq<PartTransform> RectPack(Arr<Loop> parts, Stock stock, int[] pending) {
        var (sw, sh) = stock.Extent;
        uint capW = (uint)Math.Floor(sw), capH = (uint)Math.Floor(sh);
        PackingRectangle[] rects = pending.Select(id => {
            BoundingBox b = parts[id].Bound();
            return new PackingRectangle(capW + 1u, capH + 1u, (uint)Math.Ceiling(b.Diagonal.X), (uint)Math.Ceiling(b.Diagonal.Y), id);
        }).ToArray();
        return Try(() => { RectanglePacker.Pack(rects, out _, PackingHints.FindBest, acceptableDensity: 1.0, stepSize: 1u,
                               maxBoundsWidth: capW, maxBoundsHeight: capH); return rects; })
            .Match(
                Succ: packed => toSeq(packed).Filter(r => r.Right <= sw && r.Bottom <= sh)
                    .Map(r => new PartTransform((int)r.Id, r.X - parts[(int)r.Id].Bound().Min.X, r.Y - parts[(int)r.Id].Bound().Min.Y, 0.0)),
                Fail: _ => Seq<PartTransform>());
    }

    static Seq<PartTransform> BottomLeft(Arr<Loop> parts, Stock stock, NestPolicy policy, int[] order, FrozenDictionary<UInt128, NoFitPolygon> nfp) =>
        toSeq(order).Fold(Seq<(int Id, Loop Part, double Tx, double Ty)>(), (placed, id) => {
            Loop part = parts[id];
            NoFitPolygon Pair(int fixedId) => nfp[NoFitPolygon.PairKey(parts[fixedId], part, policy.Rotations)];
            Option<Loop> ifp = stock.Planar ? None : NoFitPolygon.InnerFit(stock.Outline(), part).ToOption();
            var candidates = placed.Bind(pl => Pair(pl.Id).Boundary.Vertices.AsEnumerable())
                .Append(new Point3d(0.0, 0.0, 0.0))
                .Select(c => (Point: c, Transform: new PartTransform(id, c.X, c.Y, 0.0)))
                .Where(c => stock.Contains(part, c.Point.X, c.Point.Y) &&
                            ifp.Match(Some: l => l.Covers(c.Point), None: () => true) &&
                            placed.ForAll(pl => Pair(pl.Id).Feasible(c.Point.X - Anchor(pl.Part).X, c.Point.Y - Anchor(pl.Part).Y)))
                .OrderBy(c => policy.Score(placed.HeadOrNone().Map(pl => Pair(pl.Id)).IfNone(new NoFitPolygon(part, None)), c.Transform));
            return candidates.HeadOrNone()
                .Match(Some: c => placed.Add((id, part, c.Point.X, c.Point.Y)), None: () => placed);
        }).Map(pl => new PartTransform(pl.Id, pl.Tx, pl.Ty, 0.0));

    static Seq<PartTransform> Genetic(Arr<Loop> parts, Stock stock, NestPolicy policy, int[] pending, FrozenDictionary<UInt128, NoFitPolygon> nfp) {
        var rng = new Random(policy.Seed);
        int[][] population = Enumerable.Range(0, policy.Population).Select(_ => Shuffle((int[])pending.Clone(), rng)).ToArray();
        double bestScore = -1.0; Seq<PartTransform> bestPlace = Seq<PartTransform>();
        for (int gen = 0; gen < policy.Generations; gen++) {
            var scored = population.Select(chrom => {
                Seq<PartTransform> place = BottomLeft(parts, stock, policy, chrom, nfp);
                return (Chrom: chrom, Place: place, Score: Utilization(place, parts, Seq(stock)));
            }).OrderByDescending(s => s.Score).ToArray();
            if (scored[0].Score > bestScore) { bestScore = scored[0].Score; bestPlace = scored[0].Place; }
            population = Enumerable.Range(0, policy.Population)
                .Select(_ => Mutate(Crossover(Tournament(scored, rng), Tournament(scored, rng), rng), policy.MutationRate, rng))
                .ToArray();
        }
        return bestPlace;
    }

    static double Utilization(Seq<PartTransform> placed, Arr<Loop> parts, Seq<Stock> stocks) =>
        placed.Sum(pt => Math.Abs(PolygonAlgebra.Area(parts[pt.PartId]))) / Math.Max(1e-9, stocks.Sum(static s => s.Area));

    static Loop Transform(Loop part, PartTransform t) {
        double ct = Math.Cos(t.RotationRadians), st = Math.Sin(t.RotationRadians);
        return new Loop(part.Vertices.Map(v => new Point3d(v.X * ct - v.Y * st + t.Tx, v.X * st + v.Y * ct + t.Ty, 0.0)).ToArr(), Closed: true);
    }

    static Point3d Anchor(Loop loop) => loop.Vertices.OrderBy(v => v.Y).ThenBy(v => v.X).Head();

    static int[] Shuffle(int[] a, Random rng) { for (int i = a.Length - 1; i > 0; i--) { int j = rng.Next(i + 1); (a[i], a[j]) = (a[j], a[i]); } return a; }

    static int[] Tournament((int[] Chrom, Seq<PartTransform> Place, double Score)[] scored, Random rng) =>
        scored[Math.Min(rng.Next(scored.Length), rng.Next(scored.Length))].Chrom;

    static int[] Crossover(int[] a, int[] b, Random rng) {
        int n = a.Length, lo = rng.Next(n), hi = rng.Next(n);
        if (lo > hi) (lo, hi) = (hi, lo);
        var child = new int[n]; Array.Fill(child, -1);
        var taken = new HashSet<int>();
        for (int i = lo; i <= hi; i++) { child[i] = a[i]; taken.Add(a[i]); }
        int w = 0;
        foreach (int g in b) { if (taken.Contains(g)) continue; while (child[w] != -1) w++; child[w] = g; }
        return child;
    }

    static int[] Mutate(int[] chrom, double rate, Random rng) {
        if (rng.NextDouble() >= rate) return chrom;
        int i = rng.Next(chrom.Length), j = rng.Next(chrom.Length);
        (chrom[i], chrom[j]) = (chrom[j], chrom[i]);
        return chrom;
    }
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Profiles["Part outlines"] -->|NoFitPolygon.Of| NFP["Geometry2D Minkowski NFP"]
    Profiles -->|NoFitPolygon.InnerFit| IFP["Geometry2D MinkowskiDiff IFP"]
    Inventory["Seq&lt;Stock&gt; cut-list"] -->|sheet-by-sheet| Schedule["Schedule fold"]
    Plan["Nesting/stock NestPlan (FabricationInput.Plan)"] -->|"plan present → Nest.Honor"| Honor["map NestPlacement → PartTransform"]
    Honor -->|carried utilization, no packer| Placement
    NFP -->|Orient2D feasible| Schedule
    IFP -->|inside-container| Schedule
    Schedule -->|"Mode = nfp-true-shape"| BottomLeft["Bottom-left / GA"]
    Schedule -->|"Mode = rect-fastpath (Planar)"| Rect["RectpackSharp maxrects/guillotine"]
    BottomLeft -->|SheetIndex stamp| Placement["Placement"]
    Rect -->|SheetIndex stamp| Placement
    Schedule -->|spill unplaced forward| Schedule
    Schedule -->|"Remnant.From kerf-difference"| Remnant["Remnant + Parent lineage"]
    Remnant -->|re-enter inventory tail| Schedule
    Schedule -.->|inventory exhausted| Overflow["FabricationFault.StockOverflow"]
```
