using System.Drawing;
using Size = System.Drawing.Size;
using CsCheck;
using Library;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Xunit;

namespace Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SlabTests {

    // --- [GEOMETRY]

    private const double Tolerance = 0.001;

    private static readonly Gen<double> Length = Gen.Int[2, 100_000].Select(static units => units * Tolerance);

    [Fact]
    public void ExtrudedSlabIsValidSolidWithExactVolume() =>
        Length.Select(Length, Length).Sample(static (width, depth, height) => Solid(width, depth, height));

    [Fact]
    public void SmallestGridSlabKeepsItsVolume() => Solid(0.002, 0.002, 0.002);

    [Fact]
    public void OpenOutlineHasNoSlab() =>
        Assert.Null(Slabs.Extrude(new Line(Point3d.Origin, new Point3d(1, 0, 0)).ToNurbsCurve(), 1, Tolerance));

    private static void Solid(double width, double depth, double height) {
        Brep slab = Assert.IsType<Brep>(Slabs.Extrude(new Rectangle3d(Plane.WorldXY, width, depth).ToNurbsCurve(), height, Tolerance));
        Assert.True(slab.IsValidWithLog(out string log), log);
        Assert.True(slab.IsSolid);
        Assert.Equal(width * depth * height, slab.GetVolume(Tolerance, Tolerance), Tolerance * ((width * depth) + (height * (width + depth))));
    }

    // --- [DOCUMENT]

    [Fact]
    public async Task PlacedSlabsShareOneUndoRecordAndSurviveSave() {
        DirectoryInfo folder = Directory.CreateTempSubdirectory();
        string file = Path.Combine(folder.FullName, "slabs.3dm");
        Brep slab = Assert.IsType<Brep>(Slabs.Extrude(new Rectangle3d(Plane.WorldXY, 2, 1).ToNurbsCurve(), 0.3, Tolerance));
        try {
            var placed = await RhinoTestHost.OnMainThread(() => {
                using RhinoDoc doc = RhinoDoc.CreateHeadless(null);
                Guid[] ids = Slabs.Place(doc, [slab, slab.DuplicateBrep()], "Slabs");
                bool valid = doc.Audit(new TextLog(), attemptRepair: false);
                string[] undo = [.. doc.GetUndoRecords().Select(static record => record.Description)];
                bool written = doc.Write3dmFile(file, new FileWriteOptions { SuppressDialogBoxes = true });
                bool undone = doc.Undo();
                return (ids, valid, undo, written, undone, remaining: doc.Objects.ObjectCount(new ObjectEnumeratorSettings()));
            });
            Assert.All(placed.ids, static id => Assert.NotEqual(Guid.Empty, id));
            Assert.True(placed.valid);
            Assert.Equal(["Place Slabs"], placed.undo);
            Assert.True(placed.undone);
            Assert.Equal(0, placed.remaining);
            Assert.True(placed.written);
            using File3dm saved = File3dm.Read(file);
            Assert.Contains(saved.AllLayers, static layer => layer.Name == "Slabs");
            Assert.Equal(2, saved.Objects.Count(static item => item.Geometry is Brep { IsSolid: true }));
        } finally {
            folder.Delete(recursive: true);
        }
    }

    // --- [INTERFACE]

    public static bool Interfaced => RhinoProcess.Mode is RhinoMode.Interface;

    [Fact(Skip = "configuration Headless runs Rhino without views", SkipUnless = nameof(Interfaced))]
    public async Task PlacedSlabChangesTheViewItIsDrawnIn() {
        Brep slab = Assert.IsType<Brep>(Slabs.Extrude(new Rectangle3d(Plane.WorldXY, 4, 2).ToNurbsCurve(), 1, Tolerance));
        var (doc, view) = await RhinoTestHost.OnMainThread(() => {
            RhinoDoc doc = RhinoProcess.Untitled() ?? throw new InvalidOperationException("untitled document");
            RhinoView view = doc.Views.Add(nameof(PlacedSlabChangesTheViewItIsDrawnIn), DefinedViewportProjection.Perspective, new Rectangle(0, 0, 600, 400), floating: true);
            view.ActiveViewport.ZoomBoundingBox(slab.GetBoundingBox(accurate: true));
            view.Redraw();
            return (doc, view);
        });
        int[] empty = await RhinoTestHost.OnMainThread(() => Pixels(view));
        Guid placed = await RhinoTestHost.OnMainThread(() => {
            Guid id = Assert.Single(Slabs.Place(doc, [slab], "Slabs"));
            view.Redraw();
            return id;
        });
        int[] drawn = await RhinoTestHost.OnMainThread(() => {
            doc.Modified = false;
            return Pixels(view);
        });
        Assert.False(RhinoApp.IsRunningHeadless);
        Assert.NotEqual(Guid.Empty, placed);
        Assert.NotEqual(0, empty.Zip(drawn).Count(static pair => pair.First != pair.Second));
    }

    private static int[] Pixels(RhinoView view) {
        using Bitmap captured = view.CaptureToBitmap(new Size(600, 400));
        return [.. Enumerable.Range(0, 600 * 400).Where(static at => at % 7 == 0).Select(at => captured.GetPixel(at % 600, at / 600).ToArgb())];
    }
}
