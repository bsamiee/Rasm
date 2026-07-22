# [RASM_RHINO_RENDER_KINDS]

`RenderKind` owns material, texture, and environment specialization over one `RenderContent` lifecycle, and the photometric-web payload the light rail defers here. Material projections remain callback-bounded borrows, texture state replays every writable axis while retaining read-only simulation evidence, environment state detaches every native carrier, and each mint exits as an owned `Lease<RenderContent>`.

## [01]-[INDEX]

- [02]-[MATERIAL_BRIDGE]: `MaterialMint`, `MaterialBridge`, `SlotUsage`, and the `MaterialScent` classification fold.
- [03]-[TEXTURE]: `TextureConfig` total-state configuration, `TextureTraits`, `TextureFacsimile`, and the bitmap mint/export pair.
- [04]-[ENVIRONMENT]: `EnvironmentState` and the bake/mint pair over `SimulatedEnvironment`.
- [05]-[PHOTOMETRIC]: `PhotometricDialect`, the `PhotometricWeb` payload, serializer discovery, and the material-graph attach.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[MATERIAL_BRIDGE]

- Owner: `MaterialMint` carries each table address and admits it against the live material roster inside the document-aware mint seam. `MaterialBridge` bounds baked and physically based projections to one callback. `SlotUsage` detaches standard-slot state and its native texture-type correspondence, and `MaterialScent` derives classification from predicate rows.
- Law: `MaterialBridge.Pbr` routes `ToMaterial`/`ConvertToPhysicallyBased` onto `Rhino.DocObjects.PhysicallyBasedMaterial`; each projection remains borrowed for one window.
- Law: `Rhino.Render.PhysicallyBasedMaterial` is whole-class obsolete and never enters the design.
- Law: slot vocabulary is the native `StandardChildSlots` — the PBR slot roster including its aliasing rows is host truth the seam consumes; a wrapper row per slot is the deleted form, and `SlotFromTextureType`/`TextureTypeFromSlot` answer the type-to-slot correspondence where a consumer needs it.
- Law: assignment is operation-rail work — `AssignTo` over resolved object references with its sub-face and block choices rides the registry page's `ContentOp.Assign` case, so this page carries no table mutation.
- Growth: a new scent is one `MaterialScent` row with its two predicate columns; a new mint form is one `MaterialMint` case.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MaterialMint {
    private MaterialMint() { }
    private sealed record DirectCase(int MaterialIndex) : MaterialMint;
    private sealed record BasicCase(int MaterialIndex) : MaterialMint;
    private sealed record ImportedCase(int MaterialIndex, bool Reference) : MaterialMint;

    public static MaterialMint Direct(int materialIndex) =>
        new DirectCase(MaterialIndex: materialIndex);

    public static MaterialMint Basic(int materialIndex) =>
        new BasicCase(MaterialIndex: materialIndex);

    public static MaterialMint Imported(int materialIndex, bool reference) =>
        new ImportedCase(MaterialIndex: materialIndex, Reference: reference);

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            directCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                static (source, document) => RenderMaterial.FromMaterial(material: source, doc: document)),
            basicCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                static (source, document) => RenderMaterial.CreateBasicMaterial(material: source, doc: document)),
            importedCase: static (ctx, mint) => Minted(ctx, mint.MaterialIndex,
                (source, document) => RenderMaterial.CreateImportedMaterial(material: source, doc: document, reference: mint.Reference)));

    private static Fin<Lease<RenderContent>> Minted(
        (RhinoDoc Document, Op Op) ctx, int index, Func<Material, RhinoDoc, RenderContent?> route) =>
        from _index in guard(index >= 0 && index < ctx.Document.Materials.Count, ctx.Op.InvalidInput()).ToFin()
        from minted in ctx.Op.Catch(() => Optional(ctx.Document.Materials[index]).ToFin(Fail: ctx.Op.MissingContext())
            .Bind(source => Optional(route(source, ctx.Document)).ToFin(Fail: ctx.Op.InvalidResult()).Leased()))
        select minted;
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
    RenderMaterial.StandardChildSlots Slot,
    RenderMaterial.TextureType TextureType,
    Option<Guid> Texture,
    bool On,
    double Amount,
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
        RenderMaterial material, RenderTexture.TextureGeneration generation, Func<PhysicallyBasedMaterial, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            PhysicallyBasedMaterial projected = material.ConvertToPhysicallyBased(tg: generation);
            return Optional(projected).ToFin(Fail: key.InvalidResult()).Bind(active => {
                using Material backing = active.Material;
                return borrow(active);
            });
        });

    internal static Fin<SlotUsage> Usage(RenderMaterial material, RenderMaterial.StandardChildSlots slot, Op key) =>
        key.Catch(() => Fin.Succ(value: new SlotUsage(
            Slot: slot,
            TextureType: RenderMaterial.TextureTypeFromSlot(slot: slot),
            Texture: Optional(material.GetTextureFromUsage(slot: slot)).Map(static texture => texture.Id),
            On: material.GetTextureOnFromUsage(slot: slot),
            Amount: material.GetTextureAmountFromUsage(slot: slot),
            SlotName: material.TextureChildSlotName(slot: slot))));
}
```

## [03]-[TEXTURE]

- Owner: `TextureConfig` is the replayable live-content state, including `TextureGraphInfo`. `TextureTraits` detaches classification. `SimulatedMapping` owns direct and environment-aware mapping writes, `TextureFacsimile` carries reconstructible simulation state plus read-only transform provenance, `TextureMint` admits each native source, and `TextureExport` confirms image egress.
- Law: configuration writes are total state, never a patch — `Apply` re-asserts every field under one `ChangeReason`, so an absent field cannot silently clear and the write is replayable from the record alone.
- Law: read-only `LocalMappingTransform` and `OriginalFilename` never enter writable state; local mapping reconstructs from the admitted UVW fields, while original filename remains observation-only host provenance.
- Law: `TextureFacsimile` records `OriginalFilename` and `LocalMappingTransform` as evidence while replaying the host-writable axes through a fresh document-aware carrier.
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
    TextureGraphInfo Graph,
    bool PreviewIn3D,
    bool PreviewLocalMapping,
    bool DisplayInViewport) : IDetachedDocumentResult {
    public static Fin<TextureConfig> Of(RenderTexture texture, Op key) => key.Catch(() => {
        TextureGraphInfo graph = new();
        texture.GraphInfo(ref graph);
        return Fin.Succ(value: new TextureConfig(
            Projection: texture.GetProjectionMode(),
            Wrap: texture.GetWrapType(),
            Repeat: texture.GetRepeat(),
            RepeatLocked: texture.GetRepeatLocked(),
            Offset: texture.GetOffset(),
            OffsetLocked: texture.GetOffsetLocked(),
            Rotation: texture.GetRotation(),
            MappingChannel: texture.GetMappingChannel(),
            EnvironmentMode: texture.GetEnvironmentMappingMode(),
            Graph: graph,
            PreviewIn3D: texture.GetPreviewIn3D(),
            PreviewLocalMapping: texture.GetPreviewLocalMapping(),
            DisplayInViewport: texture.GetDisplayInViewport()));
    });

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
            texture.SetGraphInfo(self.Graph);
            texture.SetPreviewIn3D(self.PreviewIn3D, reason.Native);
            texture.SetPreviewLocalMapping(self.PreviewLocalMapping, reason.Native);
            texture.SetDisplayInViewport(self.DisplayInViewport, reason.Native);
            return Fin.Succ(value: unit);
        }));
    }
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimulatedMapping {
    private SimulatedMapping() { }
    private sealed record DirectCase(SimulatedTexture.ProjectionModes Projection, int Channel) : SimulatedMapping;
    private sealed record EnvironmentCase(
        SimulatedTexture.ProjectionModes Projection,
        int Channel,
        SimulatedTexture.EnvironmentMappingModes Environment) : SimulatedMapping;

    public static Fin<SimulatedMapping> Of(
        SimulatedTexture.ProjectionModes projection,
        int channel,
        Option<SimulatedTexture.EnvironmentMappingModes> environment = default) =>
        channel >= 0
            ? Fin.Succ(value: environment.Match(
                Some: mode => (SimulatedMapping)new EnvironmentCase(
                    Projection: projection,
                    Channel: channel,
                    Environment: mode),
                None: () => new DirectCase(Projection: projection, Channel: channel)))
            : Fin.Fail<SimulatedMapping>(error: Op.Of(name: nameof(SimulatedMapping)).InvalidInput());

    internal static Fin<SimulatedMapping> Capture(SimulatedTexture texture) {
        using Texture projected = texture.Texture();
        Option<SimulatedTexture.EnvironmentMappingModes> environment = projected.ProjectionMode switch {
            TextureProjectionModes.EnvironmentMapBox => Some(SimulatedTexture.EnvironmentMappingModes.Box),
            TextureProjectionModes.EnvironmentMapLightProbe => Some(SimulatedTexture.EnvironmentMappingModes.Lightprobe),
            TextureProjectionModes.EnvironmentMapSpherical => Some(SimulatedTexture.EnvironmentMappingModes.Spherical),
            TextureProjectionModes.EnvironmentMapCube => Some(SimulatedTexture.EnvironmentMappingModes.Cubemap),
            TextureProjectionModes.EnvironmentMapVCrossCube => Some(SimulatedTexture.EnvironmentMappingModes.VerticalCrossCubemap),
            TextureProjectionModes.EnvironmentMapHCrossCube => Some(SimulatedTexture.EnvironmentMappingModes.HorizontalCrossCubemap),
            TextureProjectionModes.EnvironmentMapHemispherical => Some(SimulatedTexture.EnvironmentMappingModes.Hemispherical),
            TextureProjectionModes.EnvironmentMapEmap => Some(SimulatedTexture.EnvironmentMappingModes.Emap),
            _ when texture.ProjectionMode == SimulatedTexture.ProjectionModes.Emap =>
                Some(SimulatedTexture.EnvironmentMappingModes.Automatic),
            _ => None,
        };
        return Of(
            projection: texture.ProjectionMode,
            channel: texture.MappingChannel,
            environment: environment);
    }

    internal Unit Apply(SimulatedTexture texture) =>
        Switch(
            state: texture,
            directCase: static (target, mapping) => {
                target.ProjectionMode = mapping.Projection;
                target.MappingChannel = mapping.Channel;
                return unit;
            },
            environmentCase: static (target, mapping) => {
                target.SetMappingChannelAndProjectionMode(mapping.Projection, mapping.Channel, mapping.Environment);
                return unit;
            });
}

public readonly record struct TextureTraits(
    Option<(int Width, int Height, int Depth)> Texels,
    Transform LocalTransform,
    RenderTexture.eLocalMappingType LocalMappingType,
    TextureEnvironmentMappingMode InternalEnvironmentMode,
    bool HdrCapable,
    bool Linear,
    bool NormalMap,
    bool ImageBased) : IDetachedDocumentResult {
    public static Fin<TextureTraits> Of(RenderTexture texture, Op key) =>
        key.Catch(() => Fin.Succ(value: new TextureTraits(
            Texels: Optional(texture.PixelSize2),
            LocalTransform: texture.LocalMappingTransform,
            LocalMappingType: texture.GetLocalMappingType(),
            InternalEnvironmentMode: texture.GetInternalEnvironmentMappingMode(),
            HdrCapable: texture.IsHdrCapable(),
            Linear: texture.IsLinear(),
            NormalMap: texture.IsNormalMap(),
            ImageBased: texture.IsImageBased())));
}

public sealed record TextureFacsimile(
    Option<string> Filename,
    Option<string> OriginalFilename,
    Transform LocalTransform,
    Vector2d Repeat,
    Vector2d Offset,
    double Rotation,
    bool Repeating,
    SimulatedMapping Mapping,
    Option<(Color4f Color, double Sensitivity)> Transparency,
    bool Filtered) : IDetachedDocumentResult {
    internal static Fin<TextureFacsimile> Of(SimulatedTexture simulated) =>
        SimulatedMapping.Capture(texture: simulated).Map(mapping => new TextureFacsimile(
            Filename: Optional(simulated.Filename).Filter(static path => path.Length > 0),
            OriginalFilename: Optional(simulated.OriginalFilename).Filter(static path => path.Length > 0),
            LocalTransform: simulated.LocalMappingTransform,
            Repeat: simulated.Repeat,
            Offset: simulated.Offset,
            Rotation: simulated.Rotation,
            Repeating: simulated.Repeating,
            Mapping: mapping,
            Transparency: simulated.HasTransparentColor
                ? Some((simulated.TransparentColor, simulated.TransparentColorSensitivity))
                : Option<(Color4f, double)>.None,
            Filtered: simulated.Filtered));

    internal Fin<Unit> Apply(SimulatedTexture simulated, Op key) {
        TextureFacsimile self = this;
        return key.Catch(() => {
            simulated.Filename = self.Filename.IfNone(string.Empty);
            simulated.Repeat = self.Repeat;
            simulated.Offset = self.Offset;
            simulated.Rotation = self.Rotation;
            simulated.Repeating = self.Repeating;
            _ = self.Mapping.Apply(texture: simulated);
            simulated.Filtered = self.Filtered;
            if (self.Transparency.Case is System.ValueTuple<Color4f, double> row) {
                simulated.HasTransparentColor = true;
                simulated.TransparentColor = row.Item1;
                simulated.TransparentColorSensitivity = row.Item2;
            } else {
                simulated.HasTransparentColor = false;
            }
            return Fin.Succ(value: unit);
        });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureMint {
    private TextureMint() { }
    private sealed record BitmapCase(System.Drawing.Bitmap Value) : TextureMint;
    private sealed record SimulatedCase(TextureFacsimile Value) : TextureMint;

    public static Fin<TextureMint> From(System.Drawing.Bitmap value) =>
        Optional(value).ToFin(Fail: Op.Of(name: nameof(TextureMint)).InvalidInput())
            .Map(static admitted => (TextureMint)new BitmapCase(Value: admitted));

    public static Fin<TextureMint> From(TextureFacsimile value) =>
        Optional(value).ToFin(Fail: Op.Of(name: nameof(TextureMint)).InvalidInput())
            .Map(static admitted => (TextureMint)new SimulatedCase(Value: admitted));

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            bitmapCase: static (ctx, mint) => ctx.Op.Catch(() =>
                Optional(RenderTexture.NewBitmapTexture(bitmap: mint.Value, doc: ctx.Document)).ToFin(Fail: ctx.Op.InvalidResult()).Leased()),
            simulatedCase: static (ctx, mint) => ctx.Op.Catch(() => {
                using SimulatedTexture carrier = new(ctx.Document);
                return mint.Value.Apply(simulated: carrier, key: ctx.Op)
                    .Bind(_ => Optional(RenderTexture.NewBitmapTexture(texture: carrier, doc: ctx.Document))
                        .ToFin(Fail: ctx.Op.InvalidResult())
                        .Leased());
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

- Owner: `EnvironmentState` detaches background color, projection, and image state. `Bake` contains the simulation lease, and `Mint` reconstructs the document-aware carriers before yielding an owned content lease.
- Law: `EnvironmentState.Bake` is the only site holding a `SimulatedEnvironment`; the disposable carrier never crosses its window.
- Law: environment duplication travels as one detached value; simulation-only transform provenance remains evidence while every host-writable image axis replays.

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
                Fin<Option<TextureFacsimile>> detached = image.ConstPointer() == IntPtr.Zero
                    ? Fin.Succ(Option<TextureFacsimile>.None)
                    : TextureFacsimile.Of(simulated: image).Map(static value => Some(value));
                return from detachedImage in detached
                       from background in PerceptualColor.OfRgb(
                           active.BackgroundColor.R,
                           active.BackgroundColor.G,
                           active.BackgroundColor.B,
                           active.BackgroundColor.A)
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
            simulated.BackgroundColor = self.Background.Quantized();
            simulated.BackgroundProjection = self.Projection;
            if (self.Image.Case is TextureFacsimile facsimile) {
                using SimulatedTexture reconstructed = new(document);
                return from _ in facsimile.Apply(simulated: reconstructed, key: key)
                       from __ in key.Catch(() => { simulated.BackgroundImage = reconstructed; return Fin.Succ(value: unit); })
                       from minted in Basic(simulated: simulated, document: document, key: key)
                       select minted;
            }
            return Basic(simulated: simulated, document: document, key: key);
        });
    }

    private static Fin<Lease<RenderContent>> Basic(SimulatedEnvironment simulated, RhinoDoc document, Op key) =>
        Optional(RenderEnvironment.NewBasicEnvironment(environment: simulated, doc: document))
            .ToFin(Fail: key.InvalidResult())
            .Leased();
}
```

## [05]-[PHOTOMETRIC]

- Owner: `PhotometricDialect` closes the light-distribution file vocabulary by extension and description; `PhotometricWeb` is the admitted payload the Objects lights rail defers here; `PhotometricPress` derives one registry `ContentSerializer` program per dialect row for host discovery, and its single transfer admission converts owned leases into `ContentTransfer` custody while disposing and refusing every other lease case.
- Law: the host carries no first-class photometric type — `Rhino.Geometry.Light` ends at intensity and power, so the web travels as texture-kind render content on the light's attached render material, addressed as a `ContentRef` child slot and embedded through the content's own `FilesToEmbed` roster.
- Law: attach is one fold — `PhotometricWeb.AttachTo` accepts owned custody only, arms the named child slot before `SetChild`, restores prior slot state on refusal, and surrenders custody only after host acceptance inside one `ChangeScope.Write` bracket; a second attach spelling beside it is the deleted form.
- Law: a new distribution dialect is one `PhotometricDialect` row; the serializer-program fold covers every row, so discovery, description, and admission cannot drift.
- Boundary: the content class the serializer materializes is the discovering plugin's `CustomRenderContentAttribute` type; this page owns admission, discovery, attach, and the embed census, never the plugin's field layout — field declaration rides the fields page.
- Boundary: the lights rail's photometric reach ends at `Radiance`; `LightEdit` never grows an IES case, and the seam crossing is this page's `PhotometricWeb` alone.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PhotometricDialect {
    public static readonly PhotometricDialect Ies = new(".ies", "IES photometric distribution");
    public static readonly PhotometricDialect Eulumdat = new(".ldt", "EULUMDAT photometric distribution");
    public static readonly PhotometricDialect CieRecord = new(".cie", "CIE photometric distribution");

    internal string Description { get; }

    internal static Fin<PhotometricDialect> OfPath(string path, Op key) =>
        key.Catch(() =>
            Validate(System.IO.Path.GetExtension(path).ToLowerInvariant(), null, out PhotometricDialect? dialect) is null
                ? Fin.Succ(value: dialect!)
                : Fin.Fail<PhotometricDialect>(error: key.InvalidInput()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PhotometricWeb : IDetachedDocumentResult {
    private PhotometricWeb(string path, PhotometricDialect dialect) => (Path, Dialect) = (path, dialect);
    public string Path { get; }
    public PhotometricDialect Dialect { get; }

    public static Fin<PhotometricWeb> Of(string path, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in op.AcceptText(value: path)
               from dialect in PhotometricDialect.OfPath(path: admitted, key: op)
               from _ in guard(System.IO.File.Exists(admitted), op.MissingContext()).ToFin()
               select new PhotometricWeb(path: admitted, dialect: dialect);
    }

    internal Fin<Unit> AttachTo(RenderContent parent, string childSlot, PhotometricPress press, ChangeReason reason, Op key) {
        PhotometricWeb self = this;
        return from slot in key.AcceptText(value: childSlot)
               from lease in press.Materialize(web: self, key: key)
               from custody in PhotometricPress.Transfer(lease: lease, key: key)
               from _ in AttachOwned(
                   parent: parent,
                   slot: slot,
                   owned: custody.Owned,
                   transfer: custody.Transfer,
                   reason: reason,
                   key: key)
               select unit;
    }

    private static Fin<Unit> AttachOwned(
        RenderContent parent,
        string slot,
        Lease<RenderContent>.Owned owned,
        ContentTransfer transfer,
        ChangeReason reason,
        Op key) {
        Fin<Unit> attached = ChangeScope.Write(content: parent, reason: reason, key: key, body: live =>
            from prior in key.Catch(() => Fin.Succ(value: live.ChildSlotOn(childSlotName: slot)))
            from _ in key.Catch(() => {
                live.SetChildSlotOn(childSlotName: slot, bOn: true, cc: reason.Native);
                return key.Confirm(success: live.SetChild(child: owned.Value, childSlotName: slot, cc: reason.Native));
            }).Match(
                Succ: _ => transfer.Take(key).Map(static _ => unit),
                Fail: fault => RestoreSlot(
                    parent: live, slot: slot, prior: prior, reason: reason, primary: fault, key: key))
            select unit);
        return attached.Match(
            Succ: value => Release(transfer: transfer, key: key).Map(_ => value),
            Fail: primary => Release(transfer: transfer, key: key).Match(
                Succ: _ => Fin.Fail<Unit>(error: primary),
                Fail: release => Fin.Fail<Unit>(error: primary + release)));
    }

    private static Fin<Unit> Release(ContentTransfer transfer, Op key) => key.Catch(() => {
        transfer.Dispose();
        return Fin.Succ(value: unit);
    });

    private static Fin<Unit> RestoreSlot(
        RenderContent parent,
        string slot,
        bool prior,
        ChangeReason reason,
        Error primary,
        Op key) =>
        key.Catch(() => {
            parent.SetChildSlotOn(childSlotName: slot, bOn: prior, cc: reason.Native);
            return Fin.Succ(value: unit);
        }).Match(
            Succ: _ => Fin.Fail<Unit>(error: primary),
            Fail: restore => Fin.Fail<Unit>(error: primary + restore));

    internal Seq<string> Embedded(RenderContent content) =>
        toSeq(content.GetEmbeddedFilesList());
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record PhotometricPress(Func<PhotometricWeb, RhinoDoc?, Fin<Lease<RenderContent>>> Reader) {
    internal Fin<Lease<RenderContent>> Materialize(PhotometricWeb web, Op key, RhinoDoc? document = null) =>
        key.Catch(() => Reader(web, document));

    public Fin<Seq<RenderContentSerializer>> Serializers(
        RetentionPolicy retention,
        Action<Error> record,
        Op? key = null) {
        Op op = key.OrDefault();
        PhotometricPress self = this;
        return from activeRetention in Optional(retention).ToFin(Fail: op.InvalidInput())
               from activeRecord in Optional(record).ToFin(Fail: op.InvalidInput())
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
        (from web in PhotometricWeb.Of(path: path, key: key)
         from lease in Materialize(web: web, key: key)
         from custody in Transfer(lease: lease, key: key)
         select custody.Transfer).MapFail(failure => {
             record(failure);
             return failure;
         });

    internal static Fin<(Lease<RenderContent>.Owned Owned, ContentTransfer Transfer)> Transfer(
        Lease<RenderContent> lease,
        Op key) =>
        key.Catch(() => {
            if (lease is Lease<RenderContent>.Owned owned) {
                return Fin.Succ(value: (Owned: owned, Transfer: new ContentTransfer(owned: owned)));
            }
            lease.Dispose();
            return Fin.Fail<(Lease<RenderContent>.Owned, ContentTransfer)>(error: key.InvalidResult());
        });
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]              | [FORM]                                                 | [ENTRY]                         |
| :-----: | :--------------------- | :------------------- | :----------------------------------------------------- | :------------------------------ |
|  [01]   | material minting       | `MaterialMint`       | document-aware leased mint                             | `Direct` / `Basic` / `Imported` |
|  [02]   | material bake and PBR  | `MaterialBridge`     | callback-bounded material projection                   | `Bake` / `Pbr` / `Usage`        |
|  [03]   | material class         | `MaterialScent`      | predicate-column rows folded into `ScentCensus`        | `CensusOf(material)`            |
|  [04]   | texture configuration  | `TextureConfig`      | total replayable state                                 | `Of` / `Apply(texture, reason)` |
|  [05]   | texture classification | `TextureTraits`      | detached local mapping and capability census            | `Of(texture, key)`              |
|  [06]   | baked-texture crossing | `TextureFacsimile`   | replayable facsimile state                             | `Of` / `Apply`                  |
|  [07]   | texture mint/export    | `TextureMint`        | admitted leased texture lifecycle                      | `From` / `Mint`                 |
|  [08]   | environment bake/mint  | `EnvironmentState`   | detached state and document-aware leased mint           | `Bake` / `Mint(document, key)`  |
|  [09]   | photometric payload    | `PhotometricWeb`     | dialect-admitted content attachment                    | `Of` / `AttachTo`               |
|  [10]   | photometric readers    | `PhotometricPress`   | declarative registry serializer roster                  | `Serializers(retention, record)` |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
