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
    private const double AspectRatioCeiling = 11.5;
    public static Fin<MeshSpace> Of(Mesh native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               from __ in MeshKernel.AspectRatioGuard(mesh: active, ceiling: AspectRatioCeiling, key: op)
               select new MeshSpace(native: active, tolerance: ctx);
    }
    internal LaplacianCache Cache => LaplacianCache.For(space: this);
    public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) =>
        MeshKernel.LaplacianOf(space: this, kind: kind, key: key.OrDefault());
    public Fin<int> EulerCharacteristic(Op? key = null) =>
        MeshKernel.EulerCharacteristicOf(space: this, key: key.OrDefault());
    public Fin<Seq<(int A, int B)>> FeatureEdges(double dihedralRadians, Op? key = null) =>
        MeshKernel.DetectFeatureEdgesOf(space: this, dihedralRadians: dihedralRadians, key: key.OrDefault());
    public Fin<double> MeanEdgeLength(Op? key = null) =>
        key.OrDefault().AcceptValue(value: Cache.MeanEdgeLength);
    public Fin<RTree> FaceTree(Op? key = null) =>
        key.OrDefault().AcceptValue(value: Cache.FaceTree);
}

internal sealed class LaplacianCache {
    private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
    private readonly Lazy<Fin<SparseLaplacian>> cotangent;
    private readonly Lazy<Fin<SparseLaplacian>> intrinsicDelaunay;
    private readonly Lazy<Fin<SparseLaplacian>> nonmanifold;
    private readonly Lazy<RTree> faceTree;
    private readonly Lazy<double> meanEdgeLength;
    private LaplacianCache(MeshSpace space) {
        Op fallback = Op.Of();
        cotangent = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: fallback));
        intrinsicDelaunay = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleIntrinsicDelaunay(mesh: space.Native, key: fallback));
        nonmanifold = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleNonmanifold(mesh: space.Native, key: fallback));
        faceTree = new Lazy<RTree>(valueFactory: () => RTree.CreateMeshFaceTree(mesh: space.Native));
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent => cotangent.Value;
    internal Fin<SparseLaplacian> IntrinsicDelaunay => intrinsicDelaunay.Value;
    internal Fin<SparseLaplacian> Nonmanifold => nonmanifold.Value;
    internal RTree FaceTree => faceTree.Value;
    internal double MeanEdgeLength => meanEdgeLength.Value;
}

[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, select: static cache => cache.Cotangent);
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, select: static cache => cache.IntrinsicDelaunay);
    public static readonly MeshLaplacian Nonmanifold = new(key: 2, select: static cache => cache.Nonmanifold);
    [UseDelegateFromConstructor] internal partial Fin<SparseLaplacian> Select(LaplacianCache cache);
}

[SmartEnum<int>]
public sealed partial class BoundaryCondition {
    public static readonly BoundaryCondition Neumann = new(key: 0);
    public static readonly BoundaryCondition Dirichlet = new(key: 1);
    public static readonly BoundaryCondition AverageOfBoth = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SurfaceParameterization {
    public static readonly SurfaceParameterization LSCM = new(key: 0);
    public static readonly SurfaceParameterization BFF = new(key: 1);
    public static readonly SurfaceParameterization BFFWithCones = new(key: 2);
    internal Fin<Arr<Point2d>> Compute(MeshSpace space, Seq<int> coneVertices, Op key) =>
        MeshKernel.ParameterizeFlatten(space: space, kind: this, coneVertices: coneVertices, key: key);
}

[Union]
public abstract partial record MeshDescriptor {
    private MeshDescriptor() { }
    public sealed record HeatKernelSignatureCase(Seq<double> Times) : MeshDescriptor;
    public sealed record WaveKernelSignatureCase(Seq<double> Energies, double Sigma) : MeshDescriptor;
    public sealed record ShapeDnaCase(int K) : MeshDescriptor;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MeshKernel {
    private const double DegenerateTriangleArea = 1e-14;

    // --- [VALIDATION] -----------------------------------------------------------------------
    // BOUNDARY ADAPTER — iterates the native Rhino face list to enforce the aspect-ratio
    // contract before construction. Functional fold would force materialising the whole face
    // collection into a Seq which doubles memory on large meshes.
    internal static Fin<Unit> AspectRatioGuard(Mesh mesh, double ceiling, Op key) {
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = mesh.Vertices[index: face.A];
            Point3d b = mesh.Vertices[index: face.B];
            Point3d c = mesh.Vertices[index: face.C];
            double ab = a.DistanceTo(other: b); double bc = b.DistanceTo(other: c); double ca = c.DistanceTo(other: a);
            double longest = Math.Max(val1: ab, val2: Math.Max(val1: bc, val2: ca));
            double shortest = Math.Min(val1: ab, val2: Math.Min(val1: bc, val2: ca));
            if (shortest < RhinoMath.ZeroTolerance || longest / shortest > ceiling)
                return Fin.Fail<Unit>(key.Caution(concern: $"Triangle {f} aspect ratio exceeds {ceiling}."));
        }
        return Fin.Succ(unit);
    }

    // --- [COTANGENT_ASSEMBLY] ---------------------------------------------------------------
    // BOUNDARY ADAPTER — triplet accumulation for SparseMatrix.FromTriplets demands an
    // imperative pass over Rhino's face collection; materialising into Seq first doubles
    // memory and obscures the per-face fold pattern. Negative-cotangent rejection enforces
    // the maximum principle per Roadmap [5.11] (obtuse triangles fail Cotangent assembly).
    internal static Fin<SparseLaplacian> AssembleCotangent(Mesh mesh, Op key) {
        int vertCount = mesh.Vertices.Count;
        List<(int Row, int Col, double Value)> stiffTriplets = [];
        List<(int Row, int Col, double Value)> massTriplets = [];
        double[] lumped = new double[vertCount];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            int va = face.A; int vb = face.B; int vc = face.C;
            Point3d pa = mesh.Vertices[index: va]; Point3d pb = mesh.Vertices[index: vb]; Point3d pc = mesh.Vertices[index: vc];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa; Vector3d bc = pc - pb;
            double area = 0.5 * Vector3d.CrossProduct(a: ab, b: ac).Length;
            if (area < DegenerateTriangleArea) continue;
            double cotA = -ab * -ac / (2.0 * area);
            double cotB = ab * -bc / (2.0 * area);
            double cotC = ac * bc / (2.0 * area);
            if (cotA < 0.0 || cotB < 0.0 || cotC < 0.0)
                return Fin.Fail<SparseLaplacian>(key.InvalidResult());
            stiffTriplets.Add(item: (vb, vc, 0.5 * cotA)); stiffTriplets.Add(item: (vc, vb, 0.5 * cotA));
            stiffTriplets.Add(item: (vc, va, 0.5 * cotB)); stiffTriplets.Add(item: (va, vc, 0.5 * cotB));
            stiffTriplets.Add(item: (va, vb, 0.5 * cotC)); stiffTriplets.Add(item: (vb, va, 0.5 * cotC));
            stiffTriplets.Add(item: (va, va, -0.5 * (cotB + cotC)));
            stiffTriplets.Add(item: (vb, vb, -0.5 * (cotA + cotC)));
            stiffTriplets.Add(item: (vc, vc, -0.5 * (cotA + cotB)));
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
    // Sharp-Soliman-Crane 2019 signpost-flip preprocessor stub. Maximum-principle violation
    // is enforced upstream by cotangent's negative-weight rejection, so on obtuse-rich
    // meshes this returns the same error the bare Cotangent assembly does until the full
    // intrinsic-flip pass lands.
    internal static Fin<SparseLaplacian> AssembleIntrinsicDelaunay(Mesh mesh, Op key) =>
        AssembleCotangent(mesh: mesh, key: key);
    // Sharp-Crane 2020 tufted-cover lifting stub. Cotangent's negative-weight rejection
    // ensures we surface obtuse failures rather than silently passing wrong weights.
    internal static Fin<SparseLaplacian> AssembleNonmanifold(Mesh mesh, Op key) =>
        AssembleCotangent(mesh: mesh, key: key);

    // --- [SELECTION] ------------------------------------------------------------------------
    internal static Fin<SparseLaplacian> LaplacianOf(MeshSpace space, MeshLaplacian kind, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active => active.Select(cache: space.Cache));

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
        key.AcceptValue(value: space.Native.Vertices.Count - space.Native.TopologyEdges.Count + space.Native.Faces.Count);
    // BOUNDARY ADAPTER — iterates the topology edge list to compute dihedral angles between
    // adjacent face normals; non-manifold or boundary edges (≠2 connected faces) skipped.
    internal static Fin<Seq<(int A, int B)>> DetectFeatureEdgesOf(MeshSpace space, double dihedralRadians, Op key) {
        if (!RhinoMath.IsValidDouble(x: dihedralRadians) || dihedralRadians <= 0.0)
            return Fin.Fail<Seq<(int, int)>>(key.InvalidInput());
        Mesh mesh = space.Native;
        Vector3f[] faceNormals = [.. mesh.FaceNormals];
        List<(int A, int B)> features = [];
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
            if (faces.Length != 2) continue;
            Vector3d na = (Vector3d)faceNormals[faces[0]]; Vector3d nb = (Vector3d)faceNormals[faces[1]];
            if (Vector3d.VectorAngle(a: na, b: nb) >= dihedralRadians) {
                IndexPair p = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
                features.Add(item: (p.I, p.J));
            }
        }
        return key.AcceptValue(value: toSeq(features));
    }

    // Sawhney-Crane 2018 BFF + Lévy 2002 LSCM. Full Cherrier-system solve deferred until
    // SparseHermitian factorization lands; callers see Op.Unsupported rather than wrong UV.
    internal static Fin<Arr<Point2d>> ParameterizeFlatten(MeshSpace space, SurfaceParameterization kind, Seq<int> coneVertices, Op key) =>
        Fin.Fail<Arr<Point2d>>(key.Unsupported(geometryType: typeof(SurfaceParameterization), outputType: typeof(Arr<Point2d>)));

    // --- [HEAT_METHOD] ----------------------------------------------------------------------
    // Crane-Weischedel-Wardetzky 2013: solve (M + tL)u = δ; X = -∇u/|∇u|; Lφ = ∇·X.
    // Per-source-set precomputation cached on the case-record key via ConditionalWeakTable.
    private sealed record DoubleBoxArr(Arr<double> Values);
    private static readonly ConditionalWeakTable<object, DoubleBoxArr> GeodesicCache = [];
    internal static Fin<double> HeatGeodesicAt(MeshSpace space, Seq<int> sources, BoundaryCondition boundary, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, boundary: boundary, key: key)
        from value in InterpolateOnFaceTree(space: space, sample: sample, perVertex: distances, key: key)
        select value;
    private static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, BoundaryCondition boundary, Op key) {
        object cacheKey = (space.Native, string.Join(separator: ",", values: sources.AsIterable().OrderBy(static i => i)), boundary.Key);
        return GeodesicCache.TryGetValue(key: cacheKey, value: out DoubleBoxArr? cached)
            ? Fin.Succ(cached.Values)
            : space.Laplacian(kind: MeshLaplacian.Cotangent, key: key).Bind(L => ComputeHeatGeodesic(space: space, laplacian: L, sources: sources, key: key))
                .Map(d => { GeodesicCache.AddOrUpdate(key: cacheKey, value: new DoubleBoxArr(Values: d)); return d; });
    }
    private static Fin<Arr<double>> ComputeHeatGeodesic(MeshSpace space, SparseLaplacian laplacian, Seq<int> sources, Op key) {
        int n = space.Native.Vertices.Count;
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<Arr<double>>(key.InvalidResult());
        double t = h * h;
        // Build A = M_lumped + t * (-L_stiff) since our Stiffness is the negative of the standard
        // Laplacian convention used in Crane-Weischedel-Wardetzky. (M_lumped[i] on diagonal.)
        List<(int Row, int Col, double Value)> aTriplets = [];
        for (int i = 0; i < n; i++) aTriplets.Add(item: (i, i, laplacian.MassLumped[index: i]));
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++) {
                int j = laplacian.Stiffness.ColInd[index: k];
                aTriplets.Add(item: (i, j, -t * laplacian.Stiffness.Values[index: k]));
            }
        Dimension dim = Dimension.Create(value: n);
        return from A in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: aTriplets, key: key)
               from chol in A.DecomposeCholesky(key: key)
               from delta in Fin.Succ(BuildSourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
               from u in chol.Solve(rhs: delta, key: key)
               from gradient in Fin.Succ(ComputeTriangleGradients(mesh: space.Native, u: u))
               from divergence in Fin.Succ(ComputeVertexDivergence(mesh: space.Native, gradients: gradient))
               from poissonChol in space.Laplacian(kind: MeshLaplacian.Cotangent, key: key)
                   .Bind(L => SparseMatrix.FromTriplets(rows: dim, cols: dim,
                       triplets: PoissonTriplets(L: L), key: key).Bind(P => P.DecomposeCholesky(key: key)))
               from phi in poissonChol.Solve(rhs: divergence, key: key)
               select NormalizeToZero(values: phi);
    }
    private static Arr<double> BuildSourceDelta(int n, Seq<int> sources, Arr<double> mass) {
        double[] delta = new double[n];
        foreach (int s in sources.AsIterable()) delta[s] = mass[index: s];
        return new Arr<double>(delta);
    }
    private static List<(int Row, int Col, double Value)> PoissonTriplets(SparseLaplacian L) {
        List<(int Row, int Col, double Value)> result = [];
        for (int i = 0; i < L.Stiffness.Rows.Value; i++)
            for (int k = L.Stiffness.RowPtr[index: i]; k < L.Stiffness.RowPtr[index: i + 1]; k++)
                result.Add(item: (i, L.Stiffness.ColInd[index: k], -L.Stiffness.Values[index: k]));
        // Pin vertex 0 to break the null space of the Laplacian (constant mode).
        result.Add(item: (0, 0, 1.0e10));
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
    // BOUNDARY ADAPTER — face RTree returns nearest face index; we barycentric-interpolate
    // the per-vertex scalar across that triangle.
    private static Fin<double> InterpolateOnFaceTree(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) {
        int bestFace = -1; double bestDist = double.MaxValue;
        _ = space.Cache.FaceTree.Search(sphere: new Sphere(center: sample, radius: space.Cache.MeanEdgeLength * 4.0),
            callback: (_, args) => {
                MeshFace face = space.Native.Faces[index: args.Id];
                if (!face.IsTriangle) return;
                Point3d va = space.Native.Vertices[index: face.A]; Point3d vb = space.Native.Vertices[index: face.B]; Point3d vc = space.Native.Vertices[index: face.C];
                Point3d centroid = new(x: (va.X + vb.X + vc.X) / 3.0, y: (va.Y + vb.Y + vc.Y) / 3.0, z: (va.Z + vb.Z + vc.Z) / 3.0);
                double d = centroid.DistanceToSquared(other: sample);
                if (d < bestDist) { bestDist = d; bestFace = args.Id; }
            });
        if (bestFace < 0) return Fin.Fail<double>(key.InvalidResult());
        MeshFace winner = space.Native.Faces[index: bestFace];
        Point3d a = space.Native.Vertices[index: winner.A]; Point3d b = space.Native.Vertices[index: winner.B]; Point3d c = space.Native.Vertices[index: winner.C];
        (double wa, double wb, double wc) = BarycentricCoordinates(p: sample, a: a, b: b, c: c);
        return key.AcceptValue(value: (wa * perVertex[index: winner.A]) + (wb * perVertex[index: winner.B]) + (wc * perVertex[index: winner.C]));
    }
    private static (double Wa, double Wb, double Wc) BarycentricCoordinates(Point3d p, Point3d a, Point3d b, Point3d c) {
        Vector3d v0 = b - a; Vector3d v1 = c - a; Vector3d v2 = p - a;
        double d00 = v0 * v0; double d01 = v0 * v1; double d11 = v1 * v1;
        double d20 = v2 * v0; double d21 = v2 * v1;
        double denom = (d00 * d11) - (d01 * d01);
        if (Math.Abs(value: denom) < RhinoMath.ZeroTolerance) return (1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0);
        double wb = ((d11 * d20) - (d01 * d21)) / denom;
        double wc = ((d00 * d21) - (d01 * d20)) / denom;
        return (1.0 - wb - wc, wb, wc);
    }

    // --- [MEAN_CURVATURE_FLOW] --------------------------------------------------------------
    // Desbrun-Meyer-Schröder-Barr 1999: implicit Euler step (M + dt L) X' = M X. We iterate
    // on a copy of vertex positions and return the per-vertex magnitude of (X' - X) as the
    // scalar field — a proxy for total displacement induced by the flow.
    private static readonly ConditionalWeakTable<object, DoubleBoxArr> McfCache = [];
    internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key) =>
        from displacements in EnsureMcfDisplacements(space: space, timeStep: timeStep, iterations: iterations, key: key)
        from value in InterpolateOnFaceTree(space: space, sample: sample, perVertex: displacements, key: key)
        select value;
    private static Fin<Arr<double>> EnsureMcfDisplacements(MeshSpace space, double timeStep, int iterations, Op key) {
        object cacheKey = (space.Native, timeStep, iterations);
        return McfCache.TryGetValue(key: cacheKey, value: out DoubleBoxArr? cached)
            ? Fin.Succ(cached.Values)
            : space.Laplacian(kind: MeshLaplacian.Cotangent, key: key)
                .Bind(L => ComputeMeanCurvatureFlow(space: space, laplacian: L, timeStep: timeStep, iterations: iterations, key: key))
                .Map(d => { McfCache.AddOrUpdate(key: cacheKey, value: new DoubleBoxArr(Values: d)); return d; });
    }
    private static Fin<Arr<double>> ComputeMeanCurvatureFlow(MeshSpace space, SparseLaplacian laplacian, double timeStep, int iterations, Op key) {
        int n = space.Native.Vertices.Count;
        Dimension dim = Dimension.Create(value: n);
        List<(int Row, int Col, double Value)> aTriplets = [];
        for (int i = 0; i < n; i++) aTriplets.Add(item: (i, i, laplacian.MassLumped[index: i]));
        for (int i = 0; i < n; i++)
            for (int k = laplacian.Stiffness.RowPtr[index: i]; k < laplacian.Stiffness.RowPtr[index: i + 1]; k++) {
                int j = laplacian.Stiffness.ColInd[index: k];
                aTriplets.Add(item: (i, j, -timeStep * laplacian.Stiffness.Values[index: k]));
            }
        return from A in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: aTriplets, key: key)
               from chol in A.DecomposeCholesky(key: key)
               from final in IterateMcf(space: space, mass: laplacian.MassLumped, chol: chol, iterations: iterations, key: key)
               select ComputeDisplacements(original: space.Native, smoothed: final);
    }
    private static Fin<(double[] X, double[] Y, double[] Z)> IterateMcf(MeshSpace space, Arr<double> mass, CholeskyResult chol, int iterations, Op key) {
        int n = space.Native.Vertices.Count;
        double[] x = new double[n]; double[] y = new double[n]; double[] z = new double[n];
        for (int i = 0; i < n; i++) { Point3d v = space.Native.Vertices[index: i]; x[i] = v.X; y[i] = v.Y; z[i] = v.Z; }
        Fin<Unit> rail = Fin.Succ(unit);
        for (int iter = 0; iter < iterations; iter++) {
            double[] rhsX = new double[n]; double[] rhsY = new double[n]; double[] rhsZ = new double[n];
            for (int i = 0; i < n; i++) { rhsX[i] = mass[index: i] * x[i]; rhsY[i] = mass[index: i] * y[i]; rhsZ[i] = mass[index: i] * z[i]; }
            Fin<Arr<double>> sx = chol.Solve(rhs: new Arr<double>(rhsX), key: key);
            Fin<Arr<double>> sy = chol.Solve(rhs: new Arr<double>(rhsY), key: key);
            Fin<Arr<double>> sz = chol.Solve(rhs: new Arr<double>(rhsZ), key: key);
            Fin<(Arr<double> X, Arr<double> Y, Arr<double> Z)> step = from a in sx from b in sy from c in sz select (X: a, Y: b, Z: c);
            if (step.IsFail) return Fin.Fail<(double[], double[], double[])>(key.InvalidResult());
            (Arr<double> nx, Arr<double> ny, Arr<double> nz) = step.IfFail(((Arr<double>, Arr<double>, Arr<double>))default);
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

    // --- [VECTOR_HEAT] ----------------------------------------------------------------------
    // Sharp-Soliman-Crane 2019 vector heat method requires a complex Hermitian connection
    // Laplacian factorization; the SparseHermitian factor pathway from Phase 0 is not yet
    // wired into a Cholesky implementation, so this surface fails honestly until Phase 7.
    internal static Fin<Vector3d> VectorHeatAt(MeshSpace space, Seq<(int Vertex, Vector3d Tangent)> seeds, Point3d sample, Op key) =>
        Fin.Fail<Vector3d>(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(Vector3d)));

    // Knöppel-Crane-Pinkall-Schröder 2013 N-RoSy cross-field via LOBPCG on the Hermitian
    // connection Laplacian. SparseHermitian Cholesky from Phase 0 is not yet integrated with
    // the LOBPCG generalised path, so the public surface fails until the assembly lands.
    internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry, Option<TensorField> guidance, Point3d sample, Op key) =>
        Fin.Fail<Vector3d>(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(Vector3d)));
}
