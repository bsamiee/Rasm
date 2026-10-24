namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Role {
    public static readonly Role Window = new("window");
    public static readonly Role Panel = new("panel");
    public static readonly Role Toolbar = new("toolbar");
    public static readonly Role Rail = new("rail");
    public static readonly Role Popover = new("popover");
    public static readonly Role Toast = new("toast");
    public static readonly Role Tab = new("tab");
    public static readonly Role Button = new("button");
    public static readonly Role ButtonPrimary = new("button-primary");
    public static readonly Role ButtonDanger = new("button-danger");
    public static readonly Role Field = new("field");
    public static readonly Role ListRow = new("list-row");
    public static readonly Role Item = new("item");
    public static readonly Role SectionHeader = new("section-header");
    public static readonly Role Readout = new("readout");
    public static readonly Role HudCapsule = new("hud-capsule");
    public static readonly Role Well = new("well");
    public static readonly Role Ground = new("ground");
    public static readonly Role Surface = new("surface");
    public static readonly Role Track = new("track");
    public static readonly Role Span = new("span");
    public static readonly Role Knob = new("knob");
    public static readonly Role Ring = new("ring");
    public static readonly Role Needle = new("needle");
    public static readonly Role Puck = new("puck");
    public static readonly Role Stop = new("stop");
    public static readonly Role Handle = new("handle");
    public static readonly Role Grid = new("grid");
    public static readonly Role GridFine = new("grid-fine");
    public static readonly Role Major = new("major");
    public static readonly Role Trace = new("trace");
    public static readonly Role Series = new("series");
    public static readonly Role Label = new("label");
    public static readonly Role Guide = new("guide");
    public static readonly Role Highlight = new("highlight");
    public static readonly Role Selection = new("selection");
    public static readonly Role Veil = new("veil");
    public static readonly Role FocusRing = new("focus-ring");
    public static readonly Role OverlayCaption = new("overlay-caption");
    public static readonly Role ClipOver = new("clip-over");
    public static readonly Role ClipUnder = new("clip-under");
    public static readonly Role Divider = new("divider");
    public static readonly Role Scrollbar = new("scrollbar");
    public static readonly Role Checker = new("checker");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Condition {
    public static readonly Condition Plain = new("plain");
    public static readonly Condition IncreaseContrast = new("increase-contrast");
    public static readonly Condition DifferentiateWithoutColor = new("differentiate-without-color");
    public static readonly Condition Selected = new("selected");
    public static readonly Condition Modified = new("modified");
    public static readonly Condition Mixed = new("mixed");
    public static readonly Condition Busy = new("busy");
    public static readonly Condition Faulted = new("faulted");
    public static readonly Condition Disabled = new("disabled");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Interaction {
    public static readonly Interaction Rest = new("rest");
    public static readonly Interaction Hover = new("hover");
    public static readonly Interaction Pressed = new("pressed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Channel {
    public static readonly Channel Fill = new("fill");
    public static readonly Channel Edge = new("edge");
    public static readonly Channel Text = new("text");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Metric {
    public static readonly Metric Extent = new("extent");
    public static readonly Metric Reach = new("reach");
    public static readonly Metric Inset = new("inset");
    public static readonly Metric Gap = new("gap");
    public static readonly Metric Radius = new("radius");
    public static readonly Metric Stroke = new("stroke");
    public static readonly Metric Icon = new("icon");
    public static readonly Metric Type = new("type");
    public static readonly Metric Spacing = new("spacing");
    public static readonly Metric Slop = new("slop");
}
