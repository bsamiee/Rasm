using CsCheck;
using Rasm.Rhino.Modeling;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TransformsTests {

    // --- [DECOMPOSITION]

    [Fact]
    public void DecompositionClassifiesEachAffineKindAndComposesBack() =>
        Classified.Sample(static (xform, kind) => {
            Decomposition parts = Decomposition.Of(xform, RhinoMath.ZeroTolerance).ThrowIfFail();
            Assert.IsType(kind, parts);
            Assert.InRange(Deviation(parts.Composed, xform), 0d, RhinoMath.SqrtEpsilon);
        });

    [Fact]
    public void ProjectiveTransformFailsAsUnsupported() =>
        Gen.Int[0, 2].Select(Gen.OneOf(Gen.Double[0.01, 1d], Gen.Const(double.NaN), Gen.Const(double.PositiveInfinity), Gen.Const(double.Epsilon))).Sample(static (column, weight) =>
            Assert.Equal(RhinoReason.Unsupported, Expect<RhinoFault>.Failed(Decomposition.Of(new Transform(1d) { [3, column] = weight }, RhinoMath.ZeroTolerance)).Reason));

    // --- [INTERPOLATION]

    [Fact]
    public void InterpolationReproducesBothEndsOfOneOrientation() =>
        Classified.Select(Classified).Sample(static (start, end) => {
            Decomposition first = Decomposition.Of(start.Xform, RhinoMath.ZeroTolerance).ThrowIfFail();
            Decomposition last = Decomposition.Of(end.Xform, RhinoMath.ZeroTolerance).ThrowIfFail();
            Assert.InRange(Deviation(first.Interpolate(last, 0d).ThrowIfFail(), start.Xform), 0d, RhinoMath.SqrtEpsilon);
            Assert.InRange(Deviation(first.Interpolate(last, 1d).ThrowIfFail(), end.Xform), 0d, RhinoMath.SqrtEpsilon);
        });

    [Fact]
    public void MirrorPairsInterpolateWithinTheMirroredOrientation() =>
        Mirror.Select(Mirror).Sample(static (start, end) => {
            Decomposition first = Decomposition.Of(start, RhinoMath.ZeroTolerance).ThrowIfFail();
            Decomposition last = Decomposition.Of(end, RhinoMath.ZeroTolerance).ThrowIfFail();
            Assert.InRange(Deviation(first.Interpolate(last, 0d).ThrowIfFail(), start), 0d, RhinoMath.SqrtEpsilon);
            Assert.InRange(Deviation(first.Interpolate(last, 1d).ThrowIfFail(), end), 0d, RhinoMath.SqrtEpsilon);
            Assert.InRange(first.Interpolate(last, 0.5).ThrowIfFail().Determinant, double.NegativeInfinity, 0d);
        });

    [Fact]
    public void InterpolatingFromIdentityTowardsATurnTurnsByTheSameFraction() =>
        ModelGrid.Turn.Select(ModelGrid.Fraction).Sample(static (turn, t) =>
            Assert.InRange(
                Deviation(
                    Decomposition.Of(Transform.Identity, RhinoMath.ZeroTolerance).ThrowIfFail()
                        .Interpolate(Decomposition.Of(Transform.Rotation(turn.Angle, turn.Axis, Point3d.Origin), RhinoMath.ZeroTolerance).ThrowIfFail(), t).ThrowIfFail(),
                    Transform.Rotation(t * turn.Angle, turn.Axis, Point3d.Origin)),
                0d,
                RhinoMath.SqrtEpsilon));

    [Fact]
    public void HalfwayTowardsAHalfTurnIsAQuarterTurnAboutTheSameAxis() {
        Transform quarter = Decomposition.Of(Transform.Identity, RhinoMath.ZeroTolerance).ThrowIfFail()
            .Interpolate(Decomposition.Of(Transform.Rotation(Math.PI, Vector3d.ZAxis, Point3d.Origin), RhinoMath.ZeroTolerance).ThrowIfFail(), 0.5).ThrowIfFail();
        Assert.Equal(0d, quarter.M00, RhinoMath.SqrtEpsilon);
        Assert.Equal(1d, Math.Abs(quarter.M10), RhinoMath.SqrtEpsilon);
        Assert.Equal(1d, quarter.M22, RhinoMath.SqrtEpsilon);
    }

    [Fact]
    public void InterpolationFromEitherEndMeetsAtComplementaryParameters() =>
        Classified.Select(Classified, ModelGrid.Fraction).Sample(static (start, end, t) => {
            Decomposition first = Decomposition.Of(start.Xform, RhinoMath.ZeroTolerance).ThrowIfFail();
            Decomposition last = Decomposition.Of(end.Xform, RhinoMath.ZeroTolerance).ThrowIfFail();
            Assert.InRange(Deviation(first.Interpolate(last, t).ThrowIfFail(), last.Interpolate(first, 1d - t).ThrowIfFail()), 0d, RhinoMath.SqrtEpsilon);
        });

    [Fact]
    public void InterpolatedTranslationIsTheLinearBlendOfItsEnds() =>
        ModelGrid.Offset.Select(ModelGrid.Offset, ModelGrid.Fraction).Sample(static (start, end, t) =>
            Assert.InRange(
                Deviation(
                    Decomposition.Of(Transform.Translation(start), RhinoMath.ZeroTolerance).ThrowIfFail()
                        .Interpolate(Decomposition.Of(Transform.Translation(end), RhinoMath.ZeroTolerance).ThrowIfFail(), t).ThrowIfFail(),
                    Transform.Translation(start + (t * (end - start)))),
                0d,
                RhinoMath.SqrtEpsilon));

    [Fact]
    public void ParameterBeyondEitherEndFailsAsOutside() =>
        Gen.OneOf(
                Gen.Double[-10d, -1e-6],
                Gen.Double[1d + 1e-6, 10d],
                Gen.OneOf(Gen.Const(double.NaN), Gen.Const(double.PositiveInfinity), Gen.Const(double.NegativeInfinity), Gen.Const(double.BitDecrement(0d)), Gen.Const(double.BitIncrement(1d))))
            .Sample(static t =>
                Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(
                    Decomposition.Of(Transform.Identity, RhinoMath.ZeroTolerance).ThrowIfFail().Interpolate(Decomposition.Of(Transform.Identity, RhinoMath.ZeroTolerance).ThrowIfFail(), t)).Reason));

    [Fact]
    public void InterpolatingAMirrorTowardsARotationFailsAsMismatched() =>
        Mirror.Select(Classified).Sample(static (mirror, rotated) =>
            Assert.Equal(RhinoReason.Mismatched, Expect<RhinoFault>.Failed(
                Decomposition.Of(mirror, RhinoMath.ZeroTolerance).ThrowIfFail().Interpolate(Decomposition.Of(rotated.Xform, RhinoMath.ZeroTolerance).ThrowIfFail(), 0.5)).Reason));

    // --- [ORIENTATION]

    [Fact]
    public Task MirroredSolidStaysOutwardWithItsVolume() =>
        Mirror.SampleAsync(static async mirror => {
            using Brep box = new Box(new BoundingBox(0, 0, 0, 10, 10, 10)).ToBrep();
            using Brep mirrored = await RhinoHost.RunAsync(Transformations.Transformed(box.DuplicateBrep(), mirror));
            Assert.Equal(BrepSolidOrientation.Outward, mirrored.SolidOrientation);
            Assert.Equal(1000d, mirrored.GetVolume(), box.GetArea() * ModelGrid.Model.Absolute);
        });

    [Fact]
    public Task MirroredMeshStaysOutwardWithEveryFacePointingAway() =>
        Mirror.SampleAsync(static async mirror => {
            using Mesh box = Mesh.CreateFromBox(new BoundingBox(0, 0, 0, 10, 10, 10), 1, 1, 1);
            Assert.Equal(1, box.SolidOrientation());
            using Mesh mirrored = await RhinoHost.RunAsync(Transformations.Transformed(box.DuplicateMesh(), mirror));
            Point3d center = new(5, 5, 5);
            center.Transform(mirror);
            Assert.All(Enumerable.Range(0, mirrored.Faces.Count), face => Assert.True(mirrored.FaceNormals[face] * (mirrored.Faces.GetFaceCenter(face) - center) > 0d));
            Assert.Equal(1, mirrored.SolidOrientation());
        });

    // --- [GENERATORS]

    private static readonly Gen<Transform> Rotation = ModelGrid.Turn.Select(static turn => Transform.Rotation(turn.Angle, turn.Axis, Point3d.Origin));

    private static readonly Gen<double> Factor = Gen.Int[100, 10_000].Select(static units => units / 1000d);

    private static readonly Gen<Transform> Stretch =
        from x in Factor
        from y in Factor
        from z in Factor
        where Math.Abs(x - y) > 0.01 || Math.Abs(y - z) > 0.01
        select Transform.Scale(Plane.WorldXY, x, y, z);

    private static readonly Gen<(Transform Xform, Type Kind)> Classified = Gen.OneOf(
        ModelGrid.Offset.Select(Rotation, static (offset, turn) => (Transform.Translation(offset) * turn, typeof(Decomposition.Rigid))),
        ModelGrid.Offset.Select(Rotation, Factor.Where(static factor => Math.Abs(factor - 1d) > 0.01), static (offset, turn, factor) => (Transform.Translation(offset) * turn * Transform.Scale(Point3d.Origin, factor), typeof(Decomposition.Similarity))),
        ModelGrid.Offset.Select(Rotation, Stretch, static (offset, turn, stretch) => (Transform.Translation(offset) * turn * stretch, typeof(Decomposition.Affine))),
        ModelGrid.Offset.Select(Rotation, Stretch, Rotation, static (offset, turn, stretch, frame) => (Transform.Translation(offset) * turn * stretch * frame, typeof(Decomposition.Affine))));

    private static readonly Gen<Transform> Mirror =
        ModelGrid.Point.Select(ModelGrid.Offset.Where(static normal => normal.Length > 1d), static (origin, normal) => Transform.Mirror(new Plane(origin, normal)));

    private static double Deviation(Transform left, Transform right) =>
        Range(0, 16).Fold(0d, (held, cell) => Math.Max(held, Math.Abs(left[cell / 4, cell % 4] - right[cell / 4, cell % 4]) / (1d + Math.Abs(right[cell / 4, cell % 4]))));
}
