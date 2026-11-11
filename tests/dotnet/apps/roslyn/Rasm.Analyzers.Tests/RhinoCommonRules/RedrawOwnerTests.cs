using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RedrawOwnerTests : CSharpCodeFixVerifier<RedrawOwnerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task RedrawOutsideCommits() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino;

        namespace Rasm.Rhino.Document {
            internal static class Commits {
                internal static void Redrawn(RhinoDoc doc) => doc.Views.Redraw();
            }
        }

        internal static class RedrawOutsideCommits {
            internal static void Redrawn(RhinoDoc doc) => {|#0:doc.Views.Redraw()|};
            internal static Action Deferred(RhinoDoc doc) => {|#1:doc.Views.Redraw|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("doc.Views.Redraw()"),
        Diagnostic().WithLocation(1).WithArguments("doc.Views.Redraw"));
}
