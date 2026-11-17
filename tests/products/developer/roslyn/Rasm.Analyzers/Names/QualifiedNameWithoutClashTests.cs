using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Names;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class QualifiedNameWithoutClashTests : CSharpCodeFixVerifier<QualifiedNameAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsQualifiedNameOnlyWhenItsNamespaceClashesWithNothing() => VerifyAnalyzerAsync(
        """
        using Rhino.Geometry;

        internal sealed record Color(int Value);

        internal static class Palette {
            internal static {|#0:System.Collections.Immutable.ImmutableArray<int>|} Empty => [];

            internal static System.Drawing.Bitmap Blank => new(1, 1);

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);

            internal static Plane Base => Plane.WorldXY;

            internal static System.Numerics.Vector2 Offset => default;
        }
        """,
        Diagnostic("RASM3010").WithLocation(0).WithArguments("System.Collections.Immutable.ImmutableArray<int>", "System.Collections.Immutable", "ImmutableArray<int>"));

    [Fact]
    public Task StaysSilentWhenImportingTheNamespaceWouldClash() => new CSharpCodeFixTest<QualifiedNameAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Imported.cs", """
            using Rhino.Geometry;

            internal static class Imported {
                internal static System.Numerics.Plane Flat => default;
            }
            """),
            ("/Declared.cs", """
            internal sealed record Color(int Value);

            internal static class Declared {
                internal static System.Drawing.Bitmap Blank => new(1, 1);
            }
            """),
            ("/Ambiguous.cs", """
            using System.Timers;

            internal static class Ambiguous {
                internal static Timer Ticker => new();

                internal static System.Threading.Thread Current => System.Threading.Thread.CurrentThread;
            }
            """),
        }, },
    }.RunAsync(TestContext.Current.CancellationToken);
}
