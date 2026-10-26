namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Role {
    public static readonly Role Window = Create("window");
    public static readonly Role Accent = Create("accent");
    public static readonly Role Panel = Create("panel");
    public static readonly Role Toolbar = Create("toolbar");
    public static readonly Role Button = Create("button");
    public static readonly Role Field = Create("field");
    public static readonly Role Divider = Create("divider");
    public static readonly Role Scrollbar = Create("scrollbar");
    public static readonly Role Label = Create("label");
    public static readonly Role Track = Create("track");
    public static readonly Role Span = Create("span");
    public static readonly Role Handle = Create("handle");
    public static readonly Role Grid = Create("grid");
    public static readonly Role Selection = Create("selection");
    public static readonly Role FocusRing = Create("focus-ring");
    public static readonly Role Well = Create("well");
    public static readonly Role Series = Create("series");
    public static readonly Role Trace = Create("trace");
    public static readonly Role GridFine = Create("grid-fine");
    public static readonly Role OverlayCaption = Create("overlay-caption");
    public static readonly Role Needle = Create("needle");
    public static readonly Role Stop = Create("stop");
    public static readonly Role Ring = Create("ring");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Appearance {
    public static readonly Appearance Native = Create("native");
    public static readonly Appearance Glass = Create("glass");
    public static readonly Appearance Drawn = Create("drawn");
    public static readonly Appearance Borderless = Create("borderless");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlSize {
    public static readonly ControlSize Regular = Create("regular");
    public static readonly ControlSize Compact = Create("compact");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Condition {
    public static readonly Condition IncreaseContrast = new("increase-contrast");
    public static readonly Condition DifferentiateWithoutColor = new("differentiate-without-color");
    public static readonly Condition ReduceTransparency = new("reduce-transparency");
    public static readonly Condition ReduceMotion = new("reduce-motion");
    public static readonly Condition Dark = new("dark");
    public static readonly Condition Default = new("default");
    public static readonly Condition Destructive = new("destructive");
    public static readonly Condition Selected = new("selected");
    public static readonly Condition Modified = new("modified");
    public static readonly Condition Mixed = new("mixed");
    public static readonly Condition Busy = new("busy");
    public static readonly Condition Warning = new("warning");
    public static readonly Condition Faulted = new("faulted");
    public static readonly Condition Disabled = new("disabled");
}

[SmartEnum]
public sealed partial class ContrastLevel {
    public static readonly ContrastLevel Minimum = new(text: 4.5, graphic: 3.0);
    public static readonly ContrastLevel Enhanced = new(text: 7.0, graphic: 3.0);

    public double Text { get; }
    public double Graphic { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Interaction {
    public static readonly Interaction Hover = new("hover", ring: false);
    public static readonly Interaction Pressed = new("pressed", ring: false);
    public static readonly Interaction Focused = new("focused", ring: true);

    public bool Ring { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Channel {
    public static readonly Channel Fill = new("fill");
    public static readonly Channel Stroke = new("stroke");
    public static readonly Channel Text = new("text");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Metric {
    public static readonly Metric Extent = new("extent", 0f);
    public static readonly Metric Reach = new("reach", 0f);
    public static readonly Metric Inset = new("inset", 0f);
    public static readonly Metric Gap = new("gap", 0f);
    public static readonly Metric Radius = new("radius", 0f);
    public static readonly Metric Smoothing = new("smoothing", 0f);
    public static readonly Metric Stroke = new("stroke", 1f);
    public static readonly Metric Icon = new("icon", 16f);
    public static readonly Metric FontSize = new("font-size", 1f);
    public static readonly Metric Spacing = new("spacing", 32f);
    public static readonly Metric Slop = new("slop", 3f);
    public static readonly Metric Fine = new("fine", 0.1f);
    public static readonly Metric Notch = new("notch", 1.2f);
    public static readonly Metric EdgeFade = new("edge-fade", 0f);

    public float Default { get; }
}
