using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Names;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class BorrowedNamespaceTests : CSharpCodeFixVerifier<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsNamespaceOwnedByReferencedAssembly() => VerifyAnalyzerAsync(
        """
        namespace {|#0:Thinktecture|} {
            internal static class Squatted;
        }

        namespace {|#1:System.Text.Json.Serialization|} {
            internal static class Converted;
        }

        namespace Rasm.Core {
            internal static class Owned;
        }
        """,
        Diagnostic("RASM3005").WithLocation(0).WithArguments("Thinktecture", "Thinktecture.Runtime.Extensions"),
        Diagnostic("RASM3005").WithLocation(1).WithArguments("System.Text.Json.Serialization", "System.Text.Json"));

    [Fact]
    public Task ReportsNamespaceOwnedByReferencedPackage() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reflection;
        using LanguageExt.Common;
        using Alias = System.Type;

        internal sealed record Label(string? Text);

        namespace {|#0:LanguageExt|} {
            internal static class SourceFactory {
                internal static Label Fail(string text) => new(text);
            }
        }
        """,
        Diagnostic("RASM3005").WithLocation(0).WithArguments("LanguageExt", "LanguageExt.Core"));
}
