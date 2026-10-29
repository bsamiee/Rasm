using System.Collections.Generic;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record BrepCut {
    public sealed record ByBrep(Brep Cutter) : BrepCut;
    public sealed record ByBreps(IterableNE<Brep> Cutters) : BrepCut;
    public sealed record ByCurves(IterableNE<Curve> Cutters) : BrepCut;
    public sealed record ByProjection(IterableNE<GeometryBase> Cutters, Vector3d Normal, bool PlanView) : BrepCut;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ConnectAt {
    public sealed record Edges(int Edge0, int Edge1) : ConnectAt;
    public sealed record Picks(Point3d Pick0, Point3d Pick1) : ConnectAt;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record WireCutter {
    public sealed record Closed(IterableNE<Curve> Curves, Option<(double Depth, bool BothSides)> Depth) : WireCutter;
    public sealed record Open(Curve Curve, Vector3d TrimDirection, bool BothSides, Option<(double Depth, double TrimDepth, bool TrimBothSides)> Depth) : WireCutter;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidTopology {
    // --- [JOINS]
    public static IO<(Brep Face0, Brep Face1)> ExtendBrepFacesToConnect(Tolerances tolerances, BrepFace face0, BrepFace face1, ConnectAt at) =>
        from answered in IO.lift(() => at.Switch(
            (Face0: face0, Face1: face1, Tolerances: tolerances),
            edges: static (state, edges) =>
                (state.Face0.Brep.Within(ComponentIndexType.BrepEdge, Seq(edges.Edge0)), state.Face1.Brep.Within(ComponentIndexType.BrepEdge, Seq(edges.Edge1)))
                    .Apply(static (_, _) => unit).As()
                    .Map(_ => (Accepted: Brep.ExtendBrepFacesToConnect(state.Face0, edges.Edge0, state.Face1, edges.Edge1, state.Tolerances.Absolute, state.Tolerances.Angle,
                        out Brep extended0, out Brep extended1), First: extended0, Second: extended1)),
            picks: static (state, picks) => (
                Accepted: Brep.ExtendBrepFacesToConnect(state.Face0, picks.Pick0, state.Face1, picks.Pick1, state.Tolerances.Absolute, state.Tolerances.Angle, out Brep extended0, out Brep extended1),
                First: extended0, Second: extended1)))
        from first in Products.Maybe(() => answered.First, nameof(Brep.ExtendBrepFacesToConnect))
        from second in Products.Maybe(() => answered.Second, nameof(Brep.ExtendBrepFacesToConnect))
        from connected in IO.lift(() =>
            from accepted in RhinoFault.Unless(answered.Accepted, RhinoReason.Declined, answered.Accepted)
            from pair in (Conversions.Required(first), Conversions.Required(second)).Apply(static (held, other) => (Face0: held, Face1: other)).As()
            select pair)
        select connected;

    // --- [SPLITS]
    public static IO<Seq<Grouped<Brep, ComponentIndex>>> SplitDisjointPieces(Brep brep) =>
        from answered in IO.lift(() => (Pieces: Brep.SplitDisjointPieces(brep, out List<int[]> map), Map: map))
        from pieces in Products.Many(() => answered.Pieces, member: nameof(Brep.SplitDisjointPieces))
        from grouped in IO.lift(() =>
            from map in Conversions.Required(answered.Map)
            from matched in RhinoFault.Unless(map.Count == pieces.Count, RhinoReason.Mismatched, map.Count)
            select pieces.Zip(toSeq(map), static (piece, faces) =>
                new Grouped<Brep, ComponentIndex>(piece, toSeq(faces).Map(static face => new ComponentIndex(ComponentIndexType.BrepFace, face)).Strict())).Strict())
        select grouped;

    public static IO<(Seq<Brep> Pieces, Option<bool> ToleranceWasRaised)> Split(Tolerances tolerances, Brep brep, BrepCut cut) =>
        cut.Switch(
            (Brep: brep, Tolerance: tolerances.Absolute),
            byBrep: static (at, cutter) =>
                from answered in IO.lift(() => (Pieces: at.Brep.Split(cutter.Cutter, at.Tolerance, out bool raised), Raised: raised))
                from pieces in Products.Many(() => answered.Pieces, required: false, nameof(Brep.Split))
                select (Pieces: pieces, ToleranceWasRaised: Some(answered.Raised)),
            byBreps: static (at, cutters) => Products.Many(() => at.Brep.Split(cutters.Cutters, at.Tolerance), required: false)
                .Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            byCurves: static (at, cutters) => Products.Many(() => at.Brep.Split(cutters.Cutters, at.Tolerance), required: false)
                .Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)),
            byProjection: static (at, cutters) => Products.Many(() => at.Brep.Split(cutters.Cutters, cutters.Normal, cutters.PlanView, at.Tolerance), required: false)
                .Map(static pieces => (Pieces: pieces, ToleranceWasRaised: Option<bool>.None)));

    public static IO<(Seq<Brep> Kept, Seq<Brep> CutAway)> WireCut(Tolerances tolerances, Brep brep, WireCutter cutter, Vector3d direction, bool splitKinkyFaces) =>
        from wired in IO.lift(() => cutter.Switch(
            (Brep: brep, Direction: direction, Tolerance: tolerances.Absolute, SplitKinkyFaces: splitKinkyFaces),
            closed: static (at, closed) => (
                Cut: at.Brep.WireCut(
                    closed.Curves, at.Direction, Conversions.Unset(closed.Depth.Map(static depth => depth.Depth)), closed.Depth.Exists(static depth => depth.BothSides),
                    at.Tolerance, at.SplitKinkyFaces, out Brep[] kept, out Brep[] cutAway),
                Kept: kept,
                CutAway: cutAway),
            open: static (at, open) => (
                Cut: at.Brep.WireCut(
                    open.Curve, at.Direction, open.TrimDirection, Conversions.Unset(open.Depth.Map(static depth => depth.Depth)), Conversions.Unset(open.Depth.Map(static depth => depth.TrimDepth)),
                    open.BothSides, open.Depth.Exists(static depth => depth.TrimBothSides), at.Tolerance, at.SplitKinkyFaces, out Brep[] kept, out Brep[] cutAway),
                Kept: kept,
                CutAway: cutAway)))
        from kept in Products.Many(() => wired.Kept, required: false, nameof(Brep.WireCut))
        from cutAway in Products.Many(() => wired.CutAway, required: false, nameof(Brep.WireCut))
        from accepted in IO.lift(() => RhinoFault.Unless(wired.Cut, RhinoReason.Declined, wired.Cut))
        select (Kept: kept, CutAway: cutAway);
}
