using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Names;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class BorrowedNamespaceTests : CSharpCodeFixVerifier<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsANamespaceAReferencedAssemblyDeclaresAndIgnoresANamespaceOnlyTheProjectDeclares() => new CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Blocks.cs", """
            namespace {|#0:LanguageExt|} {
                internal static class Squatted;
            }

            namespace {|#1:System.Text.Json.Serialization|} {
                internal static class Converted;
            }

            namespace LanguageExt.Local {
                internal static class Nested;
            }

            namespace Rasm.Core {
                internal static class Owned;
            }
            """),
            ("/Scoped.cs", """
            namespace {|#2:Thinktecture|};

            internal static class Scoped;
            """),
        }, },
        ExpectedDiagnostics = {
            Diagnostic("RASM3005").WithLocation(0).WithArguments("LanguageExt", "LanguageExt.Core"),
            Diagnostic("RASM3005").WithLocation(1).WithArguments("System.Text.Json.Serialization", "System.Text.Json"),
            Diagnostic("RASM3005").WithLocation(2).WithArguments("Thinktecture", "Thinktecture.Runtime.Extensions"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);
}
