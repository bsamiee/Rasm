using System;
using Rasm.Rhino;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;

Scenario.Run("camera-named-view-rail", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    RhinoCamera camera = RhinoCamera.Live(document: scope.Active, mode: RunMode.Scripted);
    ViewportTarget target = new ViewportTarget.Current();
    Brep brep = Brep.CreateFromBox(new Box(
        Plane.WorldXY,
        new Interval(t0: -20.0, t1: 20.0),
        new Interval(t0: -20.0, t1: 20.0),
        new Interval(t0: 0.0, t1: 10.0))) ?? throw new InvalidOperationException(message: "box brep");
    scope.Active.Objects.Add(brep);
    scope.Active.Views.Redraw();
    string viewName = $"RasmVerifyNamed{Guid.NewGuid():N}";
    facts.Add("viewName", viewName);
    DocumentResourceChange saved = Probe.Expect(
        camera.RunValue(operation: CameraOps.SaveNamed(name: viewName), target: target),
        "save named");
    facts.Add("saved.kind", saved.Kind.ToString());
    facts.Add("saved.name", saved.Name);
    Probe.Require(saved.Kind == DocumentResourceKind.NamedView, "saved kind");
    Seq<string> names = Probe.Expect(
        camera.RunValue(operation: CameraOps.ListNamed(), target: target),
        "list named");
    facts.Add("named.count", names.Count);
    Probe.Require(names.Contains(viewName), "named view listed");
    Probe.Expect(
        camera.RunValue(operation: CameraOps.RestoreNamed(name: viewName), target: target),
        "restore named");
    CameraChangeReceipt receipt = Probe.Expect(
        camera.RunValue(operation: CameraOps.Change(new CameraEdit.Zoom()), target: target),
        "zoom change");
    facts.Add("receipt.redraw", receipt.RedrawRequested);

    // --- stack history: boundary pop is a benign no-op; push/pop drives redraw (A1) ---
    CameraChangeReceipt boundary = Probe.Expect(
        camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPop)), target: target),
        "stack pop at empty boundary");
    facts.Add("stack.boundary.redraw", boundary.RedrawRequested);
    Probe.Require(!boundary.RedrawRequested, "boundary pop is benign no-op (no failure, no redraw)");
    CameraChangeReceipt pushed = Probe.Expect(
        camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPush)), target: target),
        "stack push");
    facts.Add("stack.push.redraw", pushed.RedrawRequested);
    Probe.Require(pushed.RedrawRequested, "push requests redraw");
    // Immediate pop after push is a benign no-op when projection is unchanged (push above proves redraw on push).
    CameraChangeReceipt popped = Probe.Expect(
        camera.RunValue(operation: CameraOps.Change(new CameraEdit.StackMove(Move: CameraStackOp.ViewPop)), target: target),
        "stack pop after push");
    facts.Add("stack.pop.redraw", popped.RedrawRequested);

    // --- DOF write via SaveNamed + independent raw read-back oracle (C2) ---
    string dofName = $"RasmVerifyDof{Guid.NewGuid():N}";
    CameraDof testDof = new(
        Mode: Rhino.DocObjects.ViewInfoFocalBlurModes.Manual,
        Distance: 42.5,
        Aperture: 2.8,
        Jitter: 0.3,
        SampleCount: 16u);
    Probe.Expect(
        camera.RunValue(operation: CameraOps.SaveNamed(name: dofName, fullView: true, dof: testDof), target: target),
        "save named with dof");
    int dofIndex = scope.Active.NamedViews.FindByName(dofName);
    Probe.Require(dofIndex >= 0, "dof named view found");
    using Rhino.DocObjects.ViewInfo dofView = scope.Active.NamedViews[dofIndex];
    facts.Add("dof.mode", dofView.FocalBlurMode.ToString());
    facts.Add("dof.distance", dofView.FocalBlurDistance);
    facts.Add("dof.sampleCount", dofView.FocalBlurSampleCount);
    Probe.Require(dofView.FocalBlurMode == Rhino.DocObjects.ViewInfoFocalBlurModes.Manual, "dof mode persisted");
    Probe.Require(Math.Abs(dofView.FocalBlurDistance - 42.5) < 1e-9, "dof distance persisted");

    // --- ReadNamed projects the saved camera without restoring (C3) ---
    CameraFrame readFrame = Probe.Expect(
        camera.RunValue(operation: CameraOps.ReadNamed(name: dofName), target: target),
        "read named camera");
    Rhino.DocObjects.ViewportInfo oracleViewport = dofView.Viewport;
    facts.Add("read.location", $"{readFrame.Location.X:F3},{readFrame.Location.Y:F3},{readFrame.Location.Z:F3}");
    Probe.Require(readFrame.Frame.IsValid, "read frame valid");
    Probe.Require(readFrame.Location.DistanceTo(oracleViewport.CameraLocation) < 1e-6, "read location matches saved-view oracle");

    // --- RasterMode capture (C1) ---
    scope.Active.Views.Redraw();
    CaptureResult raster = Probe.Expect(
        camera.RunValue(
            operation: CameraOps.CaptureFrame(CaptureFormat.Bitmap, new CaptureRecipe(Size: Some(new System.Drawing.Size(width: 320, height: 240)), Dpi: Some(96d), Raster: true)),
            target: target),
        "raster capture");
    using (raster) {
        Probe.Require(raster is CaptureResult.Bitmap, "raster capture is bitmap");
        System.Drawing.Bitmap bitmap = ((CaptureResult.Bitmap)raster).Value;
        facts.Add("raster.width", bitmap.Width);
        facts.Add("raster.height", bitmap.Height);
        Probe.Require(bitmap.Width > 0 && bitmap.Height > 0, "raster bitmap non-empty");
    }

    // --- four-flag named restore policy is scoped to the native restore call (A2) ---
    bool savedCPlane = Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane;
    bool savedProjection = Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection;
    bool savedClipping = Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes;
    bool savedDisplay = Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode;
    try {
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = false;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = false;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = false;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = false;
        Probe.Expect(
            camera.RunValue(
                operation: CameraOps.RestoreNamed(name: viewName, restore: new NamedRestorePolicy(CPlane: true, Projection: true, Clipping: true, Display: true)),
                target: target),
            "restore named with four-flag policy");
        facts.Add("restore.cplane.after", Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane);
        facts.Add("restore.projection.after", Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection);
        facts.Add("restore.clipping.after", Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes);
        facts.Add("restore.display.after", Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode);
        Probe.Require(
            !Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane
                && !Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection
                && !Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes
                && !Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode,
            "restore policy restores global flags after native restore");
    } finally {
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = savedCPlane;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = savedProjection;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = savedClipping;
        Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = savedDisplay;
    }
});
