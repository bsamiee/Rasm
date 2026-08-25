# [RASM_PARAMETRIC_PANELIZE]

`Panelization` owns cross-field-guided panelization: `Apply` maps a UV-provenanced surface into a panel graph whose every panel leaves with a placement frame — origin, field-aligned x-axis, and metric-true binding normal — because position without orientation is half a panel. `PanelFamily` rides the request as data, so a new family is one case over the shared assembly fold rather than a sibling mapper, and per-panel planarity is the fabrication acceptance measure whose breach routes a fault instead of shipping an unfabricatable lattice. Mould reuse keys through the `patternmap.md` material-symmetry law: a mirrored congruence merges only where the material's `MirrorGrant` merges it, `Flipped` records which panels ride the mirrored digest, and the merges the law refused count on the census as the mould-cost delta of the material choice.

Input is `surface.md`'s `SurfaceResult.UvTessellation` — mesh, per-vertex `(u, v)`, and live `NurbsForm.Surface` binding — so an unbound mesh cannot enter and every `PanelField` keeps its UV provenance. `Lattice` consumes the remesh substrate's `QuadProvenance` without re-running any field solve while `Seeded` lands geodesic-Voronoi cells over the `sample.md` distribution suite, `Symmetry` the one `RoSyOrder` row keying both arms; adjacency folds through a transient QuikGraph and leaves as SoA columns, never a leaked graph type.

## [01]-[INDEX]

- [02]-[PANELIZATION]: `PanelFamily` family-as-data folded by one `Panelization.Apply` into a placement-framed, planarity-gated `PanelField` panel graph.

## [02]-[PANELIZATION]

- Owner: `Panelization` mints the one static entry; `PanelFamily` carries the family as data, `PanelPolicy` the `IValidityEvidence` policy row carrying the lane-resolved planarity band and the `MaterialSymmetry` law the congruence fold reads (`None` reading `Free`), and `PanelResult` carries the panel-graph-plus-frame `PanelField` with its retained fabrication measures.
- Cases: `PanelFamily` cases `Lattice` and `Seeded` — the substrate-guided lattice and the sample-suite distribution, `Symmetry` the one `RoSyOrder` row keying both; `PanelOp` cases `Map` and `Planarize` — generation versus fabrication-correction, `Planarize` consuming `Map`'s carrier.
- Entry: `public static Fin<PanelResult> Apply(PanelOp op, Op? key = null)` — the one entry discriminating on the op case, the family arm discriminating inside it.
- Auto: `Map`+`Lattice` binds the substrate's `QuadProvenance` as the panel lattice and restores UV through one batch `Pullback`; `Map`+`Seeded` lands geodesic-Voronoi cells from cached heat-distance labels walled at the equidistance lerp. Both arms assemble identically — Newell plane per panel, planarity defect, adjacency folded through a transient graph into offset columns — and `Planarize` runs bounded proximal rounds toward the planarity band, keeping each panel's pre-planarization UV feet while frames and the `ShapeClass`/`Flipped`/`ChiralSplit` evidence re-derive from the planarized geometry — congruence answers the final rings, never the ones the rewrite retired.
- Law: the result's planarity band is ONE `Stat<Scalar>` derived from the field's own `Planarity` column, never a max/mean pair beside it. NAMED LOSS: the two scalar fields; the gain is that the band cannot disagree with the column it summarizes, and the consumer reads variance and RMS no pair carries. WITNESS: `result.MaxPlanarity` rebuilt as `result.Planarity.Maximum.To()`, the same value off `Stat<Scalar>.Of(column.AsSpan(), key)`.
- Law: the acceptance ceiling is `Tolerance` off `ToleranceLane.Fraction` — the defect is dimensionless (max vertex-plane deviation over panel diameter), so it belongs to the ratio band and the document sets it. NAMED LOSS: `PanelPolicy.Canonical` and its `5e-3` literal.
- Exemption: `Loop`, `Cells`, `SharedWalls`, and `ShapeClasses` hold mutable `Dictionary`/`HashSet`/`List` accumulators inside their own span windows — a walled-cell chain, a wall pairing, and a first-seen class roster are single-pass build state that never escapes the member, and `Grain` states its quantum (the model's absolute tolerance, the branch's emission-seam grain) on site.
- Output: `PanelResult` carries the planarity band, the `ChiralSplit` count, and planarize rounds beside the field; counts derivable from the field and unconsumed build tallies do not leave the producer.
- Packages: `Rasm.Processing` for the remesh substrate (`QuadProvenance`, `RemeshOp.QuadField`, `RemeshPolicy`, `RoSyOrder`) and the seed suite (`SampleKind`, `SampleKernel.Sample`, `SegmentKernel.CrossFieldAt`, `GeodesicKernel.EnsureGeodesicDistances`, `ExtractionDomain.Mesh`); `Rasm.Parametric` `surface.md` for the `UvTessellation` input and `Pullback` restore, `nurbs.md` for the frame normals, and `patternmap.md` for the `MaterialSymmetry` law the congruence fold reads; `Rasm.Spatial` `ScalarField` for density seeds and `NeighborIndex`/`NeighborSource`/`NeighborKernel` for the one batch seed snap; `Rasm.Numerics` for `VectorFrame.NewellNormal`, `Dimension`/`PositiveMagnitude`, and `GeometryFault`; `Rasm.Domain` for `Op`, `Context`/`ToleranceLane`/`Tolerance`, `ContentHash`/`CanonicalWriter`, `Stat<Scalar>`/`Scalar`, and `IValidityEvidence`; QuikGraph for the transient adjacency fold; Rhino.Geometry, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new panel family is one `PanelFamily` case over the same assembly fold; a new seed distribution is one `SampleKind` row; a new panel measure is one `PanelField` column — `ShapeClass` congruence and its `Flipped` parity are the executed precedent; a fabrication-nesting order is one projection off the adjacency columns.
- Boundary: the field solve is the substrate's — a `CrossFieldAt`/`StripeAt` loop here is the named re-derivation defect, the lattice arm consuming `QuadProvenance` whole, its sole local frame read (stripe-U off the quad's own corners) holding only because the emitted geometry is the integrated field. Output keeps provenance — a wire without UV columns is the named drop, restored by one batch `Pullback` never a per-vertex `ClosestParameter` loop; seeded labels are geodesic, a Euclidean nearest-seed the named naivety defect across folds. `Planarize` fits per-panel planes and never parameterizes, a conformal or ARAP energy belonging to `flatten.md`; QuikGraph stays transient with adjacency leaving as offset columns, a stored graph field the named lane violation; every content key rides the branch `CanonicalWriter` — a second hasher or a hand `BinaryPrimitives` preimage forks the seed the federation reproduces; mould-reuse congruence merges a mirrored digest only where the material law's `MirrorRight.Merge` right licenses it — an unconditional min-digest merge cuts a mirrored panel from a blank a chiral material cannot flip, and a fold branching on grant identity instead of the rights set is the named re-derivation; every failure routes `DevelopmentFault(Panel, …)` with the panel unit and its planarity or admission witness, composed rails surfacing their own faults untranslated.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
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

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PanelFamily {
    private PanelFamily() { }

    public sealed record Lattice(RoSyOrder Symmetry, PositiveMagnitude TargetLength) : PanelFamily;
    public sealed record Seeded(SampleKind Seeds, RoSyOrder Symmetry) : PanelFamily;
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record PanelPolicy(
    Tolerance Planarity, Dimension Rounds, RemeshPolicy Remesh, PullbackPolicy Pullback,
    Option<MaterialSymmetry> Law = default) : IValidityEvidence {
    public static PanelPolicy Of(Context context) => new(
        Planarity: context.For(lane: ToleranceLane.Fraction), Rounds: Dimension.Create(value: 32),
        Remesh: RemeshPolicy.Canonical, Pullback: PullbackPolicy.Of(context: context));

    public bool IsValid => ValidityClaim.All(Planarity.IsValid, Remesh.IsValid, Pullback.IsValid);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PanelField(
    Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv,
    Arr<Point3d> Origin, Arr<Vector3d> XAxis, Arr<Vector3d> ZAxis, Arr<double> Planarity,
    Arr<int> PatchOf, Arr<int> AdjacencyOffsets, Arr<int> Adjacent, Arr<int> Component, Arr<int> ShapeClass, Arr<bool> Flipped,
    Context Model);

public sealed record PanelResult(PanelField Field, Stat<Scalar> Planarity, int ChiralSplit, int Rounds);

// --- [OPERATIONS] ----------------------------------------------------------------------
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
                ? Fault<PanelResult>(witness: "planarity band", measure: m.Policy.Planarity.Value)
                : m.Family.Switch(
                    state: (m.Source, m.Policy, Key: k),
                    lattice: static (s, f) => LatticePanels(s.Source, f, s.Policy, s.Key),
                    seeded:  static (s, f) => SeededPanels(s.Source, f, s.Policy, s.Key)),
            planarize: static (k, p) => PlanarizeOf(p.Prior, p.Policy, k));

    // --- [LATTICE]
    static Fin<PanelResult> LatticePanels(SurfaceResult.UvTessellation source, PanelFamily.Lattice family, PanelPolicy policy, Op key) =>
        Remeshing.Apply(new RemeshOp.QuadField(source.Mesh, family.TargetLength, family.Symmetry, policy.Remesh), key)
            .Bind(remesh => remesh.Quads.Match(
                Some: quads => Reprovenance(source, remesh.Mesh, policy, key)
                    .Bind(uv => Assemble(source, LatticeBuild(remesh.Mesh, quads, uv), fieldSymmetry: None, policy, key)),
                None: () => Fault<PanelResult>(witness: "target length", measure: family.TargetLength.Value)));

    static Fin<Arr<Point2d>> Reprovenance(SurfaceResult.UvTessellation source, MeshSpace emitted, PanelPolicy policy, Op key) =>
        Surfaces.Apply(new SurfaceOp.Pullback(source.Source, toArr(emitted.Native.Vertices.ToPoint3dArray()), policy.Pullback), key)
            .Bind(result => result is SurfaceResult.Pulled pulled
                ? Fin.Succ(pulled.Uv)
                : Fault<Arr<Point2d>>(witness: "vertex extent", measure: emitted.Native.Vertices.Count));

    static PanelBuild LatticeBuild(MeshSpace emitted, QuadProvenance quads, Arr<Point2d> uv) {
        int panels = quads.Corners.Count / 4;
        return new PanelBuild(
            CornerOffsets: toArr(Enumerable.Range(0, panels + 1).Select(static p => 4 * p)),
            Corners: quads.Corners,
            Vertices: toArr(emitted.Native.Vertices.ToPoint3dArray()),
            Uv: uv,
            PatchOf: quads.PatchOf);
    }

    // --- [SEEDED]
    static Fin<PanelResult> SeededPanels(SurfaceResult.UvTessellation source, PanelFamily.Seeded family, PanelPolicy policy, Op key) =>
        ExtractionDomain.Mesh(source.Mesh, key)
            .Bind(domain => SampleKernel.Sample(family.Seeds, domain, source.Mesh.Tolerance, key))
            .Bind(seeds => SeededCells(source, seeds.Points, key))
            .Bind(build => Assemble(source, build, fieldSymmetry: Some(family.Symmetry), policy, key));

    static Fin<PanelBuild> SeededCells(SurfaceResult.UvTessellation source, Seq<Point3d> seeds, Op key) {
        Point3d[] vertices = source.Mesh.Native.Vertices.ToPoint3dArray();
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(vertices)), key: key)
            .Bind(index => NeighborKernel.GraphOf(
                index: index, needles: [.. seeds], count: Some(1), radius: Option<double>.None, key: key))
            .Bind(graph => toSeq(graph.Ids).TraverseM(hits => hits.Length > 0
                ? Fin.Succ(hits[0])
                : Fault<int>(witness: "seed with no nearest tessellation vertex")).As())
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

    static Fin<Bary[]> Loop(List<Bary[]> fragments, Point3d[] vertices, Context model, int cell) {
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
        if (next.Count < 3) { return Fault<Bary[]>(witness: "cell wall census under a ring", unit: cell, measure: next.Count); }
        List<Bary> ring = new(next.Count);
        (long, long, long) seed = next.Keys.Min();
        (long, long, long) walk = seed;
        for (int step = 0; step < next.Count; step++) {
            ring.Add(seat[walk]);
            if (!next.TryGetValue(walk, out walk)) { return Fault<Bary[]>(witness: "broken cell wall chain", unit: cell, measure: ring.Count); }
            if (walk == seed) { return Fin.Succ<Bary[]>([.. ring]); }
        }
        return Fault<Bary[]>(witness: "unclosed cell wall chain", unit: cell, measure: ring.Count);
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
        return new PanelBuild(toArr(offsets), toArr(corners), toArr(seats), toArr(feet), toArr(patchOf));
    }

    internal readonly record struct PanelBuild(
        Arr<int> CornerOffsets, Arr<int> Corners, Arr<Point3d> Vertices, Arr<Point2d> Uv, Arr<int> PatchOf);

    // --- [ASSEMBLY]
    static Fin<PanelResult> Assemble(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RoSyOrder> fieldSymmetry, PanelPolicy policy, Op key) {
        int panels = build.CornerOffsets.Count - 1;
        UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, panels));
        foreach ((int a, int b) in SharedWalls(build)) { graph.AddEdge(new SEdge<int>(a, b)); }
        Dictionary<int, int> componentOf = new();
        graph.ConnectedComponents(componentOf);
        (Arr<int> offsets, Arr<int> adjacent) = AdjacencyColumns(graph, panels);
        return Frames(source, build, fieldSymmetry, key).Bind(frames => {
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

    static Point3d[] Ring(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices, int panel) =>
        [.. Enumerable.Range(offsets[panel], offsets[panel + 1] - offsets[panel]).Select(i => vertices[corners[i]])];

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
        return (toArr(offsets), toArr(adjacent));
    }

    static Arr<double> PlanarityOf(Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices) =>
        toArr(Enumerable.Range(0, offsets.Count - 1).Select(p => Defect(Ring(offsets, corners, vertices, p))));

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

    static Fin<(Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z)> Frames(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RoSyOrder> fieldSymmetry, Op key) =>
        Range(0, build.CornerOffsets.Count - 1).ToSeq()
            .Map(p => FrameOf(source, build, fieldSymmetry, p, key))
            .TraverseM(static frame => frame)
            .As()
            .Map(static rows => (
                toArr(rows.Map(static row => row.Origin)),
                toArr(rows.Map(static row => row.X)),
                toArr(rows.Map(static row => row.Z))));

    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> FrameOf(
        SurfaceResult.UvTessellation source, PanelBuild build, Option<RoSyOrder> fieldSymmetry, int panel, Op key) {
        (int lo, int hi) = (build.CornerOffsets[panel], build.CornerOffsets[panel + 1]);
        Point3d seat = Centroid(Ring(build.CornerOffsets, build.Corners, build.Vertices, panel));
        Point2d foot = UvSeat(build, lo, hi);
        return from normal in source.Source.NormalAt(foot.X, foot.Y)
               from axis in fieldSymmetry.Match(
                   Some: order => SegmentKernel.CrossFieldAt(source.Mesh, order.Key, None, None, seat, key),
                   None: () => Fin.Succ(StripeU(build, lo, hi)))
               from frame in Orthonormal(seat, normal, axis, panel)
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

    static Fin<(Point3d Origin, Vector3d X, Vector3d Z)> Orthonormal(Point3d seat, Vector3d normal, Vector3d axis, int panel) {
        Vector3d z = normal;
        if (!z.Unitize()) { return Fault<(Point3d, Vector3d, Vector3d)>(unit: panel, witness: "degenerate normal", measure: normal.Length); }
        Vector3d x = axis - ((z * axis) * z);
        return x.Unitize()
            ? Fin.Succ((seat, x, z))
            : Fault<(Point3d, Vector3d, Vector3d)>(unit: panel, witness: "degenerate axis", measure: axis.Length);
    }

    static (Arr<int> Classes, Arr<bool> Flipped, int ChiralSplit) ShapeClasses(
        Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices,
        (Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z) frames, Context model, MaterialSymmetry law) {
        Dictionary<UInt128, int> ordinalOf = new();
        HashSet<UInt128> merged = [];
        int[] classes = new int[offsets.Count - 1];
        bool[] flipped = new bool[classes.Length];
        bool merges = law.Mirror.Rights.Admits(MirrorRight.Merge);
        for (int p = 0; p < classes.Length; p++) {
            long[] forward = LeastRotation(Invariants(offsets, corners, vertices, frames, p, model));
            long[] mirrored = LeastRotation(Mirrored(forward));
            (UInt128 f, UInt128 m) = (Digest(forward), Digest(mirrored));
            UInt128 least = UInt128.Min(f, m);
            UInt128 key = merges ? least : f;
            flipped[p] = merges && m < f;
            classes[p] = ordinalOf.TryGetValue(key, out int seen) ? seen : ordinalOf[key] = ordinalOf.Count;
            merged.Add(least);
        }
        return (toArr(classes), toArr(flipped), ordinalOf.Count - merged.Count);
    }

    static long[] Mirrored(long[] forward) {
        int n = forward.Length / 2;
        long[] mirrored = new long[forward.Length];
        for (int j = 0; j < n; j++) {
            mirrored[2 * j] = forward[2 * (n - 1 - j)];
            mirrored[(2 * j) + 1] = -forward[(2 * ((n - j) % n)) + 1];
        }
        return mirrored;
    }

    static long[] LeastRotation(long[] pairs) {
        int n = pairs.Length / 2;
        if (n <= 1) { return pairs; }
        int[] failure = new int[2 * n];
        Array.Fill(failure, -1);
        int least = 0;
        for (int j = 1; j < 2 * n; j++) {
            int i = failure[j - least - 1];
            while (i != -1 && Compare(pairs, n, j, least + i + 1) != 0) {
                if (Compare(pairs, n, j, least + i + 1) < 0) { least = j - i - 1; }
                i = failure[i];
            }
            if (Compare(pairs, n, j, least + i + 1) != 0) {
                if (Compare(pairs, n, j, least) < 0) { least = j; }
                failure[j - least] = -1;
            } else {
                failure[j - least] = i + 1;
            }
        }
        long[] rotated = new long[pairs.Length];
        for (int i = 0; i < n; i++) {
            int at = (least + i) % n;
            (rotated[2 * i], rotated[(2 * i) + 1]) = (pairs[2 * at], pairs[(2 * at) + 1]);
        }
        return rotated;
    }

    static int Compare(long[] pairs, int n, int left, int right) {
        (int a, int b) = (left % n, right % n);
        return pairs[2 * a] != pairs[2 * b]
            ? pairs[2 * a].CompareTo(pairs[2 * b])
            : pairs[(2 * a) + 1].CompareTo(pairs[(2 * b) + 1]);
    }

    static UInt128 Digest(long[] rows) =>
        ContentHash.Of(rows, static (measures, sink) => sink.Rows(toSeq(measures), static (measure, lane) => lane.I64(measure)));

    static long[] Invariants(
        Arr<int> offsets, Arr<int> corners, Arr<Point3d> vertices,
        (Arr<Point3d> Origin, Arr<Vector3d> X, Arr<Vector3d> Z) frames, int panel, Context model) {
        Point3d[] ring = Ring(offsets, corners, vertices, panel);
        (Point3d seat, Vector3d ax, Vector3d az) = (frames.Origin[panel], frames.X[panel], frames.Z[panel]);
        Vector3d ay = Vector3d.CrossProduct(az, ax);
        Point2d[] flat = [.. ring.Select(corner => new Point2d((corner - seat) * ax, (corner - seat) * ay))];
        long[] measures = new long[2 * flat.Length];
        for (int i = 0; i < flat.Length; i++) {
            Vector2d edge = flat[(i + 1) % flat.Length] - flat[i];
            Vector2d prior = flat[i] - flat[((i - 1) + flat.Length) % flat.Length];
            measures[2 * i] = (long)Math.Round(edge.Length / model.Absolute.Value);
            measures[(2 * i) + 1] = (long)Math.Round(
                Math.Atan2((prior.X * edge.Y) - (prior.Y * edge.X), (prior.X * edge.X) + (prior.Y * edge.Y)) / model.Angle.Value);
        }
        return measures;
    }

    // --- [PLANARIZE]
    static Fin<PanelResult> PlanarizeOf(PanelResult prior, PanelPolicy policy, Op key) =>
        Range(0, policy.Rounds.Value).FoldUntil(
                state: Fin.Succ((Field: prior.Field, Band: prior.Planarity, Rounds: 0)),
                f: (state, _) => state.Bind(s => ProjectRound(s.Field, key).Map(next => (next.Field, next.Band, s.Rounds + 1))),
                stateP: state => state.Match(
                    Succ: s => s.Band.Maximum.To() <= policy.Planarity.Value,
                    Fail: static _ => true))
            .Bind(final => final.Band.Maximum.To() > policy.Planarity.Value
                ? Fault<PanelResult>(witness: "planarity band", unit: WorstPanel(final.Field), measure: final.Band.Maximum.To())
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
        Arr<Point3d> moved = toArr(Enumerable.Range(0, field.Vertices.Count)
            .Select(v => hits[v] == 0 ? field.Vertices[v] : new Point3d(pulled[v].X / hits[v], pulled[v].Y / hits[v], pulled[v].Z / hits[v])));
        Arr<double> planarity = PlanarityOf(field.CornerOffsets, field.Corners, moved);
        return Stat<Scalar>.Of(planarity.AsSpan(), key).Map(band => (
            field with {
                Vertices = moved, Planarity = planarity,
                Origin = toArr(Enumerable.Range(0, panels).Select(p => Centroid(Ring(field.CornerOffsets, field.Corners, moved, p)))),
                ZAxis = toArr(Enumerable.Range(0, panels).Select(p => planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p])),
                XAxis = toArr(Enumerable.Range(0, panels).Select(p => Retangent(field.XAxis[p], planeNormal[p].IsZero ? field.ZAxis[p] : planeNormal[p]))),
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

    static Fin<T> Fault<T>(string witness, Option<int> unit = default, Option<double> measure = default) =>
        Fin.Fail<T>(new GeometryFault.DevelopmentFault(DevelopmentStage.Panel, unit, witness, measure));
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
    accDescr: PanelOp folds seeds through the cross-field remesh and pullback into the PanelField SoA wire with frames, planarity, adjacency, component, shape-class, and flip-parity columns.
    UvT["surface.md UvTessellation — mesh + (u,v) + binding"] -->|"Panelization.Apply — ONE Switch"| Family["PanelFamily — data rows"]
    Family -->|"Lattice: Remeshing.Apply(QuadField)"| Remesh["remesh.md QuadProvenance channels"]
    Remesh -->|"ONE batch Pullback — provenance restore"| Build["PanelBuild — polygons + UV"]
    Family -->|"Seeded: SampleKernel.Sample over ExtractionDomain.Mesh"| Seeds["sample.md CCVT / blue-noise seeds"]
    Seeds -->|"k cached heat solves → argmin labels → lerp walls"| Build
    Build -->|"Newell planes · planarity defects · CrossFieldAt / stripe-U frames"| Field["PanelField — frames + defects"]
    Build -->|"transient UndirectedGraph → adjacency + components"| Field
    Field -->|"Stat&lt;Scalar&gt; band over the planarity column"| Result["PanelResult — field + retained measures"]
    Field -->|"bounded proximal rounds toward the Planarity band"| Planar["Planarize — fabrication grade"]
    Field --> Gate["Generation panelization gate"]
    UvT -.->|"DevelopmentFault.Panel — planarity defect"| GeometryFault
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
