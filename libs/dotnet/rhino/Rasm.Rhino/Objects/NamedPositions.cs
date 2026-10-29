using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedPosition(Guid Id, string Name, Seq<(Guid ObjectId, Transform Xform)> Objects, Seq<Guid> Skipped) {
    public static IO<Seq<NamedPosition>> Read(RhinoDoc doc) =>
        IO.lift(() => Products.Each(Conversions.Rows(doc.NamedPositions.Ids), (id, _) => Conversions.Rows(doc.NamedPositions.Objects(id)) switch {
            var members => Products.Each(members, (found, index) => {
                Transform xform = Transform.Unset;
                return RhinoFault.Unless(doc.NamedPositions.ObjectXform(id, found, ref xform), RhinoReason.Missing, found.Id, index).Map(_ => (ObjectId: found.Id, Xform: xform));
            }).Map(objects => new NamedPosition(
                id, doc.NamedPositions.Name(id), objects, members.Filter(static found => found.HasHistoryRecord()).Map(static found => found.Id).Strict())),
        }));
}

public sealed record NamedPositionState(StateName Name, ObjectTarget Objects) : NamedState(NamedPositions.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        from objects in Objects.Objects(doc)
        from saved in IO.lift(() => NamedPositions.Saved(doc.NamedPositions, Name, toHashSet(objects.Map(static found => found.Id))))
        select NamedWrite.Undoable;
}

public sealed record NamedPositionRestore(StateName Name) : NamedRestore(NamedPositions.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        NamedPositions.Found(doc, Name).Bind(id => Products.Accepted(() => doc.NamedPositions.Restore(id))).Map(static _ => NamedWrite.Undoable);
}

// --- [TABLES] --------------------------------------------------------------------------
public static class NamedPositions {
    // --- [TABLE]
    public static NamedTable Table { get; } = new(nameof(NamedPositions), StringComparer.Ordinal) {
        Entries = Entries,
        Rename = static (doc, entry, name) => Found(doc, entry.Name).Bind(id => Products.Accepted(() => doc.NamedPositions.Rename(id, name))).Map(static _ => NamedWrite.Undoable),
        Delete = static (doc, entry) => Found(doc, entry.Name).Bind(id => Products.Accepted(() => doc.NamedPositions.Delete(id))).Map(static _ => NamedWrite.Undoable),
        Hold = static (doc, _) => Held(doc),
        Members = Some<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>>(static (doc, entry) => Found(doc, entry.Name).Map(id => Conversions.Rows(doc.NamedPositions.ObjectIds(id)))),
    };

    // --- [ROWS]
    private static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedPositions.Ids).Map(id => new NamedEntry(Table, doc.NamedPositions.Name(id), Some(id))).Strict());

    // --- [HOLD]
    private static IO<NamedWrite> Held(RhinoDoc doc) =>
        from members in IO.lift(() => toHashSet(Conversions.Rows(doc.NamedPositions.Ids).Bind(id => Conversions.Rows(doc.NamedPositions.ObjectIds(id)))))
        from held in Some(members).Filter(static set => !set.IsEmpty)
            .Traverse(set => Products.Accepted(() => doc.NamedPositions.Save(Guid.NewGuid().ToString(), set) switch { var saved => (saved != Guid.Empty, saved) })).As()
        select new NamedWrite(
            held.Traverse(id => Products.Accepted(() => doc.NamedPositions.Restore(id))).As().Map(static _ => unit),
            held.Traverse(id => Products.Accepted(() => doc.NamedPositions.Delete(id))).As().Map(static _ => unit));

    // --- [HOST]
    internal static Fin<Unit> Saved(NamedPositionTable table, string name, HashSet<Guid> wanted) =>
        Conversions.Present(table.Id(name))
            .Map(id => (Id: id, Held: toHashSet(table.ObjectIds(id))))
            .Filter(row => row.Held.IsSubsetOf(wanted))
            .Match(
                Some: row => (wanted.Count == row.Held.Count ? unit : RhinoFault.Unless(table.Append(row.Id, wanted.Except(row.Held)), RhinoReason.Declined, name))
                    .Bind(_ => RhinoFault.Unless(table.Update(row.Id), RhinoReason.Declined, name)),
                None: () => RhinoFault.Unless(table.Save(name, wanted) != Guid.Empty, RhinoReason.Declined, name));

    internal static IO<Guid> Found(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedPositions.Id(name)).ToFin(Table.Missing(name)));
}
