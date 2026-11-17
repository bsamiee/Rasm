using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Testing;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Xunit;

namespace Rasm.Rhino.Tests.Host;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RhinoProcessTests(Fixtures fixtures) {

    // --- [INTERFACE]

    public static bool Interfaced => RhinoProcess.Mode is RhinoMode.Interface;

    [Fact(Skip = "configuration Headless runs Rhino without views", SkipUnless = nameof(Interfaced))]
    public async Task InterfaceModeDrawsTheModelAndRunsScriptedCommands() {
        RhinoDoc doc = await fixtures.TakeAsync(RhinoHost.Document);
        (bool headless, Guid sphere, int changed, Seq<double> lines) = await RhinoHost.RunAsync((
            from view in IO.lift(() => doc.Views.Add(nameof(RhinoProcessTests), DefinedViewportProjection.Perspective, new Rectangle(0, 0, 600, 400), floating: true))
            from framed in Commits.WithinRedraw(doc, new RedrawPolicy.View(view), Products.Accepted(() => view.ActiveViewport.ZoomBoundingBox(new BoundingBox(-6, -6, -6, 6, 6, 6))))
            from empty in IO.lift(() => Captured(view))
            from added in IO.lift(() => doc.Objects.AddSphere(new Sphere(Point3d.Origin, 5)))
            from drawn in Commits.WithinRedraw(doc, new RedrawPolicy.View(view), IO.pure(unit))
            from shown in IO.lift(() => Captured(view))
            from scripted in Products.Accepted(() => RhinoApp.RunScript(doc.RuntimeSerialNumber, "-_Line 0,0,0 10,0,0", echo: false))
            select (RhinoApp.IsRunningHeadless, added, empty.Zip(shown).Count(static pair => pair.First != pair.Second), toSeq(doc.Objects.FindByObjectType(ObjectType.Curve)).Map(static found => ((Curve)found.Geometry).GetLength()).Strict())).OnUiThread());
        Assert.False(headless);
        Assert.NotEqual(Guid.Empty, sphere);
        Assert.InRange(changed, 100, int.MaxValue);
        Assert.Equal(10d, Assert.Single(lines), 1e-9);
    }

    // --- [CAPTURES]

    private static int[] Captured(RhinoView view) {
        using Bitmap captured = view.CaptureToBitmap(new Size(600, 400));
        return [.. from x in Enumerable.Range(0, captured.Width) from y in Enumerable.Range(0, captured.Height) select captured.GetPixel(x, y).ToArgb()];
    }
}
