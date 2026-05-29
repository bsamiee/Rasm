using System;
using Rasm.Grasshopper.UI;
using Rhino;

Scenario.Run("gh-ui-canvas-runtime", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    // Canvas-policy ops require an open editor; CanvasOp.Interaction does not force one, so open it first
    // (mirrors gh-ui-wire-runtime / gh-ui-p1-wiring-bundle).
    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");

    CanvasInteractionPolicy scopePolicy = CanvasInteractionPolicy.Create(
        allowPan: true,
        allowZoom: true,
        showTilesWhenEmpty: true,
        windowSelectObjects: true,
        windowSelectWires: false,
        windowSelectGroups: true,
        viewportDragging: default,
        actions: default,
        projection: default,
        clearSnapFeedback: false);
    CanvasResult interaction = Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Interaction(policy: scopePolicy))),
        label: "interaction");
    CanvasInteractionSnapshot interactionSnapshot = interaction switch {
        CanvasResult.InteractionResult value => value.Interaction,
        _ => throw new InvalidOperationException(message: $"unexpected interaction result: {interaction.GetType().Name}"),
    };
    facts.Add("interaction.windowSelectObjects", interactionSnapshot.After.WindowSelectObjects);
    facts.Add("interaction.windowSelectWires", interactionSnapshot.After.WindowSelectWires);
    facts.Add("interaction.windowSelectGroups", interactionSnapshot.After.WindowSelectGroups);
    Probe.Require(
        condition: interactionSnapshot.After.WindowSelectObjects && !interactionSnapshot.After.WindowSelectWires && interactionSnapshot.After.WindowSelectGroups,
        message: "interaction window-select flags");

    CanvasResult baselineSnapshot = Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Snapshot())),
        label: "baseline snapshot");
    CanvasSnapshot canvas = baselineSnapshot switch {
        CanvasResult.SnapshotResult value => value.Snapshot,
        _ => throw new InvalidOperationException(message: $"unexpected snapshot result: {baselineSnapshot.GetType().Name}"),
    };
    facts.Add("canvas.hasEditor", canvas.HasEditor);
    facts.Add("canvas.hasDocument", canvas.HasDocument);
    facts.Add("canvas.windowSelectObjects", canvas.WindowSelectObjects);
    facts.Add("canvas.windowSelectWires", canvas.WindowSelectWires);
    facts.Add("canvas.windowSelectGroups", canvas.WindowSelectGroups);
    facts.Add("canvas.logicalPixelSize", canvas.LogicalPixelSize);
    facts.Add("canvas.pointsPerPixel", canvas.PointsPerPixel);
    Probe.Require(
        condition: canvas.LogicalPixelSize > 0f && canvas.PointsPerPixel > 0f && MathF.Abs(canvas.PointsPerPixel * canvas.LogicalPixelSize - 1f) < 0.01f,
        message: "canvas logical DPI reciprocity");

    // De-GH2 WindowSelect: the visible-frame RectangleF + library SelectionMode are the whole surface;
    // no Grasshopper2.* type is named, proving the host-filtered scenario can drive window selection.
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
        facts.Add($"windowSelect.{label}.scope", scope.ToString());
        facts.Add($"windowSelect.{label}.mode", mode.ToString());
        facts.Add($"windowSelect.{label}.selected", windowSnapshot.SelectedCount);
        facts.Add($"windowSelect.{label}.deselected", windowSnapshot.DeselectedCount);
        facts.Add($"windowSelect.{label}.canvas.windowSelectObjects", windowSnapshot.Canvas.WindowSelectObjects);
        facts.Add($"windowSelect.{label}.canvas.windowSelectWires", windowSnapshot.Canvas.WindowSelectWires);
        facts.Add($"windowSelect.{label}.canvas.windowSelectGroups", windowSnapshot.Canvas.WindowSelectGroups);
    }

    CanvasResult invalidate = Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))),
        label: "invalidate");
    facts.Add("invalidate.unit", invalidate is CanvasResult.UnitResult);

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
});
