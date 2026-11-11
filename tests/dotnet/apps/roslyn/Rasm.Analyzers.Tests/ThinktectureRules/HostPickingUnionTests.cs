using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.ThinktectureRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class HostPickingUnionTests : CSharpCodeFixVerifier<HostPickingUnionAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task MeshEdit() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;
        using Thinktecture;

        [Union]
        internal abstract partial record {|#0:MeshEdit|} {
            internal sealed record Weld(double Angle, bool Pure) : MeshEdit;

            internal sealed record Heal(double Distance) : MeshEdit;
        }

        internal static class PickedEdits {
            internal static void Applied(Mesh mesh, MeshEdit edit) => edit.Switch(mesh, static (m, c) => m.Weld(c.Angle, c.Pure), static (m, c) => m.HealNakedEdges(c.Distance));
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("MeshEdit"));

    [Fact]
    public Task SpacedEdits() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;
        using Thinktecture;

        [Union]
        internal abstract partial record Spacing {
            internal sealed record Fixed(double Gap) : Spacing;

            internal sealed record Scaled(double Factor) : Spacing;
        }

        internal static class SpacedEdits {
            internal static double Gap(Spacing spacing) => spacing.Switch(static c => Math.Max(c.Gap, 0d), static c => c.Factor * 2d);
        }
        """);
}
