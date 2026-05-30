using Rasm.Grasshopper.UI;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.UI;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class LayoutGens {
    public static readonly Gen<float> Coord = Gen.Double[start: -1.0e4, finish: 1.0e4].Select(static (double d) => (float)d);
    public static readonly Gen<float> Extent = Gen.Double[start: 0.0, finish: 1.0e4].Select(static (double d) => (float)d);
    public static readonly Gen<float> GapValue = Gen.Double[start: 0.0, finish: 1.0e3].Select(static (double d) => (float)d);
    public static readonly Gen<RectangleF> Rect = Coord.Select(Coord, Extent, Extent,
        static (float x, float y, float w, float h) => new RectangleF(x: x, y: y, width: w, height: h));
    public static readonly Gen<Guid> Id = Gen.Int.Select(static (int _) => Guid.NewGuid());
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
// LayoutAxis is the projection both ComputeCursor and ComputeStretch fold over. Oracle: the geometric
// definition of each axis (origin = leading edge, span = extent, delta = single-axis move to the cursor).
public sealed class LayoutAxisLaws {
    [Fact]
    public void HorizontalProjectsLeftAndWidth() =>
        Spec.ForAll(LayoutGens.Rect, static r => {
            Assert.Equal(expected: r.Left, actual: LayoutAxis.Horizontal.Origin(bounds: r));
            Assert.Equal(expected: r.Width, actual: LayoutAxis.Horizontal.Span(bounds: r));
        });

    [Fact]
    public void VerticalProjectsTopAndHeight() =>
        Spec.ForAll(LayoutGens.Rect, static r => {
            Assert.Equal(expected: r.Top, actual: LayoutAxis.Vertical.Origin(bounds: r));
            Assert.Equal(expected: r.Height, actual: LayoutAxis.Vertical.Span(bounds: r));
        });

    [Fact]
    public void DeltaMovesAlongOwnAxisOnly() =>
        Spec.ForAll(LayoutGens.Rect.Select(LayoutGens.Coord), static tuple => {
            (RectangleF r, float cursor) = tuple;
            (float hdx, float hdy) = LayoutAxis.Horizontal.Delta(cursor: cursor, bounds: r);
            Assert.Equal(expected: cursor - r.Left, actual: hdx);
            Assert.Equal(expected: 0f, actual: hdy);
            (float vdx, float vdy) = LayoutAxis.Vertical.Delta(cursor: cursor, bounds: r);
            Assert.Equal(expected: 0f, actual: vdx);
            Assert.Equal(expected: cursor - r.Top, actual: vdy);
        });

    // Fixed point: a cursor sitting on the axis origin produces zero movement — the invariant the cursor fold
    // relies on so the first distributed object never jumps.
    [Fact]
    public void DeltaAtOriginIsZero() =>
        Spec.ForAll(LayoutGens.Rect, static r => {
            Assert.Equal(expected: 0f, actual: LayoutAxis.Horizontal.Delta(cursor: r.Left, bounds: r).Dx);
            Assert.Equal(expected: 0f, actual: LayoutAxis.Vertical.Delta(cursor: r.Top, bounds: r).Dy);
        });
}

public sealed class LayoutGapLaws {
    [Fact]
    public void AcceptsNonNegativeFinite() =>
        Spec.ValueObjectAccepts(valid: LayoutGens.GapValue, tryCreate: static x => LayoutGap.TryCreate(value: x, obj: out _));

    [Fact]
    public void RejectsNegativeAndNonFinite() =>
        Spec.Cases(items: [-1f, -0.001f, float.NaN, float.PositiveInfinity], key: static x => x,
            law: static x => Assert.False(condition: LayoutGap.TryCreate(value: x, obj: out _)));

    [Fact]
    public void ValueRoundtrips() =>
        Spec.ValueObjectRoundtrip(validGen: LayoutGens.GapValue, tryCreate: LayoutGap.TryCreate, read: static (LayoutGap g) => g.Value);
}

// The per-policy slack that drives DistributeMode.Of's Cursor-vs-Stretch decision: Stretch tolerates sub-pixel
// overflow before invoking the solver; Fixed never stretches, so its slack is the identity (0).
public sealed class LayoutGapPolicyLaws {
    [Fact]
    public void StretchToleratesSubPixelAndFixedIsZero() {
        Assert.Equal(expected: 1e-4f, actual: LayoutGapPolicy.Stretch.ContentSlack);
        Assert.Equal(expected: 0f, actual: LayoutGapPolicy.Fixed.ContentSlack);
    }

    [Fact]
    public void ExactlyTwoPolicies() => Assert.Equal(expected: 2, actual: LayoutGapPolicy.Items.Count);
}

// Factory dispatch: the convenience constructors must select the right case + policy, and transport their ids
// intact, so distribution/grid/align route to the intended arrangement without silent policy loss.
public sealed class LayoutArrangementLaws {
    private static readonly Seq<Guid> Quad = Seq(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void DistributeStretchAndFixedSelectDistinctPolicies() {
        LayoutArrangement.DistributeCase stretch = Assert.IsType<LayoutArrangement.DistributeCase>(
            @object: LayoutArrangement.DistributeStretch(axis: LayoutAxis.Horizontal, gap: LayoutGap.Create(value: 8f), ids: Quad));
        LayoutArrangement.DistributeCase @fixed = Assert.IsType<LayoutArrangement.DistributeCase>(
            @object: LayoutArrangement.DistributeFixed(axis: LayoutAxis.Vertical, gap: LayoutGap.Create(value: 8f), ids: Quad));
        Assert.Same(expected: LayoutGapPolicy.Stretch, actual: stretch.GapPolicy);
        Assert.Same(expected: LayoutGapPolicy.Fixed, actual: @fixed.GapPolicy);
        Assert.Same(expected: LayoutAxis.Horizontal, actual: stretch.Axis);
        Assert.Equal(expected: 4, actual: stretch.Ids.Count);
    }

    [Fact]
    public void GridCarriesDimensionsAndIds() {
        LayoutArrangement.GridCase grid = Assert.IsType<LayoutArrangement.GridCase>(
            @object: LayoutArrangement.Grid(rows: 2, cols: 2, gap: LayoutGap.Create(value: 4f), ids: Quad, gapPolicy: LayoutGapPolicy.Stretch));
        Assert.Equal(expected: (2, 2, 4), actual: (grid.Rows, grid.Cols, grid.Ids.Count));
    }

    [Fact]
    public void AlignCarriesAnchorAndTargets() {
        Guid anchor = Guid.NewGuid();
        LayoutArrangement.AlignCase align = Assert.IsType<LayoutArrangement.AlignCase>(
            @object: LayoutArrangement.Align(anchor: anchor, targets: Seq(Guid.NewGuid(), Guid.NewGuid())));
        Assert.Equal(expected: anchor, actual: align.Anchor);
        Assert.Equal(expected: 2, actual: align.Targets.Count);
    }
}
