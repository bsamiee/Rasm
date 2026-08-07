# [RASM_PARAMETRIC_PANELIZE]

`Panelization` owns cross-field-guided panelization: `Apply` maps a UV-provenanced surface into a panel graph whose every panel leaves with a placement frame — origin, field-aligned x-axis, and metric-true binding normal — because position without orientation is half a panel. `PanelFamily` rides the request as data, so a new family is one case over the shared assembly fold rather than a sibling mapper, and per-panel planarity is the fabrication acceptance measure whose breach routes a fault instead of shipping an unfabricatable lattice.

Input is `surface.md`'s `SurfaceResult.UvTessellation` — mesh, per-vertex `(u, v)`, and live `NurbsForm.Surface` binding — so an unbound mesh cannot enter and every `PanelField` keeps its UV provenance. `Lattice` consumes the remesh substrate's `QuadProvenance` without re-running any field solve while `Seeded` lands geodesic-Voronoi cells over the `sample.md` distribution suite, `Symmetry` the one n-RoSy integer keying both arms; adjacency folds through a transient QuikGraph and leaves as SoA columns, never a leaked graph type.

## [01]-[INDEX]

- [02]-[PANELIZATION]: `PanelFamily` family-as-data folded by one `Panelization.Apply` into a placement-framed, planarity-gated `PanelField` panel graph.

## [02]-[PANELIZATION]

- Owner: `Panelization` mints the one static entry; `PanelFamily` carries the family as data, `PanelPolicy` the `IValidityEvidence` policy row, `PanelField` the panel-graph-plus-frame SoA wire, `PanelReceipt` the evidence, `PanelResult` the carrier.
- Cases: `PanelFamily` cases `Lattice` and `Seeded` — the substrate-guided lattice and the sample-suite distribution, `Symmetry` the one n-RoSy integer keying both; `PanelOp` cases `Map` and `Planarize` — generation versus fabrication-correction, `Planarize` consuming `Map`'s carrier.
- Entry: `public static Fin<PanelResult> Apply(PanelOp op, Op? key = null)` — the one entry discriminating on the op case, the family arm discriminating inside it.
- Auto: `Map`+`Lattice` binds the substrate's `QuadProvenance` as the panel lattice and restores UV through one batch `Pullback`; `Map`+`Seeded` lands geodesic-Voronoi cells from cached heat-distance labels walled at the equidistance lerp. Both arms assemble identically — Newell plane per panel, planarity defect, adjacency folded through a transient graph into offset columns — and `Planarize` runs bounded proximal rounds toward `PlanarityBudget`, keeping each panel's pre-planarization UV feet.
- Receipt: `PanelReceipt` carries the panel/vertex/component census, max/mean planarity, singular-face count, and planarize rounds — the panelization-gate evidence; the substrate's `RemeshTrace` and the seed suite's `SampleReceipt` stay upstream.
- Packages: `Rasm.Processing` for the remesh substrate (`QuadProvenance`) and the seed suite (`SampleKind`, `SampleKernel`, `SegmentKernel.CrossFieldAt`, `GeodesicKernel`); `Rasm.Parametric` `surface.md` for the `UvTessellation` input and `Pullback` restore and `nurbs.md` for the frame normals; `Rasm.Spatial` `ScalarField` for density seeds; `Rasm.Numerics` `GeometryFault`; `Rasm.Domain` `Op`/`Context`/`IValidityEvidence`; QuikGraph for the transient adjacency fold; Rhino.Geometry, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new panel family is one `PanelFamily` case over the same assembly fold; a new seed distribution is one `SampleKind` row; a new panel measure is one `PanelField` column — `ShapeClass` congruence is the executed precedent; a fabrication-nesting order is one projection off the adjacency columns.
- Boundary: the field solve is the substrate's — a `CrossFieldAt`/`StripeAt` loop here is the named re-derivation defect, the lattice arm consuming `QuadProvenance` whole, its sole local frame read (stripe-U off the quad's own corners) holding only because the emitted geometry is the integrated field. Output keeps provenance — a wire without UV columns is the named drop, restored by one batch `Pullback` never a per-vertex `ClosestParameter` loop; seeded labels are geodesic, a Euclidean nearest-seed the named naivety defect across folds. `Planarize` fits per-panel planes and never parameterizes, a conformal or ARAP energy belonging to `flatten.md`; QuikGraph stays transient with adjacency leaving as offset columns, a stored graph field the named lane violation; every failure routes `DevelopmentFault(Panel, …)` with the panel unit and its planarity or admission witness, composed rails surfacing their own faults untranslated.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Parametric;

// --- [TYPES] ------------------------------------------------------------------------------------
// Family as data; Symmetry is the one n-RoSy integer ({1,2,4,6} at CrossField admission) — the lattice cell axis and the seeded frame alignment.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelFamily {
    private PanelFamily() { }

    public sealed record Lattice(int Symmetry, double TargetLength) : PanelFamily;
    public sealed record Seeded(SampleKind Seeds, int Symmetry) : PanelFamily;
}

// --- [CONSTANTS] --------------------------------------------------------------------------------
// PlanarityBudget is the fabrication acceptance ceiling — max vertex-plane deviation over panel diameter; Remesh/Pullback thread substrate policy.
public sealed record PanelPolicy(
    double PlanarityBudget, int PlanarizeRounds, RemeshPolicy Remesh, PullbackPolicy Pullback) : IValidityEvidence {
    public static readonly PanelPolicy Canonical = new(
        PlanarityBudget: 5e-3, PlanarizeRounds: 32, RemeshPolicy.Canonical, PullbackPolicy.Canonical);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: PlanarityBudget),
        ValidityClaim.Positive(value: PlanarizeRounds),
        ValidityClaim.Evidence(evidence: Remesh),
        ValidityClaim.Evidence(evidence: Pullback));
}

// --- [MODELS] -----------------------------------------------------------------------------------
// Panel-graph SoA wire — graph results as columns, never a leaked graph type; y = ZAxis × XAxis derived at the consumer.
// ShapeClass is the mould-reuse congruence class — panels congruent up to rigid motion within Context.Absolute
// share one ordinal — the actual cost driver of a panelized facade, one column over data the wire already carries.
public sealed record PanelField(
    Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv,
    Arr<Point3d> Origin, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis, Arr<double> Planarity,
    Arr<int> PatchOf, Arr<int> AdjacencyOffsets, Arr<int> Adjacent, Arr<int> Component, Arr<int> ShapeClass);

public sealed record PanelReceipt(
    int Panels, int Vertices, int Components, double MaxPlanarity, double MeanPlanarity, int SingularFaces, int Rounds);

public sealed record PanelResult(PanelField Field, PanelReceipt Receipt);

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelOp {
    private PanelOp() { }

    public sealed record Map(SurfaceResult.UvTessellation Source, PanelFamily Family, PanelPolicy Policy) : PanelOp;
    public sealed record Planarize(PanelResult Prior, PanelPolicy Policy) : PanelOp;
}

public static class Panelization {
    public static Fin<PanelResult> Apply(PanelOp op, Op? key = null) =>
        op.Switch(
            state: key,
            map: static (k, m) => !m.Policy.IsValid
                ? Fault<PanelResult>(unit: 0, witness: m.Policy.PlanarityBudget)
                : m.Family.Switch(
                    state: (m.Source, m.Policy, Key: k),
                    lattice: static (s, f) => LatticePanels(s.Source, f, s.Policy, s.Key),
                    seeded:  static (s, f) => SeededPanels(s.Source, f, s.Policy, s.Key)),
            planarize: static (k, p) => PlanarizeOf(p.Prior, p.Policy, k));

    // --- [LATTICE]
    // Substrate does the field work once: QuadField lands conditioning, two memoized Knöppel solves, integer-isoline quads;
    // this arm binds QuadProvenance and restores UV through one batch Pullback over the rewritten geometry.
    static Fin<PanelResult> LatticePanels(SurfaceResult.UvTessellation source, PanelFamily.Lattice family, PanelPolicy policy, Op? key) =>
        Remeshing.Apply(new RemeshOp.QuadField(source.Mesh, family.TargetLength, family.Symmetry, policy.Remesh), key)
            .Bind(remesh => remesh.Quads.Match(
                Some: quads => Reprovenance(source, remesh.Mesh, policy, key)
                    .Bind(uv => Assemble(source, LatticeBuild(remesh.Mesh, quads, uv), fieldSymmetry: None, policy, key)),
                None: () => Fault<PanelResult>(unit: 0, witness: family.TargetLength)));

    // One Surfaces.Apply(SurfaceOp.Pullback) batch over the kd-tree-seeded engine Newton; a per-vertex
    // ClosestParameter loop is the deleted form. The REWRITTEN geometry's own vertices are the probe set,
    // so provenance restores against the emitted mesh rather than the pre-remesh tessellation it replaced.
    static Fin<Arr<Point2d>> Reprovenance(SurfaceResult.UvTessellation source, MeshSpace emitted, PanelPolicy policy, Op? key) =>
        Surfaces.Apply(new SurfaceOp.Pullback(source.Source, toArr(emitted.Native.Vertices.ToPoint3dArray()), policy.Pullback), key)
            .Bind(result => result is SurfaceResult.Pulled pulled
                ? Fin.Succ(pulled.Uv)
                : Fault<Arr<Point2d>>(unit: 0, witness: emitted.Native.Vertices.Count));

    // Quads → 4-corner offset rows over the emitted vertex columns; PatchOf carries through, SingularFaces
    // into the census. The offsets are the uniform 4-stride the quad channel guarantees, so the lattice arm
    // and the seeded arm hand `Assemble` one ragged-ring shape and it never learns which family built it.
    static PanelBuild LatticeBuild(MeshSpace emitted, QuadProvenance quads, Arr<Point2d> uv) {
        int panels = quads.Corners.Count / 4;
        return new PanelBuild(
            CornerOffsets: toArr(Enumerable.Range(0, panels + 1).Select(static p => 4 * p)),
            Corners: quads.Corners,
            Vertices: toArr(emitted.Native.Vertices.ToPoint3dArray()),
            Uv: uv,
            PatchOf: quads.PatchOf,
            SingularFaces: quads.SingularFaces.Count);
    }

    // --- [SEEDED]
    // Seeds land through the receipt-bearing sample suite; labels are geodesic argmin — k cached heat solves over one pre-factored Laplacian.
    // Walls cross label-boundary edges at the equidistance lerp, one weight interpolating world AND uv — provenance, never re-projection.
    static Fin<PanelResult> SeededPanels(SurfaceResult.UvTessellation source, PanelFamily.Seeded family, PanelPolicy policy, Op? key) =>
        ExtractionDomain.Mesh(source.Mesh, key)
            .Bind(domain => SampleKernel.Sample(family.Seeds, domain, source.Mesh.Tolerance, key.OrDefault()))
            .Bind(seeds => SeededCells(source, seeds.Points, key))
            .Bind(build => Assemble(source, build, fieldSymmetry: Some(family.Symmetry), policy, key));

    // Each seed snaps to its nearest tessellation vertex; per-seed EnsureGeodesicDistances → per-vertex argmin
    // labels → wall crossings at t = δ(u)/(δ(u) − δ(v)) chained into closed cell polygons; an unclosable chain
    // routes 2449 Panel naming the cell.
    static Fin<PanelBuild> SeededCells(SurfaceResult.UvTessellation source, Seq<Point3d> seeds, Op? key) {
        Point3d[] vertices = source.Mesh.Native.Vertices.ToPoint3dArray();
        Op op = key.OrDefault();
        return seeds.Map(seed => Nearest(vertices, seed))
            .TraverseM(vertex => GeodesicKernel.EnsureGeodesicDistances(source.Mesh, Seq(vertex), op))
            .As()
            .Bind(fields => Cells(source, vertices, fields, op));
    }

    // Geodesic sources are VERTICES, so the sample suite's continuous points snap ONCE here — a face-local
    // source would make every heat solve re-derive the same projection.
    static int Nearest(Point3d[] vertices, Point3d seed) {
        (int at, double best) = (0, double.PositiveInfinity);
        for (int v = 0; v < vertices.Length; v++) {
            double span = vertices[v].DistanceToSquared(seed);
            if (span < best) { (at, best) = (v, span); }
        }
        return at;
    }

    // A fragment corner is a BARYCENTRIC weight triple over its face's corners, so one carrier reconstructs
    // world position, UV, and EVERY cell's geodesic distance at that point — the wall interpolates provenance
    // rather than re-projecting it, and the clip re-reads distances at points no vertex occupies.
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

    // Per-vertex argmin label, per-triangle fragment clip, per-cell boundary chain. A cell whose survivors do
    // not close is the 2449 fault the page's Boundary names — silently dropping it would ship a facade with a
    // hole no receipt column reports.
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
            .Map(row => Loop(row.Value, vertices, source.Mesh.Tolerance, row.Key).Map(ring => (Cell: row.Key, Ring: ring)))
            .TraverseM(static ring => ring)
            .As()
            .Map(rings => Pack(rings, vertices, source.Uv));
    }

    static int Argmin(Seq<Arr<double>> fields, int vertex) {
        (int at, double best) = (0, double.PositiveInfinity);
        for (int c = 0; c < fields.Count; c++) {
            if (fields[c][vertex] < best) { (at, best) = (c, fields[c][vertex]); }
        }
        return at;
    }

    // Sutherland-Hodgman over the SCALAR half-spaces the geodesic fields define: the triangle keeps the region
    // where `cell` wins, one competitor at a time, each crossing landing at the equidistance parameter
    // t = δ(u)/(δ(u) − δ(v)) on the difference field. The clipper is a half-space per pass, so a triple point —
    // three cells meeting inside one face — resolves as the intersection of two clips with no special case.
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

    // The equidistance field: negative exactly where `cell` is nearer than `rival`, so its zero level IS the
    // wall and the crossing parameter is the page's own t = δ(u)/(δ(u) − δ(v)).
    static double Delta(Seq<Arr<double>> fields, int cell, int rival, Bary at) => at.Of(fields[cell]) - at.Of(fields[rival]);

    static Bary Lerp(Bary from, Bary to, double t) => new(
        from.A, from.B, from.C,
        from.W0 + ((to.W0 - from.W0) * t), from.W1 + ((to.W1 - from.W1) * t), from.W2 + ((to.W2 - from.W2) * t));

    // Boundary chain per cell: every fragment is wound in its face's own order, so an edge interior to the cell
    // appears once in each direction and cancels; the survivors are the cell wall. Endpoint identity quantizes
    // at the model tolerance — the same weld grain every emission seam on this branch uses — and a chain that
    // does not return to its seed faults with the cell as its unit.
    static Fin<Bary[]> Loop(List<Bary[]> fragments, Point3d[] vertices, Context tolerance, int cell) {
        Dictionary<(long, long, long), Bary> seat = new();
        Dictionary<(long, long, long), (long, long, long)> next = new();
        HashSet<((long, long, long) From, (long, long, long) To)> directed = new();
        foreach (Bary[] fragment in fragments) {
            for (int i = 0; i < fragment.Length; i++) {
                (Bary from, Bary to) = (fragment[i], fragment[(i + 1) % fragment.Length]);
                ((long, long, long) lo, (long, long, long) hi) = (Grain(from, vertices, tolerance), Grain(to, vertices, tolerance));
                if (lo == hi) { continue; }
                seat.TryAdd(lo, from);
                seat.TryAdd(hi, to);
                if (!directed.Remove((hi, lo))) { directed.Add((lo, hi)); }
            }
        }
        foreach (((long, long, long) from, (long, long, long) to) in directed) { next[from] = to; }
        if (next.Count < 3) { return Fault<Bary[]>(unit: cell, witness: next.Count); }
        List<Bary> ring = new(next.Count);
        (long, long, long) seed = next.Keys.Min();
        (long, long, long) walk = seed;
        for (int step = 0; step < next.Count; step++) {
            ring.Add(seat[walk]);
            if (!next.TryGetValue(walk, out walk)) { return Fault<Bary[]>(unit: cell, witness: ring.Count); }
            if (walk == seed) { return Fin.Succ<Bary[]>([.. ring]); }
        }
        return Fault<Bary[]>(unit: cell, witness: ring.Count);
    }

    // Weld grain: the emission-seam quantization every branch owner shares, so a wall crossing computed twice
    // from two incident faces keys once.
    static (long, long, long) Grain(Bary at, Point3d[] vertices, Context tolerance) {
        Point3d seat = at.World(vertices);
        double grain = tolerance.Absolute.Value;
        return ((long)Math.Round(seat.X / grain), (long)Math.Round(seat.Y / grain), (long)Math.Round(seat.Z / grain));
    }

    // Ragged offset rows over one corner table: a geodesic cell carries any corner count, so the offsets are
    // the shape `Assemble` folds and the lattice arm's uniform 4-stride is one instance of it. `PatchOf` takes
    // the SEED ordinal — this arm's real provenance, the cell each panel grew from — and `SingularFaces` is a
    // measured zero, not an absent one: the seeded arm runs no cross field, so it has no singularity to count.
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
        return new PanelBuild(toArr(offsets), toArr(corners), toArr(seats), toArr(feet), toArr(patchOf), SingularFaces: 0);
    }

    internal readonly record struct PanelBuild(
        Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv, Arr<int> PatchOf, int SingularFaces);

    // --- [ASSEMBLY]
    // Frames, defects, graph (family-agnostic): VectorFrame.NewellNormal plane per panel, planarity = max vertex-plane dist / diameter, adjacency transient → columns.
    static Fin<PanelResult> Assemble(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<int> fieldSymmetry, PanelPolicy policy, Op? key) {
        int panels = build.CornerOffsets.Count - 1;
        UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, panels));
        foreach ((int a, int b) in SharedWalls(build)) { graph.AddEdge(new SEdge<int>(a, b)); }
        Dictionary<int, int> componentOf = new();
        int components = graph.ConnectedComponents(componentOf);
        (Arr<int> offsets, Arr<int> adjacent) = AdjacencyColumns(graph, panels);
        return Frames(source, build, fieldSymmetry, key).Map(frames => {
            (Arr<double> planarity, double max, double mean) = PlanarityOf(build.CornerOffsets, build.Corners, build.Vertices);
            return new PanelResult(
                new PanelField(
                    build.CornerOffsets, build.Corners, build.Vertices, build.Uv,
                    frames.Origin, frames.X, frames.Z, planarity, build.PatchOf, offsets, adjacent,
                    new Arr<int>([.. Enumerable.Range(0, panels).Select(p => componentOf.GetValueOrDefault(p))]),
                    ShapeClasses(build, frames, source.Mesh.Tolerance)),
                new PanelReceipt(panels, build.Vertices.Count, components, max, mean, build.SingularFaces, Rounds: 0));
        });
    }

    // A panel's corner ring, the one projection every measure, frame, and round reads — the offset pair is the
    // ragged shape both families hand up, so no owner below re-derives it from a family-specific stride.
    static Point3d[] Ring(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices, int panel) =>
        [.. Enumerable.Range(offsets[panel], offsets[panel + 1] - offsets[panel]).Select(i => vertices[corners[i]])];

    // Panel adjacency is a SHARED WALL, never a shared corner: two cells meeting at one vertex are not
    // neighbours, and a corner-keyed fold would weld an entire vertex fan into one component.
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

    // CSR projection off the transient graph — the columns leave, the graph type never does.
    static (Arr<int> Offsets, Arr<int> Adjacent) AdjacencyColumns(UndirectedGraph<int, SEdge<int>> graph, int panels) {
        int[] offsets = new int[panels + 1];
        for (int p = 0; p < panels; p++) { offsets[p + 1] = offsets[p] + graph.AdjacentDegree(p); }
        int[] adjacent = new int[offsets[panels]];
        for (int p = 0; p < panels; p++) {
            int at = offsets[p];
            foreach (SEdge<int> edge in graph.AdjacentEdges(p)) { adjacent[at++] = edge.Source == p ? edge.Target : edge.Source; }
        }
        return (toArr(offsets), toArr(adjacent));
    }

    // THE planarity measure, read by assembly and by every planarize round: max corner-to-plane distance over
    // panel DIAMETER — dimensionless, so one budget grades a facade panel and a millimetre shingle alike. A
    // ring whose Newell normal or diameter vanishes reads +inf, the absence a zero defect cannot spell.
    static (Arr<double> Planarity, double Max, double Mean) PlanarityOf(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices) {
        double[] planarity = [.. Enumerable.Range(0, offsets.Count - 1).Select(p => Defect(Ring(offsets, corners, vertices, p)))];
        return (toArr(planarity), planarity.Length == 0 ? 0.0 : planarity.Max(), planarity.Length == 0 ? 0.0 : planarity.Average());
    }

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

    static Point3d Centroid(ReadOnlySpan<Point3d> ring) {
        Point3d seat = Point3d.Origin;
        foreach (Point3d corner in ring) { seat += corner; }
        return seat / ring.Length;
    }

    // z = Source.NormalAt at panel mean UV (degenerate → fault, never NaN). x: lattice (None) = stripe-U mean
    // edge ((c₁+c₂)−(c₀+c₃))/2; seeded (Some n) = SegmentKernel.CrossFieldAt(space, n, None, None, origin, key);
    // x re-orthogonalizes into the tangent plane, y = z × x derived at the consumer. The traversal aborts on the
    // first panel with no frame — a fabricated fallback axis would ship a mould oriented against nothing measured.
    static Fin<(Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z)> Frames(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<int> fieldSymmetry, Op? key) =>
        Range(0, build.CornerOffsets.Count - 1).ToSeq()
            .Map(p => FrameOf(source, build, fieldSymmetry, p, key))
            .TraverseM(static frame => frame)
            .As()
            .Map(static rows => (
                toArr(rows.Map(static row => row.Origin)),
                toArr(rows.Map(static row => row.X)),
                toArr(rows.Map(static row => row.Z))));

    // The surface is evaluated at the panel's OWN corner-UV centroid, never at the world centroid re-projected,
    // so a panel spanning a high-curvature band keeps its parameter provenance instead of paying a second
    // closest-point solve the pullback already answered.
    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> FrameOf(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<int> fieldSymmetry, int panel, Op? key) {
        (int lo, int hi) = (build.CornerOffsets[panel], build.CornerOffsets[panel + 1]);
        Point3d seat = Centroid(Ring(build.CornerOffsets, build.Corners, build.Vertices, panel));
        Point2d foot = UvSeat(build, lo, hi);
        return from normal in source.Source.NormalAt(foot.X, foot.Y)
               from axis in fieldSymmetry.Match(
                   Some: symmetry => SegmentKernel.CrossFieldAt(source.Mesh, symmetry, None, None, seat, key.OrDefault()),
                   None: () => Fin.Succ(StripeU(build, lo, hi)))
               from frame in Orthonormal(seat, normal, axis, panel)
               select frame;
    }

    static Point2d UvSeat(PanelBuild build, int lo, int hi) {
        (double u, double v) = (0.0, 0.0);
        for (int i = lo; i < hi; i++) { (u, v) = (u + build.Uv[build.Corners[i]].X, v + build.Uv[build.Corners[i]].Y); }
        return new Point2d(u / (hi - lo), v / (hi - lo));
    }

    // Stripe-U mean edge — the lattice arm's SOLE local frame read, holding only because the emitted quad IS
    // the integrated field; a ragged ring has no stripe and falls back to its first edge.
    static Vector3d StripeU(PanelBuild build, int lo, int hi) =>
        hi - lo == 4
            ? ((build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]])
               + (build.Vertices[build.Corners[lo + 2]] - build.Vertices[build.Corners[lo + 3]])) * 0.5
            : build.Vertices[build.Corners[lo + 1]] - build.Vertices[build.Corners[lo]];

    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> Orthonormal(Point3d seat, Vector3d normal, Vector3d axis, int panel) {
        Vector3d z = normal;
        if (!z.Unitize()) { return Fault<(Point3d, Vector3d, Vector3d)>(unit: panel, witness: normal.Length); }
        Vector3d x = axis - ((z * axis) * z);
        return x.Unitize()
            ? Fin.Succ((seat, x, z))
            : Fault<(Point3d, Vector3d, Vector3d)>(unit: panel, witness: axis.Length);
    }

    // Congruence classification: per panel the frame-local corner polygon reduced to its rigid-motion
    // invariants — consecutive edge lengths quantized at the model's absolute tolerance, turn angles at its
    // ANGLE tolerance, because a length grain applied to a radian is a units error — folded through the branch
    // content-key owner. The digest is read forward AND reversed and keyed on the smaller, so a MIRRORED panel
    // reuses its class rather than cutting a second mould. Classes take first-seen ordinals.
    static Arr<int> ShapeClasses(PanelBuild build, (Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z) frames, Context tolerance) {
        Dictionary<UInt128, int> ordinalOf = new();
        int[] classes = new int[build.CornerOffsets.Count - 1];
        for (int p = 0; p < classes.Length; p++) {
            long[] forward = Invariants(build, frames, p, tolerance);
            long[] mirrored = [.. forward.Reverse()];
            UInt128 key = UInt128.Min(Digest(forward), Digest(mirrored));
            classes[p] = ordinalOf.TryGetValue(key, out int seen) ? seen : ordinalOf[key] = ordinalOf.Count;
        }
        return toArr(classes);
    }

    // Little-endian per row through the ONE content-key owner every kernel identity composes — a second hasher
    // beside it forks the seed the federation reproduces, so the class ordinal rides the same digest space.
    static UInt128 Digest(long[] rows) =>
        ContentHash.Of(rows, static (measures, sink) => {
            Span<byte> lane = stackalloc byte[sizeof(long)];
            foreach (long measure in measures) {
                BinaryPrimitives.WriteInt64LittleEndian(lane, measure);
                sink.Append(lane);
            }
        });

    // Corners project into the panel's own tangent frame, then alternate edge length and turn angle around the
    // ring — position and orientation divided out by construction, so two panels sharing one mould key alike.
    static long[] Invariants(PanelBuild build, (Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z) frames, int panel, Context tolerance) {
        Point3d[] ring = Ring(build.CornerOffsets, build.Corners, build.Vertices, panel);
        (Point3d seat, Vector3d ax, Vector3d az) = (frames.Origin[panel], frames.X[panel], frames.Z[panel]);
        Vector3d ay = Vector3d.CrossProduct(az, ax);
        Point2d[] flat = [.. ring.Select(corner => new Point2d((corner - seat) * ax, (corner - seat) * ay))];
        long[] measures = new long[2 * flat.Length];
        for (int i = 0; i < flat.Length; i++) {
            Vector2d edge = flat[(i + 1) % flat.Length] - flat[i];
            Vector2d prior = flat[i] - flat[((i - 1) + flat.Length) % flat.Length];
            measures[2 * i] = (long)Math.Round(edge.Length / tolerance.Absolute.Value);
            measures[(2 * i) + 1] = (long)Math.Round(
                Math.Atan2((prior.X * edge.Y) - (prior.Y * edge.X), (prior.X * edge.X) + (prior.Y * edge.Y)) / tolerance.Angle.Value);
        }
        return measures;
    }

    // --- [PLANARIZE]
    // Bounded proximal rounds — monotone in max defect, early exit inside budget: VectorFrame.NewellNormal fit per panel, every vertex → MEAN of incident projections.
    // Frames re-derive from the planarized planes; UV columns keep the pre-planarization feet — planar panels leave the surface by design.
    static Fin<PanelResult> PlanarizeOf(PanelResult prior, PanelPolicy policy, Op? key) =>
        Range(0, policy.PlanarizeRounds).Fold(
                Fin.Succ((Field: prior.Field, Max: prior.Receipt.MaxPlanarity, Rounds: 0)),
                (state, _) => state.Bind(s => s.Max <= policy.PlanarityBudget
                    ? Fin.Succ(s)
                    : ProjectRound(s.Field).Map(next => (next.Field, next.Max, s.Rounds + 1))))
            .Bind(final => final.Max > policy.PlanarityBudget
                ? Fault<PanelResult>(unit: WorstPanel(final.Field), witness: final.Max)
                : Fin.Succ(new PanelResult(final.Field, prior.Receipt with {
                    MaxPlanarity = final.Max, MeanPlanarity = MeanPlanarity(final.Field), Rounds = final.Rounds })));

    // One proximal round: fit each panel's plane at its corner centroid through the Newell normal, project every
    // incident corner onto it, and move each vertex to the MEAN of its projections — the averaging that makes
    // the round monotone in max defect where a per-panel snap oscillates between two panels' planes. Frames
    // re-derive from the FITTED planes, not the surface: a planarized panel has left the surface by design, so
    // reading `NormalAt` again would bind the mould to a sheet it no longer touches. UV columns are carried
    // untouched — they are the pre-planarization feet the receipt's provenance claim rests on.
    static Fin<(PanelField Field, double Max)> ProjectRound(PanelField field) {
        Vector3d[] pulled = new Vector3d[field.Vertices.Count];
        int[] hits = new int[field.Vertices.Count];
        int panels = field.CornerOffsets.Count - 1;
        Vector3d[] planeNormal = new Vector3d[panels];
        for (int p = 0; p < panels; p++) {
            Point3d[] ring = Ring(field.CornerOffsets, field.Corners, field.Vertices, p);
            Vector3d normal = VectorFrame.NewellNormal(ring);
            if (!normal.Unitize()) { continue; }               // a degenerate ring contributes no plane this round
            planeNormal[p] = normal;
            Point3d seat = Centroid(ring);
            for (int i = field.CornerOffsets[p]; i < field.CornerOffsets[p + 1]; i++) {
                int v = field.Corners[i];
                pulled[v] += (Vector3d)field.Vertices[v] - ((normal * (field.Vertices[v] - seat)) * normal);
                hits[v]++;
            }
        }
        Arr<Point3d> moved = toArr(Enumerable.Range(0, field.Vertices.Count)
            .Select(v => hits[v] == 0 ? field.Vertices[v] : new Point3d(pulled[v].X / hits[v], pulled[v].Y / hits[v], pulled[v].Z / hits[v])));
        (Arr<double> planarity, double max, double _) = PlanarityOf(field.CornerOffsets, field.Corners, moved);
        return Fin.Succ((
            field with {
                Vertices = moved, Planarity = planarity,
                Origin = toArr(Enumerable.Range(0, panels).Select(p => Centroid(Ring(field.CornerOffsets, field.Corners, moved, p)))),
                ZAxis = toArr(Enumerable.Range(0, panels).Select(p => planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p])),
                XAxis = toArr(Enumerable.Range(0, panels).Select(p => Retangent(field.XAxis[p], planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p]))),
            },
            max));
    }

    // Re-seat the prior x-axis into the new plane, keeping the field alignment the frame was built for; an axis
    // the new normal has swallowed keeps its prior value rather than spinning to an arbitrary perpendicular.
    static Vector3d Retangent(Vector3d prior, Vector3d normal) {
        Vector3d axis = prior - ((normal * prior) * normal);
        return axis.Unitize() ? axis : prior;
    }

    // Both read the field's OWN planarity column — the measure `ProjectRound` already wrote — so no consumer
    // re-derives a defect the carrier states, and the receipt's max and mean can never disagree with the column.
    static int WorstPanel(PanelField field) =>
        field.Planarity.Count == 0 ? 0 : Enumerable.Range(0, field.Planarity.Count).MaxBy(p => field.Planarity[p]);

    static double MeanPlanarity(PanelField field) =>
        field.Planarity.Count == 0 ? 0.0 : field.Planarity.Fold(0.0, static (sum, defect) => sum + defect) / field.Planarity.Count;

    static Fin<T> Fault<T>(int unit, double witness) =>
        Fin.Fail<T>(new GeometryFault.DevelopmentFault(DevelopmentStage.Panel, unit, witness).ToError());
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Panelization flow
    accDescr: PanelOp folds seeds through the cross-field remesh and pullback into the PanelField SoA wire with frames, planarity, adjacency, component, and shape-class columns.
    UvT["surface.md UvTessellation — mesh + (u,v) + binding"] -->|"Panelization.Apply — ONE Switch"| Family["PanelFamily — data rows"]
    Family -->|"Lattice: Remeshing.Apply(QuadField)"| Remesh["remesh.md QuadProvenance channels"]
    Remesh -->|"ONE batch Pullback — provenance restore"| Build["PanelBuild — polygons + UV"]
    Family -->|"Seeded: SampleKernel.Sample over ExtractionDomain.Mesh"| Seeds["sample.md CCVT / blue-noise seeds"]
    Seeds -->|"k cached heat solves → argmin labels → lerp walls"| Build
    Build -->|"Newell planes · planarity defects · CrossFieldAt / stripe-U frames"| Field["PanelField — frames + defects"]
    Build -->|"transient UndirectedGraph → adjacency + components"| Field
    Field -->|"bounded proximal rounds toward PlanarityBudget"| Planar["Planarize — fabrication grade"]
    Field --> Gate["Generation panelization gate"]
    UvT -.->|"2449 Panel — witness = planarity defect"| GeometryFault
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
