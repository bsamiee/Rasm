using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameClock {
    public sealed record Idle() : FrameClock {
        internal override IO<IDisposable> Subscribe(EventHandler onTick) => Events.OnIdle(onTick);
    }

    public sealed record Periodic : FrameClock {
        private Periodic(double periodSeconds) => PeriodSeconds = periodSeconds;

        public double PeriodSeconds { get; }

        public static Fin<FrameClock> Create(double periodSeconds) =>
            Limits.Above(0.0).Check(periodSeconds, nameof(PeriodSeconds)).Map<FrameClock>(static valid => new Periodic(valid));

        internal override IO<IDisposable> Subscribe(EventHandler onTick) =>
            IO.lift(IDisposable () => {
                UITimer widget = new(onTick.Invoke) { Interval = PeriodSeconds };
                widget.Start();
                return new Disposal(() => {
                    widget.Stop();
                    widget.Dispose();
                });
            });
    }

    internal abstract IO<IDisposable> Subscribe(EventHandler onTick);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionState {
    public sealed record Running(IDisposable Subscription) : MotionState;

    public sealed record Paused(Option<IDisposable> Released) : MotionState;

    public sealed record Stopped(Option<IDisposable> Released) : MotionState;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MotionHandle : IDisposable {
    private readonly FrameClock clock;

    private readonly IO<Option<CameraPose>> sample;

    private readonly Func<CameraPose, IO<Unit>> write;

    private readonly Atom<MotionState> state = Atom<MotionState>(new MotionState.Paused(None));

    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);

    internal MotionHandle(FrameClock clock, IO<Option<CameraPose>> sample, Func<CameraPose, IO<Unit>> write) =>
        (this.clock, this.sample, this.write) = (clock, sample, write);

    public bool Running => state.Value is MotionState.Running;

    public Option<Error> Fault => fault.Value;

    public IO<Unit> Resume() =>
        from paused in state.ValueIO.Map(static current => current is MotionState.Paused)
        from entered in when(paused, clock.Subscribe((_, _) => _ = Tick().RunSafe()).Bind(Enter)).As()
        select entered;

    public IO<Unit> Pause() =>
        Leave(static released => new MotionState.Paused(released));

    public IO<Unit> Stop() =>
        Leave(static released => new MotionState.Stopped(released));

    public void Dispose() => _ = Stop().RunSafe();

    private IO<Unit> Enter(IDisposable subscription) =>
        state.SwapIO(current => current is MotionState.Paused ? new MotionState.Running(subscription) : current)
            .Bind(entered => unless(entered is MotionState.Running running && ReferenceEquals(running.Subscription, subscription), IO.lift(subscription.Dispose)).As());

    private IO<Unit> Tick() =>
        (from pose in sample
         from written in pose.Match(Some: write, None: Stop)
         select written)
        .IfFail(error =>
            from noted in fault.SwapIO(_ => Some(error))
            from stopped in Stop()
            select stopped);

    private IO<Unit> Leave(Func<Option<IDisposable>, MotionState> next) =>
        state.SwapIO(current => current.Switch(
                next,
                running: static (enter, running) => enter(Some(running.Subscription)),
                paused: static (enter, _) => enter(None),
                stopped: static (_, _) => new MotionState.Stopped(None)))
            .Bind(static left => DisposalOps.Release(left.Switch(running: static _ => None, paused: static paused => paused.Released, stopped: static stopped => stopped.Released).ToSeq()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameClocks {
    public static IO<MotionHandle> Drive(RhinoDoc document, ViewportTarget target, Func<Duration, Option<CameraPose>> sample, Func<Instant> now, RedrawPolicy redraw, FrameClock clock) =>
        from started in IO.lift(now)
        let handle = new MotionHandle(
            clock,
            IO.lift(() => sample(now() - started)),
            pose => DisposalOps.Using(Viewports.ResolveViewports(document, target), rows => Navigation.ApplyToRows(document, rows, port => Cameras.WritePose(port, pose), redraw))
                .Map(static _ => unit))
        from running in handle.Resume()
        select handle;
}
