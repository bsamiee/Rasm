# [RASM_RHINO_BLOCK_OPERATIONS]

Block operations (`Rasm.Rhino.Blocks`) own one closed mutation family, one closed read family, and one admitted transaction. `Blocks.Commit` derives session needs from operation demands, acquires geometry through `GeometryIntake`, and frames each command through `DocumentCommit.Sealed`.

## [01]-[INDEX]

- [02]-[OPERATION_FAMILY]: `BlockOp` closing every definition and instance mutation, `BlockTrait` naming each case's commit demands, and the leased member admission every authoring arm runs.
- [03]-[READ_FAMILY]: `BlockAsk`/`BlockAnswer` closing the read questions, `FieldSource` dispatching the text-field grammar, and `ExplodedPiece` owning detached custody.
- [04]-[COMMIT_SPINE]: `BlockTransaction` admitting one homogeneous program and `Blocks` walking the shared commit entry.
- [05]-[SURFACE_LEDGER]: owner-to-ingress-to-rail-to-egress roster.

## [02]-[OPERATION_FAMILY]

- Owner: `BlockOp` `[Union]` carries every verified definition mutation and block-specific instance operation; `BlockTrait` `[SmartEnum<int>]` names each case's commit demands as ONE `CapabilitySet<CommitDemand>` column; `BlockMetadata`, `BlockHyperlink`, and `BlockMember` are the admitted payload owners; `SourceReference` closes the linked-source address; `LinkMode`, `LinkTraversal`, `CompactPolicy`, and `InstanceDisposition` carry host arguments as rows.
- Entry: each payload owner's `Of` is the ONE admission — the generated `Create` throws and no arm reaches it; `BlockOp.Apply` is the single total dispatch, one host member per case.
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
// --- [IMPORTS] -------------------------------------------------------------------------
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
    internal Fin<Unit> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            context: (Document: document, Domain: domain, Op: op),
            author: static (context, edit) =>
                from admitted in guard(edit.BasePoint.IsValid, context.Op.InvalidInput()).ToFin()
                from applied in Optional(context.Document.InstanceDefinitions.Find(edit.Metadata.Name.Value)).Match(
                    None: () => Authored(name: edit.Metadata.Name, edit: edit, context: context),
                    Some: existing => edit.Conflict.Switch(
                        (Existing: existing, Edit: edit, Held: context),
                        fail: static held => Fin.Fail<Unit>(error: held.Held.Op.InvalidInput()),
                        reuse: static _ => Fin.Succ(value: unit),
                        mint: static held => held.Held.Op.AcceptText(value: held.Held.Document.InstanceDefinitions
                                .GetUnusedInstanceDefinitionName(root: held.Edit.Metadata.Name.Value))
                            .Bind(minted => Authored(
                                name: ResourceName.Create(minted), edit: held.Edit, context: held.Held))))
                select unit,
            amend: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                let hyperlink = BlockHyperlink.Host(value: edit.Metadata.Hyperlink)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Modify(
                    idefIndex: definition.Index, newName: edit.Metadata.Name.Value,
                    newDescription: edit.Metadata.Description,
                    newUrl: hyperlink.Url, newUrlTag: hyperlink.Tag, quiet: edit.Interaction.IsQuiet))
                select unit,
            regeometry: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from writable in guard(!mode.Facets.Admits(capability: SourceFacet.Reads), context.Op.InvalidInput()).ToFin()
                from applied in Admitted(
                    members: edit.Members,
                    domain: context.Domain,
                    op: context.Op,
                    run: (geometry, attributes) =>
                        from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifyGeometry(
                            idefIndex: definition.Index, newGeometry: geometry, newAttributes: attributes))
                        select unit)
                select unit,
            rebind: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in edit.Source.Use(
                    body: reference => context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifySourceArchive(
                        idefIndex: definition.Index, sourceArchive: reference,
                        updateType: edit.Mode.UpdateType, quiet: edit.Interaction.IsQuiet)),
                    op: context.Op)
                select unit,
            sever: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.DestroySourceArchive(
                    definition: definition, quiet: edit.Interaction.IsQuiet))
                select unit,
            refresh: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(
                    !definition.IsTenuous && mode.Facets.Admits(capability: SourceFacet.Reads),
                    context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                select unit,
            retarget: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: definition.Index, filename: edit.Filename.Value,
                    updateNestedLinks: edit.Traversal.NestedLinks, quiet: edit.Interaction.IsQuiet))
                select unit,
            style: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from mode in SourceMode.Of(update: definition.UpdateType, key: context.Op)
                from _ in guard(mode.Facets.Admits(capability: SourceFacet.Reads), context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Catch(() => {
                    definition.LayerStyle = edit.LayerStyle.Host;
                    return context.Op.Confirm(success: definition.LayerStyle == edit.LayerStyle.Host);
                })
                select unit,
            delete: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Delete(
                    idefIndex: definition.Index,
                    deleteReferences: edit.Policy.DeleteReferences,
                    quiet: edit.Policy.Interaction.IsQuiet))
                select unit,
            undelete: static (context, edit) =>
                from definition in edit.Target.Resolve(
                    document: context.Document, lens: Definitions.DeletedLens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Undelete(idefIndex: definition.Index))
                select unit,
            purge: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Purge(idefIndex: definition.Index))
                select unit,
            purgeUnused: static (context, _) =>
                from _ in context.Op.Catch(() => Fin.Succ(value: context.Document.InstanceDefinitions.PurgeUnused()))
                select unit,
            compact: static (context, edit) =>
                from _ in context.Op.Catch(() => Fin.Succ(value: Op.Side(() =>
                    context.Document.InstanceDefinitions.Compact(ignoreUndoReferences: edit.Policy.IgnoreUndoReferences))))
                select unit,
            export: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Export(
                    idefIndex: definition.Index, filename: edit.Path.Value))
                select unit,
            place: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from _ in guard(!edit.Instances.IsEmpty, context.Op.InvalidInput()).ToFin()
                from __ in edit.Instances.TraverseM(placement => placement.Switch(
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
                select unit,
            repoint: static (context, edit) =>
                from definition in Definitions.Resolve(target: edit.Target, document: context.Document, key: context.Op)
                from ids in edit.Instances.Resolve(document: context.Document, key: context.Op)
                from _ in ids.TraverseM(id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(
                    objectId: id, instanceDefinitionIndex: definition.Index))).As()
                select unit,
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
                select unit);

    private static Fin<Unit> Authored(
        ResourceName name,
        Author edit,
        (RhinoDoc Document, Option<Context> Domain, Op Op) context) =>
        Admitted(
            members: edit.Members,
            domain: context.Domain,
            op: context.Op,
            run: (geometry, attributes) =>
                from _ in context.Op.Catch(() => ResourceIndex.Admit(
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
                select unit);

    private static Fin<Unit> Place(Transform motion, Op op, Func<Guid> add) =>
        from _ in guard(motion.IsValid, op.InvalidInput()).ToFin()
        from __ in op.Catch(() => Optional(add())
            .Filter(static value => value != Guid.Empty)
            .ToFin(Fail: op.InvalidResult()))
        select unit;

    private static Fin<Unit> Admitted(
        Seq<BlockMember> members,
        Option<Context> domain,
        Op op,
        Func<IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>, Fin<Unit>> run) =>
        members.IsEmpty
            ? op.Catch(() => run(Array.Empty<GeometryBase>(), Array.Empty<ObjectAttributes>()))
            : from active in domain.ToFin(Fail: op.MissingContext())
              from admitted in Leased(members: members, domain: active, op: op)
              from _ in op.Catch(() => run(
                      admitted.Map(static member => member.Geometry.Resource).AsIterable(),
                      admitted.Map(static member => member.Attributes).AsIterable()))
                  .Settled(
                      held: admitted,
                      release: static member => Fin.Succ(value: member.Geometry.Dispose()),
                      key: op)
              select unit;

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
- Law: `Blocks.Commit` walks the shared commit entry and nothing else — needs derive through `SessionNeed.Mutation` over the program's `UndoCustody` row, one document demand carries the whole program, the kernel `Context` resolves only where a case demands it, and `DocumentCommit.Sealed` owns bracket, restoration, redraw, and seal. A hand-spelled `UndoBracket.Begin`, a redraw triple, or an inline need roster beside this envelope is the deleted form.
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
    public static Fin<Unit> Commit(DocumentSession session, BlockTransaction transaction) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from plan in op.Need(transaction)
               from _ in owner.Demand(
                   use: document => Run(document: document, plan: plan, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(custody: plan.Custody, redraw: plan.Redraw).ToArray())
               select unit;
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

    private static Fin<Unit> Run(RhinoDoc document, BlockTransaction plan, Op op) =>
        from domain in plan.Operations.Exists(
            static operation => operation.Traits.Demands.Admits(capability: CommitDemand.KernelContext))
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from _ in DocumentCommit.Sealed(
            document: document,
            name: plan.Name,
            recordsUndo: plan.Custody == UndoCustody.Recorded,
            redraw: plan.Redraw,
            run: () => plan.Operations
                .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op))
                .As()
                .Map(static _ => unit),
            project: Fin.Succ,
            op: op)
        select unit;

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

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]            | [INGRESS]        | [RAIL]                                 | [EGRESS]              |
| :-----: | :----------------- | :--------------- | :------------------------------------- | :-------------------- |
|  [01]   | `BlockOp`          | generated values | `Apply`                                | `Unit`                |
|  [02]   | `BlockTrait`       | generated rows   | `CapabilitySet<CommitDemand>`          | undo + context demand |
|  [03]   | `BlockTransaction` | `Batch`          | factory-proved undo homogeneity        | admitted program      |
|  [04]   | `Blocks`           | `Commit` · `Ask` | `DocumentCommit.Sealed` · `Fin`        | `Unit` or answer      |
|  [05]   | `ExplodedPiece`    | `Exploded`       | `Compensated` · `Rollback` · `Custody` | detached custody      |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
