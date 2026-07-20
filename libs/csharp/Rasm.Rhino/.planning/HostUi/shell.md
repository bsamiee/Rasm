# [RASM_RHINO_HOSTUI_SHELL]

`Rasm.Rhino.HostUi` owns the Rhino application shell over Eto.

## [01]-[INDEX]

- [02]-[HOST_THREAD]: `HostWork<T>` and `HostThread.Run` own affine execution, queued delivery, affinity-required work, provenance-guarded work, and document-scoped work; `MarshalLatency` seats the marshal-seam checkpoint ledger.
- [03]-[STATUS]: `StatusProgram` folds prompt, pane, point, and toast intent into one crossing and preserves every toast outcome.
- [04]-[PROGRESS]: `ProgressPolicy`, `ProgressMove`, and `ProgressLease` own admission, movement, projection, contention evidence, and release.
- [05]-[WINDOWS]: `WindowScope`, `WindowPolicy`, `ShellWindows`, and `ShellTheme` own host parents, adoption, typed and untyped modal presentation, discovery, placement, and theme transitions.
- [06]-[RUNTIME]: `HostFacts`, `HostAssemblies`, and `ShellSkin` own capability probes, resolver receipts, collectible loading, and the skin load-phase hook.
- [07]-[CALLBACKS]: `CallbackObserver<T>`, `NamedKind`, `NamedBag`, and `NamedCallbacks` close guarded delivery and the typed named-parameter wire; `NodeFunctions` projects the node-in-code table onto the same crossing.
- [08]-[NOTICES]: `NoticeSpec`, `NoticeLease`, and `Notices` mint, present, annotate, and observe host notifications under the assembly-restriction guard.
- [09]-[TELEMETRY_ROOT]: `ShellTelemetry` opens the per-ALC telemetry capsule at the plugin app root and derives resource identity from the host snapshot.

## [02]-[HOST_THREAD]

- Owner: `HostWork<T>` closes execution modality, and `HostThread.Run` is the sole command-thread entry.
- Cases: `Execute` marshals when required, `Posted` carries an admitted `PostWaitLimit`, `Required` rejects an off-thread caller, `Guarded` brackets a faultable native call in `RiskyAction` so the host records provenance, and `Session` composes `DocumentSession.Demand` with detached result capture.
- Entry: `HostThread.Run<T>(HostWork<T>, Op?)` admits the operation once and returns `Fin<T>`.
- Law: `Session` carries every `SessionNeed` in the request value; a consumer never opens a second document demand beside the host operation.
- Law: provenance is a case, never a caller flag — `Guarded` marshals exactly like `Execute` and adds only the `RiskyAction` bracket around the body.
- Law: marshal-seam latency is a mounted ledger, never a second clock — `MarshalLatency` seats one `ILatencyContextProvider` first-mount-wins, the app root registers the checkpoint and tag names through `RegisterCheckpointNames`/`RegisterTagNames` and the tokens resolve once at mount, every off-thread crossing records queued and settled checkpoints with work and outcome tags on one frozen `ILatencyContext`, and an empty seat is the zero-cost pass-through; the `rhino.marshal` instrument row on `Objects/authoring.md` projects this ledger at the app root.
- Boundary: `HostThread` owns Rhino command-thread affinity, while `UiThread` owns Eto control-tree affinity.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;
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

    public static bool Affine => RhinoApp.IsOnMainThread;

    public static Fin<T> Run<T>(HostWork<T> work, Op? key = null) {
        ArgumentNullException.ThrowIfNull(work);
        Op op = key.OrDefault();
        return work.Switch(
            op,
            execute: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Marshalled(body: request.Body, op: held, work: nameof(HostWork<T>.Execute)),
            posted: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Posted(request: request, op: held),
            required: static (held, request) => RhinoApp.IsOnMainThread
                ? held.Catch(request.Body)
                : Fin.Fail<T>(error: new UiFault.OffThread(Key: held)),
            guarded: static (held, request) => RhinoApp.IsOnMainThread
                ? Bracketed(request: request, op: held)
                : Marshalled(body: () => Bracketed(request: request, op: held), op: held, work: nameof(HostWork<T>.Guarded)),
            session: static (held, request) => Session(work: request, op: held));
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

    private static Fin<T> Marshalled<T>(Func<Fin<T>> body, Op op, string work) =>
        MarshalLatency.Measured(work: work, run: () => op.Catch(() => {
            Fin<T>? captured = null;
            RhinoApp.InvokeAndWait(action: () => captured = op.Catch(body));
            return Settled(captured: captured, op: op, capability: nameof(RhinoApp.InvokeAndWait));
        }));

    private static Fin<T> Posted<T>(HostWork<T>.Posted request, Op op) =>
        MarshalLatency.Measured(work: nameof(HostWork<T>.Posted), run: () => op.Catch(() => {
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
            _ = Interlocked.CompareExchange(
                location1: ref state,
                value: (int)PostedState.Expired,
                comparand: (int)PostedState.Pending);
            return Fin.Fail<T>(error: new UiFault.Unavailable(Key: op, Capability: nameof(RhinoApp.InvokeOnUiThread)));
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

public static class MarshalLatency {
    public const string QueuedCheckpoint = "rasm.rhino.marshal.queued";
    public const string SettledCheckpoint = "rasm.rhino.marshal.settled";
    public const string WorkTag = "rasm.rhino.marshal.work";
    public const string OutcomeTag = "rasm.rhino.marshal.outcome";

    private static readonly Atom<Option<SeatRow>> Seat = Atom(Option<SeatRow>.None);

    // App root registers the four names through RegisterCheckpointNames/RegisterTagNames before mounting; tokens resolve once here.
    public static Fin<IDisposable> Mount(ILatencyContextProvider provider, ILatencyContextTokenIssuer issuer, Op? key = null) {
        Op op = key.OrDefault();
        return from live in Optional(provider).ToFin(Fail: op.InvalidInput())
               from mint in Optional(issuer).ToFin(Fail: op.InvalidInput())
               from row in op.Catch(() => Fin.Succ(value: new SeatRow(
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

    internal static Fin<T> Measured<T>(string work, Func<Fin<T>> run) =>
        Seat.Value.Match(
            None: run,
            Some: seat => {
                ILatencyContext ledger = seat.Provider.CreateContext();
                ledger.SetTag(seat.Work, work);
                ledger.AddCheckpoint(seat.Queued);
                Fin<T> outcome = run();
                ledger.AddCheckpoint(seat.Settled);
                ledger.SetTag(seat.Outcome, outcome.IsSucc ? "succ" : "fail");
                ledger.Freeze();
                return outcome;
            });

    private sealed record SeatRow(
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
- Entry: `StatusProgram.Apply` folds every case inside one `HostWork<StatusReceipt>.Execute` crossing.
- Receipt: `StatusReceipt` carries one `ToastOutcome` per toast, so an invalid or rejected notice stays typed without cancelling independent notices.
- Law: `StatusProgram.Combine` preserves producer order; each additional status axis is one `StatusOp` case and one fold arm.
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
    public sealed record Distance(double Value) : StatusOp;
    public sealed record Number(double Value) : StatusOp;
    public sealed record Point(Point3d Value) : StatusOp;
    public sealed record Toast(ToastSpec Spec) : StatusOp;
}

public sealed record StatusProgram(Seq<StatusOp> Operations) {
    public static StatusProgram Combine(params ReadOnlySpan<StatusProgram> programs) =>
        new(Operations: Iterable<StatusProgram>.FromSpan(programs)
            .Fold(Seq<StatusOp>(), static (all, next) => all + next.Operations));

    public Fin<StatusReceipt> Apply(Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<StatusReceipt>.Execute(
                Body: () => Operations.Fold(
                    Fin.Succ(value: new StatusReceipt(Toasts: Seq<ToastOutcome>())),
                    (state, next) => state.Bind(receipt => Apply(next: next, receipt: receipt, op: op)))),
            key: op);
    }

    private static Fin<StatusReceipt> Apply(StatusOp next, StatusReceipt receipt, Op op) =>
        next.Switch(
            (Receipt: receipt, Op: op),
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
            distance: static (held, write) => Fin.Succ(value: (Op.Side(() => StatusBar.SetDistancePane(distance: write.Value)), held.Receipt).Item2),
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
                Default: Optional(args.PromptDefault).Filter(static value => value.Length > 0),
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
- Law: the taskbar pulse is best-effort projection — a refused pulse lands as `TaskbarFault` evidence on the receipt, never a failed advance, so position and receipt always mirror the committed host meter.
- Boundary: `Progress.Use` demands `SessionNeed.Redraw`; release clears every owned projection, returns cleanup failure through the use rail, and retains failed attempts for explicit retry.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ProgressFeature {
    public static readonly ProgressFeature EmbeddedLabel = new();
    public static readonly ProgressFeature Percentage = new();
    public static readonly ProgressFeature Taskbar = new();
    public static readonly ProgressFeature WaitCursor = new();
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

    private readonly MeterGrant grant;
    private readonly Op op;
    private readonly ProgressPolicy policy;
    private readonly Atom<Seq<Error>> releaseFaults = Atom(Seq<Error>());
    private readonly object sync = new();
    private ProgressState state;

    internal ProgressLease(MeterGrant grant, ProgressPolicy policy, Op op) {
        this.grant = grant;
        this.policy = policy;
        this.op = op;
        state = new(Position: policy.Lower, Label: policy.Label, Released: false);
    }

    public Seq<Error> Faults => releaseFaults.Value;

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

    public Fin<Unit> Release() {
        lock (sync) {
            if (state.Released) return Fin.Succ(value: unit);
            Fin<Unit> cleanup = HostThread.Release(
                releases: grant.Switch(
                    this,
                    owned: static (self, owner) => Seq<Func<Fin<Unit>>>(
                        () => {
                            StatusBar.HideProgressMeter(docSerialNumber: owner.Document);
                            return Fin.Succ(value: unit);
                        },
                        () => self.policy.Features.Contains(ProgressFeature.Taskbar)
                            ? TaskbarPulse.Apply(state: PulseState.Idle)
                            : Fin.Succ(value: unit)),
                    foreign: static (_, _) => Seq<Func<Fin<Unit>>>()),
                key: op);
            return cleanup.Match(
                Succ: _ => {
                    state = state with { Released = true };
                    return Fin.Succ(value: unit);
                },
                Fail: failure => {
                    _ = releaseFaults.Swap(rows => rows.Add(failure));
                    return Fin.Fail<Unit>(error: failure);
                });
        }
    }

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
                    TaskbarFault = TaskbarPulse.Apply(state: PulseState.Working, progress: Some(receipt.Fraction), key: op)
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
                    from result in Bracketed(
                        lease: new ProgressLease(grant: grant, policy: policy, op: op),
                        wait: policy.Features.Contains(ProgressFeature.WaitCursor),
                        body: body,
                        op: op)
                    select result),
            key: op);
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
- Law: `Present` is the sole host-boundary modal presenter — an Eto `Prompt<TResult>` presents by handing `ShellWindows.Present` as its presenter seam, so no host-parented dialog reaches raw `ShowModal` or raw `ShowDialog`.
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
- Owner: `SkinProgram` carries the icon, product name, and one `SkinPhase` hook; `ShellSkin` adapts the complete `Skin` load-phase surface onto it.
- Law: every `ShellSkin` override chains the base member first, then routes its `SkinPhase` case; hook faults accumulate in `Faults` and never re-enter the host load sequence.
- Law: platform capability resolves through `HostUtils.GetPlatformService<T>` and stays behind `HostFacts`; a probe is a `HostProbe` case, so a new capability read is one case and one arm.
- Boundary: process facts include runtime architecture, Mono presence, and system references; assembly paths admit through `Op.AcceptText` before any resolver mutation.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostProbe {
    private HostProbe() { }
    public sealed record Process : HostProbe;
    public sealed record Printers : HostProbe;
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostFact {
    private HostFact() { }
    public sealed record ProcessCase(HostSnapshot Snapshot) : HostFact;
    public sealed record PrinterCase(Seq<PrinterSlot> Printers) : HostFact;
}

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
    public sealed record Completed(int Applied) : AssemblyExtensionReceipt;
    public sealed record Partial(int Applied, Error Fault) : AssemblyExtensionReceipt;
}

internal sealed record AssemblyExtensionState(int Applied, Option<Error> Fault);

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
                            : None)).Strict())).Strict()))));
    }
}

public static class HostAssemblies {
    public static Fin<AssemblyExtensionReceipt> Extend(Seq<AssemblySource> sources, Op? key = null) {
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
                        Some: fault => new AssemblyExtensionReceipt.Partial(Applied: state.Applied, Fault: fault),
                        None: () => new AssemblyExtensionReceipt.Completed(Applied: state.Applied)));
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
```

## [07]-[CALLBACKS]

- Owner: `NamedValue` closes the typed-parameter vocabulary, `NamedKind` rows carry read dispatch, and `NamedBag` serializes native common objects into detached payloads before they enter the map.
- Entry: `NamedCallbacks.Register` seats one host callback under a wire name; `NamedCallbacks.Execute` mints, executes, and detaches the response in one crossing.
- Law: wire names are plugin-claimed custody — `HostUtils.RegisterNamedCallback` silently replaces a prior handler, so `Register` claims the name in the process registry keyed on `PluginKey` before the host call, a foreign plugin's claim faults typed instead of shadowing, the owning plugin re-registers only itself, and detach releases the claim with the host row.
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
        from admitted in slots.TraverseM(slot => Optional(slot).ToFin(Fail: op.InvalidInput()).Bind(row =>
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
               from payload in Optional(value).ToFin(Fail: op.InvalidInput())
               from _ in guard(flag: Rows.Find(admitted).IsNone, False: op.InvalidInput()).ToFin()
               select new NamedBag(rows: Rows.Add(admitted, payload));
    }

    public NamedBag Remove(string key) => new(rows: Rows.Remove(key));

    public Option<NamedValue> Find(string key) => Rows.Find(key);

    internal Fin<NamedLease> WriteInto(NamedParametersEventArgs args, Op op) {
        NamedWriteState state = toSeq(Rows).Fold(
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
    private static readonly Atom<HashMap<string, PluginKey>> Names = Atom(HashMap<string, PluginKey>());

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
        return from admitted in op.AcceptText(value: name)
               from schema in NamedSlot.Admit(slots: request, op: op)
               from claim in Names.Swap(held => held.TryAdd(admitted, plugin)).Find(admitted)
                   .Filter(holder => holder == plugin)
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
                       _ = Names.Swap(held => held.Find(admitted).Filter(holder => holder == plugin).Match(
                           Some: _ => held.Remove(admitted),
                           None: () => held));
                   }));
               }).MapFail(error => {
                   _ = Names.Swap(held => held.Find(admitted).Filter(holder => holder == plugin).Match(
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
        return from mode in Optional(shape).ToFin(Fail: op.InvalidInput())
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

- Owner: `NoticeSpec` admits title, message, severity, captions, metadata, and assembly guards once; `Notices.Post` mints the host notification and returns a `NoticeLease`.
- Owner: `NoticeReply` and `NoticeSeverity` key the host button and severity vocabularies; `NoticeFact` closes reply and property-change evidence.
- Owner: `CallbackObserver<NoticeFact>` guards both notice callback families and retains consumer failures as lease evidence.
- Entry: `NoticeLease` presents, withdraws, annotates, and detaches metadata through one crossing.
- Law: `NoticeLease` serializes callback delivery, host operations, and release; disposal detaches both callback families before withdrawal through one failure-accumulating host-thread release.
- Law: reply and change facts stamp through the injected `MonotonicTimeline` — provider-branded monotonic evidence, never an ambient clock or a local counter.
- Law: metadata mutation runs inside `Notification.ExecuteAssemblyProtectedCode`; an unguarded write against a guarded notice is unrepresentable from this surface.
- Law: reply and change delivery crosses `CallbackObserver.Guard` whole, so projection and consumer faults never escape the host dispatcher.
- Boundary: `NotificationCenter.Notifications` renders host-side and its backing set stays unbound — the lease observes only its own notice through the verified `ButtonClicked` and `PropertyChanged` members.

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
            || description.Exists(static text => text is null)
            || confirmCaption.Exists(static text => text is null)
            || cancelCaption.Exists(static text => text is null)
            || alternateCaption.Exists(static text => text is null)
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

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class NoticeLease : IDisposable {
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly HostNotice notice;
    private readonly CallbackObserver<NoticeFact> observer;
    private readonly Subscription observation;
    private readonly Op op;
    private readonly MonotonicTimeline timeline;
    private readonly object sync = new();
    private int released;

    internal NoticeLease(HostNotice notice, CallbackObserver<NoticeFact> observer, MonotonicTimeline timeline, Op op) {
        this.notice = notice;
        this.observer = observer.Fork();
        this.timeline = timeline;
        this.op = op;
        PropertyChangedEventHandler handler = (_, args) => Deliver(() => Fin.Succ<NoticeFact>(
            value: new NoticeFact.ChangedCase(
                Property: args.PropertyName ?? string.Empty,
                At: timeline.Capture(key: op).ToOption())));
        observation = Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
            () => Subscription.Acquire(
                acquire: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = button => Deliver(() =>
                    NoticeReply.TryGet(button, out NoticeReply? reply) && reply is { } admitted
                        ? Fin.Succ<NoticeFact>(value: new NoticeFact.ReplyCase(
                            Reply: admitted,
                            At: timeline.Capture(key: op).ToOption()))
                        : Fin.Fail<NoticeFact>(error: op.InvalidResult()))),
                release: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = null)),
            () => Subscription.Attach(
                subscribe: callback => notice.PropertyChanged += callback,
                unsubscribe: callback => notice.PropertyChanged -= callback,
                handler: handler)))
            .Match(
                Succ: static attached => attached,
                Fail: static fault => throw fault.ToException());
    }

    public Seq<Error> Faults => faults.Value + observer.Faults;

    public Fin<Unit> Present(Op? key = null) => Crossing(body: () => Op.Side(notice.ShowModal), key: key);

    public Fin<Unit> Withdraw(Op? key = null) => Crossing(body: () => Op.Side(notice.HideModal), key: key);

    public Fin<FrozenDictionary<string, string>> Metadata(Op? key = null) => Crossing(
        body: () => notice.MetadataCopy.ToFrozenDictionary(StringComparer.Ordinal),
        key: key);

    public Fin<Unit> Annotate(string field, Option<string> value, Op? key = null) {
        Op admitted = key.OrDefault();
        return admitted.AcceptText(value: field).Bind(named => Crossing(
            body: () => Op.Side(() => HostNotice.ExecuteAssemblyProtectedCode(action: () => ignore(value.Match(
                Some: text => Op.Side(() => notice[named] = text),
                None: () => Op.Side(() => ignore(notice.RemoveMetadata(key: named))))))),
            key: admitted));
    }

    public void Dispose() {
        lock (sync) {
            if (released is not 0) return;
            released = 1;
        }
        _ = HostThread.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => {
                    observation.Dispose();
                    return Fin.Succ(value: unit);
                },
                () => {
                    notice.HideModal();
                    return Fin.Succ(value: unit);
                }),
            key: op).IfFail(failure => {
                _ = faults.Swap(rows => rows.Add(failure));
                return unit;
            });
    }

    private Fin<T> Crossing<T>(Func<T> body, Op? key = null) {
        Op admitted = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<T>.Execute(Body: () => {
                lock (sync) {
                    return from _ in guard(flag: released is 0, False: admitted.MissingContext()).ToFin()
                           from done in admitted.Catch(() => Fin.Succ(value: body()))
                           select done;
                }
            }),
            key: admitted);
    }

    private void Deliver(Func<Fin<NoticeFact>> project) {
        lock (sync) {
            if (released is 0) _ = observer.Guard(project: project, op: op);
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Notices {
    public static Fin<NoticeLease> Post(
        NoticeSpec spec,
        CallbackObserver<NoticeFact> observer,
        MonotonicTimeline timeline,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(timeline);
        Op op = key.OrDefault();
        return HostThread.Run(
            work: new HostWork<NoticeLease>.Execute(Body: () => op.Catch(() => {
                HostNotice notice = spec.Guards.IsEmpty ? new HostNotice() : new HostNotice(allowedAssemblies: spec.Guards);
                notice.Title = spec.Title.Resolve();
                notice.Message = spec.Message.Resolve();
                notice.SeverityLevel = spec.Severity.Key;
                _ = spec.Description.Iter(text => notice.Description = text.Resolve());
                _ = spec.ConfirmCaption.Iter(text => notice.ConfirmButtonTitle = text.Resolve());
                _ = spec.CancelCaption.Iter(text => notice.CancelButtonTitle = text.Resolve());
                _ = spec.AlternateCaption.Iter(text => notice.AlternateButtonTitle = text.Resolve());
                HostNotice.ExecuteAssemblyProtectedCode(action: () =>
                    ignore(toSeq(spec.Metadata.AsEnumerable()).Iter(row => Op.Side(() => notice[row.Key] = row.Value))));
                return Fin.Succ(value: new NoticeLease(
                    notice: notice,
                    observer: observer,
                    timeline: timeline,
                    op: op));
            })),
            key: op);
    }
}
```

## [09]-[TELEMETRY_ROOT]

- Owner: `ShellTelemetry` — the plugin app-root composition seam over the AppHost `PluginTelemetryHost`; one capsule per plugin `AssemblyLoadContext`, opened once at plugin load, never per feature.
- Entry: `ShellTelemetry.Open(Assembly pluginRoot, string plugin, Op? key = null)` → `Fin<PluginTelemetryHost>` — resolves the plugin ALC from the root assembly, folds one `HostFacts` process probe into the resource identity, and opens the capsule under `HostProfile.RhinoPlugin`.
- Law: the app root alone references `Rasm.AppHost` beside `Rasm.Rhino` — no `Rasm.Rhino` package source names an AppHost or OpenTelemetry type, so the strata law holds while the composition realizes at the root.
- Law: resource identity is the estate triple plus the plugin discriminator — `service.namespace` `rasm`, `service.name` `rasm.rhino`, the plugin assembly version, a boot-minted `service.instance.id`, and the `rasm.plugin` attribute — so co-resident plugins in one `Rhino.exe` separate downstream by resource, never by meter name.
- Law: `HostSnapshot` supplies the host-identity evidence — process name and version cross as `host.process`/`host.version` attributes, read through one `HostFacts.Probe(new HostProbe.Process())` at open, never re-probed per signal.
- Boundary: lifetime is the capsule's own `AssemblyLoadContext.Unloading` hook — `ForceFlush` then `Dispose` per the AppHost provider-lifetime law — so the shell registers no second unload path; every Rasm meter in the plugin process reaches the capsule `IMeterFactory`, and a process-static `Meter` stays the named defect.
- Packages: app root only — Rasm.AppHost (`PluginTelemetryHost`, `HostProfile`), OpenTelemetry (`ResourceBuilder`), BCL inbox (`AssemblyLoadContext`).
- Growth: a new resource dimension is one attribute row in the identity delegate; a second plugin is a second `Open` call with its own discriminator; zero new surface.

```csharp signature
// App-root composition: the plugin root assembly references Rasm.AppHost beside Rasm.Rhino and owns
// this seam; no Rasm.Rhino package source composes AppHost or OpenTelemetry types.
public static class ShellTelemetry {
    public static Fin<PluginTelemetryHost> Open(Assembly pluginRoot, string plugin, Op? key = null) {
        ArgumentNullException.ThrowIfNull(pluginRoot);
        Op op = key.OrDefault();
        return from name in op.AcceptText(value: plugin)
               from alc in Optional(AssemblyLoadContext.GetLoadContext(pluginRoot)).ToFin(Fail: op.MissingContext())
               from version in Optional(pluginRoot.GetName().Version).ToFin(Fail: op.MissingContext())
               from fact in HostFacts.Probe(probe: new HostProbe.Process(), key: op)
               from snapshot in fact is HostFact.ProcessCase process
                   ? Fin.Succ(value: process.Snapshot)
                   : Fin.Fail<HostSnapshot>(error: op.InvalidResult())
               from capsule in op.Catch(() => Fin.Succ(value: PluginTelemetryHost.Open(
                   alc: alc,
                   profile: HostProfile.RhinoPlugin,
                   identity: resource => resource
                       .AddService(
                           serviceName: "rasm.rhino",
                           serviceNamespace: "rasm",
                           serviceVersion: version.ToString(),
                           autoGenerateServiceInstanceId: false,
                           serviceInstanceId: Guid.CreateVersion7().ToString())
                       .AddAttributes([
                           new KeyValuePair<string, object>("rasm.plugin", name),
                           new KeyValuePair<string, object>("host.process", snapshot.ProcessName),
                           new KeyValuePair<string, object>("host.version", snapshot.ProcessVersion.ToString()),
                       ]))))
               select capsule;
    }
}
```
