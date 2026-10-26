using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.Undo;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Edits {
    // --- [RECORDS]
    internal static IO<Unit> Respond(Document document, Record record) =>
        IO.pure(record.Response).Bind(response => Callbacks.Each(Seq(
            when(response.HasFlag(Effects.ExpireLayout) || response.HasFlag(Effects.InvalidateLayout), IO.lift(static () => Optional(Editor.Instance).Iter(static editor => editor.Canvas.Invalidate()))).As(),
            when(response.HasFlag(Effects.ExpireSolution) || response.HasFlag(Effects.InvalidateSolution), IO.lift(() => ignore(document.Solution.Start()))).As(),
            when(response.HasFlag(Effects.ExpireDisplay) || response.HasFlag(Effects.InvalidateDisplay), IO.lift(document.Display.UpdateDisplay)).As()))).Map(unit);

    public static IO<A> Commit<A>(Document document, VerbNoun name, AutoSaveReason reason, Func<ActionList, IO<A>> body) =>
        DisposalOps.Transacted(
            IO.lift(static () => ActionList.Empty),
            body,
            actions => IO.lift(() => Optional(actions.ToRecord(name))).Bind(record => record.Match(
                Some: held => IO.lift(() => held.Undo(document)).Bind(_ => Respond(document, held)),
                None: static () => IO.pure(unit))),
            (actions, value) => Recorded(document, actions, name, reason).Map(_ => value));

    private static IO<Unit> Recorded(Document document, ActionList actions, VerbNoun name, AutoSaveReason reason) =>
        IO.lift(() => Optional(actions.ToRecord(name))).Bind(record => record.Match(
            Some: held => IO.lift(() => document.Undo.Do(held))
                .Bind(_ => Callbacks.Each(Seq(IO.lift(() => ignore(document.File.AutoSave(reason))), Respond(document, held))))
                .Map(unit),
            None: static () => IO.pure(unit)));

    // --- [CLUSTERS]
    public static IO<IDocumentObject> ClusterObjects(Document document, Seq<IDocumentObject> objects, ActionList actions) =>
        IO.lift(Fin<IDocumentObject> () => document.Methods.CanCreateCluster(objects, out string whyNot)
            ? Fin.Succ(document.Methods.ClusterObjects([.. objects], actions))
            : Error.New(whyNot));

    // --- [SELECTION]
    private static LanguageExt.HashSet<Guid> Adjacent(Connectivity connectivity, Seq<IDocumentObject> selected, bool upstream, bool downstream) =>
        toHashSet(selected
            .Map(static item => Connectivity.GetConnectiveId(item))
            .Bind(id => (upstream ? toSeq(connectivity.FindImmediateInputs(id)) : Seq<ConnectiveObject>()) + (downstream ? toSeq(connectivity.FindImmediateOutputs(id)) : Seq<ConnectiveObject>()))
            .Map(static node => node.Id));

    private static IO<int> Reselect(Document document, LanguageExt.HashSet<Guid> selection) =>
        from changed in IO.lift(() => toSeq(document.Objects.PrimaryAndSecondary)
                .Map(item => (Object: item, Selected: selection.Contains(item.InstanceId)))
                .Filter(static row => row.Object.Selected != row.Selected).Strict())
        from assigned in IO.lift(() => changed.Iter(static row => row.Object.Selection = row.Selected ? ObjectSelection.Selected : ObjectSelection.Unselected))
        select changed.Count;

    public static IO<int> GrowSelection(Document document, bool upstream, bool downstream) =>
        IO.lift(() => (Selected: toSeq(document.Objects.SelectedObjects).Strict(), document.Objects.Connectivity))
            .Bind(read => Reselect(document, toHashSet(read.Selected.Map(static item => item.InstanceId)).Union(Adjacent(read.Connectivity, read.Selected, upstream, downstream))));

    public static IO<int> ShiftSelection(Document document, bool upstream) =>
        IO.lift(() => (Selected: toSeq(document.Objects.SelectedObjects).Strict(), document.Objects.Connectivity))
            .Bind(read => Reselect(document, Adjacent(read.Connectivity, read.Selected, upstream, !upstream)));
}
