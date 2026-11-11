using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NarrowedDuplicateTests : CSharpCodeFixVerifier<NarrowedDuplicateAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM2103]

    [Fact]
    public Task CastDuplicate() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static T Copied<T>(Func<T> call) where T : class => call();
            }
        }

        internal static class CastDuplicate {
            internal static Curve Copied(Curve curve) => {|#0:(Curve)curve.Duplicate()|};
            internal static bool Tested(Curve curve) => {|#1:curve.Duplicate() is Curve|};

            internal static int Switched(Curve curve) {
                {|#2:switch (curve.Duplicate()) {
                    case Curve:
                        return 1;
                    default:
                        return 0;
                }|}
            }

            internal static bool Combined(Curve curve) => {|#3:curve.Duplicate() is Curve or Surface|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Copied", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(1).WithArguments("Tested", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(2).WithArguments("Switched", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(3).WithArguments("Combined", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task RelayedDuplicate() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static T Copied<T>(Func<T> call) where T : class => call();
            }
        }

        internal static class RelayedDuplicate {
            internal static Curve Copied(Curve curve) => Narrowed({|#0:curve.Duplicate()|});
            internal static int Dispatched(Curve curve) => Kind({|#1:curve.Duplicate()|});
            internal static bool Tested(Curve curve) => Typed({|#2:curve.DuplicateShallow()|});
            internal static Curve Generic(Curve curve) => Narrowed({|#3:curve.Duplicate()|}, 0);
            internal static Curve Plain(GeometryBase geometry) => Narrowed(geometry);
            private static Curve Narrowed(GeometryBase copy) => (Curve)copy;
            private static int Kind(GeometryBase copy) => copy switch { Curve => 1, _ => 0 };
            private static bool Typed(GeometryBase copy) => copy is Curve;
            private static Curve Narrowed<T>(GeometryBase copy, T unused) => (Curve)copy;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowed", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(1).WithArguments("Kind", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(2).WithArguments("Typed", "curve.DuplicateShallow()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic().WithLocation(3).WithArguments("Narrowed", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task HeldDuplicate() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static T Copied<T>(Func<T> call) where T : class => call();
            }
        }

        internal static class HeldDuplicate {
            internal static Curve Held(Curve curve) {
                GeometryBase copy = curve.Duplicate();
                return {|#0:(Curve)copy|};
            }

            internal static Curve Reassigned(Curve curve, GeometryBase other) {
                GeometryBase copy = curve.Duplicate();
                copy = other;
                return (Curve)copy;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Held", "curve.Duplicate()", "Curve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task UncastDuplicate() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static T Copied<T>(Func<T> call) where T : class => call();
            }
        }

        internal static class UncastDuplicate {
            internal static GeometryBase Kept(Curve curve) => curve.Duplicate();
            internal static GeometryBase Passed(Curve curve) => Held(curve.Duplicate());
            internal static Curve Unrelayed(GeometryBase geometry) => (Curve)geometry;
            internal static T Generic<T>(T source) where T : GeometryBase => (T)source.Duplicate();
            private static GeometryBase Held(GeometryBase copy) => copy;
        }
        """);

    // --- [LOCATIONS]

    [Fact]
    public Task RelatedLocations() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.Geometry;

        namespace Rasm.Rhino {
            public static class Products {
                public static T Copied<T>(Func<T> call) where T : class => call();
            }
        }

        internal static class Sample {
            internal static Curve Narrow(Curve curve) => Cast({|#0:curve.Duplicate()|});

            internal static Curve Cast(GeometryBase copy) => {|#1:(Curve)copy|};
        }
        """,
        Diagnostic().WithLocation(0).WithLocation(1).WithArguments("Cast", "curve.Duplicate()", "Curve"));
}
