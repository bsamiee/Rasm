using System.Linq.Expressions;
using System.Reflection;
using Rasm.Drafting;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino.Document.Notation;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record FieldRun {
    public sealed record Literal(string Text) : FieldRun;
    public sealed record Evaluator : FieldRun {
        public MethodInfo Method { get; }
        public Seq<Option<string>> Arguments { get; }

        internal Evaluator(MethodInfo method, Seq<Option<string>> arguments) => (Method, Arguments) = (method, arguments);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TextFieldOps {
    // --- [RUNS]
    public static FieldRun Field<T>(Expression<Func<T>> call) where T : IConvertible =>
        call.Body switch {
            MethodCallExpression body when body.Method.DeclaringType == typeof(TextFields) => new FieldRun.Evaluator(
                body.Method,
                toSeq(body.Arguments).Map(static argument => Optional(Expression.Lambda<Func<string?>>(argument).Compile(preferInterpretation: true)())).Strict()),
            var other => throw new ArgumentException($"{other} is no {nameof(TextFields)} evaluator call", nameof(call)),
        };

    public static Option<FieldRun> Title(TitleField field, Option<(Length Width, Length Height)> page) =>
        field.Source.Switch(
            (field.Key, Page: page),
            projectText: static (state, _) => Some(Field(() => TextFields.DocumentText(state.Key))),
            sheetText: static (state, _) => Some(Field(() => TextFields.LayoutUserText(state.Key))),
            identifier: static (_, _) => Some(Field(static () => TextFields.PageName())),
            ordinal: static (_, _) => Some(Field(static () => TextFields.PageNumber())),
            count: static (_, _) => Some(Field(static () => TextFields.NumPages())),
            designation: static (state, _) => state.Page.Bind(static size => Sheet.Match(size.Width, size.Height)).Map<FieldRun>(static sheet => sheet.Size.Designation));

    // --- [RENDERING]
    public static string Text(Seq<FieldRun> runs) {
        return string.Concat(runs.Map(static run => run.Switch(
            literal: static literal => literal.Text.Replace("%", $"%<{Quoted(Some("%"))}>%", StringComparison.Ordinal),
            evaluator: static evaluator => $"%<{evaluator.Method.Name}({string.Join(',', evaluator.Arguments.Map(Quoted))})>%")));

        static string Quoted(Option<string> argument) =>
            argument.Match(
                Some: static text => $"u\"{(Enum.GetNames<UnitSystem>().Contains(text, StringComparer.OrdinalIgnoreCase) ? text : string.Concat(text.Select(static codeUnit => char.IsAsciiHexDigit(codeUnit) || codeUnit is '-' ? codeUnit.ToString() : $"\\u{(int)codeUnit:x4}")))}\"",
                None: static () => "None");
    }

    // --- [EVALUATION]
    public static IO<string> Format(RhinoDoc doc, string text, Option<RhinoObject> obj = default, Option<RhinoObject> topParent = default, Option<InstanceObject> immediateParent = default) =>
        IO.lift(() => Refused.Unless(
            TextFields.TryFormat(text, doc, obj.ValueUnsafe(), topParent.ValueUnsafe(), immediateParent.ValueUnsafe(), out string result),
            result,
            nameof(TextFields.TryFormat)));

    public static IO<Seq<Option<string>>> Parse(RhinoDoc doc, Seq<FieldRun> runs) =>
        IO.lift(() => {
            const string noValue = "####";
            _ = TextFields.TryParse(Text(runs.Filter(static run => run.Map(literal: false, evaluator: true))), doc, out List<string> values);
            return Callbacks.Each(toSeq(values), static (value, index) =>
                RefusedElement.Unless(value is not null, Optional(value).Filter(static text => text is not noValue), nameof(TextFields.TryParse), index));
        });
}
