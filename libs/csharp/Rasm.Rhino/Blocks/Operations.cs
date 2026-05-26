using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.Runtime;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] [MUTATION] -------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockMutationOp {
    private BlockMutationOp() { }

    // [AUTHOR]
    public sealed record Author(AuthorSpec Spec, BlockMembers Members, ConflictPolicy Conflict) : BlockMutationOp;
    public sealed record AuthorFromObjects(AuthorSpec Spec, Seq<Guid> Sources, ConflictPolicy Conflict) : BlockMutationOp;
    public sealed record ModifyMetadata(DefinitionRef Ref, MetadataPatch Patch) : BlockMutationOp;
    public sealed record ModifyGeometry(DefinitionRef Ref, BlockMembers Members) : BlockMutationOp;

    // [MANAGE]
    public sealed record Rename(DefinitionRef Ref, DefinitionName NewName) : BlockMutationOp;
    public sealed record Delete(DefinitionRef Ref, DeletionPolicy Policy) : BlockMutationOp;
    public sealed record Undelete(DefinitionRef Ref) : BlockMutationOp;
    public sealed record Purge(Option<DefinitionRef> Ref) : BlockMutationOp;
    public sealed record Compact(bool IgnoreUndoReferences = false) : BlockMutationOp;

    // [PLACE]
    public sealed record Place(DefinitionRef Ref, Seq<Transform> At, Option<ObjectAttributes> Attrs) : BlockMutationOp;
    public sealed record ReplaceDefinition(Guid InstanceId, DefinitionRef NewRef) : BlockMutationOp;
    public sealed record TransformCopy(Guid InstanceId, Transform Xform, bool DeleteOriginal) : BlockMutationOp;
    public sealed record TransformWithHistory(Guid InstanceId, Transform Xform) : BlockMutationOp;
    public sealed record ExplodeIntoDocument(Guid InstanceId, ExplodePolicy Policy) : BlockMutationOp;

    // [LINK]
    public sealed record CreateArchiveLinks(Seq<FileEndpoint> Sources) : BlockMutationOp;
    public sealed record UpdateSourceArchive(DefinitionRef Ref, FileEndpoint Source, UpdatePolicy Policy, bool Quiet = true) : BlockMutationOp;
    public sealed record RefreshLinks(Option<Seq<string>> Archives, bool SkipUpToDate = false) : BlockMutationOp;
    public sealed record ReloadFromFile(DefinitionRef Ref, FileEndpoint Source, bool UpdateNestedLinks = true, bool Quiet = true) : BlockMutationOp;
    public sealed record DetachArchive(DefinitionRef Ref, bool Quiet = true) : BlockMutationOp;
    public sealed record FlattenLinked(Option<Seq<string>> Archives, Option<Seq<DefinitionRef>> Refs) : BlockMutationOp;

    // [OBSERVE] [SIDE-EFFECT]
    public sealed record Export(DefinitionRef Ref, FileEndpoint Target) : BlockMutationOp;
}

// --- [TYPES] [QUERY] ----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockQueryOp {
    private BlockQueryOp() { }

    // [MANAGE]
    public sealed record Lookup(DefinitionRef Ref) : BlockQueryOp;
    public sealed record AllocateName(Option<string> Seed) : BlockQueryOp;
    public sealed record Snapshot(Option<DefinitionRef> Ref) : BlockQueryOp;
    public sealed record Audit() : BlockQueryOp;

    // [PLACE]
    public sealed record ExplodeInspect(Guid InstanceId, ExplodePolicy Policy) : BlockQueryOp;

    // [GRAPH]
    public sealed record GraphMembers(DefinitionRef Ref) : BlockQueryOp;
    public sealed record GraphInserts(DefinitionRef Ref, ReferenceScope Scope) : BlockQueryOp;
    public sealed record GraphContainers(DefinitionRef Ref) : BlockQueryOp;
    public sealed record SelectedPart(ObjRef Ref) : BlockQueryOp;
    public sealed record DependencyAudit() : BlockQueryOp;

    // [OBSERVE]
    public sealed record Preview(DefinitionRef Ref, PreviewSpec Spec) : BlockQueryOp;
    public sealed record TextFieldsOf(DefinitionRef Ref) : BlockQueryOp;
    public sealed record AttributeFieldsOf(DefinitionRef Ref) : BlockQueryOp;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class BlockOperations {
    internal static Fin<MutationReceipt> RunMutation(BlockMutationOp op, RhinoBlocks owner) =>
        op.Switch(
            owner: owner,
            author: static (o, x) => Author(owner: o, op: x),
            authorFromObjects: static (o, x) => AuthorFromObjects(owner: o, op: x),
            modifyMetadata: static (o, x) => ModifyMetadata(owner: o, op: x),
            modifyGeometry: static (o, x) => ModifyGeometry(owner: o, op: x),
            rename: static (o, x) => Rename(owner: o, op: x),
            delete: static (o, x) => Delete(owner: o, op: x),
            undelete: static (o, x) => Undelete(owner: o, op: x),
            purge: static (o, x) => Purge(owner: o, op: x),
            compact: static (o, x) => Compact(owner: o, op: x),
            place: static (o, x) => Place(owner: o, op: x),
            replaceDefinition: static (o, x) => ReplaceDefinition(owner: o, op: x),
            transformCopy: static (o, x) => TransformCopy(owner: o, op: x),
            transformWithHistory: static (o, x) => TransformWithHistory(owner: o, op: x),
            explodeIntoDocument: static (o, x) => ExplodeIntoDocument(owner: o, op: x),
            createArchiveLinks: static (o, x) => CreateArchiveLinks(owner: o, op: x),
            updateSourceArchive: static (o, x) => UpdateSourceArchive(owner: o, op: x),
            refreshLinks: static (o, x) => RefreshLinks(owner: o, op: x),
            reloadFromFile: static (o, x) => ReloadFromFile(owner: o, op: x),
            detachArchive: static (o, x) => DetachArchive(owner: o, op: x),
            flattenLinked: static (o, x) => FlattenLinked(owner: o, op: x),
            export: static (o, x) => Export(owner: o, op: x));

    internal static Fin<BlockResult> RunQuery(BlockQueryOp op, RhinoBlocks owner) =>
        op.Switch(
            owner: owner,
            lookup: static (o, x) => Lookup(owner: o, op: x),
            allocateName: static (o, x) => AllocateName(owner: o, op: x),
            snapshot: static (o, x) => Snapshot(owner: o, op: x),
            audit: static (o, x) => Audit(owner: o, op: x),
            explodeInspect: static (o, x) => ExplodeInspect(owner: o, op: x),
            graphMembers: static (o, x) => GraphMembers(owner: o, op: x),
            graphInserts: static (o, x) => GraphInserts(owner: o, op: x),
            graphContainers: static (o, x) => GraphContainers(owner: o, op: x),
            selectedPart: static (o, x) => SelectedPart(owner: o, op: x),
            dependencyAudit: static (o, x) => DependencyAudit(owner: o, op: x),
            preview: static (o, x) => Preview(owner: o, op: x),
            textFieldsOf: static (o, x) => TextFieldsOf(owner: o, op: x),
            attributeFieldsOf: static (o, x) => AttributeFieldsOf(owner: o, op: x));

    // ---- [OPERATIONS] [DISPATCH] ---------------------------------------------------------
    private static Fin<MutationReceipt> DispatchMutation(string name, Func<Op, Fin<MutationReceipt>> body) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key));
    }

    private static Fin<BlockResult> DispatchQuery(string name, Func<Op, Fin<BlockResult>> body) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key));
    }

    internal static Fin<BlockSnapshot> LookupSnapshot(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindLive(table: table, refer: refer, key: key).Bind(d => BlockSnapshot.From(definition: d, key: key));

    private static Fin<(BlockSnapshot Snap, InstanceDefinition Live)> LookupLive(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindLive(table: table, refer: refer, key: key).Bind(d => BlockSnapshot.From(definition: d, key: key).Map(s => (Snap: s, Live: d)));

    private static Fin<InstanceDefinition> FindLive(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        refer switch {
            DefinitionRef.ById r => Optional(table.Find(instanceId: r.Id.Value, ignoreDeletedInstanceDefinitions: true)).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByIndex r when r.Index.Value < table.Count => Optional(table[r.Index.Value]).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByIndex => Fin.Fail<InstanceDefinition>(error: key.InvalidInput()),
            DefinitionRef.ByName r => Optional(table.Find(instanceDefinitionName: r.Name.Value)).ToFin(Fail: key.InvalidInput()),
            _ => Fin.Fail<InstanceDefinition>(error: key.InvalidInput()),
        };

    // ---- [OPERATIONS] [AUTHOR] -----------------------------------------------------------
    private static Fin<MutationReceipt> Author(RhinoBlocks owner, BlockMutationOp.Author op) =>
        DispatchMutation(nameof(BlockMutationOp.Author), key => PerformAuthor(owner: owner, spec: op.Spec, members: op.Members, conflict: op.Conflict, key: key));

    private static Fin<MutationReceipt> AuthorFromObjects(RhinoBlocks owner, BlockMutationOp.AuthorFromObjects op) =>
        DispatchMutation(nameof(BlockMutationOp.AuthorFromObjects), key =>
            from members in ResolveObjectMembers(doc: owner.Document, sources: op.Sources, key: key)
            from receipt in PerformAuthor(owner: owner, spec: op.Spec, members: members, conflict: op.Conflict, key: key)
            select receipt);

    private static Fin<BlockMembers> ResolveObjectMembers(RhinoDoc doc, Seq<Guid> sources, Op key) =>
        sources.IsEmpty
            ? Fin.Fail<BlockMembers>(error: key.InvalidInput())
            : sources
                .Map(id => Optional(doc.Objects.FindId(id: id)).ToFin(Fail: key.InvalidInput()))
                .TraverseM(identity)
                .As()
                .Bind(objs => BlockMembers.Of(
                    geometry: objs.Map(static o => o.Geometry.Duplicate()),
                    attributes: objs.Map(static o => o.Attributes.Duplicate()),
                    key: key));

    private static Fin<MutationReceipt> PerformAuthor(RhinoBlocks owner, AuthorSpec spec, BlockMembers members, ConflictPolicy conflict, Op key) =>
        LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(spec.Name), key: key).ToOption() switch {
            { IsSome: true, Case: BlockSnapshot existing } => Resolve(owner: owner, existing: existing, spec: spec, members: members, conflict: conflict, key: key),
            _ => AddNew(owner: owner, spec: spec, members: members, key: key),
        };

    private static Fin<MutationReceipt> AddNew(RhinoBlocks owner, AuthorSpec spec, BlockMembers members, Op key) =>
        owner.Document.InstanceDefinitions.Add(
            name: spec.Name.Value,
            description: spec.Metadata.Description.IfNone(string.Empty),
            url: spec.Metadata.Url.Map(static p => p.Value).IfNone(string.Empty),
            urlTag: spec.Metadata.UrlDescription.IfNone(string.Empty),
            basePoint: spec.BasePoint,
            geometry: members.Geometry.AsEnumerable(),
            attributes: members.Attributes.AsEnumerable()) switch {
                >= 0 => Fin.Succ(value: MutationReceipt.Of(receipt: BlockReceipt.Named(name: spec.Name.Value))),
                _ => Fin.Fail<MutationReceipt>(error: key.InvalidResult()),
            };

    private static Fin<MutationReceipt> Resolve(RhinoBlocks owner, BlockSnapshot existing, AuthorSpec spec, BlockMembers members, ConflictPolicy conflict, Op key) =>
        conflict switch {
            var p when p == ConflictPolicy.Replace => ReplaceGeometry(owner: owner, existing: existing, spec: spec, members: members, key: key),
            var p when p == ConflictPolicy.Skip => Fin.Succ(value: MutationReceipt.Of(
                receipt: DocumentReceipt.Empty,
                diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.DuplicateName(Name: existing.Name, Existing: existing.Id)))),
            var p when p == ConflictPolicy.Rename => AddNew(
                owner: owner,
                spec: spec with { Name = AllocateRename(table: owner.Document.InstanceDefinitions, seed: spec.Name) },
                members: members,
                key: key),
            _ => Fin.Fail<MutationReceipt>(error: key.InvalidInput()),
        };

    private static Fin<MutationReceipt> ReplaceGeometry(RhinoBlocks owner, BlockSnapshot existing, AuthorSpec spec, BlockMembers members, Op key) {
        string name = existing.Name.Value;
        return owner.Document.InstanceDefinitions.ModifyGeometry(
            idefIndex: existing.Index.Value,
            newGeometry: members.Geometry.AsEnumerable(),
            newAttributes: members.Attributes.AsEnumerable())
                ? CommitMetadata(table: owner.Document.InstanceDefinitions, snap: existing, patch: spec.Metadata, key: key)
                    .Map(_ => MutationReceipt.Of(receipt: BlockReceipt.Named(name: name)))
                : Fin.Fail<MutationReceipt>(error: key.InvalidResult());
    }

    private static Fin<Unit> CommitMetadata(InstanceDefinitionTable table, BlockSnapshot snap, MetadataPatch patch, Op key) =>
        table.Modify(
            idefIndex: snap.Index.Value,
            newName: snap.Name.Value,
            newDescription: patch.Description.IfNone(snap.Description.IfNone(string.Empty)),
            newUrl: patch.Url.Map(static p => p.Value).IfNone(snap.Url.Map(static u => u.Value).IfNone(string.Empty)),
            newUrlTag: patch.UrlDescription.IfNone(snap.UrlDescription.IfNone(string.Empty)),
            quiet: true)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult());

    private static DefinitionName AllocateRename(InstanceDefinitionTable table, DefinitionName seed) =>
        DefinitionName.From(value: table.GetUnusedInstanceDefinitionName(root: seed.Value)).IfFail(_ => seed);

    private static Fin<MutationReceipt> ModifyMetadata(RhinoBlocks owner, BlockMutationOp.ModifyMetadata op) =>
        DispatchMutation(nameof(BlockMutationOp.ModifyMetadata), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in op.Patch.IsEmpty
                ? Fin.Succ(value: unit)
                : CommitMetadata(table: owner.Document.InstanceDefinitions, snap: snap, patch: op.Patch, key: key)
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: snap.Name.Value)));

    private static Fin<MutationReceipt> ModifyGeometry(RhinoBlocks owner, BlockMutationOp.ModifyGeometry op) =>
        DispatchMutation(nameof(BlockMutationOp.ModifyGeometry), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in snap.Update == UpdatePolicy.Linked
                ? Fin.Fail<Unit>(error: key.InvalidInput())
                : owner.Document.InstanceDefinitions.ModifyGeometry(
                    idefIndex: snap.Index.Value,
                    newGeometry: op.Members.Geometry.AsEnumerable(),
                    newAttributes: op.Members.Attributes.AsEnumerable())
                        ? Fin.Succ(value: unit)
                        : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: snap.Name.Value)));

    // ---- [OPERATIONS] [MANAGE] [MUTATION] ------------------------------------------------
    private static Fin<MutationReceipt> Rename(RhinoBlocks owner, BlockMutationOp.Rename op) =>
        DispatchMutation(nameof(BlockMutationOp.Rename), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.Modify(
                idefIndex: snap.Index.Value,
                newName: op.NewName.Value,
                newDescription: snap.Description.IfNone(string.Empty),
                quiet: true)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: op.NewName.Value)));

    private static Fin<MutationReceipt> Delete(RhinoBlocks owner, BlockMutationOp.Delete op) =>
        DispatchMutation(nameof(BlockMutationOp.Delete), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.Delete(
                idefIndex: snap.Index.Value,
                deleteReferences: op.Policy.DeleteReferences,
                quiet: op.Policy.Quiet)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value)));

    private static Fin<MutationReceipt> Undelete(RhinoBlocks owner, BlockMutationOp.Undelete op) =>
        DispatchMutation(nameof(BlockMutationOp.Undelete), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.Undelete(idefIndex: snap.Index.Value)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value)));

    private static Fin<MutationReceipt> Purge(RhinoBlocks owner, BlockMutationOp.Purge op) =>
        DispatchMutation(nameof(BlockMutationOp.Purge), key => op.Ref.Case switch {
            DefinitionRef refer => PurgeOne(owner: owner, refer: refer, key: key),
            _ => PurgeAllUnused(owner: owner),
        });

    private static Fin<MutationReceipt> PurgeOne(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _ in owner.Document.InstanceDefinitions.Purge(idefIndex: snap.Index.Value)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Of(receipt: BlockReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value));

    private static Fin<MutationReceipt> PurgeAllUnused(RhinoBlocks owner) {
        _ = owner.Document.InstanceDefinitions.PurgeUnused();
        return Fin.Succ(value: MutationReceipt.Empty);
    }

    private static Fin<MutationReceipt> Compact(RhinoBlocks owner, BlockMutationOp.Compact op) =>
        DispatchMutation(nameof(BlockMutationOp.Compact), _ => {
            owner.Document.InstanceDefinitions.Compact(ignoreUndoReferences: op.IgnoreUndoReferences);
            return Fin.Succ(value: MutationReceipt.Empty);
        });

    // ---- [OPERATIONS] [MANAGE] [QUERY] ---------------------------------------------------
    private static Fin<BlockResult> Lookup(RhinoBlocks owner, BlockQueryOp.Lookup op) =>
        DispatchQuery(nameof(BlockQueryOp.Lookup), key =>
            LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
                .Map(static snap => (BlockResult)new BlockResult.Snapshot(Value: snap)));

    private static Fin<BlockResult> AllocateName(RhinoBlocks owner, BlockQueryOp.AllocateName op) =>
        DispatchQuery(nameof(BlockQueryOp.AllocateName), key => {
            InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
            string raw = op.Seed.Case switch {
                string seed => table.GetUnusedInstanceDefinitionName(root: seed),
                _ => table.GetUnusedInstanceDefinitionName(),
            };
            return DefinitionName.From(value: raw, key: key).Map(static n => (BlockResult)new BlockResult.Name(Value: n));
        });

    private static Fin<BlockResult> Snapshot(RhinoBlocks owner, BlockQueryOp.Snapshot op) =>
        DispatchQuery(nameof(BlockQueryOp.Snapshot), key => op.Ref.Case switch {
            DefinitionRef refer => LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
                .Map(static snap => (BlockResult)new BlockResult.Snapshot(Value: snap)),
            _ => SnapshotAll(table: owner.Document.InstanceDefinitions, key: key),
        });

    private static Fin<BlockResult> SnapshotAll(InstanceDefinitionTable table, Op key) =>
        toSeq(table.GetList(ignoreDeleted: true))
            .Filter(static def => def is not null)
            .Map(def => BlockSnapshot.From(definition: def, key: key))
            .TraverseM(identity)
            .As()
            .Map(static seq => (BlockResult)new BlockResult.Snapshots(Values: seq));

    private static Fin<BlockResult> Audit(RhinoBlocks owner, BlockQueryOp.Audit op) =>
        DispatchQuery(nameof(BlockQueryOp.Audit), key => SnapshotAll(table: owner.Document.InstanceDefinitions, key: key));

    // ---- [OPERATIONS] [PLACE] [MUTATION] -------------------------------------------------
    private static Fin<MutationReceipt> Place(RhinoBlocks owner, BlockMutationOp.Place op) =>
        DispatchMutation(nameof(BlockMutationOp.Place), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from placements in op.At.Map(xform => PlaceOne(table: owner.Document.Objects, index: snap.Index.Value, xform: xform, attrs: op.Attrs, key: key))
                .TraverseM(identity)
                .As()
            from _ in placements.IsEmpty
                ? Fin.Fail<Unit>(error: key.InvalidResult())
                : Fin.Succ(value: unit)
            select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                Created = placements,
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: snap.Name.Value)),
            }));

    private static Fin<Guid> PlaceOne(ObjectTable table, int index, Transform xform, Option<ObjectAttributes> attrs, Op key) {
        ObjectAttributes effective = attrs.IfNone(static () => new ObjectAttributes());
        Guid id = table.AddInstanceObject(instanceDefinitionIndex: index, instanceXform: xform, attributes: effective);
        return id == Guid.Empty
            ? Fin.Fail<Guid>(error: key.InvalidResult())
            : Fin.Succ(value: id);
    }

    private static Fin<MutationReceipt> ReplaceDefinition(RhinoBlocks owner, BlockMutationOp.ReplaceDefinition op) =>
        DispatchMutation(nameof(BlockMutationOp.ReplaceDefinition), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.NewRef, key: key)
            let priorName = ResolveOriginalName(objects: owner.Document.Objects, instanceId: op.InstanceId)
            from _ in owner.Document.Objects.ReplaceInstanceObject(objectId: op.InstanceId, instanceDefinitionIndex: snap.Index.Value)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                Replaced = Seq(op.InstanceId),
                ResourceChanged = ReplaceResourceChanges(prior: priorName, next: snap.Name.Value),
            }));

    private static Option<string> ResolveOriginalName(ObjectTable objects, Guid instanceId) =>
        Optional(objects.FindId(id: instanceId) as InstanceObject)
            .Bind(io => Optional(io.InstanceDefinition))
            .Bind(def => Optional(def.Name).Filter(static n => !string.IsNullOrWhiteSpace(value: n)));

    private static Seq<DocumentResourceChange> ReplaceResourceChanges(Option<string> prior, string next) =>
        prior.Case switch {
            string priorName when !string.Equals(a: priorName, b: next, comparisonType: StringComparison.OrdinalIgnoreCase) =>
                Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: priorName),
                    new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next)),
            _ => Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next)),
        };

    private static Fin<MutationReceipt> TransformCopy(RhinoBlocks owner, BlockMutationOp.TransformCopy op) =>
        DispatchMutation(nameof(BlockMutationOp.TransformCopy), key => {
            Guid id = owner.Document.Objects.Transform(objectId: op.InstanceId, xform: op.Xform, deleteOriginal: op.DeleteOriginal);
            return id == Guid.Empty
                ? Fin.Fail<MutationReceipt>(error: key.InvalidResult())
                : Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                    Created = Seq(id),
                    Deleted = op.DeleteOriginal ? Seq(op.InstanceId) : Seq<Guid>(),
                    Transformed = op.DeleteOriginal ? Seq<Guid>() : Seq(id),
                }));
        });

    private static Fin<MutationReceipt> TransformWithHistory(RhinoBlocks owner, BlockMutationOp.TransformWithHistory op) =>
        DispatchMutation(nameof(BlockMutationOp.TransformWithHistory), key => {
            Guid id = owner.Document.Objects.TransformWithHistory(objectId: op.InstanceId, xform: op.Xform);
            return id == Guid.Empty
                ? Fin.Fail<MutationReceipt>(error: key.InvalidResult())
                : Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with { Created = Seq(id) }));
        });

    private static Fin<MutationReceipt> ExplodeIntoDocument(RhinoBlocks owner, BlockMutationOp.ExplodeIntoDocument op) =>
        DispatchMutation(nameof(BlockMutationOp.ExplodeIntoDocument), key =>
            from instance in Optional(owner.Document.Objects.FindId(id: op.InstanceId) as InstanceObject).ToFin(Fail: key.InvalidInput())
            let raw = owner.Document.Objects.AddExplodedInstancePieces(instance: instance, explodeNestedInstances: true, deleteInstance: true)
            let produced = raw ?? []
            let expected = instance.InstanceDefinition?.GetObjectIds()?.Length ?? produced.Length
            let diagnostics = produced.Length == expected
                ? Seq<BlockDiagnostic>()
                : Seq<BlockDiagnostic>(new BlockDiagnostic.ExplodePartial(Requested: expected, Received: produced.Length))
            select MutationReceipt.Of(
                receipt: DocumentReceipt.Empty with {
                    Created = toSeq(produced),
                    Deleted = Seq(op.InstanceId),
                },
                diagnostics: diagnostics));

    // ---- [OPERATIONS] [PLACE] [QUERY] ----------------------------------------------------
    private static Fin<BlockResult> ExplodeInspect(RhinoBlocks owner, BlockQueryOp.ExplodeInspect op) =>
        DispatchQuery(nameof(BlockQueryOp.ExplodeInspect), key =>
            from instance in Optional(owner.Document.Objects.FindId(id: op.InstanceId) as InstanceObject).ToFin(Fail: key.InvalidInput())
            let pieces = ExplodeNative(instance: instance, policy: op.Policy)
            from contexts in toSeq(pieces)
                .Filter(static p => p is not null)
                .Map(piece => DefinitionId.From(value: instance.InstanceDefinition.Id, key: key)
                    .Map(defId => new BlockMemberContext(DefId: defId, MemberId: piece.Id, Attrs: Optional(piece.Attributes))))
                .TraverseM(identity)
                .As()
            select (BlockResult)new BlockResult.Members(Values: contexts));

    private static RhinoObject[] ExplodeNative(InstanceObject instance, ExplodePolicy policy) {
        instance.Explode(
            skipHiddenPieces: policy.SkipsHidden,
            viewportId: policy.ViewportFilter,
            explodeNestedInstances: true,
            pieces: out RhinoObject[] pieces,
            pieceAttributes: out ObjectAttributes[] _,
            pieceTransforms: out Transform[] _);
        return pieces ?? [];
    }

    // ---- [OPERATIONS] [LINK] -------------------------------------------------------------
    private static Fin<MutationReceipt> CreateArchiveLinks(RhinoBlocks owner, BlockMutationOp.CreateArchiveLinks op) =>
        DispatchMutation(nameof(BlockMutationOp.CreateArchiveLinks), key =>
            op.Sources
                .Map(endpoint => key.Catch(() => CreateOneLink(owner: owner, endpoint: endpoint, key: key)))
                .TraverseM(identity)
                .As()
                .Map(static changes => MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes })));

    private static Fin<DocumentResourceChange> CreateOneLink(RhinoBlocks owner, FileEndpoint endpoint, Op key) {
        string filename = endpoint.Path;
        string name = owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: IOPath.GetFileNameWithoutExtension(path: filename));
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: filename);
        return owner.Document.InstanceDefinitions.Add(
            name: name,
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: [],
            attributes: []) switch {
                int idx when idx >= 0 && owner.Document.InstanceDefinitions.ModifySourceArchive(
                    idefIndex: idx,
                    sourceArchive: reference,
                    updateType: InstanceDefinitionUpdateType.Linked,
                    quiet: true) =>
                    Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: filename)),
                _ => Fin.Fail<DocumentResourceChange>(error: key.InvalidResult()),
            };
    }

    private static Fin<MutationReceipt> UpdateSourceArchive(RhinoBlocks owner, BlockMutationOp.UpdateSourceArchive op) =>
        DispatchMutation(nameof(BlockMutationOp.UpdateSourceArchive), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in ApplySourceArchive(owner: owner, index: snap.Index.Value, source: op.Source, policy: op.Policy, quiet: op.Quiet, key: key)
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: snap.Name.Value)));

    private static Fin<Unit> ApplySourceArchive(RhinoBlocks owner, int index, FileEndpoint source, UpdatePolicy policy, bool quiet, Op key) {
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: source.Path);
        return owner.Document.InstanceDefinitions.ModifySourceArchive(
            idefIndex: index,
            sourceArchive: reference,
            updateType: policy.Native,
            quiet: quiet)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult());
    }

    private static Fin<MutationReceipt> RefreshLinks(RhinoBlocks owner, BlockMutationOp.RefreshLinks op) =>
        DispatchMutation(nameof(BlockMutationOp.RefreshLinks), key => {
            Seq<InstanceDefinition> targets = MatchArchives(
                candidates: LinkedCandidates(table: owner.Document.InstanceDefinitions),
                archives: op.Archives);
            Seq<InstanceDefinition> filtered = op.SkipUpToDate
                ? targets.Filter(static d => d.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate)
                : targets;
            return filtered.TraverseM(definition => owner.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition)
                    ? Fin.Succ(value: definition)
                    : Fin.Fail<InstanceDefinition>(error: key.InvalidResult()))
                .As()
                .Map(refreshed => MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                    ResourceChanged = refreshed.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
                }));
        });

    private static Fin<MutationReceipt> ReloadFromFile(RhinoBlocks owner, BlockMutationOp.ReloadFromFile op) =>
        DispatchMutation(nameof(BlockMutationOp.ReloadFromFile), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                idefIndex: snap.Index.Value,
                filename: op.Source.Path,
                updateNestedLinks: op.UpdateNestedLinks,
                quiet: op.Quiet)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: snap.Name.Value)));

    private static Fin<MutationReceipt> DetachArchive(RhinoBlocks owner, BlockMutationOp.DetachArchive op) =>
        DispatchMutation(nameof(BlockMutationOp.DetachArchive), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.DestroySourceArchive(definition: pair.Live, quiet: op.Quiet)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: pair.Snap.Name.Value)));

    private static Fin<MutationReceipt> FlattenLinked(RhinoBlocks owner, BlockMutationOp.FlattenLinked op) =>
        DispatchMutation(nameof(BlockMutationOp.FlattenLinked), key => {
            Seq<InstanceDefinition> candidates = LinkedCandidates(table: owner.Document.InstanceDefinitions)
                .Filter(static d => d.ArchiveFileStatus >= InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate);
            Seq<InstanceDefinition> targets = MatchRefs(candidates: MatchArchives(candidates: candidates, archives: op.Archives), refs: op.Refs);
            _ = targets.Iter(static d => d.LayerStyle = InstanceDefinitionLayerStyle.Active);
            Seq<BlockDiagnostic> ignored = targets
                .Filter(static d => d.LayerStyle != InstanceDefinitionLayerStyle.Active)
                .Choose(static d => DefinitionId.From(value: d.Id).ToOption()
                    .Map(id => (BlockDiagnostic)new BlockDiagnostic.LinkedSetterIgnored(
                        Id: id,
                        Actual: UpdatePolicy.FromNative(native: d.UpdateType))));
            return Fin.Succ(value: MutationReceipt.Of(
                receipt: DocumentReceipt.Empty with {
                    ResourceChanged = targets.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
                },
                diagnostics: ignored));
        });

    private static Seq<InstanceDefinition> LinkedCandidates(InstanceDefinitionTable table) =>
        toSeq(table)
            .Filter(static d => d.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded);

    private static Seq<InstanceDefinition> MatchArchives(Seq<InstanceDefinition> candidates, Option<Seq<string>> archives) =>
        archives.Case switch {
            Seq<string> filter => candidates.Filter(d => filter.Exists(p => string.Equals(a: p, b: d.SourceArchive ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase))),
            _ => candidates,
        };

    private static Seq<InstanceDefinition> MatchRefs(Seq<InstanceDefinition> candidates, Option<Seq<DefinitionRef>> refs) =>
        refs.Case switch {
            Seq<DefinitionRef> filter => candidates.Filter(d => filter.Exists(r => MatchesRef(definition: d, refer: r))),
            _ => candidates,
        };

    private static bool MatchesRef(InstanceDefinition definition, DefinitionRef refer) =>
        refer switch {
            DefinitionRef.ById byId => definition.Id == byId.Id.Value,
            DefinitionRef.ByIndex byIdx => definition.Index == byIdx.Index.Value,
            DefinitionRef.ByName byName => string.Equals(a: definition.Name, b: byName.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

    // ---- [OPERATIONS] [GRAPH] ------------------------------------------------------------
    private static Fin<BlockResult> GraphMembers(RhinoBlocks owner, BlockQueryOp.GraphMembers op) =>
        DispatchQuery(nameof(BlockQueryOp.GraphMembers), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            let members = toSeq(pair.Live.GetObjects())
                .Filter(static obj => obj is not null)
                .Map(obj => new BlockMemberContext(DefId: pair.Snap.Id, MemberId: obj.Id, Attrs: Optional(obj.Attributes)))
            select (BlockResult)new BlockResult.Members(Values: members));

    private static Fin<BlockResult> GraphInserts(RhinoBlocks owner, BlockQueryOp.GraphInserts op) =>
        DispatchQuery(nameof(BlockQueryOp.GraphInserts), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from inserts in toSeq(pair.Live.GetReferences(wheretoLook: op.Scope.Native))
                .Filter(static inst => inst is not null)
                .Map(static inst => BlockInsertContext.From(instance: inst))
                .TraverseM(identity)
                .As()
            select (BlockResult)new BlockResult.Inserts(Values: inserts));

    private static Fin<BlockResult> GraphContainers(RhinoBlocks owner, BlockQueryOp.GraphContainers op) =>
        DispatchQuery(nameof(BlockQueryOp.GraphContainers), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from containers in toSeq(pair.Live.GetContainers())
                .Filter(static c => c is not null)
                .Map(c => DefinitionId.From(value: c.Id, key: key))
                .TraverseM(identity)
                .As()
            select (BlockResult)new BlockResult.Definitions(Values: containers));

    private static Fin<BlockResult> SelectedPart(RhinoBlocks owner, BlockQueryOp.SelectedPart op) =>
        DispatchQuery(nameof(BlockQueryOp.SelectedPart), key => op.Ref.InstanceDefinitionPart() switch {
            RhinoObject member when member.Attributes.IsInstanceDefinitionObject =>
                DefinitionId.From(value: op.Ref.ObjectId, key: key)
                    .Map(defId => (BlockResult)new BlockResult.Members(Values: Seq(new BlockMemberContext(DefId: defId, MemberId: member.Id, Attrs: Optional(member.Attributes))))),
            _ => Fin.Fail<BlockResult>(error: key.InvalidResult()),
        });

    private static Fin<BlockResult> DependencyAudit(RhinoBlocks owner, BlockQueryOp.DependencyAudit op) =>
        DispatchQuery(nameof(BlockQueryOp.DependencyAudit), key => {
            Seq<InstanceDefinition> definitions = toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
                .Filter(static d => d is not null);
            Seq<BlockGraphNode> nodes = definitions.Choose(d => ProjectNode(definition: d, key: key));
            Seq<BlockGraphEdge> edges = definitions.Bind(d => ProjectEdges(definition: d, key: key));
            BlockGraph graph = new(Nodes: [.. nodes], Edges: [.. edges]);
            return Fin.Succ(value: (BlockResult)new BlockResult.Graph(Value: graph));
        });

    private static Option<BlockGraphNode> ProjectNode(InstanceDefinition definition, Op key) =>
        from id in DefinitionId.From(value: definition.Id, key: key).ToOption()
        from name in DefinitionName.From(value: definition.Name ?? string.Empty, key: key).ToOption()
        select new BlockGraphNode(Id: id, Name: name, Members: [.. definition.GetObjectIds() ?? []]);

    private static Seq<BlockGraphEdge> ProjectEdges(InstanceDefinition definition, Op key) =>
        DefinitionId.From(value: definition.Id, key: key).ToOption() switch {
            { IsSome: true, Case: DefinitionId from } => MemberEdges(definition: definition, from: from) + ArchiveEdges(definition: definition, from: from, key: key),
            _ => Seq<BlockGraphEdge>(),
        };

    private static Seq<BlockGraphEdge> MemberEdges(InstanceDefinition definition, DefinitionId from) =>
        toSeq(definition.GetObjectIds() ?? [])
            .Map(id => new BlockGraphEdge(From: from, Kind: BlockEdgeKind.Member, To: new EdgeTarget.ObjectId(Id: id)));

    private static Seq<BlockGraphEdge> ArchiveEdges(InstanceDefinition definition, DefinitionId from, Op key) =>
        string.IsNullOrWhiteSpace(value: definition.SourceArchive)
            ? Seq<BlockGraphEdge>()
            : ArchivePath.From(value: definition.SourceArchive, key: key).ToOption() switch {
                { IsSome: true, Case: ArchivePath path } => Seq(new BlockGraphEdge(From: from, Kind: BlockEdgeKind.LinkedArchive, To: new EdgeTarget.Archive(Path: path))),
                _ => Seq<BlockGraphEdge>(),
            };

    // ---- [OPERATIONS] [OBSERVE] ----------------------------------------------------------
    private static Fin<BlockResult> Preview(RhinoBlocks owner, BlockQueryOp.Preview op) =>
        DispatchQuery(nameof(BlockQueryOp.Preview), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from handle in owner.AcquirePreview(definition: pair.Live, spec: op.Spec, key: key)
            select (BlockResult)new BlockResult.Preview(Handle: handle));

    private static Fin<MutationReceipt> Export(RhinoBlocks owner, BlockMutationOp.Export op) =>
        DispatchMutation(nameof(BlockMutationOp.Export), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            from _ in owner.Document.InstanceDefinitions.Export(idefIndex: snap.Index.Value, filename: op.Target.Path)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
            select MutationReceipt.Of(receipt: BlockReceipt.Named(name: snap.Name.Value)));

    private static Fin<BlockResult> TextFieldsOf(RhinoBlocks owner, BlockQueryOp.TextFieldsOf op) =>
        DispatchQuery(nameof(BlockQueryOp.TextFieldsOf), key =>
            from snap in LookupSnapshot(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            let idStr = snap.Id.Value.ToString()
            let pairs = TextFieldPairs(idStr: idStr)
            let fields = pairs
                .Filter(static p => !string.IsNullOrWhiteSpace(value: p.Value) && !string.Equals(a: p.Value, b: "####", comparisonType: StringComparison.Ordinal))
                .Fold(HashMap<string, string>(), static (m, p) => m.AddOrUpdate(key: p.Key, value: p.Value))
            select (BlockResult)new BlockResult.Texts(Fields: fields));

    private static Seq<(string Key, string Value)> TextFieldPairs(string idStr) =>
        Seq(
            (Key: "BlockName", Value: TextFields.BlockName(blockId: idStr) ?? string.Empty),
            (Key: "BlockInstanceCount", Value: TextFields.BlockInstanceCount(instanceDefinitionNameOrId: idStr).ToString(provider: System.Globalization.CultureInfo.InvariantCulture)),
            (Key: "BlockDescription", Value: TextFields.BlockDescription(definitionNameOrId: idStr) ?? string.Empty),
            (Key: "BlockInsertionCoordinate.X", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: "x") ?? string.Empty),
            (Key: "BlockInsertionCoordinate.Y", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: "y") ?? string.Empty),
            (Key: "BlockInsertionCoordinate.Z", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: "z") ?? string.Empty));

    private static Fin<BlockResult> AttributeFieldsOf(RhinoBlocks owner, BlockQueryOp.AttributeFieldsOf op) =>
        DispatchQuery(nameof(BlockQueryOp.AttributeFieldsOf), key =>
            from pair in LookupLive(table: owner.Document.InstanceDefinitions, refer: op.Ref, key: key)
            let fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
            select (BlockResult)new BlockResult.Attributes(Values: fields));
}
