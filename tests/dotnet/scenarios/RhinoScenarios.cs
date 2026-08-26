using System.Text.Json.Nodes;
using Rasm.Bridge.Contract;
using Rasm.ScenarioKit;
using Rhino.DocObjects;

namespace Rasm.Scenarios;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class RhinoScenarios {
    private const double BoxWidth = 3.0;
    private const double BoxDepth = 4.0;
    private const double BoxHeight = 5.0;
    private const double SphereRadius = 2.0;
    private const double Relative = 1.0e-6;

    [RhinoScenario("rhino", Requires = ["rhino.document"], BudgetMs = 20_000)]
    public static Fin<Unit> ClosedSolids(ScenarioContext ctx) {
        ArgumentNullException.ThrowIfNull(ctx);
        return ctx.WithRhinoDocument(scope =>
            from box in Closed("box", Brep.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(BoxWidth, BoxDepth, BoxHeight))))
            from sphere in Closed("sphere", new Sphere(new Point3d(10.0, 0.0, SphereRadius), SphereRadius).ToBrep())
            from boxId in Added(scope, box)
            from sphereId in Added(scope, sphere)
            from boxVolume in ctx.Case(EvidenceName.Create("box.volume"), () => Measured(ctx, "box.volume", Volume(box), BoxWidth * BoxDepth * BoxHeight))
            from boxArea in ctx.Case(EvidenceName.Create("box.area"), () => Measured(ctx, "box.area", Area(box), 2.0 * ((BoxWidth * BoxDepth) + (BoxDepth * BoxHeight) + (BoxHeight * BoxWidth))))
            from sphereVolume in ctx.Case(EvidenceName.Create("sphere.volume"), () => Measured(ctx, "sphere.volume", Volume(sphere), 4.0 / 3.0 * Math.PI * Math.Pow(SphereRadius, 3.0)))
            from sphereArea in ctx.Case(EvidenceName.Create("sphere.area"), () => Measured(ctx, "sphere.area", Area(sphere), 4.0 * Math.PI * SphereRadius * SphereRadius))
            from extent in ctx.Case(EvidenceName.Create("document.extent"), () => Extent(scope, boxId, sphereId))
            from manifest in Manifest(ctx, scope, boxId, sphereId)
            from capture in ctx.CaptureSnapshot(EvidenceName.Create("solids"))
            select unit);
    }

    private static Fin<Brep> Closed(string what, Brep? brep) =>
        brep is { IsSolid: true } solid ? solid : Error.New($"{what} did not build as a closed solid");

    private static Fin<Guid> Added(RhinoDocumentScope scope, Brep brep) {
        Guid id = scope.Doc.Objects.AddBrep(brep);
        return id == Guid.Empty ? Error.New("RhinoDoc.Objects.AddBrep refused the solid") : id;
    }

    private static Fin<double> Volume(Brep brep) =>
        VolumeMassProperties.Compute(brep) is { } mass ? mass.Volume : Error.New("VolumeMassProperties.Compute returned null");

    private static Fin<double> Area(Brep brep) =>
        AreaMassProperties.Compute(brep) is { } mass ? mass.Area : Error.New("AreaMassProperties.Compute returned null");

    private static Fin<Unit> Measured(ScenarioContext ctx, string label, Fin<double> observed, double expected) =>
        ctx.Expect(EvidenceName.Create(label), observed).Bind(Fin<Unit> (value) => {
            ctx.Fact(EvidenceName.Create(label + ".expected"), expected);
            return Math.Abs(value - expected) <= Relative * expected
                ? unit
                : Error.New(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{label}: {value:R} deviates from {expected:R} beyond {Relative:R} relative"));
        });

    private static Fin<Unit> Extent(RhinoDocumentScope scope, Guid boxId, Guid sphereId) {
        BoundingBox union = BoundingBox.Empty;
        foreach (Guid id in (Guid[])[boxId, sphereId]) {
            RhinoObject? item = scope.Doc.Objects.FindId(id);
            if (item is null) {
                return Error.New($"object {id} vanished from the document");
            }
            union.Union(item.Geometry.GetBoundingBox(accurate: true));
        }
        BoundingBox expected = new(new Point3d(0.0, -SphereRadius, 0.0), new Point3d(10.0 + SphereRadius, BoxDepth, BoxHeight));
        return union.Min.DistanceTo(expected.Min) <= 1.0e-6 && union.Max.DistanceTo(expected.Max) <= 1.0e-6
            ? unit
            : Error.New($"document extent {union} differs from analytic {expected}");
    }

    private static Fin<Unit> Manifest(ScenarioContext ctx, RhinoDocumentScope scope, Guid boxId, Guid sphereId) {
        JsonArray objects = [];
        foreach (Guid id in (Guid[])[boxId, sphereId]) {
            RhinoObject? item = scope.Doc.Objects.FindId(id);
            objects.Add(new JsonObject {
                ["id"] = id.ToString("D"),
                ["type"] = item?.ObjectType.ToString(),
                ["layer"] = item is null ? null : scope.Doc.Layers[item.Attributes.LayerIndex].Name,
            });
        }
        ctx.Manifest(EvidenceRole.ObjectManifest, EvidenceName.Create("solids"), new JsonObject { ["count"] = objects.Count, ["objects"] = objects });
        return ctx.Require(EvidenceName.Create("document.objects.added"), scope.Doc.Objects.Count >= 2);
    }
}
