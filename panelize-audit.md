# 1. Require an explicit material symmetry policy
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L54-L62**
```csharp
public sealed record PanelPolicy(
    Tolerance Planarity, Dimension Rounds, RemeshPolicy Remesh, PullbackPolicy Pullback,
    Option<MaterialSymmetry> Law = default) : IValidityEvidence {
    public static PanelPolicy Of(Context context) => new(
        Planarity: context.For(lane: ToleranceLane.Fraction), Rounds: Dimension.Create(value: 32),
        Remesh: RemeshPolicy.Canonical, Pullback: PullbackPolicy.Of(context: context));

    public bool IsValid => ValidityClaim.All(Planarity.IsValid, Remesh.IsValid, Pullback.IsValid);
}
```
**To**
```csharp
public sealed record PanelPolicy(
    Tolerance Planarity, Dimension Rounds, RemeshPolicy Remesh, PullbackPolicy Pullback,
    MaterialSymmetry Symmetry) : IValidityEvidence {
    public static PanelPolicy Of(Context context) => new(
        Planarity: context.For(lane: ToleranceLane.Fraction), Rounds: Dimension.Create(value: 32),
        Remesh: RemeshPolicy.Canonical, Pullback: PullbackPolicy.Of(context: context),
        Symmetry: MaterialSymmetry.Free);

    public bool IsValid => ValidityClaim.All(Planarity.IsValid, Remesh.IsValid, Pullback.IsValid);
}
```
**Why**
`default(Option<T>)` silently changes fabrication equivalence to `Free`; symmetry is a real policy choice, not optional state. `Symmetry` is the established domain term and removes the unexplained `Law` alias.
**Change**
Make the generated smart-enum value a required policy member and place the existing `Free` default only in the canonical factory. Read the member directly during classification.
**Ripples**
In this sheet, replace `policy.Law.IfNone(MaterialSymmetry.Free)` at lines 268 and 479 with `policy.Symmetry`. In `libs/dotnet/Rasm.Fabrication/.planning/Materials/component.md`, rename the `PanelPolicy.Law` reference at line 27 to `PanelPolicy.Symmetry`. Repository search finds no other `PanelPolicy` construction to migrate.
**Delta**
LOC +1; types 0; members 0; optional-state branches -2.

# 2. Remove the unread component column
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L65-L69**
```csharp
public sealed record PanelField(
    Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv,
    Arr<Point3d> Origin, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis, Arr<double> Planarity,
    Arr<int> PatchOf, Arr<int> AdjacencyOffsets, Arr<int> Adjacent, Arr<int> Component, Arr<int> ShapeClass, Arr<bool> Flipped,
    Context Model);
```
**To**
```csharp
public sealed record PanelField(
    Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv,
    Arr<Point3d> Origin, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis, Arr<double> Planarity,
    Arr<int> PatchOf, Arr<int> AdjacencyOffsets, Arr<int> Adjacent, Arr<int> ShapeClass, Arr<bool> Flipped,
    Context Model);
```
**Why**
No repository consumer reads `Component`, while the adjacency CSR already contains exactly the connectivity needed to derive components when a future consumer actually needs them. Storing both duplicates topology and forces an otherwise unnecessary graph algorithm.
**Change**
Delete the column and its construction. Preserve the public structure-of-arrays wire used by nesting; do not replace the frame columns with an `Arr<Plane>` wrapper.
**Delta**
LOC 0; types 0; members -1.

# 3. Collapse the operation union and forwarding pipeline
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L74-L123**
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelOp {
    private PanelOp() { }

    public sealed record Map(SurfaceResult.UvTessellation Source, PanelFamily Family, PanelPolicy Policy) : PanelOp;
    public sealed record Planarize(PanelResult Prior, PanelPolicy Policy) : PanelOp;
}

public static class Panelization {
    public static Fin<PanelResult> Apply(PanelOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            map: static (k, m) => !m.Policy.IsValid
                ? Fin.Fail<PanelResult>(k.InvalidInput())
                : m.Family.Switch(
                    state: (m.Source, m.Policy, Key: k),
                    lattice: static (s, f) => LatticePanels(s.Source, f, s.Policy, s.Key),
                    seeded:  static (s, f) => SeededPanels(s.Source, f, s.Policy, s.Key)),
            planarize: static (k, p) => PlanarizeOf(p.Prior, p.Policy, k));

    // --- [LATTICE]
    static Fin<PanelResult> LatticePanels(SurfaceResult.UvTessellation source, PanelFamily.Lattice family, PanelPolicy policy, Op key) =>
        Remeshing.Apply(new RemeshOp(source.Mesh, family.TargetLength, policy.Remesh, Some(family.Order)), key)
            .Bind(remesh => remesh.Quads.Match(
                Some: quads => Reprovenance(source, remesh.Mesh, policy, key)
                    .Bind(uv => Assemble(source, LatticeBuild(remesh.Mesh, quads, uv), fieldOrder: None, policy, key)),
                None: () => Fin.Fail<PanelResult>(key.InvalidResult())));

    static Fin<Arr<Point2d>> Reprovenance(SurfaceResult.UvTessellation source, MeshSpace emitted, PanelPolicy policy, Op key) =>
        Surfaces.Apply(new SurfaceOp.Pullback(source.Source, toArray(emitted.Native.Vertices.ToPoint3dArray()), policy.Pullback), key)
            .Bind(result => result is SurfaceResult.Pulled pulled
                ? Fin.Succ(pulled.Uv)
                : Fin.Fail<Arr<Point2d>>(key.InvalidResult()));

    static PanelBuild LatticeBuild(MeshSpace emitted, QuadLayout quads, Arr<Point2d> uv) {
        int panels = quads.Corners.Count / 4;
        return new PanelBuild(
            CornerOffsets: toArray(Enumerable.Range(0, panels + 1).Select(static p => 4 * p)),
            Corners: quads.Corners,
            Vertices: toArray(emitted.Native.Vertices.ToPoint3dArray()),
            Uv: uv,
            PatchOf: quads.PatchOf);
    }

    // --- [SEEDED]
    static Fin<PanelResult> SeededPanels(SurfaceResult.UvTessellation source, PanelFamily.Seeded family, PanelPolicy policy, Op key) =>
        ExtractionDomain.Mesh(source.Mesh, key)
            .Bind(domain => SampleKernel.Sample(family.Seeds, domain, source.Mesh.Tolerance, key))
            .Bind(seeds => SeededCells(source, seeds.Points, key))
            .Bind(build => Assemble(source, build, fieldOrder: Some(family.Order), policy, key));
```
**To**
```csharp
public static class Panelization {
    public static Fin<PanelResult> Apply(
        SurfaceResult.UvTessellation source, PanelFamily family, PanelPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        if (!policy.IsValid) { return Fin.Fail<PanelResult>(op.InvalidInput()); }
        return family.Switch(
                state: (Source: source, Policy: policy, Key: op),
                lattice: static (s, f) =>
                    Remeshing.Apply(new RemeshOp(s.Source.Mesh, f.TargetLength, s.Policy.Remesh, Some(f.Order)), s.Key)
                        .Bind(remesh => remesh.Quads.Match(
                            Some: quads => {
                                Point3d[] vertices = remesh.Mesh.Native.Vertices.ToPoint3dArray();
                                int panels = quads.Corners.Count / 4;
                                return Surfaces.Apply(
                                        new SurfaceOp.Pullback(s.Source.Source, toArray(vertices), s.Policy.Pullback), s.Key)
                                    .Bind(result => result is SurfaceResult.Pulled pulled
                                        ? Assemble(s.Source, new PanelBuild(
                                            toArray(Enumerable.Range(0, panels + 1).Select(static p => 4 * p)),
                                            quads.Corners, toArray(vertices), pulled.Uv, quads.PatchOf),
                                            fieldOrder: None, s.Policy, s.Key)
                                        : Fin.Fail<PanelResult>(s.Key.InvalidResult()));
                            },
                            None: () => Fin.Fail<PanelResult>(s.Key.InvalidResult()))),
                seeded: static (s, f) =>
                    ExtractionDomain.Mesh(s.Source.Mesh, s.Key)
                        .Bind(domain => SampleKernel.Sample(f.Seeds, domain, s.Source.Mesh.Tolerance, s.Key))
                        .Bind(seeds => SeededCells(s.Source, seeds.Points, s.Key))
                        .Bind(build => Assemble(s.Source, build, fieldOrder: Some(f.Order), s.Policy, s.Key)))
            .Bind(prior => PlanarizeOf(prior, policy, op));
    }
```
**Why**
`PanelOp.Map` and `PanelOp.Planarize` expose a two-stage protocol whose intermediate `PanelResult` may violate the policy's advertised planarity tolerance. The four private helpers each forward once and obscure the only real choice, `PanelFamily`. The generated `PanelFamily.Switch` already supplies exhaustive dispatch.
**Change**
Admit policy once, build either family directly, then bind the raw result into the planarity gate before it can escape. Inline remesh reprovenance and fixed-width quad packing at their sole call site; inline seeded orchestration at its sole call site. Keep `PanelFamily` because its variants carry genuinely different inputs and algorithms.
**Ripples**
Update this sheet's prose at lines 14-16 and diagram labels at lines 537-538 from `PanelOp.Map`/`PanelOp.Planarize` to the single `Panelization.Apply` contract. Repository search finds no code-fence consumer of `PanelOp`; callers introduced later must pass `(source, family, policy, key)` and must not request a second planarization operation.
**Delta**
LOC -14; types -3; members -4.

# 4. Build the complete geodesic lower envelope
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L125-L200**
```csharp
    static Fin<PanelBuild> SeededCells(SurfaceResult.UvTessellation source, Seq<Point3d> seeds, Op key) {
        Point3d[] vertices = source.Mesh.Native.Vertices.ToPoint3dArray();
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(vertices)), key: key)
            .Bind(index => key.AcceptValidated<Dimension>(candidate: 1).Bind(one => NeighborKernel.GraphOf(
                index: index, needles: [.. seeds], count: Some(one), radius: Option<PositiveMagnitude>.None, key: key)))
            .Bind(graph => toSeq(graph.Ids).TraverseM(hits => hits.Length > 0
                ? Fin.Succ(hits[0])
                : Fin.Fail<int>(key.InvalidResult())).As())
            .Bind(snapped => snapped.TraverseM(vertex => GeodesicKernel.EnsureGeodesicDistances(source.Mesh, Seq(vertex), key)).As())
            .Bind(fields => Cells(source, vertices, fields, key));
    }

    internal readonly record struct Bary(int A, int B, int C, double W0, double W1, double W2) {
        internal double Of(Arr<double> field) => (W0 * field[A]) + (W1 * field[B]) + (W2 * field[C]);
        internal Point3d World(Point3d[] rows) => new(
            (W0 * rows[A].X) + (W1 * rows[B].X) + (W2 * rows[C].X),
            (W0 * rows[A].Y) + (W1 * rows[B].Y) + (W2 * rows[C].Y),
            (W0 * rows[A].Z) + (W1 * rows[B].Z) + (W2 * rows[C].Z));
        internal Point2d Uv(Arr<Point2d> rows) => new(
            (W0 * rows[A].X) + (W1 * rows[B].X) + (W2 * rows[C].X),
            (W0 * rows[A].Y) + (W1 * rows[B].Y) + (W2 * rows[C].Y));
    }

    static Fin<PanelBuild> Cells(SurfaceResult.UvTessellation source, Point3d[] vertices, Seq<Arr<double>> fields, Op key) {
        Mesh native = source.Mesh.Native;
        int[] label = [.. Enumerable.Range(0, vertices.Length).Select(v => Argmin(fields, v))];
        Dictionary<int, List<Bary[]>> byCell = new();
        for (int f = 0; f < native.Faces.Count; f++) {
            MeshFace face = native.Faces[f];
            Seq<(int A, int B, int C)> triangles = face.IsQuad
                ? Seq((face.A, face.B, face.C), (face.A, face.C, face.D))
                : Seq((face.A, face.B, face.C));
            foreach ((int a, int b, int c) in triangles) {
                int[] present = [.. Seq(label[a], label[b], label[c]).Distinct()];
                foreach (int cell in present) {
                    Bary[] fragment = Clip(fields, a, b, c, cell, present);
                    if (fragment.Length < 3) { continue; }
                    if (!byCell.TryGetValue(cell, out List<Bary[]>? rows)) { byCell[cell] = rows = []; }
                    rows.Add(fragment);
                }
            }
        }
        return toSeq(byCell.OrderBy(static row => row.Key))
            .TraverseM(row => Loop(row.Value, vertices, source.Mesh.Tolerance, key).Map(ring => (Cell: row.Key, Ring: ring))).As()
            .Map(rings => Pack(rings, vertices, source.Uv));
    }

    static int Argmin(Seq<Arr<double>> fields, int vertex) {
        (int at, double best) = (0, double.PositiveInfinity);
        for (int c = 0; c < fields.Count; c++) {
            if (fields[c][vertex] < best) { (at, best) = (c, fields[c][vertex]); }
        }
        return at;
    }

    static Bary[] Clip(Seq<Arr<double>> fields, int a, int b, int c, int cell, int[] present) {
        List<Bary> ring = [new(a, b, c, 1.0, 0.0, 0.0), new(a, b, c, 0.0, 1.0, 0.0), new(a, b, c, 0.0, 0.0, 1.0)];
        foreach (int rival in present) {
            if (rival == cell || ring.Count < 3) { continue; }
            List<Bary> kept = new(ring.Count + 2);
            for (int i = 0; i < ring.Count; i++) {
                (Bary u, Bary v) = (ring[i], ring[(i + 1) % ring.Count]);
                (double du, double dv) = (Delta(fields, cell, rival, u), Delta(fields, cell, rival, v));
                if (du <= 0.0) { kept.Add(u); }
                if (du * dv < 0.0) { kept.Add(Lerp(u, v, du / (du - dv))); }
            }
            ring = kept;
        }
        return [.. ring];
    }

    static double Delta(Seq<Arr<double>> fields, int cell, int rival, Bary at) => at.Of(fields[cell]) - at.Of(fields[rival]);

    static Bary Lerp(Bary from, Bary to, double t) => new(
        from.A, from.B, from.C,
        from.W0 + ((to.W0 - from.W0) * t), from.W1 + ((to.W1 - from.W1) * t), from.W2 + ((to.W2 - from.W2) * t));
```
**To**
```csharp
    static Fin<PanelBuild> SeededCells(SurfaceResult.UvTessellation source, Seq<Point3d> seeds, Op key) {
        Point3d[] vertices = source.Mesh.Native.Vertices.ToPoint3dArray();
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(vertices)), key: key)
            .Bind(index => key.AcceptValidated<Dimension>(candidate: 1).Bind(one => NeighborKernel.GraphOf(
                index: index, needles: [.. seeds], count: Some(one), radius: Option<PositiveMagnitude>.None, key: key)))
            .Bind(graph => toSeq(graph.Ids).TraverseM(hits => hits.Length > 0
                ? Fin.Succ(hits[0])
                : Fin.Fail<int>(key.InvalidResult())).As())
            .Bind(snapped => {
                Seq<int> sources = snapped.Distinct();
                return sources.IsEmpty
                    ? Fin.Fail<Seq<Arr<double>>>(key.InvalidResult())
                    : sources.TraverseM(vertex =>
                        GeodesicKernel.EnsureGeodesicDistances(source.Mesh, Seq(vertex), key)).As();
            })
            .Bind(fields => Cells(source, vertices, fields, key));
    }

    internal readonly record struct BaryPoint(int A, int B, int C, double W0, double W1, double W2) {
        internal double Of(Arr<double> field) => (W0 * field[A]) + (W1 * field[B]) + (W2 * field[C]);
        internal Point3d World(Point3d[] rows) => new(
            (W0 * rows[A].X) + (W1 * rows[B].X) + (W2 * rows[C].X),
            (W0 * rows[A].Y) + (W1 * rows[B].Y) + (W2 * rows[C].Y),
            (W0 * rows[A].Z) + (W1 * rows[B].Z) + (W2 * rows[C].Z));
        internal Point2d Uv(Arr<Point2d> rows) => new(
            (W0 * rows[A].X) + (W1 * rows[B].X) + (W2 * rows[C].X),
            (W0 * rows[A].Y) + (W1 * rows[B].Y) + (W2 * rows[C].Y));
    }

    static Fin<PanelBuild> Cells(SurfaceResult.UvTessellation source, Point3d[] vertices, Seq<Arr<double>> fields, Op key) {
        Dictionary<int, List<BaryPoint[]>> byCell = new();
        Mesh native = source.Mesh.Native;
        for (int f = 0; f < native.Faces.Count; f++) {
            MeshFace face = native.Faces[f];
            Seq<(int A, int B, int C)> triangles = face.IsQuad
                ? Seq((face.A, face.B, face.C), (face.A, face.C, face.D))
                : Seq((face.A, face.B, face.C));
            foreach ((int a, int b, int c) in triangles) {
                for (int cell = 0; cell < fields.Count; cell++) {
                    BaryPoint[] fragment = Clip(fields, a, b, c, cell);
                    if (fragment.Length < 3) { continue; }
                    if (!byCell.TryGetValue(cell, out List<BaryPoint[]>? rows)) { byCell[cell] = rows = []; }
                    rows.Add(fragment);
                }
            }
        }
        return toSeq(byCell.OrderBy(static row => row.Key))
            .TraverseM(row => Loops(row.Value, vertices, source.Mesh.Tolerance, key)
                .Map(rings => rings.Map(ring => (Cell: row.Key, Ring: ring)))).As()
            .Map(groups => Pack(groups.Bind(static group => group), vertices, source.Uv, source.Mesh.Tolerance));
    }

    static BaryPoint[] Clip(Seq<Arr<double>> fields, int a, int b, int c, int cell) {
        List<BaryPoint> ring = [
            new(a, b, c, 1.0, 0.0, 0.0),
            new(a, b, c, 0.0, 1.0, 0.0),
            new(a, b, c, 0.0, 0.0, 1.0)];
        for (int rival = 0; rival < fields.Count && ring.Count >= 3; rival++) {
            if (rival == cell) { continue; }
            List<BaryPoint> kept = new(ring.Count + 2);
            for (int i = 0; i < ring.Count; i++) {
                (BaryPoint u, BaryPoint v) = (ring[i], ring[(i + 1) % ring.Count]);
                double du = u.Of(fields[cell]) - u.Of(fields[rival]);
                double dv = v.Of(fields[cell]) - v.Of(fields[rival]);
                if (du <= 0.0) { kept.Add(u); }
                if (du * dv < 0.0) {
                    double t = du / (du - dv);
                    kept.Add(new BaryPoint(a, b, c,
                        u.W0 + ((v.W0 - u.W0) * t),
                        u.W1 + ((v.W1 - u.W1) * t),
                        u.W2 + ((v.W2 - u.W2) * t)));
                }
            }
            ring = kept;
        }
        return [.. ring];
    }
```
**Why**
Restricting candidates to the three vertex winners is not a valid lower-envelope algorithm: an affine distance field can own an interior polygon without winning at any triangle vertex. Duplicate snapped seeds also repeat full geodesic solves and create indistinguishable cells. `Bary` is an abbreviated coordinate-system name; `BaryPoint` states what the carrier represents.
**Change**
Deduplicate snapped source vertices, reject an empty source set, and clip every triangle against every distinct geodesic field. Inline the two one-call arithmetic helpers into the clipping loop and delete the now-invalid vertex-label/`Argmin` shortcut. Preserve LanguageExt `TraverseM` so the first failed geodesic computation short-circuits the build.
**Delta**
LOC -5; types 0; members -3.

# 5. Preserve every cell loop and intern shared vertices
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L202-L254**
```csharp
    static Fin<Bary[]> Loop(List<Bary[]> fragments, Point3d[] vertices, Context model, Op key) {
        Dictionary<(long, long, long), Bary> seat = new();
        Dictionary<(long, long, long), (long, long, long)> next = new();
        HashSet<((long, long, long) From, (long, long, long) To)> directed = new();
        foreach (Bary[] fragment in fragments) {
            for (int i = 0; i < fragment.Length; i++) {
                (Bary from, Bary to) = (fragment[i], fragment[(i + 1) % fragment.Length]);
                ((long, long, long) lo, (long, long, long) hi) = (Grain(from, vertices, model), Grain(to, vertices, model));
                if (lo == hi) { continue; }
                seat.TryAdd(lo, from);
                seat.TryAdd(hi, to);
                if (!directed.Remove((hi, lo))) { directed.Add((lo, hi)); }
            }
        }
        foreach (((long, long, long) from, (long, long, long) to) in directed) { next[from] = to; }
        if (next.Count < 3) { return Fin.Fail<Bary[]>(key.InvalidResult()); }
        List<Bary> ring = new(next.Count);
        (long, long, long) seed = next.Keys.Min();
        (long, long, long) walk = seed;
        for (int step = 0; step < next.Count; step++) {
            ring.Add(seat[walk]);
            if (!next.TryGetValue(walk, out walk)) { return Fin.Fail<Bary[]>(key.InvalidResult()); }
            if (walk == seed) { return Fin.Succ<Bary[]>([.. ring]); }
        }
        return Fin.Fail<Bary[]>(key.InvalidResult());
    }

    static (long, long, long) Grain(Bary at, Point3d[] vertices, Context model) {
        Point3d seat = at.World(vertices);
        double grain = model.Absolute.Value;
        return ((long)Math.Round(seat.X / grain), (long)Math.Round(seat.Y / grain), (long)Math.Round(seat.Z / grain));
    }

    static PanelBuild Pack(Seq<(int Cell, Bary[] Ring)> rings, Point3d[] vertices, Arr<Point2d> uv) {
        List<int> offsets = [0];
        List<int> corners = new();
        List<int> patchOf = new();
        List<Point3d> seats = new();
        List<Point2d> feet = new();
        foreach ((int cell, Bary[] ring) in rings) {
            foreach (Bary corner in ring) {
                corners.Add(seats.Count);
                seats.Add(corner.World(vertices));
                feet.Add(corner.Uv(uv));
            }
            offsets.Add(corners.Count);
            patchOf.Add(cell);
        }
        return new PanelBuild(toArray(offsets), toArray(corners), toArray(seats), toArray(feet), toArray(patchOf));
    }

    internal readonly record struct PanelBuild(
        Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv, Arr<int> PatchOf);
```
**To**
```csharp
    static Fin<Seq<BaryPoint[]>> Loops(List<BaryPoint[]> fragments, Point3d[] vertices, Context model, Op key) {
        Dictionary<(long, long, long), BaryPoint> seat = new();
        HashSet<((long, long, long) From, (long, long, long) To)> directed = [];
        foreach (BaryPoint[] fragment in fragments) {
            for (int i = 0; i < fragment.Length; i++) {
                (BaryPoint from, BaryPoint to) = (fragment[i], fragment[(i + 1) % fragment.Length]);
                ((long, long, long) lo, (long, long, long) hi) = (Grain(from, vertices, model), Grain(to, vertices, model));
                if (lo == hi) { continue; }
                seat.TryAdd(lo, from);
                seat.TryAdd(hi, to);
                if (!directed.Remove((hi, lo))) { directed.Add((lo, hi)); }
            }
        }

        Dictionary<(long, long, long), (long, long, long)> next = new();
        HashSet<(long, long, long)> incoming = [];
        foreach (((long, long, long) from, (long, long, long) to) in directed) {
            if (!next.TryAdd(from, to) || !incoming.Add(to)) {
                return Fin.Fail<Seq<BaryPoint[]>>(key.InvalidResult());
            }
        }

        List<BaryPoint[]> rings = [];
        while (next.Count > 0) {
            int budget = next.Count;
            (long, long, long) start = next.Keys.Min();
            (long, long, long) walk = start;
            List<BaryPoint> ring = [];
            for (int step = 0; step < budget; step++) {
                ring.Add(seat[walk]);
                if (!next.Remove(walk, out walk)) { return Fin.Fail<Seq<BaryPoint[]>>(key.InvalidResult()); }
                if (walk == start) { break; }
            }
            if (walk != start || ring.Count < 3) { return Fin.Fail<Seq<BaryPoint[]>>(key.InvalidResult()); }
            rings.Add([.. ring]);
        }
        return rings.Count > 0 ? Fin.Succ(toSeq(rings)) : Fin.Fail<Seq<BaryPoint[]>>(key.InvalidResult());
    }

    static (long, long, long) Grain(BaryPoint at, Point3d[] vertices, Context model) {
        Point3d point = at.World(vertices);
        double grain = model.Absolute.Value;
        return ((long)Math.Round(point.X / grain), (long)Math.Round(point.Y / grain), (long)Math.Round(point.Z / grain));
    }

    static PanelBuild Pack(Seq<(int Cell, BaryPoint[] Ring)> rings, Point3d[] vertices, Arr<Point2d> uv, Context model) {
        Dictionary<(long, long, long), int> vertexOf = new();
        List<int> offsets = [0];
        List<int> corners = [];
        List<int> patchOf = [];
        List<Point3d> points = [];
        List<Point2d> feet = [];
        foreach ((int cell, BaryPoint[] ring) in rings) {
            foreach (BaryPoint corner in ring) {
                (long, long, long) grain = Grain(corner, vertices, model);
                if (!vertexOf.TryGetValue(grain, out int vertex)) {
                    vertexOf.Add(grain, vertex = points.Count);
                    points.Add(corner.World(vertices));
                    feet.Add(corner.Uv(uv));
                }
                corners.Add(vertex);
            }
            offsets.Add(corners.Count);
            patchOf.Add(cell);
        }
        return new PanelBuild(toArray(offsets), toArray(corners), toArray(points), toArray(feet), toArray(patchOf));
    }

    internal readonly record struct PanelBuild(
        Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv, Arr<int> PatchOf);
```
**Why**
A geodesic Voronoi cell on a disconnected or multiply connected mesh can have more than one boundary loop; returning the first closed walk silently drops valid panels. `Pack` also allocates a fresh vertex for every panel corner, so two panels never share an index and the wall matcher reports no adjacency. That also prevents the planarization average from coupling shared corners.
**Change**
Cancel internal directed edges, require one incoming and one outgoing boundary edge, consume all closed loops, and fail on branched/open topology. Flatten all loops into panels. Intern boundary points on the same model-tolerance grain used for stitching, storing one world/UV row per shared vertex.
**Delta**
LOC +17; types 0; members 0.

# 6. Assemble directly from CSR adjacency
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L257-L279**
```csharp
    static Fin<PanelResult> Assemble(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RosyOrder> fieldOrder, PanelPolicy policy, Op key) {
        int panels = build.CornerOffsets.Count - 1;
        UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, panels));
        foreach ((int a, int b) in SharedWalls(build)) { graph.AddEdge(new SEdge<int>(a, b)); }
        Dictionary<int, int> componentOf = new();
        graph.ConnectedComponents(componentOf);
        (Arr<int> offsets, Arr<int> adjacent) = AdjacencyColumns(graph, panels);
        return Frames(source, build, fieldOrder, key).Bind(frames => {
            Arr<double> planarity = PlanarityOf(build.CornerOffsets, build.Corners, build.Vertices);
            MaterialSymmetry law = policy.Law.IfNone(MaterialSymmetry.Free);
            (Arr<int> shapeClass, Arr<bool> flipped, int chiralSplit) = ShapeClasses(
                build.CornerOffsets, build.Corners, build.Vertices, frames, source.Mesh.Tolerance, law);
            return Stat<Scalar>.Of(planarity.AsSpan(), key).Map(band => new PanelResult(
                new PanelField(
                    build.CornerOffsets, build.Corners, build.Vertices, build.Uv,
                    frames.Origin, frames.X, frames.Z, planarity, build.PatchOf, offsets, adjacent,
                    new Arr<int>([.. Enumerable.Range(0, panels).Select(p => componentOf[p])]),
                    shapeClass, flipped, source.Mesh.Tolerance),
                band, chiralSplit, Rounds: 0));
        });
    }
```
**To**
```csharp
    static Fin<PanelResult> Assemble(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RosyOrder> fieldOrder, PanelPolicy policy, Op key) {
        (Arr<int> offsets, Arr<int> adjacent) = AdjacencyOf(build);
        return Frames(source, build, fieldOrder, key).Bind(frames => {
            Arr<double> planarity = PlanarityOf(build.CornerOffsets, build.Corners, build.Vertices);
            (Arr<int> shapeClass, Arr<bool> flipped, int chiralSplit) = ShapeClasses(
                build.CornerOffsets, build.Corners, build.Vertices, frames, source.Mesh.Tolerance, policy.Symmetry);
            return Stat<Scalar>.Of(planarity.AsSpan(), key).Map(band => new PanelResult(
                new PanelField(
                    build.CornerOffsets, build.Corners, build.Vertices, build.Uv,
                    frames.Origin, frames.X, frames.Z, planarity, build.PatchOf, offsets, adjacent,
                    shapeClass, flipped, source.Mesh.Tolerance),
                band, chiralSplit, Rounds: 0));
        });
    }
```
**Why**
The module's durable topology is already CSR. Materializing a general graph, then copying it back to CSR, serves only the deleted component column and adds dependency surface without capability.
**Change**
Construct CSR once from the indexed panel build, pass explicit symmetry to classification, and populate only the retained `PanelField` columns.
**Ripples**
In `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md:L1380`, replace the invalid `panels.Result.ChiralSplit` access with `panels.ChiralSplit`; `PanelResult` has no `Result` member. Its `Outline` consumer at lines 1409-1413 continues to read the preserved `Origin`/`XAxis`/`ZAxis` columns unchanged.
**Delta**
LOC -7; types 0; members 0; transient graph objects -1.

# 7. Fuse wall matching and adjacency packing
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L284-L307**
```csharp
    static Seq<(int A, int B)> SharedWalls(PanelBuild build) {
        Dictionary<(int, int), int> byWall = new();
        List<(int A, int B)> pairs = new();
        for (int p = 0; p + 1 < build.CornerOffsets.Count; p++) {
            (int lo, int hi) = (build.CornerOffsets[p], build.CornerOffsets[p + 1]);
            for (int i = lo; i < hi; i++) {
                (int u, int v) = (build.Corners[i], build.Corners[lo + (((i - lo) + 1) % (hi - lo))]);
                (int a, int b) = (int.Min(u, v), int.Max(u, v));
                if (byWall.Remove((a, b), out int other)) { pairs.Add((other, p)); }
                else { byWall[(a, b)] = p; }
            }
        }
        return toSeq(pairs);
    }

    static (Arr<int> Offsets, Arr<int> Adjacent) AdjacencyColumns(UndirectedGraph<int, SEdge<int>> graph, int panels) {
        int[] offsets = new int[panels + 1];
        for (int p = 0; p < panels; p++) { offsets[p + 1] = offsets[p] + graph.AdjacentDegree(p); }
        int[] adjacent = new int[offsets[panels]];
        for (int p = 0; p < panels; p++) {
            int at = offsets[p];
            foreach (SEdge<int> edge in graph.AdjacentEdges(p)) { adjacent[at++] = edge.Source == p ? edge.Target : edge.Source; }
        }
        return (toArray(offsets), toArray(adjacent));
    }
```
**To**
```csharp
    static (Arr<int> Offsets, Arr<int> Adjacent) AdjacencyOf(PanelBuild build) {
        int panels = build.CornerOffsets.Count - 1;
        Dictionary<(int, int), int> byWall = new();
        List<int>[] neighbors = [.. Enumerable.Range(0, panels).Select(static _ => new List<int>())];
        for (int panel = 0; panel < panels; panel++) {
            (int lo, int hi) = (build.CornerOffsets[panel], build.CornerOffsets[panel + 1]);
            for (int i = lo; i < hi; i++) {
                (int u, int v) = (build.Corners[i], build.Corners[lo + (((i - lo) + 1) % (hi - lo))]);
                (int a, int b) = (int.Min(u, v), int.Max(u, v));
                if (byWall.Remove((a, b), out int other)) {
                    neighbors[other].Add(panel);
                    neighbors[panel].Add(other);
                } else {
                    byWall[(a, b)] = panel;
                }
            }
        }

        int[] offsets = new int[panels + 1];
        for (int panel = 0; panel < panels; panel++) {
            offsets[panel + 1] = offsets[panel] + neighbors[panel].Count;
        }
        int[] adjacent = new int[offsets[panels]];
        for (int panel = 0; panel < panels; panel++) {
            neighbors[panel].CopyTo(adjacent, offsets[panel]);
        }
        return (toArray(offsets), toArray(adjacent));
    }
```
**Why**
The two current methods create an edge sequence and a third-party graph only to recover per-panel neighbor lists. Direct packing is the same linear wall-table algorithm with one owner and no forwarding representation.
**Change**
Accumulate both directions when an indexed wall closes, prefix-sum the per-panel counts, and copy the lists into the existing CSR wire. Keep unmatched walls as boundaries.
**Delta**
LOC +1; types 0; members -1; external graph types -2.

# 8. Remove the obsolete graph dependency imports
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L31-L32**
```csharp
using QuikGraph;
using QuikGraph.Algorithms;
```
**To**
```csharp
// QuikGraph imports DELETED
```
**Why**
Direct CSR construction leaves no QuikGraph symbol in the module. Retaining the imports would falsely advertise package ownership of an algorithm now expressed with BCL collections.
**Change**
Delete both imports after `AdjacencyOf` replaces graph construction and connected-component computation.
**Ripples**
Remove the QuikGraph statement from this sheet's dependency prose at line 21. Do not remove QuikGraph from project/package policy based on this module alone; other repository modules still use it.
**Delta**
LOC -2; types 0; symbols -2.

# 9. Delegate least-squares plane fitting to RhinoCommon
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L310-L324**
```csharp
    static Arr<double> PlanarityOf(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices) =>
        toArray(Enumerable.Range(0, offsets.Count - 1).Select(p => Defect(Ring(offsets, corners, vertices, p))));

    static double Defect(ReadOnlySpan<Point3d> ring) {
        Vector3d normal = VectorFrame.NewellNormal(ring);
        double diameter = 0.0;
        for (int i = 0; i < ring.Length; i++) {
            for (int j = i + 1; j < ring.Length; j++) { diameter = double.Max(diameter, ring[i].DistanceTo(ring[j])); }
        }
        if (!normal.Unitize() || diameter <= 0.0) { return double.PositiveInfinity; }
        Point3d seat = Centroid(ring);
        double worst = 0.0;
        foreach (Point3d corner in ring) { worst = double.Max(worst, Math.Abs(normal * (corner - seat))); }
        return worst / diameter;
    }
```
**To**
```csharp
    static Arr<double> PlanarityOf(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices) =>
        toArray(Enumerable.Range(0, offsets.Count - 1).Select(p => Defect(Ring(offsets, corners, vertices, p))));

    static double Defect(Point3d[] ring) {
        double diameter = 0.0;
        for (int i = 0; i < ring.Length; i++) {
            for (int j = i + 1; j < ring.Length; j++) { diameter = double.Max(diameter, ring[i].DistanceTo(ring[j])); }
        }
        return diameter > 0.0
            && Plane.FitPlaneToPoints(ring, out Plane plane, out double maximumDeviation) is PlaneFitResult.Success
            && plane.IsValid && double.IsFinite(maximumDeviation)
                ? maximumDeviation / diameter
                : double.PositiveInfinity;
    }
```
**Why**
Newell's normal is an area-weighted polygon normal, not the least-squares plane required by the sheet's own planarity contract. RhinoCommon already owns robust plane fitting and reports the maximum deviation needed by this normalized defect.
**Change**
Keep the scale-invariant diameter normalization, but obtain both the fitted plane and maximum deviation from `Plane.FitPlaneToPoints`; map degenerate or non-finite results to the existing infinite-defect failure signal.
**Ripples**
Replace the Newell-plane claim in this sheet's prose at lines 543-544 with least-squares RhinoCommon fitting. `VectorFrame.NewellNormal` may remain in its owning module; this task removes only this call.
**Delta**
LOC -3; types 0; members 0; hand-rolled fitting paths -1.

# 10. Compute coherent planes while preserving the columnar wire
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L332-L373**
```csharp
    static Fin<(Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z)> Frames(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RosyOrder> fieldOrder, Op key) =>
        Range(0, build.CornerOffsets.Count - 1).ToSeq()
            .TraverseM(p => FrameOf(source, build, fieldOrder, p, key)).As()
            .Map(static rows => (
                toArray(rows.Map(static row => row.Origin)),
                toArray(rows.Map(static row => row.X)),
                toArray(rows.Map(static row => row.Z))));

    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> FrameOf(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RosyOrder> fieldOrder, int panel, Op key) {
        (int lo, int hi) = (build.CornerOffsets[panel], build.CornerOffsets[panel + 1]);
        Point3d seat = Centroid(Ring(build.CornerOffsets, build.Corners, build.Vertices, panel));
        Point2d foot = UvSeat(build, lo, hi);
        return from normal in source.Source.NormalAt(foot.X, foot.Y)
               from axis in fieldOrder.Match(
                   Some: order => SegmentKernel.CrossFieldAt(source.Mesh, order, None, None, seat, key),
                   None: () => Fin.Succ(StripeU(build, lo, hi)))
               from frame in Orthonormal(seat, normal, axis, key)
               select frame;
    }

    static Point2d UvSeat(PanelBuild build, int lo, int hi) {
        (double u, double v) = (0.0, 0.0);
        for (int i = lo; i < hi; i++) { (u, v) = (u + build.Uv[build.Corners[i]].X, v + build.Uv[build.Corners[i]].Y); }
        return new Point2d(u / (hi - lo), v / (hi - lo));
    }

    static Vector3d StripeU(PanelBuild build, int lo, int hi) =>
        hi - lo == 4
            ? ((build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]])
               + (build.Vertices[build.Corners[lo + 2]] - build.Vertices[build.Corners[lo + 3]])) * 0.5
            : build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]];

    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> Orthonormal(Point3d seat, Vector3d normal, Vector3d axis, Op key) {
        Vector3d z = normal;
        if (!z.Unitize()) { return Fin.Fail<(Point3d, Vector3d, Vector3d)>(key.InvalidResult()); }
        Vector3d x = axis - ((z * axis) * z);
        return x.Unitize()
            ? Fin.Succ((seat, x, z))
            : Fin.Fail<(Point3d, Vector3d, Vector3d)>(key.InvalidResult());
    }
```
**To**
```csharp
    static Fin<(Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z)> Frames(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RosyOrder> fieldOrder, Op key) =>
        Range(0, build.CornerOffsets.Count - 1).ToSeq()
            .TraverseM(panel => {
                (int lo, int hi) = (build.CornerOffsets[panel], build.CornerOffsets[panel + 1]);
                Point3d origin = Centroid(Ring(build.CornerOffsets, build.Corners, build.Vertices, panel));
                (double u, double v) = (0.0, 0.0);
                for (int i = lo; i < hi; i++) {
                    Point2d uv = build.Uv[build.Corners[i]];
                    (u, v) = (u + uv.X, v + uv.Y);
                }
                Point2d foot = new(u / (hi - lo), v / (hi - lo));
                Vector3d stripe = hi - lo == 4
                    ? ((build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]])
                       + (build.Vertices[build.Corners[lo + 2]] - build.Vertices[build.Corners[lo + 3]])) * 0.5
                    : build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]];
                return from normal in source.Source.NormalAt(foot.X, foot.Y)
                       from axis in fieldOrder.Match(
                           Some: order => SegmentKernel.CrossFieldAt(source.Mesh, order, None, None, origin, key),
                           None: () => Fin.Succ(stripe))
                       from frame in Frame(origin, normal, axis, key)
                       select frame;
            }).As()
            .Map(static frames => (
                toArray(frames.Map(static frame => frame.Origin)),
                toArray(frames.Map(static frame => frame.XAxis)),
                toArray(frames.Map(static frame => frame.ZAxis))));

    static Fin<Plane> Frame(Point3d origin, Vector3d normal, Vector3d axis, Op key) {
        Vector3d z = normal;
        if (!z.Unitize()) { return Fin.Fail<Plane>(key.InvalidResult()); }
        Vector3d x = axis - ((z * axis) * z);
        if (!x.Unitize()) { return Fin.Fail<Plane>(key.InvalidResult()); }
        Plane frame = new(origin, x, Vector3d.CrossProduct(z, x));
        return frame.IsValid ? Fin.Succ(frame) : Fin.Fail<Plane>(key.InvalidResult());
    }
```
**Why**
`UvSeat`, `StripeU`, and `FrameOf` are single-call fragments of one frame computation. A RhinoCommon `Plane` is the stronger internal carrier because it guarantees coherent origin and axes, but replacing `PanelField`'s parallel columns would weaken its documented structure-of-arrays contract and break nesting's column reads.
**Change**
Inline panel-local accumulation into the traversal, keep `TraverseM` failure propagation, and use one reusable `Frame` admission for orthonormalization. Decompose the admitted planes only at the existing columnar boundary.
**Delta**
LOC -7; types 0; members -3.

# 11. Make planarization fit, project, and refit one admitted state
**From — libs/dotnet/Rasm/.planning/Parametric/panelize.md:L462-L523**
```csharp
    static Fin<PanelResult> PlanarizeOf(PanelResult prior, PanelPolicy policy, Op key) =>
        Range(0, policy.Rounds.Value).FoldUntil(
                initialState: Fin.Succ((Field: prior.Field, Band: prior.Planarity, Rounds: 0)),
                f: (state, _) => state.Bind(s => ProjectRound(s.Field, key).Map(next => (next.Field, next.Band, s.Rounds + 1))),
                predicate: state => state.Match(
                    Succ: s => s.Band.Maximum.To() <= policy.Planarity.Value,
                    Fail: static _ => true))
            .Bind(final => final.Band.Maximum.To() > policy.Planarity.Value
                ? WorstPanel(final.Field).ToFin(key.InvalidResult()).Bind(panel =>
                    Fin.Fail<PanelResult>(new GeometryFault.PanelPlanarityExceeded(panel, final.Band.Maximum.To(), policy.Planarity)))
                : Fin.Succ(Reclassified(prior, final.Field, final.Band, final.Rounds, policy)));

    static PanelResult Reclassified(PanelResult prior, PanelField field, Stat<Scalar> band, int rounds, PanelPolicy policy) {
        (Arr<int> shapeClass, Arr<bool> flipped, int chiralSplit) = rounds == 0
            ? (field.ShapeClass, field.Flipped, prior.ChiralSplit)
            : ShapeClasses(
                field.CornerOffsets, field.Corners, field.Vertices, (field.Origin, field.XAxis, field.ZAxis),
                field.Model, policy.Law.IfNone(MaterialSymmetry.Free));
        return new PanelResult(
            field with { ShapeClass = shapeClass, Flipped = flipped },
            band, chiralSplit, rounds);
    }

    static Fin<(PanelField Field, Stat<Scalar> Band)> ProjectRound(PanelField field, Op key) {
        Vector3d[] pulled = new Vector3d[field.Vertices.Count];
        int[] hits = new int[field.Vertices.Count];
        int panels = field.CornerOffsets.Count - 1;
        Vector3d[] planeNormal = new Vector3d[panels];
        for (int p = 0; p < panels; p++) {
            Point3d[] ring = Ring(field.CornerOffsets, field.Corners, field.Vertices, p);
            Vector3d normal = VectorFrame.NewellNormal(ring);
            if (!normal.Unitize()) { continue; }
            planeNormal[p] = normal;
            Point3d seat = Centroid(ring);
            for (int i = field.CornerOffsets[p]; i < field.CornerOffsets[p + 1]; i++) {
                int v = field.Corners[i];
                pulled[v] += (Vector3d)field.Vertices[v] - ((normal * (field.Vertices[v] - seat)) * normal);
                hits[v]++;
            }
        }
        Arr<Point3d> moved = toArray(Enumerable.Range(0, field.Vertices.Count)
            .Select(v => hits[v] == 0 ? field.Vertices[v] : new Point3d(pulled[v].X / hits[v], pulled[v].Y / hits[v], pulled[v].Z / hits[v])));
        Arr<double> planarity = PlanarityOf(field.CornerOffsets, field.Corners, moved);
        return Stat<Scalar>.Of(planarity.AsSpan(), key).Map(band => (
            field with {
                Vertices = moved, Planarity = planarity,
                Origin = toArray(Enumerable.Range(0, panels).Select(p => Centroid(Ring(field.CornerOffsets, field.Corners, moved, p)))),
                ZAxis = toArray(Enumerable.Range(0, panels).Select(p => planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p])),
                XAxis = toArray(Enumerable.Range(0, panels).Select(p => Retangent(field.XAxis[p], planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p]))),
            },
            band));
    }

    static Vector3d Retangent(Vector3d prior, Vector3d normal) {
        Vector3d axis = prior - ((normal * prior) * normal);
        return axis.Unitize() ? axis : prior;
    }

    static Option<int> WorstPanel(PanelField field) =>
        field.Planarity.Count == 0
            ? Option<int>.None
            : Some(Enumerable.Range(0, field.Planarity.Count).MaxBy(p => field.Planarity[p]));
```
**To**
```csharp
    static Fin<PanelResult> PlanarizeOf(PanelResult prior, PanelPolicy policy, Op key) {
        if (prior.Planarity.Maximum.To() <= policy.Planarity.Value) { return Fin.Succ(prior); }
        return Range(0, policy.Rounds.Value).FoldUntil(
                initialState: Fin.Succ((Field: prior.Field, Band: prior.Planarity, Rounds: 0)),
                f: (state, _) => state.Bind(s =>
                    ProjectRound(s.Field, key).Map(next => (next.Field, next.Band, s.Rounds + 1))),
                predicate: row => row.State.Match(
                    Succ: s => s.Band.Maximum.To() <= policy.Planarity.Value,
                    Fail: static _ => true))
            .Bind(final => {
                double maximum = final.Band.Maximum.To();
                if (maximum > policy.Planarity.Value) {
                    int panel = Enumerable.Range(0, final.Field.Planarity.Count)
                        .MaxBy(p => final.Field.Planarity[p]);
                    return Fin.Fail<PanelResult>(
                        new GeometryFault.PanelPlanarityExceeded(panel, maximum, policy.Planarity));
                }
                (Arr<int> classes, Arr<bool> flipped, int chiralSplit) = ShapeClasses(
                    final.Field.CornerOffsets, final.Field.Corners, final.Field.Vertices,
                    (final.Field.Origin, final.Field.XAxis, final.Field.ZAxis),
                    final.Field.Model, policy.Symmetry);
                return Fin.Succ(new PanelResult(
                    final.Field with { ShapeClass = classes, Flipped = flipped },
                    final.Band, chiralSplit, final.Rounds));
            });
    }

    static Fin<(PanelField Field, Stat<Scalar> Band)> ProjectRound(PanelField field, Op key) {
        Vector3d[] displacement = new Vector3d[field.Vertices.Count];
        int[] hits = new int[field.Vertices.Count];
        int panels = field.CornerOffsets.Count - 1;
        for (int panel = 0; panel < panels; panel++) {
            Point3d[] ring = Ring(field.CornerOffsets, field.Corners, field.Vertices, panel);
            if (Plane.FitPlaneToPoints(ring, out Plane plane, out _) is not PlaneFitResult.Success || !plane.IsValid) {
                return Fin.Fail<(PanelField, Stat<Scalar>)>(key.InvalidResult());
            }
            for (int i = field.CornerOffsets[panel]; i < field.CornerOffsets[panel + 1]; i++) {
                int vertex = field.Corners[i];
                displacement[vertex] += plane.ClosestPoint(field.Vertices[vertex]) - field.Vertices[vertex];
                hits[vertex]++;
            }
        }

        Arr<Point3d> moved = toArray(Enumerable.Range(0, field.Vertices.Count).Select(vertex =>
            hits[vertex] == 0
                ? field.Vertices[vertex]
                : field.Vertices[vertex] + (displacement[vertex] / hits[vertex])));
        Arr<double> planarity = PlanarityOf(field.CornerOffsets, field.Corners, moved);
        return Range(0, panels).ToSeq().TraverseM(panel => {
                Point3d[] ring = Ring(field.CornerOffsets, field.Corners, moved, panel);
                if (Plane.FitPlaneToPoints(ring, out Plane plane, out _) is not PlaneFitResult.Success || !plane.IsValid) {
                    return Fin.Fail<Plane>(key.InvalidResult());
                }
                Vector3d normal = plane.Normal * field.ZAxis[panel] < 0.0 ? -plane.Normal : plane.Normal;
                return Frame(Centroid(ring), normal, field.XAxis[panel], key);
            }).As()
            .Bind(frames => Stat<Scalar>.Of(planarity.AsSpan(), key).Map(band => (
                field with {
                    Vertices = moved,
                    Planarity = planarity,
                    Origin = toArray(frames.Map(static frame => frame.Origin)),
                    XAxis = toArray(frames.Map(static frame => frame.XAxis)),
                    ZAxis = toArray(frames.Map(static frame => frame.ZAxis)),
                },
                band)));
    }
```
**Why**
The current round sums absolute projected coordinates into a `Vector3d`, then constructs a `Point3d` from that sum; this loses the original-point affine reference and is not displacement averaging. It also measures planarity from one plane model while projecting with another, retains stale origins, and updates tangents against pre-move normals. The existing `FoldUntil` predicate addresses the accumulator wrapper instead of its documented `State` member.
**Change**
Short-circuit an already accepted result. For each failing round, fit the same least-squares plane used by the defect metric, average projection displacements at shared indexed vertices, refit every moved panel, preserve normal orientation, and admit a coherent frame through the shared `Frame` function. Use `row.State` in the LanguageExt fold predicate. Inline final classification and worst-panel selection, deleting `Reclassified`, `Retangent`, and `WorstPanel`; classify exactly once after successful admission.
**Delta**
LOC +2; types 0; members -3; duplicated plane models -1.
