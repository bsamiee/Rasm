using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PixelPass {
    public sealed record Pointwise(Func<Vector4, int, int, Vector4> Pixel) : PixelPass;

    public sealed record Frame(Action<Span<Vector4>, int> Whole) : PixelPass;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectPipeline {
    private static readonly Guid RgbaChannel = RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA);

    public static IO<Unit> Rewrite(PostEffectPipeline pipeline, Rectangle rect, PixelPass pass) =>
        from size in IO.lift(pipeline.Dimensions)
        from rewritten in pass.Switch(
            (Pipeline: pipeline, Rect: rect, Size: size),
            pointwise: static (state, pointwise) => Rewritten(state.Pipeline, state.Rect, values => Mapped(values, state.Rect, state.Size.Height, pointwise.Pixel)),
            frame: static (state, frame) => when(
                state.Rect == new Rectangle(default, state.Size),
                Rewritten(state.Pipeline, state.Rect, values => frame.Whole(MemoryMarshal.Cast<float, Vector4>(values.AsSpan()), state.Rect.Width))).As())
        select rewritten;

    public static IO<Option<float>> OutputGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(PostEffectUuids.Gamma))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return Refused.Unless(gamma.GetParam("gamma", ref value), nameof(PostEffect.GetParam)).Map(_ => (float)value);
            })
            .As());

    private static void Mapped(float[] values, Rectangle rect, int height, Func<Vector4, int, int, Vector4> pixel) =>
        _ = Parallel.For(0, rect.Height, line => {
            Span<Vector4> row = MemoryMarshal.Cast<float, Vector4>(values.AsSpan()).Slice(line * rect.Width, rect.Width);
            for (int column = 0; column < row.Length; column++)
                row[column] = pixel(row[column], rect.X + column, height - 1 - rect.Y - line);
        });

    private static IO<Unit> Rewritten(PostEffectPipeline pipeline, Rectangle rect, Action<float[]> pass) =>
        from values in Opened(pipeline.GetChannelForRead, nameof(PostEffectPipeline.GetChannelForRead), (_, channel) => IO.lift(() => Batches.Values(channel, rect, ComponentOrders.RGBA)))
        from passed in IO.lift(() => pass(values))
        from written in Opened(pipeline.GetChannelForWrite, nameof(PostEffectPipeline.GetChannelForWrite), (opened, channel) =>
            from set in IO.lift(() => channel.SetValues(rect, rect.Size, new PixelBuffer(Marshal.UnsafeAddrOfPinnedArrayElement(values, 0))))
            from committed in IO.lift(opened.Commit)
            select committed)
        select written;

    private static IO<TValue> Opened<TValue>(Func<Guid, PostEffectChannel?> open, string member, Func<PostEffectChannel, RenderWindow.Channel, IO<TValue>> body) =>
        DisposalOps.Using(
            IO.lift(() => Missing.Unless(open(RgbaChannel), member)),
            opened =>
                from channel in IO.lift(() => Missing.Unless(opened.CPU(), nameof(PostEffectChannel.CPU)))
                from value in body(opened, channel)
                select value);
}
