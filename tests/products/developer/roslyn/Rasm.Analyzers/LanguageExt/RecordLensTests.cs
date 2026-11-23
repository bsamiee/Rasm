using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RecordLensTests : CSharpCodeFixVerifier<RecordLensAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsLensWhoseGetterAndSetterAddressOneRecordPathAndIgnoresCrossedPaths() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal sealed record Place(double Latitude, double Longitude);

        internal sealed record Scene(Place Place, double Altitude, double North);

        internal static class Optics {
            internal static readonly Lens<Scene, double> Altitude = {|#0:Lens<Scene, double>.New(static scene => scene.Altitude, static altitude => scene => scene with { Altitude = altitude })|};
            internal static readonly Lens<Scene, double> Latitude = {|#1:Lens<Scene, double>.New(Set: static latitude => scene => scene with { Place = scene.Place with { Latitude = latitude } }, Get: static scene => scene.Place.Latitude)|};
            internal static readonly Lens<Scene, double> Crossed = Lens<Scene, double>.New(static scene => scene.Altitude, static north => scene => scene with { North = north });
            internal static readonly Lens<Scene, double> Swapped = Lens<Scene, double>.New(static scene => scene.Place.Latitude, static longitude => scene => scene with { Place = scene.Place with { Longitude = longitude } });
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Lens<Scene, double>.New", "property", "Altitude", "Scene", "scene with { Altitude = altitude }"),
        Diagnostic().WithLocation(1).WithArguments("Lens<Scene, double>.New", "path", "Place.Latitude", "Scene", "scene with { Place = scene.Place with { Latitude = latitude } }"));

    [Fact]
    public Task ReportsLensOverAPositionalPropertyAndIgnoresComputedGettersComputedSettersAndNonPositionalProperties() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal sealed record Scene(double Altitude) {
            public double Height { get; init; }
        }

        internal static class Optics {
            internal static readonly Lens<Scene, double> Altitude = {|#0:Lens<Scene, double>.New(static scene => scene.Altitude, static altitude => scene => scene with { Altitude = altitude })|};
            internal static readonly Lens<Scene, double> Height = Lens<Scene, double>.New(static scene => scene.Height, static height => scene => scene with { Height = height });
            internal static readonly Lens<Scene, double> Doubled = Lens<Scene, double>.New(static scene => scene.Altitude * 2d, static altitude => scene => scene with { Altitude = altitude });
            internal static readonly Lens<Scene, double> Halved = Lens<Scene, double>.New(static scene => scene.Altitude, static altitude => scene => scene with { Altitude = altitude / 2d });
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Lens<Scene, double>.New", "property", "Altitude", "Scene", "scene with { Altitude = altitude }"));
}
