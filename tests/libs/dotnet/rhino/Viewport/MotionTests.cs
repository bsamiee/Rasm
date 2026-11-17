using CsCheck;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Viewport;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests.Viewport;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class MotionTests {

    // --- [POSES]

    [Fact]
    public void InterpolatingAPoseWithItselfHoldsItAtEveryProgress() =>
        Pose.Select(ModelGrid.Fraction).Sample(static (pose, t) =>
            Assert.True(Motions.Interpolated(pose, pose, t).ThrowIfFail().Matches(pose, Close)));

    [Fact]
    public void PosesTurnedApartByLessThanRoundingInterpolateBetweenThem() =>
        Pose.Select(Gen.Int[1, 1000], ModelGrid.Fraction).Sample(static (pose, nudge, t) => {
            Plane turned = pose.Frame;
            turned.Rotate(nudge * 1e-12, pose.Frame.YAxis);
            Assert.True(Motions.Interpolated(pose, pose with { Frame = turned }, t).ThrowIfFail().Matches(pose, Close));
        });

    [Fact]
    public void InterpolationStartsAtTheFirstPoseAndEndsAtTheSecond() =>
        Pose.Select(Pose).Sample(static (start, end) => {
            Assert.True(Motions.Interpolated(start, end, 0d).ThrowIfFail().Matches(start, Close));
            Assert.True(Motions.Interpolated(start, end, 1d).ThrowIfFail().Matches(end, Close));
        });

    [Fact]
    public void InterpolatedTargetIsTheLinearBlendOfBothTargets() =>
        Pose.Select(Pose, ModelGrid.Fraction).Sample(static (start, end, t) =>
            Assert.InRange(Motions.Interpolated(start, end, t).ThrowIfFail().Target.DistanceTo(start.Target + (t * (end.Target - start.Target))), 0d, Close.Absolute));

    [Fact]
    public void PerspectiveLensAngleBlendsLinearly() =>
        Pose.Select(Pose, Perspective, Perspective, ModelGrid.Fraction).Sample(static (start, end, near, far, t) =>
            Assert.Equal(
                double.Lerp(near.Angle.Radians.ToDouble(), far.Angle.Radians.ToDouble(), t),
                Assert.IsType<CameraLens.Perspective>(Motions.Interpolated(start with { Lens = near }, end with { Lens = far }, t).ThrowIfFail().Lens).Angle.Radians.ToDouble(),
                Close.Angle));

    // --- [GENERATORS]

    private static readonly Tolerances Close = Tolerances.Create(1e-6, 0.01, 1e-6);

    private static readonly Gen<UnitsNet.Angle> Opening = Gen.Int[20_000, 60_000].Select(static steps => UnitsNet.Angle.FromRadians(Quantities.Exact(steps * RhinoMath.DefaultAngleTolerance / 1000d)));

    private static readonly Gen<CameraLens.Perspective> Perspective = Opening.Select(static angle => new CameraLens.Perspective(angle));

    private static readonly Gen<CameraLens> Lens = Gen.OneOf(
        Perspective.Select(static lens => (CameraLens)lens),
        ModelGrid.Size.Select(static extent => (CameraLens)new CameraLens.Parallel(extent)),
        Opening.Select(ModelGrid.Turn, static (angle, turn) => (CameraLens)new CameraLens.TwoPoint(angle, turn.Axis / turn.Axis.Length)));

    private static readonly Gen<CameraPose> Pose =
        from origin in ModelGrid.Point
        from turn in ModelGrid.Turn
        from distance in ModelGrid.Size
        from lens in Lens
        let rotation = Transform.Rotation(turn.Angle, turn.Axis, Point3d.Origin)
        let frame = new Plane(origin, rotation * Vector3d.XAxis, rotation * Vector3d.YAxis)
        select new CameraPose(frame, frame.Origin - (distance * frame.ZAxis), lens);
}
