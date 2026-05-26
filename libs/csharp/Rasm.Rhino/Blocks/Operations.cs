using System.Collections.Immutable;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using IOPath = System.IO.Path;
using TextFields = Rhino.Runtime.TextFields;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] [MUTATION] -------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockMutation {
    private BlockMutation() { }

    // [AUTHOR]
    public sealed record Author(AuthorSpec Spec, Members Source, ConflictPolicy Conflict) : BlockMutation;
    public sealed record AddOrReuse(AuthorSpec Spec, Members Source) : BlockMutation;
    public sealed record ModifyMetadata(DefinitionRef Ref, MetadataPatch Patch) : BlockMutation;
    public sealed record ModifyGeometry(DefinitionRef Ref, Members Source) : BlockMutation;

    // [MANAGE]
    public sealed record Rename(DefinitionRef Ref, DefinitionName NewName) : BlockMutation;
    public sealed record Delete(DefinitionRef Ref, DeletionPolicy Policy) : BlockMutation;
    public sealed record Undelete(DefinitionRef Ref) : BlockMutation;
    public sealed record UndoModify(DefinitionRef Ref) : BlockMutation;
    public sealed record Purge(Option<DefinitionRef> Ref) : BlockMutation;
    public sealed record PurgeByPrefix(string Prefix) : BlockMutation;
    public sealed record Compact(CompactPolicy? Policy = null) : BlockMutation;

    // [PLACE]
    public sealed record Place(DefinitionRef Ref, Seq<Transform> At, Option<ObjectAttributes> Attrs, Option<HistoryRecord> History = default) : BlockMutation;
    public sealed record BatchPlace(DefinitionRef Ref, Seq<Placement> At, BatchPolicy Policy) : BlockMutation;
    public sealed record ReplaceDefinition(Guid InstanceId, DefinitionRef NewRef) : BlockMutation;
    public sealed record TransformCopy(Guid InstanceId, Transform Xform, TransformPolicy? Policy = null) : BlockMutation;
    public sealed record TransformWithHistory(Guid InstanceId, Transform Xform) : BlockMutation;
    public sealed record ExplodeIntoDocument(Guid InstanceId, ExplodePolicy Policy) : BlockMutation;

    // [LINK]
    public sealed record CreateArchiveLinks(Seq<FileEndpoint> Sources, LinkCreatePolicy? Policy = null) : BlockMutation;
    public sealed record UpdateSourceArchive(DefinitionRef Ref, FileEndpoint Source, UpdatePolicy Policy, LinkReloadPolicy? Reload = null) : BlockMutation;
    public sealed record RefreshLinks(Option<Seq<string>> Archives, LinkRefreshPolicy? Policy = null) : BlockMutation;
    public sealed record ReloadFromFile(DefinitionRef Ref, FileEndpoint Source, LinkReloadPolicy? Policy = null) : BlockMutation;
    public sealed record DetachArchive(DefinitionRef Ref, LinkReloadPolicy? Policy = null) : BlockMutation;
    public sealed record FlattenLinked(Option<Seq<string>> Archives, Option<Seq<DefinitionRef>> Refs) : BlockMutation;

    // [OBSERVE] [SIDE-EFFECT]
    public sealed record Export(DefinitionRef Ref, FileEndpoint Target) : BlockMutation;
    public sealed record ExportAttributes(DefinitionRef Ref, FileEndpoint Target) : BlockMutation;
    public sealed record SnapshotBlock(DefinitionRef Ref, SnapshotName Name) : BlockMutation;
}

// --- [TYPES] [QUERY] ----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockQuery {
    private BlockQuery() { }

    // [MANAGE]
    public sealed record Lookup(DefinitionRef Ref) : BlockQuery;
    public sealed record AllocateName(Option<string> Seed) : BlockQuery;
    public sealed record Snapshot(Option<DefinitionRef> Ref) : BlockQuery;
    public sealed record Audit() : BlockQuery;

    // [PLACE]
    public sealed record ExplodeInspect(Guid InstanceId, ExplodePolicy Policy) : BlockQuery;
    public sealed record UseSubObject(Guid InstanceId, ComponentIndex Component) : BlockQuery;

    // [GRAPH]
    public sealed record GraphMembers(DefinitionRef Ref) : BlockQuery;
    public sealed record GraphInserts(DefinitionRef Ref, ReferenceScope Scope) : BlockQuery;
    public sealed record GraphContainers(DefinitionRef Ref) : BlockQuery;
    public sealed record SelectedPart(ObjRef Ref) : BlockQuery;
    public sealed record DependencyAudit() : BlockQuery;
    public sealed record LinkedHealth(Option<Seq<string>> Archives, Option<Seq<DefinitionRef>> Refs) : BlockQuery;
    public sealed record DependsOn(DefinitionRef Ref, DependencyTarget Target) : BlockQuery;
    public sealed record Flatten(Guid InstanceId, FlattenPolicy Policy) : BlockQuery;

    // [OBSERVE]
    public sealed record Preview(DefinitionRef Ref, PreviewSpec Spec) : BlockQuery;
    public sealed record TextFieldsOf(DefinitionRef Ref) : BlockQuery;
    public sealed record AttributeFieldsOf(DefinitionRef Ref) : BlockQuery;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class Operations {
    // ---- [DISPATCH] ----------------------------------------------------------------------
    internal static Fin<MutationReceipt> RunMutation(BlockMutation op, RhinoBlocks owner) =>
        op.Switch(
            owner: owner,
            author: static (o, x) => RunMut(name: nameof(BlockMutation.Author), body: k => PerformAuthor(owner: o, spec: x.Spec, source: x.Source, conflict: x.Conflict, key: k)),
            addOrReuse: static (o, x) => RunMut(name: nameof(BlockMutation.AddOrReuse), body: k => PerformAddOrReuse(owner: o, spec: x.Spec, source: x.Source, key: k)),
            modifyMetadata: static (o, x) => RunMut(name: nameof(BlockMutation.ModifyMetadata), body: k => PerformModifyMetadata(owner: o, refer: x.Ref, patch: x.Patch, key: k)),
            modifyGeometry: static (o, x) => RunMut(name: nameof(BlockMutation.ModifyGeometry), body: k => PerformModifyGeometry(owner: o, refer: x.Ref, source: x.Source, key: k)),
            rename: static (o, x) => RunMut(name: nameof(BlockMutation.Rename), body: k => PerformRename(owner: o, refer: x.Ref, newName: x.NewName, key: k)),
            delete: static (o, x) => RunMut(name: nameof(BlockMutation.Delete), body: k => PerformDelete(owner: o, refer: x.Ref, policy: x.Policy, key: k)),
            undelete: static (o, x) => RunMut(name: nameof(BlockMutation.Undelete), body: k => PerformUndelete(owner: o, refer: x.Ref, key: k)),
            undoModify: static (o, x) => RunMut(name: nameof(BlockMutation.UndoModify), body: k => PerformUndoModify(owner: o, refer: x.Ref, key: k)),
            purge: static (o, x) => RunMut(name: nameof(BlockMutation.Purge), body: k => PerformPurge(owner: o, refer: x.Ref, key: k)),
            purgeByPrefix: static (o, x) => RunMut(name: nameof(BlockMutation.PurgeByPrefix), body: k => PerformPurgeByPrefix(owner: o, prefix: x.Prefix, key: k)),
            compact: static (o, x) => RunMut(name: nameof(BlockMutation.Compact), body: k => PerformCompact(owner: o, policy: x.Policy ?? CompactPolicy.UndoAware, key: k)),
            place: static (o, x) => RunMut(name: nameof(BlockMutation.Place), body: k => PerformPlace(owner: o, refer: x.Ref, at: x.At, attrs: x.Attrs, history: x.History, key: k)),
            batchPlace: static (o, x) => RunMut(name: nameof(BlockMutation.BatchPlace), body: k => PerformBatchPlace(owner: o, refer: x.Ref, at: x.At, policy: x.Policy, key: k)),
            replaceDefinition: static (o, x) => RunMut(name: nameof(BlockMutation.ReplaceDefinition), body: k => PerformReplaceDefinition(owner: o, instanceId: x.InstanceId, newRef: x.NewRef, key: k)),
            transformCopy: static (o, x) => RunMut(name: nameof(BlockMutation.TransformCopy), body: k => PerformTransformCopy(owner: o, instanceId: x.InstanceId, xform: x.Xform, policy: x.Policy ?? TransformPolicy.Copy, key: k)),
            transformWithHistory: static (o, x) => RunMut(name: nameof(BlockMutation.TransformWithHistory), body: k => PerformTransformWithHistory(owner: o, instanceId: x.InstanceId, xform: x.Xform, key: k)),
            explodeIntoDocument: static (o, x) => RunMut(name: nameof(BlockMutation.ExplodeIntoDocument), body: k => PerformExplodeIntoDocument(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            createArchiveLinks: static (o, x) => RunMut(name: nameof(BlockMutation.CreateArchiveLinks), body: k => PerformCreateArchiveLinks(owner: o, sources: x.Sources, policy: x.Policy ?? LinkCreatePolicy.Default, key: k)),
            updateSourceArchive: static (o, x) => RunMut(name: nameof(BlockMutation.UpdateSourceArchive), body: k => PerformUpdateSourceArchive(owner: o, refer: x.Ref, source: x.Source, policy: x.Policy, reload: x.Reload ?? LinkReloadPolicy.NestedQuiet, key: k)),
            refreshLinks: static (o, x) => RunMut(name: nameof(BlockMutation.RefreshLinks), body: k => PerformRefreshLinks(owner: o, archives: x.Archives, policy: x.Policy ?? LinkRefreshPolicy.All, key: k)),
            reloadFromFile: static (o, x) => RunMut(name: nameof(BlockMutation.ReloadFromFile), body: k => PerformReloadFromFile(owner: o, refer: x.Ref, source: x.Source, policy: x.Policy ?? LinkReloadPolicy.NestedQuiet, key: k)),
            detachArchive: static (o, x) => RunMut(name: nameof(BlockMutation.DetachArchive), body: k => PerformDetachArchive(owner: o, refer: x.Ref, policy: x.Policy ?? LinkReloadPolicy.NestedQuiet, key: k)),
            flattenLinked: static (o, x) => RunMut(name: nameof(BlockMutation.FlattenLinked), body: k => PerformFlattenLinked(owner: o, archives: x.Archives, refs: x.Refs, key: k)),
            export: static (o, x) => RunMut(name: nameof(BlockMutation.Export), body: k => PerformExport(owner: o, refer: x.Ref, target: x.Target, key: k)),
            exportAttributes: static (o, x) => RunMut(name: nameof(BlockMutation.ExportAttributes), body: k => PerformExportAttributes(owner: o, refer: x.Ref, target: x.Target, key: k)),
            snapshotBlock: static (o, x) => RunMut(name: nameof(BlockMutation.SnapshotBlock), body: k => PerformSnapshotBlock(owner: o, refer: x.Ref, name: x.Name, key: k)));

    internal static Fin<BlockResult> RunQuery(BlockQuery op, RhinoBlocks owner) =>
        op.Switch(
            owner: owner,
            lookup: static (o, x) => RunQry(name: nameof(BlockQuery.Lookup), body: k => PerformLookup(owner: o, refer: x.Ref, key: k)),
            allocateName: static (o, x) => RunQry(name: nameof(BlockQuery.AllocateName), body: k => PerformAllocateName(owner: o, seed: x.Seed, key: k)),
            snapshot: static (o, x) => RunQry(name: nameof(BlockQuery.Snapshot), body: k => PerformSnapshot(owner: o, refer: x.Ref, key: k)),
            audit: static (o, _) => RunQry(name: nameof(BlockQuery.Audit), body: k => PerformAudit(owner: o, key: k)),
            explodeInspect: static (o, x) => RunQry(name: nameof(BlockQuery.ExplodeInspect), body: k => PerformExplodeInspect(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            useSubObject: static (o, x) => RunQry(name: nameof(BlockQuery.UseSubObject), body: k => PerformUseSubObject(owner: o, instanceId: x.InstanceId, component: x.Component, key: k)),
            graphMembers: static (o, x) => RunQry(name: nameof(BlockQuery.GraphMembers), body: k => PerformGraphMembers(owner: o, refer: x.Ref, key: k)),
            graphInserts: static (o, x) => RunQry(name: nameof(BlockQuery.GraphInserts), body: k => PerformGraphInserts(owner: o, refer: x.Ref, scope: x.Scope, key: k)),
            graphContainers: static (o, x) => RunQry(name: nameof(BlockQuery.GraphContainers), body: k => PerformGraphContainers(owner: o, refer: x.Ref, key: k)),
            selectedPart: static (o, x) => RunQry(name: nameof(BlockQuery.SelectedPart), body: k => PerformSelectedPart(owner: o, refer: x.Ref, key: k)),
            dependencyAudit: static (o, _) => RunQry(name: nameof(BlockQuery.DependencyAudit), body: k => PerformDependencyAudit(owner: o, key: k)),
            linkedHealth: static (o, x) => RunQry(name: nameof(BlockQuery.LinkedHealth), body: k => PerformLinkedHealth(owner: o, archives: x.Archives, refs: x.Refs, key: k)),
            dependsOn: static (o, x) => RunQry(name: nameof(BlockQuery.DependsOn), body: k => PerformDependsOn(owner: o, refer: x.Ref, target: x.Target, key: k)),
            flatten: static (o, x) => RunQry(name: nameof(BlockQuery.Flatten), body: k => PerformFlatten(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            preview: static (o, x) => RunQry(name: nameof(BlockQuery.Preview), body: k => PerformPreview(owner: o, refer: x.Ref, spec: x.Spec, key: k)),
            textFieldsOf: static (o, x) => RunQry(name: nameof(BlockQuery.TextFieldsOf), body: k => PerformTextFields(owner: o, refer: x.Ref, key: k)),
            attributeFieldsOf: static (o, x) => RunQry(name: nameof(BlockQuery.AttributeFieldsOf), body: k => PerformAttributeFields(owner: o, refer: x.Ref, key: k)));

    private static Fin<MutationReceipt> RunMut(string name, Func<Op, Fin<MutationReceipt>> body) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key));
    }

    private static Fin<BlockResult> RunQry(string name, Func<Op, Fin<BlockResult>> body) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key));
    }

    // ---- [RESOLVE] -----------------------------------------------------------------------
    internal static Fin<(Definition Snap, InstanceDefinition Live)> Resolve(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindLive(table: table, refer: refer, key: key)
            .Bind(live => SnapshotFromLive(table: table, live: live, key: key).Map(snap => (Snap: snap, Live: live)));

    internal static Fin<Definition> ResolveSnap(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindLive(table: table, refer: refer, key: key)
            .Bind(live => SnapshotFromLive(table: table, live: live, key: key));

    private static Fin<(Definition Snap, InstanceDefinition Live)> ResolveIncludingDeleted(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindDefinition(table: table, refer: refer, includeDeleted: true, key: key)
            .Bind(live => SnapshotFromLive(table: table, live: live, key: key).Map(snap => (Snap: snap, Live: live)));

    private static Fin<Definition> SnapshotFromLive(InstanceDefinitionTable table, InstanceDefinition live, Op key) {
        uint serial = table.Document.RuntimeSerialNumber;
        return DefinitionId.From(value: live.Id, key: key).Bind(id =>
            SnapshotVault.Find(docSerial: serial, id: id).Case switch {
                Definition cached => Fin.Succ(value: cached),
                _ => Definition.From(active: live, key: key)
                    .Map(snap => { _ = SnapshotVault.Upsert(docSerial: serial, definition: snap); return snap; }),
            });
    }

    private static Fin<InstanceDefinition> FindLive(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        FindDefinition(table: table, refer: refer, includeDeleted: false, key: key);

    private static Fin<InstanceDefinition> FindDefinition(InstanceDefinitionTable table, DefinitionRef refer, bool includeDeleted, Op key) =>
        refer switch {
            DefinitionRef.ById r =>
                Optional(table.Find(instanceId: r.Id.Value, ignoreDeletedInstanceDefinitions: !includeDeleted)).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByIndex r when r.Index.Value >= 0 && r.Index.Value < table.Count =>
                Optional(table[r.Index.Value]).Filter(d => includeDeleted || !d.IsDeleted).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByName r =>
                FindByName(table: table, name: r.Name, includeDeleted: includeDeleted).ToFin(Fail: key.InvalidInput()),
            _ => Fin.Fail<InstanceDefinition>(error: key.InvalidInput()),
        };

    private static Option<InstanceDefinition> FindByName(InstanceDefinitionTable table, DefinitionName name, bool includeDeleted) =>
        includeDeleted
            ? toSeq(table.GetList(ignoreDeleted: false))
                .Filter(static d => d is not null)
                .Find(d => string.Equals(a: d.Name, b: name.Value, comparisonType: StringComparison.OrdinalIgnoreCase)
                           || string.Equals(a: d.DeletedName, b: name.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
            : Optional(table.Find(instanceDefinitionName: name.Value));

    internal static Unit InvalidateDefinition(RhinoDoc doc, Guid defId) {
        _ = SnapshotVault.Invalidate(defId: defId);
        _ = PreviewVault.Invalidate(defId: defId);
        _ = ContentIndex.EvictDoc(serial: doc.RuntimeSerialNumber);
        return unit;
    }

    private static T InvalidateWith<T>(RhinoDoc doc, Guid defId, T value) {
        _ = InvalidateDefinition(doc: doc, defId: defId);
        return value;
    }

    /// Archive-only Definitions have Index = None; mutation arms require a live index.
    private static Fin<int> RequireIndex(Definition snap, Op key) =>
        snap.Index.Case switch {
            DefinitionIndex i => Fin.Succ(value: i.Value),
            _ => Fin.Fail<int>(error: key.InvalidInput()),
        };

    private static Fin<InstanceObject> AsInstance(ObjectTable objects, Guid instanceId, Op key) =>
        Optional(objects.FindId(id: instanceId) as InstanceObject).ToFin(Fail: key.InvalidInput());

    // ---- [AUTHOR] ------------------------------------------------------------------------
    private static Fin<MutationReceipt> PerformAuthor(RhinoBlocks owner, AuthorSpec spec, Members source, ConflictPolicy conflict, Op key) =>
        spec.Admit(key: key).Bind(admitted =>
            ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(admitted.Name), key: key).ToOption() switch {
                { IsSome: true, Case: Definition existing } => ResolveConflict(owner: owner, existing: existing, spec: admitted, source: source, conflict: conflict, key: key),
                _ => Reify(doc: owner.Document, source: source, key: key)
                    .Bind(provided => AddNew(owner: owner, spec: admitted, members: provided, key: key)),
            });

    /// Lazy materialization — Skip never reifies, never duplicates.
    private static Fin<Members.Provided> Reify(RhinoDoc doc, Members source, Op key) =>
        source switch {
            Members.Provided p => Fin.Succ(value: p),
            Members.FromDocument fd => ReifyDocumentMembers(doc: doc, sources: fd.Sources, key: key),
            _ => Fin.Fail<Members.Provided>(error: key.InvalidInput()),
        };

    internal static Fin<Members.Provided> ReifyDocumentMembers(RhinoDoc doc, Seq<Guid> sources, Op key) =>
        sources.IsEmpty
            ? Fin.Fail<Members.Provided>(error: key.InvalidInput())
            : sources
                .Map(id => Optional(doc.Objects.FindId(id: id)).ToFin(Fail: key.InvalidInput()))
                .TraverseM(identity).As()
                .Bind(objs => Members.OfProvided(
                    geometry: objs.Map(static o => o.Geometry.Duplicate()),
                    attributes: objs.Map(static o => o.Attributes.Duplicate()),
                    key: key));

    private static Fin<MutationReceipt> ResolveConflict(RhinoBlocks owner, Definition existing, AuthorSpec spec, Members source, ConflictPolicy conflict, Op key) =>
        conflict switch {
            _ when conflict == ConflictPolicy.Replace =>
                Reify(doc: owner.Document, source: source, key: key)
                    .Bind(provided => ReplaceGeometry(owner: owner, existing: existing, spec: spec, members: provided, key: key)),
            _ when conflict == ConflictPolicy.Skip =>
                Fin.Succ(value: MutationReceipt.Of(
                    receipt: DocumentReceipt.Empty,
                    diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.DuplicateName(Name: existing.Name, Existing: existing.Id)))),
            _ when conflict == ConflictPolicy.Rename =>
                Reify(doc: owner.Document, source: source, key: key)
                    .Bind(provided => AddNew(
                        owner: owner,
                        spec: spec with { Name = AllocateRename(table: owner.Document.InstanceDefinitions, seed: spec.Name) },
                        members: provided,
                        key: key)),
            _ when conflict == ConflictPolicy.Fail =>
                Fin.Succ(value: MutationReceipt.Of(
                    receipt: DocumentReceipt.Empty,
                    diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.ConflictFailed(Name: existing.Name, Existing: existing.Id)))),
            _ => Fin.Fail<MutationReceipt>(error: key.InvalidInput()),
        };

    private static Fin<MutationReceipt> AddNew(RhinoBlocks owner, AuthorSpec spec, Members.Provided members, Op key) =>
        owner.Document.InstanceDefinitions.Add(
            name: spec.Name.Value,
            description: spec.Metadata.Description.IfNone(noneValue: string.Empty),
            url: spec.Metadata.Url.Map(static p => p.Value).IfNone(noneValue: string.Empty),
            urlTag: spec.Metadata.UrlDescription.IfNone(noneValue: string.Empty),
            basePoint: spec.BasePoint,
            geometry: members.Geometry.AsEnumerable(),
            attributes: members.Attributes.AsEnumerable()) switch {
                int idx when idx >= 0 => ApplyCreateState(table: owner.Document.InstanceDefinitions, idx: idx, spec: spec, key: key),
                _ => Fin.Fail<MutationReceipt>(error: key.InvalidResult()),
            };

    private static Fin<MutationReceipt> ApplyCreateState(InstanceDefinitionTable table, int idx, AuthorSpec spec, Op key) =>
        from live in Optional(table[idx]).ToFin(Fail: key.InvalidResult())
        from id in DefinitionId.From(value: live.Id, key: key)
        let layerApplied = ApplyLayer(live: live, style: spec.Layer)
        from userStringsApplied in spec.Metadata.UserStrings.IsEmpty
            ? Fin.Succ(value: unit)
            : ApplyUserStrings(table: table, idx: idx, strings: spec.Metadata.UserStrings, key: key)
        from snap in Definition.From(active: live, key: key)
        let cached = SnapshotVault.Upsert(docSerial: table.Document.RuntimeSerialNumber, definition: snap)
        let invalidated = ContentIndex.EvictDoc(serial: table.Document.RuntimeSerialNumber)
        select MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: spec.Name.Value).Document,
            diagnostics: AuthorDiagnostics(id: id, spec: spec, live: live));

    private static Unit ApplyLayer(InstanceDefinition live, LayerStyle style) {
        // BOUNDARY ADAPTER — Rhino exposes linked layer style as mutable native state.
        live.LayerStyle = style.Native;
        return unit;
    }

    private static Seq<BlockDiagnostic> AuthorDiagnostics(DefinitionId id, AuthorSpec spec, InstanceDefinition live) =>
        (spec.Metadata.UserStrings.IsEmpty ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.SilentUserStringMutation(Id: id)))
        + (spec.Update == UpdatePolicy.Static ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.SourceArchiveRequired(Id: id, Requested: spec.Update)))
        + (live.LayerStyle == spec.Layer.Native ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.LinkedSetterIgnored(Id: id, Actual: UpdatePolicy.FromNative(native: live.UpdateType))));

    private static Fin<MutationReceipt> ReplaceGeometry(RhinoBlocks owner, Definition existing, AuthorSpec spec, Members.Provided members, Op key) =>
        from idx in RequireIndex(snap: existing, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.ModifyGeometry(
            idefIndex: idx,
            newGeometry: members.Geometry.AsEnumerable(),
            newAttributes: members.Attributes.AsEnumerable()))
        from __ in CommitMetadata(table: owner.Document.InstanceDefinitions, snap: existing, patch: spec.Metadata, key: key)
        select InvalidateWith(doc: owner.Document, defId: existing.Id.Value, value: MutationReceipt.Named(name: existing.Name.Value));

    private static DefinitionName AllocateRename(InstanceDefinitionTable table, DefinitionName seed) =>
        DefinitionName.From(value: table.GetUnusedInstanceDefinitionName(root: seed.Value)).IfFail(_ => seed);

    private static Fin<MutationReceipt> PerformAddOrReuse(RhinoBlocks owner, AuthorSpec spec, Members source, Op key) =>
        spec.Admit(key: key)
            .Bind(admitted => Reify(doc: owner.Document, source: source, key: key)
                .Bind(provided => LookupByContent(owner: owner, provided: provided, spec: admitted, key: key)));

    private static Fin<MutationReceipt> LookupByContent(RhinoBlocks owner, Members.Provided provided, AuthorSpec spec, Op key) {
        BlockContentHash hash = BlockContentHash.Of(members: provided);
        return ContentIndex.Find(doc: owner.Document, hash: hash)
            .Choose(defId => EquivalentDefinition(doc: owner.Document, defId: defId, provided: provided, key: key).Map(_ => defId))
            .Find(static _ => true) switch {
                { IsSome: true, Case: DefinitionId existing } => Fin.Succ(value: MutationReceipt.Of(
                    receipt: DocumentReceipt.Empty,
                    diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.ReusedExisting(Existing: existing)))),
                _ => AddNew(owner: owner, spec: spec, members: provided, key: key)
                    .Bind(receipt => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(spec.Name), key: key)
                        .Map(snap => { _ = ContentIndex.RegisterIfMissing(doc: owner.Document, hash: hash, defId: snap.Id); return receipt; })),
            };
    }

    private static Option<Unit> EquivalentDefinition(RhinoDoc doc, DefinitionId defId, Members.Provided provided, Op key) =>
        ResolveSnap(table: doc.InstanceDefinitions, refer: DefinitionRef.Of(defId), key: key).ToOption()
            .Bind(snap => ReifyDocumentMembers(doc: doc, sources: toSeq(snap.MemberIds), key: key).ToOption())
            .Filter(existing => EquivalentMembers(left: existing, right: provided))
            .Map(static _ => unit);

    private static bool EquivalentMembers(Members.Provided left, Members.Provided right) =>
        left.Geometry.Count == right.Geometry.Count
        && left.Attributes.Count == right.Attributes.Count
        && left.Geometry.Zip(right.Geometry).ForAll(static pair => GeometryBase.GeometryEquals(first: pair.Item1, second: pair.Item2))
        && left.Attributes.Zip(right.Attributes).ForAll(static pair => EquivalentAttributes(left: pair.Item1, right: pair.Item2));

    private static bool EquivalentAttributes(ObjectAttributes left, ObjectAttributes right) =>
        left.LayerIndex == right.LayerIndex
        && left.MaterialIndex == right.MaterialIndex
        && left.LinetypeIndex == right.LinetypeIndex
        && left.ObjectColor.ToArgb() == right.ObjectColor.ToArgb()
        && left.PlotColor.ToArgb() == right.PlotColor.ToArgb()
        && string.Equals(a: left.Name, b: right.Name, comparisonType: StringComparison.Ordinal)
        && left.Visible == right.Visible
        && left.Mode == right.Mode;

    private static Fin<MutationReceipt> PerformModifyMetadata(RhinoBlocks owner, DefinitionRef refer, MetadataPatch patch, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _ in patch.IsEmpty
            ? Fin.Succ(value: unit)
            : CommitMetadata(table: owner.Document.InstanceDefinitions, snap: snap, patch: patch, key: key)
        let receipt = MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: snap.Name.Value).Document,
            diagnostics: patch.UserStrings.IsEmpty
                ? Seq<BlockDiagnostic>()
                : Seq<BlockDiagnostic>(new BlockDiagnostic.SilentUserStringMutation(Id: snap.Id)))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: receipt);

    private static Fin<MutationReceipt> PerformModifyGeometry(RhinoBlocks owner, DefinitionRef refer, Members source, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in snap.Live.Map(static live => live.Update.IsLinked).IfNone(noneValue: false)
            ? Fin.Fail<Unit>(error: key.InvalidInput())     // ModifyGeometry rejected on linked definitions (Dale Fugier policy)
            : Reify(doc: owner.Document, source: source, key: key)
                .Bind(provided => key.Confirm(success: owner.Document.InstanceDefinitions.ModifyGeometry(
                    idefIndex: idx,
                    newGeometry: provided.Geometry.AsEnumerable(),
                    newAttributes: provided.Attributes.AsEnumerable())))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<Unit> CommitMetadata(InstanceDefinitionTable table, Definition snap, MetadataPatch patch, Op key) =>
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: table.Modify(
            idefIndex: idx,
            newName: snap.Name.Value,
            newDescription: patch.Description.IfNone(snap.Description.IfNone(string.Empty)),
            newUrl: patch.Url.Map(static p => p.Value).IfNone(snap.Url.Map(static u => u.Value).IfNone(string.Empty)),
            newUrlTag: patch.UrlDescription.IfNone(snap.UrlDescription.IfNone(string.Empty)),
            quiet: true))
        from __ in patch.UserStrings.IsEmpty
            ? Fin.Succ(value: unit)
            : ApplyUserStrings(table: table, idx: idx, strings: patch.UserStrings, key: key)
        select unit;

    /// SetUserString does not fire InstanceDefinitionTableEvent.Modified — force invalidate
    /// SnapshotVault so subscribers see fresh snapshots.
    private static Fin<Unit> ApplyUserStrings(InstanceDefinitionTable table, int idx, HashMap<string, string> strings, Op key) =>
        Optional(table[idx]).ToFin(Fail: key.InvalidResult()).Map(live => {
            _ = strings.Iter((k, v) => live.SetUserString(key: k, value: v));
            _ = SnapshotVault.Invalidate(defId: live.Id);
            return unit;
        });

    // ---- [MANAGE] [MUTATION] -------------------------------------------------------------
    private static Fin<MutationReceipt> PerformRename(RhinoBlocks owner, DefinitionRef refer, DefinitionName newName, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.Modify(
            idefIndex: idx,
            newName: newName.Value,
            newDescription: snap.Description.IfNone(noneValue: string.Empty),
            quiet: true))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: newName.Value));

    private static Fin<MutationReceipt> PerformDelete(RhinoBlocks owner, DefinitionRef refer, DeletionPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.Delete(
            idefIndex: idx,
            deleteReferences: policy.DeleteReferences,
            quiet: policy.Quiet))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value));

    private static Fin<MutationReceipt> PerformUndelete(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in ResolveIncludingDeleted(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let snap = pair.Snap
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.Undelete(idefIndex: idx))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value));

    private static Fin<MutationReceipt> PerformUndoModify(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.UndoModify(idefIndex: idx))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<MutationReceipt> PerformPurge(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => PurgeOne(owner: owner, refer: r, key: key),
            _ => PurgeAllUnused(owner: owner),
        };

    private static Fin<MutationReceipt> PurgeOne(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.Purge(idefIndex: idx))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value));

    private static Fin<MutationReceipt> PurgeAllUnused(RhinoBlocks owner) {
        int purged = owner.Document.InstanceDefinitions.PurgeUnused();
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: $"<purged:{purged}>")),
            }));
    }

    private static Fin<MutationReceipt> PerformPurgeByPrefix(RhinoBlocks owner, string prefix, Op key) {
        Seq<DocumentResourceChange> changes = toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(d => d?.Name?.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase) ?? false)
            .Choose(d => owner.Document.InstanceDefinitions.Purge(idefIndex: d.Index)
                ? Some(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: $"<purged:{d.Index}>"))
                : Option<DocumentResourceChange>.None);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes }));
    }

    private static Fin<MutationReceipt> PerformCompact(RhinoBlocks owner, CompactPolicy policy, Op key) {
        owner.Document.InstanceDefinitions.Compact(ignoreUndoReferences: policy.IgnoreUndoReferences);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Empty);
    }

    // ---- [PLACE] [MUTATION] --------------------------------------------------------------
    private static Fin<MutationReceipt> PerformPlace(RhinoBlocks owner, DefinitionRef refer, Seq<Transform> at, Option<ObjectAttributes> attrs, Option<HistoryRecord> history, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from placements in at
            .Map(xform => AddInstance(table: owner.Document.Objects, index: idx, xform: xform, attrs: attrs, history: history, key: key))
            .TraverseM(identity).As()
        from _ in key.Confirm(success: !placements.IsEmpty)
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
            Created = placements,
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: snap.Name.Value)),
        });

    private static Fin<Guid> AddInstance(ObjectTable table, int index, Transform xform, Option<ObjectAttributes> attrs, Option<HistoryRecord> history, Op key) {
        ObjectAttributes effective = attrs.IfNone(static () => new ObjectAttributes());
        Guid id = history.Case switch {
            HistoryRecord rec => table.AddInstanceObject(
                instanceDefinitionIndex: index, instanceXform: xform,
                attributes: effective, history: rec, reference: false),
            _ => table.AddInstanceObject(instanceDefinitionIndex: index, instanceXform: xform, attributes: effective),
        };
        return id == Guid.Empty ? Fin.Fail<Guid>(error: key.InvalidResult()) : Fin.Succ(value: id);
    }

    private static Fin<MutationReceipt> PerformBatchPlace(RhinoBlocks owner, DefinitionRef refer, Seq<Placement> at, BatchPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from receipts in PlaceInBatch(doc: owner.Document, index: idx, at: at, policy: policy, key: key)
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
            Created = receipts,
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: snap.Name.Value)),
        });

    private static Fin<Seq<Guid>> PlaceInBatch(RhinoDoc doc, int index, Seq<Placement> at, BatchPolicy policy, Op key) {
        // BOUNDARY ADAPTER — RedrawEnabled must restore even when native placement fails.
        bool prior = doc.Views.RedrawEnabled;
        if (policy.SuppressRedraw) doc.Views.RedrawEnabled = false;
        try {
            return at.Map(p => AddInstance(table: doc.Objects, index: index, xform: p.Transform, attrs: p.Attrs, history: p.History, key: key))
                .TraverseM(identity).As();
        } finally {
            if (policy.SuppressRedraw) {
                doc.Views.RedrawEnabled = prior;
                doc.Views.Redraw();
            }
        }
    }

    private static Fin<MutationReceipt> PerformReplaceDefinition(RhinoBlocks owner, Guid instanceId, DefinitionRef newRef, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: newRef, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        let priorName = ResolveOriginalName(objects: owner.Document.Objects, instanceId: instanceId)
        from _ in key.Confirm(success: owner.Document.Objects.ReplaceInstanceObject(objectId: instanceId, instanceDefinitionIndex: idx))
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
            Replaced = Seq(instanceId),
            ResourceChanged = ReplaceResourceChanges(prior: priorName, next: snap.Name.Value),
        });

    private static Option<string> ResolveOriginalName(ObjectTable objects, Guid instanceId) =>
        AsInstance(objects: objects, instanceId: instanceId, key: Op.Of(name: nameof(ResolveOriginalName))).ToOption()
            .Bind(io => Optional(io.InstanceDefinition))
            .Bind(def => Optional(def.Name).Filter(static n => !string.IsNullOrWhiteSpace(value: n)));

    private static Seq<DocumentResourceChange> ReplaceResourceChanges(Option<string> prior, string next) =>
        prior.Case switch {
            string p when !string.Equals(a: p, b: next, comparisonType: StringComparison.OrdinalIgnoreCase) =>
                Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: p),
                    new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next)),
            _ => Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next)),
        };

    private static Fin<MutationReceipt> PerformTransformCopy(RhinoBlocks owner, Guid instanceId, Transform xform, TransformPolicy policy, Op key) {
        Guid id = owner.Document.Objects.Transform(objectId: instanceId, xform: xform, deleteOriginal: policy.DeleteOriginal);
        return id == Guid.Empty
            ? Fin.Fail<MutationReceipt>(error: key.InvalidResult())
            : Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                Created = Seq(id),
                Deleted = policy.DeleteOriginal ? Seq(instanceId) : Seq<Guid>(),
                Transformed = policy.DeleteOriginal ? Seq<Guid>() : Seq(id),
            }));
    }

    private static Fin<MutationReceipt> PerformTransformWithHistory(RhinoBlocks owner, Guid instanceId, Transform xform, Op key) {
        Guid id = owner.Document.Objects.TransformWithHistory(objectId: instanceId, xform: xform);
        return id == Guid.Empty
            ? Fin.Fail<MutationReceipt>(error: key.InvalidResult())
            : Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with { Created = Seq(id) }));
    }

    private static Fin<MutationReceipt> PerformExplodeIntoDocument(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = ExplodeNative(instance: instance, policy: policy)
        let expected = pieces.Length
        from produced in AddExplodedPieces(doc: owner.Document, pieces: pieces, key: key)
        from deleted in key.Confirm(success: owner.Document.Objects.Delete(obj: instance, quiet: true))
        let diagnostics = produced.Count == expected
            ? Seq<BlockDiagnostic>()
            : Seq<BlockDiagnostic>(new BlockDiagnostic.ExplodePartial(Requested: expected, Received: produced.Count))
        select MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                Created = produced,
                Deleted = Seq(instanceId),
            },
            diagnostics: diagnostics);

    private static Fin<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, RhinoObject[] pieces, Op key) =>
        toSeq(pieces)
            .Filter(static piece => piece?.Geometry is not null)
            .Map(piece => AddExplodedPiece(doc: doc, piece: piece, key: key))
            .TraverseM(identity).As();

    private static Fin<Guid> AddExplodedPiece(RhinoDoc doc, RhinoObject piece, Op key) {
        GeometryBase geometry = piece.Geometry.Duplicate();
        ObjectAttributes attributes = piece.Attributes.Duplicate();
        Guid id = doc.Objects.Add(geometry: geometry, attributes: attributes);
        return id == Guid.Empty ? Fin.Fail<Guid>(error: key.InvalidResult()) : Fin.Succ(value: id);
    }

    // ---- [LINK] --------------------------------------------------------------------------
    private static Fin<MutationReceipt> PerformCreateArchiveLinks(RhinoBlocks owner, Seq<FileEndpoint> sources, LinkCreatePolicy policy, Op key) =>
        from admitted in policy.Admit(key: key)
        from changes in sources
            .Map(endpoint => key.Catch(() => CreateOneLink(owner: owner, endpoint: endpoint, policy: admitted, key: key)))
            .TraverseM(identity).As()
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes });

    private static Fin<DocumentResourceChange> CreateOneLink(RhinoBlocks owner, FileEndpoint endpoint, LinkCreatePolicy policy, Op key) =>
        from source in endpoint.Input(op: key)
        from change in WithReference(source: source, key: key, use: reference =>
            AddLinkedDefinition(owner: owner, source: source, reference: reference, policy: policy, key: key))
        select change;

    private static Fin<T> WithReference<T>(FileEndpoint source, Op key, Func<FileReference, Fin<T>> use) {
        // BOUNDARY ADAPTER — FileReference owns native source metadata and must be disposed after the table call.
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: source.Path);
        return use(arg: reference);
    }

    private static Fin<DocumentResourceChange> AddLinkedDefinition(RhinoBlocks owner, FileEndpoint source, FileReference reference, LinkCreatePolicy policy, Op key) {
        // BOUNDARY ADAPTER — empty definition creation must rollback when link attachment fails.
        string filename = source.Path;
        string name = owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: IOPath.GetFileNameWithoutExtension(path: filename));
        int idx = owner.Document.InstanceDefinitions.Add(name: name, description: string.Empty, basePoint: Point3d.Origin, geometry: [], attributes: []);
        if (idx < 0) return Fin.Fail<DocumentResourceChange>(error: key.InvalidResult());
        bool modified = owner.Document.InstanceDefinitions.ModifySourceArchive(
            idefIndex: idx,
            sourceArchive: reference,
            updateType: policy.EffectiveUpdate.Native,
            quiet: policy.EffectiveReload.Quiet);
        if (!modified) return RollbackLinkedDefinition(owner: owner, idx: idx, key: key);
        InstanceDefinition live = owner.Document.InstanceDefinitions[idx];
        live.LayerStyle = policy.EffectiveLayer.Native;
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: filename));
    }

    private static Fin<DocumentResourceChange> RollbackLinkedDefinition(RhinoBlocks owner, int idx, Op key) {
        _ = owner.Document.InstanceDefinitions.Delete(idefIndex: idx, deleteReferences: true, quiet: true);
        _ = owner.Document.InstanceDefinitions.Purge(idefIndex: idx);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Fail<DocumentResourceChange>(error: key.InvalidResult());
    }

    private static Fin<MutationReceipt> PerformUpdateSourceArchive(RhinoBlocks owner, DefinitionRef refer, FileEndpoint source, UpdatePolicy policy, LinkReloadPolicy reload, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from endpoint in source.Input(op: key)
        from _ in ApplySourceArchive(owner: owner, index: idx, source: endpoint, policy: policy, quiet: reload.Quiet, key: key)
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<Unit> ApplySourceArchive(RhinoBlocks owner, int index, FileEndpoint source, UpdatePolicy policy, bool quiet, Op key) =>
        from _ in policy.IsLinked ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: key.InvalidInput())
        from changed in WithReference(source: source, key: key, use: reference =>
            key.Confirm(success: owner.Document.InstanceDefinitions.ModifySourceArchive(
                idefIndex: index, sourceArchive: reference, updateType: policy.Native, quiet: quiet)))
        select changed;

    private static Fin<MutationReceipt> PerformRefreshLinks(RhinoBlocks owner, Option<Seq<string>> archives, LinkRefreshPolicy policy, Op key) =>
        RefreshSources(doc: owner.Document, archives: archives, refs: Option<Seq<DefinitionRef>>.None, policy: policy, key: key)
            .Map(refreshed => MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                ResourceChanged = refreshed.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
            }));

    private static Fin<MutationReceipt> PerformReloadFromFile(RhinoBlocks owner, DefinitionRef refer, FileEndpoint source, LinkReloadPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from endpoint in source.Input(op: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
            idefIndex: idx, filename: endpoint.Path, updateNestedLinks: policy.UpdateNestedLinks, quiet: policy.Quiet))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<MutationReceipt> PerformDetachArchive(RhinoBlocks owner, DefinitionRef refer, LinkReloadPolicy policy, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.DestroySourceArchive(definition: pair.Live, quiet: policy.Quiet))
        select InvalidateWith(doc: owner.Document, defId: pair.Snap.Id.Value, value: MutationReceipt.Named(name: pair.Snap.Name.Value));

    private static Fin<MutationReceipt> PerformFlattenLinked(RhinoBlocks owner, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs, Op key) =>
        SelectLinked(table: owner.Document.InstanceDefinitions, archives: archives, refs: refs)
            .TraverseM(definition => DetachLinkedDefinition(owner: owner, definition: definition, key: key))
            .As()
            .Map(changes => MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes }));

    private static Fin<DocumentResourceChange> DetachLinkedDefinition(RhinoBlocks owner, InstanceDefinition definition, Op key) =>
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.DestroySourceArchive(definition: definition, quiet: true))
        let invalidated = InvalidateDefinition(doc: owner.Document, defId: definition.Id)
        select new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: definition.Name ?? string.Empty);

    private static Fin<Seq<InstanceDefinition>> RefreshSources(RhinoDoc doc, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs, LinkRefreshPolicy policy, Op key) =>
        SelectLinked(table: doc.InstanceDefinitions, archives: archives, refs: refs, policy: policy)
            .TraverseM(definition =>
                key.Confirm(success: doc.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                    .Map(_ => InvalidateWith(doc: doc, defId: definition.Id, value: definition)))
            .As();

    private static Seq<InstanceDefinition> SelectLinked(InstanceDefinitionTable table, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs, LinkRefreshPolicy? policy = null) {
        Seq<InstanceDefinition> linked = toSeq(table)
            .Filter(static d => d.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded);
        Seq<InstanceDefinition> byArchive = archives.Case switch {
            Seq<string> filter => linked.Filter(d => filter.Exists(p =>
                string.Equals(a: p, b: d.SourceArchive ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase))),
            _ => linked,
        };
        Seq<InstanceDefinition> byRef = refs.Case switch {
            Seq<DefinitionRef> filter => byArchive.Filter(d => filter.Exists(r => r.Matches(definition: d))),
            _ => byArchive,
        };
        return (policy ?? LinkRefreshPolicy.All).SkipUpToDate
            ? byRef.Filter(static d => d.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate)
            : byRef;
    }

    // ---- [OBSERVE] [SIDE-EFFECT] ---------------------------------------------------------
    private static Fin<MutationReceipt> PerformExport(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from endpoint in target.Output(op: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.Export(idefIndex: idx, filename: endpoint.Path))
        select MutationReceipt.Named(name: snap.Name.Value);

    /// `_-ExportBlockAttributes` (Rhino 9) via RunScript — no managed API exists.
    private static Fin<MutationReceipt> PerformExportAttributes(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from endpoint in target.Output(op: key)
        let script = CommandScript(command: "_-ExportBlockAttributes", args: Seq(("Definition", snap.Name.Value), ("File", endpoint.Path)))
        from _ in key.Confirm(success: RhinoApp.RunScript(documentSerialNumber: owner.Document.RuntimeSerialNumber, script: script, echo: false))
        select MutationReceipt.Named(name: snap.Name.Value);

    /// Snapshot save via RunScript; SnapshotName VO strips forbidden chars at admission.
    private static Fin<MutationReceipt> PerformSnapshotBlock(RhinoBlocks owner, DefinitionRef refer, SnapshotName name, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let script = CommandScript(command: "_-Snapshot _Save", args: Seq(("Name", name.Value)))
        from _ in key.Confirm(success: RhinoApp.RunScript(documentSerialNumber: owner.Document.RuntimeSerialNumber, script: script, echo: false))
        select MutationReceipt.Named(name: snap.Name.Value);

    private static string CommandScript(string command, Seq<(string Name, string Value)> args) =>
        string.Join(separator: " ", values: (Seq(command) + args.Map(static arg => $"_{arg.Name} {CommandToken(value: arg.Value)}") + Seq("_Enter")).AsIterable());

    private static string CommandToken(string value) =>
        $"\"{(value ?? string.Empty).Replace(oldValue: "\"", newValue: "\"\"", comparisonType: StringComparison.Ordinal).Replace(oldValue: "\r", newValue: " ", comparisonType: StringComparison.Ordinal).Replace(oldValue: "\n", newValue: " ", comparisonType: StringComparison.Ordinal)}\"";

    // ---- [MANAGE] [QUERY] ----------------------------------------------------------------
    private static Fin<BlockResult> PerformLookup(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
            .Map(static snap => (BlockResult)new BlockResult.Snapshot(Value: snap));

    private static Fin<BlockResult> PerformAllocateName(RhinoBlocks owner, Option<string> seed, Op key) {
        InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
        string raw = seed.Case switch {
            string s => table.GetUnusedInstanceDefinitionName(root: s),
            _ => table.GetUnusedInstanceDefinitionName(),
        };
        return DefinitionName.From(value: raw, key: key).Map(static n => (BlockResult)new BlockResult.Name(Value: n));
    }

    private static Fin<BlockResult> PerformSnapshot(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: r, key: key)
                .Map(static snap => (BlockResult)new BlockResult.Snapshot(Value: snap)),
            _ => SnapshotAll(table: owner.Document.InstanceDefinitions, key: key),
        };

    private static Fin<BlockResult> PerformAudit(RhinoBlocks owner, Op key) =>
        SnapshotAll(table: owner.Document.InstanceDefinitions, key: key);

    private static Fin<BlockResult> SnapshotAll(InstanceDefinitionTable table, Op key) =>
        toSeq(table.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Map(d => Definition.From(active: d, key: key))
            .TraverseM(identity).As()
            .Map(static seq => (BlockResult)new BlockResult.Snapshots(Values: seq));

    // ---- [PLACE] [QUERY] -----------------------------------------------------------------
    private static Fin<BlockResult> PerformExplodeInspect(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = ExplodeNative(instance: instance, policy: policy)
        from defId in DefinitionId.From(value: instance.InstanceDefinition?.Id ?? Guid.Empty, key: key)
        let members = toSeq(pieces)
            .Filter(static p => p is not null)
            .Map(p => new Member(DefId: defId, MemberId: p.Id, Attrs: Optional(p.Attributes)))
        select (BlockResult)new BlockResult.MembersResult(Values: members);

    private static RhinoObject[] ExplodeNative(InstanceObject instance, ExplodePolicy policy) {
        (bool skips, Guid viewport) = policy.Resolve();
        instance.Explode(
            skipHiddenPieces: skips, viewportId: viewport, explodeNestedInstances: true,
            pieces: out RhinoObject[] pieces, pieceAttributes: out ObjectAttributes[] _, pieceTransforms: out Transform[] _);
        return pieces ?? [];
    }

    private static Fin<BlockResult> PerformUseSubObject(RhinoBlocks owner, Guid instanceId, ComponentIndex component, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from valid in component is { IsSet: true, ComponentIndexType: ComponentIndexType.InstanceDefinitionPart }
            ? Fin.Succ(value: component)
            : Fin.Fail<ComponentIndex>(error: key.InvalidInput())
        from sub in Optional(instance.SubObjectFromComponentIndex(ci: valid)).ToFin(Fail: key.InvalidInput())
        from defId in DefinitionId.From(value: instance.InstanceDefinition?.Id ?? Guid.Empty, key: key)
            // Compose InstanceXform so sub-object geometry returns in world space (consistent with Flatten).
        select (BlockResult)new BlockResult.Pieces(Values: Seq(
            new FlatPiece(
                Geometry: sub.Geometry,
                Composed: instance.InstanceXform,
                Path: [defId])));

    // ---- [GRAPH] -------------------------------------------------------------------------
    private static Fin<BlockResult> PerformGraphMembers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let members = toSeq(pair.Live.GetObjects())
            .Filter(static obj => obj is not null)
            .Map(obj => new Member(DefId: pair.Snap.Id, MemberId: obj.Id, Attrs: Optional(obj.Attributes)))
        select (BlockResult)new BlockResult.MembersResult(Values: members);

    private static Fin<BlockResult> PerformGraphInserts(RhinoBlocks owner, DefinitionRef refer, ReferenceScope scope, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from inserts in toSeq(pair.Live.GetReferences(wheretoLook: scope.Native))
            .Filter(static inst => inst is not null)
            .Map(static inst => Insert.From(instance: inst))
            .TraverseM(identity).As()
        select (BlockResult)new BlockResult.Inserts(Values: inserts);

    private static Fin<BlockResult> PerformGraphContainers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from containers in toSeq(pair.Live.GetContainers())
            .Filter(static c => c is not null)
            .Map(c => DefinitionId.From(value: c.Id, key: key))
            .TraverseM(identity).As()
        select (BlockResult)new BlockResult.Definitions(Values: containers);

    private static Fin<BlockResult> PerformSelectedPart(RhinoBlocks owner, ObjRef refer, Op key) =>
        from instance in Optional(refer.Object() as InstanceObject).ToFin(Fail: key.InvalidInput())
        from definition in Optional(instance.InstanceDefinition).ToFin(Fail: key.InvalidResult())
        from member in Optional(refer.InstanceDefinitionPart()).Filter(static part => part.Attributes.IsInstanceDefinitionObject).ToFin(Fail: key.InvalidResult())
        from defId in DefinitionId.From(value: definition.Id, key: key)
        select (BlockResult)new BlockResult.MembersResult(Values: Seq(new Member(DefId: defId, MemberId: member.Id, Attrs: Optional(member.Attributes))));

    private static Fin<BlockResult> PerformDependencyAudit(RhinoBlocks owner, Op key) {
        Seq<InstanceDefinition> definitions = toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null);
        Seq<Graph.Node> nodes = definitions.Choose(d => ProjectNode(definition: d, key: key));
        Seq<Graph.Edge> edges = definitions.Bind(d => ProjectEdges(definition: d, key: key));
        Graph graph = new(Nodes: [.. nodes], Edges: [.. edges]);
        return Fin.Succ(value: (BlockResult)new BlockResult.Graphs(Value: graph));
    }

    private static Option<Graph.Node> ProjectNode(InstanceDefinition definition, Op key) =>
        from id in DefinitionId.From(value: definition.Id, key: key).ToOption()
        from name in DefinitionName.From(value: definition.Name ?? string.Empty, key: key).ToOption()
        select new Graph.Node(
            Id: id,
            Name: name,
            Members: [.. definition.GetObjectIds() ?? []],
            Update: UpdatePolicy.FromNative(native: definition.UpdateType),
            Archive: ArchiveStatus.FromNative(native: definition.ArchiveFileStatus),
            Source: Definition.NonBlank(value: definition.SourceArchive).Bind(v => ArchivePath.From(value: v, key: key).ToOption()));

    private static Seq<Graph.Edge> ProjectEdges(InstanceDefinition definition, Op key) =>
        DefinitionId.From(value: definition.Id, key: key).ToOption() switch {
            { IsSome: true, Case: DefinitionId from } => MemberEdges(definition: definition, from: from)
                + ArchiveEdges(definition: definition, from: from, key: key)
                + InsertEdges(definition: definition, from: from),
            _ => Seq<Graph.Edge>(),
        };

    private static Seq<Graph.Edge> MemberEdges(InstanceDefinition definition, DefinitionId from) =>
        toSeq(definition.GetObjectIds() ?? [])
            .Map(id => new Graph.Edge(From: from, Kind: BlockEdgeKind.Member, To: new EdgeTarget.ObjectId(Id: id)));

    private static Seq<Graph.Edge> ArchiveEdges(InstanceDefinition definition, DefinitionId from, Op key) =>
        string.IsNullOrWhiteSpace(value: definition.SourceArchive)
            ? Seq<Graph.Edge>()
            : ArchivePath.From(value: definition.SourceArchive, key: key).ToOption() switch {
                { IsSome: true, Case: ArchivePath path } => Seq(new Graph.Edge(From: from, Kind: BlockEdgeKind.LinkedArchive, To: new EdgeTarget.ArchiveTarget(Path: path))),
                _ => Seq<Graph.Edge>(),
            };

    private static Seq<Graph.Edge> InsertEdges(InstanceDefinition definition, DefinitionId from) =>
        toSeq(definition.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native) ?? [])
            .Filter(static instance => instance is not null)
            .Map(instance => new Graph.Edge(From: from, Kind: BlockEdgeKind.InstanceInsert, To: new EdgeTarget.ObjectId(Id: instance.Id)));

    private static Fin<BlockResult> PerformLinkedHealth(RhinoBlocks owner, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs, Op key) =>
        SelectLinked(table: owner.Document.InstanceDefinitions, archives: archives, refs: refs)
            .Map(definition => ProjectLinkedHealth(definition: definition, key: key))
            .TraverseM(identity).As()
            .Map(items => (BlockResult)new BlockResult.Refresh(Value: new RefreshPlan(Items: items)));

    private static Fin<LinkedHealth> ProjectLinkedHealth(InstanceDefinition definition, Op key) =>
        from id in DefinitionId.From(value: definition.Id, key: key)
        from name in DefinitionName.From(value: definition.Name ?? string.Empty, key: key)
        let update = UpdatePolicy.FromNative(native: definition.UpdateType)
        let layer = LayerStyle.FromNative(native: definition.LayerStyle)
        let archive = ArchiveStatus.FromNative(native: definition.ArchiveFileStatus)
        let source = Definition.NonBlank(value: definition.SourceArchive).Bind(path => ArchivePath.From(value: path, key: key).ToOption())
        select new LinkedHealth(
            Id: id,
            Name: name,
            Source: source,
            Update: update,
            Layer: layer,
            Archive: archive,
            SkipNestedLinked: definition.SkipNestedLinkedDefinitions,
            Diagnostics: LinkedDiagnostics(id: id, update: update, layer: layer, source: source, archive: archive));

    private static Seq<BlockDiagnostic> LinkedDiagnostics(DefinitionId id, UpdatePolicy update, LayerStyle layer, Option<ArchivePath> source, ArchiveStatus archive) =>
        (!layer.AppliesTo(policy: update)
            ? Seq<BlockDiagnostic>(new BlockDiagnostic.InvalidLayerStyle(Id: id, Update: update, Layer: layer))
            : Seq<BlockDiagnostic>())
        + (source.IsNone || archive.IsBroken
            ? Seq<BlockDiagnostic>(new BlockDiagnostic.LinkedArchiveIssue(Id: id, Status: archive))
            : Seq<BlockDiagnostic>());

    private static Fin<BlockResult> PerformDependsOn(RhinoBlocks owner, DefinitionRef refer, DependencyTarget target, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        select (BlockResult)new BlockResult.Probed(Value: target.ProbeOn(live: pair.Live));

    private static Fin<BlockResult> PerformFlatten(RhinoBlocks owner, Guid instanceId, FlattenPolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from admitted in policy.Admit(key: key)
        let pieces = Walk(instance: instance, parent: Transform.Identity, path: [], depth: 0, max: admitted.MaxDepth, key: key)
        select (BlockResult)new BlockResult.Pieces(Values: pieces);

    private static Seq<FlatPiece> Walk(InstanceObject instance, Transform parent, ImmutableArray<DefinitionId> path, int depth, int max, Op key) =>
        (depth >= max, instance.InstanceDefinition) switch {
            (true, _) or (_, null) => Seq<FlatPiece>(),
            (_, InstanceDefinition def) => WalkChildren(
                def: def,
                composed: parent * instance.InstanceXform,
                nextPath: AppendPath(path: path, id: def.Id, key: key),
                depth: depth + 1, max: max, key: key),
        };

    private static ImmutableArray<DefinitionId> AppendPath(ImmutableArray<DefinitionId> path, Guid id, Op key) =>
        DefinitionId.From(value: id, key: key).ToOption() switch {
            { IsSome: true, Case: DefinitionId defId } => path.Add(item: defId),
            _ => path,
        };

    private static Seq<FlatPiece> WalkChildren(InstanceDefinition def, Transform composed, ImmutableArray<DefinitionId> nextPath, int depth, int max, Op key) =>
        toSeq(def.GetObjects()).Filter(static o => o is not null).Bind(member => member switch {
            InstanceObject nested => Walk(instance: nested, parent: composed, path: nextPath, depth: depth, max: max, key: key),
            RhinoObject leaf when leaf.Geometry is GeometryBase g => Seq(new FlatPiece(Geometry: g, Composed: composed, Path: nextPath)),
            _ => Seq<FlatPiece>(),
        });

    // ---- [OBSERVE] [QUERY] ---------------------------------------------------------------
    private static Fin<BlockResult> PerformPreview(RhinoBlocks owner, DefinitionRef refer, PreviewSpec spec, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from admitted in spec.Admit(key: key)
        from handle in owner.AcquirePreview(definition: pair.Live, spec: admitted, key: key)
        select (BlockResult)new BlockResult.Preview(Handle: handle);

    private static Fin<BlockResult> PerformTextFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let fields = DefinitionFieldPairs(snap: pair.Snap, live: pair.Live)
            .Fold(HashMap<string, string>(), static (m, p) => m.AddOrUpdate(key: p.Key, value: p.Value))
        select (BlockResult)new BlockResult.Texts(Fields: fields);

    private static Seq<(string Key, string Value)> DefinitionFieldPairs(Definition snap, InstanceDefinition live) =>
        Seq(
            ("BlockName", snap.Name.Value),
            ("BlockInstanceCount", (live.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native)?.Length ?? 0).ToString(provider: System.Globalization.CultureInfo.InvariantCulture)),
            ("BlockDescription", snap.Description.IfNone(noneValue: string.Empty)));

    private static Fin<BlockResult> PerformAttributeFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
        select (BlockResult)new BlockResult.Attributes(Values: fields);

    // ---- [WATCH] -------------------------------------------------------------------------
    /// All four FileSystemWatcher events wired (matches McNeel RhinoFileWatcher set) so atomic-rename
    /// and delete-replace editor saves aren't missed; NotifyFilter narrows to suppress dir churn.
    internal static Fin<Subscription> AttachWatcher(RhinoBlocks owner, ArchivePath path, WatchPolicy policy) =>
        Op.Of(name: nameof(AttachWatcher)).Catch(() => {
            string dir = IOPath.GetDirectoryName(path: path.Value) ?? string.Empty;
            string filter = IOPath.GetFileName(path: path.Value);
            FileSystemWatcher watcher = new(path: dir, filter: filter) {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };
            try {
                // BOUNDARY ADAPTER — FileSystemWatcher delegate lifetime is owned by Subscription disposal.
                WatchContext ctx = new(
                    Owner: owner, Path: path, Policy: policy,
                    LastFired: Atom(value: DateTimeOffset.MinValue),
                    Watcher: watcher);
                void OnChange(object sender, FileSystemEventArgs args) => OnWatchChanged(ctx: ctx);
                void OnRename(object sender, RenamedEventArgs args) => OnWatchChanged(ctx: ctx);
                watcher.Changed += OnChange;
                watcher.Created += OnChange;
                watcher.Deleted += OnChange;
                watcher.Renamed += OnRename;
                watcher.EnableRaisingEvents = true;
                return Fin.Succ(value: Subscription.Of(detach: ctx.Watcher.Dispose));
            } catch {
                watcher.Dispose();
                throw;
            }
        });

    private static void OnWatchChanged(WatchContext ctx) {
        DateTimeOffset now = TimeProvider.System.GetUtcNow();
        if (now - ctx.LastFired.Value < ctx.Policy.Debounce) return;
        _ = ctx.LastFired.Swap(f: _ => now);
        _ = IdlePump.Enqueue(document: ctx.Owner.Document, work: doc => RefreshArchive(doc: doc, path: ctx.Path));
    }

    private static Fin<DocumentReceipt> RefreshArchive(RhinoDoc doc, ArchivePath path) =>
        Op.Of(name: nameof(RefreshArchive)).Catch(() =>
            RefreshSources(doc: doc, archives: Some(Seq(path.Value)), refs: Option<Seq<DefinitionRef>>.None, policy: LinkRefreshPolicy.All, key: Op.Of(name: nameof(RefreshArchive)))
                .Map(_ => DocumentReceipt.Empty with {
                    ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: path.Value)),
                }));

    private sealed record WatchContext(
        RhinoBlocks Owner,
        ArchivePath Path,
        WatchPolicy Policy,
        Atom<DateTimeOffset> LastFired,
        FileSystemWatcher Watcher);

    // ---- [CONTENT INDEX] [LIFECYCLE HOOKS] -----------------------------------------------
    internal static Unit InvalidateContentIndex(uint serial) => ContentIndex.EvictDoc(serial: serial);
}

// --- [COMPOSITION] [CONTENT INDEX] --------------------------------------------------------
/// Per-doc content-hash → DefinitionId index. Lazy on first AddOrReuse; flushed by EventBridge
/// on every non-sort table event. Eliminates the O(N) full-table scan per AddOrReuse call.
internal static class ContentIndex {
    private static readonly Atom<HashMap<uint, HashMap<ulong, Seq<DefinitionId>>>> byDoc =
        Atom(value: HashMap<uint, HashMap<ulong, Seq<DefinitionId>>>());

    internal static Seq<DefinitionId> Find(RhinoDoc doc, BlockContentHash hash) {
        uint serial = doc.RuntimeSerialNumber;
        HashMap<ulong, Seq<DefinitionId>> idx = Ensure(doc: doc, serial: serial);
        return idx.Find(key: hash.Value).IfNone(noneValue: Seq<DefinitionId>());
    }

    /// Merges incrementally on hit; Build runs only on cold doc to avoid O(N) per-call rehash.
    internal static Unit RegisterIfMissing(RhinoDoc doc, BlockContentHash hash, DefinitionId defId) {
        uint serial = doc.RuntimeSerialNumber;
        _ = byDoc.Swap(f: m => m.Find(key: serial) switch {
            { IsSome: true, Case: HashMap<ulong, Seq<DefinitionId>> existing } =>
                m.AddOrUpdate(key: serial, value: existing.AddOrUpdate(
                    key: hash.Value,
                    value: existing.Find(key: hash.Value).IfNone(noneValue: Seq<DefinitionId>()).Add(value: defId).Distinct())),
            _ =>
                m.AddOrUpdate(key: serial, value: Build(doc: doc).AddOrUpdate(key: hash.Value, value: Seq(defId))),
        });
        return unit;
    }

    internal static Unit EvictDoc(uint serial) {
        _ = byDoc.Swap(f: m => m.Remove(key: serial));
        return unit;
    }

    private static HashMap<ulong, Seq<DefinitionId>> Ensure(RhinoDoc doc, uint serial) {
        HashMap<ulong, Seq<DefinitionId>> idx = HashMap<ulong, Seq<DefinitionId>>();
        _ = byDoc.Swap(f: m => m.Find(key: serial) switch {
            { IsSome: true, Case: HashMap<ulong, Seq<DefinitionId>> existing } => CaptureIndex(map: m, index: existing, into: ref idx),
            _ => CaptureIndex(map: m.AddOrUpdate(key: serial, value: Build(doc: doc)), index: Build(doc: doc), into: ref idx),
        });
        return idx;
    }

    private static HashMap<uint, HashMap<ulong, Seq<DefinitionId>>> CaptureIndex(
        HashMap<uint, HashMap<ulong, Seq<DefinitionId>>> map,
        HashMap<ulong, Seq<DefinitionId>> index,
        ref HashMap<ulong, Seq<DefinitionId>> into) {
        into = index;
        return map;
    }

    private static HashMap<ulong, Seq<DefinitionId>> Build(RhinoDoc doc) =>
        toSeq(doc.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Choose(d => HashEntry(doc: doc, active: d))
            .Fold(HashMap<ulong, Seq<DefinitionId>>(), static (m, kv) => m.AddOrUpdate(
                key: kv.Key,
                value: m.Find(key: kv.Key).IfNone(noneValue: Seq<DefinitionId>()).Add(value: kv.Value).Distinct()));

    private static Option<(ulong Key, DefinitionId Value)> HashEntry(RhinoDoc doc, InstanceDefinition active) =>
        from id in DefinitionId.From(value: active.Id).ToOption()
        from provided in Operations.ReifyDocumentMembers(doc: doc, sources: toSeq(active.GetObjectIds() ?? []), key: Op.Of(name: nameof(ContentIndex))).ToOption()
        select (Key: BlockContentHash.Of(members: provided).Value, Value: id);
}
