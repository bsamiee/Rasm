using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.ThinktectureRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ValidationMessageTests : CSharpCodeFixVerifier<ValidationMessageAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ConvertedByHand() => VerifyAnalyzerAsync(
        """
        using LanguageExt.Common;
        using Thinktecture;

        internal static class ConvertedByHand {
            internal static Error Raised(ValidationError error) => {|#0:Error.New(error.Message)|};
            internal static Error Expected(ValidationError error) => {|#1:new Expected(error.Message, 0)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Error.New(error.Message)"),
        Diagnostic().WithLocation(1).WithArguments("new Expected(error.Message, 0)"));

    [Fact]
    public Task MessageRead() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        internal static class MessageRead {
            internal static string Logged(ValidationError error) => $"invalid: {error.Message}";
            internal static int Length(ValidationError error) => error.Message.Length;
        }
        """);
}
