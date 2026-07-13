# [APPUI_CONTROL_MATERIALIZATION]

One typed `ControlIntent` family materializes every interactive control from a declarative shape: a screen body is a control-intent stream, not a per-screen XAML literal. `ControlIntent` is the closed `[Union]` over the whole interactive-control vocabulary where arity, provider, and modality live in the intent shape rather than parallel control names, `ControlFactory` is the one polymorphic fold projecting each intent onto its compiled-template Avalonia control, `BehaviorRail.Intent` is the single binding bridge every materialized command rides, `Theme/tokens` roles resolve every visual, and `Shell/accessibility` derives every automation name from the one intent row. The page owns the intent vocabulary, the materialize fold and its boundary capsule, and the `ControlIntentWire` projection; it mints no parallel binding, token, automation, or template path — the `[05]-[PROHIBITIONS]` parallel-control-framework clause forecloses it. The spine is Avalonia compiled `ControlTemplate`/`DataTemplate`/`ControlTheme`, ReactiveUI commands, `Xaml.Behaviors.Avalonia`, Thinktecture.Runtime.Extensions, and LanguageExt rails.

## [01]-[INDEX]

- [01]-[CONTROL_INTENT]: One closed control vocabulary; per-kind typed shape, binding, token, and automation columns.
- [02]-[MATERIALIZE_FOLD]: The `ControlFactory` intent-to-control fold; one `BehaviorRail.Intent` bridge; total automation derivation.
- [03]-[CONTROL_RECYCLING]: The recycling-aware materialization boundary the `VirtualWindow` grid/tree/canvas kinds consume.
- [04]-[TS_PROJECTION]: `ControlIntentWire` kind-discriminated control vocabulary the web head materializes.

## [02]-[CONTROL_INTENT]

- Owner: `ControlIntent` `[Union]` the interactive-control family; `IntentBinding` the per-intent command-and-token column carrier; `ControlFault` the typed fault family on the `AppUiFaultBand.Control` registry row (6010).
- Cases: `ControlIntent` = Button | TextInput | NumberInput | DateInput | PathInput | Select | Slider | Toggle | Radio | Grid | Tree | Menu | Toolbar | Tab | Accordion | Panel | Dock | Splitter under the locked kind literals; `ControlFault` = Text | UnboundIntent | TokenUnresolved | TemplateMissing | RecyclingViolation — codes derive through the `Diagnostics/evidence.md#FAULT_TABLES` registry.
- Entry: every case is one record whose fields carry the control's typed shape (value type, range, option set, child intents) and whose `IntentBinding` carries the `Option<string>` command key, the `TokenRole` visual key, and the `AutomationName` derivation — arity, provider, and modality live in the shape, so a number editor is `NumberInput` not a `GetNumberControl` name.
- Auto: the `EditorFactory` eleven-row typed-shape→control precedent already proven in `PropertyGrid` cells (`Inspector/editor-factories`) is the inspector specialization of this vocabulary — `ControlIntent` generalizes it from property cells to whole screens, so a grid cell editor and a screen field materialize through one fold; `Theme/tokens` `TokenRow` resolves every appearance and `Shell/accessibility` `AutomationProperties.SetName` reads the one `AutomationName` column, so a per-control token literal and a per-control automation call site are the deleted forms.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new control is one `ControlIntent` case carrying its shape plus `IntentBinding`; a new container is one case nesting child intents; zero new surface — the closed eighteen-case family is the axis and a nineteenth parallel control name beside it is the rejected form.
- Boundary: `ControlIntent` is the one control vocabulary in the package — a per-screen control-builder, a second control-generation framework, and a parallel binding/token/automation path are the `[05]-[PROHIBITIONS]` parallel-control-framework rejected forms; the command column is `Option<string>` carrying the `CommandIntent` key the materialized control's `ICommand` resolves through `BehaviorRail.Intent`, never a `ReactiveCommand` instance on the intent (the intent is a serializable shape, the command resolves at materialize) so the intent crosses the `ControlIntentWire` seam unchanged; container kinds (`Grid`, `Tree`, `Tab`, `Accordion`, `Panel`, `Dock`, `Splitter`, `Toolbar`, `Menu`) carry their child-intent sequence so a whole screen is one nested intent tree; the `Grid` and `Tree` kinds carry the `VirtualWindow` window spec the `GENERIC_VIRTUALIZATION_FABRIC` owner consumes — the spec crosses the wire so a remote head windows the same viewport contract — and a windowed control mints no second virtualizer; value-carrying kinds (`TextInput`, `NumberInput`, `DateInput`, `PathInput`, `Slider`, `Toggle`) carry a typed two-way binding path read at materialize; the `Select` and `Radio` option sets are the bounded-choice column; the `Dock` and `Splitter` kinds defer their layout to the `LayoutConstraint`/`LayoutSolver` owner (`Shell/solver`) so the intent names the constraint program and the panel solves it.

```csharp signature
[Union]
public abstract partial record ControlFault : Expected, IValidationError<ControlFault> {
    private ControlFault(string detail, int code) : base(detail, code, None) { }

    public static ControlFault Create(string message) => new Text(message);

    public sealed record Text : ControlFault { public Text(string detail) : base(detail, AppUiFaultBand.Control.Code(0)) { } }
    public sealed record UnboundIntent : ControlFault { public UnboundIntent(string detail) : base(detail, AppUiFaultBand.Control.Code(1)) { } }
    public sealed record TokenUnresolved : ControlFault { public TokenUnresolved(string detail) : base(detail, AppUiFaultBand.Control.Code(2)) { } }
    public sealed record TemplateMissing : ControlFault { public TemplateMissing(string detail) : base(detail, AppUiFaultBand.Control.Code(3)) { } }
    public sealed record RecyclingViolation : ControlFault { public RecyclingViolation(string detail) : base(detail, AppUiFaultBand.Control.Code(4)) { } }
}

public sealed record IntentBinding(
    string AutomationName,
    string TokenRole,
    Option<string> Command,
    Option<string> ValuePath);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlIntent(string Key, IntentBinding Binding) {
    public sealed record Button(string Key, string Content, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record TextInput(string Key, string Watermark, bool Multiline, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record NumberInput(string Key, double Min, double Max, double Increment, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record DateInput(string Key, Option<LocalDate> Min, Option<LocalDate> Max, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record PathInput(string Key, PathBrowseMode Mode, Seq<string> Filters, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Select(string Key, Seq<(string Value, string Label)> Options, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Slider(string Key, double Min, double Max, double Step, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Toggle(string Key, string Label, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Radio(string Key, Seq<(string Value, string Label)> Options, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Grid(string Key, Seq<ControlIntent> Columns, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Tree(string Key, VirtualWindowSpec Window, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Menu(string Key, Seq<ControlIntent> Items, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Toolbar(string Key, Seq<ControlIntent> Items, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Tab(string Key, Seq<(string Header, ControlIntent Body)> Pages, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Accordion(string Key, Seq<(string Header, ControlIntent Body)> Sections, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Panel(string Key, Seq<ControlIntent> Children, string ConstraintProgram, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Dock(string Key, Seq<ControlIntent> Regions, string ConstraintProgram, IntentBinding Binding) : ControlIntent(Key, Binding);
    public sealed record Splitter(string Key, ControlIntent First, ControlIntent Second, Orientation Orientation, IntentBinding Binding) : ControlIntent(Key, Binding);

    public Seq<ControlIntent> Children => Switch(
        grid: static c => c.Columns, menu: static c => c.Items, toolbar: static c => c.Items,
        tab: static c => c.Pages.Map(static p => p.Body), accordion: static c => c.Sections.Map(static s => s.Body),
        panel: static c => c.Children, dock: static c => c.Regions, splitter: static c => Seq(c.First, c.Second),
        button: static _ => Seq<ControlIntent>(), textInput: static _ => Seq<ControlIntent>(), numberInput: static _ => Seq<ControlIntent>(),
        dateInput: static _ => Seq<ControlIntent>(), pathInput: static _ => Seq<ControlIntent>(), select: static _ => Seq<ControlIntent>(),
        slider: static _ => Seq<ControlIntent>(), toggle: static _ => Seq<ControlIntent>(), radio: static _ => Seq<ControlIntent>(),
        tree: static _ => Seq<ControlIntent>());
}

public enum PathBrowseMode { File, Directory, SaveFile }
```

## [03]-[MATERIALIZE_FOLD]

- Owner: `ControlFactory` the one intent-to-control fold; `MaterializeContext` the composition-bound resolution columns; `ControlReceipt` the materialization evidence record.
- Entry: `public Fin<Control> Materialize(ControlIntent intent, MaterializeContext context)` — one polymorphic fold (intent → realized control) over the closed family; the `Fin` rail aborts on an unbound command key, an unresolved token role, or a recycling violation, sealing the typed `ControlFault`.
- Auto: each arm constructs the compiled-template Avalonia control (`Button`, `TextBox`, `NumericUpDown`, `CalendarDatePicker`, `ComboBox`, `Slider`, `ToggleSwitch`, `RadioButton` group, `DataGrid` over the `VirtualWindow` source, `TreeView`-as-flat-`TreeRow` over the same window, `Menu`, `ToolBar`, `TabControl`, `Expander` accordion, the `LayoutSolver` panel, `DockControl`, `GridSplitter`), binds its `ICommand` through `BehaviorRail.Intent(context.Command(key))` exclusively, resolves every brush and metric through `context.Token(role)`, and applies the automation identity through `AccessOps.Identify`-shaped `AutomationProperties.SetName` from the one `IntentBinding.AutomationName` — no per-kind materializer call site, no runtime-XAML emission, no second binding bridge.
- Receipt: `ControlReceipt` — intent key, control type name, bound command key, resolved token role, `Instant` — sealed through the screen evidence stream; `TelemetryRow` contributes the control-materialized and control-rejected instruments inward through the AppHost `TelemetryContributorPort`.
- Packages: Avalonia, Avalonia.Controls.DataGrid, Xaml.Behaviors.Avalonia, ReactiveUI, LanguageExt.Core, NodaTime
- Growth: one fold arm per new `ControlIntent` case; a new container is one nesting arm recursing `Materialize` over child intents; one control instrument is one `InstrumentRow` on `ControlFactory.TelemetryRow`; zero new surface.
- Boundary: `ControlFactory` is the named boundary capsule for the control-construction statement carve-out — each arm carries the Avalonia control-construction statements while the dispatch stays one total generated `Switch`, so a new case breaks every site at compile time and a runtime `_` arm is the rejected form; the only `ICommand` binding bridge is `BehaviorRail.Intent`, so `PropertyBinderImplementation.Bind`/`OneWayBind`/`BindTo`, `CommandBinder.BindCommand`, and `IViewFor` property-expression wiring are rejected wholesale (the `[05]-[PROHIBITIONS]` ReactiveUI-code-behind clause) and a `view.OneWayBind(vm, x => x.Prop, v => v.Control.Text)` call site is the deleted form; the materialized control's view-to-view-model binding is XAML compiled-`{Binding}` over the `ValuePath`, never a C# binder call; templates are compiled `ControlTemplate`/`DataTemplate`/`ControlTheme` resolved from the theme, so `AvaloniaXamlLoader.Load` runtime materialization is the rejected form (`[05]-[PROHIBITIONS]` runtime-XAML clause) and `Surfaces.RejectRuntimeXaml` is the never-callable structural rejection; the `Grid` and `Tree` arms hand their `VirtualWindowSpec` to the `Shell/virtualization` `VirtualWindow` owner so windowing rides the one fabric and a factory-local virtualizer is the rejected form; the `Panel` and `Dock` arms hand their `ConstraintProgram` to the `Shell/solver` `LayoutSolver` panel and mount their children through `Mounted`, which stamps `LayoutSolver.ChildKeyProperty` from each child intent's `Key` before the child enters the panel — the one admitted source of the solver's child identity, so a nullable `Control.Name` fallback and a per-panel side mapping are the rejected forms; the command key resolves against the boot-frozen `CommandDeck` so an unknown key aborts the materialize on the `Fin` rail rather than binding a dead control.

```csharp signature
public sealed record MaterializeContext(
    Func<string, Option<ICommand>> Command,
    Func<string, Option<IBrush>> Token,
    Func<string, Option<double>> Metric,
    Func<VirtualWindowSpec, IObservable<IChangeSet<RealizedItem<object>, object>>> Window,
    Func<string, Fin<Control>> Layout,
    ClockPolicy Clocks);

public sealed record ControlReceipt(string IntentKey, string ControlType, Option<string> Command, string TokenRole, Instant At) {
    public const string Kind = "control";
}

public static class ControlFactory {
    public const string MaterializedInstrument = "rasm.appui.control.materialized";
    public const string RejectedInstrument = "rasm.appui.control.rejected";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, MaterializedInstrument, RejectedInstrument);

    public static Fin<Control> Materialize(ControlIntent intent, MaterializeContext context) =>
        Visual(intent, context).Bind(control => Bind(intent.Binding, control, context));

    static Fin<Control> Visual(ControlIntent intent, MaterializeContext context) => intent.Switch(
        state: context,
        button: static (ctx, c) => Fin<Control>.Succ(new Button { Content = c.Content }),
        textInput: static (ctx, c) => Fin<Control>.Succ(new TextBox { Watermark = c.Watermark, AcceptsReturn = c.Multiline }),
        numberInput: static (ctx, c) => Fin<Control>.Succ(new NumericUpDown { Minimum = (decimal)c.Min, Maximum = (decimal)c.Max, Increment = (decimal)c.Increment }),
        dateInput: static (ctx, c) => Fin<Control>.Succ(new CalendarDatePicker()),
        pathInput: static (ctx, c) => Fin<Control>.Succ(new TextBox { Watermark = c.Mode.ToString() }),
        select: static (ctx, c) => Fin<Control>.Succ(new ComboBox { ItemsSource = c.Options.Map(static o => o.Label).ToArray() }),
        slider: static (ctx, c) => Fin<Control>.Succ(new Slider { Minimum = c.Min, Maximum = c.Max, TickFrequency = c.Step }),
        toggle: static (ctx, c) => Fin<Control>.Succ(new ToggleSwitch { Content = c.Label }),
        radio: static (ctx, c) => Fin<Control>.Succ(RadioGroup(c.Options)),
        grid: static (ctx, c) => Fin<Control>.Succ(Windowed(new DataGrid { AutoGenerateColumns = false }, ctx.Window(c.Window))),
        tree: static (ctx, c) => Fin<Control>.Succ(Windowed(new DataGrid { AutoGenerateColumns = false }, ctx.Window(c.Window))),
        menu: static (ctx, c) => Children(c.Items, ctx).Map(static items => (Control)new Menu { ItemsSource = items.ToArray() }),
        toolbar: static (ctx, c) => Children(c.Items, ctx).Map(static items => (Control)new ItemsControl { ItemsSource = items.ToArray() }),
        tab: static (ctx, c) => Pages(c.Pages, ctx).Map(static pages => (Control)new TabControl { ItemsSource = pages.ToArray() }),
        accordion: static (ctx, c) => Sections(c.Sections, ctx).Map(static panels => (Control)new StackPanel().With(panels)),
        panel: static (ctx, c) => Mounted(ctx.Layout(c.ConstraintProgram), c.Children, ctx),
        dock: static (ctx, c) => Mounted(ctx.Layout(c.ConstraintProgram), c.Regions, ctx),
        splitter: static (ctx, c) => Split(c, ctx));

    // The solver child-identity admission: every solved child is stamped ChildKeyProperty from its OWN
    // intent Key here, so LayoutSolver.SolvedRect always resolves a program-owner key and a keyless
    // child is structurally unmountable — never a post-arrange failure.
    static Fin<Control> Mounted(Fin<Control> layout, Seq<ControlIntent> children, MaterializeContext context) =>
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

    // The recycling re-entry: MaterializePool routes a reset parked control back through the one Bind fold.
    public static Fin<Control> Rebind(IntentBinding binding, Control control, MaterializeContext context) =>
        Bind(binding, control, context);

    static Fin<Control> Bind(IntentBinding binding, Control control, MaterializeContext context) =>
        from command in binding.Command.Match(
            Some: key => context.Command(key).Map(Fin<Option<ICommand>>.Succ).IfNone(() => Fin<Option<ICommand>>.Fail(new ControlFault.UnboundIntent(key))),
            None: () => Fin<Option<ICommand>>.Succ(None))
        from brush in context.Token(binding.TokenRole).Map(Fin<IBrush>.Succ).IfNone(() => Fin<IBrush>.Fail(new ControlFault.TokenUnresolved(binding.TokenRole)))
        select Apply(control, binding, command, brush, context);

    // The one bound-collection hop: the realized change-set binds ONCE into a ReadOnlyObservableCollection
    // the grid consumes — ItemsSource never receives the raw stream — and the subscription parks weak-keyed
    // on the control so a freed grid frees its window binding with it.
    static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, IDisposable> LiveWindows = new();

    static Control Windowed(DataGrid grid, IObservable<IChangeSet<RealizedItem<object>, object>> window) {
        LiveWindows.Add(grid, window.Bind(out ReadOnlyObservableCollection<RealizedItem<object>> view).Subscribe());
        grid.ItemsSource = view;
        return grid;
    }

    // Recycling owns binding LIFETIME: the live ValuePath subscription parks here (weak-keyed, so an
    // unparked control frees with GC) and Apply/Unbind dispose it before rebinding or resetting.
    static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, IDisposable> LiveBindings = new();

    internal static Unit Unbind(Control control) {
        if (LiveBindings.TryGetValue(control, out IDisposable? live)) { live.Dispose(); ignore(LiveBindings.Remove(control)); }
        return unit;
    }

    // Apply covers the full token set the role carries: brush, metric (font size + min touch height),
    // value-path binding, automation identity, and the one behavior bridge — idempotent for recycling:
    // the prior binding disposes before the new one registers, so a re-applied control never stacks two.
    static Control Apply(Control control, IntentBinding binding, Option<ICommand> command, IBrush brush, MaterializeContext context) {
        AutomationProperties.SetName(control, binding.AutomationName);
        control.SetValue(TemplatedControl.ForegroundProperty, brush);
        context.Metric($"{binding.TokenRole}.font-size").Iter(size => control.SetValue(TemplatedControl.FontSizeProperty, size));
        context.Metric($"{binding.TokenRole}.min-height").Iter(height => control.SetValue(Layoutable.MinHeightProperty, height));
        ignore(Unbind(control));
        binding.ValuePath.Iter(path => LiveBindings.Add(control, control.Bind(
            ContentPropertyOf(control), new Avalonia.Data.Binding(path) { Mode = Avalonia.Data.BindingMode.TwoWay })));
        Interaction.GetBehaviors(control).Clear();
        command.Iter(cmd => Interaction.GetBehaviors(control).Add(new EventTriggerBehavior {
            EventName = ActivationEventOf(control), Actions = { BehaviorRail.Intent(cmd) },
        }));
        return control;
    }

    // Per-control value property (TextBox.Text, NumericUpDown.Value, ...) — one table, no per-kind binder.
    internal static AvaloniaProperty ContentPropertyOf(Control control) => control switch {
        TextBox => TextBox.TextProperty,
        NumericUpDown => NumericUpDown.ValueProperty,
        Slider => RangeBase.ValueProperty,
        ToggleSwitch => ToggleButton.IsCheckedProperty,
        ComboBox => SelectingItemsControl.SelectedItemProperty,
        _ => ContentControl.ContentProperty,
    };

    // Per-control activation event — the sibling table to ContentPropertyOf: the behavior bridge routes
    // the intent off the control's OWN activation semantics; Tapped is the pointer-activation floor every
    // InputElement raises, never a Button-only Click assumption.
    internal static string ActivationEventOf(Control control) => control switch {
        MenuItem => nameof(MenuItem.Click),
        Button => nameof(Button.Click),
        SelectingItemsControl => nameof(SelectingItemsControl.SelectionChanged),
        _ => nameof(InputElement.Tapped),
    };
}
```

[MATERIALIZE_LAW]:
- One bridge: every `ICommand` rides `BehaviorRail.Intent` — a `BindCommand` call site is the deleted form and the intent never carries a live command, only its key.
- One token: every brush, metric, and motion resolves through `MaterializeContext.Token` from the `TokenRow` vocabulary — a hardcoded brush is the deleted form.
- One automation: every control's name is `IntentBinding.AutomationName` applied through `AutomationProperties.SetName` — a per-control automation call site is the deleted form.
- Compiled templates only: controls construct against compiled `ControlTemplate`/`ControlTheme`; runtime XAML is rejected wholesale.
- Recursive containers: container arms recurse `Materialize` over child intents so a whole screen is one fold over one nested intent tree.

## [04]-[CONTROL_RECYCLING]

- Owner: `RecycleScope` the realized-control reuse pool; `MaterializePool` the recycling-aware materialization over the `VirtualWindow` window.
- Entry: `public Fin<Control> Realize(ControlIntent intent, MaterializeContext context, RecycleScope scope)` — materializes through the pool, reusing a parked control of the same intent key when the window scrolls a row out and back, sealing a `RecyclingViolation` when an intent key crosses control types.
- Auto: the `Grid`, `Tree`, and `Panel` kinds materialize their row/cell controls through `MaterializePool` keyed by intent key so the `VirtualWindow` recycles realized controls over a data window rather than re-materializing per scroll tick; a parked control resets its bindings to the new row's `ValuePath` and re-applies its token role, so a recycled cell carries no stale state; the pool size tracks the `VirtualWindow` realized-item count so the pool never holds more controls than the window realizes.
- Packages: Avalonia, LanguageExt.Core, BCL inbox
- Growth: a new recyclable kind is one pool-key entry; zero new surface.
- Boundary: control recycling rides the one `VirtualWindow` owner (`Shell/virtualization`) — a per-surface recycling pool is the `[05]-[PROHIBITIONS]` per-surface-virtualizer rejected form, and the pool is keyed by intent key so a recycled control always matches its intent type; the pool resets bindings on reuse so a recycled control never leaks the prior row's value; the realized-item count bounds the pool so recycling is constant-cost; a `RecyclingViolation` fault fires when an intent key reuses a control of a different type, so a pool-key collision aborts on the `Fin` rail rather than mounting a mismatched control.

```csharp signature
public sealed record RecycleScope(string WindowKey, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Stack<Control>> Pool) {
    public static RecycleScope Of(string windowKey) => new(windowKey, new(StringComparer.Ordinal));

    public Option<Control> Park(string intentKey) =>
        Pool.TryGetValue(intentKey, out var stack) && stack.Count > 0 ? Some(stack.Pop()) : None;

    public Unit Return(string intentKey, Control control) =>
        fun(() => (Pool.TryGetValue(intentKey, out var stack) ? stack : Pool[intentKey] = new()).Push(control))();
}

public static class MaterializePool {
    extension(RecycleScope scope) {
        public Fin<Control> Realize(ControlIntent intent, MaterializeContext context) =>
            scope.Park(intent.Key).Match(
                Some: parked => Rebind(parked, intent, context),
                None: () => ControlFactory.Materialize(intent, context));

        // The recycling core: a parked control genuinely RESETS — stale bindings clear, the new row's
        // ValuePath re-binds, and the token role re-applies through the one Bind fold — never a bare return.
        private static Fin<Control> Rebind(Control parked, ControlIntent intent, MaterializeContext context) =>
            parked.GetType().Name == ControlTypeOf(intent)
                ? Reset(parked).Bind(cleared => ControlFactory.Rebind(intent.Binding, cleared, context))
                : Fin<Control>.Fail(new ControlFault.RecyclingViolation($"{intent.Key}:{parked.GetType().Name}!={ControlTypeOf(intent)}"));

        private static Fin<Control> Reset(Control parked) {
            ignore(ControlFactory.Unbind(parked)); // disposes the prior row's ValuePath subscription
            parked.ClearValue(ControlFactory.ContentPropertyOf(parked)); // clears the resolved value surface
            parked.ClearValue(TemplatedControl.ForegroundProperty);
            parked.ClearValue(TemplatedControl.FontSizeProperty);
            parked.ClearValue(Layoutable.MinHeightProperty);
            parked.DataContext = null;
            Interaction.GetBehaviors(parked).Clear();
            return Fin<Control>.Succ(parked);
        }
    }
}
```

```mermaid
flowchart LR
    ControlIntent --> ControlFactory
    ControlFactory --> Visual
    Visual --> Bind
    Bind -->|Command| BehaviorRail.Intent
    Bind -->|Token| TokenRow
    Bind -->|Name| AutomationProperties
    Bind --> ControlReceipt
    ControlIntent -->|Grid/Tree/Panel| VirtualWindow
    ControlFactory --> ControlIntentWire
```

## [05]-[TS_PROJECTION]

- Owner: `ControlIntentWire`, `IntentBindingWire`, `ControlReceiptWire` — the kind-discriminated control wire contract the TypeScript head materializes; the `csharp:Rasm.AppUi/Controls` mint emits `ControlIntentWire` over the `ControlIntent` family that `typescript:core/interchange/codec` decodes and `typescript:ui/viewer` materializes (`viewer/panel`), so a web/remote caller materializes the same control vocabulary the desktop renders.
- Packages: BCL inbox
- Growth: one wire member row per new intent field; one kind literal per new control case; zero new surface.
- Boundary: shapes transcribe the camelCase Strict emission — the control kind crosses as the locked kind literal, the command crosses as the `CommandIntent` ordinal-string key (never a live command), the token role crosses as the `TokenRow` string key, the automation name crosses as a string, value paths cross as ordinal strings, date bounds cross as ISO-8601 date text or null, the `Grid`/`Tree` window spec crosses whole as `VirtualWindowSpecWire` so a remote head reconstructs the same viewport contract, and container kinds carry their child-intent arrays so a nested screen crosses as one nested intent tree; every field a wire-visible `ControlIntent` case owns has a wire representation, so a new case field is one detectable contract-pressure point and a web-only default for an omitted field is the rejected form; the wire carries the intent shape only — the realized control, the bound command, and the resolved brush never cross because the web head materializes them from the same vocabulary; this is the `ONE_UI_INTENT_WIRE` counterpart, so the `ControlIntentWire` strength/kind parity matches the `LayoutConstraintWire` ordered-program invariant.

```ts contract
interface IntentBindingWire {
  readonly automationName: string;
  readonly tokenRole: string;
  readonly command: string | null;
  readonly valuePath: string | null;
}

interface VirtualWindowSpecWire {
  readonly extent: number;
  readonly overscan: number;
  readonly mode: "fixed" | "measured";
  readonly fixedItemExtent: number;
}

type ControlIntentWire =
  | { readonly kind: "button"; readonly key: string; readonly content: string; readonly binding: IntentBindingWire }
  | { readonly kind: "textInput"; readonly key: string; readonly watermark: string; readonly multiline: boolean; readonly binding: IntentBindingWire }
  | { readonly kind: "numberInput"; readonly key: string; readonly min: number; readonly max: number; readonly increment: number; readonly binding: IntentBindingWire }
  | { readonly kind: "dateInput"; readonly key: string; readonly min: string | null; readonly max: string | null; readonly binding: IntentBindingWire }
  | { readonly kind: "pathInput"; readonly key: string; readonly mode: string; readonly filters: readonly string[]; readonly binding: IntentBindingWire }
  | { readonly kind: "select"; readonly key: string; readonly options: readonly { readonly value: string; readonly label: string }[]; readonly binding: IntentBindingWire }
  | { readonly kind: "slider"; readonly key: string; readonly min: number; readonly max: number; readonly step: number; readonly binding: IntentBindingWire }
  | { readonly kind: "toggle"; readonly key: string; readonly label: string; readonly binding: IntentBindingWire }
  | { readonly kind: "radio"; readonly key: string; readonly options: readonly { readonly value: string; readonly label: string }[]; readonly binding: IntentBindingWire }
  | { readonly kind: "grid"; readonly key: string; readonly columns: readonly ControlIntentWire[]; readonly window: VirtualWindowSpecWire; readonly binding: IntentBindingWire }
  | { readonly kind: "tree"; readonly key: string; readonly window: VirtualWindowSpecWire; readonly binding: IntentBindingWire }
  | { readonly kind: "menu"; readonly key: string; readonly items: readonly ControlIntentWire[]; readonly binding: IntentBindingWire }
  | { readonly kind: "toolbar"; readonly key: string; readonly items: readonly ControlIntentWire[]; readonly binding: IntentBindingWire }
  | { readonly kind: "tab"; readonly key: string; readonly pages: readonly { readonly header: string; readonly body: ControlIntentWire }[]; readonly binding: IntentBindingWire }
  | { readonly kind: "accordion"; readonly key: string; readonly sections: readonly { readonly header: string; readonly body: ControlIntentWire }[]; readonly binding: IntentBindingWire }
  | { readonly kind: "panel"; readonly key: string; readonly children: readonly ControlIntentWire[]; readonly constraintProgram: string; readonly binding: IntentBindingWire }
  | { readonly kind: "dock"; readonly key: string; readonly regions: readonly ControlIntentWire[]; readonly constraintProgram: string; readonly binding: IntentBindingWire }
  | { readonly kind: "splitter"; readonly key: string; readonly first: ControlIntentWire; readonly second: ControlIntentWire; readonly orientation: string; readonly binding: IntentBindingWire };

interface ControlReceiptWire { readonly intentKey: string; readonly controlType: string; readonly command: string | null; readonly tokenRole: string; readonly at: string; }
```

## [06]-[RESEARCH]

- [CONTROL_TEMPLATE]: the compiled `ControlTemplate`/`ControlTheme` resolution the materialize fold reads per kind — the `TemplatedControl.Template` and `ControlTheme` lookup against the theme dictionary the `Theme/tokens` owner emits, and the `DataGridTemplateColumn.CellTemplate` compiled-template binding for the `Grid`/`Tree` cell intents — resolved at implementation against the Avalonia 12 compiled-template surface; the `ControlIntent` union, the `ControlFactory` fold, the `BehaviorRail.Intent` bridge, and the `AutomationProperties.SetName` derivation are settled, the per-kind compiled-template member spellings are the unverified surface bound at composition.
