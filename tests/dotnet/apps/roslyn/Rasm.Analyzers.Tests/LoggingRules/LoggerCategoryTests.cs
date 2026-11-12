using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LoggingRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class LoggerCategoryTests : CSharpCodeFixVerifier<LoggerCategoryAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "ILoggerFactory.CreateLogger(string) is reported through the interface and an implementing type; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldReportError")]
    public Task StringCategory() => VerifyAnalyzerAsync(
        """
        using Microsoft.Extensions.Logging;
        using Microsoft.Extensions.Logging.Abstractions;

        internal static class StringCategory {
            internal static ILogger Literal(ILoggerFactory factory) => factory.{|#0:CreateLogger("StringCategory")|};

            internal static ILogger Named(ILoggerFactory factory) => factory.{|#1:CreateLogger(nameof(StringCategory))|};

            internal static ILogger Rendered(ILoggerFactory factory) => factory.{|#2:CreateLogger(typeof(StringCategory).ToString())|};

            internal static ILogger Concrete(NullLoggerFactory factory) => factory.{|#3:CreateLogger("StringCategory")|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("""factory.CreateLogger("StringCategory")"""),
        Diagnostic().WithLocation(1).WithArguments("factory.CreateLogger(nameof(StringCategory))"),
        Diagnostic().WithLocation(2).WithArguments("factory.CreateLogger(typeof(StringCategory).ToString())"),
        Diagnostic().WithLocation(3).WithArguments("""factory.CreateLogger("StringCategory")"""));

    [Fact(DisplayName = "CreateLogger<T>(), CreateLogger(Type), and ILoggerProvider.CreateLogger are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldNotReportError")]
    public Task TypedCategory() => VerifyAnalyzerAsync(
        """
        using System;
        using Microsoft.Extensions.Logging;

        internal sealed class TypedCategory {
            internal static ILogger Generic(ILoggerFactory factory) => factory.CreateLogger<TypedCategory>();

            internal static ILogger Typed(ILoggerFactory factory, Type owner) => factory.CreateLogger(owner);

            internal static ILogger Provided(ILoggerProvider provider, string category) => provider.CreateLogger(category);

            internal static string Named => nameof(ILoggerFactory.CreateLogger);
        }
        """);
}
