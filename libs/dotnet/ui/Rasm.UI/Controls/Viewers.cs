using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Tool {
    public sealed record Select : Tool;

    public sealed record Pan : Tool;

    public sealed record Measure(Seq<Vector2> Points, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) : Tool {
        public bool Closed { get; init; }
    }

    public sealed record Region(Brush Brush) : Tool;

    public sealed record Image(ImageTool Operation) : Tool;
}

public sealed record Series<TEntity>(Key Key, string Label, Content<Seq<TEntity>> Entities, Func<TEntity, View> Face) where TEntity : notnull {
    public bool Visible { get; init; } = true;
    public Fraction Opacity { get; init; } = Fraction.MaxValue;
    public Option<Figure> Legend { get; init; }
    public Option<ColorValue> Color { get; init; }
}

public sealed record Viewer<TEntity>(PlotPlane View, Part.Viewport Navigation, Seq<Series<TEntity>> Series, Func<TEntity, Key> Keyed, Func<Seq<Key>, Part.Choice> Choice)
    where TEntity : notnull {
    // --- [STATE]
    public Tool Tool { get; init; } = new Tool.Select();
    public Option<Func<TEntity, View>> Tip { get; init; }
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
    public Seq<View> Controls { get; init; }
    public bool Minimap { get; init; }
    public bool Ruler { get; init; }
    public Func<Vector2, Vector2, float> Distance { get; init; } = Vector2.Distance;
    public Option<string> DistanceUnit { get; init; }
    public Key Role { get; init; } = Roles.Well;

    // --- [DRAW]
    public View Drawn(CultureInfo culture) {
        Seq<(Series<TEntity> Series, uint Ordinal)> shown = Ordered.Filter(static held => held.Series.Visible).Strict();
        Seq<(Series<TEntity> Series, Seq<(Key Key, View View)> Members)> layers = shown.Rev().Map(held => (held.Series, Members(held.Series, held.Ordinal))).Strict();
        Part.Choice chosen = Choice(layers.Bind(static layer => layer.Members).Map(static member => member.Key));
        Func<View, Key, View> highlighted = (face, key) => chosen.Highlighted.Contains(key) ? face.Conditioned(Condition.Highlighted) : face;
        View entities = Tool is Tool.Select ? chosen.Drawn(member => Entities(View, layers, member)) : Entities(View, layers, highlighted);
        Option<Axis> ruled = Ruled;
        View plot = new View.Plot(View, Seq(entities) + Tooled(ruled, culture).ToSeq()) {
            Role = Role,
            Sheet = ColorAxis.Sheet(Color),
            Hovered = Hovered,
            Pointed = Pointed,
            Crosshair = Tip.Map(tip => Tipped(tip, shown, chosen.Highlighted)),
            Part = Navigation,
            Accessible = new Accessible { Role = AccessRole.Image, Label = culture.TextInfo.Listed(shown.Map(static held => held.Series.Label)) },
        }.Grown;
        return new View.Overlay(plot, [
            Corners(Cluster(PhysicalAxis.Vertical, Controls + Views(culture).ToSeq() + Loading), Some(Navigation.Zooms(View))),
            new View.Flex(PhysicalAxis.Vertical, Seq<View>(Corners(Cluster(PhysicalAxis.Vertical, Pointer.Map(at => (View)new View.Text([View.Spoken(at, culture)]) { Role = Roles.Readout }).ToSeq()), Cluster(PhysicalAxis.Vertical, Legended.ToSeq() + Overview(layers, highlighted, culture).ToSeq())))
                + ruled.Filter(_ => Ruler).Map(Scale).ToSeq()) {
                Width = Sizing.Grow.Unit,
                Align = UnitPoint.Bottom,
            },
        ]).Grown;
    }

    private static View Entities(PlotPlane plane, Seq<(Series<TEntity> Series, Seq<(Key Key, View View)> Members)> layers, Func<View, Key, View> drawn) =>
        new View.Plot(plane, layers.Map(layer => new View.Plot(plane, layer.Members.Map(member => drawn(member.View, member.Key))) {
            Opacity = layer.Series.Opacity,
            Conditions = layer.Series.Entities.Conditions,
            Accessible = new Accessible { Role = AccessRole.Group, Label = layer.Series.Label },
        }.Grown)).Grown;

    private Seq<(Key Key, View View)> Members(Series<TEntity> series, uint ordinal) => series.Entities.Current.ToSeq().Flatten().Choose(entity => Member(series, ordinal, entity));

    private Option<(Key Key, View View)> Member(Series<TEntity> series, uint ordinal, TEntity entity) {
        Key key = Keyed(entity);
        Option<double> value = Value.Map(read => read(entity));
        View face = series.Face(entity).SwitchPartially(
            ColorAxis.Series(Color, ordinal, series.Color, value),
            @default: static (_, node) => node,
            mark: static (data, mark) => mark with { Series = data });
        View member = face with { Key = key, Role = face.Role | Some(Roles.Series) };
        bool present = !Hidden.Contains(key) && (Isolated.IsEmpty || Isolated.Contains(key)) && ColorAxis.Shows(Color, value);
        return present ? Some((key, member)) : Ghosted && !Hidden.Contains(key) ? Some((key, member.Conditioned(Condition.Disabled))) : None;
    }

    private Func<Nearest, View> Tipped(Func<TEntity, View> tip, Seq<(Series<TEntity> Series, uint Ordinal)> shown, HashSet<Key> highlighted) {
        HashMap<Key, TEntity> entities = toHashMap(shown.Bind(static held => held.Series.Entities.Current.ToSeq().Flatten()).Map(entity => (Keyed(entity), entity)));
        return nearest => new View.Plot(View, toSeq<(Key Key, Datum Datum)>(nearest.Data).Filter(held => highlighted.Contains(held.Key)).Choose(held => entities.Find(held.Key)
            .Map(entity => (View)new View.Pin(held.Datum.Value, new View.Flex(PhysicalAxis.Vertical, [tip(entity)]) { Role = Roles.Tooltip }) { Align = UnitPoint.Bottom }))).Grown;
    }

    private Option<View> Tooled(Option<Axis> ruled, CultureInfo culture) =>
        Tool.Switch(
            (Viewer: this, Ruled: ruled, Culture: culture),
            select: static (_, _) => Option<View>.None,
            pan: static (_, _) => Option<View>.None,
            measure: static (held, measure) => Some(new Handles<Seq<Vector2>>(measure.Points, Points<Seq<Vector2>>.Indexed(Lens.identity<Seq<Vector2>>()), measure.Edited)
                .Drawn(held.Viewer.View, held.Viewer.Measured(measure, held.Ruled, held.Culture))),
            region: static (held, region) => Some(region.Brush.Drawn(held.Viewer.View)),
            image: static (held, image) => Some(image.Operation.Drawn(held.Viewer.View, held.Viewer.Extent)));

    private Seq<View> Measured(Tool.Measure measure, Option<Axis> ruled, CultureInfo culture) =>
        Seq<View>(new View.Mark(new Figure.Path(measure.Points) { Closed = measure.Closed }) { Role = Roles.Crosshair })
        + (from axis in ruled.ToSeq()
           from pair in toSeq((measure.Closed ? measure.Points + measure.Points.Head.ToSeq() : measure.Points).AsIterable().Window(2))
           select (View)new View.Mark(new Figure.Label(new Figure.Path([pair[0], pair[1]]), [axis.Spoken(Distance(pair[0], pair[1]), culture)])) { Role = Roles.Label });

    // --- [HUD]
    private Option<View> Views(CultureInfo culture) =>
        Saved.IsEmpty
            ? None
            : Some(new Picker(
                Saved.Map(static preset => preset.Key),
                Saved.Chosen(View).Map(static preset => preset.Key),
                key => Saved.Find(preset => preset.Key == key).Map(static preset => preset.Label).IfNone(key.Identifier),
                edit => Navigation.Edited(Saved.Applied(edit))).Drawn(culture));

    private Seq<View> Loading =>
        Series.Choose(static series => series.Entities.Switch(
            pending: static pending => Some(Progress.Ring(pending.Work)),
            ready: static _ => Option<View>.None,
            empty: static _ => Option<View>.None,
            failed: static _ => Option<View>.None));

    private Option<View> Legended {
        get {
            Seq<Plotted> keyed = Series.Choose(static series => series.Legend.Map(figure => new Plotted(series.Key, series.Label, figure) { Color = series.Color }));
            return Legend.Of(Color, keyed.IsEmpty
                    ? None
                    : Some(new Legend.Swatches(keyed) { Shown = new Selection.Many(toHashSet(Series.Filter(static series => series.Visible).Map(static series => series.Key))), Toggled = Toggled }))
                .Map(static legend => legend.Drawn);
        }
    }

    private Option<View> Overview(Seq<(Series<TEntity> Series, Seq<(Key Key, View View)> Members)> layers, Func<View, Key, View> drawn, CultureInfo culture) =>
        Minimap
            ? Some<View>(Navigation.Overview(View, [Entities(Navigation.Rest, layers, drawn)]) with {
                Sheet = ColorAxis.Sheet(Color),
                Accessible = new Accessible { Role = AccessRole.Image, Label = Spoken(View, culture) },
            })
            : None;

    private static View Scale(Axis axis) =>
        new View.Plot(new PlotPlane.Track(PhysicalAxis.Horizontal, axis), [new View.Mark(new Figure.Region()) { Role = Roles.Track }]) { Role = Roles.Hud, Guides = [new Guide(PhysicalAxis.Horizontal)] }.Stretched(PhysicalAxis.Horizontal);

    private static Option<View> Cluster(PhysicalAxis axis, Seq<View> members) => members.IsEmpty ? None : Some<View>(new View.Flex(axis, members) { Role = Roles.Hud });

    private static View.Flex Corners(Option<View> start, Option<View> end) =>
        new(PhysicalAxis.Horizontal, start.ToSeq() + Seq<View>(new View.Box { Width = Sizing.Grow.Unit }) + end.ToSeq()) { Width = Sizing.Grow.Unit };

    // --- [READ]
    private Seq<(Series<TEntity> Series, uint Ordinal)> Ordered {
        get {
            Seq<Key> legended = Series.Filter(static series => series.Legend.IsSome).Map(static series => series.Key).Strict();
            HashMap<Key, uint> ordinals = toHashMap(legended.Map(static (key, index) => (key, (uint)index)));
            return Series.Map((series, index) => (Series: series, Ordinal: ordinals.Find(series.Key).IfNone((uint)(legended.Count + index)))).Strict();
        }
    }

    private Option<Axis> Ruled =>
        from across in View.Dimension(PhysicalAxis.Horizontal)
        from range in AxisRange.Validated(0f, Distance(View.Value(new SKPoint(0f, 0.5f)), View.Value(new SKPoint(1f, 0.5f)))).ToOption()
        select (Axis)new Axis.Linear(range) { Unit = DistanceUnit | across.Unit, Places = across.Places };

    private Vector2 Extent => Navigation.Rest.Dimensions.Fold(Vector2.One, static (extent, held) => held.Along.With(extent, MathF.Abs(held.Axis.Domain.Span)));

    private static string Spoken(PlotPlane plane, CultureInfo culture) =>
        culture.TextInfo.Listed(Seq<Func<AxisRange, float>>(static range => range.From, static range => range.To)
            .Map(edge => plane.Spoken(plane.Dimensions.Fold(Vector2.Zero, (at, held) => held.Along.With(at, edge(held.Axis.Domain))), culture)));
}
