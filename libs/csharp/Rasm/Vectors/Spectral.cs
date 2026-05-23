using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) {
    public bool IsValid =>
        Eigenvalues.Count == Eigenvectors.Count
        && Eigenvalues.ForAll(static lambda => RhinoMath.IsValidDouble(x: lambda) && lambda >= -RhinoMath.SqrtEpsilon)
        && Eigenvectors.ForAll(static phi => !phi.IsEmpty && phi.ForAll(RhinoMath.IsValidDouble));
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    public SpectralBasis Truncate(int k) =>
        k >= Eigenvalues.Count
            ? this
            : new SpectralBasis(
                Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]),
                Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}

// d0: |E|x|V| signed vertex-edge incidence; d1: |F|x|E| signed edge-face incidence.
// Star0/1/2 are the Hodge stars on 0/1/2-forms (lumped Voronoi area / half-cotangent / 1-area).
// d1*d0 = 0 cohomology identity holds for manifold input.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(
    SparseMatrix D0,
    SparseMatrix D1,
    Arr<double> Star0,
    Arr<double> Star1,
    Arr<double> Star2) {
    public bool IsValid =>
        D0.IsValid && D1.IsValid
        && Star0.Count == D0.Cols.Value
        && Star1.Count == D0.Rows.Value
        && Star2.Count == D1.Rows.Value
        && Star0.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0)
        && Star1.ForAll(RhinoMath.IsValidDouble)
        && Star2.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0);
}

[Union]
public abstract partial record SpectralFilter {
    private SpectralFilter() { }
    public sealed record HeatCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record BiharmonicCase : SpectralFilter;
    public sealed record DiffusionCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record CommuteTimeCase : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    public static SpectralFilter Heat(PositiveMagnitude time) => new HeatCase(Time: time);
    public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
    public static SpectralFilter Biharmonic => new BiharmonicCase();
    public static SpectralFilter Diffusion(PositiveMagnitude time) => new DiffusionCase(Time: time);
    public static SpectralFilter CommuteTime => new CommuteTimeCase();
    public static SpectralFilter Identity => new IdentityCase();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        heatCase: static (lambda, c) => Math.Exp(d: -c.Time.Value * lambda),
        waveCase: static (lambda, c) => SpectralCore.WaveKernel(eigenvalue: lambda, energy: c.Energy.Value, bandwidth: c.Bandwidth.Value),
        biharmonicCase: static (lambda, _) => lambda > RhinoMath.SqrtEpsilon ? 1.0 / (lambda * lambda) : 0.0,
        diffusionCase: static (lambda, c) => Math.Exp(d: -2.0 * c.Time.Value * lambda),
        commuteTimeCase: static (lambda, _) => lambda > RhinoMath.SqrtEpsilon ? 1.0 / lambda : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<Arr<double>> Apply(SpectralBasis basis, Option<Seq<int>> sources, Op key) =>
        guard(basis.IsValid, key.InvalidInput())
            .Bind(_ => SpectralCore.EvaluateFiltered(basis: basis, sources: sources, filter: this, key: key));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class SpectralCore {
    private const double WaveBandwidthFloor = 1e-9;
    private const double DegenerateTriangleArea = 1e-14;

    // Aubry-Schlickewei-Cremers 2011 wave kernel: log-domain Gaussian at energy E.
    internal static double WaveKernel(double eigenvalue, double energy, double bandwidth) {
        if (eigenvalue < RhinoMath.SqrtEpsilon) return 0.0;
        double ratio = (Math.Log(d: energy) - Math.Log(d: eigenvalue)) / Math.Max(val1: bandwidth, val2: WaveBandwidthFloor);
        return Math.Exp(d: -0.5 * ratio * ratio);
    }

    // Unified spectral evaluation: per-vertex signature when sources = None
    //   F(v)    = sum_k weight(lambda_k) * phi_k(v)^2
    // Pairwise distance when sources = Some(S):
    //   D(v)^2  = (1/|S|) sum_{s in S} sum_k weight(lambda_k) * (phi_k(v) - phi_k(s))^2
    // BOUNDARY ADAPTER -- nested fold over (eigenpair, vertex, sources?) populates a pre-allocated
    // buffer; Seq materialisation inflates allocations by an order of magnitude on dense meshes.
    internal static Fin<Arr<double>> EvaluateFiltered(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, Op key) {
        int n = basis.VertexCount;
        if (n == 0) return Fin.Fail<Arr<double>>(error: key.InvalidInput());
        Seq<int> sourceSet = sources.IfNone(toSeq<int>([]));
        if (!sourceSet.IsEmpty && sourceSet.AsIterable().Any(s => s < 0 || s >= n))
            return Fin.Fail<Arr<double>>(error: key.InvalidInput());
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n))
            return Fin.Fail<Arr<double>>(error: key.InvalidResult());
        bool isPairwise = !sourceSet.IsEmpty;
        double normFactor = isPairwise ? 1.0 / sourceSet.Count : 1.0;
        double[] result = new double[n];
        for (int k = 0; k < basis.Eigenvalues.Count; k++) {
            double w = filter.Weight(eigenvalue: basis.Eigenvalues[index: k]);
            Arr<double> phi = basis.Eigenvectors[index: k];
            if (isPairwise)
                for (int v = 0; v < n; v++) {
                    double phiV = phi[index: v];
                    for (int s = 0; s < sourceSet.Count; s++) {
                        double delta = phiV - phi[index: sourceSet[index: s]];
                        result[v] += w * delta * delta;
                    }
                }
            else
                for (int v = 0; v < n; v++) result[v] += w * phi[index: v] * phi[index: v];
        }
        if (isPairwise)
            for (int v = 0; v < n; v++) result[v] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: result[v] * normFactor));
        return key.AcceptValue(value: new Arr<double>(result));
    }

    // --- [DEC_ASSEMBLY] ---------------------------------------------------------------------
    // DEC operators on a triangle mesh: d0 stores +1 at edge endpoint hi, -1 at lo (i<j);
    // Star1[e] accumulates 0.5*(cot alpha + cot beta) across the two faces sharing e;
    // d1[f,e] is +/-1 per GetEdgesForFace's sameOrientation contract.
    internal static Fin<DiscreteCalculus> Build(MeshSpace space, Op key) =>
        space.Cache.Cotangent.Bind(L => AssembleDecOperators(mesh: space.Native, mass: L.MassLumped, key: key));

    // BOUNDARY ADAPTER -- iterates Rhino's TopologyEdges and Faces directly; Seq materialisation
    // doubles peak memory on dense meshes and obscures the per-face cot-weight accumulation.
    private static Fin<DiscreteCalculus> AssembleDecOperators(Mesh mesh, Arr<double> mass, Op key) {
        int vertCount = mesh.Vertices.Count;
        int edgeCount = mesh.TopologyEdges.Count;
        int faceCount = mesh.Faces.Count;
        List<(int Row, int Col, double Value)> d0 = new(capacity: 2 * edgeCount);
        List<(int Row, int Col, double Value)> d1 = new(capacity: 3 * faceCount);
        double[] star1 = new double[edgeCount];
        double[] star2 = new double[faceCount];
        for (int e = 0; e < edgeCount; e++) {
            IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
            int lo = Math.Min(val1: pair.I, val2: pair.J);
            int hi = Math.Max(val1: pair.I, val2: pair.J);
            d0.Add(item: (e, lo, -1.0));
            d0.Add(item: (e, hi, +1.0));
        }
        for (int f = 0; f < faceCount; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa; Vector3d bc = pc - pb;
            double area = 0.5 * Vector3d.CrossProduct(a: ab, b: ac).Length;
            if (area < DegenerateTriangleArea) continue;
            star2[f] = 1.0 / area;
            double cotA = -ab * -ac / (2.0 * area);
            double cotB = ab * -bc / (2.0 * area);
            double cotC = ac * bc / (2.0 * area);
            int tvA = mesh.TopologyVertices.TopologyVertexIndex(vertexIndex: face.A);
            int tvB = mesh.TopologyVertices.TopologyVertexIndex(vertexIndex: face.B);
            int tvC = mesh.TopologyVertices.TopologyVertexIndex(vertexIndex: face.C);
            int eBC = mesh.TopologyEdges.GetEdgeIndex(topologyVertex1: tvB, topologyVertex2: tvC);
            int eCA = mesh.TopologyEdges.GetEdgeIndex(topologyVertex1: tvC, topologyVertex2: tvA);
            int eAB = mesh.TopologyEdges.GetEdgeIndex(topologyVertex1: tvA, topologyVertex2: tvB);
            if (eBC >= 0) star1[eBC] += 0.5 * cotA;
            if (eCA >= 0) star1[eCA] += 0.5 * cotB;
            if (eAB >= 0) star1[eAB] += 0.5 * cotC;
            int[] edges = mesh.TopologyEdges.GetEdgesForFace(faceIndex: f, sameOrientation: out bool[] sameOrientation);
            for (int k = 0; k < edges.Length; k++) {
                if (edges[k] < 0) continue;
                d1.Add(item: (f, edges[k], sameOrientation[k] ? 1.0 : -1.0));
            }
        }
        Dimension vDim = Dimension.Create(value: vertCount);
        Dimension eDim = Dimension.Create(value: edgeCount);
        Dimension fDim = Dimension.Create(value: faceCount);
        return from D0 in SparseMatrix.FromTriplets(rows: eDim, cols: vDim, triplets: d0, key: key)
               from D1 in SparseMatrix.FromTriplets(rows: fDim, cols: eDim, triplets: d1, key: key)
               select new DiscreteCalculus(D0: D0, D1: D1, Star0: mass, Star1: new Arr<double>(star1), Star2: new Arr<double>(star2));
    }
}
