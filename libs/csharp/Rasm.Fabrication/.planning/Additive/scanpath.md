# [RASM_FABRICATION_SCANPATH]

The LPBF scan-path owner: ONE `Scan.Plan` fold over the kernel slice stack producing the per-layer vector program — hatch vectors, contour border passes, and point exposures — the `scan-vectors` content-keyed egress a powder-bed machine consumes. The hatch geometry is the `HatchPattern` `[Union]` (`Meander` whole-region parallel fill · `Stripe(width/overlap/stagger)` band-partitioned long-part fill · `Island(size/shift)` checkerboard thermal-stress partition · `Hexagon(cell)` tiled cellular fill) — four cell laws over ONE `Cells → Clip → Sort` pipeline, never four sibling generators. The inter-layer rotation is the recurrence `θₙ = (θ₀ + n·66.7°) mod 180°` (`ScanLaw.HatchAngleDeg` — the 66.7° increment maximizes the revisit period so no stripe/island seam stacks across layers). The sorter is TWO orthogonal axes composed as `ScanSort` — `ScanDirection` the direction-op (`keep`/`alternate`/`against-gas`, the gas-flow bearing a `ScanPolicy` value, never an inline axis literal) and `ScanOrdering` the ordering-op (`linear`/`nearest`/`thermal-spacing`, the thermal arm ONE max-min-distance greedy shared by cells and vectors). The exposure partition is region set-algebra over the hole-carrying `SliceRegion` atom: `DownSkinₙ = Rₙ \ ⋃Rₙ₋₁..ₙ₋ₖ`, `UpSkinₙ = Rₙ \ ⋃Rₙ₊₁..ₙ₊ₖ`, `InSkinₙ` the remainder, and `Support` the fourth class — the `SupportPlan` planar rows hatch through the SAME fold under their own `SkinParameters` — each row binding its power/speed/hatch scales over the physics budget, so a down-skin melts cooler and slower off ONE policy map, never a per-skin generator. A hatch vector shorter than the melt-pool spot does not silently drop: it demotes to a `Spot` point exposure whose dwell reproduces the vector's energy, so the point-exposure case is generated, never decorative.

The fold consumes the landed `Process/physics#CUT_PARAMETER` `RemovalBudget.Powder(LaserPower, HatchSpacing, ScanSpeed)` triple as the parameter floor — every emitted vector carries its skin-scaled power/speed, and the hatch pitch is `budget.HatchSpacing` under the policy and skin scales. The layer set arrives from the kernel `Slicing.Apply → Fin<SliceStack>` five-channel wire — adaptive layer HEIGHT stays the kernel `LayerPlan` height-law family (SEALED — this page never re-derives an elevation schedule, it walks `stack.Elevations` as given), and holes arrive through `SliceRegion.Of` Depth parity so a bore never hatches. Region Booleans route the ONE `Geometry2D/algebra#POLYGON_ALGEBRA` owner through the region atom. The voxel/grayscale/`.cli` lane is `Additive/implicit`'s; this page is the METAL VECTOR lane, reached from `owner#run` through the `AdditivePolicy.Scan` case that CARRIES the narrowed `Powder` budget. The `scan-vectors` `ContentKey` mints over the COMPLETE program — layer ordinal, elevation, every geometry case with its skin class, power, speed, and dwell — so two programs differing in any process-relevant byte never share a key. No new fault arm: an empty stack or a nonempty region yielding zero geometry is kernel `GeometryFault.DegenerateInput`, never a silent empty program.

Wire posture: HOST-LOCAL. The `ScanPlan` crosses only the in-process seam — its `ContentKey` rides `AdditiveResult.Artifacts` on the owner result and enrolls on the Persistence artifact index; the vector rows never sit between wire and rail.

## [01]-[INDEX]

- [01]-[SCAN_PATH]: owns the `SkinRegion`/`ScanDirection`/`ScanOrdering` axes, the `ScanLaw` rotation recurrence, the `HatchPattern` cell-law union, the `ScanSort`/`SkinParameters`/`ScanPolicy` policy rows, the `LayerGeometry` 3-case vector vocabulary with the sub-spot `Spot` demotion, the `ScanLayer`/`ScanPlan` receipts, and the ONE `Scan.Plan` fold from kernel `SliceStack` plus optional `SupportPlan` to the content-keyed vector program.

## [02]-[SCAN_PATH]

- Owner: `SkinRegion` `[SmartEnum<string>]` (`down-skin`/`up-skin`/`in-skin`/`support`) the exposure-class axis — support regions are a CLASS on this axis, never a second fold; `ScanDirection` `[SmartEnum<string>]` (`keep`/`alternate`/`against-gas`) the direction-op axis; `ScanOrdering` `[SmartEnum<string>]` (`linear`/`nearest`/`thermal-spacing`) the ordering-op axis; `ScanLaw` the dependency-free recurrence anchor (`LayerRotationDeg = 66.7`, `HatchAngleDeg`, the sub-spot vector floor); `HatchPattern` `[Union]` the cell law (`Meander` · `Stripe` · `Island` · `Hexagon`); `ScanSort` the composed sorter pair; `SkinParameters` the per-class power/speed/hatch scale row; `ScanPolicy` the ONE policy carrier (pattern, sort, `θ₀`, skin depth `k`, the `Map<SkinRegion, SkinParameters>`, contour passes, hatch inset, spacing scale, gas-flow bearing); `LayerGeometry` `[Union]` (`Hatch` skin-tagged vector runs · `Contour` border ring passes · `Spot` point exposures) the machine-geometry vocabulary; `ScanLayer`/`ScanPlan` the typed receipts; `Scan` the static surface owning the ONE `Plan` fold.
- Cases: `HatchPattern` cases 4 — `Meander` one whole-plane cell at the layer angle; `Stripe(WidthMm, OverlapMm, StaggerMm)` bands perpendicular to the layer angle, band ends staggered per layer; `Island(SizeMm, ShiftMm)` the checkerboard grid shifted `ShiftMm` per layer with alternating ±90° cell angles and parity-first sequencing; `Hexagon(CellMm)` the hex tiling with per-cell axis rotation; `LayerGeometry` cases 3 — `Hatch` generated by the pipeline, `Contour` by the border pass, `Spot` by the sub-spot demotion (dwell = `1e6 · length / speed` µs, the energy-equivalent exposure); `SkinRegion` rows 4 binding `SkinParameters` through the policy map (down-skin ~0.7 power/1.2 speed against dross, up-skin ~0.85/1.1 for surface finish, in-skin 1.0/1.0, support ~0.6/1.3 — sacrificial material melts cheap); the `(pattern × skin × ordering)` space is policy DATA over one pipeline — a new pattern is one union case + one `Cells` arm.
- Entry: `public static Fin<ScanPlan> Plan(SliceStack stack, ScanPolicy policy, RemovalBudget.Powder budget, Option<SupportPlan> support = default)` — the ONE entrypoint; the `SliceStack` IS the batch (all layers absorbed in one call, no per-layer sibling entry), the support plan enters as the fourth exposure class; `Fin<T>` routes kernel `GeometryFault.DegenerateInput` on an empty stack or a region set that hatches to zero geometry, lowered per `Process/faults#FAULT_BAND`; the result carries the elevation-ordered `ScanLayer` set and the `scan-vectors` `ContentKey`.
- Auto: `Plan` folds each layer: `SliceRegion.Of` lifts the kernel forest (holes by Depth parity — a bore's interior never hatches); `Partition` runs the exposure set-algebra through the region atom at depth `k = policy.SkinDepth` and merges the layer's `SupportLayer` rows under the `Support` class; `Cells` expands the pattern union into `(cell, angle, rank)` rows at `ScanLaw.HatchAngleDeg(n, θ₀)`; each class×cell intersection hatches at `budget.HatchSpacing · policy.HatchSpacingScale · class.HatchScale`, clips through `SliceRegion.Rays` (inside outers, outside holes), and sorts under the `ScanSort` pair (direction op flips vectors — `against-gas` projects against the policy's `GasFlowDeg` bearing; ordering op ranks cells and vectors through the ONE `MaxMinOrder` greedy for `thermal-spacing`); sub-spot survivors demote to `Spot` exposures; contour passes emit inward offset rings before the hatch inset; every vector row carries its class-scaled `PowerW`/`SpeedMmS` from the `Powder` budget; `Assemble` totals path length and build time and mints `ContentKey.Of(EgressKind.ScanVectors, …)` over the payload-complete little-endian program stream.
- Receipt: `ScanPlan` IS the typed evidence — per-layer `LayerGeometry` rows with vector counts and path lengths, the build-time estimate, and the content key; no generic scan ledger, no machine-dialect bytes (the vendor build-file writer is a boundary consumer of these rows, never an arm here).
- Packages: `Rasm.Meshing` (`Slicing.Apply → Fin<SliceStack>` K3 — `Elevations`/`LayerPtr`/`Depth` walked as the layer truth; the height law stays kernel `LayerPlan`), `Additive/slicing#SLICING` (`SliceRegion` — the hole-carrying region atom), `Additive/support#SUPPORT` (`SupportPlan`/`SupportLayer` — the fourth exposure class), `Geometry2D/algebra#POLYGON_ALGEBRA` (`Clip`/`ClipOpen`/`Offset` composed through the region atom — the ONE Boolean owner), `Process/physics#CUT_PARAMETER` (`RemovalBudget.Powder` — the parameter floor), `Process/owner#FABRICATION_OWNER` (`Loop`/`Edge3`/`EgressKind.ScanVectors`/`ContentKey` atoms), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>` — the pooled canonical sink), `Rasm.Numerics` (`GeometryFault`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]`), LanguageExt.Core (`Fin`/`Seq`/`Map`), BCL inbox.
- Growth: a new hatch family (spiral, fractal, contour-parallel offset fill) is one `HatchPattern` case + one `Cells` arm over the same clip/sort pipeline; a new exposure class is one `SkinRegion` row + one `SkinParameters` map entry; a new ordering heuristic is one `ScanOrdering` row; per-region parameter overrides beyond the class axis (lattice-thin-wall power) enter as `SkinParameters` map rows, never a second parameter carrier; zero new surface.
- Boundary: `Scan` is the ONE LPBF vector owner and a per-pattern generator quartet is the deleted form — patterns are cell laws over one pipeline; the layer heights are the kernel `LayerPlan` family and an elevation schedule computed here is the SEALED-boundary violation; region Booleans route `PolygonAlgebra` through `SliceRegion` and a scan-local Clipper call site or a hole-blind region set is the named duplication defect; the sorter is the two-axis `ScanSort` pair and a flattened `MeanderAlternateLinear`-style product enum is the deleted form; the gas bearing is a policy value and an inline `+X` literal is the named defect; the `.cli`/grayscale/voxel egress is `Additive/implicit`'s lane and a rasterizer here is the split-brain defect; power/speed/hatch scalars derive from the `Powder` budget row and an inline watt/speed literal in a fold body is the named defect; the content key covers the COMPLETE program and a hatch-only digest is the byte-identity defect; the content key mints through `ContentKey.Of` and a raw hasher is the second-hasher defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;                       // SliceStack — the K3 kernel slice-stack wire; heights stay kernel LayerPlan
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] ----------------------------------------------------------------------------------------------------------------------------------------
// The exposure-class axis: support regions are a CLASS here, hatched by the same fold under their own row.
[SmartEnum<string>]
public sealed partial class SkinRegion {
    public static readonly SkinRegion DownSkin = new("down-skin");
    public static readonly SkinRegion UpSkin = new("up-skin");
    public static readonly SkinRegion InSkin = new("in-skin");
    public static readonly SkinRegion Support = new("support");
}

// Direction-op × ordering-op: the sorter is TWO orthogonal axes, never a flattened product enum.
[SmartEnum<string>]
public sealed partial class ScanDirection {
    public static readonly ScanDirection Keep = new("keep");
    public static readonly ScanDirection Alternate = new("alternate");
    public static readonly ScanDirection AgainstGas = new("against-gas");
}

[SmartEnum<string>]
public sealed partial class ScanOrdering {
    public static readonly ScanOrdering Linear = new("linear");
    public static readonly ScanOrdering Nearest = new("nearest");
    public static readonly ScanOrdering ThermalSpacing = new("thermal-spacing");
}

// --- [CONSTANTS] ------------------------------------------------------------------------------------------------------------------------------------
public static class ScanLaw {
    public const double LayerRotationDeg = 66.7;    // θn = (θ0 + n·66.7°) mod 180 — the long-period rotation so no hatch seam stacks across layers
    public const double MinVectorMm = 0.05;         // sub-spot vectors DEMOTE to Spot exposures (melt-pool spot ≈ 0.08-0.1 mm), never drop

    public static double HatchAngleDeg(int layer, double theta0Deg) => (((theta0Deg + layer * LayerRotationDeg) % 180.0) + 180.0) % 180.0;
}

// --- [MODELS] ---------------------------------------------------------------------------------------------------------------------------------------
// Four cell laws over ONE Cells → Clip → Sort pipeline; parameters are case payloads, never policy knobs.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchPattern {
    private HatchPattern() { }

    public sealed record Meander : HatchPattern;
    public sealed record Stripe(double WidthMm, double OverlapMm, double StaggerMm) : HatchPattern;
    public sealed record Island(double SizeMm, double ShiftMm) : HatchPattern;
    public sealed record Hexagon(double CellMm) : HatchPattern;
}

public readonly record struct ScanSort(ScanDirection Direction, ScanOrdering Ordering);

public readonly record struct SkinParameters(double PowerScale, double SpeedScale, double HatchScale);

public sealed record ScanPolicy(
    HatchPattern Pattern,
    ScanSort Sort,
    double Theta0Deg,
    int SkinDepth,                                  // k in DownSkinₙ = Rₙ \ ⋃Rₙ₋₁..ₙ₋ₖ (and the UpSkin mirror)
    Map<SkinRegion, SkinParameters> Skins,
    int ContourPasses,
    double ContourOffsetMm,
    double HatchSpacingScale,
    double GasFlowDeg) {                            // shield-gas bearing; against-gas melts INTO the flow — a policy value, never an inline axis
    public static ScanPolicy Lpbf(HatchPattern pattern) => new(
        pattern, new ScanSort(ScanDirection.Alternate, ScanOrdering.Linear), Theta0Deg: 57.0, SkinDepth: 3,
        Map((SkinRegion.DownSkin, new SkinParameters(0.70, 1.20, 0.90)),
            (SkinRegion.UpSkin, new SkinParameters(0.85, 1.10, 0.95)),
            (SkinRegion.InSkin, new SkinParameters(1.00, 1.00, 1.00)),
            (SkinRegion.Support, new SkinParameters(0.60, 1.30, 1.20))),
        ContourPasses: 2, ContourOffsetMm: 0.12, HatchSpacingScale: 1.0, GasFlowDeg: 0.0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerGeometry {
    private LayerGeometry() { }

    public sealed record Hatch(SkinRegion Skin, Seq<Edge3> Vectors, double PowerW, double SpeedMmS) : LayerGeometry;
    public sealed record Contour(Loop Ring, int Pass, double PowerW, double SpeedMmS) : LayerGeometry;
    public sealed record Spot(Point3d At, double DwellUs) : LayerGeometry;
}

public sealed record ScanLayer(int Layer, double Elevation, Seq<LayerGeometry> Geometry, int VectorCount, double PathMm);

public sealed record ScanPlan(Seq<ScanLayer> Layers, double TotalPathM, double EstimatedBuildS, ContentKey Key);

// --- [OPERATIONS] -----------------------------------------------------------------------------------------------------------------------------------
public static class Scan {
    public static Fin<ScanPlan> Plan(SliceStack stack, ScanPolicy policy, RemovalBudget.Powder budget, Option<SupportPlan> support = default) =>
        stack.LayerCount == 0
            ? Fin.Fail<ScanPlan>(GeometryFault.DegenerateInput("scan:empty-slice-stack").ToError())
            : toSeq(Enumerable.Range(0, stack.LayerCount))
                .Map(n => Layer(stack, n, policy, budget, support))
                .Sequence()
                .Bind(Assemble);

    static Fin<ScanLayer> Layer(SliceStack stack, int n, ScanPolicy policy, RemovalBudget.Powder budget, Option<SupportPlan> support) {
        SliceRegion region = SliceRegion.Of(stack, n);
        Seq<(SkinRegion Class, SliceRegion Area)> supportRows = SupportRows(support, n);
        if (region.IsEmpty && supportRows.IsEmpty)
            return Fin.Succ(new ScanLayer(n, stack.Elevations[n], Seq<LayerGeometry>(), 0, 0.0));

        double angle = ScanLaw.HatchAngleDeg(n, policy.Theta0Deg);
        return from inset in region.IsEmpty ? Fin.Succ(region) : region.Grow(-(policy.ContourPasses * policy.ContourOffsetMm))
               from skins in Partition(stack, n, policy.SkinDepth)
               from clipped in skins.Map(skin => skin.Area.Intersect(inset).Map(cut => (skin.Class, Area: cut))).Sequence()
               let borders = Borders(region, policy, budget)
               let hatches = clipped.ToSeq().Concat(supportRows)
                   .Bind(skin => HatchClass(skin.Class, skin.Area, n, angle, policy, budget))
               let geometry = borders.Concat(hatches)
               let path = geometry.Map(Length).Sum()
               let vectors = geometry.Map(static g => g switch { LayerGeometry.Hatch h => h.Vectors.Count, _ => 1 }).Sum()
               from layer in vectors == 0 && !region.IsEmpty && policy.ContourPasses == 0
                   ? Fin.Fail<ScanLayer>(GeometryFault.DegenerateInput($"scan:zero-vectors:layer-{n}").ToError())
                   : Fin.Succ(new ScanLayer(n, stack.Elevations[n], geometry, vectors, path))
               select layer;
    }

    // --- [SKIN_PARTITION]
    // DownSkinₙ = Rₙ \ ⋃Rₙ₋₁..ₙ₋ₖ · UpSkinₙ = Rₙ \ ⋃Rₙ₊₁..ₙ₊ₖ · InSkinₙ = Rₙ \ (Down ∪ Up) — region algebra
    // over the hole-carrying atom; layer 0 is all down-skin (plate-facing).
    static Fin<Seq<(SkinRegion Class, SliceRegion Area)>> Partition(SliceStack stack, int n, int k) =>
        from below in Merged(stack, Math.Max(0, n - k), Math.Min(k, n))
        from above in Merged(stack, n + 1, Math.Max(0, Math.Min(k, stack.LayerCount - n - 1)))
        let rn = SliceRegion.Of(stack, n)
        from down in n == 0 ? Fin.Succ(rn) : rn.Difference(below)
        from up in rn.Difference(above)
        from downUp in down.Union(up)
        from core in rn.Difference(downUp)
        select Seq((SkinRegion.DownSkin, down), (SkinRegion.UpSkin, up), (SkinRegion.InSkin, core));

    static Fin<SliceRegion> Merged(SliceStack stack, int from, int count) =>
        toSeq(Enumerable.Range(from, count))
            .Map(i => SliceRegion.Of(stack, i))
            .Fold(Fin.Succ(SliceRegion.Empty), static (acc, r) => acc.Bind(held => held.Union(r)));

    static Seq<(SkinRegion Class, SliceRegion Area)> SupportRows(Option<SupportPlan> support, int layer) =>
        support.Map(plan => plan.Planar
                .Filter(row => row.Layer == layer)
                .Bind(row => Seq(row.Sparse, row.Interface))
                .Filter(static area => !area.IsEmpty)
                .Map(static area => (SkinRegion.Support, area)))
            .IfNone(Seq<(SkinRegion, SliceRegion)>());

    // --- [CELL_LAWS]
    // Every pattern is (cell loops, cell angle, sequence rank) rows over the layer bound; the hatch/clip/sort pipeline is shared.
    static Seq<(Seq<Loop> Cell, double AngleDeg, int Rank)> Cells(HatchPattern pattern, BoundingBox bound, int layer, double angleDeg) =>
        pattern.Switch(
            state:   (bound, layer, angleDeg),
            meander: static s => Seq((Plane(s.bound), s.angleDeg, 0)),
            stripe:  static (s, p) => Bands(s.bound, s.angleDeg, p.WidthMm, p.OverlapMm, p.StaggerMm * s.layer)
                                          .Map((i, band) => (band, s.angleDeg, i)).ToSeq(),
            island:  static (s, p) => Grid(s.bound, p.SizeMm, p.ShiftMm * s.layer)
                                          .Map((i, cell) => (cell.Cell, s.angleDeg + (cell.Parity ? 90.0 : 0.0), ParityRank(i, cell.Parity))).ToSeq(),
            hexagon: static (s, p) => Hexes(s.bound, p.CellMm).Map((i, hex) => (hex, s.angleDeg + (i % 3) * 60.0, i)).ToSeq());

    // Sub-spot survivors DEMOTE to Spot exposures — dwell reproduces the vector's energy at the class speed.
    static Seq<LayerGeometry> HatchClass(SkinRegion skin, SliceRegion region, int layer, double angleDeg, ScanPolicy policy, RemovalBudget.Powder budget) {
        if (region.IsEmpty) return Seq<LayerGeometry>();
        SkinParameters p = policy.Skins.Find(skin).IfNone(new SkinParameters(1.0, 1.0, 1.0));
        double spacing = Math.Max(budget.HatchSpacing * policy.HatchSpacingScale * p.HatchScale, 1e-3);
        double speed = budget.ScanSpeed * p.SpeedScale;
        BoundingBox bound = region.Bound();
        Seq<Edge3> raw = OrderCells(Cells(policy.Pattern, bound, layer, angleDeg), policy.Sort.Ordering)
            .Bind(cell =>
                PolygonAlgebra.Clip(cell.Cell, region.Outers, ClipOp.Intersect).IfFail(Seq<Loop>()) is { IsEmpty: false } cut
                    ? Sortie(new SliceRegion(cut, region.Holes).Rays(Rays(bound, cell.AngleDeg, spacing)), policy)
                    : Seq<Edge3>());
        Seq<Edge3> vectors = raw.Filter(static v => v.A.DistanceTo(v.B) >= ScanLaw.MinVectorMm);
        Seq<LayerGeometry> spots = raw.Filter(static v => v.A.DistanceTo(v.B) < ScanLaw.MinVectorMm)
            .Map(v => (LayerGeometry)new LayerGeometry.Spot(
                (v.A + v.B) * 0.5, DwellUs: 1e6 * v.A.DistanceTo(v.B) / Math.Max(speed, 1e-3)));
        return vectors.IsEmpty
            ? spots
            : Seq((LayerGeometry)new LayerGeometry.Hatch(skin, vectors, budget.LaserPower * p.PowerScale, speed)).Concat(spots);
    }

    static Seq<LayerGeometry> Borders(SliceRegion region, ScanPolicy policy, RemovalBudget.Powder budget) =>
        toSeq(Enumerable.Range(0, policy.ContourPasses)).Bind(pass =>
            region.Grow(-pass * policy.ContourOffsetMm).Map(static r => r.Outers.Concat(r.Holes)).IfFail(Seq<Loop>())
                .Map(ring => (LayerGeometry)new LayerGeometry.Contour(ring, pass, budget.LaserPower, budget.ScanSpeed)));

    // --- [SORTER]
    // Direction op flips individual vectors; ordering op ranks cells/vectors; thermal-spacing is the ONE
    // MaxMinOrder greedy over centroids, shared by the cell and vector levels.
    static Seq<Edge3> Sortie(Seq<Edge3> vectors, ScanPolicy policy) {
        Seq<Edge3> ordered = policy.Sort.Ordering.Switch(
            state:          vectors,
            linear:         static v => v,
            nearest:        static v => GreedyNearest(v),
            thermalSpacing: static v => MaxMinOrder(v, static e => (e.A + e.B) * 0.5));
        return policy.Sort.Direction.Switch(
            state:      (Ordered: ordered, Gas: policy.GasFlowDeg),
            keep:       static s => s.Ordered,
            alternate:  static s => s.Ordered.Map((i, e) => i % 2 == 1 ? new Edge3(e.B, e.A) : e).ToSeq(),
            againstGas: static s => {
                Vector3d gas = new(Math.Cos(s.Gas * Math.PI / 180.0), Math.Sin(s.Gas * Math.PI / 180.0), 0.0);
                return s.Ordered.Map(e => (e.B - e.A) * gas > 0.0 ? new Edge3(e.B, e.A) : e);   // melt travels against the flow
            });
    }

    static Fin<ScanPlan> Assemble(Seq<ScanLayer> layers) {
        double pathMm = layers.Map(static l => l.PathMm).Sum();
        double seconds = layers.Bind(static l => l.Geometry).Map(static g => g switch {
            LayerGeometry.Hatch h => Length(h) / Math.Max(h.SpeedMmS, 1e-3),
            LayerGeometry.Contour c => Length(c) / Math.Max(c.SpeedMmS, 1e-3),
            LayerGeometry.Spot s => s.DwellUs * 1e-6,
            _ => 0.0 }).Sum();
        return Fin.Succ(new ScanPlan(layers, pathMm / 1000.0, seconds, ContentKey.Of(EgressKind.ScanVectors, Canonical(layers))));
    }

    // --- [CELL_PRIMITIVES]
    static Seq<Loop> Plane(BoundingBox b) =>
        Seq(new Loop(
            Arr(new Point3d(b.Min.X, b.Min.Y, 0), new Point3d(b.Max.X, b.Min.Y, 0), new Point3d(b.Max.X, b.Max.Y, 0), new Point3d(b.Min.X, b.Max.Y, 0)),
            Closed: true));

    static Seq<Seq<Loop>> Bands(BoundingBox b, double angleDeg, double width, double overlap, double stagger) {
        double rad = angleDeg * Math.PI / 180.0, diag = b.Min.DistanceTo(b.Max);
        Vector3d step = new(-Math.Sin(rad), Math.Cos(rad), 0.0);
        Point3d centre = (b.Min + b.Max) * 0.5 + (stagger % Math.Max(width, 1e-3)) * step;
        int count = Math.Max(1, (int)Math.Ceiling(diag / Math.Max(width, 1e-3)));
        return toSeq(Enumerable.Range(-count, 2 * count + 1)).Map(i =>
            Band(centre + i * width * step, rad, diag, width + overlap));
    }

    static Seq<Loop> Band(Point3d mid, double rad, double diag, double width) {
        Vector3d dir = new(Math.Cos(rad), Math.Sin(rad), 0.0);
        Vector3d across = new(-Math.Sin(rad), Math.Cos(rad), 0.0);
        return Seq(new Loop(Arr(
            mid - 0.5 * diag * dir - 0.5 * width * across, mid + 0.5 * diag * dir - 0.5 * width * across,
            mid + 0.5 * diag * dir + 0.5 * width * across, mid - 0.5 * diag * dir + 0.5 * width * across), Closed: true));
    }

    static Seq<(Seq<Loop> Cell, bool Parity)> Grid(BoundingBox b, double size, double shift) {
        double s = Math.Max(size, 1e-3), ox = b.Min.X + shift % s, oy = b.Min.Y + shift % s;
        int nx = (int)Math.Ceiling((b.Max.X - ox) / s) + 1, ny = (int)Math.Ceiling((b.Max.Y - oy) / s) + 1;
        return toSeq(Enumerable.Range(0, nx * ny)).Map(k => {
            int i = k % nx, j = k / nx;
            Point3d lo = new(ox + (i - 1) * s, oy + (j - 1) * s, 0.0);
            Loop cell = new(Arr(lo, new Point3d(lo.X + s, lo.Y, 0), new Point3d(lo.X + s, lo.Y + s, 0), new Point3d(lo.X, lo.Y + s, 0)), Closed: true);
            return (Seq(cell), (i + j) % 2 == 0);
        });
    }

    static Seq<Seq<Loop>> Hexes(BoundingBox b, double cell) {
        double s = Math.Max(cell, 1e-3), h = s * Math.Sqrt(3.0) / 2.0;
        int nx = (int)Math.Ceiling((b.Max.X - b.Min.X) / (1.5 * s)) + 1, ny = (int)Math.Ceiling((b.Max.Y - b.Min.Y) / (2.0 * h)) + 1;
        return toSeq(Enumerable.Range(0, nx * ny)).Map(k => {
            int i = k % nx, j = k / nx;
            Point3d c = new(b.Min.X + i * 1.5 * s, b.Min.Y + j * 2.0 * h + (i % 2 == 1 ? h : 0.0), 0.0);
            return Seq(new Loop(toArr(Enumerable.Range(0, 6).Select(v =>
                new Point3d(c.X + s * Math.Cos(v * Math.PI / 3.0), c.Y + s * Math.Sin(v * Math.PI / 3.0), 0.0))), Closed: true));
        });
    }

    static int ParityRank(int i, bool parity) => parity ? i : i + (1 << 20);   // checkerboard: even-parity cells melt first, odd cells the pass after

    static Seq<Edge3> Rays(BoundingBox b, double angleDeg, double spacing) {
        double rad = angleDeg * Math.PI / 180.0, diag = b.Min.DistanceTo(b.Max);
        Point3d centre = (b.Min + b.Max) * 0.5;
        Vector3d dir = new(Math.Cos(rad), Math.Sin(rad), 0.0);
        Vector3d step = new(-Math.Sin(rad), Math.Cos(rad), 0.0);
        int lines = Math.Max(1, (int)Math.Ceiling(diag / spacing));
        return toSeq(Enumerable.Range(-lines, 2 * lines + 1)).Map(i => {
            Point3d mid = centre + i * spacing * step;
            return new Edge3(mid - 0.5 * diag * dir, mid + 0.5 * diag * dir);
        });
    }

    // --- [ORDERING_PRIMITIVES]
    static Seq<(Seq<Loop> Cell, double AngleDeg, int Rank)> OrderCells(Seq<(Seq<Loop> Cell, double AngleDeg, int Rank)> cells, ScanOrdering ordering) =>
        ordering == ScanOrdering.ThermalSpacing
            ? MaxMinOrder(cells, static c => Centroid(c.Cell))
            : cells.OrderBy(static c => c.Rank).ToSeq();

    // The ONE max-min-distance greedy: each pick maximizes its minimum distance to everything already melted.
    // Bounded O(n²) kernel shared by the cell and vector thermal-spacing arms.
    static Seq<T> MaxMinOrder<T>(Seq<T> items, Func<T, Point3d> at) {
        var pool = items.ToList();
        var ordered = new System.Collections.Generic.List<T>(pool.Count);
        while (pool.Count > 0) {
            int pick = 0;
            if (ordered.Count > 0) {
                double bestD = -1.0;
                for (int i = 0; i < pool.Count; i++) {
                    double d = ordered.Min(o => at(o).DistanceTo(at(pool[i])));
                    if (d > bestD) { bestD = d; pick = i; }
                }
            }
            ordered.Add(pool[pick]);
            pool.RemoveAt(pick);
        }
        return toSeq(ordered);
    }

    static Seq<Edge3> GreedyNearest(Seq<Edge3> v) {
        var pool = v.ToList();
        var ordered = new System.Collections.Generic.List<Edge3>(pool.Count);
        Point3d head = pool.Count > 0 ? pool[0].A : Point3d.Origin;
        while (pool.Count > 0) {
            int best = 0; double d = double.MaxValue;
            for (int i = 0; i < pool.Count; i++) { double di = head.DistanceTo(pool[i].A); if (di < d) { d = di; best = i; } }
            ordered.Add(pool[best]); head = pool[best].B; pool.RemoveAt(best);
        }
        return toSeq(ordered);
    }

    // --- [BOUNDARIES]
    static double Length(LayerGeometry g) => g switch {
        LayerGeometry.Hatch h => h.Vectors.Map(static e => e.A.DistanceTo(e.B)).Sum(),
        LayerGeometry.Contour c => toSeq(Enumerable.Range(0, c.Ring.Count)).Map(i => c.Ring.At(i).DistanceTo(c.Ring.At(i + 1))).Sum(),
        _ => 0.0 };

    // Payload-complete little-endian program stream: layer ordinal, elevation, every geometry case with its
    // discriminant, class key, power/speed/dwell, and full coordinates — the scan-vectors content-key bytes.
    static byte[] Canonical(Seq<ScanLayer> layers) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, layers.Count);
        layers.Iter(layer => {
            Int32(writer, layer.Layer);
            Float64(writer, layer.Elevation);
            Int32(writer, layer.Geometry.Count);
            layer.Geometry.Iter(g => {
                switch (g) {
                    case LayerGeometry.Hatch h:
                        Int32(writer, 0);
                        Utf8(writer, h.Skin.Key);
                        Float64(writer, h.PowerW);
                        Float64(writer, h.SpeedMmS);
                        Int32(writer, h.Vectors.Count);
                        h.Vectors.Iter(e => { Point(writer, e.A); Point(writer, e.B); });
                        break;
                    case LayerGeometry.Contour c:
                        Int32(writer, 1);
                        Int32(writer, c.Pass);
                        Float64(writer, c.PowerW);
                        Float64(writer, c.SpeedMmS);
                        Int32(writer, c.Ring.Count);
                        toSeq(Enumerable.Range(0, c.Ring.Count)).Iter(i => Point(writer, c.Ring.At(i)));
                        break;
                    case LayerGeometry.Spot s:
                        Int32(writer, 2);
                        Float64(writer, s.DwellUs);
                        Point(writer, s.At);
                        break;
                }
            });
        });
        return writer.WrittenSpan.ToArray();
    }

    static void Point(ArrayPoolBufferWriter<byte> writer, Point3d p) {
        Float64(writer, p.X);
        Float64(writer, p.Y);
    }

    static void Utf8(ArrayPoolBufferWriter<byte> writer, string value) {
        Int32(writer, value.Length);
        writer.Write(System.Text.Encoding.UTF8.GetBytes(value));
    }

    static void Int32(ArrayPoolBufferWriter<byte> writer, int value) {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    static void Float64(ArrayPoolBufferWriter<byte> writer, double value) {
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    static Point3d Centroid(Seq<Loop> cell) {
        Seq<Point3d> pts = cell.Bind(static l => toSeq(l.Vertices));
        return pts.IsEmpty ? Point3d.Origin : new Point3d(pts.Map(static p => p.X).Sum() / pts.Count, pts.Map(static p => p.Y).Sum() / pts.Count, 0.0);
    }
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Owner["owner#run Additive Scan case (Powder budget carried typed)"] --> PlanNode["Scan.Plan"]
    Stack["kernel SliceStack (K3) heights = LayerPlan, SEALED"] -->|Depth-parity holes| Region["SliceRegion.Of"]
    Region -->|"DownSkinₙ = Rₙ \\ ⋃Rₙ₋ₖ set-algebra"| Skins["SkinRegion × SkinParameters"]
    SupportP["SupportPlan planar rows"] -->|fourth exposure class| Skins
    Pattern["HatchPattern cell law Meander · Stripe · Island · Hexagon"] -->|"θₙ = (θ₀ + n·66.7°) mod 180"| Cells["Cells (cell, angle, rank)"]
    Skins --> Hatch["hatch → SliceRegion.Rays → ScanSort (gas bearing = policy)"]
    Cells --> Hatch
    Hatch -->|"sub-spot demotes"| Spot["Spot point exposures"]
    Budget["RemovalBudget.Powder laser · hatch · speed"] -->|class-scaled| Hatch
    Hatch --> Receipt["ScanPlan + payload-complete scan-vectors ContentKey"]
    Spot --> Receipt
    Receipt -->|AdditiveResult.Artifacts| Persist["Rasm.Persistence artifact index"]
```
