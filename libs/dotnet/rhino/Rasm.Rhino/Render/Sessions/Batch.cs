using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderStart {
    private RenderStart(bool fastPreview) => FastPreview = fastPreview;

    public bool FastPreview { get; }

    public sealed record Full(bool FastPreview) : RenderStart(FastPreview);

    public sealed record Region(bool FastPreview, RhinoView View, Rectangle Rectangle, bool InWindow) : RenderStart(FastPreview);
}

public sealed record RenderWork(IO<Unit> Render, Option<Func<Guid, IO<bool>>> Ready);

public sealed record RenderEngine(
    LocalizeStringPair Caption,
    RenderWindow.StandardChannels Channels,
    bool HostWireframe,
    Func<RenderStart, Option<Size>, RenderWindow, SceneBatch, IO<RenderWork>> Open) {
    internal Seq<Guid> SupportedChannels =>
        toSeq(Enum.GetValues<RenderWindow.StandardChannels>())
            .Filter(channel => int.IsPow2((int)channel)
                && (Channels & ~(RenderWindow.StandardChannels.RGB | RenderWindow.StandardChannels.NormalXYZ | RenderWindow.StandardChannels.AlbedoRGB)).HasFlag(channel))
            .Choose(static channel => Conversions.Present(RenderWindow.ChannelId(channel)));
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ExecutionControl(Func<Guid, IO<bool>> ready, IPlugInSink sink) : PostEffectExecutionControl {
    private readonly CallbackSite site = new(sink, typeof(ExecutionControl), nameof(ReadyToExecutePostEffect));

    public override bool ReadyToExecutePostEffect(Guid pep_id) =>
        Callbacks.Answer(pep_id, ready, static () => false, site);
}

internal sealed class AsyncContext(IPlugInViews sink) : AsyncRenderContext {
    private readonly CancellationTokenSource cancellation = new();
    private readonly Retained<IDisposable> scenes = new(DisposalOps.Release);

    public IPlugInViews Sink { get; } = sink;

    public IO<bool> Start(RenderWindow window, Func<Action<IDisposable>, IO<RenderWork>> open, LocalizeStringPair caption) =>
        DisposalOps.OnFailure(
            from work in DisposalOps.Acquire(open, (_, lease) => scenes.Replace(lease))
            from control in work.Ready.Traverse(ready => DisposalOps.Acquire(IO.lift(() => new ExecutionControl(ready, Sink)), window.RegisterPostEffectExecutionControl)).As()
            from started in IO.lift(() => StartRenderThread(() => Rendered(window, work.Render, control, caption.Local), caption.English))
            select started,
            Callbacks.Each(Seq(IO.lift(StopRendering), scenes.Drain)).Map(unit));

    public override void StopRendering() =>
        _ = Callbacks.Each(Seq(IO.lift(cancellation.Cancel), IO.lift(JoinRenderThread), IO.lift(base.StopRendering))).Run();

    protected override void Dispose(bool isDisposing) {
        try {
            if (isDisposing) scenes.Dispose();
        } finally {
            try {
                if (isDisposing) cancellation.Dispose();
            } finally {
                base.Dispose(isDisposing);
            }
        }
    }

    private void Rendered(RenderWindow window, IO<Unit> render, Option<ExecutionControl> control, string caption) {
        CallbackSite site = new(Sink, typeof(AsyncContext), nameof(StartRenderThread));
        RenderWindow.RenderSuccessCode answer = Callbacks.Answer(
            IO.lift(() => Callbacks.Settled(render, cancellation.Token))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
                .Map(RenderWindow.RenderSuccessCode.Completed),
            static () => RenderWindow.RenderSuccessCode.Failed,
            site);
        Callbacks.Run(Callbacks.Each(Seq(
            IO.lift(() => window.SetProgress(caption, ProgressFraction.MaxValue)),
            IO.lift(() => window.EndAsyncRender(answer)))), site);
        _ = control.Iter(GC.KeepAlive);
    }
}

internal sealed class BatchPipeline : RenderPipeline {
    private readonly RhinoDoc document;
    private readonly RunMode mode;
    private readonly RenderEngine engine;
    private readonly RenderStart start;
    private readonly AsyncContext context;

    public BatchPipeline(RhinoDoc doc, RunMode mode, PlugIn plugIn, RenderEngine engine, RenderStart start, AsyncContext context)
        : this(doc, mode, plugIn, engine, start, context, context) { }

    private BatchPipeline(RhinoDoc doc, RunMode mode, PlugIn plugIn, RenderEngine engine, RenderStart start, AsyncContext context, AsyncRenderContext handed)
        : base(
            doc,
            mode,
            plugIn,
            start.Switch(doc, full: static (held, _) => RenderSize(held, fromRenderSources: true), region: static (_, region) => region.Rectangle.Size),
            engine.Caption.Local,
            engine.Channels,
            reuseRenderWindow: false,
            clearLastRendering: false,
            ref handed) =>
        (document, this.mode, this.engine, this.start, this.context) = (doc, mode, engine, start, context);

    protected override bool OnRenderBegin() => Begin(None);

    protected override bool OnRenderBeginQuiet(Size imageSize) => Begin(imageSize);

    protected override bool OnRenderWindowBegin(RhinoView view, Rectangle rectangle) => Begin(None);

    protected override void OnRenderEnd(RenderEndEventArgs e) => context.StopRendering();

    protected override bool ContinueModal() => context.RenderThread is { IsAlive: true };

    protected override void Dispose(bool isDisposing) =>
        _ = Callbacks.Each(isDisposing && mode is RunMode.Scripted
            ? Seq(IO.lift(context.Dispose), IO.lift(() => base.Dispose(isDisposing)))
            : Seq(IO.lift(() => base.Dispose(isDisposing)))).Run();

    private bool Begin(Option<Size> quiet, [CallerMemberName] string member = "") =>
        Callbacks.Answer(
            new RenderWindowSource.Rendering(RenderSessionId).Open(window => context.Start(window, owned =>
                from world in start.Switch(
                    (Pipeline: this, Window: window, Retain: owned),
                    full: static (state, _) => state.Pipeline.FullScene(state.Window, state.Retain),
                    region: static (state, region) => DisposalOps.Within(() => new ViewInfo(region.View.ActiveViewport), view => state.Pipeline.Scene(state.Window, view, state.Retain)))
                from work in engine.Open(start, quiet, window, world)
                select work, engine.Caption)),
            static () => false,
            new CallbackSite(context.Sink, typeof(BatchPipeline), member));

    private IO<SceneBatch> FullScene(RenderWindow window, Action<IDisposable> retain) =>
        DisposalOps.Within(() => new RenderSourceView(document), source =>
            DisposalOps.Found(source.GetViewInfo, nameof(RenderSourceView.GetViewInfo), view => Scene(window, view, retain)));

    private IO<SceneBatch> Scene(RenderWindow window, ViewInfo view, Action<IDisposable> retain) =>
        from size in IO.lift(window.Size)
        from __ in when(engine.HostWireframe, RenderWindows.AddWireframe(
            window,
            new WireframeRegion(document, view.Viewport, start.Switch(size, full: static (whole, _) => new Rectangle(System.Drawing.Point.Empty, whole), region: static (_, region) => region.Rectangle)),
            size)).As()
        from ___ in RenderWindows.AddRequested(window)
        from world in DisposalOps.Within<IDisposable, SceneBatch>(owned =>
            from queue in DefinedChangeFeed.Of(context.Sink, document, view, None, QueuePolicy.Render, None, owned)
            from ____ in queue.World
            from taken in queue.Take(retain)
            select taken, DisposalOps.Release)
        select world;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Batches {
    public static IO<Unit> Render(RhinoDoc doc, RunMode mode, IPlugInViews plugIn, RenderEngine engine, RenderStart start) =>
        DisposalOps.Within<BatchPipeline, Unit>(retain => DisposalOps.Acquire(IO.lift(() => new AsyncContext(plugIn)).Bind(context => DisposalOps.OnFailure(
            IO.lift(() => new BatchPipeline(doc, mode, plugIn.PlugIn, engine, start, context)), IO.lift(context.Dispose))), retain)
            .Bind(pipeline => IO.lift(() => start.Switch(
                pipeline,
                full: static (session, _) => Answered(session.Render(), nameof(RenderPipeline.Render)),
                region: static (session, region) => Answered(session.RenderWindow(region.View, region.Rectangle, region.InWindow), nameof(RenderPipeline.RenderWindow))))),
            DisposalOps.Release);

    private static Fin<Unit> Answered(RenderPipeline.RenderReturnCode code, string member) =>
        code switch {
            RenderPipeline.RenderReturnCode.Ok => unit,
            RenderPipeline.RenderReturnCode.Cancel => Errors.Cancelled,
            RenderPipeline.RenderReturnCode.EmptyScene => new Ended(member, Result.Nothing),
            RenderPipeline.RenderReturnCode.NoActiveView => new Refused(nameof(ViewTable.ActiveView), Refused.Absent),
            RenderPipeline.RenderReturnCode.ExitRhino => new Ended(member, Result.ExitRhino),
            RenderPipeline.RenderReturnCode.OnPreCreateWindow
                or RenderPipeline.RenderReturnCode.NoFrameWndPointer
                or RenderPipeline.RenderReturnCode.ErrorCreatingWindow
                or RenderPipeline.RenderReturnCode.ErrorStartingRender
                or RenderPipeline.RenderReturnCode.EnterModalLoop
                or RenderPipeline.RenderReturnCode.ExitModalLoop
                or RenderPipeline.RenderReturnCode.InternalError => Error.New($"{member} returned {code}"),
        };
}
