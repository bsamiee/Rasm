using System.Drawing;
using Rhino.Display;
using Rhino.Runtime;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DepthChannel {
    public static readonly DepthChannel Isocurves = new(static (capture, on) => capture.ShowIsocurves(on));
    public static readonly DepthChannel MeshWires = new(static (capture, on) => capture.ShowMeshWires(on));
    public static readonly DepthChannel Curves = new(static (capture, on) => capture.ShowCurves(on));
    public static readonly DepthChannel Points = new(static (capture, on) => capture.ShowPoints(on));
    public static readonly DepthChannel Text = new(static (capture, on) => capture.ShowText(on));
    public static readonly DepthChannel Annotations = new(static (capture, on) => capture.ShowAnnotations(on));
    public static readonly DepthChannel Lights = new(static (capture, on) => capture.ShowLights(on));

    [UseDelegateFromConstructor]
    internal partial void Show(ZBufferCapture capture, bool on);
}

public sealed record DepthRange(int Hits, float Near, float Far);

public sealed record DepthSample(System.Drawing.Point Pixel, float Depth, Point3d World);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DepthBuffer {
    private readonly ZBufferCapture capture;
    private readonly Rectangle frame;

    private DepthBuffer(ZBufferCapture capture, Rectangle frame) => (this.capture, this.frame) = (capture, frame);

    public IO<Option<DepthRange>> Range => IO.lift(() => Some(new DepthRange(capture.HitCount(), capture.MinZ(), capture.MaxZ())).Filter(static range => range.Hits > 0));

    public IO<Bitmap> Grayscale => IO.lift(() => Missing.Unless(capture.GrayscaleDib(), nameof(ZBufferCapture.GrayscaleDib)));

    public IO<Seq<DepthSample>> Samples(Seq<System.Drawing.Point> pixels) =>
        IO.lift(() => Callbacks.Each(pixels, frame.Contains, nameof(ZBufferCapture.WorldPointAt))
            .Map(_ => pixels.Map(pixel => new DepthSample(pixel, capture.ZValueAt(pixel.X, pixel.Y), capture.WorldPointAt(pixel.X, pixel.Y))).Strict()));

    public static IO<TValue> Capture<TValue>(RhinoViewport viewport, Option<Guid> mode, LanguageExt.HashSet<DepthChannel> shown, Func<DepthBuffer, IO<TValue>> body) =>
        (from spy in use(static () => new RiskyAction(nameof(ZBufferCapture)))
         from capture in use(() => new ZBufferCapture(viewport))
         from moded in IO.lift(() => mode.Iter(capture.SetDisplayMode))
         from channels in IO.lift(() => toSeq(DepthChannel.Items).Iter(channel => channel.Show(capture, shown.Contains(channel))))
         from value in body(new DepthBuffer(capture, new Rectangle(System.Drawing.Point.Empty, viewport.Size)))
         select value).Bracket();
}
