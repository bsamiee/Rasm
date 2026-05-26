using LanguageExt;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
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

Op key = Op.Of(name: SCENARIO_NAME);
Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");
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

Probe.Require(result.Values.Count == native.Vertices.Count && spectral.Values.Count == result.Values.Count && values.Count == result.Values.Count, $"value.count={result.Values.Count}");
Probe.Require(result.Values.ForAll(static value => RhinoMath.IsValidDouble(x: value)), "descriptor values contain nonfinite entries");
Probe.Require(meshReceipt.RequestedEigenpairs == 3 && meshReceipt.ReturnedEigenpairs > 0 && meshReceipt.Eigen.IsUsable, $"mesh.receipt={meshReceipt}");
Probe.Require(meshReceipt.Assembly.IsSome, "descriptor receipt did not include DEC assembly facts");
Probe.Require(meshReceipt.Spectral.Policy.ScaleNormalization.Equals(policy.ScaleNormalization) && meshReceipt.Spectral.Policy.EnergyNormalization.Equals(policy.EnergyNormalization), $"mesh.policy={meshReceipt.Spectral.Policy}");
Probe.Require(spectralReceipt.Policy.ZeroModePolicy.Equals(policy.ZeroModePolicy) && spectralReceipt.ComparisonReady && spectralReceipt.EnergyNormalized && spectralReceipt.BandwidthNormalized, $"spectral.receipt={spectralReceipt}");
Probe.Require(spectralReceipt.VertexCount == native.Vertices.Count && spectralReceipt.EigenpairCount == meshReceipt.Spectral.EigenpairCount, $"spectral.counts={spectralReceipt}");

Evidence.EmitScenarioHeader(SCENARIO_NAME, CAPTURE_PATH);
Evidence.Emit("descriptor.values", result.Values.Count);
Evidence.Emit("descriptor.returnedEigenpairs", meshReceipt.ReturnedEigenpairs);
Evidence.Emit("descriptor.cacheHit", meshReceipt.SpectralCacheHit);
Evidence.Emit("descriptor.hasAssembly", meshReceipt.Assembly.IsSome);
Evidence.Emit("spectral.comparisonReady", spectralReceipt.ComparisonReady);
