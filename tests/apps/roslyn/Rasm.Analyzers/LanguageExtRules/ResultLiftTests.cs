using System.Collections.Generic;
using System.Globalization;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ResultLiftTests : CSharpCodeFixVerifier<ResultLiftAnalyzer, LanguageExtCodeFix> {

    // --- [RASM1001]

    [Fact]
    public Task CallbackReturn() => VerifyCodeFixAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;

        internal static class CallbackReturn {
            internal static R Switch<R>(int value, Func<int, R> positive, Func<int, R> negative) => value > 0 ? positive(value) : negative(value);
            internal static Fin<int> Switched(int value) => Switch(value, static held => Fin.Succ(held), static _ => {|#0:Fin.Fail<int>(Error.New("negative"))|});
            internal static Fin<int> Matched(Option<int> value) => value.Match(Some: static held => Fin.Succ(held), None: static () => {|#1:Fin.Fail<int>(Error.New("none"))|});
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("""Fin.Fail<int>(Error.New("negative"))""", """Error.New("negative")""", "Fin<int>"),
            Diagnostic().WithLocation(1).WithArguments("""Fin.Fail<int>(Error.New("none"))""", """Error.New("none")""", "Fin<int>"),
        ],
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;

        internal static class CallbackReturn {
            internal static R Switch<R>(int value, Func<int, R> positive, Func<int, R> negative) => value > 0 ? positive(value) : negative(value);
            internal static Fin<int> Switched(int value) => Switch(value, static held => Fin.Succ(held), static _ => Error.New("negative"));
            internal static Fin<int> Matched(Option<int> value) => value.Match(Some: static held => Fin.Succ(held), None: static () => Error.New("none"));
        }
        """);

    [Fact]
    public Task ConvertedArm() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class ConvertedArm {
            internal static Validation<Error, Unit> Checked(bool held) => held ? {|#0:Success<Error, Unit>(unit)|} : Error.New("refused");
            internal static Fin<int> Arm(int count) => count switch { > 0 => {|#1:(Fin<int>)count|}, _ => Error.New("empty") };
            internal static Validation<Error, int> Paired(bool held, int value) => (held ? Fin.Succ(value) : {|#2:Fin.Fail<int>(Error.New("refused"))|}).ToValidation();
            internal static Validation<Error, int> Resolved(bool held, Fin<int> value) => (held ? value : {|#3:Fin.Fail<int>(Error.New("refused"))|}).ToValidation();
            internal static Validation<Error, int> Armed(int count, Fin<int> value) => (count switch { > 0 => value, _ => {|#4:Fin.Succ(count)|} }).ToValidation();
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Success<Error, Unit>(unit)", "unit", "Validation<Error, Unit>"),
            Diagnostic().WithLocation(1).WithArguments("(Fin<int>)count", "count", "Fin<int>"),
            Diagnostic().WithLocation(2).WithArguments("""Fin.Fail<int>(Error.New("refused"))""", """Error.New("refused")""", "Fin<int>"),
            Diagnostic().WithLocation(3).WithArguments("""Fin.Fail<int>(Error.New("refused"))""", """Error.New("refused")""", "Fin<int>"),
            Diagnostic().WithLocation(4).WithArguments("Fin.Succ(count)", "count", "Fin<int>"),
        ],
        """
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class ConvertedArm {
            internal static Validation<Error, Unit> Checked(bool held) => held ? unit : Error.New("refused");
            internal static Fin<int> Arm(int count) => count switch { > 0 => count, _ => Error.New("empty") };
            internal static Validation<Error, int> Paired(bool held, int value) => (held ? Fin.Succ(value) : Error.New("refused")).ToValidation();
            internal static Validation<Error, int> Resolved(bool held, Fin<int> value) => (held ? value : Error.New("refused")).ToValidation();
            internal static Validation<Error, int> Armed(int count, Fin<int> value) => (count switch { > 0 => value, _ => count }).ToValidation();
        }
        """);

    [Fact]
    public Task SiblingArm() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class SiblingArm {
            internal static Fin<Option<int>> Split(int count, Fin<Option<int>> valid, bool cancelled) =>
                (count switch {
                    0 when cancelled => {|#0:Fin.Fail<Option<int>>(Error.New("cancelled"))|},
                    0 => Option<int>.None,
                    1 => new Expected("absent", 1, None),
                    _ => valid,
                }).Map(static held => held);
            internal static Fin<int> Pinned(bool held, int pinned, Fin<int> found) => (held ? {|#1:Fin.Succ(pinned)|} : found.Map(static value => value + 1)).Map(static value => value * 2);
            internal static Fin<int> Merged(Fin<int> solved, Fin<int> value) =>
                solved.Match(Succ: _ => value, Fail: error => value.Match(Succ: _ => Fin.Fail<int>(error), Fail: fault => {|#2:Fin.Fail<int>(error + fault)|}));
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("""Fin.Fail<Option<int>>(Error.New("cancelled"))""", """Error.New("cancelled")""", "Fin<Option<int>>"),
            Diagnostic().WithLocation(1).WithArguments("Fin.Succ(pinned)", "pinned", "Fin<int>"),
            Diagnostic().WithLocation(2).WithArguments("Fin.Fail<int>(error + fault)", "error + fault", "Fin<int>"),
        ],
        """
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class SiblingArm {
            internal static Fin<Option<int>> Split(int count, Fin<Option<int>> valid, bool cancelled) =>
                (count switch {
                    0 when cancelled => Error.New("cancelled"),
                    0 => Option<int>.None,
                    1 => new Expected("absent", 1, None),
                    _ => valid,
                }).Map(static held => held);
            internal static Fin<int> Pinned(bool held, int pinned, Fin<int> found) => (held ? pinned : found.Map(static value => value + 1)).Map(static value => value * 2);
            internal static Fin<int> Merged(Fin<int> solved, Fin<int> value) =>
                solved.Match(Succ: _ => value, Fail: error => value.Match(Succ: _ => Fin.Fail<int>(error), Fail: fault => error + fault));
        }
        """);

    [Fact]
    public Task DefaultedArm() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class DefaultedArm {
            internal static Fin<int> Unset(int count) => {|CS8603:count switch { > 0 => (Fin<int>)count, _ => default }|};
            internal static Validation<Error, int> Typed(bool held, int value) => (held ? Fin.Succ(value) : Error.New("refused")).ToValidation();
            internal static Validation<Error, int> Defaulted(int count) => ({|CS8602:count switch { > 0 => default, _ => Fin.Succ(count) }|}).ToValidation();
        }
        """);

    [Fact]
    public Task InferredCallback() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class InferredCallback {
            internal static IO<Unit> Lifted(bool held) => IO.lift(() => held ? Fin.Succ(unit) : Error.New("refused"));
            internal static Func<int, Fin<int>> Wrapped() => fun(static (int held) => Fin.Succ(held));
        }
        """);

    [Fact]
    public Task InferredLocal() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class InferredLocal {
            internal static Fin<int> Held(int value) {
                var held = Fin.Succ(value);
                return held;
            }
            internal static Fin<int> Inferred(Fin<int> start) => start.Bind(value => Fin.Succ(value + 1));
        }
        """);

    [Fact]
    public Task DocumentedLift() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class DocumentedLift {
            internal static Fin<int> Kept(int value) => {|#0:Fin.Succ(/* value */ value)|};
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Fin.Succ(/* value */ value)", "value", "Fin<int>"),
        ],
        """
        using LanguageExt;

        internal static class DocumentedLift {
            internal static Fin<int> Kept(int value) => /* value */ value;
        }
        """);

    // --- [FORMS]

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

    [Fact]
    public Task ReportsEveryTargetTypedConversion() {
        CSharpCodeFixTest<ResultLiftAnalyzer, LanguageExtCodeFix> test = new();
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
            test.TestState.Sources.Add(($"/{name}.cs", Source(name,
                cases.Select(static (item, index) => item.Member(item.Type, item.Value, string.Create(CultureInfo.InvariantCulture, $"M{index}"))))));
        }
        return test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task FixesEachSpellingAcrossTargetContexts() {
        (string Before, string After)[] cases = [.. Forms("int", "1").Where(static form => form.Replacement.IsSome).Select(static (form, index) => {
            string name = string.Create(CultureInfo.InvariantCulture, $"M{index}");
            Func<string, string, string, string> member = TargetTyped[index % TargetTyped.Length];
            return (member(form.Type, $"{{|RASM1001:{form.Expression}|}}", name), member(form.Type, (string)form.Replacement, name));
        })];
        return new CSharpCodeFixTest<ResultLiftAnalyzer, LanguageExtCodeFix> {
            TestCode = Source("Sample", cases.Select(static item => item.Before)),
            FixedCode = Source("Sample", cases.Select(static item => item.After)),
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}
