# [RASM_FABRICATION_SUPPORT]

The support-generation owner: ONE `Support.Grow` fold over the kernel slice stack producing the planar support-region stack, the tree branch graph, and the bridge verdicts — the two GEOMETRIC support lanes (`planar` region columns, `tree` branch scaffolds) discriminated by the `SupportLane` policy row on one entry. The voxel/lattice lane stays `Additive/implicit`'s. The planar lane is region set-algebra over the hole-carrying `SliceRegion` atom with two REAL recurrences: overhang detection `overhangᵢ = layerᵢ \ grow(layerᵢ₋₁, tan(α)·h)` (the self-supporting cone advance — a region survives unsupported when the layer below, grown by the critical-angle run, still covers it) and top-down accumulation `supportᵢ = (supportᵢ₊₁ ∪ overhangᵢ₊₁₊g) \ grow(layerᵢ, xyGap)` (support falls until the model catches it, carved off the part by the XY clearance, the incoming overhang delayed by the `ZGapLayers` vertical clearance `g` so the support top never fuses to the surface it protects); the interface carve splits each column's top `InterfaceLayers` into the dense contact skin — the interface-layer HEIGHT law is the kernel `LayerPlan.SupportInterface` row (SEALED — this page owns the region GEOMETRY, never a second elevation schedule). The tree lane is an influence-area graph walk: overhang tips sample at the tip pitch, descend layer by layer under the `AvoidanceState` cache ({`Fast` far-field full-angle step toward the plate drop · `FastSafe` full-angle step toward the nearest sibling, merge-eligible · `Slow` near-model vertical-only · `Collision` inside the keep-out}) — the two full-angle states differ by their TARGET, so merge-seeking and plate-seeking descent are distinct behaviors, never aliases — merge when influence disks touch, thicken toward the trunk radius, and terminate at a typed `TreeRole` ({`tip` sampled origin · `branch` descending link · `merge` disk-fused junction · `root` plate landing · `model-rest` on-part landing with a dense interface pad}); a branch trapped in `Collision` first attempts the vertical channel, then a model-rest, and only when NEITHER exists routes `SupportUnbuildable` 2735, never a silently dropped tip.

Bridge detection is the anchor-coverage state machine over each overhang boundary ring: {`Outside` → over supported material · `Hanging` → over void, run accumulating · `Anchored` → the span re-reached support within the bridgeable run (no support mints — FFF bridges it) · `Supported` → the run exceeded `MaxBridgeMm`, a support tip mints mid-span}. The transition law is ONE `AnchorState.Next(anchoredBelow, runMm, maxBridgeMm)` row — walking a ring threads the machine and emits each `BridgeSpan` tagged with the TRANSITION verdict (the state the span ENTERED, so a `Hanging → Supported` edge lands as a `Supported` span and its mid-span tip mints), and only `Supported` spans feed tips into the active lane. Region Booleans route the ONE `Geometry2D/algebra#POLYGON_ALGEBRA` owner through the `SliceRegion` atom; the layer truth is the kernel `SliceStack` (K3). Consumers: `Additive/slicing` hatches `SupportLayer.Sparse` at support density and `Interface` dense, `Additive/scanpath` hatches both as its `Support` exposure class, `Additive/production` reads the overhang census in its orientation objective and lowers the `TreeNode` graph to `CBeamLattice` beams, and `Additive/implicit` realizes `TreeNode` graphs as PicoGK lattice scaffolds.

Wire posture: HOST-LOCAL. `SupportPlan` crosses only the in-process seam to the slicing/scanpath/production/implicit consumers — never a browser or peer wire; the lanes and state machines never sit between wire and rail.

## [01]-[INDEX]

- [01]-[SUPPORT]: owns the `SupportLane`/`AvoidanceState`/`AnchorState`/`TreeRole` axes, the `SupportPolicy` row (critical angle, XY and Z gaps, interface depth, bridge run, branch geometry), the overhang/accumulation recurrences, the influence-area tree walk with model-rests, the bridge state machine, the `SupportLayer`/`TreeNode`/`BridgeSpan`/`SupportPlan` receipts, and the ONE `Support.Grow` fold — planar and tree lanes on one entry, the voxel lane declared to `implicit`.

## [02]-[SUPPORT]

- Owner: `SupportLane` `[SmartEnum<string>]` (`planar`/`tree`) the lane discriminant — a policy row, never two entrypoints; `AvoidanceState` `[SmartEnum<string>]` (`fast`/`fast-safe`/`slow`/`collision`) the per-cell descent classification the tree walk caches per layer — each row selecting a distinct target/step behavior; `AnchorState` `[SmartEnum<string>]` (`outside`/`hanging`/`anchored`/`supported`) the bridge coverage machine carrying the ONE `Next` transition row; `TreeRole` `[SmartEnum<string>]` (`tip`/`branch`/`merge`/`root`/`model-rest`) the node disposition — the receipt names its plate roots and on-model rests, never a sentinel overload; `SupportPolicy` the parameter carrier (critical angle α, XY gap, Z-gap layers, interface depth, min-area speck floor, max bridge run, branch angle, tip/trunk/merge radii) with `Fff`/`Lpbf` seed rows; `SupportLayer` the per-layer planar receipt (sparse + interface `SliceRegion`s); `TreeNode` the branch-graph row (id, layer, position, radius, upstream parent — `-1` marks a sampled tip origin — and role); `BridgeSpan` the typed bridge verdict; `SupportPlan` the lane-tagged plan receipt with the support-volume scalar; `Support` the static surface owning the ONE `Grow` fold.
- Cases: `AvoidanceState` rows 4 — `collision` inside `grow(model, xyGap)` (no lateral descent; vertical, then model-rest, then 2735), `slow` within the near-field band `2·xyGap` (vertical-only descent), `fast-safe` within merge reach of a sibling branch (full-angle step toward that sibling), `fast` far field (full-angle step toward the plate drop); `AnchorState` rows 4 with the total transition law `Next(anchoredBelow, runMm, maxBridge)` — `anchoredBelow → anchored`, `outside ∧ ¬anchored → hanging`, `hanging ∧ run > maxBridge → supported`, else `hanging`; `TreeRole` rows 5; `SupportLane` rows 2; the planar recurrences are the two ruled formulas (overhang cone advance, top-down carve accumulation with the Z-gap delay) and the interface carve `interfaceᵢ = supportᵢ ∩ ⋃overhangᵢ₊₁..ᵢ₊ₖ`.
- Entry: `public static Fin<SupportPlan> Grow(SliceStack stack, SupportPolicy policy)` — the ONE entrypoint; the lane row discriminates planar/tree inside the fold; `Fin<T>` routes `FabricationFault.SupportUnbuildable` 2735 `(layer, region)` when the tree search terminates in `Collision` with neither a vertical channel nor a model-rest, and kernel `GeometryFault.DegenerateInput` on an empty stack, lowered per `Process/faults#FAULT_BAND`.
- Auto: `Grow` computes the per-layer overhang census once (`Census` — the cone-advance difference over `SliceRegion`; layer 0 sits on the plate and mints none; specks under `MinAreaMm2` drop); the PLANAR lane folds top-down with the accumulation recurrence — the incoming overhang delayed `ZGapLayers` below its demand, each falling region carved by the XY-gap-grown model layer, the interface carve split against the k-layer overhang union; the TREE lane samples `Supported`-verdict tips and overhang interiors at the tip pitch, then descends: each node classifies its next position through the `AvoidanceState` cache (built per layer from the XY-gap model region and the sibling influence disks), steps laterally at most `tan(branchAngle)·h` toward its state's target (`fast` the plate drop, `fast-safe` the nearest sibling), merges when disks overlap (radius grows toward `TrunkRadiusMm`, the junction node lands `TreeRole.Merge`), roots at layer 0 as `TreeRole.Root`, and rests on the model as `TreeRole.ModelRest` when descent is trapped over part material; the BRIDGE machine walks each overhang boundary ring vertex-wise (`anchoredBelow` = the layer-below region covers the vertex), threading `AnchorState.Next` and emitting transition-tagged `BridgeSpan` rows — `Anchored` spans suppress tips, `Supported` spans mint one mid-span tip.
- Receipt: `SupportPlan` IS the typed evidence — the lane tag, the planar `SupportLayer` stack, the role-tagged `TreeNode` graph, the `BridgeSpan` verdicts, and the support-volume scalar; no generic support ledger, no mesh realization (beam/lattice solids are `implicit`'s voxel lane and `production`'s `CBeamLattice` lowering; FFF support toolpaths are `slicing`'s hatch).
- Packages: `Rasm.Meshing` (`SliceStack` K3 — the layer truth; `LayerPlan.SupportInterface` the kernel HEIGHT law this page never re-derives), `Additive/slicing#SLICING` (`SliceRegion` — the hole-carrying region atom the recurrences compute over), `Geometry2D/algebra#POLYGON_ALGEBRA` (`Offset`/`Clip` composed through the region atom — the ONE Boolean owner), `Process/owner#FABRICATION_OWNER` (`Loop`/`Edge3` atoms), `Process/faults#FAULT_BAND` (`SupportUnbuildable` 2735), `Rasm.Numerics` (`GeometryFault`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Map`), BCL inbox.
- Growth: a new lane (contour-following ribs, conical volumes) is one `SupportLane` row + one `Grow` arm over the same overhang census; a new avoidance class is one `AvoidanceState` row the cache classifier emits; a new bridge verdict is one `AnchorState` row + its `Next` clause; a new node disposition is one `TreeRole` row; per-material support parameters are `SupportPolicy` seed rows (`Fff`/`Lpbf` are the exemplars); the beam REALIZATION of `TreeNode` graphs is `implicit`'s lattice lane and `production`'s `CBeamLattice` map (declared seams), never a mesh builder here; zero new surface.
- Boundary: `Support` is the ONE support-geometry owner and a per-lane sibling class family is the deleted form — lanes are policy rows on one fold; the voxel/lattice scaffold is `Additive/implicit`'s declared lane and a PicoGK call here is the split-brain defect; the interface-layer HEIGHT law is the kernel `LayerPlan.SupportInterface` row and an elevation schedule here is the SEALED-boundary violation; region Booleans route `PolygonAlgebra` through `SliceRegion` and a support-local Clipper call site or a hole-blind region set is the named duplication defect; the overhang verdict is the cone-advance formula over exact region algebra and a per-triangle normal-angle classifier here is the deleted form (the mesh-facet census is `production`'s orientation objective over kernel `Analysis/select`); a dropped unbuildable tip is the named silent-scrap defect — the fold FAILS typed with `SupportUnbuildable`.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;                       // SliceStack — the K3 layer truth; LayerPlan.SupportInterface owns interface HEIGHTS
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] ----------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SupportLane {
    public static readonly SupportLane Planar = new("planar");
    public static readonly SupportLane Tree = new("tree");
}

// Per-cell descent classification the tree walk caches per layer; each row selects a distinct target/step law.
[SmartEnum<string>]
public sealed partial class AvoidanceState {
    public static readonly AvoidanceState Fast = new("fast");             // far field: full-angle step toward the plate drop
    public static readonly AvoidanceState FastSafe = new("fast-safe");    // full-angle step toward the nearest sibling, merge-eligible
    public static readonly AvoidanceState Slow = new("slow");             // near model: vertical-only descent
    public static readonly AvoidanceState Collision = new("collision");   // inside keep-out: vertical, then model-rest, then 2735
}

// Bridge anchor-coverage machine; Next is the ONE total transition row the ring walk threads.
[SmartEnum<string>]
public sealed partial class AnchorState {
    public static readonly AnchorState Outside = new("outside");
    public static readonly AnchorState Hanging = new("hanging");
    public static readonly AnchorState Anchored = new("anchored");
    public static readonly AnchorState Supported = new("supported");

    public AnchorState Next(bool anchoredBelow, double runMm, double maxBridgeMm) =>
        anchoredBelow ? Anchored
        : this == Outside ? Hanging
        : runMm > maxBridgeMm ? Supported
        : Hanging;
}

// Node disposition: the receipt NAMES its plate roots and on-model rests — never a sentinel overload on Parent.
[SmartEnum<string>]
public sealed partial class TreeRole {
    public static readonly TreeRole Tip = new("tip");
    public static readonly TreeRole Branch = new("branch");
    public static readonly TreeRole Merge = new("merge");
    public static readonly TreeRole Root = new("root");
    public static readonly TreeRole ModelRest = new("model-rest");
}

// --- [MODELS] ---------------------------------------------------------------------------------------------------------------------------------------
public sealed record SupportPolicy(
    SupportLane Lane,
    double CriticalAngleDeg,        // α in overhangᵢ = layerᵢ \ grow(layerᵢ₋₁, tan(α)·h)
    double XyGapMm,                 // model carve clearance (the accumulation recurrence's grow)
    int ZGapLayers,                 // vertical clearance g — the overhang enters the falling set g layers below its demand
    int InterfaceLayers,            // interface carve depth k — the HEIGHTS stay kernel LayerPlan.SupportInterface
    double MinAreaMm2,              // speck floor on the overhang census
    double MaxBridgeMm,             // bridgeable unsupported run before a tip mints
    double BranchAngleDeg,          // tree lateral step per layer = tan(branch)·h
    double TipRadiusMm,
    double TrunkRadiusMm,
    double MergeRadiusMm) {
    public static SupportPolicy Fff() => new(SupportLane.Planar, 45.0, 0.8, 1, 3, 4.0, 8.0, 40.0, 0.4, 2.0, 1.5);
    public static SupportPolicy Lpbf() => new(SupportLane.Tree, 45.0, 0.15, 0, 4, 1.0, 2.0, 30.0, 0.3, 1.2, 0.8);
}

public sealed record SupportLayer(int Layer, SliceRegion Sparse, SliceRegion Interface);

// Parent is the UPSTREAM link (the node above that feeds this one); -1 marks a sampled tip origin; Role names
// the disposition — implicit draws one beam per Parent >= 0 link, production maps Merge nodes to sBall rows.
public readonly record struct TreeNode(int Id, int Layer, Point3d At, double Radius, int Parent, TreeRole Role);

public readonly record struct BridgeSpan(int Layer, Edge3 Span, AnchorState Verdict);

public sealed record SupportPlan(SupportLane Lane, Seq<SupportLayer> Planar, Seq<TreeNode> Tree, Seq<BridgeSpan> Bridges, double VolumeMm3);

// --- [OPERATIONS] -----------------------------------------------------------------------------------------------------------------------------------
public static class Support {
    public static Fin<SupportPlan> Grow(SliceStack stack, SupportPolicy policy) =>
        stack.LayerCount == 0
            ? Fin.Fail<SupportPlan>(GeometryFault.DegenerateInput("support:empty-slice-stack").ToError())
            : from census in Census(stack, policy)
              let bridges = Bridges(stack, census, policy)
              from plan in policy.Lane == SupportLane.Planar
                  ? PlanarLane(stack, census, bridges, policy)
                  : TreeLane(stack, census, bridges, policy).Map(tree =>
                      new SupportPlan(SupportLane.Tree, Seq<SupportLayer>(), tree, bridges, TreeVolume(tree, stack)))
              select plan;

    // --- [OVERHANG_CENSUS]
    // overhangᵢ = layerᵢ \ grow(layerᵢ₋₁, tan(α)·h): the self-supporting cone advance; layer 0 sits on the plate.
    static Fin<Arr<SliceRegion>> Census(SliceStack stack, SupportPolicy policy) =>
        toSeq(Enumerable.Range(0, stack.LayerCount))
            .Map(i => {
                if (i == 0) return Fin.Succ(SliceRegion.Empty);
                double h = stack.Elevations[i] - stack.Elevations[i - 1];
                double run = Math.Tan(policy.CriticalAngleDeg * Math.PI / 180.0) * h;
                return from grown in SliceRegion.Of(stack, i - 1).Grow(run)
                       from over in SliceRegion.Of(stack, i).Difference(grown)
                       select new SliceRegion(over.Outers.Filter(l => Math.Abs(PolygonAlgebra.Area(l)) >= policy.MinAreaMm2), over.Holes);
            })
            .Sequence()
            .Map(static rows => rows.ToArr());

    // --- [PLANAR_LANE]
    // supportᵢ = (supportᵢ₊₁ ∪ overhangᵢ₊₁₊g) \ grow(layerᵢ, xyGap); interfaceᵢ = supportᵢ ∩ ⋃overhangᵢ₊₁..ᵢ₊ₖ.
    // Top-down state fold: the falling column threads the rail, so a failed region Boolean stays typed.
    static Fin<SupportPlan> PlanarLane(SliceStack stack, Arr<SliceRegion> census, Seq<BridgeSpan> bridges, SupportPolicy policy) =>
        toSeq(Enumerable.Range(0, Math.Max(0, stack.LayerCount - 1)).Reverse())
            .Fold(
                Fin.Succ((Falling: SliceRegion.Empty, Layers: Seq<SupportLayer>(), Volume: 0.0)),
                (acc, i) => acc.Bind(state => {
                    int incoming = i + 1 + policy.ZGapLayers;
                    SliceRegion demand = incoming < stack.LayerCount ? census[incoming] : SliceRegion.Empty;
                    return from fed in state.Falling.Union(demand)
                           from carve in SliceRegion.Of(stack, i).Grow(policy.XyGapMm)
                           from falling in fed.Difference(carve)
                           from row in falling.IsEmpty
                               ? Fin.Succ(Option<(SupportLayer, double)>.None)
                               : from interfaceUnion in toSeq(Enumerable.Range(i + 1, Math.Min(policy.InterfaceLayers, stack.LayerCount - i - 1)))
                                     .Fold(Fin.Succ(SliceRegion.Empty), (u, j) => u.Bind(held => held.Union(census[j])))
                                 from dense in falling.Intersect(interfaceUnion)
                                 from sparse in falling.Difference(dense)
                                 let h = i + 1 < stack.LayerCount ? stack.Elevations[i + 1] - stack.Elevations[i] : 0.0
                                 select Some((new SupportLayer(i, sparse, dense), falling.Area() * h))
                           select (Falling: falling,
                                   Layers: row.Map(static r => r.Item1).Match(Some: l => state.Layers.Add(l), None: () => state.Layers),
                                   Volume: state.Volume + row.Map(static r => r.Item2).IfNone(0.0));
                }))
            .Map(state => new SupportPlan(SupportLane.Planar, state.Layers.Rev(), Seq<TreeNode>(), bridges, state.Volume));

    // --- [TREE_LANE]
    // Influence-area walk: tips descend under the avoidance cache toward their state's target, merge on disk
    // overlap, thicken toward the trunk, and land a typed TreeRole — root on the plate, rest on the model.
    static Fin<Seq<TreeNode>> TreeLane(SliceStack stack, Arr<SliceRegion> census, Seq<BridgeSpan> bridges, SupportPolicy policy) {
        var nodes = new System.Collections.Generic.List<TreeNode>();
        var active = new System.Collections.Generic.List<int>();
        for (int i = stack.LayerCount - 1; i >= 1; i--) {
            foreach (Point3d tip in Tips(census[i], bridges.Filter(b => b.Layer == i && b.Verdict == AnchorState.Supported), policy)) {
                nodes.Add(new TreeNode(nodes.Count, i, tip, policy.TipRadiusMm, Parent: -1, TreeRole.Tip));
                active.Add(nodes.Count - 1);
            }
            if (active.Count == 0) { continue; }
            double h = stack.Elevations[i] - stack.Elevations[i - 1];
            double step = Math.Tan(policy.BranchAngleDeg * Math.PI / 180.0) * h;
            SliceRegion model = SliceRegion.Of(stack, i - 1);
            SliceRegion keepOut = model.Grow(policy.XyGapMm).IfFail(model);
            var next = new System.Collections.Generic.List<int>();
            foreach (int id in active) {
                TreeNode node = nodes[id];
                AvoidanceState state = Classify(node.At, keepOut, node, active, nodes, policy);
                Point3d target = state == AvoidanceState.FastSafe
                    ? MergeTarget(node, active, nodes).IfNone(new Point3d(node.At.X, node.At.Y, 0.0))
                    : new Point3d(node.At.X, node.At.Y, 0.0);
                Point3d moved = state == AvoidanceState.Slow || state == AvoidanceState.Collision
                    ? node.At
                    : StepToward(node.At, target, step);
                bool blocked = keepOut.Covers(moved);
                bool rests = blocked && model.Covers(new Point3d(node.At.X, node.At.Y, 0.0));
                if (blocked && !rests)
                    return Fin.Fail<Seq<TreeNode>>(FabricationFault.SupportUnbuildable(i - 1, id).ToError());   // region witness = the trapped branch id
                double radius = Math.Min(policy.TrunkRadiusMm, node.Radius + 0.04 * h);
                TreeRole role = rests ? TreeRole.ModelRest : i - 1 == 0 ? TreeRole.Root : TreeRole.Branch;
                nodes.Add(new TreeNode(nodes.Count, i - 1, new Point3d(moved.X, moved.Y, stack.Elevations[i - 1]), radius, id, role));
                if (!rests) { next.Add(nodes.Count - 1); }
            }
            active.Clear();
            active.AddRange(Merge(next, nodes, policy));
        }
        return Fin.Succ(toSeq(nodes));
    }

    // --- [BRIDGE_MACHINE]
    // Ring walk threading AnchorState.Next; each span carries the TRANSITION verdict — a Hanging → Supported
    // edge lands as a Supported span, so its mid-span tip mints. Anchored spans suppress tips.
    static Seq<BridgeSpan> Bridges(SliceStack stack, Arr<SliceRegion> census, SupportPolicy policy) =>
        toSeq(Enumerable.Range(1, Math.Max(0, stack.LayerCount - 1))).Bind(i => {
            SliceRegion below = SliceRegion.Of(stack, i - 1);
            return census[i].Outers.Bind(ring => {
                var spans = new System.Collections.Generic.List<BridgeSpan>();
                AnchorState state = AnchorState.Outside;
                double run = 0.0;
                Point3d start = ring.At(0);
                for (int v = 0; v <= ring.Count; v++) {
                    Point3d at = ring.At(v);
                    bool anchored = below.Covers(at);
                    run = anchored ? 0.0 : run + (v == 0 ? 0.0 : ring.At(v - 1).DistanceTo(at));
                    AnchorState nextState = state.Next(anchored, run, policy.MaxBridgeMm);
                    if (nextState != state && v > 0) { spans.Add(new BridgeSpan(i, new Edge3(start, at), nextState)); start = at; }
                    state = nextState;
                }
                return toSeq(spans);
            });
        });

    // --- [BOUNDARIES]
    static AvoidanceState Classify(Point3d at, SliceRegion keepOut, TreeNode self, System.Collections.Generic.List<int> active,
                                   System.Collections.Generic.List<TreeNode> nodes, SupportPolicy policy) =>
        keepOut.Covers(at) ? AvoidanceState.Collision
        : NearBoundary(keepOut.Outers, at, 2.0 * policy.XyGapMm) ? AvoidanceState.Slow
        : active.Exists(id => id != self.Id && nodes[id].At.DistanceTo(at) <= 4.0 * policy.MergeRadiusMm) ? AvoidanceState.FastSafe
        : AvoidanceState.Fast;

    static Point3d StepToward(Point3d from, Point3d target, double step) {
        Vector3d lateral = new(target.X - from.X, target.Y - from.Y, 0.0);
        return lateral.Length <= step ? new Point3d(target.X, target.Y, from.Z) : from + step * (lateral / lateral.Length);
    }

    static Option<Point3d> MergeTarget(TreeNode node, System.Collections.Generic.List<int> active,
                                       System.Collections.Generic.List<TreeNode> nodes) {
        Option<TreeNode> best = None;
        double bestD = double.MaxValue;
        foreach (int id in active) {
            if (id == node.Id) continue;
            double d = nodes[id].At.DistanceTo(node.At);
            if (d < bestD) { bestD = d; best = Some(nodes[id]); }
        }
        return best.Map(static n => n.At);
    }

    // Disk-overlap fusion: the surviving host thickens toward the trunk radius and re-tags Merge.
    static System.Collections.Generic.IEnumerable<int> Merge(System.Collections.Generic.List<int> ids,
                                                             System.Collections.Generic.List<TreeNode> nodes, SupportPolicy policy) {
        var kept = new System.Collections.Generic.List<int>();
        foreach (int id in ids) {
            int absorbed = kept.FindIndex(k => nodes[k].At.DistanceTo(nodes[id].At) <= policy.MergeRadiusMm);
            if (absorbed < 0) { kept.Add(id); }
            else {
                TreeNode host = nodes[kept[absorbed]];
                nodes[kept[absorbed]] = host with {
                    Radius = Math.Min(policy.TrunkRadiusMm, host.Radius + nodes[id].Radius * 0.5),
                    Role = TreeRole.Merge,
                };
            }
        }
        return kept;
    }

    static System.Collections.Generic.IEnumerable<Point3d> Tips(SliceRegion overhang, Seq<BridgeSpan> supported, SupportPolicy policy) {
        double pitch = Math.Max(2.0 * policy.TipRadiusMm, 1e-3) * 4.0;
        foreach (Loop region in overhang.Outers) {
            BoundingBox b = region.Bound();
            for (double x = b.Min.X; x <= b.Max.X; x += pitch)
                for (double y = b.Min.Y; y <= b.Max.Y; y += pitch) {
                    Point3d p = new(x, y, b.Min.Z);
                    if (overhang.Covers(p)) yield return p;
                }
        }
        foreach (BridgeSpan span in supported)
            yield return new Point3d(0.5 * (span.Span.A.X + span.Span.B.X), 0.5 * (span.Span.A.Y + span.Span.B.Y), span.Span.A.Z);
    }

    static bool NearBoundary(Seq<Loop> regions, Point3d p, double band) =>
        regions.Exists(l => toSeq(Enumerable.Range(0, l.Count)).Exists(i => DistanceToSegment(p, l.At(i), l.At(i + 1)) <= band));

    static double DistanceToSegment(Point3d p, Point3d a, Point3d b) {
        Vector3d ab = b - a;
        double t = ab.SquareLength < 1e-12 ? 0.0 : Math.Clamp(((p - a) * ab) / ab.SquareLength, 0.0, 1.0);
        return p.DistanceTo(a + t * ab);
    }

    static double TreeVolume(Seq<TreeNode> nodes, SliceStack stack) =>
        nodes.Filter(static n => n.Parent >= 0).Map(n => {
            double h = n.Layer + 1 < stack.LayerCount ? stack.Elevations[n.Layer + 1] - stack.Elevations[n.Layer] : 0.0;
            return Math.PI * n.Radius * n.Radius * h;
        }).Sum();
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Stack["kernel SliceStack (K3) — interface HEIGHTS stay LayerPlan.SupportInterface, SEALED"]
    Stack --> Region["SliceRegion.Of — Depth-parity holes"]
    Region --> Census["overhangᵢ = layerᵢ \\ grow(layerᵢ₋₁, tanα·h)"]
    Census --> Planar["planar: supportᵢ = (supportᵢ₊₁ ∪ overhangᵢ₊₁₊g) \\ grow(layerᵢ, xyGap) + interface carve"]
    Census --> Bridge["AnchorState machine — transition-tagged spans"]
    Bridge -->|Supported spans mint tips| Tree["tree: AvoidanceState walk — fast→plate, fast-safe→sibling"]
    Census --> Tree
    Tree -->|"trapped: no channel, no rest"| Fault["SupportUnbuildable 2735"]
    Tree --> Roles["TreeRole: tip · branch · merge · root · model-rest"]
    Planar --> PlanN["SupportPlan"]
    Roles --> PlanN
    PlanN -->|Sparse/Interface hatch| Slice["slicing + scanpath Support class"]
    PlanN -->|TreeNode graph| Implicit["implicit Lattice beams + production CBeamLattice"]
    PlanN -->|overhang census| Production["production orientation objective"]
```
