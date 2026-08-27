# [APPUI_RENDER_SHADING]

One GPU shader-asset owner with a per-backend, byte-budgeted residency cache feeds the path tracer's Materials closure: `ShaderAssetCache` composes the folder's ONE `Theme/assets` `BudgetedCache` twice — a program lane retaining a compiled shader per `GpuBackend` (`SKRuntimeEffect` for the Skia Ganesh family, `Silk.NET.WebGPU` pipeline state for the Wgpu family) and a plane lane holding every native plane a bound roster row addresses under a VRAM ceiling — and `ShaderShade` is the GPU shading pass consuming the `LayeredBsdf` the path-trace integrator also shades from. `ShaderAssetCache` owns shader compilation, plane residency, retained program and native-texture lifetime, and cache identity, while `ShadeStage` resolves a frame's materials and `ShaderShade` mounts the pass; all three share the viewport's one `Wgpu` device, consume the Materials appearance model, and confine `SKSurface` ownership to `Offscreen`.

`ShaderSource` declares its bindings as DATA — one admitted `ShaderBinding` row per shader slot naming its `ShadeSource`, so a uniform write is a roster fold and a shader-local uniform name never appears as a literal in a bind arm. Every source resolves through ONE `ShadeSupply` answer, so a scalar run, a sampled plane, and a packed lane are three rows of one resolution rather than three bind arms. Texture slots key on the Materials `Rasm.Materials.Raster.TextureChannel` roster, never an AppUi-local channel vocabulary; `TextureSet` supplies the channel and packed pyramids and `EnvironmentLight` supplies the prefiltered dome the raster pass reads instead of marching, so this page uploads and binds and never mints a plane, a transfer, a sampler law, a channel name, or a lighting integral. `SKRuntimeEffect`, `Silk.NET.WebGPU`, Thinktecture, and LanguageExt supply the substrate; the CPU `LayeredBsdf` evaluation is the reference path.

## [01]-[INDEX]

- [02]-[SHADER_ASSET]: Per-`GpuBackend` shader and budgeted plane residency over the folder's one `BudgetedCache`; `SKRuntimeEffect` and wgpu pipeline-state compile; the admitted `ShaderBinding` roster and its one `ShadeSupply` resolution.
- [03]-[SURFACE_SHADE]: `ShadeStage` resolves and mounts the shade pass from the Materials `LayeredBsdf`, channel-value closure, bound `TextureSet`, and resolved `EnvironmentLight`.

## [02]-[SHADER_ASSET]

- Owner: `ShaderAsset` the per-backend compiled-shader cell; `ShaderProgram` the closed Ganesh-or-Wgpu retained native program; `ShadeTexture` the closed Ganesh-or-Wgpu retained native texture; `ResidentPlane` the native plane beside its charged bytes; `SlotWidth` the closed slot-width family a uniform kind carries; `ShaderBinding` the shader-slot roster row; `ShadeSource` the value each row pulls; `EnvironmentRead` the image-based-lighting row family; `ShadeSupply` the ONE answer a source resolves to; `ShaderArm` the per-family compile, upload, and ladder-seat delegate rows; `AdmitOutcome` the residency admission partition; `ShaderSource` the admitted backend-neutral shader source; `PlaneUpload` the backend-neutral upload request; `WgpuShaderCompiler` the composition-bound wgpu compile-and-upload capsule with its owned `WgpuPipelineState`; `ShaderAssetCache` the one residency owner over both retained kinds; `ShaderFault` the direct generated `[Union]` with one `[FaultCase]` leaf per shader failure.
- Cases: `ShaderFault` = CompileFailed | BackendUnsupported | UniformAbsent | PlaneUnbindable | DeviceLost; `ShadeSource` = Channel | Lobes | Ambient; `ShadeSupply` = Run | Sampled | Absent; `SlotWidth` = Exact | Run | Plane.
- Law: the binding roster is the ONLY place a shader slot name exists. Bind arms fold `ShaderSource.Bindings` and ask each row for its `ShadeSupply`, so adding a channel to the shader is one roster row and a hardcoded `"baseColor"` write in a bind arm is the deleted form. `Group`/`Slot` on the row carry the wgpu binding coordinates the compiler capsule reads, while the Ganesh arm addresses by `Name` alone, so one roster serves both layouts without a per-backend roster.
- Law: the roster ADMITS at `ShaderSource.Of` and never at draw time, and it refuses APPLICATIVELY — every roster gate and every row gate resolves against the same candidate, so a roster wrong on three slots names all three instead of the first the ladder met. Duplicate slot names, a colliding `(Group, Slot)` pair, a second `Lobes` row, a fixed-width `SlotWidth` disagreeing with its source's own arity, and a sampled channel row carrying no `Swizzle` companion each refuse at construction; the Ganesh compile gates every row name against `SKRuntimeEffect.Uniforms`/`Children`, the effect's own declared slot lists, so `ShaderFault.UniformAbsent` is a compile verdict rather than a per-frame surprise. Draw-time re-validation of an admitted roster is the deleted form.
- Law: a PACKED channel binds its sheet AND its lane. `ChannelPack.Lane` is the one slot correspondence, so the sheet a roughness rides inside resolves to `(plane, lane)` and the bind fold writes the lane through the row's own `Swizzle` uniform — a WGSL or SkSL swizzle literal chosen by the shader author is the fork this forecloses, because the pack order is set data the shader cannot see.
- Law: the sampler follows the SET, never a literal. Address mode reads the set's own periodicity evidence — only a set whose `Tiled` `Evidence<TileProof>` holds a measured proof the bar ACCEPTED repeats, while an ingested, refused, below-bar, or UDIM set clamps, because repeating a plane the gate never certified shows the join the proof exists to certify (a below-bar proof MEASURED that join) and repeating a UDIM tile bleeds its neighbour — and filter follows the level count. Every environment plane takes its address axes from the FROZEN equirect correspondence on its own `EnvironmentRead` row.
- Law: plane residency keys on the plane CONTENT key — a channel pyramid's own `Key`, an environment product's generated `ContentAddress.ToValue()` projection — never the channel, the set, or the light. One plane shared by many sets or many lights uploads once, an edited plane re-keys and re-uploads, and a rebind at identical bytes reuses every resident handle.
- Law: residency is BUDGETED by the folder's one cache owner. The plane lane charges each plane its extent times its storage width and releases least-touched cells until the total fits, so texture VRAM is governed exactly as the `Render/meshlets` `ResidencyBudget` governs geometry VRAM rather than growing until the device refuses. The pressure sweep respects the live GENERATION floor `RetentionPosture.Bound` carries: a resolution opens the generation, every plane it makes resident is stamped at that value, and a stamped cell is unreleasable while it stands — the one comparison that makes a handle a bound draw holds unreleasable, because a budgeted cache without it releases the texture the current frame is reading the moment the next material overruns the ceiling.
- Law: residency is MEASURED on the same key. Every admission lands one `AdmitOutcome` — a mint, a reuse of a resident handle, or a refusal the byte ceiling or a backend's own layer contract raised — so the content-key sharing this law buys reads as reuse over admissions, and a refusal, which leaves the slot `Absent` and the shade drawing on its scalar fallback, is counted rather than invisible. A refusal degrades ONE slot; every other fault fails the resolution, because an arity disagreement or an unsupported backend is a roster defect no fallback covers.
- Law: ONLY the roster's own planes upload. Residency folds the bindings, not the set, so a shader naming three slots makes three planes resident out of the full frozen TextureChannel roster and a whole-set upload is the deleted form — a set-wide residency is a roster that names every channel, never a second entry.
- Exemption: three statement bodies, each a native-lifetime or measured boundary and nothing else on the page — `UploadGanesh` sweeps rows through the plane's own decode path into caller-owned `SpanOwner` rentals, the ref-struct rentals and the row-major scatter making an expression fold unrepresentable; `ShadeSupply.Of(ReadOnlySpan<double>)` crosses a span into the strided narrowing operator; `Resident` captures its own mint's identity, which is what separates a mint from the reuse a CAS loser became.
- Entry: `public static Fin<ShaderAssetCache> Of(long planeBudgetBytes, Option<WgpuShaderCompiler> compiler)` mints both lanes — the capsule is a REQUIRED argument, a Ganesh-only composition passing `None` being a real case rather than an omitted knob; `public Fin<ShaderAsset> Compile(ShaderSource source, GpuBackend backend)` elects the arm and takes the program lane, which probes before compiling and disposes a race loser's mint; `public Fin<(ShadeTexture Texture, AdmitOutcome Outcome)> Resident(PlaneUpload request, GpuBackend backend, ShaderArm arm)` makes one plane native under the budget and names which of the three outcomes it was; `public Unit Open()` raises the plane lane's generation and protects it from the pressure sweep; `public ShadePlan Complete(...)` drains the cache counters into the plan that already carries the passes. Ganesh compiles through `SKRuntimeEffect.CreateShader` and uploads through `SKImage.FromPixelCopy`/`SKShader.CreateImage`; Wgpu compiles WGSL into a module and render pipeline whose `Bind(ShadeUniforms)` creates an owned per-draw bind group and whose `Mount(RenderTarget, nint)` records it on the active encoder before release, and uploads through the same capsule's `Upload`.
- Auto: a shader source compiles once per `(Key, Revision, GpuBackend)` cell and a plane chain uploads once per `(Key, GpuBackend)` cell; both probes, both race-loser releases, and the byte ceiling are the cache owner's, so this page states the cost, the release, and the refusal and holds no admission mechanism of its own. `PlaneUpload` carries `TexturePlane` LEVELS rather than a materialized sampler image, so one upload shape serves a channel pyramid, a packed sheet, the stored equirect, the GGX prefilter ladder, and the split-sum LUT — the `AsImage` lift stays the CPU sampler's bridge and never allocates a whole `ShadeVec4` chain to hand the GPU one level.
- Auto: `UploadGanesh` stages level 0 alone and lets `SKMipmapMode.Linear` build Skia's own box chain, so the authored `MipPolicy` — Kaiser, renormalize, or the variance coupling — survives on the Wgpu arm ONLY; that divergence is the declared Ganesh quality floor the plan's own `Backend` column names, never a silent equivalence — a catalog-settled verdict, because no SkiaSharp surface accepts a caller-supplied mip chain: the `SKImage` family admits `FromPixelCopy`/`FromPixels` level-0 images and `ToTextureImage`'s Ganesh-generated chain alone. `EnvironmentRead.Nearest` is the same verdict's LADDER arm — no SkiaSharp surface binds an explicit level set either, so a `Nearest` row seats the roughness-nearest authored level as its own single-level upload cell on a Skia-family backend while a chain-capable family keeps every authored level. Which family seats is the ARM's own `Seats` column, never a family probe at the resolution site.
- Output: `ShadePlan` carries the executable passes, backend, compile refusal, resident-plane state, admission tally, and releases since the previous plan; `ShaderAssetCache.Complete` reads its owned cache cells atomically and `Observe` projects those columns onto the contributed instruments.
- Packages: SkiaSharp, Silk.NET.WebGPU, CommunityToolkit.HighPerformance, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Custody`, `Op`, `FaultBand`), Rasm.Materials (project)
- Growth: a new shader is one `ShaderSource` keyed into the cache; a new shader slot is one `ShaderBinding` row; a new roster or row gate is one `RowGates` row; a new lighting product is one `EnvironmentRead` row carrying its own `Supply` column; a new backend FAMILY is one `ShaderArm` row carrying its compile, upload, and seat columns; a new admission outcome is one `AdmitOutcome` row the fan picks up with no edit; one shader instrument is one `InstrumentSpec` row on `ShaderAssetCache.TelemetryRow`; zero new surface.
- Boundary: the residency cache is keyed per `GpuBackend` — a per-host `GpuBackend`/`GRContext` construction in a shading arm is the `[04]-[BOUNDARIES]` rejected form, so the cache folds the leased context through the `Render/pipeline` `GpuBackend` target-factory column and a backend swap re-compiles and re-uploads one cell; the Ganesh shader is `SKRuntimeEffect` confined to the `Offscreen` capsule so an `SKSurface` outside the capsule is the rejected form.
- Boundary: the wgpu pipeline-state and every wgpu texture share the one `Wgpu` device the viewport leases through the branch `ONE_WGPU_DEVICE` `EMBED_CAPSULE` law, so a second GPU device for shading is the rejected form (`Render/shading ⇄ dotnet:Rasm.Compute # [SHAPE]: shared ONE_WGPU_DEVICE`) — the raw `DeviceCreateTexture`/`TextureCreateView`/`DeviceCreateSampler`/`QueueWriteTexture` table stays inside the composition-bound `WgpuShaderCompiler`, this page holding `nint` handles alone. The runtime arm is SPIKE-gated exactly as the viewport: the CPU `LayeredBsdf` reference shade is the floor and the GPU compile is the SPIKE.
- Boundary: this cache is the 3D-APPEARANCE half of the runtime-shader TYPE-DOMAIN partition and holds appearance programs alone — the 2D chrome roster at `Vfx/shader#EFFECT_PROGRAM` carries no backend variant, no resident plane, and a CPU-side program-and-picture budget, so a chrome program forced through this cache would arrive holding a wgpu pipeline-state arm it can never take, and neither cache holds the other's programs; both rosters are REPO-SHIPPED source, so caller-supplied shader text has no admission on either, and the shader source is backend-neutral so a backend-specific shader literal is the rejected form, the per-backend lowering living in the arm row.
- Boundary: texel lanes arrive DECODED and scene-linear from `TexturePlane.Read`, the plane's own decode ladder — INCLUDING the `pq`/`hlg` display transfers the frozen environment row alone admits, whose ST 2084 and HLG inverses are that ladder's own rows, so a `pq` dome reaches this pass already scene-linear and a transfer this pass cannot name never uploads display-referred — so the uploaded `SKImage` carries no tagged colour space (a tagged space re-transforms lanes the Materials decode already resolved) and a Render-side transfer curve, gamma divide, normal-map decode, SH reconstruction, or prefilter integral is the deleted form.

```csharp
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.AppUi.Theme;
using Rasm.Domain;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Texture;
using Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotWidth {
    private SlotWidth() { }
    public sealed record Exact(int Lanes) : SlotWidth;
    public sealed record Run : SlotWidth;
    public sealed record Plane : SlotWidth;

    public static readonly SlotWidth Variable = new Run();
    public static readonly SlotWidth Sampler = new Plane();
    public static SlotWidth Of(int lanes) => new Exact(lanes);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShaderUniformKind {
    public static readonly ShaderUniformKind Float = new("float", SlotWidth.Of(1));
    public static readonly ShaderUniformKind Float2 = new("float2", SlotWidth.Of(2));
    public static readonly ShaderUniformKind Float3 = new("float3", SlotWidth.Of(3));
    public static readonly ShaderUniformKind Float4 = new("float4", SlotWidth.Of(4));
    public static readonly ShaderUniformKind Matrix = new("matrix", SlotWidth.Of(16));
    public static readonly ShaderUniformKind Int = new("int", SlotWidth.Of(1));
    public static readonly ShaderUniformKind Run = new("run", SlotWidth.Variable);
    public static readonly ShaderUniformKind Texture = new("texture", SlotWidth.Sampler);

    public SlotWidth Width { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShadeSupply {
    private ShadeSupply() { }
    public sealed record Run(Arr<float> Values) : ShadeSupply;
    public sealed record Sampled(PlaneUpload Upload, int Lane) : ShadeSupply;
    public sealed record Absent : ShadeSupply;

    public static readonly ShadeSupply Nothing = new Absent();

    public static ShadeSupply Of(ReadOnlySpan<double> values) {
        if (values.IsEmpty) { return Nothing; }
        float[] lanes = new float[values.Length];
        TensorPrimitives.ConvertTruncating<double, float>(values, lanes);
        return new Run(toArray(lanes));
    }

    public static ShadeSupply Of(Seq<double> values) =>
        values.IsEmpty ? Nothing : new Run(values.Map(static value => (float)value).ToArr());

    public static ShadeSupply Of(UInt128 key, Seq<TexturePlane> levels, SamplerState sampler, Option<MipPolicy> mip, int lane) =>
        levels.IsEmpty ? Nothing : new Sampled(new PlaneUpload(levels, sampler, mip), lane);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnvironmentRead {
    public static readonly EnvironmentRead Irradiance = new("irradiance", ShaderUniformKind.Run, lanes: Sh9.Slots,
        static light => ShadeSupply.Of(light.Products.Irradiance.Bands.Span));
    public static readonly EnvironmentRead SpecularLadder = new("specularLadder", ShaderUniformKind.Run, lanes: 0,
        static light => ShadeSupply.Of(light.Products.RoughnessPerMip));
    public static readonly EnvironmentRead Intensity = new("intensity", ShaderUniformKind.Float, lanes: 1,
        static light => ShadeSupply.Of(Seq(light.Map.Intensity.RadiometricSi)));
    public static readonly EnvironmentRead Rotation = new("rotation", ShaderUniformKind.Float, lanes: 1,
        static light => ShadeSupply.Of(Seq(light.Map.Rotation)));
    public static readonly EnvironmentRead Specular = new("specular", ShaderUniformKind.Texture, lanes: 0, nearest: true,
        static light => ShadeSupply.Of(light.Blobs.Specular.Value, light.Products.Specular, DomeSampler, Option<MipPolicy>.None, lane: 0));
    public static readonly EnvironmentRead BrdfLut = new("brdfLut", ShaderUniformKind.Texture, lanes: 0,
        static light => ShadeSupply.Of(light.Blobs.BrdfLut.Value, Seq(light.Products.BrdfLut), LutSampler, Some(MipPolicy.None), lane: 0));
    public static readonly EnvironmentRead Equirect = new("equirect", ShaderUniformKind.Texture, lanes: 0,
        static light => ShadeSupply.Of(light.Blobs.Equirect.Value, Seq(light.Map.Plane), DomeSampler, Some(MipPolicy.None), lane: 0));

    static readonly SamplerState DomeSampler = new(AddressMode.Repeat, AddressMode.Clamp, FilterMode.Trilinear, UvFrame.Identity);
    static readonly SamplerState LutSampler = new(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Bilinear, UvFrame.Identity);

    public ShaderUniformKind Kind { get; }

    public int Lanes { get; }

    public bool Nearest { get; }

    private EnvironmentRead(string key, ShaderUniformKind kind, int lanes, Func<EnvironmentLight, ShadeSupply> supply)
        : this(kind, lanes, nearest: false, supply) {
    }

    [UseDelegateFromConstructor]
    public partial ShadeSupply Supply(EnvironmentLight light);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShadeSource {
    private ShadeSource() { }
    public sealed record Channel(TextureChannel Row) : ShadeSource;
    public sealed record Lobes : ShadeSource;
    public sealed record Ambient(EnvironmentRead Read) : ShadeSource;

    public static readonly ShadeSource LobeWeights = new Lobes();
    public static ShadeSource Of(TextureChannel row) => new Channel(row);
    public static ShadeSource Of(EnvironmentRead read) => new Ambient(read);

    public int Lanes => Switch(
        channel: static slot => slot.Row.Components,
        lobes: static _ => 0,
        ambient: static slot => slot.Read.Lanes);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShaderArm {
    public static readonly ShaderArm Ganesh = new("ganesh",
        compile: static (cache, source, backend) => cache.CompileGanesh(source, backend),
        upload: static (_, request, _) => ShaderAssetCache.UploadGanesh(request),
        seats: static read => read.Nearest);
    public static readonly ShaderArm Chained = new("chained",
        compile: static (cache, source, backend) => cache.CompileWgpu(source, backend),
        upload: static (cache, request, backend) => cache.Capsule(backend).Bind(capsule => capsule.Upload(request)),
        seats: static _ => false);

    public static Fin<ShaderArm> Of(GpuBackend backend) =>
        backend.Family.Skia
            ? Fin.Succ(Ganesh)
            : backend.Family.Chained
                ? Fin.Succ(Chained)
                : Fin.Fail<ShaderArm>(new ShaderFault.BackendUnsupported(backend.Key));

    [UseDelegateFromConstructor]
    public partial Fin<ShaderAsset> Compile(ShaderAssetCache cache, ShaderSource source, GpuBackend backend);

    [UseDelegateFromConstructor]
    public partial Fin<ShadeTexture> Upload(ShaderAssetCache cache, PlaneUpload request, GpuBackend backend);

    [UseDelegateFromConstructor]
    public partial bool Seats(EnvironmentRead read);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdmitOutcome {
    public static readonly AdmitOutcome Mint = new("mint");
    public static readonly AdmitOutcome Reuse = new("reuse");
    public static readonly AdmitOutcome Refuse = new("refuse");
}

[ComplexValueObject]
public readonly partial struct ProgramKey {
    public string Key { get; }
    public string Revision { get; }
    public string Backend { get; }
}

[ComplexValueObject]
public readonly partial struct PlaneKey {
    public UInt128 Plane { get; }
    public string Backend { get; }
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShaderFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Shader;
    private ShaderFault(string detail) { Detail = detail; }

    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record CompileFailed(string Detail)      : ShaderFault(Detail);
    [FaultCase(1)]
    public sealed partial record BackendUnsupported(string Detail) : ShaderFault(Detail);
    [FaultCase(2)]
    public sealed partial record UniformAbsent(string Detail)      : ShaderFault(Detail);
    [FaultCase(3)]
    public sealed partial record PlaneUnbindable(string Detail)    : ShaderFault(Detail);

    [FaultCase(4)]
    public sealed partial record DeviceLost(Error Cause) : ShaderFault(Cause.Message), ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }

    internal static Validation<Error, Unit> Gate(bool holds, string detail) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new UniformAbsent(detail));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ShaderBinding(
    string Name, ShaderUniformKind Kind, ShadeSource Source, int Group, int Slot, Option<string> Swizzle);

public sealed record ShaderSource(string Key, string Revision, string Sksl, string Wgsl, Seq<ShaderBinding> Bindings) {
    static readonly Seq<Func<ShaderBinding, Option<string>>> RowGates = Seq<Func<ShaderBinding, Option<string>>>(
        static row => row.Kind.Width is SlotWidth.Exact width && width.Lanes != row.Source.Lanes
            ? Some($"{row.Kind.Key} takes {width.Lanes} lanes, source carries {row.Source.Lanes}")
            : None,
        static row => row.Kind.Width is SlotWidth.Plane && row.Source is ShadeSource.Lobes
            ? Some("the lobe-weight vector is no plane")
            : None,
        static row => row.Kind.Width is SlotWidth.Plane && row.Source is ShadeSource.Ambient { Read.Lanes: > 0 } ambient
            ? Some($"{ambient.Read.Key} supplies {ambient.Read.Lanes} lanes, never a plane")
            : None,
        static row => row.Kind.Width is not SlotWidth.Plane && row.Source is ShadeSource.Ambient { Read.Lanes: > 0 } ambient
            && row.Kind != ambient.Read.Kind
            ? Some($"{ambient.Read.Key} publishes {ambient.Read.Kind.Key}, the binding declares {row.Kind.Key}")
            : None,
        static row => row.Kind.Width is SlotWidth.Plane && row.Source is ShadeSource.Channel && row.Swizzle.IsNone
            ? Some("a sampled channel row carries no swizzle slot")
            : None,
        static row => row.Kind.Width is not SlotWidth.Plane && row.Swizzle.IsSome
            ? Some("a scalar row carries a swizzle slot")
            : None);

    public static Fin<ShaderSource> Of(string key, string revision, string sksl, string wgsl, Seq<ShaderBinding> bindings) => (
        ShaderFault.Gate(!bindings.IsEmpty, $"{key}: roster carries no binding"),
        ShaderFault.Gate(bindings.Map(static row => row.Name).Distinct().Count() == bindings.Count,
            $"{key}: duplicate slot name"),
        ShaderFault.Gate(bindings.Map(static row => (row.Group, row.Slot)).Distinct().Count() == bindings.Count,
            $"{key}: colliding wgpu (group, slot)"),
        ShaderFault.Gate(bindings.Count(static row => row.Source is ShadeSource.Lobes) <= 1,
            $"{key}: a second lobe-weight slot"),
        bindings.Traverse(row => Admit(row)).As())
        .Apply((_, _, _, _, _) => new ShaderSource(revision, sksl, wgsl, bindings)).As().ToFin();

    static Validation<Error, Unit> Admit(string key, ShaderBinding row) =>
        RowGates.Traverse(gate => gate(row).Match(
            Some: detail => Validation<Error, Unit>.Fail((Error)new ShaderFault.UniformAbsent($"{key}/{row.Name}: {detail}")),
            None: static () => Validation<Error, Unit>.Success(unit))).As().Map(static _ => unit);
}

public readonly record struct PlaneUpload(
    UInt128 Key, Seq<TexturePlane> Levels, SamplerState Sampler, Option<MipPolicy> Mip) {
    public long Bytes => Levels.Fold(0L, static (sum, level) => sum + (level.Texels * level.Lanes * level.Format.Depth.Bytes));
}

public sealed class ResidentPlane(ShadeTexture texture, long bytes) : IDisposable {
    public ShadeTexture Texture { get; } = texture;
    public long Bytes { get; } = bytes;
    public void Dispose() => Texture.Dispose();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShadeTexture : IDisposable {
    private ShadeTexture() { }
    public sealed record GaneshImage(SKImage Image, SKShader Sampled) : ShadeTexture;
    public sealed record WgpuTexture(nint View, nint Sampler, IDisposable Release) : ShadeTexture;

    public void Dispose() => Switch(
        ganeshImage: static texture => { texture.Sampled.Dispose(); texture.Image.Dispose(); },
        wgpuTexture: static texture => texture.Release.Dispose());
}

public sealed record WgpuPipelineState(
    nint Module,
    nint Pipeline,
    Func<ShadeUniforms, Fin<(nint Handle, IDisposable Release)>> Bind,
    Func<RenderTarget, nint, Fin<Unit>> Mount,
    IDisposable Release) : IDisposable {
    public void Dispose() => Release.Dispose();
}

public sealed record WgpuShaderCompiler(
    Func<ShaderSource, Fin<WgpuPipelineState>> Build,
    Func<PlaneUpload, Fin<ShadeTexture>> Upload);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShaderProgram {
    private ShaderProgram() { }
    public sealed record Ganesh(SKRuntimeEffect Effect) : ShaderProgram;
    public sealed record Wgpu(WgpuPipelineState State) : ShaderProgram;

    public void Release() => Switch(
        ganesh: static program => program.Effect.Dispose(),
        wgpu: static program => program.State.Dispose());
}

public sealed record ShaderAsset(
    string Key,
    GpuBackend Backend,
    ShaderProgram Program,
    Seq<ShaderBinding> Bindings) : IDisposable {
    public void Dispose() => Program.Release();
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ShaderAssetCache : IDisposable {

    const long ProgramCeiling = 1L;

    readonly BudgetedCache<ProgramKey, ShaderAsset> programs;
    readonly BudgetedCache<PlaneKey, ResidentPlane> planes;
    ShaderAssetCache(
        BudgetedCache<ProgramKey, ShaderAsset> programs, BudgetedCache<PlaneKey, ResidentPlane> planes,
        Option<WgpuShaderCompiler> compiler) =>
        (this.programs, this.planes, Compiler) = (programs, planes, compiler);

    internal Option<WgpuShaderCompiler> Compiler { get; }

    public static Fin<ShaderAssetCache> Of(long planeBudgetBytes, Option<WgpuShaderCompiler> compiler) =>
        from programs in BudgetedCache<ProgramKey, ShaderAsset>.Of(ProgramCeiling, RetentionPosture.Holder,
            static _ => 0L, static asset => asset.Dispose(),
            static (at, _) => new ShaderFault.CompileFailed(at.ToString()), Shading)
        from planes in BudgetedCache<PlaneKey, ResidentPlane>.Of(planeBudgetBytes, RetentionPosture.Bound,
            static cell => cell.Bytes, static cell => cell.Dispose(),
            static (at, cost) => new ShaderFault.PlaneUnbindable($"{at.Plane:X32}: {cost} bytes over the plane ceiling"), Shading)
        select new ShaderAssetCache(programs, planes, compiler);

    public Unit Open() => ignore(planes.Retire(static (_, _) => false, advance: true));

    public Fin<ShaderAsset> Compile(ShaderSource source, GpuBackend backend) =>
        ShaderArm.Of(backend).Bind(arm => programs.Take(
            ProgramKey.Create(source.Key, source.Revision, backend.Key),
            () => arm.Compile(this, source, backend)));

    public Fin<(ShadeTexture Texture, AdmitOutcome Outcome)> Resident(PlaneUpload request, GpuBackend backend, ShaderArm arm) {
        ResidentPlane? mint = null;
        return planes
            .Take(PlaneKey.Create(request.Key, backend.Key),
                () => arm.Upload(this, request, backend).Map(texture => mint = new ResidentPlane(texture, request.Bytes)))
            .Map(held => (held.Texture, ReferenceEquals(held, mint) ? AdmitOutcome.Mint : AdmitOutcome.Reuse));
    }

    internal Fin<WgpuShaderCompiler> Capsule(GpuBackend backend) =>
        Compiler.ToFin(new ShaderFault.BackendUnsupported($"{backend.Key}: no wgpu compiler bound"));

    // --- [OPERATIONS] ------------------------------------------------------------------
    internal Fin<ShaderAsset> CompileGanesh(ShaderSource source, GpuBackend backend) =>
        SKRuntimeEffect.CreateShader(source.Sksl, out string error) is { } effect
            ? Declared(source, effect)
                .Map(_ => new ShaderAsset(source.Key, backend, new ShaderProgram.Ganesh(effect), source.Bindings))
                .Rollback(effect)
            : Fin.Fail<ShaderAsset>(new ShaderFault.CompileFailed($"{source.Key}: {error}"));

    static Fin<Unit> Declared(ShaderSource source, SKRuntimeEffect effect) =>
        source.Bindings
            .Bind(row => Seq<(string Name, bool Sampled)>((row.Name, row.Kind.Width is SlotWidth.Plane))
                + row.Swizzle.Map(static name => (Name: name, Sampled: false)).ToSeq())
            .Traverse(slot => ShaderFault.Gate((slot.Sampled ? effect.Children : effect.Uniforms).Contains(slot.Name),
                $"{source.Key}/{slot.Name}: the effect declares no such slot"))
            .As().Map(static _ => unit).ToFin();

    internal Fin<ShaderAsset> CompileWgpu(ShaderSource source, GpuBackend backend) =>
        Capsule(backend)
            .Bind(compiler => compiler.Build(source).MapFail(Classify))
            .Map(state => new ShaderAsset(source.Key, backend, new ShaderProgram.Wgpu(state), source.Bindings));

    static Error Classify(Error fault) =>
        fault is Fault { Retriability: Retriability.TransientCase }
            ? new ShaderFault.DeviceLost(fault)
            : fault;

    internal static Fin<ShadeTexture> UploadGanesh(PlaneUpload request) {
        TexturePlane level = request.Levels.Head;
        if (level.Layers.Value is not 1) {
            return Fin.Fail<ShadeTexture>(new ShaderFault.PlaneUnbindable($"{request.Key:X32}: {level.Layers.Value} layers on the Ganesh arm"));
        }
        (int width, int height, int lanes) = (level.Width.Value, level.Height.Value, level.Lanes);
        using SpanOwner<float> staging = SpanOwner<float>.Allocate(width * height * 4, AllocationMode.Clear);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(width * lanes, AllocationMode.Default);
        using SpanOwner<float> lane = SpanOwner<float>.Allocate(width * lanes, AllocationMode.Default);
        for (int y = 0; y < height; y++) {
            level.Read(y, layer: 0, row.Span);
            TensorPrimitives.ConvertTruncating<double, float>(row.Span, lane.Span);
            for (int x = 0; x < width; x++) {
                ReadOnlySpan<float> texel = lane.Span.Slice(x * lanes, lanes);
                int at = ((y * width) + x) * 4;
                (staging.Span[at], staging.Span[at + 1], staging.Span[at + 2], staging.Span[at + 3]) = (
                    texel[0],
                    lanes > 1 ? texel[1] : texel[0],
                    lanes > 2 ? texel[2] : texel[0],
                    level.Alpha.Carries ? texel[lanes - 1] : 1f);
            }
        }
        SKImageInfo info = new(width, height, SKColorType.RgbaF32, Association(level.Alpha));
        return SKImage.FromPixelCopy(info, MemoryMarshal.AsBytes(staging.Span)) is { } image
            ? Fin.Succ<ShadeTexture>(new ShadeTexture.GaneshImage(image, SKShader.CreateImage(
                image, Tile(request.Sampler.AddressU), Tile(request.Sampler.AddressV), Sampling(request.Sampler))))
            : Fin.Fail<ShadeTexture>(new ShaderFault.PlaneUnbindable($"{request.Key:X32}: pixel copy refused"));
    }

    static SKAlphaType Association(AlphaMode alpha) => alpha.Map(
        straight: SKAlphaType.Unpremul,
        associated: SKAlphaType.Premul,
        none: SKAlphaType.Opaque);

    static SKShaderTileMode Tile(AddressMode address) => address.Map(
        repeat: SKShaderTileMode.Repeat,
        clamp: SKShaderTileMode.Clamp,
        mirror: SKShaderTileMode.Mirror);

    static SKSamplingOptions Sampling(SamplerState sampler) => sampler.Filter.Map(
        nearest: new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None),
        bilinear: new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
        bicubic: new SKSamplingOptions(SKCubicResampler.Mitchell),
        trilinear: new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

    public static readonly InstrumentSpec Compiled = InstrumentSpec.Create(
        "rasm.appui.shader.compiled", InstrumentKind.Count, MeasureForm.Whole, "{shader}",
        "shader compiles by backend", Seq(AppUiTelemetry.BackendSlot), None, None, None);

    public static readonly InstrumentSpec Failed = InstrumentSpec.Create(
        "rasm.appui.shader.failed", InstrumentKind.Count, MeasureForm.Whole, "{shader}",
        "shader compile failures by backend and fault", Seq(AppUiTelemetry.BackendSlot, AppUiTelemetry.FaultSlot), None, None, None);

    public static readonly InstrumentSpec PlaneAdmit = InstrumentSpec.Create(
        "rasm.appui.shader.plane.admit", InstrumentKind.Count, MeasureForm.Whole, "{plane}",
        "plane admissions by backend and outcome", Seq(AppUiTelemetry.BackendSlot, AppUiTelemetry.OutcomeSlot), None, None, None);

    public static readonly InstrumentSpec Plane = InstrumentSpec.Create(
        "rasm.appui.shader.plane.resident", InstrumentKind.Level, MeasureForm.Whole, "{plane}",
        "planes resident under the residency ceiling", Seq<string>(), None, None, None);

    public static readonly InstrumentSpec PlaneByte = InstrumentSpec.Create(
        "rasm.appui.shader.plane.bytes", InstrumentKind.Level, MeasureForm.Whole, "By",
        "plane bytes charged against the residency ceiling", Seq<string>(), None, None, None);

    public static readonly InstrumentSpec PlaneEvict = InstrumentSpec.Create(
        "rasm.appui.shader.plane.evicted", InstrumentKind.Count, MeasureForm.Whole, "{plane}",
        "planes the residency ceiling released", Seq(AppUiTelemetry.BackendSlot), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Compiled, Failed, PlaneAdmit, Plane, PlaneByte, PlaneEvict);

    public ShadePlan Complete(
        Seq<RenderPass> passes, GpuBackend backend, Option<Error> refusal, HashMap<AdmitOutcome, long> admissions) =>
        planes.Seal() switch {
            var sweep => new ShadePlan(
                passes, backend, refusal, sweep.Live, sweep.Bytes, admissions, sweep.Retired),
        };

    public static Fin<Unit> Observe(InstrumentSet set, ShadePlan plan) =>
        InstrumentSet.Tags((AppUiTelemetry.BackendSlot, plan.Backend.Key)) switch {
            var backend => plan.Refusal.Match(
                    Some: fault => FaultObservation.Of(fault).Code.Match(
                        Some: code => set.Write(Failed, 1d, InstrumentSet.Tags(
                            (AppUiTelemetry.BackendSlot, plan.Backend.Key),
                            (AppUiTelemetry.FaultSlot, code))),
                        None: () => set.Write(Failed, 1d, backend)),
                    None: () => set.Write(Compiled, 1d, backend))
                .Bind(_ => toSeq(AdmitOutcome.Items).TraverseM(row => set.Write(PlaneAdmit,
                    plan.Admissions.Find(row).IfNone(0L),
                    InstrumentSet.Tags(
                        (AppUiTelemetry.BackendSlot, plan.Backend.Key),
                        (AppUiTelemetry.OutcomeSlot, row.Key)))).As())
                .Bind(_ => set.Level(Plane, plan.ResidentPlanes))
                .Bind(_ => set.Level(PlaneByte, plan.ResidentBytes))
                .Bind(_ => set.Write(PlaneEvict, plan.ReleasedPlanes, backend)),
        };

    public void Dispose() { planes.Dispose(); programs.Dispose(); }
}
```

## [03]-[SURFACE_SHADE]

- Owner: `ShadeStage` the frame-constant half of a resolution beside the cache and the elected arm; `ShadeMaterial` the per-material half; `ShadeUniforms` the per-material slot map every binding row resolves into beside its own admission tally; `SlotAdmit` the one resolved row; `BoundSlot` the resolved per-slot value; `BoundShade` the mounted per-frame shading artifact; `ShadePlan` the pass rows with the cache facts their execution produced; `ShaderShade` the pass mount on `ShaderAsset`.
- Entry: `public ShadePlan Plan(Seq<ShadeMaterial> materials)` on `ShadeStage` is the ONE composition boundary — it opens a residency generation, compiles once, resolves each material into a `ShadeUniforms`, mounts one `RenderPass` per material, and completes one plan; composition hands `plan.Passes` to the `Render/pipeline` `RenderGraph` it constructs and the plan itself to `ShaderAssetCache.Observe`. `public Fin<RenderPass> Pass(string key, ShadeUniforms uniforms)` on `ShaderAsset` projects the compiled shader and resolved slot map into one `RenderPass` under the material's own key; `public static Fin<ShadeUniforms> Of(ShadeStage stage, ShadeMaterial material)` is the ONE resolution.
- Exemption: one statement body — the composited Ganesh draw, because `SKCanvas.DrawPaint` answers void and the paint it draws through is already the bracket's.
- Law: the resolution splits FRAME-constant from MATERIAL-varying and takes two values. The source, the cache, the backend's elected arm, and the dome hold for every material a frame shades, so re-threading them per material as four of eight positional arguments is the deleted form; the `UvFrame` sits on the MATERIAL because the transform is a bind fact the Materials owner deliberately keeps off its content-addressed set — every sampled slot the material resolves inherits it, while the dome and LUT slots state `UvFrame.Identity` because the prefilter integrated under exactly that mapping.
- Law: ONE resolution answers every source. Each scalar channel reads the Materials closure, a sampled channel reads the set, the lobe row reads the layered weights, and an ambient row reads its own `EnvironmentRead` column — four inputs, one `ShadeSupply` answer, so a fifth source is a row rather than a second uniform struct and a second bind arm. `Absent` answers bind nothing and the shader's declared scalar fallback stands, so a partially-baked material draws.
- Law: the resolution ANSWERS its own ledger. Each `SlotAdmit` carries the admission outcome its row produced or the typed nothing a scalar row produces, and the fold tallies them by `AdmitOutcome` row, so the plan's counts are the resolution's own facts rather than four mutable cells read after the run. A `WriterT` ledger is refused here: the traversal already threads the accumulation it would carry, and a transformer over a fold with one writer buys a second carrier and no new fact.
- Law: image-based lighting is READ, never integrated. `EnvironmentLight` already holds the SH9 irradiance run, the GGX roughness ladder, the split-sum LUT, the stored equirect, and the dome's own intensity and rotation on the owner that prefiltered them, so this pass binds those products and the shader reconstructs a shade the `Render/pathtrace` integrator reaches by transport instead. The prefilter integral and the roughness-to-level formula are the deleted forms — the ladder crosses AS DATA and the shader picks its level by inverse interpolation of that bound `roughnessPerMip` run, the SAME table `IblProducts.SpecularLevel` reads on the producer, so the level a raster shade picks and the level the prefilter wrote agree because both read one table.
- Law: the shader body carries the WHOLE frozen read law in the frozen ORDER — un-rotate the interpolated normal by the bound `rotation`, reconstruct `E(n) = Σ Â_l(i)·L_i·Y_i(n)` against the stored-frame bands, scale by the bound `intensity` after — because the SH run, the specular ladder, and the CDF are STORED-FRAME products and a shader applying either policy out of order re-lights every dome sharing the digest. That reconstruction is a frozen transcription the shared `appearance-set` contract's `sh9Basis` fragment binds — its `$comment` names this shader-side reconstruction beside the C# prefilter, the python projection, and three's PMREM, and its `const` roster carries the nine `(l, m, basis, constant)` rows — proven at its landing against the reconstruction expectation `E(+ẑ) = 2π/3 = 2.0943951023931953` on the directional fixture, never a re-derivation with its own spelling.
- Law: the frozen `sh9` twenty-seven-value gate is CODE at two altitudes over one column. `EnvironmentRead.Lanes` carries each product's own arity — the irradiance row reading the producer's own `Sh9.Slots` — independently of any slot's `ShaderUniformKind`, so the roster admission compares two independent numbers instead of a kind against its own echo, and the resolution refuses a resolved run whose length disagrees with that arity, which is the general form of the SH gate sitting where the values first exist because a run row declares no fixed width at compile. A ladder whose depth is a set fact declares zero and stands down; every fixed-arity product is gated by construction.
- Law: the roughness the ladder seats on is ADMITTED, never defaulted. `UnitInterval` admission rides the kernel `Op.AcceptValidated` bridge, so an out-of-domain specular roughness refuses on the result rather than collapsing to the struct default and silently seating level zero — the brightest mirror level of every dome.
- Auto: the shading pass consumes the `Rasm.Materials/Appearance` `LayeredBsdf` and channel-value closure the Materials lowering produces, the `TextureSet` the `Raster/press` bake or `Raster/set` ingest produced, and the `EnvironmentLight` the `Appearance/environment` prefilter resolved. `Plan` resolves them once per material and `RenderPass.Geometry` mounts the resulting `BoundShade`, so the GPU shader evaluates the same `LayeredBsdf`, the same planes, and the same dome the CPU `Render/pathtrace` integrator shades from — the two integrators are comparable because they read one appearance model and one light rig, not because they were written to match. A plane the ceiling refuses degrades ONE slot and counts a refusal; a roster fault returns a plan with no shade passes and its typed refusal.
- Packages: SkiaSharp, Silk.NET.WebGPU, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project — `Custody`, `Op`), Rasm.Materials (project)
- Growth: a new shading parameter is one `TextureChannel` row at the Materials owner and one `ShaderBinding` row here; a new lighting product is one `EnvironmentRead` row; zero new surface — the shader consumes the roster, never re-derives it, and the roster's cardinality tracks the OpenPBR vector by construction.
- Boundary: the shading pass consumes the Materials `LayeredBsdf`, channel-value closure, `TextureSet`, and `EnvironmentLight`; the `dotnet:Rasm.Materials/Appearance` boundary supplies the closure and the resolved dome and the `dotnet:Rasm.Materials/Raster` boundary supplies planes (`Render <- dotnet:Rasm.Materials/Appearance # [BOUNDARY]: LayeredBsdf / channel-value closure / EnvironmentLight at the shading pass`, `Render <- dotnet:Rasm.Materials/Raster # [BOUNDARY]: TextureSet / TexturePlane levels at the sampler bind`). GPU shader and CPU integrator evaluate the same `LayeredBsdf` over the same planes and the same dome.
- Boundary: `ShadeStage` mounts through the one `Render/pipeline` graph and mints VALUES — the graph is constructed with its pass rows, so this page schedules nothing and holds no frame. Every material resolves to one slot map and bind group at shade time. `Render/pathtrace#LIGHT_RIG` supplies the shared `LightSource` family and resolves the same `EnvironmentLight` row this pass binds. Viewport leases the shared `Wgpu` device. LAYERED sets and cube-face domes reach the Wgpu arm alone, the Ganesh upload declaring the single-layer refusal rather than binding face zero as the whole map, and a wgpu texture reaching the Ganesh bind refuses on the result rather than writing nothing and letting the shader fall back.
- Boundary: the per-bind `UvFrame` enters at the material grain and rides every sampled slot the material resolves — the Materials owner keeps the transform OFF the set so one content-addressed atlas serves N sets, so a set-borne tiling column here would fork that key per consumer and an identity frame assumed at the sampler would silently drop the caller's KHR transform; the dome and LUT samplers state `UvFrame.Identity` because the prefilter integrated under exactly that mapping.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundSlot {
    private BoundSlot() { }
    public sealed record Lanes(Arr<float> Values) : BoundSlot;
    public sealed record Sampled(ShadeTexture Texture, int Lane) : BoundSlot;
}

public readonly record struct SlotAdmit(string Name, Option<BoundSlot> Bound, Option<AdmitOutcome> Outcome);

public sealed record ShadeMaterial(
    string Key, LayeredBsdf Bsdf, Func<TextureChannel, ShadeVec4> Fallback, Option<TextureSet> Set, UvFrame Frame);

public readonly record struct ShadeUniforms(HashMap<string, BoundSlot> Slots, HashMap<AdmitOutcome, long> Admissions) {
    public static Fin<ShadeUniforms> Of(ShadeStage stage, ShadeMaterial material) =>
        stage.Source.Bindings.Traverse(row => Resolve(stage, material, row)).As().Map(Fold);

    static ShadeUniforms Fold(Seq<SlotAdmit> rows) =>
        rows.Fold(
            (Slots: HashMap<string, BoundSlot>(), Admissions: HashMap<AdmitOutcome, long>()),
            static (held, row) => (
                row.Bound.Match(Some: bound => held.Slots.Add(row.Name, bound), None: () => held.Slots),
                row.Outcome.Match(
                    Some: outcome => held.Admissions.AddOrUpdate(outcome, static count => count + 1L, 1L),
                    None: () => held.Admissions)))
        switch {
            var folded => new ShadeUniforms(folded.Slots, folded.Admissions),
        };

    static Fin<SlotAdmit> Resolve(ShadeStage stage, ShadeMaterial material, ShaderBinding row) =>
        Supply(stage, material, row).Bind(supply => supply switch {
            ShadeSupply.Run run when row.Source.Lanes > 0 && run.Values.Count != row.Source.Lanes =>
                Fin.Fail<SlotAdmit>(new ShaderFault.UniformAbsent(
                    $"{row.Name}: source declares {row.Source.Lanes} lanes, resolved run carries {run.Values.Count}")),
            ShadeSupply.Run run => Fin.Succ(new SlotAdmit(row.Name, Some<BoundSlot>(new BoundSlot.Lanes(run.Values)), None)),
            ShadeSupply.Sampled plane => stage.Cache.Resident(plane.Upload, stage.Backend, stage.Arm).Match(
                Succ: admitted => Fin.Succ(new SlotAdmit(
                    row.Name, Some<BoundSlot>(new BoundSlot.Sampled(admitted.Texture, plane.Lane)), Some(admitted.Outcome))),
                Fail: fault => fault is ShaderFault.PlaneUnbindable
                    ? Fin.Succ(new SlotAdmit(row.Name, None, Some(AdmitOutcome.Refuse)))
                    : Fin.Fail<SlotAdmit>(fault)),
            _ => Fin.Succ(new SlotAdmit(row.Name, None, None)),
        });

    static Fin<ShadeSupply> Supply(ShadeStage stage, ShadeMaterial material, ShaderBinding row) =>
        row.Source.Switch(
            state: (Stage: stage, Material: material, Sampled: row.Kind.Width is SlotWidth.Plane),
            channel: static (context, slot) => Fin.Succ(context.Sampled
                ? context.Material.Set.Map(bound => Plane(bound, slot.Row, context.Material.Frame)).IfNone(ShadeSupply.Nothing)
                : ShadeSupply.Of(Take(context.Material.Fallback(slot.Row), slot.Row.Components))),
            lobes: static (context, _) => Fin.Succ(
                ShadeSupply.Of(context.Material.Bsdf.Lobes.Map(static lobe => lobe.Weight.Value))),
            ambient: static (context, slot) => context.Stage.Dome.Match(
                Some: light => slot.Read.Supply(light) switch {
                    ShadeSupply.Sampled ladder when context.Stage.Arm.Seats(slot.Read) =>
                        Seat(ladder, light, context.Material.Fallback),
                    var supply => Fin.Succ(supply),
                },
                None: static () => Fin.Succ(ShadeSupply.Nothing)));

    static Fin<ShadeSupply> Seat(ShadeSupply.Sampled ladder, EnvironmentLight light, Func<TextureChannel, ShadeVec4> fallback) =>
        FactoryBridge.Accept<UnitInterval>(fallback(TextureChannel.SpecularRoughness).X)
            .Map(roughness => ladder.Upload.Levels[Math.Clamp((int)Math.Round(light.SpecularLevel(roughness)), 0, ladder.Upload.Levels.Count - 1)])
            .Map(level => ShadeSupply.Of(level.Key, Seq(level),
                new SamplerState(ladder.Upload.Sampler.AddressU, ladder.Upload.Sampler.AddressV, FilterMode.Bilinear, ladder.Upload.Sampler.Frame),
                Option<MipPolicy>.None, ladder.Lane));

    static ShadeSupply Plane(TextureSet set, TextureChannel channel, UvFrame frame) =>
        set.Channels.Find(channel)
            .Map(pyramid => ShadeSupply.Of(pyramid.Key, pyramid.Levels, Sampler(set, pyramid.Levels.Count, frame),
                Some(pyramid.Policy), lane: 0))
            .IfNone(() => set.Packs
                .Find(pack => pack.Present.Contains(channel))
                .Bind(pack => pack.Pack.Lane(channel).Map(lane => ShadeSupply.Of(
                    pack.Plane.Key, pack.Plane.Levels, Sampler(set, pack.Plane.Levels.Count, frame), Some(pack.Plane.Policy), lane)))
                .IfNone(ShadeSupply.Nothing));

    static SamplerState Sampler(TextureSet set, int levels, UvFrame frame) =>
        new(Address(set), Address(set), levels > 1 ? FilterMode.Trilinear : FilterMode.Bilinear, frame);

    static AddressMode Address(TextureSet set) =>
        set.Tiled.Value().Exists(static proof => proof.Accepted) && set.Udim.IsEmpty ? AddressMode.Repeat : AddressMode.Clamp;

    static Seq<double> Take(ShadeVec4 texel, int components) => Seq(texel.X, texel.Y, texel.Z, texel.W).Take(components);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundShade {
    private BoundShade() { }
    public sealed record GaneshShader(SKShader Shader) : BoundShade;
    public sealed record WgpuBindGroup(
        nint BindGroup,
        Func<RenderTarget, nint, Fin<Unit>> Bind,
        IDisposable Release) : BoundShade {
        public Fin<Unit> Mount(RenderTarget target) => Custody.Bracket(() => Bind(target, BindGroup), Release);
    }

    public Fin<Unit> Mount(RenderTarget target) => Switch(
        state: target,
        ganeshShader: static (active, ganesh) => active.Surface.Match(
            Some: surface => Custody.Bracket(
                acquire: () => new SKPaint { Shader = ganesh.Shader },
                project: paint => Painted(surface, paint),
                key: Mounting),
            None: static () => Fin.Fail<Unit>(new ShaderFault.BackendUnsupported("shade/mount: no raster surface"))),
        wgpuBindGroup: static (active, wgpu) => wgpu.Mount(active));

    static Fin<Unit> Painted(SKSurface surface, SKPaint paint) {
        surface.Canvas.DrawPaint(paint);
        return Fin.Succ(unit);
    }
}

public readonly record struct ShadePlan(
    Seq<RenderPass> Passes,
    GpuBackend Backend,
    Option<Error> Refusal,
    int ResidentPlanes,
    long ResidentBytes,
    HashMap<AdmitOutcome, long> Admissions,
    int ReleasedPlanes);

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record ShadeStage(
    ShaderSource Source, ShaderAssetCache Cache, GpuBackend Backend, ShaderArm Arm, Option<EnvironmentLight> Dome) {
    public static Fin<ShadeStage> Of(
        ShaderSource source, ShaderAssetCache cache, GpuBackend backend, Option<EnvironmentLight> dome) =>
        ShaderArm.Of(backend).Map(arm => new ShadeStage(source, cache, backend, arm, dome));

    public ShadePlan Plan(Seq<ShadeMaterial> materials) =>
        Cache.Open() switch {
            _ => Cache.Compile(Source, Backend)
                .Bind(asset => materials.Traverse(material => Shade(asset, material)).As())
                .Match(
                    Succ: rows => Completed(rows.Map(static row => row.Pass),
                        rows.Fold(HashMap<AdmitOutcome, long>(), static (tally, row) => Merge(tally, row.Admissions)), None),
                    Fail: fault => Completed(Seq<RenderPass>(), HashMap<AdmitOutcome, long>(), Some(fault))),
        };

    Fin<(RenderPass Pass, HashMap<AdmitOutcome, long> Admissions)> Shade(ShaderAsset asset, ShadeMaterial material) =>
        from uniforms in ShadeUniforms.Of(this, material)
        from pass in asset.Pass($"{Source.Key}/{material.Key}", uniforms)
        select (pass, uniforms.Admissions);

    ShadePlan Completed(Seq<RenderPass> passes, HashMap<AdmitOutcome, long> admissions, Option<Error> refusal) =>
        Cache.Complete(passes, Backend, refusal, admissions);

    static HashMap<AdmitOutcome, long> Merge(HashMap<AdmitOutcome, long> tally, HashMap<AdmitOutcome, long> rows) =>
        rows.Fold(tally, static (held, entry) => held.AddOrUpdate(entry.Key, count => count + entry.Value, entry.Value));
}

public static class ShaderShade {
    extension(ShaderAsset asset) {
        public Fin<RenderPass> Pass(string key, ShadeUniforms uniforms) =>
            Fin<RenderPass>.Succ(new RenderPass.Geometry(
                $"shade/{key}",
                CutPhase.Whole,
                static _ => 0L,
                (target, _, _) => asset.Bound(uniforms).Bind(bound => bound.Mount(target)).Map(static _ => 0L)));

        private Fin<BoundShade> Bound(ShadeUniforms uniforms) =>
            asset.Program.Switch(
                state: (Asset: asset, Values: uniforms),
                ganesh: static (context, program) => Custody.Bracket(
                    acquire: program.Effect.BuildShader,
                    project: builder => context.Asset.Bindings
                        .Traverse(row => context.Values.Slots.Find(row.Name).Match(
                            Some: slot => Write(builder, row, slot),
                            None: static () => Fin.Succ(unit)))
                        .As()
                        .Map(_ => (BoundShade)new BoundShade.GaneshShader(builder.Build())),
                    key: Binding),
                wgpu: static (context, program) => program.State.Bind(context.Values)
                    .Map(group => (BoundShade)new BoundShade.WgpuBindGroup(
                        group.Handle, program.State.Mount, group.Release)));
    }

    static Fin<Unit> Write(SKRuntimeShaderBuilder builder, ShaderBinding row, BoundSlot slot) =>
        slot.Switch(
            state: (Builder: builder, Row: row),
            lanes: static (seat, run) => Fin.Succ(ignore(seat.Builder.Uniforms[seat.Row.Name] = run.Values.ToArray())),
            sampled: static (seat, plane) => plane.Texture.Switch(
                state: (seat.Builder, seat.Row, plane.Lane),
                ganeshImage: static (bind, image) =>
                    Fin.Succ(ignore(bind.Builder.Children[bind.Row.Name] = image.Sampled))
                        .Map(_ => bind.Row.Swizzle.Iter(name => ignore(bind.Builder.Uniforms[name] = bind.Lane))),
                wgpuTexture: static (bind, _) => Fin.Fail<Unit>(new ShaderFault.PlaneUnbindable(
                    $"{bind.Row.Name}: a wgpu texture reached the Ganesh bind"))));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Shader compilation, supply resolution, budgeted plane residency, and mounting flow
    accDescr: An admitted roster compiles per backend over the folder's budgeted cache while every binding source resolves to one supply whose sampled answers become budgeted resident planes feeding the bound shade a render pass mounts.
    ShaderSource -->|Of admits| ShadeStage
    ShadeStage -->|Compile| ShaderAssetCache
    ShaderArm -->|compile and upload rows| ShaderAssetCache
    ShaderAssetCache -->|program lane| BudgetedCache
    ShaderAssetCache -->|plane lane| BudgetedCache
    BudgetedCache --> ShaderAsset
    BudgetedCache --> ResidentPlane
    ResidentPlane --> ShadeTexture
    LayeredBsdf --> ShadeSupply
    TextureSet --> ShadeSupply
    EnvironmentLight -->|EnvironmentRead| ShadeSupply
    ShadeSupply -->|Sampled| PlaneUpload
    PlaneUpload --> ShaderAssetCache
    ShadeSupply --> ShadeUniforms
    ShadeTexture --> ShadeUniforms
    ShaderAsset -->|Bound| BoundShade
    ShadeUniforms --> BoundShade
    BoundShade -->|Mount| RenderPass
    RenderPass --> ShadePlan
    ShadePlan --> RenderGraph
    ShadePlan -->|Observe| InstrumentSet
```

## [04]-[NATIVE_BOUNDARY]

- [SHADER_COMPILE]: `ShaderProgram` closes native program ownership over exactly one `SKRuntimeEffect` or `WgpuPipelineState`, and `ShadeTexture` closes native texture ownership over exactly one `SKImage`+`SKShader` pair or one wgpu view/sampler pair. Both lanes of `ShaderAssetCache` probe before native construction, release a concurrent-insertion loser's own mint, retain one program per `(Key, Revision, GpuBackend)` cell and one plane per `(plane content key, GpuBackend)` cell, and release a plane only through the pressure sweep the live generation fences. `SKRuntimeEffect.Uniforms` and `.Children` publish the compiled slot names the roster gates against; a refused compile rolls the effect back through `Custody`.
- [BSDF_SHADE_PORT]: `ShadeUniforms.Of` projects `LayeredBsdf.Lobes` into the weight run, reads each scalar `TextureChannel` through the Materials closure, and reads each `EnvironmentRead` off `EnvironmentLight.Products`/`.Map`/`.Blobs` — `Sh9.Bands` the irradiance run, `IblProducts.RoughnessPerMip` the roughness ladder, `EnvironmentMap.Intensity`/`.Rotation` the stored orientation. `ShaderProgram` binds those values through `SKRuntimeShaderBuilder.Uniforms`/`Children` or the composition-bound wgpu bind-group column, and both arms mount through `BoundShade` on the active `RenderTarget` under `Custody.Bracket`.
- [PLANE_UPLOAD_PORT]: `TexturePlane.Read(int, int, Span<double>)` yields decoded scene-linear lanes off the plane's own ladder, so the upload path consumes plane LEVELS — `TexturePyramid.Levels`, `IblProducts.Specular`, `IblProducts.BrdfLut`, `EnvironmentMap.Plane` — and leaves `AsImage` to the CPU sampler. `UploadGanesh` stages level 0 as RGBA32F through `SKImage.FromPixelCopy(SKImageInfo, ReadOnlySpan<byte>)` and wraps it with `SKShader.CreateImage`, while the Wgpu arm hands the whole `PlaneUpload` to the composition-bound capsule, which owns `DeviceCreateTexture`, `TextureCreateView`, `DeviceCreateSampler`, and `QueueWriteTexture` behind the one device lease.

## [05]-[RESEARCH]

(none)
