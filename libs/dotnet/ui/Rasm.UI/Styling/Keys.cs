namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Condition {
    public static readonly Condition Compact = new(inherited: true);
    public static readonly Condition Glass = new(inherited: true);
    public static readonly Condition Drawn = new(inherited: true);
    public static readonly Condition Borderless = new(inherited: true);
    public static readonly Condition IncreaseContrast = new(inherited: false);
    public static readonly Condition DifferentiateWithoutColor = new(inherited: false);
    public static readonly Condition ReduceTransparency = new(inherited: false);
    public static readonly Condition ReduceMotion = new(inherited: false);
    public static readonly Condition Dark = new(inherited: false);
    public static readonly Condition Default = new(inherited: false);
    public static readonly Condition Destructive = new(inherited: false);
    public static readonly Condition Selected = new(inherited: false);
    public static readonly Condition Highlighted = new(inherited: false);
    public static readonly Condition Descending = new(inherited: false);
    public static readonly Condition Modified = new(inherited: false);
    public static readonly Condition Pending = new(inherited: false);
    public static readonly Condition Mixed = new(inherited: false);
    public static readonly Condition Busy = new(inherited: false);
    public static readonly Condition Success = new(inherited: false);
    public static readonly Condition Warning = new(inherited: false);
    public static readonly Condition Faulted = new(inherited: false);
    public static readonly Condition Disabled = new(inherited: true);

    internal bool Inherited { get; }
}

[SmartEnum]
public sealed partial class Interaction {
    public static readonly Interaction Hover = new(emphasis: Fraction.Validated(0.16f).ToOption());
    public static readonly Interaction Pressed = new(emphasis: Fraction.Validated(0.24f).ToOption());
    public static readonly Interaction Focused = new(emphasis: None);

    internal Option<Fraction> Emphasis { get; }
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
    public static readonly Metric Extent = new(0f, inherited: false, Dense);
    public static readonly Metric Measure = new(float.PositiveInfinity, inherited: false, Dense);
    public static readonly Metric Reach = new(0f, inherited: false, Dense);
    public static readonly Metric Inset = new(0f, inherited: false, Dense);
    public static readonly Metric Gap = new(0f, inherited: false, Dense);
    public static readonly Metric Radius = new(0f, inherited: false, Rounded);
    public static readonly Metric Smoothing = new(0f, inherited: false, Unscaled);
    public static readonly Metric Stroke = new(1f, inherited: false, Unscaled);
    public static readonly Metric Icon = new(16f, inherited: true, Dense);
    public static readonly Metric Turn = new(0f, inherited: false, Unscaled);
    public static readonly Metric FontSize = new(1f, inherited: false, Unscaled);
    public static readonly Metric Spacing = new(32f, inherited: false, Dense);
    public static readonly Metric Slop = new(3f, inherited: false, Dense);
    public static readonly Metric Fine = new(0.1f, inherited: false, Unscaled);
    public static readonly Metric Notch = new(1.2f, inherited: false, Unscaled);
    public static readonly Metric EdgeFade = new(0f, inherited: false, Dense);

    public float Default { get; }
    internal bool Inherited { get; }

    [UseDelegateFromConstructor]
    internal partial float Scale(Theme theme);

    private static float Dense(Theme theme) => (float)theme.Density;

    private static float Rounded(Theme theme) => (float)theme.Density * theme.Rounding;

    private static float Unscaled(Theme _) => 1f;
}
