using Rasm.Grasshopper.UI;
using Rhino;

Scenario.Run("gh-ui-repaint-absorption", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))), label: "scheduled invalidate");
    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Canvas))), label: "canvas invalidate");
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    facts.Add("repaint.canvasBeatsScheduled", (RepaintRequest.Scheduled | RepaintRequest.Canvas) is RepaintRequest.CanvasCase);
});
