using System.Drawing;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderRegion(double Left, double Top, double Right, double Bottom) {
    public static RenderRegion Of(Rectangle area, Size extent) =>
        new((double)area.Left / extent.Width, (double)area.Top / extent.Height, (double)area.Right / extent.Width, (double)area.Bottom / extent.Height);

    public Rectangle Pixels(PixelExtent frame) =>
        Rectangle.FromLTRB((int)(Left * frame.Width), (int)(Top * frame.Height), (int)(Right * frame.Width), (int)(Bottom * frame.Height));
}

[SmartEnum<string>]
public sealed partial class RegionFrame {
    public static readonly RegionFrame Full = new("full", static (_, _) => Option<Rectangle>.None);
    public static readonly RegionFrame Action = new("action", Framing.Action);
    public static readonly RegionFrame Title = new("title", Framing.Title);

    [UseDelegateFromConstructor]
    public partial Option<Rectangle> Framed(SafeFrame frame, Size extent);

    public IO<Option<RenderRegion>> Read(RhinoDoc doc, Size extent) =>
        RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), scope =>
            (from frame in use(() => scope.Settings.SafeFrame) select Framed(frame, extent).Map(area => RenderRegion.Of(area, extent))).Bracket());
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
            return DarkroomFault.Unless(view.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far), DarkroomReason.Invalid, view.Id)
                .Bind(_ => ((right - left) * Pad.Width / Frame.Width, (top - bottom) * Pad.Height / Frame.Height) switch {
                    var (x, y) => DarkroomFault.Unless(view.SetFrustum(left - x, right + x, bottom - y, top + y, near, far), DarkroomReason.Invalid, view.Id),
                });
        });

    private static int Grown(int side, double overscan) => int.CreateSaturating((2d * Math.Round(overscan * side, MidpointRounding.ToEven)) + side);
}

public sealed record RenderOverride(RenderSettings Settings, Option<float> Clay) {
    // --- [SCOPE]
    public static IO<RenderSettings> Current(RhinoDoc doc) => RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(scope.Settings.Duplicate));

    public IO<T> Around<T>(RhinoDoc doc, IO<T> launch) =>
        Current(doc).Bind(prior =>
            (from applied in RenderSettingsSources.Replace(doc, Settings)
             from clay in Clay.Traverse(albedo => ClayMeshProvider.Register(doc, albedo)).As()
             from launched in launch
             select launched).Bracket().Finally(RenderSettingsSources.Replace(doc, prior)));
}
