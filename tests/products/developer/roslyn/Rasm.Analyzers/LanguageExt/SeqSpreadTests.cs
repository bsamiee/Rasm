using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SeqSpreadTests : CSharpCodeFixVerifier<SeqSpreadAnalyzer, LanguageExtCodeFix> {
    [Fact]
    public Task RewritesSpreadThatCopiesTheSeqStepAccumulatorAndKeepsSpreadsOfOtherSequencesAndArrays() => VerifyCodeFixAsync(
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class Accumulated {
            internal static Seq<int> Folded(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => {|#0:[.. /* source */ held, /* item */ value * 2, /* end */]|});
            internal static Seq<int> Looped(int[] values) {
                Seq<int> held = [];
                foreach (int value in values)
                    held = {|#1:[.. held, value]|};
                return held;
            }
            internal static int[] Arrayed(int[] values) {
                int[] copied = [];
                foreach (int value in values)
                    copied = [.. copied, value];
                return copied;
            }
            internal static Seq<int> Seeded(Seq<int> values, Seq<int> seed) => values.Fold(Seq<int>(), (held, value) => [.. seed, value]);
            internal static Seq<int> Copied(int[] values, Seq<int> seed) {
                Seq<int> held = seed;
                Seq<int> last = [];
                foreach (int value in values)
                    last = [.. held, value];
                return last;
            }
            internal static Seq<int> Joined(Seq<int> head, int tail) => [.. head, tail];
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("[.. /* source */ held, /* item */ value * 2, /* end */]", "held", "value * 2"),
            Diagnostic().WithLocation(1).WithArguments("[.. held, value]", "held", "value"),
        ],
        """
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class Accumulated {
            internal static Seq<int> Folded(Seq<int> values) => values.Fold(Seq<int>(), static (held, value) => /* source */ held.Add(/* item */ value * 2 /* end */));
            internal static Seq<int> Looped(int[] values) {
                Seq<int> held = [];
                foreach (int value in values)
                    held = held.Add(value);
                return held;
            }
            internal static int[] Arrayed(int[] values) {
                int[] copied = [];
                foreach (int value in values)
                    copied = [.. copied, value];
                return copied;
            }
            internal static Seq<int> Seeded(Seq<int> values, Seq<int> seed) => values.Fold(Seq<int>(), (held, value) => [.. seed, value]);
            internal static Seq<int> Copied(int[] values, Seq<int> seed) {
                Seq<int> held = seed;
                Seq<int> last = [];
                foreach (int value in values)
                    last = [.. held, value];
                return last;
            }
            internal static Seq<int> Joined(Seq<int> head, int tail) => [.. head, tail];
        }
        """);
}
