using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UiMarshalTests : CSharpCodeFixVerifier<UiMarshalAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task MarshalAndRedrawOutsideSessionAndCommitsAreReportedWhileTheirOwnersReadsUndelegatedCallsAndNameofAreSilent() => new CSharpCodeFixTest<UiMarshalAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System;
            using Eto.Forms;
            using Rhino;

            namespace Rasm.Rhino {
                internal static class Session {
                    internal static void Marshaled() => Application.Instance.Invoke(static () => { });
                }
            }

            namespace Rasm.Rhino.Document {
                internal static class Commits {
                    internal static void Redrawn(RhinoDoc doc) {
                        doc.Views.RedrawEnabled = false;
                        doc.Views.Redraw();
                    }
                }
            }

            internal static class Worker {
                internal static void Marshaled() => {|#0:Application.Instance.Invoke(static () => { })|};

                internal static Action<Action> Deferred() => {|#1:Application.Instance.AsyncInvoke|};

                internal static string Named() => nameof(Application.Instance.Invoke);

                internal static void Quit() => Application.Instance.Quit();

                internal static void Redrawn(RhinoDoc doc) => {|#2:doc.Views.Redraw()|};

                internal static void Suspended(RhinoDoc doc) => {|#3:doc.Views.RedrawEnabled|} = false;

                internal static bool Enabled(RhinoDoc doc) => doc.Views.RedrawEnabled;
            }
            """,
        AdditionalAnalyzers = { new RedrawOwnerAnalyzer() },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("Application.Invoke"),
            Diagnostic().WithLocation(1).WithArguments("Application.AsyncInvoke"),
            new DiagnosticResult("RASM2102", DiagnosticSeverity.Error).WithLocation(2).WithArguments("doc.Views.Redraw()"),
            new DiagnosticResult("RASM2102", DiagnosticSeverity.Error).WithLocation(3).WithArguments("doc.Views.RedrawEnabled"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task MarshalInsideTheTypeDeclaringTheEntryPointIsSilentWhileAnotherTypeIsReported() => new CSharpCodeFixTest<UiMarshalAnalyzer, EmptyCodeFixProvider> {
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
}
