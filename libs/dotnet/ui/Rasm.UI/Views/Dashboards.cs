using System.Globalization;
using System.Numerics;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SuperLinq;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Polarity {
    public static readonly Polarity Positive = new(1);
    public static readonly Polarity Negative = new(-1);

    internal int Sign { get; }

    internal HashSet<Condition> Judged(float change) =>
        (MathF.Sign(change) * Sign) switch {
            > 0 => [Condition.Success],
            < 0 => [Condition.Faulted],
            _ => [],
        };
}

[SmartEnum]
public sealed partial class ColorMode {
    public static readonly ColorMode Value = new(static color => new StyleSheet(HashMap((Role.Stat, new Look { Text = color }))));
    public static readonly ColorMode Background = new(static color => new StyleSheet(HashMap((Role.Card, new Look {
        Fills = Seq<Material>(new Material.Solid(color)),
        Text = new ColorValue.Contrasting(color, [new ColorValue.Host(Channel.Text) { Role = Role.Window }, new ColorValue.Host(Channel.Fill) { Role = Role.Window }]),
    }))));

    [UseDelegateFromConstructor]
    internal partial StyleSheet Sheet(ColorValue color);
}

public sealed record Stat(string Label, float Value, Axis Axis) {
    public Option<float> Prior { get; init; }
    public Option<Polarity> Polarity { get; init; }
    public Option<ColorAxis> Thresholds { get; init; }
    public Option<Samples> Trend { get; init; }
    public Option<ColorMode> Coloring { get; init; } = ColorMode.Value;

    public Node Drawn(CultureInfo culture) {
        Option<ColorValue> color = Thresholds.Bind(axis => Dashboards.Threshold(axis, Value));
        Seq<Node> head = [
            new Node.Text([Label]) { Role = Role.Caption, MaxLines = 1 },
            new Node.Flex(PhysicalAxis.Horizontal, Seq<Node>(new Node.Text([Axis.Formatted(Value, culture)]) { Role = Role.Stat, MaxLines = 1 })
                + Axis.Unit.Map(static symbol => (Node)new Node.Text([symbol]) { Role = Role.Caption }).ToSeq()
                + Prior.Map(prior => (Node)Changed(prior, culture)).ToSeq()) {
                Baseline = Baseline.First,
            },
        ];
        Node Card(Seq<Node> content) =>
            new Node.Flex(PhysicalAxis.Vertical, content) {
                Role = Role.Card,
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
                Sheet = (from mode in Coloring from held in color select mode.Sheet(held)).IfNone(StyleSheet.Empty),
                Accessible = new Accessible { Role = AccessRole.Group, Label = Label, Value = new AccessValue.Ranged(Value, Axis.Domain.Low, Axis.Domain.High) { Axis = Axis } },
            };
        return Trend.Bind(trend => Sparkline(trend, color)).Match(
            Some: spark => new Node.Choice(PhysicalAxis.Vertical, [Card(head.Add(spark)) with { Key = new Key.Ordinal(0) }, Card(head) with { Key = new Key.Ordinal(1) }]) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            },
            None: () => Card(head));
    }

    private static Option<Node> Sparkline(Samples trend, Option<ColorValue> color) =>
        from across in AxisRange.Validated(0f, trend.Values.Length - 1).ToOption()
        from extent in trend.Extent
        select (Node)new Node.Plot(new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(extent)), [
            new Node.Mark(new Figure.Curve(across, trend) { Fill = new Floor.Baseline(extent.Low) }) { Role = Role.Series, Series = new DataSeries(0) { Color = color } },
        ]) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };

    private Node.Text Changed(float prior, CultureInfo culture) {
        float change = Value - prior;
        string shown = prior is 0f ? Axis.Formatted(change, culture) : (change / MathF.Abs(prior)).ToString("P", culture);
        return new Node.Text([change > 0f ? culture.NumberFormat.PositiveSign + shown : shown]) {
            Role = Role.Label,
            Conditions = Polarity.Map(polarity => polarity.Judged(change)).IfNone([]),
        };
    }
}

public sealed record Gauge(float Value, Axis Axis) {
    private static readonly Fraction Sweep = Fraction.Validated(0.75f).ThrowIfFail();
    private static readonly AxisRange Body = AxisRange.Validated(0.62f, 0.86f).ThrowIfFail();
    private static readonly AxisRange Rim = AxisRange.Validated(0.9f, 1f).ThrowIfFail();
    private static readonly PlotPlane.Polar Dial = new(
        new Axis.Linear(AxisRange.Validated((0.25f + ((float)Sweep / 2f)) / (float)Sweep, (0.25f + ((float)Sweep / 2f) - 1f) / (float)Sweep).ThrowIfFail()),
        Some<Axis>(new Axis.Linear(AxisRange.Unit)));

    public Option<ColorAxis> Thresholds { get; init; }
    public Option<ColorValue> Color { get; init; }
    public Option<PhysicalAxis> Along { get; init; }

    public Node Drawn(CultureInfo culture) {
        float at = Placed(Value);
        DataSeries series = new(0) { Color = Color | Thresholds.Bind(axis => Dashboards.Threshold(axis, Value)) };
        string spoken = Axis.Spoken(Value, culture);
        return Along.Match<Node>(Some: along => Bar(along, at, series, spoken), None: () => Arc(at, series, spoken)) with {
            Accessible = new Accessible { Role = AccessRole.ProgressBar, Value = new AccessValue.Ranged(Value, Axis.Domain.Low, Axis.Domain.High) { Axis = Axis } },
        };
    }

    private float Placed(float value) => float.Clamp(Axis.Fraction(value), 0f, 1f);

    private Node.Plot Arc(float at, DataSeries series, string spoken) =>
        new(Dial, Seq<Node>(new Node.Mark(new Figure.Region { First = AxisRange.Unit, Second = Body }) { Role = Role.Track })
            + Swept(at).Map(span => (Node)new Node.Mark(new Figure.Region { First = span, Second = Body }) { Role = Role.Series, Series = series }).ToSeq()
            + Thresholds.Map(Bands).IfNone([])
            + Seq<Node>(new Node.Pin(Vector2.Zero, new Node.Text([spoken]) { Role = Role.Stat, MaxLines = 1 }) { Align = UnitPoint.Center })) {
            Width = Sizing.Grow.Unit,
        };

    private static Node.Flex Bar(PhysicalAxis along, float at, DataSeries series, string spoken) {
        PlotPlane.Track plane = PlotPlane.Track.Unit(along);
        return new Node.Flex(along, [
            plane.Stretched(new Node.Plot(plane, Swept(at).Map(span => (Node)new Node.Mark(along.Map(horizontal: new Figure.Region { First = span }, vertical: new Figure.Region { Second = span })) {
                Role = Role.Series,
                Series = series,
            }).ToSeq()) { Role = Role.Track, Align = UnitPoint.Center }),
            new Node.Text([spoken]) { MaxLines = 1 },
        ]) {
            Width = Sizing.Grow.Unit,
        };
    }

    private Seq<Node> Bands(ColorAxis thresholds) =>
        thresholds.Ramp.Stops.Zip(thresholds.Ramp.Stops.Tail.Map(static stop => stop.At).Add(Fraction.MaxValue)).Choose(band =>
            AxisRange.Validated(Placed(thresholds.Axis.Value((float)band.First.At)), Placed(thresholds.Axis.Value((float)band.Second))).ToOption()
                .Map(span => (Node)new Node.Mark(new Figure.Region { First = span, Second = Rim }) { Role = Role.Series, Series = new DataSeries(0) { Color = band.First.Color } }));

    private static Option<AxisRange> Swept(float at) => AxisRange.Validated(0f, at).ToOption();
}

public sealed record Board<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, Node> Face, int Columns) where TItem : notnull {
    public Func<TItem, int> Span { get; init; } = static _ => 1;
    public Func<TItem, int> Rows { get; init; } = static _ => 1;
    public Factor Cell { get; init; } = Factor.Unit;
    public Option<Func<Seq<Key>, Part.Choice>> Choice { get; init; }

    public Fin<Node> Drawn =>
        toSeq(SuperEnumerable.Generate(Columns, static count => count / 2).TakeWhile(static count => count > 0))
            .Traverse(count => Laid(count).Map(laid => laid with { Key = new Key.Ordinal((uint)count) })).As()
            .Map(static alternatives => (Node)new Node.Choice(PhysicalAxis.Horizontal, alternatives) { Width = Sizing.Grow.Unit });

    private Fin<Node> Laid(int count) =>
        Items.Traverse(item => Placed(item, count)).As().Map(tiles => Choice.Match(
            Some: choice => toHashMap(tiles.Map(static tile => (tile.Key, tile.Node))) switch {
                var faces => choice(tiles.Map(static tile => tile.Key)).Drawn(key => faces[key], members => Gridded(members, count)),
            },
            None: () => Gridded(tiles.Map(static tile => tile.Node), count)));

    private Fin<(Key Key, Node Node)> Placed(TItem item, int count) {
        (int columns, int rows) = (int.Clamp(Span(item), 1, count), int.Max(1, Rows(item)));
        return Factor.Validated(columns * (float)Cell / rows).Map(aspect => (Keyed(item), Face(item) with { Key = Keyed(item), Spanning = new CellSpan(columns, rows), Aspect = aspect }));
    }

    private static Node.Grid Gridded(Seq<Node> tiles, int count) =>
        new(new Tracks(toSeq(Enumerable.Repeat<Sizing>(Sizing.Grow.Unit, count))), [tiles]) {
            Role = Role.Stack,
            Width = Sizing.Grow.Unit,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Dashboards {
    internal static Option<ColorValue> Threshold(ColorAxis axis, float value) => axis.Ramp.Valued(axis.Axis.Fraction(value));
}
