using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidDrafting>]
public sealed partial class LetteringForm {
    public static readonly LetteringForm TypeA = new("type-a", divisor: 14, xHeight: 10, pitch: 21);
    public static readonly LetteringForm TypeB = new("type-b", divisor: 10, xHeight: 7, pitch: 15);

    internal int Divisor { get; }
    internal int XHeight { get; }
    internal int Pitch { get; }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LetteringStandard {
    public static readonly LetteringStandard Iso3098 = new(static () => SheetSize.A2, Length.FromMillimeters(2.5m), Length.FromMillimeters(3.5m));
    public static readonly LetteringStandard AsmeY142 = new(static () => SheetSize.AnsiC, Length.FromInches(0.12m), Length.FromInches(0.16m));

    private readonly Func<SheetSize> _largestSmallSheet;
    private readonly Length _smallSheetHeight;
    private readonly Length _largeSheetHeight;

    public Length Height(SheetSize size) => size.Sides.Long <= _largestSmallSheet().Sides.Long ? _smallSheetHeight : _largeSheetHeight;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidDrafting>]
public sealed partial class Terminator {
    public static readonly Terminator ClosedArrow = new("closed-arrow", proportion: 1);
    public static readonly Terminator OpenArrow = new("open-arrow", proportion: 1);
    public static readonly Terminator Oblique = new("oblique", proportion: 1);
    public static readonly Terminator Dot = new("dot", proportion: QuantityValue.FromTerms(1, 2));

    internal QuantityValue Proportion { get; }
}

public sealed record StyleMetrics(SheetSize Size, LetteringForm Form, Terminator Terminator, DrawingScale Scale) {
    // --- [LETTERING]
    public Length TextHeight => Size.Series.Convention.Lettering.Height(Size);
    public Length TextGap => Stroke * 2;
    public Length BaselineSpacing => Stroke * Form.Pitch;
    public QuantityValue LineSpaceScale => QuantityValue.FromTerms(Form.Pitch, Form.Divisor);
    public QuantityValue StackHeightScale => QuantityValue.FromTerms(Form.XHeight, Form.Divisor);

    private Length Stroke => TextHeight / Form.Divisor;

    // --- [DIMENSIONING]
    public Length MaskOffset => SymbolLineWidth * 2;
    public Length ArrowLength => TextHeight * Terminator.Proportion;
    public Length ExtensionLineOffset => SymbolLineWidth * 8;
    public Length ExtensionLineExtension => SymbolLineWidth * 8;
    public Length LeaderLandingLength => SymbolLineWidth * 20;

    private Length SymbolLineWidth => TextHeight / 10;

    // --- [RESOLUTION]
    public int LengthResolution => Places(Length.FromInches(1), radix: 2);
    public int AlternateLengthResolution => Places(Length.FromMillimeters(1), radix: 10);

    private int Places(Length extent, int radix) =>
        Scale.ToModel(Stroke) switch {
            var quantum => LanguageExt.List.unfold(extent, place => place > quantum ? Some((place, place / radix)) : None).Count(),
        };
}
