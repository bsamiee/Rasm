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
            Fail: static error => error.IsExpected ? Seq<View>(new View.Text([error.Message]) { Role = Role.Label, Conditions = [Condition.Faulted], Accessible = new Accessible { Live = Live.Polite } }) : [])
        + (actions.IsEmpty ? [] : Seq<View>(new View.Flex(PhysicalAxis.Horizontal, Seq<View>(new View.Box { Width = Sizing.Grow.Unit }) + actions.Map(action => action.Drawn(valid.IsFail))) {
            Width = Sizing.Grow.Unit,
        }));

    private View Drawn(bool gated) =>
        (gated && Conditions.Contains(Condition.Default) ? Conditions.TryAdd(Condition.Disabled) : Conditions) switch {
            var state => Presentation.Button(state, AccessRole.Button, Role.Button, Some(new Part.Press(_ => Run)), CommandPresentation.IconAndLabel(Presentation, state)),
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
                Role = upright || level ? Role.Sheet : Role.Dialog,
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
        Seq<Seq<TItem>> groups = Grouped(query, culture);
        Seq<TItem> found = groups.Flatten();
        Option<TItem> current = found.Find(item => Active == Some(Keyed(item))) | found.Head;
        Option<Key> active = current.Map(Keyed);
        View results = found.IsEmpty
            ? Empty.Map(Panel.Placeholder).IfNone(static () => new View.Box())
            : new View.Scroll(PhysicalAxis.Vertical, new Part.Choice(found.Map(Keyed), active, Edited).Drawn(
                found.Map(item => Row(item, query, culture)),
                rows => new View.Flex(PhysicalAxis.Vertical, Sectioned(groups, rows, query)) { Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Menu } })).Grown;
        return new(new View.Flex(PhysicalAxis.Vertical, [
            (Query with { Glyph = Query.Glyph | Some(Role.Magnifier) }).Drawn,
            current.Bind(Preview).Match(
                Some: pane => new View.Choice(PhysicalAxis.Horizontal, [new View.Flex(PhysicalAxis.Horizontal, [results, pane]) { Width = Sizing.Grow.Unit }, results]).Grown,
                None: () => results),
        ]) {
            Role = Role.Popover,
        }) {
            Anchor = None,
            Side = UnitPoint.Center,
            Align = new UnitPoint(Fraction.Half, Fraction.MinValue),
            Dismissed = Dismissed,
            Accessible = new Accessible { Label = Query.Placeholder },
        };
    }

    private Key Keyed(TItem item) => new Key.Named(Presented(item).Name);

    private Seq<Seq<TItem>> Grouped(Search query, CultureInfo culture) {
        Seq<TItem> recent = query.Terms.IsEmpty ? Recent.Choose(key => Items.Find(item => Keyed(item) == key)) : [];
        Seq<TItem> rest = Items.Filter(item => !recent.Exists(held => Keyed(held) == Keyed(item)) && query.Matches(Presented(item).Caption, culture));
        return (recent.IsEmpty ? [] : Seq(recent)) + toSeq(rest.GroupBy(Group)).Map(static group => toSeq(group));
    }

    private Seq<View> Sectioned(Seq<Seq<TItem>> groups, Seq<View> rows, Search query) =>
        groups.Fold((At: 0, Views: Seq<View>()), (held, group) => (held.At + group.Count, held.Views
            + (held.At > 0 ? Seq(View.Divider(PhysicalAxis.Vertical)) : [])
            + Seq<View>(group.Head.Filter(item => !query.Terms.IsEmpty || !Recent.Exists(key => key == Keyed(item))).Bind(Group) switch {
                var heading => new View.Flex(PhysicalAxis.Vertical,
                    heading.Map(static title => (View)new View.Text([title]) { Role = Role.Caption, MaxLines = 1 }).ToSeq() + rows.Skip(held.At).Take(group.Count)) {
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.Group, Label = heading },
                },
            }))).Views;

    private View Row(TItem item, Search query, CultureInfo culture) => Presented(item) switch { var shown => shown.Item([], None, query.Marked(shown.Caption, culture)) };
}

public sealed record Toast(Key Key, View Content, Option<Duration> Lifetime);

public sealed record Toasts(Seq<Toast> Shown, Func<Key, IO<Unit>> Dismissed) {
    private static readonly Key Surface = new Key.Ordinal(0);
    private static readonly Points<bool> Swiped = new(static _ => [], static (shown, _, _) => new Dragged<bool>(shown, [])) {
        Placed = static (shown, _, _) => Some(new Dragged<bool>(shown, [Surface])),
        Removed = static (_, _) => Some(value: false),
        DraggedOut = true,
    };

    public Role Role { get; init; } = Role.Popover;
    public UnitPoint Corner { get; init; } = new(Fraction.MaxValue, Fraction.MaxValue);
    public int Limit { get; init; } = 3;

    public View.Layer Drawn => new(new View.Flex(PhysicalAxis.Vertical, toSeq(Shown.TakeLast(Limit)).Map(Toasted)) { Align = Corner }) { Anchor = None, Side = Corner, Align = Corner };

    private View Toasted(Toast toast) =>
        new View.Plot(PlotPlane.Cartesian.Unit, [
            new Handles<bool>(Value: true, Swiped, edit => when(edit.Phase == EditPhase.Commit && edit.Value.Exists(static shown => !shown), Dismissed(toast.Key)).As()).Drawn(PlotPlane.Cartesian.Unit, []),
            new View.Flex(PhysicalAxis.Horizontal, [toast.Content]) { Role = Role, Accessible = new Accessible { Role = AccessRole.Status, Live = Live.Polite } },
        ]) {
            Key = toast.Key,
            Expires = toast.Lifetime.Map(lifetime => new Expiry(lifetime, Dismissed(toast.Key))),
        };
}
