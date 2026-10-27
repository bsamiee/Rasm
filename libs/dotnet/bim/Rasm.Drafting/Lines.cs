namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LineWidth {
    public static readonly LineWidth Width013 = new(Length.FromMillimeters(0.13m));
    public static readonly LineWidth Width018 = new(Length.FromMillimeters(0.18m));
    public static readonly LineWidth Width025 = new(Length.FromMillimeters(0.25m));
    public static readonly LineWidth Width035 = new(Length.FromMillimeters(0.35m));
    public static readonly LineWidth Width050 = new(Length.FromMillimeters(0.5m));
    public static readonly LineWidth Width070 = new(Length.FromMillimeters(0.7m));
    public static readonly LineWidth Width100 = new(Length.FromMillimeters(1m));
    public static readonly LineWidth Width140 = new(Length.FromMillimeters(1.4m));
    public static readonly LineWidth Width200 = new(Length.FromMillimeters(2m));

    public Length Width { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LineWeight {
    public static readonly LineWeight Narrow = new(steps: -2);
    public static readonly LineWeight Medium = new(steps: -1);
    public static readonly LineWeight Wide = new(steps: 0);
    public static readonly LineWeight ExtraWide = new(steps: 2);

    internal int Steps { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LineGroup {
    public static readonly LineGroup Group025 = new(LineWidth.Width025);
    public static readonly LineGroup Group035 = new(LineWidth.Width035);
    public static readonly LineGroup Group050 = new(LineWidth.Width050);
    public static readonly LineGroup Group070 = new(LineWidth.Width070);
    public static readonly LineGroup Group100 = new(LineWidth.Width100);

    internal LineWidth Wide { get; }
    public LineWidth Width(LineWeight weight) => LineWidth.Items[LineWidth.Items.Count(width => width.Width < Wide.Width) + weight.Steps];
}

[SmartEnum]
public sealed partial class LineType {
    private const int Dot = 1;
    private const int Gap = -3;
    private const int ShortDash = 6;
    private const int Dash = 12;
    private const int LongDash = 24;
    private const int Space = -18;

    public static readonly LineType Continuous = new(IterableNE.singleton(Dot));
    public static readonly LineType Dashed = new(IterableNE.create(Dash, Gap));
    public static readonly LineType DashedSpaced = new(IterableNE.create(Dash, Space));
    public static readonly LineType LongDashedDotted = new(IterableNE.create(LongDash, Gap, Dot, Gap));
    public static readonly LineType LongDashedDoubleDotted = new(IterableNE.create(LongDash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType LongDashedTriplicateDotted = new(IterableNE.create(LongDash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType Dotted = new(IterableNE.create(Dot, Gap));
    public static readonly LineType LongDashedShortDashed = new(IterableNE.create(LongDash, Gap, ShortDash, Gap));
    public static readonly LineType LongDashedDoubleShortDashed = new(IterableNE.create(LongDash, Gap, ShortDash, Gap, ShortDash, Gap));
    public static readonly LineType DashedDotted = new(IterableNE.create(Dash, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedDotted = new(IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap));
    public static readonly LineType DashedDoubleDotted = new(IterableNE.create(Dash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedDoubleDotted = new(IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DashedTriplicateDotted = new(IterableNE.create(Dash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedTriplicateDotted = new(IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));

    private readonly IterableNE<int> _pattern;

    public IterableNE<Length> Pattern(LineWidth width) => _pattern.Map(multiple => width.Width * multiple);
}
