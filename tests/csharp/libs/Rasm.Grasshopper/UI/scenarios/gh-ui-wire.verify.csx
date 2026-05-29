using System;
using Rasm.Grasshopper.UI;
using Rhino;

// Consolidates gh-ui-wire-runtime + gh-ui-p1-wiring-bundle and adds Section-C wire/document surfaces:
// GraphMetric.Items, the ConnectAt / DisconnectInputsExcept verbs + WireEditArgs threading, MetaValues,
// DropObject(Init) / DropSnippet factories, and FindCriterion.Search. Fixed Guids keep it deterministic.
Scenario.Run("gh-ui-wire", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");
    EditorSnapshot editor = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)),
        label: "editor state",
        select: static value => value switch { EditorResult.StateResult state => state.Snapshot, _ => Option<EditorSnapshot>.None });
    facts.Add("editor.hasDocument", editor.HasDocument);

    // Hard invariant: RecentlyDrawn rejects until WirePaintObserve has run a capture pass.
    Probe.ExpectRejectedContains(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.Query(query: WireQuery.RecentlyDrawn()))),
        substring: "WirePaintObserve",
        label: "recently drawn without observe");

    Subscription observeSub = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve())),
        label: "wire paint observe") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        _ => throw new InvalidOperationException(message: "expected subscription result from wire paint observe"),
    };
    using Pen overlayPen = new(color: Colors.Crimson, thickness: 2f);
    Subscription overlaySub = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.OverlayPen(pen: overlayPen))),
        label: "overlay pen") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        _ => throw new InvalidOperationException(message: "expected subscription result from overlay pen"),
    };
    Probe.Expect(result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))), label: "scheduled invalidate");

    Fin<WireResult> drawnResult = ui.Use(intent: GhUi.Wire(op: WireOp.Query(WireQuery.RecentlyDrawn())));
    facts.Add("drawn.cacheHit", drawnResult.IsSucc);
    using (observeSub) { }
    using (overlaySub) { }

    // GraphMetric.Items sweep on an empty id set: every metric resolves a typed WireResult (Linearity /
    // Topology / SortedIds). Connectivity is captured once per call (getter allocates).
    // GraphMetric.Items is the whole vocabulary; each metric is dispatchable via WireOp.Query. The host
    // Connectivity.IsLinear throws on an empty id set, so the per-metric outcome is recorded tolerantly
    // rather than asserted green (a non-degenerate sweep would require placed, connected geometry).
    facts.Add("graphMetric.count", GraphMetric.Items.Count);
    Probe.Require(condition: GraphMetric.Items.Count == 3, message: $"graph metric item count={GraphMetric.Items.Count}");
    foreach (GraphMetric metric in GraphMetric.Items) {
        Fin<WireResult> metricResult = ui.Use(intent: GhUi.Wire(op: WireOp.Query(WireQuery.GraphMetric(ids: Seq<Guid>(), kind: metric))));
        facts.Add($"graphMetric.{metric}.dispatched", metricResult.IsSucc);
    }

    // ConnectAt / DisconnectInputsExcept dispatch + WireEditArgs threading: a wire whose endpoint Guids are
    // absent rejects at param resolution, proving WireOp.Edit(wire, edit, args) routes the new verbs. The
    // host-filtered bridge cannot deterministically place connected geometry, so a live count delta is not
    // asserted here (it would require two wired parameters).
    Guid absentSource = new("00000000-0000-0000-0000-0000000000a1");
    Guid absentTarget = new("00000000-0000-0000-0000-0000000000a2");
    WireSnapshot.ConnectedCase absentWire = new(Source: absentSource, Target: absentTarget, SourceResolved: false, TargetResolved: false, Connected: false, Selected: false);
    Probe.ExpectRejectedContains(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.Edit(wire: absentWire, edit: WireEdit.ConnectAt, args: new WireEditArgs(SourceIndex: 0, TargetIndex: 0)))),
        substring: "not found",
        label: "connect-at dispatch reaches param resolution");
    Probe.ExpectRejectedContains(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.Edit(wire: absentWire, edit: WireEdit.DisconnectInputsExcept, args: new WireEditArgs(Omit: Seq(absentTarget))))),
        substring: "not found",
        label: "disconnect-inputs-except dispatch reaches param resolution");
    facts.Add("wireEdit.connectAt.dispatched", true);
    facts.Add("wireEdit.disconnectInputsExcept.dispatched", true);

    // MetaValues dispatches and projects to Map<MetaName, Seq<string>>. MetaName is a Grasshopper2 type the
    // host-filtered csx cannot name, so the projected map shape is owned by the static gate; here we only
    // prove the query dispatches + succeeds against the live document.
    DocumentResult metaValues = Probe.Expect(
        result: ui.Use(intent: GhUi.Document(op: DocumentOp.Query(DocumentQuery.MetaValues))),
        label: "meta values dispatch");
    facts.Add("metaValues.resultType", metaValues.GetType().Name);

    // FindCriterion.Search aliases ByNameCase (host FindBySearch is a stub); the query returns a typed match set.
    DocumentResult search = Probe.Expect(
        result: ui.Use(intent: GhUi.Document(op: DocumentOp.Query(DocumentQuery.Find(FindCriterion.Search("rasm"))))),
        label: "find search");
    Seq<DocumentObjectSnapshot> matches = search switch {
        DocumentResult.FindResult value => value.Matches,
        _ => throw new InvalidOperationException(message: $"unexpected find result: {search.GetType().Name}"),
    };
    facts.Add("search.matchCount", matches.Count);

    // DropObject(Init) tuple overload + DropSnippet factory: prove factory -> case + payload wiring.
    DocumentMutation dropWithInit = DocumentMutation.Drop(proxyId: absentSource, location: new PointF(100f, 100f), init: "rasm-init");
    DocumentMutation dropSnippet = DocumentMutation.DropSnippet(file: "/tmp/rasm-snippet.ghx", location: new PointF(120f, 120f));
    Probe.Require(condition: dropWithInit is DocumentMutation.DropCase { Init.IsSome: true } && dropSnippet is DocumentMutation.DropSnippetCase, message: "drop init + snippet factory cases");
    facts.Add("drop.initIsSome", dropWithInit is DocumentMutation.DropCase { Init.IsSome: true });
    facts.Add("drop.snippetCase", dropSnippet is DocumentMutation.DropSnippetCase);
});
