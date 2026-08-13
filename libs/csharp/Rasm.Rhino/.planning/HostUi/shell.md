# [RASM_RHINO_HOSTUI_SHELL]

`Rasm.Rhino.HostUi` owns the Rhino application shell over Eto.

## [01]-[INDEX]

- [02]-[HOST_THREAD]: `HostWork<T>` and `HostThread.Run` own affine execution, queued delivery, affinity-required work, provenance-guarded work, and document-scoped work; `MarshalLatency` seats the marshal-seam checkpoint ledger and the stall gauge — lane budgets, breach verdicts, and last-stall retention.
- [03]-[STATUS]: `StatusProgram` folds prompt, pane, point, and toast intent into one document-scoped crossing, rescales every measured case into the live regime, and preserves every toast outcome.
- [04]-[PROGRESS]: `ProgressPolicy`, `ProgressMove`, and `ProgressLease` own admission, movement, projection, contention evidence, and release; the lease publishes the host governance band — `Fraction`/`Ticks` reporters and the escape-armed `Cancel` — every paced fold reads.
- [05]-[WINDOWS]: `WindowScope`, `WindowPolicy`, `ShellWindows`, and `ShellTheme` own host parents, adoption, typed and untyped modal presentation, discovery, placement, and theme transitions.
- [06]-[RUNTIME]: `HostFacts`, `HostAssemblies`, `HostScripts`, and `ShellSkin` own capability probes, resolver receipts, collectible loading, the script engine, and the skin load-phase hook; `ShellHooks` mounts the skin phase route on the registry.
- [07]-[CALLBACKS]: `CallbackObserver<T>`, `NamedKind`, `NamedBag`, and `NamedCallbacks` close guarded delivery and the typed named-parameter wire; `NodeFunctions` projects the node-in-code table onto the same crossing.
- [08]-[NOTICES]: `NoticeSpec`, `NoticeLease`, and `Notices` mint, present, annotate, and observe host notifications under the assembly-restriction guard.
- [09]-[TELEMETRY_ROOT]: `ShellIdentity` and `ShellTelemetry.Resolve` mint the plugin-side identity record — discriminator, ALC, version, content root, host snapshot — the `apps/rhino/<Plugin>/` composition root binds when it opens the per-ALC AppHost telemetry capsule.

## [02]-[HOST_THREAD]

- Owner: `HostWork<T>` closes execution modality, and `HostThread.Run` is the sole command-thread entry.
- Cases: `Execute` marshals when required, `Posted` carries an admitted `PostWaitLimit`, `Required` rejects an off-thread caller, `Guarded` brackets a faultable native call in `RiskyAction` so the host records provenance, and `Session` composes `DocumentSession.Demand` with detached result capture.
- Entry: `HostThread.Run<T>(HostWork<T>, Op?)` admits the operation once and returns `Fin<T>`.
- Law: `Session` carries every `SessionNeed` in the request value; a consumer never opens a second document demand beside the host operation.
- Law: provenance is a case, never a caller flag — `Guarded` marshals exactly like `Execute` and adds only the `RiskyAction` bracket around the body.
- Law: the posted state cell is the terminal probe, not a marker — the expiry CAS separates a body that never started from one already running, and a `Settled` read after a lapsed wait answers with the late result rather than discarding a completed crossing.
- Law: marshal-seam latency is a mounted ledger, never a second clock — `MarshalLatency` seats one `ILatencyContextProvider` first-mount-wins under the mounting plugin's identity, the app root registers the checkpoint and tag names through `RegisterCheckpointNames`/`RegisterTagNames` and the tokens resolve once at mount, and an empty seat is the zero-cost pass-through; the `rhino.marshal` instrument row on `Objects/authoring.md` projects this ledger at the app root under the `DurationInstrument` label `rasm.rhino.hostui.marshal.duration`.
- Law: the gauged set is every crossing that can queue — `Execute` and `Guarded` when marshalled, `Posted` always, and `Session` whole (its `Demand` marshals inside the host) — while `Required` never crosses (its off-thread arm is a refusal, not a queue), and the checkpoint pair lands on every exit path because the settle rides `finally`, never a success-only tail.
- Law: the stall gauge is the marshal seam's own verdict half — every gauged body mints exactly one `MarshalPulse` (lane, elapsed, breach against the lane's frame-multiple budget) whatever the exit path, a breached pulse retains on `LastStall` as hang evidence beside the running `LastPulse`, budgets tune through one `StallPolicy` value carrying its injected `TimeProvider`, and observers tap through `Watch` under keyed detach; an untuned default frame of 1/30 s over-reports and never hides, and the GH boundary's `DispatchPulse` is the twin discipline, plural by the host-twins ruling.
- Boundary: `HostThread` owns Rhino command-thread affinity, while `UiThread` owns Eto control-tree affinity; the Eto dispatch seam carries its own gauge at its own page.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto.Forms;
using Microsoft.Extensions.Diagnostics.Latency;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.NodeInCode;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Runtime;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingPointF = System.Drawing.PointF;
using HostNotice = Rhino.Runtime.Notifications.Notification;
using HostNoticeButton = Rhino.Runtime.Notifications.ButtonType;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CallbackObserver<T> {
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());

    public Action<Fin<T>> Deliver { get; }
    public Func<Error, Unit> Reject { get; }
    public Seq<Error> Faults => faults.Value;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Action<Fin<T>> deliver,
        ref Func<Error, Unit> reject) =>
        validationError = deliver is null || reject is null
            ? new ValidationError(message: "Callback observer delegates are missing.")
            : null;

    internal Unit Guard(Func<Fin<T>> project, Op op) {
        Fin<T> result = op.Catch(project);
        return op.Catch(() => {
            Deliver(result);
            return Fin.Succ(value: unit);
        }).Match(
            Succ: static _ => unit,
            Fail: primary => {
                Error retained = op.Catch(() => Fin.Succ(value: Reject(primary))).Match(
                    Succ: static _ => primary,
                    Fail: secondary => primary + secondary);
                _ = faults.Swap(rows => rows.Add(retained));
                return unit;
            });
    }

    internal CallbackObserver<T> Fork() => Create(deliver: Deliver, reject: Reject);
}

[ComplexValueObject]
public sealed partial class HostText {
    public string English { get; }
    public int Context { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string english,
        ref int context) =>
        validationError = string.IsNullOrWhiteSpace(english)
            ? new ValidationError(message: "Host text is empty.")
            : null;

    internal string Resolve() => Localization.LocalizeString(english: English, contextId: Context);
    internal LocalizeStringPair OptionName() => Localization.LocalizeCommandOptionName(english: English, contextId: Context);
    internal LocalizeStringPair OptionValue() => Localization.LocalizeCommandOptionValue(english: English, contextId: Context);
}

[ValueObject<TimeSpan>]
public readonly partial struct PostWaitLimit {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref TimeSpan value) =>
        validationError = value <= TimeSpan.Zero
            ? new ValidationError(message: "Posted work wait limit is not positive.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostWork<T> {
    private HostWork() { }

    public sealed record Execute(Func<Fin<T>> Body) : HostWork<T>;
    public sealed record Posted(Func<Fin<T>> Body, PostWaitLimit Wait) : HostWork<T>;
    public sealed record Required(Func<Fin<T>> Body) : HostWork<T>;
    public sealed record Guarded(Func<Fin<T>> Body, HostText Description) : HostWork<T>;
    public sealed record Session(
        DocumentSession Document,
        Seq<SessionNeed> Needs,
        Func<RhinoDoc, Fin<T>> Body) : HostWork<T>;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostThread {
    private enum PostedState { Pending, Running, Expired, Settled }


    public static Fin<T> Run<T>(HostWork<T> work, Op? key = null) {
        ArgumentNullException.ThrowIfNull(work);
        Op op = key.OrDefault();
        return work.Switch(
            op,
            execute: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Marshalled(body: request.Body, op: held, lane: MarshalLane.Execute, work: nameof(HostWork<T>.Execute)),
            posted: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Posted(request: request, op: held),
            required: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Fin.Fail<T>(error: new UiFault.OffThread(Key: held)),
            guarded: static (held, request) => RhinoApp.IsOnMainThread
                ? Bracketed(request: request, op: held)
                : Marshalled(body: () => Bracketed(request: request, op: held), op: held, lane: MarshalLane.Guarded, work: nameof(HostWork<T>.Guarded)),
            session: static (held, request) => MarshalLatency.Measured(
                lane: MarshalLane.Session,
                work: nameof(HostWork<T>.Session),
                run: () => Session(work: request, op: held)));
    }

    internal static Fin<Unit> Release(Seq<Func<Fin<Unit>>> releases, Op? key = null) {
        Op op = key.OrDefault();
        return Run(
            work: new HostWork<Unit>.Execute(Body: () => {
                Seq<Error> faults = releases.Choose(release => op.Catch(release).Match(
                    Succ: static _ => Option<Error>.None,
                    Fail: Some))
                    .Strict();
                return faults.Head.Match(
                    Some: first => Fin.Fail<Unit>(error: faults.Tail.Fold(first, static (all, next) => all + next)),
                    None: static () => Fin.Succ(value: unit));
            }),
            key: op);
    }

    private static Fin<T> Bracketed<T>(HostWork<T>.Guarded request, Op op) =>
        op.Catch(() => {
            using RiskyAction guard = new(description: request.Description.English);
            return request.Body();
        });

    private static Fin<T> Marshalled<T>(Func<Fin<T>> body, Op op, MarshalLane lane, string work) =>
        MarshalLatency.Measured(lane: lane, work: work, run: () => op.Catch(() => {
            Fin<T>? captured = null;
            RhinoApp.InvokeAndWait(action: () => captured = op.Catch(body));
            return Settled(captured: captured, op: op, capability: nameof(RhinoApp.InvokeAndWait));
        }));

    private static Fin<T> Posted<T>(HostWork<T>.Posted request, Op op) =>
        MarshalLatency.Measured(lane: MarshalLane.Posted, work: nameof(HostWork<T>.Posted), run: () => op.Catch(() => {
            int state = (int)PostedState.Pending;
            TaskCompletionSource<Fin<T>> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            RhinoApp.InvokeOnUiThread(
                method: () => {
                    if (Interlocked.CompareExchange(
                            location1: ref state,
                            value: (int)PostedState.Running,
                            comparand: (int)PostedState.Pending) is not (int)PostedState.Pending)
                        return;
                    completed.TrySetResult(op.Catch(request.Body));
                    Volatile.Write(location: ref state, value: (int)PostedState.Settled);
                },
                args: []);
            if (completed.Task.Wait(request.Wait.ToValue())) return completed.Task.Result;
            // Only a body that never started loses the expiry CAS; a lost CAS proves the body was already running
            // when the wait lapsed, so `Settled` is the reader that recovers a result the timeout otherwise discards
            // — a posted body that finished late still answers rather than faulting.
            return Interlocked.CompareExchange(
                    location1: ref state,
                    value: (int)PostedState.Expired,
                    comparand: (int)PostedState.Pending) is (int)PostedState.Pending
                || Volatile.Read(location: ref state) is not (int)PostedState.Settled
                ? Fin.Fail<T>(error: new UiFault.Unavailable(Key: op, Capability: nameof(RhinoApp.InvokeOnUiThread)))
                : completed.Task.Result;
        }));

    private static Fin<T> Session<T>(HostWork<T>.Session work, Op op) {
        Fin<T>? captured = null;
        return work.Document
            .Demand(
                use: document => {
                    captured = op.Catch(() => work.Body(document));
                    return captured.Value.Map(_ => work.Document.Key);
                },
                key: op,
                needs: work.Needs.ToArray())
            .Bind(_ => Settled(captured: captured, op: op, capability: nameof(DocumentSession.Demand)));
    }

    private static Fin<T> Settled<T>(Fin<T>? captured, Op op, string capability) =>
        captured is { } result
            ? result
            : Fin.Fail<T>(error: new UiFault.Unavailable(Key: op, Capability: capability));
}

[SmartEnum<int>]
public sealed partial class MarshalLane {
    public static readonly MarshalLane Execute = new(key: 0, frames: 1.0);
    public static readonly MarshalLane Guarded = new(key: 1, frames: 1.0);
    public static readonly MarshalLane Posted = new(key: 2, frames: 6.0);
    public static readonly MarshalLane Session = new(key: 3, frames: 4.0);

    public double Frames { get; }

    internal TimeSpan Budget(TimeSpan frame) => frame * Frames;
}

public sealed record StallPolicy(TimeProvider Clock, TimeSpan Frame, HashMap<int, TimeSpan> Bounds) {
    // Rhino publishes no display refresh interval, so the untuned floor is 1/30 s — over-reports and never hides;
    // an app root with a real frame anchor pushes it through Tune.
    public static readonly StallPolicy Default = new(
        Clock: TimeProvider.System, Frame: TimeSpan.FromSeconds(1.0 / 30.0), Bounds: HashMap<int, TimeSpan>());

    public TimeSpan Bound(MarshalLane lane) => Bounds.Find(lane.Key).IfNone(() => lane.Budget(frame: Frame));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MarshalPulse(string Work, MarshalLane Lane, TimeSpan Elapsed, bool Breached) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Of(holds: Lane is not null && Elapsed >= TimeSpan.Zero);
}

public static class MarshalLatency {
    public const string DurationInstrument = "rasm.rhino.hostui.marshal.duration";
    public const string QueuedCheckpoint = "rasm.rhino.marshal.queued";
    public const string SettledCheckpoint = "rasm.rhino.marshal.settled";
    public const string WorkTag = "rasm.rhino.marshal.work";
    public const string OutcomeTag = "rasm.rhino.marshal.outcome";

    private static readonly Atom<Option<SeatRow>> Seat = Atom(Option<SeatRow>.None);
    private static readonly Atom<StallPolicy> Pacing = Atom(StallPolicy.Default);
    private static readonly Atom<Option<MarshalPulse>> LastPulseCell = Atom(Option<MarshalPulse>.None);
    private static readonly Atom<Option<MarshalPulse>> LastStallCell = Atom(Option<MarshalPulse>.None);
    private static readonly Atom<HashMap<Guid, Action<MarshalPulse>>> PulseTaps = Atom(HashMap<Guid, Action<MarshalPulse>>());

    public static Option<MarshalPulse> LastPulse => LastPulseCell.Value;
    public static Option<MarshalPulse> LastStall => LastStallCell.Value;

    // App root registers the four names through RegisterCheckpointNames/RegisterTagNames before mounting; tokens resolve once here.
    public static Fin<IDisposable> Mount(PluginKey plugin, ILatencyContextProvider provider, ILatencyContextTokenIssuer issuer, Op? key = null) {
        Op op = key.OrDefault();
        return from live in op.Need(provider)
               from mint in op.Need(issuer)
               from row in op.Catch(() => Fin.Succ(value: new SeatRow(
                   Plugin: plugin,
                   Provider: live,
                   Queued: mint.GetCheckpointToken(QueuedCheckpoint),
                   Settled: mint.GetCheckpointToken(SettledCheckpoint),
                   Work: mint.GetTagToken(WorkTag),
                   Outcome: mint.GetTagToken(OutcomeTag))))
               from seat in Seat.Swap(held => held.IsNone ? Some(row) : held)
                   .Filter(held => ReferenceEquals(held, row))
                   .ToFin(Fail: op.InvalidContext())
               select (IDisposable)Subscription.Of(detach: () => ignore(Seat.Swap(held =>
                   held.Filter(live2 => ReferenceEquals(live2, row)).IsSome ? Option<SeatRow>.None : held)));
    }

    public static Fin<Unit> Tune(StallPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(policy).Map(next => ignore(Pacing.Swap(_ => next)));
    }

    public static Fin<IDisposable> Watch(Action<MarshalPulse> observer, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(observer).Map(tap => {
            Guid token = Guid.NewGuid();
            _ = PulseTaps.Swap(rows => rows.Add(token, tap));
            return (IDisposable)Subscription.Of(detach: () => ignore(PulseTaps.Swap(rows => rows.Remove(token))));
        });
    }

    // The gauge times EVERY gauged body — an empty ledger seat drops only the checkpoint half, never the pulse —
    // and both settles ride finally, so no exit path skips the checkpoint pair or the pulse mint.
    internal static Fin<T> Measured<T>(MarshalLane lane, string work, Func<Fin<T>> run) {
        StallPolicy pacing = Pacing.Value;
        long start = pacing.Clock.GetTimestamp();
        try {
            return Seat.Value.Match(
                None: run,
                Some: seat => {
                    ILatencyContext ledger = seat.Provider.CreateContext();
                    ledger.SetTag(seat.Work, work);
                    ledger.AddCheckpoint(seat.Queued);
                    bool succ = false;
                    try {
                        Fin<T> held = run();
                        succ = held.IsSucc;
                        return held;
                    }
                    finally {
                        ledger.AddCheckpoint(seat.Settled);
                        ledger.SetTag(seat.Outcome, succ ? "succ" : "fail");
                        ledger.Freeze();
                    }
                });
        }
        finally {
            TimeSpan elapsed = pacing.Clock.GetElapsedTime(startingTimestamp: start);
            MarshalPulse pulse = new(Work: work, Lane: lane, Elapsed: elapsed, Breached: elapsed > pacing.Bound(lane: lane));
            _ = LastPulseCell.Swap(_ => Some(pulse));
            _ = Op.SideWhen(condition: pulse.Breached, action: () => ignore(LastStallCell.Swap(_ => Some(pulse))));
            PulseTaps.Value.Values.Iter(tap => ignore(Op.Of(name: nameof(MarshalLatency)).Catch(() => Fin.Succ(value: Op.Side(action: () => tap(pulse))))));
        }
    }

    private sealed record SeatRow(
        PluginKey Plugin,
        ILatencyContextProvider Provider,
        CheckpointToken Queued,
        CheckpointToken Settled,
        TagToken Work,
        TagToken Outcome);
}
```

## [03]-[STATUS]

- Owner: `StatusProgram` is the ordered status algebra, and `StatusOp` carries one admitted host write per case.
- Cases: prompt, prompt message, optional message-pane content, numeric panes, point pane, and viewport toast.
- Entry: `StatusProgram.Apply` folds every case inside one document-scoped `HostWork<StatusReceipt>.Session` crossing that resolves the live `ModelUnit` regime once for the whole program.
- Receipt: `StatusReceipt` carries one `ToastOutcome` per toast, so an invalid or rejected notice stays typed without cancelling independent notices.
- Law: `StatusProgram.Combine` preserves producer order; each additional status axis is one `StatusOp` case and one fold arm.
- Law: a MEASURED case carries the regime its magnitude was resolved in — `StatusOp.Distance` takes the kernel `ModelUnit` the `Commands/acquisition.md` `Acquired.Distance` producer detaches — and the fold rescales through `ModelUnit.ScaleTo` before the host write, because the pane renders the DOCUMENT's unit label over whatever number it is handed and a regime-blind write relabels rather than converts. `Number` stays regime-free by construction: the pane's own contract is a dimensionless count.
- Boundary: `PromptWatch.Observe` detaches callback-scoped option handles into immutable `PromptFact` rows before guarded delivery.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ToastSpec {
    public RhinoView View { get; }
    public HostText Message { get; }
    public ToastPlacement Placement { get; }
}

[ValueObject<int>]
public readonly partial struct ToastHeight {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(message: "Toast height must be positive.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToastPlacement {
    private ToastPlacement() { }
    public sealed record Standard : ToastPlacement;
    public sealed record Scaled(ToastHeight Height) : ToastPlacement;
    public sealed record Located(ToastHeight Height, DrawingPointF Point) : ToastPlacement;
}

[ValueObject<uint>]
public readonly partial struct ToastId;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToastOutcome {
    private ToastOutcome() { }
    public sealed record Shown(ToastId Id) : ToastOutcome;
    public sealed record Refused(Error Fault) : ToastOutcome;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StatusOp {
    private StatusOp() { }
    public sealed record Prompt(HostText Text, Option<HostText> Default = default) : StatusOp;
    public sealed record PromptMessage(HostText Text) : StatusOp;
    public sealed record Pane(Option<HostText> Text = default) : StatusOp;
    public sealed record Distance(double Value, ModelUnit Unit) : StatusOp;
    public sealed record Number(double Value) : StatusOp;
    public sealed record Point(Point3d Value) : StatusOp;
    public sealed record Toast(ToastSpec Spec) : StatusOp;
}

public sealed record StatusProgram(Seq<StatusOp> Operations) {
    public static StatusProgram Combine(params ReadOnlySpan<StatusProgram> programs) =>
        new(Operations: Iterable<StatusProgram>.FromSpan(programs)
            .Fold(Seq<StatusOp>(), static (all, next) => all + next.Operations));

    // Panes are document space, so the program is too: the session resolves the live regime ONCE for the whole
    // fold and every measured case reads that one value, rather than each arm re-reading a document it was handed.
    public Fin<StatusReceipt> Apply(DocumentSession session, Op? key = null) {
        ArgumentNullException.ThrowIfNull(session);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<StatusReceipt>.Session(
                Document: session,
                Needs: [SessionNeed.Read],
                Body: document =>
                    from regime in ModelUnit.Of(value: document.ModelUnits, key: op)
                    from receipt in Operations.Fold(
                        Fin.Succ(value: new StatusReceipt(Toasts: Seq<ToastOutcome>())),
                        (state, next) => state.Bind(carried => Apply(next: next, receipt: carried, regime: regime, op: op)))
                    select receipt),
            key: op);
    }

    private static Fin<StatusReceipt> Apply(StatusOp next, StatusReceipt receipt, ModelUnit regime, Op op) =>
        next.Switch(
            (Receipt: receipt, Regime: regime, Op: op),
            prompt: static (held, write) => held.Op.AcceptText(value: write.Text.Resolve()).Map(text => {
                _ = write.Default.Match(
                    Some: fallback => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: text, promptDefault: fallback.Resolve())),
                    None: () => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: text)));
                return held.Receipt;
            }),
            promptMessage: static (held, write) => held.Op.AcceptText(value: write.Text.Resolve())
                .Map(text => (Op.Side(() => RhinoApp.SetCommandPromptMessage(prompt: text)), held.Receipt).Item2),
            pane: static (held, write) => write.Text.Case switch {
                HostText text => held.Op.AcceptText(value: text.Resolve())
                    .Map(accepted => (Op.Side(() => StatusBar.SetMessagePane(message: accepted)), held.Receipt).Item2),
                _ => Fin.Succ(value: (Op.Side(StatusBar.ClearMessagePane), held.Receipt).Item2),
            },
            // Rhino renders this magnitude under the document's own unit label, so a value carried in another
            // regime rescales before it is written — never relabels. Same-regime resolves to the identity
            // factor the one scale owner returns, so no arm branches on whether a conversion is needed.
            distance: static (held, write) => write.Unit.ScaleTo(target: held.Regime, key: held.Op)
                .Map(scale => (Op.Side(() => StatusBar.SetDistancePane(distance: write.Value * scale)), held.Receipt).Item2),
            number: static (held, write) => Fin.Succ(value: (Op.Side(() => StatusBar.SetNumberPane(number: write.Value)), held.Receipt).Item2),
            point: static (held, write) => Fin.Succ(value: (Op.Side(() => StatusBar.SetPointPane(point: write.Value)), held.Receipt).Item2),
            toast: static (held, write) => Fin.Succ(value: held.Receipt with {
                Toasts = held.Receipt.Toasts.Add(Shown(spec: write.Spec, op: held.Op)),
            }));

    private static ToastOutcome Shown(ToastSpec spec, Op op) =>
        (from view in Optional(spec.View).ToFin(Fail: op.MissingContext())
         from message in op.AcceptText(value: spec.Message.Resolve())
         from raised in op.Catch(() => Fin.Succ(value: spec.Placement.Switch(
             (View: view, Message: message),
             standard: static (held, _) => held.View.ShowToast(held.Message),
             scaled: static (held, placed) => held.View.ShowToast(held.Message, placed.Height.ToValue()),
             located: static (held, placed) => held.View.ShowToast(held.Message, placed.Height.ToValue(), placed.Point))))
         select ToastId.Create(value: raised))
        .Match<ToastOutcome>(Succ: static id => new ToastOutcome.Shown(Id: id), Fail: static fault => new ToastOutcome.Refused(Fault: fault));
}

public sealed record StatusReceipt(Seq<ToastOutcome> Toasts);

public sealed record PromptOption(int Index, string English, string Local);

public sealed record PromptFact(string Prompt, Option<string> Default, Seq<PromptOption> Options, long Ordinal);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PromptWatch {
    public static Fin<Subscription> Observe(CallbackObserver<PromptFact> observer, Op? key = null) {
        ArgumentNullException.ThrowIfNull(observer);
        Op op = key.OrDefault();
        long ordinal = 0;
        EventHandler<CommandPromptChangedEventArgs> handler = (_, args) => ignore(observer.Guard(
            project: () => Fin.Succ(value: new PromptFact(
                Prompt: args.Prompt,
                Default: Op.Text(args.PromptDefault),
                Options: toSeq(args.Options)
                    .Map(static option => new PromptOption(Index: option.Index, English: option.EnglishName, Local: option.LocalName))
                    .Strict(),
                Ordinal: Interlocked.Increment(location: ref ordinal))),
            op: op));
        return Subscription.Attach(
            subscribe: callback => RhinoApp.CommandPromptChanged += callback,
            unsubscribe: callback => RhinoApp.CommandPromptChanged -= callback,
            handler: handler);
    }
}
```

## [04]-[PROGRESS]

- Owner: `ProgressPolicy` admits the meter range, label, and projection features before any host call.
- Cases: `ProgressMove` closes absolute movement, relative movement, and label-only change; `MeterGrant` distinguishes an owned meter from a foreign meter.
- Entry: `Progress.Use` opens one document-scoped lease and brackets one callback; `ProgressLease.Advance` is the sole update operation.
- Receipt: `ProgressReceipt` carries grant, position, effective label, normalized fraction, and the taskbar projection fault for every attempted move.
- Law: only `MeterGrant.Owned` writes or hides the host meter; `MeterGrant.Foreign` returns unchanged witnessed receipts.
- Law: the lease IS the host end of the corpus governance band, so a paced fold takes `Fraction`/`Ticks` and `Cancel` off ONE value and no caller writes an `IProgress` shim of its own — `Modeling/meshing.md` `MeshRuntime`, `Modeling/projection.md` `ProjectionPacing`, and the kernel `ArrangementPolicy.Governed` seat are the three consumers, each already shaped for exactly these members. Every projection stays a view of `Advance`: a second position store beside the lease state forks the meter from its own receipt.
- Law: a refusal an `IProgress.Report` cannot return PARKS on `Faults` — the `void` host contract constrains the seam shape and never licenses discarding the bounds refusal that rail raises, so a fold reporting past its declared range leaves attributable evidence rather than a meter that silently stops.
- Law: escape arming is a policy ROW, so a lease either publishes a live abort edge or publishes `CancellationToken.None`, and the abort rows disarm on BOTH grants because a foreign meter still armed a native callback this lease owns.
- Law: the taskbar pulse is best-effort projection — a refused pulse lands as `TaskbarFault` evidence on the receipt, never a failed advance, so position and receipt always mirror the committed host meter.
- Boundary: `Progress.Use` demands `SessionNeed.Redraw`; release clears every owned projection, returns cleanup failure through the use rail, and retains failed attempts for explicit retry.
- Law: the lease locks in ONE direction — every operation crosses `HostThread` first and takes the state lock inside the marshalled body, never the reverse; a release holding the lock across a blocking marshal inverts the order against a concurrent advance and deadlocks the host thread against its own caller, so the marshal is always outside and the lock always inside.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ProgressFeature {
    public static readonly ProgressFeature EmbeddedLabel = new();
    public static readonly ProgressFeature Percentage = new();
    public static readonly ProgressFeature Taskbar = new();
    public static readonly ProgressFeature WaitCursor = new();
    // Escape arms the host abort edge for the lease's scope: what a user presses to stop a meter that is running
    // away is the same key the command loop already spells Cancel, and a metered fold with no abort edge is the
    // affordance without its exit. Absent, the lease's token is CancellationToken.None and every paced consumer
    // reads an uncancellable run — the honest default for a fold whose caller declared no abort.
    public static readonly ProgressFeature Escape = new();
}

[ComplexValueObject]
public sealed partial class ProgressPolicy {
    public int Lower { get; }
    public int Upper { get; }
    public HostText Label { get; }
    public FrozenSet<ProgressFeature> Features { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int lower,
        ref int upper,
        ref HostText label,
        ref FrozenSet<ProgressFeature> features) {
        validationError = upper < lower
            ? new ValidationError(message: "Progress policy is invalid.")
            : null;
    }

    public static Fin<ProgressPolicy> Of(
        int lower,
        int upper,
        HostText label,
        FrozenSet<ProgressFeature> features,
        Op? key = null) {
        Op op = key.OrDefault();
        return TryCreate(lower: lower, upper: upper, label: label, features: features, out ProgressPolicy? policy)
            ? Fin.Succ(value: policy)
            : Fin.Fail<ProgressPolicy>(error: op.InvalidInput());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgressMove {
    private ProgressMove() { }
    public sealed record Absolute(int Position, Option<HostText> Label = default) : ProgressMove;
    public sealed record Relative(int Delta, Option<HostText> Label = default) : ProgressMove;
    public sealed record Label(HostText Text) : ProgressMove;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeterGrant {
    private MeterGrant() { }
    public sealed record Owned(DocKey Document) : MeterGrant;
    public sealed record Foreign : MeterGrant;

    internal static Fin<MeterGrant> Admit(int code, DocKey document, Op op) =>
        code switch {
            1 => Fin.Succ<MeterGrant>(value: new Owned(Document: document)),
            -1 => Fin.Succ<MeterGrant>(value: new Foreign()),
            _ => Fin.Fail<MeterGrant>(error: new UiFault.Unavailable(Key: op, Capability: nameof(StatusBar.ShowProgressMeter))),
        };
}

public sealed record ProgressReceipt(
    MeterGrant Grant,
    int Position,
    HostText Label,
    UnitInterval Fraction,
    Option<Error> TaskbarFault);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ProgressLease : IDisposable {
    private sealed record ProgressState(int Position, HostText Label, bool Released);

    // IProgress projections are the ONE adapter between the host meter and every paced kernel and host fold —
    // Modeling/meshing.md MeshRuntime, Modeling/projection.md ProjectionPacing, and the kernel ArrangementPolicy
    // governance band all take Option<IProgress<double>> or IProgress<int>, so a lease hands its own Advance rail
    // outward instead of each caller writing the same shim. Report returns void and the rail returns Fin, so a
    // refused advance PARKS on the lease's fault cell — discarding it would hide a bounds refusal behind a meter
    // that silently stops moving, and the cell keeps it attributable to the Op that raised it.
    private sealed record LeaseReporter(ProgressLease Lease) : IProgress<double>, IProgress<int> {
        public void Report(double value) => Lease.Park(Lease.Advance(new ProgressMove.Absolute(
            Position: Lease.policy.Lower + (int)Math.Round(
                Math.Clamp(value: value, min: 0.0, max: 1.0) * (Lease.policy.Upper - Lease.policy.Lower)))));

        // The tick rail clamps into the declared range exactly as the fraction rail clamps into 0..1 — a consumer
        // pacing in its own domain range saturates at the meter's bounds instead of parking refusals forever.
        public void Report(int value) => Lease.Park(Lease.Advance(new ProgressMove.Absolute(
            Position: Math.Clamp(value: value, min: Lease.policy.Lower, max: Lease.policy.Upper))));
    }

    private readonly MeterGrant grant;
    private readonly Op op;
    private readonly ProgressPolicy policy;
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly LeaseReporter reporter;
    private readonly Option<Subscription> escape;
    private readonly Option<CancellationTokenSource> abort;
    private readonly object sync = new();
    private ProgressState state;

    internal ProgressLease(MeterGrant grant, ProgressPolicy policy, Option<CancellationTokenSource> abort, Option<Subscription> escape, Op op) {
        this.grant = grant;
        this.policy = policy;
        this.abort = abort;
        this.escape = escape;
        this.op = op;
        reporter = new LeaseReporter(Lease: this);
        state = new(Position: policy.Lower, Label: policy.Label, Released: false);
    }

    public Seq<Error> Faults => faults.Value;

    // One reporter instance serves both arities, so a consumer taking the fraction rail and one taking the tick
    // rail drive the SAME meter state and no second adapter exists to drift against it.
    public IProgress<double> Fraction => reporter;
    public IProgress<int> Ticks => reporter;

    // CancellationToken.None when the policy declared no Escape row — the host's own uncancellable spelling,
    // never an Option<CancellationToken> stacking a second absence on a value that already models one
    // (the Modeling/projection.md pacing law, held here at the producing end).
    public CancellationToken Cancel => abort.Match(Some: static source => source.Token, None: static () => CancellationToken.None);

    private Unit Park<T>(Fin<T> outcome) =>
        outcome.Match(Succ: static _ => unit, Fail: failure => ignore(faults.Swap(seen => seen.Add(failure))));

    public Fin<ProgressReceipt> Advance(ProgressMove move, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<ProgressReceipt>.Execute(Body: () => {
                lock (sync) {
                    return
                        from _ in guard(flag: !state.Released, False: op.MissingContext()).ToFin()
                        from next in move.Switch(
                            (State: state, Policy: policy, Op: op),
                            absolute: static (held, step) => Bounded(
                                position: step.Position,
                                label: step.Label.IfNone(held.State.Label),
                                held.Policy,
                                held.Op),
                            relative: static (held, step) => Bounded(
                                position: (long)held.State.Position + step.Delta,
                                label: step.Label.IfNone(held.State.Label),
                                held.Policy,
                                held.Op),
                            label: static (held, step) => held.Op.AcceptText(value: step.Text.Resolve())
                                .Map(_ => (Position: held.State.Position, Label: step.Text)))
                        from receipt in grant.Switch(
                            (Self: this, Move: next, Op: op),
                            owned: static (held, owner) => held.Self.Drive(document: owner.Document, held.Move, held.Op),
                            foreign: static (held, _) => Fin.Succ(value: held.Self.Receipt(state: held.Self.state)))
                        select receipt;
                }
            }),
            key: op);
    }

    // Lock order is one direction everywhere on this lease: cross to the host thread FIRST, take `sync` SECOND.
    // Holding `sync` across a blocking marshal inverts that order against `Advance` and deadlocks the pair — a caller
    // waiting on the host thread while the host thread executes an `Advance` body waiting on the lock it already holds.
    public Fin<Unit> Release() => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => {
            lock (sync) {
                if (state.Released) return Fin.Succ(value: unit);
                Fin<Unit> cleanup = Cleanup();
                return cleanup.Match(
                    Succ: _ => {
                        state = state with { Released = true };
                        return Fin.Succ(value: unit);
                    },
                    Fail: failure => {
                        _ = faults.Swap(rows => rows.Add(failure));
                        return Fin.Fail<Unit>(error: failure);
                    });
            }
        }),
        key: op);

    // Already on the host thread and already under `sync`: the release fold runs its rows in order and drains every
    // fault. The abort rows run on BOTH grants — a foreign meter still armed an escape subscription and a source of
    // its own, and leaking a live native escape callback past its lease outlives the fold it was cancelling.
    private Fin<Unit> Cleanup() => HostThread.Release(
        releases: grant.Switch(
            this,
            owned: static (self, owner) => Seq<Func<Fin<Unit>>>(
                () => {
                    StatusBar.HideProgressMeter(docSerialNumber: owner.Document);
                    return Fin.Succ(value: unit);
                },
                () => self.policy.Features.Contains(ProgressFeature.Taskbar)
                    ? TaskbarPulse.Apply(state: new PulseState.Idle(), key: self.op)
                    : Fin.Succ(value: unit)) + self.Disarm(),
            foreign: static (self, _) => self.Disarm()),
        key: op);

    private Seq<Func<Fin<Unit>>> Disarm() => Seq<Func<Fin<Unit>>>(
        () => op.Catch(() => Fin.Succ((escape.Iter(static row => row.Dispose()), unit).Item2)),
        () => op.Catch(() => Fin.Succ((abort.Iter(static source => source.Dispose()), unit).Item2)));

    public void Dispose() => _ = Release();

    private static Fin<(int Position, HostText Label)> Bounded(
        long position,
        HostText label,
        ProgressPolicy policy,
        Op op) =>
        position >= policy.Lower && position <= policy.Upper
            ? Fin.Succ(value: ((int)position, label))
            : Fin.Fail<(int, HostText)>(error: op.InvalidInput());

    private Fin<ProgressReceipt> Drive(DocKey document, (int Position, HostText Label) move, Op op) =>
        op.Catch(() => {
            StatusBar.UpdateProgressMeter(
                docSerialNumber: document,
                label: move.Label.Resolve(),
                position: move.Position,
                absolute: true);
            state = new(Position: move.Position, Label: move.Label, Released: false);
            ProgressReceipt receipt = Receipt(state: state);
            return Fin.Succ(value: policy.Features.Contains(ProgressFeature.Taskbar)
                ? receipt with {
                    TaskbarFault = TaskbarPulse.Apply(state: new PulseState.Working(Progress: receipt.Fraction), key: op)
                        .Match(Succ: static _ => Option<Error>.None, Fail: Some),
                }
                : receipt);
        });

    private ProgressReceipt Receipt(ProgressState state) => new(
        Grant: grant,
        Position: state.Position,
        Label: state.Label,
        Fraction: UnitInterval.Create(value: policy.Upper > policy.Lower
            ? Math.Clamp(
                value: (state.Position - (double)policy.Lower) / (policy.Upper - (double)policy.Lower),
                min: 0.0,
                max: 1.0)
            : 1.0),
        TaskbarFault: None);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Progress {
    public static Fin<T> Use<T>(DocumentSession session, ProgressPolicy policy, Func<ProgressLease, Fin<T>> body, Op? key = null) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(body);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<T>.Session(
                Document: session,
                Needs: [SessionNeed.Redraw],
                Body: _ =>
                    from grant in MeterGrant.Admit(
                        code: StatusBar.ShowProgressMeter(
                            docSerialNumber: session.Key,
                            lowerLimit: policy.Lower,
                            upperLimit: policy.Upper,
                            label: policy.Label.Resolve(),
                            embedLabel: policy.Features.Contains(ProgressFeature.EmbeddedLabel),
                            showPercentComplete: policy.Features.Contains(ProgressFeature.Percentage)),
                        document: session.Key,
                        op: op)
                    from armed in Armed(policy: policy, op: op)
                    from result in Bracketed(
                        lease: new ProgressLease(grant: grant, policy: policy, abort: armed.Abort, escape: armed.Escape, op: op),
                        wait: policy.Features.Contains(ProgressFeature.WaitCursor),
                        body: body,
                        op: op)
                    select result),
            key: op);
    }

    // Escape edges arm as a per-lease CLOSURE, never a static method group: RhinoApp.EscapeKeyPressed dedups an
    // already-present delegate, so two concurrent leases sharing one handler identity would arm ONE source and the
    // second fold would never see its own abort. Arming is transactional — a subscription that lands with no
    // source, or a source with no subscription, is an abort edge that fires into nothing or a token nothing raises.
    private static Fin<(Option<CancellationTokenSource> Abort, Option<Subscription> Escape)> Armed(ProgressPolicy policy, Op op) {
        if (!policy.Features.Contains(ProgressFeature.Escape)) { return Fin.Succ((Option<CancellationTokenSource>.None, Option<Subscription>.None)); }
        CancellationTokenSource source = new();
        EventHandler handler = (_, _) => ignore(op.Catch(() => Fin.Succ((Op.Side(source.Cancel), unit).Item2)));
        return Subscription.Attach(
                subscribe: callback => RhinoApp.EscapeKeyPressed += callback,
                unsubscribe: callback => RhinoApp.EscapeKeyPressed -= callback,
                handler: handler)
            .Map(subscription => (Some(source), Some(subscription)))
            .MapFail(failure => {
                source.Dispose();
                return failure;
            });
    }

    private static Fin<T> Bracketed<T>(ProgressLease lease, bool wait, Func<ProgressLease, Fin<T>> body, Op op) {
        Fin<T> result = op.Catch(() => {
            using WaitCursor? cursor = wait ? new WaitCursor() : null;
            return body(lease);
        });
        return lease.Release().Match(
            Succ: _ => result,
            Fail: cleanup => result.Match(
                Succ: _ => Fin.Fail<T>(error: cleanup),
                Fail: primary => Fin.Fail<T>(error: primary + cleanup)));
    }
}
```

## [05]-[WINDOWS]

- Owner: `WindowScope` selects the application or document parent, and `ShellWindows.Parent` resolves both through one entry.
- Owner: `WindowPolicy` carries native styling, localization, placement restore, and close-time persistence as behavior rows.
- Entry: `Adopt`, `Present`, `Discover`, and `Owner` remain separate because modeless ownership, modal return, typed census, and inverse document lookup carry distinct result regimes.
- Law: `Present` owns every modal modality on one name — a `Dialog<TResult>` returns its typed result, a bare `Dialog` (the themed message box and every result-on-the-instance dialog) returns `Unit` and the caller reads the instance, and a `CommonDialog` (every native-backed picker) returns its `DialogResult` verdict with the instance carrying the picked value — the input's static type discriminates, never a mode flag.
- Law: `Present` is the sole host-boundary modal presenter — an Eto `Prompt<TResult>` presents by handing `ShellWindows.Present` as its presenter seam, so raw `ShowModal` never appears at a consumer and raw `ShowDialog` appears exactly once: the `CommonDialog` arm, because a native picker publishes no semi-modal member, making `Present` its one sanctioned call site.
- Law: every document-scoped operation is a `HostWork<T>.Session` value, and every returned owner is detached as `DocKey`.
- Owner: `ShellTheme` projects the Rhino theme edge into an injected `ThemeSeam` as a `ThemeShift.Generated` polarity, routes each `ThemeChange` through the guarded callback owner, and returns a symmetric `Subscription` capsule; live host swatch ingestion is the panels `ThemePalette.Feed` seam over the same catalog.
- Boundary: `ShellTheme` observes only — theme mutation is the Persistence `AppTheme.Adopt` owner, and a shell consumer composes that owner rather than writing the host theme edge.
- Boundary: `WindowPolicy` keys persistence by the window type because Rhino owns the persisted slot identity.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowScope {
    private WindowScope() { }
    public sealed record Application : WindowScope;
    public sealed record Document(DocumentSession Session) : WindowScope;
}

[SmartEnum]
public sealed partial class WindowPolicy {
    public static readonly WindowPolicy Native = new(
        prepare: static window => {
            EtoExtensions.UseRhinoStyle(window);
            _ = EtoExtensions.RestorePosition(window, window.GetType());
            return unit;
        },
        persist: static window => Op.Side(() => EtoExtensions.SavePosition(window, window.GetType())));
    public static readonly WindowPolicy Localized = new(
        prepare: static window => Op.Side(() => EtoExtensions.LocalizeAndRestore(window, window.GetType())),
        persist: static window => Op.Side(() => EtoExtensions.SavePosition(window, window.GetType())));
    public static readonly WindowPolicy Bare = new(
        prepare: static _ => unit,
        persist: static _ => unit);

    [UseDelegateFromConstructor]
    internal partial Unit Prepare(Window window);

    [UseDelegateFromConstructor]
    internal partial Unit Persist(Window window);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ShellWindows {
    public static Fin<Window> Parent(WindowScope scope, Op? key = null) {
        ArgumentNullException.ThrowIfNull(scope);
        Op op = key.OrDefault();
        return scope.Switch(
            op,
            application: static (held, _) => HostThread.Run(
                work: new HostWork<Window>.Execute(Body: () => Optional(RhinoEtoApp.MainWindow).ToFin(Fail: held.MissingContext())),
                key: held),
            document: static (held, owned) => HostThread.Run(
                work: new HostWork<Window>.Session(
                    Document: owned.Session,
                    Needs: [SessionNeed.Read],
                    Body: document => Optional(RhinoEtoApp.MainWindowForDocument(document)).ToFin(Fail: held.MissingContext())),
                key: held));
    }

    public static Fin<Form> Adopt(Form window, DocumentSession session, WindowPolicy policy, Op? key = null) {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(policy);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Form>.Session(
                Document: session,
                Needs: [SessionNeed.Redraw],
                Body: document => {
                    string priorTitle = window.Title;
                    var priorLocation = window.Location;
                    var priorState = window.WindowState;
                    bool attached = false;
                    EventHandler<EventArgs> closed = (_, _) => ignore(op.Catch(() => {
                        _ = policy.Persist(window);
                        return Fin.Succ(value: unit);
                    }));
                    Fin<Form> adopted = op.Catch(() => {
                        _ = policy.Prepare(window);
                        window.Closed += closed;
                        attached = true;
                        EtoExtensions.Show(window, document);
                        return Fin.Succ(value: window);
                    });
                    return adopted.Match(
                        Succ: static value => Fin.Succ(value: value),
                        Fail: primary => op.Catch(() => {
                            if (attached) window.Closed -= closed;
                            window.Title = priorTitle;
                            window.Location = priorLocation;
                            window.WindowState = priorState;
                            return Fin.Succ(value: unit);
                        }).Match(
                            Succ: _ => Fin.Fail<Form>(error: primary),
                            Fail: rollback => Fin.Fail<Form>(error: primary + rollback)));
                }),
            key: op);
    }

    public static Fin<TResult> Present<TResult>(
        Dialog<TResult> dialog,
        DocumentSession session,
        Option<Control> parent = default,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(session);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<TResult>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: op.MissingContext())
                    .Map(owner => EtoExtensions.ShowSemiModal(dialog, document, owner))),
            key: op);
    }

    public static Fin<Unit> Present(Dialog dialog, DocumentSession session, Option<Control> parent = default, Op? key = null) {
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(session);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Unit>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: op.MissingContext())
                    .Map(owner => Op.Side(() => EtoExtensions.ShowSemiModal(dialog, document, owner)))),
            key: op);
    }

    public static Fin<DialogResult> Present(CommonDialog dialog, DocumentSession session, Option<Control> parent = default, Op? key = null) {
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(session);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<DialogResult>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: op.MissingContext())
                    .Map(owner => dialog.ShowDialog(owner))),
            key: op);
    }

    public static Fin<Seq<TWindow>> Discover<TWindow>(DocumentSession session, Op? key = null) where TWindow : Window {
        ArgumentNullException.ThrowIfNull(session);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<Seq<TWindow>>.Session(
                Document: session,
                Needs: [SessionNeed.Read],
                Body: document => Fin.Succ(value: toSeq(EtoExtensions.WindowsFromDocument<TWindow>(document)).Strict())),
            key: op);
    }

    public static Fin<DocKey> Owner(Form window, Op? key = null) {
        ArgumentNullException.ThrowIfNull(window);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<DocKey>.Execute(
                Body: () => Optional(EtoExtensions.GetRhinoDoc(window))
                    .ToFin(Fail: op.MissingContext())
                    .Bind(document => DocKey.Of(document: document, key: op))),
            key: op);
    }
}

public static class ShellTheme {
    // Decompile-proven: RunningInDarkMode reads AdvancedSettings.DarkMode — a managed settings read, not thread-affine
    // native UI state — so this read is safe off-thread and owes no HostThread crossing.
    public static ThemeVariant Current => HostUtils.RunningInDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;

    public static Fin<Subscription> Observe(ThemeSeam seam, CallbackObserver<ThemeChange> observer, Op? key = null) {
        ArgumentNullException.ThrowIfNull(seam);
        ArgumentNullException.ThrowIfNull(observer);
        Op op = key.OrDefault();
        EventHandler handler = (_, _) => ignore(observer.Guard(
            project: () => seam.Change(shift: new ThemeShift.Generated(Variant: Current), key: op),
            op: op));
        return Subscription.Attach(
            subscribe: callback => ThemeSettings.ThemeChanged += callback,
            unsubscribe: callback => ThemeSettings.ThemeChanged -= callback,
            handler: handler);
    }
}
```

## [06]-[RUNTIME]

- Owner: `HostProbe` closes the capability-read request family and `HostFact` its detached answers; `HostSnapshot` is the one process-and-OS record.
- Owner: `HostAssemblies` pre-admits every resolver source, reports a completed or partial applied prefix, and folds collectible loading over `AssemblyIntake` cases; a nullable host return projects to the rail at the call.
- Owner: `ScriptRun` closes the execute-request family over the host scripting engine — source, file, file-in-scope, expression, compiled — and `HostScripts` guards compile, binding custody, and dispatch; `ScriptUnit` capsules the compiled handle, `ScriptOutcome` detaches ran-versus-value evidence.
- Owner: `SkinProgram` carries the icon, product name, and one `SkinPhase` hook; `ShellSkin` adapts the complete `Skin` load-phase surface onto it.
- Law: every `ShellSkin` override chains the base member first, then routes its `SkinPhase` case; hook faults accumulate in `Faults` and never re-enter the host load sequence.
- Law: `ShellHooks.Mount` registers `rasm.rhino.hostui.skin` on the `MountRegistry` — the ask is the `Func<SkinPhase, Fin<Unit>>` phase hook, the grant is a `SkinProgram` carrying it, so a skin observer binds by point name and hands the granted program to its `ShellSkin` constructor with no second phase-delivery path.
- Law: platform capability stays behind `HostFacts` and enters through the two host locators by shape — `HostUtils.GetPlatformService<T>` resolves a typed service contract, `Rhino.UI.Runtime.PlatformServiceProvider` answers the fixed process facts it publishes directly (`ProcessArchitecture`) — and a probe is a `HostProbe` case, so a new capability read is one case and one arm.
- Law: engine presence is a probed host fact, never an assumption — `HostProbe.Scripting` answers with the `ScriptEngineSnapshot` search-path and runtime-assembly census, and every `HostScripts` entry refuses typed when `PythonScript.Create()` answers null.
- Owner: `TokenAsk` closes the accounts request family and `Accounts.Ask` dispatches it whole inside `RhinoAccountsManager.ExecuteProtectedCodeAsync`, so `SecretKey` custody is structurally confined to the protected callback; `TokenLease` holds the live token pair and hands out only detached `OpenIdEvidence`/`OauthEvidence`, with refresh and revoke consuming the lease's own held tokens.
- Law: entitlement is a capability probe, never a member reach — `HostProbe.Entitlement` answers `CloudHostUtils`'s pure platform-service reads headless, `HostProbe.Compute` answers the compute-endpoint census, and `ComputeEndpoints.Register` binds the append-only host roster under process-lifetime custody because the host publishes no unregister.
- Law: login progress crosses as detached `LoginPulse` facts keyed on the `LoginPhase` vocabulary — a raw `RhinoAccoountsProgressInfo` (the host's own doubled-o spelling) never leaves the dispatch closure, and `TryCached` reads the secure token cache with no server call and no UI, so a headless composition answers it.
- Law: script execution admits the complete `ScriptRun` text family and every binding name before engine creation or host dispatch, then rides `HostThread.Run`; an execute returning `false` projects onto the rail, expression absence rides `Option<object>`, and scripting-runtime exceptions convert inside the guarded window.
- Boundary: process facts include runtime architecture, Mono presence, and system references; assembly paths admit through `Op.AcceptText` before any resolver mutation.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostProbe {
    private HostProbe() { }
    public sealed record Process : HostProbe;
    public sealed record Printers : HostProbe;
    public sealed record Scripting : HostProbe;
    public sealed record Entitlement : HostProbe;
    public sealed record Compute : HostProbe;
}

[ComplexValueObject]
public sealed partial class HostSnapshot {
    public string ProcessName { get; }
    public Version ProcessVersion { get; }
    public string Edition { get; }
    public string Product { get; }
    public string Build { get; }
    public string Installation { get; }
    public uint Language { get; }
    public int Processors { get; }
    public bool DarkMode { get; }
    public bool Server { get; }
    public bool PreRelease { get; }
    public bool Mono { get; }
    public string Architecture { get; }
    public Seq<string> ReferenceAssemblies { get; }
    public Seq<string> SearchPaths { get; }
}

public sealed record PrintForm(string Name, Option<(double Width, double Height)> Extent);

public sealed record PrinterSlot(string Name, double HorizontalDpi, double VerticalDpi, Seq<PrintForm> Forms);

[ComplexValueObject]
public sealed partial class ScriptEngineSnapshot {
    public Seq<string> SearchPaths { get; }
    public Seq<string> RuntimeAssemblies { get; }
    public int ContextId { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostFact {
    private HostFact() { }
    public sealed record ProcessCase(HostSnapshot Snapshot) : HostFact;
    public sealed record PrinterCase(Seq<PrinterSlot> Printers) : HostFact;
    public sealed record ScriptCase(ScriptEngineSnapshot Engine) : HostFact;
    public sealed record EntitlementCase(EntitlementFact Verdict) : HostFact;
    public sealed record ComputeCase(Seq<ComputeEndpoint> Endpoints) : HostFact;
}

// CloudHostUtils is pure property reads off the ICloudHost platform service (DoNothingCloudHost when no provider
// ships), so the probe answers headless with no UI and no server call.
public sealed record EntitlementFact(bool Entitled, Option<string> DenyReason, Option<string> Signature);

public sealed record ComputeEndpoint(string Path, Type Contract);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblySource {
    private AssemblySource() { }
    public sealed record SearchFolder(string Path) : AssemblySource;
    public sealed record SearchFile(string Path) : AssemblySource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyIntake {
    private AssemblyIntake() { }
    public sealed record FromPath(string Path) : AssemblyIntake;
    public sealed record FromStream(Stream Source) : AssemblyIntake;
    public sealed record FromName(AssemblyName Name) : AssemblyIntake;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyExtensionReceipt {
    private AssemblyExtensionReceipt() { }
    public sealed record Completed(PluginKey Plugin, int Applied) : AssemblyExtensionReceipt;
    public sealed record Partial(PluginKey Plugin, int Applied, Error Fault) : AssemblyExtensionReceipt;
}

internal sealed record AssemblyExtensionState(int Applied, Option<Error> Fault);

// The unit carries its compiling engine: a code object executes only in the scope it compiled against, so a
// cross-engine run is unrepresentable rather than a silent empty-scope execution.
public sealed record ScriptUnit(PythonCompiledCode Code, PythonScript Engine);

public sealed record ScriptBinding(string Name, object Value);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptOutcome {
    private ScriptOutcome() { }
    public sealed record Ran : ScriptOutcome;
    public sealed record Value(Option<object> Result) : ScriptOutcome;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptRun {
    private ScriptRun() { }
    public sealed record Source(string Script) : ScriptRun;
    public sealed record File(string Path) : ScriptRun;
    public sealed record FileInScope(string Path) : ScriptRun;
    public sealed record Expression(string Statements, string Formula) : ScriptRun;
    public sealed record Compiled(ScriptUnit Unit) : ScriptRun;

    internal Fin<ScriptRun> Admit(Op op) => Switch(
        op,
        source: static (key, row) => key.AcceptText(value: row.Script)
            .Map<ScriptRun>(script => new Source(Script: script)),
        file: static (key, row) => key.AcceptText(value: row.Path)
            .Map<ScriptRun>(path => new File(Path: path)),
        fileInScope: static (key, row) => key.AcceptText(value: row.Path)
            .Map<ScriptRun>(path => new FileInScope(Path: path)),
        expression: static (key, row) =>
            from statements in key.AcceptText(value: row.Statements)
            from formula in key.AcceptText(value: row.Formula)
            select (ScriptRun)new Expression(Statements: statements, Formula: formula),
        compiled: static (_, row) => Fin.Succ<ScriptRun>(row));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SkinPhase {
    private SkinPhase() { }
    public sealed record MainFrameCreated : SkinPhase;
    public sealed record LicenseChecked : SkinPhase;
    public sealed record CommandsRegistered : SkinPhase;
    public sealed record PluginLoadOpened(int Expected) : SkinPhase;
    public sealed record PluginLoading(string Description) : SkinPhase;
    public sealed record PluginLoaded : SkinPhase;
    public sealed record PluginLoadClosed : SkinPhase;
    public sealed record SplashShown : SkinPhase;
    public sealed record SplashHidden : SkinPhase;
    public sealed record HelpRequested : SkinPhase;
}

public sealed record SkinProgram(Option<DrawingBitmap> Icon, Option<string> Product, Func<SkinPhase, Fin<Unit>> Phase) {
    public static readonly SkinProgram Inert = new(Icon: None, Product: None, Phase: static _ => Fin.Succ(value: unit));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public abstract class ShellSkin : Skin {
    private readonly SkinProgram program;
    private readonly Op op;
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());

    protected ShellSkin(SkinProgram program, Op? key = null) {
        ArgumentNullException.ThrowIfNull(program);
        this.program = program;
        op = key.OrDefault();
    }

    public Seq<Error> Faults => faults.Value;

    protected override DrawingBitmap MainRhinoIcon => program.Icon.Match(Some: static icon => icon, None: () => base.MainRhinoIcon);

    protected override string ApplicationName => program.Product.IfNone(() => base.ApplicationName);

    protected override void OnMainFrameWindowCreated() { base.OnMainFrameWindowCreated(); Route(phase: new SkinPhase.MainFrameCreated()); }

    protected override void OnLicenseCheckCompleted() { base.OnLicenseCheckCompleted(); Route(phase: new SkinPhase.LicenseChecked()); }

    protected override void OnBuiltInCommandsRegistered() { base.OnBuiltInCommandsRegistered(); Route(phase: new SkinPhase.CommandsRegistered()); }

    protected override void OnBeginLoadAtStartPlugIns(int expectedCount) { base.OnBeginLoadAtStartPlugIns(expectedCount); Route(phase: new SkinPhase.PluginLoadOpened(Expected: expectedCount)); }

    protected override void OnBeginLoadPlugIn(string description) { base.OnBeginLoadPlugIn(description); Route(phase: new SkinPhase.PluginLoading(Description: description)); }

    protected override void OnEndLoadPlugIn() { base.OnEndLoadPlugIn(); Route(phase: new SkinPhase.PluginLoaded()); }

    protected override void OnEndLoadAtStartPlugIns() { base.OnEndLoadAtStartPlugIns(); Route(phase: new SkinPhase.PluginLoadClosed()); }

    protected override void ShowSplash() { base.ShowSplash(); Route(phase: new SkinPhase.SplashShown()); }

    protected override void HideSplash() { base.HideSplash(); Route(phase: new SkinPhase.SplashHidden()); }

    protected override void ShowHelp() { base.ShowHelp(); Route(phase: new SkinPhase.HelpRequested()); }

    private void Route(SkinPhase phase) => ignore(op.Catch(() => program.Phase(phase))
        .IfFail(failure => { _ = faults.Swap(rows => rows.Add(failure)); return unit; }));
}

public static class ShellHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        return MountRegistry.Mount(
            mount: new HookMount(
                Point: RhinoPoint.HostUiSkin,
                Plugin: plugin,
                Ask: typeof(Func<SkinPhase, Fin<Unit>>),
                Grant: typeof(SkinProgram),
                Bind: ask => Optional(ask as Func<SkinPhase, Fin<Unit>>)
                    .ToFin(Fail: op.InvalidInput())
                    .Map(static phase => (object)(SkinProgram.Inert with { Phase = phase }))),
            key: op);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostFacts {
    public static Fin<HostFact> Probe(HostProbe probe, Op? key = null) {
        ArgumentNullException.ThrowIfNull(probe);
        Op op = key.OrDefault();
        return probe.Switch(
            op,
            process: static (held, _) => held.Catch(() => {
                HostUtils.GetCurrentProcessInfo(processName: out string name, processVersion: out Version version);
                return Fin.Succ<HostFact>(value: new HostFact.ProcessCase(Snapshot: HostSnapshot.Create(
                    processName: name,
                    processVersion: version,
                    edition: HostUtils.OperatingSystemEdition,
                    product: HostUtils.OperatingSystemProductName,
                    build: HostUtils.OperatingSystemBuildNumber,
                    installation: HostUtils.OperatingSystemInstallationType,
                    language: HostUtils.CurrentOSLanguage,
                    processors: HostUtils.GetSystemProcessorCount(),
                    darkMode: HostUtils.RunningInDarkMode,
                    server: HostUtils.RunningOnServer,
                    preRelease: HostUtils.IsPreRelease,
                    mono: HostUtils.RunningInMono,
                    architecture: PlatformServiceProvider.ProcessArchitecture,
                    referenceAssemblies: toSeq(HostUtils.GetSystemReferenceAssemblies()).Strict(),
                    searchPaths: toSeq(HostUtils.GetAssemblySearchPaths()).Strict())));
            }),
            printers: static (held, _) => held.Catch(() => Fin.Succ<HostFact>(value: new HostFact.PrinterCase(
                Printers: toSeq(HostUtils.GetPrinterNames()).Map(printer => new PrinterSlot(
                    Name: printer,
                    HorizontalDpi: HostUtils.GetPrinterDPI(printerName: printer, horizontal: true),
                    VerticalDpi: HostUtils.GetPrinterDPI(printerName: printer, horizontal: false),
                    Forms: toSeq(HostUtils.GetPrinterFormNames(printerName: printer)).Map(form => new PrintForm(
                        Name: form,
                        Extent: HostUtils.GetPrinterFormSize(printer, form, out double width, out double height)
                            ? Some((width, height))
                            : None)).Strict())).Strict()))),
            entitlement: static (held, _) => held.Catch(() => Fin.Succ<HostFact>(value: new HostFact.EntitlementCase(
                Verdict: new EntitlementFact(
                    Entitled: CloudHostUtils.IsEntitled,
                    DenyReason: Op.Text(CloudHostUtils.DenyReason),
                    Signature: Op.Text(CloudHostUtils.Signature))))),
            compute: static (held, _) => held.Catch(() => Fin.Succ<HostFact>(value: new HostFact.ComputeCase(
                Endpoints: toSeq(HostUtils.GetCustomComputeEndpoints())
                    .Map(static row => new ComputeEndpoint(Path: row.Item1, Contract: row.Item2))
                    .Strict()))),
            scripting: static (held, _) => held.Catch(() => Optional(PythonScript.Create())
                .ToFin(Fail: held.InvalidResult())
                .Bind(engine => held.Catch(() => Fin.Succ<HostFact>(value: new HostFact.ScriptCase(
                    Engine: ScriptEngineSnapshot.Create(
                        searchPaths: toSeq(PythonScript.SearchPaths).Strict(),
                        runtimeAssemblies: toSeq(PythonScript.RuntimeAssemblies())
                            .Map(static assembly => assembly.FullName ?? string.Empty).Strict(),
                        contextId: engine.ContextId)))))));
    }
}

public static class HostAssemblies {
    // Resolver extension is process-permanent: the host publishes no removal, so the receipt attributes every
    // applied row to the extending plugin and an applied prefix is never rolled back — custody is the
    // SnapshotParticipant permanence class, stated rather than hidden.
    public static Fin<AssemblyExtensionReceipt> Extend(PluginKey plugin, Seq<AssemblySource> sources, Op? key = null) {
        Op op = key.OrDefault();
        return
            from admitted in sources.TraverseM(source => Optional(source)
                    .ToFin(Fail: op.InvalidInput())
                    .Bind(value => value.Switch(
                        op,
                        searchFolder: static (held, row) => held.AcceptText(value: row.Path)
                            .Map<AssemblySource>(path => new AssemblySource.SearchFolder(Path: path)),
                        searchFile: static (held, row) => held.AcceptText(value: row.Path)
                            .Map<AssemblySource>(path => new AssemblySource.SearchFile(Path: path)))))
                .As()
            from receipt in HostThread.Run(
                work: new HostWork<AssemblyExtensionReceipt>.Execute(Body: () => {
                    AssemblyExtensionState state = admitted.Fold(
                        new AssemblyExtensionState(Applied: 0, Fault: None),
                        (held, source) => held.Fault.IsSome
                            ? held
                            : op.Catch(() => Fin.Succ(value: source.Switch(
                                    searchFolder: static row => Op.Side(() => AssemblyResolver.AddSearchFolder(folder: row.Path)),
                                    searchFile: static row => Op.Side(() => AssemblyResolver.AddSearchFile(file: row.Path)))))
                                .Match(
                                    Succ: static _ => held with { Applied = held.Applied + 1 },
                                    Fail: fault => held with { Fault = Some(fault) }));
                    return Fin.Succ(value: state.Fault.Match<AssemblyExtensionReceipt>(
                        Some: fault => new AssemblyExtensionReceipt.Partial(Plugin: plugin, Applied: state.Applied, Fault: fault),
                        None: () => new AssemblyExtensionReceipt.Completed(Plugin: plugin, Applied: state.Applied)));
                }),
                key: op)
            select receipt;
    }

    public static Fin<Assembly> Load(AssemblyIntake intake, Op? key = null) {
        ArgumentNullException.ThrowIfNull(intake);
        Op op = key.OrDefault();
        return intake.Switch(
            op,
            fromPath: static (held, row) => held.AcceptText(value: row.Path)
                .Bind(path => held.Catch(() => Optional(HostUtils.LoadAssemblyFrom(path: path)).ToFin(Fail: held.InvalidResult()))),
            fromStream: static (held, row) => held.Catch(() =>
                Optional(HostUtils.LoadAssemblyFromStream(stream: row.Source)).ToFin(Fail: held.InvalidResult())),
            fromName: static (held, row) => held.Catch(() =>
                Optional(HostUtils.LoadAssemblyFromName(assemblyName: row.Name)).ToFin(Fail: held.InvalidResult())));
    }
}

public static class HostScripts {
    public static Fin<ScriptUnit> Compile(string script, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptText(value: script)
            .Bind(source => Engine(op).Bind(engine => op.Catch(() =>
                Optional(engine.Compile(script: source))
                    .ToFin(Fail: op.InvalidResult())
                    .Map(code => new ScriptUnit(Code: code, Engine: engine)))));
    }

    public static Fin<ScriptOutcome> Run(ScriptRun run, Seq<ScriptBinding> bindings = default, Op? key = null) {
        ArgumentNullException.ThrowIfNull(run);
        Op op = key.OrDefault();
        return from admitted in run.Admit(op)
               from prepared in bindings.TraverseM(binding =>
                       from row in op.Need(binding)
                       from name in op.AcceptText(value: row.Name)
                       select row with { Name = name })
                   .As()
               // A compiled unit executes in the engine that compiled it; every other case mints one fresh engine
               // and both the bindings and the dispatch read the same instance.
               from engine in admitted is ScriptRun.Compiled held
                   ? Fin.Succ(value: held.Unit.Engine)
                   : Engine(op)
               from outcome in HostThread.Run(
            work: new HostWork<ScriptOutcome>.Execute(Body: () =>
                prepared.TraverseM(binding => op.Catch(() =>
                        Fin.Succ(value: Op.Side(() => engine.SetVariable(name: binding.Name, value: binding.Value)))))
                    .As()
                    .Bind(_ => admitted.Switch(
                        (Held: op, Engine: engine),
                        source: static (state, row) => state.Held.Catch(() => state.Engine.ExecuteScript(script: row.Script)
                            ? Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran())
                            : Fin.Fail<ScriptOutcome>(error: state.Held.InvalidResult())),
                        file: static (state, row) => state.Held.Catch(() => state.Engine.ExecuteFile(path: row.Path)
                                ? Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran())
                                : Fin.Fail<ScriptOutcome>(error: state.Held.InvalidResult())),
                        fileInScope: static (state, row) => state.Held.Catch(() => state.Engine.ExecuteFileInScope(path: row.Path)
                                ? Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran())
                                : Fin.Fail<ScriptOutcome>(error: state.Held.InvalidResult())),
                        expression: static (state, row) => state.Held.Catch(() => Fin.Succ<ScriptOutcome>(
                            value: new ScriptOutcome.Value(Result: Optional(state.Engine.EvaluateExpression(
                                statements: row.Statements, expression: row.Formula))))),
                        compiled: static (state, row) => state.Held.Catch(() => {
                            row.Unit.Code.Execute(scope: state.Engine);
                            return Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran());
                        })))),
            key: op)
               select outcome;
    }

    static Fin<PythonScript> Engine(Op op) =>
        op.Catch(() => Optional(PythonScript.Create()).ToFin(Fail: op.InvalidResult()));
}

// --- [ACCOUNTS_RAIL]
[SmartEnum<ProgressState>]
public sealed partial class LoginPhase {
    public static readonly LoginPhase AwaitingLogin = new(key: ProgressState.AwaitingLogin);
    public static readonly LoginPhase RetrievingTokens = new(key: ProgressState.RetrievingTokens);
    public static readonly LoginPhase AwaitingRedirect = new(key: ProgressState.AwaitingRedirect);
    public static readonly LoginPhase Other = new(key: ProgressState.Other);
}

public sealed record LoginPulse(LoginPhase Phase, Option<string> Description);

// Detached claim/expiry evidence — the live token interfaces never leave the lease.
public sealed record OpenIdEvidence(
    string Subject,
    string Issuer,
    string Audience,
    Option<DateTime> Issued,
    Option<DateTime> Expires,
    Seq<string> Emails,
    Option<bool> EmailVerified,
    Option<string> Name);

public sealed record OauthEvidence(Option<DateTime> Expires, Seq<string> Scopes, bool Expired);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TokenAsk {
    private TokenAsk() { }
    public sealed record Acquire(string ClientId, string ClientSecret) : TokenAsk;
    public sealed record AcquireScoped(
        string ClientId, string ClientSecret, Seq<string> Scopes, Option<string> Prompt, Option<int> MaxAge) : TokenAsk;
    public sealed record TryCached(string ClientId, Seq<string> Scopes) : TokenAsk;

    internal Fin<TokenAsk> Admit(Op op) => Switch(
        op,
        acquire: static (key, row) =>
            from id in key.AcceptText(value: row.ClientId)
            from secret in key.AcceptText(value: row.ClientSecret)
            select (TokenAsk)new Acquire(ClientId: id, ClientSecret: secret),
        acquireScoped: static (key, row) =>
            from id in key.AcceptText(value: row.ClientId)
            from secret in key.AcceptText(value: row.ClientSecret)
            from scopes in row.Scopes.TraverseM(scope => key.AcceptText(value: scope)).As()
            select (TokenAsk)new AcquireScoped(
                ClientId: id, ClientSecret: secret, Scopes: scopes.Strict(), Prompt: row.Prompt, MaxAge: row.MaxAge),
        tryCached: static (key, row) =>
            from id in key.AcceptText(value: row.ClientId)
            from scopes in row.Scopes.TraverseM(scope => key.AcceptText(value: scope)).As()
            select (TokenAsk)new TryCached(ClientId: id, Scopes: scopes.Strict()));
}

// --- [SERVICES]
// The lease confines the live token pair: every dispatch runs INSIDE ExecuteProtectedCode/Async, so SecretKey
// never escapes the callback and no consumer touches the accounts namespace. Refresh and revoke take the lease's
// own held tokens — a detached evidence record cannot reconstruct them, which is the confinement working.
public sealed class TokenLease : IDisposable {
    private readonly Atom<Option<(IOpenIDConnectToken OpenId, IOAuth2Token Oauth)>> held;
    private readonly string clientId;
    private readonly Op op;

    internal TokenLease(IOpenIDConnectToken openId, IOAuth2Token oauth, string clientId, Op op) {
        held = Atom(Some((openId, oauth)));
        this.clientId = clientId;
        this.op = op;
    }

    public Fin<OpenIdEvidence> OpenId(Op? key = null) => Read(
        project: static pair => new OpenIdEvidence(
            Subject: pair.OpenId.Sub,
            Issuer: pair.OpenId.Iss,
            Audience: pair.OpenId.Aud,
            Issued: Optional(pair.OpenId.Iat),
            Expires: Optional(pair.OpenId.Exp),
            Emails: toSeq(pair.OpenId.Emails).Strict(),
            EmailVerified: Optional(pair.OpenId.EmailVerified),
            Name: Op.Text(pair.OpenId.Name)),
        key: key);

    public Fin<OauthEvidence> Oauth(Op? key = null) => Read(
        project: static pair => new OauthEvidence(
            Expires: Optional(pair.Oauth.Exp),
            Scopes: toSeq(pair.Oauth.Scope).Strict(),
            Expired: pair.Oauth.IsExpired),
        key: key);

    // A lease past expiry answers dead rather than handing a stale credential.
    public bool Live => held.Value.Map(static pair => !pair.Oauth.IsExpired).IfNone(false);

    public Fin<Unit> Refresh(Op? key = null) {
        Op admitted = key.OrDefault();
        return held.Value.ToFin(Fail: admitted.MissingContext()).Bind(pair => admitted.Catch(() => {
            Task task = RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: async secret => {
                IOpenIDConnectToken updated = await RhinoAccountsManager.UpdateOpenIDConnectTokenAsync(
                    currentToken: pair.OpenId, oauth2Token: pair.Oauth, secretKey: secret, cancellationToken: CancellationToken.None);
                _ = held.Swap(current => current.Map(row => (updated, row.Oauth)));
            });
            task.Wait();
            return Fin.Succ(value: unit);
        }));
    }

    public Fin<Unit> Revoke(Op? key = null) {
        Op admitted = key.OrDefault();
        return held.Value.ToFin(Fail: admitted.MissingContext()).Bind(pair => admitted.Catch(() => {
            Task task = RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: secret =>
                RhinoAccountsManager.RevokeAuthTokenAsync(oauth2Token: pair.Oauth, secretKey: secret, cancellationToken: CancellationToken.None));
            task.Wait();
            _ = held.Swap(static _ => Option<(IOpenIDConnectToken, IOAuth2Token)>.None);
            return Fin.Succ(value: unit);
        }));
    }

    public void Dispose() => ignore(held.Swap(static _ => Option<(IOpenIDConnectToken, IOAuth2Token)>.None));

    private Fin<T> Read<T>(Func<(IOpenIDConnectToken OpenId, IOAuth2Token Oauth), T> project, Op? key = null) {
        Op admitted = key.OrDefault();
        return held.Value.ToFin(Fail: admitted.MissingContext()).Bind(pair => admitted.Catch(() => Fin.Succ(value: project(pair))));
    }
}

// --- [OPERATIONS]
public static class Accounts {
    // Interactive login confines to first acquisition; TryCached reads the secure token cache with no server call
    // and no UI, so a headless process answers it. showUI stays false on the scoped overload — the host raises its
    // own browser flow off the progress callback, and the caller observes it as detached LoginPulse facts.
    public static Fin<TokenLease> Ask(TokenAsk ask, Option<Action<LoginPulse>> progress = default, Option<Env> env = default, Op? key = null) {
        ArgumentNullException.ThrowIfNull(ask);
        Op op = key.OrDefault();
        return from admitted in ask.Admit(op)
               from cancel in Fin.Succ(value: env.Map(static held => held.Cancellation).IfNone(CancellationToken.None))
               from pulse in Fin.Succ<IProgress<RhinoAccoountsProgressInfo>>(value: new Progress<RhinoAccoountsProgressInfo>(info =>
                   progress.Iter(tap => ignore(op.Catch(() => Fin.Succ(value: Op.Side(() => tap(new LoginPulse(
                       Phase: LoginPhase.TryGet(info.State, out LoginPhase? phase) && phase is { } row ? row : LoginPhase.Other,
                       Description: Op.Text(info.Description))))))))))
               from lease in op.Catch(() => {
                   Tuple<IOpenIDConnectToken, IOAuth2Token>? pair = null;
                   Task task = RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: async secret => pair = await admitted.Switch(
                       (Secret: secret, Cancel: cancel, Pulse: pulse),
                       acquire: static (held, row) => RhinoAccountsManager.GetAuthTokensAsync(
                           clientId: row.ClientId, clientSecret: row.ClientSecret, secretKey: held.Secret, cancellationToken: held.Cancel),
                       acquireScoped: static (held, row) => RhinoAccountsManager.GetAuthTokensAsync(
                           clientId: row.ClientId, clientSecret: row.ClientSecret, scope: row.Scopes.AsEnumerable(),
                           prompt: row.Prompt.IfNone((string?)null)!, maxAge: row.MaxAge.Map(static age => (int?)age).IfNone((int?)null),
                           showUI: false, progress: held.Pulse, secretKey: held.Secret, cancellationToken: held.Cancel),
                       tryCached: static (held, row) => Task.FromResult(row.Scopes.IsEmpty
                           ? RhinoAccountsManager.TryGetAuthTokens(clientId: row.ClientId, secretKey: held.Secret)
                           : RhinoAccountsManager.TryGetAuthTokens(clientId: row.ClientId, scope: row.Scopes.AsEnumerable(), secretKey: held.Secret))));
                   task.Wait();
                   return Optional(pair)
                       .Filter(static row => row.Item1 is not null && row.Item2 is not null)
                       .ToFin(Fail: op.MissingContext())
                       .Map(row => new TokenLease(openId: row.Item1, oauth: row.Item2, clientId: admitted.Switch(
                           acquire: static held => held.ClientId,
                           acquireScoped: static held => held.ClientId,
                           tryCached: static held => held.ClientId), op: op));
               })
               select lease;
    }
}

// --- [COMPUTE_ENDPOINTS]
// Registration binds an (endpointPath, Type) pair onto an append-only host roster — no delegate, no unregister,
// no invocation surface in RhinoCommon (routing and activation live server-side in Rhino.Compute) — so the
// register receipt carries no Subscription: process-lifetime custody, the SnapshotParticipant precedent.
public static class ComputeEndpoints {
    public static Fin<ComputeEndpoint> Register(string path, Type contract, Op? key = null) {
        ArgumentNullException.ThrowIfNull(contract);
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: path)
               from row in op.Catch(() => {
                   HostUtils.RegisterComputeEndpoint(endpointPath: admitted, t: contract);
                   return Fin.Succ(value: new ComputeEndpoint(Path: admitted, Contract: contract));
               })
               select row;
    }
}
```

## [07]-[CALLBACKS]

- Owner: `NamedValue` closes the typed-parameter vocabulary, `NamedKind` rows carry read dispatch, and `NamedBag` serializes native common objects into detached payloads before they enter the map.
- Entry: `NamedCallbacks.Register` seats one host callback under a wire name; `NamedCallbacks.Execute` mints, executes, and detaches the response in one crossing.
- Law: wire names are plugin-claimed custody — `HostUtils.RegisterNamedCallback` silently replaces a prior handler, so `Register` claims the name in the process registry under a fresh claim token keyed on `PluginKey` before the host call; ANY registration against a live claim faults typed — foreign or same-plugin alike, because a silent replacement would leave the prior `Subscription`'s detach removing the new host row — and detach releases exactly its own claim with the host row.
- Law: `NamedSlot.Admit` revalidates one complete schema before native arguments exist; a callback handler detaches the request, runs the typed body, and writes the reply into the live dictionary before returning.
- Law: execution cancellation reads the kernel `Env`, never an ambient token; a cancelled execution is a typed `UiFault.Cancelled`, never a swallowed skip.
- Boundary: geometry, viewport, and meshing rows cross as serialized values; `NamedLease` owns every rehydrated common object until the synchronous host call ends, and the read-only viewport row refuses `Write`.
- Owner: `NodeFunctions` resolves the node-in-code component table into detached `NodeFunction` descriptors; `Call` is the one invocation entry, always through the warning-capturing host `Evaluate`, with `NodeCallShape` closing the flatten-versus-tree modality as a row, never a flag pair.
- Law: a `NodeFunction` detaches name, namespace, description, component id, and the input/output rosters at resolution; the live `ComponentFunctionInfo` stays private, and every invocation returns a `NodeReturn` carrying values AND warnings — the warning-silencing host variants are the discarded-evidence forms this surface never spells.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedValue {
    private NamedValue() { }
    public sealed record Text(string Value) : NamedValue;
    public sealed record TextSet(Seq<string> Values) : NamedValue;
    public sealed record Flag(bool Value) : NamedValue;
    public sealed record Number(int Value) : NamedValue;
    public sealed record Count(uint Value) : NamedValue;
    public sealed record CountSet(Seq<uint> Values) : NamedValue;
    public sealed record Scalar(double Value) : NamedValue;
    public sealed record Id(Guid Value) : NamedValue;
    public sealed record IdSet(Seq<Guid> Values) : NamedValue;
    public sealed record Paint(DrawingColor Value) : NamedValue;
    public sealed record Cell(DrawingPoint Value) : NamedValue;
    public sealed record Point(Point3d Value) : NamedValue;
    public sealed record Vector(Vector3d Value) : NamedValue;
    public sealed record Segment(Line Value) : NamedValue;
    public sealed record Sweep(Arc Value) : NamedValue;
    public sealed record Frame(Plane Value) : NamedValue;
    public sealed record PointSet(Seq<Point3d> Values) : NamedValue;
    public sealed record Geometry(Seq<string> Values) : NamedValue;
    public sealed record Camera(string Value) : NamedValue;
    public sealed record Meshing(string Value) : NamedValue;

    internal Fin<Seq<Func<Fin<Unit>>>> Write(NamedParametersEventArgs args, string key, Op op) => Switch(
        (Args: args, Key: key, Op: op),
        text: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        textSet: static (held, row) => Written(() => held.Args.Set(held.Key, row.Values.AsEnumerable()), held.Op),
        flag: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        number: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        count: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        countSet: static (held, row) => Written(() => held.Args.Set(held.Key, row.Values.AsEnumerable()), held.Op),
        scalar: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        id: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        idSet: static (held, row) => Written(() => held.Args.Set(held.Key, row.Values.AsEnumerable()), held.Op),
        paint: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        cell: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        point: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        vector: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        segment: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        sweep: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        frame: static (held, row) => Written(() => held.Args.Set(held.Key, row.Value), held.Op),
        pointSet: static (held, row) => Written(() => held.Args.Set(held.Key, [.. row.Values]), held.Op),
        geometry: static (held, row) =>
            from values in Decode<GeometryBase>(
                encoded: row.Values,
                decode: static value => CommonObject.FromJSON(value) as GeometryBase,
                op: held.Op)
            from releases in Transfer(
                values: values,
                write: () => held.Args.Set(held.Key, values.AsEnumerable()),
                op: held.Op)
            select releases,
        camera: static (held, _) => Fin.Fail<Seq<Func<Fin<Unit>>>>(error: held.Op.Unsupported()),
        meshing: static (held, row) =>
            from values in Decode<MeshingParameters>(
                encoded: Seq(row.Value),
                decode: MeshingParameters.FromEncodedString,
                op: held.Op)
            from value in values.Head.ToFin(Fail: held.Op.InvalidResult())
            from releases in Transfer(
                values: values,
                write: () => held.Args.Set(held.Key, value),
                op: held.Op)
            select releases);

    private static Fin<Seq<Func<Fin<Unit>>>> Written(Action write, Op op) => op.Catch(() => {
        write();
        return Fin.Succ(value: Seq<Func<Fin<Unit>>>());
    });

    private static Fin<Seq<T>> Decode<T>(Seq<string> encoded, Func<string, T?> decode, Op op) where T : class, IDisposable {
        (Seq<T> Values, Option<Error> Fault) state = encoded.Fold(
            (Values: Seq<T>(), Fault: Option<Error>.None),
            (held, source) => held.Fault.IsSome
                ? held
                : op.Catch(() => Optional(decode(source)).ToFin(Fail: op.InvalidResult())).Match(
                    Succ: value => (held.Values.Add(value), Option<Error>.None),
                    Fail: fault => (held.Values, Some(fault))));
        return state.Fault.Match(
            Some: fault => HostThread.Release(
                    releases: state.Values.Rev().Map(value => (Func<Fin<Unit>>)(() => {
                        value.Dispose();
                        return Fin.Succ(value: unit);
                    })),
                    key: op)
                .Match(
                    Succ: _ => Fin.Fail<Seq<T>>(error: fault),
                    Fail: release => Fin.Fail<Seq<T>>(error: fault + release)),
            None: () => Fin.Succ(value: state.Values));
    }

    private static Fin<Seq<Func<Fin<Unit>>>> Transfer<T>(Seq<T> values, Action write, Op op) where T : IDisposable {
        Seq<Func<Fin<Unit>>> releases = values.Rev().Map(value => (Func<Fin<Unit>>)(() => {
            value.Dispose();
            return Fin.Succ(value: unit);
        }));
        return op.Catch(() => {
            write();
            return Fin.Succ(value: releases);
        }).MapFail(fault => HostThread.Release(releases: releases, key: op).Match(
            Succ: _ => fault,
            Fail: release => fault + release));
    }
}

[SmartEnum<int>]
public sealed partial class NamedKind {
    public static readonly NamedKind Text = new(0, read: static (args, key) =>
        args.TryGetString(key, out string value) ? Some<NamedValue>(new NamedValue.Text(Value: value)) : None);
    public static readonly NamedKind TextSet = new(1, read: static (args, key) =>
        args.TryGetStrings(key, out string[] values) ? Some<NamedValue>(new NamedValue.TextSet(Values: toSeq(values).Strict())) : None);
    public static readonly NamedKind Flag = new(2, read: static (args, key) =>
        args.TryGetBool(key, out bool value) ? Some<NamedValue>(new NamedValue.Flag(Value: value)) : None);
    public static readonly NamedKind Number = new(3, read: static (args, key) =>
        args.TryGetInt(key, out int value) ? Some<NamedValue>(new NamedValue.Number(Value: value)) : None);
    public static readonly NamedKind Count = new(4, read: static (args, key) =>
        args.TryGetUnsignedInt(key, out uint value) ? Some<NamedValue>(new NamedValue.Count(Value: value)) : None);
    public static readonly NamedKind CountSet = new(5, read: static (args, key) =>
        args.TryGetUints(key, out uint[] values) ? Some<NamedValue>(new NamedValue.CountSet(Values: toSeq(values).Strict())) : None);
    public static readonly NamedKind Scalar = new(6, read: static (args, key) =>
        args.TryGetDouble(key, out double value) ? Some<NamedValue>(new NamedValue.Scalar(Value: value)) : None);
    public static readonly NamedKind Id = new(7, read: static (args, key) =>
        args.TryGetGuid(key, out Guid value) ? Some<NamedValue>(new NamedValue.Id(Value: value)) : None);
    public static readonly NamedKind IdSet = new(8, read: static (args, key) =>
        args.TryGetGuids(key, out Guid[] values) ? Some<NamedValue>(new NamedValue.IdSet(Values: toSeq(values).Strict())) : None);
    public static readonly NamedKind Paint = new(9, read: static (args, key) =>
        args.TryGetColor(key, out DrawingColor value) ? Some<NamedValue>(new NamedValue.Paint(Value: value)) : None);
    public static readonly NamedKind Cell = new(10, read: static (args, key) =>
        args.TryGetPoint2i(key, out DrawingPoint value) ? Some<NamedValue>(new NamedValue.Cell(Value: value)) : None);
    public static readonly NamedKind Point = new(11, read: static (args, key) =>
        args.TryGetPoint(key, out Point3d value) ? Some<NamedValue>(new NamedValue.Point(Value: value)) : None);
    public static readonly NamedKind Vector = new(12, read: static (args, key) =>
        args.TryGetVector(key, out Vector3d value) ? Some<NamedValue>(new NamedValue.Vector(Value: value)) : None);
    public static readonly NamedKind Segment = new(13, read: static (args, key) =>
        args.TryGetLine(key, out Line value) ? Some<NamedValue>(new NamedValue.Segment(Value: value)) : None);
    public static readonly NamedKind Sweep = new(14, read: static (args, key) =>
        args.TryGetArc(key, out Arc value) ? Some<NamedValue>(new NamedValue.Sweep(Value: value)) : None);
    public static readonly NamedKind Frame = new(15, read: static (args, key) =>
        args.TryGetPlane(key, out Plane value) ? Some<NamedValue>(new NamedValue.Frame(Value: value)) : None);
    public static readonly NamedKind PointSet = new(16, read: static (args, key) =>
        args.TryGetPoints(key, out Point3d[] values) ? Some<NamedValue>(new NamedValue.PointSet(Values: toSeq(values).Strict())) : None);
    public static readonly NamedKind Geometry = new(17, read: static (args, key) =>
        args.TryGetGeometry(key, out GeometryBase[] values) ? Some<NamedValue>(new NamedValue.Geometry(
            Values: toSeq(values).Map(static value => value.ToJSON(new SerializationOptions())).Strict())) : None);
    public static readonly NamedKind Camera = new(18, read: static (args, key) =>
        args.TryGetViewport(key, out ViewportInfo viewport) ? Some<NamedValue>(new NamedValue.Camera(
            Value: viewport.ToJSON(new SerializationOptions()))) : None);
    public static readonly NamedKind Meshing = new(19, read: static (args, key) =>
        args.TryGetMeshParameters(key, out MeshingParameters value) ? Some<NamedValue>(new NamedValue.Meshing(
            Value: value.ToEncodedString())) : None);

    [UseDelegateFromConstructor]
    internal partial Option<NamedValue> Read(NamedParametersEventArgs args, string key);
}

[SmartEnum<bool>]
public sealed partial class SlotPresence {
    public static readonly SlotPresence Optional = new(false);
    public static readonly SlotPresence Required = new(true);
}

[ComplexValueObject]
public sealed partial class NamedSlot {
    public string Key { get; }
    public NamedKind Kind { get; }
    public SlotPresence Presence { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref NamedKind kind,
        ref SlotPresence presence) =>
        validationError = string.IsNullOrWhiteSpace(key) || kind is null || presence is null
            ? new ValidationError(message: "Named slot is invalid.")
            : null;

    internal static Fin<Seq<NamedSlot>> Admit(Seq<NamedSlot> slots, Op op) =>
        from admitted in slots.TraverseM(slot => op.Need(slot).Bind(row =>
                TryCreate(key: row.Key, kind: row.Kind, presence: row.Presence, out NamedSlot? validated) && validated is { } value
                    ? Fin.Succ(value: value)
                    : Fin.Fail<NamedSlot>(error: op.InvalidInput())))
            .As()
        let schema = admitted.Strict()
        from _ in guard(
            flag: schema.Map(static slot => slot.Key).Distinct().Count == schema.Count,
            False: op.InvalidInput()).ToFin()
        select schema;
}

// --- [MODELS] -------------------------------------------------------------------------------
internal sealed record NamedWriteState(Seq<Func<Fin<Unit>>> Releases, Option<Error> Fault);

internal sealed class NamedLease {
    private readonly Seq<Func<Fin<Unit>>> releases;
    private int released;

    internal NamedLease(Seq<Func<Fin<Unit>>> releases) => this.releases = releases;

    internal NamedLease Append(Func<Fin<Unit>> release) => new(releases: releases.Add(release));

    internal Fin<T> Within<T>(Func<Fin<T>> body, Op op) {
        Fin<T> result = op.Catch(body);
        return Release(op).Match(
            Succ: _ => result,
            Fail: release => result.Match(
                Succ: _ => Fin.Fail<T>(error: release),
                Fail: primary => Fin.Fail<T>(error: primary + release)));
    }

    private Fin<Unit> Release(Op op) => Interlocked.Exchange(location1: ref released, value: 1) is 0
        ? HostThread.Release(releases: releases, key: op)
        : Fin.Succ(value: unit);
}

internal sealed record NamedPacket(NamedParametersEventArgs Args, NamedLease Lease) {
    internal Fin<T> Within<T>(Func<NamedParametersEventArgs, Fin<T>> body, Op op) =>
        Lease.Within(body: () => body(Args), op: op);
}

public sealed record NamedBag {
    private NamedBag(HashMap<string, NamedValue> rows) => Rows = rows;

    public static readonly NamedBag Empty = new(rows: HashMap<string, NamedValue>());

    public HashMap<string, NamedValue> Rows { get; }

    public Fin<NamedBag> Put(string name, NamedValue value, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: name)
               from payload in op.Need(value)
               from _ in guard(flag: Rows.Find(admitted).IsNone, False: op.InvalidInput()).ToFin()
               select new NamedBag(rows: Rows.Add(admitted, payload));
    }

    public NamedBag Remove(string key) => new(rows: Rows.Remove(key));

    public Option<NamedValue> Find(string key) => Rows.Find(key);

    internal Fin<NamedLease> WriteInto(NamedParametersEventArgs args, Op op) {
        NamedWriteState state = toSeq(Rows.AsIterable()).Fold(
            new NamedWriteState(Releases: Seq<Func<Fin<Unit>>>(), Fault: None),
            (held, row) => held.Fault.IsSome
                ? held
                : row.Value.Write(args: args, key: row.Key, op: op).Match(
                    Succ: releases => held with { Releases = releases + held.Releases },
                    Fail: fault => held with { Fault = Some(fault) }));
        return state.Fault.Match(
            Some: fault => HostThread.Release(releases: state.Releases, key: op).Match(
                Succ: static _ => Fin.Fail<NamedLease>(error: fault),
                Fail: release => Fin.Fail<NamedLease>(error: fault + release)),
            None: () => Fin.Succ(value: new NamedLease(releases: state.Releases)));
    }

    internal Fin<NamedPacket> Mint(Op op) {
        NamedParametersEventArgs args = new();
        return WriteInto(args: args, op: op)
            .Map(values => new NamedPacket(
                Args: args,
                Lease: values.Append(release: () => {
                    args.Dispose();
                    return Fin.Succ(value: unit);
                })))
            .MapFail(fault => (fun(args.Dispose)(), fault).Item2);
    }

    internal static Fin<NamedBag> Detach(NamedParametersEventArgs args, Seq<NamedSlot> slots, Op op) =>
        from rows in slots.TraverseM(slot => op.Catch(() => slot.Kind.Read(args: args, key: slot.Key).Match(
                Some: value => Fin.Succ(value: Some((slot.Key, value))),
                None: () => slot.Presence.Key
                    ? Fin.Fail<Option<(string, NamedValue)>>(error: op.InvalidResult(detail: slot.Key))
                    : Fin.Succ(value: Option<(string, NamedValue)>.None))))
            .As()
        select new NamedBag(rows: toHashMap(rows.Choose(static row => row)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class NamedCallbacks {
    private static readonly Atom<HashMap<string, (PluginKey Plugin, Guid Claim)>> Names = Atom(HashMap<string, (PluginKey Plugin, Guid Claim)>());

    public static Fin<Subscription> Register(
        PluginKey plugin,
        string name,
        Seq<NamedSlot> request,
        Func<NamedBag, Fin<NamedBag>> body,
        Action<Error> report,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(report);
        Op op = key.OrDefault();
        // The claim is a fresh token, not the plugin id alone: a same-plugin double registration refuses typed
        // instead of silently replacing the host handler — whose prior Subscription's detach would then remove the
        // NEW registration — so exactly one live claim maps to exactly one host row at all times.
        Guid claimId = Guid.NewGuid();
        return from admitted in op.AcceptText(value: name)
               from schema in NamedSlot.Admit(slots: request, op: op)
               from claim in Names.Swap(held => held.ContainsKey(admitted) ? held : held.Add(admitted, (plugin, claimId))).Find(admitted)
                   .Filter(holder => holder.Plugin == plugin && holder.Claim == claimId)
                   .ToFin(Fail: op.InvalidContext())
               from seated in op.Catch(() => {
                   EventHandler<NamedParametersEventArgs> handler = (_, args) => ignore(op.Catch(() => {
                       Fin<Unit> served = NamedBag.Detach(args: args, slots: schema, op: op)
                           .Bind(bag => op.Catch(() => body(bag)))
                           .Bind(reply => reply.WriteInto(args: args, op: op)
                               .Bind(lease => lease.Within(body: () => Fin.Succ(value: unit), op: op)));
                       _ = served.IfFail(failure => { report(failure); return unit; });
                       return served;
                   }));
                   HostUtils.RegisterNamedCallback(name: admitted, callback: handler);
                   return Fin.Succ(value: Subscription.Of(detach: () => {
                       HostUtils.RemoveNamedCallback(name: admitted);
                       _ = Names.Swap(held => held.Find(admitted).Filter(holder => holder.Claim == claimId).Match(
                           Some: _ => held.Remove(admitted),
                           None: () => held));
                   }));
               }).MapFail(error => {
                   _ = Names.Swap(held => held.Find(admitted).Filter(holder => holder.Claim == claimId).Match(
                       Some: _ => held.Remove(admitted),
                       None: () => held));
                   return error;
               })
               select seated;
    }

    public static Fin<Option<NamedBag>> Execute(
        string name,
        NamedBag bag,
        Seq<NamedSlot> response,
        Option<Env> env = default,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(bag);
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: name)
               from schema in NamedSlot.Admit(slots: response, op: op)
               from _ in guard(
                   flag: env.Map(static held => !held.Cancellation.IsCancellationRequested).IfNone(true),
                   False: (Error)new UiFault.Cancelled(Key: op)).ToFin()
               from reply in bag.Mint(op: op).Bind(packet => packet.Within(
                   body: args => HostUtils.ExecuteNamedCallback(name: admitted, args: args)
                       ? NamedBag.Detach(args: args, slots: schema, op: op).Map(Some)
                       : Fin.Succ(value: Option<NamedBag>.None),
                   op: op))
               select reply;
    }
}

// --- [NODE_FUNCTIONS]
[SmartEnum<int>]
public sealed partial class NodeCallShape {
    public static readonly NodeCallShape Flatten = new(0, false);
    public static readonly NodeCallShape KeepTree = new(1, true);
    internal bool Native { get; }
}

public sealed record NodeReturn(Seq<object> Values, Seq<string> Warnings);

public sealed record NodeFunction {
    private readonly ComponentFunctionInfo info;

    private NodeFunction(
        ComponentFunctionInfo info, string name, string space, string description, Guid component,
        Seq<string> inputs, Seq<bool> optionalInputs, Seq<string> outputs) {
        this.info = info;
        (Name, Space, Description, Component, Inputs, OptionalInputs, Outputs) =
            (name, space, description, component, inputs, optionalInputs, outputs);
    }

    public string Name { get; }
    public string Space { get; }
    public string Description { get; }
    public Guid Component { get; }
    public Seq<string> Inputs { get; }
    public Seq<bool> OptionalInputs { get; }
    public Seq<string> Outputs { get; }

    internal static Fin<NodeFunction> Of(ComponentFunctionInfo info, Op key) =>
        key.Catch(() => Fin.Succ(value: new NodeFunction(
            info: info,
            name: info.Name,
            space: info.Namespace,
            description: info.Description,
            component: info.ComponentGuid,
            inputs: toSeq(info.InputNames).Strict(),
            optionalInputs: toSeq(info.InputsOptional).Strict(),
            outputs: toSeq(info.OutputNames).Strict())));

    public Fin<NodeReturn> Call(Seq<object> arguments, NodeCallShape shape, Op? key = null) {
        Op op = key.OrDefault();
        NodeFunction self = this;
        return from mode in op.Need(shape)
               from produced in op.Catch(() => {
                   object[] values = self.info.Evaluate(args: arguments.AsEnumerable(), keepTree: mode.Native, warnings: out string[] warnings);
                   return Optional(values).ToFin(Fail: op.InvalidResult())
                       .Map(rows => new NodeReturn(
                           Values: toSeq(rows).Strict(),
                           Warnings: toSeq(warnings ?? []).Strict()));
               })
               select produced;
    }
}

public static class NodeFunctions {
    public static Fin<NodeFunction> Find(string fullName, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: fullName)
               from info in op.Catch(() => Optional(Components.FindComponent(admitted)).ToFin(Fail: op.MissingContext()))
               from function in NodeFunction.Of(info: info, key: op)
               select function;
    }

    public static Fin<Seq<NodeFunction>> Census(Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Fin.Succ(value: Components.NodeInCodeFunctions))
            .Bind(table => toSeq(table.GetDynamicMembers())
                .TraverseM(info => NodeFunction.Of(info: info, key: op))
                .As()
                .Map(static rows => rows.Strict()));
    }
}
```

## [08]-[NOTICES]

- Owner: `NoticeSpec` admits title, message, severity, captions, metadata, and assembly guards once; `Notices.Use` brackets one host notification behind a `NoticeLease` for the body's extent.
- Owner: `RunOutcome` closes the completion vocabulary every long-running rail projects into, and `NoticeSpec.OfRun` is the one completion-notice row — severity derives from the outcome case, the metadata bag carries the run's own scale and duration facts, and `NoticeReply` is the three-button decision the consumer reads back.
- Entry: `Notices.Announce` is the completion fire site — it takes an `Option<RunOutcome>`, so a rail whose receipt folds to `None` mid-run announces nothing and a settled rail announces once, and it folds the reply into `Option<NoticeReply>`; a bare-`RunOutcome` rail lifts through `Some` at its own edge, so one entry serves both receipt shapes.
- Law: a long run reports through `RunOutcome`, never through its own receipt type — the render and capture rails each project their receipt into that neutral carrier at their own edge, so no `RenderReceipt`, `CaptureArtifact`, or host render type crosses into this page and the notice row grows one case, never one overload per rail.
- Law: `RunOutcome.Failed` carries the rail's own `Error`, so a refused run announces at `NoticeSeverity.Serious` with the fault text as its metadata row; `Debug` and `Critical` stay caller-selected rows of the host's own five-value severity roster, reached through `NoticeSpec.Create` rather than the run projection.
- Owner: `NoticeReply` and `NoticeSeverity` key the host button and severity vocabularies; `NoticeFact` closes reply and property-change evidence.
- Owner: `CallbackObserver<NoticeFact>` guards both notice callback families and retains consumer failures as lease evidence.
- Entry: `NoticeLease` presents, withdraws, annotates, and detaches metadata through one crossing.
- Law: `NoticeLease` serializes callback delivery, host operations, and release; disposal detaches both callback families, withdraws, and retracts the centre membership through one failure-accumulating host-thread release.
- Law: reply and change facts stamp through the injected `MonotonicTimeline` — provider-branded monotonic evidence, never an ambient clock or a local counter.
- Law: EVERY notification write runs inside `Notification.ExecuteAssemblyProtectedCode` — the host guards each field setter, the metadata indexer, `RemoveMetadata`, `HideModal`, and the centre's own `Remove`, so an unwrapped write against a restricted notice throws; only `ShowModal` is unguarded and it is the one write this page spells bare.
- Law: the guard admits by the WRITING assembly, so `Notices` unions its own assembly into a non-empty `NoticeSpec.Guards` set before construction — a caller-supplied roster omitting the boundary leaves the lease unable to administer the notice it owns, and an empty roster stays empty because empty means unrestricted.
- Law: membership in `NotificationCenter.Notifications` IS rendering — a notification never added shows nowhere and its `ShowModal` queues against nothing — so the mint adds and the release retracts; the set is otherwise unbound and the lease observes only its own notice through `ButtonClicked` and `PropertyChanged`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<HostNotice.Severity>]
public sealed partial class NoticeSeverity {
    public static readonly NoticeSeverity Debug = new(key: HostNotice.Severity.Debug);
    public static readonly NoticeSeverity Info = new(key: HostNotice.Severity.Info);
    public static readonly NoticeSeverity Warning = new(key: HostNotice.Severity.Warning);
    public static readonly NoticeSeverity Serious = new(key: HostNotice.Severity.Serious);
    public static readonly NoticeSeverity Critical = new(key: HostNotice.Severity.Critical);
}

[SmartEnum<HostNoticeButton>]
public sealed partial class NoticeReply {
    public static readonly NoticeReply Dismissed = new(key: HostNoticeButton.CancelOrClose);
    public static readonly NoticeReply Confirmed = new(key: HostNoticeButton.Confirm);
    public static readonly NoticeReply Alternate = new(key: HostNoticeButton.Alternate);
}

[ComplexValueObject]
public sealed partial class NoticeSpec {
    public HostText Title { get; }
    public HostText Message { get; }
    public Option<HostText> Description { get; }
    public NoticeSeverity Severity { get; }
    public Option<HostText> ConfirmCaption { get; }
    public Option<HostText> CancelCaption { get; }
    public Option<HostText> AlternateCaption { get; }
    public FrozenDictionary<string, string> Metadata { get; }
    public Seq<Assembly> Guards { get; }

    public static Fin<NoticeSpec> OfRun(
        RunOutcome outcome, HostText message, Seq<Assembly> guards = default,
        Option<HostText> confirmCaption = default, Option<HostText> alternateCaption = default, Op? key = null) {
        Op op = key.OrDefault();
        return from active in op.Need(outcome)
               from body in op.Need(message)
               let facts = active.Switch(
                   completed: static row => row.Scale,
                   cancelled: static _ => FrozenDictionary<string, string>.Empty,
                   failed: static row => new Dictionary<string, string>(StringComparer.Ordinal) {
                       [nameof(Error)] = row.Reason.Message,
                   }.ToFrozenDictionary(StringComparer.Ordinal))
               from spec in Validate(
                   active.Switch(
                       completed: static row => row.Label,
                       cancelled: static row => row.Label,
                       failed: static row => row.Label),
                   body, Option<HostText>.None, active.Severity, confirmCaption, Option<HostText>.None,
                   alternateCaption, facts, guards, out NoticeSpec? admitted) is null && admitted is not null
                   ? Fin.Succ(value: admitted)
                   : Fin.Fail<NoticeSpec>(error: op.InvalidInput())
               select spec;
    }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText title,
        ref HostText message,
        ref Option<HostText> description,
        ref NoticeSeverity severity,
        ref Option<HostText> confirmCaption,
        ref Option<HostText> cancelCaption,
        ref Option<HostText> alternateCaption,
        ref FrozenDictionary<string, string> metadata,
        ref Seq<Assembly> guards) =>
        validationError = title is null
            || message is null
            || severity is null
            || metadata is null
            || metadata.Any(static row => string.IsNullOrWhiteSpace(row.Key) || row.Value is null)
            || !guards.ForAll(static assembly => assembly is not null)
            ? new ValidationError(message: "Notice specification contains an invalid value.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NoticeFact {
    private NoticeFact() { }
    public sealed record ReplyCase(NoticeReply Reply, Option<MonotonicStamp> At) : NoticeFact;
    public sealed record ChangedCase(string Property, Option<MonotonicStamp> At) : NoticeFact;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunOutcome {
    private RunOutcome() { }
    public sealed record Completed(HostText Label, FrozenDictionary<string, string> Scale) : RunOutcome;
    public sealed record Cancelled(HostText Label) : RunOutcome;
    public sealed record Failed(HostText Label, Error Reason) : RunOutcome;

    internal NoticeSeverity Severity => Switch(
        completed: static _ => NoticeSeverity.Info,
        cancelled: static _ => NoticeSeverity.Warning,
        failed: static _ => NoticeSeverity.Serious);
}

// --- [SERVICES] -----------------------------------------------------------------------------
// `NoticeGate` outlives the constructor: the host callbacks close over IT, never over a half-built lease, so the
// subscription set attaches BEFORE the lease value exists and an attach refusal folds onto the mint's own rail.
internal sealed class NoticeGate(CallbackObserver<NoticeFact> observer, Op op) {
    private readonly CallbackObserver<NoticeFact> observer = observer;
    private readonly Op op = op;
    private int released;

    internal object Sync { get; } = new();

    internal Seq<Error> Faults => observer.Faults;

    internal bool Claim() {
        lock (Sync) {
            if (released is not 0) return false;
            released = 1;
            return true;
        }
    }

    internal Unit Deliver(Func<Fin<NoticeFact>> project) {
        lock (Sync) return released is 0 ? observer.Guard(project: project, op: op) : unit;
    }

    internal Fin<T> Within<T>(Func<Fin<T>> body, Op key) {
        lock (Sync) {
            return from _ in guard(flag: released is 0, False: key.MissingContext()).ToFin()
                   from done in body()
                   select done;
        }
    }
}

public sealed class NoticeLease : IDisposable {
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly HostNotice notice;
    private readonly NoticeGate gate;
    private readonly Subscription observation;
    private readonly Op op;

    private NoticeLease(HostNotice notice, NoticeGate gate, Subscription observation, Op op) {
        this.notice = notice;
        this.gate = gate;
        this.observation = observation;
        this.op = op;
    }

    internal static Fin<NoticeLease> Of(
        HostNotice notice, CallbackObserver<NoticeFact> observer, MonotonicTimeline timeline, Op op) {
        NoticeGate gate = new(observer: observer.Fork(), op: op);
        PropertyChangedEventHandler changed = (_, args) => ignore(gate.Deliver(() => Fin.Succ<NoticeFact>(
            value: new NoticeFact.ChangedCase(
                Property: args.PropertyName ?? string.Empty,
                At: timeline.Capture(key: op).ToOption()))));
        return Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
                () => Subscription.Acquire(
                    acquire: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = button =>
                        ignore(gate.Deliver(() => NoticeReply.TryGet(button, out NoticeReply? reply) && reply is { } admitted
                            ? Fin.Succ<NoticeFact>(value: new NoticeFact.ReplyCase(
                                Reply: admitted,
                                At: timeline.Capture(key: op).ToOption()))
                            : Fin.Fail<NoticeFact>(error: op.InvalidResult())))),
                    release: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = null)),
                () => Subscription.Attach(
                    subscribe: callback => notice.PropertyChanged += callback,
                    unsubscribe: callback => notice.PropertyChanged -= callback,
                    handler: changed)))
            .Map(attached => new NoticeLease(notice: notice, gate: gate, observation: attached, op: op));
    }

    public Seq<Error> Faults => faults.Value + gate.Faults;

    // `ShowModal` is the ONE host write the assembly guard does not check, so it alone crosses bare.
    public Fin<Unit> Present(Op? key = null) => Crossing(body: static held => Op.Side(held.ShowModal), key: key);

    public Fin<Unit> Withdraw(Op? key = null) => Crossing(
        body: static held => Op.Side(() => HostNotice.ExecuteAssemblyProtectedCode(action: held.HideModal)),
        key: key);

    public Fin<FrozenDictionary<string, string>> Metadata(Op? key = null) => Crossing(
        body: static held => held.MetadataCopy.ToFrozenDictionary(StringComparer.Ordinal),
        key: key);

    public Fin<Unit> Annotate(string field, Option<string> value, Op? key = null) {
        Op admitted = key.OrDefault();
        return admitted.AcceptText(value: field).Bind(named => Crossing(
            body: held => Op.Side(() => HostNotice.ExecuteAssemblyProtectedCode(action: () => ignore(value.Match(
                Some: text => Op.Side(() => held[named] = text),
                None: () => Op.Side(() => ignore(held.RemoveMetadata(key: named))))))),
            key: admitted));
    }

    public Fin<Unit> Release(Op? key = null) {
        Op admitted = key.OrDefault();
        if (!gate.Claim()) return Fin.Succ(value: unit);
        HostNotice held = notice;
        Subscription attached = observation;
        return HostThread.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => {
                    attached.Dispose();
                    return Fin.Succ(value: unit);
                },
                () => admitted.Catch(() => Fin.Succ(value: Op.Side(() =>
                    HostNotice.ExecuteAssemblyProtectedCode(action: () => {
                        held.HideModal();
                        _ = NotificationCenter.Notifications.Remove(held);
                    }))))),
            key: admitted).BindFail(failure =>
                (faults.Swap(rows => rows.Add(failure)), Fin.Fail<Unit>(error: failure)).Item2);
    }

    public void Dispose() => _ = Release();

    private Fin<T> Crossing<T>(Func<HostNotice, T> body, Op? key = null) {
        Op admitted = key.OrDefault();
        HostNotice held = notice;
        return HostThread.Run(
            work: new HostWork<T>.Execute(Body: () => gate.Within(
                body: () => admitted.Catch(() => Fin.Succ(value: body(held))),
                key: admitted)),
            key: admitted);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Notices {
    public static Fin<T> Use<T>(
        NoticeSpec spec,
        CallbackObserver<NoticeFact> observer,
        MonotonicTimeline timeline,
        Func<NoticeLease, Fin<T>> body,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(body);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<T>.Execute(Body: () =>
                Mint(spec: spec, observer: observer, timeline: timeline, op: op)
                    .Bind(lease => Bracketed(lease: lease, body: body, op: op))),
            key: op);
    }

    // `None` is a run that has not settled — a mid-run receipt announces nothing rather than posting a false terminal.
    public static Fin<Option<T>> Announce<T>(
        Option<RunOutcome> outcome,
        HostText message,
        CallbackObserver<NoticeFact> observer,
        MonotonicTimeline timeline,
        Func<NoticeLease, Fin<T>> body,
        Seq<Assembly> guards = default,
        Option<HostText> confirmCaption = default,
        Option<HostText> alternateCaption = default,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(body);
        Op op = key.OrDefault();
        return outcome.Match(
            Some: settled =>
                from spec in NoticeSpec.OfRun(
                    outcome: settled, message: message, guards: guards,
                    confirmCaption: confirmCaption, alternateCaption: alternateCaption, key: op)
                from answered in Use(
                    spec: spec, observer: observer, timeline: timeline,
                    body: lease => lease.Present(key: op).Bind(_ => body(lease)), key: op)
                select Some(answered),
            None: static () => Fin.Succ(value: Option<T>.None));
    }

    // `ExecuteAssemblyProtectedCode` admits by the WRITING assembly, so a restricted roster gains this boundary
    // before its first field write, while an empty roster stays empty because empty already means unrestricted.
    private static Fin<NoticeLease> Mint(
        NoticeSpec spec, CallbackObserver<NoticeFact> observer, MonotonicTimeline timeline, Op op) =>
        op.Catch(() => {
            Seq<Assembly> allowed = spec.Guards.IsEmpty
                ? spec.Guards
                : (spec.Guards + Seq(typeof(Notices).Assembly)).Distinct().Strict();
            HostNotice notice = allowed.IsEmpty ? new HostNotice() : new HostNotice(allowedAssemblies: allowed);
            HostNotice.ExecuteAssemblyProtectedCode(action: () => {
                notice.Title = spec.Title.Resolve();
                notice.Message = spec.Message.Resolve();
                notice.SeverityLevel = spec.Severity.Key;
                _ = spec.Description.Iter(text => notice.Description = text.Resolve());
                _ = spec.ConfirmCaption.Iter(text => notice.ConfirmButtonTitle = text.Resolve());
                _ = spec.CancelCaption.Iter(text => notice.CancelButtonTitle = text.Resolve());
                _ = spec.AlternateCaption.Iter(text => notice.AlternateButtonTitle = text.Resolve());
                _ = toSeq(spec.Metadata.AsEnumerable()).Iter(row => Op.Side(() => notice[row.Key] = row.Value));
            });
            // Membership is rendering: the centre `Add` publishes the notice, and a refused publish releases the lease
            // that already holds the host callbacks.
            return NoticeLease.Of(notice: notice, observer: observer, timeline: timeline, op: op)
                .Bind(lease => op.Catch(() => Fin.Succ(value: Op.Side(() => NotificationCenter.Notifications.Add(notice))))
                    .Match(
                        Succ: _ => Fin.Succ(value: lease),
                        Fail: fault => (fun(lease.Dispose)(), Fin.Fail<NoticeLease>(error: fault)).Item2));
        });

    private static Fin<T> Bracketed<T>(NoticeLease lease, Func<NoticeLease, Fin<T>> body, Op op) {
        Fin<T> result = op.Catch(() => body(lease));
        return lease.Release(key: op).Match(
            Succ: _ => result,
            Fail: cleanup => result.Match(
                Succ: _ => Fin.Fail<T>(error: cleanup),
                Fail: primary => Fin.Fail<T>(error: primary + cleanup)));
    }
}
```

## [09]-[TELEMETRY_ROOT]

- Owner: `ShellIdentity` — the plugin-side identity record the telemetry capsule binds: admitted plugin discriminator, plugin `AssemblyLoadContext`, assembly version, resolved content root, and the one process `HostSnapshot`; `ShellTelemetry.Resolve` is its sole mint.
- Entry: `ShellTelemetry.Resolve(Assembly pluginRoot, string plugin, Op? key = null)` → `Fin<ShellIdentity>`.
- Law: the app root alone references `Rasm.AppHost` beside `Rasm.Rhino` — no package source names an AppHost or OpenTelemetry type, and this section's fence compiles against `Rasm` alone.
- Law: one `HostFacts.Probe(new HostProbe.Process())` at the identity mint carries the modeling host's executable name and version, never re-probed per signal.
- Law: content root resolves at the mint because it is plugin knowledge — plugins load from their own install directory, and a host reporting no location for a collectible or single-file assembly falls to the process base, the one path that then resolves.
- Boundary: the AppHost lacing is composition-root work, homed whole at the `apps/rhino/<Plugin>/` shell per the branch composition-root ruling — over one resolved `ShellIdentity` the root gates `ProfileSurface.Resolve` on the `HostRows.Rhino` row (`Tenancy.None`, `DeploymentTopology.InHost`, `LifecycleOwner.CallerOwned`, `Isolation.InProc`, no providers — Rhino owns the process and the plugin binds no provider port, so the row samples whole and projects its logs locally) under `TelemetryDomain.Rhino.Key`, `Environments.Production`, and the identity's content root and version, then opens `PluginTelemetryHost.Open` on the identity's `Alc` with `Seq(RhinoInstruments.Telemetry(version))` as the contributor set and the plugin, process, and version discriminators read off the identity; `Resolve` gates the axis values BEFORE the capsule opens, so an unservable row refuses while no provider exists to dispose.
- Law: capsule cardinality is one per plugin `AssemblyLoadContext`, opened once at plugin load, never per feature; a second plugin is a second identity mint and a second open with its own discriminator.
- Law: `ProfileIdentity.ResourceAttributes` owns resource identity; this package supplies the identity record and its discriminator rows alone.
- Law: `TelemetryDomain.Qualify` renders `service.name` off the `TelemetryDomain.Rhino` row, never a literal; plugin id, host process, and host version all spell `TelemetryDomain.Host.Measure`, and `Rostered` refuses an unrostered `rasm.` key.
- Law: semconv `host.*` stays the machine facts `AddHostDetector` supplies, never a rasm-owned discriminator.
- Law: `Environments.Production` floors the environment row; the `OTEL_RESOURCE_ATTRIBUTES` detector outranks it at deploy.
- Boundary: lifetime is the capsule's own `AssemblyLoadContext.Unloading` hook — `ForceFlush` then `Dispose` per the AppHost provider-lifetime law.
- Boundary: every Rasm meter in the plugin process reaches the capsule `IMeterFactory`; a process-static `Meter` stays the named defect.
- Boundary: the root registers the four `MarshalLatency` checkpoint and tag names through `RegisterCheckpointNames`/`RegisterTagNames` and seats `MarshalLatency.Mount` under the plugin identity — the composing side of the `[02]` marshal-ledger law.
- Packages: `Rasm` and BCL inbox alone — `Rasm.AppHost`, `Microsoft.Extensions.Hosting`, and `NodaTime` are `apps/rhino/<Plugin>/` references, never this package's.
- Growth: a new plugin-side resource dimension is one `ShellIdentity` column; a new machine dimension is one detector row inside `ResourceIdentity.Compose` at the root.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// [BOUNDARY]: the AppHost lacing over this record — the ProfileSurface.Resolve gate on the HostRows.Rhino
// row, PluginTelemetryHost.Open with RhinoInstruments.Telemetry as the contributor set, the TelemetryDomain
// discriminator spellings, and the MarshalLatency name registration and mount — is the apps/rhino/<Plugin>/
// composition root's alone: the one assembly referencing Rasm.AppHost beside Rasm.Rhino.
public sealed record ShellIdentity(
    string Plugin,
    Version Version,
    string ContentRoot,
    AssemblyLoadContext Alc,
    HostSnapshot Snapshot);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ShellTelemetry {
    public static Fin<ShellIdentity> Resolve(Assembly pluginRoot, string plugin, Op? key = null) {
        ArgumentNullException.ThrowIfNull(pluginRoot);
        Op op = key.OrDefault();
        return from name in op.AcceptText(value: plugin)
               from alc in Optional(AssemblyLoadContext.GetLoadContext(pluginRoot)).ToFin(Fail: op.MissingContext())
               from version in Optional(pluginRoot.GetName().Version).ToFin(Fail: op.MissingContext())
               from fact in HostFacts.Probe(probe: new HostProbe.Process(), key: op)
               from snapshot in fact is HostFact.ProcessCase process
                   ? Fin.Succ(value: process.Snapshot)
                   : Fin.Fail<HostSnapshot>(error: op.InvalidResult())
               select new ShellIdentity(
                   Plugin: name,
                   Version: version,
                   ContentRoot: ContentRoot(pluginRoot),
                   Alc: alc,
                   Snapshot: snapshot);
    }

    // Plugins load from their own install directory; a host reporting no location for a collectible or
    // single-file assembly falls to the process base, the one path that then resolves.
    private static string ContentRoot(Assembly root) =>
        Path.GetDirectoryName(root.Location) is { Length: > 0 } held ? held : AppContext.BaseDirectory;
}
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
