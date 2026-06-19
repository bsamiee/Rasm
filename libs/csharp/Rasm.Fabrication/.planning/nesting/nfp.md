# [RASM_FABRICATION_NFP]

The 2D true-shape nesting owner: `Nest` the static placement fold packing part outlines onto a `Stock` envelope through a no-fit-polygon (NFP) feasibility test, with a bottom-left greedy and a genetic placement heuristic over the same NFP primitive. The NFP construction routes the `Polygon/clipper#POLYGON_ALGEBRA` `MinkowskiSum` — the Minkowski sum of the fixed part with the reflected orbiting part — never a hand-rolled angle-sorted edge merge; Clipper2 owns the polygon construction at integer-robust precision, and the irregular/non-convex NFP (convex decomposition + per-piece Minkowski union) is one arm on the same Geometry2D owner. The stock the parts pack onto is the `Stock` `[Union]` (`Sheet`/`Plate`/`BarStock`/`TubeStock`/`Billet`/`Filament`/`Remnant`) — one closed family with a single `Bounds`/`Contains`/`Area`/`Of` total fold the feasibility kernel reads, collapsing the old virgin-rectangle-vs-remnant `Option<Remnant>` discriminant and the new plate/bar/tube/billet/filament cases into one owner: a planar `Sheet`/`Plate`/`Remnant` packs in its 2D bounds, a `BarStock`/`TubeStock` packs onto its 1-axis-revolved envelope (the unrolled circumference × length), the `Filament` is the additive feedstock budget. The feasibility verdict — whether a reference point lies inside or outside the NFP — stays the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact point-in-polygon sign. The bottom-left and genetic modes are ONE fold over the placement discriminant, never two packer classes. A DRL-guided placement policy is a `NestPolicy` column carrying an injected `Func<NoFitPolygon, PartTransform, double>` placement-score delegate (defaulted to the heuristic lowest-then-leftmost score) the app-platform consumer wires from the `Rasm.Compute/Model/inference#INFERENCE_MODES` ONNX lane (Fabrication is AEC-domain and never references the app-platform `Rasm.Compute`; the strata law forbids the downward edge), the NFP primitive unchanged. The kernel composes the `Process/owner#FABRICATION_OWNER` `Loop`/`PartTransform`/`FrontierPolicy.Nest`/`FrontierResult.Placement` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `Placement` transforms cross only the in-process seam to the `Posting/program#CUT_PROGRAM` emitter — never a browser or peer wire. The `Stock`/`NestPolicy`/`NoFitPolygon` records are host-local types that never sit between wire and rail.

## [1]-[INDEX]

- [1]-[NESTING]: owns the `Stock` union, the `NestPolicy`/`NoFitPolygon` records, and the `Nest` fold — no-fit-polygon feasibility (Minkowski via Geometry2D) with the bottom-left and genetic placement modes over the one NFP primitive and the injected placement-score delegate slot.

## [2]-[NESTING]

- Owner: `Remnant` the leftover-stock polygon carrying its boundary `Loop` and its `XxHash128`-derived `UInt128` content identity, the `Holds` exact point-in-polygon containment, and the `Of` content-address mint over the kernel `System.IO.Hashing` `XxHash128`; `Stock` `[Union]` the stock the parts pack onto — `Sheet` (virgin rectangle) · `Plate` (thick rectangle with a cut-depth column) · `BarStock`/`TubeStock` (a revolved envelope unrolled to its circumference × length planar bounds) · `Billet` (a solid block) · `Filament` (additive feedstock length) · `Remnant` (the content-keyed leftover polygon) — with one `Bounds`/`Contains`/`Area`/`Of` total fold every feasibility check reads, the `Switch` discriminating the planar rectangle bounds from the revolved-envelope bounds from the remnant polygon containment, never a parallel per-stock packer; `NestPolicy` the placement knobs (rotation step count, GA population, generations, mutation rate, the bottom-left-vs-genetic discriminant, and the `Score` placement-rank delegate); `NoFitPolygon` the sliding-locus polygon — the set of reference positions where the orbiting part touches but never overlaps the fixed part, the canonical primitive every 2D true-shape nesting heuristic reads, its boundary built through the Geometry2D Minkowski sum; `Nest` the static placement fold building each part-pair NFP and folding the parts onto the `Stock` envelope by the bottom-left or genetic-ordered heuristic.
- Cases: placement modes `bottom-left` (a deterministic greedy lowest-then-leftmost feasible position per part) and `genetic` (a GA over the part ordering + rotation, the bottom-left decode scoring each chromosome by utilization) (2); the `Stock` union cases `Sheet` · `Plate` · `BarStock` · `TubeStock` · `Billet` · `Filament` · `Remnant` (7), the one `Contains`/`Area` fold discriminating the planar bounds from the revolved envelope from the remnant containment, the remnant re-entering the same NFP feasibility set as a virgin sheet; the NFP here is the convex Minkowski merge through Geometry2D, the irregular/non-convex NFP (convex decomposition + per-piece union) the one widening arm on the same `NoFitPolygon.Of` owner.
- Entry: `public static Fin<FrontierResult> Solve(FrontierPolicy.Nest policy, FrontierInput input)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed part outline and `FabricationFault.NoFit` when a part cannot be placed within the stock under every rotation, each lowered with `.ToError()`; the body builds the pairwise NFPs, then runs the bottom-left or GA placement fold emitting the `Placement` transforms and the utilization scalar.
- Auto: `NoFitPolygon.Of` reflects the orbiting part through the origin and runs the `Polygon/clipper#POLYGON_ALGEBRA` `MinkowskiSum` of the fixed part and the reflected orbiting part, taking the result boundary as the NFP; the irregular/non-convex NFP (convex decomposition into sub-pieces, per-piece Minkowski sum, locus union) is the one widening arm on this owner over the same Geometry2D substrate. `Nest.Solve` precomputes every ordered pairwise NFP into a frozen memo, then a candidate placement is feasible when the part reference point lies OUTSIDE every already-placed part's NFP (no overlap) and `Stock.Contains` holds — for a planar `Sheet`/`Plate`/`Billet` the axis-aligned bounds, for a `BarStock`/`TubeStock` the unrolled circumference × length bounds, for a `Remnant` the exact `Remnant.Holds` polygon containment over the same `Orient2D` point-in-polygon — so a partially-consumed stock's remnant re-enters the feasibility set as stock and the next nest packs onto the real remnant rather than a virgin sheet; `bottom-left` mode folds the parts in descending-area order, sliding each to its lowest feasible NFP-boundary position scored by `NestPolicy.Score` (defaulted to the lowest-then-leftmost heuristic, overridable by the injected DRL delegate); `genetic` mode evolves a population of (order, rotation) chromosomes, decoding each through the same bottom-left placement and scoring by packed-area utilization against `Stock.Area` (the remnant polygon area when stock is a remnant, the unrolled area when bar/tube), the GA fold running tournament selection, order crossover, and swap mutation for `Generations` and returning the best decode. `Remnant.Of` content-addresses the remnant boundary through the kernel `XxHash128` so a remnant is keyed by the one content identity, never a second tag.
- Receipt: the `Placement` carries the per-part `PartTransform` (translation + rotation), the stock utilization fraction, and the unplaced count — the typed nesting evidence the posting emitter consumes; no generic nesting ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d`/`BoundingBox` — composed), the `Process/owner#FABRICATION_OWNER` `Loop.Covers` (point-in-polygon feasibility, composing the kernel `Predicate.Orient2D` transitively), Clipper2 (via `Polygon/clipper#POLYGON_ALGEBRA` — the NFP Minkowski sum), `System.IO.Hashing` (`XxHash128` — the `Stock` content identity, the same federation hash the kernel `Rasm.Geometry/Spatial/reconciliation#NAMING_HASH` reads), `System.Buffers.Binary` (`BinaryPrimitives`), LanguageExt.Core, BCL inbox.
- Growth: a full irregular-shape NFP with rotation search is one decomposition arm on `NoFitPolygon.Of` over the same Geometry2D Minkowski owner; a new stock kind is one `Stock` union case carrying its `Bounds`/`Contains`/`Area` arm, never a second nesting owner; a DRL-guided placement is the `NestPolicy.Score` delegate column the app-platform consumer fills from the `Rasm.Compute/Model/inference#INFERENCE_MODES` ONNX lane (never a Fabrication-side `Rasm.Compute` reference — the AEC→app-platform edge is forbidden), the NFP and heuristic folds unchanged; a new heuristic is one column on `NestPolicy`; zero new surface.
- Boundary: nesting is the ONE author-kernel placement owner and the NFP construction routes the one `Polygon/clipper#POLYGON_ALGEBRA` Minkowski owner — a hand-rolled angle-sorted edge merge is the deleted form; the NFP is the canonical placement primitive and a per-heuristic bespoke overlap test is the deleted form, every feasibility check reading the same NFP and the exact `Orient2D` inside/outside; the bottom-left and genetic modes are ONE fold over the placement discriminant, never two packer classes; the stock rides the one `Stock` union with one `Bounds`/`Contains`/`Area` fold and a `SheetPacker`/`BarPacker`/`RemnantPacker` per-stock packer triple is the deleted form — the `Contains` `Switch` discriminates the planar bounds from the revolved envelope from the remnant `Holds` containment over one owner, and the old `Option<Remnant>` discriminant collapses INTO the union closedness; the placement-score delegate is the one `NestPolicy.Score` column and a parallel learned-vs-heuristic packer split is the deleted form — the injected delegate ranks placements over the unchanged NFP, the heuristic score the default arm; the remnant identity is the kernel `XxHash128` content address (one federation hash, never a second tag) and the geometry domain mints no parallel digest; the point-in-polygon side test reads the shared `Process/owner#FABRICATION_OWNER` `Loop.Covers` exact-`Orient2D` containment (`Remnant.Holds` and `NoFitPolygon.Feasible` compose it, never a per-page re-rolled containment loop) and a naive `double` cross is the named robustness defect; the per-generation GA population evolution and the in-place `Crossover`/`Shuffle` chromosome scratch are the named measured-kernel statement exemption — the order-crossover and Fisher-Yates index permutation over `int[]` arrays below the dense-collection crossover, never an immutable rebuild per swap; the strata law forbids the AEC→app-platform downward edge and a `Rasm.Compute` reference in this folder is the rejected form — the DRL score crosses as a raw `double` through the injected delegate, the upstream owner never named.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Frontier;
using Rasm.Fabrication.Geometry2D;
using Rasm.Geometry;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Remnant(Loop Boundary, UInt128 Identity) {
    public static Remnant Of(Loop boundary) {
        Loop ccw = boundary.AsCcw();
        var buffer = new ArrayBufferWriter<byte>();
        foreach (Point3d v in ccw.Vertices) {
            Span<byte> slot = buffer.GetSpan(16);
            BinaryPrimitives.WriteDoubleLittleEndian(slot[..8], v.X);
            BinaryPrimitives.WriteDoubleLittleEndian(slot[8..16], v.Y);
            buffer.Advance(16);
        }
        return new Remnant(ccw, XxHash128.HashToUInt128(buffer.WrittenSpan));
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

    public double Area =>
        Switch(
            sheet:       static s => s.Width * s.Height,
            plate:       static s => s.Width * s.Height,
            barStock:    static s => Math.PI * s.Diameter * s.Length,
            tubeStock:   static s => Math.PI * s.OuterDiameter * s.Length,
            billet:      static s => s.Width * s.Height,
            filament:    static s => s.Diameter * s.SpoolLength,
            fromRemnant: static r => Math.Abs(0.5 * Enumerable.Range(0, r.Remnant.Boundary.Count)
                                          .Sum(i => r.Remnant.Boundary.At(i).X * r.Remnant.Boundary.At(i + 1).Y - r.Remnant.Boundary.At(i + 1).X * r.Remnant.Boundary.At(i).Y)));

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

public sealed record NestPolicy(bool Genetic, int Rotations, int Population, int Generations, double MutationRate, int Seed, Func<NoFitPolygon, PartTransform, double> Score) {
    static double Heuristic(NoFitPolygon nfp, PartTransform t) => t.Ty * 1e6 + t.Tx;
    public static readonly NestPolicy BottomLeft = new(Genetic: false, Rotations: 4, Population: 0, Generations: 0, MutationRate: 0.0, Seed: 1, Heuristic);
    public static readonly NestPolicy GeneticDefault = new(Genetic: true, Rotations: 4, Population: 40, Generations: 60, MutationRate: 0.15, Seed: 1, Heuristic);
}

public sealed record NoFitPolygon(Loop Boundary) {
    public static Fin<NoFitPolygon> Of(Loop fixedPart, Loop orbiting) {
        Loop a = fixedPart.AsCcw();
        Loop b = new Loop(orbiting.Vertices.Map(v => Point3d.Origin - (v - Point3d.Origin)).ToArr(), Closed: true).AsCcw();
        return PolygonAlgebra.MinkowskiSum(a, b).Map(loops => new NoFitPolygon(loops.Head.AsCcw()));
    }

    public bool Feasible(double tx, double ty) => !Boundary.Covers(new Point3d(tx, ty, 0.0));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Nest {
    public static Fin<FrontierResult> Solve(FrontierPolicy.Nest policy, FrontierInput input) =>
        input.Profiles.IsEmpty
            ? Fin.Fail<FrontierResult>(GeometryFault.DegenerateInput("nest:no-parts").ToError())
            : input.Profiles.Find(static l => !l.Closed).Match(
                Some: _ => Fin.Fail<FrontierResult>(FabricationFault.OpenLoop("nest:open-outline").ToError()),
                None: () => {
                    Arr<Loop> parts = input.Profiles.Map(static l => l.AsCcw());
                    var nfp = Enumerable.Range(0, parts.Count)
                        .SelectMany(f => Enumerable.Range(0, parts.Count).Where(o => o != f).Select(o => (f, o)))
                        .ToFrozenDictionary(pair => pair, pair => NoFitPolygon.Of(parts[pair.f], parts[pair.o]).IfFail(new NoFitPolygon(parts[pair.f])));
                    var placed = policy.Nesting.Genetic
                        ? Genetic(parts, input.Stock, policy.Nesting, nfp)
                        : BottomLeft(parts, input.Stock, policy.Nesting, Enumerable.Range(0, parts.Count).ToArray(), nfp);
                    int unplaced = parts.Count - placed.Count;
                    return unplaced == parts.Count
                        ? Fin.Fail<FrontierResult>(FabricationFault.NoFit($"nest:none-placed:{parts.Count}").ToError())
                        : Fin.Succ((FrontierResult)new FrontierResult.Placement(placed, Utilization(placed, parts, input.Stock), unplaced));
                });

    static Seq<PartTransform> BottomLeft(Arr<Loop> parts, Stock stock, NestPolicy policy, int[] order, FrozenDictionary<(int, int), NoFitPolygon> nfp) =>
        toSeq(order).Fold(Seq<(int Id, Loop Part, double Tx, double Ty)>(), (placed, id) => {
            Loop part = parts[id];
            var candidates = placed.Bind(pl => nfp[(pl.Id, id)].Boundary.Vertices.AsEnumerable())
                .Append(new Point3d(0.0, 0.0, 0.0))
                .Select(c => (Point: c, Transform: new PartTransform(id, c.X, c.Y, 0.0)))
                .Where(c => stock.Contains(part, c.Point.X, c.Point.Y) &&
                            placed.ForAll(pl => nfp[(pl.Id, id)].Feasible(c.Point.X - Anchor(pl.Part).X, c.Point.Y - Anchor(pl.Part).Y)))
                .OrderBy(c => policy.Score(nfp.TryGetValue((id, id), out var self) ? self : new NoFitPolygon(part), c.Transform));
            return candidates.HeadOrNone()
                .Match(Some: c => placed.Add((id, part, c.Point.X, c.Point.Y)), None: () => placed);
        }).Map(pl => new PartTransform(pl.Id, pl.Tx, pl.Ty, 0.0));

    static Seq<PartTransform> Genetic(Arr<Loop> parts, Stock stock, NestPolicy policy, FrozenDictionary<(int, int), NoFitPolygon> nfp) {
        var rng = new Random(policy.Seed);
        int[][] population = Enumerable.Range(0, policy.Population).Select(_ => Shuffle(Enumerable.Range(0, parts.Count).ToArray(), rng)).ToArray();
        int[] best = population[0]; double bestScore = -1.0; Seq<PartTransform> bestPlace = Seq<PartTransform>();
        for (int gen = 0; gen < policy.Generations; gen++) {
            var scored = population.Select(chrom => {
                Seq<PartTransform> place = BottomLeft(parts, stock, policy, chrom, nfp);
                return (Chrom: chrom, Place: place, Score: Utilization(place, parts, stock));
            }).OrderByDescending(s => s.Score).ToArray();
            if (scored[0].Score > bestScore) { bestScore = scored[0].Score; best = scored[0].Chrom; bestPlace = scored[0].Place; }
            population = Enumerable.Range(0, policy.Population)
                .Select(_ => Mutate(Crossover(Tournament(scored, rng), Tournament(scored, rng), rng), policy.MutationRate, rng))
                .ToArray();
        }
        return bestPlace;
    }

    static double Utilization(Seq<PartTransform> placed, Arr<Loop> parts, Stock stock) =>
        placed.Sum(pt => Math.Abs(SignedArea(parts[pt.PartId]))) / Math.Max(1e-9, stock.Area);

    static double SignedArea(Loop loop) =>
        0.5 * Enumerable.Range(0, loop.Count).Sum(i => loop.At(i).X * loop.At(i + 1).Y - loop.At(i + 1).X * loop.At(i).Y);

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
    NFP -->|Orient2D feasible| BottomLeft["Bottom-left greedy"]
    BottomLeft -->|decode| Genetic["GA order/rotation"]
    Genetic -->|best utilization| Placement["Placement"]
    BottomLeft -->|deterministic| Placement
```
