using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RedundantNarrowingTests : CSharpCodeFixVerifier<RedundantNarrowingAnalyzer, LanguageExtCodeFix> {

    // --- [RASM1007]

    [Fact]
    public Task NarrowedConcrete() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using LanguageExt.Traits;
        using Narrowing = LanguageExt.OptionExtensions;

        internal static class NarrowedConcrete {
            internal static Fin<int> Held(Fin<int> value) => value.{|#0:As()|};
            internal static Option<int> Mapped(Option<int> value) => value.Map(static held => held + 1).{|#1:As()|};

            internal static Fin<int> Static(Fin<int> value) => FinExtensions.{|#2:As(value)|};

            internal static Option<int> Aliased(Option<int> value) => Narrowing.{|#3:As(value)|};

            internal static Option<int> Selected(bool first, Option<int> left, Option<int> right) => (first ? left : right).{|#4:As()|}.Map(static held => held + 1);

            internal static Option<int> Queried(Option<int> value) => (from held in value select held + 1).{|#5:As()|}.Map(static held => held * 2);

            internal static Option<int> Cast(K<Option, int> value) => ((Option<int>)value).{|#6:As()|}.Map(static held => held + 1);

            internal static Fin<int> Commented(Fin<int> value) => value.{|#7:As(/* narrowing */)|};
        }

        namespace Custom {
            internal static class CustomNarrowing {
                internal static Fin<int> Preserve(Fin<int> value) => value.As();

                internal static Fin<int> As(this K<Fin, int> value) => ((Fin<int>)value).Map(static held => held + 99);
            }
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("As", "Fin<int>"),
            Diagnostic().WithLocation(1).WithArguments("As", "Option<int>"),
            Diagnostic().WithLocation(2).WithArguments("As", "Fin<int>"),
            Diagnostic().WithLocation(3).WithArguments("As", "Option<int>"),
            Diagnostic().WithLocation(4).WithArguments("As", "Option<int>"),
            Diagnostic().WithLocation(5).WithArguments("As", "Option<int>"),
            Diagnostic().WithLocation(6).WithArguments("As", "Option<int>"),
            Diagnostic().WithLocation(7).WithArguments("As", "Fin<int>"),
        ],
        """
        using LanguageExt;
        using LanguageExt.Traits;
        using Narrowing = LanguageExt.OptionExtensions;

        internal static class NarrowedConcrete {
            internal static Fin<int> Held(Fin<int> value) => value;
            internal static Option<int> Mapped(Option<int> value) => value.Map(static held => held + 1);

            internal static Fin<int> Static(Fin<int> value) => value;

            internal static Option<int> Aliased(Option<int> value) => value;

            internal static Option<int> Selected(bool first, Option<int> left, Option<int> right) => (first ? left : right).Map(static held => held + 1);

            internal static Option<int> Queried(Option<int> value) => (from held in value select held + 1).Map(static held => held * 2);

            internal static Option<int> Cast(K<Option, int> value) => ((Option<int>)value).Map(static held => held + 1);

            internal static Fin<int> Commented(Fin<int> value) => value /* narrowing */;
        }

        namespace Custom {
            internal static class CustomNarrowing {
                internal static Fin<int> Preserve(Fin<int> value) => value.As();

                internal static Fin<int> As(this K<Fin, int> value) => ((Fin<int>)value).Map(static held => held + 99);
            }
        }
        """);

    [Fact]
    public Task NarrowedTrait() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Traits;

        internal static class NarrowedTrait {
            internal static Fin<int> Applied(Fin<int> left, Fin<int> right) => (left, right).Apply(static (first, second) => first + second).As();
            internal static Option<int> Traited(K<Option, int> value) => value.As();
        }

        namespace Custom {
            internal static class CustomNarrowing {
                internal static Fin<int> Preserve(Fin<int> value) => value.As();

                internal static Fin<int> As(this K<Fin, int> value) => ((Fin<int>)value).Map(static held => held + 99);
            }
        }
        """);

    // --- [FIX_ALL]

    [Fact]
    public Task SelectedFixAll() {
        const string source = """
            using LanguageExt;

            internal static class Sample {
                internal static Fin<int> Lift(int value) => {|#0:{|RASM1001:Fin.Succ(value)|}|};
                internal static Fin<int> Narrow(Fin<int> value) => value.{|RASM1007:As()|};
            }
            """;
        return new CSharpCodeFixTest<RedundantNarrowingAnalyzer, LanguageExtCodeFix> {
            TestCode = source,
            FixedCode = source.Replace("value.{|RASM1007:As()|}", "value", StringComparison.Ordinal),
            FixedState = { ExpectedDiagnostics = {
                CSharpCodeFixVerifier<ResultLiftAnalyzer, LanguageExtCodeFix>.Diagnostic().WithLocation(0).WithArguments("Fin.Succ(value)", "value", "Fin<int>"),
            }, },
            AdditionalAnalyzers = { new ResultLiftAnalyzer() },
            CodeActionEquivalenceKey = "RASM1007",
        }.RunAsync(TestContext.Current.CancellationToken);
    }
}
