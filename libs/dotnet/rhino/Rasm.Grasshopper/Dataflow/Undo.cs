using Grasshopper2.Doc;
using Grasshopper2.Undo;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Dataflow;

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
                    IO.lift(() => child.Filter(static next => next.Record.State == Grasshopper2.Undo.State.Undo).Iter(next => {
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

    public static IO<Option<(Node Ancestor, Seq<Node> Path)>> Reconcile(History history, Node from, Node to) =>
        from == to
            ? IO.pure(Some((Ancestor: from, Path: Seq(from))))
            : Topology(history)
                .Map(graph => Callbacks.Found(new UndirectedBidirectionalGraph<Node, SEquatableEdge<Node>>(graph)
                        .ShortestPathsDijkstra(static _ => 1, from)(to, out IEnumerable<SEquatableEdge<Node>> edges), edges)
                    .Map(path => toSeq(path).Scan(from, static (node, edge) => edge.GetOtherVertex(node)).Strict()))
                .Map(path => from nodes in path
                             from ancestor in nodes.Find(node => node == to || node.IsDescendantNode(to))
                             select (Ancestor: ancestor, Path: nodes));

    public static IO<Seq<(Node Node, Grasshopper2.Undo.Action Action)>> Applied(History history) =>
        IO.lift(() => toSeq(history.CentralUndoSequence)
            .Bind(static node => LanguageExt.Seq.repeat(node, node.Record.Count))
            .Zip(toSeq(history.UndoActions), static (node, action) => (Node: node, Action: action))
            .Strict());

    // --- [RECORDS]
    public static IO<A> Recorded<A>(IDocumentObject target, VerbNoun name, AutoSaveReason reason, Func<Grasshopper2.Undo.Action> capture, IO<A> edit, Func<A, bool> changed) =>
        IO.lift(static environment => environment).Bind(environment =>
            (from edited in IO.liftVAsync(() => (
                 from action in IO.lift(capture)
                 from value in edit
                 select (Action: action, Value: value)).RunAsync(environment))
             from committed in when(changed(edited.Value),
                 IO.lift(() => target.AddUndoRecord(name, edited.Action)).Bind(_ =>
                     Callbacks.Each(Seq(
                         IO.lift(() => target.RequestAutoSave(reason)).Map(static _ => unit),
                         when(edited.Action.Response.HasFlag(Effects.ExpireSolution), IO.lift(() => Optional(target.Document).Iter(document => document.Solution.DelayedExpire(target)))).As(),
                         when(edited.Action.Response.HasFlag(Effects.ExpireLayout), IO.lift(target.OnLayoutChanged)).As(),
                         when(edited.Action.Response.HasFlag(Effects.ExpireLayout), IO.lift(() => Optional(target.Attributes).Iter(static attributes => attributes.InvalidateLayout()))).As(),
                         when(edited.Action.Response.HasFlag(Effects.ExpireDisplay), IO.lift(target.OnDisplayChanged)).As())))
                     .Map(static _ => unit)).As()
             select edited.Value).Uninterruptible());
}
