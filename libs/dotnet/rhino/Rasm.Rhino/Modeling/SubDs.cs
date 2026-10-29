namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ChainSeed {
    public sealed record Sorted(Seq<uint> Edges) : ChainSeed;

    public sealed record Grown(uint Edge, ChainDirection Direction, SubDChainType Type) : ChainSeed;
}

public sealed record SubDChain(Seq<(uint Edge, bool Reversed)> Edges, bool Closed, Option<NurbsCurve> Curve, Option<NurbsCurve> LoftCurve);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    // --- [STEPS]
    public static Step<SubD> FillHole(uint boundaryEdge) =>
        new(subd => IO.lift(() =>
            from held in Held(subd, ComponentIndexType.SubdEdge, [boundaryEdge])
            from boundary in Boundary(Boundaries(subd), boundaryEdge)
            from closed in RhinoFault.Unless(boundary.Closed, RhinoReason.Invalid, boundaryEdge)
            from filled in RhinoFault.Unless(subd.Faces.Add(boundary.Edges) is not null, RhinoReason.Declined, boundaryEdge)
            select subd), nameof(SubD.Faces));

    public static Step<SubD> Bridge(uint edgeA, uint edgeB) =>
        new(subd => IO.lift(() =>
            from held in Held(subd, ComponentIndexType.SubdEdge, [edgeA, edgeB])
            let boundaries = Boundaries(subd)
            from ends in (Boundary(boundaries, edgeA), Boundary(boundaries, edgeB)).Apply(static (first, second) => (First: first, Second: second)).As()
            from apart in RhinoFault.Unless(!ends.First.Edges.Exists(link => link.Id == edgeB), RhinoReason.Repeated, (edgeA, edgeB))
            from even in RhinoFault.Unless(ends.First.Edges.Count == ends.Second.Edges.Count, RhinoReason.Mismatched, (ends.First.Edges.Count, ends.Second.Edges.Count))
            from faces in Products.Each(
                Anchored(ends.First, edgeA).Zip(Anchored(ends.Second with { Edges = ends.Second.Edges.Rev() }, edgeB), static (a, b) => (A: a, B: b)).Strict(),
                (rail, index) => Quad(subd, rail.A, rail.B).ToFin(new RhinoFault(nameof(SubD.Faces), RhinoReason.Declined, index)))
            select subd), nameof(SubD.Faces));

    private static Fin<Seq<int>> Held(SubD subd, ComponentIndexType kind, Seq<uint> ids) => subd.Within(kind, ids.Map(static id => (int)id));

    private static Seq<(Seq<SubDEdge> Edges, bool Closed)> Boundaries(SubD subd) =>
        toSeq(SubDEdgeChain.SortEdgesIntoEdgeChains(subd, toSeq(subd.Edges).Filter(static edge => edge.FaceCount is 1))).Map(Free).Strict();

    private static (Seq<SubDEdge> Edges, bool Closed) Free(SubDEdge[] links) =>
        toSeq(links).Map(static edge => edge.RelativeFaceLeft is null ? edge : (SubDEdge)edge.ReverseComponentDirection()).Strict() switch {
            var free when free.Count > 1 && free[0].RelativeVertexTo.Id != free[1].RelativeVertexFrom.Id => Closure(free.Rev()),
            var free => Closure(free),
        };

    private static (Seq<SubDEdge> Edges, bool Closed) Closure(Seq<SubDEdge> edges) =>
        (Edges: edges, Closed: edges.Count > 2 && edges[0].RelativeVertexFrom.Id == edges[^1].RelativeVertexTo.Id);

    private static Fin<(Seq<SubDEdge> Edges, bool Closed)> Boundary(Seq<(Seq<SubDEdge> Edges, bool Closed)> boundaries, uint edge) =>
        boundaries.Filter(boundary => boundary.Edges.Exists(link => link.Id == edge)).Head.ToFin(new RhinoFault(nameof(ComponentIndexType.SubdEdge), RhinoReason.Missing, (int)edge));

    private static Seq<SubDEdge> Anchored((Seq<SubDEdge> Edges, bool Closed) boundary, uint edge) =>
        (boundary.Closed ? boundary.Edges.TakeWhile(link => link.Id != edge).Count : 0) switch {
            var lead => boundary.Edges.Skip(lead) + boundary.Edges.Take(lead),
        };

    private static Option<SubDFace> Quad(SubD subd, SubDEdge a, SubDEdge b) =>
        from up in Rung(subd, a.RelativeVertexTo, b.RelativeVertexFrom)
        from down in Rung(subd, b.RelativeVertexTo, a.RelativeVertexFrom)
        from face in Optional(subd.Faces.Add([a, up, b, down]))
        select face;

    private static Option<SubDEdge> Rung(SubD subd, SubDVertex from, SubDVertex to) =>
        Optional(subd.Edges.FindOrAdd(from, to)).Map(rung => {
            rung.ComponentDirection = rung.VertexFrom.Id != from.Id;
            return rung;
        });

    // --- [CHAINS]
    public static IO<Seq<SubDChain>> Chains(SubD subd, ChainSeed seed) =>
        seed.Switch(
            subd,
            sorted: static (at, of) => IO.lift(() => Held(at, ComponentIndexType.SubdEdge, of.Edges))
                .Bind(_ => toSeq(SubDEdgeChain.SortEdgesIntoEdgeChains(at, of.Edges.Map(edge => at.Edges.Find(edge))))
                    .TraverseM(links => Products.Of(() => Linked(at, links)).Bind(static chain => Read(chain))).As()),
            grown: static (at, of) => IO.lift(() => Held(at, ComponentIndexType.SubdEdge, [of.Edge]))
                .Bind(_ =>
                    from chain in Products.Of(() => new SubDEdgeChain(at, at.Edges.Find(of.Edge)))
                    from grown in IO.lift(() => chain.AddAllNeighbors(of.Direction, of.Type))
                    from read in Read(chain)
                    select Seq(read)));

    private static SubDEdgeChain Linked(SubD subd, SubDEdge[] links) {
        SubDEdgeChain chain = new(subd, links[0]);
        _ = toSeq(links).Tail.Iter(link => _ = chain.AddEdge(link));
        return chain;
    }

    private static IO<SubDChain> Read(SubDEdgeChain chain) =>
        from curve in Products.Maybe(chain.ToNurbsCurve)
        from loft in Products.Maybe(chain.ToLoftCurve)
        select new SubDChain(toSeq(chain.Edges).Map(static edge => (Edge: edge.Id, Reversed: edge.ComponentDirection)).Strict(), chain.IsClosedLoop, curve, loft);
}
