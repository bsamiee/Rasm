using Grasshopper2.Doc;
using Grasshopper2.Undo;
using QuikGraph;

namespace Rasm.Grasshopper.Doc;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Ancestry(Node Ancestor, Seq<Node> Path);

public sealed record Applied(Node Node, Grasshopper2.Undo.Action Action);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HistoryOps {
    // --- [READS]
    public static IO<ArrayBidirectionalGraph<Node, SEquatableEdge<Node>>> Topology(History history) =>
        IO.lift(() => history.Root)
            .Map(static root => (Seq(root) + toSeq(root.AllDescendants()))
                .ToBidirectionalGraph(static node => node.Children.Select(child => new SEquatableEdge<Node>(node, child)), allowParallelEdges: false)
                .ToArrayBidirectionalGraph());

    public static IO<Option<Ancestry>> Reconcile(Node start, Node end) =>
        from lineages in IO.lift(() => (Rising: Lineage(start), Falling: Lineage(end)))
        let shared = lineages.Rising.Rev().Zip(lineages.Falling.Rev()).TakeWhile(static pair => pair.First == pair.Second).Count
        select from ancestor in lineages.Rising.At(lineages.Rising.Count - shared)
               select new Ancestry(ancestor, lineages.Rising.Take(lineages.Rising.Count - shared + 1) + lineages.Falling.Take(lineages.Falling.Count - shared).Rev());

    public static IO<Seq<Applied>> Actions(History history) =>
        IO.lift(() => toSeq(history.CentralUndoSequence)
            .Bind(static node => LanguageExt.Seq.repeat(node, node.Record.Count))
            .Zip(toSeq(history.UndoActions), static (node, action) => new Applied(node, action))
            .Strict());

    private static Seq<Node> Lineage(Node node) => toSeq(LanguageExt.List.unfold(node, static step => Optional(step.Parent))).Strict();

    // --- [RECORDS]
    public static IO<A> Recorded<A>(IDocumentObject target, VerbNoun name, AutoSaveReason reason, Func<Grasshopper2.Undo.Action> capture, IO<A> edit, Func<A, bool> changed) =>
        from action in IO.lift(capture)
        from value in edit
        from recorded in when(changed(value),
            IO.lift(() => target.AddUndoRecord(name, action))
            >> IO.lift(() => ignore(target.RequestAutoSave(reason)))
            >> IO.lift(() => {
                if (action.Response.HasFlag(Effects.ExpireSolution)) _ = Optional(target.Document).Iter(document => document.Solution.DelayedExpire(target));
                if (action.Response.HasFlag(Effects.ExpireLayout)) {
                    target.OnLayoutChanged();
                    _ = Optional(target.Attributes).Iter(static attributes => attributes.InvalidateLayout());
                }
                if (action.Response.HasFlag(Effects.ExpireDisplay)) target.OnDisplayChanged();
            })).As()
        select value;
}
