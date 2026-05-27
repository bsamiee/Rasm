using System.Collections.Immutable;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using IOPath = System.IO.Path;
using TextFields = Rhino.Runtime.TextFields;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockOp {
    private BlockOp() { }

    // [AUTHOR]
    public sealed record Author(AuthorSpec Spec, Members Source, ConflictPolicy Conflict) : BlockOp;
    public sealed record AddOrReuse(AuthorSpec Spec, Members Source) : BlockOp;
    public sealed record ModifyMetadata(DefinitionRef Ref, MetadataPatch Patch) : BlockOp;
    public sealed record ModifyGeometry(DefinitionRef Ref, Members Source) : BlockOp;

    // [MANAGE]
    public sealed record Rename(DefinitionRef Ref, DefinitionName NewName) : BlockOp;
    public sealed record Delete(DefinitionRef Ref, DeletionPolicy Policy) : BlockOp;
    public sealed record Undelete(DefinitionRef Ref) : BlockOp;
    public sealed record UndoModify(DefinitionRef Ref) : BlockOp;
    public sealed record Purge(Option<DefinitionRef> Ref) : BlockOp;
    public sealed record PurgeByPrefix(DefinitionPrefix Prefix) : BlockOp;
    public sealed record Compact(CompactPolicy? Policy = null) : BlockOp;

    // [PLACE]
    public sealed record Place(DefinitionRef Ref, Seq<Placement> At, BatchPolicy? Policy = null) : BlockOp;
    public sealed record ReplaceDefinition(Guid InstanceId, DefinitionRef NewRef) : BlockOp;
    public sealed record TransformInstance(Guid InstanceId, Transform Xform, TransformPolicy? Mode = null) : BlockOp;
    public sealed record ExplodeIntoDocument(Guid InstanceId, ExplodePolicy Policy) : BlockOp;

    // [LINK]
    public sealed record CreateArchiveLinks(Seq<FileEndpoint> Sources, LinkCreatePolicy? Policy = null) : BlockOp;
    public sealed record UpdateSourceArchive(DefinitionRef Ref, FileEndpoint Source, UpdatePolicy Policy, LinkReloadPolicy? Reload = null) : BlockOp;
    public sealed record BatchRelink(Seq<LinkMap> Maps, BatchPolicy? Policy = null) : BlockOp;
    public sealed record RefreshLinks(BlockFilter? Filter = null, LinkRefreshPolicy? Policy = null) : BlockOp;
    public sealed record ReloadFromFile(DefinitionRef Ref, FileEndpoint Source, LinkReloadPolicy? Policy = null) : BlockOp;
    public sealed record DetachLinked(BlockFilter? Filter = null) : BlockOp;
    public sealed record FlattenLinked(BlockFilter? Filter = null) : BlockOp;
    public sealed record BakeArchive(FileEndpoint Source, BakePolicy? Policy = null) : BlockOp;

    // [OBSERVE] [SIDE-EFFECT]
    public sealed record Export(DefinitionRef Ref, FileEndpoint Target) : BlockOp;
    public sealed record ExportAttributes(DefinitionRef Ref, FileEndpoint Target) : BlockOp;
    public sealed record SnapshotBlock(DefinitionRef Ref, SnapshotName Name) : BlockOp;

    // [MANAGE]
    public sealed record AllocateName(Option<string> Seed) : BlockOp;
    public sealed record Snapshot(Option<DefinitionRef> Ref) : BlockOp;
    public sealed record Duplicate(DefinitionRef Ref, Option<DefinitionName> Name, ConflictPolicy? Conflict = null) : BlockOp;

    // [PLACE]
    public sealed record ExplodeInspect(Guid InstanceId, ExplodePolicy Policy) : BlockOp;
    public sealed record UseSubObject(Guid InstanceId, ComponentIndex Component) : BlockOp;
    // [GRAPH]
    public sealed record Graph(GraphQuery Query) : BlockOp;
    public sealed record SelectedPart(ObjRef Ref) : BlockOp;
    public sealed record Flatten(Guid InstanceId, DepthPolicy Policy) : BlockOp;
    public sealed record Bounds(DefinitionRef Ref, BoundsPolicy? Policy = null) : BlockOp;

    // [OBSERVE]
    public sealed record Preview(DefinitionRef Ref, PreviewSpec Spec) : BlockOp;
    public sealed record TextFieldsOf(DefinitionRef Ref, Option<Guid> InstanceId = default) : BlockOp;
    public sealed record AttributeFieldsOf(DefinitionRef Ref) : BlockOp;
    public sealed record AttributeMatrix(Option<Seq<DefinitionRef>> Refs, ReferenceScope Scope) : BlockOp;

    /// Document mutation, linked I/O, scripted export, and preview rasterization require UI-thread marshaling.
    internal bool RequiresUiThread() => this switch {
        BlockOp.AllocateName or BlockOp.Snapshot or BlockOp.ExplodeInspect or BlockOp.UseSubObject
            or BlockOp.Graph or BlockOp.SelectedPart or BlockOp.Flatten or BlockOp.Bounds
            or BlockOp.TextFieldsOf or BlockOp.AttributeFieldsOf or BlockOp.AttributeMatrix => false,
        _ => true,
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class Operations {
    // ---- [DISPATCH] ----------------------------------------------------------------------
    internal static Fin<BlockOutcome> Run(BlockOp op, RhinoBlocks owner) =>
        op.Switch(
            owner: owner,
            author: static (o, x) => RunMut(name: nameof(BlockOp.Author), body: k => PerformAuthor(owner: o, spec: x.Spec, source: x.Source, conflict: x.Conflict, key: k)),
            addOrReuse: static (o, x) => RunMut(name: nameof(BlockOp.AddOrReuse), body: k => PerformAddOrReuse(owner: o, spec: x.Spec, source: x.Source, key: k)),
            modifyMetadata: static (o, x) => RunMut(name: nameof(BlockOp.ModifyMetadata), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.ModifyMetadata(Patch: x.Patch), key: k)),
            modifyGeometry: static (o, x) => RunMut(name: nameof(BlockOp.ModifyGeometry), body: k => PerformModifyGeometry(owner: o, refer: x.Ref, source: x.Source, key: k)),
            rename: static (o, x) => RunMut(name: nameof(BlockOp.Rename), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.Rename(NewName: x.NewName), key: k)),
            delete: static (o, x) => RunMut(name: nameof(BlockOp.Delete), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.Delete(Policy: x.Policy), key: k)),
            undelete: static (o, x) => RunMut(name: nameof(BlockOp.Undelete), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.Undelete(), key: k)),
            undoModify: static (o, x) => RunMut(name: nameof(BlockOp.UndoModify), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.UndoModify(), key: k)),
            purge: static (o, x) => RunMut(name: nameof(BlockOp.Purge), body: k => PerformPurge(owner: o, refer: x.Ref, key: k)),
            purgeByPrefix: static (o, x) => RunMut(name: nameof(BlockOp.PurgeByPrefix), body: k => PerformPurgeByPrefix(owner: o, prefix: x.Prefix, key: k)),
            compact: static (o, x) => RunMut(name: nameof(BlockOp.Compact), body: k => PerformCompact(owner: o, policy: x.Policy ?? CompactPolicy.UndoAware, key: k)),
            place: static (o, x) => RunMut(name: nameof(BlockOp.Place), body: k => PerformPlace(owner: o, refer: x.Ref, at: x.At, policy: x.Policy ?? BatchPolicy.Default, key: k)),
            replaceDefinition: static (o, x) => RunMut(name: nameof(BlockOp.ReplaceDefinition), body: k => PerformReplaceDefinition(owner: o, instanceId: x.InstanceId, newRef: x.NewRef, key: k)),
            transformInstance: static (o, x) => RunMut(name: nameof(BlockOp.TransformInstance), body: k => PerformTransformInstance(owner: o, instanceId: x.InstanceId, xform: x.Xform, mode: x.Mode ?? TransformPolicy.Copy, key: k)),
            explodeIntoDocument: static (o, x) => RunMut(name: nameof(BlockOp.ExplodeIntoDocument), body: k => PerformExplodeIntoDocument(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            createArchiveLinks: static (o, x) => RunMut(name: nameof(BlockOp.CreateArchiveLinks), body: k => PerformCreateArchiveLinks(owner: o, sources: x.Sources, policy: x.Policy ?? LinkCreatePolicy.Default, key: k)),
            updateSourceArchive: static (o, x) => RunMut(name: nameof(BlockOp.UpdateSourceArchive), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.UpdateSourceArchive(Source: x.Source, Policy: x.Policy, Reload: x.Reload ?? LinkReloadPolicy.NestedQuiet), key: k)),
            batchRelink: static (o, x) => RunMut(name: nameof(BlockOp.BatchRelink), body: k => PerformBatchRelink(owner: o, maps: x.Maps, policy: x.Policy ?? BatchPolicy.Default, key: k)),
            refreshLinks: static (o, x) => RunMut(name: nameof(BlockOp.RefreshLinks), body: k => PerformRefreshLinks(owner: o, filter: x.Filter ?? BlockFilter.All, policy: x.Policy ?? LinkRefreshPolicy.All, key: k)),
            reloadFromFile: static (o, x) => RunMut(name: nameof(BlockOp.ReloadFromFile), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.ReloadFromFile(Source: x.Source, Policy: x.Policy ?? LinkReloadPolicy.NestedQuiet), key: k)),
            detachLinked: static (o, x) => RunMut(name: nameof(BlockOp.DetachLinked), body: k => PerformLinkedFilter(owner: o, filter: x.Filter ?? BlockFilter.All, key: k, worker: DetachLinkedDefinition)),
            flattenLinked: static (o, x) => RunMut(name: nameof(BlockOp.FlattenLinked), body: k => PerformLinkedFilter(owner: o, filter: x.Filter ?? BlockFilter.All, key: k, worker: ApplyLinkedLayerStyle)),
            bakeArchive: static (o, x) => RunMut(name: nameof(BlockOp.BakeArchive), body: k => PerformBakeArchive(owner: o, source: x.Source, policy: x.Policy ?? BakePolicy.Default, key: k)),
            export: static (o, x) => RunMut(name: nameof(BlockOp.Export), body: k => Mutate(owner: o, refer: x.Ref, mutation: new TableMutation.Export(Target: x.Target), key: k)),
            exportAttributes: static (o, x) => RunMut(name: nameof(BlockOp.ExportAttributes), body: k => PerformExportAttributes(owner: o, refer: x.Ref, target: x.Target, key: k)),
            snapshotBlock: static (o, x) => RunMut(name: nameof(BlockOp.SnapshotBlock), body: k => PerformSnapshotBlock(owner: o, refer: x.Ref, name: x.Name, key: k)),
            allocateName: static (o, x) => RunQry(name: nameof(BlockOp.AllocateName), body: k => PerformAllocateName(owner: o, seed: x.Seed, key: k)),
            snapshot: static (o, x) => RunQry(name: nameof(BlockOp.Snapshot), body: k => PerformSnapshot(owner: o, refer: x.Ref, key: k)),
            duplicate: static (o, x) => RunMut(name: nameof(BlockOp.Duplicate), body: k => PerformDuplicate(owner: o, refer: x.Ref, name: x.Name, conflict: x.Conflict ?? ConflictPolicy.Rename, key: k)),
            explodeInspect: static (o, x) => RunQry(name: nameof(BlockOp.ExplodeInspect), body: k => PerformExplodeInspect(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            useSubObject: static (o, x) => RunQry(name: nameof(BlockOp.UseSubObject), body: k => PerformUseSubObject(owner: o, instanceId: x.InstanceId, component: x.Component, key: k)),
            graph: static (o, x) => RunQry(name: nameof(BlockOp.Graph), body: k => PerformGraph(owner: o, query: x.Query, key: k)),
            selectedPart: static (o, x) => RunQry(name: nameof(BlockOp.SelectedPart), body: k => PerformSelectedPart(owner: o, refer: x.Ref, key: k)),
            flatten: static (o, x) => RunQry(name: nameof(BlockOp.Flatten), body: k => PerformFlatten(owner: o, instanceId: x.InstanceId, policy: x.Policy, key: k)),
            bounds: static (o, x) => RunQry(name: nameof(BlockOp.Bounds), body: k => PerformBounds(owner: o, refer: x.Ref, policy: x.Policy ?? BoundsPolicy.Default, key: k)),
            preview: static (o, x) => RunQry(name: nameof(BlockOp.Preview), body: k => PerformPreview(owner: o, refer: x.Ref, spec: x.Spec, key: k)),
            textFieldsOf: static (o, x) => RunQry(name: nameof(BlockOp.TextFieldsOf), body: k => PerformTextFields(owner: o, refer: x.Ref, instanceId: x.InstanceId, key: k)),
            attributeFieldsOf: static (o, x) => RunQry(name: nameof(BlockOp.AttributeFieldsOf), body: k => PerformAttributeFields(owner: o, refer: x.Ref, key: k)),
            attributeMatrix: static (o, x) => RunQry(name: nameof(BlockOp.AttributeMatrix), body: k => PerformAttributeMatrix(owner: o, refs: x.Refs, scope: x.Scope, key: k)));

    private static Fin<BlockOutcome> RunMut(string name, Func<Op, Fin<MutationReceipt>> body) =>
        RunPhase(name: name, body: body, project: static receipt => new BlockOutcome.Receipt(Value: receipt));

    private static Fin<BlockOutcome> RunQry(string name, Func<Op, Fin<BlockOutcome>> body) =>
        RunPhase(name: name, body: body, project: static outcome => outcome);

    private static Fin<BlockOutcome> RunPhase<T>(
        string name,
        Func<Op, Fin<T>> body,
        Func<T, BlockOutcome> project) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key).Map(project));
    }

    private static Fin<Unit> ConfirmNative(Op key, string step, bool success) =>
        success ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: key.InvalidResult(detail: step));

    /// Linked attach requires an empty placeholder definition; non-empty geometry breaks ModifySourceArchive.
    private static int AddLinkPlaceholder(InstanceDefinitionTable table, string name) =>
        table.Add(
            name: name,
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: [],
            attributes: []);

    // ---- [RESOLVE] -----------------------------------------------------------------------
    internal static Fin<(Definition Snap, InstanceDefinition Live)> Resolve(InstanceDefinitionTable table, DefinitionRef refer, Op key, bool includeDeleted = false) =>
        FindDefinition(table: table, refer: refer, includeDeleted: includeDeleted, key: key)
            .Bind(live => SnapshotFromLive(table: table, live: live, key: key).Map(snap => (Snap: snap, Live: live)));

    internal static Fin<Definition> ResolveSnap(InstanceDefinitionTable table, DefinitionRef refer, Op key) =>
        Resolve(table: table, refer: refer, key: key).Map(static pair => pair.Snap);

    private static Fin<Definition> SnapshotFromLive(InstanceDefinitionTable table, InstanceDefinition live, Op key) {
        uint serial = table.Document.RuntimeSerialNumber;
        return DefinitionId.From(value: live.Id, key: key).Bind(id =>
            SnapshotVault.Find(docSerial: serial, id: id).Case switch {
                Definition cached => Fin.Succ(value: cached),
                _ => Definition.From(definition: live, anchorDirectory: BlockPaths.DocAnchor(document: table.Document), key: key)
                    .Map(snap => { _ = SnapshotVault.Upsert(docSerial: serial, definition: snap); return snap; }),
            });
    }

    private static Fin<InstanceDefinition> FindDefinition(InstanceDefinitionTable table, DefinitionRef refer, bool includeDeleted, Op key) =>
        refer.Switch(
            state: (Table: table, IncludeDeleted: includeDeleted, Key: key),
            byId: static (ctx, r) => Optional(ctx.Table.Find(instanceId: r.Id.Value, ignoreDeletedInstanceDefinitions: !ctx.IncludeDeleted)).ToFin(Fail: ctx.Key.InvalidInput()),
            byIndex: static (ctx, r) => r.Index.Value >= 0 && r.Index.Value < ctx.Table.Count
                ? Optional(ctx.Table[r.Index.Value]).Filter(d => ctx.IncludeDeleted || !d.IsDeleted).ToFin(Fail: ctx.Key.InvalidInput())
                : Fin.Fail<InstanceDefinition>(error: ctx.Key.InvalidInput()),
            byName: static (ctx, r) => (ctx.IncludeDeleted
                ? toSeq(ctx.Table.GetList(ignoreDeleted: false))
                    .Filter(static d => d is not null)
                    .Find(d => string.Equals(a: d.Name, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase)
                               || string.Equals(a: d.DeletedName, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
                : Optional(ctx.Table.Find(instanceDefinitionName: r.Name.Value))).ToFin(Fail: ctx.Key.InvalidInput()));

    /// Flush snapshot/preview caches; re-register the content-index entry when a doc is in scope.
    internal static Unit InvalidateDefinition(Guid defId, Option<RhinoDoc> doc = default) {
        _ = SnapshotVault.Invalidate(defId: defId);
        _ = PreviewVault.Invalidate(defId: defId);
        return doc.Case switch {
            RhinoDoc active => ContentIndex.RegisterDefinition(doc: active, defId: defId),
            _ => unit,
        };
    }

    private static T InvalidateWith<T>(RhinoDoc doc, Guid defId, T value) {
        _ = InvalidateDefinition(defId: defId, doc: Some(value: doc));
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
            ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(admitted.Name), key: key).Match(
                Fail: _ => Reify(doc: owner.Document, source: source, key: key)
                    .Bind(provided => AddNew(owner: owner, spec: admitted, members: provided, key: key)),
                Succ: existing => ResolveConflict(owner: owner, existing: existing, spec: admitted, source: source, conflict: conflict, key: key)));

    private static Fin<MutationReceipt> ResolveConflict(RhinoBlocks owner, Definition existing, AuthorSpec spec, Members source, ConflictPolicy conflict, Op key) =>
        conflict.Switch(
            state: (Owner: owner, Existing: existing, Spec: spec, Source: source, Key: key),
            replace: static s => Reify(doc: s.Owner.Document, source: s.Source, key: s.Key)
                .Bind(provided => ReplaceGeometry(owner: s.Owner, existing: s.Existing, spec: s.Spec, members: provided, key: s.Key)),
            skip: static s => Fin.Succ(value: MutationReceipt.Of(
                receipt: DocumentReceipt.Empty,
                diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.DuplicateName(Name: s.Existing.Name, Existing: s.Existing.Id)))),
            fail: static s => Fin.Succ(value: MutationReceipt.Of(
                receipt: DocumentReceipt.Empty,
                diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.ConflictFailed(Name: s.Existing.Name, Existing: s.Existing.Id)))),
            rename: static s => Reify(doc: s.Owner.Document, source: s.Source, key: s.Key)
                .Bind(provided => AddNew(
                    owner: s.Owner,
                    spec: s.Spec with {
                        Name = DefinitionName.From(value: s.Owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: s.Spec.Name.Value)).IfFail(_ => s.Spec.Name),
                    },
                    members: provided,
                    key: s.Key)));

    private static Fin<Members.Provided> Reify(RhinoDoc doc, Members source, Op key) =>
        source.Switch(
            state: (Doc: doc, Key: key),
            provided: static (_, p) => Fin.Succ(value: p),
            fromDocument: static (ctx, fd) => ReifyDocumentMembers(doc: ctx.Doc, sources: fd.Sources, key: ctx.Key),
            fromConstruction: static (ctx, fc) => Members.OfProvided(geometry: fc.Geometry, attributes: fc.Attributes, key: ctx.Key));

    internal static Fin<Members.Provided> ReifyObjects(Seq<(GeometryBase Geometry, ObjectAttributes Attributes)> pairs, Op key) =>
        pairs.IsEmpty
            ? Fin.Fail<Members.Provided>(error: key.InvalidInput())
            : Members.OfProvided(
                geometry: pairs.Map(static p => p.Geometry),
                attributes: pairs.Map(static p => p.Attributes),
                key: key);

    internal static Fin<Members.Provided> ReifyDocumentMembers(RhinoDoc doc, Seq<Guid> sources, Op key) =>
        sources.IsEmpty
            ? Fin.Fail<Members.Provided>(error: key.InvalidInput())
            : sources
                .Map(id => Optional(doc.Objects.FindId(id: id)).ToFin(Fail: key.InvalidInput()))
                .TraverseM(identity).As()
                .Bind(objs => ReifyObjects(
                    pairs: objs.Map(static o => (o.Geometry.Duplicate(), Members.SanitizeAttributes(attributes: o.Attributes))),
                    key: key));

    internal static Fin<Members.Provided> ReifyDefinitionMembers(InstanceDefinition definition, Op key) =>
        Optional(definition).ToFin(Fail: key.InvalidInput())
            .Bind(def => ReifyObjects(pairs: CanonicalMemberPairs(def: def), key: key));

    /// Nested InstanceObject members project to InstanceReferenceGeometry so content-hash and graph walks agree.
    internal static Seq<(GeometryBase Geometry, ObjectAttributes Attributes)> CanonicalMemberPairs(InstanceDefinition def) =>
        toSeq(def.GetObjects()).Filter(static o => o is not null).Choose(static o => o switch {
            InstanceObject nested when nested.InstanceDefinition is InstanceDefinition inst => Some((
                (GeometryBase)new InstanceReferenceGeometry(instanceDefinitionId: inst.Id, transform: nested.InstanceXform),
                Members.SanitizeAttributes(attributes: nested.Attributes))),
            { Geometry: InstanceReferenceGeometry r } => Some((
                r.Duplicate(),
                Members.SanitizeAttributes(attributes: o!.Attributes))),
            RhinoObject leaf when leaf.Geometry is not null => Some((
                leaf.Geometry.Duplicate(),
                Members.SanitizeAttributes(attributes: leaf.Attributes))),
            _ => None,
        });

    internal static Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)> NestedReferences(InstanceDefinition def) =>
        BindDefinitionMembers(
            def: def,
            composed: Transform.Identity,
            onInstance: (nested, _) => Optional(nested.InstanceDefinition).Map(inst =>
                Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>((
                    ObjectId: nested.Id,
                    Reference: new InstanceReferenceGeometry(instanceDefinitionId: inst.Id, transform: nested.InstanceXform)))).IfNone(Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>()),
            onReference: (reference, memberId, _) => Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)>((
                ObjectId: memberId,
                Reference: (InstanceReferenceGeometry)reference.Duplicate())));

    private static Fin<MutationReceipt> AddNew(RhinoBlocks owner, AuthorSpec spec, Members.Provided members, Op key) =>
        AddDefinition(table: owner.Document.InstanceDefinitions, spec: spec, members: members) switch {
            int idx when idx >= 0 => ApplyCreateState(table: owner.Document.InstanceDefinitions, idx: idx, spec: spec, members: members, key: key),
            _ => Fin.Fail<MutationReceipt>(error: key.InvalidResult()),
        };

    private static int AddDefinition(InstanceDefinitionTable table, AuthorSpec spec, Members.Provided members) {
        string description = spec.Metadata.Description.IfNone(noneValue: string.Empty);
        return toSeq([
            () => (members.Geometry.IsEmpty, members.Attributes.IsEmpty) switch {
                (false, false) => table.Add(
                    name: spec.Name.Value,
                    description: description,
                    basePoint: spec.BasePoint,
                    geometry: members.Geometry[0].Duplicate(),
                    attributes: members.Attributes[0].Duplicate()),
                _ => -1,
            },
            () => spec.Metadata.Url.IsSome
                ? table.Add(
                    name: spec.Name.Value,
                    description: description,
                    url: spec.Metadata.Url.Map(static p => p.Value).IfNone(noneValue: string.Empty),
                    urlTag: spec.Metadata.UrlDescription.IfNone(noneValue: string.Empty),
                    basePoint: spec.BasePoint,
                    geometry: members.Geometry.AsEnumerable(),
                    attributes: members.Attributes.AsEnumerable())
                : -1,
            () => table.Add(
                name: spec.Name.Value,
                description: description,
                basePoint: spec.BasePoint,
                geometry: members.Geometry.AsEnumerable(),
                attributes: members.Attributes.AsEnumerable()),
            () => table.Add(
                name: spec.Name.Value,
                description: description,
                basePoint: spec.BasePoint,
                geometry: members.Geometry.AsEnumerable()),
        ]).Map(static attempt => attempt()).Find(static idx => idx >= 0).IfNone(noneValue: -1);
    }

    /// Tri-way member projection: nested InstanceObject, embedded InstanceReferenceGeometry, or leaf RhinoObject.
    private static Seq<T> BindDefinitionMembers<T>(
        InstanceDefinition def, Transform composed,
        Func<InstanceObject, Transform, Seq<T>> onInstance,
        Func<InstanceReferenceGeometry, Guid, Transform, Seq<T>> onReference,
        Func<RhinoObject, Transform, Seq<T>>? onLeaf = null) =>
        toSeq(def.GetObjects()).Filter(static o => o is not null).Bind(member => member switch {
            InstanceObject nested => onInstance(nested, composed),
            { Geometry: InstanceReferenceGeometry r } => onReference(r, member!.Id, composed),
            RhinoObject leaf when onLeaf is not null => onLeaf(leaf, composed),
            _ => Seq<T>(),
        });

    /// BOUNDARY ADAPTER — Rhino exposes linked layer style as mutable native state.
    private static Fin<MutationReceipt> ApplyCreateState(InstanceDefinitionTable table, int idx, AuthorSpec spec, Members.Provided members, Op key) =>
        from live in Optional(table[idx] ?? table.Find(instanceDefinitionName: spec.Name.Value)).ToFin(Fail: key.InvalidResult())
        from id in DefinitionId.From(value: live.Id, key: key)
        let _layer = (live.LayerStyle = spec.Layer.Native, unit).Item2
        from _strings in ApplyUserStringsWhenPresent(table: table, idx: idx, strings: spec.Metadata.UserStrings, key: key)
        from snap in Definition.From(definition: live, anchorDirectory: BlockPaths.DocAnchor(document: table.Document), key: key)
        let _cached = SnapshotVault.Upsert(docSerial: table.Document.RuntimeSerialNumber, definition: snap)
        let _indexed = Op.Of(name: nameof(BlockContentHash))
            .Catch(() => Fin.Succ(value: BlockContentHash.Of(members: members)))
            .Iter(hash => ContentIndex.RegisterIfMissing(doc: table.Document, hash: hash, defId: snap.Id))
        select MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: spec.Name.Value).Document,
            diagnostics: spec.CreateDiagnostics(id: id, live: live));

    /// SetUserString does not fire InstanceDefinitionTableEvent.Modified — force invalidate caches
    /// so subscribers see fresh snapshots and previews.
    private static Fin<Unit> ApplyUserStringsWhenPresent(InstanceDefinitionTable table, int idx, HashMap<string, string> strings, Op key) =>
        strings.IsEmpty
            ? Fin.Succ(value: unit)
            : Optional(table[idx]).ToFin(Fail: key.InvalidResult()).Map(live => {
                _ = strings.Iter((k, v) => live.SetUserString(key: k, value: v));
                return InvalidateDefinition(defId: live.Id);
            });

    /// Hash provided members → find matching live definition via ContentIndex → confirm equivalence by re-hash;
    /// hit emits ReusedExisting diagnostic, miss falls back to AddNew.
    private static Fin<MutationReceipt> LookupByContent(RhinoBlocks owner, Members.Provided provided, AuthorSpec spec, Op key) {
        ulong hash = BlockContentHash.Of(members: provided).Value;
        return ContentIndex.Find(doc: owner.Document, hash: BlockContentHash.Create(hash))
            .Choose(defId => Optional(owner.Document.InstanceDefinitions.Find(instanceId: defId.Value, ignoreDeletedInstanceDefinitions: true))
                .Bind(live => ReifyDefinitionMembers(definition: live, key: key).ToOption())
                .Filter(existing => BlockContentHash.Of(members: existing).Value == hash)
                .Map(_ => defId))
            .Find(static _ => true)
            .Match(
                Some: id => Fin.Succ(value: MutationReceipt.Of(
                    receipt: DocumentReceipt.Empty,
                    diagnostics: Seq<BlockDiagnostic>(new BlockDiagnostic.ReusedExisting(Existing: id)))),
                None: () => AddNew(owner: owner, spec: spec, members: provided, key: key));
    }

    private static Fin<MutationReceipt> ReplaceGeometry(RhinoBlocks owner, Definition existing, AuthorSpec spec, Members.Provided members, Op key) =>
        from idx in RequireIndex(snap: existing, key: key)
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.ModifyGeometry(
            idefIndex: idx,
            newGeometry: members.Geometry.AsEnumerable(),
            newAttributes: members.Attributes.AsEnumerable()))
        from __ in CommitMetadata(table: owner.Document.InstanceDefinitions, snap: existing, patch: spec.Metadata, key: key)
        select InvalidateWith(doc: owner.Document, defId: existing.Id.Value, value: MutationReceipt.Named(name: existing.Name.Value));

    private static Fin<MutationReceipt> PerformAddOrReuse(RhinoBlocks owner, AuthorSpec spec, Members source, Op key) =>
        spec.Admit(key: key)
            .Bind(admitted => Reify(doc: owner.Document, source: source, key: key)
                .Bind(provided => LookupByContent(owner: owner, provided: provided, spec: admitted, key: key)));

    private static Fin<MutationReceipt> PerformModifyGeometry(RhinoBlocks owner, DefinitionRef refer, Members source, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from live in snap.Live.ToFin(Fail: key.InvalidInput())
        from _ in live.Update.RejectLinkedModify(key: key)
        from provided in Reify(doc: owner.Document, source: source, key: key)
        from __ in key.Confirm(success: owner.Document.InstanceDefinitions.ModifyGeometry(
            idefIndex: idx,
            newGeometry: provided.Geometry.AsEnumerable(),
            newAttributes: provided.Attributes.AsEnumerable()))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<Unit> CommitMetadata(InstanceDefinitionTable table, Definition snap, MetadataPatch patch, Op key) =>
        from idx in RequireIndex(snap: snap, key: key)
        from metaOk in key.Confirm(success: table.Modify(
            idefIndex: idx,
            newName: snap.Name.Value,
            newDescription: patch.Description.IfNone(snap.Description.IfNone(string.Empty)),
            newUrl: patch.Url.Map(static p => p.Value).IfNone(snap.Url.Map(static u => u.Value).IfNone(string.Empty)),
            newUrlTag: patch.UrlDescription.IfNone(snap.UrlDescription.IfNone(string.Empty)),
            quiet: true))
        from strings in ApplyUserStringsWhenPresent(table: table, idx: idx, strings: patch.UserStrings, key: key)
        from dict in patch.UserDictionary.Case is ArchivableDictionary d
            ? Optional(table[idx]).ToFin(Fail: key.InvalidResult()).Map(live => {
                UserDictionary updated = new();
                _ = updated.Dictionary.ReplaceContentsWith(source: d);
                return key.Confirm(success: table.Modify(idefIndex: idx, userData: updated, quiet: true));
            })
            : Fin.Succ(value: unit)
        select unit;

    // ---- [TABLE MUTATION] ----------------------------------------------------------------
    private readonly record struct MutateCtx(RhinoBlocks Owner, int Index, Definition Snap, Op Key);

    [Union(SwitchMapStateParameterName = "ctx")]
    private abstract partial record TableMutation {
        private TableMutation() { }
        public sealed record Rename(DefinitionName NewName) : TableMutation;
        public sealed record Delete(DeletionPolicy Policy) : TableMutation;
        public sealed record Undelete() : TableMutation;
        public sealed record UndoModify() : TableMutation;
        public sealed record Purge() : TableMutation;
        public sealed record ModifyMetadata(MetadataPatch Patch) : TableMutation;
        public sealed record UpdateSourceArchive(FileEndpoint Source, UpdatePolicy Policy, LinkReloadPolicy Reload) : TableMutation;
        public sealed record ReloadFromFile(FileEndpoint Source, LinkReloadPolicy Policy) : TableMutation;
        public sealed record Export(FileEndpoint Target) : TableMutation;

        public bool IncludeDeleted => this is Undelete;

        public Fin<Unit> Confirm(MutateCtx ctx) => Switch(ctx,
            rename: static (c, m) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Modify(
                idefIndex: c.Index, newName: m.NewName.Value,
                newDescription: c.Snap.Description.IfNone(noneValue: string.Empty), quiet: true)),
            delete: static (c, m) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Delete(
                idefIndex: c.Index, deleteReferences: m.Policy.DeleteReferences, quiet: m.Policy.Quiet)),
            undelete: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Undelete(idefIndex: c.Index)),
            undoModify: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.UndoModify(idefIndex: c.Index)),
            purge: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Purge(idefIndex: c.Index)),
            modifyMetadata: static (c, m) => m.Patch.IsEmpty
                ? Fin.Succ(value: unit)
                : CommitMetadata(table: c.Owner.Document.InstanceDefinitions, snap: c.Snap, patch: m.Patch, key: c.Key),
            updateSourceArchive: static (c, m) => CommitSourceArchive(
                owner: c.Owner, index: c.Index, source: m.Source, policy: m.Policy, reload: m.Reload, key: c.Key),
            reloadFromFile: static (c, m) => m.Source.Input(op: c.Key).Bind(endpoint => c.Key.Confirm(
                success: c.Owner.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: c.Index, filename: endpoint.Path,
                    updateNestedLinks: m.Policy.UpdateNestedLinks, quiet: m.Policy.Quiet))),
            export: static (c, m) => m.Target.Output(op: c.Key).Bind(endpoint => c.Key.Confirm(
                success: c.Owner.Document.InstanceDefinitions.Export(idefIndex: c.Index, filename: endpoint.Path))));

        public MutationReceipt Project(Definition snap) => this switch {
            Delete or Undelete or Purge => MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value),
            ModifyMetadata m => MutationReceipt.Of(
                receipt: MutationReceipt.Named(name: snap.Name.Value).Document,
                diagnostics: m.Patch.ModifyDiagnostics(id: snap.Id)),
            _ => MutationReceipt.Named(name: snap.Name.Value),
        };
    }

    // ---- [MANAGE] [MUTATION] -------------------------------------------------------------
    private static Fin<MutationReceipt> Mutate(RhinoBlocks owner, DefinitionRef refer, TableMutation mutation, Op key) =>
        from snap in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key, includeDeleted: mutation.IncludeDeleted).Map(static pair => pair.Snap)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in mutation.Confirm(ctx: new MutateCtx(Owner: owner, Index: idx, Snap: snap, Key: key))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: mutation.Project(snap: snap));

    private static Fin<MutationReceipt> PerformPurge(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => Mutate(owner: owner, refer: r, mutation: new TableMutation.Purge(), key: key),
            _ => PurgeAllUnused(owner: owner),
        };

    private static Fin<MutationReceipt> PurgeAllUnused(RhinoBlocks owner) {
        int purged = owner.Document.InstanceDefinitions.PurgeUnused();
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: $"<purged:{purged}>")),
            }));
    }

    private static Fin<MutationReceipt> PerformPurgeByPrefix(RhinoBlocks owner, DefinitionPrefix prefix, Op key) {
        Seq<DocumentResourceChange> changes = toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(d => d?.Name?.StartsWith(value: prefix.Value, comparisonType: StringComparison.OrdinalIgnoreCase) ?? false)
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
    private static Fin<MutationReceipt> PerformPlace(RhinoBlocks owner, DefinitionRef refer, Seq<Placement> at, BatchPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from live in snap.Live.ToFin(Fail: key.InvalidInput())
        from idx in RequireIndex(snap: snap, key: key)
        from admitted in at.TraverseM(placement => placement.Admit(live: live, key: key)).As()
        from receipts in PlaceInBatch(doc: owner.Document, index: idx, at: admitted, policy: policy, key: key)
        from _ in key.Confirm(success: !receipts.IsEmpty)
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
            Created = receipts,
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: snap.Name.Value)),
        });

    private static Fin<Guid> AddInstance(ObjectTable table, int index, Placement placement, Op key) {
        ObjectAttributes effective = placement.Attrs.IfNone(static () => new ObjectAttributes());
        HistoryRecord? history = placement.History.Case switch {
            HistoryRecord rec => rec,
            _ => null,
        };
        Guid id = table.AddInstanceObject(
            instanceDefinitionIndex: index,
            instanceXform: placement.Transform,
            attributes: effective,
            history: history,
            reference: placement.Reference);
        return key.Confirm(success: id != Guid.Empty).Map(_ => id);
    }

    private static Fin<Seq<Guid>> PlaceInBatch(RhinoDoc doc, int index, Seq<Placement> at, BatchPolicy policy, Op key) =>
        WithBatchRedraw(doc: doc, policy: policy, key: key, work: () =>
            at.Map(p => AddInstance(table: doc.Objects, index: index, placement: p, key: key))
                .TraverseM(identity).As());

    /// Snapshot prior block name before replace so the receipt emits one (or two when distinct) resource changes.
    private static Fin<MutationReceipt> PerformReplaceDefinition(RhinoBlocks owner, Guid instanceId, DefinitionRef newRef, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: newRef, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        let priorName = AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key).ToOption()
            .Bind(io => Optional(io.InstanceDefinition))
            .Bind(def => Optional(def.Name).Filter(static n => !string.IsNullOrWhiteSpace(value: n)))
        from _ in key.Confirm(success: owner.Document.Objects.ReplaceInstanceObject(objectId: instanceId, instanceDefinitionIndex: idx))
        let next = snap.Name.Value
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
            Replaced = Seq(instanceId),
            ResourceChanged = priorName.Case is string p && !string.Equals(a: p, b: next, comparisonType: StringComparison.OrdinalIgnoreCase)
                ? Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: p),
                      new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next))
                : Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: next)),
        });

    private static Fin<MutationReceipt> PerformTransformInstance(RhinoBlocks owner, Guid instanceId, Transform xform, TransformPolicy mode, Op key) =>
        key.Catch(() => {
            Guid resultId = mode.Apply(objects: owner.Document.Objects, instanceId: instanceId, xform: xform);
            return key.Confirm(success: resultId != Guid.Empty)
                .Map(_ => MutationReceipt.Of(receipt: mode.InstanceTransform(instanceId: instanceId, resultId: resultId)));
        });

    /// Native = AddExplodedInstancePieces (one-shot); other arms = Explode + Materialize + Delete.
    private static Fin<MutationReceipt> PerformExplodeIntoDocument(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        policy is ExplodePolicy.Native native
            ? from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
              let produced = toSeq(owner.Document.Objects.AddExplodedInstancePieces(
                  instance: instance, explodeNestedInstances: native.ExplodeNested, deleteInstance: native.DeleteInstance) ?? [])
              select MutationReceipt.Of(receipt: native.ExplodeReceipt(created: produced, instanceId: instanceId))
            : ExplodeManual(owner: owner, instanceId: instanceId, policy: policy, key: key);

    private static Fin<MutationReceipt> ExplodeManual(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = ExplodePieces(instance: instance, policy: policy)
        from produced in toSeq(pieces)
            .Filter(static p => p.Piece?.Geometry is not null)
            .Map(p => MaterializePiece(doc: owner.Document, piece: p, key: key))
            .TraverseM(identity).As()
        from deleted in key.Confirm(success: owner.Document.Objects.Delete(obj: instance, quiet: true))
        select MutationReceipt.Of(
            receipt: DocumentReceipt.Empty with { Created = produced, Deleted = Seq(instanceId) },
            diagnostics: pieces.Length == produced.Count
                ? Seq<BlockDiagnostic>()
                : Seq<BlockDiagnostic>(new BlockDiagnostic.ExplodePartial(Requested: pieces.Length, Received: produced.Count)));

    private static (RhinoObject Piece, Transform Transform)[] ExplodePieces(InstanceObject instance, ExplodePolicy policy) {
        (bool skips, Guid viewport) = policy.Resolve();
        instance.Explode(
            skipHiddenPieces: skips, viewportId: viewport,
            explodeNestedInstances: policy.IncludesNestedInstances,
            pieces: out RhinoObject[] pieces,
            pieceAttributes: out ObjectAttributes[] _,
            pieceTransforms: out Transform[] transforms);
        return [.. toSeq(pieces ?? []).Zip(toSeq(transforms ?? []))];
    }

    /// Nested InstanceObject re-emits as an Instance; other geometry duplicates + transforms + adds.
    private static Fin<Guid> MaterializePiece(RhinoDoc doc, (RhinoObject Piece, Transform Transform) piece, Op key) =>
        piece.Piece switch {
            InstanceObject nested => AddInstance(
                table: doc.Objects,
                index: nested.InstanceDefinition?.Index ?? -1,
                placement: Placement.Of(xform: piece.Transform, reference: false),
                key: key),
            _ => key.Catch(() => {
                GeometryBase g = piece.Piece.Geometry.Duplicate();
                _ = g.Transform(piece.Transform);
                Guid id = doc.Objects.Add(geometry: g, attributes: piece.Piece.Attributes.Duplicate());
                return key.Confirm(success: id != Guid.Empty).Map(_ => id);
            }),
        };

    // ---- [LINK] --------------------------------------------------------------------------
    private static Fin<MutationReceipt> PerformCreateArchiveLinks(RhinoBlocks owner, Seq<FileEndpoint> sources, LinkCreatePolicy policy, Op key) =>
        from admitted in policy.Admit(key: key)
        from changes in sources
            .Map(endpoint => key.Catch(() => CreateOneLink(owner: owner, endpoint: endpoint, policy: admitted, key: key)))
            .TraverseM(identity).As()
        select MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes });

    /// Resolve endpoint and attach; link attach uses full-path FileReference like native ModifySourceArchive callers.
    private static Fin<DocumentResourceChange> CreateOneLink(RhinoBlocks owner, FileEndpoint endpoint, LinkCreatePolicy policy, Op key) =>
        from source in endpoint.Input(op: key)
        from change in CreateAndAttach(owner: owner, source: source, policy: policy, key: key)
        select change;

    /// Add empty placeholder → ModifySourceArchive + reload → finalize layer style OR rollback.
    private static Fin<DocumentResourceChange> CreateAndAttach(RhinoBlocks owner, FileEndpoint source, LinkCreatePolicy policy, Op key) {
        string filename = source.Path;
        int idx = AddLinkPlaceholder(
            table: owner.Document.InstanceDefinitions,
            name: owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: IOPath.GetFileNameWithoutExtension(path: filename)));
        return idx < 0
            ? Fin.Fail<DocumentResourceChange>(error: key.InvalidResult(detail: nameof(InstanceDefinitionTable.Add)))
            : FinalizeAttachedLink(
                owner: owner, index: idx, source: source, policy: policy, filename: filename, key: key);
    }

    private static Fin<DocumentResourceChange> FinalizeAttachedLink(
        RhinoBlocks owner,
        int index,
        FileEndpoint source,
        LinkCreatePolicy policy,
        string filename,
        Op key) =>
        ModifyAndRefreshLinked(
                owner: owner, index: index, source: source,
                policy: policy.EffectiveUpdate,
                quiet: policy.EffectiveReload.Quiet,
                updateNestedLinks: policy.EffectiveReload.UpdateNestedLinks,
                loadPath: filename, key: key)
            .Match(
                Succ: _ => Fin.Succ(value: FinalizeLink(owner: owner, index: index, layer: policy.EffectiveLayer, filename: filename)),
                Fail: error => RollbackLink(owner: owner, idx: index, error: error));

    private static Fin<DocumentResourceChange> RollbackLink(RhinoBlocks owner, int idx, Error error) {
        _ = owner.Document.InstanceDefinitions.Delete(idefIndex: idx, deleteReferences: true, quiet: true);
        _ = owner.Document.InstanceDefinitions.Purge(idefIndex: idx);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Fail<DocumentResourceChange>(error);
    }

    private static DocumentResourceChange FinalizeLink(RhinoBlocks owner, int index, LayerStyle layer, string filename) {
        InstanceDefinition live = owner.Document.InstanceDefinitions[index];
        live.LayerStyle = layer.Native;
        _ = InvalidateDefinition(defId: live.Id, doc: Some(value: owner.Document));
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: live.Name ?? filename);
    }

    private static Fin<MutationReceipt> PerformLinkedFilter(
        RhinoBlocks owner,
        BlockFilter filter,
        Op key,
        Func<RhinoBlocks, InstanceDefinition, Op, Fin<DocumentResourceChange>> worker) {
        Option<string> anchor = BlockPaths.DocAnchor(document: owner.Document);
        return filter.Apply(table: owner.Document.InstanceDefinitions, anchorDirectory: anchor, policy: LinkRefreshPolicy.All)
            .TraverseM(definition => worker(owner, definition, key))
            .As()
            .Map(changes => MutationReceipt.Of(receipt: DocumentReceipt.Empty with { ResourceChanged = changes }));
    }

    private static Fin<MutationReceipt> PerformBatchRelink(RhinoBlocks owner, Seq<LinkMap> maps, BatchPolicy policy, Op key) =>
        WithBatchRedraw(doc: owner.Document, policy: policy, key: key, work: () => maps
            .Map(map => RelinkOne(owner: owner, map: map, key: key))
            .TraverseM(identity).As()
            .Map(static receipts => receipts.Fold(initialState: MutationReceipt.Empty, f: static (acc, receipt) => acc + receipt)));

    private static Fin<MutationReceipt> RelinkOne(RhinoBlocks owner, LinkMap map, Op key) =>
        from admitted in map.Admit(key: key)
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: admitted.Ref, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        from endpoint in admitted.Source.Input(op: key)
        from _ in CommitSourceArchive(
            owner: owner,
            index: idx,
            source: endpoint,
            policy: admitted.EffectiveUpdate,
            reload: admitted.EffectiveReload,
            key: key)
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: MutationReceipt.Named(name: snap.Name.Value));

    private static Fin<T> WithBatchRedraw<T>(RhinoDoc doc, BatchPolicy policy, Op key, Func<Fin<T>> work) {
        // BOUNDARY ADAPTER — RedrawEnabled must restore even when native batch mutation fails.
        bool prior = doc.Views.RedrawEnabled;
        if (policy.SuppressRedraw) doc.Views.RedrawEnabled = false;
        try {
            return Optional(work).ToFin(Fail: key.InvalidInput()).Bind(static run => run());
        } finally {
            if (policy.SuppressRedraw) {
                doc.Views.RedrawEnabled = prior;
                doc.Views.Redraw();
            }
        }
    }

    private static Fin<MutationReceipt> PerformRefreshLinks(RhinoBlocks owner, BlockFilter filter, LinkRefreshPolicy policy, Op key) {
        Option<string> anchor = BlockPaths.DocAnchor(document: owner.Document);
        return RefreshSources(doc: owner.Document, filter: filter, anchorDirectory: anchor, policy: policy, key: key)
            .Map(refreshed => MutationReceipt.Of(receipt: DocumentReceipt.Empty with {
                ResourceChanged = refreshed.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
            }));
    }

    private static Fin<DocumentResourceChange> ApplyLinkedLayerStyle(RhinoBlocks owner, InstanceDefinition definition, Op key) =>
        UpdatePolicy.FromNative(native: definition.UpdateType).RequireLinked(key: key).Bind(_ =>
            key.Catch(() => {
                definition.LayerStyle = InstanceDefinitionLayerStyle.Active;
                _ = InvalidateDefinition(defId: definition.Id, doc: Some(value: owner.Document));
                return Fin.Succ(value: new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: definition.Name ?? string.Empty));
            }));

    private static Fin<DocumentResourceChange> DetachLinkedDefinition(RhinoBlocks owner, InstanceDefinition definition, Op key) =>
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.DestroySourceArchive(definition: definition, quiet: true))
        let invalidated = InvalidateDefinition(defId: definition.Id, doc: Some(value: owner.Document))
        select new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: definition.Name ?? string.Empty);

    private static Fin<Seq<InstanceDefinition>> RefreshSources(RhinoDoc doc, BlockFilter filter, Option<string> anchorDirectory, LinkRefreshPolicy policy, Op key) =>
        from candidates in Fin.Succ(value: filter.Apply(table: doc.InstanceDefinitions, anchorDirectory: anchorDirectory, policy: policy))
        from ordered in Archive.LinkedRefreshOrder(table: doc.InstanceDefinitions, candidates: candidates, key: key)
        from refreshed in ordered
            .TraverseM(definition =>
                key.Confirm(success: doc.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                    .Map(_ => InvalidateWith(doc: doc, defId: definition.Id, value: definition)))
            .As()
        select refreshed;

    /// ModifySourceArchive attaches the archive path; UpdateLinkedInstanceDefinition loads geometry on first attach and after relink.
    private static Fin<Unit> CommitSourceArchive(
        RhinoBlocks owner,
        int index,
        FileEndpoint source,
        UpdatePolicy policy,
        LinkReloadPolicy reload,
        Op key) =>
        from _ in policy.RequireLinked(key: key)
        from endpoint in source.Input(op: key)
        from changed in ModifyAndRefreshLinked(
            owner: owner,
            index: index,
            source: endpoint,
            policy: policy,
            quiet: reload.Quiet,
            updateNestedLinks: reload.UpdateNestedLinks,
            loadPath: endpoint.Path,
            key: key)
        select changed;

    /// ModifySourceArchive attaches the link; UpdateLinkedInstanceDefinition + RefreshLinkedBlock together
    /// guarantee geometry is loaded after first attach OR relink.
    private static Fin<Unit> ConfirmAttached(Op key, InstanceDefinition live, bool modifyReported) =>
        live.UpdateType is InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded
        && Definition.NonBlank(value: live.SourceArchive).IsSome
            ? Fin.Succ(value: unit)
            : ConfirmNative(key: key, step: nameof(InstanceDefinitionTable.ModifySourceArchive), success: modifyReported);

    private static Fin<Unit> ModifyAndRefreshLinked(
        RhinoBlocks owner, int index, FileEndpoint source,
        UpdatePolicy policy, bool quiet, bool updateNestedLinks, string loadPath, Op key) =>
        source.WithReference(key: key, reference =>
            Fin.Succ(value: owner.Document.InstanceDefinitions.ModifySourceArchive(
                idefIndex: index, sourceArchive: reference, updateType: policy.Native, quiet: quiet))
            .Bind(reported => Optional(owner.Document.InstanceDefinitions[index]).ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinitionTable)))
                .Bind(live => ConfirmAttached(key: key, live: live, modifyReported: reported)))
            .Bind(_ => ReloadLinked(
                owner: owner, index: index, loadPath: loadPath, updateNestedLinks: updateNestedLinks, quiet: quiet, key: key)));

    /// UpdateLinkedInstanceDefinition reads model-space objects; RefreshLinkedBlock follows when counts stay zero.
    private static Fin<Unit> ReloadLinked(RhinoBlocks owner, int index, string loadPath, bool updateNestedLinks, bool quiet, Op key) =>
        Optional(owner.Document.InstanceDefinitions[index]).ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinitionTable)))
            .Bind(live => live.ObjectCount >= 1
                ? Fin.Succ(value: unit)
                : Fin.Succ(value: Definition.NonBlank(value: live.SourceArchive).IfNone(noneValue: loadPath))
                    .Bind(filename => Fin.Succ(value: owner.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                            idefIndex: index, filename: filename, updateNestedLinks: updateNestedLinks, quiet: quiet))
                        .Bind(_ => Optional(owner.Document.InstanceDefinitions[index]).ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinitionTable)))
                            .Bind(refreshed => refreshed.ObjectCount >= 1
                                ? Fin.Succ(value: unit)
                                : Fin.Succ(value: owner.Document.InstanceDefinitions.RefreshLinkedBlock(definition: refreshed))
                                    .Bind(__ => Optional(owner.Document.InstanceDefinitions[index]).ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinitionTable)))
                                        .Bind(final => final.ObjectCount >= 1
                                            ? Fin.Succ(value: unit)
                                            : ConfirmNative(
                                                key: key,
                                                step: nameof(InstanceDefinitionTable.RefreshLinkedBlock),
                                                success: false)))))));
    private static Fin<MutationReceipt> PerformExportAttributes(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from endpoint in target.Output(op: key)
        from receipt in RunBlockScript(owner: owner, snap: snap, command: "_-ExportBlockAttributes",
            args: Seq(("Definition", snap.Name.Value), ("File", endpoint.Path)), key: key)
        select receipt;

    /// SnapshotName VO strips forbidden chars at admission.
    private static Fin<MutationReceipt> PerformSnapshotBlock(RhinoBlocks owner, DefinitionRef refer, SnapshotName name, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from receipt in RunBlockScript(owner: owner, snap: snap, command: "_-Snapshot _Save",
            args: Seq(("Name", name.Value)), key: key)
        select receipt;

    /// Builds a single-line scripted command (token quoting + CR/LF stripping inline) and forwards through RhinoApp.RunScript.
    private static Fin<MutationReceipt> RunBlockScript(RhinoBlocks owner, Definition snap, string command, Seq<(string Name, string Value)> args, Op key) =>
        key.Confirm(success: RhinoApp.RunScript(
                documentSerialNumber: owner.Document.RuntimeSerialNumber,
                script: string.Join(separator: " ", values: (Seq(command)
                    + args.Map(static arg => $"_{arg.Name} \"{(arg.Value ?? string.Empty)
                        .Replace(oldValue: "\"", newValue: "\"\"", comparisonType: StringComparison.Ordinal)
                        .Replace(oldValue: "\r", newValue: " ", comparisonType: StringComparison.Ordinal)
                        .Replace(oldValue: "\n", newValue: " ", comparisonType: StringComparison.Ordinal)}\"")
                    + Seq("_Enter")).AsIterable()),
                echo: false))
            .Map(_ => MutationReceipt.Named(name: snap.Name.Value));

    // ---- [MANAGE] [QUERY] ----------------------------------------------------------------
    private static Fin<BlockOutcome> PerformAllocateName(RhinoBlocks owner, Option<string> seed, Op key) {
        InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
        string raw = seed.Case switch {
            string s => table.GetUnusedInstanceDefinitionName(root: s),
            _ => table.GetUnusedInstanceDefinitionName(),
        };
        return DefinitionName.From(value: raw, key: key).Map(static n => (BlockOutcome)new BlockOutcome.Name(Value: n));
    }

    private static Fin<BlockOutcome> PerformSnapshot(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: r, key: key)
                .Map(static snap => (BlockOutcome)new BlockOutcome.Snapshot(Value: snap)),
            _ => SnapshotAll(table: owner.Document.InstanceDefinitions, key: key),
        };

    private static Fin<MutationReceipt> PerformDuplicate(RhinoBlocks owner, DefinitionRef refer, Option<DefinitionName> name, ConflictPolicy conflict, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from provided in ReifyDefinitionMembers(definition: pair.Live, key: key)
        from target in name.Match(
            Some: Fin.Succ,
            None: () => DefinitionName.From(value: owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: pair.Snap.Name.Value), key: key))
        from admitted in AuthorSpec.Of(
            name: target,
            basePoint: Point3d.Origin,
            update: pair.Snap.Live.Map(static live => live.Update).IfNone(UpdatePolicy.Static),
            layer: pair.Snap.Live.Map(static live => live.Layer).IfNone(LayerStyle.None),
            metadata: MetadataPatch.Empty,
            key: key).Bind(s => s.Admit(key: key))
        from receipt in PerformAuthor(owner: owner, spec: admitted, source: provided, conflict: conflict, key: key)
        select receipt;

    private static Fin<MutationReceipt> PerformBakeArchive(RhinoBlocks owner, FileEndpoint source, BakePolicy policy, Op key) =>
        from endpoint in source.Input(op: key)
        from model in key.Catch(() => Optional(File3dm.Read(path: endpoint.Path)).ToFin(Fail: key.InvalidInput()))
        from closure in Archive.LinkedArchiveClosure(root: model, rootPath: endpoint.Path, key: key)
        from graph in Archive.From(model: model, archivePath: Some(endpoint.Path), key: key)
        from linked in BakeArchiveLinks(owner: owner, closure: closure, policy: policy, key: key)
        from authored in BakeArchiveDefinitions(owner: owner, model: model, graph: graph, policy: policy, key: key)
        from placed in policy.RestoreInstancesWhen(place: () => BakeArchiveInstances(owner: owner, graph: graph, key: key))
        select linked + authored + placed;

    private static Fin<MutationReceipt> BakeArchiveLinks(RhinoBlocks owner, Seq<Archive.LinkedArchiveEdge> closure, BakePolicy policy, Op key) =>
        toSeq(closure
            .Filter(static edge => edge.Depth == 0)
            .Map(static edge => edge.Link)
            .DistinctBy(static link => link.Full.Value))
            .TraverseM(link => link.ToEndpoint())
            .As()
            .Bind(sources => sources.IsEmpty
                ? Fin.Succ(value: MutationReceipt.Empty)
                : PerformCreateArchiveLinks(owner: owner, sources: sources, policy: policy.EffectiveLink, key: key));

    private static Fin<MutationReceipt> BakeArchiveDefinitions(RhinoBlocks owner, File3dm model, Archive.Graph graph, BakePolicy policy, Op key) {
        Seq<Definition> plan = Archive.BakePlan(graph: graph).Filter(static def => !def.IsLinked);
        return plan.Fold(
            Fin.Succ(value: new BakeArchiveState(Receipt: MutationReceipt.Empty, LiveByArchiveId: HashMap<Guid, Guid>())),
            (stateFin, definition) => stateFin.Bind(state =>
                BakeArchiveDefinition(
                        owner: owner,
                        model: model,
                        definition: definition,
                        policy: policy,
                        liveByArchiveId: state.LiveByArchiveId,
                        key: key)
                    .Map(step => state with {
                        Receipt = state.Receipt + step.Receipt,
                        LiveByArchiveId = step.LiveByArchiveId,
                    })))
            .Map(finalState => finalState.Receipt);
    }

    /// Carries fold accumulator (receipt monoid + archive→live id map) through the bake plan.
    private sealed record BakeArchiveState(MutationReceipt Receipt, HashMap<Guid, Guid> LiveByArchiveId);

    private static Fin<BakeArchiveState> BakeArchiveDefinition(
        RhinoBlocks owner, File3dm model, Definition definition, BakePolicy policy,
        HashMap<Guid, Guid> liveByArchiveId, Op key) =>
        from members in ReifyArchiveMembers(model: model, definition: definition, liveByArchiveId: liveByArchiveId, key: key)
        from admitted in AuthorSpec.Of(
            name: definition.Name,
            basePoint: Point3d.Origin,
            update: definition.Live.Map(static live => live.Update).IfNone(UpdatePolicy.Static),
            layer: definition.Live.Map(static live => live.Layer).IfNone(LayerStyle.None),
            metadata: MetadataPatch.Empty,
            key: key).Bind(s => s.Admit(key: key))
        from authored in PerformAuthor(owner: owner, spec: admitted, source: members, conflict: policy.EffectiveConflict, key: key)
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(definition.Name), key: key)
        select new BakeArchiveState(
            Receipt: authored,
            LiveByArchiveId: liveByArchiveId.AddOrUpdate(key: definition.Id.Value, value: snap.Id.Value));

    private static Fin<Members> ReifyArchiveMembers(File3dm model, Definition definition, HashMap<Guid, Guid> liveByArchiveId, Op key) =>
        toSeq(definition.MemberIds)
            .Map(id => Optional(model.Objects.FindId(id: id)).ToFin(Fail: key.InvalidInput()))
            .TraverseM(identity).As()
            .Bind(objs => Members.OfProvided(
                geometry: objs.Map(o => o.Geometry switch {
                    InstanceReferenceGeometry r => liveByArchiveId.Find(key: r.ParentIdefId).Case is Guid liveId
                        ? new InstanceReferenceGeometry(instanceDefinitionId: liveId, transform: r.Xform)
                        : r.Duplicate(),
                    _ => o.Geometry.Duplicate(),
                }),
                attributes: objs.Map(static o => o.Attributes.Duplicate()),
                key: key))
            .Map(static provided => (Members)provided);

    /// Place each archive instance via name-resolved live definition; receipts fold into a single mutation.
    private static Fin<MutationReceipt> BakeArchiveInstances(RhinoBlocks owner, Archive.Graph graph, Op key) =>
        graph.Instances.IsEmpty
            ? Fin.Succ(value: MutationReceipt.Empty)
            : toSeq(graph.Instances)
                .Map(instance =>
                    from archiveDef in toSeq(graph.Definitions).Find(d => d.Id == instance.ParentDefId).ToFin(Fail: key.InvalidInput())
                    from live in Optional(owner.Document.InstanceDefinitions.Find(instanceDefinitionName: archiveDef.Name.Value)).ToFin(Fail: key.InvalidInput())
                    from idx in DefinitionIndex.From(value: live.Index, key: key)
                    from created in AddInstance(table: owner.Document.Objects, index: idx.Value, placement: Placement.Of(xform: instance.Xform), key: key)
                    select MutationReceipt.Of(receipt: DocumentReceipt.Empty with { Created = Seq(created) }))
                .TraverseM(identity).As()
                .Map(receipts => receipts.Fold(initialState: MutationReceipt.Empty, f: static (acc, r) => acc + r));

    private static Fin<BlockOutcome> SnapshotAll(InstanceDefinitionTable table, Op key) {
        Option<string> anchor = BlockPaths.DocAnchor(document: table.Document);
        return toSeq(table.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Map(d => Definition.From(definition: d!, anchorDirectory: anchor, key: key))
            .TraverseM(identity).As()
            .Map(static seq => (BlockOutcome)new BlockOutcome.Snapshots(Values: seq));
    }

    // ---- [PLACE] [QUERY] -----------------------------------------------------------------
    private static Fin<BlockOutcome> PerformExplodeInspect(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = ExplodePieces(instance: instance, policy: policy)
        from defId in DefinitionId.From(value: instance.InstanceDefinition?.Id ?? Guid.Empty, key: key)
        let members = toSeq(pieces)
            .Filter(static p => p.Piece is not null)
            .Map(p => new Member(DefId: defId, MemberId: p.Piece.Id, Attrs: Optional(p.Piece.Attributes)))
        select (BlockOutcome)new BlockOutcome.MembersResult(Values: members);

    private static Fin<BlockOutcome> PerformUseSubObject(RhinoBlocks owner, Guid instanceId, ComponentIndex component, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from valid in component is { IsSet: true, ComponentIndexType: ComponentIndexType.InstanceDefinitionPart }
            ? Fin.Succ(value: component)
            : Fin.Fail<ComponentIndex>(error: key.InvalidInput())
        from sub in Optional(instance.SubObjectFromComponentIndex(ci: valid)).ToFin(Fail: key.InvalidInput())
        from defId in DefinitionId.From(value: instance.InstanceDefinition?.Id ?? Guid.Empty, key: key)
            // SubObjectFromComponentIndex returns definition-local geometry; InstanceXform is the insertion transform.
        select (BlockOutcome)new BlockOutcome.Pieces(Values: Seq(
            new FlatPiece(
                Geometry: sub.Geometry,
                Composed: instance.InstanceXform,
                Path: [defId])));

    // ---- [GRAPH] -------------------------------------------------------------------------
    private static Fin<BlockOutcome> PerformGraph(RhinoBlocks owner, GraphQuery query, Op key) =>
        query.Switch(
            ctx: (Owner: owner, Key: key),
            members: static (ctx, m) => PerformGraphMembers(owner: ctx.Owner, refer: m.Ref, key: ctx.Key),
            inserts: static (ctx, i) => PerformGraphInserts(owner: ctx.Owner, refer: i.Ref, scope: i.Scope, key: ctx.Key),
            containers: static (ctx, c) => PerformGraphContainers(owner: ctx.Owner, refer: c.Ref, key: ctx.Key),
            depends: static (ctx, d) => PerformDependsOn(owner: ctx.Owner, refer: d.Ref, target: d.Target, key: ctx.Key),
            audit: static (ctx, _) => PerformDependencyAudit(owner: ctx.Owner, key: ctx.Key),
            plan: static (ctx, p) => PerformGraphPlan(owner: ctx.Owner, root: p.Root, key: ctx.Key),
            stats: static (ctx, _) => PerformStats(owner: ctx.Owner, key: ctx.Key),
            health: static (ctx, h) => PerformLinkedHealth(owner: ctx.Owner, filter: h.Filter ?? BlockFilter.All, key: ctx.Key),
            reach: static (ctx, r) => PerformGraphReach(owner: ctx.Owner, refer: r.Ref, scope: r.Scope, policy: r.Policy, key: ctx.Key),
            ensureIndexed: static (ctx, e) => PerformGraphEnsureIndexed(owner: ctx.Owner, refer: e.Ref, key: ctx.Key));

    private static Fin<BlockOutcome> PerformStats(RhinoBlocks owner, Op key) {
        InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
        return key.Catch(() => Fin.Succ(value: (BlockOutcome)new BlockOutcome.TableStats(
            Count: table.Count,
            ActiveCount: table.ActiveCount)));
    }

    private static Fin<BlockOutcome> PerformGraphEnsureIndexed(RhinoBlocks owner, Option<DefinitionRef> refer, Op key) =>
        refer.Case switch {
            DefinitionRef r => Resolve(table: owner.Document.InstanceDefinitions, refer: r, key: key)
                .Map(pair => {
                    _ = ContentIndex.RegisterDefinition(doc: owner.Document, defId: pair.Live.Id);
                    return (BlockOutcome)new BlockOutcome.Definitions(Values: Seq(pair.Snap.Id));
                }),
            _ => Fin.Succ(value: (BlockOutcome)new BlockOutcome.Definitions(Values:
                toSeq(owner.Document.InstanceDefinitions.GetList(ignoreDeleted: true))
                    .Filter(static d => d is not null)
                    .Choose(d => {
                        _ = ContentIndex.RegisterDefinition(doc: owner.Document, defId: d!.Id);
                        return DefinitionId.From(value: d!.Id, key: key).ToOption();
                    }))),
        };

    private static Fin<BlockOutcome> PerformGraphPlan(RhinoBlocks owner, Option<DefinitionRef> root, Op key) =>
        from anchor in root.Case switch {
            DefinitionRef refer => ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key).Map(Some),
            _ => Fin.Succ(Option<Definition>.None),
        }
        from graph in Archive.LiveGraph(table: owner.Document.InstanceDefinitions, anchor: anchor, key: key)
        select (BlockOutcome)new BlockOutcome.Plan(
            Order: Archive.BakePlan(graph: graph).Map(definition => definition.Id));

    private static Fin<BlockOutcome> PerformGraphReach(RhinoBlocks owner, DefinitionRef refer, ReferenceScope scope, DepthPolicy? policy, Op key) =>
        from admitted in (policy ?? DepthPolicy.Reach).Admit(key: key)
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        select (BlockOutcome)new BlockOutcome.ReachInserts(Values:
            toSeq(pair.Live.GetReferences(wheretoLook: scope.Native) ?? [])
                .Filter(static inst => inst is not null)
                .Bind(inst => TreeWalkReach(
                    table: owner.Document.InstanceDefinitions,
                    instance: inst!,
                    parent: Transform.Identity,
                    path: [],
                    depth: 0,
                    policy: admitted,
                    key: key)));

    private readonly record struct TreeWalkFrame(
        Transform Composed,
        ImmutableArray<DefinitionId> Path,
        int Depth,
        Guid InstanceId);

    private static Seq<ReachInsert> TreeWalkReach(
        InstanceDefinitionTable table,
        InstanceObject instance,
        Transform parent,
        ImmutableArray<DefinitionId> path,
        int depth,
        DepthPolicy policy,
        Op key) =>
        Optional(instance.InstanceDefinition)
            .Map(def => TreeExpandReach(
                table: table,
                def: def,
                frame: new TreeWalkFrame(Composed: parent * instance.InstanceXform, Path: path, Depth: depth, InstanceId: instance.Id),
                policy: policy,
                key: key))
            .IfNone(Seq<ReachInsert>());

    private static Seq<ReachInsert> TreeExpandReach(
        InstanceDefinitionTable table,
        InstanceDefinition def,
        TreeWalkFrame frame,
        DepthPolicy policy,
        Op key) =>
        DefinitionId.From(value: def.Id, key: key).ToOption().Map(defId => {
            ImmutableArray<DefinitionId> nextPath = frame.Path.Add(item: defId);
            ReachInsert self = new(
                InstanceId: frame.InstanceId,
                DefId: defId,
                WorldXform: frame.Composed,
                Depth: frame.Depth,
                Path: nextPath);
            return (policy.StopOnCycle && frame.Path.Contains(defId)) || frame.Depth >= policy.MaxDepth
                ? Seq(self)
                : Seq(self) + BindDefinitionMembers(
                    def: def,
                    composed: frame.Composed,
                    onInstance: (nested, parentXform) => TreeWalkReach(
                        table: table,
                        instance: nested,
                        parent: parentXform,
                        path: nextPath,
                        depth: frame.Depth + 1,
                        policy: policy,
                        key: key),
                    onReference: (reference, memberId, parentXform) =>
                        Optional(table.Find(instanceId: reference.ParentIdefId, ignoreDeletedInstanceDefinitions: true))
                            .Map(nestedDef => TreeExpandReach(
                                table: table,
                                def: nestedDef,
                                frame: new TreeWalkFrame(
                                    Composed: parentXform * reference.Xform,
                                    Path: nextPath,
                                    Depth: frame.Depth + 1,
                                    InstanceId: memberId),
                                policy: policy,
                                key: key))
                            .IfNone(Seq<ReachInsert>()));
        }).IfNone(Seq<ReachInsert>());

    /// ExpandNested unions top + nested instance bounds; otherwise unions definition-local geometry only.
    private static Fin<BlockOutcome> PerformBounds(RhinoBlocks owner, DefinitionRef refer, BoundsPolicy policy, Op key) =>
        FindDefinition(table: owner.Document.InstanceDefinitions, refer: refer, includeDeleted: false, key: key)
            .Map(live => (BlockOutcome)new BlockOutcome.Bounds(Value: policy.ExpandNested
                ? toSeq(live.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native))
                    .Filter(static i => i is not null)
                    .Fold(BoundingBox.Empty, (acc, inst) => BoundingBox.Union(
                        acc,
                        inst!.Geometry?.GetBoundingBox(xform: inst.InstanceXform) ?? BoundingBox.Empty))
                : toSeq(live.GetObjects())
                    .Filter(static o => o?.Geometry is not null)
                    .Map(static o => o!.Geometry!)
                    .Fold(BoundingBox.Empty, (acc, g) => BoundingBox.Union(acc, g.GetBoundingBox(accurate: policy.Accurate)))));

    private static Fin<BlockOutcome> PerformGraphMembers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from live in FindDefinition(table: owner.Document.InstanceDefinitions, refer: refer, includeDeleted: false, key: key)
        from defId in DefinitionId.From(value: live.Id, key: key)
        let members = toSeq(live.GetObjects())
            .Filter(static obj => obj is not null)
            .Map(obj => new Member(DefId: defId, MemberId: obj.Id, Attrs: Optional(obj.Attributes)))
        select (BlockOutcome)new BlockOutcome.MembersResult(Values: members);

    private static Fin<BlockOutcome> PerformGraphInserts(RhinoBlocks owner, DefinitionRef refer, ReferenceScope scope, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from inserts in toSeq(pair.Live.GetReferences(wheretoLook: scope.Native))
            .Filter(static inst => inst is not null)
            .Map(static inst => Insert.From(instance: inst))
            .TraverseM(identity).As()
        select (BlockOutcome)new BlockOutcome.Inserts(Values: inserts);

    private static Fin<BlockOutcome> PerformGraphContainers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from containers in toSeq(pair.Live.GetContainers())
            .Filter(static c => c is not null)
            .Map(c => DefinitionId.From(value: c.Id, key: key))
            .TraverseM(identity).As()
        select (BlockOutcome)new BlockOutcome.Definitions(Values: containers);

    private static Fin<BlockOutcome> PerformSelectedPart(RhinoBlocks owner, ObjRef refer, Op key) =>
        from instance in Optional(refer.Object() as InstanceObject).ToFin(Fail: key.InvalidInput())
        from definition in Optional(instance.InstanceDefinition).ToFin(Fail: key.InvalidResult())
        from member in Optional(refer.InstanceDefinitionPart()).Filter(static part => part.Attributes.IsInstanceDefinitionObject).ToFin(Fail: key.InvalidResult())
        from defId in DefinitionId.From(value: definition.Id, key: key)
        select (BlockOutcome)new BlockOutcome.MembersResult(Values: Seq(new Member(DefId: defId, MemberId: member.Id, Attrs: Optional(member.Attributes))));

    private static Fin<BlockOutcome> PerformDependencyAudit(RhinoBlocks owner, Op key) =>
        Archive.Audit(table: owner.Document.InstanceDefinitions, key: key)
            .Map(graph => (BlockOutcome)new BlockOutcome.Graphs(Value: graph));

    private static Fin<BlockOutcome> PerformLinkedHealth(RhinoBlocks owner, BlockFilter filter, Op key) {
        Option<string> anchor = BlockPaths.DocAnchor(document: owner.Document);
        Seq<LinkedHealth> items = filter.Apply(table: owner.Document.InstanceDefinitions, anchorDirectory: anchor, policy: LinkRefreshPolicy.All)
            .Choose(definition => Definition.From(definition: definition, anchorDirectory: anchor, key: key).ToOption().Bind(static def => def.ToLinkedHealth()));
        return Fin.Succ(value: (BlockOutcome)new BlockOutcome.Refresh(Value: new RefreshPlan(Items: items)));
    }

    private static Fin<BlockOutcome> PerformDependsOn(RhinoBlocks owner, DefinitionRef refer, DependencyTarget target, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        select (BlockOutcome)new BlockOutcome.Probed(Value: target.ProbeOn(live: pair.Live));

    private static Fin<BlockOutcome> PerformFlatten(RhinoBlocks owner, Guid instanceId, DepthPolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from admitted in policy.Admit(key: key)
        let pieces = TreeWalkFlat(
            table: owner.Document.InstanceDefinitions,
            instance: instance,
            parent: Transform.Identity,
            path: [],
            depth: 0,
            policy: admitted,
            key: key)
        select (BlockOutcome)new BlockOutcome.Pieces(Values: pieces);

    private static Seq<FlatPiece> TreeWalkFlat(
        InstanceDefinitionTable table,
        InstanceObject instance,
        Transform parent,
        ImmutableArray<DefinitionId> path,
        int depth,
        DepthPolicy policy,
        Op key) =>
        (depth >= policy.MaxDepth, instance.InstanceDefinition) switch {
            (true, _) or (_, null) => Seq<FlatPiece>(),
            (_, InstanceDefinition def) => TreeExpandFlat(
                table: table,
                def: def,
                frame: new TreeWalkFrame(Composed: parent * instance.InstanceXform, Path: path, Depth: depth + 1, InstanceId: instance.Id),
                policy: policy,
                key: key),
        };

    private static ImmutableArray<DefinitionId> AppendPath(ImmutableArray<DefinitionId> path, Guid id, Op key) =>
        DefinitionId.From(value: id, key: key).ToOption() switch {
            { IsSome: true, Case: DefinitionId defId } => path.Add(item: defId),
            _ => path,
        };

    private static Seq<FlatPiece> TreeExpandFlat(
        InstanceDefinitionTable table,
        InstanceDefinition def,
        TreeWalkFrame frame,
        DepthPolicy policy,
        Op key) =>
        BindDefinitionMembers(
            def: def,
            composed: frame.Composed,
            onInstance: (nested, parentXform) => TreeWalkFlat(
                table: table,
                instance: nested,
                parent: parentXform,
                path: frame.Path,
                depth: frame.Depth,
                policy: policy,
                key: key),
            onReference: (reference, _, parentXform) =>
                Optional(table.Find(instanceId: reference.ParentIdefId, ignoreDeletedInstanceDefinitions: true))
                    .Map(nestedDef => TreeExpandFlat(
                        table: table,
                        def: nestedDef,
                        frame: new TreeWalkFrame(
                            Composed: parentXform * reference.Xform,
                            Path: AppendPath(path: frame.Path, id: nestedDef.Id, key: key),
                            Depth: frame.Depth,
                            InstanceId: frame.InstanceId),
                        policy: policy,
                        key: key))
                    .IfNone(Seq<FlatPiece>()),
            onLeaf: (leaf, parentXform) => leaf.Geometry is GeometryBase g
                ? Seq(new FlatPiece(Geometry: g, Composed: parentXform, Path: frame.Path))
                : Seq<FlatPiece>());

    // ---- [OBSERVE] [QUERY] ---------------------------------------------------------------
    private static Fin<BlockOutcome> PerformPreview(RhinoBlocks owner, DefinitionRef refer, PreviewSpec spec, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from admitted in spec.Admit(key: key)
        from handle in owner.AcquirePreview(definition: pair.Live, spec: admitted, key: key)
        select (BlockOutcome)new BlockOutcome.Preview(Handle: handle);

    /// instanceToken == blockName when no specific instance is targeted (definition-scope fields).
    private static Fin<BlockOutcome> PerformTextFields(RhinoBlocks owner, DefinitionRef refer, Option<Guid> instanceId, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let blockName = pair.Snap.Name.Value
        let instanceToken = instanceId.Map(static id => id.ToString()).IfNone(noneValue: blockName)
        let fields = (toSeq([
                ("BlockName", TextFields.BlockName(blockName)),
                ("BlockInstanceCount", TextFields.BlockInstanceCount(blockName).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                ("BlockDescription", TextFields.BlockDescription(blockName)),
                ("BlockInstanceName", TextFields.BlockInstanceName(instanceToken)),
                ("InsertionX", TextFields.BlockInsertionCoordinate(instanceToken, Axis.X.Value)),
                ("InsertionY", TextFields.BlockInsertionCoordinate(instanceToken, Axis.Y.Value)),
                ("InsertionZ", TextFields.BlockInsertionCoordinate(instanceToken, Axis.Z.Value)),
            ]) + toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
                .Map(field => ($"Attribute:{field.Key}", TextFields.BlockAttributeText(key: field.Key, prompt: field.Prompt, defaultValue: field.DefaultValue))))
            .Fold(HashMap<string, string>(), static (m, p) => m.AddOrUpdate(key: p.Item1, value: p.Item2))
        select (BlockOutcome)new BlockOutcome.Texts(Fields: fields);

    private static Fin<BlockOutcome> PerformAttributeFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
        select (BlockOutcome)new BlockOutcome.Attributes(Values: fields);

    private static Fin<BlockOutcome> PerformAttributeMatrix(RhinoBlocks owner, Option<Seq<DefinitionRef>> refs, ReferenceScope scope, Op key) {
        InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
        return (refs.Case switch {
            Seq<DefinitionRef> selected => selected.Map(refer => Resolve(table: table, refer: refer, key: key)).TraverseM(identity).As(),
            _ => toSeq(table.GetList(ignoreDeleted: true))
                .Filter(static d => d is not null)
                .Map(live => SnapshotFromLive(table: table, live: live, key: key).Map(snap => (Snap: snap, Live: live)))
                .TraverseM(identity).As(),
        }).Map(pairs => (BlockOutcome)new BlockOutcome.AttributeMatrix(Values: pairs.Bind(pair => AttributeCells(pair: pair, scope: scope))));
    }

    private static Seq<AttributeCell> AttributeCells((Definition Snap, InstanceDefinition Live) pair, ReferenceScope scope) {
        Seq<TextFields.InstanceAttributeField> fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? []);
        Seq<InstanceObject> refs = toSeq(pair.Live.GetReferences(wheretoLook: scope.Native) ?? [])
            .Filter(static instance => instance is not null)
            .ToSeq();
        Seq<Option<Guid>> instances = refs.IsEmpty
            ? Seq<Option<Guid>>(None)
            : refs.Map(static instance => Some(value: instance.Id));
        return fields.Bind(field => instances.Map(instance => new AttributeCell(
            DefId: pair.Snap.Id,
            DefName: pair.Snap.Name,
            InstanceId: instance,
            Key: field.Key,
            Prompt: field.Prompt,
            DefaultValue: field.DefaultValue)));
    }

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
        DateTimeOffset now = ctx.Policy.EffectiveClock.GetUtcNow();
        if (now - ctx.LastFired.Value < ctx.Policy.Debounce) return;
        _ = ctx.LastFired.Swap(f: _ => now);
        _ = IdlePump.Enqueue(document: ctx.Owner.Document, work: doc => RefreshArchive(doc: doc, path: ctx.Path));
    }

    private static Fin<DocumentReceipt> RefreshArchive(RhinoDoc doc, ArchivePath path) {
        Op key = Op.Of(name: nameof(RefreshArchive));
        Option<string> anchor = BlockPaths.DocAnchor(document: doc);
        BlockFilter filter = BlockFilter.ArchivesOnly(Seq(path.Value));
        return RefreshSources(doc: doc, filter: filter, anchorDirectory: anchor, policy: LinkRefreshPolicy.All, key: key)
            .Map(refreshed => DocumentReceipt.Empty with {
                ResourceChanged = refreshed.Map(static d => new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: d.Name ?? string.Empty)),
            });
    }

    private sealed record WatchContext(
        RhinoBlocks Owner,
        ArchivePath Path,
        WatchPolicy Policy,
        Atom<DateTimeOffset> LastFired,
        FileSystemWatcher Watcher);

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
                m.AddOrUpdate(key: serial, value: HashMap<ulong, Seq<DefinitionId>>().AddOrUpdate(key: hash.Value, value: Seq(defId))),
        });
        return unit;
    }

    internal static Unit RegisterDefinition(RhinoDoc doc, Guid defId) =>
        DefinitionId.From(value: defId, key: Op.Of(name: nameof(RegisterDefinition))).ToOption()
            .Bind(id => Optional(doc.InstanceDefinitions.Find(instanceId: id.Value, ignoreDeletedInstanceDefinitions: true))
                .Bind(active => HashEntry(doc: doc, active: active)))
            .Map(entry => {
                _ = RemoveDefinition(serial: doc.RuntimeSerialNumber, defId: entry.Value);
                _ = RegisterIfMissing(doc: doc, hash: BlockContentHash.Create(value: entry.Key), defId: entry.Value);
                return unit;
            })
            .IfNone(unit);

    internal static Unit Invalidate(uint serial, RhinoDoc? doc, BlockTableEvent snapshot) {
        _ = Seq(snapshot.Old, snapshot.New)
            .Choose(static candidate => candidate)
            .Map(static definition => definition.Id)
            .Distinct()
            .Iter(defId => RemoveDefinition(serial: serial, defId: defId));
        return Optional(doc)
            .Map(active => snapshot.New
                .Map(definition => RegisterDefinition(doc: active, defId: definition.Id.Value))
                .IfNone(unit))
            .IfNone(unit);
    }

    internal static Unit RemoveDefinition(uint serial, DefinitionId defId) {
        _ = byDoc.Swap(f: m => m.Find(key: serial) switch {
            { IsSome: true, Case: HashMap<ulong, Seq<DefinitionId>> existing } => m.AddOrUpdate(
                key: serial,
                value: existing
                    .Map((hash, ids) => (Hash: hash, Ids: ids.Filter(id => id != defId)))
                    .Filter(static entry => !entry.Ids.IsEmpty)
                    .Fold(HashMap<ulong, Seq<DefinitionId>>(), static (acc, entry) => acc.AddOrUpdate(key: entry.Hash, value: entry.Ids))),
            _ => m,
        });
        return unit;
    }

    internal static Unit EvictDoc(uint serial) {
        _ = byDoc.Swap(f: m => m.Remove(key: serial));
        return unit;
    }

    /// Cold seed folds the live table once; table events keep the index aligned via RegisterDefinition.
    private static HashMap<ulong, Seq<DefinitionId>> Ensure(RhinoDoc doc, uint serial) =>
        byDoc.Value.Find(key: serial).IfNone(() => {
            HashMap<ulong, Seq<DefinitionId>> seeded = FoldHashIndex(doc: doc);
            _ = byDoc.Swap(f: m => m.AddOrUpdate(key: serial, value: seeded));
            return seeded;
        });

    private static HashMap<ulong, Seq<DefinitionId>> FoldHashIndex(RhinoDoc doc) =>
        toSeq(doc.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(static d => d is not null)
            .Choose(d => HashEntry(doc: doc, active: d))
            .Fold(HashMap<ulong, Seq<DefinitionId>>(), static (m, kv) => m.AddOrUpdate(
                key: kv.Key,
                value: m.Find(key: kv.Key).IfNone(noneValue: Seq<DefinitionId>()).Add(value: kv.Value).Distinct()));

    private static Option<(ulong Key, DefinitionId Value)> HashEntry(RhinoDoc doc, InstanceDefinition active) =>
        DefinitionId.From(value: active.Id).ToOption()
            .Bind(id => Operations.ReifyDefinitionMembers(definition: active, key: Op.Of(name: nameof(ContentIndex))).ToOption()
                .Bind(provided => Op.Of(name: nameof(ContentIndex)).Catch(() => Fin.Succ(value: BlockContentHash.Of(members: provided).Value)).ToOption()
                    .Map(hash => (Key: hash, Value: id))));
}
