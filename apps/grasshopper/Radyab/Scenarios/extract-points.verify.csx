// SCENARIO_NAME and CAPTURE_PATH are prepended by scripts/rhino.sh verify.
// Do NOT declare these constants here.
using System;
using System.IO;
using System.Linq;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;

var doc = RhinoDoc.ActiveDoc ?? throw new InvalidOperationException("No active Rhino document.");
doc.Objects.Clear();

var sphere = new Sphere(Point3d.Origin, 5.0);
var brep = sphere.ToBrep() ?? throw new InvalidOperationException("Sphere.ToBrep returned null.");
if (doc.Objects.AddBrep(brep) == Guid.Empty) throw new InvalidOperationException("AddBrep returned Guid.Empty.");

var bbox = brep.GetBoundingBox(accurate: true);
var diagonal = bbox.Diagonal.Length;
var expectedDiagonal = new Vector3d(10.0, 10.0, 10.0).Length;
if (Math.Abs(diagonal - expectedDiagonal) > 1e-6) throw new InvalidOperationException($"bbox diagonal {diagonal} != {expectedDiagonal}");

var center = bbox.Center;
if (center.DistanceTo(Point3d.Origin) > 1e-6) throw new InvalidOperationException($"bbox center {center} != origin");

var cornerCount = bbox.GetCorners().Distinct().Count();
if (cornerCount != 8) throw new InvalidOperationException($"unique bbox corners {cornerCount} != 8");

var view = doc.Views.ActiveView ?? throw new InvalidOperationException("No active view (required for macOS ViewCapture).");
view.ActiveViewport.ZoomExtents();
view.Redraw();
Directory.CreateDirectory(Path.GetDirectoryName(CAPTURE_PATH)!);
using var bmp = new ViewCapture { Width = 1024, Height = 768, TransparentBackground = false }.CaptureToBitmap(view)
    ?? throw new InvalidOperationException("ViewCapture returned null.");
bmp.Save(CAPTURE_PATH);

Console.WriteLine($"scenario={SCENARIO_NAME}");
Console.WriteLine($"bbox.diagonal={diagonal:F6}");
Console.WriteLine($"bbox.center={center}");
Console.WriteLine($"bbox.corners={cornerCount}");
Console.WriteLine($"capture={CAPTURE_PATH}");
