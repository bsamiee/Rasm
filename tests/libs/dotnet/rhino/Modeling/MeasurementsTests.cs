using CsCheck;
using Rasm.Rhino.Modeling;
using Xunit;

namespace Rasm.Rhino.Tests.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class MeasurementsTests {

    // --- [VOLUMES]

    [Fact]
    public Task WeightedVolumeIsDensityTimesPlacementsTimesVolumeAroundTheMeanPlacement() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size, Gen.Int[1, 100], Gen.Int[1, 4], ModelGrid.Size).SampleAsync(static async (x, y, z, density, count, spacing) => {
            using Brep box = new Box(new BoundingBox(0, 0, 0, x, y, z)).ToBrep();
            Seq<Transform> placements = toSeq(Enumerable.Range(0, count)).Map(at => Transform.Translation(at * spacing, 0, 0)).Strict();
            (Seq<Seq<VolumeMassProperties>> parts, VolumeMassProperties total) = await RhinoHost.RunAsync(
                IO.lift(MassSubject.Validated(new MassSource.Shape(box), density * 0.1, placements))
                    .Bind(static subject => Measurements.Volume(ModelGrid.Model, [subject], firstMoments: true, secondMoments: false, productMoments: false)));
            using VolumeMassProperties weighted = total;
            parts.Bind(static placed => placed).Iter(static part => part.Dispose());
            Assert.Equal(count, parts.Bind(static placed => placed).Count);
            Assert.Equal(density * 0.1 * count * x * y * z, weighted.Volume, density * 0.1 * count * box.GetArea() * ModelGrid.Model.Absolute);
            Assert.InRange(weighted.Centroid.DistanceTo(new Point3d((x / 2) + ((count - 1) * spacing / 2), y / 2, z / 2)), 0d, ModelGrid.Model.Absolute);
        });

    [Fact]
    public Task StretchedPlacementScalesTheVolumeByItsFactors() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size, Factor.Array[3]).SampleAsync(static async (x, y, z, factors) => {
            using Brep box = new Box(new BoundingBox(0, 0, 0, x, y, z)).ToBrep();
            (Seq<Seq<VolumeMassProperties>> parts, VolumeMassProperties total) = await RhinoHost.RunAsync(
                IO.lift(MassSubject.Validated(new MassSource.Shape(box), 1d, [Transform.Scale(Plane.WorldXY, factors[0], factors[1], factors[2])]))
                    .Bind(static subject => Measurements.Volume(ModelGrid.Model, [subject], firstMoments: false, secondMoments: false, productMoments: false)));
            using VolumeMassProperties weighted = total;
            parts.Bind(static placed => placed).Iter(static part => part.Dispose());
            (double sx, double sy, double sz) = (factors[0] * x, factors[1] * y, factors[2] * z);
            Assert.Equal(sx * sy * sz, weighted.Volume, 2 * ((sx * sy) + (sy * sz) + (sz * sx)) * ModelGrid.Model.Absolute);
        });

    // --- [DENSITY]

    [Fact]
    public void ZeroDensityIsAccepted() {
        using Point point = new(Point3d.Origin);
        Assert.Equal(0d, MassSubject.Validated(new MassSource.Shape(point), 0d, []).ThrowIfFail().Density);
    }

    [Fact]
    public void NegativeDensityFailsAsOutside() =>
        Gen.Int[1, 1000].Sample(static density => {
            using Point point = new(Point3d.Origin);
            Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(MassSubject.Validated(new MassSource.Shape(point), -density * 0.01, [])).Reason);
        });

    [Theory]
    [InlineData(-double.Epsilon)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void DensityPastZeroOrNotFiniteFailsAsOutside(double density) {
        using Point point = new(Point3d.Origin);
        Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(MassSubject.Validated(new MassSource.Shape(point), density, [])).Reason);
    }

    // --- [GENERATORS]

    private static readonly Gen<double> Factor = Gen.Int[100, 10_000].Select(static units => units / 1000d);
}
