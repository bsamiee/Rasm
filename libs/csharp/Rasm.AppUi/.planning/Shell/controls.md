# [APPUI_CONTROL_MATERIALIZATION]

One typed `ControlIntent` family materializes every interactive control from a declarative shape: a screen body is a control-intent stream, not a per-screen XAML literal. `ControlIntent` is the closed `[Union]` over the whole professional control vocabulary where arity, provider, modality, emphasis, iconography, and pending state live in the intent shape rather than parallel control names, `ControlFactory` is the one polymorphic fold projecting each intent onto its compiled-template control, `BehaviorRail.Intent` is the single binding bridge every materialized command rides, appearance reaches every control through its `ControlTheme` and its style classes alone, and `Shell/accessibility` derives every automation name from the one intent row. The page owns the intent vocabulary, the materialize fold and its boundary capsule, the context-column and package admission law, the recycling pool, and the sole projection into the generated `Rasm.Contracts.Ui.V1` control messages; it mints no parallel binding, token, automation, template, or wire-shape path — the `[04]-[BOUNDARIES]` parallel-control-framework clause forecloses it. The spine is Avalonia compiled `ControlTemplate`/`DataTemplate`/`ControlTheme`, the `Irihi.Ursa` extended-control suite, ReactiveUI commands, `Xaml.Behaviors.Avalonia`, the kernel `CapabilitySet`/`FaultBand` owners, Thinktecture.Runtime.Extensions, and LanguageExt rails.

Appearance is the page's ruling shape: the fold writes NO resolved paint, metric, or shadow onto a control. Emphasis resolves to one `ControlTheme` row of the `Theme/tokens#CONTROL_THEMES` table through `StyledElement.Theme`, the semantic `PaintRole` and the `TypographyRole` land as style classes the theme's own selectors match, and every value inside those themes binds `{DynamicResource}` — so a variant swap re-tints a materialized screen through Avalonia's own resource resolution and a `SetValue` of a resolved brush is unspellable in this fold.

## [01]-[INDEX]

- [02]-[CONTROL_INTENT]: One closed control vocabulary; per-case typed shape with the emphasis, icon, pending, and hint columns; the four-column `ControlShape` answer.
- [03]-[MATERIALIZE_FOLD]: The `ControlFactory` intent-to-control fold; the context-column and package admission law; one `BehaviorRail.Intent` bridge; total automation derivation.
- [04]-[CONTROL_RECYCLING]: The recycling-aware materialization boundary the `VirtualWindow` grid/tree/panel kinds consume.
- [05]-[TS_PROJECTION]: The design-pinned projection into the generated `Ui.V1` control-intent family.

## [02]-[CONTROL_INTENT]

- Owner: `ControlIntent` `[Union]` the interactive-control family; `IntentBinding` the per-intent command, emphasis, icon, and hint carrier; `ControlEmphasis` the emphasis ladder; `ControlSkin` the control-theme row every arm addresses; `NumericKind`/`NumericRange`, `TemporalKind` over `TemporalTrait`, `BannerSeverity`/`BannerPlacement`, and the posture rows the polymorphic cases discriminate on — each posture row carrying its host type, its value slot, and (where it constructs) its own mint as columns; `ControlShape` the one four-column per-case answer (`Parked`, `Skin`, `Slot`, `Redress`) the total `ShapeOf` fold serves; `ControlFault` the direct generated `[Union]` with one `[FaultCase]` leaf per control failure.
- Cases: `ControlIntent` = Button | Label | TextInput | NumberInput | DateInput | PathInput | ColorInput | Select | MultiSelect | Slider | Range | Toggle | Radio | Segmented | Chip | Progress | Avatar | Breadcrumb | Tooltip | Banner | EmptyState | Grid | Tree | Overview | Menu | Toolbar | Tab | Accordion | Panel | Dock | Splitter; `ControlEmphasis` = quiet | secondary | primary | danger | inverted | link; `NumericRange` = Integral | Unsigned | Real | Precise; `OptionSource` = Inline | Bound; `ControlFault` = UnboundIntent | SkinUnresolved | TemplateMissing | RecyclingViolation | SlotUnavailable | PayloadRejected.
- Law: emphasis, icon, pending, and hint are COLUMNS on `IntentBinding`, never cases — a quiet button and a danger button are one `Button` intent under two emphasis rows, so both heads materialize the ladder identically and a per-emphasis control name is unspellable.
- Law: a pending column lives ON the icon slot, so in-control progress always replaces a leading visual and a spinner that widens its own control is structurally unrepresentable; pending drives the loading slot alone and never `IsEnabled`.
- Law: numeric entry is the typed spinner matching the bound CLR type — `NumericKind` names the type, `NumericRange` carries the bounds in the widest form that type admits, and the narrowing runs through generic-math checked conversion, so a `double` field keeps `NaN` and full range while a `ulong` field keeps its top decade; a single `decimal` bound column across every numeric field is the deleted form.
- Law: the per-case host-type, value-slot, re-dress, and theme answers live in ONE place — the posture rows' columns and the total `ShapeOf` fold over them — so a second per-case ladder (a re-dress switch, a content-property switch, a host-name method) is the hand-kept-mirror defect this page deleted.
- Entry: every case is one record whose fields carry the control's typed shape and whose `IntentBinding` carries the semantic `PaintRole`, the `ControlEmphasis` row, the `Option<string>` command key, the value key, the activation trigger, the optional `IconSlot`, and the optional `HintRow` — a quiet icon-leading pending button is `Button` with three columns set, not a `GetQuietPendingIconButton` name.
- Auto: the `EditorFactory` typed-shape→control precedent already proven in `PropertyGrid` cells (`Editing/inspector#EDITOR_FACTORIES`) is the inspector specialization of this vocabulary — `ControlIntent` generalizes it from property cells to whole screens; `Theme/tokens` control-theme rows resolve every appearance and `Shell/accessibility` derives automation identity from `ControlIntent.Key`, so per-control token and automation literals are deleted.
- Packages: Rasm.Contracts (project), Irihi.Ursa, Avalonia, Rasm (kernel `FaultBand`/`[FaultCase]`/`Fault`/`CapabilitySet`), Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new control is one `ControlIntent` case carrying its shape plus `IntentBinding` and one `ShapeOf` arm; a new emphasis is one `ControlEmphasis` row naming its skin; a new numeric type is one `NumericKind` row; a new temporal shape is one `TemporalKind` row with its trait set; a new banner level is one `BannerSeverity` row carrying its own dismissibility; a new modality on an existing case is one posture row; a new fault case is one `[FaultCase]` leaf; zero new surface.
- Boundary: `ControlIntent` is the one control vocabulary in the package — a per-screen control-builder, a second control-generation framework, and a parallel binding, token, or automation path are the `[04]-[BOUNDARIES]` parallel-control-framework rejected forms; the command column is `Option<string>` carrying the `CommandRow` key the materialized control's `ICommand` resolves through `BehaviorRail.Intent`, never a `ReactiveCommand` instance on the intent, so the intent crosses the `ControlIntentWire` seam unchanged; container kinds carry their child-intent sequence so a whole screen is one nested intent tree, while `Menu`, `Breadcrumb`, and the option-bearing kinds carry their own structured ROW shapes, because a menu row's check posture, submenu, and gesture hint are fields no control intent owns; the `Grid`, `Tree`, `Select`, and `MultiSelect` kinds carry the `VirtualWindow` window spec the `Shell/virtualization` fabric owner consumes — the spec crosses the wire so a remote head windows the same viewport contract; the `Tree` kind materializes the `FlatNode` union the flatten emits, so a hierarchy and a GROUPED list ride one item template; the `Overview` kind is the one minimap primitive and names its frame producer by key rather than carrying geometry; value-carrying kinds carry a typed two-way binding path read at materialize, and `Range` carries the second path its upper thumb round-trips; the `Dock` and `Splitter` kinds defer their layout to the `Shell/solver` owner; `IconSlot` is a CONTROL-level shape and its per-row seats (`OptionRow`, `CrumbRow`, `MenuRow`) carry the ASSET and the size alone as a documented narrowing — the item template fixes the leading slot so `Placement` has no reader there, and a per-row `Pending` would need a per-row value key plus a per-row lifetime, a second binding path beside `MaterializeContext.Value`; placement, overflow, picker mode, and toggle vocabulary are the packages' own enums (`Ursa.Common` `Position`, `Ursa.Controls` `OverflowMode` and `UsePickerTypes`, Avalonia `HorizontalAlignment`, `Orientation`, `MenuItemToggleType`, `DataGridLength`), because re-spelling an admitted package's own axis is a rename shell; `OptionRow` and `CrumbRow` share a field-set by shape alone — the discriminant is the CONSUMER's binding triple (an option round-trips its `Value` through `SelectedValueBinding`, a crumb dispatches it through `CommandParameterBinding`), stated at both declarations, so the pair survives on a named payload-timing discriminant rather than folding into a row a crumb could group by accident.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Control;
    private ControlFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record UnboundIntent(string Detail)      : ControlFault(Detail);
    [FaultCase(1)]
    public sealed partial record SkinUnresolved(string Detail)     : ControlFault(Detail);
    [FaultCase(2)]
    public sealed partial record TemplateMissing(string Detail)    : ControlFault(Detail);
    [FaultCase(3)]
    public sealed partial record RecyclingViolation(string Detail) : ControlFault(Detail);
    [FaultCase(4)]
    public sealed partial record SlotUnavailable(string Detail)    : ControlFault(Detail);
    [FaultCase(5)]
    public sealed partial record PayloadRejected(string Detail)    : ControlFault(Detail);
}

// --- [TYPES] --------------------------------------------------------------------------------
// The control-theme rows this fold addresses. The Key IS the dictionary resource key, so a row is an ADDRESS
// and never a second copy of the `Theme/tokens#CONTROL_THEMES` derivation and pseudo-class columns; a shipped
// key addresses the vendor theme verbatim. An arm naming no row falls to the shipped type-keyed theme.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlSkin {
    public static readonly ControlSkin ProgressRing = new("ProgressRing");
    public static readonly ControlSkin CommandButton = new("command-button");
    public static readonly ControlSkin SecondaryButton = new("secondary-button");
    public static readonly ControlSkin QuietButton = new("quiet-button");
    public static readonly ControlSkin DangerButton = new("danger-button");
    public static readonly ControlSkin InvertedButton = new("inverted-button");
    public static readonly ControlSkin LinkButton = new("link-button");
    public static readonly ControlSkin TextEntry = new("text-entry");
    public static readonly ControlSkin RadioItem = new("radio-item");
    public static readonly ControlSkin SegmentedItem = new("segmented-item");
    public static readonly ControlSkin SegmentedIndicator = new("segmented-indicator");
    public static readonly ControlSkin GridRow = new("grid-row");
    public static readonly ControlSkin StatusChip = new("status-chip");
    public static readonly ControlSkin PaletteRow = new("palette-row");
    public static readonly ControlSkin EmptyStatePanel = new("empty-state-panel");
    public static readonly ControlSkin Tooltip = new("tooltip");
    // No ButtonGroupItem row: Ursa binds ButtonGroupItemTheme onto every group item itself, so a skin
    // address here would be a row no materialize arm reads.
    public static readonly ControlSkin AvatarCluster = new("avatar-cluster");
    public static readonly ControlSkin Banner = new("banner");
    public static readonly ControlSkin OverviewStrip = new("overview-strip");
}

// `Iconable` is a genuinely independent single bit and stays a bool by the kernel capability law's own carve:
// the link row refuses an icon because its shipped theme owns a trailing link glyph. The host type the button
// family constructs is the `ShapeOf` button arm's answer, derived from emphasis and icon together.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlEmphasis {
    public static readonly ControlEmphasis Quiet = new(
        "quiet", Rasm.Contracts.Ui.V1.ControlEmphasis.Quiet, ControlSkin.QuietButton, iconable: true);
    public static readonly ControlEmphasis Secondary = new(
        "secondary", Rasm.Contracts.Ui.V1.ControlEmphasis.Secondary, ControlSkin.SecondaryButton, iconable: true);
    public static readonly ControlEmphasis Primary = new(
        "primary", Rasm.Contracts.Ui.V1.ControlEmphasis.Primary, ControlSkin.CommandButton, iconable: true);
    public static readonly ControlEmphasis Danger = new(
        "danger", Rasm.Contracts.Ui.V1.ControlEmphasis.Danger, ControlSkin.DangerButton, iconable: true);
    public static readonly ControlEmphasis Inverted = new(
        "inverted", Rasm.Contracts.Ui.V1.ControlEmphasis.Inverted, ControlSkin.InvertedButton, iconable: true);
    public static readonly ControlEmphasis Link = new(
        "link", Rasm.Contracts.Ui.V1.ControlEmphasis.Link, ControlSkin.LinkButton, iconable: false);

    public Rasm.Contracts.Ui.V1.ControlEmphasis Wire { get; }
    public ControlSkin Skin { get; }
    public bool Iconable { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlTrigger {
    public static readonly ControlTrigger Activate = new("activate", Rasm.Contracts.Ui.V1.ControlTrigger.Activate);
    public static readonly ControlTrigger Change = new("change", Rasm.Contracts.Ui.V1.ControlTrigger.Change);
    public static readonly ControlTrigger Commit = new("commit", Rasm.Contracts.Ui.V1.ControlTrigger.Commit);

    public Rasm.Contracts.Ui.V1.ControlTrigger Wire { get; }
}

// Posture rows carry host type AND value slot as columns, so the per-case slot ladder the old content-property
// switch re-derived is a row read.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectPosture {
    public static readonly SelectPosture Closed = new(
        "closed", Rasm.Contracts.Ui.V1.SelectPosture.Closed, nameof(ComboBox), SelectingItemsControl.SelectedValueProperty);
    public static readonly SelectPosture Editable = new(
        "editable", Rasm.Contracts.Ui.V1.SelectPosture.Editable, nameof(AutoCompleteBox), AutoCompleteBox.TextProperty);

    public Rasm.Contracts.Ui.V1.SelectPosture Wire { get; }
    public string Control { get; }
    public AvaloniaProperty Slot { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MultiPosture {
    // Bound multi-select picks from a closed option set and reads back as chips; free multi-select accepts
    // arbitrary tokens, which is the one thing a bound picker cannot express.
    public static readonly MultiPosture Bound = new(
        "bound", Rasm.Contracts.Ui.V1.MultiPosture.Bound, nameof(MultiComboBox), MultiComboBox.SelectedItemsProperty);
    public static readonly MultiPosture Free = new(
        "free", Rasm.Contracts.Ui.V1.MultiPosture.Free, nameof(TagInput), TagInput.TagsProperty);

    public Rasm.Contracts.Ui.V1.MultiPosture Wire { get; }
    public string Control { get; }
    public AvaloniaProperty Slot { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SegmentPosture {
    // Select coerces to single selection and slides one indicator; Command gives every segment its own verb —
    // the postures differ in what a segment MEANS, never in how it is painted.
    public static readonly SegmentPosture Select = new("select", Rasm.Contracts.Ui.V1.SegmentPosture.Select, nameof(SelectionList));
    public static readonly SegmentPosture Command = new("command", Rasm.Contracts.Ui.V1.SegmentPosture.Command, nameof(ButtonGroup));

    public Rasm.Contracts.Ui.V1.SegmentPosture Wire { get; }
    public string Control { get; }
}

// Each chip posture MINTS its own host and names its own value slot, so the string-keyed dispatch the old
// construct arm ran over posture keys is a generated row read.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChipPosture {
    public static readonly ChipPosture Static = new(
        "static", Rasm.Contracts.Ui.V1.ChipPosture.Static, nameof(ContentControl), Some(ControlSkin.StatusChip),
        ContentControl.ContentProperty, static label => new ContentControl { Content = label });
    public static readonly ChipPosture Toggle = new(
        "toggle", Rasm.Contracts.Ui.V1.ChipPosture.Toggle, nameof(ToggleButton), Option<ControlSkin>.None,
        ToggleButton.IsCheckedProperty, static label => new ToggleButton { Content = label });
    public static readonly ChipPosture Removable = new(
        "removable", Rasm.Contracts.Ui.V1.ChipPosture.Removable, nameof(ClosableTag), Option<ControlSkin>.None,
        ContentControl.ContentProperty, static label => new ClosableTag { Content = label });

    public Rasm.Contracts.Ui.V1.ChipPosture Wire { get; }
    public string Control { get; }
    public Option<ControlSkin> Skin { get; }
    public AvaloniaProperty Slot { get; }

    [UseDelegateFromConstructor]
    public partial Control Mint(string label);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColorPosture {
    public static readonly ColorPosture Inline = new("inline", Rasm.Contracts.Ui.V1.ColorPosture.Inline, nameof(ColorView));
    public static readonly ColorPosture Flyout = new("flyout", Rasm.Contracts.Ui.V1.ColorPosture.Flyout, nameof(ColorPicker));

    public Rasm.Contracts.Ui.V1.ColorPosture Wire { get; }
    public string Control { get; }
}

// Severity carries its own DISMISSIBILITY, because non-dismissible is a posture of the severity and never a
// caller's flag: a condition the operator cannot clear is exactly the condition a close button would lie about.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BannerSeverity {
    public static readonly BannerSeverity Information = new(
        "information", Rasm.Contracts.Ui.V1.BannerSeverity.Information, NotificationType.Information, dismissible: true);
    public static readonly BannerSeverity Success = new(
        "success", Rasm.Contracts.Ui.V1.BannerSeverity.Success, NotificationType.Success, dismissible: true);
    public static readonly BannerSeverity Warning = new(
        "warning", Rasm.Contracts.Ui.V1.BannerSeverity.Warning, NotificationType.Warning, dismissible: true);
    public static readonly BannerSeverity Error = new(
        "error", Rasm.Contracts.Ui.V1.BannerSeverity.Error, NotificationType.Error, dismissible: false);

    public Rasm.Contracts.Ui.V1.BannerSeverity Wire { get; }
    public NotificationType Type { get; }
    public bool Dismissible { get; }
}

// Placement is CHROME, not layout: this row says whether the strip bleeds to the page edge or insets inside a
// section — the edge treatment the control theme selects on and nothing the parent panel can express.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BannerPlacement {
    public static readonly BannerPlacement Page = new("page", Rasm.Contracts.Ui.V1.BannerPlacement.Page);
    public static readonly BannerPlacement Section = new("section", Rasm.Contracts.Ui.V1.BannerPlacement.Section);

    public Rasm.Contracts.Ui.V1.BannerPlacement Wire { get; }
}

// Bar and Ring are ONE control under two themes; Skeleton is a different fact — a shimmer standing in for
// content that has not arrived — so it carries no fraction and no value slot at all.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProgressForm {
    public static readonly ProgressForm Bar = new(
        "bar", Rasm.Contracts.Ui.V1.ProgressForm.Bar, nameof(ProgressBar), Some((AvaloniaProperty)RangeBase.ValueProperty));
    public static readonly ProgressForm Ring = new(
        "ring", Rasm.Contracts.Ui.V1.ProgressForm.Ring, nameof(ProgressBar), Some((AvaloniaProperty)RangeBase.ValueProperty));
    public static readonly ProgressForm Skeleton = new(
        "skeleton", Rasm.Contracts.Ui.V1.ProgressForm.Skeleton, nameof(Skeleton), Option<AvaloniaProperty>.None);

    public Rasm.Contracts.Ui.V1.ProgressForm Wire { get; }
    public string Control { get; }
    public Option<AvaloniaProperty> Slot { get; }
}

// The temporal capability axis: Calendar admits blackout bounds, Upper admits a second value slot. Only the
// four declared TemporalKind rows mint sets, so an upper slot without a calendar is unspellable.
// Rank IS declaration order (kernel CapabilityRank law) — the attribute pins the roster against a reorder pass.
[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TemporalTrait : ICapability<TemporalTrait> {
    public static readonly TemporalTrait Calendar = new("calendar");
    public static readonly TemporalTrait Upper = new("upper");
}

// Each row constructs its own picker and names the value slot the two-way binding rides; the Span row alone
// declares its upper slot, and the time row's empty trait set is why a bounded time entry refuses at
// materialize instead of silently dropping its bounds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TemporalKind {
    public static readonly TemporalKind Date = new("date", Rasm.Contracts.Ui.V1.TemporalKind.Date, nameof(DateOnlyPicker),
        static () => new DateOnlyPicker(), DatePickerBase<DateOnly>.SelectedDateProperty,
        CapabilitySet<TemporalTrait>.Of(TemporalTrait.Calendar), Option<AvaloniaProperty>.None);
    public static readonly TemporalKind Time = new("time", Rasm.Contracts.Ui.V1.TemporalKind.Time, nameof(TimeOnlyPicker),
        static () => new TimeOnlyPicker(), TimePickerBase<TimeOnly>.SelectedTimeProperty,
        CapabilitySet<TemporalTrait>.None, Option<AvaloniaProperty>.None);
    public static readonly TemporalKind Moment = new("datetime", Rasm.Contracts.Ui.V1.TemporalKind.Datetime, nameof(DateTimePicker),
        static () => new DateTimePicker(), DateTimePickerBase<DateTime>.SelectedDateProperty,
        CapabilitySet<TemporalTrait>.Of(TemporalTrait.Calendar), Option<AvaloniaProperty>.None);
    public static readonly TemporalKind Span = new("range", Rasm.Contracts.Ui.V1.TemporalKind.Range, nameof(DateOnlyRangePicker),
        static () => new DateOnlyRangePicker(), DateRangePickerBase<DateOnly>.SelectedStartDateProperty,
        CapabilitySet<TemporalTrait>.All, Some((AvaloniaProperty)DateRangePickerBase<DateOnly>.SelectedEndDateProperty));

    public Rasm.Contracts.Ui.V1.TemporalKind Wire { get; }
    public string Control { get; }
    // The spinner and picker bases register their value property PER CLOSED GENERIC, so the slot is a row
    // column rather than one shared field a type probe could recover.
    public AvaloniaProperty Slot { get; }
    public CapabilitySet<TemporalTrait> Traits { get; }
    public Option<AvaloniaProperty> UpperSlot { get; }

    [UseDelegateFromConstructor]
    public partial Control Construct();
}

// Bounds cross in the WIDEST form each numeric family admits, so no field pays another family's ceiling: the
// real arm keeps the full binary64 range and NaN, the unsigned arm keeps its top decade, and the precise arm
// keeps decimal significance.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NumericRange {
    private NumericRange() { }

    public sealed record Integral(long Min, long Max, long Step) : NumericRange;
    public sealed record Unsigned(ulong Min, ulong Max, ulong Step) : NumericRange;
    public sealed record Real(double Min, double Max, double Step) : NumericRange;
    public sealed record Precise(decimal Min, decimal Max, decimal Step) : NumericRange;
}

// The typed numeric family: one row per CLR type the spinner suite admits (`.api/api-ursa.md` roster), each
// row constructing and re-dressing its own spinner through ONE generic-math narrowing owner — eleven types
// cost eleven addresses, not eleven conversion bodies. Every spinner styles as the shared spinner key by its
// own StyleKeyOverride.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NumericKind {
    public static readonly NumericKind Byte = Row(
        "byte", Rasm.Contracts.Ui.V1.NumericKind.Byte, nameof(NumericByteUpDown), Slot<byte>(),
        Spin<NumericByteUpDown, byte>, Dress<NumericByteUpDown, byte>);
    public static readonly NumericKind SByte = Row(
        "sbyte", Rasm.Contracts.Ui.V1.NumericKind.Sbyte, nameof(NumericSByteUpDown), Slot<sbyte>(),
        Spin<NumericSByteUpDown, sbyte>, Dress<NumericSByteUpDown, sbyte>);
    public static readonly NumericKind Short = Row(
        "short", Rasm.Contracts.Ui.V1.NumericKind.Short, nameof(NumericShortUpDown), Slot<short>(),
        Spin<NumericShortUpDown, short>, Dress<NumericShortUpDown, short>);
    public static readonly NumericKind UShort = Row(
        "ushort", Rasm.Contracts.Ui.V1.NumericKind.Ushort, nameof(NumericUShortUpDown), Slot<ushort>(),
        Spin<NumericUShortUpDown, ushort>, Dress<NumericUShortUpDown, ushort>);
    public static readonly NumericKind Int = Row(
        "int", Rasm.Contracts.Ui.V1.NumericKind.Int, nameof(NumericIntUpDown), Slot<int>(),
        Spin<NumericIntUpDown, int>, Dress<NumericIntUpDown, int>);
    public static readonly NumericKind UInt = Row(
        "uint", Rasm.Contracts.Ui.V1.NumericKind.Uint, nameof(NumericUIntUpDown), Slot<uint>(),
        Spin<NumericUIntUpDown, uint>, Dress<NumericUIntUpDown, uint>);
    public static readonly NumericKind Long = Row(
        "long", Rasm.Contracts.Ui.V1.NumericKind.Long, nameof(NumericLongUpDown), Slot<long>(),
        Spin<NumericLongUpDown, long>, Dress<NumericLongUpDown, long>);
    public static readonly NumericKind ULong = Row(
        "ulong", Rasm.Contracts.Ui.V1.NumericKind.Ulong, nameof(NumericULongUpDown), Slot<ulong>(),
        Spin<NumericULongUpDown, ulong>, Dress<NumericULongUpDown, ulong>);
    public static readonly NumericKind Float = Row(
        "float", Rasm.Contracts.Ui.V1.NumericKind.Float, nameof(NumericFloatUpDown), Slot<float>(),
        Spin<NumericFloatUpDown, float>, Dress<NumericFloatUpDown, float>);
    public static readonly NumericKind Double = Row(
        "double", Rasm.Contracts.Ui.V1.NumericKind.Double, nameof(NumericDoubleUpDown), Slot<double>(),
        Spin<NumericDoubleUpDown, double>, Dress<NumericDoubleUpDown, double>);
    public static readonly NumericKind Decimal = Row(
        "decimal", Rasm.Contracts.Ui.V1.NumericKind.Decimal, nameof(NumericDecimalUpDown), Slot<decimal>(),
        Spin<NumericDecimalUpDown, decimal>, Dress<NumericDecimalUpDown, decimal>);

    public Rasm.Contracts.Ui.V1.NumericKind Wire { get; }
    public string Control { get; }
    // Per-closed-generic value property; the float and double rows additionally pass NaN through their own
    // coercion override, so an unset scientific field stays unset instead of snapping to a bound.
    public AvaloniaProperty Value { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Control> Construct(NumericRange range);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Redress(Control control, NumericRange range);

    static NumericKind Row(
        string key,
        Rasm.Contracts.Ui.V1.NumericKind wire,
        string control,
        AvaloniaProperty slot,
        Func<NumericRange, Fin<Control>> construct,
        Func<Control, NumericRange, Fin<Unit>> redress) =>
        new(key, wire, control, slot, construct, redress);

    static AvaloniaProperty Slot<T>() where T : struct, IComparable<T> => NumericUpDownBase<T>.ValueProperty;

    static Fin<Control> Spin<TControl, T>(NumericRange range)
        where TControl : NumericUpDownBase<T>, new()
        where T : struct, IComparable<T>, INumberBase<T> =>
        Narrow<T>(range).Map(static bound => (Control)new TControl { Minimum = bound.Min, Maximum = bound.Max, Step = bound.Step });

    static Fin<Unit> Dress<TControl, T>(Control control, NumericRange range)
        where TControl : NumericUpDownBase<T>
        where T : struct, IComparable<T>, INumberBase<T> =>
        control is TControl typed
            ? Narrow<T>(range).Map(bound => {
                typed.Minimum = bound.Min;
                typed.Maximum = bound.Max;
                typed.Step = bound.Step;
                return unit;
            })
            : Fin<Unit>.Fail(new ControlFault.RecyclingViolation(typeof(TControl).Name));

    // ONE narrowing owner for every kind and every range form: generic-math CHECKED conversion, so a bound the
    // bound type cannot hold seals a typed payload refusal at materialize instead of wrapping silently.
    static Fin<(T Min, T Max, T Step)> Narrow<T>(NumericRange range) where T : INumberBase<T> =>
        Op.Of(name: "appui.control.narrow").Catch(() => Fin.Succ(range.Switch(
                integral: static row => (T.CreateChecked(row.Min), T.CreateChecked(row.Max), T.CreateChecked(row.Step)),
                unsigned: static row => (T.CreateChecked(row.Min), T.CreateChecked(row.Max), T.CreateChecked(row.Step)),
                real: static row => (T.CreateChecked(row.Min), T.CreateChecked(row.Max), T.CreateChecked(row.Step)),
                precise: static row => (T.CreateChecked(row.Min), T.CreateChecked(row.Max), T.CreateChecked(row.Step)))));
}
```

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// The icon slot carries its own pending key, so in-control progress can only ever REPLACE a leading visual.
// Size is a `MetricFamily.Icon` STEP, never a pixel count — the asset rail resolves the step against the live
// density, text scale, and contrast projection.
[ValueObject<uint>]
public readonly partial struct IconStep {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value > 0u ? validationError : new ValidationError("icon metric step must be positive");
}

public sealed record IconSlot(AssetKey Asset, Position Placement, IconStep Size, Option<string> Pending);

// One hint shape serving both uses: the standalone Tooltip case materializes it as a body, and the binding's
// hint column attaches that same body to any control — exactly one construction site.
public sealed record HintRow(string Body, Option<KeyGesture> Gesture);

// No automation-name column: the announced name derives from Key through the one locale label resolver, so an
// authored per-intent literal cannot drift from the id the same fold stamps.
public sealed record IntentBinding(
    PaintRole Role,
    ControlEmphasis Emphasis,
    Option<string> Command,
    Option<string> ValueKey,
    Option<ControlTrigger> Trigger,
    Option<IconSlot> Icon,
    Option<HintRow> Hint) {
    public static IntentBinding Of(PaintRole role, ControlEmphasis? emphasis = null) =>
        new(role, emphasis ?? ControlEmphasis.Secondary, None, None, None, None, None);
}

// An OPTION row: its Value round-trips through the host's SelectedValueBinding — the payload-timing
// discriminant that keeps this row beside CrumbRow (same field-set, command-dispatched consumer).
public sealed record OptionRow(string Value, string LabelKey, Option<string> Group, Option<IconSlot> Icon);

// The ONE resolved projection every package binding triple reads member paths off — a label key bound straight
// into a segment would paint the key itself, and an icon slot has no image until the asset rail answers. The
// option consumers pass None for the icon.
public sealed record BoundView(string Label, string Value, Option<IImage> Icon);

// Inline options are the option set itself; a bound source NAMES a screen-owned collection the window fabric
// realizes. Both reach the fold as ONE `Shell/virtualization` `WindowLease<OptionRow>`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionSource {
    private OptionSource() { }

    public sealed record Inline(Seq<OptionRow> Rows) : OptionSource;
    public sealed record Bound(string SourceKey) : OptionSource;
}

// A real column row: the header key resolves through the label resolver, the cell intent materializes through
// this same fold, the extent is the grid's own length algebra, and the editor is the intent the EDITING
// template materializes — its absence IS the read-only verdict.
public sealed record ColumnRow(
    string HeaderKey,
    ControlIntent Cell,
    Option<ControlIntent> Editor,
    DataGridLength Extent,
    Option<string> SortKey,
    HorizontalAlignment Align);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MenuPosture {
    public static readonly MenuPosture Command = new(
        "command", Rasm.Contracts.Ui.V1.MenuPosture.Command, Some(MenuItemToggleType.None));
    public static readonly MenuPosture Check = new(
        "check", Rasm.Contracts.Ui.V1.MenuPosture.Check, Some(MenuItemToggleType.CheckBox));
    public static readonly MenuPosture Radio = new(
        "radio", Rasm.Contracts.Ui.V1.MenuPosture.Radio, Some(MenuItemToggleType.Radio));
    public static readonly MenuPosture Divider = new(
        "separator", Rasm.Contracts.Ui.V1.MenuPosture.Separator, Option<MenuItemToggleType>.None);

    public Rasm.Contracts.Ui.V1.MenuPosture Wire { get; }
    // The separator row carries no toggle because it carries no item at all — the None IS the discriminant the
    // fold reads to construct a rule instead of a menu item.
    public Option<MenuItemToggleType> Toggle { get; }
}

// A menu row is a ROW, never a child intent: check posture, submenu, gesture hint, and icon are fields no
// control intent owns, and a submenu is this same row shape one level down.
public sealed record MenuRow(
    string Key,
    string LabelKey,
    MenuPosture Posture,
    Option<IconSlot> Icon,
    Option<KeyGesture> Gesture,
    Option<string> Command,
    Option<string> CheckedKey,
    Seq<MenuRow> Rows);

public sealed record ToolbarRow(ControlIntent Item, OverflowMode Overflow);

// A CRUMB row: its Value dispatches through the host's CommandParameterBinding — the consumer discriminant
// that keeps this row beside OptionRow.
public sealed record CrumbRow(string Value, string LabelKey, Option<IconSlot> Icon, Option<string> Command);

public sealed record AvatarRow(string LabelKey, Option<AssetKey> Portrait);

[ValueObject<uint>]
public readonly partial struct AvatarLimit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value > 0u && value <= (uint)int.MaxValue
            ? validationError
            : new ValidationError("avatar visible limit must fit a positive Int32");
}

// The picker filter grammar is the package's own bracketed form, so the row is TYPED here and encoded at the
// edge: a label carrying the grammar's reserved characters refuses at materialize rather than throwing inside
// the picker launch on a user gesture.
public sealed record FileFilterRow(string Label, Seq<string> Patterns) {
    static readonly FrozenSet<char> Reserved = new[] { '*', '.', ',', '[', ']' }.ToFrozenSet();

    public Fin<string> Encode() =>
        Label.Any(Reserved.Contains) || Patterns.IsEmpty
            ? Fin<string>.Fail(new ControlFault.PayloadRejected($"filter {Label}"))
            : Fin.Succ($"[{Label},{string.Join(',', Patterns)}]");
}

// The ONE per-case answer set, four columns in one read: `Parked` names the type a recycled control must
// already be (None = unpoolable), `Skin` the control-theme row (None = shipped type-keyed theme), `Slot` the
// value property a ValueKey binds (None = no value channel), `Redress` the in-place re-dress a parked control
// takes (present exactly on the Parked set). One total ShapeOf switch answers all four, so the re-dress
// ladder, the content-property ladder, and the host-name method this page once kept as three more mirrors of
// the same roster are unspellable. NAMED LOSS: the (intent, control) tuple pattern's compile-time control-type
// check on each re-dress body — bought back by Rebind comparing the parked type name to `Parked` before any
// Redress runs, and by each Redress body's own typed probe sealing RecyclingViolation.
public sealed record ControlShape(
    Option<string> Parked,
    Option<ControlSkin> Skin,
    Option<AvaloniaProperty> Slot,
    Option<Func<ControlIntent, Control, MaterializeContext, Fin<Control>>> Redress);
```

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlIntent(string Key, IntentBinding Binding) {
    public sealed record Button(string Key, string LabelKey, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Label(string Key, string TextKey, TypographyRole Role, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record TextInput(string Key, string Watermark, bool Multiline, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record NumberInput(string Key, NumericKind Kind, NumericRange Range, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record DateInput(string Key, TemporalKind Kind, Option<LocalDate> From, Option<LocalDate> Until, Option<string> UpperKey, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record PathInput(string Key, UsePickerTypes Mode, Seq<FileFilterRow> Filters, bool Multiple, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record ColorInput(string Key, ColorPosture Posture, bool Alpha, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Select(string Key, SelectPosture Posture, OptionSource Options, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record MultiSelect(string Key, MultiPosture Posture, OptionSource Options, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Slider(string Key, double Min, double Max, double Step, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Range(string Key, double Min, double Max, double Step, string UpperKey, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Toggle(string Key, string LabelKey, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Radio(string Key, Seq<OptionRow> Options, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Segmented(string Key, SegmentPosture Posture, Seq<OptionRow> Options, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Chip(string Key, string TextKey, ChipPosture Posture, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Progress(string Key, ProgressForm Form, Option<double> Fraction, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Avatar(
        string Key, Seq<AvatarRow> Members, AvatarLimit VisibleLimit, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Breadcrumb(string Key, Seq<CrumbRow> Crumbs, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Tooltip(string Key, HintRow Hint, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Banner(string Key, string HeadlineKey, string BodyKey, BannerSeverity Severity, BannerPlacement Placement, Seq<ControlIntent> Actions, Option<ControlIntent> Evidence, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record EmptyState(string Key, string HeadlineKey, string BodyKey, Option<ControlIntent> Action, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Grid(string Key, Seq<ColumnRow> Columns, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Tree(string Key, ControlIntent Item, string ExpansionCommand, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Overview(string Key, OverviewAxis Axis, string SourceKey, string JumpCommand, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Menu(string Key, Seq<MenuRow> Rows, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Toolbar(string Key, Seq<ToolbarRow> Rows, Orientation Orientation, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Tab(string Key, Seq<(string HeaderKey, ControlIntent Body)> Pages, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Accordion(string Key, Seq<(string HeaderKey, ControlIntent Body)> Sections, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Panel(string Key, Seq<ControlIntent> Children, string ConstraintProgram, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Dock(string Key, Seq<ControlIntent> Regions, string ConstraintProgram, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Splitter(string Key, ControlIntent First, ControlIntent Second, Orientation Orientation, IntentBinding Binding) : ControlIntent(Key, Binding);

    // Layout children alone, TOTAL over the family: a row shape (menu row, crumb, option, avatar member) never
    // appears here, so a walk can never mistake a gesture hint for a mountable control, and a NEW CONTAINER
    // case breaks this fold at compile time — the one failure mode a leaf-defaulting virtual member would turn
    // into a silently unmaterialized child tree. The grid contributes its cell AND its editor, because an
    // editing template a walk never reaches is exactly the template that ships unmaterialized.
    public Seq<ControlIntent> Children => Switch(
        grid: static c => c.Columns.Bind(static column => Seq(column.Cell) + column.Editor.ToSeq()),
        tree: static c => Seq(c.Item),
        toolbar: static c => c.Rows.Map(static row => row.Item),
        tab: static c => c.Pages.Map(static page => page.Body),
        accordion: static c => c.Sections.Map(static section => section.Body),
        emptyState: static c => c.Action.ToSeq(),
        // A banner's verbs and its evidence are CHILD INTENTS, so their command keys resolve through the same
        // deck every other button takes — a banner-local verb roster would be a second availability algebra.
        banner: static c => c.Actions + c.Evidence.ToSeq(),
        panel: static c => c.Children,
        dock: static c => c.Regions,
        splitter: static c => Seq(c.First, c.Second),
        button: static _ => Seq<ControlIntent>(), label: static _ => Seq<ControlIntent>(),
        textInput: static _ => Seq<ControlIntent>(), numberInput: static _ => Seq<ControlIntent>(),
        dateInput: static _ => Seq<ControlIntent>(), pathInput: static _ => Seq<ControlIntent>(),
        colorInput: static _ => Seq<ControlIntent>(), select: static _ => Seq<ControlIntent>(),
        multiSelect: static _ => Seq<ControlIntent>(), slider: static _ => Seq<ControlIntent>(),
        range: static _ => Seq<ControlIntent>(), toggle: static _ => Seq<ControlIntent>(),
        radio: static _ => Seq<ControlIntent>(), segmented: static _ => Seq<ControlIntent>(),
        chip: static _ => Seq<ControlIntent>(), progress: static _ => Seq<ControlIntent>(),
        avatar: static _ => Seq<ControlIntent>(), breadcrumb: static _ => Seq<ControlIntent>(),
        tooltip: static _ => Seq<ControlIntent>(), menu: static _ => Seq<ControlIntent>(),
        overview: static _ => Seq<ControlIntent>());
}
```

## [03]-[MATERIALIZE_FOLD]

- Owner: `ControlFactory` the one intent-to-control fold; `MaterializeContext` the composition-bound resolution columns; `ControlReceipt` the materialization evidence record.
- Law: every `ICommand` rides `BehaviorRail.Intent` — a `BindCommand` call site is the deleted form and the intent never carries a live command, only its key; a bound command ALWAYS attaches and the trigger column narrows which gesture raises it, so a control that resolved a verb it could never be invoked through is unrepresentable.
- Law: the fold writes NO resolved appearance value. Emphasis and posture resolve one `ControlSkin` row onto `StyledElement.Theme`, the semantic `PaintRole` key and the `TypographyRole` key land as style classes, and the control theme's own `{DynamicResource}` setters carry every brush, metric, radius, and shadow.
- Law: every control's id and name derive from `ControlIntent.Key` through the one `Apply` fold — the id is the key verbatim and the name is `MaterializeContext.Label(key)` off the composition-bound `Theme/locale` resolver.
- Law: container arms recurse `Materialize` over child intents so a whole screen is one fold over one nested intent tree.
- Entry: `public Fin<Control> Materialize(ControlIntent intent, MaterializeContext context)` — one polymorphic fold over the closed family; the `Fin` rail aborts on an unbound command key, an unresolved skin row, an unavailable slot, a refused payload, or a recycling violation, sealing the typed `ControlFault`.
- Auto: each arm constructs the compiled-template control its case names — the Avalonia core rows for text, content, choice, range, toggle, grid, tree, menu, tab, expander, and colour surfaces, and the Ursa rows for the families that roster lacks — binds its `ICommand` through `BehaviorRail.Intent(context.Command(key))` exclusively, resolves its skin through `context.Skin(row)`, derives automation identity from the intent key, and admits values, activation, icons, and pending state through the typed `MaterializeContext` boundaries; no reflection path, per-kind materializer call site, runtime-XAML emission, or second binding bridge exists.
- Receipt: `ControlReceipt` — intent key, control type name, bound command key, resolved emphasis, `Instant` — minted by `Materialize` on every successful fold through the `MaterializeContext.Evidence` column bound at composition to the screen evidence stream; `TelemetryRow` contributes the control-materialized and control-rejected instruments inward through the AppHost `TelemetryContributorPort`.
- Packages: Avalonia, Avalonia.Controls.DataGrid, Avalonia.Controls.ColorPicker, Irihi.Ursa, Irihi.Ursa.Themes.Semi, Xaml.Behaviors.Avalonia, ReactiveUI, LanguageExt.Core, NodaTime
- Growth: one fold arm and one `ShapeOf` arm per new `ControlIntent` case; a new container is one nesting arm recursing `Materialize`; one control instrument is one `InstrumentSpec` row on `ControlFactory.TelemetryRow`; zero new surface.
- Boundary: `ControlFactory` is the named boundary capsule for the control-construction statement carve-out — each arm carries the control-construction statements while the dispatch stays one total generated `Switch`, so a new case breaks every site at compile time and a runtime `_` arm is the rejected form; the only `ICommand` binding bridge is `BehaviorRail.Intent`, so `PropertyBinderImplementation.Bind`/`OneWayBind`/`BindTo`, `CommandBinder.BindCommand`, and `IViewFor` property-expression wiring are rejected wholesale (the `[04]-[BOUNDARIES]` ReactiveUI-code-behind clause); the materialized control's value bridge resolves the typed `IntentBinding.ValueKey` against the `ShapeOf` row's OWN `Slot` through `MaterializeContext.Value`, never reflection over a string property path — a ValueKey on a case whose row declares no slot refuses as `SlotUnavailable`; each arm binds its compiled template through `TemplatedControl.Template` and its theme through `StyledElement.Theme`, the grid cell intents bind `DataGridTemplateColumn.CellTemplate`/`CellEditingTemplate`, and only runtime `AvaloniaRuntimeXamlLoader` inflation is rejected by `Surfaces.RejectRuntimeInflation`; the `Grid`, `Tree`, `Select`, and `MultiSelect` arms hand their `VirtualWindowSpec` to the `Shell/virtualization` `VirtualWindow` owner and take back the fabric's own `WindowLease<TView>`, so no arm mints a second lease record and no arm binds a raw change-set to `ItemsSource`; the `Overview` arm resolves its `OverviewFrame` stream through the named source column, so the downsample, the decoration lanes, and the drag-to-jump conversion all live at the one `Shell/virtualization` `OverviewScale` owner; a control that publishes a typed gesture VALUE resolves its verb through `MaterializeContext.Gesture` rather than `Command` — the arrow lowers the value onto an existing payload case at the raising surface so the verb stays a deck row; the tree indent rides the shipped level-to-padding multi-value converter over a `{DynamicResource}` indent thickness; the `Panel` and `Dock` arms hand their `ConstraintProgram` to the `Shell/solver` `LayoutSolver` panel and mount their children through `Mounted`, which stamps `LayoutSolver.ChildKeyProperty` from each child intent's `Key` before the child enters `Children`; the command key resolves against the boot-frozen `CommandDeck` so an unknown key aborts the materialize on the `Fin` rail rather than binding a dead control; a resolved icon image is a `Theme/tokens` `Rematerialize.TintedAsset` roster member, so a theme swap rebuilds it through the swap's roster rather than through a second icon subscription here.

[CONTEXT_COLUMNS]: a column exists only where the fold CANNOT construct the value — a third-party or sibling owner holds it, or the host supplies it. Everything else constructs in the arm. `Own`/`Release` are the two verbs of ONE activation-scope custody the host alone owns; `Options`/`Window` are two lease doors whose element types no single non-generic column could carry — both discriminants named here so neither pair reads as an accidental twin.

| [INDEX] | [COLUMN]   | [EARNED_BY]      | [OWNER_IT_DEFERS_TO]                                                       |
| :-----: | :--------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `Command`  | boot-frozen deck | `Shell/commands` `CommandDeck` — the one verb registry                     |
|  [02]   | `Skin`     | sibling owner    | `Theme/tokens#CONTROL_THEMES` — the control-theme table                    |
|  [03]   | `Label`    | host locale      | `Theme/locale` resolver — the announced name and every visible caption     |
|  [04]   | `Icon`     | sibling owner    | `Theme/assets` `IconSurface.Resolve` — the one ranked asset rail           |
|  [05]   | `Options`  | sibling owner    | `Shell/virtualization` `VirtualWindow` over a screen-owned option source   |
|  [06]   | `Window`   | sibling owner    | `Shell/virtualization` `VirtualWindow` — the one realized-item fabric      |
|  [07]   | `Layout`   | sibling owner    | `Shell/solver` `LayoutSolver` — the one constraint panel                   |
|  [08]   | `Gesture`  | raising surface  | the surface's own lifting arrow over a deck row (`HistoryIntents.Scrub`)   |
|  [09]   | `Value`    | screen state     | `Shell/screens` two-way value channel over a named property slot           |
|  [10]   | `Activate` | third-party rail | `BehaviorRail.Intent` over `Xaml.Behaviors.Avalonia` — the one command hop |
|  [11]   | `Own`      | host lifetime    | the surface activation scope that disposes every bound lifetime            |
|  [12]   | `Release`  | host lifetime    | the same scope, releasing a parked control's lifetimes before reuse        |
|  [13]   | `Evidence` | evidence stream  | `Diagnostics/evidence` receipt sink bound at composition                   |
|  [14]   | `Clock`    | host clock       | the composed `IClock` — the one instant every receipt stamps               |
|  [15]   | `Overview` | sibling owner    | `Shell/virtualization` `OverviewFrame` over a screen-owned strip producer  |

[PACKAGE_ADMISSION]: every extended-control candidate is admitted at a named case, seated at the page that mounts it as a boundary capsule, or refused with its reason; absence is closed, never silent. A SEATED row is neither of the other two on purpose — the control ships and is used, but its value is not a schema field; the chord capture cell is the one such row.

| [INDEX] | [CANDIDATE]                 | [VERDICT] | [SEAT_OR_REASON]                                                       |
| :-----: | :-------------------------- | :-------- | :--------------------------------------------------------------------- |
|  [01]   | typed numeric spinner suite | admitted  | `NumberInput` — one row per CLR type under the typed-numeric law       |
|  [02]   | temporal picker family      | admitted  | `DateInput` — one row per `TemporalKind`                               |
|  [03]   | path picker                 | admitted  | `PathInput` — mode, filter grammar, and multiplicity as styled policy  |
|  [04]   | overflow tool bar           | admitted  | `Toolbar` — per-item overflow mode into the package's overflow well    |
|  [05]   | selection list              | admitted  | `Segmented` select posture — coerced single selection, sliding rail    |
|  [06]   | button group                | admitted  | `Segmented` command posture — one binding triple projects every item   |
|  [07]   | multi-select combo          | admitted  | `MultiSelect` bound posture — chip readout over a closed option set    |
|  [08]   | tag input                   | admitted  | `MultiSelect` free posture — the arbitrary-token half a picker lacks   |
|  [09]   | range slider                | admitted  | `Range` — two thumbs, two value keys                                   |
|  [10]   | breadcrumb                  | admitted  | `Breadcrumb` — item graph projected through the package binding triple |
|  [11]   | avatar                      | admitted  | `Avatar` — single member and cluster-with-overflow on one case         |
|  [12]   | closable tag                | admitted  | `Chip` removable posture — the dismiss command is the tag's own        |
|  [13]   | skeleton                    | admitted  | `Progress` skeleton form — shimmer standing in for absent content      |
|  [14]   | inline banner               | admitted  | `Banner` — the persistent condition strip a transient note cannot own  |
|  [15]   | badge                       | refused   | WRAPPER host type diverges from the intent's — the pool key lies       |
|  [16]   | busy overlay and its glyph  | refused   | in-control progress IS the icon slot's pending key                     |
|  [17]   | inline-markdown text block  | refused   | its parser reaches inline spans alone; document markdown owns the rest |
|  [18]   | message box                 | refused   | duplicates the `Shell/dialogs` confirm session under a second stack    |
|  [19]   | pagination strip            | refused   | discrete paging is a source-side window at the tables projection fold  |
|  [20]   | segmented pin-code entry    | refused   | a fixed-arity code field is `TextInput` with a commit trigger          |
|  [21]   | on-screen numeric keypad    | refused   | a host input surface, not a control intent; touch entry rides the host |
|  [22]   | masked address entry        | refused   | an address is `TextInput` under a domain admission rule                |
|  [23]   | rating input                | refused   | a bounded ordinal is `Segmented` or `Slider` with a discrete step      |
|  [24]   | marquee ticker              | refused   | continuous motion belongs to the motion vocabulary, never a control    |
|  [25]   | search input                | refused   | `TextInput`, leading icon slot, change trigger — no field is added     |
|  [26]   | split button                | refused   | `Menu` whose root row carries the primary command, submenu the rest    |
|  [27]   | chord capture cell          | seated    | `Shell/commands#BINDING_EDITOR` `KeycapCell` mounts `KeyGestureInput`  |

```csharp signature
// --- [SERVICES] -----------------------------------------------------------------------------
// The Icon column's int is a `MetricFamily.Icon` STEP. Composition binds it as one fold over the ranked asset
// rail — `IconSurface.Resolve(runtime, new AssetRequest(key, step, scale, flow, new GlyphForm.Image()),
// resolved)` — so the fold below asks for a glyph and never learns the resolve's scale, flow, cache, or
// product form.
public sealed record MaterializeContext(
    Func<string, Option<ICommand>> Command,
    Func<ControlSkin, Option<ControlTheme>> Skin,
    Func<string, string> Label,
    Func<AssetKey, int, Fin<IImage>> Icon,
    Func<OptionSource, VirtualWindowSpec, Fin<WindowLease<OptionRow>>> Options,
    Func<VirtualWindowSpec, Fin<WindowLease<RealizedItem<object>>>> Window,
    Func<string, Fin<IObservable<OverviewFrame>>> Overview,
    Func<string, Fin<Control>> Layout,
    // The LIFTING arrow for a control that publishes a typed gesture value — a strip's content-space point, a
    // disclosure's own node. A deck row materializes `ReactiveCommand<CommandPayload, DeckReceipt>`, whose
    // execute throws on any parameter outside that payload type; the arrow lowers the value onto an existing
    // payload case at the raising surface, so the verb stays a deck row and the payload union stays closed.
    Func<string, Fin<ICommand>> Gesture,
    Func<string, Control, AvaloniaProperty, Fin<IDisposable>> Value,
    Func<ControlTrigger, Control, ICommand, Fin<IDisposable>> Activate,
    Func<Control, IDisposable, Unit> Own,
    Func<Control, Unit> Release,
    Func<ControlReceipt, Unit> Evidence,
    IClock Clock);

public sealed record ControlReceipt(
    string IntentKey,
    string ControlType,
    Option<string> Command,
    ControlEmphasis Emphasis,
    Instant At);
```

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class ControlFactory {
    public static readonly InstrumentSpec Materialized = InstrumentSpec.Create(
        "rasm.appui.control.materialized", InstrumentKind.Count, MeasureForm.Whole, "{control}",
        "controls materialized by intent case", Seq(AppUiTelemetry.IntentSlot), None, None, None);
    public static readonly InstrumentSpec Rejected = InstrumentSpec.Create(
        "rasm.appui.control.rejected", InstrumentKind.Count, MeasureForm.Whole, "{control}",
        "control intents rejected", Seq<string>(), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Materialized, Rejected);

    // Composition binds this projection beside the context's Evidence column, so both counts derive from the
    // one materialize fold outcome and no dispatch arm touches the meter.
    public static Fin<Unit> Observe(InstrumentSet set, Fin<ControlReceipt> outcome) =>
        outcome.Match(
            Succ: receipt => set.Write(Materialized, 1d,
                InstrumentSet.Tags((AppUiTelemetry.IntentSlot, receipt.IntentKey))),
            Fail: _ => set.Write(Rejected, 1d));

    // Every successful materialization seals its ControlReceipt through the context's evidence column — the
    // one mint the screen evidence stream consumes; a rejected materialize carries its fault only.
    public static Fin<Control> Materialize(ControlIntent intent, MaterializeContext context) =>
        Visual(intent, context)
            .Bind(control => Bind(intent, control, context))
            .Map(control => Sealed(intent, control, context));

    private static Control Sealed(ControlIntent intent, Control control, MaterializeContext context) {
        ignore(context.Evidence(new ControlReceipt(
            intent.Key, control.GetType().Name, intent.Binding.Command, intent.Binding.Emphasis, context.Clock.GetCurrentInstant())));
        return control;
    }

    private static Fin<Control> Visual(ControlIntent intent, MaterializeContext context) => intent.Switch(
        state: context,
        button: static (ctx, c) => Command(c, ctx),
        label: static (ctx, c) => Fin<Control>.Succ(new TextBlock { Text = ctx.Label(c.TextKey), TextWrapping = c.Role.Wraps ? TextWrapping.Wrap : TextWrapping.NoWrap }),
        textInput: static (ctx, c) => Fin<Control>.Succ(new TextBox { Watermark = c.Watermark, AcceptsReturn = c.Multiline }),
        numberInput: static (ctx, c) => c.Kind.Construct(c.Range),
        dateInput: static (ctx, c) => Temporal(c),
        pathInput: static (ctx, c) => Picker(c, ctx),
        colorInput: static (ctx, c) => Fin<Control>.Succ(c.Posture == ColorPosture.Inline
            ? new ColorView { IsAlphaEnabled = c.Alpha, IsAlphaVisible = c.Alpha }
            : new ColorPicker { IsAlphaEnabled = c.Alpha, IsAlphaVisible = c.Alpha }),
        select: static (ctx, c) => Choices(c, ctx),
        multiSelect: static (ctx, c) => Multi(c, ctx),
        slider: static (ctx, c) => Fin<Control>.Succ(new Slider { Minimum = c.Min, Maximum = c.Max, TickFrequency = c.Step }),
        range: static (ctx, c) => Fin<Control>.Succ(new RangeSlider { Minimum = c.Min, Maximum = c.Max, TickFrequency = c.Step, IsSnapToTick = c.Step > 0d }),
        toggle: static (ctx, c) => Fin<Control>.Succ(new ToggleSwitch { Content = ctx.Label(c.LabelKey) }),
        radio: static (ctx, c) => Choose(c, ctx),
        segmented: static (ctx, c) => Segments(c, ctx),
        chip: static (ctx, c) => Fin<Control>.Succ(c.Posture.Mint(ctx.Label(c.TextKey))),
        progress: static (ctx, c) => Meter(c),
        avatar: static (ctx, c) => Faces(c, ctx),
        breadcrumb: static (ctx, c) => Trail(c, ctx),
        tooltip: static (ctx, c) => Fin<Control>.Succ(Hint(c.Hint, ctx)),
        banner: static (ctx, c) => Notice(c, ctx),
        emptyState: static (ctx, c) => Vacant(c, ctx),
        grid: static (ctx, c) => (Table(c, ctx), ctx.Window(c.Window)).Apply((table, lease) => Windowed(table, lease, ctx)).As(),
        tree: static (ctx, c) => (Branches(c, ctx), ctx.Window(c.Window)).Apply((tree, lease) => Windowed(tree, lease, ctx)).As(),
        overview: static (ctx, c) => Strip(c, ctx),
        menu: static (ctx, c) => Rows(c.Rows, ctx).Map(static rows => (Control)new Menu { ItemsSource = rows.ToArray() }),
        toolbar: static (ctx, c) => Bar(c, ctx),
        tab: static (ctx, c) => Pages(c.Pages, ctx).Map(static pages => (Control)new TabControl { ItemsSource = pages.ToArray() }),
        accordion: static (ctx, c) => Sections(c.Sections, ctx).Map(static panels => {
            StackPanel stack = new();
            panels.Iter(panel => stack.Children.Add(panel));
            return (Control)stack;
        }),
        panel: static (ctx, c) => Mounted(ctx.Layout(c.ConstraintProgram), c.Children, ctx),
        dock: static (ctx, c) => Mounted(ctx.Layout(c.ConstraintProgram), c.Regions, ctx),
        splitter: static (ctx, c) => Split(c, ctx));

    // The button family: emphasis picks the control type, the icon slot upgrades a plain button to the
    // icon-leading one, and the link row refuses an icon because its shipped theme already owns a trailing
    // link glyph — two affordances on one surface read as two verbs.
    private static Fin<Control> Command(ControlIntent.Button intent, MaterializeContext context) =>
        (intent.Binding.Emphasis.Iconable, intent.Binding.Icon) switch {
            (false, { IsSome: true }) => Fin<Control>.Fail(new ControlFault.SlotUnavailable($"{intent.Key}:icon")),
            (_, { IsSome: true }) => Fin<Control>.Succ(new IconButton { Content = context.Label(intent.LabelKey) }),
            _ when intent.Binding.Emphasis == ControlEmphasis.Link => Fin<Control>.Succ(new HyperlinkButton { Content = context.Label(intent.LabelKey) }),
            _ => Fin<Control>.Succ(new Button { Content = context.Label(intent.LabelKey) }),
        };

    // One icon read every arm shares, so a menu row, a crumb, and a button resolve their glyph through the
    // same ranked asset rail and an absent slot costs no lookup at all.
    private static Fin<Option<IImage>> Glyph(Option<IconSlot> slot, MaterializeContext context) =>
        slot.Match(
            Some: icon => context.Icon(icon.Asset, icon.Size).Map(Some),
            None: () => Fin<Option<IImage>>.Succ(None));

    // Temporal bounds land as BLACKOUT ranges because the picker family carries no minimum or maximum slot:
    // everything outside the bounds is blacked out. The calendar-less rows refuse bounds here rather than
    // dropping them into a control that could never honour them.
    private static Fin<Control> Temporal(ControlIntent.DateInput intent) =>
        (intent.Kind.Traits.Admits(TemporalTrait.Calendar), intent.From.IsSome || intent.Until.IsSome) switch {
            (false, true) => Fin<Control>.Fail(new ControlFault.PayloadRejected($"{intent.Key}:{intent.Kind.Key} bounds")),
            _ => Fin<Control>.Succ(Blackout(intent.Kind.Construct(), intent)),
        };

    private static Control Blackout(Control control, ControlIntent.DateInput intent) {
        if (control is DatePickerBase calendar) {
            AvaloniaList<DateRange> ranges = [];
            intent.From.Iter(from => ranges.Add(new DateRange(DateTime.MinValue.Date, from.PlusDays(-1).ToDateTimeUnspecified())));
            intent.Until.Iter(until => ranges.Add(new DateRange(until.PlusDays(1).ToDateTimeUnspecified(), DateTime.MaxValue.Date)));
            calendar.BlackoutDates = ranges;
        }
        return control;
    }

    private static Fin<Control> Picker(ControlIntent.PathInput intent, MaterializeContext context) =>
        intent.Filters.Traverse(static filter => filter.Encode()).As().Map(encoded => (Control)new PathPicker {
            UsePickerType = intent.Mode,
            AllowMultiple = intent.Multiple,
            FileFilter = string.Concat(encoded),
            Title = context.Label(intent.Key),
        });

    // The bounded-choice pair survives materialization on the CLOSED posture: each item shows its label and
    // carries its Value on Tag, and SelectedValueBinding resolves the two-way binding against that Tag. Group
    // keys fold into non-selectable header items under the palette-row skin. The EDITABLE posture admits a
    // value outside the set, so it takes resolved views, offers them as type-ahead, and round-trips text.
    private static Fin<Control> Choices(ControlIntent.Select intent, MaterializeContext context) =>
        context.Options(intent.Options, intent.Window).Map(lease => Owned(intent.Posture == SelectPosture.Closed
            ? new ComboBox { ItemsSource = Listed(lease.View, context), SelectedValueBinding = new Binding(nameof(Control.Tag)) }
            : new AutoCompleteBox { ItemsSource = Views(lease.View, context), ValueMemberBinding = new Binding(nameof(BoundView.Label)) },
            lease.Lifetime, context));

    // The multi surface reads its selection back as chips, so grouping has no seat here and the flat resolved
    // view is what the container generator needs.
    private static Fin<Control> Multi(ControlIntent.MultiSelect intent, MaterializeContext context) =>
        intent.Posture == MultiPosture.Free
            ? Fin<Control>.Succ(new TagInput { Watermark = context.Label(intent.Key) })
            : context.Options(intent.Options, intent.Window).Map(lease => Owned(new MultiComboBox {
                ItemsSource = Views(lease.View, context),
                DisplayMemberBinding = new Binding(nameof(BoundView.Label)),
                Watermark = context.Label(intent.Key),
            }, lease.Lifetime, context));

    // ONE ownership hop for every leased or bound lifetime a constructed control must carry.
    private static Control Owned(Control control, IDisposable lifetime, MaterializeContext context) {
        ignore(context.Own(control, lifetime));
        return control;
    }

    // The grouped container projection: a group boundary emits a hit-test-transparent header under the
    // palette-row skin, and every option carries its Value on Tag so the value binding round-trips.
    private static Seq<Control> Listed(ReadOnlyObservableCollection<OptionRow> rows, MaterializeContext context) =>
        toSeq(rows).Fold((Seen: Option<string>.None, Items: Seq<Control>()), (state, row) =>
            row.Group.Filter(group => state.Seen != Some(group)) is { IsSome: true, Case: string fresh }
                ? (Some(fresh), state.Items.Add(Header(fresh, context)).Add(Choice(row, context)))
                : (state.Seen, state.Items.Add(Choice(row, context)))).Items;

    private static Seq<BoundView> Views(ReadOnlyObservableCollection<OptionRow> rows, MaterializeContext context) =>
        toSeq(rows).Map(row => new BoundView(context.Label(row.LabelKey), row.Value, None));

    private static Control Header(string group, MaterializeContext context) {
        ComboBoxItem header = new() { Content = context.Label(group), IsHitTestVisible = false, Focusable = false };
        context.Skin(ControlSkin.PaletteRow).Iter(theme => header.Theme = theme);
        return header;
    }

    private static Control Choice(OptionRow row, MaterializeContext context) =>
        new ComboBoxItem { Content = context.Label(row.LabelKey), Tag = row.Value };

    // Exclusivity has ONE owner and it is the host's selection: each option is its own container carrying the
    // resolved label and its Value on Tag, and SelectedValueBinding resolves against that Tag — so a radio, a
    // drop-down, and a segmented rail all round-trip the option VALUE and never the container. The item theme
    // is resolved and refused on, because a radio row that loses its container theme is a plain list row with
    // no exclusive affordance — the appearance failure a running screen can never report.
    private static Fin<Control> Choose(ControlIntent.Radio intent, MaterializeContext context) =>
        context.Skin(ControlSkin.RadioItem).Match(
            Some: theme => Fin<Control>.Succ(new ListBox {
                ItemsSource = intent.Options.Map(option => Marked(option, theme, context)).ToArray(),
                SelectionMode = SelectionMode.Single,
                SelectedValueBinding = new Binding(nameof(Control.Tag)),
            }),
            None: () => Fin<Control>.Fail(new ControlFault.SkinUnresolved(ControlSkin.RadioItem.Key)));

    private static ListBoxItem Marked(OptionRow option, ControlTheme theme, MaterializeContext context) =>
        new() { Content = context.Label(option.LabelKey), Tag = option.Value, Theme = theme };

    private static Fin<Control> Segments(ControlIntent.Segmented intent, MaterializeContext context) =>
        Fin<Control>.Succ(intent.Posture == SegmentPosture.Select ? Sliding(intent, context) : Grouped(intent, context));

    private static Control Sliding(ControlIntent.Segmented intent, MaterializeContext context) {
        Border indicator = new();
        context.Skin(ControlSkin.SegmentedIndicator).Iter(theme => indicator.Theme = theme);
        Seq<SelectionListItem> segments = intent.Options.Map(option => {
            SelectionListItem segment = new() { Content = context.Label(option.LabelKey), Tag = option.Value };
            context.Skin(ControlSkin.SegmentedItem).Iter(theme => segment.Theme = theme);
            return segment;
        });
        return new SelectionList {
            Indicator = indicator,
            ItemsSource = segments.ToArray(),
            SelectedValueBinding = new Binding(nameof(Control.Tag)),
        };
    }

    private static Control Grouped(ControlIntent.Segmented intent, MaterializeContext context) =>
        new ButtonGroup {
            ItemsSource = intent.Options.Map(option => new BoundView(context.Label(option.LabelKey), option.Value, None)).ToArray(),
            ContentBinding = new Binding(nameof(BoundView.Label)),
            CommandParameterBinding = new Binding(nameof(BoundView.Value)),
        };

    // A fraction's ABSENCE is the indeterminate verdict, so a progress surface never carries a second flag
    // that could disagree with its value. The PENDING copy is `Theme/motion` `LatencyTier`'s, not this fold's.
    private static Fin<Control> Meter(ControlIntent.Progress intent) =>
        Fin<Control>.Succ(intent.Form == ProgressForm.Skeleton
            ? new Skeleton { IsActive = true, IsLoading = true }
            : new ProgressBar {
                Minimum = 0d,
                Maximum = 1d,
                Value = intent.Fraction.IfNone(0d),
                IsIndeterminate = intent.Fraction.IsNone,
                ShowProgressText = intent.Fraction.IsSome,
            });

    // One case covers the single face and the cluster: the visible prefix renders as portraits, the remainder
    // collapses into one overflow face carrying its own count. The cluster hosts inside a templated content
    // control because a bare panel takes setters and no template.
    private static Fin<Control> Faces(ControlIntent.Avatar intent, MaterializeContext context) =>
        intent.Members.Take((int)intent.VisibleLimit.Value)
            .Traverse(member => Face(member, context))
            .As()
            .Map(faces => {
                int hidden = intent.Members.Count - faces.Count;
                Seq<Control> shown = hidden > 0 ? faces.Add(new Avatar { Content = $"+{hidden}" }) : faces;
                StackPanel cluster = new() { Orientation = Orientation.Horizontal };
                shown.Iter(face => cluster.Children.Add(face));
                return shown.Count == 1 ? shown[0] : new ContentControl { Content = cluster };
            });

    private static Fin<Control> Face(AvatarRow member, MaterializeContext context) =>
        member.Portrait.Match(
            Some: asset => context.Icon(asset, PortraitStep).Map(image => (Control)new Avatar { Source = image }),
            None: () => Fin<Control>.Succ(new Avatar { Content = context.Label(member.LabelKey) }));

    // The top rung of the icon axis DERIVES from the family's own step count — a portrait sized by literal
    // would hold its extent across a density election the surface around it followed.
    private static int PortraitStep => MetricFamily.Icon.Steps - 1;

    // The breadcrumb projects its crumb rows through the package's own binding triple, so each entry styles
    // and dispatches as an icon button and the trail mints no per-entry control of its own.
    private static Fin<Control> Trail(ControlIntent.Breadcrumb intent, MaterializeContext context) =>
        intent.Crumbs.Traverse(crumb => Glyph(crumb.Icon, context)
                .Map(icon => new BoundView(context.Label(crumb.LabelKey), crumb.Value, icon)))
            .As()
            .Map(rows => (Control)new Breadcrumb {
                ItemsSource = rows.ToArray(),
                DisplayMemberBinding = new Binding(nameof(BoundView.Label)),
                IconBinding = new Binding(nameof(BoundView.Icon)),
                CommandParameterBinding = new Binding(nameof(BoundView.Value)),
            });

    // The hint body is ONE construction: the standalone tooltip case and the binding's hint column both land
    // here, so a gesture affordance reads identically wherever it is attached.
    private static Control Hint(HintRow hint, MaterializeContext context) {
        StackPanel body = new() { Orientation = Orientation.Horizontal };
        body.Children.Add(new TextBlock { Text = context.Label(hint.Body), TextWrapping = TextWrapping.Wrap });
        hint.Gesture.Iter(gesture => body.Children.Add(new TextBlock { Text = gesture.ToString() }));
        ContentControl host = new() { Content = body };
        context.Skin(ControlSkin.Tooltip).Iter(theme => host.Theme = theme);
        return host;
    }

    // The persistent condition strip: a banner ends when its condition does, not on a clock, and its verbs and
    // evidence recurse through this same fold so a retry button in a banner and one in a form are one control
    // under one command rail. Severity lands on the shipped strip's own notification type, which drives the
    // theme's severity pseudo-classes, so this fold writes no paint.
    private static Fin<Control> Notice(ControlIntent.Banner intent, MaterializeContext context) =>
        from actions in intent.Actions.Traverse(action => Materialize(action, context)).As()
        from evidence in intent.Evidence.Match(
            Some: row => Materialize(row, context).Map(Some),
            None: () => Fin<Option<Control>>.Succ(None))
        select Strip(intent, actions, evidence, context);

    private static Control Strip(ControlIntent.Banner intent, Seq<Control> actions, Option<Control> evidence, MaterializeContext context) {
        StackPanel body = new();
        body.Children.Add(new TextBlock { Text = context.Label(intent.BodyKey), TextWrapping = TextWrapping.Wrap });
        evidence.Iter(control => body.Children.Add(control));
        if (!actions.IsEmpty) {
            StackPanel verbs = new() { Orientation = Orientation.Horizontal };
            actions.Iter(verb => verbs.Children.Add(verb));
            body.Children.Add(verbs);
        }
        Ursa.Controls.Banner strip = new() {
            Header = context.Label(intent.HeadlineKey),
            Content = body,
            Type = intent.Severity.Type,
            CanClose = intent.Severity.Dismissible,
            ShowIcon = true,
        };
        strip.Classes.Add(intent.Placement.Key);
        return strip;
    }

    // The empty state carries its own call to action as a child intent, so the recovery verb rides the same
    // command rail every other button does.
    private static Fin<Control> Vacant(ControlIntent.EmptyState intent, MaterializeContext context) =>
        intent.Action.Match(
            Some: action => Materialize(action, context).Map(control => Stack(intent, context, Some(control))),
            None: () => Fin<Control>.Succ(Stack(intent, context, None)));

    private static Control Stack(ControlIntent.EmptyState intent, MaterializeContext context, Option<Control> action) {
        StackPanel body = new();
        body.Children.Add(new TextBlock { Text = context.Label(intent.HeadlineKey) });
        body.Children.Add(new TextBlock { Text = context.Label(intent.BodyKey), TextWrapping = TextWrapping.Wrap });
        action.Iter(control => body.Children.Add(control));
        return new ContentControl { Content = body };
    }

    private static Fin<DataGrid> Table(ControlIntent.Grid intent, MaterializeContext context) =>
        intent.Columns.Traverse(column => Column(column, context)).As().Map(columns => {
            DataGrid grid = new() { AutoGenerateColumns = false, IsReadOnly = intent.Columns.ForAll(static column => column.Editor.IsNone) };
            context.Skin(ControlSkin.GridRow).Iter(theme => grid.RowTheme = theme);
            columns.Iter(grid.Columns.Add);
            return grid;
        });

    // The sort answer is ONE Option read: an absent sort key disables sorting for that column alone, and the
    // empty-string member path the package requires for "no sort" spells only inside that None arm.
    private static Fin<DataGridColumn> Column(ColumnRow row, MaterializeContext context) =>
        Fin<DataGridColumn>.Succ(new DataGridTemplateColumn {
            Header = context.Label(row.HeaderKey),
            Width = row.Extent,
            SortMemberPath = row.SortKey.IfNone(string.Empty),
            CanUserSort = row.SortKey.IsSome,
            IsReadOnly = row.Editor.IsNone,
            CellTemplate = Cell(row.Cell, row.Align, context),
            CellEditingTemplate = row.Editor.Match(
                Some: editor => Cell(editor, row.Align, context),
                None: () => Cell(row.Cell, row.Align, context)),
        });

    // Alignment lands on the materialized cell because the grid's column model carries no alignment slot.
    private static IDataTemplate Cell(ControlIntent intent, HorizontalAlignment align, MaterializeContext context) =>
        new FuncDataTemplate<object>((_, _) => Materialize(intent, context).Match(
            Succ: control => { control.HorizontalAlignment = align; return control; },
            Fail: _ => new TextBlock()), supportsRecycling: true);

    // The tree is the flat realized window under one item template over the `Shell/virtualization` `FlatNode`
    // union, so a hierarchy and a GROUPED list materialize through this one arm; expansion STATE arrives on
    // the flattened node and writes back through the expansion command carrying its own node — a per-row
    // two-way binding into a set is the deleted form, because a set is not a property any one row owns.
    private static Fin<Control> Branches(ControlIntent.Tree intent, MaterializeContext context) =>
        context.Gesture(intent.ExpansionCommand).Map(command => (Control)new ItemsControl {
            ItemTemplate = new FuncDataTemplate<FlatNode<object>>((node, _) => node.Switch(
                row: _ => Materialize(intent.Item, context).Match(
                    Succ: control => Indented(control, command),
                    Fail: _ => (Control)new TextBlock()),
                band: heading => Indented(Heading(heading.Group, context), command)),
                supportsRecycling: true),
        });

    // A band heading is a REAL row: its label resolves through the same locale resolver every caption takes
    // and each aggregate cell renders its measure beside its value.
    private static Control Heading(GroupBand band, MaterializeContext context) {
        StackPanel head = new() { Orientation = Orientation.Horizontal };
        head.Children.Add(new TextBlock { Text = context.Label(band.LabelKey) });
        band.Cells.Iter(cell => head.Children.Add(new TextBlock {
            Text = $"{context.Label(cell.Column)} {cell.Value.ToString(CultureInfo.CurrentCulture)}",
        }));
        ContentControl host = new() { Content = head };
        context.Skin(ControlSkin.PaletteRow).Iter(theme => host.Theme = theme);
        return host;
    }

    // The disclosure publishes the realized NODE, which is a gesture value and never a payload, so it binds
    // the flatten owner's own lifting arrow.
    private static Control Indented(Control control, ICommand expand) {
        ToggleButton disclosure = new() { Command = expand };
        disclosure.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(FlatNode<object>.Expanded)));
        disclosure.Bind(Button.CommandParameterProperty, new Binding("."));
        disclosure.Bind(Visual.IsVisibleProperty, new Binding(nameof(FlatNode<object>.HasChildren)));
        DockPanel row = new();
        DockPanel.SetDock(disclosure, Avalonia.Controls.Dock.Left);
        row.Children.Add(disclosure);
        row.Children.Add(control);
        row.Bind(Layoutable.MarginProperty, new MultiBinding {
            Converter = new TreeLevelToPaddingConverter(),
            Bindings = { new Binding(nameof(FlatNode<object>.Depth)), new DynamicResourceExtension((string)IndentUnit) },
        });
        return row;
    }

    // The indent unit is the shipped fixed thickness step, ADDRESSED as a TokenKey row per the token-address
    // law — the value-named-key law forbids re-seeding that scale from a density-selected metric.
    private static readonly TokenKey IndentUnit = TokenKey.Create("SemiThicknessBase");

    // The strip takes its frames as a NAMED source exactly as a bound option set does, so a code pane's ruler,
    // a graph minimap, a long-list strip, and a history timeline are one intent under four producers; the jump
    // command is required because a strip that cannot move the surface it summarizes is a decoration.
    private static Fin<Control> Strip(ControlIntent.Overview intent, MaterializeContext context) =>
        from frames in context.Overview(intent.SourceKey)
        from jump in context.Gesture(intent.JumpCommand)
        select Streamed(intent, frames, jump, context);

    // The frame stream rides the framework's own property binding rather than a hand-rolled subscription, so
    // updates marshal on the framework's terms and the returned lifetime parks on the activation scope.
    private static Control Streamed(
        ControlIntent.Overview intent, IObservable<OverviewFrame> frames, ICommand jump, MaterializeContext context) {
        OverviewStrip strip = new() { Axis = intent.Axis, Jump = jump };
        return Owned(strip, strip.Bind(OverviewStrip.FrameProperty, frames), context);
    }

    // Menu rows recurse as rows, so a separator constructs a rule, a check row carries its toggle type and its
    // checked key, and a submenu is this same fold one level down.
    private static Fin<Seq<Control>> Rows(Seq<MenuRow> rows, MaterializeContext context) =>
        rows.Traverse(row => Row(row, context)).As();

    private static Fin<Control> Row(MenuRow row, MaterializeContext context) =>
        row.Posture == MenuPosture.Divider
            ? Fin<Control>.Succ(new Separator())
            : from icon in Glyph(row.Icon, context)
              from command in Required(row.Command, context)
              from children in Rows(row.Rows, context)
              select Item(row, icon, command, children, context);

    private static Control Item(MenuRow row, Option<IImage> icon, Option<ICommand> command, Seq<Control> children, MaterializeContext context) {
        MenuItem item = new() {
            Header = context.Label(row.LabelKey),
            ToggleType = row.Posture.Toggle.IfNone(MenuItemToggleType.None),
            // The host group slot is the one place a null is a legal spelling — the radio posture's group name
            // is the row key and every other posture crosses the host's own "no group" null.
            GroupName = row.Posture == MenuPosture.Radio ? row.Key : null,
        };
        AutomationProperties.SetAutomationId(item, row.Key);
        icon.Iter(image => item.Icon = new Avalonia.Controls.Image { Source = image });
        row.Gesture.Iter(gesture => item.InputGesture = gesture);
        command.Iter(resolved => item.Command = resolved);
        row.CheckedKey.Iter(key => context.Value(key, item, MenuItem.IsCheckedProperty).Iter(lifetime => context.Own(item, lifetime)));
        if (!children.IsEmpty) { item.ItemsSource = children.ToArray(); }
        return item;
    }

    // The tool bar is the overflow-aware host: each item declares its own overflow mode and the package's
    // popup well carries what does not fit, so a narrow window promotes items instead of clipping them.
    private static Fin<Control> Bar(ControlIntent.Toolbar intent, MaterializeContext context) =>
        intent.Rows.Traverse(row => Materialize(row.Item, context).Map(control => {
                ToolBar.SetOverflowMode(control, row.Overflow);
                return control;
            }))
            .As()
            .Map(items => {
                ToolBar bar = new() { Orientation = intent.Orientation };
                bar.ItemsSource = items.ToArray();
                return (Control)bar;
            });

    // The solver child-identity admission: every solved child is stamped ChildKeyProperty from its OWN intent
    // Key here, so LayoutSolver.SolvedRect always resolves a program-owner key and a keyless child is
    // structurally unmountable.
    private static Fin<Control> Mounted(Fin<Control> layout, Seq<ControlIntent> children, MaterializeContext context) =>
        layout.Bind(host => host is Panel panel
            ? children
                .TraverseM(child => Materialize(child, context).Map(control => {
                    control.SetValue(LayoutSolver.ChildKeyProperty, child.Key);
                    return control;
                }))
                .As()
                .Map(mounted => {
                    mounted.Iter(control => panel.Children.Add(control));
                    return (Control)panel;
                })
            : Fin<Control>.Fail(new ControlFault.TemplateMissing(nameof(LayoutSolver))));

    // Container legs are ONE recursive traverse each — a child failure aborts the whole container on the Fin
    // rail, so a half-materialized screen tree is unrepresentable.
    private static Fin<Seq<TabItem>> Pages(Seq<(string HeaderKey, ControlIntent Body)> pages, MaterializeContext context) =>
        pages.TraverseM(page => Materialize(page.Body, context)
            .Map(body => new TabItem { Header = context.Label(page.HeaderKey), Content = body })).As();

    // Each section binds `Theme/motion` `MotionApplication.Span(content, width, opening)` for the measured
    // open/close pair and `Release(content)` once the sweep settles — a section left holding its animated
    // height would refuse to grow when its own content changed underneath it.
    private static Fin<Seq<Expander>> Sections(Seq<(string HeaderKey, ControlIntent Body)> sections, MaterializeContext context) =>
        sections.TraverseM(section => Materialize(section.Body, context)
            .Map(body => new Expander { Header = context.Label(section.HeaderKey), Content = body })).As();

    private static Fin<Control> Split(ControlIntent.Splitter intent, MaterializeContext context) =>
        (Materialize(intent.First, context), Materialize(intent.Second, context))
            .Apply((first, second) => Divided(first, second, intent.Orientation)).As();

    // The splitter host: star tracks either side of an Auto splitter track, orientation selecting the axis —
    // one Grid + GridSplitter, never a bespoke split control.
    private static Control Divided(Control first, Control second, Orientation orientation) {
        Grid grid = new();
        GridSplitter splitter = new() { ResizeDirection = orientation == Orientation.Horizontal ? GridResizeDirection.Columns : GridResizeDirection.Rows };
        if (orientation == Orientation.Horizontal) {
            grid.ColumnDefinitions = new ColumnDefinitions("*,Auto,*");
            Grid.SetColumn(first, 0); Grid.SetColumn(splitter, 1); Grid.SetColumn(second, 2);
        }
        else {
            grid.RowDefinitions = new RowDefinitions("*,Auto,*");
            Grid.SetRow(first, 0); Grid.SetRow(splitter, 1); Grid.SetRow(second, 2);
        }
        grid.Children.Add(first); grid.Children.Add(splitter); grid.Children.Add(second);
        return grid;
    }

    // The one bound-collection hop: the realized change-set binds ONCE into a ReadOnlyObservableCollection the
    // grid consumes — ItemsSource never receives the raw stream.
    private static Control Windowed(Control control, WindowLease<RealizedItem<object>> lease, MaterializeContext context) {
        control.SetValue(ItemsControl.ItemsSourceProperty, lease.View);
        return Owned(control, lease.Lifetime, context);
    }
}
```

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class ControlFactory {
    // THE per-case answer set. One total generated Switch serves all four columns, so the re-dress ladder, the
    // content-property ladder, and the host-name method this page once kept beside it are unspellable. `Parked`
    // is None wherever the case's host type is NOT recoverable from the intent alone — the button family picks
    // its host from emphasis and icon together, so a pool key over it would name a host the reuse never built,
    // the same lie the `[PACKAGE_ADMISSION]` badge row is refused for.
    internal static ControlShape ShapeOf(ControlIntent intent) => intent.Switch(
        button: static c => new ControlShape(None, Some(c.Binding.Emphasis.Skin), None, None),
        label: static c => new ControlShape(Some(nameof(TextBlock)), None, None, Redressed<ControlIntent.Label>(Written)),
        textInput: static c => new ControlShape(Some(nameof(TextBox)), Some(ControlSkin.TextEntry), Some((AvaloniaProperty)TextBox.TextProperty), Redressed<ControlIntent.TextInput>(Typed)),
        numberInput: static c => new ControlShape(Some(c.Kind.Control), None, Some(c.Kind.Value), Redressed<ControlIntent.NumberInput>(Spun)),
        dateInput: static c => new ControlShape(Some(c.Kind.Control), None, Some(c.Kind.Slot), Redressed<ControlIntent.DateInput>(Dated)),
        pathInput: static c => new ControlShape(Some(nameof(PathPicker)), None, Some((AvaloniaProperty)PathPicker.SelectedPathsTextProperty), Redressed<ControlIntent.PathInput>(Picked)),
        // `ColorPicker : ColorView`, so the ONE colour slot serves both postures and the posture row needs no
        // slot column of its own (`.api/api-avalonia-color.md`: styled properties inherited unchanged).
        colorInput: static c => new ControlShape(Some(c.Posture.Control), None, Some((AvaloniaProperty)ColorView.ColorProperty), Redressed<ControlIntent.ColorInput>(Tinted)),
        select: static c => new ControlShape(Some(c.Posture.Control), None, Some(c.Posture.Slot), Redressed<ControlIntent.Select>(Rechosen)),
        multiSelect: static c => new ControlShape(Some(c.Posture.Control), None, Some(c.Posture.Slot), Redressed<ControlIntent.MultiSelect>(Remulti)),
        slider: static c => new ControlShape(Some(nameof(Slider)), None, Some((AvaloniaProperty)RangeBase.ValueProperty), Redressed<ControlIntent.Slider>(Slid)),
        range: static c => new ControlShape(Some(nameof(RangeSlider)), None, Some((AvaloniaProperty)RangeSlider.LowerValueProperty), Redressed<ControlIntent.Range>(Spanned)),
        toggle: static c => new ControlShape(Some(nameof(ToggleSwitch)), None, Some((AvaloniaProperty)ToggleButton.IsCheckedProperty), Redressed<ControlIntent.Toggle>(Flipped)),
        // The exclusive families carry their option set AS their containers, so a reuse would rebuild every
        // item anyway — the pool key would buy a template the re-dress immediately discards.
        radio: static _ => new ControlShape(None, None, Some((AvaloniaProperty)SelectingItemsControl.SelectedValueProperty), None),
        segmented: static c => new ControlShape(None, None, c.Posture == SegmentPosture.Select ? Some((AvaloniaProperty)SelectingItemsControl.SelectedValueProperty) : None, None),
        chip: static c => new ControlShape(Some(c.Posture.Control), c.Posture.Skin, Some(c.Posture.Slot), Redressed<ControlIntent.Chip>(Tagged)),
        progress: static c => new ControlShape(Some(c.Form.Control), c.Form == ProgressForm.Ring ? Some(ControlSkin.ProgressRing) : None, c.Form.Slot, Redressed<ControlIntent.Progress>(Metered)),
        avatar: static _ => new ControlShape(None, Some(ControlSkin.AvatarCluster), None, None),
        breadcrumb: static _ => new ControlShape(None, None, None, None),
        tooltip: static _ => new ControlShape(None, None, None, None),
        banner: static _ => new ControlShape(None, Some(ControlSkin.Banner), None, None),
        emptyState: static _ => new ControlShape(None, Some(ControlSkin.EmptyStatePanel), None, None),
        grid: static _ => new ControlShape(None, None, None, None),
        tree: static _ => new ControlShape(None, None, None, None),
        overview: static _ => new ControlShape(None, Some(ControlSkin.OverviewStrip), None, None),
        menu: static _ => new ControlShape(None, None, None, None),
        toolbar: static _ => new ControlShape(None, None, None, None),
        tab: static _ => new ControlShape(None, None, None, None),
        accordion: static _ => new ControlShape(None, None, None, None),
        panel: static _ => new ControlShape(None, None, None, None),
        dock: static _ => new ControlShape(None, None, None, None),
        splitter: static _ => new ControlShape(None, None, None, None));

    // ONE lift for every re-dress body: the column is typed over the family, so the case probe and its
    // RecyclingViolation seal spell once here instead of thirteen times inside the arms.
    private static Option<Func<ControlIntent, Control, MaterializeContext, Fin<Control>>> Redressed<TIntent>(
        Func<TIntent, Control, MaterializeContext, Fin<Control>> body) where TIntent : ControlIntent =>
        Some<Func<ControlIntent, Control, MaterializeContext, Fin<Control>>>((intent, control, context) =>
            intent is TIntent typed
                ? body(typed, control, context)
                : Fin<Control>.Fail(new ControlFault.RecyclingViolation(intent.Key)));

    // The SECOND value channel, total over the family. It answers a slot pair no other member answers — the
    // primary `Slot` column carries one property and a case with two thumbs carries two — so this is not a
    // mirror of `ShapeOf`, and the leaf arms compress exactly as `Children` does. An upper KEY on a temporal
    // row whose kind declares no upper slot refuses, rather than binding the lower slot twice.
    private static Option<(string Key, Option<AvaloniaProperty> Slot)> Second(ControlIntent intent) => intent.Switch(
        range: static c => Some((c.UpperKey, Some((AvaloniaProperty)RangeSlider.UpperValueProperty))),
        dateInput: static c => c.UpperKey.Map(key => (key, c.Kind.UpperSlot)),
        button: static _ => None, label: static _ => None, textInput: static _ => None,
        numberInput: static _ => None, pathInput: static _ => None, colorInput: static _ => None,
        select: static _ => None, multiSelect: static _ => None, slider: static _ => None,
        toggle: static _ => None, radio: static _ => None, segmented: static _ => None,
        chip: static _ => None, progress: static _ => None, avatar: static _ => None,
        breadcrumb: static _ => None, tooltip: static _ => None, banner: static _ => None,
        emptyState: static _ => None, grid: static _ => None, tree: static _ => None,
        overview: static _ => None, menu: static _ => None, toolbar: static _ => None,
        tab: static _ => None, accordion: static _ => None, panel: static _ => None,
        dock: static _ => None, splitter: static _ => None);

    // Style classes are the whole appearance write this fold performs: the semantic paint role every intent
    // carries, plus the typographic role the ONE case that declares the field carries. The read is over a
    // DECLARED member, not a per-case roster, so it mirrors nothing `ShapeOf` answers.
    private static Seq<string> Classed(ControlIntent intent) =>
        Seq(intent.Binding.Role.Key) + (intent is ControlIntent.Label row ? Seq(row.Role.Key) : Seq<string>());

    // --- [ADMISSION]
    // The binding admission is APPLICATIVE, not a Bind chain: a control whose command is unbound AND whose skin
    // is unresolved AND whose value key names a slot its case does not declare reports all three, because the
    // author fixing one defect at a time re-runs the whole screen for each. Every leg RESOLVES and writes
    // nothing; `Apply` performs the attach once on the success arm, so a refused materialize leaves no partly
    // dressed control behind. Refused operator: the `Fin` monadic chain this page carried, which stops at the
    // first defect.
    private static Fin<Control> Bind(ControlIntent intent, Control control, MaterializeContext context) {
        ControlShape shape = ShapeOf(intent);
        return (Verb(intent.Binding.Command, context),
                Dressed(shape.Skin, context),
                Slotted(intent, shape),
                Seconded(intent),
                Glyphed(intent.Binding.Icon, context))
            .Apply((command, theme, slot, second, glyph) =>
                Apply(intent, control, command, theme, slot, second, glyph, context))
            .As()
            .Match(Succ: static outcome => outcome, Fail: Fin<Control>.Fail);
    }

    // A command key is resolved or it is a fault: the deck is boot-frozen, so an unknown key can only ever be
    // an authoring defect, and a control bound to nothing is the one failure a running screen cannot report.
    private static Validation<Error, Option<ICommand>> Verb(Option<string> command, MaterializeContext context) =>
        command.Match(
            Some: key => context.Command(key).Match(
                Some: static resolved => Validation<Error, Option<ICommand>>.Success(Some(resolved)),
                None: () => Validation<Error, Option<ICommand>>.Fail(new ControlFault.UnboundIntent(key))),
            None: () => Validation<Error, Option<ICommand>>.Success(None));

    // The single-column collapse of the same resolution, for the row consumers that admit one command at a
    // time and have no second column to accumulate against.
    private static Fin<Option<ICommand>> Required(Option<string> command, MaterializeContext context) =>
        Verb(command, context).Match(Succ: Fin<Option<ICommand>>.Succ, Fail: Fin<Option<ICommand>>.Fail);

    private static Validation<Error, Option<ControlTheme>> Dressed(Option<ControlSkin> skin, MaterializeContext context) =>
        skin.Match(
            Some: row => context.Skin(row).Match(
                Some: static theme => Validation<Error, Option<ControlTheme>>.Success(Some(theme)),
                None: () => Validation<Error, Option<ControlTheme>>.Fail(new ControlFault.SkinUnresolved(row.Key))),
            None: () => Validation<Error, Option<ControlTheme>>.Success(None));

    // A value key is admitted against the case's OWN declared slot, so a two-way binding onto a case with no
    // value channel refuses at materialize instead of binding a property the host never registered. The pair is
    // one applicative read of the two options, so the "key without slot" corner is the only refusal shape.
    private static Validation<Error, Option<(string Key, AvaloniaProperty Slot)>> Slotted(ControlIntent intent, ControlShape shape) =>
        intent.Binding.ValueKey.Match(
            Some: key => shape.Slot.Match(
                Some: slot => Validation<Error, Option<(string, AvaloniaProperty)>>.Success(Some((key, slot))),
                None: () => Validation<Error, Option<(string, AvaloniaProperty)>>.Fail(
                    new ControlFault.SlotUnavailable($"{intent.Key}:value"))),
            None: () => Validation<Error, Option<(string, AvaloniaProperty)>>.Success(None));

    private static Validation<Error, Option<(string Key, AvaloniaProperty Slot)>> Seconded(ControlIntent intent) =>
        Second(intent).Match(
            Some: pair => pair.Slot.Match(
                Some: slot => Validation<Error, Option<(string, AvaloniaProperty)>>.Success(Some((pair.Key, slot))),
                None: () => Validation<Error, Option<(string, AvaloniaProperty)>>.Fail(
                    new ControlFault.SlotUnavailable($"{intent.Key}:upper"))),
            None: () => Validation<Error, Option<(string, AvaloniaProperty)>>.Success(None));

    private static Validation<Error, Option<IImage>> Glyphed(Option<IconSlot> slot, MaterializeContext context) =>
        slot.Match(
            Some: icon => context.Icon(icon.Asset, icon.Size).Match(
                Succ: static image => Validation<Error, Option<IImage>>.Success(Some(image)),
                Fail: Validation<Error, Option<IImage>>.Fail),
            None: () => Validation<Error, Option<IImage>>.Success(None));

    // THE one attach fold. Identity derives from the intent key alone — the id verbatim, the announced name
    // through the composition-bound locale resolver — so an authored automation literal has nowhere to live.
    // The icon quartet is REGISTERED ONCE by the icon-leading button and its attached setters reach any content
    // control (`.api/api-ursa.md` `[BUTTON_PROPERTIES]`), so one glyph write serves every host that admits one
    // and a glyph on a host that admits none refuses rather than vanishing.
    private static Fin<Control> Apply(
        ControlIntent intent, Control control, Option<ICommand> command, Option<ControlTheme> theme,
        Option<(string Key, AvaloniaProperty Slot)> valued, Option<(string Key, AvaloniaProperty Slot)> second,
        Option<IImage> glyph, MaterializeContext context) {
        AutomationProperties.SetAutomationId(control, intent.Key);
        AutomationProperties.SetName(control, context.Label(intent.Key));
        theme.Iter(row => control.SetValue(StyledElement.ThemeProperty, row));
        Classed(intent).Iter(control.Classes.Add);
        intent.Binding.Hint.Iter(hint => ToolTip.SetTip(control, Hint(hint, context)));
        return Iconed(intent, control, glyph)
            .Bind(_ => Bound(valued, control, context))
            .Bind(_ => Bound(second, control, context))
            .Bind(_ => Pending(intent, control, context))
            .Bind(_ => Fired(intent, control, command, context))
            .Map(_ => control);
    }

    private static Fin<Unit> Iconed(ControlIntent intent, Control control, Option<IImage> glyph) =>
        (glyph, intent.Binding.Icon, control) switch {
            ({ Case: IImage image }, { Case: IconSlot slot }, ContentControl host) => Seated(host, image, slot),
            ({ IsSome: true }, _, _) => Fin<Unit>.Fail(new ControlFault.SlotUnavailable($"{intent.Key}:icon")),
            _ => Fin.Succ(unit),
        };

    private static Fin<Unit> Seated(ContentControl host, IImage image, IconSlot slot) {
        host.SetValue(IconButton.IconProperty, new Avalonia.Controls.Image { Source = image });
        host.SetValue(IconButton.IconPlacementProperty, slot.Placement);
        return Fin.Succ(unit);
    }

    private static Fin<Unit> Bound(Option<(string Key, AvaloniaProperty Slot)> channel, Control control, MaterializeContext context) =>
        channel.Match(
            Some: pair => context.Value(pair.Key, control, pair.Slot).Map(lifetime => context.Own(control, lifetime)),
            None: () => Fin.Succ(unit));

    // Pending drives the shipped loading slot and NOTHING else, so an in-flight verb keeps its own hit target.
    private static Fin<Unit> Pending(ControlIntent intent, Control control, MaterializeContext context) =>
        intent.Binding.Icon.Bind(static slot => slot.Pending).Match(
            Some: key => context.Value(key, control, IconButton.IsLoadingProperty).Map(lifetime => context.Own(control, lifetime)),
            None: () => Fin.Succ(unit));

    // A resolved verb ALWAYS attaches and the trigger column narrows which gesture raises it, so a control that
    // resolved a command it could never be invoked through is unrepresentable; the default trigger is the
    // activation gesture, because a verb with no stated gesture is the plain one.
    private static Fin<Unit> Fired(ControlIntent intent, Control control, Option<ICommand> command, MaterializeContext context) =>
        command.Match(
            Some: resolved => context
                .Activate(intent.Binding.Trigger.IfNone(ControlTrigger.Activate), control, resolved)
                .Map(lifetime => context.Own(control, lifetime)),
            None: () => Fin.Succ(unit));

    // --- [REDRESS]
    // Each body re-dresses ONE parked host in place. Together with `ShapeOf`'s `Parked` column they are the
    // whole recycling contract: a body exists exactly where a host type is recoverable from the intent.
    private static Fin<Control> Written(ControlIntent.Label intent, Control control, MaterializeContext context) {
        control.SetValue(TextBlock.TextProperty, context.Label(intent.TextKey));
        control.SetValue(TextBlock.TextWrappingProperty, intent.Role.Wraps ? TextWrapping.Wrap : TextWrapping.NoWrap);
        return Fin<Control>.Succ(control);
    }

    private static Fin<Control> Typed(ControlIntent.TextInput intent, Control control, MaterializeContext context) {
        control.SetValue(TextBox.WatermarkProperty, intent.Watermark);
        control.SetValue(TextBox.AcceptsReturnProperty, intent.Multiline);
        return Fin<Control>.Succ(control);
    }

    private static Fin<Control> Spun(ControlIntent.NumberInput intent, Control control, MaterializeContext context) =>
        intent.Kind.Redress(control, intent.Range).Map(_ => control);

    private static Fin<Control> Dated(ControlIntent.DateInput intent, Control control, MaterializeContext context) =>
        intent.Kind.Traits.Admits(TemporalTrait.Calendar) || (intent.From.IsNone && intent.Until.IsNone)
            ? Fin<Control>.Succ(Blackout(control, intent))
            : Fin<Control>.Fail(new ControlFault.PayloadRejected($"{intent.Key}:{intent.Kind.Key} bounds"));

    private static Fin<Control> Picked(ControlIntent.PathInput intent, Control control, MaterializeContext context) =>
        intent.Filters.Traverse(static filter => filter.Encode()).As().Map(encoded => {
            control.SetValue(PathPicker.UsePickerTypeProperty, intent.Mode);
            control.SetValue(PathPicker.AllowMultipleProperty, intent.Multiple);
            control.SetValue(PathPicker.FileFilterProperty, string.Concat(encoded));
            control.SetValue(PathPicker.TitleProperty, context.Label(intent.Key));
            return control;
        });

    private static Fin<Control> Tinted(ControlIntent.ColorInput intent, Control control, MaterializeContext context) {
        control.SetValue(ColorView.IsAlphaEnabledProperty, intent.Alpha);
        control.SetValue(ColorView.IsAlphaVisibleProperty, intent.Alpha);
        return Fin<Control>.Succ(control);
    }

    // The option-bearing re-dress re-LEASES: the parked host keeps its compiled template — the expensive half —
    // while its items come from a fresh window, so the lease the reuse drops is the one the reset released.
    private static Fin<Control> Rechosen(ControlIntent.Select intent, Control control, MaterializeContext context) =>
        context.Options(intent.Options, intent.Window).Map(lease => {
            control.SetValue(ItemsControl.ItemsSourceProperty, intent.Posture == SelectPosture.Closed
                ? Listed(lease.View, context).ToArray()
                : (object)Views(lease.View, context).ToArray());
            return Owned(control, lease.Lifetime, context);
        });

    private static Fin<Control> Remulti(ControlIntent.MultiSelect intent, Control control, MaterializeContext context) =>
        intent.Posture == MultiPosture.Free
            ? Fin<Control>.Succ(Watermarked(control, TagInput.WatermarkProperty, intent.Key, context))
            : context.Options(intent.Options, intent.Window).Map(lease => {
                control.SetValue(ItemsControl.ItemsSourceProperty, Views(lease.View, context).ToArray());
                return Owned(Watermarked(control, MultiComboBox.WatermarkProperty, intent.Key, context), lease.Lifetime, context);
            });

    private static Control Watermarked(Control control, AvaloniaProperty slot, string key, MaterializeContext context) {
        control.SetValue(slot, context.Label(key));
        return control;
    }

    private static Fin<Control> Slid(ControlIntent.Slider intent, Control control, MaterializeContext context) {
        control.SetValue(RangeBase.MinimumProperty, intent.Min);
        control.SetValue(RangeBase.MaximumProperty, intent.Max);
        control.SetValue(Slider.TickFrequencyProperty, intent.Step);
        return Fin<Control>.Succ(control);
    }

    private static Fin<Control> Spanned(ControlIntent.Range intent, Control control, MaterializeContext context) {
        control.SetValue(RangeSlider.MinimumProperty, intent.Min);
        control.SetValue(RangeSlider.MaximumProperty, intent.Max);
        control.SetValue(RangeSlider.TickFrequencyProperty, intent.Step);
        control.SetValue(RangeSlider.IsSnapToTickProperty, intent.Step > 0d);
        return Fin<Control>.Succ(control);
    }

    private static Fin<Control> Flipped(ControlIntent.Toggle intent, Control control, MaterializeContext context) {
        control.SetValue(ContentControl.ContentProperty, context.Label(intent.LabelKey));
        return Fin<Control>.Succ(control);
    }

    // Every chip posture hosts a content control, so the label re-dresses through the ONE content slot while
    // the posture's own `Slot` column stays the VALUE channel the binding admission reads.
    private static Fin<Control> Tagged(ControlIntent.Chip intent, Control control, MaterializeContext context) {
        control.SetValue(ContentControl.ContentProperty, context.Label(intent.TextKey));
        return Fin<Control>.Succ(control);
    }

    private static Fin<Control> Metered(ControlIntent.Progress intent, Control control, MaterializeContext context) {
        intent.Form.Slot.Iter(slot => {
            control.SetValue(slot, intent.Fraction.IfNone(0d));
            control.SetValue(ProgressBar.IsIndeterminateProperty, intent.Fraction.IsNone);
            control.SetValue(ProgressBar.ShowProgressTextProperty, intent.Fraction.IsSome);
        });
        return Fin<Control>.Succ(control);
    }
}
```

## [04]-[CONTROL_RECYCLING]

- Owner: `RecycleScope` the per-screen parked-control pool holding ONE live cell; `PoolState` the value every transition swaps and answers; `MaterializePool` the recycling-aware materialization entry every windowed surface calls; `ControlFactory.Rebind` the re-dress-and-re-attach re-entry over a parked host.
- Law: a pooled host is keyed by the `ControlShape.Parked` type NAME and by nothing else — a case whose host is not recoverable from the intent alone carries `None` there and never parks, so a pool key can never name a host the reuse did not build.
- Law: the scope holds a live mutable cell, so it is a sealed class whose transitions ANSWER what they retired — a record copy would share the cell by reference and two screens would drain one pool.
- Law: the held count is a FIELD of the swapped value, never a fold over the racks — a per-return scan of every rack is the deleted form, and a count that disagrees with the racks is unrepresentable because one swap writes both.
- Law: the pool is CAPPED on its held count and the cap is a mount argument, because a screen-wide pool with no ceiling retains one control per row a long scroll ever realized; the refusal is a transition verdict the caller reads, so an over-cap control is disposed by its own scope rather than parked forever.
- Entry: `public Transition<PoolState> Return(string host, Control parked)` — parks a scrolled-out control under its host name, answering `Committed` with the post-state or `Refused` when the rack is at its cap. `public Option<Control> Take(string host)` — draws one parked host out, the absent answer being the empty rack itself. `public (Transition<PoolState> Transition, Seq<Control> Drained) Drain()` — the take-and-clear the activation scope calls, the drained roster riding the transition because an empty post-state cannot report what it released. `public static Fin<Control> Realize(ControlIntent intent, MaterializeContext context, RecycleScope scope)` — the recycling-aware fold: a parked host is reset, re-dressed, and re-bound; anything else falls to the cold `Materialize`. `public static Fin<Control> Rebind(ControlIntent intent, Control parked, MaterializeContext context)` — the re-entry, gated on the parked type name matching the shape's own `Parked` answer.
- Auto: `Realize` reads `ShapeOf(intent).Parked`, draws that host from the pool, releases every lifetime the previous tenant bound, re-dresses through the shape's OWN `Redress` column — the same body the cold fold would have used — and re-attaches through the same `Bind` admission, so a recycled control and a freshly built one are identical by construction; a failed re-dress rolls the parked control's remaining custody back rather than leaking a half-dressed host into the tree; every successful re-entry seals the same `ControlReceipt` a cold materialize does, so the evidence stream cannot tell a reuse from a build and a recycling regression shows as a receipt whose control type disagrees with its intent.
- Packages: Avalonia, Irihi.Ursa, Xaml.Behaviors.Avalonia, Rasm (kernel `Atom`/`Cell`/`Transition`/`Custody`/`Dimension`), LanguageExt.Core
- Growth: a new poolable case is one `Parked` name and one `Redress` body on the existing `ShapeOf` arm; a new pool ceiling is one `Dimension` at the mount; zero new surface.
- Boundary: `RecycleScope` is the one control pool in the package — the `Shell/virtualization` `VirtualWindow` fabric parks and draws through it for every windowed list, tree, grid, and canvas, so a per-surface control cache beside it is the `[04]-[BOUNDARIES]` per-surface-virtualizer rejected form; the pool holds CONTROLS and never intents, values, or leases, because a parked control's data is exactly what the reset drops; the reset releases through `MaterializeContext.Release` so the surface activation scope — the one owner of every bound lifetime — decides what dies, and the fold never disposes a lifetime it did not mint; `Interaction.GetBehaviors(parked).Clear()` is the one framework-forced statement, since the behaviour collection is attached state no property clear reaches; the style classes clear while pseudo-classes survive, which is why a stale variant class cannot ride a reuse into a new row and why a pointer-over state left mid-scroll resolves itself; the data context, tooltip, and theme return to UNSET rather than to null, so the host's own inheritance answers instead of a sentinel the boundary law forbids past it; a control whose rack is at its cap is refused BACK to its caller, which drops it on the activation scope — the pool never silently discards a control it accepted; the scope's `Drain` runs on the screen's own teardown and hands the roster back, so the drained controls die with the scope that built them and the pool never outlives the tree it served.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// The pool's whole state in ONE value, so every transition answers racks and count together and a count that
// disagrees with its racks is unspellable. The rack is a `Seq` because parking and drawing are both head
// operations and the reuse order is deliberately last-in-first-out — the most recently parked control is the
// one whose measured extent and realized template are still warm.
public sealed record PoolState(HashMap<string, Seq<Control>> Racks, int Held) {
    public static readonly PoolState Empty = new(HashMap<string, Seq<Control>>(), 0);

    public Seq<Control> Rack(string host) => Racks.Find(host).IfNone(Seq<Control>());
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class RecycleScope {
    // The ceiling is a MOUNT argument for the same reason the window spec's viewport extent is: the realized
    // count a screen can produce is a measurement, and a preset here would either strand a long list re-building
    // every row or retain a short one's whole history.
    public RecycleScope(Dimension rackCap) => Cap = rackCap;

    private readonly Atom<PoolState> cell = Atom(PoolState.Empty);

    public Dimension Cap { get; }

    public PoolState State => cell.Value;

    public int Held => State.Held;

    // The ceiling reads the HELD FIELD, never a fold over the racks: the count a return has to compare against
    // is written by the same swap that grows a rack, so the two can never disagree and a park costs no scan of
    // a pool a long scroll has filled.
    public Transition<PoolState> Return(string host, Control parked) =>
        Cell.Step(
            cell,
            held => held.Held >= Cap.Value
                ? Option<PoolState>.None
                : Some(new PoolState(held.Racks.AddOrUpdate(host, rack => parked.Cons(rack), Seq(parked)), held.Held + 1)),
            new ControlFault.RecyclingViolation($"{host}:{Cap.Value}"));

    // The drawn control rides BESIDE the transition exactly as the kernel's token-bearing seat does — a second
    // read of the cell cannot reconstruct which control this caller took. The CAS body re-runs until it
    // commits, so the last invocation's assignment is the answer, and a `Refused` verdict and an empty draw are
    // the same fact: the rack held nothing.
    public Option<Control> Take(string host) {
        Option<Control> drawn = None;
        ignore(Cell.Step(
            cell,
            held => {
                Seq<Control> rack = held.Rack(host);
                drawn = rack.HeadOrNone();
                return rack.IsEmpty
                    ? Option<PoolState>.None
                    : Some(new PoolState(held.Racks.SetItem(host, rack.Tail), held.Held - 1));
            },
            new ControlFault.RecyclingViolation($"{host}:empty")));
        return drawn;
    }

    // TAKE-AND-CLEAR: the drained roster is the answer, because the post-state is empty by construction and a
    // caller disposing what it drained needs the controls, not the emptiness.
    public (Transition<PoolState> Transition, Seq<Control> Drained) Drain() {
        Seq<Control> drained = Seq<Control>();
        Transition<PoolState> transition = Cell.Step(
            cell,
            held => {
                drained = held.Racks.Values.Fold(Seq<Control>(), static (all, rack) => all + rack);
                return Some(PoolState.Empty);
            },
            new ControlFault.RecyclingViolation("drain"));
        return (transition, drained);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterializePool {
    // The recycling-aware entry: the pool is consulted through the SAME shape answer the cold fold reads, so a
    // case that cannot name its host never reaches the pool at all and no second poolability table exists.
    public static Fin<Control> Realize(ControlIntent intent, MaterializeContext context, RecycleScope scope) =>
        ControlFactory.ShapeOf(intent).Parked.Bind(scope.Take).Match(
            Some: parked => ControlFactory
                .Rebind(intent, Reset(parked, context), context)
                .Rollback(() => Fin.Succ(context.Release(parked))),
            None: () => ControlFactory.Materialize(intent, context));

    // Parking is the inverse door and takes the SAME key, so a control can only ever return to the rack its own
    // intent named; an unpoolable case refuses against the pool's LIVE state, so the caller reads one verdict
    // shape whether the refusal came from the cap or from the shape.
    public static Transition<PoolState> Park(ControlIntent intent, Control control, RecycleScope scope) =>
        ControlFactory.ShapeOf(intent).Parked.Match(
            Some: host => scope.Return(host, control),
            None: () => new Transition<PoolState>.Refused(scope.State, new ControlFault.RecyclingViolation(intent.Key)));

    // The one release. Everything a previous tenant bound dies with the surface scope that owns it; the
    // behaviour collection is attached state no property clear reaches, which is the only framework-forced
    // statement here; `Classes.Clear` keeps pseudo-classes, so a stale variant class cannot ride the reuse
    // while a mid-scroll pointer-over resolves itself; the three inherited slots return to UNSET rather than
    // to a null the boundary law forbids past the edge.
    private static Control Reset(Control parked, MaterializeContext context) {
        ignore(context.Release(parked));
        Interaction.GetBehaviors(parked).Clear();
        parked.Classes.Clear();
        parked.ClearValue(StyledElement.ThemeProperty);
        parked.ClearValue(StyledElement.DataContextProperty);
        parked.ClearValue(ToolTip.TipProperty);
        return parked;
    }
}

public static partial class ControlFactory {
    // The re-entry. The parked type name is compared to the shape's OWN `Parked` answer before any re-dress
    // body runs — the compile-time control-type check the collapsed re-dress ladder gave up, bought back as a
    // sealed RecyclingViolation at exactly the one site a pool can lie.
    public static Fin<Control> Rebind(ControlIntent intent, Control parked, MaterializeContext context) =>
        Redress(intent, parked, context)
            .Bind(control => Bind(intent, control, context))
            .Map(control => Sealed(intent, control, context));

    // The gate and the re-dress are ONE read of the shape: a body exists exactly where a `Parked` name does, so
    // the two Options are matched together and a name that does not answer the live host type refuses before
    // any property write lands on a control the caller still owns.
    private static Fin<Control> Redress(ControlIntent intent, Control parked, MaterializeContext context) {
        ControlShape shape = ShapeOf(intent);
        return (from host in shape.Parked.Filter(name => name == parked.GetType().Name)
                from body in shape.Redress
                select body(intent, parked, context))
            .IfNone(() => Fin<Control>.Fail(new ControlFault.RecyclingViolation($"{intent.Key}:{parked.GetType().Name}")));
    }
}
```

## [05]-[TS_PROJECTION]

- Owner: the generated `Rasm.Contracts.Ui.V1` control-intent family — `ControlIntentWire` with its generated row messages and enums — and `ControlMap`, the sole interior-to-message correspondence. The schema and generated bindings own every peer-facing shape; this page owns only the behavioral AppUI model and the projection that seats it.
- Law: the projection is one direction. The AppUI product-shell vocabulary is producer-owned and peers decode the generated package, so no wire-to-domain inverse, hand-written message record, TypeScript schema twin, or serializer roster exists here.
- Law: `ControlIntent.Switch` remains the compile-time-complete interior fold, while generated `ControlIntentWire.ArmCase` is the only peer discriminant. `ControlMap.Emit` is the decision-complete support projection seated by `Shell/screens#TS_PROJECTION` inside the one manifest-rooted `AppUiSurfaceProgram`; a detached control tree is not an application payload.
- Entry: `ControlMap.Emit(ControlIntent)` recursively fills one generated oneof arm. A future ProtoJSON egress calls `WireJson.Formatter.Format` on the generated message, so the shared descriptor registry alone controls field names, enums, well-known types, and omissions.
- Packages: Rasm.Contracts (project, generated `Ui.V1` family), Rasm.AppHost (project, `WireJson`), Google.Protobuf, Google.Api.CommonProtos, NodaTime.Serialization.Protobuf, Riok.Mapperly, LanguageExt.Core
- Growth: a new required arm or mapped enum value regenerates every language binding and breaks the producer correspondence or its completeness proof until supplied; a new interior arm breaks the total `Switch`; zero hand-maintained peer shape or JSON options surface.
- Boundary: Interior `ControlIntent`, `IntentBinding`, row values, package enums, and smart-enum policy rows remain because materialization consumes their behavior. A smart-enum row carries its generated coordinate beside its behavior; package enums cross through Mapperly's source-and-target-complete correspondence. Both project once at the design-pinned boundary without a string ladder; the AppUI contract test owns the generated-enum roster proof for smart-enum rows. `ControlReceipt` stays interior process evidence. If a future peer consumes it, it enters the existing `EvidenceReceiptWire` oneof and `EvidenceTimelineWire` rail rather than minting a standalone receipt transport. `@rasm\/contracts/rasm/contracts/ui/v1/controls_pb` is the reusable leaf binding; the current TypeScript viewer admits it only through `appui_surface_pb`, and future apps do not re-declare either transport shape. Three sibling partials separate arm seating, row projection, and package-enum correspondence without minting another public type.

### [05.1]-[CONTROLMAP_ARMS_CS]

```csharp signature
// --- [COMPOSITION] --------------------------------------------------------------------------
public static partial class ControlMap {
    public static ControlIntentWire Emit(ControlIntent intent) => intent.Switch(
        button: static row => ToWire(row),
        label: static row => ToWire(row),
        textInput: static row => ToWire(row),
        numberInput: static row => ToWire(row),
        dateInput: static row => ToWire(row),
        pathInput: static row => ToWire(row),
        colorInput: static row => ToWire(row),
        select: static row => ToWire(row),
        multiSelect: static row => ToWire(row),
        slider: static row => ToWire(row),
        range: static row => ToWire(row),
        toggle: static row => ToWire(row),
        radio: static row => ToWire(row),
        segmented: static row => ToWire(row),
        chip: static row => ToWire(row),
        progress: static row => ToWire(row),
        avatar: static row => ToWire(row),
        breadcrumb: static row => ToWire(row),
        tooltip: static row => ToWire(row),
        banner: static row => ToWire(row),
        emptyState: static row => ToWire(row),
        grid: static row => ToWire(row),
        tree: static row => ToWire(row),
        overview: static row => ToWire(row),
        menu: static row => ToWire(row),
        toolbar: static row => ToWire(row),
        tab: static row => ToWire(row),
        accordion: static row => ToWire(row),
        panel: static row => ToWire(row),
        dock: static row => ToWire(row),
        splitter: static row => ToWire(row));

    private static ControlIntentWire ToWire(ControlIntent.Button row) =>
        Frame(row, new ControlIntentWire.Types.Button { LabelKey = row.LabelKey },
            static (wire, arm) => wire.Button = arm);

    private static ControlIntentWire ToWire(ControlIntent.Label row) =>
        Frame(row, new ControlIntentWire.Types.Label { TextKey = row.TextKey, Role = row.Role.Wire },
            static (wire, arm) => wire.Label = arm);

    private static ControlIntentWire ToWire(ControlIntent.TextInput row) =>
        Frame(row, new ControlIntentWire.Types.TextInput { Watermark = row.Watermark, Multiline = row.Multiline },
            static (wire, arm) => wire.TextInput = arm);

    private static ControlIntentWire ToWire(ControlIntent.NumberInput row) =>
        Frame(row, new ControlIntentWire.Types.NumberInput { Kind = row.Kind.Wire, Range = ToWire(row.Range) },
            static (wire, arm) => wire.NumberInput = arm);

    private static ControlIntentWire ToWire(ControlIntent.DateInput row) {
        var arm = new ControlIntentWire.Types.DateInput { Kind = row.Kind.Wire };
        row.From.Iter(value => arm.From = value.ToDate());
        row.Until.Iter(value => arm.Until = value.ToDate());
        row.UpperKey.Iter(value => arm.UpperKey = value);
        return Frame(row, arm, static (wire, value) => wire.DateInput = value);
    }

    private static ControlIntentWire ToWire(ControlIntent.PathInput row) =>
        Frame(row, new ControlIntentWire.Types.PathInput {
            Mode = Picker(row.Mode),
            Filters = { row.Filters.Map(ToWire) },
            Multiple = row.Multiple,
        }, static (wire, arm) => wire.PathInput = arm);

    private static ControlIntentWire ToWire(ControlIntent.ColorInput row) =>
        Frame(row, new ControlIntentWire.Types.ColorInput { Posture = row.Posture.Wire, Alpha = row.Alpha },
            static (wire, arm) => wire.ColorInput = arm);

    private static ControlIntentWire ToWire(ControlIntent.Select row) =>
        Frame(row, new ControlIntentWire.Types.Select {
            Posture = row.Posture.Wire,
            Options = ToWire(row.Options),
            Window = ToWire(row.Window),
        }, static (wire, arm) => wire.Select = arm);

    private static ControlIntentWire ToWire(ControlIntent.MultiSelect row) =>
        Frame(row, new ControlIntentWire.Types.MultiSelect {
            Posture = row.Posture.Wire,
            Options = ToWire(row.Options),
            Window = ToWire(row.Window),
        }, static (wire, arm) => wire.MultiSelect = arm);

    private static ControlIntentWire ToWire(ControlIntent.Slider row) =>
        Frame(row, new ControlIntentWire.Types.Slider { Min = row.Min, Max = row.Max, Step = row.Step },
            static (wire, arm) => wire.Slider = arm);

    private static ControlIntentWire ToWire(ControlIntent.Range row) =>
        Frame(row, new ControlIntentWire.Types.Range {
            Min = row.Min,
            Max = row.Max,
            Step = row.Step,
            UpperKey = row.UpperKey,
        }, static (wire, arm) => wire.Range = arm);

    private static ControlIntentWire ToWire(ControlIntent.Toggle row) =>
        Frame(row, new ControlIntentWire.Types.Toggle { LabelKey = row.LabelKey },
            static (wire, arm) => wire.Toggle = arm);

    private static ControlIntentWire ToWire(ControlIntent.Radio row) =>
        Frame(row, new ControlIntentWire.Types.Radio { Options = { row.Options.Map(ToWire) } },
            static (wire, arm) => wire.Radio = arm);

    private static ControlIntentWire ToWire(ControlIntent.Segmented row) =>
        Frame(row, new ControlIntentWire.Types.Segmented {
            Posture = row.Posture.Wire,
            Options = { row.Options.Map(ToWire) },
        }, static (wire, arm) => wire.Segmented = arm);

    private static ControlIntentWire ToWire(ControlIntent.Chip row) =>
        Frame(row, new ControlIntentWire.Types.Chip { TextKey = row.TextKey, Posture = row.Posture.Wire },
            static (wire, arm) => wire.Chip = arm);

    private static ControlIntentWire ToWire(ControlIntent.Progress row) {
        var arm = new ControlIntentWire.Types.Progress { Form = row.Form.Wire };
        row.Fraction.Iter(value => arm.Fraction = value);
        return Frame(row, arm, static (wire, value) => wire.Progress = value);
    }

    private static ControlIntentWire ToWire(ControlIntent.Avatar row) =>
        Frame(row, new ControlIntentWire.Types.Avatar {
            Members = { row.Members.Map(ToWire) },
            VisibleLimit = row.VisibleLimit.Value,
        }, static (wire, arm) => wire.Avatar = arm);

    private static ControlIntentWire ToWire(ControlIntent.Breadcrumb row) =>
        Frame(row, new ControlIntentWire.Types.Breadcrumb { Crumbs = { row.Crumbs.Map(ToWire) } },
            static (wire, arm) => wire.Breadcrumb = arm);

    private static ControlIntentWire ToWire(ControlIntent.Tooltip row) =>
        Frame(row, new ControlIntentWire.Types.Tooltip { Hint = ToWire(row.Hint) },
            static (wire, arm) => wire.Tooltip = arm);

    private static ControlIntentWire ToWire(ControlIntent.Banner row) {
        var arm = new ControlIntentWire.Types.Banner {
            HeadlineKey = row.HeadlineKey,
            BodyKey = row.BodyKey,
            Severity = row.Severity.Wire,
            Placement = row.Placement.Wire,
            Actions = { row.Actions.Map(ToWire) },
        };
        row.Evidence.Iter(value => arm.Evidence = ToWire(value));
        return Frame(row, arm, static (wire, value) => wire.Banner = value);
    }

    private static ControlIntentWire ToWire(ControlIntent.EmptyState row) {
        var arm = new ControlIntentWire.Types.EmptyState {
            HeadlineKey = row.HeadlineKey,
            BodyKey = row.BodyKey,
        };
        row.Action.Iter(value => arm.Action = ToWire(value));
        return Frame(row, arm, static (wire, value) => wire.EmptyState = value);
    }

    private static ControlIntentWire ToWire(ControlIntent.Grid row) =>
        Frame(row, new ControlIntentWire.Types.Grid {
            Columns = { row.Columns.Map(ToWire) },
            Window = ToWire(row.Window),
        }, static (wire, arm) => wire.Grid = arm);

    private static ControlIntentWire ToWire(ControlIntent.Tree row) =>
        Frame(row, new ControlIntentWire.Types.Tree {
            Item = ToWire(row.Item),
            ExpansionCommand = row.ExpansionCommand,
            Window = ToWire(row.Window),
        }, static (wire, arm) => wire.Tree = arm);

    private static ControlIntentWire ToWire(ControlIntent.Overview row) =>
        Frame(row, new ControlIntentWire.Types.Overview {
            Axis = row.Axis.Wire,
            SourceKey = row.SourceKey,
            JumpCommand = row.JumpCommand,
        }, static (wire, arm) => wire.Overview = arm);

    private static ControlIntentWire ToWire(ControlIntent.Menu row) =>
        Frame(row, new ControlIntentWire.Types.Menu { Rows = { row.Rows.Map(ToWire) } },
            static (wire, arm) => wire.Menu = arm);

    private static ControlIntentWire ToWire(ControlIntent.Toolbar row) =>
        Frame(row, new ControlIntentWire.Types.Toolbar {
            Rows = { row.Rows.Map(ToWire) },
            Orientation = Orientation(row.Orientation),
        }, static (wire, arm) => wire.Toolbar = arm);

    private static ControlIntentWire ToWire(ControlIntent.Tab row) =>
        Frame(row, new ControlIntentWire.Types.Tab { Pages = { row.Pages.Map(Section) } },
            static (wire, arm) => wire.Tab = arm);

    private static ControlIntentWire ToWire(ControlIntent.Accordion row) =>
        Frame(row, new ControlIntentWire.Types.Accordion { Sections = { row.Sections.Map(Section) } },
            static (wire, arm) => wire.Accordion = arm);

    private static ControlIntentWire ToWire(ControlIntent.Panel row) =>
        Frame(row, new ControlIntentWire.Types.Panel {
            Children = { row.Children.Map(ToWire) },
            ConstraintProgram = row.ConstraintProgram,
        }, static (wire, arm) => wire.Panel = arm);

    private static ControlIntentWire ToWire(ControlIntent.Dock row) =>
        Frame(row, new ControlIntentWire.Types.Dock {
            Regions = { row.Regions.Map(ToWire) },
            ConstraintProgram = row.ConstraintProgram,
        }, static (wire, arm) => wire.Dock = arm);

    private static ControlIntentWire ToWire(ControlIntent.Splitter row) =>
        Frame(row, new ControlIntentWire.Types.Splitter {
            First = ToWire(row.First),
            Second = ToWire(row.Second),
            Orientation = Orientation(row.Orientation),
        }, static (wire, arm) => wire.Splitter = arm);

    private static ControlIntentWire Frame<TArm>(
        ControlIntent source,
        TArm arm,
        Action<ControlIntentWire, TArm> seat) {
        var wire = new ControlIntentWire {
            Key = source.Key,
            Binding = ToWire(source.Binding),
        };
        seat(wire, arm);
        return wire;
    }
}
```

### [05.2]-[CONTROLMAP_ROWS_CS]

```csharp signature
// --- [COMPOSITION] --------------------------------------------------------------------------
public static partial class ControlMap {
    private static IntentBindingWire ToWire(IntentBinding row) {
        var wire = new IntentBindingWire {
            Role = row.Role.Key,
            Emphasis = row.Emphasis.Wire,
        };
        row.Command.Iter(value => wire.Command = value);
        row.ValueKey.Iter(value => wire.ValueKey = value);
        row.Trigger.Iter(value => wire.Trigger = value.Wire);
        row.Icon.Iter(value => wire.Icon = ToWire(value));
        row.Hint.Iter(value => wire.Hint = ToWire(value));
        return wire;
    }

    private static IconSlotWire ToWire(IconSlot row) {
        var wire = new IconSlotWire {
            Asset = row.Asset.Value,
            Placement = Icon(row.Placement),
            Size = row.Size.Value,
        };
        row.Pending.Iter(value => wire.Pending = value);
        return wire;
    }

    private static HintRowWire ToWire(HintRow row) {
        var wire = new HintRowWire { Body = row.Body };
        row.Gesture.Iter(value => wire.Gesture = value.ToString());
        return wire;
    }

    private static OptionRowWire ToWire(OptionRow row) {
        var wire = new OptionRowWire { Value = row.Value, LabelKey = row.LabelKey };
        row.Group.Iter(value => wire.Group = value);
        row.Icon.Iter(value => wire.Icon = ToWire(value));
        return wire;
    }

    private static CrumbRowWire ToWire(CrumbRow row) {
        var wire = new CrumbRowWire { Value = row.Value, LabelKey = row.LabelKey };
        row.Icon.Iter(value => wire.Icon = ToWire(value));
        row.Command.Iter(value => wire.Command = value);
        return wire;
    }

    private static AvatarRowWire ToWire(AvatarRow row) {
        var wire = new AvatarRowWire { LabelKey = row.LabelKey };
        row.Portrait.Iter(value => wire.Portrait = value.Value);
        return wire;
    }

    private static FileFilterRowWire ToWire(FileFilterRow row) =>
        new() { Label = row.Label, Patterns = { row.Patterns } };

    private static MenuRowWire ToWire(MenuRow row) {
        var wire = new MenuRowWire {
            Key = row.Key,
            LabelKey = row.LabelKey,
            Posture = row.Posture.Wire,
            Rows = { row.Rows.Map(ToWire) },
        };
        row.Icon.Iter(value => wire.Icon = ToWire(value));
        row.Gesture.Iter(value => wire.Gesture = value.ToString());
        row.Command.Iter(value => wire.Command = value);
        row.CheckedKey.Iter(value => wire.CheckedKey = value);
        return wire;
    }

    private static ToolbarRowWire ToWire(ToolbarRow row) =>
        new() { Item = ToWire(row.Item), Overflow = Overflow(row.Overflow) };

    private static SectionWire Section((string HeaderKey, ControlIntent Body) row) =>
        new() { HeaderKey = row.HeaderKey, Body = ToWire(row.Body) };

    private static ColumnRowWire ToWire(ColumnRow row) {
        var wire = new ColumnRowWire {
            HeaderKey = row.HeaderKey,
            Cell = ToWire(row.Cell),
            Extent = ToWire(row.Extent),
            Align = Align(row.Align),
        };
        row.Editor.Iter(value => wire.Editor = ToWire(value));
        row.SortKey.Iter(value => wire.SortKey = value);
        return wire;
    }

    private static ExtentWire ToWire(DataGridLength row) =>
        new() { Value = row.Value, Unit = ExtentUnit(row.UnitType) };

    private static WindowWire ToWire(VirtualWindowSpec row) =>
        new() {
            Extent = row.Extent,
            Overscan = row.Overscan,
            Mode = row.Mode.Wire,
            NominalItemExtent = row.NominalExtent.IfNone(VirtualWindowSpec.RowExtent),
        };

    private static OptionSourceWire ToWire(OptionSource source) => source.Switch(
        inline: static row => new OptionSourceWire {
            Inline = new OptionSourceWire.Types.Inline { Rows = { row.Rows.Map(ToWire) } },
        },
        bound: static row => new OptionSourceWire { Bound = row.SourceKey });

    private static NumericRangeWire ToWire(NumericRange range) => range.Switch(
        integral: static row => new NumericRangeWire {
            Integral = new NumericRangeWire.Types.Integral { Min = row.Min, Max = row.Max, Step = row.Step },
        },
        unsigned: static row => new NumericRangeWire {
            Unsigned = new NumericRangeWire.Types.Unsigned { Min = row.Min, Max = row.Max, Step = row.Step },
        },
        real: static row => new NumericRangeWire {
            Real = new NumericRangeWire.Types.Real { Min = row.Min, Max = row.Max, Step = row.Step },
        },
        precise: static row => new NumericRangeWire {
            Precise = new NumericRangeWire.Types.Precise {
                Min = row.Min.ToString(CultureInfo.InvariantCulture),
                Max = row.Max.ToString(CultureInfo.InvariantCulture),
                Step = row.Step.ToString(CultureInfo.InvariantCulture),
            },
        });

}
```

### [05.3]-[CONTROLMAP_ENUMS_CS]

```csharp signature
// --- [COMPOSITION] --------------------------------------------------------------------------
[Mapper(RequiredEnumMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class ControlMap {
    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.IconPlacement.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.IconPlacement Icon(Position value);

    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.PickerMode.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.PickerMode Picker(UsePickerTypes value);

    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.OverflowMode.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.OverflowMode Overflow(OverflowMode value);

    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.Orientation.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.Orientation Orientation(Avalonia.Layout.Orientation value);

    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.ExtentUnit.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.ExtentUnit ExtentUnit(DataGridLengthUnitType value);

    [MapperIgnoreTargetValue(Rasm.Contracts.Ui.V1.ColumnAlign.Unspecified)]
    [MapEnum(EnumMappingStrategy.ByName)]
    private static partial Rasm.Contracts.Ui.V1.ColumnAlign Align(HorizontalAlignment value);

}
```
## [06]-[RESEARCH]

(none)
