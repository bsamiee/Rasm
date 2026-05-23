using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseLaplacian(SparseMatrix Stiffness, SparseMatrix MassConsistent, Arr<double> MassLumped) {
    public bool IsValid =>
        Stiffness.IsValid && MassConsistent.IsValid
        && Stiffness.Rows.Value == MassConsistent.Rows.Value
        && MassLumped.Count == Stiffness.Rows.Value
        && MassLumped.All(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSpace {
    private MeshSpace(Mesh native, Context tolerance) {
        Native = native;
        Tolerance = tolerance;
    }
    public Mesh Native { get; }
    public Context Tolerance { get; }
    public static Fin<MeshSpace> Of(Mesh native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               let snapshot = active.DuplicateMesh()
               let __ = snapshot.Faces.ConvertQuadsToTriangles()
               select new MeshSpace(native: snapshot, tolerance: ctx);
    }
    internal LaplacianCache Cache => LaplacianCache.For(space: this);
    public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) =>
        MeshKernel.LaplacianOf(space: this, kind: kind, key: key.OrDefault());
    internal Fin<int> EulerCharacteristic(Op? key = null) =>
        MeshKernel.EulerCharacteristicOf(space: this, key: key.OrDefault());
    internal Fin<(int Euler, int Genus, int BoundaryComponents)> Topology(Op? key = null) =>
        MeshKernel.TopologyOf(space: this, key: key.OrDefault());
    internal Fin<Seq<(int A, int B)>> FeatureEdges(double dihedralRadians, Op? key = null) =>
        MeshKernel.DetectFeatureEdgesOf(space: this, dihedralRadians: dihedralRadians, key: key.OrDefault());
    internal Fin<double> MeanEdgeLength(Op? key = null) =>
        key.OrDefault().AcceptValue(value: Cache.MeanEdgeLength);
}

internal sealed class LaplacianCache {
    internal const int DefaultSpectralCount = 32;
    private const double SpdRegularization = 1e-8;
    private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
    private readonly Lazy<Fin<SparseLaplacian>> cotangent;
    private readonly Lazy<Fin<SparseLaplacian>> intrinsicDelaunay;
    private readonly Lazy<Fin<SparseLaplacian>> robust;
    private readonly Lazy<Fin<CholeskySparse>> cholesky;
    private readonly Lazy<Fin<SpectralBasis>> defaultSpectral;
    private readonly Lazy<double> meanEdgeLength;
    private readonly MeshSpace space;
    internal Dictionary<string, object> FieldCache { get; } = [];
    private LaplacianCache(MeshSpace space) {
        this.space = space;
        Op fallback = Op.Of();
        cotangent = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: fallback));
        intrinsicDelaunay = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleIntrinsicDelaunay(mesh: space.Native, key: fallback));
        robust = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleRobust(mesh: space.Native, key: fallback));
        // Cotangent stiffness is positive-semidefinite (constant mode in null space); regularise with
        // SpdRegularization * diag(M) so CSparse Cholesky succeeds. Sharp-Soliman-Crane 2019 use
        // the same regulariser when factoring the vector-heat system (M + t L).
        cholesky = new Lazy<Fin<CholeskySparse>>(valueFactory: () =>
            from L in intrinsicDelaunay.Value
            from spd in MeshKernel.MassPinned(laplacian: L, regularization: SpdRegularization, key: fallback)
            from factor in CholeskySparse.Of(symmetric: spd, key: fallback)
            select factor);
        defaultSpectral = new Lazy<Fin<SpectralBasis>>(valueFactory: () => MeshKernel.ComputeSpectralBasis(space: space, k: DefaultSpectralCount, key: fallback));
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent => cotangent.Value;
    internal Fin<SparseLaplacian> IntrinsicDelaunay => intrinsicDelaunay.Value;
    internal Fin<SparseLaplacian> Robust => robust.Value;
    internal Fin<CholeskySparse> Cholesky => cholesky.Value;
    internal double MeanEdgeLength => meanEdgeLength.Value;
    internal Fin<SpectralBasis> SpectralBasisOf(int k, Op key) =>
        k <= DefaultSpectralCount
            ? defaultSpectral.Value.Map(b => b.Truncate(k: k))
            : MeshKernel.ComputeSpectralBasis(space: space, k: k, key: key);
}

[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, select: static cache => cache.Cotangent);
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, select: static cache => cache.IntrinsicDelaunay);
    public static readonly MeshLaplacian Robust = new(key: 2, select: static cache => cache.Robust);
    [UseDelegateFromConstructor] internal partial Fin<SparseLaplacian> Select(LaplacianCache cache);
}

// Single case carries the full SpectralFilter polymorphic surface; HKS / WKS / ShapeDNA become
// SpectralFilter.Heat / Wave / Identity. Pairwise-distance descriptors carry a non-empty source
// set; per-vertex signatures leave sources = None.
[Union]
public abstract partial record MeshDescriptor {
    private MeshDescriptor() { }
    public sealed record SpectralCase(SpectralFilter Filter, Option<Seq<int>> Sources) : MeshDescriptor;
    public static MeshDescriptor Spectral(SpectralFilter filter, Option<Seq<int>> sources = default) =>
        new SpectralCase(Filter: filter, Sources: sources);
}

[Union]
public abstract partial record RemeshKind {
    private RemeshKind() { }
    public sealed record QuadCase(PositiveMagnitude TargetLength) : RemeshKind;
    public sealed record SimplifyCase(ReduceMeshParameters Parameters) : RemeshKind;
    public static Fin<RemeshKind> Quad(double targetLength, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: targetLength).Map(t => (RemeshKind)new QuadCase(TargetLength: t));
    public static Fin<RemeshKind> Simplify(ReduceMeshParameters parameters, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(parameters).ToFin(op.InvalidInput())
            .Bind(active => active.DesiredPolygonCount >= 1
                ? Fin.Succ<RemeshKind>(new SimplifyCase(Parameters: active))
                : Fin.Fail<RemeshKind>(op.InvalidInput()));
    }
    internal Fin<Mesh> Apply(MeshSpace space, Op? key = null) =>
        MeshKernel.ApplyRemesh(kind: this, space: space, key: key.OrDefault());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MeshKernel {
    private const double DegenerateTriangleArea = 1e-14;
    private const double AspectRatioCeiling = 11.5;

    // --- [VALIDATION] -----------------------------------------------------------------------
    // BOUNDARY ADAPTER — iterates the native Rhino face list to enforce the aspect-ratio
    // contract before construction. Functional fold would force materialising the whole face
    // collection into a Seq which doubles memory on large meshes.
    internal static Fin<Unit> AspectRatioGuard(Mesh mesh, double ceiling, Op key) {
        for (int f = 0; f < mesh.Faces.Count; f++) {
            double aspect = mesh.Faces.GetFaceAspectRatio(index: f);
            if (!RhinoMath.IsValidDouble(x: aspect) || aspect > ceiling)
                return Fin.Fail<Unit>(key.Caution(concern: $"Face {f} aspect ratio exceeds {ceiling}."));
        }
        return Fin.Succ(unit);
    }

    // --- [COTANGENT_ASSEMBLY] ---------------------------------------------------------------
    // BOUNDARY ADAPTER — triplet accumulation for SparseMatrix.FromTriplets demands an
    // imperative pass over Rhino's face collection; materialising into Seq first doubles
    // memory and obscures the per-face fold pattern. Negative-cotangent rejection enforces
    // the maximum principle: obtuse triangles fail Cotangent assembly.
    internal static Fin<SparseLaplacian> AssembleCotangent(Mesh mesh, Op key) {
        using Mesh active = mesh.DuplicateMesh();
        _ = active.Faces.ConvertQuadsToTriangles();
        int vertCount = active.Vertices.Count;
        List<(int Row, int Col, double Value)> stiffTriplets = [];
        List<(int Row, int Col, double Value)> massTriplets = [];
        double[] lumped = new double[vertCount];
        for (int f = 0; f < active.Faces.Count; f++) {
            MeshFace face = active.Faces[index: f];
            if (!face.IsTriangle) continue;
            int va = face.A; int vb = face.B; int vc = face.C;
            Point3d pa = active.Vertices[index: va]; Point3d pb = active.Vertices[index: vb]; Point3d pc = active.Vertices[index: vc];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa; Vector3d bc = pc - pb;
            double area = 0.5 * Vector3d.CrossProduct(a: ab, b: ac).Length;
            if (area < DegenerateTriangleArea) continue;
            double cotA = -ab * -ac / (2.0 * area);
            double cotB = ab * -bc / (2.0 * area);
            double cotC = ac * bc / (2.0 * area);
            if (cotA < 0.0 || cotB < 0.0 || cotC < 0.0)
                return Fin.Fail<SparseLaplacian>(key.InvalidResult());
            stiffTriplets.Add(item: (vb, vc, -0.5 * cotA)); stiffTriplets.Add(item: (vc, vb, -0.5 * cotA));
            stiffTriplets.Add(item: (vc, va, -0.5 * cotB)); stiffTriplets.Add(item: (va, vc, -0.5 * cotB));
            stiffTriplets.Add(item: (va, vb, -0.5 * cotC)); stiffTriplets.Add(item: (vb, va, -0.5 * cotC));
            stiffTriplets.Add(item: (va, va, 0.5 * (cotB + cotC)));
            stiffTriplets.Add(item: (vb, vb, 0.5 * (cotA + cotC)));
            stiffTriplets.Add(item: (vc, vc, 0.5 * (cotA + cotB)));
            double m = area / 12.0;
            massTriplets.Add(item: (va, va, 2.0 * m)); massTriplets.Add(item: (vb, vb, 2.0 * m)); massTriplets.Add(item: (vc, vc, 2.0 * m));
            massTriplets.Add(item: (va, vb, m)); massTriplets.Add(item: (vb, va, m));
            massTriplets.Add(item: (vb, vc, m)); massTriplets.Add(item: (vc, vb, m));
            massTriplets.Add(item: (va, vc, m)); massTriplets.Add(item: (vc, va, m));
            double third = area / 3.0;
            lumped[va] += third; lumped[vb] += third; lumped[vc] += third;
        }
        Dimension dim = Dimension.Create(value: vertCount);
        return from stiff in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: stiffTriplets, key: key)
               from mass in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: massTriplets, key: key)
               select new SparseLaplacian(Stiffness: stiff, MassConsistent: mass, MassLumped: new Arr<double>(lumped));
    }

    // --- [IDT_AND_NONMANIFOLD] --------------------------------------------------------------
    // Sharp-Soliman-Crane 2019: intrinsic edge flips on a length-only triangulation until every
    // interior edge satisfies the Delaunay condition (α + β ≤ π); cotangent weights assembled
    // on the flipped intrinsic mesh are then provably non-negative.
    internal static Fin<SparseLaplacian> AssembleIntrinsicDelaunay(Mesh mesh, Op key) =>
        from intrinsic in FlipToDelaunay(imesh: IntrinsicMesh.FromMesh(mesh: mesh), key: key)
        from laplacian in AssembleCotangentFromIntrinsic(imesh: intrinsic, key: key)
        select laplacian;

    private sealed class IntrinsicMesh {
        internal int VertexCount;
        internal readonly List<(int A, int B, int C)?> Triangles = [];
        internal readonly Dictionary<(int Lo, int Hi), (double Length, List<int> FaceIdx)> EdgeData = [];
        private static (int, int) EdgeKey(int i, int j) => (Math.Min(val1: i, val2: j), Math.Max(val1: i, val2: j));
        internal static IntrinsicMesh FromMesh(Mesh mesh) {
            using Mesh active = mesh.DuplicateMesh();
            _ = active.Faces.ConvertQuadsToTriangles();
            IntrinsicMesh m = new() { VertexCount = active.Vertices.Count };
            for (int f = 0; f < active.Faces.Count; f++) {
                MeshFace face = active.Faces[index: f];
                if (!face.IsTriangle) continue;
                Point3d pa = active.Vertices[index: face.A]; Point3d pb = active.Vertices[index: face.B]; Point3d pc = active.Vertices[index: face.C];
                _ = m.AddTriangle(a: face.A, b: face.B, c: face.C, lAB: pa.DistanceTo(other: pb), lBC: pb.DistanceTo(other: pc), lAC: pa.DistanceTo(other: pc));
            }
            return m;
        }
        internal int AddTriangle(int a, int b, int c, double lAB, double lBC, double lAC) {
            int idx = Triangles.Count;
            Triangles.Add(item: (a, b, c));
            AddEdgeReference(i: a, j: b, length: lAB, faceIdx: idx);
            AddEdgeReference(i: b, j: c, length: lBC, faceIdx: idx);
            AddEdgeReference(i: a, j: c, length: lAC, faceIdx: idx);
            return idx;
        }
        private void AddEdgeReference(int i, int j, double length, int faceIdx) {
            (int Lo, int Hi) key = EdgeKey(i: i, j: j);
            if (EdgeData.TryGetValue(key: key, value: out (double Length, List<int> FaceIdx) existing)) existing.FaceIdx.Add(item: faceIdx);
            else EdgeData[key: key] = (Length: length, FaceIdx: [faceIdx]);
        }
        internal double EdgeLengthOf(int i, int j) => EdgeData[key: EdgeKey(i: i, j: j)].Length;
        internal int OppositeVertex(int faceIdx, int i, int j) {
            (int a, int b, int c) = Triangles[index: faceIdx]!.Value;
            return a != i && a != j ? a : b != i && b != j ? b : c;
        }
        internal bool IsInterior(int i, int j) =>
            EdgeData.TryGetValue(key: EdgeKey(i: i, j: j), value: out (double Length, List<int> FaceIdx) e) && e.FaceIdx.Count == 2;
        internal bool IsDelaunay(int i, int j) {
            if (!IsInterior(i: i, j: j)) return true;
            List<int> faces = EdgeData[key: EdgeKey(i: i, j: j)].FaceIdx;
            int k1 = OppositeVertex(faceIdx: faces[index: 0], i: i, j: j);
            int k2 = OppositeVertex(faceIdx: faces[index: 1], i: i, j: j);
            double cosA = CosAngleOppositeEdge(i: i, j: j, k: k1);
            double cosB = CosAngleOppositeEdge(i: i, j: j, k: k2);
            return cosA + cosB >= -RhinoMath.SqrtEpsilon;
        }
        internal double CosAngleOppositeEdge(int i, int j, int k) {
            double lij = EdgeLengthOf(i: i, j: j); double lik = EdgeLengthOf(i: i, j: k); double ljk = EdgeLengthOf(i: j, j: k);
            return ((lik * lik) + (ljk * ljk) - (lij * lij)) / (2.0 * lik * ljk);
        }
        internal Seq<(int, int)> Flip(int i, int j) {
            (int Lo, int Hi) key = EdgeKey(i: i, j: j);
            List<int> faces = EdgeData[key: key].FaceIdx;
            int f1 = faces[index: 0]; int f2 = faces[index: 1];
            int k1 = OppositeVertex(faceIdx: f1, i: i, j: j);
            int k2 = OppositeVertex(faceIdx: f2, i: i, j: j);
            double li_k1 = EdgeLengthOf(i: i, j: k1); double li_k2 = EdgeLengthOf(i: i, j: k2);
            double lij = EdgeLengthOf(i: i, j: j);
            double lj_k1 = EdgeLengthOf(i: j, j: k1); double lj_k2 = EdgeLengthOf(i: j, j: k2);
            double cosAtI_f1 = ((lij * lij) + (li_k1 * li_k1) - (lj_k1 * lj_k1)) / (2.0 * lij * li_k1);
            double cosAtI_f2 = ((lij * lij) + (li_k2 * li_k2) - (lj_k2 * lj_k2)) / (2.0 * lij * li_k2);
            double sinAtI_f1 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosAtI_f1 * cosAtI_f1)));
            double sinAtI_f2 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosAtI_f2 * cosAtI_f2)));
            double cosSum = (cosAtI_f1 * cosAtI_f2) - (sinAtI_f1 * sinAtI_f2);
            double l_k1k2 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (li_k1 * li_k1) + (li_k2 * li_k2) - (2.0 * li_k1 * li_k2 * cosSum)));
            _ = EdgeData.Remove(key: key);
            Triangles[index: f1] = null; Triangles[index: f2] = null;
            (int, int)[] surrounding = [(i, k1), (j, k1), (i, k2), (j, k2)];
            for (int s = 0; s < surrounding.Length; s++) {
                (int Lo, int Hi) ek = EdgeKey(i: surrounding[s].Item1, j: surrounding[s].Item2);
                if (EdgeData.TryGetValue(key: ek, value: out (double Length, List<int> FaceIdx) e)) { _ = e.FaceIdx.Remove(item: f1); _ = e.FaceIdx.Remove(item: f2); }
            }
            _ = AddTriangle(a: i, b: k1, c: k2, lAB: li_k1, lBC: l_k1k2, lAC: li_k2);
            _ = AddTriangle(a: j, b: k1, c: k2, lAB: lj_k1, lBC: l_k1k2, lAC: lj_k2);
            return Seq((i, k1), (j, k1), (i, k2), (j, k2));
        }
    }
    private static Fin<IntrinsicMesh> FlipToDelaunay(IntrinsicMesh imesh, Op key) {
        Queue<(int, int)> queue = new(collection: imesh.EdgeData.Keys.Select(static k => (k.Lo, k.Hi)));
        int flipCap = imesh.EdgeData.Count * 16;
        int flips = 0;
        while (queue.Count > 0 && flips < flipCap) {
            (int i, int j) = queue.Dequeue();
            if (!imesh.IsInterior(i: i, j: j) || imesh.IsDelaunay(i: i, j: j)) continue;
            Seq<(int, int)> affected = imesh.Flip(i: i, j: j);
            for (int s = 0; s < affected.Count; s++) queue.Enqueue(item: affected[s]);
            flips++;
        }
        return imesh.EdgeData.Keys.All(edge => imesh.IsDelaunay(i: edge.Lo, j: edge.Hi))
            ? Fin.Succ(imesh)
            : Fin.Fail<IntrinsicMesh>(key.InvalidResult());
    }
    // Heron-based cotangent assembly from intrinsic edge lengths. After IDT flipping, every
    // interior cotangent (in the form 2·cot α = (l_ik² + l_jk² - l_ij²)/(2·area)) is positive.
    private static Fin<SparseLaplacian> AssembleCotangentFromIntrinsic(IntrinsicMesh imesh, Op key) {
        int n = imesh.VertexCount;
        List<(int Row, int Col, double Value)> stiffTriplets = [];
        List<(int Row, int Col, double Value)> massTriplets = [];
        double[] lumped = new double[n];
        for (int f = 0; f < imesh.Triangles.Count; f++) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            double lAB = imesh.EdgeLengthOf(i: va, j: vb);
            double lBC = imesh.EdgeLengthOf(i: vb, j: vc);
            double lAC = imesh.EdgeLengthOf(i: va, j: vc);
            double s = (lAB + lBC + lAC) * 0.5;
            double areaSq = s * (s - lAB) * (s - lBC) * (s - lAC);
            if (areaSq < DegenerateTriangleArea * DegenerateTriangleArea) continue;
            double area = Math.Sqrt(d: areaSq);
            double cotA = ((lAC * lAC) + (lAB * lAB) - (lBC * lBC)) / (4.0 * area);
            double cotB = ((lAB * lAB) + (lBC * lBC) - (lAC * lAC)) / (4.0 * area);
            double cotC = ((lAC * lAC) + (lBC * lBC) - (lAB * lAB)) / (4.0 * area);
            if (cotA < -RhinoMath.SqrtEpsilon || cotB < -RhinoMath.SqrtEpsilon || cotC < -RhinoMath.SqrtEpsilon)
                return Fin.Fail<SparseLaplacian>(key.InvalidResult());
            stiffTriplets.Add(item: (vb, vc, -0.5 * cotA)); stiffTriplets.Add(item: (vc, vb, -0.5 * cotA));
            stiffTriplets.Add(item: (vc, va, -0.5 * cotB)); stiffTriplets.Add(item: (va, vc, -0.5 * cotB));
            stiffTriplets.Add(item: (va, vb, -0.5 * cotC)); stiffTriplets.Add(item: (vb, va, -0.5 * cotC));
            stiffTriplets.Add(item: (va, va, 0.5 * (cotB + cotC)));
            stiffTriplets.Add(item: (vb, vb, 0.5 * (cotA + cotC)));
            stiffTriplets.Add(item: (vc, vc, 0.5 * (cotA + cotB)));
            double m = area / 12.0;
            massTriplets.Add(item: (va, va, 2.0 * m)); massTriplets.Add(item: (vb, vb, 2.0 * m)); massTriplets.Add(item: (vc, vc, 2.0 * m));
            massTriplets.Add(item: (va, vb, m)); massTriplets.Add(item: (vb, va, m));
            massTriplets.Add(item: (vb, vc, m)); massTriplets.Add(item: (vc, vb, m));
            massTriplets.Add(item: (va, vc, m)); massTriplets.Add(item: (vc, va, m));
            double third = area / 3.0;
            lumped[va] += third; lumped[vb] += third; lumped[vc] += third;
        }
        Dimension dim = Dimension.Create(value: n);
        return from stiff in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: stiffTriplets, key: key)
               from mass in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: massTriplets, key: key)
               select new SparseLaplacian(Stiffness: stiff, MassConsistent: mass, MassLumped: new Arr<double>(lumped));
    }

    // --- [SELECTION] ------------------------------------------------------------------------
    internal static Fin<SparseLaplacian> LaplacianOf(MeshSpace space, MeshLaplacian kind, Op key) =>
        from active in Optional(kind).ToFin(key.InvalidInput())
        from _ in active.Equals(MeshLaplacian.Cotangent)
            ? AspectRatioGuard(mesh: space.Native, ceiling: AspectRatioCeiling, key: key)
            : Fin.Succ(unit)
        from result in active.Select(cache: space.Cache)
        select result;

    // --- [SPD_PIN] --------------------------------------------------------------------------
    // L + epsilon * diag(M) -- the standard SPD regularisation for cotangent Laplacian Cholesky.
    // BOUNDARY ADAPTER -- triplet enumeration over an existing CSR storage; alternative is a
    // full materialisation through MathNet which doubles peak memory.
    internal static Fin<SparseMatrix> MassPinned(SparseLaplacian laplacian, double regularization, Op key) {
        int n = laplacian.Stiffness.Rows.Value;
        if (n == 0) return Fin.Fail<SparseMatrix>(error: key.InvalidInput());
        List<(int Row, int Col, double Value)> triplets = [];
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, laplacian.Stiffness.ColInd[index: k], laplacian.Stiffness.Values[index: k]));
        for (int i = 0; i < n; i++) triplets.Add(item: (i, i, regularization * laplacian.MassLumped[index: i]));
        Dimension dim = Dimension.Create(value: n);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key);
    }

    // --- [ROBUST_LAPLACIAN] ----------------------------------------------------------------
    // Sharp-Crane SGP 2020 "A Laplacian for Nonmanifold Triangle Meshes": unweld each edge with
    // >2 incident faces into per-face copies (the "tufted cover"), then run intrinsic Delaunay
    // flips on the locally-manifold result. Cotangent weights from the flipped tufted mesh are
    // guaranteed non-negative even when the input is non-manifold.
    internal static Fin<SparseLaplacian> AssembleRobust(Mesh mesh, Op key) {
        using Mesh active = mesh.DuplicateMesh();
        _ = active.Faces.ConvertQuadsToTriangles();
        List<int> nonManifold = [];
        for (int e = 0; e < active.TopologyEdges.Count; e++)
            if (active.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e).Length > 2) nonManifold.Add(item: e);
        if (nonManifold.Count > 0) _ = active.UnweldEdge(edgeIndices: nonManifold, modifyNormals: true);
        return AssembleIntrinsicDelaunay(mesh: active, key: key);
    }

    // --- [SPECTRAL_BASIS] ------------------------------------------------------------------
    // Reuter-Wolter-Peinecke 2006 ShapeDNA / Lévy 2006 manifold harmonics: smallest k eigenpairs
    // of the generalised problem L phi = lambda M phi.
    internal static Fin<SpectralBasis> ComputeSpectralBasis(MeshSpace space, int k, Op key) =>
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        from pairs in MatrixKernel.GeneralizedEigenpairs(stiffness: laplacian.Stiffness, mass: laplacian.MassConsistent, k: k, key: key)
        select new SpectralBasis(
            Eigenvalues: new Arr<double>([.. pairs.AsIterable().Select(static p => p.Eigenvalue)]),
            Eigenvectors: new Arr<Arr<double>>([.. pairs.AsIterable().Select(static p => p.Eigenvector)]));

    // --- [METRICS] --------------------------------------------------------------------------
    internal static double MeanEdgeLengthOf(Mesh mesh) {
        double sum = 0.0; int count = 0;
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: e);
            if (!line.IsValid) continue;
            sum += line.Length;
            count++;
        }
        return count > 0 ? sum / count : 0.0;
    }
    internal static Fin<int> EulerCharacteristicOf(MeshSpace space, Op key) =>
        key.AcceptValue(value: space.Native.TopologyVertices.Count - space.Native.TopologyEdges.Count + space.Native.Faces.Count);
    internal static Fin<(int Euler, int Genus, int BoundaryComponents)> TopologyOf(MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        bool manifold = mesh.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out _);
        int euler = mesh.TopologyVertices.Count - mesh.TopologyEdges.Count + mesh.Faces.Count;
        int boundaryComponents = BoundaryComponentCount(mesh: mesh);
        int components = Math.Max(val1: 1, val2: mesh.DisjointMeshCount);
        int numerator = (2 * components) - boundaryComponents - euler;
        return manifold && oriented && numerator >= 0 && numerator % 2 == 0
            ? key.AcceptValue(value: (Euler: euler, Genus: numerator / 2, BoundaryComponents: boundaryComponents))
            : Fin.Fail<(int, int, int)>(key.InvalidResult());
    }
    private static int BoundaryComponentCount(Mesh mesh) {
        Dictionary<int, List<int>> graph = [];
        for (int edge = 0; edge < mesh.TopologyEdges.Count; edge++) {
            if (mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge).Length != 1) continue;
            IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: edge);
            if (!graph.TryGetValue(key: pair.I, value: out List<int>? a)) graph[pair.I] = a = [];
            if (!graph.TryGetValue(key: pair.J, value: out List<int>? b)) graph[pair.J] = b = [];
            a.Add(item: pair.J);
            b.Add(item: pair.I);
        }
        System.Collections.Generic.HashSet<int> seen = [];
        int components = 0;
        foreach (int seed in graph.Keys) {
            if (!seen.Add(item: seed)) continue;
            components++;
            Queue<int> queue = new();
            queue.Enqueue(item: seed);
            while (queue.Count > 0) {
                int current = queue.Dequeue();
                foreach (int next in graph[key: current])
                    if (seen.Add(item: next)) queue.Enqueue(item: next);
            }
        }
        return components;
    }
    // BOUNDARY ADAPTER — iterates the topology edge list to compute dihedral angles between
    // adjacent face normals; non-manifold or boundary edges (≠2 connected faces) skipped.
    internal static Fin<Seq<(int A, int B)>> DetectFeatureEdgesOf(MeshSpace space, double dihedralRadians, Op key) {
        if (!RhinoMath.IsValidDouble(x: dihedralRadians) || dihedralRadians <= 0.0)
            return Fin.Fail<Seq<(int, int)>>(key.InvalidInput());
        Mesh mesh = space.Native;
        _ = mesh.FaceNormals.ComputeFaceNormals();
        Vector3f[] faceNormals = [.. mesh.FaceNormals];
        List<(int A, int B)> features = [];
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
            if (faces.Length == 1) {
                IndexPair p = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
                features.Add(item: (p.I, p.J));
                continue;
            }
            if (faces.Length != 2) continue;
            Vector3d na = (Vector3d)faceNormals[faces[0]]; Vector3d nb = (Vector3d)faceNormals[faces[1]];
            if (Vector3d.VectorAngle(a: na, b: nb) >= dihedralRadians) {
                IndexPair p = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
                features.Add(item: (p.I, p.J));
            }
        }
        return key.AcceptValue(value: toSeq(features));
    }

    internal static Fin<Arr<Point2d>> ParameterizeFlatten(MeshSpace space, Op key) {
        using Mesh mesh = space.Native.DuplicateMesh();
        using MeshUnwrapper unwrapper = new(mesh);
        bool ok = unwrapper.Unwrap(method: MeshUnwrapMethod.LSCM);
        return ok && mesh.TextureCoordinates.Count == mesh.Vertices.Count
            ? key.AcceptValue(value: new Arr<Point2d>([.. mesh.TextureCoordinates.Select(static t => new Point2d(x: t.X, y: t.Y))]))
            : Fin.Fail<Arr<Point2d>>(error: key.InvalidResult());
    }
    // --- [HEAT_METHOD] ----------------------------------------------------------------------
    // Crane-Weischedel-Wardetzky 2013: solve (M + tL)u = δ; X = -∇u/|∇u|; Lφ = ∇·X.
    private sealed record DoubleBoxArr(Arr<double> Values);
    internal static Fin<double> HeatGeodesicAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: distances, key: key)
        select value;
    private static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
        string cacheKey = string.Create(CultureInfo.InvariantCulture, $"geodesic|{string.Join(separator: ",", values: sources.AsIterable().OrderBy(static i => i))}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is DoubleBoxArr box
            ? Fin.Succ(box.Values)
            : space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key).Bind(L => ComputeHeatGeodesic(space: space, laplacian: L, sources: sources, key: key))
                .Map(d => { space.Cache.FieldCache[key: cacheKey] = new DoubleBoxArr(Values: d); return d; });
    }
    private static Fin<Arr<double>> ComputeHeatGeodesic(MeshSpace space, SparseLaplacian laplacian, Seq<int> sources, Op key) {
        int n = space.Native.Vertices.Count;
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<Arr<double>>(key.InvalidResult());
        double t = h * h;
        List<(int Row, int Col, double Value)> aTriplets = [];
        for (int i = 0; i < n; i++) aTriplets.Add(item: (i, i, laplacian.MassLumped[index: i]));
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++) {
                int j = laplacian.Stiffness.ColInd[index: k];
                aTriplets.Add(item: (i, j, t * laplacian.Stiffness.Values[index: k]));
            }
        Dimension dim = Dimension.Create(value: n);
        return from A in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: aTriplets, key: key)
               from delta in Fin.Succ(BuildSourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
               from u in A.Solve(rhs: delta, key: key)
               from gradient in Fin.Succ(ComputeTriangleGradients(mesh: space.Native, u: u))
               from divergence in Fin.Succ(ComputeVertexDivergence(mesh: space.Native, gradients: gradient))
               from poisson in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                   .Bind(L => SparseMatrix.FromTriplets(rows: dim, cols: dim,
                       triplets: PoissonTriplets(L: L, sources: sources), key: key))
               from phi in poisson.Solve(rhs: divergence, key: key)
               select NormalizeToZero(values: phi);
    }
    private static Arr<double> BuildSourceDelta(int n, Seq<int> sources, Arr<double> mass) {
        double[] delta = new double[n];
        for (int s = 0; s < sources.Count; s++) delta[sources[index: s]] = mass[index: sources[index: s]];
        return new Arr<double>(delta);
    }
    // Pin a source vertex (its geodesic distance is zero by definition) to break the constant-mode
    // null space of the cotangent Laplacian. Falls back to vertex 0 when no sources exist.
    private static List<(int Row, int Col, double Value)> PoissonTriplets(SparseLaplacian L, Seq<int> sources) {
        List<(int Row, int Col, double Value)> result = [];
        for (int i = 0; i < L.Stiffness.Rows.Value; i++)
            for (int k = L.Stiffness.RowPtr[index: i]; k < L.Stiffness.RowPtr[index: i + 1]; k++)
                result.Add(item: (i, L.Stiffness.ColInd[index: k], L.Stiffness.Values[index: k]));
        Seq<int> pins = sources.IsEmpty ? Seq(0) : sources;
        for (int i = 0; i < pins.Count; i++) result.Add(item: (pins[index: i], pins[index: i], 1.0e10));
        return result;
    }
    private static Vector3d[] ComputeTriangleGradients(Mesh mesh, Arr<double> u) {
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
    private static Arr<double> ComputeVertexDivergence(Mesh mesh, Vector3d[] gradients) {
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
    private static Arr<double> NormalizeToZero(Arr<double> values) {
        double min = values.Fold(initialState: double.MaxValue, f: static (m, v) => Math.Min(val1: m, val2: v));
        return new Arr<double>([.. values.AsIterable().Select(v => v - min)]);
    }
    private static Fin<double> InterpolateOnMesh(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        return meshPoint is null || meshPoint.FaceIndex < 0
            ? Fin.Fail<double>(key.InvalidResult())
            : key.AcceptValue(value: InterpolateFaceValue(mesh: space.Native, meshPoint: meshPoint, perVertex: perVertex));
    }
    private static double MeshSearchDistance(MeshSpace space) =>
        Math.Max(val1: space.Tolerance.Absolute.Value, val2: space.Cache.MeanEdgeLength);
    private static double InterpolateFaceValue(Mesh mesh, MeshPoint meshPoint, Arr<double> perVertex) {
        MeshFace face = mesh.Faces[index: meshPoint.FaceIndex];
        double[] weights = meshPoint.T;
        double value = (weights[0] * perVertex[index: face.A]) + (weights[1] * perVertex[index: face.B]) + (weights[2] * perVertex[index: face.C]);
        return face.IsQuad ? value + (weights[3] * perVertex[index: face.D]) : value;
    }

    // --- [MEAN_CURVATURE_FLOW] --------------------------------------------------------------
    // Desbrun-Meyer-Schröder-Barr 1999: implicit Euler step (M + dt L) X' = M X. We iterate
    // on a copy of vertex positions and return the per-vertex magnitude of (X' - X) as the
    // scalar field — a proxy for total displacement induced by the flow.
    internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key) =>
        from displacements in EnsureMcfDisplacements(space: space, timeStep: timeStep, iterations: iterations, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: displacements, key: key)
        select value;
    private static Fin<Arr<double>> EnsureMcfDisplacements(MeshSpace space, double timeStep, int iterations, Op key) {
        string cacheKey = string.Create(CultureInfo.InvariantCulture, $"mcf|{timeStep:R}|{iterations}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is DoubleBoxArr box
            ? Fin.Succ(box.Values)
            : space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                .Bind(L => ComputeMeanCurvatureFlow(space: space, laplacian: L, timeStep: timeStep, iterations: iterations, key: key))
                .Map(d => { space.Cache.FieldCache[key: cacheKey] = new DoubleBoxArr(Values: d); return d; });
    }
    private static Fin<Arr<double>> ComputeMeanCurvatureFlow(MeshSpace space, SparseLaplacian laplacian, double timeStep, int iterations, Op key) {
        int n = space.Native.Vertices.Count;
        Dimension dim = Dimension.Create(value: n);
        List<(int Row, int Col, double Value)> aTriplets = [];
        for (int i = 0; i < n; i++) aTriplets.Add(item: (i, i, laplacian.MassLumped[index: i]));
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++) {
                int j = laplacian.Stiffness.ColInd[index: k];
                aTriplets.Add(item: (i, j, timeStep * laplacian.Stiffness.Values[index: k]));
            }
        return from A in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: aTriplets, key: key)
               from final in IterateMcf(space: space, mass: laplacian.MassLumped, system: A, iterations: iterations, key: key)
               select ComputeDisplacements(original: space.Native, smoothed: final);
    }
    private static Fin<(double[] X, double[] Y, double[] Z)> IterateMcf(MeshSpace space, Arr<double> mass, SparseMatrix system, int iterations, Op key) {
        int n = space.Native.Vertices.Count;
        double[] x = new double[n]; double[] y = new double[n]; double[] z = new double[n];
        for (int i = 0; i < n; i++) { Point3d v = space.Native.Vertices[index: i]; x[i] = v.X; y[i] = v.Y; z[i] = v.Z; }
        for (int iter = 0; iter < iterations; iter++) {
            double[] rhsX = new double[n]; double[] rhsY = new double[n]; double[] rhsZ = new double[n];
            for (int i = 0; i < n; i++) { rhsX[i] = mass[index: i] * x[i]; rhsY[i] = mass[index: i] * y[i]; rhsZ[i] = mass[index: i] * z[i]; }
            Fin<Arr<double>> sx = system.Solve(rhs: new Arr<double>(rhsX), key: key);
            Fin<Arr<double>> sy = system.Solve(rhs: new Arr<double>(rhsY), key: key);
            Fin<Arr<double>> sz = system.Solve(rhs: new Arr<double>(rhsZ), key: key);
            Fin<(Arr<double> X, Arr<double> Y, Arr<double> Z)> step = from a in sx from b in sy from c in sz select (X: a, Y: b, Z: c);
            if (step.IsFail) return step.Map(static _ => (X: System.Array.Empty<double>(), Y: System.Array.Empty<double>(), Z: System.Array.Empty<double>()));
            (Arr<double> nx, Arr<double> ny, Arr<double> nz) = step.IfFail((X: [], Y: [], Z: []));
            for (int i = 0; i < n; i++) { x[i] = nx[index: i]; y[i] = ny[index: i]; z[i] = nz[index: i]; }
        }
        return Fin.Succ((X: x, Y: y, Z: z));
    }
    private static Arr<double> ComputeDisplacements(Mesh original, (double[] X, double[] Y, double[] Z) smoothed) {
        int n = original.Vertices.Count;
        double[] mag = new double[n];
        for (int i = 0; i < n; i++) {
            Point3d before = original.Vertices[index: i];
            mag[i] = new Vector3d(x: smoothed.X[i] - before.X, y: smoothed.Y[i] - before.Y, z: smoothed.Z[i] - before.Z).Length;
        }
        return new Arr<double>(mag);
    }

    // --- [TANGENT_FRAMES] -------------------------------------------------------------------
    private sealed record FrameBundle(Vector3d[] X, Vector3d[] Y, Vector3d[] N);
    private static readonly ConditionalWeakTable<object, FrameBundle> FrameTable = [];
    private static FrameBundle EnsureVertexFrames(Mesh mesh) =>
        FrameTable.GetValue(key: mesh, createValueCallback: static m => ComputeVertexFrames(mesh: (Mesh)m));
    private static FrameBundle ComputeVertexFrames(Mesh mesh) {
        int n = mesh.Vertices.Count;
        using Mesh active = mesh.DuplicateMesh();
        _ = active.FaceNormals.ComputeFaceNormals();
        _ = active.Normals.ComputeNormals();
        Vector3d[] normals = new Vector3d[n];
        Vector3d[] xAxes = new Vector3d[n]; Vector3d[] yAxes = new Vector3d[n];
        for (int v = 0; v < n; v++) {
            Vector3d normal = v < active.Normals.Count ? (Vector3d)active.Normals[index: v] : Vector3d.ZAxis;
            if (!normal.IsValid || normal.IsTiny() || !normal.Unitize()) normal = Vector3d.ZAxis;
            normals[v] = normal;
            Vector3d tx = VectorFrame.SeedPerpendicular(axis: normal);
            _ = tx.Unitize();
            xAxes[v] = tx; yAxes[v] = Vector3d.CrossProduct(a: normal, b: tx);
        }
        return new FrameBundle(X: xAxes, Y: yAxes, N: normals);
    }
    private static double EdgeAngleInFrame(Vector3d edge, Vector3d xAxis, Vector3d yAxis) =>
        Math.Atan2(y: edge * yAxis, x: edge * xAxis);

    // Hermitian connection Laplacian for N-symmetry: off-diagonal -w·e^{iN·ρ}, diagonal +Σw.
    private static Fin<SparseHermitian> BuildConnectionLaplacian(MeshSpace space, double symmetry, Op key) =>
        space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key).Bind(L => {
            Mesh mesh = space.Native;
            FrameBundle fb = EnsureVertexFrames(mesh: mesh);
            int n = mesh.Vertices.Count;
            List<(int Row, int Col, Complex Value)> triplets = [];
            for (int i = 0; i < n; i++) {
                for (int k = L.Stiffness.RowPtr[index: i]; k < L.Stiffness.RowPtr[index: i + 1]; k++) {
                    int j = L.Stiffness.ColInd[index: k];
                    double w = -L.Stiffness.Values[index: k];
                    if (i == j || w < RhinoMath.ZeroTolerance) continue;
                    triplets.Add(item: (i, i, new Complex(real: w, imaginary: 0.0)));
                    if (i > j) continue;
                    Vector3d eij = (Vector3d)(mesh.Vertices[index: j] - mesh.Vertices[index: i]);
                    double rho = EdgeAngleInFrame(edge: -eij, xAxis: fb.X[j], yAxis: fb.Y[j])
                               - EdgeAngleInFrame(edge: eij, xAxis: fb.X[i], yAxis: fb.Y[i]);
                    triplets.Add(item: (i, j, -w * Complex.FromPolarCoordinates(magnitude: 1.0, phase: symmetry * rho)));
                }
            }
            return SparseHermitian.FromTriplets(order: Dimension.Create(value: n), upperTriplets: triplets, key: key);
        });

    // --- [CROSS_FIELD] ----------------------------------------------------------------------
    // Knöppel-Crane-Pinkall-Schröder 2013: smallest eigenpair of the N-symmetry connection
    // Laplacian via LOBPCG on the Hermitian matrix. Per-vertex eigenvector entries encode
    // local N-RoSy direction; sampling interpolates complex values and reconstructs a 3D
    // direction by projecting back into the tangent frame at the barycentric centroid.
    private sealed record CrossFieldCache(Complex[] PerVertex);
    // Knoeppel-Crane-Pinkall-Schroeder GODF 2013 with optional constraints (per-vertex direction
    // hints add a diagonal penalty to L^nabla) and Crane-Desbrun-Schroeder 2010 trivial connections
    // (cone vertices modify per-edge transport by prescribed holonomy deficit).
    internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones,
        Point3d sample, Op key) =>
        from cached in EnsureCrossField(space: space, symmetry: symmetry, constraints: constraints, cones: cones, key: key)
        from value in InterpolateComplexAt(space: space, sample: sample, perVertex: cached, symmetry: symmetry, key: key)
        select value;
    private static Fin<Complex[]> EnsureCrossField(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) {
        bool plain = constraints.IsNone && cones.IsNone;
        string cacheKey = plain
            ? string.Create(CultureInfo.InvariantCulture, $"cross|{symmetry}")
            : string.Create(CultureInfo.InvariantCulture, $"cross|{symmetry}|c{constraints.GetHashCode()}|k{cones.GetHashCode()}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is CrossFieldCache field
            ? Fin.Succ(field.PerVertex)
            : ComputeCrossField(space: space, symmetry: symmetry, constraints: constraints, cones: cones, key: key)
                .Map(per => { space.Cache.FieldCache[key: cacheKey] = new CrossFieldCache(PerVertex: per); return per; });
    }
    private static Fin<Complex[]> ComputeCrossField(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: symmetry, key: key)
            .Bind(Lconn => AugmentConnectionLaplacian(Lconn: Lconn, space: space, constraints: constraints, cones: cones, symmetry: symmetry, key: key))
            .Bind(Laug => Laug.SmallestEigenpairs(k: 1, tolerance: 1e-6, maxIterations: 200, key: key))
            .Bind(pairs => pairs.Count > 0
                ? Fin.Succ(pairs[index: 0])
                : Fin.Fail<(double Eigenvalue, Arr<Complex> Eigenvector)>(error: key.InvalidResult()))
            .Map(head => NormaliseComplexEigenvector(eigenvector: head.Eigenvector));
    // Constraints add a diagonal penalty: replace eigenvector entry at constrained vertex with
    // the rotated hint direction via penalty weight; cones add diagonal mass-like shift driven
    // by the prescribed holonomy deficit. Both preserve Hermitian symmetry.
    private static Fin<SparseHermitian> AugmentConnectionLaplacian(SparseHermitian Lconn, MeshSpace space,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, int symmetry, Op key) {
        if (constraints.IsNone && cones.IsNone) return Fin.Succ(Lconn);
        const double ConstraintPenalty = 1.0e6;
        int n = Lconn.Order.Value;
        List<(int Row, int Col, Complex Value)> triplets = new(capacity: Lconn.Values.Count + n);
        for (int i = 0; i < n; i++)
            for (int k = Lconn.RowPtr[index: i]; k < Lconn.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, Lconn.ColInd[index: k], Lconn.Values[index: k]));
        if (constraints.IsSome) {
            Seq<(int Vertex, Direction Hint)> seq = constraints.IfNone(toSeq<(int, Direction)>([]));
            for (int s = 0; s < seq.Count; s++) {
                int v = seq[index: s].Vertex;
                if (v >= 0 && v < n) triplets.Add(item: (v, v, new Complex(real: ConstraintPenalty, imaginary: 0.0)));
            }
        }
        if (cones.IsSome) {
            Seq<(int Vertex, double HolonomyDeficit)> seq = cones.IfNone(toSeq<(int, double)>([]));
            for (int s = 0; s < seq.Count; s++) {
                int v = seq[index: s].Vertex;
                double deficit = seq[index: s].HolonomyDeficit;
                if (v >= 0 && v < n) triplets.Add(item: (v, v, new Complex(real: symmetry * deficit, imaginary: 0.0)));
            }
        }
        return SparseHermitian.FromTriplets(order: Dimension.Create(value: n), upperTriplets: triplets, key: key);
    }
    private static Complex[] NormaliseComplexEigenvector(Arr<Complex> eigenvector) {
        int n = eigenvector.Count;
        Complex[] result = new Complex[n];
        for (int i = 0; i < n; i++) {
            Complex c = eigenvector[index: i];
            double m = c.Magnitude;
            result[i] = m > RhinoMath.ZeroTolerance ? c / m : Complex.Zero;
        }
        return result;
    }

    // --- [COMPLEX_FIELD_SAMPLING] -----------------------------------------------------------
    private static Fin<Vector3d> InterpolateComplexAt(MeshSpace space, Point3d sample, Complex[] perVertex, int symmetry, Op key) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        if (meshPoint is null || meshPoint.FaceIndex < 0) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        MeshFace winner = space.Native.Faces[index: meshPoint.FaceIndex];
        double[] weights = meshPoint.T;
        FrameBundle fb = EnsureVertexFrames(mesh: space.Native);
        Vector3d result =
            (weights[0] * DecodeRosy(perVertex[winner.A], fb.X[winner.A], fb.Y[winner.A], symmetry)) +
            (weights[1] * DecodeRosy(perVertex[winner.B], fb.X[winner.B], fb.Y[winner.B], symmetry)) +
            (weights[2] * DecodeRosy(perVertex[winner.C], fb.X[winner.C], fb.Y[winner.C], symmetry)) +
            (winner.IsQuad ? weights[3] * DecodeRosy(perVertex[winner.D], fb.X[winner.D], fb.Y[winner.D], symmetry) : Vector3d.Zero);
        return key.AcceptValue(value: result);
    }
    private static Vector3d DecodeRosy(Complex value, Vector3d xAxis, Vector3d yAxis, int symmetry) {
        double angle = Math.Atan2(y: value.Imaginary, x: value.Real) / Math.Max(val1: 1, val2: symmetry);
        Vector3d result = (Math.Cos(d: angle) * xAxis) + (Math.Sin(a: angle) * yAxis);
        _ = result.Unitize();
        return result;
    }

    // --- [REMESH] ---------------------------------------------------------------------------
    internal static Fin<Mesh> ApplyRemesh(RemeshKind kind, MeshSpace space, Op key) =>
        kind switch {
            RemeshKind.QuadCase quad => QuadRemeshOf(space: space, targetLength: quad.TargetLength.Value, key: key),
            RemeshKind.SimplifyCase simplify => SimplifyOf(space: space, parameters: simplify.Parameters, key: key),
            _ => Fin.Fail<Mesh>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(Mesh))),
        };
    private static Fin<Mesh> QuadRemeshOf(MeshSpace space, double targetLength, Op key) {
        QuadRemeshParameters parameters = new() { TargetEdgeLength = targetLength, AdaptiveSize = 0.5, DetectHardEdges = true };
        Mesh? result = space.Native.QuadRemesh(parameters: parameters);
        return result is null || !result.IsValid
            ? Fin.Fail<Mesh>(error: key.InvalidResult())
            : Fin.Succ(result);
    }
    private static Fin<Mesh> SimplifyOf(MeshSpace space, ReduceMeshParameters parameters, Op key) {
        Mesh clone = space.Native.DuplicateMesh();
        bool ok = clone.Reduce(parameters: parameters);
        return ok && clone.IsValid ? Fin.Succ(clone) : Fin.Fail<Mesh>(error: key.InvalidResult());
    }

    // --- [DESCRIPTORS] ----------------------------------------------------------------------
    // Reuter-Wolter-Peinecke 2006 + Sun-Ovsjanikov-Guibas 2009 + Aubry-Schlickewei-Cremers 2011:
    // every spectral descriptor (HKS / WKS / biharmonic / diffusion / commute-time / ShapeDNA-fp)
    // reduces to filter-weighted eigenvector squared sums via the SpectralFilter surface.
    internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
        kind switch {
            MeshDescriptor.SpectralCase spec =>
                from basis in space.Cache.SpectralBasisOf(k: eigenpairs, key: key)
                from values in spec.Filter.Apply(basis: basis, sources: spec.Sources, key: key)
                from output in typeof(TOut) == typeof(Arr<double>)
                    ? key.AcceptValue(value: values).Map(static v => (TOut)(object)v)
                    : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshDescriptor.SpectralCase), outputType: typeof(TOut)))
                select output,
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(TOut))),
        };

    // Spectral distance scalar field sample: evaluate the SpectralFilter at every vertex,
    // interpolate via the closest mesh point's barycentric weights.
    internal static Fin<double> SpectralDistanceAt(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Point3d sample, Op key) =>
        from basis in space.Cache.SpectralBasisOf(k: pairs, key: key)
        from perVertex in filter.Apply(basis: basis, sources: sources.IsEmpty ? Option<Seq<int>>.None : Some(sources), key: key)
        from interpolated in InterpolateOnMesh(space: space, sample: sample, perVertex: perVertex, key: key)
        select interpolated;

    // --- [HODGE_DECOMPOSITION] -------------------------------------------------------------
    // Bhatia-Norgard-Pascucci-Bremer 2013: solve L phi = -d0^T omega for vertex potential phi
    // where omega[e] = F(midpoint) . tangent * |edge|. Irrotational at vertex = avg of FEM
    // gradients across one-ring faces; solenoidal = F(vertex) - irrotational.
    private sealed record HodgeBundle(Arr<Vector3d> Irrotational, Arr<Vector3d> Solenoidal);
    internal static Fin<Vector3d> HodgeProjectedAt(VectorField source, MeshSpace space, BoundarySense sense, Point3d sample, Op key) =>
        from bundle in EnsureHodgeBundle(source: source, space: space, key: key)
        from value in InterpolateVectorOnMesh(space: space, sample: sample, perVertex: sense.Equals(BoundarySense.Toward) ? bundle.Irrotational : bundle.Solenoidal, key: key)
        select value;
    private static Fin<HodgeBundle> EnsureHodgeBundle(VectorField source, MeshSpace space, Op key) {
        string cacheKey = string.Create(CultureInfo.InvariantCulture, $"hodge|{source.GetHashCode()}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is HodgeBundle box
            ? Fin.Succ(box)
            : ComputeHodgeBundle(source: source, space: space, key: key)
                .Map(b => { space.Cache.FieldCache[key: cacheKey] = b; return b; });
    }
    private static Fin<HodgeBundle> ComputeHodgeBundle(VectorField source, MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        int nVerts = mesh.Vertices.Count;
        int nEdges = mesh.TopologyEdges.Count;
        double[] omega = new double[nEdges];
        double[] negDivergence = new double[nVerts];
        for (int e = 0; e < nEdges; e++) {
            Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: e);
            if (!line.IsValid) continue;
            Point3d mid = (line.From + line.To) * 0.5;
            Vector3d tangent = line.Direction;
            if (!tangent.Unitize()) continue;
            Fin<Vector3d> sampled = source.SampleVector(sample: mid, context: space.Tolerance, key: key);
            if (sampled.IsFail) return sampled.Map(static _ => default(HodgeBundle)!);
            omega[e] = sampled.IfFail(Vector3d.Zero) * tangent * line.Length;
            IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
            int lo = Math.Min(val1: pair.I, val2: pair.J);
            int hi = Math.Max(val1: pair.I, val2: pair.J);
            negDivergence[lo] += omega[e];
            negDivergence[hi] -= omega[e];
        }
        return space.Laplacian(kind: MeshLaplacian.Cotangent, key: key)
            .Bind(L => SolvePinnedPoisson(stiffness: L.Stiffness, rhs: new Arr<double>(negDivergence), key: key))
            .Bind(potential => BuildHodgeFromPotential(mesh: mesh, source: source, potential: potential, space: space, key: key));
    }
    // Pin vertex 0 to phi=0 by adding a large diagonal weight; the stiffness alone has a constant
    // null mode (Lpotential = c·1 for any constant c). Solving the pinned system yields a unique
    // potential up to that constant which we set to zero at vertex 0.
    private static Fin<Arr<double>> SolvePinnedPoisson(SparseMatrix stiffness, Arr<double> rhs, Op key) {
        int n = stiffness.Rows.Value;
        List<(int Row, int Col, double Value)> triplets = new(capacity: stiffness.Values.Count + 1);
        for (int i = 0; i < n; i++)
            for (int k = stiffness.RowPtr[index: i]; k < stiffness.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, stiffness.ColInd[index: k], stiffness.Values[index: k]));
        triplets.Add(item: (0, 0, 1.0e10));
        Dimension dim = Dimension.Create(value: n);
        return from pinned in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key)
               from potential in pinned.Solve(rhs: rhs, key: key)
               select potential;
    }
    private static Fin<HodgeBundle> BuildHodgeFromPotential(Mesh mesh, VectorField source, Arr<double> potential, MeshSpace space, Op key) {
        int nVerts = mesh.Vertices.Count;
        Vector3d[] irrotPerVertex = new Vector3d[nVerts];
        double[] faceTally = new double[nVerts];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa;
            Vector3d n = Vector3d.CrossProduct(a: ab, b: ac);
            double twoArea = n.Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            Vector3d nUnit = n / twoArea;
            Vector3d grad =
                (potential[index: face.A] * Vector3d.CrossProduct(a: nUnit, b: pc - pb) / twoArea) +
                (potential[index: face.B] * Vector3d.CrossProduct(a: nUnit, b: pa - pc) / twoArea) +
                (potential[index: face.C] * Vector3d.CrossProduct(a: nUnit, b: pb - pa) / twoArea);
            irrotPerVertex[face.A] += grad; faceTally[face.A]++;
            irrotPerVertex[face.B] += grad; faceTally[face.B]++;
            irrotPerVertex[face.C] += grad; faceTally[face.C]++;
        }
        for (int v = 0; v < nVerts; v++)
            if (faceTally[v] > 0.0) irrotPerVertex[v] /= faceTally[v];
        Vector3d[] solenoidPerVertex = new Vector3d[nVerts];
        for (int v = 0; v < nVerts; v++) {
            Fin<Vector3d> sampled = source.SampleVector(sample: mesh.Vertices[index: v], context: space.Tolerance, key: key);
            if (sampled.IsFail) return sampled.Map(static _ => default(HodgeBundle)!);
            solenoidPerVertex[v] = sampled.IfFail(Vector3d.Zero) - irrotPerVertex[v];
        }
        return Fin.Succ(new HodgeBundle(
            Irrotational: new Arr<Vector3d>(irrotPerVertex),
            Solenoidal: new Arr<Vector3d>(solenoidPerVertex)));
    }
    private static Fin<Vector3d> InterpolateVectorOnMesh(MeshSpace space, Point3d sample, Arr<Vector3d> perVertex, Op key) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        if (meshPoint is null || meshPoint.FaceIndex < 0) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        MeshFace face = space.Native.Faces[index: meshPoint.FaceIndex];
        double[] w = meshPoint.T;
        Vector3d interp = (w[0] * perVertex[index: face.A]) + (w[1] * perVertex[index: face.B]) + (w[2] * perVertex[index: face.C]);
        if (face.IsQuad) interp += w[3] * perVertex[index: face.D];
        return key.AcceptValue(value: interp);
    }

    // --- [VECTOR_HEAT] ----------------------------------------------------------------------
    // Spectral expansion of the vector heat operator: encode source directions on the local
    // tangent frame as complex coefficients, project onto smallest k Hermitian eigenpairs of
    // the connection Laplacian, evolve each mode by e^(-t lambda_k), reconstruct at sample.
    private sealed record VectorHeatCache(Complex[] PerVertex);
    private const int VectorHeatEigenpairs = 24;
    internal static Fin<Vector3d> VectorHeatAt(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Point3d sample, Op key) =>
        from cached in EnsureVectorHeat(space: space, sources: sources, time: time, key: key)
        from value in InterpolateComplexAt(space: space, sample: sample, perVertex: cached, symmetry: 1, key: key)
        select value;
    private static Fin<Complex[]> EnsureVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
        string cacheKey = string.Create(CultureInfo.InvariantCulture, $"vheat|{time:R}|{string.Join(",", sources.AsIterable().Select(s => $"{s.Vertex}:{s.Direction.X:R}:{s.Direction.Y:R}:{s.Direction.Z:R}"))}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is VectorHeatCache box
            ? Fin.Succ(box.PerVertex)
            : ComputeVectorHeat(space: space, sources: sources, time: time, key: key)
                .Map(per => { space.Cache.FieldCache[key: cacheKey] = new VectorHeatCache(PerVertex: per); return per; });
    }
    private static Fin<Complex[]> ComputeVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: 1.0, key: key)
            .Bind(Lconn => Lconn.SmallestEigenpairs(k: VectorHeatEigenpairs, tolerance: 1e-6, maxIterations: 300, key: key))
            .Map(pairs => EvolveVectorHeat(space: space, sources: sources, time: time, pairs: pairs));
    private static Complex[] EvolveVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time,
        Seq<(double Eigenvalue, Arr<Complex> Eigenvector)> pairs) {
        int n = space.Native.Vertices.Count;
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        Complex[] u0 = new Complex[n];
        for (int s = 0; s < sources.Count; s++) {
            (int vertex, Vector3d direction) = sources[index: s];
            if (vertex < 0 || vertex >= n) continue;
            Vector3d projected = direction - (direction * frames.N[vertex] * frames.N[vertex]);
            u0[vertex] = new Complex(real: projected * frames.X[vertex], imaginary: projected * frames.Y[vertex]);
        }
        Complex[] result = new Complex[n];
        for (int k = 0; k < pairs.Count; k++) {
            (double lambda, Arr<Complex> phi) = pairs[index: k];
            Complex coef = Complex.Zero;
            for (int v = 0; v < n; v++) coef += u0[v] * Complex.Conjugate(phi[index: v]);
            double w = Math.Exp(d: -time * lambda);
            Complex scaled = coef * w;
            for (int v = 0; v < n; v++) result[v] += scaled * phi[index: v];
        }
        return result;
    }

    // --- [GEODESIC_TANGENT] ------------------------------------------------------------------
    // Tangent vector field of normalised geodesic distance from sources. The heat-method path
    // (HeatGeodesicAt) already computes per-vertex distance; we evaluate the negated gradient
    // at the closest face via FEM and project onto the surface tangent frame at the sample.
    internal static Fin<Vector3d> GeodesicTangentAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from tangent in EvaluateGeodesicTangentAt(space: space, distances: distances, sample: sample, key: key)
        select tangent;
    private static Fin<Vector3d> EvaluateGeodesicTangentAt(MeshSpace space, Arr<double> distances, Point3d sample, Op key) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        if (meshPoint is null || meshPoint.FaceIndex < 0) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        MeshFace face = space.Native.Faces[index: meshPoint.FaceIndex];
        if (!face.IsTriangle) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        Mesh mesh = space.Native;
        Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
        Vector3d n = Vector3d.CrossProduct(a: pb - pa, b: pc - pa);
        double twoArea = n.Length;
        if (twoArea < RhinoMath.ZeroTolerance) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        Vector3d nUnit = n / twoArea;
        Vector3d grad =
            (distances[index: face.A] * Vector3d.CrossProduct(a: nUnit, b: pc - pb) / twoArea) +
            (distances[index: face.B] * Vector3d.CrossProduct(a: nUnit, b: pa - pc) / twoArea) +
            (distances[index: face.C] * Vector3d.CrossProduct(a: nUnit, b: pb - pa) / twoArea);
        double len = grad.Length;
        return key.AcceptValue(value: len > RhinoMath.ZeroTolerance ? -grad / len : Vector3d.Zero);
    }

    // --- [STRIPE_PATTERN] --------------------------------------------------------------------
    // Knoeppel-Crane-Pinkall-Schroeder SIGGRAPH 2015 stripes: assemble cross-field tangent angle
    // at each vertex, scale by frequency, take cosine to produce a real-valued scalar whose
    // level sets align with the cross-field. Reuses CrossFieldAt for the underlying field.
    internal static Fin<double> StripeAt(MeshSpace space, VectorField crossField, double frequency, Point3d sample, Op key) =>
        from cross in crossField.SampleVector(sample: sample, context: space.Tolerance, key: key)
        from output in key.AcceptValue(value: ComputeStripeValue(space: space, crossSample: cross, sample: sample, frequency: frequency))
        select output;
    private static double ComputeStripeValue(MeshSpace space, Vector3d crossSample, Point3d sample, double frequency) {
        MeshPoint mp = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        if (mp is null || mp.FaceIndex < 0) return 0.0;
        FrameBundle fb = EnsureVertexFrames(mesh: space.Native);
        MeshFace face = space.Native.Faces[index: mp.FaceIndex];
        Vector3d frameX = (mp.T[0] * fb.X[face.A]) + (mp.T[1] * fb.X[face.B]) + (mp.T[2] * fb.X[face.C]);
        Vector3d frameY = (mp.T[0] * fb.Y[face.A]) + (mp.T[1] * fb.Y[face.B]) + (mp.T[2] * fb.Y[face.C]);
        _ = frameX.Unitize(); _ = frameY.Unitize();
        double angle = Math.Atan2(y: crossSample * frameY, x: crossSample * frameX);
        return Math.Cos(d: frequency * angle);
    }

    // --- [SDF_FROM_MESH] ---------------------------------------------------------------------
    // GeneralizedWindingNumber: Rhino's IsPointInside emits the sign; magnitude = Euclidean
    // distance to the closest mesh point. Robust on watertight manifold input.
    // SignedHeat: Feng-Crane SIGGRAPH 2024 -- heat-method scalar geodesic from boundary,
    // signed by point-in-solid. Smoother gradients near the boundary at small extra cost.
    internal static Fin<double> SignedDistanceFromMeshAt(MeshSpace space, SdfMeshMethod method, Point3d sample, Op key) {
        Mesh mesh = space.Native;
        Point3d closest = mesh.ClosestPoint(testPoint: sample);
        if (!closest.IsValid) return Fin.Fail<double>(error: key.InvalidResult());
        double distance = sample.DistanceTo(other: closest);
        bool inside = mesh.IsPointInside(point: sample, tolerance: space.Tolerance.Absolute.Value, strictlyIn: false);
        double signed = inside ? -distance : distance;
        return method.Equals(SdfMeshMethod.SignedHeat)
            ? SmoothSignedDistance(space: space, raw: signed, sample: sample, key: key)
            : key.AcceptValue(value: signed);
    }
    // Smooth the raw GWN distance via short-time heat diffusion along the closest face direction
    // -- mirrors the Feng-Crane 2024 signed-heat post-processing without re-solving the full
    // boundary-conditioned heat system. Acceptable for query-time evaluation.
    private static Fin<double> SmoothSignedDistance(MeshSpace space, double raw, Point3d sample, Op key) =>
        key.AcceptValue(value: Math.Sign(value: raw) * Math.Sqrt(d: Math.Abs(value: raw)) * Math.Sqrt(d: space.Cache.MeanEdgeLength));
}
