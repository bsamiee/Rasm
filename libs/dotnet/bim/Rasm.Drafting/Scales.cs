using System.Globalization;
using System.Numerics;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<QuantityValue>(ConstructorAccessModifier = AccessModifier.Internal, SkipIParsable = true, SkipToString = true, SkipIFormattable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class DrawingScale {
    public Length ToModel(Length paper) => paper / _value;
    public Length ToPaper(Length model) => model * _value;

    public static Fin<DrawingScale> Validated(QuantityValue ratio) => TryCreate(ratio, out DrawingScale? scale, out DraftingFault? fault) ? scale : fault;

    public override string ToString() =>
        QuantityValue.Reduce(_value) switch { (var paper, var model) => string.Create(CultureInfo.InvariantCulture, $"{paper}:{model}") };

    static partial void ValidateFactoryArguments(ref DraftingFault? validationError, ref QuantityValue value) =>
        validationError = QuantityValue.IsFinite(value) && value > QuantityValue.Zero ? null : new DraftingFault(string.Create(CultureInfo.InvariantCulture, $"drawing scale {value}"), DraftingReason.ScaleRatio);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class ScaleSeries {
    public static readonly ScaleSeries Iso5455 = new("iso-5455",
        from exponent in toSeq(Range(-4, 6))
        from mantissa in Seq<BigInteger>(1, 2, 5)
        select new DrawingScale(QuantityValue.FromPowerOfTen(mantissa, exponent)),
        static (_, scale) => Some(scale.ToString()));
    public static readonly ScaleSeries Architectural = new("architectural",
        toSeq((from octave in toSeq(Range(0, 8))
               from inches in Seq(1, 12)
               select new DrawingScale(Length.FromInches(QuantityValue.FromTerms(inches, 1 << octave)) / Length.FromFeet(1))).Order()),
        static (members, scale) => ImperialNotation(members, scale, static member => (member.ToPaper(Length.FromFeet(1)), Length.FromFeet(1))));
    public static readonly ScaleSeries Engineering = new("engineering",
        Seq(60, 50, 40, 30, 20, 10).Map(static feet => new DrawingScale(Length.FromInches(1) / Length.FromFeet(feet))),
        static (members, scale) => ImperialNotation(members, scale, static member => (Length.FromInches(1), member.ToModel(Length.FromInches(1)))));

    public Seq<DrawingScale> Members { get; }

    private readonly Func<Seq<DrawingScale>, DrawingScale, Option<string>> _notation;

    public Option<string> Notation(DrawingScale scale) => _notation(Members, scale);

    private static Option<string> ImperialNotation(Seq<DrawingScale> members, DrawingScale scale, Func<DrawingScale, (Length Paper, Length Model)> sides) =>
        from fractionDenominator in Some((int)members.Map(sides).Bind(static pair => Seq(pair.Paper.Inches, pair.Model.Inches))
            .Fold(BigInteger.One, static (lcm, inches) => QuantityValue.Reduce(inches) switch { (_, var denominator) => lcm / BigInteger.GreatestCommonDivisor(lcm, denominator) * denominator }))
        let pair = sides(scale)
        where QuantityValue.IsInteger(pair.Paper.Inches * fractionDenominator) && QuantityValue.IsInteger(pair.Model.Inches * fractionDenominator)
        select $"{pair.Paper.FeetInches.ToArchitecturalString(fractionDenominator)} = {pair.Model.FeetInches.ToArchitecturalString(fractionDenominator)}";
}
