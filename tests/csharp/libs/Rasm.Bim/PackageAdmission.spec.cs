using Rasm.TestKit;

namespace Rasm.Bim.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void BimKeepsSemanticExchangePackagesExplicit() {
        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Bim/Rasm.Bim.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");
        app.IncludesOnlyPackages(
            "Generator.Equals",
            "Riok.Mapperly",
            "ACadSharp",
            "Alimer.Bindings.MeshOptimizer",
            "AssimpNetter",
            "dotbim",
            "geometry3Sharp",
            "GeometryGymIFC_Core",
            "Openize.Drako",
            "Ply.Net",
            "SharpGLTF.Core",
            "SharpGLTF.Ext.3DTiles",
            "SharpGLTF.Runtime",
            "SharpGLTF.Toolkit",
            "StructuralAnalysisFormat",
            "Themis.Las",
            "UniversalSceneDescription",
            "Unofficial.laszip.netstandard",
            "DragonflySchema",
            "HoneybeeSchema",
            "NREL.OpenStudio.macOS-arm64",
            "Xbim.CobieExpress",
            "Xbim.CobieExpress.Exchanger",
            "Xbim.IO.CobieExpress",
            "ids-lib",
            "Smino.Bcf.Toolkit",
            "SwiftCollections.Lean",
            "Xbim.InformationSpecifications",
            "Xbim.Properties",
            "BrickSchema.Net",
            "VividOrange.Cases",
            "VividOrange.Countries",
            "VividOrange.Loads",
            "VividOrange.Stages",
            "bertt.CityJSON",
            "FlatGeobuf",
            "GISBlox.IO.GeoParquet",
            "MaxRev.Gdal.Core",
            "MaxRev.Gdal.MacosRuntime.Minimal.arm64",
            "NetTopologySuite",
            "NetTopologySuite.IO.Esri.Shapefile",
            "NetTopologySuite.IO.GeoJSON4STJ",
            "NetTopologySuite.IO.GeoPackage",
            "NetTopologySuite.IO.VectorTiles",
            "NetTopologySuite.IO.VectorTiles.Mapbox",
            "pocketken.H3",
            "SharpKml.Core",
            "subtree",
            "Speckle.Objects",
            "Speckle.Sdk",
            "NodaMoney",
            "NodaTime",
            "ProjNET",
            "QuikGraph",
            "System.IO.Hashing",
            "Thinktecture.Runtime.Extensions.Json",
            "UnitsNet");
        app.PackageReferenceHasAttribute(packageName: "Generator.Equals", attributeName: "PrivateAssets", expectedValue: "all");
        app.PackageReferenceHasAttribute(packageName: "Riok.Mapperly", attributeName: "PrivateAssets", expectedValue: "all");
        app.PackageReferenceHasAttribute(packageName: "NREL.OpenStudio.macOS-arm64", attributeName: "ExcludeAssets", expectedValue: "build");
        app.PackageReferenceHasAttribute(packageName: "NREL.OpenStudio.macOS-arm64", attributeName: "GeneratePathProperty", expectedValue: "true");
    }
}
