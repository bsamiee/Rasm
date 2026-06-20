# [INTERACTION]

Retained interaction is catalog rows over settled law. One closed surface-row table mounts every process modality — owned window, embedded, headless — through one builder fold; one screen catalog feeds router, dock, deep link, and workspace restore; view state is an observable projection of settled rails opened and closed by view-driven activation; every interactive cause is a case of one trigger union dispatched into one frozen command-intent table whose menus, key tables, palette, receipts, and automation identity are folds over the same rows; live collections cross exactly one binding seam; everything user-facing passes one presentation gate; theme, automation, and locale derive from keys the rows already own. A new screen or command is rows — catalog rows, intent rows, gesture and placement columns — consuming runtime phases, concurrency change-sets, validation outcomes, and visuals tokens with zero new infrastructure.

## [01]-[INTERACTION_CHOOSER]

This table routes an interaction concern to its owning surface; the most specific row wins.

| [INDEX] | [CONCERN]                   | [OWNER]                            | [REJECTED_FORM]                 |
| :-----: | :-------------------------- | :--------------------------------- | :------------------------------ |
|  [01]   | process UI modality         | surface row + one mount fold       | per-host boot fork              |
|  [02]   | screens and navigation      | catalog row + nav-verb union       | per-surface router              |
|  [03]   | dock arrangement            | factory verbs + workspace rails    | view-layer layout mutation      |
|  [04]   | view state and activation   | OAPH + `WhenActivated` scope       | constructor stream wiring       |
|  [05]   | view-model question         | `Interaction<TInput,TOutput>`      | view-model-owned dialog         |
|  [06]   | user-facing validity        | `ValidationRule` state projection  | screen-side re-validation       |
|  [07]   | interactive verbs           | one command-intent row table       | per-surface command registries  |
|  [08]   | verb availability           | seeded `CombineLatest` fold        | `CanExecute(parameter)` logic   |
|  [09]   | live collection to screen   | `SortAndBind` edge + row contract  | snapshot re-query               |
|  [10]   | modal and notice            | presentation gate + dialog session | free-floating modal window      |
|  [11]   | theme application           | one idempotent variant fold        | captured brush, per-window swap |
|  [12]   | automation, locale, gesture | intent-key derivation              | literal names at surfaces       |

## [02]-[SURFACE_MOUNT]

[MOUNT_LAW]:
- Law: every modality is a row of one closed surface table — backend fold fragment, shutdown policy, frame class, interactive gate — and mounting any screen is one fold over (catalog row × surface row); a per-host boot fork, a handle null-check at a call site, or a test-mode flag inside a screen is the rejected form, and a new modality is one row with zero screen edits.
- Law: backend rows are mutually exclusive per process; reactive admission is one `UseReactiveUI` builder row installing `AvaloniaScheduler.Instance` as `RxSchedulers.MainThreadScheduler` plus the activation, template, command, and property binding providers — a second scheduler source duplicates the seam the headless row substitutes by value.
- Law: `MainWindow` assigns at the lifetime `Startup` edge — construction before it races the lifetime, and a primary-window swap is a lifetime write, never window juggling; a tray-resident shell flips `OnExplicitShutdown` or its last closing window kills the process.
- Law: `ShutdownRequested` is the one pre-drain veto — the handler cancels and folds into the settled drain spine whose last band calls `Shutdown(exitCode)` with a code derived from drain outcome; inline teardown in the handler is rejected, and window `Closing` fires too late to coordinate a suite-wide drain.
- Law: `TopLevel` is the per-surface service capsule — `Clipboard`, `StorageProvider`, `FocusManager`, `Screens` resolve through `TopLevel.GetTopLevel(visual)` at attachment edges, absence is a capability value folded per row, and a statically cached capsule service is the defect because every float resolves its own; embedding preserves the law — a foreign view lives in the `CreateNativeControlCore`/`DestroyNativeControlCore` override pair and declares its region as an input-opaque airspace hole on the catalog row, while inbound mounts ride `EmbeddableControlRoot` under the `Prepare`/`StartRendering`/`StopRendering`/`Dispose` protocol against a real capsule.
- Law: headless is a production row — frames advance only on `ForceRenderTimerTick` and the `CaptureRenderedFrame` verb, so an animation or debounce that fails under forced ticks has smuggled wall time; synthetic input drives the real pipeline with provenance in its trigger evidence, and capture against a stub row is a typed mount-time rejection, never an empty bitmap downstream.
- Exemption: the builder fold and the startup-edge body are the platform-forced statement seam.

```csharp conceptual
[SmartEnum<string>]
public sealed partial class SurfaceRow {
    public static readonly SurfaceRow Shell = new("<row-a>", pixel: false, interactive: true,
        shutdown: ShutdownMode.OnMainWindowClose, backend: static builder => builder.UsePlatformDetect());
    public static readonly SurfaceRow Proof = new("<row-b>", pixel: false, interactive: true,
        shutdown: ShutdownMode.OnExplicitShutdown, backend: static builder =>
            builder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true }));
    public static readonly SurfaceRow Export = new("<row-c>", pixel: true, interactive: false,
        shutdown: ShutdownMode.OnExplicitShutdown, backend: static builder => builder.UseHeadless(
            new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, FrameBufferFormat = PixelFormat.Rgba8888 }));

    public bool Pixel { get; }
    public bool Interactive { get; }
    public ShutdownMode Shutdown { get; }

    [UseDelegateFromConstructor]
    public partial AppBuilder Backend(AppBuilder builder);
}

public static class Mount {
    public static int Run(SurfaceRow row, Func<Application> shell, string[] args) {
        ArgumentNullException.ThrowIfNull(row);
        return row.Backend(AppBuilder.Configure(shell)).StartWithClassicDesktopLifetime(args, row.Shutdown);
    }

    public static Fin<WriteableBitmap> Capture(SurfaceRow row, Window surface) {
        ArgumentNullException.ThrowIfNull(row);
        return row.Pixel ? Optional(surface.CaptureRenderedFrame()).ToFin(Error.New(7712, "<no-frame>"))
                         : Fin.Fail<WriteableBitmap>(Error.New(7713, $"<stub-row:{row.Key}>"));
    }
}
```

## [03]-[SCREEN_SPINE]

[CATALOG_LAW]:
- Law: one catalog row per screen — key, dock role, view-model factory — is read by five consumers: router push, dock placement, deep-link grammar, workspace restore, and generated per-screen verb rows in the command table; a consumer needing a field extends the row, never a side table on the same key, so adding a screen touches exactly one declaration.
- Law: `RoutingState` is the entire navigation owner — verbs are commands, so `NavigateBack.CanExecute` already encodes stack depth and a hand-written can-go-back flag is rejected; the verb union closes at push, reset, back, replace (pop-plus-push), and modal (routed to the presentation gate), and every cause — menu, palette, deep link, tray, restore — folds to one verb, never a second router.
- Law: restore is stack manipulation — materialize saved keys, set `NavigationStack`, let `CurrentViewModel` project the top — never command replay; an unresolvable key folds to the default row with a receipt, collapsing first-run, restore, and upgrade into one total fold over (workspace × catalog).
- Law: deep links arrive through `IActivatableLifetime.Activated` with `ActivationKind.OpenUri` — boot argv and lifetime URIs hit one grammar, and intents arriving before restore queue behind it; the empty stack renders `RoutedViewHost.DefaultContent`, view variance is a `ViewContract` row, and the catalog binds the typed `ViewModel` property because a wrong-typed `DataContext` nulls it silently.

[DOCK_LAW]:
- Law: layout state lives in the factory-created model graph and every structural verb is a factory method — the factory is the single mutation surface command rows target, and capability is row data (`CanClose`/`CanPin`/`CanFloat`, `DockGroup`, the settings-root-dockable capability fold), never drag-handler branching; document docks are items-source rows — open documents are keyed collection membership gated by the same close vetoes, and reopen-last-session is workspace restore, never a bespoke MRU.
- Law: persistence is two rails — the serialized layout string and `DockState` content rebound by `Id`, structure first — and dockable `Id` and catalog key are one vocabulary; hide is soft-close preserving the instance for restore, `OnDockableClosing`/`OnWindowClosing` are the only close vetoes, and a named workspace is a capture-and-restore command-row pair, so a layout preset is data with two receipts, never an arranging procedure.
- Law: factory lifecycle hooks fold into one layout-fact stream feeding dirty-edge capture, activation policy, and unsaved-work gating; `DockSettings` statics apply once at the composition root, and workspace capture rides `WorkspaceDirtyChanged` edges on the drain band, never window `Closing` handlers, because the model graph stays coherent mid-gesture while visual state does not — placement capture clamps saved geometry against the live screen set and folds minimized or mid-drag bounds to last settled values, so restore never lands off-screen.

```csharp conceptual
[SmartEnum]
public sealed partial class DockRole {
    public static readonly DockRole Document = new();
    public static readonly DockRole Tool = new();
}

public sealed record ScreenRow(string Key, DockRole Role, Func<IScreen, IRoutableViewModel> Make);

public readonly record struct RestoreFact(string Key, bool Resolved);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Nav {
    private Nav() { }
    public sealed record Push(string Key) : Nav;
    public sealed record Reset(string Key) : Nav;
    public sealed record Back : Nav;
}

public sealed class ShellScreen : ReactiveObject, IScreen { public RoutingState Router { get; } = new(); }

public sealed class PanelModel(IScreen host, string key) : ReactiveObject, IRoutableViewModel {
    public string? UrlPathSegment { get; } = key; public IScreen HostScreen { get; } = host;
}

public static class Spine {
    public static readonly Seq<ScreenRow> Catalog = [
        new("<screen-a>", DockRole.Document, static host => new PanelModel(host, "<screen-a>")),
        new("<screen-b>", DockRole.Tool, static host => new PanelModel(host, "<screen-b>")),
    ];

    public static IRoutableViewModel Resolved(IScreen host, string key) =>
        Catalog.Find(row => row.Key == key).IfNone(Catalog[0]).Make(host);

    public static IDisposable Steer(ShellScreen shell, Nav verb) {
        ArgumentNullException.ThrowIfNull(verb);
        return verb.Switch(
            state: shell,
            push:  static (s, p) => s.Router.Navigate.Execute(Resolved(s, p.Key)).Subscribe(),
            reset: static (s, r) => s.Router.NavigateAndReset.Execute(Resolved(s, r.Key)).Subscribe(),
            back:  static (s, _) => s.Router.NavigateBack.Execute().Subscribe());
    }

    public static Seq<RestoreFact> Restore(ShellScreen shell, Seq<string> saved) {
        ArgumentNullException.ThrowIfNull(shell);
        return (fun(shell.Router.NavigationStack.Clear)(),
            (from key in saved
             let hit = Catalog.Find(row => row.Key == key)
             let mount = fun(() => shell.Router.NavigationStack.Add(hit.IfNone(Catalog[0]).Make(shell)))()
             select new RestoreFact(key, hit.IsSome)).Strict()).Item2;
    }
}
```

## [04]-[VIEW_STATE]

[REACTIVE_LAW]:
- Law: derived state is a fold over property streams — `WhenAnyValue` into `ToProperty` — with the knob triad load-bearing: `initialValue` deletes the `default(T)` flash, `deferSubscription: true` makes expensive derivation pay-on-read, and the scheduler stays the installed main-thread value; property streams replay their current value at subscription, while a raw event projection does not and seeds at admission — the emit-on-subscribe contract every availability input downstream assumes.
- Law: deep paths ride expression-chain subscription — an `a.B.C` chain re-resolves when intermediates are replaced where a hand-wired `PropertyChanged` chain goes silently stale — and cross-object joins are `CombineLatest` over property streams, never recomputation handlers.
- Law: the constructor owns long-lived derivations; everything touching external streams, timers, or services opens inside `WhenActivated` or it leaks one subscription per dock-tab switch — activation is view-driven and composes hierarchically, so explicit child-activation forwarding double-activates.
- Law: `ThrownExceptions` is the one fault rail — command and pipeline faults join one root edge delivered on the output scheduler, and recoverable outcomes are values in `TResult`, never exceptions; once observed, the observer owns the failures, so the root subscription is mandatory, not hardening.
- Law: view-model-to-view questions ride `Interaction<TInput,TOutput>` — handlers walk in reverse registration order, the first `SetOutput` wins, an unhandled walk throws typed, and registration disposal restores the prior handler, so a headless row overrides the dialog-presenting handler without touching the asker; a small standard question vocabulary (confirm, choose, supply) beats per-screen interaction types.

[SCREEN_VALIDITY]:
- Law: settled boundary law produces the typed outcome; the screen projects it once through the state-observable `ValidationRule` — validity and message as one `ValidationState` value, re-running validation logic in a view-model is the rejected form — and `IsValid()` is a canonical availability input: submit rows gate through the availability fold, never code-behind disabling, with a pending async probe projecting invalid-with-message so the gate stays conservative without a tri-state flag.
- Law: property-scoped rules feed field adorners through the error-info bridge (`GetErrors`/`ErrorsChanged`) while context-level validity feeds the gate — one context, two read altitudes; rule membership is data with a lifecycle, so mode shifts retire and re-register `ValidationHelper` handles, and the context disposes with the screen.

```csharp conceptual
public sealed class EditScreen : ReactiveValidationObject, IActivatableViewModel {
    public static readonly Atom<Seq<Error>> Faults = Atom(Seq<Error>());

    readonly ObservableAsPropertyHelper<int> score;
    string raw = "";

    public EditScreen(Func<string, Validation<Error, int>> admit, IObservable<int> live) {
        var outcome = this.WhenAnyValue(static screen => screen.Raw).Select(value => admit(value));
        score = outcome.Select(static fold => fold.Match(Succ: identity, Fail: static _ => 0))
            .ToProperty(this, static screen => screen.Score, initialValue: 0, deferSubscription: true);

        this.ValidationRule(static screen => screen.Raw, outcome.Select(static fold =>
            new ValidationState(fold.IsSuccess, fold.Match(Succ: static _ => "", Fail: static error => error.Message))));
        this.WhenActivated(anchors => {
            live.Select(static value => value.ToString(CultureInfo.InvariantCulture)).Subscribe(value => Raw = value).DisposeWith(anchors);
            ThrownExceptions.Subscribe(static fault => ignore(Faults.Swap(held => held.Add(Error.New(fault))))).DisposeWith(anchors);
        });
    }

    public ViewModelActivator Activator { get; } = new();
    public Interaction<string, bool> Confirm { get; } = new();
    public IObservable<bool> SubmitGate => this.IsValid();
    public int Score => score.Value;
    public string Raw { get => raw; set => this.RaiseAndSetIfChanged(ref raw, value); }

    public IDisposable AutoAnswer() => Confirm.RegisterHandler(static context => context.SetOutput(true));

    protected override void Dispose(bool disposing) {
        if (disposing) { score.Dispose(); }
        base.Dispose(disposing);
    }
}
```

## [05]-[COMMAND_TABLE]

[ROW_LAW]:
- Law: one frozen row table owns the verb vocabulary — intent key, effect, availability inputs, gesture, placements — and every surface is a pure projection: menus fold by placement, key tables by gesture, the palette by localized header, tray and dock bars by tag; per-surface command registries are deleted, a new command is one row that updates N surfaces by construction, and the OS menu bar and tray render one `NativeMenu` graph — two placements of one model — where a `Click` handler beside a `Command` double-fires.
- Law: the intent key is triple-duty — localization key, icon key, automation identity — so a literal header or icon reference at a surface is a bypassed row field.
- Law: stateful verbs are toggle rows — `ToggleType` with checked state bound to its state stream, radio groups whose exactly-one-checked invariant lives in the state owner — and hiding versus disabling an unavailable verb is a per-placement policy bit, never two rows.
- Law: every cause is a case of one trigger `[Union]` carrying typed evidence — pointer, chord, menu, palette, remote, automation — dispatched totally into intent rows; a scripted or remote verb is one more arm, which is what grants it availability gating, busy exclusion, and receipts, per-target verb variants are evidence values, never sibling rows, and markup input adaptation is behavior rows — a trigger paired with `InvokeCommandAction` — never code-behind handlers.
- Law: factory choice derives from the effect's carrier — `Create` runs inline, `CreateFromTask` threads the token, `CreateRunInBackground` moves compute off the UI thread, `CreateCombined` absorbs batch verbs as the all-true fold over child `CanExecute` — and results are the receipt rail: the command is `IObservable<TResult>`, the spine subscribes once keyed by intent, receipts embed trigger provenance, and palette ranking, undo journaling, and usage attribution are folds over receipts; commands are disposable owners disposed with the table at process exit, because per-screen disposal orphans surfaces still bound.

[AVAILABILITY_AND_INPUT]:
- Law: the built pipeline is seeded-total — supplied gate, catch-to-false into `ThrownExceptions`, false seed, busy exclusion via `IsExecuting`, distinct, replay-1 — so a non-seeded input disables its row forever and a throwing input latches it disabled; adding `!IsExecuting` to a gate double-counts and deadlocks re-enablement, `IsExecuting` is also the canonical busy-indicator input, and `InvokeCommand` is the one stream-to-command pipe — emissions while unavailable drop, never queue — so a hand-subscribed execute bypasses the gate.
- Law: the `ICommand` bridge returns the cached value and ignores the parameter — parameter-dependent availability is structurally inexpressible at the control seam and enters as a typed input; the bridge maps null to `default(TParam)` silently and throws on a wrong-typed parameter at invoke time, not bind time, `CanExecuteChanged` raises only on distinct transitions, and OS menu exports subscribe it weakly, so rows never leak through native menus.
- Law: key tables derive from the gesture column — `KeyGesture.Parse` admits the invariant form, the platform format renders display text, and the two never cross — with the conflict fold over (scope × gesture) run at composition, rejecting duplicates as typed receipts instead of runtime shadowing; rate shaping (throttle, debounce) lives on trigger rows, never in execute bodies, and cancel verbs are sibling rows gated on the target's `IsExecuting` with cancellation flowing through the carrier.

```csharp conceptual
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Trigger {
    private Trigger() { }
    public sealed record Pointer(int X, int Y) : Trigger;
    public sealed record Chord(string Gesture) : Trigger;
    public sealed record Remote(string Caller) : Trigger;
}

public readonly record struct Receipt(string Intent, string Cause);

public sealed record IntentRow(
    string Key, Option<string> Gesture, Seq<string> Placements,
    Func<Trigger, CancellationToken, Task<Receipt>> Effect, Seq<IObservable<bool>> Inputs);

public sealed record BoundRow(IntentRow Row, ReactiveCommand<Trigger, Receipt> Command);

public static class CommandTable {
    public static readonly Atom<Seq<Receipt>> Receipts = Atom(Seq<Receipt>());

    public static Seq<IntentRow> Rows(IObservable<bool> valid, IObservable<bool> retained) => [
        new("<intent-a>", Some("Ctrl+R"), ["<menu-a>", "<bar-a>"], Evidenced("<intent-a>"), [valid, retained]),
        new("<intent-b>", Some("Ctrl+T"), ["<menu-a>"], Evidenced("<intent-b>"), [retained]),
    ];

    public static Seq<BoundRow> Materialize(Seq<IntentRow> rows, IScheduler ui) =>
        rows.Map(row => new BoundRow(row, ReactiveCommand.CreateFromTask(
            row.Effect,
            canExecute: Observable.CombineLatest(row.Inputs).Select(static gates => gates.All(static gate => gate)),
            outputScheduler: ui))).Strict();

    public static IDisposable Spine(Seq<BoundRow> table) =>
        table.Map(static bound => (IObservable<Receipt>)bound.Command).Merge()
            .Subscribe(static receipt => ignore(Receipts.Swap(held => held.Add(receipt))));

    public static Seq<BoundRow> Placed(Seq<BoundRow> table, string placement) =>
        table.Filter(bound => bound.Row.Placements.Exists(tag => tag == placement));

    public static Validation<Error, Seq<KeyBinding>> KeyTable(Seq<BoundRow> table, string scope) =>
        table.Choose(static bound => bound.Row.Gesture.Map(chord => (Chord: chord, bound.Command))) is var claimed
        && toSeq(claimed.GroupBy(static claim => claim.Chord).Where(static set => set.Count() > 1)) is { IsEmpty: true }
            ? claimed.Map(static claim => new KeyBinding { Gesture = KeyGesture.Parse(claim.Chord), Command = claim.Command }).Strict()
            : Error.New(7721, $"<gesture-conflict:{scope}:{string.Join(',', claimed.Map(static claim => claim.Chord))}>");

    static Func<Trigger, CancellationToken, Task<Receipt>> Evidenced(string intent) =>
        (cause, _) => Task.FromResult(new Receipt(intent, cause.Switch(
            pointer: static p => $"<pointer:{p.X}>",
            chord:   static c => $"<chord:{c.Gesture}>",
            remote:  static r => $"<remote:{r.Caller}>")));
}
```

## [06]-[BINDING_EDGE]

[EDGE_LAW]:
- Law: change-set mechanics arrive settled; this seam owns the last hop — `SortAndBind` with `SortAndBindOptions` into one bound collection: replace-not-remove preserves row identity and selection through in-place edits, the reset threshold collapses bulk storms into one notification, `UseBinarySearch` is legal only on a sort key immutable for the row's lifetime, `InitialCapacity` pre-sizes known cardinality, and rows owning resources take `DisposeMany` immediately upstream of the bind so removal disposes the row model instead of leaking subscriptions at churn rate.
- Law: exactly one `ObserveOn` onto the installed main-thread scheduler sits at the binding edge — earlier serializes background work, later does not exist — and the inline fast path makes the hop unconditional rather than thread-check-guarded.
- Law: the projection union is closed — flat, tree, grouped, paged, windowed — and every member collapses onto one depth-bearing row contract into the identical bind seam, buying one selection model, one virtualization path, and one key table; a dedicated tree control is rejected, tree-ness is row fields, expansion is a predicate stream over a persistable key set, lazy children are cache membership rather than a load-children verb, and mode switches swap the operator chain while the bound collection, selection, and templates stay invariant under one writer.

[GRID_AND_EDITORS]:
- Law: exactly one shaping owner per grid — the change-set pipeline for live sources or `DataGridCollectionView` for snapshots; stacking both double-shapes and double-notifies, `DeferRefresh()` coalesces descriptor edits into one rebuild, and commit routes the edited row back through the settled admission rail — the grid surfaces validity and never owns it, and `LoadingRow` decoration is idempotent because containers recycle.
- Law: editors are registry rows — priority-ordered `ICellEditFactory` accept-match for structured values, grammar rows by scope for text (`InstallTextMate`, `SetGrammar`), palette contracts for color — and scrub-versus-commit is the uniform dual channel: preview drives cheap visuals, commit drives the receipted mutation, and binding the mutation to the preview channel floods admission at pointer-move rate; durable text positions are anchors, never offsets, and editor theme follows the application variant through `SetTheme` on the variant edge.
- Law: one viewport owner per canvas — `ZoomBorder` owns the pan-zoom transform exclusively, input policy and constraint clamps are declared rows, and camera state is an observed stream like grid selection, so typed viewport state rides the workspace rails, never scraped transform fields; reveal-on-selection is one intent row across grids, trees, and canvases — `ScrollIntoView` or `ZoomToRectangle` per surface kind — gated on selection non-emptiness.

```csharp conceptual
public sealed record Entry(string Key, string Parent, string Path, int Rank);

public sealed class RowModel(int depth, Entry entry) : IDisposable {
    public int Depth { get; } = depth;
    public Entry Entry { get; } = entry;
    public CompositeDisposable Anchors { get; } = [];
    public void Dispose() => Anchors.Dispose();
}

[SmartEnum]
public sealed partial class ViewAs {
    public static readonly ViewAs List = new();
    public static readonly ViewAs Tree = new();
}

public static class BindingEdge {
    public static IObservable<IChangeSet<RowModel, string>> Projected(
        IObservable<IChangeSet<Entry, string>> source, ViewAs mode, IObservable<Func<Node<Entry, string>, bool>> expanded) {
        ArgumentNullException.ThrowIfNull(mode);
        return mode.Switch(
            state: (Source: source, Expanded: expanded),
            list: static held => held.Source.Transform(static entry => new RowModel(0, entry)),
            tree: static held => held.Source
                .TransformToTree(static entry => entry.Parent, held.Expanded)
                .Transform(static node => new RowModel(node.Depth, node.Item)));
    }

    public static (ReadOnlyObservableCollection<RowModel> View, IDisposable Live) Bound(
        IObservable<IChangeSet<RowModel, string>> rows, IScheduler ui) {
        ArgumentNullException.ThrowIfNull(rows);
        var live = rows.DisposeMany()
            .ObserveOn(ui)
            .SortAndBind(out var view,
                SortExpressionComparer<RowModel>.Ascending(static row => row.Entry.Path),
                new SortAndBindOptions { UseBinarySearch = true, ResetThreshold = 80, InitialCapacity = 256 })
            .Subscribe();
        return (view, live);
    }
}
```

## [07]-[PRESENTATION_GATE]

[GATE_LAW]:
- Law: dialog-versus-notice is a severity split under one law — blocking decisions are awaited sessions with typed receipts, ambient facts are non-blocking notices — and both pass one suppression fold whose posture column rides moment rows derived once from the settled phase and rank cells; a call site never inspects phase, nothing user-facing escapes the gate, and resume edges flush held items in arrival order with stale entries aged out by a declared horizon.
- Law: modal state is host-addressable — `DialogHost` by `Identifier`, the awaited task is the receipt, `DialogHost.Close(identifier, parameter)` supplies the result — so free-floating modal windows are rejected; markup-opened dialogs land in the same session stack, `CloseOnClickAway` is host policy rather than a per-dialog argument, and the session is one handler of an `Interaction`, because a view-model calling the dialog rail directly couples to presentation and forecloses the headless auto-answer.
- Law: sessions are first-class — `UpdateContent` morphs a live session, so a wizard is a fold over step states inside one session with the final close parameter as the whole flow's receipt, and the closing veto guards mid-flow abandonment.

```csharp conceptual
[SmartEnum]
public sealed partial class Posture {
    public static readonly Posture Present = new();
    public static readonly Posture Hold = new();
    public static readonly Posture Drop = new();
}

[SmartEnum<string>]
public sealed partial class Moment {
    public static readonly Moment Live = new("<moment-a>", Posture.Present);
    public static readonly Moment Capture = new("<moment-b>", Posture.Hold);
    public Posture Posture { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Surfaceable {
    private Surfaceable() { }
    public sealed record Decision(string Host, object Prompt, Option<object> Morph, Func<object?, bool> Veto) : Surfaceable;
    public sealed record Notice(string Text) : Surfaceable;
}

public static class PresentationGate {
    static readonly Atom<Seq<(Surfaceable Item, DateTimeOffset At)>> Queued = Atom(Seq<(Surfaceable, DateTimeOffset)>());

    public static Task<Fin<object>> Admit(Surfaceable item, Moment moment, TimeProvider clock) {
        ArgumentNullException.ThrowIfNull(moment);
        return moment.Posture.Switch(
            state: (Item: item, Clock: clock),
            present: static held => held.Item.Switch(
                decision: static d => Decided(d),
                notice:   static n => Task.FromResult(Fin.Succ<object>(n.Text))),
            hold: static held => (Queued.Swap(rows => rows.Add((held.Item, held.Clock.GetUtcNow()))),
                Task.FromResult(Fin.Fail<object>(Error.New(7731, "<held>")))).Item2,
            drop: static held => Task.FromResult(Fin.Fail<object>(Error.New(7732, "<dropped>"))));
    }

    public static Seq<Surfaceable> Resume(TimeProvider clock, TimeSpan horizon) {
        Seq<(Surfaceable Item, DateTimeOffset At)> taken = default;
        return (Queued.Swap(rows => (taken = rows, Seq<(Surfaceable, DateTimeOffset)>()).Item2),
                taken.Filter(row => clock.GetUtcNow() - row.At <= horizon).Map(static row => row.Item)).Item2;
    }

    static async Task<Fin<object>> Decided(Surfaceable.Decision decision) =>
        Optional(await DialogHost.Show(decision.Prompt, decision.Host,
                (object _, DialogOpenedEventArgs opened) => decision.Morph.Iter(next => opened.Session.UpdateContent(next)),
                (object _, DialogClosingEventArgs closing) => { if (decision.Veto(closing.Parameter)) { closing.Cancel(); } })
            .ConfigureAwait(true)).ToFin(Error.New(7733, "<dismissed>"));
}
```

## [08]-[SURFACE_DERIVATION]

[VARIANT_APPLICATION]:
- Law: one application-level fold maps (platform color values × pinned override × density × contrast preference) onto `RequestedThemeVariant`, palette rows, and `DensityStyle` atomically — idempotent on `PlatformColorValues` record equality, so OS signal bursts need no debounce — and the override short-circuits the fold rather than unsubscribing the platform edge (`GetColorValues` plus `ColorValuesChanged`, one application-level subscription).
- Law: variant binding is the three-level `RequestedThemeVariant` chain — `Application`, `TopLevel`, `ThemeVariantScope` — where null means inherit; floating dock windows and popups inherit from the application, not the window they detached from, so suite-wide variant rides the application fold or floats diverge silently, and per-region theming is a scope row.
- Law: a custom variant declares `InheritVariant` or resolves nothing outside its own dictionary; brand theming is `Palettes` dictionary entries plus the variant declaration, density is one `DensityStyle` swap, and the high-contrast preference selects a pre-verified variant row through the same fold, so contrast mode is zero code; code-side token reads go through theme-aware lookup at `ActualThemeVariant` — a captured brush is the staleness defect — and `ActualThemeVariantChanged` at the application is the one subscription edge.

[DERIVED_SURFACES]:
- Law: automation derives from the vocabulary — `AutomationProperties.Name` from the localization key, `AcceleratorKey` from the claimed gesture, `AutomationId` from the intent or screen key — so automation invocation is a trigger arm, never a parallel naming scheme; owner-drawn surfaces override `OnCreateAutomationPeer` or stay invisible, virtualized rows declare `PositionInSet`/`SizeOfSet` from upstream totals, announcements ride `LiveSetting` instead of focus theft, and focus topology is a `TabNavigation` container-mode row.
- Law: locale rides the same keys through atomic resource swap, and the locale row carries its direction — applying a locale folds (dictionary swap × `FlowDirection` × gesture display refresh) in one place; owner-drawn content reads its own `FlowDirection` or ships un-mirrored in RTL locales with no diagnostic, and mnemonic markers ride the access-text primitive beside the `AccessKey` row, so accelerators localize with the header.
- Law: the headless walk is the derivation audit — automation names, gestures, tab cycles, and locale-key totality diff against the table as typed receipts — and anything provable headless holds windowed because the mount law is shared; a windowed-versus-headless divergence is a row-capability fact, repaired as a row edit, never a screen patch.

```csharp conceptual
public sealed record Edge(PlatformColorValues Values, Option<ThemeVariant> Pinned, bool Compact);

public static class VariantFold {
    public static readonly ThemeVariant Brand = new("<variant-a>", ThemeVariant.Dark);
    public static readonly ThemeVariant Sharp = new("<variant-b>", ThemeVariant.Dark);

    static readonly Atom<Option<Edge>> Applied = Atom(Option<Edge>.None);

    public static Unit OnEdge(Application app, FluentTheme theme, Edge candidate) {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(candidate);
        return Applied.Value is { IsSome: true, Case: Edge held } && held == candidate
            ? unit
            : (Applied.Swap(_ => Some(candidate)), Apply(app, theme, candidate)).Item2;
    }

    static Unit Apply(Application app, FluentTheme theme, Edge edge) {
        app.RequestedThemeVariant = edge.Pinned.IfNone(
            edge.Values.ContrastPreference is ColorContrastPreference.High ? Sharp : (ThemeVariant)edge.Values.ThemeVariant);
        theme.DensityStyle = edge.Compact ? DensityStyle.Compact : DensityStyle.Normal;
        theme.Palettes[Brand] = new ColorPaletteResources { Accent = edge.Values.AccentColor1 };
        return unit;
    }
}
```
