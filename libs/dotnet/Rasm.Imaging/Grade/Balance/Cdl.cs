using System.Numerics;
using System.Xml.Linq;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade.Balance;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct CdlSlope : IMinMaxValue<CdlSlope> {
    public static CdlSlope MinValue { get; } = new(0f);
    public static CdlSlope MaxValue { get; } = new(float.MaxValue);
    public static CdlSlope Unit { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct CdlOffset : IMinMaxValue<CdlOffset> {
    public static CdlOffset MinValue { get; } = new(-float.MaxValue);
    public static CdlOffset MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct CdlPower : IMinMaxValue<CdlPower> {
    public static CdlPower MinValue { get; } = new(float.BitIncrement(0f));
    public static CdlPower MaxValue { get; } = new(float.MaxValue);
    public static CdlPower Unit { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[SmartEnum<string>(KeyMemberName = "Extension")]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CdlFormat {
    public static readonly CdlFormat Correction = new(".cc", []);
    public static readonly CdlFormat Collection = new(".ccc", ["ColorCorrectionCollection"]);
    public static readonly CdlFormat DecisionList = new(".cdl", ["ColorDecisionList", "ColorDecision"]);

    public Seq<string> Ancestors { get; }
}

public sealed record CdlState(
    CdlSlope SlopeRed, CdlSlope SlopeGreen, CdlSlope SlopeBlue,
    CdlOffset OffsetRed, CdlOffset OffsetGreen, CdlOffset OffsetBlue,
    CdlPower PowerRed, CdlPower PowerGreen, CdlPower PowerBlue,
    Saturation Saturation)
    : IStateRecord<CdlState, CdlParameter, InvalidGrade>, IPixelStage<CdlState> {
    // --- [STAGE]
    public static CdlState Default { get; } = new(
        CdlSlope.Unit, CdlSlope.Unit, CdlSlope.Unit,
        CdlOffset.Neutral, CdlOffset.Neutral, CdlOffset.Neutral,
        CdlPower.Unit, CdlPower.Unit, CdlPower.Unit,
        Saturation.Neutral);

    public LutTable.Cdl Step =>
        new(CdlStyle.FwdNoClamp, new(SlopeRed, SlopeGreen, SlopeBlue), new(OffsetRed, OffsetGreen, OffsetBlue), new(PowerRed, PowerGreen, PowerBlue), Saturation);

    public LutTable.Sequence Table(Gamut working) =>
        new([.. LutTables.Between(working, LogSpace.AgXLog.Gamut), .. LogSpace.AgXLog.Into, Step, .. LogSpace.AgXLog.Back, .. LutTables.Between(LogSpace.AgXLog.Gamut, working)]);

    public static Option<PixelPass> Pass(CdlState state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table(context.Working)));

    // --- [FILES]
    private const string Declaration = """<?xml version="1.0" encoding="UTF-8"?>""";
    private static readonly XNamespace Asc = "urn:ASC:CDL:v1.01";
    private const string CorrectionNode = "ColorCorrection";
    private const string IdAttribute = "id";

    public static Fin<Seq<(Option<string> Id, CdlState State)>> Read(string text) =>
        Try.lift(() => XDocument.Parse(text)).Run()
            .MapFail(static error => new CorrectionUnreadable(error))
            .Bind(static document => toSeq(document.Descendants()).Filter(static element => string.Equals(element.Name.LocalName, CorrectionNode, StringComparison.Ordinal))
                .Traverse(static correction => Corrected(correction).Map(state => (Optional(correction.Attribute(IdAttribute)?.Value), state)))
                .As()
                .ToFin());

    public static string Write(CdlState state, Option<string> id, CdlFormat format) =>
        $"{Declaration}\n{format.Ancestors.FoldBack(
            new XElement(Asc + CorrectionNode, id.ToSeq().Map(static value => new XAttribute(IdAttribute, value)), LutFiles.CorrectionNodes(Asc, state.Step)),
            static (inner, ancestor) => new XElement(Asc + ancestor, inner))}\n";

    public static IO<Unit> Save(CdlState state, Option<string> id, CdlFormat format, OutputPath path) => StillWriter.Commit(path, Write(state, id, format));

    private static Validation<Error, CdlState> Corrected(XElement correction) {
        static Validation<Error, T> Crossed<T>(float key) where T : IObjectFactory<T, float, InvalidGrade> =>
            T.Validate(key, provider: null, out T? item) is { } refused ? refused : item!;
        return LutFiles.CorrectionValues(correction, static name => new CorrectionMalformed(name))
            .Bind(static keys => (
                    Crossed<CdlSlope>(keys.Slope.X), Crossed<CdlSlope>(keys.Slope.Y), Crossed<CdlSlope>(keys.Slope.Z),
                    Crossed<CdlOffset>(keys.Offset.X), Crossed<CdlOffset>(keys.Offset.Y), Crossed<CdlOffset>(keys.Offset.Z),
                    Crossed<CdlPower>(keys.Power.X), Crossed<CdlPower>(keys.Power.Y), Crossed<CdlPower>(keys.Power.Z),
                    Crossed<Saturation>(keys.Saturation))
                .Apply(static (slopeRed, slopeGreen, slopeBlue, offsetRed, offsetGreen, offsetBlue, powerRed, powerGreen, powerBlue, saturation) =>
                    new CdlState(slopeRed, slopeGreen, slopeBlue, offsetRed, offsetGreen, offsetBlue, powerRed, powerGreen, powerBlue, saturation))
                .As());
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CdlParameter : IStateParameter<CdlState> {
    private static Presentation<CdlSlope, float> Slope { get; } = new() { Soft = (0f, 2f), Origin = (float)CdlSlope.Unit };
    private static Presentation<CdlOffset, float> Offset { get; } = new() { Soft = (-1f, 1f), Origin = (float)CdlOffset.Neutral };
    private static Presentation<CdlPower, float> Power { get; } = new() { Soft = ((float)CdlPower.MinValue, 2f), Origin = (float)CdlPower.Unit };

    public static readonly CdlParameter SlopeRed = new("slope-red", new StateParameter<CdlState>.Bounded<CdlSlope, float, InvalidGrade>(
        Lens<CdlState, CdlSlope>.New(static state => state.SlopeRed, static slope => state => state with { SlopeRed = slope }), Slope));
    public static readonly CdlParameter SlopeGreen = new("slope-green", new StateParameter<CdlState>.Bounded<CdlSlope, float, InvalidGrade>(
        Lens<CdlState, CdlSlope>.New(static state => state.SlopeGreen, static slope => state => state with { SlopeGreen = slope }), Slope));
    public static readonly CdlParameter SlopeBlue = new("slope-blue", new StateParameter<CdlState>.Bounded<CdlSlope, float, InvalidGrade>(
        Lens<CdlState, CdlSlope>.New(static state => state.SlopeBlue, static slope => state => state with { SlopeBlue = slope }), Slope));
    public static readonly CdlParameter OffsetRed = new("offset-red", new StateParameter<CdlState>.Bounded<CdlOffset, float, InvalidGrade>(
        Lens<CdlState, CdlOffset>.New(static state => state.OffsetRed, static offset => state => state with { OffsetRed = offset }), Offset));
    public static readonly CdlParameter OffsetGreen = new("offset-green", new StateParameter<CdlState>.Bounded<CdlOffset, float, InvalidGrade>(
        Lens<CdlState, CdlOffset>.New(static state => state.OffsetGreen, static offset => state => state with { OffsetGreen = offset }), Offset));
    public static readonly CdlParameter OffsetBlue = new("offset-blue", new StateParameter<CdlState>.Bounded<CdlOffset, float, InvalidGrade>(
        Lens<CdlState, CdlOffset>.New(static state => state.OffsetBlue, static offset => state => state with { OffsetBlue = offset }), Offset));
    public static readonly CdlParameter PowerRed = new("power-red", new StateParameter<CdlState>.Bounded<CdlPower, float, InvalidGrade>(
        Lens<CdlState, CdlPower>.New(static state => state.PowerRed, static power => state => state with { PowerRed = power }), Power));
    public static readonly CdlParameter PowerGreen = new("power-green", new StateParameter<CdlState>.Bounded<CdlPower, float, InvalidGrade>(
        Lens<CdlState, CdlPower>.New(static state => state.PowerGreen, static power => state => state with { PowerGreen = power }), Power));
    public static readonly CdlParameter PowerBlue = new("power-blue", new StateParameter<CdlState>.Bounded<CdlPower, float, InvalidGrade>(
        Lens<CdlState, CdlPower>.New(static state => state.PowerBlue, static power => state => state with { PowerBlue = power }), Power));
    public static readonly CdlParameter Saturation = new("saturation", new StateParameter<CdlState>.Bounded<Saturation, float, InvalidGrade>(
        Lens<CdlState, Saturation>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }), ContrastGradeParameter.Chroma));

    public StateParameter<CdlState> Kind { get; }
}
