using System.Globalization;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Topten.RichTextKit;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Toggle(Check Value, Func<Edit<Check>, IO<Unit>> Edited) {
    private static readonly Node Spacer = new Node.Box { Role = Role.Handle, Opacity = Fraction.MinValue };

    public Seq<Node> Content { get; init; }
    public Role Role { get; init; } = Role.Toggle;
    public Option<Check> Rest { get; init; }

    public Node Drawn {
        get {
            HashSet<Condition> state = toHashSet(Value.Conditioned.ToSeq());
            HashSet<Condition> marks = state.Union(Controls.Modified(Rest, Some(Value)));
            Node row = (Role.Equals(Role.Toggle) ? new Node.Flex(PhysicalAxis.Horizontal, Seq<Node>(new Node.Flex(PhysicalAxis.Horizontal, [new Node.Box { Role = Role.Checkmark, Conditions = state }]) { Role = Role, Conditions = state, Align = UnitPoint.Center }) + Content) { Role = Role.Label }
                : Role.Equals(Role.Switch) ? new Node.Flex(PhysicalAxis.Horizontal, Content.Add(Thumbed(state))) { Role = Role.Label }
                : new Node.Flex(PhysicalAxis.Horizontal, Content) { Role = Role }) with {
                Conditions = marks,
                Accessible = new Accessible { Role = Role.Equals(Role.Switch) ? AccessRole.Switch : AccessRole.CheckBox },
            };
            return new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, Value.Map(off: Check.On, on: Check.Off, mixed: Check.On)))) {
                Reset = Rest.Filter(rest => rest != Value).Map(rest => Edited(Edit.Of(EditPhase.Commit, rest))),
            }.Drawn(row);
        }
    }

    private Node.Overlay Thumbed(HashSet<Condition> state) =>
        new(new Node.Flex(PhysicalAxis.Horizontal, Seq(Spacer, Spacer)), [
            new Node.Box { Role = Role.Handle, Conditions = state, Align = new UnitPoint(Value.Map(off: Fraction.MinValue, on: Fraction.MaxValue, mixed: Fraction.Half), Fraction.Half) },
        ]) {
            Role = Role,
            Conditions = state,
            Align = UnitPoint.Center,
        };
}

public sealed record Picker(Seq<Key> Items, Option<Key> Value, Func<Key, string> Text, Func<Edit<Key>, IO<Unit>> Edited) {
    private static readonly Search Everything = new("");

    public Option<Func<Key, Node>> Face { get; init; }
    public Option<TextField> Filter { get; init; }
    public Option<Key> Rest { get; init; }

    public Node Drawn(CultureInfo culture) {
        HashSet<Condition> marks = toHashSet(Controls.Modified(Rest, Value));
        Search query = Filter.Map(static field => new Search(field.Text)).IfNone(Everything);
        Option<Sizing> share = Fraction.Validated(1f / Items.Count).ToOption().Map(static even => (Sizing)new Sizing.Percent(even));
        Node listed = new Node.Flex(PhysicalAxis.Vertical, Filter.Map(static field => field.Drawn).ToSeq().Add(new Node.Scroll(PhysicalAxis.Vertical,
            Choosing(Items.Filter(key => query.Matches(Text(key), culture))).Drawn(
                key => Faced(key, query, TextAlignment.Auto, culture) with { Width = Sizing.Grow.Unit },
                static rows => new Node.Flex(PhysicalAxis.Vertical, rows) { Width = Sizing.Grow.Unit })))) { Role = Role.Menu };
        Node face = new Node.Flex(PhysicalAxis.Horizontal, [
            Value.Map(key => Faced(key, Everything, TextAlignment.Auto, culture)).IfNone(static () => new Node.Box { Role = Role.Checkmark, Conditions = [Condition.Mixed], Align = UnitPoint.Center }),
            new Node.Box { Width = Sizing.Grow.Unit },
            new Node.Box { Role = Role.Chevron, Conditions = [Condition.Selected], Align = UnitPoint.Center },
        ]) { Role = Role.Button, Width = Sizing.Grow.Unit, Conditions = marks };
        Node dropdown = Controls.Dropdown(face, listed) with {
            Width = Sizing.Grow.Unit,
            Part = Some<Part>(Choosing(Items)),
            Accessible = new Accessible { Value = Value.Map(key => (AccessValue)new AccessValue.Text(Text(key))) },
        };
        return Filter.IsSome
            ? dropdown
            : new Node.Choice(PhysicalAxis.Horizontal, [
                Choosing(Items).Drawn(
                    key => new Node.Flex(PhysicalAxis.Horizontal, [Faced(key, Everything, TextAlignment.Center, culture) with { Width = Sizing.Grow.Unit }]) { Role = Role.Button, Width = share },
                    static segments => new Node.Flex(PhysicalAxis.Horizontal, segments) { Role = Role.Segment }) with {
                    Key = new Key.Ordinal(0),
                    Conditions = marks,
                },
                dropdown with { Key = new Key.Ordinal(1) },
            ]) { Width = Sizing.Grow.Unit };
    }

    private Part.Choice Choosing(Seq<Key> items) =>
        new(items, new Selection.One(Value), edit => Edited(new Edit<Key>(edit.Phase, edit.Value.Bind(static held => held.Lead)))) {
            Rest = Rest.Map(static rest => (Selection)new Selection.One(Some(rest))),
        };

    private Node Faced(Key key, Search query, TextAlignment alignment, CultureInfo culture) =>
        Face.Map(face => face(key)).IfNone(() => new Node.Text(query.Marked(Text(key), culture)) { MaxLines = 1, Alignment = alignment, Align = UnitPoint.Center });
}

public sealed record Disclosure(Node Header, Node Content, bool Expanded, Func<Edit<bool>, IO<Unit>> Edited) {
    public Role Role { get; init; } = Role.Header;

    public Node Drawn =>
        new Node.Flex(PhysicalAxis.Vertical, Seq(new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, !Expanded))).Drawn(
                new Node.Flex(PhysicalAxis.Horizontal, [new Node.Box { Role = Role.Chevron, Conditions = Expanded ? [Condition.Selected] : [], Align = UnitPoint.Center }, Header]) {
                    Role = Role,
                    Width = Sizing.Grow.Unit,
                    Sticks = PhysicalAxis.Vertical,
                    Accessible = new Accessible { Role = AccessRole.Disclosure, Expanded = Expanded },
                }))
            + (Expanded ? Seq(Content) : [])) {
            Width = Sizing.Grow.Unit,
        };
}

public sealed record TextField(string Text, Func<Edit<string>, IO<Unit>> Edited) {
    private static readonly Key Entry = new Key.Named("entry");

    public Option<string> Placeholder { get; init; }
    public Option<Role> Glyph { get; init; }
    public bool Clears { get; init; }
    public Option<int> MaxLines { get; init; } = Some(1);

    public Node Drawn =>
        new Node.Flex(PhysicalAxis.Horizontal,
            Glyph.Map(static role => (Node)new Node.Box { Role = role, Align = UnitPoint.Center }).ToSeq()
            + Seq<Node>(new Node.Text([Text]) {
                Key = Entry,
                MaxLines = MaxLines,
                Placeholder = Placeholder,
                Width = Sizing.Grow.Unit,
                Edited = Some(Edited),
                Accessible = new Accessible { Role = AccessRole.TextBox, Label = Placeholder },
            })
            + (Clears && Text.Length > 0 ? Seq(new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, ""))).Drawn(new Node.Box { Role = Role.Dismiss, Align = UnitPoint.Center })) : [])) {
            Role = Role.Field,
            Width = Sizing.Grow.Unit,
        };
}

public sealed record Tokens(Seq<Key> Items, Func<Key, Node> Face, Selection Selection, Func<Edit<Selection>, IO<Unit>> Selected) {
    public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Edited { get; init; }
    public Option<TextField> Entry { get; init; }

    public Node Drawn =>
        new Node.Flex(PhysicalAxis.Horizontal,
            Seq(new Part.Choice(Items, Selection, Selected) { Reorder = Edited }.Drawn(Chip, static chips => new Node.Flex(PhysicalAxis.Horizontal, chips) { Wraps = true }))
            + Entry.Map(static entry => entry.Drawn.Conditioned(Condition.Borderless)).ToSeq()) {
            Role = Role.Field,
            Width = Sizing.Grow.Unit,
            Wraps = true,
        };

    private Node Chip(Key key) =>
        new Node.Flex(PhysicalAxis.Horizontal, Face(key).Cons(Edited.Map(edited => new Part.Press(_ => edited(Edit.Of(EditPhase.Commit, Items.Filter(held => held != key)))).Drawn(new Node.Box { Role = Role.Dismiss, Align = UnitPoint.Center })).ToSeq())) {
            Role = Role.Chip,
        };
}

public sealed record Breadcrumb(Seq<Key> Path, Func<Key, Node> Face, Func<Key, IO<Unit>> Pressed) {
    private static readonly Key Folded = new Key.Named("overflow");

    public Node Drawn =>
        new Node.Choice(PhysicalAxis.Horizontal, toSeq(Range(0, int.Max(1, Path.Count - 1))).Map(hidden => (Node)new Node.Flex(PhysicalAxis.Horizontal,
            hidden is 0 ? Path.Map(Segment) : Path.Take(1).Map(Segment) + Seq<Node>(Overflow(Path.Skip(1).Take(hidden))) + Path.Skip(1 + hidden).Map(Segment)) {
            Key = new Key.Ordinal((uint)hidden),
        })) {
            Conditions = [Condition.Borderless],
        };

    private Node Segment(Key key) =>
        Face(key) switch {
            var face => new Node.Flex(PhysicalAxis.Horizontal,
                Seq(new Part.Press(_ => Pressed(key)).Drawn(face with { Role = face.Role | Some(Role.Button), Conditions = Path.Last == Some(key) ? face.Conditions.TryAdd(Condition.Selected) : face.Conditions }))
                + (Path.Last == Some(key) ? [] : Seq<Node>(new Node.Box { Role = Role.Chevron, Align = UnitPoint.Center }))) {
                Key = key,
            },
        };

    private Node.Flex Overflow(Seq<Key> hidden) =>
        new(PhysicalAxis.Horizontal, [
            Controls.Dropdown(new Node.Flex(PhysicalAxis.Horizontal, [new Node.Box { Role = Role.Overflow, Align = UnitPoint.Center }]) { Role = Role.Button },
                new Node.Flex(PhysicalAxis.Vertical, hidden.Map(key => new Part.Press(_ => Pressed(key)).Drawn(Face(key) with {
                    Key = key,
                    Role = Role.ListRow,
                    Width = Sizing.Grow.Unit,
                    Accessible = new Accessible { Role = AccessRole.MenuItem },
                }))) { Role = Role.Menu, Accessible = new Accessible { Role = AccessRole.Menu } }),
            new Node.Box { Role = Role.Chevron, Align = UnitPoint.Center },
        ]) {
            Key = Folded,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Controls {
    internal static Node Dropdown(Node face, Node content) => new Node.Overlay(face, [new Node.Layer(Some(Seq<Key>()), content) { Trigger = new Trigger.Press() }]);

    internal static Seq<Condition> Modified<T>(Option<T> rest, Option<T> value) => rest.Exists(held => Some(held) != value) ? [Condition.Modified] : [];

    internal static IO<Unit> Typed<T>(Edit<string> edit, Func<string, Fin<T>> parsed, Func<Edit<T>, IO<Unit>> edited) =>
        edit.Value.Match(
            Some: text => parsed(text).Match(Succ: value => edited(Edit.Of(edit.Phase, value)), Fail: error => when(edit.Phase == EditPhase.Commit, IO.fail<Unit>(error)).As()),
            None: () => edited(new Edit<T>(edit.Phase, None)));

    internal static Fin<float> Number(Axis axis, AxisRange domain, string text, CultureInfo culture) =>
        float.TryParse(text, NumberStyles.Float, culture, out float parsed)
            ? UIFault.Unless(axis.Contains(domain, parsed), UIReason.Range, parsed).Map(_ => parsed)
            : new UIFault(text, UIReason.Unreadable);

    internal static Node.Box Patch(Seq<Material> fills) =>
        new() { Role = Role.Field, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit, Sheet = new StyleSheet(HashMap((Role.Field, new Look { Fills = fills }))) };
}
