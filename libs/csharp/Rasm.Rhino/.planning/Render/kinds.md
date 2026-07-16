# [RASM_RHINO_RENDER_KINDS]

Content specializations (`Rasm.Rhino.Render`). Material, texture, and environment read their shared surface from the `RenderContent` owner and add only kind-specific capability here: the material bridge borrows a baked `Material` or a `PhysicallyBasedMaterial` projection for exactly one window each; texture-usage slots read through the native `StandardChildSlots` vocabulary; material classification folds the host scent predicates onto one vocabulary; texture configuration captures every public settable property; and environment simulation crosses through a detached, reconstructible `EnvironmentState`. Mint verbs yield owned leases, live handles die inside the demand window, and native enums remain seam values.

## [01]-[INDEX]

- [02]-[MATERIAL_BRIDGE]: `MaterialMint`, `MaterialBridge`, `SlotUsage`, and the `MaterialScent` classification fold.
- [03]-[TEXTURE]: `TextureConfig` total-state configuration, `TextureTraits`, `TextureFacsimile`, and the bitmap mint/export pair.
- [04]-[ENVIRONMENT]: `EnvironmentState` and the bake/mint pair over `SimulatedEnvironment`.
- [05]-[SURFACE_LEDGER]: page owner table.

## [02]-[MATERIAL_BRIDGE]

- Owner: `MaterialMint` `[Union]` — render-material construction from a document `Material` addressed by table index: `Direct` through `FromMaterial`, `Basic` through `CreateBasicMaterial`, `Imported` through `CreateImportedMaterial` with its reference grant; each resolves the `Material` inside the window and yields an owned `Lease<RenderContent>` for the operation rail to attach. `MaterialBridge` — the two bake directions as symmetric borrow windows: `Bake<TOut>` projects `ToMaterial(TextureGeneration)` for exactly one callback window and disposes the baked `Material` on exit, and `Pbr<TOut>` borrows the `ConvertToPhysicallyBased` projection likewise, disposing the backing material on exit. `SlotUsage` — one detached standard-slot read: occupying texture id, enable, amount, and the resolved child-slot name for a native `StandardChildSlots` value. `MaterialScent` keyless `[SmartEnum]` — the host material-class heuristics as rows with `Plain`/`Textured` predicate columns, folded into the detached `ScentCensus`.
- Law: the PBR route is `ToMaterial`/`ConvertToPhysicallyBased` onto `Rhino.DocObjects.PhysicallyBasedMaterial` — every baked or projected material is borrowed for one window, never stored, and no result shape carries a live material.
- Law: `Rhino.Render.PhysicallyBasedMaterial` is whole-class obsolete and never enters the design.
- Law: slot vocabulary is the native `StandardChildSlots` — the PBR slot roster including its aliasing rows is host truth the seam consumes; a wrapper row per slot is the deleted form, and `SlotFromTextureType`/`TextureTypeFromSlot` answer the type-to-slot correspondence where a consumer needs it.
- Law: assignment is operation-rail work — `AssignTo` over resolved object references with its sub-face and block choices rides the registry page's `ContentOp.Assign` case, so this page carries no table mutation.
- Growth: a new scent is one `MaterialScent` row with its two predicate columns; a new mint form is one `MaterialMint` case.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialMint {
    private MaterialMint() { }
    public sealed record Direct(int MaterialIndex) : MaterialMint;
    public sealed record Basic(int MaterialIndex) : MaterialMint;
    public sealed record Imported(int MaterialIndex, bool Reference) : MaterialMint;

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            direct: static (ctx, mint) => Resolved(ctx.Document, mint.MaterialIndex, ctx.Op).Bind(source => ctx.Op.Catch(() =>
                Optional(RenderMaterial.FromMaterial(material: source, doc: ctx.Document))
                    .ToFin(Fail: ctx.Op.InvalidResult())
                    .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted)))),
            basic: static (ctx, mint) => Resolved(ctx.Document, mint.MaterialIndex, ctx.Op).Bind(source => ctx.Op.Catch(() =>
                Optional(RenderMaterial.CreateBasicMaterial(material: source, doc: ctx.Document))
                    .ToFin(Fail: ctx.Op.InvalidResult())
                    .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted)))),
            imported: static (ctx, mint) => Resolved(ctx.Document, mint.MaterialIndex, ctx.Op).Bind(source => ctx.Op.Catch(() =>
                Optional(RenderMaterial.CreateImportedMaterial(material: source, doc: ctx.Document, reference: mint.Reference))
                    .ToFin(Fail: ctx.Op.InvalidResult())
                    .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted)))));

    private static Fin<Material> Resolved(RhinoDoc document, int index, Op key) =>
        key.Catch(() => Optional(document.Materials[index]).ToFin(Fail: key.MissingContext()));
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
    internal partial bool Plain(RenderMaterial material);

    [UseDelegateFromConstructor]
    internal partial bool Textured(RenderMaterial material);

    internal static ScentCensus CensusOf(RenderMaterial material) =>
        new(Rows: toSeq(Items)
            .Map(row => new ScentMark(Scent: row, Plain: row.Plain(material: material), Textured: row.Textured(material: material)))
            .Filter(static row => row.Plain || row.Textured));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ScentMark(MaterialScent Scent, bool Plain, bool Textured);

public sealed record ScentCensus(Seq<ScentMark> Rows) : IDetachedDocumentResult;

public readonly record struct SlotUsage(
    RenderMaterial.StandardChildSlots Slot, Option<Guid> Texture, bool On, double Amount, string SlotName) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterialBridge {
    internal static Fin<TOut> Bake<TOut>(
        RenderMaterial material, RenderTexture.TextureGeneration generation, Func<Material, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            using Material baked = material.ToMaterial(tg: generation);
            return Optional(baked).ToFin(Fail: key.InvalidResult()).Bind(active => key.Catch(() => borrow(active)));
        });

    internal static Fin<TOut> Pbr<TOut>(
        RenderMaterial material, RenderTexture.TextureGeneration generation, Func<PhysicallyBasedMaterial, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            PhysicallyBasedMaterial projected = material.ConvertToPhysicallyBased(tg: generation);
            return Optional(projected).ToFin(Fail: key.InvalidResult()).Bind(active => {
                using Material backing = active.Material;
                return key.Catch(() => borrow(active));
            });
        });

    internal static Fin<SlotUsage> Usage(RenderMaterial material, RenderMaterial.StandardChildSlots slot, Op key) =>
        key.Catch(() => Fin.Succ(value: new SlotUsage(
            Slot: slot,
            Texture: Optional(material.GetTextureFromUsage(slot: slot)).Map(static texture => texture.Id),
            On: material.GetTextureOnFromUsage(slot: slot),
            Amount: material.GetTextureAmountFromUsage(slot: slot),
            SlotName: material.TextureChildSlotName(slot: slot))));
}
```

## [03]-[TEXTURE]

- Owner: `TextureConfig` — every public settable texture property as one total-state record: projection, wrap, the UVW repeat/offset/rotation triple with the repeat and offset locks, mapping channel, environment mapping mode, and preview/display flags; `Of` reads it in one pass, and `Apply` writes every field inside one change bracket. `TextureTraits` — the derived classification read: local mapping transform, the internal environment-mapping mode beside the effective one `TextureConfig` writes, texel extent, and the capability predicates. `TextureFacsimile` — the reconstructible `SimulatedTexture` payload: filename, UVW values, channel, projection, filtering, and exact `Color4f` transparency. `TextureMint` `[Union]` — the two current bitmap-content admission routes, direct `Bitmap` and detached `SimulatedTexture` state, collapsed behind one leased `Mint` entry. `TextureExport` owns confirmed `SaveAsImage` output.
- Law: configuration writes are total state, never a patch — `Apply` re-asserts every field under one `ChangeReason`, so an absent field cannot silently clear and the write is replayable from the record alone.
- Law: read-only `LocalMappingTransform` and `OriginalFilename` never enter writable state; local mapping reconstructs from the admitted UVW fields, while original filename remains observation-only host provenance.
- Law: the facsimile is the baked carrier's only crossing — a `SimulatedTexture` lives inside a `using` window, `TextureFacsimile.Of` detaches it, and `Apply` reconstructs every carried field through a fresh doc-aware carrier.
- Boundary: live evaluation (`CreateEvaluator`) and the bake gate (`SimulateTexture`) are the Display render page's `TextureBake` owner; this page configures the content, that one evaluates it, and the two never merge.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record TextureConfig(
    TextureProjectionMode Projection,
    TextureWrapType Wrap,
    Vector3d Repeat,
    bool RepeatLocked,
    Vector3d Offset,
    bool OffsetLocked,
    Vector3d Rotation,
    int MappingChannel,
    TextureEnvironmentMappingMode EnvironmentMode,
    bool PreviewIn3D,
    bool PreviewLocalMapping,
    bool DisplayInViewport) : IDetachedDocumentResult {
    public static Fin<TextureConfig> Of(RenderTexture texture, Op key) =>
        key.Catch(() => Fin.Succ(value: new TextureConfig(
            Projection: texture.GetProjectionMode(),
            Wrap: texture.GetWrapType(),
            Repeat: texture.GetRepeat(),
            RepeatLocked: texture.GetRepeatLocked(),
            Offset: texture.GetOffset(),
            OffsetLocked: texture.GetOffsetLocked(),
            Rotation: texture.GetRotation(),
            MappingChannel: texture.GetMappingChannel(),
            EnvironmentMode: texture.GetEnvironmentMappingMode(),
            PreviewIn3D: texture.GetPreviewIn3D(),
            PreviewLocalMapping: texture.GetPreviewLocalMapping(),
            DisplayInViewport: texture.GetDisplayInViewport())));

    internal Fin<Unit> Apply(RenderTexture texture, ChangeReason reason, Op key) {
        TextureConfig self = this;
        return ChangeScope.Write(content: texture, reason: reason, key: key, body: _ => key.Catch(() => {
            texture.SetProjectionMode(self.Projection, reason.Native);
            texture.SetWrapType(self.Wrap, reason.Native);
            texture.SetRepeat(self.Repeat, reason.Native);
            texture.SetRepeatLocked(self.RepeatLocked, reason.Native);
            texture.SetOffset(self.Offset, reason.Native);
            texture.SetOffsetLocked(self.OffsetLocked, reason.Native);
            texture.SetRotation(self.Rotation, reason.Native);
            texture.SetMappingChannel(self.MappingChannel, reason.Native);
            texture.SetEnvironmentMappingMode(self.EnvironmentMode, reason.Native);
            texture.SetPreviewIn3D(self.PreviewIn3D, reason.Native);
            texture.SetPreviewLocalMapping(self.PreviewLocalMapping, reason.Native);
            texture.SetDisplayInViewport(self.DisplayInViewport, reason.Native);
            return Fin.Succ(value: unit);
        }));
    }
}

public readonly record struct TextureTraits(
    Option<(int Width, int Height, int Depth)> Texels,
    Transform LocalTransform,
    TextureEnvironmentMappingMode InternalEnvironmentMode,
    bool HdrCapable,
    bool Linear,
    bool NormalMap,
    bool ImageBased) : IDetachedDocumentResult {
    public static Fin<TextureTraits> Of(RenderTexture texture, Op key) =>
        key.Catch(() => Fin.Succ(value: new TextureTraits(
            Texels: Optional(texture.PixelSize2),
            LocalTransform: texture.LocalMappingTransform,
            InternalEnvironmentMode: texture.GetInternalEnvironmentMappingMode(),
            HdrCapable: texture.IsHdrCapable(),
            Linear: texture.IsLinear(),
            NormalMap: texture.IsNormalMap(),
            ImageBased: texture.IsImageBased())));
}

public sealed record TextureFacsimile(
    Option<string> Filename,
    Vector2d Repeat,
    Vector2d Offset,
    double Rotation,
    bool Repeating,
    int MappingChannel,
    SimulatedTexture.ProjectionModes Projection,
    Option<(Rhino.Display.Color4f Color, double Sensitivity)> Transparency,
    bool Filtered) : IDetachedDocumentResult {
    internal static TextureFacsimile Of(SimulatedTexture simulated) =>
        new(
            Filename: Optional(simulated.Filename).Filter(static path => path.Length > 0),
            Repeat: simulated.Repeat,
            Offset: simulated.Offset,
            Rotation: simulated.Rotation,
            Repeating: simulated.Repeating,
            MappingChannel: simulated.MappingChannel,
            Projection: simulated.ProjectionMode,
            Transparency: simulated.HasTransparentColor
                ? Some((simulated.TransparentColor, simulated.TransparentColorSensitivity))
                : Option<(Rhino.Display.Color4f, double)>.None,
            Filtered: simulated.Filtered);

    internal Fin<Unit> Apply(SimulatedTexture simulated, Op key) {
        TextureFacsimile self = this;
        return key.Catch(() => {
            simulated.Filename = self.Filename.IfNone(string.Empty);
            simulated.Repeat = self.Repeat;
            simulated.Offset = self.Offset;
            simulated.Rotation = self.Rotation;
            simulated.Repeating = self.Repeating;
            simulated.MappingChannel = self.MappingChannel;
            simulated.ProjectionMode = self.Projection;
            simulated.Filtered = self.Filtered;
            self.Transparency.Match(
                Some: row => {
                    simulated.HasTransparentColor = true;
                    simulated.TransparentColor = row.Color;
                    simulated.TransparentColorSensitivity = row.Sensitivity;
                },
                None: () => simulated.HasTransparentColor = false);
            return Fin.Succ(value: unit);
        });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureMint {
    private TextureMint() { }
    public sealed record Bitmap(System.Drawing.Bitmap Value) : TextureMint;
    public sealed record Simulated(TextureFacsimile Value) : TextureMint;

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            bitmap: static (ctx, mint) => ctx.Op.Catch(() => Optional(RenderTexture.NewBitmapTexture(bitmap: mint.Value, doc: ctx.Document))
                .ToFin(Fail: ctx.Op.InvalidResult())
                .Map(static content => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: content))),
            simulated: static (ctx, mint) => ctx.Op.Catch(() => {
                using SimulatedTexture carrier = new(ctx.Document);
                return mint.Value.Apply(simulated: carrier, key: ctx.Op)
                    .Bind(_ => Optional(RenderTexture.NewBitmapTexture(texture: carrier, doc: ctx.Document))
                        .ToFin(Fail: ctx.Op.InvalidResult())
                        .Map(static content => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: content)));
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

- Owner: `EnvironmentState` — the detached environment simulation: background color across the kernel `PerceptualColor` seam, native background projection, and optional reconstructible image. `Bake` runs `SimulateEnvironment(bool)` inside a `using` window and detaches; `Mint` reconstructs a `SimulatedEnvironment`, including its doc-aware image carrier, and constructs a basic environment through `NewBasicEnvironment` onto an owned lease.
- Law: the bake window is the only site holding a `SimulatedEnvironment` — the disposable carrier never crosses, the state record does, and a re-derived background blend beside the kernel color owner is the deleted form.
- Law: mint round-trips the bake — `Mint(Bake(x))` reconstructs the same background payload, so environment duplication across documents travels as one value, never a handle.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record EnvironmentState(
    PerceptualColor Background,
    SimulatedEnvironment.BackgroundProjections Projection,
    Option<TextureFacsimile> Image) : IDetachedDocumentResult {
    internal static Fin<EnvironmentState> Bake(RenderEnvironment environment, bool isForDataOnly, Op key) =>
        key.Catch(() => {
            using SimulatedEnvironment simulated = environment.SimulateEnvironment(isForDataOnly: isForDataOnly);
            return Optional(simulated).ToFin(Fail: key.InvalidResult()).Bind(active => {
                using SimulatedTexture image = active.BackgroundImage;
                Option<TextureFacsimile> detachedImage = image.ConstPointer() == IntPtr.Zero
                    ? Option<TextureFacsimile>.None
                    : Some(TextureFacsimile.Of(simulated: image));
                return PerceptualColor.OfRgb(active.BackgroundColor.R, active.BackgroundColor.G, active.BackgroundColor.B, active.BackgroundColor.A / 255.0)
                    .Map(background => new EnvironmentState(Background: background, Projection: active.BackgroundProjection, Image: detachedImage));
            });
        });

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) {
        EnvironmentState self = this;
        return key.Catch(() => {
            using SimulatedEnvironment simulated = new();
            (byte r, byte g, byte b, double alpha) = self.Background.ToRgb();
            simulated.BackgroundColor = System.Drawing.Color.FromArgb(
                byte.CreateSaturating(Math.Round(alpha * byte.MaxValue)), r, g, b);
            simulated.BackgroundProjection = self.Projection;
            return self.Image.Match(
                Some: facsimile => {
                    using SimulatedTexture reconstructed = new(document);
                    return facsimile.Apply(simulated: reconstructed, key: key)
                        .Bind(_ => {
                            simulated.BackgroundImage = reconstructed;
                            return Basic(simulated: simulated, document: document, key: key);
                        });
                },
                None: () => Basic(simulated: simulated, document: document, key: key));
        });
    }

    private static Fin<Lease<RenderContent>> Basic(SimulatedEnvironment simulated, RhinoDoc document, Op key) =>
        Optional(RenderEnvironment.NewBasicEnvironment(environment: simulated, doc: document))
            .ToFin(Fail: key.InvalidResult())
            .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]            | [FORM]                                                 | [ENTRY]                         |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------- | :------------------------------ |
|  [01]   | material minting       | `MaterialMint`     | one union over the three `From*` routes, leased result | `Mint(document, key)`           |
|  [02]   | material bake and PBR  | `MaterialBridge`   | one borrow window per bake direction                   | `Bake` / `Pbr` / `Usage`        |
|  [03]   | material class         | `MaterialScent`    | predicate-column rows folded into `ScentCensus`        | `CensusOf(material)`            |
|  [04]   | texture configuration  | `TextureConfig`    | total public writable state, bracketed write           | `Of` / `Apply(texture, reason)` |
|  [05]   | texture classification | `TextureTraits`    | derived local transform and capability read            | `Of(texture, key)`              |
|  [06]   | baked-texture crossing | `TextureFacsimile` | detached, reconstructible simulation payload           | `Of` / `Apply`                  |
|  [07]   | texture mint/export    | `TextureMint`      | direct/simulated leased mint plus confirmed export     | `Mint` / `TextureExport.Export` |
|  [08]   | environment bake/mint  | `EnvironmentState` | detached bake, round-tripping leased mint              | `Bake` / `Mint(document, key)`  |
