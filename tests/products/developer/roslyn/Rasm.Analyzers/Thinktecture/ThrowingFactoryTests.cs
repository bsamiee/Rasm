using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Thinktecture;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ThrowingFactoryTests : CSharpCodeFixVerifier<ThrowingFactoryAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsCreateOnCallerDataButNotInStaticFieldOrProperty() => VerifyAnalyzerAsync(
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
        Diagnostic().WithLocation(0).WithArguments("Progress.Create", "`value` from the caller", "return `Fin<Progress>` through `Validate`"),
        Diagnostic().WithLocation(1).WithArguments("Progress.Create", "`dones` from the caller", "return `Fin<Progress>` through `Validate`"));

    [Fact]
    public Task ReportsThrowInsideValidationHookButNotAssignedErrorOrConstructorCheck() => VerifyAnalyzerAsync(
        """
        using System;
        using Thinktecture;

        [ValueObject<int>]
        internal sealed partial class Cents {
            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
                validationError = value >= 0 ? null : new ValidationError("cents are negative");
        }

        [ValueObject<string>]
        [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
        internal sealed partial class Code {
            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
                {|#0:ArgumentException.ThrowIfNullOrWhiteSpace(value)|};
                value = value.Length > 8 ? {|#1:throw new ArgumentOutOfRangeException(nameof(value))|} : value.Trim();
            }
        }

        [ComplexValueObject]
        internal sealed partial class Price {
            internal int Amount { get; }

            internal int Tax { get; }

            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int amount, ref int tax) {
                Cents created = {|#2:Cents.Create(amount)|};
                Cents cast = {|#3:(Cents)tax|};
                validationError = created.CompareTo(cast) >= 0 ? null : new ValidationError("tax exceeds amount");
            }
        }

        [ComplexValueObject]
        internal sealed partial class Checked {
            internal int Amount { get; }

            static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int amount) =>
                validationError = Cents.Validate(amount, null, out Cents? _);
        }

        [SmartEnum<string>]
        internal sealed partial class Kind {
            public static readonly Kind Standard = new("standard");

            static partial void ValidateConstructorArguments(ref string key) {
                if (key.Length is 0) throw new ArgumentException("key is blank", nameof(key));
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("ArgumentException.ThrowIfNullOrWhiteSpace", "caller data inside `ValidateFactoryArguments`", "assign `validationError` and return"),
        Diagnostic().WithLocation(1).WithArguments("throw", "caller data inside `ValidateFactoryArguments`", "assign `validationError` and return"),
        Diagnostic().WithLocation(2).WithArguments("Cents.Create", "caller data inside `ValidateFactoryArguments`", "assign `validationError` and return"),
        Diagnostic().WithLocation(3).WithArguments("(Cents)", "caller data inside `ValidateFactoryArguments`", "assign `validationError` and return"));

    [Fact]
    public Task IgnoresValidateOnCallerData() => VerifyAnalyzerAsync(
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
