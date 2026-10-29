using UnitsNet;

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
    public (Angle Angle, Length Spacing) Hatching => (Angle.FromDegrees(45), Width(LineRole.Hatching.Weight).Width * 10);
    public LineWidth Width(LineWeight weight) => LineWidth.Items[LineWidth.Items.Count(width => width.Width < Wide.Width) + weight.Steps];
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LineCap {
    public static readonly LineCap Butt = new(static multiple => multiple);
    public static readonly LineCap Round = new(static multiple => multiple > QuantityValue.Zero ? (multiple > QuantityValue.One ? multiple - 1 : QuantityValue.Zero) : multiple - 1);

    [UseDelegateFromConstructor]
    internal partial QuantityValue Drawn(QuantityValue multiple);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class LineType {
    private static readonly QuantityValue Dot = QuantityValue.FromTerms(1, 2);
    private static readonly QuantityValue Gap = -3;
    private static readonly QuantityValue ShortDash = 6;
    private static readonly QuantityValue Dash = 12;
    private static readonly QuantityValue LongDash = 24;
    private static readonly QuantityValue Space = -18;

    public static readonly LineType Continuous = new("01", IterableNE.singleton(Dash));
    public static readonly LineType Dashed = new("02", IterableNE.create(Dash, Gap));
    public static readonly LineType DashedSpaced = new("03", IterableNE.create(Dash, Space));
    public static readonly LineType LongDashedDotted = new("04", IterableNE.create(LongDash, Gap, Dot, Gap));
    public static readonly LineType LongDashedDoubleDotted = new("05", IterableNE.create(LongDash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType LongDashedTriplicateDotted = new("06", IterableNE.create(LongDash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType Dotted = new("07", IterableNE.create(Dot, Gap));
    public static readonly LineType LongDashedShortDashed = new("08", IterableNE.create(LongDash, Gap, ShortDash, Gap));
    public static readonly LineType LongDashedDoubleShortDashed = new("09", IterableNE.create(LongDash, Gap, ShortDash, Gap, ShortDash, Gap));
    public static readonly LineType DashedDotted = new("10", IterableNE.create(Dash, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedDotted = new("11", IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap));
    public static readonly LineType DashedDoubleDotted = new("12", IterableNE.create(Dash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedDoubleDotted = new("13", IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DashedTriplicateDotted = new("14", IterableNE.create(Dash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));
    public static readonly LineType DoubleDashedTriplicateDotted = new("15", IterableNE.create(Dash, Gap, Dash, Gap, Dot, Gap, Dot, Gap, Dot, Gap));

    private readonly IterableNE<QuantityValue> _pattern;

    public IterableNE<Length> Pattern(LineWidth width, LineCap cap) => _pattern.Map(multiple => width.Width * cap.Drawn(multiple));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LineRole {
    public static readonly LineRole Outline = new(LineType.Continuous, LineWeight.Wide);
    public static readonly LineRole Hidden = new(LineType.Dashed, LineWeight.Narrow);
    public static readonly LineRole Cut = new(LineType.Continuous, LineWeight.ExtraWide);
    public static readonly LineRole Tangent = new(LineType.Continuous, LineWeight.Narrow);
    public static readonly LineRole Hatching = new(LineType.Continuous, LineWeight.Narrow);

    public LineType Type { get; }
    public LineWeight Weight { get; }
}
