# [RASM_INTERSECTION_SLICE]

`Slicing.Apply(SliceOp, Op?)` owns the slice stack of `Rasm.Meshing` — one section fold composing `Intersection.Apply(IntersectOp.PlaneMesh(...))` over a parallel-plane family. Crossing existence, on-plane vertex handling, segment orientation, and chain connectivity are the intersect owner's exact machinery, composed one level up. `LayerPlan` generates the plane family rather than enumerating it: its cases are height-law data over one `March` integrator, so the next layer policy is one case carrying one height law, never a sibling planner body.

Per-layer contours arrive oriented from the composed fold — segments store `from → to` along `cut.Normal × faceNormal`, closing outer loops CCW and holes CW — and a non-watertight section lands as typed open `Chain(Closed: false)` rows or the typed `SectionFault` under the policy's `SealPosture` row. Nesting is an exact-parity containment fold over the canonical coordinates every decoder reads; QuikGraph serves in-computation only. `SliceStack`, the kernel-owned SoA forest wire, binds the `Rasm.Fabrication` and `Rasm.Compute` decoders, `Chain` rows projecting from the channels on read.

## [01]-[INDEX]

- [02]-[SLICING]: `Slicing.Apply`, the section fold — `LayerPlan` height-law generator over one `March`, the parallel per-plane `IntersectOp.PlaneMesh` fold, exact-parity nesting into the `SliceStack` SoA forest wire and its `Chain` projections.
- [03]-[DENSITY_BAR]: one owner per axis with its return rail and case count.

## [02]-[SLICING]

- Owner: `SealPosture` the `[SmartEnum]` open-section law whose `[UseDelegateFromConstructor]` `Admit` column decides fault-or-admit for a layer that carries open rows; `Containment` the `[SmartEnum]` nesting verdict — its `Record` column owns what each verdict DOES to the containment graph, so the overlap contradiction is a row and never a null, and `Of` is THE even-odd point-in-ring predicate the nesting fold here and `Meshing/offset`'s medial interior test both read; `SlicePolicy` the policy row — the seal posture, guarded `Dimension` layer ceiling, slope-bin count, parallel floor, and the composed `IntersectPolicy` every per-plane fold threads; `SliceFrame` the per-run facts computed once from the soup — datum, nesting axis, elevation extent, and the binned steepest-slope and start-sorted overhang tables the height laws read; `LayerPlan` the `[Union]` height-law generator, each case one `Fin<Arr<double>>` `Elevations(SliceFrame, SlicePolicy)` fold lowering to a `Func<double,double>` height law over the one `March` body; `SliceOp` the request record — one modality, so the modality axis lives in the plan union, never a one-case request ceremony; `SliceStack` the frozen `Arr`-column result carrier admitted through `Of`, with `ContourAt`/`LayerAt`/`RootsOf`/`Depth` projections; `Slicing` the static surface.
- Cases: `LayerPlan` cases `Uniform(Height)` · `Adaptive(CuspHeight, MinHeight, MaxHeight)` · `BySlope(Arr<(SlopeCeiling, Height)> Bands)` · `SupportInterface(BaseHeight, InterfaceHeight, InterfaceLayers, OverhangCosine)` · `AtElevations(Arr<double> Elevations)` — height-law seed data the `Rasm.Fabrication` additive lane and the `Rasm.Compute` circulation bind, the family open to one more case; `SealPosture` 2 (`Required`/`Admitted`); `Containment` 3 (`Inside`/`Outside`/`Overlap`).
- Entry: `[BoundaryAdapter] public static Fin<SliceStack> Apply(SliceOp op, Op? key = null)` — the one entry, resolving the key every interior static then takes outright. `Fin<T>` routes `GeometryFault.DegenerateInput(Kind, index, witness)` on an inadmissible request and `GeometryFault.SectionFault(layer, elevation, openChains)` on a layer defect — a non-watertight layer under `SealPosture.Required` or a nesting contradiction, a containment cycle or multi-parent reduction. Composed per-plane failures surface unchanged; the fold never re-labels a sibling's typed fault. No `SliceUniform`/`SliceAdaptive`/`SliceAt` siblings — one polymorphic `Apply`, the plan case discriminating.
- Auto: `SliceFrame.Of` makes one soup pass — projecting vertices onto the datum normal for the extent, binning per-face `|n·d|` into the max-slope table, and collecting start-sorted overhang rows so the interface law filters past its own `OverhangCosine` at read, never a frame re-pass per plan. `Elevations` folds the plan's generated switch: `Adaptive` is the cusp-height bound `clamp(cusp / maxSlope, hMin, hMax)` — the geometric-error law, a height-`h` layer over cosine `|n·d|` leaving cusp `c = h·|n·d|`, so flat caps force fine layers and vertical walls admit coarse ones; `BySlope`'s band table IS the law; `SupportInterface` opens an interface band around overhangs steeper than its cosine floor; `AtElevations` validates finite, strictly ascending, in-extent. `March` is the one integrator, `MaxLayers`-gated. `ParallelHelper` partitions the plane family across pooled result slots the section fold rents — each plane's sweep independent, the fold the parallel axis and the intersect owner single-threaded per plane. Assembly drains the slots in layer order: the seal posture's `Admit` fires on any layer carrying open rows, closed rings append their vertices without the duplicate terminal, and nesting runs per layer — bbox-pruned pairs cast an exact `+U` ray whose RAW crossing count classifies even-odd into a `Containment` row, each row's `Record` column folding into the containment DAG, `ComputeTransitiveReduction` yields the immediate-parent forest (laminar ⇒ in-degree ≤ 1, a violation faults), and an in-degree-0 contour keeps `Parent = -1`, the root encoding. `SliceStack.Of` materializes the channels once as frozen `Arr` columns — never live pool leases on the wire — and re-proves the whole parent forest acyclic there, so `Depth` needs no cycle guard of its own.
- Output: `SliceStack`, the result and the wire at once — its channels carry the layer, contour, nesting-forest, open-chain, and elevation census and the `Chain` projections read over them; the hash-eligible artifacts are the frozen channel arrays, never the pooled writers or slots.
- Law: `SlicePolicy`'s three counts are guarded `Dimension` values, so a nonpositive ceiling, bin count, or floor is unrepresentable and the record carries no evidence fold of its own; the composed `IntersectPolicy` keeps its own. Height-law bounds validate ONCE at `LayerPlan.Admit`, so `March` never sees a non-positive step.
- Exemption: the mutable tables live inside one assembly sweep and publish only through `SliceStack.Of` — the `layerPtr`/`contourPtr`/`parent`/`open` channel accumulators, the pooled `MemoryOwner` slot span the parallel fold writes, and the `childPtr`/`cursor` CSR scratch pair.
- Packages: `Rasm.Meshing` (sibling — `Intersection.Apply`/`IntersectOp.PlaneMesh`/`IntersectResult.Chains`/`Chain`/`IntersectPolicy` composed never re-founded, `MeshEdit.Of` the one soup adapter, `MeshSpace`), `Rasm.Numerics` (`Predicate.Orient2D`/`Predicate.Compare` + `Sign`/`Axis` the exact nesting signs, `Dimension` the guarded counts, `GeometryFault`), `Rasm.Domain` (`Op`, `Kind`, `ValidityClaim`/`IValidityEvidence`), `Rhino.Geometry` (`Point3d`/`Vector3d`/`Plane`/`Polyline`/`BoundingBox`), QuikGraph (`BidirectionalGraph`/`AddVertexRange`/`AddEdge`/`IsDirectedAcyclicGraph`/`ComputeTransitiveReduction`/`InDegree`/`InEdge` — every member the deepest binding rung for its concern, in-computation only per the bounded-lane law), CommunityToolkit.HighPerformance (`MemoryOwner<T>` slots, `ArrayPoolBufferWriter<T>` channel emit, `ParallelHelper.For` + `IAction`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Arr<T>` the frozen channel carrier), BCL (`Array.BinarySearch` over the frame's own sorted overhang starts).
- Growth: a new layer policy is one `LayerPlan` case carrying its height law into the same `March`; a new open-section posture is one `SealPosture` row carrying its own `Admit`; a per-layer plane-slab broad-phase prune is the recorded growth row on `Spatial/index` (a plane-slab `SpatialQuery` case, never a slice-local acceleration structure); a further per-layer metric follows the `AreaAt`/`PerimeterAt`/`CentroidAt` projection rows over the existing channels; one more wire channel is a further frozen column the decoders re-bind loudly; zero new entry surface.
- Boundary: the slice owner composes `Intersection.Apply` — a slice-local plane sweep, crossing kernel, or chain walker re-founds geometry that has one owner; contour orientation is inherited from intersect's material-oriented accumulation, so a slice-side re-orientation pass repeats a decision the fold already made. Open sections are typed rows or `GeometryFault.SectionFault` under `SealPosture.Required`, never silent closure or drop. Nesting verdicts are exact parity signs — the bbox prune alone is float, a winding-number point-in-polygon with epsilon ray offsets is the deleted form — and a hand-rolled O(C²) immediate-parent scan re-does what `ComputeTransitiveReduction` owns. Wire storage is the frozen channel schema; a `Seq<Seq<Chain>>` nested-collection result beside it is a dual carriage, typed rows minting from the channels instead. Channel arrays materialize at freeze and the pool dies at assembly end, so no pooled lease crosses the seam. `Apply` is total over the `Fin` rail — a thrown exception on a degenerate plan or non-watertight layer is unrepresentable.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Meshing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SealPosture {
    public static readonly SealPosture Required = new(static (layer, elevation, open) =>
        Fin.Fail<Unit>(new GeometryFault.SectionFault(layer, elevation, open)));
    public static readonly SealPosture Admitted = new(static (_, _, _) => Fin.Succ(unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Admit(int layer, double elevation, int openRows);
}

[SmartEnum]
internal sealed partial class Containment {
    public static readonly Containment Inside = new(static (graph, outer, inner, _) => {
        graph.AddEdge(new SEdge<int>(outer, inner));
        return Fin.Succ(unit);
    });
    public static readonly Containment Outside = new(static (_, _, _, _) => Fin.Succ(unit));
    public static readonly Containment Overlap = new(static (_, _, _, contradiction) => Fin.Fail<Unit>(contradiction()));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Record(BidirectionalGraph<int, SEdge<int>> graph, int outer, int inner, Func<Error> contradiction);

    internal static Containment Of(Point3d probe, Polyline ring, Axis plane, Axis vAxis) {
        int crossings = 0;
        for (int i = 0; i < ring.Count - 1; i++) {
            (Point3d s, Point3d t) = (ring[i], ring[i + 1]);
            Sign sv = Predicate.Compare(new Implicit(s), new Implicit(probe), vAxis);
            Sign tv = Predicate.Compare(new Implicit(t), new Implicit(probe), vAxis);
            bool sBelow = sv == Sign.Negative;
            bool tBelow = tv == Sign.Negative;
            if (sBelow == tBelow) { continue; }
            Sign side = Predicate.Orient2D(new Implicit(s), new Implicit(t), new Implicit(probe), plane);
            if (side == Sign.Zero) { return Overlap; }
            if (sBelow ? side == Sign.Positive : side == Sign.Negative) { crossings++; }
        }
        return (crossings & 1) != 0 ? Inside : Outside;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record SlicePolicy(SealPosture Seal, Dimension MaxLayers, Dimension FrameBins, Dimension ParallelFloor, IntersectPolicy Intersect) {
    public static readonly SlicePolicy Canonical = new(
        Seal: SealPosture.Admitted, MaxLayers: Dimension.Create(value: 1 << 14),
        FrameBins: Dimension.Create(value: 256), ParallelFloor: Dimension.Create(value: 1),
        Intersect: IntersectPolicy.Canonical);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SliceFrame(Plane Datum, Axis Vertical, double Lo, double Hi, double[] MaxSlope, double[] OverhangStarts, double[] OverhangCosines) {
    internal static Fin<SliceFrame> Of(MeshSpace mesh, Plane datum, SlicePolicy policy, Op key) {
        using MeshEdit soup = MeshEdit.Of(mesh);
        Vector3d d = datum.Normal;
        d.Unitize();
        (double lo, double hi) = (double.PositiveInfinity, double.NegativeInfinity);
        for (int v = 0; v < soup.VertexCount; v++) {
            double e = (soup.Position(v) - datum.Origin) * d;
            (lo, hi) = (double.Min(lo, e), double.Max(hi, e));
        }
        double span = double.Max(hi - lo, EpsilonPolicy.ZeroTolerance);
        int bins = policy.FrameBins.Value;
        double[] slope = new double[bins];
        List<(double Start, double Cos)> overhang = [];
        int Bin(double e) => int.Clamp((int)((e - lo) / span * bins), 0, bins - 1);
        for (int f = 0; f < soup.FaceCount; f++) {
            (int a, int b, int c) = soup.Face(f);
            (double ea, double eb, double ec) = ((soup.Position(a) - datum.Origin) * d, (soup.Position(b) - datum.Origin) * d, (soup.Position(c) - datum.Origin) * d);
            Vector3d n = Vector3d.CrossProduct(soup.Position(b) - soup.Position(a), soup.Position(c) - soup.Position(a));
            if (!n.Unitize()) { continue; }
            double cos = n * d;
            (double fl, double fh) = (Math.Min(ea, Math.Min(eb, ec)), Math.Max(ea, Math.Max(eb, ec)));
            for (int k = Bin(fl); k <= Bin(fh); k++) { slope[k] = double.Max(slope[k], Math.Abs(cos)); }
            if (cos < 0.0) { overhang.Add((fl, -cos)); }
        }
        (double Start, double Cos)[] rows = [.. overhang.OrderBy(static row => row.Start)];
        return Axis.DominantOf(d, key).Map(vertical => new SliceFrame(datum, vertical, lo, hi, slope, [.. rows.Select(static row => row.Start)], [.. rows.Select(static row => row.Cos)]));
    }

    public double SteepestSlope(double z, double ahead) {
        double span = double.Max(Hi - Lo, EpsilonPolicy.ZeroTolerance);
        int a = int.Clamp((int)((z - Lo) / span * MaxSlope.Length), 0, MaxSlope.Length - 1);
        int b = int.Clamp((int)((z + ahead - Lo) / span * MaxSlope.Length), 0, MaxSlope.Length - 1);
        double peak = 0.0;
        for (int k = a; k <= b; k++) { peak = double.Max(peak, MaxSlope[k]); }
        return double.Max(peak, EpsilonPolicy.ZeroTolerance);
    }

    public bool NearInterface(double z, double band, double cosineFloor) {
        int at = Array.BinarySearch(OverhangStarts, z - band);
        for (int i = at >= 0 ? at : ~at; i < OverhangStarts.Length && OverhangStarts[i] <= z + band; i++) {
            if (OverhangCosines[i] >= cosineFloor) { return true; }
        }
        return false;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerPlan {
    private LayerPlan() { }

    public sealed record Uniform(double Height) : LayerPlan;
    public sealed record Adaptive(double CuspHeight, double MinHeight, double MaxHeight) : LayerPlan;
    public sealed record BySlope(Arr<(double SlopeCeiling, double Height)> Bands) : LayerPlan;
    public sealed record SupportInterface(double BaseHeight, double InterfaceHeight, int InterfaceLayers, double OverhangCosine) : LayerPlan;
    public sealed record AtElevations(Arr<double> Elevations) : LayerPlan;

    public Fin<Arr<double>> Elevations(SliceFrame frame, SlicePolicy policy) =>
        Admit().Bind(_ => Switch(
            state: (Frame: frame, Policy: policy),
            uniform:          static (s, u) => March(s.Frame, s.Policy, _ => u.Height),
            adaptive:         static (s, a) => March(s.Frame, s.Policy, z => Math.Clamp(a.CuspHeight / s.Frame.SteepestSlope(z, a.MaxHeight), a.MinHeight, a.MaxHeight)),
            bySlope:          static (s, b) => March(s.Frame, s.Policy, z => BandHeight(b.Bands, s.Frame.SteepestSlope(z, b.Bands.Fold(0.0, static (m, row) => double.Max(m, row.Height))))),
            supportInterface: static (s, i) => March(s.Frame, s.Policy, z => s.Frame.NearInterface(z, i.InterfaceLayers * i.InterfaceHeight, i.OverhangCosine) ? i.InterfaceHeight : i.BaseHeight),
            atElevations:     static (s, x) => x.Elevations.ForAll(e => e > s.Frame.Lo && e < s.Frame.Hi)
                && Enumerable.Range(1, int.Max(x.Elevations.Count - 1, 0)).All(i => x.Elevations[i - 1] < x.Elevations[i])
                    ? Fin.Succ(x.Elevations)
                    : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Plane, None, "explicit elevations out of extent or unsorted"))));

    Fin<Unit> Admit() => Switch(
        uniform:          static u => Gate(u.Height > 0.0, "non-positive layer height"),
        adaptive:         static a => Gate(a.CuspHeight > 0.0 && a.MinHeight > 0.0 && a.MaxHeight >= a.MinHeight, "degenerate cusp bounds"),
        bySlope:          static b => Gate(b.Bands.Count > 0 && b.Bands.ForAll(static row => row.Height > 0.0 && row.SlopeCeiling is > 0.0 and <= 1.0), "degenerate slope bands"),
        supportInterface: static i => Gate(i.BaseHeight > 0.0 && i.InterfaceHeight > 0.0 && i.InterfaceLayers > 0 && i.OverhangCosine is > 0.0 and <= 1.0, "degenerate interface plan"),
        atElevations:     static x => Gate(x.Elevations.Count > 0 && x.Elevations.ForAll(static e => double.IsFinite(e)), "empty or non-finite elevation family"));

    static Fin<Unit> Gate(bool holds, string witness) =>
        holds ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Plane, None, witness));

    static Fin<Arr<double>> March(SliceFrame frame, SlicePolicy policy, Func<double, double> height) {
        List<double> rows = [];
        for (double z = frame.Lo + height(frame.Lo); z < frame.Hi; z += height(z)) {
            if (rows.Count >= policy.MaxLayers.Value) {
                return Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Plane, rows.Count, "layer plan exceeds MaxLayers"));
            }
            rows.Add(z);
        }
        return Fin.Succ(new Arr<double>([.. rows]));
    }

    static double BandHeight(Arr<(double SlopeCeiling, double Height)> bands, double slope) {
        foreach ((double ceiling, double height) in bands) {
            if (slope <= ceiling) { return height; }
        }
        return bands[bands.Count - 1].Height;
    }
}

public sealed record SliceOp(MeshSpace Mesh, Plane Datum, LayerPlan Plan, SlicePolicy Policy);

public sealed record SliceStack {
    private SliceStack(Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> x, Arr<double> y,
        Arr<double> z, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open) =>
        (Elevations, LayerPtr, ContourPtr, X, Y, Z, Parent, ChildPtr, Children, Open) =
            (elevations, layerPtr, contourPtr, x, y, z, parent, childPtr, children, open);

    public Arr<double> Elevations { get; init; }
    public Arr<int> LayerPtr { get; init; }
    public Arr<int> ContourPtr { get; init; }
    public Arr<double> X { get; init; }
    public Arr<double> Y { get; init; }
    public Arr<double> Z { get; init; }
    public Arr<int> Parent { get; init; }
    public Arr<int> ChildPtr { get; init; }
    public Arr<int> Children { get; init; }
    public Arr<bool> Open { get; init; }

    internal static Fin<SliceStack> Of(Arr<double> elevations, Arr<int> layerPtr, Arr<int> contourPtr, Arr<double> x,
        Arr<double> y, Arr<double> z, Arr<int> parent, Arr<int> childPtr, Arr<int> children, Arr<bool> open, Op key) {
        BidirectionalGraph<int, SEdge<int>> forest = new(allowParallelEdges: false);
        forest.AddVertexRange(Enumerable.Range(0, parent.Count));
        for (int c = 0; c < parent.Count; c++) {
            if (parent[c] >= 0) { forest.AddEdge(new SEdge<int>(parent[c], c)); }
        }
        return forest.IsDirectedAcyclicGraph<int, SEdge<int>>()
            ? Fin.Succ(new SliceStack(elevations, layerPtr, contourPtr, x, y, z, parent, childPtr, children, open))
            : Fin.Fail<SliceStack>(new GeometryFault.SectionFault(elevations.Count, 0.0, parent.Count));
    }

    public int LayerCount => Elevations.Count;
    public int ContourCount => ContourPtr.Count - 1;
    public bool IsOpen(int contour) => Open[contour];

    public Chain ContourAt(int contour) {
        bool closed = !IsOpen(contour);
        Polyline polyline = new();
        for (int v = ContourPtr[contour]; v < ContourPtr[contour + 1]; v++) { polyline.Add(new Point3d(X[v], Y[v], Z[v])); }
        if (closed && polyline.Count > 0) { polyline.Add(polyline[0]); }
        return new Chain(polyline, closed);
    }

    public Seq<Chain> LayerAt(int layer) =>
        toSeq(Enumerable.Range(LayerPtr[layer], LayerPtr[layer + 1] - LayerPtr[layer]).Select(ContourAt));

    public IEnumerable<int> RootsOf(int layer) {
        for (int c = LayerPtr[layer]; c < LayerPtr[layer + 1]; c++) {
            if (Parent[c] < 0 && !IsOpen(c)) { yield return c; }
        }
    }

    public int Depth(int contour) {
        int depth = 0;
        for (int at = Parent[contour]; at >= 0; at = Parent[at]) { depth++; }
        return depth;
    }

    public double AreaAt(int layer) {
        double area = 0.0;
        for (int c = LayerPtr[layer]; c < LayerPtr[layer + 1]; c++) {
            if (IsOpen(c)) { continue; }
            for (int v = ContourPtr[c]; v < ContourPtr[c + 1]; v++) {
                int w = v + 1 < ContourPtr[c + 1] ? v + 1 : ContourPtr[c];
                area += (X[v] * Y[w]) - (X[w] * Y[v]);
            }
        }
        return area / 2.0;
    }

    public double PerimeterAt(int layer) {
        double length = 0.0;
        for (int c = LayerPtr[layer]; c < LayerPtr[layer + 1]; c++) {
            if (IsOpen(c)) { continue; }
            for (int v = ContourPtr[c]; v < ContourPtr[c + 1]; v++) {
                int w = v + 1 < ContourPtr[c + 1] ? v + 1 : ContourPtr[c];
                length += Math.Sqrt(((X[w] - X[v]) * (X[w] - X[v])) + ((Y[w] - Y[v]) * (Y[w] - Y[v])));
            }
        }
        return length;
    }

    public Point3d CentroidAt(int layer) {
        (double mx, double my, double area) = (0.0, 0.0, 0.0);
        for (int c = LayerPtr[layer]; c < LayerPtr[layer + 1]; c++) {
            if (IsOpen(c)) { continue; }
            for (int v = ContourPtr[c]; v < ContourPtr[c + 1]; v++) {
                int w = v + 1 < ContourPtr[c + 1] ? v + 1 : ContourPtr[c];
                double cross = (X[v] * Y[w]) - (X[w] * Y[v]);
                mx += (X[v] + X[w]) * cross;
                my += (Y[v] + Y[w]) * cross;
                area += cross;
            }
        }
        if (Math.Abs(area) > EpsilonPolicy.ZeroTolerance) { return new Point3d(mx / (3.0 * area), my / (3.0 * area), Elevations[layer]); }
        (int first, int last) = (ContourPtr[LayerPtr[layer]], ContourPtr[LayerPtr[layer + 1]]);
        (double sx, double sy) = (0.0, 0.0);
        for (int v = first; v < last; v++) { sx += X[v]; sy += Y[v]; }
        int count = Math.Max(1, last - first);
        return new Point3d(sx / count, sy / count, Elevations[layer]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Slicing {
    [BoundaryAdapter]
    public static Fin<SliceStack> Apply(SliceOp op, Op? key = null) {
        Op site = key.OrDefault();
        return Admit(op)
            .Bind(_ => SliceFrame.Of(op.Mesh, op.Datum, op.Policy, site))
            .Bind(frame => op.Plan.Elevations(frame, op.Policy).Bind(elevations => Fold(op, frame, elevations, site)));
    }

    static Fin<Unit> Admit(SliceOp op) =>
        !op.Datum.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Plane, None, "non-finite datum plane"))
        : op.Mesh.Native.Faces.Count == 0 ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh"))
        : !op.Policy.Intersect.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "invalid intersect policy"))
        : Fin.Succ(unit);

    readonly struct SectionAction(MeshSpace mesh, Plane datum, ReadOnlyMemory<double> elevations, IntersectPolicy policy, Memory<Fin<IntersectResult>> slots, Op key) : IAction {
        public void Invoke(int i) {
            double e = elevations.Span[i];
            Plane cut = new(datum.Origin + (e * datum.Normal), datum.XAxis, datum.YAxis);
            slots.Span[i] = Intersection.Apply(new IntersectOp.PlaneMesh(cut, mesh, policy), key);
        }
    }

    static Fin<SliceStack> Fold(SliceOp op, SliceFrame frame, Arr<double> elevations, Op key) {
        int layers = elevations.Count;
        using MemoryOwner<Fin<IntersectResult>> slots = MemoryOwner<Fin<IntersectResult>>.Allocate(layers);
        double[] family = [.. elevations];
        ParallelHelper.For(0, layers, new SectionAction(op.Mesh, op.Datum, family, op.Policy.Intersect, slots.Memory, key), op.Policy.ParallelFloor.Value);

        using ArrayPoolBufferWriter<double> x = new();
        using ArrayPoolBufferWriter<double> y = new();
        using ArrayPoolBufferWriter<double> z = new();
        (List<int> layerPtr, List<int> contourPtr, List<int> parent, List<bool> open) = ([0], [0], [], []);

        Fin<Unit> Layer(int k) =>
            slots.Span[k]
                .Bind(static result => result is IntersectResult.Chains chains
                    ? Fin.Succ(chains.Walked)
                    : Fin.Fail<Seq<Chain>>(new GeometryFault.IntersectionFault(PrimitiveKind.Plane, PrimitiveKind.Mesh)))
                .Bind(walked => {
                    Seq<Chain> closed = walked.Filter(static chain => chain.Closed);
                    Seq<Chain> openRows = walked.Filter(static chain => !chain.Closed);
                    return (openRows.IsEmpty ? Fin.Succ(unit) : op.Policy.Seal.Admit(k, family[k], openRows.Count)).Bind(_ => {
                        int baseOrdinal = contourPtr.Count - 1;
                        foreach (Chain chain in closed.Concat(openRows)) {
                            int extent = chain.Closed ? chain.Points.Count - 1 : chain.Points.Count;
                            (Span<double> sx, Span<double> sy, Span<double> sz) =
                                (x.GetSpan(sizeHint: extent), y.GetSpan(sizeHint: extent), z.GetSpan(sizeHint: extent));
                            for (int v = 0; v < extent; v++) {
                                (sx[v], sy[v], sz[v]) = (chain.Points[v].X, chain.Points[v].Y, chain.Points[v].Z);
                            }
                            x.Advance(count: extent); y.Advance(count: extent); z.Advance(count: extent);
                            contourPtr.Add(contourPtr[^1] + extent);
                            open.Add(!chain.Closed);
                            parent.Add(-1);
                        }
                        return Nest(frame, closed, baseOrdinal, parent, k, family[k], openRows.Count)
                            .Map(_ => { layerPtr.Add(contourPtr.Count - 1); return unit; });
                    });
                });

        return toSeq(Enumerable.Range(0, layers))
            .Fold(Fin.Succ(unit), (state, k) => state.Bind(_ => Layer(k)))
            .Bind(_ => {
                int contours = contourPtr.Count - 1;
                int[] childPtr = new int[contours + 1];
                foreach (int p in parent) { if (p >= 0) { childPtr[p + 1]++; } }
                for (int c = 0; c < contours; c++) { childPtr[c + 1] += childPtr[c]; }
                int[] children = new int[parent.Count(static p => p >= 0)];
                int[] cursor = (int[])childPtr.Clone();
                for (int c = 0; c < contours; c++) { if (parent[c] >= 0) { children[cursor[parent[c]]++] = c; } }
                return SliceStack.Of(
                    elevations: new Arr<double>(family), layerPtr: new Arr<int>([.. layerPtr]), contourPtr: new Arr<int>([.. contourPtr]),
                    x: new Arr<double>(x.WrittenSpan.ToArray()), y: new Arr<double>(y.WrittenSpan.ToArray()), z: new Arr<double>(z.WrittenSpan.ToArray()),
                    parent: new Arr<int>([.. parent]), childPtr: new Arr<int>(childPtr), children: new Arr<int>(children),
                    open: new Arr<bool>([.. open]), key: key);
            });
    }

    // --- [NESTING]
    static Fin<Unit> Nest(SliceFrame frame, Seq<Chain> closed, int baseOrdinal, List<int> parent, int layer, double elevation, int openCount) {
        int n = closed.Count;
        if (n <= 1) { return Fin.Succ(unit); }
        Axis v = Axis.Get(frame.Vertical.V);
        (double LoU, double HiU, double LoV, double HiV)[] boxes = new (double LoU, double HiU, double LoV, double HiV)[n];
        Point3d[] anchors = new Point3d[n];
        for (int i = 0; i < n; i++) {
            (boxes[i], anchors[i]) = Extremes(closed[i].Points, frame.Vertical);
        }
        BidirectionalGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, n));
        Error Contradiction() => new GeometryFault.SectionFault(layer, elevation, openCount);
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                if (i == j || boxes[i].LoU < boxes[j].LoU || boxes[i].HiU > boxes[j].HiU || boxes[i].LoV < boxes[j].LoV || boxes[i].HiV > boxes[j].HiV) { continue; }
                Fin<Unit> recorded = Containment.Of(anchors[i], closed[j].Points, frame.Vertical, v).Record(graph, outer: j, inner: i, Contradiction);
                if (recorded.IsFail) { return recorded; }
            }
        }
        if (!graph.IsDirectedAcyclicGraph<int, SEdge<int>>()) { return Fin.Fail<Unit>(Contradiction()); }
        BidirectionalGraph<int, SEdge<int>> forest = graph.ComputeTransitiveReduction();
        foreach (int inner in forest.Vertices) {
            if (forest.InDegree(inner) > 1) { return Fin.Fail<Unit>(Contradiction()); }
            if (forest.InDegree(inner) == 1) { parent[baseOrdinal + inner] = baseOrdinal + forest.InEdge(inner, 0).Source; }
        }
        return Fin.Succ(unit);
    }

    static ((double, double, double, double) Box, Point3d Anchor) Extremes(Polyline ring, Axis vertical) {
        (double loU, double hiU, double loV, double hiV) = (double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity);
        Point3d anchor = ring[0];
        (double aU, double aV) = (Axis.Coord(anchor, vertical.U), Axis.Coord(anchor, vertical.V));
        for (int i = 0; i < ring.Count - 1; i++) {
            (double pu, double pv) = (Axis.Coord(ring[i], vertical.U), Axis.Coord(ring[i], vertical.V));
            (loU, hiU, loV, hiV) = (double.Min(loU, pu), double.Max(hiU, pu), double.Min(loV, pv), double.Max(hiV, pv));
            if (pu > aU || (pu == aU && pv > aV)) { (anchor, aU, aV) = (ring[i], pu, pv); }
        }
        return ((loU, hiU, loV, hiV), anchor);
    }
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
    accTitle: Slice stack flow
    accDescr: Layer plans generate plane families folded per plane through the intersect owner into nested contours frozen as the slice stack wire.
    SliceOp -->|SliceFrame.Of — one soup pass| SliceFrame
    SliceFrame -->|height laws over ONE March| LayerPlan
    LayerPlan -->|elevation family| Fold["parallel plane fold"]
    Fold -->|IntersectOp.PlaneMesh per plane| Intersection
    Intersection -->|oriented Chains — outer CCW / holes CW| Fold
    Fold -->|exact parity signs → Containment rows| Predicate
    Fold -->|containment DAG → transitive reduction| QuikGraph
    QuikGraph -->|immediate-parent forest| SliceStack
    SliceStack -->|five-channel SoA wire| Decoders["Fabrication Additive/slicing · Compute circulation"]
    SliceOp -.->|DegenerateInput / SectionFault| GeometryFault
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. Each `[RAIL]` cell names the one return rail its owner exposes; the per-axis kind rides the indexed notes below.

| [INDEX] | [AXIS_CONCERN]  | [OWNER]       | [RAIL]                                  | [CASES] |
| :-----: | :-------------- | :------------ | :-------------------------------------- | :-----: |
|  [01]   | Slice stack     | `SliceOp`     | `Slicing.Apply → Fin<SliceStack>`       |    —    |
|  [02]   | Layer policies  | `LayerPlan`   | `Elevations → Fin<Arr<double>>`         |    5    |
|  [03]   | Run facts       | `SliceFrame`  | value (read by the height laws)         |    —    |
|  [04]   | Slice policy    | `SlicePolicy` | value (guarded `Dimension` columns)     |    —    |
|  [05]   | Seal posture    | `SealPosture` | policy rows (`Admit → Fin<Unit>`)       |    2    |
|  [06]   | Nesting verdict | `Containment` | rows + `Of → Containment` (predicate)   |    3    |
|  [07]   | Result + wire   | `SliceStack`  | `Of → Fin<SliceStack>` (frozen columns) |    —    |

- [01]-[SLICE_STACK]: request record (one modality; the plan union is the modality axis) folded by ONE `Apply`.
- [02]-[LAYER_POLICIES]: `[Union]` height-law seed rows over ONE `March` integrator (`[GENERATOR_LAW]`).
- [03]-[RUN_FACTS]: derived record — extent, binned steepest slope, overhang start+cosine rows, nesting axis (one soup pass).
- [04]-[SLICE_POLICY]: policy row — seal posture · layer ceiling · bins · parallel floor · composed `IntersectPolicy`.
- [05]-[SEAL_POSTURE]: open-section law as rows, each carrying its own admission delegate.
- [06]-[NESTING_VERDICT]: containment verdicts as rows, each carrying what it records on the DAG; `Of` is the estate's ONE even-odd ring predicate.
- [07]-[RESULT_WIRE]: frozen `Arr` SoA forest wire admitted through `Of` + `ContourAt`/`LayerAt`/`RootsOf`/`Depth` typed projections.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
