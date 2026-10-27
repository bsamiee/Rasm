using System.Collections.Generic;
using System.Reflection;
using Rasm.Drafting;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.RhinoCommon.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record FieldRun {
    public sealed record Literal(string Text) : FieldRun;
    public sealed record Evaluator : FieldRun {
        internal Evaluator(MethodInfo method, Seq<Option<string>> arguments) => (Method, Arguments) = (method, arguments);
        internal MethodInfo Method { get; }
        internal Seq<Option<string>> Arguments { get; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TextFieldOps {
    // --- [RUNS]
    public static FieldRun Field<T>(Func<string, T> evaluator, Option<string> argument) where T : IConvertible =>
        evaluator.Method.DeclaringType == typeof(TextFields)
            ? new FieldRun.Evaluator(evaluator.Method, [argument])
            : throw new ArgumentException($"{evaluator.Method} is no {nameof(TextFields)} evaluator", nameof(evaluator));

    public static Option<FieldRun> Title(TitleField field, Option<Sheet> sheet) =>
        field.Switch(
            sheet,
            projectEntry: static (_, entry) => Some(Field(TextFields.DocumentText, entry.Key)),
            sheetEntry: static (_, entry) => Some(Field(TextFields.LayoutUserText, entry.Key)),
            sheetName: static (_, _) => Some(Field(TextFields.PageName, None)),
            sheetFact: static (held, fact) => held.Map<FieldRun>(page => fact.Read(page)));

    // --- [RENDERING]
    public static string Text(Seq<FieldRun> runs) =>
        string.Concat(runs.Map(static run => run.Switch(
            literal: static literal => literal.Text.Replace("%", $"%<{Quoted("%")}>%", StringComparison.Ordinal),
            evaluator: static evaluator => $"%<{evaluator.Method.Name}({string.Join(',', evaluator.Arguments.Map(static argument => argument.Match(Quoted, "None")))})>%")));

    private static string Quoted(string text) =>
        $"""
        u"{(Enum.GetNames<UnitSystem>().Contains(text, StringComparer.OrdinalIgnoreCase) ? text : string.Concat(text.Select(static codeUnit => char.IsAsciiHexDigit(codeUnit) || codeUnit is '-' ? codeUnit.ToString() : $"\\u{(int)codeUnit:x4}")))}"
        """;

    // --- [EVALUATION]
    public static IO<string> Format(RhinoDoc doc, string text, Option<RhinoObject> obj = default, Option<RhinoObject> topParent = default, Option<InstanceObject> immediateParent = default) =>
        IO.lift(() => RhinoFault.Unless(TextFields.TryFormat(text, doc, obj.ValueUnsafe(), topParent.ValueUnsafe(), immediateParent.ValueUnsafe(), out string result)).Map(_ => result));

    public static IO<Seq<Option<string>>> Parse(RhinoDoc doc, Seq<FieldRun> runs) =>
        IO.lift(() => Callbacks.Each(runs.Filter(static run => run.Map(literal: false, evaluator: true)), (run, index) =>
            RhinoFault.Unless(TextFields.TryParse(Text([run]), doc, out List<string> values), index).Map(_ => Optional(values[0]).Filter(static text => text is not "####"))));
}
