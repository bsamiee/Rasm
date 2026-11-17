using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Switches;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CatchAllArmOverUnionTests : CSharpCodeFixVerifier<CatchAllArmAnalyzer, LanguageCodeFix> {
    [Fact]
    public Task ReportsDiscardArmOverUnion() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class DiscardArm {
            internal static double Area(Shape shape) => shape switch { Shape.Circle circle => circle.Radius, {|#0:_ => 0d|} };
        }
        """,
        Diagnostic("RASM1101").WithLocation(0).WithArguments("Shape"));

    [Fact]
    public Task ReportsDefaultSectionOverUnion() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class DefaultSection {
            internal static double Area(Shape shape) {
                switch (shape) {
                    case Shape.Circle circle:
                        return circle.Radius;
                    {|#0:default:|}
                        return 0d;
                }
            }
        }
        """,
        Diagnostic("RASM1101").WithLocation(0).WithArguments("Shape"));

    [Fact]
    public Task IgnoresCatchAllArmWhenNoArmNamesUnionCase() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [Union]
        internal abstract partial record Shape {
            internal sealed record Circle(double Radius) : Shape;
            internal sealed record Square(double Side) : Shape;
        }

        internal static class NullArm {
            internal static double Plain(object value) => value switch { int count => count, _ => 0d };
            internal static bool Present(Shape? shape) => shape switch { null => false, _ => true };
        }
        """);
}
