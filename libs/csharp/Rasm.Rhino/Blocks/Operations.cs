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
public enum BlockOpPhase { Mutation, Query }
public enum BlockOpFamily { Definition, Table, Instance, Linked, Archive, Graph, Attributes, Preview }

public readonly record struct BlockOpMeta(string Name, BlockOpPhase Phase, BlockOpFamily Family, bool RequiresUiThread);

[Union(SwitchMapStateParameterName = "owner")]
public abstract partial record BlockOp {
    private BlockOp() { }
    public sealed record Author(AuthorSpec Spec, Members Source, ConflictPolicy Conflict) : BlockOp;
    public sealed record AddOrReuse(AuthorSpec Spec, Members Source) : BlockOp;
    public sealed record Modify(DefinitionRef Ref, Option<Members> Source = default, MetadataPatch? Patch = null) : BlockOp;
    public sealed record Table(DefinitionRef Ref, TableMutation Mutation) : BlockOp;
    public sealed record Purge(Option<DefinitionRef> Ref = default, Option<DefinitionPrefix> Prefix = default) : BlockOp;
    public sealed record Compact(CompactPolicy? Policy = null) : BlockOp;
    public sealed record Instance(BlockInstanceTask Task) : BlockOp;
    public sealed record Linked(LinkLifecycle Change) : BlockOp;
    public sealed record BakeArchive(FileEndpoint Source, BakePolicy? Policy = null) : BlockOp;
    public sealed record ValidateArchiveClosure(FileEndpoint Source, ClosureValidationPolicy? Policy = null) : BlockOp;
    public sealed record ExportAttributes(DefinitionRef Ref, FileEndpoint Target) : BlockOp;
    public sealed record SnapshotBlock(DefinitionRef Ref, SnapshotName Name) : BlockOp;
    public sealed record AllocateName(Option<string> Seed) : BlockOp;
    public sealed record Snapshot(Option<DefinitionRef> Ref) : BlockOp;
    public sealed record Duplicate(DefinitionRef Ref, Option<DefinitionName> Name, ConflictPolicy? Conflict = null) : BlockOp;
    public sealed record Graph(GraphQuery Query) : BlockOp;
    public sealed record AdoptContent() : BlockOp;
    public sealed record Preview(DefinitionRef Ref, PreviewSpec Spec) : BlockOp;
    public sealed record Attributes(BlockAttributeTask Task) : BlockOp;

    public BlockOpMeta Metadata => this switch {
        Author => Meta(nameof(Author), BlockOpPhase.Mutation, BlockOpFamily.Definition, ui: true),
        AddOrReuse => Meta(nameof(AddOrReuse), BlockOpPhase.Mutation, BlockOpFamily.Definition, ui: true),
        Modify => Meta(nameof(Modify), BlockOpPhase.Mutation, BlockOpFamily.Definition, ui: true),
        Table => Meta(nameof(Table), BlockOpPhase.Mutation, BlockOpFamily.Table, ui: true),
        Purge => Meta(nameof(Purge), BlockOpPhase.Mutation, BlockOpFamily.Table, ui: true),
        Compact => Meta(nameof(Compact), BlockOpPhase.Mutation, BlockOpFamily.Table, ui: true),
        Instance task => task.Task.OpMeta,
        Linked link => link.Change.OpMeta,
        BakeArchive => Meta(nameof(BakeArchive), BlockOpPhase.Mutation, BlockOpFamily.Archive, ui: true),
        ValidateArchiveClosure => Meta(nameof(ValidateArchiveClosure), BlockOpPhase.Query, BlockOpFamily.Archive, ui: false),
        ExportAttributes => Meta(nameof(ExportAttributes), BlockOpPhase.Mutation, BlockOpFamily.Attributes, ui: true),
        SnapshotBlock => Meta(nameof(SnapshotBlock), BlockOpPhase.Mutation, BlockOpFamily.Archive, ui: true),
        AllocateName => Meta(nameof(AllocateName), BlockOpPhase.Query, BlockOpFamily.Definition, ui: false),
        Snapshot => Meta(nameof(Snapshot), BlockOpPhase.Query, BlockOpFamily.Definition, ui: false),
        Duplicate => Meta(nameof(Duplicate), BlockOpPhase.Mutation, BlockOpFamily.Definition, ui: true),
        Graph => Meta(nameof(Graph), BlockOpPhase.Query, BlockOpFamily.Graph, ui: false),
        AdoptContent => Meta(nameof(AdoptContent), BlockOpPhase.Query, BlockOpFamily.Graph, ui: false),
        Preview => Meta(nameof(Preview), BlockOpPhase.Query, BlockOpFamily.Preview, ui: false),
        Attributes task => task.Task.OpMeta,
        _ => Meta(GetType().Name, BlockOpPhase.Query, BlockOpFamily.Graph, ui: false),
    };

    private static BlockOpMeta Meta(string name, BlockOpPhase phase, BlockOpFamily family, bool ui) =>
        new(Name: name, Phase: phase, Family: family, RequiresUiThread: ui);
}

[Union]
public abstract partial record BlockInstanceTask {
    private BlockInstanceTask() { }
    public sealed record Place(DefinitionRef Ref, Seq<Placement> At, BatchPolicy? Policy = null) : BlockInstanceTask;
    public sealed record ReplaceDefinition(Guid InstanceId, DefinitionRef NewRef) : BlockInstanceTask;
    public sealed record Transform(Guid InstanceId, global::Rhino.Geometry.Transform Xform, TransformPolicy? Mode = null) : BlockInstanceTask;
    public sealed record Explode(Guid InstanceId, ExplodePolicy Policy) : BlockInstanceTask;
    public sealed record Inspect(Guid InstanceId, ExplodePolicy Policy) : BlockInstanceTask;
    public sealed record SubObject(Guid InstanceId, ComponentIndex Component) : BlockInstanceTask;
    public sealed record SelectedPart(ObjRef Ref) : BlockInstanceTask;
    public sealed record Flatten(Guid InstanceId, DepthPolicy Policy) : BlockInstanceTask;
    public sealed record Bounds(DefinitionRef Ref, BoundsPolicy? Policy = null) : BlockInstanceTask;

    public BlockOpMeta OpMeta => this switch {
        Place => Meta(nameof(Place), BlockOpPhase.Mutation, ui: true),
        ReplaceDefinition => Meta(nameof(ReplaceDefinition), BlockOpPhase.Mutation, ui: true),
        Transform => Meta(nameof(Transform), BlockOpPhase.Mutation, ui: true),
        Explode => Meta(nameof(Explode), BlockOpPhase.Mutation, ui: true),
        Inspect => Meta(nameof(Inspect), BlockOpPhase.Query, ui: false),
        SubObject => Meta(nameof(SubObject), BlockOpPhase.Query, ui: false),
        SelectedPart => Meta(nameof(SelectedPart), BlockOpPhase.Query, ui: false),
        Flatten => Meta(nameof(Flatten), BlockOpPhase.Query, ui: false),
        Bounds => Meta(nameof(Bounds), BlockOpPhase.Query, ui: false),
        _ => Meta(GetType().Name, BlockOpPhase.Query, ui: false),
    };

    private static BlockOpMeta Meta(string name, BlockOpPhase phase, bool ui) =>
        new(Name: name, Phase: phase, Family: BlockOpFamily.Instance, RequiresUiThread: ui);
}

[Union]
public abstract partial record LinkLifecycle {
    private LinkLifecycle() { }
    public sealed record Create(Seq<FileEndpoint> Sources, LinkCreatePolicy? Policy = null) : LinkLifecycle;
    public sealed record Relink(Seq<LinkMap> Maps, BatchPolicy? Policy = null) : LinkLifecycle;
    public sealed record Refresh(BlockFilter? Filter = null, LinkRefreshPolicy? Policy = null, BatchPolicy? Batch = null) : LinkLifecycle;
    public sealed record Detach(BlockFilter? Filter = null) : LinkLifecycle;
    public sealed record LayerStyle(BlockFilter? Filter = null, Blocks.LayerStyle? Style = null) : LinkLifecycle;
    public sealed record Update(LinkedPolicy Policy) : LinkLifecycle;
    public sealed record SkipNested(DefinitionRef Ref, bool Value) : LinkLifecycle;

    public BlockOpMeta OpMeta => this switch {
        Create => Meta(nameof(Create)),
        Relink => Meta(nameof(Relink)),
        Refresh => Meta(nameof(Refresh)),
        Detach => Meta(nameof(Detach)),
        LayerStyle => Meta(nameof(LayerStyle)),
        Update => Meta(nameof(Update)),
        SkipNested => Meta(nameof(SkipNested)),
        _ => Meta(GetType().Name),
    };

    private static BlockOpMeta Meta(string name) =>
        new(Name: name, Phase: BlockOpPhase.Mutation, Family: BlockOpFamily.Linked, RequiresUiThread: true);
}

[Union]
public abstract partial record BlockAttributeTask {
    private BlockAttributeTask() { }
    public sealed record Text(DefinitionRef Ref, Option<Guid> InstanceId = default) : BlockAttributeTask;
    public sealed record Schema(DefinitionRef Ref) : BlockAttributeTask;
    public sealed record Write(
        DefinitionRef Ref,
        HashMap<string, string> Values,
        ConstraintPolicy? Policy = null,
        Option<Guid> InstanceId = default,
        ReferenceScope? Scope = null,
        MetadataPatch? Metadata = null) : BlockAttributeTask;
    public sealed record Matrix(Option<Seq<DefinitionRef>> Refs, ReferenceScope Scope) : BlockAttributeTask;

    public BlockOpMeta OpMeta => this switch {
        Text => Meta(nameof(Text), BlockOpPhase.Query, ui: false),
        Schema => Meta(nameof(Schema), BlockOpPhase.Query, ui: false),
        Write => Meta(nameof(Write), BlockOpPhase.Mutation, ui: true),
        Matrix => Meta(nameof(Matrix), BlockOpPhase.Query, ui: false),
        _ => Meta(GetType().Name, BlockOpPhase.Query, ui: false),
    };

    private static BlockOpMeta Meta(string name, BlockOpPhase phase, bool ui) =>
        new(Name: name, Phase: phase, Family: BlockOpFamily.Attributes, RequiresUiThread: ui);
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record TableMutation {
    private TableMutation() { }
    public sealed record Rename(DefinitionName NewName) : TableMutation;
    public sealed record Delete(DeletionPolicy Policy) : TableMutation;
    public sealed record Undelete() : TableMutation;
    public sealed record UndoModify() : TableMutation;
    public sealed record Purge() : TableMutation;
    public sealed record UpdateSourceArchive(FileEndpoint Source, UpdatePolicy Policy, LinkReloadPolicy Reload) : TableMutation;
    public sealed record ReloadFromFile(FileEndpoint Source, LinkReloadPolicy Policy) : TableMutation;
    public sealed record Export(FileEndpoint Target) : TableMutation;

    internal bool IncludeDeleted => this is Undelete;

    internal Fin<Unit> Confirm(MutateCtx ctx) => Switch(ctx,
        rename: static (c, m) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Modify(idefIndex: c.Index, newName: m.NewName.Value, newDescription: c.Snap.Description.IfNone(noneValue: string.Empty), quiet: true)),
        delete: static (c, m) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Delete(idefIndex: c.Index, deleteReferences: m.Policy.DeleteReferences, quiet: m.Policy.Quiet)),
        undelete: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Undelete(idefIndex: c.Index)),
        undoModify: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.UndoModify(idefIndex: c.Index)),
        purge: static (c, _) => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Purge(idefIndex: c.Index)),
        updateSourceArchive: static (c, m) => Operations.CommitSourceArchive(owner: c.Owner, index: c.Index, source: m.Source, policy: m.Policy, reload: m.Reload, key: c.Key),
        reloadFromFile: static (c, m) => from endpoint in m.Source.Input(op: c.Key)
                                         from _linked in Operations.ReloadLinked(owner: c.Owner, index: c.Index, loadPath: endpoint.Path, updateNestedLinks: m.Policy.UpdateNestedLinks, quiet: m.Policy.Quiet, key: c.Key)
                                         select _linked,
        export: static (c, m) => m.Target.Output(op: c.Key).Bind(endpoint => c.Key.Confirm(success: c.Owner.Document.InstanceDefinitions.Export(idefIndex: c.Index, filename: endpoint.Path))));

    internal MutationReceipt Project(Definition snap) =>
        this is Delete or Undelete or Purge ? MutationReceipt.Lifecycle(id: snap.Id.Value, name: snap.Name.Value) : MutationReceipt.Named(name: snap.Name.Value);
}

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct MutateCtx(RhinoBlocks Owner, int Index, Definition Snap, Op Key);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class ContentIndex {
    private static readonly Atom<HashMap<uint, HashMap<ulong, Seq<DefinitionId>>>> byDoc =
        Atom(value: HashMap<uint, HashMap<ulong, Seq<DefinitionId>>>());

    private static Unit MutateDoc(uint serial, Func<HashMap<ulong, Seq<DefinitionId>>, HashMap<ulong, Seq<DefinitionId>>> update) {
        _ = byDoc.Swap(f: m => m.AddOrUpdate(key: serial, Some: update, None: () => update(arg: HashMap<ulong, Seq<DefinitionId>>())));
        return unit;
    }

    internal static Seq<DefinitionId> Find(RhinoDoc doc, BlockContentHash hash) =>
        byDoc.Value.Find(key: doc.RuntimeSerialNumber).Bind(idx => idx.Find(key: hash.Value)).IfNone(noneValue: Seq<DefinitionId>());

    internal static int Adopt(RhinoDoc doc) {
        HashMap<ulong, Seq<DefinitionId>> seeded = FoldHashIndex(doc: doc);
        _ = byDoc.Swap(f: m => m.AddOrUpdate(key: doc.RuntimeSerialNumber, value: seeded));
        return seeded.Values.Fold(0, static (acc, ids) => acc + ids.Count);
    }

    internal static Unit RegisterIfMissing(RhinoDoc doc, BlockContentHash hash, DefinitionId defId) =>
        MutateDoc(serial: doc.RuntimeSerialNumber, update: inner => inner.AddOrUpdate(
            key: hash.Value,
            Some: ids => ids.Add(value: defId).Distinct(),
            None: () => Seq(defId)));

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
        _ = Seq(snapshot.Old, snapshot.New).Choose(static candidate => candidate).Map(static definition => definition.Id).Distinct()
            .Iter(defId => RemoveDefinition(serial: serial, defId: defId));
        return Optional(doc)
            .Map(active => snapshot.New.Map(definition => RegisterDefinition(doc: active, defId: definition.Id.Value)).IfNone(unit))
            .IfNone(unit);
    }

    internal static Unit RemoveDefinition(uint serial, DefinitionId defId) =>
        MutateDoc(serial: serial, update: inner => inner
            .Map((hash, ids) => (Hash: hash, Ids: ids.Filter(id => id != defId)))
            .Filter(static entry => !entry.Ids.IsEmpty)
            .Fold(HashMap<ulong, Seq<DefinitionId>>(), static (acc, entry) => acc.AddOrUpdate(key: entry.Hash, value: entry.Ids)));

    internal static Unit EvictDoc(uint serial) {
        _ = byDoc.Swap(f: m => m.Remove(key: serial));
        return unit;
    }

    private static HashMap<ulong, Seq<DefinitionId>> FoldHashIndex(RhinoDoc doc) =>
        Definition.List(table: doc.InstanceDefinitions)
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

internal static partial class Operations {
    private const string ExportBlockAttributesCommand = "_-ExportBlockAttributes";
    private const string SnapshotSaveCommand = "_-Snapshot _Save";
    internal static Fin<BlockOutcome> Run(BlockOp op, RhinoBlocks owner) {
        BlockOpMeta meta = op.Metadata;
        return op.Switch(
            owner: (Owner: owner, Meta: meta),
            author: static (c, x) => RunMut(meta: c.Meta, body: k => PerformAuthor(owner: c.Owner, spec: x.Spec, source: x.Source, conflict: x.Conflict, key: k)),
            addOrReuse: static (c, x) => RunMut(meta: c.Meta, body: k => PerformAddOrReuse(owner: c.Owner, spec: x.Spec, source: x.Source, key: k)),
            modify: static (c, x) => RunMut(meta: c.Meta, body: k => PerformModify(owner: c.Owner, refer: x.Ref, source: x.Source, patch: x.Patch ?? MetadataPatch.Empty, key: k)),
            table: static (c, x) => RunMut(meta: c.Meta, body: k => Mutate(owner: c.Owner, refer: x.Ref, mutation: x.Mutation, key: k)),
            purge: static (c, x) => RunMut(meta: c.Meta, body: k => PerformPurge(owner: c.Owner, refer: x.Ref, prefix: x.Prefix, key: k)),
            compact: static (c, x) => RunMut(meta: c.Meta, body: k => PerformCompact(owner: c.Owner, policy: x.Policy ?? CompactPolicy.UndoAware, key: k)),
            instance: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformInstanceTask(owner: c.Owner, task: x.Task, key: k)),
            linked: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformLinkLifecycle(owner: c.Owner, change: x.Change, key: k)),
            bakeArchive: static (c, x) => RunMut(meta: c.Meta, body: k => PerformBakeArchive(owner: c.Owner, source: x.Source, policy: x.Policy ?? BakePolicy.Default, key: k)),
            validateArchiveClosure: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformValidateArchiveClosure(source: x.Source, policy: x.Policy, key: k)),
            exportAttributes: static (c, x) => RunMut(meta: c.Meta, body: k => PerformExportAttributes(owner: c.Owner, refer: x.Ref, target: x.Target, key: k)),
            snapshotBlock: static (c, x) => RunMut(meta: c.Meta, body: k => PerformSnapshotBlock(owner: c.Owner, refer: x.Ref, name: x.Name, key: k)),
            allocateName: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformAllocateName(owner: c.Owner, seed: x.Seed, key: k)),
            snapshot: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformSnapshot(owner: c.Owner, refer: x.Ref, key: k)),
            duplicate: static (c, x) => RunMut(meta: c.Meta, body: k => PerformDuplicate(owner: c.Owner, refer: x.Ref, name: x.Name, conflict: x.Conflict ?? ConflictPolicy.Rename, key: k)),
            graph: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformGraph(owner: c.Owner, query: x.Query, key: k)),
            adoptContent: static (c, _) => RunOutcome(meta: c.Meta, body: k => PerformAdoptContent(owner: c.Owner, key: k)),
            preview: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformPreview(owner: c.Owner, refer: x.Ref, spec: x.Spec, key: k)),
            attributes: static (c, x) => RunOutcome(meta: c.Meta, body: k => PerformAttributeTask(owner: c.Owner, task: x.Task, key: k)));
    }

    private static Fin<BlockOutcome> RunMut(BlockOpMeta meta, Func<Op, Fin<MutationReceipt>> body) =>
        RunPhase(name: meta.Name, body: body, project: static receipt => new BlockOutcome.Receipt(Value: receipt));

    private static Fin<BlockOutcome> RunOutcome(BlockOpMeta meta, Func<Op, Fin<BlockOutcome>> body) =>
        RunPhase(name: meta.Name, body: body, project: static outcome => outcome);

    private static Fin<BlockOutcome> RunPhase<T>(
        string name,
        Func<Op, Fin<T>> body,
        Func<T, BlockOutcome> project) {
        Op key = Op.Of(name: name);
        return key.Catch(() => body(arg: key).Map(project));
    }

    private static Fin<Unit> ConfirmNative(Op key, string step, bool success) =>
        guard(success, key.InvalidResult(detail: step)).ToFin();
    private static int AddLinkPlaceholder(InstanceDefinitionTable table, string name) =>
        table.Add(
            name: name,
            description: string.Empty,
            basePoint: Point3d.Origin,
            geometry: [],
            attributes: []);
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
                ? Definition.List(table: ctx.Table, ignoreDeleted: false)
                    .Find(d => string.Equals(a: d.Name, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase)
                               || string.Equals(a: d.DeletedName, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
                : Optional(ctx.Table.Find(instanceDefinitionName: r.Name.Value))).ToFin(Fail: ctx.Key.InvalidInput()));
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
    private static Fin<int> RequireIndex(Definition snap, Op key) =>
        snap.Index.Case switch {
            DefinitionIndex i => Fin.Succ(value: i.Value),
            _ => Fin.Fail<int>(error: key.InvalidInput()),
        };

    private static Fin<InstanceObject> AsInstance(ObjectTable objects, Guid instanceId, Op key) =>
        Optional(objects.FindId(id: instanceId) as InstanceObject).ToFin(Fail: key.InvalidInput());

    private static Seq<T> NonNull<T>(T[]? native) where T : class => toSeq(native ?? []).Choose(static item => Optional(item));
    private static Seq<InstanceObject> LiveRefs(InstanceDefinition live, ReferenceScope scope) => NonNull(native: live.GetReferences(wheretoLook: scope.Native));
    private static Fin<MutationReceipt> PerformAuthor(RhinoBlocks owner, AuthorSpec spec, Members source, ConflictPolicy conflict, Op key) =>
        spec.Admit(key: key).Bind(admitted =>
            ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(admitted.Name), key: key).Match(
                Fail: _ => AddNew(owner: owner, spec: admitted, source: source, key: key),
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
            rename: static s => AddNew(
                    owner: s.Owner,
                    spec: s.Spec with {
                        Name = DefinitionName.From(value: s.Owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: s.Spec.Name.Value)).IfFail(_ => s.Spec.Name),
                    },
                    source: s.Source,
                    key: s.Key));

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
    internal static Seq<(GeometryBase Geometry, ObjectAttributes Attributes)> CanonicalMemberPairs(InstanceDefinition def) =>
        VisitDefinitionMembers(def: def, composed: Transform.Identity).Choose(static visit =>
            visit.Reference.Case switch {
                InstanceReferenceGeometry r => Some((r.Duplicate(), Members.SanitizeAttributes(attributes: visit.Attributes))),
                _ => visit.Leaf.Bind(static leaf => Optional(leaf.Geometry).Map(g => (
                    g.Duplicate(),
                    Members.SanitizeAttributes(attributes: leaf.Attributes)))),
            });

    internal static Seq<(Guid ObjectId, InstanceReferenceGeometry Reference)> NestedReferences(InstanceDefinition def) =>
        VisitDefinitionMembers(def: def, composed: Transform.Identity)
            .Choose(static visit => visit.Reference.Map(reference => (visit.ObjectId, Reference: (InstanceReferenceGeometry)reference.Duplicate())));

    internal static Seq<MemberVisit> VisitDefinitionMembers(InstanceDefinition def, Transform composed) =>
        NonNull(native: def.GetObjects()).Choose(member => member switch {
            InstanceObject nested when nested.InstanceDefinition is InstanceDefinition inst =>
                Some(new MemberVisit(
                    ObjectId: nested.Id,
                    Attributes: nested.Attributes,
                    Composed: composed,
                    Instance: Some(nested),
                    Definition: Some(inst),
                    Reference: Some(new InstanceReferenceGeometry(instanceDefinitionId: inst.Id, transform: nested.InstanceXform)),
                    Leaf: Option<RhinoObject>.None)),
            { Geometry: InstanceReferenceGeometry r } => Some(new MemberVisit(
                ObjectId: member.Id,
                Attributes: member.Attributes,
                Composed: composed,
                Instance: Option<InstanceObject>.None,
                Definition: Option<InstanceDefinition>.None,
                Reference: Some((InstanceReferenceGeometry)r.Duplicate()),
                Leaf: Some(member))),
            RhinoObject leaf when leaf.Geometry is not null => Some(new MemberVisit(
                ObjectId: leaf.Id,
                Attributes: leaf.Attributes,
                Composed: composed,
                Instance: Option<InstanceObject>.None,
                Definition: Option<InstanceDefinition>.None,
                Reference: Option<InstanceReferenceGeometry>.None,
                Leaf: Some(leaf))),
            _ => Option<MemberVisit>.None,
        });

    private static Fin<MutationReceipt> AddNew(RhinoBlocks owner, AuthorSpec spec, Members source, Op key) =>
        spec.Source.Case switch {
            FileEndpoint endpoint => CreateLinkedArchivePlaceholder(owner: owner, spec: spec, endpoint: endpoint, key: key),
            _ => Reify(doc: owner.Document, source: source, key: key)
                .Bind(provided => AddStatic(owner: owner, spec: spec, members: provided, key: key)),
        };

    private static Fin<MutationReceipt> AddStatic(RhinoBlocks owner, AuthorSpec spec, Members.Provided members, Op key) =>
        AddDefinition(table: owner.Document.InstanceDefinitions, spec: spec, members: members) switch {
            int idx when idx >= 0 => ApplyCreateState(table: owner.Document.InstanceDefinitions, idx: idx, spec: spec, members: members, key: key),
            _ => Fin.Fail<MutationReceipt>(error: key.InvalidResult()),
        };

    private static Fin<MutationReceipt> CreateLinkedArchivePlaceholder(RhinoBlocks owner, AuthorSpec spec, FileEndpoint endpoint, Op key) =>
        from source in endpoint.Input(op: key)
        from policy in new LinkCreatePolicy(Update: spec.Update, Layer: spec.Layer, Reload: LinkReloadPolicy.NestedQuiet).Admit(key: key)
        from link in CreateAndAttach(owner: owner, source: source, policy: policy, key: key, name: Some(spec.Name))
        from id in DefinitionId.From(value: link.Id, key: key)
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: DefinitionRef.Of(id), key: key)
        from _ in spec.Metadata.IsEmpty ? Fin.Succ(value: unit) : CommitMetadata(table: owner.Document.InstanceDefinitions, snap: snap, patch: spec.Metadata, key: key)
        from live in Optional(owner.Document.InstanceDefinitions.Find(instanceId: link.Id, ignoreDeletedInstanceDefinitions: true)).ToFin(Fail: key.InvalidResult())
        select MutationReceipt.Resources(
            changes: Seq(link.Change),
            diagnostics: spec.CreateDiagnostics(id: snap.Id, live: live));

    private static int AddDefinition(InstanceDefinitionTable table, AuthorSpec spec, Members.Provided members) {
        string description = spec.Metadata.Description.IfNone(noneValue: string.Empty);
        return spec.Metadata.Url.Case switch {
            ArchivePath url => table.Add(
                name: spec.Name.Value,
                description: description,
                url: url.Value,
                urlTag: spec.Metadata.UrlDescription.IfNone(noneValue: string.Empty),
                basePoint: spec.BasePoint,
                geometry: members.Geometry.AsEnumerable(),
                attributes: members.Attributes.AsEnumerable()),
            _ => table.Add(
                name: spec.Name.Value,
                description: description,
                basePoint: spec.BasePoint,
                geometry: members.Geometry.AsEnumerable(),
                attributes: members.Attributes.AsEnumerable()),
        };
    }
    internal static Seq<T> BindDefinitionMembers<T>(
        InstanceDefinition def, Transform composed,
        Func<InstanceObject, Transform, Seq<T>> onInstance,
        Func<InstanceReferenceGeometry, Guid, Transform, Seq<T>> onReference,
        Func<RhinoObject, Transform, Seq<T>>? onLeaf = null) =>
        VisitDefinitionMembers(def: def, composed: composed).Bind(visit => visit switch {
            { Instance: { IsSome: true, Case: InstanceObject nested } } => onInstance(nested, visit.Composed),
            { Reference: { IsSome: true, Case: InstanceReferenceGeometry r } } => onReference(r, visit.ObjectId, visit.Composed),
            { Leaf: { IsSome: true, Case: RhinoObject leaf } } when onLeaf is not null => onLeaf(leaf, visit.Composed),
            _ => Seq<T>(),
        });
    private static Fin<MutationReceipt> ApplyCreateState(InstanceDefinitionTable table, int idx, AuthorSpec spec, Members.Provided members, Op key) =>
        from live in Optional(table[idx] ?? table.Find(instanceDefinitionName: spec.Name.Value)).ToFin(Fail: key.InvalidResult())
        from id in DefinitionId.From(value: live.Id, key: key)
        let _layer = (live.LayerStyle = spec.Layer.Native, unit).unit
        from _strings in ApplyUserStringsWhenPresent(table: table, idx: idx, strings: spec.Metadata.UserStrings, key: key)
        from snap in Definition.From(definition: live, anchorDirectory: BlockPaths.DocAnchor(document: table.Document), key: key)
        let _cached = SnapshotVault.Upsert(docSerial: table.Document.RuntimeSerialNumber, definition: snap)
        let _indexed = Op.Of(name: nameof(BlockContentHash))
            .Catch(() => Fin.Succ(value: BlockContentHash.Of(members: members)))
            .Iter(hash => ContentIndex.RegisterIfMissing(doc: table.Document, hash: hash, defId: snap.Id))
        select MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: spec.Name.Value).Document,
            diagnostics: spec.CreateDiagnostics(id: id, live: live));
    private static Fin<Unit> ApplyUserStringsWhenPresent(InstanceDefinitionTable table, int idx, HashMap<string, string> strings, Op key) =>
        strings.IsEmpty
            ? Fin.Succ(value: unit)
            : Optional(table[idx]).ToFin(Fail: key.InvalidResult()).Bind(live =>
                strings
                    .Map((k, v) => key.Confirm(success: live.SetUserString(key: k, value: v)))
                    .Values
                    .ToSeq()
                    .TraverseM(identity)
                    .As()
                    .Map(_ => InvalidateDefinition(defId: live.Id)));
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
                None: () => AddStatic(owner: owner, spec: spec, members: provided, key: key));
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
            .Bind(admitted => admitted.Source.Case switch {
                FileEndpoint => AddNew(owner: owner, spec: admitted, source: source, key: key),
                _ => Reify(doc: owner.Document, source: source, key: key)
                    .Bind(provided => LookupByContent(owner: owner, provided: provided, spec: admitted, key: key)),
            });

    private static Fin<MutationReceipt> PerformModify(RhinoBlocks owner, DefinitionRef refer, Option<Members> source, MetadataPatch patch, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from idx in RequireIndex(snap: pair.Snap, key: key)
        from provided in source.Case switch {
            Members requested => UpdatePolicy.FromNative(native: pair.Live.UpdateType)
                .RejectLinkedModify(key: key)
                .Bind(_ => Reify(doc: owner.Document, source: requested, key: key).Map(static members => Some(value: members))),
            _ => Fin.Succ(value: Option<Members.Provided>.None),
        }
        from _geometry in provided.Case switch {
            Members.Provided members => key.Confirm(success: owner.Document.InstanceDefinitions.ModifyGeometry(
                idefIndex: idx,
                newGeometry: members.Geometry.AsEnumerable(),
                newAttributes: members.Attributes.AsEnumerable())),
            _ => Fin.Succ(value: unit),
        }
        from _metadata in patch.IsEmpty
            ? Fin.Succ(value: unit)
            : CommitMetadata(table: owner.Document.InstanceDefinitions, snap: pair.Snap, patch: patch, key: key)
        select InvalidateWith(doc: owner.Document, defId: pair.Snap.Id.Value, value: MutationReceipt.Of(
            receipt: MutationReceipt.Named(name: pair.Snap.Name.Value).Document,
            diagnostics: patch.ModifyDiagnostics(id: pair.Snap.Id)));

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
            ? Optional(table[idx]).ToFin(Fail: key.InvalidResult()).Bind(_ => {
                UserDictionary updated = new();
                return key.Confirm(success: updated.Dictionary.ReplaceContentsWith(source: d))
                    .Bind(_ => key.Confirm(success: table.Modify(idefIndex: idx, userData: updated, quiet: true)));
            })
            : Fin.Succ(value: unit)
        select unit;
    private readonly record struct ExplodedPiece(RhinoObject Piece, ObjectAttributes Attrs, Transform Transform);
    private static Fin<MutationReceipt> Mutate(RhinoBlocks owner, DefinitionRef refer, TableMutation mutation, Op key) =>
        from snap in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key, includeDeleted: mutation.IncludeDeleted).Map(static pair => pair.Snap)
        from idx in RequireIndex(snap: snap, key: key)
        from _ in mutation.Confirm(ctx: new MutateCtx(Owner: owner, Index: idx, Snap: snap, Key: key))
        select InvalidateWith(doc: owner.Document, defId: snap.Id.Value, value: mutation.Project(snap: snap));

    private static Fin<MutationReceipt> PerformPurge(RhinoBlocks owner, Option<DefinitionRef> refer, Option<DefinitionPrefix> prefix, Op key) =>
        key.Catch(() => (refer.Case, prefix.Case) switch {
            (DefinitionRef r, _) => Mutate(owner: owner, refer: r, mutation: new TableMutation.Purge(), key: key),
            (_, DefinitionPrefix p) => PerformPurgeByPrefix(owner: owner, prefix: p, key: key),
            _ => PurgeAllUnused(owner: owner),
        });

    private static Fin<MutationReceipt> PurgeAllUnused(RhinoBlocks owner) {
        int purged = owner.Document.InstanceDefinitions.PurgeUnused();
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Resource(kind: DocumentResourceKind.Block, name: $"<purged:{purged}>"));
    }

    private static Fin<MutationReceipt> PerformPurgeByPrefix(RhinoBlocks owner, DefinitionPrefix prefix, Op key) {
        Seq<DocumentResourceChange> changes = Definition.List(table: owner.Document.InstanceDefinitions)
            .Filter(d => d?.Name?.StartsWith(value: prefix.Value, comparisonType: StringComparison.OrdinalIgnoreCase) ?? false)
            .Choose(d => owner.Document.InstanceDefinitions.Purge(idefIndex: d.Index)
                ? Some(value: DocumentResourceKind.Block.Change(name: $"<purged:{d.Index}>"))
                : Option<DocumentResourceChange>.None);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Succ(value: MutationReceipt.Resources(changes: changes));
    }

    private static Fin<MutationReceipt> PerformCompact(RhinoBlocks owner, CompactPolicy policy, Op key) =>
        key.Catch(() => {
            owner.Document.InstanceDefinitions.Compact(ignoreUndoReferences: policy.IgnoreUndoReferences);
            _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
            return Fin.Succ(value: MutationReceipt.Empty);
        });
    private static Fin<BlockOutcome> PerformInstanceTask(RhinoBlocks owner, BlockInstanceTask task, Op key) =>
        task switch {
            BlockInstanceTask.Place x => PerformPlace(owner: owner, refer: x.Ref, at: x.At, policy: x.Policy ?? BatchPolicy.Default, key: key).Map(ReceiptOutcome),
            BlockInstanceTask.ReplaceDefinition x => PerformReplaceDefinition(owner: owner, instanceId: x.InstanceId, newRef: x.NewRef, key: key).Map(ReceiptOutcome),
            BlockInstanceTask.Transform x => PerformTransformInstance(owner: owner, instanceId: x.InstanceId, xform: x.Xform, mode: x.Mode ?? TransformPolicy.Copy, key: key).Map(ReceiptOutcome),
            BlockInstanceTask.Explode x => PerformExplodeIntoDocument(owner: owner, instanceId: x.InstanceId, policy: x.Policy, key: key).Map(ReceiptOutcome),
            BlockInstanceTask.Inspect x => PerformExplodeInspect(owner: owner, instanceId: x.InstanceId, policy: x.Policy, key: key),
            BlockInstanceTask.SubObject x => PerformUseSubObject(owner: owner, instanceId: x.InstanceId, component: x.Component, key: key),
            BlockInstanceTask.SelectedPart x => PerformSelectedPart(owner: owner, refer: x.Ref, key: key),
            BlockInstanceTask.Flatten x => PerformFlatten(owner: owner, instanceId: x.InstanceId, policy: x.Policy, key: key),
            BlockInstanceTask.Bounds x => PerformBounds(owner: owner, refer: x.Ref, policy: x.Policy ?? BoundsPolicy.Default, key: key),
            _ => Fin.Fail<BlockOutcome>(error: key.InvalidInput()),
        };

    private static BlockOutcome ReceiptOutcome(MutationReceipt receipt) =>
        new BlockOutcome.Receipt(Value: receipt);

    private static Fin<MutationReceipt> PerformPlace(RhinoBlocks owner, DefinitionRef refer, Seq<Placement> at, BatchPolicy policy, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from live in snap.Live.ToFin(Fail: key.InvalidInput())
        from idx in RequireIndex(snap: snap, key: key)
        from admitted in at.TraverseM(placement => placement.Admit(live: live, key: key)).As()
        from receipts in PlaceInBatch(doc: owner.Document, index: idx, at: admitted, policy: policy, key: key)
        from _ in key.Confirm(success: !receipts.IsEmpty)
        select MutationReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: receipts, kind: DocumentResourceKind.Block, name: snap.Name.Value);

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
    private static Fin<MutationReceipt> PerformReplaceDefinition(RhinoBlocks owner, Guid instanceId, DefinitionRef newRef, Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: newRef, key: key)
        from idx in RequireIndex(snap: snap, key: key)
        let priorName = AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key).ToOption()
            .Bind(io => Optional(io.InstanceDefinition))
            .Bind(def => Optional(def.Name).Filter(static n => !string.IsNullOrWhiteSpace(value: n)))
        from _ in key.Confirm(success: owner.Document.Objects.ReplaceInstanceObject(objectId: instanceId, instanceDefinitionIndex: idx))
        let next = snap.Name.Value
        select MutationReceipt.Objects(slot: DocumentReceiptSlot.Replaced, ids: Seq(instanceId), resources: priorName.Case is string p && !string.Equals(a: p, b: next, comparisonType: StringComparison.OrdinalIgnoreCase)
                ? Seq(DocumentResourceKind.Block.Change(name: p), DocumentResourceKind.Block.Change(name: next))
                : Seq(DocumentResourceKind.Block.Change(name: next)));

    private static Fin<MutationReceipt> PerformTransformInstance(RhinoBlocks owner, Guid instanceId, Transform xform, TransformPolicy mode, Op key) =>
        key.Catch(() => {
            Guid resultId = mode.Apply(objects: owner.Document.Objects, instanceId: instanceId, xform: xform);
            return key.Confirm(success: resultId != Guid.Empty)
                .Map(_ => MutationReceipt.Of(receipt: mode.InstanceTransform(instanceId: instanceId, resultId: resultId)));
        });
    private static Fin<MutationReceipt> PerformExplodeIntoDocument(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        policy is ExplodePolicy.Native native
            ? from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
              from produced in Optional(owner.Document.Objects.AddExplodedInstancePieces(
                      instance: instance, explodeNestedInstances: native.ExplodeNested, deleteInstance: native.DeleteInstance))
                  .Map(toSeq)
                  .ToFin(Fail: key.InvalidResult())
              select MutationReceipt.Of(receipt: native.ExplodeReceipt(created: produced, instanceId: instanceId))
            : ExplodeManual(owner: owner, instanceId: instanceId, policy: policy, key: key);

    private static Fin<MutationReceipt> ExplodeManual(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        let pieces = ExplodePieces(instance: instance, policy: policy)
        from produced in toSeq(pieces).Choose(p => p.Piece.Geometry is not null
                ? Some(value: MaterializePiece(doc: owner.Document, piece: p, key: key))
                : Option<Fin<Guid>>.None)
            .TraverseM(identity).As()
        from deleted in key.Confirm(success: owner.Document.Objects.Delete(obj: instance, quiet: true))
        select MutationReceipt.Of(
            receipt: DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: produced)
                + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: Seq(instanceId)),
            diagnostics: pieces.Length == produced.Count
                ? Seq<BlockDiagnostic>()
                : Seq<BlockDiagnostic>(new BlockDiagnostic.ExplodePartial(Requested: pieces.Length, Received: produced.Count)));

    private static ExplodedPiece[] ExplodePieces(InstanceObject instance, ExplodePolicy policy) {
        (bool skips, Guid viewport) = policy.Resolve();
        instance.Explode(
            skipHiddenPieces: skips, viewportId: viewport,
            explodeNestedInstances: policy.IncludesNestedInstances,
            pieces: out RhinoObject[] pieces,
            pieceAttributes: out ObjectAttributes[] attrs,
            pieceTransforms: out Transform[] transforms);
        return [.. toSeq(pieces ?? [])
            .Zip(toSeq(attrs ?? []), static (piece, attr) => (Piece: piece, Attrs: attr))
            .Zip(toSeq(transforms ?? []), static (pair, xform) => new ExplodedPiece(
                Piece: pair.Piece,
                Attrs: pair.Attrs,
                Transform: xform))];
    }
    private static Fin<Guid> MaterializePiece(RhinoDoc doc, ExplodedPiece piece, Op key) =>
        piece.Piece switch {
            InstanceObject nested => Optional(nested.InstanceDefinition).ToFin(Fail: key.InvalidResult())
                .Bind(def => DefinitionIndex.From(value: def.Index, key: key)
                    .Bind(idx => AddInstance(
                        table: doc.Objects,
                        index: idx.Value,
                        placement: Placement.Of(xform: piece.Transform * nested.InstanceXform, reference: false),
                        key: key))),
            _ => key.Catch(() => {
                GeometryBase g = piece.Piece.Geometry.Duplicate();
                _ = g.Transform(piece.Transform);
                Guid id = doc.Objects.Add(geometry: g, attributes: piece.Attrs.Duplicate());
                return key.Confirm(success: id != Guid.Empty).Map(_ => id);
            }),
        };
    private readonly record struct CreatedLink(Guid Id, DocumentResourceChange Change);
    private readonly record struct LinkIntent(int Index, FileEndpoint Source, UpdatePolicy Update, LinkReloadPolicy Reload) {
        internal static Fin<LinkIntent> Of(int index, FileEndpoint source, UpdatePolicy update, LinkReloadPolicy reload, Op key) =>
            from _ in update.RequireLinked(key: key)
            select new LinkIntent(Index: index, Source: source, Update: update, Reload: reload);

        internal Fin<Unit> Commit(RhinoBlocks owner, string loadPath, Op key) =>
            ModifyAndRefreshLinked(
                owner: owner,
                index: Index,
                source: Source,
                policy: Update,
                quiet: Reload.Quiet,
                updateNestedLinks: Reload.UpdateNestedLinks,
                loadPath: loadPath,
                key: key);
    }

    private static Fin<BlockOutcome> PerformLinkLifecycle(RhinoBlocks owner, LinkLifecycle change, Op key) =>
        change switch {
            LinkLifecycle.Create x => PerformCreateArchiveLinks(owner: owner, sources: x.Sources, policy: x.Policy ?? LinkCreatePolicy.Default, key: key).Map(ReceiptOutcome),
            LinkLifecycle.Relink x => PerformBatchRelink(owner: owner, maps: x.Maps, policy: x.Policy ?? BatchPolicy.Default, key: key).Map(ReceiptOutcome),
            LinkLifecycle.Refresh x => PerformRefreshLinks(owner: owner, filter: x.Filter ?? BlockFilter.All, policy: x.Policy ?? LinkRefreshPolicy.All, batch: x.Batch ?? BatchPolicy.Default, key: key).Map(ReceiptOutcome),
            LinkLifecycle.Detach x => PerformLinkedFilter(owner: owner, filter: x.Filter ?? BlockFilter.All, policy: LinkRefreshPolicy.All, batch: BatchPolicy.Default, key: key, worker: DetachLinkedDefinition).Map(ReceiptOutcome),
            LinkLifecycle.LayerStyle x => PerformLinkedFilter(owner: owner, filter: x.Filter ?? BlockFilter.All, policy: LinkRefreshPolicy.All, batch: BatchPolicy.Default, key: key, worker: (o, d, k) => ApplyLinkedLayerStyle(owner: o, definition: d, style: x.Style ?? LayerStyle.Active, key: k)).Map(ReceiptOutcome),
            LinkLifecycle.Update x => PerformLinkedUpdatePolicy(owner: owner, policy: x.Policy, key: key).Map(ReceiptOutcome),
            LinkLifecycle.SkipNested x => PerformSkipNestedLinked(owner: owner, refer: x.Ref, value: x.Value, key: key).Map(ReceiptOutcome),
            _ => Fin.Fail<BlockOutcome>(error: key.InvalidInput()),
        };

    private static Fin<MutationReceipt> PerformCreateArchiveLinks(RhinoBlocks owner, Seq<FileEndpoint> sources, LinkCreatePolicy policy, Op key) =>
        from links in CreateArchiveLinks(owner: owner, sources: sources, policy: policy, key: key)
        select MutationReceipt.Resources(changes: links.Map(static link => link.Change));

    private static Fin<Seq<CreatedLink>> CreateArchiveLinks(RhinoBlocks owner, Seq<FileEndpoint> sources, LinkCreatePolicy policy, Op key) =>
        from admitted in policy.Admit(key: key)
        from links in sources
            .Map(endpoint => key.Catch(() =>
                from source in endpoint.Input(op: key)
                from link in CreateAndAttach(owner: owner, source: source, policy: admitted, key: key)
                select link))
            .TraverseM(identity).As()
        select links;

    private static Fin<CreatedLink> CreateAndAttach(RhinoBlocks owner, FileEndpoint source, LinkCreatePolicy policy, Op key, Option<DefinitionName> name = default) {
        string filename = source.Path;
        int idx = AddLinkPlaceholder(
            table: owner.Document.InstanceDefinitions,
            name: name.Map(static value => value.Value).IfNone(() =>
                owner.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: IOPath.GetFileNameWithoutExtension(path: filename))));
        return idx < 0
            ? Fin.Fail<CreatedLink>(error: key.InvalidResult(detail: nameof(InstanceDefinitionTable.Add)))
            : (from intent in LinkIntent.Of(index: idx, source: source, update: policy.EffectiveUpdate, reload: policy.EffectiveReload, key: key)
               from changed in intent.Commit(owner: owner, loadPath: filename, key: key)
               select changed)
                .BiBind(
                    Succ: _ => Fin.Succ(value: FinalizeLink(owner: owner, index: idx, layer: policy.EffectiveLayer, filename: filename)),
                    Fail: error => RollbackLink(owner: owner, idx: idx, error: error));
    }

    private static Fin<CreatedLink> RollbackLink(RhinoBlocks owner, int idx, Error error) {
        _ = owner.Document.InstanceDefinitions.Delete(idefIndex: idx, deleteReferences: true, quiet: true);
        _ = owner.Document.InstanceDefinitions.Purge(idefIndex: idx);
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return Fin.Fail<CreatedLink>(error);
    }

    private static CreatedLink FinalizeLink(RhinoBlocks owner, int index, LayerStyle layer, string filename) {
        InstanceDefinition live = owner.Document.InstanceDefinitions[index];
        live.LayerStyle = layer.Native;
        _ = InvalidateDefinition(defId: live.Id, doc: Some(value: owner.Document));
        _ = ContentIndex.EvictDoc(serial: owner.Document.RuntimeSerialNumber);
        return new CreatedLink(Id: live.Id, Change: DocumentResourceKind.Block.Change(name: live.Name ?? filename));
    }

    private static Fin<MutationReceipt> PerformLinkedUpdatePolicy(RhinoBlocks owner, LinkedPolicy policy, Op key) =>
        from active in Optional(policy).ToFin(Fail: key.InvalidInput())
        from _ in key.Catch(() => {
            owner.Document.LinkedInstanceDefinitionUpdate = active.Native;
            return Fin.Succ(value: unit);
        })
        select MutationReceipt.Empty;

    private static Fin<MutationReceipt> PerformSkipNestedLinked(RhinoBlocks owner, DefinitionRef refer, bool value, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from _linked in UpdatePolicy.FromNative(native: pair.Live.UpdateType).RequireLinked(key: key)
        from _changed in key.Catch(() => {
            pair.Live.SkipNestedLinkedDefinitions = value;
            return Fin.Succ(value: unit);
        })
        select InvalidateWith(doc: owner.Document, defId: pair.Live.Id, value: MutationReceipt.Named(name: pair.Snap.Name.Value));

    private static Fin<MutationReceipt> PerformLinkedFilter(
        RhinoBlocks owner,
        BlockFilter filter,
        LinkRefreshPolicy policy,
        BatchPolicy batch,
        Op key,
        Func<RhinoBlocks, InstanceDefinition, Op, Fin<DocumentResourceChange>> worker) {
        Option<string> anchor = BlockPaths.DocAnchor(document: owner.Document);
        return WithBatchRedraw(doc: owner.Document, policy: batch, key: key, work: () =>
            filter.Apply(table: owner.Document.InstanceDefinitions, anchorDirectory: anchor, policy: policy)
                .TraverseM(definition => worker(owner, definition, key))
                .As()
                .Map(changes => MutationReceipt.Resources(changes: changes)));
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

    private static Fin<MutationReceipt> PerformRefreshLinks(RhinoBlocks owner, BlockFilter filter, LinkRefreshPolicy policy, BatchPolicy batch, Op key) =>
        RefreshLinkedDocument(doc: owner.Document, filter: filter, policy: policy, batch: batch, key: key)
            .Map(MutationReceipt.Of);

    internal static Fin<DocumentReceipt> RefreshLinkedDocument(
        RhinoDoc doc,
        BlockFilter filter,
        LinkRefreshPolicy policy,
        BatchPolicy batch,
        Op key) =>
        WithBatchRedraw(doc: doc, policy: batch, key: key, work: () =>
            RefreshSources(
                    doc: doc,
                    filter: filter,
                    anchorDirectory: BlockPaths.DocAnchor(document: doc),
                    policy: policy,
                    key: key)
                .Map(refreshed => DocumentReceipt.Resources(changes: refreshed.Map(static d => DocumentResourceKind.Block.Change(name: d.Name ?? string.Empty)))));

    private static Fin<DocumentResourceChange> ApplyLinkedLayerStyle(RhinoBlocks owner, InstanceDefinition definition, LayerStyle style, Op key) =>
        Fin.Succ(value: UpdatePolicy.FromNative(native: definition.UpdateType))
            .Bind(policy => guard(style.AppliesTo(policy: policy), key.InvalidInput()).ToFin())
            .Bind(_ =>
            key.Catch(() => {
                definition.LayerStyle = style.Native;
                _ = InvalidateDefinition(defId: definition.Id, doc: Some(value: owner.Document));
                return Fin.Succ(value: DocumentResourceKind.Block.Change(name: definition.Name ?? string.Empty));
            }));

    private static Fin<DocumentResourceChange> DetachLinkedDefinition(RhinoBlocks owner, InstanceDefinition definition, Op key) =>
        from _ in key.Confirm(success: owner.Document.InstanceDefinitions.DestroySourceArchive(definition: definition, quiet: true))
        let invalidated = InvalidateDefinition(defId: definition.Id, doc: Some(value: owner.Document))
        select DocumentResourceKind.Block.Change(name: definition.Name ?? string.Empty);

    private static Fin<Seq<InstanceDefinition>> RefreshSources(RhinoDoc doc, BlockFilter filter, Option<string> anchorDirectory, LinkRefreshPolicy policy, Op key) =>
        from candidates in Fin.Succ(value: filter.Apply(table: doc.InstanceDefinitions, anchorDirectory: anchorDirectory, policy: policy))
        from ordered in Archive.LinkedRefreshOrder(table: doc.InstanceDefinitions, candidates: candidates, key: key)
        from refreshed in ordered
            .TraverseM(candidate =>
                Optional(doc.InstanceDefinitions.Find(instanceId: candidate.Id, ignoreDeletedInstanceDefinitions: true))
                    .ToFin(Fail: key.InvalidResult())
                    .Bind(live =>
                        key.Confirm(success: doc.InstanceDefinitions.RefreshLinkedBlock(definition: live))
                            .Map(_ => InvalidateWith(doc: doc, defId: live.Id, value: live))))
            .As()
        select refreshed;
    internal static Fin<Unit> CommitSourceArchive(
        RhinoBlocks owner,
        int index,
        FileEndpoint source,
        UpdatePolicy policy,
        LinkReloadPolicy reload,
        Op key) =>
        from endpoint in source.Input(op: key)
        from intent in LinkIntent.Of(index: index, source: endpoint, update: policy, reload: reload, key: key)
        from changed in intent.Commit(owner: owner, loadPath: endpoint.Path, key: key)
        select changed;
    private static Fin<Unit> ConfirmAttached(Op key, InstanceDefinition live, bool modifyReported) =>
        UpdatePolicy.FromNative(native: live.UpdateType).IsLinked
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
    internal static Fin<Unit> ReloadLinked(RhinoBlocks owner, int index, string loadPath, bool updateNestedLinks, bool quiet, Op key) {
        InstanceDefinitionTable defs = owner.Document.InstanceDefinitions;
        Fin<InstanceDefinition> atIndex() => Optional(defs[index]).ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinitionTable)));
        static bool Attached(InstanceDefinition live) =>
            UpdatePolicy.FromNative(native: live.UpdateType).IsLinked
            && Definition.NonBlank(value: live.SourceArchive).IsSome
            && ArchiveStatus.FromNative(native: live.ArchiveFileStatus).CanRefresh;
        Seq<Func<InstanceDefinition, bool>> loaders = Seq(
            live => defs.UpdateLinkedInstanceDefinition(idefIndex: index, filename: Definition.NonBlank(value: live.SourceArchive).IfNone(noneValue: loadPath), updateNestedLinks: updateNestedLinks, quiet: quiet),
            defs.RefreshLinkedBlock);
        return loaders
            .Fold(Fin.Succ(value: false),
                (acc, loader) => acc.Bind(done => done
                    ? Fin.Succ(value: true)
                    : atIndex().Bind(live => Fin.Succ(value: loader(live))).Bind(_ => atIndex().Map(Attached))))
            .Bind(done => done ? Fin.Succ(value: unit) : ConfirmNative(key: key, step: nameof(InstanceDefinitionTable.RefreshLinkedBlock), success: false));
    }
    private static Fin<MutationReceipt> PerformExportAttributes(RhinoBlocks owner, DefinitionRef refer, FileEndpoint target, Op key) =>
        from endpoint in target.Output(op: key)
        from receipt in RunBlockScriptFor(
            owner: owner,
            refer: refer,
            command: ExportBlockAttributesCommand,
            args: snap => Seq(("Definition", snap.Name.Value), ("File", endpoint.Path)),
            key: key)
        select receipt;

    private static Fin<MutationReceipt> PerformSnapshotBlock(RhinoBlocks owner, DefinitionRef refer, SnapshotName name, Op key) =>
        RunBlockScriptFor(
            owner: owner,
            refer: refer,
            command: SnapshotSaveCommand,
            args: _ => Seq(("Name", name.Value)),
            key: key);

    private static Fin<MutationReceipt> RunBlockScriptFor(
        RhinoBlocks owner,
        DefinitionRef refer,
        string command,
        Func<Definition, Seq<(string Name, string Value)>> args,
        Op key) =>
        from snap in ResolveSnap(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from receipt in RunBlockScript(owner: owner, snap: snap, command: command, args: args(arg: snap), key: key)
        select receipt;

    private static Fin<MutationReceipt> RunBlockScript(
        RhinoBlocks owner,
        Definition snap,
        string command,
        Seq<(string Name, string Value)> args,
        Op key) =>
        key.Confirm(success: RhinoApp.RunScript(
                documentSerialNumber: owner.Document.RuntimeSerialNumber,
                script: string.Join(separator: ' ', values: (Seq(command)
                    + args.Map(static arg => $"_{arg.Name} \"{(arg.Value ?? string.Empty)
                        .Replace(oldValue: "\"", newValue: "\"\"", comparisonType: StringComparison.Ordinal)
                        .Replace(oldChar: '\r', newChar: ' ')
                        .Replace(oldChar: '\n', newChar: ' ')}\"")
                    + Seq("_Enter")).AsIterable()),
                echo: owner.RunScriptEcho))
            .Map(_ => MutationReceipt.Named(name: snap.Name.Value));
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
            _ => SnapshotPairs(table: owner.Document.InstanceDefinitions, key: key)
                .Map(static pairs => (BlockOutcome)new BlockOutcome.Snapshots(Values: pairs.Map(static pair => pair.Snap))),
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
            source: pair.Snap.Source.Bind(static link => link.ToEndpoint().ToOption()),
            metadata: MetadataPatch.Empty,
            key: key).Bind(s => s.Admit(key: key))
        from receipt in PerformAuthor(owner: owner, spec: admitted, source: provided, conflict: conflict, key: key)
        select receipt;

    private static Fin<MutationReceipt> PerformBakeArchive(RhinoBlocks owner, FileEndpoint source, BakePolicy policy, Op key) =>
        from endpoint in source.Input(op: key)
        from result in key.Catch(() => {
            using File3dm? model = File3dm.ReadWithLog(path: endpoint.Path, errorLog: out _);
            return from active in Optional(model).ToFin(Fail: key.InvalidInput())
                   from report in Archive.ValidateArchiveClosure(root: active, rootPath: endpoint.Path, key: key)
                   from _gate in report switch {
                       { Valid: true } => Fin.Succ(value: unit),
                       _ => Fin.Fail<Unit>(error: key.InvalidResult(detail: nameof(BlockOp.BakeArchive))),
                   }
                   from closure in Fin.Succ(value: report.Edges)
                   from graph in Archive.From(model: active, archivePath: Some(endpoint.Path), key: key)
                   from linked in BakeArchiveLinks(owner: owner, graph: graph, closure: closure, policy: policy, key: key)
                   from authored in BakeArchiveDefinitions(owner: owner, model: active, graph: graph, policy: policy, liveByArchiveId: linked.LiveByArchiveId, key: key)
                   from placed in policy.RestoreInstancesWhen(place: () => BakeArchiveInstances(owner: owner, graph: graph, key: key))
                   select linked.Receipt + authored + placed;
        })
        select result;

    private static Fin<BlockOutcome> PerformValidateArchiveClosure(FileEndpoint source, ClosureValidationPolicy? policy, Op key) =>
        from endpoint in source.Input(op: key)
        from report in key.Catch(() => {
            using File3dm? model = File3dm.ReadWithLog(path: endpoint.Path, errorLog: out _);
            return Optional(model).ToFin(Fail: key.InvalidInput())
                .Bind(active => Archive.ValidateArchiveClosure(root: active, rootPath: endpoint.Path, policy: policy, key: key));
        })
        select (BlockOutcome)new BlockOutcome.ClosureReport(Value: report);

    private sealed record ArchiveLinkBake(MutationReceipt Receipt, HashMap<Guid, Guid> LiveByArchiveId);

    private static Fin<ArchiveLinkBake> BakeArchiveLinks(RhinoBlocks owner, Archive.Graph graph, Seq<Archive.LinkedArchiveEdge> closure, BakePolicy policy, Op key) =>
        toSeq(closure
            .Filter(static edge => edge.Depth == 0)
            .DistinctBy(static edge => edge.Link.Full.Value))
            .TraverseM(edge => edge.Link.ToEndpoint().Map(endpoint => (Edge: edge, Endpoint: endpoint)))
            .As()
            .Bind(sources => sources.IsEmpty
                ? Fin.Succ(value: new ArchiveLinkBake(Receipt: MutationReceipt.Empty, LiveByArchiveId: HashMap<Guid, Guid>()))
                : CreateArchiveLinks(owner: owner, sources: sources.Map(static source => source.Endpoint), policy: policy.EffectiveLink, key: key)
                    .Map(links => {
                        Archive.LinkedArchiveEdge[] edgeRows = [.. sources.Map(static source => source.Edge)];
                        CreatedLink[] liveRows = [.. links];
                        HashMap<string, Guid> liveByPath = toSeq(Enumerable.Range(start: 0, count: liveRows.Length))
                            .Fold(HashMap<string, Guid>(), (map, index) => map.AddOrUpdate(key: edgeRows[index].Link.Full.Value, value: liveRows[index].Id));
                        HashMap<Guid, Guid> liveByArchiveId = toSeq(graph.Definitions)
                            .Choose(definition => definition.Source.Bind(link => liveByPath.Find(key: link.Full.Value).Map(live => (ArchiveId: definition.Id.Value, LiveId: live))))
                            .Fold(HashMap<Guid, Guid>(), static (map, link) => map.AddOrUpdate(key: link.ArchiveId, value: link.LiveId));
                        return new ArchiveLinkBake(
                            Receipt: MutationReceipt.Resources(changes: links.Map(static link => link.Change)),
                            LiveByArchiveId: liveByArchiveId);
                    }));

    private static Fin<MutationReceipt> BakeArchiveDefinitions(RhinoBlocks owner, File3dm model, Archive.Graph graph, BakePolicy policy, HashMap<Guid, Guid> liveByArchiveId, Op key) {
        Seq<Definition> plan = Archive.BakePlan(graph: graph).Filter(static def => !def.IsLinked);
        return plan.Fold(
            Fin.Succ(value: new BakeArchiveState(Receipt: MutationReceipt.Empty, LiveByArchiveId: liveByArchiveId)),
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
    private static Fin<MutationReceipt> BakeArchiveInstances(RhinoBlocks owner, Archive.Graph graph, Op key) =>
        graph.Instances.IsEmpty
            ? Fin.Succ(value: MutationReceipt.Empty)
            : toSeq(graph.Instances)
                .Filter(instance => !toSeq(graph.Definitions).Bind(static definition => toSeq(definition.MemberIds)).Exists(id => id == instance.ObjectId))
                .Map(instance =>
                    from archiveDef in toSeq(graph.Definitions).Find(d => d.Id == instance.ParentDefId).ToFin(Fail: key.InvalidInput())
                    from live in Optional(owner.Document.InstanceDefinitions.Find(instanceDefinitionName: archiveDef.Name.Value)).ToFin(Fail: key.InvalidInput())
                    from idx in DefinitionIndex.From(value: live.Index, key: key)
                    from created in AddInstance(table: owner.Document.Objects, index: idx.Value, placement: Placement.Of(xform: instance.Xform), key: key)
                    select MutationReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(created)))
                .TraverseM(identity).As()
                .Map(receipts => receipts.Fold(initialState: MutationReceipt.Empty, f: static (acc, r) => acc + r));

    private static Fin<Seq<(Definition Snap, InstanceDefinition Live)>> SnapshotPairs(InstanceDefinitionTable table, Op key) {
        Option<string> anchor = BlockPaths.DocAnchor(document: table.Document);
        return Definition.List(table: table)
            .Map(live => Definition.From(definition: live, anchorDirectory: anchor, key: key).Map(snap => (Snap: snap, Live: live)))
            .TraverseM(identity).As();
    }
    private static Seq<Member> ProjectMembers(DefinitionId defId, InstanceDefinition live, Option<Seq<ExplodedPiece>> pieces) =>
        pieces.Match(
            Some: exploded => toSeq(exploded).Map(p => new Member(DefId: defId, MemberId: p.Piece.Id, Attrs: Some(p.Attrs))),
            None: () => VisitDefinitionMembers(def: live, composed: Transform.Identity)
                .Map(visit => new Member(DefId: defId, MemberId: visit.ObjectId, Attrs: Optional(visit.Attributes))));

    private static Fin<BlockOutcome> PerformExplodeInspect(RhinoBlocks owner, Guid instanceId, ExplodePolicy policy, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from live in Optional(instance.InstanceDefinition).ToFin(Fail: key.InvalidResult())
        from defId in DefinitionId.From(value: live.Id, key: key)
        let members = ProjectMembers(defId: defId, live: live, pieces: Some(toSeq(ExplodePieces(instance: instance, policy: policy))))
        select (BlockOutcome)new BlockOutcome.MembersResult(Values: members);

    private static Fin<BlockOutcome> PerformUseSubObject(RhinoBlocks owner, Guid instanceId, ComponentIndex component, Op key) =>
        from instance in AsInstance(objects: owner.Document.Objects, instanceId: instanceId, key: key)
        from valid in component is { IsSet: true, ComponentIndexType: ComponentIndexType.InstanceDefinitionPart }
            ? Fin.Succ(value: component)
            : Fin.Fail<ComponentIndex>(error: key.InvalidInput())
        from sub in Optional(instance.SubObjectFromComponentIndex(ci: valid)).ToFin(Fail: key.InvalidInput())
        from defId in DefinitionId.From(value: instance.InstanceDefinition?.Id ?? Guid.Empty, key: key)
        select (BlockOutcome)new BlockOutcome.Pieces(Values: Seq(
            new FlatPiece(
                Geometry: sub.Geometry,
                Composed: instance.InstanceXform,
                Path: [defId])));
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
            ensureIndexed: static (ctx, e) => PerformGraphEnsureIndexed(owner: ctx.Owner, refer: e.Ref, key: ctx.Key))
        .Map(result => (BlockOutcome)new BlockOutcome.GraphResult(Query: query, Value: result));

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
                Definition.List(table: owner.Document.InstanceDefinitions)
                    .Choose(d => {
                        _ = ContentIndex.RegisterDefinition(doc: owner.Document, defId: d.Id);
                        return DefinitionId.From(value: d.Id, key: key).ToOption();
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
            LiveRefs(live: pair.Live, scope: scope)
                .Bind(inst => Archive.WalkDefinitions(
                    table: owner.Document.InstanceDefinitions,
                    seed: inst,
                    parent: Transform.Identity,
                    path: [],
                    depth: 0,
                    policy: admitted,
                    flatLeaves: false,
                    key: key)
                    .Choose(static node => node.Reach)));

    private static Fin<BlockOutcome> PerformBounds(RhinoBlocks owner, DefinitionRef refer, BoundsPolicy policy, Op key) =>
        from live in FindDefinition(table: owner.Document.InstanceDefinitions, refer: refer, includeDeleted: false, key: key)
        let box = policy.Union(live: live)
        from _ in guard(box.IsValid, key.InvalidResult(detail: nameof(BoundingBox)))
        select (BlockOutcome)new BlockOutcome.Bounds(Value: box);

    private static Fin<BlockOutcome> PerformAdoptContent(RhinoBlocks owner, Op key) =>
        Fin.Succ(value: (BlockOutcome)new BlockOutcome.Adopted(Count: ContentIndex.Adopt(doc: owner.Document)));

    private static Fin<BlockOutcome> PerformGraphMembers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from live in FindDefinition(table: owner.Document.InstanceDefinitions, refer: refer, includeDeleted: false, key: key)
        from defId in DefinitionId.From(value: live.Id, key: key)
        select (BlockOutcome)new BlockOutcome.MembersResult(Values: ProjectMembers(defId: defId, live: live, pieces: None));

    private static Fin<BlockOutcome> PerformGraphInserts(RhinoBlocks owner, DefinitionRef refer, ReferenceScope scope, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from inserts in LiveRefs(live: pair.Live, scope: scope)
            .Map(static inst => Insert.From(instance: inst))
            .TraverseM(identity).As()
        select (BlockOutcome)new BlockOutcome.Inserts(Values: inserts);

    private static Fin<BlockOutcome> PerformGraphContainers(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from containers in NonNull(native: pair.Live.GetContainers())
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
        let pieces = Archive.WalkDefinitions(
            table: owner.Document.InstanceDefinitions,
            seed: instance,
            parent: Transform.Identity,
            path: [],
            depth: 0,
            policy: admitted,
            flatLeaves: true,
            key: key)
            .Choose(static node => node.Flat)
        select (BlockOutcome)new BlockOutcome.Pieces(Values: pieces);
    private static Fin<BlockOutcome> PerformPreview(RhinoBlocks owner, DefinitionRef refer, PreviewSpec spec, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        from admitted in spec.Admit(key: key)
        from handle in owner.AcquirePreview(definition: pair.Live, spec: admitted, key: key)
        select (BlockOutcome)new BlockOutcome.Preview(Handle: handle);

    private static Fin<BlockOutcome> PerformAttributeTask(RhinoBlocks owner, BlockAttributeTask task, Op key) =>
        task switch {
            BlockAttributeTask.Text x => PerformTextFields(owner: owner, refer: x.Ref, instanceId: x.InstanceId, key: key),
            BlockAttributeTask.Schema x => PerformAttributeFields(owner: owner, refer: x.Ref, key: key),
            BlockAttributeTask.Write x => PerformWriteAttributeFields(owner: owner, refer: x.Ref, values: x.Values, policy: x.Policy ?? ConstraintPolicy.Schema, instanceId: x.InstanceId, scope: x.Scope ?? ReferenceScope.TopAndNested, metadata: x.Metadata, key: key).Map(ReceiptOutcome),
            BlockAttributeTask.Matrix x => PerformAttributeMatrix(owner: owner, refs: x.Refs, scope: x.Scope, key: key),
            _ => Fin.Fail<BlockOutcome>(error: key.InvalidInput()),
        };

    private static Fin<BlockOutcome> PerformTextFields(RhinoBlocks owner, DefinitionRef refer, Option<Guid> instanceId, Op key) {
        static string Coordinate(RhinoDoc doc, InstanceObject instance, Func<Point3d, double> axis) {
            int digits = instance.Attributes.Space == ActiveSpace.PageSpace
                ? doc.PageDistanceDisplayPrecision
                : doc.ModelDistanceDisplayPrecision;
            return Math.Round(axis(arg: instance.InsertionPoint), digits, MidpointRounding.ToEven).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
               from instance in instanceId.Case switch {
                   Guid id => Optional(owner.Document.Objects.FindId(id: id) as InstanceObject)
                       .Filter(inst => inst.InstanceDefinition?.Id == pair.Live.Id)
                       .ToFin(Fail: key.InvalidInput())
                       .Map(Some),
                   _ => Fin.Succ(value: Option<InstanceObject>.None),
               }
               let blockName = pair.Snap.Name.Value
               let schema = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
               let definitionFields = Seq(
                   ("BlockInstanceCount", (pair.Live.GetReferences(wheretoLook: ReferenceScope.Top.Native)?.Length ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                   ("BlockDescription", pair.Live.Description ?? string.Empty))
               let instanceFields = instance
                   .Map(inst => Seq(
                       ("BlockName", blockName),
                       ("BlockInstanceName", Definition.NonBlank(value: inst.Attributes.Name).IfNone(blockName)),
                       ("InsertionX", Coordinate(doc: owner.Document, instance: inst, axis: static point => point.X)),
                       ("InsertionY", Coordinate(doc: owner.Document, instance: inst, axis: static point => point.Y)),
                       ("InsertionZ", Coordinate(doc: owner.Document, instance: inst, axis: static point => point.Z))))
                   .IfNone(Seq<(string, string)>())
               let attributeFields = schema.Map(field => ($"Attribute:{field.Key}", instance
                   .Bind(inst => Optional(inst.Attributes.GetUserString(key: field.Key)))
                   .Filter(static value => !string.IsNullOrEmpty(value))
                   .IfNone(field.DefaultValue ?? string.Empty)))
               let fields = (definitionFields + instanceFields + attributeFields)
                   .Fold(HashMap<string, string>(), static (m, p) => m.AddOrUpdate(key: p.Item1, value: p.Item2))
               select (BlockOutcome)new BlockOutcome.Texts(Fields: fields);
    }

    private static Fin<BlockOutcome> PerformAttributeFields(RhinoBlocks owner, DefinitionRef refer, Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
        select (BlockOutcome)new BlockOutcome.Attributes(Values: fields);

    private static Fin<MutationReceipt> PerformWriteAttributeFields(
        RhinoBlocks owner,
        DefinitionRef refer,
        HashMap<string, string> values,
        ConstraintPolicy policy,
        Option<Guid> instanceId,
        ReferenceScope scope,
        MetadataPatch? metadata,
        Op key) =>
        from pair in Resolve(table: owner.Document.InstanceDefinitions, refer: refer, key: key)
        let schema = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? [])
        from _keys in policy.AdmitValues(values: values, schema: schema, key: key)
        from idx in RequireIndex(snap: pair.Snap, key: key)
        from _meta in Optional(metadata).Filter(patch => !patch.IsEmpty).Case switch {
            MetadataPatch patch => CommitMetadata(
                table: owner.Document.InstanceDefinitions,
                snap: pair.Snap,
                patch: patch,
                key: key),
            _ => Fin.Succ(value: unit),
        }
        from targets in ResolveAttributeTargets(
            objects: owner.Document.Objects,
            live: pair.Live,
            instanceId: instanceId,
            scope: scope,
            key: key)
        from changed in targets.TraverseM(target => WriteAttributeValues(
            objects: owner.Document.Objects,
            instance: target,
            values: values,
            key: key)).As()
        select InvalidateWith(
            doc: owner.Document,
            defId: pair.Live.Id,
            value: MutationReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: changed));

    private static Fin<Seq<InstanceObject>> ResolveAttributeTargets(
        ObjectTable objects,
        InstanceDefinition live,
        Option<Guid> instanceId,
        ReferenceScope scope,
        Op key) =>
        instanceId.Case switch {
            Guid id => Optional(objects.FindId(id: id) as InstanceObject)
                .Filter(inst => inst.InstanceDefinition?.Id == live.Id)
                .ToFin(Fail: key.InvalidInput())
                .Map(inst => Seq(inst)),
            _ => Fin.Succ(value: LiveRefs(live: live, scope: scope)),
        };

    private static Fin<Guid> WriteAttributeValues(
        ObjectTable objects,
        InstanceObject instance,
        HashMap<string, string> values,
        Op key) {
        ObjectAttributes next = instance.Attributes.Duplicate();
        _ = values.Iter((k, v) => next.SetUserString(key: k, value: v));
        return key.Confirm(success: objects.ModifyAttributes(objectId: instance.Id, newAttributes: next, quiet: true))
            .Map(_ => instance.Id);
    }

    private static Fin<BlockOutcome> PerformAttributeMatrix(RhinoBlocks owner, Option<Seq<DefinitionRef>> refs, ReferenceScope scope, Op key) {
        InstanceDefinitionTable table = owner.Document.InstanceDefinitions;
        return (refs.Case switch {
            Seq<DefinitionRef> selected => selected.Map(refer => Resolve(table: table, refer: refer, key: key)).TraverseM(identity).As(),
            _ => SnapshotPairs(table: table, key: key),
        }).Map(pairs => (BlockOutcome)new BlockOutcome.AttributeMatrix(Values: pairs.Bind(pair => AttributeCells(pair: pair, scope: scope))));
    }

    private static Seq<AttributeCell> AttributeCells((Definition Snap, InstanceDefinition Live) pair, ReferenceScope scope) {
        Seq<TextFields.InstanceAttributeField> fields = toSeq(TextFields.GetInstanceAttributeFields(idef: pair.Live) ?? []);
        Seq<InstanceObject> refs = LiveRefs(live: pair.Live, scope: scope);
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

}
