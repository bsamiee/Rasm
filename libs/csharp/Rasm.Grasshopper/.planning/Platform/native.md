# [RASM_GRASSHOPPER_PLATFORM_NATIVE]

`MacGate` heads the macOS-native boundary — platform admission, managed-to-AppKit view extraction, rich input monitors, gesture and pressure attachment, workspace accessibility facts, screen-bound pacing evidence, and every inverse native lifecycle. `MacGate` requires both a macOS process and the active valid `Eto.Mac.Platform`; `EtoDispatch` owns UI affinity; `Lease<T>` bounds every retained observer or attachment; and deferred native callbacks record faults instead of throwing through the AppKit pump.

## [01]-[INDEX]

- [02]-[GATE_AND_ANCHOR]: `MacGate` + `MacViewRole` + `AnchorSource` + `MacAnchor` — dual platform admission and explicit `IMacControlHandler` view-role extraction.
- [03]-[INPUT]: `NativeInput` + `MonitorPlan` + `NativeMonitor` — ABI-faithful `NSEvent` evidence, absorption policy, callback containment, and leased monitor teardown.
- [04]-[GESTURE_AND_PRESSURE]: `GestureKind` + `GesturePlan` + `GestureBinding` + `PressureBinding` — typed recognizer minting, callback evidence, configuration, and exact UI-affine inverses.
- [05]-[WORKSPACE]: `AccessibilityAxis` + `AccessibilityPosture` + `PaceBounds` + `WorkspaceFact` + `WorkspaceWatch` — initial and changing accessibility facts with anchor-screen retuning evidence.

## [02]-[GATE_AND_ANCHOR]

- Owner: `MacGate.Demand(Op?)` → `Fin<Unit>` admits only when `OperatingSystem.IsMacOS()` holds and `Eto.Platform.Instance` is a valid `Eto.Mac.Platform` whose `IsMac` row is true. Process identity alone cannot prove that Eto loaded the AppKit backend, and an Eto platform claim alone cannot legalize AppKit on another operating system.
- Owner: `MacViewRole` `[SmartEnum<int>]` closes the five `NSView` roles inherited by `IMacViewHandler` from `IMacControlHandler`: `Container`, `Content`, `Event`, `Focus`, and `TextInput`. Each row carries its exact selector delegate over `IMacControlHandler`, which is precisely what `MacControlExtensions.GetMacControl(this Control)` returns, so the extraction and the role selection type-match with no intermediate probe. No `IMacViewHandler.Control` member exists, so extraction always states which native role the consumer needs.
- Owner: `AnchorSource` `[Union]` distinguishes `CanvasCase(Canvas)` from `ControlCase(Control, MacViewRole)`. `MacAnchor.Of` gates first, marshals once, extracts through `MacControlExtensions` — `GetContainerView(Widget)` for the canvas and `GetMacControl(Control)` for the role read — and captures the live view, optional window, bounds, per-view appearance, and the `IMacViewHandler` the projected-input column consumes. Any missing handler, view, or platform becomes a typed refusal.
- Law: extraction is the host's own chain, never a cast — `Handler as IMacControlHandler` resolves `null` for any composed Eto control whose handler is a managed shim and whose real control is nested one hop down (a `Panel` wrapping a `Drawable`, a themed handler, an embedded `NativeControlHost`), so a bare cast reads as "no macOS handler" where the handler exists. `GetMacControl` walks that `ControlObject` chain and `GetContainerView` walks it and then admits a direct `NSView`, which is exactly the canvas arm's own terminal read — so one extraction subsumes both arms with strictly wider reach, and `api-eto-platform.md` `[04]-[LOCAL_ADMISSION]` already rules it the admission path. `GetMacViewHandler` walks that same chain one interface narrower, so the anchor reads its role through `GetMacControl` and its behaviour handler through `GetMacViewHandler` inside the one extraction — a control whose handler is an `IMacWindow` keeps its role and carries no behaviour handler, which is the projected column's honest absence rather than a refusal.
- Law: appearance is anchor evidence, not a workspace fact — `MacControlExtensions.HasDarkTheme(NSView)` reads `EffectiveAppearance` per view and falls back to `NSAppearance.CurrentAppearance`, so a canvas on a differently-appearanced screen or inside a scoped-appearance panel answers for itself where the process-wide read answers for the key window; `[05]`'s posture republishes it on every screen-parameter and display-options notification through the anchor it already re-reads.
- Law: an anchor is UI-affine evidence scoped to the operation or lease that consumes its view. Long-lived native attachments retain the view for their own exact inverse, and consumers never cache anchors as ambient host state.
- Entry: `NativeSeam.Convert(MacAnchor, CGPoint, Option<NSView> source, Op?)` → `Fin<CGPoint>` owns `NSView.ConvertPointFromView`; a `None` source denotes window coordinates.
- Packages: Grasshopper2 (`Canvas`), Eto (`Platform.Instance`, `Widget`, `Control`), Eto.macOS (`Eto.Mac.Platform`, `IMacControlHandler`, `IMacViewHandler`, `MacControlExtensions.GetContainerView`/`GetMacControl`/`GetMacViewHandler`/`HasDarkTheme`), Microsoft.macOS (`NSView`, `NSWindow`, `NSAppearance`, `CGPoint`, `CGRect`), `Rasm.Domain`, `Eto/runtime.md` (`EtoDispatch`).
- Growth: a new managed origin is one `AnchorSource` case, and a newly admitted native role is one `MacViewRole` row; the gate and extraction rail remain unchanged.

## [03]-[INPUT]

- Owner: `NativeInput` preserves the installed `NSEvent` ABI: `NFloat` scroll, magnification, and stage-transition values; `float` rotation, pressure, and tangential pressure; native-width `nint Stage`; `NSEventModifierMask Modifiers`; and `ushort KeyCode`. Beside the raw columns it carries the managed-frame projection the host itself ships — `NSEvent.GetMouseButtons()` as an always-present `MouseButtons` decode, and `MacConversions.GetMouseEvent`/`NSEvent.ToEtoKeyEventArgs()` as the `Option`-carried `MouseEventArgs`/`KeyEventArgs` pair filled in the one `Of` fold. Native events never escape their callback; the projected args do, already detached.
- Law: raw and projected columns are one record because they answer two questions about one event and neither derives the other — the ABI columns carry `NFloat`/`nint` fidelity and the tablet, momentum, and stage axes Eto erases, while the projected columns carry the `Keys`/`MouseButtons` vocabulary `Shell/events.md` speaks. `GetMouseEvent` dereferences its handler's container view and owning widget to resolve view-space location, so `MonitorPlan.Anchor` is the pointer column's one producer — an anchored plan projects through the anchor's `GetMacViewHandler` read and an unanchored plan carries honest absence — and the stroke column is `None` for any non-keyboard event type; absence is the event's own shape, never a failed conversion.
- Owner: `MonitorPlan` carries mask, publication, absorption policy, and the optional `MacAnchor` whose handler fills the projected pointer column. `NativeMonitor.Receive` projects the event against that anchor and executes both delegates inside `Op.Catch`; a callback fault records in `LastFault` and returns the original event, preserving the responder chain rather than swallowing on uncertainty.
- Entry: `NativeSeam.Observe(MonitorPlan, Op?)` → `Fin<Lease<NativeMonitor>>` attaches one local monitor. Its owned lease marshals its idempotent inverse, calls `NSEvent.RemoveMonitor`, and disposes the returned token even when one inverse step faults.
- Law: monitor publication projects and returns. Downstream host mutation enters through its own owning session or dispatch gate, so the native callback never becomes an unbounded application-work window.
- Law: this owner is the events-algebra mirror from above — `Shell/events.md` carries no native row because the floor never imports upward, so a platform consumer composes `NativeSeam.Observe` here and publishes its projected facts under the same containment contract the `UiSource` rows carry (`Op.Catch`, exact idempotent inverse, fault retention); the `NSEventMask` vocabulary, the monitor lifetime, and the macOS gate never cross into the Shell page. That projected column pair joins the two vocabularies on ONE type, so a consumer bridging a native monitor to a `UiSource` fact reads the managed args rather than re-deriving a modifier-and-button decode the host already ships.
- Packages: Microsoft.macOS (`NSEvent`, `NSEventMask`, `NSEventType`, `NSEventPhase`, `NSEventModifierMask`, `NFloat`, `NSObject`), Eto (`MouseEventArgs`, `KeyEventArgs`, `MouseButtons`), Eto.macOS (`MacConversions.GetMouseEvent`/`GetMouseButtons`/`ToEtoKeyEventArgs`, `IMacViewHandler`), Microsoft.Extensions.Logging.Abstractions (`[LoggerMessage]`), `Rasm.Domain` (`Op`, `Lease<T>`, `ValidityClaim`), `Eto/runtime.md` (`EtoDispatch`), `Shell/telemetry.md` (`GhLog` — every native lease fault emits once at its `Record` site).
- Growth: a new event axis is one ABI-faithful field on `NativeInput`; a new managed-frame correspondence is one projected column filled in the same `Of` fold; a new monitor scope is data in `NSEventMask`.

## [04]-[GESTURE_AND_PRESSURE]

- Owner: `GestureKind` `[SmartEnum<int>]` closes the installed recognizer constructors: click, pan, magnification, rotation, and press. Each row mints its concrete recognizer through the verified `Action` constructor. `GesturePlan` carries one pre-attachment configuration delegate and one `GestureInput` publisher; the evidence contains kind, native state, and location in the bound view without exposing the recognizer.
- Entry: `NativeSeam.Gesture(MacAnchor, GesturePlan, Op?)` → `Fin<Lease<GestureBinding>>` mints, configures, attaches, and owns the recognizer. Deferred action callbacks run inside `Op.Catch` and record in the binding's fault cell. Disposal marshals, removes the recognizer from its original view, and disposes it exactly once.
- Owner: `PressureBinding` owns a verified `NSPressureConfiguration(NSPressureBehavior)` instance and remembers the view's prior optional configuration. `NativeSeam.Pressure(MacAnchor, NSPressureBehavior, Op?)` → `Fin<Lease<PressureBinding>>` assigns the new configuration; disposal restores the prior value and releases the owned configuration on the UI thread.
- Law: gesture configuration and observation remain one lifecycle. Every raw recognizer, target-selector bridge, and pressure configuration dies with the lease that attached it.
- Packages: Microsoft.macOS (`NSClickGestureRecognizer`, `NSPanGestureRecognizer`, `NSMagnificationGestureRecognizer`, `NSRotationGestureRecognizer`, `NSPressGestureRecognizer`, `NSGestureRecognizerState`, `NSPressureConfiguration`, `NSPressureBehavior`), `Rasm.Domain` (`Op`, `Lease<T>`), `Eto/runtime.md` (`EtoDispatch`).
- Growth: a new concrete recognizer is one `GestureKind` row; a new pressure posture is an `NSPressureBehavior` value on the existing entry.

## [05]-[WORKSPACE]

- Owner: `AccessibilityAxis` `[SmartEnum<string>]` closes the installed workspace display options — reduce motion, reduce transparency, differentiate without colour, increase contrast, invert colours — each row carrying its own `NSWorkspace` probe column. `AccessibilityPosture` is the frozen set that fold admits, and `Holds` is its one membership read, so a positional bool tuple is the deleted form: five same-typed slots carry no vocabulary, a caller reorders them silently, and a sixth option rewrites every construction. `NativeSeam.Accessibility(Op?)` reads one coherent posture on the UI thread.
- Owner: `PaceBounds` retains the screen handle, native-width `nint MaximumFramesPerSecond`, and refresh-interval bounds. `NativeSeam.Pace(MacAnchor, Op?)` resolves the anchor view's current window and screen on every read, validates the nonzero screen identity, positive native ceiling, finite positive intervals, and their order, and never substitutes `NSScreen.MainScreen` for the display hosting the view.
- Owner: `WorkspaceFact` carries one coherent accessibility-pace-appearance snapshot: the workspace-wide posture, the anchor screen's pace bounds, and the anchor VIEW's own dark-appearance verdict. `NativeSeam.Watch(MacAnchor, Action<WorkspaceFact>, Op?)` → `Fin<Lease<WorkspaceWatch>>` subscribes both `NSApplication.Notifications.ObserveDidChangeScreenParameters` and `NSWorkspace.Notifications.ObserveDisplayOptionsDidChange`, then publishes the initial snapshot. Either notification republishes the triple atomically, so a composition owner retunes screen pacing, accessibility policy, and skin selection off one lease.
- Law: appearance rides `WorkspaceFact` and not `AccessibilityPosture` because the posture is a process-wide `NSWorkspace` read while the appearance is a per-`NSView` `EffectiveAppearance` read — the same discriminant that keeps `PaceBounds` anchor-derived — so the posture stays anchor-free and reusable and the two view-scoped facts share one refresh.
- Boundary: `PaceBounds.MinimumRefreshInterval` pushes the measured frame interval down into `Eto/runtime.md`'s `EtoDispatch.Tune(StallPolicy with { Frame = … })` at composition — the S0 dispatch floor seeds a conservative slowest-refresh default and cannot read the display's rate itself, so the anchor-derived read is the one producer of the real frame budget.
- Law: notification callbacks execute projection and publication inside `Op.Catch`; later failures remain in `WorkspaceWatch.LastFault`. Disposal marshals and releases both notification tokens exactly once, attempting both inverses even when either faults.
- Packages: Microsoft.macOS (`NSWorkspace`, `NSApplication`, `NSScreen`, `NSWindow`, `NSAppearance`, `NSNotificationEventArgs`), Eto.macOS (`MacControlExtensions.HasDarkTheme`), `Rasm.Domain` (`Op`, `Lease<T>`, `ValidityClaim`), `Eto/runtime.md` (`EtoDispatch`).
- Growth: a new workspace policy axis is one `AccessibilityAxis` row with its probe column, and a new retuning value is one `WorkspaceFact` field; the posture, its fold, observation, and teardown never widen.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Foundation;
using Microsoft.Extensions.Logging;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;
using Rasm.Grasshopper.Shell;

namespace Rasm.Grasshopper.Platform;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MacViewRole {
    public static readonly MacViewRole Container = new(key: 0, select: static handler => handler.ContainerControl);
    public static readonly MacViewRole Content = new(key: 1, select: static handler => handler.ContentControl);
    public static readonly MacViewRole Event = new(key: 2, select: static handler => handler.EventControl);
    public static readonly MacViewRole Focus = new(key: 3, select: static handler => handler.FocusControl);
    public static readonly MacViewRole TextInput = new(key: 4, select: static handler => handler.TextInputControl);
    [UseDelegateFromConstructor] internal partial NSView Select(IMacControlHandler handler);
}

[SmartEnum<int>]
public sealed partial class GestureKind {
    public static readonly GestureKind Click = new(key: 0, mint: static action => new NSClickGestureRecognizer(action: action));
    public static readonly GestureKind Pan = new(key: 1, mint: static action => new NSPanGestureRecognizer(action: action));
    public static readonly GestureKind Magnification = new(key: 2, mint: static action => new NSMagnificationGestureRecognizer(action: action));
    public static readonly GestureKind Rotation = new(key: 3, mint: static action => new NSRotationGestureRecognizer(action: action));
    public static readonly GestureKind Press = new(key: 4, mint: static action => new NSPressGestureRecognizer(action: action));
    [UseDelegateFromConstructor] internal partial NSGestureRecognizer Mint(Action action);
}

[SmartEnum<string>]
public sealed partial class AccessibilityAxis {
    public static readonly AccessibilityAxis ReduceMotion = new(
        key: "reduce-motion", read: static workspace => workspace.AccessibilityDisplayShouldReduceMotion);
    public static readonly AccessibilityAxis ReduceTransparency = new(
        key: "reduce-transparency", read: static workspace => workspace.AccessibilityDisplayShouldReduceTransparency);
    public static readonly AccessibilityAxis DifferentiateWithoutColor = new(
        key: "differentiate-without-color", read: static workspace => workspace.AccessibilityDisplayShouldDifferentiateWithoutColor);
    public static readonly AccessibilityAxis IncreaseContrast = new(
        key: "increase-contrast", read: static workspace => workspace.AccessibilityDisplayShouldIncreaseContrast);
    public static readonly AccessibilityAxis InvertColors = new(
        key: "invert-colors", read: static workspace => workspace.AccessibilityDisplayShouldInvertColors);
    [UseDelegateFromConstructor] internal partial bool Read(NSWorkspace workspace);
}

[Union]
public abstract partial record AnchorSource {
    private AnchorSource() { }
    public sealed record CanvasCase(Canvas Surface) : AnchorSource;
    public sealed record ControlCase(Control Surface, MacViewRole Role) : AnchorSource;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record MacAnchor(
    NSView View, Option<NSWindow> Window, CGRect Bounds, bool DarkAppearance, Option<IMacViewHandler> Handler) {
    public static Fin<MacAnchor> Of(AnchorSource source, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in op.Need(source)
               from anchor in EtoDispatch.Run(body: () => valid.Switch(
                   state: op,
                   canvasCase: static (active, row) =>
                       from view in Optional(row.Surface.GetContainerView()).ToFin(active.MissingContext())
                       select (View: view, Handler: Optional(row.Surface.GetMacViewHandler())),
                   controlCase: static (active, row) =>
                       from role in active.Need(row.Role)
                       from handler in Optional(row.Surface.GetMacControl()).ToFin(active.MissingContext())
                       from view in active.Catch(body: () => Optional(role.Select(handler: handler)).ToFin(active.MissingContext()))
                       select (View: view, Handler: Optional(row.Surface.GetMacViewHandler())))
                   .Map(extracted => new MacAnchor(
                       View: extracted.View, Window: Optional(extracted.View.Window), Bounds: extracted.View.Bounds,
                       DarkAppearance: extracted.View.HasDarkTheme(), Handler: extracted.Handler)), key: op)
               select anchor;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NativeInput(
    NSEventType Kind, NSEventPhase Phase, NSEventPhase Momentum,
    NFloat ScrollDeltaX, NFloat ScrollDeltaY, NFloat Magnification, float Rotation,
    float Pressure, float TangentialPressure, nint Stage, NFloat StageTransition,
    NSEventModifierMask Modifiers, ushort KeyCode,
    MouseButtons Buttons, Option<MouseEventArgs> Pointer, Option<KeyEventArgs> Stroke) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: double.IsFinite((double)ScrollDeltaX)),
        ValidityClaim.Of(holds: double.IsFinite((double)ScrollDeltaY)),
        ValidityClaim.Of(holds: double.IsFinite((double)Magnification)),
        ValidityClaim.Of(holds: float.IsFinite(Rotation)),
        ValidityClaim.Of(holds: float.IsFinite(Pressure)),
        ValidityClaim.Of(holds: float.IsFinite(TangentialPressure)),
        ValidityClaim.Of(holds: double.IsFinite((double)StageTransition)));

    internal static NativeInput Of(NSEvent raw, Option<IMacViewHandler> handler) => new(
        Kind: raw.Type,
        Phase: raw.Phase,
        Momentum: raw.MomentumPhase,
        ScrollDeltaX: raw.ScrollingDeltaX,
        ScrollDeltaY: raw.ScrollingDeltaY,
        Magnification: raw.Magnification,
        Rotation: raw.Rotation,
        Pressure: raw.Pressure,
        TangentialPressure: raw.TangentialPressure,
        Stage: raw.Stage,
        StageTransition: raw.StageTransition,
        Modifiers: raw.ModifierFlags,
        KeyCode: raw.KeyCode,
        Buttons: raw.GetMouseButtons(),
        Pointer: handler.Map(view => MacConversions.GetMouseEvent(handler: view, theEvent: raw, includeWheel: true)),
        Stroke: raw.Type is NSEventType.KeyDown or NSEventType.KeyUp or NSEventType.FlagsChanged
            ? Optional(raw.ToEtoKeyEventArgs())
            : None);
}

public sealed record MonitorPlan(
    NSEventMask Mask, Action<NativeInput> Publish, Func<NativeInput, bool> Absorb, Option<MacAnchor> Anchor = default);

public sealed record GesturePlan(GestureKind Kind, Action<NSGestureRecognizer> Configure, Action<GestureInput> Publish);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GestureInput(GestureKind Kind, NSGestureRecognizerState State, CGPoint Location);

public sealed record AccessibilityPosture(FrozenSet<AccessibilityAxis> Active) {
    internal static AccessibilityPosture Of(NSWorkspace workspace) =>
        new(Active: AccessibilityAxis.Items.Where(row => row.Read(workspace: workspace)).ToFrozenSet());

    public bool Holds(AccessibilityAxis axis) => Active.Contains(item: axis);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PaceBounds(
    nint ScreenHandle,
    nint MaximumFramesPerSecond,
    double MinimumRefreshInterval,
    double MaximumRefreshInterval) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: ScreenHandle != 0),
        ValidityClaim.Of(holds: MaximumFramesPerSecond > 0),
        ValidityClaim.Positive(value: MinimumRefreshInterval),
        ValidityClaim.Positive(value: MaximumRefreshInterval),
        ValidityClaim.Ordered(lower: MinimumRefreshInterval, upper: MaximumRefreshInterval));
}

public sealed record WorkspaceFact(AccessibilityPosture Posture, PaceBounds Pace, bool DarkAppearance);

// --- [SERVICES] -----------------------------------------------------------------------------
internal static partial class NativeLog {
    [LoggerMessage(EventId = 4705, Level = LogLevel.Error, Message = "Native lease faulted: {Detail}")]
    internal static partial void LeaseFault(ILogger logger, string detail);
}

public abstract class UiNativeLease : IDisposable {
    private readonly Atom<Option<Error>> lastFault = Atom(Option<Error>.None);
    private int releaseState;

    public Option<Error> LastFault => lastFault.Value;

    public void Dispose() => ignore(Release(key: Op.Of(name: nameof(Dispose))));

    protected void Record(Error error) {
        ignore(lastFault.Swap(_ => Some(error)));
        NativeLog.LeaseFault(logger: GhLog.For(category: nameof(NativeSeam)), detail: error.Message);
    }

    protected abstract Fin<Unit> ReleaseOnUi(Op key);

    private Fin<Unit> Release(Op key) {
        if (Interlocked.CompareExchange(location1: ref releaseState, value: 1, comparand: 0) != 0) return Fin.Succ(unit);
        bool entered = false;
        Fin<Unit> outcome = EtoDispatch.Run(body: () => {
            entered = true;
            return ReleaseOnUi(key: key);
        }, key: key);
        outcome.IfFail(error => Record(error: error));
        Volatile.Write(location: ref releaseState, value: entered ? 2 : 0);
        return outcome;
    }

    internal static Fin<Unit> ReleaseAll(Op key, params Action[] inverses) {
        Option<Error> first = Option<Error>.None;
        inverses.Iter(inverse => key.Catch(body: () => Fin.Succ(Op.Side(action: inverse))).IfFail(error =>
            first = first.Match(Some: static current => Some(current), None: () => Some(error))));
        return first.Match(Some: static error => Fin.Fail<Unit>(error: error), None: static () => Fin.Succ(unit));
    }
}

public sealed class NativeMonitor : UiNativeLease {
    private readonly NSObject token;

    internal NativeMonitor(NSObject token) => this.token = token;

    internal NSEvent Receive(NSEvent raw, MonitorPlan plan, Op key) => key.Catch(body: () => {
        NativeInput evidence = NativeInput.Of(raw: raw, handler: plan.Anchor.Bind(static anchor => anchor.Handler));
        plan.Publish(obj: evidence);
        return Fin.Succ(plan.Absorb(arg: evidence));
    }).Match(
        Succ: absorbed => absorbed ? null! : raw,
        Fail: error => { Record(error: error); return raw; });

    protected override Fin<Unit> ReleaseOnUi(Op key) => ReleaseAll(
        key: key,
        () => NSEvent.RemoveMonitor(eventMonitor: token),
        token.Dispose);
}

public sealed class GestureBinding : UiNativeLease {
    private readonly NSView view;
    private readonly GestureKind kind;
    private readonly NSGestureRecognizer recognizer;
    private readonly Action<GestureInput> publish;

    internal GestureBinding(NSView view, GestureKind kind, NSGestureRecognizer recognizer, Action<GestureInput> publish) {
        this.view = view;
        this.kind = kind;
        this.recognizer = recognizer;
        this.publish = publish;
    }

    internal void Receive(Op key) => key.Catch(body: () => Fin.Succ(Op.Side(action: () => publish(obj: new GestureInput(
        Kind: kind,
        State: recognizer.State,
        Location: recognizer.LocationInView(view: view)))))).IfFail(error => Record(error: error));

    protected override Fin<Unit> ReleaseOnUi(Op key) => ReleaseAll(
        key: key,
        () => view.RemoveGestureRecognizer(recognizer: recognizer),
        recognizer.Dispose);
}

public sealed class PressureBinding : UiNativeLease {
    private readonly NSView view;
    private readonly Option<NSPressureConfiguration> prior;
    private readonly NSPressureConfiguration configuration;

    internal PressureBinding(NSView view, Option<NSPressureConfiguration> prior, NSPressureConfiguration configuration) {
        this.view = view;
        this.prior = prior;
        this.configuration = configuration;
    }

    protected override Fin<Unit> ReleaseOnUi(Op key) => ReleaseAll(
        key: key,
        () => view.PressureConfiguration = prior.MatchUnsafe(Some: static active => active, None: static () => null!),
        configuration.Dispose);
}

public sealed class WorkspaceWatch : UiNativeLease {
    private readonly MacAnchor anchor;
    private readonly Action<WorkspaceFact> publish;
    private readonly Op operation;
    private NSObject screenObserver = null!;
    private NSObject displayObserver = null!;

    internal WorkspaceWatch(MacAnchor anchor, Action<WorkspaceFact> publish, Op operation) {
        this.anchor = anchor;
        this.publish = publish;
        this.operation = operation;
    }

    internal void Bind(NSObject screen, NSObject display) {
        screenObserver = screen;
        displayObserver = display;
    }

    internal Fin<Unit> Refresh() =>
        from posture in NativeSeam.ReadAccessibility(key: operation)
        from bounds in NativeSeam.ReadPace(anchor: anchor, key: operation)
        from appearance in operation.Catch(body: () => Fin.Succ(anchor.View.HasDarkTheme()))
        from emitted in operation.Catch(body: () => Fin.Succ(Op.Side(action: () =>
            publish(obj: new WorkspaceFact(Posture: posture, Pace: bounds, DarkAppearance: appearance)))))
        select emitted;

    internal void RefreshDeferred() => operation.Catch(body: Refresh).IfFail(error => Record(error: error));

    protected override Fin<Unit> ReleaseOnUi(Op key) => ReleaseAll(
        key: key,
        screenObserver.Dispose,
        displayObserver.Dispose);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class MacGate {
    public static Fin<Unit> Demand(Op? key = null) {
        Op op = key.OrDefault();
        if (!OperatingSystem.IsMacOS())
            return Fin.Fail<Unit>(op.Unsupported(geometryType: typeof(NSView), outputType: typeof(Unit)));
        return from platform in op.Catch(body: () => Optional(global::Eto.Platform.Instance).ToFin(op.MissingContext()))
               from admitted in guard(
                   platform is global::Eto.Mac.Platform && platform.IsMac && platform.IsValid,
                   op.Unsupported(geometryType: typeof(NSView), outputType: typeof(Unit))).ToFin()
               select admitted;
    }
}

[BoundaryAdapter]
public static class NativeSeam {
    public static Fin<Lease<NativeMonitor>> Observe(MonitorPlan plan, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from valid in op.Need(plan)
               from publish in op.Need(valid.Publish)
               from absorb in op.Need(valid.Absorb)
               let admitted = valid with { Publish = publish, Absorb = absorb }
               from lease in EtoDispatch.Run(body: () => {
                   NSObject? token = null;
                   NativeMonitor? monitor = null;
                   Fin<Lease<NativeMonitor>> outcome = op.Catch(body: () => {
                       token = NSEvent.AddLocalMonitorForEventsMatchingMask(
                           mask: admitted.Mask,
                           handler: raw => monitor?.Receive(raw: raw, plan: admitted, key: op) ?? raw);
                       monitor = new NativeMonitor(token: token);
                       return Fin.Succ((Lease<NativeMonitor>)new Lease<NativeMonitor>.Owned(Value: monitor));
                   });
                   outcome.IfFail(_ => Optional(token).Iter(active => ignore(UiNativeLease.ReleaseAll(
                       key: op,
                       () => NSEvent.RemoveMonitor(eventMonitor: active),
                       active.Dispose))));
                   return outcome;
               }, key: op)
               select lease;
    }

    public static Fin<Lease<GestureBinding>> Gesture(MacAnchor anchor, GesturePlan plan, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from view in op.Need(anchor).Map(static active => active.View)
               from valid in op.Need(plan)
               from kind in op.Need(valid.Kind)
               from configure in op.Need(valid.Configure)
               from publish in op.Need(valid.Publish)
               from lease in EtoDispatch.Run(body: () => {
                   NSGestureRecognizer? recognizer = null;
                   bool attached = false;
                   GestureBinding? binding = null;
                   Fin<Lease<GestureBinding>> outcome = op.Catch(body: () => {
                       recognizer = kind.Mint(action: () => binding?.Receive(key: op));
                       configure(obj: recognizer);
                       view.AddGestureRecognizer(gestureRecognizer: recognizer);
                       attached = true;
                       binding = new GestureBinding(view: view, kind: kind, recognizer: recognizer, publish: publish);
                       return Fin.Succ((Lease<GestureBinding>)new Lease<GestureBinding>.Owned(Value: binding));
                   });
                   outcome.IfFail(_ => Optional(recognizer).Iter(active => ignore(UiNativeLease.ReleaseAll(
                       key: op,
                       () => Op.SideWhen(condition: attached, action: () => view.RemoveGestureRecognizer(gestureRecognizer: active)),
                       active.Dispose))));
                   return outcome;
               }, key: op)
               select lease;
    }

    public static Fin<Lease<PressureBinding>> Pressure(MacAnchor anchor, NSPressureBehavior behavior, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from view in op.Need(anchor).Map(static active => active.View)
               from lease in EtoDispatch.Run(body: () => {
                   NSPressureConfiguration? configuration = null;
                   Fin<Lease<PressureBinding>> outcome = op.Catch(body: () => {
                       Option<NSPressureConfiguration> prior = Optional(view.PressureConfiguration);
                       configuration = new NSPressureConfiguration(pressureBehavior: behavior);
                       view.PressureConfiguration = configuration;
                       PressureBinding binding = new(view: view, prior: prior, configuration: configuration);
                       return Fin.Succ((Lease<PressureBinding>)new Lease<PressureBinding>.Owned(Value: binding));
                   });
                   outcome.IfFail(_ => configuration?.Dispose());
                   return outcome;
               }, key: op)
               select lease;
    }

    public static Fin<CGPoint> Convert(MacAnchor anchor, CGPoint point, Option<NSView> source, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from view in op.Need(anchor).Map(static active => active.View)
               from projected in EtoDispatch.Run(body: () => op.Catch(body: () => Fin.Succ(view.ConvertPointFromView(
                   point: point,
                   view: source.MatchUnsafe(Some: static origin => origin, None: static () => null!)))), key: op)
               select projected;
    }

    public static Fin<AccessibilityPosture> Accessibility(Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from posture in EtoDispatch.Run(body: () => ReadAccessibility(key: op), key: op)
               select posture;
    }

    public static Fin<PaceBounds> Pace(MacAnchor anchor, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from active in op.Need(anchor)
               from bounds in EtoDispatch.Run(body: () => ReadPace(anchor: active, key: op), key: op)
               select bounds;
    }

    public static Fin<Lease<WorkspaceWatch>> Watch(MacAnchor anchor, Action<WorkspaceFact> publish, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in MacGate.Demand(key: op)
               from active in op.Need(anchor)
               from valid in op.Need(publish)
               from lease in EtoDispatch.Run(body: () => {
                   WorkspaceWatch watch = new(anchor: active, publish: valid, operation: op);
                   NSObject? screen = null;
                   NSObject? display = null;
                   Fin<Lease<WorkspaceWatch>> outcome = op.Catch(body: () => {
                       screen = NSApplication.Notifications.ObserveDidChangeScreenParameters(
                           handler: (_, _) => watch.RefreshDeferred());
                       display = NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(
                           handler: (_, _) => watch.RefreshDeferred());
                       watch.Bind(screen: screen, display: display);
                       return watch.Refresh().Map(_ =>
                           (Lease<WorkspaceWatch>)new Lease<WorkspaceWatch>.Owned(Value: watch));
                   });
                   outcome.IfFail(_ => {
                       Action[] inverses = (screen, display) switch {
                           ({ } screenToken, { } displayToken) => [screenToken.Dispose, displayToken.Dispose],
                           ({ } screenToken, null) => [screenToken.Dispose],
                           (null, { } displayToken) => [displayToken.Dispose],
                           _ => [],
                       };
                       ignore(UiNativeLease.ReleaseAll(key: op, inverses: inverses));
                   });
                   return outcome;
               }, key: op)
               select lease;
    }

    internal static Fin<AccessibilityPosture> ReadAccessibility(Op key) => key.Catch(body: () =>
        from workspace in Optional(NSWorkspace.SharedWorkspace).ToFin(key.MissingContext())
        select AccessibilityPosture.Of(workspace: workspace));

    internal static Fin<PaceBounds> ReadPace(MacAnchor anchor, Op key) => key.Catch(body: () =>
        from window in Optional(anchor.View.Window).ToFin(key.MissingContext())
        from screen in Optional(window.Screen).ToFin(key.MissingContext())
        let screenHandle = (nint)screen.Handle
        let maximumFrames = screen.MaximumFramesPerSecond
        let minimumInterval = screen.MinimumRefreshInterval
        let maximumInterval = screen.MaximumRefreshInterval
        from admitted in guard(
            screenHandle != 0 &&
            maximumFrames > 0 &&
            double.IsFinite(minimumInterval) && minimumInterval > 0.0 &&
            double.IsFinite(maximumInterval) && maximumInterval >= minimumInterval,
            key.InvalidResult()).ToFin()
        select new PaceBounds(
            ScreenHandle: screenHandle,
            MaximumFramesPerSecond: maximumFrames,
            MinimumRefreshInterval: minimumInterval,
            MaximumRefreshInterval: maximumInterval));
}
```

```mermaid
---
config:
  layout: elk
  elk:
    nodePlacementStrategy: NETWORK_SIMPLEX
    considerModelOrder: NODES_AND_EDGES
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: macOS platform gate, native attachments, and changing workspace facts
    accDescr: The operating system and active Eto macOS platform admit explicit native view roles; leased monitors, gestures, pressure, and workspace observers contain callbacks and marshal their inverses; anchor-screen changes publish pacing evidence for composition retuning.
    Process["macOS process"] -->|"admits process"| Gate["MacGate.Demand"]
    Eto["active valid Eto.Mac.Platform"] -->|"admits backend"| Gate
    Gate -->|"opens extraction"| Anchor["MacAnchor.Of"]
    Roles["IMacControlHandler view roles"] -->|"select native view"| Anchor
    Anchor -->|"bind native capability"| Attach["monitor · gesture · pressure"]
    Attach -->|"return custody"| Lease["UI-affine native leases"]
    Lease -->|"marshal disposal"| Inverse["remove and release natives"]
    Anchor -->|"bind screen context"| Watch["WorkspaceWatch"]
    Screen["screen-parameter notification"] -->|"request refresh"| Watch
    Display["display-options notification"] -->|"request refresh"| Watch
    Watch -->|"publish coherent pair"| Facts["accessibility + pace facts"]
    Facts -->|"drive policy and range"| Retune["composition retuning"]
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
