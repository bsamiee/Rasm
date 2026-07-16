# [RASM_RHINO_RENDER_REGISTRY]

Content registry and operation rail (`Rasm.Rhino.Render`). `ContentTypeInfo` detaches the disposable factory census, `ContentSerializer` adapts custom formats, and `ContentOp` has only two identity regimes: admission without an existing target and mutation of one resolved target. Nested generated unions carry the real host discriminants. `Contents.Commit` owns one demand window, one `UndoBracket`, one verified per-kind table scope, redraw restoration, and one additive receipt. Reads use typed `ContentQuery<T>` programs whose result type stays correlated at compile time and carries the session's `IDetachedDocumentResult` census — a detached fact or a self-disposing capsule, never a raw host shape. Eleven static `RenderContent` events fold into detached `ContentFact` values through `ContentStream`; document-table transitions remain the Document events owner's surface.

## [01]-[INDEX]

- [02]-[FACTORY_REGISTRY]: `ContentTypeInfo`, plug-in registration, and the `ContentSerializer` adapter.
- [03]-[OPERATION_FAMILY]: `ContentAdmission`, `ContentMutation`, and the two-case `ContentOp` dispatch.
- [04]-[COMMIT_AND_QUERY]: `ContentTransaction`, `ContentQuery<T>`, and the `Contents` entries.
- [05]-[RECEIPTS]: `ContentSlot`, `ContentBody`, and the `ContentReceipt` monoid.
- [06]-[EVENTS]: `ContentPulse`, `ContentSignal`, `ContentFact`, and the `ContentStream` observation capsule.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[FACTORY_REGISTRY]

- Owner: `ContentTypeInfo` — the detached factory descriptor: type id, internal name, render-engine id, plug-in id; the census enumerates `RenderContentType.GetAllAvailableTypes`, projects, and disposes every descriptor. `ContentSerializer` — the host `RenderContentSerializer` adapter over one typed program record carrying extension, kind, read/write grants, descriptions, and delegates.
- Law: `ContentUuids` remains the well-known factory vocabulary at the call site; a copied `[SmartEnum<Guid>]` roster adds no admission, behavior, or type safety and is deleted. Foreign factories enter by descriptor id from the census.
- Law: plug-in content classes register through `RenderContent.RegisterContent` over their `CustomRenderContentAttribute`; registration is plug-in-lifecycle surface the hosting plug-in owns, and this page carries only the `Contents.Register` pass-through that reports the registered types.
- Boundary: the host also discovers serializers through `RenderPlugIn.RenderContentSerializers()`; the adapter shape is this page's, the plug-in override that returns it is the plug-in's.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Reflection;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ContentTypeInfo(Guid TypeId, string InternalName, Guid RenderEngineId, Guid PlugInId) : IDetachedDocumentResult {
    internal static Fin<Seq<ContentTypeInfo>> Census(Op key) =>
        key.Catch(() => toSeq(RenderContentType.GetAllAvailableTypes()).TraverseM(descriptor => key.Catch(() => {
            using (descriptor) {
                return Fin.Succ(value: new ContentTypeInfo(
                    TypeId: descriptor.Id, InternalName: descriptor.InternalName,
                    RenderEngineId: descriptor.RenderEngineId, PlugInId: descriptor.PlugInId));
            }
        })).As().Map(static rows => toSeq(rows)));
}

public sealed record SerializerProgram(
    string FileExtension,
    RenderContentKind Kind,
    Option<Func<string, Fin<Lease<RenderContent>>>> Read,
    Option<Func<string, RenderContent, CreatePreviewEventArgs, Fin<Unit>>> Write,
    Option<Func<ContentSerializer, RhinoDoc, Seq<string>, RenderContentKind, RenderContentSerializer.LoadMultipleFlags, Fin<Unit>>> LoadMultiple,
    string EnglishDescription,
    string LocalDescription);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ContentSerializer : RenderContentSerializer {
    private readonly SerializerProgram program;

    public ContentSerializer(SerializerProgram program)
        : base(fileExtension: program.FileExtension, contentKind: program.Kind,
               canRead: program.Read.IsSome, canWrite: program.Write.IsSome) =>
        this.program = program;

    public override string EnglishDescription => program.EnglishDescription;
    public override string LocalDescription => program.LocalDescription;

    public override RenderContent Read(string pathToFile) =>
        program.Read
            .Map(read => read(pathToFile).Match(
                Succ: static lease => lease is Lease<RenderContent>.Owned owned ? owned.Value : (RenderContent)null!,
                Fail: static _ => (RenderContent)null!))
            .IfNoneUnsafe((RenderContent?)null)!;

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs previewArgs) =>
        program.Write.Map(write => write(pathToFile, renderContent, previewArgs).IsSucc).IfNone(false);

    public override bool CanLoadMultiple() => program.LoadMultiple.IsSome;

    public override bool LoadMultiple(
        RhinoDoc document, IEnumerable<string> paths, RenderContentKind kind, RenderContentSerializer.LoadMultipleFlags flags) =>
        program.LoadMultiple.Map(load => load(this, document, toSeq(paths), kind, flags).IsSucc).IfNone(false);

    public Fin<Unit> Register(Guid pluginId) {
        Op op = Op.Of(name: nameof(ContentSerializer));
        return op.Catch(() => op.Confirm(success: RegisterSerializer(id: pluginId)));
    }
}
```

## [03]-[OPERATION_FAMILY]

- Owner: `ContentOp` `[Union]` — two cases derived from target identity: `Admit(ContentAdmission)` has no existing target, and `Mutate(ContentRef, ContentMutation)` resolves one target once. `ContentAdmission` closes factory, serialized, material, texture, and environment mint paths behind one owned-lease rail. `ContentMutation` carries ten distinct host concerns; `TreeMutation` and `Grouping` close their bounded subspaces without boolean modes.
- Law: admission internalizes custody — every factory, IO, material, texture, and environment mint becomes an owned lease; top-level results transfer through the expected kind table, parented factory results transfer through the parent slot, and every refused transfer disposes the lease.
- Law: transaction kind is a verified table-scope key — each admission exposes its expected kind, each target mutation derives its live kind, and either must equal the plan kind before mutation.
- Law: graph surgery is one target mutation — `TreeMutation` discriminates graft, prune, and slot state under its own `ChangeReason`; slot-state admission rejects an empty patch.
- Law: field, parameter, and texture writes compose their owners; material assignment resolves `TableTarget`, contains every `ObjRef` lifetime, and carries native assignment choices.
- Growth: a new admission path is one `ContentAdmission` case; a new target concern is one `ContentMutation` case; `ContentOp` remains closed at two cases.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentAdmission {
    private ContentAdmission() { }
    public sealed record Factory(ContentKind Kind, Guid TypeId, Option<(ContentRef Parent, string Slot)> Into) : ContentAdmission;
    public sealed record Serialized(ContentKind Kind, ContentIo Source) : ContentAdmission;
    public sealed record Material(MaterialMint Source) : ContentAdmission;
    public sealed record Texture(TextureMint Source) : ContentAdmission;
    public sealed record Environment(EnvironmentState State) : ContentAdmission;

    internal ContentKind Expected => Switch(
        factory: static row => row.Kind,
        serialized: static row => row.Kind,
        material: static _ => ContentKind.Material,
        texture: static _ => ContentKind.Texture,
        environment: static _ => ContentKind.Environment);

    internal Fin<ContentReceipt> Apply(RhinoDoc document, ChangeReason reason, Op op) =>
        Switch(
            context: (Document: document, Reason: reason, Op: op),
            factory: static (context, source) =>
                from _ in guard(source.TypeId != Guid.Empty, context.Op.InvalidInput())
                from parent in source.Into.Traverse(into =>
                    from slot in context.Op.AcceptText(value: into.Slot)
                    from live in into.Parent.Resolve(document: context.Document, key: context.Op)
                    select (Content: live, Slot: slot)).As()
                from minted in context.Op.Catch(() => Optional(RenderContent.Create(context.Document, source.TypeId))
                    .ToFin(Fail: context.Op.InvalidResult()))
                from receipt in Transfer(
                    expected: source.Kind,
                    lease: new Lease<RenderContent>.Owned(Value: minted),
                    document: context.Document,
                    parent: parent,
                    reason: context.Reason,
                    op: context.Op)
                select receipt,
            serialized: static (context, source) =>
                from lease in source.Source.Mint(document: context.Document, key: context.Op)
                from receipt in Transfer(
                    expected: source.Kind, lease: lease, document: context.Document,
                    parent: Option<(RenderContent, string)>.None, reason: context.Reason, op: context.Op)
                select receipt,
            material: static (context, source) =>
                from lease in source.Source.Mint(document: context.Document, key: context.Op)
                from receipt in Transfer(
                    expected: ContentKind.Material, lease: lease, document: context.Document,
                    parent: Option<(RenderContent, string)>.None, reason: context.Reason, op: context.Op)
                select receipt,
            texture: static (context, source) =>
                from lease in source.Source.Mint(document: context.Document, key: context.Op)
                from receipt in Transfer(
                    expected: ContentKind.Texture, lease: lease, document: context.Document,
                    parent: Option<(RenderContent, string)>.None, reason: context.Reason, op: context.Op)
                select receipt,
            environment: static (context, source) =>
                from lease in source.State.Mint(document: context.Document, key: context.Op)
                from receipt in Transfer(
                    expected: ContentKind.Environment, lease: lease, document: context.Document,
                    parent: Option<(RenderContent, string)>.None, reason: context.Reason, op: context.Op)
                select receipt);

    private static Fin<ContentReceipt> Transfer(
        ContentKind expected, Lease<RenderContent> lease, RhinoDoc document,
        Option<(RenderContent Content, string Slot)> parent, ChangeReason reason, Op op) {
        Fin<ContentReceipt> outcome =
            from actual in ContentKind.Of(lease.Resource).ToFin(op.InvalidResult())
            from _ in guard(actual == expected, op.InvalidInput())
            from __ in parent.Case switch {
                (RenderContent content, string slot) => ChangeScope.Write(
                    content: content, reason: reason, key: op,
                    body: live => op.Catch(() => op.Confirm(success: live.SetChild(lease.Resource, slot)))),
                _ => op.Catch(() => op.Confirm(success: expected.Attach(document: document, content: lease.Resource))),
            }
            select ContentReceipt.Content(slot: ContentSlot.Minted, id: lease.Resource.Id)
                + ContentReceipt.Content(slot: ContentSlot.Adopted, id: lease.Resource.Id);
        return outcome.Match(
            Succ: static receipt => Fin.Succ(value: receipt),
            Fail: error => { lease.Dispose(); return Fin.Fail<ContentReceipt>(error: error); });
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TreeMutation {
    private TreeMutation() { }
    public sealed record Graft(string Slot, ContentRef Child, ChangeReason Reason) : TreeMutation;
    public sealed record Prune(Option<string> Slot, ChangeReason Reason) : TreeMutation;
    public sealed record Slot(string Name, Option<bool> On, Option<double> Amount, ChangeReason Reason) : TreeMutation;

    internal Fin<ContentReceipt> Apply(RenderContent parent, RhinoDoc document, Op op) =>
        Switch(
            state: (Parent: parent, Document: document, Op: op),
            graft: static (ctx, edit) =>
                from slot in ctx.Op.AcceptText(value: edit.Slot)
                from child in edit.Child.Resolve(document: ctx.Document, key: ctx.Op)
                from _ in ChangeScope.Write(content: ctx.Parent, reason: edit.Reason, key: ctx.Op,
                    body: live => ctx.Op.Catch(() => ctx.Op.Confirm(success: live.SetChild(renderContent: child, childSlotName: slot))))
                select ContentReceipt.Content(slot: ContentSlot.Grafted, id: ctx.Parent.Id),
            prune: static (ctx, edit) =>
                from slot in edit.Slot.Traverse(value => ctx.Op.AcceptText(value: value)).As()
                from _ in ChangeScope.Write(content: ctx.Parent, reason: edit.Reason, key: ctx.Op,
                    body: live => slot.Case switch {
                        string name => ctx.Op.Catch(() => ctx.Op.Confirm(success: live.DeleteChild(name, edit.Reason.Native))),
                        _ => ctx.Op.Catch(() => { live.DeleteAllChildren(edit.Reason.Native); return Fin.Succ(value: unit); }),
                    })
                select ContentReceipt.Content(slot: ContentSlot.Pruned, id: ctx.Parent.Id),
            slot: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from _ in guard(edit.On.IsSome || edit.Amount.IsSome, ctx.Op.InvalidInput())
                from __ in ChangeScope.Write(content: ctx.Parent, reason: edit.Reason, key: ctx.Op, body: live => ctx.Op.Catch(() => {
                    _ = edit.On.Iter(on => live.SetChildSlotOn(name, on, edit.Reason.Native));
                    _ = edit.Amount.Iter(amount => live.SetChildSlotAmount(name, amount, edit.Reason.Native));
                    return Fin.Succ(value: unit);
                }))
                select ContentReceipt.Content(slot: ContentSlot.SlotSet, id: ctx.Parent.Id));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Grouping {
    private Grouping() { }
    public sealed record Make : Grouping;
    public sealed record Ungroup : Grouping;
    public sealed record Recursive : Grouping;
    public sealed record Smart : Grouping;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentMutation {
    private ContentMutation() { }
    public sealed record Detach : ContentMutation;
    public sealed record Rename(string Name, ChangeReason Reason, bool EnsureUnique = true) : ContentMutation;
    public sealed record Tree(TreeMutation Edit) : ContentMutation;
    public sealed record Field(string Name, FieldValue Value, ChangeReason Reason) : ContentMutation;
    public sealed record Param(
        ParamScope Scope, FieldValue Value, ChangeReason Reason,
        RenderContent.ExtraRequirementsSetContexts Context) : ContentMutation;
    public sealed record Texture(TextureConfig Config, ChangeReason Reason) : ContentMutation;
    public sealed record Assign(TableTarget Objects, RenderMaterial.AssignToSubFaceChoices SubFaces, RenderMaterial.AssignToBlockChoices Blocks) : ContentMutation;
    public sealed record Replace(ContentIo Source) : ContentMutation;
    public sealed record Group(Grouping Mode) : ContentMutation;
    public sealed record Export(ContentExport Output) : ContentMutation;

    internal bool RecordsUndo => this is not Export;

    internal Fin<ContentReceipt> Apply(RenderContent content, RhinoDoc document, Op op) =>
        Switch(
            context: (Content: content, Document: document, Op: op),
            detach: static (ctx, _) =>
                from kind in ContentKind.Of(ctx.Content).ToFin(ctx.Op.InvalidResult())
                from _ in ctx.Op.Catch(() => ctx.Op.Confirm(success: kind.Detach(ctx.Document, ctx.Content)))
                select ContentReceipt.Content(slot: ContentSlot.Detached, id: ctx.Content.Id),
            rename: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from _ in ChangeScope.Write(ctx.Content, edit.Reason, live => ctx.Op.Catch(() => {
                    live.SetName(name, renameEvents: true, ensureNameUnique: edit.EnsureUnique);
                    return Fin.Succ(value: unit);
                }), ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.Renamed, id: ctx.Content.Id),
            tree: static (ctx, edit) => edit.Edit.Apply(parent: ctx.Content, document: ctx.Document, op: ctx.Op),
            field: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from _ in ChangeScope.Write(ctx.Content, edit.Reason, live => edit.Value.Write(live.Fields, name, ctx.Op), ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.FieldSet, id: ctx.Content.Id),
            param: static (ctx, edit) =>
                from _ in edit.Scope.Write(ctx.Content, edit.Value, edit.Reason, edit.Context, ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.FieldSet, id: ctx.Content.Id),
            texture: static (ctx, edit) =>
                from texture in Optional(ctx.Content as RenderTexture).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in edit.Config.Apply(texture, edit.Reason, ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.Configured, id: ctx.Content.Id),
            assign: static (ctx, edit) =>
                from material in Optional(ctx.Content as RenderMaterial).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in edit.Objects.Resolve(document: ctx.Document, key: ctx.Op)
                from _ in ctx.Op.Catch(() => {
                    Arr<ObjRef> references = toArr(ids.Map(id => new ObjRef(ctx.Document, id)));
                    try {
                        return ctx.Op.Confirm(success: material.AssignTo(references, edit.SubFaces, edit.Blocks, bInteractive: false));
                    } finally {
                        references.Iter(static reference => reference.Dispose());
                    }
                })
                select ContentReceipt.Objects(slot: ContentSlot.Assigned, ids: ids),
            replace: static (ctx, edit) =>
                from lease in edit.Source.Mint(document: ctx.Document, key: ctx.Op)
                from receipt in ReplaceWith(target: ctx.Content, lease: lease, op: ctx.Op)
                select receipt,
            group: static (ctx, edit) => edit.Mode.Switch(
                state: (Content: ctx.Content, Op: ctx.Op),
                make: static (state, _) => state.Op.Catch(() => Optional(state.Content.MakeGroupInstance())
                    .ToFin(Fail: state.Op.InvalidResult())
                    .Map(grouped => ContentReceipt.Content(slot: ContentSlot.Grouped, id: grouped.Id))),
                ungroup: static (state, _) => state.Op.Catch(() => state.Op.Confirm(success: state.Content.Ungroup()))
                    .Map(_ => ContentReceipt.Content(slot: ContentSlot.Ungrouped, id: state.Content.Id)),
                recursive: static (state, _) => state.Op.Catch(() => state.Op.Confirm(success: state.Content.UngroupRecursive()))
                    .Map(_ => ContentReceipt.Content(slot: ContentSlot.Ungrouped, id: state.Content.Id)),
                smart: static (state, _) => state.Op.Catch(() => state.Op.Confirm(success: state.Content.SmartUngroupRecursive()))
                    .Map(_ => ContentReceipt.Content(slot: ContentSlot.Ungrouped, id: state.Content.Id))),
            export: static (ctx, edit) => edit.Output.Switch(
                state: (Content: ctx.Content, Op: ctx.Op),
                archive: static (state, output) =>
                    from path in state.Op.AcceptText(value: output.Path)
                    from _ in state.Op.Catch(() => state.Op.Confirm(success: state.Content.SaveToFile(path, output.Embed)))
                    select ContentReceipt.Path(slot: ContentSlot.Exported, path: path),
                textureImage: static (state, output) =>
                    from texture in Optional(state.Content as RenderTexture).ToFin(Fail: state.Op.InvalidInput())
                    from path in state.Op.AcceptText(value: output.Path)
                    from _ in TextureExport.Export(
                        texture: texture, path: path, width: output.Width, height: output.Height, depth: output.Depth, key: state.Op)
                    select ContentReceipt.Path(slot: ContentSlot.Exported, path: path)));

    private static Fin<ContentReceipt> ReplaceWith(RenderContent target, Lease<RenderContent> lease, Op op) {
        Fin<ContentReceipt> outcome =
            from targetKind in ContentKind.Of(target).ToFin(op.InvalidResult())
            from replacementKind in ContentKind.Of(lease.Resource).ToFin(op.InvalidResult())
            from _ in guard(targetKind == replacementKind, op.InvalidInput())
            from __ in op.Catch(() => op.Confirm(success: target.Replace(newcontent: lease.Resource)))
            select ContentReceipt.Content(slot: ContentSlot.Swapped, id: lease.Resource.Id);
        return outcome.Match(
            Succ: static receipt => Fin.Succ(value: receipt),
            Fail: error => { lease.Dispose(); return Fin.Fail<ContentReceipt>(error: error); });
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentExport {
    private ContentExport() { }
    public sealed record Archive(string Path, RenderContent.EmbedFilesChoice Embed) : ContentExport;
    public sealed record TextureImage(string Path, int Width, int Height, int Depth) : ContentExport;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentOp {
    private ContentOp() { }
    public sealed record Admit(ContentAdmission Source) : ContentOp;
    public sealed record Mutate(ContentRef Target, ContentMutation Change) : ContentOp;

    internal bool RecordsUndo => Switch(
        admit: static _ => true,
        mutate: static edit => edit.Change.RecordsUndo);

    internal Fin<ContentReceipt> Apply(RhinoDoc document, ContentKind scope, ChangeReason reason, Op op) =>
        Switch(
            context: (Document: document, Scope: scope, Reason: reason, Op: op),
            admit: static (ctx, edit) =>
                from _ in guard(edit.Source.Expected == ctx.Scope, ctx.Op.InvalidInput())
                from receipt in edit.Source.Apply(document: ctx.Document, reason: ctx.Reason, op: ctx.Op)
                select receipt,
            mutate: static (ctx, edit) =>
                from content in edit.Target.Resolve(document: ctx.Document, key: ctx.Op)
                from kind in ContentKind.Of(content).ToFin(ctx.Op.InvalidResult())
                from _ in guard(kind == ctx.Scope, ctx.Op.InvalidInput())
                from receipt in edit.Change.Apply(content: content, document: ctx.Document, op: ctx.Op)
                select receipt);
}
```

## [04]-[COMMIT_AND_QUERY]

- Owner: `ContentTransaction` — the commit plan: name, verified kind under change, operation sequence, table-scope reason, redraw policy, undo grant. `ContentQuery<T>` — one typed read program constrained to marker-carrying results, so every query answer crosses the session's `Demand` seam by construction. `Contents` — commit, query, roster, factory census, current-environment census, and registration entries.
- Law: the spine is the one bracket owner — the whole mutation runs inside one `Demand` window, the undo record opens through the document `UndoBracket` only when the plan records, the plan's kind opens its table change scope around the fold and closes it on every exit, redraw suppression restores prior state, and the bracket's `Seal` rolls a failed owned record back before the fault leaves.
- Law: grants are proven per plan shape against one snapshot — `Mutate` always, `Undo` when the plan records, `Redraw` when the plan redraws — and the session is the only document ingress; the redraw vocabulary is the document `RedrawPolicy` rows, shared with the table and block rails.
- Law: reads never open an undo record — `Query`, `Roster`, and `CurrentEnvironments` demand `SessionNeed.Read` alone; every result carries `IDetachedDocumentResult` as a detached fact or a self-disposing capsule (`ContentIcon` owns its bitmap lease), and the baked-material and PBR reads are borrow windows whose live material dies inside the query.
- Law: the workflow-corrected hash resolves the document's own `LinearWorkflow` inside the query window off the probe's `DocumentWorkflow` posture; a live sub-owner never enters or leaves a query value.
- Boundary: the current-environment triple reads through `RhinoDoc.CurrentEnvironment`; the settings-side per-usage binding is the settings page's edit rail, and the two never merge.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ContentTransaction(
    string Name,
    ContentKind Kind,
    Seq<ContentOp> Operations,
    ChangeReason Reason,
    RedrawPolicy Redraw,
    bool UndoRecorded = true) {
    public static ContentTransaction Batch(string name, ContentKind kind, ChangeReason reason, params ReadOnlySpan<ContentOp> operations) =>
        new(Name: name, Kind: kind, Operations: toSeq(operations.ToArray()), Reason: reason, Redraw: RedrawPolicy.Deferred);
}

public sealed record EnvironmentBindings(
    Option<Guid> Background,
    Option<Guid> Reflection,
    Option<Guid> Lighting) : IDetachedDocumentResult;

public sealed record ContentArchive(string Xml, Seq<string> EmbeddedFiles) : IDetachedDocumentResult;

public sealed record ContentRoster(ContentKind Kind, Seq<Guid> Ids) : IDetachedDocumentResult;

public readonly record struct MatchEvidence(RenderContent.MatchDataResult Verdict) : IDetachedDocumentResult;

public sealed record ContentIcon(Lease<System.Drawing.Bitmap> Image) : IDetachedDocumentResult, IDisposable {
    public void Dispose() => Image.Dispose();
}

public sealed class ContentQuery<T> where T : IDetachedDocumentResult {
    private readonly Func<RhinoDoc, RenderContent, Op, Fin<T>> read;

    internal ContentQuery(Func<RhinoDoc, RenderContent, Op, Fin<T>> read) => this.read = read;

    internal Fin<T> Run(RhinoDoc document, RenderContent content, Op op) => read(document, content, op);
}

public static class ContentQuery {
    public static ContentQuery<ContentSnapshot> Snapshot { get; } =
        new(read: static (_, content, op) => ContentSnapshot.Of(content: content, key: op));

    public static ContentQuery<ContentArchive> Archive { get; } =
        new(read: static (_, content, op) =>
            from xml in op.AcceptText(value: content.Xml)
            from embedded in op.Catch(() => Fin.Succ(value: toSeq(content.GetEmbeddedFilesList())))
            select new ContentArchive(Xml: xml, EmbeddedFiles: embedded));

    public static ContentQuery<FieldCensus> Fields { get; } =
        new(read: static (_, content, op) => FieldCensus.Of(fields: content.Fields, key: op));

    public static ContentQuery<ScentCensus> Scents { get; } =
        Material(static (material, _) => Fin.Succ(value: MaterialScent.CensusOf(material: material)));

    public static ContentQuery<TextureConfig> Config { get; } =
        Texture(static (texture, op) => TextureConfig.Of(texture: texture, key: op));

    public static ContentQuery<TextureTraits> Traits { get; } =
        Texture(static (texture, op) => TextureTraits.Of(texture: texture, key: op));

    public static ContentQuery<HashWitness> Hash(HashProbe probe) =>
        new(read: (document, content, op) =>
            (probe.DocumentWorkflow
                ? probe.Read(content: content, workflow: document.RenderSettings.LinearWorkflow, key: op)
                : probe.Read(content: content, key: op))
            .Map(value => new HashWitness(
                Flags: probe.Flags, Excluded: probe.ExcludedParameters,
                DocumentWorkflow: probe.DocumentWorkflow, Value: value)));

    public static ContentQuery<FieldValue> Param(ParamScope scope) =>
        new(read: (_, content, op) => scope.Read(content: content, key: op));

    public static ContentQuery<SlotUsage> Usage(RenderMaterial.StandardChildSlots slot) =>
        Material((material, op) => MaterialBridge.Usage(material: material, slot: slot, key: op));

    public static ContentQuery<TOut> Bake<TOut>(
        RenderTexture.TextureGeneration generation, Func<Material, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        Material((material, op) => MaterialBridge.Bake(
            material: material, generation: generation, borrow: borrow, key: op));

    public static ContentQuery<TOut> Pbr<TOut>(
        RenderTexture.TextureGeneration generation, Func<PhysicallyBasedMaterial, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        Material((material, op) => MaterialBridge.Pbr(
            material: material, generation: generation, borrow: borrow, key: op));

    public static ContentQuery<EnvironmentState> Environment(bool dataOnly) =>
        Environment((environment, op) => EnvironmentState.Bake(environment: environment, isForDataOnly: dataOnly, key: op));

    public static ContentQuery<ContentIcon> Icon(Size2i extent) =>
        new(read: (_, content, op) => op.Catch(() =>
            content.Icon(extent.Native, out System.Drawing.Bitmap rendered) && rendered is not null
                ? Fin.Succ(value: new ContentIcon(Image: new Lease<System.Drawing.Bitmap>.Owned(Value: rendered)))
                : Fin.Fail<ContentIcon>(error: op.InvalidResult())));

    public static ContentQuery<MatchEvidence> Match(ContentRef old) =>
        new(read: (document, content, op) =>
            from prior in old.Resolve(document: document, key: op)
            from verdict in op.Catch(() => Fin.Succ(value: new MatchEvidence(Verdict: content.MatchData(oldContent: prior))))
            select verdict);

    private static ContentQuery<TOut> Material<TOut>(Func<RenderMaterial, Op, Fin<TOut>> project)
        where TOut : IDetachedDocumentResult =>
        new(read: (_, content, op) => Optional(content as RenderMaterial).ToFin(Fail: op.InvalidInput()).Bind(material => project(material, op)));

    private static ContentQuery<TOut> Texture<TOut>(Func<RenderTexture, Op, Fin<TOut>> project)
        where TOut : IDetachedDocumentResult =>
        new(read: (_, content, op) => Optional(content as RenderTexture).ToFin(Fail: op.InvalidInput()).Bind(texture => project(texture, op)));

    private static ContentQuery<TOut> Environment<TOut>(Func<RenderEnvironment, Op, Fin<TOut>> project)
        where TOut : IDetachedDocumentResult =>
        new(read: (_, content, op) => Optional(content as RenderEnvironment).ToFin(Fail: op.InvalidInput()).Bind(environment => project(environment, op)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Contents {
    public static Fin<Seq<Type>> Register(Assembly assembly, Guid pluginId) {
        Op op = Op.Of();
        return op.Catch(() => Optional(RenderContent.RegisterContent(assembly: assembly, pluginId: pluginId))
            .ToFin(Fail: op.InvalidResult())
            .Map(static registered => toSeq(registered)));
    }

    public static Fin<ContentReceipt> Commit(DocumentSession session, ContentTransaction plan) {
        Op op = Op.Of();
        return from active in Optional(plan).ToFin(Fail: op.InvalidInput())
               from name in op.AcceptText(value: active.Name)
               from _ in guard(!active.Operations.IsEmpty, op.InvalidInput())
               let recording = active.UndoRecorded && active.Operations.Exists(static operation => operation.RecordsUndo)
               let needs = Seq(SessionNeed.Mutate)
                   + (recording ? Seq(SessionNeed.Undo) : Seq<SessionNeed>())
                   + (active.Redraw.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>())
               from receipt in session.Demand(
                   use: document => Run(document: document, plan: active, name: name, recording: recording, op: op),
                   key: op,
                   needs: needs.ToArray())
               select receipt;
    }

    private static Fin<ContentReceipt> Run(RhinoDoc document, ContentTransaction plan, string name, bool recording, Op op) =>
        op.Catch(() => {
            bool priorRedraw = document.Views.RedrawEnabled;
            bool opened = false;
            Fin<ContentReceipt> outcome;
            try {
                _ = Op.SideWhen(plan.Redraw.Suppress, () =>
                    document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false));
                plan.Kind.Open(document: document, reason: plan.Reason.Native);
                opened = true;
                outcome = op.Catch(() => {
                    using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recording);
                    Fin<ContentReceipt> folded = guard(undo.Admitted, op.InvalidResult()).ToFin()
                        .Bind(_ => plan.Operations
                            .TraverseM(operation => operation.Apply(
                                document: document, scope: plan.Kind, reason: plan.Reason, op: op)).As()
                            .Map(static receipts => receipts.Fold(ContentReceipt.Empty, static (state, value) => state + value)));
                    return undo.Seal(
                        outcome: folded,
                        stamp: static (receipt, serial) => serial > 0u ? receipt + ContentReceipt.UndoRecords(serials: Seq(serial)) : receipt,
                        key: op);
                });
            } finally {
                try {
                    _ = Op.SideWhen(opened, () => plan.Kind.Close(document: document));
                } finally {
                    _ = Op.SideWhen(plan.Redraw.Suppress, () =>
                        document.Views.EnableRedraw(enable: priorRedraw, redrawDocument: false, redrawLayers: false));
                }
            }
            _ = Op.SideWhen(plan.Redraw.Enabled && outcome.IsSucc, () => document.Views.Redraw(deferred: plan.Redraw.Defers));
            return outcome;
        });

    public static Fin<T> Query<T>(DocumentSession session, ContentRef target, ContentQuery<T> query) {
        Op op = Op.Of();
        return from active in Optional(query).ToFin(Fail: op.InvalidInput())
               from result in session.Demand(
                   use: document => target.Resolve(document: document, key: op)
                       .Bind(content => active.Run(document: document, content: content, op: op)),
                   key: op,
                   needs: [SessionNeed.Read])
               select result;
    }

    public static Fin<ContentRoster> Roster(DocumentSession session, ContentKind kind) {
        Op op = Op.Of();
        return session.Demand(
            use: document => op.Catch(() => Fin.Succ(value: new ContentRoster(
                Kind: kind, Ids: kind.Roster(document).Map(static content => content.Id)))),
            key: op,
            needs: [SessionNeed.Read]);
    }

    public static Fin<Seq<ContentTypeInfo>> Types() => ContentTypeInfo.Census(key: Op.Of());

    public static Fin<EnvironmentBindings> CurrentEnvironments(DocumentSession session) {
        Op op = Op.Of();
        return session.Demand(
            use: document => op.Catch(() => {
                ICurrentEnvironment current = document.CurrentEnvironment;
                return Fin.Succ(value: new EnvironmentBindings(
                    Background: Optional(current.ForBackground).Map(static content => content.Id),
                    Reflection: Optional(current.ForReflectionAndRefraction).Map(static content => content.Id),
                    Lighting: Optional(current.ForLighting).Map(static content => content.Id)));
            }),
            key: op,
            needs: [SessionNeed.Read]);
    }
}
```

## [05]-[RECEIPTS]

- Owner: `ContentSlot` `[SmartEnum<int>]` — the consequence vocabulary; `ContentBody` — the payload union; `ContentReceipt` — the additive fold over the fact stream with slot-keyed projections, the same fact-stream form the document table and block rails carry.
- Law: one fact stream, kind-discriminated — content ids, assigned object ids, export paths, and undo serials are `ContentBody` cases on one record; every projection is a `Choose` over the stream, and a new consequence class is one slot row or one body case; the mapping page's channel bind stamps its `Mapped` facts onto this same receipt.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ContentSlot {
    public static readonly ContentSlot Minted = new(key: 0);
    public static readonly ContentSlot Adopted = new(key: 1);
    public static readonly ContentSlot Detached = new(key: 2);
    public static readonly ContentSlot Renamed = new(key: 3);
    public static readonly ContentSlot Grafted = new(key: 4);
    public static readonly ContentSlot Pruned = new(key: 5);
    public static readonly ContentSlot SlotSet = new(key: 6);
    public static readonly ContentSlot FieldSet = new(key: 7);
    public static readonly ContentSlot Configured = new(key: 8);
    public static readonly ContentSlot Assigned = new(key: 9);
    public static readonly ContentSlot Swapped = new(key: 10);
    public static readonly ContentSlot Grouped = new(key: 11);
    public static readonly ContentSlot Ungrouped = new(key: 12);
    public static readonly ContentSlot Exported = new(key: 13);
    public static readonly ContentSlot Undo = new(key: 14);
    public static readonly ContentSlot Mapped = new(key: 15);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentBody {
    private ContentBody() { }
    public sealed record Content(Guid Id) : ContentBody;
    public sealed record Object(Guid Id) : ContentBody;
    public sealed record Path(string Value) : ContentBody;
    public sealed record Record(uint Serial) : ContentBody;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ContentFactRow(ContentSlot Slot, ContentBody Body);

public readonly record struct ContentReceipt : IDetachedDocumentResult {
    private readonly Seq<ContentFactRow> facts;

    private ContentReceipt(Seq<ContentFactRow> facts) => this.facts = facts;

    public static ContentReceipt Empty { get; } = new(facts: Seq<ContentFactRow>());

    public Seq<ContentFactRow> Facts => facts;

    public static ContentReceipt operator +(ContentReceipt left, ContentReceipt right) =>
        new(facts: left.facts + right.facts);

    public static ContentReceipt Content(ContentSlot slot, Guid id) =>
        new(facts: Seq(new ContentFactRow(Slot: slot, Body: new ContentBody.Content(Id: id))));

    public static ContentReceipt Objects(ContentSlot slot, Seq<Guid> ids) =>
        new(facts: ids.Distinct().Filter(static id => id != Guid.Empty)
            .Map(id => new ContentFactRow(Slot: slot, Body: new ContentBody.Object(Id: id))));

    public static ContentReceipt Path(ContentSlot slot, string path) =>
        new(facts: Seq(new ContentFactRow(Slot: slot, Body: new ContentBody.Path(Value: path))));

    public static ContentReceipt UndoRecords(Seq<uint> serials) =>
        new(facts: serials.Filter(static serial => serial > 0u)
            .Map(serial => new ContentFactRow(Slot: ContentSlot.Undo, Body: new ContentBody.Record(Serial: serial))));

    public Seq<Guid> Contents(ContentSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is ContentBody.Content body ? Some(body.Id) : Option<Guid>.None);

    public Seq<Guid> Ids(ContentSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is ContentBody.Object body ? Some(body.Id) : Option<Guid>.None);

    public int FactCount(ContentSlot slot) =>
        facts.Count(fact => fact.Slot == slot);
}
```

## [06]-[EVENTS]

- Owner: `ContentPulse` `[SmartEnum<int>]` — the eleven static `RenderContent` events as rows, each carrying one typed bind delegate through the document `Subscription` capsule. `ContentSignal` — detached evidence: lifecycle id and reason, changed prior id and context, field name, environment usage, or preview bitmap lease plus quality and signature dimensions. `ContentFact` carries pulse, optional `DocKey`, and signal. `ContentStream` owns transactional attach, document gating, and symmetric release.
- Law: every reference-like host member projects inside the callback — content becomes its guid, the document becomes `DocKey`, the preview bitmap clones into an owned lease; a live `RenderContent` never rides a fact.
- Law: the stream and the table family split by granularity — the Document events page's `RenderContent` payload reports table transitions and material assignment; this stream reports per-content lifecycle, change context, and field mutation the table family cannot; a consumer needing both composes two watches.
- Law: reason filtering occurs at the bind — `PulseFilter` drops changed and field facts whose reason the filter names; filtering never claims debounce or coalescing semantics the host event stream does not provide.
- Growth: a new host content event is one `ContentPulse` row with its bind column; a new evidence axis is one `ContentSignal` case.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentSignal {
    private ContentSignal() { }
    public sealed record Lifecycle(Guid Content, RenderContentChangeReason Reason) : ContentSignal;
    public sealed record Changed(Guid Content, ChangeReason Reason, Option<Guid> Old) : ContentSignal;
    public sealed record FieldChanged(Guid Content, string Field, ChangeReason Reason) : ContentSignal;
    public sealed record EnvironmentFlip(RenderSettings.EnvironmentUsage Usage) : ContentSignal;
    public sealed record PreviewReady(
        Lease<System.Drawing.Bitmap> Image,
        Option<(int Width, int Height)> Signature,
        Rhino.Render.Utilities.PreviewQuality Quality) : ContentSignal;
}

public readonly record struct ContentFact(ContentPulse Pulse, Option<DocKey> Key, ContentSignal Signal);

public sealed record PulseFilter(Seq<ChangeReason> DroppedReasons) {
    public static readonly PulseFilter None = new(DroppedReasons: Seq<ChangeReason>());
    public static readonly PulseFilter WithoutRealTimeUi = new(DroppedReasons: Seq(ChangeReason.RealTimeUi));

    internal bool Admits(Option<ChangeReason> reason) =>
        reason.Map(row => !DroppedReasons.Contains(row)).IfNone(true);
}

[SmartEnum<int>]
public sealed partial class ContentPulse {
    public static readonly ContentPulse Added = new(key: 0, bind: Plain(
        subscribe: static h => RenderContent.ContentAdded += h, unsubscribe: static h => RenderContent.ContentAdded -= h));
    public static readonly ContentPulse Renamed = new(key: 1, bind: Plain(
        subscribe: static h => RenderContent.ContentRenamed += h, unsubscribe: static h => RenderContent.ContentRenamed -= h));
    public static readonly ContentPulse Deleting = new(key: 2, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleting += h, unsubscribe: static h => RenderContent.ContentDeleting -= h));
    public static readonly ContentPulse Deleted = new(key: 3, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleted += h, unsubscribe: static h => RenderContent.ContentDeleted -= h));
    public static readonly ContentPulse Replacing = new(key: 4, bind: Plain(
        subscribe: static h => RenderContent.ContentReplacing += h, unsubscribe: static h => RenderContent.ContentReplacing -= h));
    public static readonly ContentPulse Replaced = new(key: 5, bind: Plain(
        subscribe: static h => RenderContent.ContentReplaced += h, unsubscribe: static h => RenderContent.ContentReplaced -= h));
    public static readonly ContentPulse UpdatePreview = new(key: 6, bind: Plain(
        subscribe: static h => RenderContent.ContentUpdatePreview += h, unsubscribe: static h => RenderContent.ContentUpdatePreview -= h));
    public static readonly ContentPulse EnvironmentFlip = new(key: 7, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentEventArgs>>(
            subscribe: static h => RenderContent.CurrentEnvironmentChanged += h,
            unsubscribe: static h => RenderContent.CurrentEnvironmentChanged -= h,
            handler: (_, args) => ignore(Gate(pulse: pulse, scope: scope, document: args.Document,
                signal: new ContentSignal.EnvironmentFlip(Usage: args.EnvironmentUsageEx)).Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse Changed = new(key: 8, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentChanged += h,
            unsubscribe: static h => RenderContent.ContentChanged -= h,
            handler: (_, args) => ignore(
                ChangeReason.Of(native: args.ChangeContext, key: Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                    .Bind(reason => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.Changed(
                            Content: args.Content.Id, Reason: reason,
                            Old: Optional(args.OldContent).Map(static old => old.Id))))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse FieldChanged = new(key: 9, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentFieldChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentFieldChanged += h,
            unsubscribe: static h => RenderContent.ContentFieldChanged -= h,
            handler: (_, args) => ignore(
                ChangeReason.Of(native: args.ChangeContext, key: Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                    .Bind(reason => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.FieldChanged(Content: args.Content.Id, Field: args.FieldName, Reason: reason)))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse PreviewReady = new(key: 10, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<PreviewRenderedEventArgs>>(
            subscribe: static h => RenderContent.PreviewRendered += h,
            unsubscribe: static h => RenderContent.PreviewRendered -= h,
            handler: (_, args) => ignore(scope is EventScope.AnyDocument
                ? Optional(args.Bitmap)
                    .Map(image => new ContentFact(
                        Pulse: pulse,
                        Key: Option<DocKey>.None,
                        Signal: new ContentSignal.PreviewReady(
                            Image: new Lease<System.Drawing.Bitmap>.Owned(Value: (System.Drawing.Bitmap)image.Clone()),
                            Signature: Optional(args.PreviewJobSignature)
                                .Map(static signature => (signature.ImageWidth(), signature.ImageHeight())),
                            Quality: args.Quality)))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit))
                : Fin.Succ(value: unit))));

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(
        ContentPulse pulse, EventScope scope, PulseFilter filter, Func<ContentFact, Fin<Unit>> deliver);

    private static Func<ContentPulse, EventScope, PulseFilter, Func<ContentFact, Fin<Unit>>, Fin<Subscription>> Plain(
        Action<EventHandler<RenderContentEventArgs>> subscribe,
        Action<EventHandler<RenderContentEventArgs>> unsubscribe) =>
        (pulse, scope, _, deliver) => Subscription.Attach(
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            handler: (EventHandler<RenderContentEventArgs>)((_, args) => ignore(
                Gate(pulse: pulse, scope: scope, document: args.Document,
                    signal: new ContentSignal.Lifecycle(Content: args.Content.Id, Reason: args.Reason))
                .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));

    private static Option<ContentFact> Gate(ContentPulse pulse, EventScope scope, RhinoDoc? document, ContentSignal signal) =>
        Optional(document)
            .Bind(static active => DocKey.Of(document: active, key: Op.Of(name: nameof(ContentPulse))).ToOption())
            .Match(
                Some: key => scope.Switch(
                    state: (Key: key, Pulse: pulse, Signal: signal),
                    document: static (state, watched) => watched.Key == state.Key
                        ? Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))
                        : Option<ContentFact>.None,
                    anyDocument: static (state, _) => Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))),
                None: () => scope is EventScope.AnyDocument
                    ? Some(new ContentFact(Pulse: pulse, Key: Option<DocKey>.None, Signal: signal))
                    : Option<ContentFact>.None);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ContentStream : IDisposable {
    private Subscription? subscription;

    private ContentStream(Subscription subscription) => this.subscription = subscription;

    public void Dispose() {
        Subscription? captured = Interlocked.Exchange(location1: ref subscription, value: null);
        captured?.Dispose();
    }

    public static Fin<ContentStream> Of(
        EventScope scope, Seq<ContentPulse> pulses, Func<ContentFact, Fin<Unit>> sink, PulseFilter? filter = null) {
        Op op = Op.Of(name: nameof(ContentStream));
        PulseFilter active = filter ?? PulseFilter.WithoutRealTimeUi;
        return from _ in guard(!pulses.IsEmpty, op.InvalidInput())
               from attached in Subscription.AttachAll(pulses.Distinct().Map(pulse =>
                   (Func<Fin<Subscription>>)(() => pulse.Bind(pulse: pulse, scope: scope, filter: active, deliver: sink))))
               select new ContentStream(subscription: attached);
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]             | [FORM]                                                    | [ENTRY]                          |
| :-----: | :--------------- | :------------------ | :-------------------------------------------------------- | :------------------------------- |
|  [01]   | well-known ids   | `ContentUuids`      | host vocabulary consumed directly                         | factory admission                |
|  [02]   | factory census   | `ContentTypeInfo`   | exception-safe projection over disposable descriptors     | `Contents.Types`                 |
|  [03]   | custom format    | `ContentSerializer` | host serializer base over one typed program               | plug-in registration             |
|  [04]   | mutation regimes | `ContentOp`         | admission or one-target mutation                          | `Contents.Commit`                |
|  [05]   | typed reads      | `ContentQuery<T>`   | marker-carrying result-correlated query programs          | `Contents.Query<T>`              |
|  [06]   | commit spine     | `Contents`          | undo bracket + table change scope + redraw + receipt fold | `Commit(session, plan)`          |
|  [07]   | receipts         | `ContentReceipt`    | `ContentFactRow` stream + slot projections                | `Contents` / `Ids` / `FactCount` |
|  [08]   | content events   | `ContentPulse`      | eleven bind rows over the static event roster             | `ContentStream.Of`               |
|  [09]   | event evidence   | `ContentSignal`     | detached closed family, full preview payload              | `ContentStream.Of`               |
