using System;
using Rasm.Grasshopper.UI;

Scenario.Run("gh-ui-tooltip-layout", CAPTURE_PATH, (key, facts) => {
    TooltipLayoutSnapshot snap = Probe.Expect(
        result: new GrasshopperUi().Use(intent: GhUi.Tooltip(op: new TooltipOp.LayoutCase())),
        label: "tooltip layout") switch {
        CanvasChromeResult.TooltipLayoutCase layoutCase => layoutCase.Snapshot,
        _ => throw new InvalidOperationException(message: "expected TooltipLayoutCase"),
    };

    Probe.Require(snap.MinimumWidth > 0, $"minimumWidth={snap.MinimumWidth}");
    Probe.Require(snap.MaximumWidth >= snap.MinimumWidth, $"maximumWidth={snap.MaximumWidth} minimumWidth={snap.MinimumWidth}");
    Probe.Require(snap.MaximumHeight > 0, $"maximumHeight={snap.MaximumHeight}");
    Probe.Require(snap.Padding > 0, $"padding={snap.Padding}");
    Probe.Require(snap.DoublePadding == snap.Padding * 2, $"doublePadding={snap.DoublePadding} padding={snap.Padding}");
    Probe.Require(snap.IconSize > 0, $"iconSize={snap.IconSize}");

    facts.Add("layout.minimumWidth", snap.MinimumWidth);
    facts.Add("layout.maximumWidth", snap.MaximumWidth);
    facts.Add("layout.maximumHeight", snap.MaximumHeight);
    facts.Add("layout.padding", snap.Padding);
    facts.Add("layout.doublePadding", snap.DoublePadding);
    facts.Add("layout.iconSize", snap.IconSize);
});
