# [RASM_RHINO_ETO_ELEMENTS]

The typed element-realization algebra of `Rasm.Rhino.Eto` — one closed `Element` tree whose cases span the full current `Eto.Forms` control roster, one `Realize` dispatch that mints every native control, and one layout algebra (`Arrangement` + `FlowRegion`) that absorbs `DynamicLayout`, `TableLayout`, `StackLayout`, and `PixelLayout` construction as recursive rows. A screen is a value: element cases carry kind rows, policy records, and bind attachments; realization is one total fold from that value to a live control tree. The page also mints `UiFault`, the one failure vocabulary every page in this sub-domain fails through, and `ElementSpec`, the uniform identity/enablement/style/bind spine every realized control passes. Per-control public factories, a runtime-type event switch, and hand-wired child adds are the census-era forms this owner deletes: value channels ride `binding.md` attachments over the host `*Binding` properties, commands ride `chrome.md` intent rows, and a new control kind is one enum row plus one union case, never a new factory.

Realization is UI-thread work — consumers enter through the `runtime.md` dispatch owner and hand `Realize` an already-marshalled frame; this page never self-dispatches. Every host construction is bracketed by `Op.Catch`, so a throwing handler surfaces as a typed fault, never an unhandled host exception.

## [01]-[INDEX]

- [02]-[FAULT_FAMILY]: `UiFault` — the closed `Expected`-derived failure union of the Eto sub-domain: dismissal, capability absence, thread affinity, admission rejection, absent payload, and captured host refusal.
- [03]-[SPEC_FLOOR]: `ElementKey` `[ValueObject<string>]` + `ElementSpec` — the uniform identity, tooltip, enablement, visibility, style, and bind-attachment spine applied to every realized control by one fold.
- [04]-[CONTROL_ROWS]: `TextKind` · `ChoiceKind` · `ScalarKind` · `PickKind` · `PressKind` · `StaticKind` · `BoxKind` — the kind vocabularies whose `[UseDelegateFromConstructor]` mint columns construct the roster, with `TextPolicy`/`ChoicePolicy`/`ScalarPolicy`/`PickPolicy` the per-family policy records.
- [05]-[ELEMENT_TREE]: `Element` — the recursive closed screen tree over every kind family, container, split, tab, grid, painted, embedded, and web case, realized by ONE total `Realize(Op?)` dispatch.
- [06]-[GRID_FAMILY]: `CellRow` + `ColumnPlan` + `RowSeed` + `GridPlan` — the grid/tree-grid/tree construction family over `GridView`/`TreeGridView`/`TreeView` with the full host cell roster and `GridChrome` policy.
- [07]-[LAYOUT_ALGEBRA]: `Arrangement` + `FlowRegion` — the four host layout strategies as one recursive union: flow regions, scaling grid, linear run, absolute placement.

## [02]-[FAULT_FAMILY]

- Owner: `UiFault` — the one `[Union]` deriving from the kernel `Expected` bridge that every fallible surface in `Rasm.Rhino.Eto` fails through. Cases are failure carriers keyed by the raising `Op` payload, so the union carries no `[GenerateUnionOps]`; recovery predicates match on the case and `Category`, never rendered text. The kernel-substrate `Fault` stays the geometry rail — a UI failure is a `UiFault` case, and both are already `Error`, so a page composing kernel admission beside UI presentation converts nothing.
- Cases: `Dismissed(Op)` (Dismissed — a modal closed without an affirmative result) · `Unavailable(Op, string Capability)` (Capability — a platform handler or feature the backend does not carry) · `OffThread(Op)` (Thread — a control-tree touch attempted off the UI thread) · `Rejected(Op, string Field, string Reason)` (Admission — a bind-channel value the admission delegate refused) · `AbsentPayload(string Mime)` (Transfer — a clipboard or drag payload absent under its MIME key) · `HostRejected(Op, string Detail)` (Host — a captured host exception, detail preserved) — six cases, each rendering its own `Message`.
- Law: `HostRejected` is minted only by the one capture funnel — `Op.Catch` captures the throwing host body and its failure re-maps once at the raising seam; a bare `try`/`catch` or a second exception funnel anywhere in the sub-domain is the deleted form.
- Growth: a new UI failure is one case with its typed payload and `Category`; a parallel error type or a stringly `Error.New` in UI flow is the rejected form.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Rhino.Eto;

// --- [ERRORS] -------------------------------------------------------------------------------
[Union]
public abstract partial record UiFault : Expected {
    private UiFault() : base() { }
    public sealed partial record Dismissed(Op Key) : UiFault { public override string Message => $"UI operation '{Key}' was dismissed without a result."; public override string Category => "Dismissed"; }
    public sealed partial record Unavailable(Op Key, string Capability) : UiFault { public override string Message => $"UI operation '{Key}' requires host capability '{Capability}'."; public override string Category => "Capability"; }
    public sealed partial record OffThread(Op Key) : UiFault { public override string Message => $"UI operation '{Key}' touched the control tree off the UI thread."; public override string Category => "Thread"; }
    public sealed partial record Rejected(Op Key, string Field, string Reason) : UiFault { public override string Message => $"UI operation '{Key}' rejected field '{Field}': {Reason}"; public override string Category => "Admission"; }
    public sealed partial record AbsentPayload(string Mime) : UiFault { public override string Message => $"Transfer payload is absent under MIME type '{Mime}'."; public override string Category => "Transfer"; }
    public sealed partial record HostRejected(Op Key, string Detail) : UiFault { public override string Message => $"UI operation '{Key}' was refused by the host: {Detail}"; public override string Category => "Host"; }
}
```

## [03]-[SPEC_FLOOR]

- Owner: `ElementKey` `[ValueObject<string>]` — ordinal identity of one screen element, the key bind receipts, style rows, and automation identity all derive from — and `ElementSpec`, the uniform spine every realized control passes through ONE `Apply` fold: tooltip, enablement, visibility, the `platform.md` `StyleKey`, and the `binding.md` `BindAttachment` rows. A per-control property scatter or a decorator sibling that re-applies common state is the deleted form; `Apply` is the single site that writes `Control.Enabled`, `Control.Visible`, `Control.ToolTip`, and `Widget.Style`.
- Entry: `ElementSpec.Of(ElementKey)` mints the minimal spec; `with` composes tooltip, style, and binds; `Apply(Control, Op)` folds the spec onto a freshly minted control, traversing every attachment and returning the accumulated `Seq<BindReceipt>` so a screen's realization receipt carries every wired channel.
- Law: bind attachments are the ONLY value channel — the census-era `TextChanged`/`CheckedChanged`/`ValueChanged`/`SelectedIndexChanged` runtime-type event switch is deleted; `binding.md` rows ride the host `TextBinding`/`CheckedBinding`/`ValueBinding`/`SelectedIndexBinding` properties, and an element that needs a value channel declares an attachment row, never an event handler.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Domain (project — `Op`, `Op.Catch`), Eto.Forms (host — `Control.Enabled`/`Visible`/`ToolTip`, `Widget.Style`).
- Growth: a new uniform axis (automation id, context menu row, drop admission) is one `ElementSpec` field consumed inside `Apply` — every element case gains it with zero case edits.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ElementKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value: value) ? new ValidationError(message: "ElementKey requires a non-whitespace identity.") : null;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ElementSpec(
    ElementKey Key,
    Option<string> ToolTip,
    bool Enabled,
    bool Visible,
    Option<StyleKey> Style,
    Seq<BindAttachment> Binds) {
    public static ElementSpec Of(ElementKey key) => new(Key: key, ToolTip: None, Enabled: true, Visible: true, Style: None, Binds: Seq<BindAttachment>());
    internal Fin<Seq<BindReceipt>> Apply(Control control, Op key) =>
        key.Catch(() => {
            control.Enabled = Enabled;
            control.Visible = Visible;
            _ = ToolTip.Iter(tip => control.ToolTip = tip);
            _ = Style.Iter(row => control.Style = row.Value);
            return Fin.Succ(value: unit);
        }).Bind(_ => Binds.TraverseM(attachment => attachment.Wire(control)).As().Map(static receipts => receipts.Strict()));
}
```

## [04]-[CONTROL_ROWS]

- Owner: the seven kind vocabularies — `TextKind`, `ChoiceKind`, `ScalarKind`, `PickKind`, `PressKind`, `StaticKind`, `BoxKind` — each a `[SmartEnum<int>]` whose `[UseDelegateFromConstructor]` `Mint` column constructs the concrete host control from the family's policy record, so the full roster is rows on seven declarations and a new control is one row. The census-era per-control public factories (`text`, `check`, `number`, `choice` and kin) are deleted: variation lives in the row, policy lives in the record, and the family's shape is fixed by the payload the controls genuinely share.
- Cases: `TextKind` `Line`(`TextBox`) · `Area`(`TextArea`) · `Rich`(`RichTextArea`) · `Secret`(`PasswordBox`) · `Search`(`SearchBox`) — all `TextControl`-derived, so `Text` and `TextChanged` ride the base; `ChoiceKind` `Drop`(`DropDown`) · `Combo`(`ComboBox`) · `Roster`(`ListBox`) · `CheckList`(`CheckBoxList`) · `Segmented`(`SegmentedButton`) · `RadioBank`(controller-chained `RadioButton` run); `ScalarKind` `Stepper`(`NumericStepper`) · `Track`(`Slider`) · `Meter`(`ProgressBar`) · `Busy`(`Spinner`); `PickKind` `Moment`(`DateTimePicker`) · `Month`(`Calendar`) · `Pigment`(`ColorPicker`) · `Face`(`FontPicker`) · `PathPick`(`FilePicker`); `PressKind` `Push`(`Button`) · `Link`(`LinkButton`) — both rows carry a `Wire` column subscribing their own `Click`, so press wiring is a row fact, never a runtime type switch; `StaticKind` `Caption`(`Label`) · `Picture`(`ImageView`); `BoxKind` `Plain`(`Panel`) · `Framed`(`GroupBox`) · `Folding`(`Expander`) · `Scrolling`(`Scrollable`).
- Law: policy records carry the full family axis — `TextPolicy` (placeholder, read-only, max length, alignment), `ChoicePolicy` (entries, initial index, orientation for the check list, selection mode for the segmented bar), `ScalarPolicy` (bounds, increment, decimal places, tick/snap, orientation, indeterminate), `PickPolicy` (date window, picker mode, alpha admission, file action and filters) — and each row applies only the members its control admits; a policy knob that re-describes the row is the rejected form, because the row IS the modality.
- Law: `RadioBank` is the one composed row — `RadioButton(RadioButton controller)` chains mutual exclusion through the first-minted controller, and the bank realizes as one `StackLayout` run, so radio-group invariants are construction facts, never synchronization code.
- Boundary: colors, fonts, and images inside policies stay host-typed at this seam only where the host control demands its own value (`FontPicker.Value`, `ImageView.Image`); every paint-adjacent color crosses as the kernel `PerceptualColor` and quantizes at the `canvas.md` edge.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record TextPolicy(Option<string> Placeholder, bool ReadOnly, Option<int> MaxLength, TextAlignment Alignment = TextAlignment.Left) {
    public static readonly TextPolicy Free = new(Placeholder: None, ReadOnly: false, MaxLength: None);
}

public sealed record ChoicePolicy(Seq<string> Entries, Option<int> InitialIndex, Orientation Flow = Orientation.Vertical, SegmentedSelectionMode Segments = SegmentedSelectionMode.Single) {
    public static ChoicePolicy Of(params ReadOnlySpan<string> entries) => new(Entries: toSeq(entries.ToArray()), InitialIndex: None);
}

public sealed record ScalarPolicy(double Floor, double Ceiling, double Increment, int DecimalPlaces, Option<int> Tick, Orientation Axis = Orientation.Horizontal, bool Indeterminate = false) {
    public static ScalarPolicy Unit(double increment = 0.01) => new(Floor: 0.0, Ceiling: 1.0, Increment: increment, DecimalPlaces: 2, Tick: None);
}

public sealed record PickPolicy(Option<DateTime> Earliest, Option<DateTime> Latest, DateTimePickerMode Moment = DateTimePickerMode.Date, CalendarMode Span = CalendarMode.Single, bool AllowAlpha = true, FileAction FileAction = FileAction.OpenFile, Seq<FileFilter> Filters = default) {
    public static readonly PickPolicy Default = new(Earliest: None, Latest: None);
}

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextKind {
    public static readonly TextKind Line = new(key: 0, mint: static policy => Configure(new TextBox { ReadOnly = policy.ReadOnly, TextAlignment = policy.Alignment }, policy));
    public static readonly TextKind Area = new(key: 1, mint: static policy => new TextArea { ReadOnly = policy.ReadOnly });
    public static readonly TextKind Rich = new(key: 2, mint: static policy => new RichTextArea { ReadOnly = policy.ReadOnly });
    public static readonly TextKind Secret = new(key: 3, mint: static policy => policy.MaxLength.Match(Some: max => new PasswordBox { ReadOnly = policy.ReadOnly, MaxLength = max }, None: () => new PasswordBox { ReadOnly = policy.ReadOnly }));
    public static readonly TextKind Search = new(key: 4, mint: static policy => Configure(new SearchBox { ReadOnly = policy.ReadOnly, TextAlignment = policy.Alignment }, policy));
    [UseDelegateFromConstructor]
    internal partial Control Mint(TextPolicy policy);
    private static TextBox Configure(TextBox box, TextPolicy policy) {
        _ = policy.Placeholder.Iter(hint => box.PlaceholderText = hint);
        _ = policy.MaxLength.Iter(max => box.MaxLength = max);
        return box;
    }
}

[SmartEnum<int>]
public sealed partial class ChoiceKind {
    public static readonly ChoiceKind Drop = new(key: 0, mint: static policy => Selected(new DropDown { DataStore = policy.Entries.Cast<object>() }, policy));
    public static readonly ChoiceKind Combo = new(key: 1, mint: static policy => Selected(new ComboBox { DataStore = policy.Entries.Cast<object>() }, policy));
    public static readonly ChoiceKind Roster = new(key: 2, mint: static policy => Selected(new ListBox { DataStore = policy.Entries.Cast<object>() }, policy));
    public static readonly ChoiceKind CheckList = new(key: 3, mint: static policy => new CheckBoxList { DataStore = policy.Entries.Cast<object>(), Orientation = policy.Flow });
    public static readonly ChoiceKind Segmented = new(key: 4, mint: static policy => Segments(policy));
    public static readonly ChoiceKind RadioBank = new(key: 5, mint: static policy => Bank(policy));
    [UseDelegateFromConstructor]
    internal partial Control Mint(ChoicePolicy policy);
    private static ListControl Selected(ListControl control, ChoicePolicy policy) {
        _ = policy.InitialIndex.Iter(index => control.SelectedIndex = index);
        return control;
    }
    private static SegmentedButton Segments(ChoicePolicy policy) {
        SegmentedButton bar = new() { SelectionMode = policy.Segments };
        _ = policy.Entries.Iter(entry => bar.Items.Add(new ButtonSegmentedItem { Text = entry }));
        _ = policy.InitialIndex.Iter(index => bar.SelectedIndex = index);
        return bar;
    }
    private static StackLayout Bank(ChoicePolicy policy) {
        RadioButton? controller = null;
        StackLayout run = new() { Orientation = policy.Flow, Spacing = 4 };
        _ = policy.Entries.Map((entry, index) => {
            RadioButton button = new(controller: controller) { Text = entry, Checked = policy.InitialIndex.Map(chosen => chosen == index).IfNone(false) };
            controller ??= button;
            run.Items.Add(new StackLayoutItem(button));
            return unit;
        }).Strict();
        return run;
    }
}

[SmartEnum<int>]
public sealed partial class ScalarKind {
    public static readonly ScalarKind Stepper = new(key: 0, mint: static policy => new NumericStepper { MinValue = policy.Floor, MaxValue = policy.Ceiling, Increment = policy.Increment, DecimalPlaces = policy.DecimalPlaces });
    public static readonly ScalarKind Track = new(key: 1, mint: static policy => policy.Tick.Match(
        Some: tick => new Slider { MinValue = (int)policy.Floor, MaxValue = (int)policy.Ceiling, Orientation = policy.Axis, TickFrequency = tick, SnapToTick = true },
        None: () => new Slider { MinValue = (int)policy.Floor, MaxValue = (int)policy.Ceiling, Orientation = policy.Axis }));
    public static readonly ScalarKind Meter = new(key: 2, mint: static policy => new ProgressBar { MinValue = (int)policy.Floor, MaxValue = (int)policy.Ceiling, Indeterminate = policy.Indeterminate });
    public static readonly ScalarKind Busy = new(key: 3, mint: static _ => new Spinner { Enabled = true });
    [UseDelegateFromConstructor]
    internal partial Control Mint(ScalarPolicy policy);
}

[SmartEnum<int>]
public sealed partial class PickKind {
    public static readonly PickKind Moment = new(key: 0, mint: static policy => Windowed(new DateTimePicker { Mode = policy.Moment }, policy));
    public static readonly PickKind Month = new(key: 1, mint: static policy => Calendared(new Calendar { Mode = policy.Span }, policy));
    public static readonly PickKind Pigment = new(key: 2, mint: static policy => new ColorPicker { AllowAlpha = policy.AllowAlpha });
    public static readonly PickKind Face = new(key: 3, mint: static _ => new FontPicker());
    public static readonly PickKind PathPick = new(key: 4, mint: static policy => Filtered(new FilePicker { FileAction = policy.FileAction }, policy));
    [UseDelegateFromConstructor]
    internal partial Control Mint(PickPolicy policy);
    private static DateTimePicker Windowed(DateTimePicker picker, PickPolicy policy) {
        _ = policy.Earliest.Iter(floor => picker.MinDate = floor);
        _ = policy.Latest.Iter(ceiling => picker.MaxDate = ceiling);
        return picker;
    }
    private static Calendar Calendared(Calendar calendar, PickPolicy policy) {
        _ = policy.Earliest.Iter(floor => calendar.MinDate = floor);
        _ = policy.Latest.Iter(ceiling => calendar.MaxDate = ceiling);
        return calendar;
    }
    private static FilePicker Filtered(FilePicker picker, PickPolicy policy) {
        _ = policy.Filters.Iter(picker.Filters.Add);
        return picker;
    }
}

[SmartEnum<int>]
public sealed partial class PressKind {
    public static readonly PressKind Push = new(key: 0, mint: static caption => new Button { Text = caption }, wire: static (control, handler) => Op.Side(() => ((Button)control).Click += handler));
    public static readonly PressKind Link = new(key: 1, mint: static caption => new LinkButton { Text = caption }, wire: static (control, handler) => Op.Side(() => ((LinkButton)control).Click += handler));
    [UseDelegateFromConstructor]
    internal partial Control Mint(string caption);
    [UseDelegateFromConstructor]
    internal partial Unit Wire(Control control, EventHandler<EventArgs> handler);
}

[SmartEnum<int>]
public sealed partial class StaticKind {
    public static readonly StaticKind Caption = new(key: 0, mint: static content => new Label { Text = content.Text.IfNone(string.Empty), Wrap = content.Wrap, TextAlignment = content.Alignment });
    public static readonly StaticKind Picture = new(key: 1, mint: static content => content.Image.Match(Some: image => new ImageView { Image = image }, None: static () => (Control)new ImageView()));
    [UseDelegateFromConstructor]
    internal partial Control Mint(StaticContent content);
}

public sealed record StaticContent(Option<string> Text, Option<Image> Image, WrapMode Wrap = WrapMode.Word, TextAlignment Alignment = TextAlignment.Left) {
    public static StaticContent OfText(string text) => new(Text: Some(text), Image: None);
    public static StaticContent OfImage(Image image) => new(Text: None, Image: Some(image));
}

[SmartEnum<int>]
public sealed partial class BoxKind {
    public static readonly BoxKind Plain = new(key: 0, mint: static (body, dress) => new Panel { Content = body, Padding = dress.Padding });
    public static readonly BoxKind Framed = new(key: 1, mint: static (body, dress) => new GroupBox { Content = body, Padding = dress.Padding, Text = dress.Title.IfNone(string.Empty) });
    public static readonly BoxKind Folding = new(key: 2, mint: static (body, dress) => new Expander { Content = body, Header = new Label { Text = dress.Title.IfNone(string.Empty) }, Expanded = dress.Open });
    public static readonly BoxKind Scrolling = new(key: 3, mint: static (body, dress) => new Scrollable { Content = body, Padding = dress.Padding, Border = dress.Border, ExpandContentWidth = dress.ExpandWidth, ExpandContentHeight = dress.ExpandHeight });
    [UseDelegateFromConstructor]
    internal partial Control Mint(Control body, BoxDress dress);
}

public sealed record BoxDress(Padding Padding, Option<string> Title, bool Open = true, BorderType Border = BorderType.Bezel, bool ExpandWidth = true, bool ExpandHeight = true) {
    public static readonly BoxDress Bare = new(Padding: new Padding(0), Title: None);
}
```

## [05]-[ELEMENT_TREE]

- Owner: `Element` — the recursive closed `[Union]` screen tree. Leaf cases carry a kind row plus its policy; structural cases carry children; `Painted` composes the `canvas.md` surface owner, `Embedded` the `platform.md` native mount, `Tabular` the `[06]` grid family, and `Laid` the `[07]` arrangement algebra. ONE `Realize(Op? key = null)` is the sole construction entry — the total generated `Switch` mints through the kind rows, folds `ElementSpec.Apply` over every product, and recurses `TraverseM` over children, so a screen realizes or fails as one rail value and a new case breaks this dispatch loudly at compile time.
- Cases: `Text(TextKind, ElementSpec, TextPolicy)` · `Choice(ChoiceKind, ElementSpec, ChoicePolicy)` · `Scalar(ScalarKind, ElementSpec, ScalarPolicy)` · `Pick(PickKind, ElementSpec, PickPolicy)` · `Press(PressKind, ElementSpec, string Caption, Func<Fin<Unit>> Effect)` · `Toggle(ElementSpec, string Caption, Option<bool> Seed, bool ThreeState)` (`CheckBox` — `Seed` `None` realizes indeterminate under `ThreeState`, unchecked otherwise; the value channel is a `CheckedBinding` attachment) · `Static(StaticKind, ElementSpec, StaticContent)` · `Boxed(BoxKind, ElementSpec, BoxDress, Element Child)` · `Tabs(ElementSpec, TabStyle, Seq<(string Title, Element Body)> Pages, int Selected)` · `Split(ElementSpec, Orientation, SplitterFixedPanel, Element First, Element Second, Option<double> Relative)` · `Tabular(ElementSpec, GridPlan)` · `Laid(ElementSpec, Arrangement)` · `Painted(ElementSpec, SurfaceSpec)` · `Embedded(ElementSpec, NativeMount)` · `Web(ElementSpec, Uri)` · `Inspector(ElementSpec, object Subject, bool Categories)` — sixteen cases; `TabStyle` selects `TabControl` (fixed) versus `DocumentControl` (closable, reorderable).
- Law: `Press.Effect` is the one event-shaped leaf — the `Click` subscription is the named platform-forced seam, its body routed through `Op.Catch`, and a press that participates in menus, toolbars, or gestures graduates to a `chrome.md` intent row realized here as a `Press` bound to the row's `Command.Execute`; two parallel effect paths for one verb is the deleted form.
- Law: realization composes, never orchestrates — a consumer holds an `Element` value and calls `Realize` once; reaching into a realized tree to mutate structure is the deleted form, because structure changes are a new `Element` value realized into a container's `Content`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, generated `Switch`), LanguageExt.Core (`Fin`, `Seq`, `TraverseM`, `Option`), Rasm.Domain (project — `Op`, `Op.Catch`), Eto.Forms (host — the verified construction surface of `[04]`'s rows plus `CheckBox`, `TabControl`/`TabPage`, `DocumentControl`/`DocumentPage`, `Splitter`, `WebView`, `PropertyGrid`).
- Growth: a new roster control is one kind row (zero tree edits) when it fits a family, one union case when its payload is genuinely new; anticipated host additions (a token-input, a chart host) land the same way — the FIVE-TIMES demand is absorbed by the family shape, not by new factories.
- Boundary: `WebView` script execution, navigation events, and message channels are consumer wiring over the realized control's own verified members (`Url`, `ExecuteScriptAsync`, `DocumentLoaded`, `MessageReceived`); this owner realizes the host and hands it over — a second navigation abstraction here is the deleted form.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TabStyle {
    public static readonly TabStyle Fixed = new(key: 0);
    public static readonly TabStyle Closable = new(key: 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Element {
    private Element() { }
    public sealed record Text(TextKind Kind, ElementSpec Spec, TextPolicy Policy) : Element;
    public sealed record Choice(ChoiceKind Kind, ElementSpec Spec, ChoicePolicy Policy) : Element;
    public sealed record Scalar(ScalarKind Kind, ElementSpec Spec, ScalarPolicy Policy) : Element;
    public sealed record Pick(PickKind Kind, ElementSpec Spec, PickPolicy Policy) : Element;
    public sealed record Press(PressKind Kind, ElementSpec Spec, string Caption, Func<Fin<Unit>> Effect) : Element;
    public sealed record Toggle(ElementSpec Spec, string Caption, Option<bool> Seed = default, bool ThreeState = false) : Element;
    public sealed record Static(StaticKind Kind, ElementSpec Spec, StaticContent Content) : Element;
    public sealed record Boxed(BoxKind Kind, ElementSpec Spec, BoxDress Dress, Element Child) : Element;
    public sealed record Tabs(ElementSpec Spec, TabStyle Style, Seq<(string Title, Element Body)> Pages, int Selected = 0) : Element;
    public sealed record Split(ElementSpec Spec, Orientation Axis, SplitterFixedPanel Fixed, Element First, Element Second, Option<double> Relative = default) : Element;
    public sealed record Tabular(ElementSpec Spec, GridPlan Plan) : Element;
    public sealed record Laid(ElementSpec Spec, Arrangement Arrangement) : Element;
    public sealed record Painted(ElementSpec Spec, SurfaceSpec Surface) : Element;
    public sealed record Embedded(ElementSpec Spec, NativeMount Mount) : Element;
    public sealed record Web(ElementSpec Spec, Uri Address) : Element;
    public sealed record Inspector(ElementSpec Spec, object Subject, bool Categories = true) : Element;

    public Fin<Control> Realize(Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            state: op,
            text: static (op, node) => Minted(op, node.Spec, () => node.Kind.Mint(policy: node.Policy)),
            choice: static (op, node) => Minted(op, node.Spec, () => node.Kind.Mint(policy: node.Policy)),
            scalar: static (op, node) => Minted(op, node.Spec, () => node.Kind.Mint(policy: node.Policy)),
            pick: static (op, node) => Minted(op, node.Spec, () => node.Kind.Mint(policy: node.Policy)),
            press: static (op, node) => Minted(op, node.Spec, () => Pressed(op, node)),
            toggle: static (op, node) => Minted(op, node.Spec, () => (Control)new CheckBox {
                Text = node.Caption,
                ThreeState = node.ThreeState,
                Checked = node.Seed.Match(Some: static held => (bool?)held, None: () => node.ThreeState ? null : (bool?)false),
            }),
            @static: static (op, node) => Minted(op, node.Spec, () => node.Kind.Mint(content: node.Content)),
            boxed: static (op, node) => node.Child.Realize(key: op).Bind(body => Minted(op, node.Spec, () => node.Kind.Mint(body: body, dress: node.Dress))),
            tabs: static (op, node) => Paged(op, node),
            split: static (op, node) =>
                from first in node.First.Realize(key: op)
                from second in node.Second.Realize(key: op)
                from control in Minted(op, node.Spec, () => {
                    Splitter splitter = new() { Orientation = node.Axis, FixedPanel = node.Fixed, Panel1 = first, Panel2 = second };
                    _ = node.Relative.Iter(fraction => splitter.RelativePosition = fraction);
                    return (Control)splitter;
                })
                select control,
            tabular: static (op, node) => node.Plan.Realize(key: op).Bind(grid => node.Spec.Apply(control: grid, key: op).Map(_ => grid)),
            laid: static (op, node) => node.Arrangement.Realize(key: op).Bind(control => node.Spec.Apply(control: control, key: op).Map(_ => control)),
            painted: static (op, node) => node.Surface.Mount(key: op).Bind(drawable => node.Spec.Apply(control: drawable, key: op).Map(_ => (Control)drawable)),
            embedded: static (op, node) => node.Mount.Realize(key: op).Bind(host => node.Spec.Apply(control: host, key: op).Map(_ => host)),
            web: static (op, node) => Minted(op, node.Spec, () => (Control)new WebView { Url = node.Address }),
            inspector: static (op, node) => Minted(op, node.Spec, () => (Control)new PropertyGrid { SelectedObject = node.Subject, ShowCategories = node.Categories }));
    }

    private static Fin<Control> Minted(Op op, ElementSpec spec, Func<Control> mint) =>
        op.Catch(() => Fin.Succ(value: mint())).Bind(control => spec.Apply(control: control, key: op).Map(_ => control));

    private static Control Pressed(Op op, Press node) {
        Control control = node.Kind.Mint(caption: node.Caption);
        _ = node.Kind.Wire(control: control, handler: (_, _) => _ = op.Catch(node.Effect));
        return control;
    }

    private static Fin<Control> Paged(Op op, Tabs node) =>
        node.Pages.TraverseM(page => page.Body.Realize(key: op).Map(body => (page.Title, Body: body))).As().Bind(realized =>
            Minted(op, node.Spec, () => node.Style.Switch(
                state: (Pages: realized, node.Selected),
                @fixed: static held => {
                    TabControl tabs = new();
                    _ = held.Pages.Iter(page => tabs.Pages.Add(new TabPage(page.Body) { Text = page.Title }));
                    tabs.SelectedIndex = Math.Clamp(value: held.Selected, min: 0, max: Math.Max(val1: 0, val2: (int)held.Pages.Count - 1));
                    return (Control)tabs;
                },
                closable: static held => {
                    DocumentControl documents = new() { AllowReordering = true };
                    _ = held.Pages.Iter(page => documents.Pages.Add(new DocumentPage(page.Body) { Text = page.Title }));
                    documents.SelectedIndex = Math.Clamp(value: held.Selected, min: 0, max: Math.Max(val1: 0, val2: (int)held.Pages.Count - 1));
                    return (Control)documents;
                })));
}
```

## [06]-[GRID_FAMILY]

- Owner: `GridPlan` — the closed `[Union]` over the three bound-tree hosts (`Flat` over `GridView`, `Branched` over `TreeGridView` + `TreeGridItemCollection`, `Outline` over `TreeView` + `TreeItem`) — with `CellRow` the full host cell roster as a closed family, `ColumnPlan` the one column declaration, `RowSeed`/`BranchSeed` the item carriers, and `GridChrome` the presentation policy the two `Grid`-derived hosts share (`ShowHeader`, `GridLines`, `Border`, `RowHeight`, selection multiplicity); `Outline` carries no chrome because `TreeView` derives `Control`, not `Grid`. One `Realize(Op)` per plan mints store, columns, and chrome in one fold; a per-grid construction helper or a column family split by cell type is the deleted form.
- Cases: `CellRow` `Written(int Column, TextAlignment)` (`TextBoxCell`) · `Ticked(int Column)` (`CheckBoxCell`) · `Chosen(int Column, Seq<object> Choices)` (`ComboBoxCell` with `DataStore`) · `Pictured(int Column)` (`ImageViewCell`) · `Badged(int ImageColumn, int TextColumn)` (`ImageTextCell`) · `Gauged(int Column)` (`ProgressCell`) · `Hosted(Func<CellEventArgs, Control> Create, Option<Action<CellEventArgs, Control>> Configure)` (`CustomCell`) · `Drawn(Action<DrawableCellPaintEventArgs> Paint)` (`DrawableCell`) — eight cases covering the entire host cell family.
- Law: cells bind by column index into `GridItem.Values`/`TreeGridItem` values — the index is the one correspondence between `ColumnPlan` order and seed `Values` order, stated here once; `Editable` is a `ColumnPlan` field consumed at realization, and commit observation rides the host `CellEdited` event wired by the consumer over the realized grid's verified surface.
- Law: selection is egress, not state — consumers read `Grid.SelectedItems`/`SelectedRow` or attach a `binding.md` row over `SelectedItemBinding`; a parallel selection cache is the deleted form.
- Growth: a new cell modality is one `CellRow` case; a new grid host is one `GridPlan` case; a chrome axis is one `GridChrome` field consumed once in `Dress`.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CellRow {
    private CellRow() { }
    public sealed record Written(int Column, TextAlignment Alignment = TextAlignment.Left) : CellRow;
    public sealed record Ticked(int Column) : CellRow;
    public sealed record Chosen(int Column, Seq<object> Choices) : CellRow;
    public sealed record Pictured(int Column) : CellRow;
    public sealed record Badged(int ImageColumn, int TextColumn) : CellRow;
    public sealed record Gauged(int Column) : CellRow;
    public sealed record Hosted(Func<CellEventArgs, Control> Create, Option<Action<CellEventArgs, Control>> Configure = default) : CellRow;
    public sealed record Drawn(Action<DrawableCellPaintEventArgs> Paint) : CellRow;

    internal Cell Mint() => Switch(
        written: static row => new TextBoxCell(column: row.Column) { TextAlignment = row.Alignment },
        ticked: static row => new CheckBoxCell(column: row.Column),
        chosen: static row => new ComboBoxCell(column: row.Column) { DataStore = row.Choices },
        pictured: static row => new ImageViewCell(column: row.Column),
        badged: static row => new ImageTextCell(imageColumn: row.ImageColumn, textColumn: row.TextColumn),
        gauged: static row => new ProgressCell(column: row.Column),
        hosted: static row => {
            CustomCell cell = new() { CreateCell = row.Create };
            _ = row.Configure.Iter(configure => cell.ConfigureCell = configure);
            return (Cell)cell;
        },
        drawn: static row => {
            DrawableCell cell = new();
            cell.Paint += (_, args) => row.Paint(args);
            return (Cell)cell;
        });
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ColumnPlan(string Header, CellRow Cell, int Width = -1, bool Editable = false, bool Sortable = false, bool Expand = false) {
    internal GridColumn Mint() {
        GridColumn column = new() { HeaderText = Header, DataCell = Cell.Mint(), Editable = Editable, Sortable = Sortable, Expand = Expand };
        _ = Op.SideWhen(Width > 0, () => column.Width = Width);
        return column;
    }
}

public sealed record RowSeed(object[] Values, Option<object> Tag = default) {
    internal GridItem Mint() {
        GridItem item = new(values: Values);
        _ = Tag.Iter(tag => item.Tag = tag);
        return item;
    }
}

public sealed record BranchSeed(object[] Values, Seq<BranchSeed> Children, bool Expanded = false) {
    internal TreeGridItem Mint() =>
        new(children: Children.Map(static child => (ITreeGridItem)child.Mint()), values: Values) { Expanded = Expanded };
}

public sealed record GridChrome(bool ShowHeader = true, GridLines Lines = GridLines.None, BorderType Border = BorderType.Bezel, int RowHeight = -1, bool MultiSelect = false, bool AllowEmpty = true) {
    public static readonly GridChrome Default = new();
    internal TGrid Dress<TGrid>(TGrid grid) where TGrid : Grid {
        grid.ShowHeader = ShowHeader;
        grid.GridLines = Lines;
        grid.Border = Border;
        grid.AllowMultipleSelection = MultiSelect;
        grid.AllowEmptySelection = AllowEmpty;
        _ = Op.SideWhen(RowHeight > 0, () => grid.RowHeight = RowHeight);
        return grid;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridPlan {
    private GridPlan() { }
    public sealed record Flat(Seq<ColumnPlan> Columns, Seq<RowSeed> Rows, GridChrome Chrome) : GridPlan;
    public sealed record Branched(Seq<ColumnPlan> Columns, Seq<BranchSeed> Roots, GridChrome Chrome) : GridPlan;
    public sealed record Outline(TreeItem Root) : GridPlan;

    internal Fin<Control> Realize(Op key) => Switch(
        state: key,
        flat: static (op, plan) => op.Catch(() => {
            GridView grid = plan.Chrome.Dress(new GridView { DataStore = plan.Rows.Map(static seed => (object)seed.Mint()) });
            _ = plan.Columns.Iter(column => grid.Columns.Add(column.Mint()));
            return Fin.Succ(value: (Control)grid);
        }),
        branched: static (op, plan) => op.Catch(() => {
            TreeGridView tree = plan.Chrome.Dress(new TreeGridView { DataStore = new TreeGridItemCollection(items: plan.Roots.Map(static seed => (ITreeGridItem)seed.Mint())) });
            _ = plan.Columns.Iter(column => tree.Columns.Add(column.Mint()));
            return Fin.Succ(value: (Control)tree);
        }),
        outline: static (op, plan) => op.Catch(() => Fin.Succ(value: (Control)new TreeView { DataStore = plan.Root })));
}
```

## [07]-[LAYOUT_ALGEBRA]

- Owner: `Arrangement` — the closed `[Union]` over the four host layout strategies — and `FlowRegion`, the recursive region algebra that absorbs `DynamicLayout` construction. The census-era imperative builder scatter is replaced by values: a `Flow` arrangement is a region tree folded onto the verified `Add(control, xscale, yscale)`/`AddCentered`/`AddAutoSized` members, `GridTable` folds `TableRow`/`TableCell` scale flags, `Run` folds `StackLayoutItem` expansion and alignment, and `Absolute` folds `PixelLayout.Add` placements. The `Begin*`/`End*` stateful scope protocol is structurally bypassed — a region VALUE cannot dangle an unclosed scope, so nesting realizes each child region as its own layout control — with ONE arm-local exception: the `Row` fold brackets `BeginHorizontal`/`EndHorizontal` inside a single expression (opened and closed adjacently, no scope can escape) because `AddRow` carries no per-cell scale and only `Add` inside a horizontal section does.
- Cases: `Arrangement` `Flow(Padding, Size Spacing, Seq<FlowRegion>)` · `GridTable(Padding, Size Spacing, Seq<TablePlanRow>)` · `Run(Orientation, int Spacing, Seq<RunItem>)` · `Absolute(Seq<(Element Item, Point At)>)`; `FlowRegion` `Leaf(Element, Option<bool> XScale, Option<bool> YScale)` · `Row(Seq<FlowRegion>)` · `ColumnStack(Seq<FlowRegion>)` · `Centered(FlowRegion)` · `AutoSized(FlowRegion)`; `TablePlanRow(Seq<TablePlanCell> Cells, bool ScaleHeight)` with `TablePlanCell(Element Item, bool ScaleWidth)`; `RunItem(Element Item, bool Expand, Option<VerticalAlignment> Align)`.
- Law: scale is declared where it binds AND lands where the host reads it — a `Leaf`'s `XScale`/`YScale` travel with the realized control and every consuming arm writes them onto the verified `xscale`/`yscale` parameters (`Add`, `AddCentered`, `AddAutoSized`); table scale rides row/cell flags mapped to `TableRow.ScaleHeight`/`TableCell.ScaleWidth`, run expansion rides `StackLayoutItem.Expand` — a declared flag no fold consumes is the illusory form this law forecloses.
- Growth: a new placement strategy the host ships is one `Arrangement` case; a new flow modality is one `FlowRegion` case; both break `Realize` at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FlowRegion {
    private FlowRegion() { }
    public sealed record Leaf(Element Item, Option<bool> XScale = default, Option<bool> YScale = default) : FlowRegion;
    public sealed record Row(Seq<FlowRegion> Cells) : FlowRegion;
    public sealed record ColumnStack(Seq<FlowRegion> Cells) : FlowRegion;
    public sealed record Centered(FlowRegion Body) : FlowRegion;
    public sealed record AutoSized(FlowRegion Body) : FlowRegion;

    internal Fin<(Control Control, bool? XScale, bool? YScale)> Realize(Padding padding, Size spacing, Op key) => Switch(
        state: (Padding: padding, Spacing: spacing, Key: key),
        leaf: static (frame, region) => region.Item.Realize(key: frame.Key).Map(control =>
            (control, region.XScale.Match(Some: static held => (bool?)held, None: static () => null), region.YScale.Match(Some: static held => (bool?)held, None: static () => null))),
        row: static (frame, region) =>
            region.Cells.TraverseM(cell => cell.Realize(padding: frame.Padding, spacing: frame.Spacing, key: frame.Key)).As().Bind(cells =>
                frame.Key.Catch(() => {
                    DynamicLayout flow = new() { Padding = frame.Padding, Spacing = frame.Spacing };
                    _ = flow.BeginHorizontal();
                    _ = cells.Iter(cell => _ = flow.Add(control: cell.Control, xscale: cell.XScale, yscale: cell.YScale));
                    flow.EndHorizontal();
                    return Fin.Succ(value: ((Control)flow, (bool?)null, (bool?)null));
                })),
        columnStack: static (frame, region) =>
            region.Cells.TraverseM(cell => cell.Realize(padding: frame.Padding, spacing: frame.Spacing, key: frame.Key)).As().Bind(cells =>
                frame.Key.Catch(() => {
                    DynamicLayout flow = new() { Padding = frame.Padding, Spacing = frame.Spacing };
                    _ = cells.Iter(cell => _ = flow.Add(control: cell.Control, xscale: cell.XScale, yscale: cell.YScale));
                    return Fin.Succ(value: ((Control)flow, (bool?)null, (bool?)null));
                })),
        centered: static (frame, region) =>
            region.Body.Realize(padding: frame.Padding, spacing: frame.Spacing, key: frame.Key).Bind(body =>
                frame.Key.Catch(() => {
                    DynamicLayout flow = new() { Padding = frame.Padding, Spacing = frame.Spacing };
                    flow.AddCentered(control: body.Control, padding: frame.Padding, spacing: frame.Spacing, xscale: body.XScale, yscale: body.YScale);
                    return Fin.Succ(value: ((Control)flow, (bool?)null, (bool?)null));
                })),
        autoSized: static (frame, region) =>
            region.Body.Realize(padding: frame.Padding, spacing: frame.Spacing, key: frame.Key).Bind(body =>
                frame.Key.Catch(() => {
                    DynamicLayout flow = new() { Padding = frame.Padding, Spacing = frame.Spacing };
                    flow.AddAutoSized(control: body.Control, padding: frame.Padding, xscale: body.XScale, yscale: body.YScale);
                    return Fin.Succ(value: ((Control)flow, (bool?)null, (bool?)null));
                })));
}

public sealed record TablePlanCell(Element Item, bool ScaleWidth = false);

public sealed record TablePlanRow(Seq<TablePlanCell> Cells, bool ScaleHeight = false);

public sealed record RunItem(Element Item, bool Expand = false, Option<VerticalAlignment> Align = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Arrangement {
    private Arrangement() { }
    public sealed record Flow(Padding Padding, Size Spacing, Seq<FlowRegion> Regions) : Arrangement;
    public sealed record GridTable(Padding Padding, Size Spacing, Seq<TablePlanRow> Rows) : Arrangement;
    public sealed record Run(Orientation Axis, int Spacing, Seq<RunItem> Items) : Arrangement;
    public sealed record Absolute(Seq<(Element Item, Point At)> Placements) : Arrangement;

    internal Fin<Control> Realize(Op key) => Switch(
        state: key,
        flow: static (op, plan) =>
            plan.Regions.TraverseM(region => region.Realize(padding: plan.Padding, spacing: plan.Spacing, key: op)).As().Bind(regions =>
                op.Catch(() => {
                    DynamicLayout flow = new() { Padding = plan.Padding, Spacing = plan.Spacing };
                    _ = regions.Iter(region => _ = flow.Add(control: region.Control, xscale: region.XScale, yscale: region.YScale));
                    return Fin.Succ(value: (Control)flow);
                })),
        gridTable: static (op, plan) =>
            plan.Rows.TraverseM(row =>
                row.Cells.TraverseM(cell => cell.Item.Realize(key: op).Map(control => new TableCell(control: control, scaleWidth: cell.ScaleWidth))).As()
                    .Map(cells => new TableRow([.. cells]) { ScaleHeight = row.ScaleHeight })).As().Bind(rows =>
                op.Catch(() => Fin.Succ(value: (Control)new TableLayout([.. rows]) { Padding = plan.Padding, Spacing = plan.Spacing }))),
        run: static (op, plan) =>
            plan.Items.TraverseM(item => item.Item.Realize(key: op).Map(control =>
                item.Align.Match(
                    Some: alignment => new StackLayoutItem(control: control, alignment: alignment, expand: item.Expand),
                    None: () => new StackLayoutItem(control: control, expand: item.Expand)))).As().Bind(items =>
                op.Catch(() => {
                    StackLayout stack = new() { Orientation = plan.Axis, Spacing = plan.Spacing };
                    _ = items.Iter(stack.Items.Add);
                    return Fin.Succ(value: (Control)stack);
                })),
        absolute: static (op, plan) =>
            plan.Placements.TraverseM(placement => placement.Item.Realize(key: op).Map(control => (Control: control, placement.At))).As().Bind(placed =>
                op.Catch(() => {
                    PixelLayout canvas = new();
                    _ = placed.Iter(entry => canvas.Add(control: entry.Control, x: entry.At.X, y: entry.At.Y));
                    return Fin.Succ(value: (Control)canvas);
                })));
}
```
