# [RASM_FABRICATION_PARTITION]

`Partition` owns admitted point-site decomposition from a generative site field through a boundary-clipped diagram, cell topology, spanning traversal, and optional open-stroke classification. Every result retains each disconnected cell region with site, centroid, border, perimeter, edge, adjacency, area, and ordering evidence for pocketing, stippling, engraving, pen plotting, inspection, and deterministic replay.

`Partition.Seed` consumes one admitted request and returns one evidence-complete receipt. Determinism rides the kernel `Deterministic` lanes for both cloud generation and the density-rejection draw, so every draw is index-addressed rather than stream-ordered and a seed change moves the accepted field rather than only its candidates; fixed anchors participate in every field, density callbacks cross one exception boundary, separation is enforced through a pitch-keyed bucket index rather than a pairwise scan, and the diagram itself clips to the admitted `Loop` at its own owner before area, topology, or egress decisions consume it.

## [01]-[INDEX]

- [02]-[PARTITION]: `PartitionStrategy` admits generator policy, `PartitionRequest` closes output modality, and `Partition.Seed` folds `PolygonOp.Cells` and QuikGraph into `PartitionReceipt`.

## [02]-[PARTITION]

- Owner: `SiteDistribution` carries each candidate-cloud law as its own draw delegate over the kernel lanes, `DensityField` carries each acceptance-weight law as a box-relative row, `SamplingField` binds distribution, seed, and density into one generated structural value, and `PartitionRequest` admits the complete boundary-plus-projection aggregate.
- Cases: `SiteDistribution` rows close the candidate laws and `DensityField` rows close the acceptance weights — a new law of either kind is one row carrying its own fold; `PartitionProjection` closes region-only and stroke-classifying egress without overloads.
- Law: `PartitionCell.LloydResidualMm` derives relaxation convergence from the retained site-centroid pair, and `OnBoundary` separates border cells from interior cells so lead-in and stipple-density consumers read the distinction rather than re-deriving it from geometry.
- Entry: `Partition.Seed(PartitionRequest)` owns every point-site modality and preserves the frozen `Seed` operation name.
- Auto: the `SiteDistribution` row draws a candidate cloud off `Deterministic.Unit`, fixed anchors seed the separation index first, true-boundary coverage with an index-addressed density draw admits sites, and one `PolygonOp.Cells` request closes, relaxes, merges, and boundary-clips the diagram — every disconnected region of a clipped cell contributes to census area.
- Receipt: `PartitionReceipt` carries every clipped region per generating site, both the generating site and its clipped centroid, border membership, perimeter, adjacency links, the minimum spanning forest, breadth-first tour, stroke split, candidate/requested/surviving/merge census, and the nearest boundary-anchor cell.
- Packages: `Rasm` supplies the `Deterministic` draw lanes and, through `PolygonOp.Cells`, the bounded Voronoi diagram, its Lloyd relaxation, its merge fold, and the nearest-site index; the `Geometry2D` owner supplies measure, Boolean, and open-clip; QuikGraph supplies Kruskal spanning, component labels, and tree breadth-first paths; LanguageExt supplies applicative admission, traversal, the `HashMap` separation index, and the `Fin` rail.
- Growth: a new site-cloud law is one `SiteDistribution` row carrying its own lane fold; a new grading law is one `DensityField` row; a new egress modality is one `PartitionProjection` case; a new ordering law consumes the retained graph without changing tessellation; a new per-cell measure is one `PartitionCell` column read off the retained `SiteCell`.
- Boundary: QuikGraph construction is the one statement-bearing foreign-mutation seam; aggregate admission, domain computation, and egress remain expression-shaped. Diagrams are never minted here — a page-local tessellator, relaxation loop, or draw stream is the deleted form. Every `PolygonAlgebra.Apply` call carries its `Op` key, so a trace-shape refusal names the calling operation instead of a hand-written axis literal.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
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
public sealed partial class SamplingField {
    public SiteDistribution Source { get; }
    public int Seed { get; }
    public DensityField Density { get; }

    public double Weight(BoundingBox box, Point3d point) => Density.Weight(box, point);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SiteDistribution source,
        ref int seed,
        ref DensityField density) =>
        validationError = source is not null && density is not null
            ? null
            : new ValidationError(message: "partition-sampling-field");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PartitionProjection {
    private PartitionProjection() { }

    public sealed record Regions : PartitionProjection;
    public sealed record Classify(Seq<Edge3> Strokes) : PartitionProjection;
}

[ComplexValueObject]
public sealed partial class PartitionStrategy {
    public string Key { get; }
    public SamplingField Sampling { get; }
    public Arr<Point3d> Anchors { get; }
    public double SitePitchMm { get; }
    public int SiteFloor { get; }
    public int SiteCeiling { get; }
    public int RelaxIterations { get; }
    public double RelaxStrength { get; }
    public double MinimumSeparationRatio { get; }
    public int AttemptFactor { get; }
    public double MergeAreaRatio { get; }
    public double AreaToleranceMm2 { get; }
    public double SiteAreaMm2 => SitePitchMm * SitePitchMm;
    public double SeparationMm => SitePitchMm * MinimumSeparationRatio;

    public static readonly PartitionStrategy PocketRegion = Create(
        key: "pocket-region",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 104_729, density: DensityField.Flat),
        anchors: [],
        sitePitchMm: 12.0,
        siteFloor: 9,
        siteCeiling: 4_096,
        relaxIterations: 4,
        relaxStrength: 1.0,
        minimumSeparationRatio: 0.2,
        attemptFactor: 24,
        mergeAreaRatio: 0.2,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy Stipple = Create(
        key: "stipple",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 130_363, density: DensityField.Centred),
        anchors: [],
        sitePitchMm: 3.0,
        siteFloor: 32,
        siteCeiling: 16_384,
        relaxIterations: 6,
        relaxStrength: 1.0,
        minimumSeparationRatio: 0.35,
        attemptFactor: 32,
        mergeAreaRatio: 0.05,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy EngraveEvenSpacing = Create(
        key: "engrave-even-spacing",
        sampling: SamplingField.Create(SiteDistribution.Gaussian, seed: 155_921, density: DensityField.Flat),
        anchors: [],
        sitePitchMm: 5.0,
        siteFloor: 16,
        siteCeiling: 8_192,
        relaxIterations: 4,
        relaxStrength: 0.75,
        minimumSeparationRatio: 0.3,
        attemptFactor: 24,
        mergeAreaRatio: 0.1,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy PenPlot = Create(
        key: "pen-plot",
        sampling: SamplingField.Create(SiteDistribution.Uniform, seed: 196_613, density: DensityField.Flat),
        anchors: [],
        sitePitchMm: 8.0,
        siteFloor: 12,
        siteCeiling: 4_096,
        relaxIterations: 2,
        relaxStrength: 0.5,
        minimumSeparationRatio: 0.25,
        attemptFactor: 16,
        mergeAreaRatio: 0.15,
        areaToleranceMm2: 0.01);

    public int SitesFor(double boundaryAreaMm2) =>
        checked((int)Math.Clamp(
            Math.Ceiling(boundaryAreaMm2 / SiteAreaMm2),
            Math.Max(SiteFloor, Anchors.Count),
            SiteCeiling));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref SamplingField sampling,
        ref Arr<Point3d> anchors,
        ref double sitePitchMm,
        ref int siteFloor,
        ref int siteCeiling,
        ref int relaxIterations,
        ref double relaxStrength,
        ref double minimumSeparationRatio,
        ref int attemptFactor,
        ref double mergeAreaRatio,
        ref double areaToleranceMm2) {
        double separation = sitePitchMm * minimumSeparationRatio;
        K<Validation<Error>, Unit> numeric = (
            Gate(!string.IsNullOrWhiteSpace(key), "key"),
            Gate(sitePitchMm > 0.0 && double.IsFinite(sitePitchMm)
                && double.IsFinite(sitePitchMm * sitePitchMm), "site-pitch"),
            Gate(siteFloor >= 3 && siteCeiling >= siteFloor, "site-count"),
            Gate(relaxIterations >= 0 && relaxStrength is >= 0.0 and <= 1.0 && double.IsFinite(relaxStrength), "relaxation"),
            Gate(minimumSeparationRatio >= 0.0 && double.IsFinite(minimumSeparationRatio)
                && double.IsFinite(separation), "separation"),
            Gate(attemptFactor >= 1 && siteCeiling <= Array.MaxLength / attemptFactor, "attempt-capacity"),
            Gate(mergeAreaRatio is >= 0.0 and <= 1.0 && double.IsFinite(mergeAreaRatio), "merge-ratio"),
            Gate(areaToleranceMm2 > 0.0 && double.IsFinite(areaToleranceMm2), "area-tolerance"))
            .Apply(static (_, _, _, _, _, _, _, _) => unit);
        K<Validation<Error>, Unit> anchorsValid = (
            Gate(anchors.ForAll(static point => point.IsValid), "anchors-finite"),
            Gate(anchors.Count <= siteCeiling, "anchors-capacity"),
            Gate(anchors.Map((point, index) => (point, index)).ForAll(row =>
                anchors.Take(row.index).ForAll(prior => prior.DistanceTo(row.point) >= separation)), "anchors-separated"))
            .Apply(static (_, _, _) => unit);
        Validation<Error, Unit> admitted = (numeric, anchorsValid)
            .Apply(static (_, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static error => new ValidationError(message: error.Message),
            Succ: static _ => null);
    }

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"partition-strategy:{axis}"));
}

[ComplexValueObject]
public sealed partial class PartitionRequest {
    public PartitionStrategy Strategy { get; }
    public Loop Boundary { get; }
    public PartitionProjection Projection { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PartitionStrategy strategy,
        ref Loop boundary,
        ref PartitionProjection projection) {
        Validation<Error, Unit> admitted = (
            Gate(boundary.Closed && boundary.Count >= 3, "boundary"),
            Gate(strategy.Anchors.ForAll(point =>
                Math.Abs(point.Z - boundary.Plane) <= boundary.Tolerance.Absolute.Value
                && boundary.Covers(point)), "anchors-boundary"),
            Gate(projection.Switch(
                regions: static _ => true,
                classify: static request => request.Strokes.ForAll(static edge => edge.A.IsValid && edge.B.IsValid)), "projection"))
            .Apply(static (_, _, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static error => new ValidationError(message: error.Message),
            Succ: static _ => null);
    }

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

public sealed record PartitionReceipt(
    PartitionStrategy Strategy,
    Loop Boundary,
    Seq<PartitionCell> Cells,
    Seq<PartitionLink> Links,
    Seq<PartitionLink> Spanning,
    Seq<int> Tour,
    Seq<Edge3> Inside,
    Seq<Edge3> Outside,
    int CandidateSites,
    int RequestedSites,
    int ProviderSites,
    int MergedSites,
    int AnchorCell) {
    public Seq<Loop> Regions => Tour.Bind(index => Cells[index].Regions);

    public Seq<PartitionCell> Border => Cells.Filter(static cell => cell.OnBoundary);

    public double LloydResidualMm => Cells.Fold(0.0, static (bound, cell) => Math.Max(bound, cell.LloydResidualMm));
}

file sealed record SiteDiagram(
    CellReceipt Diagram,
    PartitionStrategy Strategy,
    int Requested,
    int Candidates,
    int Admitted,
    Loop Boundary,
    int Anchor);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Partition {
    public static Fin<PartitionReceipt> Seed(PartitionRequest request) =>
        from boundaryArea in MeasureArea(Seq(request.Boundary)).Map(Math.Abs)
        let requested = request.Strategy.SitesFor(boundaryArea)
        from diagram in Tessellate(request, requested, boundaryArea / requested * request.Strategy.MergeAreaRatio)
        from cells in LowerCells(diagram.Diagram, request.Boundary)
        let links = LowerLinks(diagram.Diagram, request.Boundary.Plane)
        from topology in Topology(cells, links, diagram.Anchor)
        from _ in Census(diagram, cells, boundaryArea)
        let regions = topology.Tour.Bind(index => cells[index].Regions)
        from split in request.Projection.Switch(
            regions: static _ => Fin.Succ((Inside: Seq<Edge3>(), Outside: Seq<Edge3>())),
            classify: projection => Classify(projection.Strokes, regions))
        select new PartitionReceipt(
            request.Strategy,
            request.Boundary,
            cells,
            links,
            topology.Spanning,
            topology.Tour,
            split.Inside,
            split.Outside,
            diagram.Candidates,
            diagram.Requested,
            diagram.Diagram.Cells.Count,
            diagram.Admitted - diagram.Diagram.Cells.Count,
            diagram.Anchor);

    // One request closes the diagram: the sifted seed field, the admitted boundary ring, and the relaxation and
    // merge rows the strategy already carries. Nothing here mints a tessellator, a relaxation loop, or a nearest-site
    // index — the algebra owner lowers all three onto the kernel dual and hands back the clipped rings whole.
    private static Fin<SiteDiagram> Tessellate(PartitionRequest request, int requested, double mergeFloor) =>
        from candidates in Candidates(request, requested)
        from accepted in Accept(request.Strategy, request.Boundary, requested, candidates)
        from _ in accepted.Count == requested
            ? Fin.Succ(unit)
            : Degenerate<Unit>(request.Strategy, accepted.Count)
        let policy = SitePolicy.Create(
            relaxations: request.Strategy.RelaxIterations,
            relaxationStrength: request.Strategy.RelaxStrength,
            merge: mergeFloor > 0.0 ? Some(SiteMerge.Create(minimumArea: mergeFloor, minimumSeparation: 0.0)) : None)
        from trace in PolygonAlgebra.Apply(
            new PolygonOp.Cells(accepted.ToArr(), request.Boundary, policy),
            Op.Of(name: nameof(Tessellate)))
        from diagram in trace is PolygonTrace.Celled celled
            ? Fin.Succ(celled.Result)
            : Degenerate<CellReceipt>(request.Strategy, accepted.Count)
        from anchor in diagram.Locate(request.Boundary.At(0), Op.Of(name: nameof(Tessellate)))
        select new SiteDiagram(
            diagram, request.Strategy, requested, candidates.Count, accepted.Count, request.Boundary, anchor);

    // Cloud draws are lane-addressed, so candidate i is a pure function of (seed, i) and the box — a rejected
    // candidate never shifts the ones after it, which a stream-positioned provider draw could not promise.
    private static Fin<Seq<Point3d>> Candidates(PartitionRequest request, int requested) {
        BoundingBox box = request.Boundary.Bound();
        SamplingField sampling = request.Strategy.Sampling;
        return Try.lift(() => checked(requested * request.Strategy.AttemptFactor))
            .Run()
            .MapFail(_ => new FabricationFault.PartitionDegenerate(Subject(request.Strategy), requested).ToError())
            .Map(attempts => Range(0, attempts).ToSeq().Map(index => sampling.Source.Draw(box, sampling.Seed, (long)index)).ToSeq())
            .As()
            .ToFin();
    }

    private static Fin<Seq<Point3d>> Accept(
        PartitionStrategy strategy,
        Loop boundary,
        int requested,
        Seq<Point3d> candidates) =>
        // Rejection draws ride their own lane, so they neither consume nor perturb the cloud lanes: candidate i's
        // draw is a pure function of (seed, i) and its acceptance a pure function of the field row and the box, so
        // the whole sift replays from the strategy alone.
        Try.lift(() => boundary.Bound() is var box && candidates.Map((point, index) => (
            Point: point,
            Draw: Deterministic.Unit(lanes: [(long)index, 2L], seed: strategy.Sampling.Seed),
            Acceptance: strategy.Sampling.Weight(box, point))).ToArr())
            .Run()
            .MapFail(_ => new FabricationFault.PartitionDegenerate(Subject(strategy), candidates.Count).ToError())
            .Bind(rows => rows.ForAll(static row => double.IsFinite(row.Acceptance) && row.Acceptance is >= 0.0 and <= 1.0)
                ? Fin.Succ(Sift(strategy, boundary, requested, strategy.SeparationMm, rows))
                : Degenerate<Seq<Point3d>>(strategy, rows.Count));

    private static Seq<Point3d> Sift(
        PartitionStrategy strategy,
        Loop boundary,
        int requested,
        double separation,
        Arr<(Point3d Point, double Draw, double Acceptance)> rows) =>
        rows.Fold(
            (Index: strategy.Anchors.ToSeq().Fold(HashMap<(int X, int Y), Seq<Point3d>>.Empty,
                 (index, point) => Place(index, point, separation)),
             Accepted: strategy.Anchors.ToSeq()),
            (state, row) => state.Accepted.Count >= requested
                || !row.Point.IsValid
                || !boundary.Covers(row.Point)
                || row.Draw > row.Acceptance
                || Crowded(state.Index, row.Point, separation)
                    ? state
                    : (Place(state.Index, row.Point, separation), state.Accepted.Add(row.Point)))
            .Accepted;

    private static (int X, int Y) Bucket(Point3d point, double separation, int shiftX = 0, int shiftY = 0) =>
        separation > 0.0
            ? ((int)Math.Floor(point.X / separation) + shiftX, (int)Math.Floor(point.Y / separation) + shiftY)
            : (shiftX, shiftY);

    private static HashMap<(int X, int Y), Seq<Point3d>> Place(
        HashMap<(int X, int Y), Seq<Point3d>> index,
        Point3d point,
        double separation) =>
        separation > 0.0
            ? index.AddOrUpdate(
                Bucket(point, separation),
                index.Find(Bucket(point, separation)).Map(bucket => bucket.Add(point)).IfNone(Seq(point)))
            : index;

    private static bool Crowded(
        HashMap<(int X, int Y), Seq<Point3d>> index,
        Point3d point,
        double separation) =>
        separation > 0.0 && Neighbourhood.Exists(shift =>
            index.Find(Bucket(point, separation, shift.X, shift.Y))
                .Map(bucket => bucket.Exists(placed => placed.DistanceTo(point) < separation))
                .IfNone(false));

    private static readonly Arr<(int X, int Y)> Neighbourhood =
        Range(-1, 3).Bind(x => Range(-1, 3).ToSeq().Map(y => (X: x, Y: y))).ToArr();

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

    private static Fin<double> MeasureArea(Seq<Loop> paths) =>
        Measure(paths).Map(static measured => measured.FilledArea);

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
    private static Seq<PartitionLink> LowerLinks(CellReceipt diagram, double elevation) =>
        diagram.Adjacency.ToSeq().Map(edge => new PartitionLink(
            edge.A, edge.B, edge.Start, edge.End, edge.Mid, edge.Length));

    private static Fin<(Seq<PartitionLink> Spanning, Seq<int> Tour)> Topology(
        Seq<PartitionCell> cells,
        Seq<PartitionLink> links,
        int anchor) {
        UndirectedGraph<int, TaggedEdge<int, PartitionLink>> graph = new(allowParallelEdges: false);
        _ = graph.AddVertexRange(Range(0, cells.Count));
        links.Iter(link => graph.AddEdge(new TaggedEdge<int, PartitionLink>(link.Source, link.Target, link)));
        Seq<TaggedEdge<int, PartitionLink>> spanning = toSeq(graph.MinimumSpanningTreeKruskal(static edge => edge.Tag.LengthMm));
        UndirectedGraph<int, TaggedEdge<int, PartitionLink>> forest = new(allowParallelEdges: false);
        _ = forest.AddVertexRange(Range(0, cells.Count));
        spanning.Iter(edge => forest.AddEdge(edge));
        Dictionary<int, int> components = [];
        _ = forest.ConnectedComponents(components);
        Seq<int> tour = toSeq(components.GroupBy(static pair => pair.Value).OrderBy(group => group.Any(pair => pair.Key == anchor) ? 0 : 1)
            .ThenBy(static group => group.Min(static pair => pair.Key)))
            .Bind(group => Breadth(forest, group.Select(static pair => pair.Key).Contains(anchor) ? anchor : group.Min(static pair => pair.Key),
                toSeq(group.Select(static pair => pair.Key))));
        return tour.Count == cells.Count
            ? Fin.Succ((spanning.Map(static edge => edge.Tag), tour))
            : Fin.Fail<(Seq<PartitionLink>, Seq<int>)>(new GeometryFault.DegenerateInput(Kind.Curve, None, "partition:topology").ToError());
    }

    private static Seq<int> Breadth(
        UndirectedGraph<int, TaggedEdge<int, PartitionLink>> forest,
        int root,
        Seq<int> vertices) {
        TryFunc<int, IEnumerable<TaggedEdge<int, PartitionLink>>> paths = forest.TreeBreadthFirstSearch(root);
        return vertices.Map(vertex => paths(vertex, out IEnumerable<TaggedEdge<int, PartitionLink>> path)
                ? (Vertex: vertex, Depth: path.Count(), Length: path.Sum(static edge => edge.Tag.LengthMm))
                : (Vertex: vertex, Depth: vertex == root ? 0 : int.MaxValue, Length: vertex == root ? 0.0 : double.PositiveInfinity))
            .OrderBy(static row => row.Depth)
            .ThenBy(static row => row.Length)
            .ThenBy(static row => row.Vertex)
            .Map(static row => row.Vertex);
    }

    private static Fin<Unit> Census(SiteDiagram diagram, Seq<PartitionCell> cells, double boundaryArea) =>
        cells.Fold(0.0, static (sum, cell) => sum + cell.AreaMm2) is var cellArea
        && diagram.Requested >= diagram.Diagram.Cells.Count
        && diagram.Diagram.Cells.Count > 0
        && diagram.Candidates >= diagram.Requested
        && !cells.IsEmpty
        && Math.Abs(boundaryArea - cellArea) <= diagram.Boundary.Tolerance.Absolute.Value * diagram.Boundary.Bound().Diagonal.Length
            + diagram.Strategy.AreaToleranceMm2
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PartitionDegenerate(Subject(diagram.Strategy), cells.Count).ToError());

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
