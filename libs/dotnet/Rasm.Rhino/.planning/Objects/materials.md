# [RASM_RHINO_OBJECTS_MATERIALS]

Object render support belongs to `Rasm.Rhino.Objects`. `MaterialScope` discriminates the material-resolution overload family; `MaterialAsk<TAnswer>` is the self-typed read whose case fixes its own answer shape — material identity, component bindings, mapping identity and transform, cache state, per-object meshing policy, meshability, policy-complete batch meshes, and provider parameters; `MaterialEdit` owns mapping, cache, meshing-policy, and provider-parameter writes under one `CommitDemand` set. `Materials.Ask` and `Materials.Commit` are the two entries, and `MaterialProgram` is the admitted edit roster that makes a mixed program unspellable.

## [01]-[INDEX]

- [02]-[SCOPE_AND_STAMP]: `SurfaceSide`, `MeshKind`, `MaterialScope`, `MaterialRealm`, `RenderMeshPolicy`, `MaterialStamp`, and the resolution law.
- [03]-[ASK_FAMILY]: `MaterialAsk<TAnswer>` — the self-typed read over materials, mappings, caches, policy, and the batch harvest.
- [04]-[EDIT_AND_COMMIT]: `CommitDemand`, `MaterialEdit`, `MaterialProgram`, and the `Materials` entry pair.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SCOPE_AND_STAMP]

- Owner: `SurfaceSide` `[SmartEnum<bool>]` closes the face axis; `MeshKind` `[SmartEnum<int>]` is the folder's ONE crossing of the host `MeshType` discriminant; `MaterialScope` `[Union]` closes face, component, plug-in-keyed component, and hypothetical-attribute resolution; `MaterialRealm` carries the legacy/render host-member family as four `[UseDelegateFromConstructor]` columns — `Face`, `Part`, `Keyed`, `Under` — one row per material family; `PartUnder` carries an `AttributeProgram` that builds a scoped duplicate; `RenderMeshPolicy` is the encoded meshing-parameter value; `MaterialStamp` carries detached resolved identity and name.
- Law: no raw host discriminant crosses a signature this page declares. `Document/tables.md` states the folder Law for `ObjectType` and names `MeshKind` as its counterpart for `MeshType`; that row lands HERE, this page being the mesh custodian, and `Host` is the only member that reads the raw value — at the host call itself.
- Law: `MaterialScope` and `MaterialRealm` are independent discriminants — `MaterialAsk.Resolve(MaterialRealm, MaterialScope)` is the sole material question, one `MaterialScope.Resolve` dispatch selects the `MaterialRealm` column per scope, and that row selects `Material` versus `RenderMaterial` without a second family dispatch, sibling ask cases, or a boolean knob. The face axis is a ROW on the scope's one `Face` case, so the two literals that spelled front and back at the call site read their own vocabulary instead.
- Law: resolution detaches — the resolved material projects to `MaterialStamp` inside the grant window through the boundary mapper, because a `Material` is table state addressed through the document pipeline and a `RenderMaterial` is render-content state owned by the render tables; a live material handle crossing this boundary is the deleted form.
- Law: the per-component census is queried, never scanned — `HasSubobjectMaterials` gates `SubobjectMaterialComponents`, and stored per-plug-in rows install and retract through `AttributeEdit.FaceMaterials`.
- Law: `RenderMeshPolicy` is the RENDER-MESH parameter encoding and carries no meshing STRATEGY. `Rasm.Compute` `Solver/discretization`'s `MeshPolicy` names a finite-element meshing strategy, so the host-boundary one carries the qualifier its concept already had.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<T>]`, `[Union]`, `[ValueObject<T>]`, `[ComplexValueObject]`, `[ValidationError]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`); Riok.Mapperly (`libs/dotnet/.api/api-mapperly.md` — `[Mapper]`, `[MapProperty]`, `[UserMapping]`, `RequiredMappingStrategy.Target`); RhinoCommon objects (`.api/api-rhinocommon-objects.md` — `RhinoObject.GetMaterial`/`GetRenderMaterial`, `MeshType`, `GetRenderMeshParameters`, `SetRenderMeshParameters`); RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `MeshingParameters.FromEncodedString`/`ToEncodedString`); `Document/session.md` (`DraftFault`); `Render/mapping.md` (`MappingChannel`, `MappingSpec`, `MappingProfile`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Render;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class SurfaceSide {
    public static readonly SurfaceSide Front = new(key: true);
    public static readonly SurfaceSide Back = new(key: false);
}

[SmartEnum<int>]
public sealed partial class MeshKind {
    public static readonly MeshKind Default = new(key: (int)MeshType.Default, wire: "default");
    public static readonly MeshKind Render = new(key: (int)MeshType.Render, wire: "render");
    public static readonly MeshKind Analysis = new(key: (int)MeshType.Analysis, wire: "analysis");
    public static readonly MeshKind Preview = new(key: (int)MeshType.Preview, wire: "preview");
    public static readonly MeshKind Any = new(key: (int)MeshType.Any, wire: "any");

    internal string Wire { get; }
    internal MeshType Host => (MeshType)Key;

    internal static Fin<MeshKind> Of(MeshType kind) =>
        FactoryBridge.Row<MeshType, MeshKind>(candidate: kind, ordinal: static value => (int)value);
}

[SmartEnum<bool>]
public sealed partial class MeshThread {
    public static readonly MeshThread Worker = new(key: true);
    public static readonly MeshThread Caller = new(key: false);
}

[SmartEnum<bool>]
public sealed partial class MeshDialog {
    public static readonly MeshDialog Simple = new(key: true);
    public static readonly MeshDialog Full = new(key: false);

    internal static MeshDialog Of(bool simple) => simple ? Simple : Full;
}

[SmartEnum]
public sealed partial class MaterialRealm {
    public static readonly MaterialRealm Legacy = new(
        face: static (native, side, key) => MaterialMap.Detach(native.GetMaterial(frontMaterial: side.Key)),
        part: static (native, component, key) => MaterialMap.Detach(native.GetMaterial(componentIndex: component)),
        keyed: static (native, component, plugIn, key) => MaterialMap.Detach(native.GetMaterial(componentIndex: component, plugInId: plugIn)),
        under: static (native, component, plugIn, attributes, key) => MaterialMap.Detach(native.GetMaterial(componentIndex: component, plugInId: plugIn, attributes: attributes)));
    public static readonly MaterialRealm Rendered = new(
        face: static (native, side, key) => MaterialMap.Detach(native.GetRenderMaterial(frontMaterial: side.Key)),
        part: static (native, component, key) => MaterialMap.Detach(native.GetRenderMaterial(componentIndex: component)),
        keyed: static (native, component, plugIn, key) => MaterialMap.Detach(native.GetRenderMaterial(componentIndex: component, plugInId: plugIn)),
        under: static (native, component, plugIn, attributes, key) => MaterialMap.Detach(native.GetRenderMaterial(componentIndex: component, plugInId: plugIn, attributes: attributes)));

    [UseDelegateFromConstructor]
    internal partial Fin<MaterialStamp> Face(RhinoObject native, SurfaceSide side);

    [UseDelegateFromConstructor]
    internal partial Fin<MaterialStamp> Part(RhinoObject native, ComponentIndex component);

    [UseDelegateFromConstructor]
    internal partial Fin<MaterialStamp> Keyed(RhinoObject native, ComponentIndex component, Guid plugIn);

    [UseDelegateFromConstructor]
    internal partial Fin<MaterialStamp> Under(
        RhinoObject native, ComponentIndex component, Guid plugIn, ObjectAttributes attributes);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialScope {
    private MaterialScope() { }
    public sealed record Face(SurfaceSide Side) : MaterialScope;
    public sealed record Part(ComponentIndex Component) : MaterialScope;
    public sealed record PartFor(ComponentIndex Component, Guid PlugIn) : MaterialScope;
    public sealed record PartUnder(ComponentIndex Component, Guid PlugIn, AttributeProgram Program) : MaterialScope;

    internal Fin<MaterialScope> Admit() =>
        Switch(face: static (op, scope) => Admit.Need(scope.Side).Map(_ => (MaterialScope)scope),
            part: static (_, scope) => Fin.Succ<MaterialScope>(scope),
            partFor: static (op, scope) => guard(scope.PlugIn != Guid.Empty, new KernelFault.InvalidInput()).ToFin().Map(_ => (MaterialScope)scope),
            partUnder: static (op, scope) =>
                from _ in guard(scope.PlugIn != Guid.Empty, new KernelFault.InvalidInput()).ToFin()
                from __ in Admit.Need(scope.Program)
                select (MaterialScope)scope);

    internal Fin<MaterialStamp> Resolve(MaterialRealm realm, RhinoObject native) =>
        Switch(
            (Realm: realm, Native: native),
            face: static (ctx, scope) => Try.lift(() => ctx.Realm.Face(ctx.Native, scope.Side)).Run().Bind(static inner => inner),
            part: static (ctx, scope) => Try.lift(() => ctx.Realm.Part(ctx.Native, scope.Component)).Run().Bind(static inner => inner),
            partFor: static (ctx, scope) => Try.lift(() => ctx.Realm.Keyed(ctx.Native, scope.Component, scope.PlugIn)).Run().Bind(static inner => inner),
            partUnder: static (ctx, scope) => Try.lift(() => {
                using ObjectAttributes attributes = ctx.Native.Attributes.Duplicate();
                return scope.Program.Apply(attributes)
                    .Bind(_ => ctx.Realm.Under(ctx.Native, scope.Component, scope.PlugIn, attributes));
            }).Run().Bind(static inner => inner));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct MaterialStamp(Guid Id, Option<string> Name) : IDetachedDocumentResult;

public readonly record struct MappingStamp(MappingChannel Channel, Guid Id, Transform ObjectTransform) : IDetachedDocumentResult;

[ValueObject<string>]
[ValidationError]
public sealed partial class RenderMeshPolicy {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        using MeshingParameters? native = string.IsNullOrWhiteSpace(value)
            ? null
            : MeshingParameters.FromEncodedString(value);
        if (native is null) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { nameof(MeshingParameters.FromEncodedString), value ?? string.Empty }));
        } else {
            value = native.ToEncodedString();
        }
    }

    internal static Fin<RenderMeshPolicy> Capture(MeshingParameters native) =>
        Try.lift(() => FactoryBridge.Accept<RenderMeshPolicy>(
            fault: Validate(native.ToEncodedString(), out RenderMeshPolicy? admitted),
            admitted: admitted)).Run().Bind(static inner => inner);

    internal Fin<T> Use<T>(Func<MeshingParameters, Fin<T>> body) =>
        Try.lift(() => {
            using MeshingParameters? native = MeshingParameters.FromEncodedString(ToValue());
            return native is null ? Fin.Fail<T>(new KernelFault.InvalidResult()) : body(native);
        }).Run().Bind(static inner => inner);
}

[ValueObject<int>]
[ValidationError]
public readonly partial struct MeshUiStyle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(MeshUiStyle), value, "non-negative" }));
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

    internal Fin<ProviderValue> Admit() =>
        Switch(flag: static (_, value) => Fin.Succ<ProviderValue>(value),
            signed: static (_, value) => Fin.Succ<ProviderValue>(value),
            unsigned: static (_, value) => Fin.Succ<ProviderValue>(value),
            real: static (op, value) => guard(double.IsFinite(value.Value), new KernelFault.InvalidInput()).ToFin()
                .Map(_ => (ProviderValue)value),
            precise: static (_, value) => Fin.Succ<ProviderValue>(value),
            text: static (op, value) => Acceptance.Text(value: value.Value)
                .Map(text => (ProviderValue)new Text(Value: text)));

    internal static Fin<ProviderValue> Of(IConvertible native) => (native switch {
        bool value => Fin.Succ<ProviderValue>(new Flag(value)),
        sbyte or short or int or long => Fin.Succ<ProviderValue>(new Signed(native.ToInt64(System.Globalization.CultureInfo.InvariantCulture))),
        byte or ushort or uint or ulong => Fin.Succ<ProviderValue>(new Unsigned(native.ToUInt64(System.Globalization.CultureInfo.InvariantCulture))),
        float or double => Fin.Succ<ProviderValue>(new Real(native.ToDouble(System.Globalization.CultureInfo.InvariantCulture))),
        decimal value => Fin.Succ<ProviderValue>(new Precise(value)),
        string value => Fin.Succ<ProviderValue>(new Text(value)),
        _ => Fin.Fail<ProviderValue>(new KernelFault.InvalidResult(Detail: Some(native.GetTypeCode().ToString()))),
    }).Bind(value => value.Admit());
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeshBatch : IDetachedDocumentResult {
    private MeshBatch() { }
    public sealed record Worker(RenderMeshPolicy Policy, MeshThread Thread) : MeshBatch;
    public sealed record Dialog(RenderMeshPolicy Policy, MeshDialog Prompt) : MeshBatch;
    public sealed record Styled(RenderMeshPolicy Policy, MeshUiStyle Style, Transform Motion) : MeshBatch;

    internal Fin<MeshBatch> Admit() =>
        Switch(worker: static (key, batch) =>
                from policy in Admit.Need(batch.Policy)
                from thread in Admit.Need(batch.Thread)
                select (MeshBatch)new Worker(Policy: policy, Thread: thread),
            dialog: static (key, batch) =>
                from policy in Admit.Need(batch.Policy)
                from prompt in Admit.Need(batch.Prompt)
                select (MeshBatch)new Dialog(Policy: policy, Prompt: prompt),
            styled: static (key, batch) =>
                from policy in Admit.Need(batch.Policy)
                from motion in Acceptance.Input(value: batch.Motion)
                select (MeshBatch)new Styled(Policy: policy, Style: batch.Style, Motion: motion));

    internal Fin<MeshRun> Run(Seq<RhinoObject> natives) =>
        Switch(
            natives,
            worker: static (context, batch) => batch.Policy.Use(
                parameters => Try.lift(() => {
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.AsIterable(),
                        parameters: parameters,
                        meshes: out Mesh[] meshes,
                        attributes: out ObjectAttributes[] attributes,
                        useWorkerThread: batch.Thread.Key);
                    return Capture(
                        verdict: verdict,
                        meshes: meshes,
                        attributes: attributes,
                        parameters: parameters,
                        settle: policy => new Worker(Policy: policy, Thread: batch.Thread));
                }).Run().Bind(static inner => inner)),
            dialog: static (context, batch) => batch.Policy.Use(
                parameters => Try.lift(() => {
                    bool simple = batch.Prompt.Key;
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.AsIterable(),
                        parameters: ref parameters,
                        simpleDialog: ref simple,
                        meshes: out Mesh[] meshes,
                        attributes: out ObjectAttributes[] attributes);
                    return Capture(
                        verdict: verdict,
                        meshes: meshes,
                        attributes: attributes,
                        parameters: parameters,
                        settle: policy => new Dialog(Policy: policy, Prompt: MeshDialog.Of(simple: simple)));
                }).Run().Bind(static inner => inner)),
            styled: static (context, batch) => batch.Policy.Use(
                parameters => Try.lift(() => {
                    int style = batch.Style.Value;
                    Result verdict = RhinoObject.MeshObjects(
                        rhinoObjects: context.AsIterable(),
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
                        settle: policy => new Styled(
                            Policy: policy, Style: MeshUiStyle.Create(style), Motion: batch.Motion));
                }).Run().Bind(static inner => inner)));

    private static Fin<MeshRun> Capture(
        Result verdict,
        Mesh[] meshes,
        ObjectAttributes[] attributes,
        MeshingParameters parameters,
        Func<RenderMeshPolicy, MeshBatch> settle) =>
        (from policy in Error.New(parameters.Message, parameters)
         from terminal in CommandVerdict.OfNative(result: verdict)
         select new MeshRun(Verdict: terminal, Meshes: meshes, Attributes: attributes, Settled: settle(policy)))
        .Rollback(
            release: () => ObjectPiece.Release(geometry: meshes, attributes: attributes));
}

internal sealed record MeshRun(
    CommandVerdict Verdict,
    Mesh[] Meshes,
    ObjectAttributes[] Attributes,
    MeshBatch Settled) {
    internal Fin<Unit> Release() => ObjectPiece.Release(geometry: Meshes, attributes: Attributes);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class MaterialMap {
    internal static Fin<MaterialStamp> Detach(Material? native) =>
        Optional(native).ToFin(Fail: new KernelFault.InvalidResult()).Map(Stamp);

    internal static Fin<MaterialStamp> Detach(RenderMaterial? native) =>
        Optional(native).ToFin(Fail: new KernelFault.InvalidResult()).Map(Stamp);

    private static partial MaterialStamp Stamp(Material native);

    private static partial MaterialStamp Stamp(RenderMaterial native);

    [UserMapping]
    private static Option<string> Label(string value) => HostEdge.Text(value);
}
```

## [03]-[ASK_FAMILY]

- Owner: `MaterialAsk<TAnswer>` is the SELF-TYPED read — the request fixes its answer shape, so `Materials.Ask` returns the caller's own type and no answer union, no cast arm, and no consumer downcast exists. `MaterialAsk` (the static factory class) is the case roster: material resolution, component bindings, mapping identity and transform, cache census, cached-mesh custody, per-object meshing policy, meshability, the `MeshBatch` harvest, and provider parameters.
- Law: NAMED LOSS — the one `Dispose` fold over every answer case. `MaterialAnswer` implemented `IDisposable` for all nine cases while seven owned nothing; `MeshPieces` and `MeshHarvest` are the only owning answers and expose `Fin<Unit> Release()`, keeping cleanup refusals typed instead of discarding them from `Dispose`.
- Law: cache reads never build — `MeshCount` and `GetMeshes` answer the existing cache and `IsMeshable` answers capability, so a read inside a paused command allocates nothing; construction is the edit family's `BuildCache`, and `Harvest` alone runs the batch mesher.
- Law: cached meshes cross under custody — `GetMeshes` returns non-owning const wrappers parented to the live object, so each result detaches through `GeometryCrossing.Cross` onto its own handle before the grant closes; a consumer holding a parented cache mesh across a regen dereferences freed memory, and mutating one silently fails to persist.
- Law: meshing policy crosses encoded — `RenderMeshPolicy` captures `ToEncodedString()` while each `MeshingParameters` carrier is still scoped and reconstructs it with `FromEncodedString()` only for one host call; `MeshFallback` selects document fallback without exporting a boolean policy, and both `GetRenderMeshParameters` arguments are spelled because the parameterless call IS the document-fallback arm — a knob whose two arms reach one host behaviour selects nothing.
- Law: meshing-carrier ownership splits by the host ARGUMENT, never by the member — `GetRenderMeshParameters(true)` fills a freshly minted carrier this pipeline leases and disposes, `GetRenderMeshParameters(false)` and `ObjectAttributes.CustomMeshingParameters` hand back a wrapper over stored host memory whose unconditional `Dispose` frees state the owner still holds, so those two reads encode inside the borrow and never bracket it.
- Law: `Harvest` is the batch lane and `MeshBatch` is its ONE family — worker-thread, mutable simple-dialog, and mutable UI-style-plus-transform modalities over one resolved roster, and the run answers a `Settled` value of that same family carrying the captured policy and every ref-updated column back, so a new modality cannot land on the request without landing on the fact. `MeshRun` owns both host arrays from the mesher's return until detachment crosses them or `Release` frees them, the host verdict folds through `CommandVerdict.OfNative` inside the run, and a non-`Completed` terminal names its own key in the refusal.
- Law: the paired detach is the state page's `ObjectPiece.Paired` — the custody fold that forces the identity projection before the source arrays release exists ONCE, and this pipeline composes it and adds only the owner zip the batch lane needs.
- Law: provider evidence is `ProviderValue` — bool, signed, unsigned, real, decimal, and text values remain distinct generated cases in both directions, and every constructed case re-enters the one `Admit` fold so a non-finite provider readback refuses exactly as a non-finite write does; arbitrary `IConvertible` values fail instead of type-erasing into text.
- Boundary: `MappingRoster` returns channel identity, mapping identity, and the object transform from `GetTextureMapping(channel, out Transform)`; construction, profile, inverse recovery, and evaluation remain `MappingSpec`/`Mappings.Run` responsibilities on the render mapping owner.
- Growth: a new render-support read is one static factory whose return type names its own answer.
- Packages: LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`, `Traverse`); Thinktecture.Runtime.Extensions (`[SmartEnum<bool>]`); RhinoCommon objects (`.api/api-rhinocommon-objects.md` — `MeshCount`, `GetMeshes`, `IsMeshable`, `GetTextureChannels`, `GetTextureMapping`, `GetCustomRenderMeshParameter`, `RhinoObject.MeshObjects`); `Objects/state.md` (`ObjectPiece.Paired`, `ObjectPiece.Acquire`, `ObjectPiece.Release`); kernel `Domain/results` (`Lease<T>`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class MeshFallback {
    public static readonly MeshFallback Document = new(key: true);
    public static readonly MeshFallback ObjectOnly = new(key: false);
}

public sealed record MaterialAsk<TAnswer> {
    internal MaterialAsk(
        Func< Fin<MaterialAsk<TAnswer>>> admit, Func<Seq<RhinoObject>, Fin<TAnswer>> read) =>
        (Admit, Read) = (admit, read);

    internal Func< Fin<MaterialAsk<TAnswer>>> Admit { get; }
    internal Func<Seq<RhinoObject>, Fin<TAnswer>> Read { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeshPieces(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : IDetachedDocumentResult {
    public Fin<Unit> Release() => ObjectPiece.Release(rows: Rows);
}

public sealed record MeshHarvest(Seq<(Guid Id, ObjectPiece Product)> Rows, MeshBatch Settled) : IDetachedDocumentResult {
    public Fin<Unit> Release() => ObjectPiece.Release(rows: Rows);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialAsk {
    public static MaterialAsk<Seq<(Guid Id, MaterialStamp Stamp)>> Resolve(MaterialRealm realm, MaterialScope scope) => new(
        admit: op =>
            from family in Admit.Need(realm)
            from address in Admit.Need(scope).Bind(value => value.Admit())
            select Resolve(realm: family, scope: address),
        read: (natives, op) => natives
            .TraverseM(native => scope.Resolve(realm, native, op).Map(stamp => (native.Id, stamp))).As());

    public static MaterialAsk<Seq<(Guid Id, Seq<ComponentIndex> Components)>> PartCensus { get; } = Free(
        read: static (natives, op) => natives
            .TraverseM(native => Try.lift(() => Fin.Succ(value: (native.Id, native.HasSubobjectMaterials
                ? toSeq(native.SubobjectMaterialComponents)
                : Seq<ComponentIndex>()))).Run().Bind(static inner => inner)).As());

    public static MaterialAsk<Seq<(Guid Id, Seq<MappingStamp> Values)>> MappingRoster { get; } = Free(
        read: static (natives, op) => natives
            .TraverseM(native => Try.lift(() => (native.HasTextureMapping()
                ? toSeq(native.GetTextureChannels())
                : Seq<int>()).TraverseM(channel => Try.lift(() => {
                    using TextureMapping? mapping = native.GetTextureMapping(channel, out Transform objectTransform);
                    return from admitted in Optional(mapping).ToFin(Fail: new KernelFault.InvalidResult())
                           from slot in MappingChannel.Admit(value: channel)
                           select new MappingStamp(
                               Channel: slot, Id: admitted.Id, ObjectTransform: objectTransform);
                }).Run().Bind(static inner => inner)).As().Map(values => (native.Id, values))).Run().Bind(static inner => inner)).As());

    public static MaterialAsk<Seq<(Guid Id, int Count)>> CacheCensus(MeshKind kind, RenderMeshPolicy policy) => new(
        admit: op =>
            from row in Admit.Need(kind)
            from admitted in Admit.Need(policy)
            select CacheCensus(kind: row, policy: admitted),
        read: (natives, op) => policy.Use(
            parameters => natives
                .TraverseM(native => Try.lift(() => Fin.Succ(value: (native.Id, native.MeshCount(
                    meshType: kind.Host, parameters: parameters)))).Run().Bind(static inner => inner)).As()));

    public static MaterialAsk<MeshPieces> CachedMeshes(MeshKind kind) => new(
        admit: op => Admit.Need(kind).Map(row => CachedMeshes(kind: row)),
        read: (natives, op) => ObjectPiece.Acquire(
                natives: natives,
                detach: native => Try.lift(() =>
                    Optional(native.GetMeshes(meshType: kind.Host)).ToFin(Fail: new KernelFault.InvalidResult())
                        .Bind(meshes => ObjectPiece.DetachAll(
                            rows: toSeq(meshes).Map(static mesh => ((GeometryBase)mesh, Option<ObjectAttributes>.None))))).Run().Bind(static inner => inner))
            .Map(static rows => new MeshPieces(Rows: rows)));

    public static MaterialAsk<Seq<(Guid Id, Option<RenderMeshPolicy> Value)>> CachePolicy(MeshFallback fallback) => new(
        admit: op => Admit.Need(fallback).Map(row => CachePolicy(fallback: row)),
        read: (natives, op) => natives
            .TraverseM(native => Try.lift(() => fallback == MeshFallback.Document
                ? Fresh(policy: native.GetRenderMeshParameters(returnDocumentParametersIfUnset: true))
                : Stored(policy: native.GetRenderMeshParameters(returnDocumentParametersIfUnset: false))).Run().Bind(static inner => inner)
                .Map(value => (native.Id, value))).As());

    public static MaterialAsk<Seq<(Guid Id, bool Verdict)>> Meshable(MeshKind kind) => new(
        admit: op => Admit.Need(kind).Map(row => Meshable(kind: row)),
        read: (natives, op) => natives
            .TraverseM(native => Try.lift(() =>
                Fin.Succ(value: (native.Id, native.IsMeshable(meshType: kind.Host)))).Run().Bind(static inner => inner)).As());

    public static MaterialAsk<MeshHarvest> Harvest(MeshBatch batch) => new(
        admit: op => Admit.Need(batch).Bind(value => value.Admit()).Map(value => Harvest(batch: value)),
        read: (natives, op) => batch.Run(natives: natives)
            .Bind(run => run.Verdict == CommandVerdict.Completed
                ? Harvested(meshes: run.Meshes, attributes: run.Attributes)
                    .Map(rows => new MeshHarvest(Rows: rows, Settled: run.Settled))
                : Fin.Fail<MeshHarvest>(error: new KernelFault.InvalidResult(Detail: Some(run.Verdict.Key.ToString(
                        System.Globalization.CultureInfo.InvariantCulture))))
                    .Rollback(release: () => run.Release())));

    public static MaterialAsk<Seq<(Guid Id, Option<ProviderValue> Value)>> Knob(Guid provider, string name) => new(
        admit: op =>
            from _ in guard(provider != Guid.Empty, new KernelFault.InvalidInput()).ToFin()
            from admitted in Acceptance.Text(value: name)
            select Knob(provider: provider, name: admitted),
        read: (natives, op) => natives
            .TraverseM(native => Try.lift(() =>
                Optional(native.GetCustomRenderMeshParameter(providerId: provider, parameterName: name))
                    .Traverse(value => ProviderValue.Of(value, op)).As()
                    .Map(value => (native.Id, value))).Run().Bind(static inner => inner)).As());

    private static MaterialAsk<TAnswer> Free<TAnswer>(Func<Seq<RhinoObject>, Fin<TAnswer>> read) =>
        new(admit: _ => Fin.Succ(value: Free(read)), read: read);

    private static Fin<Option<RenderMeshPolicy>> Fresh(MeshingParameters? policy) =>
        Optional(policy)
            .TraverseM(value => new Lease<MeshingParameters>.Owned(Value: value)
                .Use(held => Error.New(held.Message, held)))
            .As();

    private static Fin<Option<RenderMeshPolicy>> Stored(MeshingParameters? policy) =>
        Optional(policy).Traverse(value => Error.New(value.Message, value)).As();

    private static Fin<Seq<(Guid Id, ObjectPiece Product)>> Harvested(
        Mesh[]? meshes, ObjectAttributes[]? attributes) =>
        from owners in Optional(attributes).ToFin(Fail: new KernelFault.InvalidResult())
            .Map(static values => toSeq(values).Map(static value => value.ObjectId).Strict())
        from pieces in ObjectPiece.Paired(geometry: meshes, attributes: attributes)
        from _ in guard(pieces.Count == owners.Count, new KernelFault.InvalidResult())
        select pieces.Map((piece, index) => (owners[index], piece)).Strict();
}
```

## [04]-[EDIT_AND_COMMIT]

- Owner: `CommitDemand` is the two-row commit vocabulary every edit declares; `MaterialEdit` `[Union]` closes mapping, cache, policy, and provider mutations; `MaterialProgram` `[ComplexValueObject]` is the admitted edit roster; `SetMapping` composes render-owned `MappingSpec`/`MappingProfile` and mints `TextureMapping` only inside the call.
- Law: the commit demands are a READABLE SET, not a bool table. `Undo` states that the program records and rolls back; `Solo` states that the effect is regenerable and must run alone, which is why the one-at-a-time rule exists at all. The spine derives grant needs and redraw policy from the same set, so rollback never promises to reverse an untracked side effect and no second window opens between the mutation and its repaint.
- Law: a mixed program is UNREPRESENTABLE, not guarded at the entry. `MaterialProgram` admits a nonempty roster whose members share one demand set and refuses a `Solo` roster of more than one, so the homogeneity law states once on the value every caller must build rather than inside the commit body every caller reaches through.
- Law: `SetTextureMapping` and `CreateMeshes` expose no catalogued verdict semantics, so each raw integer remains inside the caught host call and projects to `Unit` without a zero-or-sign test.
- Boundary: `HasCustomRenderMeshes`, `CustomRenderMeshesBoundingBox`, and the live `RenderMeshes` accessor demand a viewport, plug-in, and display-pipeline context this package does not own — they ride the Display and Render owners; this page's provider reach ends at the parameter knob.
- Boundary: zero raw `catch` crosses this page — every host call rides `Try.lift` or a host-verdict fold, which is the folder exemplar and stays.
- Growth: a new render-support mutation is one edit case with its demand set; the admitted program and the spine read it with zero new surface.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`, `[ComplexValueObject]`, `[ValidationError]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`, `ForAll`); RhinoCommon objects (`SetTextureMapping`, `CreateMeshes`, `DestroyMeshes`, `SetRenderMeshParameters`, `SetCustomRenderMeshParameter`); `Objects/state.md` (`ObjectSpine.Commit`, `Objects.Resolve`); `Render/mapping.md` (`MappingChannel`, `MappingSpec`, `MappingProfile`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommitDemand : ICapability<CommitDemand> {
    public static readonly CommitDemand Undo = new(key: "undo");
    public static readonly CommitDemand Solo = new(key: "solo");
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialEdit {
    private MaterialEdit() { }
    public sealed record SetMapping(
        MappingChannel Channel,
        MappingSpec Spec,
        MappingProfile Profile,
        Option<Transform> Motion = default) : MaterialEdit;
    public sealed record BuildCache(MeshKind Kind, RenderMeshPolicy Policy, MeshFallback IgnoreCustom) : MaterialEdit;
    public sealed record DropCache(MeshKind Kind) : MaterialEdit;
    public sealed record SetCachePolicy(RenderMeshPolicy Policy) : MaterialEdit;
    public sealed record SetKnob(Guid Provider, string Name, ProviderValue Value) : MaterialEdit;

    internal CapabilitySet<CommitDemand> Demands => Map(
        setMapping: CapabilitySet<CommitDemand>.Of(CommitDemand.Undo),
        buildCache: CapabilitySet<CommitDemand>.Of(CommitDemand.Solo),
        dropCache: CapabilitySet<CommitDemand>.Of(CommitDemand.Solo),
        setCachePolicy: CapabilitySet<CommitDemand>.Of(CommitDemand.Undo),
        setKnob: CapabilitySet<CommitDemand>.Of(CommitDemand.Solo));

    internal Fin<MaterialEdit> Admit() =>
        Switch(setMapping: static (key, edit) =>
                from channel in Admit.Need(edit.Channel)
                from spec in Admit.Need(edit.Spec)
                from profile in Admit.Need(edit.Profile)
                from motion in edit.Motion.Traverse(value => Acceptance.Input(value: value)).As()
                select (MaterialEdit)new SetMapping(
                    Channel: channel, Spec: spec, Profile: profile, Motion: motion),
            buildCache: static (key, edit) =>
                from kind in Admit.Need(edit.Kind)
                from policy in Admit.Need(edit.Policy)
                from ignore in Admit.Need(edit.IgnoreCustom)
                select (MaterialEdit)new BuildCache(Kind: kind, Policy: policy, IgnoreCustom: ignore),
            dropCache: static (key, edit) => Admit.Need(edit.Kind).Map(_ => (MaterialEdit)edit),
            setCachePolicy: static (key, edit) => Admit.Need(edit.Policy)
                .Map(policy => (MaterialEdit)new SetCachePolicy(Policy: policy)),
            setKnob: static (key, edit) =>
                from _ in guard(edit.Provider != Guid.Empty, new KernelFault.InvalidInput()).ToFin()
                from name in Acceptance.Text(value: edit.Name)
                from value in Admit.Need(edit.Value).Bind(item => item.Admit())
                select (MaterialEdit)new SetKnob(Provider: edit.Provider, Name: name, Value: value));

    internal Fin<Unit> Apply(RhinoObject native) =>
        Switch(
            native,
            setMapping: static (context, edit) => edit.Spec.Mint(edit.Profile.Cap)
                .Bind(mapping => mapping.Use(value =>
                    from _ in edit.Profile.Apply(value)
                    from __ in Try.lift(() => Fin.Succ(value: edit.Motion.Case switch {
                        Transform motion => context.SetTextureMapping(
                            channel: edit.Channel.Value, tm: value, objectTransform: motion),
                        _ => context.SetTextureMapping(channel: edit.Channel.Value, tm: value),
                    })).Run().Bind(static inner => inner)
                    select unit)),
            buildCache: static (context, edit) => edit.Policy.Use(
                parameters => Try.lift(() => Fin.Succ(value: context.CreateMeshes(
                    meshType: edit.Kind.Host,
                    parameters: parameters,
                    ignoreCustomParameters: edit.IgnoreCustom.Key))).Run().Bind(static inner => inner).Map(static _ => unit)),
            dropCache: static (context, edit) => Try.lift(() => {
                context.DestroyMeshes(meshType: edit.Kind.Host);
                return Fin.Succ(unit);
            }).Run().Bind(static inner => inner),
            setCachePolicy: static (context, edit) => edit.Policy.Use(
                parameters => Admit.Confirm(success: context.SetRenderMeshParameters(mp: parameters))),
            setKnob: static (context, edit) => Try.lift(() => context.SetCustomRenderMeshParameter(
                providerId: edit.Provider, parameterName: edit.Name, value: edit.Value)).Run().Bind(static inner => inner));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class MaterialProgram {
    public Seq<MaterialEdit> Edits { get; }
    public CapabilitySet<CommitDemand> Demands { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Seq<MaterialEdit> edits, ref CapabilitySet<CommitDemand> demands) {
        if (edits.IsEmpty) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { op, nameof(Edits) }));
            return;
        }

        demands = edits.Head.Map(static edit => edit.Demands).IfNone(CapabilitySet<CommitDemand>.Of());
        CapabilitySet<CommitDemand> shared = demands;
        Seq<MaterialEdit> roster = edits;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (!roster.ForAll(edit => edit.Demands == shared), () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Edits), "one demand set across the roster" }))),
                (shared.Admits(capability: CommitDemand.Solo) && roster.Count is not 1,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Edits), "a solo effect runs alone" })))));
    }

    public static Fin<MaterialProgram> Of(params ReadOnlySpan<MaterialEdit> edits) {
        return from requested in LanguageExt.Iterable<MaterialEdit>.FromSpan(edits).ToSeq()
                   .TraverseM(edit => Admit.Need(edit).Bind(value => value.Admit())).As()
               from admitted in FactoryBridge.Accept<MaterialProgram>(
                   fault: Validate(requested, CapabilitySet<CommitDemand>.Of(), out MaterialProgram? built),
                   admitted: built)
               select admitted;
    }

    internal bool RecordsUndo => Demands.Admits(capability: CommitDemand.Undo);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Materials {
    public static Fin<TAnswer> Ask<TAnswer>(
        DocumentSession session, TableTarget target, MaterialAsk<TAnswer> ask) {
        return from active in Admit.Need(ask).Bind(value => value.Admit())
               from answer in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target)
                       from folded in active.Read(natives, op)
                       select folded,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<Unit> Commit(
        DocumentSession session, TableTarget target, RedrawPolicy redraw, MaterialProgram program) {
        return from policy in Admit.Need(redraw)
               from plan in Admit.Need(program)
               from _ in ObjectSpine.Commit(
                   session: session,
                   name: nameof(Materials),
                   redraw: policy,
                   run: (document, gate) => Objects.Resolve(document: document, target: target, key: gate)
                       .Bind(natives => natives.TraverseM(native => plan.Edits
                           .TraverseM(edit => edit.Apply(native: native, op: gate)).As()).As()
                           .Map(static _ => unit)),
                   recordsUndo: plan.RecordsUndo)
               select unit;
    }
}

```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]             | [OWNER]            | [FORM]                                                    | [ENTRY]                    |
| :-----: | :-------------------- | :----------------- | :-------------------------------------------------------- | :------------------------- |
|  [01]   | material resolution   | scope plus realm   | overload union plus row-owned material family             | `MaterialAsk.Resolve`      |
|  [02]   | mesh discriminant     | `MeshKind`         | the folder's one crossing of the host `MeshType`          | `MeshKind.Of`              |
|  [03]   | detached identity     | `MaterialStamp`    | generated host projection of resolved id and name         | `MaterialMap.Detach`       |
|  [04]   | render-support reads  | `MaterialAsk<T>`   | self-typed request fixing its own answer                  | `Materials.Ask`            |
|  [05]   | meshing policy        | `RenderMeshPolicy` | normalized encoding with call-scoped native custody       | cache asks and edits       |
|  [06]   | batch meshing         | `MeshBatch`        | request and settled fact as one family, `MeshRun` custody | `MaterialAsk.Harvest`      |
|  [07]   | owned answers         | `MeshPieces`       | the two piece-bearing reads, each its own disposer        | `MaterialAsk.CachedMeshes` |
|  [08]   | commit demand         | `CommitDemand`     | undo and solo rows the spine and the program both read    | `MaterialProgram.Of`       |
|  [09]   | render-support writes | `MaterialEdit`     | demand-set edits over one admitted program                | `Materials.Commit`         |

## [06]-[RESEARCH]

(none)
