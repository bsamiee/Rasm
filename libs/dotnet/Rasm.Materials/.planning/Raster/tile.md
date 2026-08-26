# [MATERIALS_TILE]

THE TILING SYNTHESIZER AND ITS DETERMINISTIC GATE. One `TileStrategy` `[SmartEnum<string>]` closes the tiling algebra — `offsetHeal` the wrap-and-heal minimum-error cut, `graphCut` the Kwatra minimum-cut seam over the overlap band, `wang` the COMPLETE edge-coloured aperiodic family laid out as one atlas with per-colour boundary strips, `histogramBlend` the Heitz-Neyret variance-preserving Gaussian blend — and each row is one `Solve` delegate producing a `TilePlan`, so a new tiling method is a row and never a second synthesizer. Tiling is SET-COHERENT by construction: the plan is derived ONCE from a single guide channel and carries only channel-independent geometry — a wrap offset, a per-texel `[0,1]` blend field, and the Wang edge table — so `TileSynth.Apply` folds the SAME geometry over every channel and pack plane in the set and a normal, height, or occlusion plane cannot acquire a seam its base colour does not have. One `TileProof.Grade` grades the result deterministically and mints the `TileProof` that is the ONLY way a `set#TEXTURE_SET` `TextureSet`'s `Tiled` column acquires a measured proof — a caller's assertion has no spelling that reaches it.

Tiling is PROCEDURAL, never learned: a diffusion or sampler-loop tiling stage does not exist on this page, because a learned generator leaks at its decoder boundary exactly where the seam must be exact, and a set whose channels were each tiled by an independent stochastic pass has no shared geometry at all. The learned scorer is a SPIKE beside the deterministic floor, never in place of it. The page composes the `set#TEXTURE_SET` `TextureSet`/`TextureChannel`/`ChannelPackPlane`/`TextureSetDraft` bundle it re-mints, the `plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid` substrate with its paired-policy mip rebuild and its `AsImage` sampler bridge, the `codec#RASTER_FAULT` band-2460 `Tile` channel, the kernel `SpectralArena` transform band for the periodicity spectrum, MathNet.Numerics `Distributions.Normal` for the Gaussian round trip, QuikGraph's reversed-edge augmentor and Edmonds-Karp solver for the minimum-cut seam, the kernel `Deterministic` splitmix64 stream for every jitter, `ValidityClaim` for the validity fold, and `TimeProvider` for the one measured wall time — reminting no graph algorithm, no distribution, no transform, no clock, and no random source.

## [01]-[INDEX]

- [02]-[TILE_SYNTH]: the `WangEdge` plan carrier, the `TilePolicy` row, the `TileStrategy` four-row solver table with its `offsetHeal`/`graphCut`/`wang`/`histogramBlend` kernels, the `TilePlan` channel-independent geometry, the one `TileKernel.Fold` applicator, and the one `TileSynth.Tileify` set fold.
- [03]-[TILE_GATE]: the `TileScore` measurement, the `TileProof` tileability evidence, the bounded kernel-transform-band lattice read, the base-boundary wrap-gradient ratio, the `TileProof.Grade` deterministic verdict over the `TileGate` measurement kernels, and the learned-scorer SPIKE beside it.

## [02]-[TILE_SYNTH]

- Owner: `TileLane` the DECLARED draw ordinals every jitter addresses through; `SeamAxis` the axis a seam line or healing band runs along; `TileSynth` the set-coherent tiling fold; `TileStrategy` `[SmartEnum<string>]` the four solver rows; `TilePlan` the channel-independent geometry; `TilePolicy` the caller's policy row; `WangEdge` the per-tile edge-digit assignment; `TileRun` the run record.
- Cases: lane {`atlas`, `strip`} · axis {`vertical`, `horizontal`} · strategy {`offsetHeal` (half-offset wrap, per-row minimum-error dynamic-programming cut, feathered band), `graphCut` (grid minimum-cut over the overlap band by Edmonds-Karp residual reachability), `wang` (the COMPLETE colours⁴ edge-coloured family assembled into one atlas, boundary bands drawn from per-colour strips), `histogramBlend` (per-lane quantile-to-Gaussian transform over `filter#PLANE_OP` `OrderStatistics`, variance-preserving weighted blend, interpolated inverse)}.
- Law: the guide channel decides the geometry alone. A set whose guide channel is absent REFUSES rather than falling back to an arbitrary present channel, because a plan derived from a different field than the caller graded is a plan the run misdescribes; a LAYERED set refuses outright, since the sampler bridge carries one layer and a cube face tiled independently of its neighbours has no shared boundary at all.
- Entry: `public static Fin<(TextureSet Set, TileRun Run)> Tileify(TextureSet set, TilePolicy policy, Op key, TimeProvider? clock = null)` is the ONE entry — it solves the plan from the guide inside `Op.Catch` so a QuikGraph refusal remains exact, folds `Apply` over every channel and pack plane in PAIRING ORDER, re-admits the result through `set#TEXTURE_SET` `TextureSet.Of` so the tiled set re-keys, and grades it through `[03]-[TILE_GATE]`; `public static Fin<TexturePyramid> Apply(TilePlan plan, TexturePyramid pyramid, Op key, Option<TexturePyramid> paired = default)` is the per-plane fold the set fold drives and the only surface a caller composing a single plane reaches, and it refuses an extent mismatch or a layered plane rather than reading past a single-layer fold.
- Packages: QuikGraph (composed — `AdjacencyGraph<int, Edge<int>>` the overlap-band flow graph, `Edge<int>` the REFERENCE edge, `ReversedEdgeAugmentorAlgorithm<int, Edge<int>>.AddReversedEdges` under a `using` whose own dispose retires them, `EdmondsKarpMaximumFlowAlgorithm<int, Edge<int>>(graph, capacities, edgeFactory, augmentor)`/`Compute`/`ResidualCapacities` the augmenting solve and the residual capacities the cut walk reads), MathNet.Numerics (composed — `Distributions.Normal.InvCDF`/`Normal.CDF` the Gaussian round trip), `Rasm.Numerics` (composed — `SpectralArena.Interleaved`/`Transform` and `Spectrum.Power` the kernel transform band `[03]` grades on), `Rasm` (project — `Deterministic.Of`/`Draw.At`/`Draw.Unit` the lane-keyed stateless draw and `IDrawLane<TSelf>` the declared-ordinal floor, `Dimension`, `Op`, `ValidityClaim`/`ValidityClaim.Evidence`/`WhenPresent`, `Evidence`/`Evidence.Of` the kernel probe evidence the `Tiled` fill and the run's `Score` ride), `plane#TEXTURE_PLANE` (composed — `TexturePlane`/`TexturePyramid`/`PlaneFormat`/`MipPolicy` and the `AsImage` decoded-level bridge), `set#TEXTURE_SET` (composed — `TextureSet`/`TextureChannel`/`ChannelPackPlane`/`TextureSetDraft`), `codec#RASTER_FAULT` (composed — `RasterFault` band 2460), `Rasm.Materials.Appearance.Texture` (composed — `ShadeVec4`), CommunityToolkit.HighPerformance (`ReadOnlyMemory2D<T>`/`ReadOnlySpan2D<T>` the plan's blend field and every staging plane, `SpanOwner<T>.Allocate` the per-row scratch the plane write accessor takes), BCL (`TimeProvider`, `ReferenceEqualityComparer`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new tiling method is one `TileStrategy` row carrying its `Solve` delegate — the plan shape already carries an offset, a per-texel blend field, and a quadrant table, so a method combining them differently needs no new carrier; a new policy knob is one `TilePolicy` column; a new plan geometry is one `TilePlan` column every strategy row leaves at its identity. There is NO per-strategy tiler type, NO `SeamlessTexture` surface, and NO per-channel tiling entry — the named defect is a second synthesizer, and the repair is a row.
- Law: `TilePlan` carries exactly what `Fold` CONSUMES — a wrap offset, a `[0,1]` blend field with its band width, and the Wang table — and never a colour, a histogram, or a channel reference, which is what makes one plan legal over every plane of the roster at four component counts and five transfers. The two per-axis cuts ride on the run alone, folded into the field at solve time, because a plan whose applicator could honour a cut and drop the field is how a computed seam stops reaching the output.
- Law: the half-offset wrap makes a CROSS seam, so `offsetHeal` and `graphCut` each cut BOTH axes and the field is the union of the two ramps — a single-axis heal ships the horizontal seam untouched. Each texel takes two candidates, the plan's offset tap and its DOUBLE-offset tap, which for the canonical half-extent offset are the wrapped and the unshifted source; the field is the mix between them, identity-zero away from every seam, so the interior is the wrapped image untouched.
- Law: the `histogramBlend` quantile transform is the ONE per-channel quantity and it is derived INSIDE `Fold` from that channel's own plane, PER LANE, against the SHARED blend field, through `filter#PLANE_OP` `OrderStatistics` rather than a transcription of it. The forward map interpolates that owner's stride-sampled order statistics into `Normal.InvCDF`, so the blend runs in a genuine standard-Gaussian space and inverts back through `Normal.CDF` into the channel's own sample values — a bare CDF reaches a UNIFORM space where a linear mix is not variance-preserving and the seam band regresses toward its mean, and a fixed-bin unit-range histogram quantizes the float substrate and clips every scene-linear value above one. That owner's HAZEN plotting position keeps the extremes finite under `Normal.InvCDF`, where the kernel `Distribution.Of` R-7 convention answers a reported-percentile question and pins them at 0 and 1 — two owners for two questions, never one drift.
- Law: the `wang` atlas is EXTENT-PRESERVING and COMPLETE — every `colours⁴` edge combination across a `colours² × colours²` grid of the source extent, because a family whose opposite edges derive from each other holds only `colours` distinct pairs and cannot tile aperiodically. `Fold` reads each tile's interior from its own origin while the boundary bands read PER-COLOUR strips whose origins derive from the seed and the edge colour alone, so two tiles sharing a digit share the strip byte-for-byte and every legal adjacency is seamless by construction; the four-colour corner meet is feathered by the same ramp and MEASURED at `[03]` into `TileScore.CornerResidual`, so the family's known bound rides the proof as a number rather than as a sentence.
- Law: every strategy runs in the plane's DECODED domain — the `AsImage` lift decodes an `srgb` plane to scene-linear before any difference metric — so a seam cost computed on encoded values, which weights dark texels by the transfer's own curve, is unrepresentable. Jitter is REPLAYABLE and LANE-KEYED: every draw addresses `Deterministic.Of(seed, TileLane.X)` at a DECLARED ordinal and takes its unit off a `Draw` prefix, so a re-solve at one seed produces byte-identical geometry, an atlas origin never shares a stream with a strip origin, and no tile's draw depends on the iteration that reached it. The deleted default seed was the draw owner's own splitmix64 gamma word transcribed into a policy column — the second-splitmix that owner's Boundary law forecloses (`SEEDED_FROM_STRING_HASH`) — so the default is now the federation's zero and the lanes do the separating.
- Law: the `graphCut` band is a GRID FLOW GRAPH whose vertices are the overlap texels with a source and a sink, and an edge's capacity is the matching cost at both endpoints. BOTH directions of every neighbour pair enter as SEPARATE `Edge<int>` instances at one capacity, because an undirected minimum cut needs both arcs saturable; the augmentor mints one reverse per arc and the capacity map is keyed by REFERENCE, where a value-typed edge would alias each augmentor reverse onto the arc it duplicates and hand the solver twice the residual capacity it should have. Source and sink arcs carry a FINITE bound one above the band's own summed cost, because Edmonds-Karp sums capacities along augmenting paths and two infinite arcs on one path overflow every residual comparison downstream. The cut reads a residual-capacity breadth-first walk from the source rather than the algorithm's own vertex colouring, so the seam derives from published capacities and never from an internal traversal state. The SOLVER OBJECT is bound directly rather than through `AlgorithmExtensions.MaximumFlow`, because that extension surfaces the flow value and the predecessor accessor alone and a cut needs `ResidualCapacities`; the augmentor's `using` scope IS the auxiliary-edge lifecycle, so a hand `RemoveReversedEdges` beside it retires the reverses twice.
- Law: `graphCut` IS THE COST of this page and its bound is DECLARED. The flow graph carries `lines × (2·Overlap + 1)` vertices with four arcs each and one augmentor reverse per arc — at the default overlap on a 16k plane that is 1.1 million vertices and some 8.5 million arcs, and Edmonds-Karp's augmenting search is superlinear in both. `TilePolicy.Overlap` is therefore the knob that bounds the solve rather than a feathering preference, and the `benchmarks#BENCH_KERNEL` tiling workload grades the pair: a caller whose extent makes the cut dominate takes `offsetHeal`, whose dynamic-programming walk is exactly `lines × (2·Overlap + 1)` cost evaluations with no graph at all and whose cut follows the same structure the flow would have found on all but the hardest fields. Both solve ONCE per set, never per channel, which is the whole reason the plan is channel-independent.
- Law: TILING PAYS TWO WHOLE-PLANE RESIDENCIES PER PLANE and cannot band. `AsImage` materializes the chain at its declared cost and `Fold` allocates one more `ShadeVec4` run for the target, so a 4k plane holds roughly 683 MiB of chain plus 512 MiB of target while it is being tiled, sequentially, one plane at a time. Banding is structurally unavailable rather than merely unimplemented: the half-extent offset makes every texel's second candidate a tap half a plane away, and the Wang strip reads reach an arbitrary seeded origin, so no row band holds both candidates of any texel. The bound is therefore the LARGEST SINGLE PLANE rather than the set, which is what keeps a full-channel set tileable at extents a simultaneous fold could not reach.
- Law: planes fold in PAIRING ORDER and that order is a DEPENDENCY WALK over the `TextureChannel.Pair` relation, not a two-bucket sort — a paired channel's rebuild reads its companion's already-tiled chain, and that companion may itself be paired. A cycle terminates by rebuilding the second of a mutually paired pair unpaired, which is the `Coupled: false` floor the mip policy already publishes.
- Boundary: wall time rides an OPTION-shaped `TimeProvider`, so a run reports a measured elapsed or the caller's own clock, never a literal zero a benchmark reads as instantaneous and never a null a boundary re-tests. Every loop-bearing member states its own KERNEL-EXEMPTION at the loop and each names the shape no span operator reaches — a sequential recurrence, a graph build, a generator, a two-candidate applicator, or a streaming row accessor; every admission, dispatch, and egress surface on the page is expression-bodied.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.Distributions;
using QuikGraph;
using QuikGraph.Algorithms.MaximumFlow;
using Rasm.Domain;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TileLane : IDrawLane<TileLane> {
    public static readonly TileLane Atlas = new(key: 0, lane: 0L);
    public static readonly TileLane Strip = new(key: 1, lane: 1L);

    public long Lane { get; }
}

[SmartEnum<int>]
public sealed partial class SeamAxis {
    public static readonly SeamAxis Vertical = new(key: 0);
    public static readonly SeamAxis Horizontal = new(key: 1);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileStrategy {
    public static readonly TileStrategy OffsetHeal     = new("offsetHeal",     solve: TileKernel.HealCut);
    public static readonly TileStrategy GraphCut       = new("graphCut",       solve: TileKernel.CutBand);
    public static readonly TileStrategy Wang           = new("wang",           solve: TileKernel.WangAssign);
    public static readonly TileStrategy HistogramBlend = new("histogramBlend", solve: TileKernel.BlendField);

    [UseDelegateFromConstructor]
    public partial TilePlan Solve(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WangEdge(int Tile, int North, int East, int South, int West, int OriginX, int OriginY);

public sealed record TilePolicy(
    TileStrategy Strategy, TextureChannel Guide, int Overlap, double AcceptScore, ulong Seed, int WangColors, int GradeEdge,
    Option<Func<ReadOnlyMemory2D<ShadeVec4>, double>> Scorer = default) {
    public static TilePolicy Default(TextureChannel guide) =>
        new(TileStrategy.GraphCut, guide, Overlap: 32, AcceptScore: 0.85, Seed: 0UL, WangColors: 2, GradeEdge: 256);
}

public sealed record TilePlan(
    TileStrategy Strategy, Dimension Width, Dimension Height, int OffsetX, int OffsetY, int Band,
    ReadOnlyMemory2D<float> Blend, Seq<WangEdge> Wang, Seq<int> Cut, Seq<int> CutY, ulong Seed);

public sealed record TileRun(
    TileStrategy Strategy, TextureChannel Guide, Evidence<TileScore> Score, Seq<int> Cut, Seq<int> CutY, Seq<WangEdge> Wang,
    int Planes, ulong Seed, double ElapsedMs) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Planes, 1),
        ValidityClaim.Nonnegative(ElapsedMs),
        ValidityClaim.Evidence(Score.Value()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TileSynth {
    public static Fin<(TextureSet Set, TileRun Run)> Tileify(
        TextureSet set, TilePolicy policy, Op key, Option<TimeProvider> clock = default) {
        TimeProvider ticks = clock.IfNone(TimeProvider.System);
        long opened = ticks.GetTimestamp();
        return from _ in guard(set.Layers.Value is 1, new RasterFault.Tile(key, $"<layered-set-has-no-shared-boundary:{set.Law.Key}:{set.Layers.Value}>"))
               from guide in set.Channels.Find(policy.Guide).ToFin(new RasterFault.Tile(key, $"<tile-guide-absent:{policy.Guide.Key}>"))
               from image in guide.AsImage(key)
               from plan in key.Catch(() => Fin.Succ(
                   policy.Strategy.Solve(image.Levels[0].AsMemory2D(guide.Base.Height.Value, guide.Base.Width.Value), policy, policy.Seed)))
               from channels in PairingOrder(set.Channels)
                   .Fold(Fin.Succ(HashMap<TextureChannel, TexturePyramid>.Empty), (acc, row) =>
                       acc.Bind(map => set.Channels.Find(row).ToFin(new RasterFault.Tile(key, $"<tile-channel-lost:{row.Key}>"))
                           .Bind(pyramid => Apply(plan, pyramid, key, Companion(row, set.Channels).Bind(map.Find))
                               .Map(tiled => map.Add(row, tiled)))))
               from packs in set.Packs.Fold(Fin.Succ(Seq<ChannelPackPlane>()), (acc, pack) =>
                   acc.Bind(rows => Apply(plan, pack.Plane, key).Map(tiled => rows.Add(pack with { Plane = tiled }))))
               from regraded in channels.Find(policy.Guide).ToFin(new RasterFault.Tile(key, "<tile-guide-lost>"))
               let scored = TileProof.Grade(regraded, policy, key)
               let graded = Evidence.Of(scored)
               from tiled in TextureSet.Of(new TextureSetDraft(set.Width, set.Height, set.Layers, set.Law, set.Convention,
                   set.Alpha, set.HeightScaleMm, graded, set.Udim, channels, packs, set.Conductor, set.Material), key)
               select (tiled, new TileRun(policy.Strategy, policy.Guide, Evidence.Of(scored.Map(static p => p.Score)),
                   plan.Cut, plan.CutY, plan.Wang, channels.Count + packs.Count, policy.Seed, ticks.GetElapsedTime(opened).TotalMilliseconds));
    }

    private static Seq<TextureChannel> PairingOrder(HashMap<TextureChannel, TexturePyramid> planes) =>
        toSeq(planes.Keys.OrderBy(static row => row.Ordinal))
            .Fold((Order: Seq<TextureChannel>.Empty, Seen: Set<TextureChannel>.Empty), (state, row) => Emit(row, planes, state))
            .Order;

    private static (Seq<TextureChannel> Order, Set<TextureChannel> Seen) Emit(
        TextureChannel row, HashMap<TextureChannel, TexturePyramid> planes,
        (Seq<TextureChannel> Order, Set<TextureChannel> Seen) state) {
        if (state.Seen.Contains(row)) { return state; }
        (Seq<TextureChannel> Order, Set<TextureChannel> Seen) entered = (state.Order, state.Seen.Add(row));
        (Seq<TextureChannel> Order, Set<TextureChannel> Seen) resolved =
            Companion(row, planes).Match(Some: companion => Emit(companion, planes, entered), None: () => entered);
        return (resolved.Order.Add(row), resolved.Seen);
    }

    private static Option<TextureChannel> Companion(TextureChannel row, HashMap<TextureChannel, TexturePyramid> planes) =>
        row.Pair.Bind(static name => TextureChannel.TryGet(name, out TextureChannel? companion) ? Some(companion) : None)
            .Filter(planes.ContainsKey);

    public static Fin<TexturePyramid> Apply(TilePlan plan, TexturePyramid pyramid, Op key, Option<TexturePyramid> paired = default) =>
        pyramid.Base.Width != plan.Width || pyramid.Base.Height != plan.Height
            ? Fin.Fail<TexturePyramid>(new RasterFault.Tile(key, $"<tile-plan-extent-mismatch:{plan.Width.Value}x{plan.Height.Value}>"))
            : pyramid.Base.Layers.Value is not 1
                ? Fin.Fail<TexturePyramid>(new RasterFault.Tile(key, $"<tile-plane-layered:{pyramid.Base.Layers.Value}>"))
                  : from image in pyramid.AsImage(key)
                  from top in TexturePlane.Of(pyramid.Base.Format, pyramid.Base.Grid, pyramid.Base.Layers,
                      pyramid.Base.Transfer, pyramid.Base.Alpha, pyramid.Base.Range, pyramid.Base.Primaries,
                      key, AllocationMode.Default)
                  from chain in TexturePyramid.Of(
                          Fill(top, TileKernel.Fold(plan, image.Levels[0].AsMemory2D(pyramid.Base.Height.Value, pyramid.Base.Width.Value))),
                          pyramid.Policy, key, paired)
                      .Rollback(top)
                  select chain;

    static TexturePlane Fill(TexturePlane plane, ReadOnlyMemory2D<ShadeVec4> texels) {
        ReadOnlySpan2D<ShadeVec4> source = texels.Span;
        for (int layer = 0; layer < plane.Layers.Value; layer++) {
            for (int row = 0; row < plane.Height.Value; row++) {
                plane.WriteShade(row, layer, source.GetRowSpan((layer * plane.Height.Value) + row));
            }
        }
        return plane;
    }
}

internal static class TileKernel {
    internal static TilePlan HealCut(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, band = Math.Min(policy.Overlap, Math.Min(w, h) / 4);
        ReadOnlySpan2D<ShadeVec4> src = guide.Span;
        int[] cut = new int[h];
        int[] cutY = new int[w];
        MinErrorPath(src, w / 2, band, h, w, cut, SeamAxis.Vertical);
        MinErrorPath(src, h / 2, band, w, h, cutY, SeamAxis.Horizontal);
        return new TilePlan(TileStrategy.OffsetHeal, Dimension.Create(w), Dimension.Create(h), w / 2, h / 2, band,
            Ramped(w, h, cut, cutY, band), Seq<WangEdge>(), toSeq(cut), toSeq(cutY), seed);
    }

    static void MinErrorPath(ReadOnlySpan2D<ShadeVec4> src, int centre, int band, int lines, int span, Span<int> cut, SeamAxis axis) {
        int lo = Math.Max(1, centre - band), hi = Math.Min(span - 2, centre + band), width = hi - lo + 1;
        double[] prev = new double[width], next = new double[width];
        int[,] back = new int[lines, width];
        for (int line = 0; line < lines; line++) {
            for (int c = 0; c < width; c++) {
                int i = lo + c;
                double local = axis == SeamAxis.Vertical ? Seam(src, line, i, span, axis) : Seam(src, i, line, span, axis);
                if (line is 0) { next[c] = local; back[line, c] = c; continue; }
                int best = c;
                for (int d = Math.Max(0, c - 1); d <= Math.Min(width - 1, c + 1); d++) { if (prev[d] < prev[best]) { best = d; } }
                next[c] = local + prev[best];
                back[line, c] = best;
            }
            (prev, next) = (next, prev);
        }
        int tail = 0;
        for (int c = 1; c < width; c++) { if (prev[c] < prev[tail]) { tail = c; } }
        for (int line = lines - 1; line >= 0; line--) { cut[line] = lo + tail; tail = back[line, tail]; }
    }

    internal static TilePlan CutBand(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, band = Math.Min(policy.Overlap, Math.Min(w, h) / 4);
        ReadOnlySpan2D<ShadeVec4> src = guide.Span;
        int[] cut = AxisCut(src, w / 2, band, h, w, SeamAxis.Vertical);
        int[] cutY = AxisCut(src, h / 2, band, w, h, SeamAxis.Horizontal);
        return new TilePlan(TileStrategy.GraphCut, Dimension.Create(w), Dimension.Create(h), w / 2, h / 2, band,
            Ramped(w, h, cut, cutY, band), Seq<WangEdge>(), toSeq(cut), toSeq(cutY), seed);
    }

    static int[] AxisCut(ReadOnlySpan2D<ShadeVec4> src, int centre, int band, int lines, int span, SeamAxis axis) {
        int lo = centre - band, width = (2 * band) + 1, source = width * lines, sink = source + 1;
        AdjacencyGraph<int, Edge<int>> flow = new(allowParallelEdges: true);
        Dictionary<Edge<int>, double> capacity = new(ReferenceEqualityComparer.Instance);
        double total = 0.0;
        void Link(int a, int b, double c) { Edge<int> e = new(a, b); flow.AddVerticesAndEdge(e); capacity[e] = c; }
        double At(int line, int i) => axis == SeamAxis.Vertical ? Seam(src, line, i, span, axis) : Seam(src, i, line, span, axis);
        for (int line = 0; line < lines; line++) {
            for (int c = 0; c < width; c++) {
                int i = lo + c;
                if (c + 1 < width) { total += At(line, i) + At(line, i + 1); }
                if (line + 1 < lines) { total += At(line, i) + At(line + 1, i); }
            }
        }
        double bound = total + 1.0;
        for (int line = 0; line < lines; line++) {
            for (int c = 0; c < width; c++) {
                int v = (line * width) + c, i = lo + c;
                if (c is 0) { Link(source, v, bound); }
                if (c == width - 1) { Link(v, sink, bound); }
                if (c + 1 < width) { double m = At(line, i) + At(line, i + 1); Link(v, v + 1, m); Link(v + 1, v, m); }
                if (line + 1 < lines) { double m = At(line, i) + At(line + 1, i); Link(v, v + width, m); Link(v + width, v, m); }
            }
        }
        using ReversedEdgeAugmentorAlgorithm<int, Edge<int>> augmentor = new(flow, static (a, b) => new Edge<int>(a, b));
        augmentor.AddReversedEdges();
        EdmondsKarpMaximumFlowAlgorithm<int, Edge<int>> solve = new(flow, e => capacity.TryGetValue(e, out double c) ? c : 0.0, static (a, b) => new Edge<int>(a, b), augmentor);
        solve.Compute(source, sink);
        return Reachable(flow, solve.ResidualCapacities, source, width, lines, lo);
    }

    static int[] Reachable(AdjacencyGraph<int, Edge<int>> flow, IDictionary<Edge<int>, double> residual, int source, int width, int height, int lo) {
        HashSet<int> seen = [source];
        Queue<int> open = new([source]);
        while (open.Count > 0) {
            int v = open.Dequeue();
            foreach (Edge<int> e in flow.OutEdges(v)) { if (residual.TryGetValue(e, out double r) && r > 0.0 && seen.Add(e.Target)) { open.Enqueue(e.Target); } }
        }
        int[] cut = new int[height];
        for (int y = 0; y < height; y++) {
            int reached = 0;
            for (int c = 0; c < width; c++) { if (seen.Contains((y * width) + c)) { reached++; } }
            cut[y] = lo + Math.Min(reached, width - 1);
        }
        return cut;
    }

    internal static TilePlan WangAssign(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, colours = Math.Max(2, policy.WangColors);
        int grid = colours * colours, tiles = grid * grid;
        int cw = Math.Max(1, w / grid), ch = Math.Max(1, h / grid);
        int band = Math.Min(policy.Overlap, Math.Min(cw, ch) / 4);
        Deterministic.Draw atlas = Deterministic.Of(unchecked((long)seed), TileLane.Atlas);
        WangEdge[] rows = new WangEdge[tiles];
        for (int t = 0; t < tiles; t++) {
            rows[t] = new WangEdge(t,
                North: t % colours, East: (t / colours) % colours,
                South: (t / (colours * colours)) % colours, West: (t / (colours * colours * colours)) % colours,
                OriginX: (int)(atlas.At(t, 0).Unit * w), OriginY: (int)(atlas.At(t, 1).Unit * h));
        }
        float[] field = new float[w * h];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                int d = Math.Min(Math.Min(x % cw, cw - 1 - (x % cw)), Math.Min(y % ch, ch - 1 - (y % ch)));
                field[(y * w) + x] = (float)(band > 0 && d < band ? 1.0 - (d / (double)band) : 0.0);
            }
        }
        return new TilePlan(TileStrategy.Wang, Dimension.Create(w), Dimension.Create(h), 0, 0, band,
            new ReadOnlyMemory2D<float>(field, h, w), toSeq(rows), Seq<int>(), Seq<int>(), seed);
    }

    internal static TilePlan BlendField(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, band = Math.Min(policy.Overlap, Math.Min(w, h) / 4);
        float[] field = new float[w * h];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double u = Math.Max(Ramp(x, w / 2, band), Ramp(y, h / 2, band));
                field[(y * w) + x] = (float)(u > 0.0 ? u / Math.Sqrt((u * u) + ((1.0 - u) * (1.0 - u))) : 0.0);
            }
        }
        return new TilePlan(TileStrategy.HistogramBlend, Dimension.Create(w), Dimension.Create(h), w / 2, h / 2, band,
            new ReadOnlyMemory2D<float>(field, h, w), Seq<WangEdge>(), Seq<int>(), Seq<int>(), seed);
    }

    internal static ReadOnlyMemory2D<ShadeVec4> Fold(TilePlan plan, ReadOnlyMemory2D<ShadeVec4> plane) {
        int w = plane.Width, h = plane.Height;
        ShadeVec4[] target = new ShadeVec4[w * h];
        ReadOnlySpan2D<ShadeVec4> src = plane.Span;
        ReadOnlySpan2D<float> blend = plan.Blend.Span;
        OrderStatistics? lut = plan.Strategy == TileStrategy.HistogramBlend ? OrderStatistics.Of(src) : null;
        int grid = plan.Wang.IsEmpty ? 0 : (int)Math.Round(Math.Sqrt(plan.Wang.Count));
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double t = blend[y, x];
                ShadeVec4 a, b;
                if (grid > 0) {
                    (a, b) = WangTaps(plan, src, grid, w, h, x, y);
                } else {
                    a = src[(y + plan.OffsetY) % h, (x + plan.OffsetX) % w];
                    b = src[(y + (2 * plan.OffsetY)) % h, (x + (2 * plan.OffsetX)) % w];
                }
                target[(y * w) + x] = lut is null ? ShadeVec4.Lerp(a, b, t) : Inverse(lut, ShadeVec4.Lerp(Gauss(lut, a), Gauss(lut, b), t));
            }
        }
        return new ReadOnlyMemory2D<ShadeVec4>(target, h, w);
    }

    static (ShadeVec4 Interior, ShadeVec4 Strip) WangTaps(TilePlan plan, ReadOnlySpan2D<ShadeVec4> src, int grid, int w, int h, int x, int y) {
        int cw = Math.Max(1, w / grid), ch = Math.Max(1, h / grid);
        int cellX = (x / cw) % grid, cellY = (y / ch) % grid;
        WangEdge tile = plan.Wang[(cellY * grid) + cellX];
        int lx = x % cw, ly = y % ch;
        ShadeVec4 interior = src[(tile.OriginY + ly) % h, (tile.OriginX + lx) % w];
        int dN = ly, dS = ch - 1 - ly, dW = lx, dE = cw - 1 - lx;
        int d = Math.Min(Math.Min(dN, dS), Math.Min(dW, dE));
        (int colour, SeamAxis run, int off) =
            d == dN ? (tile.North, SeamAxis.Horizontal, ly)
            : d == dS ? (tile.South, SeamAxis.Horizontal, ly - ch)
            : d == dW ? (tile.West, SeamAxis.Vertical, lx)
            : (tile.East, SeamAxis.Vertical, lx - cw);
        (int ox, int oy) = StripOrigin(plan.Seed, colour, run, w, h);
        ShadeVec4 strip = run == SeamAxis.Horizontal
            ? src[(((oy + off) % h) + h) % h, (ox + x) % w]
            : src[(oy + y) % h, (((ox + off) % w) + w) % w];
        return (interior, strip);
    }

    static (int X, int Y) StripOrigin(ulong seed, int colour, SeamAxis axis, int w, int h) {
        Deterministic.Draw strip = Deterministic.Of(unchecked((long)seed), TileLane.Strip).At(axis.Key, colour);
        return ((int)(strip.At(0).Unit * w), (int)(strip.At(1).Unit * h));
    }

    static double Cost(ShadeVec4 a, ShadeVec4 b) { ShadeVec4 d = a + (b * -1.0); return (d.X * d.X) + (d.Y * d.Y) + (d.Z * d.Z) + (d.W * d.W); }
    static double Seam(ReadOnlySpan2D<ShadeVec4> src, int y, int x, int span, SeamAxis axis) =>
        axis == SeamAxis.Vertical
            ? Cost(src[y, ((x % span) + span) % span], src[y, (((x + (span / 2)) % span) + span) % span])
            : Cost(src[((y % span) + span) % span, x], src[(((y + (span / 2)) % span) + span) % span, x]);
    static double Ramp(int i, int centre, int band) => band <= 0 ? 0.0 : Math.Clamp(1.0 - (Math.Abs(i - centre) / (double)band), 0.0, 1.0);
    static ReadOnlyMemory2D<float> Ramped(int w, int h, ReadOnlySpan<int> cut, ReadOnlySpan<int> cutY, int band) {
        float[] f = new float[w * h];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) { f[(y * w) + x] = (float)Math.Max(Ramp(x, cut[y], band), Ramp(y, cutY[x], band)); }
        }
        return new ReadOnlyMemory2D<float>(f, h, w);
    }

    static ShadeVec4 Gauss(OrderStatistics lut, ShadeVec4 v) =>
        new(Normal.InvCDF(0.0, 1.0, lut.Quantile(0, v.X)), Normal.InvCDF(0.0, 1.0, lut.Quantile(1, v.Y)), Normal.InvCDF(0.0, 1.0, lut.Quantile(2, v.Z)), v.W);
    static ShadeVec4 Inverse(OrderStatistics lut, ShadeVec4 v) =>
        new(lut.Value(0, Normal.CDF(0.0, 1.0, v.X)), lut.Value(1, Normal.CDF(0.0, 1.0, v.Y)), lut.Value(2, Normal.CDF(0.0, 1.0, v.Z)), v.W);
}
```

## [03]-[TILE_GATE]

- Owner: `TileProof` the minted evidence a `set#TEXTURE_SET` `TextureSet` carries, holding its own `Grade` mint; `TileGate` the measurement kernels; `TileScore` the measurement row.
- Law: `TileProof` has no construction outside its own `Grade` — the constructor is PRIVATE and the mint is the type's own static, so no assembly-wide internal factory widens the reach — and `TextureSet.Tiled` therefore cannot be asserted, only earned; an ingested third-party set claiming tileability in its own manifest carries `Absent` until it is graded here.
- Entry: `public static Fin<TileProof> TileProof.Grade(TexturePyramid pyramid, TilePolicy policy, Op key)` is the ONE probe — a grade is EVIDENCE, never a verdict, so a plane that tiles badly still MEASURES and the caller decides; deterministic signals read the plane's own decoded row accessor, and a learned scorer preserves its exact exceptional error while a non-finite answer fails as `RasterFault.Tile`.
- Packages: `Rasm.Numerics` (composed — `SpectralArena.Interleaved`/`Transform`/`SpectralSense.Forward`/`SpectralScaling.Symmetric` the kernel transform band over the bounded luminance staging, `Spectrum.Power` the per-bin read), `plane#TEXTURE_PLANE` (composed — `TexturePyramid.Levels` the bounded grading level, `TexturePlane.Grid` the arena's own lattice, `TexturePlane.Read` the streaming decoded accessor both signals fold), CommunityToolkit.HighPerformance (`SpanOwner<T>` the row rentals), BCL inbox (`System.Numerics.Complex`), `Rasm` (project — `ValidityClaim`, `Evidence`/`Evidence.Of` the kernel probe evidence the `Learned` column and the `Tiled` fill ride).
- Law: A GRADE IS ALWAYS PUBLISHED. `Grade` mints the proof it measured whatever that measurement says, `TileProof.AcceptBar` records the bar it was graded against, and `Accepted` is a predicate over the pair — so a below-bar set carries the real score it earned and a consumer can read how far short it fell. `TileSynth.Tileify` folds the probe outcome onto the set's `Tiled` column through the kernel `Evidence.Of(Fin<TileProof>)` mint, so the column holds three facts apart: `Measured` a grade that ran, its own `Accepted` the acceptance read; `Refused` a spectral band that rejected, carrying the band's own cause; `Absent` a set nothing ever graded. The deleted `Option` let a refused band, a below-bar grade, and an ungraded ingest all wear one `None`, and tileability stays earned evidence — it is now the measured-and-accepted read rather than bare presence.
- Law: `TileScore.Learned` is the OPTIONAL fourth signal, RECORDED and never graded. `TilePolicy.Scorer` carries it as a CLOSURE the appearance frontier supplies — this owner sits below that frontier and names no model type, exactly as a press subject carries a radiance closure without naming a sky — and the frontier resolves the card, its licence class, and its tensor contract on its own side. The deterministic pair stays authoritative because it alone is reproducible across machines: folding a learned number into `Value` would make a stored proof unverifiable, and a retired weight file would take the verdict with it. The column rides kernel `Evidence<double>`: a scorer nothing supplied never ran and reads `Absent`, while a scorer that throws or answers a non-finite number RAN and reads `Refused` with its own cause — never faulting a grade whose reproducible half succeeded, and never masquerading as a scorer nobody configured.
- Growth: a new tileability signal is one `TileScore` column and one term in the combined verdict — `CornerResidual` and `Learned` are RECORDED columns and NOT verdict terms, because the Wang corner bound is a family property to publish rather than a defect to reject, and a plan with no corner meets reads absence; `CornerResidual` lawfully stays `Option<double>` because every `None` path is that ONE no-corner-meets state and no refusal path exists to conflate, where `Learned` rides `Evidence<double>` because an unsupplied scorer and a crashed one are two facts; the acceptance threshold and the spectral grading extent are the caller's `TilePolicy.AcceptScore` and `TilePolicy.GradeEdge` columns, never constants here.
- Boundary: the grade is TWO independent measurements against ONE verdict, because either alone is defeatable, and each reads the level its own signal lives at. `SeamRatio` compares the mean squared difference across the WRAP boundary against the mean squared difference between interior neighbours, measured on the BASE level — a seam is a high-frequency artefact a mip erases, so grading it on a reduced level grades away what is being tested — and the fold streams two rows at a time through the plane's own decoded accessor, so a 16k plane costs one row pair of memory rather than a full materialization. A plane whose wrap is exact has a boundary statistically indistinguishable from its interior, so the ratio approaches one and a visible seam drives it above one; a plane that is merely BLURRED at its border also scores well here, which is why it cannot be the whole verdict. `LatticeLeak` folds the kernel transform band over the pyramid level nearest the policy's own `GradeEdge` extent — the spectral signature of a discontinuity is scale-free, and a full-resolution transform over a 4k plane would stage sixteen million complex samples for a scalar answer — reads its per-bin power off `Spectrum.Power`, answers the band's own typed REFUSAL where the transform refuses (zero is what a perfectly periodic plane earns, so an unmeasured plane publishing it would invert the verdict, and `Grade` mints no proof on a refused second signal — the cause rides the probe outcome instead of vanishing into absence), and measures the MEDIAN axis-bin power rather than the total axis power, because the `kx = 0`/`ky = 0` cross carries two different things: a seam raises the whole axis as a broadband `1/k` floor, while a genuinely periodic pattern raises isolated axis HARMONICS. Reading total axis power fails every correctly-tiling brick, weave, and lattice; reading the median reads the floor those harmonics sit on. The combined `Value` is the product of the two normalized terms and the caller's own `AcceptScore` decides; `CornerResidual` rides beside them as RECORDED evidence rather than as a third term, measured as the mean squared difference across the four texels meeting at each interior atlas-cell corner and absent for every strategy with no corner meets, so an atlas publishes the bound its edge strips cannot resolve instead of grading seamless on two signals that never look at a corner; the learned tileability scorer is a SPIKE — an optional ONNX stage the `neural#PBR_STAGE` registry admits as a quality gate BESIDE this floor, never in place of it, because a scorer whose weights retire takes the deterministic verdict with it. Every measurement runs on the plane's LUMINANCE — the AP1 scene-linear `ShadeVec4.Luminance` weights, so a green-heavy pattern is not read as a red one — and the staging buffer is caller-owned and filled by index, the page's declared kernel exemption.

```csharp
using System.Numerics;
using Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TileScore(
    double SeamRatio, double LatticeLeak, double Value, Dimension GradedAt, Option<double> CornerResidual, Evidence<double> Learned)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(SeamRatio), ValidityClaim.UnitInterval(LatticeLeak), ValidityClaim.UnitInterval(Value),
        ValidityClaim.WhenPresent(CornerResidual, ValidityClaim.Nonnegative),
        ValidityClaim.WhenPresent(Learned.Value(), ValidityClaim.Finite));
}

public sealed record TileProof {
    private TileProof(TileStrategy strategy, TileScore score, ulong seed, double bar) =>
        (Strategy, Score, Seed, AcceptBar) = (strategy, score, seed, bar);
    public TileStrategy Strategy { get; }
    public TileScore Score { get; }
    public ulong Seed { get; }
    public double AcceptBar { get; }
    public bool Accepted => Score.Value >= AcceptBar;

    public static Fin<TileProof> Grade(TexturePyramid pyramid, TilePolicy policy, Op key) {
        TexturePlane spectral = TileGate.Level(pyramid, policy.GradeEdge);
        double ratio = TileGate.SeamRatio(pyramid.Base);
        double seam = 1.0 / (1.0 + Math.Max(0.0, ratio - 1.0));
        return TileGate.LatticeLeak(spectral).Map(leak =>
            new TileProof(policy.Strategy,
                new TileScore(ratio, leak, Math.Clamp(seam * (1.0 - leak), 0.0, 1.0), spectral.Width,
                    TileGate.CornerResidual(pyramid.Base, policy), TileGate.Learned(pyramid.Base, policy, key)),
                policy.Seed, policy.AcceptScore));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class TileGate {
    internal static TexturePlane Level(TexturePyramid pyramid, int gradeEdge) =>
        pyramid.Levels.Filter(level => Math.Max(level.Width.Value, level.Height.Value) >= gradeEdge).Last.IfNone(pyramid.Base);

    internal static Option<double> CornerResidual(TexturePlane plane, TilePolicy policy) {
        if (policy.Strategy != TileStrategy.Wang) { return None; }
        int colours = Math.Max(2, policy.WangColors), grid = colours * colours;
        int w = plane.Width.Value, h = plane.Height.Value;
        int cw = Math.Max(1, w / grid), ch = Math.Max(1, h / grid);
        if (grid < 2 || cw < 2 || ch < 2) { return None; }
        using SpanOwner<ShadeVec4> upper = SpanOwner<ShadeVec4>.Allocate(w);
        using SpanOwner<ShadeVec4> lower = SpanOwner<ShadeVec4>.Allocate(w);
        double total = 0.0;
        int met = 0;
        for (int cell = 1; cell < grid; cell++) {
            int y = cell * ch;
            if (y >= h) { break; }
            plane.ReadShade(row: y - 1, layer: 0, upper.Span);
            plane.ReadShade(row: y, layer: 0, lower.Span);
            for (int across = 1; across < grid; across++) {
                int x = across * cw;
                if (x >= w) { break; }
                total += Delta(upper.Span[x - 1], upper.Span[x]) + Delta(lower.Span[x - 1], lower.Span[x])
                    + Delta(upper.Span[x - 1], lower.Span[x - 1]) + Delta(upper.Span[x], lower.Span[x]);
                met++;
            }
        }
        return met > 0 ? Some(total / (4 * met)) : None;
    }

    internal static Evidence<double> Learned(TexturePlane plane, TilePolicy policy, Op key) =>
        policy.Scorer.Match(
            Some: scorer => {
                ShadeVec4[] staged = new ShadeVec4[plane.Width.Value * plane.Height.Value];
                for (int row = 0; row < plane.Height.Value; row++) {
                    plane.ReadShade(row, layer: 0, staged.AsSpan(row * plane.Width.Value, plane.Width.Value));
                }
                return Evidence.Of(key.Catch(() => {
                    double read = scorer(new ReadOnlyMemory2D<ShadeVec4>(staged, plane.Height.Value, plane.Width.Value));
                    return double.IsFinite(read)
                        ? Fin.Succ(read)
                        : Fin.Fail<double>(new RasterFault.Tile(key, $"<tile-learned-nonfinite:{read}>"));
                }));
            },
            None: static () => new Evidence<double>.Absent());

    internal static double SeamRatio(TexturePlane plane) {
        int w = plane.Width.Value, h = plane.Height.Value;
        using SpanOwner<ShadeVec4> first = SpanOwner<ShadeVec4>.Allocate(w);
        using SpanOwner<ShadeVec4> upper = SpanOwner<ShadeVec4>.Allocate(w);
        using SpanOwner<ShadeVec4> lower = SpanOwner<ShadeVec4>.Allocate(w);
        plane.ReadShade(row: 0, layer: 0, first.Span);
        double seam = 0.0, interior = 0.0;
        for (int y = 0; y < h; y++) {
            plane.ReadShade(row: y, layer: 0, upper.Span);
            seam += Delta(upper.Span[w - 1], upper.Span[0]);
            if (y + 1 >= h) { for (int x = 0; x < w; x++) { seam += Delta(upper.Span[x], first.Span[x]); } break; }
            plane.ReadShade(row: y + 1, layer: 0, lower.Span);
            for (int x = 0; x + 1 < w; x++) { interior += Delta(upper.Span[x], lower.Span[x]) + Delta(upper.Span[x], upper.Span[x + 1]); }
        }
        double normalizedSeam = seam / (w + h), normalizedInterior = interior / Math.Max(1, 2 * (w - 1) * (h - 1));
        return normalizedInterior > 0.0 ? normalizedSeam / normalizedInterior : normalizedSeam > 0.0 ? double.PositiveInfinity : 1.0;
    }

    internal static Fin<double> LatticeLeak(TexturePlane plane) {
        int w = plane.Width.Value, h = plane.Height.Value;
        using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(w);
        Complex[] arena = new Complex[w * h];
        for (int y = 0, at = 0; y < h; y++) {
            plane.ReadShade(row: y, layer: 0, field.Span);
            for (int x = 0; x < w; x++, at++) { arena[at] = new Complex(field.Span[x].Luminance, 0.0); }
        }
        return new SpectralArena.Interleaved(Values: arena, Lattice: plane.Grid)
            .Transform(SpectralSense.Forward, SpectralScaling.Symmetric)
            .Bind(static spectrum => spectrum.Power())
            .Map(power => AxisFloor(power, w, h));
    }

    private static double AxisFloor(Arr<double> power, int width, int height) {
        double[] axis = new double[width + height - 2];
        double total = 0.0;
        int taken = 0;
        for (int y = 0, at = 0; y < height; y++) {
            for (int x = 0; x < width; x++, at++) {
                if (y is 0 && x is 0) { continue; }
                total += power[at];
                if ((y is 0 || x is 0) && taken < axis.Length) { axis[taken++] = power[at]; }
            }
        }
        if (total <= 0.0 || taken is 0) { return 0.0; }
        Array.Sort(axis, 0, taken);
        return Math.Clamp(axis[taken / 2] * taken / total, 0.0, 1.0);
    }

    static double Delta(ShadeVec4 a, ShadeVec4 b) { ShadeVec4 d = a + (b * -1.0); return (d.X * d.X) + (d.Y * d.Y) + (d.Z * d.Z); }
}
```

## [04]-[RESEARCH]

(none)
