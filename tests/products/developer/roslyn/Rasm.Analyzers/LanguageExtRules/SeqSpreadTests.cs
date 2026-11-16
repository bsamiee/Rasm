using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SeqSpreadTests : CSharpCodeFixVerifier<SeqSpreadAnalyzer, LanguageExtCodeFix> {
    [Fact]
    public Task AccumulatorSpread() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class AccumulatorSpread {
            internal static Seq<int> Folded(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => {|#0:[.. held, value * 2]|});
            internal static Seq<int> Looped(int[] values) {
                Seq<int> held = [];
                foreach (int value in values)
                    held = {|#1:[.. held, value]|};
                return held;
            }

            internal static Seq<int> Commented(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => {|#2:[.. /* source */ held, /* item */ value, /* end */]|});
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("[.. held, value * 2]", "held", "value * 2"),
            Diagnostic().WithLocation(1).WithArguments("[.. held, value]", "held", "value"),
            Diagnostic().WithLocation(2).WithArguments("[.. /* source */ held, /* item */ value, /* end */]", "held", "value"),
        ],
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class AccumulatorSpread {
            internal static Seq<int> Folded(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => held.Add(value * 2));
            internal static Seq<int> Looped(int[] values) {
                Seq<int> held = [];
                foreach (int value in values)
                    held = held.Add(value);
                return held;
            }

            internal static Seq<int> Commented(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => /* source */ held.Add(/* item */ value /* end */));
        }
        """);

    [Fact]
    public Task OneTimeSpread() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class OneTimeSpread {
            internal static Seq<int> Joined(Seq<int> head, int tail) => [.. head, tail];
        }
        """);
}
