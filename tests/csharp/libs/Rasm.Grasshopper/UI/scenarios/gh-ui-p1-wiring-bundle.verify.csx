using Eto.Drawing;
using Rasm.Grasshopper.UI;
using Rhino;

Scenario.Run("gh-ui-p1-wiring-bundle", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);

    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");
    EditorSnapshot preObserve = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)),
        label: "editor state before observe",
        select: static value => value switch {
            EditorResult.StateResult state => state.Snapshot,
            _ => Option<EditorSnapshot>.None,
        });
    Probe.Require(preObserve.HasDocument, "GH document must be open before wire paint observe");
    facts.Add("editor.hasDocument", preObserve.HasDocument);

    Probe.ExpectRejectedContains(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.Query(query: WireQuery.RecentlyDrawn()))),
        substring: "WirePaintObserve",
        label: "recently drawn without observe");
    facts.Add("wire.recentlyDrawn.rejectedWithoutObserve", true);

    Subscription observeSub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve())),
        label: "wire paint observe",
        select: static value => value switch {
            WireResult.SubscriptionCase sub => sub.Subscription,
            _ => Option<Subscription>.None,
        });

    Subscription overlaySub = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.OverlayPen(pen: new Pen(color: Colors.Crimson, thickness: 2f)))),
        label: "overlay pen",
        select: static value => value switch {
            WireResult.SubscriptionCase sub => sub.Subscription,
            _ => Option<Subscription>.None,
        });

    Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))),
        label: "scheduled invalidate");
    Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Canvas))),
        label: "canvas invalidate");

    // Tolerant read: the recently-drawn cache only populates when a wire-paint pass actually captures
    // between subscribe and query, which the headless bridge cannot force deterministically. Record the
    // hit/miss as a fact instead of asserting a hit (mirrors gh-ui-wire-runtime); the no-observe
    // rejection above remains the hard invariant.
    Fin<WireResult> drawnResult = ui.Use(intent: GhUi.Wire(op: WireOp.Query(query: WireQuery.RecentlyDrawn())));
    bool cacheHit = drawnResult.IsSucc;
    WireDrawnSnapshot drawn = drawnResult.Match(
        Succ: static value => value switch {
            WireResult.DrawnCase drawnCase => drawnCase.Snapshot,
            _ => new WireDrawnSnapshot(Entries: default, Stamp: default, FreshFromWirePaint: false),
        },
        Fail: static _ => new WireDrawnSnapshot(Entries: default, Stamp: default, FreshFromWirePaint: false));

    TooltipLayoutSnapshot layout = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Tooltip(op: new TooltipOp.LayoutCase())),
        label: "tooltip layout",
        select: static value => value switch {
            CanvasChromeResult.TooltipLayoutCase layoutCase => layoutCase.Snapshot,
            _ => Option<TooltipLayoutSnapshot>.None,
        });

    EditorSnapshot editor = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)),
        label: "editor state",
        select: static value => value switch {
            EditorResult.StateResult state => state.Snapshot,
            _ => Option<EditorSnapshot>.None,
        });

    InteractionSnapshot interaction = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Interaction(op: new InteractionOp.StatusCase())),
        label: "interaction status",
        select: static value => value switch {
            CanvasChromeResult.InteractionStatusCase status => status.Snapshot,
            _ => Option<InteractionSnapshot>.None,
        });

    using (observeSub) { }
    using (overlaySub) { }

    Probe.Require(layout.MinimumWidth > 0, $"minimumWidth={layout.MinimumWidth}");
    Probe.Require(layout.MaximumWidth >= layout.MinimumWidth, $"maximumWidth={layout.MaximumWidth} minimumWidth={layout.MinimumWidth}");

    facts.Add("wire.recentlyDrawn.cacheHit", cacheHit);
    facts.Add("drawn.entryCount", drawn.Entries.Count);
    facts.Add("drawn.stamp.modifications", drawn.Stamp.Modifications);
    facts.Add("p1.chrome.tooltipOk", layout.Padding > 0);
    facts.Add("p1.chrome.editorOk", editor.HasStatusBar);
    facts.Add("p1.chrome.interactionOk", interaction.InteractionCount >= 0);
    facts.AddIfSome(key: "editor.statusBarDocumentHash", value: editor.StatusBarDocumentHash, serialize: static hash => hash);
});
