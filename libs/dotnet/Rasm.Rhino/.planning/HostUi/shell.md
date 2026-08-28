# [RASM_RHINO_HOSTUI_SHELL]

`Rasm.Rhino.HostUi` owns the Rhino application shell: the command-thread crossing every host write rides, the status and progress surfaces the host publishes, window parenting and modal presentation, the capability probes and resolver the process answers, the scripting and node-in-code pipelines, the skin load-phase adapter, the accounts and compute-endpoint custody, the typed named-parameter bus, the notification family, and the composition capsule that seats every process-lifetime registry the boundary owns. The capsule is the boundary's ONE in-package composition owner — `Plugin/lifecycle#ADAPTER`'s `OnLoad` is the load root that opens it, and the `apps/<app>/` plugin shell stays the out-of-package root for AppHost lacing and app pins.

Composition is downward: `Fin`, `Cell`/`Transition`, `Lease<T>`, `Custody`, `Ring<T>`, `FaultCell`, `HookBinding`, `CapabilitySet`/`CapabilityLaw`, `ValidityClaim`, and `PackageIdentity<TKey,THostFact>` from `Rasm/Domain`; `MonotonicTimeline`, `MonotonicStamp`, and `GaugedSpan<TLane>` from `Rasm/Parametric/projections`; `UiFault`, `DispatchLane`, `StallPolicy`, `UiThread`, `ThemeGrid`, `ThemePort`, `ThemeShift`, `ThemeVariant`, `ThemeChange`, `ChromeStyler`, `Presence`, `PresenceOp`, `PresenceMount`, and `PulseState` from `Rasm/Interaction`; `Env` from `Rasm/Analysis/query`; `ModelUnit`, `DocKey`, `DocumentSession`, `SessionNeed`, `Subscription`, `PluginKey`, `RhinoPoint`, and `MountRegistry` from the Document spine. The boundary mints no fault family of its own — every refusal codes on the kernel `UiFault` over `FaultBand.Interaction`, `HostRejected` carrying the host member a platform call refused at. `ShellIdentity`, `ShellTelemetry`, `MarshalLane`, `MarshalPulse`, and a local `StallPolicy` are deleted forms: the kernel owns each, and the identity resolve carries `PluginKey` by construction.

## [01]-[INDEX]

- [02]-[HOST_THREAD]: `HostWork<T>` and `HostThread.Run` own affine execution, posted delivery, affinity-required work, provenance-guarded work, and document-scoped work; `MarshalLatency` seats the marshal-boundary ledger under the capsule's timeline and gauges every queued crossing on its `DispatchLane`.
- [03]-[STATUS]: `StatusProgram` folds prompt, pane, point, and toast intent into one document-scoped crossing, rescales every measured case into the live regime, and preserves every toast outcome.
- [04]-[PROGRESS]: `ProgressPolicy`, `ProgressMove`, and `ProgressLease` own admission, movement, projection, contention evidence, and release; the lease publishes the host governance band every paced fold reads and projects onto OS presence through the kernel gate.
- [05]-[WINDOWS]: `WindowScope`, `WindowPolicy`, `ShellWindows`, and `ShellTheme` own host parents, document adoption, typed and untyped modal presentation, discovery, and the theme edge over the kernel grid.
- [06]-[CAPABILITY]: `HostProbe`, `HostFact`, `HostTrait`, `HostSnapshot`, `HostFacts`, and `HostAssemblies` own capability probes, the one process record, resolver extension, and collectible loading.
- [07]-[SCRIPTING]: `ScriptRun`, `ScriptUnit`, `HostScripts`, `NodeCallShape`, `NodeFunction`, and `NodeFunctions` own the host script engine and the node-in-code component table over one crossing.
- [08]-[SKIN]: `SkinPhase`, `SkinProgram`, `ShellSkin`, and `ShellHooks` own the `Rhino.Runtime.Skin` load-phase adapter and its typed hook binding.
- [09]-[ACCOUNTS]: `TokenAsk`, `TokenLease`, `Accounts`, `EntitlementFact`, and `HostEndpoints` own secret-key-confined token custody, detached claim evidence, and the append-only compute-endpoint roster.
- [10]-[CALLBACKS]: `NamedValue`, `NamedKind`, `NamedSlot`, `NamedBag`, `NamedRegistry`, and `NamedCallbacks` close the typed named-parameter wire and its plugin-claimed name custody.
- [11]-[NOTICES]: `NoticeSpec`, `RunOutcome`, `NoticeLease`, and `Notices` mint, present, annotate, and observe host notifications under the assembly-restriction guard.
- [12]-[COMPOSITION_CAPSULE]: `ShellMount` and `ShellCapsule` close the process-lifetime mount family, mint the boundary's one timeline and one fault cell, and retire every seat in reverse mount order.

## [02]-[HOST_THREAD]

- Owner: `HostWork<T>` closes execution modality, `HostThread.Run` is the sole command-thread entry, and `MarshalLatency` is the boundary's mounted ledger.
- Cases: `Execute` marshals when required, `Posted` carries an admitted `PostWaitLimit`, `Required` rejects an off-thread caller, `Guarded` brackets a faultable native call in `RiskyAction` so the host records provenance, and `Session` composes `DocumentSession.Demand`.
- Cases: `PostedState` is `Pending`, `Running`, `Expired`, or `Settled` — a closed family stepped through `Cell.Step`, never an `int` under `Interlocked`.
- Entry: `HostThread.Run<T>(HostWork<T>)` admits the operation once and returns `Fin<T>`; `MarshalLatency.Mount(PluginKey, ILatencyContextProvider, ILatencyContextTokenIssuer, MonotonicTimeline)` seats the ledger.
- Law: `Session` carries every `SessionNeed` in the request value; a consumer never opens a second document demand beside the host operation.
- Law: provenance is a case, never a caller flag — `Guarded` marshals exactly like `Execute` and adds only the `RiskyAction` bracket around the body.
- Law: the posted state cell is the terminal probe, not a marker. The expiry step DECLINES when the body already left `Pending`, so `Refused` carries the state the body reached and a `Settled` read after a lapsed wait answers with the late result rather than discarding a completed crossing; `Committed` proves the body never started and is the only arm that refuses.
- Law: `Session` threads its result THROUGH the demand. `DocumentSession.Demand` bounds its result on `IDetachedDocumentResult`, so the crossing rides a private `Crossed<T>` capsule carrying the body's own result — the `Document/session#SOURCE_AND_SESSION` `DetachedContext` precedent — and no mutable slot is captured across the callback.
- Law: kernel `Custody.Release` is the one all-attempted release fold. `HostThread.Release` is the marshal around it: it crosses to the command thread and delegates the complete release roster without owning another fold.
- Law: marshal-boundary latency is a mounted ledger, never a second clock — `MarshalLatency` seats one `ILatencyContextProvider` first-mount-wins under the mounting plugin's identity, the app root registers the checkpoint, measure, and tag names through `RegisterCheckpointNames`/`RegisterMeasureNames`/`RegisterTagNames` and the tokens resolve once at mount, and an empty seat is the zero-cost pass-through. The `RhinoInstruments.MarshalDuration` row (`Document/events#TELEMETRY_TAP`) reads this ledger as `rasm.rhino.hostui.marshal.duration`.
- Law: the seat cell is a MOUNT SEAT, not a composition root — its sole writer is `ShellMount.Marshal` through `Cell.Seat`, the same shape `Blocks/lifecycle#LIFECYCLE` holds for its vault (branch RULINGS `[02]`). `HostThread.Run` is the boundary's most-composed primitive and is reached from every sub-domain, so the ledger arrives at a seat rather than as a parameter; every entry a plug-in root reaches once — `NamedCallbacks`, `Progress.Use`, `Notices.Use` — takes its dependency as a VALUE instead.
- Law: the gauged set is every crossing that can queue — `Execute` and `Guarded` when marshalled, `Posted` always, and `Session` whole (its `Demand` marshals inside the host) — while `Required` never crosses, its off-thread arm being a refusal rather than a queue.
- Law: the lane is the kernel `DispatchLane` and the budget is its own. `Execute` and `Guarded` ride `Immediate` (one frame), `Posted` rides `Deferred` (six), and `Session` rides `Interactive` (four) — the exact multiples this boundary carried before the roster existed, so the collapse loses no budget and seats no local pace. Breach is DERIVED off `GaugedSpan` and lands as a measure on the ledger; a second retained pulse beside it would be one fact in two places, and `UiThread.LastPulse` answers the Eto marshal, which is a different boundary.
- Exemption: a gauge failure fails the crossing. `MonotonicTimeline.Gauged` runs the body INSIDE its own carrier and states that only a gauge failure fails the outer one, so a broken capture surfaces rather than publishing a crossing whose evidence does not exist.
- Packages: `Rasm/Domain/results` (`Cell`, `Transition`, `Ring<T>`, `ValidityClaim`), `Domain/frame`; `Rasm/Interaction/dispatch` (`UiFault`, `DispatchLane`); `Rasm/Parametric/projections` (`MonotonicTimeline`, `GaugedSpan`); `Rasm.Rhino/Document/session` (`DocumentSession`, `SessionNeed`, `IDetachedDocumentResult`), kernel `Domain/results` (`Custody`); `libs/dotnet/.api/api-telemetry-abstractions.md` (`ILatencyContext`, `ILatencyContextProvider`, `ILatencyContextTokenIssuer`, `CheckpointToken`, `MeasureToken`, `TagToken`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`RhinoApp.IsOnMainThread`/`InvokeAndWait`/`InvokeOnUiThread`, `Localization`), `api-rhinocommon-runtime.md` (`RiskyAction`).
- Growth: a new crossing modality is one `HostWork<T>` case and one `Run` arm; a new gauged coordinate is one measure token the seat resolves once.
- Boundary: `HostThread` owns Rhino command-thread affinity while the kernel `UiThread` owns Eto control-tree affinity — two marshals, two boundaries, one lane roster and one pace band between them.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto.Forms;
using Microsoft.Extensions.Diagnostics.Latency;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.NodeInCode;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;
using Rhino.UI;
using Rhino.UI.Runtime;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingPointF = System.Drawing.PointF;
using HostNotice = Rhino.Runtime.Notifications.Notification;
using HostNoticeButton = Rhino.Runtime.Notifications.ButtonType;
using HostNoticeCenter = Rhino.Runtime.Notifications.NotificationCenter;
using LoginProgress = System.Progress<Rhino.Runtime.RhinoAccounts.RhinoAccoountsProgressInfo>;

namespace Rasm.Rhino.HostUi;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class ShellFaults {
    internal static readonly Rasm.Numerics.Dimension Cap = Rasm.Numerics.Dimension.Create(value: 256);
    internal static Ring<Error> Ring() => new(cap: Cap);
    internal static FaultCell Cell() => new(cap: Cap, clock: TimeProvider.System);
}

// --- [TYPES] ---------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CallbackObserver<T> {
    private readonly Ring<Error> faults = ShellFaults.Ring();

    public Action<Fin<T>> Deliver { get; }
    public Func<Error, Unit> Reject { get; }
    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Action<Fin<T>> deliver,
        ref Func<Error, Unit> reject) =>
        validationError = deliver is null
            ? new ValidationError(message: $"{nameof(Deliver)} is absent.")
            : reject is null
                ? new ValidationError(message: $"{nameof(Reject)} is absent.")
                : null;

    internal Unit Guard(Func<Fin<T>> project) {
        Fin<T> result = Try.lift(project).Run().Bind(static inner => inner);
        return Try.lift(() => {
            Deliver(result);
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner).Match(
            Succ: static _ => unit,
            Fail: primary => {
                Error retained = Try.lift(() => Reject(primary)).Run().Match(
                    Succ: static _ => primary,
                    Fail: secondary => primary + secondary);
                return ignore(faults.Park(item: retained));
            });
    }

    internal CallbackObserver<T> Fork() => Create(deliver: Deliver, reject: Reject);
}

[ComplexValueObject]
public sealed partial class HostText {
    public string English { get; }
    public int Context { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string english,
        ref int context) =>
        validationError = string.IsNullOrWhiteSpace(english)
            ? new ValidationError(message: $"{nameof(English)} is blank.")
            : null;

    internal string Resolve() => Localization.LocalizeString(english: English, contextId: Context);
    internal LocalizeStringPair OptionName() => Localization.LocalizeCommandOptionName(english: English, contextId: Context);
    internal LocalizeStringPair OptionValue() => Localization.LocalizeCommandOptionValue(english: English, contextId: Context);
}

[ValueObject<TimeSpan>]
public readonly partial struct PostWaitLimit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref TimeSpan value) =>
        validationError = value <= TimeSpan.Zero
            ? new ValidationError(message: $"{nameof(PostWaitLimit)} is not positive.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LeaseState<TLive> {
    private LeaseState() { }
    public sealed record Live(TLive Held) : LeaseState<TLive>;
    public sealed record Released : LeaseState<TLive>;
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PostedState {
    private PostedState() { }
    public sealed record Pending : PostedState;
    public sealed record Running : PostedState;
    public sealed record Expired : PostedState;
    public sealed record Settled : PostedState;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostThread {
    private sealed record Crossed<T>(Fin<T> Value) : IDetachedDocumentResult;

    public static Fin<T> Run<T>(HostWork<T> work) {
        return work.Switch(execute: static request => RhinoApp.IsOnMainThread
                ? Try.lift(request.Body).Run().Bind(static inner => inner)
                : Marshalled(body: request.Body, lane: DispatchLane.Immediate),
            posted: static request => RhinoApp.IsOnMainThread
                ? Try.lift(request.Body).Run().Bind(static inner => inner)
                : MarshalLatency.Measured(lane: DispatchLane.Deferred, body: () => Posted(request: request)),
            required: static request => RhinoApp.IsOnMainThread
                ? Try.lift(request.Body).Run().Bind(static inner => inner)
                : Fin.Fail<T>(error: new UiFault.OffThread()),
            guarded: static request => RhinoApp.IsOnMainThread
                ? Bracketed(request: request)
                : Marshalled(body: () => Bracketed(request: request), lane: DispatchLane.Immediate),
            session: static request => MarshalLatency.Measured(
                lane: DispatchLane.Interactive,
                body: () => Session(work: request)));
    }

    internal static Fin<Unit> Release(Seq<Func<Fin<Unit>>> releases) {
        return Run(work: new HostWork<Unit>.Execute(Body: () => Custody.Release(releases: releases)));
    }

    private static Fin<T> Bracketed<T>(HostWork<T>.Guarded request) =>
        Try.lift(() => {
            using RiskyAction guard = new(description: request.Description.English);
            return request.Body();
        }).Run().Bind(static inner => inner);

    private static Fin<T> Marshalled<T>(Func<Fin<T>> body, DispatchLane lane) =>
        MarshalLatency.Measured(lane: lane, body: () => Try.lift(() => {
            Atom<Option<Fin<T>>> landed = Atom(Option<Fin<T>>.None);
            RhinoApp.InvokeAndWait(action: () => ignore(Cell.Seat(landed, () => Try.lift(body).Run().Bind(static inner => inner))));
            return Settled(landed: landed, member: nameof(RhinoApp.InvokeAndWait));
        }).Run().Bind(static inner => inner));

    private static Fin<T> Posted<T>(HostWork<T>.Posted request) =>
        Try.lift(() => {
            Atom<PostedState> state = Atom<PostedState>(new PostedState.Pending());
            TaskCompletionSource<Fin<T>> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            RhinoApp.InvokeOnUiThread(
                method: () => ignore(Cell.Step(
                        cell: state,
                        step: static held => held is PostedState.Pending ? Some<PostedState>(new PostedState.Running()) : None,
                        declined: new KernelFault.InvalidContext())
                    is Transition<PostedState>.Committed
                        ? (completed.TrySetResult(Try.lift(request.Body).Run().Bind(static inner => inner)),
                           Cell.Step(state, static _ => Some<PostedState>(new PostedState.Settled()), new KernelFault.InvalidContext())).Item2
                        : Cell.Step(state, static held => Some(held), new KernelFault.InvalidContext())),
                args: []);
            if (completed.Task.Wait(request.Wait.ToValue())) { return completed.Task.Result; }
            return Cell.Step(
                    cell: state,
                    step: static held => held is PostedState.Pending ? Some<PostedState>(new PostedState.Expired()) : None,
                    declined: new KernelFault.InvalidContext())
                .Switch(
                    state: (Task: completed.Task),
                    committed: static (ctx, _) => Fin.Fail<T>(error: new UiFault.HostRejected(Detail: nameof(RhinoApp.InvokeOnUiThread))),
                    ceded: static (ctx, _) => Fin.Fail<T>(error: new KernelFault.InvalidResult()),
                    refused: static (ctx, row) => row.State is PostedState.Settled
                        ? ctx.Task.Result
                        : Fin.Fail<T>(error: new UiFault.HostRejected(Detail: nameof(RhinoApp.InvokeOnUiThread))),
                    contended: static (ctx, _) => Fin.Fail<T>(error: new KernelFault.InvalidResult()));
        }).Run().Bind(static inner => inner);

    private static Fin<T> Session<T>(HostWork<T>.Session work) =>
        Admit.Demand(
                use: document => Fin.Succ(value: new Crossed<T>(Value: Try.lift(() => work.Body(document)).Run().Bind(static inner => inner))),
                needs: work.Needs.ToArray())
            .Bind(static held => held.Value);

    private static Fin<T> Settled<T>(Atom<Option<Fin<T>>> landed, string member) =>
        landed.Value.Match(
            Some: static result => result,
            None: () => Fin.Fail<T>(error: new UiFault.HostRejected(Detail: member)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class MarshalLatency {
    public const string DurationInstrument = "rasm.rhino.hostui.marshal.duration";
    public const string QueuedCheckpoint = "rasm.rhino.marshal.queued";
    public const string SettledCheckpoint = "rasm.rhino.marshal.settled";
    public const string ElapsedMeasure = "rasm.rhino.marshal.elapsed";
    public const string OverrunMeasure = "rasm.rhino.marshal.overrun";
    public const string WorkTag = "rasm.rhino.marshal.work";
    public const string LaneTag = "rasm.rhino.marshal.lane";
    public const string OutcomeTag = "rasm.rhino.marshal.outcome";

    private static readonly Atom<Option<MarshalSeat>> Seat = Atom(Option<MarshalSeat>.None);

    public static Fin<Lease<IDisposable>> Mount(
        PluginKey plugin,
        ILatencyContextProvider provider,
        ILatencyContextTokenIssuer issuer,
        MonotonicTimeline timeline) {
        return from live in Admit.Need(provider)
               from mint in Admit.Need(issuer)
               from clock in Admit.Need(timeline)
               from row in Try.lift(() => new MarshalSeat(
                   Plugin: plugin,
                   Provider: live,
                   Timeline: clock,
                   Queued: mint.GetCheckpointToken(QueuedCheckpoint),
                   Settled: mint.GetCheckpointToken(SettledCheckpoint),
                   Elapsed: mint.GetMeasureToken(ElapsedMeasure),
                   Overrun: mint.GetMeasureToken(OverrunMeasure),
                   Work: mint.GetTagToken(WorkTag),
                   Lane: mint.GetTagToken(LaneTag),
                   Outcome: mint.GetTagToken(OutcomeTag))).Run()
               from seated in Cell.Seat(Seat, () => row).Switch(
                   committed: static _ => Fin.Succ(value: unit),
                   ceded: static (held) => Fin.Fail<Unit>(error: new KernelFault.InvalidContext()),
                   refused: static refusal => Fin.Fail<Unit>(error: refusal.Cause),
                   contended: static (held) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()))
               select (Lease<IDisposable>)new Lease<IDisposable>.Owned(Value: Subscription.Of(
                   detach: () => ignore(Cell.Step(
                       cell: Seat,
                       step: seated => seated.Filter(live2 => ReferenceEquals(live2, row)).IsSome ? Some(Option<MarshalSeat>.None) : None,
                       declined: new KernelFault.InvalidContext()))));
    }

    internal static Fin<T> Measured<T>(DispatchLane lane, Func<Fin<T>> body) =>
        Seat.Value.Match(
            None: body,
            Some: seat => {
                ILatencyContext ledger = seat.Provider.CreateContext();
                ledger.SetTag(seat.Work, work.ToValue());
                ledger.SetTag(seat.Lane, lane.Key.ToString(CultureInfo.InvariantCulture));
                ledger.AddCheckpoint(seat.Queued);
                return seat.Timeline.Gauged<T, DispatchLane>(lane: lane, work: work, body: body)
                    .Bind(pair => {
                        ledger.AddCheckpoint(seat.Settled);
                        ledger.RecordMeasure(seat.Elapsed, pair.Span.Elapsed.Ticks);
                        ledger.RecordMeasure(seat.Overrun, pair.Span.Overrun.Ticks);
                        ledger.SetTag(seat.Outcome, pair.Value.IsSucc ? "succ" : "fail");
                        ledger.Freeze();
                        return pair.Value;
                    });
            });

    private sealed record MarshalSeat(
        PluginKey Plugin,
        ILatencyContextProvider Provider,
        MonotonicTimeline Timeline,
        CheckpointToken Queued,
        CheckpointToken Settled,
        MeasureToken Elapsed,
        MeasureToken Overrun,
        TagToken Work,
        TagToken Lane,
        TagToken Outcome);
}
```

## [03]-[STATUS]

- Owner: `StatusProgram` is the ordered status algebra, and `StatusOp` carries one admitted host write per case.
- Cases: prompt, prompt message, optional message-pane content, distance, number, point pane, and viewport toast.
- Entry: `StatusProgram.Apply` folds every case inside one document-scoped `HostWork<Seq<ToastOutcome>>.Session` crossing that resolves the live `ModelUnit` regime once for the whole program; `PromptWatch.Observe` detaches host prompt facts.
- Output: `StatusProgram.Apply` returns one `ToastOutcome` per toast, so an invalid or rejected notice stays typed without cancelling independent notices.
- Law: `StatusProgram.Combine` preserves producer order; each additional status axis is one `StatusOp` case and one fold arm.
- Law: a MEASURED case carries the regime its magnitude was resolved in — `StatusOp.Distance` takes the kernel `ModelUnit` the `Commands/acquisition#ACQUISITION` `Acquired.Distance` producer detaches — and the fold rescales through `ModelUnit.ScaleTo` before the host write, because the pane renders the DOCUMENT's unit label over whatever number it is handed and a regime-blind write relabels rather than converts. `Number` stays regime-free by construction: the pane's own contract is a dimensionless count.
- Law: text admits BEFORE it localizes. `Acceptance.Text` screens the authored English and `HostText.Resolve` hands the admitted value to the host localizer, so a blank or whitespace prompt never reaches `Localization` and the resolved string is what the pane receives.
- Law: prompt ordinals ride an atomic cell, never a captured `ref` local — `Atom<long>.Swap` answers the post-state, which is the ordinal the fact carries, and a lost increment is unrepresentable.
- Packages: `Rasm/Domain/results` (`Cell`); `Rasm/Interaction/dispatch` (`UiFault`); `Rasm.Rhino/Document/session` (`DocumentSession`, `SessionNeed`, `ModelUnit`), `Document/lifetime` (`Subscription`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`RhinoApp.SetCommandPrompt`/`SetCommandPromptMessage`/`CommandPromptChanged`, `StatusBar.SetMessagePane`/`ClearMessagePane`/`SetDistancePane`/`SetNumberPane`/`SetPointPane`, `RhinoView.ShowToast`, `CommandPromptChangedEventArgs`).
- Growth: a new status axis is one `StatusOp` case and one `Apply` arm; a new toast placement is one `ToastPlacement` case the host overload set already admits.
- Boundary: `PromptWatch.Observe` detaches callback-scoped option handles into immutable `PromptFact` rows before guarded delivery.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ToastSpec {
    public RhinoView View { get; }
    public HostText Message { get; }
    public ToastPlacement Placement { get; }
}

[ValueObject<int>]
public readonly partial struct ToastHeight {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError(message: $"{nameof(ToastHeight)} is not positive.");
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

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record PromptOption(int Index, string English, string Local);

[Equatable]
public sealed partial record PromptFact(
    string Prompt, Option<string> Default, [property: OrderedEquality] Seq<PromptOption> Options, long Ordinal);

public sealed record StatusProgram(Seq<StatusOp> Operations) {
    public static StatusProgram Combine(params ReadOnlySpan<StatusProgram> programs) =>
        new(Operations: Iterable<StatusProgram>.FromSpan(programs)
            .Fold(Seq<StatusOp>(), static (all, next) => all + next.Operations));

    public Fin<Seq<ToastOutcome>> Apply(DocumentSession session) {
        return HostThread.Run(
            work: new HostWork<Seq<ToastOutcome>>.Session(
                Document: session,
                Needs: [SessionNeed.Read],
                Body: document =>
                    from regime in ModelUnit.Of(value: document.ModelUnits)
                    from toasts in Operations.Fold(
                        Fin.Succ(value: Seq<ToastOutcome>()),
                        (state, next) => state.Bind(carried => Apply(next: next, toasts: carried, regime: regime)))
                    select toasts));
    }

    private static Fin<Seq<ToastOutcome>> Apply(StatusOp next, Seq<ToastOutcome> toasts, ModelUnit regime) =>
        next.Switch(
            (Toasts: toasts, Regime: regime),
            prompt: static (held, write) => Admitted(text: write.Text).Map(prompt => {
                _ = write.Default.Match(
                    Some: fallback => HostEdge.Side(() => RhinoApp.SetCommandPrompt(
                        prompt: prompt, promptDefault: fallback.Resolve())),
                    None: () => HostEdge.Side(() => RhinoApp.SetCommandPrompt(prompt: prompt)));
                return held.Toasts;
            }),
            promptMessage: static (held, write) => Admitted(text: write.Text)
                .Map(prompt => (HostEdge.Side(() => RhinoApp.SetCommandPromptMessage(prompt: prompt)), held.Toasts).Item2),
            pane: static (held, write) => write.Text.Match(
                Some: text => Admitted(text: text)
                    .Map(message => (HostEdge.Side(() => StatusBar.SetMessagePane(message: message)), held.Toasts).Item2),
                None: () => Fin.Succ(value: (HostEdge.Side(StatusBar.ClearMessagePane), held.Toasts).Item2)),
            distance: static (held, write) => write.Unit.ScaleTo(target: held.Regime)
                .Map(scale => (HostEdge.Side(() => StatusBar.SetDistancePane(distance: write.Value * scale)), held.Toasts).Item2),
            number: static (held, write) => Fin.Succ(value: (HostEdge.Side(() => StatusBar.SetNumberPane(number: write.Value)), held.Toasts).Item2),
            point: static (held, write) => Fin.Succ(value: (HostEdge.Side(() => StatusBar.SetPointPane(point: write.Value)), held.Toasts).Item2),
            toast: static (held, write) => Fin.Succ(value: held.Toasts.Add(Shown(spec: write.Spec))));

    private static Fin<string> Admitted(HostText text) => Acceptance.Text(value: text.English).Map(_ => text.Resolve());

    private static ToastOutcome Shown(ToastSpec spec) =>
        (from view in Optional(spec.View).ToFin(Fail: new KernelFault.MissingContext())
         from message in Admitted(text: spec.Message)
         from raised in Try.lift(() => spec.Placement.Switch(
             (View: view, Message: message),
             standard: static (held, _) => held.View.ShowToast(held.Message),
             scaled: static (held, placed) => held.View.ShowToast(held.Message, placed.Height.ToValue()),
             located: static (held, placed) => held.View.ShowToast(held.Message, placed.Height.ToValue(), placed.Point))).Run()
         select ToastId.Create(value: raised))
        .Match<ToastOutcome>(Succ: static id => new ToastOutcome.Shown(Id: id), Fail: static fault => new ToastOutcome.Refused(Fault: fault));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PromptWatch {
    public static Fin<Subscription> Observe(CallbackObserver<PromptFact> observer) {
        Atom<long> ordinal = Atom(0L);
        EventHandler<CommandPromptChangedEventArgs> handler = (_, args) => ignore(observer.Guard(
            project: () => Fin.Succ(value: new PromptFact(
                Prompt: args.Prompt,
                Default: HostEdge.NonEmpty(args.PromptDefault),
                Options: toSeq(args.Options)
                    .Map(static option => new PromptOption(Index: option.Index, English: option.EnglishName, Local: option.LocalName))
                    .Strict(),
                Ordinal: ordinal.Swap(static held => held + 1L)))));
        return Subscription.Attach(
            subscribe: callback => RhinoApp.CommandPromptChanged += callback,
            unsubscribe: callback => RhinoApp.CommandPromptChanged -= callback,
            handler: handler);
    }
}
```

## [04]-[PROGRESS]

- Owner: `ProgressPolicy` admits the meter range, label, and projection features before any host call; `ProgressLease` owns the live meter.
- Cases: `ProgressMove` closes absolute movement, relative movement, and label-only change; `MeterGrant` distinguishes an owned meter from a foreign meter.
- Entry: `Progress.Use(DocumentSession, ProgressPolicy, FaultCell, Func<ProgressLease, Fin<T>>)` opens one document-scoped lease and brackets one callback; `ProgressLease.Advance` is the sole update operation.
- Output: `ProgressReading` carries grant, position, effective label, normalized fraction, and the presence-projection fault for every attempted move.
- Law: only `MeterGrant.Owned` writes or hides the host meter; `MeterGrant.Foreign` returns the reading unchanged.
- Law: the lease IS the host end of the corpus governance band, so a paced fold takes `Fraction`/`Ticks` and `Cancel` off ONE value and no caller writes an `IProgress` shim of its own — `Modeling/solids#MODEL_GATE` `ModelRuntime`, `Modeling/projection#PACING` `ProjectionPacing`, and the kernel `ArrangementPolicy` governance band are the three consumers, each already shaped for exactly these members. Every projection stays a view of `Advance`: a second position store beside the lease state forks the meter from its own reading.
- Law: a refusal an `IProgress.Report` cannot return PARKS on the lease's bounded ring — the `void` host contract constrains the boundary shape and never licenses discarding the bounds refusal that result raises, so a fold reporting past its declared range leaves attributable evidence rather than a meter that silently stops, and the ring's `Shed` reads as a number rather than as process memory.
- Law: escape arming is a policy ROW, so a lease either publishes a live abort edge or publishes `CancellationToken.None`, and the abort rows disarm on BOTH grants because a foreign meter still armed a native callback this lease owns.
- Law: OS presence is the kernel `Presence` gate and never a second taskbar writer. The lease holds AT MOST ONE `Lease<PresenceMount>`: the first projection applies it and every later step STEERS it in place (`PresenceMount.Steer`, `Prior` never re-captured), because a presence mount RESTORES the state it overwrote on release and a release-per-step would land the restored idle between frames. A refused apply lands on the reading's `PresenceFault`, never a failed advance, so position and reading always mirror the committed host meter.
- Law: lifecycle is a case, never a flag — `LeaseState<(int Position, HostText Label)>` carries the live meter position and its label together, so a released lease has no position to read and a guard against a boolean flag has no spelling.
- Law: the lease locks in ONE direction — every operation crosses `HostThread` first and takes the state lock inside the marshalled body, never the reverse; a release holding the lock across a blocking marshal inverts the order against a concurrent advance and deadlocks the host thread against its own caller, so the marshal is always outside and the lock always inside.
- Exemption: `[ATOM_STATE]` — the state field rides `lock` rather than a `Cell` transition. Host writes run inside the guarded region and a compare-and-swap body re-runs on every contended retry, which would repeat `UpdateProgressMeter` against the host; the lock is the platform-forced form and is contained to this owner.
- Packages: `Rasm/Domain/results` (`Ring<T>`, `Lease<T>`), `Domain/hooks` (`FaultCell`), `Numerics/atoms` (`UnitInterval`); `Rasm/Interaction/chrome` (`Presence`, `PresenceOp`, `PresenceMount`, `PulseState`), `Interaction/dispatch` (`UiFault`); `Rasm.Rhino/Document/session` (`DocumentSession`, `SessionNeed`, `DocKey`), `Document/lifetime` (`Subscription`), kernel `Domain/results` (`Custody`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`StatusBar.ShowProgressMeter`/`UpdateProgressMeter`/`HideProgressMeter`, `WaitCursor`, `RhinoApp.EscapeKeyPressed`).
- Growth: a new projection feature is one `ProgressFeature` row and one arm; a new movement modality is one `ProgressMove` case.
- Boundary: `Progress.Use` demands `SessionNeed.Redraw`; release clears every owned projection, returns cleanup failure through the use bracket, and retains failed attempts for explicit retry.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ProgressFeature {
    public static readonly ProgressFeature EmbeddedLabel = new();
    public static readonly ProgressFeature Percentage = new();
    public static readonly ProgressFeature Presence = new();
    public static readonly ProgressFeature WaitCursor = new();
    public static readonly ProgressFeature Escape = new();
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

    internal static Fin<MeterGrant> Admit(int code, DocKey document) =>
        code switch {
            1 => Fin.Succ<MeterGrant>(value: new Owned(Document: document)),
            -1 => Fin.Succ<MeterGrant>(value: new Foreign()),
            _ => Fin.Fail<MeterGrant>(error: new UiFault.HostRejected(Detail: nameof(StatusBar.ShowProgressMeter))),
        };
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ProgressPolicy {
    public int Lower { get; }
    public int Upper { get; }
    public HostText Label { get; }
    public FrozenSet<ProgressFeature> Features { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int lower,
        ref int upper,
        ref HostText label,
        ref FrozenSet<ProgressFeature> features) =>
        validationError = label is null
            ? new ValidationError(message: $"{nameof(Label)} is absent.")
            : features is null
                ? new ValidationError(message: $"{nameof(Features)} is absent.")
                : upper < lower
                    ? new ValidationError(message: $"{nameof(Upper)} is below {nameof(Lower)}.")
                    : null;

    public static Fin<ProgressPolicy> Of(
        int lower,
        int upper,
        HostText label,
        FrozenSet<ProgressFeature> features) =>
        FactoryBridge.Accept<ProgressPolicy>(
            Validate(lower: lower, upper: upper, label: label, features: features, obj: out ProgressPolicy? admitted),
            admitted);
}

public sealed record ProgressReading(
    MeterGrant Grant,
    int Position,
    HostText Label,
    UnitInterval Fraction,
    Option<Error> PresenceFault);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ProgressLease : IDisposable {
    private sealed record LeaseReporter(ProgressLease Lease) : IProgress<double>, IProgress<int> {
        public void Report(double value) => Lease.Park(Lease.Advance(new ProgressMove.Absolute(
            Position: Lease.policy.Lower + (int)Math.Round(
                Math.Clamp(value: value, min: 0.0, max: 1.0) * (Lease.policy.Upper - Lease.policy.Lower)))));

        public void Report(int value) => Lease.Park(Lease.Advance(new ProgressMove.Absolute(
            Position: Math.Clamp(value: value, min: Lease.policy.Lower, max: Lease.policy.Upper))));
    }

    private readonly MeterGrant grant;
    private readonly ProgressPolicy policy;
    private readonly FaultCell cell;
    private readonly Ring<Error> faults = ShellFaults.Ring();
    private readonly LeaseReporter reporter;
    private readonly Option<Subscription> escape;
    private readonly Option<CancellationTokenSource> abort;
    private readonly Atom<Option<Lease<PresenceMount>>> presence = Atom(Option<Lease<PresenceMount>>.None);
    private readonly Lock sync = new();
    private LeaseState<(int Position, HostText Label)> state;

    internal ProgressLease(
        MeterGrant grant,
        ProgressPolicy policy,
        FaultCell cell,
        Option<CancellationTokenSource> abort,
        Option<Subscription> escape) {
        (this.grant, this.policy, this.cell, this.abort, this.escape, this.op) = (grant, policy, cell, abort, escape);
        reporter = new LeaseReporter(Lease: this);
        state = new LeaseState<(int, HostText)>.Live(Held: (policy.Lower, policy.Label));
    }

    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public IProgress<double> Fraction => reporter;
    public IProgress<int> Ticks => reporter;

    public CancellationToken Cancel => abort.Match(Some: static source => source.Token, None: static () => CancellationToken.None);

    public Fin<ProgressReading> Advance(ProgressMove move) {
        return HostThread.Run(
            work: new HostWork<ProgressReading>.Execute(Body: () => {
                lock (sync) {
                    return state.Switch(
                        (Self: this, Move: move),
                        live: static (ctx, row) =>
                            from next in ctx.Move.Switch(
                                (Held: row.Held, Policy: ctx.Self.policy),
                                absolute: static (carried, step) => Bounded(
                                    position: step.Position,
                                    label: step.Label.IfNone(carried.Held.Label),
                                    policy: carried.Policy),
                                relative: static (carried, step) => Bounded(
                                    position: (long)carried.Held.Position + step.Delta,
                                    label: step.Label.IfNone(carried.Held.Label),
                                    policy: carried.Policy),
                                label: static (carried, step) => Acceptance.Text(value: step.Text.English)
                                    .Map(_ => (Position: carried.Held.Position, Label: step.Text)))
                            from outcome in ctx.Self.grant.Switch(
                                (Self: ctx.Self, Move: next),
                                owned: static (carried, owner) => carried.Self.Drive(
                                    document: owner.Document, move: carried.Move),
                                foreign: static (carried, _) => Fin.Succ(value: carried.Self.Reading(
                                    position: carried.Move.Position, label: carried.Move.Label, fault: None)))
                            select outcome,
                        released: static (ctx, _) => Fin.Fail<ProgressReading>(error: new KernelFault.MissingContext()));
                }
            }));
    }

    public Fin<Unit> Release() => HostThread.Run(
        work: new HostWork<Unit>.Execute(Body: () => {
            lock (sync) {
                return state.Switch(
                    this,
                    live: static (self, _) => self.Cleanup().Match(
                        Succ: _ => {
                            self.state = new LeaseState<(int, HostText)>.Released();
                            return Fin.Succ(value: unit);
                        },
                        Fail: failure => (self.faults.Park(item: failure), Fin.Fail<Unit>(error: failure)).Item2),
                    released: static (_, _) => Fin.Succ(value: unit));
            }
        }));

    public void Dispose() => _ = Release();

    private Unit Park<T>(Fin<T> outcome) =>
        outcome.Match(Succ: static _ => unit, Fail: failure => ignore(faults.Park(item: failure)));

    private Fin<Unit> Cleanup() => HostThread.Release(
        releases: grant.Switch(
            this,
            owned: static (self, owner) => Seq<Func<Fin<Unit>>>(
                () => Try.lift(() => StatusBar.HideProgressMeter(docSerialNumber: owner.Document)).Run().Bind(static inner => inner),
                self.Restore) + self.Disarm(),
            foreign: static (self, _) => Seq<Func<Fin<Unit>>>(self.Restore) + self.Disarm()));

    private Seq<Func<Fin<Unit>>> Disarm() => Seq<Func<Fin<Unit>>>(
        () => Try.lift(() => (escape.Iter(static row => row.Dispose()), unit).Item2).Run(),
        () => Try.lift(() => (abort.Iter(static source => source.Dispose()), unit).Item2).Run());

    private static Fin<(int Position, HostText Label)> Bounded(long position, HostText label, ProgressPolicy policy) =>
        position >= policy.Lower && position <= policy.Upper
            ? Fin.Succ(value: ((int)position, label))
            : Fin.Fail<(int, HostText)>(error: new KernelFault.InvalidInput(Axis: Some(nameof(ProgressMove.Absolute.Position))));

    private Fin<ProgressReading> Drive(DocKey document, (int Position, HostText Label) move) =>
        Try.lift(() => {
            StatusBar.UpdateProgressMeter(
                docSerialNumber: document,
                label: move.Label.Resolve(),
                position: move.Position,
                absolute: true);
            state = new LeaseState<(int, HostText)>.Live(Held: move);
            return Fin.Succ(value: Reading(
                position: move.Position,
                label: move.Label,
                fault: policy.Features.Contains(ProgressFeature.Presence)
                    ? Project(fraction: Fraction(position: move.Position)).Match(
                        Succ: static _ => Option<Error>.None, Fail: Some)
                    : None));
        }).Run().Bind(static inner => inner);

    private Fin<Unit> Project(UnitInterval fraction) =>
        presence.Value.Match(
            Some: standing => standing.Resource.Steer(
                operation: new PresenceOp.Pulse(State: new PulseState.Working(Progress: fraction))),
            None: () =>
                from mount in Presence.Apply(
                    operation: new PresenceOp.Pulse(State: new PulseState.Working(Progress: fraction)),
                    faults: cell)
                from seated in Cell.Seat(presence, () => mount).Switch(
                    state: (Mount: mount),
                    committed: static (_, _) => Fin.Succ(value: unit),
                    ceded: static (ctx, _) => (HostEdge.Side(ctx.Mount.Dispose), Fin.Fail<Unit>(error: new KernelFault.InvalidContext())).Item2,
                    refused: static (_, row) => Fin.Fail<Unit>(error: row.Cause),
                    contended: static (ctx, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()))
                select seated);

    private Fin<Unit> Restore() => Cell.Take(presence).Current
        .TraverseM(mount => Try.lift(() => mount.Dispose()).Run())
        .As()
        .Map(static _ => unit);

    private UnitInterval Fraction(int position) => UnitInterval.Create(value: policy.Upper > policy.Lower
        ? Math.Clamp(
            value: (position - (double)policy.Lower) / (policy.Upper - (double)policy.Lower),
            min: 0.0,
            max: 1.0)
        : 1.0);

    private ProgressReading Reading(int position, HostText label, Option<Error> fault) => new(
        Grant: grant,
        Position: position,
        Label: label,
        Fraction: Fraction(position: position),
        PresenceFault: fault);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Progress {
    public static Fin<T> Use<T>(
        DocumentSession session,
        ProgressPolicy policy,
        FaultCell faults,
        Func<ProgressLease, Fin<T>> body) {
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
                        document: session.Key)
                    from armed in Armed(policy: policy)
                    from result in Bracketed(
                        lease: new ProgressLease(
                            grant: grant, policy: policy, cell: faults,
                            abort: armed.Abort, escape: armed.Escape),
                        wait: policy.Features.Contains(ProgressFeature.WaitCursor),
                        body: body)
                    select result));
    }

    private static Fin<(Option<CancellationTokenSource> Abort, Option<Subscription> Escape)> Armed(ProgressPolicy policy) {
        if (!policy.Features.Contains(ProgressFeature.Escape)) { return Fin.Succ((Option<CancellationTokenSource>.None, Option<Subscription>.None)); }
        CancellationTokenSource source = new();
        EventHandler handler = (_, _) => ignore(Try.lift(source.Cancel).Run());
        return Subscription.Attach(
                subscribe: callback => RhinoApp.EscapeKeyPressed += callback,
                unsubscribe: callback => RhinoApp.EscapeKeyPressed -= callback,
                handler: handler)
            .Map(subscription => (Some(source), Some(subscription)))
            .Rollback(source);
    }

    private static Fin<T> Bracketed<T>(ProgressLease lease, bool wait, Func<ProgressLease, Fin<T>> body) =>
        Try.lift(() => {
            using WaitCursor? cursor = wait ? new WaitCursor() : null;
            return body(lease);
        }).Run().Bind(static inner => inner)
        .Settled(held: Seq(lease), release: static held => held.Release());
}
```

## [05]-[WINDOWS]

- Owner: `WindowScope` selects the application or document parent, and `ShellWindows.Parent` resolves both through one entry.
- Owner: `WindowPolicy` carries the kernel `ChromeStyler` a window wears and the close-time persistence Rhino keys by window type; `ShellTheme` projects the Rhino theme edge onto the kernel grid.
- Entry: `Adopt`, `Present`, `Discover`, and `Owner` remain separate because modeless ownership, modal return, typed census, and inverse document lookup carry distinct result regimes.
- Law: `Present` owns every modal modality on one name — a `Dialog<TResult>` returns its typed result, a bare `Dialog` (the themed message box and every result-on-the-instance dialog) returns `Unit` and the caller reads the instance, and a `CommonDialog` (every native-backed picker) returns its `DialogResult` verdict with the instance carrying the picked value — the input's static type discriminates, never a mode flag.
- Law: `Present` is the sole host-boundary modal presenter and is the presenter VALUE the kernel `Prompt<TResult>.Ask` takes as its `present` argument, so `Rasm.Interaction` never sees a `Rhino.UI` type and no adapter stands between them. Raw `ShowModal` never appears at a consumer and raw `ShowDialog` appears exactly once: the `CommonDialog` arm, because a native picker publishes no semi-modal member.
- Law: adoption is DOCUMENT DOCKING and is not the kernel realize. `Interaction/chrome#WINDOW` `WindowSpec.Realize` builds a window it then owns; this entry takes a window the CALLER already built and binds it to a Rhino document through the registered host bridge, which is a boundary the kernel cannot name. The kernel's own mint-order law — menu and toolbar before the window, both drained on the no-window path — already governs everything it realizes; what stays here is the inverse this entry alone owes: a failed show ROLLS BACK the caller's own title, location, and window state through `Custody.Rollback`, because those belong to a value this boundary borrowed and never owned.
- Law: `WindowPolicy` rows carry a kernel `ChromeStyler` and a persist function. The styler is the kernel's one dress mechanism; persistence keys by the window TYPE because Rhino owns the persisted slot identity, and a row declaring neither is the bare row rather than two absent columns.
- Law: `ShellTheme` observes only — the polarity probe is Rhino's and the grid is the kernel's. `Current` reads the host LIVE at every call, never a boot snapshot: the OS theme changes under a running process, and `HostSnapshot`'s `HostTrait.DarkMode` row is a point-in-time fact of the process record, not the theme edge's answer. Theme mutation is `Persistence/appsettings#STATE_AND_FAMILY`'s `AppTheme`, reached through `AppSettings.Commit(AppOperation.ThemeCase)`, and a shell consumer composes that owner rather than writing the host theme edge.
- Exemption: `HostUtils.RunningInDarkMode` reads `AdvancedSettings.DarkMode`, a managed settings read rather than thread-affine native UI state, so it is safe off-thread and owes no `HostThread` crossing.
- Packages: `Rasm/Domain/results` (`Lease<T>`); `Rasm/Interaction/chrome` (`ChromeStyler`), `Interaction/paint` (`ThemeVariant`, `ThemeShift`, `ThemeChange`), `Interaction/platform` (`ThemePort`); `Rasm.Rhino/Document/session` (`DocumentSession`, `SessionNeed`, `DocKey`), `Document/lifetime` (`Subscription`), kernel `Domain/results` (`Custody`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`RhinoEtoApp.MainWindow`/`MainWindowForDocument`, `EtoExtensions.UseRhinoStyle`/`Show`/`ShowSemiModal`/`SavePosition`/`RestorePosition`/`LocalizeAndRestore`/`WindowsFromDocument`/`GetRhinoDoc`, `ThemeSettings.ThemeChanged`), `api-rhinocommon-runtime.md` (`HostUtils.RunningInDarkMode`); `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md` (`Window`, `Form`, `Dialog`, `Dialog<TResult>`, `CommonDialog`, `DialogResult`, `Control`).
- Growth: a new dress-and-persist posture is one `WindowPolicy` row; a new parent axis is one `WindowScope` case and one `Parent` arm.
- Boundary: every document-scoped operation is a `HostWork<T>.Session` value, and every returned owner detaches as `DocKey`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowScope {
    private WindowScope() { }
    public sealed record Application : WindowScope;
    public sealed record Document(DocumentSession Session) : WindowScope;
}

[SmartEnum]
public sealed partial class WindowPolicy {
    public static readonly WindowPolicy Native = new(
        styler: Some(new ChromeStyler(Dress: static (control, key) => Try.lift(() => {
            EtoExtensions.UseRhinoStyle(control);
            return control is Window window
                ? Fin.Succ(value: ignore(EtoExtensions.RestorePosition(window, window.GetType())))
                : Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner))),
        persist: static (window, key) => Try.lift(() => EtoExtensions.SavePosition(window, window.GetType())).Run().Bind(static inner => inner));
    public static readonly WindowPolicy Localized = new(
        styler: Some(new ChromeStyler(Dress: static (control, key) => Try.lift(() => control is Window window
            ? Fin.Succ(value: HostEdge.Side(() => EtoExtensions.LocalizeAndRestore(window, window.GetType())))
            : Fin.Succ(value: unit)).Run().Bind(static inner => inner))),
        persist: static (window, key) => Try.lift(() => EtoExtensions.SavePosition(window, window.GetType())).Run().Bind(static inner => inner));
    public static readonly WindowPolicy Bare = new(
        styler: Option<ChromeStyler>.None,
        persist: static (_, _) => Fin.Succ(value: unit));

    public Option<ChromeStyler> Styler { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Persist(Window window);

    internal Fin<Unit> Prepare(Window window) =>
        Styler.TraverseM(dress => dress.Dress(window)).As().Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ShellWindows {
    public static Fin<Window> Parent(WindowScope scope) {
        return scope.Switch(application: static _ => HostThread.Run(
                work: new HostWork<Window>.Execute(Body: () => Optional(RhinoEtoApp.MainWindow).ToFin(Fail: new KernelFault.MissingContext()))),
            document: static owned => HostThread.Run(
                work: new HostWork<Window>.Session(
                    Document: owned.Session,
                    Needs: [SessionNeed.Read],
                    Body: document => Optional(RhinoEtoApp.MainWindowForDocument(document)).ToFin(Fail: new KernelFault.MissingContext()))));
    }

    public static Fin<Form> Adopt(Form window, DocumentSession session, WindowPolicy policy) {
        return HostThread.Run(
            work: new HostWork<Form>.Session(
                Document: session,
                Needs: [SessionNeed.Redraw],
                Body: document => {
                    (string Title, Eto.Drawing.Point Location, WindowState State) prior =
                        (window.Title, window.Location, window.WindowState);
                    Atom<Option<Subscription>> attached = Atom(Option<Subscription>.None);
                    return (from _ in policy.Prepare(window: window)
                            from closed in Subscription.Attach(
                                subscribe: callback => window.Closed += callback,
                                unsubscribe: callback => window.Closed -= callback,
                                handler: (EventHandler<EventArgs>)((_, _) => ignore(policy.Persist(window: window))))
                            from seated in Fin.Succ(value: ignore(Cell.Seat(attached, () => closed)))
                            from shown in Try.lift(() => EtoExtensions.Show(window, document)).Run().Bind(static inner => inner)
                            select window)
                        .Rollback(
                            release: () => Try.lift(() => {
                                _ = Cell.Take(attached).Current.Iter(static row => row.Dispose());
                                (window.Title, window.Location, window.WindowState) = prior;
                                return Fin.Succ(value: unit);
                            }).Run().Bind(static inner => inner));
                }));
    }

    public static Fin<TResult> Present<TResult>(
        Dialog<TResult> dialog,
        DocumentSession session,
        Option<Control> parent = default) {
        return HostThread.Run(
            work: new HostWork<TResult>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: new KernelFault.MissingContext())
                    .Bind(owner => Try.lift(() => EtoExtensions.ShowSemiModal(dialog, document, owner)).Run())));
    }

    public static Fin<Unit> Present(Dialog dialog, DocumentSession session, Option<Control> parent = default) {
        return HostThread.Run(
            work: new HostWork<Unit>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: new KernelFault.MissingContext())
                    .Bind(owner => Try.lift(() => EtoExtensions.ShowSemiModal(dialog, document, owner)).Run().Bind(static inner => inner))));
    }

    public static Fin<DialogResult> Present(CommonDialog dialog, DocumentSession session, Option<Control> parent = default) {
        return HostThread.Run(
            work: new HostWork<DialogResult>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => (parent | Optional((Control)RhinoEtoApp.MainWindowForDocument(document)))
                    .ToFin(Fail: new KernelFault.MissingContext())
                    .Bind(owner => Try.lift(() => dialog.ShowDialog(owner)).Run())));
    }

    public static Fin<Seq<TWindow>> Discover<TWindow>(DocumentSession session) where TWindow : Window {
        return HostThread.Run(
            work: new HostWork<Seq<TWindow>>.Session(
                Document: session,
                Needs: [SessionNeed.Read],
                Body: document => Try.lift(() => toSeq(EtoExtensions.WindowsFromDocument<TWindow>(document)).Strict()).Run()));
    }

    public static Fin<DocKey> Owner(Form window) {
        return HostThread.Run(
            work: new HostWork<DocKey>.Execute(
                Body: () => Optional(EtoExtensions.GetRhinoDoc(window))
                    .ToFin(Fail: new KernelFault.MissingContext())
                    .Bind(document => DocKey.Of(document: document))));
    }
}

public static class ShellTheme {
    public static ThemeVariant Current => HostUtils.RunningInDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;

    public static Fin<Subscription> Observe(ThemePort theme, CallbackObserver<ThemeChange> observer) {
        EventHandler handler = (_, _) => ignore(observer.Guard(
            project: () => theme.Change(shift: new ThemeShift.Generated(Variant: Current))));
        return Subscription.Attach(
            subscribe: callback => ThemeSettings.ThemeChanged += callback,
            unsubscribe: callback => ThemeSettings.ThemeChanged -= callback,
            handler: handler);
    }
}
```

## [06]-[CAPABILITY]

- Owner: `HostProbe` closes the capability-read request family and `HostFact` its detached answers; `HostSnapshot` is the one process-and-OS record; `HostTrait` is its capability column.
- Owner: `HostAssemblies` pre-admits every resolver source, reports the applied prefix with its refusal, and folds collectible loading over `AssemblyIntake` cases.
- Cases: `HostProbe` is process, printers, scripting, entitlement, or compute; `EntitlementFact` is granted with its signature or denied with its reason, so the corner where a denial carries no reason and an entitlement carries one is unrepresentable.
- Entry: `Admit.Probe(HostProbe)` answers the family; `HostFacts.Process()` is the typed process probe whose RETURN TYPE is the answer, so the capsule binds it as the S14 host slot and no case test stands between the probe and the identity.
- Auto: the four independent host switches — dark mode, server host, pre-release build, Mono runtime — ride ONE `CapabilitySet<HostTrait>` read by set algebra. Every corner is real (a pre-release server build under Mono in dark mode is a machine that exists), so the law is `CapabilityLaw.Open` and states it.
- Law: platform capability stays behind `HostFacts` and enters through the two host locators by shape — `HostUtils.GetPlatformService<T>` resolves a typed service contract and `Rhino.UI.Runtime.PlatformServiceProvider` answers the fixed process facts it publishes directly — so a new capability read is one `HostProbe` case and one arm.
- Law: engine presence is a probed host fact, never an assumption — `HostProbe.Scripting` answers the `ScriptEngineSnapshot` search-path and runtime-assembly census, and every `HostScripts` entry refuses typed when `PythonScript.Create()` answers null.
- Law: entitlement is a capability probe, never a member reach — `CloudHostUtils` is pure property reads off the `ICloudHost` platform service (`DoNothingCloudHost` when no provider ships), so the probe answers headless with no UI and no server call.
- Law: resolver extension is process-permanent. The host publishes no removal, so `ExtensionTally` attributes every applied row to the extending plugin and an applied PREFIX is never rolled back — custody is the `SnapshotParticipant` permanence class, stated rather than hidden. The fold holds that prefix because a traverse abandons it: a combinator that aborts on the first refusal loses exactly the count the tally exists to attribute.
- Law: absence rides `Option` on the tally, never a second case. `Fault.IsNone` IS the completed answer, so the intermediate state record and the two-case union that restated it have no spelling left.
- Packages: `Rasm/Domain/results` , `Domain/validation` (`CapabilitySet`, `CapabilityLaw`, `ICapability`); `Rasm.Rhino/Document/events` (`PluginKey`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`HostUtils.GetCurrentProcessInfo`/`OperatingSystemEdition`/`OperatingSystemProductName`/`OperatingSystemBuildNumber`/`OperatingSystemInstallationType`/`CurrentOSLanguage`/`GetSystemProcessorCount`/`RunningInDarkMode`/`RunningOnServer`/`IsPreRelease`/`RunningInMono`/`GetSystemReferenceAssemblies`/`GetAssemblySearchPaths`/`GetPrinterNames`/`GetPrinterDPI`/`GetPrinterFormNames`/`GetPrinterFormSize`/`GetCustomComputeEndpoints`/`LoadAssemblyFrom`/`LoadAssemblyFromStream`/`LoadAssemblyFromName`, `AssemblyResolver.AddSearchFolder`/`AddSearchFile`, `CloudHostUtils.IsEntitled`/`DenyReason`/`Signature`, `PythonScript.Create`/`SearchPaths`/`RuntimeAssemblies`), `api-rhino-ui.md` (`PlatformServiceProvider.ProcessArchitecture`).
- Growth: a new capability read is one `HostProbe` case, one `HostFact` case, and one `Probe` arm; a new process trait is one `HostTrait` row the snapshot's own fold already fills.
- Boundary: process facts include runtime architecture and system references; assembly paths admit through `Acceptance.Text` before any resolver mutation.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostTrait : ICapability<HostTrait> {
    public static readonly HostTrait DarkMode = new(key: "dark-mode");
    public static readonly HostTrait ServerHost = new(key: "server-host");
    public static readonly HostTrait PreRelease = new(key: "pre-release");
    public static readonly HostTrait MonoRuntime = new(key: "mono-runtime");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostProbe {
    private HostProbe() { }
    public sealed record Process : HostProbe;
    public sealed record Printers : HostProbe;
    public sealed record Scripting : HostProbe;
    public sealed record Entitlement : HostProbe;
    public sealed record Compute : HostProbe;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EntitlementFact {
    private EntitlementFact() { }
    public sealed record Granted(Option<string> Signature) : EntitlementFact;
    public sealed record Denied(Option<string> Reason) : EntitlementFact;

    internal static EntitlementFact Of(bool entitled, string? reason, string? signature) => entitled
        ? new Granted(Signature: HostEdge.NonEmpty(signature))
        : new Denied(Reason: HostEdge.NonEmpty(reason));
}

// --- [MODELS] --------------------------------------------------------------------------
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
    public string Architecture { get; }
    public CapabilitySet<HostTrait> Traits { get; }
    public Seq<string> ReferenceAssemblies { get; }
    public Seq<string> SearchPaths { get; }

    public static CapabilityLaw<HostTrait> Law => CapabilityLaw<HostTrait>.Open;
}

public sealed record PrintForm(string Name, Option<(double Width, double Height)> Extent);

public sealed record PrinterSlot(string Name, double HorizontalDpi, double VerticalDpi, Seq<PrintForm> Forms);

[ComplexValueObject]
public sealed partial class ScriptEngineSnapshot {
    public Seq<string> SearchPaths { get; }
    public Seq<string> RuntimeAssemblies { get; }
    public int ContextId { get; }
}

public sealed record HostEndpoint(string Path, Type Contract);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostFact {
    private HostFact() { }
    public sealed record ProcessCase(HostSnapshot Snapshot) : HostFact;
    public sealed record PrinterCase(Seq<PrinterSlot> Printers) : HostFact;
    public sealed record ScriptCase(ScriptEngineSnapshot Engine) : HostFact;
    public sealed record EntitlementCase(EntitlementFact Verdict) : HostFact;
    public sealed record ComputeCase(Seq<HostEndpoint> Endpoints) : HostFact;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblySource {
    private AssemblySource() { }
    public sealed record SearchFolder(string Path) : AssemblySource;
    public sealed record SearchFile(string Path) : AssemblySource;

    internal Fin<AssemblySource> Admit() => Switch(searchFolder: static row => Acceptance.Text(value: row.Path).Map<AssemblySource>(path => new SearchFolder(Path: path)),
        searchFile: static row => Acceptance.Text(value: row.Path).Map<AssemblySource>(path => new SearchFile(Path: path)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssemblyIntake {
    private AssemblyIntake() { }
    public sealed record FromPath(string Path) : AssemblyIntake;
    public sealed record FromStream(Stream Source) : AssemblyIntake;
    public sealed record FromName(AssemblyName Name) : AssemblyIntake;
}

public sealed record ExtensionTally(PluginKey Plugin, int Applied, Option<Error> Fault);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostFacts {
    public static Fin<HostFact> Probe(HostProbe probe) {
        return probe.Switch(process: static _ => Process(op: held).Map<HostFact>(static snapshot => new HostFact.ProcessCase(Snapshot: snapshot)),
            printers: static _ => Try.lift(() => Fin.Succ<HostFact>(value: new HostFact.PrinterCase(
                Printers: toSeq(HostUtils.GetPrinterNames()).Map(printer => new PrinterSlot(
                    Name: printer,
                    HorizontalDpi: HostUtils.GetPrinterDPI(printerName: printer, horizontal: true),
                    VerticalDpi: HostUtils.GetPrinterDPI(printerName: printer, horizontal: false),
                    Forms: toSeq(HostUtils.GetPrinterFormNames(printerName: printer)).Map(form => new PrintForm(
                        Name: form,
                        Extent: Admit.Probe<(double Width, double Height)>(() => (
                            HostUtils.GetPrinterFormSize(printer, form, out double width, out double height),
                            (width, height))))).Strict())).Strict()))).Run().Bind(static inner => inner),
            entitlement: static _ => Try.lift(() => Fin.Succ<HostFact>(value: new HostFact.EntitlementCase(
                Verdict: EntitlementFact.Of(
                    entitled: CloudHostUtils.IsEntitled,
                    reason: CloudHostUtils.DenyReason,
                    signature: CloudHostUtils.Signature)))).Run().Bind(static inner => inner),
            compute: static _ => Try.lift(() => Fin.Succ<HostFact>(value: new HostFact.ComputeCase(
                Endpoints: toSeq(HostUtils.GetCustomComputeEndpoints())
                    .Map(static row => new HostEndpoint(Path: row.Item1, Contract: row.Item2))
                    .Strict()))).Run().Bind(static inner => inner),
            scripting: static _ => Try.lift(() => Optional(PythonScript.Create())
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(PythonScript.Create))))
                .Bind(engine => Try.lift(() => Fin.Succ<HostFact>(value: new HostFact.ScriptCase(
                    Engine: ScriptEngineSnapshot.Create(
                        searchPaths: toSeq(PythonScript.SearchPaths).Strict(),
                        runtimeAssemblies: toSeq(PythonScript.RuntimeAssemblies())
                            .Map(static assembly => assembly.FullName ?? string.Empty).Strict(),
                        contextId: engine.ContextId)))).Run().Bind(static inner => inner))).Run().Bind(static inner => inner));
    }

    public static Fin<HostSnapshot> Process() =>
        Try.lift(() => {
            HostUtils.GetCurrentProcessInfo(processName: out string name, processVersion: out Version version);
            return Fin.Succ(value: HostSnapshot.Create(
                processName: name,
                processVersion: version,
                edition: HostUtils.OperatingSystemEdition,
                product: HostUtils.OperatingSystemProductName,
                build: HostUtils.OperatingSystemBuildNumber,
                installation: HostUtils.OperatingSystemInstallationType,
                language: HostUtils.CurrentOSLanguage,
                processors: HostUtils.GetSystemProcessorCount(),
                architecture: PlatformServiceProvider.ProcessArchitecture,
                traits: CapabilitySet<HostTrait>.Of(
                    [.. Seq(
                        (HostUtils.RunningInDarkMode, HostTrait.DarkMode),
                        (HostUtils.RunningOnServer, HostTrait.ServerHost),
                        (HostUtils.IsPreRelease, HostTrait.PreRelease),
                        (HostUtils.RunningInMono, HostTrait.MonoRuntime))
                        .Filter(static row => row.Item1)
                        .Map(static row => row.Item2)]),
                referenceAssemblies: toSeq(HostUtils.GetSystemReferenceAssemblies()).Strict(),
                searchPaths: toSeq(HostUtils.GetAssemblySearchPaths()).Strict()));
        }).Run().Bind(static inner => inner);
}

public static class HostAssemblies {
    public static Fin<ExtensionTally> Extend(PluginKey plugin, Seq<AssemblySource> sources) {
        return from admitted in sources.TraverseM(source => Admit.Need(source).Bind(row => row.Admit(op))).As()
               from outcome in HostThread.Run(
                   work: new HostWork<ExtensionTally>.Execute(Body: () => Fin.Succ(value: admitted.Fold(
                       new ExtensionTally(Plugin: plugin, Applied: 0, Fault: None),
                       (held, source) => held.Fault.IsSome
                           ? held
                           : Try.lift(() => source.Switch(
                                   searchFolder: static row => AssemblyResolver.AddSearchFolder(folder: row.Path),
                                   searchFile: static row => AssemblyResolver.AddSearchFile(file: row.Path))).Run().Bind(static inner => inner)
                               .Match(
                                   Succ: static _ => held with { Applied = held.Applied + 1 },
                                   Fail: fault => held with { Fault = Some(fault) })))))
               select outcome;
    }

    public static Fin<Assembly> Load(AssemblyIntake intake) {
        return intake.Switch(fromPath: static row => Acceptance.Text(value: row.Path)
                .Bind(path => Try.lift(() => Optional(HostUtils.LoadAssemblyFrom(path: path))
                    .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(HostUtils.LoadAssemblyFrom))))).Run().Bind(static inner => inner)),
            fromStream: static row => Try.lift(() => Optional(HostUtils.LoadAssemblyFromStream(stream: row.Source))
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(HostUtils.LoadAssemblyFromStream))))).Run().Bind(static inner => inner),
            fromName: static row => Try.lift(() => Optional(HostUtils.LoadAssemblyFromName(assemblyName: row.Name))
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(HostUtils.LoadAssemblyFromName))))).Run().Bind(static inner => inner));
    }
}
```

## [07]-[SCRIPTING]

- Owner: `ScriptRun` closes the execute-request family over the host scripting engine — source, file, file-in-scope, expression, compiled — and `HostScripts` guards compile, binding custody, and dispatch; `ScriptUnit` capsules the compiled handle and `ScriptOutcome` detaches ran-versus-value evidence.
- Owner: `NodeFunctions` resolves the node-in-code component table into detached `NodeFunction` descriptors; `Call` is the one invocation entry.
- Cases: `NodeCallShape` closes the flatten-versus-tree modality as a `[SmartEnum<bool>]` keyed on the host `keepTree` argument itself, so the ordinal and the native flag that were two spellings of one axis are one row.
- Entry: `HostScripts.Compile(string)` and `HostScripts.Run(ScriptRun, Seq<ScriptBinding>)`; `NodeFunctions.Find(string)` and `NodeFunctions.Census()`.
- Law: script execution admits the complete `ScriptRun` text family and every binding name BEFORE engine creation or host dispatch, then rides `HostThread.Run`; an execute returning `false` projects onto the carrier, expression absence rides `Option<object>`, and scripting-runtime exceptions convert inside the guarded window.
- Law: the unit carries its compiling engine — a code object executes only in the scope it compiled against, so a cross-engine run is unrepresentable rather than a silent empty-scope execution. A compiled case reuses that engine; every other case mints one fresh engine and both the bindings and the dispatch read the same instance.
- Law: a `NodeFunction` detaches name, namespace, description, component id, and the input and output rosters at resolution; the live `ComponentFunctionInfo` stays private, and every invocation returns a `NodeReturn` carrying values AND warnings — the warning-silencing host variants are the discarded-evidence forms this surface never spells.
- Packages: `Rasm/Domain/results` ; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`PythonScript.Create`/`Compile`/`SetVariable`/`ExecuteScript`/`ExecuteFile`/`ExecuteFileInScope`/`EvaluateExpression`/`ContextId`, `PythonCompiledCode.Execute`, `Components.FindComponent`/`NodeInCodeFunctions`, `ComponentFunctionInfo.Name`/`Namespace`/`Description`/`ComponentGuid`/`InputNames`/`InputsOptional`/`OutputNames`/`Evaluate`).
- Growth: a new execute modality is one `ScriptRun` case with one `Admit` leg and one dispatch arm; a new call shape is one `NodeCallShape` row.
- Boundary: `PythonScript`, `PythonCompiledCode`, and `ComponentFunctionInfo` never cross a public signature except inside `ScriptUnit`, whose whole contract is that pairing.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    internal Fin<ScriptRun> Admit() => Switch(source: static row => Acceptance.Text(value: row.Script).Map<ScriptRun>(script => new Source(Script: script)),
        file: static row => Acceptance.Text(value: row.Path).Map<ScriptRun>(path => new File(Path: path)),
        fileInScope: static row => Acceptance.Text(value: row.Path).Map<ScriptRun>(path => new FileInScope(Path: path)),
        expression: static (row) =>
            from statements in Acceptance.Text(value: row.Statements)
            from formula in Acceptance.Text(value: row.Formula)
            select (ScriptRun)new Expression(Statements: statements, Formula: formula),
        compiled: static row => Fin.Succ<ScriptRun>(row));
}

[SmartEnum<bool>]
public sealed partial class NodeCallShape {
    public static readonly NodeCallShape Flatten = new(key: false);
    public static readonly NodeCallShape KeepTree = new(key: true);
}

// --- [MODELS] --------------------------------------------------------------------------
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

    internal static Fin<NodeFunction> Of(ComponentFunctionInfo info) =>
        Try.lift(() => new NodeFunction(
            info: info,
            name: info.Name,
            space: info.Namespace,
            description: info.Description,
            component: info.ComponentGuid,
            inputs: toSeq(info.InputNames).Strict(),
            optionalInputs: toSeq(info.InputsOptional).Strict(),
            outputs: toSeq(info.OutputNames).Strict())).Run();

    public Fin<NodeReturn> Call(Seq<object> arguments, NodeCallShape shape) {
        NodeFunction self = this;
        return from mode in Admit.Need(shape)
               from produced in Try.lift(() => {
                   object[] values = self.info.Evaluate(
                       args: arguments.AsEnumerable(), keepTree: mode.Key, warnings: out string[] warnings);
                   return Optional(values)
                       .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(ComponentFunctionInfo.Evaluate))))
                       .Map(rows => new NodeReturn(Values: toSeq(rows).Strict(), Warnings: toSeq(warnings ?? []).Strict()));
               }).Run().Bind(static inner => inner)
               select produced;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostScripts {
    public static Fin<ScriptUnit> Compile(string script) {
        return from source in Acceptance.Text(value: script)
               from engine in Engine()
               from code in Try.lift(() => Optional(engine.Compile(script: source))
                   .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(PythonScript.Compile))))).Run().Bind(static inner => inner)
               select new ScriptUnit(Code: code, Engine: engine);
    }

    public static Fin<ScriptOutcome> Run(ScriptRun run, Seq<ScriptBinding> bindings = default) {
        return from admitted in run.Admit()
               from prepared in bindings.TraverseM(binding =>
                       from row in Admit.Need(binding)
                       from name in Acceptance.Text(value: row.Name)
                       select row with { Name = name })
                   .As()
               from engine in admitted is ScriptRun.Compiled held ? Fin.Succ(value: held.Unit.Engine) : Engine()
               from outcome in HostThread.Run(
                   work: new HostWork<ScriptOutcome>.Execute(Body: () =>
                       from _ in prepared.TraverseM(binding => Try.lift(() =>
                               engine.SetVariable(name: binding.Name, value: binding.Value)).Run().Bind(static inner => inner))
                           .As()
                       from settled in admitted.Switch(
                           (Held: op, Engine: engine),
                           source: static (state, row) => Ran(
                               ran: () => state.Engine.ExecuteScript(script: row.Script),
                               member: nameof(PythonScript.ExecuteScript), op: state.Held),
                           file: static (state, row) => Ran(
                               ran: () => state.Engine.ExecuteFile(path: row.Path),
                               member: nameof(PythonScript.ExecuteFile), op: state.Held),
                           fileInScope: static (state, row) => Ran(
                               ran: () => state.Engine.ExecuteFileInScope(path: row.Path),
                               member: nameof(PythonScript.ExecuteFileInScope), op: state.Held),
                           expression: static (state, row) => Try.lift(() => Fin.Succ<ScriptOutcome>(
                               value: new ScriptOutcome.Value(Result: Optional(state.Engine.EvaluateExpression(
                                   statements: row.Statements, expression: row.Formula))))).Run().Bind(static inner => inner),
                           compiled: static (state, row) => Try.lift(() => {
                               row.Unit.Code.Execute(scope: state.Engine);
                               return Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran());
                           }).Run().Bind(static inner => inner))
                       select settled))
               select outcome;
    }

    private static Fin<ScriptOutcome> Ran(Func<bool> ran, string member) =>
        Try.lift(() => ran()
            ? Fin.Succ<ScriptOutcome>(value: new ScriptOutcome.Ran())
            : Fin.Fail<ScriptOutcome>(error: new UiFault.HostRejected(Detail: member))).Run().Bind(static inner => inner);

    private static Fin<PythonScript> Engine() =>
        Try.lift(() => Optional(PythonScript.Create()).ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(nameof(PythonScript.Create))))).Run().Bind(static inner => inner);
}

public static class NodeFunctions {
    public static Fin<NodeFunction> Find(string fullName) {
        return from admitted in Acceptance.Text(value: fullName)
               from info in Try.lift(() => Optional(Components.FindComponent(admitted)).ToFin(Fail: new KernelFault.MissingContext())).Run().Bind(static inner => inner)
               from function in NodeFunction.Of(info: info)
               select function;
    }

    public static Fin<Seq<NodeFunction>> Census() {
        return Try.lift(() => Components.NodeInCodeFunctions).Run()
            .Bind(table => toSeq(table.GetDynamicMembers())
                .TraverseM(info => NodeFunction.Of(info: info))
                .As()
                .Map(static rows => rows.Strict()));
    }
}
```

## [08]-[SKIN]

- Owner: `SkinProgram` carries the icon, product name, and one `SkinPhase` hook; `ShellSkin` adapts the complete `Rhino.Runtime.Skin` load-phase surface onto it; `ShellHooks` binds the phase route on the registry.
- Cases: ten `SkinPhase` rows mirroring the host's ten load-phase virtuals, each carrying exactly the payload its virtual supplies.
- Entry: `ShellHooks.Mount(PluginKey)` registers `rasm.rhino.hostui.skin` as a TYPED `HookBinding` on `MountRegistry`; a skin observer binds by point name and hands the granted program to its `ShellSkin` constructor.
- Law: every `ShellSkin` override chains the base member first, then routes its `SkinPhase` case; hook faults ring in the adapter's own bounded cell and never re-enter the host load sequence.
- Law: ask and grant stay TYPED. The binding is `HookBinding<RhinoPoint, PluginKey, Func<SkinPhase, Fin<Unit>>, SkinProgram>` and the kernel `HookMounts` answers the typed bind beneath `MountRegistry`'s name-addressed discovery, so no `Type` pair and no `object` cast stands on the resolve path and a mismatched ask fails by name at the seat rather than at a cast.
- Packages: `Rasm/Domain/results` (`Ring<T>`, `Lease<T>`), `Domain/hooks` (`HookBinding`); `Rasm.Rhino/Document/events` (`RhinoPoint`, `PluginKey`, `MountRegistry`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`Skin` and its ten load-phase virtuals, `MainRhinoIcon`, `ApplicationName`).
- Growth: a new load phase is one `SkinPhase` case and one sealed override chaining its base.
- Boundary: `Rhino.Runtime.Skin` is host-constructed and never crosses a public signature; the adapter is the only derivation.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SkinProgram(
    Option<DrawingBitmap> Icon, Option<string> Product, [property: IgnoreMember] Func<SkinPhase, Fin<Unit>> Phase) {
    public static readonly SkinProgram Inert = new(Icon: None, Product: None, Phase: static _ => Fin.Succ(value: unit));
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ShellSkin : Skin {
    private readonly SkinProgram program;
    private readonly Ring<Error> faults = ShellFaults.Ring();

    protected ShellSkin(SkinProgram program) {
        this.program = program;
    }

    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

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

    private void Route(SkinPhase phase) => ignore(Try.lift(() => program.Phase(phase)).Run().Bind(static inner => inner)
        .IfFail(failure => ignore(faults.Park(item: failure))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ShellHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin) =>
        MountRegistry.Mount(
            binding: new HookBinding<RhinoPoint, PluginKey, Func<SkinPhase, Fin<Unit>>, SkinProgram>(
                Point: RhinoPoint.HostUiSkin,
                Owner: plugin,
                Bind: static phase => Fin.Succ(value: SkinProgram.Inert with { Phase = phase })));
}
```

## [09]-[ACCOUNTS]

- Owner: `TokenAsk` closes the accounts request family and `Accounts.Ask` dispatches it whole inside `RhinoAccountsManager.ExecuteProtectedCodeAsync`, so `SecretKey` custody is structurally confined to the protected callback; `TokenLease` holds the live token pair and hands out only detached `OpenIdEvidence`/`OauthEvidence`.
- Owner: `HostEndpoints` binds the append-only host endpoint roster under process-lifetime custody because the host publishes no unregister.
- Cases: `TokenAsk` is acquire, scoped acquire, or cached read — three cases over ONE base `ClientId` column, because every arm names the client it asks for and a three-arm switch to read it back was the column standing outside the union.
- Entry: `Accounts.Ask(TokenAsk, Option<Action<LoginPulse>>, Option<Env>)` answers `ValueTask<Fin<TokenLease>>`; `TokenLease.Refresh`/`Revoke` answer `ValueTask<Fin<Unit>>`.
- Auto: the accounts pipeline is ASYNCHRONOUS because the host's protected-code entry is. `Try.lift(Func<ValueTask<Fin<T>>>).Run().Bind(static inner => inner)` is the one async funnel and captures both the thrown and the faulted-task shapes onto the same fault categories, so the three `Task.Wait()` sync-over-async bridges have no spelling left and no caller blocks a thread on a completed task to reach a value it already has.
- Law: interactive login confines to first acquisition; `TryCached` reads the secure token cache with no server call and no UI, so a headless composition answers it. `showUI` stays false on the scoped overload — the host raises its own browser flow off the progress callback, and the caller observes it as detached `LoginPulse` facts.
- Law: login progress crosses as detached `LoginPulse` facts keyed on the `LoginPhase` vocabulary through `FactoryBridge.Row` — a raw `RhinoAccoountsProgressInfo` (the host's own doubled-o spelling) never leaves the dispatch closure, and an unrostered host state reads `Other` rather than refusing a login that is otherwise proceeding.
- Law: detached evidence carries FACTS, never verdicts a clock derives. `OauthEvidence` publishes its `Instant` expiry and the caller decides against its own timeline; `TokenLease.Live` re-reads the LIVE token, which is the only value that can answer. NAMED LOSS: the host's own `IsExpired` verdict at detach time — a stored copy disagreed with the lease the moment the record was held, and the expiry instant it derived from is the fact both readings share.
- Law: refresh and revoke consume the lease's OWN held tokens — a detached evidence record cannot reconstruct them, which is the confinement working.
- Packages: `Rasm/Domain/results` (`Cell`), `Domain/validation` (`FactoryBridge.Row`); `Rasm/Analysis/query` (`Env`); NodaTime (`Instant`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`RhinoAccountsManager.ExecuteProtectedCodeAsync`/`GetAuthTokensAsync`/`TryGetAuthTokens`/`UpdateOpenIDConnectTokenAsync`/`RevokeAuthTokenAsync`, `IOpenIDConnectToken`, `IOAuth2Token`, `SecretKey`, `RhinoAccoountsProgressInfo`, `ProgressState`, `HostUtils.RegisterComputeEndpoint`).
- Growth: a new request modality is one `TokenAsk` case and one dispatch arm; a new login phase is one `LoginPhase` row keyed on the host enum.
- Boundary: `SecretKey` never leaves the protected callback and the live token interfaces never leave the lease.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<ProgressState>]
public sealed partial class LoginPhase {
    public static readonly LoginPhase AwaitingLogin = new(key: ProgressState.AwaitingLogin);
    public static readonly LoginPhase RetrievingTokens = new(key: ProgressState.RetrievingTokens);
    public static readonly LoginPhase AwaitingRedirect = new(key: ProgressState.AwaitingRedirect);
    public static readonly LoginPhase Other = new(key: ProgressState.Other);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TokenAsk(string ClientId) {
    public sealed record Acquire(string ClientId, string ClientSecret) : TokenAsk(ClientId);
    public sealed record AcquireScoped(
        string ClientId, string ClientSecret, Seq<string> Scopes, Option<string> Prompt, Option<int> MaxAge) : TokenAsk(ClientId);
    public sealed record TryCached(string ClientId, Seq<string> Scopes) : TokenAsk(ClientId);

    internal Fin<TokenAsk> Admit() => Switch(acquire: static row =>
            from id in Acceptance.Text(value: row.ClientId)
            from secret in Acceptance.Text(value: row.ClientSecret)
            select (TokenAsk)new Acquire(ClientId: id, ClientSecret: secret),
        acquireScoped: static (row) =>
            from id in Acceptance.Text(value: row.ClientId)
            from secret in Acceptance.Text(value: row.ClientSecret)
            from scopes in row.Scopes.TraverseM(scope => Acceptance.Text(value: scope)).As()
            select (TokenAsk)new AcquireScoped(
                ClientId: id, ClientSecret: secret, Scopes: scopes.Strict(), Prompt: row.Prompt, MaxAge: row.MaxAge),
        tryCached: static (row) =>
            from id in Acceptance.Text(value: row.ClientId)
            from scopes in row.Scopes.TraverseM(scope => Acceptance.Text(value: scope)).As()
            select (TokenAsk)new TryCached(ClientId: id, Scopes: scopes.Strict()));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LoginPulse(LoginPhase Phase, Option<string> Description);

public sealed record OpenIdEvidence(
    string Subject,
    string Issuer,
    string Audience,
    Option<Instant> Issued,
    Option<Instant> Expires,
    Seq<string> Emails,
    Option<bool> EmailVerified,
    Option<string> Name);

public sealed record OauthEvidence(Option<Instant> Expires, Seq<string> Scopes);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class TokenLease : IDisposable {
    private readonly Atom<Option<(IOpenIDConnectToken OpenId, IOAuth2Token Oauth)>> held;
    private readonly string clientId;

    internal TokenLease(IOpenIDConnectToken openId, IOAuth2Token oauth, string clientId) {
        held = Atom(Some((openId, oauth)));
        (this.clientId, this.op) = (clientId);
    }

    public string ClientId => clientId;

    public Fin<OpenIdEvidence> OpenId() => Read(
        project: static pair => new OpenIdEvidence(
            Subject: pair.OpenId.Sub,
            Issuer: pair.OpenId.Iss,
            Audience: pair.OpenId.Aud,
            Issued: Moment(pair.OpenId.Iat),
            Expires: Moment(pair.OpenId.Exp),
            Emails: toSeq(pair.OpenId.Emails).Strict(),
            EmailVerified: Optional(pair.OpenId.EmailVerified),
            Name: HostEdge.NonEmpty(pair.OpenId.Name)));

    public Fin<OauthEvidence> Oauth() => Read(
        project: static pair => new OauthEvidence(
            Expires: Moment(pair.Oauth.Exp),
            Scopes: toSeq(pair.Oauth.Scope).Strict()));

    public bool Live => held.Value.Map(static pair => !pair.Oauth.IsExpired).IfNone(false);

    public ValueTask<Fin<Unit>> Refresh() {
        return HostEdge.Captured(CancellationToken.None, async _ => {
            if (held.Value.Case is not (IOpenIDConnectToken openId, IOAuth2Token oauth)) {
                return Fin.Fail<Unit>(error: admitted.MissingContext());
            }
            await RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: async secret => {
                IOpenIDConnectToken updated = await RhinoAccountsManager.UpdateOpenIDConnectTokenAsync(
                    currentToken: openId, oauth2Token: oauth,
                    secretKey: secret, cancellationToken: CancellationToken.None).ConfigureAwait(false);
                _ = held.Swap(current => current.Map(row => (updated, row.Oauth)));
            }).ConfigureAwait(false);
            return Fin.Succ(value: unit);
        });
    }

    public ValueTask<Fin<Unit>> Revoke() {
        return HostEdge.Captured(CancellationToken.None, async _ => {
            if (held.Value.Case is not (IOpenIDConnectToken _, IOAuth2Token oauth)) {
                return Fin.Fail<Unit>(error: admitted.MissingContext());
            }
            await RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: secret =>
                RhinoAccountsManager.RevokeAuthTokenAsync(
                    oauth2Token: oauth, secretKey: secret, cancellationToken: CancellationToken.None))
                .ConfigureAwait(false);
            return Fin.Succ(value: ignore(Cell.Take(held)));
        });
    }

    public void Dispose() => ignore(Cell.Take(held));

    private static Option<Instant> Moment(DateTime? stamp) =>
        Optional(stamp).Map(static held => Instant.FromDateTimeUtc(held.ToUniversalTime()));

    private Fin<T> Read<T>(Func<(IOpenIDConnectToken OpenId, IOAuth2Token Oauth), T> project) {
        return held.Value.ToFin(Fail: new KernelFault.MissingContext())
            .Bind(pair => Try.lift(() => project(pair)).Run());
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Accounts {
    public static ValueTask<Fin<TokenLease>> Ask(
        TokenAsk ask, Option<Action<LoginPulse>> progress = default, Option<Env> env = default) {
        return ask.Admit().Match(
            Succ: request => Dispatch(request: request, progress: progress, env: env),
            Fail: fault => ValueTask.FromResult(Fin.Fail<TokenLease>(error: fault)));
    }

    private static ValueTask<Fin<TokenLease>> Dispatch(
        TokenAsk request, Option<Action<LoginPulse>> progress, Option<Env> env) {
        CancellationToken cancel = env.Map(static held => held.Cancellation).IfNone(CancellationToken.None);
        return HostEdge.Captured(cancel, async token => {
            LoginProgress pulse = new(info => progress.Iter(tap => ignore(Try.lift(() => HostEdge.Side(() => tap(new LoginPulse(
                Phase: FactoryBridge.Row<ProgressState, LoginPhase>(info.State).IfFail(LoginPhase.Other),
                Description: HostEdge.NonEmpty(info.Description))))).Run())));
            Atom<Option<(IOpenIDConnectToken OpenId, IOAuth2Token Oauth)>> landed =
                Atom(Option<(IOpenIDConnectToken, IOAuth2Token)>.None);
            await RhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode: async secret => {
                Tuple<IOpenIDConnectToken, IOAuth2Token> pair = await request.Switch(
                    (Secret: secret, Cancel: token, Pulse: pulse),
                    acquire: static (state, row) => RhinoAccountsManager.GetAuthTokensAsync(
                        clientId: row.ClientId, clientSecret: row.ClientSecret,
                        secretKey: state.Secret, cancellationToken: state.Cancel),
                    acquireScoped: static (state, row) => RhinoAccountsManager.GetAuthTokensAsync(
                        clientId: row.ClientId, clientSecret: row.ClientSecret, scope: row.Scopes.AsEnumerable(),
                        prompt: HostEdge.Slot(row.Prompt), maxAge: HostEdge.Nullable(row.MaxAge),
                        showUI: false, progress: state.Pulse,
                        secretKey: state.Secret, cancellationToken: state.Cancel),
                    tryCached: static (state, row) => Task.FromResult(row.Scopes.IsEmpty
                        ? RhinoAccountsManager.TryGetAuthTokens(clientId: row.ClientId, secretKey: state.Secret)
                        : RhinoAccountsManager.TryGetAuthTokens(
                            clientId: row.ClientId, scope: row.Scopes.AsEnumerable(), secretKey: state.Secret)))
                    .ConfigureAwait(false);
                _ = Cell.Seat(landed, () => (pair.Item1, pair.Item2));
            }).ConfigureAwait(false);
            return landed.Value
                .Filter(static row => row.OpenId is not null && row.Oauth is not null)
                .ToFin(Fail: new KernelFault.MissingContext())
                .Map(row => new TokenLease(openId: row.OpenId, oauth: row.Oauth, clientId: request.ClientId));
        });
    }
}

public static class HostEndpoints {
    public static Fin<HostEndpoint> Register(string path, Type contract) {
        return from admitted in Acceptance.Text(value: path)
               from row in Try.lift(() => {
                   HostUtils.RegisterComputeEndpoint(endpointPath: admitted, t: contract);
                   return Fin.Succ(value: new HostEndpoint(Path: admitted, Contract: contract));
               }).Run().Bind(static inner => inner)
               select row;
    }
}
```

## [10]-[CALLBACKS]

- Owner: `NamedKind` is the ONE host-schema roster — one row per `NamedParametersEventArgs` member pair, carrying its payload case, its read, and its optional write; `NamedValue` is the payload family those rows carry; `NamedBag` serializes native common objects into detached payloads before they enter the map.
- Owner: `NamedRegistry` holds plugin-claimed wire-name custody as an instance the capsule seats; `NamedCallbacks` is the entry pair over it.
- Entry: `NamedCallbacks.Register(NamedRegistry, PluginKey, string, Seq<NamedSlot>, Func<NamedBag, Fin<NamedBag>>, Action<Error>)` seats one host callback under a wire name; `Execute(string, NamedBag, Seq<NamedSlot>, Option<Env>)` mints, executes, and detaches the response in one crossing.
- Auto: the roster is declared ONCE. The host publishes each named parameter as a `TryGet*`/`Set` PAIR, so one `NamedKind` row carries both legs beside the `NamedValue` case they move, and the twenty-arm write switch that enumerated the same correspondence a second time has no spelling left. NAMED LOSS: compile-time exhaustiveness over the case-to-row map — bought back by `NamedValue.Kind` returning `Option<NamedKind>`, so a case with no row refuses by name at the ONE read rather than falling through a switch.
- Auto: a read-only parameter is a row whose write column is ABSENT, never a row whose write arm always fails. `ViewportInfo` has a `TryGetViewport` and no `Set` counterpart, so `Camera` carries `None` and the refusal names the row at the one site that would have written it.
- Law: wire names are plugin-claimed custody — `HostUtils.RegisterNamedCallback` silently replaces a prior handler, so `Register` claims the name in the registry under a fresh claim token keyed on `PluginKey` before the host call; ANY registration against a live claim faults typed — foreign or same-plugin alike, because a silent replacement would leave the prior `Subscription`'s detach removing the new host row — and detach releases exactly its own claim with the host row.
- Law: the registry is a VALUE the capsule publishes, never a process static. `NamedCallbacks` is reached once per plug-in from the load root, which holds the capsule, so the dependency arrives as a parameter; only `HostThread`'s marshal ledger — reached from every sub-domain and every page — arrives at a seat.
- Law: `NamedSlot.Admit` revalidates one complete schema before native arguments exist, and the schema is a keyed carrier, so a duplicate key is unrepresentable rather than caught by a distinct-count guard.
- Law: execution cancellation reads the kernel `Env`; its direct poll carries `Errors.Cancelled`, while caught cancellation remains `Try.lift` custody.
- Law: a decode or write fold holds its own PREFIX for release. A traverse abandons the values it already rehydrated, and every one of them is a native handle this boundary owns until the synchronous host call ends, so the fold accumulates and `Custody.Release` reverses and drains it.
- Packages: `Rasm/Domain/results` (`Cell`, `Transition`), `Domain/validation` (`Admit.Probe`); `Rasm/Interaction/dispatch` (`UiFault`); `Rasm/Analysis/query` (`Env`); `Rasm.Rhino/Document/events` (`PluginKey`), `Document/lifetime` (`Subscription`), kernel `Domain/results` (`Custody`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`HostUtils.RegisterNamedCallback`/`RemoveNamedCallback`/`ExecuteNamedCallback`, `NamedParametersEventArgs` and its `TryGet*`/`Set` roster), `api-rhinocommon-fileio.md` (`SerializationOptions`), `api-rhinocommon-geometry.md` (`CommonObject.ToJSON`/`FromJSON`), `api-rhinocommon-meshing.md` (`MeshingParameters.ToEncodedString`/`FromEncodedString`).
- Growth: a new host parameter is ONE `NamedKind` row and ONE `NamedValue` case; nothing else edits.
- Boundary: geometry, viewport, and meshing rows cross as serialized values, and the three live-handle readers the host also publishes — `TryGetObjRefs`, `TryGetRhinoObjects`, and the native window-handle pair — carry document handles and raw pointers this boundary's detachment law forecloses; object identity crosses on the `IdSet` row instead. `NamedLease` owns every rehydrated common object until the synchronous host call ends.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    internal Option<NamedKind> Kind => NamedKind.Rows.Value.Find(GetType());
}

[SmartEnum<int>]
public sealed partial class NamedKind {
    public static readonly NamedKind Text = new(key: 0, payload: typeof(NamedValue.Text),
        read: static (args, key) => Admit.Probe<string>(() => (args.TryGetString(out string value), value))
            .Map<NamedValue>(value => new NamedValue.Text(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Text>(value, row => args.Set(row.Value))));
    public static readonly NamedKind TextSet = new(key: 1, payload: typeof(NamedValue.TextSet),
        read: static (args, key) => Admit.Probe<string[]>(() => (args.TryGetStrings(out string[] values), values))
            .Map<NamedValue>(values => new NamedValue.TextSet(Values: toSeq(values).Strict())),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.TextSet>(value, row => args.Set(row.Values.AsEnumerable()))));
    public static readonly NamedKind Flag = new(key: 2, payload: typeof(NamedValue.Flag),
        read: static (args, key) => Admit.Probe<bool>(() => (args.TryGetBool(out bool value), value))
            .Map<NamedValue>(value => new NamedValue.Flag(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Flag>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Number = new(key: 3, payload: typeof(NamedValue.Number),
        read: static (args, key) => Admit.Probe<int>(() => (args.TryGetInt(out int value), value))
            .Map<NamedValue>(value => new NamedValue.Number(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Number>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Count = new(key: 4, payload: typeof(NamedValue.Count),
        read: static (args, key) => Admit.Probe<uint>(() => (args.TryGetUnsignedInt(out uint value), value))
            .Map<NamedValue>(value => new NamedValue.Count(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Count>(value, row => args.Set(row.Value))));
    public static readonly NamedKind CountSet = new(key: 5, payload: typeof(NamedValue.CountSet),
        read: static (args, key) => Admit.Probe<uint[]>(() => (args.TryGetUints(out uint[] values), values))
            .Map<NamedValue>(values => new NamedValue.CountSet(Values: toSeq(values).Strict())),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.CountSet>(value, row => args.Set(row.Values.AsEnumerable()))));
    public static readonly NamedKind Scalar = new(key: 6, payload: typeof(NamedValue.Scalar),
        read: static (args, key) => Admit.Probe<double>(() => (args.TryGetDouble(out double value), value))
            .Map<NamedValue>(value => new NamedValue.Scalar(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Scalar>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Id = new(key: 7, payload: typeof(NamedValue.Id),
        read: static (args, key) => Admit.Probe<Guid>(() => (args.TryGetGuid(out Guid value), value))
            .Map<NamedValue>(value => new NamedValue.Id(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Id>(value, row => args.Set(row.Value))));
    public static readonly NamedKind IdSet = new(key: 8, payload: typeof(NamedValue.IdSet),
        read: static (args, key) => Admit.Probe<Guid[]>(() => (args.TryGetGuids(out Guid[] values), values))
            .Map<NamedValue>(values => new NamedValue.IdSet(Values: toSeq(values).Strict())),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.IdSet>(value, row => args.Set(row.Values.AsEnumerable()))));
    public static readonly NamedKind Paint = new(key: 9, payload: typeof(NamedValue.Paint),
        read: static (args, key) => Admit.Probe<DrawingColor>(() => (args.TryGetColor(out DrawingColor value), value))
            .Map<NamedValue>(value => new NamedValue.Paint(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Paint>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Cell = new(key: 10, payload: typeof(NamedValue.Cell),
        read: static (args, key) => Admit.Probe<DrawingPoint>(() => (args.TryGetPoint2i(out DrawingPoint value), value))
            .Map<NamedValue>(value => new NamedValue.Cell(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Cell>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Point = new(key: 11, payload: typeof(NamedValue.Point),
        read: static (args, key) => Admit.Probe<Point3d>(() => (args.TryGetPoint(out Point3d value), value))
            .Map<NamedValue>(value => new NamedValue.Point(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Point>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Vector = new(key: 12, payload: typeof(NamedValue.Vector),
        read: static (args, key) => Admit.Probe<Vector3d>(() => (args.TryGetVector(out Vector3d value), value))
            .Map<NamedValue>(value => new NamedValue.Vector(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Vector>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Segment = new(key: 13, payload: typeof(NamedValue.Segment),
        read: static (args, key) => Admit.Probe<Line>(() => (args.TryGetLine(out Line value), value))
            .Map<NamedValue>(value => new NamedValue.Segment(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Segment>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Sweep = new(key: 14, payload: typeof(NamedValue.Sweep),
        read: static (args, key) => Admit.Probe<Arc>(() => (args.TryGetArc(out Arc value), value))
            .Map<NamedValue>(value => new NamedValue.Sweep(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Sweep>(value, row => args.Set(row.Value))));
    public static readonly NamedKind Frame = new(key: 15, payload: typeof(NamedValue.Frame),
        read: static (args, key) => Admit.Probe<Plane>(() => (args.TryGetPlane(out Plane value), value))
            .Map<NamedValue>(value => new NamedValue.Frame(Value: value)),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.Frame>(value, row => args.Set(row.Value))));
    public static readonly NamedKind PointSet = new(key: 16, payload: typeof(NamedValue.PointSet),
        read: static (args, key) => Admit.Probe<Point3d[]>(() => (args.TryGetPoints(out Point3d[] values), values))
            .Map<NamedValue>(values => new NamedValue.PointSet(Values: toSeq(values).Strict())),
        write: Some<NamedWrite>(static (args, key, value, op) => Set<NamedValue.PointSet>(value, row => args.Set([.. row.Values]))));
    public static readonly NamedKind Geometry = new(key: 17, payload: typeof(NamedValue.Geometry),
        read: static (args, key) => Admit.Probe<GeometryBase[]>(() => (args.TryGetGeometry(out GeometryBase[] values), values))
            .Map<NamedValue>(values => new NamedValue.Geometry(
                Values: toSeq(values).Map(static value => value.ToJSON(new SerializationOptions())).Strict())),
        write: Some<NamedWrite>(static (args, key, value, op) => value is NamedValue.Geometry row
            ? from rehydrated in Rehydrate<GeometryBase>(
                  encoded: row.Values, decode: static text => CommonObject.FromJSON(text) as GeometryBase)
              from releases in Transfer(values: rehydrated, write: () => args.Set(rehydrated.AsEnumerable()))
              select releases
            : Fin.Fail<Seq<Func<Fin<Unit>>>>(error: new KernelFault.InvalidInput(Axis: Some()))));
    public static readonly NamedKind Camera = new(key: 18, payload: typeof(NamedValue.Camera),
        read: static (args, key) => Admit.Probe<ViewportInfo>(() => (args.TryGetViewport(out ViewportInfo value), value))
            .Map<NamedValue>(value => new NamedValue.Camera(Value: value.ToJSON(new SerializationOptions()))),
        write: Option<NamedWrite>.None);
    public static readonly NamedKind Meshing = new(key: 19, payload: typeof(NamedValue.Meshing),
        read: static (args, key) => Admit.Probe<MeshingParameters>(() => (args.TryGetMeshParameters(out MeshingParameters value), value))
            .Map<NamedValue>(value => new NamedValue.Meshing(Value: value.ToEncodedString())),
        write: Some<NamedWrite>(static (args, key, value, op) => value is NamedValue.Meshing row
            ? from rehydrated in Rehydrate<MeshingParameters>(
                  encoded: Seq(row.Value), decode: MeshingParameters.FromEncodedString)
              from single in rehydrated.Head.ToFin(Fail: new KernelFault.InvalidResult(Detail: Some()))
              from releases in Transfer(values: rehydrated, write: () => args.Set(single))
              select releases
            : Fin.Fail<Seq<Func<Fin<Unit>>>>(error: new KernelFault.InvalidInput(Axis: Some()))));

    internal delegate Fin<Seq<Func<Fin<Unit>>>> NamedWrite(NamedParametersEventArgs args, string key, NamedValue value);

    public Type Payload { get; }
    internal Option<NamedWrite> Write { get; }

    [UseDelegateFromConstructor]
    internal partial Option<NamedValue> Read(NamedParametersEventArgs args, string key);

    internal static readonly Lazy<HashMap<Type, NamedKind>> Rows =
        new(static () => toHashMap(toSeq(Items).Map(static row => (row.Payload, row))));

    private static Fin<Seq<Func<Fin<Unit>>>> Set<TCase>(NamedValue value, string key, Action<TCase> write)
        where TCase : NamedValue =>
        value is TCase row
            ? Try.lift(() => write(row)).Run().Bind(static inner => inner).Map(static _ => Seq<Func<Fin<Unit>>>())
            : Fin.Fail<Seq<Func<Fin<Unit>>>>(error: new KernelFault.InvalidInput(Axis: Some()));

    private static Fin<Seq<T>> Rehydrate<T>(Seq<string> encoded, Func<string, T?> decode)
        where T : class, IDisposable {
        (Seq<T> Values, Option<Error> Fault) state = encoded.Fold(
            (Values: Seq<T>(), Fault: Option<Error>.None),
            (held, source) => held.Fault.IsSome
                ? held
                : Try.lift(() => Optional(decode(source)).ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(typeof(T).Name)))).Run().Bind(static inner => inner).Match(
                    Succ: value => (held.Values.Add(value), Option<Error>.None),
                    Fail: fault => (held.Values, Some(fault))));
        return state.Fault.Match(
            Some: fault => Fin.Fail<Seq<T>>(error: fault)
                .Rollback(held: state.Values, release: static value => Fin.Succ(value: ignore(fun(value.Dispose)()))),
            None: () => Fin.Succ(value: state.Values));
    }

    private static Fin<Seq<Func<Fin<Unit>>>> Transfer<T>(Seq<T> values, Action write) where T : IDisposable {
        Seq<Func<Fin<Unit>>> releases = values.Rev()
            .Map(value => (Func<Fin<Unit>>)(() => Fin.Succ(value: ignore(fun(value.Dispose)()))));
        return Try.lift(write).Run()
            .Map(_ => releases)
            .Rollback(release: () => HostThread.Release(releases: releases));
    }
}

[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class SlotPresence {
    public static readonly SlotPresence Optional = new(key: false);
    public static readonly SlotPresence Required = new(key: true);
}

[ComplexValueObject]
public sealed partial class NamedSlot {
    public string Key { get; }
    public NamedKind Kind { get; }
    public SlotPresence Presence { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref NamedKind kind,
        ref SlotPresence presence) =>
        validationError = string.IsNullOrWhiteSpace(key)
            ? new ValidationError(message: $"{nameof(Key)} is blank.")
            : kind is null
                ? new ValidationError(message: $"{nameof(Kind)} is absent.")
                : presence is null
                    ? new ValidationError(message: $"{nameof(Presence)} is absent.")
                    : null;

    internal static Fin<HashMap<string, NamedSlot>> Admit(Seq<NamedSlot> slots) =>
        slots.TraverseM(slot => Admit.Need(slot).Bind(row => FactoryBridge.Accept<NamedSlot>(
                Validate(kind: row.Kind, presence: row.Presence, obj: out NamedSlot? admitted),
                admitted)))
            .As()
            .Bind(admitted => admitted.Fold(
                Fin.Succ(value: HashMap<string, NamedSlot>()),
                (held, row) => held.Bind(schema => schema.ContainsKey(row.Key)
                    ? Fin.Fail<HashMap<string, NamedSlot>>(error: new KernelFault.InvalidInput(Axis: Some(row.Key)))
                    : Fin.Succ(value: schema.Add(row.Key, row)))));
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed class NamedLease {
    private readonly Seq<Func<Fin<Unit>>> releases;
    private readonly Atom<LeaseState<Unit>> state = Atom<LeaseState<Unit>>(new LeaseState<Unit>.Live(Held: unit));

    internal NamedLease(Seq<Func<Fin<Unit>>> releases) => this.releases = releases;

    internal NamedLease Append(Func<Fin<Unit>> release) => new(releases: releases.Add(release));

    internal Fin<T> Within<T>(Func<Fin<T>> body) =>
        Try.lift(body).Run().Bind(static inner => inner).Settled(held: Seq(this), release: held => held.Release());

    private Fin<Unit> Release() => Cell.Step(
            cell: state,
            step: static held => held is LeaseState<Unit>.Live ? Some<LeaseState<Unit>>(new LeaseState<Unit>.Released()) : None,
            declined: new KernelFault.InvalidContext())
        is Transition<LeaseState<Unit>>.Committed
        ? HostThread.Release(releases: releases)
        : Fin.Succ(value: unit);
}

internal sealed record NamedPacket(NamedParametersEventArgs Args, NamedLease Lease) {
    internal Fin<T> Within<T>(Func<NamedParametersEventArgs, Fin<T>> body) =>
        Lease.Within(body: () => body(Args));
}

public sealed record NamedBag {
    private NamedBag(HashMap<string, NamedValue> rows) => Rows = rows;

    public static readonly NamedBag Empty = new(rows: HashMap<string, NamedValue>());

    public HashMap<string, NamedValue> Rows { get; }

    public Fin<NamedBag> Put(string name, NamedValue value) {
        return from admitted in Acceptance.Text(value: name)
               from payload in Admit.Need(value)
               from _ in guard(flag: Rows.Find(admitted).IsNone, False: new KernelFault.InvalidInput(Axis: Some(admitted)))
               select new NamedBag(rows: Rows.Add(admitted, payload));
    }

    public NamedBag Remove(string key) => new(rows: Rows.Remove());

    public Option<NamedValue> Find(string key) => Rows.Find(key);

    internal Fin<NamedLease> WriteInto(NamedParametersEventArgs args) {
        (Seq<Func<Fin<Unit>>> Releases, Option<Error> Fault) state = toSeq(Rows.AsIterable()).Fold(
            (Releases: Seq<Func<Fin<Unit>>>(), Fault: Option<Error>.None),
            (held, row) => held.Fault.IsSome
                ? held
                : row.Value.Kind
                    .Bind(static kind => kind.Write)
                    .ToFin(Fail: new KernelFault.InvalidInput(Axis: Some(row.Key)))
                    .Bind(write => write(args, row.Value))
                    .Match(
                        Succ: releases => held with { Releases = releases + held.Releases },
                        Fail: fault => held with { Fault = Some(fault) }));
        return state.Fault.Match(
            Some: fault => Fin.Fail<NamedLease>(error: fault)
                .Rollback(release: () => HostThread.Release(releases: state.Releases)),
            None: () => Fin.Succ(value: new NamedLease(releases: state.Releases)));
    }

    internal Fin<NamedPacket> Mint() {
        NamedParametersEventArgs args = new();
        return WriteInto(args: args)
            .Map(values => new NamedPacket(
                Args: args,
                Lease: values.Append(release: () => Fin.Succ(value: ignore(fun(args.Dispose)())))))
            .Rollback(release: () => Fin.Succ(value: ignore(fun(args.Dispose)())));
    }

    internal static Fin<NamedBag> Detach(NamedParametersEventArgs args, HashMap<string, NamedSlot> schema) =>
        toSeq(schema.Values)
            .TraverseM(slot => Try.lift(() => slot.Kind.Read(args: args).Match(
                Some: value => Fin.Succ(value: Some((slot.Key, value))),
                None: () => slot.Presence
                    ? Fin.Fail<Option<(string, NamedValue)>>(error: new KernelFault.InvalidResult(Detail: Some(slot.Key)))
                    : Fin.Succ(value: Option<(string, NamedValue)>.None))).Run().Bind(static inner => inner))
            .As()
            .Map(static rows => new NamedBag(rows: toHashMap(rows.Choose(static row => row))));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class NamedRegistry {
    private readonly Atom<HashMap<string, (PluginKey Plugin, Guid Claim)>> names =
        Atom(HashMap<string, (PluginKey Plugin, Guid Claim)>());

    public static NamedRegistry Of() => new();

    public Seq<(string Name, PluginKey Plugin)> Census =>
        names.Value.AsIterable().ToSeq().Map(static row => (Name: row.Key, Plugin: row.Value.Plugin)).Strict();

    internal Fin<Guid> Claim(string name, PluginKey plugin) {
        Guid token = Guid.NewGuid();
        return Cell.Claim(cell: names, mint: () => (plugin, token)).Switch(
            committed: static _ => Fin.Succ(value: token),
            ceded: static (held) => Fin.Fail<Guid>(error: new KernelFault.InvalidContext()),
            refused: static row => Fin.Fail<Guid>(error: row.Cause),
            contended: static (held) => Fin.Fail<Guid>(error: new KernelFault.InvalidResult()));
    }

    internal Unit Yield(string name, Guid token) => ignore(names.Swap(held =>
        held.Find(name).Filter(holder => holder.Claim == token).Match(
            Some: _ => held.Remove(name),
            None: () => held)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    public static Fin<Subscription> Register(
        NamedRegistry registry,
        PluginKey plugin,
        string name,
        Seq<NamedSlot> request,
        Func<NamedBag, Fin<NamedBag>> body,
        Action<Error> report) {
        return from admitted in Acceptance.Text(value: name)
               from schema in NamedSlot.Admit(slots: request)
               from claim in registry.Claim(name: admitted, plugin: plugin)
               from seated in Try.lift(() => {
                   EventHandler<NamedParametersEventArgs> handler = (_, args) => ignore(Try.lift(() =>
                       (from bag in NamedBag.Detach(args: args, schema: schema)
                        from reply in Try.lift(() => body(bag)).Run().Bind(static inner => inner)
                        from lease in reply.WriteInto(args: args)
                        from served in lease.Within(body: static () => Fin.Succ(value: unit))
                        select served)
                       .IfFail(failure => ignore(HostEdge.Side(() => report(failure))))).Run().Bind(static inner => inner));
                   HostUtils.RegisterNamedCallback(name: admitted, callback: handler);
                   return Fin.Succ(value: Subscription.Of(detach: () => {
                       HostUtils.RemoveNamedCallback(name: admitted);
                       _ = registry.Yield(name: admitted, token: claim);
                   }));
               }).Run().Bind(static inner => inner).Rollback(release: () => Fin.Succ(value: registry.Yield(name: admitted, token: claim)))
               select seated;
    }

    public static Fin<Option<NamedBag>> Execute(
        string name,
        NamedBag bag,
        Seq<NamedSlot> response,
        Option<Env> env = default) {
        return from admitted in Acceptance.Text(value: name)
               from schema in NamedSlot.Admit(slots: response)
               from _ in guard(
                   flag: env.Map(static held => !held.Cancellation.IsCancellationRequested).IfNone(true),
                   False: Errors.Cancelled)
               from reply in bag.Mint().Bind(packet => packet.Within(
                   body: args => HostUtils.ExecuteNamedCallback(name: admitted, args: args)
                       ? NamedBag.Detach(args: args, schema: schema).Map(Some)
                       : Fin.Succ(value: Option<NamedBag>.None)))
               select reply;
    }
}
```

## [11]-[NOTICES]

- Owner: `NoticeSpec` admits title, message, severity, captions, metadata, and assembly guards once; `Notices.Use` brackets one host notification behind a `NoticeLease` for the body's extent.
- Owner: `RunOutcome` closes the completion vocabulary every long-running operation projects into, and `NoticeSpec.OfRun` is the one completion-notice row — severity derives from the outcome case, the metadata bag carries the run's own scale and duration facts, and `NoticeReply` is the three-button decision the consumer reads back.
- Owner: `NoticeReply` and `NoticeSeverity` key the host button and severity vocabularies; `NoticeFact` closes reply and property-change evidence; `CallbackObserver<NoticeFact>` guards both notice callback families.
- Entry: `Notices.Use` brackets a lease; `Notices.Announce` is the completion fire site — it takes an `Option<RunOutcome>`, so a run whose outcome folds to `None` mid-run announces nothing and a settled run announces once, and it folds the reply into `Option<NoticeReply>`.
- Law: a long run reports through `RunOutcome`, never through its own result type — the render and capture pipelines each project their result into that neutral carrier at their own edge, so no `RenderYield`, `CaptureArtifact`, or host render type crosses into this page and the notice row grows one case, never one overload per pipeline.
- Law: `RunOutcome.Failed` carries the run's own `Error`, so a refused run announces at `NoticeSeverity.Serious` with the fault text as its metadata row; `Debug` and `Critical` are caller-selected rows of the host's own five-value roster, reached through `NoticeSpec.Of` — the RESULT entry — rather than the generated throwing factory or the run projection.
- Law: `Announce` is a PUBLISHED boundary entry with no in-corpus caller by design — the folder rules that a public entry's altitude is the `apps/<app>/` plugin-shell command body, so its zero-caller census proves altitude and not death; the render and capture pipelines project `RunOutcome` and state that projection IS their whole obligation.
- Law: `NoticeLease` serializes callback delivery, host operations, and release through ONE gate whose lifecycle is a case; disposal detaches both callback families, withdraws, and retracts the centre membership through one failure-accumulating host-thread release.
- Law: reply and change facts stamp through the injected `MonotonicTimeline` — the capsule's one clock, taken as a required parameter here because a notice reaches this entry from a command body that already holds the capsule.
- Law: EVERY notification write runs inside `Notification.ExecuteAssemblyProtectedCode` — the host guards each field setter, the metadata indexer, `RemoveMetadata`, `HideModal`, and the centre's own `Remove`, so an unwrapped write against a restricted notice throws; only `ShowModal` is unguarded and it is the one write this page spells bare.
- Law: the guard admits by the WRITING assembly, so `Notices` unions its own assembly into a non-empty `NoticeSpec.Guards` set before construction — a caller-supplied roster omitting the boundary leaves the lease unable to administer the notice it owns — and an empty roster stays empty because empty means unrestricted.
- Law: membership in `NotificationCenter.Notifications` IS rendering — a notification never added shows nowhere and its `ShowModal` queues against nothing — so the mint adds and the release retracts; the set is otherwise unbound and the lease observes only its own notice.
- Exemption: `NoticeGate` holds a `Lock` rather than a `Cell` transition. Host writes and observer delivery run inside the guarded region and a compare-and-swap body re-runs on every contended retry, which would repeat a host write; the gate outlives the constructor because the host callbacks close over IT, never over a half-built lease.
- Packages: `Rasm/Domain/results` (`Ring<T>`, `Cell`, `Transition`), `Domain/validation` (`FactoryBridge.Row`, `FactoryBridge.Accept`); `Rasm/Interaction/dispatch` (`UiFault`); `Rasm/Parametric/projections` (`MonotonicTimeline`, `MonotonicStamp`); `Rasm.Rhino/Document/lifetime` (`Subscription`), kernel `Domain/results` (`Custody`); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`Notification` and its `Severity`/`ButtonType`, `ExecuteAssemblyProtectedCode`, `ShowModal`/`HideModal`, `MetadataCopy`/`RemoveMetadata`, `NotificationCenter.Notifications`).
- Growth: a new completion modality is one `RunOutcome` case with its severity arm and its metadata projection; a new host severity or button is one keyed row.
- Boundary: no Eto type appears on this cluster — the host notification family is `Rhino.Runtime.Notifications` alone, and the kernel `Presence` gate owns OS notification-centre, tray, taskbar, and badge presence, which never alias with this in-host centre.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    internal HostText Label => Switch(
        completed: static row => row.Label,
        cancelled: static row => row.Label,
        failed: static row => row.Label);

    internal NoticeSeverity Severity => Switch(
        completed: static _ => NoticeSeverity.Info,
        cancelled: static _ => NoticeSeverity.Warning,
        failed: static _ => NoticeSeverity.Serious);

    internal FrozenDictionary<string, string> Facts => Switch(
        completed: static row => row.Scale,
        cancelled: static _ => FrozenDictionary<string, string>.Empty,
        failed: static row => new Dictionary<string, string>(StringComparer.Ordinal) {
            [nameof(Error)] = row.Reason.Message,
        }.ToFrozenDictionary(StringComparer.Ordinal));
}

// --- [MODELS] --------------------------------------------------------------------------
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

    public static Fin<NoticeSpec> Of(
        HostText title,
        HostText message,
        NoticeSeverity severity,
        Option<HostText> description = default,
        Option<HostText> confirmCaption = default,
        Option<HostText> cancelCaption = default,
        Option<HostText> alternateCaption = default,
        Option<FrozenDictionary<string, string>> metadata = default,
        Seq<Assembly> guards = default) =>
        FactoryBridge.Accept<NoticeSpec>(
            Validate(
                title, message, description, severity, confirmCaption, cancelCaption, alternateCaption,
                metadata.IfNone(FrozenDictionary<string, string>.Empty), guards, out NoticeSpec? admitted),
            admitted);

    public static Fin<NoticeSpec> OfRun(
        RunOutcome outcome, HostText message, Seq<Assembly> guards = default,
        Option<HostText> confirmCaption = default, Option<HostText> alternateCaption = default) {
        return from active in Admit.Need(outcome)
               from body in Admit.Need(message)
               from spec in Of(
                   title: active.Label, message: body, severity: active.Severity,
                   confirmCaption: confirmCaption, alternateCaption: alternateCaption,
                   metadata: Some(active.Facts), guards: guards)
               select spec;
    }

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
            ? new ValidationError(message: $"{nameof(Title)} is absent.")
            : message is null
                ? new ValidationError(message: $"{nameof(Message)} is absent.")
                : severity is null
                    ? new ValidationError(message: $"{nameof(Severity)} is absent.")
                    : metadata is null
                        ? new ValidationError(message: $"{nameof(Metadata)} is absent.")
                        : metadata.Any(static row => string.IsNullOrWhiteSpace(row.Key) || row.Value is null)
                            ? new ValidationError(message: $"{nameof(Metadata)} carries a blank key or absent value.")
                            : guards.ForAll(static assembly => assembly is not null)
                                ? null
                                : new ValidationError(message: $"{nameof(Guards)} carries an absent assembly.");
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class NoticeGate(CallbackObserver<NoticeFact> observer) {
    private readonly CallbackObserver<NoticeFact> observer = observer;
    private LeaseState<Unit> state = new LeaseState<Unit>.Live(Held: unit);

    internal Lock Sync { get; } = new();

    internal Seq<Error> Faults => observer.Faults;

    internal bool Claimed() {
        lock (Sync) {
            if (state is not LeaseState<Unit>.Live) { return false; }
            state = new LeaseState<Unit>.Released();
            return true;
        }
    }

    internal Unit Deliver(Func<Fin<NoticeFact>> project) {
        lock (Sync) {
            return state.Switch(
                (Self: this, Project: project),
                live: static (held, _) => held.Self.observer.Guard(project: held.Project, op: held.Self.op),
                released: static (_, _) => unit);
        }
    }

    internal Fin<T> Within<T>(Func<Fin<T>> body) {
        lock (Sync) {
            return state.Switch(
                body,
                live: static (held, _) => held(),
                released: static (held, _) => Fin.Fail<T>(error: new UiFault.Released(Key: held.Key)));
        }
    }
}

public sealed class NoticeLease : IDisposable {
    private readonly Ring<Error> faults = ShellFaults.Ring();
    private readonly HostNotice notice;
    private readonly NoticeGate gate;
    private readonly Subscription observation;

    private NoticeLease(HostNotice notice, NoticeGate gate, Subscription observation) =>
        (this.notice, this.gate, this.observation, this.op) = (notice, gate, observation);

    internal static Fin<NoticeLease> Of(
        HostNotice notice, CallbackObserver<NoticeFact> observer, MonotonicTimeline timeline) {
        NoticeGate gate = new(observer: observer.Fork());
        PropertyChangedEventHandler changed = (_, args) => ignore(gate.Deliver(() => Fin.Succ<NoticeFact>(
            value: new NoticeFact.ChangedCase(
                Property: args.PropertyName ?? string.Empty,
                At: timeline.Capture().ToOption()))));
        return Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
                () => Subscription.Acquire(
                    acquire: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = button =>
                        ignore(gate.Deliver(() => FactoryBridge.Row<HostNoticeButton, NoticeReply>(button)
                            .Map<NoticeFact>(reply => new NoticeFact.ReplyCase(
                                Reply: reply, At: timeline.Capture().ToOption()))))),
                    release: () => HostNotice.ExecuteAssemblyProtectedCode(action: () => notice.ButtonClicked = null)),
                () => Subscription.Attach(
                    subscribe: callback => notice.PropertyChanged += callback,
                    unsubscribe: callback => notice.PropertyChanged -= callback,
                    handler: changed)))
            .Map(attached => new NoticeLease(notice: notice, gate: gate, observation: attached));
    }

    public Seq<Error> Faults => faults.Parked + gate.Faults;
    public long Shed => faults.Shed;

    public Fin<Unit> Present() => Crossing(body: static held => HostEdge.Side(held.ShowModal));

    public Fin<Unit> Withdraw() => Crossing(
        body: static held => HostEdge.Side(() => HostNotice.ExecuteAssemblyProtectedCode(action: held.HideModal)));

    public Fin<FrozenDictionary<string, string>> Metadata() => Crossing(
        body: static held => held.MetadataCopy.ToFrozenDictionary(StringComparer.Ordinal));

    public Fin<Unit> Annotate(string field, Option<string> value) {
        return Acceptance.Text(value: field).Bind(named => Crossing(
            body: held => HostEdge.Side(() => HostNotice.ExecuteAssemblyProtectedCode(action: () => ignore(value.Match(
                Some: text => HostEdge.Side(() => held[named] = text),
                None: () => HostEdge.Side(() => ignore(held.RemoveMetadata(key: named)))))))));
    }

    public Fin<Unit> Release() {
        if (!gate.Claimed()) { return Fin.Succ(value: unit); }
        HostNotice held = notice;
        Subscription attached = observation;
        return HostThread.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => Try.lift(attached.Dispose).Run(),
                () => Try.lift(() => HostNotice.ExecuteAssemblyProtectedCode(action: () => {
                    held.HideModal();
                    _ = HostNoticeCenter.Notifications.Remove(held);
                })).Run().Bind(static inner => inner)))
            .MapFail(failure => (faults.Park(item: failure), failure).Item2);
    }

    public void Dispose() => _ = Release();

    private Fin<T> Crossing<T>(Func<HostNotice, T> body) {
        HostNotice held = notice;
        return HostThread.Run(
            work: new HostWork<T>.Execute(Body: () => gate.Within(
                body: () => Try.lift(() => body(held)).Run())));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Notices {
    public static Fin<T> Use<T>(
        NoticeSpec spec,
        CallbackObserver<NoticeFact> observer,
        MonotonicTimeline timeline,
        Func<NoticeLease, Fin<T>> body) {
        return HostThread.Run(
            work: new HostWork<T>.Execute(Body: () => Mint(spec: spec, observer: observer, timeline: timeline)
                .Bind(lease => Try.lift(() => body(lease)).Run().Bind(static inner => inner)
                    .Settled(held: Seq(lease), release: held => held.Release()))));
    }

    public static Fin<Option<T>> Announce<T>(
        Option<RunOutcome> outcome,
        HostText message,
        CallbackObserver<NoticeFact> observer,
        MonotonicTimeline timeline,
        Func<NoticeLease, Fin<T>> body,
        Seq<Assembly> guards = default,
        Option<HostText> confirmCaption = default,
        Option<HostText> alternateCaption = default) {
        return outcome.TraverseM(settled =>
                from spec in NoticeSpec.OfRun(
                    outcome: settled, message: message, guards: guards,
                    confirmCaption: confirmCaption, alternateCaption: alternateCaption)
                from answered in Use(
                    spec: spec, observer: observer, timeline: timeline,
                    body: lease => lease.Present().Bind(_ => body(lease)))
                select answered)
            .As();
    }

    private static Fin<NoticeLease> Mint(
        NoticeSpec spec, CallbackObserver<NoticeFact> observer, MonotonicTimeline timeline) =>
        Try.lift(() => {
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
                _ = toSeq(spec.Metadata.AsEnumerable()).Iter(row => HostEdge.Side(() => notice[row.Key] = row.Value));
            });
            return NoticeLease.Of(notice: notice, observer: observer, timeline: timeline)
                .Bind(lease => Try.lift(() => HostNoticeCenter.Notifications.Add(notice)).Run().Bind(static inner => inner)
                    .Map(_ => lease)
                    .Rollback(release: () => Try.lift(lease.Dispose).Run()));
        }).Run().Bind(static inner => inner);
}
```

## [12]-[COMPOSITION_CAPSULE]

- Owner: `ShellMount` closes the process-lifetime mount family and `ShellCapsule` is the ONE seat table the boundary opens; `ShellSeat` is the seat arity every foreign owner's mount answers.
- Cases: `Marshal` seats the latency ledger under the capsule's timeline, `Pacing` tunes the process pace band, `Theme` freezes the kernel grid and publishes its boundary, `Named` mints the wire-name registry and seats its rows, `Hooks` runs the boundary's `*Hooks.Mount` roster, `Vault` and `Engines` run a foreign owner's own seat, `Resolver` extends the assembly resolver, and `Endpoints` binds the compute roster.
- Entry: `ShellCapsule.Open(PackageIdentity<PluginKey, HostSnapshot>, MonotonicTimeline, params ReadOnlySpan<ShellMount>)` seats every declared row and answers a leased capsule; `Release` retires every seat in reverse mount order.
- Auto: the capsule COMPOSES its timeline and never mints one. `Plugin/lifecycle#LOAD_ROOT` holds the plug-in `Assembly` and is the only moment inside `libs/` that can resolve the identity, so the boundary's ONE `MonotonicTimeline` mints in that same fold and threads in here as a required parameter — a second mint at this seat would fork the causal order the folder ruling exists to keep single.
- Auto: a mount is DATA. `PluginBoot.Mounts` is a declared `Seq<ShellMount>`, so a new load-time act is one row a plug-in program states and never a statement inside a host override.
- Law: the discriminant is what a case does to the capsule. `Marshal`, `Pacing`, and `Theme` CONSUME the capsule's own timeline; `Theme` and `Named` PUBLISH a value later mounts and consumers read; the rest seat foreign or local owners and publish nothing. A case that neither consumes nor publishes carries its owner's seat as a thunk, because a payload naming a `Display` or `Blocks` type here would invert the import edge those pages already declare toward this one.
- Law: mount ORDER is declaration order and retirement is its exact inverse, run through `Custody.Release` so every retirement runs even when an earlier one refuses and the whole refusal set answers as `Error.Many`. A refused mount ROLLS BACK every seat already taken before it answers, so a partial capsule is unrepresentable.
- Law: `Bind` has no spelling here. `MountRegistry.Bind` is the boundary's name-addressed resolve and a capsule member forwarding to it would put one name two hops from its owner; a consumer holding a point binds at the registry.
- Law: the capsule owns the boundary's ONE `FaultCell` — the bounded isolated-fault sink the kernel `Presence` gate and the kernel `ThemePort` registration both take, so a per-consumer cell beside it would be three rings answering one question.
- Output: `ReleaseFaults` publishes every retirement refusal the capsule retained; the leased capsule IS the transfer of ownership, and `PluginRoot` holds it for the process.
- Packages: `Rasm/Domain/frame` (`PackageIdentity<TKey,THostFact>`), `Domain/results` (`Lease<T>`, `Ring<T>`, `Cell`), `Domain/hooks` (`FaultCell`); `Rasm/Interaction/dispatch` (`DispatchLane`, `StallPolicy`, `UiThread.Tune`), `Interaction/paint` (`ThemeGrid`, `ThemeProgram`, `ThemeVariant`, `ContrastRule`), `Interaction/platform` (`ThemePort`); `Rasm/Parametric/projections` (`MonotonicTimeline`, `PaceBand`); `Rasm.Rhino/Document/events` (`PluginKey`), kernel `Domain/results` (`Custody`); `libs/dotnet/.api/api-telemetry-abstractions.md` (`ILatencyContextProvider`, `ILatencyContextTokenIssuer`).
- Growth: a new process-lifetime registry is ONE `ShellMount` case and ONE arm; every consumer is untouched or broken at compile time.
- Boundary: this capsule is the IN-package composition owner and holds no AppHost, hosting, or OpenTelemetry type; the `apps/<app>/` plugin shell is the one assembly referencing `Rasm.AppHost` beside `Rasm.Rhino`, and it binds `PluginRoot` for the lacing that follows.
- Boundary: the app root laces telemetry over the resolved identity — it gates `ProfileSurface.Resolve` on the `HostRows.Rhino` row (`Tenancy.None`, `DeploymentTopology.InHost`, `LifecycleOwner.CallerOwned`, `Isolation.InProc`, no providers, because Rhino owns the process and the plugin binds no provider port, so the row samples whole and projects its logs locally) under `TelemetryDomain.Rhino.Key`, `Environments.Production`, and the identity's content root and version, then opens `PluginTelemetryHost.Open` on `Identity.Alc` with `Seq(RhinoInstruments.Telemetry(version))` as the contributor set and the plugin, process, and version discriminators read off the identity; the axis values gate BEFORE the capsule opens, so an unservable row refuses while no provider exists to dispose.
- Boundary: capsule cardinality is one per plugin `AssemblyLoadContext`, opened once at load and never per feature; a second plugin is a second identity resolve and a second open under its own discriminator. `ProfileIdentity.ResourceAttributes` owns resource identity, `TelemetryDomain.Qualify` renders `service.name` off the `TelemetryDomain.Rhino` row rather than a literal, `Rostered` refuses an unrostered `rasm.` key, semconv `host.*` stays the machine facts `AddHostDetector` supplies, and `Environments.Production` floors the environment row while the `OTEL_RESOURCE_ATTRIBUTES` detector outranks it at deploy.
- Boundary: telemetry LIFETIME is the plugin `AssemblyLoadContext`'s own `Unloading` hook — `ForceFlush` then `Dispose` per the AppHost provider-lifetime law — and every Rasm meter in the plugin process reaches the capsule `IMeterFactory`, a process-static `Meter` staying the named defect. The app root also registers the eight `MarshalLatency` checkpoint, measure, and tag names before the `ShellMount.Marshal` row seats the ledger.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public delegate Fin<Seq<Func<Fin<Unit>>>> ShellSeat();

public sealed record NamedRow(
    string Name,
    Seq<NamedSlot> Request,
    Func<NamedBag, Fin<NamedBag>> Body,
    Action<Error> Report);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShellMount {
    private ShellMount() { }

    public sealed record Marshal(ILatencyContextProvider Provider, ILatencyContextTokenIssuer Issuer) : ShellMount;
    public sealed record Pacing(PaceBand Band, HashMap<DispatchLane, double> Stretch) : ShellMount;
    public sealed record Theme(ThemeProgram Program, ThemeVariant Initial, Seq<ContrastRule> Contrast) : ShellMount;
    public sealed record Named(Seq<NamedRow> Rows) : ShellMount;
    public sealed record Hooks(Seq<Func<PluginKey, Fin<IDisposable>>> Mounts) : ShellMount;
    public sealed record Vault(ShellSeat Seat) : ShellMount;
    public sealed record Engines(ShellSeat Seat) : ShellMount;
    public sealed record Resolver(Seq<AssemblySource> Sources) : ShellMount;
    public sealed record Endpoints(Seq<HostEndpoint> Rows) : ShellMount;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ShellCapsule : IDisposable {
    private sealed record Seating(Seq<Func<Fin<Unit>>> Retire, Option<ThemePort> Themes, Option<NamedRegistry> Names);

    private readonly Atom<LeaseState<Seq<Func<Fin<Unit>>>>> retire;
    private readonly Ring<Error> teardown = ShellFaults.Ring();

    private ShellCapsule(
        PackageIdentity<PluginKey, HostSnapshot> identity,
        MonotonicTimeline timeline,
        FaultCell faults,
        Seating seated) {
        (Identity, Timeline, Faults, Themes, Names, this.op) =
            (identity, timeline, faults, seated.Themes, seated.Names);
        retire = Atom<LeaseState<Seq<Func<Fin<Unit>>>>>(new LeaseState<Seq<Func<Fin<Unit>>>>.Live(Held: seated.Retire));
    }

    public PackageIdentity<PluginKey, HostSnapshot> Identity { get; }
    public MonotonicTimeline Timeline { get; }
    public FaultCell Faults { get; }
    public Option<ThemePort> Themes { get; }
    public Option<NamedRegistry> Names { get; }
    public Seq<Error> ReleaseFaults => teardown.Parked;

    public static Fin<Lease<ShellCapsule>> Open(
        PackageIdentity<PluginKey, HostSnapshot> identity,
        MonotonicTimeline timeline,
        params ReadOnlySpan<ShellMount> mounts) {
        FaultCell faults = ShellFaults.Cell();
        return Iterable<ShellMount>.FromSpan(mounts)
            .Fold(
                Fin.Succ(value: new Seating(Retire: Seq<Func<Fin<Unit>>>(), Themes: None, Names: None)),
                (held, mount) => held.Bind(seated => Seat(
                        mount: mount, seated: seated, identity: identity, timeline: timeline, faults: faults)
                    .Rollback(release: () => Custody.Release(releases: seated.Retire.Rev()))))
            .Map(seated => (Lease<ShellCapsule>)new Lease<ShellCapsule>.Owned(Value: new ShellCapsule(
                identity: identity, timeline: timeline, faults: faults, seated: seated)));
    }

    public Fin<Unit> Release() {
        Seq<Func<Fin<Unit>>> drained = Seq<Func<Fin<Unit>>>();
        return Cell.Step(
                cell: retire,
                step: held => held is LeaseState<Seq<Func<Fin<Unit>>>>.Live live
                    ? (drained = live.Held, Some<LeaseState<Seq<Func<Fin<Unit>>>>>(new LeaseState<Seq<Func<Fin<Unit>>>>.Released())).Item2
                    : None,
                declined: new KernelFault.InvalidContext())
            is Transition<LeaseState<Seq<Func<Fin<Unit>>>>>.Committed
            ? Custody.Release(releases: drained.Rev()).MapFail(failure =>
                (teardown.Park(item: failure), failure).Item2)
            : Fin.Succ(value: unit);
    }

    public void Dispose() => _ = Release();

    private static Fin<Seating> Seat(
        ShellMount mount,
        Seating seated,
        PackageIdentity<PluginKey, HostSnapshot> identity,
        MonotonicTimeline timeline,
        FaultCell faults) =>
        mount.Switch(
            (Seated: seated, Identity: identity, Timeline: timeline, Faults: faults),
            marshal: static (held, row) => MarshalLatency.Mount(
                    plugin: held.Identity.Plugin, provider: row.Provider, issuer: row.Issuer,
                    timeline: held.Timeline)
                .Map(lease => held.Seated with { Retire = held.Seated.Retire.Add(Retiring(lease)) }),
            pacing: static (held, row) => UiThread.Tune(
                    policy: new StallPolicy(Pace: row.Band, Stretch: row.Stretch),
                    clock: Some(held.Timeline))
                .Map(_ => held.Seated),
            theme: static (held, row) =>
                from grid in ThemeGrid.Freeze(
                        program: row.Program, initial: row.Initial, contrast: row.Contrast,
                        clock: held.Timeline)
                    .ToFin()
                from theme in ThemePort.Of(grid: grid)
                select held.Seated with { Themes = Some(theme) },
            named: static (held, row) => {
                NamedRegistry registry = NamedRegistry.Of();
                return row.Rows
                    .TraverseM(entry => NamedCallbacks.Register(
                        registry: registry, plugin: held.Identity.Plugin, name: entry.Name, request: entry.Request,
                        body: entry.Body, report: entry.Report))
                    .As()
                    .Map(seats => held.Seated with {
                        Names = Some(registry),
                        Retire = held.Seated.Retire + seats.Map(seat => Retiring(seat)),
                    });
            },
            hooks: static (held, row) => row.Mounts
                .TraverseM(mount => mount(held.Identity.Plugin))
                .As()
                .Map(seats => held.Seated with { Retire = held.Seated.Retire + seats.Map(seat => Retiring(seat)) }),
            vault: static (held, row) => row.Seat(held.Op)
                .Map(rows => held.Seated with { Retire = held.Seated.Retire + rows }),
            engines: static (held, row) => row.Seat(held.Op)
                .Map(rows => held.Seated with { Retire = held.Seated.Retire + rows }),
            resolver: static (held, row) => HostAssemblies
                .Extend(plugin: held.Identity.Plugin, sources: row.Sources)
                .Bind(outcome => outcome.Fault.Match(
                    Some: Fin.Fail<Seating>,
                    None: () => Fin.Succ(value: held.Seated))),
            endpoints: static (held, row) => row.Rows
                .TraverseM(entry => HostEndpoints.Register(path: entry.Path, contract: entry.Contract))
                .As()
                .Map(_ => held.Seated));

    private static Func<Fin<Unit>> Retiring(IDisposable seat) => () => Try.lift(seat.Dispose).Run();

    private static Func<Fin<Unit>> Retiring(Lease<IDisposable> seat) => () => Try.lift(() => seat.Dispose()).Run().Bind(static inner => inner);
}
```

## [13]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
