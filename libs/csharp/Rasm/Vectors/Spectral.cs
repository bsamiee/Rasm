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
// Star0/1/2 are Hodge stars on 0/1/2-forms (barycentric/lumped mass / half-cotangent / 1-area).
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
        && Star1.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0)
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

    // Partial monoidal composition. Closed pairs: Heat∘Heat (time-additive), Diffusion∘Diffusion
    // (time-additive), Identity neutral. All other pairs (Wave×Wave, Biharmonic×Heat,
    // CommuteTime×*, etc.) are not algebraically closed and return None — callers cannot fake
    // closure by silently producing approximate filters.
    public Option<SpectralFilter> Compose(SpectralFilter other) => (this, other) switch {
        (HeatCase a, HeatCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => (SpectralFilter)Heat(time: time)),
        (DiffusionCase a, DiffusionCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => (SpectralFilter)Diffusion(time: time)),
        (IdentityCase, _) => Some(other),
        (_, IdentityCase) => Some(this),
        _ => Option<SpectralFilter>.None,
    };
    private static Option<PositiveMagnitude> Positive(double value) =>
        PositiveMagnitude.TryCreate(value: value, obj: out PositiveMagnitude magnitude)
            ? Some(magnitude)
            : Option<PositiveMagnitude>.None;
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

    // --- [CROUZEIX_RAVIART] -----------------------------------------------------------------
    // Stein-Wardetzky-Jacobson-Grinspun 2020 Crouzeix-Raviart connection Laplacian on edges,
    // verified against geometry-central `intrinsic_geometry_interface.cpp` lines 586-679. Feng-
    // Crane 2024 SHM diffuses per-edge complex tangent encodings via (M_CR + t·L_CR) X_t = X_0
    // on the unflipped intrinsic-Delaunay edge set. The system is SPD-intended; factorisation
    // rejection remains a typed failure.
    // M_CR[e] = (1/3) Σ_{f∋e} area(f) (diagonal); used as both LHS coefficient and pivot for the
    // real-2|E| embedding [[A,-B],[B,A]] where H = A + iB is Hermitian.
    internal static Fin<SparseMatrix> BuildCrouzeixRaviartHeatSystem(MeshKernel.IntrinsicMesh mesh, double time, Op key) {
        if (!mesh.IsFrozen) return Fin.Fail<SparseMatrix>(error: key.InvalidInput());
        if (mesh.HasFlips) return Fin.Fail<SparseMatrix>(error: key.Unsupported(geometryType: typeof(MeshKernel.IntrinsicMesh), outputType: typeof(SparseMatrix)));
        int eCount = mesh.EdgeCount;
        if (eCount == 0) return Fin.Fail<SparseMatrix>(error: key.InvalidInput());
        int faceCapacity = mesh.Triangles.Count;
        List<(int Row, int Col, double Value)> triplets = new(capacity: (2 * eCount) + (faceCapacity * 36));
        // Diagonal mass M_CR: Σ_{f∋e} area(f)/3, accumulated per face then injected at (e,e) and
        // (e+|E|, e+|E|) for the real-block embedding.
        double[] mass = new double[eCount];
        foreach (int f in mesh.LiveFaceIndices()) {
            double area = mesh.AreaOfFace(faceIdx: f);
            if (area < DegenerateTriangleArea) continue;
            double contribution = area / 3.0;
            int[] edges = mesh.EdgesOfFace(faceIdx: f);
            for (int k = 0; k < edges.Length; k++) if (edges[k] >= 0) mass[edges[k]] += contribution;
        }
        for (int e = 0; e < eCount; e++) {
            triplets.Add(item: (e, e, mass[e]));
            triplets.Add(item: (e + eCount, e + eCount, mass[e]));
        }
        // Per-face CR connection contributions. Each surviving intrinsic face emits three corner
        // pairs; intrinsic edge orientations come from the canonical (Lo,Hi) ordering on
        // IntrinsicEdge — sameOrientation[k] is true iff the face's cyclic traversal of edges[k]
        // matches the canonical Lo→Hi direction.
        foreach (int f in mesh.LiveFaceIndices()) {
            (int A, int B, int C)? slot = mesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            int[] edges = mesh.EdgesOfFace(faceIdx: f);
            if (edges.Length != 3 || edges[0] < 0 || edges[1] < 0 || edges[2] < 0) continue;
            double lAB = mesh.EdgeAt(index: edges[0]).Length;
            double lBC = mesh.EdgeAt(index: edges[1]).Length;
            double lCA = mesh.EdgeAt(index: edges[2]).Length;
            double area = mesh.AreaOfFace(faceIdx: f);
            if (area < DegenerateTriangleArea) continue;
            bool oAB = mesh.EdgeAt(index: edges[0]).Lo == va;
            bool oBC = mesh.EdgeAt(index: edges[1]).Lo == vb;
            bool oCA = mesh.EdgeAt(index: edges[2]).Lo == vc;
            bool[] sameOrientation = [oAB, oBC, oCA];
            EmitCrouzeixRaviartPair(triplets: triplets, edges: edges, sameOrientation: sameOrientation, eCount: eCount, heA: 0, heB: 1, lA: lBC, lB: lAB, lOpp: lCA, area: area, time: time);
            EmitCrouzeixRaviartPair(triplets: triplets, edges: edges, sameOrientation: sameOrientation, eCount: eCount, heA: 1, heB: 2, lA: lCA, lB: lBC, lOpp: lAB, area: area, time: time);
            EmitCrouzeixRaviartPair(triplets: triplets, edges: edges, sameOrientation: sameOrientation, eCount: eCount, heA: 2, heB: 0, lA: lAB, lB: lCA, lOpp: lBC, area: area, time: time);
        }
        Dimension dim = Dimension.Create(value: 2 * eCount);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key);
    }
    private static void EmitCrouzeixRaviartPair(List<(int Row, int Col, double Value)> triplets, int[] edges, bool[] sameOrientation, int eCount, int heA, int heB, double lA, double lB, double lOpp, double area, double time) {
        int iE = edges[heA];
        int jE = edges[heB];
        if (iE < 0 || jE < 0) return;
        double weight = ((lA * lA) + (lB * lB) - (lOpp * lOpp)) / (2.0 * area);
        double cosTheta = ((lA * lA) + (lB * lB) - (lOpp * lOpp)) / (2.0 * lA * lB);
        double sinTheta = 2.0 * area / (lA * lB);
        double sij = sameOrientation[heA] == sameOrientation[heB] ? 1.0 : -1.0;
        double re = weight * sij * cosTheta * time;
        double im = -weight * sij * sinTheta * time;
        double diag = weight * time;
        triplets.Add(item: (iE, iE, diag));
        triplets.Add(item: (jE, jE, diag));
        triplets.Add(item: (iE + eCount, iE + eCount, diag));
        triplets.Add(item: (jE + eCount, jE + eCount, diag));
        triplets.Add(item: (iE, jE, re));
        triplets.Add(item: (jE, iE, re));
        triplets.Add(item: (iE + eCount, jE + eCount, re));
        triplets.Add(item: (jE + eCount, iE + eCount, re));
        triplets.Add(item: (iE, jE + eCount, -im));
        triplets.Add(item: (jE + eCount, iE, -im));
        triplets.Add(item: (iE + eCount, jE, im));
        triplets.Add(item: (jE, iE + eCount, im));
    }
    // Sample the per-edge complex field X_t at each intrinsic face barycenter. Per `SignedHeat
    // Solver::sampleAtFaceBarycenters`: for each face edge with canonical tangent t_e (unit) and
    // in-plane normal n_e = N_f × t_e, accumulate Y_f += Re(X_t[e])·t_e + Im(X_t[e])·n_e then
    // normalise per face. Result is Vector3d per intrinsic face index; null face slots produce
    // zero entries. The face plane uses extrinsic 3D vertex positions (the intrinsic face has
    // the same 3-vertex set in 3D as any flipped variant, just different edge structure).
    internal static Vector3d[] SampleCrouzeixRaviartFaceField(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Arr<double> stacked) {
        int eCount = imesh.EdgeCount;
        int faceCapacity = imesh.Triangles.Count;
        Vector3d[] field = new Vector3d[faceCapacity];
        foreach (int f in imesh.LiveFaceIndices()) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            Point3d pa = mesh.Vertices[index: va];
            Point3d pb = mesh.Vertices[index: vb];
            Point3d pc = mesh.Vertices[index: vc];
            Vector3d normal = Vector3d.CrossProduct(a: pb - pa, b: pc - pa);
            if (!normal.Unitize()) continue;
            int[] edges = imesh.EdgesOfFace(faceIdx: f);
            Vector3d acc = Vector3d.Zero;
            for (int k = 0; k < edges.Length; k++) {
                int e = edges[k];
                if (e < 0) continue;
                MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
                Vector3d tangent = (Vector3d)(mesh.Vertices[index: edge.Hi] - mesh.Vertices[index: edge.Lo]);
                if (!tangent.Unitize()) continue;
                Vector3d perp = Vector3d.CrossProduct(a: normal, b: tangent);
                double xRe = stacked[index: e];
                double xIm = stacked[index: e + eCount];
                acc += (xRe * tangent) + (xIm * perp);
            }
            double mag = acc.Length;
            field[f] = mag > RhinoMath.ZeroTolerance ? acc / mag : Vector3d.Zero;
        }
        return field;
    }
    // Cotangent vertex divergence on the intrinsic-Delaunay face set. Matches the extrinsic
    // ComputeVertexDivergence formula but reads cotangents from intrinsic edge lengths (via
    // Heron's law of cosines) and uses extrinsic vertex positions for the edge basis against
    // which Y_f is projected. Required by SHM since L_CR is intrinsic — the divergence MUST be
    // intrinsic to remain self-consistent on flipped faces.
    internal static Arr<double> ComputeIntrinsicVertexDivergence(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Vector3d[] faceFields) {
        int n = mesh.Vertices.Count;
        double[] div = new double[n];
        foreach (int f in imesh.LiveFaceIndices()) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            int[] edges = imesh.EdgesOfFace(faceIdx: f);
            if (edges.Length != 3 || edges[0] < 0 || edges[1] < 0 || edges[2] < 0) continue;
            double lAB = imesh.EdgeAt(index: edges[0]).Length;
            double lBC = imesh.EdgeAt(index: edges[1]).Length;
            double lCA = imesh.EdgeAt(index: edges[2]).Length;
            double area = imesh.AreaOfFace(faceIdx: f);
            if (area < DegenerateTriangleArea) continue;
            double quadArea = 4.0 * area;
            double cotA = ((lAB * lAB) + (lCA * lCA) - (lBC * lBC)) / quadArea;
            double cotB = ((lAB * lAB) + (lBC * lBC) - (lCA * lCA)) / quadArea;
            double cotC = ((lBC * lBC) + (lCA * lCA) - (lAB * lAB)) / quadArea;
            Point3d pa = mesh.Vertices[index: va];
            Point3d pb = mesh.Vertices[index: vb];
            Point3d pc = mesh.Vertices[index: vc];
            Vector3d ab = pb - pa;
            Vector3d bc = pc - pb;
            Vector3d ca = pa - pc;
            Vector3d g = faceFields[f];
            div[va] += 0.5 * ((cotB * (ab * g)) + (cotC * (-ca * g)));
            div[vb] += 0.5 * ((cotC * (bc * g)) + (cotA * (-ab * g)));
            div[vc] += 0.5 * ((cotA * (ca * g)) + (cotB * (-bc * g)));
        }
        return new Arr<double>(div);
    }

    // --- [CDS_HOLONOMY] ---------------------------------------------------------------------
    // Discrete angle defect K_v = 2π − Σ_{f∋v} interior_angle, computed from intrinsic edge
    // lengths via the law of cosines. Σ K_v = 2π·χ on closed meshes (discrete Gauss-Bonnet).
    internal static Arr<double> ComputeIntrinsicAngleDefects(MeshKernel.IntrinsicMesh imesh) {
        int n = imesh.VertexCount;
        double[] defect = new double[n];
        for (int v = 0; v < n; v++) defect[v] = 2.0 * Math.PI;
        foreach (int f in imesh.LiveFaceIndices()) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            int[] edges = imesh.EdgesOfFace(faceIdx: f);
            if (edges.Length != 3 || edges[0] < 0 || edges[1] < 0 || edges[2] < 0) continue;
            double lAB = imesh.EdgeAt(index: edges[0]).Length;
            double lBC = imesh.EdgeAt(index: edges[1]).Length;
            double lCA = imesh.EdgeAt(index: edges[2]).Length;
            defect[va] -= AngleFromLengths(opposite: lBC, adjacent1: lAB, adjacent2: lCA);
            defect[vb] -= AngleFromLengths(opposite: lCA, adjacent1: lAB, adjacent2: lBC);
            defect[vc] -= AngleFromLengths(opposite: lAB, adjacent1: lBC, adjacent2: lCA);
        }
        return new Arr<double>(defect);
    }
    private static double AngleFromLengths(double opposite, double adjacent1, double adjacent2) {
        double cosTheta = ((adjacent1 * adjacent1) + (adjacent2 * adjacent2) - (opposite * opposite)) / (2.0 * adjacent1 * adjacent2);
        return Math.Acos(d: Math.Min(val1: 1.0, val2: Math.Max(val1: -1.0, val2: cosTheta)));
    }

    // Crane-Desbrun-Schröder 2010 trivial connection 1-form on the INTRINSIC-Delaunay edge set.
    // Returns per-intrinsic-edge angle adjustment α_e such that ρ_ij ← ρ_ij + α_e produces a
    // connection with prescribed singularities at cone vertices. Algorithm: (1) build closed
    // primal 1-form u with target_v = 2π·k_v − K_v per vertex, distributed onto one incident
    // intrinsic edge; (2) Hodge-decompose via coexact solve L_cot · β = d^T · diag(★₁) · u;
    // (3) α_e = −(d₀β)_e per intrinsic edge. Closed mesh only; γ harmonic term (genus > 0)
    // deferred. The intrinsic edge index space matches LaplacianCache.IntrinsicMeshSnapshot, so
    // EnumerateConnectionEntries' α-lookup hits every flipped edge (no extrinsic mismatch).
    internal static Fin<Arr<double>> DistributeHolonomy(MeshSpace space, MeshKernel.IntrinsicMesh imesh, Seq<(int Vertex, double ConeIndex)> cones, Op key) {
        Arr<double> defects = ComputeIntrinsicAngleDefects(imesh: imesh);
        return from _ in ValidateGaussBonnet(mesh: space.Native, imesh: imesh, defects: defects, cones: cones, key: key)
               let u = BuildConePrimalOneForm(imesh: imesh, defects: defects, cones: cones)
               let star1 = ComputeIntrinsicStar1(imesh: imesh)
               let rhs = IntrinsicCoexactRhs(imesh: imesh, star1: star1, u: u)
               from beta in space.Cache.Cholesky.Bind(factor => factor.Solve(rhs: rhs, key: key))
               let dBeta = IntrinsicEdgeGradient(imesh: imesh, beta: beta)
               select NegatedCopy(values: dBeta);
    }
    private static Fin<Unit> ValidateGaussBonnet(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Arr<double> defects, Seq<(int Vertex, double ConeIndex)> cones, Op key) {
        Polyline[]? naked = mesh.GetNakedEdges();
        if (naked is null || naked.Length > 0) return Fin.Fail<Unit>(error: key.InvalidInput());
        double sumK = 0.0;
        for (int v = 0; v < defects.Count; v++) sumK += defects[index: v];
        double euler = sumK / (2.0 * Math.PI);
        double sumPrescribed = 0.0;
        for (int c = 0; c < cones.Count; c++) sumPrescribed += cones[index: c].ConeIndex;
        return Math.Abs(value: sumPrescribed - euler) > 1.0e-6
            ? Fin.Fail<Unit>(error: key.InvalidInput())
            : Fin.Succ(unit);
    }
    // Per-vertex target: 2π·k_v − K_v. Distribute onto one incident intrinsic edge (taken
    // canonically via IntrinsicMesh.FirstIncidentEdge). The d₀ sign convention is −1 at Lo,
    // +1 at Hi, so the dump sign is +target_v when v = Hi, −target_v when v = Lo.
    private static Arr<double> BuildConePrimalOneForm(MeshKernel.IntrinsicMesh imesh, Arr<double> defects, Seq<(int Vertex, double ConeIndex)> cones) {
        int n = imesh.VertexCount;
        int eCount = imesh.EdgeCount;
        double[] target = new double[n];
        for (int v = 0; v < n; v++) target[v] = -defects[index: v];
        for (int c = 0; c < cones.Count; c++) {
            (int v, double k) = cones[index: c];
            if (v >= 0 && v < n) target[v] += 2.0 * Math.PI * k;
        }
        double[] u = new double[eCount];
        for (int v = 0; v < n; v++) {
            if (Math.Abs(value: target[v]) < RhinoMath.ZeroTolerance) continue;
            int e = imesh.FirstIncidentEdge(vertexIdx: v);
            if (e < 0) continue;
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            double sign = v == edge.Hi ? 1.0 : -1.0;
            u[e] += target[v] * sign;
        }
        return new Arr<double>(u);
    }
    // Per-intrinsic-edge cotangent weight: Σ over incident faces of 0.5·cot(opposite angle),
    // computed from intrinsic edge lengths via law of cosines / Heron area. Always ≥ 0 on
    // intrinsic-Delaunay meshes. Shared by holonomy distribution (★₁) and connection-Laplacian
    // assembly (off-diagonal cotangent weight w for each intrinsic edge).
    internal static Arr<double> ComputeIntrinsicStar1(MeshKernel.IntrinsicMesh imesh) {
        int eCount = imesh.EdgeCount;
        double[] star1 = new double[eCount];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            star1[e] = 0.5 * (CotAtOppositeCorner(imesh: imesh, edge: edge, faceIdx: edge.Face0)
                            + CotAtOppositeCorner(imesh: imesh, edge: edge, faceIdx: edge.Face1));
        }
        return new Arr<double>(star1);
    }
    private static double CotAtOppositeCorner(MeshKernel.IntrinsicMesh imesh, MeshKernel.IntrinsicEdge edge, int faceIdx) {
        if (faceIdx < 0) return 0.0;
        (int A, int B, int C)? slot = imesh.Triangles[index: faceIdx];
        if (slot is null) return 0.0;
        (int va, int vb, int vc) = slot.Value;
        int oppV = (va != edge.Lo && va != edge.Hi) ? va
                 : (vb != edge.Lo && vb != edge.Hi) ? vb
                 : vc;
        int eOppLo = imesh.IndexOfEdge(lo: oppV, hi: edge.Lo);
        int eOppHi = imesh.IndexOfEdge(lo: oppV, hi: edge.Hi);
        if (eOppLo < 0 || eOppHi < 0) return 0.0;
        double lOppLo = imesh.EdgeAt(index: eOppLo).Length;
        double lOppHi = imesh.EdgeAt(index: eOppHi).Length;
        double area = imesh.AreaOfFace(faceIdx: faceIdx);
        return area < DegenerateTriangleArea
            ? 0.0
            : ((lOppLo * lOppLo) + (lOppHi * lOppHi) - (edge.Length * edge.Length)) / (4.0 * area);
    }
    // d_intrinsic^T · diag(★₁) · u: per-vertex contribution of incident intrinsic edges weighted
    // by the per-edge cotan weight and the primal 1-form value. Sign matches the d₀ convention
    // (−1 at Lo, +1 at Hi).
    private static Arr<double> IntrinsicCoexactRhs(MeshKernel.IntrinsicMesh imesh, Arr<double> star1, Arr<double> u) {
        int n = imesh.VertexCount;
        int eCount = imesh.EdgeCount;
        double[] rhs = new double[n];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            double weighted = star1[index: e] * u[index: e];
            rhs[edge.Lo] += -weighted;
            rhs[edge.Hi] += +weighted;
        }
        return new Arr<double>(rhs);
    }
    // d_intrinsic · β per intrinsic edge: gradient of the vertex 0-form β along each edge.
    private static Arr<double> IntrinsicEdgeGradient(MeshKernel.IntrinsicMesh imesh, Arr<double> beta) {
        int eCount = imesh.EdgeCount;
        double[] grad = new double[eCount];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            grad[e] = beta[index: edge.Hi] - beta[index: edge.Lo];
        }
        return new Arr<double>(grad);
    }
    private static Arr<double> NegatedCopy(Arr<double> values) {
        int n = values.Count;
        double[] negated = new double[n];
        for (int i = 0; i < n; i++) negated[i] = -values[index: i];
        return new Arr<double>(negated);
    }

    // --- [HEAT_SCAFFOLD] --------------------------------------------------------------------
    // Mass-weighted impulse at each source vertex; matches Crane heat-method convention so
    // (M + tL)u = δ gives u(source) ≈ 1 in the small-t limit.
    internal static Arr<double> BuildSourceDelta(int n, Seq<int> sources, Arr<double> mass) {
        double[] delta = new double[n];
        for (int s = 0; s < sources.Count; s++) delta[sources[index: s]] = mass[index: sources[index: s]];
        return new Arr<double>(delta);
    }
    // Triplets for L_cot with a hard 1e10 pin at each source. Callers admit non-empty sources
    // before assembly so a null-mode pin is never synthesized silently.
    internal static List<(int Row, int Col, double Value)> PoissonTriplets(SparseLaplacian L, Seq<int> sources) {
        List<(int Row, int Col, double Value)> result = [];
        for (int i = 0; i < L.Stiffness.Rows.Value; i++)
            for (int k = L.Stiffness.RowPtr[index: i]; k < L.Stiffness.RowPtr[index: i + 1]; k++)
                result.Add(item: (i, L.Stiffness.ColInd[index: k], L.Stiffness.Values[index: k]));
        for (int i = 0; i < sources.Count; i++) result.Add(item: (sources[index: i], sources[index: i], 1.0e10));
        return result;
    }
    // FEM gradient per triangle of a scalar field u: −∇u normalised to unit length.
    // BOUNDARY ADAPTER — Rhino MeshFace + Vertices direct access; Seq materialisation
    // doubles peak memory on dense meshes.
    internal static Vector3d[] ComputeTriangleGradients(Mesh mesh, Arr<double> u) {
        Vector3d[] gradients = new Vector3d[mesh.Faces.Count];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d n = Vector3d.CrossProduct(a: pb - pa, b: pc - pa); double twoArea = n.Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            Vector3d nUnit = n / twoArea;
            Vector3d ua = Vector3d.CrossProduct(a: nUnit, b: pc - pb) * u[index: face.A];
            Vector3d ub = Vector3d.CrossProduct(a: nUnit, b: pa - pc) * u[index: face.B];
            Vector3d uc = Vector3d.CrossProduct(a: nUnit, b: pb - pa) * u[index: face.C];
            Vector3d g = (ua + ub + uc) / twoArea;
            double len = g.Length;
            gradients[f] = len > RhinoMath.ZeroTolerance ? -g / len : Vector3d.Zero;
        }
        return gradients;
    }
    // Cotangent vertex divergence of a per-triangle tangent field. Standard Crane heat-method
    // ∇·X formula: ½ Σ_{f∋v} cot(α₁)·⟨e₁,X_f⟩ + cot(α₂)·⟨e₂,X_f⟩.
    internal static Arr<double> ComputeVertexDivergence(Mesh mesh, Vector3d[] gradients) {
        int n = mesh.Vertices.Count;
        double[] div = new double[n];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d ab = pb - pa; Vector3d bc = pc - pb; Vector3d ca = pa - pc;
            double twoArea = Vector3d.CrossProduct(a: ab, b: pc - pa).Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            double cotA = -ab * ca / twoArea;
            double cotB = ab * -bc / twoArea;
            double cotC = bc * ca / twoArea;
            Vector3d g = gradients[f];
            div[face.A] += 0.5 * ((cotB * (ab * g)) + (cotC * (-ca * g)));
            div[face.B] += 0.5 * ((cotC * (bc * g)) + (cotA * (-ab * g)));
            div[face.C] += 0.5 * ((cotA * (ca * g)) + (cotB * (-bc * g)));
        }
        return new Arr<double>(div);
    }

    // --- [DEC_ASSEMBLY] ---------------------------------------------------------------------
    internal static Fin<DiscreteCalculus> Build(MeshSpace space, Op key) =>
        from imesh in space.Cache.IntrinsicMeshSnapshot
        from laplacian in space.Cache.IntrinsicDelaunay
        from dec in AssembleDecOperators(imesh: imesh, mass: laplacian.MassLumped, key: key)
        select dec;

    private static Fin<DiscreteCalculus> AssembleDecOperators(MeshKernel.IntrinsicMesh imesh, Arr<double> mass, Op key) {
        int vertCount = imesh.VertexCount;
        int edgeCount = imesh.EdgeCount;
        int[] liveFaces = [.. imesh.LiveFaceIndices()];
        int faceCount = liveFaces.Length;
        List<(int Row, int Col, double Value)> d0 = new(capacity: 2 * edgeCount);
        List<(int Row, int Col, double Value)> d1 = new(capacity: 3 * faceCount);
        Arr<double> star1 = ComputeIntrinsicStar1(imesh: imesh);
        double[] star2 = new double[faceCount];
        for (int e = 0; e < edgeCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            d0.Add(item: (e, edge.Lo, -1.0));
            d0.Add(item: (e, edge.Hi, +1.0));
        }
        for (int row = 0; row < faceCount; row++) {
            int faceIdx = liveFaces[row];
            (int A, int B, int C)? slot = imesh.Triangles[index: faceIdx];
            if (slot is null) continue;
            (int a, int b, int c) = slot.Value;
            double area = imesh.AreaOfFace(faceIdx: faceIdx);
            if (area < DegenerateTriangleArea) continue;
            star2[row] = 1.0 / area;
            int[] edges = imesh.EdgesOfFace(faceIdx: faceIdx);
            if (edges.Length != 3 || edges[0] < 0 || edges[1] < 0 || edges[2] < 0) continue;
            d1.Add(item: (row, edges[0], EdgeOrientation(imesh: imesh, edgeIndex: edges[0], from: a, to: b)));
            d1.Add(item: (row, edges[1], EdgeOrientation(imesh: imesh, edgeIndex: edges[1], from: b, to: c)));
            d1.Add(item: (row, edges[2], EdgeOrientation(imesh: imesh, edgeIndex: edges[2], from: c, to: a)));
        }
        Dimension vDim = Dimension.Create(value: vertCount);
        Dimension eDim = Dimension.Create(value: edgeCount);
        Dimension fDim = Dimension.Create(value: faceCount);
        return mass.Count != vertCount
            || !mass.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0)
            || !BoundaryCompositionIsZero(d0: d0, d1: d1)
            ? Fin.Fail<DiscreteCalculus>(key.InvalidResult())
            : from D0 in SparseMatrix.FromTriplets(rows: eDim, cols: vDim, triplets: d0, key: key)
              from D1 in SparseMatrix.FromTriplets(rows: fDim, cols: eDim, triplets: d1, key: key)
              select new DiscreteCalculus(D0: D0, D1: D1, Star0: mass, Star1: star1, Star2: new Arr<double>(star2));
    }
    private static double EdgeOrientation(MeshKernel.IntrinsicMesh imesh, int edgeIndex, int from, int to) {
        MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
        return edge.Lo == from && edge.Hi == to ? 1.0 : -1.0;
    }
    private static bool BoundaryCompositionIsZero(List<(int Row, int Col, double Value)> d0, List<(int Row, int Col, double Value)> d1) {
        ILookup<int, (int Row, int Col, double Value)> d0ByEdge = d0.ToLookup(static row => row.Row);
        Dictionary<(int Face, int Vertex), double> composed = [];
        foreach ((int face, int edge, double fSign) in d1)
            foreach ((int d0Edge, int vertex, double eSign) in d0ByEdge[key: edge]) {
                (int Face, int Vertex) key = (face, vertex);
                composed[key] = composed.GetValueOrDefault(key: key) + (fSign * eSign);
            }
        return composed.Values.All(static value => Math.Abs(value: value) <= RhinoMath.SqrtEpsilon);
    }
}
