using System;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;

Scenario.Run("gh-ui-wire-runtime", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();

    EditorSnapshot editor = Probe.ExpectCase(
        result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)),
        label: "ensure visible",
        select: static value => value switch {
            EditorResult.StateResult state => state.Snapshot,
            _ => Option<EditorSnapshot>.None,
        });

    facts.Add("editor.hasDocument", editor.HasDocument);

    Subscription observeSub = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve())),
        label: "wire paint observe") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        _ => throw new InvalidOperationException(message: "expected subscription result from wire paint observe"),
    };

    using Eto.Drawing.Pen overlayPen = new(color: Eto.Drawing.Colors.Crimson, thickness: 2f);
    Subscription overlaySub = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.OverlayPen(pen: overlayPen))),
        label: "overlay pen") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        _ => throw new InvalidOperationException(message: "expected subscription result from overlay pen"),
    };

    Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))),
        label: "scheduled invalidate");
    Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Canvas))),
        label: "canvas invalidate");

    Fin<WireResult> drawnResult = ui.Use(intent: GhUi.Wire(op: WireOp.Query(WireQuery.RecentlyDrawn())));
    bool cacheHit = drawnResult.IsSucc;
    WireDrawnSnapshot snapshot = drawnResult.Match(
        Succ: r => r switch {
            WireResult.DrawnCase d => d.Snapshot,
            _ => throw new InvalidOperationException(message: "expected drawn snapshot"),
        },
        Fail: _ => new WireDrawnSnapshot(Entries: default, Stamp: default, FreshFromWirePaint: false));

    using (observeSub) { }
    using (overlaySub) { }

    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    facts.Add("drawn.cacheHit", cacheHit);
    facts.Add("drawn.entryCount", snapshot.Entries.Count);
    facts.Add("drawn.documentModifications", snapshot.DocumentModifications);
    facts.Add("drawn.stamp.modifications", snapshot.Stamp.Modifications);
    facts.Add("drawn.stamp.projectionZoom", snapshot.Stamp.ProjectionZoom);
    facts.Add("drawn.stamp.innerFrameWidth", snapshot.Stamp.DrawInnerFrame.Width);
});
