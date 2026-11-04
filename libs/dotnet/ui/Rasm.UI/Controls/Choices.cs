using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Topten.RichTextKit;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ToggleKind {
    public static readonly ToggleKind CheckBox = new(Roles.Label, AccessRole.CheckBox,
        static (toggle, state) => Seq<View>(new View.Flex(PhysicalAxis.Horizontal, [new View.Box { Role = Roles.Checkmark, Conditions = state }]) { Role = Roles.Toggle, Conditions = state, Align = UnitPoint.Center }) + toggle.Content);

    public static readonly ToggleKind Switch = new(Roles.Label, AccessRole.Switch, static (toggle, state) => toggle.Content.Add(toggle.Thumbed(state)));

    public static readonly ToggleKind Button = new(Roles.Button, AccessRole.CheckBox, static (toggle, _) => toggle.Content);

    internal Key Row { get; }
    internal AccessRole Access { get; }

    [UseDelegateFromConstructor]
    internal partial Seq<View> Faced(Toggle toggle, HashSet<Condition> state);
}

public sealed record Toggle(Check Value, Func<Edit<Check>, IO<Unit>> Edited) {
    private static readonly View Spacer = new View.Box { Role = Roles.Handle, Opacity = Fraction.MinValue };

    public Toggle(Option<bool> value, Func<Edit<bool>, IO<Unit>> edited) : this(Check.Of(value), edit => edited(edit.Map(static check => check == Check.On).As())) { }

    public Seq<View> Content { get; init; }
    public ToggleKind Kind { get; init; } = ToggleKind.CheckBox;
    public Option<Check> Rest { get; init; }

    public View Drawn {
        get {
            HashSet<Condition> state = toHashSet(Value.Conditioned.ToSeq());
            return new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, Value.Map(off: Check.On, on: Check.Off, mixed: Check.On)))) {
                Reset = Edit.Restoring(Rest, Some(Value)).Map(rest => Edited(Edit.Of(EditPhase.Commit, rest))),
            }.Drawn(new View.Flex(PhysicalAxis.Horizontal, Kind.Faced(this, state)) {
                Role = Kind.Row,
                Conditions = state.Union(Edit.Marked(Rest, Some(Value))),
                Accessible = new Accessible { Role = Kind.Access },
            });
        }
    }

    internal static View.Box Glyphed(Key glyph, Check check) => new() { Role = glyph, Conditions = check == Check.Off ? [Condition.Disabled] : [] };

    internal View.Overlay Thumbed(HashSet<Condition> state) =>
        new(new View.Flex(PhysicalAxis.Horizontal, Seq(Spacer, Spacer)), [
            new View.Box { Role = Roles.Handle, Conditions = state, Align = new UnitPoint(Value.Map(off: Fraction.MinValue, on: Fraction.MaxValue, mixed: Fraction.Half), Fraction.Half) },
        ]) {
            Role = Roles.Switch,
            Conditions = state,
            Align = UnitPoint.Center,
        };
}

public sealed record Picker(Seq<Key> Items, Option<Key> Value, Func<Key, string> Text, Func<Edit<Key>, IO<Unit>> Edited) {
    public Option<Func<Key, View>> Face { get; init; }
    public Option<TextField> Filter { get; init; }
    public Option<Key> Rest { get; init; }

    public View Drawn(CultureInfo culture) {
        HashSet<Condition> marks = Edit.Marked(Rest, Value);
        Search query = Search.Of(Filter);
        Option<Sizing> share = Items.IsEmpty ? None : Some<Sizing>(new Sizing.Percent(new Fraction(1f / Items.Count)));
        View listed = new View.Flex(PhysicalAxis.Vertical, Filter.Map(static field => field.Drawn).ToSeq().Add(new View.Scroll(PhysicalAxis.Vertical,
            Choosing(Items.Filter(key => query.Matches(Text(key), culture))).Drawn(
                key => Faced(key, query, TextAlignment.Auto, culture) with { Width = Sizing.Grow.Unit },
                static rows => new View.Flex(PhysicalAxis.Vertical, rows) { Width = Sizing.Grow.Unit })))) { Role = Roles.Menu };
        View face = new View.Flex(PhysicalAxis.Horizontal, [
            Value.Map(key => Faced(key, Search.Everything, TextAlignment.Auto, culture)).IfNone(static () => new View.Box { Role = Roles.Checkmark, Conditions = [Condition.Mixed], Align = UnitPoint.Center }),
            new View.Box { Width = Sizing.Grow.Unit },
            new View.Box { Role = Roles.Chevron, Conditions = [Condition.Selected], Align = UnitPoint.Center },
        ]) { Role = Roles.Button, Width = Sizing.Grow.Unit, Conditions = marks };
        View dropdown = Dropdown(face, listed) with {
            Width = Sizing.Grow.Unit,
            Part = Some<Part>(Choosing(Items)),
            Accessible = new Accessible { Value = Value.Map(key => (AccessValue)new AccessValue.Text(Text(key))) },
        };
        return Filter.IsSome
            ? dropdown
            : new View.Choice(PhysicalAxis.Horizontal, [
                Choosing(Items).Drawn(
                    key => new View.Flex(PhysicalAxis.Horizontal, [Faced(key, Search.Everything, TextAlignment.Center, culture) with { Width = Sizing.Grow.Unit }]) { Role = Roles.Button, Width = share },
                    static segments => new View.Flex(PhysicalAxis.Horizontal, segments) { Role = Roles.Segment }) with {
                    Conditions = marks,
                },
                dropdown,
            ]) { Width = Sizing.Grow.Unit };
    }

    internal static View Dropdown(View face, View content) => new View.Overlay(face, [new View.Layer(content) { Trigger = new Trigger.Press() }]);

    private Part.Choice Choosing(Seq<Key> items) => new(items, Value, Edited) { Rest = Rest.Map(static rest => (Selection)new Selection.One(Some(rest))) };

    private View Faced(Key key, Search query, TextAlignment alignment, CultureInfo culture) =>
        Face.Map(face => face(key)).IfNone(() => new View.Text(query.Marked(Text(key), culture)) { MaxLines = 1, Alignment = alignment, Align = UnitPoint.Center });
}

public sealed record Tokens(Seq<Key> Items, Func<Key, View> Face, Selection Selection, Func<Edit<Selection>, IO<Unit>> Selected) {
    public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Edited { get; init; }
    public Option<TextField> Entry { get; init; }

    public View Drawn =>
        new View.Flex(PhysicalAxis.Horizontal,
            Seq(new Part.Choice(Items, Selection, Selected) { Reorder = Edited, Removed = Edited.Map(edited => Part.Choice.Without(Items, edited)) } switch {
                var choice => choice.Drawn(key => Chip(choice, key), static chips => new View.Flex(PhysicalAxis.Horizontal, chips) { Wraps = true }),
            })
            + Entry.Map(static entry => entry.Drawn.Conditioned(Condition.Borderless)).ToSeq()) {
            Role = Roles.Field,
            Width = Sizing.Grow.Unit,
            Wraps = true,
        };

    private View.Flex Chip(Part.Choice choice, Key key) => new(PhysicalAxis.Horizontal, Face(key).Cons(choice.Dismiss(key).ToSeq())) { Role = Roles.Chip };
}

public sealed record Preset<T>(Key Key, string Label, T Value) where T : notnull;

public sealed record RangePicker(Axis Axis, Seq<Preset<AxisRange>> Presets, Func<Edit<AxisRange>, IO<Unit>> Edited) {
    public View Drawn(CultureInfo culture) {
        AxisRange range = Axis.Domain;
        Option<Preset<AxisRange>> chosen = Presets.Chosen(range);
        View face = new View.Flex(PhysicalAxis.Horizontal,
            chosen.Match(
                Some: static preset => Seq<View>(new View.Text([preset.Label]) { MaxLines = 1 }),
                None: () => Seq<View>(Bound(range.From, culture), new View.Box { Role = Roles.Chevron }, Bound(range.To, culture)))
            + (Presets.IsEmpty ? [] : Seq<View>(new View.Box { Width = Sizing.Grow.Unit }, new View.Box { Role = Roles.Chevron, Conditions = [Condition.Selected] }))) {
            Role = Roles.Button,
            Width = Sizing.Grow.Unit,
        };
        View listed = new Part.Choice(Presets.Map(static preset => preset.Key), chosen.Map(static preset => preset.Key), edit => Edited(Presets.Applied(edit)))
            .Drawn(Presets.Map(static preset => (View)new View.Text([preset.Label]) { MaxLines = 1, Width = Sizing.Grow.Unit }), static rows => new View.Flex(PhysicalAxis.Vertical, rows) { Role = Roles.Menu, Width = Sizing.Grow.Unit });
        return new View.Flex(PhysicalAxis.Horizontal, [Shift(-1f, culture), Presets.IsEmpty ? face : Picker.Dropdown(face, listed) with { Width = Sizing.Grow.Unit }, Shift(1f, culture)]) {
            Role = Roles.Segment,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Spoken(range, culture) },
        };
    }

    private View Shift(float sign, CultureInfo culture) {
        View face = new View.Flex(PhysicalAxis.Horizontal, [
            new View.Box { Role = Roles.Chevron, Align = UnitPoint.Center, Transform = sign < 0f ? Some(Matrix4x4.CreateScale(-1f, 1f, 1f)) : None },
        ]) {
            Role = Roles.Button,
        };
        return Axis.Domain.Shifted(sign * Axis.Domain.Span).Match(
            Some: target => new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, target))).Drawn(face with { Accessible = new Accessible { Label = Spoken(target, culture) } }),
            None: () => face.Conditioned(Condition.Disabled));
    }

    private View.Text Bound(float value, CultureInfo culture) => new([Axis.Spoken(value, culture)]) { MaxLines = 1 };

    private string Spoken(AxisRange range, CultureInfo culture) => culture.TextInfo.Listed([Axis.Spoken(range.From, culture), Axis.Spoken(range.To, culture)]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PresetExtensions {
    extension<T>(Seq<Preset<T>> presets) where T : notnull {
        internal Option<Preset<T>> Chosen(T value) => presets.Find(preset => preset.Value.Equals(value));

        internal Edit<T> Applied(Edit<Key> edit) => edit.Choose(key => presets.Find(preset => preset.Key == key).Map(static preset => preset.Value));
    }
}
