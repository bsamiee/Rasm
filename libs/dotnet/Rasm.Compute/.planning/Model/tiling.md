# [COMPUTE_TILING]

`TilePlan` owns the fixed-bucket grid a plane larger than every admitted session shape runs under: source extent, the ordered `TileProduct` roster binding each product to its graph tensor and component lane, bucket edge, seam overlap, scale, and the three row families that generate the space — `PadMode` folding an out-of-range index back onto the plane, `TileBlend` shaping the overlap taper, and `TileLayout` carrying the gather, scatter, normalize, and filler-consuming stack kernels for one dimension order. The plan itself owns the layout-free weight accumulation every field shares and the `TileTensor` derivation folding the roster into the session's own output cardinality.

`RunOps.InferTiled` is the fold: one gather-run-scatter pass per window over the `Model/run#RUN_MODES` bound pulse, overlap-adding each field of one forward pass into its own pooled plane, reading each grade off its lane, proving its own coverage against the measured weight floor, and closing every product through one item-partitioned normalize. `TileMosaic` is the assembled field set beside the grade set and the one release point for every arena. `Model/stage#STAGE_FOLD` constructs the plan and consumes the mosaic; nothing else does.

## [01]-[INDEX]

- [02]-[TILE_PLAN]: fixed-bucket grid derivation with row-owned pad, blend, and layout kernels; the roster→tensor→lane fold binding every product of one forward pass to its own bytes; the edge-row taper table; the shared weight plane; and one accumulating admission naming every refusal a grid can carry.
- [03]-[TILE_FOLD]: the gather-run-scatter mosaic over one bound flow, its measured coverage floor, its item-partitioned close, and the origin-window canary two providers compare on.
- [04]-[RESEARCH]: open questions.

## [02]-[TILE_PLAN]

- Owner: `TilePlan` `[ComplexValueObject]` owns the whole tiling — source extent, source channels, the ordered `TileProduct` roster, bucket edge, overlap, scale, and the `PadMode`/`TileBlend`/`TileLayout` rows that generate the space; `TileEdge` owns the four per-axis taper variants and each row's own ramp fill; `TileAdmission` carries the leased card's bucket and seam authority; `TileGate` closes the plan's refusal roster so a malformed grid names EVERY invariant it broke; `TileRefusal` names this owner's shared contract refusals without a string-key roster.
- Cases: `TileProduct.Plane` a field across the tile and `TileProduct.Measure` a grade over it; `TileEdge` rows `Interior`, `Leading`, `Trailing`, `Both`; `PadMode.Reflect`; `TileBlend` rows `Hann`, `Linear`, `Smoothstep`; `TileLayout` rows `Planar` (`NCHW`) and `Interleaved` (`NHWC`).
- Entry: `public Fin<TilePlan> Validate(...)` through the generated `[ComplexValueObject]` factory seam — the one place the fixed-bucket law is spelled, composed at `Model/stage#STAGE_WIRE` `StageRequest.Plan` and nowhere else.
- Law: tiles are FIXED-SHAPE. Dynamic input extents re-partition the graph and defeat memory-pattern reuse on every call, so the bound input holds one bucket shape for the whole mosaic and the plane adapts to the bucket rather than the session adapting to the plane. Grids count the first tile whole and step the remainder by the stride, so an extent equal to its bucket is exactly one tile; stepping the whole extent against the stride emits a trailing tile carrying no new texels.
- Law: the step derivation is a STATED CROSS-END INVARIANT, not a seat. The specifying end derives the identical column count from the identical extent, bucket, and overlap columns the wire already threads, and the strata forbid either end naming the other's type — so both ends carry the same arithmetic by construction and a change to it is a change at both cards in one edit. A shared helper here would be a strata reference, and a wire column carrying the count would let a producer's grid and this executor's grid disagree without either noticing.
- Law: ONE grid carries EVERY product. Appearance estimators emit base colour, normal, and roughness from a single forward pass, so `TilePlan.Products` scatters all three out of the same tile run and a mosaic costs the grid rather than the grid times the plane count.
- Law: a product binds to a TENSOR and a LANE, never to a position. A PACKED export names one tensor for several products, so `TilePlan.Tensors` folds the roster into the distinct tensors in first-appearance order — the order `InferenceSession.OutputNames` carries — and each row takes the channel offset its earlier lanes on that tensor leave. The run then resolves a TENSOR by position and slices each lane out of it, so a graph emitting roughness beside metalness in one `material` tensor lands two planes where a one-tensor-one-product assumption lands the first tensor's bytes twice. Result cardinality proves against the TENSOR count rather than the roster width, and a lane whose element count disagrees with its tensor's declared channel sum refuses.
- Law: a MEASURE is rank-0 and grades ONE tile. A grade owns no arena, no taper, and no accumulation — the lane read IS the value — so a graded tensor's element count is its lane count rather than the tile area, and every lane on one tensor shares one modality. Aggregating N tile grades into one number mints a statistic no model measured and no score row declares a direction for, so a roster carrying a measure admits exactly one window and a source extent past its bucket refuses at the plan, where the specifying end's own bucket roster is what declares a scorer's admissible extent.
- Law: reassembly is OVERLAP-ADD, never last-writer-wins. Each produced tile scatters through its taper weights into its product's accumulation plane, one shared weight plane accumulates the taper mass, and one divide per product closes the mosaic — so an overlap band carries the weighted mean of both estimates rather than a hard seam, and a blend row whose profile does not sum to unity still reconstructs exactly because the divide normalizes what accumulated instead of trusting the profile. `TilePlan.Accumulate` owns that weight plane as pure geometry — one taper mass per texel, free of layout and product.
- Law: taper applies only where a tile MEETS a neighbour, and WHICH edges meet is a ROW rather than a bit pair. Each axis carries a `TileEdge` row naming whether the leading edge, the trailing edge, both, or neither abuts a neighbour, and the row itself fills its own ramp — so a tile touching the plane border keeps unit weight there, `Interior` names the all-unit ramp a single-tile axis reads, and no consumer re-derives a taper from `mask & 1`. Tapering against the plane border divides the outermost texels by a weight no neighbour ever completes and fades the plane's own edge.
- Law: absence is an OPTION, never a negative index. A pad row answers the source index it folded to, or NOTHING for a texel no source covers; the gather rows clear on absence. A `-1` sentinel made four gather arms test a value `Reflect` never produces, so the arms read as dead where the carrier states the fact.
- Law: admission ACCUMULATES. Twelve independent invariants folded into one `&&` chain produced one message for a dozen distinct refusals, so a plan breaking four of them named none of the four; the roster of `(row, predicate)` gates folds instead and the refusal names every row that failed.
- Auto: `TilePlan` derives grid, stride, output extent, bucket key, and the bound input and per-tensor output shapes from its own columns, so a caller states extents and a bucket and never a coordinate — and the binder that seats the flow reads the same shapes the fold runs. Admission rejects a nonpositive extent or channel count, an empty roster, a duplicated role key or duplicated `(Tensor, Lane)` pair, a tensor mixing field lanes with grade lanes, a multi-window grid under a graded roster, an overlap at or past half the shorter bucket edge, a blend profile that is not complementary over its own sampled band, and any field whose output element count passes `Array.MaxLength`. It does NOT restate which pad row is legal: the row family is the general tiling vocabulary and the frozen wire pins `reflect` at `StageRequest.Admit`, the boundary that carries it. Gathering stages each tile through the pad row over a `Span2D` plane view, taking the contiguous row copy whenever the row lies wholly inside the plane and folding per texel only at an edge. Scattering accumulates through `TensorPrimitives.Multiply`/`Add`/`MultiplyAdd` over the per-row weight vector, so reassembly vectorizes rather than walking texels.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new seam profile is one `TileBlend` row with its ramp; a new tensor layout is one `TileLayout` row carrying its own gather, scatter, normalize, and stack kernels — never a layout flag branching inside the fold; a new edge posture is one `TileEdge` row with its own fill; a model emitting another plane is one more `TileProduct.Plane` row naming its own tensor at lane zero, a model PACKING another plane into a tensor it already names is one more row at that tensor's next lane, and a model grading its input is one `TileProduct.Measure` row — no surface moves for any of the three; a stage that up-samples is the `Scale` column, which threads every field grid without a caller recomputing anything; a pad posture beyond reflection is one `PadMode` row whose `Fold` may answer absence for a texel no source covers, which the gather rows already clear on; a new plan invariant is one `TileGate` row.
- Boundary: this owner names no session, no port, and no archive member. Every carrier crossing a kernel row is a span view, so the four layout delegates are the only shape that holds them and `Span2D`/`ReadOnlySpan2D` projections carry the row addressing the flat index arithmetic used to spell at eleven sites. `TileLayout.Stack` consumes a `PlaneFill` filler rather than a materialized plane, so each row owns its own landing discipline and the double copy dies where placement is contiguous; the filler is a delegate the composing root binds — a blob copy, or a `Runtime/archive#HDF_ARCHIVE` hyperslab fill for an archive-resident plane — so no PureHDF member reaches a Compute signature.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileEdge {
    public static readonly TileEdge Interior = new("interior", index: 0, leads: false, trails: false);
    public static readonly TileEdge Leading = new("leading", index: 1, leads: true, trails: false);
    public static readonly TileEdge Trailing = new("trailing", index: 2, leads: false, trails: true);
    public static readonly TileEdge Both = new("both", index: 3, leads: true, trails: true);

    private TileEdge(string key, int index, bool leads, bool trails) : this(key) =>
        (Index, Leads, Trails) = (index, leads, trails);

    public int Index { get; }

    public bool Leads { get; }

    public bool Trails { get; }

    public static TileEdge Of(bool leads, bool trails) =>
        (leads, trails) switch {
            (true, true) => Both,
            (true, false) => Leading,
            (false, true) => Trailing,
            _ => Interior,
        };

    public void Taper(Span<float> ramp, TileBlend blend, int taper) {
        ramp.Fill(1f);
        for (int index = 0; taper > 0 && index < taper && index < ramp.Length; index++) {
            float weight = blend.Weight((index + 0.5f) / taper);
            if (Leads) { ramp[index] = weight; }
            if (Trails) { ramp[ramp.Length - 1 - index] = weight; }
        }
    }
}

public readonly record struct TileWindow(int Column, int Row, int SourceX, int SourceY, TileEdge EdgeX, TileEdge EdgeY);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileProduct {
    private TileProduct(string tensor, int lane, string role, int channels) =>
        (Tensor, Lane, Role, Channels) = (tensor, lane, role, channels);

    public sealed record Plane(string Tensor, int Lane, string Role, int Channels)
        : TileProduct(Tensor, Lane, Role, Channels);

    public sealed record Measure(string Tensor, int Lane, string Role)
        : TileProduct(Tensor, Lane, Role, channels: 1);

    public string Tensor { get; }

    public int Lane { get; }

    public string Role { get; }

    public int Channels { get; }

    public bool Graded => Switch(plane: static _ => false, measure: static _ => true);

    public Option<Plane> Field => Switch(plane: Some, measure: static _ => Option<Plane>.None);

    public Option<Measure> Grade => Switch(plane: static _ => Option<Measure>.None, measure: Some);
}

public readonly record struct TileSlice(TileProduct Product, int Offset, int Slot);

public readonly record struct TileTensor(string Name, int Channels, Seq<TileSlice> Slices) {
    public bool Graded => Slices.Exists(static slice => slice.Product.Graded);

    public long Expected(long area) => Graded ? Channels : area * Channels;
}

public delegate void TileGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window);

public delegate void TileScatter(
    ReadOnlySpan<float> tile, Span<float> plane,
    ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
    TilePlan plan, TileWindow window, int channels, int offset, int total);

public delegate void TileNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels);

public delegate Fin<Unit> PlaneFill(Span<float> destination);

public delegate Fin<Unit> TileStack(PlaneFill fill, Span<float> stacked, int channels, int offset, int total, int texels);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PadMode {
    public static readonly PadMode Reflect = new("reflect", static (index, extent) => {
        if (extent is 1) { return Some(0); }
        int period = 2 * (extent - 1);
        int folded = Math.Abs(index) % period;
        return Some(folded < extent ? folded : period - folded);
    });
    [UseDelegateFromConstructor]
    public partial Option<int> Fold(int index, int extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileBlend {
    public static readonly TileBlend Hann = new("hann", static t => 0.5f * (1f - MathF.Cos(MathF.PI * t)));
    public static readonly TileBlend Linear = new("linear", static t => t);
    public static readonly TileBlend Smoothstep = new("smoothstep", static t => t * t * (3f - 2f * t));

    [UseDelegateFromConstructor]
    public partial float Weight(float t);

    public bool Complementary(int samples = 17, float tolerance = 1e-4f) =>
        Enumerable.Range(0, samples).All(step =>
            (step / (float)(samples - 1)) is var t
            && MathF.Abs(Weight(t) + Weight(1f - t) - 1f) <= tolerance);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileLayout {
    public static readonly TileLayout Planar = new(
        "nchw", static (channels, height, width) => [1L, channels, height, width],
        PlanarGather, PlanarScatter, PlanarNormalize, PlanarStack);
    public static readonly TileLayout Interleaved = new(
        "nhwc", static (channels, height, width) => [1L, height, width, channels],
        InterleavedGather, InterleavedScatter, InterleavedNormalize, InterleavedStack);

    private TileLayout(
        string key, Func<int, int, int, long[]> shape,
        TileGather gather, TileScatter scatter, TileNormalize normalize, TileStack stack) : this(key) =>
        (Shape, Gather, Scatter, Normalize, Stack) = (shape, gather, scatter, normalize, stack);

    public Func<int, int, int, long[]> Shape { get; }
    public TileGather Gather { get; }
    public TileScatter Scatter { get; }
    public TileNormalize Normalize { get; }
    public TileStack Stack { get; }

    static void PlanarGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window) {
        bool interior = window.SourceX >= 0 && window.SourceX + plan.TileWidth <= plan.SourceWidth;
        Span2D<float> tilePlane = tile.AsSpan2D(plan.Channels * plan.TileHeight, plan.TileWidth);
        ReadOnlySpan2D<float> sourcePlane = source.AsSpan2D(plan.Channels * plan.SourceHeight, plan.SourceWidth);
        for (int y = 0; y < plan.TileHeight; y++) {
            Option<int> folded = plan.Pad.Fold(window.SourceY + y, plan.SourceHeight);
            for (int channel = 0; channel < plan.Channels; channel++) {
                Span<float> row = tilePlane.GetRowSpan(channel * plan.TileHeight + y);
                if (folded.Case is not int sourceY) { row.Clear(); continue; }
                ReadOnlySpan<float> plane = sourcePlane.GetRowSpan(channel * plan.SourceHeight + sourceY);
                if (interior) { plane.Slice(window.SourceX, plan.TileWidth).CopyTo(row); continue; }
                for (int x = 0; x < plan.TileWidth; x++) {
                    row[x] = plan.Pad.Fold(window.SourceX + x, plan.SourceWidth).Case is int sourceX ? plane[sourceX] : 0f;
                }
            }
        }
    }

    static void PlanarScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels, int offset, int total) {
        int tileWidth = plan.TileWidth * plan.Scale;
        int tileHeight = plan.TileHeight * plan.Scale;
        int originX = window.SourceX * plan.Scale;
        int originY = window.SourceY * plan.Scale;
        int span = Math.Min(tileWidth, plan.OutputWidth - originX);
        if (span <= 0) { return; }
        ReadOnlySpan2D<float> produced = tile.AsSpan2D(total * tileHeight, tileWidth);
        Span2D<float> target = plane.AsSpan2D(channels * plan.OutputHeight, plan.OutputWidth);
        for (int y = 0; y < tileHeight; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)plan.OutputHeight) { continue; }
            Span<float> weights = row[..span];
            TensorPrimitives.Multiply(rampX[..span], rampY[y], weights);
            for (int channel = 0; channel < channels; channel++) {
                ReadOnlySpan<float> lane = produced.GetRowSpan((offset + channel) * tileHeight + y)[..span];
                Span<float> band = target.GetRowSpan(channel * plan.OutputHeight + planeY).Slice(originX, span);
                TensorPrimitives.MultiplyAdd(lane, weights, band, band);
            }
        }
    }

    static void PlanarNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels) {
        Span2D<float> bands = plane.AsSpan2D(channels, weight.Length);
        for (int channel = 0; channel < channels; channel++) {
            Span<float> band = bands.GetRowSpan(channel);
            TensorPrimitives.Divide(band, weight, band);
        }
    }

    static void InterleavedGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window) {
        Span2D<float> tilePlane = tile.AsSpan2D(plan.TileHeight, plan.TileWidth * plan.Channels);
        ReadOnlySpan2D<float> sourcePlane = source.AsSpan2D(plan.SourceHeight, plan.SourceWidth * plan.Channels);
        for (int y = 0; y < plan.TileHeight; y++) {
            Option<int> folded = plan.Pad.Fold(window.SourceY + y, plan.SourceHeight);
            Span<float> tileRow = tilePlane.GetRowSpan(y);
            if (folded.Case is not int sourceY) { tileRow.Clear(); continue; }
            ReadOnlySpan<float> sourceRow = sourcePlane.GetRowSpan(sourceY);
            for (int x = 0; x < plan.TileWidth; x++) {
                Span<float> texel = tileRow.Slice(x * plan.Channels, plan.Channels);
                if (plan.Pad.Fold(window.SourceX + x, plan.SourceWidth).Case is int sourceX) {
                    sourceRow.Slice(sourceX * plan.Channels, plan.Channels).CopyTo(texel);
                }
                else { texel.Clear(); }
            }
        }
    }

    static void InterleavedScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels, int offset, int total) {
        int tileWidth = plan.TileWidth * plan.Scale;
        int tileHeight = plan.TileHeight * plan.Scale;
        int originX = window.SourceX * plan.Scale;
        int originY = window.SourceY * plan.Scale;
        ReadOnlySpan2D<float> produced = tile.AsSpan2D(tileHeight, tileWidth * total);
        Span2D<float> target = plane.AsSpan2D(plan.OutputHeight, plan.OutputWidth * channels);
        for (int y = 0; y < tileHeight; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)plan.OutputHeight) { continue; }
            ReadOnlySpan<float> producedRow = produced.GetRowSpan(y);
            Span<float> targetRow = target.GetRowSpan(planeY);
            for (int x = 0; x < tileWidth; x++) {
                int planeX = originX + x;
                if ((uint)planeX >= (uint)plan.OutputWidth) { continue; }
                ReadOnlySpan<float> lane = producedRow.Slice((x * total) + offset, channels);
                Span<float> band = targetRow.Slice(planeX * channels, channels);
                TensorPrimitives.MultiplyAdd(lane, rampX[x] * rampY[y], band, band);
            }
        }
    }

    static void InterleavedNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels) {
        Span2D<float> texels = plane.AsSpan2D(weight.Length, channels);
        for (int texel = 0; texel < weight.Length; texel++) {
            Span<float> band = texels.GetRowSpan(texel);
            TensorPrimitives.Divide(band, weight[texel], band);
        }
    }

    static Fin<Unit> PlanarStack(PlaneFill fill, Span<float> stacked, int channels, int offset, int total, int texels) =>
        fill(stacked.Slice(offset * texels, channels * texels));

    static Fin<Unit> InterleavedStack(PlaneFill fill, Span<float> stacked, int channels, int offset, int total, int texels) {
        using SpanOwner<float> scratch = SpanOwner<float>.Allocate(channels * texels);
        if (fill(scratch.Span).Case is Error missed) { return Fin.Fail<Unit>(missed); }
        ReadOnlySpan2D<float> plane = scratch.Span.AsSpan2D(texels, channels);
        Span2D<float> seated = stacked.AsSpan2D(texels, total);
        for (int texel = 0; texel < texels; texel++) {
            plane.GetRowSpan(texel).CopyTo(seated.GetRowSpan(texel).Slice(offset, channels));
        }
        return Fin.Succ(Unit.Default);
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TileAdmission(Seq<int> Edges, int MinOverlap, int MaxOverlap) {
    public static readonly TileAdmission Unbounded = new(Seq<int>(), 1, int.MaxValue);

    public bool Admits(int tileWidth, int tileHeight, int overlap) =>
        tileWidth > 0 && tileHeight > 0
        && (Edges.IsEmpty || (Edges.Contains(tileWidth) && Edges.Contains(tileHeight)))
        && overlap >= MinOverlap && overlap <= MaxOverlap;
}

public readonly record struct TileCandidate(
    int SourceWidth, int SourceHeight, int Channels, Seq<TileProduct> Products,
    int TileWidth, int TileHeight, int Overlap, int Scale,
    TileAdmission Admission, TileBlend Blend);

// --- [ERRORS] --------------------------------------------------------------------------
public static class TileRefusal {
    public static readonly ContractRefusal SourceLength = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal TensorCardinality = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal OutputLength = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Coverage = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal CanaryOutput = new(ComputeArea.Model, ComputeContract.Valid);

}

public readonly record struct TileGate(string Row, Func<TileCandidate, bool> Holds);

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class TilePlan {
    public int SourceWidth { get; }

    public int SourceHeight { get; }

    public int Channels { get; }

    public Seq<TileProduct> Products { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public int Overlap { get; }

    public int Scale { get; }

    public TileAdmission Admission { get; }

    public PadMode Pad { get; }

    public TileBlend Blend { get; }

    public TileLayout Layout { get; }

    public string Bucket => $"{TileWidth}x{TileHeight}";

    public int StrideX => TileWidth - Overlap;

    public int StrideY => TileHeight - Overlap;

    public int Columns => Steps(SourceWidth, TileWidth, Overlap);

    public int Rows => Steps(SourceHeight, TileHeight, Overlap);

    public int OutputWidth => SourceWidth * Scale;

    public int OutputHeight => SourceHeight * Scale;

    public long[] InputShape => Layout.Shape(Channels, TileHeight, TileWidth);

    public Seq<TileTensor> Tensors =>
        Products.Fold(
            (Roster: Seq<TileTensor>(), Fields: 0, Grades: 0),
            static (state, product) => (
                Roster: Seated(state.Roster, product, product.Graded ? state.Grades : state.Fields),
                Fields: state.Fields + (product.Graded ? 0 : 1),
                Grades: state.Grades + (product.Graded ? 1 : 0)))
            .Roster;

    public Seq<TileProduct.Plane> Fields => Products.Choose(static product => product.Field);

    public Seq<TileProduct.Measure> Scorers => Products.Choose(static product => product.Grade);

    public Option<long[]> OutputShape(TileTensor tensor) =>
        tensor.Graded ? None : Some(Layout.Shape(tensor.Channels, TileHeight * Scale, TileWidth * Scale));

    public Seq<TileWindow> Windows {
        get {
            int columns = Columns;
            int rows = Rows;
            return toSeq(Enumerable.Range(0, rows).SelectMany(row => Enumerable.Range(0, columns).Select(column =>
                new TileWindow(
                    column, row, column * StrideX, row * StrideY,
                    EdgeX: TileEdge.Of(leads: column > 0, trails: column < columns - 1),
                    EdgeY: TileEdge.Of(leads: row > 0, trails: row < rows - 1)))));
        }
    }

    public FrozenDictionary<TileEdge, float[]> Ramps(int span, int taper) =>
        toSeq(TileEdge.Items).ToFrozenDictionary(
            static edge => edge,
            edge => {
                float[] ramp = new float[span];
                edge.Taper(ramp, Blend, taper);
                return ramp;
            });

    public void Accumulate(
        Span<float> weight, ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row, TileWindow window) {
        int originX = window.SourceX * Scale;
        int originY = window.SourceY * Scale;
        int span = Math.Min(TileWidth * Scale, OutputWidth - originX);
        if (span <= 0) { return; }
        Span2D<float> plane = weight.AsSpan2D(OutputHeight, OutputWidth);
        for (int y = 0; y < TileHeight * Scale; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)OutputHeight) { continue; }
            Span<float> weights = row[..span];
            TensorPrimitives.Multiply(rampX[..span], rampY[y], weights);
            Span<float> covered = plane.GetRowSpan(planeY).Slice(originX, span);
            TensorPrimitives.Add(covered, weights, covered);
        }
    }

    static int Steps(int extent, int tile, int overlap) =>
        extent <= tile ? 1 : 1 + (int)Math.Ceiling((double)(extent - tile) / (tile - overlap));

    static Seq<TileTensor> Seated(Seq<TileTensor> roster, TileProduct product, int slot) =>
        roster.Exists(tensor => StringComparer.Ordinal.Equals(tensor.Name, product.Tensor))
            ? roster.Map(tensor => StringComparer.Ordinal.Equals(tensor.Name, product.Tensor)
                ? tensor with {
                    Channels = tensor.Channels + product.Channels,
                    Slices = tensor.Slices.Add(new TileSlice(product, tensor.Channels, slot)),
                }
                : tensor)
            : roster.Add(new TileTensor(product.Tensor, product.Channels, Seq(new TileSlice(product, 0, slot))));

    static readonly Seq<TileGate> Gates = Seq(
        new TileGate("extent", static c => c.SourceWidth > 0 && c.SourceHeight > 0 && c.Channels > 0),
        new TileGate("roster-empty", static c => !c.Products.IsEmpty),
        new TileGate("roster-columns", static c => c.Products.ForAll(static product =>
            product.Tensor.Length > 0 && product.Role.Length > 0 && product.Lane >= 0 && product.Channels > 0)),
        new TileGate("role-collision", static c =>
            c.Products.Map(static product => product.Role).ToFrozenSet(StringComparer.Ordinal).Count == c.Products.Count),
        new TileGate("lane-collision", static c =>
            c.Products.Map(static product => $"{product.Tensor}#{product.Lane}").ToFrozenSet(StringComparer.Ordinal).Count == c.Products.Count),
        new TileGate("tensor-modality", static c =>
            c.Products.Map(static product => product.Tensor).ToFrozenSet(StringComparer.Ordinal).All(tensor =>
                c.Products.Filter(product => StringComparer.Ordinal.Equals(product.Tensor, tensor)) is var lanes
                && (lanes.ForAll(static lane => !lane.Graded) || lanes.ForAll(static lane => lane.Graded)))),
        new TileGate("grade-window", static c =>
            !c.Products.Exists(static product => product.Graded)
            || (Steps(c.SourceWidth, c.TileWidth, c.Overlap) is 1 && Steps(c.SourceHeight, c.TileHeight, c.Overlap) is 1)),
        new TileGate("card-admission", static c => c.Admission.Admits(c.TileWidth, c.TileHeight, c.Overlap)),
        new TileGate("scale", static c => c.Scale > 0),
        new TileGate("seam-containment", static c => c.Overlap * 2 < Math.Min(c.TileWidth, c.TileHeight)),
        new TileGate("blend-complementary", static c => c.Blend.Complementary()),
        new TileGate("addressable", static c => c.Products.ForAll(product =>
            (long)c.SourceWidth * c.SourceHeight * c.Scale * c.Scale * product.Channels <= Array.MaxLength)));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int sourceWidth, ref int sourceHeight, ref int channels, ref Seq<TileProduct> products,
        ref int tileWidth, ref int tileHeight, ref int overlap, ref int scale,
        ref TileAdmission admission, ref PadMode pad, ref TileBlend blend, ref TileLayout layout) {
        TileCandidate candidate = new(
            sourceWidth, sourceHeight, channels, products, tileWidth, tileHeight, overlap, scale, admission, blend);
        Seq<string> broken = Gates.Filter(gate => !gate.Holds(candidate)).Map(static gate => gate.Row);
        validationError = broken.IsEmpty
            ? null
            : new ValidationError(message:
                $"<tile-plan:{sourceWidth}x{sourceHeight}:{tileWidth}x{tileHeight}:{overlap}:{scale}:{string.Join(',', broken)}>");
    }
}
```

## [03]-[TILE_FOLD]

- Owner: `RunOps.InferTiled` is the mosaic fold and `RunOps.Canary` the single-window probe; `TilePlane` pairs one assembled field arena with the roster row that placed it; `TileGrade` carries one graded value; `TileMosaic` is the assembled field set beside the grade set and the one release point for every arena; `NormalizeProduct` is the item-partitioned close.
- Entry: `public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source)` on `BoundFlow` — one entry for the whole mosaic, because a per-tile entrypoint pushes the grid, the padding, the taper, and the coverage proof onto every caller; `source` is `ReadOnlyMemory` rather than a span so the scatter closure the run bracket invokes holds the arenas it writes.
- Law: coverage proves from the MEASURED weight floor — `TensorPrimitives.Min` over the weight plane — and a floor at or below zero refuses rather than dividing a texel no tile reached. The floor crosses out on the mosaic because a reassembly at 0.001 publishes as healthy without it.
- Law: closing the mosaic partitions by ITEM. `ParallelHelper.ForEach` over an `IRefAction<TilePlane>` hands each worker its own plane where the corpus's index-partitioned `For` rows hand it a slot number, and the products are independent by construction — each divides its own arena by the one shared weight plane and reads nothing another writes.
- Law: a partly-built mosaic never escapes. Every plane already allocated returns to the pool before a fault leaves, so an abandoned grid strands no arena and a caller that encodes and drops the mosaic returns each rental through one `Dispose`.
- Law: this fold SPILLS NOTHING. Every field accumulates in a pooled host arena for the whole grid and the mosaic owns those rentals, so PureHDF appears on no signature this page carries and no archive member reaches a Compute type. A plane budget past the arena ceiling is a REFUSAL at admission — `TilePlan`'s addressable gate — rather than a band-sealing writer whose ordering law had no producer to enforce it against.
- Auto: the fold allocates one cleared `MemoryOwner<float>` per field, walks `plan.Windows` sequentially through the one bound input, gathers each tile through the layout's own kernel, runs ONE pulse per window, zips the produced tensors against the plan's derived roster by position, scatters each field lane through its taper and reads each grade off its lane, accumulates the shared weight plane once per window, proves coverage, normalizes every plane in parallel, and hands back the mosaic. Result cardinality proves against the TENSOR count and each tensor's element count against its own modality-aware expectation.
- Receipt: a mosaic reports as one `ComputeReceipt.ModelRun` whose mode is the tiled key and whose `BatchSize` is `TileMosaic.Tiles`, the count inferred; per-tile and per-product receipt fan-out are the rejected forms for the same reason a batch window emits one — the grid ran once.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core
- Growth: a further mosaic-level measured column is one field on `TileMosaic`; a further per-window observation is one column on `TileWindow`; zero new surface for a wider roster, a packed tensor, or an added grade.
- Boundary: `InferTiled` composes the `Model/sessions#SESSION_CAPSULE` shared-arena `BoundFlow` and NEVER opens a session — the flow's bound input is the bucket and its bound outputs are the tensor roster, so a mosaic and its session warm-up name the same shapes by construction. Tiles run sequentially through the one bound input because the binding holds a single device-resident staging value; intra-tile parallelism belongs to the session's own thread pool, and the only fold this page partitions itself is the per-product normalize, which touches no binding at all. Every arena is a pooled `MemoryOwner<float>` released on the fold's exit, and the mosaic transfers one accumulation rental per product to the caller, so a failed pulse disposes every plane before the fault leaves.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public sealed record TilePlane(TileProduct.Plane Product, MemoryOwner<float> Plane);

public readonly record struct TileGrade(string Role, float Value);

public sealed class TileMosaic : IDisposable {
    internal TileMosaic(Seq<TilePlane> planes, Seq<TileGrade> grades, TilePlan plan, int tiles, float coverage) =>
        (Planes, Grades, Plan, Tiles, Coverage) = (planes, grades, plan, tiles, coverage);

    public Seq<TilePlane> Planes { get; }
    public Seq<TileGrade> Grades { get; }
    public TilePlan Plan { get; }
    public int Tiles { get; }
    public float Coverage { get; }

    public void Dispose() => Planes.Iter(static produced => produced.Plane.Dispose());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
readonly struct NormalizeProduct(TileLayout layout, ReadOnlyMemory<float> weight) : IRefAction<TilePlane> {
    public void Invoke(ref TilePlane produced) =>
        layout.Normalize(produced.Plane.Span, weight.Span, produced.Product.Channels);
}

public static partial class RunOps {
    extension(BoundFlow flow) {
        public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source) {
            if (source.Length != (long)plan.Channels * plan.SourceWidth * plan.SourceHeight) {
                return TileRefusal.SourceLength.Fault<TileMosaic>();
            }
            int texels = plan.OutputWidth * plan.OutputHeight;
            TilePlane[] planes = plan.Fields
                .Map(field => new TilePlane(field, MemoryOwner<float>.Allocate(texels * field.Channels, AllocationMode.Clear)))
                .ToArray();
            Seq<TileProduct.Measure> scorers = plan.Scorers;
            float[] grades = new float[scorers.Count];
            Seq<TileTensor> tensors = plan.Tensors;
            using MemoryOwner<float> weight = MemoryOwner<float>.Allocate(texels, AllocationMode.Clear);
            using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(plan.Channels * plan.TileHeight * plan.TileWidth);
            using MemoryOwner<float> row = MemoryOwner<float>.Allocate(plan.TileWidth * plan.Scale);
            FrozenDictionary<TileEdge, float[]> rampX = plan.Ramps(plan.TileWidth * plan.Scale, plan.Overlap * plan.Scale);
            FrozenDictionary<TileEdge, float[]> rampY = plan.Ramps(plan.TileHeight * plan.Scale, plan.Overlap * plan.Scale);
            long area = (long)plan.TileHeight * plan.Scale * plan.TileWidth * plan.Scale;
            Seq<TileWindow> windows = plan.Windows;
            foreach (TileWindow window in windows) {
                plan.Layout.Gather(source.Span, tile.Span, plan, window);
                Fin<Unit> pulsed = flow.Pulse(options, scope, new FlowPayload.Floats(tile.Memory), results => {
                    if (results.Count != tensors.Count) {
                        return TileRefusal.TensorCardinality.Fault<Unit>();
                    }
                    foreach ((OrtValue value, TileTensor tensor) in toSeq(results).Zip(tensors)) {
                        ReadOnlySpan<float> produced = value.GetTensorDataAsSpan<float>();
                        if (produced.Length != tensor.Expected(area) || !TensorPrimitives.IsFiniteAll(produced)) {
                            return TileRefusal.OutputLength.Fault<Unit>();
                        }
                        foreach (TileSlice slice in tensor.Slices) {
                            if (slice.Product.Graded) { grades[slice.Slot] = produced[slice.Offset]; continue; }
                            plan.Layout.Scatter(
                                produced, planes[slice.Slot].Plane.Span,
                                rampX[window.EdgeX], rampY[window.EdgeY], row.Span, plan, window,
                                slice.Product.Channels, slice.Offset, tensor.Channels);
                        }
                    }
                    return Fin.Succ(unit);
                });
                if (pulsed.Case is Error fault) { return Strand<TileMosaic>(planes, fault); }
                plan.Accumulate(weight.Span, rampX[window.EdgeX], rampY[window.EdgeY], row.Span, window);
            }
            float coverage = TensorPrimitives.Min<float>(weight.Span);
            if (coverage <= 0f) { return Strand<TileMosaic>(planes, TileRefusal.Coverage.Fault()); }
            ParallelHelper.ForEach<TilePlane, NormalizeProduct>(
                planes.AsMemory(), new NormalizeProduct(plan.Layout, weight.Memory), minimumActionsPerThread: 1);
            return Fin.Succ(new TileMosaic(
                toSeq(planes), scorers.Map((grade, index) => new TileGrade(grade.Role, grades[index])),
                plan, windows.Count, coverage));
        }

        public Fin<float[]> Canary(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source) {
            using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(plan.Channels * plan.TileHeight * plan.TileWidth);
            plan.Layout.Gather(source.Span, tile.Span, plan, new TileWindow(0, 0, 0, 0, TileEdge.Interior, TileEdge.Interior));
            return flow.Pulse(options, scope, new FlowPayload.Floats(tile.Memory), static results =>
                results.Count is 0
                    ? TileRefusal.CanaryOutput.Fault<float[]>()
                    : Fin.Succ(results.First().GetTensorDataAsSpan<float>().ToArray()));
        }
    }

    static Fin<T> Strand<T>(TilePlane[] planes, Error fault) {
        foreach (TilePlane held in planes) { held.Plane.Dispose(); }
        return Fin.Fail<T>(fault);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
