using System.Drawing;
using Rhino.Display;
using Rhino.Runtime;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ZBufferChannel {
    public static readonly ZBufferChannel Isocurves = new(static (capture, on) => capture.ShowIsocurves(on));
    public static readonly ZBufferChannel MeshWires = new(static (capture, on) => capture.ShowMeshWires(on));
    public static readonly ZBufferChannel Curves = new(static (capture, on) => capture.ShowCurves(on));
    public static readonly ZBufferChannel Points = new(static (capture, on) => capture.ShowPoints(on));
    public static readonly ZBufferChannel Text = new(static (capture, on) => capture.ShowText(on));
    public static readonly ZBufferChannel Annotations = new(static (capture, on) => capture.ShowAnnotations(on));
    public static readonly ZBufferChannel Lights = new(static (capture, on) => capture.ShowLights(on));

    [UseDelegateFromConstructor]
    internal partial void Show(ZBufferCapture capture, bool on);
}

public sealed record ZBufferRange(int Hits, float Near, float Far);

public sealed record ZBufferSample(System.Drawing.Point Pixel, float Depth, Point3d World);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ZBuffer {
    private readonly ZBufferCapture capture;
    private readonly Rectangle frame;

    private ZBuffer(ZBufferCapture capture, Rectangle frame) => (this.capture, this.frame) = (capture, frame);

    public IO<Option<ZBufferRange>> Range => IO.lift(() => Some(new ZBufferRange(capture.HitCount(), capture.MinZ(), capture.MaxZ())).Filter(static range => range.Hits > 0));

    public IO<Bitmap> Grayscale => IO.lift(() => Refused.UnlessFound(capture.GrayscaleDib(), nameof(ZBufferCapture.GrayscaleDib)));

    public IO<Seq<ZBufferSample>> Samples(Seq<System.Drawing.Point> pixels) =>
        IO.lift(() => Callbacks.Each(pixels, frame.Contains, nameof(ZBufferCapture.WorldPointAt))
            .Map(_ => pixels.Map(pixel => new ZBufferSample(pixel, capture.ZValueAt(pixel.X, pixel.Y), capture.WorldPointAt(pixel.X, pixel.Y))).Strict()));

    public static IO<TValue> Capture<TValue>(RhinoViewport viewport, Option<Guid> mode, LanguageExt.HashSet<ZBufferChannel> shown, Func<ZBuffer, IO<TValue>> body) =>
        DisposalOps.Within(static () => new RiskyAction(nameof(ZBufferCapture)), _ => DisposalOps.Within(() => new ZBufferCapture(viewport), capture =>
            from moded in IO.lift(() => mode.Iter(capture.SetDisplayMode))
            from channels in IO.lift(() => toSeq(ZBufferChannel.Items).Iter(channel => channel.Show(capture, shown.Contains(channel))))
            from value in body(new ZBuffer(capture, new Rectangle(System.Drawing.Point.Empty, viewport.Size)))
            select value));
}
