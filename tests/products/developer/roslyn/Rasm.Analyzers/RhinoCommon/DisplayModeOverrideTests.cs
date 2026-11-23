using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DisplayModeOverrideTests : CSharpCodeFixVerifier<DisplayModeOverrideAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReadSetDisplayModeOverrideResultIsReportedWhileADiscardOrStatementIsSilent() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Display;
        using Rhino.DocObjects;

        internal static class Overrides {
            internal static bool Returned(ObjectAttributes staged, DisplayModeDescription description) => {|#0:staged.SetDisplayModeOverride(description)|};

            internal static void Assigned(ObjectAttributes staged, DisplayModeDescription description, Guid viewport, out bool done) => done = {|#1:staged.SetDisplayModeOverride(description, viewport)|};

            internal static void Discarded(ObjectAttributes staged, DisplayModeDescription description, Guid viewport) => _ = staged.SetDisplayModeOverride(description, viewport);

            internal static void Stated(ObjectAttributes staged, DisplayModeDescription description) => staged.SetDisplayModeOverride(description);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("staged.SetDisplayModeOverride(description)"),
        Diagnostic().WithLocation(1).WithArguments("staged.SetDisplayModeOverride(description, viewport)"));
}
