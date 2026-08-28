# 1. Delete the unsupported masked-control case

From:

`[02]-[SPEC]` — `ControlSpec.Masked`
```csharp
    public sealed record Masked(ElementSpec Spec, string Mask, string Seed) : ControlSpec(Spec);
```

To:

```csharp
// ControlSpec.Masked DELETED
```

Why: Eto exposes concealed entry through `PasswordBox` but no mask-pattern member; `TextRole.Secret` already owns the supported behavior and the `Mask` value has no realization target.

Change: Express concealed entry as `ControlSpec.Text` with `TextRole.Secret` and delete the unrealizable case.

Delta: Net -1 declared LOC and -1 nested union case/type.

# 2. Delete the invalid element factory

From:

`[02]-[SPEC]` — `ElementSpec`
```csharp
public sealed record ElementSpec(
    ElementKey Key,
    ElementState State,
    Option<string> ToolTip,
    Option<StyleKey> Style,
    Seq<IBindingPlan> Bindings) {
    public static ElementSpec Of(ElementKey key) =>
        new(State: ElementState.Active, ToolTip: None, Style: None, Bindings: Seq<IBindingPlan>());
}
```

To:

```csharp
public sealed record ElementSpec(
    ElementKey Key,
    ElementState State,
    Option<string> ToolTip,
    Option<StyleKey> Style,
    Seq<IBindingPlan> Bindings);
```

Why: `Of` omits the required `Key` argument and adds a second construction surface that cannot produce its declared result.

Change: Delete the factory and construct the positional record explicitly.

Delta: Net -3 declared LOC and -1 module-level member.

# 3. Replace alpha mode with the host property value

From:

`[03]-[ROLES]` — `AlphaMode`
```csharp
[SmartEnum<int>]
public sealed partial class AlphaMode {
    public static readonly AlphaMode Opaque = new(key: 0,
        seat: static picker => HostEdge.Side(() => picker.AllowAlpha = false));
    public static readonly AlphaMode Alpha = new(key: 1,
        seat: static picker => HostEdge.Side(() => picker.AllowAlpha = true));

    [UseDelegateFromConstructor] internal partial Unit Seat(ColorPicker picker);
}
```

To:

```csharp
// AlphaMode DELETED
```

Why: The owner copies one boolean into `ColorPicker.AllowAlpha`; it carries no independent identity, admission, or distinct operation.

Change: Replace `ControlSpec.Colour.Alpha` with `bool AllowAlpha` and assign it in the realization arm.

Delta: Net -8 declared LOC, -1 module-level type, and -3 declared members.

Ripples: Replace `PickerSpec.Shade.Alpha` in `libs/dotnet/Rasm/.planning/Interaction/chrome.md` and its `ask.Alpha` consumer in `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md` with `bool AllowAlpha`.

# 4. Replace inspector mode with the host property value

From:

`[03]-[ROLES]` — `InspectorMode`
```csharp
[SmartEnum<int>]
public sealed partial class InspectorMode {
    public static readonly InspectorMode Flat = new(key: 0,
        seat: static grid => HostEdge.Side(() => grid.ShowCategories = false));
    public static readonly InspectorMode Grouped = new(key: 1,
        seat: static grid => HostEdge.Side(() => grid.ShowCategories = true));

    [UseDelegateFromConstructor] internal partial Unit Seat(PropertyGrid grid);
}
```

To:

```csharp
// InspectorMode DELETED
```

Why: The two rows are opposite literals for `PropertyGrid.ShowCategories`; the boolean already is the complete policy.

Change: Replace `ControlSpec.Inspector.Mode` with `bool ShowCategories` and assign it directly.

Delta: Net -8 declared LOC, -1 module-level type, and -3 declared members.

# 5. Replace stretch mode with one placement value

From:

`[03]-[ROLES]` — `Stretch`
```csharp
[SmartEnum<int>]
public sealed partial class Stretch {
    public static readonly Stretch Fixed = new(key: 0,
        stack: static item => HostEdge.Side(() => item.Expand = false),
        slot: static cell => HostEdge.Side(() => cell.ScaleWidth = false));
    public static readonly Stretch Fill = new(key: 1,
        stack: static item => HostEdge.Side(() => item.Expand = true),
        slot: static cell => HostEdge.Side(() => cell.ScaleWidth = true));

    [UseDelegateFromConstructor] internal partial Unit Stack(StackLayoutItem item);
    [UseDelegateFromConstructor] internal partial Unit Slot(TableCell cell);
}
```

To:

```csharp
// Stretch DELETED
```

Why: Both delegate columns copy the same boolean to the corresponding placement property; the wrapper duplicates that value with a type, two rows, and two methods.

Change: Replace `StackChild.Stretch` and `TableSlot.Stretch` with `bool Expand` and assign it to `StackLayoutItem.Expand` or `TableCell.ScaleWidth`.

Delta: Net -10 declared LOC, -1 module-level type, and -4 declared members.

# 6. Split progress bars from spinners

From:

`[03]-[ROLES]` — `ProgressRole`
```csharp
[SmartEnum<int>]
public sealed partial class ProgressRole {
    public static readonly ProgressRole Track = new(key: 0,
        mint: static (fraction, floor, ceiling) => fraction.Match<Control>(
            Some: held => new ProgressBar { MinValue = floor, MaxValue = ceiling, Value = held },
            None: static () => new ProgressBar { Indeterminate = true }));

    public static readonly ProgressRole Pulse = new(key: 1,
        mint: static (_, _, _) => new ProgressBar { Indeterminate = true });

    public static readonly ProgressRole Spin = new(key: 2,
        mint: static (_, _, _) => new Spinner { Enabled = true });

    [UseDelegateFromConstructor] internal partial Control Mint(Option<int> fraction, int floor, int ceiling);
}
```

To:

```csharp
// ProgressRole DELETED
```

Why: `Pulse` duplicates `Track` with an absent fraction, while `Spin` ignores all three progress-bar arguments; the row family hides two different payload shapes.

Change: Keep `ControlSpec.Progress(ElementSpec, Option<int>, int, int)` for determinate or indeterminate bars, add parameterless-payload `ControlSpec.Spinner(ElementSpec)`, and construct both directly in generated dispatch arms.

Delta: Net at least -9 declared LOC, -1 module-level type, and -4 role members, with +1 nested union case/type.

# 7. Construct checkbox lists through their own data store

From:

`[03]-[ROLES]` — `ChoiceRole.CheckSet`
```csharp
    public static readonly ChoiceRole CheckSet = new(key: 5,
        mint: static (options, _, axis) => Listed(new CheckBoxList { Orientation = axis }, options, None),
        read: static (control, _) => new FieldValue.PickSet(Keys: toSeq(((CheckBoxList)control).SelectedKeys)));
```

To:

```csharp
    public static readonly ChoiceRole CheckSet = new(key: 5,
        mint: static (options, _, axis) => new CheckBoxList { Orientation = axis, DataStore = options },
        read: static (control, _) => new FieldValue.PickSet(Keys: toSeq(((CheckBoxList)control).SelectedKeys)));
```

Why: `CheckBoxList` is a `Panel`, not a `ListControl`, so it cannot enter `Listed`; it owns its option roster through `DataStore`.

Change: Construct the checkbox list directly and assign its data store.

Delta: Net 0 declared LOC, types, and members; removes one invalid helper call.

# 8. Use canonical keyless cell rows

From:

`[03]-[ROLES]` — `CellKind`
```csharp
[SmartEnum<int>]
public sealed partial class CellKind {
    public static readonly CellKind Script = new(key: 0,
        mint: static (column, policy) => new TextBoxCell(column) { TextAlignment = policy.Align });
    public static readonly CellKind Mark = new(key: 1, mint: static (column, _) => new CheckBoxCell(column));
    public static readonly CellKind Pick = new(key: 2,
        mint: static (column, policy) => new ComboBoxCell(column) { DataStore = policy.Options });
    public static readonly CellKind Figure = new(key: 3, mint: static (column, _) => new ImageViewCell(column));
    public static readonly CellKind Gauge = new(key: 4, mint: static (column, _) => new ProgressCell(column));
    public static readonly CellKind Duo = new(key: 5,
        mint: static (column, policy) => new ImageTextCell(column, policy.Companion.IfNone(column)));

    [UseDelegateFromConstructor] internal partial Cell Mint(int column, CellPolicy policy);
}
```

To:

```csharp
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
```

Why: These process-local behavior rows need generated dispatch but no persisted integer identity, and the current names obscure the exact Eto cell semantics.

Change: Make the roster keyless and rename each row for the cell it constructs.

Delta: Net 0 declared LOC, types, and members; removes the generated keyed lookup and conversion surface.

# 9. Delete text-policy no-op surfaces

From:

`[03]-[ROLES]` — `TextPolicy`
```csharp
public sealed record TextPolicy(Option<string> Placeholder, Option<int> MaxLength, CapabilitySet<EditTrait> Traits) {
    internal Fin<CapabilitySet<EditTrait>> Admitted => EditTrait.Law.Admit(held: Traits);

    public static TextPolicy Default => Seed.Value;
    private static readonly Lazy<TextPolicy> Seed = new(static () => new(
        Placeholder: None,
        MaxLength: None,
        Traits: CapabilitySet<EditTrait>.Of(EditTrait.Editable, EditTrait.Wrapping, EditTrait.Return)));
}
```

To:

```csharp
public sealed record TextPolicy(
    Option<string> Placeholder,
    Option<int> MaxLength,
    CapabilitySet<EditTrait> Traits);
```

Why: `EditTrait.Law` is open and cannot refuse, while the unused lazy default adds two members around one ordinary value.

Change: Read `Traits` directly and construct required policy values at their consumers.

Delta: Net -5 declared LOC and -3 module-level members.

# 10. Delete the unused number-policy default

From:

`[03]-[ROLES]` — `NumberPolicy`
```csharp
public sealed record NumberPolicy(
    Option<double> Floor, Option<double> Ceiling, double Increment, int Decimals, Option<string> Format, bool Wrap) {
    public static NumberPolicy Default => Seed.Value;
    private static readonly Lazy<NumberPolicy> Seed = new(static () => new(
        Floor: None, Ceiling: None, Increment: 1d, Decimals: 2, Format: None, Wrap: false));
}
```

To:

```csharp
public sealed record NumberPolicy(
    Option<double> Floor, Option<double> Ceiling, double Increment, int Decimals, Option<string> Format, bool Wrap);
```

Why: Neither default member has a consumer, and the lazy cell defers only a constant record construction.

Change: Delete `Default` and `Seed`.

Delta: Net -4 declared LOC and -2 module-level members.

# 11. Delete slider and cell default wrappers

From:

`[03]-[ROLES]` — `SliderPolicy` and `CellPolicy`
```csharp
public sealed record SliderPolicy(int Tick, bool Snap, Orientation Axis) {
    public static SliderPolicy Default => Seed.Value;
    private static readonly Lazy<SliderPolicy> Seed = new(static () => new(Tick: 0, Snap: false, Axis: Orientation.Horizontal));
}

public sealed record CellPolicy(TextAlignment Align, Seq<object> Options, Option<int> Companion) {
    public static CellPolicy Default => Seed.Value;
    private static readonly Lazy<CellPolicy> Seed = new(static () => new(
        Align: TextAlignment.Left, Options: Seq<object>(), Companion: None));
}
```

To:

```csharp
public sealed record SliderPolicy(int Tick, bool Snap, Orientation Axis);

public sealed record CellPolicy(TextAlignment Align, Seq<object> Options, Option<int> Companion);
```

Why: Both lazy fields wrap constant record constructors; the slider default is unused and the cell default exists only for the removable column factory.

Change: Delete both default surfaces.

Delta: Net -7 declared LOC and -4 module-level members.

# 12. Delete the column convenience factory

From:

`[03]-[ROLES]` — `ColumnPlan<TRow>`
```csharp
public sealed record ColumnPlan<TRow>(
    string Header,
    Func<TRow, object> Read,
    CellSpec Cell,
    CapabilitySet<ColumnTrait> Traits,
    Option<int> Width,
    Option<string> Tip) {
    public static ColumnPlan<TRow> Of(string header, Func<TRow, object> read) => new(
        Header: header,
        Read: read,
        Cell: new CellSpec.Bound(Kind: CellKind.Script, Policy: CellPolicy.Default),
        Traits: CapabilitySet<ColumnTrait>.Of(ColumnTrait.Sortable, ColumnTrait.Resizable),
        Width: None,
        Tip: None);
}
```

To:

```csharp
public sealed record ColumnPlan<TRow>(
    string Header,
    Func<TRow, object> Read,
    CellSpec Cell,
    CapabilitySet<ColumnTrait> Traits,
    Option<int> Width,
    Option<string> Tip);
```

Why: `Of` is unused and hides cell and capability data behind a second construction vocabulary.

Change: Delete the factory and construct the plan directly with the renamed cell row and an explicit policy.

Delta: Net -8 declared LOC and -1 module-level member.

# 13. Delete open-law grid admission

From:

`[03]-[ROLES]` — `GridPlan<TRow>.Admitted`
```csharp
    internal Fin<Unit> Admitted =>
        from grid in GridTrait.Law.Admit(held: Traits)
        from held in Columns.Traverse(column => ColumnTrait.Law.Admit(held: column.Traits)).As()
        select unit;
```

To:

```csharp
// GridPlan<TRow>.Admitted DELETED
```

Why: Both capability laws are open, so the query and traversal manufacture failure flow around inputs that cannot fail.

Change: Read the grid and column trait sets directly during realization.

Delta: Net -4 declared LOC and -1 module-level member.

# 14. Remove invalid cell-dispatch arguments

From:

`[03]-[ROLES]` — `CellSpec.Mint`
```csharp
    internal Cell Mint(int column, FaultCell faults) => Switch(
        state: (Column: column, Faults: faults),
        bound: static (held, cell) => cell.Kind.Mint(column: held.Column, policy: cell.Policy),
        custom: static (held, cell) => Templated(cell, held.Faults, held.Key),
        drawn: static (held, cell) => Painted(cell, held.Faults, held.Key));
```

To:

```csharp
    internal Cell Mint(int column, FaultCell faults) => Switch(
        state: (Column: column, Faults: faults),
        bound: static (held, cell) => cell.Kind.Mint(column: held.Column, policy: cell.Policy),
        custom: static (held, cell) => Templated(cell, held.Faults),
        drawn: static (held, cell) => Painted(cell, held.Faults));
```

Why: The state tuple has no `Key`, and both declared target methods accept only the case and `FaultCell`.

Change: Call the custom and drawn constructors at their declared arity.

Delta: Net 0 declared LOC, types, and members; removes two invalid argument expressions.

# 15. Represent absent pick text explicitly

From:

`[04]-[CAPTURE]` — `FieldValue.Pick`
```csharp
    public sealed record Pick(Option<int> Index, string Text) : FieldValue;
```

To:

```csharp
    public sealed record Pick(Option<int> Index, Option<string> Text) : FieldValue;
```

Why: Drop-down and list selections currently encode no selection as `string.Empty`, which collides with a legitimate empty option or editable combo-box text.

Change: Return `None` when a non-editable control has no selected item and `Some(control.Text)` for editable combo-box text.

Delta: Net 0 declared LOC, types, and members; removes one sentinel encoding.

Ripples: Update `FieldEntry.Choice`, `FieldJson`, and export-field projections in `libs/dotnet/Rasm.AppUi/.planning/Editing/forms.md` and `libs/dotnet/Rasm.AppUi/.planning/Document/export.md` to unwrap `Pick.Text` explicitly.

# 16. Delete the field-guard wrapper and retained control

From:

`[04]-[CAPTURE]` — `FieldGuard` and `FieldPort`
```csharp
public sealed record FieldGuard(Func<FieldValue, Fin<FieldValue>> Admit);

public sealed record FieldPort(FieldTag Tag, Control Editor, Func<Fin<FieldValue>> Pick, Option<FieldGuard> Guard);
```

To:

```csharp
public sealed record FieldPort(
    FieldTag Tag,
    Func<Fin<FieldValue>> Pick,
    Option<Func<FieldValue, Fin<FieldValue>>> Admit);
```

Why: `FieldGuard` only forwards to one delegate, and `Pick` already closes over the editor; retaining both adds a type and a live-control reference without capability.

Change: Store the admission function directly on the port and remove `Editor`.

Delta: Net -1 declared LOC, -1 module-level type, and -2 declared members.

Ripples: Replace `ControlSpec.Field.Guard` with `Option<Func<FieldValue, Fin<FieldValue>>>` in the target specification.

# 17. Return the admitted field map directly

From:

`[04]-[CAPTURE]` — `FieldReport`
```csharp
public sealed record FieldReport(HashMap<FieldTag, FieldValue> Values, HashMap<FieldTag, FieldGuard> Guards) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Guards.ForAll((tag, guard) =>
        Values.Find(tag).Exists(value => guard.Admit(value).IsSucc)));

    public Option<FieldValue> Value(FieldTag tag) => Values.Find(tag);
}
```

To:

```csharp
// FieldReport DELETED
```

Why: A successful harvest has already admitted every independently read value; retaining guards re-validates an immutable snapshot, and `Value` forwards one `HashMap.Find` call.

Change: Make `ElementMount.Harvest` return `Fin<HashMap<FieldTag, FieldValue>>` and pass that map directly to choice projections.

Delta: Net -6 declared LOC, -1 module-level type, and -4 declared members.

Ripples: Change `PromptChoice<TResult>.Project` in `libs/dotnet/Rasm/.planning/Interaction/chrome.md` to accept `HashMap<FieldTag, FieldValue>`, replace `FieldReport.Value(tag)` with `values.Find(tag)`, and remove the obsolete report rule from `libs/dotnet/Rasm/RULINGS.md`.

# 18. Delete control-mint convenience factories

From:

`[05]-[REALIZE]` — `ControlMint`
```csharp
public readonly record struct ControlMint(
    Lease<Control> Host,
    Seq<Lease<IDisposable>> Resources,
    Seq<ControlMint> Children,
    Option<Func<Fin<FieldValue>>> Pick) {
    public static ControlMint Leaf(Control host) => new(
        Host: new Lease<Control>.Owned(host),
        Resources: Seq<Lease<IDisposable>>(), Children: Seq<ControlMint>(), Pick: None);

    public static ControlMint Editor(Control host, Func<Fin<FieldValue>> pick) => new(
        Host: new Lease<Control>.Owned(host),
        Resources: Seq<Lease<IDisposable>>(), Children: Seq<ControlMint>(), Pick: Some(pick));

    internal Fin<Unit> Drain();
}
```

To:

```csharp
public readonly record struct ControlMint(
    Lease<Control> Host,
    Seq<Lease<IDisposable>> Resources = default,
    Seq<ControlMint> Children = default,
    Option<Func<Fin<FieldValue>>> Pick = default) {
    internal Fin<Unit> Drain();
}
```

Why: `Leaf` and `Editor` only fill default collection and option values around the positional constructor.

Change: Use the constructor directly, naming `Pick` only for editors and `Children` only for composite mints.

Delta: Net -6 declared LOC and -2 module-level members.

Ripples: Replace `ControlMint.Leaf` and `ControlMint.Editor` calls in `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md` with direct construction.

# 19. Absorb construction operations into the mount owner

From:

`[05]-[REALIZE]` — `ControlForge`
```csharp
public static class ControlForge {
    public static Fin<Lease<ElementMount>> Realize(ControlSpec spec, ElementRuntime runtime);

    public static Fin<ElementMount> Grow(ControlSpec spec, ElementRuntime runtime);

    public static Fin<Seq<ElementMount>> GrowAll(Seq<ControlSpec> specs, ElementRuntime runtime);
}
```

To:

```csharp
// ControlForge DELETED
```

Why: Construction already belongs to `ElementMount`; a separate static owner adds three coined entry names, and `GrowAll` duplicates the internal `Gather` operation without a consumer.

Change: Move the affinity-gated entry to `ElementMount.Create`, move the current-thread entry to `ElementMount.CreateCurrent`, and delete `GrowAll`.

Delta: Net -5 declared LOC, -1 module-level type, and -1 net module-level member after moving the two retained operations.

Ripples: Replace `ControlForge.Realize` and `ControlForge.Grow` in `libs/dotnet/Rasm.Rhino/.planning/HostUi/pages.md`, `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md`, and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/chrome.md`; update `libs/dotnet/Rasm.Rhino/ARCHITECTURE.md` and `libs/dotnet/Rasm/RULINGS.md`.

# 20. Execute view verbs against the mounted host

From:

`[05]-[REALIZE]` — `ElementMount.Drive`
```csharp
    public Fin<ViewEcho> Drive(Grid view, ViewVerb verb);
```

To:

```csharp
    public Fin<ViewEcho> Execute(ViewVerb verb);
```

Why: Accepting an arbitrary `Grid` lets a mount mutate a control it does not own; the mounted `Host` is the sole target and already supports a typed non-grid refusal.

Change: Resolve `Host` inside generated verb dispatch and use the canonical execution name.

Delta: Net 0 declared LOC and -1 public parameter; member count is unchanged.

# 21. Couple reload depth to a subtree target

From:

`[06]-[VIEW]` — `ViewVerb.Reload`
```csharp
    public sealed record Reload(Option<ITreeGridItem> Scope, bool Children) : ViewVerb;
```

To:

```csharp
    public sealed record Reload(Option<(ITreeGridItem Item, bool Children)> Target) : ViewVerb;
```

Why: Whole-grid reload has no child flag, so `Reload(None, true)` carries a meaningless state that every consumer must ignore.

Change: Use `None` for `ReloadData()` and `Some((item, children))` for `ReloadItem(item, children)`.

Delta: Net 0 declared LOC and -1 generated case property.

# 22. Delete unsupported grid point probing

From:

`[06]-[VIEW]` — `ViewVerb.Probe`
```csharp
    public sealed record Probe(EtoPointF At) : ViewVerb;
```

To:

```csharp
// ViewVerb.Probe DELETED
```

Why: Eto `Grid` exposes selection, editing, scrolling, and tree reload operations but no row-and-column point hit-test.

Change: Delete the verb and its unused `EtoPointF` import alias.

Delta: Net -1 declared LOC, -1 nested union case/type, and -1 case member.

# 23. Delete the unsupported hit echo

From:

`[06]-[VIEW]` — `ViewEcho.Hit`
```csharp
    public sealed record Hit(Option<ITreeGridItem> Item, int Column) : ViewEcho;
```

To:

```csharp
// ViewEcho.Hit DELETED
```

Why: `Hit` exists only as the answer to the unsupported `Probe` verb and has no producing host operation.

Change: Delete the unreachable echo case.

Delta: Net -1 declared LOC, -1 nested union case/type, and -2 case members.

# 24. Delete the duplicate notice declarations

From:

`[06]-[VIEW]` — `NoticeRole`, `NoticeChoice<TResult>`, and `ThemedNotice<TResult>`
```csharp
[SmartEnum<int>]
public sealed partial class NoticeRole {
    public static readonly NoticeRole Ordinary = new(key: 0);
    public static readonly NoticeRole Default = new(key: 1);
    public static readonly NoticeRole Abort = new(key: 2);
}

public sealed record NoticeChoice<TResult>(string Caption, TResult Result, NoticeRole Role);

public sealed record ThemedNotice<TResult>(
    string Text, TextAlignment Alignment, Option<Lease<EtoImage>> Badge, Seq<NoticeChoice<TResult>> Choices) {
    public Fin<Lease<NoticeMount<TResult>>> Mint();
}
```

To:

```csharp
// NoticeRole, NoticeChoice<TResult>, and ThemedNotice<TResult> DELETED
```

Why: `Prompt<TResult>` already owns result-typed choices, arbitrary `ControlSpec` content, dismissal, refusal posture, presentation, and one marshal window.

Change: Express notices as `Prompt<TResult>` values and render text or badges through the existing control tree.

Delta: Net -11 declared LOC, -3 module-level types, and at least -8 declared members.

Ripples: Use `Prompt<TResult>` and `PromptChoice<TResult>` from `libs/dotnet/Rasm/.planning/Interaction/chrome.md` and remove the notice names from the target index, owner prose, and imports.

# 25. Delete the duplicate notice mount

From:

`[06]-[VIEW]` — `NoticeMount<TResult>`
```csharp
public sealed class NoticeMount<TResult> : IDisposable {
    public ThemedMessageBox Host { get; }

    public Option<TResult> Reply { get; }

    public Fin<Unit> Release();

    public void Dispose() => ignore(Release());
}
```

To:

```csharp
// NoticeMount<TResult> DELETED
```

Why: `PromptMount<TResult>` already owns the dialog, typed settlement, release, and disposal; the second mount also exposes a live host unnecessarily.

Change: Let the existing prompt mount own the modal lifetime and typed outcome.

Delta: Net -8 declared LOC, -1 module-level type, and -4 declared members.
