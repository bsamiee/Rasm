using System;
using Rasm.Grasshopper.UI;
using Rhino;
using GhDuration = Grasshopper2.UI.Animation.Duration;

// Crash-free core: the editor opens with NO visible window (EditorOp.Show(visible:false), never EnsureVisible) so the
// GH2 StatusBar.DrawStateField NPE is never armed; State reads back; canvas ops run only when the headless editor
// exposes a canvas (the §1 gate, decided at runtime by the Snapshot/State signal); SmartEnum vocabularies and the new
// cosmetic factories (multi-stop gradient, text font, particle emitter) construct in the live GH2 ALC.
Scenario.Run("gh-ui-core", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.Show(visible: false))), label: "show editor headless");

    EditorSnapshot editor = Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)), label: "editor state") switch {
        EditorResult.StateResult value => value.Snapshot,
        EditorResult other => throw new InvalidOperationException(message: $"unexpected editor result: {other.GetType().Name}"),
    };
    facts.Add("editor.hasEditor", editor.HasEditor);
    facts.Add("editor.hasCanvas", editor.HasCanvas);
    facts.Add("editor.hasDocument", editor.HasDocument);
    Probe.Require(condition: editor.HasEditor, message: "headless Show(visible:false) creates the editor without a deferred abort");

    Unit RunCanvasOps() {
        DocumentMutation[] mutations = new[] {
            DocumentMutation.Selection(op: SelectionOp.All),
            DocumentMutation.Target(subject: ObjectScope.Selection, op: DocumentTargetOp.Delete()),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "Rasm", state: true), location: new PointF(-80f, 0f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "Core", state: false), location: new PointF(80f, 0f)),
        };
        DocumentMutationDelta populated = Probe.Expect(
            result: ui.Use(intent: GhUi.Document(op: DocumentOp.Mutate(mutations: mutations))),
            label: "populate canvas") switch {
            DocumentResult.MutationResult value => value.Delta.Payload,
            DocumentResult other => throw new InvalidOperationException(message: $"unexpected mutate result: {other.GetType().Name}"),
        };
        facts.Add("canvas.objectCount", populated.After.ObjectCount);
        facts.Add("canvas.attributeBounds", populated.After.AttributeBounds.ToString());
        Probe.Require(condition: populated.After.ObjectCount == 2 && populated.After.Modifications > 0, message: "clear-then-place lands exactly two toggles on the headless canvas and records modifications");

        Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Solution))), label: "run solution");
        DocumentSnapshot solved = Probe.Expect(result: ui.Use(intent: GhUi.Document(op: DocumentOp.Query(query: DocumentQuery.Snapshot))), label: "post-solution document") switch {
            DocumentResult.SnapshotResult value => value.Snapshot,
            DocumentResult other => throw new InvalidOperationException(message: $"unexpected document result: {other.GetType().Name}"),
        };
        facts.Add("solution.objectCount", solved.ObjectCount);
        facts.Add("solution.modifications", solved.Modifications);
        Probe.Require(condition: solved.ObjectCount == 2, message: "the document retains both objects after a headless solution pass");

        CanvasSnapshot snapshot = Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Snapshot())), label: "canvas snapshot") switch {
            CanvasResult.SnapshotResult value => value.Snapshot,
            CanvasResult other => throw new InvalidOperationException(message: $"unexpected snapshot result: {other.GetType().Name}"),
        };
        facts.Add("canvas.logicalPixelSize", snapshot.LogicalPixelSize);

        CanvasBitmap bitmap = Probe.Expect(
            result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Render(width: 400, height: 200, layers: CanvasBitmapLayers.All))),
            label: "offscreen render") switch {
            CanvasResult.BitmapResult value => value.Bitmap,
            CanvasResult other => throw new InvalidOperationException(message: $"unexpected render result: {other.GetType().Name}"),
        };
        facts.Add("render.width", bitmap.Width);
        facts.Add("render.pngBytes", bitmap.Png.Length);
        Probe.Require(condition: bitmap.Width == 400 && bitmap.Png.Length > 0, message: "offscreen render of the populated canvas produced PNG bytes");

        DrawMark mark = DrawMark.Rectangle(bounds: new RectangleF(0f, 0f, 24f, 24f), edge: Colors.Crimson, fill: Some(Colors.SkyBlue), thickness: 2f);
        Subscription drawSub = Probe.Expect(
            result: ui.Use(intent: Paint.Hook(phase: CanvasPaintPhase.AfterObjects, paint: new DrawPlan(Marks: Seq(mark)).Apply, clock: MotionClock.MessageLoop)),
            label: "draw plan hook");
        using (drawSub) { }
        facts.Add("draw.hookAttached", true);
        return unit;
    }

    Unit SkipCanvasOps() {
        facts.Add("canvas.opsSkipped", "headless editor exposes no canvas (§1 gate)");
        return unit;
    }

    // §1 gate: canvas ops resolve only with a canvas. Some-branch failures throw (real regression); the None branch
    // records the skip explicitly so the evidence — not silence — reveals that the headless editor has no canvas.
    _ = editor.HasCanvas ? RunCanvasOps() : SkipCanvasOps();

    facts.Add("iconAdjust.count", IconAdjust.Items.Count);
    Probe.Require(condition: IconAdjust.Items.Count == 4 && IconAdjust.None.Key == 0 && IconAdjust.Faded.Key == 3, message: "IconAdjust vocabulary");
    facts.Add("dialogPresentation.count", DialogPresentation.Items.Count);
    Probe.Require(condition: DialogPresentation.Items.Count == 2 && DialogPresentation.Modal.Key == 0 && DialogPresentation.AttachedSheet.Key == 1, message: "DialogPresentation vocabulary");
    facts.Add("graphMetric.count", GraphMetric.Items.Count);
    Probe.Require(condition: GraphMetric.Items.Count == 6 && GraphMetric.Linearity.Key == 0 && GraphMetric.RelayCollapsed.Key == 3 && GraphMetric.Paths.Key == 4 && GraphMetric.Integrity.Key == 5, message: "GraphMetric vocabulary");
    facts.Add("emitterShape.count", EmitterShape.Items.Count);
    Probe.Require(condition: EmitterShape.Items.Count == 6 && EmitterShape.Circle.Key == 4, message: "EmitterShape vocabulary");

    CosmeticIntent gradient = CosmeticIntent.Gradient(
        bounds: new RectangleF(0f, 0f, 40f, 12f),
        colors: Seq(Colors.Black, Colors.Orange, Colors.White),
        kind: GradientKind.Axial,
        duration: GhDuration.Fast,
        points: CosmeticGradientPoints.Stops(locations: new float[] { 0f, 0.5f, 1f }));
    Probe.Require(condition: gradient is CosmeticIntent.GradientCase g && g.Colors.Count == 3 && g.Points.Locations.IsSome, message: "multi-stop gradient factory carries 3 colours + stops");
    facts.Add("cosmetic.gradientColours", ((CosmeticIntent.GradientCase)gradient).Colors.Count);

    CosmeticIntent text = new CosmeticIntent.TextLayerCase(Text: "Rasm", Origin: new PointF(4f, 4f), Tint: Colors.White, FontSize: 12f, Duration: GhDuration.Normal, FontFamily: Some("Helvetica Neue"));
    Probe.Require(condition: text is CosmeticIntent.TextLayerCase t && t.FontFamily.IsSome, message: "text-layer factory carries a font family");
    facts.Add("cosmetic.textHasFont", ((CosmeticIntent.TextLayerCase)text).FontFamily.IsSome);

    CosmeticIntent emitter = CosmeticIntent.Emitter(bounds: new RectangleF(0f, 0f, 30f, 30f), tint: Colors.Gold, duration: GhDuration.Slow, shape: EmitterShape.Circle, birthRate: 24f);
    Probe.Require(condition: emitter is CosmeticIntent.EmitterCase e && e.Shape == EmitterShape.Circle && e.BirthRate == 24f, message: "emitter factory carries shape + birth rate");
    facts.Add("cosmetic.emitterShape", ((CosmeticIntent.EmitterCase)emitter).Shape.ToString());

    // Subscription monoid runtime smoke; the full algebra (identity/associativity/LIFO/teardown) is owned by Subscription.spec.cs.
    System.Collections.Generic.List<int> order = new();
    Subscription composed = Subscription.Atom(detach: () => order.Add(item: 1)) | Subscription.Atom(detach: () => order.Add(item: 2));
    composed.Dispose();
    Probe.Require(condition: order.Count == 2 && order[0] == 2 && order[1] == 1, message: "subscription composite tears down LIFO at runtime");
    facts.Add("subscription.lifoOrder", string.Join(separator: ",", values: order));

    facts.Add("repaint.canvasBeatsScheduled", (RepaintRequest.Scheduled | RepaintRequest.Canvas) is RepaintRequest.CanvasCase);
    Probe.Require(condition: (RepaintRequest.Scheduled | RepaintRequest.Canvas) is RepaintRequest.CanvasCase, message: "repaint join absorbs scheduled into canvas");
});
