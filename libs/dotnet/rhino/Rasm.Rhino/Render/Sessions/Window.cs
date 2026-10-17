using System.Drawing;
using Rasm.Imaging.Pixels;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WireframeRegion(RhinoDoc Document, ViewportInfo Viewport, Rectangle Region);

[Union]
public abstract partial record RenderWindowSource {
    public abstract IO<T> Open<T>(Func<RenderWindow, IO<T>> body);

    public sealed record Detached(PixelExtent Extent, ViewInfo View) : RenderWindowSource {
        public override IO<T> Open<T>(Func<RenderWindow, IO<T>> body) =>
            DisposalOps.Found(() => RenderWindow.Create(new Size(Extent.Width, Extent.Height)), nameof(RenderWindow.Create),
                window => IO.lift(() => window.SetView(View)).Bind(_ => body(window)));
    }

    public sealed record Rendering(Guid SessionId) : RenderWindowSource {
        public override IO<T> Open<T>(Func<RenderWindow, IO<T>> body) =>
            IO.lift(() => Refused.UnlessFound(RenderWindow.FromSessionId(SessionId), nameof(RenderWindow.FromSessionId))).Bind(body);
    }
}

public sealed record WindowChannels(
    Seq<RenderWindow.StandardChannels> Requested,
    LanguageExt.HashSet<RenderWindow.StandardChannels> Available,
    Option<RenderWindow.StandardChannels> Shown);

[SmartEnum<int>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class ChannelLayout {
    public static readonly ChannelLayout Scalar = new(sizeof(float), ComponentOrders.Irrelevant);
    public static readonly ChannelLayout Triple = new(3 * sizeof(float), ComponentOrders.RGB);
    public static readonly ChannelLayout Rgba = new(4 * sizeof(float), ComponentOrders.RGBA);

    public ComponentOrders Order { get; }
    public int Components => Key / sizeof(float);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ProgressFraction : System.Numerics.IMinMaxValue<ProgressFraction> {
    public static ProgressFraction MinValue { get; } = new(0f);
    public static ProgressFraction MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderWindows {
    // --- [CHANNELS]
    public static IO<WindowChannels> Channels(RenderWindow window) =>
        from requested in IO.lift(() => Known(window.GetRequestedRenderChannelsAsStandardChannels()))
        let members = Known(Enum.GetValues<RenderWindow.StandardChannels>())
        from available in IO.lift(() => toHashSet(members.Filter(channel => window.IsChannelAvailable(RenderWindow.ChannelId(channel)))))
        from shown in IO.lift(() => members.Find(channel => window.IsChannelShown(RenderWindow.ChannelId(channel))))
        select new WindowChannels(requested, available, shown);

    public static IO<Seq<RenderWindow.StandardChannels>> AddRequested(RenderWindow window) =>
        from channels in Channels(window)
        let missing = channels.Requested.Filter(channel => !channels.Available.Contains(channel)).Strict()
        from added in IO.lift(() => Callbacks.Each(missing, window.AddChannel, nameof(RenderWindow.AddChannel)))
        select missing;

    public static IO<Unit> AddWireframe(RenderWindow window, WireframeRegion region, Size size) =>
        IO.lift(() => Refused.Unless(window.AddWireframeChannel(region.Document, region.Viewport, size, region.Region), nameof(RenderWindow.AddWireframeChannel)));

    private static Seq<RenderWindow.StandardChannels> Known(RenderWindow.StandardChannels[] channels) =>
        toSeq(channels).Filter(static channel => channel is not RenderWindow.StandardChannels.None).Strict();

    // --- [PIXELS]
    public static IO<PixelFrame> Read(RenderWindow window, RenderWindow.StandardChannels channel) =>
        Opened(window, channel, static opened =>
            from extent in IO.lift(() => PixelExtent.Validated(opened.Width, opened.Height))
            from frame in IO.lift(() => Frame(opened, new Rectangle(System.Drawing.Point.Empty, new Size(extent.Width, extent.Height)), extent))
            select frame);

    public static Fin<PixelFrame> Frame(RenderWindow.Channel opened, Rectangle rect, PixelExtent extent) =>
        from read in (ChannelLayout.Validated(opened.PixelSize()).ToValidation(), PixelExtent.Validated(rect.Width, rect.Height).ToValidation())
            .Apply(static (layout, size) => (Layout: layout, Size: size)).As().ToFin()
        let stride = (long)read.Size.Width * read.Layout.Key
        let bounds = (Left: (long)rect.X, Top: (long)rect.Y, Right: (long)rect.X + read.Size.Width, Bottom: (long)rect.Y + read.Size.Height)
        from native in Refused.Unless(stride <= int.MaxValue && bounds.Left >= 0L && bounds.Top >= 0L && bounds.Right <= opened.Width && bounds.Bottom <= opened.Height,
            stride, nameof(RenderWindow.Channel.GetValues))
        select new PixelFrame(rect.Location, read.Size, extent, block => Fill(opened, rect, (int)native, read.Layout, block));

    public static IO<Unit> Blit(RenderWindow window, PixelFrame frame) =>
        Opened(window, RenderWindow.StandardChannels.RGBA, opened =>
            from inside in IO.lift(() => Refused.Unless(
                frame.Window is var bounds && bounds.Left >= 0L && bounds.Top >= 0L && bounds.Right <= opened.Width && bounds.Bottom <= opened.Height,
                nameof(RenderWindow.Channel.SetValues)))
            let rect = new Rectangle(frame.Origin, new Size(frame.Size.Width, frame.Size.Height))
            from written in IO.lift(() => opened.SetValues(rect, rect.Size, new PixelBuffer(frame.Address)))
            select rect)
            .Bind(rect => IO.lift(() => window.InvalidateArea(rect)));

    public static IO<T> Snapshot<T>(RenderWindow window, Func<Bitmap, IO<T>> body) =>
        DisposalOps.Found(window.GetBitmap, nameof(RenderWindow.GetBitmap), body);

    internal static void Widen(float[] block, int components, int stride, int pixels) {
        for (int pixel = pixels - 1; components < 4 && pixel >= 0; pixel--) {
            int source = pixel * stride;
            (block[4 * pixel], block[(4 * pixel) + 1], block[(4 * pixel) + 2], block[(4 * pixel) + 3]) =
                (block[source], block[source + int.Min(1, components - 1)], block[source + int.Min(2, components - 1)], 1f);
        }
    }

    private static IO<T> Opened<T>(RenderWindow window, RenderWindow.StandardChannels channel, Func<RenderWindow.Channel, IO<T>> body) =>
        DisposalOps.Found(() => window.OpenChannel(channel), nameof(RenderWindow.OpenChannel), body);

    private static void Fill(RenderWindow.Channel opened, Rectangle rect, int stride, ChannelLayout layout, float[] block) {
        opened.GetValues(rect, stride, layout.Order, ref block);
        Widen(block, layout.Components, layout.Components, rect.Width * rect.Height);
    }
}
