using System.Buffers;
using System.Drawing;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class ChannelLayout {
    public static readonly ChannelLayout Scalar = new(sizeof(float), ComponentOrders.Irrelevant);
    public static readonly ChannelLayout Triple = new(3 * sizeof(float), ComponentOrders.RGB);
    public static readonly ChannelLayout Rgba = new(4 * sizeof(float), ComponentOrders.RGBA);

    public ComponentOrders Order { get; }
    public int Components => Key / sizeof(float);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Channels {
    // --- [IDENTITY]
    public static Rectangle Extent(PostEffectPipeline pipeline) => new(default, pipeline.Dimensions());

    public static Guid[] Required(Seq<RenderWindow.StandardChannels> channels) =>
        [.. RenderWindow.StandardChannels.RGBA.Cons(channels).Distinct().Map(RenderWindow.ChannelId)];

    // --- [PIXELS]
    public static Fin<float[]> Read(RenderWindow.Channel cpu, Rectangle rect) =>
        from layout in Fin.Succ(ChannelLayout.Get(cpu.PixelSize()))
        from bounded in RhinoFault.Unless(
            rect is { X: >= 0, Y: >= 0, Width: > 0, Height: > 0 } && rect.Right <= cpu.Width && rect.Bottom <= cpu.Height && (long)rect.Width * layout.Key <= int.MaxValue)
        select Widened(cpu, rect, layout, rect.Width * layout.Key);

    public static Fin<Unit> Put(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory<float> rgba) =>
        RhinoFault.Unless(rgba.Length == 4L * rect.Width * rect.Height && rect is { X: >= 0, Y: >= 0 } && rect.Right <= cpu.Width && rect.Bottom <= cpu.Height)
            .Map(_ => Pinned(cpu, rect, rgba));

    internal static void Widen(float[] block, int components, int stride, int pixels) {
        for (int pixel = pixels - 1; components < 4 && pixel >= 0; pixel--) {
            int source = pixel * stride;
            (block[4 * pixel], block[(4 * pixel) + 1], block[(4 * pixel) + 2], block[(4 * pixel) + 3]) =
                (block[source], block[source + int.Min(1, components - 1)], block[source + int.Min(2, components - 1)], 1f);
        }
    }

    private static unsafe Unit Pinned(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory<float> rgba) {
        using MemoryHandle pinned = rgba.Pin();
        cpu.SetValues(rect, rect.Size, new PixelBuffer((nint)pinned.Pointer));
        return unit;
    }

    private static float[] Widened(RenderWindow.Channel cpu, Rectangle rect, ChannelLayout layout, int stride) {
        float[] block = new float[4 * rect.Width * rect.Height];
        cpu.GetValues(rect, stride, layout.Order, ref block);
        Widen(block, layout.Components, layout.Components, rect.Width * rect.Height);
        return block;
    }

    // --- [PIPELINE]
    public static IO<float[]> Read(Func<Guid, PostEffectChannel?> open, RenderWindow.StandardChannels channel, Rectangle rect) =>
        Opened(open, RenderWindow.ChannelId(channel), (_, cpu) => IO.lift(() => Read(cpu, rect)));

    public static IO<Unit> Write(PostEffectPipeline pipeline, Rectangle rect, ReadOnlyMemory<float> rgba) =>
        Opened(pipeline.GetChannelForWrite, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (opened, cpu) =>
            from written in IO.lift(() => Put(cpu, rect, rgba))
            from committed in IO.lift(opened.Commit)
            select committed);

    public static IO<Option<T>> Probed<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        unitIO.Bind(_ => Products.Held(open(channel))).Bind(found => found.Traverse(opened => Mapped(opened, body)).As()).Bracket();

    public static IO<T> Opened<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        unitIO.Bind(_ => Products.Made(open(channel))).Bind(opened => Mapped(opened, body)).Bracket();

    private static IO<T> Mapped<T>(PostEffectChannel opened, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        IO.lift(() => Conversions.Found(opened.CPU())).Bind(cpu => body(opened, cpu));
}
