using System.Collections.Generic;
using Rasm.Drafting;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------

[Union]
public abstract partial record FieldRun {
    public sealed record Literal(string Text) : FieldRun;

    public sealed record Evaluator(string Function, Seq<Option<string>> Arguments) : FieldRun;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TitleSource {
    public virtual Option<string> Key => None;

    public sealed record DocumentText(string Stored) : TitleSource { public override Option<string> Key => Stored; }

    public sealed record LayoutText(string Stored) : TitleSource { public override Option<string> Key => Stored; }

    public sealed record PageName : TitleSource;

    public sealed record SheetFact(Func<Sheet, string> Read) : TitleSource;

    public static HashMap<TitleField, TitleSource> Iso7200 { get; } = HashMap<TitleField, TitleSource>(
        (TitleField.Iso7200.LegalOwner, new DocumentText("legal-owner")),
        (TitleField.Iso7200.IdentificationNumber, new PageName()),
        (TitleField.Iso7200.RevisionIndex, new LayoutText("revision-index")),
        (TitleField.Iso7200.DateOfIssue, new LayoutText("date-of-issue")),
        (TitleField.Iso7200.SegmentNumber, new LayoutText("segment-number")),
        (TitleField.Iso7200.SegmentCount, new LayoutText("segment-count")),
        (TitleField.Iso7200.LanguageCode, new DocumentText("language-code")),
        (TitleField.Iso7200.Title, new LayoutText("title")),
        (TitleField.Iso7200.SupplementaryTitle, new LayoutText("supplementary-title")),
        (TitleField.Iso7200.ResponsibleDepartment, new DocumentText("responsible-department")),
        (TitleField.Iso7200.TechnicalReference, new DocumentText("technical-reference")),
        (TitleField.Iso7200.ApprovalPerson, new LayoutText("approval-person")),
        (TitleField.Iso7200.Creator, new LayoutText("creator")),
        (TitleField.Iso7200.DocumentType, new DocumentText("document-type")),
        (TitleField.Iso7200.Classification, new DocumentText("classification")),
        (TitleField.Iso7200.DocumentStatus, new LayoutText("document-status")),
        (TitleField.Iso7200.PageNumber, new LayoutText("page-number")),
        (TitleField.Iso7200.PageCount, new LayoutText("page-count")),
        (TitleField.Iso7200.PaperSize, new SheetFact(static sheet => sheet.Size.Name)),
        (TitleField.Custom.ProjectName, new DocumentText("project-name")),
        (TitleField.Custom.Client, new DocumentText("client")),
        (TitleField.Custom.Checker, new LayoutText("checker")));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class FieldFormulas {

    // --- [RUNS]

    public static FieldRun Field(string function, params Seq<Option<string>> arguments) => new FieldRun.Evaluator(function, arguments);

    public static Option<FieldRun> Title(TitleSource source, Option<Sheet> sheet) =>
        source.Switch(
            sheet,
            documentText: static (_, document) => Some(Field(nameof(TextFields.DocumentText), document.Stored)),
            layoutText: static (_, layout) => Some(Field(nameof(TextFields.LayoutUserText), layout.Stored)),
            pageName: static (_, _) => Some(Field(nameof(TextFields.PageName))),
            sheetFact: static (held, fact) => held.Map<FieldRun>(page => fact.Read(page)));

    // --- [RENDERING]

    public static string Text(Seq<FieldRun> runs) =>
        string.Concat(runs.Map(static run => run.Switch(
            literal: static literal => literal.Text.Replace("%", $"%<{Quoted("%")}>%", StringComparison.Ordinal),
            evaluator: static evaluator => $"%<{evaluator.Function}({string.Join(',', evaluator.Arguments.Map(static argument => argument.Match(Quoted, "None")))})>%")));

    private static string Quoted(string text) =>
        $"""
        u"{(Enum.TryParse(text, ignoreCase: true, out UnitSystem system) && string.Equals(Enum.GetName(system), text, StringComparison.OrdinalIgnoreCase) ? text : string.Concat(text.Select(static codeUnit => char.IsAsciiHexDigit(codeUnit) || codeUnit is '-' ? codeUnit.ToString() : $"\\u{(int)codeUnit:x4}")))}"
        """;

    // --- [EVALUATION]

    public static IO<string> Format(RhinoDoc doc, string text, Option<RhinoObject> obj = default, Option<RhinoObject> topParent = default, Option<InstanceObject> immediateParent = default) =>
        Products.Accepted(() => (TextFields.TryFormat(text, doc, obj.ValueUnsafe(), topParent.ValueUnsafe(), immediateParent.ValueUnsafe(), out string result), result));

    public static IO<Seq<Option<string>>> Parse(RhinoDoc doc, Seq<FieldRun> runs) =>
        runs.Filter(static run => run is FieldRun.Evaluator)
            .Map(run => Products.Accepted(() => (TextFields.TryParse(Text([run]), doc, out List<string> values), values))
                .Map(static values => Optional(values[0]).Filter(static text => text is not "####")))
            .TraverseM(static parsed => parsed)
            .As();
}
