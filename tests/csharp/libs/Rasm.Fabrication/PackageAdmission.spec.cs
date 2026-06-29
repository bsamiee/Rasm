using Rasm.TestKit;

namespace Rasm.Fabrication.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void FabricationKeepsPortableGeometryAndToolingPackagesExplicit() {
        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");
        app.IncludesOnlyPackages(
            "ACadSharp",
            "CavalierContours",
            "Clipper2",
            "DSTV.Net",
            "geometry3Sharp",
            "MTConnect.NET-Common",
            "OcctNet.Wrapper",
            "PicoGK",
            "RectpackSharp",
            "Robots",
            "SharpVoronoiLib",
            "System.IO.Hashing",
            "UnitsNet");
        app.PackageReferenceHasAttribute(packageName: "SharpVoronoiLib", attributeName: "Aliases", expectedValue: "Voronoi");
    }
}
