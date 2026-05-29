using System;
using Rasm.Grasshopper.UI;
using Rhino;

// Consolidates gh-ui-canvas-runtime + gh-ui-repaint-absorption and adds the Section-C draw surface:
// Bezier/Curve/Polygon DrawMark factories, the IconAdjust SmartEnum sweep, and a DrawPlan paint hook
// rendered through CanvasOp.Render. Library-types-only: no Grasshopper2.* type is named.
Scenario.Run("gh-ui-canvas", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");

    CanvasResult baselineSnapshot = Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Snapshot())),
        label: "baseline snapshot");
    CanvasSnapshot canvas = baselineSnapshot switch {
        CanvasResult.SnapshotResult value => value.Snapshot,
        _ => throw new InvalidOperationException(message: $"unexpected snapshot result: {baselineSnapshot.GetType().Name}"),
    };
    facts.Add("canvas.hasEditor", canvas.HasEditor);
    facts.Add("canvas.logicalPixelSize", canvas.LogicalPixelSize);
    facts.Add("canvas.pointsPerPixel", canvas.PointsPerPixel);
    Probe.Require(
        condition: canvas.LogicalPixelSize > 0f && canvas.PointsPerPixel > 0f && MathF.Abs(canvas.PointsPerPixel * canvas.LogicalPixelSize - 1f) < 0.01f,
        message: "canvas logical DPI reciprocity");

    // De-GH2 WindowSelect: visible-frame RectangleF + library SelectionMode drive selection with no host type.
    foreach ((string label, CanvasWindowScope scope, SelectionMode mode) in new (string, CanvasWindowScope, SelectionMode)[] {
        ("objects", CanvasWindowScope.Objects, SelectionMode.Promote),
        ("wires", CanvasWindowScope.Wires, SelectionMode.Include),
        ("groups", CanvasWindowScope.Groups, SelectionMode.Exclude),
        ("objectsAndWires", CanvasWindowScope.ObjectsAndWires, SelectionMode.Inverse),
    }) {
        CanvasResult result = Probe.Expect(
            result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.WindowSelect(window: canvas.VisibleFrame, mode: mode, scope: scope))),
            label: $"window select {label}");
        CanvasWindowSnapshot windowSnapshot = result switch {
            CanvasResult.WindowResult value => value.Window,
            _ => throw new InvalidOperationException(message: $"unexpected window result: {result.GetType().Name}"),
        };
        facts.Add($"windowSelect.{label}.selected", windowSnapshot.SelectedCount);
        facts.Add($"windowSelect.{label}.deselected", windowSnapshot.DeselectedCount);
    }

    // IconAdjust SmartEnum sweep: None/Disabled/Greyscale/Faded each carry a distinct key + colour filter.
    facts.Add("iconAdjust.count", IconAdjust.Items.Count);
    foreach (IconAdjust adjust in IconAdjust.Items) {
        facts.Add($"iconAdjust.{adjust}.key", adjust.Key);
    }
    Probe.Require(
        condition: IconAdjust.Items.Count == 4
            && IconAdjust.None.Key == 0 && IconAdjust.Disabled.Key == 1
            && IconAdjust.Greyscale.Key == 2 && IconAdjust.Faded.Key == 3,
        message: "IconAdjust items + keys");

    // Bezier/Curve/Polygon factory -> case wiring (the Apply Switch arms execute when the canvas paints).
    DrawMark bezier = DrawMark.Bezier(start: new PointF(0f, 0f), control1: new PointF(10f, 20f), control2: new PointF(20f, 20f), end: new PointF(30f, 0f), edge: Colors.Black, thickness: 1.5f);
    DrawMark curve = DrawMark.Curve(points: new PointF[] { new(0f, 0f), new(10f, 12f), new(20f, 0f) }, edge: Colors.SteelBlue, tension: 0.5f);
    DrawMark polygon = DrawMark.Polygon(points: new PointF[] { new(0f, 0f), new(12f, 0f), new(6f, 12f) }, edge: Colors.Black, fill: Some(Colors.Gray));
    Probe.Require(condition: bezier is DrawMark.BezierCase && curve is DrawMark.CurveCase && polygon is DrawMark.PolygonCase, message: "draw mark factory cases");
    facts.Add("draw.bezierCase", bezier is DrawMark.BezierCase);
    facts.Add("draw.curveCase", curve is DrawMark.CurveCase);
    facts.Add("draw.polygonCase", polygon is DrawMark.PolygonCase);

    DrawPlan plan = new(Marks: Seq(bezier, curve, polygon));
    Subscription drawSub = Probe.Expect(
        result: ui.Use(intent: GhUi.Paint(request: new PaintRequest<Subscription>.Hook(Phase: CanvasPaintPhase.AfterObjects, Plan: plan))),
        label: "draw plan hook");
    facts.Add("draw.planMarks", plan.Marks.Count);

    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))), label: "scheduled invalidate");
    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Canvas))), label: "canvas invalidate");

    CanvasResult render = Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Render(width: 64, height: 64, layers: CanvasBitmapLayers.Background | CanvasBitmapLayers.Wires))),
        label: "render");
    CanvasBitmap bitmap = render switch {
        CanvasResult.BitmapResult value => value.Bitmap,
        _ => throw new InvalidOperationException(message: $"unexpected render result: {render.GetType().Name}"),
    };
    facts.Add("render.width", bitmap.Width);
    facts.Add("render.height", bitmap.Height);
    facts.Add("render.pngBytes", bitmap.Png.Length);
    Probe.Require(condition: bitmap.Width == 64 && bitmap.Height == 64 && bitmap.Png.Length > 0, message: "render bitmap evidence");

    using (drawSub) { }

    // Repaint absorption: Canvas dominates Scheduled in the RepaintRequest join lattice.
    facts.Add("repaint.canvasBeatsScheduled", (RepaintRequest.Scheduled | RepaintRequest.Canvas) is RepaintRequest.CanvasCase);
    Probe.Require(condition: (RepaintRequest.Scheduled | RepaintRequest.Canvas) is RepaintRequest.CanvasCase, message: "repaint join absorbs scheduled into canvas");
});
