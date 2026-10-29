using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EdgeReplacement {
    private EdgeReplacement(BrepReplaceEdgeMethod method, Option<Curve> guide) => (Method, Guide) = (method, guide);

    public BrepReplaceEdgeMethod Method { get; }

    public Option<Curve> Guide { get; }

    public sealed record Line() : EdgeReplacement(BrepReplaceEdgeMethod.Line, None);

    public sealed record ExtendSideEdges() : EdgeReplacement(BrepReplaceEdgeMethod.ExtendSideEdges, None);

    public sealed record Along(Curve Path) : EdgeReplacement(BrepReplaceEdgeMethod.Curve, Some(Path));

    public Step<Brep> ReplaceEdge(Tolerances tolerances, Seq<int> edges) =>
        Steps.Derived<Brep>(brep =>
            from held in IO.lift(() => brep.Within(ComponentIndexType.BrepEdge, edges))
            from replaced in Products.Of(() => brep.ReplaceEdge(held, Method, Guide.ValueUnsafe(), tolerances.Absolute))
            select replaced);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidEdits {
    // --- [FACES]
    public static Step<Brep> ReplaceSurface(Tolerances tolerances, int face, Surface surface, bool rebuildSharedEdges, bool rebuildVertices) =>
        Steps.Derived<Brep>(brep =>
            from held in IO.lift(() => brep.Within(ComponentIndexType.BrepFace, Seq(face)))
            from index in IO.lift(() => Conversions.Required(Conversions.Present(brep.AddSurface(surface))))
            from changed in Products.Accepted(() => brep.Faces[face].ChangeSurface(index))
            from rebuilt in Products.Accepted(() => brep.Faces[face].RebuildEdges(tolerances.Absolute, rebuildSharedEdges, rebuildVertices))
            select brep);

    // --- [LOOPS]
    public static Step<Brep> RemoveHoles(Tolerances tolerances, Seq<int> loops) =>
        Steps.Derived<Brep>(brep =>
            from inner in IO.lift(() => brep.Within(loops.Map(static loop => new ComponentIndex(ComponentIndexType.BrepLoop, loop))).Bind(held =>
                Products.Each(held, (loop, element) => brep.Loops[loop.Index].LoopType switch {
                    BrepLoopType.Inner => Fin.Succ(loop),
                    var type => new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{type} {ComponentIndexType.BrepLoop} {loop.Index}"), RhinoReason.Mismatched, element),
                })))
            from removed in Products.Of(() => brep.RemoveHoles(inner, tolerances.Absolute))
            select removed);

    // --- [EDGES]
    public static Step<Brep> JoinEdges(Tolerances tolerances, Seq<(int Edge0, int Edge1)> pairs) =>
        Steps.Derived<Brep>(brep =>
            from held in IO.lift(() => brep.Within(ComponentIndexType.BrepEdge, pairs.Bind(static pair => Seq(pair.Edge0, pair.Edge1))))
            from joined in pairs.TraverseM(pair => Products.Accepted(() => brep.JoinEdges(pair.Edge0, pair.Edge1, tolerances.Absolute, compact: false))).As()
            from compacted in IO.lift(brep.Compact)
            select brep);
}
