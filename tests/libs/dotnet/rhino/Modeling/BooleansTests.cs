using CsCheck;
using Rasm.Rhino.Modeling;
using Xunit;

namespace Rasm.Rhino.Tests.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class BooleansTests {

    // --- [SOLIDS]

    [Fact]
    public Task DifferenceOfACornerOverlapIsOneValidSolidWithTheOverlapRemoved() =>
        Corner.SampleAsync(static async corner => {
            using Brep kept = new Box(new BoundingBox(Point3d.Origin, corner.Extent)).ToBrep();
            using Brep cutter = new Box(new BoundingBox(corner.Cut, corner.Cut + new Vector3d(corner.Extent))).ToBrep();
            Grouped<Brep, int> solid = Assert.Single(await RhinoHost.RunAsync(Booleans.Solid(ModelGrid.Model, new Boolean<Brep>(SetOperation.Difference, [kept], [cutter]), manifoldOnly: true)));
            using Brep piece = solid.Value;
            using VolumeMassProperties mass = VolumeMassProperties.Compute(piece);
            BoundingBox bounds = piece.GetBoundingBox(accurate: true);
            Assert.True(piece.IsValidTopology(out string topology), topology);
            Assert.True(piece.IsValidGeometry(out string geometry), geometry);
            Assert.Equal((true, 9, Seq(0)), (piece.IsSolid, piece.Faces.Count, solid.From));
            Assert.Equal(
                (corner.Extent.X * corner.Extent.Y * corner.Extent.Z) - ((corner.Extent.X - corner.Cut.X) * (corner.Extent.Y - corner.Cut.Y) * (corner.Extent.Z - corner.Cut.Z)),
                mass.Volume,
                piece.GetArea() * ModelGrid.Model.Absolute);
            Assert.InRange(bounds.Min.DistanceTo(Point3d.Origin) + bounds.Max.DistanceTo(corner.Extent), 0d, ModelGrid.Model.Absolute);
        });

    // --- [GENERATORS]

    private static readonly Gen<(Point3d Extent, Point3d Cut)> Corner =
        from x in ModelGrid.Size
        from y in ModelGrid.Size
        from z in ModelGrid.Size
        from cut in ModelGrid.Fraction.Where(static t => t is >= 0.1 and <= 0.9).Array[3]
        select (Extent: new Point3d(x, y, z), Cut: new Point3d(x * cut[0], y * cut[1], z * cut[2]));
}
