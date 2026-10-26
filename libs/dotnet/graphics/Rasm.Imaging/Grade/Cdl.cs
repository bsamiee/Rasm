using System.Xml;
using System.Xml.Linq;
using Rasm.Imaging.Color;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(KeyMemberName = "Extension")]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CdlFormat {
    public static readonly CdlFormat Correction = new(".cc", []);
    public static readonly CdlFormat Collection = new(".ccc", ["ColorCorrectionCollection"]);
    public static readonly CdlFormat DecisionList = new(".cdl", ["ColorDecisionList", "ColorDecision"]);

    private const string Declaration = """<?xml version="1.0" encoding="UTF-8"?>""";
    private const string CorrectionNode = "ColorCorrection";
    private const string IdAttribute = "id";
    private static readonly XNamespace Asc = "urn:ASC:CDL:v1.01";

    public Seq<string> Ancestors { get; }

    public static Fin<IterableNE<(Option<string> Id, LutTable.Cdl Correction)>> Read(string text) =>
        Try.lift(() => XDocument.Parse(text)).Run()
            .MapFail(static error => error.Exception.Case is XmlException xml ? Errors.ParseError($"CDL text did not parse, {xml.Message}") : error)
            .Bind(static document => toSeq(document.Descendants()).Filter(static element => element.Name.LocalName is CorrectionNode)
                .Traverse(static correction => LutFiles.CorrectionValues(correction, static name => Error.New($"CDL correction holds no readable {name}"))
                    .Map(values => (Optional(correction.Attribute(IdAttribute)?.Value), new LutTable.Cdl(CdlStyle.FwdNoClamp, values.Slope, values.Offset, values.Power, values.Saturation))))
                .As())
            .Bind(static corrections => corrections.AsIterableNE().ToFin(Errors.ParseError($"CDL text holds no {CorrectionNode}")));

    public string Write(LutTable.Cdl correction, Option<string> id) =>
        $"{Declaration}\n{Ancestors.FoldBack(
            new XElement(Asc + CorrectionNode, id.ToSeq().Map(static value => new XAttribute(IdAttribute, value)), LutFiles.CorrectionNodes(Asc, correction)),
            static (inner, ancestor) => new XElement(Asc + ancestor, inner))}\n";
}
