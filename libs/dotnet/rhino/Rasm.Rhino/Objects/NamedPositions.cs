using Rasm.RhinoCommon.Document.Tables;
using Rhino;
using Rhino.DocObjects.Tables;

namespace Rasm.RhinoCommon.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedPosition(Guid Id, string Name, Seq<(Guid ObjectId, Transform Xform)> Objects, Seq<Guid> Skipped) {
    public static IO<Seq<NamedPosition>> Read(RhinoDoc doc) =>
        IO.lift(() => Callbacks.Each(Conversions.Rows(doc.NamedPositions.Ids), (id, _) => Conversions.Rows(doc.NamedPositions.Objects(id)) switch {
            var members => Callbacks.Each(members, (found, index) => {
                Transform xform = Transform.Unset;
                return RhinoFault.Unless(doc.NamedPositions.ObjectXform(id, found, ref xform), index).Map(_ => (ObjectId: found.Id, Xform: xform));
            }).Map(objects => new NamedPosition(
                id, doc.NamedPositions.Name(id), objects, members.Filter(static found => found.HasHistoryRecord()).Map(static found => found.Id).Strict())),
        }));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NamedPositions {
    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedPositions.Ids).Map(id => new NamedEntry(NamedTable.Positions, doc.NamedPositions.Name(id), Some(id))).Strict());

    internal static IO<Seq<Guid>> Members(RhinoDoc doc, NamedEntry entry) =>
        Found(doc, entry.Name).Map(id => Conversions.Rows(doc.NamedPositions.ObjectIds(id)));

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        Found(doc, entry.Name).Bind(id => Landed(() => RhinoFault.Unless(doc.NamedPositions.Rename(id, name))));

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        Found(doc, entry.Name).Bind(id => Landed(() => RhinoFault.Unless(doc.NamedPositions.Delete(id))));

    // --- [STATES]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.Position state) =>
        from objects in state.Objects.Objects(doc)
        from saved in IO.lift(() => Saved(doc.NamedPositions, state.Name, toHashSet(objects.Map(static found => found.Id))))
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.Position target) =>
        Found(doc, target.Name).Bind(id => Landed(() => RhinoFault.Unless(doc.NamedPositions.Restore(id))));

    internal static IO<NamedWrite> Hold(RhinoDoc doc) =>
        from members in IO.lift(() => toHashSet(Conversions.Rows(doc.NamedPositions.Ids).Bind(id => Conversions.Rows(doc.NamedPositions.ObjectIds(id)))))
        from held in IO.lift(() => Some(members).Filter(static set => !set.IsEmpty)
            .Traverse(set => Conversions.Found(Conversions.Present(doc.NamedPositions.Save(Guid.NewGuid().ToString(), set)))).As())
        select (NamedWrite)new NamedWrite.Compensated(
            held.Traverse(id => IO.lift(() => RhinoFault.Unless(doc.NamedPositions.Restore(id)))).As().Map(static _ => unit),
            held.Traverse(id => IO.lift(() => RhinoFault.Unless(doc.NamedPositions.Delete(id)))).As().Map(static _ => unit));

    private static Fin<Unit> Saved(NamedPositionTable table, string name, HashSet<Guid> wanted) =>
        Conversions.Present(table.Id(name))
            .Map(id => (Id: id, Held: toHashSet(table.ObjectIds(id))))
            .Filter(row => row.Held.IsSubsetOf(wanted))
            .Match(
                Some: row => (wanted.Count == row.Held.Count ? unit : RhinoFault.Unless(table.Append(row.Id, wanted.Except(row.Held))))
                    .Bind(_ => RhinoFault.Unless(table.Update(row.Id))),
                None: () => Conversions.Found(Conversions.Present(table.Save(name, wanted))).Map(static _ => unit));

    // --- [HOST]
    private static IO<Guid> Found(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedPositions.Id(name)).ToFin(new RhinoFault(name, RhinoReason.Missing)));

    private static IO<NamedWrite> Landed(Func<Fin<Unit>> call) =>
        IO.lift(call).Map(static _ => (NamedWrite)new NamedWrite.Recorded());
}
