using System.Globalization;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TopologyTrim {
    public sealed record Along(int Edge, bool Rev3d, Curve Curve, Option<IsoStatus> Iso) : TopologyTrim;

    public sealed record Singular(int Vertex, Curve Curve, IsoStatus Iso) : TopologyTrim;
}

public sealed record TopologyLoop(BrepLoopType Type, IterableNE<TopologyTrim> Trims);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TopologyFace {
    public sealed record Trimmed(int Surface, bool Reversed, IterableNE<TopologyLoop> Loops) : TopologyFace;

    public sealed record Planar(int Surface, IterableNE<(BrepLoopType Type, IterableNE<Curve> Boundary)> Loops) : TopologyFace;
}

public sealed record TopologyEdge(int Curve, int Start, int End) {
    public Option<Interval> Domain { get; init; }

    public Option<double> Tolerance { get; init; }
}

public sealed record BrepTopology(Seq<Surface> Surfaces, Seq<Curve> Curves, Seq<(Point3d Point, Option<double> Tolerance)> Vertices, Seq<TopologyEdge> Edges, Seq<TopologyFace> Faces);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidAssembly {
    // --- [TOPOLOGY]
    public static IO<Brep> Build(BrepTopology topology) =>
        from brep in use(static () => new Brep())
        from declared in IO.lift(() => Declared(brep, topology))
        from valid in IO.lift(() => RhinoFault.Unless(brep.IsValidTopology(out string log), RhinoReason.Invalid, log))
        select brep;

    private static Fin<Unit> Declared(Brep brep, BrepTopology topology) =>
        from surfaces in Products.Each(topology.Surfaces, (surface, at) => Conversions.Required(Conversions.Present(brep.AddSurface(surface)), at))
        from curves in Products.Each(topology.Curves, (curve, at) => Conversions.Required(Conversions.Present(brep.AddEdgeCurve(curve)), at))
        from vertices in Products.Each(topology.Vertices, (vertex, at) => Conversions.Required(brep.Vertices.Add(vertex.Point, Conversions.Unset(vertex.Tolerance)), at))
        from edges in Products.Each(topology.Edges, (edge, at) =>
            (Picked(vertices, edge.Start, at), Picked(vertices, edge.End, at), Picked(curves, edge.Curve, at)).Apply(static (start, end, curve) => (Start: start, End: end, Curve: curve)).As()
                .Bind(ends => Conversions.Required(edge.Domain.Match(
                    Some: domain => brep.Edges.Add(ends.Start, ends.End, ends.Curve, domain, Conversions.Unset(edge.Tolerance)),
                    None: () => brep.Edges.Add(ends.Start, ends.End, ends.Curve, Conversions.Unset(edge.Tolerance))), at)))
        from trims in Products.Each(topology.Faces, (face, at) => Faced((Brep: brep, Surfaces: surfaces, Vertices: vertices, Edges: edges, At: at), face))
        select Settled(brep, trims.Bind(static held => held));

    private static Fin<Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>> Faced((Brep Brep, Seq<int> Surfaces, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, TopologyFace face) =>
        face.Switch(
            of,
            trimmed: static (of, trimmed) =>
                from surface in Picked(of.Surfaces, trimmed.Surface, of.At)
                from added in Conversions.Required(of.Brep.Faces.Add(surface), of.At).Map(face => Oriented(face, trimmed.Reversed))
                from loops in Products.Each(toSeq(trimmed.Loops), (loop, _) => Conversions.Required(of.Brep.Loops.Add(loop.Type, added), of.At)
                    .Bind(held => Products.Each(toSeq(loop.Trims), (trim, _) => Trimmed((of.Brep, Loop: held, of.Vertices, of.Edges, of.At), trim))))
                select loops.Bind(static held => held),
            planar: static (of, planar) =>
                from surface in Picked(of.Surfaces, planar.Surface, of.At)
                from added in Conversions.Required(of.Brep.Faces.Add(surface), of.At)
                from loops in Products.Each(toSeq(planar.Loops), (loop, _) => Conversions.Required(of.Brep.Loops.AddPlanarFaceLoop(added.FaceIndex, loop.Type, loop.Boundary), of.At))
                select Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>());

    private static Fin<(BrepTrim Trim, Option<IsoStatus> Iso)> Trimmed((Brep Brep, BrepLoop Loop, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, TopologyTrim trim) =>
        trim.Switch(
            of,
            along: static (of, along) =>
                from edge in Picked(of.Edges, along.Edge, of.At)
                from curve in Conversions.Required(Conversions.Present(of.Brep.AddTrimCurve(along.Curve)), of.At)
                from added in Conversions.Required(of.Brep.Trims.Add(edge, along.Rev3d, of.Loop, curve), of.At)
                select (Trim: added, along.Iso),
            singular: static (of, singular) =>
                from vertex in Picked(of.Vertices, singular.Vertex, of.At)
                from curve in Conversions.Required(Conversions.Present(of.Brep.AddTrimCurve(singular.Curve)), of.At)
                from added in Conversions.Required(of.Brep.Trims.AddSingularTrim(vertex, of.Loop, singular.Iso, curve), of.At)
                select (Trim: added, Iso: Some(singular.Iso)));

    private static Fin<T> Picked<T>(Seq<T> rows, int index, int at) =>
        rows.At(index).ToFin(new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{typeof(T).Name} {index} of {rows.Count}"), RhinoReason.Outside, at));

    private static BrepFace Oriented(BrepFace face, bool reversed) {
        face.OrientationIsReversed = reversed;
        return face;
    }

    private static Unit Settled(Brep brep, Seq<(BrepTrim Trim, Option<IsoStatus> Iso)> trims) {
        brep.SetTrimIsoFlags();
        _ = trims.Iter(static held => held.Iso.Iter(iso => held.Trim.IsoStatus = iso));
        brep.SetTolerancesBoxesAndFlags(
            bLazy: true, bSetVertexTolerances: true, bSetEdgeTolerances: true, bSetTrimTolerances: true,
            bSetTrimIsoFlags: false, bSetTrimTypeFlags: true, bSetLoopTypeFlags: true, bSetTrimBoxes: true);
        return unit;
    }

    // --- [EXTRUSIONS]
    public static IO<Extrusion> ProfiledExtrusion(Line path, Vector3d up, Curve outerProfile, Seq<Curve> innerProfiles, bool cap) =>
        from made in use(static () => new Extrusion())
        from pathed in Products.Accepted(() => made.SetPathAndUp(path.From, path.To, up))
        from outer in Products.Accepted(() => made.SetOuterProfile(outerProfile, cap))
        from inner in innerProfiles.TraverseM(profile => Products.Accepted(() => made.AddInnerProfile(profile))).As()
        from valid in IO.lift(() => RhinoFault.Unless(made.IsValidWithLog(out string log), RhinoReason.Invalid, log))
        select made;
}
