using System.Globalization;
using CsCheck;
using Rasm.Rhino.Modeling;
using Xunit;

namespace Rasm.Rhino.Tests.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DiagnosisTests {

    // --- [CURVES]

    [Fact]
    public async Task ZeroLengthLineIsInvalidWithRhinoLog() {
        using LineCurve line = new(Point3d.Origin, Point3d.Origin);
        Diagnosis<LineCurve> diagnosis = await RhinoHost.RunAsync(line.Diagnose(ModelGrid.Model));
        Assert.Equal((false, true), (diagnosis.Valid, diagnosis.Log.IsSome));
        Assert.Equal([DefectKind.Invalid, DefectKind.ShortCurve], diagnosis.Defects.Map(static defect => defect.Kind));
    }

    [Fact]
    public Task PolylineOpenWithinToleranceIsClosedByItsRepairWithAKinkAtItsSeam() =>
        ModelGrid.Size.Select(ModelGrid.Size, Gen.Int[1, 999]).SampleAsync(static async (width, depth, gap) => {
            using PolylineCurve open = new([new Point3d(0, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, gap * ModelGrid.Model.Absolute / 1000d)]);
            Diagnosis<PolylineCurve> found = await RhinoHost.RunAsync(open.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.OpenCurve], found.Defects.Map(static defect => defect.Kind));
            using PolylineCurve closed = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<PolylineCurve>(), open, found.Repair));
            Assert.Equal((false, true), (open.IsClosed, closed.IsClosed));
            Assert.Equal([DefectKind.SeamKink], (await RhinoHost.RunAsync(closed.Diagnose(ModelGrid.Model))).Defects.Map(static defect => defect.Kind));
        });

    [Fact]
    public Task PolylineOpenBeyondToleranceIsNotOfferedClosure() =>
        ModelGrid.Size.Select(ModelGrid.Size, Gen.Int[1, 1000]).SampleAsync(static async (width, depth, excess) => {
            using PolylineCurve open = new([new Point3d(0, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, ModelGrid.Model.Absolute * (1d + (excess / 1000d)))]);
            Assert.Empty((await RhinoHost.RunAsync(open.Diagnose(ModelGrid.Model))).Defects);
        });

    [Fact]
    public Task ClosedRectangleWithItsSeamMidEdgeHasNoDefects() =>
        ModelGrid.Size.Select(ModelGrid.Size).SampleAsync(static async (width, depth) => {
            using PolylineCurve closed = new([new Point3d(width / 2, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, 0), new Point3d(width / 2, 0, 0)]);
            Assert.Empty((await RhinoHost.RunAsync(closed.Diagnose(ModelGrid.Model))).Defects);
        });

    // --- [BREPS]

    [Fact]
    public Task InwardBoxIsDiagnosedAndRepairedOutwardKeepingItsVolume() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size).SampleAsync(static async (x, y, z) => {
            using Brep box = new Box(new BoundingBox(0, 0, 0, x, y, z)).ToBrep();
            box.Flip();
            Diagnosis<Brep> found = await RhinoHost.RunAsync(box.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.InwardSolid], found.Defects.Map(static defect => defect.Kind));
            using Brep repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Brep>(), box, found.Repair));
            Assert.Equal((BrepSolidOrientation.Inward, BrepSolidOrientation.Outward), (box.SolidOrientation, repaired.SolidOrientation));
            Assert.Equal(x * y * z, repaired.GetVolume(), ModelGrid.Model.Absolute * ((x * y) + (y * z) + (x * z)) * 2d);
            Assert.Empty((await RhinoHost.RunAsync(repaired.Diagnose(ModelGrid.Model))).Defects);
        });

    [Fact]
    public async Task JoinedSheetLocatesEachNakedEdgeAndNotTheSeamBetweenItsFaces() {
        using Brep left = Brep.CreateFromCornerPoints(new Point3d(0, 0, 0), new Point3d(3, 0, 0), new Point3d(3, 2, 0), new Point3d(0, 2, 0), ModelGrid.Model.Absolute);
        using Brep right = Brep.CreateFromCornerPoints(new Point3d(3, 0, 0), new Point3d(7, 0, 0), new Point3d(7, 2, 0), new Point3d(3, 2, 0), ModelGrid.Model.Absolute);
        using Brep sheet = Assert.Single(Brep.JoinBreps([left, right], ModelGrid.Model.Absolute));
        Defect<Brep> naked = Assert.Single((await RhinoHost.RunAsync(sheet.Diagnose(ModelGrid.Model))).Defects);
        int[] outer = [.. sheet.Edges.Where(static edge => Math.Abs(edge.PointAt(edge.Domain.Mid).X - 3) > ModelGrid.Model.Absolute).Select(static edge => edge.EdgeIndex)];
        Assert.Equal((DefectKind.NakedEdges, 7, 6), (naked.Kind, sheet.Edges.Count, naked.Count));
        Assert.Equal(outer.Order(), naked.At.Map(static at => at.Index).Order());
        Assert.All(naked.At, static at => Assert.Equal(ComponentIndexType.BrepEdge, at.ComponentIndexType));
    }

    // --- [MESHES]

    [Fact]
    public Task InwardMeshIsDiagnosedAndRepairedOutwardKeepingItsVolume() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size).SampleAsync(static async (x, y, z) => {
            using Mesh mesh = Box(x, y, z);
            mesh.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true);
            Diagnosis<Mesh> found = await RhinoHost.RunAsync(mesh.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.InwardSolid], found.Defects.Map(static defect => defect.Kind));
            using Mesh repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Mesh>(), mesh, found.Repair));
            Point3d center = new(x / 2, y / 2, z / 2);
            Assert.All(Enumerable.Range(0, repaired.Faces.Count), face => Assert.True(repaired.FaceNormals[face] * (repaired.Faces.GetFaceCenter(face) - center) > 0d, string.Create(CultureInfo.InvariantCulture, $"face {face} points inward")));
            Assert.Equal(x * y * z, repaired.Volume(), ModelGrid.Model.Absolute * ((x * y) + (y * z) + (x * z)) * 2d);
            Assert.Empty((await RhinoHost.RunAsync(repaired.Diagnose(ModelGrid.Model))).Defects);
        });

    [Fact]
    public Task UnusedMeshVerticesAreCountedAndCulledByTheRepairWhichKeepsEveryFaceAndUsedVertex() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size, ModelGrid.Point.Array[1, 12]).SampleAsync(static async (x, y, z, strays) => {
            using Mesh mesh = Box(x, y, z);
            Point3d[] used = [.. mesh.Vertices.ToPoint3dArray()];
            toSeq(strays).Iter(stray => {
                mesh.Vertices.Add(stray);
                mesh.Normals.Add(Vector3d.ZAxis);
            });
            Diagnosis<Mesh> found = await RhinoHost.RunAsync(mesh.Diagnose(ModelGrid.Model));
            Defect<Mesh> stray = Assert.Single(found.Defects);
            Assert.Equal((true, DefectKind.UnusedVertices, strays.Length), (found.Valid, stray.Kind, stray.Count));
            using Mesh repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Mesh>(), mesh, stray.Repair));
            Assert.Equal(used.Order(), repaired.Vertices.ToPoint3dArray().Order());
            Assert.Equal(mesh.Faces.Count, repaired.Faces.Count);
            Assert.Empty((await RhinoHost.RunAsync(repaired.Diagnose(ModelGrid.Model))).Defects);
            Assert.Equal(x * y * z, repaired.Volume(), ModelGrid.Model.Absolute * ((x * y) + (y * z) + (x * z)) * 2d);
        });

    // --- [GEOMETRY]

    private static Mesh Box(double x, double y, double z) {
        Mesh mesh = Mesh.CreateFromBox(new BoundingBox(0, 0, 0, x, y, z), 1, 1, 1);
        mesh.TextureCoordinates.Clear();
        mesh.VertexColors.Clear();
        return mesh;
    }
}
