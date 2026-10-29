using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record OffsetResult {
    public sealed record Joined(Brep Offset) : OffsetResult;
    public sealed record Pieces(Seq<Brep> Offsets, Seq<Brep> Blends, Seq<Brep> Walls) : OffsetResult;
}

public sealed record BlendEdge(BrepEdge Edge, BlendContinuity Continuity) {
    public Option<BrepFace> Face { get; init; }

    public Option<Interval> Domain { get; init; }

    public bool Reversed { get; init; }

    internal Fin<(BrepFace Face, Interval Domain)> Side =>
        from face in Face.Match(
            Some: static held => Fin.Succ(held),
            None: () => toSeq(Edge.AdjacentFaces()) switch {
                [var single] => Edge.Brep.Faces[single],
                var faces => new RhinoFault(nameof(Edge.AdjacentFaces), RhinoReason.Mismatched, faces.Count),
            })
        select (Face: face, Domain: Domain.IfNone(Edge.Domain));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Shells {
    // --- [OFFSETS]
    public static IO<OffsetResult> Offset(Tolerances tolerances, Brep brep, double distance, bool solid, bool extend, bool shrink) =>
        from answered in IO.lift(() => (Offsets: Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerances.Absolute, out Brep[] blends, out Brep[] walls), Blends: blends, Walls: walls))
        from offsets in Products.Many(() => answered.Offsets, member: nameof(Brep.CreateOffsetBrep))
        from blends in Products.Many(() => answered.Blends, required: false, nameof(Brep.CreateOffsetBrep))
        from walls in Products.Many(() => answered.Walls, required: false, nameof(Brep.CreateOffsetBrep))
        select offsets.Count is 1 && blends.IsEmpty && walls.IsEmpty
            ? (OffsetResult)new OffsetResult.Joined(offsets[0])
            : new OffsetResult.Pieces(offsets, blends, walls);

    public static IO<Seq<Brep>> Shell(Tolerances tolerances, Brep brep, Seq<int> removedFaces, double distance) =>
        from faces in IO.lift(() => brep.Within(ComponentIndexType.BrepFace, removedFaces))
        from shelled in Products.Many(() => Brep.CreateShell(brep, faces, distance, tolerances.Absolute))
        select shelled;

    // --- [BLENDS]
    public static IO<Seq<Brep>> Blend(BlendEdge first, BlendEdge second) =>
        from sides in IO.lift(() => (first.Side, second.Side).Apply(static (start, end) => (Start: start, End: end)).As())
        from blended in Products.Many(() => Brep.CreateBlendSurface(
            sides.Start.Face, first.Edge, sides.Start.Domain, first.Reversed, first.Continuity, sides.End.Face, second.Edge, sides.End.Domain, second.Reversed, second.Continuity))
        select blended;

    public static IO<Curve> BlendShape(BlendEdge first, double t0, BlendEdge second, double t1) =>
        from sides in IO.lift(() => (first.Side, second.Side).Apply(static (start, end) => (Start: start, End: end)).As())
        from shape in Products.Of(() => Brep.CreateBlendShape(
            sides.Start.Face, first.Edge, t0, first.Reversed, first.Continuity, sides.End.Face, second.Edge, t1, second.Reversed, second.Continuity))
        select shape;
}
