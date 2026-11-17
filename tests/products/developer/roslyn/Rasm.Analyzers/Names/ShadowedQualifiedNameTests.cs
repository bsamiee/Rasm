using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Names;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ShadowedQualifiedNameTests : CSharpCodeFixVerifier<QualifiedNameAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsQualifiedStaticImportShadowedByParameter() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Rhino.Geometry;
        using static LanguageExt.Prelude;

        internal static class Painter {
            internal static int Bitmap(int size) => size;

            internal static System.Drawing.Bitmap Made(int size) => new(size, size);

            internal static Unit Run(int unit) => {|#0:Prelude.unit|};

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);
        }
        """,
        Diagnostic("RASM3009").WithLocation(0).WithArguments("Prelude.unit", "int unit", "LanguageExt.Prelude"));

    [Fact]
    public Task StaysSilentWhenQualificationSeparatesPackageTypesOrNamesNonTypeMember() => VerifyAnalyzerAsync(
        """
        using Rhino.Geometry;

        internal static class Placement {
            internal static int Bitmap(int size) => size;

            internal static System.Drawing.Bitmap Made(int size) => new(size, size);

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);
        }
        """);
}
