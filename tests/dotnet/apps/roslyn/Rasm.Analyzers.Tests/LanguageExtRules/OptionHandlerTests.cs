using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OptionHandlerTests : CSharpCodeFixVerifier<OptionHandlerAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM1008]

    [Fact]
    public Task PassedThrough() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class PassedThrough {
            internal static int Present(Option<int> value) => {|#0:value.Match(Some: static held => held, None: static () => 0)|};
            internal static int Succeeded(Fin<int> value) => {|#1:value.Match(Succ: static held => held, Fail: static _ => 0)|};
        }
        """,
        Diagnostic("RASM1008").WithLocation(0).WithArguments("Match", "Some", "value.IfNone(static () => 0)"),
        Diagnostic("RASM1008").WithLocation(1).WithArguments("Match", "Succ", "value.IfFail(static _ => 0)"));

    [Fact]
    public Task BothHandled() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class BothHandled {
            internal static int Present(Option<int> value) => value.Match(Some: static held => held + 1, None: static () => 0);
            internal static int Succeeded(Fin<int> value) => value.Match(Succ: static held => held * 2, Fail: static error => error.Code);
        }
        """);

    // --- [RASM1009]

    [Fact]
    public Task NullableByHand() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class NullableByHand {
            internal static string? Matched(Option<string> value) => {|#0:value.Match(Some: static held => held, None: static () => {|CS8603:null|})|};
            internal static string? Cased(Option<string> value) => {|#1:(string?)value.Case|};
            internal static string? Tested(Option<string> value) => {|#2:value.IsSome ? (string)value : null|};
            internal static int? Lifted(Option<int> value) => {|#3:value.Match(Some: static held => (int?)held, None: static () => null)|};
            internal static int? Cast(Option<int> value) => {|#4:(int?)value.Case|};

            internal static string? Property(HeldOption source) => {|#5:source.Value.IsSome ? (string)source.Value : null|};
        }

        internal sealed class HeldOption {
            internal Option<string> Value { get; set; }
        }
        """,
        Diagnostic("RASM1009").WithLocation(0).WithArguments("value", "value.ValueUnsafe()"),
        Diagnostic("RASM1009").WithLocation(1).WithArguments("value", "value.ValueUnsafe()"),
        Diagnostic("RASM1009").WithLocation(2).WithArguments("value", "value.ValueUnsafe()"),
        Diagnostic("RASM1009").WithLocation(3).WithArguments("value", "value.Map<int?>(static held => (int?)held).ValueUnsafe()"),
        Diagnostic("RASM1009").WithLocation(4).WithArguments("value", "value.ToNullable()"),
        Diagnostic("RASM1009").WithLocation(5).WithArguments("source.Value", "source.Value.ValueUnsafe()"));

    [Fact]
    public Task NullableKept() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class NullableKept {
            internal static string Defaulted(Option<string> value) => value.IfNone("");
            internal static int Length(Option<string> value) => value.Match(Some: static held => held.Length, None: static () => 0);

            internal static int Required(Option<int> value) => {|CS8605:(int)value.Case|};

            internal static int Zero(Option<int> value) => value.IsSome ? (int)value : default(int);

            internal static string? Repeated(ChangingOption source) => source.Value.IsSome ? (string)source.Value : null;

            internal static string? FieldBacked(FieldBackedOption source) => source.Value.IsSome ? (string)source.Value : null;
        }

        internal sealed class ChangingOption {
            private int reads;

            internal Option<string> Value => ++reads == 1 ? "present" : Option<string>.None;
        }

        internal sealed class FieldBackedOption {
            internal Option<string> Value {
                get => field = field.IsSome ? Option<string>.None : "present";
                set;
            }
        }
        """);
}
