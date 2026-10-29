using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
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

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ExecutionControl(Func<Guid, IO<bool>> ready) : PostEffectExecutionControl {
    public override bool ReadyToExecutePostEffect(Guid pep_id) => Session.Run(ready(pep_id), failed: false);
}

internal sealed class AsyncContext : AsyncRenderContext {
    private readonly CancellationTokenSource cancellation = new();
    private readonly Ref<Option<Owned<RenderWork>>> works = Ref(Option<Owned<RenderWork>>.None);

    public Option<RenderWork> Work => works.Value.Map(static owned => owned.Value);

    public IO<bool> Start(RenderWindow window, IO<RenderWork> open, LocalizeStringPair caption) =>
        (from work in Products.Replace(works, open).Scoped(Session.Scope)
         from control in work.Ready.Traverse(ready => IO.lift(() => {
             ExecutionControl registered = new(ready);
             window.RegisterPostEffectExecutionControl(registered);
             return registered;
         })).As()
         from started in IO.lift(() => StartRenderThread(() => Rendered(window, work.Render, control, caption.Local), caption.English))
         select started)
        .Catch(error => Products.Each(Seq(IO.lift(StopRendering), Products.Clear(works).Scoped(Session.Scope))).Map(static _ => unit) >> IO.fail<bool>(error)).As();

    public override void StopRendering() =>
        _ = Session.Run(Products.Each(Seq(IO.lift(cancellation.Cancel), IO.lift(JoinRenderThread), IO.lift(base.StopRendering))));

    protected override void Dispose(bool isDisposing) {
        if (isDisposing) {
            _ = Session.Run(Products.Clear(works).Scoped(Session.Scope));
            cancellation.Dispose();
        }
        base.Dispose(isDisposing);
    }

    private void Rendered(RenderWindow window, IO<Unit> render, Option<ExecutionControl> control, string caption) {
        RenderWindow.RenderSuccessCode answer = Session.Run(
            IO.lift(() => Session.RunSafe(render, cancellation.Token))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
                .Map(static _ => RenderWindow.RenderSuccessCode.Completed),
            failed: RenderWindow.RenderSuccessCode.Failed);
        _ = Session.Run(Products.Each(Seq(IO.lift(() => window.SetProgress(caption, 1f)), IO.lift(() => window.EndAsyncRender(answer)))));
        _ = control.Iter(GC.KeepAlive);
    }
}

internal sealed class BatchPipeline : RenderPipeline {
    private readonly RhinoDoc document;
    private readonly RunMode mode;
    private readonly RenderEngine engine;
    private readonly RenderStart start;
    private readonly AsyncContext context;

    public BatchPipeline(RhinoDoc doc, RunMode mode, RenderEngine engine, RenderStart start, AsyncContext context)
        : this(doc, mode, engine, start, context, context) { }

    private BatchPipeline(RhinoDoc doc, RunMode mode, RenderEngine engine, RenderStart start, AsyncContext context, AsyncRenderContext handed)
        : base(
            doc,
            mode,
            Session.PlugIn,
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

    public override bool SupportsPause() => context.Work.Exists(static work => work.Pause.IsSome);

    public override void PauseRendering() => Paused(paused: true);

    public override void ResumeRendering() => Paused(paused: false);

    protected override void Dispose(bool isDisposing) =>
        _ = Session.Run(Products.Each(isDisposing && mode is RunMode.Scripted
            ? Seq(IO.lift(context.Dispose), IO.lift(() => base.Dispose(isDisposing)))
            : Seq(IO.lift(() => base.Dispose(isDisposing)))));

    private void Paused(bool paused) => _ = Session.Run(context.Work.Bind(static work => work.Pause).Traverse(pause => pause(paused)).As());

    private bool Begin(Option<Size> quiet, [CallerMemberName] string member = "") =>
        Session.Run(
            from window in IO.lift(() => Conversions.Required(global::Rhino.Render.RenderWindow.FromSessionId(RenderSessionId)))
            from started in context.Start(
                window,
                from view in start.Switch(
                    document,
                    full: static (doc, _) => from source in use(() => new RenderSourceView(doc)) from view in Products.Of(source.GetViewInfo) select view,
                    region: static (_, region) => use(() => new ViewInfo(region.View.ActiveViewport)))
                from feed in Scene(window, view)
                from work in engine.Open(start, quiet, window, feed)
                select work,
                engine.Caption)
            select started,
            failed: false,
            member);

    private IO<DefinedChangeFeed> Scene(RenderWindow window, ViewInfo view) =>
        from size in IO.lift(window.Size)
        from wired in when(engine.HostWireframe, Products.Accepted(() => window.AddWireframeChannel(
            document, view.Viewport, size, start.Switch(size, full: static (whole, _) => new Rectangle(default, whole), region: static (_, region) => region.Rectangle)))).As()
        from added in RenderWindows.AddRequested(window)
        from feed in DefinedChangeFeed.Of(document, view, None, QueuePolicy.Render, None)
        from built in feed.World
        select feed;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Batches {
    public static IO<Result> Render(RhinoDoc doc, RunMode mode, RenderEngine engine, RenderStart start) =>
        (from pipeline in Products.Transfer(static () => new AsyncContext(), context => use(() => new BatchPipeline(doc, mode, engine, start, context)))
         from code in IO.lift(() => start.Switch(
             pipeline,
             full: static (session, _) => session.Render(),
             region: static (session, region) => session.RenderWindow(region.View, region.Rectangle, region.InWindow)))
         from result in IO.lift(() => pipeline.CommandResult() switch {
             Result.Failure => Fin.Fail<Result>(new RhinoFault(RenderPipeline.LocalizeRenderReturnCode(code), RhinoReason.Declined)),
             var settled => settled,
         })
         select result).Bracket();
}
