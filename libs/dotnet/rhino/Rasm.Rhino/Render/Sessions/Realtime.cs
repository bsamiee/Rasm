using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RealtimeDefinition(string Product, TimeProvider Clock) {
    public bool OpenGl { get; init; }
    public bool FastDraw { get; init; }
    public DisplayTechnology Technology { get; init; } = DisplayTechnology.None;
    public bool Hud { get; init; } = true;
    public bool HudControls { get; init; } = true;
    public bool CaptureProgress { get; init; } = true;
}

public sealed record RealtimeTarget(Size Size, bool ForCapture, RenderWindow Window);

public sealed record RealtimeStart(RealtimeTarget Target, RhinoDoc Document, ViewInfo View, ViewportInfo Viewport, DefinedChangeFeed Feed);

public sealed record RealtimeSession(RealtimeTarget Target, Instant Started, int Pass, Option<int> Limit) {
    public Option<uint> Frame { get; init; }
    public Option<string> Status { get; init; }
    public Option<Instant> PausedAt { get; init; }
    public bool Locked { get; init; }

    public static RealtimeSession Of(RealtimeTarget target, Instant now, Option<int> limit) => new(target, now, 0, limit);

    public bool Completed => !Locked && Frame.IsSome && Limit.Match(Some: limit => Pass >= limit, None: () => Target.ForCapture);

    public Option<double> Progress => Limit.Map(limit => Pass >= limit ? 1d : (double)Pass / limit);

    public RealtimeSession Restarted(Instant now) => this with { Started = now, Pass = 0, Frame = None };

    public RealtimeSession Resumed(Instant now) =>
        PausedAt.Match(Some: at => this with { Started = Started + (now - at), PausedAt = None }, None: () => this);
}

[SmartEnum]
public sealed partial class HudGesture {
    public static readonly HudGesture Left = new(static control => control.LeftClicked, acts: true);
    public static readonly HudGesture Right = new(static control => control.RightClicked, acts: false);
    public static readonly HudGesture Double = new(static control => control.DoubleClicked, acts: false);

    internal Func<HudControl, Func<RealtimeDisplayMode, HostEvent<EventArgs>>> Of { get; }
    internal bool Acts { get; }
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

    internal Func<RealtimeDisplayMode, HostEvent<EventArgs>> LeftClicked { get; }
    internal Func<RealtimeDisplayMode, HostEvent<EventArgs>> RightClicked { get; }
    internal Func<RealtimeDisplayMode, HostEvent<EventArgs>> DoubleClicked { get; }

    [IgnoreMember]
    public Func<RealtimeSession, Instant, RealtimeSession> Act { get; private init; } = static (session, _) => session;

    private static HostEvent<EventArgs> Hud(Action<EventHandler> add, Action<EventHandler> remove, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Subscriptions.On<EventHandler, EventArgs>(add, remove, static handler => (sender, args) => handler(sender, args), label);
}

[Union]
public abstract partial record HudEvent {
    public sealed record Clicked(HudControl Control, HudGesture Gesture) : HudEvent;
    public sealed record MaxPassesEdited(int MaxPasses) : HudEvent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedRealtimeEngine<TSelf> : RealtimeDisplayMode where TSelf : DefinedRealtimeEngine<TSelf>, IDefined<RealtimeDefinition> {
    // --- [STATE]
    private readonly Ref<Option<Owned<RealtimeRun>>> sessions = Ref(Option<Owned<RealtimeRun>>.None);
    private readonly Atom<Option<DisplayPipelineAttributes>> attributes = Atom(Option<DisplayPipelineAttributes>.None);

    private sealed record RealtimeRun(RealtimeSession Session, DefinedChangeFeed Feed);

    // --- [ROWS]
    protected abstract IO<Unit> Start(RealtimeStart start);

    protected abstract IO<Unit> Stop();

    protected virtual Option<IO<Unit>> Wake => None;

    protected virtual Option<Func<RealtimeTarget, IO<Unit>>> Resize => None;

    protected virtual Option<Func<HudEvent, IO<Unit>>> Hud => None;

    protected virtual Option<Func<DisplayPipelineAttributes, IO<Unit>>> Settings => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Framebuffer => None;

    protected virtual Option<Func<DisplayPipeline, IO<Unit>>> Middleground => None;

    // --- [LIFECYCLE]
    public sealed override void PostConstruct() => SetUseDrawOpenGl(TSelf.Definition.OpenGl);

    public sealed override bool DrawOpenGl() => TSelf.Definition.OpenGl;

    public sealed override bool UseFastDraw() => TSelf.Definition.FastDraw;

    public sealed override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) =>
        _ = attributes.Swap(_ => Some(displayPipelineAttributes));

    public sealed override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) =>
        Session.Run(Started(new RealtimeTarget(new Size(w, h), forCapture, renderWindow), doc, view, viewportInfo).Map(static _ => true), failed: false);

    public sealed override bool OnRenderSizeChanged(int width, int height) =>
        Session.Run(
            (from session in Running
             from resize in Resize
             select Resized(session.Target with { Size = new Size(width, height) }, resize).Map(static _ => true))
                .IfNone(IO.lift(() => base.OnRenderSizeChanged(width, height))),
            failed: false);

    public sealed override void ShutdownRenderer() => _ = Session.Run(Stop().Finally(Closed));

    public sealed override void GetRenderSize(out int width, out int height) =>
        (width, height) = Running.Map(static session => (session.Target.Size.Width, session.Target.Size.Height)).IfNone((0, 0));

    private IO<Unit> Started(RealtimeTarget target, RhinoDoc doc, ViewInfo view, ViewportInfo viewport) =>
        (from opened in Products.Replace(sessions,
             from sized in IO.lift(() => target.Window.SetSize(target.Size))
             from added in RenderWindows.AddRequested(target.Window)
             from now in IO.lift(TSelf.Definition.Clock.GetCurrentInstant)
             from passes in IO.lift(() => MaxPasses)
             from attached in Attached
             from feed in DefinedChangeFeed.Of(doc, view, attributes.Value, target.ForCapture ? QueuePolicy.Capture : QueuePolicy.Viewport, Wake)
             select new RealtimeRun(RealtimeSession.Of(target, now, Conversions.Present(target.ForCapture && passes > 0, passes)), feed)).Scoped(Session.Scope)
         from world in opened.Feed.World
         from started in Start(new RealtimeStart(opened.Session.Target, doc, view, viewport, opened.Feed))
         select unit)
        .Catch(error => Products.Each(Seq(IO.lift(Stop).Bind(static stop => stop), Closed)).Map(static _ => unit) >> IO.fail<Unit>(error)).As();

    private IO<Unit> Attached =>
        from hud in (from control in toSeq(HudControl.Items) from gesture in toSeq(HudGesture.Items) select (Control: control, Gesture: gesture))
            .TraverseM(pair => pair.Gesture.Of(pair.Control)(this).Attach(_ =>
                when(pair.Gesture.Acts, Changed(pair.Control.Act)).As() >> Delivered(new HudEvent.Clicked(pair.Control, pair.Gesture)))).As()
        from passes in Subscriptions.On<HudMaxPassesChangedEventArgs>(handler => MaxPassesChanged += handler, handler => MaxPassesChanged -= handler).Attach(args => Edited(args.MaxPasses))
        from settings in Settings.TraverseM(react => Subscriptions.Inline<DisplayPipelineSettingsChangedEventArgs>(
            handler => OnDisplayPipelineSettingsChanged += handler, handler => OnDisplayPipelineSettingsChanged -= handler).Attach(args => react(args.Attributes))).As()
        from framebuffer in Framebuffer.TraverseM(draw => Subscriptions.Inline<InitFramebufferEventArgs>(handler => OnInitFramebuffer += handler, handler => OnInitFramebuffer -= handler).Attach(args => draw(args.Pipeline))).As()
        from middleground in Middleground.TraverseM(draw => Subscriptions.Inline<DrawMiddlegroundEventArgs>(handler => OnDrawMiddleground += handler, handler => OnDrawMiddleground -= handler).Attach(args => draw(args.Pipeline))).As()
        select unit;

    private IO<Unit> Closed => Products.Clear(sessions).Scoped(Session.Scope);

    private IO<Unit> Resized(RealtimeTarget target, Func<RealtimeTarget, IO<Unit>> resize) =>
        from sized in IO.lift(() => target.Window.SetSize(target.Size))
        from restarted in Changed((session, now) => (session with { Target = target }).Restarted(now))
        from resized in resize(target)
        select unit;

    // --- [FRAMES]
    public sealed override bool IsRendererStarted() => Running.IsSome;

    public sealed override bool IsFrameBufferAvailable(ViewInfo view) =>
        Running.Exists(session => session.Locked || session.Frame.Exists(frame => session.Target.ForCapture || frame == ComputeViewportCrc(view)));

    public sealed override bool IsCompleted() => Running.Exists(static session => session.Completed);

    public sealed override int LastRenderedPass() => Running.Map(static session => session.Pass).IfNone(base.LastRenderedPass());

    public sealed override bool ShowCaptureProgress() => TSelf.Definition.CaptureProgress;

    public sealed override double CaptureProgress() => Running.Bind(static session => session.Progress).IfNone(base.CaptureProgress);

    // --- [HUD]
    public sealed override bool HudShow() => TSelf.Definition.Hud;

    public sealed override bool HudShowControls() => TSelf.Definition.HudControls;

    public sealed override string HudProductName() => LocalText.Of(TSelf.Definition.Product).Local;

    public sealed override string HudCustomStatusText() => Conversions.Unset(Running.Bind(static session => session.Status));

    public sealed override bool HudShowCustomStatusText() => Running.Exists(static session => session.Status.IsSome);

    public sealed override int HudLastRenderedPass() => Running.Map(static session => session.Pass).IfNone(base.HudLastRenderedPass());

    public sealed override bool HudShowPasses() => Running.IsSome;

    public sealed override int HudMaximumPasses() => Running.Bind(static session => session.Limit).IfNone(base.HudMaximumPasses());

    public sealed override bool HudShowMaxPasses() => Running.Exists(static session => session.Limit.IsSome);

    public sealed override bool HudAllowEditMaxPasses() => HudShowMaxPasses();

    public sealed override bool HudRendererPaused() => Running.Exists(static session => session.PausedAt.IsSome);

    public sealed override bool HudRendererLocked() => Running.Exists(static session => session.Locked);

    public sealed override DateTime HudStartTime() => Running.Map(static session => session.Started.ToDateTimeUtc()).IfNone(base.HudStartTime());

    private IO<Unit> Edited(int maxPasses) =>
        from limited in Changed((session, _) => session with { Limit = Some(maxPasses) })
        from delivered in Delivered(new HudEvent.MaxPassesEdited(maxPasses))
        select unit;

    private IO<Unit> Delivered(HudEvent hudEvent) => Hud.TraverseM(deliver => deliver(hudEvent)).As().Map(static _ => unit);

    // --- [REPORTS]
    protected Option<RealtimeSession> Running => sessions.Value.Map(static lease => lease.Value.Session);

    protected IO<Unit> FrameRendered(int pass, ViewInfo view) =>
        IO.lift(() => ComputeViewportCrc(view)).Bind(frame => Changed((session, _) => session.Locked ? session : session with { Pass = pass, Frame = Some(frame) }));

    protected IO<Unit> FrameReset() => Changed(static (session, now) => session.Locked ? session : session.Restarted(now));

    protected IO<Unit> StatusChanged(Option<string> status) => Changed((session, _) => session with { Status = status });

    protected IO<Unit> LimitChanged(Option<int> limit) => Changed((session, _) => session with { Limit = limit });

    private IO<Unit> Changed(Func<RealtimeSession, Instant, RealtimeSession> change) =>
        from now in IO.lift(TSelf.Definition.Clock.GetCurrentInstant)
        from swapped in atomicIO(() => sessions.Swap(state => state.Map(lease => lease with { Value = lease.Value with { Session = change(lease.Value.Session, now) } })))
        from redrawn in IO.lift(SignalRedraw)
        select unit;
}

public abstract class DefinedRealtimeEngineInfo<TEngine> : RealtimeDisplayModeClassInfo where TEngine : DefinedRealtimeEngine<TEngine>, IDefined<RealtimeDefinition> {
    public sealed override string Name => LocalText.Of(TEngine.Definition.Product).Local;
    public sealed override Guid GUID => RealtimeDisplayModeType.GUID;
    public sealed override bool DrawOpenGl => TEngine.Definition.OpenGl;
    public sealed override DisplayTechnology RequiredDisplayTechnology => TEngine.Definition.Technology;
    public sealed override Type RealtimeDisplayModeType => typeof(TEngine);
}
