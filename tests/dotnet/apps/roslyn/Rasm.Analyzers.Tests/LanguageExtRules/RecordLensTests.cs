using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RecordLensTests : CSharpCodeFixVerifier<RecordLensAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task Optics() => VerifyAnalyzerAsync(
        """
        using System.Numerics;
        using LanguageExt;

        internal sealed record Place(double Latitude, double Longitude);

        internal sealed record Site(Place Place, double North);

        internal sealed record Scene(Site Site, double Altitude);

        internal static class Optics {
            internal static readonly Lens<Scene, double> Altitude = {|#0:Lens<Scene, double>.New(static scene => scene.Altitude, static altitude => scene => scene with { Altitude = altitude })|};

            internal static readonly Lens<Scene, double> North = {|#1:Lens<Scene, double>.New(static scene => scene.Site.North, static north => scene => scene with { Site = scene.Site with { North = north } })|};

            internal static readonly Lens<Scene, double> Latitude = {|#2:Lens<Scene, double>.New(
                static scene => scene.Site.Place.Latitude,
                static latitude => scene => scene with { Site = scene.Site with { Place = scene.Site.Place with { Latitude = latitude } } })|};

            internal static readonly Lens<Scene, double> Reordered = {|#3:Lens<Scene, double>.New(Set: static altitude => scene => scene with { Altitude = altitude }, Get: static scene => scene.Altitude)|};

            internal static readonly Lens<Vector3, float> Red = Lens<Vector3, float>.New(static rgb => rgb.X, static x => rgb => rgb with { X = x });

            internal static readonly Lens<Scene, Scene> Whole = Lens<Scene, Scene>.New(static scene => scene, static next => _ => next);

            internal static readonly Lens<Scene, double> Doubled = Lens<Scene, double>.New(static scene => scene.Altitude * 2d, static altitude => scene => scene with { Altitude = altitude / 2d });
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Lens<Scene, double>.New", "property", "Altitude", "Scene", "scene with { Altitude = altitude }"),
        Diagnostic().WithLocation(1).WithArguments("Lens<Scene, double>.New", "path", "Site.North", "Scene", "scene with { Site = scene.Site with { North = north } }"),
        Diagnostic().WithLocation(2).WithArguments("Lens<Scene, double>.New", "path", "Site.Place.Latitude", "Scene", "scene with { Site = scene.Site with { Place = scene.Site.Place with { Latitude = latitude } } }"),
        Diagnostic().WithLocation(3).WithArguments("Lens<Scene, double>.New", "property", "Altitude", "Scene", "scene with { Altitude = altitude }"));
}
