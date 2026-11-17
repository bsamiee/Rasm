using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RenderSettingsWriteTests : CSharpCodeFixVerifier<PropertyWriteAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsRenderSettingsWriteOutsideRenderSettingsSources() => VerifyAnalyzerAsync(
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
    public Task IgnoresRenderSettingsRead() => VerifyAnalyzerAsync(
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
