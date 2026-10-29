using System.Buffers;
using System.Drawing;
using System.Globalization;
using CommunityToolkit.HighPerformance;
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
    public static Fin<Memory2D<float>> Read(RenderWindow.Channel cpu, Rectangle rect) =>
        from layout in cpu.PixelSize() switch { var size => ChannelLayout.TryGet(size, out ChannelLayout? found) ? Fin.Succ(found) : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"channel pixel size {size}"), RhinoReason.Unsupported) }
        from inside in Inside(cpu, rect, layout.Components)
        select Copied(cpu, rect, layout);

    public static Fin<Unit> Put(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory2D<float> rgba) =>
        (RhinoFault.Unless(rgba.Height == rect.Height && rgba.Width == 4 * rect.Width, RhinoReason.Mismatched, (rgba.Height, rgba.Width, rect)),
         rgba.TryGetMemory(out ReadOnlyMemory<float> block) ? Fin.Succ(block) : new RhinoFault($"pitched block of {rect}", RhinoReason.Unsupported),
         Inside(cpu, rect, 4))
            .Apply((_, packed, _) => Pinned(cpu, rect, packed)).As();

    public static Memory2D<float> Widened(Span2D<float> native, int width) {
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
            rect is { X: >= 0, Y: >= 0, Width: > 0, Height: > 0 } && rect.Right <= cpu.Width && rect.Bottom <= cpu.Height && (long)rect.Width * rect.Height * components <= System.Array.MaxLength,
            RhinoReason.Outside, (rect, cpu.Width, cpu.Height));

    private static Memory2D<float> Copied(RenderWindow.Channel cpu, Rectangle rect, ChannelLayout layout) {
        float[] block = GC.AllocateUninitializedArray<float>(layout.Components * rect.Width * rect.Height);
        cpu.GetValues(rect, rect.Width * layout.Key, layout.Order, ref block);
        return new Memory2D<float>(block, rect.Height, layout.Components * rect.Width);
    }

    private static unsafe Unit Pinned(RenderWindow.Channel cpu, Rectangle rect, ReadOnlyMemory<float> rgba) {
        using MemoryHandle pinned = rgba.Pin();
        cpu.SetValues(rect, rect.Size, new PixelBuffer((nint)pinned.Pointer));
        return unit;
    }

    // --- [PIPELINE]
    public static IO<Memory2D<float>> Read(Func<Guid, PostEffectChannel?> open, RenderWindow.StandardChannels channel, Rectangle rect) =>
        Opened(open, RenderWindow.ChannelId(channel), (_, cpu) => IO.lift(() => Read(cpu, rect)));

    public static IO<Unit> Write(Func<Guid, PostEffectChannel?> open, Rectangle rect, ReadOnlyMemory2D<float> rgba, bool commit) =>
        Opened(open, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (opened, cpu) =>
            from written in IO.lift(() => Put(cpu, rect, rgba))
            from committed in when(commit, IO.lift(opened.Commit)).As()
            select unit);

    public static IO<T> Opened<T>(Func<Guid, PostEffectChannel?> open, Guid channel, Func<PostEffectChannel, RenderWindow.Channel, IO<T>> body) =>
        (from opened in Products.Of(() => open(channel))
         from cpu in IO.lift(() => Conversions.Required(opened.CPU()))
         from value in body(opened, cpu)
         select value).Bracket();
}
