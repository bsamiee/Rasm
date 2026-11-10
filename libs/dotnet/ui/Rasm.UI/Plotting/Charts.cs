using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Plotted(Key Key, string Label, Figure Figure) {
    public Option<Key> Role { get; init; }
    public Option<ColorValue> Color { get; init; }
    public Option<double> Value { get; init; }
    public Option<Vector2> LabelAt { get; init; }

    public static Fin<Plotted> Histogram(Key key, string label, Samples samples, Seq<float> edges) =>
        UIFault.Unless(edges.Count >= 2 && edges.AsIterable().Window(2).All(static pair => pair[0] < pair[1]), UIReason.Order, edges)
            .Bind(_ => Binned(samples, edges))
            .Map(figure => new Plotted(key, label, figure));

    public static Fin<Plotted> Density(Key key, string label, Samples samples, Factor bandwidth, AxisRange domain, int count) {
        double[] data = [.. samples.Finite.Select(static value => (double)value)];
        return (UIFault.Unless(count > 1, UIReason.Range, count), UIFault.Unless(data.Length > 0, UIReason.Missing, samples.Values.Length))
            .Apply((_, _) => new Plotted(key, label, new Figure.Curve(domain, new Samples(Generate.LinearSpacedMap(count, domain.From, domain.To, at => (float)KernelDensity.EstimateGaussian(at, (float)bandwidth, data)))) {
                Fill = new Floor.Baseline(0f),
                Interpolation = Interpolation.Monotone,
            })).As();
    }

    public static Fin<Plotted> Regression(Key key, string label, Seq<Vector2> points, int order) {
        double[] across = [.. points.Map(static point => (double)point.X)];
        double[] up = [.. points.Map(static point => (double)point.Y)];
        return UIFault.Unless(order >= 0 && order < points.Count, UIReason.Range, (order, points.Count))
            .Bind(_ => AxisRange.Validated((float)TensorPrimitives.Min(across), (float)TensorPrimitives.Max(across)))
            .Map(domain => Fit.PolynomialFunc(across, up, order) switch { var fit => new Plotted(key, label, new Figure.Graph(domain, at => (float)fit(at))) });
    }

    public static Plotted Arranged(Arranged arranged, string label) => new(arranged.Key, label, arranged.Figure) { Value = (double)arranged.Weight, LabelAt = arranged.At };

    private static Fin<Figure> Binned(Samples samples, Seq<float> edges) {
        Seq<(float From, float To)> bins = toSeq(edges.AsIterable().Window(2).Select(static pair => (pair[0], pair[1]))).Strict();
        Histogram histogram = new();
        _ = bins.Iter(bin => histogram.AddBucket(new Bucket(bin.From, bin.To)));
        histogram.AddData(samples.Finite.Where(value => value >= edges[0] && value <= edges[^1]).Select(static value => (double)value));
        float[] counts = [.. Enumerable.Range(0, histogram.BucketCount).Select(index => (float)histogram[index].Count)];
        float width = edges[1] - edges[0];
        float center = edges[0] + (width / 2f);
        return bins.ForAll(bin => Precision.AlmostEqualRelative(bin.To - bin.From, width))
            ? AxisRange.Validated(center, center + (width * int.Max(1, bins.Count - 1))).Map(domain => (Figure)new Figure.Bars(domain, new Samples(counts)))
            : new Figure.Path(new Vector2(edges[0], 0f).Cons(bins.Map((bin, index) => Seq(new Vector2(bin.From, counts[index]), new Vector2(bin.To, counts[index]))).Bind(static steps => steps)).Add(new Vector2(edges[^1], 0f))) { Closed = true };
    }
}

[SmartEnum]
public sealed partial class Stacking {
    public static readonly Stacking Overlap = new(static (series, _) => series);
    public static readonly Stacking Stacked = new(static (series, _) => Summed(series, identity));
    public static readonly Stacking Grouped = new(static (series, plane) => Dodged(series, plane));
    public static readonly Stacking Normalized = new(static (series, _) => Summed(series, static sums => {
        float[] totals = System.Array.ConvertAll(sums[^1], static total => total is 0f ? 1f : total);
        return sums.Map(sum => Combined(sum, totals, TensorPrimitives.Divide)).Strict();
    }));

    private delegate void Combine(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> into);

    [UseDelegateFromConstructor]
    internal partial Fin<Seq<Plotted>> Arranged(Seq<Plotted> series, PlotPlane plane);

    private static Fin<Seq<Plotted>> Summed(Seq<Plotted> series, Func<Seq<float[]>, Seq<float[]>> transform) =>
        from layers in series.Traverse(static plotted => plotted.Figure.SwitchPartially<Key, Fin<(AxisRange Domain, ReadOnlyMemory<float> Values, Func<Samples, Samples, Figure> Rebuilt)>>(
            plotted.Key,
            @default: static (key, _) => new UIFault(key, UIReason.Range),
            curve: static (_, curve) => Pure((curve.Domain, curve.Samples.Values, fun((Samples lower, Samples upper) => (Figure)(curve with { Samples = upper, Fill = curve.Fill.Map(_ => (Floor)new Floor.Lower(lower)) })))),
            bars: static (_, bars) => Pure((bars.Domain, bars.Values.Values, fun((Samples lower, Samples upper) => (Figure)(bars with { Values = upper, Floor = new Floor.Lower(lower) })))))).As()
        from aligned in UIFault.Unless(layers.ForAll(layer => layer.Domain == layers[0].Domain && layer.Values.Length == layers[0].Values.Length), UIReason.Range, layers.Map(static layer => (layer.Domain, layer.Values.Length)))
        let bounds = transform(Sums(layers.Map(static layer => layer.Values)))
        select series.Map((plotted, index) => plotted with { Figure = layers[index].Rebuilt(new Samples(bounds[index]), new Samples(bounds[index + 1])) });

    private static Seq<float[]> Sums(Seq<ReadOnlyMemory<float>> layers) =>
        layers.Scan(new float[layers.Head.Map(static values => values.Length).IfNone(0)], static (sum, values) => Combined(sum, values.Span, TensorPrimitives.Add)).Strict();

    private static Fin<Seq<Plotted>> Dodged(Seq<Plotted> series, PlotPlane plane) =>
        series.Map(static (plotted, index) => (Plotted: plotted, Index: index)).Traverse(held => held.Plotted.Figure is Figure.Bars bars
            ? from width in Fraction.Validated((float)bars.Width / series.Count)
              let offset = (((held.Index + 0.5f) / series.Count) - 0.5f) * (float)bars.Width * plane.Bandwidth * bars.Values.Slot(bars.Domain)
              from domain in AxisRange.Validated(bars.Domain.From + offset, bars.Domain.To + offset)
              select held.Plotted with { Figure = bars with { Domain = domain, Width = width } }
            : new UIFault(held.Plotted.Key, UIReason.Range, held.Index)).As();

    private static float[] Combined(float[] sum, ReadOnlySpan<float> values, Combine combine) {
        float[] next = new float[sum.Length];
        combine(sum, values, next);
        return next;
    }
}

public sealed record ColorAxis(Axis Axis, ColorRamp Ramp) {
    public Option<AxisRange> Shown { get; init; }
    public Option<Func<Edit<AxisRange>, IO<Unit>>> Filtered { get; init; }

    internal Option<ColorValue> Threshold(float value) => Ramp.Valued(Axis.Fraction(value));

    internal static DataSeries Series(Option<ColorAxis> color, uint ordinal, Option<ColorValue> fixedColor, Option<double> value) =>
        new(ordinal) { Color = fixedColor, Value = value.Map(held => color.Map(axis => (double)axis.Axis.Fraction((float)held)).IfNone(held)) };

    internal static bool Shows(Option<ColorAxis> color, Option<double> value) =>
        color.ForAll(axis => value.ForAll(held => axis.Shown.ForAll(range => axis.Axis.Contains(range, (float)held))));

    internal static StyleSheet Sheet(Option<ColorAxis> color) => color.Map(static axis => new StyleSheet(HashMap((Roles.Series, new Look { Scale = axis.Ramp })))).IfNone(StyleSheet.Empty);
}

public sealed record Brush(PlotPlane Value, Func<Edit<PlotPlane>, IO<Unit>> Edited) {
    internal View Drawn(PlotPlane plane) =>
        new Handles<PlotPlane>(Value, Ends(plane), Edited) { BackgroundCursor = CursorShape.Crosshair }.Drawn(plane, [
            new View.Mark(new Figure.Region { First = Value.Dimension(PhysicalAxis.Horizontal).Map(static axis => axis.Domain), Second = Value.Dimension(PhysicalAxis.Vertical).Map(static axis => axis.Domain) }) {
                Role = Roles.Selection,
            },
        ]);

    private Points<PlotPlane> Ends(PlotPlane plane) =>
        Points<PlotPlane>.Fixed(Value.Dimensions.Bind(held => Seq(End(plane, held.Along, start: true), End(plane, held.Along, start: false)))) with {
            Placed = (brush, at, _) =>
                from banded in brush.Dimensions.Fold(Some(brush), (held, free) =>
                    from current in held
                    from band in Banded(plane, free.Along)
                    let under = free.Along.Of(at) - 0.5f
                    from range in AxisRange.Validated(band.Edge(under, 0), band.Edge(under, 1)).ToOption()
                    from next in current.WithDomain(free.Along, range)
                    select next)
                select (banded, (Key)new Key.Ordinal(1u)),
        };

    private static Lens<PlotPlane, Vector2> End(PlotPlane plane, PhysicalAxis along, bool start) =>
        Lens<PlotPlane, Vector2>.New(
            brush => along.With(plane.Center, brush.Dimension(along).Map(axis => start ? axis.Domain.From : axis.Domain.To).IfNone(along.Of(plane.Center))),
            to => brush =>
                (from axis in brush.Dimension(along)
                 let edge = Banded(plane, along).Map(band => band.Edge(along.Of(to), 0)).IfNone(along.Of(to))
                 from range in AxisRange.Validated(start ? edge : axis.Domain.From, start ? axis.Domain.To : edge).ToOption()
                 from next in brush.WithDomain(along, range)
                 select next).IfNone(brush));

    private static Option<Axis.Band> Banded(PlotPlane plane, PhysicalAxis along) => plane.Dimension(along).Bind(static axis => Optional(axis as Axis.Band));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Legend {
    private static readonly Figure Line = new Figure.Path([new Vector2(0f, 0.5f), new Vector2(1f, 0.5f)]);
    private static readonly Figure Area = new Figure.Region();

    internal static View.Mark Strip { get; } =
        new(new Figure.Field(Marks.Steps, 1, new Samples(Generate.LinearSpacedMap(Marks.Steps, 0.5d / Marks.Steps, 1d - (0.5d / Marks.Steps), static at => (float)at)))) { Role = Roles.Well };

    public abstract View Drawn { get; }

    internal static Option<Legend> Of(Option<ColorAxis> color, Option<Swatches> swatches) => color.Map(static axis => (Legend)new Ramp(axis)) | swatches.Map(static held => (Legend)held);

    internal static StyleSheet Scaled(ColorRamp ramp) =>
        new(HashMap((Roles.Well, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(new ColorValue.Data())), Scale = ramp })));

    public sealed record Swatches(Seq<Plotted> Series) : Legend {
        public Option<Selection> Shown { get; init; }
        public Option<Func<Edit<Selection>, IO<Unit>>> Toggled { get; init; }

        internal Selection Visible => Shown.IfNone(() => new Selection.Many(toHashSet(Series.Map(static plotted => plotted.Key))));

        public override View Drawn {
            get {
                Selection visible = Visible;
                Seq<View> rows = Series.Map((plotted, index) => (View)Row(plotted, index, visible.Holds(plotted.Key))).Strict();
                return Toggled.Match(
                    Some: toggled => new Part.Choice(Series.Map(static plotted => plotted.Key), visible, toggled).Drawn(rows, Stacked),
                    None: () => Stacked(rows));
            }
        }

        private static View Stacked(Seq<View> rows) => new View.Flex(PhysicalAxis.Vertical, rows) { Role = Roles.Panel };

        private static View.Flex Row(Plotted plotted, int ordinal, bool shown) {
            View key = new View.Plot(PlotPlane.Cartesian.Unit, [new View.Mark(Keyed(plotted.Figure)) { Role = plotted.Role | Some(Roles.Series), Series = ColorAxis.Series(None, (uint)ordinal, plotted.Color, plotted.Value) }]) { Role = Roles.Legend };
            return new View.Flex(PhysicalAxis.Horizontal, [shown ? key : key.Conditioned(Condition.Disabled), new View.Text([plotted.Label]) { Role = Roles.Label }]) {
                Baseline = Baseline.Cap,
                Accessible = new Accessible { Label = plotted.Label },
            };
        }

        private static Figure Keyed(Figure figure) =>
            figure.SwitchPartially(
                @default: static held => held.Continuous ? Line : Area,
                grid: static _ => Line,
                points: static points => new Figure.Points([new Vector2(0.5f)]) { Marker = points.Marker },
                label: static _ => Line);
    }

    public sealed record Ramp(ColorAxis Axis) : Legend {
        public override View Drawn {
            get {
                PlotPlane.Track plane = new(PhysicalAxis.Horizontal, Axis.Axis);
                return new View.Plot(plane, [
                    Axis.Filtered.Match(Some: filtered => new Handles<AxisRange>(Axis.Shown.IfNone(Axis.Axis.Domain), Filter, filtered).Drawn(plane, [Strip]), None: static () => Strip),
                ]) {
                    Role = Roles.Legend,
                    Guides = [new Guide(PhysicalAxis.Horizontal)],
                    Sheet = Scaled(Axis.Ramp),
                }.Stretched(plane.Along);
            }
        }

        private static Points<AxisRange> Filter { get; } = Points<AxisRange>.Fixed([
            Lens<AxisRange, Vector2>.New(static range => new Vector2(range.From, 0.5f), static to => range => AxisRange.Validated(to.X, range.To).ToOption().IfNone(range)),
            Lens<AxisRange, Vector2>.New(static range => new Vector2(range.To, 0.5f), static to => range => AxisRange.Validated(range.From, to.X).ToOption().IfNone(range)),
        ]);
    }
}

public sealed record Chart(PlotPlane Plane, Seq<Plotted> Series) {
    public Seq<Guide> Guides { get; init; }
    public Stacking Stacking { get; init; } = Stacking.Overlap;
    public Option<ColorAxis> Color { get; init; }
    public UnitPoint LegendAlign { get; init; } = new(Fraction.MaxValue, Fraction.MinValue);
    public Option<Brush> Brush { get; init; }
    public Option<Part.Viewport> Navigation { get; init; }
    public Option<Func<Option<Nearest>, IO<Unit>>> Pointed { get; init; }
    public Option<Selection> Shown { get; init; }
    public Option<Func<Edit<Selection>, IO<Unit>>> Toggled { get; init; }
    public Selection Selection { get; init; } = Selection.Empty;
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public Fin<View> Drawn(CultureInfo culture) =>
        Stacking.Arranged(Series, Plane).Map(arranged => {
            Legend.Swatches swatches = new(Series) { Shown = Shown, Toggled = Toggled };
            Selection visible = swatches.Visible;
            string title = culture.TextInfo.Listed(Series.Map(static plotted => plotted.Label));
            Seq<View> marks = arranged.Map((plotted, index) => Marked(plotted, index, visible.Holds(plotted.Key))).Strict();
            HashMap<Key, Plotted> keyed = toHashMap(arranged.Map(static plotted => (plotted.Key, plotted)));
            View plot = new View.Plot(Plane, Layered(arranged.Map(static plotted => plotted.Key), marks) + arranged.Choose(Pinned) + Brush.Map(brush => brush.Drawn(Plane)).ToSeq()) {
                Role = Roles.Well,
                Width = Sizing.Grow.Unit,
                Guides = Guides,
                Pointed = Pointed,
                Crosshair = Some<Func<Nearest, View>>(nearest => Crossed(nearest, keyed, culture)),
                Part = Navigation.Map(static viewport => (Part)viewport),
                Accessible = new Accessible { Role = AccessRole.Image, Label = title, Chart = Described(arranged, title) },
                Sheet = ColorAxis.Sheet(Color),
            };
            return Legend.Of(Color, Some(swatches).Filter(_ => Series.Count > 1 || Toggled.IsSome)).Match(
                Some: legend => new View.Overlay(plot, [legend.Drawn with { Align = LegendAlign }]),
                None: () => plot);
        });

    private View Marked(Plotted plotted, int ordinal, bool shown) {
        View mark = new View.Mark(plotted.Figure) { Key = plotted.Key, Role = plotted.Role | Some(Roles.Series), Series = ColorAxis.Series(Color, (uint)ordinal, plotted.Color, plotted.Value) };
        return shown && ColorAxis.Shows(Color, plotted.Value) ? mark : mark.Conditioned(Condition.Disabled);
    }

    private Seq<View> Layered(Seq<Key> keys, Seq<View> marks) =>
        Selected.Match(
            Some: selected => Seq(new Part.Choice(keys, Selection, selected).Drawn(marks, members => new View.Plot(Plane, members).Grown)),
            None: () => marks);

    private static Option<View> Pinned(Plotted plotted) =>
        plotted.LabelAt.Map(at => (View)new View.Pin(at, new View.Text([plotted.Label]) { Role = Roles.Label }) { Align = UnitPoint.Center });

    private View Crossed(Nearest nearest, HashMap<Key, Plotted> keyed, CultureInfo culture) {
        AxisRange across = Plane.Dimension(PhysicalAxis.Vertical).Map(static axis => axis.Domain).IfNone(AxisRange.Unit);
        Seq<(Plotted Plotted, Datum Datum)> read = toSeq<(Key Key, Datum Value)>(nearest.Data).Choose(held => keyed.Find(held.Key).Map(plotted => (plotted, held.Value))).Strict();
        return new View.Plot(Plane,
            read.Map(static held => held.Datum.Value.X).Distinct().Map(at => (View)new View.Mark(new Figure.Path([new Vector2(at, across.From), new Vector2(at, across.To)])) { Role = Roles.Crosshair })
            + read.Head.Map(first => (View)new View.Pin(first.Datum.Value, new View.Flex(PhysicalAxis.Vertical, read.Map(held => (View)new View.Flex(PhysicalAxis.Horizontal, [
                new View.Text([held.Plotted.Label]) { Role = Roles.Label },
                new View.Text([Plane.Spoken(held.Datum.Value, culture)]),
            ]))) { Role = Roles.Readout })).ToSeq()).Grown;
    }

    private ChartDescription Described(Seq<Plotted> arranged, string title) {
        HashMap<PhysicalAxis, string> titles = toHashMap(Guides.Choose(static guide => guide.Title.Map(text => (guide.Along, text))));
        return new ChartDescription(
            title,
            Plane.Dimensions.Map(held => new AxisDescription(titles.Find(held.Along), held.Axis)),
            arranged.Map(static plotted => new SeriesDescription(plotted.Label, plotted.Figure)));
    }
}
