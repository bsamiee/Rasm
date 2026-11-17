using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UncheckedCountTests : CSharpCodeFixVerifier<UncheckedCountAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsDeleteCountNeverComparedWithInputCount() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using Rhino;

        internal static class DroppedCount {
            internal static int Dropped(RhinoDoc doc, IReadOnlyList<Guid> ids) => {|#0:doc.Objects.Delete(ids, true)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Delete", "ids"));

    [Fact]
    public Task IgnoresDeleteCountComparedWithInputCount() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using Rhino;

        internal static class ComparedCount {
            internal static bool Checked(RhinoDoc doc, IReadOnlyList<Guid> ids) => doc.Objects.Delete(ids, true) == ids.Count;
        }
        """);
}
