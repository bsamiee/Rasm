using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;
using UnitsNet;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CameraMove(ViewportSet Viewports, CameraPose To, Duration Span, Func<double, double> Progress, RedrawPolicy Redraw) {
    internal Reduced<double> Sample(Duration elapsed) =>
        (Span > Duration.Zero ? double.Min(elapsed / Span, 1d) : 1d) switch {
            var at => at < 1d ? Reduced.Continue(Progress(at)) : Reduced.Done(Progress(1d)),
        };
}

internal sealed record ViewportMotion(ViewportRef Row, CameraPose From, CameraPose Shown, uint Written);

[Union]
internal abstract partial record Phase {
    internal sealed record Running(EnvIO Source) : Phase;

    internal sealed record Paused : Phase;

    internal sealed record Stopped : Phase;
}

internal sealed record MotionState(Seq<ViewportMotion> Viewports, CameraPose To, Duration Start, Duration Clock, Option<Duration> Last, Phase Phase) {
    public MotionState Ticked(Duration tick) =>
        this with { Clock = Last.Match(Some: last => Clock + (tick - last), None: () => Clock), Last = tick };

    public MotionState Retargeted(CameraPose goal) =>
        this with { To = goal, Start = Clock, Viewports = Viewports.Map(static motion => motion with { From = motion.Shown }).Strict() };
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Motion(RhinoDoc Document, CameraMove Move, Func<Sink<Duration>, IO<Unit>> Frames, Conduit<Duration, Duration> Ticks, Ref<MotionState> State) {
    public IO<(MotionState Prior, MotionState Next)> Transition(Func<MotionState, MotionState> next) =>
        atomicIO(() => {
            MotionState prior = State.Value;
            State.Value = next(prior);
            return (prior, State.Value);
        });

    public IO<Unit> Halt() =>
        Transition(static state => state with { Phase = new Phase.Stopped() }).Bind(moved => moved.Prior.Phase.Switch(
            (Motion: this, moved.Next),
            running: static (state, running) => Products.Each(Seq(IO.lift(running.Source.Dispose), state.Motion.Settled(state.Next))).Map(unit),
            paused: static (state, _) => state.Motion.Settled(state.Next),
            stopped: static (_, _) => IO.pure(unit))).Uninterruptible();

    public IO<T> Uncommitted<T>(IO<T> body) =>
        Products.Changed(() => Document.UndoRecordingEnabled, value => Document.UndoRecordingEnabled = value, value: false).Bind(_ => body).Bracket();

    private IO<Unit> Settled(MotionState state) =>
        state.Viewports.Filter(static motion => motion.Row.Pending.IsSome) switch {
            var details => Products.Each(Seq(
                Ticks.Complete(),
                when(!details.IsEmpty,
                    Uncommitted(Products.Each(details.Map(static motion => Written(motion.Row, motion.From)))).Map(static _ => unit)
                    >> Commits.Commit(Document, nameof(CameraMove), Move.Redraw, Products.Each(details.Map(static motion => Written(motion.Row, motion.Shown)))).Map(unit)).As()))
                .Map(unit),
        };

    internal static IO<Unit> Written(ViewportRef row, CameraPose pose) =>
        IO.lift(() => Cameras.Write(row.Viewport, pose)) >> row.Commit;
}

public sealed class MotionHandle {
    private readonly Motion motion;

    internal MotionHandle(Motion motion, ForkIO<Unit> consumer) => (this.motion, Completed) = (motion, consumer.Await);

    public IO<Unit> Completed { get; }

    public IO<Unit> Pause() =>
        motion.Transition(static state => state.Phase is Phase.Running ? state with { Phase = new Phase.Paused(), Last = None } : state)
            .Bind(static moved => IO.lift(() => (moved.Prior.Phase as Phase.Running)?.Source.Dispose())).Uninterruptible();

    public IO<Unit> Resume() =>
        from scope in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
        from moved in motion.Transition(state => state.Phase is Phase.Paused ? state with { Phase = new Phase.Running(scope), Last = None } : state)
        from started in moved.Prior.Phase.Switch(
            (Motion: motion, Scope: scope),
            running: static (state, _) => IO.lift(state.Scope.Dispose),
            paused: static (state, _) => state.Motion.Frames(state.Motion.Ticks.Sink).Scoped(state.Scope)
                .Catch(error => state.Motion.Halt() >> IO.fail<Unit>(error)).As(),
            stopped: static (state, _) => IO.lift(state.Scope.Dispose) >> IO.fail<Unit>(Errors.Cancelled))
        select unit;

    public IO<Unit> Retarget(CameraPose goal) =>
        motion.Transition(state => state.Phase is Phase.Stopped ? state : state.Retargeted(goal))
            .Bind(static moved => when(moved.Prior.Phase is Phase.Stopped, IO.fail<Unit>(Errors.Cancelled)).As());

    public IO<Unit> Stop() => motion.Halt();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Motions {
    // --- [SOURCES]
    public static Func<Sink<Duration>, IO<Unit>> Timer(Duration period, TimeProvider time) =>
        ticks => use(() => new UITimer((_, _) => Session.Run(ticks.Post(time.GetElapsedTime(0L).ToDuration()))) { Interval = period.TotalSeconds }, static timer => {
            timer.Stop();
            timer.Dispose();
        })
            .Bind(static timer => IO.lift(timer.Start));

    // --- [POSES]
    public static Fin<CameraPose> Interpolated(CameraPose from, CameraPose to, double progress) =>
        (RhinoFault.Unless(Quaternion.Slerp(Quaternion.Rotation(Plane.WorldXY, from.Frame), Quaternion.Rotation(Plane.WorldXY, to.Frame), progress).GetRotation(out Plane turned),
                    RhinoReason.Invalid, progress)
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

    public static Fin<Seq<CameraPose>> Frames(CameraPose from, CameraPose to, int count, Func<double, double> progress) =>
        toSeq(Enumerable.Range(0, int.Max(count, 1))).Traverse(frame => Interpolated(from, to, progress(count > 1 ? frame / (double)(count - 1) : 1d))).As();

    private static Fin<Point3d> Local(CameraPose pose) =>
        RhinoFault.Unless(new Plane(pose.Target, pose.Frame.XAxis, pose.Frame.YAxis).RemapToPlaneSpace(pose.Frame.Origin, out Point3d local), RhinoReason.Invalid, pose.Frame.Origin)
            .Map(_ => local);

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
            perspective: static (at, goal) => new CameraLens.Perspective(Angle.FromRadians(Quantities.Exact(double.Lerp(at.Held.Radians.ToDouble(), goal.Angle.Radians.ToDouble(), at.Progress)))),
            twoPoint: static (at, goal) => (CameraLens)(goal with { Angle = Angle.FromRadians(Quantities.Exact(double.Lerp(at.Held.Radians.ToDouble(), goal.Angle.Radians.ToDouble(), at.Progress))) }));

    // --- [DRIVE]
    public static IO<MotionHandle> Drive(RhinoDoc document, CameraMove move, Func<Sink<Duration>, IO<Unit>> frames) =>
        from rows in Viewports.Resolve(document, move.Viewports)
        from motions in rows.TraverseM(static row => Cameras.Read(row.Viewport).Map(pose => new ViewportMotion(row, pose, pose, row.Viewport.ChangeCounter))).As()
        from pushed in IO.lift(() => rows.Filter(static row => row.Pending.IsNone).Iter(static row => row.Viewport.PushViewProjection()))
        from ticks in use(IO.lift(static () => Conduit.make(Buffer<Duration>.New)), static open => open.Complete())
        from motion in use(IO.lift(() => new Motion(
            document, move, frames, ticks, Ref(new MotionState(motions, move.To, Duration.Zero, Duration.Zero, None, new Phase.Paused())))), static state => state.Halt())
        from consumer in use(
            motion.Ticks.Reduce(unit, (_, tick) => Frame(motion, tick)).Finally(motion.Halt().OnUiThread()).Fork(),
            static fork => fork.Cancel)
        let handle = new MotionHandle(motion, consumer)
        from resumed in handle.Resume()
        select handle;

    private static IO<Reduced<Unit>> Frame(Motion motion, Duration tick) =>
        (from moved in motion.Transition(held => held.Phase is Phase.Running ? held.Ticked(tick) : held)
         from reduced in moved.Next.Phase.Switch(
             (Motion: motion, State: moved.Next),
             running: static (state, _) => Show(state.Motion, state.State, state.Motion.Move.Sample(state.State.Clock - state.State.Start)),
             paused: static (_, _) => Reduced.ContinueIO(unit),
             stopped: static (_, _) => Reduced.DoneIO(unit))
         select reduced)
        .OnUiThread();

    private static IO<Reduced<Unit>> Show(Motion motion, MotionState state, Reduced<double> sample) =>
        from shown in Commits.WithinRedraw(motion.Document, motion.Move.Redraw, motion.Uncommitted(
            state.Viewports.Filter(static leg => leg.Row.Viewport.ChangeCounter == leg.Written).TraverseM(leg =>
                from pose in IO.lift(() => Interpolated(leg.From, state.To, sample.Value))
                from written in Motion.Written(leg.Row, pose)
                select leg with { Shown = pose, Written = leg.Row.Viewport.ChangeCounter }).As()))
        from updated in motion.Transition(held => held with { Viewports = shown })
        select sample.Continue ? Reduced.Continue(unit) : Reduced.Done(unit);
}
