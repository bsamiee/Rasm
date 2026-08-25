# [RASM_RHINO_BLOCK_OPERATIONS]

Block operations (`Rasm.Rhino.Blocks`) own one closed mutation family, one closed read family, one admitted transaction, and the folder's two fact-stream vocabularies. `Blocks.Commit` derives session needs from operation demands, acquires geometry through `GeometryIntake`, frames the change through `DocumentCommit.Sealed`, and emits stable definition and object evidence. Every consequence this folder publishes — a mutation, a bake, a preview sweep, a refresh degrade — lands as one `BlockReceipt` fact, so a reader counts per slot on one owner and no second receipt shape exists beside it.

## [01]-[INDEX]

- [02]-[OPERATION_FAMILY]: `BlockOp` closing every definition and instance mutation, `BlockTrait` naming each case's commit demands, and the leased member admission every authoring arm runs.
- [03]-[READ_FAMILY]: `BlockAsk`/`BlockAnswer` closing the read questions, `FieldSource` dispatching the text-field grammar, and `ExplodedPiece` owning detached custody.
- [04]-[COMMIT_SPINE]: `BlockTransaction` admitting one homogeneous program and `Blocks` walking the shared commit entry.
- [05]-[RECEIPTS]: `BlockSlot`/`BlockBody`/`BlockBodyKind` closing the shared stream over this folder's vocabularies, and `BlockReceipts` minting and projecting it.
- [06]-[SURFACE_LEDGER]: owner-to-ingress-to-rail-to-egress roster.

## [02]-[OPERATION_FAMILY]

- Owner: `BlockOp` `[Union]` carries every verified definition mutation and block-specific instance operation; `BlockTrait` `[SmartEnum<int>]` names each case's commit demands as ONE `CapabilitySet<CommitDemand>` column; `BlockMetadata`, `BlockHyperlink`, and `BlockMember` are the admitted payload owners; `SourceReference` closes the linked-source address; `LinkMode`, `LinkTraversal`, `CompactPolicy`, and `InstanceDisposition` carry host arguments as rows.
- Entry: each payload owner's `Of` is the ONE admission — the generated `Create` throws and no arm reaches it; `BlockOp.Apply` is the single total dispatch, one host member per case, one receipt fragment per case.
- Law: a case's commit demands are a SET on its trait row. `Undo` and `KernelContext` were two bool columns whose fourth corner — a context-demanding case that opens no undo record — the three rows never inhabited and nothing forbade; the set states each row's demands, `BlockTransaction` reads the undo axis for homogeneity, and `Run` reads the context axis for the domain census. The trait table is TOTAL with no catch-all: a catch-all read as "everything else records undo", so a new unrecorded case — the shape `Purge`, `Compact`, and `Export` already are — would silently join the recorded lane and enter a sealed program it cannot roll back.
- Law: every address column takes its spine owner. `BlockMetadata` takes `ResourceName`, every file address takes `DocumentPath`, and the baked instance takes `ResourceId`, because the empty guid is exactly what a failed host lookup answers with — the guard each read site re-spelled is unrepresentable once the request carries the owner.
- Law: the interior never re-validates. A union case column, a `[SmartEnum]` row, and an admitted value object each proved at construction, so `Need` survives only where a HOST read can answer null — the `as InstanceObject` cast and the caller-supplied entries — and the null guards the placement arms carried delete with `Placement`'s own admission factories.
- Law: `BlockMember` pairs one already-admitted `GeometryIntake` with one attribute set, acquisition retains that bijection, and release brackets ACQUISITION rather than outcome — every lease closes on both exits through the folder's one release fold, and a disposer refusal APPENDS to the primary instead of vanishing inside a `finally`.
- Law: the source axis crosses through `SourceMode`, never a raw `InstanceDefinitionUpdateType` comparison — the mode's `Facets` set gates the linked-source verbs and the retired ordinal folds onto `Static` at admission, so no fence spells the `[Obsolete]` host case.
- Law: `Bake` compares produced object ids with the source definition roster — shallow expansion requires equality, recursive expansion at least the direct roster — and `AddExplodedInstancePieces` answers null on a no-op despite its non-nullable signature, so a zero-member definition admits that sentinel as an empty roster and any other null is a refusal.
- Boundary: `HostInteraction` is the Document spine's dialogue axis and reaches the host as a `quiet:` argument; `FileReference : IDisposable`, so every linked-source mint rides an owned lease that releases the moment the host call returns.
- Growth: a new mutation is one `BlockOp` case, one trait row read, and one `Apply` arm — the generated dispatch breaks loudly until all three land.
- Packages: RhinoCommon blocks (`.api/api-rhinocommon-blocks.md` — `InstanceDefinitionTable` authoring, linked-source, lifecycle, and instance members; `FileReference`), `Rasm.Rhino.Document` (`DocumentCommit`, `HostInteraction`, `ResourceRef`/`ResourceId`/`ResourceIndex`/`ResourceName`, `DocumentPath`, `GeometryIntake`), kernel `Domain/rails` (`Op`, `Lease<T>`, `Fault`, `Custody`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class CommitDemand : ICapability<CommitDemand> {
    public static readonly CommitDemand Undo = new(key: "undo");
    public static readonly CommitDemand KernelContext = new(key: "kernel-context");
}

[SmartEnum<int>]
internal sealed partial class BlockTrait {
    public static readonly BlockTrait Mutation = new(key: 0, demands: CapabilitySet<CommitDemand>.Of(CommitDemand.Undo));
    public static readonly BlockTrait Contextual = new(
        key: 1, demands: CapabilitySet<CommitDemand>.Of(CommitDemand.Undo, CommitDemand.KernelContext));
    public static readonly BlockTrait Unrecorded = new(key: 2, demands: CapabilitySet<CommitDemand>.Of());

    public CapabilitySet<CommitDemand> Demands { get; }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class BlockHyperlink {
    public string Url { get; }
    public string Tag { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string url, ref string tag) {
        url = url?.Trim() ?? string.Empty;
        tag = tag?.Trim() ?? string.Empty;
        validationError = url.Length > 0
            && Uri.TryCreate(uriString: url, uriKind: UriKind.RelativeOrAbsolute, result: out _)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(BlockHyperlink), "a parseable url beside its tag", Option<Op>.None }));
    }

    public static Fin<BlockHyperlink> Of(string url, string tag, Op? key = null) =>
        key.OrDefault().AcceptValidated<BlockHyperlink>(
            fault: Validate(url, tag, out BlockHyperlink? admitted),
            admitted: admitted);

    internal static (string Url, string Tag) Host(Option<BlockHyperlink> value) => value.Match(
        Some: static held => (held.Url, held.Tag),
        None: static () => (string.Empty, string.Empty));
}

[ComplexValueObject]
[ValidationError]
public sealed partial class BlockMetadata {
    public ResourceName Name { get; }
    public string Description { get; }
    public Option<BlockHyperlink> Hyperlink { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ResourceName name,
        ref string description,
        ref Option<BlockHyperlink> hyperlink) {
        description = description ?? string.Empty;
        validationError = name is not null
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(BlockMetadata), "an admitted definition name", Option<Op>.None }));
    }

    public static Fin<BlockMetadata> Of(
        ResourceName name, string description, Option<BlockHyperlink> hyperlink = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<BlockMetadata>(
            fault: Validate(name, description, hyperlink, out BlockMetadata? admitted),
            admitted: admitted);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SourceReference {
    private SourceReference() { }
    public sealed record Absolute(DocumentPath Full) : SourceReference;

    public sealed record Anchored(DocumentPath Full, string Relative) : SourceReference;

    public static Fin<SourceReference> Of(string full, Option<string> relative, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in DocumentPath.Of(value: full, key: op)
               from anchor in relative.Traverse(value => op.AcceptText(value: value)).As()
               select anchor.Match(
                   Some: text => (SourceReference)new Anchored(Full: admitted, Relative: text),
                   None: () => new Absolute(Full: admitted));
    }

    internal DocumentPath Full => Switch(
        absolute: static row => row.Full,
        anchored: static row => row.Full);

    internal Fin<T> Use<T>(Func<FileReference, Fin<T>> body, Op op) =>
        op.Catch(() => Switch(
            context: (Body: body, Op: op),
            absolute: static (use, row) => new Lease<FileReference>.Owned(
                Value: FileReference.CreateFromFullPath(fullPath: row.Full.Value)).Use(body: use.Body, key: use.Op),
            anchored: static (use, row) => new Lease<FileReference>.Owned(
                Value: FileReference.CreateFromFullAndRelativePaths(
                    fullPath: row.Full.Value, relativePath: row.Relative)).Use(body: use.Body, key: use.Op)));
}

[ComplexValueObject]
[ValidationError]
public sealed partial class BlockMember {
    public GeometryIntake Source { get; }
    public ObjectAttributes Attributes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryIntake source,
        ref ObjectAttributes attributes) =>
        validationError = source is not null && attributes is not null
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(BlockMember), "an admitted geometry intake beside its attribute set", Option<Op>.None }));

    public static Fin<BlockMember> Of(GeometryIntake source, ObjectAttributes attributes, Op? key = null) =>
        key.OrDefault().AcceptValidated<BlockMember>(
            fault: Validate(source, attributes, out BlockMember? admitted),
            admitted: admitted);
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
    public sealed record Bake(ResourceId Instance, ExplodeDepth Depth, InstanceDisposition Disposition) : BlockOp;

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

    // --- [OPERATIONS]
    internal Fin<BlockReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            context: (Document: document, Domain: domain, Op: op),
            author: static (context, edit) =>
                from _ in guard(edit.BasePoint.IsValid, context.Op.InvalidInput()).ToFin()
                from receipt in Optional(context.Document.InstanceDefinitions.Find(edit.Metadata.Name.Value)).Match(
                    None: () => Authored(name: edit.Metadata.Name, edit: edit, context: context),
                    Some: existing => edit.Conflict.Switch(
                        (Existing: existing, Edit: edit, Held: context),
                        fail: static held => Fin.Fail<BlockReceipt>(error: held.Held.Op.InvalidInput()),
                        reuse: static held => BlockReceipt.Definition(
                            slot: BlockSlot.Reused, definition: held.Existing, key: held.Held.Op),
                        mint: static held => held.Held.Op.AcceptText(value: held.Held.Document.InstanceDefinitions
                                .GetUnusedInstanceDefinitionName(root: held.Edit.Metadata.Name.Value))
                            .Bind(minted => Authored(
                                name: ResourceName.Create(minted), edit: held.Edit, context: held.Held))))
                select receipt,
            amend: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                let hyperlink = BlockHyperlink.Host(value: edit.Metadata.Hyperlink)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Modify(
                    idefIndex: definition.Index, newName: edit.Metadata.Name.Value,
                    newDescription: edit.Metadata.Description,
                    newUrl: hyperlink.Url, newUrlTag: hyperlink.Tag, quiet: edit.Interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Amended, definition: definition, key: context.Op)
                select receipt,
            regeometry: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(!mode.Facets.Admits(capability: SourceFacet.Reads), context.Op.InvalidInput()).ToFin()
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
                from _ in edit.Source.Use(
                    body: reference => context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifySourceArchive(
                        idefIndex: definition.Index, sourceArchive: reference,
                        updateType: edit.Mode.UpdateType, quiet: edit.Interaction.IsQuiet)),
                    op: context.Op)
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Rebound,
                    definition: definition,
                    key: context.Op,
                    path: Some(edit.Source.Full))
                select receipt,
            sever: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.DestroySourceArchive(
                    definition: definition, quiet: edit.Interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Severed, definition: definition, key: context.Op)
                select receipt,
            refresh: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(
                    !definition.IsTenuous && mode.Facets.Admits(capability: SourceFacet.Reads),
                    context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Refreshed, definition: definition, key: context.Op)
                select receipt,
            retarget: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: definition.Index, filename: edit.Filename.Value,
                    updateNestedLinks: edit.Traversal.NestedLinks, quiet: edit.Interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Retargeted,
                    definition: definition,
                    key: context.Op,
                    path: Some(edit.Filename))
                select receipt,
            style: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(mode.Facets.Admits(capability: SourceFacet.Reads), context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    definition.LayerStyle = edit.LayerStyle.Host;
                    return context.Op.Confirm(success: definition.LayerStyle == edit.LayerStyle.Host);
                })
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Styled, definition: definition, key: context.Op)
                select receipt,
            delete: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Delete(
                    idefIndex: definition.Index,
                    deleteReferences: edit.Policy.DeleteReferences,
                    quiet: edit.Policy.Interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Deleted, definition: definition, key: context.Op)
                select receipt,
            undelete: static (context, edit) =>
                from definition in edit.Target.Resolve(
                    document: context.Document, lens: Definitions.DeletedLens, key: context.Op)
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
                from _ in context.Op.Catch(() => Fin.Succ(value: Op.Side(() =>
                    context.Document.InstanceDefinitions.Compact(ignoreUndoReferences: edit.Policy.IgnoreUndoReferences))))
                from receipt in BlockReceipt.Signal(slot: BlockSlot.Compacted, key: context.Op)
                select receipt,
            export: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Export(
                    idefIndex: definition.Index, filename: edit.Path.Value))
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Exported,
                    definition: definition,
                    key: context.Op,
                    path: Some(edit.Path))
                select receipt,
            place: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(!edit.Instances.IsEmpty, context.Op.InvalidInput()).ToFin()
                from placed in edit.Instances.TraverseM(placement => placement.Switch(
                    state: (Document: context.Document, Index: definition.Index, Op: context.Op),
                    bare: static (ctx, request) => Place(motion: request.Motion, op: ctx.Op,
                        add: () => ctx.Document.Objects.AddInstanceObject(
                            instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion)),
                    attributed: static (ctx, request) => Place(motion: request.Motion, op: ctx.Op,
                        add: () => ctx.Document.Objects.AddInstanceObject(
                            instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion,
                            attributes: request.Attributes)),
                    recorded: static (ctx, request) => request.History.Use(
                        body: record => Place(motion: request.Motion, op: ctx.Op,
                            add: () => ctx.Document.Objects.AddInstanceObject(
                                instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion,
                                attributes: request.Attributes,
                                history: record, reference: request.Kind.IsReference)),
                        key: ctx.Op))).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Placed, ids: placed, key: context.Op)
                select receipt,
            repoint: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from ids in edit.Instances.Resolve(document: context.Document, key: context.Op)
                from repointed in ids.TraverseM(id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(
                    objectId: id, instanceDefinitionIndex: definition.Index)).Map(_ => id)).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Repointed, ids: repointed, key: context.Op)
                select receipt,
            bake: static (context, edit) =>
                from native in Optional(context.Document.Objects.FindId(edit.Instance.Value))
                    .ToFin(Fail: context.Op.MissingContext())
                from instance in context.Op.Need(native as InstanceObject)
                from expected in context.Op.Catch(() => Optional(instance.InstanceDefinition)
                    .ToFin(Fail: context.Op.InvalidResult())
                    .Map(static definition => definition.ObjectCount))
                from ids in context.Op.Catch(() => Optional(context.Document.Objects.AddExplodedInstancePieces(
                        instance: instance,
                        explodeNestedInstances: edit.Depth.Nested,
                        deleteInstance: edit.Disposition.DeleteInstance))
                    .Map(toSeq)
                    .Match(
                        Some: static pieces => Fin.Succ(value: pieces),
                        None: () => expected == 0
                            ? Fin.Succ(value: Seq<Guid>())
                            : Fin.Fail<Seq<Guid>>(error: context.Op.InvalidResult())))
                from __ in guard(
                    expected >= 0 && (edit.Depth.Nested ? ids.Count >= expected : ids.Count == expected),
                    context.Op.InvalidResult()).ToFin()
                from objects in ids.IsEmpty
                    ? Fin.Succ(value: BlockReceipt.Empty)
                    : BlockReceipt.Objects(slot: BlockSlot.Baked, ids: ids, key: context.Op)
                from tally in BlockReceipt.Tally(slot: BlockSlot.Baked, count: ids.Count, key: context.Op)
                select objects + tally);

    private static Fin<BlockReceipt> Authored(
        ResourceName name,
        Author edit,
        (RhinoDoc Document, Option<Context> Domain, Op Op) context) =>
        Admitted(
            members: edit.Members,
            domain: context.Domain,
            op: context.Op,
            run: (geometry, attributes) =>
                from index in context.Op.Catch(() => ResourceIndex.Admit(
                    value: edit.Metadata.Hyperlink.Match(
                        Some: held => context.Document.InstanceDefinitions.Add(
                            name: name.Value,
                            description: edit.Metadata.Description,
                            url: held.Url,
                            urlTag: held.Tag,
                            basePoint: edit.BasePoint,
                            geometry: geometry,
                            attributes: attributes),
                        None: () => context.Document.InstanceDefinitions.Add(
                            name: name.Value,
                            description: edit.Metadata.Description,
                            basePoint: edit.BasePoint,
                            geometry: geometry,
                            attributes: attributes)),
                    key: context.Op))
                from created in Receipt(
                    document: context.Document,
                    index: index,
                    slot: BlockSlot.Authored,
                    op: context.Op)
                select created);

    private static Fin<BlockReceipt> Receipt(RhinoDoc document, ResourceIndex index, BlockSlot slot, Op op) =>
        from definition in Optional(document.InstanceDefinitions[index.Value]).ToFin(Fail: op.InvalidResult())
        from receipt in BlockReceipt.Definition(slot: slot, definition: definition, key: op)
        select receipt;

    private static Fin<Guid> Place(Transform motion, Op op, Func<Guid> add) =>
        from _ in guard(motion.IsValid, op.InvalidInput()).ToFin()
        from id in op.Catch(() => Optional(add())
            .Filter(static value => value != Guid.Empty)
            .ToFin(Fail: op.InvalidResult()))
        select id;

    private static Fin<BlockReceipt> Admitted(
        Seq<BlockMember> members,
        Option<Context> domain,
        Op op,
        Func<IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>, Fin<BlockReceipt>> run) =>
        members.IsEmpty
            ? op.Catch(() => run(Array.Empty<GeometryBase>(), Array.Empty<ObjectAttributes>()))
            : from active in domain.ToFin(Fail: op.MissingContext())
              from admitted in Leased(members: members, domain: active, op: op)
              from receipt in op.Catch(() => run(
                      admitted.Map(static member => member.Geometry.Resource).AsIterable(),
                      admitted.Map(static member => member.Attributes).AsIterable()))
                  .Settled(
                      held: admitted,
                      release: static member => Fin.Succ(value: member.Geometry.Dispose()),
                      key: op)
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
            rollback: landed => Custody.Release(
                held: landed,
                release: static prior => Fin.Succ(value: prior.Geometry.Dispose()),
                key: op));
}
```

## [03]-[READ_FAMILY]

- Owner: `BlockAsk` `[Union]` closes state, dependency, preview, field extraction, name minting, and instance explosion; `BlockAnswer` `[Union]` carries the seven detached answers; `FieldSource` `[Union]` co-locates every text-field dispatch with its case family; `BlockField` is the detached descriptor and `FieldMap` its generated projection; `ExplodedPiece` owns detached geometry and attribute custody.
- Entry: `FieldSource.Read` answers two `BlockAnswer` cases from one `BlockAsk.Fields`, which is why the request and answer arities are genuinely six to seven; `ExplodedPiece` is minted only by the explode fold, never by a caller.
- Law: the host descriptor projection is GENERATED — three get-only host columns onto three record columns is a copy `[Mapper]` owns, and a hand member-by-member body beside the generator is the deleted form.
- Law: array cardinality is proven ONCE and carried. The native explode answers three parallel arrays; one zip proves their lengths agree and hands the fold a row of admitted triples, so no step re-indexes three arrays and no step re-proves the count.
- Law: custody direction is the PARENT's — an exploded piece's `Geometry` is a non-owning const wrapper parented to its `RhinoObject`, so the release disposes the parent and the wrapper falls with it; disposing the child while leaking the parent inverts ownership and double-frees at parent teardown.
- Law: the attribute set stays the CAPTURE's until the whole fold lands. A rolled-back piece releases only the detached geometry it minted, and the final sweep releases every captured parent plus the attributes no surviving product took, so each attribute set disposes exactly once whichever arm runs. `ProductCustody` is that decision as a ROW — the bool the release took was this discriminant left unnamed.
- Law: failure-release custody is the rail's, not a call site's — each of the three release points is one `Rollback` or `Settled`, where three hand `MapFail` blocks folding `primary + cleanup` stood.
- Boundary: `Lease<System.Drawing.Bitmap>` is a GDI carrier the preview lifecycle owns, and `Rhino.DocObjects.TextFields` is the host's own field grammar — both stay host-side; the kernel's raster owners carry no GDI handle.
- Exemption: `Capture` is the statement-shaped native out-parameter boundary — two `Explode` overloads whose only outputs are three `out` arrays — and both collapse immediately onto the same tuple rail.
- Packages: RhinoCommon blocks (`.api/api-rhinocommon-blocks.md` — `InstanceObject.Explode` overloads, `TextFields.GetInstanceAttributeFields`, `TextFields.BlockAttributeText`), `Rasm.Rhino.Document` (`GeometryCrossing`/`GeometryHandle`, `DocumentCommit`, `TableTarget`, `ResourceRef`), kernel `Domain/rails` (`Custody`), Riok.Mapperly (`[Mapper]`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
                from ids in source.Target.Resolve(document: ctx.Document, key: ctx.Op)
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
                from fallback in ctx.Op.AcceptText(value: source.DefaultValue)
                from token in ctx.Op.Catch(() => ctx.Op.AcceptText(value: TextFields.BlockAttributeText(
                    key: key, prompt: prompt, defaultValue: fallback)))
                select (BlockAnswer)new BlockAnswer.Token(Value: token));

    private static Arr<BlockField> Described(IEnumerable<TextFields.InstanceAttributeField> descriptors) =>
        toArr(descriptors).Map(FieldMap.From);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAsk {
    private BlockAsk() { }
    public sealed record State(ResourceRef Target, ReferenceScope Scope) : BlockAsk;
    public sealed record Dependency(ResourceRef Target, BlockDependency Probe) : BlockAsk;
    public sealed record Preview(ResourceRef Target, BlockPreview Spec) : BlockAsk;
    public sealed record Fields(FieldSource Source) : BlockAsk;
    public sealed record MintName(Option<ResourceName> Root) : BlockAsk;
    public sealed record Pieces(ResourceId Instance, ExplodePolicy Policy) : BlockAsk;
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

    public Fin<Unit> Release(Op key) => Switch(
        context: key,
        state: static (_, _) => Fin.Succ(unit),
        dependency: static (_, _) => Fin.Succ(unit),
        rendered: static (op, answer) => Custody.Dispose(held: Seq(answer.Preview), key: op),
        fields: static (_, _) => Fin.Succ(unit),
        token: static (_, _) => Fin.Succ(unit),
        minted: static (_, _) => Fin.Succ(unit),
        pieces: static (op, answer) => Custody.Release(
            held: answer.Products, release: piece => piece.Release(op), key: op));
}

[SmartEnum<int>]
internal sealed partial class ProductCustody {
    internal static readonly ProductCustody Retained = new(key: 0);
    internal static readonly ProductCustody Released = new(key: 1);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BlockField(
    string Key,
    string Prompt,
    string DefaultValue) : IDetachedDocumentResult;

public sealed class ExplodedPiece {
    private int disposed;

    internal ExplodedPiece(GeometryHandle geometry, ObjectAttributes attributes, Transform motion) =>
        (Geometry, Attributes, Motion) = (geometry, attributes, motion);

    public GeometryHandle Geometry { get; }
    public ObjectAttributes Attributes { get; }
    public Transform Motion { get; }

    public Fin<Unit> Release(Op key) => Interlocked.Exchange(location1: ref disposed, value: 1) is not 0
        ? Fin.Succ(unit)
        : Custody.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => key.Catch(() => Fin.Succ(value: Op.Side(Geometry.Dispose))),
                () => key.Catch(() => Fin.Succ(value: Op.Side(Attributes.Dispose)))),
            key: key);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class FieldMap {
    internal static partial BlockField From(TextFields.InstanceAttributeField descriptor);
}
```

## [04]-[COMMIT_SPINE]

- Owner: `BlockTransaction` `[ComplexValueObject]` is the admitted homogeneous program; `Blocks` is the folder's one mutation and read entry.
- Entry: `BlockTransaction.Batch` admits the name, the redraw posture, and the operation span through the spine's one `Admission.All` fold, then proves the program at the generated factory; `Blocks.Commit` and `Blocks.Ask` are the only public rails and each mints its own key.
- Law: the undo posture is the PROGRAM's, proved uniform at admission and DERIVED thereafter. A stored `recordsUndo` column was a second authority over a fact `Operations` already states, and a mixed recorded/unrecorded program fails at the factory rather than inside a bracket it cannot roll back.
- Law: the homogeneity invariant is STRUCTURAL — it lives in `ValidateFactoryArguments`, so no path constructs a `BlockTransaction` whose operations disagree on the undo axis, and the hand `sealed class` with a private constructor is the deleted form.
- Law: `Blocks.Commit` walks the shared commit entry and nothing else — needs derive through `SessionNeed.Mutation` over the program's `UndoCustody` row, one document demand carries the whole program, the kernel `Context` resolves only where a case demands it, and `DocumentCommit.Sealed` owns bracket, restoration, redraw, and stamp. A hand-spelled `UndoBracket.Begin`, a redraw triple, or an inline need roster beside this envelope is the deleted form.
- Law: the receipt fold is the stream's monoid — every arm's fragment accumulates through `+`, and the sealed serial rides the stamp projection rather than a rail.
- Packages: `Rasm.Rhino.Document` (`DocumentSession`, `SessionNeed`, `UndoCustody`, `RedrawPolicy`, `DocumentCommit`, `Admission`), kernel `Domain/rails` (`Op`, `Context`, `Fault`, `Custody`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class BlockTransaction {
    public string Name { get; }
    public Seq<BlockOp> Operations { get; }
    public RedrawPolicy Redraw { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string name,
        ref Seq<BlockOp> operations,
        ref RedrawPolicy redraw) =>
        validationError = !operations.IsEmpty
            && redraw is not null
            && operations
                .Map(static operation => operation.Traits.Demands.Admits(capability: CommitDemand.Undo))
                .Distinct()
                .Count is 1
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(BlockTransaction), "a nonempty program whose operations share one undo posture", Option<Op>.None }));

    internal UndoCustody Custody =>
        Operations.Head.Exists(static operation => operation.Traits.Demands.Admits(capability: CommitDemand.Undo))
            ? UndoCustody.Recorded
            : UndoCustody.Unrecorded;

    public static Fin<BlockTransaction> Batch(string name, RedrawPolicy redraw, params ReadOnlySpan<BlockOp> operations) {
        Op op = Op.Of();
        return from admitted in op.AcceptText(value: name)
               from policy in op.Need(redraw)
               from program in Admission.All(values: operations, key: op)
               from plan in op.AcceptValidated<BlockTransaction>(
                   fault: Validate(admitted, program, policy, out BlockTransaction? built),
                   admitted: built)
               select plan;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class Blocks {
    public static Fin<BlockReceipt> Commit(DocumentSession session, BlockTransaction transaction) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from plan in op.Need(transaction)
               from receipt in owner.Demand(
                   use: document => Run(document: document, plan: plan, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(custody: plan.Custody, redraw: plan.Redraw).ToArray())
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
                           from snapshot in BlockSnapshot.Of(
                               target: ask.Target,
                               document: ctx.Document,
                               scope: ask.Scope,
                               key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.State(Snapshot: snapshot),
                       dependency: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from measure in ask.Probe.Measure(owner: definition, document: ctx.Document, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Dependency(Measure: measure),
                       preview: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from bitmap in ask.Spec.Render(definition: definition, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Rendered(
                               Preview: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
                       fields: static (ctx, ask) => ask.Source.Read(document: ctx.Document, op: ctx.Op),
                       mintName: static (ctx, ask) =>
                           from minted in ctx.Op.Catch(() => ctx.Op.AcceptText(value: ask.Root.Match(
                               Some: root => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: root.Value),
                               None: () => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName())))
                           select (BlockAnswer)new BlockAnswer.Minted(Name: ResourceName.Create(minted)),
                       pieces: static (ctx, ask) =>
                           from native in Optional(ctx.Document.Objects.FindId(ask.Instance.Value))
                               .ToFin(Fail: ctx.Op.MissingContext())
                           from instance in ctx.Op.Need(native as InstanceObject)
                           from products in Exploded(instance: instance, policy: ask.Policy, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Pieces(Products: products)),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    private static Fin<BlockReceipt> Run(RhinoDoc document, BlockTransaction plan, Op op) =>
        from domain in plan.Operations.Exists(
            static operation => operation.Traits.Demands.Admits(capability: CommitDemand.KernelContext))
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from receipt in DocumentCommit.Sealed(
            document: document,
            name: plan.Name,
            recordsUndo: plan.Custody == UndoCustody.Recorded,
            redraw: plan.Redraw,
            run: () => plan.Operations
                .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op))
                .As()
                .Map(static receipts => receipts.Fold(BlockReceipt.Empty, static (state, value) => state + value)),
            stamp: static (receipt, serial) => receipt.Stamped(
                slot: BlockSlot.Undo,
                record: static stamped => new BlockBody.Record(Serial: stamped),
                serial: serial),
            project: Fin.Succ,
            op: op)
        select receipt;

    private static Fin<Seq<ExplodedPiece>> Exploded(InstanceObject instance, ExplodePolicy policy, Op key) =>
        policy.Switch(
            state: (Instance: instance, Op: key),
            all: static (held, request) => Capture(
                instance: held.Instance, depth: request.Depth, viewport: Option<ResourceId>.None, key: held.Op),
            visible: static (held, request) => Capture(
                instance: held.Instance, depth: request.Depth, viewport: Some(request.Viewport), key: held.Op))
        .Bind(captured =>
            from rows in Paired(captured: captured, key: key).Rollback(
                release: () => Discarded(
                    captured: captured,
                    products: Seq<ExplodedPiece>(),
                    custody: ProductCustody.Released,
                    key: key),
                key: key)
            from products in DocumentCommit.Compensated(
                    source: rows,
                    land: row => Detached(row: row, key: key),
                    rollback: landed => Custody.Release(
                        held: landed,
                        release: static piece => Fin.Succ(value: Op.Side(piece.Geometry.Dispose)),
                        key: key))
                .Rollback(
                    release: () => Discarded(
                        captured: captured,
                        products: Seq<ExplodedPiece>(),
                        custody: ProductCustody.Released,
                        key: key),
                    key: key)
            from _ in Discarded(
                    captured: captured,
                    products: products,
                    custody: ProductCustody.Retained,
                    key: key)
                .Rollback(
                    release: () => Custody.Release(
                        held: products,
                        release: piece => piece.Release(key),
                        key: key),
                    key: key)
            select products);

    private static Fin<Seq<(RhinoObject Piece, ObjectAttributes Attributes, Transform Motion)>> Paired(
        (RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions) captured,
        Op key) =>
        guard(
            captured.Pieces.Length == captured.Attributes.Length
                && captured.Pieces.Length == captured.Motions.Length,
            key.InvalidResult()).ToFin()
            .Map(_ => toSeq(captured.Pieces)
                .Zip(toSeq(captured.Attributes), static (piece, attributes) => (Piece: piece, Attributes: attributes))
                .Zip(toSeq(captured.Motions), static (pair, motion) => (pair.Piece, pair.Attributes, Motion: motion)));

    private static Fin<ExplodedPiece> Detached(
        (RhinoObject Piece, ObjectAttributes Attributes, Transform Motion) row,
        Op key) =>
        from piece in Optional(row.Piece).ToFin(Fail: key.InvalidResult())
        from attributes in Optional(row.Attributes).ToFin(Fail: key.InvalidResult())
        from geometry in Optional(piece.Geometry).ToFin(Fail: key.InvalidResult())
        from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
        select new ExplodedPiece(geometry: handle, attributes: attributes, motion: row.Motion);

    private static Fin<Unit> Discarded(
        (RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions) captured,
        Seq<ExplodedPiece> products,
        ProductCustody custody,
        Op key) {
        HashSet<ObjectAttributes> transferred = products
            .Map(static product => product.Attributes)
            .ToHashSet(ReferenceEqualityComparer.Instance);
        return Custody.Release(
            releases: toSeq(captured.Pieces)
                    .Choose(static piece => Optional(piece))
                    .Map(piece => (Func<Fin<Unit>>)(() => Fin.Succ(value: Op.Side(piece.Dispose))))
                + (custody == ProductCustody.Released
                    ? products.Map(product => (Func<Fin<Unit>>)(() => product.Release(key)))
                    : Seq<Func<Fin<Unit>>>())
                + toSeq(captured.Attributes)
                    .Choose(static attributes => Optional(attributes))
                    .Filter(attributes => !transferred.Contains(item: attributes))
                    .Map(attributes => (Func<Fin<Unit>>)(() => Fin.Succ(value: Op.Side(attributes.Dispose)))),
            key: key);
    }

    private static Fin<(RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions)> Capture(
        InstanceObject instance,
        ExplodeDepth depth,
        Option<ResourceId> viewport,
        Op key) =>
        key.Catch(() => {
            if (viewport.Case is ResourceId scoped) {
                instance.Explode(
                    skipHiddenPieces: true, viewportId: scoped.Value, explodeNestedInstances: depth.Nested,
                    pieces: out RhinoObject[] visible,
                    pieceAttributes: out ObjectAttributes[] visibleAttributes,
                    pieceTransforms: out Transform[] visibleMotions);
                return Fin.Succ(value: (Pieces: visible, Attributes: visibleAttributes, Motions: visibleMotions));
            }
            instance.Explode(
                explodeNestedInstances: depth.Nested,
                pieces: out RhinoObject[] pieces,
                pieceAttributes: out ObjectAttributes[] attributes,
                pieceTransforms: out Transform[] motions);
            return Fin.Succ(value: (Pieces: pieces, Attributes: attributes, Motions: motions));
        });
}
```

## [05]-[RECEIPTS]

- Owner: `BlockBodyKind` is the body-kind capability vocabulary; `BlockSlot` `[SmartEnum<int>] : IFactSlot<BlockBody, BlockBodyKind>` is the consequence vocabulary declaring its emitted kinds as ONE set column; `BlockBody` `[Union] : IFactBody<BlockBodyKind>` is the payload family answering its own kind; `BlockReceipt` and `BlockFact` are the closed instantiation of the spine's stream; `BlockReceipts` is the folder's mint and projection surface.
- Entry: the four static mints — `Definition`, `Objects`, `Tally`, `Signal` — are the fact ingress for the mutation family, and `Refresh` is the preview lifecycle's; every projection reads by body kind off `Project`.
- Law: the stream MACHINERY is not this folder's. The accumulation, the cross-product gate, the undo projection, and the slot-keyed readers live once on `Document/facts.md`; a folder-local receipt, fact, gate, or projection beside that owner is the deleted form, and the same two declarations are all a third mutation folder needs to join.
- Law: admission is a READABLE SET, not an opaque predicate. Each slot declares the body kinds it emits as `CapabilitySet<BlockBodyKind>` and the kinded contract derives `Admits`, so a census, a receipt printer, or a reader enumerates the cross product off the rows — the seven `Func<BlockBody, bool>` type tests it replaces could answer "may this body land here" but never "which bodies does this slot emit". The body answers its own kind through one total generated fold, so no slot re-derives a kind by type test and a new case breaks the fold loudly.
- Law: row order IS key order. `Reused` was declared second and keyed last, so the roster read one way and the refusal detail printed another. NAMED LOSS: the prior key assignment; the keys are process-local — the stream is a detached in-memory result and no archive, wire, or setting stores a slot key — so the re-key costs nothing and the declaration order is now the only order.
- Law: every address column on `BlockBody` takes its spine owner — `ResourceId`, `ResourceIndex`, `DocumentPath`, `UndoSerial` — because each raw primitive's invalid value is precisely what a failed host member answers with, and a receipt publishing one is indistinguishable from a real consequence. The address admissions stay HERE, because which host members lie about failure is this folder's evidence law, not the stream's.
- Law: the undo stamp is a projection on the stream, not a rail — `DocumentCommit.Sealed` stamps every sealed receipt, an unrecorded program's serial is zero, `UndoSerial` refuses zero, so no fact claims record zero and the total `(receipt, serial) -> receipt` shape holds.
- Law: preview lifecycle consequences are BLOCK facts. A sweep's freed, retired, and re-rendered rows land on disjoint slots and a degrade rides its own slot carrying the typed cause, so `Blocks/lifecycle.md` counts per slot through `FactCount` and no folder-local refresh receipt stands beside the stream owner.
- Law: the projection family is TOTAL over the payload-bearing body kinds — one reader per kind, `Signal` reading through the owner's `FactCount` — so a missing arm is the defect and a one-hop projection of another projection is not an arm at all.
- Growth: a new consequence is one slot row naming its kind set; a new payload is one body case, one kind row, and one projection arm.
- Packages: `Document/facts.md` (`IFactSlot<TBody, TKind>`, `IFactBody<TKind>`, `Fact`, `FactStream`, `UndoSerial`), `Document/tables.md` (`ResourceId`, `ResourceIndex`, `DocumentPath`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BlockBodyKind : ICapability<BlockBodyKind> {
    public static readonly BlockBodyKind Definition = new(key: "definition");
    public static readonly BlockBodyKind Object = new(key: "object");
    public static readonly BlockBodyKind Tally = new(key: "tally");
    public static readonly BlockBodyKind Path = new(key: "path");
    public static readonly BlockBodyKind Record = new(key: "record");
    public static readonly BlockBodyKind Signal = new(key: "signal");
    public static readonly BlockBodyKind Degrade = new(key: "degrade");
}

[SmartEnum<int>]
public sealed partial class BlockSlot : IFactSlot<BlockBody, BlockBodyKind> {
    private static readonly CapabilitySet<BlockBodyKind> Named = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Definition);
    private static readonly CapabilitySet<BlockBodyKind> Sourced = CapabilitySet<BlockBodyKind>.Of(
        BlockBodyKind.Definition, BlockBodyKind.Path);
    private static readonly CapabilitySet<BlockBodyKind> Counted = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Tally);
    private static readonly CapabilitySet<BlockBodyKind> Marked = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Signal);
    private static readonly CapabilitySet<BlockBodyKind> Instanced = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Object);
    private static readonly CapabilitySet<BlockBodyKind> Harvested = CapabilitySet<BlockBodyKind>.Of(
        BlockBodyKind.Object, BlockBodyKind.Tally);
    private static readonly CapabilitySet<BlockBodyKind> Faded = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Degrade);
    private static readonly CapabilitySet<BlockBodyKind> Stamped = CapabilitySet<BlockBodyKind>.Of(BlockBodyKind.Record);

    public static readonly BlockSlot Authored = new(key: 0, bodies: Named);
    public static readonly BlockSlot Reused = new(key: 1, bodies: Named);
    public static readonly BlockSlot Amended = new(key: 2, bodies: Named);
    public static readonly BlockSlot Regeometried = new(key: 3, bodies: Named);
    public static readonly BlockSlot Rebound = new(key: 4, bodies: Sourced);
    public static readonly BlockSlot Severed = new(key: 5, bodies: Named);
    public static readonly BlockSlot Refreshed = new(key: 6, bodies: Named);
    public static readonly BlockSlot Retargeted = new(key: 7, bodies: Sourced);
    public static readonly BlockSlot Styled = new(key: 8, bodies: Named);
    public static readonly BlockSlot Deleted = new(key: 9, bodies: Named);
    public static readonly BlockSlot Revived = new(key: 10, bodies: Named);
    public static readonly BlockSlot Purged = new(key: 11, bodies: Named);
    public static readonly BlockSlot Reclaimed = new(key: 12, bodies: Counted);
    public static readonly BlockSlot Compacted = new(key: 13, bodies: Marked);
    public static readonly BlockSlot Exported = new(key: 14, bodies: Sourced);
    public static readonly BlockSlot Placed = new(key: 15, bodies: Instanced);
    public static readonly BlockSlot Repointed = new(key: 16, bodies: Instanced);
    public static readonly BlockSlot Baked = new(key: 17, bodies: Harvested);
    public static readonly BlockSlot PreviewFreed = new(key: 18, bodies: Instanced);
    public static readonly BlockSlot PreviewRetired = new(key: 19, bodies: Instanced);
    public static readonly BlockSlot PreviewRerendered = new(key: 20, bodies: Instanced);
    public static readonly BlockSlot PreviewDegraded = new(key: 21, bodies: Faded);
    public static readonly BlockSlot Undo = new(key: 22, bodies: Stamped);

    public CapabilitySet<BlockBodyKind> Bodies { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockBody : IFactBody<BlockBodyKind> {
    private BlockBody() { }
    public sealed record Definition(ResourceId Key, ResourceIndex Index) : BlockBody;
    public sealed record Object(ResourceId Id) : BlockBody;
    public sealed record Tally(int Count) : BlockBody;
    public sealed record Path(DocumentPath Value) : BlockBody;
    public sealed record Record(UndoSerial Serial) : BlockBody;
    public sealed record Signal : BlockBody;
    public sealed record Degrade(
        ResourceId Definition,
        RefreshPolicy Requested,
        RefreshPolicy Effective,
        RefreshRefusal Cause) : BlockBody;

    public BlockBodyKind Kind => Map(
        definition: BlockBodyKind.Definition,
        @object: BlockBodyKind.Object,
        tally: BlockBodyKind.Tally,
        path: BlockBodyKind.Path,
        record: BlockBodyKind.Record,
        signal: BlockBodyKind.Signal,
        degrade: BlockBodyKind.Degrade);
}

// --- [EXPORTS] -------------------------------------------------------------------------
global using BlockFact = Rasm.Rhino.Document.Fact<Rasm.Rhino.Blocks.BlockSlot, Rasm.Rhino.Blocks.BlockBody>;
global using BlockReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Blocks.BlockSlot, Rasm.Rhino.Blocks.BlockBody>;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BlockReceipts {
    extension(BlockReceipt) {
        public static Fin<BlockReceipt> Definition(
            BlockSlot slot,
            InstanceDefinition definition,
            Op key,
            Option<DocumentPath> path = default) =>
            from admitted in Optional(definition).ToFin(Fail: key.InvalidResult())
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
                .Traverse(id => ResourceId.Admit(value: id, key: key).ToValidation())
                .As()
                .ToFin()
            from receipt in BlockReceipt.All(
                slot: slot,
                bodies: admitted.Distinct().Map(static id => (BlockBody)new BlockBody.Object(Id: id)),
                key: key)
            select receipt;

        public static Fin<BlockReceipt> Tally(BlockSlot slot, int count, Op key) =>
            from _ in guard(count >= 0, key.InvalidResult()).ToFin()
            from receipt in BlockReceipt.Of(slot: slot, body: new BlockBody.Tally(Count: count), key: key)
            select receipt;

        public static Fin<BlockReceipt> Signal(BlockSlot slot, Op key) =>
            BlockReceipt.Of(slot: slot, body: new BlockBody.Signal(), key: key);

        internal static Fin<BlockReceipt> Refresh(
            Seq<(Guid Definition, SweepAction Action)> rows,
            Seq<RefreshDegrade> degraded,
            Op key) =>
            from swept in rows.TraverseM(row =>
                from slot in row.Action.Landing().ToFin(Fail: key.InvalidResult())
                from identity in ResourceId.Admit(value: row.Definition, key: key)
                from fact in BlockReceipt.Of(slot: slot, body: new BlockBody.Object(Id: identity), key: key)
                select fact).As()
            from faded in degraded.TraverseM(row =>
                from identity in ResourceId.Admit(value: row.Definition, key: key)
                from fact in BlockReceipt.Of(
                    slot: BlockSlot.PreviewDegraded,
                    body: new BlockBody.Degrade(
                        Definition: identity,
                        Requested: row.Requested,
                        Effective: row.Effective,
                        Cause: row.Cause),
                    key: key)
                select fact).As()
            select (swept + faded).Fold(BlockReceipt.Empty, static (state, value) => state + value);
    }

    extension(BlockReceipt receipt) {
        public Seq<(ResourceId Key, ResourceIndex Index)> DefinitionRefs(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Definition value
                ? Some((value.Key, value.Index))
                : Option<(ResourceId, ResourceIndex)>.None);

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

        public Seq<UndoSerial> Serials(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Record value
                ? Some(value.Serial)
                : Option<UndoSerial>.None);

        public Seq<BlockBody.Degrade> Degrades(BlockSlot slot) =>
            receipt.Project(slot: slot, select: static body => body is BlockBody.Degrade value
                ? Some(value)
                : Option<BlockBody.Degrade>.None);
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]                 | [INGRESS]                  | [RAIL]                                 | [EGRESS]              |
| :-----: | :---------------------- | :------------------------- | :------------------------------------- | :-------------------- |
|  [01]   | `BlockOp`               | generated values           | `Apply`                                | receipt fragment      |
|  [02]   | `BlockTrait`            | generated rows             | `CapabilitySet<CommitDemand>`          | undo + context demand |
|  [03]   | `BlockTransaction`      | `Batch`                    | factory-proved undo homogeneity        | admitted program      |
|  [04]   | `Blocks`                | `Commit` · `Ask`           | `DocumentCommit.Sealed` · `Fin`        | receipt or answer     |
|  [05]   | `ExplodedPiece`         | `Exploded`                 | `Compensated` · `Rollback` · `Custody` | detached custody      |
|  [06]   | `BlockSlot`/`BlockBody` | `BlockReceipts` extensions | spine `FactStream` accumulation        | slot projections      |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
