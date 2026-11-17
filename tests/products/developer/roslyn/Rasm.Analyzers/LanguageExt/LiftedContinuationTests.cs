using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class LiftedContinuationTests : CSharpCodeFixVerifier<LiftedContinuationAnalyzer, LanguageExtCodeFix> {
    [Fact]
    public Task RewritesBindOverPureValueOrCallFreeThunkToMap() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Projected {
            internal static IO<int> Pure(IO<int> source) => source.{|#0:Bind(value => IO.pure(value + 1))|};

            internal static IO<int> Lifted(IO<int> source) => source.{|#1:Bind(static value => IO.lift(() => value * 2))|};

            internal static IO<long> Widened(IO<int> source) => source.{|#2:Bind(static value => IO.pure<long>(value))|};

            internal static IO<int> Same(IO<int> source) => source.{|#3:Bind(static value => IO.pure(value))|};
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Bind", "IO.pure(value + 1)", "Map(value => value + 1)"),
            Diagnostic().WithLocation(1).WithArguments("Bind", "IO.lift(() => value * 2)", "Map(static value => value * 2)"),
            Diagnostic().WithLocation(2).WithArguments("Bind", "IO.pure<long>(value)", "Map<long>(static value => value)"),
            Diagnostic().WithLocation(3).WithArguments("Bind", "IO.pure(value)", "source"),
        ],
        """
        using LanguageExt;

        internal static class Projected {
            internal static IO<int> Pure(IO<int> source) => source.Map(value => value + 1);

            internal static IO<int> Lifted(IO<int> source) => source.Map(static value => value * 2);

            internal static IO<long> Widened(IO<int> source) => source.Map<long>(static value => value);

            internal static IO<int> Same(IO<int> source) => source;
        }
        """);

    [Fact]
    public Task RewritesLaterFromClauseOverPureValueOrCallFreeThunkToLet() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Queried {
            internal static IO<int> Pure(IO<int> source) =>
                from value in source
                from next in {|#0:IO.pure(value + 1)|}
                select next;

            internal static IO<int> Lifted(IO<int> source) =>
                from value in source
                from doubled in {|#1:IO.lift(() => value * 2)|}
                select value + doubled;
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("SelectMany", "IO.pure(value + 1)", "let next = value + 1"),
            Diagnostic().WithLocation(1).WithArguments("SelectMany", "IO.lift(() => value * 2)", "let doubled = value * 2"),
        ],
        """
        using LanguageExt;

        internal static class Queried {
            internal static IO<int> Pure(IO<int> source) =>
                from value in source
                let next = value + 1
                select next;

            internal static IO<int> Lifted(IO<int> source) =>
                from value in source
                let doubled = value * 2
                select value + doubled;
        }
        """);

    [Fact]
    public Task RewritesFinOrEitherThunkFromContinuationToLiftedValue() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class Evaluated {
            internal static IO<int> Parsed(IO<string> source) => source.Bind(static text => {|#0:IO.lift(() => Parse(text))|});

            internal static IO<int> Split(IO<string> source) => source.Bind(static text => {|#1:IO.lift(() => Halve(text))|});

            internal static IO<int> Queried(IO<string> source) =>
                from text in source
                from parsed in {|#2:IO.lift(() => Parse(text))|}
                select parsed;

            internal static IO<int> Selected(IO<string> source) => source.SelectMany(static text => {|#3:IO.lift(() => Parse(text))|}, static (_, parsed) => parsed);

            private static Fin<int> Parse(string text) => text.Length;

            private static Either<Error, int> Halve(string text) => text.Length / 2;
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Bind", "IO.lift(() => Parse(text))", "IO.lift(Parse(text))"),
            Diagnostic().WithLocation(1).WithArguments("Bind", "IO.lift(() => Halve(text))", "IO.lift(Halve(text))"),
            Diagnostic().WithLocation(2).WithArguments("SelectMany", "IO.lift(() => Parse(text))", "IO.lift(Parse(text))"),
            Diagnostic().WithLocation(3).WithArguments("SelectMany", "IO.lift(() => Parse(text))", "IO.lift(Parse(text))"),
        ],
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class Evaluated {
            internal static IO<int> Parsed(IO<string> source) => source.Bind(static text => IO.lift(Parse(text)));

            internal static IO<int> Split(IO<string> source) => source.Bind(static text => IO.lift(Halve(text)));

            internal static IO<int> Queried(IO<string> source) =>
                from text in source
                from parsed in IO.lift(Parse(text))
                select parsed;

            internal static IO<int> Selected(IO<string> source) => source.SelectMany(static text => IO.lift(Parse(text)), static (_, parsed) => parsed);

            private static Fin<int> Parse(string text) => text.Length;

            private static Either<Error, int> Halve(string text) => text.Length / 2;
        }
        """);

    [Fact]
    public Task RewritesChainedAndNestedContinuationsInOneFixAllPass() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Chained {
            internal static IO<int> Twice(IO<int> source) => source.{|#0:Bind(static first => IO.pure(first + 1))|}.{|#1:Bind(static second => IO.pure(second * 2))|};

            internal static IO<int> Nested(IO<int> source, IO<int> other) => source.Bind(first => other.{|#2:Bind(second => IO.pure(first + second))|});
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Bind", "IO.pure(first + 1)", "Map(static first => first + 1)"),
            Diagnostic().WithLocation(1).WithArguments("Bind", "IO.pure(second * 2)", "Map(static second => second * 2)"),
            Diagnostic().WithLocation(2).WithArguments("Bind", "IO.pure(first + second)", "Map(second => first + second)"),
        ],
        """
        using LanguageExt;

        internal static class Chained {
            internal static IO<int> Twice(IO<int> source) => source.Map(static first => first + 1).Map(static second => second * 2);

            internal static IO<int> Nested(IO<int> source, IO<int> other) => source.Bind(first => other.Map(second => first + second));
        }
        """);

    [Fact]
    public Task KeepsCommentsBesideLiftedValueThroughFix() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Commented {
            internal static IO<int> Next(IO<int> source) => source.{|RASM1011:Bind(value => /* next */ IO.pure(value + 1 /* one */))|};
        }
        """,
        """
        using LanguageExt;

        internal static class Commented {
            internal static IO<int> Next(IO<int> source) => source.Map(value => /* next */ value + 1 /* one */);
        }
        """);

    [Fact]
    public Task IgnoresSideEffectingThunkMapPartialResultAndFirstFromClause() => VerifyAnalyzerAsync(
        """
        using System.Text;
        using LanguageExt;

        internal static class Deferred {
            private static int last;

            internal static IO<int> Called(IO<string> source) => source.Bind(static text => IO.lift(() => Measure(text)));

            internal static IO<int> Stored(IO<int> source) => source.Bind(static value => IO.lift(() => last = value));

            internal static IO<int> Stepped(IO<int> source) => source.Bind(static _ => IO.lift(() => ++last));

            internal static IO<StringBuilder> Built(IO<string> source) => source.Bind(static text => IO.lift(() => new StringBuilder(text)));

            internal static IO<int> Mapped(IO<int> source) => source.Map(static value => value + 1);

            internal static IO<int> Chosen(IO<int> source, IO<int> other) => source.Bind(value => value > 0 ? IO.pure(value) : other);

            internal static IO<int> First => IO.pure(1);

            internal static IO<int> Started =>
                from value in IO.pure(1)
                from next in Next(value)
                select next;

            private static int Measure(string text) => text.Length;

            private static IO<int> Next(int value) => IO.lift(() => Measure($"{value}"));
        }
        """);
}
