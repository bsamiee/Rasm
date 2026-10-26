using UnitsNet;
using UnitsNet.Units;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SignedFraction : IPresented<SignedFraction, float> {
    public static SignedFraction MinValue { get; } = new(-1f);
    public static SignedFraction MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ProperFraction : IPresented<ProperFraction, float> {
    public static ProperFraction MinValue => Zero;
    public static ProperFraction MaxValue { get; } = new(float.BitDecrement(1f));
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PositiveFraction : IPresented<PositiveFraction, float> {
    public static PositiveFraction MinValue { get; } = new(float.BitIncrement(0f));
    public static PositiveFraction MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation => ProperFraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Magnitude : IPresented<Magnitude, float> {
    public static Magnitude MinValue => Zero;
    public static Magnitude MaxValue { get; } = new(float.MaxValue);
    public static Magnitude Unit { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SignedMagnitude : IPresented<SignedMagnitude, float> {
    public static SignedMagnitude MinValue { get; } = new(-float.MaxValue);
    public static SignedMagnitude MaxValue { get; } = new(float.MaxValue);
    public static SignedMagnitude Unit { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Zero };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PositiveCount : IPresented<PositiveCount, int> {
    public static PositiveCount MinValue { get; } = new(1);
    public static PositiveCount MaxValue { get; } = new(int.MaxValue);
    public static Presentation<int> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}
