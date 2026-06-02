using System;
using System.Linq;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

static Mesh OpenGrid() {
    Mesh mesh = Mesh.CreateFromPlane(
        plane: Plane.WorldXY,
        xInterval: new Interval(t0: -1.0, t1: 1.0),
        yInterval: new Interval(t0: -1.0, t1: 1.0),
        xCount: 8,
        yCount: 8);
    _ = mesh.Normals.ComputeNormals();
    _ = mesh.Compact();
    return mesh;
}

static T Project<T>(Fin<VectorIntent> intent, Context context, Op key, string label) =>
    Probe.Expect(Probe.Expect(intent, $"{label}: intent").Project<T>(context: context, key: key), $"{label}: project");

Context context = Probe.Expect(Context.Of(units: Rhino.UnitSystem.Millimeters).ToFin(), "context");

Scenario.Run("vectors-sample-dwork-continuous", CAPTURE_PATH, (key, facts) => {
    Mesh native = OpenGrid();
    MeshSpace space = Probe.Expect(MeshSpace.Of(native: native, context: context, key: key), "mesh space");
    ExtractionDomain domain = Probe.Expect(ExtractionDomain.Mesh(value: space, key: key), "mesh domain");
    SampleKind kind = Probe.Expect(SampleKind.DworkVariableDensity(radius: ScalarField.Constant(value: 0.30), count: 5, minRadius: 0.25, attempts: 30, seed: 42, key: key), "dwork kind");
    VectorIntent intent = Probe.Expect(VectorIntent.Sample(domain: domain, kind: kind, key: key), "dwork intent");
    Point3d[] first = Project<Seq<Point3d>>(intent: Fin.Succ(intent), context: context, key: key, label: "first points").AsIterable().ToArray();
    Point3d[] second = Project<Seq<Point3d>>(intent: Fin.Succ(intent), context: context, key: key, label: "second points").AsIterable().ToArray();
    SampleReceipt receipt = Project<SampleReceipt>(intent: Fin.Succ(intent), context: context, key: key, label: "receipt");
    SampleAlgorithmReceipt algorithm = Probe.ExpectSome(result: receipt.Algorithm, label: "algorithm");
    DworkReceipt dwork = Probe.ExpectSome(result: algorithm.Dwork, label: "dwork receipt");
    Probe.Require(first.Length == second.Length && first.Length == receipt.Emitted && receipt.Emitted > 1, $"counts first={first.Length} second={second.Length} receipt={receipt}");
    for (int i = 0; i < first.Length; i++) {
        Probe.Require(first[i].DistanceTo(other: second[i]) <= context.Absolute.Value, $"determinism index={i} first={first[i]} second={second[i]}");
        MeshPoint hit = native.ClosestMeshPoint(testPoint: first[i], maximumDistance: context.Absolute.Value);
        Probe.Require(hit is not null && hit.FaceIndex >= 0, $"mesh projection index={i} point={first[i]}");
    }
    Probe.Require(dwork.Domain.Equals(DworkSamplingDomain.ContinuousMesh) && dwork.ContinuousMesh && !dwork.CandidateOnly, $"dwork.domain={dwork.Domain}");
    Probe.Require(receipt.CandidateCount.IsNone && dwork.BackgroundCellSize.IsSome && dwork.BackgroundGridCells.IfNone(0) > 0, $"dwork.grid={dwork}");
    Probe.Require(dwork.ActiveListAnnulusSampling && dwork.LocalRadiusConflictChecks && dwork.DeterministicSeed, $"dwork.flags={dwork}");
    Probe.Require(dwork.GeneratedCandidates == receipt.Attempted && receipt.Rejected == dwork.RejectedTooClose + dwork.RejectedDomain, $"dwork.counts={dwork} receipt={receipt}");
    Probe.Require(receipt.MinSpacing.Map(min => min + context.Absolute.Value >= 0.30).IfNone(false), $"spacing={receipt.MinSpacing}");
    Probe.Require(!algorithm.TransportAssignmentValidated && !algorithm.CapacityResidualValidated, $"algorithm={algorithm}");
    facts.Add("dwork.emitted", receipt.Emitted);
    facts.Add("dwork.rejectedTooClose", dwork.RejectedTooClose);
    facts.Add("dwork.rejectedDomain", dwork.RejectedDomain);
    facts.Add("dwork.generated", dwork.GeneratedCandidates);
    facts.Add("dwork.gridCells", dwork.BackgroundGridCells.IfNone(0));
    facts.Add("dwork.spectrumValidated", algorithm.MeshSpectrumValidated);
});
