using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Construction;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UninstantiatedClassSuppressionTests {
    [Fact]
    public Task ListedRootDescendantsAreSuppressed() => new CSharpCodeFixTest<HostInstantiatedSuppressor, EmptyCodeFixProvider> {
        TestCode = """
            namespace Host {
                public interface IComponent { }

                public abstract class Command { }
            }

            internal sealed class {|#0:Component|} : Host.IComponent { }

            internal sealed class {|#1:Run|} : Host.Command { }

            internal sealed class {|#2:Unrelated|} { }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            dotnet_diagnostic.CA1812.severity = warning
            RASM3114.instantiated = T:Host.IComponent|T:Host.Command
            """), }, },
        AdditionalAnalyzers = { new CSharpAvoidUninstantiatedInternalClasses() },
        ExpectedDiagnostics = {
            new DiagnosticResult("CA1812", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(true),
            new DiagnosticResult("CA1812", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true),
            new DiagnosticResult("CA1812", DiagnosticSeverity.Warning).WithLocation(2).WithIsSuppressed(false),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task UnlistedRootKeepsFinding() => new CSharpCodeFixTest<HostInstantiatedSuppressor, EmptyCodeFixProvider> {
        TestCode = """
            namespace Host {
                public interface IComponent { }
            }

            internal sealed class {|#0:Component|} : Host.IComponent { }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            dotnet_diagnostic.CA1812.severity = warning
            """), }, },
        AdditionalAnalyzers = { new CSharpAvoidUninstantiatedInternalClasses() },
        ExpectedDiagnostics = { new DiagnosticResult("CA1812", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(false) },
    }.RunAsync(TestContext.Current.CancellationToken);
}
