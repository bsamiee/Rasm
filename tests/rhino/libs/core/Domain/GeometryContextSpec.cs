using System.Xml.Linq;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using NUnit.Framework;
using Rhino;
using Rhino.Geometry;
using Rhino.Testing.Fixtures;
using static LanguageExt.Prelude;

namespace Runtime.Rhino.Tests.Libs.Core.Domain;

// --- [RUNTIME SPECS] ---------------------------------------------------------------------------

[TestFixture]
[RhinoTestFixture]
public sealed class GeometryContextSpec {
    [OneTimeSetUp]
    public void Smoke() {
        string settingsPath = Path.Combine(
            path1: TestContext.CurrentContext.TestDirectory,
            path2: "Rhino.Testing.Configs.xml");
        string? configuredPath = XDocument.Load(uri: settingsPath)
            .Root?
            .Element(name: "RhinoSystemDirectory")?
            .Value;

        Assert.Multiple(static () => {
            Assert.That(actual: RhinoDoc.ActiveDoc, expression: Is.Not.Null);
            Assert.That(actual: RhinoApp.Version.Major, expression: Is.EqualTo(expected: 9));
        });
        Assert.That(
            actual: configuredPath,
            expression: Is.EqualTo(expected: "/Applications/RhinoWIP.app/Contents/Frameworks"));
    }

    [TestCase(UnitSystem.Millimeters)]
    [TestCase(UnitSystem.Meters)]
    [TestCase(UnitSystem.Feet)]
    public void CreatesDefault(UnitSystem units) {
        GeometryContext context = GeometryContext.CreateDefault(units: units)
            .ToFin()
            .Match(
                Succ: static (GeometryContext candidate) => candidate,
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));

        Assert.That(actual: context.Units, expression: Is.EqualTo(expected: units));
    }

    [TestCase(UnitSystem.Millimeters)]
    [TestCase(UnitSystem.Meters)]
    [TestCase(UnitSystem.Feet)]
    public void CreatesKnownUnits(UnitSystem units) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: RhinoMath.ToRadians(1.0),
                    units: units)
                .ToFin()
                .IsSucc,
            expression: Is.True);

    [TestCase(UnitSystem.Unset)]
    [TestCase(UnitSystem.CustomUnits)]
    public void RejectsKnownUnitsWithoutScale(UnitSystem units) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: RhinoMath.ToRadians(1.0),
                    units: units)
                .ToFin()
                .IsFail,
            expression: Is.True);

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(0.0)]
    [TestCase(-1.0)]
    public void RejectsInvalidAbsoluteTolerance(double absoluteTolerance) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: absoluteTolerance,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: RhinoMath.ToRadians(1.0),
                    units: UnitSystem.Millimeters)
                .ToFin()
                .IsFail,
            expression: Is.True);

    [TestCase(0.0)]
    [TestCase(0.5)]
    [TestCase(0.999)]
    public void AcceptsRelativeToleranceRange(double relativeTolerance) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: relativeTolerance,
                    angleToleranceRadians: RhinoMath.ToRadians(1.0),
                    units: UnitSystem.Millimeters)
                .ToFin()
                .IsSucc,
            expression: Is.True);

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(-0.01)]
    [TestCase(1.0)]
    public void RejectsInvalidRelativeTolerance(double relativeTolerance) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: relativeTolerance,
                    angleToleranceRadians: RhinoMath.ToRadians(1.0),
                    units: UnitSystem.Millimeters)
                .ToFin()
                .IsFail,
            expression: Is.True);

    [TestCase(0.001)]
    [TestCase(Math.PI)]
    [TestCase(Math.Tau)]
    public void AcceptsAngleToleranceRange(double angleToleranceRadians) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: angleToleranceRadians,
                    units: UnitSystem.Millimeters)
                .ToFin()
                .IsSucc,
            expression: Is.True);

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(0.0)]
    [TestCase(-0.001)]
    [TestCase(7.0)]
    public void RejectsInvalidAngleTolerance(double angleToleranceRadians) =>
        Assert.That(
            actual: GeometryContext.FromKnownUnits(
                    absoluteTolerance: 0.01,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: angleToleranceRadians,
                    units: UnitSystem.Millimeters)
                .ToFin()
                .IsFail,
            expression: Is.True);

    [Test]
    public void ReadsDocument() {
        RhinoDoc doc = Optional(RhinoDoc.ActiveDoc)
            .ToFin(Error.New(message: "Rhino.Testing did not create an active Rhino document."))
            .Match(
                Succ: static (RhinoDoc candidate) => candidate,
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));

        GeometryContext context = GeometryContext.FromDocument(doc: doc)
            .ToFin()
            .Match(
                Succ: static (GeometryContext candidate) => candidate,
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));

        Assert.That(actual: context.Units, expression: Is.EqualTo(expected: doc.ModelUnitSystem));
    }

    [Test]
    public void ValidatesClosedCurve() {
        GeometryContext context = Context();
        using Curve curve = new Circle(plane: Plane.WorldXY, radius: 1.0).ToNurbsCurve();

        Assert.That(
            actual: context.Validate(geometry: curve).ToFin().IsSucc,
            expression: Is.True);
    }

    [Test]
    public void ValidatesClosedBrep() {
        GeometryContext context = Context();
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();

        Assert.That(
            actual: context.Validate(geometry: brep).ToFin().IsSucc,
            expression: Is.True);
    }

    [Test]
    public void ValidatesClosedMesh() {
        GeometryContext context = Context();
        using Mesh mesh = Mesh.CreateFromSphere(sphere: new Sphere(center: Point3d.Origin, radius: 1.0), xCount: 16, yCount: 16);

        Assert.That(
            actual: context.Validate(geometry: mesh).ToFin().IsSucc,
            expression: Is.True);
    }

    [Test]
    public void ValidatesExtrusion() {
        GeometryContext context = Context();
        using Curve profile = new Circle(plane: Plane.WorldXY, radius: 1.0).ToNurbsCurve();
        using Extrusion extrusion = Extrusion.Create(planarCurve: profile, height: 1.0, cap: true);

        Assert.That(
            actual: context.Validate(geometry: extrusion).ToFin().IsSucc,
            expression: Is.True);
    }

    [Test]
    public void RejectsShortCurve() {
        GeometryContext context = Context();
        using Curve curve = new Circle(plane: Plane.WorldXY, radius: 0.0001).ToNurbsCurve();

        Assert.That(
            actual: context.Validate(geometry: curve).ToFin().Match(
                Succ: static (Curve _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "curve-length-readiness",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    [Test]
    public void RejectsSelfIntersectingCurve() {
        GeometryContext context = Context();
        using Curve curve = new Polyline([
                Point3d.Origin,
                new Point3d(x: 1.0, y: 1.0, z: 0.0),
                new Point3d(x: 0.0, y: 1.0, z: 0.0),
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                Point3d.Origin,
            ])
            .ToNurbsCurve();

        Assert.That(
            actual: context.Validate(geometry: curve).ToFin().Match(
                Succ: static (Curve _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "curve-self-intersection",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    [Test]
    public void RejectsDiscontinuousCurve() {
        GeometryContext context = Context();
        using Curve curve = new Polyline([
                Point3d.Origin,
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 1.0, y: 1.0, z: 0.0),
            ])
            .ToNurbsCurve();

        Assert.That(
            actual: context.Validate(geometry: curve).ToFin().Match(
                Succ: static (Curve _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "continuity-readiness",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    [Test]
    public void RejectsOpenMesh() {
        GeometryContext context = Context();
        using Mesh mesh = new();
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2, vertex4: 3);
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();

        Assert.That(
            actual: context.Validate(geometry: mesh).ToFin().Match(
                Succ: static (Mesh _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "mesh-manifold-readiness",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    [Test]
    public void RejectsGappedPolycurve() {
        GeometryContext context = Context();
        using PolyCurve polyCurve = new();
        using LineCurve first = new(from: Point3d.Origin, to: new Point3d(x: 1.0, y: 0.0, z: 0.0));
        using LineCurve second = new(from: new Point3d(x: 2.0, y: 0.0, z: 0.0), to: new Point3d(x: 3.0, y: 0.0, z: 0.0));
        _ = polyCurve.Append(first);
        _ = polyCurve.Append(second);

        Assert.That(
            actual: context.Validate(geometry: polyCurve).ToFin().Match(
                Succ: static (PolyCurve _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "polycurve-structure",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    [Test]
    public void RejectsUnusableSurfaceDomain() {
        GeometryContext context = Context();
        using NurbsSurface surface = NurbsSurface.CreateFromCorners(
            corner1: Point3d.Origin,
            corner2: new Point3d(x: 1.0, y: 0.0, z: 0.0),
            corner3: new Point3d(x: 0.0, y: 1.0, z: 0.0));
        _ = surface.SetDomain(direction: 0, domain: new Interval(t0: 0.0, t1: 0.001));
        _ = surface.SetDomain(direction: 1, domain: new Interval(t0: 0.0, t1: 0.001));

        Assert.That(
            actual: context.Validate(geometry: surface).ToFin().Match(
                Succ: static (NurbsSurface _) => false,
                Fail: static (Error fault) => fault.Message.Contains(
                    value: "surface-domain-readiness",
                    comparisonType: StringComparison.Ordinal)),
            expression: Is.True);
    }

    private static GeometryContext Context() =>
        GeometryContext.FromDocument(doc: Optional(RhinoDoc.ActiveDoc)
                .ToFin(Error.New(message: "Rhino.Testing did not create an active Rhino document."))
                .Match(
                    Succ: static (RhinoDoc candidate) => candidate,
                    Fail: static (Error fault) => throw new AssertionException(message: fault.Message)))
            .ToFin()
            .Match(
                Succ: static (GeometryContext context) => context,
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));
}
