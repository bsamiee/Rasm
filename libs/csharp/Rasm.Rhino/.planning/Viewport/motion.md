# [RASM_RHINO_MOTION]

Host motion-pacing adapter (`Rasm.Rhino.Viewport`). Sampling mathematics is kernel-owned — `Easing`, `CyclePlan`, `SpringShape`, `FieldIntegrator`, and `PerceptualColor` arrive from `Rasm.Parametric` and `Rasm.Numerics`. This page owns host cadence: redraw landing, clock selection, accessibility posture, screen-rate projection, bounded frame deltas, attachment lifetime, and the `MotionPump` composition. Display-link advance reads `CADisplayLink.TargetTimestamp`; timer and idle advance through `MonotonicTimeline` over the injected `TimeProvider`.

## [01]-[INDEX]

- [02]-[REDRAW_TARGETS]: `RedrawTarget` — the frame-landing rows and their one invalidation dispatch.
- [03]-[CLOCKS_AND_GATES]: `FrameClock` rows, `FrameRatePolicy`, `MotionGate` accessibility state, and the macOS display-link pacer with screen-parameter rebinding.
- [04]-[PUMP]: `MotionScript` the kernel-sampled timeline, `MotionSample`, the `MotionPump` drive fold with retargeting, and the reduced-motion collapse.

## [02]-[REDRAW_TARGETS]

- Owner: `RedrawTarget` `[Union]` owns frame landing: no redraw, one addressed view, every document view, or one Eto canvas invalidation callback. A conduit-bound overlay uses the view case because the host repaints per view; the canvas owner hands its own `Drawable.Invalidate` closure, so this page never references the Eto control tree.
- Entry: `Invalidate(DocumentSession, Op) : Fin<Unit>` — the one dispatch; view-addressed rows resolve through the `ViewportLease` per invalidation so a closed view refuses instead of redrawing a dead handle.
- Law: a target is data on the drive, never a branch in the tick body — the pump invalidates whatever row it holds, and adding a landing surface is one case with the pump untouched.
- Boundary: invalidation requests a repaint and returns; paint itself happens on the host's draw pass — a target that blocks until pixels land inverts the host contract and is unrepresentable here.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using AppKit;
using CoreAnimation;
using Foundation;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedrawTarget {
    private RedrawTarget() { }
    public sealed record NoneCase : RedrawTarget;
    public sealed record ViewCase(ViewportTarget Target) : RedrawTarget;
    public sealed record DocumentCase : RedrawTarget;
    public sealed record CanvasCase(Action Invalidate) : RedrawTarget;

    internal Fin<Unit> Invalidate(DocumentSession session, Op key) =>
        Switch(
            (Session: session, Op: key),
            noneCase: static (_, _) => Fin.Succ(unit),
            viewCase: static (ctx, target) => ViewportLease.Of(session: ctx.Session, target: target.Target, key: ctx.Op)
                .Bind(lease => lease.Use(borrow: static row => Fin.Succ(value: Op.Side(row.View.Redraw)), key: ctx.Op)),
            documentCase: static (ctx, _) => HostThread.Run(
                work: new HostWork<Unit>.Session(
                    Document: ctx.Session,
                    Needs: [SessionNeed.Redraw],
                    Body: static document => Fin.Succ(value: Op.Side(document.Views.Redraw))),
                key: ctx.Op),
            canvasCase: static (ctx, canvas) => ctx.Op.Catch(canvas.Invalidate));
}
```

## [03]-[CLOCKS_AND_GATES]

- Owner: `FrameClock` `[Union]` owns display-link, timer, and idle pacing; `FrameRatePolicy` `[ComplexValueObject]` owns the requested frequency interval; `MotionPresentation` owns the complete accessibility posture delivered with every sample; `FrameTick` owns timestamp and delta evidence. `TimeProvider` enters once through `MotionPump.Drive` and feeds portable clock rows.
- Entry: `FrameClock.Resolve(Option<FrameRatePolicy>, Op?) : Fin<FrameClock>` admits a frequency policy and selects display link or timer, while `FrameClock.Idle()` admits background-tolerant pacing explicitly; `Start(onTick, onFault, TimeProvider, Op) : Fin<MotionAttachment>` returns one pause/resume/dispose lifecycle and preserves clock failures on the drive rail.
- Law: the display link is built from the SCREEN — `NSScreen.GetDisplayLink(target, selector)` on the key window's screen with `NSScreen.MainScreen` as the windowless fallback — and its callback advances on `CADisplayLink.TargetTimestamp`, the next presentation time; a wall-clock read inside a vsync tick double-advances across rebinds and is the deleted form.
- Law: the link lifecycle is create → `AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common)` → `Paused` toggling → `Invalidate` — an invalidated link is dead and rebuilt, never resumed; `ObserveDidChangeScreenParameters` fires on display reconfiguration and the pacer re-reads `MaximumFramesPerSecond` and rebinds the link, so a monitor swap re-rates a running animation instead of orphaning it.
- Law: tick delivery is already on the UI loop for every row — the display link attaches to the main run loop, `UITimer.Elapsed` raises on the UI thread, and `RhinoApp.Idle` is main-thread by contract — so the pump body never marshals.
- Law: the timer and idle rows derive every elapsed interval through one kernel `MonotonicTimeline` beat chain per drive — `Capture` seeds the origin, `Beat` advances ordinal, elapsed, and delta evidence — so no clock row reads or subtracts raw provider timestamps; the display-link row alone reads `TargetTimestamp`, the host's own presentation clock.
- Boundary: `Microsoft.macOS` members live only inside the platform-gated pacer (`OperatingSystem.IsMacOS()` selects the row); portable code holds `FrameClock` values and `FrameTick` facts, never an `NSScreen`, `CADisplayLink`, or `nint`.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class FrameRatePolicy {
    public static FrameRatePolicy Portable { get; } = Create(min: 30f, max: 60f, preferred: 60f);

    public float Min { get; }
    public float Max { get; }
    public float Preferred { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref float min,
        ref float max,
        ref float preferred) {
        validationError = float.IsFinite(min) && float.IsFinite(max) && float.IsFinite(preferred)
            && min > 0f && max >= min && preferred >= min && preferred <= max
            ? validationError
            : new ValidationError("frame-rate policy requires 0 < min <= preferred <= max");
    }
}

[ValueObject<double>]
public readonly partial struct FrameInterval {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value > 0.0
            ? validationError
            : new ValidationError(message: "frame interval is invalid");
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct FrameTick {
    public double Timestamp { get; }
    public double Delta { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double timestamp, ref double delta) {
        validationError = double.IsFinite(timestamp) && double.IsFinite(delta) && delta >= 0.0
            ? validationError
            : new ValidationError(message: "frame tick is invalid");
    }
}

[SmartEnum<int>]
public sealed partial class MotionAccommodation {
    public static readonly MotionAccommodation ReducedMotion = new(
        key: 0,
        active: static () => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion);
    public static readonly MotionAccommodation IncreasedContrast = new(
        key: 1,
        active: static () => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldIncreaseContrast);
    public static readonly MotionAccommodation DifferentiateWithoutColor = new(
        key: 2,
        active: static () => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldDifferentiateWithoutColor);
    public static readonly MotionAccommodation ReducedTransparency = new(
        key: 3,
        active: static () => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceTransparency);

    [UseDelegateFromConstructor]
    internal partial bool Active();
}

[ComplexValueObject]
public sealed partial class MotionPresentation {
    public FrozenSet<MotionAccommodation> Accommodations { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<MotionAccommodation> accommodations) {
        validationError = accommodations is not null
            ? validationError
            : new ValidationError(message: "motion presentation is invalid");
    }

    public bool ReduceMotion => Accommodations.Contains(MotionAccommodation.ReducedMotion);
}

internal static class MotionGate {
    internal static MotionPresentation Probe() =>
        MotionPresentation.Create(accommodations: OperatingSystem.IsMacOS()
            ? toSeq(MotionAccommodation.Items).Filter(static row => row.Active()).ToFrozenSet()
            : FrozenSet<MotionAccommodation>.Empty);
}

internal interface MotionAttachment : IDisposable {
    Unit Pause();
    Unit Resume();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameClock {
    private FrameClock() { }
    public sealed record DisplayLinkCase(FrameRatePolicy Rate) : FrameClock;
    public sealed record TimerCase(FrameInterval Interval) : FrameClock;
    public sealed record IdleCase : FrameClock;

    public static Fin<FrameClock> Resolve(Option<FrameRatePolicy> rate = default, Op? key = null) {
        FrameRatePolicy selected = rate.IfNone(FrameRatePolicy.Portable);
        return Fin.Succ(OperatingSystem.IsMacOS() && MacPacer.ScreenReachable
            ? (FrameClock)new DisplayLinkCase(Rate: selected)
            : new TimerCase(Interval: FrameInterval.Create(value: 1.0 / selected.Preferred)));
    }

    public static FrameClock Idle() => new IdleCase();

    internal Fin<MotionAttachment> Start(Action<FrameTick> onTick, Action<Error> onFault, TimeProvider provider, Op key) =>
        from _ in key.Catch(() => Fin.Succ(Op.Side(Eto.Forms.Application.Instance.EnsureUIThread)))
        from attachment in Switch(
            (OnTick: onTick, OnFault: onFault, Provider: provider, Op: key),
            displayLinkCase: static (ctx, clock) => MacPacer.Start(rate: clock.Rate, onTick: ctx.OnTick, onFault: ctx.OnFault, key: ctx.Op),
            timerCase: static (ctx, clock) =>
                from beats in TickBeats(onTick: ctx.OnTick, onFault: ctx.OnFault, provider: ctx.Provider, key: ctx.Op)
                from mount in ctx.Op.Catch(() => Fin.Succ<MotionAttachment>(Attachment.Timer(interval: (double)clock.Interval, beat: beats)))
                select mount,
            idleCase: static (ctx, _) =>
                from beats in TickBeats(onTick: ctx.OnTick, onFault: ctx.OnFault, provider: ctx.Provider, key: ctx.Op)
                from mount in ctx.Op.Catch(() => Fin.Succ<MotionAttachment>(Attachment.Idle(beat: beats)))
                select mount)
        select attachment;

    // Kernel timeline owns every interval: each drive advances its own MonotonicBeat chain, so no clock row subtracts raw provider timestamps.
    private static Fin<Action> TickBeats(Action<FrameTick> onTick, Action<Error> onFault, TimeProvider provider, Op key) =>
        from timeline in MonotonicTimeline.Of(provider: provider, key: key)
        from origin in timeline.Capture(key: key)
        let chain = Atom(Option<MonotonicBeat>.None)
        select (Action)(() => {
            _ = chain.Swap(prior => timeline.Beat(seed: prior.IfNone(origin), key: key)
                .Bind(beat => key.Catch(() => {
                    onTick(FrameTick.Create(timestamp: beat.Elapsed.TotalSeconds, delta: beat.Delta.TotalSeconds));
                    return Fin.Succ(beat);
                }))
                .Match(Succ: Some, Fail: error => {
                    onFault(error);
                    return prior;
                }));
        });
}

internal sealed class Attachment(Func<Unit> pause, Func<Unit> resume, Action release) : MotionAttachment {
    internal static readonly Attachment Completed = new(static () => unit, static () => unit, static () => { });

    internal static Attachment Timer(double interval, Action beat) {
        Eto.Forms.UITimer timer = new((_, _) => beat()) { Interval = interval };
        timer.Start();
        return new(fun(timer.Stop), fun(timer.Start), () => { timer.Stop(); timer.Dispose(); });
    }

    internal static Attachment Idle(Action beat) {
        int attached = 0;
        EventHandler handler = (_, _) => beat();
        Unit On() => Interlocked.Exchange(ref attached, 1) is 0 ? fun(() => RhinoApp.Idle += handler)() : unit;
        Unit Off() => Interlocked.Exchange(ref attached, 0) is 1 ? fun(() => RhinoApp.Idle -= handler)() : unit;
        return (On(), new Attachment(pause: Off, resume: On, release: () => _ = Off())).Item2;
    }

    public Unit Pause() => pause();
    public Unit Resume() => resume();
    public void Dispose() => release();
}

// --- [SERVICES] -----------------------------------------------------------------------------
// One Microsoft.macOS crossing: display-link pacing behind the platform gate; the link is screen-vended
// (NSScreen.GetDisplayLink); teardown is Invalidate, and a screen-parameter change rebuilds the link in place.
internal sealed class MacPacer : NSObject, MotionAttachment {
    private static readonly Selector TickSelector = new("pacerTick:");
    private readonly Action<FrameTick> onTick;
    private readonly Action<Error> onFault;
    private readonly FrameRatePolicy rate;
    private readonly Op key;
    private CADisplayLink link;
    private NSObject? screenObserver;
    private double last = double.NaN;

    private MacPacer(Action<FrameTick> onTick, Action<Error> onFault, FrameRatePolicy rate, NSScreen screen, Op key) {
        this.onTick = onTick;
        this.onFault = onFault;
        this.rate = rate;
        this.key = key;
        link = Configured(link: screen.GetDisplayLink(this, TickSelector), screen: screen);
        link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
        screenObserver = NSApplication.Notifications.ObserveDidChangeScreenParameters((_, _) =>
            _ = Guarded(key.Catch(() => Fin.Succ(Op.Side(Rebind)))));
    }

    private Unit Guarded(Fin<Unit> outcome) => outcome.Match(
        Succ: static _ => unit,
        Fail: error => {
            link.Paused = true;
            onFault(error);
            return unit;
        });

    internal static bool ScreenReachable =>
        OperatingSystem.IsMacOS() && (NSApplication.SharedApplication.KeyWindow?.Screen ?? NSScreen.MainScreen) is not null;

    internal static Fin<MotionAttachment> Start(FrameRatePolicy rate, Action<FrameTick> onTick, Action<Error> onFault, Op key) =>
        from _ in guard(OperatingSystem.IsMacOS(), key.MissingContext()).ToFin()
        from screen in Optional(NSApplication.SharedApplication.KeyWindow?.Screen ?? NSScreen.MainScreen).ToFin(Fail: key.MissingContext())
        from pacer in key.Catch(() => Fin.Succ<MotionAttachment>(new MacPacer(onTick: onTick, onFault: onFault, rate: rate, screen: screen, key: key)))
        select pacer;

    public Unit Pause() { link.Paused = true; return unit; }
    public Unit Resume() { link.Paused = false; return unit; }

    [Export("pacerTick:")]
    public void Tick(CADisplayLink sender) {
        double now = sender.TargetTimestamp;
        _ = Guarded(key.Catch(() => {
            onTick(FrameTick.Create(timestamp: now, delta: double.IsNaN(last) ? sender.Duration : now - last));
            last = now;
            return Fin.Succ(unit);
        }));
    }

    private CADisplayLink Configured(CADisplayLink link, NSScreen screen) {
        float ceiling = (float)Math.Max(1L, (long)screen.MaximumFramesPerSecond);
        float maximum = Math.Min(rate.Max, ceiling);
        float minimum = Math.Min(rate.Min, maximum);
        float preferred = Math.Clamp(rate.Preferred, minimum, maximum);
        link.PreferredFrameRateRange = CAFrameRateRange.Create(minimum: minimum, maximum: maximum, preferred: preferred);
        return link;
    }

    private void Rebind() {
        NSScreen? screen = NSApplication.SharedApplication.KeyWindow?.Screen ?? NSScreen.MainScreen;
        if (screen is null) { return; }
        bool paused = link.Paused;
        CADisplayLink replaced = Configured(link: screen.GetDisplayLink(this, TickSelector), screen: screen);
        replaced.Paused = paused;
        replaced.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
        link.Invalidate();
        link = replaced;
        last = double.NaN;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            link.Invalidate();
            screenObserver?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

## [04]-[PUMP]

- Owner: `MotionScript` `[Union]` owns kernel-sampled tween and spring plans; `MotionStepPolicy` bounds driven-spring frame deltas, and each `MotionDrive` owns its retarget atom. `MotionSample` `[Union]` carries the sampled value plus `MotionPresentation`, so reduce-motion, contrast, color differentiation, and transparency facts reach the consumer instead of becoming dead probes. `MotionDrive` owns pause, resume, retarget, completion, and disposal.
- Entry: `MotionPump.Drive(session, script, target, TimeProvider, apply, clock?, integrator?, Op?) : Fin<MotionDrive>` executes sample → apply → invalidate per tick. Accessibility probes fresh inside `Drive`; `Pause`, `Resume`, `Retarget`, `Completion`, and `Dispose` expose the complete lifecycle.
- Law: reduced motion is a collapse, not a skip — when `MotionPresentation.ReduceMotion` holds, the drive applies the terminal sample once (`t = 1` for a tween, the settled state for a spring), invalidates once, and completes; perceivable state changes still land, motion does not.
- Law: the tick body computes nothing — `CyclePlan.Phase`, `Easing.Evaluate`, and `SpringShape.Step` are the kernel calls, the apply is the consumer's, the invalidation is the target row's; a numeric expression in the pump beyond elapsed-time bookkeeping is the census defect this page exists to kill.
- Law: `MotionValue` preserves every finite easing result, including overshoot; a consumer whose domain is bounded performs its own projection at that boundary.
- Law: one injected `Lock` serializes tick, clock fault, pause, resume, retarget, and disposal transitions; disposal waits for an in-flight tick and no callback begins host work after release.
- Law: spring settling is evidence-driven — `|position − target| ≤ EpsilonPolicy.SqrtEpsilon · max(1, |target|)` and `|velocity| ≤ EpsilonPolicy.SqrtEpsilon` — so a drive terminates on state, never on an iteration guess; a color tween is a tween whose apply samples `PerceptualColor.Mix` at the eased parameter, needing no third script case.
- Law: `MotionDrive` latches terminal intent, pauses and releases the attachment, then publishes one typed outcome; the tick rail folds every kernel, consumer, invalidation, pause, and release fault onto that terminal before any waiter resumes.
- Law: pause, resume, retarget, and disposal serialize on one lifecycle gate — disposal waits out an in-flight call, no call reaches a disposed clock, and a released drive refuses with `InvalidContext`; `Dispose` enters the same terminal fold as natural completion.
- Boundary: one drive owns one clock attachment; concurrent drives on one target coexist because invalidation coalesces at the host — the pump never de-duplicates redraws across drives.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionSample {
    private MotionSample() { }
    public sealed record EasedCase(MotionValue Value, CyclePhase Phase, MotionPresentation Presentation) : MotionSample;
    public sealed record SprungCase(SpringState State, bool Settled, MotionPresentation Presentation) : MotionSample;
}

[ValueObject<double>]
public readonly partial struct MotionValue {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value)
            ? validationError
            : new ValidationError(message: "motion value is invalid");
    }
}

[ValueObject<double>]
public readonly partial struct MotionPeriod {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value > 0.0
            ? validationError
            : new ValidationError(message: "motion period is invalid");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionScript {
    private MotionScript() { }
    public sealed record TweenCase(Easing Curve, MotionPeriod Period, CyclePlan Plan) : MotionScript;
    public sealed record SpringCase(SpringShape Shape, SpringState From, double Target, MotionStepPolicy Step) : MotionScript;

    public static Fin<MotionScript> Tween(Easing curve, double periodSeconds, Option<CyclePlan> plan = default, Op? key = null) {
        Op op = key.OrDefault();
        return from row in op.Need(value: curve)
               from period in op.AcceptValidated<MotionPeriod>(candidate: periodSeconds)
               from cycle in plan.Match(Some: Fin.Succ, None: () => CyclePlan.Of(count: Some(1), yoyo: false, key: op))
               select (MotionScript)new TweenCase(Curve: row, Period: period, Plan: cycle);
    }

    public static Fin<MotionScript> Spring(
        SpringShape shape,
        SpringState from,
        double target,
        Option<MotionStepPolicy> step = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(shape.IsValid && from.IsValid, op.InvalidInput()).ToFin()
               from goal in op.Finite(value: target)
               select (MotionScript)new SpringCase(
                   Shape: shape,
                   From: from,
                   Target: goal,
                   Step: step.IfNone(MotionStepPolicy.Interactive));
    }
}

[ComplexValueObject]
public sealed partial class MotionStepPolicy {
    public static MotionStepPolicy Interactive { get; } = Create(
        minimumSeconds: 1.0 / 240.0,
        maximumSeconds: 1.0 / 30.0);

    public double MinimumSeconds { get; }
    public double MaximumSeconds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double minimumSeconds,
        ref double maximumSeconds) {
        validationError = double.IsFinite(minimumSeconds) && double.IsFinite(maximumSeconds)
            && minimumSeconds > 0.0 && maximumSeconds >= minimumSeconds
                ? validationError
                : new ValidationError(message: "motion step policy is invalid");
    }

    public double Clamp(double seconds) => Math.Clamp(seconds, MinimumSeconds, MaximumSeconds);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class MotionDrive : IDisposable {
    private readonly Lock gate;
    private readonly MotionAttachment clock;
    private readonly Option<Atom<double>> springTarget;
    private readonly Func<Fin<Unit>, Fin<Unit>> finish;
    private bool released;

    internal MotionDrive(
        Lock gate,
        MotionAttachment clock,
        Option<Atom<double>> springTarget,
        Task<Fin<Unit>> completion,
        Func<Fin<Unit>, Fin<Unit>> finish) {
        this.gate = gate;
        this.clock = clock;
        this.springTarget = springTarget;
        this.finish = finish;
        Completion = completion;
    }

    public Task<Fin<Unit>> Completion { get; }

    public Fin<Unit> Pause(Op? key = null) {
        Op op = key.OrDefault();
        lock (gate) {
            return released || Completion.IsCompleted
                ? Fin.Fail<Unit>(op.InvalidContext())
                : op.Catch(() => Fin.Succ(clock.Pause()));
        }
    }

    public Fin<Unit> Resume(Op? key = null) {
        Op op = key.OrDefault();
        lock (gate) {
            return released || Completion.IsCompleted
                ? Fin.Fail<Unit>(op.InvalidContext())
                : op.Catch(() => Fin.Succ(clock.Resume()));
        }
    }

    public Fin<Unit> Retarget(double target, Op? key = null) {
        Op op = key.OrDefault();
        lock (gate) {
            return from _ in guard(!released && !Completion.IsCompleted, op.InvalidContext()).ToFin()
                   from goal in op.Finite(value: target)
                   from cell in springTarget.ToFin(Fail: op.InvalidInput())
                   select ignore(cell.Swap(_ => goal));
        }
    }

    public void Dispose() {
        lock (gate) {
            if (released) { return; }
            released = true;
            _ = finish(Fin.Succ(value: unit));
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MotionPump {
    public static Fin<MotionDrive> Drive(
        DocumentSession session,
        MotionScript script,
        RedrawTarget target,
        TimeProvider provider,
        Func<MotionSample, Fin<Unit>> apply,
        Option<FrameClock> clock = default,
        Option<FieldIntegrator> integrator = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from timeline in op.Need(value: script)
               from landing in op.Need(value: target)
               from clockProvider in Optional(provider).ToFin(Fail: op.MissingContext())
               from consumer in op.Need(value: apply)
               from selectedClock in clock.Match(Some: Fin.Succ, None: () => FrameClock.Resolve(key: op))
               from stepper in integrator.Match(
                   Some: value => FieldIntegrator.AdmitOrFixed(value: value, key: op),
                   None: () => FieldIntegrator.AdmitOrFixed(value: null, key: op))
               from presentation in op.Catch(() => Fin.Succ(value: MotionGate.Probe()))
               from drive in presentation.ReduceMotion
                   ? Collapsed(session: owner, timeline: timeline, presentation: presentation, landing: landing, apply: consumer, key: op)
                   : Running(session: owner, timeline: timeline, presentation: presentation, landing: landing, provider: clockProvider, apply: consumer, clock: selectedClock, stepper: stepper, key: op)
               select drive;
    }

    private static Fin<MotionDrive> Collapsed(DocumentSession session, MotionScript timeline, MotionPresentation presentation, RedrawTarget landing, Func<MotionSample, Fin<Unit>> apply, Op key) =>
        from terminal in timeline.Switch(
            (Op: key, Presentation: presentation),
            tweenCase: static (ctx, tween) =>
                from phase in tween.Plan.Phase(elapsed: (double)tween.Period * tween.Plan.Count.IfNone(1), period: (double)tween.Period, key: ctx.Op)
                from end in ctx.Op.AcceptValidated<UnitInterval>(candidate: 1.0)
                from value in ctx.Op.AcceptValidated<MotionValue>(candidate: tween.Curve.Evaluate(t: end))
                select (MotionSample)new MotionSample.EasedCase(Value: value, Phase: phase, Presentation: ctx.Presentation),
            springCase: static (ctx, spring) => Fin.Succ(
                (MotionSample)new MotionSample.SprungCase(State: new SpringState(Position: spring.Target, Velocity: 0.0), Settled: true, Presentation: ctx.Presentation)))
        from _ in key.Catch(() => apply(terminal))
        from __ in landing.Invalidate(session: session, key: key)
        select new MotionDrive(
            gate: new Lock(),
            clock: Attachment.Completed,
            springTarget: None,
            completion: Task.FromResult(Fin.Succ(value: unit)),
            finish: static outcome => outcome);

    private static Fin<MotionDrive> Running(DocumentSession session, MotionScript timeline, MotionPresentation presentation, RedrawTarget landing, TimeProvider provider, Func<MotionSample, Fin<Unit>> apply, FrameClock clock, FieldIntegrator stepper, Op key) {
        TaskCompletionSource<Fin<Unit>> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Atom<double> elapsed = Atom(0.0);
        Atom<SpringState> springState = Atom(timeline is MotionScript.SpringCase seeded ? seeded.From : new SpringState(Position: 0.0, Velocity: 0.0));
        Option<Atom<double>> retarget = timeline is MotionScript.SpringCase spring ? Some(Atom(spring.Target)) : None;
        Atom<Option<MotionAttachment>> mounted = Atom(Option<MotionAttachment>.None);
        Lock gate = new();
        Option<Fin<Unit>> pending = None;
        bool stopping = false;

        Fin<Unit> PauseMounted() => mounted.Value.Match(
            Some: attachment => key.Catch(() => Fin.Succ(value: attachment.Pause())),
            None: static () => Fin.Succ(value: unit));

        Fin<Unit> ReleaseMounted() {
            Option<MotionAttachment> owned = mounted.Value;
            _ = mounted.Swap(static _ => None);
            return owned.Match(
                Some: attachment => key.Catch(() => {
                    attachment.Dispose();
                    return Fin.Succ(value: unit);
                }),
                None: static () => Fin.Succ(value: unit));
        }

        static Fin<Unit> Merge(Fin<Unit> primary, Fin<Unit> cleanup) => cleanup.Match(
            Succ: _ => primary,
            Fail: release => primary.Match(
                Succ: _ => Fin.Fail<Unit>(error: release),
                Fail: fault => Fin.Fail<Unit>(error: fault + release)));

        Fin<Unit> Finish(Fin<Unit> primary) {
            if (done.Task.IsCompleted) { return done.Task.Result; }
            stopping = true;
            if (mounted.Value.IsNone) {
                pending = Some(primary);
                return primary;
            }
            Fin<Unit> pause = PauseMounted();
            Fin<Unit> release = ReleaseMounted();
            Fin<Unit> settled = Merge(Merge(primary, pause), release);
            _ = done.TrySetResult(settled);
            return settled;
        }

        return clock.Start(onTick: tick => {
            lock (gate) {
                if (stopping) { return; }
                double at = elapsed.Swap(total => total + Math.Max(0.0, tick.Delta));
                Fin<(MotionSample Sample, bool Finished)> advanced = key.Catch(() => timeline.Switch(
                    (At: at, Tick: tick, Spring: springState, Stepper: stepper, Target: retarget, Presentation: presentation, Op: key),
                    tweenCase: static (ctx, tween) =>
                        from phase in tween.Plan.Phase(elapsed: ctx.At, period: (double)tween.Period, key: ctx.Op)
                        from eased in ctx.Op.AcceptValidated<MotionValue>(candidate: tween.Curve.Evaluate(t: phase.Local))
                        select ((MotionSample)new MotionSample.EasedCase(Value: eased, Phase: phase, Presentation: ctx.Presentation), phase.Completed),
                    springCase: static (ctx, spring) =>
                        from target in ctx.Target.ToFin(Fail: ctx.Op.InvalidResult())
                        from step in spring.Shape.Step(
                            from: ctx.Spring.Value,
                            target: target.Value,
                            h: spring.Step.Clamp(seconds: ctx.Tick.Delta),
                            integrator: ctx.Stepper,
                            key: ctx.Op)
                        from next in step.Switch(
                            acceptedCase: accepted => Fin.Succ(accepted.Next),
                            rejectedCase: _ => Fin.Fail<SpringState>(ctx.Op.InvalidResult()))
                        from _ in Fin.Succ(ignore(ctx.Spring.Swap(_ => next)))
                        from settled in Fin.Succ(
                            Math.Abs(next.Position - target.Value) <= EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, Math.Abs(target.Value))
                            && Math.Abs(next.Velocity) <= EpsilonPolicy.SqrtEpsilon)
                        select ((MotionSample)new MotionSample.SprungCase(State: next, Settled: settled, Presentation: ctx.Presentation), settled)));
                _ = advanced
                    .Bind(frame => key.Catch(() => apply(frame.Sample))
                        .Bind(_ => landing.Invalidate(session: session, key: key))
                        .Map(_ => frame.Finished))
                    .Match(
                        Succ: finished => { if (finished) { _ = Finish(Fin.Succ(value: unit)); } },
                        Fail: error => { _ = Finish(Fin.Fail<Unit>(error)); });
            }
        }, onFault: error => {
            lock (gate) {
                if (stopping) { return; }
                _ = Finish(Fin.Fail<Unit>(error));
            }
        }, provider: provider, key: key).Bind(attachment => {
            lock (gate) {
                _ = mounted.Swap(_ => Some(attachment));
                _ = pending.Iter(primary => { _ = Finish(primary); });
                return Fin.Succ(new MotionDrive(
                    gate: gate,
                    clock: attachment,
                    springTarget: retarget,
                    completion: done.Task,
                    finish: Finish));
            }
        });
    }
}
```

```mermaid
flowchart LR
    Kernel["Rasm.Parametric Easing · CyclePlan · SpringShape / Rasm.Numerics PerceptualColor · FieldIntegrator"] -->|samples| Pump["MotionPump.Drive"]
    Clock["FrameClock — CADisplayLink · UITimer · RhinoApp.Idle"] -->|FrameTick — TargetTimestamp| Pump
    Gate["MotionGate — NSWorkspace reduce-motion family"] -->|collapse decision| Pump
    Screen["NSScreen.MaximumFramesPerSecond · ObserveDidChangeScreenParameters"] -->|FrameRatePolicy · rebind| Clock
    Pump -->|MotionSample| Apply["consumer apply — camera pose · conduit state · canvas paint"]
    Pump -->|RedrawTarget rows| Land["RhinoView.Redraw · Views.Redraw · canvas Invalidate"]
    Pump -->|Retarget| Spring["Atom&lt;double&gt; spring goal"]
```
