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
        Assert.Equal([DefectKind.Invalid.Message, DefectKind.ShortCurve.Message], diagnosis.Defects.Map(static defect => defect.Kind.Message));
    }

    [Fact]
    public Task PolylineOpenWithinToleranceIsClosedByItsRepairWithAKinkAtItsSeam() =>
        ModelGrid.Size.Select(ModelGrid.Size, Gen.Int[1, 999]).SampleAsync(static async (width, depth, gap) => {
            using PolylineCurve open = new([new Point3d(0, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, gap * ModelGrid.Model.Absolute / 1000d)]);
            Diagnosis<PolylineCurve> found = await RhinoHost.RunAsync(open.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.OpenCurve.Message], found.Defects.Map(static defect => defect.Kind.Message));
            using PolylineCurve closed = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<PolylineCurve>(), open, found.Repair));
            Assert.Equal((false, true), (open.IsClosed, closed.IsClosed));
            Assert.Equal([DefectKind.SeamKink.Message], (await RhinoHost.RunAsync(closed.Diagnose(ModelGrid.Model))).Defects.Map(static defect => defect.Kind.Message));
        });

    [Fact]
    public Task PolylineOpenBeyondToleranceIsNotOfferedClosure() =>
        ModelGrid.Size.Select(ModelGrid.Size, Gen.Int[1, 1000]).SampleAsync(static async (width, depth, excess) => {
            using PolylineCurve open = new([new Point3d(0, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, ModelGrid.Model.Absolute * (1d + (excess / 1000d)))]);
            Assert.Empty((await RhinoHost.RunAsync(open.Diagnose(ModelGrid.Model))).Defects.Map(static defect => defect.Kind.Message));
        });

    [Fact]
    public Task ClosedRectangleWithItsSeamMidEdgeHasNoDefects() =>
        ModelGrid.Size.Select(ModelGrid.Size).SampleAsync(static async (width, depth) => {
            using PolylineCurve closed = new([new Point3d(width / 2, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), new Point3d(0, 0, 0), new Point3d(width / 2, 0, 0)]);
            Assert.Empty((await RhinoHost.RunAsync(closed.Diagnose(ModelGrid.Model))).Defects.Map(static defect => defect.Kind.Message));
        });

    // --- [BREPS]

    [Fact]
    public Task InwardBoxIsDiagnosedAndRepairedOutward() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size).SampleAsync(static async (x, y, z) => {
            using Brep box = new Box(new BoundingBox(0, 0, 0, x, y, z)).ToBrep();
            box.Flip();
            Diagnosis<Brep> found = await RhinoHost.RunAsync(box.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.InwardSolid.Message], found.Defects.Map(static defect => defect.Kind.Message));
            using Brep repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Brep>(), box, found.Repair));
            Assert.Equal((BrepSolidOrientation.Inward, BrepSolidOrientation.Outward), (box.SolidOrientation, repaired.SolidOrientation));
            Assert.Empty((await RhinoHost.RunAsync(repaired.Diagnose(ModelGrid.Model))).Defects);
        });

    [Fact]
    public Task OpenSheetLocatesEachNakedEdge() =>
        ModelGrid.Size.Select(ModelGrid.Size).SampleAsync(static async (width, depth) => {
            using Brep sheet = Brep.CreateFromCornerPoints(new Point3d(0, 0, 0), new Point3d(width, 0, 0), new Point3d(width, depth, 0), new Point3d(0, depth, 0), ModelGrid.Model.Absolute);
            Defect<Brep> naked = Assert.Single((await RhinoHost.RunAsync(sheet.Diagnose(ModelGrid.Model))).Defects);
            Assert.Equal((DefectKind.NakedEdges.Message, 4), (naked.Kind.Message, naked.Count));
            Assert.Equal(toSeq(Enumerable.Range(0, 4)).Map(static edge => new ComponentIndex(ComponentIndexType.BrepEdge, edge)), naked.At);
        });

    // --- [MESHES]

    [Fact]
    public Task InwardMeshIsDiagnosedAndRepairedOutward() =>
        ModelGrid.Size.Select(ModelGrid.Size, ModelGrid.Size).SampleAsync(static async (x, y, z) => {
            using Mesh mesh = Mesh.CreateFromBox(new BoundingBox(0, 0, 0, x, y, z), 1, 1, 1);
            mesh.TextureCoordinates.Clear();
            mesh.VertexColors.Clear();
            mesh.Flip(vertexNormals: true, faceNormals: true, faceOrientation: true);
            Diagnosis<Mesh> found = await RhinoHost.RunAsync(mesh.Diagnose(ModelGrid.Model));
            Assert.Equal([DefectKind.InwardSolid.Message], found.Defects.Map(static defect => defect.Kind.Message));
            using Mesh repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Mesh>(), mesh, found.Repair));
            Point3d center = new(x / 2, y / 2, z / 2);
            Assert.All(Enumerable.Range(0, repaired.Faces.Count), face => Assert.True(repaired.FaceNormals[face] * (repaired.Faces.GetFaceCenter(face) - center) > 0d, string.Create(CultureInfo.InvariantCulture, $"face {face} points inward")));
            Assert.Equal((-1, 1), (mesh.SolidOrientation(), repaired.SolidOrientation()));
            Assert.Empty((await RhinoHost.RunAsync(repaired.Diagnose(ModelGrid.Model))).Defects);
        });

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task EveryUnusedMeshVertexIsCountedAndCulledByItsRepair(int unused) {
        using Mesh mesh = Mesh.CreateFromBox(new BoundingBox(0, 0, 0, 10, 10, 10), 1, 1, 1);
        mesh.TextureCoordinates.Clear();
        mesh.VertexColors.Clear();
        int used = mesh.Vertices.Count;
        toSeq(Enumerable.Range(1, unused)).Iter(at => {
            mesh.Vertices.Add(20 * at, 20, 20);
            mesh.Normals.Add(Vector3d.ZAxis);
        });
        Diagnosis<Mesh> found = await RhinoHost.RunAsync(mesh.Diagnose(ModelGrid.Model));
        Assert.True(found.Valid);
        Defect<Mesh> stray = Assert.Single(found.Defects);
        Assert.Equal((DefectKind.UnusedVertices.Message, unused), (stray.Kind.Message, stray.Count));
        using Mesh repaired = await RhinoHost.RunAsync(Edits.Apply(Stages.Geometry<Mesh>(), mesh, stray.Repair));
        Assert.Equal((used + unused, used), (mesh.Vertices.Count, repaired.Vertices.Count));
    }
}
