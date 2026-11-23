using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ResultLiftTests : CSharpCodeFixVerifier<ResultLiftAnalyzer, LanguageExtCodeFix> {

    // --- [RASM1001]

    [Fact]
    public Task RewritesFinFactoryReturnedFromCallbackToValue() => VerifyCodeFixAsync(
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
    public Task RewritesFactoryOrCastInConditionalOrSwitchArmToValue() => VerifyCodeFixAsync(
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
    public Task RewritesAFactoryInANestedCallbackWhoseSiblingStatesTheResultType() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class SiblingArm {
            internal static Fin<int> Merged(Fin<int> solved, Fin<int> value) =>
                solved.Match(Succ: _ => value, Fail: error => value.Match(Succ: _ => Fin.Fail<int>(error), Fail: fault => {|#0:Fin.Fail<int>(error + fault)|}));
        }
        """,
        [Diagnostic().WithLocation(0).WithArguments("Fin.Fail<int>(error + fault)", "error + fault", "Fin<int>")],
        """
        using LanguageExt;

        internal static class SiblingArm {
            internal static Fin<int> Merged(Fin<int> solved, Fin<int> value) =>
                solved.Match(Succ: _ => value, Fail: error => value.Match(Succ: _ => Fin.Fail<int>(error), Fail: fault => error + fault));
        }
        """);

    [Fact]
    public Task IgnoresFactoryArmThatAloneStatesResultType() => VerifyAnalyzerAsync(
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
    public Task IgnoresFactoryInferringLambdaReturnType() => VerifyAnalyzerAsync(
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
    public Task IgnoresFactoryInferringVarLocalOrBindReturnType() => VerifyAnalyzerAsync(
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

    // --- [FORMS]

    [Fact]
    public Task RewritesEachFactorySpellingInEachTargetTypedContextAndKeepsFactoriesWhoseValueWouldNotConvertToTheSameCase() => VerifyCodeFixAsync(
        """
        using System;
        using System.Threading.Tasks;
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class Spelled {
            internal static Fin<int> Arrow() => {|#0:Fin.Succ(/* value */ 1)|};
            internal static Fin<int> Returned() { return {|#1:Pure(1)|}; }
            internal static readonly Fin<int> Field = {|#2:new Fin<int>.Succ(1)|};
            internal static Fin<int> Property { get; } = {|#3:Fin.Fail<int>(Error.New("refused"))|};
            internal static Fin<int> Branched(bool held) => held ? {|#4:Fail(Error.New("refused"))|} : Branched(false);
            internal static Fin<int> Passed() { return Take({|#5:new Fin<int>.Fail(Error.New("refused"))|}); static Fin<int> Take(Fin<int> value) => value; }
            internal static Validation<Error, int> Assigned() { Validation<Error, int> held; held = {|#6:Success<Error, int>(1)|}; return held; }
            internal static Func<Validation<Error, int>> Deferred() => () => {|#7:Validation.Success<Error, int>(1)|};
            internal static Func<Task<Validation<Error, int>>> Awaited() => async () => { await Task.Yield(); return {|#8:Fail<Error, int>(Error.New("refused"))|}; };
            internal static Validation<Error, int> Refused() => {|#9:Validation.Fail<Error, int>(Error.New("refused"))|};
            internal static Fin<Expected> Narrowed() => {|#10:Fin.Succ(new Expected("held", 1, None))|};
            internal static Fin<Error> Ambiguous() => Fin.Succ(Error.New("held"));
            internal static Fin<string> Described() => Fin.Fail<string>("refused");
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Fin.Succ(/* value */ 1)", "1", "Fin<int>"),
            Diagnostic().WithLocation(1).WithArguments("Pure(1)", "1", "Fin<int>"),
            Diagnostic().WithLocation(2).WithArguments("new Fin<int>.Succ(1)", "1", "Fin<int>"),
            Diagnostic().WithLocation(3).WithArguments("""Fin.Fail<int>(Error.New("refused"))""", """Error.New("refused")""", "Fin<int>"),
            Diagnostic().WithLocation(4).WithArguments("""Fail(Error.New("refused"))""", """Error.New("refused")""", "Fin<int>"),
            Diagnostic().WithLocation(5).WithArguments("""new Fin<int>.Fail(Error.New("refused"))""", """Error.New("refused")""", "Fin<int>"),
            Diagnostic().WithLocation(6).WithArguments("Success<Error, int>(1)", "1", "Validation<Error, int>"),
            Diagnostic().WithLocation(7).WithArguments("Validation.Success<Error, int>(1)", "1", "Validation<Error, int>"),
            Diagnostic().WithLocation(8).WithArguments("""Fail<Error, int>(Error.New("refused"))""", """Error.New("refused")""", "Validation<Error, int>"),
            Diagnostic().WithLocation(9).WithArguments("""Validation.Fail<Error, int>(Error.New("refused"))""", """Error.New("refused")""", "Validation<Error, int>"),
            Diagnostic().WithLocation(10).WithArguments("""Fin.Succ(new Expected("held", 1, None))""", """new Expected("held", 1, None)""", "Fin<Expected>"),
        ],
        """
        using System;
        using System.Threading.Tasks;
        using LanguageExt;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class Spelled {
            internal static Fin<int> Arrow() => /* value */ 1;
            internal static Fin<int> Returned() { return 1; }
            internal static readonly Fin<int> Field = 1;
            internal static Fin<int> Property { get; } = Error.New("refused");
            internal static Fin<int> Branched(bool held) => held ? Error.New("refused") : Branched(false);
            internal static Fin<int> Passed() { return Take(Error.New("refused")); static Fin<int> Take(Fin<int> value) => value; }
            internal static Validation<Error, int> Assigned() { Validation<Error, int> held; held = 1; return held; }
            internal static Func<Validation<Error, int>> Deferred() => () => 1;
            internal static Func<Task<Validation<Error, int>>> Awaited() => async () => { await Task.Yield(); return Error.New("refused"); };
            internal static Validation<Error, int> Refused() => Error.New("refused");
            internal static Fin<Expected> Narrowed() => new Expected("held", 1, None);
            internal static Fin<Error> Ambiguous() => Fin.Succ(Error.New("held"));
            internal static Fin<string> Described() => Fin.Fail<string>("refused");
        }
        """);

    // --- [FIX_ALL]

    [Fact]
    public Task FixAllOfRedundantAsDeletesConcreteNarrowingAloneAndKeepsTraitAndCustomAs() {
        const string source = """
            using LanguageExt;
            using LanguageExt.Traits;

            internal static class Sample {
                internal static Fin<int> Lift(int value) => {|#0:{|RASM1001:Fin.Succ(value)|}|};
                internal static Fin<int> Narrow(Fin<int> value) => value.{|RASM1007:As()|};
                internal static Option<int> Traited(K<Option, int> value) => value.As();
            }

            namespace Custom {
                internal static class CustomNarrowing {
                    internal static Fin<int> Preserve(Fin<int> value) => value.As();

                    internal static Fin<int> As(this K<Fin, int> value) => ((Fin<int>)value).Map(static held => held + 99);
                }
            }
            """;
        return new CSharpCodeFixTest<RedundantNarrowingAnalyzer, LanguageExtCodeFix> {
            TestCode = source,
            FixedCode = source.Replace("value.{|RASM1007:As()|}", "value", StringComparison.Ordinal),
            FixedState = { ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("Fin.Succ(value)", "value", "Fin<int>") }, },
            AdditionalAnalyzers = { new ResultLiftAnalyzer() },
            CodeActionEquivalenceKey = "RASM1007",
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}
