using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Viewport;
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
    Func<RenderStart, Option<PixelExtent>, RenderWindow, SceneBatch, IO<RenderWork>> Open);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ExecutionControl(Func<Guid, IO<bool>> ready, IPlugInSink sink) : PostEffectExecutionControl {
    private readonly CallbackSite site = new(sink, typeof(ExecutionControl), nameof(ReadyToExecutePostEffect));

    public override bool ReadyToExecutePostEffect(Guid pep_id) =>
        Callbacks.Answer(pep_id, ready, static () => false, site);
}

internal sealed class AsyncContext(IPlugInSink sink) : AsyncRenderContext {
    private readonly CancellationTokenSource cancellation = new();

    public IPlugInSink Sink { get; } = sink;

    public IO<bool> Start(global::Rhino.Render.RenderWindow window, RenderWork work, LocalizeStringPair caption) =>
        from control in IO.lift(() => work.Ready.Map(ready => new ExecutionControl(ready, Sink)).Do(window.RegisterPostEffectExecutionControl))
        from started in IO.lift(() => StartRenderThread(() => Rendered(window, work.Render, control, caption.Local), caption.English))
        select started;

    public override void StopRendering() {
        cancellation.Cancel();
        JoinRenderThread();
        base.StopRendering();
    }

    protected override void Dispose(bool isDisposing) {
        if (isDisposing)
            cancellation.Dispose();
        base.Dispose(isDisposing);
    }

    private void Rendered(global::Rhino.Render.RenderWindow window, IO<Unit> render, Option<ExecutionControl> control, string caption) {
        using EnvIO environment = EnvIO.New(token: cancellation.Token);
        window.EndAsyncRender(Callbacks.Answer(
            IO.lift(() => Try.lift(() => render.Run(environment)).Run())
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
                .Bind(_ => IO.lift(fun(() => window.SetProgress(caption, ProgressFraction.MaxValue))))
                .Map(static _ => global::Rhino.Render.RenderWindow.RenderSuccessCode.Completed),
            static () => global::Rhino.Render.RenderWindow.RenderSuccessCode.Failed,
            new CallbackSite(Sink, typeof(AsyncContext), nameof(StartRenderThread))));
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

    protected override void Dispose(bool isDisposing) {
        if (isDisposing && mode is RunMode.Scripted)
            context.Dispose();
        base.Dispose(isDisposing);
    }

    private bool Begin(Option<Size> quiet, [CallerMemberName] string member = "") =>
        Callbacks.Answer(
            from extent in IO.lift(quiet.Traverse(static size => RenderWindows.Extent(size.Width, size.Height)).As())
            from window in new RenderWindowSource.Rendering(RenderSessionId).Open()
            from world in start.Switch(
                (Pipeline: this, Window: window, Member: member),
                full: static (state, _) => use(() => new RenderSourceView(state.Pipeline.document))
                    .Bind(source => IO.lift(() => Missing.Unless(source.GetViewInfo(), nameof(RenderSourceView.GetViewInfo))))
                    .Bind(view => state.Pipeline.Scene(state.Window, view, state.Member))
                    .Bracket(),
                region: static (state, region) => use(() => new ViewInfo(region.View.ActiveViewport))
                    .Bind(view => state.Pipeline.Scene(state.Window, view, state.Member))
                    .Bracket())
            from work in engine.Open(start, extent, window, world)
            from started in context.Start(window, work, engine.Caption)
            select started,
            static () => false,
            new CallbackSite(context.Sink, GetType(), member));

    private IO<SceneBatch> Scene(global::Rhino.Render.RenderWindow window, ViewInfo view, string member) =>
        from size in IO.lift(window.Size)
        from extent in IO.lift(RenderWindows.Extent(size.Width, size.Height))
        from lens in IO.lift(() => Cameras.Focus(view, document.ModelUnits))
            .Catch(static error => error.IsType<InvalidPixelValue>(), error => IO.lift(fun(() => context.Sink.Report(error, GetType(), member))).Map(static _ => Option<LensFocus>.None))
        from camera in IO.lift(Cameras.ToCamera(view, document.ModelUnits, extent, lens))
        from lights in Cameras.ToLights(document)
        from _ in RenderRuns.Framed(RenderSessionId, RenderRun.Framing(camera, lights, document.ModelUnits, None))
        from __ in when(engine.HostWireframe, RenderWindows.AddWireframe(
            window,
            new WireframeRegion(document, view.Viewport, start.Switch(size, full: static (whole, _) => new Rectangle(Point.Empty, whole), region: static (_, region) => region.Rectangle)),
            size)).As()
        from ___ in RenderWindows.AddRequested(window)
        from world in (from queue in use(DefinedChangeQueue.Of(PlugIn, document, view, None, QueuePolicy.Render, None))
                       from ____ in queue.World
                       from taken in queue.Take
                       select taken).Bracket()
        select world;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Batches {
    public static IO<Unit> Render(RhinoDoc doc, RunMode mode, PlugIn plugIn, RenderEngine engine, RenderStart start) =>
        use(() => new BatchPipeline(doc, mode, plugIn, engine, start, new AsyncContext((IPlugInSink)plugIn)))
            .Bind(pipeline => IO.lift(() => start.Switch(
                pipeline,
                full: static (session, _) => Answered(session.Render(), nameof(RenderPipeline.Render)),
                region: static (session, region) => Answered(session.RenderWindow(region.View, region.Rectangle, region.InWindow), nameof(RenderPipeline.RenderWindow)))))
            .Bracket();

    private static Fin<Unit> Answered(RenderPipeline.RenderReturnCode code, string member) =>
        code switch {
            RenderPipeline.RenderReturnCode.Ok => unit,
            RenderPipeline.RenderReturnCode.Cancel => Errors.Cancelled,
            RenderPipeline.RenderReturnCode.EmptyScene => new Ended(member, Result.Nothing),
            RenderPipeline.RenderReturnCode.NoActiveView => new Missing(nameof(ViewTable.ActiveView)),
            RenderPipeline.RenderReturnCode.ExitRhino => new Ended(member, Result.ExitRhino),
            RenderPipeline.RenderReturnCode.OnPreCreateWindow
                or RenderPipeline.RenderReturnCode.NoFrameWndPointer
                or RenderPipeline.RenderReturnCode.ErrorCreatingWindow
                or RenderPipeline.RenderReturnCode.ErrorStartingRender
                or RenderPipeline.RenderReturnCode.EnterModalLoop
                or RenderPipeline.RenderReturnCode.ExitModalLoop
                or RenderPipeline.RenderReturnCode.InternalError => new RenderRefused(code),
        };
}
