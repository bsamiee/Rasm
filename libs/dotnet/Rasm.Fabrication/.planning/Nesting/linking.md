# [RASM_FABRICATION_LINKING]

`Linking` owns post-placement cut topology. It preserves every placement transform, converts common boundaries into one shared cut with explicit source-contour omissions, threads physical instances under containment precedence, retains bridge gaps as cut evidence, and derives waste partitions from bounded point-site Voronoi adjacency.

`LinkPlan` carries one objective verdict whose empty `Applied` sequence is the measured baseline. `ContourCut.Pierce` marks one entry per connected cut component, `ContourCut.Path` starts at that entry so posting leads at loop parameter zero, and `OmittedSpan` names the segment each shared window removes.

## [01]-[INDEX]

- [02]-[CUT_LINKING]: owns cut-graph admission, common-line matching, precedence-safe chains, bridge gaps, point-site waste partitioning, objective selection, and posting topology.

## [02]-[CUT_LINKING]

- Owner: `LinkRun` is the generated ingress owner; `CutLinkPolicy` and `CutLinkObjective` are generated policy values; `LinkOp` closes applied edit evidence; `LinkVerdict` carries one baseline-or-applied payload; `CommonLine` owns the ONE collinear-overlap measure the package reads.
- Law: `CommonLine` owns the package's single collinearity kernel and its single disjointness fold — `CommonLine.Share` decides every pair under a `CommonLineBudget` the caller states, `CommonLine.Disjoint` resolves every longest-first claim, and this page's own matching composes both, so a second walk or a second claim rule anywhere is the deleted form. Separation reads the `Geometry2D` clamped endpoint gap, so two segments whose infinite lines run the right distance apart while their bodies pass each other never pair. The result carries the pierce census beside the overlap, which is what makes "fewer pierces at equal yield" readable rather than inferred.
- Law: six objective weights fan onto one comparable number and every term reaches it DIMENSIONLESS. `LinkBasis` carries the characteristic length, the sheet area, and the pierce count the terms divide through, derived once per plan from the admitted run and threaded on the scoring INPUT — never on `LinkEvidence`, which is the comparison's published result. Summing raw counts, millimetres, and square millimetres let the sheet's own scale decide the ranking before any weight was read.
- Law: candidate pairing is a RADIUS query over one spatial index of segment midpoints, each primitive inflated by the pairing radius plus the admitted maximum segment span. The parts-by-contours-by-segments cross product a bounding-box prefilter still admitted inside one sheet is the deleted form: a common line is a local relation and the index answers locality.
- Law: route nodes key on the QUANTIZED station, never a raw `Point3d`. Two cut endpoints meeting within the admitted tolerance are ONE node, and exact-float keying forked them at exactly the junctions a rapid must traverse — so both the dedup and the node index read the one quantized key.
- Cases: `LinkCapability` selects common-line and chain scheduling as one `CapabilitySet` column; `BridgeSpacing` and `WasteVoronoi` ride PRESENCE — an absent policy IS the disabled lane, so a `Disabled` case and an absent column never answer one question twice.
- Law: `CutLinkPolicy` carries typed measures — `Length` on every span, `Ratio` on the miter limit — and reads its match, angular, and arc budgets off the admitted `Context` lanes, because `ToleranceLane` owns every band it derives and a policy column beside a lane is a copy that drifts from it.
- Entry: `Plan(LinkRun)` admits placement, plane-compatible polygonal profiles, sheet stock, sheet-scoped keep-outs, and policy once before one composed graph fold.
- Auto: a longest-first conflict fold selects non-overlapping common lines; `ConnectedComponents`, transitive reduction, topological order, Kruskal forest, breadth-first paths, and A-star routes derive chain and rapid topology; `PolygonOp.Cells` derives bounded point-site waste adjacency with its shared segments and each closed cell ring measures against the reusable floor; `PolygonAlgebra.Apply` owns line-space offset, Boolean, measure, relation, and open clip.
- Law: no tour search seats on this page and none is owed. Chain order is PRECEDENCE order — containment reduced to its transitive core, topologically sorted, tie-broken by Kruskal-tree reach from the seat nearest the rapid origin — so reordering chains to shorten travel emits an inner contour before the outer part it drops out of. `RouteSheet` therefore folds a FIXED sequence and one search remains, the per-leg detour, already `ShortestPathsAStar` over the partition's own visibility graph. Any traveling-salesman pass over these nodes optimizes an order the precedence graph never leaves free.
- Result: `LinkComparison` carries pierce, rapid, cut, shared, bridge, partition, heat, quality, and remnant-loss axes before and after the candidate topology; remnant loss combines partition kerf area with sub-floor cell area as offcut-side costs.
- Packages: `LanguageExt.Core`, `QuikGraph` (`ConnectedComponents`, `ComputeTransitiveReduction`, `TopologicalSort`, `MinimumSpanningTreeKruskal`, `TreeBreadthFirstSearch`, `ShortestPathsAStar`, `IsDirectedAcyclicGraph` — every graph question on this page is one of these operators, and no hand walk stands beside one), `Thinktecture.Runtime.Extensions`, `Rasm` (`JoinType`/`EndType`/`BooleanOp`/`OffsetPolicy`, `ICapability`/`CapabilitySet`, `Context`/`ToleranceLane`, `PositiveMagnitude`), `UnitsNet` (`Length`, `Area`, `Ratio` on the policy's own spans), `RhinoCommon`, and the `Geometry2D` owners compose the surface.
- Growth: a cut edit is one `LinkOp` case; a scoring axis is one `LinkEvidence` member with one `CutLinkObjective` weight; a second waste decomposition turns `WasteVoronoi` into the union over its own payloads while the `Option` that routes enablement stays untouched; no consumer gains an orchestration step.
- Boundary: `ChainRow.SheetIndex`, `Instances`, `SourceParts`, `Pierces`, `Members`, `Shared`, and `RapidPaths` form the posting boundary, and `ContourCut.Path` is entry-rotated so a consumer leads at parameter zero without re-deriving the entry; mutable `QuikGraph` construction is the one statement-bearing site, and the waste diagram is never minted here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LinkCapability : ICapability<LinkCapability> {
    public static readonly LinkCapability CommonLine = new("common-line");
    public static readonly LinkCapability Chain = new("chain");
}

public readonly record struct BridgeSpacing(Length Width, Length Spacing, Length EndClearance) {
    public bool Valid =>
        Width.Millimeters > 0.0 && double.IsFinite(Width.Millimeters)
        && Spacing.Millimeters > Width.Millimeters && double.IsFinite(Spacing.Millimeters)
        && EndClearance.Millimeters >= 0.0 && double.IsFinite(EndClearance.Millimeters);
}

public readonly record struct WasteVoronoi(
    Length SiteSpacing,
    int MaxSites,
    int Relaxations,
    Ratio RelaxationStrength,
    Length MergeDistance,
    Length MinEdge,
    Area MinReusable,
    int RapidProbeNodes) {
    public bool Valid =>
        SiteSpacing.Millimeters > 0.0 && double.IsFinite(SiteSpacing.Millimeters)
        && MaxSites >= 2 && Relaxations >= 0
        && RelaxationStrength.DecimalFractions is > 0.0 and <= 1.0
        && MergeDistance.Millimeters >= 0.0 && double.IsFinite(MergeDistance.Millimeters)
        && MinEdge.Millimeters > 0.0 && double.IsFinite(MinEdge.Millimeters)
        && MinReusable.SquareMillimeters >= 0.0 && double.IsFinite(MinReusable.SquareMillimeters)
        && RapidProbeNodes >= 1;
}

[ComplexValueObject]
public sealed partial class CutLinkObjective {
    public double Pierce { get; }
    public double Rapid { get; }
    public double Cut { get; }
    public double Heat { get; }
    public double Quality { get; }
    public double Remnant { get; }

    public double Score(LinkEvidence evidence, LinkBasis basis) =>
        (evidence.Pierces / basis.Pierces * Pierce)
        + (evidence.RapidMm / basis.LengthMm * Rapid)
        + (evidence.CutMm / basis.LengthMm * Cut)
        + (evidence.HeatLoad * Heat)
        + (evidence.QualityRisk / basis.Pierces * Quality)
        + (evidence.RemnantLossMm2 / basis.AreaMm2 * Remnant);

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
            : new ValidationError("link:objective-weights");
    }
}

[ComplexValueObject]
public sealed partial class CutLinkPolicy {
    public CapabilitySet<LinkCapability> Enabled { get; }
    public Length CutWidth { get; }
    public Ratio ClearanceMiterLimit { get; }
    public Length MinSharedLength { get; }

    public Length MaxSegmentSpan { get; }
    public int MaxChainParts { get; }
    public Length ChainBand { get; }
    public Length MaxContinuousCut { get; }
    public Point3d RapidOrigin { get; }
    public Context Tolerance { get; }
    public Option<BridgeSpacing> Bridge { get; }
    public Option<WasteVoronoi> Waste { get; }
    public CutLinkObjective Objective { get; }

    public double MatchToleranceMm => Tolerance.For(ToleranceLane.Match).Value;
    public double AngularToleranceRadians => Tolerance.For(ToleranceLane.Angle).Value;
    public double ArcToleranceMm => Tolerance.For(ToleranceLane.Arc).Value;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<LinkCapability> enabled,
        ref Length cutWidth,
        ref Ratio clearanceMiterLimit,
        ref Length minSharedLength,
        ref Length maxSegmentSpan,
        ref int maxChainParts,
        ref Length chainBand,
        ref Length maxContinuousCut,
        ref Point3d rapidOrigin,
        ref Context tolerance,
        ref Option<BridgeSpacing> bridge,
        ref Option<WasteVoronoi> waste,
        ref CutLinkObjective objective) {
        double[] spans = [cutWidth.Millimeters, minSharedLength.Millimeters, maxSegmentSpan.Millimeters,
            chainBand.Millimeters, maxContinuousCut.Millimeters, clearanceMiterLimit.DecimalFractions];
        validationError = spans.All(static value => double.IsFinite(value) && value > 0.0)
            && tolerance.IsValid
            && tolerance.For(ToleranceLane.Angle).Value < Math.PI / 2.0
            && tolerance.For(ToleranceLane.Match).Value > 0.0
            && tolerance.For(ToleranceLane.Arc).Value > 0.0
            && maxChainParts > 0 && rapidOrigin.IsValid
            && bridge.ForAll(static row => row.Valid) && waste.ForAll(static row => row.Valid)
                ? null
                : new ValidationError("link:link-policy");
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
                ? new ValidationError("link:link-run")
                : null;
    }

    static bool Polygon(Loop loop, Context tolerance, Plane plane) =>
        loop.Closed && loop.Count >= 3 && loop.Tolerance == tolerance && loop.Plane.Equals(plane);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SegmentWindow(double Start, double End) {
    public double Length(double span) => (End - Start) * span;
    public bool Overlaps(SegmentWindow other, double tolerance) => Math.Min(End, other.End) - Math.Max(Start, other.Start) > tolerance;
}

public readonly record struct SegmentRef(PartInstance Part, int Contour, int Segment);
public sealed record SharedEdge(
    int SheetIndex,
    SegmentRef A,
    SegmentRef B,
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
        double FragmentAreaMm2) : LinkOp;
}

public readonly record struct LinkBasis(double LengthMm, double AreaMm2, double Pierces) {
    public static LinkBasis Of(LinkRun run) {
        double area = Math.Max(run.StockBySheet.Values.Sum(static stock => stock.Facts.AreaMm2), double.Epsilon);
        return new LinkBasis(Math.Sqrt(area), area, Math.Max(run.Placement.Parts.Count, 1));
    }
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
    double FragmentAreaMm2,
    AdjacencyGraph<int, TaggedEdge<int, double>> Routes,
    Map<int, Point3d> Nodes,
    bool Routed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Linking {
    public static Fin<LinkPlan> Plan(LinkRun run) =>
        from placed in Place(run)
        from candidates in Candidates(placed, run)
        from clear in candidates.Traverse(edge => Clears(edge, placed, run).Map(ok => (Edge: edge, Clear: ok)).ToValidation()).As().ToFin()
        let selected = run.Policy.Enabled.Admits(LinkCapability.CommonLine)
            ? Match(clear.Filter(static row => row.Clear).Map(static row => row.Edge))
            : Seq<SharedEdge>()
        from precedence in Precedence(placed)
        let bridges = Bridges(selected, run.Policy.Bridge)
        from baseline in Chains(placed, Seq<SharedEdge>(), Seq<LinkOp.Bridge>(), precedence, false, run.Policy)
        from optimized in run.Policy.Enabled.Admits(LinkCapability.Chain) || !selected.IsEmpty
            ? Chains(placed, selected, bridges, precedence, run.Policy.Enabled.Admits(LinkCapability.Chain), run.Policy)
            : Fin.Succ(baseline)
        from safe in Waste(placed, run, None)
        from waste in Waste(placed, run, run.Policy.Waste)
        from baseRouted in Route(baseline, safe, run.Policy)
        from routed in Route(optimized, waste, run.Policy)
        let appliedShared = routed.Bind(static chain => chain.Shared.Map(static cut => cut.Edge)).Distinct()
        let appliedBridges = bridges.Filter(bridge => appliedShared.Contains(bridge.Pair))
        let baseEvidence = Evidence(baseRouted, Seq<SharedEdge>(), Seq<LinkOp.Bridge>(), safe, run)
        let nextEvidence = Evidence(routed, appliedShared, appliedBridges, waste, run)
        let basis = LinkBasis.Of(run)
        let comparison = new LinkComparison(
            baseEvidence,
            nextEvidence,
            run.Policy.Objective.Score(baseEvidence, basis),
            run.Policy.Objective.Score(nextEvidence, basis))
        let operations = appliedShared.Map(static edge => (LinkOp)new LinkOp.CommonLine(edge))
            .Concat(routed.Map(static chain => (LinkOp)new LinkOp.ChainCut(chain)))
            .Concat(appliedBridges.Map(static bridge => (LinkOp)bridge))
            .Concat(waste.Filter(static row => !row.Cuts.IsEmpty)
                .Map(static row => (LinkOp)new LinkOp.WasteCutUp(
                    row.SheetIndex, row.Cuts, row.Sites, row.Cells, row.FragmentAreaMm2)))
        select comparison.ScoreAfter < comparison.ScoreBefore && !operations.IsEmpty
            ? new LinkPlan(new LinkVerdict(operations, routed), comparison)
            : new LinkPlan(new LinkVerdict(Seq<LinkOp>(), baseRouted),
                comparison with { After = baseEvidence, ScoreAfter = comparison.ScoreBefore });

    private static Fin<Seq<PlacedPart>> Place(LinkRun run) =>
        run.Placement.Parts.Traverse(transform => (
            from region in run.Profiles[transform.PartId].Traverse(transform.Apply).As()
            from trace in PolygonAlgebra.Apply(new PolygonOp.Topology(region, PolygonFill.NonZero))
            from topology in trace.Regioned(
                new KernelFault.InvalidValue("linking", "link:placement-topology"))
            let ordered = toSeq(topology.Nodes.OrderByDescending(static node => node.Depth)
                .ThenBy(static node => Math.Abs(node.SignedArea)).Select(static node => node.Boundary))
            from measure in Measure(ordered)
            select new PlacedPart(
                new PartInstance(transform.PartId, transform.Instance), transform.SheetIndex, ordered, measure))
            .ToValidation()).As().ToFin();

    private static Fin<PolygonMeasure> Measure(Seq<Loop> region) =>
        PolygonAlgebra.Apply(new PolygonOp.Measure(region, PolygonFill.NonZero))
            .Bind(static trace => trace.Measure(
                new KernelFault.InvalidValue("linking", "link:measure-trace")));

    private static Fin<Seq<SharedEdge>> Candidates(Seq<PlacedPart> placed, LinkRun run) {
        Seq<SegmentSite> sites = placed.Bind(part => part.Region
            .Map((loop, contour) => (loop, contour))
            .Bind(row => toSeq(Enumerable.Range(0, row.loop.Count))
                .Filter(segment => row.loop.BulgeAt(segment) == 0.0)
                .Map(segment => new SegmentSite(part, row.loop, row.contour, segment,
                    row.loop.At(segment) + (0.5 * (row.loop.At(segment + 1) - row.loop.At(segment)))))));
        double radius = (0.5 * run.Policy.CutWidth.Millimeters) + run.Policy.MatchToleranceMm
            + run.Policy.MaxSegmentSpan.Millimeters;
        return SpatialIndex.Build(SpatialKind.Bvh, sites.Map(site => Ball(site.Midpoint, radius)).ToArray(), BuildPolicy.Canonical)
            .Bind(index => sites
                .Map((site, ordinal) => (site, ordinal))
                .Traverse(row => index
                    .Query(Ball(row.site.Midpoint, radius), Some(new Sphere(row.site.Midpoint, radius)))
                    .Map(found => found
                        .Filter(other => other > row.ordinal)
                        .Map(other => (Left: row.site, Right: sites[other])))
                    .ToValidation())
                .As().ToFin())
            .Map(pairs => toSeq(pairs
                .Bind(identity)
                .Filter(static pair => pair.Left.Part.SheetIndex == pair.Right.Part.SheetIndex
                    && Compare(pair.Left.Part.Part, pair.Right.Part.Part) < 0)
                .Bind(pair => Pair(pair.Left.Part, pair.Left.Loop, pair.Left.Contour, pair.Left.Segment,
                    pair.Right.Part, pair.Right.Loop, pair.Right.Contour, pair.Right.Segment, run.Policy).ToSeq())
                .OrderByDescending(static edge => edge.SharedLengthMm)
                .ThenBy(static edge => edge.A.Part.PartId)
                .ThenBy(static edge => edge.A.Part.Ordinal)
                .ThenBy(static edge => edge.A.Contour)
                .ThenBy(static edge => edge.A.Segment)));
    }

    private readonly record struct SegmentSite(PlacedPart Part, Loop Loop, int Contour, int Segment, Point3d Midpoint);

    private static BoundingBox Ball(Point3d at, double radius) => new(
        new Point3d(at.X - radius, at.Y - radius, at.Z - radius),
        new Point3d(at.X + radius, at.Y + radius, at.Z + radius));

    private static Option<SharedEdge> Pair(
        PlacedPart a,
        Loop left,
        int leftContour,
        int leftEdge,
        PlacedPart b,
        Loop right,
        int rightContour,
        int rightEdge,
        CutLinkPolicy policy) =>
        CommonLine.Share(left, leftEdge, right, rightEdge, new CommonLineBudget(
                policy.CutWidth.Millimeters,
                policy.MatchToleranceMm,
                policy.AngularToleranceRadians,
                policy.MinSharedLength.Millimeters))
            .Map(span => new SharedEdge(
                a.SheetIndex,
                new SegmentRef(a.Part, leftContour, leftEdge),
                new SegmentRef(b.Part, rightContour, rightEdge),
                span.WindowA,
                span.WindowB,
                span.SpanAmm,
                span.SpanBmm,
                span.Cut,
                span.Cut.A.DistanceTo(span.Cut.B),
                left.Tolerance));

    private static Fin<bool> Clears(SharedEdge edge, Seq<PlacedPart> placed, LinkRun run) =>
        from segment in Loop.Admit(Arr(edge.Cut.A, edge.Cut.B), closed: false, Arr<double>(), edge.Tolerance)
        from context in Context.Canonical.Override(
            ToleranceLane.Arc, run.Policy.ArcToleranceMm, UnitsNet.Units.LengthUnit.Millimeter)
        let policy = OffsetPolicy.Of(context) with {
            MiterLimit = PositiveMagnitude.Create(run.Policy.ClearanceMiterLimit.DecimalFractions) }
        from clearanceTrace in PolygonAlgebra.Apply(new PolygonOp.Offset(
            Seq(segment),
            new OffsetField.Uniform((0.5 * run.Policy.CutWidth.Millimeters) + run.Policy.MatchToleranceMm),
            JoinType.Square,
            EndType.Square,
            policy))
        from clearance in Paths(clearanceTrace)
        from stock in run.StockBySheet.Find(edge.SheetIndex)
            .ToFin(new KernelFault.InvalidValue("linking", $"link:sheet:{edge.SheetIndex}"))
        from outsideTrace in PolygonAlgebra.Apply(new PolygonOp.Boolean(
            clearance, stock.Region, BooleanOp.Difference, PolygonFill.NonZero))
        from outside in Paths(outsideTrace)
        from blockedTrace in PolygonAlgebra.Apply(new PolygonOp.Boolean(
            clearance,
            placed.Filter(row => row.SheetIndex == edge.SheetIndex
                    && row.Part != edge.A.Part && row.Part != edge.B.Part)
                .Bind(static row => row.Region).Concat(stock.Exclusions)
                .Concat(run.KeepOutBySheet.Find(edge.SheetIndex).IfNone(Seq<Loop>())),
            BooleanOp.Intersection,
            PolygonFill.NonZero))
        from blocked in Paths(blockedTrace)
        select outside.IsEmpty && blocked.IsEmpty;

    private static Fin<Seq<Loop>> Paths(PolygonTrace trace) =>
        trace.Loops(new KernelFault.InvalidValue("linking", "link:path-trace"));

    private static Fin<(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside)> Split(PolygonTrace trace) =>
        trace.Runs(new KernelFault.InvalidValue("linking", "link:split-trace"));

    private static Seq<SharedEdge> Match(Seq<SharedEdge> candidates) =>
        CommonLine.Disjoint(
            toSeq(candidates.OrderByDescending(static candidate => candidate.SharedLengthMm)
                .ThenBy(static candidate => candidate.A.Part.PartId)
                .ThenBy(static candidate => candidate.A.Part.Ordinal)),
            static (taken, candidate) => Conflicts(taken, candidate, candidate.Tolerance.Absolute.Value));

    private static Fin<BidirectionalGraph<PartInstance, SEdge<PartInstance>>> Precedence(Seq<PlacedPart> placed) =>
        placed.Bind(outer => placed.Filter(inner => inner.SheetIndex == outer.SheetIndex && inner.Part != outer.Part)
                .Map(inner => (Outer: outer, Inner: inner)))
            .Traverse(pair => PolygonAlgebra.Apply(new PolygonOp.Contains(
                    pair.Outer.Region,
                    pair.Inner.Region.Bind(static loop => toSeq(loop.Vertices)).ToArr(),
                    PolygonFill.NonZero))
                .Bind(trace => trace
                    .Relations(new KernelFault.InvalidValue("linking", "link:containment-trace"))
                    .Map(relations => (pair.Outer.Part, pair.Inner.Part,
                        Inside: relations.ForAll(static relation => relation != PointRelation.Outside)
                            && relations.Exists(static relation => relation == PointRelation.Inside))))
                .ToValidation()).As().ToFin().Bind(rows => {
                    BidirectionalGraph<PartInstance, SEdge<PartInstance>> graph = new(allowParallelEdges: false);
                    graph.AddVertexRange(placed.Map(static row => row.Part));
                    rows.Filter(static row => row.Inside).Iter(row => graph.AddEdge(new SEdge<PartInstance>(row.Inner, row.Outer)));
                    return graph.IsDirectedAcyclicGraph()
                        ? Fin.Succ(graph.ComputeTransitiveReduction())
                        : Fin.Fail<BidirectionalGraph<PartInstance, SEdge<PartInstance>>>(
                            new GeometryFault.DegenerateInput(Kind.Polyline, None, "link:containment-cycle"));
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
                && left.Measure.Centroid.DistanceTo(right.Measure.Centroid) <= policy.ChainBand.Millimeters)
            .Map(right => new SEdge<PartInstance>(left.Part, right.Part))).Iter(islands.AddEdge);
        Dictionary<PartInstance, int> labels = new();
        islands.ConnectedComponents(labels);
        Map<PartInstance, int> rank = toSeq(precedence.TopologicalSort()).Map((part, index) => (part, index)).ToMap();
        Seq<Seq<PlacedPart>> chunks = toSeq(toSeq(placed.GroupBy(row => (row.SheetIndex, labels[row.Part])))
            .Bind(group => OrderBands(toSeq(group), shared, rank, policy)
                .Bind(members => Chunks(members, policy.MaxChainParts)))
            .OrderBy(chunk => chunk.Min(member => rank.Find(member.Part).IfNone(int.MaxValue)))
            .ThenBy(static chunk => chunk.Head.Map(static row => row.SheetIndex).IfNone(int.MaxValue)));
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
                && left.Measure.Centroid.DistanceTo(right.Measure.Centroid) <= policy.ChainBand.Millimeters)
            .Map(right => new TaggedEdge<PartInstance, double>(
                left.Part, right.Part, left.Measure.Centroid.DistanceTo(right.Measure.Centroid))))
            .Iter(graph.AddEdge);
        shared.Filter(edge => ids.Contains(edge.A.Part) && ids.Contains(edge.B.Part))
            .Iter(edge => graph.AddEdge(new TaggedEdge<PartInstance, double>(edge.A.Part, edge.B.Part, edge.SharedLengthMm)));
        Dictionary<PartInstance, int> labels = new();
        graph.ConnectedComponents(labels);
        return toSeq(members.GroupBy(row => labels[row.Part]))
            .Map(group => OrderConnected(toSeq(group), graph, rank, policy));
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
        Option<TryFunc<PartInstance, IEnumerable<TaggedEdge<PartInstance, double>>>> paths = members
            .Fold(Option<PlacedPart>.None, (best, row) =>
                best.Filter(held => Seat(held).CompareTo(Seat(row)) <= 0).IfNone(row))
            .Map(root => tree.TreeBreadthFirstSearch(root.Part));
        return toSeq(members.OrderBy(row => rank.Find(row.Part).IfNone(int.MaxValue))
            .ThenBy(row => paths.Map(walk => PathCount(walk, row.Part)).IfNone(int.MaxValue))
            .ThenBy(static row => row.Part.PartId)
            .ThenBy(static row => row.Part.Ordinal));

        (int Rank, double Reach) Seat(PlacedPart row) => (
            rank.Find(row.Part).IfNone(int.MaxValue),
            row.Measure.Centroid.DistanceTo(policy.RapidOrigin));
    }

    private static int PathCount<TVertex, TEdge>(TryFunc<TVertex, IEnumerable<TEdge>> paths, TVertex target)
        where TEdge : IEdge<TVertex> =>
        paths(target, out IEnumerable<TEdge>? path) ? path.Count() : int.MaxValue;

    private static Seq<Seq<PlacedPart>> Chunks(Seq<PlacedPart> members, int size) =>
        toSeq(members.Map((member, index) => (Member: member, Chunk: index / size)).GroupBy(static row => row.Chunk))
            .Map(static group => toSeq(group).Map(static row => row.Member));

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
        return from sheet in members.Head.Map(static row => row.SheetIndex)
                   .ToFin(new KernelFault.InvalidValue("linking", "link:chain-members"))
               from rows in members.Traverse(member => member.Region.Map((path, contour) => (path, contour))
                       .Traverse(row => Cut(member.Part, row.contour, row.path, cuts,
                           pierces.Contains((member.Part, row.contour))).ToValidation())
                       .As().ToFin()
                       .Map(cutRows => new ChainMember(
                           member.Part, cutRows, member.Region.Sum(static path => path.Length())))
                       .ToValidation())
                   .As().ToFin()
               select Row(index, sheet, rows, cuts);
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

    private static Fin<ContourCut> Cut(PartInstance part, int contour, Loop path, Seq<SharedCut> cuts, bool pierce) {
        Seq<OmittedSpan> omitted = cuts.Bind(cut => Omitted(part, contour, cut.Edge));
        Set<int> blocked = toSet(omitted.Map(static span => span.Segment));
        Seq<int> candidates = toSeq(Enumerable.Range(0, path.Count)).Filter(index => !blocked.Contains(index));
        int start = (candidates.IsEmpty ? toSeq(Enumerable.Range(0, path.Count)) : candidates)
            .Fold(Option<int>.None, (best, index) => best
                .Filter(held => Seat(held).CompareTo(Seat(index)) <= 0).IfNone(index))
            .IfNone(0);
        return Rotate(path, start).Map(rotated => new ContourCut(
            contour,
            rotated,
            toSeq(omitted.Map(span => span with { Segment = Wrap(span.Segment - start, path.Count) })
                .OrderBy(static span => span.Segment)),
            rotated.At(0),
            pierce));

        (double Y, double X) Seat(int index) => (path.At(index).Y, path.At(index).X);
    }

    private static Fin<Loop> Rotate(Loop loop, int start) => start == 0
        ? Fin.Succ(loop)
        : Loop.Admit(
            Range(0, loop.Count).ToSeq().Map(offset => loop.At(start + offset)).ToArr(),
            loop.Closed,
            Range(0, loop.Count).ToSeq().Map(offset => loop.BulgeAt(start + offset)).ToArr(),
            loop.Tolerance);

    private static int Wrap(int index, int count) => ((index % count) + count) % count;

    private static Seq<OmittedSpan> Omitted(PartInstance part, int contour, SharedEdge edge) =>
        edge.A.Part == part && edge.A.Contour == contour ? Seq(new OmittedSpan(edge.A.Segment, edge.WindowA))
            : edge.B.Part == part && edge.B.Contour == contour ? Seq(new OmittedSpan(edge.B.Segment, edge.WindowB))
            : Seq<OmittedSpan>();

    private static Seq<LinkOp.Bridge> Bridges(Seq<SharedEdge> shared, Option<BridgeSpacing> policy) =>
        policy.Map(row => shared.Bind(edge => {
            double width = row.Width.Millimeters, pitch = row.Spacing.Millimeters, setback = row.EndClearance.Millimeters;
            double available = edge.SharedLengthMm - (2.0 * setback) - width;
            int count = available < 0.0 ? 0 : 1 + (int)Math.Floor(available / pitch);
            return toSeq(Enumerable.Range(0, count)).Map(slot => new LinkOp.Bridge(
                edge,
                Lerp(edge.Cut, (setback + (0.5 * width) + (slot * pitch)) / edge.SharedLengthMm),
                width));
        })).IfNone(Seq<LinkOp.Bridge>());

    private static Fin<Seq<WasteRow>> Waste(Seq<PlacedPart> placed, LinkRun run, Option<WasteVoronoi> policy) =>
        run.StockBySheet.ToSeq().Traverse(row => Partition(
                row.Key,
                row.Value,
                placed.Filter(part => part.SheetIndex == row.Key),
                run.KeepOutBySheet.Find(row.Key).IfNone(Seq<Loop>()),
                policy).ToValidation())
            .As().ToFin()
            .Map(static rows => rows.Somes());

    private static Fin<Option<WasteRow>> Partition(
        int sheet,
        Stock stock,
        Seq<PlacedPart> occupied,
        Seq<Loop> keepOut,
        Option<WasteVoronoi> policy) => PolygonAlgebra.Apply(new PolygonOp.Boolean(
                stock.Region,
                occupied.Bind(static part => part.Region).Concat(stock.Exclusions).Concat(keepOut),
                BooleanOp.Difference,
                PolygonFill.NonZero))
            .Bind(Paths)
            .Bind(usable => usable.IsEmpty
                ? Fin.Succ(Option<WasteRow>.None)
                : policy.Match(
                    Some: row => Seed(usable, row.SiteSpacing.Millimeters, row.MaxSites)
                        .Bind(sites => sites.Count < 2
                            ? Fin.Succ(Some(Safe(sheet, usable) with { Sites = sites.Count }))
                            : BuildVoronoi(sheet, usable, sites, row).Map(Some)),
                    None: () => Fin.Succ(Some(Safe(sheet, usable)))));

    private static WasteRow Safe(int sheet, Seq<Loop> usable) => new(
        sheet,
        usable,
        Seq<Edge3>(),
        Sites: 0,
        Cells: 0,
        FragmentAreaMm2: 0.0,
        new AdjacencyGraph<int, TaggedEdge<int, double>>(allowParallelEdges: false),
        Map<int, Point3d>(),
        Routed: false);

    private static Fin<Seq<Point3d>> Seed(Seq<Loop> usable, double spacing, int maxSites) {
        BoundingBox bounds = new(usable.Bind(static loop => toSeq(loop.Vertices)));
        int columns = (int)Math.Min(maxSites - 1L, Math.Max(1.0, Math.Ceiling((bounds.Max.X - bounds.Min.X) / spacing)));
        int rows = (int)Math.Min(maxSites - 1L, Math.Max(1.0, Math.Ceiling((bounds.Max.Y - bounds.Min.Y) / spacing)));
        long slots = (long)(columns + 1) * (rows + 1);
        Seq<Point3d> grid = Range(0, (int)Math.Min(maxSites, slots)).ToSeq().Map(index => {
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
            .Bind(trace => trace
                .Relations(new KernelFault.InvalidValue("linking", "link:waste-seed-trace"))
                .Map(relations => toSeq(bounded.Zip(relations, static (point, relation) => (point, relation)))
                    .Choose(static row => row.relation == PointRelation.Outside ? None : Some(row.point))));
    }

    private static Fin<WasteRow> BuildVoronoi(
        int sheet,
        Seq<Loop> usable,
        Seq<Point3d> seeds,
        WasteVoronoi policy) {
        return from outline in usable.Head.ToFin(
                   new GeometryFault.DegenerateInput(Kind.Polyline, None, "link:waste-outline"))
               let policyRow = SitePolicy.Create(
                   relaxations: policy.Relaxations,
                   relaxationStrength: policy.RelaxationStrength.DecimalFractions,
                   merge: policy.MergeDistance.Millimeters > 0.0
                       ? Some(SiteMerge.Create(minimumArea: 0.0, minimumSeparation: policy.MergeDistance.Millimeters))
                       : None)
               from trace in PolygonAlgebra.Apply(
                   new PolygonOp.Cells(seeds.ToArr(), outline, policyRow))
               from diagram in trace.Diagram(
                   new KernelFault.InvalidValue("linking", "link:waste-cell-trace"))
               let raw = diagram.Adjacency.ToSeq()
                   .Filter(edge => edge.Length >= policy.MinEdge.Millimeters)
                   .Map(static edge => new Edge3(edge.Start, edge.End))
               let closed = diagram.Cells.ToSeq().Map(static cell => cell.Ring)
               from fragment in Fragmented(closed, usable, policy.MinReusable.SquareMillimeters)
               from clipped in PolygonAlgebra.Apply(new PolygonOp.ClipOpen(raw.Map(Seq), usable, PolygonFill.NonZero))
               from split in Split(clipped)
               select RouteGraph(
                   sheet, usable, split.Inside, diagram.Cells.Count, closed.Count, fragment,
                   diagram.Tolerance.Absolute.Value);
    }

    private static Fin<double> Fragmented(Seq<Loop> cells, Seq<Loop> usable, double floorMm2) =>
        cells.Traverse(cell => PolygonAlgebra.Apply(new PolygonOp.Boolean(
                    Seq(cell), usable, BooleanOp.Intersection, PolygonFill.NonZero))
                .Bind(Paths)
                .Bind(region => region.IsEmpty
                    ? Fin.Succ(0.0)
                    : PolygonAlgebra.Apply(new PolygonOp.Measure(region, PolygonFill.NonZero))
                        .Bind(static trace => trace
                            .Measure(new KernelFault.InvalidValue("linking", "link:cell-measure-trace"))
                            .Map(static measured => measured.FilledArea)))
                .ToValidation())
            .As().ToFin()
            .Map(areas => areas.Filter(area => area > 0.0 && area < floorMm2).Fold(0.0, static (sum, area) => sum + area));

    private static WasteRow RouteGraph(
        int sheet,
        Seq<Loop> usable,
        Seq<Seq<Edge3>> paths,
        int sites,
        int cells,
        double fragmentAreaMm2,
        double grid) {
        Seq<Edge3> cuts = paths.Bind(identity);
        Seq<(long X, long Y, long Z)> stations = cuts
            .Bind(edge => Seq(Station(edge.A, grid), Station(edge.B, grid))).Distinct();
        Map<(long X, long Y, long Z), int> ids = stations.Fold(
            Map<(long, long, long), int>(), static (index, station) => index.Add(station, index.Count));
        Map<int, Point3d> nodes = toMap(cuts
            .Bind(edge => Seq((Station(edge.A, grid), edge.A), (Station(edge.B, grid), edge.B)))
            .Map(row => (ids[row.Item1], row.Item2))
            .Distinct());
        AdjacencyGraph<int, TaggedEdge<int, double>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(nodes.Keys);
        cuts.Iter(edge => {
            int a = ids[Station(edge.A, grid)], b = ids[Station(edge.B, grid)];
            double length = edge.A.DistanceTo(edge.B);
            graph.AddEdge(new TaggedEdge<int, double>(a, b, length));
            graph.AddEdge(new TaggedEdge<int, double>(b, a, length));
        });
        return new WasteRow(sheet, usable, cuts, sites, cells, fragmentAreaMm2, graph, nodes, Routed: true);
    }

    private static (long X, long Y, long Z) Station(Point3d point, double grid) => (
        (long)Math.Round(point.X / grid, MidpointRounding.ToEven),
        (long)Math.Round(point.Y / grid, MidpointRounding.ToEven),
        (long)Math.Round(point.Z / grid, MidpointRounding.ToEven));

    private static Fin<Seq<ChainRow>> Route(Seq<ChainRow> chains, Seq<WasteRow> partitions, CutLinkPolicy policy) =>
        toSeq(chains.GroupBy(static chain => chain.SheetIndex).OrderBy(static group => group.Key))
            .Traverse(group => RouteSheet(
                toSeq(group),
                partitions.Find(row => row.SheetIndex == group.Key),
                policy.RapidOrigin,
                policy.Waste.Map(static row => row.RapidProbeNodes).IfNone(1)).ToValidation())
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
                            .IfNone(Fin.Fail<Seq<Point3d>>(new KernelFault.InvalidValue("linking", "link:rapid-usable-region")))
                        select (Cursor: pierce, Paths: prior.Paths.Add(path)))
                select (routed.Cursor, Rows: state.Rows.Add(chain with { RapidPaths = routed.Paths })))
            .Map(static state => state.Rows);

    private static Fin<Seq<Point3d>> RapidPath(WasteRow partition, Point3d from, Point3d to, int probes) =>
        from direct in Visible(partition.Usable, from, to)
        from path in direct || !partition.Routed
            ? Fin.Succ(Seq(from, to))
            : partition.Nodes.IsEmpty
                ? Fin.Fail<Seq<Point3d>>(new KernelFault.InvalidValue("linking", "link:rapid-blocked"))
                : from starts in VisibleNodes(partition, from, probes)
                  from ends in VisibleNodes(partition, to, probes)
                  from route in toSeq(starts.Bind(start => ends.Choose(end => GraphPath(partition, start, end)))
                          .OrderBy(Tour))
                      .Head
                      .ToFin(new KernelFault.InvalidValue("linking", "link:rapid-route"))
                  select Seq(from).Concat(route).Add(to)
        select path;

    private static Fin<Seq<int>> VisibleNodes(WasteRow partition, Point3d point, int probes) =>
        toSeq(partition.Nodes.AsIterable().OrderBy(row => row.Value.DistanceTo(point)).Take(probes))
            .Traverse(row => Visible(partition.Usable, point, row.Value)
                .Map(clear => (row.Key, Clear: clear)).ToValidation())
            .As().ToFin()
            .Bind(static rows => rows.Filter(static row => row.Clear).Map(static row => row.Key) is { IsEmpty: false } visible
                ? Fin.Succ(visible)
                : Fin.Fail<Seq<int>>(new KernelFault.InvalidValue("linking", "link:rapid-connector")));

    private static Fin<bool> Visible(Seq<Loop> usable, Point3d first, Point3d second) =>
        usable.Head.Map(static loop => loop.Tolerance.Absolute.Value)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "link:visibility-outline"))
            .Bind(tolerance => first.DistanceTo(second) <= tolerance
                ? Fin.Succ(true)
                : PolygonAlgebra.Apply(new PolygonOp.ClipOpen(
                        Seq(Seq(new Edge3(first, second))), usable, PolygonFill.NonZero))
                    .Bind(Split)
                    .Map(split => split.Outside.Bind(identity)
                        .Sum(static edge => edge.A.DistanceTo(edge.B)) <= tolerance));

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
        double continuous = toSeq(chains.Map(static chain => chain.CutLengthMm)
                .Concat(partitions.Map(static row => row.Cuts.Sum(static edge => edge.A.DistanceTo(edge.B))))
                .OrderByDescending(static value => value))
            .Head.IfNone(0.0);
        double heat = continuous / run.Policy.MaxContinuousCut.Millimeters;
        double quality = shared.Sum(edge => run.Policy.MatchToleranceMm / edge.SharedLengthMm) + bridges.Count;
        double remnant = (partition * run.Policy.CutWidth.Millimeters) + partitions.Sum(static row => row.FragmentAreaMm2);
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

    private static int Compare(PartInstance left, PartInstance right) =>
        left.PartId != right.PartId ? left.PartId.CompareTo(right.PartId) : left.Ordinal.CompareTo(right.Ordinal);

    private static Point3d Lerp(Edge3 edge, double parameter) => edge.A + (parameter * (edge.B - edge.A));

    private static double Tour(Seq<Point3d> points) =>
        points.Head.Map(head => points.Tail.Fold((At: head, Length: 0.0),
            static (state, point) => (point, state.Length + state.At.DistanceTo(point))).Length).IfNone(0.0);
}

public readonly record struct CommonLineCensus(double OverlapMm, int Pairs, int Pierces);

public readonly record struct CommonLineBudget(
    double SeparationMm,
    double ToleranceMm,
    double AngularRadians,
    double MinimumOverlapMm) {
    public static CommonLineBudget Touching(Context left, Context right) =>
        new(0.0,
            Math.Max(left.Absolute.Value, right.Absolute.Value),
            Math.Max(left.Angle.Value, right.Angle.Value),
            Math.Max(left.Absolute.Value, right.Absolute.Value));
}

public readonly record struct CommonLineSpan(
    SegmentWindow WindowA,
    SegmentWindow WindowB,
    double SpanAmm,
    double SpanBmm,
    Edge3 Cut,
    double OverlapMm);

public static class CommonLine {
    public static Option<CommonLineSpan> Share(
        Loop left,
        int leftSegment,
        Loop right,
        int rightSegment,
        CommonLineBudget budget) {
        if (left.BulgeAt(leftSegment) != 0.0 || right.BulgeAt(rightSegment) != 0.0) return None;
        Point3d a0 = left.At(leftSegment), a1 = left.At(leftSegment + 1);
        Point3d b0 = right.At(rightSegment), b1 = right.At(rightSegment + 1);
        Edge3 leftEdge = new(a0, a1), rightEdge = new(b0, b1);
        Vector3d da = a1 - a0, db = b1 - b0;
        double la = da.Length, lb = db.Length;
        if (la <= budget.ToleranceMm || lb <= budget.ToleranceMm) return None;
        if ((da * db) / (la * lb) > -Math.Cos(budget.AngularRadians)) return None;
        if (Seq(leftEdge.Gap(b0), leftEdge.Gap(b1), rightEdge.Gap(a0), rightEdge.Gap(a1))
            .Exists(gap => Math.Abs(gap - budget.SeparationMm) > budget.ToleranceMm)) return None;
        double first = ((b0 - a0) * da) / (la * la), second = ((b1 - a0) * da) / (la * la);
        double lo = Math.Max(0.0, Math.Min(first, second)), hi = Math.Min(1.0, Math.Max(first, second));
        double overlap = (hi - lo) * la;
        if (overlap < budget.MinimumOverlapMm) return None;
        Point3d start = a0 + (da * lo), end = a0 + (da * hi);
        Point3d startB = Project(start, b0, db), endB = Project(end, b0, db);
        double bFirst = ((startB - b0) * db) / (lb * lb), bLast = ((endB - b0) * db) / (lb * lb);
        return Some(new CommonLineSpan(
            new SegmentWindow(lo, hi),
            new SegmentWindow(Math.Max(0.0, Math.Min(bFirst, bLast)), Math.Min(1.0, Math.Max(bFirst, bLast))),
            la,
            lb,
            new Edge3(start + (0.5 * (startB - start)), end + (0.5 * (endB - end))),
            overlap));
    }

    public static Seq<T> Disjoint<T>(Seq<T> ordered, Func<T, T, bool> conflicts) =>
        ordered.Fold(
            Seq<T>(),
            (accepted, candidate) => accepted.Exists(taken => conflicts(taken, candidate))
                ? accepted
                : accepted.Add(candidate));

    public static CommonLineCensus Measure(Seq<Loop> shapes) {
        Seq<(Loop Loop, int Segment)> sites = shapes.Bind(loop => toSeq(Enumerable.Range(0, loop.Count))
            .Filter(segment => loop.BulgeAt(segment) == 0.0)
            .Map(segment => (loop, segment)));
        Seq<(int Left, int Right, double Overlap)> accepted = Disjoint(
            toSeq(sites
                .Map((site, ordinal) => (site, ordinal))
                .Bind(left => sites.Map((site, ordinal) => (site, ordinal))
                    .Filter(right => right.ordinal > left.ordinal && !ReferenceEquals(right.site.Loop, left.site.Loop))
                    .Bind(right => Share(
                            left.site.Loop,
                            left.site.Segment,
                            right.site.Loop,
                            right.site.Segment,
                            CommonLineBudget.Touching(left.site.Loop.Tolerance, right.site.Loop.Tolerance))
                        .ToSeq()
                        .Map(span => (left.ordinal, right.ordinal, span.OverlapMm))))
                .OrderByDescending(static row => row.OverlapMm)
                .ThenBy(static row => row.ordinal)),
            static (taken, candidate) => taken.Left == candidate.Left || taken.Left == candidate.Right
                || taken.Right == candidate.Left || taken.Right == candidate.Right);
        return new CommonLineCensus(
            accepted.Sum(static row => row.Overlap),
            accepted.Count,
            Math.Max(0, shapes.Count - accepted.Count));
    }

    internal static Point3d Project(Point3d point, Point3d origin, Vector3d direction) =>
        origin + ((((point - origin) * direction) / (direction * direction)) * direction);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
