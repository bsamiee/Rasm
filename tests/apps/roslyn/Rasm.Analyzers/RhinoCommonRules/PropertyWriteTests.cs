using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PropertyWriteTests : CSharpCodeFixVerifier<PropertyWriteAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM2011]

    [Fact]
    public Task DockedView() => VerifyAnalyzerAsync(
        """
        using Rhino.Display;

        internal static class DockedView {
            internal static void Docked(RhinoView view) => {|#0:view.Floating = false|};

            internal static void Statement(RhinoView view) {
                {|#1:view.Floating = false|};
            }

            internal static int Nested(RhinoView view) {
                int first, second;
                {|#2:((view.Floating, first), second) = ((false, 1), 2)|};
                return first + second;
            }

            internal static void Both(RhinoView first, RhinoView second) => {|#3:(first.Floating, second.Floating) = (false, false)|};
        }
        """,
        Diagnostic("RASM2011").WithLocation(0).WithArguments("view.Floating = false"),
        Diagnostic("RASM2011").WithLocation(1).WithArguments("view.Floating = false"),
        Diagnostic("RASM2011").WithLocation(2).WithArguments("((view.Floating, first), second) = ((false, 1), 2)"),
        Diagnostic("RASM2011").WithLocation(3).WithArguments("(first.Floating, second.Floating) = (false, false)"));

    [Fact]
    public Task FloatedView() => VerifyAnalyzerAsync(
        """
        using Rhino.Display;

        internal static class FloatedView {
            internal static void Floated(RhinoView view) => view.Floating = true;

            internal static void Restored(RhinoView view, bool floating) => view.Floating = floating;

            internal static bool Read(RhinoView view) => view.Floating;

            internal static int Nested(RhinoView view) {
                int first, second;
                ((view.Floating, first), second) = ((true, 1), 2);
                return first + second;
            }

            internal static int Assigned(RhinoView view, bool floating) {
                int count;
                (view.Floating, count) = (floating, 1);
                return count;
            }
        }
        """);

    // --- [RASM2104]

    [Fact]
    public Task ToggledConduit() => VerifyAnalyzerAsync(
        """
        using Rhino.Display;

        namespace Rasm.Rhino.Display {
            internal static class Conduits {
                internal static void Enable(DisplayConduit conduit) => conduit.Enabled = true;

                internal static void Disable(DisplayConduit conduit) => conduit.Enabled = false;
            }
        }

        internal static class ToggledConduit {
            internal static void Enable(DisplayConduit conduit) => {|#0:conduit.Enabled = true|};

            internal static void Disable(DisplayConduit conduit) {
                {|#1:conduit.Enabled = false|};
            }

            internal static int Deconstructed(DisplayConduit conduit) {
                int count;
                {|#2:(conduit.Enabled, count) = (true, 1)|};
                return count;
            }
        }
        """,
        Diagnostic("RASM2104").WithLocation(0).WithArguments("conduit.Enabled = true"),
        Diagnostic("RASM2104").WithLocation(1).WithArguments("conduit.Enabled = false"),
        Diagnostic("RASM2104").WithLocation(2).WithArguments("(conduit.Enabled, count) = (true, 1)"));

    [Fact]
    public Task ReadConduit() => VerifyAnalyzerAsync(
        """
        using Rhino.Display;

        namespace Rasm.Rhino.Display {
            internal static class Conduits {
                internal static void Enable(DisplayConduit conduit) => conduit.Enabled = true;

                internal static void Disable(DisplayConduit conduit) => conduit.Enabled = false;
            }
        }

        internal static class ReadConduit {
            internal static bool Enabled(DisplayConduit conduit) => conduit.Enabled;
        }
        """);

    // --- [RASM2105]

    [Fact]
    public Task WrittenSettings() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Rhino;
        using Rhino.Render;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino.Render.Settings {
            internal static class RenderSettingsSources {
                internal static IO<Unit> Committed(RhinoDoc doc, RenderSettings staged) => IO.lift(() => { doc.RenderSettings = staged; });
            }
        }

        internal static class WrittenSettings {
            internal static IO<Unit> Committed(RhinoDoc doc, RenderSettings staged) =>
                from committed in IO.lift(() => { {|#0:doc.RenderSettings = staged|}; })
                select unit;

            internal static void Assigned(RhinoDoc doc, RenderSettings staged) => {|#1:doc.RenderSettings = staged|};

            internal static int Deconstructed(RhinoDoc doc, RenderSettings staged) {
                int count;
                {|#2:(doc.RenderSettings, count) = (staged, 1)|};
                return count;
            }
        }
        """,
        Diagnostic("RASM2105").WithLocation(0).WithArguments("doc.RenderSettings = staged"),
        Diagnostic("RASM2105").WithLocation(1).WithArguments("doc.RenderSettings = staged"),
        Diagnostic("RASM2105").WithLocation(2).WithArguments("(doc.RenderSettings, count) = (staged, 1)"));

    [Fact]
    public Task ReadSettings() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Rhino;
        using Rhino.Render;

        namespace Rasm.Rhino.Render.Settings {
            internal static class RenderSettingsSources {
                internal static IO<Unit> Committed(RhinoDoc doc, RenderSettings staged) => IO.lift(() => { doc.RenderSettings = staged; });
            }
        }

        internal static class ReadSettings {
            internal static RenderSettings Read(RhinoDoc doc) => doc.RenderSettings;
        }
        """);
}
