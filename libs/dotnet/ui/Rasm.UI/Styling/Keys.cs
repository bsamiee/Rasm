namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Condition {
    public static readonly Condition IncreaseContrast = new(inherited: false, identity);
    public static readonly Condition DifferentiateWithoutColor = new(inherited: false, identity);
    public static readonly Condition ReduceTransparency = new(inherited: false,
        static look => look with { Fills = look.Fills.Map(static fills => fills.Map(static fill => fill.Opaque)) });
    public static readonly Condition ReduceMotion = new(inherited: false, static look => look with { Motion = Some<TimingCurve>(new TimingCurve.Instant()) });
    public static readonly Condition Dark = new(inherited: false, identity);
    public static readonly Condition Default = new(inherited: false, identity);
    public static readonly Condition Destructive = new(inherited: false, identity);
    public static readonly Condition Selected = new(inherited: false, identity);
    public static readonly Condition Modified = new(inherited: false, identity);
    public static readonly Condition Mixed = new(inherited: false, identity);
    public static readonly Condition Busy = new(inherited: false, identity);
    public static readonly Condition Warning = new(inherited: false, identity);
    public static readonly Condition Faulted = new(inherited: false, identity);
    public static readonly Condition Disabled = new(inherited: true, identity);

    internal bool Inherited { get; }

    [UseDelegateFromConstructor]
    internal partial Look Applied(Look look);
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
    public static readonly Interaction Hover = new(showsFocusRing: false);
    public static readonly Interaction Pressed = new(showsFocusRing: false);
    public static readonly Interaction Focused = new(showsFocusRing: true);

    public bool ShowsFocusRing { get; }
}

[SmartEnum]
public sealed partial class Channel {
    public static readonly Channel Fill = new(inherited: false);
    public static readonly Channel Stroke = new(inherited: false);
    public static readonly Channel Text = new(inherited: true);

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
