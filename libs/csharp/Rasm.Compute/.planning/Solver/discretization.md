# [COMPUTE_DISCRETIZATION]

Rasm.Compute solver discretization: one volumetric `MeshKernel` owner generating tet/hex/boundary-layer meshes from a boundary `BoundaryShell` through real Delaunay/octree/sweep/inflation cores with adaptive h/p/hp refinement, one `ElementClass` `[SmartEnum<string>]` element-topology axis carrying its reference-node table, its monomial polynomial space, and a `ShapeFamily` discriminant that drives one isoparametric `Sample` so twelve continuum element types collapse onto a Vandermonde coefficient mechanism plus an explicit serendipity arm and a rational pyramid arm — and the owned Frame family (`beam2-euler`/`beam2-timoshenko`, the 2-node 12-DOF member rows whose `Member` closed form carries end releases by static condensation, rigid-end offsets by eccentricity transform, and semi-rigid end springs by exact in-series condensation — the owned replacement for the retired BFE/FEALiTE frame backends, exceeding their consumed set), one closed `MeshMetric` Verdict quality vocabulary read once per element over the real edge/face topology, and one `FieldSpace` over `FieldStation` rows as the solve-native scalar/vector/tensor representation. This page owns the `ComparerAccessors.StringOrdinal` accessor, the `Monomial`/`ShapeSample`/`Aabb` value types, the `ElementClass`/`MeshAlgorithm`/`MeshMetric`/`FieldStation` vocabulary, the `QuadratureRule` owned-build Gauss tables, the `BoundaryShell`/`MeshPolicy`/`DiscreteMesh`/`FieldSpace` carriers, and the `MeshKernel` generation+refinement fold; raw element-node memory projects through `TensorMarshal.CreateReadOnlyTensorSpan` without an owner copy, and assembly emits `SparseCompressedRowMatrixStorage<double>` through `Tensor/factor`, metric reductions ride `Tensor/dispatch` `TensorPrimitives` folds, MathNet `Matrix<double>.Inverse` factors the one-time per-class Vandermonde, every Delaunay SIGN DECISION routes the kernel exact-predicate floor through the coordinate-level `Predicate.Orient3D`/`Predicate.InSphere` cores (`Rasm/Numerics/predicates` — raw double tuples, so no kernel value type enters a lane signature; the hand-rolled Bowyer-Watson TOPOLOGY stays owned, only the sign path is the kernel's), and the `ComputeReceipt` rail, `WorkLane`/`Substrate`/`AllocationClass`, `CorrelationId`, and `ClockPolicy` arrive settled. `DiscreteMesh` and `FieldSpace` cross to `Solver/contract` as the assembly substrate, and surface-mesh boundary triangulation is the host `Mesh.CreateFromBrep`→`Rasm.Meshing` `MeshSpace.Of(Mesh, Context)` wire flattened to the `BoundaryShell` triangle soup, composed never re-derived.

## [01]-[INDEX]

- [01]-[DISCRETIZATION_MESH]: volumetric mesher; tet/hex/boundary-layer; isoparametric shape functions; Verdict quality.

## [02]-[DISCRETIZATION_MESH]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `ElementClass` `[SmartEnum<string>]` element-topology rows carrying a `ShapeFamily` discriminant, the reference-node natural-coordinate table, the `Monomial` polynomial-space basis, the corner/edge/face topology tables, and the quadrature rule, all driving one isoparametric `Sample` returning shape values, physical gradients, and the Jacobian determinant; `MeshAlgorithm` `[SmartEnum<string>]` generation-strategy rows carrying a `MeshStrategy` core selector, a `PointSource` interior-seed column, and a conforming flag; `MeshMetric` `[SmartEnum<string>]` closed Verdict quality vocabulary (scaled-Jacobian, aspect-ratio, skewness, min-dihedral, condition) reading the real corner/edge/face topology; `FieldStation` `[SmartEnum<string>]` nodal/integration-point/cell/boundary rows carrying their count derivation; `MeshKernel` static surface generating a `DiscreteMesh` from a boundary `BoundaryShell` then refining it adaptively; `DiscreteMesh` the conforming/non-conforming volumetric mesh carrier; `FieldSpace` the integration-point/nodal scalar/vector/tensor field the solve writes; `QuadratureRule` the owned-build Gauss table the element class indexes; `BoundaryShell` the boundary-triangulation carrier with the ray-cast inclusion test; `Aabb`/`Monomial`/`ShapeSample` the value types.
- Cases: `ElementClass` rows tet4 · tet10 · hex8 · hex20 · hex27 · wedge6 · wedge18 · pyramid5 · tri3 · tri6 · quad4 · quad8 · beam2-euler · beam2-timoshenko over four `ShapeFamily` arms (Polynomial via the Vandermonde monomial mechanism, Reduced via the explicit serendipity corner/midside formulas, Pyramid via the rational apex basis, Frame via the closed-form 12-DOF `Member` stiffness the solve contract scatters — releases/offsets/semi-rigid springs as row behavior, the `Shear` column selecting the Timoshenko Φ terms); `MeshAlgorithm` rows delaunay · frontal-delaunay · advancing-front · octree · sweep · boundary-layer over four `MeshStrategy` cores (Delaunay/Octree/Sweep/Inflation) and four `PointSource` seeds (boundary/lattice/frontal/front); `MeshMetric` rows scaled-jacobian · aspect-ratio · skewness · min-dihedral · condition; `FieldStation` rows nodal · integration-point · cell · boundary; `FieldSpace` rank rows scalar · vector · tensor over `FieldStation` positions.
- Entry: `public static Fin<DiscreteMesh> Discretize(BoundaryShell boundary, MeshPolicy policy, ClockPolicy clocks)` — `BoundaryShell.Validate` rejects malformed buffers, invalid indices, degenerate triangles, open edges, and inconsistent winding before generation; `MeshPolicy.Validate` rejects incoherent strategy/element and numeric policy values; `Fin<T>` then aborts on generation failure or an element failing the metric's directional quality threshold through `MeshMetric.Admits`; `Refine(DiscreteMesh, MeshPolicy, ReadOnlySpan<double> cellError, ClockPolicy)` re-meshes the Dörfler-marked cell set by the keyless `RefineKind` `H` (red subdivision), `P` (order elevation), or `Hp` (graded) axis returning the adapted mesh and the carried error estimator; `Quality(DiscreteMesh, MeshMetric)` reads the per-element metric once; `ElementClass.Sample((double, double, double) natural, ReadOnlySpan<double> nodalXyz)` is the isoparametric evaluation the assembly consumes and `ShapeGrad` is its gradient projection.
- Auto: `Discretize` routes the `MeshStrategy` core by the algorithm row — a closed manifold solid routes the Bowyer-Watson `Delaunay` tetrahedralization over the boundary surface nodes plus the `PointSource` interior seeds, a feature-graded fill routes the `Octree` hex recursion, a sweepable prism routes `Sweep` extrusion of the boundary cross-section, and a viscous wall routes the `Inflation` wall-normal anisotropic graded hex; every core filters cells by the `BoundaryShell.Encloses` ray-cast and packs the conforming `DiscreteMesh`; `Sample` evaluates the `ShapeFamily` arm — Polynomial reads the lazily-memoized per-class Vandermonde coefficient matrix `(N_i = Σ_m C[m,i]·P_m(ξ))` and its monomial derivatives, Reduced reads the explicit serendipity corner/midside formulas, Pyramid the rational apex basis — then maps reference derivatives through the inline `dim×dim` Jacobian inverse to physical `∂N/∂x` and the determinant; `Refine` reads the per-cell error estimator and marks the cells whose estimator exceeds the policy fraction by the Dörfler bulk criterion, then either red-subdivides (h) the marked set expanded to its edge-conforming closure — any cell sharing a split edge joins the set to a fixpoint unless the mortar column carries the hanging node — or globally order-elevates (p) the element order — the marked set drives the hp routing decision while a uniform-order mesh elevates wholesale — through the shared edge-midpoint map so the interior stays conforming and a hanging node rides the mortar column only when the policy sets it; `Quality` folds the requested `MeshMetric` over the element set through the element class's `Metric` delegate, never a per-call recompute.
- Receipt: the `Discretization` `ComputeReceipt` case carries the algorithm key, element-class key, node and element counts, the boundary-layer count, the worst-element quality scalar, the chosen metric key, and elapsed; `Refine` stamps the refinement level, the marked-cell count, the marking fraction, and the post-refine error estimator on the same case so an adaptive sweep is one receipt chain by correlation.
- Packages: Rasm (project), MathNet.Numerics, CommunityToolkit.HighPerformance, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new element topology is one `ElementClass` row carrying its `ShapeFamily`, reference-node table, monomial space, and corner/edge/face tables; a new generation strategy is one `MeshAlgorithm` row carrying its `MeshStrategy`/`PointSource` columns plus its core; a new quality measure is one `MeshMetric` row carrying its per-element delegate; a new field rank is one `FieldSpace` rank row; a new Gauss order is one `QuadratureRule` entry; zero new surface.
- Boundary: the mesher is the volumetric discretization owner the FEA/CFD solve consumes — the boundary triangulation enters as the `BoundaryShell` triangle soup the host `Mesh.CreateFromBrep(brep, MeshingParameters)` tessellation produces — wrapped through the `Rasm.Meshing` `MeshSpace.Of(Mesh, Context)` owner (the `Brep` coerced via the `Rasm` `Domain` `GeometryRequest.BrepForm` owner) and flattened to the `Vertices`/`Triangles` soup at the boundary (`[03]-[BOUNDARY_TRIANGULATION]`) — and the inclusion test is the owned ray-cast, so this kernel never re-derives a surface mesher and never leaks a host geometry type into a solve signature; the element shape functions and `B`-matrix are the `ElementClass.Sample` isoparametric evaluation dispatching on `ShapeFamily` — the Polynomial arm collapses tet4/tet10/hex8/hex27/wedge6/wedge18/tri3/tri6/quad4 onto one Vandermonde monomial mechanism keyed by the per-row reference-node table and `Monomial` space (a singular per-element shape-function reimplementation is the deleted form, and the prior single trilinear stencil reused across every curvilinear topology is the named illusory defect), the Reduced arm carries the explicit quad8/hex20 serendipity corner/midside formulas, and the Pyramid arm the rational apex basis whose `(1−ζ)` denominator the conical quadrature avoids; the element owns its integration scaling — `Sample.DetJ` is the Jacobian determinant the assembly weights each Gauss point by, never a centroid-volume approximation; the quadrature is the owned-build symmetric/tensor table per reference domain (triangle/tet area-volume coordinates, `[-1,1]` cube tensor Gauss, triangle⊗line prism, conical pyramid) and a 2D element indexing a 3D rule is the named defect the rebuild closes, the canonical 1/2/3-point Gauss-Legendre nodes built as owned compile-time `static readonly` constants and the tensor/prism/conical rules composed from them so a per-element runtime `GaussLegendreRule(−1, 1, order)` construction is the avoided allocation; the quality measure is the closed Verdict `MeshMetric` SmartEnum read once through the element class's `Metric` delegate over the real corner-Jacobian, edge-length, face-angle, and dihedral topology, never a per-call recompute, never the first-four-nodes slice, and never a parallel quality type; the generation strategy is real per row — Bowyer-Watson incremental Delaunay with the orientation-robust in-sphere predicate, graded octree recursion, boundary-cross-section sweep extrusion, and wall-normal anisotropic inflation — so the prior bounding-box voxelization masquerading as six unstructured meshers is the deleted form; adaptive refinement is conforming red subdivision through the shared edge-midpoint map by default and non-conforming only when the policy mortar column is set, a hanging node without a constraint row is the rejected form, and the prior cell-duplication subdivision is the named fake; the mesh is solve-native raw SI `double` (the typed `MeasureValue`/`Dimension` vocabulary lives at the `Rasm.Element/Properties/quantity#MEASURE_VALUE` seam and is admitted once upstream, never threaded through this hot numeric kernel); the metric reductions ride the `Tensor/dispatch#KERNEL_DISPATCH` `TensorPrimitives.Min`/`Max` SIMD folds over the flat per-element span, MathNet factors only the cold per-class Vandermonde inverse, the in-sphere/orientation SIGN DECISIONS route the kernel `Rasm/Numerics/predicates` coordinate-level exact cores (`Predicate.Orient3D`/`Predicate.InSphere` over raw double tuples with the `Sign.Times` orientation-normalization fold — near-coplanar/cocircular building geometry decides exactly, and the float `Orient`/`InSphere`/`Det4` sign path is the deleted re-owned-kernel-geometry defect), the local boundary carrier is `BoundaryShell` — named OFF the kernel Vectors `MeshSpace` it flattens from (a re-declared soup carrier under a frozen kernel type's name is the deleted form) — and the inline `dim×dim` Jacobian inverse is this page's named kernel exemption.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ShapeFamily {
    public static readonly ShapeFamily Polynomial = new(static (element, natural, nodalXyz) => element.PolynomialSample(natural, nodalXyz));
    public static readonly ShapeFamily Reduced = new(static (element, natural, nodalXyz) => element.ReducedSample(natural, nodalXyz));
    public static readonly ShapeFamily Pyramid = new(static (element, natural, nodalXyz) =>
        Topology.Iso(element.PyramidShape(natural), element.PyramidGrad(natural), nodalXyz, element.Nodes, element.Dim));
    public static readonly ShapeFamily Frame = new(static (_, natural, nodalXyz) => ElementClass.LineSample(natural, nodalXyz));

    [UseDelegateFromConstructor]
    public partial ShapeSample Sample(ElementClass element, (double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz);
}

[SmartEnum]
public sealed partial class MeshStrategy {
    public static readonly MeshStrategy Delaunay = new(static (boundary, policy) => DelaunayCore.Fill(boundary, policy));
    public static readonly MeshStrategy Octree = new(static (boundary, policy) => OctreeCore.Fill(boundary, policy));
    public static readonly MeshStrategy Sweep = new(static (boundary, policy) => SweepCore.Fill(boundary, policy));
    public static readonly MeshStrategy Inflation = new(static (boundary, policy) => InflationCore.Fill(boundary, policy));

    [UseDelegateFromConstructor]
    public partial MeshBuild Fill(BoundaryShell boundary, MeshPolicy policy);
}

[SmartEnum]
public sealed partial class PointSource {
    public static readonly PointSource Boundary = new(static (target, _) => target);
    public static readonly PointSource Lattice = new(static (target, _) => target);
    public static readonly PointSource Frontal = new(static (target, _) => target * 0.75);
    public static readonly PointSource Front = new(static (target, grading) => target * grading);

    [UseDelegateFromConstructor]
    public partial double Spacing(double target, double grading);
}

[SmartEnum]
public sealed partial class RefineKind {
    public static readonly RefineKind H = new();
    public static readonly RefineKind P = new();
    public static readonly RefineKind Hp = new();
}

public readonly record struct Monomial(int I, int J, int K) {
    public double Eval((double X, double Y, double Z) p) => Pow(p.X, I) * Pow(p.Y, J) * Pow(p.Z, K);

    public double D(int axis, (double X, double Y, double Z) p) => axis switch {
        0 => I == 0 ? 0.0 : I * Pow(p.X, I - 1) * Pow(p.Y, J) * Pow(p.Z, K),
        1 => J == 0 ? 0.0 : J * Pow(p.X, I) * Pow(p.Y, J - 1) * Pow(p.Z, K),
        _ => K == 0 ? 0.0 : K * Pow(p.X, I) * Pow(p.Y, J) * Pow(p.Z, K - 1),
    };

    static double Pow(double b, int e) => e <= 0 ? 1.0 : e == 1 ? b : Math.Pow(b, e);
}

public readonly record struct Aabb(Vector3 Lo, Vector3 Hi) {
    public Vector3 Span => Hi - Lo;
    public Vector3 Center => (Lo + Hi) * 0.5f;

    public static Aabb Of(ReadOnlySpan<float> vertices) {
        Vector3 lo = new(float.MaxValue), hi = new(float.MinValue);
        for (int v = 0; v + 2 < vertices.Length; v += 3) {
            Vector3 p = new(vertices[v], vertices[v + 1], vertices[v + 2]);
            lo = Vector3.Min(lo, p); hi = Vector3.Max(hi, p);
        }
        return new(lo, hi);
    }
}

// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct ShapeSample(double[] Shape, double[] Grad, double DetJ);

public readonly record struct QuadratureRule(int Order, int Dimension, ImmutableArray<(double X, double Y, double Z, double Weight)> Points) {
    static readonly ImmutableArray<(double Node, double Weight)> Gauss1 = [(0.0, 2.0)];
    static readonly ImmutableArray<(double Node, double Weight)> Gauss2 = [(-1.0 / Math.Sqrt(3.0), 1.0), (1.0 / Math.Sqrt(3.0), 1.0)];
    static readonly ImmutableArray<(double Node, double Weight)> Gauss3 = [(-Math.Sqrt(0.6), 5.0 / 9.0), (0.0, 8.0 / 9.0), (Math.Sqrt(0.6), 5.0 / 9.0)];

    public static readonly QuadratureRule Line2 = new(2, 1, [.. Gauss2.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    public static readonly QuadratureRule Tri1 = new(1, 2, [(1.0 / 3.0, 1.0 / 3.0, 0.0, 0.5)]);
    public static readonly QuadratureRule Tri3 = new(2, 2, [
        (1.0 / 6.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (2.0 / 3.0, 1.0 / 6.0, 0.0, 1.0 / 6.0), (1.0 / 6.0, 2.0 / 3.0, 0.0, 1.0 / 6.0)]);
    public static readonly QuadratureRule Tet1 = new(1, 3, [(0.25, 0.25, 0.25, 1.0 / 6.0)]);
    public static readonly QuadratureRule Tet4 = new(2, 3, [.. Simplex3([0.5854101966249685, 0.1381966011250105])]);
    public static readonly QuadratureRule Quad1 = TensorCube(2, Gauss1);
    public static readonly QuadratureRule Quad4 = TensorCube(2, Gauss2);
    public static readonly QuadratureRule Quad9 = TensorCube(2, Gauss3);
    public static readonly QuadratureRule Hex1 = TensorCube(3, Gauss1);
    public static readonly QuadratureRule Hex8 = TensorCube(3, Gauss2);
    public static readonly QuadratureRule Hex27 = TensorCube(3, Gauss3);
    public static readonly QuadratureRule Wedge6 = PrismProduct(Tri3, Gauss2);
    public static readonly QuadratureRule Wedge18 = PrismProduct(Tri3, Gauss3);
    public static readonly QuadratureRule Pyramid5 = Conical(2);

    static IEnumerable<(double, double, double, double)> Simplex3(double[] ab) {
        (double a, double b) = (ab[0], ab[1]);
        yield return (a, b, b, 1.0 / 24.0); yield return (b, a, b, 1.0 / 24.0);
        yield return (b, b, a, 1.0 / 24.0); yield return (b, b, b, 1.0 / 24.0);
    }

    static QuadratureRule TensorCube(int dim, ImmutableArray<(double Node, double Weight)> line) {
        List<(double, double, double, double)> rows = [];
        int n = line.Length;
        for (int k = 0; k < (dim == 3 ? n : 1); k++)
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++) {
                    double w = line[i].Weight * line[j].Weight * (dim == 3 ? line[k].Weight : 1.0);
                    rows.Add((line[i].Node, line[j].Node, dim == 3 ? line[k].Node : 0.0, w));
                }
        return new(2 * n - 1, dim, [.. rows]);
    }

    static QuadratureRule PrismProduct(QuadratureRule tri, ImmutableArray<(double Node, double Weight)> line) {
        List<(double, double, double, double)> rows = [];
        foreach ((double X, double Y, double Z, double Weight) point in tri.Points)
            foreach ((double node, double weight) in line) { rows.Add((point.X, point.Y, node, point.Weight * weight)); }
        return new(tri.Order + line.Length, 3, [.. rows]);
    }

    static QuadratureRule Conical(int n) {
        ImmutableArray<(double Node, double Weight)> baseLine = n == 2 ? Gauss2 : Gauss3;
        (double, double)[] zeta = [(0.1127016653792583, 0.2777777777777778), (0.5, 0.4444444444444444), (0.8872983346207417, 0.2777777777777778)];
        List<(double, double, double, double)> rows = [];
        foreach ((double z, double wz) in zeta) {
            double scale = 1.0 - z;
            foreach ((double bj, double wj) in baseLine)
                foreach ((double bi, double wi) in baseLine) { rows.Add((bi * scale, bj * scale, z, wi * wj * wz * scale * scale)); }
        }
        return new(5, 3, [.. rows]);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElementClass {
    public static readonly ElementClass Tet4 = new("tet4", ShapeFamily.Polynomial, dim: 3, order: 1, volumetric: true,
        QuadratureRule.Tet1, Topology.TetRef4, Topology.TetP1, Topology.TetEdges, Topology.TetFaces, [0, 1, 2, 3], () => Tet10);
    public static readonly ElementClass Tet10 = new("tet10", ShapeFamily.Polynomial, dim: 3, order: 2, volumetric: true,
        QuadratureRule.Tet4, Topology.TetRef10, Topology.TetP2, Topology.TetEdges, Topology.TetFaces, [0, 1, 2, 3], () => Tet10);
    public static readonly ElementClass Hex8 = new("hex8", ShapeFamily.Polynomial, dim: 3, order: 1, volumetric: true,
        QuadratureRule.Hex8, Topology.HexRef8, Topology.HexQ1, Topology.HexEdges, Topology.HexFaces, [.. Enumerable.Range(0, 8)], () => Hex20);
    public static readonly ElementClass Hex20 = new("hex20", ShapeFamily.Reduced, dim: 3, order: 2, volumetric: true,
        QuadratureRule.Hex27, Topology.HexRef20, ImmutableArray<Monomial>.Empty, Topology.HexEdges, Topology.HexFaces, [.. Enumerable.Range(0, 8)], () => Hex20);
    public static readonly ElementClass Hex27 = new("hex27", ShapeFamily.Polynomial, dim: 3, order: 2, volumetric: true,
        QuadratureRule.Hex27, Topology.HexRef27, Topology.HexQ2, Topology.HexEdges, Topology.HexFaces, [.. Enumerable.Range(0, 8)], () => Hex27);
    public static readonly ElementClass Wedge6 = new("wedge6", ShapeFamily.Polynomial, dim: 3, order: 1, volumetric: true,
        QuadratureRule.Wedge6, Topology.WedgeRef6, Topology.WedgeP1, Topology.WedgeEdges, Topology.WedgeFaces, [.. Enumerable.Range(0, 6)], () => Wedge6);
    public static readonly ElementClass Wedge18 = new("wedge18", ShapeFamily.Polynomial, dim: 3, order: 2, volumetric: true,
        QuadratureRule.Wedge18, Topology.WedgeRef18, Topology.WedgeP2, Topology.WedgeEdges, Topology.WedgeFaces, [.. Enumerable.Range(0, 6)], () => Wedge18);
    public static readonly ElementClass Pyramid5 = new("pyramid5", ShapeFamily.Pyramid, dim: 3, order: 1, volumetric: true,
        QuadratureRule.Pyramid5, Topology.PyramidRef5, ImmutableArray<Monomial>.Empty, Topology.PyramidEdges, Topology.PyramidFaces, [.. Enumerable.Range(0, 5)], () => Pyramid5);
    public static readonly ElementClass Tri3 = new("tri3", ShapeFamily.Polynomial, dim: 2, order: 1, volumetric: false,
        QuadratureRule.Tri3, Topology.TriRef3, Topology.TriP1, Topology.TriEdges, Topology.TriFaces, [0, 1, 2], () => Tri6);
    public static readonly ElementClass Tri6 = new("tri6", ShapeFamily.Polynomial, dim: 2, order: 2, volumetric: false,
        QuadratureRule.Tri3, Topology.TriRef6, Topology.TriP2, Topology.TriEdges, Topology.TriFaces, [0, 1, 2], () => Tri6);
    public static readonly ElementClass Quad4 = new("quad4", ShapeFamily.Polynomial, dim: 2, order: 1, volumetric: false,
        QuadratureRule.Quad4, Topology.QuadRef4, Topology.QuadQ1, Topology.QuadEdges, Topology.QuadFaces, [.. Enumerable.Range(0, 4)], () => Quad8);
    public static readonly ElementClass Quad8 = new("quad8", ShapeFamily.Reduced, dim: 2, order: 2, volumetric: false,
        QuadratureRule.Quad9, Topology.QuadRef8, ImmutableArray<Monomial>.Empty, Topology.QuadEdges, Topology.QuadFaces, [.. Enumerable.Range(0, 4)], () => Quad8);
    public static readonly ElementClass Beam2Euler = new("beam2-euler", ShapeFamily.Frame, dim: 1, order: 1, volumetric: false,
        QuadratureRule.Line2, Topology.LineRef2, Topology.LineP1, Topology.LineEdges, ImmutableArray<ImmutableArray<int>>.Empty, [0, 1], () => Beam2Euler, shear: false);
    public static readonly ElementClass Beam2Timoshenko = new("beam2-timoshenko", ShapeFamily.Frame, dim: 1, order: 1, volumetric: false,
        QuadratureRule.Line2, Topology.LineRef2, Topology.LineP1, Topology.LineEdges, ImmutableArray<ImmutableArray<int>>.Empty, [0, 1], () => Beam2Timoshenko, shear: true);

    public ShapeFamily Family { get; }
    public bool Shear { get; }
    public int Dim { get; }
    public int Order { get; }
    public bool Volumetric { get; }
    public QuadratureRule Quadrature { get; }
    public ImmutableArray<(double X, double Y, double Z)> Reference { get; }
    public ImmutableArray<Monomial> Basis { get; }
    public ImmutableArray<(int A, int B)> Edges { get; }
    public ImmutableArray<ImmutableArray<int>> Faces { get; }
    public ImmutableArray<int> Corners { get; }

    public int Nodes => Reference.Length;
    public ElementClass Elevate => elevate();

    private readonly Func<ElementClass> elevate;
    private double[,]? coefficients;
    private double[,] Coefficients => coefficients ??= Topology.Vandermonde(Reference, Basis);

    public ShapeSample Sample((double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz) => Family.Sample(this, natural, nodalXyz);

    internal static ShapeSample LineSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
        double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double[] grad = [-dx / (l * l), -dy / (l * l), -dz / (l * l), dx / (l * l), dy / (l * l), dz / (l * l)];
        return new([0.5 * (1.0 - nat.X), 0.5 * (1.0 + nat.X)], grad, l * 0.5);
    }

    public double[] ShapeGrad((double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz) => Sample(natural, nodalXyz).Grad;

    public double Metric(MeshMetric metric, ReadOnlySpan<double> nodalXyz) => metric.Measure(this, nodalXyz);

    public Fin<Unit> Member(ReadOnlySpan<double> xyz, in FrameMember member, double e, double nu, Span<double> local) {
        double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
        double length = Math.Sqrt(dx * dx + dy * dy + dz * dz) - member.OffsetI - member.OffsetJ;
        if (!(length > 0.0)) {
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<frame-degenerate-length:{Key}:L={length:e3}:offI={member.OffsetI:e3}:offJ={member.OffsetJ:e3}>"));
        }
        double g = e / (2.0 * (1.0 + nu));
        double phiY = Shear && member.ShearAreaZ > 0.0 ? 12.0 * e * member.Iy / (g * member.ShearAreaZ * length * length) : 0.0;
        double phiZ = Shear && member.ShearAreaY > 0.0 ? 12.0 * e * member.Iz / (g * member.ShearAreaY * length * length) : 0.0;
        Span<double> k = stackalloc double[144];
        Topology.LocalFrame(k, e, g, member, length, phiY, phiZ);
        Topology.SemiRigid(k, member, e, length);
        Topology.CondenseReleases(k, member.ReleaseMask);
        Topology.Eccentric(k, member.OffsetI, member.OffsetJ);
        Topology.RotateFrame(k, dx, dy, dz, local);
        return Fin.Succ(unit);
    }

    internal ShapeSample PolynomialSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        int n = Nodes;
        double[,] c = Coefficients;
        double[] shape = new double[n];
        double[,] dnRef = new double[n, 3];
        for (int m = 0; m < n; m++) {
            double p = Basis[m].Eval(nat), dpx = Basis[m].D(0, nat), dpy = Basis[m].D(1, nat), dpz = Basis[m].D(2, nat);
            for (int i = 0; i < n; i++) {
                double a = c[m, i];
                shape[i] += a * p; dnRef[i, 0] += a * dpx; dnRef[i, 1] += a * dpy; dnRef[i, 2] += a * dpz;
            }
        }
        return Topology.Iso(shape, dnRef, xyz, n, Dim);
    }

    internal ShapeSample ReducedSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        int n = Nodes;
        double[] shape = new double[n];
        double[,] dn = new double[n, 3];
        for (int i = 0; i < n; i++) {
            (double X, double Y, double Z) reference = Reference[i];
            (double s, double dx, double dy, double dz) = Dim == 2 ? Serendipity2(nat, reference) : Serendipity3(nat, reference);
            shape[i] = s; dn[i, 0] = dx; dn[i, 1] = dy; dn[i, 2] = dz;
        }
        return Topology.Iso(shape, dn, xyz, n, Dim);
    }

    static (double, double, double, double) Serendipity2((double X, double Y, double Z) p, (double X, double Y, double Z) r) {
        (double xi, double eta) = (p.X, p.Y);
        if (r.X != 0.0 && r.Y != 0.0) {
            double sx = r.X, sy = r.Y, gx = 1 + xi * sx, gy = 1 + eta * sy, q = xi * sx + eta * sy - 1;
            return (0.25 * gx * gy * q, 0.25 * sx * gy * (q + gx), 0.25 * sy * gx * (q + gy), 0.0);
        }
        if (r.X == 0.0) { double sy = r.Y, gy = 1 + eta * sy; return (0.5 * (1 - xi * xi) * gy, 0.5 * -2 * xi * gy, 0.5 * (1 - xi * xi) * sy, 0.0); }
        double sx = r.X, gx = 1 + xi * sx; return (0.5 * gx * (1 - eta * eta), 0.5 * sx * (1 - eta * eta), 0.5 * gx * -2 * eta, 0.0);
    }

    static (double, double, double, double) Serendipity3((double X, double Y, double Z) p, (double X, double Y, double Z) r) {
        (double xi, double eta, double ze) = (p.X, p.Y, p.Z);
        if (r.X != 0.0 && r.Y != 0.0 && r.Z != 0.0) {
            double sx = r.X, sy = r.Y, sz = r.Z, gx = 1 + xi * sx, gy = 1 + eta * sy, gz = 1 + ze * sz, q = xi * sx + eta * sy + ze * sz - 2;
            return (0.125 * gx * gy * gz * q, 0.125 * gy * gz * (sx * q + gx * sx), 0.125 * gx * gz * (sy * q + gy * sy), 0.125 * gx * gy * (sz * q + gz * sz));
        }
        double mx = r.X == 0.0 ? 1 - xi * xi : 1 + xi * r.X, my = r.Y == 0.0 ? 1 - eta * eta : 1 + eta * r.Y, mz = r.Z == 0.0 ? 1 - ze * ze : 1 + ze * r.Z;
        double dmx = r.X == 0.0 ? -2 * xi : r.X, dmy = r.Y == 0.0 ? -2 * eta : r.Y, dmz = r.Z == 0.0 ? -2 * ze : r.Z;
        return (0.25 * mx * my * mz, 0.25 * dmx * my * mz, 0.25 * mx * dmy * mz, 0.25 * mx * my * dmz);
    }

    internal double[] PyramidShape((double X, double Y, double Z) p) {
        double[] n = new double[5];
        double inv = 1.0 / Math.Max(1e-12, 1.0 - p.Z);
        for (int i = 0; i < 4; i++) {
            (double X, double Y, double Z) reference = Reference[i];
            n[i] = 0.25 * ((1 - p.Z) + reference.X * p.X + reference.Y * p.Y + reference.X * reference.Y * p.X * p.Y * inv);
        }
        n[4] = p.Z;
        return n;
    }

    internal double[,] PyramidGrad((double X, double Y, double Z) p) {
        double[,] dn = new double[5, 3];
        double inv = 1.0 / Math.Max(1e-12, 1.0 - p.Z), inv2 = inv * inv;
        for (int i = 0; i < 4; i++) {
            (double X, double Y, double Z) reference = Reference[i];
            dn[i, 0] = 0.25 * (reference.X + reference.X * reference.Y * p.Y * inv);
            dn[i, 1] = 0.25 * (reference.Y + reference.X * reference.Y * p.X * inv);
            dn[i, 2] = 0.25 * (-1 + reference.X * reference.Y * p.X * p.Y * inv2);
        }
        dn[4, 2] = 1.0;
        return dn;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshMetric {
    public static readonly MeshMetric ScaledJacobian = new("scaled-jacobian", ascendingBetter: true, ScaledJacobianMeasure);
    public static readonly MeshMetric AspectRatio = new("aspect-ratio", ascendingBetter: false, AspectRatioMeasure);
    public static readonly MeshMetric Skewness = new("skewness", ascendingBetter: false, SkewnessMeasure);
    public static readonly MeshMetric MinDihedral = new("min-dihedral", ascendingBetter: true, MinDihedralMeasure);
    public static readonly MeshMetric Condition = new("condition", ascendingBetter: false, ConditionMeasure);

    public bool AscendingBetter { get; }

    [UseDelegateFromConstructor]
    public partial double Measure(ElementClass element, ReadOnlySpan<double> nodalXyz);

    public double Worst(ReadOnlySpan<double> perElement) =>
        perElement.IsEmpty ? 0.0 : AscendingBetter ? TensorPrimitives.Min(perElement) : TensorPrimitives.Max(perElement);

    public bool Admits(double worst, double threshold) => AscendingBetter ? worst > threshold : worst < threshold;

    static double ScaledJacobianMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double worst = double.MaxValue;
        foreach (int corner in element.Corners) {
            (Vector3 e1, Vector3 e2, Vector3 e3) = Topology.CornerFrame(element, corner, xyz);
            double det = Vector3.Dot(Vector3.Cross(e1, e2), e3);
            double scale = (double)e1.Length() * e2.Length() * e3.Length();
            worst = Math.Min(worst, scale > 1e-12 ? det / scale : 0.0);
        }
        return worst == double.MaxValue ? 0.0 : worst;
    }

    static double AspectRatioMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double longest = 0.0, shortest = double.MaxValue;
        foreach ((int a, int b) in element.Edges) {
            double length = (Topology.Node(xyz, b) - Topology.Node(xyz, a)).Length();
            longest = Math.Max(longest, length); shortest = Math.Min(shortest, length);
        }
        return shortest > 1e-12 ? longest / shortest : double.MaxValue;
    }

    static double SkewnessMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double worst = 0.0;
        foreach (ImmutableArray<int> face in element.Faces) {
            double ideal = face.Length == 3 ? 60.0 : 90.0;
            for (int i = 0; i < face.Length; i++) {
                Vector3 o = Topology.Node(xyz, face[i]);
                Vector3 u = Topology.Node(xyz, face[(i + 1) % face.Length]) - o, v = Topology.Node(xyz, face[(i + face.Length - 1) % face.Length]) - o;
                double angle = Math.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(u), Vector3.Normalize(v)), -1.0, 1.0)) * 180.0 / Math.PI;
                worst = Math.Max(worst, Math.Max((angle - ideal) / (180.0 - ideal), (ideal - angle) / ideal));
            }
        }
        return worst;
    }

    static double MinDihedralMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double smallest = 180.0;
        foreach ((int a, int b) in element.Edges) {
            ImmutableArray<ImmutableArray<int>> incident = Topology.FacesOnEdge(element, a, b);
            if (incident.Length < 2) { continue; }
            Vector3 n1 = Topology.FaceNormal(incident[0], xyz), n2 = Topology.FaceNormal(incident[1], xyz);
            double angle = 180.0 - Math.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(n1), Vector3.Normalize(n2)), -1.0, 1.0)) * 180.0 / Math.PI;
            smallest = Math.Min(smallest, angle);
        }
        return smallest;
    }

    static double ConditionMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double jacobian = Math.Abs(ScaledJacobianMeasure(element, xyz));
        return jacobian > 1e-12 ? 1.0 / jacobian : double.MaxValue;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeshAlgorithm {
    public static readonly MeshAlgorithm Delaunay = new("delaunay", conforming: true, MeshStrategy.Delaunay, PointSource.Lattice);
    public static readonly MeshAlgorithm FrontalDelaunay = new("frontal-delaunay", conforming: true, MeshStrategy.Delaunay, PointSource.Frontal);
    public static readonly MeshAlgorithm AdvancingFront = new("advancing-front", conforming: true, MeshStrategy.Delaunay, PointSource.Front);
    public static readonly MeshAlgorithm Octree = new("octree", conforming: false, MeshStrategy.Octree, PointSource.Boundary);
    public static readonly MeshAlgorithm Sweep = new("sweep", conforming: true, MeshStrategy.Sweep, PointSource.Boundary);
    public static readonly MeshAlgorithm BoundaryLayer = new("boundary-layer", conforming: true, MeshStrategy.Inflation, PointSource.Boundary);

    public bool Conforming { get; }
    public MeshStrategy Strategy { get; }
    public PointSource Seed { get; }

    public ElementClass BaseElement => Strategy == MeshStrategy.Delaunay ? ElementClass.Tet4 : ElementClass.Hex8;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldStation {
    public static readonly FieldStation Nodal = new("nodal", static m => m.NodeCount);
    public static readonly FieldStation IntegrationPoint = new("integration-point", static m => m.ElementCount * m.Element.Quadrature.Points.Length);
    public static readonly FieldStation Cell = new("cell", static m => m.ElementCount);
    public static readonly FieldStation Boundary = new("boundary", static m => m.BoundaryCount);

    [UseDelegateFromConstructor]
    public partial long Count(DiscreteMesh mesh);
}

public sealed record FieldSpace(FieldStation Station, int Rank, int Components, long Count) {
    public static FieldSpace Scalar(FieldStation station, long count) => new(station, 0, 1, count);
    public static FieldSpace Vector(FieldStation station, int dim, long count) => new(station, 1, dim, count);
    public static FieldSpace Tensor(FieldStation station, int dim, long count) => new(station, 2, dim * dim, count);
    public static Fin<FieldSpace> OfKey(DiscreteMesh mesh, string station, int rank, int dim) =>
        FieldStation.TryGet(station, out FieldStation resolved)
            ? Fin.Succ(mesh.FieldOf(resolved, rank, dim))
            : Fin.Fail<FieldSpace>(new ComputeFault.ModelRejected($"<field-station-key:{station}>"));

    public long Cardinality => Count * Components;
}

public sealed record BoundaryShell(ReadOnlyMemory<float> Vertices, ReadOnlyMemory<int> Triangles, Aabb Bounds) {
    public static BoundaryShell Of(ReadOnlyMemory<float> vertices, ReadOnlyMemory<int> triangles) => new(vertices, triangles, Aabb.Of(vertices.Span));

    public int VertexCount => Vertices.Length / 3;
    public int TriangleCount => Triangles.Length / 3;
    public Vector3 Vertex(int i) {
        ReadOnlySpan<float> vertices = Vertices.Span;
        return new(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
    }

    public Fin<Unit> Validate() {
        if (Vertices.Length < 12 || Vertices.Length % 3 != 0) {
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-vertex-buffer:{Vertices.Length}>"));
        }
        if (Triangles.Length < 12 || Triangles.Length % 3 != 0) {
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-index-buffer:{Triangles.Length}>"));
        }
        ReadOnlySpan<float> coordinates = Vertices.Span;
        for (int coordinate = 0; coordinate < coordinates.Length; coordinate++) {
            if (!float.IsFinite(coordinates[coordinate])) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-nonfinite-coordinate:{coordinate}>"));
            }
        }
        Dictionary<(int Lo, int Hi), (int Count, int Balance)> edges = [];
        ReadOnlySpan<int> triangles = Triangles.Span;
        for (int offset = 0; offset < triangles.Length; offset += 3) {
            int a = triangles[offset], b = triangles[offset + 1], c = triangles[offset + 2];
            if ((uint)a >= VertexCount || (uint)b >= VertexCount || (uint)c >= VertexCount) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-index-range:{offset / 3}>"));
            }
            if (a == b || b == c || c == a || Vector3.Cross(Vertex(b) - Vertex(a), Vertex(c) - Vertex(a)).LengthSquared() <= 1e-20f) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-degenerate-face:{offset / 3}>"));
            }
            AddEdge(edges, a, b); AddEdge(edges, b, c); AddEdge(edges, c, a);
        }
        foreach (KeyValuePair<(int Lo, int Hi), (int Count, int Balance)> edge in edges) {
            if (edge.Value.Count != 2 || edge.Value.Balance != 0) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-boundary-nonmanifold-edge:{edge.Key.Lo}:{edge.Key.Hi}:count={edge.Value.Count}:winding={edge.Value.Balance}>"));
            }
        }
        return Fin.Succ(unit);

        static void AddEdge(Dictionary<(int Lo, int Hi), (int Count, int Balance)> edges, int from, int to) {
            (int Lo, int Hi) key = from < to ? (from, to) : (to, from);
            int direction = from < to ? 1 : -1;
            (int Count, int Balance) current = edges.GetValueOrDefault(key);
            edges[key] = (current.Count + 1, current.Balance + direction);
        }
    }

    public bool Encloses(Vector3 p) {
        ReadOnlySpan<int> tri = Triangles.Span;
        Vector3 dir = Vector3.UnitX;
        int crossings = 0;
        for (int t = 0; t < tri.Length; t += 3) {
            Vector3 a = Vertex(tri[t]), e1 = Vertex(tri[t + 1]) - a, e2 = Vertex(tri[t + 2]) - a, h = Vector3.Cross(dir, e2);
            float det = Vector3.Dot(e1, h);
            if (Math.Abs(det) < 1e-9f) { continue; }
            float inv = 1f / det;
            Vector3 s = p - a; float u = Vector3.Dot(s, h) * inv;
            if (u is < 0f or > 1f) { continue; }
            Vector3 q = Vector3.Cross(s, e1); float v = Vector3.Dot(dir, q) * inv;
            if (v < 0f || u + v > 1f) { continue; }
            if (Vector3.Dot(e2, q) * inv > 1e-9f) { crossings++; }
        }
        return (crossings & 1) == 1;
    }
}

public sealed record FrameMember(
    double Area, double Iy, double Iz, double J,
    int ReleaseMask = 0,
    double OffsetI = 0.0, double OffsetJ = 0.0,
    double SpringYi = double.PositiveInfinity, double SpringZi = double.PositiveInfinity,
    double SpringYj = double.PositiveInfinity, double SpringZj = double.PositiveInfinity,
    double ShearAreaY = 0.0, double ShearAreaZ = 0.0) {
    public void WriteCanonical(ArrayBufferWriter<byte> sink) {
        Span<byte> scratch = stackalloc byte[8];
        void Write(double v) { BinaryPrimitives.WriteDoubleLittleEndian(scratch, v); sink.Write(scratch); }
        BinaryPrimitives.WriteInt32LittleEndian(scratch, ReleaseMask); sink.Write(scratch[..4]);
        Write(Area); Write(Iy); Write(Iz); Write(J); Write(OffsetI); Write(OffsetJ);
        Write(SpringYi); Write(SpringZi); Write(SpringYj); Write(SpringZj); Write(ShearAreaY); Write(ShearAreaZ);
    }
}

public sealed record MeshPolicy(
    MeshAlgorithm Algorithm,
    ElementClass Element,
    MeshMetric Metric,
    double TargetEdgeLength,
    double GradingRatio,
    int BoundaryLayerCount,
    double BoundaryLayerGrowth,
    double FirstLayerThickness,
    double RefineFraction,
    RefineKind RefineAxis,
    int MaxRefineLevel,
    double QualityFloor,
    bool Mortar) {
    public static readonly MeshPolicy CanonicalTet = new(
        Algorithm: MeshAlgorithm.Delaunay, Element: ElementClass.Tet4, Metric: MeshMetric.ScaledJacobian,
        TargetEdgeLength: 0.05, GradingRatio: 1.4, BoundaryLayerCount: 0, BoundaryLayerGrowth: 1.2,
        FirstLayerThickness: 0.001, RefineFraction: 0.1, RefineAxis: RefineKind.H, MaxRefineLevel: 4, QualityFloor: 0.02, Mortar: false);
    public static readonly MeshPolicy CanonicalViscous = CanonicalTet with {
        Algorithm = MeshAlgorithm.BoundaryLayer, Element = ElementClass.Hex8, BoundaryLayerCount = 12 };
    public static readonly MeshPolicy CanonicalHp = CanonicalTet with { RefineAxis = RefineKind.Hp, Metric = MeshMetric.Condition, QualityFloor = 50.0 };

    public static Fin<MeshPolicy> OfKeys(MeshPolicy template, string algorithm, string element, string metric) {
        if (!MeshAlgorithm.TryGet(algorithm, out MeshAlgorithm resolvedAlgorithm)
            || !ElementClass.TryGet(element, out ElementClass resolvedElement)
            || !MeshMetric.TryGet(metric, out MeshMetric resolvedMetric)) {
            return Fin.Fail<MeshPolicy>(new ComputeFault.ModelRejected(
                $"<mesh-vocabulary-key:{algorithm}/{element}/{metric}:algorithms={MeshAlgorithm.Items.Count}:elements={ElementClass.Items.Count}:metrics={MeshMetric.Items.Count}>"));
        }
        MeshPolicy resolved = template with { Algorithm = resolvedAlgorithm, Element = resolvedElement, Metric = resolvedMetric };
        return resolved.Validate().Map(_ => resolved);
    }

    public Fin<Unit> Validate() =>
        Element != Algorithm.BaseElement
            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-element-strategy-mismatch:{Element.Key}≠{Algorithm.BaseElement.Key}@{Algorithm.Key}>"))
            : !double.IsFinite(TargetEdgeLength) || TargetEdgeLength <= 0.0
                ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-target-edge:{TargetEdgeLength}>"))
                : !double.IsFinite(GradingRatio) || GradingRatio < 1.0
                    ? Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<mesh-grading-ratio:{GradingRatio}>"))
                    : BoundaryLayerCount < 0 || !double.IsFinite(BoundaryLayerGrowth) || BoundaryLayerGrowth < 1.0 || !double.IsFinite(FirstLayerThickness) || FirstLayerThickness <= 0.0
                        ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<mesh-boundary-layer-policy>"))
                        : !double.IsFinite(RefineFraction) || RefineFraction is <= 0.0 or > 1.0 || MaxRefineLevel < 0 || !double.IsFinite(QualityFloor)
                            ? Fin.Fail<Unit>(new ComputeFault.ModelRejected("<mesh-refinement-policy>"))
                            : Fin.Succ(unit);
}

public sealed record DiscreteMesh(
    ElementClass Element,
    MeshAlgorithm Algorithm,
    ReadOnlyMemory<float> Nodes,
    ReadOnlyMemory<long> Connectivity,
    long NodeCount,
    long ElementCount,
    long BoundaryCount,
    int BoundaryLayers,
    int RefineLevel,
    MeshMetric Metric,
    double WorstQuality,
    Option<double> ErrorEstimate,
    Instant At) {
    public FieldSpace FieldOf(FieldStation station, int rank, int dim) => new(station, rank, Components(rank, dim), station.Count(this));

    public ReadOnlyTensorSpan<float> NodeTensor =>
        TensorMarshal.CreateReadOnlyTensorSpan(ref MemoryMarshal.GetReference(Nodes.Span), Nodes.Length, [(nint)NodeCount, 3], [], pinned: false);
    public ReadOnlyTensorSpan<long> ElementTensor =>
        TensorMarshal.CreateReadOnlyTensorSpan(ref MemoryMarshal.GetReference(Connectivity.Span), Connectivity.Length, [(nint)ElementCount, Element.Nodes], [], pinned: false);
    public ReadOnlySpan<float> Coordinates => Nodes.Span;
    public ReadOnlySpan<long> Indices => Connectivity.Span;

    public ReadOnlySpan<double> NodalXyz(long element) {
        ReadOnlySpan<long> conn = Indices;
        ReadOnlySpan<float> pos = Coordinates;
        int per = Element.Nodes;
        double[] xyz = new double[per * 3];
        for (int v = 0; v < per; v++) {
            long node = conn[(int)(element * per + v)];
            xyz[v * 3] = pos[(int)node * 3]; xyz[v * 3 + 1] = pos[(int)node * 3 + 1]; xyz[v * 3 + 2] = pos[(int)node * 3 + 2];
        }
        return xyz;
    }

    static int Components(int rank, int dim) => rank switch { 0 => 1, 1 => dim, _ => dim * dim };
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class MeshKernel {
    public static Fin<DiscreteMesh> Discretize(BoundaryShell boundary, MeshPolicy policy, ClockPolicy clocks) =>
        from boundaryValid in boundary.Validate()
        from policyValid in policy.Validate()
        from built in Try.lift(() => Generate(boundary, policy)).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<mesh-generation-failed:{error.Message}>"))
        from admitted in policy.Metric.Admits(built.Quality, policy.QualityFloor)
            ? Fin.Succ(Pack(built, policy, refineLevel: 0, None, clocks.Now))
            : Fin.Fail<DiscreteMesh>(new ComputeFault.ModelRejected($"<mesh-quality-rejected:{policy.Element.Key}:q={built.Quality:e3}>"))
        select admitted;

    public static Fin<DiscreteMesh> Refine(DiscreteMesh mesh, MeshPolicy policy, ReadOnlySpan<double> cellError, ClockPolicy clocks) {
        if (mesh.RefineLevel >= policy.MaxRefineLevel) { return Fin.Succ(mesh); }
        if (cellError.Length != mesh.ElementCount) {
            return Fin.Fail<DiscreteMesh>(new ComputeFault.ModelRejected($"<refine-estimator-cardinality:{cellError.Length}!={mesh.ElementCount}>"));
        }
        for (int cell = 0; cell < cellError.Length; cell++) {
            if (!double.IsFinite(cellError[cell]) || cellError[cell] < 0.0) {
                return Fin.Fail<DiscreteMesh>(new ComputeFault.ModelRejected($"<refine-estimator-value:{cell}:{cellError[cell]}>"));
            }
        }
        double threshold = DorflerThreshold(cellError, policy.RefineFraction);
        Seq<int> marked = Marked(cellError, threshold);
        MeshBuild built = policy.RefineAxis.Switch(
            state: (Mesh: mesh, Policy: policy, Marked: marked),
            h: static state => Subdivide(state.Mesh, state.Marked, state.Policy),
            p: static state => Elevate(state.Mesh, state.Policy),
            hp: static state => state.Marked.Count > state.Mesh.ElementCount / 4
                ? Elevate(state.Mesh, state.Policy)
                : Subdivide(state.Mesh, state.Marked, state.Policy));
        bool refined = built.ElementCount > mesh.ElementCount || built.NodeCount > mesh.NodeCount;
        if (!refined) { return Fin.Succ(mesh); }
        return policy.Metric.Admits(built.Quality, policy.QualityFloor)
            ? Fin.Succ(Pack(built, policy, mesh.RefineLevel + 1, Some(threshold), clocks.Now))
            : Fin.Fail<DiscreteMesh>(new ComputeFault.ModelRejected($"<refine-inverted:{built.Element.Key}>"));
    }

    public static double Quality(DiscreteMesh mesh, MeshMetric metric) {
        double[] perElement = new double[checked((int)mesh.ElementCount)];
        for (long cell = 0; cell < mesh.ElementCount; cell++) { perElement[cell] = mesh.Element.Metric(metric, mesh.NodalXyz(cell)); }
        return metric.Worst(perElement);
    }

    public static ComputeReceipt.Discretization Receipt(DiscreteMesh mesh, CorrelationId correlation, Duration elapsed) =>
        new(mesh.Algorithm.Key, mesh.Element.Key, mesh.NodeCount, mesh.ElementCount, mesh.BoundaryLayers, mesh.RefineLevel, mesh.WorstQuality, mesh.Metric.Key) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static MeshBuild Generate(BoundaryShell boundary, MeshPolicy policy) => policy.Algorithm.Strategy.Fill(boundary, policy);

    static double DorflerThreshold(ReadOnlySpan<double> cellError, double bulkFraction) {
        if (cellError.Length == 0) { return double.MaxValue; }
        double total = TensorPrimitives.Sum(cellError);
        if (total <= 0.0) { return double.MaxValue; }
        double target = bulkFraction * total, accumulated = 0.0;
        double[] sorted = cellError.ToArray();
        Array.Sort(sorted);
        for (int i = sorted.Length - 1; i >= 0; i--) {
            accumulated += sorted[i];
            if (accumulated >= target) { return sorted[i]; }
        }
        return sorted[0];
    }

    static Seq<int> Marked(ReadOnlySpan<double> cellError, double threshold) {
        Seq<int> marked = Seq<int>();
        for (int cell = 0; cell < cellError.Length; cell++) { if (cellError[cell] >= threshold) { marked = marked.Add(cell); } }
        return marked;
    }

    static DiscreteMesh Pack(MeshBuild built, MeshPolicy policy, int refineLevel, Option<double> error, Instant at) {
        ReadOnlyMemory<float> nodes = built.Nodes.AsMemory();
        ReadOnlyMemory<long> connectivity = CollectionsMarshal.AsSpan(built.Cells).ToArray().AsMemory();
        return new(built.Element, policy.Algorithm, nodes, connectivity, built.NodeCount, built.ElementCount, built.BoundaryCount,
            built.Layers, refineLevel, policy.Metric, built.Quality, error, at);
    }

    static MeshBuild Elevate(DiscreteMesh mesh, MeshPolicy policy) {
        ElementClass elevated = mesh.Element.Elevate;
        if (elevated == mesh.Element) { return Carry(mesh, policy); }
        Refinement refine = new(mesh.Coordinates, mesh.NodeCount);
        ReadOnlySpan<long> conn = mesh.Indices;
        int per = mesh.Element.Nodes;
        List<long> cells = new(checked((int)mesh.ElementCount) * elevated.Nodes);
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            for (int v = 0; v < per; v++) { cells.Add(conn[cell * per + v]); }
            foreach ((int a, int b) in mesh.Element.Edges) { cells.Add(refine.EdgeMid(conn[cell * per + a], conn[cell * per + b])); }
        }
        return new(elevated, refine.Nodes(), cells, cells.Count / elevated.Nodes, refine.Count, mesh.BoundaryLayers).Scored(policy.Metric);
    }

    static MeshBuild Subdivide(DiscreteMesh mesh, Seq<int> marked, MeshPolicy policy) {
        ImmutableArray<ImmutableArray<int>> template = Topology.RedTemplate(mesh.Element);
        if (template.IsEmpty) { return Carry(mesh, policy); }
        Seq<int> closed = policy.Mortar ? marked : Closed(mesh, marked);
        Refinement refine = new(mesh.Coordinates, mesh.NodeCount);
        ReadOnlySpan<long> conn = mesh.Indices;
        int per = mesh.Element.Nodes;
        List<long> cells = new(conn.Length * 2);
        for (int cell = 0; cell < mesh.ElementCount; cell++) {
            if (!closed.Contains(cell)) { for (int v = 0; v < per; v++) { cells.Add(conn[cell * per + v]); } continue; }
            Span<long> child = refine.RedNodes(mesh.Element, conn.Slice(cell * per, per));
            foreach (ImmutableArray<int> sub in template) foreach (int local in sub) { cells.Add(child[local]); }
        }
        return new(mesh.Element, refine.Nodes(), cells, cells.Count / per, refine.Count, mesh.BoundaryLayers).Scored(policy.Metric);
    }

    static Seq<int> Closed(DiscreteMesh mesh, Seq<int> marked) {
        ReadOnlySpan<long> conn = mesh.Indices;
        int per = mesh.Element.Nodes;
        HashSet<int> active = [.. marked];
        HashSet<(long Lo, long Hi)> split = [];
        foreach (int cell in marked) {
            foreach ((int a, int b) in mesh.Element.Edges) {
                long lo = conn[cell * per + a], hi = conn[cell * per + b];
                split.Add(lo < hi ? (lo, hi) : (hi, lo));
            }
        }
        for (bool grew = true; grew;) {
            grew = false;
            for (int cell = 0; cell < mesh.ElementCount; cell++) {
                if (active.Contains(cell)) { continue; }
                foreach ((int a, int b) in mesh.Element.Edges) {
                    long lo = conn[cell * per + a], hi = conn[cell * per + b];
                    if (!split.Contains(lo < hi ? (lo, hi) : (hi, lo))) { continue; }
                    active.Add(cell);
                    foreach ((int c, int d) in mesh.Element.Edges) {
                        long fromNode = conn[cell * per + c], toNode = conn[cell * per + d];
                        split.Add(fromNode < toNode ? (fromNode, toNode) : (toNode, fromNode));
                    }
                    grew = true;
                    break;
                }
            }
        }
        return toSeq(active.OrderBy(static cell => cell));
    }

    static MeshBuild Carry(DiscreteMesh mesh, MeshPolicy policy) =>
        new(mesh.Element, [.. mesh.Coordinates], [.. mesh.Indices], mesh.ElementCount, mesh.NodeCount, mesh.BoundaryLayers).Scored(policy.Metric);
}

public static class DelaunayCore {
    public static MeshBuild Fill(BoundaryShell boundary, MeshPolicy policy) {
        List<Vector3> points = Seed(boundary, policy);
        List<(int A, int B, int C, int D)> tets = Triangulate(points);
        List<long> kept = new(tets.Count * 4);
        int cells = 0;
        foreach ((int a, int b, int c, int d) in tets) {
            Vector3 centroid = (points[a] + points[b] + points[c] + points[d]) * 0.25f;
            if (!boundary.Encloses(centroid)) { continue; }
            (int x, int y) = Topology.Orient(points[a], points[b], points[c], points[d]) ? (b, c) : (c, b);
            kept.AddRange([a, x, y, d]); cells++;
        }
        float[] flat = new float[points.Count * 3];
        for (int i = 0; i < points.Count; i++) { flat[i * 3] = points[i].X; flat[i * 3 + 1] = points[i].Y; flat[i * 3 + 2] = points[i].Z; }
        return new(policy.Element, flat, kept, cells, points.Count, 0).Scored(policy.Metric);
    }

    static List<Vector3> Seed(BoundaryShell boundary, MeshPolicy policy) {
        List<Vector3> points = new(boundary.VertexCount);
        for (int v = 0; v < boundary.VertexCount; v++) { points.Add(boundary.Vertex(v)); }
        Aabb box = boundary.Bounds;
        double edge = policy.Seed.Spacing(policy.TargetEdgeLength, policy.GradingRatio);
        for (float z = box.Lo.Z + (float)edge; z < box.Hi.Z; z += (float)edge)
            for (float y = box.Lo.Y + (float)edge; y < box.Hi.Y; y += (float)edge)
                for (float x = box.Lo.X + (float)edge; x < box.Hi.X; x += (float)edge) {
                    Vector3 p = new(x, y, z);
                    if (boundary.Encloses(p)) { points.Add(p); }
                }
        return points;
    }

    static List<(int A, int B, int C, int D)> Triangulate(List<Vector3> points) {
        Aabb box = Aabb.Of(MemoryMarshal.Cast<Vector3, float>(CollectionsMarshal.AsSpan(points)));
        Vector3 mid = box.Center; float span = Math.Max(box.Span.X, Math.Max(box.Span.Y, box.Span.Z)) * 8f + 1f;
        int n = points.Count;
        points.Add(mid + new Vector3(-span, -span, -span)); points.Add(mid + new Vector3(span * 3, 0, 0));
        points.Add(mid + new Vector3(0, span * 3, 0)); points.Add(mid + new Vector3(0, 0, span * 3));
        List<(int, int, int, int)> tets = [(n, n + 1, n + 2, n + 3)];
        for (int p = 0; p < n; p++) {
            Dictionary<(int, int, int), int> faces = [];
            tets.RemoveAll(t => {
                if (Topology.InSphere(points[t.Item1], points[t.Item2], points[t.Item3], points[t.Item4], points[p])) {
                    foreach ((int, int, int) face in Topology.TetFaceTriples(t)) {
                        (int, int, int) key = Topology.SortTriple(face);
                        faces[key] = faces.TryGetValue(key, out int count) ? count + 1 : 1;
                    }
                    return true;
                }
                return false;
            });
            foreach (KeyValuePair<(int, int, int), int> face in faces) {
                if (face.Value == 1) { tets.Add(Topology.OrientedTet(points, face.Key, p)); }
            }
        }
        tets.RemoveAll(t => t.Item1 >= n || t.Item2 >= n || t.Item3 >= n || t.Item4 >= n);
        points.RemoveRange(n, 4);
        return tets;
    }
}

public static class OctreeCore {
    public static MeshBuild Fill(BoundaryShell boundary, MeshPolicy policy) {
        Dictionary<(int, int, int), int> nodes = [];
        List<long> cells = [];
        List<Vector3> verts = [];
        int count = Recurse(boundary, boundary.Bounds, policy, nodes, verts, cells, depth: 0);
        float[] flat = new float[verts.Count * 3];
        for (int i = 0; i < verts.Count; i++) { flat[i * 3] = verts[i].X; flat[i * 3 + 1] = verts[i].Y; flat[i * 3 + 2] = verts[i].Z; }
        return new(policy.Element, flat, cells, count, verts.Count, 0).Scored(policy.Metric);
    }

    static int Recurse(BoundaryShell boundary, Aabb box, MeshPolicy policy, Dictionary<(int, int, int), int> nodes, List<Vector3> verts, List<long> cells, int depth) {
        float size = Math.Max(box.Span.X, Math.Max(box.Span.Y, box.Span.Z));
        bool straddles = boundary.Encloses(box.Center) ^ boundary.Encloses(box.Lo);
        if (size > policy.TargetEdgeLength && depth < policy.MaxRefineLevel + 6 && (straddles || size > policy.TargetEdgeLength * policy.GradingRatio)) {
            Vector3 c = box.Center; int emitted = 0;
            foreach (Aabb child in Topology.OctreeChildren(box, c)) { emitted += Recurse(boundary, child, policy, nodes, verts, cells, depth + 1); }
            return emitted;
        }
        if (!boundary.Encloses(box.Center)) { return 0; }
        foreach ((float X, float Y, float Z) corner in Topology.HexCorners(box)) { cells.Add(Node(corner, nodes, verts)); }
        return 1;
    }

    static long Node((float X, float Y, float Z) p, Dictionary<(int, int, int), int> nodes, List<Vector3> verts) {
        (int, int, int) key = ((int)MathF.Round(p.X * 1e5f), (int)MathF.Round(p.Y * 1e5f), (int)MathF.Round(p.Z * 1e5f));
        if (nodes.TryGetValue(key, out int id)) { return id; }
        id = verts.Count; nodes[key] = id; verts.Add(new(p.X, p.Y, p.Z));
        return id;
    }
}

public static class SweepCore {
    public static MeshBuild Fill(BoundaryShell boundary, MeshPolicy policy) {
        Aabb box = boundary.Bounds;
        int layers = Math.Max(1, (int)Math.Ceiling((box.Hi.Z - box.Lo.Z) / policy.TargetEdgeLength));
        (int nx, int ny, List<(float X, float Y)> plane) = BasePlane(boundary, policy, box);
        List<Vector3> verts = new((layers + 1) * plane.Count);
        for (int l = 0; l <= layers; l++) {
            float z = box.Lo.Z + (box.Hi.Z - box.Lo.Z) * l / layers;
            foreach ((float X, float Y) point in plane) { verts.Add(new(point.X, point.Y, z)); }
        }
        List<long> cells = [];
        int count = 0, stride = plane.Count;
        for (int l = 0; l < layers; l++)
            for (int j = 0; j < ny - 1; j++)
                for (int i = 0; i < nx - 1; i++) {
                    int b0 = l * stride + j * nx + i, t0 = b0 + stride;
                    Vector3 mid = (verts[b0] + verts[t0 + nx + 1]) * 0.5f;
                    if (!boundary.Encloses(mid)) { continue; }
                    cells.AddRange([b0, b0 + 1, b0 + nx + 1, b0 + nx, t0, t0 + 1, t0 + nx + 1, t0 + nx]); count++;
                }
        float[] flat = new float[verts.Count * 3];
        for (int v = 0; v < verts.Count; v++) { flat[v * 3] = verts[v].X; flat[v * 3 + 1] = verts[v].Y; flat[v * 3 + 2] = verts[v].Z; }
        return new(policy.Element, flat, cells, count, verts.Count, layers).Scored(policy.Metric);
    }

    public static (int Nx, int Ny, List<(float X, float Y)> Plane) BasePlane(BoundaryShell boundary, MeshPolicy policy, Aabb box) {
        int nx = Math.Max(2, (int)Math.Ceiling((box.Hi.X - box.Lo.X) / policy.TargetEdgeLength) + 1);
        int ny = Math.Max(2, (int)Math.Ceiling((box.Hi.Y - box.Lo.Y) / policy.TargetEdgeLength) + 1);
        List<(float, float)> plane = new(nx * ny);
        for (int j = 0; j < ny; j++) for (int i = 0; i < nx; i++) { plane.Add((box.Lo.X + (box.Hi.X - box.Lo.X) * i / (nx - 1), box.Lo.Y + (box.Hi.Y - box.Lo.Y) * j / (ny - 1))); }
        return (nx, ny, plane);
    }
}

public static class InflationCore {
    public static MeshBuild Fill(BoundaryShell boundary, MeshPolicy policy) {
        Aabb box = boundary.Bounds;
        List<float> levels = WallNormal(box, policy);
        (int nx, int ny, List<(float X, float Y)> plane) = SweepCore.BasePlane(boundary, policy, box);
        List<Vector3> verts = new(levels.Count * plane.Count);
        foreach (float z in levels) foreach ((float X, float Y) point in plane) { verts.Add(new(point.X, point.Y, z)); }
        List<long> cells = [];
        int count = 0, stride = plane.Count;
        for (int l = 0; l < levels.Count - 1; l++)
            for (int j = 0; j < ny - 1; j++)
                for (int i = 0; i < nx - 1; i++) {
                    int b0 = l * stride + j * nx + i, t0 = b0 + stride;
                    if (!boundary.Encloses((verts[b0] + verts[t0 + nx + 1]) * 0.5f)) { continue; }
                    cells.AddRange([b0, b0 + 1, b0 + nx + 1, b0 + nx, t0, t0 + 1, t0 + nx + 1, t0 + nx]); count++;
                }
        float[] flat = new float[verts.Count * 3];
        for (int v = 0; v < verts.Count; v++) { flat[v * 3] = verts[v].X; flat[v * 3 + 1] = verts[v].Y; flat[v * 3 + 2] = verts[v].Z; }
        return new(policy.Element, flat, cells, count, verts.Count, policy.BoundaryLayerCount).Scored(policy.Metric);
    }

    static List<float> WallNormal(Aabb box, MeshPolicy policy) {
        List<float> levels = [box.Lo.Z];
        float z = box.Lo.Z, thickness = (float)policy.FirstLayerThickness;
        for (int layer = 0; layer < policy.BoundaryLayerCount && z < box.Hi.Z; layer++) {
            z += thickness; levels.Add(Math.Min(z, box.Hi.Z)); thickness *= (float)policy.BoundaryLayerGrowth;
        }
        for (z = levels[^1] + (float)policy.TargetEdgeLength; z < box.Hi.Z; z += (float)policy.TargetEdgeLength) { levels.Add(z); }
        levels.Add(box.Hi.Z);
        return levels;
    }
}

public sealed record MeshBuild(ElementClass Element, float[] Nodes, List<long> Cells, long ElementCount, long NodeCount, int Layers) {
    public long BoundaryCount { get; init; }
    public double Quality { get; init; } = 1.0;

    public MeshBuild Scored(MeshMetric metric) {
        if (ElementCount == 0) { return this with { Quality = 0.0, BoundaryCount = BoundaryFold() }; }
        double[] perElement = new double[checked((int)ElementCount)];
        int per = Element.Nodes;
        for (int cell = 0; cell < ElementCount; cell++) {
            double[] xyz = new double[per * 3];
            for (int v = 0; v < per; v++) { long node = Cells[cell * per + v]; xyz[v * 3] = Nodes[(int)node * 3]; xyz[v * 3 + 1] = Nodes[(int)node * 3 + 1]; xyz[v * 3 + 2] = Nodes[(int)node * 3 + 2]; }
            perElement[cell] = Element.Metric(metric, xyz);
        }
        return this with { Quality = metric.Worst(perElement), BoundaryCount = BoundaryFold() };
    }

    long BoundaryFold() {
        Dictionary<string, (int Count, long[] Nodes)> faceCount = [];
        int per = Element.Nodes;
        for (int cell = 0; cell < ElementCount; cell++)
            foreach (ImmutableArray<int> face in Element.Faces) {
                if (face.Length < 3) { continue; }
                long[] ids = new long[face.Length];
                for (int i = 0; i < face.Length; i++) { ids[i] = Cells[cell * per + face[i]]; }
                long[] sorted = (long[])ids.Clone(); Array.Sort(sorted);
                string key = string.Join(',', sorted);
                faceCount[key] = faceCount.TryGetValue(key, out (int Count, long[] Nodes) entry) ? (entry.Count + 1, entry.Nodes) : (1, ids);
            }
        HashSet<long> hull = [];
        foreach (KeyValuePair<string, (int Count, long[] Nodes)> face in faceCount) {
            if (face.Value.Count == 1) foreach (long node in face.Value.Nodes) { hull.Add(node); }
        }
        return hull.Count;
    }
}

public sealed class Refinement {
    readonly List<float> nodes;
    readonly Dictionary<(long, long), long> edgeMid = [];
    readonly Dictionary<string, long> faceMid = [];
    long count;

    public Refinement(ReadOnlySpan<float> seed, long seedCount) { nodes = [.. seed.ToArray()]; count = seedCount; }

    public long EdgeMid(long a, long b) {
        (long, long) key = a < b ? (a, b) : (b, a);
        if (edgeMid.TryGetValue(key, out long mid)) { return mid; }
        mid = count++;
        for (int d = 0; d < 3; d++) { nodes.Add(0.5f * (nodes[(int)a * 3 + d] + nodes[(int)b * 3 + d])); }
        edgeMid[key] = mid;
        return mid;
    }

    public Span<long> RedNodes(ElementClass element, ReadOnlySpan<long> corners) {
        int extra = element == ElementClass.Hex8 ? 7 : element == ElementClass.Quad4 ? 1 : 0;
        long[] pool = new long[corners.Length + element.Edges.Length + extra];
        corners.CopyTo(pool);
        int next = corners.Length;
        foreach ((int a, int b) in element.Edges) { pool[next++] = EdgeMid(corners[a], corners[b]); }
        if (element == ElementClass.Hex8) {
            foreach (ImmutableArray<int> face in element.Faces) { pool[next++] = FaceMid(corners, face); }
            pool[next] = Centre(corners);
        }
        else if (element == ElementClass.Quad4) { pool[next] = FaceMid(corners, element.Faces[0]); }
        return pool;
    }

    long FaceMid(ReadOnlySpan<long> corners, ImmutableArray<int> face) {
        long[] ids = new long[face.Length];
        for (int i = 0; i < face.Length; i++) { ids[i] = corners[face[i]]; }
        long[] sorted = (long[])ids.Clone(); Array.Sort(sorted);
        string key = string.Join(',', sorted);
        if (faceMid.TryGetValue(key, out long mid)) { return mid; }
        mid = count++;
        Span<float> acc = stackalloc float[3];
        foreach (int v in face) for (int d = 0; d < 3; d++) { acc[d] += nodes[(int)corners[v] * 3 + d]; }
        for (int d = 0; d < 3; d++) { nodes.Add(acc[d] / face.Length); }
        faceMid[key] = mid;
        return mid;
    }

    long Centre(ReadOnlySpan<long> corners) {
        long mid = count++;
        Span<float> acc = stackalloc float[3];
        foreach (long v in corners) for (int d = 0; d < 3; d++) { acc[d] += nodes[(int)v * 3 + d]; }
        for (int d = 0; d < 3; d++) { nodes.Add(acc[d] / corners.Length); }
        return mid;
    }

    public float[] Nodes() => [.. nodes];
    public long Count => count;
}

public static class Topology {
    public static readonly ImmutableArray<(double, double, double)> LineRef2 = [(-1, 0, 0), (1, 0, 0)];
    public static readonly ImmutableArray<Monomial> LineP1 = [new(0, 0, 0), new(1, 0, 0)];
    public static readonly ImmutableArray<(int A, int B)> LineEdges = [(0, 1)];
    public static readonly ImmutableArray<(double, double, double)> TetRef4 = [(0, 0, 0), (1, 0, 0), (0, 1, 0), (0, 0, 1)];
    public static readonly ImmutableArray<(double, double, double)> TetRef10 = [.. TetRef4, .. EdgeMidsRef(TetRef4, TetEdges)];
    public static readonly ImmutableArray<(double, double, double)> TriRef3 = [(0, 0, 0), (1, 0, 0), (0, 1, 0)];
    public static readonly ImmutableArray<(double, double, double)> TriRef6 = [.. TriRef3, .. EdgeMidsRef(TriRef3, TriEdges)];
    public static readonly ImmutableArray<(double, double, double)> QuadRef4 = [(-1, -1, 0), (1, -1, 0), (1, 1, 0), (-1, 1, 0)];
    public static readonly ImmutableArray<(double, double, double)> QuadRef8 = [.. QuadRef4, .. EdgeMidsRef(QuadRef4, QuadEdges)];
    public static readonly ImmutableArray<(double, double, double)> HexRef8 = [(-1, -1, -1), (1, -1, -1), (1, 1, -1), (-1, 1, -1), (-1, -1, 1), (1, -1, 1), (1, 1, 1), (-1, 1, 1)];
    public static readonly ImmutableArray<(double, double, double)> HexRef20 = [.. HexRef8, .. EdgeMidsRef(HexRef8, HexEdges)];
    public static readonly ImmutableArray<(double, double, double)> HexRef27 = [.. Grid([-1, 0, 1], 3)];
    public static readonly ImmutableArray<(double, double, double)> WedgeRef6 = [(0, 0, -1), (1, 0, -1), (0, 1, -1), (0, 0, 1), (1, 0, 1), (0, 1, 1)];
    public static readonly ImmutableArray<(double, double, double)> WedgeRef18 = [.. WedgePrismRef()];
    public static readonly ImmutableArray<(double, double, double)> PyramidRef5 = [(-1, -1, 0), (1, -1, 0), (1, 1, 0), (-1, 1, 0), (0, 0, 1)];

    public static readonly ImmutableArray<Monomial> TetP1 = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)];
    public static readonly ImmutableArray<Monomial> TetP2 = [.. TetP1, new(2, 0, 0), new(0, 2, 0), new(0, 0, 2), new(1, 1, 0), new(0, 1, 1), new(1, 0, 1)];
    public static readonly ImmutableArray<Monomial> TriP1 = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)];
    public static readonly ImmutableArray<Monomial> TriP2 = [.. TriP1, new(2, 0, 0), new(0, 2, 0), new(1, 1, 0)];
    public static readonly ImmutableArray<Monomial> QuadQ1 = [.. Tensor([0, 1], 2)];
    public static readonly ImmutableArray<Monomial> HexQ1 = [.. Tensor([0, 1], 3)];
    public static readonly ImmutableArray<Monomial> HexQ2 = [.. Tensor([0, 1, 2], 3)];
    public static readonly ImmutableArray<Monomial> WedgeP1 = [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 1), new(0, 1, 1)];
    public static readonly ImmutableArray<Monomial> WedgeP2 = [.. WedgePrismMonomials()];

    public static readonly ImmutableArray<(int, int)> TetEdges = [(0, 1), (1, 2), (2, 0), (0, 3), (1, 3), (2, 3)];
    public static readonly ImmutableArray<(int, int)> TriEdges = [(0, 1), (1, 2), (2, 0)];
    public static readonly ImmutableArray<(int, int)> QuadEdges = [(0, 1), (1, 2), (2, 3), (3, 0)];
    public static readonly ImmutableArray<(int, int)> HexEdges = [(0, 1), (1, 2), (2, 3), (3, 0), (4, 5), (5, 6), (6, 7), (7, 4), (0, 4), (1, 5), (2, 6), (3, 7)];
    public static readonly ImmutableArray<(int, int)> WedgeEdges = [(0, 1), (1, 2), (2, 0), (3, 4), (4, 5), (5, 3), (0, 3), (1, 4), (2, 5)];
    public static readonly ImmutableArray<(int, int)> PyramidEdges = [(0, 1), (1, 2), (2, 3), (3, 0), (0, 4), (1, 4), (2, 4), (3, 4)];

    public static readonly ImmutableArray<ImmutableArray<int>> TetFaces = [[0, 2, 1], [0, 1, 3], [1, 2, 3], [2, 0, 3]];
    public static readonly ImmutableArray<ImmutableArray<int>> TriFaces = [[0, 1, 2]];
    public static readonly ImmutableArray<ImmutableArray<int>> QuadFaces = [[0, 1, 2, 3]];
    public static readonly ImmutableArray<ImmutableArray<int>> HexFaces = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]];
    public static readonly ImmutableArray<ImmutableArray<int>> WedgeFaces = [[0, 2, 1], [3, 4, 5], [0, 1, 4, 3], [1, 2, 5, 4], [2, 0, 3, 5]];
    public static readonly ImmutableArray<ImmutableArray<int>> PyramidFaces = [[0, 3, 2, 1], [0, 1, 4], [1, 2, 4], [2, 3, 4], [3, 0, 4]];

    static readonly ImmutableArray<ImmutableArray<int>> TetRed = [[0, 4, 6, 7], [1, 5, 4, 8], [2, 6, 5, 9], [3, 7, 9, 8], [4, 5, 6, 9], [4, 9, 7, 8], [4, 8, 9, 5], [4, 6, 9, 7]];
    static readonly ImmutableArray<ImmutableArray<int>> HexRed = [.. HexOctants()];
    static readonly ImmutableArray<ImmutableArray<int>> TriRed = [[0, 3, 5], [3, 1, 4], [5, 4, 2], [3, 4, 5]];
    static readonly ImmutableArray<ImmutableArray<int>> QuadRed = [[0, 4, 8, 7], [4, 1, 5, 8], [8, 5, 2, 6], [7, 8, 6, 3]];

    public static double[,] Vandermonde(ImmutableArray<(double X, double Y, double Z)> nodes, ImmutableArray<Monomial> basis) {
        int n = nodes.Length;
        Matrix<double> vandermonde = Matrix<double>.Build.Dense(n, n, (i, m) => basis[m].Eval(nodes[i]));
        Matrix<double> inverse = vandermonde.Inverse();
        double[,] c = new double[n, n];
        for (int m = 0; m < n; m++) for (int i = 0; i < n; i++) { c[m, i] = inverse[m, i]; }
        return c;
    }

    public static ShapeSample Iso(double[] shape, double[,] dnRef, ReadOnlySpan<double> xyz, int nodes, int dim) {
        double[,] j = new double[dim, dim];
        for (int i = 0; i < nodes; i++) for (int a = 0; a < dim; a++) for (int b = 0; b < dim; b++) { j[a, b] += dnRef[i, a] * xyz[i * 3 + b]; }
        (double det, double[,] inv) = dim == 3 ? Invert3(j) : Invert2(j);
        double[] grad = new double[nodes * 3];
        for (int i = 0; i < nodes; i++) for (int b = 0; b < dim; b++) { double s = 0.0; for (int a = 0; a < dim; a++) { s += dnRef[i, a] * inv[a, b]; } grad[i * 3 + b] = s; }
        return new(shape, grad, det);
    }

    static (double, double[,]) Invert2(double[,] j) {
        double det = j[0, 0] * j[1, 1] - j[0, 1] * j[1, 0], inv = 1.0 / (Math.Abs(det) < 1e-300 ? 1e-300 : det);
        return (det, new[,] { { j[1, 1] * inv, -j[0, 1] * inv }, { -j[1, 0] * inv, j[0, 0] * inv } });
    }

    static (double, double[,]) Invert3(double[,] j) {
        double c00 = j[1, 1] * j[2, 2] - j[1, 2] * j[2, 1], c01 = j[1, 2] * j[2, 0] - j[1, 0] * j[2, 2], c02 = j[1, 0] * j[2, 1] - j[1, 1] * j[2, 0];
        double det = j[0, 0] * c00 + j[0, 1] * c01 + j[0, 2] * c02, inv = 1.0 / (Math.Abs(det) < 1e-300 ? 1e-300 : det);
        double[,] m = {
            { c00 * inv, (j[0, 2] * j[2, 1] - j[0, 1] * j[2, 2]) * inv, (j[0, 1] * j[1, 2] - j[0, 2] * j[1, 1]) * inv },
            { c01 * inv, (j[0, 0] * j[2, 2] - j[0, 2] * j[2, 0]) * inv, (j[0, 2] * j[1, 0] - j[0, 0] * j[1, 2]) * inv },
            { c02 * inv, (j[0, 1] * j[2, 0] - j[0, 0] * j[2, 1]) * inv, (j[0, 0] * j[1, 1] - j[0, 1] * j[1, 0]) * inv } };
        return (det, m);
    }

    public static Vector3 Node(ReadOnlySpan<double> xyz, int index) => new((float)xyz[index * 3], (float)xyz[index * 3 + 1], (float)xyz[index * 3 + 2]);

    public static (Vector3 E1, Vector3 E2, Vector3 E3) CornerFrame(ElementClass element, int corner, ReadOnlySpan<double> xyz) {
        Vector3 o = Node(xyz, corner);
        List<Vector3> incident = new(3);
        foreach ((int a, int b) in element.Edges) { if (a == corner) { incident.Add(Node(xyz, b) - o); } else if (b == corner) { incident.Add(Node(xyz, a) - o); } }
        return (incident[0], incident.Count > 1 ? incident[1] : incident[0], incident.Count > 2 ? incident[2] : Vector3.Cross(incident[0], incident.Count > 1 ? incident[1] : Vector3.UnitZ));
    }

    public static ImmutableArray<ImmutableArray<int>> FacesOnEdge(ElementClass element, int a, int b) =>
        [.. element.Faces.Where(f => Contains(f, a) && Contains(f, b))];

    public static Vector3 FaceNormal(ImmutableArray<int> face, ReadOnlySpan<double> xyz) =>
        Vector3.Cross(Node(xyz, face[1]) - Node(xyz, face[0]), Node(xyz, face[2]) - Node(xyz, face[0]));

    public static bool Orient(Vector3 a, Vector3 b, Vector3 c, Vector3 d) =>
        Predicate.Orient3D(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z) == Sign.Positive;

    public static bool InSphere(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 p) =>
        Predicate.Orient3D(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z)
            .Times(Predicate.InSphere(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z, p.X, p.Y, p.Z)) == Sign.Positive;

    public static IEnumerable<(int, int, int)> TetFaceTriples((int A, int B, int C, int D) t) {
        yield return (t.A, t.B, t.C); yield return (t.A, t.B, t.D); yield return (t.A, t.C, t.D); yield return (t.B, t.C, t.D);
    }

    public static (long, long, long) SortTriple((long, long, long) f) {
        long a = f.Item1, b = f.Item2, c = f.Item3;
        if (a > b) { (a, b) = (b, a); }
        if (b > c) { (b, c) = (c, b); }
        if (a > b) { (a, b) = (b, a); }
        return (a, b, c);
    }

    public static (int, int, int) SortTriple((int, int, int) f) {
        (long, long, long) sorted = SortTriple(((long)f.Item1, f.Item2, f.Item3));
        return ((int)sorted.Item1, (int)sorted.Item2, (int)sorted.Item3);
    }

    public static (int, int, int, int) OrientedTet(List<Vector3> points, (int, int, int) face, int apex) =>
        Orient(points[face.Item1], points[face.Item2], points[face.Item3], points[apex])
            ? (face.Item1, face.Item2, face.Item3, apex) : (face.Item1, face.Item3, face.Item2, apex);


    // --- [FRAME_STIFFNESS]
    public static void LocalFrame(Span<double> k, double e, double g, in FrameMember m, double l, double phiY, double phiZ) {
        k.Clear();
        double ax = e * m.Area / l, tor = g * m.J / l;
        double bz = 12.0 * e * m.Iz / ((1.0 + phiZ) * l * l * l), by = 12.0 * e * m.Iy / ((1.0 + phiY) * l * l * l);
        double cz = 6.0 * e * m.Iz / ((1.0 + phiZ) * l * l), cy = 6.0 * e * m.Iy / ((1.0 + phiY) * l * l);
        double dzz = (4.0 + phiZ) * e * m.Iz / ((1.0 + phiZ) * l), ezz = (2.0 - phiZ) * e * m.Iz / ((1.0 + phiZ) * l);
        double dyy = (4.0 + phiY) * e * m.Iy / ((1.0 + phiY) * l), eyy = (2.0 - phiY) * e * m.Iy / ((1.0 + phiY) * l);
        void Set(int r, int c, double v) { k[r * 12 + c] = v; k[c * 12 + r] = v; }
        Set(0, 0, ax); Set(0, 6, -ax); Set(6, 6, ax);
        Set(3, 3, tor); Set(3, 9, -tor); Set(9, 9, tor);
        Set(1, 1, bz); Set(1, 5, cz); Set(1, 7, -bz); Set(1, 11, cz);
        Set(5, 5, dzz); Set(5, 7, -cz); Set(5, 11, ezz);
        Set(7, 7, bz); Set(7, 11, -cz); Set(11, 11, dzz);
        Set(2, 2, by); Set(2, 4, -cy); Set(2, 8, -by); Set(2, 10, -cy);
        Set(4, 4, dyy); Set(4, 8, cy); Set(4, 10, eyy);
        Set(8, 8, by); Set(8, 10, cy); Set(10, 10, dyy);
    }

    public static void SemiRigid(Span<double> k, in FrameMember m, double e, double l) {
        Fold(k, 4, m.SpringYi); Fold(k, 5, m.SpringZi); Fold(k, 10, m.SpringYj); Fold(k, 11, m.SpringZj);
        static void Fold(Span<double> k, int d, double spring) {
            if (double.IsPositiveInfinity(spring) || k[d * 12 + d] <= 0.0) { return; }
            double alpha = spring / (spring + k[d * 12 + d]);
            for (int i = 0; i < 12; i++) { if (i != d) { k[d * 12 + i] *= alpha; k[i * 12 + d] *= alpha; } }
            k[d * 12 + d] *= alpha;
        }
    }

    public static void CondenseReleases(Span<double> k, int releaseMask) {
        for (int d = 0; d < 12; d++) {
            if ((releaseMask & (1 << d)) == 0 || Math.Abs(k[d * 12 + d]) < 1e-30) { continue; }
            double pivot = k[d * 12 + d];
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++) { if (i != d && j != d) { k[i * 12 + j] -= k[i * 12 + d] * k[d * 12 + j] / pivot; } }
            for (int i = 0; i < 12; i++) { k[d * 12 + i] = 0.0; k[i * 12 + d] = 0.0; }
        }
    }

    public static void Eccentric(Span<double> k, double offsetI, double offsetJ) {
        if (offsetI == 0.0 && offsetJ == 0.0) { return; }
        Span<double> e = stackalloc double[144];
        for (int i = 0; i < 12; i++) { e[i * 12 + i] = 1.0; }
        e[1 * 12 + 5] = offsetI; e[2 * 12 + 4] = -offsetI;
        e[7 * 12 + 11] = -offsetJ; e[8 * 12 + 10] = offsetJ;
        Congruence(k, e);
    }

    public static void RotateFrame(Span<double> k, double dx, double dy, double dz, Span<double> global) {
        double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double cx = dx / l, cy = dy / l, cz = dz / l;
        Span<double> r = stackalloc double[9];
        double d = Math.Sqrt(cx * cx + cy * cy);
        if (d < 1e-9) { r[0] = 0; r[1] = 0; r[2] = cz; r[3] = 0; r[4] = 1; r[5] = 0; r[6] = -cz; r[7] = 0; r[8] = 0; }
        else { r[0] = cx; r[1] = cy; r[2] = cz; r[3] = -cy / d; r[4] = cx / d; r[5] = 0; r[6] = -cx * cz / d; r[7] = -cy * cz / d; r[8] = d; }
        Span<double> t = stackalloc double[144];
        for (int b = 0; b < 4; b++)
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++) { t[(b * 3 + i) * 12 + (b * 3 + j)] = r[i * 3 + j]; }
        Congruence(k, t);
        k.CopyTo(global);
    }

    static void Congruence(Span<double> k, ReadOnlySpan<double> t) {
        Span<double> scratch = stackalloc double[144];
        for (int i = 0; i < 12; i++)
            for (int j = 0; j < 12; j++) {
                double sum = 0.0;
                for (int m = 0; m < 12; m++) { sum += k[i * 12 + m] * t[m * 12 + j]; }
                scratch[i * 12 + j] = sum;
            }
        for (int i = 0; i < 12; i++)
            for (int j = 0; j < 12; j++) {
                double sum = 0.0;
                for (int m = 0; m < 12; m++) { sum += t[m * 12 + i] * scratch[m * 12 + j]; }
                k[i * 12 + j] = sum;
            }
    }

    public static ImmutableArray<ImmutableArray<int>> RedTemplate(ElementClass element) =>
        element == ElementClass.Tet4 ? TetRed : element == ElementClass.Hex8 ? HexRed
            : element == ElementClass.Tri3 ? TriRed : element == ElementClass.Quad4 ? QuadRed : [];

    public static IEnumerable<Aabb> OctreeChildren(Aabb box, Vector3 c) {
        foreach ((double X, double Y, double Z) corner in HexRef8) {
            Vector3 lo = new(corner.Item1 < 0 ? box.Lo.X : c.X, corner.Item2 < 0 ? box.Lo.Y : c.Y, corner.Item3 < 0 ? box.Lo.Z : c.Z);
            yield return new(lo, lo + (box.Span * 0.5f));
        }
    }

    public static IEnumerable<(float X, float Y, float Z)> HexCorners(Aabb box) {
        foreach ((double X, double Y, double Z) corner in HexRef8) {
            yield return (corner.Item1 < 0 ? box.Lo.X : box.Hi.X, corner.Item2 < 0 ? box.Lo.Y : box.Hi.Y, corner.Item3 < 0 ? box.Lo.Z : box.Hi.Z);
        }
    }

    static bool Contains(ImmutableArray<int> face, int node) { foreach (int v in face) { if (v == node) { return true; } } return false; }

    static IEnumerable<(double, double, double)> EdgeMidsRef(ImmutableArray<(double X, double Y, double Z)> nodes, ImmutableArray<(int A, int B)> edges) {
        foreach ((int a, int b) in edges) { yield return ((nodes[a].X + nodes[b].X) * 0.5, (nodes[a].Y + nodes[b].Y) * 0.5, (nodes[a].Z + nodes[b].Z) * 0.5); }
    }

    static IEnumerable<(double, double, double)> Grid(double[] axis, int dim) {
        foreach (double z in dim == 3 ? axis : [0.0]) foreach (double y in axis) foreach (double x in axis) { yield return (x, y, z); }
    }

    static IEnumerable<Monomial> Tensor(int[] exps, int dim) {
        foreach (int k in dim == 3 ? exps : [0]) foreach (int j in exps) foreach (int i in exps) { yield return new(i, j, k); }
    }

    static IEnumerable<(double, double, double)> WedgePrismRef() {
        foreach (double z in (double[])[-1, 0, 1]) foreach ((double X, double Y, double Z) point in TriRef6) { yield return (point.X, point.Y, z); }
    }

    static IEnumerable<Monomial> WedgePrismMonomials() {
        foreach (int c in (int[])[0, 1, 2]) foreach (Monomial term in TriP2) { yield return new(term.I, term.J, c); }
    }

    static IEnumerable<ImmutableArray<int>> HexOctants() {
        int[][] octant = [[0, 8, 20, 11, 16, 22, 26, 25], [8, 1, 9, 20, 22, 17, 23, 26], [20, 9, 2, 10, 26, 23, 18, 24], [11, 20, 10, 3, 25, 26, 24, 19],
            [16, 22, 26, 25, 4, 12, 21, 15], [22, 17, 23, 26, 12, 5, 13, 21], [26, 23, 18, 24, 21, 13, 6, 14], [25, 26, 24, 19, 15, 21, 14, 7]];
        foreach (int[] sub in octant) { yield return [.. sub]; }
    }
}
```

```mermaid
accTitle: Solver discretization flow
accDescr: Boundary shells become discrete meshes, fields, element samples, quality verdicts, and adaptive refinements.
flowchart LR
    BoundaryShell -->|Discretize| MeshKernel
    MeshKernel -->|Delaunay/Octree/Sweep/Inflation| MeshBuild
    MeshBuild -->|Pack| DiscreteMesh
    DiscreteMesh -->|FieldOf| FieldSpace
    DiscreteMesh -->|NodalXyz| ElementClass
    ElementClass -->|Sample N/dN/detJ| ShapeSample
    ElementClass -->|Metric| MeshMetric
    DiscreteMesh -->|cellError| MeshKernel
    MeshKernel -->|Refine h/p/hp| Refinement
    Refinement --> DiscreteMesh
    MeshKernel -.->|Fin fail| ComputeFault
```

## [03]-[RESEARCH]

- [BOUNDARY_TRIANGULATION]: host `Brep` input first coerces through the `Rasm` `GeometryRequest.BrepForm` host-coercion owner, then `Mesh.CreateFromBrep(brep, MeshingParameters.QualityRenderMesh)` tessellates it to a render `Mesh`.
- [BOUNDARY_TRIANGULATION]: `Rasm.Meshing` `MeshSpace.Of(Mesh native, Context)` owns the discrete host mesh, and this page's `BoundaryShell.Of(ReadOnlyMemory<float> Vertices, ReadOnlyMemory<int> Triangles)` carries its flattened host-neutral soup through `DuplicateNative().Vertices.ToFloatArray()` and `DuplicateNative().Faces.ToIntArray(asTriangles: true)`.
- [BOUNDARY_TRIANGULATION]: host/kernel composition owns parametric→discrete tessellation, so Compute never re-derives a surface mesher and no host `Brep`/`Mesh` crosses a solve signature.
- [BOUNDARY_TRIANGULATION]: `ElementClass.Sample`, owned-build `QuadratureRule` tables, and Bowyer-Watson/octree/sweep/inflation cores remain cluster-owned; `Solver/contract` consumes the `Bᵀ·D·B` handoff weighted by `Sample.DetJ`.
