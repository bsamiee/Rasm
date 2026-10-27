using Grasshopper2.Bake;
using Grasshopper2.Doc;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Objects;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Grasshopper.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BakeOps {
    // --- [SOURCES]
    private const BakeDataState OwnedStates = BakeDataState.Invalid | BakeDataState.Valid | BakeDataState.Expired;

    private static IO<Seq<(Guid Process, IBakeAware Bakeable)>> Bakeables(Seq<IDocumentObject> sources) =>
        IO.lift(() => sources.Choose(static source => Optional(source as IBakeAware).Map(bakeable => (Process: source.InstanceId, Bakeable: bakeable))).Strict());

    private static IO<Seq<Guid>> Reconcile(
        Func<Seq<(BakeKey Key, Guid Id)>> read,
        Func<ObjectAttributes> defaults,
        Func<Option<Guid>, GeometryBase, ObjectAttributes, IO<Guid>> write,
        Func<Seq<Guid>, IO<Unit>> prune,
        Func<BakeKey, bool> scope,
        Map<BakeKey, GeometryPair> desired) =>
        (from held in IO.lift(() => toSeq(read().Filter(row => scope(row.Key) || desired.ContainsKey(row.Key)).OrderBy(static row => row.Id)).Strict())
         from rows in desired.ToSeq().TraverseM(row => Prepared(row, defaults)).As()
         from plan in IO.lift(() => rows.Map(row => {
             Option<Guid> previous = held.Find(found => row.Key.CompareTo(found.Key) is 0).Map(static found => found.Id);
             Guid id = previous.IfNone(Guid.NewGuid);
             ObjectAttributes attributes = row.Attributes;
             ArchivableDictionary dictionary = attributes.UserDictionary;
             attributes.ObjectId = id;
             row.Key.AssignToDictionary(dictionary);
             _ = dictionary.Set(BakeKey.BakeObjectGuid, id);
             _ = dictionary.Remove(BakeKey.BakeObjectHash);
             return (Previous: previous, row.Geometry, Attributes: attributes);
         }).Strict())
         let kept = toHashSet(plan.Map(static row => row.Attributes.ObjectId))
         let stale = held.Filter(row => !kept.Contains(row.Id)).Map(static row => row.Id).Strict()
         from values in plan.TraverseM(row => write(row.Previous, row.Geometry, row.Attributes)).As()
         from pruned in prune(stale)
         select values).Bracket();

    private static IO<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes)> Prepared((BakeKey Key, GeometryPair Value) row, Func<ObjectAttributes> defaults) =>
        from copied in IO.lift(row.Value.Geometry.EnsurePrivateCopy)
        from attributes in use(() => row.Value.Attributes.Match(Some: static attributes => attributes.Duplicate(), None: defaults))
        select (row.Key, row.Value.Geometry, Attributes: attributes);

    // --- [DOCUMENT]
    public static IO<Seq<string>> Bake(
        RhinoDoc doc,
        string process,
        Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits,
        MetaPattern meta,
        BakeUpdateMode mode,
        Seq<IDocumentObject> sources) =>
        TableOps.WithAttributes(doc.CreateDefaultAttributes, None, staged =>
            from edited in AttributeOps.Apply(doc, staged, edits)
            from bakeables in Bakeables(sources)
            from baked in bakeables.TraverseM(pair =>
                IO.lift(() => Conversions.Rows(pair.Bakeable.BakeShapes(new BakeContext(process, pair.Process, doc, staged, meta: meta), mode)))).As()
            select baked.Flatten());

    public static IO<Seq<Guid>> Reconcile(RhinoDoc doc, Func<BakeKey, bool> scope, Map<BakeKey, GeometryPair> desired) =>
        Reconcile(
            () => toSeq(BakeContext.FindBakedObjects(doc, OwnedStates)).Map(static found => (Key: found.key, found.obj.Id)),
            doc.CreateDefaultAttributes,
            (previous, geometry, attributes) => previous.Match(
                Some: id =>
                    from replaced in TableOps.Apply(doc, new TableOp.Replace(id, geometry, IgnoreModes: true))
                    from modified in IO.lift(() => GrasshopperFault.Unless(doc.Objects.ModifyAttributes(id, attributes, quiet: true)).Map(_ => id))
                    select modified,
                None: () => TableOps.Add(doc, new GeometryPair(geometry, Some(attributes)), None, reference: false)),
            stale => TableOps.Apply(doc, new TableOp.Delete(new ObjectTarget.Ids(stale), Quiet: true, IgnoreModes: true)).Map(unit),
            scope,
            desired);

    // --- [FILE]
    public static IO<Seq<string>> Bake(File3dm file, string process, MetaPattern meta, BakeUpdateMode mode, Seq<IDocumentObject> sources) =>
        (from attributes in use(static () => new ObjectAttributes())
         from bakeables in Bakeables(sources)
         from baked in bakeables.TraverseM(pair =>
             IO.lift(() => Conversions.Rows(pair.Bakeable.BakeShapes(new BakeContext(process, pair.Process, file, attributes, meta: meta), mode)))).As()
         select baked.Flatten()).Bracket();

    public static IO<Seq<Guid>> Reconcile(File3dm file, Func<BakeKey, bool> scope, Map<BakeKey, GeometryPair> desired) =>
        Reconcile(
            () => toSeq(BakeContext.FindBakedObjects(file, OwnedStates)).Map(static found => (Key: found.key, found.obj.Id)),
            static () => new ObjectAttributes(),
            (previous, geometry, attributes) =>
                from removed in IO.lift(() => previous.Match(
                    Some: id => GrasshopperFault.Unless(file.Objects.Delete(id)),
                    None: static () => unit))
                from added in IO.lift(() => Conversions.Found(Conversions.Present(file.Objects.Add(geometry, attributes))))
                select added,
            stale => IO.lift(() => Callbacks.Each(stale, (id, index) => GrasshopperFault.Unless(file.Objects.Delete(id), index)).Map(static _ => unit)),
            scope,
            desired);
}
