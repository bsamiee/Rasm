using System.Collections.Concurrent;
using System.Drawing;
using CsCheck;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Viewport;
using Rasm.Testing;
using Rhino;
using Rhino.Display;
using Xunit;

namespace Rasm.Rhino.Tests.Viewport;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class MotionTests(Fixtures fixtures) {

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

    // --- [DRIVING]

    [Theory(Skip = "configuration Headless runs Rhino without views", SkipType = typeof(RhinoProcessTests), SkipUnless = nameof(RhinoProcessTests.Interfaced))]
    [InlineData(4, new[] { 0, 1, 2, 6 }, new[] { 0d, 0.25, 0.5, 1d })]
    [InlineData(4, new[] { 0, 4 }, new[] { 0d, 1d })]
    [InlineData(0, new[] { 3 }, new[] { 1d })]
    public async Task MoveHandsItsCurveEachTicksShareOfTheSpanAndEndsWhereTheCurveIsAtOne(int span, int[] ticks, double[] asked) {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        ConcurrentQueue<double> shares = new();
        Vector3d offset = new(10, -4, 3);
        (CameraPose opening, CameraPose ended) = await RhinoHost.RunAsync((
            from view in IO.lift(() => doc.Views.Add(nameof(MotionTests), DefinedViewportProjection.Perspective, new Rectangle(0, 0, 300, 200), floating: true))
            from start in Cameras.Read(view.MainViewport)
            let move = new CameraMove(new ViewportSet.One(new ViewportTarget.Serial(view.RuntimeSerialNumber)), Moved(start, offset), Duration.FromSeconds(span), share => {
                shares.Enqueue(share);
                return share * share / 2d;
            }, new RedrawPolicy.Silent())
            from handle in Motions.Drive(doc, move, sink => toSeq(ticks).TraverseM(tick => sink.Post(Duration.FromSeconds(tick))).As().Map(static _ => unit))
            from completed in handle.Completed
            from shown in Cameras.Read(view.MainViewport)
            select (start, shown)).OnUiThread());
        Assert.Equal(asked, shares);
        Assert.True(ended.Matches(Moved(opening, offset / 2d), Close), $"{ended} against {Moved(opening, offset / 2d)}");
    }

    private static CameraPose Moved(CameraPose pose, Vector3d offset) {
        Plane frame = pose.Frame;
        frame.Translate(offset);
        return pose with { Frame = frame, Target = pose.Target + offset };
    }

    // --- [LENSES]

    [Fact]
    public void PerspectiveLensAngleBlendsLinearly() =>
        Pose.Select(Pose, Perspective, Perspective, ModelGrid.Fraction).Sample(static (start, end, near, far, t) =>
            Assert.Equal(
                double.Lerp(near.Angle.Radians.ToDouble(), far.Angle.Radians.ToDouble(), t),
                Assert.IsType<CameraLens.Perspective>(LensAt(start, end, near, far, t)).Angle.Radians.ToDouble(),
                Close.Angle));

    [Fact]
    public void LensOfTheSameKindMovesNoFurtherPerStepThanItsShareOfTheWholeChange() =>
        Pose.Select(Pose, Alike, Step).Sample(static (start, end, lenses, step) =>
            Assert.InRange(
                Apart(LensAt(start, end, lenses.Near, lenses.Far, step.Before), LensAt(start, end, lenses.Near, lenses.Far, step.After)),
                0d,
                ((step.After - step.Before) * Apart(lenses.Near, lenses.Far)) + Close.Angle));

    [Fact]
    public void TwoPointUpStaysUnitAndTurnsSteadilyTowardTheEndUp() =>
        Pose.Select(Pose, TwoPoint, TwoPoint, Step).Sample(static (start, end, near, far, step) => {
            CameraLens.TwoPoint before = Assert.IsType<CameraLens.TwoPoint>(LensAt(start, end, near, far, step.Before));
            CameraLens.TwoPoint after = Assert.IsType<CameraLens.TwoPoint>(LensAt(start, end, near, far, step.After));
            Assert.Equal(1d, after.Up.Length, Close.Angle);
            Assert.InRange(Vector3d.VectorAngle(after.Up, far.Up), 0d, Vector3d.VectorAngle(before.Up, far.Up) + Close.Angle);
        });

    [Fact]
    public void TwoPointUpTurningToItsOppositeStaysUnitAndClosesTheHalfTurnEvenly() =>
        Pose.Select(Pose, TwoPoint, ModelGrid.Fraction).Sample(static (start, end, near, t) => {
            CameraLens.TwoPoint shown = Assert.IsType<CameraLens.TwoPoint>(LensAt(start, end, near, near with { Up = -near.Up }, t));
            Assert.Equal(1d, shown.Up.Length, Close.Angle);
            Assert.Equal((1d - t) * Math.PI, Vector3d.VectorAngle(shown.Up, -near.Up), Close.Angle);
        });

    [Fact]
    public void LensOfAnotherKindTakesOverOnceTheMoveLeavesItsStart() =>
        Pose.Select(Pose, Unlike, Gen.Int[1, 1000]).Sample(static (start, end, lenses, step) =>
            Assert.Equal(lenses.Far, LensAt(start, end, lenses.Near, lenses.Far, step / 1000d)));

    private static CameraLens LensAt(CameraPose start, CameraPose end, CameraLens near, CameraLens far, double t) =>
        Motions.Interpolated(start with { Lens = near }, end with { Lens = far }, t).ThrowIfFail().Lens;

    private static double Apart(CameraLens near, CameraLens far) =>
        near.Switch(
            far,
            parallel: static (other, a) => Math.Abs(a.Extent - Assert.IsType<CameraLens.Parallel>(other).Extent),
            perspective: static (other, a) => Math.Abs((a.Angle - Assert.IsType<CameraLens.Perspective>(other).Angle).Radians.ToDouble()),
            twoPoint: static (other, a) => Assert.IsType<CameraLens.TwoPoint>(other) switch {
                var b => Math.Abs((a.Angle - b.Angle).Radians.ToDouble()) + Vector3d.VectorAngle(a.Up, b.Up),
            });

    // --- [GENERATORS]

    private static readonly Tolerances Close = Tolerances.Create(1e-6, 0.01, 1e-6);

    private static readonly Gen<UnitsNet.Angle> Opening = Gen.Int[20_000, 60_000].Select(static steps => UnitsNet.Angle.FromRadians(Quantities.Exact(steps * RhinoMath.DefaultAngleTolerance / 1000d)));

    private static readonly Gen<CameraLens.Parallel> Parallel = ModelGrid.Size.Select(static extent => new CameraLens.Parallel(extent));

    private static readonly Gen<CameraLens.Perspective> Perspective = Opening.Select(static angle => new CameraLens.Perspective(angle));

    private static readonly Gen<CameraLens.TwoPoint> TwoPoint = Opening.Select(ModelGrid.Turn, static (angle, turn) => new CameraLens.TwoPoint(angle, turn.Axis / turn.Axis.Length));

    private static readonly Gen<CameraLens> Lens = Gen.OneOf(
        Parallel.Select(static lens => (CameraLens)lens),
        Perspective.Select(static lens => (CameraLens)lens),
        TwoPoint.Select(static lens => (CameraLens)lens));

    private static readonly Gen<(CameraLens Near, CameraLens Far)> Alike = Gen.OneOf(
        Parallel.Select(Parallel, static (near, far) => ((CameraLens)near, (CameraLens)far)),
        Perspective.Select(Perspective, static (near, far) => ((CameraLens)near, (CameraLens)far)),
        TwoPoint.Select(TwoPoint, static (near, far) => ((CameraLens)near, (CameraLens)far)));

    private static readonly Gen<(CameraLens Near, CameraLens Far)> Unlike = Lens.Select(Lens).Where(static lenses => lenses.Item1.GetType() != lenses.Item2.GetType());

    private static readonly Gen<(double Before, double After)> Step =
        from after in Gen.OneOf(ModelGrid.Fraction, Gen.Const(1d))
        from step in Gen.Int[1, 50]
        select (double.Max(after - (step / 1000d), 0d), after);

    private static readonly Gen<CameraPose> Pose =
        from origin in ModelGrid.Point
        from turn in ModelGrid.Turn
        from distance in ModelGrid.Size
        from lens in Lens
        let rotation = Transform.Rotation(turn.Angle, turn.Axis, Point3d.Origin)
        let frame = new Plane(origin, rotation * Vector3d.XAxis, rotation * Vector3d.YAxis)
        select new CameraPose(frame, frame.Origin - (distance * frame.ZAxis), lens);
}
