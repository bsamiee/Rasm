using System.Collections.Generic;
using System.Globalization;

namespace Rasm.Analyzers.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ResultLifts {

    // --- [CASES]

    private static readonly (string Type, string Expression)[] Values = [
        ("int", "1"), ("string", "string.Empty"), ("Unit", "unit"), ("Seq<int>", "Seq(1, 2)"),
        ("Option<int>", "Some(1)"), ("object", "new object()"), (nameof(Error), """Error.New("held")"""), ("Expected", """new Expected("held", 1, None)"""),
    ];

    private static readonly Func<string, string, string, string>[] TargetTyped = [
        static (type, value, name) => $"internal static {type} {name}() => {value};",
        static (type, value, name) => $$"""internal static {{type}} {{name}}() { return {{value}}; }""",
        static (type, value, name) => $"internal static readonly {type} {name} = {value};",
        static (type, value, name) => $$"""internal static {{type}} {{name}} { get; } = {{value}};""",
        static (type, value, name) => $"internal static {type} {name}(bool held) => held ? {value} : {name}(false);",
        static (type, value, name) => $$"""internal static {{type}} {{name}}() { return Take({{value}}); static {{type}} Take({{type}} value) => value; }""",
        static (type, value, name) => $$"""internal static {{type}} {{name}}() { {{type}} held; held = {{value}}; return held; }""",
        static (type, value, name) => $"internal static Func<{type}> {name}() => () => {value};",
        static (type, value, name) => $$"""internal static Func<Task<{{type}}>> {{name}}() => async () => { await Task.Yield(); return {{value}}; };""",
    ];

    private static readonly Func<string, string, string, string>[] Inferred = [
        static (type, value, name) => $$"""internal static {{type}} {{name}}() { var held = {{value}}; return held; }""",
        static (type, value, name) => $"internal static {type} {name}() => Same({value});",
        static (type, value, name) => $"internal static {type}? {name}(bool held) => held ? {value} : default;",
    ];

    private static ImmutableArray<(string Type, string Expression, Option<string> Replacement)> Forms(string type, string value) {
        string fin = $"Fin<{type}>";
        string validation = $"Validation<Error, {type}>";
        const string error = """Error.New("refused")""";
        return [
            (fin, $"Fin.Succ({value})", value), (fin, $"Pure({value})", value), (fin, $"new {fin}.Succ({value})", value),
            (fin, $"Fin.Fail<{type}>({error})", error), (fin, $"Fail({error})", error), (fin, $"new {fin}.Fail({error})", error),
            (fin, $"""Fin.Fail<{type}>("refused")""", None),
            (validation, $"Success<Error, {type}>({value})", value), (validation, $"Validation.Success<Error, {type}>({value})", value),
            (validation, $"Fail<Error, {type}>({error})", error), (validation, $"Validation.Fail<Error, {type}>({error})", error),
        ];
    }

    private static string Source(string name, IEnumerable<string> members) => $$"""
        using System;
        using System.Threading.Tasks;
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class {{name}} {
            internal static R Same<R>(R value) => value;
        {{string.Join('\n', members.Select(static member => $"    {member}"))}}
        }

        """;

    // --- [ASSERTIONS]

    [Fact]
    public Task ReportsEveryTargetTypedConversion() {
        Analysis analysis = new(Analysis.Rules["RASM1001"].Descriptor);
        (Func<string, string, string, string> Write, bool HasTarget)[] members = [
            .. TargetTyped.Select(static write => (write, true)),
            .. Inferred.Select(static write => (write, false)),
        ];
        foreach ((int valueIndex, (string type, string value)) in Values.Index()) {
            string name = string.Create(CultureInfo.InvariantCulture, $"Lift{valueIndex}");
            IEnumerable<(Func<string, string, string, string> Member, string Type, string Value)> cases =
                from form in Forms(type, value)
                from member in members
                select (member.Write, form.Type, !string.Equals(type, nameof(Error), StringComparison.Ordinal) && form.Replacement.IsSome && member.HasTarget
                    ? $"{{|RASM1001:{form.Expression}|}}" : form.Expression);
            analysis.TestState.Sources.Add(($"/{name}.cs", Source(name,
                cases.Select(static (item, index) => item.Member(item.Type, item.Value, string.Create(CultureInfo.InvariantCulture, $"M{index}"))))));
        }
        return analysis.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task FixesEachSpellingAcrossTargetContexts() {
        (string Before, string After)[] cases = [.. Forms("int", "1").Where(static form => form.Replacement.IsSome).Select(static (form, index) => {
            string name = string.Create(CultureInfo.InvariantCulture, $"M{index}");
            Func<string, string, string, string> member = TargetTyped[index % TargetTyped.Length];
            return (member(form.Type, $"{{|RASM1001:{form.Expression}|}}", name), member(form.Type, (string)form.Replacement, name));
        })];
        return new Analysis(Analysis.Rules["RASM1001"].Descriptor) {
            TestCode = Source("Sample", cases.Select(static item => item.Before)),
            FixedCode = Source("Sample", cases.Select(static item => item.After)),
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}
