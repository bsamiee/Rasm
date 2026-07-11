# [RASM_RHINO_HOSTUI_SHELL]

The host-runtime operation surface of `Rasm.Rhino.HostUi` — the one owner of Rhino's application-level UI machinery: command-thread affinity and marshalling over `RhinoApp`, the command-prompt and status-bar write spine as one combinable delta value, prompt observation over the Rhino 9 `CommandPromptChanged` edge, viewport toasts as receipted notes, the document-scoped status-bar progress meter with created/refused/foreign ownership evidence, and Rhino window ownership — `RhinoEtoApp` document windows, `EtoExtensions` native styling, position persistence, semi-modal presentation, and typed window discovery. The census scattered these across per-effect static helpers (`RhinoUi.Protect`/`DispatchThread`/`Invoke`/`Enqueue`/`Deliver`, a status record, a toast record, a progress class, ad-hoc window calls); this page collapses them into five owners with polymorphic entries: one marshal boundary, one status monoid applied in one fold, one leased meter, one window-adoption fold, one theme edge. Every fallible body crosses `Op.Catch`; every failure is a `UiFault` case; document scope enters as a `DocumentSession` capability demand, never an ambient document read. The Eto-side dispatch (`UiThread` over `Application.Instance`) is the Eto sub-domain's; this owner is the Rhino command-thread shape, and the two never substitute for each other — Eto realization marshals through `UiThread`, host document/status/window work marshals through `HostThread`.

## [01]-[INDEX]

- [02]-[HOST_THREAD]: `HostThread` — Rhino main-thread affinity, the blocking railed marshal, the fire-and-forget post, the affinity guard over `RhinoApp.IsOnMainThread`/`InvokeAndWait`/`InvokeOnUiThread`, and the session-crossing marshal every session-gated HostUi entry rides.
- [03]-[STATUS_SPINE]: `StatusDelta` + `StatusReceipt` + `ToastNote`/`ToastHandle` + `PromptFact`/`PromptWatch` — the one combinable screen-state delta over command prompt and status panes, receipted viewport toasts, and the typed prompt observation stream.
- [04]-[PROGRESS_METER]: `MeterGrant` + `ProgressSpec`/`ProgressStep` + `ProgressLease` + `Progress` — the document-scoped status-bar meter as a leased resource with created/refused/foreign ownership evidence and an optional taskbar mirror.
- [05]-[WINDOW_OWNERSHIP]: `WindowDress` + `ShellWindows` + `ShellTheme` — document window resolution, native styling and placement persistence as one adoption fold, semi-modal presentation, typed window discovery, and the Rhino light/dark edge into the Eto theme seam.

## [02]-[HOST_THREAD]

- Owner: `HostThread` — the ONE boundary between any producer thread and Rhino's command thread. Three shapes discriminate on the caller's need: `On<T>` runs inline when already affine and otherwise blocks through `RhinoApp.InvokeAndWait` capturing the railed result, `Post` fires through `RhinoApp.InvokeOnUiThread` and forgets, `Guard` asserts affinity as a typed `UiFault.OffThread` for entries whose contract requires an existing frame. Every marshalled body crosses `Op.Catch` on the far side, so a throwing host body is a typed fault on the caller's rail, never an unhandled marshal exception. The census `Protect`/`DispatchThread`/`Invoke`/`Enqueue`/`Deliver` quintet — five spellings of this one boundary — is dead.
- Law: `On` from the command thread executes inline with zero marshal cost, so callers never pre-test `IsOnMainThread`; a caller-side affinity branch beside `On` re-derives what the entry already owns.
- Law: an `InvokeAndWait` that returns without executing its delegate surfaces as `UiFault.Unavailable` naming the member — a null-result marshal is host evidence, never a silent default.
- Law: this owner marshals to the Rhino command thread only; Eto control-tree work rides the Eto sub-domain's `UiThread`, and a body needing both surfaces enters through `HostThread` and composes `UiThread` inside, because Rhino's UI thread hosts the Eto loop and the outer marshal makes the inner one an inline no-op.
- Law: `OnSession` is the one seam between the document capability rail and a UI-handle result — `DocumentSession.Demand` already marshals live work onto the command thread and constrains its result to `IDetachedDocumentResult`, so the demand's own result is the session `DocKey` while the typed UI value (a window, an answer, a lease) rides the closure capture; every session-gated HostUi entry crosses here, and a direct `session.Demand` beside it re-derives the seam.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`, `Op.Catch`, `Op.Side`), Document sub-domain (`DocumentSession`, `SessionNeed`, `DocKey`), Eto sub-domain (`UiFault`), RhinoCommon (`RhinoApp.IsOnMainThread`, `RhinoApp.InvokeRequired`, `RhinoApp.InvokeAndWait`, `RhinoApp.InvokeOnUiThread`).
- Growth (HOST): a new marshal shape the host ships lands as one member on this owner; a per-page dispatch helper is one member too many.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Runtime;
using Rhino.UI;
using DrawingPointF = System.Drawing.PointF;

namespace Rasm.Rhino.HostUi;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostThread {
    public static bool Affine => RhinoApp.IsOnMainThread;

    public static bool MarshalRequired => RhinoApp.InvokeRequired;

    public static Fin<T> On<T>(Func<Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoApp.IsOnMainThread ? op.Catch(body) : Marshalled(body: body, op: op);
    }

    public static Unit Post(Action body, Op? key = null) {
        Op op = key.OrDefault();
        return Op.Side(() => RhinoApp.InvokeOnUiThread(method: () => ignore(op.Catch(() => Fin.Succ(value: Op.Side(body)))), args: []));
    }

    public static Fin<Unit> Guard(Op? key = null) =>
        RhinoApp.IsOnMainThread ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: new UiFault.OffThread(Key: key.OrDefault()));

    internal static Fin<T> OnSession<T>(DocumentSession session, Func<RhinoDoc, Fin<T>> body, Op op, params ReadOnlySpan<SessionNeed> needs) {
        Fin<T>? captured = null;
        return session
            .Demand(
                use: document => {
                    captured = op.Catch(() => body(document));
                    return captured.Value.Map(_ => session.Key);
                },
                key: op,
                needs: needs)
            .Bind(_ => captured is { } settled
                ? settled
                : Fin.Fail<T>(error: new UiFault.Unavailable(Key: op, Capability: nameof(DocumentSession.Demand))));
    }

    private static Fin<T> Marshalled<T>(Func<Fin<T>> body, Op op) =>
        op.Catch(() => {
            Fin<T>? captured = null;
            RhinoApp.InvokeAndWait(action: () => captured = op.Catch(body));
            return captured is { } settled
                ? settled
                : Fin.Fail<T>(error: new UiFault.Unavailable(Key: op, Capability: nameof(RhinoApp.InvokeAndWait)));
        });
}
```

## [03]-[STATUS_SPINE]

- Owner: `StatusDelta` — the one combinable screen-state value over every app-level status write: command prompt with optional default, transient command message, message/distance/number/point panes, pane clearing, and a `Seq<ToastNote>` of viewport notices. `Apply` is the single write site — one marshalled fold lands the whole delta and returns a `StatusReceipt` carrying the toast handles, so a batch of status intent is one host crossing. `+` merges deltas newest-wins per slot through the `Option` choice algebra, `Collapse` folds any arity, and `Scripted` projects a command-history-safe message row. The census split — a status record applying itself, a toast record applying itself, prompt writes scattered — is one owner now.
- Law: absence composes — an unset slot in a later delta preserves the earlier delta's value under `+`, so incremental status producers merge without coordination and the applied delta is always the fold of every contributor. The aggregation is the LanguageExt `Option` choice fold; no local option algebra exists here. A `PromptDefault` without a `Prompt` is inert in the applied fold, because the host writes the default only through the two-argument prompt call.
- Law: toasts are best-effort rows — each note applies independently and a refused note never cross-cancels its siblings; the receipt carries every minted `ToastHandle` because `RhinoView.ShowToast` returns the toast id and the handle is the only future dismiss target the host admits.
- Law: progress-meter state never rides this delta — the meter is `[04]`'s leased resource, and a `HideProgress` flag beside a lease is the split-brain the census carried and this page deletes.
- Owner: `PromptWatch` — the typed observation of the Rhino 9 `RhinoApp.CommandPromptChanged` edge: `Observe` subscribes once, stamps a `PromptFact` per transition — the event's `Prompt`, its `PromptDefault` as typed absence, the live `CommandLineOption` roster detached into `(Index, English, Local)` rows because the host option object is a native-pointer handle that never outlives the callback, and a monotonic ordinal — and returns a `ShellSubscription` whose `Halt` detaches, so a command-line-reactive surface (palette, HUD, prompt mirror) composes a fact stream, never a raw host event.
- Packages: LanguageExt.Core (`Option`, `Seq`, `TraverseM`), Rasm.Domain (`Op`), Eto sub-domain (`UiFault`), RhinoCommon (`RhinoApp.SetCommandPrompt` both arities, `RhinoApp.SetCommandPromptMessage`, `RhinoApp.CommandPromptChanged`, `CommandPromptChangedEventArgs.Prompt`/`PromptDefault`/`Options`, `CommandLineOption.Index`/`EnglishName`/`LocalName`, `StatusBar.ClearMessagePane`/`SetMessagePane`/`SetDistancePane`/`SetNumberPane`/`SetPointPane`, `RhinoView.ShowToast` all three arities).
- Growth (DOMAIN): a new status axis the concept demands (a pane the host adds, a toast dismiss verb once the host ships one) is one `StatusDelta` slot consumed inside `Apply` or one `ToastHandle` member — every producer gains it through the merge with zero call-site edits.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ToastNote(RhinoView View, string Message, Option<int> TextHeight = default, Option<DrawingPointF> Location = default);

public readonly record struct ToastHandle(uint Id);

public readonly record struct PromptFact(string Prompt, Option<string> Default, Seq<(int Index, string English, string Local)> Options, long Ordinal);

public sealed record ShellSubscription(Func<Unit> Halt);

public readonly record struct StatusReceipt(Seq<ToastHandle> Toasts);

public readonly record struct StatusDelta(
    Option<string> Prompt = default,
    Option<string> PromptDefault = default,
    Option<string> PromptMessage = default,
    Option<string> Pane = default,
    Option<double> Distance = default,
    Option<double> Number = default,
    Option<Point3d> Point = default,
    Seq<ToastNote> Toasts = default,
    bool ClearPane = false) {

    public static StatusDelta operator +(StatusDelta earlier, StatusDelta later) => new(
        Prompt: later.Prompt | earlier.Prompt,
        PromptDefault: later.PromptDefault | earlier.PromptDefault,
        PromptMessage: later.PromptMessage | earlier.PromptMessage,
        Pane: later.Pane | (later.ClearPane ? Option<string>.None : earlier.Pane),
        Distance: later.Distance | earlier.Distance,
        Number: later.Number | earlier.Number,
        Point: later.Point | earlier.Point,
        Toasts: earlier.Toasts + later.Toasts,
        ClearPane: earlier.ClearPane || later.ClearPane);

    public static StatusDelta Collapse(params ReadOnlySpan<StatusDelta> deltas) =>
        toSeq(deltas.ToArray()).Fold(new StatusDelta(), static (folded, delta) => folded + delta);

    public static StatusDelta Scripted(string message) =>
        string.IsNullOrWhiteSpace(value: message) ? new StatusDelta() : new StatusDelta(PromptMessage: Some(message.Trim()));

    public Fin<StatusReceipt> Apply(Op? key = null) {
        Op op = key.OrDefault();
        StatusDelta delta = this;
        return HostThread.On(body: () => {
            _ = (delta.Prompt.Case, delta.PromptDefault.Case) switch {
                (string prompt, string fallback) => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: prompt, promptDefault: fallback)),
                (string prompt, _) => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: prompt)),
                _ => unit,
            };
            _ = delta.PromptMessage.Iter(static value => RhinoApp.SetCommandPromptMessage(prompt: value));
            _ = Op.SideWhen(delta.ClearPane, static () => StatusBar.ClearMessagePane());
            _ = delta.Pane.Iter(static value => StatusBar.SetMessagePane(message: value));
            _ = delta.Distance.Iter(static value => StatusBar.SetDistancePane(distance: value));
            _ = delta.Number.Iter(static value => StatusBar.SetNumberPane(number: value));
            _ = delta.Point.Iter(static value => StatusBar.SetPointPane(point: value));
            return delta.Toasts
                .TraverseM(note => Shown(note: note, op: op).Map(Some) | Fin.Succ(value: Option<ToastHandle>.None))
                .As()
                .Map(handles => new StatusReceipt(Toasts: handles.Somes().Strict()));
        }, key: op);
    }

    private static Fin<ToastHandle> Shown(ToastNote note, Op op) =>
        from view in Optional(note.View).ToFin(Fail: op.MissingContext())
        from message in op.AcceptText(value: note.Message)
        select new ToastHandle(Id: (note.TextHeight.Case, note.Location.Case) switch {
            (int height, DrawingPointF point) when height > 0 => view.ShowToast(message, height, point),
            (int height, _) when height > 0 => view.ShowToast(message, height),
            _ => view.ShowToast(message),
        });
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PromptWatch {
    public static Fin<ShellSubscription> Observe(Action<PromptFact> onPrompt, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => {
            long ordinal = 0;
            EventHandler<CommandPromptChangedEventArgs> handler = (_, args) => ignore(op.Catch(() => {
                onPrompt(new PromptFact(
                    Prompt: args.Prompt,
                    Default: Optional(args.PromptDefault).Filter(static value => value.Length > 0),
                    Options: toSeq(args.Options).Map(static option => (option.Index, English: option.EnglishName, Local: option.LocalName)).Strict(),
                    Ordinal: Interlocked.Increment(location: ref ordinal)));
                return Fin.Succ(value: unit);
            }));
            RhinoApp.CommandPromptChanged += handler;
            return Fin.Succ(value: new ShellSubscription(Halt: () => Op.Side(() => RhinoApp.CommandPromptChanged -= handler)));
        });
    }
}
```

## [04]-[PROGRESS_METER]

- Owner: `MeterGrant` — the closed ownership family the tri-state `StatusBar.ShowProgressMeter` return admits: `Created` owns the meter and its teardown, `Foreign` names a meter another process owns (every step is a witnessed no-op, never a failure, and teardown never touches it), `Refused` never constructs a lease — and `ProgressLease`, the disposable meter scope: `Step` drives absolute, relative, and label-only updates as one value, `Fold` threads a collection through step-per-item progress, `Fraction` projects the position onto the kernel `UnitInterval`, and `Dispose` hides only the owned meter exactly once. `Progress.Use` is the sole entry: it demands `SessionNeed.Redraw` on the `DocumentSession` (a headless or view-less document cannot host a meter), opens the meter against the session's `DocKey` serial, and brackets the body so no exit path leaks a visible meter.
- Law: ownership is structural evidence, not a boolean — the grant case decides step and teardown behavior through total generated dispatch, so updating or hiding a foreign meter is unrepresentable rather than forbidden.
- Law: a label-only step rides the host's own contract — `UpdateProgressMeter` at `RhinoMath.UnsetIntIndex` updates the label without moving the position — and an out-of-window position is a typed rejection before any host write, so the meter never renders an impossible state.
- Law: `MirrorTaskbar` is a policy slot, not a second progress surface — the mirror projects `Fraction` through the Eto sub-domain's `TaskbarPulse` on every step and clears it at disposal, so OS-level progress is a derived view of the one meter position.
- Exemption: the `try`/`finally` bracket inside `Use` is the platform-forced lease seam — the lease must dispose on the failure arm the rail is mid-flight on.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`), Rasm.Numerics (`UnitInterval`), Document sub-domain (`DocumentSession`, `SessionNeed`, `DocKey`), Eto sub-domain (`UiFault`, `TaskbarPulse`, `PulseState`), RhinoCommon (`StatusBar.ShowProgressMeter`/`UpdateProgressMeter` both arities/`HideProgressMeter`, `RhinoMath.UnsetIntIndex`).
- Growth (CONSUMER): a long-running consumer axis (step cadence throttling, an elapsed-time column on the lease) is one `ProgressSpec` slot or one lease member; a second meter abstraction is the deleted form.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeterGrant {
    private MeterGrant() { }
    public sealed record Created(DocKey Document) : MeterGrant;
    public sealed record Foreign : MeterGrant;
    public sealed record Refused : MeterGrant;

    internal static MeterGrant Admit(int code, DocKey document) =>
        code switch {
            1 => new Created(Document: document),
            -1 => new Foreign(),
            _ => new Refused(),
        };
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ProgressSpec(
    int Lower, int Upper, string Label, bool EmbedLabel = true, bool ShowPercent = true, bool MirrorTaskbar = false);

public readonly record struct ProgressStep(Option<int> Position = default, Option<string> Label = default, bool Absolute = true) {
    public static ProgressStep To(int position, string? label = null) => new(Position: Some(position), Label: Optional(label));
    public static ProgressStep By(int delta = 1, string? label = null) => new(Position: Some(delta), Label: Optional(label), Absolute: false);
    public static ProgressStep Note(string label) => new(Label: Some(label));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ProgressLease : IDisposable {
    private readonly MeterGrant grant;
    private readonly ProgressSpec spec;
    private readonly Atom<int> current;
    private int released;

    internal ProgressLease(MeterGrant grant, ProgressSpec spec) {
        this.grant = grant;
        this.spec = spec;
        current = Atom(spec.Lower);
    }

    public MeterGrant Grant => grant;

    public UnitInterval Fraction =>
        UnitInterval.Create(value: spec.Upper > spec.Lower
            ? Math.Clamp(value: (current.Value - spec.Lower) / (double)(spec.Upper - spec.Lower), min: 0.0, max: 1.0)
            : 1.0);

    public Fin<int> Step(ProgressStep step, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(flag: Volatile.Read(location: ref released) is 0, False: op.MissingContext()).ToFin()
               from position in grant.Switch(
                   state: (Self: this, Move: step, Op: op),
                   created: static (held, owned) => held.Self.Drive(document: owned.Document, step: held.Move, op: held.Op),
                   foreign: static (held, _) => Fin.Succ(value: held.Self.current.Value),
                   refused: static (held, _) => Fin.Fail<int>(error: new UiFault.Unavailable(Key: held.Op, Capability: nameof(StatusBar.ShowProgressMeter))))
               select position;
    }

    public Fin<TState> Fold<TItem, TState>(
        Seq<TItem> items, TState seed, Func<TState, TItem, Fin<TState>> step, Func<TItem, ProgressStep> progress, Op? key = null) {
        Op op = key.OrDefault();
        return items.Fold(
            Fin.Succ(value: seed),
            (state, item) =>
                from held in state
                from next in step(held, item)
                from _ in Step(step: progress(item), key: op)
                select next);
    }

    public void Dispose() {
        _ = Interlocked.Exchange(location1: ref released, value: 1) is 0
            ? grant.Switch(
                state: spec,
                created: static (held, owned) => {
                    _ = Op.Side(() => StatusBar.HideProgressMeter(docSerialNumber: owned.Document));
                    return Op.SideWhen(held.MirrorTaskbar, static () => ignore(TaskbarPulse.Clear()));
                },
                foreign: static (_, _) => unit,
                refused: static (_, _) => unit)
            : unit;
    }

    private Fin<int> Drive(DocKey document, ProgressStep step, Op op) {
        int held = current.Value;
        Option<int> target = step.Position.Map(position => step.Absolute ? position : held + position);
        return from _ in target.Match(
                   Some: position => guard(flag: position >= spec.Lower && position <= spec.Upper, False: op.InvalidInput()).ToFin(),
                   None: () => Fin.Succ(value: unit))
               from position in op.Catch(() => {
                   _ = (target.Case, step.Position.Case, step.Label.Case) switch {
                       (int _, int raw, string label) => Op.Side(() => StatusBar.UpdateProgressMeter(
                           docSerialNumber: document, label: label, position: raw, absolute: step.Absolute)),
                       (int _, int raw, _) => Op.Side(() => StatusBar.UpdateProgressMeter(
                           docSerialNumber: document, position: raw, absolute: step.Absolute)),
                       (_, _, string label) => Op.Side(() => StatusBar.UpdateProgressMeter(
                           docSerialNumber: document, label: label, position: RhinoMath.UnsetIntIndex, absolute: true)),
                       _ => unit,
                   };
                   return Fin.Succ(value: ignore(current.Swap(_ => target.IfNone(held))));
               })
               from _mirror in Fin.Succ(value: Op.SideWhen(spec.MirrorTaskbar, () => ignore(TaskbarPulse.Show(state: PulseState.Working, fraction: Fraction))))
               select target.IfNone(held);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Progress {
    public static Fin<T> Use<T>(DocumentSession session, ProgressSpec spec, Func<ProgressLease, Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: _ =>
                from label in op.AcceptText(value: spec.Label)
                from _ in guard(flag: spec.Upper >= spec.Lower, False: op.InvalidInput()).ToFin()
                from lease in MeterGrant.Admit(
                    code: StatusBar.ShowProgressMeter(
                        docSerialNumber: session.Key,
                        lowerLimit: spec.Lower,
                        upperLimit: spec.Upper,
                        label: label,
                        embedLabel: spec.EmbedLabel,
                        showPercentComplete: spec.ShowPercent),
                    document: session.Key) switch {
                        MeterGrant.Refused => Fin.Fail<ProgressLease>(error: new UiFault.Unavailable(Key: op, Capability: nameof(StatusBar.ShowProgressMeter))),
                        MeterGrant granted => Fin.Succ(value: new ProgressLease(grant: granted, spec: spec)),
                    }
                from result in Bracketed(lease: lease, body: body, op: op)
                select result,
            op: op,
            SessionNeed.Redraw);
    }

    private static Fin<T> Bracketed<T>(ProgressLease lease, Func<ProgressLease, Fin<T>> body, Op op) {
        try { return op.Catch(() => body(lease)); }
        finally { lease.Dispose(); }
    }
}
```

## [05]-[WINDOW_OWNERSHIP]

- Owner: `ShellWindows` — the one seam between an Eto window and Rhino's document-window regime — and `WindowDress`, the adoption policy value: native styling (`UseRhinoStyle`), placement persistence keyed by window type (`RestorePosition`/`SavePosition`), and the localize-and-restore fold (`LocalizeAndRestore`). `Adopt` lands a modeless `Form` against a live document in one fold — style, restore or localize, wire save-on-close, `Show` — under `SessionNeed.Redraw`; `SemiModal` presents a typed Eto `Dialog<T>` document-anchored through `ShowSemiModal` under `SessionNeed.Dialog`, defaulting its parent to the document main window; `MainWindow` resolves `RhinoEtoApp.MainWindowForDocument` under a session demand and `AppWindow` resolves the application `RhinoEtoApp.MainWindow` with no document in play; `Discover<TWindow>` projects `WindowsFromDocument<TWindow>` as a typed census under `SessionNeed.Read`, answering empty on a window-less document, and `Owner` inverts the seam — `EtoExtensions.GetRhinoDoc` detached onto a `DocKey` — naming the document a shown form belongs to. Every document-scoped entry crosses `HostThread.OnSession` — a raw `EtoExtensions` call or a bare `DocKey` handle re-entry outside this owner is the scattered census form this fold deletes.
- Law: placement persistence keys on the window's own type — one type, one persisted slot — so two instances of one window shape share a slot by host contract; a per-instance slot is a window-identity concern the host does not carry, and the dress declares which persistence verbs run rather than call sites choosing member spellings. The `Localize` row rides the host's fused `LocalizeAndRestore`, so localization implies placement restore by host contract.
- Law: `SemiModal` returns the dialog's typed result verbatim — the affirm/dismiss rail belongs to the Eto chrome `Prompt`, and a consumer wanting railed dismissal composes a `Prompt`-built dialog through this presentation; a second dismissal vocabulary here forks the modal rail.
- Owner: `ShellTheme` — the Rhino light/dark edge: `Current` projects `HostUtils.RunningInDarkMode` onto the Eto sub-domain's `ThemeVariant`, `Sync` pushes the current variant through `ThemeSeam.OnHostThemeChanged` so one call swaps the theme catalog and rebroadcasts every tracked control, and `Observe` subscribes the host `ThemeSettings.ThemeChanged` transition edge — a public static handler field, the per-control notifier behind `EtoExtensions` being host-private — pulsing `Sync` per transition and returning a `ShellSubscription`. The shell owns this wiring; the Eto platform seam consumes it.
- Packages: LanguageExt.Core, Rasm.Domain (`Op`), Document sub-domain (`DocumentSession`, `SessionNeed`, `DocKey`), Eto sub-domain (`UiFault`, `ThemeVariant`, `ThemeSeam`), Rhino.UI (`RhinoEtoApp.MainWindow`/`MainWindowForDocument`, `EtoExtensions.UseRhinoStyle`/`Show`/`ShowSemiModal` both arities/`SavePosition`/`RestorePosition`/`LocalizeAndRestore`/`WindowsFromDocument<T>`/`GetRhinoDoc`, `ThemeSettings.ThemeChanged`), RhinoCommon (`HostUtils.RunningInDarkMode`).
- Growth (CONSUMER): a window-lifecycle evidence axis future plugins require (adopted-at ordinal, restored-versus-defaulted placement) is one field on the `Adopt` return; a new host presentation member is one verb on this owner.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record WindowDress(bool RhinoStyle = true, bool RestorePlacement = true, bool SavePlacement = true, bool Localize = false) {
    public static readonly WindowDress Native = new();
    public static readonly WindowDress Localized = new(Localize: true);
    public static readonly WindowDress Bare = new(RhinoStyle: false, RestorePlacement: false, SavePlacement: false);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ShellWindows {
    public static Fin<Window> AppWindow(Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.On(body: () => Optional(RhinoEtoApp.MainWindow).ToFin(Fail: op.MissingContext()), key: op);
    }

    public static Fin<Window> MainWindow(DocumentSession session, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => Optional(RhinoEtoApp.MainWindowForDocument(document)).ToFin(Fail: op.MissingContext()),
            op: op,
            SessionNeed.Redraw);
    }

    public static Fin<Form> Adopt(Form window, DocumentSession session, Option<WindowDress> dress = default, Op? key = null) {
        Op op = key.OrDefault();
        WindowDress policy = dress.IfNone(WindowDress.Native);
        return HostThread.OnSession(
            session: session,
            body: document => {
                _ = Op.SideWhen(policy.RhinoStyle, () => EtoExtensions.UseRhinoStyle(window));
                _ = (policy.Localize, policy.RestorePlacement) switch {
                    (true, _) => Op.Side(() => EtoExtensions.LocalizeAndRestore(window, window.GetType())),
                    (false, true) => Op.Side(() => ignore(EtoExtensions.RestorePosition(window, window.GetType()))),
                    _ => unit,
                };
                _ = Op.SideWhen(policy.SavePlacement, () => window.Closed += (_, _) => EtoExtensions.SavePosition(window, window.GetType()));
                EtoExtensions.Show(window, document);
                return Fin.Succ(value: window);
            },
            op: op,
            SessionNeed.Redraw);
    }

    public static Fin<TResult> SemiModal<TResult>(Dialog<TResult> dialog, DocumentSession session, Option<Control> parent = default, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => parent
                .Match(
                    Some: static anchor => Fin.Succ(value: anchor),
                    None: () => Optional((Control)RhinoEtoApp.MainWindowForDocument(document)).ToFin(Fail: op.MissingContext()))
                .Map(owner => EtoExtensions.ShowSemiModal(dialog, document, owner)),
            op: op,
            SessionNeed.Dialog);
    }

    public static Fin<Seq<TWindow>> Discover<TWindow>(DocumentSession session, Op? key = null) where TWindow : Window {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => Fin.Succ(value: toSeq(EtoExtensions.WindowsFromDocument<TWindow>(document)).Strict()),
            op: op,
            SessionNeed.Read);
    }

    public static Fin<DocKey> Owner(Form window, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.On(
            body: () => Optional(EtoExtensions.GetRhinoDoc(window))
                .ToFin(Fail: op.MissingContext())
                .Bind(document => DocKey.Of(document: document, key: op)),
            key: op);
    }
}

public static class ShellTheme {
    public static ThemeVariant Current => HostUtils.RunningInDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;

    public static Unit Sync() => ignore(ThemeSeam.OnHostThemeChanged(next: Current));

    public static Fin<ShellSubscription> Observe(Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => {
            EventHandler handler = (_, _) => ignore(op.Catch(() => Fin.Succ(value: Sync())));
            ThemeSettings.ThemeChanged += handler;
            return Fin.Succ(value: new ShellSubscription(Halt: () => Op.Side(() => ThemeSettings.ThemeChanged -= handler)));
        });
    }
}
```
