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

// Typed cache keys for every kernel that memoises per-vertex / per-edge fields on a mesh. C#
// record-struct value equality is the actual cache discriminator — no string concatenation, no
// hash-only keys with silent collisions. Each tuple element bottoms out in a value-equal type
// (Seq has structural equality from LanguageExt; Direction / Vector3d / VectorField are
// records with auto-generated equality).
internal readonly record struct GeodesicKey(Seq<int> Sources);
[StructLayout(LayoutKind.Auto)]
internal readonly record struct McfKey(double TimeStep, int Iterations);
[StructLayout(LayoutKind.Auto)]
internal readonly record struct CrossFieldKey(
    int Symmetry,
    Option<Seq<(int Vertex, Direction Hint)>> Constraints,
    Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones);
internal readonly record struct HodgeKey(VectorField Source);
[StructLayout(LayoutKind.Auto)]
internal readonly record struct VectorHeatKey(double Time, Seq<(int Vertex, Vector3d Direction)> Sources);

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
    private readonly Lazy<Fin<MeshKernel.IntrinsicMesh>> intrinsicMesh;
    private readonly Lazy<Fin<Arr<double>>> signedHeat;
    private readonly Atom<HashMap<(int Symmetry, double Time), Fin<CholeskySparse>>> connectionCholesky;
    private readonly Atom<HashMap<double, Fin<CholeskySparse>>> scalarHeatCholesky;
    private readonly Atom<HashMap<double, Fin<CholeskySparse>>> edgeConnectionCholesky;
    private readonly Atom<HashMap<GeodesicKey, Fin<Arr<double>>>> geodesicCache;
    private readonly Atom<HashMap<McfKey, Fin<Arr<double>>>> mcfCache;
    private readonly Atom<HashMap<CrossFieldKey, Fin<Complex[]>>> crossFieldCache;
    private readonly Atom<HashMap<HodgeKey, Fin<MeshKernel.HodgeBundle>>> hodgeCache;
    private readonly Atom<HashMap<VectorHeatKey, Fin<Complex[]>>> vectorHeatCache;
    private readonly MeshSpace space;
    private LaplacianCache(MeshSpace space) {
        this.space = space;
        Op fallback = Op.Of();
        intrinsicMesh = new Lazy<Fin<MeshKernel.IntrinsicMesh>>(valueFactory: () => MeshKernel.BuildIntrinsicMesh(mesh: space.Native, key: fallback));
        cotangent = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: fallback));
        intrinsicDelaunay = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => intrinsicMesh.Value.Bind(im => MeshKernel.AssembleCotangentFromIntrinsic(imesh: im, key: fallback)));
        robust = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleRobust(mesh: space.Native, key: fallback));
        // Sharp-Soliman-Crane 2019: regularise the scalar (M+tL) by ε·diag(M) before Cholesky to
        // break the constant null space; identical regulariser used for the vector-heat system.
        cholesky = new Lazy<Fin<CholeskySparse>>(valueFactory: () =>
            from L in intrinsicDelaunay.Value
            from spd in MeshKernel.MassPinned(laplacian: L, regularization: SpdRegularization, key: fallback)
            from factor in CholeskySparse.Of(symmetric: spd, key: fallback)
            select factor);
        defaultSpectral = new Lazy<Fin<SpectralBasis>>(valueFactory: () => MeshKernel.ComputeSpectralBasis(space: space, k: DefaultSpectralCount, key: fallback));
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
        signedHeat = new Lazy<Fin<Arr<double>>>(valueFactory: () => MeshKernel.ComputeSignedHeat(space: space, key: fallback));
        connectionCholesky = Atom(value: HashMap<(int Symmetry, double Time), Fin<CholeskySparse>>());
        scalarHeatCholesky = Atom(value: HashMap<double, Fin<CholeskySparse>>());
        edgeConnectionCholesky = Atom(value: HashMap<double, Fin<CholeskySparse>>());
        geodesicCache = Atom(value: HashMap<GeodesicKey, Fin<Arr<double>>>());
        mcfCache = Atom(value: HashMap<McfKey, Fin<Arr<double>>>());
        crossFieldCache = Atom(value: HashMap<CrossFieldKey, Fin<Complex[]>>());
        hodgeCache = Atom(value: HashMap<HodgeKey, Fin<MeshKernel.HodgeBundle>>());
        vectorHeatCache = Atom(value: HashMap<VectorHeatKey, Fin<Complex[]>>());
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent => cotangent.Value;
    internal Fin<SparseLaplacian> IntrinsicDelaunay => intrinsicDelaunay.Value;
    internal Fin<SparseLaplacian> Robust => robust.Value;
    internal Fin<CholeskySparse> Cholesky => cholesky.Value;
    internal Fin<MeshKernel.IntrinsicMesh> IntrinsicMeshSnapshot => intrinsicMesh.Value;
    internal Fin<Arr<double>> SignedHeat => signedHeat.Value;
    internal double MeanEdgeLength => meanEdgeLength.Value;
    internal Fin<SpectralBasis> SpectralBasisOf(int k, Op key) =>
        k <= DefaultSpectralCount
            ? defaultSpectral.Value.Map(b => b.Truncate(k: k))
            : MeshKernel.ComputeSpectralBasis(space: space, k: k, key: key);
    // Centralised memoisation helper: probe → on miss, run compute and persist. Atom.Swap is
    // STM-safe for concurrent callers; the worst case is duplicate compute (last writer wins).
    private static Fin<T> Memoise<TKey, T>(Atom<HashMap<TKey, Fin<T>>> atom, TKey probe, Func<Fin<T>> compute) =>
        atom.Value.Find(key: probe).IfNone(() => {
            Fin<T> computed = compute();
            _ = atom.Swap(f: map => map.AddOrUpdate(key: probe, value: computed));
            return computed;
        });
    // Parametric Cholesky factor of the 2V real-block embedding of (M + t·L_conn). Cached per
    // (symmetry, time) so VHM queries with the same parameters reuse the AMD-ordered factor.
    internal Fin<CholeskySparse> ConnectionCholesky(int symmetry, double time, Op key) =>
        Memoise(atom: connectionCholesky, probe: (symmetry, time), compute: () =>
            from real in MeshKernel.BuildConnectionLaplacianRealSystem(space: space, symmetry: symmetry, time: time, edgeAdjustment: Option<Arr<double>>.None, key: key)
            from factor in CholeskySparse.Of(symmetric: real, key: key)
            select factor);
    // Cone-affected uncached factor. Cones modify the connection at the off-diagonal level, so the
    // factor cannot be keyed by (symmetry, time) alone; rebuild and refactor each call. Falls back
    // to the cached path when edgeAdjustment is None (callers should prefer the cached overload).
    internal Fin<CholeskySparse> ConnectionCholeskyAdjusted(int symmetry, double time, Arr<double> edgeAdjustment, Op key) =>
        from real in MeshKernel.BuildConnectionLaplacianRealSystem(space: space, symmetry: symmetry, time: time, edgeAdjustment: Some(edgeAdjustment), key: key)
        from factor in CholeskySparse.Of(symmetric: real, key: key)
        select factor;
    // Parametric Cholesky factor of the scalar heat system (M + t·L_cot). Used by VHM magnitude
    // carriers (φ from |X₀|, ψ from δ_sources) and any heat-method consumer with fixed time step.
    internal Fin<CholeskySparse> ScalarHeatCholesky(double time, Op key) =>
        Memoise(atom: scalarHeatCholesky, probe: time, compute: () =>
            from L in intrinsicDelaunay.Value
            from system in MeshKernel.AssembleScalarHeatSystem(laplacian: L, time: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system, key: key)
            select factor);
    // Cholesky factor of the real-2|E| block (M_CR + t·L_CR) for the Crouzeix-Raviart connection
    // system. Used by the Feng-Crane 2024 Signed Heat Method. SPD iff the underlying complex
    // Hermitian L_CR is PSD — guaranteed because L_CR is assembled on the intrinsic-Delaunay
    // edge set exposed through IntrinsicMeshSnapshot.
    internal Fin<CholeskySparse> EdgeConnectionCholesky(double time, Op key) =>
        Memoise(atom: edgeConnectionCholesky, probe: time, compute: () =>
            from imesh in intrinsicMesh.Value
            from system in SpectralCore.BuildCrouzeixRaviartHeatSystem(mesh: imesh, time: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system, key: key)
            select factor);
    // Typed per-kernel field caches. Each invocation memoises by the structurally-equal key
    // record, returning the previously-computed Fin on cache hit (including failures, which the
    // caller can retry by passing different inputs).
    internal Fin<Arr<double>> Geodesic(GeodesicKey probe, Func<Fin<Arr<double>>> compute) => Memoise(geodesicCache, probe, compute);
    internal Fin<Arr<double>> Mcf(McfKey probe, Func<Fin<Arr<double>>> compute) => Memoise(mcfCache, probe, compute);
    internal Fin<Complex[]> CrossField(CrossFieldKey probe, Func<Fin<Complex[]>> compute) => Memoise(crossFieldCache, probe, compute);
    internal Fin<MeshKernel.HodgeBundle> Hodge(HodgeKey probe, Func<Fin<MeshKernel.HodgeBundle>> compute) => Memoise(hodgeCache, probe, compute);
    internal Fin<Complex[]> VectorHeat(VectorHeatKey probe, Func<Fin<Complex[]>> compute) => Memoise(vectorHeatCache, probe, compute);
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
        BuildIntrinsicMesh(mesh: mesh, key: key).Bind(im => AssembleCotangentFromIntrinsic(imesh: im, key: key));

    // Flip an extrinsic mesh to intrinsic-Delaunay then Freeze() the result so its edge/face
    // index spaces become stable and immutable. Downstream consumers (SHM, cone holonomy, Hodge
    // decomposition) read frozen accessors only; mutating flip helpers are off-limits post-Freeze.
    internal static Fin<IntrinsicMesh> BuildIntrinsicMesh(Mesh mesh, Op key) =>
        FlipToDelaunay(imesh: IntrinsicMesh.FromMesh(mesh: mesh), key: key)
            .Map(im => { im.Freeze(); return im; });

    // Intrinsic edge (post-Freeze): undirected vertex pair with intrinsic length and the indices
    // of the (≤2) incident faces. Face1 = -1 for naked boundary edges.
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct IntrinsicEdge(int Lo, int Hi, double Length, int Face0, int Face1) {
        internal bool IsInterior => Face1 >= 0;
    }

    internal sealed class IntrinsicMesh {
        internal int VertexCount;
        internal readonly List<(int A, int B, int C)?> Triangles = [];
        internal readonly Dictionary<(int Lo, int Hi), (double Length, List<int> FaceIdx)> EdgeData = [];
        // Post-Freeze projections (immutable). Index space stabilises only after all flips end.
        private IntrinsicEdge[]? frozenEdges;
        private Dictionary<(int Lo, int Hi), int>? frozenEdgeIndex;
        private int[][]? frozenFaceEdges;
        private double[]? frozenFaceAreas;
        private int[]? frozenFirstIncidentEdge;
        internal bool IsFrozen => frozenEdges is not null;
        internal int EdgeCount => frozenEdges?.Length ?? 0;
        internal IntrinsicEdge EdgeAt(int index) => frozenEdges![index];
        internal int IndexOfEdge(int lo, int hi) {
            (int Lo, int Hi) probe = EdgeKey(i: lo, j: hi);
            return frozenEdgeIndex is null
                ? -1
                : frozenEdgeIndex.TryGetValue(key: probe, value: out int idx) ? idx : -1;
        }
        internal int[] EdgesOfFace(int faceIdx) => frozenFaceEdges![faceIdx];
        internal double AreaOfFace(int faceIdx) => frozenFaceAreas![faceIdx];
        internal int FirstIncidentEdge(int vertexIdx) => frozenFirstIncidentEdge![vertexIdx];
        internal IEnumerable<int> LiveFaceIndices() {
            for (int f = 0; f < Triangles.Count; f++) if (Triangles[index: f].HasValue) yield return f;
        }
        // Snapshot the post-flip topology into integer-indexed arrays. Boundary edges (Face1 < 0)
        // are unchanged through IDT flips; interior edges may have been replaced by the flip
        // operations, and only the surviving edges in EdgeData populate the frozen index.
        internal void Freeze() {
            int eCount = EdgeData.Count;
            frozenEdges = new IntrinsicEdge[eCount];
            frozenEdgeIndex = new Dictionary<(int Lo, int Hi), int>(capacity: eCount);
            int idx = 0;
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx)> kv in EdgeData) {
                List<int> faces = kv.Value.FaceIdx;
                int f0 = faces.Count > 0 ? faces[index: 0] : -1;
                int f1 = faces.Count > 1 ? faces[index: 1] : -1;
                frozenEdges[idx] = new IntrinsicEdge(Lo: kv.Key.Lo, Hi: kv.Key.Hi, Length: kv.Value.Length, Face0: f0, Face1: f1);
                frozenEdgeIndex[key: kv.Key] = idx;
                idx++;
            }
            int faceCapacity = Triangles.Count;
            frozenFaceEdges = new int[faceCapacity][];
            frozenFaceAreas = new double[faceCapacity];
            for (int f = 0; f < faceCapacity; f++) {
                (int A, int B, int C)? slot = Triangles[index: f];
                if (slot is null) { frozenFaceEdges[f] = []; continue; }
                (int a, int b, int c) = slot.Value;
                int eAB = IndexOfEdge(lo: a, hi: b);
                int eBC = IndexOfEdge(lo: b, hi: c);
                int eCA = IndexOfEdge(lo: c, hi: a);
                frozenFaceEdges[f] = [eAB, eBC, eCA];
                double lAB = eAB >= 0 ? frozenEdges[eAB].Length : 0.0;
                double lBC = eBC >= 0 ? frozenEdges[eBC].Length : 0.0;
                double lCA = eCA >= 0 ? frozenEdges[eCA].Length : 0.0;
                double s = (lAB + lBC + lCA) * 0.5;
                double argInside = s * (s - lAB) * (s - lBC) * (s - lCA);
                frozenFaceAreas[f] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: argInside));
            }
            frozenFirstIncidentEdge = new int[VertexCount];
            System.Array.Fill(array: frozenFirstIncidentEdge, value: -1);
            for (int e = 0; e < frozenEdges.Length; e++) {
                IntrinsicEdge edge = frozenEdges[e];
                if (frozenFirstIncidentEdge[edge.Lo] < 0) frozenFirstIncidentEdge[edge.Lo] = e;
                if (frozenFirstIncidentEdge[edge.Hi] < 0) frozenFirstIncidentEdge[edge.Hi] = e;
            }
        }
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
    internal static Fin<SparseLaplacian> AssembleCotangentFromIntrinsic(IntrinsicMesh imesh, Op key) {
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

    // Assemble the scalar heat system (M + t·L_cot) as a real SPD SparseMatrix. M is the lumped
    // mass; L_cot the cotangent stiffness. Used by ScalarHeatCholesky for VHM magnitude carriers
    // and any other time-step-fixed heat solve.
    internal static Fin<SparseMatrix> AssembleScalarHeatSystem(SparseLaplacian laplacian, double time, Op key) {
        int n = laplacian.MassLumped.Count;
        if (n == 0) return Fin.Fail<SparseMatrix>(error: key.InvalidInput());
        List<(int Row, int Col, double Value)> triplets = new(capacity: n + laplacian.Stiffness.NonZeros);
        for (int i = 0; i < n; i++) triplets.Add(item: (i, i, laplacian.MassLumped[index: i]));
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, laplacian.Stiffness.ColInd[index: k], time * laplacian.Stiffness.Values[index: k]));
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
    internal static Fin<double> HeatGeodesicAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: distances, key: key)
        select value;
    private static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
        // Order-invariant key: distance-from-{a,b} == distance-from-{b,a}, so we sort.
        Seq<int> ordered = toSeq(sources.AsIterable().OrderBy(static i => i));
        return space.Cache.Geodesic(probe: new GeodesicKey(Sources: ordered),
            compute: () => space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                .Bind(L => ComputeHeatGeodesic(space: space, laplacian: L, sources: ordered, key: key)));
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
               from delta in Fin.Succ(SpectralCore.BuildSourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
               from u in A.Solve(rhs: delta, key: key)
               from gradient in Fin.Succ(SpectralCore.ComputeTriangleGradients(mesh: space.Native, u: u))
               from divergence in Fin.Succ(SpectralCore.ComputeVertexDivergence(mesh: space.Native, gradients: gradient))
               from poisson in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                   .Bind(L => SparseMatrix.FromTriplets(rows: dim, cols: dim,
                       triplets: SpectralCore.PoissonTriplets(L: L, sources: sources), key: key))
               from phi in poisson.Solve(rhs: divergence, key: key)
               select NormalizeToZero(values: phi);
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
    private static Fin<Arr<double>> EnsureMcfDisplacements(MeshSpace space, double timeStep, int iterations, Op key) =>
        space.Cache.Mcf(probe: new McfKey(TimeStep: timeStep, Iterations: iterations),
            compute: () => space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                .Bind(L => ComputeMeanCurvatureFlow(space: space, laplacian: L, timeStep: timeStep, iterations: iterations, key: key)));
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

    // Per-edge connection contributions sourced from the INTRINSIC-Delaunay edge set: upper-
    // triangular (Lo, Hi) cotangent weight w and raw transport angle ρ. Diagonal accumulates Σw
    // at both endpoints; off-diagonals carry -w·exp(iN·ρ) once symmetry N is applied by the
    // consumer. Optional edgeAdjustment α_e (CDS 2010 trivial connection) is indexed by INTRINSIC
    // edge — same index space as IntrinsicMeshSnapshot, so flipped edges are properly addressed.
    private static Fin<Seq<(int I, int J, double Weight, double Rho)>> EnumerateConnectionEntries(MeshSpace space, Option<Arr<double>> edgeAdjustment, Op key) =>
        space.Cache.IntrinsicMeshSnapshot.Map(imesh => {
            Mesh mesh = space.Native;
            FrameBundle fb = EnsureVertexFrames(mesh: mesh);
            int eCount = imesh.EdgeCount;
            bool hasAdjustment = edgeAdjustment.IsSome;
            Arr<double> adjustment = edgeAdjustment.IfNone(new Arr<double>([]));
            // Per-intrinsic-edge cotangent weight w = 0.5·(cot α + cot β) — identical to ★₁ on
            // the intrinsic-Delaunay mesh, so reuse SpectralCore's helper instead of duplicating
            // the Heron / law-of-cosines accumulation here.
            Arr<double> weights = SpectralCore.ComputeIntrinsicStar1(imesh: imesh);
            List<(int, int, double, double)> entries = new(capacity: eCount);
            for (int e = 0; e < eCount; e++) {
                double w = weights[index: e];
                if (w < RhinoMath.ZeroTolerance) continue;
                IntrinsicEdge edge = imesh.EdgeAt(index: e);
                int i = edge.Lo, j = edge.Hi;
                Vector3d eij = (Vector3d)(mesh.Vertices[index: j] - mesh.Vertices[index: i]);
                double rho = EdgeAngleInFrame(edge: -eij, xAxis: fb.X[j], yAxis: fb.Y[j])
                           - EdgeAngleInFrame(edge: eij, xAxis: fb.X[i], yAxis: fb.Y[i]);
                if (hasAdjustment && e < adjustment.Count) rho += adjustment[index: e];
                entries.Add(item: (i, j, w, rho));
            }
            return toSeq(entries);
        });

    // Hermitian connection Laplacian for N-symmetry: off-diagonal -w·exp(iN·ρ), diagonal +Σw.
    // Used by CrossField LOBPCG path (smallest constrained eigenvector). Optional edgeAdjustment
    // applies CDS 2010 trivial-connection correction before assembly.
    private static Fin<SparseHermitian> BuildConnectionLaplacian(MeshSpace space, double symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        from entries in EnumerateConnectionEntries(space: space, edgeAdjustment: edgeAdjustment, key: key)
        let n = space.Native.Vertices.Count
        let triplets = AssembleHermitianTriplets(entries: entries, symmetry: symmetry)
        from result in SparseHermitian.FromTriplets(order: Dimension.Create(value: n), upperTriplets: triplets, key: key)
        select result;

    private static List<(int Row, int Col, Complex Value)> AssembleHermitianTriplets(Seq<(int I, int J, double Weight, double Rho)> entries, double symmetry) {
        List<(int, int, Complex)> triplets = new(capacity: entries.Count * 3);
        for (int e = 0; e < entries.Count; e++) {
            (int i, int j, double w, double rho) = entries[index: e];
            Complex offDiag = -w * Complex.FromPolarCoordinates(magnitude: 1.0, phase: symmetry * rho);
            triplets.Add(item: (i, i, new Complex(real: w, imaginary: 0.0)));
            triplets.Add(item: (j, j, new Complex(real: w, imaginary: 0.0)));
            triplets.Add(item: (i, j, offDiag));
        }
        return triplets;
    }

    // Real-block SPD reformulation of (M_V + t·L_conn) for direct Cholesky factor-and-solve.
    // Hermitian H = A + iB (A symmetric, B antisymmetric) embeds as [[A,-B],[B,A]] of order 2V.
    // Per Sharp-Soliman-Crane 2019 Theorem 4.1, L_conn is PSD on intrinsic Delaunay meshes;
    // the cache routes through IntrinsicDelaunay so the block matrix is SPD.
    internal static Fin<SparseMatrix> BuildConnectionLaplacianRealSystem(MeshSpace space, double symmetry, double time, Option<Arr<double>> edgeAdjustment, Op key) =>
        from entries in EnumerateConnectionEntries(space: space, edgeAdjustment: edgeAdjustment, key: key)
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        let n = space.Native.Vertices.Count
        let triplets = AssembleRealBlockTriplets(n: n, mass: laplacian.MassLumped, entries: entries, symmetry: symmetry, time: time)
        from result in SparseMatrix.FromTriplets(rows: Dimension.Create(value: 2 * n), cols: Dimension.Create(value: 2 * n), triplets: triplets, key: key)
        select result;

    private static List<(int Row, int Col, double Value)> AssembleRealBlockTriplets(int n, Arr<double> mass, Seq<(int I, int J, double Weight, double Rho)> entries, double symmetry, double time) {
        List<(int, int, double)> triplets = new(capacity: (2 * n) + (entries.Count * 12));
        for (int v = 0; v < n; v++) {
            triplets.Add(item: (v, v, mass[index: v]));
            triplets.Add(item: (v + n, v + n, mass[index: v]));
        }
        for (int e = 0; e < entries.Count; e++) {
            (int i, int j, double w, double rho) = entries[index: e];
            double re = -w * Math.Cos(d: symmetry * rho) * time;
            double im = -w * Math.Sin(a: symmetry * rho) * time;
            double diag = time * w;
            triplets.Add(item: (i, i, diag));
            triplets.Add(item: (j, j, diag));
            triplets.Add(item: (i + n, i + n, diag));
            triplets.Add(item: (j + n, j + n, diag));
            triplets.Add(item: (i, j, re));
            triplets.Add(item: (j, i, re));
            triplets.Add(item: (i + n, j + n, re));
            triplets.Add(item: (j + n, i + n, re));
            triplets.Add(item: (i, j + n, -im));
            triplets.Add(item: (j + n, i, -im));
            triplets.Add(item: (i + n, j, im));
            triplets.Add(item: (j, i + n, im));
        }
        return triplets;
    }

    // --- [CROSS_FIELD] ----------------------------------------------------------------------
    // Knöppel-Crane-Pinkall-Schröder 2013: smallest eigenpair of the N-symmetry connection
    // Laplacian via LOBPCG on the Hermitian matrix. Per-vertex eigenvector entries encode
    // local N-RoSy direction; sampling interpolates complex values and reconstructs a 3D
    // direction by projecting back into the tangent frame at the barycentric centroid.
    // Knoeppel-Crane-Pinkall-Schroeder GODF 2013 with optional constraints (per-vertex direction
    // hints add a diagonal penalty to L^nabla) and Crane-Desbrun-Schroeder 2010 trivial connections
    // (cone vertices modify per-edge transport by prescribed holonomy deficit).
    internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones,
        Point3d sample, Op key) =>
        from cached in EnsureCrossField(space: space, symmetry: symmetry, constraints: constraints, cones: cones, key: key)
        from value in InterpolateRosyAt(space: space, sample: sample, perVertex: cached, symmetry: symmetry, key: key)
        select value;
    private static Fin<Complex[]> EnsureCrossField(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
        space.Cache.CrossField(probe: new CrossFieldKey(Symmetry: symmetry, Constraints: constraints, Cones: cones),
            compute: () => ComputeCrossField(space: space, symmetry: symmetry, constraints: constraints, cones: cones, key: key));
    // Knöppel-Crane-Pinkall-Schröder GODF 2013 + Crane-Desbrun-Schröder 2010 trivial connections.
    // With hints: solve (A − λ_T·B)·u = B·q̂ at λ_T = 0 — soft alignment toward q̂. Without hints:
    // smallest eigenpair of A. With cones: compute per-edge α via Hodge-decomposed primal 1-form
    // and apply ρ_ij ← ρ_ij + α_e before assembly. Cone path bypasses the cache since the factor
    // depends on the cone prescription.
    private const double ConstrainedShiftReciprocal = 1.0e9;
    private static Fin<Complex[]> ComputeCrossField(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
        ResolveEdgeAdjustment(space: space, cones: cones, key: key).Bind(adjustment =>
            constraints.IsSome
                ? SolveConstrainedCrossField(space: space, symmetry: symmetry, hints: constraints.IfNone(toSeq<(int, Direction)>([])), edgeAdjustment: adjustment, key: key)
                : SolveSmoothestCrossField(space: space, symmetry: symmetry, edgeAdjustment: adjustment, key: key));
    // Cones carry holonomy deficits (Δ_v = 2π·k_v); convert to indices k_v = Δ_v/(2π) and route
    // through SpectralCore.DistributeHolonomy on the intrinsic mesh so α_e is indexed by intrinsic
    // edge (matching EnumerateConnectionEntries' lookup). Returns None when no cones supplied.
    private static Fin<Option<Arr<double>>> ResolveEdgeAdjustment(MeshSpace space, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) {
        if (cones.IsNone) return Fin.Succ(Option<Arr<double>>.None);
        Seq<(int Vertex, double ConeIndex)> indices = cones
            .IfNone(toSeq<(int, double)>([]))
            .Map(c => (c.Vertex, ConeIndex: c.HolonomyDeficit / (2.0 * Math.PI)));
        return space.Cache.IntrinsicMeshSnapshot
            .Bind(imesh => SpectralCore.DistributeHolonomy(space: space, imesh: imesh, cones: indices, key: key))
            .Map(Some);
    }
    private static Fin<Complex[]> SolveSmoothestCrossField(MeshSpace space, int symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: symmetry, edgeAdjustment: edgeAdjustment, key: key)
            .Bind(Lconn => Lconn.SmallestEigenpairs(k: 1, tolerance: 1e-6, maxIterations: 200, key: key))
            .Bind(pairs => pairs.Count > 0
                ? Fin.Succ(pairs[index: 0])
                : Fin.Fail<(double Eigenvalue, Arr<Complex> Eigenvector)>(error: key.InvalidResult()))
            .Map(head => NormaliseComplexEigenvector(eigenvector: head.Eigenvector));
    private static Fin<Complex[]> SolveConstrainedCrossField(MeshSpace space, int symmetry,
        Seq<(int Vertex, Direction Hint)> hints, Option<Arr<double>> edgeAdjustment, Op key) {
        int n = space.Native.Vertices.Count;
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        return from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
               let qHat = EncodeAndRescaleHints(n: n, hints: hints, frames: frames, symmetry: symmetry, mass: laplacian.MassLumped)
               let rhs = StackMassWeighted(n: n, qHat: qHat, mass: laplacian.MassLumped)
               from factor in edgeAdjustment.IsSome
                   ? space.Cache.ConnectionCholeskyAdjusted(symmetry: symmetry, time: ConstrainedShiftReciprocal, edgeAdjustment: edgeAdjustment.IfNone(new Arr<double>([])), key: key)
                   : space.Cache.ConnectionCholesky(symmetry: symmetry, time: ConstrainedShiftReciprocal, key: key)
               from solution in factor.Solve(rhs: rhs, key: key)
               select NormaliseComplexEigenvector(eigenvector: ReassembleComplex(n: n, real: solution));
    }
    // Per-vertex hint: project to tangent plane → complex coord in local frame → raise to nSym
    // for rotational symmetry → unit. After encoding, globally B-norm rescale q̂ ← q̂ / √(q̂* B q̂)
    // so the alignment energy is normalised independent of the number of hint vertices.
    private static Complex[] EncodeAndRescaleHints(int n, Seq<(int Vertex, Direction Hint)> hints, FrameBundle frames, int symmetry, Arr<double> mass) {
        Complex[] qHat = new Complex[n];
        for (int s = 0; s < hints.Count; s++) {
            (int v, Direction hint) = hints[index: s];
            if (v < 0 || v >= n) continue;
            Vector3d direction = hint.Value;
            Vector3d projected = direction - (direction * frames.N[v] * frames.N[v]);
            Complex tangent = new(real: projected * frames.X[v], imaginary: projected * frames.Y[v]);
            double mag = tangent.Magnitude;
            if (mag < RhinoMath.SqrtEpsilon) continue;
            Complex unit = tangent / mag;
            qHat[v] = Complex.Pow(value: unit, power: symmetry);
        }
        double bNormSq = 0.0;
        for (int v = 0; v < n; v++) bNormSq += mass[index: v] * (qHat[v] * Complex.Conjugate(qHat[v])).Real;
        double bNorm = Math.Sqrt(d: bNormSq);
        if (bNorm > RhinoMath.SqrtEpsilon)
            for (int v = 0; v < n; v++) qHat[v] /= bNorm;
        return qHat;
    }
    // Mass-weighted RHS B·q̂ stacked as [Re; Im] for the real-2V Cholesky.
    private static Arr<double> StackMassWeighted(int n, Complex[] qHat, Arr<double> mass) {
        double[] rhs = new double[2 * n];
        for (int v = 0; v < n; v++) {
            double m = mass[index: v];
            rhs[v] = m * qHat[v].Real;
            rhs[v + n] = m * qHat[v].Imaginary;
        }
        return new Arr<double>(rhs);
    }
    private static Arr<Complex> ReassembleComplex(int n, Arr<double> real) {
        Complex[] result = new Complex[n];
        for (int v = 0; v < n; v++)
            result[v] = new Complex(real: real[index: v], imaginary: real[index: v + n]);
        return new Arr<Complex>(result);
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
    // RoSy sample: per-vertex complex carries an N-fold rotation (unit magnitude); barycentric
    // blend averages tangent directions in the local frame, then unitises. Used by cross fields.
    private static Fin<Vector3d> InterpolateRosyAt(MeshSpace space, Point3d sample, Complex[] perVertex, int symmetry, Op key) =>
        BarycentricBlend(space: space, sample: sample, perVertex: perVertex, key: key,
            decode: (value, x, y) => DecodeRosy(value: value, xAxis: x, yAxis: y, symmetry: symmetry));
    // Tangent vector sample: per-vertex complex carries (re,im) tangent coefficients with magnitude;
    // barycentric blend preserves both direction AND magnitude. Used by VHM and Hodge projection.
    private static Fin<Vector3d> InterpolateTangentVectorAt(MeshSpace space, Point3d sample, Complex[] perVertex, Op key) =>
        BarycentricBlend(space: space, sample: sample, perVertex: perVertex, key: key,
            decode: static (value, x, y) => (value.Real * x) + (value.Imaginary * y));
    private static Fin<Vector3d> BarycentricBlend(MeshSpace space, Point3d sample, Complex[] perVertex, Op key, Func<Complex, Vector3d, Vector3d, Vector3d> decode) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        if (meshPoint is null || meshPoint.FaceIndex < 0) return Fin.Fail<Vector3d>(error: key.InvalidResult());
        MeshFace winner = space.Native.Faces[index: meshPoint.FaceIndex];
        double[] weights = meshPoint.T;
        FrameBundle fb = EnsureVertexFrames(mesh: space.Native);
        Vector3d result =
            (weights[0] * decode(perVertex[winner.A], fb.X[winner.A], fb.Y[winner.A])) +
            (weights[1] * decode(perVertex[winner.B], fb.X[winner.B], fb.Y[winner.B])) +
            (weights[2] * decode(perVertex[winner.C], fb.X[winner.C], fb.Y[winner.C])) +
            (winner.IsQuad ? weights[3] * decode(perVertex[winner.D], fb.X[winner.D], fb.Y[winner.D]) : Vector3d.Zero);
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
    internal sealed record HodgeBundle(Arr<Vector3d> Irrotational, Arr<Vector3d> Solenoidal);
    internal static Fin<Vector3d> HodgeProjectedAt(VectorField source, MeshSpace space, BoundarySense sense, Point3d sample, Op key) =>
        from bundle in EnsureHodgeBundle(source: source, space: space, key: key)
        from value in InterpolateVectorOnMesh(space: space, sample: sample, perVertex: sense.Equals(BoundarySense.Toward) ? bundle.Irrotational : bundle.Solenoidal, key: key)
        select value;
    private static Fin<HodgeBundle> EnsureHodgeBundle(VectorField source, MeshSpace space, Op key) =>
        space.Cache.Hodge(probe: new HodgeKey(Source: source),
            compute: () => ComputeHodgeBundle(source: source, space: space, key: key));
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
    // Sharp-Soliman-Crane 2019: parallel-transport vectors via short-time heat flow on the complex
    // line bundle. Three solves share the AMD-ordered Cholesky factors cached in LaplacianCache:
    //   (M + t·L_conn) X_t = X_0         (direction; real-2V SPD reformulation)
    //   (M + t·L_cot)  φ_t = |X_0|        (magnitude carrier)
    //   (M + t·L_cot)  ψ_t = δ_sources    (indicator carrier)
    // Per-vertex result = (X_t / |X_t|) · (φ_t / ψ_t).
    internal static Fin<Vector3d> VectorHeatAt(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Point3d sample, Op key) =>
        from cached in EnsureVectorHeat(space: space, sources: sources, time: time, key: key)
        from value in InterpolateTangentVectorAt(space: space, sample: sample, perVertex: cached, key: key)
        select value;
    private static Fin<Complex[]> EnsureVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) =>
        space.Cache.VectorHeat(probe: new VectorHeatKey(Time: time, Sources: sources),
            compute: () => ComputeVectorHeat(space: space, sources: sources, time: time, key: key));
    private static Fin<Complex[]> ComputeVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
        int n = space.Native.Vertices.Count;
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        return from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
               from connectionFactor in space.Cache.ConnectionCholesky(symmetry: 1, time: time, key: key)
               from scalarFactor in space.Cache.ScalarHeatCholesky(time: time, key: key)
               let rhs = EncodeVectorHeatSources(n: n, sources: sources, frames: frames, mass: laplacian.MassLumped)
               from direction in connectionFactor.Solve(rhs: rhs.StackedDirection, key: key)
               from magnitude in scalarFactor.Solve(rhs: rhs.Magnitude, key: key)
               from indicator in scalarFactor.Solve(rhs: rhs.Indicator, key: key)
               select RecoverVectorHeat(n: n, direction: direction, magnitude: magnitude, indicator: indicator);
    }
    // Mass-weighted source encoding for the three VHM systems. Each source vertex v contributes
    // mass[v]·(Re,Im) to the stacked 2V direction RHS, mass[v]·‖dir‖ to the magnitude RHS, and
    // mass[v] to the indicator RHS. The mass weighting matches the heat-method convention used
    // in `BuildSourceDelta` for scalar geodesics.
    private sealed record VectorHeatRhs(Arr<double> StackedDirection, Arr<double> Magnitude, Arr<double> Indicator);
    private static VectorHeatRhs EncodeVectorHeatSources(int n, Seq<(int Vertex, Vector3d Direction)> sources, FrameBundle frames, Arr<double> mass) {
        double[] stacked = new double[2 * n];
        double[] magnitude = new double[n];
        double[] indicator = new double[n];
        for (int s = 0; s < sources.Count; s++) {
            (int v, Vector3d direction) = sources[index: s];
            if (v < 0 || v >= n) continue;
            Vector3d projected = direction - (direction * frames.N[v] * frames.N[v]);
            double mv = mass[index: v];
            stacked[v] += mv * (projected * frames.X[v]);
            stacked[v + n] += mv * (projected * frames.Y[v]);
            magnitude[v] += mv * direction.Length;
            indicator[v] += mv;
        }
        return new VectorHeatRhs(
            StackedDirection: new Arr<double>(stacked),
            Magnitude: new Arr<double>(magnitude),
            Indicator: new Arr<double>(indicator));
    }
    private static Complex[] RecoverVectorHeat(int n, Arr<double> direction, Arr<double> magnitude, Arr<double> indicator) {
        Complex[] result = new Complex[n];
        for (int v = 0; v < n; v++) {
            Complex raw = new(real: direction[index: v], imaginary: direction[index: v + n]);
            double mag = raw.Magnitude;
            Complex unit = mag > RhinoMath.SqrtEpsilon ? raw / mag : Complex.Zero;
            double ind = indicator[index: v];
            double scale = ind > RhinoMath.SqrtEpsilon ? magnitude[index: v] / ind : 0.0;
            result[v] = unit * scale;
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
    // GeneralizedWindingNumber: Rhino IsPointInside emits the sign; magnitude = Euclidean distance
    // to the closest mesh point. Robust on closed watertight input only.
    // SignedHeat: Feng-Crane SIGGRAPH 2024 — diffuse oriented boundary normals via the Crouzeix-
    // Raviart connection Laplacian on edges, sample face barycenters, normalise, take vertex
    // divergence, Poisson-recover scalar φ; sign emerges from source orientation (NOT from
    // IsPointInside, which fails on non-watertight input).
    internal static Fin<double> SignedDistanceFromMeshAt(MeshSpace space, SdfMeshMethod method, Point3d sample, Op key) =>
        method.Equals(SdfMeshMethod.SignedHeat)
            ? SignedHeatDistance(space: space, sample: sample, key: key)
            : ClosestPointSignedDistance(space: space, sample: sample, key: key);
    private static Fin<double> ClosestPointSignedDistance(MeshSpace space, Point3d sample, Op key) {
        Mesh mesh = space.Native;
        Point3d closest = mesh.ClosestPoint(testPoint: sample);
        if (!closest.IsValid) return Fin.Fail<double>(error: key.InvalidResult());
        double distance = sample.DistanceTo(other: closest);
        bool inside = mesh.IsPointInside(point: sample, tolerance: space.Tolerance.Absolute.Value, strictlyIn: false);
        return key.AcceptValue(value: inside ? -distance : distance);
    }
    // SHM φ field — mesh-invariant (boundary topology only changes when the mesh changes, which
    // invalidates the entire LaplacianCache). Stored as Lazy<Fin<Arr<double>>> via the cache; the
    // sample-time path interpolates on the cached field.
    private static Fin<double> SignedHeatDistance(MeshSpace space, Point3d sample, Op key) {
        Polyline[]? polylines = space.Native.GetNakedEdges();
        return polylines is null || polylines.Length == 0
            ? ClosestPointSignedDistance(space: space, sample: sample, key: key)
            : from phi in space.Cache.SignedHeat
              from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: phi, key: key)
              select signed;
    }
    // Feng-Crane SHM end-to-end. Routes through the intrinsic-Delaunay edge set so L_CR is PSD
    // and Cholesky succeeds on arbitrary input; boundary edges survive IDT flips unchanged so
    // the source encoding maps cleanly to intrinsic edge indices.
    internal static Fin<Arr<double>> ComputeSignedHeat(MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        Polyline[]? polylines = mesh.GetNakedEdges();
        if (polylines is null || polylines.Length == 0) return Fin.Fail<Arr<double>>(error: key.InvalidInput());
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<Arr<double>>(error: key.InvalidResult());
        double t = 0.5 * h * (0.5 * h);
        return from imesh in space.Cache.IntrinsicMeshSnapshot
               let encoded = EncodeSignedHeatBoundarySource(mesh: mesh, imesh: imesh, polylines: polylines)
               from heatFactor in space.Cache.EdgeConnectionCholesky(time: t, key: key)
               from Xt in heatFactor.Solve(rhs: encoded.Rhs, key: key)
               let faceField = SpectralCore.SampleCrouzeixRaviartFaceField(mesh: mesh, imesh: imesh, stacked: Xt)
               let divergence = SpectralCore.ComputeIntrinsicVertexDivergence(mesh: mesh, imesh: imesh, faceFields: faceField)
               from poissonFactor in space.Cache.Cholesky
               from phi in poissonFactor.Solve(rhs: divergence, key: key)
               select ShiftSignedHeat(phi: phi, sourceVertices: encoded.SourceVertices);
    }
    // Source encoding per Feng-Crane Algorithm 11 (edge-as-source): each polyline segment maps to
    // an intrinsic boundary edge (boundary preserved through IDT), contributing X₀[e] += length · i ·
    // sign with sign tracking canonical Lo→Hi traversal. Real part stays 0; imaginary part stacks
    // into the 2|E| real RHS.
    private static (Arr<double> Rhs, Seq<int> SourceVertices) EncodeSignedHeatBoundarySource(Mesh mesh, IntrinsicMesh imesh, Polyline[] polylines) {
        int eCount = imesh.EdgeCount;
        double[] stacked = new double[2 * eCount];
        System.Collections.Generic.HashSet<int> sources = [];
        int vertexCount = mesh.Vertices.Count;
        double tolSq = RhinoMath.ZeroTolerance * 1.0e2;
        tolSq *= tolSq;
        for (int p = 0; p < polylines.Length; p++) {
            Polyline poly = polylines[p];
            int prev = FindClosestVertex(mesh: mesh, vertexCount: vertexCount, target: poly[0], tolSq: tolSq);
            if (prev >= 0) _ = sources.Add(item: prev);
            for (int q = 1; q < poly.Count; q++) {
                int curr = FindClosestVertex(mesh: mesh, vertexCount: vertexCount, target: poly[q], tolSq: tolSq);
                if (curr >= 0) _ = sources.Add(item: curr);
                if (prev >= 0 && curr >= 0) {
                    int e = imesh.IndexOfEdge(lo: prev, hi: curr);
                    if (e >= 0) {
                        IntrinsicEdge edge = imesh.EdgeAt(index: e);
                        double sign = prev == edge.Lo ? 1.0 : -1.0;
                        stacked[e + eCount] += edge.Length * sign;
                    }
                }
                prev = curr;
            }
        }
        return (Rhs: new Arr<double>(stacked), SourceVertices: toSeq(sources));
    }
    private static int FindClosestVertex(Mesh mesh, int vertexCount, Point3d target, double tolSq) {
        int best = -1;
        double bestSq = double.MaxValue;
        for (int v = 0; v < vertexCount; v++) {
            double dx = target.X - mesh.Vertices[index: v].X;
            double dy = target.Y - mesh.Vertices[index: v].Y;
            double dz = target.Z - mesh.Vertices[index: v].Z;
            double sq = (dx * dx) + (dy * dy) + (dz * dz);
            if (sq < bestSq) { bestSq = sq; best = v; }
        }
        return bestSq <= tolSq ? best : -1;
    }
    // Per geometry-central: flip sign (Laplacian is positive), then subtract average φ along the
    // boundary source so φ ≈ 0 on the boundary itself.
    private static Arr<double> ShiftSignedHeat(Arr<double> phi, Seq<int> sourceVertices) {
        int n = phi.Count;
        double[] result = new double[n];
        for (int v = 0; v < n; v++) result[v] = -phi[index: v];
        if (sourceVertices.IsEmpty) return new Arr<double>(result);
        double sum = 0.0;
        for (int s = 0; s < sourceVertices.Count; s++) sum += result[sourceVertices[index: s]];
        double mean = sum / sourceVertices.Count;
        for (int v = 0; v < n; v++) result[v] -= mean;
        return new Arr<double>(result);
    }
}
