# [RASM_RHINO_ETO_CHROME]

The window, dialog, menu, toolbar, command, and document-output owner of `Rasm.Rhino.Eto`. One `IntentRow` table is the whole interactive-verb vocabulary: a row carries key, captions, icon, gesture, availability, and a `CommandKind` case, and every chrome surface — application menu bar, popup menu, toolbar, tray menu — is a pure projection fold over the same materialized rows, so the census-era parallel menu/toolbar/command construction APIs collapse into one declaration whose `Command`/`CheckCommand`/`RadioCommand` instances the host itself projects into both item families. Windows are `ShellPlan` values realized over `Form`/`FloatingForm`; modal interrogation is `Prompt<TResult>` over `Dialog<TResult>` with `ShowModal`/`ShowModalAsync` railed so dismissal is a typed `UiFault.Dismissed`, never a default-value sentinel; and document output is `PrintPlan` — pages are `canvas.md` scenes rendered by the identical paint fold into the host page `Graphics`, routed silent, chooser-gated, or previewed by one `PrintRoute` dispatch.

## [01]-[INDEX]

- [02]-[INTENT_TABLE]: `IntentKey` + `PlacementSlot` + `CommandKind` + `IntentRow` + `IntentTable` — the one verb table, its materialization into host commands, the availability sweep, the receipt stream, and the menu/popup/toolbar projection folds.
- [03]-[WINDOWS]: `ShellKind` + `ShellPlan` — modeless and floating window construction as one value realized over the window surface, chrome bound from the intent table.
- [04]-[MODAL_RAIL]: `Prompt<TResult>` — typed modal interrogation over `Dialog<TResult>` with affirm/negate rows, default/abort wiring, and the dismissal rail.
- [05]-[PRINT_FLOW]: `PrintSpec` + `PrintRoute` + `PrintPlan` + `PrintReceipt` — the paginated document flow: scene pages, job settings, silent/chooser/preview routing, and typed run evidence.

## [02]-[INTENT_TABLE]

- Owner: `IntentRow` — ONE frozen row per interactive verb — and `IntentTable`, the materialization and projection owner. A row's `CommandKind` selects the host command shape: `Act` mints `Command`, `Toggle` mints `CheckCommand` seeded from its state read and writing through its state write, `Pick` mints `RadioCommand` chained per group through `Controller`. Materialization wires `Executed` through `Op.Catch` into the effect, stamps `MenuText`/`ToolBarText`/`ToolTip`/`Image`/`Shortcut`, and appends an `IntentReceipt` per invocation into one fact stream. Projections fold placements: `MenuOf` builds a `MenuBar`, `PopupOf` a `ContextMenu`, `BarOf` a `ToolBar` — each from `CreateMenuItem()`/`CreateToolItem()` on the SAME command instance, so enablement, caption, icon, and check state stay synchronized across every surface by host construction, never by synchronization code.
- Law: placement is data — `PlacementSlot(Place, Rank, Group, SubMenu)` orders rows per surface, a group transition inserts the separator item, and a `SubMenu` name folds its slice into one `ButtonMenuItem`; the census-era `UiChromeOp`/`UiAction` parallel bar builders are deleted, and a verb reachable from N surfaces is one row with N slots.
- Law: availability is a sweep, not a poll — rows carry `Func<bool>` reads over screen state, `RefreshAvailability()` reapplies `Command.Enabled` and re-seeds every `Toggle` row's `Checked` from its state read in one fold, and callers invoke it on their state edges; per-item enabled or checked writes at call sites are the deleted form.
- Law: gestures ride `Keys` on the row — `Command.Shortcut` is the one gesture sink, and a duplicate gesture inside one table is a materialization-time typed rejection, never runtime shadowing.
- Entry: `IntentTable.Materialize(rows, Op?)` → `Fin<IntentTable>`; `MenuOf(place)`/`PopupOf(place)`/`BarOf(place)`; `Invoke(key)` drives a verb programmatically through the same command path, so scripted invocation shares availability gating and receipts.
- Growth: a new verb is one row; a new surface is one projection fold over the same rows; a new cause needing evidence is one `IntentReceipt` field.

```csharp
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
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value: value) ? new ValidationError(message: "IntentKey requires a non-whitespace identity.") : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandKind {
    private CommandKind() { }
    public sealed record Act(Func<Fin<Unit>> Effect) : CommandKind;
    public sealed record Toggle(Func<bool> Read, Action<bool> Write) : CommandKind;
    public sealed record Pick(string Group, Action Chosen) : CommandKind;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PlacementSlot(string Place, int Rank, int Group = 0, Option<string> SubMenu = default);

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

public readonly record struct IntentReceipt(IntentKey Key, long Ordinal);

public sealed record BoundIntent(IntentRow Row, Command Command);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class IntentTable {
    private readonly Seq<BoundIntent> bound;
    private readonly Atom<Seq<IntentReceipt>> receipts;
    private IntentTable(Seq<BoundIntent> bound, Atom<Seq<IntentReceipt>> receipts) { this.bound = bound; this.receipts = receipts; }

    public Seq<IntentReceipt> Receipts => receipts.Value;

    public static Fin<IntentTable> Materialize(Seq<IntentRow> rows, Op? key = null) {
        Op op = key.OrDefault();
        return Distinct(rows: rows, op: op).Bind(_ => op.Catch(() => {
            System.Collections.Generic.Dictionary<string, RadioCommand> controllers = new(StringComparer.Ordinal);
            Atom<Seq<IntentReceipt>> receipts = Atom(Seq<IntentReceipt>());
            Unit Stamp(IntentKey cause) => ignore(receipts.Swap(held => held.Add(new IntentReceipt(Key: cause, Ordinal: held.Count + 1))));
            return Fin.Succ(value: new IntentTable(
                bound: rows.Map(row => new BoundIntent(Row: row, Command: Mint(row: row, controllers: controllers, op: op, stamp: Stamp))).Strict(),
                receipts: receipts));
        }));
    }

    public Unit RefreshAvailability() => ignore(bound.Iter(static entry => {
        entry.Command.Enabled = entry.Row.Available();
        _ = entry.Row.Kind.Switch(
            state: entry.Command,
            act: static (_, _) => unit,
            toggle: static (host, kind) => host is CheckCommand check ? Op.Side(() => check.Checked = kind.Read()) : unit,
            pick: static (_, _) => unit);
    }));

    public Fin<Unit> Invoke(IntentKey key, Op? op = null) =>
        bound.Find(entry => entry.Row.Key == key)
            .ToFin(Fail: new UiFault.Unavailable(Key: op.OrDefault(), Capability: key.Value))
            .Bind(entry => entry.Row.Available()
                ? op.OrDefault().Catch(() => { entry.Command.Execute(); return Fin.Succ(value: unit); })
                : Fin.Fail<Unit>(error: new UiFault.Unavailable(Key: op.OrDefault(), Capability: key.Value)));

    public MenuBar MenuOf(string place) => Chromed(host: new MenuBar(), place: place);

    public ContextMenu PopupOf(string place) => Chromed(host: new ContextMenu(), place: place);

    public ToolBar BarOf(string place) {
        ToolBar bar = new();
        _ = Placed(place: place).Fold(Option<int>.None, (group, pair) => {
            _ = Op.SideWhen(group.Map(held => held != pair.Slot.Group).IfNone(false), () => bar.Items.Add(new SeparatorToolItem()));
            bar.Items.Add(pair.Entry.Command.CreateToolItem());
            return Some(pair.Slot.Group);
        });
        return bar;
    }

    private THost Chromed<THost>(THost host, string place) where THost : Menu, ISubmenu {
        System.Collections.Generic.Dictionary<string, ButtonMenuItem> branches = new(StringComparer.Ordinal);
        _ = Placed(place: place).Fold(Option<int>.None, (group, pair) => {
            MenuItem item = pair.Entry.Command.CreateMenuItem();
            _ = Op.SideWhen(
                group.Map(held => held != pair.Slot.Group).IfNone(false) && pair.Slot.SubMenu.IsNone,
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

    private Seq<(PlacementSlot Slot, BoundIntent Entry)> Placed(string place) =>
        toSeq(bound.Bind(entry => entry.Row.Slots
                .Filter(slot => string.Equals(a: slot.Place, b: place, comparisonType: StringComparison.Ordinal))
                .Map(slot => (Slot: slot, Entry: entry)))
            .OrderBy(static pair => (pair.Slot.Group, pair.Slot.Rank)));

    private static Command Mint(IntentRow row, System.Collections.Generic.Dictionary<string, RadioCommand> controllers, Op op, Func<IntentKey, Unit> stamp) {
        Command command = row.Kind.Switch(
            act: static _ => new Command(),
            toggle: static kind => (Command)new CheckCommand { Checked = kind.Read() },
            pick: kind => {
                RadioCommand radio = new();
                ref RadioCommand? seat = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(controllers, kind.Group, out bool chained);
                RadioCommand? controller = seat;
                seat ??= radio;
                _ = Op.SideWhen(chained, () => radio.Controller = controller!);
                return (Command)radio;
            });
        command.ID = row.Key.Value;
        command.MenuText = row.MenuText;
        command.ToolBarText = row.ToolText.IfNone(row.MenuText);
        command.ToolTip = row.Hint.IfNone(row.MenuText);
        command.Enabled = row.Available();
        _ = row.Icon.Iter(image => command.Image = image);
        _ = row.Gesture.Iter(gesture => command.Shortcut = gesture);
        command.Executed += (_, _) => ignore(op.Catch(() => {
            _ = stamp(row.Key);
            return row.Kind.Switch(
                state: command,
                act: static (_, kind) => kind.Effect(),
                toggle: static (host, kind) => Fin.Succ(value: Op.Side(() => kind.Write(host is CheckCommand check && check.Checked))),
                pick: static (_, kind) => Fin.Succ(value: Op.Side(kind.Chosen)));
        }));
        return command;
    }

    private static Fin<Unit> Distinct(Seq<IntentRow> rows, Op op) =>
        rows.Choose(static row => row.Gesture).GroupBy(static gesture => gesture).Exists(static group => group.Count() > 1)
            ? Fin.Fail<Unit>(error: new UiFault.Rejected(Key: op, Field: nameof(IntentRow.Gesture), Reason: "duplicate gesture inside one intent table"))
            : Fin.Succ(value: unit);
}
```

## [03]-[WINDOWS]

- Owner: `ShellKind` `[SmartEnum<int>]` — the window-shape rows (`Modeless` → `Form`, `Utility` → `FloatingForm`) — and `ShellPlan`, the one window value: title, client size, sizing capabilities (`Resizable`/`Maximizable`/`Minimizable`/`Closeable`), presence (`Topmost`, `ShowInTaskbar`), the host `WindowStyle` at its seam, optional owner, content `Element`, and optional menu/toolbar placements resolved from an `IntentTable`. `Realize` mints the window, realizes content through `elements.md`, binds `Menu`/`ToolBar` from the table folds, and returns the live `Form`; `Present` shows it.
- Law: window chrome comes from the table — a `ShellPlan` names placement tags, never menu items; a window constructing its own `MenuBar` beside the table forks the verb vocabulary and is the deleted form.
- Law: lifecycle observation rides the verified `Closed`/`Closing` events wired by the consumer over the realized window; host styling (`Rhino` window style, position persistence) is the HostUi unit's shell seam and never enters this plan.
- Growth: a new window capability the host ships (opacity, movable-by-background) is one plan field consumed in `Realize`.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ShellKind {
    public static readonly ShellKind Modeless = new(key: 0, mint: static () => new Form());
    public static readonly ShellKind Utility = new(key: 1, mint: static () => new FloatingForm());
    [UseDelegateFromConstructor]
    internal partial Form Mint();
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ShellPlan(
    ShellKind Kind,
    string Title,
    Element Content,
    Option<Size> ClientSize = default,
    bool Resizable = true,
    bool Maximizable = true,
    bool Minimizable = true,
    bool Closeable = true,
    bool Topmost = false,
    bool ShowInTaskbar = true,
    WindowStyle Style = WindowStyle.Default,
    Option<Window> Owner = default,
    Option<(IntentTable Table, string MenuPlace)> Menu = default,
    Option<(IntentTable Table, string BarPlace)> Bar = default) {

    public Fin<Form> Realize(Op? key = null) {
        Op op = key.OrDefault();
        return Content.Realize(key: op).Bind(body => op.Catch(() => {
            Form window = Kind.Mint();
            window.Title = Title;
            window.Content = body;
            window.Resizable = Resizable;
            window.Maximizable = Maximizable;
            window.Minimizable = Minimizable;
            window.Closeable = Closeable;
            window.Topmost = Topmost;
            window.ShowInTaskbar = ShowInTaskbar;
            window.WindowStyle = Style;
            _ = ClientSize.Iter(size => window.ClientSize = size);
            _ = Owner.Iter(owner => window.Owner = owner);
            _ = Menu.Iter(chrome => window.Menu = chrome.Table.MenuOf(place: chrome.MenuPlace));
            _ = Bar.Iter(chrome => window.ToolBar = chrome.Table.BarOf(place: chrome.BarPlace));
            return Fin.Succ(value: window);
        }));
    }

    public Fin<Form> Present(Op? key = null) =>
        Realize(key: key).Map(window => (Shown: Op.Side(window.Show), window).window);
}
```

## [04]-[MODAL_RAIL]

- Owner: `Prompt<TResult>` — typed modal interrogation over `Dialog<TResult>`: content `Element`, an affirm row (caption plus result projector), an optional negate caption, and optional client size. `Ask` realizes the dialog, wires the affirm button through `Close(result)` with `DefaultButton`, the negate through `Close()` with `AbortButton`, runs `ShowModal(owner)`, and rails the outcome — an affirmative close yields `Fin.Succ(result)`, every other exit (negate, chrome close, escape) yields `UiFault.Dismissed` — so the `default(TResult)` sentinel a bare `ShowModal` returns on dismissal never reaches a consumer. `AskAsync` is the same rail over `ShowModalAsync`.
- Law: the affirm projector runs INSIDE the close wiring — it reads screen state at click time, and its result is both the host `Close(T)` payload and the rail value; computing the result after `ShowModal` returns races teardown and is the deleted form.
- Law: dismissal is one fault, not N — negate, window chrome, and escape all fold to `Dismissed`; a consumer distinguishing dismissal causes has a design smell this rail refuses to carry. The `await`-side `try`/`catch` inside `AskAsync` is the named platform-forced seam — `Op.Catch` is synchronous and the modal await crosses it.
- Boundary: file, folder, color, and font choosers are the HostUi unit's dialog intent rail (Rhino-styled, document-anchored); this owner is the pure-Eto typed modal only, and `MessageBox` convenience presentation likewise lives with the host shell.
- Growth: a multi-button interrogation is `Prompt<TVerdict>` whose affirm rows each project a verdict case — one more `(caption, projector)` row, never a second dialog rail.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Prompt<TResult>(
    string Title,
    Element Content,
    string AffirmCaption,
    Func<Fin<TResult>> Affirm,
    Option<string> NegateCaption = default,
    Option<Size> ClientSize = default,
    Seq<(string Caption, Func<Fin<TResult>> Project)> Extra = default) {

    public Fin<TResult> Ask(Control owner, Op? key = null) {
        Op op = key.OrDefault();
        return Build(op: op).Bind(built => op.Catch(() => {
            _ = built.Dialog.ShowModal(owner: owner);
            return Settle(built: built, op: op);
        }));
    }

    public async Task<Fin<TResult>> AskAsync(Control owner, Op? key = null) {
        Op op = key.OrDefault();
        return await Build(op: op).Match(
            Succ: async built => {
                try { _ = await built.Dialog.ShowModalAsync(owner: owner).ConfigureAwait(true); }
                catch (Exception thrown) { return Fin.Fail<TResult>(error: new UiFault.HostRejected(Key: op, Detail: thrown.Message)); }
                return Settle(built: built, op: op);
            },
            Fail: static fault => Task.FromResult(Fin.Fail<TResult>(error: fault))).ConfigureAwait(true);
    }

    private static Fin<TResult> Settle((Dialog<TResult> Dialog, Atom<Option<Fin<TResult>>> Verdict) built, Op op) =>
        built.Verdict.Value.Match(
            Some: static verdict => verdict,
            None: () => Fin.Fail<TResult>(error: new UiFault.Dismissed(Key: op)));

    private Fin<(Dialog<TResult> Dialog, Atom<Option<Fin<TResult>>> Verdict)> Build(Op op) =>
        Content.Realize(key: op).Bind(body => op.Catch(() => {
            Atom<Option<Fin<TResult>>> verdict = Atom(Option<Fin<TResult>>.None);
            Dialog<TResult> dialog = new() { Title = Title, Content = body };
            _ = ClientSize.Iter(size => dialog.ClientSize = size);
            dialog.DefaultButton = Answered(dialog: dialog, verdict: verdict, caption: AffirmCaption, project: Affirm);
            _ = Extra.Iter(row => ignore(Answered(dialog: dialog, verdict: verdict, caption: row.Caption, project: row.Project)));
            _ = NegateCaption.Iter(caption => {
                Button negate = new() { Text = caption };
                negate.Click += (_, _) => dialog.Close();
                dialog.NegativeButtons.Add(negate);
                dialog.AbortButton = negate;
            });
            return Fin.Succ(value: (Dialog: dialog, Verdict: verdict));
        }));

    private static Button Answered(Dialog<TResult> dialog, Atom<Option<Fin<TResult>>> verdict, string caption, Func<Fin<TResult>> project) {
        Button choice = new() { Text = caption };
        choice.Click += (_, _) => ignore(project().Match(
            Succ: result => Op.Side(() => { _ = verdict.Swap(_ => Some(Fin.Succ(value: result))); dialog.Close(result: result); }),
            Fail: fault => Op.Side(() => ignore(verdict.Swap(_ => Some(Fin.Fail<TResult>(error: fault)))))));
        dialog.PositiveButtons.Add(choice);
        return choice;
    }
}
```

## [05]-[PRINT_FLOW]

- Owner: `PrintSpec` — the job-configuration value projected onto the host `PrintSettings` (copies, collation, reverse, orientation, optional page range) — `PrintRoute`, the closed presentation union (`Silent` runs the configured job, `Chooser(parent)` gates it behind the OS `PrintDialog`, `Preview(parent)` renders it on screen through `PrintPreviewDialog`), `PrintPlan`, the one paginated job (name, `Seq<Scene>` pages, spec, route), and `PrintReceipt`, the typed run evidence. Pages are `canvas.md` scenes: `PrintPage` renders `Pages[CurrentPage]` through the identical `Scene.Render` fold into the page `Graphics`, so screen paint and page paint are one vocabulary and print fidelity is structural; the chooser's `MaximumPageRange` derives from the same page sequence.
- Law: the lifecycle events are the receipt spine — `Printing` resets the run fact, each `PrintPage` appends a page fact and projects fractional progress through the `runtime.md` taskbar owner, `Printed` marks completion and clears the projection — so `PrintReceipt.Completed` is host evidence, never an assumption, and a cancelled chooser folds to `UiFault.Dismissed` with zero pages run.
- Law: `PageCount` derives from the page sequence — a hand-set count beside a page list is the census-class split-brain this plan forecloses by construction; an empty page sequence dies at `Run` as a typed rejection before any host object exists, so `Range<int>(1, 0)` never reaches `MaximumPageRange`.
- Growth: a new job knob the host ships (selection printing, duplex) is one `PrintSpec` field consumed in `Configure`; a new presentation is one `PrintRoute` case breaking `Run` at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintRoute {
    private PrintRoute() { }
    public sealed record Silent : PrintRoute;
    public sealed record Chooser(Control Parent) : PrintRoute;
    public sealed record Preview(Window Parent) : PrintRoute;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PrintSpec(int Copies = 1, bool Collate = true, bool Reverse = false, PageOrientation Orientation = PageOrientation.Portrait, Option<Range<int>> PageRange = default) {
    public static readonly PrintSpec Single = new();
    internal PrintSettings Configure() {
        PrintSettings settings = new() { Copies = Copies, Collate = Collate, Reverse = Reverse, Orientation = Orientation };
        _ = PageRange.Iter(range => settings.SelectedPageRange = range);
        return settings;
    }
}

public sealed record PrintReceipt(string Name, int PagesRun, bool Completed) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Nonnegative(value: PagesRun), ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Name)));
}

public sealed record PrintPlan(string Name, Seq<Scene> Pages, PrintSpec Spec, PrintRoute Route) {
    public Fin<PrintReceipt> Run(Op? key = null) {
        Op op = key.OrDefault();
        Seq<Scene> pages = Pages;
        if (pages.IsEmpty) {
            return Fin.Fail<PrintReceipt>(error: new UiFault.Rejected(Key: op, Field: nameof(Pages), Reason: "a print plan requires at least one page"));
        }
        return op.Catch(() => {
            Atom<(int Rendered, bool Done)> progress = Atom((Rendered: 0, Done: false));
            PrintSettings settings = Spec.Configure();
            settings.MaximumPageRange = new Range<int>(1, pages.Count);
            PrintDocument document = new() { Name = Name, PageCount = pages.Count, PrintSettings = settings };
            document.Printing += (_, _) => ignore(progress.Swap(static _ => (Rendered: 0, Done: false)));
            document.PrintPage += (_, args) => ignore(
                args.CurrentPage >= 0 && args.CurrentPage < pages.Count
                    ? pages[args.CurrentPage].Render(graphics: args.Graphics, key: op).Map(_ => {
                        _ = progress.Swap(static held => (held.Rendered + 1, held.Done));
                        return TaskbarPulse.Show(state: PulseState.Working, fraction: UnitInterval.Create(value: (args.CurrentPage + 1) / (double)pages.Count));
                    })
                    : Fin.Fail<Unit>(error: new UiFault.Rejected(Key: op, Field: nameof(Pages), Reason: $"page {args.CurrentPage} outside the plan")));
            document.Printed += (_, _) => {
                _ = progress.Swap(static held => (held.Rendered, Done: true));
                _ = TaskbarPulse.Clear();
            };
            return Route.Switch(
                state: (Document: document, Key: op),
                silent: static (held, _) => held.Key.Catch(() => { held.Document.Print(); return Fin.Succ(value: unit); }),
                chooser: static (held, route) => Presented(verdict: new PrintDialog { PrintSettings = held.Document.PrintSettings }.ShowDialog(parent: route.Parent, document: held.Document), op: held.Key),
                preview: static (held, route) => Presented(verdict: new PrintPreviewDialog(document: held.Document).ShowDialog(parent: route.Parent), op: held.Key))
                .Map(_ => progress.Value switch { { } held => new PrintReceipt(Name: Name, PagesRun: held.Rendered, Completed: held.Done) });
        });
    }

    private static Fin<Unit> Presented(DialogResult verdict, Op op) =>
        verdict == DialogResult.Ok ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: new UiFault.Dismissed(Key: op));
}
```
