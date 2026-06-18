# [RASM_FABRICATION_NFP]

The 2D true-shape nesting owner: `Nest` the static placement fold packing part outlines onto a stock sheet through a no-fit-polygon (NFP) feasibility test, with a bottom-left greedy and a genetic placement heuristic over the same NFP primitive. The NFP construction routes the `geometry2d/clipper#POLYGON_ALGEBRA` `MinkowskiSum` — the Minkowski sum of the fixed part with the reflected orbiting part — never a hand-rolled angle-sorted edge merge; Clipper2 owns the polygon construction at integer-robust precision, and the irregular/non-convex NFP (convex decomposition + per-piece Minkowski union) is one arm on the same Geometry2D owner. The feasibility verdict — whether a reference point lies inside or outside the NFP — stays the kernel `Rasm.Geometry/numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact point-in-polygon sign. The bottom-left and genetic modes are ONE fold over the placement discriminant, never two packer classes. A DRL-guided placement policy is a forward `NestPolicy` column composing the `Rasm.Compute/models/inference#INFERENCE_MODES` ONNX lane, the NFP primitive unchanged. The kernel composes the `frontier/owner#FABRICATION_OWNER` `Loop`/`PartTransform`/`FrontierPolicy.Nest`/`FrontierResult.Placement` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The `Placement` transforms cross only the in-process seam to the `posting/program#CUT_PROGRAM` emitter — never a browser or peer wire. The `SheetBounds`/`NestPolicy`/`NoFitPolygon` records are host-local types that never sit between wire and rail.

## [1]-[INDEX]

One cluster: `[2]-[NESTING]` owns the `SheetBounds`/`NestPolicy`/`NoFitPolygon` records and the `Nest` fold — no-fit-polygon feasibility (Minkowski via Geometry2D) with the bottom-left and genetic placement modes over the one NFP primitive.

## [2]-[NESTING]

- Owner: `SheetBounds` the stock sheet extents the parts pack onto; `NestPolicy` the placement knobs (rotation step count, GA population, generations, mutation rate, the bottom-left-vs-genetic discriminant); `NoFitPolygon` the sliding-locus polygon — the set of reference positions where the orbiting part touches but never overlaps the fixed part, the canonical primitive every 2D true-shape nesting heuristic reads, its boundary built through the Geometry2D Minkowski sum; `Nest` the static placement fold building each part-pair NFP and folding the parts onto the sheet by the bottom-left or genetic-ordered heuristic.
- Cases: placement modes `bottom-left` (a deterministic greedy lowest-then-leftmost feasible position per part) and `genetic` (a GA over the part ordering + rotation, the bottom-left decode scoring each chromosome by utilization) (2); the NFP here is the convex Minkowski merge through Geometry2D, the irregular/non-convex NFP (convex decomposition + per-piece union) the one widening arm on the same `NoFitPolygon.Of` owner.
- Entry: `public static Fin<FrontierResult> Solve(FrontierPolicy.Nest policy, FrontierInput input)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed part outline and `FabricationFault.NoFit` when a part cannot be placed within the sheet under every rotation, each lowered with `.ToError()`; the body builds the pairwise NFPs, then runs the bottom-left or GA placement fold emitting the `Placement` transforms and the utilization scalar.
- Auto: `NoFitPolygon.Of` reflects the orbiting part through the origin and runs the `geometry2d/clipper#POLYGON_ALGEBRA` `MinkowskiSum` of the fixed part and the reflected orbiting part, taking the result boundary as the NFP; the irregular/non-convex NFP (convex decomposition into sub-pieces, per-piece Minkowski sum, locus union) is the one widening arm on this owner over the same Geometry2D substrate. `Nest.Solve` precomputes every ordered pairwise NFP into a frozen memo, then a candidate placement is feasible when the part reference point lies OUTSIDE every already-placed part's NFP (no overlap) and inside the sheet, the inside/outside test the exact `Orient2D` point-in-polygon; `bottom-left` mode folds the parts in descending-area order, sliding each to its lowest feasible NFP-boundary position; `genetic` mode evolves a population of (order, rotation) chromosomes, decoding each through the same bottom-left placement and scoring by packed-area utilization, the GA fold running tournament selection, order crossover, and swap mutation for `Generations` and returning the best decode.
- Receipt: the `Placement` carries the per-part `PartTransform` (translation + rotation), the sheet utilization fraction, and the unplaced count — the typed nesting evidence the posting emitter consumes; no generic nesting ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d`/`BoundingBox` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, point-in-polygon feasibility), Clipper2 (via `geometry2d/clipper#POLYGON_ALGEBRA` — the NFP Minkowski sum), LanguageExt.Core, BCL inbox.
- Growth: a full irregular-shape NFP with rotation search is one decomposition arm on `NoFitPolygon.Of` over the same Geometry2D Minkowski owner; a DRL-guided placement is one `NestPolicy` column composing the `Rasm.Compute/models/inference#INFERENCE_MODES` ONNX lane, the NFP unchanged; a new heuristic is one column on `NestPolicy`; zero new surface.
- Boundary: nesting is the ONE author-kernel placement owner and the NFP construction routes the one `geometry2d/clipper#POLYGON_ALGEBRA` Minkowski owner — a hand-rolled angle-sorted edge merge is the deleted form; the NFP is the canonical placement primitive and a per-heuristic bespoke overlap test is the deleted form, every feasibility check reading the same NFP and the exact `Orient2D` inside/outside; the bottom-left and genetic modes are ONE fold over the placement discriminant, never two packer classes; the point-in-polygon side test reads `Predicate.Orient2D` exact sign and a naive `double` cross is the named robustness defect; the per-generation GA population evolution and the in-place `Crossover`/`Shuffle` chromosome scratch are the named measured-kernel statement exemption — the order-crossover and Fisher-Yates index permutation over `int[]` arrays below the dense-collection crossover, never an immutable rebuild per swap.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Frontier;
using Rasm.Fabrication.Geometry2D;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SheetBounds(double Width, double Height) {
    public bool Contains(Loop part, double tx, double ty) =>
        part.Vertices.ForAll(v => v.X + tx >= 0.0 && v.X + tx <= Width && v.Y + ty >= 0.0 && v.Y + ty <= Height);
}

public sealed record NestPolicy(bool Genetic, int Rotations, int Population, int Generations, double MutationRate, int Seed) {
    public static readonly NestPolicy BottomLeft = new(Genetic: false, Rotations: 4, Population: 0, Generations: 0, MutationRate: 0.0, Seed: 1);
    public static readonly NestPolicy GeneticDefault = new(Genetic: true, Rotations: 4, Population: 40, Generations: 60, MutationRate: 0.15, Seed: 1);
}

public sealed record NoFitPolygon(Loop Boundary) {
    public static Fin<NoFitPolygon> Of(Loop fixedPart, Loop orbiting) {
        Loop a = fixedPart.AsCcw();
        Loop b = new Loop(orbiting.Vertices.Map(v => Point3d.Origin - (v - Point3d.Origin)).ToArr(), Closed: true).AsCcw();
        return PolygonAlgebra.MinkowskiSum(a, b).Map(loops => new NoFitPolygon(loops.Head.AsCcw()));
    }

    public bool Feasible(double tx, double ty) {
        var p = new Point3d(tx, ty, 0.0);
        for (int i = 0; i < Boundary.Count; i++)
            if (Predicate.Orient2D(Boundary.At(i), Boundary.At(i + 1), p) == Sign.Negative) return true;
        return false;
    }
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
                        ? Genetic(parts, input.Sheet, policy.Nesting, nfp)
                        : BottomLeft(parts, input.Sheet, policy.Nesting, Enumerable.Range(0, parts.Count).ToArray(), nfp);
                    int unplaced = parts.Count - placed.Count;
                    return unplaced == parts.Count
                        ? Fin.Fail<FrontierResult>(FabricationFault.NoFit($"nest:none-placed:{parts.Count}").ToError())
                        : Fin.Succ((FrontierResult)new FrontierResult.Placement(placed, Utilization(placed, parts, input.Sheet), unplaced));
                });

    static Seq<PartTransform> BottomLeft(Arr<Loop> parts, SheetBounds sheet, NestPolicy policy, int[] order, FrozenDictionary<(int, int), NoFitPolygon> nfp) =>
        toSeq(order).Fold(Seq<(int Id, Loop Part, double Tx, double Ty)>(), (placed, id) => {
            Loop part = parts[id];
            var candidates = placed.Bind(pl => nfp[(pl.Id, id)].Boundary.Vertices.AsEnumerable())
                .Append(new Point3d(0.0, 0.0, 0.0))
                .OrderBy(c => c.Y).ThenBy(c => c.X);
            return candidates.Filter(c =>
                    sheet.Contains(part, c.X, c.Y) &&
                    placed.ForAll(pl => nfp[(pl.Id, id)].Feasible(c.X - Anchor(pl.Part).X, c.Y - Anchor(pl.Part).Y)))
                .HeadOrNone()
                .Match(Some: c => placed.Add((id, part, c.X, c.Y)), None: () => placed);
        }).Map(pl => new PartTransform(pl.Id, pl.Tx, pl.Ty, 0.0));

    static Seq<PartTransform> Genetic(Arr<Loop> parts, SheetBounds sheet, NestPolicy policy, FrozenDictionary<(int, int), NoFitPolygon> nfp) {
        var rng = new Random(policy.Seed);
        int[][] population = Enumerable.Range(0, policy.Population).Select(_ => Shuffle(Enumerable.Range(0, parts.Count).ToArray(), rng)).ToArray();
        int[] best = population[0]; double bestScore = -1.0; Seq<PartTransform> bestPlace = Seq<PartTransform>();
        for (int gen = 0; gen < policy.Generations; gen++) {
            var scored = population.Select(chrom => {
                Seq<PartTransform> place = BottomLeft(parts, sheet, policy, chrom, nfp);
                return (Chrom: chrom, Place: place, Score: Utilization(place, parts, sheet));
            }).OrderByDescending(s => s.Score).ToArray();
            if (scored[0].Score > bestScore) { bestScore = scored[0].Score; best = scored[0].Chrom; bestPlace = scored[0].Place; }
            population = Enumerable.Range(0, policy.Population)
                .Select(_ => Mutate(Crossover(Tournament(scored, rng), Tournament(scored, rng), rng), policy.MutationRate, rng))
                .ToArray();
        }
        return bestPlace;
    }

    static double Utilization(Seq<PartTransform> placed, Arr<Loop> parts, SheetBounds sheet) =>
        placed.Sum(pt => Math.Abs(SignedArea(parts[pt.PartId]))) / Math.Max(1e-9, sheet.Width * sheet.Height);

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
