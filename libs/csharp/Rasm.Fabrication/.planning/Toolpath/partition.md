# [RASM_FABRICATION_PARTITION]

`Partition` owns admitted point-site decomposition from a generative site field through a boundary-clipped diagram, cell topology, spanning traversal, optional open-stroke classification, and — where the projection carries a depth — the kernel's 3D complex over the same accepted sites. Every result retains each disconnected cell region with site, centroid, border, perimeter, edge, adjacency, area, traversal, and ordering evidence for pocketing, stippling, engraving, pen plotting, inspection, and deterministic replay, beside the bound-gated solid measures a lattice, fracture, or support-cell substrate reads.

`Partition.Seed` consumes one admitted request and returns one evidence-complete receipt. A strategy declares a target AREAL DENSITY and the boundary area resolves it into the concrete site count, pitch, separation, and merge floor one run uses, so a clamped count carries its own achieved pitch instead of separating against a nominal one it never realized. Determinism rides the kernel `Deterministic` lanes for both cloud generation and the density-rejection draw over ONE site box, so every draw is index-addressed rather than stream-ordered and the acceptance weight answers on the same box the candidate was drawn into; fixed anchors participate in every field, density callbacks cross one exception boundary, separation is enforced through a pitch-keyed bucket index rather than a pairwise scan, and the diagram itself clips to the admitted `Loop` at its own owner before area, topology, or egress decisions consume it.

## [01]-[INDEX]

- [02]-[PARTITION]: `PartitionStrategy` admits generator policy and resolves it against boundary area, `PartitionRequest` closes output modality, and `Partition.Seed` folds `PolygonOp.Cells`, `VectorIntent.Voronoi`, and QuikGraph into `PartitionReceipt`.

## [02]-[PARTITION]

- Owner: `SiteDistribution` carries each candidate-cloud law as its own draw delegate over the kernel lanes, `DensityField` carries each acceptance-weight law as a box-relative row, `SamplingField` binds distribution, seed, and density into one generated structural value, `PartitionField` carries the boundary-resolved run geometry, and `PartitionRequest` admits the complete boundary-plus-projection aggregate.
- Cases: `SiteDistribution` rows close the candidate laws and `DensityField` rows close the acceptance weights — a new law of either kind is one row carrying its own fold; `PartitionProjection` closes region-only, stroke-classifying, and volumetric egress without overloads, its `DepthMm` column the one value the site box, the anchor gate, and the containment predicate read so the planar cases stay branch-free at zero.
- Law: the strategy declares `SiteDensityPerMm2` and `PartitionStrategy.Resolve` maps the boundary area onto the run's site count, pitch, separation, and merge floor. Site count clamps between the floor and the ceiling, and pitch is then the square root of the REALIZED cell area — so a clamped run separates and merges against the spacing it actually achieved, where a stored nominal pitch over-rejected a floored run and under-rejected a capped one.
- Law: the candidate draw and the acceptance weight read ONE site box — the depth-inflated boundary bound. A weight evaluated against the flat bound while the draw ran against the inflated one graded every volumetric field along the wrong axis, and the shared box is what keeps the sift's replay a pure function of the strategy.
- Law: `PartitionCell.LloydResidualMm` derives relaxation convergence from the retained site-centroid pair, and `OnBoundary` separates border cells from interior cells so lead-in and stipple-density consumers read the distinction rather than re-deriving it from geometry.
- Law: the planar decomposition and the volumetric one answer different planes and stay UNJOINED. The planar diagram relaxes its seeds and re-indexes across its merge pass, so a `PartitionCell` ordinal names a survivor; `PartitionSolid.Site` is the ordinal its seed entered the accepted set on, which the kernel dual preserves because it neither relaxes nor merges. A per-cell volumetric column would key one decomposition on an ordinal the other cannot honour, so the solid rows retain whole beside the cells rather than folding into them.
- Entry: `Partition.Seed(PartitionRequest)` owns every point-site modality and preserves the frozen `Seed` operation name.
- Auto: the `SiteDistribution` row draws a candidate cloud off `Deterministic.Unit` into the site box, fixed anchors seed the three-axis separation index first, footprint coverage inside the depth band with an index-addressed density draw admits sites under a bounded fold that stops at the resolved count, one `PolygonOp.Cells` request closes, relaxes, merges, and boundary-clips the planar diagram over the deduplicated site footprints — every disconnected region of a clipped cell contributes to census area — and a depth-bearing projection additionally folds the accepted sites through `VectorIntent.Voronoi` for the 3D complex.
- Auto: spanning topology mints ONE undirected container from the adjacency links and ONE directed forest from the Kruskal edges, both through the edge-set projection rather than a hand-populated second graph; a single `BreadthFirstSearchAlgorithm` per component carries two attached distance recorders, so hop depth and path length come from ONE walk instead of a per-vertex path re-enumeration.
- Receipt: `PartitionReceipt` carries every clipped region per generating site, both the generating site and its clipped centroid, border membership, perimeter, adjacency links, the minimum spanning forest, breadth-first tour, per-cell traversal evidence, stroke split, the `PartitionSolid` rows, candidate/requested/surviving/merge census, the nearest boundary-anchor cell, and the density closure. `PartitionDensity` closes the inverse the strategy opened — target areal density and cell area against the density and mean cell area the retained cells realized, beside the walk's Lloyd residual — so a consumer reads whether the field it asked for is the field it got. `PartitionSolid` reads the kernel `CloudVoronoiCell` column for column — bound-gated `Option` volume, centroid, and extent beside the natural-neighbour sites — so an unbounded cell publishes absence rather than a zero no fold measured, and `Bounded` is the measured subset a lattice or support-cell consumer reads. The volumetric fold projects the whole `CloudVoronoiResult` and refuses on a `Rejected` receipt, so an empty solid set means the caller asked for no depth and never that a degenerate dual returned nothing.
- Packages: `Rasm` supplies the `Deterministic` draw lanes, the `VectorCloud`/`VectorIntent.Voronoi` rail carrying the 3D dual, its per-cell measures, and the `CloudVoronoiResult`/`CloudVoronoiReceipt`/`CloudFoldStatus` outcome evidence the solid fold gates on, and, through `PolygonOp.Cells`, the bounded planar Voronoi diagram, its Lloyd relaxation, its merge fold, and the nearest-site index; the `Geometry2D` owner supplies measure, Boolean, and open-clip; QuikGraph supplies the edge-set container projections, Kruskal spanning, component labels, the breadth-first walk, and its distance recorders; LanguageExt supplies applicative admission, the bounded fold, traversal, the `HashMap` separation index, and the `Fin` rail.
- Growth: a new site-cloud law is one `SiteDistribution` row carrying its own lane fold; a new grading law is one `DensityField` row; a new egress modality is one `PartitionProjection` case; a new ordering law consumes the retained graph without changing tessellation; a new per-cell planar measure is one `PartitionCell` column read off the retained `SiteCell`, and a new volumetric measure is one `PartitionSolid` column read off the retained `CloudVoronoiCell`.
- Boundary: QuikGraph construction and the observed walk are the one statement-bearing foreign-mutation seam; aggregate admission, domain computation, and egress remain expression-shaped. Diagrams are never minted here — a page-local tessellator, relaxation loop, draw stream, or volumetric dual is the deleted form, the planar complex routing `PolygonOp.Cells` and the 3D complex `VectorIntent.Voronoi`. Every `PolygonAlgebra.Apply` call carries its `Op` key, so a trace-shape refusal names the calling operation instead of a hand-written axis literal. No absence rides a sentinel count or an infinite length: an unreachable cell leaves the tour and the closure census refuses.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Each row IS its candidate law: a lane-addressed draw over the admitted box, so a cloud replays from
// (seed, index) alone with no stream position to preserve and no fourth draw owner beside the kernel's.
// Gaussian folds two unit lanes through Box-Muller and clamps into the box, keeping the row total.
[SmartEnum<string>]
public sealed partial class SiteDistribution {
    public static readonly SiteDistribution Uniform = new("uniform", static (box, seed, index) => new Point3d(
        box.Min.X + (Deterministic.Unit(lanes: [index, 0L], seed: seed) * box.Diagonal.X),
        box.Min.Y + (Deterministic.Unit(lanes: [index, 1L], seed: seed) * box.Diagonal.Y),
        box.Min.Z));

    public static readonly SiteDistribution Gaussian = new("gaussian", static (box, seed, index) => {
        double radius = Math.Sqrt(-2.0 * Math.Log(double.Max(Deterministic.Unit(lanes: [index, 0L], seed: seed), 1e-12)));
        double angle = 2.0 * Math.PI * Deterministic.Unit(lanes: [index, 1L], seed: seed);
        return new Point3d(
            double.Clamp(box.Center.X + (radius * Math.Cos(angle) * box.Diagonal.X / 6.0), box.Min.X, box.Max.X),
            double.Clamp(box.Center.Y + (radius * Math.Sin(angle) * box.Diagonal.Y / 6.0), box.Min.Y, box.Max.Y),
            box.Min.Z);
    });

    // The planar rows rest every site on the box floor; this is the row that spends the third axis, and it is the
    // one a volumetric projection draws through. Depth rides lane 3 rather than lane 2 because the acceptance draw
    // already owns lane 2, so appending leaves every planar strategy's accepted field byte-identical. Over a flat
    // box the row degenerates onto the floor exactly as `Uniform` does, so the distribution and the projection stay
    // orthogonal: a planar projection over this row is the uniform field, never a fault.
    public static readonly SiteDistribution Volumetric = new("volumetric", static (box, seed, index) => new Point3d(
        box.Min.X + (Deterministic.Unit(lanes: [index, 0L], seed: seed) * box.Diagonal.X),
        box.Min.Y + (Deterministic.Unit(lanes: [index, 1L], seed: seed) * box.Diagonal.Y),
        box.Min.Z + (Deterministic.Unit(lanes: [index, 3L], seed: seed) * box.Diagonal.Z)));

    [UseDelegateFromConstructor] internal partial Point3d Draw(BoundingBox box, long seed, long index);
}

// Density is a WEIGHT FIELD over the admitted box, never a caller callback. A strategy row is static declaration
// data, so an `Option<Func<Point3d, double>>` slot no row could fill is what left the index-addressed rejection
// draw, the acceptance column, and the `[0,1]` gate dead for every producer. Each row states its own normalized
// weight, so a graded field declares its grading, a uniform field declares `Flat`, and the acceptance test reads a
// real value on both — and a new grading law is one row with every consumer untouched.
[SmartEnum<string>]
public sealed partial class DensityField {
    public static readonly DensityField Flat = new("flat", static (_, _) => 1.0);
    public static readonly DensityField Centred = new("centred", static (box, point) =>
        double.Clamp(1.0 - Radial(box, point), 0.0, 1.0));
    public static readonly DensityField Perimeter = new("perimeter", static (box, point) =>
        double.Clamp(Radial(box, point), 0.0, 1.0));
    public static readonly DensityField Graded = new("graded", static (box, point) => box.Diagonal.X > 0.0
        ? double.Clamp((point.X - box.Min.X) / box.Diagonal.X, 0.0, 1.0)
        : 1.0);

    [UseDelegateFromConstructor]
    public partial double Weight(BoundingBox box, Point3d point);

    // Normalized distance from the box centre to its corner, so every row reads one bounded axis.
    private static double Radial(BoundingBox box, Point3d point) =>
        box.Diagonal.Length > 0.0 ? 2.0 * box.Center.DistanceTo(point) / box.Diagonal.Length : 0.0;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class SamplingField {
    public SiteDistribution Source { get; }
    public int Seed { get; }
    public DensityField Density { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref SiteDistribution source,
        ref int seed,
        ref DensityField density) =>
        validationError = source is not null && density is not null
            ? null
            : new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "partition-sampling-field");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PartitionProjection {
    private PartitionProjection() { }

    public sealed record Regions : PartitionProjection;
    public sealed record Classify(Seq<Edge3> Strokes) : PartitionProjection;
    public sealed record Volumetric(double DepthMm) : PartitionProjection;

    // Depth is the ONE column the site box, the anchor gate, and the sift containment test all read, so the planar
    // cases answer zero and every one of those folds stays branch-free — a zero-depth band collapses onto the
    // boundary plane exactly as the coplanar gate spelled it before this case existed.
    public double DepthMm => Switch(
        regions: static _ => 0.0,
        classify: static _ => 0.0,
        volumetric: static row => row.DepthMm);
}

// The boundary-resolved run geometry: how many sites this area admits at the declared density, the cell area that
// count realizes, the pitch that area implies, the separation the sift enforces, and the merge floor the diagram
// coalesces under. Every one of the four derives from the realized cell area, so a floored or capped count carries
// spacing that matches the field it actually produced.
public readonly record struct PartitionField(
    int Sites,
    double CellAreaMm2,
    double PitchMm,
    double SeparationMm,
    double MergeFloorMm2);

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class PartitionStrategy {
    public string Key { get; }
    public SamplingField Sampling { get; }
    public Arr<Point3d> Anchors { get; }

    // Target sites per square millimetre — the axis a stippling, pocketing, or plotting consumer actually states.
    // Each preset declares it as the reciprocal of its nominal cell pitch, so the number a reader recognizes stays
    // visible while the value the algebra consumes is the density the boundary area multiplies against.
    public double SiteDensityPerMm2 { get; }
    public int SiteFloor { get; }
    public int SiteCeiling { get; }
    public int RelaxIterations { get; }
    public double RelaxStrength { get; }
    public double MinimumSeparationRatio { get; }
    public int AttemptFactor { get; }
    public double MergeAreaRatio { get; }

    public static readonly PartitionStrategy PocketRegion = Create(
        key: "pocket-region",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 104_729, density: DensityField.Flat),
        anchors: [],
        siteDensityPerMm2: 1.0 / (12.0 * 12.0),
        siteFloor: 9,
        siteCeiling: 4_096,
        relaxIterations: 4,
        relaxStrength: 1.0,
        minimumSeparationRatio: 0.2,
        attemptFactor: 24,
        mergeAreaRatio: 0.2);

    public static readonly PartitionStrategy Stipple = Create(
        key: "stipple",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 130_363, density: DensityField.Centred),
        anchors: [],
        siteDensityPerMm2: 1.0 / (3.0 * 3.0),
        siteFloor: 32,
        siteCeiling: 16_384,
        relaxIterations: 6,
        relaxStrength: 1.0,
        minimumSeparationRatio: 0.35,
        attemptFactor: 32,
        mergeAreaRatio: 0.05);

    public static readonly PartitionStrategy EngraveEvenSpacing = Create(
        key: "engrave-even-spacing",
        sampling: SamplingField.Create(SiteDistribution.Gaussian, seed: 155_921, density: DensityField.Flat),
        anchors: [],
        siteDensityPerMm2: 1.0 / (5.0 * 5.0),
        siteFloor: 16,
        siteCeiling: 8_192,
        relaxIterations: 4,
        relaxStrength: 0.75,
        minimumSeparationRatio: 0.3,
        attemptFactor: 24,
        mergeAreaRatio: 0.1);

    public static readonly PartitionStrategy PenPlot = Create(
        key: "pen-plot",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 196_613, density: DensityField.Flat),
        anchors: [],
        siteDensityPerMm2: 1.0 / (8.0 * 8.0),
        siteFloor: 12,
        siteCeiling: 4_096,
        relaxIterations: 2,
        relaxStrength: 0.5,
        minimumSeparationRatio: 0.25,
        attemptFactor: 16,
        mergeAreaRatio: 0.15);

    // The density map: area in, run geometry out. Pitch is the root of the REALIZED cell area rather than a stored
    // constant, so separation and the merge floor follow a clamped count instead of the count that was requested.
    public PartitionField Resolve(double boundaryAreaMm2) {
        int sites = checked((int)Math.Clamp(
            Math.Ceiling(boundaryAreaMm2 * SiteDensityPerMm2),
            Math.Max(SiteFloor, Anchors.Count),
            SiteCeiling));
        double cell = boundaryAreaMm2 / sites;
        double pitch = Math.Sqrt(cell);
        return new PartitionField(sites, cell, pitch, pitch * MinimumSeparationRatio, cell * MergeAreaRatio);
    }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref string key,
        ref SamplingField sampling,
        ref Arr<Point3d> anchors,
        ref double siteDensityPerMm2,
        ref int siteFloor,
        ref int siteCeiling,
        ref int relaxIterations,
        ref double relaxStrength,
        ref double minimumSeparationRatio,
        ref int attemptFactor,
        ref double mergeAreaRatio) {
        K<Validation<Error>, Unit> numeric = (
            Gate(Witness.Keyed(key), "key"),
            Gate(Witness.Positive(siteDensityPerMm2), "site-density"),
            Gate(siteFloor >= 3 && siteCeiling >= siteFloor, "site-count"),
            Gate(relaxIterations >= 0 && relaxStrength is >= 0.0 and <= 1.0 && double.IsFinite(relaxStrength), "relaxation"),
            Gate(minimumSeparationRatio >= 0.0 && double.IsFinite(minimumSeparationRatio), "separation"),
            Gate(attemptFactor >= 1 && siteCeiling <= Array.MaxLength / attemptFactor, "attempt-capacity"),
            Gate(mergeAreaRatio is >= 0.0 and <= 1.0 && double.IsFinite(mergeAreaRatio), "merge-ratio"))
            .Apply(static (_, _, _, _, _, _, _) => unit);
        Seq<Point3d> sites = toSeq(anchors);
        K<Validation<Error>, Unit> anchorsValid = (
            Gate(sites.ForAll(static point => point.IsValid), "anchors-finite"),
            Gate(sites.Count <= siteCeiling, "anchors-capacity"))
            .Apply(static (_, _) => unit);
        validationError = (numeric, anchorsValid)
            .Apply(static (_, _) => unit)
            .As()
            .Match<FabricationFault?>(
                Fail: static error => new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message),
                Succ: static _ => null);
    }

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"partition-strategy:{axis}"));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class PartitionRequest {
    public PartitionStrategy Strategy { get; }
    public Loop Boundary { get; }
    public PartitionProjection Projection { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref PartitionStrategy strategy,
        ref Loop boundary,
        ref PartitionProjection projection) {
        // Anchor separation reads the RESOLVED field, so an anchor set is judged against the spacing the boundary
        // area actually admits rather than a nominal pitch the run may never reach.
        double separation = Partition.MeasuredArea(boundary)
            .Map(area => strategy.Resolve(Math.Abs(area)).SeparationMm)
            .IfFail(0.0);
        Seq<Point3d> anchors = strategy.Anchors.ToSeq();
        validationError = (
            Gate(boundary.Closed && boundary.Count >= 3, "boundary"),
            Gate(anchors.ForAll(point => Partition.Covers(boundary, point, projection.DepthMm)), "anchors-boundary"),
            Gate(anchors.Map(static (point, index) => (Point: point, Index: index)).ForAll(row =>
                anchors.Take(row.Index).ForAll(prior => prior.DistanceTo(row.Point) >= separation)), "anchors-separated"),
            Gate(projection.Switch(
                regions: static _ => true,
                classify: static request => request.Strokes.ForAll(static edge => edge.A.IsValid && edge.B.IsValid),
                volumetric: static request => Witness.Positive(request.DepthMm)), "projection"))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .Match<FabricationFault?>(
                Fail: static error => new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message),
                Succ: static _ => null);
    }

    public static Fin<PartitionRequest> Admit(
        PartitionStrategy strategy,
        Loop boundary,
        PartitionProjection projection) =>
        Validate(strategy, boundary, projection, out PartitionRequest request).Admitted(request);

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"partition-request:{axis}"));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record PartitionLink(
    int Source,
    int Target,
    Point3d Start,
    Point3d End,
    Point3d Mid,
    double LengthMm);

public sealed record PartitionCell(
    int Index,
    Seq<Loop> Regions,
    Point3d Site,
    Point3d Centroid,
    double AreaMm2,
    double PerimeterMm,
    bool OnBoundary,
    Seq<int> Neighbours) {
    public double LloydResidualMm => Site.DistanceTo(Centroid);
}

// The walk's own outputs, published rather than consumed and dropped: which spanning component the cell fell in,
// how many hops the breadth-first walk reached it in, and the path length those hops accumulated.
public readonly record struct PartitionVisit(int Cell, int Component, int Depth, double LengthMm);

// The inverse the strategy's density opened. Target against realized, so a consumer reads whether the clamp, the
// sift, or the merge moved the field away from the density it asked for instead of inferring it from cell counts.
public readonly record struct PartitionDensity(
    double TargetPerMm2,
    double AchievedPerMm2,
    double TargetCellAreaMm2,
    double MeanCellAreaMm2,
    double LloydResidualMm);

public sealed record PartitionReceipt(
    PartitionStrategy Strategy,
    Loop Boundary,
    Seq<PartitionCell> Cells,
    Seq<PartitionLink> Links,
    Seq<PartitionLink> Spanning,
    Seq<int> Tour,
    Seq<PartitionVisit> Traversal,
    Seq<Edge3> Inside,
    Seq<Edge3> Outside,
    Seq<PartitionSolid> Solids,
    PartitionDensity Density,
    int CandidateSites,
    int RequestedSites,
    int ProviderSites,
    int MergedSites,
    int AnchorCell) {
    public Seq<Loop> Regions => Tour.Bind(index => Cells[index].Regions);

    public Seq<PartitionCell> Border => Cells.Filter(static cell => cell.OnBoundary);

    public Seq<PartitionSolid> Bounded => Solids.Filter(static solid => solid.VolumeMm3.IsSome);
}

// The volumetric decomposition is the kernel `CloudVoronoiCell` read WHOLE — site ordinal into the accepted set,
// generating seed, bound species, and the three measures the kernel publishes as `Option` because an unbounded or
// degenerate cell measured none of them. Re-deriving a volume, a centroid, or an adjacency here would mint a second
// dual beside the one the page already forbids, and a zero on an unbounded cell would spell a measurement no fold
// took. Neighbours are the kernel's natural-neighbour sites, so a support-cell or fracture consumer reads its
// stolen-volume neighbourhood off the retained row instead of re-tessellating.
public sealed record PartitionSolid(
    int Site,
    Point3d Seed,
    Seq<int> Neighbours,
    Option<double> VolumeMm3,
    Option<Point3d> Centroid,
    Option<double> ExtentMm);

file sealed record SiteDiagram(
    CellReceipt Diagram,
    PartitionStrategy Strategy,
    PartitionField Field,
    int Candidates,
    int Admitted,
    Loop Boundary,
    int Anchor,
    Seq<PartitionSolid> Solids);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Partition {
    public static Fin<PartitionReceipt> Seed(PartitionRequest request) =>
        from boundaryArea in MeasuredArea(request.Boundary).Map(Math.Abs)
        let field = request.Strategy.Resolve(boundaryArea)
        from diagram in Tessellate(request, field)
        from cells in LowerCells(diagram.Diagram, request.Boundary)
        let links = LowerLinks(diagram.Diagram)
        from topology in Topology(cells, links, diagram.Anchor)
        from _ in Census(diagram, cells, boundaryArea)
        let regions = topology.Tour.Bind(index => cells[index].Regions)
        from split in request.Projection.Switch(
            regions: static _ => Fin.Succ((Inside: Seq<Edge3>(), Outside: Seq<Edge3>())),
            classify: projection => Classify(projection.Strokes, regions),
            volumetric: static _ => Fin.Succ((Inside: Seq<Edge3>(), Outside: Seq<Edge3>())))
        select new PartitionReceipt(
            request.Strategy,
            request.Boundary,
            cells,
            links,
            topology.Spanning,
            topology.Tour,
            topology.Traversal,
            split.Inside,
            split.Outside,
            diagram.Solids,
            Closure(request.Strategy, field, cells, boundaryArea),
            diagram.Candidates,
            field.Sites,
            diagram.Diagram.Cells.Count,
            diagram.Admitted - diagram.Diagram.Cells.Count,
            diagram.Anchor);

    // One request closes the diagram: the sifted seed field, the admitted boundary ring, and the relaxation and
    // merge rows the resolved field already carries. Nothing here mints a tessellator, a relaxation loop, a
    // nearest-site index, or a volumetric dual — the algebra owner lowers the planar three onto the kernel dual and
    // hands back the clipped rings whole, and the kernel `VectorIntent.Voronoi` rail owns the 3D complex.
    //
    // The planar leg tessellates the accepted sites' FOOTPRINTS, deduplicated at the admitted grain: a volumetric
    // field separates in depth, so two sites may share a footprint, and a duplicate seed is a degenerate point set
    // to the constrained substrate. Under a planar projection every site already rests on the plane and the
    // deduplication is the identity, so the two lanes run one expression.
    private static Fin<SiteDiagram> Tessellate(PartitionRequest request, PartitionField field) =>
        from candidates in Candidates(request, field)
        from accepted in Accept(request, field, candidates)
        from _ in accepted.Count == field.Sites
            ? Fin.Succ(unit)
            : Degenerate<Unit>(request.Strategy, accepted.Count)
        let policy = SitePolicy.Create(
            relaxations: request.Strategy.RelaxIterations,
            relaxationStrength: request.Strategy.RelaxStrength,
            merge: field.MergeFloorMm2 > 0.0
                ? Some(SiteMerge.Create(minimumArea: field.MergeFloorMm2, minimumSeparation: 0.0))
                : None)
        let footprints = accepted
            .Map(site => new Point3d(site.X, site.Y, request.Boundary.Plane))
            .DistinctBy(site => (Math.Round(site.X / request.Boundary.Tolerance.Absolute.Value),
                Math.Round(site.Y / request.Boundary.Tolerance.Absolute.Value)))
        from trace in PolygonAlgebra.Apply(
            new PolygonOp.Cells(footprints.ToArr(), request.Boundary, policy),
            Op.Of(name: nameof(Tessellate)))
        from diagram in trace is PolygonTrace.Celled celled
            ? Fin.Succ(celled.Result)
            : Degenerate<CellReceipt>(request.Strategy, accepted.Count)
        from anchor in diagram.Locate(request.Boundary.At(0), Op.Of(name: nameof(Tessellate)))
        from solids in Solids(request, accepted)
        select new SiteDiagram(
            diagram, request.Strategy, field, candidates.Count, accepted.Count, request.Boundary, anchor, solids);

    // The 3D complex is the kernel's whole: a cluster cloud over the accepted sites, `VectorIntent.Voronoi` the one
    // public rail, and the projected `CloudVoronoiCell` rows read column for column. `CloudVoronoiCell.Site` is the
    // ordinal the seed entered on, so the retained rows address the accepted set directly — which is exactly why
    // they are NOT joined to `PartitionCell`: the planar diagram relaxes its seeds and re-indexes across its merge
    // pass, so a planar cell ordinal names a survivor, never an accepted site. The two decompositions answer
    // different planes and stay unjoined rather than sharing a key one of them cannot honour.
    //
    // The projection takes the WHOLE `CloudVoronoiResult` — the rail's identity row, so cells and receipt arrive
    // from ONE fold and no second dual runs — because the cells row ALONE cannot tell a refusal from an absence:
    // a degenerate site set funnels through the kernel's own `ConvexHullGenerationException` seam into a
    // `Rejected` receipt carrying EMPTY cells, which read here as a `Seq<PartitionSolid>` byte-identical to the
    // no-depth arm's. Gating on `Status` is the volumetric twin of the planar `Census` closure: a rejected 3D fold
    // names its cause where a caller can act on it instead of publishing zero solids for a decomposition that
    // never happened, and `Rejection` carries the package's own outcome so the refusal names the degeneracy.
    private static Fin<Seq<PartitionSolid>> Solids(PartitionRequest request, Seq<Point3d> accepted) =>
        request.Projection.DepthMm <= 0.0
            ? Fin.Succ(Seq<PartitionSolid>())
            : from cloud in VectorCloud.Cluster(accepted, request.Boundary.Tolerance, key: Op.Of(name: nameof(Solids)))
              from intent in VectorIntent.Voronoi(cloud, key: Op.Of(name: nameof(Solids)))
              from dual in intent.Project<CloudVoronoiResult>(request.Boundary.Tolerance, Op.Of(name: nameof(Solids)))
              from measured in dual.Receipt.Status.Equals(CloudFoldStatus.Completed)
                  ? Fin.Succ(dual.Cells)
                  : Degenerate<Seq<CloudVoronoiCell>>(request.Strategy, accepted.Count)
              select measured.Map(static cell => new PartitionSolid(
                  cell.Site, cell.Seed, toSeq(cell.Neighbors), cell.Volume, cell.Centroid, cell.Extent));

    // Cloud draws are lane-addressed, so candidate i is a pure function of (seed, i) and the box — a rejected
    // candidate never shifts the ones after it, which a stream-positioned provider draw could not promise.
    private static Fin<Seq<Point3d>> Candidates(PartitionRequest request, PartitionField field) {
        BoundingBox box = Box(request.Boundary, request.Projection.DepthMm);
        SamplingField sampling = request.Strategy.Sampling;
        return Try.lift(() => checked(field.Sites * request.Strategy.AttemptFactor))
            .Run()
            .MapFail(_ => new FabricationFault.PartitionDegenerate(Subject(request.Strategy), field.Sites).ToError())
            .Map(attempts => Range(0, attempts).ToSeq().Map(index => sampling.Source.Draw(box, sampling.Seed, (long)index)).ToSeq())
            .As()
            .ToFin();
    }

    // The site box is the boundary footprint inflated by the projection's depth; a planar projection carries zero
    // depth, so it IS the flat bound the planar distribution rows already read their floor off. ONE box serves the
    // draw and the acceptance weight — a weight graded against the flat bound under a depth-bearing draw asked the
    // field a question about a box no candidate came from.
    internal static BoundingBox Box(Loop boundary, double depthMm) {
        BoundingBox flat = boundary.Bound();
        return depthMm > 0.0
            ? new BoundingBox(flat.Min, new Point3d(flat.Max.X, flat.Max.Y, boundary.Plane + depthMm))
            : flat;
    }

    // A site admits by its FOOTPRINT against the boundary ring and its ELEVATION against the projection's depth
    // band. A planar projection carries zero depth, so the band collapses onto the boundary plane and this is
    // exactly the coplanar containment the page held before the volumetric case existed — one predicate serving
    // anchor admission and the sift, so the two can never drift apart.
    internal static bool Covers(Loop boundary, Point3d site, double depthMm) {
        double slack = boundary.Tolerance.Absolute.Value;
        return site.IsValid
            && site.Z >= boundary.Plane - slack
            && site.Z <= boundary.Plane + depthMm + slack
            && boundary.Covers(new Point3d(site.X, site.Y, boundary.Plane));
    }

    private static Fin<Seq<Point3d>> Accept(PartitionRequest request, PartitionField field, Seq<Point3d> candidates) =>
        // Rejection draws ride their own lane, so they neither consume nor perturb the cloud lanes: candidate i's
        // draw is a pure function of (seed, i) and its acceptance a pure function of the field row and the box, so
        // the whole sift replays from the strategy alone.
        Try.lift(() => Box(request.Boundary, request.Projection.DepthMm) is var box
                && candidates.Map((point, index) => (
                    Point: point,
                    Draw: Deterministic.Unit(lanes: [(long)index, 2L], seed: request.Strategy.Sampling.Seed),
                    Acceptance: request.Strategy.Sampling.Density.Weight(box, point))).ToArr())
            .Run()
            .MapFail(_ => new FabricationFault.PartitionDegenerate(Subject(request.Strategy), candidates.Count).ToError())
            .Bind(rows => rows.ForAll(static row => double.IsFinite(row.Acceptance) && row.Acceptance is >= 0.0 and <= 1.0)
                ? Fin.Succ(Sift(request, field, rows))
                : Degenerate<Seq<Point3d>>(request.Strategy, rows.Count));

    // The fold STOPS at the resolved count: a strategy whose attempt budget is twenty-four times its site count
    // walked the whole budget to place the last site it needed, so the bounded fold is the batch's own bound.
    private static Seq<Point3d> Sift(
        PartitionRequest request,
        PartitionField field,
        Arr<(Point3d Point, double Draw, double Acceptance)> rows) =>
        rows.FoldWhile(
            (Index: request.Strategy.Anchors.ToSeq().Fold(HashMap<(int X, int Y, int Z), Seq<Point3d>>.Empty,
                 (index, point) => Place(index, point, field.SeparationMm)),
             Accepted: request.Strategy.Anchors.ToSeq()),
            (state, row) => !Covers(request.Boundary, row.Point, request.Projection.DepthMm)
                || row.Draw > row.Acceptance
                || Crowded(state.Index, row.Point, field.SeparationMm)
                    ? state
                    : (Place(state.Index, row.Point, field.SeparationMm), state.Accepted.Add(row.Point)),
            pair => pair.Item1.Accepted.Count < field.Sites)
            .Accepted;

    // The separation index keys on all three axes, so a volumetric field separates in depth on the same structure
    // the planar field separates in plane. A planar field puts every site in one Z bucket, so the generalization
    // costs it two constant-miss shift planes rather than a second index shape.
    private static (int X, int Y, int Z) Bucket(Point3d point, double separation, int shiftX = 0, int shiftY = 0, int shiftZ = 0) =>
        separation > 0.0
            ? ((int)Math.Floor(point.X / separation) + shiftX, (int)Math.Floor(point.Y / separation) + shiftY,
                (int)Math.Floor(point.Z / separation) + shiftZ)
            : (shiftX, shiftY, shiftZ);

    private static HashMap<(int X, int Y, int Z), Seq<Point3d>> Place(
        HashMap<(int X, int Y, int Z), Seq<Point3d>> index,
        Point3d point,
        double separation) =>
        separation > 0.0
            ? index.AddOrUpdate(Bucket(point, separation), held => held.Add(point), Seq(point))
            : index;

    private static bool Crowded(
        HashMap<(int X, int Y, int Z), Seq<Point3d>> index,
        Point3d point,
        double separation) =>
        separation > 0.0 && Neighbourhood.Exists(shift =>
            index.Find(Bucket(point, separation, shift.X, shift.Y, shift.Z))
                .Map(bucket => bucket.Exists(placed => placed.DistanceTo(point) < separation))
                .IfNone(false));

    private static readonly Arr<(int X, int Y, int Z)> Neighbourhood = Range(-1, 3)
        .Bind(x => Range(-1, 3).ToSeq().Bind(y => Range(-1, 3).ToSeq().Map(z => (X: x, Y: y, Z: z)))).ToArr();

    // Diagram clipping seeds from the boundary ring through half-planes, which is exact on a convex ring and
    // conservative on a re-entrant one, so the exact Boolean still runs here — and it is also what splits a cell a
    // concave boundary severs into its disconnected regions. A cell whose clipped area differs from its ring area
    // touches the boundary, which is the border verdict lead-in and stipple-density consumers read.
    private static Fin<Seq<PartitionCell>> LowerCells(CellReceipt diagram, Loop boundary) =>
        diagram.Cells.ToSeq().Traverse(cell =>
            from measured in Intersect(cell.Ring, boundary).Bind(pieces => pieces.Traverse(piece => Measure(Seq(piece))
                .Map(value => (Region: piece, Measure: value))).As())
            let regions = measured.Filter(static piece => piece.Region.Count >= 3 && piece.Measure.FilledArea > 0.0)
            from _ in regions.IsEmpty
                ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, cell.Site, "partition:empty-cell").ToError())
                : Fin.Succ(unit)
            let area = regions.Sum(static piece => piece.Measure.FilledArea)
            select new PartitionCell(
                cell.Site,
                regions.Map(static piece => piece.Region),
                cell.Seed,
                new Point3d(
                    regions.Sum(piece => piece.Measure.Centroid.X * piece.Measure.FilledArea) / area,
                    regions.Sum(piece => piece.Measure.Centroid.Y * piece.Measure.FilledArea) / area,
                    boundary.Plane),
                area,
                regions.Sum(static piece => piece.Measure.BoundaryLength),
                Math.Abs(cell.Area - area) > boundary.Tolerance.Absolute.Value,
                diagram.Adjacency
                    .Filter(edge => edge.A == cell.Site || edge.B == cell.Site)
                    .Map(edge => edge.A == cell.Site ? edge.B : edge.A)
                    .ToSeq()
                    .Distinct()))
        .As();

    internal static Fin<double> MeasuredArea(Loop boundary) =>
        Measure(Seq(boundary)).Map(static measured => measured.FilledArea);

    private static Fin<PolygonMeasure> Measure(Seq<Loop> paths) =>
        PolygonAlgebra.Apply(new PolygonOp.Measure(paths, PolygonFill.NonZero), Op.Of())
            .Bind(static trace => trace is PolygonTrace.Measured measured
                ? Fin.Succ(measured.Result)
                : Fin.Fail<PolygonMeasure>(Op.Of(name: nameof(Measure)).InvalidResult()));

    private static Fin<Seq<Loop>> Intersect(Loop subject, Loop clip) =>
        PolygonAlgebra.Apply(new PolygonOp.Boolean(Seq(subject), Seq(clip), BooleanOp.Intersection, PolygonFill.NonZero), Op.Of())
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Filter(static node => !node.IsHole).Map(static node => node.Boundary))
                : Fin.Fail<Seq<Loop>>(Op.Of(name: nameof(Intersect)).InvalidResult()));

    private static Fin<(Seq<Edge3> Inside, Seq<Edge3> Outside)> Classify(Seq<Edge3> subject, Seq<Loop> clip) =>
        PolygonAlgebra.Apply(new PolygonOp.ClipOpen(Seq(subject), clip, PolygonFill.NonZero), Op.Of())
            .Bind(static trace => trace is PolygonTrace.SplitRuns split
                ? Fin.Succ((split.Inside.Bind(static run => run), split.Outside.Bind(static run => run)))
                : Fin.Fail<(Seq<Edge3>, Seq<Edge3>)>(Op.Of(name: nameof(Classify)).InvalidResult()));

    // Adjacency already carries its shared dual segment, so a link is a projection, never a second derivation.
    private static Seq<PartitionLink> LowerLinks(CellReceipt diagram) =>
        diagram.Adjacency.ToSeq().Map(static edge => new PartitionLink(
            edge.A, edge.B, edge.Start, edge.End, edge.Mid, edge.Length));

    // Both containers project from their own edge set: the undirected one Kruskal spans, and the directed forest
    // the rooted walk descends, each carrying the full vertex range so an isolated cell stays a component of one.
    private static Fin<(Seq<PartitionLink> Spanning, Seq<int> Tour, Seq<PartitionVisit> Traversal)> Topology(
        Seq<PartitionCell> cells,
        Seq<PartitionLink> links,
        int anchor) {
        UndirectedGraph<int, TaggedEdge<int, PartitionLink>> graph = links
            .Map(static link => new TaggedEdge<int, PartitionLink>(link.Source, link.Target, link))
            .ToUndirectedGraph<int, TaggedEdge<int, PartitionLink>>(allowParallelEdges: false);
        graph.AddVertexRange(Range(0, cells.Count));
        Seq<TaggedEdge<int, PartitionLink>> spanning = toSeq(graph.MinimumSpanningTreeKruskal(static edge => edge.Tag.LengthMm));
        BidirectionalGraph<int, TaggedEdge<int, PartitionLink>> forest = spanning
            .Bind(static edge => Seq(edge, new TaggedEdge<int, PartitionLink>(edge.Target, edge.Source, edge.Tag)))
            .ToBidirectionalGraph<int, TaggedEdge<int, PartitionLink>>(allowParallelEdges: false);
        forest.AddVertexRange(Range(0, cells.Count));
        Dictionary<int, int> labels = [];
        _ = forest.WeaklyConnectedComponents(labels);
        Seq<PartitionVisit> traversal = toSeq(labels.GroupBy(static pair => pair.Value)
            .OrderBy(group => group.Any(pair => pair.Key == anchor) ? 0 : 1)
            .ThenBy(static group => group.Min(static pair => pair.Key)))
            .Bind(group => Breadth(
                forest,
                group.Select(static pair => pair.Key).Contains(anchor) ? anchor : group.Min(static pair => pair.Key),
                group.Key,
                toSeq(group.Select(static pair => pair.Key))));
        return traversal.Count == cells.Count
            ? Fin.Succ((spanning.Map(static edge => edge.Tag), traversal.Map(static row => row.Cell), traversal))
            : Fin.Fail<(Seq<PartitionLink>, Seq<int>, Seq<PartitionVisit>)>(
                new GeometryFault.DegenerateInput(Kind.Curve, None, "partition:topology").ToError());
    }

    // ONE walk, two recorders: hop depth rides unit weights and path length rides the link measure, so the tour
    // order, the published depth, and the published length all come from the same descent. A vertex the walk never
    // reached leaves the tour rather than entering it under a sentinel depth, and the closure census refuses.
    private static Seq<PartitionVisit> Breadth(
        BidirectionalGraph<int, TaggedEdge<int, PartitionLink>> forest,
        int root,
        int component,
        Seq<int> vertices) {
        BreadthFirstSearchAlgorithm<int, TaggedEdge<int, PartitionLink>> search = new(forest);
        VertexDistanceRecorderObserver<int, TaggedEdge<int, PartitionLink>> hops = new(static _ => 1.0);
        VertexDistanceRecorderObserver<int, TaggedEdge<int, PartitionLink>> spans = new(static edge => edge.Tag.LengthMm);
        using (hops.Attach(search))
        using (spans.Attach(search))
            search.Compute(root);
        HashMap<int, double> depth = toHashMap(hops.Distances.Select(static row => (row.Key, row.Value)));
        HashMap<int, double> length = toHashMap(spans.Distances.Select(static row => (row.Key, row.Value)));
        return toSeq((Seq(new PartitionVisit(root, component, 0, 0.0))
                + vertices.Filter(vertex => vertex != root).Choose(vertex =>
                    (depth.Find(vertex), length.Find(vertex))
                        .Apply((hop, span) => new PartitionVisit(vertex, component, (int)hop, span))
                        .As()))
            .OrderBy(static row => row.Depth)
            .ThenBy(static row => row.LengthMm)
            .ThenBy(static row => row.Cell));
    }

    // The closure the density opened: what the strategy asked for against what the retained cells realized.
    private static PartitionDensity Closure(
        PartitionStrategy strategy,
        PartitionField field,
        Seq<PartitionCell> cells,
        double boundaryArea) =>
        new(strategy.SiteDensityPerMm2,
            cells.Count / boundaryArea,
            field.CellAreaMm2,
            cells.Fold(0.0, static (sum, cell) => sum + cell.AreaMm2) / cells.Count,
            cells.Fold(0.0, static (bound, cell) => Math.Max(bound, cell.LloydResidualMm)));

    // The area closure reads the boundary's own tolerance on both terms — a diagonal-scaled slack for the clipped
    // perimeter and a squared floor for a boundary small enough that the scaled term vanishes.
    private static Fin<Unit> Census(SiteDiagram diagram, Seq<PartitionCell> cells, double boundaryArea) {
        double slack = diagram.Boundary.Tolerance.Absolute.Value;
        double cellArea = cells.Fold(0.0, static (sum, cell) => sum + cell.AreaMm2);
        return diagram.Field.Sites >= diagram.Diagram.Cells.Count
            && diagram.Diagram.Cells.Count > 0
            && diagram.Candidates >= diagram.Field.Sites
            && !cells.IsEmpty
            && Math.Abs(boundaryArea - cellArea) <= slack * diagram.Boundary.Bound().Diagonal.Length + slack * slack
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PartitionDegenerate(Subject(diagram.Strategy), cells.Count).ToError());
    }

    private static Fin<T> Degenerate<T>(PartitionStrategy strategy, int count) =>
        Fin.Fail<T>(new FabricationFault.PartitionDegenerate(Subject(strategy), count).ToError());

    private static FaultSubject.Partition Subject(PartitionStrategy strategy) => new(strategy.Key);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
