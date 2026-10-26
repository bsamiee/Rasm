using System.Drawing;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRealtimeProduct {
    public const bool DrawsOpenGl = false;

    public static abstract string Product { get; }
}

public enum HudGesture { LeftClick, RightClick, DoubleClick }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RealtimeTarget(Size Size, bool ForCapture, RenderWindow Window);

public sealed record RealtimeStart(RealtimeTarget Target, RhinoDoc Document, ViewInfo View, ViewportInfo Viewport, Func<Action<Disposal<SceneBatch>>, IO<SceneBatch>> Take);

public sealed record RealtimeSession(
    RealtimeTarget Target, IDisposable Scene, Instant Started, int Pass, Option<int> Limit,
    Option<uint> Frame, Option<string> Status, Option<Instant> PausedAt, bool Locked) {
    public static RealtimeSession Of(RealtimeTarget target, IDisposable scene, Instant now, Option<int> limit) =>
        new(target, scene, now, 0, limit, None, None, None, Locked: false);

    public bool Completed => !Locked && Frame.IsSome && Limit.Match(Some: limit => Pass >= limit, None: () => Target.ForCapture);

    public Option<double> Progress => Limit.Map(limit => Pass >= limit ? 1d : (double)Pass / limit);

    public RealtimeSession Restarted(Instant now) => this with { Started = now, Pass = 0, Frame = None };

    public RealtimeSession Resumed(Instant now) =>
        PausedAt.Match(Some: at => this with { Started = Started + (now - at), PausedAt = None }, None: () => this);
}

[SmartEnum]
public sealed partial class HudControl {
    public static readonly HudControl Play = new(
        Some<Func<RealtimeSession, Instant, RealtimeSession>>(static (session, now) => session.Resumed(now)),
        static (mode, handler) => mode.HudPlayButtonLeftClicked += handler,
        static (mode, handler) => mode.HudPlayButtonRightClicked += handler,
        static (mode, handler) => mode.HudPlayButtonDoubleClicked += handler);
    public static readonly HudControl Pause = new(
        Some<Func<RealtimeSession, Instant, RealtimeSession>>(static (session, now) => session with { PausedAt = session.PausedAt | Some(now) }),
        static (mode, handler) => mode.HudPauseButtonLeftClicked += handler,
        static (mode, handler) => mode.HudPauseButtonRightClicked += handler,
        static (mode, handler) => mode.HudPauseButtonDoubleClicked += handler);
    public static readonly HudControl Lock = new(
        Some<Func<RealtimeSession, Instant, RealtimeSession>>(static (session, _) => session with { Locked = true }),
        static (mode, handler) => mode.HudLockButtonLeftClicked += handler,
        static (mode, handler) => mode.HudLockButtonRightClicked += handler,
        static (mode, handler) => mode.HudLockButtonDoubleClicked += handler);
    public static readonly HudControl Unlock = new(
        Some<Func<RealtimeSession, Instant, RealtimeSession>>(static (session, _) => session with { Locked = false }),
        static (mode, handler) => mode.HudUnlockButtonLeftClicked += handler,
        static (mode, handler) => mode.HudUnlockButtonRightClicked += handler,
        static (mode, handler) => mode.HudUnlockButtonDoubleClicked += handler);
    public static readonly HudControl ProductName = new(
        None,
        static (mode, handler) => mode.HudProductNameLeftClicked += handler,
        static (mode, handler) => mode.HudProductNameRightClicked += handler,
        static (mode, handler) => mode.HudProductNameDoubleClicked += handler);
    public static readonly HudControl StatusText = new(
        None,
        static (mode, handler) => mode.HudStatusTextLeftClicked += handler,
        static (mode, handler) => mode.HudStatusTextRightClicked += handler,
        static (mode, handler) => mode.HudStatusTextDoubleClicked += handler);
    public static readonly HudControl Time = new(
        None,
        static (mode, handler) => mode.HudTimeLeftClicked += handler,
        static (mode, handler) => mode.HudTimeRightClicked += handler,
        static (mode, handler) => mode.HudTimeDoubleClicked += handler);
    public static readonly HudControl PostEffectsOn = new(
        None,
        static (mode, handler) => mode.HudPostEffectsOnButtonLeftClicked += handler,
        static (mode, handler) => mode.HudPostEffectsOnButtonRightClicked += handler,
        static (mode, handler) => mode.HudPostEffectsOnButtonDoubleClicked += handler);
    public static readonly HudControl PostEffectsOff = new(
        None,
        static (mode, handler) => mode.HudPostEffectsOffButtonLeftClicked += handler,
        static (mode, handler) => mode.HudPostEffectsOffButtonRightClicked += handler,
        static (mode, handler) => mode.HudPostEffectsOffButtonDoubleClicked += handler);

    public Option<Func<RealtimeSession, Instant, RealtimeSession>> Act { get; }

    [UseDelegateFromConstructor]
    public partial void LeftClicked(RealtimeDisplayMode mode, EventHandler handler);

    [UseDelegateFromConstructor]
    public partial void RightClicked(RealtimeDisplayMode mode, EventHandler handler);

    [UseDelegateFromConstructor]
    public partial void DoubleClicked(RealtimeDisplayMode mode, EventHandler handler);
}

[Union]
public abstract partial record HudEvent {
    public sealed record Clicked(HudControl Control, HudGesture Gesture) : HudEvent;
    public sealed record MaxPassesEdited(int MaxPasses) : HudEvent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedRealtimeEngine<TEngine> : RealtimeDisplayMode
    where TEngine : DefinedRealtimeEngine<TEngine>, IRealtimeProduct, new() {
    // --- [STATE]
    private readonly Ref<Option<RealtimeSession>> sessions = Ref(Option<RealtimeSession>.None);
    private readonly Atom<Option<DisplayPipelineAttributes>> attributes = Atom(Option<DisplayPipelineAttributes>.None);
    private readonly IPlugInViews views;

    protected DefinedRealtimeEngine() => views = HostedTypes.Owner(typeof(TEngine));

    // --- [ROWS]
    protected abstract IO<Unit> Start(RealtimeStart start);

    protected abstract IO<Unit> Stop();

    protected virtual Option<IO<Unit>> Wake => None;

    protected virtual Option<Func<RealtimeTarget, IO<Unit>>> Resize => None;

    protected virtual Option<Func<HudEvent, IO<Unit>>> Hud => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Framebuffer => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Middleground => None;

    // --- [LIFECYCLE]
    public sealed override void PostConstruct() =>
        Callbacks.Run(IO.lift(Subscribe), CallbackSite.Of(views, this));

    public sealed override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) =>
        _ = attributes.Swap(_ => Some(displayPipelineAttributes));

    public sealed override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) =>
        Callbacks.Succeeded(
            Started(w, h, doc, view, viewportInfo, forCapture, renderWindow),
            CallbackSite.Of(views, this));

    public sealed override bool OnRenderSizeChanged(int width, int height) =>
        Callbacks.Succeeded(
            from session in Session
            from resize in Resize
            select Resized(session.Target with { Size = new Size(width, height) }, resize),
            () => base.OnRenderSizeChanged(width, height),
            CallbackSite.Of(views, this));

    public sealed override void ShutdownRenderer() =>
        Callbacks.Run(DisposalOps.Within(static () => unit, _ => Stop(), _ => Closed), CallbackSite.Of(views, this));

    public sealed override void GetRenderSize(out int width, out int height) =>
        (width, height) = Session.Map(static session => (session.Target.Size.Width, session.Target.Size.Height)).IfNone((0, 0));

    private void Subscribe() {
        SetUseDrawOpenGl(IRealtimeProduct.DrawsOpenGl);
        _ = toSeq(HudControl.Items).Iter(Attach);
        MaxPassesChanged += Callbacks.Handler<HudMaxPassesChangedEventArgs>(args => Edited(args.MaxPasses), CallbackSite.Of(views, this, nameof(MaxPassesChanged)));
        _ = Framebuffer.Iter(draw => OnInitFramebuffer += Callbacks.Handler<InitFramebufferEventArgs>(args => draw(args.Pipeline), CallbackSite.Of(views, this, nameof(OnInitFramebuffer))));
        _ = Middleground.Iter(draw => OnDrawMiddleground += Callbacks.Handler<DrawMiddlegroundEventArgs>(args => draw(args.Pipeline), CallbackSite.Of(views, this, nameof(OnDrawMiddleground))));
    }

    private IO<Unit> Started(int width, int height, RhinoDoc doc, ViewInfo view, ViewportInfo viewport, bool forCapture, RenderWindow window) =>
        DisposalOps.OnFailure(
            from opened in DisposalOps.Acquire(owned =>
                from framing in DisposalOps.Acquire(IO.pure(viewport), owned)
                let target = new RealtimeTarget(new Size(width, height), forCapture, window)
                from sized in IO.lift(() => target.Window.SetSize(target.Size))
                from added in RenderWindows.AddRequested(target.Window)
                from now in IO.lift(views.Clock.GetCurrentInstant)
                from passes in IO.lift(() => MaxPasses)
                let limit = Callbacks.Found(target.ForCapture && passes > 0, passes)
                from queue in DefinedChangeFeed.Of(views, doc, view, attributes.Value, target.ForCapture ? QueuePolicy.Capture : QueuePolicy.Viewport, Wake, owned)
                select (Queue: queue, Now: now, Target: target, Limit: limit), (created, lease) => {
                    Option<RealtimeSession> prior = atomic(() => {
                        Option<RealtimeSession> held = sessions.Value;
                        sessions.Value = Some(RealtimeSession.Of(created.Target, lease, created.Now, created.Limit));
                        return held;
                    });
                    return DisposalOps.Release(prior.Map(static session => session.Scene).ToSeq());
                })
            from world in opened.Queue.World
            from started in Start(new RealtimeStart(opened.Target, doc, view, viewport, opened.Queue.Take))
            select unit,
            Callbacks.Each(Seq(IO.pure(unit).Bind(_ => Stop()), Closed)).Map(unit));

    private IO<Unit> Closed =>
        atomicIO(() => {
            Option<RealtimeSession> prior = sessions.Value;
            sessions.Value = None;
            return prior;
        }).Bind(static held => DisposalOps.Release(held.Map(static session => session.Scene).ToSeq())).Uninterruptible();

    private IO<Unit> Resized(RealtimeTarget target, Func<RealtimeTarget, IO<Unit>> resize) =>
        from sized in IO.lift(() => target.Window.SetSize(target.Size))
        from restarted in Swapped((session, now) => (session with { Target = target }).Restarted(now))
        from resized in resize(target)
        select unit;

    // --- [FRAMES]
    public sealed override bool IsRendererStarted() => Session.IsSome;

    public sealed override bool IsFrameBufferAvailable(ViewInfo view) =>
        Session.Exists(session => session.Locked || session.Frame.Exists(frame => session.Target.ForCapture || frame == ComputeViewportCrc(view)));

    public sealed override bool IsCompleted() => Session.Exists(static session => session.Completed);

    public sealed override int LastRenderedPass() =>
        Session.Map(static session => session.Pass).IfNone(base.LastRenderedPass());

    public sealed override double CaptureProgress() =>
        Session.Bind(static session => session.Progress).IfNone(base.CaptureProgress);

    // --- [HUD]
    public sealed override string HudProductName() => RowText.Localize(TEngine.Product, views).Local;

    public sealed override string HudCustomStatusText() => Conversions.Unset(Session.Bind(static session => session.Status));

    public sealed override bool HudShowCustomStatusText() => Session.Exists(static session => session.Status.IsSome);

    public sealed override int HudLastRenderedPass() =>
        Session.Map(static session => session.Pass).IfNone(base.HudLastRenderedPass());

    public sealed override bool HudShowPasses() => Session.IsSome;

    public sealed override int HudMaximumPasses() =>
        Session.Bind(static session => session.Limit).IfNone(base.HudMaximumPasses());

    public sealed override bool HudShowMaxPasses() => Session.Exists(static session => session.Limit.IsSome);

    public sealed override bool HudAllowEditMaxPasses() => HudShowMaxPasses();

    public sealed override bool HudRendererPaused() => Session.Exists(static session => session.PausedAt.IsSome);

    public sealed override bool HudRendererLocked() => Session.Exists(static session => session.Locked);

    public sealed override DateTime HudStartTime() =>
        Session.Map(static session => session.Started.ToDateTimeUtc()).IfNone(base.HudStartTime());

    private void Attach(HudControl control) {
        control.LeftClicked(this, Clicked(control, HudGesture.LeftClick, control.Act));
        control.RightClicked(this, Clicked(control, HudGesture.RightClick, None));
        control.DoubleClicked(this, Clicked(control, HudGesture.DoubleClick, None));
    }

    private EventHandler Clicked(HudControl control, HudGesture gesture, Option<Func<RealtimeSession, Instant, RealtimeSession>> act) =>
        Callbacks.Handler<EventArgs>(
            _ =>
                from acted in act.TraverseM(Swapped).As()
                from delivered in Delivered(new HudEvent.Clicked(control, gesture))
                select unit,
            CallbackSite.Of(views, this, nameof(Hud))).Invoke;

    private IO<Unit> Edited(int maxPasses) =>
        from limited in Swapped((session, _) => session with { Limit = Some(maxPasses) })
        from delivered in Delivered(new HudEvent.MaxPassesEdited(maxPasses))
        select unit;

    private IO<Unit> Delivered(HudEvent hudEvent) =>
        Hud.TraverseM(deliver => deliver(hudEvent)).As().Map(unit);

    // --- [REPORTS]
    protected Option<RealtimeSession> Session => sessions.Value;

    protected IO<Unit> FrameRendered(int pass, ViewInfo view) =>
        IO.lift(() => ComputeViewportCrc(view))
            .Bind(frame => Swapped((session, _) => session.Locked ? session : session with { Pass = pass, Frame = Some(frame) }));

    protected IO<Unit> FrameReset() => Swapped(static (session, now) => session.Locked ? session : session.Restarted(now));

    protected IO<Unit> StatusChanged(Option<string> status) => Swapped((session, _) => session with { Status = status });

    protected IO<Unit> LimitChanged(Option<int> limit) => Swapped((session, _) => session with { Limit = limit });

    private IO<Unit> Swapped(Func<RealtimeSession, Instant, RealtimeSession> change) =>
        from now in IO.lift(views.Clock.GetCurrentInstant)
        from swapped in atomicIO(() => sessions.Swap(state => state.Map(current => change(current, now))))
        from redrawn in IO.lift(SignalRedraw)
        select unit;
}

public abstract class DefinedRealtimeEngineInfo<TEngine> : RealtimeDisplayModeClassInfo
    where TEngine : DefinedRealtimeEngine<TEngine>, IRealtimeProduct, new() {
    public sealed override string Name => RowText.Localize(TEngine.Product, HostedTypes.Owner(typeof(TEngine))).Local;
    public sealed override Guid GUID => RealtimeDisplayModeType.GUID;
    public sealed override bool DrawOpenGl => IRealtimeProduct.DrawsOpenGl;
    public sealed override Type RealtimeDisplayModeType => typeof(TEngine);
}
