using System.Drawing;
using Rhino.Display;
using Rhino.Runtime;

namespace Rasm.Rhino.Viewport;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ZBuffer {
    private readonly ZBufferCapture capture;
    private readonly Rectangle frame;

    private ZBuffer(ZBufferCapture capture, Rectangle frame) => (this.capture, this.frame) = (capture, frame);

    public IO<Option<(int Hits, float Near, float Far)>> Range =>
        IO.lift(() => Conversions.Present(capture.HitCount() > 0, (Hits: capture.HitCount(), Near: capture.MinZ(), Far: capture.MaxZ())));

    public IO<Bitmap> Grayscale => Products.Of(capture.GrayscaleDib);

    public IO<Seq<(System.Drawing.Point Pixel, float Depth, Point3d World)>> Samples(Seq<System.Drawing.Point> pixels) =>
        IO.lift(() => Products.Each(pixels, (pixel, index) => RhinoFault.Unless(frame.Contains(pixel), RhinoReason.Outside, pixel, index))
            .Map(_ => pixels.Map(pixel => (Pixel: pixel, Depth: capture.ZValueAt(pixel.X, pixel.Y), World: capture.WorldPointAt(pixel.X, pixel.Y))).Strict()));

    public static IO<TValue> Capture<TValue>(RhinoViewport viewport, Action<ZBufferCapture> setup, Func<ZBuffer, IO<TValue>> body) =>
        (from risky in use(static () => new RiskyAction(nameof(ZBufferCapture)))
         from capture in use(() => new ZBufferCapture(viewport))
         from staged in IO.lift(() => setup(capture))
         from value in body(new ZBuffer(capture, new Rectangle(System.Drawing.Point.Empty, viewport.Size)))
         select value).Bracket();
}
