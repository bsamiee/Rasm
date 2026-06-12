using System.Runtime.InteropServices;

namespace Rasm.Rhino.Camera;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MotionProfile(
    int FrameCount,
    UI.Easing Easing,
    Option<double> LensStart = default,
    Option<double> LensEnd = default,
    Seq<double> Samples = default) {
    private static readonly Op OfKey = Op.Of(name: nameof(Of));

    public static Fin<MotionProfile> Of(int frameCount, UI.Easing easing, Option<double> lensStart = default, Option<double> lensEnd = default, Seq<double> samples = default) =>
        from _frames in guard(frameCount >= 2, OfKey.InvalidInput()).ToFin()
        from _lens in guard(
            lensStart.Map(static value => RhinoMath.IsValidDouble(x: value)).IfNone(noneValue: true) && lensEnd.Map(static value => RhinoMath.IsValidDouble(x: value)).IfNone(noneValue: true),
            OfKey.InvalidInput()).ToFin()
        from _samples in guard(samples.IsEmpty || (samples.Count >= 2 && samples.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= 1.0)), OfKey.InvalidInput()).ToFin()
        select new MotionProfile(FrameCount: frameCount, Easing: easing, LensStart: lensStart, LensEnd: lensEnd, Samples: samples);

    internal Fin<Seq<double>> Steps(Op op) {
        MotionProfile self = this;
        Seq<double> source = self.Samples.IsEmpty
            ? toSeq(Enumerable.Range(start: 0, count: self.FrameCount)).Map(i => (double)i / (self.FrameCount - 1))
            : self.Samples;
        return from _valid in guard(source.Count >= 2 && source.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= 1.0), op.InvalidInput()).ToFin()
               select source.Map(self.Easing.Apply);
    }
}

// One sampled step of a camera move: the eased pose plus its interpolated lens. `Edits` lifts the
// step onto the apply rail so a caller pumps it via `CameraOps.Change(frame.Edits)` at its own cadence.
[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionFrame(CameraFrame Frame, double Lens) {
    public Seq<CameraEdit> Edits =>
        Seq<CameraEdit>(new CameraEdit.Frame(Value: Frame), new CameraEdit.Lens(Millimeters: Lens));
}
