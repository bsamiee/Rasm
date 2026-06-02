using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: ring/polyline native metrics; static owns metadata, cluster admission/projection, mass normalization, Sinkhorn receipts.
internal static class CloudMetricGens {
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).ToFin(), label: "cloud context");
    public static readonly VectorCloudMetric[] All = [
        VectorCloudMetric.Normal, VectorCloudMetric.Area, VectorCloudMetric.Perimeter, VectorCloudMetric.EdgeAspect,
        VectorCloudMetric.Skewness, VectorCloudMetric.Compactness, VectorCloudMetric.MomentAnisotropy,
        VectorCloudMetric.RadiiOfGyration, VectorCloudMetric.AreaError, VectorCloudMetric.CentroidError,
        VectorCloudMetric.Centroid, VectorCloudMetric.BestFitPlane, VectorCloudMetric.PrincipalAxes,
        VectorCloudMetric.PrincipalFrame, VectorCloudMetric.Shape, VectorCloudMetric.BishopFrames,
        VectorCloudMetric.TangentFlow, VectorCloudMetric.CumulativeArcLength, VectorCloudMetric.EdgeCurvatures,
        VectorCloudMetric.OpenLength, VectorCloudMetric.Covariance, VectorCloudMetric.PrincipalDirection, VectorCloudMetric.Spread,
        VectorCloudMetric.OrientedNormals, VectorCloudMetric.PrincipalCurvature, VectorCloudMetric.Curvedness, VectorCloudMetric.ShapeIndex,
        VectorCloudMetric.Admission, VectorCloudMetric.Neighborhood, VectorCloudMetric.CurvatureReceipt,
    ];
    public static readonly Seq<Point3d> Triangle = Gens.UnitTriangle3;
    public static readonly Gen<Seq<double>> RawMass3 = Gens.Positive.Array[3].Select(static values => toSeq(values));
    public static readonly Gen<Seq<double>> Simplex3 = Gens.Simplex(count: 3);
    public static readonly Gen<Seq<double>> ZeroMass3 = Gens.Positive.Select(Gen.Int[start: 0, finish: 2], static (double mass, int zero) =>
        toSeq(Enumerable.Range(start: 0, count: 3).Select(i => i == zero ? 0.0 : mass + i + 1.0)));
    public static readonly Type[] OutputFamily = [
        typeof(Vector3d), typeof(double), typeof(Point3d), typeof(Plane),
        typeof(Seq<Vector3d>), typeof(VectorCloudShape), typeof(Seq<Plane>),
        typeof(SymmetricMatrix), typeof(Seq<double>), typeof(CloudCurvatureResult),
        typeof(CloudAdmissionReceipt), typeof(CloudNeighborhoodReceipt), typeof(CloudCurvatureReceipt),
    ];
    public static CloudMetricPolicy MetricPolicy(int neighbors, double residual = 1.0e-3) {
        Op key = Op.Of(name: "cloud-test");
        return Spec.SuccValue(
            from count in key.AcceptValidated<Dimension>(candidate: neighbors)
            from gap in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-8)
            from fit in key.AcceptValidated<PositiveMagnitude>(candidate: residual)
            select new CloudMetricPolicy(Neighborhood: new CloudNeighborhoodPolicy(NeighborCount: count, Radius: Option<PositiveMagnitude>.None, EigenGapTolerance: gap, FitResidualTolerance: fit)),
            label: "cloud metric policy");
    }
    public static CloudTransportPolicy TransportPolicy(double regularization = 1.0, int maxIterations = 64, bool debiased = false, double? massRelaxation = null, double? convergenceTolerance = null, double? couplingCutoff = null) =>
        Spec.SuccValue(CloudTransportPolicy.Of(regularization: regularization, maxIterations: maxIterations, debiased: debiased, massRelaxation: massRelaxation, convergenceTolerance: convergenceTolerance, couplingCutoff: couplingCutoff, key: Op.Of(name: "cloud-test")), label: "cloud transport policy");
    public static VectorCloud ClusterOf(Seq<Point3d> points) =>
        Spec.SuccValue(VectorCloud.Cluster(points: points, context: Model, key: Op.Of(name: "cloud-test")), label: "cluster");
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorCloudMetricLaws {
    [Fact]
    public void MetadataCasesAreDistinctAndUseKnownOutputs() =>
        Spec.Cases(items: CloudMetricGens.All, key: static metric => metric.Key, law: static metric =>
            Assert.Contains(expected: metric.Output, collection: CloudMetricGens.OutputFamily));
    [Fact]
    public void OutputFamiliesAreDeclaredByMetricRole() {
        Seq<(VectorCloudMetric Metric, Type Output)> cases = Seq(
            (VectorCloudMetric.Shape, typeof(VectorCloudShape)),
            (VectorCloudMetric.Covariance, typeof(SymmetricMatrix)),
            (VectorCloudMetric.Area, typeof(double)),
            (VectorCloudMetric.Perimeter, typeof(double)),
            (VectorCloudMetric.OpenLength, typeof(double)),
            (VectorCloudMetric.BestFitPlane, typeof(Plane)),
            (VectorCloudMetric.PrincipalFrame, typeof(Plane)),
            (VectorCloudMetric.PrincipalAxes, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.BishopFrames, typeof(Seq<Plane>)),
            (VectorCloudMetric.TangentFlow, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.OrientedNormals, typeof(Seq<Vector3d>)),
            (VectorCloudMetric.PrincipalCurvature, typeof(CloudCurvatureResult)),
            (VectorCloudMetric.CumulativeArcLength, typeof(Seq<double>)),
            (VectorCloudMetric.EdgeCurvatures, typeof(Seq<double>)),
            (VectorCloudMetric.Curvedness, typeof(Seq<double>)),
            (VectorCloudMetric.ShapeIndex, typeof(Seq<double>)),
            (VectorCloudMetric.Admission, typeof(CloudAdmissionReceipt)),
            (VectorCloudMetric.Neighborhood, typeof(CloudNeighborhoodReceipt)),
            (VectorCloudMetric.CurvatureReceipt, typeof(CloudCurvatureReceipt)));
        _ = cases.Iter(static c => Assert.Equal(expected: c.Output, actual: c.Metric.Output));
    }
    [Fact]
    public void ClusterMetricsAdmitProjectAndRejectUnsupportedOutputs() {
        VectorCloud cluster = Spec.SuccValue(VectorCloud.Cluster(points: CloudMetricGens.Triangle, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "cluster");
        Assert.True(VectorCloudMetric.Covariance.AdmitsCase(cloud: cluster));
        Assert.False(VectorCloudMetric.Area.AdmitsCase(cloud: cluster));
        Spec.Succ(VectorCloudMetric.Covariance.Project<SymmetricMatrix>(cloud: cluster, key: Op.Of(name: "cloud-test")),
            then: cov => {
                Assert.Equal(expected: 3, actual: cov.Dimension.Value);
                Spec.Equal(left: toSeq(cov.Upper.AsIterable()), right: toSeq(Numeric.CovarianceUpper(points: CloudMetricGens.Triangle).AsIterable()), tolerance: 1.0e-12, what: "covariance");
            });
        Spec.Succ(VectorCloudMetric.Centroid.Project<Point3d>(cloud: cluster, key: Op.Of(name: "cloud-test")),
            then: centroid => Spec.Equal(left: centroid, right: Numeric.Centroid(points: CloudMetricGens.Triangle), tolerance: 1.0e-12));
        Spec.Fail(VectorCloudMetric.Covariance.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
        Spec.Fail(VectorCloudMetric.Area.Project<double>(cloud: cluster, key: Op.Of(name: "cloud-test")));
        Assert.Equal(expected: typeof(CloudCurvatureResult), actual: VectorCloudMetric.PrincipalCurvature.Output);
    }
}

public sealed class VectorCloudMassLaws {
    [Fact]
    public void WeightedClusterNormalizesMassRail() =>
        Spec.ForAll(CloudMetricGens.RawMass3, mass => Spec.Succ(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), then: cloud => {
            VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: cloud);
            Assert.True(condition: cluster.Admission.IsValid);
            Assert.Equal(expected: CloudMetricGens.Triangle.Count, actual: cluster.Admission.OriginalToUnique.Count);
            Spec.Some(cluster.Mass, weights => {
                double total = Numeric.Sum(values: mass);
                Spec.Equal(left: Numeric.Sum(values: toSeq(weights.AsIterable())), right: 1.0, tolerance: 1.0e-12, what: "mass sum");
                Spec.Equal(left: toSeq(mass.AsIterable().Select(value => value / total)), right: toSeq(weights.AsIterable()), tolerance: 1.0e-12, what: "mass");
            });
        }));
    [Fact]
    public void WeightedClusterRejectsBadMassRails() {
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, 2.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, -1.0, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(0.0, 0.0, 0.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input");
        Spec.ForAll(CloudMetricGens.ZeroMass3, mass =>
            Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input"));
        Spec.ForAll(Gens.NonFinite, x =>
            Spec.FailCategory(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(1.0, x, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), category: "Input"));
    }
    [Fact]
    public void AlreadyNormalMassRailStaysProportional() =>
        Spec.ForAll(CloudMetricGens.Simplex3, mass => Spec.Succ(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: mass, context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), then: cloud =>
            Spec.Some(Assert.IsType<VectorCloud.ClusterCase>(@object: cloud).Mass, weights =>
                Spec.Equal(left: mass, right: toSeq(weights.AsIterable()), tolerance: 1.0e-12, what: "simplex mass"))));
    [Fact]
    public void ClusterAdmissionValidatesMassOnceAndRebuildsSameCase() {
        VectorCloud weighted = Spec.SuccValue(VectorCloud.WeightedCluster(points: CloudMetricGens.Triangle, mass: Seq(2.0, 3.0, 5.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "weighted");
        Spec.Succ(VectorCloud.Admit(value: weighted, key: Op.Of(name: "cloud-test")), admitted => {
            VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: admitted);
            Spec.Some(cluster.Mass, mass => Spec.Equal(left: Seq(0.2, 0.3, 0.5), right: toSeq(mass.AsIterable()), tolerance: 1.0e-12, what: "admitted mass"));
        });
        VectorCloud duplicates = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[0], CloudMetricGens.Triangle[0], CloudMetricGens.Triangle[1]), mass: Seq(2.0, 3.0, 5.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "dedup weighted");
        VectorCloud.ClusterCase dedup = Assert.IsType<VectorCloud.ClusterCase>(@object: duplicates);
        Assert.Equal(expected: (3, 2, 1, 1), actual: (dedup.Admission.InputCount, dedup.Admission.OutputCount, dedup.Admission.InputDuplicateCoordinateCount, dedup.Admission.MergedCoordinateCount));
        Spec.Some(dedup.Mass, mass => Spec.Equal(left: Seq(0.5, 0.5), right: toSeq(mass.AsIterable()), tolerance: 1.0e-12, what: "dedup mass"));
        Spec.Succ(VectorCloud.Admit(value: CloudMetricGens.ClusterOf(points: CloudMetricGens.Triangle), key: Op.Of(name: "cloud-test")), admitted =>
            Assert.True(condition: Assert.IsType<VectorCloud.ClusterCase>(@object: admitted).Mass.IsNone));
    }
    [Fact]
    public void SinkhornReceiptsUseTypedStopSemantics() {
        VectorCloud source = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[0]), mass: Seq(2.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "source");
        VectorCloud target = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[1]), mass: Seq(5.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "target");
        CloudTransportPolicy balanced = CloudMetricGens.TransportPolicy(maxIterations: 32);
        CloudTransportPolicy relaxed = CloudMetricGens.TransportPolicy(maxIterations: 32, massRelaxation: 2.0);
        Spec.Succ(CloudKernel.Sinkhorn<SinkhornReceipt>(source: source, target: target, policy: balanced, key: Op.Of(name: "cloud-test")), then: receipt =>
            Assert.Equal(expected: SinkhornStopKind.BalancedMarginalsConverged, actual: receipt.Stop));
        Spec.Succ(CloudKernel.Sinkhorn<SinkhornReceipt>(source: source, target: target, policy: relaxed, key: Op.Of(name: "cloud-test")), then: receipt =>
            Assert.Equal(expected: SinkhornStopKind.RelaxedScalingConverged, actual: receipt.Stop));
        Spec.Succ(CloudKernel.Sinkhorn<double>(source: source, target: target, policy: balanced, key: Op.Of(name: "cloud-test")),
            then: distance => Spec.Equal(left: distance, right: 1.0, tolerance: 1.0e-12, what: "one-point OT distance"));
        Spec.FailCategory(CloudKernel.Sinkhorn<double>(source: source, target: target, policy: default, key: Op.Of(name: "cloud-test")), category: "Tolerance");
        Spec.FailCategory(CloudKernel.Sinkhorn<Point3d>(source: source, target: target, policy: balanced, key: Op.Of(name: "cloud-test")), category: "Unsupported");
    }
    [Fact]
    public void SinkhornReceiptsExposeToleranceCutoffAndMarginalProof() {
        VectorCloud source = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[0], CloudMetricGens.Triangle[1]), mass: Seq(1.0, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "source");
        VectorCloud target = Spec.SuccValue(VectorCloud.WeightedCluster(points: Seq(CloudMetricGens.Triangle[0], CloudMetricGens.Triangle[2]), mass: Seq(1.0, 1.0), context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test")), label: "target");
        Op key = Op.Of(name: "cloud-test");
        CloudTransportPolicy policy = CloudMetricGens.TransportPolicy(regularization: 0.2, maxIterations: 128, debiased: true, convergenceTolerance: 1.0e-8, couplingCutoff: 1.0e-8);
        Spec.Succ(CloudKernel.Sinkhorn<SinkhornReceipt>(source: source, target: target, policy: policy, key: key), receipt => {
            Spec.Equal(left: receipt.ConvergenceTolerance, right: policy.ConvergenceTolerance.Value, tolerance: 0.0, what: "sinkhorn tolerance");
            Spec.Equal(left: receipt.CouplingCutoff, right: policy.CouplingCutoff.Value, tolerance: 0.0, what: "sinkhorn cutoff");
            Assert.True(condition: receipt.SourceConvergenceResidual <= policy.ConvergenceTolerance.Value);
            Assert.True(condition: receipt.TargetConvergenceResidual <= policy.ConvergenceTolerance.Value);
            Assert.Equal(expected: receipt.NonZeroCouplings, actual: receipt.Correspondences.NonZeroCount);
            Spec.Equal(left: receipt.CouplingMass, right: receipt.Correspondences.TotalMass, tolerance: 1.0e-12, what: "retained coupling mass");
            Assert.True(condition: receipt.Correspondences.CoveredSourceCount > 0);
            Assert.True(condition: receipt.Correspondences.CoveredTargetCount > 0);
            Assert.True(condition: receipt.Correspondences.RetainedSourceMass > 0.0 && receipt.Correspondences.RetainedTargetMass > 0.0);
            _ = receipt.Correspondences.Items.Iter(item => Spec.Some(item.Confidence, confidence => Assert.InRange(actual: confidence, low: 0.0, high: 1.0)));
        });
    }
}

public sealed class VectorCloudHullAndCurvatureLaws {
    [Fact]
    public void HullReceiptsExposeRejectedAndUnsupportedFactsWithoutNativeCalls() {
        VectorCloud tooFew = CloudMetricGens.ClusterOf(points: Seq(new Point3d(0.0, 0.0, 0.0), new Point3d(1.0, 0.0, 0.0), new Point3d(0.0, 1.0, 0.0)));
        Spec.Succ(VectorIntent.Hull(source: tooFew, kind: CloudHullKind.Convex3D, key: Op.Of(name: "cloud-test")).Bind(intent => intent.Project<CloudHullReceipt>(context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test"))), receipt => {
            Assert.Equal(expected: CloudHullStatus.Rejected, actual: receipt.Status);
            Assert.False(condition: receipt.CoplanarRejected);
            Assert.Equal(expected: 3, actual: receipt.InputCount);
            Assert.Equal(expected: 0, actual: receipt.OutputVertexCount);
            Assert.True(condition: receipt.NativeRouted);
        });
        Spec.Succ(VectorIntent.Hull(source: tooFew, kind: CloudHullKind.AlphaShape, key: Op.Of(name: "cloud-test")).Bind(intent => intent.Project<CloudHullReceipt>(context: CloudMetricGens.Model, key: Op.Of(name: "cloud-test"))), receipt => {
            Assert.Equal(expected: CloudHullStatus.Unsupported, actual: receipt.Status);
            Assert.False(condition: receipt.NativeRouted);
            Assert.Equal(expected: 0, actual: receipt.NativeFacetCount);
        });
    }
    [Fact]
    public void CurvatureResultReceiptRequiresConservedAcceptedRejectedCounts() {
        CloudNeighborhoodReceipt neighborhood = new(InputCount: 2, QueryCount: 2, RequestedNeighborCount: 2, SearchBackend: CloudNeighborhoodSearchBackend.RhinoPointCloudKnn, RadiusLimited: false, Radius: Option<double>.None, NativeIndexRouted: true, SelfNeighborIncluded: true, EmptyNeighborhoodCount: 0, OutOfRangeIndexCount: 0, DuplicateIndexCount: 0, DuplicateCoordinateCount: 0, MinReturnedCount: 1, MaxReturnedCount: 1, MeanReturnedCount: 1.0);
        CloudCurvatureRangeReceipt range = new(AcceptedSampleCount: 0, Kind: CloudCurvatureRangeKind.Empty, PlaneLikeCount: 0, SphereLikeCount: 0, SaddleLikeCount: 0, MixedCount: 0, MinK1: 0.0, MaxK1: 0.0, MinK2: 0.0, MaxK2: 0.0, MinGaussian: 0.0, MaxGaussian: 0.0, MinMean: 0.0, MaxMean: 0.0, MinShapeIndex: 0.0, MaxShapeIndex: 0.0, Tolerance: 1.0e-4);
        CloudCurvatureReceipt receipt = new(InputCount: 2, RequestedNeighborCount: 2, AcceptedSampleCount: 0, RejectedSampleCount: 2, RankRejectedCount: 1, ResidualRejectedCount: 1, MeanResidual: 0.0, MaxResidual: 0.0, EigenGapTolerance: 1.0e-8, FitResidualTolerance: 1.0e-4, Neighborhood: neighborhood, Range: range);
        CloudCurvatureResult result = new(Samples: Seq<CloudCurvatureSample>(), Receipt: receipt);
        Assert.True(condition: receipt.IsValid);
        Assert.True(condition: result.IsValid);
        Assert.False(condition: (receipt with { RejectedSampleCount = 1 }).IsValid);
        Assert.False(condition: (receipt with { RankRejectedCount = 0 }).IsValid);
    }
}
