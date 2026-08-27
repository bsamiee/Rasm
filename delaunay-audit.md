# 1. Publish tetrahedron topology instead of the mutable arena

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:719-743`, anchored by `public Fin<...> Triangles` and the following `public Fin<DualGraph> VoronoiDual`.

**From:**

```csharp
return Fin.Succ((toArr(corners), toArr(faces)));
}

public Fin<DualGraph> VoronoiDual(Op? key = null) {
```

**To:**

```csharp
return Fin.Succ((toArr(corners), toArr(faces)));
}

public Fin<(Arr<Point3d> Corners, Arr<(int A, int B, int C, int D)> Cells)> Tetrahedra() {
    if (Kind != TessellationKind.Tetrahedralization) {
        return new GeometryFault.UnsupportedTessellationProjection(
            Kind, typeof(ValueTuple<Arr<Point3d>, Arr<(int, int, int, int)>>));
    }
    Dictionary<int, int> slot = new();
    List<Point3d> corners = [];
    List<(int A, int B, int C, int D)> cells = new(Store.LiveCount);
    int[] at = new int[4];
    foreach (int simplex in Store.Live) {
        ReadOnlySpan<int> vertices = Store.SimplexVertices(simplex);
        for (int i = 0; i < 4; i++) {
            if (!slot.TryGetValue(vertices[i], out at[i])) {
                at[i] = slot[vertices[i]] = corners.Count;
                corners.Add(Store.Row(vertices[i]).AsExplicit);
            }
        }
        cells.Add((at[0], at[1], at[2], at[3]));
    }
    return (toArr(corners), toArr(cells));
}

public Fin<DualGraph> VoronoiDual(Op? key = null) {
```

**Why:** `SimplexStore` is a mutable build arena, not a consumer contract. The only external reader wants live tetrahedron topology and coordinates, so this compact frozen projection is the prerequisite for removing its access to liveness, adjacency, mutation, and vertex-carrier internals in move 2. Three-dimensional admission already rejects implicit rows, making `AsExplicit` total here; the slot map preserves shared vertex identity while dropping unused rows. This is the typed counterpart to `Triangles()` and avoids adding a record or rank-erased simplex carrier.

**Ripples:** In `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md:395-410`, bind `Tetrahedra()` after `Build`, change `Kept` to accept its compact corners and cells, size from `projection.Cells.Count`, iterate the cell tuples instead of `tessellation.Store.Live`, and build node coordinates from `projection.Corners` rather than the pre-build seed sequence. Update that page's boundary prose to remove direct `SimplexStore` composition.

# 2. Collapse the union that mirrors `TessellationKind`

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:232-264`, anchored by `[Union(ConversionFromValue = ConversionOperatorsGeneration.None)] public abstract partial class Tessellation`, and `:556-582`, anchored by `static Tessellation Seeded`.

**From:**

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial class Tessellation : IValidityEvidence {
    private Tessellation() { }

    public sealed class Triangulation(SimplexStore store, int superBase, Axis plane, TessellationPolicy policy,
        Option<(Point3d P, Point3d Q, Point3d R)> support) : Tessellation {
        public SimplexStore Store { get; } = store;
        public int SuperBase { get; } = superBase;
        public Axis Plane { get; } = plane;
        public TessellationPolicy Policy { get; } = policy;
        public Option<(Point3d P, Point3d Q, Point3d R)> Support { get; } = support;
    }

    public sealed class Tetrahedralization(SimplexStore store, int superBase, TessellationPolicy policy) : Tessellation {
        public SimplexStore Store { get; } = store;
        public int SuperBase { get; } = superBase;
        public TessellationPolicy Policy { get; } = policy;
    }

    public TessellationKind Kind =>
        Switch(triangulation: static _ => TessellationKind.Triangulation, tetrahedralization: static _ => TessellationKind.Tetrahedralization);
    public SimplexStore Store =>
        Switch(triangulation: static t => t.Store, tetrahedralization: static t => t.Store);
    int SuperBase => Switch(triangulation: static t => t.SuperBase, tetrahedralization: static t => t.SuperBase);
    Axis Projection => Switch(triangulation: static t => t.Plane, tetrahedralization: static _ => Axis.Z);
    TessellationPolicy Policy => Switch(triangulation: static t => t.Policy, tetrahedralization: static t => t.Policy);
```

```csharp
return p.Kind == TessellationKind.Triangulation
    ? new Triangulation(store, superBase, p.Plane, p.Policy, p.Support)
    : new Tetrahedralization(store, superBase, p.Policy);
```

**To:**

```csharp
public sealed partial class Tessellation : IValidityEvidence {
    private Tessellation(SimplexStore store, int superBase, TessellationKind kind, Axis projection,
        TessellationPolicy policy, Option<(Point3d P, Point3d Q, Point3d R)> support) =>
        (Store, SuperBase, Kind, Projection, Policy, Support) =
        (store, superBase, kind, projection, policy, support);

    SimplexStore Store { get; }
    int SuperBase { get; }
    public TessellationKind Kind { get; }
    Axis Projection { get; }
    TessellationPolicy Policy { get; }
    Option<(Point3d P, Point3d Q, Point3d R)> Support { get; }
```

```csharp
return new Tessellation(store, superBase, p.Kind,
    p.Kind == TessellationKind.Triangulation ? p.Plane : Axis.Z,
    p.Policy,
    p.Kind == TessellationKind.Triangulation ? p.Support : None);

// Tessellation.Triangulation DELETED
// Tessellation.Tetrahedralization DELETED
// Tessellation generated Switch/Map surface DELETED
```

**Why:** The union cases repeat the already-required `TessellationKind` roster and share the same arena, super-simplex boundary, policy, mutation lifecycle, and consumers. Their only asymmetric values already have canonical total forms: tetrahedralization projects on `Axis.Z` and carries no planar support. One private constructor normalizes those values once, makes the illegal tetrahedral-support state unreachable, deletes two public case types and five generated projection bodies, and retains the stronger smart-enum owner that admission and faults already consume. After move 1 replaces the sole foreign store read with `Tetrahedra()`, the arena property is private as well. No consumer pattern-matches the case classes; every consumer reads `Kind` or a frozen projection.

# 3. Close the arena type and delete its unused arity projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:82-104`, anchored by `public sealed class SimplexStore`.

**From:**

```csharp
public sealed class SimplexStore {

public int Arity => arity;
```

**To:**

```csharp
internal sealed class SimplexStore {

// Arity DELETED
```

**Why:** After moves 1-2 remove the only foreign arena read, `SimplexStore` is an implementation type and no longer earns public reach. Its `Arity` property has zero reads anywhere; arena methods already use the private `arity` field. Tightening the type and deleting that projection removes public surface without relocating eighty lines or manufacturing another owner.

# 4. Seat the depth-one carrier inside tessellation

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:191-197`, anchored by `[Union(ConversionFromValue = ConversionOperatorsGeneration.None)] public abstract partial record Carrier`, and `:232-234`, anchored by the `Tessellation` union declaration.

**From:**

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Carrier {
    private Carrier() { }

    public sealed record PairCase(Point3d A, Point3d B) : Carrier;
    public sealed record PlaneCase(Point3d P, Point3d Q, Point3d R) : Carrier;
}
```

**To:**

```csharp
public sealed partial class Tessellation : IValidityEvidence {
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    private abstract partial record Carrier {
        private Carrier() { }

        public sealed record PairCase(Point3d A, Point3d B) : Carrier;
        public sealed record PlaneCase(Point3d P, Point3d Q, Point3d R) : Carrier;
    }

// namespace-level Carrier DELETED
```

**Why:** `Carrier` is a genuine normalization: recovery resolves the three `Conform` cases into the two original-entity shapes the exact `Implicit` constructors accept, so flattening it into tuples and presence flags would reintroduce invalid product states. It is nevertheless consumed only by `Tessellation.SteinerOf`, `CarrierOf`, and `ExplicitPair`. Nesting it privately deletes one public module-level type and its public case surface while preserving the closed Thinktecture case family and its structural identity. The case declarations remain `public` inside their private owner, matching the generator's regular-union shape; the containing type still makes their effective reach private.

# 5. Put incidence reach on the store that owns the graph

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:152-163`, anchored by `internal int LastLive()`, and `:215-230`, anchored by `internal static class SimplexWalk`; consumers at `:381-384` and `:592-595`.

**From:**

```csharp
internal static class SimplexWalk {
    internal static Seq<int> Reach(SimplexStore store, int arity, int seed, Func<int, bool> admits) {
        DelegateIncidenceGraph<int, SEquatableEdge<int>> incidence = new(
            (int s, out IEnumerable<SEquatableEdge<int>> outs) => {
                outs = Enumerable.Range(0, arity)
                    .Select(f => store.Neighbour(s, f))
                    .Where(n => n >= 0 && store.Alive(n) && admits(n))
                    .Select(n => new SEquatableEdge<int>(s, n));
                return true;
            });
        ImplicitDepthFirstSearchAlgorithm<int, SEquatableEdge<int>> walk = new(incidence);
        VertexPredecessorRecorderObserver<int, SEquatableEdge<int>> reached = new();
        using (reached.Attach(walk)) { walk.Compute(seed); }
        return Seq(seed) + toSeq(reached.VerticesPredecessors.Keys);
    }
}

IndexSet cavity = [.. SimplexWalk.Reach(Store, Kind.SimplexArity, seed,
    s => Restorable(s) && InCircum(s, in q) == Sign.Positive)];

return seed < 0 ? [] : SimplexWalk.Reach(Store, Kind.SimplexArity, seed, s => Store.SimplexVertices(s).Contains(vertex));
```

**To:**

```csharp
internal Seq<int> Reach(int seed, Func<int, bool> admits) {
    DelegateIncidenceGraph<int, SEquatableEdge<int>> incidence = new(
        (int s, out IEnumerable<SEquatableEdge<int>> outs) => {
            outs = Enumerable.Range(0, arity)
                .Select(f => Neighbour(s, f))
                .Where(n => n >= 0 && Alive(n) && admits(n))
                .Select(n => new SEquatableEdge<int>(s, n));
            return true;
        });
    ImplicitDepthFirstSearchAlgorithm<int, SEquatableEdge<int>> walk = new(incidence);
    VertexPredecessorRecorderObserver<int, SEquatableEdge<int>> reached = new();
    using (reached.Attach(walk)) { walk.Compute(seed); }
    return Seq(seed) + toSeq(reached.VerticesPredecessors.Keys);
}

IndexSet cavity = [.. Store.Reach(seed,
    s => Restorable(s) && InCircum(s, in q) == Sign.Positive)];

return seed < 0 ? [] : Store.Reach(seed, s => Store.SimplexVertices(s).Contains(vertex));

// SimplexWalk DELETED
```

**Why:** `SimplexStore` already owns the neighbour columns, liveness test, and arity that define this incidence graph. Moving the one QuikGraph walk there deletes a file-level type and the redundant `store`/`arity` arguments while preserving one traversal implementation and both call-site predicates.

# 6. Internalize Morton ordering and delete its public utility owner

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:270-273`, anchored by `// --- [BUILD]`, and `:1105-1130`, anchored by `public static class Delaunay`.

**From:**

```csharp
Delaunay.InsertionOrder(admitted.Vertices)

public static class Delaunay {
    public static Seq<int> InsertionOrder(Arr<Implicit> rows) {
        Option<Point3d>[] sites = [.. rows.AsIterable().Map(static r => r.Round())];
        BoundingBox box = new(sites.Somes());
        Vector3d span = box.Max - box.Min;
        uint[] codes = Array.ConvertAll(sites, at => at.Match(
            Some: p => Morton(Normalize(p.X, box.Min.X, span.X), Normalize(p.Y, box.Min.Y, span.Y), Normalize(p.Z, box.Min.Z, span.Z)),
            None: static () => 0u));
        return toSeq(Enumerable.Range(0, sites.Length)
            .OrderBy(i => sites[i].IsNone).ThenBy(i => codes[i]).ThenBy(static i => i));
    }

    static uint Morton(uint x, uint y, uint z) => Expand10(x) | (Expand10(y) << 1) | (Expand10(z) << 2);

    static uint Expand10(uint v) {
        v &= 0x3FF;
        v = (v | (v << 16)) & 0x030000FF;
        v = (v | (v << 8)) & 0x0300F00F;
        v = (v | (v << 4)) & 0x030C30C3;
        v = (v | (v << 2)) & 0x09249249;
        return v;
    }

    static uint Normalize(double value, double min, double span) =>
        span <= EpsilonPolicy.ZeroTolerance ? 0u : (uint)Math.Clamp((int)(1023.0 * (value - min) / span), 0, 1023);
}
```

**To:**

```csharp
InsertionOrder(admitted.Vertices)

static Seq<int> InsertionOrder(Arr<Implicit> rows) {
    Option<Point3d>[] sites = [.. rows.AsIterable().Map(static r => r.Round())];
    BoundingBox box = new(sites.Somes());
    Vector3d span = box.Max - box.Min;
    uint[] codes = Array.ConvertAll(sites, at => at.Match(
        Some: p => Morton(Normalize(p.X, box.Min.X, span.X), Normalize(p.Y, box.Min.Y, span.Y), Normalize(p.Z, box.Min.Z, span.Z)),
        None: static () => 0u));
    return toSeq(Enumerable.Range(0, sites.Length)
        .OrderBy(i => sites[i].IsNone).ThenBy(i => codes[i]).ThenBy(static i => i));

    static uint Morton(uint x, uint y, uint z) => Expand10(x) | (Expand10(y) << 1) | (Expand10(z) << 2);
    static uint Expand10(uint v) {
        v &= 0x3FF;
        v = (v | (v << 16)) & 0x030000FF;
        v = (v | (v << 8)) & 0x0300F00F;
        v = (v | (v << 4)) & 0x030C30C3;
        v = (v | (v << 2)) & 0x09249249;
        return v;
    }
    static uint Normalize(double value, double min, double span) =>
        span <= EpsilonPolicy.ZeroTolerance ? 0u : (uint)Math.Clamp((int)(1023.0 * (value - min) / span), 0, 1023);
}

// Delaunay DELETED
```

**Why:** Morton ordering is not an independent public capability: the only consumer is `Tessellation.Build`. Housing the ordering beside that consumer deletes one public file-level type, removes its public method, and turns the three bit-manipulation helpers into local implementation details without changing the stable ordering or its explicit-before-implicit rule.

# 7. Express build and recovery as monadic folds

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:269-282`, anchored by `// --- [BUILD]`.

**From:**

```csharp
public static Fin<Tessellation> Build(TessellationOp op, Op? key = null) =>
    op.Switch(
        points: static p => Admit(p).Bind(static admitted => Delaunay.InsertionOrder(admitted.Vertices).Fold(
                Fin.Succ(Seeded(admitted)), (acc, v) => acc.Bind(t => t.InsertRow(v).Map(_ => t)))
            .Bind(filled => admitted.Conforms.Map(static (c, i) => (Index: i, Row: c))
                .Fold(Fin.Succ(filled), (acc, c) => acc.Bind(t => t.RecoverOne(c.Row, c.Index))))
            .Bind(static done => done.Policy.Mode.Settle(done))
            .Bind(static done => done.Stripped())),
        insert: static i => i.Into.AdmitRow(i.Vertex)
            .Bind(row => i.Into.InsertRow(i.Into.Store.AddVertex(in row)))
            .Map(_ => i.Into),
        recover: static r => r.Conforms.Map(static (c, i) => (Index: i, Row: c))
            .Fold(r.Into.AdmitIds(r.Conforms), (acc, c) => acc.Bind(t => t.RecoverOne(c.Row, c.Index))));
```

**To:**

```csharp
public static Fin<Tessellation> Build(TessellationOp op, Op? key = null) =>
    op.Switch(
        points: static p => Admit(p).Bind(static admitted => InsertionOrder(admitted.Vertices)
            .FoldM(Seeded(admitted), (t, v) => t.InsertRow(v).Map(_ => t)).As()
            .Bind(filled => admitted.Conforms.Map(static (c, i) => (Index: i, Row: c))
                .FoldM(filled, (t, c) => t.RecoverOne(c.Row, c.Index)).As())
            .Bind(static done => done.Policy.Mode.Settle(done))
            .Bind(static done => done.Stripped())),
        insert: static i => i.Into.AdmitRow(i.Vertex)
            .Bind(row => i.Into.InsertRow(i.Into.Store.AddVertex(in row)))
            .Map(_ => i.Into),
        recover: static r => r.Into.AdmitIds(r.Conforms)
            .Bind(admitted => r.Conforms.Map(static (c, i) => (Index: i, Row: c))
                .FoldM(admitted, (t, c) => t.RecoverOne(c.Row, c.Index)).As()));
```

**Why:** These are state transitions whose step already returns `Fin<Tessellation>`. LanguageExt `FoldM` is the exact carrier operation; it removes three hand-rolled `Fin` accumulators, two synthetic `Fin.Succ` seeds, and the repeated `acc.Bind` plumbing while retaining short-circuit failure and left-to-right mutation order. `.As()` is the required re-anchor from `K<Fin, Tessellation>`.

# 8. Fold facet edges directly at the exhaustive dispatch

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:475-479`, anchored by `Fin<Tessellation> RecoverOne(Conform conform, int index)`, and `:902-906`, anchored by `Fin<Tessellation> RecoverFacet(Conform.Facet facet, int index)`.

**From:**

```csharp
facet:    f => RecoverFacet(f, index),

Fin<Tessellation> RecoverFacet(Conform.Facet facet, int index) =>
    Range(0, facet.Boundary.Count).ToSeq().Fold(
        Fin.Succ(this),
        (acc, i) => acc.Bind(t => t.RecoverOne(new Conform.Edge(facet.Boundary[i], facet.Boundary[(i + 1) % facet.Boundary.Count]), index)))
    .Bind(t => t.FacetConform(facet, index));
```

**To:**

```csharp
facet: f => Range(0, f.Boundary.Count)
    .FoldM(this, (t, i) => t.RecoverOne(
        new Conform.Edge(f.Boundary[i], f.Boundary[(i + 1) % f.Boundary.Count]), index)).As()
    .Bind(t => t.FacetConform(f, index)),

// RecoverFacet DELETED
```

**Why:** The facet boundary is another fallible state fold, not a pure fold over a `Fin` accumulator. `FoldM` removes the artificial success seed, one bind layer, and the unnecessary `ToSeq()` conversion. Seating the fold in the generated total `Conform.Switch` arm also deletes `RecoverFacet`, a one-call forwarding member whose only remaining work was this fold and the terminal `FacetConform` bind.

# 9. Remove operation keys that never enter tessellation behavior

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:270`, anchored by `public static Fin<Tessellation> Build`; `:719`, anchored by `public Fin<...> Triangles`; `:742`, anchored by `public Fin<DualGraph> VoronoiDual`; and `:777-784`, anchored by the bounded `VoronoiDual` overload.

**From:**

```csharp
public static Fin<Tessellation> Build(TessellationOp op, Op? key = null) =>

public Fin<(Arr<Point3d> Corners, Arr<(int A, int B, int C)> Faces)> Triangles(Op? key = null) {

public Fin<DualGraph> VoronoiDual(Op? key = null) {

from cells in ClipCells(boundary: boundary, key: op)

Fin<Arr<BoundedCell>> ClipCells(Polyline boundary, Op key) {
```

**To:**

```csharp
public static Fin<Tessellation> Build(TessellationOp op) =>

public Fin<(Arr<Point3d> Corners, Arr<(int A, int B, int C)> Faces)> Triangles() {

public Fin<DualGraph> VoronoiDual() {

from cells in ClipCells(boundary)

Fin<Arr<BoundedCell>> ClipCells(Polyline boundary) {
```

**Why:** None of these four parameters is read. Their faults already carry concrete geometry evidence rather than an `Op`, so preserving decorative keys advertises observability that does not exist and forces every consumer to thread dead state. Keep the key on `ToMesh`, where `MeshEdit.ToSpace` consumes it, and on bounded `VoronoiDual`, where `op.InvalidInput()` consumes it.

**Ripples:** Drop the matching arguments in `libs/dotnet/Rasm.Compute/.planning/Solver/discretization.md` (`Build` at lines 392 and 439; `Triangles` at 443), `libs/dotnet/Rasm/.planning/Meshing/arrangement.md` (`Build` at 360 and 442; `Triangles` at 363 and 443), `libs/dotnet/Rasm/.planning/Meshing/offset.md` (`Build` at 595, unbounded `VoronoiDual` at 596, `Triangles` at 597), `libs/dotnet/Rasm/.planning/Processing/repair.md` (`Build` at 433, `Triangles` at 435), and `libs/dotnet/Rasm.Fabrication/.planning/Geometry2D/algebra.md` (`Build` at 733, unbounded `VoronoiDual` at 742). Leave `VoronoiDual(ring, op)` at `algebra.md:741` unchanged.

# 10. Re-enter facet boundaries before carrier operations

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:180-186`, anchored by `internal bool Broken(int rows)`.

**From:**

```csharp
facet:    static (n, f) => f.Boundary.Count < 3 || f.Boundary.Distinct().Count != f.Boundary.Count
                           || f.Boundary.Exists(v => v < 0 || v >= n),
```

**To:**

```csharp
facet:    static (n, f) => f.Boundary.Count < 3 || toSeq(f.Boundary).Distinct().Count != f.Boundary.Count
                           || f.Boundary.Exists(v => v < 0 || v >= n),
```

**Why:** LanguageExt `Arr<A>` has no `Distinct` member. The current expression therefore exits to LINQ `IEnumerable<int>`, where `Count` is a method rather than the property used here. `toSeq` is the substrate’s required re-entry, after which `Seq.Distinct()` remains a carrier and publishes the intended `Count` property.

# 11. Emit outward faces directly in the projection loop

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:700-717`, anchored by `int Emit(int v) => slot[v]`.

**From:**

```csharp
int Emit(int v) => slot[v];
int arity = Kind.SimplexArity;
foreach (int s in Store.Live) {
    ReadOnlySpan<int> vs = Store.SimplexVertices(s);
    if (arity == 3) { edit.AddFace(Emit(vs[0]), Emit(vs[1]), Emit(vs[2])); continue; }
    for (int f = 0; f < 4; f++) {
        if (Store.Neighbour(s, f) < 0) { EmitOutward(edit, Emit, vs, f); }
    }
}

void EmitOutward(MeshEdit edit, Func<int, int> emit, ReadOnlySpan<int> vs, int f) {
    (int a, int b, int c) = (vs[(f + 1) & 3], vs[(f + 2) & 3], vs[(f + 3) & 3]);
    bool flip = Predicate.Orient3D(Store.Row(a).AsExplicit, Store.Row(b).AsExplicit, Store.Row(c).AsExplicit, Store.Row(vs[f]).AsExplicit) == Sign.Positive;
    if (flip) { edit.AddFace(emit(a), emit(c), emit(b)); }
    else { edit.AddFace(emit(a), emit(b), emit(c)); }
}
```

**To:**

```csharp
int arity = Kind.SimplexArity;
foreach (int s in Store.Live) {
    ReadOnlySpan<int> vs = Store.SimplexVertices(s);
    if (arity == 3) { edit.AddFace(slot[vs[0]], slot[vs[1]], slot[vs[2]]); continue; }
    for (int f = 0; f < 4; f++) {
        if (Store.Neighbour(s, f) >= 0) { continue; }
        (int a, int b, int c) = (vs[(f + 1) & 3], vs[(f + 2) & 3], vs[(f + 3) & 3]);
        bool flip = Predicate.Orient3D(Store.Row(a).AsExplicit, Store.Row(b).AsExplicit, Store.Row(c).AsExplicit, Store.Row(vs[f]).AsExplicit) == Sign.Positive;
        edit.AddFace(slot[a], slot[flip ? c : b], slot[flip ? b : c]);
    }
}

// Emit DELETED
// EmitOutward DELETED
```

**Why:** `Emit` only indexes `slot`, and `EmitOutward` has one call site inside the loop whose vertices and face ordinal it immediately consumes. Direct indexing plus conditional winding deletes a local delegate, a private class member, two branches, and repeated call plumbing while leaving the exact `Orient3D` decision adjacent to emitted boundary faces.

# 12. Localize the row-admission failure mint

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:315-327`, anchored by `Fin<Implicit> AdmitRow(Implicit vertex)`.

**From:**

```csharp
Fin<Implicit> AdmitRow(Implicit vertex) =>
    vertex.IsExplicit && !ValidityClaim.Finite(point: vertex.AsExplicit)
        ? RejectRow("non-finite explicit row")
        : vertex.IsExplicit && Enumerable.Range(0, Store.VertexCount).Any(v => Store.Row(v).IsExplicit && Rank(Store.Row(v), vertex) == 0)
            ? RejectRow("duplicate row")
            : !vertex.IsExplicit && Policy.Mode == TessellationMode.Delaunay
                ? RejectRow("implicit rows demand constrained mode")
                : !vertex.IsExplicit && Kind == TessellationKind.Tetrahedralization
                    ? RejectRow("3D implicit rows are CDTet-gated growth")
                    : Fin.Succ(vertex);

Fin<Implicit> RejectRow(string witness) =>
    Fin.Fail<Implicit>(new GeometryFault.DegenerateInput(Rasm.Domain.Kind.Point, Store.VertexCount, witness));
```

**To:**

```csharp
Fin<Implicit> AdmitRow(Implicit vertex) {
    Fin<Implicit> Reject(string witness) =>
        new GeometryFault.DegenerateInput(Rasm.Domain.Kind.Point, Store.VertexCount, witness);
    return vertex.IsExplicit && !ValidityClaim.Finite(point: vertex.AsExplicit) ? Reject("non-finite explicit row")
        : vertex.IsExplicit && Enumerable.Range(0, Store.VertexCount).Any(v => Store.Row(v).IsExplicit && Rank(Store.Row(v), vertex) == 0) ? Reject("duplicate row")
        : !vertex.IsExplicit && Policy.Mode == TessellationMode.Delaunay ? Reject("implicit rows demand constrained mode")
        : !vertex.IsExplicit && Kind == TessellationKind.Tetrahedralization ? Reject("3D implicit rows are CDTet-gated growth")
        : vertex;
}

// RejectRow DELETED
```

**Why:** `RejectRow` is called only from `AdmitRow` and carries no policy beyond that method's current insertion ordinal. Localizing it removes one class-level member and keeps the failure mint beside all four row-admission branches. Keep `Conform.Detached`: two generated `Switch` arms share it, so inlining or localizing it would duplicate the complete bounds predicate or force the otherwise static exhaustive arms to capture a local function.

# 13. Inline the directed exit test into its only walk

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:349-378`, anchored by `// --- [LOCATE]` and `int ExitFace(int simplex, in Implicit query)`.

**From:**

```csharp
Fin<int> Locate(int query) {
    int current = Store.LastLive();
    Implicit q = Store.Row(query);
    for (int step = 0; step <= Store.SimplexCount; step++) {
        int exit = ExitFace(current, in q);
        if (exit < 0) { return Fin.Succ(current); }
        int next = Store.Neighbour(current, exit);
        if (next < 0) { return Fin.Fail<int>(new GeometryFault.WalkExitedHull(current)); }
        current = next;
    }
    return Fin.Fail<int>(new GeometryFault.WalkLimitReached(current, Store.SimplexCount));
}

int ExitFace(int simplex, in Implicit query) {
    ReadOnlySpan<int> vs = Store.SimplexVertices(simplex);
    int arity = Kind.SimplexArity;
    for (int f = 0; f < arity; f++) {
        Sign side;
        if (arity == 3) {
            side = Predicate.Orient2D(Store.Row(vs[(f + 1) % 3]), Store.Row(vs[(f + 2) % 3]), query, Projection);
        }
        else {
            (Point3d u1, Point3d u2, Point3d u3) = (Store.Row(vs[(f + 1) & 3]).AsExplicit, Store.Row(vs[(f + 2) & 3]).AsExplicit, Store.Row(vs[(f + 3) & 3]).AsExplicit);
            side = Predicate.Orient3D(u1, u2, u3, query.AsExplicit).Times(Predicate.Orient3D(u1, u2, u3, Store.Row(vs[f]).AsExplicit));
        }
        if (side == Sign.Negative) { return f; }
    }
    return -1;
}
```

**To:**

```csharp
Fin<int> Locate(int query) {
    int current = Store.LastLive();
    Implicit q = Store.Row(query);
    int arity = Kind.SimplexArity;
    for (int step = 0; step <= Store.SimplexCount; step++) {
        ReadOnlySpan<int> vertices = Store.SimplexVertices(current);
        int exit = -1;
        for (int face = 0; face < arity; face++) {
            Sign side;
            if (arity == 3) {
                side = Predicate.Orient2D(Store.Row(vertices[(face + 1) % 3]), Store.Row(vertices[(face + 2) % 3]), q, Projection);
            }
            else {
                (Point3d a, Point3d b, Point3d c) = (Store.Row(vertices[(face + 1) & 3]).AsExplicit,
                    Store.Row(vertices[(face + 2) & 3]).AsExplicit, Store.Row(vertices[(face + 3) & 3]).AsExplicit);
                side = Predicate.Orient3D(a, b, c, q.AsExplicit)
                    .Times(Predicate.Orient3D(a, b, c, Store.Row(vertices[face]).AsExplicit));
            }
            if (side == Sign.Negative) { exit = face; break; }
        }
        if (exit < 0) { return current; }
        int next = Store.Neighbour(current, exit);
        if (next < 0) { return new GeometryFault.WalkExitedHull(current); }
        current = next;
    }
    return new GeometryFault.WalkLimitReached(current, Store.SimplexCount);
}

// ExitFace DELETED
```

**Why:** `ExitFace` has one caller and no meaning outside the directed straight-line walk. Folding its exact face predicate into `Locate` removes a private member and the repeated simplex/query call boundary while retaining the one-path, no-frontier algorithm and its typed hull and budget failures. The arity read is hoisted once for the full walk, and the explicit method return type lets LanguageExt lift the bare index and faults without `Fin.Succ`/`Fin.Fail` shells.

# 14. Inline the one-use support projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:528-538`, anchored by `Fin<Implicit> SteinerOf`, and `:552-553`, anchored by `Option<(Point3d P, Point3d Q, Point3d R)> SupportWitness()`.

**From:**

```csharp
(Carrier.PlaneCase op, Carrier.PlaneCase tp) when SupportWitness().Case is ((Point3d sp, Point3d sq, Point3d sr)) =>
    Fin.Succ<Implicit>(new Implicit.ThreePlaneIntersection(sp, sq, sr, op.P, op.Q, op.R, tp.P, tp.Q, tp.R)),

Option<(Point3d P, Point3d Q, Point3d R)> SupportWitness() =>
    Switch(triangulation: static t => t.Support, tetrahedralization: static _ => None);
```

**To:**

```csharp
(Carrier.PlaneCase op, Carrier.PlaneCase tp) when Support.Case is ((Point3d sp, Point3d sq, Point3d sr)) =>
    Fin.Succ<Implicit>(new Implicit.ThreePlaneIntersection(sp, sq, sr, op.P, op.Q, op.R, tp.P, tp.Q, tp.R)),

// SupportWitness DELETED
```

**Why:** Move 2 normalizes planar support onto the sealed tessellation once and makes tetrahedral support structurally absent. Reading that private `Option` at the only three-plane arm deletes the one-call forwarding member and avoids introducing a nested generated dispatch inside the carrier product match.

# 15. Remove unused string keys from the two non-wire vocabularies

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:43-67`, anchored by `public sealed partial class TessellationKind` and `public sealed partial class TessellationMode`.

**From:**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TessellationKind {
    public static readonly TessellationKind Triangulation      = new("triangulation", simplexArity: 3);
    public static readonly TessellationKind Tetrahedralization = new("tetrahedralization", simplexArity: 4);

    public int SimplexArity { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TessellationMode {
    public static readonly TessellationMode Delaunay = new("delaunay",
        insert: static (into, at, vertex) => into.Restorable(at) ? into.CavityInsert(at, vertex) : into.SplitInsert(at, vertex),
        settle: static into => into.RestoreEmptyCircum());
    public static readonly TessellationMode Constrained = new("constrained",
        insert: static (into, at, vertex) => into.SplitInsert(at, vertex),
        settle: static into => Fin.Succ(into));
```

**To:**

```csharp
[SmartEnum]
public sealed partial class TessellationKind {
    public static readonly TessellationKind Triangulation      = new(simplexArity: 3);
    public static readonly TessellationKind Tetrahedralization = new(simplexArity: 4);

    public int SimplexArity { get; }
}

[SmartEnum]
public sealed partial class TessellationMode {
    public static readonly TessellationMode Delaunay = new(
        insert: static (into, at, vertex) => into.Restorable(at) ? into.CavityInsert(at, vertex) : into.SplitInsert(at, vertex),
        settle: static into => into.RestoreEmptyCircum());
    public static readonly TessellationMode Constrained = new(
        insert: static (into, at, vertex) => into.SplitInsert(at, vertex),
        settle: static into => Fin.Succ(into));

// TessellationKind string keys and comparer attributes DELETED
// TessellationMode string keys and comparer attributes DELETED
```

**Why:** Neither vocabulary crosses a text boundary, performs keyed admission, serializes its key, nor has a consumer that reads `Key`; all decisions use row identity and owned behavior columns. Keyless Thinktecture smart enums retain the closed item roster, equality, `Items`, and generated dispatch while deleting six string/comparer declarations and four duplicated semantic literals. The arity and insert/settle columns remain the actual programmatic meaning.

# 16. Nest tessellation-only projection shapes under their owner

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:199-201`, anchored by `public sealed record DualGraph` and `public sealed record BoundedCell`, and `:232`, anchored by the `Tessellation` declaration.

**From:**

```csharp
public sealed record DualGraph(Arr<Point3d> Circumcenters, Arr<double> Radius, Arr<(int A, int B)> Edges, Arr<(int U, int V)> Across);

public sealed record BoundedCell(int Site, Arr<Point3d> Ring);
```

**To:**

```csharp
public sealed partial class Tessellation : IValidityEvidence {
    public sealed record DualGraph(
        Arr<Point3d> Circumcenters, Arr<double> Radius,
        Arr<(int A, int B)> Edges, Arr<(int U, int V)> Across);

    public sealed record BoundedCell(int Site, Arr<Point3d> Ring);

// namespace-level DualGraph DELETED
// namespace-level BoundedCell DELETED
```

**Why:** Both records are output shapes of `Tessellation.VoronoiDual`, have no independent admission or operation, and are never named by a code consumer outside the target. Nesting preserves their typed public results and inferred consumer use while deleting two namespace-level symbols and making their ownership explicit without a wrapper or rename hop.

**Ripples:** Qualify the prose-only `BoundedCell.Ring` reference in `libs/dotnet/.api/api-clipper2.md:310` as `Tessellation.BoundedCell.Ring`. No C# fence outside the target spells either type.

# 17. Sequence the bounded-cell guard with the monadic operator

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:777-783`, anchored by `public Fin<Arr<BoundedCell>> VoronoiDual(Polyline boundary, Op? key = null)`; apply after move 9 removes the dead `ClipCells` key.

**From:**

```csharp
Op op = key.OrDefault();
if (Kind != TessellationKind.Triangulation) { return Fin.Fail<Arr<BoundedCell>>(new GeometryFault.UnsupportedTessellationProjection(Kind, typeof(Arr<BoundedCell>))); }
return from _ in guard(boundary.IsClosed && boundary.Count >= 4 && boundary.All(static p => p.IsValid), op.InvalidInput()).ToFin()
       from cells in ClipCells(boundary)
       select cells;
```

**To:**

```csharp
Op op = key.OrDefault();
if (Kind != TessellationKind.Triangulation) {
    return new GeometryFault.UnsupportedTessellationProjection(Kind, typeof(Arr<BoundedCell>));
}
return guard(boundary.IsClosed && boundary.Count >= 4 && boundary.All(static p => p.IsValid), op.InvalidInput()).ToFin()
    >> (_ => ClipCells(boundary));
```

**Why:** The guard's `Unit` is intentionally discarded and the cell computation must run only after admission. LanguageExt `>>` is the exact deferred Kleisli sequence for that shape; the lambda preserves short-circuiting, unlike an eagerly evaluated right-hand carrier, and deletes a two-clause query that merely reconstructs the second value. The explicit `Fin<Arr<BoundedCell>>` method target also lifts the typed fault without a `Fin.Fail` shell.

# 18. Localize the bounded-cell kernels to their only projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:784-853`, anchored by `Fin<Arr<BoundedCell>> ClipCells`, `int[] Adjacent`, `Halfplane Bisector`, and `static Point3d[] Wound`.

**From:**

```csharp
int[] Adjacent(int site) {
    IndexSet ring = [];
    foreach (int s in Star(site)) {
        foreach (int w in Store.SimplexVertices(s)) {
            if (w != site && !(w >= SuperBase && w < SuperBase + Kind.SimplexArity)) { ring.Add(w); }
        }
    }
    int[] ordered = [.. ring];
    Array.Sort(ordered);
    return ordered;
}

Halfplane Bisector(Point3d at, Point3d other) {
    (Axis u, Axis v) = (Projection.U, Projection.V);
    (double mu, double mv) = ((u.Read(at) + u.Read(other)) * 0.5, (v.Read(at) + v.Read(other)) * 0.5);
    (double pu, double pv) = (v.Read(at) - v.Read(other), u.Read(other) - u.Read(at));
    return new Halfplane.Frame(Planar(mu, mv), Planar(mu + pu, mv + pv), Projection);

    Point3d Planar(double du, double dv) {
        Span<double> row = [0.0, 0.0, 0.0];
        (row[u.Key], row[v.Key]) = (du, dv);
        return new Point3d(row[0], row[1], row[2]);
    }
}

static Point3d[] Wound(Polyline boundary, Axis u, Axis v) {
    Point3d[] open = [.. boundary.Take(boundary.Count - 1)];
    double twice = 0.0;
    for (int i = 0; i < open.Length; i++) {
        (Point3d a, Point3d b) = (open[i], open[(i + 1) % open.Length]);
        twice += (u.Read(a) * v.Read(b)) - (u.Read(b) * v.Read(a));
    }
    if (twice < 0.0) { Array.Reverse(open); }
    return open;
}
```

**To:**

```csharp
return Fin.Succ(toArr(cells));

int[] Adjacent(int site) {
    IndexSet ring = [];
    foreach (int s in Star(site)) {
        foreach (int w in Store.SimplexVertices(s)) {
            if (w != site && !(w >= SuperBase && w < SuperBase + Kind.SimplexArity)) { ring.Add(w); }
        }
    }
    int[] ordered = [.. ring];
    Array.Sort(ordered);
    return ordered;
}

Halfplane Bisector(Point3d at, Point3d other) {
    (Axis u, Axis v) = (Projection.U, Projection.V);
    (double mu, double mv) = ((u.Read(at) + u.Read(other)) * 0.5, (v.Read(at) + v.Read(other)) * 0.5);
    (double pu, double pv) = (v.Read(at) - v.Read(other), u.Read(other) - u.Read(at));
    return new Halfplane.Frame(Planar(mu, mv), Planar(mu + pu, mv + pv), Projection);

    Point3d Planar(double du, double dv) {
        Span<double> row = [0.0, 0.0, 0.0];
        (row[u.Key], row[v.Key]) = (du, dv);
        return new Point3d(row[0], row[1], row[2]);
    }
}

static Point3d[] Wound(Polyline boundary, Axis u, Axis v) {
    Point3d[] open = [.. boundary.Take(boundary.Count - 1)];
    double twice = 0.0;
    for (int i = 0; i < open.Length; i++) {
        (Point3d a, Point3d b) = (open[i], open[(i + 1) % open.Length]);
        twice += (u.Read(a) * v.Read(b)) - (u.Read(b) * v.Read(a));
    }
    if (twice < 0.0) { Array.Reverse(open); }
    return open;
}
}

// class-level Adjacent DELETED
// class-level Bisector DELETED
// class-level Wound DELETED
```

**Why:** These three kernels exist only to build one bounded Voronoi projection and share that method's store, projection axes, and clipping lifetime. Keeping their nontrivial bodies as local functions preserves the named algebra and avoids inlining noise, while deleting three class-level members and preventing unrelated tessellation operations from acquiring a false dependency on cell-clipping internals.

# 19. Localize circumcircle construction to the dual projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:742-775`, anchored by `public Fin<DualGraph> VoronoiDual`, and `:1090-1102`, anchored by `static (Point3d Center, double Radius) Circumcircle`.

**From:**

```csharp
(centers[i], radius[i]) = Circumcircle(r0.AsExplicit, r1.AsExplicit, r2.AsExplicit, Projection);

static (Point3d Center, double Radius) Circumcircle(Point3d a, Point3d b, Point3d c, Axis plane) {
    (Axis u, Axis v) = (plane.U, plane.V);
    (double ax, double ay) = (u.Read(a), v.Read(a));
    (double bx, double by) = (u.Read(b) - ax, v.Read(b) - ay);
    (double cx, double cy) = (u.Read(c) - ax, v.Read(c) - ay);
    double d = 2.0 * ((bx * cy) - (by * cx));
    (double px, double py) = (((cy * ((bx * bx) + (by * by))) - (by * ((cx * cx) + (cy * cy)))) / d,
                              ((bx * ((cx * cx) + (cy * cy))) - (cx * ((bx * bx) + (by * by)))) / d);
    Span<double> at = [0.0, 0.0, 0.0];
    (at[u.Key], at[v.Key], at[plane.Key]) = (ax + px, ay + py, (plane.Read(a) + plane.Read(b) + plane.Read(c)) / 3.0);
    Point3d center = new(at[0], at[1], at[2]);
    return (center, center.DistanceTo(a));
}
```

**To:**

```csharp
(centers[i], radius[i]) = Circumcircle(r0.AsExplicit, r1.AsExplicit, r2.AsExplicit, Projection);

return Fin.Succ(new DualGraph(toArr(centers), toArr(radius), toArr(edges), toArr(across)));

static (Point3d Center, double Radius) Circumcircle(Point3d a, Point3d b, Point3d c, Axis plane) {
    (Axis u, Axis v) = (plane.U, plane.V);
    (double ax, double ay) = (u.Read(a), v.Read(a));
    (double bx, double by) = (u.Read(b) - ax, v.Read(b) - ay);
    (double cx, double cy) = (u.Read(c) - ax, v.Read(c) - ay);
    double d = 2.0 * ((bx * cy) - (by * cx));
    (double px, double py) = (((cy * ((bx * bx) + (by * by))) - (by * ((cx * cx) + (cy * cy)))) / d,
                              ((bx * ((cx * cx) + (cy * cy))) - (cx * ((bx * bx) + (by * by)))) / d);
    Span<double> at = [0.0, 0.0, 0.0];
    (at[u.Key], at[v.Key], at[plane.Key]) = (ax + px, ay + py, (plane.Read(a) + plane.Read(b) + plane.Read(c)) / 3.0);
    Point3d center = new(at[0], at[1], at[2]);
    return (center, center.DistanceTo(a));
}
}

// class-level Circumcircle DELETED
```

**Why:** Circumcircle construction has one caller and no meaning outside the explicit-triangle dual projection. Local placement retains the numeric derivation as a named kernel while removing another class-level member and binding its lifetime to the collinearity and finite-center gates that make its division lawful.

# 20. Remove the duplicate cavity-seed insertion

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:381-385`, anchored by `internal Fin<int> CavityInsert(int seed, int query)`.

**From:**

```csharp
IndexSet cavity = [.. SimplexWalk.Reach(Store, Kind.SimplexArity, seed,
    s => Restorable(s) && InCircum(s, in q) == Sign.Positive)];
cavity.Add(seed);
```

**To:**

```csharp
IndexSet cavity = [.. SimplexWalk.Reach(Store, Kind.SimplexArity, seed,
    s => Restorable(s) && InCircum(s, in q) == Sign.Positive)];

// cavity.Add(seed) DELETED
```

**Why:** `SimplexWalk.Reach` constructs its result as `Seq(seed) + reached`, so the `HashSet<int>` initializer already contains `seed` even when the supplied cavity predicate rejects it. The second insertion can never change the set or the cone boundary; deleting it removes a misleading suggestion that traversal may omit its root. Apply this after move 5 with `Store.Reach` as the call spelling.

# 21. Report the budget the corridor actually exhausted

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:481-502`, anchored by `Fin<Tessellation> RecoverEnds(Conform root, int index)` and the first `guard-- <= 0` branch.

**From:**

```csharp
int guard = Policy.CorridorGuard(Store.SimplexCount);
while (!EdgePresent(a, b)) {
    if (guard-- <= 0) {
        return Fin.Fail<Tessellation>(new GeometryFault.ConstraintUnrecoverable(index, Policy.MaxRecoverySteiner));
    }
```

**To:**

```csharp
int guard = Policy.CorridorGuard(Store.SimplexCount);
while (!EdgePresent(a, b)) {
    if (guard-- <= 0) {
        return Fin.Fail<Tessellation>(new GeometryFault.ConstraintUnrecoverable(index, Policy.MaxFlipPasses));
    }
```

**Why:** `CorridorGuard` derives this loop ceiling from `MaxFlipPasses × max(simplexCount, 1)`. `MaxRecoverySteiner` governs the separate Steiner insertion counter tested later in the same method. Reporting the Steiner budget when the flip corridor expires misidentifies the exhausted policy axis and makes a caller tune the wrong limit; the 3D corridor already reports `MaxFlipPasses` for the identical reason.

# 22. Delete the fabricated facet endpoints

**Location:** `libs/dotnet/Rasm/.planning/Meshing/delaunay.md:174-178`, anchored by `public (int A, int B) Ends`; `:475-485`, anchored by `Fin<Tessellation> RecoverOne` and `Fin<Tessellation> RecoverEnds`; and `:909-911`, anchored by `Fin<Tessellation> RecoverEdge3D`.

**From:**

```csharp
public (int A, int B) Ends =>
    Switch(
        edge:     static e => (e.A, e.B),
        facet:    static f => (f.Boundary[0], f.Boundary[^1]),
        crossing: static c => (c.A, c.B));

Fin<Tessellation> RecoverOne(Conform conform, int index) =>
    conform.Switch(
        edge:     e => RecoverEnds(e, index),
        facet:    f => Range(0, f.Boundary.Count)
            .FoldM(this, (t, i) => t.RecoverOne(
                new Conform.Edge(f.Boundary[i], f.Boundary[(i + 1) % f.Boundary.Count]), index)).As()
            .Bind(t => t.FacetConform(f, index)),
        crossing: c => RecoverEnds(c, index));

Fin<Tessellation> RecoverEnds(Conform root, int index) {
    if (Kind == TessellationKind.Tetrahedralization) { return RecoverEdge3D(root, index); }
    Queue<(int A, int B, Conform Root)> queue = new();
    (int rootA, int rootB) = root.Ends;

Fin<Tessellation> RecoverEdge3D(Conform edge, int index) {
    (int a, int b) = edge.Ends;
```

**To:**

```csharp
Fin<Tessellation> RecoverOne(Conform conform, int index) =>
    conform.Switch(
        edge:     e => RecoverEnds(e.A, e.B, e, index),
        facet:    f => Range(0, f.Boundary.Count)
            .FoldM(this, (t, i) => t.RecoverOne(
                new Conform.Edge(f.Boundary[i], f.Boundary[(i + 1) % f.Boundary.Count]), index)).As()
            .Bind(t => t.FacetConform(f, index)),
        crossing: c => RecoverEnds(c.A, c.B, c, index));

Fin<Tessellation> RecoverEnds(int rootA, int rootB, Conform root, int index) {
    if (Kind == TessellationKind.Tetrahedralization) { return RecoverEdge3D(rootA, rootB, root, index); }
    Queue<(int A, int B, Conform Root)> queue = new();

Fin<Tessellation> RecoverEdge3D(int a, int b, Conform edge, int index) {

// Conform.Ends DELETED
```

**Why:** A facet is a closed boundary, so its first and last vertices are merely one boundary edge, not semantic endpoints of the facet. The property exists only because the two genuine two-ended cases were widened back to `Conform` before recovery. The generated exhaustive dispatch already has the precise `Edge` and `Crossing` payloads; passing their endpoints into the shared kernel deletes one public member and one invalid facet arm while retaining the root case as the exact Steiner carrier. Apply this after move 8 so its folded facet arm is the one shown here.
