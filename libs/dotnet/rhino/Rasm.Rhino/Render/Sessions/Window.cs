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
    public abstract IO<RenderWindow> Open();

    public sealed record Detached(PixelExtent Extent, ViewInfo View) : RenderWindowSource {
        public override IO<RenderWindow> Open() =>
            from window in IO.lift(() => Missing.Unless(RenderWindow.Create(new Size(Extent.Width, Extent.Height)), nameof(RenderWindow.Create)))
            from viewed in IO.lift(() => window.SetView(View))
            select window;
    }

    public sealed record Rendering(Guid SessionId) : RenderWindowSource {
        public override IO<RenderWindow> Open() =>
            IO.lift(() => Missing.Unless(RenderWindow.FromSessionId(SessionId), nameof(RenderWindow.FromSessionId)));
    }
}

public sealed record WindowChannels(
    Seq<RenderWindow.StandardChannels> Requested,
    LanguageExt.HashSet<RenderWindow.StandardChannels> Available,
    Option<RenderWindow.StandardChannels> Shown);

[SmartEnum<int>(SkipIParsable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
internal sealed partial class ChannelLayout {
    public static readonly ChannelLayout Scalar = new(sizeof(float), ComponentOrders.Irrelevant);
    public static readonly ChannelLayout Triple = new(3 * sizeof(float), ComponentOrders.RGB);
    public static readonly ChannelLayout Rgba = new(4 * sizeof(float), ComponentOrders.RGBA);

    public ComponentOrders Order { get; }
    public int Components => Key / sizeof(float);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ProgressFraction : System.Numerics.IMinMaxValue<ProgressFraction> {
    public static ProgressFraction MinValue { get; } = new(0f);
    public static ProgressFraction MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
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
        toSeq(channels).Filter(static channel => channel != RenderWindow.StandardChannels.None).Strict();

    // --- [PIXELS]
    public static IO<PixelFrame> Read(RenderWindow window, RenderWindow.StandardChannels channel) =>
        (from opened in use(Open(window, channel))
         from extent in IO.lift(() => Extent(opened.Width, opened.Height))
         from frame in IO.lift(() => Frame(opened, new Rectangle(0, 0, extent.Width, extent.Height), extent))
         select frame).Bracket();

    public static Fin<PixelFrame> Frame(RenderWindow.Channel opened, Rectangle rect, PixelExtent extent) =>
        (Conversions.Validated<ChannelLayout, int, InvalidRhinoValue>(opened.PixelSize()).ToValidation(), Extent(rect.Width, rect.Height).ToValidation())
            .Apply((layout, size) => new PixelFrame(rect.Location, size, extent, block => Fill(opened, rect, layout, block)))
            .As()
            .ToFin();

    public static IO<Unit> Blit(RenderWindow window, PixelFrame frame) =>
        (from opened in use(Open(window, RenderWindow.StandardChannels.RGBA))
         from inside in IO.lift(() => Refused.Unless(new Rectangle(0, 0, opened.Width, opened.Height).Contains(frame.Window), nameof(RenderWindow.Channel.SetValues)))
         from written in IO.lift(() => opened.SetValues(frame.Window, frame.Window.Size, new PixelBuffer(frame.Address)))
         select written)
            .Bracket()
            .Bind(_ => IO.lift(() => window.InvalidateArea(frame.Window)));

    public static IO<Bitmap> Snapshot(RenderWindow window) =>
        IO.lift(() => Missing.Unless(window.GetBitmap(), nameof(RenderWindow.GetBitmap)));

    internal static Fin<PixelExtent> Extent(int width, int height) =>
        PixelExtent.Validate(width, height, out PixelExtent extent) is { } error ? error : extent;

    private static IO<RenderWindow.Channel> Open(RenderWindow window, RenderWindow.StandardChannels channel) =>
        IO.lift(() => Missing.Unless(window.OpenChannel(channel), nameof(RenderWindow.OpenChannel)));

    private static void Fill(RenderWindow.Channel opened, Rectangle rect, ChannelLayout layout, float[] block) {
        opened.GetValues(rect, rect.Width * layout.Key, layout.Order, ref block);
        for (int pixel = (rect.Width * rect.Height) - 1; layout.Components < 4 && pixel >= 0; pixel--) {
            int source = pixel * layout.Components;
            (block[4 * pixel], block[(4 * pixel) + 1], block[(4 * pixel) + 2], block[(4 * pixel) + 3]) =
                (block[source], block[source + int.Min(1, layout.Components - 1)], block[source + int.Min(2, layout.Components - 1)], 1f);
        }
    }
}
