using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LetteringForm {
    public static readonly LetteringForm TypeA = new(divisor: 14, lowercase: 10, pitch: 21);
    public static readonly LetteringForm TypeB = new(divisor: 10, lowercase: 7, pitch: 15);

    internal int Divisor { get; }
    internal int Lowercase { get; }
    internal int Pitch { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LetteringStandard {
    public static readonly LetteringStandard Iso3098 = new(zoneHeight: Length.FromMillimeters(3.5m), form: LetteringForm.TypeB, height: static size => size.Sides.Long <= SheetSize.A2.Sides.Long ? Length.FromMillimeters(2.5m) : Length.FromMillimeters(3.5m));
    public static readonly LetteringStandard AsmeY142 = new(zoneHeight: Length.FromInches(0.24m), form: LetteringForm.TypeB, height: static _ => Length.FromInches(0.12m));

    public Length ZoneHeight { get; }
    public LetteringForm Form { get; }

    [UseDelegateFromConstructor]
    public partial Length Height(SheetSize size);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class Terminator {
    public static readonly Terminator ClosedArrow = new(proportion: 1);
    public static readonly Terminator OpenArrow = new(proportion: 1);
    public static readonly Terminator Oblique = new(proportion: 1);
    public static readonly Terminator Dot = new(proportion: QuantityValue.FromTerms(1, 2));

    internal QuantityValue Proportion { get; }
}

public sealed record StyleMetrics(SheetSize Size, LetteringForm Form, Terminator Terminator, DrawingScale Scale) {
    // --- [LETTERING]
    public Length Height => Size.Convention.Lettering.Height(Size);
    public Length LineThickness => Height / Form.Divisor;
    public Length LowercaseHeight => LineThickness * Form.Lowercase;
    public Length CharacterSpacing => LineThickness * 2;
    public Length BaselineSpacing => LineThickness * Form.Pitch;
    public Length Resolution => Scale.ToModel(LineThickness);

    // --- [DIMENSIONING]
    public Length TerminatorLength => Height * Terminator.Proportion;
    public Length ExtensionOvershoot => LineThickness * 8;
    public Length ExtensionGap => LineThickness * 8;
    public Length DimensionLineSpacing => BaselineSpacing * 2;
    public Length ReferenceLineLength => LineThickness * 20;
}
