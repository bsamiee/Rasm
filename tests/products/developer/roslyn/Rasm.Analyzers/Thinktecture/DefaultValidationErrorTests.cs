using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Thinktecture;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefaultValidationErrorTests : CSharpCodeFixVerifier<DefaultValidationErrorAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ValidationRaisingTheDefaultErrorIsReportedWhileADomainErrorHooksAndSmartEnumLookupsAreSilent() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        internal sealed class Fault : IValidationError<Fault> {
            public static Fault Create(string message) => new();
        }

        [ValueObject<int>]
        internal sealed partial class Count {
            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
                validationError = value > 0 ? null : new ValidationError("count is not positive");
        }

        [ValueObject<int>]
        [ValidationError<Fault>]
        internal sealed partial class Width {
            static partial void ValidateFactoryArguments(ref Fault? validationError, ref int value) =>
                validationError = value > 0 ? null : new Fault();
        }

        [SmartEnum<string>]
        internal sealed partial class Kind {
            public static readonly Kind Plain = new("plain");
            public static readonly Kind Bold = new("bold");
        }

        internal static class Reads {
            internal static bool Counted(int value) => {|#0:Count.TryCreate(value, out _)|};

            internal static bool Validated(int value) => {|#2:Count.Validate(value, null, out _)|} is null;

            internal static bool Defaulted<TCount>(int value) where TCount : IObjectFactory<TCount, int, ValidationError> => {|#1:TCount.Validate(value, null, out _)|} is null;

            internal static bool Widened(int value) => Width.TryCreate(value, out _);

            internal static bool Constrained<TWidth>(int value) where TWidth : IObjectFactory<TWidth, int, Fault> => TWidth.Validate(value, null, out _) is null;

            internal static ValidationError? Hook(int value) => Count.Validate(value, null, out _);

            internal static void Nested(ref ValidationError? error, int value) => error = Count.Validate(value, null, out _);

            internal static bool Known(string key) => Kind.TryGet(key, out _);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Count.TryCreate(value, out _)", "Count"),
        Diagnostic().WithLocation(1).WithArguments("TCount.Validate(value, null, out _)", "TCount"),
        Diagnostic().WithLocation(2).WithArguments("Count.Validate(value, null, out _)", "Count"));
}
