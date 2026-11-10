using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Gauge(float Value, Axis Axis) {
    internal const float Top = 0.25f;
    internal static readonly PlotPlane.Polar Turn = new(new Axis.Linear(AxisRange.Unit), Some<Axis>(new Axis.Linear(AxisRange.Unit)));
    internal static readonly View.Mark Band = new(new Figure.Region());
    private static readonly AxisRange Annulus = new((float)Fraction.Half, (float)Fraction.MaxValue);

    public Option<ColorAxis> Thresholds { get; init; }
    public Option<ColorValue> Color { get; init; }
    public Option<PhysicalAxis> Along { get; init; }
    public Fraction Sweep { get; init; } = new(0.75f);

    public View Drawn(CultureInfo culture) {
        float start = Top + ((float)Sweep / 2f);
        View.Mark swept = Band with { Role = Roles.Series, Series = new DataSeries(0) { Color = Color || Thresholds.Bind(axis => axis.Threshold(Value)) } };
        string spoken = Axis.Spoken(Value, culture);
        Seq<View> Marked(Func<float, float, View.Mark, Option<View>> placed) =>
            Thresholds.Map(thresholds => thresholds.Ramp.Stops.Zip(thresholds.Ramp.Stops.Tail.Map(static stop => stop.At).Add(Fraction.MaxValue)).Choose(band =>
                placed(Placed(thresholds.Axis.Value((float)band.First.At)), Placed(thresholds.Axis.Value((float)band.Second)), Band with {
                    Role = Roles.Series,
                    Series = new DataSeries(0) { Color = band.First.Color },
                    Conditions = [Condition.Disabled],
                }))).IfNone([])
            + placed(0f, Placed(Value), swept).ToSeq();
        return Along.Match(
            Some: along => new View.Flex(along, [Bar(along, Marked((from, to, mark) => Span(along, from, to, mark))), new View.Text([spoken]) { MaxLines = 1 }]).Stretched(along),
            None: () => new View.Plot(Turn,
                Arc(start, Sweep, 0f, 1f, Band with { Role = Roles.Track }).ToSeq()
                + Marked((from, to, mark) => Arc(start, Sweep, from, to, mark))
                + Seq<View>(new View.Pin(Vector2.Zero, new View.Text([spoken]) { Role = Roles.Stat, MaxLines = 1 }) { Align = UnitPoint.Center })) {
                Width = Sizing.Grow.Unit,
            }) with {
            Accessible = new Accessible { Role = AccessRole.ProgressBar, Value = new AccessValue.Ranged(Axis, Value) },
        };
    }

    internal static View Bar(PhysicalAxis along, Seq<View> marks) =>
        new View.Plot(PlotPlane.Track.Unit(along), marks) { Role = Roles.Track, Align = UnitPoint.Center }.Stretched(along);

    internal static Option<View> Span(PhysicalAxis along, float from, float to, View.Mark mark) =>
        AxisRange.Validated(from, to).ToOption().Map(span => (View)(mark with { Figure = along.Map(horizontal: new Figure.Region { First = span }, vertical: new Figure.Region { Second = span }) }));

    internal static Option<View> Arc(float start, Fraction sweep, float from, float to, View.Mark mark) =>
        AxisRange.Validated(start - ((float)sweep * to), start - ((float)sweep * from)).ToOption().Map(span => (View)(mark with { Figure = new Figure.Region { First = span, Second = Annulus } }));

    private float Placed(float value) => float.Clamp(Axis.Fraction(value), 0f, 1f);
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public Fraction Fraction => new((float)((double)Done / Total));

    public static Fin<Progress> Validated(long done, long total) => UIFault.Unless(total > 0L && done >= 0L && done <= total, UIReason.Range, (done, total)).Map(_ => new Progress(done, total));

    public static View Meter(Option<Progress> work) =>
        Gauge.Bar(PhysicalAxis.Horizontal, work.Bind(static progress => Gauge.Span(PhysicalAxis.Horizontal, 0f, (float)progress.Fraction, Gauge.Band with { Role = Roles.Span })).ToSeq()) with {
            Conditions = [Condition.Busy],
            Accessible = Announced(work),
        };

    public static View Ring(Option<Progress> work) =>
        work.Match(
            Some: static progress => (View)new View.Plot(Gauge.Turn, Seq(
                Gauge.Arc(Gauge.Top, Fraction.MaxValue, 0f, 1f, Gauge.Band with { Role = Roles.Track }),
                Gauge.Arc(Gauge.Top, Fraction.MaxValue, 0f, (float)progress.Fraction, Gauge.Band with { Role = Roles.Span })).Somes()) {
                Role = Roles.Spinner,
                Accessible = Announced(Some(progress)),
            },
            None: static () => new View.Box { Role = Roles.Spinner, Conditions = [Condition.Busy], Accessible = Announced(None) });

    private static Accessible Announced(Option<Progress> work) =>
        new() { Role = AccessRole.ProgressBar, Value = work.Map(static progress => (AccessValue)new AccessValue.Ranged(progress.Done, 0d, progress.Total)) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Content<T> where T : notnull {
    public Option<T> Current => Switch(pending: static pending => pending.Prior, ready: static ready => Some(ready.Value), empty: static _ => Option<T>.None, failed: static _ => Option<T>.None);

    public HashSet<Condition> Conditions => Map<HashSet<Condition>>(pending: [Condition.Busy], ready: [], empty: [], failed: [Condition.Faulted]);

    public Option<string> Message =>
        Switch(
            pending: static _ => Option<string>.None,
            ready: static _ => Option<string>.None,
            empty: static empty => Some(empty.Reason),
            failed: static failed => failed.Error.IsExpected ? Some(failed.Error.Message) : None);

    public Content<T> Requested(Option<Progress> work) => new Pending { Prior = Current, Work = work };

    public static Content<T> Arrived(Fin<T> arrival) => arrival.Match<Content<T>>(Succ: static value => new Ready(value), Fail: static error => new Failed(error));

    public View Drawn(Func<T, View> ready) =>
        Switch(
            ready,
            pending: static (held, pending) => pending.Prior.Map(value => held(value).Conditioned(Condition.Busy)).IfNone(() => Progress.Meter(pending.Work)),
            ready: static (held, ready) => held(ready.Value),
            empty: static (_, _) => new View.Box(),
            failed: static (_, _) => new View.Box());

    public sealed record Pending : Content<T> {
        public Option<T> Prior { get; init; }
        public Option<Progress> Work { get; init; }
    }

    public sealed record Ready(T Value) : Content<T>;

    public sealed record Empty(string Reason) : Content<T>;

    public sealed record Failed(Error Error) : Content<T>;
}

[SmartEnum]
public sealed partial class Polarity {
    public static readonly Polarity Positive = new(1);
    public static readonly Polarity Negative = new(-1);

    internal int Sign { get; }
}

[SmartEnum]
public sealed partial class ColorMode {
    public static readonly ColorMode Value = new(static color => new StyleSheet(HashMap((Roles.Stat, new Look { Text = color }))));
    public static readonly ColorMode Background = new(static color => new StyleSheet(HashMap((Roles.Card, new Look {
        Fills = Seq<Material>(new Material.Solid(color)),
        Text = new ColorValue.Contrasting(color, [new ColorValue.Host(Token.CanvasText), new ColorValue.Host(Token.Canvas)]),
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

    public View Drawn(CultureInfo culture) {
        Option<ColorValue> color = Thresholds.Bind(axis => axis.Threshold(Value));
        Seq<View> head = [
            new View.Text([Label]) { Role = Roles.Caption, MaxLines = 1 },
            new View.Flex(PhysicalAxis.Horizontal, Seq<View>(new View.Text([Axis.Formatted(Value, culture)]) { Role = Roles.Stat, MaxLines = 1 })
                + Axis.Unit.Map(static symbol => (View)new View.Text([symbol]) { Role = Roles.Caption }).ToSeq()
                + Prior.Map(prior => (View)Changed(prior, culture)).ToSeq()) {
                Baseline = Baseline.First,
            },
        ];
        View Card(Seq<View> content) =>
            new View.Flex(PhysicalAxis.Vertical, content) {
                Role = Roles.Card,
                Sheet = (from mode in Coloring from held in color select mode.Sheet(held)).IfNone(StyleSheet.Empty),
                Accessible = new Accessible { Role = AccessRole.Group, Label = Label, Value = new AccessValue.Ranged(Axis, Value) },
            }.Grown;
        return Trend.Bind(trend => Sparkline(trend, color)).Match(
            Some: spark => new View.Choice(PhysicalAxis.Vertical, [Card(head.Add(spark)), Card(head)]).Grown,
            None: () => Card(head));
    }

    private static Option<View> Sparkline(Samples trend, Option<ColorValue> color) =>
        from across in AxisRange.Validated(0f, trend.Values.Length - 1).ToOption()
        from extent in trend.Extent
        select new View.Plot(new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(extent)), [
            new View.Mark(new Figure.Curve(across, trend) { Fill = new Floor.Baseline(extent.Low) }) { Role = Roles.Series, Series = new DataSeries(0) { Color = color } },
        ]).Grown;

    private View.Text Changed(float prior, CultureInfo culture) {
        float change = Value - prior;
        string shown = prior is 0f ? Axis.Formatted(change, culture) : (change / MathF.Abs(prior)).ToString("P", culture);
        return new View.Text([change > 0f ? culture.NumberFormat.PositiveSign + shown : shown]) {
            Role = Roles.Label,
            Conditions = Polarity.Map(polarity => MathF.Sign(change) * polarity.Sign).IfNone(0) switch {
                > 0 => [Condition.Success],
                < 0 => [Condition.Faulted],
                _ => [],
            },
        };
    }
}
