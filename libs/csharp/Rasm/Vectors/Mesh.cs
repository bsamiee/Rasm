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
    private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
    private readonly Lazy<Fin<SparseLaplacian>> cotangent;
    private readonly Lazy<Fin<SparseLaplacian>> intrinsicDelaunay;
    private readonly Lazy<double> meanEdgeLength;
    internal Dictionary<string, object> FieldCache { get; } = [];
    private LaplacianCache(MeshSpace space) {
        Op fallback = Op.Of();
        cotangent = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: fallback));
        intrinsicDelaunay = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleIntrinsicDelaunay(mesh: space.Native, key: fallback));
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent => cotangent.Value;
    internal Fin<SparseLaplacian> IntrinsicDelaunay => intrinsicDelaunay.Value;
    internal double MeanEdgeLength => meanEdgeLength.Value;
}

[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, select: static cache => cache.Cotangent);
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, select: static cache => cache.IntrinsicDelaunay);
    [UseDelegateFromConstructor] internal partial Fin<SparseLaplacian> Select(LaplacianCache cache);
}

[Union]
public abstract partial record MeshDescriptor {
    private MeshDescriptor() { }
    public sealed record HeatKernelSignatureCase(Seq<double> Times) : MeshDescriptor;
    public sealed record WaveKernelSignatureCase(Seq<double> Energies, double Sigma) : MeshDescriptor;
    public sealed record ShapeDnaCase : MeshDescriptor;
    public static Fin<MeshDescriptor> HeatKernelSignature(Seq<double> times, Op? key = null) {
        Op op = key.OrDefault();
        return guard(!times.IsEmpty && times.ForAll(static t => RhinoMath.IsValidDouble(x: t) && t > RhinoMath.ZeroTolerance), op.InvalidInput())
            .Bind(_ => Fin.Succ<MeshDescriptor>(new HeatKernelSignatureCase(Times: times)));
    }
    public static Fin<MeshDescriptor> WaveKernelSignature(Seq<double> energies, double sigma, Op? key = null) {
        Op op = key.OrDefault();
        return guard(!energies.IsEmpty
                && energies.ForAll(static e => RhinoMath.IsValidDouble(x: e) && e > RhinoMath.ZeroTolerance)
                && RhinoMath.IsValidDouble(x: sigma)
                && sigma > RhinoMath.ZeroTolerance, op.InvalidInput())
            .Bind(_ => Fin.Succ<MeshDescriptor>(new WaveKernelSignatureCase(Energies: energies, Sigma: sigma)));
    }
    public static MeshDescriptor ShapeDna => new ShapeDnaCase();
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
    // the maximum principle per Roadmap [5.11] (obtuse triangles fail Cotangent assembly).
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
    internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry, Point3d sample, Op key) =>
        from cached in EnsureCrossField(space: space, symmetry: symmetry, key: key)
        from value in InterpolateComplexAt(space: space, sample: sample, perVertex: cached, symmetry: symmetry, key: key)
        select value;
    private static Fin<Complex[]> EnsureCrossField(MeshSpace space, int symmetry, Op key) {
        string cacheKey = string.Create(CultureInfo.InvariantCulture, $"cross|{symmetry}");
        return space.Cache.FieldCache.TryGetValue(key: cacheKey, value: out object? cached) && cached is CrossFieldCache field
            ? Fin.Succ(field.PerVertex)
            : ComputeCrossField(space: space, symmetry: symmetry, key: key)
                .Map(per => { space.Cache.FieldCache[key: cacheKey] = new CrossFieldCache(PerVertex: per); return per; });
    }
    private static Fin<Complex[]> ComputeCrossField(MeshSpace space, int symmetry, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: symmetry, key: key)
            .Bind(Lconn => Lconn.SmallestEigenpairs(k: 1, tolerance: 1e-6, maxIterations: 200, key: key))
            .Bind(pairs => pairs.Count > 0
                ? Fin.Succ(pairs[index: 0])
                : Fin.Fail<(double Eigenvalue, Arr<Complex> Eigenvector)>(error: key.InvalidResult()))
            .Map(head => NormaliseComplexEigenvector(eigenvector: head.Eigenvector));
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
}
