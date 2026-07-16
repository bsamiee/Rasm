# [RASM_RHINO_ETO_RUNTIME]

The ambient Eto runtime owner of `Rasm.Rhino.Eto` — the process-wide surfaces that sit beside the control tree: UI-thread dispatch over `Application`, the `UITimer` clock as a leased pulse, display and input-device projection, typed clipboard and drag-drop transfer under validated MIME keys, and system presence (notification toast, tray indicator, taskbar progress). The census routed UI-thread work through host-app dispatch and touched none of the transfer, notification, tray, screen, or static-input surface; this owner internalizes each behind one rail so downstream code composes a marshalled effect or a keyed payload and never reaches `Application.Instance`, a stringy MIME argument, or a raw static read. Dispatch is the one thread boundary every sibling page assumes: realization, binding wires, chrome mutation, and scene swaps all run inside a frame this owner marshals.

## [01]-[INDEX]

- [02]-[DISPATCH]: `UiThread` — the one UI-thread boundary: synchronous railed reads, fire-and-forget posts, awaitable marshalling, the affinity guard, and the message-pump verb.
- [03]-[CLOCK]: `Pulse` + `PulseBeat` + `PulseLease` — the widget-free `UITimer` clock as a leased resource delivering monotonic beat evidence.
- [04]-[STAGE]: `Displays` + `PointerState` + `ModifierState` + `CursorRow` — display roster and geometry, live pointer and modifier reads, and the cursor roster as rows with one apply verb.
- [05]-[TRANSFER]: `Mime` + `PayloadSlot` + `TransferTarget` + `Transfer` — the one typed-payload contract over clipboard and drag bundles, write folds and `Option`-railed reads.
- [06]-[DRAG]: `DragPlan` + `Drop` — drag initiation as a value over `DoDragDrop` and drop admission projecting `DragEventArgs` into one typed record.
- [07]-[PRESENCE]: `Toast` + `TrayLease` + `TaskbarPulse` — notification delivery, leased tray presence, and OS progress projection over a closed state row set.

## [02]-[DISPATCH]

- Owner: `UiThread` — the ONE boundary between any producer thread and the control tree. Three marshal shapes discriminate on the caller's need, never on a flag: `On<T>` blocks and returns the railed UI-side result, `Post` fires and forgets, `OnAsync<T>` returns an awaitable rail. `Guard` is the affinity assert projected to a typed `UiFault.OffThread`, and `Pump` runs one message-loop pass for a synchronous modal wait. Every body crosses through `Op.Catch`, so a throwing UI-side computation is a typed fault on the caller's rail, never an unhandled marshal exception.
- Law: sibling pages assume the frame — `Element.Realize`, `Bind.Rig` wires, `Surface.Swap`, and chrome mutation run inside a marshalled frame; the ONE self-dispatching surface is this owner, and a second dispatch mechanism (an ambient context capture, a host-app invoke) in this sub-domain is the deleted form. Rhino's own command-thread routing is the HostUi unit's shell concern; this owner is the Eto shape.
- Law: `On` from the UI thread executes inline — `Application.Instance.Invoke` already collapses the re-entrant case — so callers never pre-test affinity; `Guard` exists for entry points whose contract REQUIRES an existing frame and must refuse rather than marshal.
- Packages: LanguageExt.Core (`Fin`, `Task` interop), Rasm.Domain (project — `Op`, `Op.Catch`), Eto.Forms (host — `Application.Instance`, `Invoke`, `AsyncInvoke`, `InvokeAsync`, `EnsureUIThread`, `RunIteration`, `IsUIThread`).
- Growth: a new marshal shape the host ships is one member on this owner; a per-page dispatch helper is the deleted form.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Rhino.Eto;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class UiThread {
    public static Fin<T> On<T>(Func<Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Application.Instance.Invoke(() => op.Catch(body)));
    }

    public static Unit Post(Action body, Op? key = null) {
        Op op = key.OrDefault();
        return Op.Side(() => Application.Instance.AsyncInvoke(() => ignore(op.Catch(() => Fin.Succ(value: Op.Side(body))))));
    }

    public static async Task<Fin<T>> OnAsync<T>(Func<Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return await Application.Instance.InvokeAsync(() => op.Catch(body)).ConfigureAwait(false);
    }

    public static Fin<Unit> Guard(Op? key = null) =>
        Application.Instance.IsUIThread ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: new UiFault.OffThread(Key: key.OrDefault()));

    public static Unit Pump() => Op.Side(Application.Instance.RunIteration);
}
```

## [03]-[CLOCK]

- Owner: `Pulse` — the widget-free Eto clock as a leased resource: `Start` admits the interval as a `PositiveMagnitude` in seconds, constructs the `UITimer`, subscribes `Elapsed` once (the named platform-forced event seam), and returns a `PulseLease` whose `Halt` stops and disposes — so a running timer cannot leak past its owner. `PulseBeat` is the per-tick evidence: monotonic ordinal plus elapsed seconds derived from `Stopwatch` timestamps, because the host tick carries no time payload.
- Law: this clock paces UI cadence only — debounce displays, ambient polling, toast timeouts; frame-accurate animation pacing (display links, redraw targets, motion clocks) is the Viewport unit's motion owner, and easing/spring math is kernel territory — a duplicate temporal derivation here is the deleted form.
- Law: `Elapsed` fires on the UI thread by host contract, so the beat handler mutates UI state directly; a handler dispatching again through `UiThread` is a double-marshal defect.
- Growth: a beat-evidence axis (drift, missed-tick count) is one `PulseBeat` field computed in the one subscription body.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PulseBeat(long Ordinal, double ElapsedSeconds) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Nonnegative(value: ElapsedSeconds), ValidityClaim.Of(holds: Ordinal >= 0));
}

public sealed record PulseLease(Func<Unit> Halt);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Pulse {
    public static Fin<PulseLease> Start(PositiveMagnitude intervalSeconds, Action<PulseBeat> onBeat, Op? key = null) =>
        key.OrDefault().Catch(() => {
            long origin = Stopwatch.GetTimestamp();
            long ordinal = 0;
            UITimer timer = new((_, _) => onBeat(new PulseBeat(
                Ordinal: Interlocked.Increment(ref ordinal),
                ElapsedSeconds: Stopwatch.GetElapsedTime(startingTimestamp: origin).TotalSeconds))) {
                Interval = intervalSeconds.Value,
            };
            timer.Start();
            return Fin.Succ(value: new PulseLease(Halt: () => Op.Side(() => { timer.Stop(); timer.Dispose(); })));
        });
}
```

## [04]-[STAGE]

- Owner: `Displays` — the projection over the host `Screen` statics: roster, primary, point and rectangle resolution, per-display scale evidence (`DPI`, `Scale`, `LogicalPixelSize`, `Bounds`, `WorkingArea`), and railed region capture — plus `PointerState`/`ModifierState`, the live device reads, and `CursorRow` `[SmartEnum<int>]`, the built-in cursor roster as rows with `Apply(Control)` and the global `Override()` pointer swap. A raw static-field cursor lookup or a scattered `Screen.Screens` walk is the deleted form: consumers read one projection record.
- Law: `DisplayFacts` is evidence per display — geometry, working area, scale triple, primacy — snapshotted at read; screen topology changes re-read, never cache, because the host set mutates with hardware.
- Law: capture is railed — `Displays.Capture(bounds)` wraps `Screen.GetImage` through `Op.Catch` so a platform that refuses screen recording surfaces a typed fault, never a null image.
- Cases: `CursorRow` `Standard` · `Arrow` · `Crosshair` · `Hand` · `Move` · `Beam` · `SplitVertical` · `SplitHorizontal` · `SizeAll` · `Denied` — the verified `Cursors` roster; a custom cursor is one added row minting `Cursor(Bitmap, PointF)`.
- Growth: a new host cursor is one row; a new display fact is one `DisplayFacts` field read in one place.

```csharp
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
    public Unit Override() => Op.Side(() => Mouse.SetCursor(cursor: Resolve()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DisplayFacts(RectangleF Bounds, RectangleF WorkingArea, float Dpi, float Scale, float LogicalPixelSize, bool Primary) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Positive(value: Dpi), ValidityClaim.Positive(value: Scale));
    internal static DisplayFacts Of(Screen screen) =>
        new(Bounds: screen.Bounds, WorkingArea: screen.WorkingArea, Dpi: screen.DPI, Scale: screen.Scale, LogicalPixelSize: screen.LogicalPixelSize, Primary: screen.IsPrimary);
}

public readonly record struct PointerState(PointF Position, MouseButtons Pressed) {
    public static PointerState Read() => new(Position: Mouse.Position, Pressed: Mouse.Buttons);
    public static bool AnyLive(MouseButtons buttons) => Mouse.IsAnyButtonPressed(buttons: buttons);
    public bool Holds(MouseButtons buttons) => (Pressed & buttons) != MouseButtons.None;
}

public readonly record struct ModifierState(Keys Held) {
    public static ModifierState Read() => new(Held: Keyboard.Modifiers);
    public static bool Locked(Keys key) => Keyboard.IsKeyLocked(key: key);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Displays {
    public static Seq<DisplayFacts> Roster() => toSeq(Screen.Screens).Map(DisplayFacts.Of);

    public static DisplayFacts Primary() => DisplayFacts.Of(screen: Screen.PrimaryScreen);

    public static DisplayFacts At(PointF point) => DisplayFacts.Of(screen: Screen.FromPoint(point: point));

    public static DisplayFacts Covering(RectangleF bounds) => DisplayFacts.Of(screen: Screen.FromRectangle(rectangle: bounds));

    public static Fin<Image> Capture(RectangleF bounds, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Optional(Screen.FromRectangle(rectangle: bounds).GetImage(rect: bounds))
            .ToFin(Fail: new UiFault.Unavailable(Key: op, Capability: nameof(Screen.GetImage))));
    }
}
```

## [05]-[TRANSFER]

- Owner: `Mime` `[ValueObject<string>]` — the validated MIME key every keyed transfer read and write carries — `PayloadSlot`, the closed `[Union]` over the five host payload shapes (text, bytes, stream, boxed object, and the un-keyed URI list the host carries as the `Uris` property — file drops and copied paths), `TransferTarget`, the two-lifetime union (`Board` the persistent process-external clipboard, `Bundle` a drag-scoped `DataObject`), and `Transfer`, the one write fold plus `Option`-railed reads. `Clipboard` and `DataObject` both implement the host `IDataObject` contract, so the target union projects one `Surface` and every keyed verb runs one body over it; only the stream pair (`GetDataStream`/`SetDataStream`) is class-level off-interface, carried as the target's one `Streamed` dispatch. A stringy `type` argument, an unguarded `GetString` null, or a per-target verb copy is the deleted form.
- Law: writes fold — `Transfer.Write(target, slots)` lands every slot in one pass, each slot dispatching its own `Set*` member on the projected `Surface` (or the `Uris` property for `Linked`, the `Streamed` dispatch for `Streamed`); every keyed write shape has its read verb (`ReadText`/`ReadBytes`/`ReadStream`/`ReadBoxed`), `ReadUris` gates on `ContainsUris`, and reads gate on `Contains` first, so absence is `None` and a read of a wrong-shaped slot is the host's concern surfaced through `Op.Catch`.
- Law: canonical keys are rows — `Mime.PlainText`, `Mime.Png`, and the sub-domain's own `Mime.Rasm` (the boxed-payload key intra-process drags share) are declared once; ad-hoc literals at call sites are the deleted form.
- Growth: a new payload shape the host ships is one `PayloadSlot` case breaking the write fold at compile time; a new canonical key is one static row.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct Mime {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value: value) || !value.Contains(value: '/')
            ? new ValidationError(message: $"Mime requires a type/subtype key (got '{value}').")
            : null;
    public static readonly Mime PlainText = Create(value: "text/plain");
    public static readonly Mime Png = Create(value: "image/png");
    public static readonly Mime Rasm = Create(value: "application/x-rasm");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PayloadSlot {
    private PayloadSlot() { }
    public sealed record Text(Mime Key, string Value) : PayloadSlot;
    public sealed record Bytes(Mime Key, byte[] Value) : PayloadSlot;
    public sealed record Streamed(Mime Key, Stream Value) : PayloadSlot;
    public sealed record Boxed(Mime Key, object Value) : PayloadSlot;
    public sealed record Linked(Seq<Uri> Value) : PayloadSlot;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransferTarget {
    private TransferTarget() { }
    public sealed record Board : TransferTarget;
    public sealed record Bundle(DataObject Payload) : TransferTarget;
    public static readonly TransferTarget Clipboard = new Board();
    public static TransferTarget Of(DataObject payload) => new Bundle(Payload: payload);

    internal IDataObject Surface => Switch(
        board: static _ => (IDataObject)global::Eto.Forms.Clipboard.Instance,
        bundle: static held => held.Payload);

    internal Stream Streamed(string type) => Switch(
        state: type,
        board: static (key, _) => global::Eto.Forms.Clipboard.Instance.GetDataStream(type: key),
        bundle: static (key, held) => held.Payload.GetDataStream(type: key));

    internal Unit Streamed(Stream value, string type) => Switch(
        state: (Value: value, Type: type),
        board: static (row, _) => Op.Side(() => global::Eto.Forms.Clipboard.Instance.SetDataStream(stream: row.Value, type: row.Type)),
        bundle: static (row, held) => Op.Side(() => held.Payload.SetDataStream(stream: row.Value, type: row.Type)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Transfer {
    public static Fin<Unit> Write(TransferTarget target, Seq<PayloadSlot> slots, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: ignore(slots.Iter(slot => Land(target: target, slot: slot)))));

    public static Option<string> ReadText(TransferTarget target, Mime mime) =>
        Keyed(target: target, mime: mime, read: static (surface, type) => surface.GetString(type: type));

    public static Option<byte[]> ReadBytes(TransferTarget target, Mime mime) =>
        Keyed(target: target, mime: mime, read: static (surface, type) => surface.GetData(type: type));

    public static Option<Stream> ReadStream(TransferTarget target, Mime mime) =>
        target.Surface.Contains(type: mime.Value) ? Optional(target.Streamed(type: mime.Value)) : None;

    public static Option<Seq<Uri>> ReadUris(TransferTarget target) =>
        target.Surface is var surface && surface.ContainsUris ? Optional(surface.Uris).Map(static held => toSeq(held)) : None;

    public static Option<T> ReadBoxed<T>(TransferTarget target, Mime mime) =>
        Keyed(target: target, mime: mime, read: static (surface, type) => surface.GetObject<T>(type: type));

    public static Fin<T> Require<T>(TransferTarget target, Mime mime) =>
        ReadBoxed<T>(target: target, mime: mime).ToFin(Fail: new UiFault.AbsentPayload(Mime: mime.Value));

    public static Unit Clear(TransferTarget target) => Op.Side(target.Surface.Clear);

    private static Option<T> Keyed<T>(TransferTarget target, Mime mime, Func<IDataObject, string, T> read) =>
        target.Surface is var surface && surface.Contains(type: mime.Value) ? Optional(read(surface, mime.Value)) : None;

    private static Unit Land(TransferTarget target, PayloadSlot slot) =>
        slot.Switch(
            state: (Surface: target.Surface, Target: target),
            text: static (held, row) => Op.Side(() => held.Surface.SetString(value: row.Value, type: row.Key.Value)),
            bytes: static (held, row) => Op.Side(() => held.Surface.SetData(value: row.Value, type: row.Key.Value)),
            streamed: static (held, row) => held.Target.Streamed(value: row.Value, type: row.Key.Value),
            boxed: static (held, row) => Op.Side(() => held.Surface.SetObject(value: row.Value, type: row.Key.Value)),
            linked: static (held, row) => Op.Side(() => held.Surface.Uris = [.. row.Value]));
}
```

## [06]-[DRAG]

- Owner: `DragPlan` — drag initiation as one value: payload slots, permitted `DragEffects`, and an optional drag image with cursor offset — whose `Begin(Control)` builds the `DataObject` through the `[05]` write fold and enters the host `DoDragDrop` overload the image option selects — and `Drop`, the admission record projecting `DragEventArgs` into typed evidence (location, allowed and resolved effects, the payload as a `TransferTarget.Bundle`) at the one seam a drop handler crosses. `SetDropDescription` rides the plan as an optional annotation row.
- Law: effect negotiation is data — the handler reads `Drop.Allowed`, decides, and writes `Drop.Resolve(effect)` back through the args; scattering `Effects` writes across handlers is the deleted form.
- Law: drop handlers admit ONCE — `Drop.Of(args)` at the handler head, every subsequent read off the typed record; reaching back into raw `DragEventArgs` past the seam is the deleted form.
- Growth: a per-format preview row or a spring-loaded hover policy is one field on the plan consumed at `Begin`; a second drag entry is the deleted form.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DragPlan(Seq<PayloadSlot> Slots, DragEffects Permitted, Option<(Image Image, PointF Offset)> Ghost = default, Option<(string Format, string Inner)> Description = default) {
    public Fin<Unit> Begin(Control source, Op? key = null) {
        Op op = key.OrDefault();
        DataObject payload = new();
        return Transfer.Write(target: TransferTarget.Of(payload: payload), slots: Slots, key: op).Bind(_ =>
            op.Catch(() => Fin.Succ(value: Ghost.Match(
                Some: ghost => Op.Side(() => source.DoDragDrop(data: payload, allowedEffects: Permitted, image: ghost.Image, cursorOffset: ghost.Offset)),
                None: () => Op.Side(() => source.DoDragDrop(data: payload, allowedEffects: Permitted))))));
    }
}

public sealed record Drop(PointF Location, DragEffects Allowed, TransferTarget Payload, DragEventArgs Raw) {
    public static Drop Of(DragEventArgs args) =>
        new(Location: args.Location, Allowed: args.AllowedEffects, Payload: TransferTarget.Of(payload: args.Data), Raw: args);

    public Unit Resolve(DragEffects effect) => Op.Side(() => Raw.Effects = effect);

    public Unit Describe(string format, string inner) => Op.Side(() => Raw.SetDropDescription(format: format, inner: inner));
}
```

## [07]-[PRESENCE]

- Owner: `Toast` — notification delivery as one value (title, message, optional icon and content image, optional tray anchor) over the verified `Notification` surface — `TrayLease`, the tray presence as a leased resource (`Show` acquires, `Halt` hides and disposes) carrying its `ContextMenu` and activation callback — and `TaskbarPulse`, the OS progress projection whose `PulseState` `[SmartEnum<int>]` rows carry the host `TaskbarProgressState` column and whose fraction is a kernel `UnitInterval`, so an out-of-range progress write is unrepresentable.
- Law: tray menus are chrome projections — the `ContextMenu` a lease binds arrives from the `chrome.md` intent-table fold, so tray verbs share availability and receipts with every other placement; a tray-local menu construction is the deleted form.
- Law: progress is stateless projection — `TaskbarPulse.Show(state, fraction)` writes and forgets; the long-running work owns its own progress fold and projects here at its cadence.
- Growth: a new presence surface (badge label, dock bounce) is one owner member over its verified host member; a new progress mode is one `PulseState` row.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PulseState {
    public static readonly PulseState Idle = new(key: 0, row: TaskbarProgressState.None);
    public static readonly PulseState Working = new(key: 1, row: TaskbarProgressState.Progress);
    public static readonly PulseState Waiting = new(key: 2, row: TaskbarProgressState.Indeterminate);
    public static readonly PulseState Paused = new(key: 3, row: TaskbarProgressState.Paused);
    public static readonly PulseState Failed = new(key: 4, row: TaskbarProgressState.Error);
    internal TaskbarProgressState Row { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record Toast(string Title, string Message, Option<Icon> Badge = default, Option<Image> Content = default) {
    public Fin<Unit> Deliver(Option<TrayLease> anchor = default, Op? key = null) =>
        key.OrDefault().Catch(() => {
            Notification notification = new() { Title = Title, Message = Message };
            _ = Badge.Iter(icon => notification.Icon = icon);
            _ = Content.Iter(image => notification.ContentImage = image);
            notification.Show(indicator: anchor.Map(static lease => lease.Indicator).IfNoneUnsafe((TrayIndicator?)null));
            return Fin.Succ(value: unit);
        });
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class TrayLease {
    private TrayLease(TrayIndicator indicator) => Indicator = indicator;

    internal TrayIndicator Indicator { get; }

    public static Fin<TrayLease> Show(string title, Image image, ContextMenu menu, Action onActivated, Op? key = null) =>
        key.OrDefault().Catch(() => {
            TrayIndicator indicator = new() { Title = title, Image = image };
            indicator.SetMenu(menu: menu);
            indicator.Activated += (_, _) => onActivated();
            indicator.Show();
            return Fin.Succ(value: new TrayLease(indicator: indicator));
        });

    public Unit Halt() => Op.Side(() => { Indicator.Hide(); Indicator.Dispose(); });
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class TaskbarPulse {
    public static Unit Show(PulseState state, UnitInterval fraction) =>
        Op.Side(() => Taskbar.SetProgress(state: state.Row, progress: (float)fraction.Value));

    public static Unit Clear() => Op.Side(() => Taskbar.SetProgress(state: TaskbarProgressState.None));
}
```
