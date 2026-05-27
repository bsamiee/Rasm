using System;
using Eto.Drawing;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;
using Rhino;

Scenario.Run("gh-ui-wire-runtime", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();

    Subscription observeSub = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve)),
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

    Probe.Expect(
        result: ui.Use(intent: GhUi.Canvas(op: CanvasOp.Invalidate(repaint: RepaintRequest.Scheduled))),
        label: "scheduled invalidate");

    WireDrawnSnapshot snapshot = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.Query(query: WireQuery.RecentlyDrawn()))),
        label: "recently drawn") switch {
        WireResult.DrawnCase drawnCase => drawnCase.Snapshot,
        _ => throw new InvalidOperationException(message: "expected drawn snapshot"),
    };

    using (observeSub) { }
    using (overlaySub) { }

    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    facts.Add("drawn.entryCount", snapshot.Entries.Count);
    facts.Add("drawn.freshFromWirePaint", snapshot.FreshFromWirePaint);
    facts.Add("drawn.documentModifications", snapshot.DocumentModifications);
});
