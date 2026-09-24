using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderCallbacks(
    Func<IO<Unit>> Begin,
    Option<Func<Size, IO<Unit>>> BeginQuiet,
    Func<RhinoView, Rectangle, IO<Unit>> BeginRegion,
    Func<RenderEndEventArgs, IO<Unit>> End,
    Func<IO<bool>> Continue,
    Option<(Func<IO<Unit>> Pause, Func<IO<Unit>> Resume)> Pausing,
    Option<Func<IO<bool>>> ProcessGeometry,
    Option<Func<IO<bool>>> ProcessLights,
    Option<Func<IO<bool>>> RenderEmptyScene,
    Option<Func<RhinoObject, IO<bool>>> Ignore,
    Option<Func<RhinoObject, Material, Mesh, IO<bool>>> AddMesh,
    Option<Func<LightObject, IO<bool>>> AddLight,
    Action<Error> Reject);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderWindowSource {
    public sealed record Session(bool FromRenderViewSource) : RenderWindowSource;

    public sealed record Viewport(ViewportInfo Info, bool FromRenderViewSource, Rectangle Region) : RenderWindowSource;

    public sealed record Detached(Size Size, ViewInfo View) : RenderWindowSource;
}

public sealed record ChannelPresence(RenderWindow.StandardChannels Channel, bool Available, bool Shown, bool Requested);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class BatchPipeline(RhinoDoc doc, RunMode mode, PlugIn plugin, Size size, string caption, RenderWindow.StandardChannels channels, bool clearLastRendering, RenderCallbacks callbacks)
    : RenderPipeline(doc, mode, plugin, size, caption, channels, reuseRenderWindow: false, clearLastRendering) {
    protected override bool OnRenderBegin() =>
        Answers.Succeeded(callbacks.Begin(), callbacks.Reject);

    protected override bool OnRenderBeginQuiet(Size imageSize) =>
        Answers.Succeeded(callbacks.BeginQuiet.Map(begin => begin(imageSize)), callbacks.Reject, () => base.OnRenderBeginQuiet(imageSize));

    protected override bool OnRenderWindowBegin(RhinoView view, Rectangle rectangle) =>
        Answers.Succeeded(callbacks.BeginRegion(view, rectangle), callbacks.Reject);

    protected override void OnRenderEnd(RenderEndEventArgs e) =>
        _ = Answers.Answer(callbacks.End(e), callbacks.Reject, unit);

    protected override bool ContinueModal() =>
        Answers.Answer(callbacks.Continue(), callbacks.Reject, fallback: false);

    public override bool SupportsPause() =>
        callbacks.Pausing.IsSome;

    public override void PauseRendering() {
        base.PauseRendering();
        _ = Answers.Answer(callbacks.Pausing.Map(static pausing => pausing.Pause()), callbacks.Reject, static () => unit);
    }

    public override void ResumeRendering() {
        base.ResumeRendering();
        _ = Answers.Answer(callbacks.Pausing.Map(static pausing => pausing.Resume()), callbacks.Reject, static () => unit);
    }

    protected override bool NeedToProcessGeometryTable() =>
        Answers.Answer(callbacks.ProcessGeometry.Map(static process => process()), callbacks.Reject, refused: false, base.NeedToProcessGeometryTable);

    protected override bool NeedToProcessLightTable() =>
        Answers.Answer(callbacks.ProcessLights.Map(static process => process()), callbacks.Reject, refused: false, base.NeedToProcessLightTable);

    protected override bool RenderSceneWithNoMeshes() =>
        Answers.Answer(callbacks.RenderEmptyScene.Map(static render => render()), callbacks.Reject, refused: false, base.RenderSceneWithNoMeshes);

    protected override bool IgnoreRhinoObject(RhinoObject obj) =>
        Answers.Answer(callbacks.Ignore.Map(ignore => ignore(obj)), callbacks.Reject, refused: false, () => base.IgnoreRhinoObject(obj));

    protected override bool AddRenderMeshToScene(RhinoObject obj, Material material, Mesh mesh) =>
        Answers.Answer(callbacks.AddMesh.Map(add => add(obj, material, mesh)), callbacks.Reject, refused: false, () => base.AddRenderMeshToScene(obj, material, mesh));

    protected override bool AddLightToScene(LightObject light) =>
        Answers.Answer(callbacks.AddLight.Map(add => add(light)), callbacks.Reject, refused: false, () => base.AddLightToScene(light));
}

public sealed class AsyncContext(Option<CallbackExecutionControl> executionControl, Action<Error> reject) : AsyncRenderContext {
    private readonly CancellationTokenSource source = new();

    public IO<Unit> Launch(string name, Func<CancellationToken, IO<Unit>> body) =>
        from registered in executionControl
            .Traverse(control => IO.lift(() => Missing.Unless(RenderWindow, nameof(RenderWindow)))
                .Bind(window => IO.lift(() => window.RegisterPostEffectExecutionControl(control))))
            .As()
        from started in IO.lift(() => { _ = StartRenderThread(() => Finish(body(source.Token)), name); })
        select started;

    private void Finish(IO<Unit> effect) =>
        Optional(RenderWindow).Match(
            Some: window => window.EndAsyncRender(Answers.Answer(effect.Map(static _ => RenderWindow.RenderSuccessCode.Completed), reject, RenderWindow.RenderSuccessCode.Failed)),
            None: () => reject(new Missing(nameof(RenderWindow))));

    public override void StopRendering() {
        source.Cancel();
        JoinRenderThread();
        base.StopRendering();
    }

    protected override void Dispose(bool isDisposing) {
        if (isDisposing)
            source.Dispose();
        base.Dispose(isDisposing);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Batches {
    // --- [SESSION]
    public static IO<Unit> Render(BatchPipeline pipeline) =>
        IO.lift(() => Answered(pipeline.Render()));

    public static IO<Unit> RenderRegion(BatchPipeline pipeline, RhinoView view, Rectangle region, bool inWindow) =>
        IO.lift(() => Answered(pipeline.RenderWindow(view, region, inWindow)));

    public static IO<Unit> Attach(BatchPipeline pipeline, AsyncContext context) =>
        IO.lift(() => {
            AsyncRenderContext handed = context;
            pipeline.SetAsyncRenderContext(ref handed);
        });

    public static IO<Unit> Save(BatchPipeline pipeline, string path, bool alpha) =>
        IO.lift(() => Refused.Unless(pipeline.SaveImage(path, alpha), nameof(RenderPipeline.SaveImage)));

    private static Fin<Unit> Answered(RenderPipeline.RenderReturnCode code) =>
        code switch {
            RenderPipeline.RenderReturnCode.Ok => unit,
            RenderPipeline.RenderReturnCode.Cancel => new Canceled(),
            RenderPipeline.RenderReturnCode.NoActiveView => new NoActiveView(),
            _ => new RenderRefused(code),
        };

    // --- [WINDOW]
    public static IO<TValue> WithWindow<TValue>(BatchPipeline pipeline, RenderWindowSource scope, Func<RenderWindow, IO<TValue>> body) =>
        scope.Switch(
            (Pipeline: pipeline, Body: body),
            session: static (state, session) => Disposal.Using(
                IO.lift(() => Missing.Unless(state.Pipeline.GetRenderWindowFromRenderViewSource(session.FromRenderViewSource), nameof(RenderPipeline.GetRenderWindowFromRenderViewSource))),
                state.Body),
            viewport: static (state, viewport) => Disposal.Using(
                IO.lift(() => Missing.Unless(state.Pipeline.GetRenderWindow(viewport.Info, viewport.FromRenderViewSource, viewport.Region), nameof(RenderPipeline.GetRenderWindow))),
                state.Body),
            detached: static (state, detached) =>
                Disposal.Using(IO.lift(() => Missing.Unless(RenderWindow.Create(detached.Size), nameof(RenderWindow.Create))), window =>
                    from viewed in IO.lift(() => window.SetView(detached.View))
                    from value in state.Body(window)
                    select value));

    public static IO<Unit> AddChannels(RenderWindow window, Seq<RenderWindow.StandardChannels> channels) =>
        channels.TraverseM(channel => IO.lift(() => Refused.Unless(window.AddChannel(channel), nameof(RenderWindow.AddChannel)))).As().Map(static _ => unit);

    public static IO<Seq<ChannelPresence>> Channels(RenderWindow window, Seq<RenderWindow.StandardChannels> channels) =>
        IO.lift(() => toSeq(window.GetRequestedRenderChannels()))
            .Map(requested => (
                from channel in channels
                let id = RenderWindow.ChannelId(channel)
                select new ChannelPresence(channel, window.IsChannelAvailable(id), window.IsChannelShown(id), requested.Exists(found => found == id))).Strict());

    public static IO<Unit> Blit(RenderWindow window, Rectangle region, Seq<Color4f> colors) =>
        from sized in IO.lift(CountMismatch.Unless(region.Width * region.Height, colors.Count, nameof(colors)))
        from written in IO.lift(() => window.SetRGBAChannelColors(region, [.. colors]))
        from invalidated in IO.lift(() => window.InvalidateArea(region))
        select invalidated;

    public static IO<Seq<float>> Read(RenderWindow window, RenderWindow.StandardChannels channel, Rectangle region, ComponentOrders order) =>
        Disposal.Using(IO.lift(() => Missing.Unless(window.OpenChannel(channel), nameof(RenderWindow.OpenChannel))), opened =>
            from inside in IO.lift(Invalid.Unless(new Rectangle(0, 0, opened.Width, opened.Height).Contains(region), nameof(region)))
            from values in IO.lift(() => {
                float[] buffer = new float[region.Width * region.Height * (opened.PixelSize() / sizeof(float))];
                opened.GetValues(region, region.Width, order, ref buffer);
                return toSeq(buffer);
            })
            select values);

    public static IO<TValue> WithSnapshot<TValue>(RenderWindow window, Func<Bitmap, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Missing.Unless(window.GetBitmap(), nameof(RenderWindow.GetBitmap))), body);
}
