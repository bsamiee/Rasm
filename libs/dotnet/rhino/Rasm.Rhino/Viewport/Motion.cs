using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Styling;
using Rhino;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CameraMove(ViewportSet Viewports, CameraPose To, TimingCurve Curve, RedrawPolicy Redraw);

internal sealed record Leg(ViewportRef Row, CameraPose From, CameraPose Shown, uint Written, TimingCurve Curve);

[Union]
internal abstract partial record Phase {
    internal sealed record Running(IDisposable Source) : Phase;

    internal sealed record Paused : Phase;

    internal sealed record Stopped : Phase;
}

internal sealed record Course(Seq<Leg> Legs, CameraPose To, Duration Start, Duration Clock, Option<Duration> Last, Phase Phase) {
    public Course Ticked(Duration tick) =>
        this with { Clock = Last.Match(Some: last => Clock + (tick - last), None: () => Clock), Last = tick };

    public Course Retargeted(CameraPose goal) =>
        this with {
            To = goal,
            Start = Clock,
            Legs = Legs.Map(leg => (Was: Span(leg.From, To), Now: Span(leg.Shown, goal)) switch {
                var span => leg with { From = leg.Shown, Curve = leg.Curve.From(span.Now > 0d ? leg.Curve.Velocity(Clock - Start) * span.Was / span.Now : 0d) },
            }).Strict(),
        };

    private static double Span(CameraPose from, CameraPose to) => from.Frame.Origin.DistanceTo(to.Frame.Origin) + from.Target.DistanceTo(to.Target);
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Pacing(
    RhinoDoc Document,
    RedrawPolicy Redraw,
    Func<Sink<Duration>, Action<IDisposable>, IO<Unit>> Frames,
    Conduit<Duration, Duration> Ticks,
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
                from swapped in state.Course.SwapIO(static course => course with { Phase = new Phase.Paused(), Last = None })
                from released in IO.lift(running.Source.Dispose)
                select released,
            paused: static (_, _) => IO.pure(unit),
            stopped: static (_, _) => IO.pure(unit))).Uninterruptible();

    public IO<Unit> Resume() =>
        pacing.Course.ValueIO.Bind(held => held.Phase.Switch(
            pacing,
            running: static (_, _) => IO.pure(unit),
            paused: static (state, _) => DisposalOps.Acquire(
                owned => state.Frames(state.Ticks.Sink, owned),
                (_, source) => state.Course.SwapIO(course => course with { Phase = new Phase.Running(source), Last = None }).Map(unit)),
            stopped: static (_, _) => IO.fail<Unit>(Errors.Cancelled)));

    public IO<Unit> Retarget(CameraPose goal) =>
        from held in pacing.Course.ValueIO
        from live in held.Phase.Map(running: IO.pure(unit), paused: IO.pure(unit), stopped: IO.fail<Unit>(Errors.Cancelled))
        from moved in pacing.Course.SwapIO(course => course.Retargeted(goal))
        select unit;

    public IO<Unit> Stop() => pacing.Halt();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Motions {
    // --- [SOURCES]
    public static Func<Sink<Duration>, Action<IDisposable>, IO<Unit>> Timer(MotionSpan period, TimeProvider time) =>
        (ticks, retain) => Copies.Owned(
            () => new UITimer((_, _) => ticks.Post(time.GetElapsedTime(0L).ToDuration()).Run()) { Interval = ((Duration)period).TotalSeconds },
            static timer => IO.lift(timer.Start),
            static timer => Callbacks.Each(Seq(IO.lift(timer.Stop), IO.lift(timer.Dispose))).Map(unit), retain);

    // --- [POSES]
    public static Fin<CameraPose> Interpolated(CameraPose from, CameraPose to, double progress) =>
        (Refused.Unless(
                Quaternion.Slerp(Quaternion.Rotation(Plane.WorldXY, from.Frame), Quaternion.Rotation(Plane.WorldXY, to.Frame), progress).GetRotation(out Plane turned),
                turned,
                nameof(Quaternion.GetRotation)),
            Local(from),
            Local(to),
            Lens(from.Lens, to.Lens, progress))
        .Apply((rotated, near, far, lens) => new Plane(new Line(from.Target, to.Target).PointAt(progress), rotated.XAxis, rotated.YAxis) switch {
            var aimed => new CameraPose(new Plane(new Line(near, far).PointAt(progress) switch { var at => aimed.PointAt(at.X, at.Y, at.Z) }, rotated.XAxis, rotated.YAxis), aimed.Origin, lens),
        })
        .As();

    private static Fin<Point3d> Local(CameraPose pose) =>
        Refused.Unless(new Plane(pose.Target, pose.Frame.XAxis, pose.Frame.YAxis).RemapToPlaneSpace(pose.Frame.Origin, out Point3d local), local, nameof(Plane.RemapToPlaneSpace));

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
    public static IO<MotionHandle> Drive(RhinoDoc document, CameraMove move, Func<Sink<Duration>, Action<IDisposable>, IO<Unit>> frames, IPlugInSink sink,
        Action<MotionHandle, IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from rows in Viewports.Resolve(document, move.Viewports, owned)
        from curve in IO.lift(() => HostTheme.Conditions.Contains(Condition.ReduceMotion) ? new TimingCurve.Instant() : move.Curve)
        from legs in rows.TraverseM(row => Cameras.Read(row.Viewport).Map(pose => new Leg(row, pose, pose, row.Viewport.ChangeCounter, curve))).As()
        from ticks in Copies.Owned(static () => Conduit.make(Buffer<Duration>.New), static open => Fin.Succ(open), static open => open.Complete(), owned)
        from pacing in Copies.Owned(() => new Pacing(
            document, move.Redraw, frames, ticks, Atom(new Course(legs, move.To, Duration.Zero, Duration.Zero, None, new Phase.Paused()))),
            static state => Fin.Succ(state), static state => state.Halt(), owned)
        from consumer in DisposalOps.Fork(
            DisposalOps.Within(() => pacing, static state => state.Ticks.Reduce(unit, (_, tick) => Frame(state, tick)), static state => state.Halt().OnUiThread())
                .IfFail(error => IO.lift(() => sink.Report(error, typeof(Motions), nameof(Drive)))
                    .MapFail(fault => error + fault).Bind(_ => IO.fail<Unit>(error))), owned)
        let handle = new MotionHandle(pacing, consumer)
        from resumed in handle.Resume()
        select handle, retain);

    private static IO<Reduced<Unit>> Frame(Pacing pacing, Duration tick) =>
        (from course in pacing.Course.SwapIO(held => held.Phase.Map(running: held.Ticked(tick), paused: held, stopped: held))
         from reduced in course.Phase.Switch(
             (Pacing: pacing, Course: course),
             running: static (state, _) => Show(state.Pacing, state.Course, state.Course.Clock - state.Course.Start),
             paused: static (_, _) => Reduced.ContinueIO(unit),
             stopped: static (_, _) => Reduced.DoneIO(unit))
         select reduced)
        .OnUiThread();

    private static IO<Reduced<Unit>> Show(Pacing pacing, Course course, Duration at) =>
        from shown in Commits.WithinRedraw(pacing.Document, pacing.Redraw, IO.lift(() => course.Legs
            .Filter(static leg => leg.Row.Viewport.ChangeCounter == leg.Written)
            .Traverse(leg => leg.Curve.Sample(at) switch {
                var sample =>
                    from pose in Interpolated(leg.From, course.To, sample.Value)
                    from written in Cameras.Write(leg.Row.Viewport, pose)
                    from committed in leg.Row.Commit()
                    select (Leg: leg with { Shown = pose, Written = leg.Row.Viewport.ChangeCounter }, Sample: sample),
            }).As()))
        from swapped in pacing.Course.SwapIO(held => held with { Legs = shown.Map(static moved => moved.Leg) })
        select shown.ForAll(static moved => !moved.Sample.Continue) ? Reduced.Done(unit) : Reduced.Continue(unit);
}
