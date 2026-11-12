using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DisplayModeOverrideTests : CSharpCodeFixVerifier<DisplayModeOverrideAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task TrustedOverride() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using Rhino.Display;
        using Rhino.DocObjects;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            internal static class Refused {
                internal static Fin<Unit> Unless(bool done, string member) => done ? unit : Error.New(member);
            }
        }

        internal static class TrustedOverride {
            internal static IO<Unit> Viewport(ObjectAttributes staged, DisplayModeDescription description, Guid viewport) =>
                IO.lift(() => Rasm.Rhino.Refused.Unless({|#0:staged.SetDisplayModeOverride(description, viewport)|}, nameof(ObjectAttributes.SetDisplayModeOverride)));

            internal static bool Whole(ObjectAttributes staged, DisplayModeDescription description) => {|#1:staged.SetDisplayModeOverride(description)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("staged.SetDisplayModeOverride(description, viewport)"),
        Diagnostic().WithLocation(1).WithArguments("staged.SetDisplayModeOverride(description)"));

    [Fact]
    public Task DiscardedOverride() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using Rhino.Display;
        using Rhino.DocObjects;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            internal static class Refused {
                internal static Fin<Unit> Unless(bool done, string member) => done ? unit : Error.New(member);
            }
        }

        internal static class DiscardedOverride {
            internal static IO<Unit> Viewport(ObjectAttributes staged, DisplayModeDescription description, Guid viewport) =>
                IO.lift(() => { _ = staged.SetDisplayModeOverride(description, viewport); });

            internal static void Whole(ObjectAttributes staged, DisplayModeDescription description) => staged.SetDisplayModeOverride(description);
        }
        """);
}
