# [MATERIALS_TILE]

THE TILING SYNTHESIZER AND ITS DETERMINISTIC GATE. One `TileStrategy` `[SmartEnum<string>]` closes the tiling algebra — `offsetHeal` the wrap-and-heal minimum-error cut, `graphCut` the Kwatra minimum-cut seam over the overlap band, `wang` the edge-coloured aperiodic tile family laid out as one atlas, `histogramBlend` the Heitz-Neyret variance-preserving Gaussian blend — and each row is one `Solve` delegate producing a `TilePlan`, so a new tiling method is a row and never a second synthesizer. Tiling is SET-COHERENT by construction: the plan is derived ONCE from a single guide channel and carries only channel-independent geometry — a wrap offset, a per-texel `[0,1]` blend field, and a Wang quadrant table — so `TileSynth.Apply` folds the SAME geometry over every channel and pack plane in the set and a normal, height, or occlusion plane cannot acquire a seam its base colour does not have. One `TileGate.Grade` grades the result deterministically and mints the `TileProof` that is the ONLY way a `set#TEXTURE_SET` `TextureSet` acquires its `Tiled` column — a caller's assertion has no spelling that reaches it.

Tiling is PROCEDURAL, never learned: a diffusion or sampler-loop tiling stage does not exist on this page, because a learned generator leaks at its decoder boundary exactly where the seam must be exact, and a set whose channels were each tiled by an independent stochastic pass has no shared geometry at all. The learned scorer is a SPIKE beside the deterministic floor, never in place of it. The page composes the `set#TEXTURE_SET` `TextureSet`/`TextureChannel`/`ChannelPackPlane`/`TextureSetDraft` bundle it re-mints, the `plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid` substrate with its paired-policy mip rebuild and its `AsImage` sampler bridge, the `codec#RASTER_FAULT` band-2460 `Tile` rail, MathNet.Numerics `IntegralTransforms` for the periodicity spectrum and `Distributions.Normal` for the Gaussian round trip, QuikGraph's reversed-edge augmentor and Edmonds-Karp solver for the minimum-cut seam, the kernel `Deterministic` splitmix64 stream for every jitter, `ValidityClaim` for the receipt fold, and `TimeProvider` for the one measured wall time — reminting no graph algorithm, no distribution, no transform, no clock, and no random source.

## [01]-[INDEX]

- [02]-[TILE_SYNTH]: the `WangEdge` plan carrier, the `TilePolicy` row, the `TileStrategy` four-row solver table with its `offsetHeal`/`graphCut`/`wang`/`histogramBlend` kernels, the `TilePlan` channel-independent geometry, the one `TileKernel.Fold` applicator, and the one `TileSynth.Tileify` set fold.
- [03]-[TILE_GATE]: the `TileScore` measurement, the `TileProof` tileability evidence, the bounded `Fourier.Forward2D` lattice read, the base-boundary wrap-gradient ratio, the `TileGate.Grade` deterministic verdict, and the learned-scorer SPIKE beside it.

## [02]-[TILE_SYNTH]

- Owner: `TileSynth` the set-coherent tiling fold; `TileStrategy` `[SmartEnum<string>]` the four solver rows; `TilePlan` the channel-independent geometry; `TilePolicy` the caller's policy row; `WangEdge` the per-tile quadrant assignment; `TileReceipt` the run evidence.
- Cases: strategy {`offsetHeal` (half-offset wrap, per-row minimum-error dynamic-programming cut, feathered band), `graphCut` (grid minimum-cut over the overlap band by Edmonds-Karp residual reachability), `wang` (an edge-coloured tile family assembled into one atlas at the source extent), `histogramBlend` (per-lane rank-to-Gaussian transform, variance-preserving weighted blend, inverse transform)}.
- Law: the guide channel decides the geometry alone. A set whose guide channel is absent REFUSES rather than falling back to an arbitrary present channel, because a plan derived from a different field than the caller graded is a plan the receipt misdescribes; a LAYERED set refuses outright, since the sampler bridge carries one layer and a cube face tiled independently of its neighbours has no shared boundary at all.
- Entry: `public static Fin<(TextureSet Set, TileReceipt Receipt)> Tileify(TextureSet set, TilePolicy policy, Op key, TimeProvider? clock = null)` is the ONE entry — it solves the plan from the guide, folds `Apply` over every channel and pack plane in PAIRING ORDER, re-admits the result through `set#TEXTURE_SET` `TextureSet.Of` so the tiled set re-keys, and grades it through `[03]-[TILE_GATE]`; `public static Fin<TexturePyramid> Apply(TilePlan plan, TexturePyramid pyramid, Op key, Option<TexturePyramid> paired = default)` is the per-plane fold the set fold drives and the only surface a caller composing a single plane reaches, and it is TOTAL over a plan whose extent matches.
- Packages: QuikGraph (composed — `AdjacencyGraph<int, Edge<int>>` the overlap-band flow graph, `Edge<int>` the REFERENCE edge, `ReversedEdgeAugmentorAlgorithm<int, Edge<int>>.AddReversedEdges`/`RemoveReversedEdges` the residual-edge lifecycle, `EdmondsKarpMaximumFlowAlgorithm<int, Edge<int>>(graph, capacities, edgeFactory, augmentor)`/`Compute`/`ResidualCapacities` the augmenting solve and the residual capacities the cut walk reads), MathNet.Numerics (composed — `Distributions.Normal.InvCDF`/`Normal.CDF` the Gaussian round trip; `IntegralTransforms.Fourier.Forward2D` and `ComplexExtensions.MagnitudeSquared` at `[03]`), `Rasm` (project — `Deterministic.NextUnit` the one replayable jitter stream, `Dimension`, `Op`, `ValidityClaim`), `plane#TEXTURE_PLANE` (composed — `TexturePlane`/`TexturePyramid`/`PlaneFormat`/`MipPolicy` and the `AsImage` decoded-level bridge), `set#TEXTURE_SET` (composed — `TextureSet`/`TextureChannel`/`ChannelPackPlane`/`TextureSetDraft`), `codec#RASTER_FAULT` (composed — `RasterFault` band 2460), `Rasm.Materials.Appearance.Texture` (composed — `ShadeVec4`), CommunityToolkit.HighPerformance (`ReadOnlyMemory2D<T>`/`ReadOnlySpan2D<T>` the plan's blend field and every staging plane, `SpanOwner<T>.Allocate` the per-row scratch the plane write rail takes), BCL (`TimeProvider`, `ReferenceEqualityComparer`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new tiling method is one `TileStrategy` row carrying its `Solve` delegate — the plan shape already carries an offset, a per-texel blend field, and a quadrant table, so a method combining them differently needs no new carrier; a new policy knob is one `TilePolicy` column; a new plan geometry is one `TilePlan` column every strategy row leaves at its identity. There is NO per-strategy tiler type, NO `SeamlessTexture` surface, and NO per-channel tiling entry — the named defect is a second synthesizer, and the repair is a row.
- Boundary: `TilePlan` carries exactly what `Fold` CONSUMES — a wrap offset, a `[0,1]` blend field, and a Wang quadrant table — and never a colour, a histogram, a cut index, or a channel reference, which is what makes one plan legal over thirty-four planes of four different component counts and five different transfers. The seam CUT is not a plan column: a cut is the solver's decision about WHERE the blend field ramps, so it is folded INTO that field at solve time and recorded on the receipt as evidence; a plan carrying both a cut and a field lets an applicator consume one and ignore the other, which is precisely how a computed seam stops reaching the output. Each texel takes two candidates — the plan's offset tap and its DOUBLE-offset tap, which for the canonical half-extent offset are the wrapped and the unshifted source — and the field is the mix between them, identity-zero away from every seam so the interior is the wrapped image untouched. The `histogramBlend` row's rank transform is the ONE per-channel quantity and it is derived INSIDE `Fold` from that channel's own plane, PER LANE, against the SHARED blend field: the forward map is the empirical CDF composed with `Normal.InvCDF`, so the blend genuinely runs in a standard-Gaussian space and inverts back through `Normal.CDF` into the channel's own distribution — a bare CDF maps to a UNIFORM space, where a linear mix is not variance-preserving and the seam band regresses toward its own mean. The `wang` row's atlas is EXTENT-PRESERVING: the plan lays `colours²` tiles across a `colours × colours` grid of the source extent and `Fold` reads each target texel from its tile's own quadrant origin, so the set shape never changes and a consumer indexes tiles by the receipt's `WangEdge` rows. Every strategy runs in the plane's DECODED domain — the `AsImage` lift decodes an `srgb` plane to scene-linear before any difference metric, so a seam cost computed on encoded values (which weights dark texels wrongly by the transfer's own curve) is unrepresentable. Jitter is REPLAYABLE: the Wang quadrant draw consumes `Deterministic.NextUnit` over the plan's own `Seed`, so a plan re-solved at the same seed produces byte-identical geometry and a tiled set's content key is reproducible. The `graphCut` band is a GRID FLOW GRAPH whose vertices are the overlap texels with a source and a sink; both directions of every neighbour pair enter as SEPARATE `Edge<int>` instances at the same capacity because a minimum cut over an undirected grid needs both arcs saturable, the augmentor then mints one reverse per arc, and the capacity map is keyed by REFERENCE — a value-typed edge under a value-keyed map aliases each augmentor reverse onto the real arc it duplicates, handing the solver a residual graph with twice the capacity it should have and a cut through the wrong texels. The cut is read by a residual-capacity breadth-first walk from the source rather than from the algorithm's own vertex colouring, so the seam derives from published capacities and never from an internal traversal state. Planes fold in PAIRING ORDER — a channel whose `MipPolicy` is paired rebuilds its chain against its `TextureChannel.Pair` companion's already-tiled pyramid, so a roughness channel's variance coupling reads the tiled normal rather than refusing at an unpaired rebuild. Wall time rides the injected `TimeProvider`, so a receipt reports a measured elapsed or the caller's own clock, never a literal zero a benchmark reads as instantaneous. The `[EXPRESSION_SPINE]` kernel exemptions on this page are the four `Solve` kernels, the `Fold` applicator, the per-lane rank transform, and the `[03]` spectral staging fill — fixed-extent numeric kernels filling caller-owned buffers by index; every admission, dispatch, and egress surface on the page is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using CommunityToolkit.HighPerformance;           // Memory2D/Span2D/ReadOnlySpan2D — the blend field and every staging plane
using CommunityToolkit.HighPerformance.Buffers;   // SpanOwner — the stack-scoped per-row rental the write rail takes
using LanguageExt;                                // Seq, Option, Fin
using MathNet.Numerics.Distributions;             // Normal — InvCDF/CDF, the Gaussian round trip
using QuikGraph;                                  // AdjacencyGraph, Edge, EdgeFactory
using QuikGraph.Algorithms.MaximumFlow;           // EdmondsKarpMaximumFlowAlgorithm, ReversedEdgeAugmentorAlgorithm
using Rasm.Domain;                                // Op, Deterministic, ValidityClaim, IValidityEvidence
using Rasm.Materials.Appearance.Texture;          // ShadeVec4
using Rasm.Numerics;                              // Dimension
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// The solver table. Each row folds the guide plane, the policy, and the seed into ONE TilePlan; the plan is
// then channel-independent, which is the whole set-coherence guarantee. A row NEVER touches the set.
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

// --- [MODELS] ------------------------------------------------------------------------------
// One Wang tile: the four edge colours whose shared boundary strips make every adjacency legal, plus the
// source-patch origin each tile was drawn from. Assembly is deterministic in the plan's seed, and Tile is the
// atlas cell index a consumer addresses the tiled plane by.
public readonly record struct WangEdge(int Tile, int North, int East, int South, int West, int OriginX, int OriginY);

public sealed record TilePolicy(
    TileStrategy Strategy, TextureChannel Guide, int Overlap, double AcceptScore, ulong Seed, int WangColors) {
    public static TilePolicy Default(TextureChannel guide) =>
        new(TileStrategy.GraphCut, guide, Overlap: 32, AcceptScore: 0.85, Seed: 0x9E3779B97F4A7C15UL, WangColors: 2);
}

// GEOMETRY ONLY, and exactly what Fold reads: an offset naming the two candidate taps, a [0,1] blend field
// between them, and the Wang quadrant table. The solver's cut lives INSIDE the field it shaped — a plan
// carrying a cut beside a field lets an applicator honour one and drop the other, which is how a computed
// seam stops reaching the output.
public sealed record TilePlan(
    TileStrategy Strategy, Dimension Width, Dimension Height, int OffsetX, int OffsetY,
    ReadOnlyMemory2D<float> Blend, Seq<WangEdge> Wang, Seq<int> Cut, ulong Seed);

public sealed record TileReceipt(
    TileStrategy Strategy, TextureChannel Guide, TileScore Score, Seq<int> Cut, Seq<WangEdge> Wang,
    int Planes, ulong Seed, double ElapsedMs) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Planes, 1),
        ValidityClaim.Nonnegative(ElapsedMs),
        ValidityClaim.Evidence(Score));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TileSynth {
    // Solve ONCE from the guide, fold over EVERY plane in PAIRING ORDER, re-admit so the tiled set re-keys, then
    // grade. Tiled is the gate's own minted proof — a caller cannot assert it — and a failing grade returns the
    // tiled planes with no proof rather than discarding work the caller may still want at a lower bar.
    public static Fin<(TextureSet Set, TileReceipt Receipt)> Tileify(TextureSet set, TilePolicy policy, Op key, TimeProvider? clock = null) {
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return from _ in guard(set.Layers.Value is 1, RasterFault.Tile(key, $"<layered-set-has-no-shared-boundary:{set.Law.Key}:{set.Layers.Value}>"))
               from guide in set.Channels.Find(policy.Guide).ToFin(RasterFault.Tile(key, $"<tile-guide-absent:{policy.Guide.Key}>"))
               from image in guide.AsImage(key)
               let plan = policy.Strategy.Solve(image.Levels[0], policy, policy.Seed)
               // Pairing order: an unpaired channel first, so a paired rebuild reads its companion already tiled.
               from channels in toSeq(set.Channels).OrderBy(static pair => pair.Key.Pair.IsSome).ThenBy(static pair => pair.Key.Ordinal)
                   .Fold(Fin.Succ(HashMap<TextureChannel, TexturePyramid>.Empty), (acc, pair) =>
                       acc.Bind(map => Apply(plan, pair.Value, key, pair.Key.Pair.Bind(name => TextureChannel.TryGet(name, out TextureChannel? row) ? map.Find(row) : Option<TexturePyramid>.None))
                           .Map(tiled => map.Add(pair.Key, tiled))))
               from packs in set.Packs.Fold(Fin.Succ(Seq<ChannelPackPlane>()), (acc, pack) =>
                   acc.Bind(rows => Apply(plan, pack.Plane, key).Map(tiled => rows.Add(pack with { Plane = tiled }))))
               from graded in channels.Find(policy.Guide).ToFin(RasterFault.Tile(key, "<tile-guide-lost>"))
               let scored = TileGate.Grade(graded, policy)
               from tiled in TextureSet.Of(new TextureSetDraft(set.Width, set.Height, set.Layers, set.Law, set.Convention,
                   set.Alpha, set.HeightScaleMm, scored, set.Udim, channels, packs, set.Conductor, set.Material), key)
               select (tiled, new TileReceipt(policy.Strategy, policy.Guide, scored.Match(Some: static p => p.Score, None: static () => TileScore.Refused),
                   plan.Cut, plan.Wang, channels.Count + packs.Count, policy.Seed, ticks.GetElapsedTime(opened).TotalMilliseconds));
    }

    // The per-plane fold: tile the BASE level and rebuild the chain from it, so every mip level descends from
    // one tiled top rather than each level acquiring its own seam. A paired policy carries its companion into
    // the rebuild, so a roughness chain re-absorbs the tiled normal's variance instead of refusing unpaired.
    // The plane substrate owns allocation, transfer, and quantization — this fold hands it decoded texels.
    public static Fin<TexturePyramid> Apply(TilePlan plan, TexturePyramid pyramid, Op key, Option<TexturePyramid> paired = default) =>
        pyramid.Base.Width == plan.Width && pyramid.Base.Height == plan.Height
            ? from image in pyramid.AsImage(key)
              from top in TexturePlane.Of(pyramid.Base.Format, pyramid.Base.Width, pyramid.Base.Height, pyramid.Base.Transfer, pyramid.Base.Alpha, key, Some(pyramid.Base.Layers))
              let filled = Fill(top, TileKernel.Fold(plan, image.Levels[0]))
              from chain in TexturePyramid.Of(filled, pyramid.Policy, key, paired)
              select chain
            : Fin.Fail<TexturePyramid>(RasterFault.Tile(key, $"<tile-plan-extent-mismatch:{plan.Width.Value}x{plan.Height.Value}>"));

    // Row-wise writes through the plane's OWN WriteShade rail, so alpha association, transfer encode, and depth
    // narrowing all happen exactly once at their owner. The fold covers EVERY layer, because a plane whose tail
    // layers keep their untiled texels is half-tiled.
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

// The four solver kernels and the one applicator — the page's [EXPRESSION_SPINE] exemption, fixed-extent
// numeric folds filling caller-owned buffers by index.
internal static class TileKernel {
    // OFFSET-HEAL: wrap by half the extent so the former borders meet as a central cross, then cut each axis
    // along the minimum accumulated difference through the overlap band and RAMP the blend field across that
    // cut. The cut is a dynamic-programming walk, so it follows structure (a mortar joint, a grain boundary)
    // rather than a straight line the eye reads as a ruler.
    internal static TilePlan HealCut(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, band = Math.Min(policy.Overlap, Math.Min(w, h) / 4);
        ReadOnlySpan2D<ShadeVec4> src = guide.Span;
        int[] cut = new int[h];
        MinErrorPath(src, w / 2, band, h, w, cut);
        return new TilePlan(TileStrategy.OffsetHeal, Dimension.Create(w), Dimension.Create(h), w / 2, h / 2,
            Ramped(w, h, cut, band), Seq<WangEdge>(), toSeq(cut), seed);
    }

    // The DP: cost[c] accumulates the squared difference between the wrapped copy and the interior at each
    // candidate boundary index, and each scanline may move the boundary by at most one texel, so the recovered
    // path is 8-connected. Backtracking from the minimum terminal cost yields the cut.
    static void MinErrorPath(ReadOnlySpan2D<ShadeVec4> src, int centre, int band, int lines, int span, Span<int> cut) {
        int lo = Math.Max(1, centre - band), hi = Math.Min(span - 2, centre + band), width = hi - lo + 1;
        double[] prev = new double[width], next = new double[width];
        int[,] back = new int[lines, width];
        for (int line = 0; line < lines; line++) {
            for (int c = 0; c < width; c++) {
                int i = lo + c;
                double local = Cost(src[line, i], src[line, (i + (span / 2)) % span]);
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

    // GRAPH-CUT: the Kwatra formulation as a minimum s-t cut over the overlap band. Vertices are band texels
    // plus a source bound to the wrapped strip and a sink bound to the interior strip; an edge's capacity is
    // the matching cost at both endpoints. BOTH directions of every neighbour pair enter as SEPARATE Edge<int>
    // instances at one capacity, because an undirected min cut needs both arcs saturable; the augmentor then
    // mints one reverse per arc and the capacity map is keyed by REFERENCE — Edge<TVertex> overrides ToString
    // alone, so its identity IS its instance, where the value-typed SEdge<TVertex> would alias each augmentor
    // reverse onto the arc it duplicates and hand the solver twice the residual capacity it should have.
    internal static TilePlan CutBand(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, ox = w / 2, band = Math.Min(policy.Overlap, w / 4);
        int lo = ox - band, width = (2 * band) + 1, source = width * h, sink = source + 1;
        ReadOnlySpan2D<ShadeVec4> src = guide.Span;
        AdjacencyGraph<int, Edge<int>> flow = new(allowParallelEdges: true);
        Dictionary<Edge<int>, double> capacity = new(ReferenceEqualityComparer.Instance);
        void Link(int a, int b, double c) { Edge<int> e = new(a, b); flow.AddVerticesAndEdge(e); capacity[e] = c; }
        for (int y = 0; y < h; y++) {
            for (int c = 0; c < width; c++) {
                int v = (y * width) + c, i = lo + c;
                if (c is 0) { Link(source, v, double.MaxValue); }
                if (c == width - 1) { Link(v, sink, double.MaxValue); }
                if (c + 1 < width) { double m = Seam(src, y, i, w) + Seam(src, y, i + 1, w); Link(v, v + 1, m); Link(v + 1, v, m); }
                if (y + 1 < h) { double m = Seam(src, y, i, w) + Seam(src, y + 1, i, w); Link(v, v + width, m); Link(v + width, v, m); }
            }
        }
        using ReversedEdgeAugmentorAlgorithm<int, Edge<int>> augmentor = new(flow, static (a, b) => new Edge<int>(a, b));
        augmentor.AddReversedEdges();
        EdmondsKarpMaximumFlowAlgorithm<int, Edge<int>> solve = new(flow, e => capacity.TryGetValue(e, out double c) ? c : 0.0, static (a, b) => new Edge<int>(a, b), augmentor);
        solve.Compute(source, sink);
        int[] cut = Reachable(flow, solve.ResidualCapacities, source, width, h, lo);
        augmentor.RemoveReversedEdges();
        return new TilePlan(TileStrategy.GraphCut, Dimension.Create(w), Dimension.Create(h), ox, h / 2,
            Ramped(w, h, cut, band), Seq<WangEdge>(), toSeq(cut), seed);
    }

    // The cut is the source side of the residual graph, walked from published ResidualCapacities rather than
    // from the algorithm's own vertex colouring — one breadth-first pass over edges whose residual is positive.
    static int[] Reachable(AdjacencyGraph<int, Edge<int>> flow, IDictionary<Edge<int>, double> residual, int source, int width, int height, int lo) {
        HashSet<int> seen = [source];
        Queue<int> open = new([source]);
        while (open.Count > 0) {
            int v = open.Dequeue();
            foreach (Edge<int> e in flow.OutEdges(v)) { if (residual.TryGetValue(e, out double r) && r > 0.0 && seen.Add(e.Target)) { open.Enqueue(e.Target); } }
        }
        int[] cut = new int[height];
        for (int y = 0; y < height; y++) { int c = 0; while (c < width - 1 && seen.Contains((y * width) + c)) { c++; } cut[y] = lo + c; }
        return cut;
    }

    // WANG: assign each of colours^2 tiles its four edge colours and one source origin, then lay them out as a
    // colours x colours atlas at the SOURCE extent — the set shape never changes and a consumer indexes tiles
    // through the receipt's WangEdge rows. Every draw rides the plan's own splitmix64 stream, so the assignment
    // replays byte-identically at one seed. The blend field is identity zero: a Wang tile is a hard copy.
    internal static TilePlan WangAssign(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, colours = Math.Max(2, policy.WangColors), tiles = colours * colours;
        ulong state = seed;
        WangEdge[] rows = new WangEdge[tiles];
        for (int t = 0; t < tiles; t++) {
            rows[t] = new WangEdge(t, North: t / colours, East: t % colours, South: ((t / colours) + 1) % colours, West: ((t % colours) + 1) % colours,
                OriginX: (int)(Deterministic.NextUnit(ref state) * w), OriginY: (int)(Deterministic.NextUnit(ref state) * h));
        }
        return new TilePlan(TileStrategy.Wang, Dimension.Create(w), Dimension.Create(h), 0, 0,
            Zeroed(w, h), toSeq(rows), Seq<int>(), seed);
    }

    // HISTOGRAM-BLEND: the plan carries only the variance-preserving blend field — w / sqrt(w^2 + (1-w)^2)
    // rather than the plain partition of unity — so the blend keeps each channel's contrast instead of
    // regressing the seam band toward its mean. Away from every seam the ramp is zero, so the interior is the
    // offset tap untouched; the per-lane rank transform is per-channel and lives in Fold, never in the plan.
    internal static TilePlan BlendField(ReadOnlyMemory2D<ShadeVec4> guide, TilePolicy policy, ulong seed) {
        int w = guide.Width, h = guide.Height, band = Math.Min(policy.Overlap, Math.Min(w, h) / 4);
        float[] field = new float[w * h];
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                double u = Math.Max(Seam(x, w / 2, band), Seam(y, h / 2, band));
                field[(y * w) + x] = (float)(u > 0.0 ? u / Math.Sqrt((u * u) + ((1.0 - u) * (1.0 - u))) : 0.0);
            }
        }
        return new TilePlan(TileStrategy.HistogramBlend, Dimension.Create(w), Dimension.Create(h), w / 2, h / 2,
            new ReadOnlyMemory2D<float>(field, h, w), Seq<WangEdge>(), Seq<int>(), seed);
    }

    // The one applicator every strategy shares. Two candidates per texel: the plan's offset tap and its DOUBLE
    // offset (for the canonical half-extent offset, the wrapped and the unshifted source), mixed by the blend
    // field. The wang row overrides the source coordinate through its quadrant table. The histogram row alone
    // round-trips through this channel's OWN per-lane rank transform, so the mix runs in a standard-Gaussian
    // space and inverts back into the channel's own distribution.
    internal static ReadOnlyMemory2D<ShadeVec4> Fold(TilePlan plan, ReadOnlyMemory2D<ShadeVec4> plane) {
        int w = plane.Width, h = plane.Height;
        ShadeVec4[] target = new ShadeVec4[w * h];
        ReadOnlySpan2D<ShadeVec4> src = plane.Span;
        ReadOnlySpan2D<float> blend = plan.Blend.Span;
        double[][]? lut = plan.Strategy == TileStrategy.HistogramBlend ? RankLut(src) : null;
        int grid = plan.Wang.IsEmpty ? 0 : (int)Math.Round(Math.Sqrt(plan.Wang.Count));
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                (int ax, int ay) = grid > 0 ? Quadrant(plan, grid, w, h, x, y) : (((x + plan.OffsetX) % w), ((y + plan.OffsetY) % h));
                ShadeVec4 a = src[ay, ax];
                ShadeVec4 b = src[(y + (2 * plan.OffsetY)) % h, (x + (2 * plan.OffsetX)) % w];
                double t = blend[y, x];
                target[(y * w) + x] = lut is null ? ShadeVec4.Lerp(a, b, t) : Inverse(lut, ShadeVec4.Lerp(Gauss(lut, a), Gauss(lut, b), t));
            }
        }
        return new ReadOnlyMemory2D<ShadeVec4>(target, h, w);
    }

    // The atlas read: the target texel's cell selects its WangEdge row, and the offset inside the cell reads
    // from that row's own source origin, wrapped — so two tiles sharing an edge colour drew the same strip.
    static (int X, int Y) Quadrant(TilePlan plan, int grid, int w, int h, int x, int y) {
        int cw = w / grid, ch = h / grid;
        WangEdge tile = plan.Wang[(((y / ch) % grid) * grid) + ((x / cw) % grid)];
        return ((tile.OriginX + (x % cw)) % w, (tile.OriginY + (y % ch)) % h);
    }

    static double Cost(ShadeVec4 a, ShadeVec4 b) { ShadeVec4 d = a + (b * -1.0); return (d.X * d.X) + (d.Y * d.Y) + (d.Z * d.Z) + (d.W * d.W); }
    static double Seam(ReadOnlySpan2D<ShadeVec4> src, int y, int x, int w) => Cost(src[y, ((x % w) + w) % w], src[y, (((x + (w / 2)) % w) + w) % w]);
    // Zero away from the seam, one at it: the interior is the offset tap untouched and only the band mixes.
    static double Seam(int i, int centre, int band) => band <= 0 ? 0.0 : Math.Clamp(1.0 - (Math.Abs(i - centre) / (double)band), 0.0, 1.0);
    static ReadOnlyMemory2D<float> Zeroed(int w, int h) => new(new float[w * h], h, w);
    static ReadOnlyMemory2D<float> Ramped(int w, int h, ReadOnlySpan<int> cut, int band) {
        float[] f = new float[w * h];
        for (int y = 0; y < h; y++) { for (int x = 0; x < w; x++) { f[(y * w) + x] = (float)Seam(x, cut[y], band); } }
        return new ReadOnlyMemory2D<float>(f, h, w);
    }

    // The per-lane rank transform: a 256-bin empirical CDF per colour lane composed with the standard-normal
    // quantile, so the forward map genuinely reaches a Gaussian space. A bare CDF reaches a UNIFORM space,
    // where a linear mix is not variance-preserving and the seam band regresses toward its own mean — and a
    // luminance-built table applied to three lanes shifts hue wherever the lanes differ.
    static double[][] RankLut(ReadOnlySpan2D<ShadeVec4> plane) {
        double[][] lanes = [new double[256], new double[256], new double[256]];
        for (int y = 0; y < plane.Height; y++) {
            for (int x = 0; x < plane.Width; x++) {
                ShadeVec4 texel = plane[y, x];
                lanes[0][Bin(texel.X)] += 1.0; lanes[1][Bin(texel.Y)] += 1.0; lanes[2][Bin(texel.Z)] += 1.0;
            }
        }
        foreach (double[] lane in lanes) {
            double total = 0.0;
            for (int i = 0; i < lane.Length; i++) { total += lane[i]; lane[i] = total; }
            for (int i = 0; i < lane.Length; i++) { lane[i] = total > 0.0 ? Math.Clamp(lane[i] / total, 1e-6, 1.0 - 1e-6) : 0.5; }
        }
        return lanes;
    }

    static int Bin(double v) => Math.Clamp((int)(v * 255.0), 0, 255);
    static ShadeVec4 Gauss(double[][] lut, ShadeVec4 v) => new(Normal.InvCDF(0.0, 1.0, lut[0][Bin(v.X)]), Normal.InvCDF(0.0, 1.0, lut[1][Bin(v.Y)]), Normal.InvCDF(0.0, 1.0, lut[2][Bin(v.Z)]), v.W);
    static ShadeVec4 Inverse(double[][] lut, ShadeVec4 v) => new(Unmap(lut[0], v.X), Unmap(lut[1], v.Y), Unmap(lut[2], v.Z), v.W);
    static double Unmap(double[] lane, double gaussian) {
        int i = Array.BinarySearch(lane, Normal.CDF(0.0, 1.0, gaussian));
        return (i < 0 ? ~i : i) / 255.0;
    }
}
```

## [03]-[TILE_GATE]

- Owner: `TileGate` the deterministic tileability grade; `TileScore` the measurement; `TileProof` the minted evidence a `set#TEXTURE_SET` `TextureSet` carries.
- Law: `TileProof` has no public construction — `Grade` is its only mint — so `TextureSet.Tiled` cannot be asserted, only earned; an ingested third-party set claiming tileability in its own manifest carries `None` until it is graded here.
- Entry: `public static Option<TileProof> Grade(TexturePyramid pyramid, TilePolicy policy)` is TOTAL — a grade is EVIDENCE, never a rail, so a plane that tiles badly returns absence and the caller decides; both signals read the plane's own decoded row rail, so no sampler lift and therefore no fault exists on this path.
- Packages: MathNet.Numerics (composed — `IntegralTransforms.Fourier.Forward2D(Complex[], int, int, FourierOptions)` over the bounded row-major luminance staging, `FourierOptions.Default` the symmetric scaling that makes the forward-inverse round trip an identity, `ComplexExtensions.MagnitudeSquared` the per-bin power), `plane#TEXTURE_PLANE` (composed — `TexturePyramid.Levels` the bounded grading level, `TexturePlane.Read` the streaming decoded rail both signals fold), CommunityToolkit.HighPerformance (`SpanOwner<T>` the row rentals), BCL inbox (`System.Numerics.Complex`), `Rasm` (project — `ValidityClaim`).
- Growth: a new tileability signal is one `TileScore` column and one term in the combined verdict; the acceptance threshold is the caller's `TilePolicy.AcceptScore`, never a constant here.
- Boundary: the grade is TWO independent measurements against ONE verdict, because either alone is defeatable, and each reads the level its own signal lives at. `SeamRatio` compares the mean squared difference across the WRAP boundary against the mean squared difference between interior neighbours, measured on the BASE level — a seam is a high-frequency artefact a mip erases, so grading it on a reduced level grades away what is being tested — and the fold streams two rows at a time through the plane's own decoded rail, so a 16k plane costs one row pair of memory rather than a full materialization. A plane whose wrap is exact has a boundary statistically indistinguishable from its interior, so the ratio approaches one and a visible seam drives it above one; a plane that is merely BLURRED at its border also scores well here, which is why it cannot be the whole verdict. `LatticeLeak` reads the `Fourier.Forward2D` spectrum on the pyramid level nearest a bounded grading extent — the spectral signature of a discontinuity is scale-free, and a full-resolution transform over a 4k plane would stage sixteen million complex samples for a scalar answer — and measures the MEDIAN axis-bin power rather than the total axis power, because the `kx = 0`/`ky = 0` cross carries two different things: a seam raises the whole axis as a broadband `1/k` floor, while a genuinely periodic pattern raises isolated axis HARMONICS. Reading total axis power fails every correctly-tiling brick, weave, and lattice; reading the median reads the floor those harmonics sit on. The combined `Value` is the product of the two normalized terms and the caller's own `AcceptScore` decides; the learned tileability scorer is a SPIKE — an optional ONNX stage the `neural#PBR_STAGE` registry admits as a quality gate BESIDE this floor, never in place of it, because a scorer whose weights retire takes the deterministic verdict with it. Every measurement runs on the plane's LUMINANCE — the AP1 scene-linear `ShadeVec4.Luminance` weights, so a green-heavy pattern is not read as a red one — and the staging buffer is caller-owned and filled by index, the page's declared kernel exemption.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — the [02] prelude is in scope, plus:)
using System.Numerics;                            // Complex — the Fourier staging element
using MathNet.Numerics;                           // ComplexExtensions.MagnitudeSquared
using MathNet.Numerics.IntegralTransforms;        // Fourier.Forward2D, FourierOptions

// --- [MODELS] ------------------------------------------------------------------------------
// Two independent measurements, one verdict, each carrying the extent it was measured at so a reader can
// re-derive the number. SeamRatio alone passes a blurred border; LatticeLeak alone passes a plane whose seam
// is sharp but spectrally quiet. The product is what neither defeats.
public readonly record struct TileScore(double SeamRatio, double LatticeLeak, double Value, Dimension GradedAt)
    : IValidityEvidence {
    public static readonly TileScore Refused = new(double.PositiveInfinity, 1.0, 0.0, Dimension.Create(1));
    public bool IsValid => ValidityClaim.All(ValidityClaim.Nonnegative(SeamRatio), ValidityClaim.UnitInterval(LatticeLeak), ValidityClaim.UnitInterval(Value));
}

// The evidence a set carries in place of a boolean. Construction is private and Grade is the only mint, so a
// TextureSet's Tiled column records a measurement that happened rather than a claim someone made.
public sealed record TileProof {
    private TileProof(TileStrategy strategy, TileScore score, ulong seed) => (Strategy, Score, Seed) = (strategy, score, seed);
    public TileStrategy Strategy { get; }
    public TileScore Score { get; }
    public ulong Seed { get; }
    internal static TileProof Mint(TileStrategy strategy, TileScore score, ulong seed) => new(strategy, score, seed);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TileGate {
    // The bounded grading extent: the spectral signature of a discontinuity is scale-free, so the lattice read
    // takes the pyramid level nearest this edge and a 4k plane never stages sixteen million complex samples.
    const int GradeEdge = 256;

    // TOTAL: a grade is evidence, so a badly-tiling plane returns absence and no call here can fault. Both
    // signals read through the plane's own decoded row rail, so the gate needs no sampler lift and a layered
    // plane grades its first layer rather than refusing a bridge it never crosses.
    public static Option<TileProof> Grade(TexturePyramid pyramid, TilePolicy policy) {
        TexturePlane spectral = Level(pyramid);
        double ratio = SeamRatio(pyramid.Base);
        double seam = 1.0 / (1.0 + Math.Max(0.0, ratio - 1.0));
        double leak = LatticeLeak(spectral);
        double value = Math.Clamp(seam * (1.0 - leak), 0.0, 1.0);
        TileScore score = new(ratio, leak, value, spectral.Width);
        return value >= policy.AcceptScore ? Some(TileProof.Mint(policy.Strategy, score, policy.Seed)) : Option<TileProof>.None;
    }

    // The coarsest level whose longer side still covers the grading edge; a single-level pyramid grades at its
    // own extent, which is the honest answer rather than a refusal.
    static TexturePlane Level(TexturePyramid pyramid) =>
        pyramid.Levels.Filter(static level => Math.Max(level.Width.Value, level.Height.Value) >= GradeEdge).LastOrNone().IfNone(pyramid.Base);

    // Wrap energy against interior energy, STREAMED over the base plane's own decoded rail: the last row
    // against the first and the last column against the first, versus every four-neighbour interior pair. A
    // seamless plane's two numbers agree, and the fold holds two rows at a time whatever the extent.
    static double SeamRatio(TexturePlane plane) {
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

    // The spectral floor under the axis cross: a plane whose implicit periodic extension is discontinuous
    // raises the whole kx = 0 / ky = 0 axis as a broadband 1/k floor, while a genuinely periodic pattern raises
    // isolated axis harmonics. Reading TOTAL axis power fails every correctly-tiling brick; reading the MEDIAN
    // axis bin reads the floor those harmonics sit on. Forward2D mutates the caller-owned row-major buffer in
    // place under the symmetric FourierOptions.Default scaling, so the measured fraction is scale-invariant.
    static double LatticeLeak(TexturePlane plane) {
        int w = plane.Width.Value, h = plane.Height.Value;
        using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(w);
        Complex[] spectrum = new Complex[w * h];
        for (int y = 0; y < h; y++) {
            plane.ReadShade(row: y, layer: 0, field.Span);
            for (int x = 0; x < w; x++) { spectrum[(y * w) + x] = new Complex(field.Span[x].Luminance, 0.0); }
        }
        Fourier.Forward2D(spectrum, h, w, FourierOptions.Default);
        double[] axis = new double[w + h - 2];
        double total = 0.0;
        int taken = 0;
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                if (y is 0 && x is 0) { continue; }
                double power = spectrum[(y * w) + x].MagnitudeSquared();
                total += power;
                if ((y is 0 || x is 0) && taken < axis.Length) { axis[taken++] = power; }
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

- [TEXTILE_SCORER]-[BLOCKED]: a learned tileability scorer admits as a `neural#PBR_STAGE` registry row grading a candidate plane beside `TileGate.Grade`, with the deterministic floor shipping unchanged; convergence needs a licence-clean ONNX export and a fixed-shape tile contract, and the scorer never becomes the acceptance authority because a retired weight file takes the verdict with it.
