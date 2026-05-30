using System;
using Rasm.Domain;
using Rasm.Rhino.UI;
using Rasm.Rhino.Commands;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Display;
using Rhino.Geometry;

Scenario.Run("ui-motion-overlay", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    RhinoView view = scope.Active.Views.ActiveView ?? throw new InvalidOperationException(message: "no active view");
    RhinoCommandContext context = Probe.Expect(RhinoCommandContext.Of(doc: scope.Active, mode: RunMode.Scripted), "context", facts);
    RhinoUi ui = context.Ui;

    // Decay handle: returned, initial value emitted to the sink, CAS retarget of velocity, velocity read-back.
    double decaySunk = double.NaN;
    MotionHandle<double, double> decay = Probe.Expect(
        ui.Use(UiViewportRequest.Animate<double, double>(
            spec: MotionSpec.Decay<double, double>(from: 0.0, velocity: 10.0, friction: 5.0, vector: MotionVector.Double),
            view: view, sink: value => decaySunk = value)),
        "decay animate handle",
        facts);
    using (decay) {
        facts.Add("decay.initialSunk", decaySunk);
        _ = Probe.Expect(decay.Retarget(target: 0.0, velocity: Some(20.0)), "decay retarget", facts);
        facts.Add("decay.velocity", decay.Velocity);
        Probe.Require(!double.IsNaN(decaySunk), "decay sink received an initial value");
    }

    // Spring handle: live retarget toward a new target (CAS the runner cell, no driver restart).
    MotionHandle<double, double> spring = Probe.Expect(
        ui.Use(UiViewportRequest.Animate<double, double>(
            spec: MotionSpec.Spring<double, double>(from: 0.0, to: 100.0, config: SpringPreset.Snappy.Config, vector: MotionVector.Double),
            view: view, sink: _ => { })),
        "spring animate handle",
        facts);
    using (spring) {
        _ = Probe.Expect(spring.Retarget(target: 50.0), "spring retarget", facts);
        facts.Add("spring.velocity", spring.Velocity);
    }

    // OverlayFilter monoid composed then applied through a live display conduit; Reset clears it.
    OverlayFilter composed =
        new OverlayFilter(Geometry: Some(ObjectType.Curve))
        + new OverlayFilter(Space: Some(ActiveSpace.ModelSpace));
    facts.Add("filter.geometry", composed.Geometry.ToString());
    _ = Probe.Expect(
        ui.Use(UiViewportRequest.Preview<Unit>(
            preview: UiViewportPreview.Empty,
            run: previewScope =>
                from bound in previewScope.Overlay.Filter(filter: composed, document: scope.Active)
                from cleared in previewScope.Overlay.Filter(filter: composed + OverlayFilter.Reset, document: scope.Active)
                select cleared,
            interactive: false)),
        "overlay filter compose + reset",
        facts);
    facts.Add("filter.apply.ok", true);

    // Gumball frame update through the live conduit.
    BoundingBox region = new BoundingBox(min: new Point3d(0.0, 0.0, 0.0), max: new Point3d(8.0, 5.0, 3.0));
    bool changed = Probe.Expect(
        ui.Use(UiIntent.Gumball<bool>(
            spec: UiGumballSpec.Of(source: region),
            run: gumball => gumball.Update(frame: new Plane(origin: new Point3d(1.0, 1.0, 0.0), normal: Vector3d.ZAxis)),
            interactive: false)),
        "gumball frame update",
        facts);
    facts.Add("gumball.changed", changed);
});
