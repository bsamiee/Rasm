using System.Numerics;
using Eto.Drawing;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Rhino.Render.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class TraceChannel {
    public static readonly TraceChannel Luma = new(
        new MarkColor.Series(PaintSlot.ControlText, DashStyles.DashDot), CurveChannel.Master, Option<Lens<Vector3, float>>.None, static traces => traces.Luma);
    public static readonly TraceChannel Red = new(
        new MarkColor.Series(Color.FromArgb(0xFF, 0x00, 0x00), DashStyles.Solid), CurveChannel.Red,
        Some(Lens<Vector3, float>.New(static rgb => rgb.X, static x => rgb => rgb with { X = x })), static traces => traces.Red);
    public static readonly TraceChannel Green = new(
        new MarkColor.Series(Color.FromArgb(0x00, 0xFF, 0x00), DashStyles.Dash), CurveChannel.Green,
        Some(Lens<Vector3, float>.New(static rgb => rgb.Y, static y => rgb => rgb with { Y = y })), static traces => traces.Green);
    public static readonly TraceChannel Blue = new(
        new MarkColor.Series(Color.FromArgb(0x26, 0x63, 0xFF), DashStyles.Dot), CurveChannel.Blue,
        Some(Lens<Vector3, float>.New(static rgb => rgb.Z, static z => rgb => rgb with { Z = z })), static traces => traces.Blue);

    public MarkColor Mark { get; }
    public CurveChannel Curve { get; }
    public Option<Lens<Vector3, float>> Lane { get; }

    [UseDelegateFromConstructor]
    public partial BinGrid Bins(Traces traces);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ImagePlots {
    public static AxisRange Range(CurveAxis axis) => AxisRange.Create(float.CreateSaturating(axis.Span.Low), float.CreateSaturating(axis.Span.High));

    public static Axis Placed(CurveAxis axis) => axis.Periodic ? new Axis.Periodic(Range(axis)) : new Axis.Linear(Range(axis));

    public static Vector4 HueRamp(float hue) => new Hsy(hue, 1f, Exposure.MiddleGray).ToRgb(1f);

    public static Fin<PixelExtent> Device(PlotPlane plane, float scale) =>
        PixelExtent.Validated((int)MathF.Ceiling(plane.Plot.Width * scale), (int)MathF.Ceiling(plane.Plot.Height * scale));

    public static Bitmap Pixels(PixelExtent extent, IEnumerable<int> argb) => Plots.Pixels(new Size(extent.Width, extent.Height), argb);
}
