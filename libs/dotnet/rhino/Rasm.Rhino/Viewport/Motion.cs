using Eto.Forms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rhino;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Duration>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MotionSpan : System.Numerics.IMinMaxValue<MotionSpan> {
    public static MotionSpan MinValue { get; } = new(Duration.Epsilon);
    public static MotionSpan MaxValue { get; } = new(Duration.FromMilliseconds(int.MaxValue));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Duration value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DampingRatio : System.Numerics.IMinMaxValue<DampingRatio> {
    public static DampingRatio MinValue { get; } = new(1d);
    public static DampingRatio MaxValue { get; } = new(double.Sqrt(double.MaxValue));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct VisibleChange : System.Numerics.IMinMaxValue<VisibleChange> {
    public static VisibleChange MinValue { get; } = new(double.BitIncrement(0d));
    public static VisibleChange MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum]
public sealed partial class Easing {
    public static readonly Easing Linear = new(static time => time);
    public static readonly Easing In = new(static time => double.Pow(time, 3d));
    public static readonly Easing Out = new(static time => 1d - double.Pow(1d - time, 3d));
    public static readonly Easing InOut = new(static time => time < 0.5d ? 4d * double.Pow(time, 3d) : 1d - (double.Pow((-2d * time) + 2d, 3d) / 2d));

    [UseDelegateFromConstructor]
    public partial double Progress(double time);
}

[SmartEnum]
public sealed partial class FrameTick {
    public static readonly FrameTick Next = new();
    public static readonly FrameTick Final = new();
}

[Union]
public abstract partial record TimingCurve {
    public static Reduced<double> End { get; } = Reduced.Done(1d);

    public abstract Reduced<double> Sample(Duration elapsed);

    private static Reduced<double> Decayed(double remaining, VisibleChange rest) =>
        remaining < rest ? End : Reduced.Continue(1d - remaining);

    public sealed record Eased(MotionSpan Length, Easing Easing) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) =>
            elapsed >= Length ? End : Reduced.Continue(Easing.Progress(elapsed / Length));
    }

    public sealed record Spring(MotionSpan Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) =>
            Decayed(Remaining(double.Tau * (elapsed / Response), Damping, double.Sqrt((Damping - 1d) * (Damping + 1d))), Rest);

        private static double Remaining(double phase, double damping, double root) =>
            root == 0d
                ? (1d + phase) * double.Exp(-phase)
                : (-2d * root * phase) switch {
                    var gap => 0.5d * double.Exp(-phase / (damping + root)) * (1d + double.Exp(gap) - (damping / root * double.ExpM1(gap))),
                };
    }

    public sealed record Glide(MotionSpan TimeConstant, VisibleChange Rest) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) =>
            Decayed(double.Exp(-(elapsed / TimeConstant)), Rest);
    }
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
        Phase.Switch(
            (Curve: curve, Tick: tick, Time: Motion(elapsed) - Leg.Start),
            running: static (state, _) => Some(state.Tick.Switch(state, next: static at => at.Curve.Sample(at.Time), final: static _ => TimingCurve.End)),
            paused: static (_, _) => Option<Reduced<double>>.None,
            stopped: static (_, _) => Option<Reduced<double>>.None);

    public Course Resumed(Phase.Paused paused, Duration elapsed, IDisposable source) =>
        this with { Excluded = Excluded + (elapsed - paused.Since), Phase = new Phase.Running(source) };

    public Course Retargeted(CameraPose goal, Duration elapsed) =>
        this with { Leg = new Leg(Shown, goal, Motion(elapsed)) };
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Pacing(
    RhinoDoc Document,
    CameraMove Move,
    Func<Sink<FrameTick>, IO<IDisposable>> Frames,
    TimeProvider Time,
    long Start,
    Conduit<FrameTick, FrameTick> Ticks,
    Atom<Course> Course) {
    public IO<Duration> Elapsed => IO.lift(() => Time.GetElapsedTime(Start).ToDuration());

    public IO<Unit> Halt() =>
        from held in Course.ValueIO
        from halted in held.Phase.Switch(
            this,
            running: static (pacing, running) => IO.lift(running.Source.Dispose).Bind(_ => pacing.Close()),
            paused: static (pacing, _) => pacing.Close(),
            stopped: static (_, _) => IO.pure(unit))
        select halted;

    private IO<Unit> Close() =>
        Course.SwapIO(static course => course with { Phase = new Phase.Stopped() }).Bind(_ => Ticks.Complete());
}

public sealed class MotionHandle {
    private readonly Pacing pacing;

    internal MotionHandle(Pacing pacing, ForkIO<Unit> consumer) => (this.pacing, Completed) = (pacing, consumer.Await);

    public IO<Unit> Completed { get; }

    public IO<Unit> Pause() =>
        from held in pacing.Course.ValueIO
        from paused in held.Phase.Switch(
            pacing,
            running: static (state, running) =>
                from released in IO.lift(running.Source.Dispose)
                from elapsed in state.Elapsed
                from swapped in state.Course.SwapIO(course => course with { Phase = new Phase.Paused(elapsed) })
                select unit,
            paused: static (_, _) => IO.pure(unit),
            stopped: static (_, _) => IO.pure(unit))
        select paused;

    public IO<Unit> Resume() =>
        from held in pacing.Course.ValueIO
        from resumed in held.Phase.Switch(
            pacing,
            running: static (_, _) => IO.pure(unit),
            paused: static (state, paused) =>
                from source in state.Frames(state.Ticks.Sink)
                from elapsed in state.Elapsed
                from entered in state.Course.SwapIO(course => course.Resumed(paused, elapsed, source))
                select unit,
            stopped: static (_, _) => IO.fail<Unit>(new MotionEnded()))
        select resumed;

    public IO<Unit> Retarget(CameraPose goal) =>
        from held in pacing.Course.ValueIO
        from moved in held.Phase.Switch(
            (Pacing: pacing, Goal: goal),
            running: static (state, _) => Moved(state.Pacing, state.Goal),
            paused: static (state, _) => Moved(state.Pacing, state.Goal),
            stopped: static (_, _) => IO.fail<Unit>(new MotionEnded()))
        select moved;

    public IO<Unit> Stop() => pacing.Halt();

    private static IO<Unit> Moved(Pacing pacing, CameraPose goal) =>
        from elapsed in pacing.Elapsed
        from swapped in pacing.Course.SwapIO(course => course.Retargeted(goal, elapsed))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Motions {
    // --- [SOURCES]
    public static Func<Sink<FrameTick>, IO<IDisposable>> Idle { get; } = Turns(FrameTick.Next);

    public static Func<Sink<FrameTick>, IO<IDisposable>> Final { get; } = Turns(FrameTick.Final);

    public static Func<Sink<FrameTick>, IO<IDisposable>> Timer(MotionSpan period) =>
        ticks =>
            from timer in IO.lift(UITimer () => new UITimer((_, _) => ticks.Post(FrameTick.Next).Run()) { Interval = ((Duration)period).TotalSeconds })
            from started in DisposalOps.OnFailure(IO.lift(timer.Start), IO.lift(timer.Dispose))
            select (IDisposable)new Disposal<UITimer>(timer, static held => {
                held.Stop();
                held.Dispose();
            });

    private static Func<Sink<FrameTick>, IO<IDisposable>> Turns(FrameTick tick) =>
        ticks => Subscriptions.Attach<EventHandler>(
            static handler => RhinoApp.Idle += handler,
            static handler => RhinoApp.Idle -= handler,
            (_, _) => ticks.Post(tick).Run());

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
                Left: held => Conversions.Validated<ParallelExtent, double, InvalidRhinoValue>(double.Lerp(held, goal.Extent, state.Progress)).Map<CameraLens>(static extent => extent),
                Right: _ => Fin.Succ(state.Goal)),
            perspective: static (state, goal) => state.Held.Match(
                Left: _ => Fin.Succ(state.Goal),
                Right: held => Conversions.Validated<CameraAngle, double, InvalidRhinoValue>(double.Lerp(held, goal.Angle, state.Progress)).Map<CameraLens>(static angle => angle)),
            twoPoint: static (state, goal) => state.Held.Match(
                Left: _ => Fin.Succ(state.Goal),
                Right: held => Conversions.Validated<CameraAngle, double, InvalidRhinoValue>(double.Lerp(held, goal.Angle, state.Progress))
                    .Map<CameraLens>(angle => new CameraLens.TwoPoint(angle, goal.Up))));

    // --- [DRIVE]
    public static IO<MotionHandle> Drive(RhinoDoc document, CameraMove move, Func<Sink<FrameTick>, IO<IDisposable>> frames, TimeProvider time, IPlugInSink sink) =>
        from ticks in IO.lift(static () => Conduit.make(Buffer<FrameTick>.New))
        from start in IO.lift(time.GetTimestamp)
        let pacing = new Pacing(
            document, move, frames, time, start, ticks,
            Atom(new Course(new Leg(move.From, move.To, Duration.Zero), move.From, Duration.Zero, new Phase.Paused(Duration.Zero))))
        from consumer in ticks.Reduce(unit, (_, tick) => Frame(pacing, tick))
            .Catch(error => IO.lift(() => sink.Report(error, typeof(Motions), nameof(Drive))).Bind(_ => IO.fail<Unit>(error)))
            .As()
            .Finally(pacing.Halt().Post())
            .Fork()
        let handle = new MotionHandle(pacing, consumer)
        from resumed in handle.Resume()
        select handle;

    private static IO<Reduced<Unit>> Frame(Pacing pacing, FrameTick tick) =>
        (from course in pacing.Course.ValueIO
         from elapsed in pacing.Elapsed
         from reduced in course.Sample(pacing.Move.Curve, tick, elapsed).Match(
             Some: sample => Show(pacing, course.Leg, sample),
             None: static () => Reduced.ContinueIO(unit))
         select reduced)
        .Post();

    private static IO<Reduced<Unit>> Show(Pacing pacing, Leg leg, Reduced<double> sample) =>
        from pose in IO.lift(Interpolated(leg.From, leg.To, sample.Value))
        from written in Navigation.ApplyToViewports(pacing.Document, pacing.Move.Viewports, port => Cameras.WritePose(port, pose), pacing.Move.Redraw)
        from shown in pacing.Course.SwapIO(course => course with { Shown = pose })
        from halted in unless(sample.Continue, pacing.Halt()).As()
        select sample.Map(static _ => unit);
}
