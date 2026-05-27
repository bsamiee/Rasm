using System;
using System.Drawing;
using Grasshopper2.Extensions;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;

Scenario.Run("gh-ui-canvas-runtime", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    CanvasInteractionPolicy scopePolicy = CanvasInteractionPolicy.Default with {
        WindowSelectObjects = true,
        WindowSelectWires = false,
        WindowSelectGroups = true,
    };
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

    WindowSelection window = new(
        left: canvas.VisibleFrame.Left,
        top: canvas.VisibleFrame.Top,
        right: canvas.VisibleFrame.Right,
        bottom: canvas.VisibleFrame.Bottom);

    foreach ((string label, CanvasWindowScope scope) in new (string, CanvasWindowScope)[] {
        ("objects", CanvasWindowScope.Objects),
        ("wires", CanvasWindowScope.Wires),
        ("groups", CanvasWindowScope.Groups),
        ("objectsAndWires", CanvasWindowScope.ObjectsAndWires),
    }) {
        CanvasResult result = Probe.Expect(
            result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.WindowSelect(window: window, mode: SelectionMode.Replace, scope: scope))),
            label: $"window select {label}");
        CanvasWindowSnapshot windowSnapshot = result switch {
            CanvasResult.WindowResult value => value.Window,
            _ => throw new InvalidOperationException(message: $"unexpected window result: {result.GetType().Name}"),
        };
        facts.Add($"windowSelect.{label}.scope", scope.ToString());
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
