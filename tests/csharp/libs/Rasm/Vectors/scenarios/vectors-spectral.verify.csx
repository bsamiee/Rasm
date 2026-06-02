using System;
using System.Linq;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

static Mesh Tetrahedron() {
    Mesh mesh = new();
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
    _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 2, vertex3: 1);
    _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 3);
    _ = mesh.Faces.AddFace(vertex1: 1, vertex2: 2, vertex3: 3);
    _ = mesh.Faces.AddFace(vertex1: 2, vertex2: 0, vertex3: 3);
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}
static Mesh OpenSquare() {
    Mesh mesh = Mesh.CreateFromPlane(
        plane: Plane.WorldXY,
        xInterval: new Interval(t0: -1.0, t1: 1.0),
        yInterval: new Interval(t0: -1.0, t1: 1.0),
        xCount: 1,
        yCount: 1);
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}

Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");

static T Project<T>(Fin<VectorIntent> intent, Context context, Op key, string label) =>
    Probe.Expect(Probe.Expect(intent, $"{label}: intent").Project<T>(context: context, key: key), $"{label}: project");

// --- [SCENARIO: vectors-spectral-dec] ---------------------------------------------------
Scenario.Run("vectors-spectral-dec", CAPTURE_PATH, (key, facts) => {
    static Mesh DegenerateFaceMesh() {
        Mesh mesh = new();
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 0.0, y: 0.0, z: 1.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 1.0, y: 0.0, z: 1.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 1.0, z: 0.0);
        _ = mesh.Vertices.Add(x: 2.0, y: 0.0, z: 1.0);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2);
        _ = mesh.Faces.AddFace(vertex1: 3, vertex2: 4, vertex3: 5);
        _ = mesh.Faces.AddFace(vertex1: 6, vertex2: 7, vertex3: 8);
        _ = mesh.Faces.AddFace(vertex1: 0, vertex2: 3, vertex3: 6);
        _ = mesh.Normals.ComputeNormals();
        return mesh;
    }
    static Mesh TorusMesh(int uCount, int vCount) {
        Mesh mesh = new();
        double major = 3.0;
        double minor = 0.75;
        for (int u = 0; u < uCount; u++) {
            double theta = 2.0 * Math.PI * u / uCount;
            for (int v = 0; v < vCount; v++) {
                double phi = 2.0 * Math.PI * v / vCount;
                double radius = major + (minor * Math.Cos(d: phi));
                _ = mesh.Vertices.Add(x: radius * Math.Cos(d: theta), y: radius * Math.Sin(a: theta), z: minor * Math.Sin(a: phi));
            }
        }
        for (int u = 0; u < uCount; u++) {
            for (int v = 0; v < vCount; v++) {
                int a = (u * vCount) + v;
                int b = (((u + 1) % uCount) * vCount) + v;
                int c = (((u + 1) % uCount) * vCount) + ((v + 1) % vCount);
                int d = (u * vCount) + ((v + 1) % vCount);
                _ = mesh.Faces.AddFace(vertex1: a, vertex2: b, vertex3: c, vertex4: d);
            }
        }
        _ = mesh.Normals.ComputeNormals();
        _ = mesh.Compact();
        return mesh;
    }
    using Mesh native = Tetrahedron();
    MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
    DiscreteCalculus calculus = Project<DiscreteCalculus>(intent: VectorIntent.DiscreteCalculus(space: space, key: key), context: context, key: key, label: "dec");
    SpectralAssemblyReceipt receipt = calculus.Receipt;
    using Mesh openNative = OpenSquare();
    MeshSpace openSpace = Probe.Expect(MeshSpace.Of(native: openNative, context: context, key: key), "open space");
    SpectralAssemblyReceipt openReceipt = Project<SpectralAssemblyReceipt>(intent: VectorIntent.DiscreteCalculus(space: openSpace, key: key), context: context, key: key, label: "open dec");
    FlattenResult flatten = Project<FlattenResult>(intent: VectorIntent.Flatten(space: openSpace, key: key), context: context, key: key, label: "flatten result");
    FlattenReceipt flattenReceipt = Project<FlattenReceipt>(intent: VectorIntent.Flatten(space: openSpace, key: key), context: context, key: key, label: "flatten receipt");
    RemeshKind simplify = Probe.Expect(RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 1 }, key: key), "simplify kind");
    RemeshResult remesh = Project<RemeshResult>(intent: VectorIntent.Remesh(space: openSpace, kind: simplify, key: key), context: context, key: key, label: "remesh result");
    RemeshReceipt remeshReceipt = Project<RemeshReceipt>(intent: VectorIntent.Remesh(space: openSpace, kind: simplify, key: key), context: context, key: key, label: "remesh receipt");
    using Mesh degenerateNative = DegenerateFaceMesh();
    MeshSpace degenerateSpace = Probe.Expect(MeshSpace.Of(native: degenerateNative, context: context, key: key), "degenerate space");
    SpectralAssemblyReceipt degenerateReceipt = Project<SpectralAssemblyReceipt>(intent: VectorIntent.DiscreteCalculus(space: degenerateSpace, key: key), context: context, key: key, label: "degenerate dec");
    using Mesh torusNative = TorusMesh(uCount: 8, vCount: 6);
    MeshSpace torusSpace = Probe.Expect(MeshSpace.Of(native: torusNative, context: context, key: key), "torus space");
    TopologyReceipt torusTopology = Project<TopologyReceipt>(intent: VectorIntent.Topology(space: torusSpace, key: key), context: context, key: key, label: "torus topology");
    bool genusPositiveHodgeUnsupported = VectorField.Hodge(source: VectorField.Constant(value: Vector3d.XAxis), space: torusSpace, key: key)
        .Bind(field => VectorIntent.Probe(source: ExtractionProbe.Vector(source: field), sample: torusNative.Vertices[0], key: key))
        .Bind(intent => intent.Project<Vector3d>(context: context, key: key))
        .Match(Succ: static _ => false, Fail: static error => error.Category() == "Unsupported");
    VectorAngle featureAngle = Probe.Expect(key.AcceptValidated<VectorAngle>(candidate: 0.1), "feature angle");
    PositiveMagnitude curvatureThreshold = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 0.01), "feature curvature");
    PositiveMagnitude featureScale = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 1.0), "feature scale");
    MeshFeaturePolicy curvaturePolicy = new(DihedralThreshold: featureAngle, CurvatureThreshold: curvatureThreshold, SmoothingScale: featureScale, FaceRegions: Option<Arr<int>>.None);
    MeshFeaturePolicy regionPolicy = curvaturePolicy with { FaceRegions = Some(new Arr<int>((System.Collections.Generic.IEnumerable<int>)new[] { 0, 1, 0, 1 })) };
    FeatureReceipt curvatureFeatures = Project<FeatureReceipt>(intent: VectorIntent.Features(space: space, policy: curvaturePolicy, key: key), context: context, key: key, label: "curvature features");
    FeatureReceipt regionFeatures = Project<FeatureReceipt>(intent: VectorIntent.Features(space: space, policy: regionPolicy, key: key), context: context, key: key, label: "region features");
    Arr<double> faceValues = new((System.Collections.Generic.IEnumerable<double>)new[] { 0.0, 0.25, 1.0, 1.25 });
    MeshDescriptor identityDescriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity);
    MeshSegmentation thresholdKind = Probe.Expect(MeshSegmentation.ScalarThreshold(values: faceValues, threshold: 0.5, key: key), "threshold segmentation");
    MeshSegmentation bandsKind = Probe.Expect(MeshSegmentation.ScalarBands(values: faceValues, bandCount: 2, key: key), "band segmentation");
    MeshSegmentation growKind = Probe.Expect(MeshSegmentation.SeededRegionGrow(values: faceValues, seedFaces: Seq(0, 2), tolerance: 0.4, maxIterations: 16, key: key), "grow segmentation");
    MeshSegmentation clustersKind = Probe.Expect(MeshSegmentation.DescriptorClusters(descriptor: identityDescriptor, eigenpairs: 3, regionCount: 2, maxIterations: 16, tolerance: 1.0e-9, key: key), "descriptor segmentation");
    MeshSegmentation watershedKind = Probe.Expect(MeshSegmentation.Watershed(values: faceValues, mergeTolerance: 0.1, key: key), "watershed segmentation");
    MeshSegmentation ncutKind = Probe.Expect(MeshSegmentation.NormalizedCut(values: faceValues, regionCount: 2, eigenpairs: 2, maxIterations: 16, tolerance: 1.0e-9, key: key), "ncut segmentation");
    MeshSegmentationReceipt[] segmentationReceipts = new[] {
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: thresholdKind, key: key), context: context, key: key, label: "threshold receipt"),
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: bandsKind, key: key), context: context, key: key, label: "bands receipt"),
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: growKind, key: key), context: context, key: key, label: "grow receipt"),
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: clustersKind, key: key), context: context, key: key, label: "clusters receipt"),
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: watershedKind, key: key), context: context, key: key, label: "watershed receipt"),
        Project<MeshSegmentationReceipt>(intent: VectorIntent.Segmentation(space: space, kind: ncutKind, key: key), context: context, key: key, label: "ncut receipt"),
    };
    ExtractionDomain meshDomain = Probe.Expect(ExtractionDomain.Mesh(value: space, key: key), "mesh domain");
    SampleKind meshSampleKind = Probe.Expect(SampleKind.Capacity(count: 2, capacity: 4, iterations: 2, tolerance: 1.0, key: key), "mesh sample kind");
    SampleReceipt meshSample = Project<SampleReceipt>(intent: VectorIntent.Sample(domain: meshDomain, kind: meshSampleKind, key: key), context: context, key: key, label: "mesh sample receipt");
    SampleAlgorithmReceipt sampleAlgorithm = Probe.ExpectSome(result: meshSample.Algorithm, label: "mesh sample algorithm");
    MeshSamplingSpectrumReceipt sampleSpectrum = Probe.ExpectSome(result: sampleAlgorithm.Spectrum, label: "mesh sample spectrum");
    VectorField logField = Probe.Expect(VectorField.TangentLogMap(space: space, source: 0, time: 0.05, key: key), "tangent log map field");
    TangentLogMapReceipt logReceipt = Project<TangentLogMapReceipt>(intent: VectorIntent.Probe(source: ExtractionProbe.Vector(source: logField), sample: native.Vertices[1], key: key), context: context, key: key, label: "tangent log receipt");
    Probe.Require(calculus.IsValid, "valid DEC");
    Probe.Require(receipt.VertexCount == 4 && receipt.EdgeCount == 6 && receipt.FaceCount == 4, $"receipt={receipt}");
    Probe.Require(receipt.BoundaryCompositionResidual <= RhinoMath.SqrtEpsilon, $"d1d0={receipt.BoundaryCompositionResidual:R}");
    Probe.Require(receipt.PositiveStar0Count == 4 && receipt.PositiveStar2Count == 4, $"stars={receipt}");
    Probe.Require(receipt.Genus.Map(static genus => genus == 0).IfNone(false), $"genus={receipt.Genus}");
    Probe.Require(receipt.BoundaryEdgeCount == 0 && receipt.BoundaryComponentCount == 0 && receipt.NonManifoldEdgeCount == 0 && receipt.EulerCharacteristic == 2 && receipt.TopologyEulerValidated, $"closed.topology={receipt}");
    Probe.Require(openReceipt.BoundaryEdgeCount > 0 && openReceipt.BoundaryComponentCount > 0 && openReceipt.HarmonicDimension == 0, $"open.receipt={openReceipt}");
    Probe.Require(flatten.Mesh.IsValid && flatten.Receipt.Valid && flattenReceipt.Valid, $"flatten={flattenReceipt}");
    Probe.Require(flattenReceipt.EdgeLengthDistortionRms.Map(static rms => RhinoMath.IsValidDouble(x: rms) && rms >= 0.0).IfNone(false), $"flatten.distortion={flattenReceipt.EdgeLengthDistortionRms}");
    Probe.Require(remesh.Mesh.IsValid && remesh.Receipt.Status.Equals(RemeshStatus.Completed) && remeshReceipt.Status.Equals(RemeshStatus.Completed), $"remesh={remeshReceipt}");
    Probe.Require(degenerateReceipt.Kind.Equals(SpectralAssemblyKind.Dec) && degenerateReceipt.SkippedDegenerateFaces > 0, $"degenerate.receipt={degenerateReceipt}");
    Probe.Require(torusTopology.Genus.Map(static genus => genus > 0).IfNone(false) && genusPositiveHodgeUnsupported, $"torus.topology={torusTopology}");
    Probe.Require(curvatureFeatures.Algorithm?.Equals(MeshFeatureAlgorithm.DihedralProxy) == true && curvatureFeatures.Edges.Count > 0 && curvatureFeatures.Edges.AsIterable().Any(static edge => edge.SignedDihedralRadians.IsSome), $"curvature.features={curvatureFeatures}");
    Probe.Require(regionFeatures.Algorithm?.Equals(MeshFeatureAlgorithm.DihedralProxy) == true && regionFeatures.RegionBoundaryEdges > 0 && regionFeatures.Edges.AsIterable().Any(static edge => edge.Kind.Equals(MeshFeatureKind.RegionBoundary)), $"region.features={regionFeatures}");
    Probe.Require(segmentationReceipts.Select(static receipt => receipt.Algorithm.Key).Distinct().Count() == 6 && segmentationReceipts.All(static receipt => receipt.AssignedFaceCount > 0 && (receipt.Status.Equals(MeshSegmentationStatus.Completed) || receipt.Status.Equals(MeshSegmentationStatus.MaxIterationsExhausted))), "segmentation receipts did not cover all six rails");
    Probe.Require(sampleAlgorithm.Kind.Equals(SampleAlgorithmKind.CapacityLimitedLloydCandidate) && sampleAlgorithm.CapacityResidualValidated && !sampleAlgorithm.TransportAssignmentValidated && sampleAlgorithm.MeshSpectrumValidated == sampleSpectrum.Validated && sampleSpectrum.Algorithm?.Equals(MeshSamplingSpectrumAlgorithm.CandidateSpectrum) == true && sampleSpectrum.SampleCount == meshSample.Emitted, $"sample.spectrum={sampleSpectrum}");
    Probe.Require(logReceipt.VectorHeatBacked && logReceipt.RejectsFlippedIntrinsic && logReceipt.Algorithm?.Equals(TangentLogMapAlgorithm.VectorHeatApproximate) == true && logReceipt.FiniteLogCount == 1, $"log.receipt={logReceipt}");
    facts.Add("dec.nonzeros", receipt.NonZeros);
    facts.Add("dec.d1d0", receipt.BoundaryCompositionResidual);
    facts.Add("open.boundaryEdges", openReceipt.BoundaryEdgeCount);
    facts.Add("flatten.distortion", flattenReceipt.EdgeLengthDistortionRms.IfNone(-1.0));
    facts.Add("remesh.faces", remeshReceipt.PostFaceCount);
    facts.Add("torus.genus", torusTopology.Genus.IfNone(-1));
    facts.Add("features.regionBoundary", regionFeatures.RegionBoundaryEdges);
    facts.Add("segmentation.rails", segmentationReceipts.Length);
    facts.Add("sample.spectrumValidated", sampleSpectrum.Validated);
    facts.Add("log.vectorHeatBacked", logReceipt.VectorHeatBacked);
});

// --- [SCENARIO: vectors-spectral-descriptor] --------------------------------------------
Scenario.Run("vectors-spectral-descriptor", CAPTURE_PATH, (key, facts) => {
    using Mesh native = Tetrahedron();
    MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
    SpectralDescriptorPolicy policy = new(
        ScaleNormalization: SpectralScaleNormalization.FirstNonZeroEigenvalue,
        EnergyNormalization: SpectralEnergyNormalization.UnitL2,
        ZeroModePolicy: SpectralZeroModePolicy.Drop,
        CropCount: Some(Dimension.Create(value: 2)));
    MeshDescriptor descriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity, sources: Option<Seq<int>>.None, policy: policy);
    VectorIntent intent = Probe.Expect(VectorIntent.Descriptor(space: space, kind: descriptor, pairs: 3, key: key), "descriptor intent");
    DescriptorResult result = Probe.Expect(intent.Project<DescriptorResult>(context: context, key: key), "descriptor result");
    DescriptorReceipt meshReceipt = Probe.Expect(intent.Project<DescriptorReceipt>(context: context, key: key), "descriptor receipt");
    SpectralDescriptor spectral = Probe.Expect(intent.Project<SpectralDescriptor>(context: context, key: key), "spectral descriptor");
    SpectralDescriptorReceipt spectralReceipt = Probe.Expect(intent.Project<SpectralDescriptorReceipt>(context: context, key: key), "spectral receipt");
    Arr<double> values = Probe.Expect(intent.Project<Arr<double>>(context: context, key: key), "descriptor values");
    PositiveMagnitude waveEnergy = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 2.0), "wave energy");
    PositiveMagnitude waveBandwidth = Probe.Expect(key.AcceptValidated<PositiveMagnitude>(candidate: 0.75), "wave bandwidth");
    MeshDescriptor waveDescriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Wave(energy: waveEnergy, bandwidth: waveBandwidth), sources: Option<Seq<int>>.None, policy: SpectralDescriptorPolicy.Raw);
    VectorIntent waveIntent = Probe.Expect(VectorIntent.Descriptor(space: space, kind: waveDescriptor, pairs: 3, key: key), "wave descriptor intent");
    SpectralDescriptorReceipt waveReceipt = Probe.Expect(waveIntent.Project<SpectralDescriptorReceipt>(context: context, key: key), "wave spectral receipt");
    SpectralWaveReceipt wave = Probe.ExpectSome(result: waveReceipt.Wave, label: "wave receipt");
    Probe.Require(result.Values.Count == native.Vertices.Count && spectral.Values.Count == result.Values.Count && values.Count == result.Values.Count, $"value.count={result.Values.Count}");
    Probe.Require(result.Values.ForAll(static value => RhinoMath.IsValidDouble(x: value)), "descriptor values contain nonfinite entries");
    Probe.Require(meshReceipt.RequestedEigenpairs == 3 && meshReceipt.ReturnedEigenpairs > 0 && meshReceipt.Eigen.IsUsable, $"mesh.receipt={meshReceipt}");
    Probe.Require(meshReceipt.Assembly.IsSome, "descriptor receipt did not include DEC assembly facts");
    Probe.Require(meshReceipt.Spectral.Policy.ScaleNormalization.Equals(policy.ScaleNormalization) && meshReceipt.Spectral.Policy.EnergyNormalization.Equals(policy.EnergyNormalization), $"mesh.policy={meshReceipt.Spectral.Policy}");
    Probe.Require(spectralReceipt.Policy.ZeroModePolicy.Equals(policy.ZeroModePolicy) && spectralReceipt.ComparisonReady && spectralReceipt.EnergyNormalized && spectralReceipt.ScaleNormalized && spectralReceipt.Wave.IsNone, $"spectral.receipt={spectralReceipt}");
    Probe.Require(spectralReceipt.VertexCount == native.Vertices.Count && spectralReceipt.EigenpairCount == meshReceipt.Spectral.EigenpairCount, $"spectral.counts={spectralReceipt}");
    Probe.Require(!waveReceipt.ScaleNormalized && wave.WksNormalized && wave.NormalizedWeightSum is > 0.999999999 and < 1.000000001 && wave.NonZeroEigenpairCount > 0, $"wave.receipt={waveReceipt}");
    facts.Add("descriptor.values", result.Values.Count);
    facts.Add("descriptor.returnedEigenpairs", meshReceipt.ReturnedEigenpairs);
    facts.Add("descriptor.cacheHit", meshReceipt.SpectralCacheHit);
    facts.Add("descriptor.hasAssembly", meshReceipt.Assembly.IsSome);
    facts.Add("spectral.comparisonReady", spectralReceipt.ComparisonReady);
    facts.Add("spectral.waveNormalized", wave.WksNormalized);
    facts.Add("spectral.waveWeightSum", wave.NormalizedWeightSum);
});

// --- [SCENARIO: vectors-spectral-edge-connection] ---------------------------------------
Scenario.Run("vectors-spectral-edge-connection", CAPTURE_PATH, (key, facts) => {
    using Mesh native = OpenSquare();
    MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "space");
    VolumeSolverPolicy solver = Probe.Expect(VolumeSolverPolicy.SparseCholesky(residualTolerance: 1.0e-4, key: key), "solver policy");
    SdfMeshPolicy policy = Probe.Expect(SdfMeshPolicy.BoundarySignedHeat(solver: solver, key: key), "boundary policy");
    ScalarField field = Probe.Expect(ScalarField.SignedDistanceFromMesh(space: space, policy: policy, key: key), "boundary signed heat field");
    SdfSample sample = Probe.Expect(field.SampleSdfDetailed(sample: new Point3d(x: 0.25, y: 0.25, z: 0.1), context: context, key: key), "boundary signed heat sample");
    SdfMeshReceipt mesh = Probe.ExpectSome(result: sample.Receipt.Mesh, label: "mesh receipt");
    SignedHeatReceipt signed = Probe.ExpectSome(result: mesh.SignedHeat, label: "signed heat receipt");
    SpectralAssemblyReceipt edge = Probe.ExpectSome(result: signed.EdgeAssembly, label: "edge assembly receipt");
    SolveReceipt heat = Probe.ExpectSome(result: signed.HeatSolve, label: "heat solve");
    Probe.Require(edge.Kind == SpectralAssemblyKind.EdgeConnection && edge.EdgeCount > 0 && edge.ComponentCount == 2, $"edge.space={edge}");
    Probe.Require(edge.MatrixRows == edge.EdgeCount * edge.ComponentCount && edge.MatrixCols == edge.MatrixRows, $"edge.shape={edge}");
    Probe.Require(edge.AdmittedFaceCount + edge.SkippedDegenerateFaces == edge.FaceCount, $"edge.faces={edge}");
    Probe.Require(edge.PositiveMassCount > 0 && edge.PositiveMassCount <= edge.EdgeCount, $"edge.mass={edge}");
    Probe.Require(edge.SymmetryResidual <= RhinoMath.SqrtEpsilon && edge.FactorNonZeros.IsSome, $"edge.factor={edge}");
    Probe.Require(heat.IsUsable && signed.PoissonSolve.IsUsable, $"solves heat={heat} poisson={signed.PoissonSolve}");
    facts.Add("edgeDofs", edge.EdgeCount);
    facts.Add("edgeRows", edge.MatrixRows);
    facts.Add("edgeNonZeros", edge.NonZeros);
    facts.Add("edgeFactorNonZeros", edge.FactorNonZeros.IfNone(0));
    facts.Add("solverTolerance", solver.ResidualTolerance.Value);
});
