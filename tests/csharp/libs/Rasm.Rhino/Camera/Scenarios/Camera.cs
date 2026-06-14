using System.Diagnostics.CodeAnalysis;
using Rasm.TestKit.Scenarios;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Tests.Camera.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Camera theme — named-view save/list/restore/read, stack history, DOF
// persistence with a raw NamedViews oracle, raster capture, and the four-flag restore policy
// scoped to the native restore call.
internal static class CameraScenarios {
    [RhinoScenario(theme: "camera")]
    internal static Fin<Unit> NamedViewRail(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        let camera = RhinoCamera.Live(document: scope.Doc, mode: RunMode.Scripted)
        let target = (ViewportTarget)new ViewportTarget.Current()
        from brep in BoxBrep(x0: -20.0, x1: 20.0, y0: -20.0, y1: 20.0, z0: 0.0, z1: 10.0)
        let boxId = scope.Doc.Objects.Add(brep)
        let redrawn = Redraw(doc: scope.Doc)
        let viewName = Stamp(stem: "RasmVerifyNamed")
        let viewNameFact = Note(ctx: ctx, key: "viewName", value: viewName)
        from saved in ctx.Expect(label: "save named", projection: camera.RunValue(operation: CameraOps.SaveNamed(name: viewName), target: target))
        let savedKindFact = Note(ctx: ctx, key: "saved.kind", value: Text(value: saved.Kind))
        let savedNameFact = Note(ctx: ctx, key: "saved.name", value: saved.Name)
        from savedLaw in ctx.Require(label: "saved kind", observed: saved.Kind == DocumentResourceKind.NamedView)
        from names in ctx.Expect(label: "list named", projection: camera.RunValue(operation: CameraOps.ListNamed(), target: target))
        let namedCountFact = Note(ctx: ctx, key: "named.count", value: names.Count)
        from listedLaw in ctx.Require(label: "named view listed", observed: names.Exists(name => string.Equals(a: name, b: viewName, comparisonType: StringComparison.Ordinal)))
        from restored in ctx.Expect(label: "restore named", projection: camera.RunValue(operation: CameraOps.RestoreNamed(name: viewName), target: target))
        from zoomReceipt in ctx.Expect(label: "zoom change", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.Zoom()), target: target))
        let zoomFact = Note(ctx: ctx, key: "receipt.redraw", value: zoomReceipt.RedrawRequested)
        from boundary in ctx.Expect(label: "stack pop at empty boundary", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPop)), target: target))
        let boundaryFact = Note(ctx: ctx, key: "stack.boundary.redraw", value: boundary.RedrawRequested)
        from boundaryLaw in ctx.Require(label: "boundary pop benign no-op", observed: !boundary.RedrawRequested)
        from pushed in ctx.Expect(label: "stack push", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPush)), target: target))
        let pushFact = Note(ctx: ctx, key: "stack.push.redraw", value: pushed.RedrawRequested)
        from pushLaw in ctx.Require(label: "push requests redraw", observed: pushed.RedrawRequested)
        from popped in ctx.Expect(label: "stack pop after push", projection: camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPop)), target: target))
        let popFact = Note(ctx: ctx, key: "stack.pop.redraw", value: popped.RedrawRequested)
        from dofChecked in DofAndReadNamed(ctx: ctx, camera: camera, target: target, doc: scope.Doc)
        let redrawnAgain = Redraw(doc: scope.Doc)
        from rasterChecked in RasterCapture(ctx: ctx, camera: camera, target: target)
        from fourFlag in FourFlagRestore(ctx: ctx, camera: camera, target: target, viewName: viewName)
        select Done(scope: scope);

    // BOUNDARY ADAPTER — CA2000: ownership of the transient brep transfers into the document table.
    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "Ownership transfers into the Fin rail; the document copies the geometry and the transient brep is finalizer-released.")]
    private static Fin<Brep> BoxBrep(double x0, double x1, double y0, double y1, double z0, double z1) =>
        Optional(Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: x0, t1: x1),
            new Interval(t0: y0, t1: y1),
            new Interval(t0: z0, t1: z1))))
        .ToFin(Fail: Error.New(message: "box brep construction failed"));

    private static Fin<Unit> DofAndReadNamed(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, RhinoDoc doc) {
        string dofName = Stamp(stem: "RasmVerifyDof");
        CameraDof testDof = new(
            Mode: ViewInfoFocalBlurModes.Manual,
            Distance: 42.5,
            Aperture: 2.8,
            Jitter: 0.3,
            SampleCount: 16u);
        return
            from dofSaved in ctx.Expect(label: "save named with dof", projection: camera.RunValue(operation: CameraOps.SaveNamed(name: dofName, fullView: true, dof: testDof), target: target))
            let dofIndex = doc.NamedViews.FindByName(dofName)
            from dofFound in ctx.Require(label: "dof named view found", observed: dofIndex >= 0)
            from inspected in Bracket(acquire: Fin.Succ(value: doc.NamedViews[dofIndex]), use: dofView =>
                from modeFact in Fin.Succ(value: Note(ctx: ctx, key: "dof.mode", value: Text(value: dofView.FocalBlurMode)))
                let distanceFact = Note(ctx: ctx, key: "dof.distance", value: dofView.FocalBlurDistance)
                let sampleFact = Note(ctx: ctx, key: "dof.sampleCount", value: dofView.FocalBlurSampleCount)
                from modeLaw in ctx.Require(label: "dof mode persisted", observed: dofView.FocalBlurMode == ViewInfoFocalBlurModes.Manual)
                from distanceLaw in ctx.Require(label: "dof distance persisted", observed: Math.Abs(value: dofView.FocalBlurDistance - 42.5) < 1e-9)
                from readFrame in ctx.Expect(label: "read named camera", projection: camera.RunValue(operation: CameraOps.ReadNamed(name: dofName), target: target))
                let oracle = dofView.Viewport
                let locationFact = Note(ctx: ctx, key: "read.location", value: $"{readFrame.Location.X:F3},{readFrame.Location.Y:F3},{readFrame.Location.Z:F3}")
                from frameLaw in ctx.Require(label: "read frame valid", observed: readFrame.Frame.IsValid)
                from locationLaw in ctx.Require(label: "read location matches saved-view oracle", observed: readFrame.Location.DistanceTo(other: oracle.CameraLocation) < 1e-6)
                select unit)
            select unit;
    }

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    // BOUNDARY ADAPTER — global ApplicationSettings flags are process state; the try/finally
    // bracket restores the operator's configuration regardless of the rail outcome.
    private static Fin<Unit> FourFlagRestore(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target, string viewName) {
        bool savedCPlane = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane;
        bool savedProjection = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection;
        bool savedClipping = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes;
        bool savedDisplay = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode;
        try {
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = false;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = false;
            return
                from restored in ctx.Expect(
                    label: "restore named with four-flag policy",
                    projection: camera.RunValue(operation: CameraOps.RestoreNamed(name: viewName, restore: new NamedRestorePolicy(CPlane: true, Projection: true, Clipping: true, Display: true)), target: target))
                let cplaneFact = Note(ctx: ctx, key: "restore.cplane.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane)
                let projectionFact = Note(ctx: ctx, key: "restore.projection.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection)
                let clippingFact = Note(ctx: ctx, key: "restore.clipping.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes)
                let displayFact = Note(ctx: ctx, key: "restore.display.after", value: global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode)
                from flagLaw in ctx.Require(
                    label: "restore policy restores global flags after native restore",
                    observed: !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes
                        && !global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode)
                select unit;
        } finally {
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = savedCPlane;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = savedProjection;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = savedClipping;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = savedDisplay;
        }
    }

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static Fin<Unit> RasterCapture(ScenarioContext ctx, RhinoCamera camera, ViewportTarget target) =>
        Bracket(
            acquire: ctx.Expect(label: "raster capture", projection: camera.RunValue(
                operation: CameraOps.CaptureFrame(format: CaptureFormat.Bitmap, recipe: new CaptureRecipe(Size: Some(new System.Drawing.Size(width: 320, height: 240)), Dpi: Some(96d), Raster: true)),
                target: target)),
            use: raster =>
                from isBitmap in ctx.Require(label: "raster capture is bitmap", observed: raster is CaptureResult.Bitmap)
                from bitmap in raster is CaptureResult.Bitmap admitted
                    ? Fin.Succ(value: admitted)
                    : Fin.Fail<CaptureResult.Bitmap>(error: Error.New(message: "raster capture is not a bitmap"))
                let widthFact = Note(ctx: ctx, key: "raster.width", value: bitmap.Value.Width)
                let heightFact = Note(ctx: ctx, key: "raster.height", value: bitmap.Value.Height)
                from nonEmpty in ctx.Require(label: "raster bitmap non-empty", observed: bitmap.Value.Width > 0 && bitmap.Value.Height > 0)
                select unit);

    private static Unit Redraw(RhinoDoc doc) {
        doc.Views.Redraw();
        return unit;
    }

    private static string Stamp(string stem) =>
        $"{stem}{Guid.NewGuid():N}";

    private static string Text(object? value) =>
        Convert.ToString(value: value, provider: System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

    // BOUNDARY ADAPTER — native handle bracket: disposes the acquired resource on both rails so
    // failed Require gates cannot leak ViewInfo/CaptureResult handles.
    private static Fin<T> Bracket<THandle, T>(Fin<THandle> acquire, Func<THandle, Fin<T>> use) where THandle : IDisposable {
        if (acquire is not Fin<THandle>.Succ(THandle handle)) {
            return acquire is Fin<THandle>.Fail(Error fault)
                ? Fin.Fail<T>(error: fault)
                : Fin.Fail<T>(error: Error.New(message: "bracket acquisition unresolved"));
        }
        try {
            return use(handle);
        } finally {
            handle.Dispose();
        }
    }
}
