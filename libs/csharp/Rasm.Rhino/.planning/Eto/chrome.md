# [RASM_RHINO_ETO_CHROME]

`IntentTable` materializes one interactive-verb vocabulary into commands, menus, context menus, and toolbars. `ShellPlan` and `Prompt<TResult>` retain realized element receipts, and `PrintPlan` defers one page-evidenced document run whose pages replay the same `PaintProgram` seam the canvas surface mounts.

## [01]-[INDEX]

- [02]-[INTENT_TABLE]: `IntentRow`, `CommandKind`, and `IntentTable` own command materialization, invocation evidence, state refresh, and chrome projection.
- [03]-[WINDOWS]: `ShellPlan` realizes modeless and utility windows under `ShellReceipt` custody.
- [04]-[MODAL_RAIL]: `Prompt<TResult>` returns typed answers, dismissal, cancellation, and host failure without default sentinels.
- [05]-[PRINT_FLOW]: `PrintPlan` defers page rendering, composes `PageSettings`, and derives completion from every page fact.

## [02]-[INTENT_TABLE]

- Owner: `IntentTable` mints each command once and projects every placement from that command instance.
- Entry: `Materialize` accumulates independent identity, gesture, and placement conflicts before construction; command access, refresh, and every chrome projection share the active lifecycle rail.
- Receipt: every UI or programmatic execution appends one `IntentReceipt` carrying the exact `Fin<Unit>` outcome.
- Growth: a verb is one `IntentRow`; a surface occurrence is one `PlacementSlot`; a command modality is one `CommandKind` case.
- Boundary: `Command.Execute` remains inside the table, and host events retain their otherwise-unreturnable result in `Receipts`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct IntentKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Intent identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct PlacementKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Placement identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandKind {
    private CommandKind() { }
    public sealed record Act(Func<Fin<Unit>> Effect) : CommandKind;
    public sealed record Toggle(Func<bool> Read, Action<bool> Write) : CommandKind;
    public sealed record Pick(string Group, Func<bool> Read, Action Choose) : CommandKind;

    internal Fin<Unit> Execute(Command host, Op key) => Switch(
        state: (Host: host, Key: key),
        act: static (held, kind) => held.Key.Catch(kind.Effect),
        toggle: static (held, kind) => held.Key.Catch(() => Fin.Succ(Op.Side(() =>
            kind.Write(held.Host is CheckCommand check && check.Checked)))),
        pick: static (held, kind) => held.Key.Catch(() => Fin.Succ(Op.Side(kind.Choose))));

    internal Unit Refresh(Command host) => Switch(
        state: host,
        act: static (_, _) => unit,
        toggle: static (command, kind) => command is CheckCommand check ? Op.Side(() => check.Checked = kind.Read()) : unit,
        pick: static (command, kind) => command is RadioCommand radio ? Op.Side(() => radio.Checked = kind.Read()) : unit);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PlacementSlot(PlacementKey Place, int Rank, int Group, Option<string> SubMenu);

public sealed record IntentRow(
    IntentKey Key,
    string MenuText,
    Option<string> ToolText,
    Option<string> Hint,
    Option<Image> Icon,
    Option<Keys> Gesture,
    Func<bool> Available,
    CommandKind Kind,
    Seq<PlacementSlot> Slots);

public sealed record IntentReceipt(IntentKey Key, long Ordinal, Fin<Unit> Outcome) : IValidityEvidence {
    public bool IsValid => Outcome.IsSucc;
}

internal sealed record BoundIntent(IntentRow Row, Command Command, EventHandler<EventArgs> Executed);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class IntentTable : UiLease {
    private readonly Seq<BoundIntent> bound;
    private readonly Atom<Seq<IntentReceipt>> receipts;
    private readonly Op key;

    private IntentTable(Seq<BoundIntent> bound, Atom<Seq<IntentReceipt>> receipts, Op key) {
        this.bound = bound;
        this.receipts = receipts;
        this.key = key;
    }

    public Seq<IntentReceipt> Receipts => receipts.Value;

    public static Fin<IntentTable> Materialize(Seq<IntentRow> rows, Op? key = null) {
        Op op = key.OrDefault();
        return Distinct(rows, op).Bind(_ => op.Catch(() => {
            System.Collections.Generic.Dictionary<string, RadioCommand> controllers = new(StringComparer.Ordinal);
            Atom<Seq<IntentReceipt>> facts = Atom(Seq<IntentReceipt>());
            return rows.Fold(Fin.Succ(Seq<BoundIntent>()), (rail, row) =>
                rail.Bind(held => op.Catch(() => Fin.Succ(Bind(row, controllers, facts, op)))
                    .Map(held.Add)
                    .MapFail(fault => fault.Also(Severed(held, op)))))
                .Map(bound => (Bound: bound, Facts: facts));
        }).Map(held => new IntentTable(held.Bound, held.Facts, op)));
    }

    public Fin<Command> Command(IntentKey key, Op? op = null) =>
        Active(op.OrDefault()).Bind(_ => Find(key, op.OrDefault()).Map(static entry => entry.Command));

    public Fin<Unit> Invoke(IntentKey key, Op? op = null) =>
        Active(op.OrDefault()).Bind(_ => Find(key, op.OrDefault()).Bind(entry => Execute(entry, op.OrDefault(), receipts)));

    public Fin<Unit> RefreshAvailability(Op? key = null) {
        Op op = key.OrDefault();
        return Active(op).Bind(_ => bound.Map(entry => (Action)(() => {
                entry.Command.Enabled = entry.Row.Available();
                _ = entry.Row.Kind.Refresh(entry.Command);
            })).Drained(op));
    }

    public Fin<MenuBar> MenuOf(PlacementKey place, Op? key = null) =>
        Project(place, key.OrDefault(), static placed => Chromed(new MenuBar(), placed));

    public Fin<ContextMenu> PopupOf(PlacementKey place, Op? key = null) =>
        Project(place, key.OrDefault(), static placed => Chromed(new ContextMenu(), placed));

    public Fin<ToolBar> BarOf(PlacementKey place, Op? key = null) =>
        Project(place, key.OrDefault(), Barred);

    private static ToolBar Barred(Seq<(PlacementSlot Slot, BoundIntent Entry)> placed) {
        ToolBar bar = new();
        _ = placed.Fold(Option<int>.None, (group, pair) => {
            _ = Op.SideWhen(group.Map(value => value != pair.Slot.Group).IfNone(false), () => bar.Items.Add(new SeparatorToolItem()));
            bar.Items.Add(pair.Entry.Command.CreateToolItem());
            return Some(pair.Slot.Group);
        });
        return bar;
    }

    protected override Fin<Unit> Free() => Severed(bound, key);

    private static BoundIntent Bind(
        IntentRow row,
        System.Collections.Generic.Dictionary<string, RadioCommand> controllers,
        Atom<Seq<IntentReceipt>> facts,
        Op op) {
        Command command = Mint(row, controllers);
        BoundIntent? bound = null;
        EventHandler<EventArgs> executed = (_, _) => ignore(Execute(bound!, op, facts));
        bound = new BoundIntent(row, command, executed);
        command.Executed += executed;
        return bound;
    }

    private static Fin<Unit> Execute(BoundIntent entry, Op op, Atom<Seq<IntentReceipt>> sink) {
        Fin<Unit> outcome = op.Catch(() => Fin.Succ(entry.Row.Available())).Bind(available => available
            ? entry.Row.Kind.Execute(entry.Command, op)
            : Fin.Fail<Unit>(new UiFault.Unavailable(Key: op, Capability: entry.Row.Key.Value)));
        _ = sink.Swap(held => held.Add(new IntentReceipt(entry.Row.Key, held.Count + 1, outcome)));
        return outcome;
    }

    private Fin<BoundIntent> Find(IntentKey key, Op op) =>
        bound.Find(entry => entry.Row.Key == key)
            .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: key.Value));

    private Fin<Unit> Active(Op op) => Released
        ? Fin.Fail<Unit>(new UiFault.Released(Key: op, Resource: nameof(IntentTable)))
        : Fin.Succ(unit);

    private Fin<THost> Project<THost>(
        PlacementKey place,
        Op op,
        Func<Seq<(PlacementSlot Slot, BoundIntent Entry)>, THost> materialize) =>
        Active(op).Bind(_ => op.Catch(() => Fin.Succ(materialize(Placed(place)))));

    private static THost Chromed<THost>(
        THost host,
        Seq<(PlacementSlot Slot, BoundIntent Entry)> placed) where THost : Menu, ISubmenu {
        System.Collections.Generic.Dictionary<string, ButtonMenuItem> branches = new(StringComparer.Ordinal);
        _ = placed.Fold(Option<int>.None, (group, pair) => {
            MenuItem item = pair.Entry.Command.CreateMenuItem();
            _ = Op.SideWhen(group.Map(value => value != pair.Slot.Group).IfNone(false) && pair.Slot.SubMenu.IsNone,
                () => host.Items.Add(new SeparatorMenuItem()));
            _ = pair.Slot.SubMenu.Match(
                Some: title => {
                    ref ButtonMenuItem? seat = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(branches, title, out bool chained);
                    ButtonMenuItem branch = seat ??= new ButtonMenuItem { Text = title };
                    _ = Op.SideWhen(!chained, () => host.Items.Add(branch));
                    return Op.Side(() => branch.Items.Add(item));
                },
                None: () => Op.Side(() => host.Items.Add(item)));
            return Some(pair.Slot.Group);
        });
        return host;
    }

    private Seq<(PlacementSlot Slot, BoundIntent Entry)> Placed(PlacementKey place) =>
        toSeq(bound.Bind(entry => entry.Row.Slots
                .Filter(slot => slot.Place == place)
                .Map(slot => (Slot: slot, Entry: entry)))
            .OrderBy(static pair => (pair.Slot.Group, pair.Slot.Rank)));

    private static Command Mint(IntentRow row, System.Collections.Generic.Dictionary<string, RadioCommand> controllers) {
        Command command = row.Kind.Switch(
            act: static _ => new Command(),
            toggle: static kind => (Command)new CheckCommand { Checked = kind.Read() },
            pick: kind => {
                RadioCommand radio = new() { Checked = kind.Read() };
                ref RadioCommand? seat = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(controllers, kind.Group, out bool chained);
                RadioCommand? controller = seat;
                seat ??= radio;
                _ = Op.SideWhen(chained, () => radio.Controller = controller!);
                return radio;
            });
        command.ID = row.Key.Value;
        command.MenuText = row.MenuText;
        command.ToolBarText = row.ToolText.IfNone(row.MenuText);
        command.ToolTip = row.Hint.IfNone(row.MenuText);
        command.Enabled = row.Available();
        _ = row.Icon.Iter(value => command.Image = value);
        _ = row.Gesture.Iter(value => command.Shortcut = value);
        return command;
    }

    private static Fin<Unit> Distinct(Seq<IntentRow> rows, Op op) =>
        (Gate(
             rows.GroupBy(static row => row.Key).ForAll(static group => group.Count() == 1),
             new UiFault.Rejected(Key: op, Field: nameof(IntentRow.Key), Reason: "duplicate intent identity")),
         Gate(
             rows.Choose(static row => row.Gesture).GroupBy(static gesture => gesture).ForAll(static group => group.Count() == 1),
             new UiFault.Rejected(Key: op, Field: nameof(IntentRow.Gesture), Reason: "duplicate gesture")),
         Gate(
             rows.Bind(static row => row.Slots)
                 .GroupBy(static slot => (slot.Place, slot.Group, slot.Rank))
                 .ForAll(static group => group.Count() == 1),
             new UiFault.Rejected(Key: op, Field: nameof(IntentRow.Slots), Reason: "duplicate placement rank")))
        .Apply(static (_, _, _) => unit)
        .As()
        .ToFin();

    private static K<Validation<Error>, Unit> Gate(bool accepted, Error fault) =>
        (accepted ? Fin.Succ(unit) : Fin.Fail<Unit>(fault)).ToValidation();

    private static Fin<Unit> Severed(Seq<BoundIntent> entries, Op op) =>
        entries.Rev().Map(entry => (Action)(() => entry.Command.Executed -= entry.Executed)).Drained(op);
}
```

## [03]-[WINDOWS]

- Owner: `ShellPlan` carries window shape, capabilities, content, and placement keys; `ShellReceipt` owns the window and its realized element subtree.
- Entry: `Realize` requires the injected `ElementRuntime`; `Present` shows the same owned receipt.
- Receipt: teardown detaches content before disposing the element tree and window, so ownership never doubles through Eto's visual tree.
- Growth: another window kind is one `ShellKind` row, another capability is one `ShellCapability` bit consumed in `Realize`, and another placement or presentation property is one `Option` field applied in `Realize`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ShellKind {
    public static readonly ShellKind Modeless = new(key: 0, mint: static () => new Form());
    public static readonly ShellKind Utility = new(key: 1, mint: static () => new FloatingForm());
    [UseDelegateFromConstructor]
    internal partial Form Mint();
}

[Flags]
public enum ShellCapability {
    None = 0,
    Resize = 1 << 0,
    Maximize = 1 << 1,
    Minimize = 1 << 2,
    Close = 1 << 3,
    Topmost = 1 << 4,
    Taskbar = 1 << 5,
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ShellPlan(
    ShellKind Kind,
    string Title,
    Element Content,
    ShellCapability Capabilities,
    Option<Size> ClientSize,
    Option<Point> Location,
    Option<WindowState> State,
    Option<Icon> Icon,
    WindowStyle Style,
    Option<Window> Owner,
    Option<(IntentTable Table, PlacementKey Place)> Menu,
    Option<(IntentTable Table, PlacementKey Place)> Bar) {

    public Fin<ShellReceipt> Realize(ElementRuntime runtime, Op? key = null) {
        Op op = key.OrDefault();
        Form? window = null;
        return Content.Realize(runtime, op).Bind(body =>
            Menu.Match(
                Some: value => value.Table.MenuOf(value.Place, op).Map(static menu => Optional(menu)),
                None: static () => Fin.Succ(Option<MenuBar>.None))
            .Bind(menu => Bar.Match(
                Some: value => value.Table.BarOf(value.Place, op).Map(static bar => Optional(bar)),
                None: static () => Fin.Succ(Option<ToolBar>.None))
                .Bind(bar => op.Catch(() => {
                    Form owned = window = Kind.Mint();
                    owned.Title = Title;
                    owned.Content = body.Host;
                    owned.Resizable = Capabilities.HasFlag(ShellCapability.Resize);
                    owned.Maximizable = Capabilities.HasFlag(ShellCapability.Maximize);
                    owned.Minimizable = Capabilities.HasFlag(ShellCapability.Minimize);
                    owned.Closeable = Capabilities.HasFlag(ShellCapability.Close);
                    owned.Topmost = Capabilities.HasFlag(ShellCapability.Topmost);
                    owned.ShowInTaskbar = Capabilities.HasFlag(ShellCapability.Taskbar);
                    owned.WindowStyle = Style;
                    _ = ClientSize.Iter(value => owned.ClientSize = value);
                    _ = Location.Iter(value => owned.Location = value);
                    _ = State.Iter(value => owned.WindowState = value);
                    _ = Icon.Iter(value => owned.Icon = value);
                    _ = Owner.Iter(value => owned.Owner = value);
                    _ = menu.Iter(value => owned.Menu = value);
                    _ = bar.Iter(value => owned.ToolBar = value);
                    return Fin.Succ(new ShellReceipt(owned, body, op));
                })))
            .MapFail(fault => Optional(window)
                .Map(owned => fault.Also(ShellReceipt.Severed(owned, body, op)))
                .IfNone(() => fault.Also(body.Release()))));
    }

    public Fin<ShellReceipt> Present(ElementRuntime runtime, Op? key = null) {
        Op op = key.OrDefault();
        return Realize(runtime, op).Bind(receipt => op.Catch(() => {
            receipt.Window.Show();
            return Fin.Succ(receipt);
        }).MapFail(fault => fault.Also(receipt.Release())));
    }
}

public sealed class ShellReceipt : UiLease {
    private readonly ElementReceipt body;
    private readonly Op key;

    internal ShellReceipt(Form window, ElementReceipt body, Op key) {
        Window = window;
        this.body = body;
        this.key = key;
    }

    public Form Window { get; }

    protected override Fin<Unit> Free() => Severed(Window, body, key);

    internal static Fin<Unit> Severed(Window window, ElementReceipt body, Op op) =>
        Seq<Func<Fin<Unit>>>(
            () => op.Catch(() => Fin.Succ(Op.Side(() => window.Content = null))),
            body.Release,
            () => op.Catch(() => Fin.Succ(Op.Side(window.Dispose)))).Drained(op);
}
```

## [04]-[MODAL_RAIL]

- Owner: `Prompt<TResult>` builds one `Dialog<Option<Fin<TResult>>>`; `PromptLease<TResult>` owns its dialog and content receipt without a parallel verdict store.
- Entry: `Ask` admits the prompt before realizing content and guards either the owner-bound `ShowModal` presenter or an injected presenter; `AskAsync` marshals construction, presentation, cancellation close, and release through awaitable UI crossings.
- Seam: presentation is the presenter value — a host boundary hands its own modal presenter (`ShellWindows.Present` at the Rhino boundary), so the semi-modal host contract owns every host-parented prompt and raw `ShowModal` reaches only host-free shells.
- Receipt: choices and cancellation close with an explicit `Fin<TResult>`, while native dismissal projects `Option.None` to `UiFault.Dismissed` without a result sentinel.
- Growth: another affirmative outcome is one `PromptChoice<TResult>` row; cancellation and dismissal remain distinct faults.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PromptChoice<TResult>(string Caption, Func<Fin<TResult>> Project);

public sealed record Prompt<TResult>(
    string Title,
    Element Content,
    Seq<PromptChoice<TResult>> Choices,
    Option<string> CancelCaption,
    Option<Size> ClientSize,
    DialogDisplayMode DisplayMode,
    Action<Error> Report) {

    public Fin<TResult> Ask(ElementRuntime runtime, Control owner, Op? key = null) {
        Op op = key.OrDefault();
        return Ask(runtime, present: dialog => Fin.Succ(dialog.ShowModal(owner)), key: op);
    }

    public Fin<TResult> Ask(
        ElementRuntime runtime,
        Func<Dialog<Option<Fin<TResult>>>, Fin<Option<Fin<TResult>>>> present,
        Op? key = null) {
        Op op = key.OrDefault();
        return Build(runtime, op).Bind(lease =>
            op.Catch(() => present(lease.Dialog))
                .Bind(verdict => PromptLease<TResult>.Settle(verdict, op))
                .Sealed(lease.Release()));
    }

    public async Task<Fin<TResult>> AskAsync(
        ElementRuntime runtime,
        Control owner,
        CancellationToken cancellationToken,
        Op? key = null) {
        Op op = key.OrDefault();
        Fin<PromptLease<TResult>> built = await UiThread.Run(
            new UiDispatch<PromptLease<TResult>>.Awaited(() => Build(runtime, op)), op).ConfigureAwait(false);
        return await built.Match(
            Succ: lease => AskOwnedAsync(lease, owner, cancellationToken, op),
            Fail: static fault => Task.FromResult(Fin.Fail<TResult>(fault))).ConfigureAwait(false);
    }

    private static async Task<Fin<TResult>> AskOwnedAsync(
        PromptLease<TResult> lease,
        Control owner,
        CancellationToken cancellationToken,
        Op op) {
        Fin<TResult> outcome;
        try {
            Task<Option<Fin<TResult>>> shown = Application.Instance.InvokeAsync(() => lease.Dialog.ShowModal(owner));
            TaskCompletionSource<Unit> cancelled = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenRegistration cancellation = cancellationToken.Register(
                static signal => ((TaskCompletionSource<Unit>)signal!).TrySetResult(unit), cancelled);
            Task completed = await Task.WhenAny(shown, cancelled.Task).ConfigureAwait(false);
            if (ReferenceEquals(completed, cancelled.Task)) {
                await Application.Instance.InvokeAsync(lease.Cancel).ConfigureAwait(false);
            }
            outcome = PromptLease<TResult>.Settle(await shown.ConfigureAwait(false), op);
        } catch (Exception thrown) {
            outcome = Fin.Fail<TResult>(new UiFault.HostRejected(Key: op, Detail: thrown.Message));
        }
        Fin<Unit> cleanup = await UiThread.Run(new UiDispatch<Unit>.Awaited(lease.Release), op).ConfigureAwait(false);
        return outcome.Sealed(cleanup);
    }

    private Fin<PromptLease<TResult>> Build(ElementRuntime runtime, Op op) {
        Dialog<Option<Fin<TResult>>>? dialog = null;
        return Admit(op).Bind(_ => Content.Realize(runtime, op)).Bind(body => op.Catch(() => {
                Dialog<Option<Fin<TResult>>> owned = dialog = new Dialog<Option<Fin<TResult>>> {
                    Title = Title,
                    Content = body.Host,
                    DisplayMode = DisplayMode,
                };
                _ = ClientSize.Iter(value => owned.ClientSize = value);
                Seq<Button> buttons = Choices.Map(choice => Answered(owned, choice, Report, op)).Strict();
                owned.DefaultButton = buttons[0];
                _ = CancelCaption.Iter(caption => {
                    Button cancel = new() { Text = caption };
                    cancel.Click += (_, _) =>
                        _ = op.Catch(() => Fin.Succ(Op.Side(() =>
                            owned.Close(Some(Fin.Fail<TResult>(new UiFault.Cancelled(Key: op))))))).Match(
                                Succ: static closed => closed,
                                Fail: fault => Op.Side(() => Report(fault)));
                    owned.NegativeButtons.Add(cancel);
                    owned.AbortButton = cancel;
                });
                return Fin.Succ(new PromptLease<TResult>(owned, body, op));
            }).MapFail(fault => Optional(dialog)
                .Map(owned => fault.Also(ShellReceipt.Severed(owned, body, op)))
                .IfNone(() => fault.Also(body.Release()))));
    }

    private Fin<Unit> Admit(Op op) => (Title, Content, Report) switch {
        (var title, _, _) when string.IsNullOrWhiteSpace(title) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Title), Reason: "prompt title requires text")),
        (_, null, _) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Content), Reason: "prompt content is absent")),
        (_, _, null) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Report), Reason: "prompt reporter is absent")),
        _ when Choices.IsEmpty =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Choices), Reason: "a prompt requires an answer choice")),
        _ when CancelCaption.Exists(static caption => string.IsNullOrWhiteSpace(caption)) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(CancelCaption), Reason: "cancel caption requires text")),
        _ => Choices.Map((choice, index) => (Choice: choice, Index: index))
            .Find(static row => row.Choice is null || string.IsNullOrWhiteSpace(row.Choice.Caption) || row.Choice.Project is null)
            .Match(
                Some: row => Fin.Fail<Unit>(new UiFault.Rejected(
                    Key: op, Field: nameof(Choices), Reason: $"choice {row.Index} requires caption text and a projection")),
                None: () => Fin.Succ(unit)),
    };

    private static Button Answered(
        Dialog<Option<Fin<TResult>>> dialog,
        PromptChoice<TResult> choice,
        Action<Error> report,
        Op op) {
        Button button = new() { Text = choice.Caption };
        button.Click += (_, _) =>
            _ = op.Catch(() => Fin.Succ(Op.Side(() => dialog.Close(Some(op.Catch(choice.Project)))))).Match(
                Succ: static closed => closed,
                Fail: fault => Op.Side(() => report(fault)));
        dialog.PositiveButtons.Add(button);
        return button;
    }
}

internal sealed class PromptLease<TResult>(Dialog<Option<Fin<TResult>>> dialog, ElementReceipt body, Op key) : UiLease {
    internal Dialog<Option<Fin<TResult>>> Dialog { get; } = dialog;

    internal Unit Cancel() => Released
        ? unit
        : Op.Side(() => Dialog.Close(Some(Fin.Fail<TResult>(new UiFault.Cancelled(Key: key)))));

    internal static Fin<TResult> Settle(Option<Fin<TResult>> verdict, Op op) => verdict.Match(
        Some: static outcome => outcome,
        None: () => Fin.Fail<TResult>(new UiFault.Dismissed(Key: op)));

    protected override Fin<Unit> Free() => ShellReceipt.Severed(Dialog, body, key);
}
```

## [05]-[PRINT_FLOW]

- Owner: `PrintPlan` admits and defers one `PrintDocument` run; `PrintPage` replays a mounted `PaintProgram` under one `ScenePolicy` and host, bounded, or `PageSettings.PrintableArea` framing.
- Entry: `Run` returns `IO<Fin<PrintReceipt>>`; printer interaction and document lifetime begin only when the caller executes the effect.
- Receipt: every attempted page normalizes Eto's selected-range page number to a zero-based source and scope ordinal; `PrintReceipt.Completed` requires one in-range fact per expected page plus host completion and zero failed facts, while `PresenceFailures` preserves taskbar projection faults.
- Seam: `PrintReceipt` is this driver's raw run outcome only — printer-evidence vocabulary is the Exchange publish receipt family, and the composing app root folds these facts into it.
- Growth: a route is one `PrintRoute` case, a frame source is one `PageFrame` case, and a job option is one `PrintSpec` field.
- Boundary: `PrintPage` stores render failure because Eto's page event cannot return it; `Run` never converts that failure into successful completion.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintRoute {
    private PrintRoute() { }
    public sealed record Silent : PrintRoute;
    public sealed record Chooser(Control Parent) : PrintRoute;
    public sealed record Preview(Window Parent) : PrintRoute;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PageFrame {
    private PageFrame() { }
    public sealed record Host : PageFrame;
    public sealed record Printer(PageSettings Settings) : PageFrame;
    public sealed record Bounded(RectangleF Bounds) : PageFrame;

    internal RectangleF Resolve(PrintPageEventArgs args) => Switch(
        state: args,
        host: static (page, _) => new RectangleF(0f, 0f, page.PageSize.Width, page.PageSize.Height),
        printer: static (_, frame) => frame.Settings.PrintableArea,
        bounded: static (_, frame) => frame.Bounds);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintScope {
    private PrintScope() { }
    public sealed record All : PrintScope;
    public sealed record Selected(Range<int> Range) : PrintScope;

    internal Unit Apply(PrintSettings settings) => Switch(
        state: settings,
        all: static (host, _) => Op.Side(() => host.PrintSelection = PrintSelection.AllPages),
        selected: static (host, scope) => Op.Side(() => {
            host.PrintSelection = PrintSelection.SelectedPages;
            host.SelectedPageRange = scope.Range;
        }));

    internal static int Expected(PrintSettings settings, int pageCount) =>
        settings.PrintSelection == PrintSelection.SelectedPages
            ? Math.Clamp(settings.SelectedPageRange.End - settings.SelectedPageRange.Start + 1, 0, pageCount)
            : pageCount;

    internal static Fin<PrintPageSeat> Seat(PrintSettings settings, int currentPage, int pageCount, Op op) {
        bool selected = settings.PrintSelection == PrintSelection.SelectedPages;
        int expected = Expected(settings, pageCount);
        int source = selected ? currentPage - 1 : currentPage;
        int first = selected ? settings.SelectedPageRange.Start - 1 : 0;
        int ordinal = source - first;
        return source >= 0 && source < pageCount && ordinal >= 0 && ordinal < expected
            ? Fin.Succ(new PrintPageSeat(Ordinal: ordinal, Source: source, Expected: expected))
            : Fin.Fail<PrintPageSeat>(new UiFault.Rejected(
                Key: op, Field: nameof(currentPage), Reason: $"page {currentPage} is outside the selected print scope"));
    }

    internal Fin<Unit> Admit(int pageCount, Op op) => Switch(
        state: (PageCount: pageCount, Key: op),
        all: static (_, _) => Fin.Succ(unit),
        selected: static (held, scope) => scope.Range.Start < 1 || scope.Range.End < scope.Range.Start || scope.Range.End > held.PageCount
            ? Fin.Fail<Unit>(new UiFault.Rejected(Key: held.Key, Field: nameof(PrintScope), Reason: "selected pages must be an ordered subset of the job"))
            : Fin.Succ(unit));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PrintSpec(
    int Copies,
    bool Collate,
    bool Reverse,
    PageOrientation Orientation,
    PrintScope Scope) {

    internal Fin<PrintSpec> Admit(int pageCount, Op op) => this switch {
        { Copies: < 1 } => Fin.Fail<PrintSpec>(new UiFault.Rejected(Key: op, Field: nameof(Copies), Reason: "copy count must be positive")),
        { Scope: null } => Fin.Fail<PrintSpec>(new UiFault.Rejected(Key: op, Field: nameof(Scope), Reason: "print scope is absent")),
        _ => Scope.Admit(pageCount, op).Map(_ => this),
    };

    internal PrintSettings Configure(int pageCount) {
        PrintSettings settings = new() {
            Copies = Copies,
            Collate = Collate,
            Reverse = Reverse,
            Orientation = Orientation,
            MaximumPageRange = new Range<int>(1, pageCount),
        };
        _ = Scope.Apply(settings);
        return settings;
    }
}

public readonly record struct PrintPageSeat(int Ordinal, int Source, int Expected);

public sealed record PrintPage(PaintProgram Program, PageFrame Frame, ScenePolicy Policy) {
    internal Fin<PrintPage> Admit(Op op) => (Program, Frame, Policy) switch {
        (null, _, _) => Fin.Fail<PrintPage>(new UiFault.Rejected(Key: op, Field: nameof(Program), Reason: "page paint program is absent")),
        (_, null, _) => Fin.Fail<PrintPage>(new UiFault.Rejected(Key: op, Field: nameof(Frame), Reason: "page frame is absent")),
        (_, _, null) => Fin.Fail<PrintPage>(new UiFault.Rejected(Key: op, Field: nameof(Policy), Reason: "page scene policy is absent")),
        _ => PaintProgram.Admit(Program, op).Map(_ => this),
    };

    internal Fin<RectangleF> Render(PrintPageEventArgs args, Op op) => op.Catch(() => {
        RectangleF frame = Frame.Resolve(args);
        return Policy.Use(args.Graphics, () => {
            args.Graphics.SetClip(frame);
            return Program.Paint(args.Graphics).Map(_ => frame);
        });
    });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintPageFact : IValidityEvidence {
    private PrintPageFact() { }
    public sealed record Rendered(int PageIndex, RectangleF Frame) : PrintPageFact;
    public sealed record Failed(int PageIndex, Error Failure) : PrintPageFact;

    public int Index => Switch(
        rendered: static fact => fact.PageIndex,
        failed: static fact => fact.PageIndex);

    public bool IsValid => this is Rendered;
}

public sealed record PrintReceipt(
    string Name,
    Seq<PrintPageFact> Pages,
    int Expected,
    bool HostCompleted,
    Seq<Error> PresenceFailures) : IValidityEvidence {
    public Seq<PrintPageFact.Failed> Failed => Pages.Choose(static fact => Optional(fact as PrintPageFact.Failed));

    public int Actual => Pages.Count;

    public bool Completed => HostCompleted
        && Actual == Expected
        && Pages.GroupBy(static fact => fact.Index).Count() == Actual
        && Pages.ForAll(fact => fact.Index >= 0 && fact.Index < Expected)
        && Failed.IsEmpty;

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Completed && PresenceFailures.IsEmpty;
}

public sealed record PrintPlan(string Name, Seq<PrintPage> Pages, PrintSpec Spec, PrintRoute Route) {
    public IO<Fin<PrintReceipt>> Run(Op? key = null) => IO.lift(() => RunNow(key.OrDefault()));

    private Fin<PrintReceipt> RunNow(Op op) => Admit(op).Bind(_ => op.Catch(() => {
            Atom<Seq<PrintPageFact>> facts = Atom(Seq<PrintPageFact>());
            Atom<Seq<Error>> presenceFailures = Atom(Seq<Error>());
            Atom<bool> hostCompleted = Atom(false);
            using PrintDocument document = new() {
                Name = Name,
                PageCount = Pages.Count,
                PrintSettings = Spec.Configure(Pages.Count),
            };
            document.Printing += (_, _) => {
                _ = facts.Swap(static _ => Seq<PrintPageFact>());
                _ = presenceFailures.Swap(static _ => Seq<Error>());
                _ = hostCompleted.Swap(static _ => false);
            };
            document.PrintPage += (_, args) => {
                _ = op.Catch(() => {
                    PrintPageFact fact = PrintScope.Seat(document.PrintSettings, args.CurrentPage, Pages.Count, op).Match(
                        Succ: seat => Pages[seat.Source].Render(args, op).Match<PrintPageFact>(
                            Succ: frame => new PrintPageFact.Rendered(seat.Ordinal, frame),
                            Fail: fault => new PrintPageFact.Failed(seat.Ordinal, fault)),
                        Fail: fault => new PrintPageFact.Failed(args.CurrentPage, fault));
                    _ = facts.Swap(held => held.Add(fact));
                    int expected = PrintScope.Expected(document.PrintSettings, Pages.Count);
                    UnitInterval progress = UnitInterval.Create(Math.Clamp(facts.Value.Count / (double)expected, 0d, 1d));
                    Fin<Unit> presence = fact.IsValid
                        ? TaskbarPulse.Apply(new PulseState.Working(progress), op)
                        : TaskbarPulse.Apply(new PulseState.Failed(progress), op);
                    _ = presence.Match(
                        Succ: static applied => applied,
                        Fail: fault => ignore(presenceFailures.Swap(held => held.Add(fault))));
                    return Fin.Succ(unit);
                }).Match(
                    Succ: static rendered => rendered,
                    Fail: fault => ignore(facts.Swap(held => held.Add(new PrintPageFact.Failed(args.CurrentPage, fault)))));
            };
            document.Printed += (_, _) => {
                _ = op.Catch(() => {
                    _ = hostCompleted.Swap(static _ => true);
                    return TaskbarPulse.Apply(new PulseState.Idle(), op);
                }).Match(
                    Succ: static applied => applied,
                    Fail: fault => ignore(presenceFailures.Swap(held => held.Add(fault))));
            };
            return Present(document, op).Map(_ => new PrintReceipt(
                Name, facts.Value, PrintScope.Expected(document.PrintSettings, Pages.Count), hostCompleted.Value, presenceFailures.Value));
        }));

    private Fin<Unit> Admit(Op op) => (Name, Spec, Route) switch {
        (var name, _, _) when string.IsNullOrWhiteSpace(name) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Name), Reason: "print job name requires text")),
        (_, null, _) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Spec), Reason: "print specification is absent")),
        (_, _, null) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Route), Reason: "print route is absent")),
        _ when Pages.IsEmpty =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Pages), Reason: "a print plan requires a page")),
        _ when Pages.Exists(static page => page is null) =>
            Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(Pages), Reason: "print page is absent")),
        _ => Spec.Admit(Pages.Count, op)
            .Bind(_ => Pages.TraverseM(page => page.Admit(op)).As())
            .Map(static _ => unit),
    };

    private Fin<Unit> Present(PrintDocument document, Op op) => Route.Switch(
        state: (Document: document, Key: op),
        silent: static (held, _) => held.Key.Catch(() => {
            held.Document.Print();
            return Fin.Succ(unit);
        }),
        chooser: static (held, route) => held.Key.Catch(() => {
            using PrintDialog dialog = new() { PrintSettings = held.Document.PrintSettings, AllowPageRange = true };
            return Accepted(dialog.ShowDialog(route.Parent, held.Document), held.Key);
        }),
        preview: static (held, route) => held.Key.Catch(() => {
            using PrintPreviewDialog dialog = new(held.Document);
            return Accepted(dialog.ShowDialog(route.Parent), held.Key);
        }));

    private static Fin<Unit> Accepted(DialogResult result, Op op) =>
        result == DialogResult.Ok ? Fin.Succ(unit) : Fin.Fail<Unit>(new UiFault.Dismissed(Key: op));
}
```
