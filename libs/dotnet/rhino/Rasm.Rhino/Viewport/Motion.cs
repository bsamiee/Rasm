using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using UnitsNet;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CameraMove(ViewportSet Viewports, CameraPose To, Duration Span, Func<double, double> Progress, RedrawPolicy Redraw) {
    internal Reduced<double> Sample(Duration elapsed) =>
        (Span > Duration.Zero ? double.Min(elapsed / Span, 1d) : 1d) switch {
            var at => at < 1d ? Reduced.Continue(Progress(at)) : Reduced.Done(Progress(1d)),
        };
}

internal sealed record Leg(ViewportRef Row, CameraPose From, CameraPose Shown, uint Written);

[Union]
internal abstract partial record Phase {
    internal sealed record Running(EnvIO Source) : Phase;

    internal sealed record Paused : Phase;

    internal sealed record Stopped : Phase;
}

internal sealed record Course(Seq<Leg> Legs, CameraPose To, Duration Start, Duration Clock, Option<Duration> Last, Phase Phase) {
    public Course Ticked(Duration tick) =>
        this with { Clock = Last.Match(Some: last => Clock + (tick - last), None: () => Clock), Last = tick };

    public Course Retargeted(CameraPose goal) =>
        this with { To = goal, Start = Clock, Legs = Legs.Map(static leg => leg with { From = leg.Shown }).Strict() };
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Pacing(
    RhinoDoc Document,
    CameraMove Move,
    Func<Sink<Duration>, IO<Unit>> Frames,
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
            paused: static (state, _) =>
                from scope in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
                from framed in state.Frames(state.Ticks.Sink).Under(scope).Catch(error => IO.lift(scope.Dispose) >> IO.fail<Unit>(error)).As()
                from swapped in state.Course.SwapIO(course => course with { Phase = new Phase.Running(scope), Last = None })
                select unit,
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
    public static Func<Sink<Duration>, IO<Unit>> Timer(Duration period, TimeProvider time) =>
        ticks => use(() => new UITimer((_, _) => ticks.Post(time.GetElapsedTime(0L).ToDuration()).Run()) { Interval = period.TotalSeconds }, static timer => {
            timer.Stop();
            timer.Dispose();
        })
            .Bind(static timer => IO.lift(timer.Start));

    // --- [POSES]
    public static Fin<CameraPose> Interpolated(CameraPose from, CameraPose to, double progress) =>
        (RhinoFault.Unless(Quaternion.Slerp(Quaternion.Rotation(Plane.WorldXY, from.Frame), Quaternion.Rotation(Plane.WorldXY, to.Frame), progress).GetRotation(out Plane turned))
                .Map(_ => turned),
            Local(from),
            Local(to))
        .Apply((rotated, near, far) => new Plane(new Line(from.Target, to.Target).PointAt(progress), rotated.XAxis, rotated.YAxis) switch {
            var aimed => new CameraPose(
                new Plane(new Line(near, far).PointAt(progress) switch { var at => aimed.PointAt(at.X, at.Y, at.Z) }, rotated.XAxis, rotated.YAxis),
                aimed.Origin,
                Lens(from.Lens, to.Lens, progress)),
        })
        .As();

    private static Fin<Point3d> Local(CameraPose pose) =>
        RhinoFault.Unless(new Plane(pose.Target, pose.Frame.XAxis, pose.Frame.YAxis).RemapToPlaneSpace(pose.Frame.Origin, out Point3d local)).Map(_ => local);

    private static CameraLens Lens(CameraLens from, CameraLens to, double progress) =>
        from.Switch(
            (To: to, Progress: progress),
            parallel: static (at, held) => at.To.Switch(
                (Held: held, at.Progress),
                parallel: static (state, goal) => new CameraLens.Parallel(double.Lerp(state.Held.Extent, goal.Extent, state.Progress)),
                perspective: static (_, goal) => goal,
                twoPoint: static (_, goal) => (CameraLens)goal),
            perspective: static (at, held) => Turned(at.To, held.Angle, at.Progress),
            twoPoint: static (at, held) => Turned(at.To, held.Angle, at.Progress));

    private static CameraLens Turned(CameraLens to, Angle held, double progress) =>
        to.Switch(
            (Held: held, Progress: progress),
            parallel: static (_, goal) => goal,
            perspective: static (at, goal) => new CameraLens.Perspective(Angle.FromRadians(double.Lerp(at.Held.Radians.ToDouble(), goal.Angle.Radians.ToDouble(), at.Progress))),
            twoPoint: static (at, goal) => (CameraLens)(goal with { Angle = Angle.FromRadians(double.Lerp(at.Held.Radians.ToDouble(), goal.Angle.Radians.ToDouble(), at.Progress)) }));

    // --- [DRIVE]
    public static IO<MotionHandle> Drive(RhinoDoc document, CameraMove move, Func<Sink<Duration>, IO<Unit>> frames, object owner) =>
        from rows in Viewports.Resolve(document, move.Viewports)
        from legs in rows.TraverseM(static row => Cameras.Read(row.Viewport).Map(pose => new Leg(row, pose, pose, row.Viewport.ChangeCounter))).As()
        from ticks in use(IO.lift(static () => Conduit.make(Buffer<Duration>.New)), static open => open.Complete())
        from pacing in use(IO.lift(() => new Pacing(
            document, move, frames, ticks, Atom(new Course(legs, move.To, Duration.Zero, Duration.Zero, None, new Phase.Paused())))), static state => state.Halt())
        from consumer in use(
            pacing.Ticks.Reduce(unit, (_, tick) => Frame(pacing, tick)).Finally(pacing.Halt().OnUiThread())
                .Catch(error => IO.lift(() => owner.Run(IO.fail<Unit>(error))) >> IO.fail<Unit>(error)).As()
                .Fork(),
            static fork => fork.Cancel)
        let handle = new MotionHandle(pacing, consumer)
        from resumed in handle.Resume()
        select handle;

    private static IO<Reduced<Unit>> Frame(Pacing pacing, Duration tick) =>
        (from course in pacing.Course.SwapIO(held => held.Phase.Map(running: held.Ticked(tick), paused: held, stopped: held))
         from reduced in course.Phase.Switch(
             (Pacing: pacing, Course: course),
             running: static (state, _) => Show(state.Pacing, state.Course, state.Pacing.Move.Sample(state.Course.Clock - state.Course.Start)),
             paused: static (_, _) => Reduced.ContinueIO(unit),
             stopped: static (_, _) => Reduced.DoneIO(unit))
         select reduced)
        .OnUiThread();

    private static IO<Reduced<Unit>> Show(Pacing pacing, Course course, Reduced<double> sample) =>
        from shown in Commits.WithinRedraw(pacing.Document, pacing.Move.Redraw, IO.lift(() => course.Legs
            .Filter(static leg => leg.Row.Viewport.ChangeCounter == leg.Written)
            .Traverse(leg =>
                from pose in Interpolated(leg.From, course.To, sample.Value)
                from written in Cameras.Write(leg.Row.Viewport, pose)
                from committed in leg.Row.Commit()
                select leg with { Shown = pose, Written = leg.Row.Viewport.ChangeCounter }).As()))
        from swapped in pacing.Course.SwapIO(held => held with { Legs = shown })
        select sample.Continue ? Reduced.Continue(unit) : Reduced.Done(unit);
}
