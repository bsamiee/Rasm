using System.Reflection;
using Grasshopper2.Bake;
using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Types.Assistant;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Grasshopper.Components;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Bakes {

    // --- [RECONCILIATION]

    public static IO<Seq<Guid>> Reconcile(
        RhinoDoc target, string process, Guid id, ITree desired, StashOverwriteMode expired, UserPattern user = default, MetaPattern meta = default, Option<ObjectAttributes> attributes = default) =>
        from written in (from layers in IO.lift(() => BakeContext.EnsureLayers(desired, target, attributes.ValueUnsafe(), user, meta))
                         from context in IO.lift(() => new BakeContext(process, id, target, attributes.ValueUnsafe(), user, meta))
                         from record in use(() => target.BeginUndoRecord($"Bake {context.ProcessName}"), serial => target.EndUndoRecord(serial))
                         from ids in IO.lift(Written(target, Planned(
                             context,
                             desired,
                             expired,
                             toSeq(BakeContext.FindBakedObjects(target, BakeDataState.Valid | BakeDataState.Expired)).Map(static found => (found.key, BakeKey.HarvestFromObject(found.obj, out _), found.obj.Id)),
                             target.Groups.Add)))
                         select ids).Bracket()
        from redrawn in IO.lift(target.RedrawAndReturnImmediately)
        select written;

    public static IO<Seq<Guid>> Reconcile(
        File3dm target, string process, Guid id, ITree desired, StashOverwriteMode expired, UserPattern user = default, MetaPattern meta = default, Option<ObjectAttributes> attributes = default) =>
        from layers in IO.lift(() => BakeContext.EnsureLayers(desired, target, attributes.ValueUnsafe(), user, meta))
        from context in IO.lift(() => new BakeContext(process, id, target, attributes.ValueUnsafe(), user, meta))
        from ids in IO.lift(Written(target, Planned(
            context,
            desired,
            expired,
            toSeq(BakeContext.FindBakedObjects(target, BakeDataState.Valid | BakeDataState.Expired)).Map(static found => (found.key, BakeKey.HarvestFromObject(found.obj, out _), found.obj.Id)),
            target.AllGroups.AddGroup)))
        select ids;

    // --- [PLANS]

    private static (Seq<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes, Option<Guid> Into)> Writes, Seq<Guid> Deleted, Seq<Guid> Severed) Planned(
        BakeContext context, ITree desired, StashOverwriteMode expired, Seq<(BakeKey Key, BakeDataState State, Guid Id)> found, Func<int> addGroup) {
        Seq<(BakeKey Key, BakeDataState State, Guid Id)> held = found.Filter(row => row.Key.ProcessGuid == context.ProcessGuid).Strict();
        Seq<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes)> parts = (
            from leaf in toSeq(desired.AllTwigs).Map(static (twig, index) => toSeq(twig.Pears).Map((pear, item) => (Twig: index, Item: item, Pear: pear))).Flatten()
            from pear in Optional(leaf.Pear).ToSeq()
            from part in Parts(context, leaf.Twig, leaf.Item, pear, addGroup)
            select part).Strict();
        HashMap<(int Twig, int Item, int Part), Seq<(BakeDataState State, Guid Id)>> occupied = held.Fold(
            HashMap<(int Twig, int Item, int Part), Seq<(BakeDataState State, Guid Id)>>(),
            static (slots, row) => slots.AddOrUpdate(Slot(row.Key), occupants => occupants.Add((row.State, row.Id)), () => Seq((row.State, row.Id))));
        HashSet<(int Twig, int Item, int Part)> wanted = toHashSet(parts.Map(static part => Slot(part.Key)));
        bool Touched((BakeDataState State, Guid Id) occupant) => occupant.State is not BakeDataState.Valid;
        Seq<(BakeDataState State, Guid Id)> Writable(Seq<(BakeDataState State, Guid Id)> occupants) => occupants.Filter(occupant => !Touched(occupant) || expired is StashOverwriteMode.Overwrite);
        bool Skipped((int Twig, int Item, int Part) slot) => expired is StashOverwriteMode.Skip && wanted.Contains(slot) && occupied.Find(slot).Exists(occupants => occupants.Exists(Touched));
        return (
            parts.Filter(part => !Skipped(Slot(part.Key))).Map(part => (part.Key, part.Geometry, part.Attributes, occupied.Find(Slot(part.Key)).Bind(occupants => Writable(occupants).Head).Map(static occupant => occupant.Id))).Strict(),
            toSeq(occupied.AsIterable()).Filter(entry => !Skipped(entry.Key)).Bind(entry => Writable(entry.Value).Skip(wanted.Contains(entry.Key) ? 1 : 0)).Map(static occupant => occupant.Id).Strict(),
            expired is StashOverwriteMode.Sever ? held.Filter(row => Touched((row.State, row.Id))).Map(static row => row.Id).Strict() : Seq<Guid>());
    }

    private static Seq<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes)> Parts(BakeContext context, int twig, int item, IPear pear, Func<int> addGroup) =>
        Optional(TypeAssistantServer.FindByType(pear.Type)).Filter(static assistant => assistant.ZupportsBake)
            .Bind(assistant => Optional(assistant.ToGeometry(pear)))
            .Map(static shapes => toSeq(shapes).Filter(static shape => shape is not null).Strict())
            .ToSeq()
            .Bind(shapes => Attributes(context, pear, shapes.Count, addGroup) switch {
                var attributes => shapes.Map((shape, part) => (
                    new BakeKey(context.ProcessName, context.ProcessGuid, twig, item, part),
                    ReferenceEquals(shape, pear.Item) ? shape.Duplicate() : shape,
                    attributes)),
            });

    private static ObjectAttributes Attributes(BakeContext context, IPear pear, int count, Func<int> addGroup) {
        ObjectAttributes attributes = context.SpecificAttributes(pear);
        Seq<(string Name, string Value)> texts = (
            from meta in Optional(pear.TryMeta()).ToSeq()
            from name in toSeq(meta.Names())
            where !string.Equals(name[0], StandardNames.Rhino.Id[0], StringComparison.OrdinalIgnoreCase)
            select (Name: name.ToString(), Value: meta[name].ToString())).Strict();
        if (count > 1)
            attributes.AddToGroup(addGroup());
        _ = texts.Iter(text => attributes.SetUserString(text.Name, text.Value));
        return attributes;
    }

    private static (int Twig, int Item, int Part) Slot(BakeKey key) => (key.TwigIndex, key.ItemIndex, key.PartIndex);

    // --- [WRITES]

    private static Fin<Seq<Guid>> Written(RhinoDoc document, (Seq<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes, Option<Guid> Into)> Writes, Seq<Guid> Deleted, Seq<Guid> Severed) plan) {
        _ = plan.Severed.Iter(id => Optional(document.Objects.FindId(id)).Iter(static found => Severed(found.Attributes.UserDictionary)));
        Seq<bool> deleted = plan.Deleted.Map(id => Optional(document.Objects.FindId(id)).Exists(found => document.Objects.Delete(found, quiet: true, ignoreModes: true))).Strict();
        return GrasshopperFault.Unless(deleted.ForAll(static removed => removed), GrasshopperReason.Declined, plan.Deleted)
            .Bind(_ => plan.Writes.TraverseM(write => write.Into.Match(
                Some: objectId =>
                    from replaced in GrasshopperFault.Unless(document.Objects.Replace(objectId, write.Geometry, ignoreModes: true), GrasshopperReason.Declined, write.Key)
                    from modified in GrasshopperFault.Unless(document.Objects.ModifyAttributes(objectId, write.Attributes, quiet: true), GrasshopperReason.Declined, write.Key)
                    select objectId,
                None: () => document.Objects.Add(write.Geometry, write.Attributes) switch {
                    var added => GrasshopperFault.Unless(added != Guid.Empty, GrasshopperReason.Declined, write.Key).Map(_ => added),
                }).Map(id => {
                    write.Key.AssignToObject(document.Objects.FindId(id));
                    return id;
                })).As());
    }

    private static Fin<Seq<Guid>> Written(File3dm file, (Seq<(BakeKey Key, GeometryBase Geometry, ObjectAttributes Attributes, Option<Guid> Into)> Writes, Seq<Guid> Deleted, Seq<Guid> Severed) plan) {
        _ = plan.Severed.Iter(id => Optional(file.Objects.FindId(id)).Iter(static found => Severed(found.Attributes.UserDictionary)));
        return GrasshopperFault.Unless(file.Objects.Delete(plan.Deleted) == plan.Deleted.Count, GrasshopperReason.Declined, plan.Deleted)
            .Bind(_ => plan.Writes.TraverseM(write =>
                (from deleted in write.Into.Traverse(objectId => GrasshopperFault.Unless(file.Objects.Delete(objectId), GrasshopperReason.Declined, write.Key)).As()
                 let id = file.Objects.Add(write.Geometry, write.Attributes)
                 from added in GrasshopperFault.Unless(id != Guid.Empty, GrasshopperReason.Declined, write.Key)
                 select id).Map(id => {
                     write.Key.AssignToObject(file.Objects.FindId(id));
                     return id;
                 })).As());
    }

    // --- [KEYS]

    private static readonly Memo<Seq<string>> Keys = memo(static () => toSeq(typeof(BakeKey).GetFields(BindingFlags.Public | BindingFlags.Static))
        .Filter(static field => field.IsLiteral)
        .Choose(static field => field.GetRawConstantValue() is string key ? Some(key) : None));

    private static Unit Severed(ArchivableDictionary dictionary) => Keys.Value.Iter(key => dictionary.Remove(key));
}
