# [RASM_FABRICATION_LINKING]

`Linking` owns post-placement cut topology. It preserves every placement transform, converts common boundaries into one shared cut with explicit source-contour omissions, threads physical instances under containment precedence, retains bridge gaps as cut evidence, and derives waste partitions from bounded point-site Voronoi adjacency.

`LinkPlan` carries one objective verdict whose empty `Applied` sequence is the measured baseline. `ContourCut.Pierce` marks one entry per connected cut component, `ContourCut.Path` starts at that entry so posting leads at loop parameter zero, and `OmittedSpan` names the segment each shared window removes.

## [01]-[INDEX]

- [01]-[CUT_LINKING]: owns cut-graph admission, common-line matching, precedence-safe chains, bridge gaps, point-site waste partitioning, objective selection, and posting topology.

## [02]-[CUT_LINKING]

- Owner: `LinkRun` is the generated ingress owner; `CutLinkPolicy` and `CutLinkObjective` are generated policy values; `LinkOp` closes applied edit evidence; `LinkVerdict` carries one baseline-or-applied payload.
- Cases: `LinkCapability` selects common-line and chain scheduling; `BridgePolicy` and `WastePolicy` derive their own enablement from their payload cases.
- Entry: `Plan(LinkRun)` admits placement, plane-compatible polygonal profiles, sheet stock, sheet-scoped keep-outs, and policy once before one composed graph fold.
- Auto: a longest-first conflict fold selects non-overlapping common lines; `ConnectedComponents`, transitive reduction, topological order, Kruskal forest, breadth-first paths, and A-star routes derive chain and rapid topology; `VoronoiPlane` derives bounded point-site waste adjacency and `VoronoiSite.ClockwisePoints` measures each closed cell against the reusable floor; `PolygonAlgebra.Apply` owns line-space offset, Boolean, measure, relation, and open clip.
- Receipt: `LinkComparison` carries pierce, rapid, cut, shared, bridge, partition, heat, quality, and remnant-loss axes before and after the candidate topology; remnant loss combines partition kerf area with sub-floor cell area as offcut-side costs.
- Packages: `LanguageExt.Core`, `QuikGraph`, `SharpVoronoiLib`, `Thinktecture.Runtime.Extensions`, `Rasm`, `RhinoCommon`, and the `Geometry2D` owners compose the surface.
- Growth: a cut edit is one `LinkOp` case; a scoring axis is one `LinkEvidence` member with one `CutLinkObjective` weight; a waste decomposition is one `WastePolicy` case; no consumer gains an orchestration step.
- Boundary: `ChainRow.SheetIndex`, `Instances`, `SourceParts`, `Pierces`, `Members`, `Shared`, and `RapidPaths` form the posting seam, and `ContourCut.Path` is entry-rotated so a consumer leads at parameter zero without re-deriving the entry; mutable `QuikGraph` construction is statement-bearing, and `VoronoiPlane` construction runs through the synchronous `Try` rail.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
extern alias Voronoi;

using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using BorderEdgeGeneration = Voronoi::SharpVoronoiLib.BorderEdgeGeneration;
using VoronoiPlane = Voronoi::SharpVoronoiLib.VoronoiPlane;
using VoronoiSite = Voronoi::SharpVoronoiLib.VoronoiSite;
using VoronoiSiteMergeDecision = Voronoi::SharpVoronoiLib.VoronoiSiteMergeDecision;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LinkCapability {
    public static readonly LinkCapability CommonLine = new("common-line");
    public static readonly LinkCapability Chain = new("chain");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BridgePolicy {
    private BridgePolicy() { }

    public sealed record Disabled : BridgePolicy;
    public sealed record Spaced(double WidthMm, double SpacingMm, double EndClearanceMm) : BridgePolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WastePolicy {
    private WastePolicy() { }

    public sealed record Disabled : WastePolicy;
    public sealed record Voronoi(
        double SiteSpacingMm,
        int MaxSites,
        int Relaxations,
        float RelaxationStrength,
        double MergeDistanceMm,
        double MinEdgeMm,
        double MinReusableAreaMm2,
        int RapidProbeNodes)
        : WastePolicy;
}

[ComplexValueObject]
public sealed partial class CutLinkObjective {
    public double Pierce { get; }
    public double Rapid { get; }
    public double Cut { get; }
    public double Heat { get; }
    public double Quality { get; }
    public double Remnant { get; }

    public double Score(LinkEvidence evidence) =>
        (evidence.Pierces * Pierce) + (evidence.RapidMm * Rapid) + (evidence.CutMm * Cut)
        + (evidence.HeatLoad * Heat) + (evidence.QualityRisk * Quality) + (evidence.RemnantLossMm2 * Remnant);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double pierce,
        ref double rapid,
        ref double cut,
        ref double heat,
        ref double quality,
        ref double remnant) {
        double[] weights = [pierce, rapid, cut, heat, quality, remnant];
        validationError = weights.All(static value => double.IsFinite(value) && value >= 0.0)
            && weights.Any(static value => value > 0.0)
            ? null
            : new ValidationError("link objective weights must be finite and nonnegative");
    }
}

[ComplexValueObject]
public sealed partial class CutLinkPolicy {
    public Set<LinkCapability> Enabled { get; }
    public double CutWidthMm { get; }
    public double MatchToleranceMm { get; }
    public double AngularToleranceRadians { get; }
    public double ArcToleranceMm { get; }
    public double ClearanceMiterLimit { get; }
    public double MinSharedLengthMm { get; }
    public int MaxChainParts { get; }
    public double ChainBandMm { get; }
    public double MaxContinuousCutMm { get; }
    public Point3d RapidOrigin { get; }
    public BridgePolicy Bridge { get; }
    public WastePolicy Waste { get; }
    public CutLinkObjective Objective { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Set<LinkCapability> enabled,
        ref double cutWidthMm,
        ref double matchToleranceMm,
        ref double angularToleranceRadians,
        ref double arcToleranceMm,
        ref double clearanceMiterLimit,
        ref double minSharedLengthMm,
        ref int maxChainParts,
        ref double chainBandMm,
        ref double maxContinuousCutMm,
        ref Point3d rapidOrigin,
        ref BridgePolicy bridge,
        ref WastePolicy waste,
        ref CutLinkObjective objective) {
        double[] policyScalars = [cutWidthMm, matchToleranceMm, angularToleranceRadians, arcToleranceMm, clearanceMiterLimit, minSharedLengthMm,
            chainBandMm, maxContinuousCutMm];
        bool scalars = policyScalars.All(static value => double.IsFinite(value) && value > 0.0);
        bool bridgeValid = bridge.Switch(
            disabled: static _ => true,
            spaced: static row => {
                double[] positive = [row.WidthMm, row.SpacingMm];
                return positive.All(static value => double.IsFinite(value) && value > 0.0)
                    && double.IsFinite(row.EndClearanceMm) && row.EndClearanceMm >= 0.0 && row.SpacingMm > row.WidthMm;
            });
        bool wasteValid = waste.Switch(
            disabled: static _ => true,
            voronoi: static row => double.IsFinite(row.SiteSpacingMm) && row.SiteSpacingMm > 0.0 && row.MaxSites >= 2 && row.Relaxations >= 0
                && row.RelaxationStrength is > 0.0f and <= 1.0f && double.IsFinite(row.MergeDistanceMm) && row.MergeDistanceMm >= 0.0
                && double.IsFinite(row.MinEdgeMm) && row.MinEdgeMm > 0.0
                && double.IsFinite(row.MinReusableAreaMm2) && row.MinReusableAreaMm2 >= 0.0 && row.RapidProbeNodes >= 1);
        validationError = scalars && angularToleranceRadians < Math.PI / 2.0 && maxChainParts > 0 && rapidOrigin.IsValid
            && bridgeValid && wasteValid
                ? null
                : new ValidationError("link policy is outside its admitted geometric or objective domain");
    }
}

[ComplexValueObject]
public sealed partial class LinkRun {
    public FabricationResult.Placement Placement { get; }
    public Arr<Seq<Loop>> Profiles { get; }
    public Map<int, Stock> StockBySheet { get; }
    public Map<int, Seq<Loop>> KeepOutBySheet { get; }
    public CutLinkPolicy Policy { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FabricationResult.Placement placement,
        ref Arr<Seq<Loop>> profiles,
        ref Map<int, Stock> stockBySheet,
        ref Map<int, Seq<Loop>> keepOutBySheet,
        ref CutLinkPolicy policy) {
        Set<int> occupied = toSet(placement.Parts.Map(static transform => transform.SheetIndex));
        bool census = occupied.Count == stockBySheet.Count && stockBySheet.Keys.ForAll(occupied.Contains);
        bool identities = placement.Parts.Map(static transform => new PartInstance(transform.PartId, transform.Instance))
            .Distinct().Count == placement.Parts.Count;
        bool profilesAdmitted = profiles.ForAll(region => region.Head.Exists(anchor =>
            region.ForAll(loop => Polygon(loop, anchor.Tolerance, anchor.Plane))));
        bool keepOutsAdmitted = keepOutBySheet.Keys.ForAll(sheet => stockBySheet.Find(sheet).Exists(stock =>
            stock.Region.Head.Exists(anchor => keepOutBySheet.Find(sheet).Exists(region =>
                region.ForAll(loop => Polygon(loop, stock.Tolerance, anchor.Plane))))));
        validationError = placement.Parts.IsEmpty || profiles.IsEmpty || profiles.Exists(static region => region.IsEmpty)
            || !census || !identities || !profilesAdmitted || !keepOutsAdmitted
            || placement.Parts.Exists(transform => transform.PartId < 0 || transform.PartId >= profiles.Count)
                ? new ValidationError("link run requires unique part instances and plane-compatible polygonal profiles, stock, and keep-outs")
                : null;
    }

    static bool Polygon(Loop loop, Context tolerance, Plane plane) =>
        loop.Closed && loop.Count >= 3 && loop.Tolerance == tolerance && loop.Plane.Equals(plane);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SegmentWindow(double Start, double End) {
    public double Length(double span) => (End - Start) * span;
    public bool Overlaps(SegmentWindow other, double tolerance) => Math.Min(End, other.End) - Math.Max(Start, other.Start) > tolerance;
}

public readonly record struct EdgeRef(PartInstance Part, int Contour, int Segment);
public sealed record SharedEdge(
    int SheetIndex,
    EdgeRef A,
    EdgeRef B,
    SegmentWindow WindowA,
    SegmentWindow WindowB,
    double SpanAmm,
    double SpanBmm,
    Edge3 Cut,
    double SharedLengthMm,
    Context Tolerance);

public sealed record SharedCut(SharedEdge Edge, Seq<SegmentWindow> Gaps);
public readonly record struct OmittedSpan(int Segment, SegmentWindow Window);
public sealed record ContourCut(int Contour, Loop Path, Seq<OmittedSpan> Omitted, Point3d Entry, bool Pierce);
public sealed record ChainMember(PartInstance Part, Seq<ContourCut> Contours, double CutLengthMm);

public sealed record ChainRow(
    int Chain,
    int SheetIndex,
    Seq<Point3d> Pierces,
    Seq<ChainMember> Members,
    Seq<SharedCut> Shared,
    Seq<Seq<Point3d>> RapidPaths,
    double CutLengthMm) {
    public Seq<int> Instances => Members.Map(static member => member.Part.Ordinal);
    public Seq<int> SourceParts => Members.Map(static member => member.Part.PartId);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinkOp {
    private LinkOp() { }

    public sealed record CommonLine(SharedEdge Pair) : LinkOp;
    public sealed record ChainCut(ChainRow Row) : LinkOp;
    public sealed record Bridge(SharedEdge Pair, Point3d At, double WidthMm) : LinkOp;
    public sealed record WasteCutUp(
        int SheetIndex,
        Seq<Edge3> Cuts,
        int Sites,
        int Cells,
        int DuplicateSites,
        double FragmentAreaMm2) : LinkOp;
}

public sealed record LinkEvidence(
    int Pierces,
    double RapidMm,
    double CutMm,
    double SharedMm,
    double BridgeGapMm,
    double PartitionMm,
    double HeatLoad,
    double QualityRisk,
    double RemnantLossMm2);

public sealed record LinkComparison(LinkEvidence Before, LinkEvidence After, double ScoreBefore, double ScoreAfter);

public sealed record LinkVerdict(Seq<LinkOp> Applied, Seq<ChainRow> Chains);
public sealed record LinkPlan(LinkVerdict Verdict, LinkComparison Evidence);

file sealed record PlacedPart(
    PartInstance Part,
    int SheetIndex,
    Seq<Loop> Region,
    PolygonMeasure Measure);

file sealed record WasteRow(
    int SheetIndex,
    Seq<Loop> Usable,
    Seq<Edge3> Cuts,
    int Sites,
    int Cells,
    int DuplicateSites,
    double FragmentAreaMm2,
    AdjacencyGraph<int, TaggedEdge<int, double>> Routes,
    Map<int, Point3d> Nodes);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Linking {
    public static Fin<LinkPlan> Plan(LinkRun run) =>
        from placed in Place(run)
        from clear in Candidates(placed, run).Traverse(edge => Clears(edge, placed, run).Map(ok => (Edge: edge, Clear: ok)).ToValidation()).As().ToFin()
        let selected = run.Policy.Enabled.Contains(LinkCapability.CommonLine)
            ? Match(clear.Filter(static row => row.Clear).Map(static row => row.Edge))
            : Seq<SharedEdge>()
        from precedence in Precedence(placed)
        let bridges = Bridges(selected, run.Policy.Bridge)
        from baseline in Chains(placed, Seq<SharedEdge>(), Seq<LinkOp.Bridge>(), precedence, false, run.Policy)
        from optimized in run.Policy.Enabled.Contains(LinkCapability.Chain) || !selected.IsEmpty
            ? Chains(placed, selected, bridges, precedence, run.Policy.Enabled.Contains(LinkCapability.Chain), run.Policy)
            : Fin.Succ(baseline)
        from safe in Waste(placed, run, new WastePolicy.Disabled())
        from waste in Waste(placed, run, run.Policy.Waste)
        from baseRouted in Route(baseline, safe, run.Policy)
        from routed in Route(optimized, waste, run.Policy)
        let appliedShared = routed.Bind(static chain => chain.Shared.Map(static cut => cut.Edge)).Distinct()
        let appliedBridges = bridges.Filter(bridge => appliedShared.Contains(bridge.Pair))
        let baseEvidence = Evidence(baseRouted, Seq<SharedEdge>(), Seq<LinkOp.Bridge>(), safe, run)
        let nextEvidence = Evidence(routed, appliedShared, appliedBridges, waste, run)
        let comparison = new LinkComparison(
            baseEvidence,
            nextEvidence,
            run.Policy.Objective.Score(baseEvidence),
            run.Policy.Objective.Score(nextEvidence))
        let operations = appliedShared.Map(static edge => (LinkOp)new LinkOp.CommonLine(edge))
            .Concat(routed.Map(static chain => (LinkOp)new LinkOp.ChainCut(chain)))
            .Concat(appliedBridges.Map(static bridge => (LinkOp)bridge))
            .Concat(waste.Filter(static row => !row.Cuts.IsEmpty)
                .Map(static row => (LinkOp)new LinkOp.WasteCutUp(
                    row.SheetIndex, row.Cuts, row.Sites, row.Cells, row.DuplicateSites, row.FragmentAreaMm2)))
        select comparison.ScoreAfter < comparison.ScoreBefore && !operations.IsEmpty
            ? new LinkPlan(new LinkVerdict(operations, routed), comparison)
            : new LinkPlan(new LinkVerdict(Seq<LinkOp>(), baseRouted),
                comparison with { After = baseEvidence, ScoreAfter = comparison.ScoreBefore });

    private static Fin<Seq<PlacedPart>> Place(LinkRun run) =>
        run.Placement.Parts.Traverse(transform => (
            from region in run.Profiles[transform.PartId].Traverse(transform.Apply).As()
            from trace in PolygonAlgebra.Apply(new PolygonOp.Topology(region, PolygonFill.NonZero))
            from ordered in trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.OrderByDescending(static node => node.Depth)
                    .ThenBy(static node => Math.Abs(node.SignedArea)).Map(static node => node.Boundary).ToSeq())
                : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:placement-topology").ToError())
            from measure in Measure(ordered)
            select new PlacedPart(
                new PartInstance(transform.PartId, transform.Instance), transform.SheetIndex, ordered, measure))
            .ToValidation()).As().ToFin();

    private static Fin<PolygonMeasure> Measure(Seq<Loop> region) =>
        PolygonAlgebra.Apply(new PolygonOp.Measure(region, PolygonFill.NonZero))
            .Bind(static trace => trace is PolygonTrace.Measured measured
                ? Fin.Succ(measured.Result)
                : Fin.Fail<PolygonMeasure>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:measure-trace").ToError()));

    private static Seq<SharedEdge> Candidates(Seq<PlacedPart> placed, LinkRun run) =>
        (from a in placed
         from b in placed
         where b.SheetIndex == a.SheetIndex && Compare(a.Part, b.Part) < 0
            && Near(a.Measure.Bounds, b.Measure.Bounds, run.Policy.CutWidthMm + run.Policy.MatchToleranceMm)
         from left in a.Region.Map((loop, contour) => (loop, contour))
         from right in b.Region.Map((loop, contour) => (loop, contour))
         from edgeA in toSeq(Enumerable.Range(0, left.loop.Count))
         from edgeB in toSeq(Enumerable.Range(0, right.loop.Count))
         from pair in Pair(a, left.loop, left.contour, edgeA, b, right.loop, right.contour, edgeB, run.Policy).ToSeq()
         select pair)
            .OrderByDescending(static edge => edge.SharedLengthMm)
            .ThenBy(static edge => edge.A.Part.PartId)
            .ThenBy(static edge => edge.A.Part.Ordinal)
            .ThenBy(static edge => edge.A.Contour)
            .ThenBy(static edge => edge.A.Segment)
            .ToSeq();

    private static Option<SharedEdge> Pair(
        PlacedPart a,
        Loop left,
        int leftContour,
        int leftEdge,
        PlacedPart b,
        Loop right,
        int rightContour,
        int rightEdge,
        CutLinkPolicy policy) {
        if (left.BulgeAt(leftEdge) != 0.0 || right.BulgeAt(rightEdge) != 0.0) return None;
        Point3d a0 = left.At(leftEdge), a1 = left.At(leftEdge + 1), b0 = right.At(rightEdge), b1 = right.At(rightEdge + 1);
        Vector3d da = a1 - a0, db = b1 - b0;
        double la = da.Length, lb = db.Length, epsilon = left.Tolerance.Absolute.Value;
        if (la <= epsilon || lb <= epsilon || (da * db) / (la * lb) > -Math.Cos(policy.AngularToleranceRadians)) return None;
        double[] separations = [Dist(a0, a1, b0), Dist(a0, a1, b1), Dist(b0, b1, a0), Dist(b0, b1, a1)];
        if (separations.Any(distance => Math.Abs(distance - policy.CutWidthMm) > policy.MatchToleranceMm)) return None;
        double aStart = ((b0 - a0) * da) / (la * la), aEnd = ((b1 - a0) * da) / (la * la);
        double lo = Math.Max(0.0, Math.Min(aStart, aEnd)), hi = Math.Min(1.0, Math.Max(aStart, aEnd));
        if ((hi - lo) * la < policy.MinSharedLengthMm) return None;
        Point3d first = a0 + (da * lo), last = a0 + (da * hi);
        Point3d firstB = Project(first, b0, db), lastB = Project(last, b0, db);
        Point3d cutA = first + (0.5 * (firstB - first)), cutB = last + (0.5 * (lastB - last));
        double bFirst = ((firstB - b0) * db) / (lb * lb), bLast = ((lastB - b0) * db) / (lb * lb);
        bool forwardA = Forward(a0, a1), forwardB = Forward(b0, b1);
        if (forwardA == forwardB) return None;
        return Some(new SharedEdge(
            a.SheetIndex,
            new EdgeRef(a.Part, leftContour, leftEdge),
            new EdgeRef(b.Part, rightContour, rightEdge),
            new SegmentWindow(lo, hi),
            new SegmentWindow(Math.Max(0.0, Math.Min(bFirst, bLast)), Math.Min(1.0, Math.Max(bFirst, bLast))),
            la,
            lb,
            new Edge3(cutA, cutB),
            cutA.DistanceTo(cutB),
            left.Tolerance));
    }

    private static Fin<bool> Clears(SharedEdge edge, Seq<PlacedPart> placed, LinkRun run) =>
        from segment in Loop.Admit(Arr(edge.Cut.A, edge.Cut.B), closed: false, Arr<double>(), edge.Tolerance)
        from policy in OffsetPolicy.Admit(
            OffsetJoin.Square,
            OffsetEnd.Square,
            miterLimit: run.Policy.ClearanceMiterLimit,
            arcTolerance: run.Policy.ArcToleranceMm)
        from clearanceTrace in PolygonAlgebra.Apply(new PolygonOp.Offset(
            Seq(segment),
            new OffsetField.Uniform((0.5 * run.Policy.CutWidthMm) + run.Policy.MatchToleranceMm),
            policy))
        from clearance in Paths(clearanceTrace)
        from stock in run.StockBySheet.Find(edge.SheetIndex)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, $"link:sheet:{edge.SheetIndex}").ToError())
        from outsideTrace in PolygonAlgebra.Apply(new PolygonOp.Boolean(
            clearance, stock.Region, PolygonBoolean.Difference, PolygonFill.NonZero))
        from outside in Paths(outsideTrace)
        from blockedTrace in PolygonAlgebra.Apply(new PolygonOp.Boolean(
            clearance,
            placed.Filter(row => row.SheetIndex == edge.SheetIndex
                    && row.Part != edge.A.Part && row.Part != edge.B.Part)
                .Bind(static row => row.Region).Concat(stock.Exclusions)
                .Concat(run.KeepOutBySheet.Find(edge.SheetIndex).IfNone(Seq<Loop>())),
            PolygonBoolean.Intersection,
            PolygonFill.NonZero))
        from blocked in Paths(blockedTrace)
        select outside.IsEmpty && blocked.IsEmpty;

    private static Fin<Seq<Loop>> Paths(PolygonTrace trace) => trace switch {
        PolygonTrace.Paths paths => Fin.Succ(paths.Result),
        PolygonTrace.Regions regions => Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary)),
        _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:path-trace").ToError()),
    };

    private static Seq<SharedEdge> Match(Seq<SharedEdge> candidates) =>
        candidates.OrderByDescending(static candidate => candidate.SharedLengthMm)
            .ThenBy(static candidate => candidate.A.Part.PartId)
            .ThenBy(static candidate => candidate.A.Part.Ordinal)
            .ToSeq()
            .Fold(Seq<SharedEdge>(), (accepted, candidate) => accepted.Exists(edge =>
                Conflicts(edge, candidate, candidate.Tolerance.Absolute.Value))
                ? accepted
                : accepted.Add(candidate));

    private static Fin<BidirectionalGraph<PartInstance, SEdge<PartInstance>>> Precedence(Seq<PlacedPart> placed) =>
        placed.Bind(outer => placed.Filter(inner => inner.SheetIndex == outer.SheetIndex && inner.Part != outer.Part)
                .Map(inner => (Outer: outer, Inner: inner)))
            .Traverse(pair => PolygonAlgebra.Apply(new PolygonOp.Contains(
                    pair.Outer.Region,
                    pair.Inner.Region.Bind(static loop => toSeq(loop.Vertices)).ToArr(),
                    PolygonFill.NonZero))
                .Bind(trace => trace is PolygonTrace.Related related
                        ? Fin.Succ((pair.Outer.Part, pair.Inner.Part,
                            Inside: related.Result.ForAll(static relation => relation != PointRelation.Outside)
                                && related.Result.Exists(static relation => relation == PointRelation.Inside)))
                    : Fin.Fail<(PartInstance Outer, PartInstance Inner, bool Inside)>(
                        new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:containment-trace").ToError()))
                .ToValidation()).As().ToFin().Bind(rows => {
                    BidirectionalGraph<PartInstance, SEdge<PartInstance>> graph = new(allowParallelEdges: false);
                    graph.AddVertexRange(placed.Map(static row => row.Part));
                    rows.Filter(static row => row.Inside).Iter(row => graph.AddEdge(new SEdge<PartInstance>(row.Inner, row.Outer)));
                    return graph.IsDirectedAcyclicGraph()
                        ? Fin.Succ(graph.ComputeTransitiveReduction())
                        : Fin.Fail<BidirectionalGraph<PartInstance, SEdge<PartInstance>>>(
                            new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:containment-cycle").ToError());
                });

    private static Fin<Seq<ChainRow>> Chains(
        Seq<PlacedPart> placed,
        Seq<SharedEdge> shared,
        Seq<LinkOp.Bridge> bridges,
        BidirectionalGraph<PartInstance, SEdge<PartInstance>> precedence,
        bool joinBands,
        CutLinkPolicy policy) {
        UndirectedGraph<PartInstance, SEdge<PartInstance>> islands = new(allowParallelEdges: false);
        islands.AddVertexRange(placed.Map(static row => row.Part));
        shared.Iter(edge => islands.AddEdge(new SEdge<PartInstance>(edge.A.Part, edge.B.Part)));
        if (joinBands) placed.Bind(left => placed.Filter(right => right.SheetIndex == left.SheetIndex && Compare(left.Part, right.Part) < 0
                && left.Measure.Centroid.DistanceTo(right.Measure.Centroid) <= policy.ChainBandMm)
            .Map(right => new SEdge<PartInstance>(left.Part, right.Part))).Iter(islands.AddEdge);
        Dictionary<PartInstance, int> labels = new();
        islands.ConnectedComponents(labels);
        Map<PartInstance, int> rank = toSeq(precedence.TopologicalSort()).Map((part, index) => (part, index)).ToMap();
        Seq<Seq<PlacedPart>> chunks = toSeq(placed.GroupBy(row => (row.SheetIndex, labels[row.Part])))
            .Bind(group => OrderBands(group.ToSeq(), shared, rank, policy)
                .Bind(members => Chunks(members, policy.MaxChainParts)))
            .OrderBy(chunk => chunk.Map(member => rank.Find(member.Part).IfNone(int.MaxValue)).Min())
            .ThenBy(static chunk => chunk.Head().SheetIndex).ToSeq();
        return chunks.Map((chunk, index) => (Chunk: chunk, Index: index))
            .Traverse(row => Chain(row.Chunk, row.Index, shared, bridges).ToValidation()).As().ToFin();
    }

    private static Seq<Seq<PlacedPart>> OrderBands(
        Seq<PlacedPart> members,
        Seq<SharedEdge> shared,
        Map<PartInstance, int> rank,
        CutLinkPolicy policy) {
        Set<PartInstance> ids = toSet(members.Map(static row => row.Part));
        UndirectedGraph<PartInstance, TaggedEdge<PartInstance, double>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(ids);
        members.Bind(left => members.Filter(right => Compare(left.Part, right.Part) < 0
                && left.Measure.Centroid.DistanceTo(right.Measure.Centroid) <= policy.ChainBandMm)
            .Map(right => new TaggedEdge<PartInstance, double>(
                left.Part, right.Part, left.Measure.Centroid.DistanceTo(right.Measure.Centroid))))
            .Iter(graph.AddEdge);
        shared.Filter(edge => ids.Contains(edge.A.Part) && ids.Contains(edge.B.Part))
            .Iter(edge => graph.AddEdge(new TaggedEdge<PartInstance, double>(edge.A.Part, edge.B.Part, edge.SharedLengthMm)));
        Dictionary<PartInstance, int> labels = new();
        graph.ConnectedComponents(labels);
        return toSeq(members.GroupBy(row => labels[row.Part]))
            .Map(group => OrderConnected(group.ToSeq(), graph, rank, policy));
    }

    private static Seq<PlacedPart> OrderConnected(
        Seq<PlacedPart> members,
        UndirectedGraph<PartInstance, TaggedEdge<PartInstance, double>> band,
        Map<PartInstance, int> rank,
        CutLinkPolicy policy) {
        Set<PartInstance> ids = toSet(members.Map(static row => row.Part));
        UndirectedGraph<PartInstance, TaggedEdge<PartInstance, double>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(ids);
        graph.AddEdgeRange(band.Edges.Filter(edge => ids.Contains(edge.Source) && ids.Contains(edge.Target)));
        Seq<TaggedEdge<PartInstance, double>> treeEdges = toSeq(graph.MinimumSpanningTreeKruskal(static edge => edge.Tag));
        UndirectedGraph<PartInstance, TaggedEdge<PartInstance, double>> tree = new(allowParallelEdges: false);
        tree.AddVertexRange(graph.Vertices);
        tree.AddEdgeRange(treeEdges);
        PartInstance root = members.OrderBy(row => rank.Find(row.Part).IfNone(int.MaxValue))
            .ThenBy(row => row.Measure.Centroid.DistanceTo(policy.RapidOrigin)).Head().Part;
        TryFunc<PartInstance, IEnumerable<TaggedEdge<PartInstance, double>>> paths = tree.TreeBreadthFirstSearch(root);
        return members.OrderBy(row => rank.Find(row.Part).IfNone(int.MaxValue))
            .ThenBy(row => PathCount(paths, row.Part)).ThenBy(static row => row.Part.PartId)
            .ThenBy(static row => row.Part.Ordinal).ToSeq();
    }

    private static int PathCount<TVertex, TEdge>(TryFunc<TVertex, IEnumerable<TEdge>> paths, TVertex target)
        where TEdge : IEdge<TVertex> =>
        paths(target, out IEnumerable<TEdge>? path) ? path.Count() : int.MaxValue;

    private static Seq<Seq<PlacedPart>> Chunks(Seq<PlacedPart> members, int size) =>
        toSeq(members.Map((member, index) => (Member: member, Chunk: index / size)).GroupBy(static row => row.Chunk))
            .Map(static group => group.Map(static row => row.Member).ToSeq());

    private static Fin<ChainRow> Chain(
        Seq<PlacedPart> members,
        int index,
        Seq<SharedEdge> shared,
        Seq<LinkOp.Bridge> bridges) {
        Set<PartInstance> ids = toSet(members.Map(static row => row.Part));
        Seq<SharedCut> cuts = shared.Filter(edge => ids.Contains(edge.A.Part) && ids.Contains(edge.B.Part))
            .Map(edge => new SharedCut(edge, bridges.Filter(bridge => bridge.Pair == edge)
                .Map(bridge => Window(edge.Cut, bridge.At, bridge.WidthMm))));
        Seq<(PartInstance Part, int Contour, Loop Path)> contours = members.Bind(member =>
            member.Region.Map((path, contour) => (member.Part, contour, path)));
        Map<(PartInstance Part, int Contour), int> vertices = contours
            .Map((contour, vertex) => ((contour.Part, contour.Contour), vertex)).ToMap();
        UndirectedGraph<int, SEdge<int>> topology = new(allowParallelEdges: false);
        topology.AddVertexRange(vertices.Values);
        cuts.Iter(cut => topology.AddEdge(new SEdge<int>(
            vertices[(cut.Edge.A.Part, cut.Edge.A.Contour)],
            vertices[(cut.Edge.B.Part, cut.Edge.B.Contour)])));
        Dictionary<int, int> labels = new();
        topology.ConnectedComponents(labels);
        Set<(PartInstance Part, int Contour)> pierces = toSet(contours.Map((contour, at) => (contour, at))
            .Filter(row => contours.Take(row.at).ForAll(prior => labels[vertices[(prior.Part, prior.Contour)]]
                != labels[vertices[(row.contour.Part, row.contour.Contour)]]))
            .Map(static row => (row.contour.Part, row.contour.Contour)));
        return members.Traverse(member => member.Region.Map((path, contour) => (path, contour))
                .Traverse(row => Cut(member.Part, row.contour, row.path, cuts,
                    pierces.Contains((member.Part, row.contour))).ToValidation())
                .As().ToFin()
                .Map(cutRows => new ChainMember(member.Part, cutRows, member.Region.Sum(static path => path.Length())))
                .ToValidation())
            .As().ToFin()
            .Map(rows => Row(index, members.Head().SheetIndex, rows, cuts));
    }

    private static ChainRow Row(int index, int sheet, Seq<ChainMember> rows, Seq<SharedCut> cuts) {
        double gap = cuts.Sum(static cut => cut.Gaps.Sum(window => window.Length(cut.Edge.SharedLengthMm)));
        return new ChainRow(
            index,
            sheet,
            rows.Bind(static member => member.Contours.Filter(static contour => contour.Pierce).Map(static contour => contour.Entry)),
            rows,
            cuts,
            Seq<Seq<Point3d>>(),
            rows.Sum(static row => row.CutLengthMm) - cuts.Sum(static cut => cut.Edge.SharedLengthMm) - gap);
    }

    // Posting leads at loop parameter zero, so the emitted path is rotated to start at the chosen entry and every
    // omitted span's segment index rotates with it; an entry is never placed on a span this contour does not cut.
    private static Fin<ContourCut> Cut(PartInstance part, int contour, Loop path, Seq<SharedCut> cuts, bool pierce) {
        Seq<OmittedSpan> omitted = cuts.Bind(cut => Omitted(part, contour, cut.Edge));
        Set<int> blocked = toSet(omitted.Map(static span => span.Segment));
        Seq<int> candidates = toSeq(Enumerable.Range(0, path.Count)).Filter(index => !blocked.Contains(index));
        int start = (candidates.IsEmpty ? toSeq(Enumerable.Range(0, path.Count)) : candidates)
            .OrderBy(index => path.At(index).Y).ThenBy(index => path.At(index).X).Head();
        return Rotate(path, start).Map(rotated => new ContourCut(
            contour,
            rotated,
            omitted.Map(span => span with { Segment = Wrap(span.Segment - start, path.Count) })
                .OrderBy(static span => span.Segment).ToSeq(),
            rotated.At(0),
            pierce));
    }

    private static Fin<Loop> Rotate(Loop loop, int start) => start == 0
        ? Fin.Succ(loop)
        : Loop.Admit(
            Range(0, loop.Count).Map(offset => loop.At(start + offset)).ToArr(),
            loop.Closed,
            Range(0, loop.Count).Map(offset => loop.BulgeAt(start + offset)).ToArr(),
            loop.Tolerance);

    private static int Wrap(int index, int count) => ((index % count) + count) % count;

    private static Seq<OmittedSpan> Omitted(PartInstance part, int contour, SharedEdge edge) =>
        edge.A.Part == part && edge.A.Contour == contour ? Seq(new OmittedSpan(edge.A.Segment, edge.WindowA))
            : edge.B.Part == part && edge.B.Contour == contour ? Seq(new OmittedSpan(edge.B.Segment, edge.WindowB))
            : Seq<OmittedSpan>();

    private static Seq<LinkOp.Bridge> Bridges(Seq<SharedEdge> shared, BridgePolicy policy) => policy.Switch(
        disabled: static _ => Seq<LinkOp.Bridge>(),
        spaced: row => shared.Bind(edge => {
            double available = edge.SharedLengthMm - (2.0 * row.EndClearanceMm) - row.WidthMm;
            int count = available < 0.0 ? 0 : 1 + (int)Math.Floor(available / row.SpacingMm);
            return toSeq(Enumerable.Range(0, count)).Map(slot => new LinkOp.Bridge(
                edge,
                Lerp(edge.Cut,
                    (row.EndClearanceMm + (0.5 * row.WidthMm) + (slot * row.SpacingMm)) / edge.SharedLengthMm),
                row.WidthMm));
        }));

    private static Fin<Seq<WasteRow>> Waste(Seq<PlacedPart> placed, LinkRun run, WastePolicy policy) =>
        run.StockBySheet.ToSeq().Traverse(row => Partition(
                row.Key,
                row.Value,
                placed.Filter(part => part.SheetIndex == row.Key),
                run.KeepOutBySheet.Find(row.Key).IfNone(Seq<Loop>()),
                policy).ToValidation())
            .As().ToFin()
            .Map(static rows => rows.Choose(identity));

    private static Fin<Option<WasteRow>> Partition(
        int sheet,
        Stock stock,
        Seq<PlacedPart> occupied,
        Seq<Loop> keepOut,
        WastePolicy policy) => PolygonAlgebra.Apply(new PolygonOp.Boolean(
                stock.Region,
                occupied.Bind(static part => part.Region).Concat(stock.Exclusions).Concat(keepOut),
                PolygonBoolean.Difference,
                PolygonFill.NonZero))
            .Bind(Paths)
            .Bind(usable => usable.IsEmpty
                ? Fin.Succ(Option<WasteRow>.None)
                : policy.Switch(
                    disabled: _ => Fin.Succ(Some(Safe(sheet, usable))),
                    voronoi: row => Seed(usable, row.SiteSpacingMm, row.MaxSites)
                        .Bind(sites => sites.Count < 2
                            ? Fin.Succ(Some(Safe(sheet, usable) with { Sites = sites.Count, Cells = sites.Count }))
                            : BuildVoronoi(sheet, usable, sites, row).Map(Some))));

    private static WasteRow Safe(int sheet, Seq<Loop> usable) => new(
        sheet,
        usable,
        Seq<Edge3>(),
        0,
        0,
        0,
        0.0,
        new AdjacencyGraph<int, TaggedEdge<int, double>>(allowParallelEdges: false),
        Map<int, Point3d>());

    private static Fin<Seq<Point3d>> Seed(Seq<Loop> usable, double spacing, int maxSites) {
        BoundingBox bounds = new(usable.Bind(static loop => toSeq(loop.Vertices)));
        int columns = (int)Math.Min(maxSites - 1L, Math.Max(1.0, Math.Ceiling((bounds.Max.X - bounds.Min.X) / spacing)));
        int rows = (int)Math.Min(maxSites - 1L, Math.Max(1.0, Math.Ceiling((bounds.Max.Y - bounds.Min.Y) / spacing)));
        long slots = (long)(columns + 1) * (rows + 1);
        Seq<Point3d> grid = Range(0, maxSites).TakeWhile(index => index < slots).Map(index => {
            int column = index / (rows + 1);
            int row = index % (rows + 1);
            return new Point3d(
                Math.Min(bounds.Max.X, bounds.Min.X + (column * spacing)),
                Math.Min(bounds.Max.Y, bounds.Min.Y + (row * spacing)),
                0.0);
        });
        Seq<Point3d> bounded = usable.Bind(static loop => toSeq(loop.Vertices)).Take(maxSites).Distinct()
            .Concat(grid).Distinct().Take(maxSites);
        return PolygonAlgebra.Apply(new PolygonOp.Contains(usable, bounded.ToArr(), PolygonFill.NonZero))
            .Bind(trace => trace is PolygonTrace.Related related
                ? Fin.Succ(toSeq(bounded.Zip(related.Result, static (point, relation) => (point, relation)))
                    .Choose(static row => row.relation == PointRelation.Outside ? None : Some(row.point)))
                : Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:waste-seed-trace").ToError()));
    }

    private static Fin<WasteRow> BuildVoronoi(
        int sheet,
        Seq<Loop> usable,
        Seq<Point3d> seeds,
        WastePolicy.Voronoi policy) {
        return from diagram in Try.lift(() => {
                   BoundingBox bounds = new(usable.Bind(static loop => toSeq(loop.Vertices)));
                   VoronoiPlane plane = new(bounds.Min.X, bounds.Min.Y, bounds.Max.X, bounds.Max.Y);
                   plane.SetSites(seeds.Map(static point => new VoronoiSite(point.X, point.Y)).ToList());
                   plane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
                   if (policy.Relaxations > 0)
                       plane.Relax(policy.Relaxations, policy.RelaxationStrength, reTessellate: true);
                   plane.MergeSites((left, right) => Math.Hypot(
                           left.Centroid.X - right.Centroid.X,
                           left.Centroid.Y - right.Centroid.Y) < policy.MergeDistanceMm
                       ? VoronoiSiteMergeDecision.MergeIntoSite1
                       : VoronoiSiteMergeDecision.DontMerge);
                   Seq<Edge3> raw = toSeq(plane.Edges)
                       .Filter(edge => edge.Left is not null && edge.Right is not null && edge.Length >= policy.MinEdgeMm)
                       .Map(edge => new Edge3(new Point3d(edge.Start.X, edge.Start.Y, 0.0),
                           new Point3d(edge.End.X, edge.End.Y, 0.0)));
                   Seq<Loop> closed = toSeq(plane.Sites).Filter(static site => site.Closed)
                       .Map(static site => toSeq(site.ClockwisePoints)
                           .Map(static point => new Point3d(point.X, point.Y, 0.0)).ToArr())
                       .Filter(static ring => ring.Count >= 3)
                       .Choose(ring => Loop.Admit(ring, closed: true, Arr<double>(), usable.Head().Tolerance).ToOption());
                   return (Raw: raw, Closed: closed, Sites: plane.Sites.Count, Duplicates: plane.DuplicateCount);
               }).Run()
               from fragment in Fragmented(diagram.Closed, usable, policy.MinReusableAreaMm2)
               from clipped in PolygonAlgebra.Apply(new PolygonOp.ClipOpen(diagram.Raw.Map(Seq1), usable, PolygonFill.NonZero))
               from split in Split(clipped)
               select RouteGraph(
                   sheet, usable, split.Inside, diagram.Sites, diagram.Closed.Count, diagram.Duplicates, fragment);
    }

    // Partitioning trades one large offcut for many cells: the cells that land under the reusable floor are the
    // material the cut-up actually destroys, so the objective weighs them beside the kerf it spends.
    private static Fin<double> Fragmented(Seq<Loop> cells, Seq<Loop> usable, double floorMm2) =>
        cells.Traverse(cell => PolygonAlgebra.Apply(new PolygonOp.Boolean(
                    Seq(cell), usable, PolygonBoolean.Intersection, PolygonFill.NonZero))
                .Bind(Paths)
                .Bind(region => region.IsEmpty
                    ? Fin.Succ(0.0)
                    : PolygonAlgebra.Apply(new PolygonOp.Measure(region, PolygonFill.NonZero))
                        .Bind(static trace => trace is PolygonTrace.Measured measured
                            ? Fin.Succ(measured.Result.FilledArea)
                            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:cell-measure-trace").ToError())))
                .ToValidation())
            .As().ToFin()
            .Map(areas => areas.Filter(area => area > 0.0 && area < floorMm2).Sum());

    private static Fin<(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside)> Split(PolygonTrace trace) =>
        trace is PolygonTrace.SplitRuns split
            ? Fin.Succ((split.Inside, split.Outside))
            : Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:split-trace").ToError());

    private static WasteRow RouteGraph(
        int sheet,
        Seq<Loop> usable,
        Seq<Seq<Edge3>> paths,
        int sites,
        int cells,
        int duplicateSites,
        double fragmentAreaMm2) {
        Seq<Edge3> cuts = paths.Bind(identity);
        Seq<Point3d> points = cuts.Bind(static edge => Seq(edge.A, edge.B)).Distinct();
        Dictionary<Point3d, int> ids = points.Map((point, index) => (point, index))
            .ToDictionary(static row => row.point, static row => row.index);
        Map<int, Point3d> nodes = points.Map((point, index) => (index, point)).ToMap();
        AdjacencyGraph<int, TaggedEdge<int, double>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(nodes.Keys);
        cuts.Iter(edge => {
            int a = ids[edge.A], b = ids[edge.B];
            double length = edge.A.DistanceTo(edge.B);
            graph.AddEdge(new TaggedEdge<int, double>(a, b, length));
            graph.AddEdge(new TaggedEdge<int, double>(b, a, length));
        });
        return new WasteRow(sheet, usable, cuts, sites, cells, duplicateSites, fragmentAreaMm2, graph, nodes);
    }

    private static Fin<Seq<ChainRow>> Route(Seq<ChainRow> chains, Seq<WasteRow> partitions, CutLinkPolicy policy) =>
        chains.GroupBy(static chain => chain.SheetIndex).OrderBy(static group => group.Key)
            .Traverse(group => RouteSheet(
                group.ToSeq(),
                partitions.Find(row => row.SheetIndex == group.Key),
                policy.RapidOrigin,
                policy.Waste.Switch(disabled: static _ => 1, voronoi: static row => row.RapidProbeNodes)).ToValidation())
            .As().ToFin().Map(static rows => rows.Bind(identity));

    private static Fin<Seq<ChainRow>> RouteSheet(
        Seq<ChainRow> chains,
        Option<WasteRow> partition,
        Point3d cursor,
        int probes) => chains.Fold(
            Fin.Succ((Cursor: cursor, Rows: Seq<ChainRow>())),
            (effect, chain) =>
                from state in effect
                from routed in chain.Pierces.Fold(
                    Fin.Succ((Cursor: state.Cursor, Paths: Seq<Seq<Point3d>>())),
                    (route, pierce) =>
                        from prior in route
                        from path in partition.Map(row => RapidPath(row, prior.Cursor, pierce, probes))
                            .IfNone(Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(
                                Kind.Polyline, -1, "link:rapid-usable-region").ToError()))
                        select (Cursor: pierce, Paths: prior.Paths.Add(path)))
                select (routed.Cursor, Rows: state.Rows.Add(chain with { RapidPaths = routed.Paths })))
            .Map(static state => state.Rows);

    // A leg that never leaves the waste region is already optimal, so the partition detour and its per-node
    // visibility clips only run when the direct leg would cross a cut part.
    private static Fin<Seq<Point3d>> RapidPath(WasteRow partition, Point3d from, Point3d to, int probes) =>
        from direct in Visible(partition.Usable, from, to)
        from path in direct
            ? Fin.Succ(Seq(from, to))
            : partition.Nodes.IsEmpty
                ? Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:rapid-blocked").ToError())
                : from starts in VisibleNodes(partition, from, probes)
                  from ends in VisibleNodes(partition, to, probes)
                  from route in starts.Bind(start => ends.Choose(end => GraphPath(partition, start, end)))
                      .OrderBy(Tour).HeadOrNone()
                      .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:rapid-route").ToError())
                  select Seq(from).Concat(route).Add(to)
        select path;

    private static Fin<Seq<int>> VisibleNodes(WasteRow partition, Point3d point, int probes) =>
        partition.Nodes.ToSeq().OrderBy(row => row.Value.DistanceTo(point)).Take(probes).ToSeq()
            .Traverse(row => Visible(partition.Usable, point, row.Value)
                .Map(clear => (row.Key, Clear: clear)).ToValidation())
            .As().ToFin()
            .Bind(static rows => rows.Filter(static row => row.Clear).Map(static row => row.Key) is { IsEmpty: false } visible
                ? Fin.Succ(visible)
                : Fin.Fail<Seq<int>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "link:rapid-connector").ToError()));

    private static Fin<bool> Visible(Seq<Loop> usable, Point3d first, Point3d second) =>
        first.DistanceTo(second) <= usable.Head().Tolerance.Absolute.Value
            ? Fin.Succ(true)
            : PolygonAlgebra.Apply(new PolygonOp.ClipOpen(
                    Seq(Seq(new Edge3(first, second))), usable, PolygonFill.NonZero))
                .Bind(Split)
                .Map(split => split.Outside.Bind(identity)
                    .Sum(static edge => edge.A.DistanceTo(edge.B)) <= usable.Head().Tolerance.Absolute.Value);

    private static Option<Seq<Point3d>> GraphPath(WasteRow partition, int source, int target) {
        if (source == target) return Some(Seq(partition.Nodes[source]));
        TryFunc<int, IEnumerable<TaggedEdge<int, double>>> paths = partition.Routes.ShortestPathsAStar(
            static edge => edge.Tag,
            vertex => partition.Nodes[vertex].DistanceTo(partition.Nodes[target]),
            source);
        return paths(target, out IEnumerable<TaggedEdge<int, double>>? route)
            ? Some(Seq(partition.Nodes[source]).Concat(toSeq(route).Map(edge => partition.Nodes[edge.Target])))
            : None;
    }

    private static LinkEvidence Evidence(
        Seq<ChainRow> chains,
        Seq<SharedEdge> shared,
        Seq<LinkOp.Bridge> bridges,
        Seq<WasteRow> partitions,
        LinkRun run) {
        double rapid = chains.Bind(static chain => chain.RapidPaths).Sum(Tour);
        double partition = partitions.Bind(static row => row.Cuts).Sum(static edge => edge.A.DistanceTo(edge.B));
        double cut = chains.Sum(static chain => chain.CutLengthMm) + partition;
        double bridge = bridges.Sum(static row => row.WidthMm);
        double continuous = chains.Map(static chain => chain.CutLengthMm)
            .Concat(partitions.Map(static row => row.Cuts.Sum(static edge => edge.A.DistanceTo(edge.B))))
            .OrderByDescending(static value => value).HeadOrNone().IfNone(0.0);
        double heat = continuous / run.Policy.MaxContinuousCutMm;
        double quality = shared.Sum(edge => run.Policy.MatchToleranceMm / edge.SharedLengthMm) + bridges.Count;
        // Remnant loss lives entirely in the waste region: kerf swept by the partition cuts and the cells that
        // fall under the reusable floor. Common-line and bridge spans run between parts, never through the offcut,
        // so crediting them here would subtract part-side savings from an offcut-side cost and can read negative.
        double remnant = (partition * run.Policy.CutWidthMm) + partitions.Sum(static row => row.FragmentAreaMm2);
        return new LinkEvidence(chains.Sum(static chain => chain.Pierces.Count), rapid, cut,
            shared.Sum(static edge => edge.SharedLengthMm), bridge,
            partition, heat, quality, remnant);
    }

    private static bool Conflicts(SharedEdge accepted, SharedEdge candidate, double tolerance) =>
        (accepted.A == candidate.A && accepted.WindowA.Overlaps(candidate.WindowA, tolerance / accepted.SpanAmm))
        || (accepted.A == candidate.B && accepted.WindowA.Overlaps(candidate.WindowB, tolerance / accepted.SpanAmm))
        || (accepted.B == candidate.A && accepted.WindowB.Overlaps(candidate.WindowA, tolerance / accepted.SpanBmm))
        || (accepted.B == candidate.B && accepted.WindowB.Overlaps(candidate.WindowB, tolerance / accepted.SpanBmm));

    private static SegmentWindow Window(Edge3 edge, Point3d at, double width) {
        double length = edge.A.DistanceTo(edge.B);
        double center = edge.A.DistanceTo(at) / length;
        double half = 0.5 * width / length;
        return new SegmentWindow(Math.Max(0.0, center - half), Math.Min(1.0, center + half));
    }

    private static bool Near(BoundingBox left, BoundingBox right, double band) =>
        left.Min.X - band <= right.Max.X && right.Min.X - band <= left.Max.X
        && left.Min.Y - band <= right.Max.Y && right.Min.Y - band <= left.Max.Y;

    private static bool Forward(Point3d first, Point3d second) =>
        first.X < second.X || (first.X == second.X && first.Y < second.Y);

    private static int Compare(PartInstance left, PartInstance right) =>
        left.PartId != right.PartId ? left.PartId.CompareTo(right.PartId) : left.Ordinal.CompareTo(right.Ordinal);

    private static double Dist(Point3d first, Point3d second, Point3d point) {
        Vector3d direction = second - first;
        return Vector3d.CrossProduct(direction, point - first).Length / direction.Length;
    }

    private static Point3d Project(Point3d point, Point3d origin, Vector3d direction) =>
        origin + ((((point - origin) * direction) / (direction * direction)) * direction);

    private static Point3d Lerp(Edge3 edge, double parameter) => edge.A + (parameter * (edge.B - edge.A));

    private static double Tour(Seq<Point3d> points) =>
        points.Tail.Fold((At: points.Head(), Length: 0.0),
            static (state, point) => (point, state.Length + state.At.DistanceTo(point))).Length;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
