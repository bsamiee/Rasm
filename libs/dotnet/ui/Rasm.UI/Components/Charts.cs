using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Plotted(Key Key, string Label, Figure Figure) {
    public Option<Role> Role { get; init; }
    public Option<SKColorF> Color { get; init; }
    public Option<double> Value { get; init; }
    public Option<Vector2> LabelAt { get; init; }

    internal DataSeries Series(int ordinal, Option<ColorAxis> axis) =>
        new((uint)ordinal) { Color = Color, Value = axis.Match(Some: held => Value.Map(held.Fraction), None: () => Value) };

    // --- [STATISTICS]
    public static Fin<Plotted> Histogram(Key key, string label, Samples samples, Seq<AxisRange> buckets) =>
        UIFault.Unless(!buckets.IsEmpty && buckets.ForAll(static bucket => bucket.Span > 0f) && buckets.AsIterable().Window(2).All(static pair => pair[0].To.CompareTo(pair[1].From) is 0), UIReason.Order, buckets)
            .Bind(_ => Binned(buckets, Counted(samples, buckets)))
            .Map(figure => new Plotted(key, label, figure));

    public static Fin<Plotted> Density(Key key, string label, Samples samples, Factor bandwidth, AxisRange domain, int count) {
        double[] data = [.. Finite(samples)];
        return (UIFault.Unless(count > 1, UIReason.Range, count), UIFault.Unless(data.Length > 0, UIReason.Missing, samples.Values.Length))
            .Apply((_, _) => new Plotted(key, label, new Figure.Curve(domain, new Samples(Generate.LinearSpacedMap(count, domain.From, domain.To, at => (float)KernelDensity.EstimateGaussian(at, (float)bandwidth, data)))) {
                Fill = new Floor.Baseline(0f),
                Interpolation = Interpolation.Monotone,
            })).As();
    }

    public static Fin<Plotted> Box(Key key, string label, Samples samples, float at) => Summary.Of(samples).Map(summary => new Plotted(key, label, new Figure.Box(at, summary)));

    public static Fin<Plotted> Regression(Key key, string label, Seq<Vector2> points, int order) {
        double[] across = [.. points.Map(static point => (double)point.X)];
        double[] up = [.. points.Map(static point => (double)point.Y)];
        return UIFault.Unless(order >= 0 && order < points.Count, UIReason.Range, (order, points.Count))
            .Bind(_ => AxisRange.Validated((float)TensorPrimitives.Min(across), (float)TensorPrimitives.Max(across)))
            .Map(domain => Fit.PolynomialFunc(across, up, order) switch { var fit => new Plotted(key, label, new Figure.Graph(domain, at => (float)fit(at))) });
    }

    public static Plotted Arranged(Arranged arranged, string label) => new(arranged.Key, label, arranged.Figure) { Value = (double)arranged.Weight, LabelAt = arranged.At };

    private static Seq<double> Finite(Samples samples) => toSeq(MemoryMarshal.ToEnumerable(samples.Values).Where(float.IsFinite).Select(static value => (double)value));

    private static float[] Counted(Samples samples, Seq<AxisRange> buckets) {
        (float low, float high) = (buckets[0].From, buckets[^1].To);
        Histogram histogram = new();
        _ = buckets.Iter(bucket => histogram.AddBucket(new Bucket(bucket.From, bucket.To, 0d)));
        histogram.AddData(Finite(samples).Filter(value => value > low && value <= high));
        return [.. Enumerable.Range(0, histogram.BucketCount).Select(index => (float)histogram[index].Count)];
    }

    private static Fin<Figure> Binned(Seq<AxisRange> buckets, float[] counts) {
        AxisRange first = buckets[0];
        float center = first.From + (first.Span / 2f);
        return buckets.ForAll(bucket => bucket.Span.CompareTo(first.Span) is 0)
            ? AxisRange.Validated(center, center + (first.Span * int.Max(1, buckets.Count - 1))).Map(domain => (Figure)new Figure.Bars(domain, new Samples(counts)))
            : new Figure.Path(new Vector2(first.From, 0f).Cons(buckets.Map((bucket, index) => Seq(new Vector2(bucket.From, counts[index]), new Vector2(bucket.To, counts[index]))).Bind(static steps => steps))
                .Add(new Vector2(buckets[^1].To, 0f))) { Closed = true };
    }
}

[SmartEnum]
public sealed partial class Stacking {
    public static readonly Stacking Overlap = new(static (series, _) => series);
    public static readonly Stacking Stacked = new(static (series, _) => Summed(series, normalized: false));
    public static readonly Stacking Grouped = new(static (series, plane) => Dodged(series, plane));
    public static readonly Stacking Normalized = new(static (series, _) => Summed(series, normalized: true));

    [UseDelegateFromConstructor]
    internal partial Fin<Seq<Plotted>> Arranged(Seq<Plotted> series, PlotPlane plane);

    private static Fin<Seq<Plotted>> Summed(Seq<Plotted> series, bool normalized) =>
        from layers in series.Traverse(static plotted => Layered(plotted.Figure)).As()
        from aligned in UIFault.Unless(layers.ForAll(layer => layer.Domain == layers[0].Domain && layer.Values.Length == layers[0].Values.Length), UIReason.Range, layers.Map(static layer => (layer.Domain, layer.Values.Length)))
        let bounds = Bounds(layers, normalized)
        select series.Map((plotted, index) => plotted with { Figure = layers[index].Rebuilt(new Samples(bounds[index]), new Samples(bounds[index + 1])) });

    private static Seq<float[]> Bounds(Seq<Layer> layers, bool normalized) {
        Seq<float[]> sums = layers.Scan(new float[layers.Head.Map(static layer => layer.Values.Length).IfNone(0)], static (sum, layer) => Added(sum, layer.Values)).Strict();
        return normalized ? sums.Map(sum => Divided(sum, sums[^1])).Strict() : sums;
    }

    private static Fin<Seq<Plotted>> Dodged(Seq<Plotted> series, PlotPlane plane) {
        float bandwidth = plane.Dimension(PhysicalAxis.Horizontal).Map(static axis => axis.Bandwidth).IfNone(1f);
        return series.Map(static (plotted, index) => (Plotted: plotted, Index: index)).Traverse(held => held.Plotted.Figure is Figure.Bars bars
            ? from width in Fraction.Validated((float)bars.Width / series.Count)
              let offset = (((held.Index + 0.5f) / series.Count) - 0.5f) * (float)bars.Width * bandwidth * bars.Values.Slot(bars.Domain)
              from domain in AxisRange.Validated(bars.Domain.From + offset, bars.Domain.To + offset)
              select held.Plotted with { Figure = bars with { Domain = domain, Width = width } }
            : new UIFault(held.Plotted.Figure.ToString(), UIReason.Range)).As();
    }

    private static Fin<Layer> Layered(Figure figure) =>
        figure.Switch(
            figure,
            rule: Refused,
            path: Refused,
            region: Refused,
            curve: static (_, curve) => new Layer(curve.Domain, curve.Samples.Values, (lower, upper) => curve with { Samples = upper, Fill = curve.Fill.Map(_ => (Floor)new Floor.Lower(lower)) }),
            graph: Refused,
            grid: Refused,
            points: Refused,
            bars: static (_, bars) => new Layer(bars.Domain, bars.Values.Values, (lower, upper) => bars with { Values = upper, Bases = lower }),
            box: Refused,
            field: Refused,
            label: Refused);

    private static Fin<Layer> Refused<TCase>(Figure figure, TCase _) => new UIFault(figure.ToString(), UIReason.Range);

    private static float[] Added(float[] sum, ReadOnlyMemory<float> values) {
        float[] next = new float[sum.Length];
        TensorPrimitives.Add(sum, values.Span, next);
        return next;
    }

    private static float[] Divided(float[] sum, float[] totals) {
        float[] next = new float[sum.Length];
        TensorPrimitives.Divide(sum, totals, next);
        return next;
    }

    private sealed record Layer(AxisRange Domain, ReadOnlyMemory<float> Values, Func<Samples, Samples, Figure> Rebuilt);
}

public sealed record ColorAxis(Axis Axis, ColorRamp Ramp) {
    public Option<int> Bands { get; private init; }
    public Option<AxisRange> Shown { get; init; }
    public Option<Func<Edit<AxisRange>, IO<Unit>>> Filtered { get; init; }

    public Fin<ColorAxis> Banded(int count) => UIFault.Unless(count > 0, UIReason.Range, count).Map(_ => this with { Bands = count });

    internal double Fraction(double value) {
        double at = Axis.Fraction((float)value);
        return Bands.Match(Some: count => (Math.Clamp(Math.Floor(at * count), 0d, count - 1d) + 0.5d) / count, None: () => at);
    }

    internal bool Holds(double value) => Shown.ForAll(range => Axis.Contains(range, (float)value));
}

public sealed record Brush(PlotPlane Value, Func<Edit<PlotPlane>, IO<Unit>> Edited) {
    internal Node Drawn(PlotPlane plane) =>
        new Handles<PlotPlane>(Value, Ends(plane), Edited) { BackgroundCursor = CursorShape.Crosshair }.Drawn(plane, [
            new Node.Mark(new Figure.Region { First = Value.Dimension(PhysicalAxis.Horizontal).Map(static axis => axis.Domain), Second = Value.Dimension(PhysicalAxis.Vertical).Map(static axis => axis.Domain) }) {
                Role = Role.Selection,
            },
        ]) with {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };

    private static Points<PlotPlane> Ends(PlotPlane plane) {
        Vector2 middle = plane.Value(new SKPoint(0.5f, 0.5f));
        return new Points<PlotPlane>(
            brush => brush.Dimensions.Map((held, index) => Seq(
                new HandlePoint(new Key.Ordinal((uint)(2 * index)), held.Along.With(middle, held.Axis.Domain.From)),
                new HandlePoint(new Key.Ordinal((uint)((2 * index) + 1)), held.Along.With(middle, held.Axis.Domain.To)))).Bind(static points => points),
            (brush, targets, _) => new Dragged<PlotPlane>(targets.Fold(brush, (held, target) => Moved(plane, held, target)), targets.Map(static target => target.Key))) {
            Placed = (brush, at, _) =>
                from banded in brush.Dimensions.Fold(Some(brush), (held, free) =>
                    from current in held
                    from band in plane.Dimension(free.Along).Bind(static axis => axis is Axis.Band band ? Some(band) : None)
                    from range in AxisRange.Validated(band.Edge(free.Along.Of(at), 0), band.Edge(free.Along.Of(at), 1)).ToOption()
                    from next in current.WithDomain(free.Along, range)
                    select next)
                select new Added<PlotPlane>(banded, new Key.Ordinal(1u)),
        };
    }

    private static PlotPlane Moved(PlotPlane plane, PlotPlane brush, HandlePoint target) =>
        (from index in target.Key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None
         from free in brush.Dimensions.At(index / 2)
         let edge = plane.Dimension(free.Along).Bind(static axis => axis is Axis.Band band ? Some(band) : None).Match(Some: band => band.Edge(free.Along.Of(target.At), 0), None: () => free.Along.Of(target.At))
         from range in AxisRange.Validated(index % 2 is 0 ? edge : free.Axis.Domain.From, index % 2 is 0 ? free.Axis.Domain.To : edge).ToOption()
         from next in brush.WithDomain(free.Along, range)
         select next).IfNone(brush);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Legend {
    private const int Resolution = byte.MaxValue + 1;
    private static readonly PlotPlane Square = new PlotPlane.Cartesian(new Axis.Linear(AxisRange.Unit), new Axis.Linear(AxisRange.Unit));
    private static readonly Figure Line = new Figure.Rule(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
    private static readonly Figure Area = new Figure.Region();
    private static readonly Figure.Field Spectrum = Figure.Field.Validated(Resolution, 1, new Samples(Generate.LinearSpacedMap(Resolution, 0.5d / Resolution, 1d - (0.5d / Resolution), static at => (float)at))).ThrowIfFail();

    internal static Node.Mark Strip { get; } = new(Spectrum) { Role = Role.Well };

    public abstract Node Drawn { get; }

    internal static StyleSheet Scaled(ColorRamp ramp) =>
        new() { Looks = HashMap((Role.Well, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Data())), Strokes = Seq<Stroke>(), Scale = ramp })) };

    public sealed record Swatches(Seq<Plotted> Series) : Legend {
        public Option<Selection> Shown { get; init; }
        public Option<Func<Edit<Selection>, IO<Unit>>> Toggled { get; init; }

        internal Selection Visible => Shown.IfNone(() => new Selection.Many(toHashSet(Series.Map(static plotted => plotted.Key))));

        public override Node Drawn {
            get {
                Selection visible = Visible;
                Seq<(Key Key, Node Row)> rows = Series.Map((plotted, index) => (plotted.Key, (Node)Row(plotted, index, visible.Holds(plotted.Key)))).Strict();
                HashMap<Key, Node> keyed = toHashMap(rows);
                return Toggled.Match(
                    Some: toggled => new Part.Choice(rows.Map(static row => row.Key), visible, toggled).Drawn(key => keyed[key], Stacked),
                    None: () => Stacked(rows.Map(static row => row.Row)));
            }
        }

        private static Node Stacked(Seq<Node> rows) => new Node.Flex(PhysicalAxis.Vertical, rows) { Role = Role.Panel };

        private static Node.Flex Row(Plotted plotted, int ordinal, bool shown) {
            Node key = new Node.Plot(Square, [new Node.Mark(Keyed(plotted.Figure)) { Role = plotted.Role | Some(Role.Series), Series = plotted.Series(ordinal, None) }]) { Role = Role.Legend };
            return new Node.Flex(PhysicalAxis.Horizontal, [shown ? key : key.Conditioned(Condition.Disabled), new Node.Text([plotted.Label]) { Role = Role.Label }]) {
                Baseline = Baseline.Cap,
                Accessible = new Accessible { Label = plotted.Label },
            };
        }

        private static Figure Keyed(Figure figure) =>
            figure.Switch(
                rule: static _ => Line,
                path: static _ => Line,
                region: static _ => Area,
                curve: static _ => Line,
                graph: static _ => Line,
                grid: static _ => Line,
                points: static points => new Figure.Points([new Vector2(0.5f)]) { Marker = points.Marker },
                bars: static _ => Area,
                box: static _ => Area,
                field: static _ => Area,
                label: static _ => Line);
    }

    public sealed record Ramp(ColorAxis Axis) : Legend {
        public override Node Drawn {
            get {
                PlotPlane.Track plane = new(PhysicalAxis.Horizontal, Axis.Axis);
                Seq<Node> marks = Axis.Bands.Match(Some: count => toSeq(Range(0, count)).Choose(band => Banded(band, count)), None: static () => Seq<Node>(Strip));
                return new Node.Styled(plane.Stretched(new Node.Plot(plane, Axis.Filtered.Match(
                    Some: filtered => Seq(new Handles<AxisRange>(Axis.Shown.IfNone(Axis.Axis.Domain), Filter, filtered).Drawn(plane, marks) with { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }),
                    None: () => marks)) {
                    Role = Role.Legend,
                    Guides = [new Guide(PhysicalAxis.Horizontal)],
                })) {
                    Sheet = Scaled(Axis.Ramp),
                };
            }
        }

        private static Points<AxisRange> Filter { get; } = Points<AxisRange>.Fixed([
            Lens<AxisRange, Vector2>.New(static range => new Vector2(range.From, 0.5f), static to => range => AxisRange.Validated(to.X, range.To).ToOption().IfNone(range)),
            Lens<AxisRange, Vector2>.New(static range => new Vector2(range.To, 0.5f), static to => range => AxisRange.Validated(range.From, to.X).ToOption().IfNone(range)),
        ]);

        private Option<Node> Banded(int band, int count) =>
            AxisRange.Validated(Axis.Axis.Value(band / (float)count), Axis.Axis.Value((band + 1) / (float)count)).ToOption().Map(span => (Node)new Node.Mark(new Figure.Region { First = span }) {
                Role = Role.Well,
                Series = new DataSeries((uint)band) { Value = (band + 0.5d) / count },
            });
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
    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public Fin<Node> Drawn(CultureInfo culture) =>
        Stacking.Arranged(Series, Plane).Map(arranged => {
            Legend.Swatches swatches = new(Series) { Shown = Shown, Toggled = Toggled };
            Selection visible = swatches.Visible;
            string title = string.Join($"{culture.TextInfo.ListSeparator} ", Series.Map(static plotted => plotted.Label));
            Seq<(Key Key, Node Mark)> marks = arranged.Map((plotted, index) => (plotted.Key, Marked(plotted, index, visible.Holds(plotted.Key)))).Strict();
            HashMap<Key, Plotted> keyed = toHashMap(arranged.Map(static plotted => (plotted.Key, plotted)));
            Node plot = new Node.Plot(Plane, Layered(marks) + arranged.Choose(Pinned) + Brush.Map(brush => brush.Drawn(Plane)).ToSeq()) {
                Role = Role.Well,
                Width = Sizing.Grow.Unit,
                Guides = Guides,
                Pointed = Pointed,
                Crosshair = Some<Func<Nearest, Node>>(nearest => Crossed(nearest, keyed, culture)),
                Part = Navigation.Map(static viewport => (Part)viewport),
                Accessible = new Accessible { Role = AccessRole.Image, Label = title, Chart = Described(arranged, title) },
            };
            Node scaled = Color.Match(Some: axis => new Node.Styled(plot) { Sheet = new StyleSheet { Looks = HashMap((Role.Series, new Look { Scale = axis.Ramp })) } }, None: () => plot);
            return Color.Match(Some: static axis => Some(new Legend.Ramp(axis).Drawn), None: () => Series.Count > 1 || Toggled.IsSome ? Some(swatches.Drawn) : None).Match(
                Some: legend => new Node.Overlay(scaled, [legend with { Align = LegendAlign }]) { Width = Sizing.Grow.Unit },
                None: () => scaled);
        });

    private Node Marked(Plotted plotted, int ordinal, bool shown) {
        Node mark = new Node.Mark(plotted.Figure) { Key = plotted.Key, Role = plotted.Role | Some(Role.Series), Series = plotted.Series(ordinal, Color) };
        return shown && Color.ForAll(axis => plotted.Value.ForAll(axis.Holds)) ? mark : mark.Conditioned(Condition.Disabled);
    }

    private Seq<Node> Layered(Seq<(Key Key, Node Mark)> marks) =>
        Selected.Match(
            Some: selected => {
                HashMap<Key, Node> keyed = toHashMap(marks);
                return Seq(new Part.Choice(marks.Map(static held => held.Key), Selection, selected).Drawn(key => keyed[key], members => new Node.Plot(Plane, members) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }));
            },
            None: () => marks.Map(static held => held.Mark));

    private static Option<Node> Pinned(Plotted plotted) =>
        plotted.LabelAt.Map(at => (Node)new Node.Pin(at, new Node.Text([plotted.Label]) { Role = Role.Label }) { Align = UnitPoint.Center });

    private Node.Plot Crossed(Nearest nearest, HashMap<Key, Plotted> keyed, CultureInfo culture) {
        AxisRange across = Plane.Dimension(PhysicalAxis.Vertical).Map(static axis => axis.Domain).IfNone(AxisRange.Unit);
        Seq<(Plotted Plotted, Datum Datum)> read = toSeq<(Key Key, Datum Value)>(nearest.Data).Choose(held => keyed.Find(held.Key).Map(plotted => (plotted, held.Value))).Strict();
        return new Node.Plot(Plane,
            read.Map(static held => held.Datum.Value.X).Distinct().Map(at => (Node)new Node.Mark(new Figure.Rule(new Vector2(at, across.From), new Vector2(at, across.To))) { Role = Role.Crosshair })
            + read.Head.Map(first => (Node)new Node.Pin(first.Datum.Value, new Node.Flex(PhysicalAxis.Vertical, read.Map(held => (Node)new Node.Flex(PhysicalAxis.Horizontal, [
                new Node.Text([held.Plotted.Label]) { Role = Role.Label },
                new Node.Text([Plane.Spoken(held.Datum.Value, culture)]),
            ]))) { Role = Role.Readout })).ToSeq()) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private ChartDescription Described(Seq<Plotted> arranged, string title) {
        HashMap<PhysicalAxis, string> titles = toHashMap(Guides.Choose(static guide => guide.Title.Map(text => (guide.Along, text))));
        return new ChartDescription(
            title,
            Plane.Dimensions.Map(held => new AxisDescription(titles.Find(held.Along).IfNone(""), held.Axis)),
            arranged.Map(static plotted => new SeriesDescription(plotted.Label, plotted.Figure.Continuous, plotted.Figure.Data.Map(static datum => datum.Value))));
    }
}
