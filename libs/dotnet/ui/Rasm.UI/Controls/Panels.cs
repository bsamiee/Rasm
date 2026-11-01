using System.Numerics;
using System.Numerics.Tensors;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SuperLinq;
using Topten.RichTextKit;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Panel(string Title, Seq<View> Body) {
    private static readonly Seq<Condition> Signals = [Condition.Faulted, Condition.Warning, Condition.Success];

    public Option<string> Subtitle { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Seq<View> Actions { get; init; }
    public Option<TextField> Search { get; init; }
    public Seq<View> Status { get; init; }
    public Option<string> Empty { get; init; }

    public View Drawn =>
        new View.Choice(PhysicalAxis.Horizontal, [
            new View.Flex(PhysicalAxis.Vertical,
                Seq<View>(Header(Icon(Glyph), Title, Subtitle, Actions))
                + Search.Map(static entry => (entry with { Glyph = entry.Glyph | Some(Role.Magnifier) }).Drawn).ToSeq()
                + Seq(Body.IsEmpty ? Empty.Map(Placeholder).IfNone(static () => new View.Box { Height = Sizing.Grow.Unit }) : Scrolled(Body))
                + (Status.IsEmpty ? [] : Seq<View>(new View.Flex(PhysicalAxis.Horizontal, Status) {
                    Role = Role.StatusBar,
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.Status, Live = Live.Polite },
                }))).Grown,
            new View.Flex(PhysicalAxis.Vertical, [Icon(Glyph).Head.IfNone(() => new View.Text([Title]) { MaxLines = 1 })]) {
                Height = Sizing.Grow.Unit,
            },
        ]) {
            Role = Role.Panel,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
        }.Grown;

    internal static View.Flex Header(Seq<View> leading, string title, Option<string> subtitle, Seq<View> trailing) =>
        new(PhysicalAxis.Horizontal,
            leading
            + Seq<View>(new View.Flex(PhysicalAxis.Vertical,
                Seq<View>(new View.Text([title]) { MaxLines = 1, Accessible = new Accessible { Role = AccessRole.Heading } })
                + subtitle.Map(static text => (View)new View.Text([text]) { Role = Role.Caption, MaxLines = 1 }).ToSeq()) {
                Width = Sizing.Grow.Unit,
                Align = UnitPoint.Center,
            })
            + trailing) {
            Role = Role.Header,
            Width = Sizing.Grow.Unit,
        };

    internal static Seq<View> Icon(Option<GlyphSource> glyph) => glyph.Map(static art => (View)new View.Glyph(art) { Align = UnitPoint.Center }).ToSeq();

    internal static Option<View> StatusGlyph(HashSet<Condition> conditions) =>
        conditions.Contains(Condition.Busy)
            ? Some(Progress.Ring(None) with { Align = UnitPoint.Center })
            : Signals.Find(conditions.Contains).Map(static signal => (View)new View.Box { Role = Role.Status, Conditions = [signal], Align = UnitPoint.Center });

    internal static View Placeholder(string message) =>
        new View.Text([message]) { Role = Role.Caption, Alignment = TextAlignment.Center, Width = Sizing.Grow.Unit, Align = UnitPoint.Center };

    internal static View Scrolled(Seq<View> content) =>
        content switch {
            [{ Height.Case: Sizing { Stretches: true } } only] => only,
            _ => new View.Scroll(PhysicalAxis.Vertical, new View.Flex(PhysicalAxis.Vertical, content) { Width = Sizing.Grow.Unit }).Grown,
        };
}

public sealed record Section(string Title, Seq<View> Content) {
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Summary { get; init; }
    public Seq<View> Actions { get; init; }
    public Option<string> Empty { get; init; }
    public bool Expanded { get; init; } = true;
    public Option<Func<Edit<bool>, IO<Unit>>> Edited { get; init; }

    public View Drawn =>
        new Disclosure(
            Panel.Header(Panel.Icon(Glyph), Title, None, Summary.Filter(_ => !Expanded).Map(static summary => (View)new View.Text([summary]) { Role = Role.Caption, MaxLines = 1, Align = UnitPoint.Center }).ToSeq() + Actions),
            new View.Flex(PhysicalAxis.Vertical, Content.IsEmpty ? Empty.Map(Panel.Placeholder).ToSeq() : Content) { Width = Sizing.Grow.Unit },
            Expanded) {
            Edited = Edited,
        }.Drawn with {
            Role = Role.Section,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
        };
}

public sealed record Card(string Title) {
    public Option<string> Subtitle { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<View> Media { get; init; }
    public Seq<View> Content { get; init; }
    public Seq<DialogAction> Actions { get; init; }
    public HashSet<Condition> Conditions { get; init; }

    public View Drawn =>
        new View.Flex(PhysicalAxis.Vertical,
            Media.ToSeq()
            + Seq<View>(Panel.Header(Panel.Icon(Glyph), Title, Subtitle, Panel.StatusGlyph(Conditions).ToSeq()) with { Role = None })
            + Content
            + DialogAction.Footer(Actions, unit)) {
            Role = Role.Card,
            Width = Sizing.Grow.Unit,
            Conditions = Conditions,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
        };
}

public sealed record Disclosure(View Header, View Content, bool Expanded) {
    public Option<Func<Edit<bool>, IO<Unit>>> Edited { get; init; }
    public Role Role { get; init; } = Role.Header;

    public View Drawn =>
        new View.Flex(PhysicalAxis.Vertical,
            Seq(Edited.Match(
                    Some: edited => new Part.Press(_ => edited(Edit.Of(EditPhase.Commit, !Expanded))).Drawn(new View.Flex(PhysicalAxis.Horizontal, [Face(Expanded), Header]) {
                        Accessible = new Accessible { Role = AccessRole.Disclosure, Expanded = Expanded },
                    }),
                    None: () => Header) with {
                Role = Role,
                Width = Sizing.Grow.Unit,
                Sticks = PhysicalAxis.Vertical,
            })
            + (Expanded ? Seq(Content) : [])) {
            Width = Sizing.Grow.Unit,
        };

    internal static View Chevron(Option<bool> open, Option<IO<Unit>> toggle) =>
        open.Match(
            Some: shown => (Face(shown) with { Accessible = new Accessible { Role = AccessRole.Disclosure, Expanded = shown } }) switch {
                var chevron => toggle.Map(run => new Part.Press(_ => run).Drawn(chevron)).IfNone(chevron),
            },
            None: static () => Face(open: false) with { Opacity = Fraction.MinValue });

    private static View.Box Face(bool open) => new() { Role = Role.Chevron, Conditions = open ? [Condition.Selected] : [], Align = UnitPoint.Center };
}

public sealed record Split(PhysicalAxis Along, Seq<(View Content, Factor Weight)> Panes, Func<Edit<Seq<Factor>>, IO<Unit>> Edited) {
    public Fin<View> Drawn {
        get {
            float total = TensorPrimitives.Sum<float>([.. Panes.Map(static pane => (float)pane.Weight)]);
            return Panes.Traverse(pane => Fraction.Validated((float)pane.Weight / total).Map(share => Along.Map(
                horizontal: pane.Content with { Width = new Sizing.Percent(share) },
                vertical: pane.Content with { Height = new Sizing.Percent(share) }))).As().Map(shared => {
                    View panes = new View.Flex(Along, shared.Intersperse(View.Divider(Along)).Strict()).Grown;
                    return AxisRange.Validated(Along.Map(horizontal: 0f, vertical: total), Along.Map(horizontal: total, vertical: 0f)).ToOption().Filter(_ => Panes.Count > 1).Match(
                        Some: domain => Divided(new Handles<Seq<Factor>>(Panes.Map(static pane => pane.Weight), Weighted(total), Edited), new PlotPlane.Track(Along, new Axis.Linear(domain)), panes),
                        None: () => panes);
                });
        }
    }

    internal static View Divided<TValue>(Handles<TValue> handles, PlotPlane.Track plane, View content) where TValue : notnull =>
        (handles with {
            Points = handles.Points with { Placed = static (_, _, _) => None },
            BackgroundCursor = CursorShape.Default,
            Handle = (_, at) => new View.Pin(plane.Along.Cross.With(at, 0f), new View.Box()) { Span = plane.Along.Cross.With(Vector2.Zero, 1f) },
        }).Drawn(plane, [content]);

    private Points<Seq<Factor>> Weighted(float total) =>
        Points<Seq<Factor>>.Listed(
            static weights => weights.Map(static weight => (float)weight).Scan(0f, static (sum, weight) => sum + weight).Tail.Init,
            edge => Along.With(new Vector2(0.5f), edge),
            (_, to) => Some(Along.Of(to)),
            (_, edges) => toSeq((Seq(0f) + edges).Add(total).AsIterable().Window(2).Select(static pair => Factor.Validated(pair[1] - pair[0]))).Traverse(static weight => weight).As().ToOption(),
            static (_, _) => None) with {
            Removed = static (_, _) => None,
        };
}

public sealed record Board<TItem>(Seq<TItem> Items, Func<TItem, Key> Keyed, Func<TItem, View> Face, int Columns) where TItem : notnull {
    public Func<TItem, int> Span { get; init; } = static _ => 1;
    public Func<TItem, int> Rows { get; init; } = static _ => 1;
    public Factor Cell { get; init; } = Factor.Unit;
    public Option<Func<Seq<Key>, Part.Choice>> Choice { get; init; }

    public Fin<View> Drawn =>
        toSeq(SuperEnumerable.Generate(Columns, static count => count / 2).TakeWhile(static count => count > 0))
            .Traverse(Arranged).As()
            .Map(static alternatives => (View)new View.Choice(PhysicalAxis.Horizontal, alternatives) { Width = Sizing.Grow.Unit });

    private Fin<View> Arranged(int count) =>
        Items.Traverse(item => Placed(item, count)).As().Map(tiles => Choice.Match(
            Some: choice => choice(Items.Map(Keyed)).Drawn(tiles, members => Gridded(members, count)),
            None: () => Gridded(tiles, count)));

    private Fin<View> Placed(TItem item, int count) {
        (int columns, int rows) = (int.Clamp(Span(item), 1, count), int.Max(1, Rows(item)));
        return Factor.Validated(columns * (float)Cell / rows).Map(aspect => Face(item) with { Key = Keyed(item), Spanning = new CellSpan(columns, rows), Aspect = aspect });
    }

    private static View.Grid Gridded(Seq<View> tiles, int count) =>
        new(new Tracks(LanguageExt.Seq.repeat<Sizing>(Sizing.Grow.Unit, count)), [tiles]) {
            Role = Role.Stack,
            Width = Sizing.Grow.Unit,
        };
}
