# [RASM_RHINO_ETO_RUNTIME]

`UiThread` owns every crossing into Eto's control tree, and the ambient runtime projects cadence, displays, input, transfer, drag/drop, notifications, tray presence, and taskbar state as typed values. Every marshal completes on a caller-visible rail, drag providers remain behind admitted values, and every event or host resource returns deterministic lifetime ownership.

## [01]-[INDEX]

- [02]-[THREAD]: `UiThread` folds current-frame, blocking, and awaited dispatch through one entry.
- [03]-[AMBIENT]: `Pulse`, `Displays`, `PointerState`, `ModifierState`, and `CursorRow` detach ambient time, display, and input facts.
- [04]-[TRANSFER]: `Transfer`, `DragPlan`, and `Drop` own payload algebra, drag initiation, and admitted negotiation.
- [05]-[PRESENCE]: `Toast`, `TrayLease`, `TaskbarPulse`, and `PresenceBadge` own system presence and release.

## [02]-[THREAD]

- Owner: `UiThread` resolves one `UiDispatch<TResult>` through `Application.Instance`; no callback discards a failure.
- Cases: `Current` asserts affinity, `Blocking` runs in-frame or marshals through `Invoke`, and `Awaited` marshals through `InvokeAsync`.
- Entry: `UiThread.Run` is the one modality-polymorphic crossing and always returns an observable `ValueTask<Fin<TResult>>`.
- Receipt: the returned `Fin<TResult>` captures host marshal faults and body faults.
- Growth: another host dispatch form is one `UiDispatch<TResult>` case.
- Boundary: `HostThread` owns Rhino command affinity; a body needing both enters `HostThread` before this owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UiDispatch<TResult> {
    private UiDispatch() { }
    public sealed record Current(Func<Fin<TResult>> Body) : UiDispatch<TResult>;
    public sealed record Blocking(Func<Fin<TResult>> Body) : UiDispatch<TResult>;
    public sealed record Awaited(Func<Fin<TResult>> Body) : UiDispatch<TResult>;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class UiThread {
    public static ValueTask<Fin<TResult>> Run<TResult>(UiDispatch<TResult> dispatch, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(dispatch).Match(
            Some: work => work.Switch(
                state: op,
                current: static (at, current) => ValueTask.FromResult(
                    at.Catch(() => Application.Instance.IsUIThread
                        ? current.Body()
                        : Fin.Fail<TResult>(new UiFault.OffThread(Key: at)))),
                blocking: static (at, blocking) => ValueTask.FromResult(at.Catch(() => Application.Instance.IsUIThread
                    ? blocking.Body()
                    : Application.Instance.Invoke(blocking.Body))),
                awaited: static (at, awaited) => new ValueTask<Fin<TResult>>(Await(at, awaited.Body))),
            None: () => ValueTask.FromResult(Fin.Fail<TResult>(
                new UiFault.Rejected(Key: op, Field: nameof(dispatch), Reason: "dispatch requires a case"))));
    }

    public static Fin<Unit> Iterate(Op? key = null) =>
        key.OrDefault().Catch(() => {
            Application.Instance.EnsureUIThread();
            Application.Instance.RunIteration();
            return Fin.Succ(unit);
        });

    private static async Task<Fin<TResult>> Await<TResult>(Op op, Func<Fin<TResult>> body) {
        try {
            return await Application.Instance.InvokeAsync(() => op.Catch(body)).ConfigureAwait(false);
        } catch (Exception thrown) {
            return Fin.Fail<TResult>(new UiFault.HostRejected(Key: op, Detail: thrown.Message));
        }
    }
}
```

## [03]-[AMBIENT]

- Owner: `Pulse` leases `UITimer` and advances one kernel `MonotonicTimeline` beat chain per lease; `Displays`, `PointerState`, and `ModifierState` snapshot ambient facts; `CursorRow` applies one cursor vocabulary.
- Entry: `Pulse.Start` accepts cadence and clock explicitly — the clock admits through `MonotonicTimeline.Of`, `Capture` seeds the origin, and each tick advances the chain through `Beat`, so no tick reads or subtracts raw provider timestamps.
- Receipt: `PulseBeat` carries ordinal, elapsed time, requested interval, drift, and missed-beat evidence; `Pulse.Failures` retains primary callback faults plus reporter faults.
- Auto: display topology is re-read rather than cached; pointer reads reject unsupported backends before touching provider state.
- Growth: another beat metric extends `PulseBeat`; another cursor is one `CursorRow` row.
- Boundary: `Pulse` paces UI housekeeping; viewport motion owns frame pacing.
- Exemption: `UITimer` construction, start, and release form the callback-boundary statement seam; a failed start disposes the unleased timer.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CursorRow {
    public static readonly CursorRow Standard = new(key: 0, resolve: static () => Cursors.Default);
    public static readonly CursorRow Arrow = new(key: 1, resolve: static () => Cursors.Arrow);
    public static readonly CursorRow Crosshair = new(key: 2, resolve: static () => Cursors.Crosshair);
    public static readonly CursorRow Hand = new(key: 3, resolve: static () => Cursors.Pointer);
    public static readonly CursorRow Move = new(key: 4, resolve: static () => Cursors.Move);
    public static readonly CursorRow Beam = new(key: 5, resolve: static () => Cursors.IBeam);
    public static readonly CursorRow SplitVertical = new(key: 6, resolve: static () => Cursors.VerticalSplit);
    public static readonly CursorRow SplitHorizontal = new(key: 7, resolve: static () => Cursors.HorizontalSplit);
    public static readonly CursorRow SizeAll = new(key: 8, resolve: static () => Cursors.SizeAll);
    public static readonly CursorRow Denied = new(key: 9, resolve: static () => Cursors.NotAllowed);

    [UseDelegateFromConstructor]
    internal partial Cursor Resolve();

    public Unit Apply(Control control) => Op.Side(() => control.Cursor = Resolve());
    public Unit Override() => Op.Side(() => Mouse.SetCursor(Resolve()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PulseBeat(long Ordinal, TimeSpan Elapsed, TimeSpan Interval, TimeSpan Drift, long Missed);

public sealed record DisplayFacts(
    RectangleF Bounds,
    RectangleF WorkingArea,
    float Dpi,
    float Scale,
    float LogicalPixelSize,
    bool Primary);

public readonly record struct PointerState(PointF Position, MouseButtons Pressed) {
    public static Fin<PointerState> Read(Op? key = null) => Mouse.IsSupported
        ? Fin.Succ(new PointerState(Mouse.Position, Mouse.Buttons))
        : Fin.Fail<PointerState>(new UiFault.Unavailable(Key: key.OrDefault(), Capability: nameof(Mouse)));

    public bool Holds(MouseButtons buttons) => (Pressed & buttons) != MouseButtons.None;
}

public readonly record struct ModifierState(Keys Held) {
    public static ModifierState Read() => new(Keyboard.Modifiers);
    public static bool Locked(Keys key) => Keyboard.IsKeyLocked(key);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class Pulse : UiLease {
    private readonly UITimer timer;
    private readonly MonotonicTimeline timeline;
    private readonly MonotonicStamp origin;
    private readonly Atom<Option<MonotonicBeat>> chain = Atom(Option<MonotonicBeat>.None);
    private readonly TimeSpan interval;
    private readonly Action<PulseBeat> publish;
    private readonly Action<Error> report;
    private readonly Atom<Seq<Error>> failures = Atom(Seq<Error>());
    private readonly Op key;

    private Pulse(
        UITimer timer,
        MonotonicTimeline timeline,
        MonotonicStamp origin,
        TimeSpan interval,
        Action<PulseBeat> publish,
        Action<Error> report,
        Op key) {
        this.timer = timer;
        this.timeline = timeline;
        this.origin = origin;
        this.interval = interval;
        this.publish = publish;
        this.report = report;
        this.key = key;
    }

    public Seq<Error> Failures => failures.Value;

    public static Fin<Pulse> Start(
        PositiveMagnitude seconds,
        TimeProvider clock,
        Action<PulseBeat> publish,
        Action<Error> report,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => {
            ArgumentNullException.ThrowIfNull(publish);
            ArgumentNullException.ThrowIfNull(report);
            TimeSpan interval = TimeSpan.FromSeconds(seconds.Value);
            return interval.Ticks < 1
                ? Fin.Fail<Pulse>(new UiFault.Rejected(Key: op, Field: nameof(seconds), Reason: "timer cadence is below host resolution"))
                : MonotonicTimeline.Of(provider: clock, key: op).Bind(timeline =>
                    timeline.Capture(key: op).Bind(origin => {
                        Pulse? pulse = null;
                        UITimer? timer = null;
                        bool started = false;
                        try {
                            timer = new UITimer((_, _) => pulse!.Tick()) { Interval = seconds.Value };
                            pulse = new Pulse(timer, timeline, origin, interval, publish, report, op);
                            timer.Start();
                            started = true;
                            return Fin.Succ(pulse);
                        } finally {
                            if (!started)
                                timer?.Dispose();
                        }
                    }));
        });
    }

    protected override Fin<Unit> Free() => Seq<Action>(timer.Stop, timer.Dispose).Drained(key);

    private void Tick() => _ = chain.Swap(prior => prior
        .Match(Some: held => timeline.Beat(seed: held, key: key), None: () => timeline.Beat(seed: origin, key: key))
        .Match(
            Succ: beat => {
                _ = key.Catch(() => {
                    publish(new PulseBeat(
                        Ordinal: beat.Ordinal,
                        Elapsed: beat.Elapsed,
                        Interval: interval,
                        Drift: beat.Elapsed - TimeSpan.FromTicks(interval.Ticks * beat.Ordinal),
                        Missed: Math.Max(0L, (beat.Elapsed.Ticks / interval.Ticks) - beat.Ordinal)));
                    return Fin.Succ(unit);
                }).Match(Succ: static published => published, Fail: Retain);
                return Some(beat);
            },
            Fail: fault => (Retain(fault), prior).Item2));

    private Unit Retain(Error fault) =>
        ignore(failures.Swap(held => held.Add(fault.Reported(report, key))));
}

public static class Displays {
    public static Seq<DisplayFacts> Roster() => toSeq(Screen.Screens).Map(Snapshot);
    public static DisplayFacts Primary() => Snapshot(Screen.PrimaryScreen);
    public static DisplayFacts At(PointF point) => Snapshot(Screen.FromPoint(point));
    public static DisplayFacts Covering(RectangleF bounds) => Snapshot(Screen.FromRectangle(bounds));
    public static Fin<Image> Capture(RectangleF bounds, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ<Image>(Screen.FromRectangle(bounds).GetImage(bounds)));

    private static DisplayFacts Snapshot(Screen screen) =>
        new(screen.Bounds, screen.WorkingArea, screen.DPI, screen.Scale, screen.LogicalPixelSize, screen.IsPrimary);
}
```

## [04]-[TRANSFER]

- Owner: `Transfer` folds read, write, inspect, and clear operations over clipboard or drag bundles; `Drop` hides `DragEventArgs` after admission.
- Cases: `PayloadSlot` spans text, HTML, image, URI, bytes, stream, and boxed payloads; `TransferQuery` mirrors every readable shape, while `PayloadPresence` makes optional versus required reads explicit.
- Entry: `Transfer.Apply` is the one bidirectional transfer entry; awaited `Transfer.Begin` marshals the complete drag lifetime through `UiThread`, and `Drop.Of` admits drop ingress.
- Receipt: optional reads preserve absence in `TransferReceipt.Read`; required absence returns `UiFault.AbsentPayload`; writes expose every slot fact, and detached drag bundles publish only after every fact succeeds.
- Growth: another payload shape adds one `PayloadSlot` case, one `TransferQuery` case, and compiler-forced fold arms.
- Boundary: `DragEventArgs` remains private inside `Drop`; byte slots detach into immutable storage before egress, and `Transfer.Begin` owns streamed slots through drag completion and releases every stream on either outcome.
- Exemption: provider effect and drop-description assignment form the drag/drop statement seam.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct Mime {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "MIME identity requires a type and subtype.");
            return;
        }
        value = value.Trim();
        int separator = value.IndexOf('/', StringComparison.Ordinal);
        validationError = separator <= 0
            || separator >= value.Length - 1
            || value.IndexOf('/', separator + 1) >= 0
            || value.Any(char.IsWhiteSpace)
            ? new ValidationError(message: "MIME identity requires a type and subtype.")
            : null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PayloadSlot {
    private PayloadSlot() { }
    public sealed record Text(string Value) : PayloadSlot;
    public sealed record Html(string Value) : PayloadSlot;
    public sealed record Picture(Image Value) : PayloadSlot;
    public sealed record Linked(Seq<Uri> Value) : PayloadSlot;
    public sealed record Bytes(Mime Key, Arr<byte> Value) : PayloadSlot;
    public sealed record Streamed(Mime Key, Stream Value) : PayloadSlot, IDisposable {
        private int released;
        public void Dispose() {
            if (Interlocked.Exchange(ref released, 1) == 0)
                Value.Dispose();
        }
    }
    public sealed record Boxed(Mime Key, object Value) : PayloadSlot;

    internal string Identity => Switch(
        text: static _ => nameof(Text),
        html: static _ => nameof(Html),
        picture: static _ => nameof(Picture),
        linked: static _ => nameof(Linked),
        bytes: static slot => slot.Key.Value,
        streamed: static slot => slot.Key.Value,
        boxed: static slot => slot.Key.Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferQuery {
    private TransferQuery() { }
    public sealed record Text : TransferQuery;
    public sealed record Html : TransferQuery;
    public sealed record Picture : TransferQuery;
    public sealed record Linked : TransferQuery;
    public sealed record Bytes(Mime Key) : TransferQuery;
    public sealed record Streamed(Mime Key) : TransferQuery;
    public sealed record Boxed(Mime Key, Type Expected) : TransferQuery;

    internal string Identity => Switch(
        text: static _ => nameof(Text),
        html: static _ => nameof(Html),
        picture: static _ => nameof(Picture),
        linked: static _ => nameof(Linked),
        bytes: static query => query.Key.Value,
        streamed: static query => query.Key.Value,
        boxed: static query => query.Key.Value);
}

[SmartEnum<int>]
public sealed partial class PayloadPresence {
    public static readonly PayloadPresence Optional = new(key: 0, isRequired: false);
    public static readonly PayloadPresence Required = new(key: 1, isRequired: true);
    internal bool IsRequired { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferOp {
    private TransferOp() { }
    public sealed record Read(TransferQuery Query, PayloadPresence Presence) : TransferOp;
    public sealed record Write(Seq<PayloadSlot> Slots) : TransferOp;
    public sealed record Inspect : TransferOp;
    public sealed record Clear : TransferOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferReceipt {
    private TransferReceipt() { }
    public sealed record Read(Option<PayloadSlot> Payload) : TransferReceipt;
    public sealed record Written(Seq<TransferWriteFact> Slots) : TransferReceipt {
        public int Count => Slots.Filter(static fact => fact.IsValid).Count;

        public Option<Error> Failure => Slots
            .Choose(static fact => fact is TransferWriteFact.Rejected rejected ? Some(rejected.Fault) : None)
            .Fold(Option<Error>.None, static (held, fault) => Some(held.Match(
                Some: prior => prior + fault,
                None: () => fault)));
    }
    public sealed record Inventory(Seq<string> Types) : TransferReceipt;
    public sealed record Cleared : TransferReceipt;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferWriteFact : IValidityEvidence {
    private TransferWriteFact() { }
    public sealed record Committed(string Slot) : TransferWriteFact;
    public sealed record Rejected(string Slot, Error Fault) : TransferWriteFact;

    public bool IsValid => this is Committed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferTarget {
    private TransferTarget() { }
    public sealed record Board : TransferTarget;
    internal sealed record Bundle(DataObject Data) : TransferTarget;
    public static readonly TransferTarget Clipboard = new Board();

    internal IDataObject Surface => Switch(
        board: static _ => global::Eto.Forms.Clipboard.Instance,
        bundle: static target => target.Data);

    internal Stream ReadStream(string type) => Switch(
        state: type,
        board: static (value, _) => global::Eto.Forms.Clipboard.Instance.GetDataStream(value),
        bundle: static (value, target) => target.Data.GetDataStream(value));

    internal Unit WriteStream(string type, Stream value) => Switch(
        state: (Type: type, Value: value),
        board: static (row, _) => Op.Side(() => global::Eto.Forms.Clipboard.Instance.SetDataStream(row.Value, row.Type)),
        bundle: static (row, target) => Op.Side(() => target.Data.SetDataStream(row.Value, row.Type)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DragPlan(Seq<PayloadSlot> Slots, DragEffects Permitted, Option<(Image Image, PointF Offset)> Ghost);
public readonly record struct DragReceipt(int Payloads, DragEffects Permitted);
public readonly record struct DropReceipt(PointF Location, DragEffects Allowed, DragEffects Resolved);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class Drop {
    private readonly DragEventArgs provider;

    private Drop(DragEventArgs provider) {
        this.provider = provider;
        Location = provider.Location;
        Allowed = provider.AllowedEffects;
        Payload = new TransferTarget.Bundle(provider.Data);
    }

    public PointF Location { get; }
    public DragEffects Allowed { get; }
    public TransferTarget Payload { get; }

    public static Fin<Drop> Of(DragEventArgs provider, Op? key = null) =>
        Optional(provider)
            .ToFin(new UiFault.Rejected(Key: key.OrDefault(), Field: nameof(provider), Reason: "drop provider is absent"))
            .Map(static admitted => new Drop(admitted));

    public Fin<DropReceipt> Resolve(DragEffects effect, Option<(string Format, string Inner)> description, Op? key = null) {
        Op op = key.OrDefault();
        return (effect & Allowed) != effect
            ? Fin.Fail<DropReceipt>(new UiFault.Rejected(
                Key: op, Field: nameof(effect), Reason: "resolved drop effect is outside the provider allowance"))
            : op.Catch(() => {
                DragEffects prior = provider.Effects;
                return op.Catch(() => {
                    _ = description.Iter(row => provider.SetDropDescription(row.Format, row.Inner));
                    provider.Effects = effect;
                    return Fin.Succ(new DropReceipt(Location, Allowed, effect));
                }).MapFail(fault => fault.Also(op.Catch(() =>
                    Fin.Succ(Op.Side(() => provider.Effects = prior)))));
            });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Transfer {
    public static Fin<TransferReceipt> Apply(TransferTarget target, TransferOp operation, Op? key = null) =>
        key.OrDefault().Catch(() => operation.Switch(
            state: (Target: target, Key: key.OrDefault()),
            read: static (held, op) => Read(held.Target, op.Query).Match(
                Some: payload => Fin.Succ<TransferReceipt>(new TransferReceipt.Read(Some(payload))),
                None: () => op.Presence.IsRequired
                    ? Fin.Fail<TransferReceipt>(new UiFault.AbsentPayload(Key: held.Key, Payload: op.Query.Identity))
                    : Fin.Succ<TransferReceipt>(new TransferReceipt.Read(None))),
            write: static (held, op) => Fin.Succ<TransferReceipt>(new TransferReceipt.Written(
                op.Slots.Map(slot => held.Key.Catch(() => Fin.Succ(Land(held.Target, slot))).Match(
                    Succ: _ => (TransferWriteFact)new TransferWriteFact.Committed(slot.Identity),
                    Fail: fault => new TransferWriteFact.Rejected(slot.Identity, fault))))),
            inspect: static (held, _) => Fin.Succ<TransferReceipt>(new TransferReceipt.Inventory(toSeq(held.Target.Surface.Types))),
            clear: static (held, _) => Fin.Succ<TransferReceipt>(
                (Op.Side(held.Target.Surface.Clear), new TransferReceipt.Cleared()).Item2)));

    public static ValueTask<Fin<DragReceipt>> Begin(Control source, DragPlan plan, Op? key = null) {
        Op op = key.OrDefault();
        return UiThread.Run(new UiDispatch<DragReceipt>.Awaited(() => Drag(source, plan, op)), op);
    }

    private static Fin<DragReceipt> Drag(Control source, DragPlan plan, Op op) {
        Fin<DragReceipt> outcome = op.Catch(() => {
            DataObject data = new();
            return Apply(new TransferTarget.Bundle(data), new TransferOp.Write(plan.Slots), op).Bind(receipt =>
                receipt is TransferReceipt.Written written
                    ? written.Failure.Match(
                        Some: Fin.Fail<DragReceipt>,
                        None: () => op.Catch(() => {
                            _ = plan.Ghost.Match(
                                Some: ghost => Op.Side(() => source.DoDragDrop(data, plan.Permitted, ghost.Image, ghost.Offset)),
                                None: () => Op.Side(() => source.DoDragDrop(data, plan.Permitted)));
                            return Fin.Succ(new DragReceipt(written.Count, plan.Permitted));
                        }))
                    : Fin.Fail<DragReceipt>(new UiFault.Rejected(Key: op, Field: nameof(plan.Slots), Reason: "drag payload was not written")));
        });
        Fin<Unit> cleanup = plan.Slots
            .Choose(static slot => slot is PayloadSlot.Streamed streamed ? Some<Action>(streamed.Dispose) : None)
            .Drained(op);
        return outcome.Sealed(cleanup);
    }

    private static Option<PayloadSlot> Read(TransferTarget target, TransferQuery query) => query.Switch(
        state: target,
        text: static (at, _) => at.Surface.ContainsText ? Some<PayloadSlot>(new PayloadSlot.Text(at.Surface.Text)) : None,
        html: static (at, _) => at.Surface.ContainsHtml ? Some<PayloadSlot>(new PayloadSlot.Html(at.Surface.Html)) : None,
        picture: static (at, _) => at.Surface.ContainsImage ? Some<PayloadSlot>(new PayloadSlot.Picture(at.Surface.Image)) : None,
        linked: static (at, _) => at.Surface.ContainsUris ? Some<PayloadSlot>(new PayloadSlot.Linked(toSeq(at.Surface.Uris))) : None,
        bytes: static (at, op) => at.Surface.Contains(op.Key.Value)
            ? Some<PayloadSlot>(new PayloadSlot.Bytes(op.Key, Arr.create<byte>(at.Surface.GetData(op.Key.Value))))
            : None,
        streamed: static (at, op) => at.Surface.Contains(op.Key.Value) ? Some<PayloadSlot>(new PayloadSlot.Streamed(op.Key, at.ReadStream(op.Key.Value))) : None,
        boxed: static (at, op) => at.Surface.Contains(op.Key.Value)
            ? Optional(at.Surface.GetObject(op.Key.Value, op.Expected)).Map(value => (PayloadSlot)new PayloadSlot.Boxed(op.Key, value))
            : None);

    private static Unit Land(TransferTarget target, PayloadSlot slot) => slot.Switch(
        state: target,
        text: static (at, payload) => Op.Side(() => at.Surface.Text = payload.Value),
        html: static (at, payload) => Op.Side(() => at.Surface.Html = payload.Value),
        picture: static (at, payload) => Op.Side(() => at.Surface.Image = payload.Value),
        linked: static (at, payload) => Op.Side(() => at.Surface.Uris = [.. payload.Value]),
        bytes: static (at, payload) => Op.Side(() => at.Surface.SetData(payload.Value.ToArray(), payload.Key.Value)),
        streamed: static (at, payload) => at.WriteStream(payload.Key.Value, payload.Value),
        boxed: static (at, payload) => Op.Side(() => at.Surface.SetObject(payload.Value, payload.Key.Value)));
}
```

## [05]-[PRESENCE]

- Owner: `Toast` owns notification activation, `TrayLease` owns tray lifetime, and `TaskbarPulse` plus `PresenceBadge` project process status through `UiThread`.
- Entry: `ToastDelivery.Deliver`, `TrayLease.Show`, `TaskbarPulse.Apply`, and `PresenceBadge.Apply` synchronously marshal their complete presence transaction and return its `Fin`.
- Receipt: `ToastLease` and `TrayLease` extend `UiLease`, retain activation and reporter failures, and marshal deterministic release through the same owner; taskbar state is recoverable from `PulseState`.
- Growth: another progress mode is one `PulseState` case carrying exactly the evidence `Taskbar.SetProgress` consumes.
- Boundary: a notification requiring a tray indicator rejects an absent anchor before `Show`; macOS notification refusal remains a host fault.
- Boundary: `ToastDelivery` owns OS notification-center presence only; Rhino-host notification and viewport-toast presence belong to the host boundary's `Notices` and status owners, and the two surfaces never alias.
- Exemption: notification and tray event wiring, native show/hide, and deterministic detach are the presence-boundary statement seam.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ToastKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Toast identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PulseState {
    private PulseState() { }
    public sealed record Idle : PulseState;
    public sealed record Working(UnitInterval Progress) : PulseState;
    public sealed record Waiting : PulseState;
    public sealed record Paused(UnitInterval Progress) : PulseState;
    public sealed record Failed(UnitInterval Progress) : PulseState;

    internal (TaskbarProgressState State, float Progress) Project() => Switch(
        idle: static _ => (TaskbarProgressState.None, 0f),
        working: static state => (TaskbarProgressState.Progress, (float)state.Progress.Value),
        waiting: static _ => (TaskbarProgressState.Indeterminate, 0f),
        paused: static state => (TaskbarProgressState.Paused, (float)state.Progress.Value),
        failed: static state => (TaskbarProgressState.Error, (float)state.Progress.Value));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Toast(ToastKey Key, string Title, string Message, Option<Image> Content);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ToastLease : UiLease {
    private readonly EventHandler<NotificationEventArgs> activated;
    private readonly Atom<Seq<Error>> failures = Atom(Seq<Error>());
    private readonly Op key;
    private readonly Notification notification;

    internal ToastLease(
        ToastKey key,
        Notification notification,
        Action<ToastKey> onActivated,
        Action<Error> report,
        Op op) {
        this.key = op;
        this.notification = notification;
        activated = (_, args) => {
            if (string.Equals(args.UserData, key.Value, StringComparison.Ordinal))
                _ = op.Catch(() => Fin.Succ(Op.Side(() => onActivated(key)))).Match(
                    Succ: static handled => handled,
                    Fail: fault => ignore(failures.Swap(held => held.Add(fault.Reported(report, op)))));
        };
        Application.Instance.NotificationActivated += activated;
    }

    public Seq<Error> Failures => failures.Value;

    protected override Fin<Unit> Free() => UiThread.Run(
        new UiDispatch<Unit>.Blocking(() => Seq<Action>(
            () => Application.Instance.NotificationActivated -= activated,
            notification.Dispose).Drained(key)), key).Result;
}

public sealed class TrayLease : UiLease {
    private readonly EventHandler<EventArgs> activated;
    private readonly Atom<Seq<Error>> failures;
    private readonly Op key;

    private TrayLease(TrayIndicator indicator, EventHandler<EventArgs> activated, Atom<Seq<Error>> failures, Op key) {
        Indicator = indicator;
        this.activated = activated;
        this.failures = failures;
        this.key = key;
    }

    internal TrayIndicator Indicator { get; }
    public Seq<Error> Failures => failures.Value;

    public static Fin<TrayLease> Show(
        string title,
        Image image,
        ContextMenu menu,
        Action onActivated,
        Action<Error> report,
        Op? key = null) {
        Op op = key.OrDefault();
        return UiThread.Run(new UiDispatch<TrayLease>.Blocking(() => op.Catch(() => {
            TrayIndicator indicator = new() { Title = title, Image = image };
            Atom<Seq<Error>> failures = Atom(Seq<Error>());
            EventHandler<EventArgs> activated = (_, _) =>
                _ = op.Catch(() => Fin.Succ(Op.Side(onActivated))).Match(
                    Succ: static handled => handled,
                    Fail: fault => ignore(failures.Swap(held => held.Add(fault.Reported(report, op)))));
            try {
                indicator.SetMenu(menu);
                indicator.Activated += activated;
                indicator.Show();
                return Fin.Succ(new TrayLease(indicator, activated, failures, op));
            } catch {
                try { indicator.Activated -= activated; }
                finally { indicator.Dispose(); }
                throw;
            }
        })), op).Result;
    }

    protected override Fin<Unit> Free() => UiThread.Run(
        new UiDispatch<Unit>.Blocking(() => Seq<Action>(
            () => Indicator.Activated -= activated,
            Indicator.Hide,
            Indicator.Dispose).Drained(key)), key).Result;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ToastDelivery {
    public static Fin<ToastLease> Deliver(
        Toast toast,
        Action<ToastKey> onActivated,
        Option<TrayLease> anchor,
        Action<Error> report,
        Op? key = null) {
        Op op = key.OrDefault();
        Notification? notification = null;
        return UiThread.Run(new UiDispatch<ToastLease>.Blocking(() => op.Catch(() => {
            Notification owned = notification = new Notification { Title = toast.Title, Message = toast.Message, UserData = toast.Key.Value };
            _ = toast.Content.Iter(image => owned.ContentImage = image);
            if (owned.RequiresTrayIndicator && anchor.IsNone)
                return Fin.Fail<ToastLease>(new UiFault.Unavailable(Key: op, Capability: nameof(TrayIndicator)));
            owned.Show(anchor.Map(static held => held.Indicator).IfNoneUnsafe((TrayIndicator?)null));
            return Fin.Succ(new ToastLease(toast.Key, owned, onActivated, report, op));
        }).MapFail(fault => Optional(notification).Map(owned =>
            op.Catch(() => Fin.Succ(Op.Side(owned.Dispose))).Match(
                Succ: _ => fault,
                Fail: cleanup => fault + cleanup)).IfNone(fault))), op).Result;
    }
}

public static class TaskbarPulse {
    public static Fin<Unit> Apply(PulseState state, Op? key = null) {
        Op op = key.OrDefault();
        return UiThread.Run(new UiDispatch<Unit>.Blocking(() => op.Catch(() => {
            (TaskbarProgressState host, float progress) = state.Project();
            Taskbar.SetProgress(host, progress);
            return Fin.Succ(unit);
        })), op).Result;
    }
}

public static class PresenceBadge {
    public static Fin<Unit> Apply(Option<string> label, Op? key = null) {
        Op op = key.OrDefault();
        return UiThread.Run(new UiDispatch<Unit>.Blocking(() => op.Catch(() => {
            Application.Instance.BadgeLabel = label.IfNone(string.Empty);
            return Fin.Succ(unit);
        })), op).Result;
    }
}
```
