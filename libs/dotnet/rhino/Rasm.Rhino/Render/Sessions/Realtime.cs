using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRealtimeProduct {
    public static abstract string Product { get; }
}

public enum HudGesture { LeftClick, RightClick, DoubleClick }

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PassCount : System.Numerics.IMinMaxValue<PassCount> {
    public static PassCount MinValue { get; } = new(0);
    public static PassCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record RealtimeTarget(PixelExtent Extent, bool ForCapture, RenderWindow Window);

public sealed record RealtimeStart(RealtimeTarget Target, RhinoDoc Document, ViewInfo View, ViewportInfo Viewport, IO<SceneBatch> Take);

public sealed record RealtimeSession(
    RealtimeTarget Target, Disposal<DefinedChangeQueue> Scene, Instant Started, PassCount Pass, Option<PassCount> Limit,
    Option<uint> Frame, Option<string> Status, Option<Instant> PausedAt, bool Locked) {
    public static RealtimeSession Of(RealtimeTarget target, Disposal<DefinedChangeQueue> scene, Instant now) =>
        new(target, scene, now, PassCount.Zero, None, None, None, None, Locked: false);

    public bool Completed => !Locked && Frame.IsSome && (Target.ForCapture || Limit.Exists(limit => Pass >= limit));

    public Option<Fin<ProgressFraction>> Progress =>
        Limit.Map(limit => Pass >= limit ? ProgressFraction.MaxValue : Conversions.Validated<ProgressFraction, float, InvalidRhinoValue>((float)Pass.ToValue() / limit.ToValue()));

    public RealtimeSession Restarted(Instant now) => this with { Started = now, Pass = PassCount.Zero, Frame = None };

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
    public sealed record MaxPassesEdited(PassCount MaxPasses) : HudEvent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedRealtimeEngine<TEngine> : RealtimeDisplayMode
    where TEngine : DefinedRealtimeEngine<TEngine>, IRealtimeProduct, new() {
    // --- [STATE]
    public const bool DrawsOpenGl = false;

    private readonly Atom<(Option<RealtimeSession> Held, Option<RealtimeSession> Taken)> sessions =
        Atom((Held: Option<RealtimeSession>.None, Taken: Option<RealtimeSession>.None));
    private readonly Atom<Option<DisplayPipelineAttributes>> attributes = Atom(Option<DisplayPipelineAttributes>.None);
    private readonly IPlugInViews views;

    protected DefinedRealtimeEngine() => views = (IPlugInViews)IPlugInSink.Of(this);

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
        _ = Callbacks.Answer(IO.lift(Subscribe), static () => unit, CallbackSite.Of(this));

    public sealed override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) =>
        _ = attributes.Swap(_ => Some(displayPipelineAttributes));

    public sealed override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) =>
        Callbacks.Succeeded(
            IO.lift(RenderWindows.Extent(w, h)).Bind(extent => Started(new RealtimeTarget(extent, forCapture, renderWindow), doc, view, viewportInfo)),
            CallbackSite.Of(this));

    public sealed override bool OnRenderSizeChanged(int width, int height) =>
        Callbacks.Succeeded(
            from session in Session
            from resize in Resize
            select IO.lift(RenderWindows.Extent(width, height)).Bind(extent => Resized(session.Target with { Extent = extent }, resize)),
            () => base.OnRenderSizeChanged(width, height),
            CallbackSite.Of(this));

    public sealed override void ShutdownRenderer() =>
        _ = Callbacks.Answer(IO.pure(unit).Bind(_ => Stop()).Finally(Closed), static () => unit, CallbackSite.Of(this));

    public sealed override void GetRenderSize(out int width, out int height) =>
        (width, height) = Session.Map(static session => (session.Target.Extent.Width, session.Target.Extent.Height)).IfNone((0, 0));

    private void Subscribe() {
        SetUseDrawOpenGl(DrawsOpenGl);
        _ = toSeq(HudControl.Items).Iter(Attach);
        MaxPassesChanged += Callbacks.Handler<HudMaxPassesChangedEventArgs>(args => Edited(args.MaxPasses), CallbackSite.Of(this, nameof(MaxPassesChanged)));
        _ = Framebuffer.Iter(draw => OnInitFramebuffer += Callbacks.Handler<InitFramebufferEventArgs>(args => draw(args.Pipeline), CallbackSite.Of(this, nameof(OnInitFramebuffer))));
        _ = Middleground.Iter(draw => OnDrawMiddleground += Callbacks.Handler<DrawMiddlegroundEventArgs>(args => draw(args.Pipeline), CallbackSite.Of(this, nameof(OnDrawMiddleground))));
    }

    private IO<Unit> Started(RealtimeTarget target, RhinoDoc doc, ViewInfo view, ViewportInfo viewport) =>
        from sized in IO.lift(() => target.Window.SetSize(new Size(target.Extent.Width, target.Extent.Height)))
        from added in RenderWindows.AddRequested(target.Window)
        from now in IO.lift(views.Clock.GetCurrentInstant)
        from queue in DefinedChangeQueue.Of((PlugIn)views, doc, view, attributes.Value, target.ForCapture ? QueuePolicy.Capture : QueuePolicy.Viewport, Wake)
        let scene = new Disposal<DefinedChangeQueue>(queue, static owned => owned.Dispose())
        from opened in sessions.SwapIO(_ => (Some(RealtimeSession.Of(target, scene, now)), Option<RealtimeSession>.None))
        from world in DisposalOps.OnFailure(queue.World, Closed)
        from started in DisposalOps.OnFailure(Start(new RealtimeStart(target, doc, view, viewport, queue.Take)), Closed)
        select unit;

    private IO<Unit> Closed =>
        sessions.SwapIO(static state => (Option<RealtimeSession>.None, state.Held))
            .Bind(static state => DisposalOps.Release(state.Taken.Map(static session => session.Scene).ToSeq()));

    private IO<Unit> Resized(RealtimeTarget target, Func<RealtimeTarget, IO<Unit>> resize) =>
        from sized in IO.lift(() => target.Window.SetSize(new Size(target.Extent.Width, target.Extent.Height)))
        from restarted in Swapped((session, now) => (session with { Target = target }).Restarted(now))
        from resized in resize(target)
        select unit;

    // --- [FRAMES]
    public sealed override bool IsRendererStarted() => Session.IsSome;

    public sealed override bool IsFrameBufferAvailable(ViewInfo view) =>
        Session.Exists(session => session.Locked || session.Frame.Exists(frame => session.Target.ForCapture || frame == ComputeViewportCrc(view)));

    public sealed override bool IsCompleted() => Session.Exists(static session => session.Completed);

    public sealed override int LastRenderedPass() =>
        Session.Map(static session => session.Pass.ToValue()).IfNone(base.LastRenderedPass());

    public sealed override double CaptureProgress() =>
        Callbacks.Answer(
            Session.Bind(static session => session.Progress).Map(static progress => IO.lift(progress.Map(static fraction => (double)fraction.ToValue()))),
            base.CaptureProgress,
            base.CaptureProgress,
            CallbackSite.Of(this));

    // --- [HUD]
    public sealed override string HudProductName() => RowText.Localize(TEngine.Product, table: Some<object>(views)).Local;

    public sealed override string HudCustomStatusText() => Conversions.Unset(Session.Bind(static session => session.Status));

    public sealed override bool HudShowCustomStatusText() => Session.Exists(static session => session.Status.IsSome);

    public sealed override int HudLastRenderedPass() =>
        Session.Map(static session => session.Pass.ToValue()).IfNone(base.HudLastRenderedPass());

    public sealed override bool HudShowPasses() => Session.IsSome;

    public sealed override int HudMaximumPasses() =>
        Session.Bind(static session => session.Limit).Map(static limit => limit.ToValue()).IfNone(base.HudMaximumPasses());

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
            CallbackSite.Of(this, nameof(Hud))).Invoke;

    private IO<Unit> Edited(int maxPasses) =>
        from limit in IO.lift(Conversions.Validated<PassCount, int, InvalidRhinoValue>(maxPasses))
        from limited in Swapped((session, _) => session with { Limit = Some(limit) })
        from delivered in Delivered(new HudEvent.MaxPassesEdited(limit))
        select unit;

    private IO<Unit> Delivered(HudEvent hudEvent) =>
        Hud.TraverseM(deliver => deliver(hudEvent)).As().Map(static _ => unit);

    // --- [REPORTS]
    protected Option<RealtimeSession> Session => sessions.Value.Held;

    protected IO<Unit> FrameRendered(PassCount pass, ViewInfo view) =>
        IO.lift(() => ComputeViewportCrc(view))
            .Bind(frame => Swapped((session, _) => session.Locked ? session : session with { Pass = pass, Frame = Some(frame) }));

    protected IO<Unit> FrameReset() => Swapped(static (session, now) => session.Locked ? session : session.Restarted(now));

    protected IO<Unit> StatusChanged(Option<string> status) => Swapped((session, _) => session with { Status = status });

    protected IO<Unit> LimitChanged(Option<PassCount> limit) => Swapped((session, _) => session with { Limit = limit });

    private IO<Unit> Swapped(Func<RealtimeSession, Instant, RealtimeSession> change) =>
        from now in IO.lift(views.Clock.GetCurrentInstant)
        from swapped in sessions.SwapIO(state => (state.Held.Map(current => change(current, now)), Option<RealtimeSession>.None))
        from redrawn in IO.lift(SignalRedraw)
        select unit;
}

public abstract class DefinedRealtimeEngineInfo<TEngine> : RealtimeDisplayModeClassInfo
    where TEngine : DefinedRealtimeEngine<TEngine>, IRealtimeProduct, new() {
    public sealed override string Name => RowText.Localize(TEngine.Product, table: Some<object>(IPlugInSink.Of(this))).Local;
    public sealed override Guid GUID => RealtimeDisplayModeType.GUID;
    public sealed override bool DrawOpenGl => DefinedRealtimeEngine<TEngine>.DrawsOpenGl;
    public sealed override Type RealtimeDisplayModeType => typeof(TEngine);
}
