# [MATERIALS_GPU]

THE SURFACELESS BAKE DEVICE AND ITS CLOSED WGSL MODULE TABLE. This page is the ONLY surface in `Rasm.Materials` that spells `Silk.NET.WebGPU`: one `PressDevice` acquires a headless adapter and device (a null `CompatibleSurface`, no window, no swapchain, no present), owns the compiled `ComputePipeline` per kernel behind one cache, and exposes ONE `Dispatch` entry over a closed `WgslKernel` table whose WGSL bodies are FENCE LAW — `noiseField`, `checkerField`, `gradientField`, `mathFold`, `mixFold`, `equirectToCube`, `irradianceSh`, `prefilterSpecular` — so a new GPU kernel is a row carrying its source, its binding roster, its workgroup shape, its host reduction, and its golden vector, never a second device or a second dispatch surface. Silk.NET 2.x is maintenance-mode and 3.x reshapes the binding, so the raw function table is confined to this one internal seam: `press#TEXTURE_PRESS` names a `PressBackend` row and never a WebGPU type, and the swap point is this page alone.

Uniform blocks cross as WORDS, never floats. Every kernel's `Params` struct interleaves `f32`, `u32`, and `i32` members, so a float-typed uniform carrier writes `4f` into a `u32` slot as the bit pattern `0x40000000` and the shader reads one billion — a silent wrong texel no validation layer reports. `KernelUniform` is the one writer, appending each member in the row's declared word order under the WGSL sixteen-byte `vec4` alignment, and every golden fixture declares its block through that same writer, so the fixture and the dispatch cannot disagree about layout. GPU output is THROUGHPUT, never IDENTITY: a `PressDevice` result is a preview or an accelerator product whose bytes are never content-addressed, because GPU `f32` cannot reproduce the CPU `f64` procedural lattice and a GPU-keyed plane forks the content key at its preimage; `press#PRESS_RECEIPT` makes that structural — a `webgpu` press yields a `Preview` carrying no `TextureSet` and therefore no key — and the CPU-versus-GPU divergence this page measures rides the press receipt as TELEMETRY, graded against a benchmark row, never fed into a key. Every kernel ships a GOLDEN VECTOR whose expected value is EXACTLY COMPUTABLE from the algorithm's own definition — including its own quadrature where the kernel integrates — so a driver, backend, or WGSL-compiler regression surfaces as a fixture failure rather than as a subtly wrong texture. The page composes `Silk.NET.WebGPU` and its `Silk.NET.WebGPU.Extensions.WGPU` vendor table, the kernel `Lease<T>` resource rail, `Op` fault key, and `ValidityClaim` receipt fold, the `codec#RASTER_FAULT` band-2460 `Device` case, the `set#TEXTURE_SET` `TextureChannel` roster the parity read joins on, the `graph#MATERIAL_GRAPH` `MathOp`/`MixOp` and `texture#TEXTURE_UV` `NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn` vocabularies it LOWERS, and `System.Runtime.InteropServices.Marshal` for the one UTF-8 marshalling boundary — reminting no device wrapper, no shader abstraction, no vocabulary, and no fault.

## [01]-[INDEX]

- [02]-[PRESS_DEVICE]: the `DevicePolicy` row, the `KernelUniform` word writer, the `KernelBuffer`/`KernelBinding` request shape, the `KernelReceipt`, the `PressDevice` headless lifecycle over `Lease<PressDevice>`, and the one `Dispatch` fold with its submission-index drain and error-scope rail.
- [03]-[WGSL_KERNEL]: the `BindingKind` roster, the `KernelReduce` host-fold axis, the `WgslOpCode` lowering table over the appearance vocabularies, the closed `WgslKernel` table with each row's binding layout and workgroup shape, and the WGSL module bodies as fence law.
- [04]-[GOLDEN_VECTOR]: the `GoldenVector` fixture row per kernel, each expected value exactly computable from the kernel's own definition and quadrature, with the tolerance, the prefix-comparison law, and the parity read the benchmark corpus gates on.
- [05]-[KERNEL_CHAIN]: the `ChainNode` lowering request, the linear-scan buffer allocator over the compiled order, the `ChainPlan` slot pool with its three admission ceilings, and the plural `Dispatch` modality that executes a chain in one submission.
- [06]-[RESEARCH]: open epistemic debt with its verification route.

## [02]-[PRESS_DEVICE]

- Owner: `PressDevice` the surfaceless bake device and pipeline cache; `DevicePolicy` the acquisition policy row; `KernelUniform` the WGSL-aligned word writer; `KernelBuffer` `[Union]` the per-binding request; `KernelBinding` the dispatch request; `KernelReceipt` the dispatch evidence; `PressDevice.NativeSlot` the ONE completion slot every native callback writes.
- Cases: buffer {`Uniform` (a read-only constant block of WGSL words), `Read` (a read-only `f32` storage input), `Write` (a device-written storage output the fold reads back)}.
- Law: buffer POSITION is the WGSL `@binding` index — a request's buffer sequence IS the layout, so a kernel row's declared roster and a caller's request cannot disagree without failing the roster gate loudly rather than reading a wrong slot silently.
- Law: a uniform block is a WORD sequence, not a float array. Every `Params` struct on `[03]` interleaves `f32`, `u32`, and `i32` members, so a float-typed carrier writing `4f` into a `u32` slot hands the shader `0x40000000` read as `1073741824` — a wrong texel no validation layer reports, on every kernel that carries an extent, an octave count, a seed, or an op code.
- Entry: `public static Fin<Lease<PressDevice>> Acquire(DevicePolicy policy, Op key)` mints the headless device on the `Lease<T>` resource rail so the `Owned` case disposes every native handle at the projection's close; `public Fin<KernelReceipt> Dispatch(WgslKernel kernel, KernelBinding binding, Op key)` is the ONE dispatch — it compiles or reuses the pipeline, uploads, records, submits, drains on the submission index, reads back, applies the row's own host reduction, and pops the error scope, so a caller composes an outcome and never sequences the device's internals.
- Packages: Silk.NET.WebGPU (composed — `WebGPU.GetApi()` the function-table root, `CreateInstance`, `InstanceRequestAdapter` with a NULL `RequestAdapterOptions.CompatibleSurface`, `AdapterRequestDevice` over a `DeviceDescriptor.RequiredLimits` chaining the adapter's own `SupportedLimits.Limits`, `DeviceGetQueue`, `InstanceProcessEvents`, `DeviceCreateShaderModule` over the `ShaderModuleWGSLDescriptor` chain at `SType.ShaderModuleWgslDescriptor`, `DeviceCreateComputePipeline`, `ComputePipelineGetBindGroupLayout`, `DeviceCreateBindGroup`, `DeviceCreateBuffer`, `QueueWriteBuffer`, `DeviceCreateCommandEncoder`, `CommandEncoderBeginComputePass`, `ComputePassEncoderSetPipeline`/`SetBindGroup`/`DispatchWorkgroups`/`End`, `CommandEncoderCopyBufferToBuffer`, `CommandEncoderFinish`, `BufferMapAsync`, `BufferGetMapState`, `BufferGetMappedRange`, `BufferUnmap`, `DevicePushErrorScope`/`DevicePopErrorScope`, `DeviceCreateQuerySet` and `CommandEncoderWriteTimestamp`/`CommandEncoderResolveQuerySet` gated on `AdapterHasFeature(FeatureName.TimestampQuery)`, `AdapterGetLimits(Adapter*, SupportedLimits*) -> Bool32` and `DeviceGetLimits(Device*, SupportedLimits*) -> Bool32` — both answers railed, the DEVICE's block feeding the `Limits.MaxComputeWorkgroupsPerDimension`/`MaxComputeInvocationsPerWorkgroup`/`MaxStorageBufferBindingSize` ceilings the dispatch gate reads — and the void `XxxRelease` pairs including `QueueRelease`/`DeviceRelease`/`AdapterRelease`/`InstanceRelease`), Silk.NET.WebGPU.Extensions.WGPU (composed — `WebGPU.TryGetDeviceExtension<Wgpu>` the loader, `InstanceExtras` chained onto `InstanceDescriptor.NextInChain` at `NativeSType.STypeInstanceExtras` carrying the `InstanceBackend` mask and the `InstanceFlag` word, `Wgpu.QueueSubmitForIndex(Queue*, nuint, CommandBuffer**) -> ulong` the submission-index mint, `Wgpu.DevicePoll(Device*, Bool32, WrappedSubmissionIndex*) -> Bool32` the DETERMINISTIC drain a surfaceless device closes its readback on, `Wgpu.SetLogLevel`/`SetLogCallback` routing the native diagnostic stream into the receipt sink), `Rasm` (project — `Lease<T>`, `Op`, `ValidityClaim`, `IValidityEvidence`), `codec#RASTER_FAULT` (composed — `RasterFault` band 2460), BCL inbox (`Marshal.StringToCoTaskMemUTF8`/`FreeCoTaskMem` the one UTF-8 marshalling owner, `Encoding.UTF8.GetString(byte*, int)` the native-message projection, `UnmanagedCallersOnly` over `CallConvCdecl` the thunk-free completion shape, `MemoryHandle` the upload pin, `ConcurrentDictionary` the pipeline cache, `BitConverter.SingleToUInt32Bits` the uniform word projection).
- Growth: a new kernel is one `WgslKernel` row; a new acquisition constraint is one `DevicePolicy` column; a new binding kind is one `KernelBuffer` case; a new uniform member type is one `KernelUniform` append. There is NO per-kernel device, NO managed wrapper renaming the native surface, and NO second `Dispatch` overload — arity and modality both ride the request's own buffer sequence.
- Boundary: the adapter request passes a NULL `CompatibleSurface`, so the same lifecycle yields a device with no window; a bake never opens a viewport to obtain a device, and a folder already holding a device never re-requests one. Because there is no present to pump the event loop, the readback closes on the SUBMISSION INDEX rather than on a spin: `Wgpu.QueueSubmitForIndex` mints the index for the exact submission and `Wgpu.DevicePoll(device, wait: true, &index)` blocks until that submission retires and its map callback has run, so the fold reaches `BufferGetMapState` already `Mapped`. A `wait: false` poll loop around `BufferGetMapState` is the frame-driven form a presented plane uses because it must not block its own frame; a bake has no frame, so the loop only burns a core waiting for the answer the index already names. Readback is two-phase and BUFFER-SHAPED throughout: `CommandEncoderCopyBufferToBuffer` lands the device result in a `MapRead | CopyDst` staging buffer, and a buffer copy carries no row pitch, so the mapped range reads as one flat `f32` run. The 256-byte `ImageCopyBuffer.Layout.BytesPerRow` alignment a host un-pads row-wise belongs to `CommandEncoderCopyTextureToBuffer`, and this device allocates no texture — that padding becomes real the day a kernel row declares a storage-texture binding, which is the declared growth leg and not a cost paid today. Validation is a POLICY row, not an unconditional bracket, and it arms BOTH halves from one column: `DevicePolicy.Validation` sets `InstanceFlag.Validation` in the chained `InstanceExtras` so the native layer runs at all, and arms `DevicePushErrorScope(ErrorFilter.Validation)` around the pass, whose `DevicePopErrorScope` drains that layer's verdict into a `RasterFault.Device` with the native message preserved — a proving run pays both and a throughput run neither, where a scope armed over an unarmed layer would drain clean on a malformed dispatch. Backend selection is likewise INSTANCE state: `DevicePolicy.Backends` rides `InstanceExtras.Backends` as a flags MASK constraining which backends the instance builds, which is strictly more than the single `RequestAdapterOptions.BackendType` narrowing one request against an instance that already built them all. `Wgpu.SetLogCallback` routes adapter selection and device-lost onto the same rail unconditionally because a lost device is never optional evidence. The error scope IS the compile-diagnostics channel: `wgpuShaderModuleGetCompilationInfo` is UNIMPLEMENTED in wgpu-native and ABORTS the process — a non-unwinding panic, never a failed call — so a WGSL refusal reaches the host through the `DevicePushErrorScope`/`DevicePopErrorScope` pair and the log sink alone, and no fence on this page spells `GetCompilationInfo`. Timing is OPTIONAL EVIDENCE: `FeatureName.TimestampQuery` is probed on the adapter before the device requires it — requiring a feature the adapter lacks refuses the whole device and trades every dispatch for one timing column — and an absent feature leaves `KernelReceipt.GpuNanos` as a typed ABSENCE rather than a zero, because a fabricated zero and an unmeasured pass are the two states a parity benchmark must keep apart. Resolved query values are NANOSECONDS and this binding exposes no tick period to convert against — `Limits` carries dispatch ceilings alone and no period member — so the resolved delta IS the receipt column and no conversion step exists to get wrong. Limits are negotiated in BOTH directions and read at BOTH subjects for different reasons: the adapter's `SupportedLimits.Limits` chains back as the `DeviceDescriptor.RequiredLimits` requirement, because an unset requirement asks for the specification's conservative defaults rather than the hardware's headroom, and the DEVICE's own `DeviceGetLimits` block is what the dispatch gate compares against, because a device grants at or below what it was asked. Both reads answer `Bool32` and both answers rail, since a false answer leaves the struct zeroed and a zero ceiling refuses every dispatch as a breach rather than as the unread block it is. The pipeline cache is CONCURRENT because one leased device serves every binding of a press and a caller may fan them; a `FrozenDictionary` cannot admit a compile and a plain `Dictionary` tears under two. Every native handle releases through its own `XxxRelease` inside the `Lease<T>` projection window, and every per-dispatch handle releases in the recording fold's `finally` on the refusal path as much as the success one; `PressDevice` implements `IDisposable` solely so the kernel resource rail can carry it, and the `Owned` case's `using` is the platform-forced disposal seam this page declares. Native completions are `[UnmanagedCallersOnly]` over `CallConvCdecl` writing the CALLER's own stack slot, never `Pfn…Callback.From(delegate)`: the managed-delegate form mints one pinned `SilkMarshal` thunk per construction, so a device brought once per press with a map callback per binding leaks one thunk per dispatch. The slot copies the native message INLINE because the `byte*` a callback receives dies with the call and an unmanaged callee cannot hand a managed string back through a field; a slot storing that pointer renders freed bytes into a fault detail. The `[EXPRESSION_SPINE]` exemption is the unsafe marshalling spine — descriptor construction, pointer plumbing, the uniform word append, and the recording fold — which is platform-forced; every admission, dispatch selection, and egress surface is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers;                             // MemoryHandle — the upload pin, one per buffer write
using System.Collections.Concurrent;              // ConcurrentDictionary — the pipeline cache one leased device fans
using System.Collections.Frozen;
using System.Runtime.CompilerServices;            // CallConvCdecl — the native completion calling convention
using System.Runtime.InteropServices;             // Marshal, UnmanagedCallersOnly — the interop owners
using System.Text;                                // Encoding — the ONE native-message projection
using System.Threading;                           // Lock — the diagnostic-slot gate
using LanguageExt;                                // Seq, Option, Fin
using Rasm.Domain;                                // Op, Lease, ValidityClaim, IValidityEvidence
using Rasm.Materials.Appearance.Graph;            // MathOp, MixOp — the vocabularies this page LOWERS
using Rasm.Materials.Appearance.Texture;          // NoiseBasis, FractalMode, CellularDistance, CellularReturn, ShadeVec4
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
    // A WgslOpCode lowering is ALREADY a word, so it appends without a signed round trip a caller could get
    // wrong; the separate name keeps a vocabulary code from reading as an arbitrary integer parameter.
    public KernelUniform Code(uint value) => new(Words.Add(value));
    public KernelUniform Extent(Dimension width, Dimension height) => U32(width.Value).U32(height.Value);
    public KernelUniform Pad(int words) => new(Words + toSeq(Enumerable.Repeat(0u, words)));
    // A vec4 member starts on a sixteen-byte boundary; the pad is the alignment WGSL states, not a guess.
    public KernelUniform Vec4(double x, double y, double z, double w) =>
        Pad((4 - (Words.Count % 4)) % 4).F32(x).F32(y).F32(z).F32(w);
    // The field register crosses as ONE vec4 append, so a colour column reaches a shader through the same four
    // lanes the CPU arm lerps and no caller unpacks a ShadeVec4 by hand at a call site.
    public KernelUniform Vec4(ShadeVec4 value) => Vec4(value.X, value.Y, value.Z, value.W);

    public KernelBuffer Block => new KernelBuffer.Uniform(Words.ToArray());
}

// PowerPreference, the backend MASK, and Validation are POLICY, never constants: a bake on a discrete adapter
// and a bake on the integrated one are the same row at different values, a CI lane pinning a backend is one
// column, and the two rows genuinely SPLIT — Default is the throughput lane (no error scope, timestamps for the
// parity telemetry) and Proving arms both halves of validation. Two rows that differ only in name are one policy
// wearing two labels. Backends is the INSTANCE-level flags mask rather than a single BackendType a per-adapter
// request narrows to: wgpu-native honours the mask while it BUILDS backends, so one column states the whole
// admissible set and an instance never stands up a backend the policy excluded.
public readonly record struct DevicePolicy(PowerPreference Power, InstanceBackend Backends, bool Timestamps, bool Validation) {
    public static readonly DevicePolicy Default = new(PowerPreference.HighPerformance, InstanceBackend.All, Timestamps: true, Validation: false);
    public static readonly DevicePolicy Proving = Default with { Validation = true };
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
    // Every native completion writes ONE slot shape: whether it fired, the status row it carried, the handle it
    // produced, and an INLINE UTF-8 copy of its message. The copy is the whole point — the byte* a callback
    // receives lives only for that call, and an [UnmanagedCallersOnly] body cannot hand a managed string back
    // through a field, so a slot storing the pointer would render freed bytes into a fault detail. Fired is
    // separate from Status because every status vocabulary here spells success as zero, and a slot that never
    // fired would read as a clean success.
    internal struct NativeSlot {
        internal const int MessageCap = 256;
        internal int Fired;
        internal int Status;
        internal nint Handle;
        internal int Length;
        internal fixed byte Message[MessageCap];
    }

    // The native diagnostic stream carries no userdata slot of its own, so the last error-or-warning line lands
    // in ONE process-static slot every refusal on this page appends to its detail. A bounded last-line slot is
    // the honest sink: an unbounded buffer behind a native callback grows on a stream nothing drains. The gate
    // serializes writer and reader — the log callback fires on arbitrary native threads while a concurrent
    // dispatch renders a refusal, and an unguarded fixed-buffer copy tears Message against Length.
    static NativeSlot diagnostic;
    static readonly Lock DiagnosticGate = new();

    readonly WebGPU api;
    readonly Wgpu vendor;
    readonly Instance* instance;
    readonly Adapter* adapter;
    readonly Device* device;
    readonly Queue* queue;
    readonly DevicePolicy policy;
    readonly ConcurrentDictionary<string, nint> pipelines = new(StringComparer.Ordinal);
    // Timestamps is the ADAPTER's answer folded with the policy's request, never the policy alone — an adapter
    // without FeatureName.TimestampQuery yields a device that cannot resolve a query set, and Limits is the
    // negotiated ceiling every dispatch admission reads before it records a workgroup count the backend refuses.
    readonly bool timestamps;
    readonly Limits limits;

    PressDevice(WebGPU api, Wgpu vendor, Instance* instance, Adapter* adapter, Device* device, Queue* queue, DevicePolicy policy, bool timestamps, Limits limits) =>
        (this.api, this.vendor, this.instance, this.adapter, this.device, this.queue, this.policy, this.timestamps, this.limits) =
        (api, vendor, instance, adapter, device, queue, policy, timestamps, limits);

    // Headless acquisition: CompatibleSurface stays NULL, so the lifecycle yields a device with no window, no
    // SurfaceConfigure, and no present. The adapter and device callbacks retire through InstanceProcessEvents;
    // the extension table loads over the live core and is what every later readback drains through.
    public static Fin<Lease<PressDevice>> Acquire(DevicePolicy policy, Op key) =>
        Bring(policy, key).Map(static device => (Lease<PressDevice>)new Lease<PressDevice>.Owned(device));

    // Each request is followed by a BOUNDED InstanceProcessEvents pump rather than an open spin: wgpu-native
    // retires both callbacks synchronously on this backend, and the bound turns a callback that never fires into
    // a named refusal instead of a hang. Every failure releases what it already brought, in reverse order.
    static Fin<PressDevice> Bring(DevicePolicy policy, Op key) {
        const int PumpBound = 1024;
        WebGPU api = WebGPU.GetApi();
        // The backend mask and the validation layer are INSTANCE state in wgpu-native, chained through
        // InstanceExtras at NativeSType.STypeInstanceExtras — the vendor chain tag is a NativeSType value cast
        // onto the core SType field, which is how every wgpu extras struct seats itself. Setting them here
        // constrains backend construction itself, where RequestAdapterOptions.BackendType only narrows one
        // request against an instance that already built every backend. InstanceFlag.Validation arms the native
        // layer whose diagnostics the policy's error scope then collects, so ONE column drives both halves and
        // an armed scope can never read an unarmed layer.
        InstanceExtras extras = new() {
            Chain = new ChainedStruct { SType = (SType)NativeSType.STypeInstanceExtras },
            Backends = policy.Backends,
            Flags = (uint)(policy.Validation ? InstanceFlag.Validation : InstanceFlag.Default),
        };
        InstanceDescriptor instanceDescriptor = new() { NextInChain = (ChainedStruct*)&extras };
        Instance* instance = api.CreateInstance(&instanceDescriptor);
        if (instance is null) { return Fin.Fail<PressDevice>(RasterFault.Device(key, $"<wgpu-instance-null:{Diagnostic()}>")); }

        NativeSlot slot = default;
        RequestAdapterOptions options = new() { CompatibleSurface = null, PowerPreference = policy.Power };
        api.InstanceRequestAdapter(instance, &options, new PfnRequestAdapterCallback(&OnAdapter), &slot);
        for (int pump = 0; slot.Fired is 0 && pump < PumpBound; pump++) { api.InstanceProcessEvents(instance); }
        Adapter* adapter = (Adapter*)slot.Handle;
        if (adapter is null) {
            api.InstanceRelease(instance);
            return Fin.Fail<PressDevice>(RasterFault.Device(key, $"<wgpu-adapter:{(RequestAdapterStatus)slot.Status}:{Detail(&slot)}>"));
        }

        // AdapterGetLimits answers Bool32 and a FALSE answer leaves the struct at its zero default, where a
        // MaxComputeWorkgroupsPerDimension of zero refuses every later dispatch as a ceiling breach rather than
        // as the unread limits it is — so the answer rails HERE and never reaches the gate as data.
        SupportedLimits supported = default;
        if (!api.AdapterGetLimits(adapter, &supported)) {
            api.AdapterRelease(adapter);
            api.InstanceRelease(instance);
            return Fin.Fail<PressDevice>(RasterFault.Device(key, $"<adapter-limits-unread:{Diagnostic()}>"));
        }
        // The feature is probed on the ADAPTER before the device requires it: requiring a feature the adapter
        // lacks refuses the whole device, which would trade every dispatch for one optional timing column.
        bool timestamps = policy.Timestamps && api.AdapterHasFeature(adapter, FeatureName.TimestampQuery);

        slot = default;
        FeatureName required = FeatureName.TimestampQuery;
        // The adapter's OWN answer is what the device requires. A null RequiredLimits requests the WebGPU
        // DEFAULT ceilings, which sit far below a discrete adapter's — a 16k plane would refuse against the
        // conservative floor while the hardware carries the headroom, and the refusal would name a limit no
        // operator can raise. Chaining the adapter's supported block back as the requirement makes the
        // negotiated device carry the whole adapter.
        RequiredLimits requiredLimits = new() { Limits = supported.Limits };
        DeviceDescriptor deviceDescriptor = new() {
            RequiredFeatureCount = timestamps ? (nuint)1 : 0,
            RequiredFeatures = timestamps ? &required : null,
            RequiredLimits = &requiredLimits,
        };
        api.AdapterRequestDevice(adapter, &deviceDescriptor, new PfnRequestDeviceCallback(&OnDevice), &slot);
        for (int pump = 0; slot.Fired is 0 && pump < PumpBound; pump++) { api.InstanceProcessEvents(instance); }
        Device* device = (Device*)slot.Handle;
        if (device is null) {
            api.AdapterRelease(adapter);
            api.InstanceRelease(instance);
            return Fin.Fail<PressDevice>(RasterFault.Device(key, $"<wgpu-device:{(RequestDeviceStatus)slot.Status}:{Detail(&slot)}>"));
        }

        // The GATE reads the DEVICE's block, never the adapter's. A device grants what the backend honours for
        // the requested feature set, which is at or below the requirement — so admitting a dispatch against the
        // adapter's headroom passes a workgroup count the device then refuses mid-record, and the two blocks are
        // the same struct read at two different subjects, which is exactly how one silently stands for the other.
        SupportedLimits negotiated = default;
        if (!api.DeviceGetLimits(device, &negotiated)) {
            api.DeviceRelease(device);
            api.AdapterRelease(adapter);
            api.InstanceRelease(instance);
            return Fin.Fail<PressDevice>(RasterFault.Device(key, $"<device-limits-unread:{Diagnostic()}>"));
        }

        if (!api.TryGetDeviceExtension(device, out Wgpu vendor)) {
            api.DeviceRelease(device);
            api.AdapterRelease(adapter);
            api.InstanceRelease(instance);
            return Fin.Fail<PressDevice>(RasterFault.Device(key, "<wgpu-vendor-extension-absent>"));
        }
        // The log route is UNCONDITIONAL where the error scope is a policy row: a lost device is never optional
        // evidence, and the verbosity floor keeps the sink at the two levels a refusal quotes.
        vendor.SetLogLevel(LogLevel.Warn);
        vendor.SetLogCallback(new PfnLogCallback(&OnLog), null);
        return Fin.Succ(new PressDevice(api, vendor, instance, adapter, device, api.DeviceGetQueue(device), policy, timestamps, negotiated.Limits));
    }

    // The ONE dispatch. Compile-or-reuse, upload, record, submit, drain on the submission index, read back,
    // reduce, pop the error scope: a caller composes the receipt and never sequences the device.
    // The roster-drift verdict reads a FROZEN value rather than re-walking two rosters per dispatch: the answer is
    // fixed the moment the vocabularies finish initializing, so it is a proof the type already holds and a
    // dispatch consults it.
    public Fin<KernelReceipt> Dispatch(WgslKernel kernel, KernelBinding binding, Op key) =>
        from _ in WgslOpCode.Total(key)
        from __ in Guard(kernel, binding, key)
        from pipeline in Pipeline(kernel, key)
        from output in Run(kernel, pipeline, binding, key)
        select output;

    // The roster gate reads the ROW, so a kernel's declared binding kinds are the contract a request answers,
    // and the negotiated Limits gate the dispatch the backend would otherwise refuse mid-record. The refusal
    // quotes the reached value beside the granted one on every ceiling, so an operator reads how far past the
    // request went rather than only that it did.
    Fin<Unit> Guard(WgslKernel kernel, KernelBinding binding, Op key) =>
        binding.Buffers.Count != kernel.Layout.Count || !binding.Buffers.Zip(kernel.Layout).ForAll(static pair => pair.Item1.Kind == pair.Item2)
            ? Fin.Fail<Unit>(RasterFault.Device(key, $"<kernel-layout-mismatch:{kernel.Key}>"))
            : Ceilings(kernel, binding).Find(static ceiling => ceiling.Reached > ceiling.Granted)
                .Map(ceiling => (Error)RasterFault.Device(key, $"<{ceiling.Name}-ceiling:{kernel.Key}:{ceiling.Reached}:{ceiling.Granted}>"))
                .Match(Some: Fin.Fail<Unit>, None: static () => Fin.Succ(unit));

    // --- [DISPATCH_CEILING]
    // Three negotiated ceilings, ONE shape — what the request reaches against what the device granted — so a
    // fourth is a row rather than a fourth nested arm. A 16k plane reaches the per-dimension workgroup count
    // first and a full-resolution storage plane the binding size, while the row's own workgroup shape is
    // measured against the invocation budget because a shape outgrowing it refuses at PIPELINE CREATION, whose
    // validation message names the module rather than the dispatch and reaches the caller as a compile refusal
    // for a kernel that compiles everywhere else. Each ceiling costs one comparison against a validation abort.
    Seq<(string Name, ulong Reached, ulong Granted)> Ceilings(WgslKernel kernel, KernelBinding binding) =>
        Seq(("workgroup",  (ulong)Math.Max(binding.GroupsX, Math.Max(binding.GroupsY, binding.GroupsZ)), (ulong)limits.MaxComputeWorkgroupsPerDimension),
            ("invocation", (ulong)kernel.WorkgroupX * kernel.WorkgroupY,                                 (ulong)limits.MaxComputeInvocationsPerWorkgroup),
            ("storage",    binding.Buffers.Filter(static buffer => buffer.Kind != BindingKind.Uniform)
                                          .Fold(0UL, static (widest, buffer) => Math.Max(widest, (ulong)buffer.ByteLength)), limits.MaxStorageBufferBindingSize));

    // WGSL compiles ONCE per kernel per device, through a concurrent cache because one leased device serves
    // every binding of a press. The source and the entry point are native UTF-8, minted through
    // Marshal.StringToCoTaskMemUTF8 and retired in the compile fold's finally — the interop owner is one for
    // both directions across this boundary. SType.ShaderModuleWgslDescriptor is the chain tag; its same-valued
    // lower-cased twin is a closed-window spelling this page never takes. A refused module reports
    // through the error scope and the log sink alone — wgpuShaderModuleGetCompilationInfo is unimplemented in
    // wgpu-native and aborts the process with a non-unwinding panic, so no diagnostic path spells it. Auto
    // layout is deliberate: ComputePipelineGetBindGroupLayout(0) reads what the WGSL @group declarations
    // imply, so the roster is stated once, in the shader.
    Fin<nint> Pipeline(WgslKernel kernel, Op key) {
        if (pipelines.TryGetValue(kernel.Key, out nint cached)) { return Fin.Succ(cached); }
        nint source = Marshal.StringToCoTaskMemUTF8(kernel.Source);
        nint entry = Marshal.StringToCoTaskMemUTF8("main");
        try {
            ShaderModuleWGSLDescriptor wgsl = new() {
                Chain = new ChainedStruct { SType = SType.ShaderModuleWgslDescriptor },
                Code = (byte*)source,
            };
            ShaderModuleDescriptor moduleDescriptor = new() { NextInChain = (ChainedStruct*)&wgsl };
            ShaderModule* module = api.DeviceCreateShaderModule(device, &moduleDescriptor);
            if (module is null) { return Fin.Fail<nint>(RasterFault.Device(key, $"<wgsl-compile:{kernel.Key}:{Diagnostic()}>")); }
            ComputePipelineDescriptor descriptor = new() {
                Layout = null,
                Compute = new ProgrammableStageDescriptor { Module = module, EntryPoint = (byte*)entry },
            };
            ComputePipeline* pipeline = api.DeviceCreateComputePipeline(device, &descriptor);
            // The module is referenced by the pipeline it built, so it retires here rather than living as long
            // as the cache entry — one compiled kernel holds one handle, not two.
            api.ShaderModuleRelease(module);
            if (pipeline is null) { return Fin.Fail<nint>(RasterFault.Device(key, $"<compute-pipeline:{kernel.Key}:{Diagnostic()}>")); }
            // A concurrent loser releases ITS pipeline and takes the winner's: two compiles of one kernel are
            // legal under the cache's own race, two LIVE pipelines under one key are the leak.
            if (pipelines.TryAdd(kernel.Key, (nint)pipeline)) { return Fin.Succ((nint)pipeline); }
            api.ComputePipelineRelease(pipeline);
            return Fin.Succ(pipelines[kernel.Key]);
        } finally {
            Marshal.FreeCoTaskMem(source);
            Marshal.FreeCoTaskMem(entry);
        }
    }

    // Record and drain. QueueSubmitForIndex mints the index for THIS submission and DevicePoll(wait: true, &index)
    // blocks until it retires and its map callback has run, so BufferGetMapState is already Mapped when the fold
    // reads it — a wait:false spin is the frame-driven form a presented plane needs and a bake has no frame.
    // Position IS the @binding index, so the request's own buffer sequence seats the bind group with no layout
    // authored beside the shader's. Readback is buffer-to-buffer and therefore has NO row pitch: the 256-byte
    // BytesPerRow padding belongs to CommandEncoderCopyTextureToBuffer, and this device allocates no texture.
    Fin<KernelReceipt> Run(WgslKernel kernel, nint pipeline, KernelBinding binding, Op key) {
        // ONE fact, read at two scales: the query set holds a begin and an end stamp, and each stamp is a u64, so
        // the resolve buffer's size DERIVES from the count rather than restating it — a `Count = 2` beside a bare
        // `16` is two spellings that a third stamp would separate silently, resolving eight bytes short.
        const uint TimestampQueries = 2;
        const ulong TimestampBytes = TimestampQueries * sizeof(ulong);
        // Bounded BEFORE this frame ever runs: Dispatch's Guard proved the buffer count against the kernel
        // row's declared Layout (four slots at the widest row), so the stackalloc size is roster data, never a
        // caller-supplied length.
        int slots = binding.Buffers.Count;
        Span<nint> resources = stackalloc nint[slots];
        Span<BindGroupEntry> entries = stackalloc BindGroupEntry[slots];
        resources.Clear();
        nint staging = 0, querySet = 0, resolved = 0, timing = 0, group = 0, layout = 0, encoder = 0, pass = 0, commands = 0;
        int writeSlot = -1;
        ulong writeBytes = 0;
        NativeSlot scope = default, mapped = default;
        bool scoped = policy.Validation;
        if (scoped) { api.DevicePushErrorScope(device, ErrorFilter.Validation); }
        try {
            for (int slot = 0; slot < slots; slot++) {
                KernelBuffer request = binding.Buffers[slot];
                ulong bytes = (ulong)request.ByteLength;
                BufferDescriptor descriptor = new() { Usage = request.Kind.Usage, Size = bytes, MappedAtCreation = false };
                Buffer* handle = api.DeviceCreateBuffer(device, &descriptor);
                if (handle is null) { return Refuse(key, $"<buffer-alloc:{kernel.Key}:{slot}:{bytes}>"); }
                resources[slot] = (nint)handle;
                entries[slot] = new BindGroupEntry { Binding = (uint)slot, Buffer = handle, Offset = 0, Size = bytes };
                switch (request) {
                    case KernelBuffer.Uniform uniform: {
                        using MemoryHandle pin = uniform.Words.Pin();
                        api.QueueWriteBuffer(queue, handle, 0, pin.Pointer, (nuint)bytes);
                        break;
                    }
                    case KernelBuffer.Read read: {
                        using MemoryHandle pin = read.Elements.Pin();
                        api.QueueWriteBuffer(queue, handle, 0, pin.Pointer, (nuint)bytes);
                        break;
                    }
                    default: (writeSlot, writeBytes) = (slot, bytes); break;
                }
            }
            if (writeSlot < 0 || writeBytes is 0) { return Refuse(key, $"<kernel-no-write:{kernel.Key}>"); }

            BufferDescriptor stagingDescriptor = new() { Usage = BufferUsage.MapRead | BufferUsage.CopyDst, Size = writeBytes };
            Buffer* stagingBuffer = api.DeviceCreateBuffer(device, &stagingDescriptor);
            if (stagingBuffer is null) { return Refuse(key, $"<staging-alloc:{writeBytes}>"); }
            staging = (nint)stagingBuffer;

            BindGroupLayout* groupLayout = api.ComputePipelineGetBindGroupLayout((ComputePipeline*)pipeline, 0);
            layout = (nint)groupLayout;
            fixed (BindGroupEntry* seated = entries) {
                BindGroupDescriptor groupDescriptor = new() { Layout = groupLayout, EntryCount = (nuint)slots, Entries = seated };
                BindGroup* bound = api.DeviceCreateBindGroup(device, &groupDescriptor);
                if (bound is null) { return Refuse(key, $"<bind-group:{kernel.Key}>"); }
                group = (nint)bound;
            }

            if (timestamps) {
                QuerySetDescriptor queryDescriptor = new() { Type = QueryType.Timestamp, Count = TimestampQueries };
                querySet = (nint)api.DeviceCreateQuerySet(device, &queryDescriptor);
                BufferDescriptor resolveDescriptor = new() { Usage = BufferUsage.QueryResolve | BufferUsage.CopySrc, Size = TimestampBytes };
                resolved = (nint)api.DeviceCreateBuffer(device, &resolveDescriptor);
                BufferDescriptor timingDescriptor = new() { Usage = BufferUsage.MapRead | BufferUsage.CopyDst, Size = TimestampBytes };
                timing = (nint)api.DeviceCreateBuffer(device, &timingDescriptor);
            }

            CommandEncoderDescriptor encoderDescriptor = default;
            CommandEncoder* recorder = api.DeviceCreateCommandEncoder(device, &encoderDescriptor);
            encoder = (nint)recorder;
            if (timestamps) { api.CommandEncoderWriteTimestamp(recorder, (QuerySet*)querySet, 0); }
            ComputePassDescriptor passDescriptor = default;
            ComputePassEncoder* compute = api.CommandEncoderBeginComputePass(recorder, &passDescriptor);
            pass = (nint)compute;
            api.ComputePassEncoderSetPipeline(compute, (ComputePipeline*)pipeline);
            api.ComputePassEncoderSetBindGroup(compute, 0, (BindGroup*)group, 0, null);
            api.ComputePassEncoderDispatchWorkgroups(compute, binding.GroupsX, binding.GroupsY, binding.GroupsZ);
            api.ComputePassEncoderEnd(compute);
            if (timestamps) {
                api.CommandEncoderWriteTimestamp(recorder, (QuerySet*)querySet, 1);
                api.CommandEncoderResolveQuerySet(recorder, (QuerySet*)querySet, 0, 2, (Buffer*)resolved, 0);
                api.CommandEncoderCopyBufferToBuffer(recorder, (Buffer*)resolved, 0, (Buffer*)timing, 0, TimestampBytes);
            }
            api.CommandEncoderCopyBufferToBuffer(recorder, (Buffer*)resources[writeSlot], 0, stagingBuffer, 0, writeBytes);
            CommandBufferDescriptor finishDescriptor = default;
            CommandBuffer* recorded = api.CommandEncoderFinish(recorder, &finishDescriptor);
            commands = (nint)recorded;

            ulong index = vendor.QueueSubmitForIndex(queue, 1, &recorded);
            WrappedSubmissionIndex wrapped = new() { Queue = queue, SubmissionIndex = index };
            api.BufferMapAsync(stagingBuffer, MapMode.Read, 0, (nuint)writeBytes, new PfnBufferMapCallback(&OnMap), &mapped);
            vendor.DevicePoll(device, true, &wrapped);
            if (api.BufferGetMapState(stagingBuffer) != BufferMapState.Mapped) {
                return Refuse(key, $"<readback-unmapped:{kernel.Key}:{(BufferMapAsyncStatus)mapped.Status}:{Diagnostic()}>");
            }
            float[] raw = new float[writeBytes / sizeof(float)];
            new ReadOnlySpan<float>(api.BufferGetMappedRange(stagingBuffer, 0, (nuint)writeBytes), raw.Length).CopyTo(raw);
            api.BufferUnmap(stagingBuffer);

            // Resolved timestamp values are NANOSECONDS by the specification and this binding exposes no tick
            // period to convert against — Limits carries none — so the delta IS the receipt column. An adapter
            // that measured nothing leaves a typed ABSENCE, because a fabricated zero and an unmeasured pass are
            // the two states the parity benchmark exists to keep apart.
            Option<ulong> nanos = Option<ulong>.None;
            if (timestamps) {
                NativeSlot clocked = default;
                api.BufferMapAsync((Buffer*)timing, MapMode.Read, 0, (nuint)TimestampBytes, new PfnBufferMapCallback(&OnMap), &clocked);
                // A fresh wait-poll, NOT the retired submission index: the index above already drained, so an
                // index-scoped poll would return without running THIS map's callback and drop GpuNanos to
                // absence on a working adapter.
                vendor.DevicePoll(device, true, null);
                if (api.BufferGetMapState((Buffer*)timing) == BufferMapState.Mapped) {
                    ulong* ticks = (ulong*)api.BufferGetMappedRange((Buffer*)timing, 0, (nuint)TimestampBytes);
                    if (ticks[1] > ticks[0]) { nanos = ticks[1] - ticks[0]; }
                    api.BufferUnmap((Buffer*)timing);
                }
            }

            if (scoped) {
                scoped = false;
                api.DevicePopErrorScope(device, new PfnErrorCallback(&OnError), &scope);
                vendor.DevicePoll(device, true, null);
                if (scope.Fired is not 0 && scope.Status != (int)ErrorType.NoError) {
                    return Refuse(key, $"<validation:{(ErrorType)scope.Status}:{Detail(&scope)}>");
                }
            }
            // The row's OWN reduction folds the raw readback, so no consumer re-derives a kernel's tail.
            return Fin.Succ(new KernelReceipt(kernel, kernel.Reduce.Fold(raw, kernel.Reduce.Stride), nanos, Dispatches: 1u));
        } finally {
            // A refusal leaves the scope open, so popping it here keeps push and pop paired on every path; the
            // drain discards its message because the refusal already carries one and the log sink holds the rest.
            if (scoped) {
                api.DevicePopErrorScope(device, new PfnErrorCallback(&OnError), null);
                vendor.DevicePoll(device, true, null);
            }
            if (commands is not 0) { api.CommandBufferRelease((CommandBuffer*)commands); }
            if (pass is not 0) { api.ComputePassEncoderRelease((ComputePassEncoder*)pass); }
            if (encoder is not 0) { api.CommandEncoderRelease((CommandEncoder*)encoder); }
            if (group is not 0) { api.BindGroupRelease((BindGroup*)group); }
            if (layout is not 0) { api.BindGroupLayoutRelease((BindGroupLayout*)layout); }
            if (timing is not 0) { api.BufferRelease((Buffer*)timing); }
            if (resolved is not 0) { api.BufferRelease((Buffer*)resolved); }
            if (querySet is not 0) { api.QuerySetRelease((QuerySet*)querySet); }
            if (staging is not 0) { api.BufferRelease((Buffer*)staging); }
            for (int slot = 0; slot < slots; slot++) { if (resources[slot] is not 0) { api.BufferRelease((Buffer*)resources[slot]); } }
        }
    }

    // Native handles are pointer-wrapped structs released through their own XxxRelease/XxxDestroy, never
    // IDisposable — IDisposable exists here solely so the kernel Lease<T> rail can carry the device, and the
    // Owned case's `using` is the platform-forced disposal seam. Order is the reverse of acquisition: the
    // pipelines the cache owns, then queue, device, adapter, instance.
    public void Dispose() {
        foreach (KeyValuePair<string, nint> entry in pipelines) { api.ComputePipelineRelease((ComputePipeline*)entry.Value); }
        pipelines.Clear();
        api.QueueRelease(queue);
        api.DeviceRelease(device);
        api.AdapterRelease(adapter);
        api.InstanceRelease(instance);
    }

    static Fin<KernelReceipt> Refuse(Op key, string detail) => Fin.Fail<KernelReceipt>(RasterFault.Device(key, detail));

    // --- [CALLBACKS]
    // Every completion is [UnmanagedCallersOnly] over the caller's own stack slot rather than a
    // Pfn…Callback.From(delegate) thunk: SilkMarshal.DelegateToPtr mints one pinned marshalling thunk per
    // construction, and a device brought once per press with a map callback per binding would leak one thunk per
    // dispatch. A null userdata is the deliberate DISCARD form the error-scope drain takes on a refusal path.
    static void Capture(NativeSlot* slot, int status, nint handle, byte* message) {
        if (slot is null) { return; }
        slot->Fired = 1;
        slot->Status = status;
        slot->Handle = handle;
        int length = 0;
        if (message is not null) { while (length < NativeSlot.MessageCap - 1 && message[length] != 0) { length++; } }
        for (int at = 0; at < length; at++) { slot->Message[at] = message[at]; }
        slot->Length = length;
    }

    static string Detail(NativeSlot* slot) => slot->Length is 0 ? "<none>" : Encoding.UTF8.GetString(slot->Message, slot->Length);

    static string Diagnostic() { lock (DiagnosticGate) { fixed (NativeSlot* slot = &diagnostic) { return Detail(slot); } } }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnAdapter(RequestAdapterStatus status, Adapter* produced, byte* message, void* userdata) =>
        Capture((NativeSlot*)userdata, (int)status, (nint)produced, message);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnDevice(RequestDeviceStatus status, Device* produced, byte* message, void* userdata) =>
        Capture((NativeSlot*)userdata, (int)status, (nint)produced, message);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnMap(BufferMapAsyncStatus status, void* userdata) =>
        Capture((NativeSlot*)userdata, (int)status, handle: 0, message: null);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnError(ErrorType type, byte* message, void* userdata) =>
        Capture((NativeSlot*)userdata, (int)type, handle: 0, message);

    // LogLevel ascends Off, Error, Warn, Info, Debug, Trace, so the error-and-warning band is the half-open
    // range between Off and Info — the verbosity floor SetLogLevel already installs, restated here so a raised
    // floor cannot silently widen the slot into a per-frame overwrite.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnLog(LogLevel level, byte* message, void* userdata) {
        if (level is > LogLevel.Off and <= LogLevel.Warn) {
            lock (DiagnosticGate) { fixed (NativeSlot* slot = &diagnostic) { Capture(slot, (int)level, handle: 0, message); } }
        }
    }
}
```

## [03]-[WGSL_KERNEL]

- Owner: `WgslKernel` `[SmartEnum<string>]` the closed compute-module table; `BindingKind` `[SmartEnum<string>]` the per-slot binding vocabulary; `KernelReduce` `[SmartEnum<string>]` the host-side tail fold; `WgslOpCode` the appearance-vocabulary lowering table.
- Cases: kernel {`noiseField`, `checkerField`, `gradientField`, `mathFold`, `mixFold`, `equirectToCube`, `irradianceSh`, `prefilterSpecular`} · binding {`uniform`, `read`, `write`} · reduce {`none`, `partialSum`}.
- Law: the WGSL body IS the law, not a summary of one — each row carries its complete module source, so the shader a device compiles and the algorithm this corpus specifies are one text with no second spelling to drift.
- Law: every op code a shader switches on is the CPU vocabulary's own key. `NoiseBasis`, `FractalMode`, `CellularDistance`, and `CellularReturn` are `[SmartEnum<int>]`, so their keys ARE the codes and no third numbering exists; `MathOp` and `MixOp` are string-keyed, so `WgslOpCode` derives their codes from `Items` declaration order behind a `Lazy` accessor — the lowering table is this page's because graph.md owns the vocabulary and this page owns its GPU encoding.
- Law: WGSL `i32 >> u32` is ARITHMETIC — sign-propagating, agreeing with the C# `int` shift on every hash including the negative half — so the `h >> 15u`/`h << 19u`/masked-index spellings are FROZEN and a `bitcast<u32>` round trip is a defect on sight: it converts the arithmetic shift the FNL hash depends on into a logical one and silently re-seeds every gradient. Naga refuses a shift whose LEFT operand is an untyped literal in every position — a mask transcribed from a CPU defining sequence (`63 << 2`) spells the concrete suffix (`63i << 2u`) or the resolved literal — and a runtime-indexed table declares `var` or `var<private>`, because a `let`-bound array value and a module-scope `const` array alike admit only a constant index. The CPU/GPU parity this table claims against `texture#TEXTURE_UV` grades by TOLERANCE, never texel equality: Metal's `cos`/`sin` diverge from `MathF` at the last ULP, which alone breaks bit-identity on most gradient samples while the integer lattice agrees exactly.
- Law: the WGSL body is the LAW but never a second ALGORITHM — a kernel whose CPU twin exists transcribes that twin's own kernel members and its constants, so a divergence is a transcription defect a fixture must be able to name rather than a lane the two arms are entitled to. `prefilterSpecular` therefore spells `Microfacet.SampleVisibleNormal` and `Microfacet.AlphaOf`'s own `1e-4` floor, `Deterministic.Hammersley`'s half-texel-offset pair, and the sweep's own tangent-to-world completion — a D-proportional half-vector draw is the deleted form even where the two distributions agree in the limit, because the same Hammersley pair drives the polar and azimuth angles in OPPOSITE roles and the two tap sets separate at every finite budget, which is exactly what `[04]`'s `SplitDomePrefilter` reads. `Microfacet.VisibleNormalPdf` is the one CPU member the transcription does NOT reach, and the honest statement is a CAPABILITY one rather than an algorithmic one: the pdf's only consumer is the filtered-importance mip term, the row binds ONE FLAT SOURCE PLANE and not a pyramid, so there is no level to select and the term has nothing to compute against. That is a missing binding, not a cheaper derivation. Its consequence — a bright source texel drawn at low probability arriving unfiltered, where the CPU arm's mip selection would have widened its footprint — is a REAL divergence, and it is measured rather than asserted: `[04]`'s `SplitDomePrefilter` fixture reads a drawn direction over a split dome, which is exactly the arrangement that separates a filtered draw from an unfiltered one, and the parity workload publishes the gap as telemetry on the same column every other backend divergence rides. A mip-bound source binding is the growth leg that closes it, and closing it is a binding change rather than a shader rewrite.
- Entry: the table is the entry — `WgslKernel.Items` is the roster `[04]-[GOLDEN_VECTOR]` iterates and `PressDevice.Dispatch` selects on; `Layout` declares the binding kinds a request answers, `Reduce` declares the host fold the dispatch applies to the raw readback, and `Groups(width, height, layers)` derives the full three-dimensional workgroup count from the row's own workgroup shape so no caller computes a dispatch dimension and a six-face cube dispatches in one call.
- Packages: Silk.NET.WebGPU (the `ShaderModuleWGSLDescriptor` chain each row's `Source` fills, `BufferUsage` each `BindingKind` names), `graph#MATERIAL_GRAPH` (composed — `MathOp`/`MixOp`, the rosters `mathFold` and `mixFold` lower), `texture#TEXTURE_UV` (composed — `NoiseBasis`/`FractalMode`/`CellularDistance`/`CellularReturn`, the rosters `noiseField` lowers), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new GPU kernel is one row carrying source, layout, workgroup shape, host reduction, and golden vector — never a second module table and never an ungated arm. `noiseField` covers the FULL field algebra at BOTH dimensions — every `NoiseBasis` in its 2D and SOLID 3D arm, every `FractalMode`, the whole `CellularDistance` × `CellularReturn` product, the domain warp, and the period wrap — so a real authored noise source, planar or solid, previews as itself; `mathFold` and `mixFold` cover the FULL pointwise `MathOp` and `MixOp` rosters. A new vocabulary row lands one arm in the corresponding WGSL switch beside its CPU delegate.
- Boundary: every field kernel runs at `@workgroup_size(8, 8, 1)` over a texel grid and every reduction at `@workgroup_size(64, 1, 1)` over a linear range, so the dispatch shape derives from the row and the caller supplies extent alone; `Groups` carries the LAYER axis because a cube map is six faces of one dispatch and a hardcoded `Z = 1` makes `equirectToCube`'s own `gid.z` face selector unreachable; every FIELD kernel indexes `gid.z` into its flat write, so a layered dispatch writes every slice (a 2D field replicates across layers, the CPU lane's own layer-invariant sampling) instead of racing all Z-slices onto slice zero. Bind group zero is the only group and the WGSL `@group(0) @binding(n)` declarations ARE the layout the auto-derived `ComputePipelineGetBindGroupLayout(0)` reads — a hand-authored `BindGroupLayout` beside them is a second statement of one fact. Storage buffers carry `f32` element arrays with four elements per RGBA texel, because a `vec4<f32>` storage element imposes a sixteen-byte stride the host un-packs anyway. NO ROW NEEDS A STORAGE TEXTURE and the absence is DECLARED rather than pending: every kernel on the table is a texel-indexed gather or scatter over flat `f32` buffers, none performs hardware filtering, writes a compressed target, or produces a presentable surface, and those are the three capabilities a storage texture buys. The `COPY_BYTES_PER_ROW_ALIGNMENT` 256-byte padding follows a texture-to-buffer copy the device therefore never performs, so it is a cost this lane does not pay; a FIRST storage-texture row is the growth leg, and landing one owns the host un-pad step as part of landing it rather than as a discovered surprise. `noiseField` reproduces the `texture#TEXTURE_UV` FastNoiseLite lattice at `f32` — the same `PrimeX`/`PrimeY` lattice primes, the same `0x27d4eb2d` hash multiplier, the same quintic fade and Hermite smoothstep, the same 2D simplex skew and unskew with the `99.83685446303647` bound, the same `1.4247691104677813` Perlin normalizer, the same cellular jitter radius `0.43701595` over the SAME golden-angle unit-vector table at the FNL ROUNDED-cell anchor (never a floor-plus-half lattice or a scalar-hash displacement), and the same fractal-bounding amplitude cascade — with the twenty-four-direction gradient table GENERATED from its `82.5° − 15°·k` defining sequence rather than transcribed, so the GPU and CPU lattices agree in structure and diverge only in float width; the periodic arm wraps the INTEGER lattice coordinate modulo the row's own period, so a period-wrapped sample is exact rather than approximately periodic. The SOLID 3D arm rides the SAME row: the `dimension` uniform selects the 3D lattice — `PrimeZ`, the `0.964921414852142333984375` Perlin3D normalizer, the OpenSimplex2 rotated two-cell fold at the `32.69428253173828125` bound, the 3×3×3 cellular neighbourhood at the `0.39614353` jitter radius — with the gradient quads GENERATED as `var<private>` from the 12-edge `(±1,±1,0)` family plus the published 4-entry tail (a `const` table refuses the runtime quad index) and the cellular offsets from the spherical-Fibonacci closed form, which needs no table at all; the four 3D arms fit the same `Params`/`dst` binding shape, so solid is a COLUMN, never a second kernel, the solid depth is the layer-centre coordinate (a volume dispatch is a real 3D field where the planar arm replicates across layers), and the measured parity floor against the CPU arms is `3.1e-4` worst-arm absolute — f32 rounding riding the simplex fold's corner thresholds, zero sign flips, zero non-finite lanes, `1e-3` clearing every arm — so `press#PRESS_PLAN` ADMITS a solid source and a triplanar-of-solid-noise on the accelerator while texel equality stays unasserted. `mathFold` matches the CPU `MathOp` shape for shape — the ZIP family (add/subtract/multiply/divide/modulo/min/max) folds PER LANE exactly as the shape-preserving `Zip` does, and the `AsScalar` rows carry the operand POLARITY as a uniform because the CPU projection reads a colour's AP1 luminance and a scalar port's own value, which a plane's four lanes cannot tell apart — a kernel assuming `.x` silently reduces every colour operand to its red channel. `equirectToCube` is the ONE kernel whose product is a cube, so the `faceDir` source fragment concatenates onto its row alone at construction, WGSL having no include; the mapping itself is FROZEN — `u = 0.5 + atan2(d.y, d.x) / 2π`, `v = acos(clamp(d.z, −1, 1)) / π`, `v = 0` at `+Z`, `u` increasing counter-clockwise viewed from `+Z` — with the up axis `+Z`, no field and no knob, matching the OpenPBR local frame the whole appearance plane shares. `irradianceSh` writes PER-WORKGROUP PARTIALS — twenty-seven `f32` per workgroup at `workgroup_index * 27` — and its `KernelReduce.PartialSum` row folds them host-side in workgroup-index order: WGSL has no `f32` atomic, a workgroup-order-dependent atomic sum makes the projection non-deterministic across dispatches, and NO subgroup path exists ON THE PINNED BINDING and the reason is two facts, not one: its naga refuses the `enable subgroups;` directive outright — trunk recognizes the directive for diagnostics and still leaves the functionality unimplemented — and the pinned native carries ZERO subgroup surface beside it, no WGSL builtins and no feature bit, its whole enable-extension vocabulary being `f16` and `dual_source_blending`. The refinement that matters for a future bump: on a current `wgpu-native` the subgroup ops are reachable through the NATIVE FEATURE BIT without the directive at all, so this arms through the FEATURE ROSTER rather than through the directive parse, and the gate to watch is the adapter's granted feature row. Until then the 1728-element workgroup tile at 6912 of the 16384-byte budget IS the reduction law, and it replays bitwise-identical partials across independent dispatches; the reduction is a ROW column rather than a caller step, so no consumer re-derives a kernel's tail. `prefilterSpecular` draws the visible-normal distribution over the half-texel-offset Hammersley pair, rotates each tangent-space draw onto the texel's world normal, weights by `N·L`, and discards the below-horizon half, so the prefiltered value of a constant environment is that constant at every roughness — the property `[04]`'s first fixture gates on and the property NO sampler fails, which is why the second fixture reads a drawn direction instead. Its product lands EQUIRECT through the frozen correspondence inverted in four lines, because `environment#IBL_PREFILTER` declares one storage arrangement for every level and an accelerator that changes the product's layout is a second product rather than a faster one; the row therefore takes the plane extent where the cube kernel takes an edge, and the `gid.z` axis stays the layer index the flat write already carries. [SPIKE]: the `irradianceSh` subgroup-reduction arm converges on the granted feature roster a live adapter reports off a bumped native alone; the deterministic floor is the host-side fold of the twenty-seven per-workgroup partials in workgroup-index order over the 1728-element tile, standing whole without it.
- Boundary: a PROVEN kernel and a REACHABLE one are two facts, so the table states both. `noiseField`, `checkerField`, and `gradientField` carry a dispatching consumer — `press#PRESS_PLAN` lowers a `Source` subject onto them and gates that lowerability at plan admission. `prefilterSpecular`, `irradianceSh`, and `equirectToCube` carry one at `environment#IBL_PREFILTER`, which reads the `IblPolicy.Backend` row and the `Option<PressDevice>` the accelerator arm takes. `mathFold` and `mixFold` carry their consumer at `[05]-[KERNEL_CHAIN]`: `press#PRESS_PLAN` lowers a `Graph` subject into a `ChainNode` sequence and the plan admission gates it on the allocator's own footprint, so both rows are dispatched rather than fixture-only. Fixtures prove a kernel computes its own law and nothing more; only a named consumer proves anything runs it, and a reader conflating the two mistakes a proven kernel for a reachable one.

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
// the fold a column keeps the tail with the kernel that produced it rather than in every consumer. ReduceSpan
// and ReductionGroups are the ONE expression every reduction-dispatch fact derives from — the kernel row's
// Groups, the write-buffer sizing, and the fixture's own `groups` uniform word all call it, so the number the
// grid-stride loop reads and the number the binding dispatches cannot disagree.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelReduce {
    public static readonly KernelReduce None       = new("none",       stride: 0, fold: static (raw, _) => raw);
    public static readonly KernelReduce PartialSum = new("partialSum", stride: 27, fold: Sum);

    // 64 lanes x 64 texels per lane — the reduction workgroup's own span, derived nowhere else.
    public const long ReduceSpan = 64L * 64L;
    public static uint ReductionGroups(long texels) => (uint)((texels + ReduceSpan - 1) / ReduceSpan);

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
// [SmartEnum<int>], so their KEYS are the codes and this table never re-numbers them. MathOp and MixOp codes are
// EXPLICIT rows pinned to the WGSL switch arms below — the shader text hand-numbers its cases (WGSL admits no
// generated dispatch), so an Items-order derivation beside it would be a SECOND numbering that drifts silently
// the day graph.md reorders a declaration; one visible correspondence, whose COVERAGE is proved at type
// initialization and stored. The stored proof names every roster row the table missed, where the per-call
// recount it replaces charged two Count comparisons and a Lazy dereference to every Dispatch and every
// Golden.Prove and reported the drift as a pair of arithmetic counts. The verdict still LEAVES through a Fin,
// because a type-initializer throw surfaces as an unrecoverable TypeInitializationException at an arbitrary
// first-use site, off every rail this corpus routes on.
public static class WgslOpCode {
    static readonly FrozenDictionary<MathOp, uint> MathCodes = new (MathOp Row, uint Code)[] {
        (MathOp.Add, 0u), (MathOp.Subtract, 1u), (MathOp.Multiply, 2u), (MathOp.Divide, 3u), (MathOp.Modulo, 4u),
        (MathOp.Scale, 5u), (MathOp.Power, 6u), (MathOp.Sqrt, 7u), (MathOp.Abs, 8u), (MathOp.Sin, 9u),
        (MathOp.Cos, 10u), (MathOp.Min, 11u), (MathOp.Max, 12u), (MathOp.DotProduct, 13u), (MathOp.CrossProduct, 14u),
        (MathOp.Normalize, 15u), (MathOp.Clamp01, 16u), (MathOp.OneMinus, 17u), (MathOp.Fresnel, 18u),
        (MathOp.Atan2, 19u), (MathOp.Sign, 20u), (MathOp.Floor, 21u), (MathOp.Ceil, 22u), (MathOp.Round, 23u),
        (MathOp.Exp, 24u), (MathOp.Ln, 25u), (MathOp.Magnitude, 26u), (MathOp.Distance, 27u),
    }.ToFrozenDictionary(static e => e.Row, static e => e.Code);

    // The rows the BINARY fold structurally cannot carry: each reads three or more operands, and mathFold binds
    // exactly two read planes by its own Layout. They are a DECLARED carve the plan-admission gate refuses on,
    // never a silent gap — so the coverage proof names a genuine roster append and never re-reports the arity
    // this kernel refuses by construction.
    static readonly FrozenSet<MathOp> Unlowerable = FrozenSet.ToFrozenSet([
        MathOp.Smoothstep, MathOp.Contrast, MathOp.Remap, MathOp.Range, MathOp.IfGreater, MathOp.IfEqual, MathOp.Pick]);

    static readonly FrozenDictionary<MixOp, uint> MixCodes = new (MixOp Row, uint Code)[] {
        (MixOp.Lerp, 0u), (MixOp.Multiply, 1u), (MixOp.Screen, 2u), (MixOp.Overlay, 3u), (MixOp.Darken, 4u),
        (MixOp.Lighten, 5u), (MixOp.Dodge, 6u), (MixOp.Burn, 7u), (MixOp.HardLight, 8u), (MixOp.SoftLight, 9u),
        (MixOp.Difference, 10u), (MixOp.Exclusion, 11u), (MixOp.Hue, 12u), (MixOp.Saturation, 13u),
        (MixOp.Colour, 14u), (MixOp.Luminosity, 15u),
    }.ToFrozenDictionary(static e => e.Row, static e => e.Code);

    // The TYPE-INIT proof: every roster row this table left uncoded, named once. Field initializers run in
    // declaration order, so both tables are seated before the coverage read; an EMPTY sequence IS the totality,
    // and a populated one carries the exact keys a rebuild has to add rather than the count it is short by.
    static readonly Seq<string> Uncovered =
        toSeq(MathOp.Items).Filter(static row => !MathCodes.ContainsKey(row) && !Unlowerable.Contains(row))
                           .Map(static row => $"math:{row.Key}") +
        toSeq(MixOp.Items).Filter(static row => !MixCodes.ContainsKey(row)).Map(static row => $"mix:{row.Key}");

    // The stored verdict on the ONE rail: a roster append that misses a row rails the first dispatch by name
    // instead of tearing type initialization down at whatever surface touched the class first. The verdict is a
    // TYPE-INIT PROOF read as a frozen value — `Uncovered` cannot change after the rosters freeze, so re-walking
    // two rosters and rebuilding a joined string on every dispatch re-derives a constant; the fault carries a
    // pre-rendered detail so even the refusing arm allocates nothing per call.
    static readonly Fin<Unit> Verdict = Uncovered.IsEmpty
        ? Fin.Succ(unit)
        : Fin.Fail<Unit>(RasterFault.Device(Op.Of(name: nameof(WgslOpCode)), $"<wgsl-op-roster-drift:{string.Join(',', Uncovered)}>"));

    public static Fin<Unit> Total(Op key) => Verdict;

    public static uint Of(MathOp op) => MathCodes[op];
    public static uint Of(MixOp op) => MixCodes[op];
    public static uint Of(NoiseBasis basis) => unchecked((uint)basis.Key);
    public static uint Of(FractalMode fractal) => unchecked((uint)fractal.Key);
    public static uint Of(CellularDistance metric) => unchecked((uint)metric.Key);
    public static uint Of(CellularReturn projection) => unchecked((uint)projection.Key);
}

// The closed module table. Source is the WHOLE shader, so the text a device compiles and the algorithm this
// corpus specifies are one artefact. Groups derives the full three-dimensional dispatch from the row's own
// workgroup shape and the plan's own extent, layers included; WriteElements derives the write-buffer sizing
// from the SAME expressions, so a reduction row's buffer holds groups x stride floats and never a texel-count
// formula three orders too large. Golden is a SEQUENCE — a kernel whose law spans several conventions proves
// each under its own dispatch, and a kernel without at least one cannot be declared.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WgslKernel {
    public static readonly WgslKernel NoiseField        = new("noiseField",        source: Wgsl.NoiseField,                      layout: Field,   x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.NoiseLatticeZero, Raster.Golden.NoiseSolidLatticeZero));
    public static readonly WgslKernel CheckerField      = new("checkerField",      source: Wgsl.CheckerField,                    layout: Field,   x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.CheckerParity));
    public static readonly WgslKernel GradientField     = new("gradientField",     source: Wgsl.GradientField,                   layout: Sampled, x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.GradientMidpoint));
    public static readonly WgslKernel MathFold          = new("mathFold",          source: Wgsl.MathFold,                        layout: Binary,  x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.MathFloorMod, Raster.Golden.MathSafeDiv, Raster.Golden.MathSqrtClamp));
    public static readonly WgslKernel MixFold           = new("mixFold",           source: Wgsl.MixFold,                         layout: Binary,  x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.MixMultiply));
    public static readonly WgslKernel EquirectToCube    = new("equirectToCube",    source: Wgsl.FaceDir + Wgsl.EquirectToCube,   layout: Sampled, x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.CubeFaceCentre));
    public static readonly WgslKernel IrradianceSh      = new("irradianceSh",      source: Wgsl.IrradianceSh,                    layout: Sampled, x: 64u, y: 1u, reduce: KernelReduce.PartialSum, golden: Seq(Raster.Golden.ConstantIrradiance));
    public static readonly WgslKernel PrefilterSpecular = new("prefilterSpecular", source: Wgsl.PrefilterSpecular, layout: Sampled, x: 8u,  y: 8u, reduce: KernelReduce.None,       golden: Seq(Raster.Golden.ConstantPrefilter, Raster.Golden.SplitDomePrefilter));

    public string Source { get; }
    public Seq<BindingKind> Layout { get; }
    public uint WorkgroupX { get; }
    public uint WorkgroupY { get; }
    public KernelReduce Reduce { get; }
    public Seq<GoldenVector> Golden { get; }

    // Layers ride the Z axis: a six-face cube is one dispatch, the field kernels index gid.z so every layer
    // slice is written (a planar field replicates across layers, exactly the CPU lane's layer-invariant
    // sampling; the solid noise arm samples each slice's layer-centre depth), and a hardcoded Z of one would
    // make the cube kernel's own gid.z face selector unreachable. A REDUCTION
    // row dispatches LINEAR — its grid-stride loop reads only gid.x and wid.x, so a texel-tiled (X, Y)
    // dispatch hands every Y-row workgroup wid.x = 0 and races them all onto partials[0]; the group count is
    // KernelReduce.ReductionGroups, the ONE expression the dispatch, the write sizing, and the fixture's own
    // `groups` uniform word all read.
    public (uint X, uint Y, uint Z) Groups(Dimension width, Dimension height, Dimension layers) =>
        Reduce.Stride > 0
            ? (KernelReduce.ReductionGroups((long)width.Value * height.Value * layers.Value), 1u, 1u)
            : (((uint)width.Value + WorkgroupX - 1) / WorkgroupX, ((uint)height.Value + WorkgroupY - 1) / WorkgroupY, (uint)layers.Value);

    // The write buffer's element count, derived beside Groups so the two facts share one origin: a field row
    // writes four floats per texel, a reduction row stride floats per workgroup.
    // The operand arity a chain step must satisfy, read off the row's OWN layout rather than declared beside it:
    // the layout IS the binding contract, so a step's read count and its shader's `@binding` declarations cannot
    // disagree. `Field` reads nothing, `Sampled` one, `Binary` two.
    public int Reads => Layout.Filter(static kind => kind == BindingKind.Read).Count;

    public int WriteElements(Dimension width, Dimension height, Dimension layers) =>
        Reduce.Stride > 0
            ? checked((int)(KernelReduce.ReductionGroups((long)width.Value * height.Value * layers.Value) * (uint)Reduce.Stride))
            : checked(width.Value * height.Value * layers.Value * 4);

    static readonly Seq<BindingKind> Field   = Seq(BindingKind.Uniform, BindingKind.Write);
    static readonly Seq<BindingKind> Sampled = Seq(BindingKind.Uniform, BindingKind.Read, BindingKind.Write);
    static readonly Seq<BindingKind> Binary  = Seq(BindingKind.Uniform, BindingKind.Read, BindingKind.Read, BindingKind.Write);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The WGSL bodies. Each module declares its own bind group zero, so the auto-derived layout reads exactly
// these declarations and no host-side layout restates them.
internal static class Wgsl {
    // The cube-face mapping, held by the ONE kernel whose product is a cube. WGSL has no include, so the
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

    // The FULL FastNoiseLite algebra at f32, 2D and SOLID 3D: four bases per dimension, three fractal
    // trajectories, the whole cellular distance x return product, the domain warp, and the period wrap. The
    // 24-direction 2D gradient table GENERATES from its 82.5 - 15*k defining sequence, the 3D quads from the
    // 12-edge family plus the published tail as var<private> (a const table refuses the runtime quad index),
    // the 3D cellular offsets from the spherical-Fibonacci closed form with no table at all, and the periodic
    // arm wraps the INTEGER lattice coordinate so a period-wrapped sample is exact, not approximately seamless.
    // The dimension uniform selects the lattice: planar replicates across layers, solid samples the layer-
    // centre depth, so a solid or triplanar-of-solid-noise source previews as itself. i32 shifts are
    // ARITHMETIC, matching the C# hash exactly; a bitcast<u32> round trip is a defect that re-seeds every
    // gradient.
    internal const string NoiseField = """
        struct Params {
            extent: vec2<u32>, frequency: f32, lacunarity: f32,
            gain: f32, weighted: f32, pingPong: f32, jitter: f32,
            period: f32, warpAmp: f32, warpFreq: f32,
            octaves: u32, seed: i32, basis: u32, fractal: u32,
            metric: u32, feature: u32, warpSeed: i32, dimension: u32, layers: u32,
            lo: vec4<f32>, hi: vec4<f32>
        };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read_write> dst : array<f32>;

        const PX : i32 = 501125321;
        const PY : i32 = 1136930381;
        const PZ : i32 = 1720413743;
        const SKEW : f32 = 0.3660254037844386;      // (sqrt(3) - 1) / 2
        const UNSKEW : f32 = 0.21132486540518713;   // (3 - sqrt(3)) / 6

        // var<private>, never const: the runtime quad index refuses against a const table in module scope
        // exactly as against a let-bound local. The 64-quad FNL cycle is EDGE3[q % 12] for q < 60, TAIL3
        // above — generated, never 256 transcribed lanes.
        var<private> EDGE3 : array<vec3<f32>, 12> = array<vec3<f32>, 12>(
            vec3<f32>(0.0, 1.0, 1.0), vec3<f32>(0.0, -1.0, 1.0), vec3<f32>(0.0, 1.0, -1.0), vec3<f32>(0.0, -1.0, -1.0),
            vec3<f32>(1.0, 0.0, 1.0), vec3<f32>(-1.0, 0.0, 1.0), vec3<f32>(1.0, 0.0, -1.0), vec3<f32>(-1.0, 0.0, -1.0),
            vec3<f32>(1.0, 1.0, 0.0), vec3<f32>(-1.0, 1.0, 0.0), vec3<f32>(1.0, -1.0, 0.0), vec3<f32>(-1.0, -1.0, 0.0));
        var<private> TAIL3 : array<vec3<f32>, 4> = array<vec3<f32>, 4>(
            vec3<f32>(1.0, 1.0, 0.0), vec3<f32>(0.0, -1.0, 1.0), vec3<f32>(-1.0, 1.0, 0.0), vec3<f32>(0.0, -1.0, -1.0));

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
            h = h * h;
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

        fn simplex(x: f32, y: f32, seed: i32) -> f32 {
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
            if (t0 > 0.0) { t0 = t0 * t0; total = total + t0 * t0 * grad(seed, i * PX, j * PY, x0, y0); }
            var t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 > 0.0) { t1 = t1 * t1; total = total + t1 * t1 * grad(seed, (i + i1) * PX, (j + j1) * PY, x1, y1); }
            var t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 > 0.0) { t2 = t2 * t2; total = total + t2 * t2 * grad(seed, (i + 1) * PX, (j + 1) * PY, x2, y2); }
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
        // The CPU Worley2D lattice verbatim at f32: the neighbourhood anchors at the FNL ROUNDED cell
        // (half-away-from-zero, never floor), each feature displaces by a hash-indexed golden-angle UNIT
        // vector at the 0.43701595 jitter radius, the hash reads the WRAPPED neighbour while the feature sits
        // at its UNWRAPPED coordinate, the cell value is the RAW hash scaled — and the projection clamps to
        // [-1, 1] exactly as the CPU fold does.
        fn fastRound(v: f32) -> i32 { if (v >= 0.0) { return i32(v + 0.5); } return i32(v - 0.5); }
        fn randVec(h: i32) -> vec2<f32> {
            let a = f32((h >> 1u) & 255) * 2.399963229728653;
            return vec2<f32>(cos(a), sin(a));
        }
        // ONE feature projection serves both cellular dimensions: a new CellularReturn row lands one arm here,
        // never a 2D and a 3D twin.
        fn featureOf(f1: f32, f2: f32, cell: f32) -> f32 {
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
        fn worley(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            let xr = fastRound(x); let yr = fastRound(y);
            let radius = 0.43701595 * p.jitter;
            var f1 = 1e30; var f2 = 1e30; var cell = 0.0;
            for (var oy = -1; oy <= 1; oy = oy + 1) {
                for (var ox = -1; ox <= 1; ox = ox + 1) {
                    let cx = xr + ox; let cy = yr + oy;
                    let h = hash(seed, wrap(cx, period) * PX, wrap(cy, period) * PY);
                    let rv = randVec(h) * radius;
                    let d = metric(f32(cx) + rv.x - x, f32(cy) + rv.y - y);
                    if (d < f1) { f2 = f1; f1 = d; cell = f32(h) * 4.6566128730773926e-10; }
                    else if (d < f2) { f2 = d; }
                }
            }
            return clamp(featureOf(f1, f2, cell), -1.0, 1.0);
        }

        // The SOLID 3D family: the same hash chain over the third prime, the generated quad table, and the
        // FNL 3D anchors — the 0.964921414852142333984375 Perlin normalizer, the OpenSimplex2 rotated two-cell
        // fold at the 32.69428253173828125 bound, the 0.39614353 cellular jitter radius. The cell-advance
        // (xSign >> 1u) & PX and the h ^ (h >> 15u) chain both REQUIRE the arithmetic i32 shift: xSign is ±1,
        // so -1 >> 1 must stay -1 to select the prime.
        fn hash3(seed: i32, xp: i32, yp: i32, zp: i32) -> i32 { return (seed ^ xp ^ yp ^ zp) * 668265261; }
        fn grad3(seed: i32, xp: i32, yp: i32, zp: i32, xd: f32, yd: f32, zd: f32) -> f32 {
            var h = hash3(seed, xp, yp, zp);
            h = h ^ (h >> 15u);
            let q = u32(h & 252) >> 2u;
            var g : vec3<f32>;
            if (q < 60u) { g = EDGE3[q % 12u]; } else { g = TAIL3[q - 60u]; }
            return xd * g.x + yd * g.y + zd * g.z;
        }
        fn valCoord3(seed: i32, xp: i32, yp: i32, zp: i32) -> f32 {
            var h = hash3(seed, xp, yp, zp);
            h = h * h;
            h = h ^ (h << 19u);
            return f32(h) * 4.6566128730773926e-10;
        }

        fn perlin3(x: f32, y: f32, z: f32, seed: i32, period: i32) -> f32 {
            let ix = i32(floor(x)); let iy = i32(floor(y)); let iz = i32(floor(z));
            let xd0 = x - f32(ix); let yd0 = y - f32(iy); let zd0 = z - f32(iz);
            let xs = fade(xd0); let ys = fade(yd0); let zs = fade(zd0);
            let x0 = wrap(ix, period) * PX; let y0 = wrap(iy, period) * PY; let z0 = wrap(iz, period) * PZ;
            let x1 = wrap(ix + 1, period) * PX; let y1 = wrap(iy + 1, period) * PY; let z1 = wrap(iz + 1, period) * PZ;
            let n000 = grad3(seed, x0, y0, z0, xd0, yd0, zd0);
            let n100 = grad3(seed, x1, y0, z0, xd0 - 1.0, yd0, zd0);
            let n010 = grad3(seed, x0, y1, z0, xd0, yd0 - 1.0, zd0);
            let n110 = grad3(seed, x1, y1, z0, xd0 - 1.0, yd0 - 1.0, zd0);
            let n001 = grad3(seed, x0, y0, z1, xd0, yd0, zd0 - 1.0);
            let n101 = grad3(seed, x1, y0, z1, xd0 - 1.0, yd0, zd0 - 1.0);
            let n011 = grad3(seed, x0, y1, z1, xd0, yd0 - 1.0, zd0 - 1.0);
            let n111 = grad3(seed, x1, y1, z1, xd0 - 1.0, yd0 - 1.0, zd0 - 1.0);
            let xf0 = mix(mix(n000, n100, xs), mix(n010, n110, xs), ys);
            let xf1 = mix(mix(n001, n101, xs), mix(n011, n111, xs), ys);
            return mix(xf0, xf1, zs) * 0.9649214148521423;
        }

        fn value3(x: f32, y: f32, z: f32, seed: i32, period: i32) -> f32 {
            let ix = i32(floor(x)); let iy = i32(floor(y)); let iz = i32(floor(z));
            let xs = hermite(x - f32(ix)); let ys = hermite(y - f32(iy)); let zs = hermite(z - f32(iz));
            let x0 = wrap(ix, period) * PX; let y0 = wrap(iy, period) * PY; let z0 = wrap(iz, period) * PZ;
            let x1 = wrap(ix + 1, period) * PX; let y1 = wrap(iy + 1, period) * PY; let z1 = wrap(iz + 1, period) * PZ;
            let f0 = mix(mix(valCoord3(seed, x0, y0, z0), valCoord3(seed, x1, y0, z0), xs),
                         mix(valCoord3(seed, x0, y1, z0), valCoord3(seed, x1, y1, z0), xs), ys);
            let f1 = mix(mix(valCoord3(seed, x0, y0, z1), valCoord3(seed, x1, y0, z1), xs),
                         mix(valCoord3(seed, x0, y1, z1), valCoord3(seed, x1, y1, z1), xs), ys);
            return mix(f0, f1, zs);
        }

        // OpenSimplex2's rotated two-cell fold, seed-only exactly as the CPU arm: Noise.Of refuses a periodic
        // simplex outright, so no period column reaches this body.
        fn simplex3(x: f32, y: f32, z: f32, seed: i32) -> f32 {
            let r = (x + y + z) * 0.6666666666666666;
            let xr = r - x; let yr = r - y; let zr = r - z;
            let i = fastRound(xr); let j = fastRound(yr); let k = fastRound(zr);
            var x0 = xr - f32(i); var y0 = yr - f32(j); var z0 = zr - f32(k);
            var xSign = i32(-1.0 - x0) | 1; var ySign = i32(-1.0 - y0) | 1; var zSign = i32(-1.0 - z0) | 1;
            var ax0 = f32(xSign) * -x0; var ay0 = f32(ySign) * -y0; var az0 = f32(zSign) * -z0;
            var ip = i * PX; var jp = j * PY; var kp = k * PZ;
            var s = seed;
            var value = 0.0;
            var a = (0.6 - x0 * x0) - (y0 * y0 + z0 * z0);
            for (var l = 0; l < 2; l = l + 1) {
                if (a > 0.0) { let a2 = a * a; value = value + a2 * a2 * grad3(s, ip, jp, kp, x0, y0, z0); }
                if (ax0 >= ay0 && ax0 >= az0) {
                    var b = a + ax0 + ax0;
                    if (b > 1.0) { b = b - 1.0; let b2 = b * b; value = value + b2 * b2 * grad3(s, ip - xSign * PX, jp, kp, x0 + f32(xSign), y0, z0); }
                } else if (ay0 > ax0 && ay0 >= az0) {
                    var b = a + ay0 + ay0;
                    if (b > 1.0) { b = b - 1.0; let b2 = b * b; value = value + b2 * b2 * grad3(s, ip, jp - ySign * PY, kp, x0, y0 + f32(ySign), z0); }
                } else {
                    var b = a + az0 + az0;
                    if (b > 1.0) { b = b - 1.0; let b2 = b * b; value = value + b2 * b2 * grad3(s, ip, jp, kp - zSign * PZ, x0, y0, z0 + f32(zSign)); }
                }
                if (l == 1) { break; }
                ax0 = 0.5 - ax0; ay0 = 0.5 - ay0; az0 = 0.5 - az0;
                x0 = f32(xSign) * ax0; y0 = f32(ySign) * ay0; z0 = f32(zSign) * az0;
                a = a + (0.75 - ax0 - ay0 - az0);
                ip = ip + ((xSign >> 1u) & PX); jp = jp + ((ySign >> 1u) & PY); kp = kp + ((zSign >> 1u) & PZ);
                xSign = -xSign; ySign = -ySign; zSign = -zSign;
                s = ~s;
            }
            return value * 32.694282531738281;
        }

        // The spherical-Fibonacci unit offset in closed form: quad index h>>2 & 255 maps to the same lane the
        // CPU RandVecs3D builder fills, so the table never exists on either side.
        fn randVec3(h: i32) -> vec3<f32> {
            let i = f32((h >> 2u) & 255);
            let z = 1.0 - (2.0 * i + 1.0) / 256.0;
            let r = sqrt(1.0 - z * z);
            let a = i * 2.399963229728653;
            return vec3<f32>(r * cos(a), r * sin(a), z);
        }
        fn metric3(dx: f32, dy: f32, dz: f32) -> f32 {
            switch (p.metric) {
                case 1u: { return sqrt(dx * dx + dy * dy + dz * dz); }
                case 2u: { return abs(dx) + abs(dy) + abs(dz); }
                case 3u: { return abs(dx) + abs(dy) + abs(dz) + dx * dx + dy * dy + dz * dz; }
                default: { return dx * dx + dy * dy + dz * dz; }
            }
        }
        fn worley3(x: f32, y: f32, z: f32, seed: i32, period: i32) -> f32 {
            let xr = fastRound(x); let yr = fastRound(y); let zr = fastRound(z);
            let radius = 0.39614353 * p.jitter;
            var f1 = 1e30; var f2 = 1e30; var cell = 0.0;
            for (var oz = -1; oz <= 1; oz = oz + 1) {
                for (var oy = -1; oy <= 1; oy = oy + 1) {
                    for (var ox = -1; ox <= 1; ox = ox + 1) {
                        let cx = xr + ox; let cy = yr + oy; let cz = zr + oz;
                        let h = hash3(seed, wrap(cx, period) * PX, wrap(cy, period) * PY, wrap(cz, period) * PZ);
                        let rv = randVec3(h) * radius;
                        let d = metric3(f32(cx) + rv.x - x, f32(cy) + rv.y - y, f32(cz) + rv.z - z);
                        if (d < f1) { f2 = f1; f1 = d; cell = f32(h) * 4.6566128730773926e-10; }
                        else if (d < f2) { f2 = d; }
                    }
                }
            }
            return clamp(featureOf(f1, f2, cell), -1.0, 1.0);
        }

        fn basisAt(x: f32, y: f32, seed: i32, period: i32) -> f32 {
            switch (p.basis) {
                case 1u: { return simplex(x, y, seed); }
                case 2u: { return valueBasis(x, y, seed, period); }
                case 3u: { return worley(x, y, seed, period); }
                default: { return perlin(x, y, seed, period); }
            }
        }

        fn basisAt3(x: f32, y: f32, z: f32, seed: i32, period: i32) -> f32 {
            switch (p.basis) {
                case 1u: { return simplex3(x, y, z, seed); }
                case 2u: { return value3(x, y, z, seed, period); }
                case 3u: { return worley3(x, y, z, seed, period); }
                default: { return perlin3(x, y, z, seed, period); }
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
            let solid = p.dimension == 1u;
            var sx = uv.x * p.frequency; var sy = uv.y * p.frequency;
            // The solid depth is the layer-centre coordinate — the same slab-centre position the CPU volume
            // law reads — so a one-layer solid plane samples depth 0.5 and a volume varies per slice.
            var sz = 0.0;
            if (solid) { sz = ((f32(gid.z) + 0.5) / f32(max(p.layers, 1u))) * p.frequency; }
            if (p.warpAmp != 0.0) {
                if (solid) {
                    let wx = simplex3(sx * p.warpFreq, sy * p.warpFreq, sz * p.warpFreq, p.warpSeed);
                    let wy = simplex3(sx * p.warpFreq + 1000.0, sy * p.warpFreq, sz * p.warpFreq, p.warpSeed);
                    let wz = simplex3(sx * p.warpFreq, sy * p.warpFreq + 1000.0, sz * p.warpFreq, p.warpSeed);
                    sx = sx + wx * p.warpAmp; sy = sy + wy * p.warpAmp; sz = sz + wz * p.warpAmp;
                } else {
                    let wx = simplex(sx * p.warpFreq, sy * p.warpFreq, p.warpSeed);
                    let wy = simplex(sx * p.warpFreq + 137.0, sy * p.warpFreq - 41.0, p.warpSeed);
                    sx = sx + wx * p.warpAmp; sy = sy + wy * p.warpAmp;
                }
            }
            let octaves = max(1u, p.octaves);
            var sum = 0.0; var amp = bounding(p.gain, octaves); var freq = 1.0;
            let period = i32(p.period);
            for (var o = 0u; o < octaves; o = o + 1u) {
                var n = 0.0;
                if (solid) { n = basisAt3(sx * freq, sy * freq, sz * freq, p.seed + i32(o), i32(f32(period) * freq)); }
                else { n = basisAt(sx * freq, sy * freq, p.seed + i32(o), i32(f32(period) * freq)); }
                var damp = 1.0;
                switch (p.fractal) {
                    case 1u: { let f = abs(n); sum = sum + (f * -2.0 + 1.0) * amp; damp = 1.0 - f; }
                    case 2u: { let q = pingPongWave((n + 1.0) * p.pingPong); sum = sum + (q - 0.5) * 2.0 * amp; damp = q; }
                    default: { sum = sum + n * amp; damp = min(n + 1.0, 2.0) * 0.5; }
                }
                amp = amp * mix(1.0, damp, p.weighted) * p.gain;
                freq = freq * p.lacunarity;
            }
            // The field maps through the source's OWN low and high COLOURS, four lanes and alpha included — the
            // same ShadeVec4 lerp the CPU arm takes. A scalar lo/hi pair previews every authored colour ramp as
            // grey, which is a wrong preview wearing a right name.
            let t = clamp((sum + 1.0) * 0.5, 0.0, 1.0);
            let i = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
            let v = mix(p.lo, p.hi, t);
            dst[i] = v.x; dst[i + 1u] = v.y; dst[i + 2u] = v.z; dst[i + 3u] = v.w;
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
            let i = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
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
            let i = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
            for (var c = 0u; c < 4u; c = c + 1u) { dst[i + c] = mix(lut[lo * 4u + c], lut[hi * 4u + c], f); }
        }
        """;

    // The FULL pointwise MathOp roster, matching the CPU rows shape for shape: the ZIP family folds PER LANE
    // (shape follows the wider operand, a scalar⊕scalar stays scalar — lane replication on the plane makes the
    // componentwise fold serve both), and the AsScalar rows read the POLARITY uniform because the CPU
    // projection reads a colour's AP1 luminance and a scalar port's own value — a kernel assuming .x silently
    // reduces every colour operand to its red channel. Totality conventions match the CPU rows exactly: a
    // zero divisor folds divide AND modulo to zero PER LANE, modulo is FLOORED, a negative sqrt operand
    // clamps to zero, a zero-length normalize returns the zero vector.
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
        // WGSL round() is half-to-even; the CPU row is MidpointRounding.AwayFromZero, so the twin transcribes
        // the CPU rule rather than the intrinsic that happens to share its name.
        fn roundAway(v: f32) -> f32 { if (v >= 0.0) { return floor(v + 0.5); } return ceil(v - 0.5); }
        @compute @workgroup_size(8, 8, 1)
        fn main(@builtin(global_invocation_id) gid : vec3<u32>) {
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let i = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
            let a = vec4<f32>(lhs[i], lhs[i + 1u], lhs[i + 2u], lhs[i + 3u]);
            let b = vec4<f32>(rhs[i], rhs[i + 1u], rhs[i + 2u], rhs[i + 3u]);
            let sa = asScalar(p.lhsKind, a);
            let sb = asScalar(p.rhsKind, b);
            // The ZIP family (add/subtract/multiply/divide/modulo/min/max) folds PER LANE, matching the CPU
            // MathOp's shape-preserving Zip — a scalar channel is lane-replicated on the plane, so the
            // componentwise fold serves both polarities and no arm re-widens a scalar through a broadcast.
            // The scalar projections serve exactly the rows whose CPU delegate reads AsScalar.
            var r = vec4<f32>(0.0, 0.0, 0.0, 1.0);
            switch (p.op) {
                case 0u:  { r = vec4<f32>(a.xyz + b.xyz, 1.0); }
                case 1u:  { r = vec4<f32>(a.xyz - b.xyz, 1.0); }
                case 2u:  { r = vec4<f32>(a.xyz * b.xyz, 1.0); }
                case 3u:  { r = vec4<f32>(safeDiv(a.x, b.x), safeDiv(a.y, b.y), safeDiv(a.z, b.z), 1.0); }
                case 4u:  { r = vec4<f32>(floorMod(a.x, b.x), floorMod(a.y, b.y), floorMod(a.z, b.z), 1.0); }
                case 5u:  { r = vec4<f32>(a.xyz * sb, 1.0); }
                case 6u:  { r = vec4<f32>(vec3<f32>(pow(sa, sb)), 1.0); }
                case 7u:  { r = vec4<f32>(vec3<f32>(sqrt(max(0.0, sa))), 1.0); }
                case 8u:  { r = vec4<f32>(vec3<f32>(abs(sa)), 1.0); }
                case 9u:  { r = vec4<f32>(vec3<f32>(sin(sa)), 1.0); }
                case 10u: { r = vec4<f32>(vec3<f32>(cos(sa)), 1.0); }
                case 11u: { r = vec4<f32>(min(a.xyz, b.xyz), 1.0); }
                case 12u: { r = vec4<f32>(max(a.xyz, b.xyz), 1.0); }
                case 13u: { r = vec4<f32>(vec3<f32>(dot(a.xyz, b.xyz)), 1.0); }
                case 14u: { r = vec4<f32>(cross(a.xyz, b.xyz), 1.0); }
                case 15u: { r = vec4<f32>(norm(a.xyz), 1.0); }
                case 16u: { r = vec4<f32>(vec3<f32>(clamp(sa, 0.0, 1.0)), 1.0); }
                case 17u: { r = vec4<f32>(vec3<f32>(1.0 - sa), 1.0); }
                case 18u: { r = vec4<f32>(vec3<f32>(schlick(clamp(dot(a.xyz, b.xyz), 0.0, 1.0))), 1.0); }
                case 19u: { r = vec4<f32>(atan2(a.x, b.x), atan2(a.y, b.y), atan2(a.z, b.z), 1.0); }
                case 20u: { r = vec4<f32>(vec3<f32>(sign(sa)), 1.0); }
                case 21u: { r = vec4<f32>(vec3<f32>(floor(sa)), 1.0); }
                case 22u: { r = vec4<f32>(vec3<f32>(ceil(sa)), 1.0); }
                case 23u: { r = vec4<f32>(vec3<f32>(roundAway(sa)), 1.0); }
                case 24u: { r = vec4<f32>(vec3<f32>(exp(sa)), 1.0); }
                case 25u: { r = vec4<f32>(vec3<f32>(select(0.0, log(sa), sa > 0.0)), 1.0); }
                case 26u: { r = vec4<f32>(vec3<f32>(length(a.xyz)), 1.0); }
                case 27u: { r = vec4<f32>(vec3<f32>(length(a.xyz - b.xyz)), 1.0); }
                default:  { r = a; }
            }
            dst[i] = r.x; dst[i + 1u] = r.y; dst[i + 2u] = r.z; dst[i + 3u] = r.w;
        }
        """;

    // The FULL W3C compositing vocabulary: eleven separable modes plus the four non-separable HSL modes, each
    // blended b over a then lerped by the factor as blend opacity — the same algebra the CPU MixOp row reads
    // out of Unicolour, so a graph taking the GPU arm composites identically in structure, W3C Lum
    // coefficients included. The CPU path clips the blended value into the [0,1] W3C reflectance domain
    // through Unicolour's own Blend, so an HDR intermediate diverges here by that clip alone — the divergence
    // the parity workload measures.
    internal const string MixFold = """
        struct Params { extent: vec2<u32>, mode: u32, pad: u32, factor: f32, pad0: f32, pad1: f32, pad2: f32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> backdrop : array<f32>;
        @group(0) @binding(2) var<storage, read> source : array<f32>;
        @group(0) @binding(3) var<storage, read_write> dst : array<f32>;
        // The W3C compositing Lum coefficients — the algebra the CPU MixOp reads out of Unicolour's Blend —
        // never the AP1 luminance weights, which belong to the SCALAR polarity projection in mathFold: the
        // four non-separable modes are defined over W3C's own 0.3/0.59/0.11, and an AP1 lum here diverges
        // every Hue/Saturation/Colour/Luminosity arm from the bake, not just the HDR clip.
        fn lum(c: vec3<f32>) -> f32 { return dot(c, vec3<f32>(0.3, 0.59, 0.11)); }
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
            let i = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
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
        // `var`, never `let`: the accumulation loop indexes the basis with a runtime k, and WGSL admits a
        // runtime index only against a reference — a let-bound array VALUE and a module-scope `const` array
        // alike demand a const index and refuse to compile, so every runtime-indexed table in this module
        // table is `var` or `var<private>`.
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
                var b = basis(d);
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

    // This module TRANSCRIBES the CPU specular level, member for member: the Heitz VNDF draw is
    // Microfacet.SampleVisibleNormal, the tangent-to-world completion is the sweep's own Oriented, the
    // low-discrepancy pair is Deterministic.Hammersley's half-texel-offset form, the alpha floor is
    // Microfacet.AlphaOf's own 1e-4, and every level lands EQUIRECT because the product's arrangement is the
    // CPU fold's declared law. Each tap weights by N.L with the below-horizon half discarded and the weight
    // sum normalizing, exactly as environment#IBL_PREFILTER's SpecularSweep does.
    internal const string PrefilterSpecular = """
        struct Params { extent: vec2<u32>, srcWidth: u32, srcHeight: u32, roughness: f32, samples: u32, pad0: u32, pad1: u32 };
        @group(0) @binding(0) var<uniform> p : Params;
        @group(0) @binding(1) var<storage, read> equirect : array<f32>;
        @group(0) @binding(2) var<storage, read_write> dst : array<f32>;
        const PI : f32 = 3.141592653589793;
        const TAU : f32 = 6.283185307179586;
        // equirectDir INVERTS the frozen correspondence — phi = (u - 0.5)*2pi, theta = v*pi — four lines off the
        // forward law this page already transcribes, so the level this kernel writes and the level the
        // CPU fold writes address one arrangement and no re-projection stands between them.
        fn equirectDir(u: f32, v: f32) -> vec3<f32> {
            let phi = (u - 0.5) * TAU;
            let theta = v * PI;
            let st = sin(theta);
            return vec3<f32>(st * cos(phi), st * sin(phi), cos(theta));
        }
        fn radical(bits: u32) -> f32 {
            var b = bits;
            b = (b << 16u) | (b >> 16u);
            b = ((b & 0x55555555u) << 1u) | ((b & 0xAAAAAAAAu) >> 1u);
            b = ((b & 0x33333333u) << 2u) | ((b & 0xCCCCCCCCu) >> 2u);
            b = ((b & 0x0F0F0F0Fu) << 4u) | ((b & 0xF0F0F0F0u) >> 4u);
            b = ((b & 0x00FF00FFu) << 8u) | ((b & 0xFF00FF00u) >> 8u);
            return f32(b) * 2.3283064365386963e-10;
        }
        // Microfacet.SampleVisibleNormal at f32, in TANGENT space: the visible-normal draw the conductor,
        // dielectric, and clearcoat lobes share, so the prefiltered dome and the shaded surface integrate one
        // distribution. wo is the local normal at this call site — the split-sum's own N = V = R assumption —
        // and the body still carries the general wo so the transcription is the twin rather than its special
        // case. A D-proportional half-vector draw here is a SECOND algorithm even where the two agree in the
        // limit, because the same Hammersley pair drives the polar and azimuth angles in OPPOSITE roles.
        fn sampleVisibleNormal(wo: vec3<f32>, ax: f32, ay: f32, u0: f32, u1: f32) -> vec3<f32> {
            let vh = normalize(vec3<f32>(ax * wo.x, ay * wo.y, wo.z));
            let lensq = vh.x * vh.x + vh.y * vh.y;
            let t1 = select(vec3<f32>(1.0, 0.0, 0.0), vec3<f32>(-vh.y, vh.x, 0.0) * inverseSqrt(max(lensq, 1e-20)), lensq > 0.0);
            let t2 = cross(vh, t1);
            let r = sqrt(u0);
            let phi = TAU * u1;
            let p1 = r * cos(phi);
            let s = 0.5 * (1.0 + vh.z);
            let p2 = (1.0 - s) * sqrt(max(0.0, 1.0 - p1 * p1)) + s * (r * sin(phi));
            let pz = sqrt(max(0.0, 1.0 - p1 * p1 - p2 * p2));
            let nh = t1 * p1 + t2 * p2 + vh * pz;
            return normalize(vec3<f32>(ax * nh.x, ay * nh.y, max(1e-6, nh.z)));
        }
        // oriented IS the one tangent-to-world crossing, twin of the CPU sweep's own Oriented completion: an
        // orthonormal frame per texel rotates the tangent-space draw onto the texel's world normal.
        fn oriented(n: vec3<f32>, local: vec3<f32>) -> vec3<f32> {
            let up = select(vec3<f32>(0.0, 0.0, 1.0), vec3<f32>(1.0, 0.0, 0.0), abs(n.z) > 0.999);
            let tx = normalize(cross(up, n));
            let ty = cross(n, tx);
            return normalize(tx * local.x + ty * local.y + n * local.z);
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
            if (gid.x >= p.extent.x || gid.y >= p.extent.y) { return; }
            let n = equirectDir((f32(gid.x) + 0.5) / f32(p.extent.x), (f32(gid.y) + 0.5) / f32(p.extent.y));
            let a = max(1e-4, p.roughness * p.roughness);
            var acc = vec3<f32>(0.0); var weight = 0.0;
            for (var i = 0u; i < p.samples; i = i + 1u) {
                let h = oriented(n, sampleVisibleNormal(vec3<f32>(0.0, 0.0, 1.0), a, a, (f32(i) + 0.5) / f32(p.samples), radical(i)));
                let l = normalize(2.0 * dot(n, h) * h - n);
                let ndl = dot(n, l);
                if (ndl > 0.0) { acc = acc + sample(l) * ndl; weight = weight + ndl; }
            }
            let r = select(vec3<f32>(0.0), acc / weight, weight > 0.0);
            let di = ((gid.z * p.extent.y + gid.y) * p.extent.x + gid.x) * 4u;
            dst[di] = r.x; dst[di + 1u] = r.y; dst[di + 2u] = r.z; dst[di + 3u] = 1.0;
        }
        """;
}
```

## [04]-[GOLDEN_VECTOR]

- Owner: `GoldenVector` the per-kernel fixture row; `Golden` the fixture table.
- Law: every expected value is EXACTLY COMPUTABLE from the algorithm's own definition — INCLUDING its own quadrature where the kernel integrates. A Perlin lattice node is exactly zero because both corner displacement vectors vanish there; a checker parity is an integer; a two-texel LUT midpoint is exactly the mean; the constant-radiance SH projection is the closed-form midpoint sum — `sh_0 = K · (2π²/h) · csc(π/2h)`, whose limit is the analytic `2√π` and whose gap IS the midpoint-rule error the resolution fixes, and `sh_6 = K₆ · 2π · (π/h) · ((3/4)csc(3π/2h) − (1/4)csc(π/2h))`, the ONE band whose quadrature residue does not cancel, derived from `(3cos²θ−1)sinθ = (3/4)sin3θ − (1/4)sinθ` — while every azimuth-dependent band cancels exactly on the uniform φ grid; a constant environment prefilters to itself at every roughness because the weight sum normalizes; a one-tap visible-normal draw lands the (8, 15, 17) triple in exact binary, so the prefiltered value IS the source texel that one direction addresses. A property EVERY candidate algorithm satisfies proves the WEIGHT and nothing else, so a kernel whose algorithm is settled carries a second fixture keyed on the draw itself — the invariant fixture and the draw fixture are a deliberate pair, never two similar cases. A transcribed decimal nobody can re-derive is the deleted form, an ANALYTIC value asserted against a DISCRETE kernel at a tolerance the quadrature cannot reach is the same defect wearing a derivation, and a ZERO asserted where the quadrature legitimately leaves a residue is its twin — each fails a CORRECT kernel and passes only after someone loosens it.
- Law: a fixture's `Input` supplies every READ buffer the kernel's `Layout` declares. A sampling kernel whose fixture supplies no input reads an unbound or zero buffer, so its expected value describes a dispatch that never happened.
- Entry: `public static Fin<Unit> Prove(PressDevice device, WgslKernel kernel, Op key)` runs the op-code totality gate, then dispatches EVERY fixture on the row's own `Golden` sequence and compares each reduced output's leading `Expected.Length` elements against the fixture's tolerance; `Golden.All` DERIVES from `WgslKernel.Items` — the `Projection/benchmarks` parity workload and the proof estate iterate one public projection, and no hand-maintained second roster exists to drift.
- Packages: `[02]-[PRESS_DEVICE]` and `[03]-[WGSL_KERNEL]` (the device and table this proves), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new kernel's fixtures are the `golden:` sequence on its own row — a kernel without at least one cannot be declared, because the row's constructor takes the sequence, and a kernel whose law spans several conventions proves each under its OWN dispatch (a single-op uniform cannot exercise three ops in one pass).
- Boundary: the comparison is a PREFIX read of the reduced output, because a fixture pins the texels its dispatch determines and a full-plane expectation would restate the kernel; the row declares the extent that makes the prefix meaningful, so `Expected` and `Uniform` are read together or not at all. Tolerance is per-fixture: `1e-6` ABSOLUTE for the exactly-zero, integer, and single-tap cases — a one-tap dispatch normalizes by its own weight, so the answer is the source texel to within one rounding and a quadrature bound is slack the fixture never needs — and per-band ABSOLUTE sized to the accumulation error where a reduction sums thousands of `f32` terms — the irradiance row's `1e-4` sits three orders below both of its non-zero values, so a band-order swap, a normalization slip, and a fake zero all still fail while a correct kernel passes; a tolerance loose enough to hide a wrong gradient table is worse than no fixture. The irradiance vector doubles as the AXIS discriminator: the companion `L(ω) = ω·ẑ` case places its single non-zero coefficient at `sh_2`, and a `+Y`-up implementation places it at `sh_1` or `sh_3` and fails — which is the one check that catches an up-axis fork every visual comparison passes. The cube fixture paints SIX DISTINCT probes — four single-texel equator probes pinning the azimuth origin and every equator-face permutation outright, two whole-row pole probes pinning the up axis without reading `u` at the pole, where `atan2(0, 0)` is indeterminate and a single-texel probe would pin an implementation detail; the per-face `(s, t)` axis signs ride the frozen `faceDir` text alone, with the off-centre edge-≥2 fixture the declared growth leg. A golden failure rails `RasterFault.Device` naming the kernel, the fixture, and the divergent index, and it is a HARD failure rather than a telemetry row: the CPU-versus-GPU divergence a press measures is telemetry precisely because the CPU result is authoritative there, whereas a kernel disagreeing with its own closed-form answer is a broken kernel.

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
            .F32(0.0).F32(0.0).F32(1.0)                                 // period, warpAmp, warpFreq
            .U32(1).I32(1337).Code(WgslOpCode.Of(NoiseBasis.Perlin)).Code(WgslOpCode.Of(FractalMode.FBm))   // octaves, seed, basis, fractal
            .Code(WgslOpCode.Of(CellularDistance.EuclideanSq)).Code(WgslOpCode.Of(CellularReturn.Distance)).I32(0)   // metric, feature, warpSeed
            .Vec4(-1.0, -1.0, -1.0, 1.0).Vec4(1.0, 1.0, 1.0, 1.0),      // lo, hi — the Vec4 pad zeroes dimension (planar) and layers (unread on the planar path)
        Input: Seq<ReadOnlyMemory<float>>(),
        Expected: new[] { 0f, 0f, 0f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // The solid twin: at dimension 1 the one texel centre (uv 0.5, layer-centre depth 0.5 over one layer) at
    // frequency 2 lands on lattice node (1, 1, 1), where every Perlin3D corner displacement vanishes — one
    // dispatch proving the dimension column, the layer-depth derivation, and the 3D gradient path together.
    internal static readonly GoldenVector NoiseSolidLatticeZero = new("noise-solid-lattice-node",
        KernelUniform.Empty.Extent(One, One).F32(2.0).F32(2.0)          // extent, frequency, lacunarity
            .F32(0.5).F32(0.0).F32(2.0).F32(1.0)                        // gain, weighted, pingPong, jitter
            .F32(0.0).F32(0.0).F32(1.0)                                 // period, warpAmp, warpFreq
            .U32(1).I32(1337).Code(WgslOpCode.Of(NoiseBasis.Perlin)).Code(WgslOpCode.Of(FractalMode.FBm))   // octaves, seed, basis, fractal
            .Code(WgslOpCode.Of(CellularDistance.EuclideanSq)).Code(WgslOpCode.Of(CellularReturn.Distance)).I32(0).U32(1).U32(1)   // metric, feature, warpSeed, dimension SOLID, layers
            .Vec4(-1.0, -1.0, -1.0, 1.0).Vec4(1.0, 1.0, 1.0, 1.0),
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

    // The three totality conventions, ONE DISPATCH EACH — a single-op uniform cannot exercise three ops, and
    // the prior one-dispatch form proved floored modulo alone while claiming all three. Floored modulo of
    // -1.5 by 1 is 0.5 (never the CLR remainder -0.5) and a zero modulus folds to 0; a zero divisor folds
    // divide to 0 per lane; a negative sqrt operand clamps to 0. The zip rows read lane-replicated scalars,
    // so every expected texel replicates its scalar across XYZ.
    internal static readonly GoldenVector MathFloorMod = new("math-floor-mod",
        KernelUniform.Empty.Extent(Two, One).U32(4).U32(0).U32(0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(
            new[] { -1.5f, -1.5f, -1.5f, 1f, 1f, 1f, 1f, 1f },
            new[] { 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f }),
        Expected: new[] { 0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0f, 1f }, Width: Two, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    internal static readonly GoldenVector MathSafeDiv = new("math-safe-div",
        KernelUniform.Empty.Extent(Two, One).U32(3).U32(0).U32(0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(
            new[] { 1f, 1f, 1f, 1f, 3f, 3f, 3f, 1f },
            new[] { 0f, 0f, 0f, 1f, 2f, 2f, 2f, 1f }),
        Expected: new[] { 0f, 0f, 0f, 1f, 1.5f, 1.5f, 1.5f, 1f }, Width: Two, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    internal static readonly GoldenVector MathSqrtClamp = new("math-sqrt-clamp",
        KernelUniform.Empty.Extent(Two, One).U32(7).U32(0).U32(0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(
            new[] { -4f, -4f, -4f, 1f, 4f, 4f, 4f, 1f },
            new[] { 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f }),
        Expected: new[] { 0f, 0f, 0f, 1f, 2f, 2f, 2f, 1f }, Width: Two, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // Multiply at full opacity over a 0.5 backdrop and a 0.5 source is exactly 0.25.
    internal static readonly GoldenVector MixMultiply = new("mix-multiply",
        KernelUniform.Empty.Extent(One, One).U32(1).Pad(1).F32(1.0).Pad(3),
        Input: Seq<ReadOnlyMemory<float>>(new[] { 0.5f, 0.5f, 0.5f, 1f }, new[] { 0.5f, 0.5f, 0.5f, 1f }),
        Expected: new[] { 0.25f, 0.25f, 0.25f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // ALL SIX face centres in one dispatch at edge 1, faces riding gid.z, against an 8x4 source whose probes
    // are SIX DISTINCT colours — so the fixture pins the face ORDER outright, not an equivalence class of it.
    // Per face centre: +X is d = (1,0,0), u = 0.5, v = 0.5 -> texel (4,2); -X is u = 1.0 -> clamped column 7,
    // row 2; +Y is u = 0.75 -> (6,2); -Y is u = 0.25 -> (2,2); +Z is v = 0 -> ROW 0, painted WHOLE because
    // atan2(0,0) at the pole is indeterminate in WGSL and a single-texel probe would pin an implementation
    // detail; -Z is v = 1 -> row 3, likewise painted whole. Four distinct equator probes discriminate the
    // azimuth origin and every face permutation; the two painted pole rows discriminate the up axis without
    // reading an undefined u. The per-face (s,t) axis SIGNS are pinned by the frozen faceDir text alone —
    // centres sample s = t = 0, so an off-centre fixture at edge >= 2 is the declared growth leg, not a fact
    // this fixture claims.
    internal static readonly GoldenVector CubeFaceCentre = new("cube-face-centre",
        KernelUniform.Empty.U32(0).U32(1).U32(8).U32(4),
        Input: Seq<ReadOnlyMemory<float>>(CubeSource()),
        Expected: new[] {
            0.125f, 0f, 0f, 1f,        // +X: equator at the azimuth origin
            0.25f,  0f, 0f, 1f,        // -X: u = 1.0, the clamped far column
            0.375f, 0f, 0f, 1f,        // +Y: u = 0.75
            0.5f,   0f, 0f, 1f,        // -Y: u = 0.25
            0.625f, 0.5f, 0.25f, 1f,   // +Z: the painted pole row — the up-axis discriminator
            0.75f,  0.25f, 0.125f, 1f },  // -Z: the opposite painted pole row
        Width: One, Height: One, Layers: Six, Tolerance: 1e-6, Relative: false);

    // The 8x4 equirect source the cube fixture samples: row 0 and row 3 painted whole with the two pole
    // probes, four single-texel equator probes on row 2 at the four face-centre columns, black elsewhere.
    static ReadOnlyMemory<float> CubeSource() {
        float[] plane = new float[8 * 4 * 4];
        for (int x = 0; x < 8; x++) {
            Paint(plane, x, row: 0, 0.625f, 0.5f, 0.25f);
            Paint(plane, x, row: 3, 0.75f, 0.25f, 0.125f);
        }
        Paint(plane, 4, row: 2, 0.125f, 0f, 0f);
        Paint(plane, 7, row: 2, 0.25f, 0f, 0f);
        Paint(plane, 6, row: 2, 0.375f, 0f, 0f);
        Paint(plane, 2, row: 2, 0.5f, 0f, 0f);
        for (int texel = 0; texel < 32; texel++) { plane[(texel * 4) + 3] = 1f; }
        return plane;
    }

    static void Paint(float[] plane, int x, int row, float r, float g, float b) {
        int at = ((row * 8) + x) * 4;
        (plane[at], plane[at + 1], plane[at + 2]) = (r, g, b);
    }

    // L = 1 over the whole sphere. The kernel is a MIDPOINT quadrature, so every expected value is the
    // closed-form SUM, not the analytic integral. sh_0: sum over rows of sin((j+0.5)pi/h) is csc(pi/2h), so
    // sh_0 = K * (2pi^2/h) * csc(pi/2h) = 0.28209479177387814 * 0.6168502750680849 / 0.049067674327418015
    // = 3.5463317, whose limit as h grows is the analytic 2*sqrt(pi) = 3.5449077 and whose 4.0e-4 relative
    // gap IS the midpoint error the height fixes. Bands 1-5, 7, 8 cancel EXACTLY (azimuthal sums of sin/cos
    // multiples vanish on the uniform phi grid; the band-2 row sum hits sin^2(pi) = 0). sh_6 does NOT cancel:
    // (3cos^2 t - 1) sin t = (3/4) sin 3t - (1/4) sin t, so its quadrature residue is
    // sh_6 = K6 * 2pi * (pi/h) * ((3/4) csc(3pi/2h) - (1/4) csc(pi/2h)) = 0.0031923 per channel — the one
    // band the discrete kernel legitimately leaves non-zero, and a fixture expecting 0 there fails a CORRECT
    // kernel by eight orders under a relative bound. Tolerance is per-band ABSOLUTE at 1e-4: wide enough for
    // f32 accumulation over 2048 texels, and three orders below both non-zero values, so a band-order swap, a
    // normalization slip, and a fake-zero all still fail. The companion L = w.z case places its single
    // non-zero at sh_2, the AXIS discriminator a +Y-up implementation fails. The groups word derives from
    // KernelReduce.ReductionGroups — the SAME expression the dispatch and the write sizing read.
    internal static readonly GoldenVector ConstantIrradiance = new("constant-irradiance",
        KernelUniform.Empty.U32(64).U32(32).U32(checked((int)KernelReduce.ReductionGroups(64 * 32))).Pad(1),
        Input: Seq<ReadOnlyMemory<float>>(Constant(64 * 32)),
        Expected: new[] {
            3.5463317f, 3.5463317f, 3.5463317f,
            0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
            0.0031923f, 0.0031923f, 0.0031923f,
            0f, 0f, 0f, 0f, 0f, 0f },
        Width: SixtyFour, Height: ThirtyTwo, Layers: One, Tolerance: 1e-4, Relative: false);

    // A constant environment prefilters to that constant at EVERY roughness, because the N.L weights sum in
    // both numerator and denominator — the one property that catches a broken importance-sample WEIGHT. It
    // catches nothing about the DIRECTION the weight is applied at: a constant dome answers the same value
    // for every draw, every tangent frame, and every extent, which is why the sibling below reads a drawn
    // direction instead. The output is EQUIRECT at the row's declared 2:1 aspect.
    internal static readonly GoldenVector ConstantPrefilter = new("constant-prefilter",
        KernelUniform.Empty.Extent(Four, Two).U32(8).U32(4).F32(0.5).U32(64).Pad(2),
        Input: Seq<ReadOnlyMemory<float>>(Constant(8 * 4)),
        Expected: new[] { 1f, 1f, 1f, 1f }, Width: Four, Height: Two, Layers: One, Tolerance: 1e-5, Relative: true);

    // This fixture DISCRIMINATES the sampler, exact in closed form because its dispatch is ONE tap. At samples = 1
    // Hammersley yields u0 = (0 + 0.5)/1 = 0.5 and u1 = radical(0) = 0, and the VNDF at wo = the local
    // normal reduces to vh = (0,0,1), t1 = (1,0,0), t2 = (0,1,0), s = 1 — so p1 = sqrt(0.5), p2 = 0,
    // pz = sqrt(0.5) and h = normalize(a*sqrt(0.5), 0, sqrt(0.5)). At roughness 0.5 the alpha floor leaves
    // a = 0.25, so h.z^2 = 0.5 / (0.5*0.0625 + 0.5) = 16/17, giving N.L = 2*h.z^2 - 1 = 15/17 and the
    // tangent x lane 2*h.z*h.x = 8/17 — the (8, 15, 17) triple, EXACT in binary at f32.
    // The
    // 1x1 output texel sits at u = v = 0.5, so its equirect direction is N = (1, 0, 0) and the oriented
    // frame maps tangent x onto +Y and tangent y onto +Z: the tap lands at L = (15/17, 8/17, 0). Its
    // longitude is 0.5 + atan2(8, 15)/2pi = 0.577979, so column floor(0.577979 * 16) = 9 of a sixteen-column
    // source, a quarter texel clear of the boundary; its latitude is acos(0)/pi = 0.5, row 0 of the one-row
    // source. One tap makes the weight normalization exact, so the prefiltered value IS that texel.
    //
    // Every wrong sampler lands on column 8 instead, which the source paints white: a D-proportional
    // half-vector draw reads cos(theta_h) = sqrt((1 - u1)/(1 + (a^2 - 1)*u1)) = 1 at u1 = 0, collapsing h
    // onto N; and a Hammersley pair missing its half-texel offset reads u0 = 0, collapsing nh onto N by the
    // other road. One fixture separates the visible-normal draw, the D-proportional draw, and the offset
    // defect, and it does so at exact arithmetic rather than at a quadrature bound.
    internal static readonly GoldenVector SplitDomePrefilter = new("split-dome-prefilter",
        KernelUniform.Empty.Extent(One, One).U32(16).U32(1).F32(0.5).U32(1).Pad(2),
        Input: Seq<ReadOnlyMemory<float>>(Meridian(16, mirror: 8, drawn: 9)),
        Expected: new[] { 0.25f, 0.5f, 0.75f, 1f }, Width: One, Height: One, Layers: One, Tolerance: 1e-6, Relative: false);

    // ONE roster, DERIVED: every kernel's own fixtures in table order — a hand-maintained second sequence
    // beside the rows is the dual-numbering defect this page names, and the derived form reaches the parity
    // workload and the proof estate as a public read.
    public static Seq<GoldenVector> All => toSeq(WgslKernel.Items).Bind(static kernel => kernel.Golden);

    // EVERY fixture on the row, each under its own dispatch, compared as a PREFIX of the reduced output: a
    // fixture pins the texels its extent determines, and a full-plane expectation would restate the kernel it
    // proves. The op-code totality gate runs FIRST, so a roster append that missed a lowering row rails by
    // name before any fixture reads a wrong code. A divergence is a HARD failure naming the kernel, the
    // fixture, and the index — a kernel disagreeing with its own closed-form answer is broken, where a
    // CPU-versus-GPU gap is telemetry because the CPU result is authoritative there.
    public static Fin<Unit> Prove(PressDevice device, WgslKernel kernel, Op key) =>
        WgslOpCode.Total(key).Bind(_ =>
            kernel.Golden.Fold(Fin.Succ(unit), (acc, fixture) =>
                acc.Bind(__ => device.Dispatch(kernel, Bind(kernel, fixture, kernel.Groups(fixture.Width, fixture.Height, fixture.Layers)), key)
                    .Bind(receipt => Compare(kernel, fixture, receipt, key)))));

    // Buffer ORDER is the layout: the uniform block first, then every declared read plane, then the one write
    // sized by the kernel's OWN WriteElements — the same derivation the press dispatch reads, so a reduction
    // fixture allocates groups x stride floats and never a texel-count buffer three orders too large.
    static KernelBinding Bind(WgslKernel kernel, GoldenVector fixture, (uint X, uint Y, uint Z) groups) =>
        new(fixture.Input.Fold(Seq(fixture.Uniform.Block), static (buffers, plane) => buffers.Add(new KernelBuffer.Read(plane)))
                .Add(new KernelBuffer.Write(kernel.WriteElements(fixture.Width, fixture.Height, fixture.Layers))),
            groups.X, groups.Y, groups.Z);

    static Fin<Unit> Compare(WgslKernel kernel, GoldenVector fixture, KernelReceipt receipt, Op key) {
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

    // The meridian source the split-dome probe reads: one row, the mirror column painted white and the drawn
    // column painted a distinct triple, every other column black — so a tap landing on the wrong side reads a
    // NAMED wrong value the divergence message quotes, never the zero an absent or unbound read also produces.
    static ReadOnlyMemory<float> Meridian(int texels, int mirror, int drawn) {
        float[] plane = new float[texels * 4];
        for (int texel = 0; texel < texels; texel++) { plane[(texel * 4) + 3] = 1f; }
        (plane[mirror * 4], plane[(mirror * 4) + 1], plane[(mirror * 4) + 2]) = (1f, 1f, 1f);
        (plane[drawn * 4], plane[(drawn * 4) + 1], plane[(drawn * 4) + 2]) = (0.25f, 0.5f, 0.75f);
        return plane;
    }

    static readonly Dimension One = Dimension.Create(1);
    static readonly Dimension Two = Dimension.Create(2);
    static readonly Dimension Four = Dimension.Create(4);
    static readonly Dimension Six = Dimension.Create(6);
    static readonly Dimension ThirtyTwo = Dimension.Create(32);
    static readonly Dimension SixtyFour = Dimension.Create(64);
}
```

## [05]-[KERNEL_CHAIN]

- Owner: `ChainNode` the caller's per-node lowering request; `ChainStep` the allocated dispatch row; `ChainPlan` the whole chain with its slot pool and its terminal slot; the `PressDevice.Dispatch` plural modality that executes one.
- Law: A CHAIN IS THE PLURAL MODALITY OF ONE DISPATCH, discriminating on the REQUEST's own shape and never on a name suffix or a flag. A `KernelBinding` names one kernel and carries HOST buffers; a `ChainPlan` names an ordered chain whose intermediates never leave the device. That distinction is the whole throughput argument: the chain uploads one uniform block per step and reads back ONE plane at the end, where routing each node through the singular entry would pay a buffer map, a host copy, and a submission drain per node — three round trips per node against zero.
- Law: the CALLER LOWERS, THIS OWNER PLANS. A `ChainNode` names a kernel row, the indices of the earlier nodes it consumes, and its own uniform words — nothing else. `press#TEXTURE_PRESS` lowers a compiled `MaterialGraph` into that sequence (a field kernel per procedural `Texture` node, `mathFold` per `Math` node, `mixFold` per `Mix` node, in the compiled topological order), so this page never names an `AppearanceNode` and its dependency on the appearance graph stays exactly the `MathOp`/`MixOp` vocabularies it already lowers. A node union crossing here would make the device page a consumer of the graph algebra.
- Law: OPERAND INDICES POINT STRICTLY BACKWARD and operand ARITY equals the kernel row's own `Reads` count. The caller's sequence IS the compiled topological sort, so a forward or self reference is a sequence that is not one, and a short or long operand list would bind a slot the shader never declared. Both refuse BEFORE the scan, because the scan's totality rests on both.
- Law: THE ALLOCATOR IS A LINEAR SCAN over that order — the classic linear-scan register allocation with plane-sized buffers where a register allocator has registers, and it is affordable precisely because the compile already owns the sort. A node's output LIVES from its own dispatch until its LAST consumer's; one pass over the operand relation raises each producer's death to the highest step that reads it, retirement buckets group the deaths so freeing is O(1) per node rather than a re-walk of every predecessor, and slots recycle through a free stack. The HIGH-WATER MARK is the slot count, and it is exactly the DAG's MAXIMUM LIVE WIDTH — a chain of a hundred pointwise nodes allocates two buffers, while a wide fan-in allocates its fan.
- Law: ASSIGN BEFORE RETIRE, and the order is CORRECTNESS rather than taste. A node's operands are bound as READS in the very dispatch that writes its output, and one bind group may not alias a buffer as both a read and a write. Retiring an operand whose last use is this step BEFORE assigning this step's own slot would hand the node the buffer it is reading — a data race a strict driver reports as an aliasing error and a lax one silently miscomputes into a plausible plane. The TERMINAL never retires at all, because the readback is its consumer.
- Law: THREE CEILINGS, each answering a different question and none standing for another. PER-BUFFER: one slot holds one plane, so a plane must fit `maxStorageBufferBindingSize`'s conformance minimum — the same floor the single-dispatch lane takes, read here per slot, which is why a chain's extent ceiling is identical to a single kernel's. PER-STAGE: a step binds its operands plus its own output, so an operand arity above seven exceeds the conformance minimum storage-binding count, and the structural gate refuses it before the scan. AGGREGATE: the whole slot pool is resident at once, and WebGPU publishes NO conformance minimum for total device allocation — so that bound is an ESTATE DECLARATION rather than a floor and is stated as one. A device granting less than the declaration refuses at `[02]`'s own dispatch gate against its granted limits, exactly as an over-floor extent does.
- Law: A REFUSED CHAIN REFUSES THE ACCELERATOR, never the bake. `press#PRESS_PLAN` reads this admission as the `Graph` subject's lowerability arm, and its recourse is the CPU lane — which is content-authoritative anyway, so the fallback loses throughput and nothing else. A chain product is a `Preview` like every other accelerator product: it carries no `TextureSet` and therefore no key, so the content-identity veto needs no new statement here.
- Entry: `ChainPlan.Of(Seq<ChainNode> nodes, Op key)` is the ONE planner — it gates the sequence structurally, scans the live ranges, and answers the assignment with its slot count and terminal slot; `ChainPlan.Admits(long texels, Op key)` is the ONE footprint gate a plan admission reads; `PressDevice.Dispatch(ChainPlan plan, Dimension width, Dimension height, Dimension layers, Op key)` executes it against a pool allocated once.
- Packages: Silk.NET.WebGPU (`DeviceCreateBuffer` the slot pool, `DeviceCreateBindGroup` per step over the pipeline's own auto-derived layout, `ComputePassEncoderSetPipeline`/`SetBindGroup`/`DispatchWorkgroups`, `CommandEncoderCopyBufferToBuffer` the ONE terminal readback, `QueueSubmit`), `Rasm.Numerics` (`Dimension`), `codec#RASTER_FAULT` (`RasterFault.Device`), LanguageExt.Core.
- Growth: a new lowerable node class is one `WgslKernel` row plus one lowering arm at the CALLER — the planner reads `Reads` off the row and needs no edit. A different allocation policy is a different scan over the same live-range table; the table itself is the derivation every policy would share.
- Boundary: the plan is CPU-SIDE PLANNING DATA and holds no device handle — it is derived once from the compiled order and is reusable across extents, which is what lets the footprint gate answer a plan question without renting a device. The pool is allocated per EXECUTION and released at its close, so a plan outlives a device and a device outlives no plan. Every step records into ONE encoder and the whole chain submits ONCE, so the submission-index drain and the error scope that `[02]` owns wrap the chain exactly as they wrap a single dispatch. The `[EXPRESSION_SPINE]` exemptions here are the two scans — the live-range pass and the allocation walk — which are fixed-extent index folds over caller-owned arrays.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — the [02] prelude is in scope.)

// --- [MODELS] ------------------------------------------------------------------------------
// One node as the CALLER lowers it: a kernel row, the earlier nodes it consumes by index, and its own uniform
// words. No appearance-graph type crosses, so this page stays a consumer of the MathOp/MixOp vocabularies alone
// and never of the node algebra that carries them.
public readonly record struct ChainNode(WgslKernel Kernel, Seq<int> Operands, ReadOnlyMemory<uint> Words);

// One node AFTER allocation: the same kernel and words, with slot indices where the node had node indices.
public readonly record struct ChainStep(WgslKernel Kernel, Seq<int> Reads, int Write, ReadOnlyMemory<uint> Words);

// The planned chain. Slots is the pool width the execution allocates and Terminal the slot the readback reads —
// both derived, neither declared, so a caller cannot assert a pool the assignment does not use.
public sealed record ChainPlan(Seq<ChainStep> Steps, int Slots, int Terminal) {
    // Sixteen bytes per texel is [03]'s own storage arrangement — four f32 lanes per RGBA texel — so a slot's
    // size and the single-dispatch lane's texel ceiling are one fact read at two scales.
    const long BytesPerTexel = 16;
    // The WebGPU conformance minimum for maxStorageBufferBindingSize: the guaranteed floor every conformant
    // device grants, and the same number press#PRESS_PLAN's accelerator row reads for a single plane.
    const long BindingFloor = 134_217_728;
    // The conformance minimum for maxStorageBuffersPerShaderStage. A step binds its operands plus its output.
    const int StorageBindingsPerStage = 8;
    // An ESTATE DECLARATION, not a conformance floor — WebGPU publishes no guaranteed minimum for total device
    // allocation, so the aggregate bound is this corpus's own and says so. One gibibyte holds sixteen slots at
    // the 2048-square extent the single-buffer floor already caps a preview at.
    const long Footprint = 1L << 30;

    public long Bytes(long texels) => Slots * texels * BytesPerTexel;

    // THE PLANNER. Structural gates first, because the scan below is total only under them; then the live-range
    // pass; then the allocation walk whose high-water mark IS the slot count.
    public static Fin<ChainPlan> Of(Seq<ChainNode> nodes, Op key) {
        if (nodes.IsEmpty) { return RasterFault.Device(key, "<chain-empty>"); }
        int count = nodes.Count;
        for (int at = 0; at < count; at++) {
            ChainNode node = nodes[at];
            if (node.Operands.Count != node.Kernel.Reads) {
                return RasterFault.Device(key, $"<chain-operand-arity:{at}:{node.Kernel.Key}:{node.Operands.Count}!={node.Kernel.Reads}>");
            }
            if (node.Kernel.Reads + 1 > StorageBindingsPerStage) {
                return RasterFault.Device(key, $"<chain-step-bindings:{node.Kernel.Key}:{node.Kernel.Reads + 1}:{StorageBindingsPerStage}>");
            }
            foreach (int operand in node.Operands) {
                if (operand < 0 || operand >= at) { return RasterFault.Device(key, $"<chain-operand-order:{at}:{operand}>"); }
            }
        }

        // LIVE RANGES. A node's output dies at its LAST consumer's step; one pass over the operand relation raises
        // each producer's death, and a node nothing consumes dies at its own step. The terminal is the exception
        // the readback creates and the walk below excludes it by name.
        int[] dies = new int[count];
        for (int at = 0; at < count; at++) { dies[at] = at; }
        for (int at = 0; at < count; at++) {
            foreach (int operand in nodes[at].Operands) { dies[operand] = at; }
        }

        // RETIREMENT BUCKETS as an intrusive list — one head per step, one link per node — so the walk frees in
        // O(1) per node rather than re-scanning every predecessor at every step. That is the "linear" in linear
        // scan, and it allocates two int runs rather than a list per bucket.
        int[] head = new int[count], next = new int[count];
        Array.Fill(head, -1);
        for (int at = count - 1; at >= 0; at--) { next[at] = head[dies[at]]; head[dies[at]] = at; }

        // THE SCAN. Assign, then retire — never the reverse, because this step's operands are bound as reads in
        // the same dispatch that writes this step's output and one bind group may not alias a buffer as both.
        int[] slotOf = new int[count], free = new int[count];
        int freed = 0, minted = 0;
        for (int at = 0; at < count; at++) {
            slotOf[at] = freed > 0 ? free[--freed] : minted++;
            for (int retire = head[at]; retire >= 0; retire = next[retire]) {
                if (retire != count - 1) { free[freed++] = slotOf[retire]; }
            }
        }

        return Fin.Succ(new ChainPlan(
            toSeq(Enumerable.Range(0, count).Select(at =>
                new ChainStep(nodes[at].Kernel, nodes[at].Operands.Map(operand => slotOf[operand]), slotOf[at], nodes[at].Words))),
            minted, slotOf[count - 1]));
    }

    // The footprint gate a plan admission reads WITHOUT renting a device: the per-slot bound is the conformance
    // floor and the aggregate is the estate declaration, so both answer from declared numbers alone.
    public Fin<Unit> Admits(long texels, Op key) =>
        texels * BytesPerTexel > BindingFloor
            ? Fin.Fail<Unit>(RasterFault.Device(key, $"<chain-slot-over-binding-floor:{texels * BytesPerTexel}:{BindingFloor}>"))
            : Bytes(texels) > Footprint
                ? Fin.Fail<Unit>(RasterFault.Device(key, $"<chain-footprint:{Slots}:{Bytes(texels)}:{Footprint}>"))
                : Fin.Succ(unit);
}
```

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
namespace Rasm.Materials.Raster;

public sealed partial class PressDevice {
    // THE PLURAL DISPATCH. Same name, discriminating on the request's own shape: a KernelBinding is one kernel with
    // host buffers, a ChainPlan is an ordered chain whose intermediates stay device-resident. The pool allocates
    // ONCE at plane size, every step records into ONE encoder, and the whole chain submits once — so the
    // submission-index drain and the error scope [02] owns wrap a chain exactly as they wrap a single dispatch,
    // and the host pays one readback rather than one per node.
    public Fin<KernelReceipt> Dispatch(ChainPlan plan, Dimension width, Dimension height, Dimension layers, Op key) =>
        from _ in WgslOpCode.Total(key)
        from __ in plan.Admits((long)width.Value * height.Value * layers.Value, key)
        from pipelines in plan.Steps.Fold(Fin.Succ(Seq<nint>()), (acc, step) =>
            acc.Bind(built => Pipeline(step.Kernel, key).Map(built.Add)))
        from output in RunChain(plan, pipelines, width, height, layers, key)
        select output;

    // The pool is allocated per EXECUTION and released at its close, so a plan holds no device handle and outlives
    // any device. Each slot is one plane-sized storage buffer carrying both usages the chain needs — a step writes
    // its own slot and reads its operands' — and the terminal slot alone copies into the mapped readback, which is
    // the whole reason the intermediates never round-trip.
    Fin<KernelReceipt> RunChain(ChainPlan plan, Seq<nint> pipelines, Dimension width, Dimension height, Dimension layers, Op key) =>
        Pooled(plan, width, height, layers, key, (pool, encoder) =>
            plan.Steps.Fold(Fin.Succ(0), (acc, step) => acc.Bind(index =>
                Record(pool, encoder, pipelines[index], step, width, height, layers, key).Map(_ => index + 1))));

    // Pooled is the RESOURCE BOUNDARY and the one statement-shaped seam this cluster takes: it allocates the slot
    // pool and the readback buffer, pushes the error scope, opens ONE encoder, hands both to the body, then
    // finishes, submits once, drains on the submission index, copies the terminal slot out, pops the scope, and
    // releases every native handle on BOTH outcomes. Every failure arm passes through it, so a refused step
    // leaks no buffer and a device outlives no pool — the same bracket discipline `[02]`'s own `Run` holds, and
    // the reason the chain needs its own is that the pool's lifetime spans every step rather than one.
    Fin<KernelReceipt> Pooled(
        ChainPlan plan, Dimension width, Dimension height, Dimension layers, Op key,
        Func<Seq<nint>, nint, Fin<int>> body);

    // Record encodes ONE step against the already-open encoder: it writes the step's uniform words through the one
    // `KernelUniform` writer, mints a bind group over the pipeline's own auto-derived layout in the row's declared
    // binding order (uniform, then each read slot in operand order, then the write slot), sets the pipeline, and
    // dispatches at the row's own `Groups`. It submits nothing — the whole chain is one submission — which is what
    // keeps the step count off the host's round-trip cost.
    Fin<Unit> Record(
        Seq<nint> pool, nint encoder, nint pipeline, ChainStep step,
        Dimension width, Dimension height, Dimension layers, Op key);
}
```

## [06]-[RESEARCH]

(none)
