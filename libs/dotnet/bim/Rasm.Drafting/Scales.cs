using System.Globalization;
using System.Numerics;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<QuantityValue>(SkipFactoryMethods = true, SkipToString = true, SkipIFormattable = true)]
public sealed partial class DrawingScale {
    public (BigInteger Paper, BigInteger Model) Terms => QuantityValue.Reduce(_value) switch { var (paper, model) => (paper, model) };
    public Length ToModel(Length paper) => paper / _value;
    public Length ToPaper(Length model) => model * _value;

    public static Fin<DrawingScale> Validated(QuantityValue ratio) =>
        DraftingFault.Unless(QuantityValue.IsFinite(ratio) && ratio > QuantityValue.Zero, DraftingReason.ScaleRatio, ratio).Map(_ => new DrawingScale(ratio));

    public static Fin<DrawingScale> Parse(string text) =>
        toSeq(text.Split(':'))
            .Traverse(term => Term(text, term))
            .As()
            .Bind(terms => terms.Count is 2 ? Validated(QuantityValue.FromTerms(terms[0], terms[1])) : new DraftingFault(text, DraftingReason.ScaleRatio));

    private static Fin<BigInteger> Term(string text, string term) =>
        BigInteger.TryParse(term, NumberStyles.None, CultureInfo.InvariantCulture, out BigInteger value) && !value.IsZero ? value : new DraftingFault(text, DraftingReason.ScaleRatio);

    public override string ToString() => Terms switch { var (paper, model) => string.Create(CultureInfo.InvariantCulture, $"{paper}:{model}") };
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ScaleSeries {
    public static readonly ScaleSeries Iso5455 = new((from exponent in toSeq(Range(-4, 6)) from mantissa in Seq<BigInteger>(1, 2, 5) select QuantityValue.FromPowerOfTen(mantissa, exponent)).Traverse(DrawingScale.Validated).As().ThrowIfFail(), static scale => Some(scale.ToString()));
    public static readonly ScaleSeries Architectural = Imperial(
        toSeq((from octave in toSeq(Range(0, 8)) from inches in Seq(1, 12) select Length.FromInches(QuantityValue.FromTerms(inches, 1 << octave)) / Length.FromFeet(1)).Order()).Traverse(DrawingScale.Validated).As().ThrowIfFail(),
        static member => member.ToPaper(Length.FromFeet(1)), static _ => Length.FromFeet(1));
    public static readonly ScaleSeries Engineering = Imperial(
        Seq(60, 50, 40, 30, 20, 10).Map(static feet => Length.FromInches(1) / Length.FromFeet(feet)).Traverse(DrawingScale.Validated).As().ThrowIfFail(),
        static _ => Length.FromInches(1), static member => member.ToModel(Length.FromInches(1)));

    public Seq<DrawingScale> Members { get; }

    [UseDelegateFromConstructor]
    public partial Option<string> Notation(DrawingScale scale);

    public Option<DrawingScale> Read(string text) => Members.Find(member => Notation(member).Exists(notation => string.Equals(notation, text.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Option<DrawingScale> Nearest(double ratio, double tolerance) =>
        Members.Map(member => (Member: member, Error: double.Abs(double.Log(((QuantityValue)member).ToDouble() / ratio))))
            .Fold(Option<(DrawingScale Member, double Error)>.None, static (best, entry) => best.Filter(held => held.Error <= entry.Error) || Some(entry))
            .Filter(best => best.Error <= double.Log(1d + tolerance))
            .Map(static best => best.Member);

    private static ScaleSeries Imperial(Seq<DrawingScale> members, Func<DrawingScale, Length> paper, Func<DrawingScale, Length> model) {
        int denominator = (int)(members.Map(paper) + members.Map(model)).Fold(BigInteger.One, static (lcm, side) => {
            (_, BigInteger reduced) = QuantityValue.Reduce(side.Inches);
            return lcm / BigInteger.GreatestCommonDivisor(lcm, reduced) * reduced;
        });
        return new(members, scale => Some((Paper: paper(scale), Model: model(scale))).Filter(sides => QuantityValue.IsInteger(sides.Paper.Inches * denominator) && QuantityValue.IsInteger(sides.Model.Inches * denominator))
            .Map(sides => $"{sides.Paper.FeetInches.ToArchitecturalString(denominator)} = {sides.Model.FeetInches.ToArchitecturalString(denominator)}"));
    }
}
