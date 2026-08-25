# [COMPUTE_SOLVER_ELEMENT]

Rasm.Compute finite-element topology: one `ElementClass` `[SmartEnum<string>]` element axis carrying its reference-node table, its `Monomial` polynomial space, the corner/edge/facet topology, and a `ShapeFamily` discriminant that drives one isoparametric `Sample`, so twelve continuum element types collapse onto a Vandermonde coefficient mechanism, an explicit serendipity arm, and a rational pyramid arm — beside the owned Frame family (`beam2-euler`/`beam2-timoshenko`, the 2-node 12-DOF member rows whose `Member` closed form carries end releases by static condensation, rigid-end offsets by eccentricity transform, and semi-rigid end springs by exact in-series condensation, the owned replacement for the retired BFE/FEALiTE frame backends). One closed `CellQuality` Verdict vocabulary reads the real edge/face topology once per element over a `QualitySense` direction row, so a metric's better-is-higher law is a two-row behaviour vocabulary rather than a bool every reader re-interprets.

The volumetric facet tables are the kernel's: `Rasm/Meshing/mesh#MESH_SPACE` `CellTopology` owns the CGNS/VTK Lagrange cell roster with its node count, corner count, and reference facet table, and each volumetric row here reads `CellTopology.<row>.Facets` and `.Corners` rather than re-typing them — the kernel's own "a quadratic row shares its linear parent's facet table" law is what lets `Hex27` read `Hex8`'s and `Wedge18` read `Wedge6`'s. Integration rules arrive settled from `Rasm/Numerics/integrate#QUADRATURE`: each row declares a `ReferenceElement` reference domain and an integration ORDER, the kernel row elects the smallest owned rule at or above it, and the election rides the kernel's own `Fin` — a row whose order exceeds its domain's ceiling REFUSES BY NAME at policy admission rather than reaching an assembly fold as a success-shaped under-integration. Exact sign decisions, tessellation topology, and content-key framing are all composed: `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` frames every frame-member preimage, and `Rasm.Element` `MeasureValue` admits the dimensioned vocabulary one stratum up so this hot numeric kernel carries raw SI `double` alone.

## [01]-[INDEX]

- [02]-[ELEMENT_TOPOLOGY]: element-class axis, isoparametric sampling, reference/basis/topology rosters, and the cell-quality vocabulary.
- [03]-[FRAME_STIFFNESS]: the 2-node 12-DOF member family — local stiffness, semi-rigid springs, end releases, eccentric offsets, and the member-local triad.

## [02]-[ELEMENT_TOPOLOGY]

- Owner: `ElementClass` `[SmartEnum<string>]` element-topology rows carrying a `ShapeFamily` discriminant, a `ShearModel` behaviour column, the reference-node natural-coordinate table, the `Monomial` polynomial-space basis, the corner/edge/facet topology, the kernel `ReferenceElement` reference domain beside the integration order that elects its rule, and the `Elevate` order-elevation target — all driving one isoparametric `Sample` returning shape values, physical gradients, and the Jacobian determinant; `ShapeFamily` `[SmartEnum]` the four sampling arms; `ShearModel` `[SmartEnum<string>]` the Timoshenko shear-parameter column; `CellQuality` `[SmartEnum<string>]` the closed Verdict quality vocabulary over a `QualitySense` direction row; `ElementTopology` the reference-table, Vandermonde, and isoparametric-mapping owner; `Monomial`/`ShapeSample` the value types.
- Cases: `ElementClass` rows tet4 · tet10 · hex8 · hex20 · hex27 · wedge6 · wedge18 · pyramid5 · tri3 · tri6 · quad4 · quad8 · beam2-euler · beam2-timoshenko over four `ShapeFamily` arms (Polynomial via the Vandermonde monomial mechanism, Reduced via the explicit serendipity corner/midside formulas, Pyramid via the rational apex basis, Frame via the closed-form 12-DOF `Member` stiffness the solve contract scatters); `ShearModel` rows rigid · shearing; `CellQuality` rows scaled-jacobian · aspect-ratio · skewness · min-dihedral · condition over `QualitySense` rows higher · lower.
- Entry: `ElementClass.Sample((double, double, double) natural, ReadOnlySpan<double> nodalXyz)` is the isoparametric evaluation the assembly consumes and `ShapeGrad` its gradient projection; `ElementClass.Quadrature` is the memoized `Fin<QuadratureRule>` election every admission binds ONCE, so a consumer reads a proven rule off the admitted mesh and never re-elects; `ElementClass.Metric(CellQuality, ReadOnlySpan<double>)` reads the per-element quality; `ElementClass.Member(...)` writes the 12×12 global member stiffness.
- Auto: `Sample` evaluates the `ShapeFamily` arm — Polynomial reads the lazily-memoized per-class Vandermonde coefficient matrix `(N_i = Σ_m C[m,i]·P_m(ξ))` and its monomial derivatives, Reduced reads the explicit serendipity corner/midside formulas, Pyramid the rational apex basis — then maps reference derivatives through the inline `dim×dim` Jacobian inverse to physical `∂N/∂x` and the determinant. `MidEdges`/`MidFaces`/`MidInterior` DERIVE from the row's own node count against its edge and quad-facet tables, so the p-refinement fold mints exactly the nodes the TARGET row declares and no caller re-derives an elevation arity.
- Receipt: none — the element axis is read by `Solver/discretization`'s mesh receipt and `Solver/contract`'s solve receipt, and a per-element receipt would restate the row key both already carry.
- Packages: Rasm (project — kernel `ReferenceElement`/`QuadratureRule`, `CellTopology`, `CanonicalWriter`/`ContentHash`, `ICapability`/`CapabilitySet`, `Op`), MathNet.Numerics (`Matrix<double>.Build.Dense`/`Inverse` — the cold per-class Vandermonde factorization alone), System.Numerics.Tensors, System.Numerics (`Vector3`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new element topology is one `ElementClass` row carrying its `ShapeFamily`, reference table, monomial space, edge/facet tables, its `(ReferenceElement, IntegrationOrder)` pair, and its elevation target; a new quality measure is one `CellQuality` row carrying its measure delegate and its `QualitySense`; a higher-order Gauss rule is one entry on the kernel `ReferenceElement` ladder and lifts every row declaring that order with zero edits here; zero new surface.
- Boundary: the volumetric facet and corner tables COMPOSE `CellTopology` — the kernel owns the CGNS/VTK roster and its facet data upstream, so a byte-identical re-typing here is two owners for one fact and the deleted form; the 2-D and line rows keep page-local tables because the kernel roster carries no planar or line cell. The row's own `Faces` is therefore one column with one source per row, never a kernel read beside a local fallback.
- Boundary: the element owns its integration scaling — `ShapeSample.DetJ` is the Jacobian determinant the assembly weights each Gauss point by, never a centroid-volume approximation; the quadrature is the kernel `ReferenceElement` row's own table, reached by declaring the reference domain and the integration ORDER rather than naming a rule constant, so a 2-D element can never index a 3-D rule and a full-versus-reduced integration decision is one integer on the row. The election RAILS: `ReferenceElement.Rule` refuses `KernelFault.OutOfRange` against its own ceiling, and `Quadrature` carries that `Fin` unflattened so the kernel's typed-exhaustion law survives the crossing — a bare `QuadratureRule` slot swallows exactly the refusal the law exists to publish, and every wedge and pyramid row on the pre-repair roster proved it by declaring an order no rule could serve.
- Boundary: the quality measure is the closed `CellQuality` vocabulary read once through the element class's `Metric` delegate over the real corner-Jacobian, edge-length, face-angle, and dihedral topology, never a per-call recompute and never the first-four-nodes slice. Direction is a `QualitySense` ROW carrying both the reduction and the admission test, so no reader re-derives which end of the scale is better from a bool; the quality gate runs over the WHOLE element set before any sample is taken, so neither Jacobian inverse substitutes a floor for a vanishing determinant — a substituted pivot under a named refusal can only launder a mesh that never admitted.
- Boundary: order elevation is a NODE-COUNT derivation, not a hand roster — a target row's corners, edge midpoints, quad-facet centres, and interior node are read off its own tables in the CGNS ordering the reference table already carries, so `Hex20 → Hex27` and `Wedge6 → Wedge18` mint their seven and twelve extra nodes from the same fold that mints `Tet4 → Tet10`'s six. A terminal row elevates to itself, which is how a fully-elevated mesh stays a mesh instead of failing.
- Exemption: the `dim×dim` Jacobian inverse, the 12×12 frame congruence, the Vandermonde accumulation, and the serendipity formula bodies are MEASURED span kernels — fixed-size dense arithmetic over `stackalloc`/pooled planes with no traversal shape to fold. Every one of them dies with the call that fills it and none crosses a page surface.
- Boundary: the mesh is solve-native raw SI `double` — the typed `MeasureValue`/`Dimension` vocabulary lives at the `Rasm.Element/Properties/quantity#MEASURE_VALUE` seam and is admitted once upstream, never threaded through this hot numeric kernel; metric reductions ride the `Tensor/dispatch#KERNEL_DISPATCH` `TensorPrimitives` SIMD folds over the flat per-element span, and MathNet factors only the cold per-class Vandermonde inverse.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ShapeFamily {
    public static readonly ShapeFamily Polynomial = new(static (element, natural, nodalXyz) => element.PolynomialSample(natural, nodalXyz));
    public static readonly ShapeFamily Reduced = new(static (element, natural, nodalXyz) => element.ReducedSample(natural, nodalXyz));
    public static readonly ShapeFamily Pyramid = new(static (element, natural, nodalXyz) =>
        ElementTopology.Iso(element.PyramidShape(natural), element.PyramidGrad(natural), nodalXyz, element.Nodes, element.Dim));
    public static readonly ShapeFamily Frame = new(static (_, natural, nodalXyz) => ElementClass.LineSample(natural, nodalXyz));

    [UseDelegateFromConstructor]
    public partial ShapeSample Sample(ElementClass element, (double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShearModel {
    public static readonly ShearModel Rigid = new("rigid", static (_, _, _, _, _) => 0.0);
    public static readonly ShearModel Shearing = new("shearing", static (young, inertia, shear, shearArea, length) =>
        shearArea > 0.0 ? 12.0 * young * inertia / (shear * shearArea * length * length) : 0.0);

    [UseDelegateFromConstructor]
    public partial double Phi(double young, double inertia, double shear, double shearArea, double length);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QualitySense {
    public static readonly QualitySense Higher = new("higher",
        worst: static values => TensorPrimitives.Min(values), admits: static (worst, floor) => worst > floor);
    public static readonly QualitySense Lower = new("lower",
        worst: static values => TensorPrimitives.Max(values), admits: static (worst, ceiling) => worst < ceiling);

    [UseDelegateFromConstructor] public partial double Worst(ReadOnlySpan<double> perElement);
    [UseDelegateFromConstructor] public partial bool Admits(double worst, double threshold);
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

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ShapeSample(double[] Shape, double[] Grad, double DetJ);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElementClass {
    public static readonly ElementClass Tet4 = new("tet4", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 1,
        ReferenceElement.Tet, integrationOrder: 2, ElementTopology.TetRef4, ElementTopology.TetP1, ElementTopology.TetEdges,
        CellTopology.Tet4.Facets, CellTopology.Tet4.Corners, () => Tet10);
    public static readonly ElementClass Tet10 = new("tet10", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 2,
        ReferenceElement.Tet, integrationOrder: 2, ElementTopology.TetRef10, ElementTopology.TetP2, ElementTopology.TetEdges,
        CellTopology.Tet10.Facets, CellTopology.Tet10.Corners, () => Tet10);
    public static readonly ElementClass Hex8 = new("hex8", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 1,
        ReferenceElement.Hex, integrationOrder: 3, ElementTopology.HexRef8, ElementTopology.HexQ1, ElementTopology.HexEdges,
        CellTopology.Hex8.Facets, CellTopology.Hex8.Corners, () => Hex20);
    public static readonly ElementClass Hex20 = new("hex20", ShapeFamily.Reduced, ShearModel.Rigid, dim: 3, order: 2,
        ReferenceElement.Hex, integrationOrder: 5, ElementTopology.HexRef20, ImmutableArray<Monomial>.Empty, ElementTopology.HexEdges,
        CellTopology.Hex20.Facets, CellTopology.Hex20.Corners, () => Hex27);
    public static readonly ElementClass Hex27 = new("hex27", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 2,
        ReferenceElement.Hex, integrationOrder: 5, ElementTopology.HexRef27, ElementTopology.HexQ2, ElementTopology.HexEdges,
        CellTopology.Hex8.Facets, CellTopology.Hex8.Corners, () => Hex27);
    public static readonly ElementClass Wedge6 = new("wedge6", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 1,
        ReferenceElement.Wedge, integrationOrder: 2, ElementTopology.WedgeRef6, ElementTopology.WedgeP1, ElementTopology.WedgeEdges,
        CellTopology.Wedge6.Facets, CellTopology.Wedge6.Corners, () => Wedge18);
    public static readonly ElementClass Wedge18 = new("wedge18", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 3, order: 2,
        ReferenceElement.Wedge, integrationOrder: 5, ElementTopology.WedgeRef18, ElementTopology.WedgeP2, ElementTopology.WedgeEdges,
        CellTopology.Wedge6.Facets, CellTopology.Wedge6.Corners, () => Wedge18);
    public static readonly ElementClass Pyramid5 = new("pyramid5", ShapeFamily.Pyramid, ShearModel.Rigid, dim: 3, order: 1,
        ReferenceElement.Pyramid, integrationOrder: 2, ElementTopology.PyramidRef5, ImmutableArray<Monomial>.Empty, ElementTopology.PyramidEdges,
        CellTopology.Pyramid5.Facets, CellTopology.Pyramid5.Corners, () => Pyramid5);
    public static readonly ElementClass Tri3 = new("tri3", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 2, order: 1,
        ReferenceElement.Tri, integrationOrder: 2, ElementTopology.TriRef3, ElementTopology.TriP1, ElementTopology.TriEdges,
        ElementTopology.TriFaces, corners: 3, () => Tri6);
    public static readonly ElementClass Tri6 = new("tri6", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 2, order: 2,
        ReferenceElement.Tri, integrationOrder: 2, ElementTopology.TriRef6, ElementTopology.TriP2, ElementTopology.TriEdges,
        ElementTopology.TriFaces, corners: 3, () => Tri6);
    public static readonly ElementClass Quad4 = new("quad4", ShapeFamily.Polynomial, ShearModel.Rigid, dim: 2, order: 1,
        ReferenceElement.Quad, integrationOrder: 3, ElementTopology.QuadRef4, ElementTopology.QuadQ1, ElementTopology.QuadEdges,
        ElementTopology.QuadFaces, corners: 4, () => Quad8);
    public static readonly ElementClass Quad8 = new("quad8", ShapeFamily.Reduced, ShearModel.Rigid, dim: 2, order: 2,
        ReferenceElement.Quad, integrationOrder: 5, ElementTopology.QuadRef8, ImmutableArray<Monomial>.Empty, ElementTopology.QuadEdges,
        ElementTopology.QuadFaces, corners: 4, () => Quad8);
    public static readonly ElementClass Beam2Euler = new("beam2-euler", ShapeFamily.Frame, ShearModel.Rigid, dim: 1, order: 1,
        ReferenceElement.Line, integrationOrder: 2, ElementTopology.LineRef2, ElementTopology.LineP1, ElementTopology.LineEdges,
        ImmutableArray<ImmutableArray<int>>.Empty, corners: 2, () => Beam2Euler);
    public static readonly ElementClass Beam2Timoshenko = new("beam2-timoshenko", ShapeFamily.Frame, ShearModel.Shearing, dim: 1, order: 1,
        ReferenceElement.Line, integrationOrder: 2, ElementTopology.LineRef2, ElementTopology.LineP1, ElementTopology.LineEdges,
        ImmutableArray<ImmutableArray<int>>.Empty, corners: 2, () => Beam2Timoshenko);

    public ShapeFamily Family { get; }
    public ShearModel Shear { get; }
    public int Dim { get; }
    public int Order { get; }
    public ReferenceElement ReferenceDomain { get; }
    public int IntegrationOrder { get; }
    public ImmutableArray<(double X, double Y, double Z)> Reference { get; }
    public ImmutableArray<Monomial> Basis { get; }
    public ImmutableArray<(int A, int B)> Edges { get; }
    public ImmutableArray<ImmutableArray<int>> Faces { get; }
    public int Corners { get; }

    [UseDelegateFromConstructor]
    public partial ElementClass Elevate();

    public int Nodes => Reference.Length;
    public ImmutableArray<int> Ordinals => ElementTopology.Ordinals(Corners);

    public int MidEdges => Math.Clamp(Nodes - Corners, 0, Edges.Length);
    public int MidFaces => Math.Clamp(Nodes - Corners - MidEdges, 0, QuadFacets);
    public int MidInterior => Math.Max(0, Nodes - Corners - MidEdges - MidFaces);
    int QuadFacets => Faces.Count(static facet => facet.Length == 4);

    private static readonly Op QuadratureKey = Op.Of(name: nameof(Quadrature));

    private static readonly Lazy<FrozenDictionary<ElementClass, Fin<QuadratureRule>>> Rules = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => row.ReferenceDomain.Rule(order: row.IntegrationOrder, key: QuadratureKey)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public Fin<QuadratureRule> Quadrature => Rules.Value[this];

    static readonly ConcurrentDictionary<ElementClass, double[,]> Vandermondes = new();
    private double[,] Coefficients => Vandermondes.GetOrAdd(this, static row => ElementTopology.Vandermonde(row.Reference, row.Basis));

    public ShapeSample Sample((double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz) => Family.Sample(this, natural, nodalXyz);

    public double[] ShapeGrad((double X, double Y, double Z) natural, ReadOnlySpan<double> nodalXyz) => Sample(natural, nodalXyz).Grad;

    public double Metric(CellQuality metric, ReadOnlySpan<double> nodalXyz) => metric.Measure(this, nodalXyz);

    internal static ShapeSample LineSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
        double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double[] grad = [-dx / (l * l), -dy / (l * l), -dz / (l * l), dx / (l * l), dy / (l * l), dz / (l * l)];
        return new([0.5 * (1.0 - nat.X), 0.5 * (1.0 + nat.X)], grad, l * 0.5);
    }

    internal ShapeSample PolynomialSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        int n = Nodes;
        double[,] c = Coefficients;
        using SpanOwner<double> plane = SpanOwner<double>.Allocate(n * 4);
        Span<double> shape = plane.Span[..n];
        Span2D<double> dnRef = Span2D<double>.DangerousCreate(ref plane.Span[n], n, 3, 0);
        plane.Span.Clear();
        for (int m = 0; m < n; m++) {
            double p = Basis[m].Eval(nat), dpx = Basis[m].D(0, nat), dpy = Basis[m].D(1, nat), dpz = Basis[m].D(2, nat);
            for (int i = 0; i < n; i++) {
                double a = c[m, i];
                shape[i] += a * p; dnRef[i, 0] += a * dpx; dnRef[i, 1] += a * dpy; dnRef[i, 2] += a * dpz;
            }
        }
        return ElementTopology.Iso(shape, dnRef, xyz, n, Dim);
    }

    internal ShapeSample ReducedSample((double X, double Y, double Z) nat, ReadOnlySpan<double> xyz) {
        int n = Nodes;
        using SpanOwner<double> plane = SpanOwner<double>.Allocate(n * 4);
        Span<double> shape = plane.Span[..n];
        Span2D<double> dn = Span2D<double>.DangerousCreate(ref plane.Span[n], n, 3, 0);
        for (int i = 0; i < n; i++) {
            (double X, double Y, double Z) reference = Reference[i];
            (double s, double dx, double dy, double dz) = Dim == 2 ? Serendipity2(nat, reference) : Serendipity3(nat, reference);
            shape[i] = s; dn[i, 0] = dx; dn[i, 1] = dy; dn[i, 2] = dz;
        }
        return ElementTopology.Iso(shape, dn, xyz, n, Dim);
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
        double inv = 1.0 / Math.Max(EpsilonPolicy.ZeroTolerance, 1.0 - p.Z);
        for (int i = 0; i < 4; i++) {
            (double X, double Y, double Z) reference = Reference[i];
            n[i] = 0.25 * ((1 - p.Z) + reference.X * p.X + reference.Y * p.Y + reference.X * reference.Y * p.X * p.Y * inv);
        }
        n[4] = p.Z;
        return n;
    }

    internal Span2D<double> PyramidGrad((double X, double Y, double Z) p) {
        double[] flat = new double[15];
        Span2D<double> dn = new(flat, 5, 3);
        double inv = 1.0 / Math.Max(EpsilonPolicy.ZeroTolerance, 1.0 - p.Z), inv2 = inv * inv;
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
public sealed partial class CellQuality {
    public static readonly CellQuality ScaledJacobian = new("scaled-jacobian", QualitySense.Higher, ScaledJacobianMeasure);
    public static readonly CellQuality AspectRatio = new("aspect-ratio", QualitySense.Lower, AspectRatioMeasure);
    public static readonly CellQuality Skewness = new("skewness", QualitySense.Lower, SkewnessMeasure);
    public static readonly CellQuality MinDihedral = new("min-dihedral", QualitySense.Higher, MinDihedralMeasure);
    public static readonly CellQuality Condition = new("condition", QualitySense.Lower, ConditionMeasure);

    public QualitySense Sense { get; }

    [UseDelegateFromConstructor]
    public partial double Measure(ElementClass element, ReadOnlySpan<double> nodalXyz);

    public double Worst(ReadOnlySpan<double> perElement) => perElement.IsEmpty ? 0.0 : Sense.Worst(perElement);
    public bool Admits(double worst, double threshold) => Sense.Admits(worst, threshold);

    static double ScaledJacobianMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double worst = double.MaxValue;
        foreach (int corner in element.Ordinals) {
            (Vector3 e1, Vector3 e2, Vector3 e3) = ElementTopology.CornerFrame(element, corner, xyz);
            double det = Vector3.Dot(Vector3.Cross(e1, e2), e3);
            double scale = (double)e1.Length() * e2.Length() * e3.Length();
            worst = Math.Min(worst, scale > EpsilonPolicy.ZeroTolerance ? det / scale : 0.0);
        }
        return worst == double.MaxValue ? 0.0 : worst;
    }

    static double AspectRatioMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double longest = 0.0, shortest = double.MaxValue;
        foreach ((int a, int b) in element.Edges) {
            double length = (ElementTopology.Node(xyz, b) - ElementTopology.Node(xyz, a)).Length();
            longest = Math.Max(longest, length); shortest = Math.Min(shortest, length);
        }
        return shortest > EpsilonPolicy.ZeroTolerance ? longest / shortest : double.MaxValue;
    }

    static double SkewnessMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double worst = 0.0;
        foreach (ImmutableArray<int> face in element.Faces) {
            double ideal = face.Length == 3 ? 60.0 : 90.0;
            for (int i = 0; i < face.Length; i++) {
                Vector3 o = ElementTopology.Node(xyz, face[i]);
                Vector3 u = ElementTopology.Node(xyz, face[(i + 1) % face.Length]) - o, v = ElementTopology.Node(xyz, face[(i + face.Length - 1) % face.Length]) - o;
                double angle = Math.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(u), Vector3.Normalize(v)), -1.0, 1.0)) * 180.0 / Math.PI;
                worst = Math.Max(worst, Math.Max((angle - ideal) / (180.0 - ideal), (ideal - angle) / ideal));
            }
        }
        return worst;
    }

    static double MinDihedralMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double smallest = 180.0;
        foreach ((int a, int b) in element.Edges) {
            ImmutableArray<ImmutableArray<int>> incident = ElementTopology.FacesOnEdge(element, a, b);
            if (incident.Length < 2) { continue; }
            Vector3 n1 = ElementTopology.FaceNormal(incident[0], xyz), n2 = ElementTopology.FaceNormal(incident[1], xyz);
            double angle = 180.0 - Math.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(n1), Vector3.Normalize(n2)), -1.0, 1.0)) * 180.0 / Math.PI;
            smallest = Math.Min(smallest, angle);
        }
        return smallest;
    }

    static double ConditionMeasure(ElementClass element, ReadOnlySpan<double> xyz) {
        double jacobian = Math.Abs(ScaledJacobianMeasure(element, xyz));
        return jacobian > EpsilonPolicy.ZeroTolerance ? 1.0 / jacobian : double.MaxValue;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ElementTopology {
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

    public static readonly ImmutableArray<ImmutableArray<int>> TriFaces = [[0, 1, 2]];
    public static readonly ImmutableArray<ImmutableArray<int>> QuadFaces = [[0, 1, 2, 3]];

    public static ImmutableArray<int> Ordinals(int count) => [.. Enumerable.Range(0, count)];

    public static double[,] Vandermonde(ImmutableArray<(double X, double Y, double Z)> nodes, ImmutableArray<Monomial> basis) {
        int n = nodes.Length;
        Matrix<double> inverse = Matrix<double>.Build.Dense(n, n, (i, m) => basis[m].Eval(nodes[i])).Inverse();
        double[,] c = new double[n, n];
        for (int m = 0; m < n; m++) for (int i = 0; i < n; i++) { c[m, i] = inverse[m, i]; }
        return c;
    }

    public static ShapeSample Iso(ReadOnlySpan<double> shape, ReadOnlySpan2D<double> dnRef, ReadOnlySpan<double> xyz, int nodes, int dim) {
        Span<double> j = stackalloc double[9];
        j.Clear();
        for (int i = 0; i < nodes; i++) for (int a = 0; a < dim; a++) for (int b = 0; b < dim; b++) { j[a * 3 + b] += dnRef[i, a] * xyz[i * 3 + b]; }
        Span<double> inv = stackalloc double[9];
        double det = dim == 3 ? Invert3(j, inv) : Invert2(j, inv);
        double[] grad = new double[nodes * 3];
        for (int i = 0; i < nodes; i++)
            for (int b = 0; b < dim; b++) { double s = 0.0; for (int a = 0; a < dim; a++) { s += dnRef[i, a] * inv[a * 3 + b]; } grad[i * 3 + b] = s; }
        return new(shape.ToArray(), grad, det);
    }

    static double Invert2(ReadOnlySpan<double> j, Span<double> inv) {
        double det = j[0] * j[4] - j[1] * j[3], s = 1.0 / det;
        inv[0] = j[4] * s; inv[1] = -j[1] * s; inv[3] = -j[3] * s; inv[4] = j[0] * s;
        return det;
    }

    static double Invert3(ReadOnlySpan<double> j, Span<double> inv) {
        double c00 = j[4] * j[8] - j[5] * j[7], c01 = j[5] * j[6] - j[3] * j[8], c02 = j[3] * j[7] - j[4] * j[6];
        double det = j[0] * c00 + j[1] * c01 + j[2] * c02, s = 1.0 / det;
        inv[0] = c00 * s; inv[1] = (j[2] * j[7] - j[1] * j[8]) * s; inv[2] = (j[1] * j[5] - j[2] * j[4]) * s;
        inv[3] = c01 * s; inv[4] = (j[0] * j[8] - j[2] * j[6]) * s; inv[5] = (j[2] * j[3] - j[0] * j[5]) * s;
        inv[6] = c02 * s; inv[7] = (j[1] * j[6] - j[0] * j[7]) * s; inv[8] = (j[0] * j[4] - j[1] * j[3]) * s;
        return det;
    }

    public static Vector3 Node(ReadOnlySpan<double> xyz, int index) => new((float)xyz[index * 3], (float)xyz[index * 3 + 1], (float)xyz[index * 3 + 2]);

    public static (Vector3 E1, Vector3 E2, Vector3 E3) CornerFrame(ElementClass element, int corner, ReadOnlySpan<double> xyz) {
        Vector3 o = Node(xyz, corner);
        Span<Vector3> incident = stackalloc Vector3[3];
        int found = 0;
        foreach ((int a, int b) in element.Edges) {
            if (found == 3) { break; }
            if (a == corner) { incident[found++] = Node(xyz, b) - o; }
            else if (b == corner) { incident[found++] = Node(xyz, a) - o; }
        }
        return element.Dim == 3 ? (incident[0], incident[1], incident[2]) : (incident[0], incident[1], Vector3.Cross(incident[0], incident[1]));
    }

    public static ImmutableArray<ImmutableArray<int>> FacesOnEdge(ElementClass element, int a, int b) =>
        [.. element.Faces.Where(face => face.Contains(a) && face.Contains(b))];

    public static Vector3 FaceNormal(ImmutableArray<int> face, ReadOnlySpan<double> xyz) =>
        Vector3.Cross(Node(xyz, face[1]) - Node(xyz, face[0]), Node(xyz, face[2]) - Node(xyz, face[0]));

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
}
```

## [03]-[FRAME_STIFFNESS]

- Owner: `DofRelease` `[SmartEnum<string>] : ICapability<DofRelease>` closes the twelve member-local degrees of freedom as a capability vocabulary; `FrameMember` carries the section, the member roll, the release set, the rigid-end offsets, and the semi-rigid end springs; `FrameKernel` owns the local stiffness, the spring fold, the release condensation, the eccentricity transform, and the member-local triad every load resolution and station recovery reads back.
- Cases: `DofRelease` rows axial-i · shear-y-i · shear-z-i · torsion-i · bending-y-i · bending-z-i and their j-end peers, each carrying its own local row index; a release SET is `CapabilitySet<DofRelease>`, so the twelve-corner product a bitmask spelled as one `int` becomes membership over a closed roster.
- Entry: `ElementClass.Member(ReadOnlySpan<double> xyz, in FrameMember member, double young, double poisson, Span<double> local)` folds local stiffness, spring, release, eccentricity, and rotation in that order and writes the 12×12 global block; `FrameKernel.Triad` exposes the same member-local frame the stiffness was rotated with; `FrameMember.WriteCanonical(CanonicalWriter)` frames the member's content-key preimage.
- Auto: the shear parameter reads the row's own `ShearModel` column, so a Timoshenko section and an Euler section differ by a row rather than by two ternaries reading a bool; the release condensation walks the release SET rather than twelve bit tests, so a mask corner nothing names is unrepresentable.
- Receipt: none — the member block lands in `Solver/contract`'s assembly and its evidence is that fold's.
- Packages: Rasm (project — `CanonicalWriter`/`ContentHash`, `ICapability`/`CapabilitySet`, `EpsilonPolicy`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new end condition is one `DofRelease` row and zero condensation edits; a new section column is one `FrameMember` field with its `WriteCanonical` line; zero new surface.
- Boundary: the canonical preimage is the kernel's — `CanonicalWriter` is the ONE public way to emit a multi-field preimage, and a hand `ArrayBufferWriter<byte>` walk writing fifteen unframed doubles with no length prefix and no tolerance quantization is the deleted form the identity owner exists to foreclose. Tolerance is PART OF THE KEY, so a member preimage carries the caller's grid rather than raw bits two near-identical sections address as two identities.
- Boundary: `Up` carries the member roll — the AxisCurve's own orientation vector — so the local triad is (x̂ along axis, ẑ the up projected orthogonal to x̂, ŷ = ẑ×x̂); a roll derived from the global direction alone cannot represent an arbitrarily rotated section and is the deleted form. A near-parallel up degenerates to the global-Z (or global-Y for verticals) fallback so the triad stays orthonormal.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DofRelease : ICapability<DofRelease> {
    public static readonly DofRelease AxialI = new("axial-i", local: 0);
    public static readonly DofRelease ShearYI = new("shear-y-i", local: 1);
    public static readonly DofRelease ShearZI = new("shear-z-i", local: 2);
    public static readonly DofRelease TorsionI = new("torsion-i", local: 3);
    public static readonly DofRelease BendingYI = new("bending-y-i", local: 4);
    public static readonly DofRelease BendingZI = new("bending-z-i", local: 5);
    public static readonly DofRelease AxialJ = new("axial-j", local: 6);
    public static readonly DofRelease ShearYJ = new("shear-y-j", local: 7);
    public static readonly DofRelease ShearZJ = new("shear-z-j", local: 8);
    public static readonly DofRelease TorsionJ = new("torsion-j", local: 9);
    public static readonly DofRelease BendingYJ = new("bending-y-j", local: 10);
    public static readonly DofRelease BendingZJ = new("bending-z-j", local: 11);

    public int Local { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

[Equatable]
public sealed partial record FrameMember(
    double Area, double Iy, double Iz, double J,
    double Iw,
    double UpX, double UpY, double UpZ,
    [property: UnorderedEquality] CapabilitySet<DofRelease> Releases,
    double OffsetI, double OffsetJ,
    double SpringYi, double SpringZi, double SpringYj, double SpringZj,
    double ShearAreaY, double ShearAreaZ) {
    public static FrameMember Prismatic(double area, double iy, double iz, double j) =>
        new(area, iy, iz, j, Iw: 0.0, UpX: 0.0, UpY: 0.0, UpZ: 1.0, Releases: CapabilitySet<DofRelease>.None,
            OffsetI: 0.0, OffsetJ: 0.0,
            SpringYi: double.PositiveInfinity, SpringZi: double.PositiveInfinity,
            SpringYj: double.PositiveInfinity, SpringZj: double.PositiveInfinity,
            ShearAreaY: 0.0, ShearAreaZ: 0.0);

    public void WriteCanonical(CanonicalWriter sink) {
        sink.Double(Area).Double(Iy).Double(Iz).Double(J)
            .Double(UpX).Double(UpY).Double(UpZ)
            .Double(OffsetI).Double(OffsetJ)
            .Double(SpringYi).Double(SpringZi).Double(SpringYj).Double(SpringZj)
            .Double(ShearAreaY).Double(ShearAreaZ)
            .Rows(toSeq(Releases.Held.OrderBy(static row => row.Local)), static (row, writer) => writer.Ordinal(row.Local));
    }

    public UInt128 Key(double tolerance) => ContentHash.Of(this, static (member, sink) => member.WriteCanonical(sink), tolerance);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class FrameKernel {
    public static Fin<Unit> Member(ElementClass element, ReadOnlySpan<double> xyz, in FrameMember member, double young, double poisson, Span<double> global) {
        double dx = xyz[3] - xyz[0], dy = xyz[4] - xyz[1], dz = xyz[5] - xyz[2];
        double length = Math.Sqrt(dx * dx + dy * dy + dz * dz) - member.OffsetI - member.OffsetJ;
        if (!(length > 0.0)) {
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
        }
        double shear = young / (2.0 * (1.0 + poisson));
        double phiY = element.Shear.Phi(young, member.Iy, shear, member.ShearAreaZ, length);
        double phiZ = element.Shear.Phi(young, member.Iz, shear, member.ShearAreaY, length);
        Span<double> k = stackalloc double[144];
        LocalFrame(k, young, shear, member, length, phiY, phiZ);
        SemiRigid(k, member);
        CondenseReleases(k, member.Releases);
        Eccentric(k, member.OffsetI, member.OffsetJ);
        RotateFrame(k, dx, dy, dz, member.UpX, member.UpY, member.UpZ, global);
        return Fin.Succ(unit);
    }

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

    public static void SemiRigid(Span<double> k, in FrameMember m) {
        Fold(k, DofRelease.BendingYI.Local, m.SpringYi); Fold(k, DofRelease.BendingZI.Local, m.SpringZi);
        Fold(k, DofRelease.BendingYJ.Local, m.SpringYj); Fold(k, DofRelease.BendingZJ.Local, m.SpringZj);
        static void Fold(Span<double> k, int d, double spring) {
            if (double.IsPositiveInfinity(spring) || k[d * 12 + d] <= 0.0) { return; }
            double alpha = spring / (spring + k[d * 12 + d]);
            for (int i = 0; i < 12; i++) { if (i != d) { k[d * 12 + i] *= alpha; k[i * 12 + d] *= alpha; } }
            k[d * 12 + d] *= alpha;
        }
    }

    public static void CondenseReleases(Span<double> k, CapabilitySet<DofRelease> releases) {
        foreach (DofRelease release in releases.Held.OrderBy(static row => row.Local)) {
            int d = release.Local;
            double pivot = k[d * 12 + d];
            if (Math.Abs(pivot) < EpsilonPolicy.ZeroTolerance) { continue; }
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

    public static void Triad(double dx, double dy, double dz, double upX, double upY, double upZ, Span<double> r) {
        double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double cx = dx / l, cy = dy / l, cz = dz / l;
        double dot = upX * cx + upY * cy + upZ * cz;
        double zx = upX - dot * cx, zy = upY - dot * cy, zz = upZ - dot * cz;
        double zn = Math.Sqrt(zx * zx + zy * zy + zz * zz);
        if (zn < EpsilonPolicy.SqrtEpsilon) {
            (zx, zy, zz) = Math.Abs(cz) < 0.9 ? (0.0, 0.0, 1.0) : (0.0, 1.0, 0.0);
            dot = zx * cx + zy * cy + zz * cz;
            (zx, zy, zz) = (zx - dot * cx, zy - dot * cy, zz - dot * cz);
            zn = Math.Sqrt(zx * zx + zy * zy + zz * zz);
        }
        (zx, zy, zz) = (zx / zn, zy / zn, zz / zn);
        double yx = zy * cz - zz * cy, yy = zz * cx - zx * cz, yz = zx * cy - zy * cx;
        r[0] = cx; r[1] = cy; r[2] = cz;
        r[3] = yx; r[4] = yy; r[5] = yz;
        r[6] = zx; r[7] = zy; r[8] = zz;
    }

    public static void RotateFrame(Span<double> k, double dx, double dy, double dz, double upX, double upY, double upZ, Span<double> global) {
        Span<double> r = stackalloc double[9];
        Triad(dx, dy, dz, upX, upY, upZ, r);
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
}
```

## [04]-[RESEARCH]

(none)
