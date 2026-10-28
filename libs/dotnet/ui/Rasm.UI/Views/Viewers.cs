using System.Globalization;
using System.Numerics;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Preset<T>(Key Key, string Label, T Value) where T : notnull;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Tool {
    private Tool(bool selects = false) => Selects = selects;

    internal bool Selects { get; }

    public sealed record Select() : Tool(selects: true);

    public sealed record Pan() : Tool;

    public sealed record Measure(Seq<Vector2> Points, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) : Tool {
        public bool Closed { get; init; }
    }

    public sealed record Region(Brush Brush) : Tool;

    public sealed record Image(ImageTool Operation) : Tool;
}

public sealed record Series<TEntity>(Key Key, string Label, Content<Seq<TEntity>> Entities, Func<TEntity, Node> Face) where TEntity : notnull {
    public bool Visible { get; init; } = true;
    public Fraction Opacity { get; init; } = Fraction.MaxValue;
    public Option<Figure> Legend { get; init; }
    public Option<ColorValue> Color { get; init; }
}

public sealed record Viewer<TEntity>(PlotPlane View, Part.Viewport Navigation, Seq<Series<TEntity>> Series, Func<TEntity, Key> Keyed) where TEntity : notnull {
    // --- [STATE]
    public Tool Tool { get; init; } = new Tool.Select();
    public Selection Selection { get; init; } = new Selection.Many([]);
    public Func<Edit<Selection>, IO<Unit>> Selected { get; init; } = static _ => IO.pure(unit);
    public Func<Option<Key>, Option<IO<Unit>>> Hover { get; init; } = static _ => None;
    public HashSet<Key> Highlighted { get; init; }
    public Option<Func<TEntity, Node>> Tip { get; init; }
    public Func<Seq<Key>, SKRect, Option<IO<Unit>>> Menu { get; init; } = static (_, _) => None;
    public HashSet<Key> Hidden { get; init; }
    public HashSet<Key> Isolated { get; init; }
    public bool Ghosted { get; init; }
    public Option<ColorAxis> Color { get; init; }
    public Option<Func<TEntity, double>> Value { get; init; }
    public Option<Func<Edit<Selection>, IO<Unit>>> Toggled { get; init; }
    public Seq<Preset<PlotPlane>> Saved { get; init; }
    public Option<Vector2> Pointer { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }
    public Option<Func<Option<Nearest>, IO<Unit>>> Pointed { get; init; }
    public Seq<Node> Controls { get; init; }
    public bool Minimap { get; init; }
    public bool Ruler { get; init; }
    public Func<Vector2, Vector2, float> Distance { get; init; } = Vector2.Distance;
    public Option<string> DistanceUnit { get; init; }
    public Role Role { get; init; } = Role.Well;

    // --- [DRAW]
    public Node Drawn(CultureInfo culture) {
        Seq<(Series<TEntity> Series, uint Ordinal)> shown = Ordered.Filter(static held => held.Series.Visible).Strict();
        Option<Axis> ruled = Ruled;
        Node plot = new Node.Plot(View, Seq(Entities(View, shown, Tool.Selects)) + Tooled(ruled, culture).ToSeq()) {
            Role = Role,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Sheet = Scaled,
            Hovered = Hovered,
            Pointed = Pointed,
            Crosshair = Tip.Map(tip => Tipped(tip, shown)),
            Part = Navigation,
            Accessible = new Accessible { Role = AccessRole.Image, Label = string.Join($"{culture.TextInfo.ListSeparator} ", shown.Map(static held => held.Series.Label)) },
        };
        return new Node.Overlay(plot, [
            Corners(Cluster(PhysicalAxis.Vertical, Controls + Views(culture).ToSeq() + Status), Some(Viewers.Zooms(Navigation, View))),
            new Node.Flex(PhysicalAxis.Vertical, Seq<Node>(Corners(Cluster(PhysicalAxis.Vertical, Readout(culture).ToSeq()), Cluster(PhysicalAxis.Vertical, Legended.ToSeq() + Overview(shown, culture).ToSeq())))
                + ruled.Filter(_ => Ruler).Map(Scale).ToSeq()) {
                Width = Sizing.Grow.Unit,
                Align = UnitPoint.Bottom,
            },
        ]) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private Node Entities(PlotPlane plane, Seq<(Series<TEntity> Series, uint Ordinal)> shown, bool choosing) {
        Seq<(Series<TEntity> Series, HashSet<Key> Keys, Seq<(Key Key, Node Node)> Members)> drawn = shown.Rev()
            .Map(held => Members(held.Series, held.Ordinal) switch { var members => (held.Series, toHashSet(members.Map(static member => member.Key)), members) }).Strict();
        Seq<(Key Key, Node Node)> placed = drawn.Bind(static held => held.Members).Strict();
        HashMap<Key, Node> keyed = toHashMap(placed);
        Func<Seq<Node>, Node> stacked = members => new Node.Plot(plane, drawn.Map(held => (Node)new Node.Plot(plane, members.Filter(member => member.Key.Exists(held.Keys.Contains))) {
            Opacity = held.Series.Opacity,
            Conditions = held.Series.Entities.Conditions,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.Group, Label = held.Series.Label },
        })) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
        return choosing
            ? new Part.Choice(placed.Map(static member => member.Key), Selection, Selected) { Hover = Hover, Highlighted = Highlighted, Menu = Menu }.Drawn(key => keyed[key], stacked)
            : stacked(placed.Map(member => Highlighted.Contains(member.Key) ? member.Node.Conditioned(Condition.Highlighted) : member.Node));
    }

    private Seq<(Key Key, Node Node)> Members(Series<TEntity> series, uint ordinal) => series.Entities.Current.ToSeq().Flatten().Choose(entity => Member(series, ordinal, entity));

    private Option<(Key Key, Node Node)> Member(Series<TEntity> series, uint ordinal, TEntity entity) {
        Key key = Keyed(entity);
        Option<double> value = Value.Map(read => read(entity));
        Node face = series.Face(entity).SwitchPartially<DataSeries, Node>(
            new DataSeries(ordinal) { Color = series.Color, Value = value.Map(held => Color.Map(axis => (double)axis.Axis.Fraction((float)held)).IfNone(held)) },
            @default: static (_, node) => node,
            mark: static (data, mark) => mark with { Series = data });
        Node member = face with { Key = key, Role = face.Role | Some(Role.Series) };
        bool present = !Hidden.Contains(key) && (Isolated.IsEmpty || Isolated.Contains(key)) && Color.ForAll(axis => value.ForAll(axis.Holds));
        return present ? Some((key, member)) : Ghosted && !Hidden.Contains(key) ? Some((key, member.Conditioned(Condition.Disabled))) : None;
    }

    private Func<Nearest, Node> Tipped(Func<TEntity, Node> tip, Seq<(Series<TEntity> Series, uint Ordinal)> shown) {
        HashMap<Key, TEntity> entities = toHashMap(shown.Bind(static held => held.Series.Entities.Current.ToSeq().Flatten()).Map(entity => (Keyed(entity), entity)));
        return nearest => new Node.Plot(View, toSeq<(Key Key, Datum Datum)>(nearest.Data).Filter(held => Highlighted.Contains(held.Key)).Choose(held => entities.Find(held.Key)
            .Map(entity => (Node)new Node.Pin(held.Datum.Value, new Node.Flex(PhysicalAxis.Vertical, [tip(entity)]) { Role = Role.Tooltip }) { Align = UnitPoint.Bottom }))) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private Option<Node> Tooled(Option<Axis> ruled, CultureInfo culture) =>
        Tool.Switch(
            (Viewer: this, Ruled: ruled, Culture: culture),
            select: static (_, _) => Option<Node>.None,
            pan: static (_, _) => Option<Node>.None,
            measure: static (held, measure) => Some(new Handles<Seq<Vector2>>(measure.Points, Points<Seq<Vector2>>.Indexed(Lens.identity<Seq<Vector2>>()), measure.Edited)
                .Drawn(held.Viewer.View, held.Viewer.Measured(measure, held.Ruled, held.Culture))),
            region: static (held, region) => Some(region.Brush.Drawn(held.Viewer.View)),
            image: static (held, image) => Some(image.Operation.Drawn(held.Viewer.View, held.Viewer.Extent)));

    private Seq<Node> Measured(Tool.Measure measure, Option<Axis> ruled, CultureInfo culture) =>
        Seq<Node>(new Node.Mark(new Figure.Path(measure.Points) { Closed = measure.Closed }) { Role = Role.Crosshair })
        + (from axis in ruled.ToSeq()
           from pair in toSeq((measure.Closed ? measure.Points + measure.Points.Take(1) : measure.Points).AsIterable().Window(2))
           select (Node)new Node.Mark(new Figure.Label(new Figure.Path([pair[0], pair[1]]), [axis.Spoken(Distance(pair[0], pair[1]), culture)])) { Role = Role.Label });

    // --- [HUD]
    private Option<Node> Views(CultureInfo culture) {
        HashMap<Key, Preset<PlotPlane>> saved = toHashMap(Saved.Map(static preset => (preset.Key, preset)));
        return Saved.IsEmpty
            ? None
            : Some(new Picker(
                Saved.Map(static preset => preset.Key),
                Saved.Find(preset => preset.Value == View).Map(static preset => preset.Key),
                key => saved[key].Label,
                edit => Navigation.Edited(new Edit<PlotPlane>(edit.Phase, edit.Value.Bind(saved.Find).Map(static preset => preset.Value)))).Drawn(culture));
    }

    private Seq<Node> Status =>
        Series.Choose(static series => series.Entities.Switch(
            series,
            pending: static (held, pending) => Some<Node>(Reported(Progress.Ring(pending.Work), held.Label)),
            ready: static (_, _) => Option<Node>.None,
            empty: static (_, empty) => Some<Node>(Reported(new Node.Box { Role = Role.Status }, empty.Reason)),
            failed: static (held, _) => Some<Node>(Reported(new Node.Box { Role = Role.Status, Conditions = [Condition.Faulted] }, held.Entities.Message.IfNone(held.Label)))));

    private Option<Node> Readout(CultureInfo culture) => Pointer.Map(at => (Node)new Node.Text([View.Spoken(at, culture)]) { Role = Role.Readout });

    private Option<Node> Legended {
        get {
            Seq<Plotted> keyed = Series.Choose(static series => series.Legend.Map(figure => new Plotted(series.Key, series.Label, figure) { Color = series.Color }));
            return Color.Map(static axis => new Legend.Ramp(axis).Drawn)
                | (keyed.IsEmpty
                    ? None
                    : Some(new Legend.Swatches(keyed) { Shown = new Selection.Many(toHashSet(Series.Filter(static series => series.Visible).Map(static series => series.Key))), Toggled = Toggled }.Drawn));
        }
    }

    private Option<Node> Overview(Seq<(Series<TEntity> Series, uint Ordinal)> shown, CultureInfo culture) =>
        Minimap
            ? Some<Node>(new Node.Plot(Navigation.Rest, [Entities(Navigation.Rest, shown, choosing: false), new Brush(View, Navigation.Edited).Drawn(Navigation.Rest)]) {
                Role = Role.Well,
                Sheet = Scaled,
                Aspect = Navigation.Rest.Aspect | Factor.Validated(Extent.X / Extent.Y).ToOption(),
                Accessible = new Accessible { Role = AccessRole.Image, Label = Spoken(View, culture) },
            })
            : None;

    private static Node Scale(Axis axis) =>
        new PlotPlane.Track(PhysicalAxis.Horizontal, axis) switch {
            var plane => plane.Stretched(new Node.Plot(plane, [new Node.Mark(new Figure.Region()) { Role = Role.Track }]) { Role = Role.Hud, Guides = [new Guide(PhysicalAxis.Horizontal)] }),
        };

    private static Option<Node> Cluster(PhysicalAxis axis, Seq<Node> members) => members.IsEmpty ? None : Some<Node>(new Node.Flex(axis, members) { Role = Role.Hud });

    private static Node.Flex Corners(Option<Node> start, Option<Node> end) =>
        new(PhysicalAxis.Horizontal, start.ToSeq() + Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit }) + end.ToSeq()) { Width = Sizing.Grow.Unit };

    private static Node.Flex Reported(Node mark, string text) =>
        new(PhysicalAxis.Horizontal, [mark, new Node.Text([text]) { Role = Role.Label, MaxLines = 1 }]) { Accessible = new Accessible { Role = AccessRole.Status, Live = Live.Polite } };

    // --- [READ]
    private Seq<(Series<TEntity> Series, uint Ordinal)> Ordered {
        get {
            Seq<Key> legended = Series.Filter(static series => series.Legend.IsSome).Map(static series => series.Key).Strict();
            return Series.Map((series, index) => (Series: series, Ordinal: (uint)Selection.Position(legended, series.Key).IfNone(legended.Count + index))).Strict();
        }
    }

    private Option<Axis> Ruled =>
        from across in View.Dimension(PhysicalAxis.Horizontal)
        from range in AxisRange.Validated(0f, Distance(View.Value(new SKPoint((float)Fraction.MinValue, (float)Fraction.Half)), View.Value(new SKPoint((float)Fraction.MaxValue, (float)Fraction.Half)))).ToOption()
        select (Axis)new Axis.Linear(range) { Unit = DistanceUnit | across.Unit, Places = across.Places };

    private StyleSheet Scaled => Color.Map(static axis => new StyleSheet(HashMap((Role.Series, new Look { Scale = axis.Ramp })))).IfNone(StyleSheet.Empty);

    private Vector2 Extent => Navigation.Rest.Dimensions.Fold(Vector2.One, static (extent, held) => held.Along.With(extent, MathF.Abs(held.Axis.Domain.Span)));

    private static string Spoken(PlotPlane plane, CultureInfo culture) =>
        string.Join($"{culture.TextInfo.ListSeparator} ", Seq<Func<AxisRange, float>>(static range => range.From, static range => range.To)
            .Map(edge => plane.Spoken(plane.Dimensions.Fold(Vector2.Zero, (at, held) => held.Along.With(at, edge(held.Axis.Domain))), culture)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Viewers {
    internal static Node Zooms(Part.Viewport navigation, PlotPlane view) =>
        new Node.Flex(PhysicalAxis.Horizontal, [
            Zoom(Role.Add, Some(navigation.Stepped(view, -1))),
            Zoom(Role.Subtract, Some(navigation.Stepped(view, 1))),
            Zoom(Role.Revert, view == navigation.Rest ? None : Some<Part>(new Part.Press(_ => navigation.Edited(Edit.Of(EditPhase.Commit, navigation.Rest))))),
        ]) {
            Role = Role.Hud,
        };

    private static Node.Flex Zoom(Role glyph, Option<Part> part) =>
        new(PhysicalAxis.Horizontal, [new Node.Box { Role = glyph }]) {
            Role = Role.Button,
            Conditions = part.IsSome ? [Condition.Borderless] : [Condition.Borderless, Condition.Disabled],
            Part = part,
        };
}
