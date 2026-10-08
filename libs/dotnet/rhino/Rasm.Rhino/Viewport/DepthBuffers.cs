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

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
internal sealed partial class DepthPixel {
    public System.Drawing.Point Pixel { get; }

    public Size Extent { get; }

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref System.Drawing.Point pixel, ref Size extent) =>
        validationError = new Rectangle(System.Drawing.Point.Empty, extent).Contains(pixel) ? null : new InvalidRhinoValue();
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DepthBuffer(ZBufferCapture capture, Size extent) {
    public Size Extent => extent;

    public IO<Option<DepthRange>> Range =>
        use(static () => new RiskyAction(nameof(ZBufferCapture.HitCount)))
            .Bind(_ => IO.lift(() => Some(new DepthRange(capture.HitCount(), capture.MinZ(), capture.MaxZ())).Filter(static range => range.Hits > 0)))
            .Bracket();

    public IO<Seq<DepthSample>> Samples(Seq<System.Drawing.Point> pixels) =>
        from inside in IO.lift(Callbacks.Each(pixels, (pixel, _) => DepthPixel.Validate(pixel, extent, out DepthPixel? item) is { } error ? Fin.Fail<DepthPixel>(error) : item!))
        from samples in use(static () => new RiskyAction(nameof(ZBufferCapture.WorldPointAt)))
            .Bind(_ => IO.lift(() => inside
                .Map(pixel => new DepthSample(pixel.Pixel, capture.ZValueAt(pixel.Pixel.X, pixel.Pixel.Y), capture.WorldPointAt(pixel.Pixel.X, pixel.Pixel.Y)))
                .Strict()))
            .Bracket()
        select samples;

    public IO<Bitmap> Grayscale =>
        use(static () => new RiskyAction(nameof(ZBufferCapture.GrayscaleDib)))
            .Bind(_ => IO.lift(() => Missing.Unless(capture.GrayscaleDib(), nameof(ZBufferCapture.GrayscaleDib))))
            .Bracket();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DepthBuffers {
    public static IO<TValue> Capture<TValue>(RhinoViewport viewport, Option<Guid> mode, LanguageExt.HashSet<DepthChannel> shown, Func<DepthBuffer, IO<TValue>> body) =>
        (from capture in use(() => new ZBufferCapture(viewport))
         from configured in IO.lift(() => {
             _ = mode.Iter(capture.SetDisplayMode);
             _ = toSeq(DepthChannel.Items).Iter(channel => channel.Show(capture, shown.Contains(channel)));
         })
         from extent in IO.lift(() => viewport.Size)
         from value in body(new DepthBuffer(capture, extent))
         select value).Bracket();
}
