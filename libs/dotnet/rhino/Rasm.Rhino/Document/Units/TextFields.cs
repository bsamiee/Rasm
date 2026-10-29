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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FieldFormulas {
    // --- [RUNS]
    public static FieldRun Field(string function, params Seq<Option<string>> arguments) => new FieldRun.Evaluator(function, arguments);

    public static Option<FieldRun> Title(TitleField field, Option<Sheet> sheet) =>
        field.Entry.Switch(
            (Sheet: sheet, field.Key),
            projectEntry: static (state, _) => Some(Field(nameof(TextFields.DocumentText), state.Key)),
            sheetEntry: static (state, _) => Some(Field(nameof(TextFields.LayoutUserText), state.Key)),
            sheetName: static (_, _) => Some(Field(nameof(TextFields.PageName))),
            sheetFact: static (state, fact) => state.Sheet.Map<FieldRun>(page => fact.Read(page)));

    // --- [RENDERING]
    public static string Text(Seq<FieldRun> runs) =>
        string.Concat(runs.Map(static run => run.Switch(
            literal: static literal => literal.Text.Replace("%", $"%<{Quoted("%")}>%", StringComparison.Ordinal),
            evaluator: static evaluator => $"%<{evaluator.Function}({string.Join(',', evaluator.Arguments.Map(static argument => argument.Match(Quoted, "None")))})>%")));

    private static string Quoted(string text) =>
        $"""
        u"{(Enum.GetNames<UnitSystem>().Contains(text, StringComparer.OrdinalIgnoreCase) ? text : string.Concat(text.Select(static codeUnit => char.IsAsciiHexDigit(codeUnit) || codeUnit is '-' ? codeUnit.ToString() : $"\\u{(int)codeUnit:x4}")))}"
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
