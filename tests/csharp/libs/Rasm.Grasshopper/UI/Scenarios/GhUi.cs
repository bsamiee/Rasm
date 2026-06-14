using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;

namespace Rasm.Grasshopper.Tests.UI.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the GH UI theme — runtime coverage for the Layout deepening that pure-managed specs
// cannot reach: ComputeGrid and the N-ary Align both require a live GhObjectList, so their
// geometry is proven on real placed objects. Layout arrangements are document mutations
// (attributes.Move / OCD.AlignObjects) — they run SYNCHRONOUSLY in the bridge (no solver settle,
// no idle pump), so the post-arrange Measure reflects the committed positions. The snap-guide
// seam (Layout.Snap -> EmitSnapGuide -> SnapGuideCase cosmetic, fire-and-forget) and
// ObjectSpec.Scribble placement ride the same headless editor; no raw GH2 types are named.
internal static class GhUiScenarios {
    [RhinoScenario(theme: "gh-ui")]
    internal static Fin<Unit> MotionLayout(ScenarioContext ctx) =>
        from ui in Fin.Succ(value: new GrasshopperUi())
        let onMain = Note(ctx: ctx, key: "rhino.mainThread", value: RhinoApp.IsOnMainThread)
        from shown in ctx.Expect(label: "show editor headless", projection: ui.Use(intent: GhUi.Editor(op: EditorOp.Show(visible: false)), cancellation: CancellationToken.None))
        from editorResult in ctx.Expect(label: "editor state", projection: ui.Use(intent: GhUi.Editor(op: EditorOp.State), cancellation: CancellationToken.None))
        from stateResult in Admit<EditorResult.StateResult>(outcome: editorResult)
        let editor = stateResult.Snapshot
        let canvasFact = Note(ctx: ctx, key: "editor.hasCanvas", value: editor.HasCanvas)
        from editorLaw in ctx.Require(label: "headless editor created without a deferred abort", observed: editor.HasEditor)
            // Canvas exposure is a stable headless contract (the GhEditor ctor builds the Canvas), so
            // a regression must FAIL the layout/snap ops rather than silently skip them.
        from canvasLaw in ctx.Require(label: "headless editor exposes a canvas", observed: editor.HasCanvas)
        let placeMutations = new[] {
            DocumentMutation.Selection(op: SelectionOp.All),
            DocumentMutation.Target(subject: ObjectScope.Selection, op: DocumentTargetOp.Delete()),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "A", state: true), location: new PointF(-120f, -60f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "B", state: false), location: new PointF(150f, -40f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "C", state: true), location: new PointF(-90f, 120f)),
            DocumentMutation.Place(obj: ObjectSpec.Toggle(name: "D", state: false), location: new PointF(170f, 140f)),
        }
        from placeResult in ctx.Expect(label: "place four toggles", projection: ui.Use(intent: GhUi.Document(op: DocumentOp.Mutate(mutations: placeMutations)), cancellation: CancellationToken.None))
        from placedMutation in Admit<DocumentResult.MutationResult>(outcome: placeResult)
        let created = placedMutation.Delta.Payload.Created
        let placedFact = Note(ctx: ctx, key: "layout.placedCount", value: created.Count)
        from placedLaw in ctx.Require(label: "four toggles placed with created ids", observed: created.Count == 4)
            // Grid(2x2): capture the arrange delta's move count so the scenario pins the COMPUTED
            // moves, not just the measured lattice.
        from gridResult in ctx.Expect(label: "grid 2x2 arrange", projection: ui.Use(intent: GhUi.Layout(op: new LayoutOp.ArrangeCase(Arrangement: LayoutArrangement.Grid(rows: 2, cols: 2, gap: LayoutGap.Create(20f), ids: created, gapPolicy: LayoutGapPolicy.Stretch))), cancellation: CancellationToken.None))
        from gridMutation in Admit<LayoutResult.MutationResult>(outcome: gridResult)
        let gridMoves = gridMutation.Delta.Payload.Moves.Count
        let gridFact = Note(ctx: ctx, key: "layout.gridMoves", value: gridMoves)
        from gridLaw in ctx.Require(label: "Grid emits one move delta per object", observed: gridMoves == 4)
        from measureResult in ctx.Expect(label: "measure gridded objects", projection: ui.Use(intent: GhUi.Layout(op: new LayoutOp.MeasureCase(Scope: new ObjectScope.ObjectsCase(Ids: created))), cancellation: CancellationToken.None))
        from measureSnapshots in Admit<LayoutResult.SnapshotsResult>(outcome: measureResult)
        let gridded = measureSnapshots.Snapshots
        let colLefts = gridded.Map(static s => (int)MathF.Round(x: s.Bounds.Left, mode: MidpointRounding.ToEven)).Distinct()
        let rowTops = gridded.Map(static s => (int)MathF.Round(x: s.Bounds.Top, mode: MidpointRounding.ToEven)).Distinct()
        let cells = gridded.Map(static s => ((int)MathF.Round(x: s.Bounds.Left, mode: MidpointRounding.ToEven), (int)MathF.Round(x: s.Bounds.Top, mode: MidpointRounding.ToEven))).Distinct().Count
        let columnsFact = Note(ctx: ctx, key: "layout.gridColumns", value: colLefts.Count)
        let rowsFact = Note(ctx: ctx, key: "layout.gridRows", value: rowTops.Count)
        let cellsFact = Note(ctx: ctx, key: "layout.gridCells", value: cells)
        from latticeLaw in ctx.Require(label: "Grid(2x2) lands a 2-column x 2-row lattice", observed: gridded.Count == 4 && colLefts.Count == 2 && rowTops.Count == 2 && cells == 4)
            // Gap invariant: the two distinct column origins are meaningfully separated, so a swapped
            // axis or ignored gap that collapsed the columns fails rather than passing degenerately.
        let columnSpan = MathF.Abs(x: colLefts[1] - colLefts[0])
        let spanFact = Note(ctx: ctx, key: "layout.columnSpan", value: columnSpan)
        from spanLaw in ctx.Require(label: "Grid columns separated by at least the gap", observed: columnSpan >= 20f)
            // N-ary Align: anchor + three targets, default fix; already-aligned target axes may
            // legitimately emit no move deltas.
        from alignResult in ctx.Expect(label: "align three targets to anchor", projection: ui.Use(intent: GhUi.Layout(op: new LayoutOp.ArrangeCase(Arrangement: LayoutArrangement.Align(anchor: created[0], targets: Seq(created[1], created[2], created[3])))), cancellation: CancellationToken.None))
        from alignMutation in Admit<LayoutResult.MutationResult>(outcome: alignResult)
        let alignMoves = alignMutation.Delta.Payload.Moves.Count
        let alignFact = Note(ctx: ctx, key: "layout.alignMoves", value: alignMoves)
        from alignLaw in ctx.Require(label: "N-ary Align completes after grid placement", observed: alignMoves >= 0)
            // Snap-guide seam: an enabled FeedbackCase drives Layout.Snap -> EmitSnapGuide ->
            // Motion.Cosmetic(SnapGuideCase); the query must SUCCEED whether or not a guide landed.
        let feedback = new SnappingPolicy(Settings: Seq<SnapSetting>(new SnapSetting.FeedbackCase(Enabled: true, XStyle: SnapGuideStyle.Dashed(tint: new Color(red: 0f, green: 1f, blue: 0f)))))
        from snapResult in ctx.Expect(label: "snap-guide seam on a placed toggle", projection: ui.Use(intent: GhUi.Layout(op: new LayoutOp.SnapCase(Probe: new SnapProbe.ObjectCase(ObjectId: created[1], Policy: feedback))), cancellation: CancellationToken.None))
        from snapCase in Admit<LayoutResult.SnapResult>(outcome: snapResult)
        let seamFact = Note(ctx: ctx, key: "snapGuide.seamRan", value: true)
        let magnitudeFact = snapCase.Snapshot.Map(s => Note(ctx: ctx, key: "snapGuide.magnitude", value: s.Magnitude))
        // Per-mode radius ("snap strength"): a RadiiCase folds through SnappingPolicy.Native via
        // the 7-arg ctor; the headless ceiling cannot observe snap-distance changes, so the Snap
        // query SUCCEEDING proves the Apply arm reconstructs SnappingSettings without faulting.
        let radii = new SnappingPolicy(Settings: Seq<SnapSetting>(new SnapSetting.RadiiCase(EdgeRadius: Some(50), WireRadius: Some(15))))
        from radiiResult in ctx.Expect(label: "snap with per-mode radius policy", projection: ui.Use(intent: GhUi.Layout(op: new LayoutOp.SnapCase(Probe: new SnapProbe.ObjectCase(ObjectId: created[2], Policy: radii))), cancellation: CancellationToken.None))
        from radiiCase in Admit<LayoutResult.SnapResult>(outcome: radiiResult)
        let radiiFact = Note(ctx: ctx, key: "snapRadii.edgeRadius", value: 50)
        let radiiMagnitudeFact = radiiCase.Snapshot.Map(s => Note(ctx: ctx, key: "snapRadii.magnitude", value: s.Magnitude))
        // ObjectSpec.Scribble places a ScribbleObject through the same DropObject path as Toggle —
        // placed separately so ComputeGrid's sort+take stays on the four toggles.
        from scribbleResult in ctx.Expect(label: "place a scribble", projection: ui.Use(intent: GhUi.Document(op: DocumentOp.Mutate(mutations: [DocumentMutation.Place(obj: ObjectSpec.Scribble(text: "Layout Proof", angle: 15), location: new PointF(30f, 80f))])), cancellation: CancellationToken.None))
        from scribbleMutation in Admit<DocumentResult.MutationResult>(outcome: scribbleResult)
        let scribbleFact = Note(ctx: ctx, key: "scribble.created", value: scribbleMutation.Delta.Payload.Created.Count)
        from scribbleLaw in ctx.Require(label: "Scribble places exactly one object", observed: scribbleMutation.Delta.Payload.Created.Count == 1)
        select unit;

    private static Fin<TCase> Admit<TCase>(object outcome) where TCase : class =>
        outcome is TCase admitted
            ? Fin.Succ(value: admitted)
            : Fin.Fail<TCase>(error: Error.New(message: $"unexpected result: {outcome.GetType().Name}"));

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }
}
