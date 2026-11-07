using System.Globalization;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Styling;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Tabs(Seq<Key> Items, Option<Key> Value, Func<Key, string> Label, Func<Edit<Key>, IO<Unit>> Selected) {
    private static readonly Key Folded = new Key.Named("overflow");

    public Option<Func<Key, GlyphSource>> Glyph { get; init; }
    public Option<Func<Key, View>> Face { get; init; }
    public Func<Key, bool> Enabled { get; init; } = static _ => true;
    public Option<Func<Key, View>> Content { get; init; }
    public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Edited { get; init; }
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;

    public View Drawn {
        get {
            View strip = new View.Choice(Axis, Strips) {
                Width = Axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
                Height = Axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            };
            return (from content in Content from key in Value select content(key)).Match(
                Some: page => new View.Flex(Axis.Cross, [strip, page with { Width = page.Width | Some<Sizing>(Sizing.Grow.Unit), Height = page.Height | Some<Sizing>(Sizing.Grow.Unit) }]).Grown,
                None: () => strip);
        }
    }

    internal static View Overflow(Seq<Key> hidden, Part.Choice choice, Func<Key, View> face) =>
        Picker.Dropdown(new View.Flex(PhysicalAxis.Horizontal, [new View.Box { Role = Roles.Overflow, Align = UnitPoint.Center }]) { Role = Roles.Button },
            choice.Drawn(hidden,
                key => face(key) switch { var row => row with { Width = Sizing.Grow.Unit, Accessible = row.Accessible.Combine(new Accessible { Role = AccessRole.MenuItem }) } },
                static rows => new View.Flex(PhysicalAxis.Vertical, rows) { Role = Roles.Menu, Accessible = new Accessible { Role = AccessRole.Menu } })) with {
            Key = Folded,
        };

    private Seq<View> Strips =>
        Seq(Strip(Items, iconic: false, [])) + Glyph.Map(_ => Strip(Items, iconic: true, [])).ToSeq()
        + toSeq(Range(1, int.Max(0, Items.Count - 1))).Map(hidden => Kept(Items.Count - hidden) switch {
            var shown => Strip(shown, Glyph.IsSome, toSeq(Items.Except(shown))),
        });

    private Seq<Key> Kept(int count) {
        HashSet<Key> kept = toHashSet((Items.Filter(key => Value == Some(key)) + Items.Filter(key => Value != Some(key))).Take(count));
        return Items.Filter(kept.Contains);
    }

    private View Strip(Seq<Key> shown, bool iconic, Seq<Key> hidden) =>
        new Part.Choice(shown.Filter(Enabled), Value, Selected) {
            Reorder = hidden.IsEmpty && shown.ForAll(Enabled) ? Edited : None,
            Removed = Edited.Map(edited => Part.Choice.Without(Items, edited)),
        } switch {
            var choice => choice.Drawn(
                shown,
                key => Tab(choice, key, iconic),
                tabs => new View.Flex(Axis,
                    tabs + (hidden.IsEmpty ? [] : Seq(Overflow(hidden, new Part.Choice(hidden.Filter(Enabled), Value, Selected), key => new View.Flex(PhysicalAxis.Horizontal, Faced(key, iconic: false)))))) {
                    Role = Roles.TabList,
                    Accessible = new Accessible { Role = AccessRole.TabList, Orientation = Axis },
                }),
        };

    private View.Flex Tab(Part.Choice choice, Key key, bool iconic) =>
        new(PhysicalAxis.Horizontal, Faced(key, iconic) + choice.Dismiss(key).Filter(_ => !iconic).ToSeq()) {
            Role = Roles.Tab,
            Accessible = new Accessible { Role = AccessRole.Tab, Label = Label(key) },
        };

    private Seq<View> Faced(Key key, bool iconic) =>
        Face.Match(
            Some: face => Seq(face(key)),
            None: () => Panel.Icon(Glyph.Map(glyph => glyph(key))) + (iconic ? [] : Seq<View>(new View.Text([Label(key)]) { MaxLines = 1, Align = UnitPoint.Center })));
}

public sealed record Breadcrumb(Seq<Key> Path, Func<Key, View> Face, Func<Key, IO<Unit>> Pressed) {
    private static readonly View Separator = new View.Box { Role = Roles.Chevron, Align = UnitPoint.Center };

    public View Drawn =>
        new View.Choice(PhysicalAxis.Horizontal, toSeq(Range(0, int.Max(1, Path.Count - 1))).Map(hidden => (View)new View.Flex(PhysicalAxis.Horizontal,
            hidden is 0
                ? Path.Map(Segment)
                : toSeq(Enumerable.Take(Path.Skip(1), hidden)) switch {
                    var folded => Path.Head.ToSeq().Map(Segment) + Seq(Tabs.Overflow(folded, new Part.Choice(folded, None, edit => edit.Value.Filter(_ => edit.Phase == EditPhase.Commit).Map(Pressed).IfNone(unitIO)), Face), Separator) + Path.Skip(1 + hidden).Map(Segment),
                }))) {
            Conditions = [Condition.Borderless],
        };

    private View Segment(Key key) =>
        Face(key) switch {
            var face => new View.Flex(PhysicalAxis.Horizontal,
                Seq(new Part.Press(_ => Pressed(key)).Drawn(face with { Role = face.Role | Some(Roles.Button), Conditions = Path.Last == Some(key) ? face.Conditions.TryAdd(Condition.Selected) : face.Conditions }))
                + (Path.Last == Some(key) ? [] : Seq(Separator))) {
                Key = key,
            },
        };
}

public sealed record Step(Key Key, string Title, View Content) {
    public Fin<Unit> Valid { get; init; } = unit;
    public Option<string> Summary { get; init; }
}

public sealed record Steps(Seq<Step> Pages, Key Current, Func<Edit<Key>, IO<Unit>> Edited, CommandPresentation Back, CommandPresentation Next, DialogAction Finish) {
    public Option<DialogAction> Cancel { get; init; }

    public Fin<View> Drawn(CultureInfo culture) {
        HashMap<Key, (Step Page, int Index)> indexed = toHashMap(Pages.Map(static (page, index) => (page.Key, (Page: page, Index: index))));
        int reach = int.Min(Pages.TakeWhile(static page => page.Valid.IsSucc).Count + 1, Pages.Count);
        return
            from at in indexed.Find(Current).Map(static held => held.Index).ToFin(new UIFault(Current, UIReason.Missing))
            let next = Pages.Skip(at + 1).Head
            select (View)new View.Flex(PhysicalAxis.Vertical,
                Seq(new Tabs(Pages.Map(static page => page.Key), Some(Current), key => indexed[key].Page.Title, Edited) {
                    Face = Some<Func<Key, View>>(key => indexed[key] switch { var held => Marker(held.Page, held.Index, held.Index < at && held.Page.Valid.IsSucc, culture) }),
                    Enabled = key => indexed[key].Index < reach,
                    Content = Some<Func<Key, View>>(key => indexed[key].Page.Content),
                }.Drawn)
                + DialogAction.Footer(
                    Cancel.ToSeq()
                    + Pages.At(at - 1).Map(prior => new DialogAction(Back, Edited(Edit.Of(EditPhase.Commit, prior.Key)))).ToSeq()
                    + Seq(next.Match(
                        Some: following => new DialogAction(Next, Edited(Edit.Of(EditPhase.Commit, following.Key))) { Conditions = [Condition.Default] },
                        None: () => Finish with { Conditions = Finish.Conditions.TryAdd(Condition.Default) })),
                    next.IsSome ? Pages.Skip(at).Head.Map(static page => page.Valid).IfNone(unit) : Pages.Traverse(static page => page.Valid).As().Map(static _ => unit))) {
                Width = Sizing.Grow.Unit,
            };
    }

    private static View.Flex Marker(Step page, int index, bool done, CultureInfo culture) =>
        new(PhysicalAxis.Horizontal, [
            Panel.StatusGlyph(done ? [Condition.Success] : []).IfNone(() => new View.Text([(index + 1).ToString(culture)]) { Role = Roles.Caption, Align = UnitPoint.Center }),
            new View.Flex(PhysicalAxis.Vertical,
                Seq<View>(new View.Text([page.Title]) { MaxLines = 1 })
                + page.Summary.Filter(_ => done).Map(static summary => (View)new View.Text([summary]) { Role = Roles.Caption, MaxLines = 1 }).ToSeq()) {
                Align = UnitPoint.Center,
            },
        ]);
}
