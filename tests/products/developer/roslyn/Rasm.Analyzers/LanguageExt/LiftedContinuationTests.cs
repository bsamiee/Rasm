using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class LiftedContinuationTests : CSharpCodeFixVerifier<LiftedContinuationAnalyzer, LanguageExtCodeFix> {
    [Fact]
    public Task RewritesCallFreeBindContinuationsToMapAndKeepsContinuationsWithEffects() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Continued {
            private static int last;

            internal static IO<int> Pure(IO<int> source) => source.{|#0:Bind(value => IO.pure(value + 1))|};
            internal static IO<int> Lifted(IO<int> source) => source.{|#1:Bind(static value => IO.lift(() => value * 2))|};
            internal static IO<long> Widened(IO<int> source) => source.{|#2:Bind(static value => IO.pure<long>(value))|};
            internal static IO<int> Same(IO<int> source) => source.{|#3:Bind(static value => IO.pure(value))|};
            internal static IO<int> Replaced(IO<int> source, int other) => source.{|#6:Bind(_ => IO.pure(other))|};
            internal static IO<int> Twice(IO<int> source) => source.{|#4:Bind(static first => IO.pure(first + 1))|}.{|#5:Bind(static second => IO.pure(second * 2))|};
            internal static IO<int> Commented(IO<int> source) => source.{|RASM1011:Bind(value => /* next */ IO.pure(value + 1 /* one */))|};
            internal static IO<int> Called(IO<string> source) => source.Bind(static text => IO.lift(() => Measure(text)));
            internal static IO<int> Stored(IO<int> source) => source.Bind(static value => IO.lift(() => last = value));
            internal static IO<int> Stepped(IO<int> source) => source.Bind(static _ => IO.lift(() => ++last));

            private static int Measure(string text) => text.Length;
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Bind", "IO.pure(value + 1)", "Map(value => value + 1)"),
            Diagnostic().WithLocation(1).WithArguments("Bind", "IO.lift(() => value * 2)", "Map(static value => value * 2)"),
            Diagnostic().WithLocation(2).WithArguments("Bind", "IO.pure<long>(value)", "Map<long>(static value => value)"),
            Diagnostic().WithLocation(3).WithArguments("Bind", "IO.pure(value)", "source"),
            Diagnostic().WithLocation(4).WithArguments("Bind", "IO.pure(first + 1)", "Map(static first => first + 1)"),
            Diagnostic().WithLocation(5).WithArguments("Bind", "IO.pure(second * 2)", "Map(static second => second * 2)"),
            Diagnostic().WithLocation(6).WithArguments("Bind", "IO.pure(other)", "Map(_ => other)"),
        ],
        """
        using LanguageExt;

        internal static class Continued {
            private static int last;

            internal static IO<int> Pure(IO<int> source) => source.Map(value => value + 1);
            internal static IO<int> Lifted(IO<int> source) => source.Map(static value => value * 2);
            internal static IO<long> Widened(IO<int> source) => source.Map<long>(static value => value);
            internal static IO<int> Same(IO<int> source) => source;
            internal static IO<int> Replaced(IO<int> source, int other) => source.Map(_ => other);
            internal static IO<int> Twice(IO<int> source) => source.Map(static first => first + 1).Map(static second => second * 2);
            internal static IO<int> Commented(IO<int> source) => source.Map(value => /* next */ value + 1 /* one */);
            internal static IO<int> Called(IO<string> source) => source.Bind(static text => IO.lift(() => Measure(text)));
            internal static IO<int> Stored(IO<int> source) => source.Bind(static value => IO.lift(() => last = value));
            internal static IO<int> Stepped(IO<int> source) => source.Bind(static _ => IO.lift(() => ++last));

            private static int Measure(string text) => text.Length;
        }
        """);

    [Fact]
    public Task RewritesCallFreeLaterFromClauseToLetAndKeepsFromClausesWithEffects() => VerifyCodeFixAsync(
        """
        using LanguageExt;

        internal static class Queried {
            internal static IO<int> Pure(IO<int> source) =>
                from value in source
                from next in {|#0:IO.pure(value + 1)|}
                select next;

            internal static IO<int> Measured(IO<string> source) =>
                from text in source
                from length in IO.lift(() => Measure(text))
                select length;

            internal static IO<int> Started =>
                from value in IO.pure(1)
                from next in Next(value)
                select next;

            private static int Measure(string text) => text.Length;

            private static IO<int> Next(int value) => IO.lift(() => Measure($"{value}"));
        }
        """,
        [Diagnostic().WithLocation(0).WithArguments("SelectMany", "IO.pure(value + 1)", "let next = value + 1")],
        """
        using LanguageExt;

        internal static class Queried {
            internal static IO<int> Pure(IO<int> source) =>
                from value in source
                let next = value + 1
                select next;

            internal static IO<int> Measured(IO<string> source) =>
                from text in source
                from length in IO.lift(() => Measure(text))
                select length;

            internal static IO<int> Started =>
                from value in IO.pure(1)
                from next in Next(value)
                select next;

            private static int Measure(string text) => text.Length;

            private static IO<int> Next(int value) => IO.lift(() => Measure($"{value}"));
        }
        """);

    [Fact]
    public Task LiftsTheValueOfAFinOrEitherThunkAndKeepsOneBoundOnAnExplicitlyTypedLift() => VerifyCodeFixAsync(
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
            internal static IO<int> Kept(string text) => IO.lift<Fin<int>>(() => Parse(text)).Bind(static parsed => IO.lift(() => parsed));

            private static Fin<int> Parse(string text) => text.Length;
            private static Either<Error, int> Halve(string text) => text.Length / 2;
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Bind", "IO.lift(() => Parse(text))", "IO.lift(Parse(text))"),
            Diagnostic().WithLocation(1).WithArguments("Bind", "IO.lift(() => Halve(text))", "IO.lift(Halve(text))"),
            Diagnostic().WithLocation(2).WithArguments("SelectMany", "IO.lift(() => Parse(text))", "IO.lift(Parse(text))"),
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
            internal static IO<int> Kept(string text) => IO.lift<Fin<int>>(() => Parse(text)).Bind(static parsed => IO.lift(() => parsed));

            private static Fin<int> Parse(string text) => text.Length;
            private static Either<Error, int> Halve(string text) => text.Length / 2;
        }
        """);
}
