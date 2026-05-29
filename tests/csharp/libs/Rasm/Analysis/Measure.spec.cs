using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Rasm.Analysis.Measure is bridge-heavy: every magnitude/centroid/moment success evaluates a live native
// kernel (LengthMassProperties/AreaMassProperties/VolumeMassProperties.Compute, GeometryKernel.*Form,
// Curve.GetLength, SubD.ToBrep) and is owned by *.verify.csx, NOT faked here. Deferred surfaces:
// MassKind.Compute/Aggregate (+ LengthOf/AreaOf/VolumeOf/SumAggregate), PrincipalFrameOf, Analyze.LengthOf/
// CentroidOf/MassCentroidOf on geometry, MassPropertyExtract/PrincipalAxesFromMoments (they pattern-match
// *MassProperties objects whose CONSTRUCTION is native). The static rail owns the pure surface: the Measure
// union catalog (factory statics project the right case + payload), the MassProperty/MassKind SmartEnum
// catalogs (key-uniqueness + each case's EXACT declared Output + Suffix/Label + the First/Second/Product
// moments truth table re-derived by an INDEPENDENT oracle), and the Measure.Operation<TGeom,TOut> DISPATCH —
// a pure Type-shape decision (SpatialMidpoint requires TOut==Point3d; MassProperty requires TOut==Output;
// MassKind.None rejects InvalidInput "Input") asserted via IsSupported and the Reject-only Run rail (which
// short-circuits before any native evaluator, exactly like the Topology spec's null-aspect law).
internal static class MeasureGens {
    public static readonly Op Key = Op.Of(name: "measure-test");
    public static readonly MassProperty[] Properties =
        [MassProperty.Magnitude, MassProperty.MagnitudeError, MassProperty.Centroid, MassProperty.CentroidError,
         MassProperty.Radii, MassProperty.PrincipalAxes, MassProperty.Inertia, MassProperty.InertiaProducts];
    public static readonly MassKind[] Kinds = [MassKind.None, MassKind.Length, MassKind.Area, MassKind.Volume];
    public static readonly MassKind[] MassKinds = [MassKind.Length, MassKind.Area, MassKind.Volume];
    // Independent moment oracle: re-derives First/Second/Product per (property,kind) from the property semantics,
    // not from production's stored Func<MassKind,bool> closures. First non-trivial above Magnitude; Second adds
    // the Length-error special case; Product only for the inertia family (PrincipalAxes/Inertia/InertiaProducts).
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
    public void KeysAreContiguousAndOutputTypesArePinned() {
        Spec.SmartEnumCatalogMatches(production: MeasureGens.Properties, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7], key: static p => p.Key);
        Spec.Cases(items: MeasureGens.Properties, key: static p => p.Key, law: static p =>
            Assert.Equal(expected: p.Key switch {
                0 or 1 => typeof(double),
                2 => typeof(Point3d),
                5 => typeof(ValueTuple<double, Vector3d>),
                _ => typeof(Vector3d),
            }, actual: p.Output));
    }
    [Fact]
    public void OutputPartitionMatchesDeclaredMultiplicities() {
        Assert.Equal(expected: 2, actual: MeasureGens.Properties.Count(static p => p.Output == typeof(double)));
        Assert.Equal(expected: 1, actual: MeasureGens.Properties.Count(static p => p.Output == typeof(Point3d)));
        Assert.Equal(expected: 4, actual: MeasureGens.Properties.Count(static p => p.Output == typeof(Vector3d)));
        Assert.Equal(expected: 1, actual: MeasureGens.Properties.Count(static p => p.Output == typeof(ValueTuple<double, Vector3d>)));
    }
    [Fact]
    public void SuffixesAreDistinctAndMagnitudeAloneIsEmpty() {
        Assert.Equal(expected: string.Empty, actual: MassProperty.Magnitude.Suffix);
        Assert.Equal(expected: MeasureGens.Properties.Length, actual: MeasureGens.Properties.Select(static p => p.Suffix).Distinct(comparer: StringComparer.Ordinal).Count());
    }
    [Fact]
    public void MomentFlagsMatchIndependentTruthTableAcrossEveryKind() =>
        Spec.Cases(items: MeasureGens.Properties, key: static p => p.Key, law: static p =>
            _ = MeasureGens.Kinds.AsIterable().Iter(k => {
                Assert.Equal(expected: MeasureGens.ExpectFirst(p), actual: p.FirstMoments(mass: k));
                Assert.Equal(expected: MeasureGens.ExpectSecond(p, k), actual: p.SecondMoments(mass: k));
                Assert.Equal(expected: MeasureGens.ExpectProduct(p), actual: p.ProductMoments(mass: k));
            }));
}

public sealed class MassKindCatalogLaws {
    [Fact]
    public void KeysAreContiguousLabelsAndRequirementsArePinned() {
        Spec.SmartEnumCatalogMatches(production: MeasureGens.Kinds, expectedKeys: [0, 1, 2, 3], key: static k => k.Key);
        Spec.Cases(items: MeasureGens.Kinds, key: static k => k.Key, law: static k => {
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
        // Area/Volume fix Magnitude; the parametric factories transport (mass, property) verbatim.
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
    public void LengthSupportsDoubleOverCurveGeometryAndRejectsForeignOutput() {
        Assert.True(condition: Measure.Length.Operation<Curve, double>().IsSupported);
        Assert.True(condition: Measure.Length.Operation<GeometryBase, double>().IsSupported);
        Assert.False(condition: Measure.Length.Operation<Curve, Point3d>().IsSupported);
        Assert.False(condition: Measure.Length.Operation<Point3d, double>().IsSupported);
    }
    [Fact]
    public void SpatialMidpointRequiresPoint3dOutputForEverySupportedGeometry() {
        Assert.True(condition: Measure.SpatialMidpoint.Operation<Mesh, Point3d>().IsSupported);
        Assert.True(condition: Measure.SpatialMidpoint.Operation<Brep, Point3d>().IsSupported);
        Assert.True(condition: Measure.SpatialMidpoint.Operation<BoundingBox, Point3d>().IsSupported);
        Assert.False(condition: Measure.SpatialMidpoint.Operation<Mesh, Vector3d>().IsSupported);
        Assert.False(condition: Measure.SpatialMidpoint.Operation<Mesh, double>().IsSupported);
    }
    [Fact]
    public void MassPropertyIsSupportedExactlyWhenOutputEqualsDeclaredOutput() =>
        Spec.Cases(items: MeasureGens.Properties, key: static p => p.Key, law: static p => {
            Measure aspect = new Measure.MassPropertyCase(Mass: MassKind.Volume, Property: p);
            Assert.True(condition: aspect.Operation<Brep, double>().IsSupported == (p.Output == typeof(double)));
            Assert.True(condition: aspect.Operation<Brep, Point3d>().IsSupported == (p.Output == typeof(Point3d)));
            Assert.True(condition: aspect.Operation<Brep, Vector3d>().IsSupported == (p.Output == typeof(Vector3d)));
            Assert.True(condition: aspect.Operation<Brep, ValueTuple<double, Vector3d>>().IsSupported == (p.Output == typeof(ValueTuple<double, Vector3d>)));
        });
    [Fact]
    public void MassKindNoneRejectsEveryOutputWithInputFaultRegardlessOfPropertyOutputMatch() =>
        Spec.Cases(items: MeasureGens.Properties, key: static p => p.Key, law: static p => {
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
