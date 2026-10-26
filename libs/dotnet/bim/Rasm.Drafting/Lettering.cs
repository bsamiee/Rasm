using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class LetteringForm {
    public static readonly LetteringForm TypeA = new("type-a", divisor: 14, xHeight: 10, pitch: 21);
    public static readonly LetteringForm TypeB = new("type-b", divisor: 10, xHeight: 7, pitch: 15);

    internal int Divisor { get; }
    internal int XHeight { get; }
    internal int Pitch { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LetteringStandard {
    public static readonly LetteringStandard Iso3098 = new(zoneHeight: Length.FromMillimeters(3.5m), height: static size => size.Sides.Long <= SheetSize.A2.Sides.Long ? Length.FromMillimeters(2.5m) : Length.FromMillimeters(3.5m));
    public static readonly LetteringStandard AsmeY142 = new(zoneHeight: Length.FromInches(0.24m), height: static _ => Length.FromInches(0.12m));

    public Length ZoneHeight { get; }

    [UseDelegateFromConstructor]
    public partial Length Height(SheetSize size);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class Terminator {
    public static readonly Terminator ClosedArrow = new("closed-arrow", proportion: 1);
    public static readonly Terminator OpenArrow = new("open-arrow", proportion: 1);
    public static readonly Terminator Oblique = new("oblique", proportion: 1);
    public static readonly Terminator Dot = new("dot", proportion: QuantityValue.FromTerms(1, 2));

    internal QuantityValue Proportion { get; }
}

public sealed record StyleMetrics(SheetSize Size, LetteringForm Form, Terminator Terminator, DrawingScale Scale) {
    // --- [LETTERING]
    public Length TextHeight => Size.Convention.Lettering.Height(Size);
    public Length TextGap => Stroke * 2;
    public Length BaselineSpacing => Stroke * Form.Pitch;
    public QuantityValue LineSpaceScale => QuantityValue.FromTerms(Form.Pitch, Form.Divisor);
    public QuantityValue StackHeightScale => QuantityValue.FromTerms(Form.XHeight, Form.Divisor);
    public Length Resolution => Scale.ToModel(Stroke);

    private Length Stroke => TextHeight / Form.Divisor;

    // --- [DIMENSIONING]
    public Length MaskOffset => SymbolLineWidth * 2;
    public Length ArrowLength => TextHeight * Terminator.Proportion;
    public Length ExtensionLineOffset => SymbolLineWidth * 8;
    public Length ExtensionLineExtension => SymbolLineWidth * 8;
    public Length LeaderLandingLength => SymbolLineWidth * 20;

    private Length SymbolLineWidth => TextHeight / 10;
}
