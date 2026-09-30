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
    Func<RhinoView, Rectangle, IO<Unit>> BeginRegion,
    Func<RenderEndEventArgs, IO<Unit>> End,
    Func<IO<bool>> Continue,
    Action<Error> Reject);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderWindowSource {
    public sealed record Session(bool FromRenderViewSource) : RenderWindowSource;

    public sealed record Region(ViewportInfo Viewport, bool FromRenderViewSource, Rectangle Bounds) : RenderWindowSource;

    public sealed record Detached(Size Size, ViewInfo View) : RenderWindowSource;
}

public sealed record ChannelPresence(RenderWindow.StandardChannels Channel, bool Available, bool Shown, bool Requested);

// --- [SERVICES] ------------------------------------------------------------------------
public class BatchPipeline(RhinoDoc doc, RunMode mode, PlugIn plugin, Size size, string caption, RenderWindow.StandardChannels channels, bool clearLastRendering, RenderCallbacks callbacks)
    : RenderPipeline(doc, mode, plugin, size, caption, channels, reuseRenderWindow: false, clearLastRendering) {
    public RhinoDoc Document { get; } = doc;

    protected sealed override bool OnRenderBegin() =>
        Answers.Succeeded(callbacks.Begin(), callbacks.Reject);

    protected sealed override bool OnRenderWindowBegin(RhinoView view, Rectangle rectangle) =>
        Answers.Succeeded(callbacks.BeginRegion(view, rectangle), callbacks.Reject);

    protected sealed override void OnRenderEnd(RenderEndEventArgs e) =>
        _ = Answers.Answer(callbacks.End(e), callbacks.Reject, unit);

    protected sealed override bool ContinueModal() =>
        Answers.Answer(callbacks.Continue(), callbacks.Reject, fallback: false);
}

public sealed class AsyncContext(Option<CallbackExecutionControl> executionControl, Action<Error> reject) : AsyncRenderContext {
    private readonly CancellationTokenSource source = new();

    public IO<Unit> Launch(string name, Func<CancellationToken, IO<Unit>> body) =>
        from window in IO.lift(() => Missing.Unless(RenderWindow, nameof(RenderWindow)))
        from registered in executionControl.Traverse(control => IO.lift(() => window.RegisterPostEffectExecutionControl(control))).As()
        from started in IO.lift(() => StartRenderThread(() => window.EndAsyncRender(Answers.Answer(body(source.Token).Map(static _ => RenderWindow.RenderSuccessCode.Completed), reject, RenderWindow.RenderSuccessCode.Failed)), name))
        select unit;

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
            session: static (state, session) => DisposalOps.Using(Window(state.Pipeline, session.FromRenderViewSource), state.Body),
            region: static (state, region) =>
                DisposalOps.Using(Window(state.Pipeline, region.FromRenderViewSource), window =>
                    from wireframe in IO.lift(() => Refused.Unless(
                        window.AddWireframeChannel(state.Pipeline.Document, region.Viewport, RenderPipeline.RenderSize(state.Pipeline.Document, region.FromRenderViewSource), region.Bounds),
                        nameof(RenderWindow.AddWireframeChannel)))
                    from value in state.Body(window)
                    select value),
            detached: static (state, detached) =>
                DisposalOps.Using(IO.lift(() => Missing.Unless(RenderWindow.Create(detached.Size), nameof(RenderWindow.Create))), window =>
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
        DisposalOps.Using(IO.lift(() => Missing.Unless(window.OpenChannel(channel), nameof(RenderWindow.OpenChannel))), opened =>
            from inside in IO.lift(Invalid.Unless(new Rectangle(0, 0, opened.Width, opened.Height).Contains(region), nameof(region)))
            from values in IO.lift(() => toSeq(Values(opened, region, order)))
            select values);

    public static IO<TValue> WithSnapshot<TValue>(RenderWindow window, Func<Bitmap, IO<TValue>> body) =>
        DisposalOps.Using(IO.lift(() => Missing.Unless(window.GetBitmap(), nameof(RenderWindow.GetBitmap))), body);

    /// <summary>Reads the <paramref name="region"/> values of <paramref name="channel"/> row by row into a pinned array a <see cref="PixelBuffer"/> can address, the host taking the row stride in bytes as the width times <see cref="RenderWindow.Channel.PixelSize"/></summary>
    internal static float[] Values(RenderWindow.Channel channel, Rectangle region, ComponentOrders order) {
        int pixel = channel.PixelSize();
        float[] values = GC.AllocateUninitializedArray<float>(region.Width * region.Height * pixel / sizeof(float), pinned: true);
        channel.GetValues(region, region.Width * pixel, order, ref values);
        return values;
    }

    private static IO<RenderWindow> Window(BatchPipeline pipeline, bool fromRenderViewSource) =>
        IO.lift(() => Missing.Unless(pipeline.GetRenderWindowFromRenderViewSource(fromRenderViewSource), nameof(RenderPipeline.GetRenderWindowFromRenderViewSource)));
}
