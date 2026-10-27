using Grasshopper2.Bake;
using Grasshopper2.Doc;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BakeEntry(GeometryBase Geometry, ObjectAttributes Attributes);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BakeOps {
    // --- [SOURCES]
    private const BakeDataState OwnedStates = BakeDataState.Invalid | BakeDataState.Valid | BakeDataState.Expired;

    private static IO<Seq<string>> Baked(Seq<IDocumentObject> sources, BakeUpdateMode mode, Func<Guid, BakeContext> context) =>
        sources.Choose(static source => Optional(source as IBakeAware).Map(bakeable => (Process: source.InstanceId, Bakeable: bakeable))).Strict()
            .TraverseM(pair => IO.lift(() => toSeq(pair.Bakeable.BakeShapes(context(pair.Process), mode)))).As()
            .Map(static baked => baked.Flatten());

    private static IO<Seq<Guid>> Reconciled(
        Func<Seq<(BakeKey Key, Guid Id)>> read,
        Func<Option<Guid>, BakeEntry, IO<Guid>> write,
        Func<Seq<Guid>, IO<Unit>> prune,
        Func<BakeKey, bool> scope,
        Map<BakeKey, BakeEntry> desired) =>
        (from held in IO.lift(() => toSeq(read().Filter(row => scope(row.Key) || desired.ContainsKey(row.Key)).OrderBy(static row => row.Id)).Strict())
         from rows in desired.ToSeq().TraverseM(Prepared).As()
         from plan in IO.lift(() => rows.Map(row => {
             Option<Guid> previous = held.Find(found => row.Key.CompareTo(found.Key) is 0).Map(static found => found.Id);
             Guid id = previous.IfNone(Guid.NewGuid);
             ArchivableDictionary dictionary = row.Entry.Attributes.UserDictionary;
             row.Entry.Attributes.ObjectId = id;
             row.Key.AssignToDictionary(dictionary);
             _ = dictionary.Set(BakeKey.BakeObjectGuid, id);
             _ = dictionary.Remove(BakeKey.BakeObjectHash);
             return (Previous: previous, row.Entry);
         }).Strict())
         let kept = toHashSet(plan.Map(static row => row.Entry.Attributes.ObjectId))
         let stale = held.Filter(row => !kept.Contains(row.Id)).Map(static row => row.Id).Strict()
         from values in plan.TraverseM(row => write(row.Previous, row.Entry)).As()
         from pruned in prune(stale)
         select values).Bracket();

    private static IO<(BakeKey Key, BakeEntry Entry)> Prepared((BakeKey Key, BakeEntry Value) row) =>
        from copied in IO.lift(row.Value.Geometry.EnsurePrivateCopy)
        from attributes in use(() => row.Value.Attributes.Duplicate())
        select (row.Key, Entry: new BakeEntry(row.Value.Geometry, attributes));

    // --- [DOCUMENT]
    public static IO<Seq<string>> Bake(RhinoDoc doc, string process, Option<ObjectAttributes> attributes, MetaPattern meta, BakeUpdateMode mode, Seq<IDocumentObject> sources) =>
        Baked(sources, mode, id => new BakeContext(process, id, doc, attributes.ValueUnsafe(), meta: meta));

    public static IO<Seq<Guid>> Reconcile(RhinoDoc doc, Func<BakeKey, bool> scope, Map<BakeKey, BakeEntry> desired) =>
        Reconciled(
            () => toSeq(BakeContext.FindBakedObjects(doc, OwnedStates)).Map(static found => (Key: found.key, found.obj.Id)),
            (previous, entry) => IO.lift(() => previous.Match(
                Some: id => GrasshopperFault.Unless(doc.Objects.Replace(id, entry.Geometry, ignoreModes: true))
                    .Bind(_ => GrasshopperFault.Unless(doc.Objects.ModifyAttributes(id, entry.Attributes, quiet: true)))
                    .Map(_ => id),
                None: () => Hosts.Found(doc.Objects.FindId(doc.Objects.Add(entry.Geometry, entry.Attributes))).Map(static added => added.Id))),
            stale => IO.lift(() => GrasshopperFault.Unless(doc.Objects.Delete(stale, quiet: true) == stale.Count)),
            scope,
            desired);

    // --- [FILE]
    public static IO<Seq<string>> Bake(File3dm file, string process, MetaPattern meta, BakeUpdateMode mode, Seq<IDocumentObject> sources) =>
        (from attributes in use(static () => new ObjectAttributes())
         from baked in Baked(sources, mode, id => new BakeContext(process, id, file, attributes, meta: meta))
         select baked).Bracket();

    public static IO<Seq<Guid>> Reconcile(File3dm file, Func<BakeKey, bool> scope, Map<BakeKey, BakeEntry> desired) =>
        Reconciled(
            () => toSeq(BakeContext.FindBakedObjects(file, OwnedStates)).Map(static found => (Key: found.key, found.obj.Id)),
            (previous, entry) =>
                from removed in IO.lift(() => previous.Traverse(id => GrasshopperFault.Unless(file.Objects.Delete(id))).As())
                from added in IO.lift(() => Hosts.Found(file.Objects.FindId(file.Objects.Add(entry.Geometry, entry.Attributes))))
                select added.Id,
            stale => IO.lift(() => stale.Map((id, index) => GrasshopperFault.Unless(file.Objects.Delete(id), index)).Traverse(static deleted => deleted).As()).Map(unit),
            scope,
            desired);
}
