using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Modeling;
using Rasm.UI;
using Rhino;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class FrameTick {
    public static readonly FrameTick Next = new();
    public static readonly FrameTick Final = new();
}

public sealed record CameraMove(ViewportSet Viewports, CameraPose From, CameraPose To, TimingCurve Curve, RedrawPolicy Redraw);

internal sealed record Leg(CameraPose From, CameraPose To, Duration Start);

[Union]
internal abstract partial record Phase {
    internal sealed record Running(IDisposable Source) : Phase;

    internal sealed record Paused(Duration Since) : Phase;

    internal sealed record Stopped : Phase;
}

internal sealed record Course(Leg Leg, CameraPose Shown, Duration Excluded, Phase Phase) {
    public Duration Motion(Duration elapsed) =>
        Phase.Switch(
            elapsed,
            running: static (at, _) => at,
            paused: static (_, paused) => paused.Since,
            stopped: static (at, _) => at)
        - Excluded;

    public Option<Reduced<double>> Sample(TimingCurve curve, FrameTick tick, Duration elapsed) =>
        Phase.Map(
            running: Some(tick.Map(next: curve.Sample(Motion(elapsed) - Leg.Start), final: TimingCurve.End)),
            paused: Option<Reduced<double>>.None,
            stopped: Option<Reduced<double>>.None);

    public Course Resumed(Phase.Paused paused, Duration elapsed, IDisposable source) =>
        this with { Excluded = Excluded + (elapsed - paused.Since), Phase = new Phase.Running(source) };

    public Course Retargeted(CameraPose goal, Duration elapsed) =>
        this with { Leg = new Leg(Shown, goal, Motion(elapsed)) };
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Pacing(
    RhinoDoc Document,
    CameraMove Move,
    Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Frames,
    IO<Duration> Elapsed,
    Conduit<FrameTick, FrameTick> Ticks,
    Atom<Course> Course) {
    public IO<Unit> Halt() =>
        Course.ValueIO.Bind(held => held.Phase.Switch(
            this,
            running: static (pacing, running) => pacing.Close(IO.lift(running.Source.Dispose)),
            paused: static (pacing, _) => pacing.Close(IO.pure(unit)),
            stopped: static (_, _) => IO.pure(unit)));

    private IO<Unit> Close(IO<Unit> release) =>
        Course.SwapIO(static course => course with { Phase = new Phase.Stopped() })
            .Bind(_ => Callbacks.Each(Seq(release, Ticks.Complete()))).Map(unit).Uninterruptible();
}

public sealed class MotionHandle {
    private readonly Pacing pacing;

    internal MotionHandle(Pacing pacing, ForkIO<Unit> consumer) => (this.pacing, Completed) = (pacing, consumer.Await);

    public IO<Unit> Completed { get; }

    public IO<Unit> Pause() =>
        pacing.Course.ValueIO.Bind(held => held.Phase.Switch(
            pacing,
            running: static (state, running) =>
                from elapsed in state.Elapsed
                from swapped in state.Course.SwapIO(course => course with { Phase = new Phase.Paused(elapsed) })
                from released in IO.lift(running.Source.Dispose)
                select released,
            paused: static (_, _) => IO.pure(unit),
            stopped: static (_, _) => IO.pure(unit))).Uninterruptible();

    public IO<Unit> Resume() =>
        pacing.Course.ValueIO.Bind(held => held.Phase.Switch(
            pacing,
            running: static (_, _) => IO.pure(unit),
            paused: static (state, paused) => DisposalOps.Replace(owned =>
                from attached in state.Frames(state.Ticks.Sink, owned)
                from elapsed in state.Elapsed
                select elapsed,
                (elapsed, source) => {
                    _ = state.Course.Swap(course => course.Resumed(paused, elapsed, source));
                    return IO.pure(unit);
                }).Map(unit),
            stopped: static (_, _) => IO.fail<Unit>(Error.New("motion ended"))));

    public IO<Unit> Retarget(CameraPose goal) =>
        from held in pacing.Course.ValueIO
        from live in held.Phase.Map(running: IO.pure(unit), paused: IO.pure(unit), stopped: IO.fail<Unit>(Error.New("motion ended")))
        from elapsed in pacing.Elapsed
        from moved in pacing.Course.SwapIO(course => course.Retargeted(goal, elapsed))
        select unit;

    public IO<Unit> Stop() => pacing.Halt();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Motions {
    // --- [SOURCES]
    public static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Idle { get; } = Turns(FrameTick.Next);

    public static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Final { get; } = Turns(FrameTick.Final);

    public static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Timer(MotionSpan period) =>
        (ticks, retain) => Copies.Owned(
            () => new UITimer((_, _) => ticks.Post(FrameTick.Next).Run()) { Interval = ((Duration)period).TotalSeconds },
            static timer => IO.lift(timer.Start),
            static timer => Callbacks.Each(Seq(IO.lift(timer.Stop), IO.lift(timer.Dispose))).Map(unit), retain);

    private static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Turns(FrameTick tick) =>
        (ticks, retain) => Subscriptions.Attach<EventHandler>(
            static handler => RhinoApp.Idle += handler,
            static handler => RhinoApp.Idle -= handler,
            (_, _) => ticks.Post(tick).Run(), retain);

    // --- [POSES]
    public static Fin<CameraPose> Interpolated(CameraPose from, CameraPose to, double progress) =>
        (Refused.Unless(
                Quaternion.Slerp(Quaternion.Rotation(Plane.WorldXY, from.Frame), Quaternion.Rotation(Plane.WorldXY, to.Frame), progress).GetRotation(out Plane turned),
                turned,
                nameof(Quaternion.GetRotation)),
            Lens(from.Lens, to.Lens, progress))
        .Apply((rotated, lens) => new CameraPose(
            new Plane(new Line(from.Frame.Origin, to.Frame.Origin).PointAt(progress), rotated.XAxis, rotated.YAxis),
            new Line(from.Target, to.Target).PointAt(progress),
            lens))
        .As();

    private static Fin<CameraLens> Lens(CameraLens from, CameraLens to, double progress) =>
        to.Switch(
            (Held: from.Switch(
                    parallel: static held => Left<ParallelExtent, CameraAngle>(held.Extent),
                    perspective: static held => Right<ParallelExtent, CameraAngle>(held.Angle),
                    twoPoint: static held => Right<ParallelExtent, CameraAngle>(held.Angle)),
                Goal: to,
                Progress: progress),
            parallel: static (state, goal) => state.Held.Match(
                Left: held => ParallelExtent.Validated(double.Lerp(held, goal.Extent, state.Progress)).Map<CameraLens>(static extent => extent),
                Right: _ => state.Goal),
            perspective: static (state, goal) => state.Held.Match(
                Left: _ => state.Goal,
                Right: held => CameraAngle.Validated(double.Lerp(held, goal.Angle, state.Progress)).Map<CameraLens>(static angle => angle)),
            twoPoint: static (state, goal) => state.Held.Match(
                Left: _ => state.Goal,
                Right: held => CameraAngle.Validated(double.Lerp(held, goal.Angle, state.Progress))
                    .Map<CameraLens>(angle => new CameraLens.TwoPoint(angle, goal.Up))));

    // --- [DRIVE]
    public static IO<MotionHandle> Drive(RhinoDoc document, CameraMove move, Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> frames, TimeProvider time, IPlugInSink sink,
        Action<MotionHandle, IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from ticks in Copies.Owned(static () => Conduit.make(Buffer<FrameTick>.New), static open => Fin.Succ(open), static open => open.Complete(), owned)
        from start in IO.lift(time.GetTimestamp)
        from pacing in Copies.Owned(() => new Pacing(
            document, move, frames, IO.lift(() => time.GetElapsedTime(start).ToDuration()), ticks,
            Atom(new Course(new Leg(move.From, move.To, Duration.Zero), move.From, Duration.Zero, new Phase.Paused(Duration.Zero)))),
            static state => Fin.Succ(state), static state => state.Halt(), owned)
        from consumer in DisposalOps.Fork(
            DisposalOps.Within(() => pacing, static state => state.Ticks.Reduce(unit, (_, tick) => Frame(state, tick)), static state => state.Halt().OnUiThread())
                .IfFail(error => IO.lift(() => sink.Report(error, typeof(Motions), nameof(Drive)))
                    .MapFail(fault => error + fault).Bind(_ => IO.fail<Unit>(error))), owned)
        let handle = new MotionHandle(pacing, consumer)
        from resumed in handle.Resume()
        select handle, retain);

    private static IO<Reduced<Unit>> Frame(Pacing pacing, FrameTick tick) =>
        (from course in pacing.Course.ValueIO
         from elapsed in pacing.Elapsed
         from reduced in course.Sample(pacing.Move.Curve, tick, elapsed).Match(
             Some: sample => Show(pacing, course.Leg, sample),
             None: static () => Reduced.ContinueIO(unit))
         select reduced)
        .OnUiThread();

    private static IO<Reduced<Unit>> Show(Pacing pacing, Leg leg, Reduced<double> sample) =>
        from pose in IO.lift(Interpolated(leg.From, leg.To, sample.Value))
        from written in Navigation.ApplyToViewports(pacing.Document, pacing.Move.Viewports, port => IO.lift(() => Cameras.WritePose(port, pose)), pacing.Move.Redraw)
        from shown in pacing.Course.SwapIO(course => course with { Shown = pose })
        select sample.Map(static _ => unit);
}
