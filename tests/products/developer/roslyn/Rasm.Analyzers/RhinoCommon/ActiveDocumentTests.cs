using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ActiveDocumentTests : CSharpCodeFixVerifier<ActiveDocumentAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ActiveDocReadBesideAReachableDocumentMemberIsReportedNamingItWhileStaticSelfAndComparedReadsAreSilent() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal sealed class Held {
            private readonly RhinoDoc document;

            internal Held(RhinoDoc document) => this.document = document;

            internal uint Field() => {|#0:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;

            internal static uint Static() => RhinoDoc.ActiveDoc.RuntimeSerialNumber;

            internal static uint Parameter(RhinoDoc given) => {|#1:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;

            internal static bool Active(RhinoDoc given) => given.RuntimeSerialNumber == RhinoDoc.ActiveDoc.RuntimeSerialNumber;
        }

        internal sealed class Current {
            internal RhinoDoc Document => RhinoDoc.ActiveDoc;

            internal uint Serial() => {|#2:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("document"),
        Diagnostic().WithLocation(1).WithArguments("given"),
        Diagnostic().WithLocation(2).WithArguments("Document"));

    [Fact]
    public Task ActiveDocReadAfterALocalDocumentIsReportedWhileAReadBeforeItsDeclarationIsSilent() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal static class Locals {
            internal static uint Prior() {
                RhinoDoc held = RhinoDoc.CreateHeadless(null);
                return held.RuntimeSerialNumber + {|#0:RhinoDoc.ActiveDoc|}.RuntimeSerialNumber;
            }

            internal static uint Later() {
                uint serial = RhinoDoc.ActiveDoc.RuntimeSerialNumber;
                RhinoDoc held = RhinoDoc.ActiveDoc;
                return serial + held.RuntimeSerialNumber;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("held"));
}
