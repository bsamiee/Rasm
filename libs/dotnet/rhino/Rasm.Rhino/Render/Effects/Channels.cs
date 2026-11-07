using System.Buffers;
using System.Drawing;
using CommunityToolkit.HighPerformance;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Channels {

    // --- [IDENTITY]

    public static Rectangle Extent(PostEffectPipeline pipeline) => new(default, pipeline.Dimensions());

    public static Guid[] Required(Seq<RenderWindow.StandardChannels> channels) =>
        [.. RenderWindow.StandardChannels.RGBA.Cons(channels).Distinct().Map(RenderWindow.ChannelId)];

    public static Seq<RenderWindow.StandardChannels> Components(RenderWindow.StandardChannels channel) =>
        channel switch {
            RenderWindow.StandardChannels.None => [],
            RenderWindow.StandardChannels.RGBA => [RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue, RenderWindow.StandardChannels.Alpha],
            RenderWindow.StandardChannels.RGB => [RenderWindow.StandardChannels.Red, RenderWindow.StandardChannels.Green, RenderWindow.StandardChannels.Blue],
            RenderWindow.StandardChannels.NormalXYZ => [RenderWindow.StandardChannels.NormalX, RenderWindow.StandardChannels.NormalY, RenderWindow.StandardChannels.NormalZ],
            RenderWindow.StandardChannels.AlbedoRGB => [RenderWindow.StandardChannels.AlbedoRed, RenderWindow.StandardChannels.AlbedoGreen, RenderWindow.StandardChannels.AlbedoBlue],
            _ => [channel],
        };

    // --- [PIXELS]

    public static Fin<Memory2D<float>> Read(RenderWindow.Channel cpu, Rectangle rect) {
        int size = cpu.PixelSize();
        int components = size / sizeof(float);
        return from order in size switch {
            sizeof(float) => Fin.Succ(ComponentOrders.Irrelevant),
            3 * sizeof(float) => ComponentOrders.RGB,
            4 * sizeof(float) => ComponentOrders.RGBA,
            _ => new RhinoFault(size, RhinoReason.Unsupported),
        }
               from accepted in (Inside(cpu, rect, components),
                       RhinoFault.Unless(rect.Width <= int.MaxValue / (components * sizeof(float)), RhinoReason.Outside, rect))
                   .Apply(static (_, _) => unit).As()
               select Copied(cpu, rect, components, order);
    }

    public static Fin<Unit> Put(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory2D<float> rgba) =>
        (RhinoFault.Unless(rgba.Height == rect.Height && rgba.Width == 4L * rect.Width, RhinoReason.Mismatched, (rgba.Height, rgba.Width, rect)),
         rgba.TryGetMemory(out ReadOnlyMemory<float> block) ? Fin.Succ(block) : new RhinoFault(rect, RhinoReason.Unsupported),
         Inside(cpu, rect, 4))
            .Apply((_, packed, _) => Pinned(cpu, rect, packed)).As();

    public static Memory2D<float> Widened(ReadOnlySpan2D<float> native, int width) {
        float[] rgba = GC.AllocateUninitializedArray<float>(4 * width * native.Height);
        Span2D<float> rows = new(rgba, native.Height, 4 * width);
        int components = native.Width / width;
        for (int y = 0; y < native.Height; y++) {
            Span2D<float> pixels = rows.GetRowSpan(y).AsSpan2D(width, 4);
            native.GetRowSpan(y).AsSpan2D(width, components).CopyTo(pixels[.., ..components]);
            Widen(pixels, components);
        }
        return new Memory2D<float>(rgba, native.Height, 4 * width);
    }

    internal static void Widen(Span2D<float> pixels, int components) {
        if (components is 1) {
            pixels[.., ..1].CopyTo(pixels[.., 1..2]);
            pixels[.., ..1].CopyTo(pixels[.., 2..3]);
        }
        if (components < 4) pixels[.., 3..].Fill(1f);
    }

    private static Fin<Unit> Inside(RenderWindow.Channel cpu, Rectangle rect, int components) =>
        RhinoFault.Unless(
            rect is { X: >= 0, Y: >= 0, Width: > 0, Height: > 0 } && (long)rect.X + rect.Width <= cpu.Width && (long)rect.Y + rect.Height <= cpu.Height && (long)rect.Width * rect.Height <= System.Array.MaxLength / components,
            RhinoReason.Outside, (rect, cpu.Width, cpu.Height));

    private static Memory2D<float> Copied(RenderWindow.Channel cpu, Rectangle rect, int components, ComponentOrders order) {
        float[] block = GC.AllocateUninitializedArray<float>(components * rect.Width * rect.Height);
        cpu.GetValues(rect, rect.Width * components * sizeof(float), order, ref block);
        return new Memory2D<float>(block, rect.Height, components * rect.Width);
    }

    private static unsafe Unit Pinned(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory<float> rgba) {
        using MemoryHandle pinned = rgba.Pin();
        cpu.SetValues(rect, rect.Size, new PixelBuffer((nint)pinned.Pointer));
        return unit;
    }

    // --- [PIPELINE]

    public static IO<Memory2D<float>> Read(Func<Guid, PostEffectChannel?> open, RenderWindow.StandardChannels channel, Rectangle rect) =>
        Opened(open, RenderWindow.ChannelId(channel), (_, cpu) => IO.lift(() => Read(cpu, rect)));

    public static IO<Option<Memory2D<float>>> Offered(Func<Guid, PostEffectChannel?> open, RenderWindow.StandardChannels channel, Rectangle rect) =>
        Available(open, RenderWindow.ChannelId(channel), (_, cpu) => IO.lift(() => Read(cpu, rect)));

    public static IO<Unit> Write(Func<Guid, PostEffectChannel?> open, Rectangle rect, ReadOnlyMemory2D<float> rgba, bool commit) =>
        Opened(open, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (opened, cpu) =>
            from written in IO.lift(() => Put(cpu, rect, rgba))
            from committed in when(commit, IO.lift(opened.Commit)).As()
            select unit);

    public static IO<T> Opened<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        Available(open, channel, body).Bind(found => IO.lift(found.ToFin(new RhinoFault(channel, RhinoReason.Missing))));

    public static IO<Option<T>> Available<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        (from opened in Products.Maybe(() => open(channel))
         from value in opened.Traverse(held => IO.lift(() => Conversions.Required(held.CPU())).Bind(cpu => body(held, cpu))).As()
         select value).Bracket();
}
