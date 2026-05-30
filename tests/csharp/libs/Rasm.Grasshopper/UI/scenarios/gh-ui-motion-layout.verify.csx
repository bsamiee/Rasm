using System;
using Rasm.Grasshopper.UI;
using Rhino;

// Runtime coverage for the Layout deepening that pure-managed specs cannot reach: ComputeGrid and the N-ary
// Align both require a live GhObjectList, so their geometry is proven here on real placed objects. Layout
// arrangements are document mutations (attributes.Move / OCD.AlignObjects) — they run SYNCHRONOUSLY in the
// bridge (no solver settle, no idle pump), so the post-arrange Measure reflects the committed positions. This
// scenario also exercises the snap-guide seam (Layout.Snap -> EmitSnapGuide -> SnapGuideCase cosmetic,
// fire-and-forget) and ObjectSpec.Scribble placement. Driven entirely through Rasm.Grasshopper.UI wrappers; no
// raw GH2 types (Align's fix defaults), union cases built via record constructors, Seqs via Seq(...) (the csx
// compiles at C# 10 — no collection expressions, no System.Linq).
Scenario.Run("gh-ui-motion-layout", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.Show(visible: false))), label: "show editor headless");
    EditorSnapshot editor = Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)), label: "editor state") switch {
        EditorResult.StateResult value => value.Snapshot,
        EditorResult other => throw new InvalidOperationException(message: $"unexpected editor result: {other.GetType().Name}"),
    };
    facts.Add("editor.hasCanvas", editor.HasCanvas);
    Probe.Require(condition: editor.HasEditor, message: "headless editor created without a deferred abort");
    // Canvas exposure is a stable headless contract (the GhEditor ctor builds the Canvas), so a regression must
    // FAIL the layout/snap ops rather than silently skip them — a hard requirement, not a gate.
    Probe.Require(condition: editor.HasCanvas, message: "headless editor must expose a canvas for the layout + snap-guide ops");

    // Clear leftovers from the long-lived endpoint, then place four deliberately-scattered toggles and capture
    // their created ids (DocumentMutationDelta.Created) for the arrangement + read-back.
    DocumentMutationDelta placed = Probe.Expect(
        result: ui.Use(intent: GhUi.Document(op: DocumentOp.Mutate(
            DocumentMutation.Selection(op: SelectionOp.All),
            DocumentMutation.Target(subject: ObjectScope.Selection, op: DocumentTargetOp.Delete()),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "A", state: true), location: new PointF(-120f, -60f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "B", state: false), location: new PointF(150f, -40f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "C", state: true), location: new PointF(-90f, 120f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "D", state: false), location: new PointF(170f, 140f))))),
        label: "place four toggles") switch {
        DocumentResult.MutationResult value => value.Delta.Payload,
        DocumentResult other => throw new InvalidOperationException(message: $"unexpected mutate result: {other.GetType().Name}"),
    };
    Seq<Guid> created = placed.Created;
    facts.Add("layout.placedCount", created.Count);
    Probe.Require(condition: created.Count == 4, message: "four toggles placed with created ids");

    // Grid(2x2): first row distributes horizontally → column lefts, first column vertically → row tops. Capture
    // the arrange delta's move count (one per object) so the test pins the COMPUTED moves, not just the measured
    // lattice — a no-op arrange would still satisfy the lattice once execution is attempted.
    int gridMoves = Probe.Expect(
        result: ui.Use(intent: GhUi.Layout(op: new LayoutOp.ArrangeCase(Arrangement: LayoutArrangement.Grid(rows: 2, cols: 2, gap: LayoutGap.Create(20f), ids: created, gapPolicy: LayoutGapPolicy.Stretch)))),
        label: "grid 2x2 arrange") switch {
        LayoutResult.MutationResult value => value.Delta.Payload.Moves.Count,
        LayoutResult other => throw new InvalidOperationException(message: $"unexpected grid result: {other.GetType().Name}"),
    };
    facts.Add("layout.gridMoves", gridMoves);
    Probe.Require(condition: gridMoves == 4, message: "Grid emits exactly one move delta per object");

    Seq<LayoutSnapshot> gridded = Probe.Expect(
        result: ui.Use(intent: GhUi.Layout(op: new LayoutOp.MeasureCase(Scope: new ObjectScope.ObjectsCase(Ids: created)))),
        label: "measure gridded objects") switch {
        LayoutResult.SnapshotsResult value => value.Snapshots,
        LayoutResult other => throw new InvalidOperationException(message: $"unexpected measure result: {other.GetType().Name}"),
    };
    Seq<int> colLefts = gridded.Map(static s => (int)MathF.Round(s.Bounds.Left)).Distinct();
    Seq<int> rowTops = gridded.Map(static s => (int)MathF.Round(s.Bounds.Top)).Distinct();
    int cells = gridded.Map(static s => ((int)MathF.Round(s.Bounds.Left), (int)MathF.Round(s.Bounds.Top))).Distinct().Count;
    facts.Add("layout.gridColumns", colLefts.Count);
    facts.Add("layout.gridRows", rowTops.Count);
    facts.Add("layout.gridCells", cells);
    Probe.Require(condition: gridded.Count == 4 && colLefts.Count == 2 && rowTops.Count == 2 && cells == 4, message: "Grid(2x2) lands four objects on a 2-column x 2-row aligned lattice");
    // Gap invariant: the two distinct column origins are meaningfully separated (>= the requested 20px gap), so
    // a swapped axis or ignored gap that collapsed the columns would fail rather than pass on a degenerate lattice.
    float columnSpan = MathF.Abs(colLefts[1] - colLefts[0]);
    facts.Add("layout.columnSpan", columnSpan);
    Probe.Require(condition: columnSpan >= 20f, message: "Grid columns are separated by at least the requested gap");

    // N-ary Align: anchor + three targets, default fix. Proves the TraverseM/UndoGroup aggregation emits exactly
    // one move delta per target and commits as a single arrangement (the generalised pairwise rail).
    int alignMoves = Probe.Expect(
        result: ui.Use(intent: GhUi.Layout(op: new LayoutOp.ArrangeCase(Arrangement: LayoutArrangement.Align(anchor: created[0], targets: Seq(created[1], created[2], created[3]))))),
        label: "align three targets to anchor") switch {
        LayoutResult.MutationResult value => value.Delta.Payload.Moves.Count,
        LayoutResult other => throw new InvalidOperationException(message: $"unexpected align result: {other.GetType().Name}"),
    };
    facts.Add("layout.alignMoves", alignMoves);
    Probe.Require(condition: alignMoves == 3, message: "N-ary Align aggregates exactly one move delta per target");

    // Snap-guide seam: a SnapProbe whose policy carries an enabled SnapSetting.FeedbackCase drives Layout.Snap ->
    // EmitSnapGuide -> Motion.Cosmetic(SnapGuideCase) (fire-and-forget, best-effort). Assert the Snap query
    // SUCCEEDS regardless of whether a guide landed — proving the seam executes without faulting the query. The
    // cosmetic attaches to the canvas NSView overlay (not the DrawToBitmap surface), so it is not asserted via PNG.
    SnappingPolicy feedback = new(Settings: Seq<SnapSetting>(new SnapSetting.FeedbackCase(Enabled: true, Style: SnapGuideStyle.Dashed(tint: new Eto.Drawing.Color(red: 0f, green: 1f, blue: 0f)))));
    Option<SnappingSnapshot> snap = Probe.Expect(
        result: ui.Use(intent: GhUi.Layout(op: new LayoutOp.SnapCase(Probe: new SnapProbe.ObjectCase(ObjectId: created[1], Policy: feedback)))),
        label: "snap-guide seam on a placed toggle") switch {
        LayoutResult.SnapResult value => value.Snapshot,
        LayoutResult other => throw new InvalidOperationException(message: $"unexpected snap result: {other.GetType().Name}"),
    };
    facts.Add("snapGuide.seamRan", true);
    facts.AddIfSome("snapGuide.magnitude", snap, static s => s.Magnitude);

    // Per-mode radius ("snap strength"): a RadiiCase folds through SnappingPolicy.Native via the 7-arg ctor
    // (EdgeRadius widens the edge/middle/gap catch window, WireRadius the wire-straightening one). The headless
    // ceiling cannot deterministically observe snap-distance changes, so — like the FeedbackCase seam above —
    // assert the Snap query SUCCEEDS, proving the new Apply arm reconstructs SnappingSettings without faulting.
    SnappingPolicy radii = new(Settings: Seq<SnapSetting>(new SnapSetting.RadiiCase(EdgeRadius: Some(50), WireRadius: Some(15))));
    Option<SnappingSnapshot> radiiSnap = Probe.Expect(
        result: ui.Use(intent: GhUi.Layout(op: new LayoutOp.SnapCase(Probe: new SnapProbe.ObjectCase(ObjectId: created[2], Policy: radii)))),
        label: "snap with per-mode radius policy") switch {
        LayoutResult.SnapResult value => value.Snapshot,
        LayoutResult other => throw new InvalidOperationException(message: $"unexpected radii snap result: {other.GetType().Name}"),
    };
    facts.Add("snapRadii.edgeRadius", 50);
    facts.AddIfSome("snapRadii.magnitude", radiiSnap, static s => s.Magnitude);

    // ObjectSpec.Scribble places a ScribbleObject (Grasshopper2.SpecialObjects DocumentObject) through the same
    // DropObject path as Toggle — placed separately from the grid set so ComputeGrid's sort+take stays on the
    // four toggles, and proven by its own created id.
    DocumentMutationDelta scribbled = Probe.Expect(
        result: ui.Use(intent: GhUi.Document(op: DocumentOp.Mutate(
            DocumentMutation.Place(obj: ObjectSpec.Scribble(text: "Layout Proof", angle: 15), location: new PointF(30f, 80f))))),
        label: "place a scribble") switch {
        DocumentResult.MutationResult value => value.Delta.Payload,
        DocumentResult other => throw new InvalidOperationException(message: $"unexpected scribble result: {other.GetType().Name}"),
    };
    facts.Add("scribble.created", scribbled.Created.Count);
    Probe.Require(condition: scribbled.Created.Count == 1, message: "ObjectSpec.Scribble places exactly one ScribbleObject");
});
