# [RASM_RHINO_OBJECTS_MATERIALS]

Object render support belongs to `Rasm.Rhino.Objects`. `MaterialScope` discriminates the material-resolution overload family; `MaterialAsk` reads detached material identity, component bindings, mapping identity and transform, cache state, per-object meshing policy, meshability, policy-complete batch meshes, and provider parameters; `MaterialEdit` owns mapping, cache, meshing-policy, and provider-parameter writes. `Materials.Ask` and `Materials.Commit` are the two entries, and `Commit` separates undo-recorded programs from one-shot regenerable effects.

## [01]-[INDEX]

- [02]-[SCOPE_AND_STAMP]: `MaterialScope`, `MaterialStamp`, and the resolution law.
- [03]-[ASK_FAMILY]: `MaterialAsk`/`MaterialAnswer` — the read dispatch over materials, mappings, caches, policy, and the batch harvest.
- [04]-[EDIT_AND_COMMIT]: `MaterialEdit`, typed `MaterialFact`, the receipt, and the `Materials` entry pair.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SCOPE_AND_STAMP]

- Owner: `MaterialScope` `[Union]` closes front, back, component, plug-in-keyed component, and hypothetical-attribute resolution; `MaterialRealm` carries the legacy/render host-member family as four delegate columns — `Face`, `Part`, `Keyed`, `Under` — one row per material family; `PartUnder` carries an `AttributeProgram` that builds a scoped duplicate; `MaterialStamp` carries detached resolved identity and name.
- Law: scope and realm are independent discriminants — `MaterialAsk.Resolve(MaterialRealm, MaterialScope)` is the sole material question, one `MaterialScope.Resolve` dispatch selects the realm column per scope, and the realm row selects `Material` versus `RenderMaterial` without a second per-realm dispatch, sibling ask cases, or a boolean knob.
- Law: resolution detaches — the resolved material projects to `MaterialStamp` inside the grant window, because a `Material` is table state addressed through the document rail and a `RenderMaterial` is render-content state owned by the render tables; a live material handle crossing this seam is the deleted form.
- Law: the per-component census is queried, never scanned — `HasSubobjectMaterials` gates `SubobjectMaterialComponents`, and stored per-plug-in rows install and retract through `AttributeEdit.FaceMaterials`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Render;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class MaterialRealm {
    public static readonly MaterialRealm Legacy = new(
        face: static (native, front, key) => Stamp(native.GetMaterial(frontMaterial: front), key),
        part: static (native, component, key) => Stamp(native.GetMaterial(componentIndex: component), key),
        keyed: static (native, component, plugIn, key) => Stamp(native.GetMaterial(componentIndex: component, plugInId: plugIn), key),
        under: static (native, component, plugIn, attributes, key) => Stamp(native.GetMaterial(componentIndex: component, plugInId: plugIn, attributes: attributes), key));
    public static readonly MaterialRealm Rendered = new(
        face: static (native, front, key) => Stamp(native.GetRenderMaterial(frontMaterial: front), key),
        part: static (native, component, key) => Stamp(native.GetRenderMaterial(componentIndex: component), key),
        keyed: static (native, component, plugIn, key) => Stamp(native.GetRenderMaterial(componentIndex: component, plugInId: plugIn), key),
        under: static (native, component, plugIn, attributes, key) => Stamp(native.GetRenderMaterial(componentIndex: component, plugInId: plugIn, attributes: attributes), key));

    internal Func<RhinoObject, bool, Op, Fin<MaterialStamp>> Face { get; }
    internal Func<RhinoObject, ComponentIndex, Op, Fin<MaterialStamp>> Part { get; }
    internal Func<RhinoObject, ComponentIndex, Guid, Op, Fin<MaterialStamp>> Keyed { get; }
    internal Func<RhinoObject, ComponentIndex, Guid, ObjectAttributes, Op, Fin<MaterialStamp>> Under { get; }

    private static Fin<MaterialStamp> Stamp(Material? material, Op key) =>
        Optional(material).ToFin(Fail: key.InvalidResult()).Map(value => new MaterialStamp(
            Id: value.Id,
            Name: Optional(value.Name).Filter(static text => text.Length > 0)));

    private static Fin<MaterialStamp> Stamp(RenderMaterial? material, Op key) =>
        Optional(material).ToFin(Fail: key.InvalidResult()).Map(value => new MaterialStamp(
            Id: value.Id,
            Name: Optional(value.Name).Filter(static text => text.Length > 0)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialScope {
    private MaterialScope() { }
    public sealed record Front : MaterialScope;
    public sealed record Back : MaterialScope;
    public sealed record Part(ComponentIndex Component) : MaterialScope;
    public sealed record PartFor(ComponentIndex Component, Guid PlugIn) : MaterialScope;
    public sealed record PartUnder(ComponentIndex Component, Guid PlugIn, AttributeProgram Program) : MaterialScope;

    internal Fin<MaterialScope> Admit(Op key) =>
        Switch(
            key,
            front: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            back: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            part: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            partFor: static (op, scope) => guard(scope.PlugIn != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => (MaterialScope)scope),
            partUnder: static (op, scope) =>
                from _ in guard(scope.PlugIn != Guid.Empty, op.InvalidInput()).ToFin()
                from __ in op.Need(scope.Program)
                select (MaterialScope)scope);

    internal Fin<MaterialStamp> Resolve(MaterialRealm realm, RhinoObject native, Op key) =>
        Switch(
            (Realm: realm, Native: native, Op: key),
            front: static (ctx, _) => ctx.Op.Catch(() => ctx.Realm.Face(ctx.Native, true, ctx.Op)),
            back: static (ctx, _) => ctx.Op.Catch(() => ctx.Realm.Face(ctx.Native, false, ctx.Op)),
            part: static (ctx, scope) => ctx.Op.Catch(() => ctx.Realm.Part(ctx.Native, scope.Component, ctx.Op)),
            partFor: static (ctx, scope) => ctx.Op.Catch(() => ctx.Realm.Keyed(ctx.Native, scope.Component, scope.PlugIn, ctx.Op)),
            partUnder: static (ctx, scope) => ctx.Op.Catch(() => {
                using ObjectAttributes attributes = ctx.Native.Attributes.Duplicate();
                return scope.Program.Apply(attributes)
                    .Bind(_ => ctx.Realm.Under(ctx.Native, scope.Component, scope.PlugIn, attributes, ctx.Op));
            }));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MaterialStamp(Guid Id, Option<string> Name) : IDetachedDocumentResult;
public readonly record struct MappingStamp(int Channel, Guid Id, Transform ObjectTransform) : IDetachedDocumentResult;

[ValueObject<string>]
public sealed partial class MeshPolicy {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        using MeshingParameters? native = string.IsNullOrWhiteSpace(value)
            ? null
            : MeshingParameters.FromEncodedString(value);
        if (native is null) {
            validationError = new ValidationError(message: "meshing policy is invalid");
        } else {
            value = native.ToEncodedString();
        }
    }

    internal static Fin<MeshPolicy> Capture(MeshingParameters native, Op key) =>
        key.Catch(() => Create(native.ToEncodedString()).ToFin().MapFail(_ => key.InvalidResult()));

    internal Fin<T> Use<T>(Func<MeshingParameters, Fin<T>> body, Op key) =>
        key.Catch(() => {
            using MeshingParameters? native = MeshingParameters.FromEncodedString(ToValue());
            return native is null ? Fin.Fail<T>(key.InvalidResult()) : body(native);
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProviderValue : IDetachedDocumentResult {
    private ProviderValue() { }
    public sealed record Flag(bool Value) : ProviderValue;
    public sealed record Signed(long Value) : ProviderValue;
    public sealed record Unsigned(ulong Value) : ProviderValue;
    public sealed record Real(double Value) : ProviderValue;
    public sealed record Precise(decimal Value) : ProviderValue;
    public sealed record Text(string Value) : ProviderValue;

    internal IConvertible Native => Switch<IConvertible>(
        flag: static value => value.Value,
        signed: static value => value.Value,
        unsigned: static value => value.Value,
        real: static value => value.Value,
        precise: static value => value.Value,
        text: static value => value.Value);

    internal Fin<ProviderValue> Admit(Op key) =>
        Switch(
            key,
            flag: static (_, value) => Fin.Succ<ProviderValue>(value),
            signed: static (_, value) => Fin.Succ<ProviderValue>(value),
            unsigned: static (_, value) => Fin.Succ<ProviderValue>(value),
            real: static (op, value) => guard(double.IsFinite(value.Value), op.InvalidInput()).ToFin()
                .Map(_ => (ProviderValue)value),
            precise: static (_, value) => Fin.Succ<ProviderValue>(value),
            text: static (op, value) => op.AcceptText(value: value.Value)
                .Map(text => (ProviderValue)new Text(Value: text)));

    internal static Fin<ProviderValue> Of(IConvertible native, Op key) => (native switch {
        bool value => Fin.Succ<ProviderValue>(new Flag(value)),
        sbyte or short or int or long => Fin.Succ<ProviderValue>(new Signed(native.ToInt64(System.Globalization.CultureInfo.InvariantCulture))),
        byte or ushort or uint or ulong => Fin.Succ<ProviderValue>(new Unsigned(native.ToUInt64(System.Globalization.CultureInfo.InvariantCulture))),
        float or double => Fin.Succ<ProviderValue>(new Real(native.ToDouble(System.Globalization.CultureInfo.InvariantCulture))),
        decimal value => Fin.Succ<ProviderValue>(new Precise(value)),
        string value => Fin.Succ<ProviderValue>(new Text(value)),
        _ => Fin.Fail<ProviderValue>(key.InvalidResult(detail: native.GetTypeCode().ToString())),
    }).Bind(value => value.Admit(key));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshBatchFact : IDetachedDocumentResult {
    private MeshBatchFact() { }
    public sealed record Worker(ObjectSignal Enabled) : MeshBatchFact;
    public sealed record Dialog(ObjectSignal Simple) : MeshBatchFact;
    public sealed record Styled(int Style, Transform Motion) : MeshBatchFact;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshBatch {
    private MeshBatch() { }
    public sealed record Worker(MeshPolicy Policy, ObjectSignal Enabled) : MeshBatch;
    public sealed record Dialog(MeshPolicy Policy, ObjectSignal Simple) : MeshBatch;
    public sealed record Styled(MeshPolicy Policy, int Style, Transform Motion) : MeshBatch;

    internal Fin<MeshBatch> Admit(Op op) =>
        Switch(
            op,
            worker: static (key, batch) =>
                from policy in key.Need(batch.Policy)
                from enabled in key.Need(batch.Enabled)
                select (MeshBatch)new Worker(Policy: policy, Enabled: enabled),
            dialog: static (key, batch) =>
                from policy in key.Need(batch.Policy)
                from simple in key.Need(batch.Simple)
                select (MeshBatch)new Dialog(Policy: policy, Simple: simple),
            styled: static (key, batch) =>
                from policy in key.Need(batch.Policy)
                from motion in key.AcceptInput(value: batch.Motion)
                select (MeshBatch)new Styled(Policy: policy, Style: batch.Style, Motion: motion));

    internal Fin<(
        Result Verdict,
        Mesh[] Meshes,
        ObjectAttributes[] Attributes,
        MeshPolicy Policy,
        MeshBatchFact Mode)> Run(Seq<RhinoObject> natives, Op op) =>
        Switch(
            (Natives: natives, Op: op),
            worker: static (context, batch) => batch.Policy.Use(
                parameters => context.Op.Catch(() => {
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.Natives.AsIterable(),
                        parameters: parameters,
                        meshes: out Mesh[] meshes,
                        attributes: out ObjectAttributes[] attributes,
                        useWorkerThread: batch.Enabled.On);
                    return Capture(
                        verdict: verdict,
                        meshes: meshes,
                        attributes: attributes,
                        parameters: parameters,
                        mode: new MeshBatchFact.Worker(Enabled: batch.Enabled),
                        op: context.Op);
                }),
                context.Op),
            dialog: static (context, batch) => batch.Policy.Use(
                parameters => context.Op.Catch(() => {
                    bool simple = batch.Simple.On;
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.Natives.AsIterable(),
                        parameters: ref parameters,
                        simpleDialog: ref simple,
                        meshes: out Mesh[] meshes,
                        attributes: out ObjectAttributes[] attributes);
                    return Capture(
                        verdict: verdict,
                        meshes: meshes,
                        attributes: attributes,
                        parameters: parameters,
                        mode: new MeshBatchFact.Dialog(
                            Simple: simple ? ObjectSignal.Enabled : ObjectSignal.Disabled),
                        op: context.Op);
                }),
                context.Op),
            styled: static (context, batch) => batch.Policy.Use(
                parameters => context.Op.Catch(() => {
                    int style = batch.Style;
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.Natives.AsIterable(),
                        parameters: ref parameters,
                        uiStyle: ref style,
                        xform: batch.Motion,
                        meshes: out Mesh[] meshes,
                        attributes: out ObjectAttributes[] attributes);
                    return Capture(
                        verdict: verdict,
                        meshes: meshes,
                        attributes: attributes,
                        parameters: parameters,
                        mode: new MeshBatchFact.Styled(Style: style, Motion: batch.Motion),
                        op: context.Op);
                }),
                context.Op));

    private static Fin<(
        Result Verdict,
        Mesh[] Meshes,
        ObjectAttributes[] Attributes,
        MeshPolicy Policy,
        MeshBatchFact Mode)> Capture(
        Result verdict,
        Mesh[] meshes,
        ObjectAttributes[] attributes,
        MeshingParameters parameters,
        MeshBatchFact mode,
        Op op) =>
        MeshPolicy.Capture(parameters, op)
            .Map(policy => (verdict, meshes, attributes, policy, mode))
            .MapFail(error => {
                _ = ObjectPiece.Release(geometry: meshes, attributes: attributes);
                return error;
            });
}
```

## [03]-[ASK_FAMILY]

- Owner: `MaterialAsk` `[Union]` closes material resolution, component bindings, mapping identity and transform, cache census, cached-mesh custody, per-object meshing policy, meshability, the `MeshBatch` harvest family, and provider parameters. `MaterialAnswer` `[Union]` carries object identity on every plural row and owns every detached mesh until disposal.
- Law: cache reads never build — `MeshCount` and `GetMeshes` answer the existing cache and `IsMeshable` answers capability, so a read inside a paused command allocates nothing; construction is the edit family's `BuildCache`, and `Harvest` alone runs the batch mesher.
- Law: cached meshes cross under custody — `GetMeshes` returns non-owning const wrappers parented to the live object, so each result detaches through `GeometryCrossing.Cross` onto its own handle before the grant closes; a consumer holding a parented cache mesh across a regen dereferences freed memory, and mutating one silently fails to persist.
- Law: meshing policy crosses encoded — `MeshPolicy` captures `ToEncodedString()` while each caller-owned `MeshingParameters` is still scoped and reconstructs it with `FromEncodedString()` only for one host call; `ObjectSignal` selects document fallback without exporting a boolean policy.
- Law: `Harvest` is the batch lane — `MeshBatch` closes worker-thread, mutable simple-dialog, and mutable UI-style-plus-transform modalities over one resolved roster; ref-updated policy and interaction state return beside identity-bearing products, the host verdict folds through `CommandVerdict.OfNative`, and every failure before detachment releases both host arrays.
- Law: provider evidence is `ProviderValue` — bool, signed, unsigned, real, decimal, and text values remain distinct generated cases in both directions, and every constructed case re-enters the one `Admit` fold so a non-finite provider readback refuses exactly as a non-finite write does; arbitrary `IConvertible` values fail instead of type-erasing into text.
- Boundary: `MappingRoster` returns channel identity, mapping identity, and the object transform from `GetTextureMapping(channel, out Transform)`; construction, profile, inverse recovery, and evaluation remain `MappingSpec`/`Mappings.Run` responsibilities on the render mapping owner.
- Growth: a new render-support read is one ask case with its answer case.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialAsk {
    private MaterialAsk() { }
    public sealed record Resolve(MaterialRealm Realm, MaterialScope Scope) : MaterialAsk;
    public sealed record PartCensus : MaterialAsk;
    public sealed record MappingRoster : MaterialAsk;
    public sealed record CacheCensus(MeshType Kind, MeshPolicy Policy) : MaterialAsk;
    public sealed record CachedMeshes(MeshType Kind) : MaterialAsk;
    public sealed record CachePolicy(ObjectSignal DocumentFallback) : MaterialAsk;
    public sealed record Meshable(MeshType Kind) : MaterialAsk;
    public sealed record Harvest(MeshBatch Batch) : MaterialAsk;
    public sealed record Knob(Guid Provider, string Name) : MaterialAsk;

    internal Fin<MaterialAsk> Admit(Op op) =>
        Switch(
            op,
            resolve: static (key, ask) =>
                from realm in key.Need(ask.Realm)
                from scope in key.Need(ask.Scope)
                from admitted in scope.Admit(key: key)
                select (MaterialAsk)new Resolve(Realm: realm, Scope: admitted),
            partCensus: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            mappingRoster: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            cacheCensus: static (key, ask) => key.Need(ask.Policy)
                .Map(policy => (MaterialAsk)new CacheCensus(Kind: ask.Kind, Policy: policy)),
            cachedMeshes: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            cachePolicy: static (key, ask) => key.Need(ask.DocumentFallback).Map(_ => (MaterialAsk)ask),
            meshable: static (_, ask) => Fin.Succ<MaterialAsk>(ask),
            harvest: static (key, ask) => key.Need(ask.Batch)
                .Bind(batch => batch.Admit(key))
                .Map(batch => (MaterialAsk)new Harvest(Batch: batch)),
            knob: static (key, ask) =>
                from _ in guard(ask.Provider != Guid.Empty, key.InvalidInput()).ToFin()
                from name in key.AcceptText(value: ask.Name)
                select (MaterialAsk)new Knob(Provider: ask.Provider, Name: name));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialAnswer : IDetachedDocumentResult, IDisposable {
    private MaterialAnswer() { }
    public sealed record Stamped(Seq<(Guid Id, MaterialStamp Stamp)> Rows) : MaterialAnswer;
    public sealed record PartMaterials(Seq<(Guid Id, Seq<ComponentIndex> Components)> Rows) : MaterialAnswer;
    public sealed record Mappings(Seq<(Guid Id, Seq<MappingStamp> Values)> Rows) : MaterialAnswer;
    public sealed record Tally(Seq<(Guid Id, int Count)> Rows) : MaterialAnswer;
    public sealed record Able(Seq<(Guid Id, bool Verdict)> Rows) : MaterialAnswer;
    public sealed record Pieces(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : MaterialAnswer;
    public sealed record Policy(Seq<(Guid Id, Option<MeshPolicy> Value)> Rows) : MaterialAnswer;
    public sealed record Harvested(
        Seq<(Guid Id, ObjectPiece Product)> Rows,
        MeshPolicy Policy,
        MeshBatchFact Mode) : MaterialAnswer;
    public sealed record KnobValue(Seq<(Guid Id, Option<ProviderValue> Value)> Rows) : MaterialAnswer;

    public void Dispose() =>
        Switch(
            stamped: static _ => unit,
            partMaterials: static _ => unit,
            mappings: static _ => unit,
            tally: static _ => unit,
            able: static _ => unit,
            pieces: static answer => ObjectPiece.Release(answer.Rows),
            policy: static _ => unit,
            harvested: static answer => ObjectPiece.Release(answer.Rows),
            knobValue: static _ => unit);
}
```

## [04]-[EDIT_AND_COMMIT]

- Owner: `MaterialEdit` `[Union]` closes mapping, cache, policy, and provider mutations; `SetMapping` composes render-owned `MappingSpec`/`MappingProfile` and mints `TextureMapping` only inside the call; `MaterialFact` `[Union]` owns one typed consequence per edit without sentinel integers; `ObjectReceipt<MaterialFact>` collects facts and every undo serial without a domain operator overload.
- Law: undo recording is a trait row. Recorded programs contain only `SetMapping` and `SetCachePolicy`; regenerable cache and provider effects run one at a time without an undo record. Admission rejects mixed programs, so rollback never promises to reverse an untracked side effect.
- Law: the commit rides `ObjectSpine.Commit` — admission and the trait guard precede the grant, the spine derives needs from the recording trait and redraw policy, and this page supplies only the resolve-then-apply fact fold, so no second window opens between the mutation and its repaint.
- Law: integer-returning writes preserve the host return — `SetTextureMapping` and `CreateMeshes` expose no catalogued verdict semantics, so receipts carry their values unchanged and invent no zero-or-sign success rule.
- Boundary: `HasCustomRenderMeshes`, `CustomRenderMeshesBoundingBox`, and the live `RenderMeshes` accessor demand a viewport, plug-in, and display-pipeline context this package does not own — they ride the Display and Render owners; this page's provider reach ends at the parameter knob.
- Growth: a new render-support mutation is one edit case with its trait and slot; the spine and the receipt read it with zero new surface.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialEdit {
    private MaterialEdit() { }
    public sealed record SetMapping(
        int Channel,
        MappingSpec Spec,
        MappingProfile Profile,
        Option<Transform> Motion = default) : MaterialEdit;
    public sealed record BuildCache(MeshType Kind, MeshPolicy Policy, ObjectSignal IgnoreCustom) : MaterialEdit;
    public sealed record DropCache(MeshType Kind) : MaterialEdit;
    public sealed record SetCachePolicy(MeshPolicy Policy) : MaterialEdit;
    public sealed record SetKnob(Guid Provider, string Name, ProviderValue Value) : MaterialEdit;

    internal Fin<MaterialEdit> Admit(Op op) =>
        Switch(
            op,
            setMapping: static (key, edit) =>
                from _ in guard(edit.Channel >= 0, key.InvalidInput()).ToFin()
                from spec in key.Need(edit.Spec)
                from profile in key.Need(edit.Profile)
                from motion in edit.Motion.Traverse(value => key.AcceptInput(value: value)).As()
                select (MaterialEdit)new SetMapping(
                    Channel: edit.Channel, Spec: spec, Profile: profile, Motion: motion),
            buildCache: static (key, edit) =>
                from policy in key.Need(edit.Policy)
                from ignore in key.Need(edit.IgnoreCustom)
                select (MaterialEdit)new BuildCache(Kind: edit.Kind, Policy: policy, IgnoreCustom: ignore),
            dropCache: static (_, edit) => Fin.Succ<MaterialEdit>(edit),
            setCachePolicy: static (key, edit) => key.Need(edit.Policy)
                .Map(policy => (MaterialEdit)new SetCachePolicy(Policy: policy)),
            setKnob: static (key, edit) =>
                from _ in guard(edit.Provider != Guid.Empty, key.InvalidInput()).ToFin()
                from name in key.AcceptText(value: edit.Name)
                from value in key.Need(edit.Value).Bind(item => item.Admit(key))
                select (MaterialEdit)new SetKnob(Provider: edit.Provider, Name: name, Value: value));

    internal bool RecordsUndo => Switch(
        setMapping: static _ => true,
        buildCache: static _ => false,
        dropCache: static _ => false,
        setCachePolicy: static _ => true,
        setKnob: static _ => false);

    internal Fin<MaterialFact> Apply(RhinoObject native, Op op) =>
        Switch(
            (Native: native, Op: op),
            setMapping: static (context, edit) => edit.Spec.Mint(edit.Profile.Cap, context.Op)
                .Bind(mapping => mapping.Use(value =>
                    from _ in edit.Profile.Apply(value, context.Op)
                    from native in context.Op.Catch(() => Fin.Succ(value: edit.Motion.Case switch {
                        Transform motion => context.Native.SetTextureMapping(
                            channel: edit.Channel, tm: value, objectTransform: motion),
                        _ => context.Native.SetTextureMapping(channel: edit.Channel, tm: value),
                    }))
                    select native))
                .Map(native => (MaterialFact)new MaterialFact.Mapped(
                    Id: context.Native.Id, Channel: edit.Channel, Native: native)),
            buildCache: static (context, edit) => edit.Policy.Use(
                parameters => context.Op.Catch(() => Fin.Succ<MaterialFact>(new MaterialFact.CacheBuilt(
                    Id: context.Native.Id,
                    Kind: edit.Kind,
                    Native: context.Native.CreateMeshes(
                        meshType: edit.Kind,
                        parameters: parameters,
                        ignoreCustomParameters: edit.IgnoreCustom.On)))),
                context.Op),
            dropCache: static (context, edit) => context.Op.Catch(() => {
                context.Native.DestroyMeshes(meshType: edit.Kind);
                return Fin.Succ<MaterialFact>(new MaterialFact.CacheDropped(Id: context.Native.Id, Kind: edit.Kind));
            }),
            setCachePolicy: static (context, edit) => edit.Policy.Use(
                parameters => context.Op.Confirm(success: context.Native.SetRenderMeshParameters(mp: parameters))
                    .Map(_ => (MaterialFact)new MaterialFact.PolicySet(Id: context.Native.Id)),
                context.Op),
            setKnob: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Catch(() => context.Native.SetCustomRenderMeshParameter(providerId: edit.Provider, parameterName: name, value: edit.Value.Native))
                select (MaterialFact)new MaterialFact.KnobSet(
                    Id: context.Native.Id, Provider: edit.Provider, Name: name));
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialFact {
    private MaterialFact() { }
    public sealed record Mapped(Guid Id, int Channel, int Native) : MaterialFact;
    public sealed record CacheBuilt(Guid Id, MeshType Kind, int Native) : MaterialFact;
    public sealed record CacheDropped(Guid Id, MeshType Kind) : MaterialFact;
    public sealed record PolicySet(Guid Id) : MaterialFact;
    public sealed record KnobSet(Guid Id, Guid Provider, string Name) : MaterialFact;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Materials {
    public static Fin<MaterialAnswer> Ask(DocumentSession session, TableTarget target, MaterialAsk ask) {
        Op op = Op.Of();
        return from active in op.Need(ask).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           (Natives: natives, Op: op),
                           resolve: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ask.Scope.Resolve(ask.Realm, native, ctx.Op)
                                   .Map(stamp => (native.Id, stamp))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Stamped(Rows: rows)),
                           partCensus: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id, native.HasSubobjectMaterials
                                   ? toSeq(native.SubobjectMaterialComponents)
                                   : Seq<ComponentIndex>())))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.PartMaterials(Rows: rows)),
                           mappingRoster: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => (native.HasTextureMapping()
                                   ? toSeq(native.GetTextureChannels())
                                   : Seq<int>()).TraverseM(channel => ctx.Op.Catch(() => {
                                       using TextureMapping? mapping = native.GetTextureMapping(channel, out Transform objectTransform);
                                       return Optional(mapping).ToFin(Fail: ctx.Op.InvalidResult())
                                           .Map(value => new MappingStamp(
                                               Channel: channel,
                                               Id: value.Id,
                                               ObjectTransform: objectTransform));
                                   })).As().Map(values => (native.Id, values)))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Mappings(Rows: rows)),
                           cacheCensus: static (ctx, ask) => ask.Policy.Use(
                               parameters => ctx.Natives
                                   .TraverseM(native => ctx.Op.Catch(() => Fin.Succ(value: (native.Id, native.MeshCount(
                                       meshType: ask.Kind,
                                       parameters: parameters))))).As()
                                   .Map(static rows => (MaterialAnswer)new MaterialAnswer.Tally(Rows: rows)),
                               ctx.Op),
                           cachedMeshes: static (ctx, ask) => Cached(
                                   natives: ctx.Natives,
                                   kind: ask.Kind,
                                   key: ctx.Op)
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Pieces(Rows: rows)),
                           cachePolicy: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() => {
                                   using MeshingParameters? policy = ask.DocumentFallback.On
                                       ? native.GetRenderMeshParameters(returnDocumentParametersIfUnset: true)
                                       : native.GetRenderMeshParameters();
                                   return Optional(policy)
                                       .Traverse(value => MeshPolicy.Capture(value, ctx.Op)).As()
                                       .Map(value => (native.Id, value));
                               })).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Policy(Rows: rows)),
                           meshable: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Fin.Succ(value: (native.Id, native.IsMeshable(meshType: ask.Kind))))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.Able(Rows: rows)),
                           harvest: static (ctx, ask) => ask.Batch.Run(natives: ctx.Natives, op: ctx.Op)
                               .Bind(run => {
                                   if (CommandVerdict.OfNative(result: run.Verdict) != CommandVerdict.Completed) {
                                       _ = ObjectPiece.Release(geometry: run.Meshes, attributes: run.Attributes);
                                       return Fin.Fail<MaterialAnswer>(error: ctx.Op.InvalidResult());
                                   }
                                   return Detach(meshes: run.Meshes, attributes: run.Attributes, key: ctx.Op)
                                       .Map(rows => (MaterialAnswer)new MaterialAnswer.Harvested(
                                           Rows: rows,
                                           Policy: run.Policy,
                                           Mode: run.Mode));
                               }),
                           knob: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Optional(native.GetCustomRenderMeshParameter(providerId: ask.Provider, parameterName: ask.Name))
                                       .Traverse(value => ProviderValue.Of(value, ctx.Op)).As()
                                       .Map(value => (native.Id, value)))).As()
                               .Map(static rows => (MaterialAnswer)new MaterialAnswer.KnobValue(Rows: rows)))
                       select folded,
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<ObjectReceipt<MaterialFact>> Commit(
        DocumentSession session, TableTarget target, RedrawPolicy redraw, params ReadOnlySpan<MaterialEdit> edits) {
        Op op = Op.Of();
        return from policy in op.Need(redraw)
               from requested in toSeq(edits.ToArray())
                   .TraverseM(edit => op.Need(edit)).As()
               from _ in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               from plan in requested.TraverseM(edit => edit.Admit(op: op)).As()
               let recording = plan.Exists(static edit => edit.RecordsUndo)
               from traits in guard(
                   plan.ForAll(edit => edit.RecordsUndo == recording)
                   && (recording || plan.Count is 1),
                   op.InvalidInput()).ToFin()
               from receipt in ObjectSpine.Commit(
                   session: session,
                   name: nameof(Materials),
                   redraw: policy,
                   fold: (document, key) => Objects.Resolve(document: document, target: target, key: key)
                       .Bind(natives => natives.TraverseM(native => plan
                           .TraverseM(edit => edit.Apply(native: native, op: key)).As()).As()
                           .Map(static grouped => grouped.Bind(static facts => facts))),
                   op: op,
                   recordsUndo: recording)
               select receipt;
    }

    private static Fin<Seq<(Guid Id, ObjectPiece Product)>> Detach(
        Mesh[]? meshes,
        ObjectAttributes[]? attributes,
        Op key) {
        Fin<Seq<(Guid Id, ObjectPiece Product)>> result =
            from shapes in Optional(meshes).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from paired in Optional(attributes).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from _ in guard(shapes.Count == paired.Count, key.InvalidResult()).ToFin()
            from pieces in ObjectPiece.DetachAll(
                rows: shapes.Map((shape, index) => ((GeometryBase)shape, Some(paired[index]))),
                key: key)
            select pieces.Map((piece, index) => (paired[index].ObjectId, piece));
        _ = ObjectPiece.Release(geometry: meshes, attributes: attributes);
        return result;
    }

    private static Fin<Seq<(Guid Id, Seq<ObjectPiece> Products)>> Cached(
        Seq<RhinoObject> natives, MeshType kind, Op key) =>
        ObjectPiece.Acquire(
            natives: natives,
            detach: native => key.Catch(() =>
                Optional(native.GetMeshes(meshType: kind)).ToFin(Fail: key.InvalidResult())
                    .Bind(meshes => ObjectPiece.DetachAll(
                        rows: toSeq(meshes).Map(static mesh => ((GeometryBase)mesh, Option<ObjectAttributes>.None)),
                        key: key))));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]             | [OWNER]           | [FORM]                                                 | [ENTRY]                       |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------------- | :---------------------------- |
|  [01]   | material resolution   | scope plus realm  | overload union plus row-owned material family          | `MaterialAsk.Resolve`         |
|  [02]   | detached identity     | `MaterialStamp`   | resolved id and name                                   | `MaterialAnswer.Stamped`      |
|  [03]   | render-support reads  | `MaterialAsk`     | one union over materials, channels, caches, and knobs  | `Materials.Ask`               |
|  [04]   | meshing policy        | `MeshPolicy`      | normalized encoding with call-scoped native custody    | cache asks and edits          |
|  [05]   | batch meshing         | `MeshBatch`       | host modalities onto identified rows                     | `MaterialAsk.Harvest`         |
|  [06]   | render-support writes | `MaterialEdit`    | trait-row undo recording on the shared spine           | `Materials.Commit`            |
|  [07]   | consequence evidence  | `MaterialFact`    | generated cases collected by `ObjectReceipt`           | `Materials.Commit`            |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
