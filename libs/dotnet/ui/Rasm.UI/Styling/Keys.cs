namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIComparable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
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

[ValueObject<string>(SkipIComparable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Appearance {
    public static readonly Appearance Native = Create("native");
    public static readonly Appearance Glass = Create("glass");
    public static readonly Appearance Drawn = Create("drawn");
    public static readonly Appearance Borderless = Create("borderless");
}

[ValueObject<string>(SkipIComparable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ControlSize {
    public static readonly ControlSize Regular = Create("regular");
    public static readonly ControlSize Compact = Create("compact");
}

[SmartEnum<string>]
public sealed partial class Condition {
    public static readonly Condition IncreaseContrast = new("increase-contrast", inherited: false, identity);
    public static readonly Condition DifferentiateWithoutColor = new("differentiate-without-color", inherited: false, identity);
    public static readonly Condition ReduceTransparency = new("reduce-transparency", inherited: false,
        static look => look with { Fills = look.Fills.Map(static fills => fills.Map(static fill => fill.Opaqued)) });
    public static readonly Condition ReduceMotion = new("reduce-motion", inherited: false, static look => look with { Motion = Some<TimingCurve>(new TimingCurve.Instant()) });
    public static readonly Condition Dark = new("dark", inherited: false, identity);
    public static readonly Condition Default = new("default", inherited: false, identity);
    public static readonly Condition Destructive = new("destructive", inherited: false, identity);
    public static readonly Condition Selected = new("selected", inherited: false, identity);
    public static readonly Condition Modified = new("modified", inherited: false, identity);
    public static readonly Condition Mixed = new("mixed", inherited: false, identity);
    public static readonly Condition Busy = new("busy", inherited: false, identity);
    public static readonly Condition Warning = new("warning", inherited: false, identity);
    public static readonly Condition Faulted = new("faulted", inherited: false, identity);
    public static readonly Condition Disabled = new("disabled", inherited: true, identity);

    internal bool Inherited { get; }

    [UseDelegateFromConstructor]
    internal partial Look Restyled(Look look);
}

[SmartEnum]
public sealed partial class ContrastLevel {
    public static readonly ContrastLevel Minimum = new(text: 4.5, graphic: 3.0);
    public static readonly ContrastLevel Enhanced = new(text: 7.0, graphic: 3.0);

    public double Text { get; }
    public double Graphic { get; }
}

[SmartEnum]
public sealed partial class Interaction {
    public static readonly Interaction Hover = new(ring: false);
    public static readonly Interaction Pressed = new(ring: false);
    public static readonly Interaction Focused = new(ring: true);

    public bool Ring { get; }
}

[SmartEnum<string>]
public sealed partial class Channel {
    public static readonly Channel Fill = new("fill", inherited: false);
    public static readonly Channel Stroke = new("stroke", inherited: false);
    public static readonly Channel Text = new("text", inherited: true);

    internal bool Inherited { get; }
}

[SmartEnum]
public sealed partial class Metric {
    public static readonly Metric Extent = new(0f);
    public static readonly Metric Reach = new(0f);
    public static readonly Metric Inset = new(0f);
    public static readonly Metric Gap = new(0f);
    public static readonly Metric Radius = new(0f);
    public static readonly Metric Smoothing = new(0f);
    public static readonly Metric Stroke = new(1f);
    public static readonly Metric Icon = new(16f);
    public static readonly Metric FontSize = new(1f);
    public static readonly Metric Spacing = new(32f);
    public static readonly Metric Slop = new(3f);
    public static readonly Metric Fine = new(0.1f);
    public static readonly Metric Notch = new(1.2f);
    public static readonly Metric EdgeFade = new(0f);

    public float Default { get; }
}
