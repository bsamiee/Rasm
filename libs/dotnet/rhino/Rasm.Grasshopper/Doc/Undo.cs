using Grasshopper2.Doc;
using Grasshopper2.Undo;
using QuikGraph;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HistoryOps {
    // --- [NAVIGATION]
    public static IO<Unit> Restore(History history, Node node) =>
        IO.lift(() => (node.Record.State, Optional(history.FirstUndo), Optional(history.FirstRedo), Optional(node.PrimaryChild)))
            .Bind(read => read switch {
                (Grasshopper2.Undo.State.Redo, var firstUndo, var firstRedo, _) =>
                    when(firstUndo.IsNone && firstRedo.Exists(first => first != node && !first.IsDescendantNode(node)), IO.lift(() => history.Redo()))
                        .As()
                        .Bind(_ => IO.lift(() => history.Redo(node))),
                (Grasshopper2.Undo.State.Undo, var firstUndo, _, var child) =>
                    IO.lift(() => child.Filter(static next => next.Record.State is Grasshopper2.Undo.State.Undo).Iter(next => {
                        if (firstUndo.Exists(first => first == next))
                            history.Undo();
                        else
                            history.Undo(next);
                    })),
            })
            .Uninterruptible();

    public static IO<Unit> Restore(Document document, Record record, Grasshopper2.Undo.State state) =>
        IO.lift(() => record.State)
            .Bind(current => when(current != state,
                IO.lift(state switch {
                    Grasshopper2.Undo.State.Undo => fun(() => record.Redo(document)),
                    Grasshopper2.Undo.State.Redo => fun(() => record.Undo(document)),
                }).Bind(_ => Edits.Respond(document, record))).As())
            .Uninterruptible();

    // --- [READS]
    public static IO<ArrayBidirectionalGraph<Node, SEquatableEdge<Node>>> Topology(History history) =>
        IO.lift(() => history.Root)
            .Map(static root => (Seq(root) + toSeq(root.AllDescendants()))
                .ToBidirectionalGraph(static node => node.Children.Select(child => new SEquatableEdge<Node>(node, child)), allowParallelEdges: false)
                .ToArrayBidirectionalGraph());

    public static IO<Option<(Node Ancestor, Seq<Node> Path)>> Reconcile(Node start, Node end) =>
        from lineages in IO.lift(() => (Rising: Lineage(start), Falling: Lineage(end)))
        let shared = lineages.Rising.Rev().Zip(lineages.Falling.Rev()).TakeWhile(static pair => pair.First == pair.Second).Count
        select from ancestor in lineages.Rising.At(lineages.Rising.Count - shared)
               select (Ancestor: ancestor, Path: lineages.Rising.Take(lineages.Rising.Count - shared + 1) + lineages.Falling.Take(lineages.Falling.Count - shared).Rev());

    public static IO<Seq<(Node Node, Grasshopper2.Undo.Action Action)>> Applied(History history) =>
        IO.lift(() => toSeq(history.CentralUndoSequence)
            .Bind(static node => LanguageExt.Seq.repeat(node, node.Record.Count))
            .Zip(toSeq(history.UndoActions), static (node, action) => (Node: node, Action: action))
            .Strict());

    private static Seq<Node> Lineage(Node node) => toSeq(LanguageExt.List.unfold(node, static step => Optional(step.Parent))).Strict();

    // --- [RECORDS]
    public static IO<A> Recorded<A>(IDocumentObject target, VerbNoun name, AutoSaveReason reason, Func<Grasshopper2.Undo.Action> capture, IO<A> edit, Func<A, bool> changed) =>
        DisposalOps.Transacted(
            IO.lift(capture),
            _ => edit,
            static _ => unitIO,
            (action, value) => when(changed(value),
                IO.lift(() => target.AddUndoRecord(name, action)).Bind(_ =>
                    Callbacks.Each(Seq(
                        IO.lift(() => target.RequestAutoSave(reason)).Map(unit),
                        when(action.Response.HasFlag(Effects.ExpireSolution), IO.lift(() => Optional(target.Document).Iter(document => document.Solution.DelayedExpire(target)))).As(),
                        when(action.Response.HasFlag(Effects.ExpireLayout), IO.lift(target.OnLayoutChanged)).As(),
                        when(action.Response.HasFlag(Effects.ExpireLayout), IO.lift(() => Optional(target.Attributes).Iter(static attributes => attributes.InvalidateLayout()))).As(),
                        when(action.Response.HasFlag(Effects.ExpireDisplay), IO.lift(target.OnDisplayChanged)).As())))
                    .Map(static _ => unit)).As().Map(_ => value));
}
