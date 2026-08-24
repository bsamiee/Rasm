# [RASM_RHINO_RENDER_KINDS]

`RenderKind` owns material, texture, and environment specialization over one `RenderContent` lifecycle, and the photometric-file payload the light rail defers here. Material projections remain callback-bounded borrows, texture state replays every writable axis through one roster fold while retaining read-only simulation evidence, environment state detaches every native carrier, and each mint exits as an owned `Lease<RenderContent>`.

## [01]-[INDEX]

- [02]-[MATERIAL_BRIDGE]: `MaterialMint`, `MaterialBridge`, `SlotUsage`, and the `MaterialScent` classification fold.
- [03]-[TEXTURE]: `TextureConfig` total-state configuration over the `TextureAxis` write roster, `TextureTraits`, `TextureFacsimile` over `FacsimileAxis`, the shared `AxisFold`, and the bitmap mint/export pair.
- [04]-[ENVIRONMENT]: `EnvironmentState` and the bake/mint pair over `SimulatedEnvironment`.
- [05]-[PHOTOMETRIC]: `PhotometricDialect`, the `PhotometricFile` payload, serializer discovery, and the material-graph attach.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[MATERIAL_BRIDGE]

- Owner: `MaterialMint` carries each table address and admits it against the live material roster inside the document-aware mint seam. `MaterialBridge` bounds baked and physically based projections to one callback. `SlotUsage` detaches standard-slot state and its native texture-type correspondence, and `MaterialScent` derives classification from predicate rows.
- Law: `MaterialBridge.Pbr` routes `ToMaterial`/`ConvertToPhysicallyBased` onto `Rhino.DocObjects.PhysicallyBasedMaterial`; each projection remains borrowed for one window.
- Law: `Rhino.Render.PhysicallyBasedMaterial` is whole-class obsolete and never enters the design — it exists, so the simple name is ambiguous under this prelude and the document type is spelled `global::`-qualified at every fence site.
- Law: `SetChild(renderContent, childSlotName)` is the live host spelling; the `ChangeContexts` overload is obsolete and the reason already rides the enclosing `ChangeScope.Write`, so no attach carries a context argument.
- Law: `TextureType` is `Rhino.DocObjects`', never a `RenderMaterial` nested type — `TextureTypeFromSlot`/`SlotFromTextureType` are statics on `RenderMaterial` returning and taking that document enum, so the qualified spelling resolves nothing.
- Law: slot vocabulary is the native `StandardChildSlots` — the PBR slot roster including its aliasing rows is host truth the seam consumes; a wrapper row per slot is the deleted form, and `SlotFromTextureType`/`TextureTypeFromSlot` answer the type-to-slot correspondence where a consumer needs it.
- Law: an absent slot texture carries NO grant — `SlotUsage.Texture` is one option over the whole `(id, posture, amount)` product, so a slot with no texture cannot publish an amount or an on-posture, and the corner a three-field record admits is unrepresentable.
- Law: import residency is a ROW, never a `bool reference` argument — `MaterialResidency` names both corners of the host's import flag at the call site.
- Law: assignment is operation-rail work — `AssignTo` over resolved object references with its sub-face and block choices rides the registry page's `ContentOp.Assign` case, so this page carries no table mutation.
- Law: `MaterialScent.CensusOf` narrows by row, never by column — both host predicates are independent native calls, so a mark costs both reads and a caller wanting one classification names that row rather than paying the whole roster. Marks holding NEITHER form are barred by the vocabulary's own law, so the census filter and the carrier state one fact instead of two.
- Growth: a new scent is one `MaterialScent` row with its two predicate columns; a new mint form is one `MaterialMint` case.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderMaterial.FromMaterial`/`CreateBasicMaterial`/`CreateImportedMaterial`, `ToMaterial`, `ConvertToPhysicallyBased`, `GetTextureFromUsage`/`GetTextureOnFromUsage`/`GetTextureAmountFromUsage`/`TextureChildSlotName`, `RenderMaterial.StandardChildSlots`, `TextureTypeFromSlot`, `RenderTexture.TextureGeneration`, the `SmellsLike*`/`SmellsLikeTextured*` predicate pairs); `api-rhinocommon-objects.md` (`Material`, `PhysicallyBasedMaterial`, `TextureType`); kernel `Domain/rails` (`Op`, `Lease<T>`), `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw.Forbidden`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `guard`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[UseDelegateFromConstructor]`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;
using Thinktecture;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class MaterialResidency {
    public static readonly MaterialResidency Local = new(key: false);
    public static readonly MaterialResidency Linked = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class UsagePosture {
    public static readonly UsagePosture Off = new(key: false);
    public static readonly UsagePosture On = new(key: true);

    internal static UsagePosture Of(bool native) => native ? On : Off;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialMint {
    private MaterialMint() { }
    private sealed record DirectCase(int MaterialIndex) : MaterialMint;
    private sealed record BasicCase(int MaterialIndex) : MaterialMint;
    private sealed record ImportedCase(int MaterialIndex, MaterialResidency Residency) : MaterialMint;

    public static MaterialMint Direct(int materialIndex) =>
        new DirectCase(MaterialIndex: materialIndex);

    public static MaterialMint Basic(int materialIndex) =>
        new BasicCase(MaterialIndex: materialIndex);

    public static MaterialMint Imported(int materialIndex, MaterialResidency residency) =>
        new ImportedCase(MaterialIndex: materialIndex, Residency: residency);

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            directCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                static (source, document) => RenderMaterial.FromMaterial(material: source, doc: document)),
            basicCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                static (source, document) => RenderMaterial.CreateBasicMaterial(material: source, doc: document)),
            importedCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                (source, document) => RenderMaterial.CreateImportedMaterial(
                    material: source, doc: document, reference: mint.Residency.Key)));

    private static Fin<Lease<RenderContent>> Minted(
        (RhinoDoc Document, Op Op) ctx, int index, Func<Material, RhinoDoc, RenderContent?> route) =>
        from _index in guard(index >= 0 && index < ctx.Document.Materials.Count, ctx.Op.InvalidInput()).ToFin()
        from source in ctx.Op.Catch(() => Optional(ctx.Document.Materials[index]).ToFin(Fail: ctx.Op.MissingContext()))
        from minted in Seam.Minted(mint: () => route(source, ctx.Document), key: ctx.Op)
        select minted;
}

// Both host predicates are INDEPENDENT native entry points (`Rdk_RenderMaterial_SmellsLike` against
// `Rdk_RenderMaterial_SmellsLikeTextured`), so neither implies the other and no column short-circuits — a mark needs
// both reads. The empty corner is the one illegal one: a scent holding neither form is not a classification, so the
// law bars the empty set and the census cannot publish a row that says nothing.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScentForm : ICapability<ScentForm> {
    public static readonly ScentForm Plain = new(key: "plain", bit: 1);
    public static readonly ScentForm Textured = new(key: "textured", bit: 2);

    internal int Bit { get; }

    public static CapabilityLaw<ScentForm> Law => law.Value;
    private static readonly Lazy<CapabilityLaw<ScentForm>> law = new(static () =>
        CapabilityLaw<ScentForm>.Forbidden(barred: Seq(CapabilitySet<ScentForm>.None)));

    internal static Fin<CapabilitySet<ScentForm>> Of(bool plain, bool textured, Op key) =>
        CapabilitySet<ScentForm>
            .OfMask(mask: (plain ? Plain.Bit : 0) | (textured ? Textured.Bit : 0), bit: static row => row.Bit, key: key)
            .Bind(held => Law.Admit(held: held));
}

[SmartEnum]
public sealed partial class MaterialScent {
    public static readonly MaterialScent Plaster = new(plain: static m => m.SmellsLikePlaster, textured: static m => m.SmellsLikeTexturedPlaster);
    public static readonly MaterialScent Paint = new(plain: static m => m.SmellsLikePaint, textured: static m => m.SmellsLikeTexturedPaint);
    public static readonly MaterialScent Metal = new(plain: static m => m.SmellsLikeMetal, textured: static m => m.SmellsLikeTexturedMetal);
    public static readonly MaterialScent Plastic = new(plain: static m => m.SmellsLikePlastic, textured: static m => m.SmellsLikeTexturedPlastic);
    public static readonly MaterialScent Gem = new(plain: static m => m.SmellsLikeGem, textured: static m => m.SmellsLikeTexturedGem);
    public static readonly MaterialScent Glass = new(plain: static m => m.SmellsLikeGlass, textured: static m => m.SmellsLikeTexturedGlass);

    [UseDelegateFromConstructor]
    private partial bool Plain(RenderMaterial material);

    [UseDelegateFromConstructor]
    private partial bool Textured(RenderMaterial material);

    // Both row reads fold into one held set, and the law's own refusal IS the filter: a mark holding neither
    // form fails admission and drops, so no downstream predicate re-asks whether the mark carries anything.
    internal static ScentCensus CensusOf(RenderMaterial material, Seq<MaterialScent> wanted = default, Op? key = null) {
        Op op = key.OrDefault();
        return new ScentCensus(Rows: (wanted.IsEmpty ? toSeq(Items) : wanted.Distinct())
            .Map(row => ScentForm
                .Of(plain: row.Plain(material: material), textured: row.Textured(material: material), key: op)
                .Map(held => new ScentMark(Scent: row, Forms: held))
                .ToOption())
            .Somes()
            .Strict());
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ScentMark(MaterialScent Scent, CapabilitySet<ScentForm> Forms);

public sealed record ScentCensus(Seq<ScentMark> Rows) : IDetachedDocumentResult;

public readonly record struct SlotUsage(
    RenderMaterial.StandardChildSlots Slot,
    TextureType TextureType,
    Option<(Guid Texture, UsagePosture Posture, double Amount)> Grant,
    string SlotName) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterialBridge {
    internal static Fin<TOut> Bake<TOut>(
        RenderMaterial material, RenderTexture.TextureGeneration generation, Func<Material, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            using Material baked = material.ToMaterial(tg: generation);
            return Optional(baked).ToFin(Fail: key.InvalidResult()).Bind(borrow);
        });

    internal static Fin<TOut> Pbr<TOut>(
        RenderMaterial material, RenderTexture.TextureGeneration generation, Func<global::Rhino.DocObjects.PhysicallyBasedMaterial, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            global::Rhino.DocObjects.PhysicallyBasedMaterial projected = material.ConvertToPhysicallyBased(tg: generation);
            return Optional(projected).ToFin(Fail: key.InvalidResult()).Bind(active => {
                using Material backing = active.Material;
                return borrow(active);
            });
        });

    internal static Fin<SlotUsage> Usage(RenderMaterial material, RenderMaterial.StandardChildSlots slot, Op key) =>
        key.Catch(() => Fin.Succ(value: new SlotUsage(
            Slot: slot,
            TextureType: RenderMaterial.TextureTypeFromSlot(slot: slot),
            Grant: Optional(material.GetTextureFromUsage(slot: slot)).Map(texture => (
                Texture: texture.Id,
                Posture: UsagePosture.Of(native: material.GetTextureOnFromUsage(slot: slot)),
                Amount: material.GetTextureAmountFromUsage(slot: slot))),
            SlotName: material.TextureChildSlotName(slot: slot))));
}
```

## [03]-[TEXTURE]

- Owner: `TextureConfig` is the replayable live-content state, including `TextureGraphInfo`, and `TextureAxis` rows own its per-axis host writes. `TextureTraits` detaches classification. `SimulatedMapping` owns direct and environment-aware mapping writes, `EnvironmentProjection` closes the host's projection-to-environment correspondence, `TextureFacsimile` carries reconstructible simulation state and read-only transform provenance over the `FacsimileAxis` rows, `TextureMint` admits each native source, and `TextureExport` confirms image egress.
- Law: configuration writes are total state, never a patch — `Apply` re-asserts every field under one `ChangeReason`, so an absent field cannot silently clear and the write is replayable from the record alone.
- Law: `AxisFold.Apply` is the ONE write-roster fold on this branch — it takes any `string`-keyed row roster, prefixes the refusing row's own key onto the fault, and answers `Unit`; a second per-roster copy of the same traverse is the deleted form, and `TextureAxis`, `FacsimileAxis`, and every sibling page's `*State.Apply` roster take it unchanged.
- Law: `TextureAxis` and `FacsimileAxis` are the write rosters — every row answers `Unit` so the fold is uniform, a conditional host pair is one row whose option is its own predicate, and a straight-line setter run beside the roster is the deleted form.
- Law: boolean texture axes ride ONE `CapabilitySet<TextureToggle>` whose row keys are the `TextureAxis` keys they write, so a boundary reading a persisted toggle word resolves it through `Admits(key)` without a second correspondence table. This collapse LOSES per-toggle compile-time exhaustiveness; the axis rows buy it back, naming their toggle row explicitly and breaking loudly when one is retired.
- Law: read-only `LocalMappingTransform` and `OriginalFilename` never enter writable state; local mapping reconstructs from the admitted UVW fields, while original filename remains observation-only host provenance.
- Law: `EnvironmentProjection` closes the eight host projection modes that name an environment mapping, and the simulated `Emap` posture is the one fallback row the key column cannot carry — it reads the OTHER host enum, so it stays the named `else` inside the one owner rather than an inline arm at the call site. Unrostered projections read as legal absence, not a fault, so this correspondence answers `Option` and takes no row-read rail.
- Boundary: live evaluation (`CreateEvaluator`) and the bake gate (`SimulateTexture`) are the Display render page's `TextureBake` owner; this page configures the content, that one evaluates it, and the two never merge.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderTexture` get/set pairs for projection, wrap, repeat, offset, rotation, mapping channel, environment mode, graph info, preview and viewport flags; `PixelSize2`, `LocalMappingTransform`, `GetLocalMappingType`, `GetInternalEnvironmentMappingMode`, `IsHdrCapable`/`IsLinear`/`IsNormalMap`/`IsImageBased`, `NewBitmapTexture` both arities, `SaveAsImage`, `SimulatedTexture` writable axes, `SetMappingChannelAndProjectionMode`); `api-rhinocommon-geometry.md` (`Vector2d`, `Vector3d`, `Transform`); `api-rhinocommon-display.md` (`Color4f`); kernel `Domain/rails` (`Op.Catch`, `Op.Side`, `Lease<T>`), `Domain/validation` (`ICapability`, `CapabilitySet`, `ISmartEnum`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`, `MapFail`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[UseDelegateFromConstructor]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Five boolean texture axes ride ONE capability column. Each row's key IS its `TextureAxis` key, so a persisted or
// wire toggle word resolves through the kernel `Admits(string)` boundary arm and no second correspondence exists.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextureToggle : ICapability<TextureToggle> {
    public static readonly TextureToggle RepeatLocked = new(key: "repeat-locked", holds: static texture => texture.GetRepeatLocked());
    public static readonly TextureToggle OffsetLocked = new(key: "offset-locked", holds: static texture => texture.GetOffsetLocked());
    public static readonly TextureToggle PreviewIn3D = new(key: "preview-3d", holds: static texture => texture.GetPreviewIn3D());
    public static readonly TextureToggle PreviewLocalMapping = new(key: "preview-local-mapping", holds: static texture => texture.GetPreviewLocalMapping());
    public static readonly TextureToggle DisplayInViewport = new(key: "display-in-viewport", holds: static texture => texture.GetDisplayInViewport());

    [UseDelegateFromConstructor]
    private partial bool Holds(RenderTexture texture);

    internal static CapabilitySet<TextureToggle> Of(RenderTexture texture) =>
        CapabilitySet<TextureToggle>.Of(Items.Where(row => row.Holds(texture: texture)).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextureTrait : ICapability<TextureTrait> {
    public static readonly TextureTrait HdrCapable = new(key: "hdr-capable", holds: static texture => texture.IsHdrCapable());
    public static readonly TextureTrait Linear = new(key: "linear", holds: static texture => texture.IsLinear());
    public static readonly TextureTrait NormalMap = new(key: "normal-map", holds: static texture => texture.IsNormalMap());
    public static readonly TextureTrait ImageBased = new(key: "image-based", holds: static texture => texture.IsImageBased());

    [UseDelegateFromConstructor]
    private partial bool Holds(RenderTexture texture);

    internal static CapabilitySet<TextureTrait> Of(RenderTexture texture) =>
        CapabilitySet<TextureTrait>.Of(Items.Where(row => row.Holds(texture: texture)).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FacsimileTrait : ICapability<FacsimileTrait> {
    public static readonly FacsimileTrait Repeating = new(key: "repeating", holds: static simulated => simulated.Repeating);
    public static readonly FacsimileTrait Filtered = new(key: "filtered", holds: static simulated => simulated.Filtered);

    [UseDelegateFromConstructor]
    private partial bool Holds(SimulatedTexture simulated);

    internal static CapabilitySet<FacsimileTrait> Of(SimulatedTexture simulated) =>
        CapabilitySet<FacsimileTrait>.Of(Items.Where(row => row.Holds(simulated: simulated)).ToArray());
}

// Each writable texture axis is one row over the whole state, so a mid-apply host refusal names the axis that refused
// instead of vanishing into a straight-line setter run. `SetGraphInfo` takes no change context, so its row ignores the
// reason column the rest of the roster consumes.
[SmartEnum<string>]
public sealed partial class TextureAxis {
    public static readonly TextureAxis Projection = new("projection",
        static (texture, state, reason) => Op.Side(() => texture.SetProjectionMode(state.Projection, reason)));
    public static readonly TextureAxis Wrap = new("wrap",
        static (texture, state, reason) => Op.Side(() => texture.SetWrapType(state.Wrap, reason)));
    public static readonly TextureAxis Repeat = new("repeat",
        static (texture, state, reason) => Op.Side(() => texture.SetRepeat(state.Repeat, reason)));
    public static readonly TextureAxis RepeatLocked = new("repeat-locked",
        static (texture, state, reason) => Op.Side(() => texture.SetRepeatLocked(state.Toggles.Admits(TextureToggle.RepeatLocked), reason)));
    public static readonly TextureAxis Offset = new("offset",
        static (texture, state, reason) => Op.Side(() => texture.SetOffset(state.Offset, reason)));
    public static readonly TextureAxis OffsetLocked = new("offset-locked",
        static (texture, state, reason) => Op.Side(() => texture.SetOffsetLocked(state.Toggles.Admits(TextureToggle.OffsetLocked), reason)));
    public static readonly TextureAxis Rotation = new("rotation",
        static (texture, state, reason) => Op.Side(() => texture.SetRotation(state.Rotation, reason)));
    public static readonly TextureAxis MappingChannel = new("mapping-channel",
        static (texture, state, reason) => Op.Side(() => texture.SetMappingChannel(state.Channel, reason)));
    public static readonly TextureAxis EnvironmentMode = new("environment-mode",
        static (texture, state, reason) => Op.Side(() => texture.SetEnvironmentMappingMode(state.EnvironmentMode, reason)));
    public static readonly TextureAxis Graph = new("graph",
        static (texture, state, _) => Op.Side(() => texture.SetGraphInfo(state.Graph)));
    public static readonly TextureAxis PreviewIn3D = new("preview-3d",
        static (texture, state, reason) => Op.Side(() => texture.SetPreviewIn3D(state.Toggles.Admits(TextureToggle.PreviewIn3D), reason)));
    public static readonly TextureAxis PreviewLocalMapping = new("preview-local-mapping",
        static (texture, state, reason) => Op.Side(() => texture.SetPreviewLocalMapping(state.Toggles.Admits(TextureToggle.PreviewLocalMapping), reason)));
    public static readonly TextureAxis DisplayInViewport = new("display-in-viewport",
        static (texture, state, reason) => Op.Side(() => texture.SetDisplayInViewport(state.Toggles.Admits(TextureToggle.DisplayInViewport), reason)));

    [UseDelegateFromConstructor]
    internal partial Unit Write(RenderTexture texture, TextureConfig state, RenderContent.ChangeContexts reason);
}

// One axis is conditional: the host pairs `HasTransparentColor` with the colour and its sensitivity, so the option IS
// its row predicate, and both writes ride inside it rather than an `if`/`else` outside the roster.
[SmartEnum<string>]
public sealed partial class FacsimileAxis {
    public static readonly FacsimileAxis Filename = new("filename",
        static (simulated, state) => Op.Side(() => simulated.Filename = state.Filename.IfNone(string.Empty)));
    public static readonly FacsimileAxis Repeat = new("repeat",
        static (simulated, state) => Op.Side(() => simulated.Repeat = state.Repeat));
    public static readonly FacsimileAxis Offset = new("offset",
        static (simulated, state) => Op.Side(() => simulated.Offset = state.Offset));
    public static readonly FacsimileAxis Rotation = new("rotation",
        static (simulated, state) => Op.Side(() => simulated.Rotation = state.Rotation));
    public static readonly FacsimileAxis Repeating = new("repeating",
        static (simulated, state) => Op.Side(() => simulated.Repeating = state.Traits.Admits(FacsimileTrait.Repeating)));
    public static readonly FacsimileAxis Mapping = new("mapping",
        static (simulated, state) => state.Mapping.Apply(texture: simulated));
    public static readonly FacsimileAxis Filtered = new("filtered",
        static (simulated, state) => Op.Side(() => simulated.Filtered = state.Traits.Admits(FacsimileTrait.Filtered)));
    public static readonly FacsimileAxis Transparency = new("transparency",
        static (simulated, state) => Op.Side(() => {
            simulated.HasTransparentColor = state.Transparency.IsSome;
            _ = state.Transparency.Iter(row => {
                simulated.TransparentColor = row.Color;
                simulated.TransparentColorSensitivity = row.Sensitivity;
            });
        }));

    [UseDelegateFromConstructor]
    internal partial Unit Write(SimulatedTexture simulated, TextureFacsimile state);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class AxisFold {
    // This branch folds every write roster here and preserves the exact failure returned by the host boundary.
    internal static Fin<Unit> Apply<TRow, TTarget, TState>(TTarget target, TState state, Func<TRow, TTarget, TState, Unit> write, Op key)
        where TRow : class, ISmartEnum<string, TRow, ValidationError> =>
        toSeq(TRow.Items)
            .TraverseM(row => key.Catch(() => Fin.Succ(value: write(arg1: row, arg2: target, arg3: state))))
            .As()
            .Map(static _ => unit);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record TextureConfig(
    TextureProjectionMode Projection,
    TextureWrapType Wrap,
    Vector3d Repeat,
    Vector3d Offset,
    Vector3d Rotation,
    int Channel,
    TextureEnvironmentMappingMode EnvironmentMode,
    TextureGraphInfo Graph,
    CapabilitySet<TextureToggle> Toggles) : IDetachedDocumentResult {
    public static Fin<TextureConfig> Of(RenderTexture texture, Op key) => key.Catch(() => {
        TextureGraphInfo graph = new();
        texture.GraphInfo(ref graph);
        return Fin.Succ(value: new TextureConfig(
            Projection: texture.GetProjectionMode(),
            Wrap: texture.GetWrapType(),
            Repeat: texture.GetRepeat(),
            Offset: texture.GetOffset(),
            Rotation: texture.GetRotation(),
            Channel: texture.GetMappingChannel(),
            EnvironmentMode: texture.GetEnvironmentMappingMode(),
            Graph: graph,
            Toggles: TextureToggle.Of(texture: texture)));
    });

    internal Fin<Unit> Apply(RenderTexture texture, ChangeReason reason, Op key) {
        TextureConfig self = this;
        return ChangeScope.Write(content: texture, reason: reason, key: key, body: _ =>
            AxisFold.Apply<TextureAxis, RenderTexture, (TextureConfig State, RenderContent.ChangeContexts Reason)>(
                target: texture,
                state: (State: self, Reason: reason.Native),
                write: static (row, target, state) => row.Write(texture: target, state: state.State, reason: state.Reason),
                key: key));
    }
}

// --- [TYPES] --------------------------------------------------------------------------------
// Host truth: an environment mapping is named through EITHER the texture projection mode or the simulated
// projection posture. Eight projection modes carry it as a key column; `Emap` on the OTHER enum is the one row a key
// column cannot hold, so it stays the named fallback here rather than an inline arm at the capture site.
[SmartEnum<TextureProjectionModes>]
public sealed partial class EnvironmentProjection {
    public static readonly EnvironmentProjection Box = new(
        key: TextureProjectionModes.EnvironmentMapBox, mode: SimulatedTexture.EnvironmentMappingModes.Box);
    public static readonly EnvironmentProjection LightProbe = new(
        key: TextureProjectionModes.EnvironmentMapLightProbe, mode: SimulatedTexture.EnvironmentMappingModes.Lightprobe);
    public static readonly EnvironmentProjection Spherical = new(
        key: TextureProjectionModes.EnvironmentMapSpherical, mode: SimulatedTexture.EnvironmentMappingModes.Spherical);
    public static readonly EnvironmentProjection Cube = new(
        key: TextureProjectionModes.EnvironmentMapCube, mode: SimulatedTexture.EnvironmentMappingModes.Cubemap);
    public static readonly EnvironmentProjection VerticalCross = new(
        key: TextureProjectionModes.EnvironmentMapVCrossCube, mode: SimulatedTexture.EnvironmentMappingModes.VerticalCrossCubemap);
    public static readonly EnvironmentProjection HorizontalCross = new(
        key: TextureProjectionModes.EnvironmentMapHCrossCube, mode: SimulatedTexture.EnvironmentMappingModes.HorizontalCrossCubemap);
    public static readonly EnvironmentProjection Hemispherical = new(
        key: TextureProjectionModes.EnvironmentMapHemispherical, mode: SimulatedTexture.EnvironmentMappingModes.Hemispherical);
    public static readonly EnvironmentProjection Emap = new(
        key: TextureProjectionModes.EnvironmentMapEmap, mode: SimulatedTexture.EnvironmentMappingModes.Emap);

    internal SimulatedTexture.EnvironmentMappingModes Mode { get; }

    internal static Option<SimulatedTexture.EnvironmentMappingModes> Of(
        TextureProjectionModes projection, SimulatedTexture.ProjectionModes simulated) =>
        TryGet(projection, out EnvironmentProjection? row)
            ? Some(row.Mode)
            : simulated == SimulatedTexture.ProjectionModes.Emap
                ? Some(SimulatedTexture.EnvironmentMappingModes.Automatic)
                : Option<SimulatedTexture.EnvironmentMappingModes>.None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimulatedMapping {
    private SimulatedMapping() { }
    private sealed record DirectCase(SimulatedTexture.ProjectionModes Projection, int Channel) : SimulatedMapping;
    private sealed record EnvironmentCase(
        SimulatedTexture.ProjectionModes Projection,
        int Channel,
        SimulatedTexture.EnvironmentMappingModes Environment) : SimulatedMapping;

    // Host truth: a simulated texture's mapping channel is zero-based and zero is the ordinary default, so this
    // owner guards `>= 0` and never composes the positive-only `MappingChannel` the object-mapping rail admits.
    public static Fin<SimulatedMapping> Of(
        SimulatedTexture.ProjectionModes projection,
        int channel,
        Option<SimulatedTexture.EnvironmentMappingModes> environment = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(channel >= 0, op.InvalidInput()).ToFin()
               select environment.Match(
                   Some: mode => (SimulatedMapping)new EnvironmentCase(
                       Projection: projection, Channel: channel, Environment: mode),
                   None: () => new DirectCase(Projection: projection, Channel: channel));
    }

    internal static Fin<SimulatedMapping> Capture(SimulatedTexture texture, Op key) {
        using Texture projected = texture.Texture();
        return Of(
            projection: texture.ProjectionMode,
            channel: texture.MappingChannel,
            environment: EnvironmentProjection.Of(
                projection: projected.ProjectionMode, simulated: texture.ProjectionMode),
            key: key);
    }

    internal Unit Apply(SimulatedTexture texture) =>
        Switch(
            state: texture,
            directCase: static (target, mapping) => Op.Side(() => {
                target.ProjectionMode = mapping.Projection;
                target.MappingChannel = mapping.Channel;
            }),
            environmentCase: static (target, mapping) => Op.Side(() =>
                target.SetMappingChannelAndProjectionMode(mapping.Projection, mapping.Channel, mapping.Environment)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct TextureTraits(
    Option<(int Width, int Height, int Depth)> Texels,
    Transform LocalTransform,
    RenderTexture.eLocalMappingType LocalMappingType,
    TextureEnvironmentMappingMode InternalEnvironmentMode,
    CapabilitySet<TextureTrait> Traits) : IDetachedDocumentResult {
    public static Fin<TextureTraits> Of(RenderTexture texture, Op key) =>
        key.Catch(() => Fin.Succ(value: new TextureTraits(
            Texels: Optional(texture.PixelSize2),
            LocalTransform: texture.LocalMappingTransform,
            LocalMappingType: texture.GetLocalMappingType(),
            InternalEnvironmentMode: texture.GetInternalEnvironmentMappingMode(),
            Traits: TextureTrait.Of(texture: texture))));
}

public sealed record TextureFacsimile(
    Option<string> Filename,
    Option<string> OriginalFilename,
    Transform LocalTransform,
    Vector2d Repeat,
    Vector2d Offset,
    double Rotation,
    SimulatedMapping Mapping,
    Option<(Color4f Color, double Sensitivity)> Transparency,
    CapabilitySet<FacsimileTrait> Traits) : IDetachedDocumentResult {
    internal static Fin<TextureFacsimile> Of(SimulatedTexture simulated, Op key) =>
        SimulatedMapping.Capture(texture: simulated, key: key).Map(mapping => new TextureFacsimile(
            Filename: Op.Text(simulated.Filename),
            OriginalFilename: Op.Text(simulated.OriginalFilename),
            LocalTransform: simulated.LocalMappingTransform,
            Repeat: simulated.Repeat,
            Offset: simulated.Offset,
            Rotation: simulated.Rotation,
            Mapping: mapping,
            Transparency: simulated.HasTransparentColor
                ? Some((simulated.TransparentColor, simulated.TransparentColorSensitivity))
                : Option<(Color4f, double)>.None,
            Traits: FacsimileTrait.Of(simulated: simulated)));

    internal Fin<Unit> Apply(SimulatedTexture simulated, Op key) {
        TextureFacsimile self = this;
        return AxisFold.Apply<FacsimileAxis, SimulatedTexture, TextureFacsimile>(
            target: simulated,
            state: self,
            write: static (row, target, state) => row.Write(simulated: target, state: state),
            key: key);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureMint {
    private TextureMint() { }
    private sealed record BitmapCase(System.Drawing.Bitmap Value) : TextureMint;
    private sealed record SimulatedCase(TextureFacsimile Value) : TextureMint;

    public static Fin<TextureMint> From(System.Drawing.Bitmap value, Op? key = null) =>
        key.OrDefault().Need(value).Map(static admitted => (TextureMint)new BitmapCase(Value: admitted));

    public static Fin<TextureMint> From(TextureFacsimile value, Op? key = null) =>
        key.OrDefault().Need(value).Map(static admitted => (TextureMint)new SimulatedCase(Value: admitted));

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            bitmapCase: static (ctx, mint) =>
                Seam.Minted(mint: () => RenderTexture.NewBitmapTexture(bitmap: mint.Value, doc: ctx.Document), key: ctx.Op),
            simulatedCase: static (ctx, mint) => ctx.Op.Catch(() => {
                using SimulatedTexture carrier = new(ctx.Document);
                return mint.Value.Apply(simulated: carrier, key: ctx.Op)
                    .Bind(_ => Seam.Minted(
                        mint: () => RenderTexture.NewBitmapTexture(texture: carrier, doc: ctx.Document), key: ctx.Op));
            }));
}

public static class TextureExport {
    internal static Fin<Unit> Export(RenderTexture texture, string path, int width, int height, int depth, Op key) =>
        from admitted in key.AcceptText(value: path)
        from _ in guard(width > 0 && height > 0 && depth > 0, key.InvalidInput())
        from confirmed in key.Catch(() => key.Confirm(success: texture.SaveAsImage(admitted, width, height, depth)))
        select unit;
}
```

## [04]-[ENVIRONMENT]

- Owner: `EnvironmentState` detaches background color, projection, and image state. `Bake` contains the simulation lease under a `BakeScope` posture, and `Mint` reconstructs the document-aware carriers before yielding an owned content lease.
- Law: `EnvironmentState.Bake` and `Mint` are the only sites holding a `SimulatedEnvironment`; the carrier has a public parameterless constructor, is `IDisposable`, and never crosses its window.
- Law: the bake posture is a ROW — `BakeScope` names the host's data-only and full-simulation corners, so no caller passes a bare boolean into a native simulation call.
- Law: colour crosses at the kernel owner in BOTH directions — `PerceptualColor.OfHost` admits the host background colour whole and `ToDrawing` returns it under a gamut policy on the rail, so no local quantizer exists and a chroma outside the host gamut refuses instead of clipping silently.
- Law: environment image absence reads through `SimulatedTexture.ConstPointer()`, never `Optional` — the `BackgroundImage` getter mints a fresh parent-backed facsimile per read and is never null, so a null projection admits an empty environment as an imaged one.
- Law: environment duplication travels as one detached value; simulation-only transform provenance remains evidence while every host-writable image axis replays.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderEnvironment.SimulateEnvironment`, `NewBasicEnvironment`, `SimulatedEnvironment.BackgroundColor`/`BackgroundImage`/`BackgroundProjection`, `SimulatedTexture.ConstPointer`); kernel `Numerics/atoms` (`PerceptualColor.OfHost`, `PerceptualColor.ToDrawing`), `Domain/rails` (`Lease<T>`, `Op.Catch`); LanguageExt.Core (`Fin`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class BakeScope {
    public static readonly BakeScope Full = new(key: false);
    public static readonly BakeScope DataOnly = new(key: true);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record EnvironmentState(
    PerceptualColor Background,
    SimulatedEnvironment.BackgroundProjections Projection,
    Option<TextureFacsimile> Image) : IDetachedDocumentResult {
    internal static Fin<EnvironmentState> Bake(RenderEnvironment environment, BakeScope scope, Op key) =>
        key.Catch(() => {
            using SimulatedEnvironment simulated = environment.SimulateEnvironment(isForDataOnly: scope.Key);
            return Optional(simulated).ToFin(Fail: key.InvalidResult()).Bind(active => {
                // Host truth: `BackgroundImage` MINTS a parent-backed `SimulatedTexture` on every read and never answers
                // null, so `Optional(image)` is always `Some`; the public `ConstPointer()` resolves through the parent and
                // answers `IntPtr.Zero` when the environment holds no image, which is the only real absence discriminant.
                using SimulatedTexture image = active.BackgroundImage;
                Fin<Option<TextureFacsimile>> detached = image.ConstPointer() == IntPtr.Zero
                    ? Fin.Succ(Option<TextureFacsimile>.None)
                    : TextureFacsimile.Of(simulated: image, key: key).Map(static value => Some(value));
                return from detachedImage in detached
                       from background in PerceptualColor.OfHost(host: active.BackgroundColor, key: key)
                       select new EnvironmentState(
                           Background: background,
                           Projection: active.BackgroundProjection,
                           Image: detachedImage);
            });
        });

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) {
        EnvironmentState self = this;
        return key.Catch(() => {
            using SimulatedEnvironment simulated = new();
            return from background in self.Background.ToDrawing(key: key)
                   from _ in key.Catch(() => {
                       simulated.BackgroundColor = background;
                       simulated.BackgroundProjection = self.Projection;
                       return Fin.Succ(value: unit);
                   })
                   from minted in self.Image.Match(
                       Some: facsimile => Imaged(facsimile: facsimile, simulated: simulated, document: document, key: key),
                       None: () => Basic(simulated: simulated, document: document, key: key))
                   select minted;
        });
    }

    private static Fin<Lease<RenderContent>> Imaged(
        TextureFacsimile facsimile, SimulatedEnvironment simulated, RhinoDoc document, Op key) {
        using SimulatedTexture reconstructed = new(document);
        return from _ in facsimile.Apply(simulated: reconstructed, key: key)
               from __ in key.Catch(() => { simulated.BackgroundImage = reconstructed; return Fin.Succ(value: unit); })
               from minted in Basic(simulated: simulated, document: document, key: key)
               select minted;
    }

    private static Fin<Lease<RenderContent>> Basic(SimulatedEnvironment simulated, RhinoDoc document, Op key) =>
        Seam.Minted(mint: () => RenderEnvironment.NewBasicEnvironment(environment: simulated, doc: document), key: key);
}
```

## [05]-[PHOTOMETRIC]

- Owner: `PhotometricDialect` closes the light-distribution file vocabulary by extension and description; `PhotometricFile` is the admitted payload the Objects lights rail defers here; `PhotometricPress` derives one registry `ContentSerializer` program per dialect row for host discovery, and its custody admissions convert owned leases into `ContentTransfer` custody.
- Law: the name is `PhotometricFile`, never `PhotometricWeb` — this owner is a FILE REFERENCE admitted by extension, while the AppUi owner of that spelling is a DECODED candela table; two concepts, and the fork closes at the name rather than at an allowlist row.
- Law: the path is a `FileLocation`, never a raw string — the kernel owner admits the path shape once, so the dialect read, the existence probe, and the serializer program all consume one admitted value.
- Law: the host carries no first-class photometric type — `Rhino.Geometry.Light` ends at intensity and power, so the file travels as texture-kind render content on the light's attached render material, addressed as a `ContentRef` child slot and embedded through the content's own `FilesToEmbed` roster.
- Law: attach ANSWERS the address it seated — `AttachTo` returns the `ContentRef` of the content the host accepted, so the content key downstream readers need is published by the operation that creates it rather than re-derived by a second walk of the child-slot graph.
- Law: attach is one fold — custody rides `Lease<ContentTransfer>`, so the release is the kernel's own and a release refusal AGGREGATES into the attach fault. Arming the slot is the one compensating write: a host refusal restores the prior slot state and the restore's own failure accumulates onto the primary. Any second attach spelling beside it is the deleted form.
- Law: a borrowed lease REFUSES without disposing — `Lease<T>.Borrowed.Dispose()` is a documented no-op because the host still owns the value, so a dispose call on the refusal path is vacuous ceremony.
- Law: a new distribution dialect is one `PhotometricDialect` row; the serializer-program fold covers every row, so discovery, description, and admission cannot drift.
- Boundary: the content class the serializer materializes is the discovering plugin's `CustomRenderContentAttribute` type; this page owns admission, discovery, attach, and the embed census, never the plugin's field layout — field declaration rides the fields page.
- Boundary: the lights rail's photometric reach ends at `Radiance`; `LightEdit` never grows an IES case, and the seam crossing is this page's `PhotometricFile` alone. `AttachTo`'s declared consumer is LANDED — `Objects/lights.md` `IPhotometricRegistry.WebOf` is the one address the emitter holds into this stratum — while `Embedded` and `Serializers` stay open until the `Plugin/lifecycle` serializer Hooks row's registration body composes them; until then the obligation reads open rather than met.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.SetChild`, `ChildSlotOn`/`SetChildSlotOn`, `GetEmbeddedFilesList`, `FilesToEmbed`, `RenderContentSerializer`); kernel `Interaction/asset` (`FileLocation`), `Domain/rails` (`Lease<T>.Acquire`/`Use`, `Op.AcceptText`, `Op.Catch`), `Domain/validation` (`Op.AcceptValidated<TVO>`); `Display/render.md` (`RenderFault`); `Render/registry.md` (`ContentTransfer`, `ContentSerializer`, `SerializerProgram`, `ContentExtension`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[ComplexValueObject]`, `[ValidationError]`).

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhotometricDialect {
    public static readonly PhotometricDialect Ies = new(".ies", "IES photometric distribution");
    public static readonly PhotometricDialect Eulumdat = new(".ldt", "EULUMDAT photometric distribution");
    public static readonly PhotometricDialect CieRecord = new(".cie", "CIE photometric distribution");

    internal string Description { get; }

    internal static Fin<PhotometricDialect> OfPath(FileLocation path, Op key) =>
        key.Catch(() => key.Row<string, PhotometricDialect>(
            System.IO.Path.GetExtension(path.Value).ToLowerInvariant()));
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class PhotometricFile : IDetachedDocumentResult {
    public FileLocation Path { get; }
    public PhotometricDialect Dialect { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FileLocation path,
        ref PhotometricDialect dialect) =>
        validationError = dialect is not null && System.IO.File.Exists(path.Value)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(PhotometricFile) }));

    public static Fin<PhotometricFile> Of(string path, Op? key = null) {
        Op op = key.OrDefault();
        return from location in op.AcceptValidated<FileLocation>(path)
               from dialect in PhotometricDialect.OfPath(path: location, key: op)
               from admitted in op.AcceptValidated<PhotometricFile>(
                   Validate(location, dialect, out PhotometricFile? created), created)
               select admitted;
    }

    // Custody rides the kernel lease for the whole attach: the release is `Use`'s own and its refusal aggregates
    // into the attach fault, so no hand-written success/failure release pair survives.
    internal Fin<ContentRef> AttachTo(RenderContent parent, string childSlot, PhotometricPress press, ChangeReason reason, Op key) {
        PhotometricFile self = this;
        return from slot in key.AcceptText(value: childSlot)
               from lease in press.Materialize(file: self, key: key)
               from custody in PhotometricPress.Custody(lease: lease, key: key)
               from seated in custody.Lease.Use(
                   body: transfer => ChangeScope.Write(content: parent, reason: reason, key: key, body: live =>
                       from prior in key.Catch(() => Fin.Succ(value: live.ChildSlotOn(childSlotName: slot)))
                       from taken in key.Catch(() => {
                               live.SetChildSlotOn(childSlotName: slot, bOn: true, cc: reason.Native);
                               return key.Confirm(success: live.SetChild(renderContent: custody.Content, childSlotName: slot));
                           })
                           .Bind(_ => transfer.Take(key: key))
                           .MapFail(fault => Restored(
                               parent: live, slot: slot, prior: prior, reason: reason, primary: fault, key: key))
                       select taken),
                   key: key)
               from address in ContentRef.Of(id: seated.Id, key: key)
               select address;
    }

    // One compensating write: the slot was armed before the host call, so a refusal puts it back and the
    // restore's own failure ACCUMULATES onto the primary rather than replacing it.
    private static Error Restored(
        RenderContent parent, string slot, bool prior, ChangeReason reason, Error primary, Op key) =>
        key.Catch(() => {
                parent.SetChildSlotOn(childSlotName: slot, bOn: prior, cc: reason.Native);
                return Fin.Succ(value: unit);
            })
            .Map(_ => primary)
            .IfFail(restore => primary + restore);

    internal Seq<string> Embedded(RenderContent content) =>
        toSeq(content.GetEmbeddedFilesList());
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record PhotometricPress(Func<PhotometricFile, RhinoDoc?, Fin<Lease<RenderContent>>> Reader) {
    internal Fin<Lease<RenderContent>> Materialize(PhotometricFile file, Op key, RhinoDoc? document = null) =>
        key.Catch(() => Reader(file, document));

    public Fin<Seq<RenderContentSerializer>> Serializers(
        RetentionPolicy retention,
        Action<Error> record,
        Op? key = null) {
        Op op = key.OrDefault();
        PhotometricPress self = this;
        return from activeRetention in op.Need(retention)
               from activeRecord in op.Need(record)
               from rows in toSeq(PhotometricDialect.Items).TraverseM(dialect =>
                   from extension in ContentExtension.Of(value: dialect.Key, key: op)
                   from serializer in ContentSerializer.Of(program: new SerializerProgram(
                       FileExtension: extension,
                       Kind: ContentKind.Texture,
                       Read: Some<Func<string, Fin<ContentTransfer>>>(path =>
                           self.Read(path: path, record: activeRecord, key: op)),
                       Write: None,
                       LoadMultiple: None,
                       Retention: activeRetention,
                       EnglishDescription: dialect.Description,
                       LocalDescription: dialect.Description), key: op)
                   select (RenderContentSerializer)serializer).As()
               select rows;
    }

    private Fin<ContentTransfer> Read(string path, Action<Error> record, Op key) =>
        (from file in PhotometricFile.Of(path: path, key: key)
         from lease in Materialize(file: file, key: key)
         from transfer in Transfer(lease: lease, key: key)
         select transfer).MapFail(failure => {
             record(failure);
             return failure;
         });

    // Borrowed leases refuse without a dispose call: the kernel's `Borrowed.Dispose()` is a documented no-op
    // because the host still owns the value, so the refusal states the case and nothing else.
    private static Fin<Lease<RenderContent>.Owned> Held(Lease<RenderContent> lease, Op key) =>
        lease is Lease<RenderContent>.Owned owned
            ? Fin.Succ(value: owned)
            : Fin.Fail<Lease<RenderContent>.Owned>(
                error: key.InvalidResult(detail: nameof(Lease<RenderContent>.Borrowed)));

    // Host serializers TAKE the transfer, so this arm hands out the bare custody; the attach path keeps it on a
    // lease because the window is its own.
    internal static Fin<ContentTransfer> Transfer(Lease<RenderContent> lease, Op key) =>
        Held(lease: lease, key: key).Bind(owned => key.Catch(() => Fin.Succ(value: new ContentTransfer(owned: owned))));

    internal static Fin<(RenderContent Content, Lease<ContentTransfer> Lease)> Custody(Lease<RenderContent> lease, Op key) =>
        Held(lease: lease, key: key).Bind(owned =>
            Lease<ContentTransfer>.Acquire(mint: () => new ContentTransfer(owned: owned), key: key)
                .Map(held => (Content: owned.Value, Lease: held)));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                 | [FORM]                                            | [ENTRY]                          |
| :-----: | :--------------------- | :---------------------- | :------------------------------------------------ | :------------------------------- |
|  [01]   | material minting       | `MaterialMint`          | document-aware leased mint                        | `Direct` / `Basic` / `Imported`  |
|  [02]   | material bake and PBR  | `MaterialBridge`        | callback-bounded material projection              | `Bake` / `Pbr` / `Usage`         |
|  [03]   | material class         | `MaterialScent`         | predicate-column rows folded into `ScentCensus`   | `CensusOf(material, wanted)`     |
|  [04]   | scent forms            | `ScentForm`             | `ICapability` pair whose law bars the empty set   | `ScentForm.Law`                  |
|  [05]   | texture configuration  | `TextureConfig`         | total replayable state, toggles as one column     | `Of` / `Apply(texture, reason)`  |
|  [06]   | texture write roster   | `TextureAxis`           | one row per writable host axis                    | `Write` / `Items`                |
|  [07]   | roster fold            | `AxisFold`              | the branch's one keyed write traverse             | `Apply(target, state, write)`    |
|  [08]   | texture classification | `TextureTraits`         | detached local mapping and capability column      | `Of(texture, key)`               |
|  [09]   | baked-texture crossing | `TextureFacsimile`      | replayable facsimile state                        | `Of` / `Apply`                   |
|  [10]   | facsimile write roster | `FacsimileAxis`         | one row per simulated axis, option as predicate   | `Write` / `Items`                |
|  [11]   | environment projection | `EnvironmentProjection` | host projection keyed, simulated `Emap` fallback  | `Of(projection, simulated)`      |
|  [12]   | texture mint/export    | `TextureMint`           | admitted leased texture lifecycle                 | `From` / `Mint`                  |
|  [13]   | environment bake/mint  | `EnvironmentState`      | detached state and document-aware leased mint     | `Bake` / `Mint(document, key)`   |
|  [14]   | photometric payload    | `PhotometricFile`       | dialect-admitted attachment answering its address | `Of` / `AttachTo`                |
|  [15]   | photometric readers    | `PhotometricPress`      | declarative registry serializer roster            | `Serializers(retention, record)` |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
