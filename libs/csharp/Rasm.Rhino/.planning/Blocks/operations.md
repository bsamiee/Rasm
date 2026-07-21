# [RASM_RHINO_BLOCK_OPERATIONS]

Block operations (`Rasm.Rhino.Blocks`) own one closed mutation family, one closed read family, one admitted transaction, and one fact-stream receipt. `Blocks.Commit` derives session needs from operation traits, acquires geometry through `GeometryIntake`, seals one shared `UndoBracket`, restores redraw through an accumulating rail, and emits stable definition and object evidence.

## [01]-[INDEX]

| [INDEX] | [OWNER] | [CONTRACT] |
| :-----: | :------ | :--------- |
|  [01]   | operation values · `BlockOp` | admitted mutation program |
|  [02]   | `BlockAsk` · `BlockAnswer` | detached read and projection program |
|  [03]   | `BlockTransaction` · `Blocks` | transaction admission and execution |
|  [04]   | `BlockReceipt` | consequence fact stream |

## [02]-[OPERATION_FAMILY]

`BlockOp` carries every verified definition mutation and block-specific instance operation. Shared metadata, linked-source, interaction, traversal, compaction, placement, and bake decisions enter as generated values; host booleans are projections of those values, never call-site discriminants.

`BlockMember` pairs one already-admitted `GeometryIntake` with one attribute set. Acquisition retains that bijection, and every lease closes after the host call or on partial admission failure.

`BlockMetadata` admits one trimmed canonical name, so authoring and amendment observe identical identity text.

`Bake` compares produced object ids with the source definition roster; shallow expansion requires equality, while recursive expansion requires at least the direct roster. Zero-member definitions admit the native null no-op as an empty object roster, every bake emits a produced-count tally, and partial insertion returns a typed failure.

```csharp signature
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
    public string Name { get; }
    public string Description { get; }
    public Option<BlockHyperlink> Hyperlink { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string name,
        ref string description,
        ref Option<BlockHyperlink> hyperlink) {
        name = name?.Trim() ?? string.Empty;
        validationError = string.IsNullOrWhiteSpace(value: name) || description is null
            ? new ValidationError(message: "block metadata is invalid")
            : validationError;
    }
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
public sealed partial class HostInteraction {
    public static readonly HostInteraction Quiet = new(key: 0, isQuiet: true);
    public static readonly HostInteraction Interactive = new(key: 1, isQuiet: false);

    public bool IsQuiet { get; }
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
        string FullPath,
        Option<string> RelativePath,
        LinkMode Mode,
        HostInteraction Interaction) : BlockOp;
    public sealed record Sever(ResourceRef Target, HostInteraction Interaction) : BlockOp;
    public sealed record Refresh(ResourceRef Target) : BlockOp;
    public sealed record Retarget(
        ResourceRef Target,
        string Filename,
        LinkTraversal Traversal,
        HostInteraction Interaction) : BlockOp;
    public sealed record Style(ResourceRef Target, InstanceDefinitionLayerStyle LayerStyle) : BlockOp;
    public sealed record Delete(ResourceRef Target, DeletionPolicy Policy) : BlockOp;
    public sealed record Undelete(ResourceRef Target) : BlockOp;
    public sealed record Purge(ResourceRef Target) : BlockOp;
    public sealed record PurgeUnused : BlockOp;
    public sealed record Compact(CompactPolicy Policy) : BlockOp;
    public sealed record Export(ResourceRef Target, string Path) : BlockOp;
    public sealed record Place(ResourceRef Target, Seq<Placement> Instances) : BlockOp;
    public sealed record Repoint(TableTarget Instances, ResourceRef Target) : BlockOp;
    public sealed record Bake(Guid InstanceId, ExplodeDepth Depth, InstanceDisposition Disposition) : BlockOp;

    internal BlockTrait Traits => this switch {
        Author or Regeometry => BlockTrait.Contextual,
        Purge or PurgeUnused or Compact or Export => BlockTrait.Unrecorded,
        _ => BlockTrait.Mutation,
    };

    internal Fin<BlockReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            context: (Document: document, Domain: domain, Op: op),
            author: static (context, edit) =>
                from metadata in Optional(edit.Metadata).ToFin(Fail: context.Op.InvalidInput())
                from conflict in Optional(edit.Conflict).ToFin(Fail: context.Op.InvalidInput())
                from _ in guard(edit.BasePoint.IsValid, context.Op.InvalidInput()).ToFin()
                from name in context.Op.AcceptText(value: metadata.Name)
                from resolved in Optional(context.Document.InstanceDefinitions.Find(name)).Case switch {
                    InstanceDefinition existing => conflict.Switch(
                        (Existing: existing, Document: context.Document, Name: name, Op: context.Op),
                        fail: static held => Fin.Fail<(string Name, Option<InstanceDefinition> Reused)>(
                            error: held.Op.InvalidInput()),
                        reuse: static held => Fin.Succ(value: (held.Name, Some(held.Existing))),
                        mint: static held => held.Op.AcceptText(value: held.Document.InstanceDefinitions
                            .GetUnusedInstanceDefinitionName(root: held.Name))
                            .Map(minted => (minted, Option<InstanceDefinition>.None))),
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
                                        name: resolved.Name,
                                        description: metadata.Description,
                                        url: hyperlink.Url,
                                        urlTag: hyperlink.Tag,
                                        basePoint: edit.BasePoint,
                                        geometry: geometry,
                                        attributes: attributes),
                                    _ => context.Document.InstanceDefinitions.Add(
                                        name: resolved.Name,
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
                from metadata in Optional(edit.Metadata).ToFin(Fail: context.Op.InvalidInput())
                from interaction in Optional(edit.Interaction).ToFin(Fail: context.Op.InvalidInput())
                let hyperlink = metadata.Hyperlink.Case switch {
                    BlockHyperlink value => (Url: value.Url, Tag: value.Tag),
                    _ => (Url: string.Empty, Tag: string.Empty),
                }
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Modify(
                    idefIndex: definition.Index, newName: metadata.Name, newDescription: metadata.Description,
                    newUrl: hyperlink.Url, newUrlTag: hyperlink.Tag, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Amended, definition: definition, key: context.Op)
                select receipt,
            regeometry: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(
                    definition.UpdateType is InstanceDefinitionUpdateType.Static
                        or InstanceDefinitionUpdateType.Embedded,
                    context.Op.InvalidInput()).ToFin()
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
                from path in context.Op.AcceptText(value: edit.FullPath)
                from relative in edit.RelativePath.Traverse(context.Op.AcceptText).As()
                from mode in Optional(edit.Mode).ToFin(Fail: context.Op.InvalidInput())
                from interaction in Optional(edit.Interaction).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Catch(() => {
                    using FileReference reference = relative.Case switch {
                        string anchor => FileReference.CreateFromFullAndRelativePaths(fullPath: path, relativePath: anchor),
                        _ => FileReference.CreateFromFullPath(fullPath: path),
                    };
                    return context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifySourceArchive(
                        idefIndex: definition.Index, sourceArchive: reference, updateType: mode.UpdateType, quiet: interaction.IsQuiet));
                })
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Rebound,
                    definition: definition,
                    key: context.Op,
                    path: Some(path))
                select receipt,
            sever: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from interaction in Optional(edit.Interaction).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.DestroySourceArchive(
                    definition: definition, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Severed, definition: definition, key: context.Op)
                select receipt,
            refresh: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(
                    !definition.IsTenuous
                        && (definition.UpdateType is InstanceDefinitionUpdateType.Linked
                            or InstanceDefinitionUpdateType.LinkedAndEmbedded),
                        context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Refreshed, definition: definition, key: context.Op)
                select receipt,
            retarget: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from path in context.Op.AcceptText(value: edit.Filename)
                from traversal in Optional(edit.Traversal).ToFin(Fail: context.Op.InvalidInput())
                from interaction in Optional(edit.Interaction).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: definition.Index, filename: path, updateNestedLinks: traversal.NestedLinks, quiet: interaction.IsQuiet))
                from receipt in BlockReceipt.Definition(
                    slot: BlockSlot.Retargeted,
                    definition: definition,
                    key: context.Op,
                    path: Some(path))
                select receipt,
            style: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(
                    (definition.UpdateType is InstanceDefinitionUpdateType.Linked
                        or InstanceDefinitionUpdateType.LinkedAndEmbedded)
                        && Enum.IsDefined(value: edit.LayerStyle),
                    context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    definition.LayerStyle = edit.LayerStyle;
                    return context.Op.Confirm(success: definition.LayerStyle == edit.LayerStyle);
                })
                from receipt in BlockReceipt.Definition(slot: BlockSlot.Styled, definition: definition, key: context.Op)
                select receipt,
            delete: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from policy in Optional(edit.Policy).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Delete(
                    idefIndex: definition.Index, deleteReferences: policy.DeleteReferences, quiet: policy.Quiet))
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
                from policy in Optional(edit.Policy).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Catch(() => {
                    context.Document.InstanceDefinitions.Compact(ignoreUndoReferences: policy.IgnoreUndoReferences);
                    return Fin.Succ(value: unit);
                })
                select BlockReceipt.Signal(slot: BlockSlot.Compacted),
            export: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from path in context.Op.AcceptText(value: edit.Path)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Export(
                    idefIndex: definition.Index, filename: path))
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
                        recorded: static (ctx, request) =>
                            from _ in guard(
                                request.Attributes is not null && request.History is not null && request.Kind is not null,
                                ctx.Op.InvalidInput()).ToFin()
                            from id in Place(motion: request.Motion, op: ctx.Op,
                                add: () => ctx.Document.Objects.AddInstanceObject(
                                    instanceDefinitionIndex: ctx.Index, instanceXform: request.Motion, attributes: request.Attributes,
                                    history: request.History, reference: request.Kind.IsReference))
                            select id))).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Placed, ids: placed, key: context.Op)
                select receipt,
            repoint: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from target in Optional(edit.Instances).ToFin(Fail: context.Op.InvalidInput())
                from ids in target.Resolve(document: context.Document, key: context.Op)
                from repointed in ids.TraverseM(id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(
                    objectId: id, instanceDefinitionIndex: definition.Index)).Map(_ => id)).As()
                from receipt in BlockReceipt.Objects(slot: BlockSlot.Repointed, ids: repointed, key: context.Op)
                select receipt,
            bake: static (context, edit) =>
                from _ in guard(edit.InstanceId != Guid.Empty, context.Op.InvalidInput()).ToFin()
                from depth in Optional(edit.Depth).ToFin(Fail: context.Op.InvalidInput())
                from disposition in Optional(edit.Disposition).ToFin(Fail: context.Op.InvalidInput())
                from native in Optional(context.Document.Objects.FindId(edit.InstanceId))
                    .ToFin(Fail: context.Op.MissingContext())
                from instance in Optional(native as InstanceObject).ToFin(Fail: context.Op.InvalidInput())
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
        return Optional(target).ToFin(Fail: op.InvalidInput()).Bind(active => active.Switch(
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
            land: member => Optional(member).ToFin(Fail: op.InvalidInput())
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
                from target in Optional(source.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in target.Resolve(document: ctx.Document, key: ctx.Op)
                from id in ids.Head.Filter(_ => ids.Count == 1).ToFin(Fail: ctx.Op.InvalidInput())
                from native in Optional(ctx.Document.Objects.FindId(id)).ToFin(Fail: ctx.Op.MissingContext())
                from text in Optional(native as TextObject).ToFin(Fail: ctx.Op.InvalidInput())
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
                from fallback in Optional(source.DefaultValue).ToFin(Fail: ctx.Op.InvalidInput())
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
    public sealed record MintName(Option<string> Root) : BlockAsk;
    public sealed record Pieces(Guid InstanceId, ExplodePolicy Policy) : BlockAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAnswer : IDetachedDocumentResult {
    private BlockAnswer() { }
    public sealed record State(BlockSnapshot Snapshot) : BlockAnswer;
    public sealed record Dependency(int Measure) : BlockAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Preview) : BlockAnswer;
    public sealed record Fields(Arr<BlockField> Descriptors) : BlockAnswer;
    public sealed record Token(string Value) : BlockAnswer;
    public sealed record Minted(string Name) : BlockAnswer;
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

`Blocks.Commit` walks the shared envelope: needs derive through `SessionNeed.Mutation`, one document demand, optional kernel context, and `DocumentCommit.Sealed` owns the bracket, restoration, and post-restore redraw — a hand-spelled envelope beside it is the deleted form.

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
               from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from program in toSeq(operations.ToArray())
                   .TraverseM(operation => Optional(operation).ToFin(Fail: op.InvalidInput()))
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
        return from owner in Optional(session).ToFin(Fail: op.InvalidInput())
               from plan in Optional(transaction).ToFin(Fail: op.InvalidInput())
               from receipt in owner.Demand(
                   use: document => Run(document: document, plan: plan, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(undo: plan.RecordsUndo, redraw: plan.Redraw).ToArray())
               select receipt;
    }

    public static Fin<BlockAnswer> Ask(DocumentSession session, BlockAsk request) {
        Op op = Op.Of();
        return from owner in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in owner.Demand(
                   use: document => active.Switch(
                       context: (Document: document, Op: op),
                       state: static (ctx, ask) =>
                           from scope in Optional(ask.Scope).ToFin(Fail: ctx.Op.InvalidInput())
                           from snapshot in BlockSnapshot.Of(
                               target: ask.Target,
                               document: ctx.Document,
                               scope: scope,
                               key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.State(Snapshot: snapshot),
                       dependency: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from probe in Optional(ask.Probe).ToFin(Fail: ctx.Op.InvalidInput())
                           from measure in probe.Measure(owner: definition, document: ctx.Document, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Dependency(Measure: measure),
                       preview: static (ctx, ask) =>
                           from definition in Definitions.Resolve(target: ask.Target, document: ctx.Document, key: ctx.Op)
                           from spec in Optional(ask.Spec).ToFin(Fail: ctx.Op.InvalidInput())
                           from bitmap in spec.Render(definition: definition, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Rendered(
                               Preview: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
                       fields: static (ctx, ask) => Optional(ask.Source).ToFin(Fail: ctx.Op.InvalidInput())
                           .Bind(source => source.Read(document: ctx.Document, op: ctx.Op)),
                       mintName: static (ctx, ask) =>
                           from root in ask.Root.Traverse(ctx.Op.AcceptText).As()
                           from minted in ctx.Op.Catch(() => ctx.Op.AcceptText(value: root.Case switch {
                               string value => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: value),
                               _ => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(),
                           }))
                           select (BlockAnswer)new BlockAnswer.Minted(Name: minted),
                       pieces: static (ctx, ask) =>
                           from _ in guard(ask.InstanceId != Guid.Empty, ctx.Op.InvalidInput()).ToFin()
                           from native in Optional(ctx.Document.Objects.FindId(ask.InstanceId))
                               .ToFin(Fail: ctx.Op.MissingContext())
                           from instance in Optional(native as InstanceObject).ToFin(Fail: ctx.Op.InvalidInput())
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
            stamp: static (receipt, serial) => receipt + BlockReceipt.UndoRecords(serials: Seq(serial)),
            op: op)
        select receipt;

    private static Fin<Seq<ExplodedPiece>> Exploded(InstanceObject instance, ExplodePolicy policy, Op key) =>
        Optional(policy).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() => {
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

    private static Fin<Unit> ReleaseCaptured(
        (RhinoObject[] Pieces, ObjectAttributes[] Attributes, Transform[] Motions) captured,
        Seq<ExplodedPiece> products,
        bool retainProducts,
        Op key) {
        System.Collections.Generic.HashSet<ObjectAttributes> transferred = new(ReferenceEqualityComparer.Instance);
        products.Iter(product => transferred.Add(item: product.Attributes));
        Seq<Action> actions = toSeq(captured.Pieces)
            .Choose(static piece => Optional(piece?.Geometry))
            .Map(static geometry => new Action(geometry.Dispose))
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
        Optional(depth).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() => {
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

`BlockReceipt` has no default state. Each definition fact retains stable guid and transient table index and mints its optional path fact in the same call; object receipt admission rejects any empty id before distinct projection, and path, tally, signal, and undo facts share the same closed payload family.

Slot projections derive from `Facts`; no consumer re-queries a mutation merely to reconstruct its consequences.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BlockSlot {
    public static readonly BlockSlot Authored = new(key: 0);
    public static readonly BlockSlot Reused = new(key: 18);
    public static readonly BlockSlot Amended = new(key: 1);
    public static readonly BlockSlot Regeometried = new(key: 2);
    public static readonly BlockSlot Rebound = new(key: 3);
    public static readonly BlockSlot Severed = new(key: 4);
    public static readonly BlockSlot Refreshed = new(key: 5);
    public static readonly BlockSlot Retargeted = new(key: 6);
    public static readonly BlockSlot Styled = new(key: 7);
    public static readonly BlockSlot Deleted = new(key: 8);
    public static readonly BlockSlot Revived = new(key: 9);
    public static readonly BlockSlot Purged = new(key: 10);
    public static readonly BlockSlot Reclaimed = new(key: 11);
    public static readonly BlockSlot Compacted = new(key: 12);
    public static readonly BlockSlot Exported = new(key: 13);
    public static readonly BlockSlot Placed = new(key: 14);
    public static readonly BlockSlot Repointed = new(key: 15);
    public static readonly BlockSlot Baked = new(key: 16);
    public static readonly BlockSlot Undo = new(key: 17);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockBody {
    private BlockBody() { }
    public sealed record Definition(Guid Key, int Index) : BlockBody;
    public sealed record Object(Guid Id) : BlockBody;
    public sealed record Tally(int Count) : BlockBody;
    public sealed record Path(string Value) : BlockBody;
    public sealed record Record(uint Serial) : BlockBody;
    public sealed record Signal : BlockBody;
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record BlockFact(BlockSlot Slot, BlockBody Body);

public sealed class BlockReceipt : IDetachedDocumentResult {
    private readonly Seq<BlockFact> facts;

    private BlockReceipt(Seq<BlockFact> facts) => this.facts = facts;

    public static BlockReceipt Empty { get; } = new(facts: Seq<BlockFact>());

    public Seq<BlockFact> Facts => facts;

    public static BlockReceipt operator +(BlockReceipt left, BlockReceipt right) =>
        new(facts: left.facts + right.facts);

    public static Fin<BlockReceipt> Definition(
        BlockSlot slot,
        InstanceDefinition definition,
        Op key,
        Option<string> path = default) =>
        from admitted in Optional(definition).ToFin(Fail: key.InvalidResult())
        from _ in guard(admitted.Id != Guid.Empty && admitted.Index >= 0, key.InvalidResult()).ToFin()
        select new BlockReceipt(
            facts: Seq(new BlockFact(
                Slot: slot,
                Body: new BlockBody.Definition(Key: admitted.Id, Index: admitted.Index)))
                + path.Map(value => new BlockFact(Slot: slot, Body: new BlockBody.Path(Value: value))).ToSeq());

    public static Fin<BlockReceipt> Objects(BlockSlot slot, Seq<Guid> ids, Op key) =>
        from _ in guard(!ids.IsEmpty, key.InvalidResult()).ToFin()
        from admitted in ids
            .Traverse(id => guard(id != Guid.Empty, key.InvalidResult()).ToFin().ToValidation())
            .As()
            .ToFin()
        select new BlockReceipt(facts: admitted.Distinct()
            .Map(id => new BlockFact(Slot: slot, Body: new BlockBody.Object(Id: id))));

    public static Fin<BlockReceipt> Tally(BlockSlot slot, int count, Op key) =>
        guard(count >= 0, key.InvalidResult()).ToFin()
            .Map(_ => new BlockReceipt(facts: Seq(new BlockFact(Slot: slot, Body: new BlockBody.Tally(Count: count)))));

    internal static BlockReceipt UndoRecords(Seq<uint> serials) =>
        new(facts: serials.Map(serial => new BlockFact(
            Slot: BlockSlot.Undo,
            Body: new BlockBody.Record(Serial: serial))));

    public static BlockReceipt Signal(BlockSlot slot) =>
        new(facts: Seq(new BlockFact(Slot: slot, Body: new BlockBody.Signal())));

    public Seq<(Guid Key, int Index)> DefinitionRefs(BlockSlot slot) =>
        Project(slot: slot, pick: static body => body is BlockBody.Definition value
            ? Some((value.Key, value.Index))
            : Option<(Guid, int)>.None);

    public Seq<int> Definitions(BlockSlot slot) =>
        DefinitionRefs(slot: slot).Map(static definition => definition.Index);

    public Seq<Guid> Ids(BlockSlot slot) =>
        Project(slot: slot, pick: static body => body is BlockBody.Object value ? Some(value.Id) : Option<Guid>.None);

    public Seq<int> Tallies(BlockSlot slot) =>
        Project(slot: slot, pick: static body => body is BlockBody.Tally value ? Some(value.Count) : Option<int>.None);

    public Seq<string> Paths(BlockSlot slot) =>
        Project(slot: slot, pick: static body => body is BlockBody.Path value ? Some(value.Value) : Option<string>.None);

    public int FactCount(BlockSlot slot) => facts.Count(fact => fact.Slot == slot);

    private Seq<T> Project<T>(BlockSlot slot, Func<BlockBody, Option<T>> pick) =>
        facts.Filter(fact => fact.Slot == slot).Choose(fact => pick(fact.Body));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [OWNER] | [INGRESS] | [RAIL] | [EGRESS] |
| :-----: | :------ | :-------- | :----- | :------- |
|  [01]   | `BlockOp` | generated values | `Apply` | receipt fragment |
|  [02]   | `BlockTransaction` | `Batch` | trait homogeneity | admitted program |
|  [03]   | `Blocks` | `Commit` · `Ask` | `Fin` · `Validation` · `UndoBracket` | receipt or answer |
|  [04]   | `BlockReceipt` | closed factories | additive facts | slot projections |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
