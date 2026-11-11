using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.ThinktectureRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ThrowingFactoryTests : CSharpCodeFixVerifier<ThrowingFactoryAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task Meter() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Thinktecture;
        using static LanguageExt.Prelude;

        [ComplexValueObject]
        internal sealed partial class Progress {
            internal long Done { get; }

            internal long Total { get; }

            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long done, ref long total) =>
                validationError = total > 0 && done <= total ? null : new ValidationError("progress is out of range");
        }

        internal sealed class Meter(long total) {
            internal static readonly Seq<Progress> Units = Seq(1, 2).Map(x => Progress.Create(x, 1));

            internal Progress Reported(int value) => {|#0:Progress.Create(value, total)|};

            internal Seq<Progress> Totals(Seq<long> dones, Seq<long> totals) => dones.Zip(totals, {|#1:Progress.Create|});

            internal static Progress Whole => Progress.Create(1, 1);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Progress", "value"),
        Diagnostic().WithLocation(1).WithArguments("Progress", "dones"));

    [Fact]
    public Task CheckedMeter() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Thinktecture;
        using static LanguageExt.Prelude;

        [ComplexValueObject]
        internal sealed partial class Progress {
            internal long Done { get; }

            internal long Total { get; }

            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long done, ref long total) =>
                validationError = total > 0 && done <= total ? null : new ValidationError("progress is out of range");
        }

        internal static class CheckedMeter {
            internal static Option<Progress> Reported(long value, long total) => Progress.Validate(value, total, out Progress? item) is null ? Optional(item) : None;
        }
        """);
}
