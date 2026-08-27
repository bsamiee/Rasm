# 1. Remove imports made obsolete by immutable projection and the existing numerics namespace
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L26-L35**
```csharp
using System.Linq;
using Dimension = Rasm.Numerics.Dimension;
```
**To**
```csharp
// System.Linq and Dimension alias DELETED
```
**Why**
`Arr<T>.Map` owns the only LINQ projection, and `Dimension` is already unambiguous through `Rasm.Numerics`; retaining either import adds an unnecessary resolution path.
**Change**
Project limit rows with `Arr<T>.Map` and reference `Dimension` through the imported numerics namespace.
**Delta**
LOC -2; imports -2; symbols 0.

# 2. Store fixed stencil coefficients as data and remove the public cache-forwarding member
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L44-L71**
```csharp
    public static readonly SubdivisionScheme CatmullClark = new(
        "catmull-clark", arity: 4,
        vertexStencil: static (valence, sharpness) => CcVertex(valence, sharpness),
        edgeStencil: static () => (0.25, 0.25),
        limitStencil: static valence => (valence / (valence + 5d), 4d / (valence * (valence + 5d))),
        tangentWeight: static (valence, index) => TangentCc(valence, index),
        boundaryVertexStencil: static () => (0.75, 0.125),
        boundaryEdgeStencil: static () => 0.5);

    public static readonly SubdivisionScheme Loop = new(
        "loop", arity: 3,
        vertexStencil: static (valence, sharpness) => LoopVertex(valence, sharpness),
        edgeStencil: static () => (0.375, 0.125),
        limitStencil: static valence => (1d - valence * Beta(valence), Beta(valence)),
        tangentWeight: static (valence, index) => TangentLoop(valence, index),
        boundaryVertexStencil: static () => (0.75, 0.125),
        boundaryEdgeStencil: static () => 0.5);

    public partial (double Self, double Neighbor) VertexStencil(int valence, double sharpness);
    public partial (double Ends, double Wings) EdgeStencil();
    public partial (double Self, double Ring) LimitStencil(int valence);
    public partial double TangentWeight(int valence, int index);
    public partial (double Self, double End) BoundaryVertexStencil();
    public partial double BoundaryEdgeStencil();

    public Fin<StamBasis> Eigenbasis(int valence, Matrix neighborhood, Op key) =>
        StamCache.For(this, valence, neighborhood, key);
```
**To**
```csharp
    public static readonly SubdivisionScheme CatmullClark = new(
        "catmull-clark", arity: 4,
        edgeStencil: (0.25, 0.25), boundaryVertexStencil: (0.75, 0.125), boundaryEdgeStencil: 0.5,
        vertexStencil: static (valence, sharpness) => CcVertex(valence, sharpness),
        limitStencil: static valence => (valence / (valence + 5d), 4d / (valence * (valence + 5d))),
        tangentWeight: static (valence, index) => TangentCc(valence, index));

    public static readonly SubdivisionScheme Loop = new(
        "loop", arity: 3,
        edgeStencil: (0.375, 0.125), boundaryVertexStencil: (0.75, 0.125), boundaryEdgeStencil: 0.5,
        vertexStencil: static (valence, sharpness) => LoopVertex(valence, sharpness),
        limitStencil: static valence => (1d - valence * Beta(valence), Beta(valence)),
        tangentWeight: static (valence, index) => TangentLoop(valence, index));

    internal (double Ends, double Wings) EdgeStencil { get; }
    internal (double Self, double End) BoundaryVertexStencil { get; }
    internal double BoundaryEdgeStencil { get; }

    internal partial (double Self, double Neighbor) VertexStencil(int valence, double sharpness);
    internal partial (double Self, double Ring) LimitStencil(int valence);
    internal partial double TangentWeight(int valence, int index);

    // Eigenbasis DELETED
```
**Why**
Zero-argument delegates turn constants into methods and expose implementation coefficients as public behavior. `Eigenbasis` is a one-call forwarding shell into a module-private concern. The generated smart-enum constructor already supports ordinary data columns followed by delegate columns.
**Change**
Declare the three fixed coefficients as internal generated columns, retain delegates only for coefficients that depend on valence or sharpness, narrow those delegates to module use, and call the private basis cache directly from subdivision evaluation.
**Delta**
LOC -5; members -1; public members -7.

# 3. Reduce the policy shell to the sharpness data it actually owns
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L78-L85**
```csharp
// ── Policy ───────────────────────────────────────────────────────────────────
public sealed record SubdividePolicy(
    Arr<(int A, int B, double Sharpness)> Creases,
    Arr<(int Vertex, double Sharpness)> Corners,
    Arr<int> Region) : IValidityEvidence
{
    public static readonly SubdividePolicy Canonical = new([], [], []);
    public bool IsValid =>
        Creases.ForAll(static e => e.A != e.B && RhinoMath.IsValidDouble(e.Sharpness) && e.Sharpness > 0) &&
        Corners.ForAll(static c => c.Vertex >= 0 && RhinoMath.IsValidDouble(c.Sharpness) && c.Sharpness > 0);
}
```
**To**
```csharp
// ── Sharpness ────────────────────────────────────────────────────────────────
public sealed record Sharpness(
    Arr<(int A, int B, double Value)> Creases,
    Arr<(int Vertex, double Value)> Corners);
```
**Why**
The current record mixes crease/corner data with a refine-only face selection, provides a canonical instance solely to mean absence, and advertises validity without mesh bounds, canonical edge identity, duplicate, or region checks. Validity is relational to `MeshSpace`, so this value cannot truthfully own it.
**Change**
Rename the carrier to the domain term it represents, remove `Region`, `Canonical`, and `IValidityEvidence`, represent absence with `Option<Sharpness>`, and perform complete normalization and admission once beside the mesh-consuming operation.
**Delta**
LOC -6; types 0; members -3.

# 4. Accumulate independent face-sample errors before converting to the effect rail
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L95-L99**
```csharp
        from ordinal in key.Demand(face >= 0, "subdivide.sample.face", $"face index {face} is negative").Map(_ => face)
        from du in key.AcceptValidated<UnitInterval>(u)
        from dv in key.AcceptValidated<UnitInterval>(v)
        select new FaceSample(ordinal, du, dv);
```
**To**
```csharp
        (
            key.Demand(face >= 0, "subdivide.sample.face", $"face index {face} is negative").ToValidation().Map(_ => face),
            key.AcceptValidated<UnitInterval>(u).ToValidation(),
            key.AcceptValidated<UnitInterval>(v).ToValidation()
        ).Apply(static (ordinal, du, dv) => new FaceSample(ordinal, du, dv)).As().ToFin();
```
**Why**
Face ordinal, `u`, and `v` are independent input gates. Monadic binding suppresses later diagnostics after the first failure; applicative validation preserves the same admitted type with complete error accumulation.
**Change**
Lift each gate to `Validation<Error,T>`, apply the private constructor once, and cross to `Fin` only after admission is complete. Keep topology-dependent face bounds and triangular-domain checks at operation admission, where the mesh and scheme are available.
**Delta**
LOC +1; types 0; members 0.

# 5. Internalize the Stam cache and retain a solve factor instead of a materialized inverse
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L102-L116**
```csharp
public sealed record StamBasis(int Valence, Arr<double> Eigenvalues, Matrix Basis, Matrix InverseBasis);

internal static class StamCache
{
    private static readonly Atom<HashMap<(SubdivisionScheme Scheme, int Valence), StamBasis>> Cache = Atom(HashMap<(SubdivisionScheme, int), StamBasis>());

    internal static Fin<StamBasis> For(SubdivisionScheme scheme, int valence, Matrix neighborhood, Op key) =>
        Cache.Value.Find((scheme, valence)).Match(
            Some: Fin<StamBasis>.Succ,
            None: () => Assemble(scheme, valence, neighborhood, key).Bind(basis =>
                Cache.Cell.Claim(snapshot => snapshot.Find((scheme, valence)).Match(
                    Some: _ => None,
                    None: () => Some(snapshot.Add((scheme, valence), basis))))
                .Current.Find((scheme, valence)).ToFin(key.InvalidResult())));

    private static Fin<StamBasis> Assemble(SubdivisionScheme scheme, int valence, Matrix neighborhood, Op key);
}
```
**To**
```csharp
    private sealed record StamBasis(Arr<double> Eigenvalues, Matrix Basis, LuResult Factor);

    private static readonly Atom<HashMap<(SubdivisionScheme Scheme, int Valence), StamBasis>> Bases =
        Atom(HashMap<(SubdivisionScheme, int), StamBasis>());

    private static Fin<StamBasis> Basis(SubdivisionScheme scheme, int valence, Matrix neighborhood, Op key) =>
        Bases.Value.Find((scheme, valence)).Match(
            Some: Fin<StamBasis>.Succ,
            None: () => AssembleBasis(scheme, valence, neighborhood, key).Bind(basis =>
                Bases.Cell.Claim(snapshot => snapshot.Find((scheme, valence)).Match(
                    Some: _ => None,
                    None: () => Some(snapshot.Add((scheme, valence), basis))))
                .Current.Find((scheme, valence)).ToFin(key.InvalidResult())));

    private static Fin<StamBasis> AssembleBasis(SubdivisionScheme scheme, int valence, Matrix neighborhood, Op key);
```
**Why**
`StamBasis.Valence` duplicates the cache key, the public basis type leaks a private evaluator detail, and an explicit inverse duplicates a reusable factor while worsening numerical behavior. `LuResult.SolveDetailed` is the numerics owner for repeated right-hand-side solves and preserves residual and stop evidence.
**Change**
Move the replacement block to the top of `Subdivision`, retain the settled `(scheme, valence)` key and `Cell.Claim` winner read-back, remove the forwarding cache class, and have assembly retain `Matrix.DecomposeLu()` output. Evaluation must consume the whole `LinearSolution` verdict before accepting its vector.
**Delta**
LOC -1; module-level types -2; private nested types +1; public types -1.

# 6. Put region selection on refinement, remove the duplicate context, and reuse the limit field carrier
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L122-L132**
```csharp
    public sealed record Refine(
        MeshSpace Space, SubdivisionScheme Scheme, Dimension Levels, SubdividePolicy Policy,
        Context Model, Option<Arr<Point2d>> Uv = default) : SubdivideOp;
    public sealed record Limit(
        MeshSpace Space, SubdivisionScheme Scheme, Arr<FaceSample> Samples, SubdividePolicy Policy) : SubdivideOp;

    public sealed record Refined(
        MeshSpace Mesh, Arr<Point3d> LimitPositions, Arr<Vector3d> LimitNormals,
        Option<Arr<Point2d>> Uv) : SubdivisionResult;
```
**To**
```csharp
    public sealed record Refine(
        MeshSpace Space, SubdivisionScheme Scheme, Dimension Levels, Arr<int> Region = default,
        Option<Sharpness> Sharpness = default, Option<Arr<Point2d>> Uv = default) : SubdivideOp;
    public sealed record Limit(
        MeshSpace Space, SubdivisionScheme Scheme, Arr<FaceSample> Samples,
        Option<Sharpness> Sharpness = default) : SubdivideOp;

    public sealed record Refined(
        MeshSpace Mesh, LimitField Limit, Option<Arr<Point2d>> Uv) : SubdivisionResult;
```
**Why**
`Region` has no meaning for pointwise limit sampling, `Context Model` duplicates `MeshSpace.Tolerance`, and `Refined` repeats the exact point/normal product already named by `LimitField`. Optional sharpness expresses the smooth case without a sentinel record.
**Change**
Move region selection to `Refine`, read tolerance exclusively from `Space`, admit optional sharpness for both operations, and make publication construct one `LimitField` used either directly or inside `Refined`. Preserve `Uv` as parametric provenance; it is not interchangeable with `MeshEdit.SetCornerUv` wedge data.
**Delta**
LOC 0; types 0; members -1.

# 7. Admit mesh-relative features once and replace the refinement tuple with private state
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L142-L159**
```csharp
    // Refinement composes one admitted seed and one repeated level transition.
    private static Fin<SubdivisionResult> RefineOf(SubdivideOp.Refine op, Op key) =>
        !op.Policy.IsValid
            ? key.InvalidInput<SubdivisionResult>()
            : AdmitBase(op).Bind(seed =>
                Range(0, op.Levels.Value).Fold(
                    Fin<(SubdivisionLevel Level, HashMap<(int, int), double> Creases, HashMap<int, double> Corners, int Closures)>.Succ(seed),
                    (state, level) => state.Bind(current =>
                        Advance(op.Scheme, current.Level, current.Creases, current.Corners, op.Policy.Region, level)))
                .Bind(final => Publish(op, final.Level, final.Closures, key)));

    internal sealed record SubdivisionLevel(MeshSpace Space, EdgeTable Edges, Arr<Arr<int>> VertexFaces);
    internal sealed record EdgeTable(Arr<(int A, int B)> Edges, Arr<int> OppositeA, Arr<int> OppositeB);

    private static Fin<(SubdivisionLevel Level, HashMap<(int, int), double> Creases, HashMap<int, double> Corners, int Closures)> AdmitBase(SubdivideOp.Refine op);
    private static Fin<(SubdivisionLevel Level, HashMap<(int, int), double> Creases, HashMap<int, double> Corners, int Closures)> Advance(
        SubdivisionScheme scheme, SubdivisionLevel current, HashMap<(int, int), double> creases,
        HashMap<int, double> corners, Arr<int> region, int level);
    private static Fin<SubdivisionResult> Publish(SubdivideOp.Refine request, SubdivisionLevel level, int closures, Op key);
```
**To**
```csharp
    private sealed record Level(MeshSpace Space, Edges Edges, Arr<Arr<int>> VertexFaces);
    private sealed record Edges(Arr<(int A, int B)> Items, Arr<int> OppositeA, Arr<int> OppositeB);
    private sealed record State(
        Level Level, HashMap<(int A, int B), double> Creases,
        HashMap<int, double> Corners, Set<int> Region, int Closures);

    private static Fin<SubdivisionResult> RefineOf(SubdivideOp.Refine op, Op key) =>
        Admit(op, key).Bind(seed =>
            Range(0, op.Levels.Value).Fold(
                Fin<State>.Succ(seed),
                (state, iteration) => state.Bind(current => Advance(op.Scheme, current, iteration)))
            .Bind(final => Publish(op, final, key)));

    private static Fin<(HashMap<(int A, int B), double> Creases, HashMap<int, double> Corners)> Admit(
        MeshSpace space, Option<Sharpness> sharpness, Op key);
    private static Fin<State> Admit(SubdivideOp.Refine op, Op key);
    private static Fin<State> Advance(SubdivisionScheme scheme, State current, int iteration);
    private static Fin<SubdivisionResult> Publish(SubdivideOp.Refine request, State state, Op key);
```
**Why**
The current boolean precheck is incomplete and duplicates a second admission step, while the evolving four-value tuple is repeated across the fold and method signatures. The two topology carriers also have no consumers outside this module and should not be `internal`.
**Change**
Leave generated `Apply(...).Switch(...)` dispatch unchanged because only the selected arm may run. In the shared admission overload, accumulate sharpness errors; reject non-finite or non-positive values, self-edges, out-of-range vertex ids, and duplicate undirected edge/corner keys; then canonicalize edge keys. In the refine overload, also require face arity to equal `Scheme.Arity`, `Uv` count to equal mesh vertex count, levels to be admitted, and region face ids to be in range before converting region to `Set<int>`. Return one `State`, and pass that carrier through the fold and publication.
**Delta**
LOC -2; private types +1; members +1; internal types -2.

# 8. Share limit admission and project the traversed immutable rows directly
**From — libs/dotnet/Rasm/.planning/Parametric/subdivide.md:L162-L169**
```csharp
    private static Fin<SubdivisionResult> LimitOf(SubdivideOp.Limit op, Op key) =>
        op.Samples
            .TraverseM(sample => EvaluateLimit(op.Space, op.Scheme, sample, op.Policy, key))
            .As()
            .Map(rows => (SubdivisionResult)new SubdivisionResult.LimitField(
                new Arr<Point3d>([.. rows.Select(static row => row.Point)]),
                new Arr<Vector3d>([.. rows.Select(static row => row.Normal)])));

    private static Fin<(Point3d Point, Vector3d Normal)> EvaluateLimit(
        MeshSpace space, SubdivisionScheme scheme, FaceSample sample, SubdividePolicy policy, Op key);
```
**To**
```csharp
    private static Fin<SubdivisionResult> LimitOf(SubdivideOp.Limit op, Op key) =>
        Admit(op, key)
            .Bind(input => input.Op.Samples.TraverseM(sample => EvaluateLimit(input, sample, key)).As())
            .Map(rows => (SubdivisionResult)new SubdivisionResult.LimitField(
                rows.Map(static row => row.Point),
                rows.Map(static row => row.Normal)));

    private static Fin<(SubdivideOp.Limit Op, HashMap<(int A, int B), double> Creases, HashMap<int, double> Corners)> Admit(
        SubdivideOp.Limit op, Op key);
    private static Fin<(Point3d Point, Vector3d Normal)> EvaluateLimit(
        (SubdivideOp.Limit Op, HashMap<(int A, int B), double> Creases, HashMap<int, double> Corners) input,
        FaceSample sample, Op key);
```
**Why**
The current path evaluates before proving sample face bounds, scheme/face compatibility, triangular coordinates, or mesh-relative sharpness. Its terminal LINQ projections also enumerate immutable rows into arrays only to reconstruct `Arr<T>` values that `Map` already returns.
**Change**
Reuse the sharpness admission from refinement, then accumulate every sample's face-bound and face-arity errors against `MeshSpace.Native`. For triangular faces require `U.Value + V.Value <= 1`; quadrilateral samples need only their independently admitted unit coordinates. Traverse evaluation only after that gate, obtain bases through the private `(scheme, valence)` cache, consume complete factor-solve verdicts, and project the resulting `Arr` directly.
**Delta**
LOC +2; types 0; members +1.
