using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UiMarshalTests : CSharpCodeFixVerifier<UiMarshalAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task SingleInvoke() => VerifyAnalyzerAsync(
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
    public Task InvokeNamed() => VerifyAnalyzerAsync(
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
