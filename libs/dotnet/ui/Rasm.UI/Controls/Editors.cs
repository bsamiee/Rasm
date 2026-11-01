using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradientEditor(ColorRamp Value, Func<Edit<ColorRamp>, IO<Unit>> Edited) {
    private static readonly PlotPlane.Track Track = PlotPlane.Track.Unit(PhysicalAxis.Horizontal);

    public Selection Selection { get; init; } = Selection.Empty;
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public View Drawn =>
        new Handles<ColorRamp>(Value, Stops, Edited) {
            Selection = Selection,
            Selected = Selected,
            Handle = (key, at) => new View.Pin(at, new View.Box { Role = Role.Stop, Conditions = Selection.Holds(key) ? [Condition.Selected] : [] }) { Align = UnitPoint.Center },
        }.Drawn(Track, [Legend.Strip]) with {
            Sheet = Legend.Scaled(Value),
        };

    private static Points<ColorRamp> Stops { get; } =
        Points<ColorRamp>.Listed(
            static ramp => ramp.Stops,
            static stop => new Vector2((float)stop.At, 0.5f),
            static (stop, to) => Fraction.Validated(to.X).ToOption().Map(at => stop with { At = at }),
            static (ramp, stops) => stops.IsEmpty ? None : Some(ramp.WithStops(stops)),
            static (ramp, at) =>
                from position in Fraction.Validated(at.X).ToOption()
                from color in ramp.At(position)
                select new Stop(color, position));
}

public sealed record CurveEditor(PlotPlane.Cartesian Plane, Seq<Vector2> Points, Func<float, float> Fit, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Seq<Func<float, float>> References { get; init; }
    public Option<Samples> Underlay { get; init; }
    public Seq<View> Background { get; init; }
    public DataSeries Series { get; init; } = new(0);
    public Selection Selection { get; init; } = Selection.Empty;
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }

    public View Drawn =>
        new Handles<Seq<Vector2>>(Points, Knots, Edited) { Selection = Selection, Selected = Selected }.Drawn(Plane,
            Background
            + Underlay.Map(underlay => (View)new View.Mark(new Figure.Curve(Plane.X.Domain, underlay) { Fill = new Floor.Baseline(0f) }) { Role = Role.Trace }).ToSeq()
            + Seq<View>(new View.Mark(new Figure.Grid(PhysicalAxis.Horizontal)) { Role = Role.Grid }, new View.Mark(new Figure.Grid(PhysicalAxis.Vertical)) { Role = Role.Grid })
            + (Plane.X == Plane.Y ? Seq<View>(new View.Mark(new Figure.Path([new Vector2(Plane.X.Domain.From), new Vector2(Plane.X.Domain.To)])) { Role = Role.GridFine }) : [])
            + References.Map(reference => (View)new View.Mark(new Figure.Graph(Plane.X.Domain, reference)) { Role = Role.Trace })
            + Seq<View>(new View.Mark(new Figure.Graph(Plane.X.Domain, Fit)) { Role = Role.Series, Series = Series })) with {
            Role = Role.Well,
            Aspect = Factor.Unit,
        };

    private Points<Seq<Vector2>> Knots =>
        Points<Seq<Vector2>>.Listed(
            static knots => knots,
            static knot => knot,
            static (_, to) => Some(to),
            static (_, knots) => Some(toSeq(knots.OrderBy(static knot => knot.X))),
            (_, at) => Some(at with { Y = Plane.Y.Constrained(Fit(at.X)) }));
}

public sealed record Slider(PhysicalAxis Along, AxisRange Domain, Seq<float> Value, Func<Edit<Seq<float>>, IO<Unit>> Edited) {
    public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
    public Option<AxisRange> SoftRange { get; init; }
    public Option<float> Step { get; init; }
    public Option<float> Origin { get; init; }
    public Option<Seq<float>> Rest { get; init; }
    public Seq<View> Background { get; init; }
    public Role Role { get; init; } = Role.Track;

    public View Drawn(CultureInfo culture) =>
        Placed.Match(
            Succ: axis => Shown(axis, culture),
            Fail: error => Shown(Laid(SoftRange.IfNone(Domain)), culture).Conditioned(Condition.Faulted) switch {
                var faulted => faulted with { Accessible = faulted.Accessible.Combine(new Accessible { Help = error.Message }) },
            });

    private Fin<Axis> Placed {
        get {
            Axis soft = Laid(SoftRange.IfNone(Domain));
            (float low, float high) = Value.Fold((Low: 0f, High: 1f), (held, value) => soft.Fraction(value) switch { var at => (float.Min(held.Low, at), float.Max(held.High, at)) });
            return low < 0f || high > 1f
                ? AxisRange.Validated(low < 0f ? float.Max(soft.Value((2f * low) - 1f), Domain.Low) : soft.Domain.From, high > 1f ? float.Min(soft.Value(2f * high), Domain.High) : soft.Domain.To).Map(Laid)
                : soft;
        }
    }

    private Axis Laid(AxisRange range) => Track(range) switch { var axis => axis with { Places = axis.Places | Step.Map(Axis.Digits) } };

    private View Shown(Axis axis, CultureInfo culture) {
        PlotPlane.Track plane = new(Along, axis);
        bool filled = Role != Role.Track;
        Seq<float> ends = Value is [var only] ? Seq(Origin.IfNone(axis.Domain.From), only) : Value;
        HashSet<Condition> marks = Edit.Marked(Rest, Some(Value));
        View slid = new Handles<Seq<float>>(Value, Thumbs(plane), Edited) { Rest = Rest }.Drawn(plane,
            (filled ? [] : Seq(new View.Box { Role = Role }.Stretched(Along))) + Background
            + (from first in ends.Head from last in ends.Last from span in AxisRange.Validated(first, last).ToOption() select (View)new View.Mark(new Figure.Region { First = Some(span) }) { Role = Role.Span }).ToSeq());
        return (filled ? new View.Overlay(slid, Seq(Readout(axis, culture, marks)) + Entry(axis, culture).ToSeq()) { Role = Role, Conditions = marks } : slid).Stretched(Along);
    }

    private Option<View> Entry(Axis axis, CultureInfo culture) =>
        Value is [var only]
            ? Some<View>(new View.Layer(new TextField(axis.Formatted(only, culture),
                edit => Edit.Typed(edit, text => axis.Parsed(Domain, text, culture).Map(typed => Seq(Quantized(typed))), Edited)).Drawn) {
                Side = UnitPoint.Center,
                Align = UnitPoint.Center,
                Trigger = new Trigger.Press(),
            })
            : None;

    private View Readout(Axis axis, CultureInfo culture, HashSet<Condition> marks) =>
        Value.Last.Map(last => (View)new View.Text([culture.TextInfo.Listed(Value.Init.Map(value => axis.Formatted(value, culture)).Add(axis.Spoken(last, culture)))]) {
            Role = Role.Label,
            MaxLines = 1,
            Align = UnitPoint.Center,
            Conditions = marks,
        }).IfNone(() => new View.Box { Role = Role.Checkmark, Conditions = marks.TryAdd(Condition.Mixed), Align = UnitPoint.Center });

    private Points<Seq<float>> Thumbs(PlotPlane.Track plane) =>
        new(values => values.Map((value, index) => new HandlePoint(new Key.Ordinal((uint)index), plane.Along.With(new Vector2(0.5f), value))),
            (values, targets, _) => new Dragged<Seq<float>>(targets.Fold(values, (held, target) => Moved(held, target.Key, Along.Of(target.At))), targets.Map(static target => target.Key))) {
            Placed = (values, at, nearest) => values.IsEmpty
                ? Some(new Dragged<Seq<float>>(Seq(Quantized(Along.Of(at))), [new Key.Ordinal(0u)]))
                : nearest.Map(key => new Dragged<Seq<float>>(Moved(values, key, Along.Of(at)), [key])),
            Incremented = (values, key, sign) =>
                from index in Optional(key as Key.Ordinal)
                from value in values.At((int)index.Index)
                select Moved(values, key, value + (sign * Step.IfNone(() => MathF.Pow(10f, -plane.Axis.Decimals)))),
        };

    private Seq<float> Moved(Seq<float> values, Key key, float to) =>
        key is Key.Ordinal { Index: var index }
            ? values.Map((value, at) => at == (int)index
                ? float.Max(values.Take(at).Last.IfNone(Domain.Low), float.Min(values.At(at + 1).IfNone(Domain.High), Quantized(to)))
                : value)
            : values;

    private float Quantized(float shown) => float.Clamp(Step.Map(step => MathF.Round(shown / step, MidpointRounding.ToEven) * step).IfNone(shown), Domain.Low, Domain.High);
}

public sealed record Swatch(Unicolour Color, Func<SKRect, IO<Unit>> Pressed) {
    public Option<Func<Unicolour, IO<Unit>>> Dropped { get; init; }

    public View Drawn =>
        new Part.Press(Pressed).Drawn(Patch(Color)) with {
            Lift = Lift.Of(_ => new Transfer.Color(Color.ToSKColorF())),
            Drop = Dropped.Map(static dropped => Drop.Of<Transfer.Color>((color, _, _) => dropped(color.Value.ToUnicolour()))),
        };

    public static View.Box Patch(Unicolour colour) => Patch([new Material.Solid(new ColorValue.Fixed(colour))]) with { Accessible = new Accessible { Value = new AccessValue.Text(Hex(colour)) } };

    internal static View.Box Patch(Seq<Material> fills) =>
        new() { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit, Sheet = new StyleSheet(HashMap((Role.Field, new Look { Fills = fills }))) };

    internal static string Hex(Unicolour colour) => colour.MapToRgbGamut(GamutMap.OklchChromaReduction) switch { var shown => shown.Alpha.A < 1d ? shown.Hex + shown.Alpha.Hex : shown.Hex };
}

public sealed record SwatchList(Seq<Unicolour> Colors, Selection Selection, Func<Edit<Selection>, IO<Unit>> Selected) {
    public Option<Func<Edit<Seq<Unicolour>>, IO<Unit>>> Edited { get; init; }

    public View Drawn {
        get {
            Seq<Key> keys = Colors.Map(static (_, index) => (Key)new Key.Ordinal((uint)index)).Strict();
            return new Part.Choice(keys, Selection, Selected).Drawn(Colors.Map(static colour => (View)Swatch.Patch(colour)), static items => new View.Flex(PhysicalAxis.Horizontal, items) { Width = Sizing.Grow.Unit }) with {
                Lift = new Lift(moved => IO.lift(moved.Head.Bind(key => keys.Position(key)).Bind(index => Colors.At(index)).Map(static colour => (Transfer)new Transfer.Color(colour.ToSKColorF())).ToFin(new UIFault(moved.ToString(), UIReason.Missing)))),
                Drop = Edited.Map(edited => Drop.Of<Transfer.Color>((color, target, placement) => edited(Edit.Of(EditPhase.Commit, Inserted(keys, color.Value.ToUnicolour(), target, placement))))),
            };
        }
    }

    private Seq<Unicolour> Inserted(Seq<Key> keys, Unicolour colour, Option<Key> target, Placement placement) =>
        target.Bind(key => keys.Position(key)).Match(
            Some: index => placement.Map(before: (Cut: index, Resume: index), inside: (Cut: index, Resume: index + 1), after: (Cut: index + 1, Resume: index + 1)) switch {
                var (cut, resume) => Colors.Take(cut) + Seq(colour) + Colors.Skip(resume),
            },
            None: () => Colors.Add(colour));
}

public sealed record ColorPicker(Unicolour Value, Func<Edit<Unicolour>, IO<Unit>> Edited) {
    private static readonly AxisRange Turn = AxisRange.Validated(0f, 360f).ThrowIfFail();
    private static readonly ColorRamp Hues = new ColorRamp.Smooth(toSeq(Range(0, 4)).Map(static third =>
        new Stop(new ColorValue.Fixed(Colored(Turn.Span * third / 3f, 1d, 1d, 1d)), Fraction.Validated(third / 3f).ThrowIfFail()))) { Space = ColourSpace.Hsb };
    private static readonly Material Shade = Spread(new ColorValue.Fixed(new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d, 0d)), new ColorValue.Fixed(Css.Black), PhysicalAxis.Vertical);
    private static readonly StyleSheet Unspanned = new(HashMap((Role.Span, Look.Unpainted)));

    public bool Alpha { get; init; } = true;
    public Option<Unicolour> Rest { get; init; }

    public View Drawn(CultureInfo culture) {
        (double read, double saturation, double brightness) = Value.Hsb.Tuple;
        double hue = double.IsNaN(read) ? 0d : read;
        double alpha = Value.Alpha.A;
        return new View.Flex(PhysicalAxis.Vertical,
            Seq(Plane(hue, alpha), Tracked(Turn, hue, at => Colored(at, saturation, brightness, alpha), new Material.Gradient(Hues, new GradientShape.Linear(0f)), culture))
            + (Alpha
                ? Seq(Tracked(AxisRange.Unit, alpha, at => Colored(hue, saturation, brightness, at),
                    Spread(new ColorValue.Fixed(Colored(hue, saturation, brightness, 0d)), new ColorValue.Fixed(Colored(hue, saturation, brightness, 1d)), PhysicalAxis.Horizontal), culture))
                : [])
            + Seq(new TextField(Swatch.Hex(Value), edit => Edit.Typed(edit, Parsed, Edited)).Drawn)) {
            Width = Sizing.Grow.Unit,
        };
    }

    private View Plane(double hue, double alpha) =>
        new Handles<Unicolour>(Value, Points<Unicolour>.Fixed([Lens<Unicolour, Vector2>.New(static held => held.Hsb switch { var read => new Vector2((float)read.S, (float)read.B) }, at => _ => Colored(hue, at.X, at.Y, alpha))]), Edited) {
            Rest = Rest,
        }.Drawn(PlotPlane.Cartesian.Unit, [Swatch.Patch([Spread(new ColorValue.Fixed(Css.White), new ColorValue.Fixed(Colored(hue, 1d, 1d, 1d)), PhysicalAxis.Horizontal), Shade])]) with {
            Aspect = Factor.Unit,
            Width = Sizing.Grow.Unit,
        };

    private View Tracked(AxisRange domain, double value, Func<float, Unicolour> color, Material fill, CultureInfo culture) =>
        new Slider(PhysicalAxis.Horizontal, domain, Seq((float)value), edit => Edited(edit.Choose(held => held.Head.Map(color)))) {
            Background = [Swatch.Patch([fill])],
        }.Drawn(culture) with {
            Sheet = Unspanned,
        };

    private static Fin<Unicolour> Parsed(string text) =>
        Try.lift(() => Optional(Css.FromName(text)).IfNone(() => new Unicolour(text))).Run()
            .MapFail(error => new UIFault(text, UIReason.Unreadable, Inner: error));

    private static Unicolour Colored(double hue, double saturation, double brightness, double alpha) => new(ColourSpace.Hsb, hue, saturation, brightness, alpha);

    private static Material.Gradient Spread(ColorValue from, ColorValue to, PhysicalAxis along) =>
        new(new ColorRamp.Smooth([new Stop(from, Fraction.MinValue), new Stop(to, Fraction.MaxValue)]) { Space = ColourSpace.Rgb }, new GradientShape.Linear(along.Map(horizontal: 0f, vertical: 90f)));
}

public sealed record Dial(Axis.Periodic Axis, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Option<float> Rest { get; init; }

    private PlotPlane.Polar Plane => new(Axis, None);

    public View Drawn =>
        new Handles<float>(Value, Points<float>.Fixed([Lens<float, Vector2>.New(static held => new Vector2(held, 1f), at => _ => Axis.Constrained(at.X))]), Edited) { Rest = Rest }
            .Drawn(Plane, [
                new View.Mark(new Figure.Region()) { Role = Role.Track },
                new View.Mark(new Figure.Path([new Vector2(Value, 0f), new Vector2(Value, 1f)])) { Role = Role.Needle },
            ]) with {
            Aspect = Plane.Aspect,
        };
}

public sealed record Scrub(string Label, AxisRange Domain, float Value, Func<Edit<float>, IO<Unit>> Edited) {
    public Func<AxisRange, Axis> Track { get; init; } = static range => new Axis.Linear(range);
    public Option<float> Gearing { get; init; }
    public Option<float> Rest { get; init; }

    public View Handle =>
        new PlotPlane.Track(PhysicalAxis.Horizontal, Track(Domain)) switch {
            var plane => new Handles<float>(Value, Points<float>.Scalar(plane, Lens.identity<float>()), Edited) {
                Gearing = Gearing,
                Rest = Rest,
                Handle = (_, _) => new View.Pin(new Vector2(Domain.Low, 0f), new View.Box().Grown) {
                    Role = Role.Label,
                    Span = new Vector2(MathF.Abs(Domain.Span), 1f),
                    Accessible = new Accessible { Label = Label },
                },
            }.Drawn(plane, [new View.Text([Label]) { Role = Role.Label }]) with {
                Width = Sizing.Fit.Content,
            },
        };

    public View Entry(CultureInfo culture) =>
        Track(Domain) switch {
            var axis => new View.Text([axis.Formatted(Value, culture)]) {
                MaxLines = 1,
                Width = Sizing.Grow.Unit,
                Edited = Some<Func<Edit<string>, IO<Unit>>>(edit => Edit.Typed(edit, text => axis.Parsed(Domain, text, culture), Edited)),
            },
        };

    public View Drawn(CultureInfo culture) => new View.Flex(PhysicalAxis.Horizontal, [Handle, Entry(culture)]) { Width = Sizing.Grow.Unit };
}
