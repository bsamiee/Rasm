using System.Globalization;
using Rasm.UI.Components;
using Rasm.UI.Styling;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DialogAction(CommandPresentation Presentation, IO<Unit> Run) {
    public HashSet<Condition> Conditions { get; init; }
}

public sealed record Dialog(string Title, Seq<DialogAction> Actions) {
    public Option<string> Message { get; init; }
    public Seq<Node> Content { get; init; }
    public Fin<Unit> Valid { get; init; } = unit;
    public Option<IO<Unit>> Dismissed { get; init; }
    public HashSet<Condition> Conditions { get; init; }

    public Node.Layer Drawn =>
        new(None, new Node.Flex(PhysicalAxis.Vertical,
            Seq<Node>(Panels.Header(Panels.Status(Conditions).ToSeq(), Title, None, []) with { Role = Role.Header })
            + Panels.Body(Message.Map(static text => (Node)new Node.Text([text]) { Width = Sizing.Grow.Unit }).ToSeq() + Content)
            + Dialogs.Answered(Actions, Valid)) {
            Role = Role.Dialog,
            Conditions = Conditions,
        }) {
            Modal = true,
            Side = UnitPoint.Center,
            Align = UnitPoint.Center,
            Dismissed = Dismissed,
            Accessible = new Accessible { Label = Title, Help = Message },
        };
}

public sealed record Sheet(string Title, Seq<Node> Content) {
    public UnitPoint Edge { get; init; } = new(Fraction.MaxValue, Fraction.Half);
    public bool Modal { get; init; }
    public Seq<Node> Actions { get; init; }
    public Option<IO<Unit>> Dismissed { get; init; }

    public Node.Layer Drawn {
        get {
            Option<Sizing> spanned = new Sizing.Percent(Fraction.MaxValue);
            bool upright = Edge.X == Fraction.MinValue || Edge.X == Fraction.MaxValue;
            return new(None, new Node.Flex(PhysicalAxis.Vertical, [
                Panels.Header([], Title, None, Actions + Dismissed.Map(static close => new Part.Press(_ => close).Drawn(new Node.Box { Role = Role.Dismiss, Align = UnitPoint.Center })).ToSeq()) with {
                    Role = Role.Header,
                },
                Panels.Body(Content),
            ]) {
                Role = Role.Sheet,
            }) {
                Modal = Modal,
                Side = UnitPoint.Center,
                Align = Edge,
                Width = upright ? None : spanned,
                Height = upright ? spanned : None,
                Dismissed = Dismissed,
                Accessible = new Accessible { Label = Title },
            };
        }
    }
}

public sealed record Palette<TItem>(Seq<TItem> Items, Func<TItem, Key> Key, Func<TItem, string> Text, TextField Query, Func<Edit<Key>, IO<Unit>> Edited) where TItem : notnull {
    private static readonly Node Rule = new Node.Box { Role = Role.Divider, Width = Sizing.Grow.Unit };

    public Option<Key> Active { get; init; }
    public Func<TItem, Option<string>> Group { get; init; } = static _ => None;
    public Func<TItem, Option<GlyphSource>> Glyph { get; init; } = static _ => None;
    public Func<TItem, Option<string>> Shortcut { get; init; } = static _ => None;
    public Func<TItem, Option<Node>> Preview { get; init; } = static _ => None;
    public Seq<Key> Recent { get; init; }
    public Option<string> Empty { get; init; }
    public Option<IO<Unit>> Dismissed { get; init; }

    public Node.Layer Drawn(CultureInfo culture) {
        Search query = new(Query.Text);
        Seq<Seq<TItem>> groups = Grouped(query, culture);
        Seq<TItem> found = groups.Flatten();
        HashMap<Key, TItem> keyed = toHashMap(found.Map(item => (Key(item), item)));
        Option<TItem> active = found.Find(item => Active == Some(Key(item))) | found.Head;
        Node results = found.IsEmpty
            ? Empty.Map(Panels.Said).IfNone(static () => new Node.Box())
            : new Node.Scroll(PhysicalAxis.Vertical, new Part.Choice(found.Map(Key), new Selection.One(active.Map(Key)), edit => Edited(new Edit<Key>(edit.Phase, edit.Value.Bind(static held => held.Lead)))).Drawn(
                key => Row(keyed[key], active.Exists(held => Key(held) == key), query, culture),
                rows => new Node.Flex(PhysicalAxis.Vertical, Sliced(groups, rows)) { Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Menu } })) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
        return new(None, new Node.Flex(PhysicalAxis.Vertical, [
            (Query with { Glyph = Query.Glyph | Some(Role.Magnifier) }).Drawn,
            active.Bind(Preview).Match(
                Some: pane => new Node.Choice(PhysicalAxis.Horizontal, [new Node.Flex(PhysicalAxis.Horizontal, [results, pane]) { Key = new Key.Ordinal(0), Width = Sizing.Grow.Unit }, results with { Key = new Key.Ordinal(1) }]) {
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                },
                None: () => results),
        ]) {
            Role = Role.Popover,
        }) {
            Side = UnitPoint.Center,
            Align = new UnitPoint(Fraction.Half, Fraction.MinValue),
            Dismissed = Dismissed,
            Accessible = new Accessible { Label = Query.Placeholder },
        };
    }

    private Seq<Seq<TItem>> Grouped(Search query, CultureInfo culture) {
        Seq<TItem> recent = Query.Text.Length is 0 ? Recent.Choose(key => Items.Find(item => Key(item) == key)) : [];
        Seq<TItem> rest = Items.Filter(item => !recent.Exists(held => Key(held) == Key(item)) && query.Matches(Text(item), culture));
        return (recent.IsEmpty ? [] : Seq(recent)) + toSeq(rest.GroupBy(Group)).Map(static group => toSeq(group));
    }

    private Seq<Node> Sliced(Seq<Seq<TItem>> groups, Seq<Node> rows) =>
        groups.Fold((At: 0, Nodes: Seq<Node>()), (held, group) => (held.At + group.Count, held.Nodes
            + (held.At > 0 ? Seq(Rule) : [])
            + Seq<Node>(group.Head.Filter(item => Query.Text.Length > 0 || !Recent.Exists(key => key == Key(item))).Bind(Group) switch {
                var heading => new Node.Flex(PhysicalAxis.Vertical,
                    heading.Map(static title => (Node)new Node.Text([title]) { Role = Role.Caption, MaxLines = 1 }).ToSeq() + rows.Skip(held.At).Take(group.Count)) {
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.Group, Label = heading },
                },
            }))).Nodes;

    private Node Row(TItem item, bool active, Search query, CultureInfo culture) =>
        new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, Key(item)))).Drawn(new Node.Flex(PhysicalAxis.Horizontal,
            Panels.Pictured(Glyph(item))
            + Seq<Node>(new Node.Text(query.Marked(Text(item), culture)) { MaxLines = 1, Width = Sizing.Grow.Unit, Align = UnitPoint.Center })
            + Shortcut(item).Map(static keys => (Node)new Node.Text([keys]) { Role = Role.Keycap, Align = UnitPoint.Center }).ToSeq()) {
            Width = Sizing.Grow.Unit,
            Conditions = active ? [Condition.Default] : [],
            Accessible = new Accessible { Role = AccessRole.MenuItem, Label = Text(item) },
        });
}

public sealed record Step(Key Key, string Title, Node Content) {
    public Fin<Unit> Valid { get; init; } = unit;
    public Option<string> Summary { get; init; }
}

public sealed record Steps(Seq<Step> Pages, Key Current, Func<Edit<Key>, IO<Unit>> Edited, CommandPresentation Back, CommandPresentation Next, DialogAction Finish) {
    public Option<DialogAction> Cancel { get; init; }

    public Node Drawn(CultureInfo culture) {
        int at = Selection.Position(Pages.Map(static page => page.Key), Current).IfNone(0);
        int reach = Pages.Map(static (page, index) => (Page: page, Index: index)).Find(static held => held.Page.Valid.IsFail).Map(static held => held.Index + 1).IfNone(Pages.Count);
        HashMap<Key, (Step Page, int Index)> indexed = toHashMap(Pages.Map(static (page, index) => (page.Key, (Page: page, Index: index))));
        Option<Step> next = Pages.Skip(at + 1).Head;
        return new Node.Flex(PhysicalAxis.Vertical,
            Seq(new Part.Choice(Pages.Take(reach).Map(static page => page.Key), new Selection.One(Some(Current)), edit => Edited(new Edit<Key>(edit.Phase, edit.Value.Bind(static held => held.Lead)))).Drawn(
                key => indexed[key] switch { var held => Marker(held.Page, held.Index, at, culture) },
                markers => new Node.Flex(PhysicalAxis.Horizontal, markers + Pages.Skip(reach).Map((page, index) => Marker(page, reach + index, at, culture).Conditioned(Condition.Disabled))) {
                    Role = Role.TabList,
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.TabList, Orientation = PhysicalAxis.Horizontal },
                }))
            + Pages.Skip(at).Head.Map(static page => page.Content).ToSeq()
            + Dialogs.Answered(
                Cancel.ToSeq()
                + Pages.Take(at).Last.Map(prior => new DialogAction(Back, Edited(Edit.Of(EditPhase.Commit, prior.Key)))).ToSeq()
                + Seq(next.Match(
                    Some: following => new DialogAction(Next, Edited(Edit.Of(EditPhase.Commit, following.Key))) { Conditions = [Condition.Default] },
                    None: () => Finish with { Conditions = Finish.Conditions.TryAdd(Condition.Default) })),
                next.IsSome ? Pages.Skip(at).Head.Map(static page => page.Valid).IfNone(unit) : Pages.Traverse(static page => page.Valid).As().Map(static _ => unit))) {
            Width = Sizing.Grow.Unit,
        };
    }

    private static Node.Flex Marker(Step page, int index, int at, CultureInfo culture) =>
        (index < at && page.Valid.IsSucc) switch {
            var done => new Node.Flex(PhysicalAxis.Horizontal, [
                done
                    ? new Node.Box { Role = Role.Status, Conditions = [Condition.Success], Align = UnitPoint.Center }
                    : new Node.Text([(index + 1).ToString(culture)]) { Role = Role.Caption, Align = UnitPoint.Center },
                new Node.Flex(PhysicalAxis.Vertical,
                    Seq<Node>(new Node.Text([page.Title]) { MaxLines = 1 })
                    + page.Summary.Filter(_ => done).Map(static summary => (Node)new Node.Text([summary]) { Role = Role.Caption, MaxLines = 1 }).ToSeq()) {
                    Align = UnitPoint.Center,
                },
            ]) {
                Role = Role.Tab,
                Accessible = new Accessible { Role = AccessRole.Tab, Label = page.Title },
            },
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Dialogs {
    internal static Seq<Node> Answered(Seq<DialogAction> actions, Fin<Unit> valid) =>
        valid.Match(
            Succ: static _ => Seq<Node>(),
            Fail: static error => error.IsExpected ? Seq<Node>(new Node.Text([error.Message]) { Role = Role.Label, Conditions = [Condition.Faulted], Accessible = new Accessible { Live = Live.Polite } }) : [])
        + (actions.IsEmpty ? [] : Seq<Node>(new Node.Flex(PhysicalAxis.Horizontal, Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit }) + actions.Map(action => Pressed(action, valid.IsFail))) {
            Width = Sizing.Grow.Unit,
        }));

    private static Node Pressed(DialogAction action, bool gated) =>
        (gated && action.Conditions.Contains(Condition.Default) ? action.Conditions.TryAdd(Condition.Disabled) : action.Conditions) switch {
            var marks => new Part.Press(_ => action.Run).Drawn(new Node.Flex(PhysicalAxis.Horizontal, Commands.IconAndLabel(action.Presentation, new CommandState(marks, None))) {
                Key = new Key.Named(action.Presentation.Name),
                Role = Role.Button,
                Conditions = marks,
                Accessible = new Accessible { Label = action.Presentation.Caption, Help = action.Presentation.Help, Shortcut = action.Presentation.Shortcut },
            }),
        };
}
