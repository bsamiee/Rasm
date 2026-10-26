using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderRegion(double Left, double Top, double Right, double Bottom) {
    public Rectangle Pixels(PixelExtent frame) =>
        Rectangle.FromLTRB((int)(Left * frame.Width), (int)(Top * frame.Height), (int)(Right * frame.Width), (int)(Bottom * frame.Height));
}

public sealed class OverscanFrame {
    private OverscanFrame(PixelExtent frame, PixelExtent padded) => (Frame, Padded) = (frame, padded);

    public PixelExtent Frame { get; }
    public PixelExtent Padded { get; }
    public Size Pad => new((Padded.Width - Frame.Width) / 2, (Padded.Height - Frame.Height) / 2);
    public Rectangle DataWindow => new(-Pad.Width, -Pad.Height, Padded.Width, Padded.Height);

    public static Fin<OverscanFrame> Around(PixelExtent frame, double overscan) =>
        PixelExtent.Validated(Grown(frame.Width, overscan), Grown(frame.Height, overscan)).Map(padded => new OverscanFrame(frame, padded));

    public IO<Unit> Widen(ViewportInfo view) =>
        IO.lift(() => {
            view.FrustumAspect = (double)Frame.Width / Frame.Height;
            return Refused.Unless(view.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far), nameof(ViewportInfo.GetFrustum))
                .Bind(_ => ((right - left) * Pad.Width / Frame.Width, (top - bottom) * Pad.Height / Frame.Height) switch {
                    var (x, y) => Refused.Unless(view.SetFrustum(left - x, right + x, bottom - y, top + y, near, far), nameof(ViewportInfo.SetFrustum)),
                });
        });

    private static int Grown(int side, double overscan) => int.CreateSaturating((2d * Math.Round(overscan * side, MidpointRounding.ToEven)) + side);
}

public sealed record RenderOverride(RenderSettings Settings, Option<float> Clay) {
    // --- [SCOPE]
    public IO<T> Around<T>(RhinoDoc doc, IPlugInViews views, IO<T> launch) =>
        DisposalOps.Within(
            RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(scope.Settings.Duplicate)),
            _ => DisposalOps.Within<IDisposable, T>(retain =>
                    from applied in RenderSettingsSources.Edit(doc, Settings)
                    from clay in Clay.Traverse(albedo => ClayMeshProvider.Register(views, doc, albedo, retain)).As()
                    from launched in launch
                    select launched,
                DisposalOps.Release),
            prior => RenderSettingsSources.Edit(doc, prior));
}
