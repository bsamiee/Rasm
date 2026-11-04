using Rasm.TestSupport;
using Rasm.UI;
using Rasm.UI.Layout;
using Rasm.UI.Motion;
using Rasm.UI.Plotting;
using SkiaSharp;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Layout {
    [Fact]
    public void WeightedCaps() =>
        Gen.Int[1, 16].Select(Gen.Int[1, 16], Gen.Int[0, 16], static (first, second, cap) => (First: first, Second: second, Cap: cap)).ForAll(static sample => {
            Seq<Sizing> sizes = [new Sizing.Grow(Factor.Validated(sample.First).ThrowIfFail()) { Limits = Limits.Validated(0f, sample.Cap).ThrowIfFail() }, new Sizing.Grow(Factor.Validated(sample.Second).ThrowIfFail())];
            float space = sample.Cap + sample.First + sample.Second;
            float first = (float)double.Min(sample.Cap, (double)space * sample.First / (sample.First + sample.Second));
            float second = first == sample.Cap ? space - first : (float)((double)space * sample.Second / (sample.First + sample.Second));
            Seq<float> actual = Sizing.Shared(sizes, [0f, 0f], space);
            Assert.Equal(first, actual[0]);
            Assert.Equal(second, actual[1]);
            Assert.Equal(actual.Rev(), Sizing.Shared(sizes.Rev(), [0f, 0f], space));
        });

    [Fact]
    public void SeparatedGrowWeights() {
        Seq<Sizing> sizes = [new Sizing.Grow(Factor.Validated(float.ScaleB(1f, 100)).ThrowIfFail()) { Limits = Limits.Validated(0f, 1f).ThrowIfFail() }, new Sizing.Grow(Factor.Validated(float.ScaleB(1f, -100)).ThrowIfFail())];
        Assert.Equal(Seq(1f, 2f), Sizing.Shared(sizes, [0f, 0f], 3f));
    }

    [Fact]
    public void SizingCopiesUseCurrentValues() {
        Seq<Sizing> sizes = [new Sizing.Fixed(2f) with { Extent = 5f }, new Sizing.Grow(Factor.Unit) with { Weight = Factor.Validated(3f).ThrowIfFail() }, new Sizing.Grow(Factor.Unit)];
        Assert.Equal(Seq(5f, 6f, 2f), Sizing.Shared(sizes, [0f, 0f, 0f], 13f));
    }

    [Fact]
    public void ShrinkTiers() {
        Seq<Sizing> sizes = [new Sizing.Fixed(10f), new Sizing.Grow(Factor.Unit) { Limits = Limits.Validated(2f, float.PositiveInfinity).ThrowIfFail() }, Sizing.Fit.Content, new Sizing.Percent(Fraction.Half)];
        Assert.Equal(Seq(10f, 2f, 4f, 4f), Sizing.Shared(sizes, [10f, 10f, 10f, 0f], 20f));
        Assert.Equal(Seq(10f, 2f, 0f, 0f), Sizing.Shared(sizes, [10f, 10f, 10f, 0f], 0f));
        Assert.Equal(Seq(10f, 10f, 10f, 0f), Sizing.Shared(sizes, [10f, 10f, 10f, 0f], float.PositiveInfinity));
    }

    [Fact]
    public void SaturatedGrowLeavesSpace() {
        Seq<Sizing> sizes = [new Sizing.Grow(Factor.Unit) { Limits = Limits.Validated(0f, 2f).ThrowIfFail() }, new Sizing.Grow(Factor.Unit) { Limits = Limits.Validated(0f, 3f).ThrowIfFail() }];
        Assert.Equal(Seq(2f, 3f), Sizing.Shared(sizes, [0f, 0f], 100f));
    }

    [Fact]
    public void EmptySankey() {
        Network network = Arrangements.Sankey(PlotPlane.Cartesian.Unit.In(SKRect.Create(100f, 100f)), [], Fraction.Half, Fraction.MinValue).ThrowIfFail();
        Assert.Empty(network.Nodes);
        Assert.Empty(network.Links);
    }

    [Fact]
    public void ZeroWeightSankey() {
        Flow flow = Flow.Validated(new Key.Ordinal(0), new Key.Ordinal(1), new Key.Ordinal(2), 0f).ThrowIfFail();
        Network network = Arrangements.Sankey(PlotPlane.Cartesian.Unit.In(SKRect.Create(100f, 100f)), [flow], Fraction.Half, Fraction.MinValue).ThrowIfFail();
        Arranged link = Assert.Single(network.Links);
        Assert.Equal(flow.Key, link.Key);
        Assert.Equal(flow.Weight, link.Weight);
        Assert.All(Assert.IsType<Figure.Path>(link.Figure).Vertices, static point => {
            Assert.InRange(point.X, 0f, 1f);
            Assert.InRange(point.Y, 0f, 1f);
        });
    }

    [Fact]
    public void ScaledAxisRangeAvoidsIntermediateOverflow() {
        float magnitude = float.ScaleB(1f, 126);
        AxisRange range = AxisRange.Validated(-3f * magnitude, -2f * magnitude).ThrowIfFail();
        AxisRange scaled = Assert.Single(range.Scaled(3f * magnitude, 0f, 0.5f).ToSeq());
        Assert.Equal(-3f * magnitude, scaled.From);
        Assert.Equal(-2.5f * magnitude, scaled.To);
    }

    [Fact]
    public void DiagonalEasingEndpoints() {
        foreach (double control in new[] { 0d, 1d }) Endpoints(Easing.Validated(control, control, control, control).ThrowIfFail());
        Gen.Double[0d, 1d].Select(Gen.Double[0d, 1d], static (first, second) => Easing.Validated(first, first, second, second).ThrowIfFail())
            .ForAll(Endpoints);

        static void Endpoints(Easing easing) {
            foreach (Fraction time in new[] { Fraction.MinValue, Fraction.MaxValue }) {
                (double progress, double slope) = Assert.Single(easing.At(time).ToSeq());
                Assert.Equal((float)time, progress);
                Assert.Equal(1d, slope);
            }
        }
    }
}
