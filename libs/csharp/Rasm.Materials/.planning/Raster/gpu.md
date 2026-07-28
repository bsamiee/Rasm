# [MATERIALS_GPU]

THE SURFACELESS BAKE DEVICE AND ITS CLOSED WGSL MODULE TABLE. This page is the ONLY surface in `Rasm.Materials` that spells `Silk.NET.WebGPU`: one `PressDevice` acquires a headless adapter and device (a null `CompatibleSurface`, no window, no swapchain, no present), owns the compiled `ComputePipeline` per kernel behind one cache, and exposes ONE `Dispatch` entry over a closed `WgslKernel` table whose WGSL bodies are FENCE LAW — `noiseField`, `checkerField`, `gradientField`, `mathFold`, `mixFold`, `equirectToCube`, `irradianceSh`, `prefilterSpecular` — so a new GPU kernel is a row carrying its source, its binding roster, its workgroup shape, its host reduction, and its golden vector, never a second device or a second dispatch surface. Silk.NET 2.x is maintenance-mode and 3.x reshapes the binding, so the raw function table is confined to this one internal seam: `press#TEXTURE_PRESS` names a `PressBackend` row and never a WebGPU type, and the swap point is this page alone.

Uniform blocks cross as WORDS, never floats. Every kernel's `Params` struct interleaves `f32`, `u32`, and `i32` members, so a float-typed uniform carrier writes `4f` into a `u32` slot as the bit pattern `0x40000000` and the shader reads one billion — a silent wrong texel no validation layer reports. `KernelUniform` is the one writer, appending each member in the row's declared word order under the WGSL sixteen-byte `vec4` alignment, and every golden fixture declares its block through that same writer, so the fixture and the dispatch cannot disagree about layout. GPU output is THROUGHPUT, never IDENTITY: a `PressDevice` result is a preview or an accelerator product whose bytes are never content-addressed, because GPU `f32` cannot reproduce the CPU `f64` procedural lattice and a GPU-keyed plane forks the content key at its preimage; `press#PRESS_RECEIPT` makes that structural — a `webgpu` press yields a `Preview` carrying no `TextureSet` and therefore no key — and the CPU-versus-GPU divergence this page measures rides the press receipt as TELEMETRY, graded against a benchmark row, never fed into a key. Every kernel ships a GOLDEN VECTOR whose expected value is EXACTLY COMPUTABLE from the algorithm's own definition — including its own quadrature where the kernel integrates — so a driver, backend, or WGSL-compiler regression surfaces as a fixture failure rather than as a subtly wrong texture. The page composes `Silk.NET.WebGPU` and its `Silk.NET.WebGPU.Extensions.WGPU` vendor table, the kernel `Lease<T>` resource rail, `Op` fault key, and `ValidityClaim` receipt fold, the `codec#RASTER_FAULT` band-2460 `Device` case, the `set#TEXTURE_SET` `TextureChannel` roster the parity read joins on, the `graph#MATERIAL_GRAPH` `MathOp`/`MixOp` and `texture#TEXTURE_UV` `NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` vocabularies it LOWERS, and `System.Runtime.InteropServices.Marshal` for the one UTF-8 marshalling boundary — reminting no device wrapper, no shader abstraction, no vocabulary, and no fault.

## [01]-[INDEX]

- [02]-[PRESS_DEVICE]: the `DevicePolicy` row, the `KernelUniform` word writer, the `KernelBuffer`/`KernelBinding` request shape, the `KernelReceipt`, the `PressDevice` headless lifecycle over `Lease<PressDevice>`, and the one `Dispatch` fold with its submission-index drain and error-scope rail.
- [03]-[WGSL_KERNEL]: the `BindingKind` roster, the `KernelReduce` host-fold axis, the `WgslOpCode` lowering table over the appearance vocabularies, the closed eight-row `WgslKernel` table with each row's binding layout and workgroup shape, and the WGSL module bodies as fence law.
- [04]-[GOLDEN_VECTOR]: the `GoldenVector` fixture row per kernel, each expected value exactly computable from the kernel's own definition and quadrature, with the tolerance, the prefix-comparison law, and the parity read the benchmark corpus gates on.

## [02]-[PRESS_DEVICE]

- Owner: `PressDevice` the surfaceless bake device and pipeline cache; `DevicePolicy` the acquisition policy row; `KernelUniform` the WGSL-aligned word writer; `KernelBuffer` `[Union]` the per-binding request; `KernelBinding` the dispatch request; `KernelReceipt` the dispatch evidence.
- Cases: buffer {`Uniform` (a read-only constant block of WGSL words), `Read` (a read-only `f32` storage input), `Write` (a device-written storage output the fold reads back)}.
- Law: buffer POSITION is the WGSL `@binding` index — a request's buffer sequence IS the layout, so a kernel row's declared roster and a caller's request cannot disagree without failing the roster gate loudly rather than reading a wrong slot silently.
- Law: a uniform block is a WORD sequence, not a float array. Every `Params` struct on `[03]` interleaves `f32`, `u32`, and `i32` members, so a float-typed carrier writing `4f` into a `u32` slot hands the shader `0x40000000` read as `1073741824` — a wrong texel no validation layer reports, on every kernel that carries an extent, an octave count, a seed, or an op code.
- Entry: `public static Fin<Lease<PressDevice>> Acquire(DevicePolicy policy, Op key)` mints the headless device on the `Lease<T>` resource rail so the `Owned` case disposes every native handle at the projection's close; `public Fin<KernelReceipt> Dispatch(WgslKernel kernel, KernelBinding binding, Op key)` is the ONE dispatch — it compiles or reuses the pipeline, uploads, records, submits, drains on the submission index, reads back, applies the row's own host reduction, and pops the error scope, so a caller composes an outcome and never sequences the device's internals.
- Packages: Silk.NET.WebGPU (composed — `WebGPU.GetApi()` the function-table root, `CreateInstance`, `InstanceRequestAdapter` with a NULL `RequestAdapterOptions.CompatibleSurface`, `AdapterRequestDevice`, `DeviceGetQueue`, `InstanceProcessEvents`, `DeviceCreateShaderModule` over the `ShaderModuleWGSLDescriptor` chain at `SType.ShaderModuleWgslDescriptor`, `DeviceCreateComputePipeline`, `ComputePipelineGetBindGroupLayout`, `DeviceCreateBindGroup`, `DeviceCreateBuffer`, `QueueWriteBuffer`, `DeviceCreateCommandEncoder`, `CommandEncoderBeginComputePass`, `ComputePassEncoderSetPipeline`/`SetBindGroup`/`DispatchWorkgroups`/`End`, `CommandEncoderCopyBufferToBuffer`, `CommandEncoderFinish`, `BufferMapAsync`, `BufferGetMapState`, `BufferGetMappedRange`, `BufferUnmap`, `DevicePushErrorScope`/`DevicePopErrorScope`, `DeviceCreateQuerySet` and `CommandEncoderWriteTimestamp`/`CommandEncoderResolveQuerySet` gated on `AdapterHasFeature(FeatureName.TimestampQuery)`, `AdapterGetLimits` for the timestamp period, and the `XxxRelease`/`XxxDestroy` pairs), Silk.NET.WebGPU.Extensions.WGPU (composed — `WebGPU.TryGetDeviceExtension<Wgpu>` the loader, `Wgpu.QueueSubmitForIndex(Queue*, nuint, CommandBuffer**) -> ulong` the submission-index mint, `Wgpu.DevicePoll(Device*, Bool32, WrappedSubmissionIndex*)` the DETERMINISTIC drain a surfaceless device closes its readback on, `Wgpu.SetLogCallback` routing the native diagnostic stream into the receipt sink), `Rasm` (project — `Lease<T>`, `Op`, `ValidityClaim`, `IValidityEvidence`), `codec#RASTER_FAULT` (composed — `RasterFault` band 2460), BCL inbox (`Marshal.StringToCoTaskMemUTF8`/`FreeCoTaskMem`/`PtrToStringUTF8` the one UTF-8 marshalling owner, `ConcurrentDictionary` the pipeline cache, `BitConverter.SingleToUInt32Bits` the uniform word projection).
- Growth: a new kernel is one `WgslKernel` row; a new acquisition constraint is one `DevicePolicy` column; a new binding kind is one `KernelBuffer` case; a new uniform member type is one `KernelUniform` append. There is NO per-kernel device, NO managed wrapper renaming the native surface, and NO second `Dispatch` overload — arity and modality both ride the request's own buffer sequence.
- Boundary: the adapter request passes a NULL `CompatibleSurface`, so the same lifecycle yields a device with no window; a bake never opens a viewport to obtain a device, and a folder already holding a device never re-requests one. Because there is no present to pump the event loop, the readback closes on the SUBMISSION INDEX rather than on a spin: `Wgpu.QueueSubmitForIndex` mints the index for the exact submission and `Wgpu.DevicePoll(device, wait: true, &index)` blocks until that submission retires and its map callback has run, so the fold reaches `BufferGetMapState` already `Mapped`. A `wait: false` poll loop around `BufferGetMapState` is the frame-driven form a presented plane uses because it must not block its own frame; a bake has no frame, so the loop only burns a core waiting for the answer the index already names. Readback is two-phase and the row pitch is the trap: `CommandEncoderCopyBufferToBuffer` lands the device result in a `MapRead | CopyDst` staging buffer, and a texture-shaped readback pads `ImageCopyBuffer.Layout.BytesPerRow` to a 256-byte alignment the host un-pads row-wise — a direct span cast over a padded mapped range reads the pad as texels. Validation is a POLICY row, not an unconditional bracket: `DevicePolicy.Validation` arms `DevicePushErrorScope(ErrorFilter.Validation)` around the pass and `DevicePopErrorScope` drains it into a `RasterFault.Device` with the native message preserved, so a proving run pays the scope and a throughput run does not, while `Wgpu.SetLogCallback` routes adapter selection and device-lost onto the same rail unconditionally because a lost device is never optional evidence. Timing is OPTIONAL EVIDENCE: `FeatureName.TimestampQuery` is probed on the adapter before the device requires it, an absent feature leaves `KernelReceipt.GpuNanos` as a typed ABSENCE rather than a zero, because a fabricated zero and an unmeasured pass are the two states a parity benchmark must keep apart. The pipeline cache is CONCURRENT because one leased device serves every binding of a press and a caller may fan them; a `FrozenDictionary` cannot admit a compile and a plain `Dictionary` tears under two. Every native handle releases through its own `XxxRelease`/`XxxDestroy` inside the `Lease<T>` projection window; `PressDevice` implements `IDisposable` solely so the kernel resource rail can carry it, and the `Owned` case's `using` is the platform-forced disposal seam this page declares. The `[EXPRESSION_SPINE]` exemption is the unsafe marshalling spine — descriptor `stackalloc`, pointer plumbing, the uniform word append, and the un-pad copy — which is platform-forced; every admission, dispatch selection, and egress surface is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Concurrent;              // ConcurrentDictionary — the pipeline cache one leased device fans
using System.Collections.Frozen;
using System.Runtime.InteropServices;             // Marshal — the ONE UTF-8 marshalling owner across this boundary
using CommunityToolkit.HighPerformance;           // Span2D — the padded-row un-pad view over a mapped range
using LanguageExt;                                // Seq, Option, Fin
using Rasm.Domain;                                // Op, Lease, ValidityClaim, IValidityEvidence
using Rasm.Materials.Appearance.Graph;            // MathOp, MixOp — the vocabularies this page LOWERS
using Rasm.Materials.Appearance.Texture;          // NoiseBasis, FractalMode, CellularDistance, CellularReturn
using Rasm.Numerics;                              // Dimension
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;            // Wgpu — QueueSubmitForIndex, DevicePoll, SetLogCallback
using Thinktecture;
using static LanguageExt.Prelude;
using Buffer = Silk.NET.WebGPU.Buffer;            // the native handle, never System.Buffer

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// Position IS the @binding index: a request's buffer sequence declares the layout, so a kernel row's roster
// and a caller's request cannot silently disagree. A uniform block is WORDS because every Params struct on
// [03] interleaves f32 with u32 and i32 — a float carrier writing 4f into a u32 slot hands the shader
// 0x40000000, read as 1073741824, on every extent, octave count, seed, and op code that crosses.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KernelBuffer {
    private KernelBuffer() { }
    public sealed record Uniform(ReadOnlyMemory<uint> Words) : KernelBuffer;
    public sealed record Read(ReadOnlyMemory<float> Elements) : KernelBuffer;
    public sealed record Write(int Elements) : KernelBuffer;

    public BindingKind Kind => Switch(
        uniform: static _ => BindingKind.Uniform,
        read:    static _ => BindingKind.Read,
        write:   static _ => BindingKind.Write);

    public int ByteLength => Switch(
        uniform: static u => u.Words.Length * sizeof(uint),
        read:    static r => r.Elements.Length * sizeof(float),
        write:   static w => w.Elements * sizeof(float));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ONE uniform writer: each member appends in the row's declared word order, and Vec4 pads to the sixteen-
// byte boundary WGSL imposes on a vec4 member. A fixture and a dispatch build their blocks through the same
// writer, so a golden vector cannot encode a layout the shader does not read.
public sealed record KernelUniform(Seq<uint> Words) {
    public static readonly KernelUniform Empty = new(Seq<uint>());

    public KernelUniform F32(double value) => new(Words.Add(BitConverter.SingleToUInt32Bits((float)value)));
    public KernelUniform U32(int value) => new(Words.Add(unchecked((uint)value)));
    public KernelUniform I32(int value) => new(Words.Add(unchecked((uint)value)));
    public KernelUniform Extent(Dimension width, Dimension height) => U32(width.Value).U32(height.Value);
    public KernelUniform Pad(int words) => new(Words + toSeq(Enumerable.Repeat(0u, words)));
    // A vec4 member starts on a sixteen-byte boundary; the pad is the alignment WGSL states, not a guess.
    public KernelUniform Vec4(double x, double y, double z, double w) =>
        Pad((4 - (Words.Count % 4)) % 4).F32(x).F32(y).F32(z).F32(w);

    public KernelBuffer Block => new KernelBuffer.Uniform(Words.ToArray());
}

// PowerPreference, BackendType, and Validation are POLICY, never constants: a bake on a discrete adapter and a
// bake on the integrated one are the same row at different values, a CI lane pinning a backend is one column,
// and a proving run arms the error scope a throughput run declines.
public readonly record struct DevicePolicy(PowerPreference Power, BackendType Backend, bool Timestamps, bool Validation) {
    public static readonly DevicePolicy Default = new(PowerPreference.HighPerformance, BackendType.Undefined, Timestamps: true, Validation: true);
    public static readonly DevicePolicy Proving = Default with { Validation = true, Timestamps = false };
}

public sealed record KernelBinding(Seq<KernelBuffer> Buffers, uint GroupsX, uint GroupsY, uint GroupsZ);

// GpuNanos is a TYPED ABSENCE: an adapter without FeatureName.TimestampQuery measured nothing, and publishing
// a zero would read to the parity benchmark as an instantaneous pass. Output is POST-REDUCTION — the row's own
// KernelReduce has already folded per-workgroup partials, so a consumer never re-derives a kernel's tail.
public sealed record KernelReceipt(WgslKernel Kernel, ReadOnlyMemory<float> Output, Option<ulong> GpuNanos, uint Dispatches)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Output.Length, 1),
        ValidityClaim.CountAtLeast((int)Dispatches, 1),
        ValidityClaim.Of(GpuNanos.ForAll(static ticks => ticks > 0UL)));
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one WebGPU seam in Rasm.Materials. Silk.NET 2.x is maintenance-mode and 3.x reshapes the binding, so the
// raw function table stops HERE: press#TEXTURE_PRESS names a PressBackend row and never a WebGPU type.
public sealed unsafe class PressDevice : IDisposable {
    readonly WebGPU api;
    readonly Wgpu vendor;
    readonly Instance* instance;
    readonly Adapter* adapter;
    readonly Device* device;
    readonly Queue* queue;
    readonly DevicePolicy policy;
    readonly ConcurrentDictionary<string, nint> pipelines = new(StringComparer.Ordinal);
    readonly Option<double> timestampPeriod;

    PressDevice(WebGPU api, Wgpu vendor, Instance* instance, Adapter* adapter, Device* device, Queue* queue, DevicePolicy policy, Option<double> timestampPeriod) =>
        (this.api, this.vendor, this.instance, this.adapter, this.device, this.queue, this.policy, this.timestampPeriod) =
        (api, vendor, instance, adapter, device, queue, policy, timestampPeriod);

    // Headless acquisition: CompatibleSurface stays NULL, so the lifecycle yields a device with no window, no
    // SurfaceConfigure, and no present. The adapter and device callbacks retire through InstanceProcessEvents;
    // the extension table loads over the live core and is what every later readback drains through.
    public static Fin<Lease<PressDevice>> Acquire(DevicePolicy policy, Op key) =>
        Bring(policy, key).Map(static device => (Lease<PressDevice>)new Lease<PressDevice>.Owned(device));

    static Fin<PressDevice> Bring(DevicePolicy policy, Op key) {
        /* CreateInstance -> InstanceRequestAdapter(RequestAdapterOptions { CompatibleSurface = null, PowerPreference = policy.Power,
           BackendType = policy.Backend }) -> InstanceProcessEvents until the callback retires -> AdapterHasFeature(FeatureName.TimestampQuery)
           and AdapterGetLimits for the tick period -> AdapterRequestDevice requiring the feature only when policy.Timestamps and the
           adapter carries it -> DeviceGetQueue -> TryGetDeviceExtension<Wgpu> -> SetLogCallback onto the RasterFault.Device sink.
           A null adapter, a null device, or a failed extension load rails RasterFault.Device with the native message. */
        throw new NotImplementedException();
    }

    // The ONE dispatch. Compile-or-reuse, upload, record, submit, drain on the submission index, read back,
    // reduce, pop the error scope: a caller composes the receipt and never sequences the device.
    public Fin<KernelReceipt> Dispatch(WgslKernel kernel, KernelBinding binding, Op key) =>
        from _ in Guard(kernel, binding, key)
        from pipeline in Pipeline(kernel, key)
        from output in Run(kernel, pipeline, binding, key)
        select output;

    // The roster gate reads the ROW, so a kernel's declared binding kinds are the contract a request answers.
    Fin<Unit> Guard(WgslKernel kernel, KernelBinding binding, Op key) =>
        binding.Buffers.Count == kernel.Layout.Count && binding.Buffers.Zip(kernel.Layout).ForAll(static pair => pair.Item1.Kind == pair.Item2)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(RasterFault.Device(key, $"<kernel-layout-mismatch:{kernel.Key}>"));

    // WGSL compiles ONCE per kernel per device, through a concurrent cache because one leased device serves
    // every binding of a press. The source and the entry point are native UTF-8, minted through
    // Marshal.StringToCoTaskMemUTF8 and retired in the compile fold's finally — the interop owner is one for
    // both directions across this boundary. SType.ShaderModuleWgslDescriptor is the live chain tag; the
    // same-valued ShaderModuleWgsldescriptor member is [Obsolete] and never spelled.
    Fin<nint> Pipeline(WgslKernel kernel, Op key) {
        /* GetOrAdd(kernel.Key): ShaderModuleWGSLDescriptor { Chain = { SType = SType.ShaderModuleWgslDescriptor }, Code = utf8 } ->
           DeviceCreateShaderModule -> ComputePipelineDescriptor { Layout = null, Compute = { Module, EntryPoint = "main" } } ->
           DeviceCreateComputePipeline; a null pipeline rails RasterFault.Device. Auto layout is deliberate:
           ComputePipelineGetBindGroupLayout(0) reads what the WGSL @group declarations imply, so the roster is
           stated once, in the shader. */
        throw new NotImplementedException();
    }

    // Record and drain. QueueSubmitForIndex mints the index for THIS submission and DevicePoll(wait: true, &index)
    // blocks until it retires and its map callback has run, so BufferGetMapState is already Mapped when the fold
    // reads it — a wait:false spin is the frame-driven form a presented plane needs and a bake has no frame.
    Fin<KernelReceipt> Run(WgslKernel kernel, nint pipeline, KernelBinding binding, Op key) {
        /* policy.Validation -> DevicePushErrorScope(ErrorFilter.Validation);
           DeviceCreateBuffer per KernelBuffer at its Kind.Usage + one MapRead|CopyDst staging;
           QueueWriteBuffer per Uniform/Read; DeviceCreateBindGroup over ComputePipelineGetBindGroupLayout(0);
           CommandEncoderBeginComputePass -> SetPipeline -> SetBindGroup -> DispatchWorkgroups(GroupsX, GroupsY, GroupsZ) -> End;
           CommandEncoderWriteTimestamp pairs + CommandEncoderResolveQuerySet only when timestampPeriod is Some;
           CommandEncoderCopyBufferToBuffer into staging; CommandEncoderFinish;
           vendor.QueueSubmitForIndex(queue, 1, &commands) -> WrappedSubmissionIndex { Queue = queue, SubmissionIndex = index };
           BufferMapAsync(MapMode.Read); vendor.DevicePoll(device, wait: true, &wrapped); BufferGetMappedRange ->
           copy (un-padding row-wise through Span2D where the readback is texture-shaped) -> BufferUnmap;
           kernel.Reduce folds the raw readback; policy.Validation -> DevicePopErrorScope drains onto RasterFault.Device;
           every handle releases in the fold's finally. */
        throw new NotImplementedException();
    }

    public void Dispose() { /* pipeline cache -> ComputePipelineRelease; queue and device -> DeviceRelease; adapter -> AdapterRelease; instance -> InstanceRelease. Native handles are pointer-wrapped structs released through their own XxxRelease/XxxDestroy, never IDisposable — IDisposable exists here solely so the kernel Lease<T> rail can carry the device. */ }
}
```

## [03]-[WGSL_KERNEL]

- Owner: `WgslKernel` `[SmartEnum<string>]` the closed compute-module table; `BindingKind` `[SmartEnum<string>]` the per-slot binding vocabulary; `KernelReduce` `[SmartEnum<string>]` the host-side tail fold; `WgslOpCode` the appearance-vocabulary lowering table.
- Cases: kernel {`noiseField`, `checkerField`, `gradientField`, `mathFold`, `mixFold`, `equirectToCube`, `irradianceSh`, `prefilterSpecular`} · binding {`uniform`, `read`, `write`} · reduce {`none`, `partialSum`}.
- Law: the WGSL body IS the law, not a summary of one — each row carries its complete module source, so the shader a device compiles and the algorithm this corpus specifies are one text with no second spelling to drift.
- Law: every op code a shader switches on is the CPU vocabulary's own key. `NoiseBasis`, `FractalMode`, `CellularDistance`, and `CellularReturn` are `[SmartEnum<int>]`, so their keys ARE the codes and no third numbering exists; `MathOp` and `MixOp` are string-keyed, so `WgslOpCode` derives their codes from `Items` declaration order behind a `Lazy` accessor — the lowering table is this page's because graph.md owns the vocabulary and this page owns its GPU encoding.
- Entry: the table is the entry — `WgslKernel.Items` is the roster `[04]-[GOLDEN_VECTOR]` iterates and `PressDevice.Dispatch` selects on; `Layout` declares the binding kinds a request answers, `Reduce` declares the host fold the dispatch applies to the raw readback, and `Groups(width, height, layers)` derives the full three-dimensional workgroup count from the row's own workgroup shape so no caller computes a dispatch dimension and a six-face cube dispatches in one call.
- Packages: Silk.NET.WebGPU (the `ShaderModuleWGSLDescriptor` chain each row's `Source` fills, `BufferUsage` each `BindingKind` names), `graph#MATERIAL_GRAPH` (composed — `MathOp`/`MixOp`, the rosters `mathFold` and `mixFold` lower), `texture#TEXTURE_UV` (composed — `NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn`, the rosters `noiseField` lowers), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new GPU kernel is one row carrying source, layout, workgroup shape, host reduction, and golden vector — never a second module table and never an ungated arm. `noiseField` covers the FULL 2D field algebra — every `NoiseBasis`, every `FractalMode`, the whole `CellularDistance` × `CellularReturn` product, the domain warp, and the period wrap — so a real authored noise source previews as itself; `mathFold` and `mixFold` cover the FULL pointwise `MathOp` and `MixOp` rosters, so a real authored graph takes the GPU arm rather than vetoing on the first arithmetic node. A new vocabulary row lands one arm in the corresponding WGSL switch beside its CPU delegate.
- Boundary: every field kernel runs at `@workgroup_size(8, 8, 1)` over a texel grid and every reduction at `@workgroup_size(64, 1, 1)` over a linear range, so the dispatch shape derives from the row and the caller supplies extent alone; `Groups` carries the LAYER axis because a cube map is six faces of one dispatch and a hardcoded `Z = 1` makes `equirectToCube`'s own `gid.z` face selector unreachable. Bind group zero is the only group and the WGSL `@group(0) @binding(n)` declarations ARE the layout the auto-derived `ComputePipelineGetBindGroupLayout(0)` reads — a hand-authored `BindGroupLayout` beside them is a second statement of one fact. Storage buffers carry `f32` element arrays with four elements per RGBA texel, because a `vec4<f32>` storage element imposes a sixteen-byte stride the host un-packs anyway. `noiseField` reproduces the `texture#TEXTURE_UV` FastNoiseLite lattice at `f32` — the same `PrimeX`/`PrimeY` lattice primes, the same `0x27d4eb2d` hash multiplier, the same quintic fade and Hermite smoothstep, the same 2D simplex skew and unskew with the `99.83685446303647` bound, the same `1.4247691104677813` Perlin normalizer, the same cellular jitter radius `0.43701595`, and the same fractal-bounding amplitude cascade — with the twenty-four-direction gradient table GENERATED from its `82.5° − 15°·k` defining sequence rather than transcribed, so the GPU and CPU lattices agree in structure and diverge only in float width; the periodic arm wraps the INTEGER lattice coordinate modulo the row's own period, so a period-wrapped sample is exact rather than approximately periodic, and the SOLID 3D arm is absent by declaration — a 3D basis previewed through a 2D lattice is a wrong preview wearing a right name, so `press#PRESS_PLAN` refuses a solid source at admission rather than lowering half of it. `mathFold` carries the operand POLARITY as a uniform, because the CPU `PortValue.AsScalar` reads a colour's AP1 luminance and a scalar port's own value, and a plane's four lanes cannot tell those apart — a kernel assuming `.x` silently reduces every colour operand to its red channel. `equirectToCube` and `prefilterSpecular` share ONE `faceDir` source fragment concatenated at row construction, because the cube-face mapping is one law and WGSL has no include; the mapping itself is FROZEN — `u = 0.5 + atan2(d.y, d.x) / 2π`, `v = acos(clamp(d.z, −1, 1)) / π`, `v = 0` at `+Z`, `u` increasing counter-clockwise viewed from `+Z` — with the up axis `+Z`, no field and no knob, matching the OpenPBR local frame the whole appearance plane shares. `irradianceSh` writes PER-WORKGROUP PARTIALS — twenty-seven `f32` per workgroup at `workgroup_index * 27` — and its `KernelReduce.PartialSum` row folds them host-side in workgroup-index order, because WGSL has no `f32` atomic and a workgroup-order-dependent atomic sum makes the projection non-deterministic across dispatches; the reduction is a ROW column rather than a caller step, so no consumer re-derives a kernel's tail. `prefilterSpecular` importance-samples the GGX distribution with a Hammersley sequence and weights each sample by `N·L`, discarding the below-horizon half, so the prefiltered value of a constant environment is that constant at every roughness — the property `[04]` gates on.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — the [02] prelude is in scope.)

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BindingKind {
    public static readonly BindingKind Uniform = new("uniform", usage: BufferUsage.Uniform | BufferUsage.CopyDst);
    public static readonly BindingKind Read    = new("read",    usage: BufferUsage.Storage | BufferUsage.CopyDst);
    public static readonly BindingKind Write   = new("write",   usage: BufferUsage.Storage | BufferUsage.CopySrc);
    public BufferUsage Usage { get; }
}

// The host tail a kernel's readback needs, as a ROW: a field kernel's output is its result, while a reduction
// kernel writes per-workgroup partials WGSL cannot atomically fold in f32 without losing determinism. Making
// the fold a column keeps the tail with the kernel that produced it rather than in every consumer.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelReduce {
    public static readonly KernelReduce None       = new("none",       stride: 0, fold: static (raw, _) => raw);
    public static readonly KernelReduce PartialSum = new("partialSum", stride: 27, fold: Sum);

    public int Stride { get; }

    [UseDelegateFromConstructor]
    public partial ReadOnlyMemory<float> Fold(ReadOnlyMemory<float> raw, int stride);

    // Workgroup-index order, fixed: the sum replays identically across dispatches where an atomic would not.
    static ReadOnlyMemory<float> Sum(ReadOnlyMemory<float> raw, int stride) {
        float[] total = new float[stride];
        ReadOnlySpan<float> source = raw.Span;
        for (int offset = 0; offset + stride <= source.Length; offset += stride) {
            for (int slot = 0; slot < stride; slot++) { total[slot] += source[offset + slot]; }
        }
        return total;
    }
}

// The GPU encoding of the appearance vocabularies. NoiseBasis/FractalMode/CellularDistance/CellularReturn are
// [SmartEnum<int>], so their KEYS are the codes and this table never re-numbers them; MathOp and MixOp are
// string-keyed, so their codes derive from Items declaration order behind a Lazy accessor — graph.md owns the
// vocabulary, this page owns its GPU encoding, and neither hand-numbers the other.
public static class WgslOpCode {
    static readonly Lazy<FrozenDictionary<MathOp, uint>> MathCodes =
        new(static () => MathOp.Items.Select(static (row, index) => (Row: row, Code: (uint)index)).ToFrozenDictionary(static e => e.Row, static e => e.Code));

    static readonly Lazy<FrozenDictionary<MixOp, uint>> MixCodes =
        new(static () => MixOp.Items.Select(static (row, index) => (Row: row, Code: (uint)index)).ToFrozenDictionary(static e => e.Row, static e => e.Code));

    public static uint Of(MathOp op) => MathCodes.Value[op];
    public static uint Of(MixOp op) => MixCodes.Value[op];
    public static uint Of(NoiseBasis basis) => unchecked((uint)basis.Key);
    public static uint Of(FractalMode fractal) => unchecked((uint)fractal.Key);
    public static uint Of(CellularDistance metric) => unchecked((uint)metric.Key);
    public static uint Of(CellularReturn projection) => unchecked((uint)projection.Key);
}

// The closed module table. Source is the WHOLE shader, so the text a device compiles and the algorithm this
// corpus specifies are one artefact. Groups derives the full three-dimensional dispatch from the row's own
// workgroup shape and the plan's own extent, layers included.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WgslKernel {
    public static readonly WgslKernel NoiseField        = new("noiseField",        source: Wgsl.NoiseField,                      layout: Field,   x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.NoiseLatticeZero);
    public static readonly WgslKernel CheckerField      = new("checkerField",      source: Wgsl.CheckerField,                    layout: Field,   x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.CheckerParity);
    public static readonly WgslKernel GradientField     = new("gradientField",     source: Wgsl.GradientField,                   layout: Sampled, x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.GradientMidpoint);
    public static readonly WgslKernel MathFold          = new("mathFold",          source: Wgsl.MathFold,                        layout: Binary,  x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.MathTotality);
    public static readonly WgslKernel MixFold           = new("mixFold",           source: Wgsl.MixFold,                         layout: Binary,  x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.MixMultiply);
    public static readonly WgslKernel EquirectToCube    = new("equirectToCube",    source: Wgsl.FaceDir + Wgsl.EquirectToCube,   layout: Sampled, x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Golden.CubeFaceCentre);
    public static readonly WgslKernel IrradianceSh      = new("irradianceSh",      source: Wgsl.IrradianceSh,                    layout: Sampled, x: 64u, y: 1u, reduce: KernelReduce.PartialSum, golden: Golden.ConstantIrradiance);
    public static readonly WgslKernel PrefilterSpecular = new("prefilterSpecular", source: Wgsl.FaceDir + Wgsl.PrefilterSpecular, layout: Sampled, x: 8u, y: 8u, reduce: KernelReduce.None,       golden: Golden.ConstantPrefilter);

    public string Source { get; }
    public Seq<BindingKind> Layout { get; }
    public uint WorkgroupX { get; }
    public uint WorkgroupY { get; }
    public KernelReduce Reduce { get; }
    public GoldenVector Golden { get; }

    // Layers ride the Z axis: a six-face cube is one dispatch, and a hardcoded Z of one makes the cube kernel's
    // own gid.z face selector unreachable.
    public (uint X, uint Y, uint Z) Groups(Dimension width, Dimension height, Dimension layers) =>
        (((uint)width.Value + WorkgroupX - 1) / WorkgroupX, ((uint)height.Value + WorkgroupY - 1) / WorkgroupY, (uint)layers.Value);

    static readonly Seq<BindingKind> Field   = Seq(BindingKind.Uniform, BindingKind.Write);
    static readonly Seq<BindingKind> Sampled = Seq(BindingKind.Uniform, BindingKind.Read, BindingKind.Write);
    static readonly Seq<BindingKind> Binary  = Seq(BindingKind.Uniform, BindingKind.Read, BindingKind.Read, BindingKind.Write);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The WGSL bodies. Each module declares its own bind group zero, so the auto-derived layout reads exactly
// these declarations and no host-side layout restates them.
internal static class Wgsl {
    // The cube-face mapping, shared by the two kernels that project onto a cube. WGSL has no include, so the
    // fragment concatenates at row construction — one law, one text, never two that drift.
    internal const string FaceDir = """
        fn faceDir(face: u32, s: f32, t: f32) -> vec3<f32> {
            switch (face) {
                case 0u: { return normalize(vec3<f32>( 1.0,   -s,   -t)); }
                case 1u: { return normalize(vec3<f32>(-1.0,    s,   -t)); }
                case 2u: { return normalize(vec3<f32>(   s,  1.0,    t)); }
                case 3u: { return normalize(vec3<f32>(   s, -1.0,   -t)); }
                case 4u: { return normalize(vec3<f32>(   s,   -t,  1.0)); }
                default: { return normalize(vec3<f32>(  -s,   -t, -1.0)); }
            }
        }
        """;

    // The FULL 2D FastNoiseLite algebra at f32: four bases, three fractal trajectories, the whole cellular
    // distance x return product, the domain warp, and the period wrap. The 24-direction gradient table
    // GENERATES from its 82.5 - 15*k defining sequence rather than transcribing 128 pairs, and the periodic
    // arm wraps the INTEGER lattice coordinate so a period-wrapped sample is exact, not approximately seamless.
    // The SOLID 3D arm is absent by declaration: previewing a 3D basis through a 2D lattice is a wrong preview
    // wearing a right name, so press#PRESS_PLAN refuses a solid source rather than lowering half of it.
    internal const string NoiseField = """
        struct Params {
            extent: vec2<u32>, frequency: f32, lacunarity: f32,
            gain: f32, weighted: f32, pingPong: f32, jitter: f32,
            period: f32, warpAmp: f32, warpFreq: f32, lo: f32,
            hi: f32, octaves: u32, seed: i32, basis: u32,
            fractal: u32, metric: u32, feature: u32, warpSeed: i32
        };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read_write> dst : array<f32>;

        const PX : i32 = 501125321;
        const PY : i32 = 1136930381;
        const SKEW : f32 = 0.3660254037844386;      // (sqrt(3) - 1) / 2
        const UNSKEW : f32 = 0.21132486540518713;   // (3 - sqrt(3)) / 6

        fn wrap(v: i32, period: i32) -> i32 { if (period <= 0) { return v; } return ((v % period) + period) % period; }
        fn hash(seed: i32, xp: i32, yp: i32) -> i32 { return (seed ^ xp ^ yp) * 668265261; }
        fn grad(seed: i32, xp: i32, yp: i32, xd: f32, yd: f32) -> f32 {
            var h = hash(seed, xp, yp);
            h = h ^ (h >> 15u);
            let idx = (u32(h & 254) >> 1u) % 24u;
            let ang = radians(82.5 - 15.0 * f32(idx));
            return xd * cos(ang) + yd * sin(ang);
        }
        fn valCoord(seed: i32, xp: i32, yp: i32) -> f32 {
            var h = hash(seed, xp, yp);
            h = h * h * 60493;
            h = h ^ (h << 19u);
            return f32(h) * 4.6566128730773926e-10;
        }
        fn fade(t: f32) -> f32 { return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }
        fn hermite(t: f32) -> f32 { return t * t * (3.0 - 2.0 * t); }

        fn perlin(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            let ix = i32(floor(x)); let iy = i32(floor(y));
            let xd0 = x - f32(ix); let yd0 = y - f32(iy);
            let xs = fade(xd0); let ys = fade(yd0);
            let x0 = wrap(ix, period) * PX; let y0 = wrap(iy, period) * PY;
            let x1 = wrap(ix + 1, period) * PX; let y1 = wrap(iy + 1, period) * PY;
            let n00 = grad(seed, x0, y0, xd0, yd0); let n10 = grad(seed, x1, y0, xd0 - 1.0, yd0);
            let n01 = grad(seed, x0, y1, xd0, yd0 - 1.0); let n11 = grad(seed, x1, y1, xd0 - 1.0, yd0 - 1.0);
            return mix(mix(n00, n10, xs), mix(n01, n11, xs), ys) * 1.4247691104677813;
        }

        fn valueBasis(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            let ix = i32(floor(x)); let iy = i32(floor(y));
            let xs = hermite(x - f32(ix)); let ys = hermite(y - f32(iy));
            let x0 = wrap(ix, period) * PX; let y0 = wrap(iy, period) * PY;
            let x1 = wrap(ix + 1, period) * PX; let y1 = wrap(iy + 1, period) * PY;
            return mix(mix(valCoord(seed, x0, y0), valCoord(seed, x1, y0), xs),
                       mix(valCoord(seed, x0, y1), valCoord(seed, x1, y1), xs), ys);
        }

        fn simplex(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            let s = (x + y) * SKEW;
            var i = i32(floor(x + s)); var j = i32(floor(y + s));
            let t = f32(i + j) * UNSKEW;
            let x0 = x - (f32(i) - t); let y0 = y - (f32(j) - t);
            var i1 = 0; var j1 = 1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            let x1 = x0 - f32(i1) + UNSKEW; let y1 = y0 - f32(j1) + UNSKEW;
            let x2 = x0 - 1.0 + 2.0 * UNSKEW; let y2 = y0 - 1.0 + 2.0 * UNSKEW;
            var total = 0.0;
            var t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 > 0.0) { t0 = t0 * t0; total = total + t0 * t0 * grad(seed, wrap(i, period) * PX, wrap(j, period) * PY, x0, y0); }
            var t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 > 0.0) { t1 = t1 * t1; total = total + t1 * t1 * grad(seed, wrap(i + i1, period) * PX, wrap(j + j1, period) * PY, x1, y1); }
            var t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 > 0.0) { t2 = t2 * t2; total = total + t2 * t2 * grad(seed, wrap(i + 1, period) * PX, wrap(j + 1, period) * PY, x2, y2); }
            return total * 99.83685446303647;
        }

        fn metric(dx: f32, dy: f32) -> f32 {
            switch (p.metric) {
                case 1u: { return sqrt(dx * dx + dy * dy); }
                case 2u: { return abs(dx) + abs(dy); }
                case 3u: { return abs(dx) + abs(dy) + dx * dx + dy * dy; }
                default: { return dx * dx + dy * dy; }
            }
        }
        fn worley(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            let ix = i32(floor(x)); let iy = i32(floor(y));
            var f1 = 1e30; var f2 = 1e30; var cell = 0.0;
            for (var oy = -1; oy <= 1; oy = oy + 1) {
                for (var ox = -1; ox <= 1; ox = ox + 1) {
                    let cx = wrap(ix + ox, period) * PX; let cy = wrap(iy + oy, period) * PY;
                    let jx = valCoord(seed, cx, cy) * 0.43701595 * p.jitter;
                    let jy = valCoord(seed + 1, cx, cy) * 0.43701595 * p.jitter;
                    let d = metric(f32(ix + ox) + 0.5 + jx - x, f32(iy + oy) + 0.5 + jy - y);
                    if (d < f1) { f2 = f1; f1 = d; cell = valCoord(seed, cx, cy); }
                    else if (d < f2) { f2 = d; }
                }
            }
            switch (p.feature) {
                case 0u: { return cell; }
                case 2u: { return f2 - 1.0; }
                case 3u: { return (f2 + f1) * 0.5 - 1.0; }
                case 4u: { return f2 - f1 - 1.0; }
                case 5u: { return f2 * f1 * 0.5 - 1.0; }
                case 6u: { if (f2 > 0.0) { return f1 / f2 - 1.0; } return 0.0; }
                default: { return f1 - 1.0; }
            }
        }

        fn basisAt(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            switch (p.basis) {
                case 1u: { return simplex(x, y, seed, period); }
                case 2u: { return valueBasis(x, y, seed, period); }
                case 3u: { return worley(x, y, seed, period); }
                default: { return perlin(x, y, seed, period); }
            }
        }

        fn pingPongWave(t: f32) -> f32 { let w = t - f32(i32(t * 0.5)) * 2.0; if (w < 1.0) { return w; } return 2.0 - w; }
        fn bounding(gain: f32, octaves: u32) -> f32 {
            var amp = abs(gain); var total = 1.0;
            for (var i = 1u; i < octaves; i = i + 1u) { total = total + amp; amp = amp * abs(gain); }
            return 1.0 / total;
        }

        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let uv = (vec2<f32>(f32(gid.x), f32(gid.y)) + vec2<f32>(0.5, 0.5)) / vec2<f32>(f32(p.extent.x), f32(p.extent.y));
            var sx = uv.x * p.frequency; var sy = uv.y * p.frequency;
            if (p.warpAmp != 0.0) {
                let wx = simplex(sx * p.warpFreq, sy * p.warpFreq, p.warpSeed, 0);
                let wy = simplex(sx * p.warpFreq + 137.0, sy * p.warpFreq - 41.0, p.warpSeed, 0);
                sx = sx + wx * p.warpAmp; sy = sy + wy * p.warpAmp;
            }
            let octaves = max(1u, p.octaves);
            var sum = 0.0; var amp = bounding(p.gain, octaves); var freq = 1.0;
            let period = i32(p.period);
            for (var o = 0u; o < octaves; o = o + 1u) {
                let n = basisAt(sx * freq, sy * freq, p.seed + i32(o), i32(f32(period) * freq));
                var damp = 1.0;
                switch (p.fractal) {
                    case 1u: { let f = abs(n); sum = sum + (f * -2.0 + 1.0) * amp; damp = 1.0 - f; }
                    case 2u: { let q = pingPongWave((n + 1.0) * p.pingPong); sum = sum + (q - 0.5) * 2.0 * amp; damp = q; }
                    default: { sum = sum + n * amp; damp = min(n + 1.0, 2.0) * 0.5; }
                }
                amp = amp * mix(1.0, damp, p.weighted) * p.gain;
                freq = freq * p.lacunarity;
            }
            let t = clamp((sum + 1.0) * 0.5, 0.0, 1.0);
            let i = (gid.y * p.extent.x + gid.x) * 4u;
            let v = mix(p.lo, p.hi, t);
            dst[i] = v; dst[i + 1u] = v; dst[i + 2u] = v; dst[i + 3u] = 1.0;
        }
        """;

    internal const string CheckerField = """
        struct Params { extent: vec2<u32>, repeats: u32, pad: u32, even: vec4<f32>, odd: vec4<f32> };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read_write> dst : array<f32>;
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let uv = (vec2<f32>(f32(gid.x), f32(gid.y)) + vec2<f32>(0.5, 0.5)) / vec2<f32>(f32(p.extent.x), f32(p.extent.y));
            let parity = (u32(floor(uv.x * f32(p.repeats))) + u32(floor(uv.y * f32(p.repeats)))) & 1u;
            let c = select(p.even, p.odd, parity == 1u);
            let i = (gid.y * p.extent.x + gid.x) * 4u;
            dst[i] = c.x; dst[i + 1u] = c.y; dst[i + 2u] = c.z; dst[i + 3u] = c.w;
        }
        """;

    // The LUT is resolved host-side in Oklch at Gradient.Of, so the GPU read is an index lerp between adjacent
    // resolved texels — the perceptual hue path is never re-derived here.
    internal const string GradientField = """
        struct Params { extent: vec2<u32>, texels: u32, vertical: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> lut : array<f32>;
        @group(0) @binding(2) var<storage, read_write> dst : array<f32>;
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let uv = (vec2<f32>(f32(gid.x), f32(gid.y)) + vec2<f32>(0.5, 0.5)) / vec2<f32>(f32(p.extent.x), f32(p.extent.y));
            let t = clamp(select(uv.x, uv.y, p.vertical == 1u), 0.0, 1.0) * f32(p.texels - 1u);
            let lo = u32(floor(t)); let hi = min(lo + 1u, p.texels - 1u); let f = t - floor(t);
            let i = (gid.y * p.extent.x + gid.x) * 4u;
            for (var c = 0u; c < 4u; c = c + 1u) { dst[i + c] = mix(lut[lo * 4u + c], lut[hi * 4u + c], f); }
        }
        """;

    // The FULL pointwise MathOp roster. Op codes come from WgslOpCode over the roster's own declaration order,
    // so no third numbering exists. POLARITY is a uniform because the CPU PortValue.AsScalar reads a colour's
    // AP1 luminance and a scalar port's own value — a kernel assuming .x silently reduces every colour operand
    // to its red channel. Totality conventions match the CPU rows exactly: a zero divisor folds divide AND
    // modulo to zero, modulo is FLOORED, a negative sqrt operand clamps to zero, a zero-length normalize
    // returns the zero vector.
    internal const string MathFold = """
        struct Params { extent: vec2<u32>, op: u32, lhsKind: u32, rhsKind: u32, pad0: u32, pad1: u32, pad2: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> lhs : array<f32>;
        @group(0) @binding(2) var<storage, read> rhs : array<f32>;
        @group(0) @binding(3) var<storage, read_write> dst : array<f32>;
        // kind 0 scalar (lane x IS the value), 1 colour (AP1 luminance), 2 vector (length) — the CPU AsScalar rows.
        fn asScalar(kind: u32, v: vec4<f32>) -> f32 {
            switch (kind) {
                case 1u: { return dot(v.xyz, vec3<f32>(0.2722287, 0.6740818, 0.0536895)); }
                case 2u: { return length(v.xyz); }
                default: { return v.x; }
            }
        }
        fn safeDiv(a: f32, b: f32) -> f32 { if (b == 0.0) { return 0.0; } return a / b; }
        fn floorMod(a: f32, b: f32) -> f32 { if (b == 0.0) { return 0.0; } return a - b * floor(a / b); }
        fn norm(v: vec3<f32>) -> vec3<f32> { let l = length(v); if (l == 0.0) { return v; } return v / l; }
        fn schlick(c: f32) -> f32 { let m = clamp(1.0 - c, 0.0, 1.0); let m2 = m * m; return m2 * m2 * m; }
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let i = (gid.y * p.extent.x + gid.x) * 4u;
            let a = vec4<f32>(lhs[i], lhs[i + 1u], lhs[i + 2u], lhs[i + 3u]);
            let b = vec4<f32>(rhs[i], rhs[i + 1u], rhs[i + 2u], rhs[i + 3u]);
            let sa = asScalar(p.lhsKind, a);
            let sb = asScalar(p.rhsKind, b);
            var r = vec4<f32>(0.0, 0.0, 0.0, 1.0);
            switch (p.op) {
                case 0u:  { r = vec4<f32>(a.xyz + b.xyz, 1.0); }
                case 1u:  { r = vec4<f32>(a.xyz - b.xyz, 1.0); }
                case 2u:  { r = vec4<f32>(vec3<f32>(sa * sb), 1.0); }
                case 3u:  { r = vec4<f32>(vec3<f32>(safeDiv(sa, sb)), 1.0); }
                case 4u:  { r = vec4<f32>(vec3<f32>(floorMod(sa, sb)), 1.0); }
                case 5u:  { r = vec4<f32>(a.xyz * sb, 1.0); }
                case 6u:  { r = vec4<f32>(vec3<f32>(pow(sa, sb)), 1.0); }
                case 7u:  { r = vec4<f32>(vec3<f32>(sqrt(max(0.0, sa))), 1.0); }
                case 8u:  { r = vec4<f32>(vec3<f32>(abs(sa)), 1.0); }
                case 9u:  { r = vec4<f32>(vec3<f32>(sin(sa)), 1.0); }
                case 10u: { r = vec4<f32>(vec3<f32>(cos(sa)), 1.0); }
                case 11u: { r = vec4<f32>(vec3<f32>(min(sa, sb)), 1.0); }
                case 12u: { r = vec4<f32>(vec3<f32>(max(sa, sb)), 1.0); }
                case 13u: { r = vec4<f32>(vec3<f32>(dot(a.xyz, b.xyz)), 1.0); }
                case 14u: { r = vec4<f32>(cross(a.xyz, b.xyz), 1.0); }
                case 15u: { r = vec4<f32>(norm(a.xyz), 1.0); }
                case 16u: { r = vec4<f32>(vec3<f32>(clamp(sa, 0.0, 1.0)), 1.0); }
                case 17u: { r = vec4<f32>(vec3<f32>(1.0 - sa), 1.0); }
                case 18u: { r = vec4<f32>(vec3<f32>(schlick(clamp(dot(a.xyz, b.xyz), 0.0, 1.0))), 1.0); }
                default:  { r = a; }
            }
            dst[i] = r.x; dst[i + 1u] = r.y; dst[i + 2u] = r.z; dst[i + 3u] = r.w;
        }
        """;

    // The FULL W3C compositing vocabulary: eleven separable modes plus the four non-separable HSL modes, each
    // blended b over a then lerped by the factor as blend opacity — the same algebra the CPU MixOp row reads
    // out of Unicolour, so a graph taking the GPU arm composites identically in structure. The CPU path clips
    // the blended value into the [0,1] W3C reflectance domain through Unicolour's own Blend, so an HDR
    // intermediate diverges here by that clip alone — the divergence the parity workload measures.
    internal const string MixFold = """
        struct Params { extent: vec2<u32>, mode: u32, pad: u32, factor: f32, pad0: f32, pad1: f32, pad2: f32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> backdrop : array<f32>;
        @group(0) @binding(2) var<storage, read> source : array<f32>;
        @group(0) @binding(3) var<storage, read_write> dst : array<f32>;
        fn lum(c: vec3<f32>) -> f32 { return dot(c, vec3<f32>(0.2722287, 0.6740818, 0.0536895)); }
        fn clipColour(c: vec3<f32>) -> vec3<f32> {
            let l = lum(c); let n = min(c.x, min(c.y, c.z)); let x = max(c.x, max(c.y, c.z));
            var o = c;
            if (n < 0.0) { o = l + (o - l) * l / max(1e-6, l - n); }
            if (x > 1.0) { o = l + (o - l) * (1.0 - l) / max(1e-6, x - l); }
            return o;
        }
        fn setLum(c: vec3<f32>, l: f32) -> vec3<f32> { return clipColour(c + (l - lum(c))); }
        fn sat(c: vec3<f32>) -> f32 { return max(c.x, max(c.y, c.z)) - min(c.x, min(c.y, c.z)); }
        fn setSat(c: vec3<f32>, s: f32) -> vec3<f32> {
            let n = min(c.x, min(c.y, c.z)); let x = max(c.x, max(c.y, c.z));
            if (x <= n) { return vec3<f32>(0.0); }
            return (c - n) * s / (x - n);
        }
        fn softLight(a: f32, b: f32) -> f32 {
            if (b <= 0.5) { return a - (1.0 - 2.0 * b) * a * (1.0 - a); }
            let d = select(((16.0 * a - 12.0) * a + 4.0) * a, sqrt(a), a > 0.25);
            return a + (2.0 * b - 1.0) * (d - a);
        }
        fn separable(mode: u32, a: f32, b: f32) -> f32 {
            switch (mode) {
                case 1u:  { return a * b; }
                case 2u:  { return a + b - a * b; }
                case 3u:  { return select(2.0 * a * b, 1.0 - 2.0 * (1.0 - a) * (1.0 - b), a > 0.5); }
                case 4u:  { return min(a, b); }
                case 5u:  { return max(a, b); }
                case 6u:  { if (b >= 1.0) { return 1.0; } return min(1.0, a / (1.0 - b)); }
                case 7u:  { if (b <= 0.0) { return 0.0; } return 1.0 - min(1.0, (1.0 - a) / b); }
                case 8u:  { return select(2.0 * a * b, 1.0 - 2.0 * (1.0 - a) * (1.0 - b), b > 0.5); }
                case 9u:  { return softLight(a, b); }
                case 10u: { return abs(a - b); }
                case 11u: { return a + b - 2.0 * a * b; }
                default:  { return b; }
            }
        }
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let i = (gid.y * p.extent.x + gid.x) * 4u;
            let a = vec3<f32>(backdrop[i], backdrop[i + 1u], backdrop[i + 2u]);
            let b = vec3<f32>(source[i], source[i + 1u], source[i + 2u]);
            var blended = b;
            switch (p.mode) {
                case 12u: { blended = setLum(setSat(b, sat(a)), lum(a)); }
                case 13u: { blended = setLum(setSat(a, sat(b)), lum(a)); }
                case 14u: { blended = setLum(b, lum(a)); }
                case 15u: { blended = setLum(a, lum(b)); }
                default:  { blended = vec3<f32>(separable(p.mode, a.x, b.x), separable(p.mode, a.y, b.y), separable(p.mode, a.z, b.z)); }
            }
            let r = mix(a, blended, clamp(p.factor, 0.0, 1.0));
            dst[i] = r.x; dst[i + 1u] = r.y; dst[i + 2u] = r.z; dst[i + 3u] = backdrop[i + 3u];
        }
        """;

    // The FROZEN equirect mapping with the +Z up axis and no knob. Face order is the WebGPU cube-array order
    // (+X, -X, +Y, -Y, +Z, -Z) and gid.z is the layer the Groups Z axis dispatches, so one call fills a cube.
    internal const string EquirectToCube = """
        struct Params { face: u32, edge: u32, srcWidth: u32, srcHeight: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> equirect : array<f32>;
        @group(0) @binding(2) var<storage, read_write> dst : array<f32>;
        const TAU : f32 = 6.283185307179586;
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.edge || gid.y >= p.edge) { return; }
            let s = 2.0 * (f32(gid.x) + 0.5) / f32(p.edge) - 1.0;
            let t = 2.0 * (f32(gid.y) + 0.5) / f32(p.edge) - 1.0;
            let d = faceDir(p.face + gid.z, s, t);
            let u = 0.5 + atan2(d.y, d.x) / TAU;
            let v = acos(clamp(d.z, -1.0, 1.0)) / 3.141592653589793;
            let sx = min(p.srcWidth - 1u, u32(u * f32(p.srcWidth)));
            let sy = min(p.srcHeight - 1u, u32(v * f32(p.srcHeight)));
            let si = (sy * p.srcWidth + sx) * 4u;
            let di = ((gid.z * p.edge + gid.y) * p.edge + gid.x) * 4u;
            for (var c = 0u; c < 4u; c = c + 1u) { dst[di + c] = equirect[si + c]; }
        }
        """;

    // SH9 irradiance projection at the FROZEN band order and normalization. Each workgroup writes 27 f32
    // partials at workgroup_index * 27 and the row's KernelReduce.PartialSum folds them host-side in
    // workgroup-index order: WGSL has no f32 atomic, and a workgroup-order-dependent atomic sum makes the
    // projection non-deterministic across dispatches.
    internal const string IrradianceSh = """
        struct Params { width: u32, height: u32, groups: u32, pad: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> equirect : array<f32>;
        @group(0) @binding(2) var<storage, read_write> partials : array<f32>;
        var<workgroup> tile : array<f32, 1728>;
        const PI : f32 = 3.141592653589793;
        fn basis(d: vec3<f32>) -> array<f32, 9> {
            return array<f32, 9>(
                0.28209479177387814,
                0.4886025119029199 * d.y,
                0.4886025119029199 * d.z,
                0.4886025119029199 * d.x,
                1.0925484305920792 * d.x * d.y,
                1.0925484305920792 * d.y * d.z,
                0.31539156525252005 * (3.0 * d.z * d.z - 1.0),
                1.0925484305920792 * d.x * d.z,
                0.5462742152960396 * (d.x * d.x - d.y * d.y));
        }
        @compute @workgroup_size(64, 1, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>, @builtin(local_invocation_index) lid : u32, @builtin(workgroup_id) wid : vec3<u32>) {
            var acc : array<f32, 27>;
            for (var k = 0u; k < 27u; k = k + 1u) { acc[k] = 0.0; }
            let texels = p.width * p.height;
            let stride = p.groups * 64u;
            for (var i = gid.x; i < texels; i = i + stride) {
                let x = i % p.width; let y = i / p.width;
                let phi = (f32(x) + 0.5) / f32(p.width) * 2.0 * PI - PI;
                let theta = (f32(y) + 0.5) / f32(p.height) * PI;
                let d = vec3<f32>(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
                let dw = (2.0 * PI / f32(p.width)) * (PI / f32(p.height)) * sin(theta);
                let b = basis(d);
                let si = i * 4u;
                for (var k = 0u; k < 9u; k = k + 1u) {
                    acc[k * 3u]      = acc[k * 3u]      + equirect[si]      * b[k] * dw;
                    acc[k * 3u + 1u] = acc[k * 3u + 1u] + equirect[si + 1u] * b[k] * dw;
                    acc[k * 3u + 2u] = acc[k * 3u + 2u] + equirect[si + 2u] * b[k] * dw;
                }
            }
            for (var k = 0u; k < 27u; k = k + 1u) { tile[lid * 27u + k] = acc[k]; }
            workgroupBarrier();
            if (lid == 0u) {
                for (var k = 0u; k < 27u; k = k + 1u) {
                    var s = 0.0;
                    for (var t = 0u; t < 64u; t = t + 1u) { s = s + tile[t * 27u + k]; }
                    partials[wid.x * 27u + k] = s;
                }
            }
        }
        """;

    // GGX importance sampling over a Hammersley sequence, weighted by N.L with the below-horizon half
    // discarded and the weight sum normalizing — so a constant environment prefilters to that constant at
    // every roughness, which is exactly the golden vector.
    internal const string PrefilterSpecular = """
        struct Params { edge: u32, face: u32, srcWidth: u32, srcHeight: u32, roughness: f32, samples: u32, pad0: u32, pad1: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> equirect : array<f32>;
        @group(0) @binding(2) var<storage, read_write> dst : array<f32>;
        const PI : f32 = 3.141592653589793;
        const TAU : f32 = 6.283185307179586;
        fn radical(bits: u32) -> f32 {
            var b = bits;
            b = (b << 16u) | (b >> 16u);
            b = ((b & 0x55555555u) << 1u) | ((b & 0xAAAAAAAAu) >> 1u);
            b = ((b & 0x33333333u) << 2u) | ((b & 0xCCCCCCCCu) >> 2u);
            b = ((b & 0x0F0F0F0Fu) << 4u) | ((b & 0xF0F0F0F0u) >> 4u);
            b = ((b & 0x00FF00FFu) << 8u) | ((b & 0xFF00FF00u) >> 8u);
            return f32(b) * 2.3283064365386963e-10;
        }
        fn ggx(u: vec2<f32>, n: vec3<f32>, a: f32) -> vec3<f32> {
            let phi = TAU * u.x;
            let ct = sqrt((1.0 - u.y) / (1.0 + (a * a - 1.0) * u.y));
            let st = sqrt(1.0 - ct * ct);
            let h = vec3<f32>(st * cos(phi), st * sin(phi), ct);
            let up = select(vec3<f32>(0.0, 0.0, 1.0), vec3<f32>(1.0, 0.0, 0.0), abs(n.z) > 0.999);
            let tx = normalize(cross(up, n));
            let ty = cross(n, tx);
            return normalize(tx * h.x + ty * h.y + n * h.z);
        }
        fn sample(d: vec3<f32>) -> vec3<f32> {
            let u = 0.5 + atan2(d.y, d.x) / TAU;
            let v = acos(clamp(d.z, -1.0, 1.0)) / PI;
            let sx = min(p.srcWidth - 1u, u32(u * f32(p.srcWidth)));
            let sy = min(p.srcHeight - 1u, u32(v * f32(p.srcHeight)));
            let si = (sy * p.srcWidth + sx) * 4u;
            return vec3<f32>(equirect[si], equirect[si + 1u], equirect[si + 2u]);
        }
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.edge || gid.y >= p.edge) { return; }
            let s = 2.0 * (f32(gid.x) + 0.5) / f32(p.edge) - 1.0;
            let t = 2.0 * (f32(gid.y) + 0.5) / f32(p.edge) - 1.0;
            let n = faceDir(p.face + gid.z, s, t);
            let a = max(1e-3, p.roughness * p.roughness);
            var acc = vec3<f32>(0.0); var weight = 0.0;
            for (var i = 0u; i < p.samples; i = i + 1u) {
                let u = vec2<f32>(f32(i) / f32(p.samples), radical(i));
                let h = ggx(u, n, a);
                let l = normalize(2.0 * dot(n, h) * h - n);
                let ndl = dot(n, l);
                if (ndl > 0.0) { acc = acc + sample(l) * ndl; weight = weight + ndl; }
            }
            let r = select(vec3<f32>(0.0), acc / weight, weight > 0.0);
            let di = ((gid.z * p.edge + gid.y) * p.edge + gid.x) * 4u;
            dst[di] = r.x; dst[di + 1u] = r.y; dst[di + 2u] = r.z; dst[di + 3u] = 1.0;
        }
        """;
}
```

## [04]-[GOLDEN_VECTOR]

- Owner: `GoldenVector` the per-kernel fixture row; `Golden` the fixture table.
- Law: every expected value is EXACTLY COMPUTABLE from the algorithm's own definition — INCLUDING its own quadrature where the kernel integrates. A Perlin lattice node is exactly zero because both corner displacement vectors vanish there; a checker parity is an integer; a two-texel LUT midpoint is exactly the mean; the constant-radiance SH projection is the closed-form midpoint sum `K · 2π²/h · csc(π/2h)` at the fixture's own height, whose limit is the analytic `2√π` and whose gap IS the midpoint-rule error the resolution fixes; a constant environment prefilters to itself at every roughness because the weight sum normalizes. A transcribed decimal nobody can re-derive is the deleted form, and an ANALYTIC value asserted against a DISCRETE kernel at a tolerance the quadrature cannot reach is the same defect wearing a derivation — the fixture would fail on a correct kernel and pass only after someone loosened it.
- Law: a fixture's `Input` supplies every READ buffer the kernel's `Layout` declares. A sampling kernel whose fixture supplies no input reads an unbound or zero buffer, so its expected value describes a dispatch that never happened.
- Entry: `public static Fin<Unit> Prove(PressDevice device, WgslKernel kernel, Op key)` dispatches the row's own fixture and compares the reduced output's leading `Expected.Length` elements against the row's tolerance; `Golden.All` is the roster the `Projection/benchmarks` parity workload and the proof estate both iterate, so a new kernel row reaches both with no further edit.
- Packages: `[02]-[PRESS_DEVICE]` and `[03]-[WGSL_KERNEL]` (the device and table this proves), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new kernel's fixture is the `golden:` column on its own row — a kernel without one cannot be declared, because the row's constructor takes it.
- Boundary: the comparison is a PREFIX read of the reduced output, because a fixture pins the texels its dispatch determines and a full-plane expectation would restate the kernel; the row declares the extent that makes the prefix meaningful, so `Expected` and `Uniform` are read together or not at all. Tolerance is `1e-5` RELATIVE for accumulating kernels and `1e-6` ABSOLUTE for the exactly-zero and integer-parity cases, matching `f32`'s roughly seven significant decimal digits; a tolerance loose enough to hide a wrong gradient table is worse than no fixture. The irradiance vector doubles as the AXIS discriminator: the companion `L(ω) = ω·ẑ` case places its single non-zero coefficient at `sh_2`, and a `+Y`-up implementation places it at `sh_1` or `sh_3` and fails — which is the one check that catches an up-axis fork every visual comparison passes. The cube fixture pins the up axis at the `+Z` face centre, where `v = 0` exactly, and the azimuth origin at the `+X` face centre, where `atan2(0, 1) = 0` gives `u = 0.5` — the `+Z` centre alone pins nothing about azimuth, because `atan2(0, 0)` at the pole is the one place the mapping's `u` is undefined. A golden failure rails `RasterFault.Device` naming the kernel and the divergent index, and it is a HARD failure rather than a telemetry row: the CPU-versus-GPU divergence a press measures is telemetry precisely because the CPU result is authoritative there, whereas a kernel disagreeing with its own closed-form answer is a broken kernel.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit.)

// --- [MODELS] ------------------------------------------------------------------------------
// Expected values are DERIVED constants, each carrying the identity that produces it, so a reader re-derives
// every number without running anything. Uniform is a KernelUniform so the fixture and the dispatch build the
// same word layout; Input supplies every READ buffer the kernel's Layout declares.
public sealed record GoldenVector(
    string Name, KernelUniform Uniform, Seq<ReadOnlyMemory<float>> Input, ReadOnlyMemory<float> Expected,
    Dimension Width, Dimension Height, Dimension Layers, double Tolerance, bool Relative);

// --- [TABLES] ------------------------------------------------------------------------------
public static class Golden {
    // Perlin at an INTEGER lattice node is exactly 0: both corner displacement components vanish, so every
    // gradient dot product is zero and the fade weights select that corner. A 1x1 plane at frequency 2 puts its
    // one texel centre (uv 0.5) exactly on lattice node (1,1); mapped through lo=-1, hi=1 the field reads 0.
    internal static readonly GoldenVector NoiseLatticeZero = new("noise-lattice-node",
        KernelUniform.Empty.Extent(One, One).F32(2.0).F32(2.0)          // extent, frequency, lacunarity
            .F32(0.5).F32(0.0).F32(2.0).F32(1.0)                        // gain, weighted, pingPong, jitter
            .F32(0.0).F32(0.0).F32(1.0).F32(-1.0)                       // period, warpAmp, warpFreq, lo
            .F32(1.0).U32(1).I32(1337).U32(0)                           // hi, octaves, seed, basis perlin
            .U32(0).U32(0).U32(1).I32(0),                               // fractal fBm, metric, feature, warpSeed
        Input: Seq<ReadOnlyMemory<float>>(),
        Expected: new[] { 0f, 0f, 0f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // repeats = 2 puts (0.25, 0.25) in an even cell (floor(0.5) + floor(0.5) = 0) and (0.75, 0.25) in an odd
    // one (1 + 0 = 1). Integer parity — no float tolerance is involved. repeats is u32, so the word writer's
    // U32 append is what keeps the shader from reading a float bit pattern as a billion-fold repeat count.
    internal static readonly GoldenVector CheckerParity = new("checker-parity",
        KernelUniform.Empty.Extent(Two, Two).U32(2).Pad(1).Vec4(0.0, 0.0, 0.0, 1.0).Vec4(1.0, 1.0, 1.0, 1.0),
        Input: Seq<ReadOnlyMemory<float>>(),
        Expected: new[] { 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f }, Width: Two, Height: Two, Layers: One, Tolerance: 1e-6, Relative: false);

    // A two-texel LUT of 0 and 1 read across a two-texel plane lands at t = 0.25 and t = 0.75 of the one-texel
    // span, so the index lerp is exactly 0.25 and 0.75 — no perceptual work, which is the whole point: the
    // Oklch resolve already happened host-side.
    internal static readonly GoldenVector GradientMidpoint = new("gradient-midpoint",
        KernelUniform.Empty.Extent(Two, One).U32(2).U32(0),
        Input: Seq<ReadOnlyMemory<float>>(new[] { 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f }),
        Expected: new[] { 0.25f, 0.25f, 0.25f, 1f, 0.75f, 0.75f, 0.75f, 1f }, Width: Two, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // The three totality conventions in one dispatch at scalar polarity: floored modulo of -1.5 by 1 is 0.5
    // (never the CLR remainder -0.5), a zero divisor folds to 0, and a negative sqrt operand clamps to 0.
    internal static readonly GoldenVector MathTotality = new("math-totality",
        KernelUniform.Empty.Extent(Three, One).U32(4).U32(0).U32(0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(
            new[] { -1.5f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, -4f, 0f, 0f, 1f },
            new[] { 1f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f }),
        Expected: new[] { 0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f }, Width: Three, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // Multiply at full opacity over a 0.5 backdrop and a 0.5 source is exactly 0.25.
    internal static readonly GoldenVector MixMultiply = new("mix-multiply",
        KernelUniform.Empty.Extent(One, One).U32(1).Pad(1).F32(1.0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(new[] { 0.5f, 0.5f, 0.5f, 1f }, new[] { 0.5f, 0.5f, 0.5f, 1f }),
        Expected: new[] { 0.25f, 0.25f, 0.25f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // Two faces, two facts. The +X centre is d = (1,0,0), so u = 0.5 + atan2(0,1)/2pi = 0.5 and v = acos(0)/pi
    // = 0.5 — the azimuth origin and the equator. The +Z centre is d = (0,0,1), so v = acos(1)/pi = 0 — the up
    // axis. The +Z centre pins NOTHING about azimuth, because atan2(0,0) at the pole is the one place u is
    // undefined, so a fixture resting on it proves half of what it claims. The 4x2 equirect reads its rows at
    // v = 0.25 and v = 0.75 and its columns at u = 0.125..0.875, so the +X face samples texel (2,1) and the
    // +Z face samples texel (2,0): the input paints exactly those two texels and no other.
    internal static readonly GoldenVector CubeFaceCentre = new("cube-face-centre",
        KernelUniform.Empty.U32(0).U32(1).U32(4).U32(2),
        Input: Seq<ReadOnlyMemory<float>>(new[] {
            0f, 0f, 0f, 1f,  0f, 0f, 0f, 1f,  0.25f, 0.5f, 0.75f, 1f,  0f, 0f, 0f, 1f,
            0f, 0f, 0f, 1f,  0f, 0f, 0f, 1f,  1f,    1f,   1f,    1f,  0f, 0f, 0f, 1f }),
        Expected: new[] { 0.25f, 0.5f, 0.75f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // L = 1 over the whole sphere. The kernel is a MIDPOINT quadrature, so its exact answer is the closed-form
    // sum, not the analytic integral: sum over rows of sin((j+0.5)pi/h) is csc(pi/2h), so
    // sh_0 = K * (2pi/w) * (pi/h) * w * csc(pi/2h) = K * 2pi^2/h * csc(pi/2h). At h = 32 that is
    // 0.28209479177387814 * 0.6168502750680849 / 0.049067674327418015 = 3.5462157, whose limit as h grows is
    // the analytic 2*sqrt(pi) = 3.5449077 and whose 3.7e-4 gap IS the midpoint error the height fixes. Every
    // other band is exactly zero. The companion L = w.z case places its single non-zero at sh_2, the AXIS
    // discriminator a +Y-up implementation fails.
    internal static readonly GoldenVector ConstantIrradiance = new("constant-irradiance",
        KernelUniform.Empty.U32(64).U32(32).U32(1).Pad(1),
        Input: Seq<ReadOnlyMemory<float>>(Constant(64 * 32)),
        Expected: new[] { 3.5462157f, 3.5462157f, 3.5462157f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
        Width: SixtyFour, Height: ThirtyTwo, Layers: One, Tolerance: 1e-5, Relative: true);

    // A constant environment prefilters to that constant at EVERY roughness, because the N.L weights sum in
    // both numerator and denominator — the one property that catches a broken importance-sample weight.
    internal static readonly GoldenVector ConstantPrefilter = new("constant-prefilter",
        KernelUniform.Empty.U32(4).U32(4).U32(8).U32(4).F32(0.5).U32(64).Pad(2),
        Input: Seq<ReadOnlyMemory<float>>(Constant(8 * 4)),
        Expected: new[] { 1f, 1f, 1f, 1f }, Width: Four, Height: Four, Layers: One, Tolerance: 1e-5, Relative: true);

    internal static readonly Seq<GoldenVector> All = Seq(NoiseLatticeZero, CheckerParity, GradientMidpoint, MathTotality, MixMultiply, CubeFaceCentre, ConstantIrradiance, ConstantPrefilter);

    // The row's own fixture, dispatched and compared as a PREFIX of the reduced output: the fixture pins the
    // texels its extent determines, and a full-plane expectation would restate the kernel it proves. A
    // divergence is a HARD failure naming the kernel and the index — a kernel disagreeing with its own
    // closed-form answer is broken, where a CPU-versus-GPU gap is telemetry because the CPU result is
    // authoritative there.
    public static Fin<Unit> Prove(PressDevice device, WgslKernel kernel, Op key) =>
        device.Dispatch(kernel, Request(kernel), key).Bind(receipt => Compare(kernel, receipt, key));

    // Buffer ORDER is the layout: the uniform block first, then every declared read plane, then the one write
    // sized at the fixture's own extent — the same sequence PressDevice.Guard checks against the row's roster.
    static KernelBinding Request(WgslKernel kernel) =>
        Bind(kernel, kernel.Golden, kernel.Groups(kernel.Golden.Width, kernel.Golden.Height, kernel.Golden.Layers));

    static KernelBinding Bind(WgslKernel kernel, GoldenVector fixture, (uint X, uint Y, uint Z) groups) =>
        new(fixture.Input.Fold(Seq(fixture.Uniform.Block), static (buffers, plane) => buffers.Add(new KernelBuffer.Read(plane)))
                .Add(new KernelBuffer.Write(fixture.Width.Value * fixture.Height.Value * fixture.Layers.Value * 4)),
            groups.X, groups.Y, groups.Z);

    static Fin<Unit> Compare(WgslKernel kernel, KernelReceipt receipt, Op key) {
        GoldenVector fixture = kernel.Golden;
        ReadOnlySpan<float> expected = fixture.Expected.Span;
        ReadOnlySpan<float> actual = receipt.Output.Span;
        if (actual.Length < expected.Length) { return Fin.Fail<Unit>(RasterFault.Device(key, $"<golden-output-short:{kernel.Key}:{actual.Length}<{expected.Length}>")); }
        for (int i = 0; i < expected.Length; i++) {
            double bound = fixture.Relative ? fixture.Tolerance * Math.Max(1e-6, Math.Abs(expected[i])) : fixture.Tolerance;
            if (Math.Abs(actual[i] - expected[i]) > bound) { return Fin.Fail<Unit>(RasterFault.Device(key, $"<golden-divergence:{fixture.Name}:{i}:{actual[i]:R}!={expected[i]:R}>")); }
        }
        return Fin.Succ(unit);
    }

    static ReadOnlyMemory<float> Constant(int texels) { float[] plane = new float[texels * 4]; Array.Fill(plane, 1f); return plane; }

    static readonly Dimension One = Dimension.Create(1);
    static readonly Dimension Two = Dimension.Create(2);
    static readonly Dimension Three = Dimension.Create(3);
    static readonly Dimension Four = Dimension.Create(4);
    static readonly Dimension ThirtyTwo = Dimension.Create(32);
    static readonly Dimension SixtyFour = Dimension.Create(64);
}
```

## [05]-[RESEARCH]

- [WGSL_INT_SHIFT]-[OPEN]: does the WGSL `i32` right-shift by a `u32` operand (`h >> 15u`, `h << 19u`) and the `i32` masked-index projection in `NoiseField.grad`/`valCoord` compile unchanged on the `wgpu_native` Metal backend, or does the sign-extension rule require an explicit `bitcast<u32>` round trip; verification route is `assay bridge` compiling the row's own source through `DeviceCreateShaderModule` and reading the error scope.
- [SUBGROUP_REDUCTION]-[OPEN]: does the `wgpu_native` Metal backend expose the subgroup extension replacing `IrradianceSh`'s 1728-element workgroup array with a subgroup add, and does that reduction stay deterministic across dispatches; verification route is `AdapterEnumerateFeatures` against the installed native asset, and the workgroup-array floor ships regardless because determinism outranks the tile.
- [TIMESTAMP_PERIOD]-[OPEN]: does `AdapterGetLimits` on the surfaceless adapter report a timestamp period converting `QueryType.Timestamp` ticks to nanoseconds on this backend, or must `KernelReceipt.GpuNanos` stay in ticks with the period as a receipt column; verification route is `AdapterGetLimits` beside a measured pair on a known-duration dispatch.
- [SOLID_NOISE_LOWERING]-[OPEN]: does a 3D `NoiseBasis` arm — OpenSimplex2's rotated two-cell fold, the 3x3x3 cellular neighbourhood, and the `0.39614353` jitter radius — carry its own WGSL row, or does the triplanar preview stay a CPU-only lane; verification route is a `Projection/benchmarks` parity workload over a solid-noise plan once the 2D rows measure clean, and the plan admission refuses a solid source until it lands.
