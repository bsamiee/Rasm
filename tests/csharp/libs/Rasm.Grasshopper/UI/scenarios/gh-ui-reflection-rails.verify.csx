using System;
using Rasm.Grasshopper.UI;
using Rasm.TestKit.Scenarios;

Scenario.Run("gh-ui-reflection-rails", CAPTURE_PATH, (key, facts) => {
    GrasshopperUi ui = new();

    TooltipLayoutSnapshot layout = Probe.Expect(
        result: ui.Use(intent: GhUi.Tooltip(op: new TooltipOp.LayoutCase())),
        label: "tooltip layout rail") switch {
        CanvasChromeResult.TooltipLayoutCase layoutCase => layoutCase.Snapshot,
        _ => throw new InvalidOperationException(message: "expected TooltipLayoutCase"),
    };

    Probe.Require(layout.MinimumWidth > 0, $"minimumWidth={layout.MinimumWidth}");
    Probe.Require(layout.MaximumWidth >= layout.MinimumWidth, $"maximumWidth={layout.MaximumWidth}");
    Probe.Require(layout.Padding > 0, $"padding={layout.Padding}");

    Subscription observe = Probe.Expect(
        result: ui.Use(intent: GhUi.Wire(op: WireOp.WirePaintObserve())),
        label: "wire repository rail") switch {
        WireResult.SubscriptionCase sub => sub.Subscription,
        _ => throw new InvalidOperationException(message: "expected WirePaintObserve subscription"),
    };

    EditorSnapshot editor = Probe.Expect(
        result: ui.Use(intent: GhUi.Editor(op: EditorOp.State)),
        label: "editor status bar read") switch {
        EditorResult.StateResult state => state.Snapshot,
        _ => throw new InvalidOperationException(message: "expected editor state"),
    };

    using (observe) { }

    facts.Add("tooltip.layout.minimumWidth", layout.MinimumWidth);
    facts.Add("tooltip.layout.maximumWidth", layout.MaximumWidth);
    facts.Add("tooltip.layout.padding", layout.Padding);
    facts.Add("editor.hasStatusBar", editor.HasStatusBar);
    editor.StatusBarDocumentHash.IfSome(hash => facts.Add("editor.statusBarDocumentHash", hash));
});
