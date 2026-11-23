using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PropertyWriteTests : CSharpCodeFixVerifier<PropertyWriteAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM2011]

    [Fact]
    public Task FalseWrittenToFloatingIsReportedWhileTrueOrAVariableIsSilent() => VerifyAnalyzerAsync(
        """
        using Rhino.Display;

        internal static class Views {
            internal static void Docked(RhinoView view) => {|#0:view.Floating = false|};

            internal static int Nested(RhinoView view) {
                int first, second;
                {|#1:((view.Floating, first), second) = ((false, 1), 2)|};
                return first + second;
            }

            internal static void Floated(RhinoView view) => view.Floating = true;

            internal static void Restored(RhinoView view, bool floating) => view.Floating = floating;

            internal static int Kept(RhinoView view) {
                int count;
                (view.Floating, count) = (true, 1);
                return count;
            }
        }
        """,
        Diagnostic("RASM2011").WithLocation(0).WithArguments("view.Floating = false"),
        Diagnostic("RASM2011").WithLocation(1).WithArguments("((view.Floating, first), second) = ((false, 1), 2)"));

    // --- [RASM2104]

    [Fact]
    public Task ConduitToggledOutsideConduitsIsReportedWhileALambdaInsideConduitsIsSilent() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Display;

        namespace Rasm.Rhino.Display {
            internal static class Conduits {
                internal static Action Enable(DisplayConduit conduit) => () => conduit.Enabled = true;
            }
        }

        internal static class Toggles {
            internal static Action Enable(DisplayConduit conduit) => () => {|#0:conduit.Enabled = true|};

            internal static bool Enabled(DisplayConduit conduit) => conduit.Enabled;
        }
        """,
        Diagnostic("RASM2104").WithLocation(0).WithArguments("conduit.Enabled = true"));

    // --- [RASM2105]

    [Fact]
    public Task RenderSettingsWrittenOutsideRenderSettingsSourcesIsReportedWhileReadsAreSilent() => VerifyAnalyzerAsync(
        """
        using Rhino;
        using Rhino.Render;

        namespace Rasm.Rhino.Render.Settings {
            internal static class RenderSettingsSources {
                internal static void Edit(RhinoDoc doc, RenderSettings staged) => doc.RenderSettings = staged;
            }
        }

        internal static class Renders {
            internal static void Assigned(RhinoDoc doc, RenderSettings staged) => {|#0:doc.RenderSettings = staged|};

            internal static RenderSettings Read(RhinoDoc doc) => doc.RenderSettings;
        }
        """,
        Diagnostic("RASM2105").WithLocation(0).WithArguments("doc.RenderSettings = staged"));
}
