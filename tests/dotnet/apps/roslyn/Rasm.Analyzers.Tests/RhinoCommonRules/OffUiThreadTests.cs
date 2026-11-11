using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OffUiThreadTests : CSharpCodeFixVerifier<OffUiThreadAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task TaskRunDocument() => VerifyAnalyzerAsync(
        """
        using System.Threading.Tasks;
        using Rhino;

        namespace Rasm.Rhino {
            public static class Session {
                public static T OnUiThread<T>(T value) => value;
            }
        }

        internal static class TaskRunDocument {
            internal static Task<int> Counted(RhinoDoc doc) => Task.Run(() => {|#0:doc|}.Objects.Count);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("doc", "Task.Run"));

    [Fact]
    public Task TaskRunValue() => VerifyAnalyzerAsync(
        """
        using System.Threading.Tasks;

        namespace Rasm.Rhino {
            public static class Session {
                public static T OnUiThread<T>(T value) => value;
            }
        }

        internal static class TaskRunValue {
            internal static Task<int> Detached(int count) => Task.Run(() => count + 1);
        }
        """);
}
