using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
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
internal sealed class ExecutionControl(Func<Guid, IO<bool>> ready, IPlugInViews owner) : PostEffectExecutionControl {
    public override bool ReadyToExecutePostEffect(Guid pep_id) => owner.Answer(ready(pep_id), failed: false);
}

internal sealed class AsyncContext(IPlugInViews owner) : AsyncRenderContext {
    private readonly CancellationTokenSource cancellation = new();
    private readonly Ref<Option<Lease<RenderWork>>> scenes = Ref(Option<Lease<RenderWork>>.None);

    public IPlugInViews Owner { get; } = owner;

    public IO<bool> Start(RenderWindow window, IO<RenderWork> open, LocalizeStringPair caption) =>
        (from work in Owner.Session.Held(Products.Swapped(scenes, open))
         from control in work.Ready.Traverse(ready => IO.lift(() => {
             ExecutionControl registered = new(ready, Owner);
             window.RegisterPostEffectExecutionControl(registered);
             return registered;
         })).As()
         from started in IO.lift(() => StartRenderThread(() => Rendered(window, work.Render, control, caption.Local), caption.English))
         select started)
        .Catch(error => Callbacks.Each(Seq(IO.lift(StopRendering), Owner.Session.Held(Products.Cleared(scenes)))).Map(unit) >> IO.fail<bool>(error)).As();

    public override void StopRendering() =>
        _ = Owner.Run(Callbacks.Each(Seq(IO.lift(cancellation.Cancel), IO.lift(JoinRenderThread), IO.lift(base.StopRendering))));

    protected override void Dispose(bool isDisposing) {
        if (isDisposing) {
            _ = Owner.Run(Owner.Session.Held(Products.Cleared(scenes)));
            cancellation.Dispose();
        }
        base.Dispose(isDisposing);
    }

    private void Rendered(RenderWindow window, IO<Unit> render, Option<ExecutionControl> control, string caption) {
        RenderWindow.RenderSuccessCode answer = Owner.Answer(
            IO.lift(() => Callbacks.Settled(render, cancellation.Token))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
                .Map(RenderWindow.RenderSuccessCode.Completed),
            failed: RenderWindow.RenderSuccessCode.Failed);
        _ = Owner.Run(Callbacks.Each(Seq(
            IO.lift(() => window.SetProgress(caption, 1f)),
            IO.lift(() => window.EndAsyncRender(answer)))));
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
        _ = context.Owner.Run(Callbacks.Each(isDisposing && mode is RunMode.Scripted
            ? Seq(IO.lift(context.Dispose), IO.lift(() => base.Dispose(isDisposing)))
            : Seq(IO.lift(() => base.Dispose(isDisposing)))));

    private bool Begin(Option<Size> quiet, [CallerMemberName] string member = "") =>
        context.Owner.Answer(
            new RenderWindowSource.Rendering(RenderSessionId).Open(window => context.Start(
                window,
                from world in start.Switch(
                    (Pipeline: this, Window: window),
                    full: static (state, _) => state.Pipeline.FullScene(state.Window),
                    region: static (state, region) => IO.lift(() => new ViewInfo(region.View.ActiveViewport))
                        .Bind(view => state.Pipeline.Scene(state.Window, view).Finally(IO.lift(view.Dispose))))
                from work in engine.Open(start, quiet, window, world)
                select work,
                engine.Caption)),
            failed: false,
            member);

    private IO<SceneBatch> FullScene(RenderWindow window) =>
        IO.lift(() => new RenderSourceView(document)).Bind(source =>
            IO.lift(() => Conversions.Found(source.GetViewInfo()))
                .Bind(view => Scene(window, view).Finally(IO.lift(view.Dispose)))
                .Finally(IO.lift(source.Dispose)));

    private IO<SceneBatch> Scene(RenderWindow window, ViewInfo view) =>
        from size in IO.lift(window.Size)
        from wired in when(engine.HostWireframe, RenderWindows.AddWireframe(
            window,
            new WireframeRegion(document, view.Viewport, start.Switch(size, full: static (whole, _) => new Rectangle(System.Drawing.Point.Empty, whole), region: static (_, region) => region.Rectangle)),
            size)).As()
        from added in RenderWindows.AddRequested(window)
        from queue in DefinedChangeFeed.Of(context.Owner, document, view, None, QueuePolicy.Render, None)
        from built in queue.World
        from taken in queue.Take
        from released in release(queue)
        select taken;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Batches {
    public static IO<Unit> Render(RhinoDoc doc, RunMode mode, IPlugInViews plugIn, RenderEngine engine, RenderStart start) =>
        (from context in IO.lift(() => new AsyncContext(plugIn))
         from pipeline in use(() => new BatchPipeline(doc, mode, plugIn.PlugIn, engine, start, context)).Catch(error => IO.lift(context.Dispose) >> IO.fail<BatchPipeline>(error))
         from rendered in IO.lift(() => start.Switch(
             pipeline,
             full: static (session, _) => Answered(session.Render()),
             region: static (session, region) => Answered(session.RenderWindow(region.View, region.Rectangle, region.InWindow))))
         select rendered).Bracket();

    private static Fin<Unit> Answered(RenderPipeline.RenderReturnCode code, [CallerArgumentExpression(nameof(code))] string call = "") =>
        code switch {
            RenderPipeline.RenderReturnCode.Ok => unit,
            RenderPipeline.RenderReturnCode.Cancel => Errors.Cancelled,
            RenderPipeline.RenderReturnCode.EmptyScene => new RhinoFault(call, RhinoReason.Nothing),
            RenderPipeline.RenderReturnCode.ExitRhino => new RhinoFault(call, RhinoReason.Exited),
            RenderPipeline.RenderReturnCode.NoActiveView
                or RenderPipeline.RenderReturnCode.OnPreCreateWindow
                or RenderPipeline.RenderReturnCode.NoFrameWndPointer
                or RenderPipeline.RenderReturnCode.ErrorCreatingWindow
                or RenderPipeline.RenderReturnCode.ErrorStartingRender
                or RenderPipeline.RenderReturnCode.EnterModalLoop
                or RenderPipeline.RenderReturnCode.ExitModalLoop
                or RenderPipeline.RenderReturnCode.InternalError => new RhinoFault($"{call} {code}", RhinoReason.Declined),
        };
}
