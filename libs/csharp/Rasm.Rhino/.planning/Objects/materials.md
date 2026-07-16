# [RASM_RHINO_OBJECTS_MATERIALS]

Object render support belongs to `Rasm.Rhino.Objects`. `MaterialScope` discriminates the material-resolution overload family; `MaterialAsk` reads detached material identity, component bindings, mapping-channel presence, cache state, per-object meshing policy, meshability, batch-harvested meshes, and provider parameters; `MaterialEdit` owns mapping, cache, meshing-policy, and provider-parameter writes. `Materials.Ask` and `Materials.Commit` are the two entries, and `Commit` separates undo-recorded programs from one-shot regenerable effects.

## [01]-[INDEX]

- [02]-[SCOPE_AND_STAMP]: `MaterialScope`, `MaterialStamp`, and the resolution law.
- [03]-[ASK_FAMILY]: `MaterialAsk`/`MaterialAnswer` — the read dispatch over materials, mappings, caches, policy, and the batch harvest.
- [04]-[EDIT_AND_COMMIT]: `MaterialSlot`, `MaterialEdit`, the receipt, and the `Materials` entry pair.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SCOPE_AND_STAMP]

- Owner: `MaterialScope` `[Union]` closes front, back, component, plug-in-keyed component, and hypothetical-attribute resolution. `MaterialStamp` carries detached resolved identity and name.
- Law: the scope discriminates the overload — each case selects exactly one host `GetMaterial`/`GetRenderMaterial` signature, so a caller states what it wants and never selects an overload; the realm (legacy `Material` versus `RenderMaterial`) is the ask case, never a flag on the scope.
- Law: resolution detaches — the resolved material projects to `MaterialStamp` inside the grant window, because a `Material` is table state addressed through the document rail and a `RenderMaterial` is render-content state owned by the render tables; a live material handle crossing this seam is the deleted form.
- Law: the per-component census is queried, never scanned — `HasSubobjectMaterials` gates `SubobjectMaterialComponents`, and stored per-plug-in rows install and retract through `AttributeEdit.FaceMaterials`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialScope {
    private MaterialScope() { }
    public sealed record Front : MaterialScope;
    public sealed record Back : MaterialScope;
    public sealed record Part(ComponentIndex Component) : MaterialScope;
    public sealed record PartFor(ComponentIndex Component, Guid PlugIn) : MaterialScope;
    public sealed record PartUnder(ComponentIndex Component, Guid PlugIn, ObjectAttributes Attributes) : MaterialScope;

    internal Fin<MaterialScope> Admit(Op key) =>
        Switch(
            state: key,
            front: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            back: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            part: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            partFor: static (op, scope) => guard(scope.PlugIn != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => (MaterialScope)scope),
            partUnder: static (op, scope) =>
                from _ in guard(scope.PlugIn != Guid.Empty, op.InvalidInput()).ToFin()
                from __ in Optional(scope.Attributes).ToFin(Fail: op.InvalidInput())
                select (MaterialScope)scope);

    internal Fin<MaterialStamp> Legacy(RhinoObject native, Op key) =>
        key.Catch(() => Optional(Switch(
                front: _ => native.GetMaterial(frontMaterial: true),
                back: _ => native.GetMaterial(frontMaterial: false),
                part: scope => native.GetMaterial(componentIndex: scope.Component),
                partFor: scope => native.GetMaterial(componentIndex: scope.Component, plugInId: scope.PlugIn),
                partUnder: scope => native.GetMaterial(
                    componentIndex: scope.Component, plugInId: scope.PlugIn, attributes: scope.Attributes)))
            .ToFin(Fail: key.InvalidResult())
            .Map(material => new MaterialStamp(
                Id: material.Id,
                Name: Optional(material.Name).Filter(static text => text.Length > 0))));

    internal Fin<MaterialStamp> Rendered(RhinoObject native, Op key) =>
        key.Catch(() => Optional(Switch(
                front: _ => native.GetRenderMaterial(frontMaterial: true),
                back: _ => native.GetRenderMaterial(frontMaterial: false),
                part: scope => native.GetRenderMaterial(componentIndex: scope.Component),
                partFor: scope => native.GetRenderMaterial(componentIndex: scope.Component, plugInId: scope.PlugIn),
                partUnder: scope => native.GetRenderMaterial(
                    componentIndex: scope.Component, plugInId: scope.PlugIn, attributes: scope.Attributes)))
            .ToFin(Fail: key.InvalidResult())
            .Map(content => new MaterialStamp(
                Id: content.Id,
                Name: Optional(content.Name).Filter(static text => text.Length > 0))));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MaterialStamp(Guid Id, Option<string> Name) : IDetachedDocumentResult;

public readonly record struct KnobStamp(System.TypeCode Kind, string Canonical) : IDetachedDocumentResult;
```

## [03]-[ASK_FAMILY]

- Owner: `MaterialAsk` `[Union]` closes material resolution, component bindings, mapping-channel presence, cache census, cached-mesh custody, per-object meshing policy, meshability, the batch harvest, and provider parameters. `MaterialAnswer` `[Union]` carries object identity on every plural row.
- Law: cache reads never build — `MeshCount` and `GetMeshes` answer the existing cache and `IsMeshable` answers capability, so a read inside a paused command allocates nothing; construction is the edit family's `BuildCache`, and `Harvest` alone runs the batch mesher.
- Law: cached meshes cross under custody — `GetMeshes` returns non-owning const wrappers parented to the live object, so each result detaches through `GeometryCrossing.Cross` onto its own handle before the grant closes; a consumer holding a parented cache mesh across a regen dereferences freed memory, and mutating one silently fails to persist.
- Law: meshing policy crosses owned — both `GetRenderMeshParameters` overloads mint a fresh caller-owned `MeshingParameters`, so the `CachePolicy` answer carries each object's policy as `Lease<MeshingParameters>.Owned` and the consumer disposes it; `DocumentFallback` selects the `returnDocumentParametersIfUnset` overload.
- Law: `Harvest` is the batch lane — one `RhinoObject.MeshObjects` call meshes the whole resolved roster, the host verdict folds through `CommandVerdict.OfNative`, and the paired mesh and attribute arrays prove equal cardinality before each caller-owned product detaches onto its own handle; per-object `BuildCache` loops re-deriving the batch member are the deleted form.
- Law: provider evidence preserves `IConvertible.GetTypeCode()` beside its invariant canonical value; bool, numeric, and string parameters never collapse to indistinguishable text.
- Boundary: mapping reads stop at `HasTextureMapping()` and `GetTextureChannels` — presence and roster. `GetTextureMapping` value programs belong to the render mapping owner, so a local `Mapping` answer carrying only channel and transform is a second owner of that seam and the deleted form.
- Growth: a new render-support read is one ask case with its answer case.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialAsk {
    private MaterialAsk() { }
    public sealed record Legacy(MaterialScope Scope) : MaterialAsk;
    public sealed record Rendered(MaterialScope Scope) : MaterialAsk;
    public sealed record PartCensus : MaterialAsk;
    public sealed record MappingRoster : MaterialAsk;
    public sealed record CacheCensus(MeshType Kind, MeshingParameters Parameters) : MaterialAsk;
    public sealed record CachedMeshes(MeshType Kind) : MaterialAsk;
    public sealed record CachePolicy(bool DocumentFallback = false) : MaterialAsk;
    public sealed record Meshable(MeshType Kind) : MaterialAsk;
    public sealed record Harvest(MeshingParameters Parameters) : MaterialAsk;
    public sealed record Knob(Guid Provider, string Name) : MaterialAsk;

    internal Fin<MaterialAsk> Admit(Op op) =>
        Switch(
            context: op,
            legacy: static (key, ask) =>
                from scope in Optional(ask.Scope).ToFin(Fail: key.InvalidInput())
                from admitted in scope.Admit(key: key)
                select (MaterialAsk)new Legacy(Scope: admitted),
            rendered: static (key, ask) =>
                from scope in Optional(ask.Scope).ToFin(Fail: key.InvalidInput())
                from admitted in scope.Admit(key: key)
                select (MaterialAsk)new Rendered(Scope: admitted),
            partCensus: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            mappingRoster: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            cacheCensus: static (key, ask) => Optional(ask.Parameters).ToFin(Fail: key.InvalidInput()).Map(_ => (MaterialAsk)ask),
            cachedMeshes: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            cachePolicy: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            meshable: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            harvest: static (key, ask) => Optional(ask.Parameters).ToFin(Fail: key.InvalidInput()).Map(_ => (MaterialAsk)ask),
            knob: static (key, ask) =>
                from _ in guard(ask.Provider != Guid.Empty, key.InvalidInput()).ToFin()
                from __ in key.AcceptText(value: ask.Name)
                select (MaterialAsk)ask);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialAnswer : IDetachedDocumentResult {
    private MaterialAnswer() { }
    public sealed record Stamped(Seq<(Guid Id, MaterialStamp Stamp)> Rows) : MaterialAnswer;
    public sealed record PartMaterials(Seq<(Guid Id, Seq<ComponentIndex> Components)> Rows) : MaterialAnswer;
    public sealed record Channels(Seq<(Guid Id, Seq<int> Values)> Rows) : MaterialAnswer;
    public sealed record Tally(Seq<(Guid Id, int Count)> Rows) : MaterialAnswer;
    public sealed record Able(Seq<(Guid Id, bool Verdict)> Rows) : MaterialAnswer;
    public sealed record Pieces(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : MaterialAnswer;
    public sealed record Policy(Seq<(Guid Id, Lease<MeshingParameters> Value)> Rows) : MaterialAnswer;
    public sealed record Harvested(Seq<ObjectPiece> Products) : MaterialAnswer;
    public sealed record KnobValue(Seq<(Guid Id, Option<KnobStamp> Value)> Rows) : MaterialAnswer;
}
```

## [04]-[EDIT_AND_COMMIT]

- Owner: `MaterialSlot` `[SmartEnum<int>]` — the consequence vocabulary; `MaterialEdit` `[Union]` — the mutations: `SetMapping` installs a channel's texture mapping with an optional object transform, `BuildCache` constructs meshes of one kind, `DropCache` destroys them, `SetCachePolicy` writes the per-object meshing parameters, `SetKnob` writes a provider parameter; `MaterialFact`/`MaterialReceipt` — the additive evidence stream; `Materials` — the two entries: `Ask`, `Commit`.
- Law: undo recording is a trait row. Recorded programs contain only `SetMapping` and `SetCachePolicy`; regenerable cache and provider effects run one at a time without an undo record. Admission rejects mixed programs, so rollback never promises to reverse an untracked side effect.
- Law: the commit spine is one grant window — resolution, the shared `UndoBracket`, the seal, and the post-success redraw all run inside one `Demand`, so no second window opens between the mutation and its repaint.
- Law: integer-returning writes preserve the host return — `SetTextureMapping` and `CreateMeshes` expose no catalogued verdict semantics, so receipts carry their values unchanged and invent no zero-or-sign success rule.
- Boundary: `HasCustomRenderMeshes`, `CustomRenderMeshesBoundingBox`, and the live `RenderMeshes` accessor demand a viewport, plug-in, and display-pipeline context this package does not own — they ride the Display and Render owners; this page's provider reach ends at the parameter knob.
- Growth: a new render-support mutation is one edit case with its trait and slot; the spine and the receipt read it with zero new surface.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MaterialSlot {
    public static readonly MaterialSlot Mapped = new(key: 0);
    public static readonly MaterialSlot CacheBuilt = new(key: 1);
    public static readonly MaterialSlot CacheDropped = new(key: 2);
    public static readonly MaterialSlot PolicySet = new(key: 3);
    public static readonly MaterialSlot KnobSet = new(key: 4);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialEdit {
    private MaterialEdit() { }
    public sealed record SetMapping(int Channel, TextureMapping Map, Option<Transform> Motion = default) : MaterialEdit;
    public sealed record BuildCache(MeshType Kind, MeshingParameters Parameters, bool IgnoreCustom = false) : MaterialEdit;
    public sealed record DropCache(MeshType Kind) : MaterialEdit;
    public sealed record SetCachePolicy(MeshingParameters Parameters) : MaterialEdit;
    public sealed record SetKnob(Guid Provider, string Name, IConvertible Value) : MaterialEdit;

    internal Fin<MaterialEdit> Admit(Op op) =>
        Switch(
            context: op,
            setMapping: static (key, edit) =>
                from _ in guard(edit.Channel >= 0, key.InvalidInput()).ToFin()
                from __ in Optional(edit.Map).ToFin(Fail: key.InvalidInput())
                select (MaterialEdit)edit,
            buildCache: static (key, edit) => Optional(edit.Parameters).ToFin(Fail: key.InvalidInput()).Map(_ => (MaterialEdit)edit),
            dropCache: static (_, edit) => Fin.Succ<MaterialEdit>(edit),
            setCachePolicy: static (key, edit) => Optional(edit.Parameters).ToFin(Fail: key.InvalidInput()).Map(_ => (MaterialEdit)edit),
            setKnob: static (key, edit) =>
                from _ in guard(edit.Provider != Guid.Empty, key.InvalidInput()).ToFin()
                from __ in key.AcceptText(value: edit.Name)
                from ___ in Optional(edit.Value).ToFin(Fail: key.InvalidInput())
                select (MaterialEdit)edit);

    internal bool RecordsUndo => this is SetMapping or SetCachePolicy;

    internal Fin<MaterialFact> Apply(RhinoObject native, Op op) =>
        Switch(
            context: (Native: native, Op: op),
            setMapping: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: edit.Motion.Case switch {
                    Transform motion => context.Native.SetTextureMapping(channel: edit.Channel, tm: edit.Map, objectTransform: motion),
                    _ => context.Native.SetTextureMapping(channel: edit.Channel, tm: edit.Map),
                }))
                .Map(native => new MaterialFact(Id: context.Native.Id, Slot: MaterialSlot.Mapped, Value: native)),
            buildCache: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: new MaterialFact(
                Id: context.Native.Id,
                Slot: MaterialSlot.CacheBuilt,
                Value: context.Native.CreateMeshes(
                    meshType: edit.Kind,
                    parameters: edit.Parameters,
                    ignoreCustomParameters: edit.IgnoreCustom)))),
            dropCache: static (context, edit) => context.Op.Catch(() => {
                context.Native.DestroyMeshes(meshType: edit.Kind);
                return Fin.Succ(value: new MaterialFact(Id: context.Native.Id, Slot: MaterialSlot.CacheDropped, Value: 0));
            }),
            setCachePolicy: static (context, edit) => context.Op
                .Confirm(success: context.Native.SetRenderMeshParameters(mp: edit.Parameters))
                .Map(_ => new MaterialFact(Id: context.Native.Id, Slot: MaterialSlot.PolicySet, Value: 1)),
            setKnob: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Catch(() => {
                    context.Native.SetCustomRenderMeshParameter(providerId: edit.Provider, parameterName: name, value: edit.Value);
                    return Fin.Succ(value: unit);
                })
                select new MaterialFact(Id: context.Native.Id, Slot: MaterialSlot.KnobSet, Value: 1));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MaterialFact(Guid Id, MaterialSlot Slot, int Value);

public readonly record struct MaterialReceipt(Seq<MaterialFact> Facts, Option<uint> UndoSerial = default) : IDetachedDocumentResult {
    public static MaterialReceipt operator +(MaterialReceipt left, MaterialReceipt right) =>
        new(
            Facts: left.Facts + right.Facts,
            UndoSerial: left.UndoSerial.IsSome ? left.UndoSerial : right.UndoSerial);

    public Seq<int> Values(MaterialSlot slot) =>
        Facts.Filter(fact => fact.Slot == slot).Map(static fact => fact.Value);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Materials {
    public static Fin<MaterialAnswer> Ask(DocumentSession session, TableTarget target, MaterialAsk ask) {
        Op op = Op.Of();
        return from active in Optional(ask).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           context: (Natives: natives, Op: op),
                           legacy: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ask.Scope.Legacy(native: native, key: ctx.Op)
                                   .Map(stamp => (native.Id, stamp))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Stamped(Rows: rows)),
                           rendered: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ask.Scope.Rendered(native: native, key: ctx.Op)
                                   .Map(stamp => (native.Id, stamp))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Stamped(Rows: rows)),
                           partCensus: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id, native.HasSubobjectMaterials
                                   ? toSeq(native.SubobjectMaterialComponents)
                                   : Seq<ComponentIndex>())))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.PartMaterials(Rows: rows)),
                           mappingRoster: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id, native.HasTextureMapping()
                                   ? toSeq(native.GetTextureChannels())
                                   : Seq<int>())))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Channels(Rows: rows)),
                           cacheCensus: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id, native.MeshCount(
                                   meshType: ask.Kind,
                                   parameters: ask.Parameters))))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Tally(Rows: rows)),
                           cachedMeshes: static (ctx, ask) => Cached(
                                   natives: ctx.Natives,
                                   kind: ask.Kind,
                                   key: ctx.Op)
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Pieces(Rows: rows)),
                           cachePolicy: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Optional(ask.DocumentFallback
                                           ? native.GetRenderMeshParameters(returnDocumentParametersIfUnset: true)
                                           : native.GetRenderMeshParameters())
                                       .ToFin(Fail: ctx.Op.InvalidResult())
                                       .Map(policy => (native.Id, (Lease<MeshingParameters>)new Lease<MeshingParameters>.Owned(Value: policy))))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Policy(Rows: rows)),
                           meshable: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Fin.Succ(value: (native.Id, native.IsMeshable(meshType: ask.Kind))))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Able(Rows: rows)),
                           harvest: static (ctx, ask) => ctx.Op.Catch(() => {
                               Result verdict = RhinoObject.MeshObjects(
                                   rhinoObjects: ctx.Natives.AsIterable(), parameters: ask.Parameters,
                                   meshes: out Mesh[] meshes, attributes: out ObjectAttributes[] attributes);
                               if (CommandVerdict.OfNative(result: verdict) != CommandVerdict.Completed) {
                                   Dispose(meshes: meshes, attributes: attributes);
                                   return Fin.Fail<MaterialAnswer>(error: ctx.Op.InvalidResult());
                               }
                               return Detach(meshes: meshes, attributes: attributes, key: ctx.Op)
                                   .Map(static products => (MaterialAnswer)new MaterialAnswer.Harvested(Products: products));
                           }),
                           knob: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id,
                                   Optional(native.GetCustomRenderMeshParameter(providerId: ask.Provider, parameterName: ask.Name))
                                       .Bind(value => Optional(System.Convert.ToString(
                                               value: value,
                                               provider: System.Globalization.CultureInfo.InvariantCulture))
                                           .Map(text => new KnobStamp(
                                               Kind: value.GetTypeCode(),
                                               Canonical: text))))))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.KnobValue(Rows: rows)))
                       select folded,
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<MaterialReceipt> Commit(
        DocumentSession session, TableTarget target, RedrawPolicy redraw, params ReadOnlySpan<MaterialEdit> edits) {
        Op op = Op.Of();
        return from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from requested in toSeq(edits.ToArray())
                   .TraverseM(edit => Optional(edit).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               from plan in requested.TraverseM(edit => edit.Admit(op: op)).As()
               let recording = plan.Exists(static edit => edit.RecordsUndo)
               from traits in guard(
                   plan.ForAll(edit => edit.RecordsUndo == recording)
                   && (recording || plan.Count is 1),
                   op.InvalidInput()).ToFin()
               let needs = Seq(SessionNeed.Mutate)
                   + (recording ? Seq(SessionNeed.Undo) : Seq<SessionNeed>())
                   + (policy.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>())
               from receipt in session.Demand(
                   use: document => op.Catch(() => {
                       using UndoBracket undo = UndoBracket.Begin(
                           document: document, name: nameof(Materials), recordsUndo: recording);
                       Fin<MaterialReceipt> folded = guard(undo.Admitted, op.InvalidResult()).ToFin()
                           .Bind(_ => Objects.Resolve(document: document, target: target, key: op))
                           .Bind(natives => natives.TraverseM(native => plan
                               .TraverseM(edit => edit.Apply(native: native, op: op)).As()).As()
                               .Map(static grouped => new MaterialReceipt(Facts: grouped.Bind(static facts => facts))));
                       Fin<MaterialReceipt> stamped = undo.Seal(
                           outcome: folded,
                           stamp: static (receipt, serial) => receipt with {
                               UndoSerial = serial > 0u ? Some(serial) : Option<uint>.None,
                           },
                           key: op);
                       return stamped.Bind(receipt => policy.Enabled
                           ? op.Catch(() => {
                               document.Views.Redraw(deferred: policy.Defers);
                               return Fin.Succ(value: receipt);
                           })
                           : Fin.Succ(value: receipt));
                   }),
                   key: op,
                   needs: needs.ToArray())
               select receipt;
    }

    private static Fin<Seq<ObjectPiece>> Detach(Mesh[]? meshes, ObjectAttributes[]? attributes, Op key) {
        Fin<Seq<ObjectPiece>> result =
            from shapes in Optional(meshes).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from paired in Optional(attributes).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from _ in guard(shapes.Count == paired.Count, key.InvalidResult()).ToFin()
            from pieces in shapes.Map((shape, index) => (Shape: shape, Index: index)).Fold(
                Fin.Succ(value: Seq<ObjectPiece>()),
                (state, row) => state.Bind(held =>
                    ObjectPiece.Detach(
                            geometry: row.Shape,
                            attributes: Some(paired[row.Index]),
                            key: key)
                    .Map(piece => held.Add(value: piece))
                    .MapFail(error => {
                        _ = held.Iter(static piece => piece.Dispose());
                        return error;
                    })))
            select pieces;
        Dispose(meshes: meshes, attributes: attributes);
        return result;
    }

    private static Fin<Seq<(Guid Id, Seq<ObjectPiece> Products)>> Cached(
        Seq<RhinoObject> natives, MeshType kind, Op key) =>
        natives.Fold(
            Fin.Succ(value: Seq<(Guid Id, Seq<ObjectPiece> Products)>()),
            (state, native) => state.Bind(held => key.Catch(() =>
                    Optional(native.GetMeshes(meshType: kind)).ToFin(Fail: key.InvalidResult())
                        .Bind(meshes => toSeq(meshes).Fold(
                            Fin.Succ(value: Seq<ObjectPiece>()),
                            (pieces, mesh) => pieces.Bind(detached => ObjectPiece.Detach(
                                    geometry: mesh,
                                    attributes: Option<ObjectAttributes>.None,
                                    key: key)
                                .Map(piece => detached.Add(value: piece))
                                .MapFail(error => {
                                    _ = detached.Iter(static prior => prior.Dispose());
                                    return error;
                                })))
                        .Map(products => (native.Id, products))))
                .Map(row => held.Add(value: row))
                .MapFail(error => {
                    _ = held.Iter(static row => row.Products.Iter(static piece => piece.Dispose()));
                    return error;
                })));

    private static void Dispose(Mesh[]? meshes, ObjectAttributes[]? attributes) {
        _ = Optional(meshes).Iter(static rows => {
            foreach (Mesh? mesh in rows) { mesh?.Dispose(); }
        });
        _ = Optional(attributes).Iter(static rows => {
            foreach (ObjectAttributes? metadata in rows) { metadata?.Dispose(); }
        });
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]             | [OWNER]           | [FORM]                                                 | [ENTRY]                       |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------------- | :---------------------------- |
|  [01]   | material resolution   | `MaterialScope`   | one address union discriminating the overload family   | `MaterialAsk.Legacy/Rendered` |
|  [02]   | detached identity     | `MaterialStamp`   | resolved id and name                                   | `MaterialAnswer.Stamped`      |
|  [03]   | render-support reads  | `MaterialAsk`     | one union over materials, channels, caches, and knobs  | `Materials.Ask`               |
|  [04]   | meshing policy read   | `MaterialAsk`     | owned `MeshingParameters` under `Lease` custody        | `MaterialAsk.CachePolicy`     |
|  [05]   | batch meshing         | `MaterialAsk`     | one host batch call onto detached `ObjectPiece` rows   | `MaterialAsk.Harvest`         |
|  [06]   | render-support writes | `MaterialEdit`    | trait-row undo recording under the shared bracket      | `Materials.Commit`            |
|  [07]   | consequence evidence  | `MaterialReceipt` | slot-keyed values plus optional undo serial            | `Values(slot)`                |
