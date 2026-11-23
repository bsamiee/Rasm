using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NarrowedDuplicateTests : CSharpCodeFixVerifier<NarrowedDuplicateAnalyzer, EmptyCodeFixProvider> {

    // --- [SOURCES]

    private const string Products = """
        using System.Runtime.CompilerServices;
        using LanguageExt;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static IO<T> Copied<T>(T source, [CallerArgumentExpression(nameof(source))] string member = "") where T : GeometryBase => IO.pure(source);
            }
        }
        """;

    // --- [NARROWING]

    [Fact]
    public Task DuplicateNarrowedToADerivedTypeIsReportedInEachSpellingWhileAnIdentityOrTypeParameterCastIsSilent() => new CSharpCodeFixTest<NarrowedDuplicateAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            Sources = {
                Products,
                """
                using Rhino.Geometry;

                internal static class Narrowed {
                    internal static Curve Cast(Curve curve) => {|#0:(Curve)curve.Duplicate()|};

                    internal static bool Tested(Curve curve) => {|#1:curve.DuplicateShallow() is Curve|};

                    internal static bool Patterned(Curve curve) => {|#2:curve.Duplicate() is Curve or Surface|};

                    internal static int Switched(Curve curve) => {|#3:curve.Duplicate() switch { Curve => 1, _ => 0 }|};

                    internal static int Stated(Curve curve) {
                        {|#4:switch (curve.Duplicate()) {
                            case Curve:
                                return 1;
                            default:
                                return 0;
                        }|}
                    }

                    internal static GeometryBase Identity(Curve curve) => (GeometryBase)curve.Duplicate();

                    internal static T Generic<T>(T source) where T : GeometryBase => (T)source.Duplicate();
                }
                """,
            },
            ExpectedDiagnostics = {
                Diagnostic().WithLocation(0).WithArguments("Cast", "curve.Duplicate()", "Curve"),
                Diagnostic().WithLocation(1).WithArguments("Tested", "curve.DuplicateShallow()", "Curve"),
                Diagnostic().WithLocation(2).WithArguments("Patterned", "curve.Duplicate()", "Curve"),
                Diagnostic().WithLocation(3).WithArguments("Switched", "curve.Duplicate()", "Curve"),
                Diagnostic().WithLocation(4).WithArguments("Stated", "curve.Duplicate()", "Curve"),
            },
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [RELAYS]

    [Fact]
    public Task DuplicateNarrowedThroughALocalOrParameterIsReportedWhileAReassignedLocalIsSilent() => new CSharpCodeFixTest<NarrowedDuplicateAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            Sources = {
                Products,
                """
                using Rhino.Geometry;

                internal static class Relayed {
                    internal static Curve Held(Curve curve) {
                        GeometryBase copy = curve.Duplicate();
                        return {|#0:(Curve)copy|};
                    }

                    internal static Curve Reassigned(Curve curve, GeometryBase other) {
                        GeometryBase copy = curve.Duplicate();
                        copy = other;
                        return (Curve)copy;
                    }

                    internal static Curve Passed(Curve curve) => Cast({|#1:curve.Duplicate()|});

                    internal static Curve Typed(Curve curve) => Cast({|#3:curve.Duplicate()|}, 0);

                    internal static GeometryBase Kept(Curve curve) => Keep(curve.Duplicate());

                    private static Curve Cast(GeometryBase copy) => {|#2:(Curve)copy|};

                    private static Curve Cast<T>(GeometryBase copy, T unused) => {|#4:(Curve)copy|};

                    private static GeometryBase Keep(GeometryBase copy) => copy;
                }
                """,
            },
            ExpectedDiagnostics = {
                Diagnostic().WithLocation(0).WithArguments("Held", "curve.Duplicate()", "Curve"),
                Diagnostic().WithLocation(1).WithLocation(2).WithArguments("Cast", "curve.Duplicate()", "Curve"),
                Diagnostic().WithLocation(3).WithLocation(4).WithArguments("Cast", "curve.Duplicate()", "Curve"),
            },
        },
    }.RunAsync(TestContext.Current.CancellationToken);
}
