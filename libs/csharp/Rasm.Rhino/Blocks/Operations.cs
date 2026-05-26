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
    public sealed record Compact(bool IgnoreUndoReferences = false) : BlockMutation;

    // [PLACE]
    public sealed record Place(DefinitionRef Ref, Seq<Transform> At, Option<ObjectAttributes> Attrs, Option<HistoryRecord> History = default) : BlockMutation;
    public sealed record BatchPlace(DefinitionRef Ref, Seq<Placement> At, BatchPolicy Policy) : BlockMutation;
    public sealed record ReplaceDefinition(Guid InstanceId, DefinitionRef NewRef) : BlockMutation;
    public sealed record TransformCopy(Guid InstanceId, Transform Xform, bool DeleteOriginal) : BlockMutation;
    public sealed record TransformWithHistory(Guid InstanceId, Transform Xform) : BlockMutation;
    public sealed record ExplodeIntoDocument(Guid InstanceId, ExplodePolicy Policy) : BlockMutation;

    // [LINK]
    public sealed record CreateArchiveLinks(Seq<FileEndpoint> Sources) : BlockMutation;
    public sealed record UpdateSourceArchive(DefinitionRef Ref, FileEndpoint Source, UpdatePolicy Policy, bool Quiet = true) : BlockMutation;
    public sealed record RefreshLinks(Option<Seq<string>> Archives, bool SkipUpToDate = false) : BlockMutation;
    public sealed record ReloadFromFile(DefinitionRef Ref, FileEndpoint Source, bool UpdateNestedLinks = true, bool Quiet = true) : BlockMutation;
    public sealed record DetachArchive(DefinitionRef Ref, bool Quiet = true) : BlockMutation;
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
            compact: static (o, x) => RunMut(name: nameof(BlockMutation.Compact), body: k => PerformCompact(owner: o, ignoreUndo: x.IgnoreUndoReferences, key: k)),
            place: static (o, x) => RunMut(name: nameof(BlockMutation.Place), body: k => PerformPlace(owner: o, refer: x.Ref, at: x.At, attrs: x.Attrs, history: x.History, key: k)),
            batchPlace: static (o, x) => RunMut(name: nameof(BlockMutation.BatchPlace), body: k => PerformBatchPlace(owner: o, refer: x.Ref, at: x.At, policy: x.Policy, key: k)),
            replaceDefinition: static (o, x) => RunMut(name: nameof(BlockMutation.ReplaceDefinition), body: k => PerformReplaceDefinition(owner: o, instanceId: x.InstanceId, newRef: x.NewRef, key: k)),
            transformCopy: static (o, x) => RunMut(name: nameof(BlockMutation.TransformCopy), body: k => PerformTransformCopy(owner: o, instanceId: x.InstanceId, xform: x.Xform, deleteOriginal: x.DeleteOriginal, key: k)),
            transformWithHistory: static (o, x) => RunMut(name: nameof(BlockMutation.TransformWithHistory), body: k => PerformTransformWithHistory(owner: o, instanceId: x.InstanceId, xform: x.Xform, key: k)),
            explodeIntoDocument: static (o, x) => RunMut(name: nameof(BlockMutation.ExplodeIntoDocument), body: k => PerformExplodeIntoDocument(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            createArchiveLinks: static (o, x) => RunMut(name: nameof(BlockMutation.CreateArchiveLinks), body: k => PerformCreateArchiveLinks(owner: o, sources: x.Sources, key: k)),
            updateSourceArchive: static (o, x) => RunMut(name: nameof(BlockMutation.UpdateSourceArchive), body: k => PerformUpdateSourceArchive(owner: o, refer: x.Ref, source: x.Source, policy: x.Policy, quiet: x.Quiet, key: k)),
            refreshLinks: static (o, x) => RunMut(name: nameof(BlockMutation.RefreshLinks), body: k => PerformRefreshLinks(owner: o, archives: x.Archives, skipUpToDate: x.SkipUpToDate, key: k)),
            reloadFromFile: static (o, x) => RunMut(name: nameof(BlockMutation.ReloadFromFile), body: k => PerformReloadFromFile(owner: o, refer: x.Ref, source: x.Source, nested: x.UpdateNestedLinks, quiet: x.Quiet, key: k)),
            detachArchive: static (o, x) => RunMut(name: nameof(BlockMutation.DetachArchive), body: k => PerformDetachArchive(owner: o, refer: x.Ref, quiet: x.Quiet, key: k)),
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
            .Bind(d => Definition.From(active: d, key: key).Map(snap => (Snap: snap, Live: d)));

    internal static Fin<Definition> ResolveSnap(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        Resolve(table: table, refer: refer, key: key).Map(static p => p.Snap);

    private static Fin<InstanceDefinition> FindLive(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        refer switch {
            DefinitionRef.ById r =>
                Optional(table.Find(instanceId: r.Id.Value, ignoreDeletedInstanceDefinitions: true)).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByIndex r when r.Index.Value >= 0 && r.Index.Value < table.Count =>
                Optional(table[r.Index.Value]).ToFin(Fail: key.InvalidInput()),
            DefinitionRef.ByName r =>
                Optional(table.Find(instanceDefinitionName: r.Name.Value)).ToFin(Fail: key.InvalidInput()),
            _ => Fin.Fail<InstanceDefinition>(error: key.InvalidInput()),
        };

    /// Archive-only Definitions have Index = None; mutation arms require a live index.
    private static Fin<int> RequireIndex(Definition snap, Op key) =>
        snap.Index.Case switch {
            DefinitionIndex i => Fin.Succ(value: i.Value),
            _ => Fin.Fail<int>(error: key.InvalidInput()),
        };

    /// Single source of truth for `FindId(id) as InstanceObject` — collapses the cast +
    /// null-check pattern that appeared at 5 call sites (Explode/Replace/SubObject/Flatten/Watch).
    private static Fin<InstanceObject> AsInstance(ObjectTable objects, Guid instanceId, Op key) =>
        Optional(objects.FindId(id: instanceId) as InstanceObject).ToFin(Fail: key.InvalidInput());

    // ---- [AUTHOR] ------------------------------------------------------------------------
    private static Fin<MutationReceipt> PerformAuthor(RhinoBlocks owner, AuthorSpec spec, Members source, ConflictPolicy conflict, Op key) =>
        ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(spec.Name), key: key).ToOption() switch {
            { IsSome: true, Case: Definition existing } => ResolveConflict(owner: owner, existing: existing, spec: spec, source: source, conflict: conflict, key: key),
            _ => MaterializeAndAdd(owner: owner, spec: spec, source: source, key: key),
        };

    private static Fin<MutationReceipt> MaterializeAndAdd(RhinoBlocks owner, AuthorSpec spec, Members source, Op key) =>
        Reify(doc: owner.Document, source: source, key: key)
            .Bind(provided => AddNew(owner: owner, spec: spec, members: provided, key: key));

    /// Lazy materialization — Skip never reifies, never duplicates.
    private static Fin<Members.Provided> Reify(RhinoDoc doc, Members source, Op key) =>
        source switch {
            Members.Provided p => Fin.Succ(value: p),
            Members.FromDocument fd => ReifyFromDocument(doc: doc, sources: fd.Sources, key: key),
            _ => Fin.Fail<Members.Provided>(error: key.InvalidInput()),
        };

    private static Fin<Members.Provided> ReifyFromDocument(RhinoDoc doc, Seq<Guid> sources, Op key) =>
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
        (conflict, owner, existing, spec, source, key) switch {
            var s when s.conflict == ConflictPolicy.Replace =>
                Reify(doc: s.owner.Document, source: s.source, key: s.key)
                    .Bind(provided => ReplaceGeometry(owner: s.owner, existing: s.existing, spec: s.spec, members: provided, key: s.key)),
            var s when s.conflict == ConflictPolicy.Skip =>
                Fin.Succ(value: MutationReceipt.Of(
                    receipt: DocumentReceipt.Empty,
                    diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.DuplicateName(Name: s.existing.Name, Existing: s.existing.Id)))),
            var s when s.conflict == ConflictPolicy.Rename =>
                Reify(doc: s.owner.Document, source: s.source, key: s.key)
                    .Bind(provided => AddNew(
                        owner: s.owner,
                        spec: s.spec with { Name = AllocateRename(table: s.owner.Document.InstanceDefinitions, seed: s.spec.Name) },
                        members: provided,
                        key: s.key)),
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
                >= 0 => spec.Metadata.UserStrings.IsEmpty
                    ? Fin.Succ(value: MutationReceipt.Named(name: spec.Name.Value))
                    : ApplyUserStringsAt(table: owner.Document.InstanceDefinitions, name: spec.Name, strings: spec.Metadata.UserStrings, key: key)
                        .Bind(_ => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(spec.Name), key: key))
                        .Map(snap => MutationReceipt.Of(
                            receipt: MutationReceipt.Named(name: spec.Name.Value).Document,
                            diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.SilentUserStringMutation(Id: snap.Id)))),
                _ => Fin.Fail<MutationReceipt>(error: key.InvalidResult()),
            };

    private static Fin<MutationReceipt> ReplaceGeometry(RhinoBlocks owner, Definition existing, AuthorSpec spec, Members.Provided members, Op key) =>
        from idx in RequireIndex(snap: existing, key: key)
        from _ in owner.Document.InstanceDefinitions.ModifyGeometry(
            idefIndex: idx,
            newGeometry: members.Geometry.AsEnumerable(),
            newAttributes: members.Attributes.AsEnumerable())
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
        from __ in CommitMetadata(table: owner.Document.InstanceDefinitions, snap: existing, patch: spec.Metadata, key: key)
        select MutationReceipt.Named(name: existing.Name.Value);

    private static DefinitionName AllocateRename(InstanceDefinitionTable table, DefinitionName seed) =>
        DefinitionName.From(value: table.GetUnusedInstanceDefinitionName(root: seed.Value)).IfFail(_ => seed);

    private static Fin<MutationReceipt> PerformAddOrReuse(RhinoBlocks owner, AuthorSpec spec, Members source, Op key) =>
        Reify(doc: owner.Document, source: source, key: key)
            .Bind(provided => LookupByContent(owner: owner, provided: provided, spec: spec, key: key));

    private static Fin<MutationReceipt> LookupByContent(RhinoBlocks owner, Members.Provided provided, AuthorSpec spec, Op key) {
        BlockContentHash hash = BlockContentHash.Of(members: provided);
        return ContentIndex.Find(doc: owner.Document, hash: hash) switch {
            { IsSome: true, Case: DefinitionId existing } => Fin.Succ(value: MutationReceipt.Of(
                receipt: DocumentReceipt.Empty,
                diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.ReusedExisting(Existing: existing)))),
            _ => AddNew(owner: owner, spec: spec, members: provided, key: key)
                .Bind(receipt => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(spec.Name), key: key)
                    .Map(snap => { _ = ContentIndex.RegisterIfMissing(doc: owner.Document, hash: hash, defId: snap.Id); return receipt; })),
        };
    }

    private static Fin<MutationReceipt> PerformModifyMetadata(RhinoBlocks owner, DefinitionRef refer, MetadataPatch patch, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _ in patch.IsEmpty
            ? Fin.Succ(value: unit)
            : CommitMetadata(table: owner.Document.InstanceDefinitions, snap: snap, patch: patch, key: key)
        select MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: snap.Name.Value).Document,
            diagnostics: patch.UserStrings.IsEmpty
                ? Seq<BlockDiagnostic>()
                : Seq<BlockDiagnostic>(new BlockDiagnostic.SilentUserStringMutation(Id: snap.Id)));

    private static Fin<MutationReceipt> PerformModifyGeometry(RhinoBlocks owner, DefinitionRef refer, Members source, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in snap.Live.Map(static live => live.Update == UpdatePolicy.Linked).IfNone(noneValue: false)
            ? Fin.Fail<Unit>(error: key.InvalidInput())     // ModifyGeometry rejected on linked definitions (Dale Fugier policy)
            : Reify(doc: owner.Document, source: source, key: key)
                .Bind(provided => owner.Document.InstanceDefinitions.ModifyGeometry(
                    idefIndex: idx,
                    newGeometry: provided.Geometry.AsEnumerable(),
                    newAttributes: provided.Attributes.AsEnumerable())
                        ? Fin.Succ(value: unit)
                        : Fin.Fail<Unit>(error: key.InvalidResult()))
        select MutationReceipt.Named(name: snap.Name.Value);

    private static Fin<Unit> CommitMetadata(InstanceDefinitionTable table, Definition snap, MetadataPatch patch, Op key) =>
        from idx in RequireIndex(snap: snap, key: key)
        from _ in table.Modify(
            idefIndex: idx,
            newName: snap.Name.Value,
            newDescription: patch.Description.IfNone(snap.Description.IfNone(string.Empty)),
            newUrl: patch.Url.Map(static p => p.Value).IfNone(snap.Url.Map(static u => u.Value).IfNone(string.Empty)),
            newUrlTag: patch.UrlDescription.IfNone(snap.UrlDescription.IfNone(string.Empty)),
            quiet: true)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
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

    private static Fin<Unit> ApplyUserStringsAt(InstanceDefinitionTable table, DefinitionName name, HashMap<string, string> strings, Op key) =>
        ResolveSnap(table: table, refer: DefinitionRef.Of(name), key: key)
            .Bind(snap => RequireIndex(snap: snap, key: key))
            .Bind(idx => ApplyUserStrings(table: table, idx: idx, strings: strings, key: key));

    // ---- [MANAGE] [MUTATION] -------------------------------------------------------------
    private static Fin<MutationReceipt> PerformRename(RhinoBlocks owner, DefinitionRef refer, DefinitionName newName, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.Modify(
            idefIndex: idx,
            newName: newName.Value,
            newDescription: snap.Description.IfNone(noneValue: string.Empty),
            quiet: true)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: newName.Value);

    private static Fin<MutationReceipt> PerformDelete(RhinoBlocks owner, DefinitionRef refer, DeletionPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.Delete(
            idefIndex: idx,
            deleteReferences: policy.DeleteReferences,
            quiet: policy.Quiet)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value);

    private static Fin<MutationReceipt> PerformUndelete(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.Undelete(idefIndex: idx)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value);

    private static Fin<MutationReceipt> PerformUndoModify(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.UndoModify(idefIndex: idx)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: snap.Name.Value);

    private static Fin<MutationReceipt> PerformPurge(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => PurgeOne(owner: owner, refer: r, key: key),
            _ => PurgeAllUnused(owner: owner),
        };

    private static Fin<MutationReceipt> PurgeOne(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.Purge(idefIndex: idx)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value);

    private static Fin<MutationReceipt> PurgeAllUnused(RhinoBlocks owner) {
        int purged = owner.Document.InstanceDefinitions.PurgeUnused();
        return Fin.Succ(value: MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: $"<purged:{purged}>")),
            }));
    }

    private static Fin<MutationReceipt> PerformPurgeByPrefix(RhinoBlocks owner, string prefix, Op key) =>
        Op.Of(name: nameof(PerformPurgeByPrefix)).Catch(() => {
            Seq<int> indices = toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
                .Filter(d => d?.Name?.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase) ?? false)
                .Map(static d => d.Index);
            Seq<DocumentResourceChange> changes = indices.Choose(idx =>
                owner.Document.InstanceDefinitions.Purge(idefIndex: idx)
                    ? Some(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: $"<purged:{idx}>"))
                    : Option<DocumentResourceChange>.None);
            return Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes }));
        });

    private static Fin<MutationReceipt> PerformCompact(RhinoBlocks owner, bool ignoreUndo, Op key) {
        owner.Document.InstanceDefinitions.Compact(ignoreUndoReferences: ignoreUndo);
        return Fin.Succ(value: MutationReceipt.Empty);
    }

    // ---- [PLACE] [MUTATION] --------------------------------------------------------------
    private static Fin<MutationReceipt> PerformPlace(RhinoBlocks owner, DefinitionRef refer, Seq<Transform> at, Option<ObjectAttributes> attrs, Option<HistoryRecord> history, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from placements in at
            .Map(xform => AddInstance(table: owner.Document.Objects, index: idx, xform: xform, attrs: attrs, history: history, key: key))
            .TraverseM(identity).As()
        from _ in placements.IsEmpty
            ? Fin.Fail<Unit>(error: key.InvalidResult())
            : Fin.Succ(value: unit)
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
        // [BOUNDARY ADAPTER — RedrawEnabled state MUST restore on every exit path; LINQ short-circuit
        //  leaks the suppressed state. try/finally is the only correct boundary primitive here.]
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
        from _ in owner.Document.Objects.ReplaceInstanceObject(objectId: instanceId, instanceDefinitionIndex: idx)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
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

    private static Fin<MutationReceipt> PerformTransformCopy(RhinoBlocks owner, Guid instanceId, Transform xform, bool deleteOriginal, Op key) {
        Guid id = owner.Document.Objects.Transform(objectId: instanceId, xform: xform, deleteOriginal: deleteOriginal);
        return id == Guid.Empty
            ? Fin.Fail<MutationReceipt>(error: key.InvalidResult())
            : Fin.Succ(value: MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                Created = Seq(id),
                Deleted = deleteOriginal ? Seq(instanceId) : Seq<Guid>(),
                Transformed = deleteOriginal ? Seq<Guid>() : Seq(id),
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
        let raw = owner.Document.Objects.AddExplodedInstancePieces(instance: instance, explodeNestedInstances: true, deleteInstance: true)
        let produced = raw ?? []
        let expected = instance.InstanceDefinition?.GetObjectIds()?.Length ?? produced.Length
        let diagnostics = produced.Length == expected
            ? Seq<BlockDiagnostic>()
            : Seq<BlockDiagnostic>(new BlockDiagnostic.ExplodePartial(Requested: expected, Received: produced.Length))
        select MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                Created = toSeq(produced),
                Deleted = Seq(instanceId),
            },
            diagnostics: diagnostics);

    // ---- [LINK] --------------------------------------------------------------------------
    private static Fin<MutationReceipt> PerformCreateArchiveLinks(RhinoBlocks owner, Seq<FileEndpoint> sources, Op key) =>
        sources
            .Map(endpoint => key.Catch(() => CreateOneLink(owner: owner, endpoint: endpoint, key: key)))
            .TraverseM(identity).As()
            .Map(static changes => MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes }));

    private static Fin<DocumentResourceChange> CreateOneLink(RhinoBlocks owner, FileEndpoint endpoint, Op key) {
        string filename = endpoint.Path;
        string name = owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: IOPath.GetFileNameWithoutExtension(path: filename));
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: filename);
        return owner.Document.InstanceDefinitions.Add(
            name: name, description: string.Empty, basePoint: Point3d.Origin,
            geometry: [], attributes: []) switch {
                int idx when idx >= 0 && owner.Document.InstanceDefinitions.ModifySourceArchive(
                    idefIndex: idx, sourceArchive: reference,
                    updateType: InstanceDefinitionUpdateType.Linked, quiet: true) =>
                    Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: filename)),
                _ => Fin.Fail<DocumentResourceChange>(error: key.InvalidResult()),
            };
    }

    private static Fin<MutationReceipt> PerformUpdateSourceArchive(RhinoBlocks owner, DefinitionRef refer, FileEndpoint source, UpdatePolicy policy, bool quiet, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in ApplySourceArchive(owner: owner, index: idx, source: source, policy: policy, quiet: quiet, key: key)
        select MutationReceipt.Named(name: snap.Name.Value);

    private static Fin<Unit> ApplySourceArchive(RhinoBlocks owner, int index, FileEndpoint source, UpdatePolicy policy, bool quiet, Op key) {
        using FileReference reference = FileReference.CreateFromFullPath(fullPath: source.Path);
        return owner.Document.InstanceDefinitions.ModifySourceArchive(
            idefIndex: index, sourceArchive: reference, updateType: policy.Native, quiet: quiet)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult());
    }

    private static Fin<MutationReceipt> PerformRefreshLinks(RhinoBlocks owner, Option<Seq<string>> archives, bool skipUpToDate, Op key) {
        Seq<InstanceDefinition> targets = SelectLinked(table: owner.Document.InstanceDefinitions, archives: archives, refs: Option<Seq<DefinitionRef>>.None);
        Seq<InstanceDefinition> filtered = skipUpToDate
            ? targets.Filter(static d => d.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate)
            : targets;
        return filtered.TraverseM(definition => owner.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition)
                ? Fin.Succ(value: definition)
                : Fin.Fail<InstanceDefinition>(error: key.InvalidResult()))
            .As()
            .Map(refreshed => MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                ResourceChanged = refreshed.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
            }));
    }

    private static Fin<MutationReceipt> PerformReloadFromFile(RhinoBlocks owner, DefinitionRef refer, FileEndpoint source, bool nested, bool quiet, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
            idefIndex: idx, filename: source.Path, updateNestedLinks: nested, quiet: quiet)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: snap.Name.Value);

    private static Fin<MutationReceipt> PerformDetachArchive(RhinoBlocks owner, DefinitionRef refer, bool quiet, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _ in owner.Document.InstanceDefinitions.DestroySourceArchive(definition: pair.Live, quiet: quiet)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: pair.Snap.Name.Value);

    private static Fin<MutationReceipt> PerformFlattenLinked(RhinoBlocks owner, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs, Op key) {
        Seq<InstanceDefinition> linked = SelectLinked(table: owner.Document.InstanceDefinitions, archives: archives, refs: refs)
            .Filter(static d => d.ArchiveFileStatus >= InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate);
        _ = linked.Iter(static d => d.LayerStyle = InstanceDefinitionLayerStyle.Active);
        Seq<BlockDiagnostic> ignored = linked
            .Filter(static d => d.LayerStyle != InstanceDefinitionLayerStyle.Active)
            .Choose(static d => DefinitionId.From(value: d.Id).ToOption()
                .Map(id => (BlockDiagnostic)new BlockDiagnostic.LinkedSetterIgnored(
                    Id: id, Actual: UpdatePolicy.FromNative(native: d.UpdateType))));
        return Fin.Succ(value: MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                ResourceChanged = linked.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
            },
            diagnostics: ignored));
    }

    private static Seq<InstanceDefinition> SelectLinked(InstanceDefinitionTable table, Option<Seq<string>> archives, Option<Seq<DefinitionRef>> refs) {
        Seq<InstanceDefinition> linked = toSeq(table)
            .Filter(static d => d.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded);
        Seq<InstanceDefinition> byArchive = archives.Case switch {
            Seq<string> filter => linked.Filter(d => filter.Exists(p =>
                string.Equals(a: p, b: d.SourceArchive ?? string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase))),
            _ => linked,
        };
        return refs.Case switch {
            Seq<DefinitionRef> filter => byArchive.Filter(d => filter.Exists(r => MatchesRef(definition: d, refer: r))),
            _ => byArchive,
        };
    }

    private static bool MatchesRef(InstanceDefinition definition, DefinitionRef refer) =>
        refer switch {
            DefinitionRef.ById byId => definition.Id == byId.Id.Value,
            DefinitionRef.ByIndex byIdx => definition.Index == byIdx.Index.Value,
            DefinitionRef.ByName byName => string.Equals(a: definition.Name, b: byName.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

    // ---- [OBSERVE] [SIDE-EFFECT] ---------------------------------------------------------
    private static Fin<MutationReceipt> PerformExport(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in owner.Document.InstanceDefinitions.Export(idefIndex: idx, filename: target.Path)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: snap.Name.Value);

    /// Rhino 9 `_-ExportBlockAttributes` command — exports definition attributes as CSV.
    /// Routed via RunScript (no first-class managed API). SnapshotName VO pre-sanitizes input.
    private static Fin<MutationReceipt> PerformExportAttributes(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let script = $"_-ExportBlockAttributes _Definition={snap.Name.Value} _File=\"{target.Path}\" _Enter"
        from _ in RhinoApp.RunScript(documentSerialNumber: owner.Document.RuntimeSerialNumber, script: script, echo: false)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: snap.Name.Value);

    /// Snapshot save via RunScript. SnapshotName VO strips forbidden chars (/, \, ", ', =, \n, \r).
    /// [DEFERRED — plan §9] RhinoDoc.RegisterSnapShotClient is the canonical extension point for
    /// per-block geometry-vary snapshots. RunScript is the simpler initial integration; a future
    /// `BlockMutation.RegisterSnapshotClient(SnapShotsClient)` arm exposes the override surface.
    private static Fin<MutationReceipt> PerformSnapshotBlock(RhinoBlocks owner, DefinitionRef refer, SnapshotName name, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let script = $"_-Snapshot _Save _Name=\"{name.Value}\" _Enter"
        from _ in RhinoApp.RunScript(documentSerialNumber: owner.Document.RuntimeSerialNumber, script: script, echo: false)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.InvalidResult())
        select MutationReceipt.Named(name: snap.Name.Value);

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
        from sub in Optional(instance.SubObjectFromComponentIndex(ci: component)).ToFin(Fail: key.InvalidInput())
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
        refer.InstanceDefinitionPart() switch {
            RhinoObject member when member.Attributes.IsInstanceDefinitionObject =>
                DefinitionId.From(value: refer.ObjectId, key: key)
                    .Map(defId => (BlockResult)new BlockResult.MembersResult(Values: Seq(new Member(DefId: defId, MemberId: member.Id, Attrs: Optional(member.Attributes))))),
            _ => Fin.Fail<BlockResult>(error: key.InvalidResult()),
        };

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
        select new Graph.Node(Id: id, Name: name, Members: [.. definition.GetObjectIds() ?? []]);

    private static Seq<Graph.Edge> ProjectEdges(InstanceDefinition definition, Op key) =>
        DefinitionId.From(value: definition.Id, key: key).ToOption() switch {
            { IsSome: true, Case: DefinitionId from } => MemberEdges(definition: definition, from: from) + ArchiveEdges(definition: definition, from: from, key: key),
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

    private static Fin<BlockResult> PerformDependsOn(RhinoBlocks owner, DefinitionRef refer, DependencyTarget target, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from probe in ProbeOf(target: target, live: pair.Live, key: key)
        select (BlockResult)new BlockResult.Probed(Value: probe);

    /// Rail-preserving dispatch — closed-Union DependencyTarget projects to typed Probe; the
    /// wildcard arm returns Fin.Fail instead of throwing, preserving the Fin rail per doctrine.
    private static Fin<Probe> ProbeOf(DependencyTarget target, InstanceDefinition live, Op key) =>
        target switch {
            DependencyTarget.OnDefinition t => Fin.Succ<Probe>(value: new Probe.DefinitionDepth(Depth: live.UsesDefinition(otherIdefIndex: t.OtherIndex))),
            DependencyTarget.OnLayer t => Fin.Succ<Probe>(value: new Probe.LayerUsed(Used: live.UsesLayer(layerIndex: t.LayerIndex))),
            DependencyTarget.OnLinetype t => Fin.Succ<Probe>(value: new Probe.LinetypeUsed(Used: live.UsesLinetype(linetypeIndex: t.LinetypeIndex))),
            _ => Fin.Fail<Probe>(error: key.InvalidInput()),
        };

    private static Fin<BlockResult> PerformFlatten(RhinoBlocks owner, Guid instanceId, FlattenPolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = Walk(instance: instance, parent: Transform.Identity, path: [], depth: 0, max: policy.MaxDepth, key: key)
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
        from handle in owner.AcquirePreview(definition: pair.Live, spec: spec, key: key)
        select (BlockResult)new BlockResult.Preview(Handle: handle);

    private static Fin<BlockResult> PerformTextFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let idStr = snap.Id.Value.ToString()
        let fields = TextFieldPairs(idStr: idStr)
            .Filter(static p => !string.IsNullOrWhiteSpace(value: p.Value) && !string.Equals(a: p.Value, b: "####", comparisonType: StringComparison.Ordinal))
            .Fold(HashMap<string, string>(), static (m, p) => m.AddOrUpdate(key: p.Key, value: p.Value))
        select (BlockResult)new BlockResult.Texts(Fields: fields);

    private static Seq<(string Key, string Value)> TextFieldPairs(string idStr) =>
        Seq(
            (Key: "BlockName", Value: TextFields.BlockName(blockId: idStr) ?? string.Empty),
            (Key: "BlockInstanceCount", Value: TextFields.BlockInstanceCount(instanceDefinitionNameOrId: idStr).ToString(provider: System.Globalization.CultureInfo.InvariantCulture)),
            (Key: "BlockDescription", Value: TextFields.BlockDescription(definitionNameOrId: idStr) ?? string.Empty),
            (Key: "BlockInsertionCoordinate.X", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: Axis.X.Value) ?? string.Empty),
            (Key: "BlockInsertionCoordinate.Y", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: Axis.Y.Value) ?? string.Empty),
            (Key: "BlockInsertionCoordinate.Z", Value: TextFields.BlockInsertionCoordinate(blockId: idStr, axis: Axis.Z.Value) ?? string.Empty));

    private static Fin<BlockResult> PerformAttributeFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
        select (BlockResult)new BlockResult.Attributes(Values: fields);

    // ---- [WATCH] -------------------------------------------------------------------------
    /// Subscribes to the four FileSystemWatcher lifecycle events (matches McNeel's internal
    /// RhinoFileWatcher hook set) so atomic-rename + delete-replace editor save flows aren't
    /// missed. NotifyFilter is narrowed to FileName + LastWrite (RhinoFileWatcher default) so
    /// directory-attribute churn doesn't fire spurious refresh requests.
    ///
    /// [BOUNDARY ADAPTER — FileSystemWatcher.Dispose must run if hook attach or activation throws
    /// AFTER construction; LanguageExt LINQ pipelines short-circuit without disposing.]
    internal static Fin<Subscription> AttachWatcher(RhinoBlocks owner, ArchivePath path, WatchPolicy policy) =>
        Op.Of(name: nameof(AttachWatcher)).Catch(() => {
            string dir = IOPath.GetDirectoryName(path: path.Value) ?? string.Empty;
            string filter = IOPath.GetFileName(path: path.Value);
            FileSystemWatcher watcher = new(path: dir, filter: filter) {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };
            try {
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
        Op.Of(name: nameof(RefreshArchive)).Catch(() => {
            _ = toSeq(doc.InstanceDefinitions)
                .Filter(d => string.Equals(a: d?.SourceArchive ?? string.Empty, b: path.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
                .Iter(d => doc.InstanceDefinitions.RefreshLinkedBlock(definition: d));
            return Fin.Succ(value: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: path.Value)),
            });
        });

    private sealed record WatchContext(
        RhinoBlocks Owner,
        ArchivePath Path,
        WatchPolicy Policy,
        Atom<DateTimeOffset> LastFired,
        FileSystemWatcher Watcher);

    // ---- [CONTENT INDEX] [LIFECYCLE HOOKS] -----------------------------------------------
    /// Single eviction hook invoked by EventBridge on both document close AND table mutation
    /// (Added/Modified/Deleted). Keeps the content-hash index coherent without per-call linear
    /// scans. Collapsed from `OnDocClose` + `InvalidateContentIndex` — both bodies were identical.
    internal static Unit InvalidateContentIndex(uint serial) => ContentIndex.EvictDoc(serial: serial);
}

// --- [COMPOSITION] [CONTENT INDEX] --------------------------------------------------------
/// Per-doc content-hash → DefinitionId index, cached process-statically. Built lazily on
/// first AddOrReuse lookup; flushed on InstanceDefinitionTableEvent.Added/Modified/Deleted
/// via EventBridge. Eliminates the O(N) full-table scan per AddOrReuse call.
internal static class ContentIndex {
    private static readonly Atom<HashMap<uint, HashMap<ulong, DefinitionId>>> byDoc =
        Atom(value: HashMap<uint, HashMap<ulong, DefinitionId>>());

    internal static Option<DefinitionId> Find(RhinoDoc doc, BlockContentHash hash) {
        uint serial = doc.RuntimeSerialNumber;
        HashMap<ulong, DefinitionId> idx = byDoc.Value.Find(key: serial) switch {
            { IsSome: true, Case: HashMap<ulong, DefinitionId> existing } => existing,
            _ => Build(doc: doc, serial: serial),
        };
        return idx.Find(key: hash.Value);
    }

    /// On partial-miss, merge the new (hash, defId) into the existing inner HashMap instead of
    /// rebuilding the entire per-doc index. The previous rebuild was an O(N) hash recomputation
    /// on every AddOrReuse call that returned a fresh definition. Build runs only when no inner
    /// map exists yet (cold doc).
    internal static Unit RegisterIfMissing(RhinoDoc doc, BlockContentHash hash, DefinitionId defId) {
        uint serial = doc.RuntimeSerialNumber;
        _ = byDoc.Swap(f: m => m.Find(key: serial) switch {
            { IsSome: true, Case: HashMap<ulong, DefinitionId> existing } when existing.ContainsKey(key: hash.Value) =>
                m,
            { IsSome: true, Case: HashMap<ulong, DefinitionId> existing } =>
                m.AddOrUpdate(key: serial, value: existing.AddOrUpdate(key: hash.Value, value: defId)),
            _ =>
                m.AddOrUpdate(key: serial, value: Build(doc: doc, serial: serial).AddOrUpdate(key: hash.Value, value: defId)),
        });
        return unit;
    }

    internal static Unit EvictDoc(uint serial) {
        _ = byDoc.Swap(f: m => m.Remove(key: serial));
        return unit;
    }

    /// Pure projection — does NOT touch `byDoc`. Caller installs the result via outer Swap so
    /// build never runs inside another Swap's CAS retry loop (which would re-execute on contention
    /// and produce divergent intermediate state).
    private static HashMap<ulong, DefinitionId> Build(RhinoDoc doc, uint serial) =>
        toSeq(doc.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Choose(d => HashEntry(doc: doc, active: d))
            .Fold(HashMap<ulong, DefinitionId>(), static (m, kv) => m.AddOrUpdate(key: kv.Key, value: kv.Value));

    private static Option<(ulong Key, DefinitionId Value)> HashEntry(RhinoDoc doc, InstanceDefinition active) =>
        from id in DefinitionId.From(value: active.Id).ToOption()
        from provided in ReifyForHash(doc: doc, ids: active.GetObjectIds() ?? [])
        select (Key: BlockContentHash.Of(members: provided).Value, Value: id);

    private static Option<Members.Provided> ReifyForHash(RhinoDoc doc, Guid[] ids) {
        Seq<RhinoObject> objects = toSeq(ids).Choose(id => Optional(doc.Objects.FindId(id: id)));
        return Members.OfProvided(
            geometry: objects.Map(static o => o.Geometry.Duplicate()),
            attributes: objects.Map(static o => o.Attributes.Duplicate())).ToOption();
    }
}
