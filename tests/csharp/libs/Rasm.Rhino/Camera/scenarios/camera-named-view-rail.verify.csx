using System;
using LanguageExt;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using static LanguageExt.Prelude;

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
});
