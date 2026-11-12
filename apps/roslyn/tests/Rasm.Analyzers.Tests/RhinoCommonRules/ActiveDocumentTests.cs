using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ActiveDocumentTests : CSharpCodeFixVerifier<ActiveDocumentAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ActiveDocWithField() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal sealed class ActiveDocWithField(RhinoDoc doc) {
            internal RhinoDoc Document { get; } = doc;

            internal uint Serial() => {|#0:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Document"));

    [Fact]
    public Task ActiveDocWithParameter() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal static class ActiveDocWithParameter {
            internal static uint Serial(RhinoDoc doc) => {|#0:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("doc"));

    [Fact]
    public Task ActiveDocCompared() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal static class ActiveDocCompared {
            internal static bool Active(RhinoDoc doc) => doc == RhinoDoc.ActiveDoc;
            internal static uint Current() => RhinoDoc.ActiveDoc.RuntimeSerialNumber;
        }
        """);
}
