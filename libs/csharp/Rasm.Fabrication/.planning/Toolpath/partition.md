# [RASM_FABRICATION_PARTITION]

`Partition` owns admitted point-site decomposition from a generative site field through a boundary-clipped diagram, provider topology, spanning traversal, and optional open-stroke classification. Every result retains each disconnected cell region with site, centroid, border, perimeter, edge, adjacency, area, and ordering evidence for pocketing, stippling, engraving, pen plotting, inspection, and deterministic replay.

`Partition.Seed` consumes one admitted request and returns one evidence-complete receipt. Determinism rides one seeded provider stream that carries both cloud generation and the density-rejection draw, so a seed change moves the accepted field rather than only its candidates; fixed anchors participate in every field, density callbacks cross one exception boundary, separation is enforced through a pitch-keyed bucket index rather than a pairwise scan, and every cell clips to the admitted `Loop` before area, merge, topology, or egress decisions consume it.

## [01]-[INDEX]

- [02]-[PARTITION]: `PartitionStrategy` admits generator policy, `PartitionRequest` closes output modality, and `Partition.Seed` folds SharpVoronoiLib and QuikGraph into `PartitionReceipt`.

## [02]-[PARTITION]

- Owner: `SamplingSource` admits native and caller-supplied provider generation behind one payload-bearing union, `SamplingField` absorbs seed and optional density weighting into one generated structural value, and `PartitionRequest` admits the complete boundary-plus-projection aggregate.
- Cases: `SamplingSource.Native` carries the bounded provider vocabulary while `SamplingSource.Custom` opens the verified generation strategy seam; `PartitionProjection` closes region-only and stroke-classifying egress without overloads.
- Entry: `Partition.Seed(PartitionRequest)` owns every point-site modality and preserves the frozen `Seed` operation name.
- Auto: SharpVoronoiLib generates a seeded candidate cloud, fixed anchors seed the separation index first, true-boundary coverage with a seeded density draw admits sites, Fortune tessellation closes cells, Lloyd relaxation regularizes them, and every disconnected clipped region contributes to merging and census area.
- Receipt: `PartitionReceipt` carries every clipped region per generating site, both the generating site and its clipped centroid, border membership, perimeter, provider edges, the minimum spanning forest, breadth-first tour, stroke split, candidate/requested/provider/merge census, and the nearest boundary-anchor cell.
- Evidence: `PartitionCell.LloydResidualMm` derives relaxation convergence from the retained site-centroid pair, and `OnBoundary` separates border cells from interior cells so lead-in and stipple-density consumers read the distinction rather than re-deriving it from geometry.
- Packages: SharpVoronoiLib supplies native and `IPointGenerationAlgorithm` generation, the `IRandomNumberGenerator` determinism seam behind `SeededRandomNumberGenerator`, `SetSites`, `Tessellate`, `Relax`, `MergeSites`, `Contains`, `BorderLocation`, edge topology, and `GetNearestSiteTo`; QuikGraph supplies Kruskal spanning, component labels, and tree breadth-first paths; LanguageExt supplies applicative admission, traversal, the `HashMap` separation index, and the `Fin` rail.
- Growth: a new site-cloud algorithm implements `IPointGenerationAlgorithm` and enters through `SamplingSource.Custom`; a new egress modality is one `PartitionProjection` case; a new ordering law consumes the retained graph without changing tessellation; a new per-cell measure is one `PartitionCell` column read off the retained provider site.
- Boundary: SharpVoronoiLib and QuikGraph capsules are the statement-bearing foreign-mutation seams; aggregate admission, domain computation, and egress remain expression-shaped. Every `PolygonAlgebra.Apply` call carries its `Op` key, so a trace-shape refusal names the calling operation instead of a hand-written axis literal.

```csharp signature
extern alias Voronoi;

// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using VEdge = Voronoi::SharpVoronoiLib.VoronoiEdge;
using VPlane = Voronoi::SharpVoronoiLib.VoronoiPlane;
using VPoint = Voronoi::SharpVoronoiLib.VoronoiPoint;
using VSite = Voronoi::SharpVoronoiLib.VoronoiSite;
using BorderEdgeGeneration = Voronoi::SharpVoronoiLib.BorderEdgeGeneration;
using IPointGenerationAlgorithm = Voronoi::SharpVoronoiLib.IPointGenerationAlgorithm;
using IRandomNumberGenerator = Voronoi::SharpVoronoiLib.IRandomNumberGenerator;
using NearestSiteLookupMethod = Voronoi::SharpVoronoiLib.NearestSiteLookupMethod;
using PointBorderLocation = Voronoi::SharpVoronoiLib.PointBorderLocation;
using PointGenerationMethod = Voronoi::SharpVoronoiLib.PointGenerationMethod;
using SeededRandomNumberGenerator = Voronoi::SharpVoronoiLib.SeededRandomNumberGenerator;
using VoronoiSiteMergeDecision = Voronoi::SharpVoronoiLib.VoronoiSiteMergeDecision;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SamplingSource {
    private SamplingSource() { }

    public sealed record Native(PointGenerationMethod Method) : SamplingSource;
    public sealed record Custom(IPointGenerationAlgorithm Algorithm) : SamplingSource;
}

[ComplexValueObject]
public sealed partial class SamplingField {
    public SamplingSource Source { get; }
    public int Seed { get; }
    public Option<Func<Point3d, double>> Density { get; }

    public double Weight(Point3d point) => Density.Map(callback => callback(point)).IfNone(1.0);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SamplingSource source,
        ref int seed,
        ref Option<Func<Point3d, double>> density) =>
        validationError = source.Switch(
            native: static _ => true,
            custom: static row => row.Algorithm is not null)
        && density.ForAll(static callback => callback is not null)
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
    public float RelaxStrength { get; }
    public double MinimumSeparationRatio { get; }
    public int AttemptFactor { get; }
    public double MergeAreaRatio { get; }
    public double AreaToleranceMm2 { get; }
    public double SiteAreaMm2 => SitePitchMm * SitePitchMm;
    public double SeparationMm => SitePitchMm * MinimumSeparationRatio;

    public static readonly PartitionStrategy PocketRegion = Create(
        key: "pocket-region",
        sampling: SamplingField.Create(new SamplingSource.Native(PointGenerationMethod.Uniform), seed: 104_729, density: None),
        anchors: [],
        sitePitchMm: 12.0,
        siteFloor: 9,
        siteCeiling: 4_096,
        relaxIterations: 4,
        relaxStrength: 1.0f,
        minimumSeparationRatio: 0.2,
        attemptFactor: 24,
        mergeAreaRatio: 0.2,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy Stipple = Create(
        key: "stipple",
        sampling: SamplingField.Create(new SamplingSource.Native(PointGenerationMethod.Uniform), seed: 130_363, density: None),
        anchors: [],
        sitePitchMm: 3.0,
        siteFloor: 32,
        siteCeiling: 16_384,
        relaxIterations: 6,
        relaxStrength: 1.0f,
        minimumSeparationRatio: 0.35,
        attemptFactor: 32,
        mergeAreaRatio: 0.05,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy EngraveEvenSpacing = Create(
        key: "engrave-even-spacing",
        sampling: SamplingField.Create(new SamplingSource.Native(PointGenerationMethod.Gaussian), seed: 155_921, density: None),
        anchors: [],
        sitePitchMm: 5.0,
        siteFloor: 16,
        siteCeiling: 8_192,
        relaxIterations: 4,
        relaxStrength: 0.75f,
        minimumSeparationRatio: 0.3,
        attemptFactor: 24,
        mergeAreaRatio: 0.1,
        areaToleranceMm2: 0.01);

    public static readonly PartitionStrategy PenPlot = Create(
        key: "pen-plot",
        sampling: SamplingField.Create(new SamplingSource.Native(PointGenerationMethod.Uniform), seed: 196_613, density: None),
        anchors: [],
        sitePitchMm: 8.0,
        siteFloor: 12,
        siteCeiling: 4_096,
        relaxIterations: 2,
        relaxStrength: 0.5f,
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
        ref float relaxStrength,
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
            Gate(relaxIterations >= 0 && relaxStrength is >= 0.0f and <= 1.0f && float.IsFinite(relaxStrength), "relaxation"),
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
        admitted
            ? Fin.Succ(unit).ToValidation()
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"partition-strategy:{axis}").ToError()).ToValidation();
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
        admitted
            ? Fin.Succ(unit).ToValidation()
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"partition-request:{axis}").ToError()).ToValidation();
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

file sealed record ProviderDiagram(
    VPlane Plane,
    Seq<VSite> Sites,
    PartitionStrategy Strategy,
    int Requested,
    int Candidates,
    int Initial,
    Loop Boundary,
    int Anchor);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Partition {
    public static Fin<PartitionReceipt> Seed(PartitionRequest request) =>
        from boundaryArea in MeasureArea(Seq(request.Boundary)).Map(Math.Abs)
        let requested = request.Strategy.SitesFor(boundaryArea)
        from diagram in Tessellate(request, requested, boundaryArea / requested * request.Strategy.MergeAreaRatio)
        from cells in LowerCells(diagram.Sites, request.Boundary)
        let links = LowerLinks(diagram.Plane, diagram.Sites, request.Boundary.Plane)
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
            diagram.Sites.Count,
            diagram.Initial - diagram.Sites.Count,
            diagram.Anchor);

    private static Fin<ProviderDiagram> Tessellate(PartitionRequest request, int requested, double mergeFloor) =>
        Try.lift<Fin<ProviderDiagram>>(() => {
            BoundingBox bounds = request.Boundary.Bound();
            VPlane plane = new(bounds.Min.X, bounds.Min.Y, bounds.Max.X, bounds.Max.Y);
            SamplingField sampling = request.Strategy.Sampling;
            int attempts = checked(requested * request.Strategy.AttemptFactor);
            SeededRandomNumberGenerator random = new(sampling.Seed);
            sampling.Source.Switch(
                state: (Plane: plane, Attempts: attempts, Random: random),
                native: static (state, row) => state.Plane.GenerateRandomSites(state.Attempts, row.Method, state.Random),
                custom: static (state, row) => state.Plane.GenerateRandomSites(state.Attempts, row.Algorithm, state.Random));
            Seq<Point3d> candidates = toSeq(plane.Sites).Map(site => new Point3d(site.X, site.Y, request.Boundary.Plane));
        return Accept(request.Strategy, request.Boundary, requested, candidates, sampling.Weight, random).Bind(accepted => {
                if (accepted.Count != requested)
                    return Degenerate<ProviderDiagram>(request.Strategy, accepted.Count);

                plane.SetSites(accepted.Map(static point => new VSite(point.X, point.Y)).ToList());
                plane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
                if (request.Strategy.RelaxIterations > 0)
                    plane.Relax(request.Strategy.RelaxIterations, request.Strategy.RelaxStrength, reTessellate: true);

                Seq<VSite> initial = toSeq(plane.Sites);
                if (plane.DuplicateCount > 0 || !Valid(initial))
                    return Degenerate<ProviderDiagram>(request.Strategy, initial.Count);

                return Merge(plane, request.Boundary, mergeFloor).Bind(_ => {
                    Seq<VSite> sites = toSeq(plane.Sites);
                    if (plane.DuplicateCount > 0 || !Valid(sites))
                        return Degenerate<ProviderDiagram>(request.Strategy, sites.Count);

                    Point3d boundaryAnchor = request.Boundary.At(0);
                    VSite anchor = plane.GetNearestSiteTo(boundaryAnchor.X, boundaryAnchor.Y, NearestSiteLookupMethod.KDTree);
                    return IndexOf(sites, anchor)
                        .Map(anchorIndex => new ProviderDiagram(
                            plane,
                            sites,
                            request.Strategy,
                            requested,
                            candidates.Count,
                            initial.Count,
                            request.Boundary,
                            anchorIndex))
                        .ToFin(FabricationFault.PartitionDegenerate(Subject(request.Strategy), sites.Count).ToError());
                });
            });
        })
        .Run()
        .MapFail(_ => FabricationFault.PartitionDegenerate(Subject(request.Strategy), requested).ToError())
        .Bind(static result => result);

    private static Fin<Seq<Point3d>> Accept(
        PartitionStrategy strategy,
        Loop boundary,
        int requested,
        Seq<Point3d> candidates,
        Func<Point3d, double> acceptance,
        IRandomNumberGenerator random) =>
        // ToArr forces the stateful provider draw in candidate order; a lazy Map would desequence the seeded stream.
        Try.lift(() => candidates.Map(point => (
            Point: point,
            Draw: random.NextDouble(),
            Acceptance: acceptance(point))).ToArr())
            .Run()
            .MapFail(_ => FabricationFault.PartitionDegenerate(Subject(strategy), candidates.Count).ToError())
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
        Range(-1, 3).Bind(x => Range(-1, 3).Map(y => (X: x, Y: y))).ToArr();

    private static bool Valid(Seq<VSite> sites) =>
        !sites.IsEmpty && sites.ForAll(site => {
            Seq<VEdge> edges = toSeq(site.ClockwiseEdges);
            return site.Closed
                && site.ClockwisePoints.Count >= 3
                && site.ClockwiseEdgesWound.Count == site.ClockwisePoints.Count
                && edges.Count == site.ClockwisePoints.Count
                && site.ClockwisePoints.All(static point => double.IsFinite(point.X) && double.IsFinite(point.Y))
                && site.Contains(site.Centroid.X, site.Centroid.Y)
                && edges.Map((edge, index) => edge.CommonPointWith(edges[(index + 1) % edges.Count])).ForAll(static point => point is not null)
                && !site.Neighbours.Any(neighbour => ReferenceEquals(site, neighbour));
        });

    private static Fin<Unit> Merge(VPlane plane, Loop boundary, double floor) =>
        floor <= 0.0
            ? Fin.Succ(unit)
            : toSeq(plane.Sites).Traverse(site =>
                ClippedArea(site, boundary).Map(area => (Site: site, Area: area)))
              .As()
              .Map(rows => {
                  Dictionary<VSite, double> live = new(ReferenceEqualityComparer.Instance);
                  rows.Iter(row => live[row.Site] = row.Area);
                  plane.MergeSites((left, right) => {
                      double leftArea = live[left];
                      double rightArea = live[right];
                      VoronoiSiteMergeDecision decision =
                          leftArea < floor && leftArea <= rightArea ? VoronoiSiteMergeDecision.MergeIntoSite2
                          : rightArea < floor ? VoronoiSiteMergeDecision.MergeIntoSite1
                          : VoronoiSiteMergeDecision.DontMerge;
                      if (decision == VoronoiSiteMergeDecision.MergeIntoSite2) live[right] = leftArea + rightArea;
                      if (decision == VoronoiSiteMergeDecision.MergeIntoSite1) live[left] = leftArea + rightArea;
                      return decision;
                  });
                  return unit;
              });

    private static Fin<double> ClippedArea(VSite site, Loop boundary) =>
        RawCell(site, boundary).Bind(cell => Intersect(cell, boundary)
            .Bind(pieces => pieces.Traverse(piece => MeasureArea(Seq(piece))).As()
                .Map(static areas => areas.Fold(0.0, static (sum, area) => sum + Math.Abs(area)))));

    private static Fin<Seq<PartitionCell>> LowerCells(Seq<VSite> sites, Loop boundary) =>
        sites.Map((site, index) => (Site: site, Index: index)).Traverse(row =>
            from cell in RawCell(row.Site, boundary)
            from raw in Measure(Seq(cell))
            from measured in Intersect(cell, boundary).Bind(pieces => pieces.Traverse(piece => Measure(Seq(piece))
                .Map(value => (Region: piece, Measure: value))).As())
            let regions = measured.Filter(static piece => piece.Region.Count >= 3 && piece.Measure.FilledArea > 0.0)
            from _ in regions.IsEmpty
                ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "partition:empty-cell").ToError())
                : Fin.Succ(unit)
            let area = regions.Sum(static piece => piece.Measure.FilledArea)
            select new PartitionCell(
                row.Index,
                regions.Map(static piece => piece.Region),
                new Point3d(row.Site.X, row.Site.Y, boundary.Plane),
                new Point3d(
                    regions.Sum(piece => piece.Measure.Centroid.X * piece.Measure.FilledArea) / area,
                    regions.Sum(piece => piece.Measure.Centroid.Y * piece.Measure.FilledArea) / area,
                    boundary.Plane),
                area,
                regions.Sum(static piece => piece.Measure.BoundaryLength),
                Math.Abs(raw.FilledArea - area) > boundary.Tolerance.Absolute.Value,
                toSeq(row.Site.Neighbours)
                    .Choose(neighbour => IndexOf(sites, neighbour))
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
        PolygonAlgebra.Apply(new PolygonOp.Boolean(Seq(subject), Seq(clip), PolygonBoolean.Intersection, PolygonFill.NonZero), Op.Of())
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Filter(static node => !node.IsHole).Map(static node => node.Boundary))
                : Fin.Fail<Seq<Loop>>(Op.Of(name: nameof(Intersect)).InvalidResult()));

    private static Fin<(Seq<Edge3> Inside, Seq<Edge3> Outside)> Classify(Seq<Edge3> subject, Seq<Loop> clip) =>
        PolygonAlgebra.Apply(new PolygonOp.ClipOpen(Seq(subject), clip, PolygonFill.NonZero), Op.Of())
            .Bind(static trace => trace is PolygonTrace.SplitRuns split
                ? Fin.Succ((split.Inside.Bind(static run => run), split.Outside.Bind(static run => run)))
                : Fin.Fail<(Seq<Edge3>, Seq<Edge3>)>(Op.Of(name: nameof(Classify)).InvalidResult()));

    private static Seq<PartitionLink> LowerLinks(VPlane plane, Seq<VSite> sites, double elevation) =>
        toSeq(plane.Edges).Choose(edge =>
            from left in IndexOf(sites, edge.Left)
            from right in IndexOf(sites, edge.Right)
            where left != right
            select new PartitionLink(
                    Math.Min(left, right),
                    Math.Max(left, right),
                    ToPoint(edge.Start, elevation),
                    ToPoint(edge.End, elevation),
                    ToPoint(edge.Mid, elevation),
                    edge.Length))
        .DistinctBy(static link => (link.Source, link.Target))
        .ToSeq();

    private static Option<int> IndexOf(Seq<VSite> sites, VSite target) =>
        sites.Map((site, index) => ReferenceEquals(site, target) ? Some(index) : None)
            .Choose(static index => index)
            .HeadOrNone();

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
            : Fin.Fail<(Seq<PartitionLink>, Seq<int>)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "partition:topology").ToError());
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

    private static Fin<Unit> Census(ProviderDiagram diagram, Seq<PartitionCell> cells, double boundaryArea) =>
        cells.Fold(0.0, static (sum, cell) => sum + cell.AreaMm2) is var cellArea
        && diagram.Requested >= diagram.Sites.Count
        && diagram.Sites.Count > 0
        && diagram.Candidates >= diagram.Requested
        && !cells.IsEmpty
        && Math.Abs(boundaryArea - cellArea) <= diagram.Boundary.Tolerance.Absolute.Value * diagram.Boundary.Bound().Diagonal.Length
            + diagram.Strategy.AreaToleranceMm2
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.PartitionDegenerate(Subject(diagram.Strategy), cells.Count).ToError());

    private static Fin<Loop> RawCell(VSite site, Loop boundary) =>
        Loop.Admit(
            toSeq(site.ClockwisePoints).Map(point => ToPoint(point, boundary.Plane)).ToArr(),
            closed: true,
            bulges: [],
            tolerance: boundary.Tolerance).Map(static loop => loop.AsCcw());

    private static Fin<T> Degenerate<T>(PartitionStrategy strategy, int count) =>
        Fin.Fail<T>(FabricationFault.PartitionDegenerate(Subject(strategy), count).ToError());

    private static FaultSubject.Partition Subject(PartitionStrategy strategy) => new(strategy.Key);

    private static Point3d ToPoint(VPoint point, double elevation) => new(point.X, point.Y, elevation);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
