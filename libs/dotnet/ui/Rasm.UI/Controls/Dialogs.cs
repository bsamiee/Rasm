using System.Globalization;
using Rasm.UI.Motion;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DialogAction(CommandPresentation Presentation, IO<Unit> Run) {
    public HashSet<Condition> Conditions { get; init; }

    internal static Seq<View> Footer(Seq<DialogAction> actions, Fin<Unit> valid) =>
        valid.Match(
            Succ: static _ => Seq<View>(),
            Fail: static error => error.IsExpected ? Seq<View>(new View.Text([error.Message]) { Role = Roles.Label, Conditions = [Condition.Faulted], Accessible = new Accessible { Interrupts = Some(value: false) } }) : [])
        + (actions.IsEmpty ? [] : Seq<View>(new View.Flex(PhysicalAxis.Horizontal, Seq<View>(new View.Box { Width = Sizing.Grow.Unit }) + actions.Map(action => action.Drawn(valid.IsFail))) {
            Width = Sizing.Grow.Unit,
        }));

    private View Drawn(bool gated) =>
        (gated && Conditions.Contains(Condition.Default) ? Conditions.TryAdd(Condition.Disabled) : Conditions) switch {
            var state => Presentation.Button(state, AccessRole.Button, Roles.Button, Some(new Part.Press(_ => Run)), CommandPresentation.IconAndLabel(Presentation, state)),
        };
}

public sealed record Dialog(string Title, Seq<View> Content) {
    public UnitPoint Edge { get; init; } = UnitPoint.Center;
    public bool Modal { get; init; } = true;
    public Option<string> Message { get; init; }
    public Seq<View> Tools { get; init; }
    public Seq<DialogAction> Actions { get; init; }
    public Fin<Unit> Valid { get; init; } = unit;
    public Option<IO<Unit>> Dismissed { get; init; }
    public HashSet<Condition> Conditions { get; init; }

    public View.Layer Drawn {
        get {
            Option<Sizing> spanned = new Sizing.Percent(Fraction.MaxValue);
            bool upright = Edge.X == Fraction.MinValue || Edge.X == Fraction.MaxValue;
            bool level = !upright && (Edge.Y == Fraction.MinValue || Edge.Y == Fraction.MaxValue);
            return new(new View.Flex(PhysicalAxis.Vertical,
                Seq<View>(Panel.Header(Panel.StatusGlyph(Conditions).ToSeq(), Title, None, Tools + Dismissed.Filter(_ => Actions.IsEmpty).Map(Part.Press.Dismiss).ToSeq()))
                + Panel.Scrolled(Message.Map(static text => (View)new View.Text([text]) { Width = Sizing.Grow.Unit }).ToSeq() + Content)
                + DialogAction.Footer(Actions, Valid)) {
                Role = upright || level ? Roles.Sheet : Roles.Dialog,
                Conditions = Conditions,
            }) {
                Anchor = None,
                Modal = Modal,
                Side = UnitPoint.Center,
                Align = Edge,
                Width = level ? spanned : None,
                Height = upright ? spanned : None,
                Dismissed = Dismissed,
                Accessible = new Accessible { Label = Title, Help = Message },
            };
        }
    }
}

public sealed record Palette<TItem>(Seq<TItem> Items, Func<TItem, CommandPresentation> Presented, TextField Query, Func<Edit<Key>, IO<Unit>> Edited) where TItem : notnull {
    public Option<Key> Active { get; init; }
    public Func<TItem, Option<string>> Group { get; init; } = static _ => None;
    public Func<TItem, Option<View>> Preview { get; init; } = static _ => None;
    public Seq<Key> Recent { get; init; }
    public Option<string> Empty { get; init; }
    public Option<IO<Unit>> Dismissed { get; init; }

    public View.Layer Drawn(CultureInfo culture) {
        Search query = Query.Search;
        Seq<(TItem Item, CommandPresentation Shown)> entries = Items.Map(item => (Item: item, Shown: Presented(item))).Strict();
        Seq<(TItem Item, CommandPresentation Shown)> recent = query.Terms.IsEmpty ? Recent.Choose(toHashMap(entries.Map(static entry => (entry.Shown.Key, entry))).Find) : [];
        HashSet<Key> recents = toHashSet(recent.Map(static entry => entry.Shown.Key));
        Seq<(Option<string> Heading, Seq<(TItem Item, CommandPresentation Shown)> Items)> groups = (recent.IsEmpty ? [] : Seq((Heading: Option<string>.None, Items: recent)))
            + toSeq(entries.Filter(entry => !recents.Contains(entry.Shown.Key) && query.Matches(entry.Shown.Caption, culture)).GroupBy(entry => Group(entry.Item)))
                .Map(static group => (Heading: group.Key, Items: toSeq(group)));
        Seq<(TItem Item, CommandPresentation Shown)> found = groups.Bind(static group => group.Items);
        Option<(TItem Item, CommandPresentation Shown)> current = found.Find(entry => Active == Some(entry.Shown.Key)) || found.Head;
        Option<Key> active = current.Map(static entry => entry.Shown.Key);
        View results = found.IsEmpty
            ? Empty.Map(Panel.Placeholder).IfNone(static () => new View.Box())
            : new View.Scroll(PhysicalAxis.Vertical, new Part.Choice(found.Map(static entry => entry.Shown.Key), active, Edited).Drawn(
                found.Map(entry => entry.Shown.Item([], None, query.Marked(entry.Shown.Caption, culture))),
                rows => new View.Flex(PhysicalAxis.Vertical,
                    groups.Zip(groups.Scan(0, static (at, group) => at + group.Items.Count)).Map(group => (View)new View.Flex(PhysicalAxis.Vertical,
                        group.First.Heading.Map(static title => (View)new View.Text([title]) { Role = Roles.Caption, MaxLines = 1 }).ToSeq()
                        + rows.Skip(group.Second).Take(group.First.Items.Count)) {
                        Width = Sizing.Grow.Unit,
                        Accessible = new Accessible { Role = AccessRole.Group, Label = group.First.Heading },
                    }).Intersperse(View.Divider(PhysicalAxis.Vertical))) {
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.Menu },
                })).Grown;
        return new(new View.Flex(PhysicalAxis.Vertical, [
            (Query with { Glyph = Query.Glyph | Some(Roles.Magnifier) }).Drawn,
            current.Bind(entry => Preview(entry.Item)).Match(
                Some: pane => new View.Choice(PhysicalAxis.Horizontal, [new View.Flex(PhysicalAxis.Horizontal, [results, pane]) { Width = Sizing.Grow.Unit }, results]).Grown,
                None: () => results),
        ]) {
            Role = Roles.Popover,
        }) {
            Anchor = None,
            Side = UnitPoint.Center,
            Align = new UnitPoint(Fraction.Half, Fraction.MinValue),
            Dismissed = Dismissed,
            Accessible = new Accessible { Label = Query.Placeholder },
        };
    }
}

public sealed record Toast(Key Key, View Content, Option<Duration> Lifetime);

public sealed record Toasts(Seq<Toast> Shown, Func<Key, IO<Unit>> Dismissed) {
    private static readonly Key Surface = new Key.Ordinal(0);
    private static readonly Points<bool> Swiped = new(static _ => [], static (shown, _, _) => (shown, [])) {
        Placed = static (shown, _, _) => Some((shown, Surface)),
        Removed = static (_, _) => Some((Value: false, Renames: HashMap((Surface, Option<Key>.None)))),
        DraggedOut = true,
    };

    public Key Role { get; init; } = Roles.Popover;
    public UnitPoint Corner { get; init; } = new(Fraction.MaxValue, Fraction.MaxValue);
    public int Limit { get; init; } = 3;

    public View.Layer Drawn => new(new View.Flex(PhysicalAxis.Vertical, toSeq(Shown.TakeLast(Limit)).Map(Toasted)) { Align = Corner }) { Anchor = None, Side = Corner, Align = Corner };

    private View Toasted(Toast toast) =>
        new View.Plot(PlotPlane.Cartesian.Unit, [
            new Handles<bool>(Value: true, Swiped, edit => when(edit.Phase == EditPhase.Commit && edit.Value.Exists(static shown => !shown), Dismissed(toast.Key)).As()).Drawn(PlotPlane.Cartesian.Unit, []),
            new View.Flex(PhysicalAxis.Horizontal, [toast.Content]) { Role = Role, Accessible = new Accessible { Role = AccessRole.Status, Interrupts = Some(value: false) } },
        ]) {
            Key = toast.Key,
            Expires = toast.Lifetime.Map(lifetime => new Expiry(lifetime, Dismissed(toast.Key))),
        };
}
