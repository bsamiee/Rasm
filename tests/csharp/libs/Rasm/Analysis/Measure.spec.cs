using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native mass/centroid paths; static owns Measure catalog, MassProperty/MassKind metadata, moment oracle, Operation dispatch pre-native.
internal static class MeasureGens {
    public static readonly Op Key = Op.Of(name: "measure-test");
    public static readonly MassKind[] MassKinds = [.. MassKind.Items.Where(static k => k.Key != MassKind.None.Key)];
    // Independent moment oracle from property semantics (First/Second/Product), not production's stored predicates.
    public static bool ExpectFirst(MassProperty p) => p.Key is not (0 or 1);
    public static bool ExpectSecond(MassProperty p, MassKind k) => p.Key switch {
        0 => false,
        1 or 2 or 3 => k.Equals(MassKind.Length),
        _ => true,
    };
    public static bool ExpectProduct(MassProperty p) => p.Key >= 5;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MassPropertyCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesArePinned() =>
        Spec.SmartEnumOutputCatalog(items: MassProperty.Items, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7], key: static p => p.Key,
            output: static p => p.Output,
            expectedOutput: static p => p.Key switch {
                0 or 1 => typeof(double),
                2 => typeof(Point3d),
                5 => typeof(ValueTuple<double, Vector3d>),
                _ => typeof(Vector3d),
            });
    [Fact]
    public void OutputPartitionMatchesDeclaredMultiplicities() =>
        Assert.Multiple(
            () => Assert.Equal(expected: 2, actual: MassProperty.Items.Count(static p => p.Output == typeof(double))),
            () => Assert.Equal(expected: 1, actual: MassProperty.Items.Count(static p => p.Output == typeof(Point3d))),
            () => Assert.Equal(expected: 4, actual: MassProperty.Items.Count(static p => p.Output == typeof(Vector3d))),
            () => Assert.Equal(expected: 1, actual: MassProperty.Items.Count(static p => p.Output == typeof(ValueTuple<double, Vector3d>))));
    [Fact]
    public void SuffixesAreDistinctAndMagnitudeAloneIsEmpty() {
        Assert.Equal(expected: string.Empty, actual: MassProperty.Magnitude.Suffix);
        Assert.Equal(expected: MassProperty.Items.Count, actual: MassProperty.Items.Select(static p => p.Suffix).Distinct(comparer: StringComparer.Ordinal).Count());
    }
    [Fact]
    public void MomentFlagsMatchIndependentTruthTableAcrossEveryKind() =>
        Spec.Cases(items: MassProperty.Items, key: static p => p.Key, law: static p =>
            _ = MassKind.Items.AsIterable().Iter(k => {
                Assert.Equal(expected: MeasureGens.ExpectFirst(p), actual: p.FirstMoments(mass: k));
                Assert.Equal(expected: MeasureGens.ExpectSecond(p, k), actual: p.SecondMoments(mass: k));
                Assert.Equal(expected: MeasureGens.ExpectProduct(p), actual: p.ProductMoments(mass: k));
            }));
}

public sealed class MassKindCatalogLaws {
    [Fact]
    public void KeysAreContiguousLabelsAndRequirementsArePinned() {
        Spec.SmartEnumCatalogMatches(production: MassKind.Items, expectedKeys: [0, 1, 2, 3], key: static k => k.Key);
        Spec.Cases(items: MassKind.Items, key: static k => k.Key, law: static k => {
            Assert.Equal(expected: k.Key switch { 0 => "None", 1 => "Length", 2 => "Area", _ => "Volume" }, actual: k.Label);
            Assert.Equal(expected: k.Key switch {
                0 => Requirement.None,
                1 => Requirement.CurveLength,
                2 => Requirement.AreaMass,
                _ => Requirement.VolumeMass,
            }, actual: k.Requirement);
        });
    }
}

public sealed class MeasureUnionCatalogLaws {
    public static readonly (string Label, Measure Aspect)[] Cases =
        [("Length", Measure.Length), ("SpatialMidpoint", Measure.SpatialMidpoint), ("Area", Measure.Area), ("Volume", Measure.Volume),
         ("Centroid", Measure.Centroid(mass: MassKind.Volume)), ("MassError", Measure.MassError(mass: MassKind.Area)),
         ("CentroidError", Measure.CentroidError(mass: MassKind.Volume)), ("Radii", Measure.Radii(mass: MassKind.Volume)),
         ("PrincipalAxes", Measure.PrincipalAxes(mass: MassKind.Volume)), ("Inertia", Measure.Inertia(mass: MassKind.Volume)),
         ("InertiaProducts", Measure.InertiaProducts(mass: MassKind.Volume))];
    [Fact]
    public void FactoriesProjectThreeUnionCaseTypesAndCarryTheirMassPropertyPair() {
        _ = Assert.IsType<Measure.LengthCase>(@object: Measure.Length);
        _ = Assert.IsType<Measure.SpatialMidpointCase>(@object: Measure.SpatialMidpoint);
        Assert.Equal(expected: 3, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 9, actual: Cases.Count(static c => c.Aspect is Measure.MassPropertyCase));
        Assert.Equal(expected: (MassKind.Area, MassProperty.Magnitude), actual: AsPair(aspect: Measure.Area));
        Assert.Equal(expected: (MassKind.Volume, MassProperty.Magnitude), actual: AsPair(aspect: Measure.Volume));
        Assert.Equal(expected: (MassKind.Length, MassProperty.Centroid), actual: AsPair(aspect: Measure.Centroid(mass: MassKind.Length)));
        Assert.Equal(expected: (MassKind.Area, MassProperty.PrincipalAxes), actual: AsPair(aspect: Measure.PrincipalAxes(mass: MassKind.Area)));
    }
    [Fact]
    public void EveryMassFactoryRoutesToItsDistinctProperty() =>
        Spec.Cases(items: MeasureGens.MassKinds, key: static k => k.Key, law: static k => {
            Assert.Equal(expected: MassProperty.MagnitudeError, actual: AsPair(aspect: Measure.MassError(mass: k)).Property);
            Assert.Equal(expected: MassProperty.CentroidError, actual: AsPair(aspect: Measure.CentroidError(mass: k)).Property);
            Assert.Equal(expected: MassProperty.Radii, actual: AsPair(aspect: Measure.Radii(mass: k)).Property);
            Assert.Equal(expected: MassProperty.Inertia, actual: AsPair(aspect: Measure.Inertia(mass: k)).Property);
            Assert.Equal(expected: MassProperty.InertiaProducts, actual: AsPair(aspect: Measure.InertiaProducts(mass: k)).Property);
        });
    private static (MassKind Mass, MassProperty Property) AsPair(Measure aspect) {
        Measure.MassPropertyCase massCase = Assert.IsType<Measure.MassPropertyCase>(@object: aspect);
        return (massCase.Mass, massCase.Property);
    }
}

public sealed class MeasureDispatchLaws {
    [Fact]
    public void LengthSupportsDoubleOverCurveGeometryAndRejectsForeignOutput() =>
        Spec.SupportMatrix(
            ("Length Curve→double", static () => Measure.Length.Operation<Curve, double>().IsSupported, true),
            ("Length GeometryBase→double", static () => Measure.Length.Operation<GeometryBase, double>().IsSupported, true),
            ("Length foreign-output Point3d", static () => Measure.Length.Operation<Curve, Point3d>().IsSupported, false),
            ("Length foreign-geometry Point3d", static () => Measure.Length.Operation<Point3d, double>().IsSupported, false));
    [Fact]
    public void SpatialMidpointRequiresPoint3dOutputForEverySupportedGeometry() =>
        Spec.SupportMatrix(
            ("Midpoint Mesh→Point3d", static () => Measure.SpatialMidpoint.Operation<Mesh, Point3d>().IsSupported, true),
            ("Midpoint Brep→Point3d", static () => Measure.SpatialMidpoint.Operation<Brep, Point3d>().IsSupported, true),
            ("Midpoint BoundingBox→Point3d", static () => Measure.SpatialMidpoint.Operation<BoundingBox, Point3d>().IsSupported, true),
            ("Midpoint foreign-output Vector3d", static () => Measure.SpatialMidpoint.Operation<Mesh, Vector3d>().IsSupported, false),
            ("Midpoint foreign-output double", static () => Measure.SpatialMidpoint.Operation<Mesh, double>().IsSupported, false));
    [Fact]
    public void MassPropertyIsSupportedExactlyWhenOutputEqualsDeclaredOutput() =>
        Spec.Cases(items: MassProperty.Items, key: static p => p.Key, law: static p => {
            Measure aspect = new Measure.MassPropertyCase(Mass: MassKind.Volume, Property: p);
            Assert.True(condition: aspect.Operation<Brep, double>().IsSupported == (p.Output == typeof(double)));
            Assert.True(condition: aspect.Operation<Brep, Point3d>().IsSupported == (p.Output == typeof(Point3d)));
            Assert.True(condition: aspect.Operation<Brep, Vector3d>().IsSupported == (p.Output == typeof(Vector3d)));
            Assert.True(condition: aspect.Operation<Brep, ValueTuple<double, Vector3d>>().IsSupported == (p.Output == typeof(ValueTuple<double, Vector3d>)));
        });
    [Fact]
    public void MassKindNoneRejectsEveryOutputWithInputFaultRegardlessOfPropertyOutputMatch() =>
        Spec.Cases(items: MassProperty.Items, key: static p => p.Key, law: static p => {
            Measure aspect = new Measure.MassPropertyCase(Mass: MassKind.None, Property: p);
            Assert.False(condition: aspect.Operation<Brep, double>().IsSupported);
            Assert.False(condition: aspect.Operation<Brep, Vector3d>().IsSupported);
            Spec.Invalid(Analyze.Run(operation: aspect.Operation<Brep, double>(), input: default(Brep)!),
                then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        });
    [Fact]
    public void RejectedDispatchSurfacesYieldStableFaultCategoriesWithoutNativeEvaluation() {
        Spec.Invalid(Analyze.Run(operation: Analyze.Measure<Curve, double>(aspect: null!), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: Measure.SpatialMidpoint.Operation<Mesh, double>(), input: default(Mesh)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Mesh), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(double), actual: fault.OutputType);
            });
    }
}
