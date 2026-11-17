using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OptionToNullableTests : CSharpCodeFixVerifier<OptionHandlerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsOptionToNullableByMatchCaseCastOrIsSomeConditional() => VerifyAnalyzerAsync(
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
    public Task IgnoresNonNullableResultOrOptionPropertyChangingPerRead() => VerifyAnalyzerAsync(
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
