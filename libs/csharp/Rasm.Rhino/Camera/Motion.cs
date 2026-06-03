using System.Runtime.InteropServices;

namespace Rasm.Rhino.Camera;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MotionProfile(
    int FrameCount,
    UI.Easing Easing,
    Option<double> LensStart = default,
    Option<double> LensEnd = default) {
    private static readonly Op OfKey = Op.Of(name: nameof(Of));

    public static Fin<MotionProfile> Of(int frameCount, UI.Easing easing, Option<double> lensStart = default, Option<double> lensEnd = default) =>
        from _frames in guard(frameCount >= 2, OfKey.InvalidInput()).ToFin()
        from _lens in guard(
            lensStart.Map(static value => RhinoMath.IsValidDouble(x: value)).IfNone(noneValue: true) && lensEnd.Map(static value => RhinoMath.IsValidDouble(x: value)).IfNone(noneValue: true),
            OfKey.InvalidInput()).ToFin()
        select new MotionProfile(FrameCount: frameCount, Easing: easing, LensStart: lensStart, LensEnd: lensEnd);
}

// One sampled step of a camera move: the eased pose plus its interpolated lens. `Edits` lifts the
// step onto the apply rail so a caller pumps it via `CameraOps.Change(frame.Edits)` at its own cadence.
[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionFrame(CameraFrame Frame, double Lens) {
    public Seq<CameraEdit> Edits =>
        Seq<CameraEdit>(new CameraEdit.Frame(Value: Frame), new CameraEdit.Lens(Millimeters: Lens));
}
