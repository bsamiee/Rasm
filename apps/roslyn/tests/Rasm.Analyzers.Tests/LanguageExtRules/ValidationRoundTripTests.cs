using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ValidationRoundTripTests : CSharpCodeFixVerifier<ValidationRoundTripAnalyzer, LanguageExtCodeFix> {

    // --- [RASM1004]

    [Fact]
    public Task CustomConversions() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class CustomConversions {
            internal static Fin<int> Preserve(Fin<int> value) => ToFin(ToValidation(value));

            internal static Validation<Error, int> ToValidation(Fin<int> value) => value.Map(static held => held + 1).ToValidation();

            internal static Fin<int> ToFin(Validation<Error, int> value) => value.ToFin().Map(static held => held * 2);
        }
        """);

    [Fact]
    public Task ValidationRoundTrip() => VerifyCodeFixAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using LanguageExt.Traits;

        internal static class ValidationRoundTrip {
            internal static Fin<int> Positive(int value) => value > 0 ? value : Error.New("negative");
            internal static Fin<int> Sum(int left, int right) =>
                (Positive(left).ToValidation(), Positive(right).ToValidation())
                    .Apply(static (first, second) => first + second)
                    .As()
                    .{|#0:ToFin()|};
            internal static Fin<Seq<int>> All(Seq<int> values) => values.Traverse(static value => Positive(value).ToValidation()).As().{|#1:ToFin()|};
            internal static Fin<int> Nested(Seq<int> values, int last) =>
                (values.Traverse(static value => Positive(value).ToValidation()).As(), Positive(last).ToValidation()).Apply(static (all, one) => all.Count + one).As().{|#2:ToFin()|};
            internal static Fin<int> Single(int value) => Positive(value).ToValidation().{|#3:ToFin()|};
            internal static Fin<int> Grouped(bool held, int value) => (held ? Positive(value) : Positive(-value)).ToValidation().{|#4:ToFin()|};
            internal static Fin<Seq<int>> Queried(Seq<int> values) =>
                values.Traverse(value =>
                    (from first in Positive(value)
                     from second in Positive(first)
                     select second).ToValidation()).As().{|#5:ToFin()|};
            internal static Fin<int> Paired(bool held, int left, int right) =>
                ((held ? Positive(left) : Positive(right)).ToValidation(), Positive(right).ToValidation()).Apply(static (first, second) => first + second).As().{|#6:ToFin()|};

            internal static Fin<int> Repeated(int value) => Positive(value).ToValidation().{|#7:ToFin()|}.ToValidation().{|#8:ToFin()|};

            internal static Fin<int> Static(int value) => ValidationExtensions.{|#9:ToFin(Positive(value).ToValidation())|};

            internal static Fin<int> Commented(Fin<int> value) => value.ToValidation(/* conversion */).{|#10:ToFin()|};
        }

        internal static class CustomComposition {
            internal static Fin<int> Paired(Fin<int> left, Fin<int> right) =>
                (left.ToValidation(), right.ToValidation()).Pretend(static (first, second) => first + second).As().ToFin();

            internal static Fin<Seq<int>> Traversed(Seq<int> values) =>
                values.Pretend(static value => ValidationRoundTrip.Positive(value).ToValidation()).As().ToFin();

            internal static K<Validation<Error>, int> Pretend(this (Validation<Error, int>, Validation<Error, int>) values, Func<int, int, int> apply) =>
                Validation.Success<Error, int>(apply(0, 0));

            internal static K<Validation<Error>, Seq<int>> Pretend(this Seq<int> values, Func<int, K<Validation<Error>, int>> apply) =>
                Validation.Success<Error, Seq<int>>(values);
        }
        """,
        [
            Diagnostic().WithLocation(0).WithArguments("Apply", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(1).WithArguments("Traverse", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(2).WithArguments("Apply", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(3).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(4).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(5).WithArguments("Traverse", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(6).WithArguments("Apply", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(7).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(8).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(9).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
            Diagnostic().WithLocation(10).WithArguments("ToValidation", "ToValidation", "ToFin").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using LanguageExt.Traits;

        internal static class ValidationRoundTrip {
            internal static Fin<int> Positive(int value) => value > 0 ? value : Error.New("negative");
            internal static Fin<int> Sum(int left, int right) =>
                (Positive(left), Positive(right))
                    .Apply(static (first, second) => first + second)
                    .As();
            internal static Fin<Seq<int>> All(Seq<int> values) => values.Traverse(static value => Positive(value)).As();
            internal static Fin<int> Nested(Seq<int> values, int last) =>
                (values.Traverse(static value => Positive(value)).As(), Positive(last)).Apply(static (all, one) => all.Count + one).As();
            internal static Fin<int> Single(int value) => Positive(value);
            internal static Fin<int> Grouped(bool held, int value) => held ? Positive(value) : Positive(-value);
            internal static Fin<Seq<int>> Queried(Seq<int> values) =>
                values.Traverse(value =>
                    from first in Positive(value)
                    from second in Positive(first)
                    select second).As();
            internal static Fin<int> Paired(bool held, int left, int right) =>
                (held ? Positive(left) : Positive(right), Positive(right)).Apply(static (first, second) => first + second).As();

            internal static Fin<int> Repeated(int value) => Positive(value);

            internal static Fin<int> Static(int value) => Positive(value);

            internal static Fin<int> Commented(Fin<int> value) => value /* conversion */;
        }

        internal static class CustomComposition {
            internal static Fin<int> Paired(Fin<int> left, Fin<int> right) =>
                (left.ToValidation(), right.ToValidation()).Pretend(static (first, second) => first + second).As().ToFin();

            internal static Fin<Seq<int>> Traversed(Seq<int> values) =>
                values.Pretend(static value => ValidationRoundTrip.Positive(value).ToValidation()).As().ToFin();

            internal static K<Validation<Error>, int> Pretend(this (Validation<Error, int>, Validation<Error, int>) values, Func<int, int, int> apply) =>
                Validation.Success<Error, int>(apply(0, 0));

            internal static K<Validation<Error>, Seq<int>> Pretend(this Seq<int> values, Func<int, K<Validation<Error>, int>> apply) =>
                Validation.Success<Error, Seq<int>>(values);
        }
        """);

    [Fact]
    public Task ValidationKept() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class ValidationKept {
            internal static Fin<int> Positive(int value) => value > 0 ? value : Error.New("negative");
            internal static Validation<Error, int> Kept(int left, int right) => (Positive(left).ToValidation(), Positive(right).ToValidation()).Apply(static (first, second) => first + second).As();
            internal static Fin<int> Mixed(Validation<Error, int> held, int right) => (held, Positive(right).ToValidation()).Apply(static (first, second) => first + second).As().ToFin();
        }
        """);

    // --- [LOCATIONS]

    [Fact]
    public Task RelatedLocations() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class Sample {
            internal static Fin<int> Round(Fin<int> value) => {|#2:{|#1:value.ToValidation()|}.{|#0:ToFin()|}|};
        }
        """,
        Diagnostic().WithLocation(0).WithLocation(1).WithLocation(2).WithArguments("ToValidation", "ToValidation", "ToFin"));

    // --- [TOP_LEVEL]

    [Fact]
    public Task TopLevelConversion() => new CSharpCodeFixTest<ValidationRoundTripAnalyzer, LanguageExtCodeFix> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            LanguageExt.ValidationExtensions.{|RASM1004:ToFin(LanguageExt.Fin.Succ(1).ToValidation())|};
            """,
        FixedCode = """
            LanguageExt.Fin.Succ(1);
            """,
    }.RunAsync(TestContext.Current.CancellationToken);
}
