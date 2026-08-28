# [RASM_CONTROL]

`Rasm.Interaction` owns the one control tree. Each screen is a VALUE — a recursive closed family whose leaf cases carry exactly the payload their host widget consumes and whose container cases nest the family itself — grown bottom-up into a mount that owns every control, binding, resource, and child it created. Modality lives in role rows when cases share one payload shape, so secret text, segmented selection, and radio selection remain rows while a spinner remains its own payload-free case and an absent progress fraction selects an indeterminate bar. Capture is typed and tagged: a field addressed by tag answers a closed value union, admission runs over that value rather than over a control, and a harvest accumulates every refusal in one pass.

Both host boundaries carried a construction union and neither carried the other's cases. Rhino held the per-node identity, state, tooltip, style, and binding axis, the recursive structural band with its painted, embedded, inspector, and collection escapes, and the typed grid plan with its tree budgets; Grasshopper held the role rows collapsing modality, the policy records carrying knob sets as data, the tagged capture algebra, dispatch-free construction, and the live-view verb gate. This owner is their union at every axis, and each boundary keeps only what its host types genuinely bind: Rhino's `Rhino.UI` widget cases and section leaves become `Custom` and `Embedded` instances, Grasshopper's canvas objects and attributes stay at its canvas.

Composition is downward and sideways within the sub-domain: `Lease<T>`, `Validation`, `CapabilitySet<TCapability>`, and `ICapability<TSelf>` from `Domain`; `PerceptualColor` and `UnitInterval` from `Numerics/atoms`; `UiFault`, `UiDispatch<T>`, `DispatchLane`, and `UiThread` from `Interaction/dispatch`; `IBindingPlan`, `BindLink`, and `BindLedger` from `Interaction/binding`; `StyleKey`, `ThemePort`, and `NativeMount` from `Interaction/platform`; `SurfaceSpec` and `TypeFace` from `Interaction/paint`; `IntentKey` and `IntentTable` from `Interaction/chrome`.

## [01]-[INDEX]

- [02]-[SPEC]: `ElementKey`, `ElementState`, `ElementSpec`, `ElementRuntime`, `ControlSpec` — the per-node uniform axis and the one recursive construction family.
- [03]-[ROLES]: the six role rosters, the four capability vocabularies, the policy records, `CellSpec`, the column and grid plans, and `LayoutPlan` — every knob and modality that is data rather than a case.
- [04]-[CAPTURE]: `FieldTag`, `FieldValue`, `FieldPort` — tagged identity, the closed capture vocabulary, the direct admission function, and the accumulated harvest.
- [05]-[REALIZE]: `ControlMint`, `UiLease`, `ElementMount` — the leaf answer, the accruing one-shot release base every mount derives, and the owned subtree with affinity-gated and current-thread construction.
- [06]-[VIEW]: `ViewVerb`, `ViewEcho` — the marshalled live-view verbs and their settled evidence.

## [02]-[SPEC]

- Owner: `ElementKey` the node identity; `ElementState` the visibility-and-enablement row carrying its own seat; `ElementSpec` the uniform axis every node carries once; `ElementRuntime` the two injected registries realization reads; `ControlSpec` the one recursive construction family, thirty-one cases wide.
- Cases: a leaf band of twenty-one typed widgets, one data-view case over a typed grid plan, eight container and layout cases nesting the family, and one `Field` case tagging a capturing editor. Every family the installed construction surface carries is admitted as a case, and every MODALITY of a family is a role row on its case.
- Entry: `ElementMount.Create` is the affinity-gated public entry and `ElementMount.CreateCurrent` the dispatch-free core; neither is reachable from a spec value, so a spec stays inert data a consumer can build off the marshal.
- Auto: the per-node axis is a base positional column every case passes, so identity, state, tooltip, style, and binding plans apply exactly once per node in one place. Grasshopper had no such axis and tagged only its capturing editors, so a Grasshopper panel could not name, hide, style, or bind a container at all; that gap closes with no case added.
- Law: recursion is CASE-owned — a container case holds a `ControlSpec` field, so depth growth is absorbed locally, every consumer's generated dispatch stays total, and a new structural case breaks the construction fold at compile time. Dispatch-route recursion is the rejected placement, and the generated dispatch is depth-honest recursion bounded by the runtime stack, so hostile depth is admitted at the boundary rather than pushed through this fold.
- Law: `Field` is the ONE tagging site. An editor case outside a `Field` renders and never harvests, and a `Field` over a case with no intrinsic pick is a typed construction refusal, so "which values come back" is recoverable from the spec value alone.
- Law: `Custom` answers `ControlMint`, not a bare control, so the foreign-extension escape hands its host objects AND its beside-minted child controls to the mount in the same answer. Rhino's escape returned a control alone, which left every object and every leaf it minted around that control with no custody.
- Law: `Colour` carries `PerceptualColor` and the host colour appears only at the paint correspondence, so no consumer of this family holds a host colour value.
- Law: NO case carries a raw host mode knob. The instant family spelled two cases with an IDENTICAL column shape whose only discriminant was the presentation widget, each carrying that discriminant as a host enum; one case over `MomentRole` carries both, and the two enums live inside the rows that construct them. A knob on a case and a row on a roster are two ways to say modality, and only the row grows without a consumer edit.
- Law: NAMED LOSS — the Grasshopper `Sheet` case does not land. A property sheet over one subject IS `Inspector` with a single-subject roster, and the arity is the only thing the second case carried; what is lost is the structural guarantee of exactly one subject, which no consumer read. Witness: `new ControlSpec.Sheet(spec, subject)` becomes `new ControlSpec.Inspector(spec, Seq(subject), ShowCategories: false, Skin: new InspectorSkin.Native())`.
- Law: three cases carry ERASED payloads and each names the host that erased them. `Inspector.Subjects` and `Collection.Items` are `Seq<object>` because the host property grid and the host collection editor both address `object`, and a union case cannot introduce a type parameter its family does not carry — `Collection` therefore carries `Type Element` beside its items, the same declared-type column the transfer owner's one erased case carries. NAMED LOSS: a compile-time element check on those two cases, which neither host surface offers.
- Growth: a new widget family is one case with one construction arm; a new modality of an existing family is one role row and no consumer edits.
- Boundary: Rhino's eleven `Rhino.UI` widget cases and its section leaves seat as `Custom` and `Embedded` instances — host widgets are ROWS on this owner, never a fork of it. Grasshopper's canvas objects and attributes stay at its canvas, and its native-host case becomes `Embedded` over an eager mount.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using EtoImage = Eto.Drawing.Image;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ElementKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "ElementKey requires a non-blank identity.");
    }
}

[SmartEnum<int>]
public sealed partial class ElementState {
    public static readonly ElementState Active = new(key: 0,
        seat: static control => HostEdge.Side(() => (control.Visible, control.Enabled) = (true, true)));
    public static readonly ElementState Disabled = new(key: 1,
        seat: static control => HostEdge.Side(() => (control.Visible, control.Enabled) = (true, false)));
    public static readonly ElementState Hidden = new(key: 2,
        seat: static control => HostEdge.Side(() => (control.Visible, control.Enabled) = (false, false)));

    [UseDelegateFromConstructor] internal partial Unit Seat(Control control);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SplitSeat {
    private SplitSeat() { }
    public sealed record Pixels(int At) : SplitSeat;
    public sealed record Fraction(UnitInterval At) : SplitSeat;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InspectorSkin {
    private InspectorSkin() { }
    public sealed record Native : InspectorSkin;
    public sealed record Themed(bool Description) : InspectorSkin;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlSpec {
    private ControlSpec(ElementSpec spec) => Node = spec;

    public ElementSpec Node { get; }

    public sealed record Text(ElementSpec Spec, TextRole Role, string Seed, TextPolicy Policy) : ControlSpec(Spec);
    public sealed record Number(ElementSpec Spec, double Seed, NumberPolicy Policy) : ControlSpec(Spec);
    public sealed record Flag(ElementSpec Spec, FlagRole Role, Option<bool> Seed, string Caption) : ControlSpec(Spec);
    public sealed record Choice(ElementSpec Spec, ChoiceRole Role, Seq<string> Options, Option<int> Selected, Orientation Axis) : ControlSpec(Spec);
    public sealed record Slider(ElementSpec Spec, int Seed, int Floor, int Ceiling, SliderPolicy Policy) : ControlSpec(Spec);
    public sealed record Progress(ElementSpec Spec, Option<int> Fraction, int Floor, int Ceiling) : ControlSpec(Spec);
    public sealed record Spinner(ElementSpec Spec) : ControlSpec(Spec);
    public sealed record Colour(ElementSpec Spec, PerceptualColor Seed, bool AllowAlpha) : ControlSpec(Spec);
    public sealed record Moment(ElementSpec Spec, MomentRole Role, Option<DateTime> Seed, Option<DateTime> Floor, Option<DateTime> Ceiling) : ControlSpec(Spec);
    public sealed record Face(ElementSpec Spec, Option<TypeFace> Seed) : ControlSpec(Spec);
    public sealed record File(ElementSpec Spec, FileAction Action, Seq<FileFilter> Filters, Option<string> Seed) : ControlSpec(Spec);
    public sealed record Label(ElementSpec Spec, string Value, TextAlignment Alignment) : ControlSpec(Spec);
    public sealed record Picture(ElementSpec Spec, Lease<EtoImage> Value) : ControlSpec(Spec);
    public sealed record Separator(ElementSpec Spec, Orientation Axis, int Thickness) : ControlSpec(Spec);
    public sealed record Browser(ElementSpec Spec, Option<Uri> Home) : ControlSpec(Spec);
    public sealed record Inspector(ElementSpec Spec, Seq<object> Subjects, bool ShowCategories, InspectorSkin Skin) : ControlSpec(Spec);
    public sealed record Verb(ElementSpec Spec, ButtonRole Role, string Text, Option<IntentKey> Intent) : ControlSpec(Spec);
    public sealed record Collection(ElementSpec Spec, Seq<object> Items, Type Element, Option<ControlSpec> Extra) : ControlSpec(Spec);
    public sealed record Embedded(ElementSpec Spec, NativeMount Mount) : ControlSpec(Spec);
    public sealed record Painted(ElementSpec Spec, SurfaceSpec Surface) : ControlSpec(Spec);
    public sealed record Custom(ElementSpec Spec, Func<Fin<ControlMint>> Mint) : ControlSpec(Spec);

    public sealed record Grid(ElementSpec Spec, IGridPlan Plan) : ControlSpec(Spec);

    public sealed record Panel(ElementSpec Spec, ControlSpec Child) : ControlSpec(Spec);
    public sealed record Group(ElementSpec Spec, string Header, ControlSpec Child) : ControlSpec(Spec);
    public sealed record Expander(ElementSpec Spec, string Header, bool Expanded, Option<ControlSpec> Chrome, ControlSpec Child) : ControlSpec(Spec);
    public sealed record Scroll(ElementSpec Spec, ControlSpec Child, BorderType Border, CapabilitySet<ScrollAxis> Expand) : ControlSpec(Spec) {
        internal Fin<CapabilitySet<ScrollAxis>> Admitted => ScrollAxis.Law.Admit(held: Expand);
    }
    public sealed record Tabs(ElementSpec Spec, Seq<TabPlan> Pages, DockPosition Edge, int Selected) : ControlSpec(Spec);
    public sealed record Documents(ElementSpec Spec, Seq<DocumentPlan> Pages, bool Reorder, int Selected) : ControlSpec(Spec);
    public sealed record Split(ElementSpec Spec, ControlSpec First, ControlSpec Second, Orientation Axis, Option<SplitSeat> Seat, Option<int> Gutter, SplitterFixedPanel Fixed) : ControlSpec(Spec);
    public sealed record Layout(ElementSpec Spec, LayoutPlan Plan) : ControlSpec(Spec);

    public sealed record Field(
        ElementSpec Spec,
        FieldTag Tag,
        Option<string> Caption,
        ControlSpec Editor,
        Option<Func<FieldValue, Fin<FieldValue>>> Admit) : ControlSpec(Spec);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ElementSpec(
    ElementKey Key,
    ElementState State,
    Option<string> ToolTip,
    Option<StyleKey> Style,
    Seq<IBindingPlan> Bindings);

public sealed record ElementRuntime(ThemePort Themes, IntentTable Intents);
```

## [03]-[ROLES]

- Owner: `TextRole`, `ChoiceRole`, `FlagRole`, `ButtonRole`, `MomentRole`, and `CellKind` the six behavior rosters; `EditTrait`, `ScrollAxis`, `ColumnTrait`, and `GridTrait` the four capability vocabularies; `TextPolicy`, `NumberPolicy`, `SliderPolicy`, and `CellPolicy` the knob records; `CellSpec`, `ColumnPlan<TRow>`, `RowBudget`, `GridRows<TRow>`, and `GridPlan<TRow>` the typed data-view plan; `TabPlan`, `DocumentPlan`, `StackChild`, `TableSlot`, and `LayoutPlan` the placement vocabulary.
- Cases: `CellSpec` is `Bound` over a declarative kind row, `Custom` carrying a consumer's cell factory, and `Drawn` carrying a paint callback. `GridRows<TRow>` is `Flat` or `Tree`, the tree carrying child projection, expansion predicate, node budget, path-depth budget, and identity equality. `LayoutPlan` is `Flow`, `Rows`, `Table`, `Stack`, and `Absolute` — five host placement strategies, not one with a mode knob.
- Auto: a row's construction column composes only members the row's own widget carries, so the construction fold never probes a control type — the ROW is the dispatch. A new presentation of an existing semantic is one row; a new semantic is one `ControlSpec` case.
- Auto: the two budgets on the tree case are one value type read on two axes — a node budget caps what the whole expansion admits and a depth budget caps how far one path descends, and a linear chain inside the node budget still overflows the runtime stack, which is the one failure no result catches.
- Law: a knob set with two or more independent presence bits rides a `CapabilitySet<TCapability>` over a named vocabulary, never a bool blob. `EditTrait` replaces Grasshopper's five text bools and Rhino's access and wrap rosters; `ScrollAxis` replaces the expand-width and expand-height pair; `ColumnTrait` absorbs both column feature sets; `GridTrait` absorbs the grid chrome and selection rosters. Every corner of all four is legal, so realization reads each held set directly.
- Law: NAMED LOSS on `GridTrait` — Rhino's four named selection corners lose their names, and a reader of a plan reads a set rather than `MultipleOptional`. Bought back by the vocabulary's wire text and by growth: a fifth grid posture is one vocabulary row rather than a doubled roster. Witness: `Rhino Eto/elements.md:821 GridSelection` (four rows over two bool columns) becomes `CapabilitySet<GridTrait>.Of(GridTrait.Multiple, GridTrait.Empty)`.
- Law: the tri-state seed crosses as `HostEdge.Nullable`, the one place a `null` is a legal spelling — a host slot the domain never reads back; a hand-spelled `Match` onto `null` puts that projection at every row instead of at its one owner.
- Law: a genuinely independent single bit stays a bool and says so — `NumberPolicy.Wrap` (the value wraps at its bounds), `SliderPolicy.Snap`, `Expander.Expanded`, `Documents.Reorder`, `DocumentPlan.Closable`, and `InspectorSkin.Themed.Description` each name one axis with no legal-corner law over a second.
- Law: declarative cells are ROWS and callback-bearing cells are CASES. The discriminant is whether the cell carries a consumer callback the fold must guard: a bound cell has none and is fully described by its kind and policy, while a custom or drawn cell needs its raises routed to the plan's failure sink. Rhino spelled eight cases and Grasshopper eight rows; six of the eight were the same declarative cells on both sides.
- Law: `TextRole.Rich` reads back markup while every other text row reads plain text, so the read column is per-row rather than role-invariant. A single shared read would silently return a rich editor's plain projection and lose its formatting.
- Law: a row carries distinct behavior or identity, never a boolean already accepted by the host. `ElementState` keeps its three-state seat; colour alpha, inspector categories, stack expansion, and table-cell width scaling remain direct booleans.
- Law: the table case carries per-cell width expansion and no column or row expansion sets. NAMED LOSS: Grasshopper's stretch-column and stretch-row rosters. An expanded column IS every cell in that column expanded, which the per-cell form expresses exactly and which the set form cannot express per cell, so the per-cell form strictly contains the set form.
- Law: a `Rows` slot is `Option`-shaped, so a button row right-aligns by data through the host's flexible-space slot rather than by a spacer control.
- Law: `CellPolicy.Options` and `ColumnPlan.Read` are erased at the HOST edge and nowhere earlier — the combo cell's data store is `IEnumerable<object>` and the cell binding reads an erased value, so the plan stays typed in `TRow` to the last step and projects at item construction. NAMED LOSS: a typed option roster per cell, which would force one column plan type per value type and a heterogeneous column sequence no grid can hold.
- Packages: Eto.Forms for the widget, cell, layout, and placement rosters (registered at `libs/dotnet/.api/api-eto-forms.md` and its two boundary partitions); Thinktecture for the row vocabularies and their delegate columns; `Domain/validation` for `ICapability` and `CapabilitySet`.
- Growth: a role row, a policy field, a capability row, a legal corner, a cell case, a topology case, or a placement case — seven growth axes, each one declaration.
- Boundary: the bare stepper pair and the legacy numeric-up-down alias earn no row — the first carries no capture semantic and the second is a legacy spelling of the numeric stepper. A masked stepper hosts through `Embedded` until a typed-provider case earns its own admission.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Generic;
using Eto.Forms;
using EtoImage = Eto.Drawing.Image;
using EtoPadding = Eto.Drawing.Padding;
using EtoPoint = Eto.Drawing.Point;
using EtoSize = Eto.Drawing.Size;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
// --- [VOCABULARY]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EditTrait : ICapability<EditTrait> {
    public static readonly EditTrait Editable = new(key: "editable", rank: 0);
    public static readonly EditTrait Wrapping = new(key: "wrapping", rank: 1);
    public static readonly EditTrait Spelling = new(key: "spelling", rank: 2);
    public static readonly EditTrait Return = new(key: "return", rank: 3);
    public static readonly EditTrait Tab = new(key: "tab", rank: 4);

    public int Rank { get; }

    public static CapabilityLaw<EditTrait> Law => CapabilityLaw<EditTrait>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScrollAxis : ICapability<ScrollAxis> {
    public static readonly ScrollAxis Width = new(key: "width", rank: 0);
    public static readonly ScrollAxis Height = new(key: "height", rank: 1);

    public int Rank { get; }

    public static CapabilityLaw<ScrollAxis> Law => CapabilityLaw<ScrollAxis>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnTrait : ICapability<ColumnTrait> {
    public static readonly ColumnTrait Editable = new(key: "editable", rank: 0);
    public static readonly ColumnTrait Sortable = new(key: "sortable", rank: 1);
    public static readonly ColumnTrait Resizable = new(key: "resizable", rank: 2);
    public static readonly ColumnTrait AutoSized = new(key: "auto-sized", rank: 3);
    public static readonly ColumnTrait Hidden = new(key: "hidden", rank: 4);
    public static readonly ColumnTrait Expand = new(key: "expand", rank: 5);

    public int Rank { get; }

    public static CapabilityLaw<ColumnTrait> Law => CapabilityLaw<ColumnTrait>.Open;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridTrait : ICapability<GridTrait> {
    public static readonly GridTrait Header = new(key: "header", rank: 0);
    public static readonly GridTrait Multiple = new(key: "multiple", rank: 1);
    public static readonly GridTrait Empty = new(key: "empty", rank: 2);
    public static readonly GridTrait Reorder = new(key: "reorder", rank: 3);

    public int Rank { get; }

    public static CapabilityLaw<GridTrait> Law => CapabilityLaw<GridTrait>.Open;
}

// --- [ROLES]
[SmartEnum<int>]
public sealed partial class TextRole {
    public static readonly TextRole Plain = new(key: 0,
        mint: static (seed, policy) => {
            TextBox box = new() { Text = seed, ReadOnly = !policy.Traits.Admits(EditTrait.Editable) };
            policy.Placeholder.Iter(hint => box.PlaceholderText = hint);
            policy.MaxLength.Iter(cap => box.MaxLength = cap);
            return box;
        },
        read: static control => new FieldValue.Text(Value: control.Text));

    public static readonly TextRole Secret = new(key: 1,
        mint: static (seed, policy) => {
            PasswordBox box = new() { Text = seed };
            policy.MaxLength.Iter(cap => box.MaxLength = cap);
            return box;
        },
        read: static control => new FieldValue.Text(Value: control.Text));

    public static readonly TextRole Search = new(key: 2,
        mint: static (seed, policy) => {
            SearchBox box = new() { Text = seed, ReadOnly = !policy.Traits.Admits(EditTrait.Editable) };
            policy.Placeholder.Iter(hint => box.PlaceholderText = hint);
            return box;
        },
        read: static control => new FieldValue.Text(Value: control.Text));

    public static readonly TextRole Area = new(key: 3,
        mint: static (seed, policy) => new TextArea {
            Text = seed,
            ReadOnly = !policy.Traits.Admits(EditTrait.Editable),
            Wrap = policy.Traits.Admits(EditTrait.Wrapping),
            SpellCheck = policy.Traits.Admits(EditTrait.Spelling),
            AcceptsReturn = policy.Traits.Admits(EditTrait.Return),
            AcceptsTab = policy.Traits.Admits(EditTrait.Tab),
        },
        read: static control => new FieldValue.Text(Value: control.Text));

    public static readonly TextRole Stepped = new(key: 4,
        mint: static (seed, policy) => {
            TextStepper box = new() { Text = seed, ReadOnly = !policy.Traits.Admits(EditTrait.Editable) };
            policy.Placeholder.Iter(hint => box.PlaceholderText = hint);
            policy.MaxLength.Iter(cap => box.MaxLength = cap);
            return box;
        },
        read: static control => new FieldValue.Text(Value: control.Text));

    public static readonly TextRole Rich = new(key: 5,
        mint: static (seed, _) => new RichTextArea { Rtf = seed },
        read: static control => new FieldValue.Markup(Rtf: ((RichTextArea)control).Rtf));

    [UseDelegateFromConstructor] internal partial TextControl Mint(string seed, TextPolicy policy);
    [UseDelegateFromConstructor] internal partial FieldValue Read(TextControl control);
}

[SmartEnum<int>]
public sealed partial class ChoiceRole {
    public static readonly ChoiceRole Drop = new(key: 0,
        mint: static (options, selected, _) => Listed(new DropDown(), options, selected),
        read: static (control, options) => Picked(((ListControl)control).SelectedIndex, options));

    public static readonly ChoiceRole Combo = new(key: 1,
        mint: static (options, selected, _) => Listed(new ComboBox { AutoComplete = true }, options, selected),
        read: static (control, _) => new FieldValue.Pick(
            Index: Chosen(((ComboBox)control).SelectedIndex), Text: Some(((ComboBox)control).Text)));

    public static readonly ChoiceRole Roll = new(key: 2,
        mint: static (options, selected, _) => Listed(new ListBox(), options, selected),
        read: static (control, options) => Picked(((ListControl)control).SelectedIndex, options));

    public static readonly ChoiceRole RadioSet = new(key: 3,
        mint: static (options, selected, axis) => Listed(new RadioButtonList { Orientation = axis }, options, selected),
        read: static (control, options) => Picked(((RadioButtonList)control).SelectedIndex, options));

    public static readonly ChoiceRole Segments = new(key: 4,
        mint: static (options, selected, _) => Banded(SegmentedSelectionMode.Single, options, selected),
        read: static (control, options) => Picked(((SegmentedButton)control).SelectedIndex, options));

    public static readonly ChoiceRole CheckSet = new(key: 5,
        mint: static (options, _, axis) => new CheckBoxList { Orientation = axis, DataStore = options },
        read: static (control, _) => new FieldValue.PickSet(Keys: toSeq(((CheckBoxList)control).SelectedKeys)));

    public static readonly ChoiceRole SegmentSet = new(key: 6,
        mint: static (options, _, _) => Banded(SegmentedSelectionMode.Multiple, options, None),
        read: static (control, options) => new FieldValue.PickSet(
            Keys: toSeq(((SegmentedButton)control).SelectedIndexes).Map(index => options[index])));

    [UseDelegateFromConstructor] internal partial Control Mint(Seq<string> options, Option<int> selected, Orientation axis);
    [UseDelegateFromConstructor] internal partial FieldValue Read(Control control, Seq<string> options);

    private static ListControl Listed(ListControl control, Seq<string> options, Option<int> selected) {
        options.Iter(option => control.Items.Add(option));
        control.SelectedIndex = Seated(selected, options.Count);
        return control;
    }

    private static SegmentedButton Banded(SegmentedSelectionMode mode, Seq<string> options, Option<int> selected) {
        SegmentedButton bar = new() { SelectionMode = mode };
        options.Iter(option => bar.Items.Add(new ButtonSegmentedItem { Text = option }));
        bar.SelectedIndex = Seated(selected, options.Count);
        return bar;
    }

    private static int Seated(Option<int> selected, int count) => count == 0
        ? -1
        : selected.Match(Some: held => int.Clamp(value: held, min: 0, max: count - 1), None: static () => -1);

    private static Option<int> Chosen(int index) => index >= 0 ? Some(index) : None;

    private static FieldValue Picked(int index, Seq<string> options) => new FieldValue.Pick(
        Index: Chosen(index),
        Text: index >= 0 && index < options.Count ? Some(options[index]) : None);
}

[SmartEnum<int>]
public sealed partial class FlagRole {
    public static readonly FlagRole Binary = new(key: 0,
        mint: static (seed, caption) => new CheckBox { Text = caption, Checked = seed.IfNone(false) },
        read: static control => new FieldValue.Flag(Value: Optional(control.Checked)));

    public static readonly FlagRole Tri = new(key: 1,
        mint: static (seed, caption) => new CheckBox {
            Text = caption,
            ThreeState = true,
            Checked = HostEdge.Nullable(seed),
        },
        read: static control => new FieldValue.Flag(Value: Optional(control.Checked)));

    [UseDelegateFromConstructor] internal partial CheckBox Mint(Option<bool> seed, string caption);
    [UseDelegateFromConstructor] internal partial FieldValue Read(CheckBox control);
}

[SmartEnum<int>]
public sealed partial class ButtonRole {
    public static readonly ButtonRole Push = new(key: 0,
        mint: static (text, command) => command.Match<Control>(
            Some: static verb => new Button { Command = verb },
            None: () => new Button { Text = text }),
        pick: static _ => Option<Func<Fin<FieldValue>>>.None);

    public static readonly ButtonRole Toggle = new(key: 1,
        mint: static (text, _) => new ToggleButton { Text = text },
        pick: static control => Some<Func<Fin<FieldValue>>>(() => Fin.Succ<FieldValue>(
            new FieldValue.Flag(Value: Some(((ToggleButton)control).Checked)))));

    public static readonly ButtonRole Link = new(key: 2,
        mint: static (text, command) => command.Match<Control>(
            Some: static verb => new LinkButton { Command = verb },
            None: () => new LinkButton { Text = text }),
        pick: static _ => Option<Func<Fin<FieldValue>>>.None);

    [UseDelegateFromConstructor] internal partial Control Mint(string text, Option<Command> command);

    [UseDelegateFromConstructor] internal partial Option<Func<Fin<FieldValue>>> Pick(Control control);
}

[SmartEnum<int>]
public sealed partial class MomentRole {
    public static readonly MomentRole Date = new(key: 0,
        mint: static (seed, floor, ceiling) => Stamped(DateTimePickerMode.Date, seed, floor, ceiling),
        read: static control => new FieldValue.Stamp(Value: Optional(((DateTimePicker)control).Value)));

    public static readonly MomentRole Time = new(key: 1,
        mint: static (seed, floor, ceiling) => Stamped(DateTimePickerMode.Time, seed, floor, ceiling),
        read: static control => new FieldValue.Stamp(Value: Optional(((DateTimePicker)control).Value)));

    public static readonly MomentRole Stamp = new(key: 2,
        mint: static (seed, floor, ceiling) => Stamped(DateTimePickerMode.DateTime, seed, floor, ceiling),
        read: static control => new FieldValue.Stamp(Value: Optional(((DateTimePicker)control).Value)));

    public static readonly MomentRole Day = new(key: 3,
        mint: static (seed, floor, ceiling) => Dated(CalendarMode.Single, seed, floor, ceiling),
        read: static control => new FieldValue.Stamp(
            Value: Some(((global::Eto.Forms.Calendar)control).SelectedDate)));

    public static readonly MomentRole Range = new(key: 4,
        mint: static (seed, floor, ceiling) => Dated(CalendarMode.Range, seed, floor, ceiling),
        read: static control => Spanned((global::Eto.Forms.Calendar)control));

    [UseDelegateFromConstructor]
    internal partial Control Mint(Option<DateTime> seed, Option<DateTime> floor, Option<DateTime> ceiling);
    [UseDelegateFromConstructor] internal partial FieldValue Read(Control control);

    private static Control Stamped(
        DateTimePickerMode mode, Option<DateTime> seed, Option<DateTime> floor, Option<DateTime> ceiling) {
        DateTimePicker picker = new() { Mode = mode, Value = HostEdge.Nullable(seed) };
        floor.Iter(at => picker.MinDate = at);
        ceiling.Iter(at => picker.MaxDate = at);
        return picker;
    }

    private static Control Dated(
        CalendarMode mode, Option<DateTime> seed, Option<DateTime> floor, Option<DateTime> ceiling) {
        global::Eto.Forms.Calendar picker = new() { Mode = mode };
        seed.Iter(at => picker.SelectedDate = at);
        floor.Iter(at => picker.MinDate = at);
        ceiling.Iter(at => picker.MaxDate = at);
        return picker;
    }

    private static FieldValue Spanned(global::Eto.Forms.Calendar picker) =>
        new FieldValue.Span(Start: picker.SelectedRange.Start, End: picker.SelectedRange.End);
}

[SmartEnum]
public sealed partial class CellKind {
    public static readonly CellKind Text = new(
        mint: static (column, policy) => new TextBoxCell(column) { TextAlignment = policy.Align });
    public static readonly CellKind CheckBox = new(mint: static (column, _) => new CheckBoxCell(column));
    public static readonly CellKind ComboBox = new(
        mint: static (column, policy) => new ComboBoxCell(column) { DataStore = policy.Options });
    public static readonly CellKind Image = new(mint: static (column, _) => new ImageViewCell(column));
    public static readonly CellKind Progress = new(mint: static (column, _) => new ProgressCell(column));
    public static readonly CellKind ImageText = new(
        mint: static (column, policy) => new ImageTextCell(column, policy.Companion.IfNone(column)));

    [UseDelegateFromConstructor] internal partial Cell Mint(int column, CellPolicy policy);
}

// --- [SHAPES]
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct RowBudget {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(message: "RowBudget must be positive.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CellSpec {
    private CellSpec() { }

    public sealed record Bound(CellKind Kind, CellPolicy Policy) : CellSpec;
    public sealed record Custom(Func<CellEventArgs, Control> Create, Option<Action<CellEventArgs, Control>> Configure) : CellSpec;
    public sealed record Drawn(Action<CellPaintEventArgs> Paint) : CellSpec;

    internal Cell Mint(int column, FaultCell faults) => Switch(
        state: (Column: column, Faults: faults),
        bound: static (held, cell) => cell.Kind.Mint(column: held.Column, policy: cell.Policy),
        custom: static (held, cell) => Templated(cell, held.Faults),
        drawn: static (held, cell) => Painted(cell, held.Faults));

    private static CustomCell Templated(Custom cell, FaultCell faults);
    private static DrawableCell Painted(Drawn cell, FaultCell faults);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridRows<TRow> {
    private GridRows() { }

    public sealed record Flat(Seq<TRow> Rows) : GridRows<TRow>;
    public sealed record Tree(
        Seq<TRow> Roots,
        Func<TRow, Seq<TRow>> Children,
        Func<TRow, bool> Expanded,
        RowBudget Limit,
        RowBudget Depth,
        IEqualityComparer<TRow> Identity) : GridRows<TRow>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayoutPlan {
    private LayoutPlan() { }

    public sealed record Flow(EtoPadding Pad, EtoSize Gap, Seq<ControlSpec> Items) : LayoutPlan;
    public sealed record Rows(EtoPadding Pad, EtoSize Gap, Seq<Seq<Option<ControlSpec>>> Lines) : LayoutPlan;
    public sealed record Table(EtoPadding Pad, EtoSize Gap, Seq<Seq<TableSlot>> Lines) : LayoutPlan;
    public sealed record Stack(
        Orientation Axis,
        int Gap,
        EtoPadding Pad,
        Option<HorizontalAlignment> AlignX,
        Option<VerticalAlignment> AlignY,
        Seq<StackChild> Items) : LayoutPlan;
    public sealed record Absolute(Seq<(ControlSpec Item, EtoPoint At)> Items) : LayoutPlan;
}

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record TextPolicy(
    Option<string> Placeholder,
    Option<int> MaxLength,
    CapabilitySet<EditTrait> Traits);

public sealed record NumberPolicy(
    Option<double> Floor, Option<double> Ceiling, double Increment, int Decimals, Option<string> Format, bool Wrap);

public sealed record SliderPolicy(int Tick, bool Snap, Orientation Axis);

public sealed record CellPolicy(TextAlignment Align, Seq<object> Options, Option<int> Companion);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColumnPlan<TRow>(
    string Header,
    Func<TRow, object> Read,
    CellSpec Cell,
    CapabilitySet<ColumnTrait> Traits,
    Option<int> Width,
    Option<string> Tip);

public sealed record TabPlan(string Title, Option<Lease<EtoImage>> Badge, ControlSpec Body);

public sealed record DocumentPlan(string Title, bool Closable, Option<Lease<EtoImage>> Badge, ControlSpec Body);

public sealed record StackChild(ControlSpec Item, bool Expand);

public sealed record TableSlot(Option<ControlSpec> Item, bool Expand);

// --- [SERVICES] ------------------------------------------------------------------------
public interface IGridPlan {
    Fin<ControlMint> Realize();
    Seq<IsolatedFault> Failures { get; }
}

public sealed class GridPlan<TRow>(
    Seq<ColumnPlan<TRow>> columns,
    GridRows<TRow> rows,
    CapabilitySet<GridTrait> traits,
    GridLines lines,
    BorderType border,
    Option<int> rowHeight,
    FaultCell faults) : IGridPlan {
    public Seq<ColumnPlan<TRow>> Columns { get; } = columns;
    public GridRows<TRow> Rows { get; } = rows;
    public CapabilitySet<GridTrait> Traits { get; } = traits;

    public Seq<IsolatedFault> Failures => faults.Parked;

    public Fin<ControlMint> Realize();
}
```

## [04]-[CAPTURE]

- Owner: `FieldTag` the semantic field identity; `FieldValue` the one typed capture vocabulary; `FieldPort` one realized tag-to-pick binding carrying its admission function directly.
- Cases: eleven capture shapes — plain text, markup, number, tri-state flag, single pick with its index and text, multi pick, colour, stamp, date span, path, and typeface. No object-valued capture and no per-consumer cast survives.
- Entry: `ElementMount.Harvest` answers `Fin<HashMap<FieldTag, FieldValue>>`; success carries every admitted field and failure carries every independent refusal.
- Auto: admission functions run inside the harvest after the typed pick, so they see a `FieldValue` and never a control.
- Auto: independent refusals accumulate through the `Validation` applicative, so a six-field surface reports all six faults in one pass. A first-defect bind chain would send a user back six times for one form.
- Law: admission is a pure function stored directly on the port, so the harvest reads ONE roster rather than joining ports against the spec tree it already walked.
- Law: no capture value spells absence as a sentinel. An unselected choice carries an absent ordinal beside its free text, an unanswered tri-state flag carries an absent bool, and an unset stamp carries an absent instant — the host's negative-ordinal and nullable encodings are admitted once at the role row that reads them.
- Law: colour captures as `PerceptualColor` and typeface as the paint owner's `TypeFace`, so a captured value carries the kernel's own colour and type identity rather than a host struct a consumer must convert.
- Law: a pick never raises — every pick closure runs under the operation's catch inside the harvest, so a released or host-rejected read lands as a typed refusal rather than an exception crossing the event pump.
- Output: the tag-keyed value map; the accumulated refusal is the failure arm, never a partial map.
- Packages: LanguageExt.Core for `Validation`, `Fin`, `HashMap`, and `Seq`.
- Growth: a new capture shape is one `FieldValue` case with the pick arm on the owning role row or case; the port shape never widens.
- Boundary: Rhino carried NO capture algebra — values left only through bindings, so a modal that never bound could not read its own fields. Both boundaries gain this owner whole.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct FieldTag {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "FieldTag requires a non-blank identity.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldValue {
    private FieldValue() { }

    public sealed record Text(string Value) : FieldValue;
    public sealed record Markup(string Rtf) : FieldValue;
    public sealed record Number(double Value) : FieldValue;
    public sealed record Flag(Option<bool> Value) : FieldValue;
    public sealed record Pick(Option<int> Index, Option<string> Text) : FieldValue;
    public sealed record PickSet(Seq<string> Keys) : FieldValue;
    public sealed record Colour(PerceptualColor Value) : FieldValue;
    public sealed record Stamp(Option<DateTime> Value) : FieldValue;
    public sealed record Span(DateTime Start, DateTime End) : FieldValue;
    public sealed record Path(Option<string> Value) : FieldValue;
    public sealed record Face(TypeFace Value) : FieldValue;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldPort(
    FieldTag Tag,
    Func<Fin<FieldValue>> Pick,
    Option<Func<FieldValue, Fin<FieldValue>>> Admit);

```

## [05]-[REALIZE]

- Owner: `ControlMint` the leaf's answer — its control, the host objects it minted beside that control, the child mints it minted inside it, and its intrinsic pick; `UiLease` the accruing one-shot release base (guarded transition + fault ledger + reverse drain) every mount derives; `ElementMount` the owned subtree and its construction entries.
- Entry: `ElementMount.Create` is the affinity-gated public entry answering a leased mount, while `ElementMount.CreateCurrent` is the dispatch-free core a presenting owner calls inside its own marshal window. Callers already holding the window call the core; a caller that does not calls the gate. Every boundary mount DERIVES `UiLease` and accrues its teardown arms at mint — a hand one-shot latch, fault list, or reverse loop beside the base is the deleted form.
- Auto: the gate admits the application through the published affinity probe rather than inlining a thread test, so a headless process refuses `KernelFault.MissingContext` and a worker thread refuses `UiFault.OffThread` — two outcomes an inlined boolean test collapses into one.
- Auto: the tag registry is INJECTIVE by construction — the seal refuses a duplicate tag, so a realized subtree's ports address exactly one editor each and a harvest cannot silently drop a shadowed field.
- Auto: `Create` DRAINS the grid plan's parked refusals onto the mount's own teardown ledger, so a cell factory's raise routed into the plan's sink reaches a reader; a sink no surface drains is evidence nothing can act on.
- Auto: construction brackets its own unwind. Leaves raising after minting host objects release them and the child mints it had already made, a child roster that fails mid-fold releases the mounts it already gathered in reverse order, and a binding roster that fails releases the links it already rigged — so a partial realization leaks no native handle.
- Law: teardown is reverse-order and ALL-ATTEMPTED — bindings, then child mounts, then the mint's own child mints, then resources, then the host if this mount owns it, every step running even when an earlier one refuses, and every refusal accumulating on the mount's own ledger. Teardown faults never ride the unwinding stack, because disposal fires from a `finally` where a raise REPLACES the primary exception.
- Law: a leaf's BESIDE-minted controls are child MINTS, never anonymous resources. Widget families building one control per row inside its own layout — an image button per button row, a per-item editor inside a collection — hands each back as a `ControlMint` carrying its own resources and its own intrinsic pick, and the mount drains those children in reverse mint order before releasing the leaf that contains them. Erasing them into `Resources` as `Lease<IDisposable>` loses each child's pick and inverts nothing about the order, which is exactly how a boundary widget row lost both its capture and its teardown reach. NAMED LOSS: none — a child mint with no pick and no resources is the resource entry it replaces.
- Law: host ownership is a `Lease` CASE, not a flag. A painted node borrows the control its surface owns and an embedded node borrows the control its mount owns, while every minted leaf owns its own; the two custody rosters both boundaries carried collapse into the one resource lease.
- Law: realization threads its own key derived from the node identity, so a binding fault is keyed to the element that produced it rather than to the realize call that spanned the whole tree.
- Law: the construction fold is dispatch-free and the view verbs alone marshal themselves, so construction, presentation, and capture share ONE marshal window at the presenter and a spec row never crosses on its own.
- Output: `ElementMount` exposes the host control, the port roster, and the accumulated release faults; `Harvest` answers the admitted field map and `Execute` answers a view echo.
- Packages: Eto.Forms for the construction surface; LanguageExt.Core for `Fin`, `Validation`, `Lease`, `Seq`; `Interaction/dispatch` for the probe, the crossing, and the `FaultGate` every host raise on this page funnels through — this owner mints no second isolation gate.
- Growth: a new spec case is one construction arm breaking loudly; a new custody shape is a resource-lease case, not a mount column.
- Boundary: NAMED LOSS — Grasshopper's dispatch-free construction and Rhino's affinity-gated construction become ONE owner with two members rather than two entries with two contracts. Neither guarantee is erased: the core still marshals nothing and the gate still refuses off-thread. Witness: `GH Eto/controls.md:301` (the dispatch-free law) against `Rhino Eto/elements.md:596` (the off-thread refusal).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Rasm.Domain;

namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ControlMint(
    Lease<Control> Host,
    Seq<Lease<IDisposable>> Resources = default,
    Seq<ControlMint> Children = default,
    Option<Func<Fin<FieldValue>>> Pick = default) {
    internal Fin<Unit> Drain();
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class UiLease : IMount, IDisposable {
    protected UiLease();
    public Seq<Error> ReleaseFaults { get; }
    protected Fin<Unit> Accrue(Func<Fin<Unit>> arm);
    public Fin<Unit> Release();
    public void Dispose();
}

public sealed class ElementMount : UiLease {
    private readonly Seq<Lease<BindLink>> bindings;
    private readonly Seq<ElementMount> children;
    private readonly Seq<ControlMint> minted;
    private readonly Seq<Lease<IDisposable>> resources;
    private readonly Lease<Control> host;

    public Control Host { get; }
    public Seq<FieldPort> Ports { get; }

    public static Fin<Lease<ElementMount>> Create(ControlSpec spec, ElementRuntime runtime);

    public static Fin<ElementMount> CreateCurrent(ControlSpec spec, ElementRuntime runtime);

    internal static Fin<ElementMount> Create(
        ControlMint mint, ElementSpec spec, ElementRuntime runtime, Seq<ElementMount> children);

    internal static Fin<ElementMount> Mint(
        Func<Fin<ControlMint>> mint, ElementSpec spec, ElementRuntime runtime, Seq<ElementMount> children);

    internal static Fin<Seq<ElementMount>> Gather(Seq<ControlSpec> nodes, ElementRuntime runtime);

    public Fin<HashMap<FieldTag, FieldValue>> Harvest();

    public Fin<ViewEcho> Execute(ViewVerb verb);
}

```

## [06]-[VIEW]

- Owner: `ViewVerb` the marshalled live-view vocabulary and `ViewEcho` its settled evidence.
- Cases: nine verbs — select and unselect one row, select and unselect all, begin an inline edit, commit it, cancel it, reveal a row, and reload a tree scope — against two echoes carrying editing state and commit acceptance.
- Entry: `ElementMount.Execute` is the ONE verb gate and resolves the mounted host as its target.
- Auto: the verbs marshal themselves — alone on this page — because they mutate live view state outside any presentation window, while construction and capture stay inside the presenter's one crossing.
- Law: the tree-only reload refuses TYPED on a flat view rather than downcasting, so the caller receives a refusal instead of a host raise.
- Output: one `ViewEcho` per executed verb, settled rather than a live read.
- Packages: Eto.Forms for the grid and tree verb surface (registered at `libs/dotnet/.api/api-eto-forms.md`); `Interaction/dispatch` for the crossing.
- Growth: a new verb is one case with one execute arm; a new echo rides its verb's case.
- Boundary: cell-edit and selection event streams, calendar raises, and document-page lifecycle are the input owner's source rows observed on the realized control, never construction state.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Eto.Forms;
using Thinktecture;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewVerb {
    private ViewVerb() { }

    public sealed record Select(int Row) : ViewVerb;
    public sealed record Unselect(int Row) : ViewVerb;
    public sealed record SelectAll : ViewVerb;
    public sealed record UnselectAll : ViewVerb;
    public sealed record Edit(int Row, int Column) : ViewVerb;
    public sealed record Commit : ViewVerb;
    public sealed record Cancel : ViewVerb;
    public sealed record Reveal(int Row) : ViewVerb;
    public sealed record Reload(Option<(ITreeGridItem Item, bool Children)> Target) : ViewVerb;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewEcho {
    private ViewEcho() { }

    public sealed record Settled(bool Editing) : ViewEcho;
    public sealed record Committed(bool Accepted) : ViewEcho;
}

```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
