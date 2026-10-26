using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(KeyMemberName = "Extension")]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CdlFormat {
    public static readonly CdlFormat Correction = new(".cc", []);
    public static readonly CdlFormat Collection = new(".ccc", ["ColorCorrectionCollection"]);
    public static readonly CdlFormat DecisionList = new(".cdl", ["ColorDecisionList", "ColorDecision"]);

    public Seq<string> Ancestors { get; }
}

public sealed record CdlState(Channels<Magnitude> Slope, Channels<SignedMagnitude> Offset, Channels<PositiveMagnitude> Power, Magnitude Saturation)
    : IStateRecord<CdlState, CdlParameter>, IPixelStage<CdlState> {
    // --- [STAGE]
    public static CdlState Default { get; } = new(new(Magnitude.Unit), new(SignedMagnitude.Zero), new(PositiveMagnitude.Unit), Magnitude.Unit);

    public LutTable.Cdl Step =>
        new(CdlStyle.FwdNoClamp, new(Slope.Red, Slope.Green, Slope.Blue), new(Offset.Red, Offset.Green, Offset.Blue), new(Power.Red, Power.Green, Power.Blue), Saturation);

    public LutTable.Sequence Table(Gamut working) =>
        LutTables.Bracketed(working, LogSpace.AgXLog.Gamut, LogSpace.AgXLog.Into, Step, LogSpace.AgXLog.Back);

    public static Option<PixelPass> Pass(CdlState state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table(context.Working)));

    // --- [FILES]
    private const string Declaration = """<?xml version="1.0" encoding="UTF-8"?>""";
    private static readonly XNamespace Asc = "urn:ASC:CDL:v1.01";
    private const string CorrectionNode = "ColorCorrection";
    private const string IdAttribute = "id";

    public static Fin<IterableNE<(Option<string> Id, CdlState State)>> Read(string text) =>
        Try.lift(() => XDocument.Parse(text)).Run()
            .MapFail(static error => error.Exception.Case is XmlException xml ? Errors.ParseError($"CDL text did not parse, {xml.Message}") : error)
            .Bind(static document => toSeq(document.Descendants()).Filter(static element => element.Name.LocalName is CorrectionNode)
                .Traverse(static correction => Corrected(correction).Map(state => (Optional(correction.Attribute(IdAttribute)?.Value), state))).As())
            .Bind(static corrections => corrections.AsIterableNE().ToFin(Errors.ParseError($"CDL text holds no {CorrectionNode}")));

    public static string Write(CdlState state, Option<string> id, CdlFormat format) =>
        $"{Declaration}\n{format.Ancestors.FoldBack(
            new XElement(Asc + CorrectionNode, id.ToSeq().Map(static value => new XAttribute(IdAttribute, value)), LutFiles.CorrectionNodes(Asc, state.Step)),
            static (inner, ancestor) => new XElement(Asc + ancestor, inner))}\n";

    private static Fin<CdlState> Corrected(XElement correction) =>
        LutFiles.CorrectionValues(correction, static name => Error.New($"CDL correction holds no readable {name}"))
            .Bind(static keys => (
                    Channels<Magnitude>.Validated(keys.Slope, Magnitude.Validated), Channels<SignedMagnitude>.Validated(keys.Offset, SignedMagnitude.Validated),
                    Channels<PositiveMagnitude>.Validated(keys.Power, PositiveMagnitude.Validated), Magnitude.Validated(keys.Saturation))
                .Apply(static (slope, offset, power, saturation) => new CdlState(slope, offset, power, saturation))
                .As());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CdlParameter : IStateParameter<CdlState> {
    private static readonly Lens<CdlState, Channels<Magnitude>> Slopes = Lens<CdlState, Channels<Magnitude>>.New(static state => state.Slope, static slope => state => state with { Slope = slope });
    private static readonly Lens<CdlState, Channels<SignedMagnitude>> Offsets = Lens<CdlState, Channels<SignedMagnitude>>.New(static state => state.Offset, static offset => state => state with { Offset = offset });
    private static readonly Lens<CdlState, Channels<PositiveMagnitude>> Powers = Lens<CdlState, Channels<PositiveMagnitude>>.New(static state => state.Power, static power => state => state with { Power = power });
    private static readonly Presentation<float> Slope = new() { Soft = (0f, 2f), Origin = (float)Magnitude.Unit };
    private static readonly Presentation<float> Offset = new() { Soft = (-1f, 1f), Origin = (float)SignedMagnitude.Zero };
    private static readonly Presentation<float> Power = new() { Soft = ((float)PositiveMagnitude.MinValue, 2f), Origin = (float)PositiveMagnitude.Unit };

    public static readonly CdlParameter SlopeRed = new("slope-red", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.RedOf), Slope), new("Red slope", "Red multiplier before offset"));
    public static readonly CdlParameter SlopeGreen = new("slope-green", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.GreenOf), Slope), new("Green slope", "Green multiplier before offset"));
    public static readonly CdlParameter SlopeBlue = new("slope-blue", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.BlueOf), Slope), new("Blue slope", "Blue multiplier before offset"));
    public static readonly CdlParameter OffsetRed = new("offset-red", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.RedOf), Offset), new("Red offset", "Value added to red after slope"));
    public static readonly CdlParameter OffsetGreen = new("offset-green", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.GreenOf), Offset), new("Green offset", "Value added to green after slope"));
    public static readonly CdlParameter OffsetBlue = new("offset-blue", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.BlueOf), Offset), new("Blue offset", "Value added to blue after slope"));
    public static readonly CdlParameter PowerRed = new("power-red", StateParameter.Bounded(lens(Powers, Channels<PositiveMagnitude>.RedOf), Power), new("Red power", "Exponent on red after offset"));
    public static readonly CdlParameter PowerGreen = new("power-green", StateParameter.Bounded(lens(Powers, Channels<PositiveMagnitude>.GreenOf), Power), new("Green power", "Exponent on green after offset"));
    public static readonly CdlParameter PowerBlue = new("power-blue", StateParameter.Bounded(lens(Powers, Channels<PositiveMagnitude>.BlueOf), Power), new("Blue power", "Exponent on blue after offset"));
    public static readonly CdlParameter Saturation = new("saturation", StateParameter.Bounded(Lens<CdlState, Magnitude>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }), ContrastGradeParameter.Chroma),
        new("Saturation", "Distance from Rec.709 luma after power"));

    public StateParameter<CdlState> Kind { get; }
    public ParameterText Text { get; }
}
