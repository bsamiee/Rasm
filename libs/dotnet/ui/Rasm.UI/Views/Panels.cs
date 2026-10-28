using Rasm.UI.Components;
using Rasm.UI.Styling;
using Topten.RichTextKit;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Panel(string Title, Seq<Node> Body) {
    public Option<string> Subtitle { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Seq<Node> Actions { get; init; }
    public Option<TextField> Search { get; init; }
    public Seq<Node> Status { get; init; }
    public Option<string> Empty { get; init; }

    public Node Drawn =>
        new Node.Choice(PhysicalAxis.Horizontal, [
            new Node.Flex(PhysicalAxis.Vertical,
                Seq<Node>(Panels.Header(Panels.Pictured(Glyph), Title, Subtitle, Actions) with { Role = Role.Header })
                + Search.Map(static entry => (entry with { Glyph = entry.Glyph | Some(Role.Magnifier) }).Drawn).ToSeq()
                + Seq(Body.IsEmpty ? Empty.Map(Panels.Said).IfNone(static () => new Node.Box { Height = Sizing.Grow.Unit }) : Panels.Body(Body))
                + (Status.IsEmpty ? [] : Seq<Node>(new Node.Flex(PhysicalAxis.Horizontal, Status) {
                    Role = Role.StatusBar,
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.Status, Live = Live.Polite },
                }))) {
                Key = new Key.Ordinal(0),
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            },
            new Node.Flex(PhysicalAxis.Vertical, [Panels.Pictured(Glyph).Head.IfNone(() => new Node.Text([Title]) { MaxLines = 1 })]) {
                Key = new Key.Ordinal(1),
                Height = Sizing.Grow.Unit,
            },
        ]) {
            Role = Role.Panel,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
        };
}

public sealed record Section(string Title, Seq<Node> Content) {
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Summary { get; init; }
    public Seq<Node> Actions { get; init; }
    public Option<string> Empty { get; init; }
    public bool Expanded { get; init; } = true;
    public Option<Func<Edit<bool>, IO<Unit>>> Edited { get; init; }

    public Node Drawn {
        get {
            Node.Flex header = Panels.Header(Panels.Pictured(Glyph), Title, None,
                Summary.Filter(_ => !Expanded).Map(static summary => (Node)new Node.Text([summary]) { Role = Role.Caption, MaxLines = 1, Align = UnitPoint.Center }).ToSeq() + Actions);
            Node.Flex body = new(PhysicalAxis.Vertical, Content.IsEmpty ? Empty.Map(Panels.Said).ToSeq() : Content) { Width = Sizing.Grow.Unit };
            return Edited.Match(
                Some: edited => new Disclosure(header, body, Expanded, edited).Drawn,
                None: () => new Node.Flex(PhysicalAxis.Vertical, Seq<Node>(header with { Role = Role.Header, Sticks = PhysicalAxis.Vertical }) + (Expanded ? Seq<Node>(body) : [])) {
                    Width = Sizing.Grow.Unit,
                }) with {
                Role = Role.Section,
                Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
            };
        }
    }
}

public sealed record Tabs(Seq<Key> Items, Option<Key> Value, Func<Key, string> Label, Func<Edit<Key>, IO<Unit>> Selected) {
    private static readonly Key Folded = new Key.Named("overflow");

    public Option<Func<Key, GlyphSource>> Glyph { get; init; }
    public Option<Func<Key, Node>> Content { get; init; }
    public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Edited { get; init; }
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;

    public Node Drawn {
        get {
            Node strip = new Node.Choice(Axis, Strips.Map(static (strip, index) => strip with { Key = new Key.Ordinal((uint)index) })) {
                Width = Axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
                Height = Axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            };
            return (from content in Content from key in Value select content(key)).Match(
                Some: page => new Node.Flex(Axis.Cross, [strip, page with { Width = page.Width | Some<Sizing>(Sizing.Grow.Unit), Height = page.Height | Some<Sizing>(Sizing.Grow.Unit) }]) {
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                },
                None: () => strip);
        }
    }

    private Seq<Node> Strips =>
        Seq(Strip(Items, iconic: false, [])) + Glyph.Map(_ => Strip(Items, iconic: true, [])).ToSeq()
        + toSeq(Range(1, int.Max(0, Items.Count - 1))).Map(hidden => Kept(Items.Count - hidden) switch {
            var shown => Strip(shown, Glyph.IsSome, Items.Filter(key => !shown.Exists(held => held == key))),
        });

    private Func<Edit<Selection>, IO<Unit>> Chosen => edit => Selected(new Edit<Key>(edit.Phase, edit.Value.Bind(static held => held.Lead)));

    private Seq<Key> Kept(int count) {
        HashSet<Key> kept = toHashSet((Items.Filter(key => Value == Some(key)) + Items.Filter(key => Value != Some(key))).Take(count));
        return Items.Filter(kept.Contains);
    }

    private Node Strip(Seq<Key> shown, bool iconic, Seq<Key> hidden) =>
        new Part.Choice(shown, new Selection.One(Value), Chosen) { Reorder = hidden.IsEmpty ? Edited : None }.Drawn(
            key => Tab(key, iconic),
            tabs => new Node.Flex(Axis, tabs + (hidden.IsEmpty ? [] : Seq(Overflow(hidden)))) {
                Role = Role.TabList,
                Accessible = new Accessible { Role = AccessRole.TabList, Orientation = Axis },
            });

    private Node.Flex Tab(Key key, bool iconic) =>
        new(PhysicalAxis.Horizontal,
            Faced(key, iconic)
            + Edited.Filter(_ => !iconic).Map(edited => new Part.Press(_ => edited(Edit.Of(EditPhase.Commit, Items.Filter(held => held != key)))).Drawn(new Node.Box { Role = Role.Dismiss, Align = UnitPoint.Center })).ToSeq()) {
            Role = Role.Tab,
            Accessible = new Accessible { Role = AccessRole.Tab, Label = Label(key) },
        };

    private Seq<Node> Faced(Key key, bool iconic) =>
        Panels.Pictured(Glyph.Map(glyph => glyph(key))) + (iconic ? [] : Seq<Node>(new Node.Text([Label(key)]) { MaxLines = 1, Align = UnitPoint.Center }));

    private Node Overflow(Seq<Key> hidden) =>
        Controls.Dropdown(new Node.Box { Role = Role.Overflow, Align = UnitPoint.Center }, new Part.Choice(hidden, new Selection.One(Value), Chosen).Drawn(
            key => new Node.Flex(PhysicalAxis.Horizontal, Faced(key, iconic: false)) { Width = Sizing.Grow.Unit },
            static rows => new Node.Flex(PhysicalAxis.Vertical, rows) { Role = Role.Menu })) with {
            Key = Folded,
        };
}

public sealed record Card(string Title) {
    public Option<string> Subtitle { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<Node> Media { get; init; }
    public Seq<Node> Content { get; init; }
    public Seq<DialogAction> Actions { get; init; }
    public HashSet<Condition> Conditions { get; init; }

    public Node Drawn =>
        new Node.Flex(PhysicalAxis.Vertical,
            Media.ToSeq()
            + Seq<Node>(Panels.Header(Panels.Pictured(Glyph), Title, Subtitle, Panels.Status(Conditions).ToSeq()))
            + Content
            + Dialogs.Answered(Actions, unit)) {
            Role = Role.Card,
            Width = Sizing.Grow.Unit,
            Conditions = Conditions,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Title },
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Panels {
    private static readonly Seq<Condition> Signals = [Condition.Faulted, Condition.Warning, Condition.Success];

    internal static Node.Flex Header(Seq<Node> leading, string title, Option<string> subtitle, Seq<Node> trailing) =>
        new(PhysicalAxis.Horizontal,
            leading
            + Seq<Node>(new Node.Flex(PhysicalAxis.Vertical,
                Seq<Node>(new Node.Text([title]) { MaxLines = 1, Accessible = new Accessible { Role = AccessRole.Heading } })
                + subtitle.Map(static text => (Node)new Node.Text([text]) { Role = Role.Caption, MaxLines = 1 }).ToSeq()) {
                Width = Sizing.Grow.Unit,
                Align = UnitPoint.Center,
            })
            + trailing) {
            Width = Sizing.Grow.Unit,
        };

    internal static Seq<Node> Pictured(Option<GlyphSource> glyph) => glyph.Map(static art => (Node)new Node.Glyph(art) { Align = UnitPoint.Center }).ToSeq();

    internal static Option<Node> Status(HashSet<Condition> conditions) =>
        conditions.Contains(Condition.Busy)
            ? Some<Node>(new Node.Box { Role = Role.Spinner, Conditions = [Condition.Busy], Align = UnitPoint.Center })
            : Signals.Find(conditions.Contains).Map(static signal => (Node)new Node.Box { Role = Role.Status, Conditions = [signal], Align = UnitPoint.Center });

    internal static Node Said(string message) =>
        new Node.Text([message]) { Role = Role.Caption, Alignment = TextAlignment.Center, Width = Sizing.Grow.Unit, Align = UnitPoint.Center };

    internal static Node Body(Seq<Node> content) =>
        content switch {
            [{ Height.Case: Sizing { Stretches: true } } only] => only,
            _ => new Node.Scroll(PhysicalAxis.Vertical, new Node.Flex(PhysicalAxis.Vertical, content) { Width = Sizing.Grow.Unit }) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            },
        };
}
