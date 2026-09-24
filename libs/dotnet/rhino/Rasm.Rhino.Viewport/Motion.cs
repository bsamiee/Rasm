using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameClock {
    public sealed record Idle() : FrameClock;

    public sealed record Periodic : FrameClock {
        private Periodic(double periodSeconds) => PeriodSeconds = periodSeconds;

        public double PeriodSeconds { get; }

        public static Fin<FrameClock> Create(double periodSeconds) =>
            DocumentUnits.PositiveFinite(periodSeconds, nameof(PeriodSeconds)).ToFin().Map<FrameClock>(static valid => new Periodic(valid));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionState {
    public sealed record Running(IDisposable Subscription) : MotionState;

    public sealed record Paused(Option<IDisposable> Released) : MotionState;

    public sealed record Stopped(Option<IDisposable> Released) : MotionState;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MotionHandle : IDisposable {
    private readonly Func<MotionHandle, IO<IDisposable>> subscribe;

    private readonly Atom<MotionState> state = Atom<MotionState>(new MotionState.Paused(None));

    private readonly Atom<Option<Error>> fault = Atom(Option<Error>.None);

    internal MotionHandle(Func<MotionHandle, IO<IDisposable>> subscribe) =>
        this.subscribe = subscribe;

    public Option<Error> Fault => fault.Value;

    public bool Running => state.Value is MotionState.Running;

    public IO<Unit> Pause() =>
        Leave(static released => new MotionState.Paused(released));

    public IO<Unit> Resume() =>
        from paused in state.ValueIO.Map(static current => current is MotionState.Paused)
        from entered in when(paused, subscribe(this).Bind(Enter)).As()
        select entered;

    public IO<Unit> Stop() =>
        Leave(static released => new MotionState.Stopped(released));

    public void Dispose() => _ = Stop().RunSafe();

    internal void Faulted(Error error) => _ = fault.Swap(_ => Some(error));

    private IO<Unit> Leave(Func<Option<IDisposable>, MotionState> next) =>
        state.SwapIO(current => current.Switch(
                next,
                running: static (enter, running) => enter(Some(running.Subscription)),
                paused: static (enter, _) => enter(None),
                stopped: static (_, _) => new MotionState.Stopped(None)))
            .Bind(static left => Disposal.Release(left.Switch(running: static _ => None, paused: static paused => paused.Released, stopped: static stopped => stopped.Released).ToSeq()));

    private IO<Unit> Enter(IDisposable subscription) =>
        state.SwapIO(current => current is MotionState.Paused ? new MotionState.Running(subscription) : current)
            .Bind(entered => unless(entered is MotionState.Running running && ReferenceEquals(running.Subscription, subscription), IO.lift(subscription.Dispose)).As());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameClocks {
    public static IO<MotionHandle> Drive(RhinoDoc document, ViewportTarget target, Func<Duration, Option<CameraPose>> sample, Func<Instant> now, RedrawPolicy redraw, FrameClock clock) =>
        from started in IO.lift(now)
        from handle in IO.lift(() => new MotionHandle(owner => Subscribe(clock, (_, _) => _ = Tick(document, target, sample, now, redraw, started, owner).RunSafe())))
        from running in handle.Resume()
        select handle;

    private static IO<IDisposable> Subscribe(FrameClock clock, EventHandler onTick) =>
        clock.Switch(
            onTick,
            idle: static (tick, _) => Events.OnIdle(tick),
            periodic: static (tick, periodic) => IO.lift(() => {
                UITimer widget = new(tick.Invoke) { Interval = periodic.PeriodSeconds };
                widget.Start();
                return widget;
            }).Map<IDisposable>(static widget => new Disposal(() => {
                widget.Stop();
                widget.Dispose();
            })));

    private static IO<Unit> Tick(RhinoDoc document, ViewportTarget target, Func<Duration, Option<CameraPose>> sample, Func<Instant> now, RedrawPolicy redraw, Instant started, MotionHandle handle) =>
        (from pose in IO.lift(() => sample(now() - started))
         from applied in pose.Match(
             Some: next =>
                 from rows in Viewports.ResolveViewports(document, target)
                 from written in Navigation.ApplyToRows(document, rows, port => Cameras.WritePose(port, next), redraw)
                 select unit,
             None: handle.Stop)
         select applied)
        .IfFail(error =>
            from noted in IO.lift(() => handle.Faulted(error))
            from stopped in handle.Stop()
            select stopped);
}
