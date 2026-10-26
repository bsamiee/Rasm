using System.Drawing;
using Rasm.Rhino.Render.Effects;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WireframeRegion(RhinoDoc Document, ViewportInfo Viewport, Rectangle Region);

[Union]
public abstract partial record RenderWindowSource {
    public abstract IO<T> Open<T>(Func<RenderWindow, IO<T>> body);

    public sealed record Detached(Size Size, ViewInfo View) : RenderWindowSource {
        public override IO<T> Open<T>(Func<RenderWindow, IO<T>> body) =>
            DisposalOps.Found(() => RenderWindow.Create(Size), nameof(RenderWindow.Create),
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
        from added in IO.lift(() => Callbacks.Each(missing, window.AddChannel, nameof(RenderWindow.AddChannel)))
        select missing;

    public static IO<Unit> AddWireframe(RenderWindow window, WireframeRegion region, Size size) =>
        IO.lift(() => Refused.Unless(window.AddWireframeChannel(region.Document, region.Viewport, size, region.Region), nameof(RenderWindow.AddWireframeChannel)));

    private static Seq<RenderWindow.StandardChannels> Known(RenderWindow.StandardChannels[] channels) =>
        toSeq(channels).Filter(static channel => channel is not RenderWindow.StandardChannels.None).Strict();

    // --- [PIXELS]
    public static IO<(Size Size, float[] Pixels)> Read(RenderWindow window, RenderWindow.StandardChannels channel) =>
        Opened(window, channel, static opened => IO.lift(() =>
            new Rectangle(0, 0, opened.Width, opened.Height) switch {
                var whole => Channels.Read(opened, whole).Map(pixels => (whole.Size, pixels)),
            }));

    public static IO<Unit> Blit(RenderWindow window, Rectangle rect, ReadOnlyMemory<float> rgba) =>
        from written in Opened(window, RenderWindow.StandardChannels.RGBA, opened => IO.lift(() => Channels.Put(opened, rect, rgba)))
        from invalidated in IO.lift(() => window.InvalidateArea(rect))
        select unit;

    private static IO<T> Opened<T>(RenderWindow window, RenderWindow.StandardChannels channel, Func<RenderWindow.Channel, IO<T>> body) =>
        DisposalOps.Found(() => window.OpenChannel(channel), nameof(RenderWindow.OpenChannel), body);
}
