using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.RhinoCommon.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRealtimeProduct {
    public const bool DrawsOpenGl = false;

    public static abstract string Product { get; }
}

public enum HudGesture { LeftClick, RightClick, DoubleClick }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RealtimeTarget(Size Size, bool ForCapture, RenderWindow Window);

public sealed record RealtimeStart(RealtimeTarget Target, RhinoDoc Document, ViewInfo View, ViewportInfo Viewport, DefinedChangeFeed Feed);

public sealed record RealtimeSession(
    RealtimeTarget Target, Instant Started, int Pass, Option<int> Limit,
    Option<uint> Frame, Option<string> Status, Option<Instant> PausedAt, bool Locked) {
    public static RealtimeSession Of(RealtimeTarget target, Instant now, Option<int> limit) =>
        new(target, now, 0, limit, None, None, None, Locked: false);

    public bool Completed => !Locked && Frame.IsSome && Limit.Match(Some: limit => Pass >= limit, None: () => Target.ForCapture);

    public Option<double> Progress => Limit.Map(limit => Pass >= limit ? 1d : (double)Pass / limit);

    public RealtimeSession Restarted(Instant now) => this with { Started = now, Pass = 0, Frame = None };

    public RealtimeSession Resumed(Instant now) =>
        PausedAt.Match(Some: at => this with { Started = Started + (now - at), PausedAt = None }, None: () => this);
}

[SmartEnum]
public sealed partial class HudControl {
    public static readonly HudControl Play = new(
        static mode => Hud(handler => mode.HudPlayButtonLeftClicked += handler, handler => mode.HudPlayButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudPlayButtonRightClicked += handler, handler => mode.HudPlayButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudPlayButtonDoubleClicked += handler, handler => mode.HudPlayButtonDoubleClicked -= handler)) {
        Act = static (session, now) => session.Resumed(now),
    };
    public static readonly HudControl Pause = new(
        static mode => Hud(handler => mode.HudPauseButtonLeftClicked += handler, handler => mode.HudPauseButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudPauseButtonRightClicked += handler, handler => mode.HudPauseButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudPauseButtonDoubleClicked += handler, handler => mode.HudPauseButtonDoubleClicked -= handler)) {
        Act = static (session, now) => session with { PausedAt = session.PausedAt | Some(now) },
    };
    public static readonly HudControl Lock = new(
        static mode => Hud(handler => mode.HudLockButtonLeftClicked += handler, handler => mode.HudLockButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudLockButtonRightClicked += handler, handler => mode.HudLockButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudLockButtonDoubleClicked += handler, handler => mode.HudLockButtonDoubleClicked -= handler)) {
        Act = static (session, _) => session with { Locked = true },
    };
    public static readonly HudControl Unlock = new(
        static mode => Hud(handler => mode.HudUnlockButtonLeftClicked += handler, handler => mode.HudUnlockButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudUnlockButtonRightClicked += handler, handler => mode.HudUnlockButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudUnlockButtonDoubleClicked += handler, handler => mode.HudUnlockButtonDoubleClicked -= handler)) {
        Act = static (session, _) => session with { Locked = false },
    };
    public static readonly HudControl ProductName = new(
        static mode => Hud(handler => mode.HudProductNameLeftClicked += handler, handler => mode.HudProductNameLeftClicked -= handler),
        static mode => Hud(handler => mode.HudProductNameRightClicked += handler, handler => mode.HudProductNameRightClicked -= handler),
        static mode => Hud(handler => mode.HudProductNameDoubleClicked += handler, handler => mode.HudProductNameDoubleClicked -= handler));
    public static readonly HudControl StatusText = new(
        static mode => Hud(handler => mode.HudStatusTextLeftClicked += handler, handler => mode.HudStatusTextLeftClicked -= handler),
        static mode => Hud(handler => mode.HudStatusTextRightClicked += handler, handler => mode.HudStatusTextRightClicked -= handler),
        static mode => Hud(handler => mode.HudStatusTextDoubleClicked += handler, handler => mode.HudStatusTextDoubleClicked -= handler));
    public static readonly HudControl Time = new(
        static mode => Hud(handler => mode.HudTimeLeftClicked += handler, handler => mode.HudTimeLeftClicked -= handler),
        static mode => Hud(handler => mode.HudTimeRightClicked += handler, handler => mode.HudTimeRightClicked -= handler),
        static mode => Hud(handler => mode.HudTimeDoubleClicked += handler, handler => mode.HudTimeDoubleClicked -= handler));
    public static readonly HudControl PostEffectsOn = new(
        static mode => Hud(handler => mode.HudPostEffectsOnButtonLeftClicked += handler, handler => mode.HudPostEffectsOnButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudPostEffectsOnButtonRightClicked += handler, handler => mode.HudPostEffectsOnButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudPostEffectsOnButtonDoubleClicked += handler, handler => mode.HudPostEffectsOnButtonDoubleClicked -= handler));
    public static readonly HudControl PostEffectsOff = new(
        static mode => Hud(handler => mode.HudPostEffectsOffButtonLeftClicked += handler, handler => mode.HudPostEffectsOffButtonLeftClicked -= handler),
        static mode => Hud(handler => mode.HudPostEffectsOffButtonRightClicked += handler, handler => mode.HudPostEffectsOffButtonRightClicked -= handler),
        static mode => Hud(handler => mode.HudPostEffectsOffButtonDoubleClicked += handler, handler => mode.HudPostEffectsOffButtonDoubleClicked -= handler));

    public Func<RealtimeDisplayMode, HostEvent<EventArgs>> LeftClicked { get; }

    public Func<RealtimeDisplayMode, HostEvent<EventArgs>> RightClicked { get; }

    public Func<RealtimeDisplayMode, HostEvent<EventArgs>> DoubleClicked { get; }

    [IgnoreMember]
    public Func<RealtimeSession, Instant, RealtimeSession> Act { get; private init; } = static (session, _) => session;

    private static HostEvent<EventArgs> Hud(Action<EventHandler> add, Action<EventHandler> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Subscriptions.Host<EventHandler, EventArgs>(add, remove, static handler => (sender, args) => handler(sender, args), label);
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
    private readonly Ref<Option<Lease<RealtimeRun>>> sessions = Ref(Option<Lease<RealtimeRun>>.None);
    private readonly Atom<Option<DisplayPipelineAttributes>> attributes = Atom(Option<DisplayPipelineAttributes>.None);
    private readonly IPlugInViews views;

    protected DefinedRealtimeEngine() => views = PlugInOwners.Owner(typeof(TEngine));

    private sealed record RealtimeRun(RealtimeSession Session, DefinedChangeFeed Feed);

    // --- [ROWS]
    protected abstract IO<Unit> Start(RealtimeStart start);

    protected abstract IO<Unit> Stop();

    protected virtual Option<IO<Unit>> Wake => None;

    protected virtual Option<Func<RealtimeTarget, IO<Unit>>> Resize => None;

    protected virtual Option<Func<HudEvent, IO<Unit>>> Hud => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Framebuffer => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Middleground => None;

    // --- [LIFECYCLE]
    public sealed override void PostConstruct() => SetUseDrawOpenGl(IRealtimeProduct.DrawsOpenGl);

    public sealed override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) =>
        _ = attributes.Swap(_ => Some(displayPipelineAttributes));

    public sealed override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) =>
        views.Answer(Started(w, h, doc, view, viewportInfo, forCapture, renderWindow).Map(value: true), failed: false);

    public sealed override bool OnRenderSizeChanged(int width, int height) =>
        (from session in Session
         from resize in Resize
         select Resized(session.Target with { Size = new Size(width, height) }, resize))
            .Match(Some: resized => views.Answer(resized.Map(value: true), failed: false), None: () => base.OnRenderSizeChanged(width, height));

    public sealed override void ShutdownRenderer() => _ = views.Run(Stop().Finally(Closed));

    public sealed override void GetRenderSize(out int width, out int height) =>
        (width, height) = Session.Map(static session => (session.Target.Size.Width, session.Target.Size.Height)).IfNone((0, 0));

    private IO<Unit> Started(int width, int height, RhinoDoc doc, ViewInfo view, ViewportInfo viewport, bool forCapture, RenderWindow window) =>
        (from opened in views.Session.Held(Products.Swapped(sessions,
             from framing in use(() => viewport)
             let target = new RealtimeTarget(new Size(width, height), forCapture, window)
             from sized in IO.lift(() => target.Window.SetSize(target.Size))
             from added in RenderWindows.AddRequested(target.Window)
             from now in IO.lift(views.Clock.GetCurrentInstant)
             from passes in IO.lift(() => MaxPasses)
             from attached in Attached
             from feed in DefinedChangeFeed.Of(views, doc, view, attributes.Value, target.ForCapture ? QueuePolicy.Capture : QueuePolicy.Viewport, Wake)
             select new RealtimeRun(RealtimeSession.Of(target, now, Conversions.Found(target.ForCapture && passes > 0, passes)), feed)))
         from world in opened.Feed.World
         from started in Start(new RealtimeStart(opened.Session.Target, doc, view, viewport, opened.Feed))
         select unit)
        .Catch(error => Callbacks.Each(Seq(unitIO.Bind(_ => Stop()), Closed)).Map(unit) >> IO.fail<Unit>(error)).As();

    private IO<Unit> Attached =>
        from hud in toSeq(HudControl.Items).TraverseM(control =>
            from left in control.LeftClicked(this).Inline(_ => Changed(control.Act) >> Delivered(new HudEvent.Clicked(control, HudGesture.LeftClick)), views)
            from right in control.RightClicked(this).Inline(_ => Delivered(new HudEvent.Clicked(control, HudGesture.RightClick)), views)
            from twice in control.DoubleClicked(this).Inline(_ => Delivered(new HudEvent.Clicked(control, HudGesture.DoubleClick)), views)
            select unit).As()
        from passes in Subscriptions.Host<HudMaxPassesChangedEventArgs>(handler => MaxPassesChanged += handler, handler => MaxPassesChanged -= handler)
            .Inline(args => Edited(args.MaxPasses), views)
        from framebuffer in Framebuffer.TraverseM(draw => Subscriptions.During<InitFramebufferEventArgs>(handler => OnInitFramebuffer += handler, handler => OnInitFramebuffer -= handler)
            .Inline(args => draw(args.Pipeline), views)).As()
        from middleground in Middleground.TraverseM(draw => Subscriptions.During<DrawMiddlegroundEventArgs>(handler => OnDrawMiddleground += handler, handler => OnDrawMiddleground -= handler)
            .Inline(args => draw(args.Pipeline), views)).As()
        select unit;

    private IO<Unit> Closed => views.Session.Held(Products.Cleared(sessions));

    private IO<Unit> Resized(RealtimeTarget target, Func<RealtimeTarget, IO<Unit>> resize) =>
        from sized in IO.lift(() => target.Window.SetSize(target.Size))
        from restarted in Changed((session, now) => (session with { Target = target }).Restarted(now))
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

    private IO<Unit> Edited(int maxPasses) =>
        from limited in Changed((session, _) => session with { Limit = Some(maxPasses) })
        from delivered in Delivered(new HudEvent.MaxPassesEdited(maxPasses))
        select unit;

    private IO<Unit> Delivered(HudEvent hudEvent) =>
        Hud.TraverseM(deliver => deliver(hudEvent)).As().Map(unit);

    // --- [REPORTS]
    protected Option<RealtimeSession> Session => sessions.Value.Map(static lease => lease.Value.Session);

    protected IO<Unit> FrameRendered(int pass, ViewInfo view) =>
        IO.lift(() => ComputeViewportCrc(view))
            .Bind(frame => Changed((session, _) => session.Locked ? session : session with { Pass = pass, Frame = Some(frame) }));

    protected IO<Unit> FrameReset() => Changed(static (session, now) => session.Locked ? session : session.Restarted(now));

    protected IO<Unit> StatusChanged(Option<string> status) => Changed((session, _) => session with { Status = status });

    protected IO<Unit> LimitChanged(Option<int> limit) => Changed((session, _) => session with { Limit = limit });

    private IO<Unit> Changed(Func<RealtimeSession, Instant, RealtimeSession> change) =>
        from now in IO.lift(views.Clock.GetCurrentInstant)
        from swapped in atomicIO(() => sessions.Swap(state => state.Map(lease => lease with { Value = lease.Value with { Session = change(lease.Value.Session, now) } })))
        from redrawn in IO.lift(SignalRedraw)
        select unit;
}

public abstract class DefinedRealtimeEngineInfo<TEngine> : RealtimeDisplayModeClassInfo
    where TEngine : DefinedRealtimeEngine<TEngine>, IRealtimeProduct, new() {
    public sealed override string Name => RowText.Localize(TEngine.Product, PlugInOwners.Owner(typeof(TEngine))).Local;
    public sealed override Guid GUID => RealtimeDisplayModeType.GUID;
    public sealed override bool DrawOpenGl => IRealtimeProduct.DrawsOpenGl;
    public sealed override Type RealtimeDisplayModeType => typeof(TEngine);
}
