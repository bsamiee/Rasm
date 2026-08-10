# [RASM_RHINO_BLOCK_OPERATIONS]

Block operations (`Rasm.Rhino.Blocks`) own one closed mutation family, one closed read family, one admitted transaction, and one fact-stream receipt. `Blocks.Commit` derives session needs from operation traits, acquires geometry through `GeometryIntake`, seals one shared `UndoBracket`, restores redraw through an accumulating rail, and emits stable definition and object evidence.

## [01]-[INDEX]

- [02]-[OPERATION_FAMILY]: `BlockOp` carrying every verified definition mutation and block-specific instance operation as generated values, `BlockMember` pairing one admitted `GeometryIntake` with one attribute set under a lease that closes on every exit.
- [03]-[READ_FAMILY]: `BlockAsk`/`BlockAnswer` closing state, dependency, preview, field extraction, token composition, name minting, and instance explosion, `ExplodedPiece` owning its detached geometry and attribute custody.
- [04]-[COMMIT_SPINE]: `BlockTransaction` admitting one homogeneous program through `BlockTrait`-derived undo and kernel-context requirements, and `Blocks.Commit` walking the one `DocumentCommit.Sealed` entry.
- [05]-[RECEIPTS]: `BlockSlot`/`BlockBody` the folder's contribution to the Document spine's shared `FactStream`, `BlockFacts` its mint and projection extension surface, path, tally, signal, and undo facts sharing one closed payload family and slot projections deriving from `Facts`.
- [06]-[SURFACE_LEDGER]: owner-to-ingress-to-rail-to-egress roster across `BlockOp`, `BlockTransaction`, `Blocks`, and the slot and body vocabularies.

## [02]-[OPERATION_FAMILY]

`BlockOp` carries every verified definition mutation and block-specific instance operation. Shared metadata, linked-source, interaction, traversal, compaction, placement, and bake decisions enter as generated values; host booleans are projections of those values, never call-site discriminants.

`BlockMember` pairs one already-admitted `GeometryIntake` with one attribute set. Acquisition retains that bijection, and every lease closes after the host call or on partial admission failure.

Names and paths carry their spine owners: `BlockMetadata` takes the Document spine's `ResourceName`, every file address takes `DocumentPath`, and `SourceReference` closes the linked-source address as one value — absolute or anchored — whose `Use` mints the disposable `FileReference` inside an owned lease and releases it the moment the host call returns. A bare `string` name or path at any of these slots is the deleted form.

The history record is single-use host custody: `Placement.Recorded` carries `Lease<HistoryRecord>`, the placement arm consumes it through `Use`, and no durable payload column retains it past its one `AddInstanceObject`.

The source axis crosses through `SourceMode`, never a raw `InstanceDefinitionUpdateType` comparison: `Reads` gates the linked-source verbs, and the retired ordinal folds onto `Static` at admission so no fence spells the `[Obsolete]` host case.

`Bake` compares produced object ids with the source definition roster; shallow expansion requires equality, while recursive expansion requires at least the direct roster. Zero-member definitions admit the native null no-op as an empty object roster, every bake emits a produced-count tally, and partial insertion returns a typed failure.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class BlockHyperlink {
    public string Url { get; }
    public string Tag { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string url,
        ref string tag) {
        url = url?.Trim() ?? string.Empty;
        validationError = !string.IsNullOrWhiteSpace(value: url)
            && Uri.TryCreate(uriString: url, uriKind: UriKind.RelativeOrAbsolute, result: out _)
            && tag is not null
            ? null
            : new ValidationError(message: "block hyperlink is invalid");
    }
}

[ComplexValueObject]
public sealed partial class BlockMetadata {
    public ResourceName Name { get; }
    public string Description { get; }
    public Option<BlockHyperlink> Hyperlink { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ResourceName name,
        ref string description,
        ref Option<BlockHyperlink> hyperlink) =>
        validationError = name is null || description is null
            ? new ValidationError(message: "block metadata is invalid")
            : validationError;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SourceReference {
    private SourceReference() { }
    public sealed record Absolute(DocumentPath Full) : SourceReference;

    // `DocumentPath` admits only a fully qualified path, so the host's relative leg keeps admitted text: the
    // relative anchor is a genuinely distinct address shape, not a second spelling of the absolute one.
    public sealed record Anchored(DocumentPath Full, string Relative) : SourceReference;

    public static Fin<SourceReference> Of(string full, Option<string> relative, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in DocumentPath.Of(value: full, key: op)
               from anchor in relative.Traverse(value => op.AcceptText(value: value)).As()
               select anchor.Case switch {
                   string text => (SourceReference)new Anchored(Full: admitted, Relative: text),
                   _ => new Absolute(Full: admitted),
               };
    }

    internal DocumentPath Full => Switch(
        absolute: static row => row.Full,
        anchored: static row => row.Full);

    // Exemption: `FileReference : IDisposable`, so each mint enters an owned lease whose `Use` disposes it
    // the moment the host call returns — the one native carrier this operation family holds.
    internal Fin<T> Use<T>(Func<FileReference, Fin<T>> body, Op op) =>
        op.Catch(() => Switch(
            context: body,
            absolute: static (use, row) => new Lease<FileReference>.Owned(
                Value: FileReference.CreateFromFullPath(fullPath: row.Full.Value)).Use(use),
            anchored: static (use, row) => new Lease<FileReference>.Owned(
                Value: FileReference.CreateFromFullAndRelativePaths(
                    fullPath: row.Full.Value, relativePath: row.Relative)).Use(use)));
}

[ComplexValueObject]
public sealed partial class BlockMember {
    public GeometryIntake Source { get; }
    public ObjectAttributes Attributes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryIntake source,
        ref ObjectAttributes attributes) {
        validationError = source is not null && attributes is not null
            ? validationError
            : new ValidationError(message: "block member is invalid");
    }
}

[SmartEnum<int>]
public sealed partial class LinkMode {
    public static readonly LinkMode Linked = new(key: 0, updateType: InstanceDefinitionUpdateType.Linked);
    public static readonly LinkMode LinkedAndEmbedded = new(key: 1, updateType: InstanceDefinitionUpdateType.LinkedAndEmbedded);

    public InstanceDefinitionUpdateType UpdateType { get; }
}

[SmartEnum<int>]
public sealed partial class LinkTraversal {
    public static readonly LinkTraversal Current = new(key: 0, nestedLinks: false);
    public static readonly LinkTraversal Closure = new(key: 1, nestedLinks: true);

    public bool NestedLinks { get; }
}

[SmartEnum<int>]
public sealed partial class CompactPolicy {
    public static readonly CompactPolicy PreserveUndo = new(key: 0, ignoreUndoReferences: false);
    public static readonly CompactPolicy ReclaimUndo = new(key: 1, ignoreUndoReferences: true);

    public bool IgnoreUndoReferences { get; }
}

[SmartEnum<int>]
public sealed partial class InstanceDisposition {
    public static readonly InstanceDisposition Retain = new(key: 0, deleteInstance: false);
    public static readonly InstanceDisposition Replace = new(key: 1, deleteInstance: true);

    public bool DeleteInstance { get; }
}

[SmartEnum<int>]
internal sealed partial class BlockTrait {
    public static readonly BlockTrait Mutation = new(key: 0, recordsUndo: true, requiresContext: false);
    public static readonly BlockTrait Contextual = new(key: 1, recordsUndo: true, requiresContext: true);
    public static readonly BlockTrait Unrecorded = new(key: 2, recordsUndo: false, requiresContext: false);

    public bool RecordsUndo { get; }
    public bool RequiresContext { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockOp {
    private BlockOp() { }
    public sealed record Author(
        BlockMetadata Metadata,
        Point3d BasePoint,
        Seq<BlockMember> Members,
        ConflictPolicy Conflict) : BlockOp;
    public sealed record Amend(ResourceRef Target, BlockMetadata Metadata, HostInteraction Interaction) : BlockOp;
    public sealed record Regeometry(ResourceRef Target, Seq<BlockMember> Members) : BlockOp;
    public sealed record Rebind(
        ResourceRef Target,
        SourceReference Source,
        LinkMode Mode,
        HostInteraction Interaction) : BlockOp;
    public sealed record Sever(ResourceRef Target, HostInteraction Interaction) : BlockOp;
    public sealed record Refresh(ResourceRef Target) : BlockOp;
    public sealed record Retarget(
        ResourceRef Target,
        DocumentPath Filename,
        LinkTraversal Traversal,
        HostInteraction Interaction) : BlockOp;
    public sealed record Style(ResourceRef Target, LayerScope LayerStyle) : BlockOp;
    public sealed record Delete(ResourceRef Target, DeletionPolicy Policy) : BlockOp;
    public sealed record Undelete(ResourceRef Target) : BlockOp;
    public sealed record Purge(ResourceRef Target) : BlockOp;
    public sealed record PurgeUnused : BlockOp;
    public sealed record Compact(CompactPolicy Policy) : BlockOp;
    public sealed record Export(ResourceRef Target, DocumentPath Path) : BlockOp;
    public sealed record Place(ResourceRef Target, Seq<Placement> Instances) : BlockOp;
    public sealed record Repoint(TableTarget Instances, ResourceRef Target) : BlockOp;
    public sealed record Bake(Guid InstanceId, ExplodeDepth Depth, InstanceDisposition Disposition) : BlockOp;

    // Every case names its own trait. The catch-all read as "everything else records undo", so a new case that
    // opened no undo record — the exact shape `Purge`, `Compact`, and `Export` already are — would silently join
    // the recorded lane and the transaction admission would accept it into a sealed program it cannot roll back.
    internal BlockTrait Traits => Map(
        author: BlockTrait.Contextual,
        amend: BlockTrait.Mutation,
        regeometry: BlockTrait.Contextual,
        rebind: BlockTrait.Mutation,
        sever: BlockTrait.Mutation,
        refresh: BlockTrait.Mutation,
        retarget: BlockTrait.Mutation,
        style: BlockTrait.Mutation,
        delete: BlockTrait.Mutation,
        undelete: BlockTrait.Mutation,
        purge: BlockTrait.Unrecorded,
        purgeUnused: BlockTrait.Unrecorded,
        compact: BlockTrait.Unrecorded,
        export: BlockTrait.Unrecorded,
        place: BlockTrait.Mutation,
        repoint: BlockTrait.Mutation,
        bake: BlockTrait.Mutation);

    internal Fin<BlockReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            context: (Document: document, Domain: domain, Op: op),
            author: static (context, edit) =>
                from metadata in context.Op.Need(edit.Metadata)
                from conflict in context.Op.Need(edit.Conflict)
                from _ in guard(edit.BasePoint.IsValid, context.Op.InvalidInput()).ToFin()
                from name in context.Op.Need(metadata.Name)
                from resolved in Optional(context.Document.InstanceDefinitions.Find(name.Value)).Case switch {
                    InstanceDefinition existing => conflict.Switch(
                        (Existing: existing, Document: context.Document, Name: name, Op: context.Op),
                        fail: static held => Fin.Fail<(ResourceName Name, Option<InstanceDefinition> Reused)>(
                            error: held.Op.InvalidInput()),
                        reuse: static held => Fin.Succ(value: (held.Name, Some(held.Existing))),
                        mint: static held => held.Op.AcceptText(value: held.Document.InstanceDefinitions
                            .GetUnusedInstanceDefinitionName(root: held.Name.Value))
                            .Map(minted => (ResourceName.Create(minted), Option<InstanceDefinition>.None))),
                    _ => Fin.Succ(value: (name, Option<InstanceDefinition>.None)),
                }
                from receipt in resolved.Reused.Case switch {
                    InstanceDefinition reused => BlockReceipt.Definition(
                        slot: BlockSlot.Reused,
                        definition: reused,
                        key: context.Op),
                    _ => Admitted(
                        members: edit.Members,
                        domain: context.Domain,
                        op: context.Op,
                        run: (geometry, attributes) =>
                            from index in context.Op.Catch(() => {
                                int added = metadata.Hyperlink.Case switch {
                                    BlockHyperlink hyperlink => context.Document.InstanceDefinitions.Add(
                                        name: resolved.Name.Value,
                                        description: metadata.Description,
                                        url: hyperlink.Url,
                                        urlTag: hyperlink.Tag,
                                        basePoint: edit.BasePoint,
                                        geometry: geometry,
                                        attributes: attributes),
                                    _ => context.Document.InstanceDefinitions.Add(
                                        name: resolved.Name.Value,
                                        description: metadata.Description,
                                        basePoint: edit.BasePoint,
                                        geometry: geometry,
                                        attributes: attributes),
                                };
                                return added >= 0
                                    ? Fin.Succ(value: added)
                                    : Fin.Fail<int>(error: context.Op.InvalidResult());
                            })
                            from created in Receipt(
                                document: context.Document,
                                index: index,
                                slot: BlockSlot.Authored,
                                op: context.Op)
                            select created),
                }
                select receipt,
            amend: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from metadata in context.Op.Need(edit.Metadata)
                from interaction in context.Op.Need(edit.Interaction)
                let hyperlink = metadata.Hyperlink.Case switch {
                    BlockHyperlink value => (Url: value.Url, Tag: value.Tag),
                    _ => (Url: string.Empty, Tag: string.Empty),
                }
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Modify(
                    idefIndex: definition.Index, newName: metadata.Name.Value, newDescription: metadata.Description,
                    newUrl: hyperlink.Url, newUrlTag: hyperlink.Tag, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Amended, definition: definition, key: context.Op)
                select receipt,
            regeometry: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(!mode.Reads, context.Op.InvalidInput()).ToFin()
                from receipt in Admitted(
                    members: edit.Members,
                    domain: context.Domain,
                    op: context.Op,
                    run: (geometry, attributes) =>
                        from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifyGeometry(
                            idefIndex: definition.Index, newGeometry: geometry, newAttributes: attributes))
                        from receipt in BlockReceipt.Definition(
                            slot: BlockSlot.Regeometried,
                            definition: definition,
                            key: context.Op)
                        select receipt)
                select receipt,
            rebind: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from source in context.Op.Need(edit.Source)
                from mode in context.Op.Need(edit.Mode)
                from interaction in context.Op.Need(edit.Interaction)
                from _ in source.Use(
                    body: reference => context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifySourceArchive(
                        idefIndex: definition.Index, sourceArchive: reference, updateType: mode.UpdateType, quiet: interaction.IsQuiet)),
                    op: context.Op)
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Rebound,
                    definition: definition,
                    key: context.Op,
                    path: Some(source.Full))
                select receipt,
            sever: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from interaction in context.Op.Need(edit.Interaction)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.DestroySourceArchive(
                    definition: definition, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Severed, definition: definition, key: context.Op)
                select receipt,
            refresh: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(!definition.IsTenuous && mode.Reads, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Refreshed, definition: definition, key: context.Op)
                select receipt,
            retarget: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from path in context.Op.Need(edit.Filename)
                from traversal in context.Op.Need(edit.Traversal)
                from interaction in context.Op.Need(edit.Interaction)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: definition.Index, filename: path.Value, updateNestedLinks: traversal.NestedLinks, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Retargeted,
                    definition: definition,
                    key: context.Op,
                    path: Some(path))
                select receipt,
            style: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from scope in context.Op.Need(edit.LayerStyle)
                from _ in guard(mode.Reads, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    definition.LayerStyle = scope.Host;
                    return context.Op.Confirm(success: definition.LayerStyle == scope.Host);
                })
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Styled, definition: definition, key: context.Op)
                select receipt,
            delete: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from policy in context.Op.Need(edit.Policy)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Delete(
                    idefIndex: definition.Index, deleteReferences: policy.DeleteReferences, quiet: policy.Interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Deleted, definition: definition, key: context.Op)
                select receipt,
            undelete: static (context, edit) =>
                from definition in Deleted(document: context.Document, target: edit.Target, op: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Undelete(idefIndex: definition.Index))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Revived, definition: definition, key: context.Op)
                select receipt,
            purge: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Purge(idefIndex: definition.Index))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Purged, definition: definition, key: context.Op)
                select receipt,
            purgeUnused: static (context, _) =>
                from tally in context.Op.Catch(() => Fin.Succ(value: context.Document.InstanceDefinitions.PurgeUnused()))
                from receipt in BlockReceipt.Tally(slot: BlockSlot.Reclaimed, count: tally, key: context.Op)
                select receipt,
            compact: static (context, edit) =>
                from policy in context.Op.Need(edit.Policy)
                from _ in context.Op.Catch(() => {
                    context.Document.InstanceDefinitions.Compact(ignoreUndoReferences: policy.IgnoreUndoReferences);
                    return Fin.Succ(value: unit);
                })
                from receipt in BlockReceipt.Signal(slot: BlockSlot.Compacted, key: context.Op)
                select receipt,
            export: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from path in context.Op.Need(edit.Path)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Export(
                    idefIndex: definition.Index, filename: path.Value))
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Exported,
                    definition: definition,
                    key: context.Op,
                    path: Some(path))
                select receipt,
            place: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(!edit.Instances.IsEmpty, context.Op.InvalidInput()).ToFin()
                from placed in edit.Instances.TraverseM(placement => Optional(placement)
                    .ToFin(Fail: context.Op.InvalidInput()).Bind(active => active.Switch(
                        context: (Document: context.Document, Index: definition.Index, Op: context.Op),
                        bare: static (ctx, request) => Place(motion: request.Motion, op: ctx.Op,
                            add: () => ctx.Document.Objects.AddInstanceObject(
                                instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion)),
                        attributed: static (ctx, request) =>
                            from _ in guard(request.Attributes is not null, ctx.Op.InvalidInput()).ToFin()
                            from id in Place(motion: request.Motion, op: ctx.Op,
                                add: () => ctx.Document.Objects.AddInstanceObject(
                                    instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion, attributes: request.Attributes))
                            select id,
                        // The history record is single-use host custody: it threads into exactly one `AddInstanceObject`
                        // and is released the moment that call returns, so it never survives as a durable payload.
                        recorded: static (ctx, request) =>
                            from _ in guard(
                                request.Attributes is not null && request.History is not null && request.Kind is not null,
                                ctx.Op.InvalidInput()).ToFin()
                            from id in request.History.Use(record => Place(motion: request.Motion, op: ctx.Op,
                                add: () => ctx.Document.Objects.AddInstanceObject(
                                    instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion, attributes: request.Attributes,
                                    history: record, reference: request.Kind.IsReference)))
                            select id))).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Placed, ids: placed, key: context.Op)
                select receipt,
            repoint: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from target in context.Op.Need(edit.Instances)
                from ids in target.Resolve(document: context.Document, key: context.Op)
                from repointed in ids.TraverseM(id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(
                    objectId: id, instanceDefinitionIndex: definition.Index)).Map(_ => id)).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Repointed, ids: repointed, key: context.Op)
                select receipt,
            bake: static (context, edit) =>
                from _ in guard(edit.InstanceId != Guid.Empty, context.Op.InvalidInput()).ToFin()
                from depth in context.Op.Need(edit.Depth)
                from disposition in context.Op.Need(edit.Disposition)
                from native in Optional(context.Document.Objects.FindId(edit.InstanceId))
                    .ToFin(Fail: context.Op.MissingContext())
                from instance in context.Op.Need(native as InstanceObject)
                from expected in context.Op.Catch(() => Optional(instance.InstanceDefinition)
                    .ToFin(Fail: context.Op.InvalidResult())
                    .Map(static definition => definition.ObjectCount))
                from ids in context.Op.Catch(() => {
                    Guid[]? pieces = context.Document.Objects.AddExplodedInstancePieces(
                        instance: instance,
                        explodeNestedInstances: depth.Nested,
                        deleteInstance: disposition.DeleteInstance);
                    return pieces is not null
                        ? Fin.Succ(value: toSeq(pieces))
                        : expected == 0
                            ? Fin.Succ(value: Seq<Guid>())
                            : Fin.Fail<Seq<Guid>>(error: context.Op.InvalidResult());
                })
                from __ in guard(
                    expected >= 0 && (depth.Nested ? ids.Count >= expected : ids.Count == expected),
                    context.Op.InvalidResult()).ToFin()
                from objects in ids.IsEmpty
                    ? Fin.Succ(value: BlockReceipt.Empty)
                    : BlockReceipt.Objects(slot: BlockSlot.Baked, ids: ids, key: context.Op)
                from tally in BlockReceipt.Tally(slot: BlockSlot.Baked, count: ids.Count, key: context.Op)
                select objects + tally);

    private static Fin<BlockReceipt> Receipt(RhinoDoc document, int index, BlockSlot slot, Op op) =>
        from definition in Optional(document.InstanceDefinitions[index]).ToFin(Fail: op.InvalidResult())
        from receipt in BlockReceipt.Definition(slot: slot, definition: definition, key: op)
        select receipt;

    private static Fin<Guid> Place(Transform motion, Op op, Func<Guid> add) =>
        from _ in guard(motion.IsValid, op.InvalidInput()).ToFin()
        from id in op.Catch(() => Optional(add())
            .Filter(static value => value != Guid.Empty)
            .ToFin(Fail: op.InvalidResult()))
        select id;

    private static Fin<InstanceDefinition> Deleted(RhinoDoc document, ResourceRef target, Op op) {
        Seq<InstanceDefinition> roster = toSeq(document.InstanceDefinitions.GetList(ignoreDeleted: false))
            .Choose(static definition => Optional(definition))
            .Filter(static definition => definition.IsDeleted);
        return op.Need(target).Bind(active => active.Switch(
            context: (Roster: roster, Op: op),
            byId: static (ctx, value) => ctx.Roster.Find(definition => definition.Id == value.Value)
                .ToFin(Fail: ctx.Op.MissingContext()),
            byName: static (ctx, value) => ctx.Roster
                .Find(definition => string.Equals(definition.Name, value.Value, StringComparison.OrdinalIgnoreCase))
                .ToFin(Fail: ctx.Op.MissingContext()),
            byIndex: static (ctx, value) => ctx.Roster.Find(definition => definition.Index == value.Value)
                .ToFin(Fail: ctx.Op.MissingContext())));
    }

    private static Fin<BlockReceipt> Admitted(
        Seq<BlockMember> members,
        Option<Context> domain,
        Op op,
        Func<IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>, Fin<BlockReceipt>> run) =>
        members.IsEmpty
            ? op.Catch(() => run(Array.Empty<GeometryBase>(), Array.Empty<ObjectAttributes>()))
            : from active in domain.ToFin(Fail: op.MissingContext())
              from admitted in Leased(members: members, domain: active, op: op)
              from receipt in op.Catch(() => {
                  try {
                      return run(
                          admitted.Map(static member => member.Geometry.Resource).AsIterable(),
                          admitted.Map(static member => member.Attributes).AsIterable());
                  }
                  finally {
                      admitted.Iter(static member => member.Geometry.Dispose());
                  }
              })
              select receipt;

    private static Fin<Seq<(Lease<GeometryBase> Geometry, ObjectAttributes Attributes)>> Leased(
        Seq<BlockMember> members,
        Context domain,
        Op op) =>
        DocumentCommit.Compensated(
            source: members,
            land: member => op.Need(member)
                .Bind(active => active.Source.Admit(domain: domain, key: op)
                    .Map(geometry => (Geometry: geometry, Attributes: active.Attributes))),
            rollback: landed => Fin.Succ(value: ignore(landed.Iter(static prior => prior.Geometry.Dispose()))));
}
```

## [03]-[READ_FAMILY]

`BlockAsk` closes state, dependency, preview, field extraction, token composition, name minting, and instance explosion. `FieldSource.Read` co-locates every text-field dispatch with its case family, so `Blocks.Ask` stays one flat switch. `Blocks.Ask` keeps each native handle inside one read demand; answers carry snapshots, scalars, descriptors, or explicit owned leases.

`ExplodedPiece` owns its detached geometry and native attribute copy. Array cardinality is proven before crossing; every exit attempts release of all captured source geometries, untransferred attributes, and failed-prefix products, while success transfers only product attributes to caller custody.

`Capture` is the statement-shaped native out-parameter boundary; both overloads collapse immediately onto the same tuple rail.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldSource {
    private FieldSource() { }
    public sealed record Text(string Value) : FieldSource;
    public sealed record Object(TableTarget Target) : FieldSource;
    public sealed record Definition(ResourceRef Target) : FieldSource;
    public sealed record Token(string Key, string Prompt, string DefaultValue) : FieldSource;

    internal Fin<BlockAnswer> Read(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            text: static (ctx, source) =>
                from text in ctx.Op.AcceptText(value: source.Value)
                from descriptors in ctx.Op.Catch(() => Fin.Succ(
                    value: Described(descriptors: TextFields.GetInstanceAttributeFields(str: text))))
                select (BlockAnswer)new BlockAnswer.Fields(Descriptors: descriptors),
            @object: static (ctx, source) =>
                from target in ctx.Op.Need(source.Target)
                from ids in target.Resolve(document: ctx.Document, key: ctx.Op)
                from id in ids.Head.Filter(_ => ids.Count == 1).ToFin(Fail: ctx.Op.InvalidInput())
                from native in Optional(ctx.Document.Objects.FindId(id)).ToFin(Fail: ctx.Op.MissingContext())
                from text in ctx.Op.Need(native as TextObject)
                from descriptors in ctx.Op.Catch(() => Fin.Succ(
                    value: Described(descriptors: TextFields.GetInstanceAttributeFields(text: text))))
                select (BlockAnswer)new BlockAnswer.Fields(Descriptors: descriptors),
            definition: static (ctx, source) =>
                from definition in Definitions.Resolve(target: source.Target, document: ctx.Document, key: ctx.Op)
                from descriptors in ctx.Op.Catch(() => Fin.Succ(
                    value: Described(descriptors: TextFields.GetInstanceAttributeFields(idef: definition))))
                select (BlockAnswer)new BlockAnswer.Fields(Descriptors: descriptors),
            token: static (ctx, source) =>
                from key in ctx.Op.AcceptText(value: source.Key)
                from prompt in ctx.Op.AcceptText(value: source.Prompt)
                from fallback in ctx.Op.Need(source.DefaultValue)
                from token in ctx.Op.Catch(() => ctx.Op.AcceptText(value: TextFields.BlockAttributeText(
                    key: key, prompt: prompt, defaultValue: fallback)))
                select (BlockAnswer)new BlockAnswer.Token(Value: token));

    private static Arr<BlockField> Described(IEnumerable<TextFields.InstanceAttributeField> descriptors) =>
        toArr(descriptors).Map(static descriptor => new BlockField(
            Key: descriptor.Key, Prompt: descriptor.Prompt, DefaultValue: descriptor.DefaultValue));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAsk {
    private BlockAsk() { }
    public sealed record State(ResourceRef Target, ReferenceScope Scope) : BlockAsk;
    public sealed record Dependency(ResourceRef Target, BlockDependency Probe) : BlockAsk;
    public sealed record Preview(ResourceRef Target, BlockPreview Spec) : BlockAsk;
    public sealed record Fields(FieldSource Source) : BlockAsk;
    public sealed record MintName(Option<ResourceName> Root) : BlockAsk;
    public sealed record Pieces(Guid InstanceId, ExplodePolicy Policy) : BlockAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAnswer : IDetachedDocumentResult {
    private BlockAnswer() { }
    public sealed record State(BlockSnapshot Snapshot) : BlockAnswer;
    public sealed record Dependency(BlockDependencyAnswer Measure) : BlockAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Preview) : BlockAnswer;
    public sealed record Fields(Arr<BlockField> Descriptors) : BlockAnswer;
    public sealed record Token(string Value) : BlockAnswer;
    public sealed record Minted(ResourceName Name) : BlockAnswer;
    public sealed record Pieces(Seq<ExplodedPiece> Products) : BlockAnswer;
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record BlockField(
    string Key,
    string Prompt,
    string DefaultValue) : IDetachedDocumentResult;

public sealed class ExplodedPiece : IDisposable {
    private int disposed;

    internal ExplodedPiece(GeometryHandle geometry, ObjectAttributes attributes, Transform motion) =>
        (Geometry, Attributes, Motion) = (geometry, attributes, motion);

    public GeometryHandle Geometry { get; }
    public ObjectAttributes Attributes { get; }
    public Transform Motion { get; }

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref disposed, value: 1) == 0) {
            try {
                Geometry.Dispose();
            }
            finally {
                Attributes.Dispose();
            }
        }
    }
}
```

## [04]-[COMMIT_SPINE]

`BlockTransaction` admits one homogeneous program. `BlockTrait` derives undo and kernel-context requirements from each `BlockOp` case, so a mixed recorded/unrecorded program fails before document acquisition and no transaction flag can contradict its operations.

`Blocks.Commit` walks the shared commit entry: needs derive through `SessionNeed.Mutation`, one document demand, optional kernel context, and `DocumentCommit.Sealed` owns the bracket, restoration, and post-restore redraw — a hand-spelled `UndoBracket.Begin` or redraw triple beside it is the deleted form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public sealed class BlockTransaction {
    private BlockTransaction(string name, Seq<BlockOp> operations, RedrawPolicy redraw, bool recordsUndo) =>
        (Name, Operations, Redraw, RecordsUndo) = (name, operations, redraw, recordsUndo);

    public string Name { get; }
    public Seq<BlockOp> Operations { get; }
    public RedrawPolicy Redraw { get; }
    internal bool RecordsUndo { get; }

    public static Fin<BlockTransaction> Batch(string name, RedrawPolicy redraw, params ReadOnlySpan<BlockOp> operations) {
        Op op = Op.Of();
        return from admitted in op.AcceptText(value: name)
               from policy in op.Need(redraw)
               from program in LanguageExt.Iterable<BlockOp>.FromSpan(operations).ToSeq()
                   .TraverseM(operation => op.Need(operation))
                   .As()
               from _ in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               let records = program.Head.Map(static operation => operation.Traits.RecordsUndo).IfNone(false)
               from __ in guard(program.ForAll(operation => operation.Traits.RecordsUndo == records), op.InvalidInput()).ToFin()
               select new BlockTransaction(name: admitted, operations: program, redraw: policy, recordsUndo: records);
    }
}

// --- [SERVICES] ----------------------------------------------------------------------------
public static class Blocks {
    public static Fin<BlockReceipt> Commit(DocumentSession session, BlockTransaction transaction) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from plan in op.Need(transaction)
               from receipt in owner.Demand(
                   use: document => Run(document: document, plan: plan, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(undo: plan.RecordsUndo, redraw: plan.Redraw).ToArray())
               select receipt;
    }

    public static Fin<BlockAnswer> Ask(DocumentSession session, BlockAsk request) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from active in op.Need(request)
               from answer in owner.Demand(
                   use: document => active.Switch(
                       context: (Document: document, Op: op),
                       state: static (ctx, ask) =>
                           from scope in ctx.Op.Need(ask.Scope)
                           from snapshot in BlockSnapshot.Of(
                               target: ask.Target,
                               document: ctx.Document,
                               scope: scope,
                               key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.State(Snapshot: snapshot),
                       dependency: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from probe in ctx.Op.Need(ask.Probe)
                           from measure in probe.Measure(owner: definition, document: ctx.Document, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Dependency(Measure: measure),
                       preview: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from spec in ctx.Op.Need(ask.Spec)
                           from bitmap in spec.Render(definition: definition, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Rendered(
                               Preview: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
                       fields: static (ctx, ask) => ctx.Op.Need(ask.Source)
                           .Bind(source => source.Read(document: ctx.Document, op: ctx.Op)),
                       mintName: static (ctx, ask) =>
                           from root in ask.Root.Traverse(value => ctx.Op.Need(value)).As()
                           from minted in ctx.Op.Catch(() => ctx.Op.AcceptText(value: root.Case switch {
                               ResourceName value => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: value.Value),
                               _ => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(),
                           }))
                           select (BlockAnswer)new BlockAnswer.Minted(Name: ResourceName.Create(minted)),
                       pieces: static (ctx, ask) =>
                           from _ in guard(ask.InstanceId != Guid.Empty, ctx.Op.InvalidInput()).ToFin()
                           from native in Optional(ctx.Document.Objects.FindId(ask.InstanceId))
                               .ToFin(Fail: ctx.Op.MissingContext())
                           from instance in ctx.Op.Need(native as InstanceObject)
                           from products in Exploded(instance: instance, policy: ask.Policy, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Pieces(Products: products)),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    private static Fin<BlockReceipt> Run(RhinoDoc document, BlockTransaction plan, Op op) =>
        from domain in plan.Operations.Exists(static operation => operation.Traits.RequiresContext)
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from receipt in DocumentCommit.Sealed(
            document: document,
            name: plan.Name,
            recordsUndo: plan.RecordsUndo,
            redraw: plan.Redraw,
            run: () => plan.Operations
                .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op))
                .As()
                .Map(static receipts => receipts.Fold(BlockReceipt.Empty, static (state, value) => state + value)),
            stamp: static (receipt, serial) => receipt.Stamped(
                slot: BlockSlot.Undo,
                record: static stamped => new BlockBody.Record(Serial: stamped),
                serial: serial),
            op: op)
        select receipt;

    private static Fin<Seq<ExplodedPiece>> Exploded(InstanceObject instance, ExplodePolicy policy, Op key) =>
        key.Need(policy).Bind(active => key.Catch(() => {
            Fin<(RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions)> native = active.Switch(
                all: request => Capture(instance: instance, depth: request.Depth, viewport: Option<Guid>.None, key: key),
                visible: request => request.ViewportId != Guid.Empty
                    ? Capture(instance: instance, depth: request.Depth, viewport: Some(request.ViewportId), key: key)
                    : Fin.Fail<(RhinoObject[], ObjectAttributes[], Transform[])>(error: key.InvalidInput()));
            return from captured in native
                   from _ in guard(
                       captured.Pieces.Length == captured.Attributes.Length
                           && captured.Pieces.Length == captured.Motions.Length,
                       key.InvalidResult()).ToFin().MapFail(primary => ReleaseCaptured(
                           captured: captured,
                           products: Seq<ExplodedPiece>(),
                           retainProducts: false,
                           key: key).Match(
                               Succ: _ => primary,
                               Fail: cleanup => primary + cleanup))
                   from products in toSeq(Enumerable.Range(start: 0, count: captured.Pieces.Length)).Fold(
                       Fin.Succ(value: Seq<ExplodedPiece>()),
                       (rail, index) => rail.Bind(held => (
                           from piece in Optional(captured.Pieces[index]).ToFin(Fail: key.InvalidResult())
                           from attribute in Optional(captured.Attributes[index]).ToFin(Fail: key.InvalidResult())
                           from geometry in Optional(piece.Geometry).ToFin(Fail: key.InvalidResult())
                           from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
                           select new ExplodedPiece(geometry: handle, attributes: attribute, motion: captured.Motions[index]))
                           .Map(held.Add)
                           .MapFail(primary => ReleaseCaptured(
                               captured: captured,
                               products: held,
                               retainProducts: false,
                               key: key).Match(
                                   Succ: _ => primary,
                                   Fail: cleanup => primary + cleanup))))
                   from __ in ReleaseCaptured(
                       captured: captured,
                       products: products,
                       retainProducts: true,
                       key: key).MapFail(primary => ReleaseProducts(products: products, key: key).Match(
                           Succ: _ => primary,
                           Fail: cleanup => primary + cleanup))
                   select products;
        }));

    // Custody direction is the parent's: an exploded piece's `Geometry` is a non-owning const wrapper parented to
    // its `RhinoObject`, so the release disposes the PARENT and the wrapper falls with it — disposing the child
    // while leaking the parent inverts ownership and double-frees at parent teardown.
    private static Fin<Unit> ReleaseCaptured(
        (RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions) captured,
        Seq<ExplodedPiece> products,
        bool retainProducts,
        Op key) {
        System.Collections.Generic.HashSet<ObjectAttributes> transferred = new(ReferenceEqualityComparer.Instance);
        products.Iter(product => transferred.Add(item: product.Attributes));
        Seq<Action> actions = toSeq(captured.Pieces)
            .Choose(static piece => Optional(piece))
            .Map(static piece => new Action(piece.Dispose))
            + (retainProducts ? Seq<Action>() : products.Map(static product => new Action(product.Dispose)))
            + toSeq(captured.Attributes)
                .Choose(static attributes => Optional(attributes))
                .Filter(attributes => !transferred.Contains(item: attributes))
                .Map(static attributes => new Action(attributes.Dispose));
        return actions
            .Traverse(action => key.Catch(() => Fin.Succ(value: Op.Side(action))).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);
    }

    private static Fin<Unit> ReleaseProducts(Seq<ExplodedPiece> products, Op key) => products
        .Traverse(product => key.Catch(() => Fin.Succ(value: Op.Side(product.Dispose))).ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Fin<(RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions)> Capture(
        InstanceObject instance,
        ExplodeDepth depth,
        Option<Guid> viewport,
        Op key) =>
        key.Need(depth).Bind(active => key.Catch(() => {
            if (viewport.Case is Guid viewportId) {
                instance.Explode(
                    skipHiddenPieces: true, viewportId: viewportId, explodeNestedInstances: active.Nested,
                    pieces: out RhinoObject[] visible,
                    pieceAttributes: out ObjectAttributes[] visibleAttributes,
                    pieceTransforms: out Transform[] visibleMotions);
                return Fin.Succ(value: (Pieces: visible, Attributes: visibleAttributes, Motions: visibleMotions));
            }
            instance.Explode(
                explodeNestedInstances: active.Nested,
                pieces: out RhinoObject[] pieces,
                pieceAttributes: out ObjectAttributes[] attributes,
                pieceTransforms: out Transform[] motions);
            return Fin.Succ(value: (Pieces: pieces, Attributes: attributes, Motions: motions));
        }));
}
```

## [05]-[RECEIPTS]

`BlockReceipt` is the Document spine's `FactStream<TSlot, TBody>` closed over this folder's `BlockSlot` and `BlockBody`, so the accumulation, the gate, the undo projection, and the slot-keyed reader are the owner's and this page contributes only the two vocabularies plus its own mint and projection surface as extension blocks. Each definition fact retains stable guid and transient table index and mints its optional path fact in the same call; object receipt admission rejects any empty id before distinct projection, and path, tally, signal, and undo facts share the same closed payload family.

- Law: the stream MACHINERY is not this folder's — a folder-local receipt, fact, gate, or projection beside the owner is the deleted form, and the same two declarations are all a third mutation folder needs to join.
- Law: every address column on `BlockBody` takes its spine owner — `ResourceId`, `ResourceIndex`, `DocumentPath`, `UndoSerial` — because each raw primitive's invalid value is precisely what a failed host member answers with, and a receipt publishing one is indistinguishable from a real consequence. The address admissions stay on this page, because which host members lie about failure is this folder's evidence law, not the stream's.
- Law: the undo stamp is a projection on the stream, not a rail — `DocumentCommit.Sealed` stamps every sealed receipt, an unrecorded program's serial is zero, `UndoSerial` refuses zero, so no fact claims record zero and the total `(receipt, serial) -> receipt` shape holds.

The stream factory is the one fact ingress and the cross-product gate: `BlockSlot` carries an `Admits` predicate row naming exactly the body kinds its slot emits, so a tally landed on an authoring slot or a path on a tally slot refuses at construction with the slot named, and a new slot cannot compile without declaring its bodies.

Slot projections derive from `Facts`; no consumer re-queries a mutation merely to reconstruct its consequences.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// This folder's whole contribution to the shared stream: a keyed slot vocabulary and a body union. The
// accumulation, the gate, the undo projection, and the slot-keyed reader live once on the Document spine's
// `FactStream<TSlot, TBody>`, which this page closes as `BlockReceipt`.
[SmartEnum<int>]
public sealed partial class BlockSlot : IFactSlot<BlockBody> {
    public static readonly BlockSlot Authored = new(key: 0, admits: Named);
    public static readonly BlockSlot Reused = new(key: 18, admits: Named);
    public static readonly BlockSlot Amended = new(key: 1, admits: Named);
    public static readonly BlockSlot Regeometried = new(key: 2, admits: Named);
    public static readonly BlockSlot Rebound = new(key: 3, admits: Sourced);
    public static readonly BlockSlot Severed = new(key: 4, admits: Named);
    public static readonly BlockSlot Refreshed = new(key: 5, admits: Named);
    public static readonly BlockSlot Retargeted = new(key: 6, admits: Sourced);
    public static readonly BlockSlot Styled = new(key: 7, admits: Named);
    public static readonly BlockSlot Deleted = new(key: 8, admits: Named);
    public static readonly BlockSlot Revived = new(key: 9, admits: Named);
    public static readonly BlockSlot Purged = new(key: 10, admits: Named);
    public static readonly BlockSlot Reclaimed = new(key: 11, admits: Counted);
    public static readonly BlockSlot Compacted = new(key: 12, admits: Marked);
    public static readonly BlockSlot Exported = new(key: 13, admits: Sourced);
    public static readonly BlockSlot Placed = new(key: 14, admits: Instanced);
    public static readonly BlockSlot Repointed = new(key: 15, admits: Instanced);
    public static readonly BlockSlot Baked = new(key: 16, admits: Harvested);
    public static readonly BlockSlot Undo = new(key: 17, admits: Stamped);

    // The cross product a receipt may express: one predicate row per slot, so a new slot cannot compile
    // without declaring which body kinds it emits and a mismatched pairing refuses at the stream factory.
    [UseDelegateFromConstructor]
    public partial bool Admits(BlockBody body);

    private static bool Named(BlockBody body) => body is BlockBody.Definition;
    private static bool Sourced(BlockBody body) => body is BlockBody.Definition or BlockBody.Path;
    private static bool Counted(BlockBody body) => body is BlockBody.Tally;
    private static bool Marked(BlockBody body) => body is BlockBody.Signal;
    private static bool Instanced(BlockBody body) => body is BlockBody.Object;
    private static bool Harvested(BlockBody body) => body is BlockBody.Object or BlockBody.Tally;
    private static bool Stamped(BlockBody body) => body is BlockBody.Record;
}

// Every address column takes its spine owner. The path column already did; the other four carried raw primitives
// whose invalid values are exactly the ones a host failure answers with — an empty definition guid, a `-1` table
// index, an empty object id, a zero undo serial — so a receipt could publish a "created" fact naming nothing and
// no reader could tell it from a real one.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockBody {
    private BlockBody() { }
    public sealed record Definition(ResourceId Key, ResourceIndex Index) : BlockBody;
    public sealed record Object(ResourceId Id) : BlockBody;
    public sealed record Tally(int Count) : BlockBody;
    public sealed record Path(DocumentPath Value) : BlockBody;
    public sealed record Record(UndoSerial Serial) : BlockBody;
    public sealed record Signal : BlockBody;
}

// --- [EXPORTS] -------------------------------------------------------------------------------
// The folder's receipt IS the spine's stream closed over this folder's two vocabularies; the aliases carry the
// domain names call sites already read, and the project-level alias rows publish them namespace-wide, so no
// consumer spells the instantiation and no folder-local receipt type exists to drift from the owner.
global using BlockFact = Rasm.Rhino.Document.Fact<Rasm.Rhino.Blocks.BlockSlot, Rasm.Rhino.Blocks.BlockBody>;
global using BlockReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Blocks.BlockSlot, Rasm.Rhino.Blocks.BlockBody>;

// --- [OPERATIONS] ----------------------------------------------------------------------------
// The folder's mint surface rides an extension block over the closed instantiation, so every `BlockReceipt.*`
// call site reads as it did while the accumulation and the gate stay on the one owner. The address admissions
// stay HERE, because they are this folder's evidence law: a host member reporting failure as an empty guid or a
// `-1` index must refuse before its value reaches a body.
public static class BlockFacts {
    extension(BlockReceipt) {
        public static Fin<BlockReceipt> Definition(
            BlockSlot slot,
            InstanceDefinition definition,
            Op key,
            Option<DocumentPath> path = default) =>
            from admitted in Optional(definition).ToFin(Fail: key.InvalidResult())
            from _ in guard(admitted.Id != Guid.Empty && admitted.Index >= 0, key.InvalidResult()).ToFin()
            from identity in ResourceId.Admit(value: admitted.Id, key: key)
            from index in ResourceIndex.Admit(value: admitted.Index, key: key)
            from named in BlockReceipt.Of(
                slot: slot,
                body: new BlockBody.Definition(Key: identity, Index: index),
                key: key)
            from sourced in path
                .Traverse(value => BlockReceipt.Of(slot: slot, body: new BlockBody.Path(Value: value), key: key))
                .As()
            select sourced.IfNone(BlockReceipt.Empty) + named;

        public static Fin<BlockReceipt> Objects(BlockSlot slot, Seq<Guid> ids, Op key) =>
            from _ in guard(!ids.IsEmpty, key.InvalidResult()).ToFin()
            from admitted in ids
                .Traverse(id => guard(id != Guid.Empty, key.InvalidResult()).ToFin().ToValidation())
                .As()
                .ToFin()
            from bodies in admitted.Distinct()
                .TraverseM(id => ResourceId.Admit(value: id, key: key)
                    .Map(value => (BlockBody)new BlockBody.Object(Id: value)))
                .As()
            from receipt in BlockReceipt.All(slot: slot, bodies: bodies, key: key)
            select receipt;

        public static Fin<BlockReceipt> Tally(BlockSlot slot, int count, Op key) =>
            from _ in guard(count >= 0, key.InvalidResult()).ToFin()
            from receipt in BlockReceipt.Of(slot: slot, body: new BlockBody.Tally(Count: count), key: key)
            select receipt;

        public static Fin<BlockReceipt> Signal(BlockSlot slot, Op key) =>
            BlockReceipt.Of(slot: slot, body: new BlockBody.Signal(), key: key);
    }

    extension(BlockReceipt receipt) {
        public Seq<(ResourceId Key, ResourceIndex Index)> DefinitionRefs(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Definition value
                ? Some((value.Key, value.Index))
                : Option<(ResourceId, ResourceIndex)>.None);

        public Seq<ResourceIndex> Definitions(BlockSlot slot) =>
            receipt.DefinitionRefs(slot: slot).Map(static definition => definition.Index);

        public Seq<ResourceId> Ids(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Object value
                ? Some(value.Id)
                : Option<ResourceId>.None);

        public Seq<int> Tallies(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Tally value
                ? Some(value.Count)
                : Option<int>.None);

        public Seq<DocumentPath> Paths(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Path value
                ? Some(value.Value)
                : Option<DocumentPath>.None);
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]                 | [INGRESS]               | [RAIL]                               | [EGRESS]          |
| :-----: | :---------------------- | :---------------------- | :----------------------------------- | :---------------- |
|  [01]   | `BlockOp`               | generated values        | `Apply`                              | receipt fragment  |
|  [02]   | `BlockTransaction`      | `Batch`                 | trait homogeneity                    | admitted program  |
|  [03]   | `Blocks`                | `Commit` · `Ask`        | `Fin` · `Validation` · `UndoBracket` | receipt or answer |
|  [04]   | `BlockSlot`/`BlockBody` | `BlockFacts` extensions | spine `FactStream` accumulation      | slot projections  |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
