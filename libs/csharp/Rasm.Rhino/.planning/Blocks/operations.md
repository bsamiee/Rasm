# [RASM_RHINO_BLOCK_OPERATIONS]

The instance-definition operation rail (`Rasm.Rhino.Blocks`). ONE `BlockOp` union carries every block mutation the host admits — authoring with conflict policy, whole-metadata amendment, geometry replacement, the four linked-source transitions, delete/undelete/purge/purge-unused/compact/export, instance placement, instance repointing, and exploded-piece baking — committed through one `Blocks.Commit` spine that proves the plan-derived session grants inside one `Demand` window, brackets one undo record through the document `UndoBracket`, suppresses and restores redraw, and folds every consequence into one `BlockReceipt` fact stream; the census-era 19-case `BlockOp` with its `BlockAttributeTask`/`BlockInstanceTask`/`LinkLifecycle`/`TableMutation` parallel unions, its per-feature planners, its `RunScript` fallback path, and the source-path relativity verb (the host member is obsolete and unconditionally answers false) are dead. Reads ride the same polymorphic discipline on one `BlockAsk`/`BlockAnswer` pair — state snapshot, preview render, attribute-field extraction, name minting, instance explosion — so a consumer submits one request vocabulary and never orchestrates host call sequences. Geometry ingress composes the document kernel lattice: every raw source admits through `GeometryIntake` onto a lease, and definition geometry never enters unvalidated.

## [01]-[INDEX]

- [02]-[OPERATION_FAMILY]: the `BlockOp` union — the whole mutation verb roster with its total dispatch.
- [03]-[READ_FAMILY]: `FieldSource`, `ExplodedPiece`, and the `BlockAsk`/`BlockAnswer` read rail.
- [04]-[COMMIT_SPINE]: `BlockTransaction` and the `Blocks` entry pair with the undo bracket and redraw law.
- [05]-[RECEIPTS]: `BlockSlot`, `BlockFact`, and the `BlockReceipt` monoid.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[OPERATION_FAMILY]

- Owner: `BlockOp` `[Union]` — one flat verb family over the whole verified mutation surface; every case resolves its definition through `BlockRef`, executes its host member, and answers a `BlockReceipt` fragment on the rail.
- Law: authoring internalizes the conflict decision — `Author` probes the name, and `ConflictPolicy` selects refuse, reuse, or host-minted rename inside the arm, so no caller pre-probes the table; the hyperlink payload selects the metadata-bearing `Add` overload from its presence.
- Law: `Amend` carries the whole metadata target — the host `Modify` member writes name, description, url, and url tag together, so the case is total state, never a patch whose absent fields silently clear.
- Law: linked-source transitions are four verbs on one rail — `Rebind` through `ModifySourceArchive`, `Sever` through `DestroySourceArchive`, `Refresh` through `RefreshLinkedBlock`, `Retarget` through `UpdateLinkedInstanceDefinition` — each answering the definition fact it changed; the census-era `RunScript` block-update path is dead because every transition has a verified table member, and path relativity is `Rebind` payload (`RelativePath` selects the relative-path `FileReference` mint) because the host's standalone relativity member is obsolete and inert.
- Law: geometry ingress is the kernel lattice — `Author` and `Regeometry` admit every raw source through `GeometryIntake.Admit`, hold the leases across the one host call, and dispose them on every exit, so an inadmissible source fails typed before the table is touched.
- Law: instance work addresses through the document vocabulary — `Repoint` resolves its instances through `TableTarget`, `Place` validates every `Placement` at construction, and `Bake` demands a live `InstanceObject`; a raw guid loop at a call site is the deleted form.
- Growth: a new host block verb is one case with its arm; the commit spine, the receipt, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockOp {
    private BlockOp() { }
    public sealed record Author(
        string Name,
        Option<string> Description,
        Option<(string Url, string Tag)> Hyperlink,
        Point3d BasePoint,
        Seq<object> Sources,
        Seq<ObjectAttributes> Attributes,
        ConflictPolicy Conflict) : BlockOp;
    public sealed record Amend(BlockRef Target, string Name, string Description, string Url, string UrlTag, bool Quiet = true) : BlockOp;
    public sealed record Regeometry(BlockRef Target, Seq<object> Sources, Seq<ObjectAttributes> Attributes) : BlockOp;
    public sealed record Rebind(BlockRef Target, string Path, Option<string> RelativePath, InstanceDefinitionUpdateType UpdateType, bool Quiet = true) : BlockOp;
    public sealed record Sever(BlockRef Target, bool Quiet = true) : BlockOp;
    public sealed record Refresh(BlockRef Target) : BlockOp;
    public sealed record Retarget(BlockRef Target, string Filename, bool NestedLinks = true, bool Quiet = true) : BlockOp;
    public sealed record Delete(BlockRef Target, DeletionPolicy Policy) : BlockOp;
    public sealed record Undelete(BlockRef Target) : BlockOp;
    public sealed record Purge(BlockRef Target) : BlockOp;
    public sealed record PurgeUnused : BlockOp;
    public sealed record Compact(bool IgnoreUndoReferences = true) : BlockOp;
    public sealed record Export(BlockRef Target, string Path) : BlockOp;
    public sealed record Place(BlockRef Target, Seq<Placement> Instances) : BlockOp;
    public sealed record Repoint(TableTarget Instances, BlockRef Target) : BlockOp;
    public sealed record Bake(Guid InstanceId, bool Nested = false, bool DeleteInstance = true) : BlockOp;

    internal bool RecordsUndo => this is not Export;

    internal bool RequiresContext => this is Author or Regeometry;

    internal Fin<BlockReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            (Document: document, Domain: domain, Op: op),
            author: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from resolved in Optional(context.Document.InstanceDefinitions.Find(name)).Case switch {
                    InstanceDefinition existing => edit.Conflict.Switch(
                        state: (Existing: existing, Document: context.Document, Name: name, Op: context.Op),
                        fail: static held => Fin.Fail<(string Name, Option<int> Reused)>(error: held.Op.InvalidInput()),
                        reuse: static held => Fin.Succ(value: (held.Name, Some(held.Existing.Index))),
                        mint: static held => held.Op.AcceptText(
                            value: held.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: held.Name))
                            .Map(minted => (minted, Option<int>.None))),
                    _ => Fin.Succ(value: (name, Option<int>.None)),
                }
                from receipt in resolved.Reused.Case switch {
                    int reused => Fin.Succ(value: BlockReceipt.Definition(slot: BlockSlot.Authored, index: reused)),
                    _ => Admitted(sources: edit.Sources, domain: context.Domain, op: context.Op, run: natives =>
                        from index in context.Op.Catch(() => {
                            int added = edit.Hyperlink.Case switch {
                                (string url, string tag) => context.Document.InstanceDefinitions.Add(
                                    name: resolved.Name, description: edit.Description.IfNone(string.Empty), url: url, urlTag: tag,
                                    basePoint: edit.BasePoint, geometry: natives, attributes: edit.Attributes.AsIterable()),
                                _ => context.Document.InstanceDefinitions.Add(
                                    name: resolved.Name, description: edit.Description.IfNone(string.Empty),
                                    basePoint: edit.BasePoint, geometry: natives, attributes: edit.Attributes.AsIterable()),
                            };
                            return added >= 0 ? Fin.Succ(value: added) : Fin.Fail<int>(error: context.Op.InvalidResult());
                        })
                        select BlockReceipt.Definition(slot: BlockSlot.Authored, index: index)),
                }
                select receipt,
            amend: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Modify(
                    idefIndex: definition.Index, newName: edit.Name, newDescription: edit.Description,
                    newUrl: edit.Url, newUrlTag: edit.UrlTag, quiet: edit.Quiet))
                select BlockReceipt.Definition(slot: BlockSlot.Amended, index: definition.Index),
            regeometry: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from receipt in Admitted(sources: edit.Sources, domain: context.Domain, op: context.Op, run: natives =>
                    from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifyGeometry(
                        idefIndex: definition.Index, newGeometry: natives, newAttributes: edit.Attributes.AsIterable()))
                    select BlockReceipt.Definition(slot: BlockSlot.Regeometried, index: definition.Index))
                select receipt,
            rebind: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from path in context.Op.AcceptText(value: edit.Path)
                from _ in context.Op.Catch(() => {
                    using FileReference reference = edit.RelativePath.Case switch {
                        string relative => FileReference.CreateFromFullAndRelativePaths(fullPath: path, relativePath: relative),
                        _ => FileReference.CreateFromFullPath(fullPath: path),
                    };
                    return context.Op.Confirm(success: context.Document.InstanceDefinitions.ModifySourceArchive(
                        idefIndex: definition.Index, sourceArchive: reference, updateType: edit.UpdateType, quiet: edit.Quiet));
                })
                select BlockReceipt.Definition(slot: BlockSlot.Rebound, index: definition.Index),
            sever: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.DestroySourceArchive(
                    definition: definition, quiet: edit.Quiet))
                select BlockReceipt.Definition(slot: BlockSlot.Severed, index: definition.Index),
            refresh: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.RefreshLinkedBlock(definition: definition))
                select BlockReceipt.Definition(slot: BlockSlot.Refreshed, index: definition.Index),
            retarget: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from path in context.Op.AcceptText(value: edit.Filename)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.UpdateLinkedInstanceDefinition(
                    idefIndex: definition.Index, filename: path, updateNestedLinks: edit.NestedLinks, quiet: edit.Quiet))
                select BlockReceipt.Definition(slot: BlockSlot.Retargeted, index: definition.Index),
            delete: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Delete(
                    idefIndex: definition.Index, deleteReferences: edit.Policy.DeleteReferences, quiet: edit.Policy.Quiet))
                select BlockReceipt.Definition(slot: BlockSlot.Deleted, index: definition.Index),
            undelete: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Undelete(idefIndex: definition.Index))
                select BlockReceipt.Definition(slot: BlockSlot.Revived, index: definition.Index),
            purge: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Purge(idefIndex: definition.Index))
                select BlockReceipt.Definition(slot: BlockSlot.Purged, index: definition.Index),
            purgeUnused: static (context, _) =>
                from tally in context.Op.Catch(() => Fin.Succ(value: context.Document.InstanceDefinitions.PurgeUnused()))
                select BlockReceipt.Tally(slot: BlockSlot.Reclaimed, count: tally),
            compact: static (context, edit) =>
                from _ in context.Op.Catch(() => {
                    context.Document.InstanceDefinitions.Compact(ignoreUndoReferences: edit.IgnoreUndoReferences);
                    return Fin.Succ(value: unit);
                })
                select BlockReceipt.Tally(slot: BlockSlot.Compacted, count: 1),
            export: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from path in context.Op.AcceptText(value: edit.Path)
                from _ in context.Op.Confirm(success: context.Document.InstanceDefinitions.Export(
                    idefIndex: definition.Index, filename: path))
                select BlockReceipt.Path(slot: BlockSlot.Exported, path: path),
            place: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from _ in guard(!edit.Instances.IsEmpty, context.Op.InvalidInput())
                from placed in edit.Instances.TraverseM(placement => context.Op.Catch(() => {
                    Guid id = context.Document.Objects.AddInstanceObject(
                        instanceDefinitionIndex: definition.Index,
                        instanceXform: placement.Motion,
                        attributes: placement.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                        history: placement.History.IfNoneUnsafe((HistoryRecord?)null),
                        reference: placement.Reference);
                    return id != Guid.Empty ? Fin.Succ(value: id) : Fin.Fail<Guid>(error: context.Op.InvalidResult());
                })).As()
                select BlockReceipt.Objects(slot: BlockSlot.Placed, ids: placed),
            repoint: static (context, edit) =>
                from definition in edit.Target.Resolve(document: context.Document, key: context.Op)
                from ids in edit.Instances.Resolve(document: context.Document, key: context.Op)
                from repointed in ids.TraverseM(id => context.Op
                    .Confirm(success: context.Document.Objects.ReplaceInstanceObject(objectId: id, instanceDefinitionIndex: definition.Index))
                    .Map(_ => id)).As()
                select BlockReceipt.Objects(slot: BlockSlot.Repointed, ids: repointed),
            bake: static (context, edit) =>
                from native in Optional(context.Document.Objects.FindId(edit.InstanceId)).ToFin(Fail: context.Op.MissingContext())
                from instance in Optional(native as InstanceObject).ToFin(Fail: context.Op.InvalidInput())
                from ids in context.Op.Catch(() => Optional(context.Document.Objects.AddExplodedInstancePieces(
                        instance: instance, explodeNestedInstances: edit.Nested, deleteInstance: edit.DeleteInstance))
                    .Map(static pieces => toSeq(pieces))
                    .ToFin(Fail: context.Op.InvalidResult()))
                select BlockReceipt.Objects(slot: BlockSlot.Baked, ids: ids));

    private static Fin<BlockReceipt> Admitted(
        Seq<object> sources, Option<Context> domain, Op op, Func<Iterable<GeometryBase>, Fin<BlockReceipt>> run) =>
        from _ in guard(!sources.IsEmpty, op.InvalidInput())
        from active in domain.ToFin(Fail: op.MissingContext())
        from leases in Leased(sources: sources, domain: active, op: op)
        from receipt in Fin.Succ(value: unit).Bind(_ => {
            Fin<BlockReceipt> inner = op.Catch(() => run(leases.Map(static lease => lease.Resource).AsIterable()));
            _ = leases.Iter(static lease => lease.Dispose());
            return inner;
        })
        select receipt;

    private static Fin<Seq<Lease<GeometryBase>>> Leased(Seq<object> sources, Context domain, Op op) =>
        sources.Fold(Fin.Succ(value: Seq<Lease<GeometryBase>>()), (state, source) => state.Bind(held =>
            GeometryIntake.Admit(source: source, domain: domain, key: op).Match(
                Succ: lease => Fin.Succ(value: held.Add(value: lease)),
                Fail: error => {
                    _ = held.Iter(static prior => prior.Dispose());
                    return Fin.Fail<Seq<Lease<GeometryBase>>>(error: error);
                })));
}
```

## [03]-[READ_FAMILY]

- Owner: `FieldSource` `[Union]` — the three verified attribute-field extraction sources: raw text-field string, live text object, definition; `ExplodedPiece` — one exploded product: leased detached geometry, attributes, composed transform; `BlockAsk` `[Union]` — the read requests: state snapshot, preview render, field extraction, name minting, instance explosion; `BlockAnswer` `[Union]` — the typed results keyed by request case.
- Law: reads never open an undo record — `Blocks.Ask` demands `SessionNeed.Read` alone and touches no table mutation member, so a preview render or a field extraction inside a paused command never pollutes the undo stack.
- Law: exploded geometry crosses under custody — each piece's geometry detaches through `GeometryCrossing.Cross` onto its own `GeometryHandle` before the host objects fall out of scope, and the piece carries its composed transform so a consumer re-derives nothing from matrix state.
- Law: the preview answer is a lease, never a bitmap field — the owned `Lease` decides disposal, and the lifecycle page's vault is the one place a rendered preview is retained past the call.
- Law: `BlockAttributeText` is the compose direction of the field family — the token builder and the three extraction sources live on one owner, forward and inverse together.
- RESEARCH: the `InstanceAttributeField` member surface is unverified — the `Fields` answer carries the descriptor roster opaquely, and the typed field projection (key, prompt, default) lands after catalog verification.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldSource {
    private FieldSource() { }
    public sealed record OfText(string Value) : FieldSource;
    public sealed record OfObject(TextObject Value) : FieldSource;
    public sealed record OfDefinition(BlockRef Target) : FieldSource;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAsk {
    private BlockAsk() { }
    public sealed record State(BlockRef Target, ReferenceScope Scope) : BlockAsk;
    public sealed record Preview(BlockRef Target, PreviewSpec Spec) : BlockAsk;
    public sealed record Fields(FieldSource Source) : BlockAsk;
    public sealed record MintName(Option<string> Root) : BlockAsk;
    public sealed record Pieces(Guid InstanceId, ExplodePolicy Policy) : BlockAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockAnswer : IDetachedDocumentResult {
    private BlockAnswer() { }
    public sealed record State(BlockSnapshot Snapshot) : BlockAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Preview) : BlockAnswer;
    public sealed record Fields(Arr<InstanceAttributeField> Descriptors) : BlockAnswer;
    public sealed record Minted(string Name) : BlockAnswer;
    public sealed record Pieces(Seq<ExplodedPiece> Products) : BlockAnswer;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ExplodedPiece(GeometryHandle Geometry, ObjectAttributes Attributes, Transform Motion);
```

## [04]-[COMMIT_SPINE]

- Owner: `BlockTransaction` — the commit plan: name, operation sequence, redraw policy, undo grant; `Blocks` — the two entries: `Commit` the mutation spine, `Ask` the read dispatch.
- Law: the spine is the one bracket owner — the whole mutation runs inside one `Demand` window, the undo record opens through the document `UndoBracket` only when the plan records, redraw suppression restores the prior state on every exit, and the bracket's `Seal` rolls a failed owned record back and clears redo before the fault leaves; a block mutation outside `Blocks.Commit` has no undo, redraw, or receipt story and is the deleted form.
- Law: grants are proven per plan shape against one snapshot — `Mutate` always, `Undo` when the plan records, `Redraw` when the plan redraws — inside the one `Demand` call, and the session is the only document ingress, so the rail never sees a bare `RhinoDoc` from a consumer; the kernel context resolves inside the same window only when an operation's `RequiresContext` row demands it.
- Law: the deferred-redraw route is the document policy value — the plan reuses the document `RedrawPolicy` rows so a block commit and a table commit share one redraw vocabulary.
- Boundary: `TextFields` extraction, preview rendering, and explosion are `Ask` cases; the field-token composer `BlockAttributeText` is the one static compose member beside them.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BlockTransaction(
    string Name,
    Seq<BlockOp> Operations,
    RedrawPolicy Redraw,
    bool UndoRecorded = true) {
    public static BlockTransaction Batch(string name, params ReadOnlySpan<BlockOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Blocks {
    public static Fin<BlockReceipt> Commit(DocumentSession session, BlockTransaction plan) {
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

    private static Fin<BlockReceipt> Run(RhinoDoc document, BlockTransaction plan, string name, bool recording, Op op) =>
        from domain in plan.Operations.Exists(static operation => operation.RequiresContext)
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from receipt in op.Catch(() => {
            bool priorRedraw = document.Views.RedrawEnabled;
            Fin<Unit> suppressed = op.Catch(() => {
                _ = Op.SideWhen(plan.Redraw.Suppress, () =>
                    document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false));
                return Fin.Succ(value: unit);
            });
            Fin<BlockReceipt> outcome = suppressed.Bind(_ => op.Catch(() => {
                using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recording);
                Fin<BlockReceipt> folded = guard(undo.Admitted, op.InvalidResult()).ToFin()
                    .Bind(_ => plan.Operations
                        .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op)).As()
                        .Map(static receipts => receipts.Fold(BlockReceipt.Empty, static (state, value) => state + value)));
                return undo.Seal(outcome: folded.Map(static _ => TableReceipt.Empty), key: op)
                    .Bind(record => folded.Map(value => value + BlockReceipt.UndoRecords(serials: record.UndoRecords)));
            }));
            Fin<Unit> restored = op.Catch(() => {
                _ = Op.SideWhen(plan.Redraw.Suppress, () =>
                    document.Views.EnableRedraw(enable: priorRedraw, redrawDocument: false, redrawLayers: false));
                return Fin.Succ(value: unit);
            });
            return (outcome, restored).Apply(static (folded, _) => folded).As();
        })
        from _ in plan.Redraw.Enabled
            ? op.Catch(() => { document.Views.Redraw(deferred: plan.Redraw.Defers); return Fin.Succ(value: unit); })
            : Fin.Succ(value: unit)
        select receipt;

    public static Fin<BlockAnswer> Ask(DocumentSession session, BlockAsk request) {
        Op op = Op.Of();
        return from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in session.Demand(
                   use: document => active.Switch(
                       context: (Document: document, Op: op),
                       state: static (ctx, ask) =>
                           from definition in ask.Target.Resolve(document: ctx.Document, key: ctx.Op)
                           from snapshot in BlockSnapshot.Of(definition: definition, scope: ask.Scope, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.State(Snapshot: snapshot),
                       preview: static (ctx, ask) =>
                           from definition in ask.Target.Resolve(document: ctx.Document, key: ctx.Op)
                           from bitmap in ask.Spec.Render(definition: definition, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Rendered(
                               Preview: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
                       fields: static (ctx, ask) => ask.Source.Switch(
                           state: ctx,
                           ofText: static (inner, source) => inner.Op.Catch(() => Fin.Succ<BlockAnswer>(
                               value: new BlockAnswer.Fields(Descriptors: toArr(TextFields.GetInstanceAttributeFields(str: source.Value))))),
                           ofObject: static (inner, source) => inner.Op.Catch(() => Fin.Succ<BlockAnswer>(
                               value: new BlockAnswer.Fields(Descriptors: toArr(TextFields.GetInstanceAttributeFields(text: source.Value))))),
                           ofDefinition: static (inner, source) =>
                               from definition in source.Target.Resolve(document: inner.Document, key: inner.Op)
                               from descriptors in inner.Op.Catch(() => Fin.Succ(
                                   value: toArr(TextFields.GetInstanceAttributeFields(idef: definition))))
                               select (BlockAnswer)new BlockAnswer.Fields(Descriptors: descriptors)),
                       mintName: static (ctx, ask) =>
                           from minted in ctx.Op.AcceptText(value: ask.Root.Case switch {
                               string root => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(root: root),
                               _ => ctx.Document.InstanceDefinitions.GetUnusedInstanceDefinitionName(),
                           })
                           select (BlockAnswer)new BlockAnswer.Minted(Name: minted),
                       pieces: static (ctx, ask) =>
                           from native in Optional(ctx.Document.Objects.FindId(ask.InstanceId)).ToFin(Fail: ctx.Op.MissingContext())
                           from instance in Optional(native as InstanceObject).ToFin(Fail: ctx.Op.InvalidInput())
                           from products in Exploded(instance: instance, policy: ask.Policy, key: ctx.Op)
                           select (BlockAnswer)new BlockAnswer.Pieces(Products: products)),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    private static Fin<Seq<ExplodedPiece>> Exploded(InstanceObject instance, ExplodePolicy policy, Op key) =>
        key.Catch(() => {
            RhinoObject[] pieces;
            ObjectAttributes[] attributes;
            Transform[] motions;
            if (policy.SkipHiddenInViewport.Case is Guid viewport) {
                instance.Explode(
                    skipHiddenPieces: true, viewportId: viewport, explodeNestedInstances: policy.Nested,
                    pieces: out pieces, pieceAttributes: out attributes, pieceTransforms: out motions);
            } else {
                instance.Explode(
                    explodeNestedInstances: policy.Nested,
                    pieces: out pieces, pieceAttributes: out attributes, pieceTransforms: out motions);
            }
            return toSeq(pieces)
                .Zip(toSeq(attributes), static (piece, attribute) => (Piece: piece, Attribute: attribute))
                .Zip(toSeq(motions), static (pair, motion) => (pair.Piece, pair.Attribute, Motion: motion))
                .TraverseM(row =>
                    from geometry in Optional(row.Piece.Geometry).ToFin(Fail: key.InvalidResult())
                    from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
                    select new ExplodedPiece(Geometry: handle, Attributes: row.Attribute, Motion: row.Motion)).As();
        });
}
```

## [05]-[RECEIPTS]

- Owner: `BlockSlot` `[SmartEnum<int>]` — the consequence vocabulary; `BlockFact` — one fact record carrying a slot and a `BlockBody` payload union; `BlockReceipt` — the additive fold over the fact stream with slot-keyed projections, the same fact-stream form the document table rail carries.
- Law: one fact stream, kind-discriminated — definition indexes, placed and baked object ids, reclamation tallies, export paths, and undo serials are `BlockBody` cases on one record, never parallel receipt types; every projection is a `Choose` over the stream.
- Law: the receipt is the mutation's evidence — a consumer reads `Definitions(slot)`, `Ids(slot)`, and the tallies off the receipt instead of re-querying the table, and the graph and lifecycle pages key their invalidation on these facts.
- Growth: a new consequence class is one slot row or one body case; every projection gains it for free.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BlockSlot {
    public static readonly BlockSlot Authored = new(key: 0);
    public static readonly BlockSlot Amended = new(key: 1);
    public static readonly BlockSlot Regeometried = new(key: 2);
    public static readonly BlockSlot Rebound = new(key: 3);
    public static readonly BlockSlot Severed = new(key: 4);
    public static readonly BlockSlot Refreshed = new(key: 5);
    public static readonly BlockSlot Retargeted = new(key: 6);
    public static readonly BlockSlot Deleted = new(key: 7);
    public static readonly BlockSlot Revived = new(key: 8);
    public static readonly BlockSlot Purged = new(key: 9);
    public static readonly BlockSlot Reclaimed = new(key: 10);
    public static readonly BlockSlot Compacted = new(key: 11);
    public static readonly BlockSlot Exported = new(key: 12);
    public static readonly BlockSlot Placed = new(key: 13);
    public static readonly BlockSlot Repointed = new(key: 14);
    public static readonly BlockSlot Baked = new(key: 15);
    public static readonly BlockSlot Undo = new(key: 16);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockBody {
    private BlockBody() { }
    public sealed record Definition(int Index) : BlockBody;
    public sealed record Object(Guid Id) : BlockBody;
    public sealed record Tally(int Count) : BlockBody;
    public sealed record Path(string Value) : BlockBody;
    public sealed record Record(uint Serial) : BlockBody;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct BlockFact(BlockSlot Slot, BlockBody Body);

public readonly record struct BlockReceipt : IDetachedDocumentResult {
    private readonly Seq<BlockFact> facts;

    private BlockReceipt(Seq<BlockFact> facts) => this.facts = facts;

    public static BlockReceipt Empty { get; } = new(facts: Seq<BlockFact>());

    public Seq<BlockFact> Facts => facts;

    public static BlockReceipt operator +(BlockReceipt left, BlockReceipt right) =>
        new(facts: left.facts + right.facts);

    public static BlockReceipt Definition(BlockSlot slot, int index) =>
        new(facts: Seq(new BlockFact(Slot: slot, Body: new BlockBody.Definition(Index: index))));

    public static BlockReceipt Objects(BlockSlot slot, Seq<Guid> ids) =>
        new(facts: ids.Distinct().Filter(static id => id != Guid.Empty)
            .Map(id => new BlockFact(Slot: slot, Body: new BlockBody.Object(Id: id))));

    public static BlockReceipt Tally(BlockSlot slot, int count) =>
        new(facts: Seq(new BlockFact(Slot: slot, Body: new BlockBody.Tally(Count: count))));

    public static BlockReceipt Path(BlockSlot slot, string path) =>
        new(facts: Seq(new BlockFact(Slot: slot, Body: new BlockBody.Path(Value: path))));

    public static BlockReceipt UndoRecords(Seq<uint> serials) =>
        new(facts: serials.Filter(static serial => serial > 0u)
            .Map(serial => new BlockFact(Slot: BlockSlot.Undo, Body: new BlockBody.Record(Serial: serial))));

    public Seq<int> Definitions(BlockSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is BlockBody.Definition body ? Some(body.Index) : Option<int>.None);

    public Seq<Guid> Ids(BlockSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is BlockBody.Object body ? Some(body.Id) : Option<Guid>.None);

    public int Count(BlockSlot slot) =>
        facts.Count(fact => fact.Slot == slot);
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]            | [FORM]                                              | [ENTRY]                        |
| :-----: | :---------------- | :----------------- | :--------------------------------------------------- | :------------------------------ |
|  [01]   | mutation verbs    | `BlockOp`          | one flat `[Union]`, total generated dispatch          | `Blocks.Commit`                  |
|  [02]   | read requests     | `BlockAsk`         | one union, typed `BlockAnswer` per case               | `Blocks.Ask`                     |
|  [03]   | commit spine      | `Blocks`           | undo bracket + redraw suppression + receipt fold      | `Commit(session, plan)`          |
|  [04]   | exploded custody  | `ExplodedPiece`    | `GeometryHandle` detached geometry + composed transform | `BlockAsk.Pieces`                |
|  [05]   | consequence facts | `BlockReceipt`     | one `BlockFact` stream, slot-keyed projections        | `Definitions` / `Ids` / `Count`  |
