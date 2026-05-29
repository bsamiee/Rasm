using System;
using Rasm.Grasshopper.UI;
using Rhino;

// Consolidates gh-ui-tooltip-layout + gh-ui-reflection-rails and adds the Section-C TooltipRail Fin-lift
// plus the DialogPresentation SmartEnum sweep. The reflection-rail drift canary is preserved and clearly
// marked. PulseHandle.Retarget and MotionRequest.Sequence are owned by the static gate: their constructors
// require Grasshopper2 Duration/Motion enums, which the host-filtered csx context cannot reference.
Scenario.Run("gh-ui-pure-rails", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();
    facts.Add("rhino.mainThread", RhinoApp.IsOnMainThread);
    Probe.Expect(result: ui.Use(intent: GhUi.Editor(op: EditorOp.EnsureVisible)), label: "ensure grasshopper editor");

    // TooltipRail Fin-lift: ReadInt now returns Fin<int> and ReadLayout composes the six constants through
    // an applicative chain (no Convert.ToInt32 boxing). All constants must resolve to a coherent layout.
    TooltipLayoutSnapshot layout = Probe.Expect(
        result: ui.Use(intent: GhUi.Tooltip(op: new TooltipOp.LayoutCase())),
        label: "tooltip layout fin-lift") switch {
        CanvasChromeResult.TooltipLayoutCase value => value.Snapshot,
        CanvasChromeResult other => throw new InvalidOperationException(message: $"unexpected tooltip layout: {other.GetType().Name}"),
    };
    Probe.Require(
        condition: layout.MinimumWidth > 0 && layout.MaximumWidth >= layout.MinimumWidth && layout.MaximumHeight > 0
            && layout.Padding > 0 && layout.DoublePadding == layout.Padding * 2 && layout.IconSize > 0,
        message: $"tooltip layout constants (min={layout.MinimumWidth} pad={layout.Padding} icon={layout.IconSize})");
    facts.Add("tooltip.minimumWidth", layout.MinimumWidth);
    facts.Add("tooltip.padding", layout.Padding);
    facts.Add("tooltip.iconSize", layout.IconSize);

    // [REFLECTION-RAIL DRIFT CANARY] WirePaintObserve bootstraps the GH2-internal WireRepository members by
    // name. If the host shape drifts, Bootstrap fails and this rejects -- the canary that guards the
    // reflection rail. A green subscription proves every reflected member still resolves on the live build.
    Subscription observe = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve())),
        label: "wire repository reflection rail canary") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        WireResult other => throw new InvalidOperationException(message: $"expected WirePaintObserve subscription, got {other.GetType().Name}"),
    };
    using (observe) { }
    facts.Add("reflection.wireRepositoryResolved", true);

    // DialogPresentation SmartEnum sweep: Modal/AttachedSheet are the whole vocabulary; keys are stable.
    facts.Add("dialogPresentation.count", DialogPresentation.Items.Count);
    foreach (DialogPresentation presentation in DialogPresentation.Items) {
        facts.Add($"dialogPresentation.{presentation}.key", presentation.Key);
    }
    Probe.Require(
        condition: DialogPresentation.Items.Count == 2 && DialogPresentation.Modal.Key == 0 && DialogPresentation.AttachedSheet.Key == 1,
        message: "dialog presentation items + keys");
});
