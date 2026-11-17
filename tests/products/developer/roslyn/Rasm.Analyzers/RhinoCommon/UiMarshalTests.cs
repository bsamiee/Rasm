using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UiMarshalTests : CSharpCodeFixVerifier<UiMarshalAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsSingleUiThreadCallOutsideSession() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;

        namespace Rasm.Rhino {
            internal static class Session {
                internal static void Marshaled() => Application.Instance.Invoke(static () => { });
            }
        }

        internal static class SingleInvoke {
            internal static void Marshaled() => {|#0:Application.Instance.Invoke(static () => { })|};
            internal static Action<Action> Deferred() => {|#1:Application.Instance.AsyncInvoke|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Application.Invoke"),
        Diagnostic().WithLocation(1).WithArguments("Application.AsyncInvoke"));

    [Fact]
    public Task ReportsUiThreadCallOutsideTypeDeclaringEntryPoint() => new CSharpCodeFixTest<UiMarshalAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            using Eto.Forms;

            internal static class Program {
                private static void Main() => Application.Instance.Invoke(static () => { });
            }

            internal static class Elsewhere {
                internal static void Marshaled() => {|#0:Application.Instance.Invoke(static () => { })|};
            }
            """,
        ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("Application.Invoke") },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task IgnoresInvokeInsideNameof() => VerifyAnalyzerAsync(
        """
        using Eto.Forms;

        namespace Rasm.Rhino {
            internal static class Session {
                internal static void Marshaled() => Application.Instance.Invoke(static () => { });
            }
        }

        internal static class InvokeNamed {
            internal static string Named() => nameof(Application.Instance.Invoke);
        }
        """);
}
