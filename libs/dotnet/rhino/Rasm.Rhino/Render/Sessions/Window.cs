using System.Drawing;
using CommunityToolkit.HighPerformance;
using Rasm.Rhino.Render.Effects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WindowChannels(
    Seq<RenderWindow.StandardChannels> Requested,
    HashSet<RenderWindow.StandardChannels> Available,
    Option<RenderWindow.StandardChannels> Shown);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderWindows {
    // --- [CHANNELS]
    public static IO<WindowChannels> ChannelState(RenderWindow window) =>
        from requested in IO.lift(() => Known(window.GetRequestedRenderChannelsAsStandardChannels()))
        let members = Known(Enum.GetValues<RenderWindow.StandardChannels>())
        from available in IO.lift(() => toHashSet(members.Filter(channel => window.IsChannelAvailable(RenderWindow.ChannelId(channel)))))
        from shown in IO.lift(() => members.Find(channel => window.IsChannelShown(RenderWindow.ChannelId(channel))))
        select new WindowChannels(requested, available, shown);

    public static IO<Seq<RenderWindow.StandardChannels>> AddRequested(RenderWindow window) =>
        from channels in ChannelState(window)
        let missing = channels.Requested.Filter(channel => !channels.Available.Contains(channel)).Strict()
        from added in IO.lift(() => Products.Each(missing, (channel, index) => RhinoFault.Unless(window.AddChannel(channel), RhinoReason.Declined, channel, index)))
        select missing;

    private static Seq<RenderWindow.StandardChannels> Known(RenderWindow.StandardChannels[] channels) =>
        toSeq(channels).Filter(static channel => channel is not RenderWindow.StandardChannels.None).Strict();

    // --- [PIXELS]
    public static IO<Memory2D<float>> Read(RenderWindow window, RenderWindow.StandardChannels channel) =>
        Opened(window, channel, static opened => IO.lift(() => Channels.Read(opened, new Rectangle(0, 0, opened.Width, opened.Height))
            .Map(native => Channels.Widened(native.Span, opened.Width))));

    public static IO<Unit> Blit(RenderWindow window, Rectangle rect, ReadOnlyMemory2D<float> rgba) =>
        from written in Opened(window, RenderWindow.StandardChannels.RGBA, opened => IO.lift(() => Channels.Put(opened, rect, rgba)))
        from invalidated in IO.lift(() => window.InvalidateArea(rect))
        select unit;

    private static IO<T> Opened<T>(RenderWindow window, RenderWindow.StandardChannels channel, Func<RenderWindow.Channel, IO<T>> body) =>
        Products.Of(() => window.OpenChannel(channel)).Bind(body).Bracket();
}
