# [RASM_GRASSHOPPER_CANVAS_MOTION]

GH2's motion boundary composes host `Animated<T>` tweens, flex-frame sampling, animated glyphs, and lease-owned canvas pacing over the kernel motion module: `MotionScript`/`MotionSample`/`MotionDrive.Step` sample every drive, `UiClock`/`PulseBeat`/`FaultPosture` own the beat, `Tween.Between` owns every interpolation, and the SAMPLE IS APPLIED BY THE HOST — each drive rides beside its apply closure at this mount, the kernel names no apply hook (the ruled host-side arm). Local `DriveSpec`/`DriveFrame`/`UiCadence`/`ClockBeat` vocabulary is DELETED onto those owners.

`CanvasPacer` owns one leased clock over the injected session timeline, stops it on terminal settlement, schedules one repaint only after a sampled write set, and releases every timer edge through its lease. Budget judgment is the kernel gauge: `BudgetRow` realizes `IGaugeLane`, every bound DERIVES from the reference frame period, and a judgment answers measured `GaugedSpan<BudgetRow>` rows — the breach filter is the consumer's own `Filter(span => span.Breached)`.

## [01]-[INDEX]

- [02]-[VOCAB]: `SpanRow` + `PaceRow` — named host spans and host-motion-to-kernel substitution rows.
- [03]-[TWEENS]: `Lerp` + `Tweens` + `FlexDrive` + `FrameWindow` — kernel-backed interpolators, exact `Animated<T>` composition, and flex-frame evidence.
- [04]-[GLYPHS]: `NoticeGlyph` + `GlyphPath` — animated feedback paths over kernel figures and one time-parameterized draw.
- [05]-[PACER]: `CanvasPacer` — one lease-owned kernel clock, shared kernel sampling, host apply closures, conditional repaint, terminal stop.
- [06]-[BUDGET]: `BudgetRow` + `BudgetSubject` + `BudgetGate` — the gauge-lane vocabulary and the one read-time judgment.

## [02]-[VOCAB]

- Owner: `SpanRow` maps every live `Duration` member to `Animators.DurationToTimeSpan`; `PaceRow` maps every prompt or delayed `Motion` member to its host value and the declared kernel `Easing` substitution sampled drives read.
- Law: delayed rows retain the same kernel curve as their prompt counterpart because delay belongs to host phase policy; host tweens evaluate `MotionEquations.Blend` — the kernel column is a substitution policy, not a claim that both equations coincide.
- Law: a consumer names a span row or an exact `TimeSpan` and a pace row; raw host literals do not cross the composition gate.
- Packages: Grasshopper2 (`Motion`, `Duration`, `Animators.DurationToTimeSpan`), `Rasm.Parametric` (`Easing`), Thinktecture.
- Growth: a new host span or kind is one row keyed on its host ordinal; the kernel column absorbs the pairing.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.UI.Animation;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SpanRow {
    public static readonly SpanRow Abrupt = new(key: (int)Duration.Abrupt, host: Duration.Abrupt);
    public static readonly SpanRow Brief = new(key: (int)Duration.Brief, host: Duration.Brief);
    public static readonly SpanRow Fast = new(key: (int)Duration.Fast, host: Duration.Fast);
    public static readonly SpanRow Normal = new(key: (int)Duration.Normal, host: Duration.Normal);
    public static readonly SpanRow Slow = new(key: (int)Duration.Slow, host: Duration.Slow);
    public static readonly SpanRow Tedious = new(key: (int)Duration.Tedious, host: Duration.Tedious);
    public static readonly SpanRow Torpid = new(key: (int)Duration.Torpid, host: Duration.Torpid);
    public static readonly SpanRow Glacial = new(key: (int)Duration.Ĝlāçïāľ, host: Duration.Ĝlāçïāľ);

    public Duration Host { get; }
    public TimeSpan Span => Animators.DurationToTimeSpan(Host);
}

[SmartEnum<int>]
public sealed partial class PaceRow {
    public static readonly PaceRow Linear = new(key: (int)Motion.Linear, host: Motion.Linear, kernel: Easing.Linear);
    public static readonly PaceRow LinearDelayed = new(key: (int)Motion.LinearDelayed, host: Motion.LinearDelayed, kernel: Easing.Linear);
    public static readonly PaceRow EaseIn = new(key: (int)Motion.EaseIn, host: Motion.EaseIn, kernel: Easing.CubicIn);
    public static readonly PaceRow EaseInDelayed = new(key: (int)Motion.EaseInDelayed, host: Motion.EaseInDelayed, kernel: Easing.CubicIn);
    public static readonly PaceRow EaseOut = new(key: (int)Motion.EaseOut, host: Motion.EaseOut, kernel: Easing.CubicOut);
    public static readonly PaceRow EaseOutDelayed = new(key: (int)Motion.EaseOutDelayed, host: Motion.EaseOutDelayed, kernel: Easing.CubicOut);
    public static readonly PaceRow EaseInOut = new(key: (int)Motion.EaseInOut, host: Motion.EaseInOut, kernel: Easing.CubicInOut);
    public static readonly PaceRow EaseInOutDelayed = new(key: (int)Motion.EaseInOutDelayed, host: Motion.EaseInOutDelayed, kernel: Easing.CubicInOut);
    public static readonly PaceRow SnapIn = new(key: (int)Motion.SnapIn, host: Motion.SnapIn, kernel: Easing.QuintIn);
    public static readonly PaceRow SnapInDelayed = new(key: (int)Motion.SnapInDelayed, host: Motion.SnapInDelayed, kernel: Easing.QuintIn);
    public static readonly PaceRow SnapOut = new(key: (int)Motion.SnapOut, host: Motion.SnapOut, kernel: Easing.QuintOut);
    public static readonly PaceRow SnapOutDelayed = new(key: (int)Motion.SnapOutDelayed, host: Motion.SnapOutDelayed, kernel: Easing.QuintOut);
    public static readonly PaceRow Bounce = new(key: (int)Motion.Bounce, host: Motion.Bounce, kernel: Easing.BounceOut);
    public static readonly PaceRow BounceDelayed = new(key: (int)Motion.BounceDelayed, host: Motion.BounceDelayed, kernel: Easing.BounceOut);
    public static readonly PaceRow Twang = new(key: (int)Motion.Twang, host: Motion.Twang, kernel: Easing.ElasticOut);
    public static readonly PaceRow TwangDelayed = new(key: (int)Motion.TwangDelayed, host: Motion.TwangDelayed, kernel: Easing.ElasticOut);

    public Motion Host { get; }
    public Easing Kernel { get; }
}
```

## [03]-[TWEENS]

- Owner: `Lerp` — the ONE adapter from kernel interpolation onto the host `Interpolate<T>` delegate: `Of` lifts a kernel `Tween.Between` member (the clamp admission folds once, here), and `Perceptual` lifts the kernel colour blend with its refusals PARKED on the composition's `FaultCell` — a rejected intermediate sample holds the nearest endpoint visually while the fault is attributable evidence, never a silent wrong pixel. Five hand Eto interpolators are DELETED: the kernel `Tween` owns float, double, point, size, frame, and perceptual colour, and this page re-derives none of them.
- Owner: `Tweens` (renamed from `Tween` — the kernel owns that name) binds the host signatures: `Hold`, `Glide` (span row or exact `TimeSpan`), `Extend` (retarget through the host `Chain` fold), and `Sample`. `FlexDrive` — the per-frame drive over `IFlexControl.Animate`, `Window` projecting `FrameWindow` timing evidence and writing it through `GhInstruments.Windowed` for the surface's document, and `ZoomGate` resolving the motion-gated ZUI factor.
- Law: one tween owns one visual; chaining retargets the existing carrier without resetting motion from a stale endpoint.
- Boundary: viewport navigation animation is the host's own (`Canvas/canvas.md`'s `NavTarget` carries `Duration`); skin blending is `Skin.Interpolate`; sparkle lifecycles are host-owned on `SparkleSpec`.
- Packages: Grasshopper2 (`Animated<T>`, `Interpolate<T>`, `IFlexControl`, `ZoomThreshold`), `Rasm.Interaction` (`Tween`, `PaintColor`), `Rasm.Numerics` (`BlendPath`, `UnitInterval`, `PerceptualColor`), `Rasm.Domain` (`FaultCell`), `Shell/telemetry.md` (`GhInstruments`), LanguageExt.Core.
- Growth: a new carrier type is one kernel `Tween` member lifted through `Lerp.Of`; the binder never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Flex;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Canvas;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct FrameWindow(DateTime Start, DateTime End) : IValidityEvidence {
    public bool IsValid => End >= Start;
    public TimeSpan Cost => End - Start;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lerp {
    public static Interpolate<T> Of<T>(Func<T, T, UnitInterval, T> kernel) =>
        (a, b, t) => kernel(a, b, Factor(value: t));

    public static Interpolate<Color> Perceptual(BlendPath path, FaultCell faults) =>
        (a, b, t) => {
            UnitInterval factor = Factor(value: t);
            return (from left in PaintColor.OfHost(host: a)
                    from right in PaintColor.OfHost(host: b)
                    from mixed in Tween.Between(from: left, to: right, at: factor, path: Some(path))
                    from host in mixed.ToEto()
                    select host)
                .IfFail(cause => (ignore(faults.Park(point: Hook, cause: cause)), factor.Value >= 1d ? b : a).Item2);
        };

    private static UnitInterval Factor(double value) => UnitInterval.Create(
        value: double.IsFinite(value) ? Math.Clamp(value, 0d, 1d) : 0d);

    private static readonly HookId Hook = HookId.Create(value: "rasm.grasshopper.canvas.motion");
}

public static class Tweens {
    public static Animated<T> Hold<T>(T value, Interpolate<T> lerp) => Animated<T>.CreateFinished(value, lerp);

    public static Animated<T> Glide<T>(T from, T to, SpanRow span, PaceRow pace, Interpolate<T> lerp) =>
        Animated<T>.CreateUnfinished(from, to, span.Span, pace.Host, lerp);

    public static Animated<T> Glide<T>(T from, T to, TimeSpan span, PaceRow pace, Interpolate<T> lerp) =>
        Animated<T>.CreateUnfinished(from, to, span, pace.Host, lerp);

    public static Animated<T> Extend<T>(Animated<T> tween, T target, SpanRow span, PaceRow pace) =>
        tween.Chain(target, span.Host, pace.Host);

    public static T Sample<T>(Animated<T> tween, DateTime at) => tween.Evaluate(at);
}

public static class FlexDrive {
    public static Fin<T> Run<T>(IFlexControl surface, Animated<T> tween);
    public static Fin<FrameWindow> Window(IFlexControl surface);
    public static Fin<float> ZoomGate(IFlexControl surface, ZoomThreshold threshold);
}
```

## [04]-[GLYPHS]

- Owner: `NoticeGlyph` `[SmartEnum<int>]` — FIVE rows over the verified `AnimatedPath` factories: the four semantic glyphs and `Arrow`, now a row rather than a sibling static (the roster's one `Mint(size, angle)` column serves all five; the four spin-less rows accept and ignore the angle exactly as their host factories do — the family-absorb cost, stated). `GlyphPath` — kernel-figure construction and the unified time-parameterized draw.
- Law: `Custom` builds from KERNEL figures — `Seq<Option<PathSpec>>` where absence is the pen-up gap and a present figure lowers onto the host add (`LineCase`→`AddLine`, `PolylineCase`→`AddLines`, `EllipseCase`→`AddCircle`, `ArcCase`→`AddArc`); a figure the host path cannot stroke refuses NAMING it, and the local `StrokeStep` primitive union — a subset re-spelling of the kernel vocabulary — is deleted.
- Law: `Trace` dispatches the four host `Draw` overloads on end and pose presence through two nested `Option` folds; without `end`, `phase` admits the host's `[0,2]` grow-then-erase key, with `end` an ordered normalized segment through the accumulating admission — a refused segment names which clause failed.
- Law: glyph strokes draw inside a paint window and their time parameter comes from an existing tween or drive; a glyph never owns a clock.
- Packages: Grasshopper2 (`AnimatedPath`, `IAnimatedStroke`), `Rasm.Interaction` (`PathSpec`), `Rasm.Numerics` (`VectorAngle`, `UnitInterval`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new semantic glyph is one row; a new figure lowering is one arm named against the host surface.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.UI.Animation;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NoticeGlyph {
    public static readonly NoticeGlyph Error = new(key: 0, mint: static (size, _) => AnimatedPath.CreateErrorPath(size));
    public static readonly NoticeGlyph Warning = new(key: 1, mint: static (size, _) => AnimatedPath.CreateWarningPath(size));
    public static readonly NoticeGlyph Success = new(key: 2, mint: static (size, _) => AnimatedPath.CreateSuccessPath(size));
    public static readonly NoticeGlyph Message = new(key: 3, mint: static (size, _) => AnimatedPath.CreateMessagePath(size));
    public static readonly NoticeGlyph Arrow = new(key: 4, mint: static (size, angle) => AnimatedPath.CreateArrowPath(size, (float)angle.Value));

    [UseDelegateFromConstructor]
    internal partial AnimatedPath MintRaw(float size, VectorAngle angle);

    public Fin<AnimatedPath> Mint(float size, Option<VectorAngle> angle = default) {
        return from span in Admit.Positive(value: size)
               from path in Try.lift(() => MintRaw(size: (float)span, angle: angle.IfNone(VectorAngle.Create(value: 0d)))).Run()
               select path;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GlyphPath {
    public static Fin<AnimatedPath> Custom(Seq<Option<PathSpec>> steps) {
        return Try.lift(() => steps.Fold(Fin.Succ(new AnimatedPath()), (held, step) => held.Bind(path => step.Match(
            Some: figure => figure switch {
                PathSpec.LineCase line => Try.lift(() => (HostEdge.Side(() => path.AddLine(line.From, line.To)), path).Item2).Run(),
                PathSpec.PolylineCase poly => Try.lift(() => (HostEdge.Side(() => path.AddLines(poly.Points.ToArray())), path).Item2).Run(),
                PathSpec.EllipseCase ring => Try.lift(() => (HostEdge.Side(() => path.AddCircle(new CircleF(ring.Frame))), path).Item2).Run(),
                PathSpec.ArcCase arc => Try.lift(() => (HostEdge.Side(() => path.AddArc(new ArcF(arc.Frame, (float)arc.Start.Value, (float)arc.Sweep.Value))), path).Item2).Run(),
                _ => Fin.Fail<AnimatedPath>(new KernelFault.InvalidValue(
                    Label: figure.GetType().Name, Requirement: "a figure the host animated path strokes (line, polyline, circle, arc)")),
            },
            None: () => Fin.Succ((HostEdge.Side(path.AddGap), path).Item2))))).Run().Bind(static inner => inner);
    }

    public static Fin<Unit> Trace(
        AnimatedPath path, Graphics graphics, Pen pen, double phase, Option<UnitInterval> end,
        PointF at, Option<(float Scale, VectorAngle Angle)> pose);
}
```

## [05]-[PACER]

- Owner: `CanvasPacer` — the lease-owned GH2 clock pacer over the KERNEL clock: `Mount(cadence, drives, accessibility, clock, faults)` admits a non-empty drive set — each drive a kernel `MotionScript` BESIDE its host `Action<MotionSample>` apply closure (the ruled host apply arm; the kernel names no apply hook) — creates one inert owned `UiClock` over the injected session timeline, seats it through `Cell.Seat`, and starts it only after ownership installed. Clock callback holds weak references to pacer and clock, so the clock never roots its owner and an abandoned pacer disposes the orphaned clock on the next tick.
- Law: `MotionDrive.Step` is the shared sampling path for this pacer and the platform display link — each successful beat steps every script at `beat.Evidence`, applies every sample through its own closure under `Try.lift`, retains only continuing drives through one `Cell.Commit`, schedules ONE repaint for that write set through `GhSession.Apply(RepaintCase(Scheduled))`, and stops the clock after the terminal repaint request.
- Law: an empty live set stops defensively and schedules nothing; a sampling or apply fault RETIRES its row, the survivor roster commits and the repaint schedules FIRST (commit-then-park — moved visuals never strand unpainted), then the fault returns on the beat path where the kernel `FaultPosture.Halt` stops the clock with the cause parked on the composition's `FaultCell` — no newest-only fault column exists here.
- Law: custody is the kernel idiom — the clock seat is `Cell.Seat` over an option cell (a doubled mount reads `Ceded` and disposes its surplus with the refusal AGGREGATED), the release one-shot is the `Atom<bool>` latch through `Cell.Step`, and the drive roster advances through `Cell.Commit` — the two interlocked integer ladders and the five discarded swaps are unspellable.
- Boundary: drive writes update consumer state; the repaint renders it in the next paint window — this pacer never writes host visuals directly. Composition root's mount roster reaches this owner, and this pacer is the one mount of `Platform/layers.md`'s `MotionAttachment` where a drive belongs on the compositor instead of the paint clock.
- Packages: `Rasm.Interaction` (`UiClock`, `PulseBeat`, `FaultPosture`, `Accessibility`), `Rasm.Parametric` (`MotionScript`, `MotionSample`, `MotionDrive`, `MonotonicTimeline`), `Rasm.Domain` (`Lease<T>`, `FaultCell`, `Cell`), `Shell/session.md` (`GhSession`, `SessionOp.RepaintCase`, `RepaintRow`), `Rasm.Numerics` (`PositiveMagnitude`).
- Growth: a new drive shape is one kernel `MotionScript` case; neither pacer gains a parallel sampling arm.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Grasshopper.Canvas;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CanvasPacer : IDisposable {
    private readonly Atom<Seq<(MotionScript Script, Action<MotionSample> Apply)>> live;
    private readonly CapabilitySet<Accessibility> accessibility;
    private readonly FaultCell faults;
    private readonly Atom<Option<Lease<UiClock>>> clock = Atom(Option<Lease<UiClock>>.None);
    private readonly Atom<bool> released = Atom(false);

    public static Fin<Lease<CanvasPacer>> Mount(
        PositiveMagnitude cadence,
        Seq<(MotionScript Script, Action<MotionSample> Apply)> drives,
        CapabilitySet<Accessibility> accessibility,
        MonotonicTimeline clock,
        FaultCell faults) {
        return from admitted in guard(!drives.IsEmpty, new KernelFault.InvalidInput()).ToFin()
               from scripts in drives.TraverseM(row => MotionDrive.Admit(script: row.Script).Map(_ => row)).As()
               let pacer = new CanvasPacer(drives: scripts.Strict(), accessibility: accessibility, faults: faults)
               let weakPacer = new WeakReference<CanvasPacer>(pacer)
               from owned in UiClock.Of(
                   cadence: cadence,
                   beat: beat => Tick(owner: weakPacer, beat: beat),
                   posture: Some(FaultPosture.Halt),
                   faults: Some(faults),
                   clock: Some(clock))
               from mounted in pacer.Seat(owned: owned)
               select mounted;
    }

    public void Dispose() => _ = Release();

    private Fin<Lease<CanvasPacer>> Seat(Lease<UiClock> owned) =>
        Cell.Seat(cell: clock, mint: () => owned).Switch(
            state: operation,
            committed: (op, _) => owned.Resource.Start()
                .Map(_ => (Lease<CanvasPacer>)new Lease<CanvasPacer>.Owned(Value: this)),
            ceded: (op, _) => Fin.Fail<Lease<CanvasPacer>>(new KernelFault.InvalidResult())
                .Rollback(release: () => Fin.Succ(owned.Dispose())),
            refused: static (_, row) => Fin.Fail<Lease<CanvasPacer>>(row.Cause),
            contended: static (op, _) => Fin.Fail<Lease<CanvasPacer>>(new KernelFault.InvalidResult()));

    private Fin<Unit> Advance(PulseBeat beat) {
        Seq<(MotionScript Script, Action<MotionSample> Apply)> active = live.Value;
        if (active.IsEmpty) { return Stop(); }
        (Seq<Error> faulted, Seq<((MotionScript Script, Action<MotionSample> Apply) Row, bool Continues)> stepped) = active
            .Map(row => MotionDrive.Step(script: row.Script, beat: beat.Evidence, accessibility: accessibility)
                .Bind(step => Try.lift(() => row.Apply(step.Sample)).Run().Bind(static inner => inner)
                    .Map(_ => (Row: row, step.Continues))))
            .Partition();
        Seq<(MotionScript Script, Action<MotionSample> Apply)> continuing =
            stepped.Filter(static row => row.Continues).Map(static row => row.Row).Strict();
        return Cell.Commit(live, _ => continuing).Switch(
            state: (Key: operation, Faulted: faulted),
            committed: (state, _) => GhSession.Apply(
                    op: new SessionOp.RepaintCase(Row: RepaintRow.Scheduled, Delay: None))
                .Bind(_ => state.Faulted.IsEmpty
                    ? continuing.IsEmpty ? Stop() : Fin.Succ(unit)
                    : Fin.Fail<Unit>(Error.Many(state.Faulted))),
            ceded: static (state, _) => Fin.Fail<Unit>(new KernelFault.InvalidResult()),
            refused: static (state, row) => Fin.Fail<Unit>(Error.Many([row.Cause, .. state.Faulted])),
            contended: static (state, _) => Fin.Fail<Unit>(new KernelFault.InvalidResult()));
    }

    private Fin<Unit> Stop() => clock.Value.ToFin(new KernelFault.InvalidResult())
        .Bind(owned => owned.Resource.Stop());

    private static Fin<Unit> Tick(WeakReference<CanvasPacer> owner, PulseBeat beat) =>
        owner.TryGetTarget(out CanvasPacer? active)
            ? active.Advance(beat: beat)
            : Fin.Fail<Unit>(new KernelFault.InvalidContext());

    private Fin<Unit> Release();
}
```

## [06]-[BUDGET]

- Owner: `BudgetRow` `[SmartEnum<string>]` realizing `IGaugeLane<BudgetRow>` — the closed budget vocabulary whose every bound DERIVES from the reference frame period as a dimensionless frame fraction: one row per judged cost axis, no millisecond literal anywhere, and the kernel gauge floor is what a `GaugedSpan<BudgetRow>` reads its bound from.
- Owner: `BudgetSubject` `[Union]` — the judgment ingress: one polymorphic gate discriminates on the result shape (`FrameWindow`, `FramePulse`, the kernel `PaintTally`, or a row-addressed raw cost).
- Entry: `BudgetGate.Judge(BudgetSubject subject, Option<PaceBand> pace = default)` → `Fin<Seq<GaugedSpan<BudgetRow>>>` — EVERY measured axis answers as a kernel gauged span; the pass verdict is the consumer's own `Filter(span => span.Breached)` over the sequence (NAMED LOSS: the breach-only sequence — bought back by that one filter; the judging consumer writes each breached span through `Shell/telemetry.md`'s `GhInstruments.Breached`), and `Overrun` derives on the span. Supplied `PaceBand` bounds every row as `Period × Frames` — the band's own period, no reference division — so a ProMotion panel judges at its real frame budget and an absent band reads the kernel `Portable` declared row.
- Law: judgment happens at read time over results already settled — the gate never samples, never owns a clock, and never mutates a result; a breached span is shaped for the repo benchmark-claim fold, so the app-root benchmark suite consumes it without re-measuring.
- Law: the host-free kernel families this boundary exercises carry corpus benchmark rows; this gate owns the live-session judgment, the corpus owns the regression floor, and both read the same row bounds.
- Packages: LanguageExt.Core, Thinktecture, `Rasm.Parametric` (`IGaugeLane`, `GaugedSpan`, `PaceBand`), `Rasm.Interaction` (`PaintTally`), `Canvas/canvas.md` (`FramePulse`), `Rasm.Domain`.
- Growth: a new judged axis is one row with one subject arm; a tuned bound is a row fraction change with every consumer untouched.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Parametric;
using Thinktecture;

namespace Rasm.Grasshopper.Canvas;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BudgetRow : IGaugeLane<BudgetRow> {
    public static readonly BudgetRow PaintPass = new(key: "paint.pass", frames: 0.35);
    public static readonly BudgetRow FrameDraw = new(key: "frame.draw", frames: 0.5);
    public static readonly BudgetRow FrameFull = new(key: "frame.full", frames: 1.0);
    public static readonly BudgetRow LayerGrid = new(key: "layer.grid", frames: 0.12);
    public static readonly BudgetRow LayerWire = new(key: "layer.wire", frames: 0.25);
    public static readonly BudgetRow LayerText = new(key: "layer.text", frames: 0.18);
    public static readonly BudgetRow LayerIcon = new(key: "layer.icon", frames: 0.12);
    public static readonly BudgetRow LayerShape = new(key: "layer.shape", frames: 0.3);
    public static readonly BudgetRow LayerLayout = new(key: "layer.layout", frames: 0.18);
    public static readonly BudgetRow DriveStep = new(key: "drive.step", frames: 0.12);

    public double Frames { get; }

    public TimeSpan Bound => PaceBand.Portable.Period * Frames;
}

[Union]
public abstract partial record BudgetSubject {
    private BudgetSubject() { }
    public sealed record WindowCase(FrameWindow Window) : BudgetSubject;
    public sealed record PulseCase(FramePulse Pulse) : BudgetSubject;
    public sealed record PaintCase(PaintTally Tally) : BudgetSubject;
    public sealed record StepCase(BudgetRow Row, TimeSpan Cost) : BudgetSubject;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BudgetGate {
    public static Fin<Seq<GaugedSpan<BudgetRow>>> Judge(
        BudgetSubject subject, Option<PaceBand> pace = default) {
        PaceBand band = pace.IfNone(PaceBand.Portable);
        return Admit.Need(subject).Map(valid => valid.Switch(
            state: band,
            windowCase: static (s, c) => Spans(s, Seq((BudgetRow.FrameDraw, c.Window.Cost))),
            pulseCase: static (s, c) => Spans(s, Seq(
                (BudgetRow.LayerGrid, c.Pulse.Grid), (BudgetRow.LayerWire, c.Pulse.Wire),
                (BudgetRow.LayerText, c.Pulse.Text), (BudgetRow.LayerIcon, c.Pulse.Icon),
                (BudgetRow.LayerShape, c.Pulse.Shape), (BudgetRow.LayerLayout, c.Pulse.Layout),
                (BudgetRow.FrameFull, c.Pulse.FullFrame))),
            paintCase: static (s, c) => Spans(s, Seq((BudgetRow.PaintPass, c.Tally.Span.Elapsed))),
            stepCase: static (s, c) => Spans(s, Seq((c.Row, c.Cost)))));
    }

    private static Seq<GaugedSpan<BudgetRow>> Spans(
        (PaceBand Band) state, Seq<(BudgetRow Row, TimeSpan Cost)> rows) =>
        rows.Map(row => new GaugedSpan<BudgetRow>(
            Lane: row.Row, Work: state.Key, Elapsed: row.Cost, Bound: state.Band.Period * row.Frames)).Strict();
}
```

## [07]-[DENSITY_BAR]

| [INDEX] | [CONCERN]       | [OWNER]                     | [RESULT]                                            | [CASES] |
| :-----: | :-------------- | :-------------------------- | :-------------------------------------------------- | :-----: |
|  [01]   | host vocabulary | `SpanRow` + `PaceRow`       | host-ordinal keys, kernel substitution columns      |   24    |
|  [02]   | interpolation   | `Lerp` + `Tweens`           | kernel `Tween.Between` lifted onto `Interpolate<T>` |    2    |
|  [03]   | flex sampling   | `FlexDrive` + `FrameWindow` | host draw clock, typed timing evidence              |    3    |
|  [04]   | glyphs          | `NoticeGlyph` + `GlyphPath` | five factory rows, kernel-figure lowering           |    5    |
|  [05]   | pacing          | `CanvasPacer`               | kernel clock/sampler, host apply closures, `Cell`   |    1    |
|  [06]   | budget          | `BudgetRow` + `BudgetGate`  | `IGaugeLane` fractions → `GaugedSpan` sequence      |   10    |

`MotionScript`/`MotionSample`/`MotionDrive`/`Accessibility`, `UiClock`/`PulseBeat`/`FaultPosture`, `Tween`, and `GaugedSpan`/`IGaugeLane`/`PaceBand` are kernel owners; `DriveSpec`, `DriveFrame`, `UiCadence`, `ClockBeat`, `AccessibilityPosture`, the five hand interpolators, `StrokeStep`, `BudgetBreach`, the millisecond bound table, and the two interlocked ladders deleted onto them.

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
