using Rasm.TestKit;

namespace Rasm.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void KernelKeepsNumericsGeometryAndContentHashPackagesExplicit() {
        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm/Rasm.csproj");
        app.IncludesNoProjects();
        app.IncludesOnlyPackages(
            "CSparse",
            "ExtendedNumerics.BigRational",
            "MathNet.Numerics",
            "MathNet.Numerics.Providers.MKL",
            "MathNet.Numerics.Providers.OpenBLAS",
            "PeterO.Numbers",
            "System.Numerics.Tensors",
            "TYoshimura.DoubleDouble",
            "GShark",
            "LibTessDotNet",
            "Supercluster.KDTree.Net",
            "CavalierContours",
            "Clipper2",
            "geometry3Sharp",
            "MIConvexHull",
            "SharpVoronoiLib",
            "Triangle",
            "System.IO.Hashing");
        app.PackageReferenceHasAttribute(packageName: "SharpVoronoiLib", attributeName: "Aliases", expectedValue: "Voronoi");
        app.PackageReferenceHasAttribute(packageName: "Triangle", attributeName: "Aliases", expectedValue: "TriangleNet");
    }
}
