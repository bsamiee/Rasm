using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using NSubstitute;
using Rasm.Spatial;
using Rasm.TestSupport;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Spatial {
    [Fact]
    public void AffineBounds() =>
        Gen.Int[-8, 8].Array2D[3, 4].Select(Gen.Int[-8, 8].Array[6], static (entries, ends) => (Entries: entries, Ends: ends)).ForAll(static sample => {
            Matrix<double> matrix = Matrix<double>.Build.Dense(4, 4, (row, column) => row is 3 ? column is 3 ? 1d : 0d : sample.Entries[row, column]);
            Bounds source = new(new(sample.Ends[0], sample.Ends[1], sample.Ends[2]), new(sample.Ends[3], sample.Ends[4], sample.Ends[5]));
            Bounds actual = source.Transformed(matrix);
            Vector<double>[] corners = [.. from x in new[] { source.Min.X, source.Max.X }
                                           from y in new[] { source.Min.Y, source.Max.Y }
                                           from z in new[] { source.Min.Z, source.Max.Z }
                                           select matrix * Vector<double>.Build.DenseOfArray([x, y, z, 1d])];
            Assert.Equal(new[] { corners.Min(static corner => corner[0]), corners.Min(static corner => corner[1]), corners.Min(static corner => corner[2]) }, new[] { actual.Min.X, actual.Min.Y, actual.Min.Z });
            Assert.Equal(new[] { corners.Max(static corner => corner[0]), corners.Max(static corner => corner[1]), corners.Max(static corner => corner[2]) }, new[] { actual.Max.X, actual.Max.Y, actual.Max.Z });
        });

    [Theory]
    [InlineData(-double.MaxValue, double.MaxValue)]
    [InlineData(7d, -9d)]
    public void IdentityBounds(double first, double second) {
        Bounds actual = new Bounds(new(first, 2d, 3d), new(second, 5d, 7d)).Transformed(Matrix<double>.Build.DenseIdentity(4));
        Assert.Equal(new Vector3D<double>(double.Min(first, second), 2d, 3d), actual.Min);
        Assert.Equal(new Vector3D<double>(double.Max(first, second), 5d, 7d), actual.Max);
    }

    [Fact]
    public void TransformedBoundsBits() {
        Gen<double> values = Gen.Double.Special;
        values.Array2D[4, 4].Select(values.Array[6], static (entries, ends) => (Entries: entries, Ends: ends)).ForAll(static sample =>
            BoundsBits(new(new(sample.Ends[0], sample.Ends[1], sample.Ends[2]), new(sample.Ends[3], sample.Ends[4], sample.Ends[5])), Matrix<double>.Build.DenseOfArray(sample.Entries)));
    }

    [Theory]
    [InlineData(0UL, 0x8000_0000_0000_0000UL)]
    [InlineData(0x7ff0_0000_0000_0000UL, 0xfff0_0000_0000_0000UL)]
    [InlineData(0x4000_0000_0000_0000UL, 0xc000_0000_0000_0000UL)]
    [InlineData(0x7ff8_0000_0000_0001UL, 0x7ff8_0000_0000_0002UL)]
    [InlineData(0x7ff8_0000_0000_0002UL, 0x7ff8_0000_0000_0001UL)]
    [InlineData(0x7ff0_0000_0000_0001UL, 0xfff8_0000_0000_0002UL)]
    [InlineData(1UL, 0x000f_ffff_ffff_ffffUL)]
    public void SpecialTransformedBoundsBits(ulong firstBits, ulong secondBits) {
        (double first, double second) = (BitConverter.UInt64BitsToDouble(firstBits), BitConverter.UInt64BitsToDouble(secondBits));
        Bounds source = new(new(first, 1d, 2d), new(second, 3d, 4d));
        foreach (Matrix<double> matrix in new[] { Matrix<double>.Build.DenseIdentity(4), Matrix<double>.Build.Dense(4, 4, static (_, column) => column is 0 ? 1d : double.NegativeZero) })
            BoundsBits(source, matrix);
    }

    [Fact]
    public void BezierVolumeEvaluation() =>
        (from degrees in Gen.Int[1, 4].Array[3]
         from coordinates in Gen.Int[-16, 16].Array[3 * (degrees[0] + 1) * (degrees[1] + 1) * (degrees[2] + 1)]
         select (Degrees: degrees, Coordinates: coordinates))
            .Select(Gen.Int[-2, 6].Array[3], static (sample, at) => (sample.Degrees, sample.Coordinates, At: at))
            .ForAll(static sample => {
                (int u, int v, int w) = (sample.Degrees[0], sample.Degrees[1], sample.Degrees[2]);
                Seq<Vector3D<double>> controls = toSeq(sample.Coordinates.Chunk(3).Select(static point => new Vector3D<double>(point[0], point[1], point[2])));
                Vector3D<double> at = new(sample.At[0] / 4d, sample.At[1] / 4d, sample.At[2] / 4d);
                Vector3D<double> expected = Interpolated(controls.Chunk(u + 1).Select(row => Interpolated(row, at.X)).Chunk(v + 1).Select(row => Interpolated(row, at.Y)), at.Z);
                Assert.Equal(expected, BezierVolume.Validated(u, v, w, controls).ThrowIfFail().At(at));
            });

    [Fact]
    public void BezierVolumeConcatenatedControls() {
        const int uDegree = 2;
        const int vDegree = 1;
        const int wDegree = 1;
        const int controlCount = (uDegree + 1) * (vDegree + 1) * (wDegree + 1);
        Gen.Int[-16, 16].Array[3 * controlCount].Select(Gen.Int[1, controlCount - 1], static (coordinates, split) => (Coordinates: coordinates, Split: split)).ForAll(static sample => {
            IEnumerable<Vector3D<double>> controls = sample.Coordinates.Chunk(3).Select(static point => new Vector3D<double>(point[0], point[1], point[2]));
            BezierVolume volume = BezierVolume.Validated(uDegree, vDegree, wDegree, toSeq(controls.Take(sample.Split)).Concat(toSeq(controls.Skip(sample.Split)))).ThrowIfFail();
            foreach (Vector3D<double> at in new[] { new Vector3D<double>(0d, 0d, 0d), new(1d, 1d, 1d), new(0.25d, 0.5d, 0.75d), new(-0.5d, 1.5d, 0.25d) }) {
                Vector3D<double> expected = Interpolated(controls.Chunk(uDegree + 1).Select(row => Interpolated(row, at.X)).Chunk(vDegree + 1).Select(row => Interpolated(row, at.Y)), at.Z);
                Assert.Equal(expected, volume.At(at));
            }
        });
    }

    [Theory]
    [InlineData(3, 4)]
    [InlineData(4, 3)]
    public void MovedFieldShape(int rows, int columns) {
        Fin<ScalarField.Moved> result = ScalarField.Moved.Of(new ScalarField.Uniform(1d), Matrix<double>.Build.Dense(rows, columns));
        Assert.Equal(SpatialRule.Shape, Assert.IsType<SpatialFault>(Assert.IsType<Fin<ScalarField.Moved>.Fail>(result).Error).Reason);
    }

    [Fact]
    public void MovedFieldRank() {
        Fin<ScalarField.Moved> result = ScalarField.Moved.Of(new ScalarField.Uniform(1d), Matrix<double>.Build.DiagonalOfDiagonalArray([1d, 0d, 1d, 1d]));
        Assert.Equal(SpatialRule.Singular, Assert.IsType<SpatialFault>(Assert.IsType<Fin<ScalarField.Moved>.Fail>(result).Error).Reason);
    }

    [Fact]
    public void BezierVolumeCountOverflow() =>
        Assert.Equal(SpatialRule.Count, Assert.IsType<SpatialFault>(Assert.IsType<Fin<BezierVolume>.Fail>(BezierVolume.Validated(int.MaxValue, int.MaxValue, int.MaxValue, [])).Error).Reason);

    [Fact]
    public void MovedFieldOwnsMatrix() {
        Matrix<double> matrix = Matrix<double>.Build.DenseIdentity(4);
        ScalarField.Moved moved = ScalarField.Moved.Of(new ScalarField.Ball(new(0d, 0d, 0d), 1d), matrix).ThrowIfFail();
        matrix[0, 3] = 10d;
        moved.Map[1, 3] = 20d;
        Assert.Equal(-1d, moved.Distance(new(0d, 0d, 0d)));
        Assert.Equal(Some(new Bounds(new(-1d, -1d, -1d), new(1d, 1d, 1d))), moved.Bounds);
    }

    [Fact]
    public void PolynomialPatchOwnsMatrices() {
        Matrix<double>[] powers = [.. Enumerable.Range(0, 4).Select(static component => Matrix<double>.Build.Dense(2, 2, (row, column) => (2 * row) + column == component ? 1d : 0d))];
        HomogeneousGrid grid = HomogeneousGrid.Validated(powers[2], powers[1], powers[3], powers[0]).ThrowIfFail();
        ParameterSpan interval = ParameterSpan.Validated(0d, 1d).ThrowIfFail();
        PolynomialPatch patch = new(grid, interval, interval);
        foreach (Matrix<double> input in powers) input.Clear();
        foreach (Matrix<double> exposed in new[] { grid.X, grid.Y, grid.Z, grid.W }) exposed.Clear();
        HomogeneousGrid net = patch.Net;
        Assert.Equal([0d, 0d, 1d, 1d], net.X.ToRowMajorArray().AsSpan());
        Assert.Equal([0d, 1d, 0d, 1d], net.Y.ToRowMajorArray().AsSpan());
        Assert.Equal([0d, 0d, 0d, 1d], net.Z.ToRowMajorArray().AsSpan());
        Assert.Equal([1d, 1d, 1d, 1d], net.W.ToRowMajorArray().AsSpan());
    }

    [Fact]
    public void TrochoidLargeRadii() {
        AnalyticCurve.Trochoid outside = AnalyticCurve.Trochoid.Validated(new Roll.Circle(int.MaxValue, int.MaxValue, RollSide.Outside), 1d, 1d).ThrowIfFail();
        Assert.Equal(Seq(0d, double.Tau), outside.Joins);
        Assert.Equal(new Vector3D<double>(1d, 0d, 0d), outside.At(0d));
        AnalyticCurve.Trochoid inside = AnalyticCurve.Trochoid.Validated(new Roll.Circle(1, 1 << 30, RollSide.Inside), 1d, 0d).ThrowIfFail();
        Assert.Equal(new Vector3D<double>(-1073741823d / 1073741824d, 0d, 0d), inside.At(0d));
    }

    [Fact]
    public void TrochoidLargeArches() {
        AnalyticCurve.Trochoid curve = AnalyticCurve.Trochoid.Validated(new Roll.Line(int.MaxValue), 1d, 1d).ThrowIfFail();
        Assert.Equal(Seq(0d, double.Tau), curve.Joins.Take(2));
        Assert.Equal(new Vector3D<double>(0d, 0d, 0d), curve.At(0d));
    }

    [Fact]
    public void SmoothSuperformulaLargeSymmetry() {
        AnalyticCurve.Superformula curve = AnalyticCurve.Superformula.Validated(int.MaxValue, 1, 1d, 2d, 2d, 1d, 1d).ThrowIfFail();
        TestAssertions.Equal([0d, 2d * double.Tau], curve.Joins.ToArray(), Tolerance.WithinUlps(2), NumericComparison.Elementwise);
        Assert.Equal(new Vector3D<double>(1d, 0d, 0d), curve.At(0d));
    }

    [Theory]
    [InlineData(2d, 2d, new[] { 0, 6 })]
    [InlineData(1d, 2d, new[] { 0, 1, 3, 5, 6 })]
    [InlineData(2d, 1d, new[] { 0, 2, 4, 6 })]
    [InlineData(1d, 1d, new[] { 0, 1, 2, 3, 4, 5, 6 })]
    public void SuperformulaDerivativeJoins(double cosinePower, double sinePower, int[] turns) {
        AnalyticCurve.Superformula curve = AnalyticCurve.Superformula.Validated(3, 1, 1d, cosinePower, sinePower, 1d, 1d).ThrowIfFail();
        TestAssertions.Equal([.. turns.Select(static turn => turn * double.Tau / 3d)], curve.Joins.ToArray(), Tolerance.WithinUlps(2), NumericComparison.Elementwise);
    }

    [Fact]
    public void SharedFieldsKeepOccurrenceOrder() {
        List<Vector3D<double>> samples = [];
        ScalarField shared = new ScalarField.Measured(point => {
            samples.Add(point);
            return point.X;
        }, None);
        Matrix<double> map = Matrix<double>.Build.DenseIdentity(4);
        map[0, 3] = 1d;
        ScalarField shifted = ScalarField.Moved.Of(shared, map).ThrowIfFail();
        ScalarField body = new ScalarField.Difference(new ScalarField.Offset(shifted, new ScalarField.Uniform(1d)), shared);
        ScalarField shell = new ScalarField.Shell(shared, new ScalarField.Uniform(2d));
        ScalarField field = new ScalarField.Combined([new ScalarField.Blend(body, shell, 1d), shared], Combination.Union);
        Vector3D<double> point = new(3d, 0d, 0d);
        Tolerance tolerance = Tolerance.WithinUlps(2);
        Assert.True(tolerance.Matches(1d, field.Distance(point)));
        Assert.Equal([new Vector3D<double>(2d, 0d, 0d), point, point, point], samples,
            (expected, actual) => NumericComparison.Elementwise([expected.X, expected.Y, expected.Z], [actual.X, actual.Y, actual.Z], tolerance));
    }

    [Fact]
    public void FieldCallbackFailureKeepsRepeatedOperands() {
        InvalidOperationException expected = new();
        Vector3D<double> point = new(3d, 0d, 0d);
        Func<Vector3D<double>, double> sample = Substitute.For<Func<Vector3D<double>, double>>();
        _ = sample(default).ReturnsForAnyArgs(_ => throw expected, static _ => 2d);
        Func<Vector3D<double>, double> final = Substitute.For<Func<Vector3D<double>, double>>();
        _ = final(default).ReturnsForAnyArgs(4d);
        ScalarField leaf = new ScalarField.Measured(sample, None);
        ScalarField last = new ScalarField.Measured(final, None);
        Action opened = Substitute.For<Action>();
        Action innerClosed = Substitute.For<Action>();
        Action outerClosed = Substitute.For<Action>();
        ScalarField field = new ScalarField.Combined(toSeq(Outer()), Combination.Union);
        opened.DidNotReceive()();
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => field.Distance(point)));
        _ = final.DidNotReceiveWithAnyArgs()(default);
        sample.ClearReceivedCalls();
        Assert.Equal(2d, field.Distance(point));
        Received.InOrder(() => {
            _ = sample(point);
            _ = sample(point);
            _ = final(point);
        });
        Assert.Equal(2d, field.Distance(point));
        _ = sample.Received(4)(point);
        _ = final.Received(2)(point);
        opened.Received(1)();
        innerClosed.Received(1)();
        outerClosed.Received(1)();

        IEnumerable<ScalarField> Inner() {
            try {
                yield return leaf;
                yield return leaf;
            } finally {
                innerClosed();
            }
        }

        IEnumerable<ScalarField> Outer() {
            try {
                opened();
                yield return new ScalarField.Combined(toSeq(Inner()), Combination.Union);
                yield return last;
            } finally {
                outerClosed();
            }
        }
    }

    [Fact]
    public void FieldOperandFailureKeepsException() =>
        OperandFailure(static field => field.Distance(new(0d, 0d, 0d)));

    [Fact]
    public void BoundsOperandFailureKeepsException() =>
        OperandFailure(static field => field.Bounds);

    [Fact]
    public void NestedFieldEvaluation() {
        ScalarField inner = new ScalarField.Offset(new ScalarField.Uniform(7d), new ScalarField.Uniform(2d));
        ScalarField measured = new ScalarField.Measured(inner.Distance, None);
        ScalarField outer = new ScalarField.Combined([new ScalarField.Uniform(9d), new ScalarField.Difference(measured, new ScalarField.Uniform(-6d)), measured], Combination.Union);
        Assert.Equal(5d, outer.Distance(new(0d, 0d, 0d)));
        Assert.Equal(5d, outer.Distance(new(1d, 2d, 3d)));
    }

    private static void BoundsBits(Bounds source, Matrix<double> matrix) {
        (Vector3D<double> X, Vector3D<double> Y, Vector3D<double> Z, Vector3D<double> Translation) rows = Transforms.Rows(matrix);
        (Vector3D<double> Min, Vector3D<double> Max) expected =
            (from x in new[] { source.Min.X, source.Max.X }
             from y in new[] { source.Min.Y, source.Max.Y }
             from z in new[] { source.Min.Z, source.Max.Z }
             select rows.Apply(new Vector3D<double>(x, y, z)))
                .Select(static corner => (Min: corner, Max: corner))
                .Aggregate(static (held, corner) => (
                    new Vector3D<double>(double.Min(held.Min.X, corner.Min.X), double.Min(held.Min.Y, corner.Min.Y), double.Min(held.Min.Z, corner.Min.Z)),
                    new Vector3D<double>(double.Max(held.Max.X, corner.Max.X), double.Max(held.Max.Y, corner.Max.Y), double.Max(held.Max.Z, corner.Max.Z))));
        Bounds actual = source.Transformed(matrix);
        Assert.Equal(Bits(expected), Bits((actual.Min, actual.Max)));

        static IEnumerable<long> Bits((Vector3D<double> Min, Vector3D<double> Max) pair) =>
            from point in new[] { pair.Min, pair.Max }
            from value in new[] { point.X, point.Y, point.Z }
            select BitConverter.DoubleToInt64Bits(value);
    }

    private static void OperandFailure<A>(Func<ScalarField, A> evaluate) {
        InvalidOperationException failure = new();
        Action closed = Substitute.For<Action>();
        ScalarField field = new ScalarField.Combined(toSeq(Operands()), Combination.Union);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => evaluate(field)));
        closed.Received(1)();

        IEnumerable<ScalarField> Operands() {
            try {
                yield return new ScalarField.Ball(new(0d, 0d, 0d), 1d);
                throw failure;
            } finally {
                closed();
            }
        }
    }

    private static Vector3D<double> Interpolated(IEnumerable<Vector3D<double>> controls, double at) {
        Vector3D<double>[] row = [.. controls];
        for (int count = row.Length - 1; count > 0; count--)
            for (int index = 0; index < count; index++)
                row[index] = ((1d - at) * row[index]) + (at * row[index + 1]);
        return row[0];
    }
}
