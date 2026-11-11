using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.ThinktectureRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefaultValidationErrorTests : CSharpCodeFixVerifier<DefaultValidationErrorAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ConstrainedError() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        internal sealed class Fault : IValidationError<Fault> {
            public static Fault Create(string message) => new();
        }

        internal static class ConstrainedError {
            internal static bool Valid<TCount>(int value) where TCount : IObjectFactory<TCount, int, Fault> => TCount.Validate(value, null, out _) is null;
        }
        """);

    [Fact]
    public Task ValidateOutsideFactory() => VerifyAnalyzerAsync(
        """
        using System;
        using Thinktecture;

        internal sealed class Count : IObjectFactory<Count, int, ValidationError> {
            public static ValidationError? Validate(int value, IFormatProvider? provider, out Count? item) {
                item = value > 0 ? new Count() : null;
                return item is null ? new ValidationError("count is not positive") : null;
            }
        }

        internal static class ValidateOutsideFactory {
            internal static bool Valid(int value) => {|#0:Count.Validate(value, null, out _)|} is null;

            internal static bool Defaulted<TCount>(int value) where TCount : IObjectFactory<TCount, int, ValidationError> => {|#1:TCount.Validate(value, null, out _)|} is null;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Count.Validate(value, null, out _)", "Count"),
        Diagnostic().WithLocation(1).WithArguments("TCount.Validate(value, null, out _)", "TCount"));

    [Fact]
    public Task ValidationHook() => VerifyAnalyzerAsync(
        """
        using System;
        using Thinktecture;

        internal sealed class Count : IObjectFactory<Count, int, ValidationError> {
            public static ValidationError? Validate(int value, IFormatProvider? provider, out Count? item) {
                item = value > 0 ? new Count() : null;
                return item is null ? new ValidationError("count is not positive") : null;
            }
        }

        internal static class ValidationHook {
            internal static ValidationError? Hook(int value) => Count.Validate(value, null, out _);
        }
        """);
}
