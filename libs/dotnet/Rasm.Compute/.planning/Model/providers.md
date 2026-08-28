# [COMPUTE_PROVIDERS]

`ExecutionProvider` rows select ONNX Runtime registration through host-gated device discovery and deterministic affinity over one frozen runtime snapshot. `ModelPrecision` owns execution posture as one `CapabilitySet<NumericTrait>` corner, `WarmForm` owns which warm-start mechanism a row reaches, and compiled-context compatibility answers a four-case `WarmVerdict` an EP-context warm start admits on `EP_SUPPORTED_OPTIMAL` alone.

`ExecutionProvider`, `ModelPrecision`, and `WarmForm` own discovery, registration, compatibility, resolution, warm-start form, and result identity. ONNX Runtime supplies provider members, the kernel `CapabilitySet`/`CapabilityLaw` pair carries numeric posture and its legal corners, `RosterFingerprint` keys behavior, and NodaTime owns negative-cache duration.

## [01]-[INDEX]

- [02]-[EP_AXIS]: execution-provider rows over one frozen runtime snapshot carrying the autoEP `OrtEpDevice` census beside its per-device veto rows, one polymorphic register threading the selected device ordinal on a `Fin` result, a typed two-step compatibility probe answering the four-case `WarmVerdict`, one numeric-posture capability set under its corner law, and the three-row warm-start form axis every row selects one of.

## [02]-[EP_AXIS]

- Owner: `ExecutionProvider` `[SmartEnum<string>]` rows (provider name, wire spelling, host gate, precision-keyed EP options, artifact-site location options, session keys, device-ordinal option key, device policy, hardware affinity, warm form, register delegate); `NumericTrait` the kernel-`ICapability` numeric vocabulary and `NumericPosture` its four legal corners beside the one `CapabilityLaw` built from them; `ModelPrecision` `[SmartEnum<string>]` rows (wire spelling, one posture corner, negative TTL, the posture-derived MatMulNBits key table); `WarmForm` `[SmartEnum<string>]` rows (artifact suffix, runtime-keyed identity, presence probe); `WarmVerdict` the four-case warm-start answer every consumer dispatches on; `WebGpuOption` and `OpenVinoDevice` the two admitted EP option vocabularies; `ProviderSnapshot` the once-materialized loaded-provider census, published-device census, per-provider `VetoRow` census, and the one device fingerprint every warm and result identity folds; `ArtifactSite` the directory-and-warm-path pair a row's location options read; the `Devices`/`AutoSelect`/`Vetoes`/`OptionsFor`/`Register`/`Compatible`/`Warmth`/`BindWarm`/`Resolve`/`FromWire`/`ResultKey` fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `Cuda`, `DirectMl`, `TensorRt`, `CoreMl`, `WebGpu`, `OpenVino`, `MiGraphX`, `Nnapi`, `Dnnl`; `ModelPrecision` rows `Full`, `Fp16`, `Bf16`, `Int8`, `Int4`; `NumericTrait` rows `narrow-accumulation`, `bfloat16-fast-math`, `quantized-graph` over the four `NumericPosture` corners `Native`, `Narrowed`, `FastMath`, `Quantized`; `WarmForm` rows `EpContext`, `OptimizedGraph`, `EngineCache`; `WarmVerdict` cases `Absent`, `Admissible`, `Recompile`, `NotApplicable`.
- Law: `Cpu` is the GUARANTEED FLOOR and every other row a POLICY THAT MAY REFUSE. `CPUExecutionProvider` ships with every runtime under a host gate that never closes, so `Resolve` degrades a key naming no row — or a row whose native provider the host never loaded — onto the floor rather than faulting a caller holding no alternative. `IsFloor` derives that identity instead of a column, so exactly one row answers it and no construction forks the answer.
- Law: each row carries its OWN wire spelling. `WireKey` is a row column and `FromWire` the one projection. Rows spelling `None` never cross, which keeps a posture demanding settled pre-quantized bytes unreachable from a wire carrying only an execution preference.
- Law: an unresolvable PROVIDER degrades and an unresolvable PRECISION refuses. `ExecutionProvider.FromWire` answers `Floor` for a spelling this roster cannot honour and the consuming record reports what ran; `ModelPrecision.FromWire` answers `None`, so the consumer refuses and the `CoreMl` row's `ModelFormat` pin carries the precision statement instead.
- Law: numeric posture is a CAPABILITY SET under a corner law, never adjacent bools. Three independent booleans left eight products representable where four name a posture a runtime executes, so the traits ride `CapabilitySet<NumericTrait>` and `NumericPosture.Law` legislates the four corners: a row claiming `Bfloat16FastMath` or `QuantizedGraph` while denying `NarrowAccumulation` claims a mechanism and refuses its own consequence, and the two accelerated mechanisms together name a graph that runs its MatMulNBits kernels through the MLAS arm64 bf16 GEMM path no runtime routes it down. NAMED LOSS: per-flag compile-time exhaustiveness — a reader now asks `Posture.Admits(row)` rather than reading a named column — bought back by the corner roster, which makes every illegal product unreachable rather than merely untested, and by `Rostered`, the one gate `Register` threads so a hand-built set refuses before any native append. WITNESS: `CpuSessionKeys`'s `mlas.enable_gemm_fastmath_arm64_bfloat16` write and `CoreMlOptions`'s `AllowLowPrecisionAccumulationOnGPU` write each read one trait off the set the row declared.
- Law: the MatMulNBits accuracy floor rides the QUANTIZED CORNER, never a per-row column. Both quantized rows carried the same literal `4` — fp32 accumulation over the dequantized block — so an `Option<int>` column beside the trait was a hand-kept mirror of the trait itself, and `QuantizedGraph` was a column no surface read. `QdqKeys` now derives from the corner and computes ONCE at construction. NAMED LOSS: a per-row accuracy floor; a row needing another floor lands its own `NumericTrait` row and its own corner, never a fourth column. WITNESS: `Int8` and `Int4` produce byte-identical `QdqKeys` today, which is what a per-row column was spelling twice.
- Law: EVERY row carries a warm-start form and the floor row is not exempt. `WarmForm.EpContext` is the compiled-partition mechanism — a blob reloaded through the `ep.context_*` keys whose admissibility its own embedded compat info answers; `WarmForm.OptimizedGraph` is the managed mechanism `Cpu` and `Dnnl` take, where ORT writes the post-optimization graph through `OptimizedModelFilePath` and a later cold open loads THAT graph at `ORT_DISABLE_ALL`; `WarmForm.EngineCache` is `TensorRt`'s, whose warm start is the EP's OWN engine cache — `trt_engine_cache_enable` plus `trt_engine_cache_path` arm it at registration and the EP writes and reads that directory itself, so the artifact is a directory rather than a file and its presence probe reads one. Without the managed row the guaranteed floor — the row every degrade lands on and every parity probe runs — re-pays `ORT_ENABLE_ALL` from source on every cold open forever, and `Compatible` answers `EP_NOT_APPLICABLE` for it by construction, so no compat verdict opens that door. `Suffix`, `RuntimeKeyed`, and `Present` keep all three forms inside the ONE artifact-key derivation: the compat-info-bearing blob needs no runtime column, the managed graph and the engine cache carry none and take one, so an artifact another runtime wrote misses its key rather than loading.
- Law: a row is admitted by EXECUTION, never by inventory. `ROCMExecutionProvider` carries no row here — the EP left the runtime's source tree, and its managed surface (both append overloads, the options capsule, the one-call shorthand) survives only as ABI-stable stubs whose native entry points unconditionally answer that the provider is not enabled in this build. A row over them would publish an accelerated route that executes nothing while `Available` answered false forever, which is precisely the inventory-versus-execution defect this folder already settled; AMD hardware reaches this roster through `MiGraphX`, the migration the runtime itself names.
- Law: the generic string append is a CLOSED native roster, and it decides each row's registration family. `AppendExecutionProvider(name, options)` marshals the name straight to native, where a fixed table admits a short or canonical spelling per EP and refuses everything else with `ORT_INVALID_ARGUMENT` — `CoreMLExecutionProvider`, `WebGPU`, `OpenVINO`, `DML`, `MIGraphX`, and `CPU` are on it; `CUDAExecutionProvider` and `TensorrtExecutionProvider` are NOT. Rows therefore split three ways and the split is measured, not stylistic: option-bag rows take the string append, `Cuda` and `TensorRt` take their `OrtCUDAProviderOptions`/`OrtTensorRTProviderOptions` `UpdateOptions` capsule and the typed append (their only dictionary path), and `Cpu`, `Nnapi`, and `Dnnl` take the typed scalar member their EP publishes. Spelling a name the table refuses fails at the append rather than at the run, so a row is admitted to the string family by its NAME being on that table and by nothing else.
- Law: accelerated rows answer to the floor by MEASUREMENT, and the residual band is an OBSERVATION. Every non-floor row produces a result whose residual against a floor-provider run over the same input is measured at the run and reported outward, because a provider drawing its speed from lower internal precision degrades silently and by construction raises nothing. The band a model family reaches is therefore the `Model/stage#PARITY` canary's own record at that family's first execution on this host — carried on the parity artifact beside the verdict it gated — never a constant this axis asserts: a spec-side tolerance grades every family against whichever one happened to be measured when it was written. This axis declares the obligation; the consumer owning admission performs the comparison and owns the tolerance.
- Law: every native leg answers on the `Fin` result and NONE classifies. `Register` and `Compatible` both cross a boundary that raises — a refused option key, a refused provider name, an unreadable compat artifact — so each returns `Fin` carrying the raw error, and `Model/sessions#SESSION_CAPSULE` `ModelSessions.Faulted` alone turns it into a typed `ComputeFault`. A `void` register and an `Option` compat probe made absence and failure one answer at the boundary that knows the difference, and a second classifier here would report one cancellation as two faults.
- Law: a row this host cannot claim refuses BY NAME with the veto that closed it. `ProviderSnapshot` folds the per-provider `VetoRow` census — hardware device, `OrtDeviceEpIncompatibilityReason` bitmask, notes, error code — into the SAME freeze as the device census, because `GetHardwareDevices` is exactly as immutable as `GetEpDevices` and a property re-reading it per call contradicted this axis's own freeze law. `Register` reads it: a row publishing no device while every hardware device vetoes it refuses `SubstrateUnavailable` naming the driver, device, or dependency, rather than failing at the native append with a message naming nothing.
- Law: the SELECTED DEVICE is part of result identity. `AutoSelect` ranks by affinity, so two adapters on one host run different silicon under one provider key — and a provider drawing speed from lower internal precision produces a different residual per adapter. `ResultKey` therefore folds the chosen device's fingerprint, the same `ProviderSnapshot.Fingerprint` derivation `Model/sessions#SESSION_CAPSULE` folds into `ContextKey` and the allocator lease, so a dual-GPU host cannot publish one `ModelResultKey` or one `ParityKey` for two devices. Device identity seats HERE beside the census that mints it, never at the session capsule that consumes it.
- Auto: `ProviderSnapshot` freezes the loaded-provider set, the published `OrtEpDevice` census, and the per-provider veto census ONCE behind a lazy accessor, and `Available`/`Devices`/`Vetoes`/`Resolve`/`FromWire` read that frozen census — `Available` still short-circuits on `HostGate` first. `FromWire` scans the row roster rather than caching a frozen inverse, because a static table folded from `Items` beside the row fields reads an empty roster whenever its initializer wins the ordering race — and a ten-row ordinal scan disappears beside the session lease it precedes. `AutoSelect` ranks the snapshot's devices by row-owned `HardwareAffinity`, then CPU last, then provider/vendor/device identity for deterministic ties. One selected-device snapshot passes through `Register`, `Compatible`, and `Warmth`. `Register` folds session keys and precision `QdqKeys`, composes row-owned EP and `ArtifactSite` location option tables, writes the SELECTED device's ordinal under the row's own `OrdinalKey`, then uses direct autoEP registration when the snapshot is non-empty or the row's verified fallback registration otherwise — measured at the pin: `CoreMl` publishes NO `OrtEpDevice`, so its snapshot is empty BY CONSTRUCTION and the fallback arm is its only reachable path, while `WebGpu` is the macOS row that publishes a GPU device and takes the autoEP arm; `EpOptions` is EMPTY on every published device, so no row inherits discovery defaults and the row-owned option table is the sole source. Location options are row-owned and disjoint: `CoreMl` alone contributes `ModelCacheDirectory` and `TensorRt` alone `trt_engine_cache_path`. `Compatible` runs the two-step probe over the same snapshot against the compiled artifact's embedded compat info. `Warmth` proves the artifact present through the FORM's own probe — a file for the blob and managed rows, a directory for the engine cache — then dispatches on that form onto a `WarmVerdict` case: the EP-context arm answers `Admissible` on `EP_SUPPORTED_OPTIMAL` alone, `Recompile` carrying the verdict for `EP_UNSUPPORTED` and `EP_SUPPORTED_PREFER_RECOMPILATION`, and `NotApplicable` where the row published no device to ask; the managed and engine-cache arms answer `Admissible` on presence, their runtime and behavior columns having keyed the artifact name. `BindWarm` takes that verdict and writes each form's own session state under one call: the `ep.context_*` triple for the blob rows, `OptimizedModelFilePath` for the managed rows on a MISS alone (an admissible graph is the open's own load source), and NOTHING for the engine cache, whose arming already rode the EP option bag. `Resolve` reads the row roster by key, proves the native provider loaded through `Available`, and answers `Floor` otherwise. `ResultKey` stamps provider, runtime, precision, the shared behavior-option fingerprint, and the selected device's fingerprint.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Domain.ICapability`/`CapabilitySet`/`CapabilityLaw`), BCL inbox
- Growth: a built-in accelerator is one `ExecutionProvider` row minted through `Accelerator` with its provider name, OS gate, `HardwareAffinity`, EP-option and device-ordinal columns, and register delegate, its EP-context warm form implied by the factory; a row whose EP compiles no context partition declares through the constructor instead and states its own `WarmForm` — including an accelerator whose native provider this platform's runtime may not carry, because `Available` and `Resolve` already answer absence and no caller-facing surface moves. That row reaches every wire the moment it declares a `WireKey`; before it declares one, `FromWire` degrades the spelling it answers to onto the floor exactly as it degrades an unloaded provider. An OUT-OF-TREE accelerator is one row on this same roster and nothing else: its library registers once at composition through `OrtEnv.RegisterExecutionProviderLibrary`, its devices then publish into the snapshot like any built-in EP's, and its register delegate takes the autoEP device arm — the string-append name table is closed native vocabulary no out-of-tree name enters, and a row minted at RUNTIME never joins `Items`, so `TryGet`, `Resolve`, and `FromWire` could never answer it and every wire and result naming it read the floor instead. Its library bytes still reach identity: `Model/sessions#SESSION_CAPSULE` `DeviceFingerprint` folds the device's own `EpMetadata` and `EpOptions` into the resident and warm-artifact keys, so a library version change re-keys both. Each quantization posture is one `ModelPrecision` row naming a `NumericPosture` corner, folded into the same registration and fingerprint pipelines; a new numeric mechanism is one `NumericTrait` row plus the corners it legally joins, and every row that cannot hold it keeps refusing at `NumericPosture.Law` with no reader edited; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on `AutoSelect`; a pinned WebGPU or OpenVINO option is one `WebGpuOption`/`OpenVinoDevice` row the same admitted fold already reads; a fourth warm-start mechanism is one `WarmForm` row with its suffix, runtime-keying column, presence probe, and the arms `Warmth` and `BindWarm` then demand — the artifact key, the blob lane, and the retention row all follow it with no edit; a fifth warm ANSWER is one `WarmVerdict` case whose consumers break at compile time rather than reading a bit that erased it.
- Boundary: each row owns one provider-specific fallback registration and one autoEP device registration path selected by a caller-held device snapshot, so one lease appends one provider. `CoreMl` and `WebGpu` use the generic `AppendExecutionProvider(name, options)` spelling for one measured reason: neither carries a dedicated managed method nor an exported C append entry (the pinned dylib exports exactly `_CPU` and `_CoreML` beside `OrtGetApiBase`, and no `_WebGPU`), so the string append is their only path — `CoreMl` because its row owns `ModelFormat`, compute units, specialization, cache directory, and precision beyond the flags overload, `WebGpu` because the EP has no other spelling at all; the `CoreMl` flags overload never runs beside its row. `OpenVino`, `DirectMl`, and `MiGraphX` take that same spelling because it is their ONLY managed dictionary path — none publishes an options capsule, and each typed member takes a bare scalar that cannot carry an option bag at all. The DEVICE ORDINAL rides that bag as the row's own `OrdinalKey` rather than a registration literal: `AutoSelect` ranks by affinity, so binding device zero would run the graph on whichever adapter the driver enumerated first while every result named the device the rank chose; the ordinal is the device's index within THIS row's frozen census, which is why the census must be frozen — a re-enumeration hands back fresh instances a selected device can never be located in again. `WireKey` stays out of `OptionsFor` and out of `ResultKey`: it names the row for a boundary record and reaches nothing the built session does, so a rename changing no execution re-keys no cached result. Location options affect native artifact placement but stay out of result identity, while EP/session/precision options enter `OptionsFor`. `HostGate` expresses row-specific OS capability while the frozen loaded-provider set proves the native provider. `Warm` stays out of `OptionsFor` and out of `ResultKey`: it selects which artifact a cold open reads, never what the built graph computes, so a form change re-keys the warm artifact through `Suffix` and leaves every cached result standing. Rows reference `WarmForm` fields across a type boundary, so the roster-race the `FromWire` scan guards against — a same-class static folded from `Items` beside the row initializers — cannot arise here. `Full` holds `NumericPosture.Native` and leaves `mlas.enable_gemm_fastmath_arm64_bfloat16` disabled; `Bf16` alone holds `NumericTrait.Bfloat16FastMath` and sets it. The posture also reaches CoreML low-precision accumulation and the MatMulNBits accuracy floor, and every behavior option participates in `RosterFingerprint.Of`. Compatibility consumes `OrtCompiledModelCompatibility` directly and admits EP-context reuse only for an existing `EP_SUPPORTED_OPTIMAL` artifact. `OrtEpDevice`, `OrtHardwareDevice`, and the `OrtDeviceEpIncompatibilityReason` bitmask are catalogued at `.api/api-onnxruntime.md` and read from there — a member table restated on this page is a second owner for a fact the catalogue already holds.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NumericTrait : ICapability<NumericTrait> {
    public static readonly NumericTrait NarrowAccumulation = new("narrow-accumulation");
    public static readonly NumericTrait Bfloat16FastMath = new("bfloat16-fast-math");
    public static readonly NumericTrait QuantizedGraph = new("quantized-graph");
}

public static class NumericPosture {
    static CapabilitySet<NumericTrait> Of(params ReadOnlySpan<NumericTrait> held) => CapabilitySet<NumericTrait>.Of(held);

    public static readonly CapabilitySet<NumericTrait> Native = CapabilitySet<NumericTrait>.None;
    public static readonly CapabilitySet<NumericTrait> Narrowed = Of(NumericTrait.NarrowAccumulation);
    public static readonly CapabilitySet<NumericTrait> FastMath = Of(NumericTrait.NarrowAccumulation, NumericTrait.Bfloat16FastMath);
    public static readonly CapabilitySet<NumericTrait> Quantized = Of(NumericTrait.NarrowAccumulation, NumericTrait.QuantizedGraph);

    public static readonly CapabilityLaw<NumericTrait> Law = new(Seq(Native, Narrowed, FastMath, Quantized));

    public const int MatMulNBitsFloor = 4;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WarmForm {
    public static readonly WarmForm EpContext = new("ep-context", "ctx.onnx", false, File.Exists);
    public static readonly WarmForm OptimizedGraph = new("optimized-graph", "opt.onnx", true, File.Exists);
    public static readonly WarmForm EngineCache = new("engine-cache", "trt.cache", true, Directory.Exists);

    public string Suffix { get; }

    public bool RuntimeKeyed { get; }

    [UseDelegateFromConstructor]
    public partial bool Present(string path);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WarmVerdict(string Path) {
    public sealed record Absent(string Path) : WarmVerdict(Path);

    public sealed record Admissible(string Path, Option<OrtCompiledModelCompatibility> Compat) : WarmVerdict(Path);

    public sealed record Recompile(string Path, OrtCompiledModelCompatibility Compat) : WarmVerdict(Path);

    public sealed record NotApplicable(string Path) : WarmVerdict(Path);

    public bool Bound => Switch(
        absent: static _ => false,
        admissible: static _ => true,
        recompile: static _ => false,
        notApplicable: static _ => false);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WebGpuOption {
    static FrozenSet<string> Values(params ReadOnlySpan<string> admitted) => admitted.ToArray().ToFrozenSet(StringComparer.Ordinal);
    static readonly FrozenSet<string> Toggle = Values("0", "1");
    static readonly FrozenSet<string> CacheMode = Values("disabled", "lazyRelease", "simple", "bucket");

    public static readonly WebGpuOption PowerPreference = new("powerPreference", Values("high-performance", "low-power"));
    public static readonly WebGpuOption PreferredLayout = new("preferredLayout", Values("NCHW", "NHWC"));
    public static readonly WebGpuOption ValidationMode = new("validationMode", Values("disabled", "wgpuOnly", "basic", "full"));
    public static readonly WebGpuOption StorageBufferCacheMode = new("storageBufferCacheMode", CacheMode);
    public static readonly WebGpuOption UniformBufferCacheMode = new("uniformBufferCacheMode", CacheMode);
    public static readonly WebGpuOption QueryResolveBufferCacheMode = new("queryResolveBufferCacheMode", CacheMode);
    public static readonly WebGpuOption DefaultBufferCacheMode = new("defaultBufferCacheMode", CacheMode);
    public static readonly WebGpuOption EnableGraphCapture = new("enableGraphCapture", Toggle);
    public static readonly WebGpuOption EnableInt64 = new("enableInt64", Toggle);
    public static readonly WebGpuOption PreserveDevice = new("preserveDevice", Toggle);
    public static readonly WebGpuOption MaxStorageBufferBindingSize = new("maxStorageBufferBindingSize", FrozenSet<string>.Empty);

    public FrozenSet<string> Admitted { get; }

    public Fin<KeyValuePair<string, string>> Pin(string value) =>
        Admitted.Count is 0 || Admitted.Contains(value)
            ? Fin.Succ(new KeyValuePair<string, string>(Key, value))
            : Fin.Fail<KeyValuePair<string, string>>(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Rostered, new ContractEvidence.Keys(Key, value))));

    public static Fin<FrozenDictionary<string, string>> Pins(Seq<(WebGpuOption Option, string Value)> pins) =>
        pins.Traverse(static pin => pin.Option.Pin(pin.Value).ToValidation()).As().ToFin()
            .Map(static admitted => admitted.ToFrozenDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpenVinoDevice {
    public static readonly OpenVinoDevice Auto = new("AUTO");
    public static readonly OpenVinoDevice Cpu = new("CPU");
    public static readonly OpenVinoDevice Gpu = new("GPU");
    public static readonly OpenVinoDevice Npu = new("NPU");
    public static readonly OpenVinoDevice Hetero = new("HETERO");
    public static readonly OpenVinoDevice Multi = new("MULTI");
}

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelPrecision {
    public static readonly ModelPrecision Full = new("full", wireKey: "fp32", posture: NumericPosture.Native, negativeTtl: Duration.FromMinutes(15));
    public static readonly ModelPrecision Fp16 = new("fp16", wireKey: "fp16", posture: NumericPosture.Narrowed, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Bf16 = new("bf16", wireKey: Option<string>.None, posture: NumericPosture.FastMath, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Int8 = new("int8", wireKey: Option<string>.None, posture: NumericPosture.Quantized, negativeTtl: Duration.FromMinutes(5));
    public static readonly ModelPrecision Int4 = new("int4", wireKey: Option<string>.None, posture: NumericPosture.Quantized, negativeTtl: Duration.FromMinutes(2));

    private ModelPrecision(string key, Option<string> wireKey, CapabilitySet<NumericTrait> posture, Duration negativeTtl) : this(key) =>
        (WireKey, Posture, NegativeTtl, QdqKeys) = (wireKey, posture, negativeTtl, Qdq(posture));

    public Option<string> WireKey { get; }
    public CapabilitySet<NumericTrait> Posture { get; }

    public Duration NegativeTtl { get; }

    public FrozenDictionary<string, string> QdqKeys { get; }

    public static Fin<Unit> Rostered => Legal.Value;

    static readonly Lazy<Fin<Unit>> Legal = new(
        static () => toSeq(Items)
            .Traverse(static row => NumericPosture.Law.Admit(row.Posture).ToValidation())
            .As().ToFin().Map(static _ => unit),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Option<ModelPrecision> FromWire(string wire) =>
        toSeq(Items).Find(row => row.WireKey.Exists(key => StringComparer.Ordinal.Equals(wire)));

    static FrozenDictionary<string, string> Qdq(CapabilitySet<NumericTrait> posture) =>
        posture.Admits(NumericTrait.QuantizedGraph)
            ? new Dictionary<string, string>(StringComparer.Ordinal) {
                ["session.qdq_matmulnbits_accuracy_level"] = NumericPosture.MatMulNBitsFloor.ToString(CultureInfo.InvariantCulture),
            }.ToFrozenDictionary(StringComparer.Ordinal)
            : FrozenDictionary<string, string>.Empty;
}

public readonly record struct VetoRow(OrtHardwareDeviceType Device, OrtDeviceEpIncompatibilityReason Reason, string Notes, int Code);

public sealed record ProviderSnapshot(
    FrozenSet<string> Loaded,
    FrozenDictionary<string, Seq<OrtEpDevice>> Published,
    FrozenDictionary<string, Seq<VetoRow>> Vetoed) {
    public static ProviderSnapshot Of(OrtEnv env, Seq<string> providers) {
        Seq<OrtHardwareDevice> hardware = toSeq(env.GetHardwareDevices());
        return new(
            env.GetAvailableProviders().ToFrozenSet(StringComparer.Ordinal),
            toSeq(env.GetEpDevices())
                .GroupBy(static device => device.EpName, StringComparer.Ordinal)
                .ToFrozenDictionary(static group => group.Key, static group => toSeq(group), StringComparer.Ordinal),
            providers.ToFrozenDictionary(
                static provider => provider,
                provider => hardware.Map(device => {
                    using OrtDeviceEpIncompatibilityDetails details = env.GetHardwareDeviceEpIncompatibilityDetails(provider, device);
                    return new VetoRow(device.Type, details.ReasonsBitmask, details.Notes, details.ErrorCode);
                }).Filter(static row => row.Reason != OrtDeviceEpIncompatibilityReason.None),
                StringComparer.Ordinal));
    }

    public Seq<OrtEpDevice> For(string providerName) =>
        Published.TryGetValue(providerName, out Seq<OrtEpDevice> devices) ? devices : Seq<OrtEpDevice>();

    public Seq<VetoRow> VetoesFor(string providerName) =>
        Vetoed.TryGetValue(providerName, out Seq<VetoRow> rows) ? rows : Seq<VetoRow>();

    public static ulong Fingerprint(OrtEpDevice device) => RosterFingerprint.Of(
        new KeyValuePair<string, string>[] {
            new("ep", device.EpName),
            new("ep-vendor", device.EpVendor),
            new("hardware-vendor-id", device.HardwareDevice.VendorId.ToString(CultureInfo.InvariantCulture)),
            new("hardware-vendor", device.HardwareDevice.Vendor),
            new("hardware-device", device.HardwareDevice.DeviceId.ToString(CultureInfo.InvariantCulture)),
            new("hardware-type", ((int)device.HardwareDevice.Type).ToString(CultureInfo.InvariantCulture)),
        }
        .Concat(device.EpMetadata.Entries.Select(static row => new KeyValuePair<string, string>($"ep-meta:{row.Key}", row.Value)))
        .Concat(device.EpOptions.Entries.Select(static row => new KeyValuePair<string, string>($"ep-option:{row.Key}", row.Value)))
        .Concat(device.HardwareDevice.Metadata.Entries.Select(static row => new KeyValuePair<string, string>($"hardware-meta:{row.Key}", row.Value))));
}

public readonly record struct ArtifactSite(string Directory, string WarmPath);

// --- [SERVICES] ------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExecutionProvider {
    static readonly Lazy<ProviderSnapshot> Frozen = new(
        static () => ProviderSnapshot.Of(OrtEnv.Instance(), toSeq(Items).Map(static row => row.ProviderName)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ProviderSnapshot Snapshot => Frozen.Value;

    static readonly FrozenDictionary<string, string> CoreMlRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["ModelFormat"] = "MLProgram",
        ["MLComputeUnits"] = "ALL",
        ["RequireStaticInputShapes"] = "0",
        ["EnableOnSubgraphs"] = "0",
        ["SpecializationStrategy"] = "Default",
        ["ProfileComputePlan"] = "0",
        ["AllowLowPrecisionAccumulationOnGPU"] = "0",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static FrozenDictionary<string, string> CpuSessionKeys(ModelPrecision precision) =>
        new Dictionary<string, string>(StringComparer.Ordinal) {
            ["mlas.enable_gemm_fastmath_arm64_bfloat16"] = precision.Posture.Admits(NumericTrait.Bfloat16FastMath) ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static FrozenDictionary<string, string> CoreMlOptions(ModelPrecision precision) =>
        new Dictionary<string, string>(CoreMlRows, StringComparer.Ordinal) {
            ["AllowLowPrecisionAccumulationOnGPU"] = precision.Posture.Admits(NumericTrait.NarrowAccumulation) ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> TensorRtRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["trt_engine_cache_enable"] = "1",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> OpenVinoRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["device_type"] = OpenVinoDevice.Auto.Key,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly Lazy<FrozenDictionary<string, string>> WebGpuRows = new(
        static () => WebGpuOption.Pins(Seq<(WebGpuOption, string)>()).IfFail(static _ => FrozenDictionary<string, string>.Empty),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static readonly ExecutionProvider Cpu = new(
        "cpu", providerName: "CPUExecutionProvider", wireKey: "cpu", hostGate: static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, locationOptions: static _ => FrozenDictionary<string, string>.Empty,
        sessionKeys: CpuSessionKeys, ordinalKey: Option<string>.None,
        devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        warm: WarmForm.OptimizedGraph,
        registerRow: static (options, _) => options.AppendExecutionProvider_CPU(1));

    public static readonly ExecutionProvider Cuda = Accelerator(
        "cuda", "CUDAExecutionProvider", OrtHardwareDeviceType.GPU, static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => {
            using OrtCUDAProviderOptions cuda = new();
            cuda.UpdateOptions(rows);
            options.AppendExecutionProvider_CUDA(cuda);
        });

    public static readonly ExecutionProvider DirectMl = Accelerator(
        "directml", "DmlExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsWindows,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => options.AppendExecutionProvider("DML", rows));

    public static readonly ExecutionProvider TensorRt = new(
        "tensorrt", providerName: "TensorrtExecutionProvider", wireKey: Option<string>.None, hostGate: static () => true,
        epOptions: static _ => TensorRtRows,
        locationOptions: static site => new Dictionary<string, string>(StringComparer.Ordinal) {
            ["trt_engine_cache_path"] = site.WarmPath,
        }.ToFrozenDictionary(StringComparer.Ordinal),
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.GPU,
        warm: WarmForm.EngineCache,
        registerRow: static (options, rows) => {
            using OrtTensorRTProviderOptions tensorRt = new();
            tensorRt.UpdateOptions(rows);
            options.AppendExecutionProvider_Tensorrt(tensorRt);
        });

    public static readonly ExecutionProvider CoreMl = Accelerator(
        "coreml", "CoreMLExecutionProvider", OrtHardwareDeviceType.NPU,
        static () => OperatingSystem.IsMacOSVersionAtLeast(12),
        epOptions: CoreMlOptions,
        register: static (options, rows) => options.AppendExecutionProvider("CoreMLExecutionProvider", rows),
        wireKey: "coreMl",
        devicePolicy: ExecutionProviderDevicePolicy.PREFER_NPU,
        locationOptions: static site => new Dictionary<string, string>(StringComparer.Ordinal) { ["ModelCacheDirectory"] = site.Directory }.ToFrozenDictionary(StringComparer.Ordinal));

    public static readonly ExecutionProvider WebGpu = Accelerator(
        "webgpu", "WebGpuExecutionProvider", OrtHardwareDeviceType.GPU,
        static () => OperatingSystem.IsMacOS(),
        epOptions: static _ => WebGpuRows.Value,
        register: static (options, rows) => options.AppendExecutionProvider("WebGPU", rows),
        wireKey: "webGpu",
        devicePolicy: ExecutionProviderDevicePolicy.PREFER_GPU);

    public static readonly ExecutionProvider OpenVino = Accelerator(
        "openvino", "OpenVINOExecutionProvider", OrtHardwareDeviceType.NPU, static () => true,
        epOptions: static _ => OpenVinoRows,
        register: static (options, rows) => options.AppendExecutionProvider("OpenVINO", rows));

    public static readonly ExecutionProvider MiGraphX = Accelerator(
        "migraphx", "MIGraphXExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsLinux,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => options.AppendExecutionProvider("MIGraphX", rows));

    public static readonly ExecutionProvider Nnapi = Accelerator(
        "nnapi", "NnapiExecutionProvider", OrtHardwareDeviceType.NPU, OperatingSystem.IsAndroid,
        epOptions: static _ => FrozenDictionary<string, string>.Empty,
        register: static (options, _) => options.AppendExecutionProvider_Nnapi(NnapiFlags.NNAPI_FLAG_USE_NONE));

    public static readonly ExecutionProvider Dnnl = new(
        "dnnl", providerName: "DnnlExecutionProvider", wireKey: Option<string>.None, hostGate: static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty,
        locationOptions: static _ => FrozenDictionary<string, string>.Empty,
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: Option<string>.None,
        devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        warm: WarmForm.OptimizedGraph,
        registerRow: static (options, _) => options.AppendExecutionProvider_Dnnl(1));

    private ExecutionProvider(
        string key, string providerName, Option<string> wireKey, Func<bool> hostGate,
        Func<ModelPrecision, FrozenDictionary<string, string>> epOptions,
        Func<ArtifactSite, FrozenDictionary<string, string>> locationOptions,
        Func<ModelPrecision, FrozenDictionary<string, string>> sessionKeys,
        Option<string> ordinalKey,
        Option<ExecutionProviderDevicePolicy> devicePolicy, OrtHardwareDeviceType hardwareAffinity, WarmForm warm,
        Action<SessionOptions, Dictionary<string, string>> registerRow) : this(key) {
        (ProviderName, WireKey, HostGate, EpOptions, LocationOptions, SessionKeys, OrdinalKey, DevicePolicy, HardwareAffinity, Warm, RegisterRow) =
            (providerName, wireKey, hostGate, epOptions, locationOptions, sessionKeys, ordinalKey, devicePolicy, hardwareAffinity, warm, registerRow);
        ranked = new(() => toSeq(Devices
            .OrderByDescending(device => Rank(device.HardwareDevice.Type, HardwareAffinity))
            .ThenBy(static device => device.EpName, StringComparer.Ordinal)
            .ThenBy(static device => device.EpVendor, StringComparer.Ordinal)
            .ThenBy(static device => device.HardwareDevice.VendorId)
            .ThenBy(static device => device.HardwareDevice.DeviceId)), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    readonly Lazy<Seq<OrtEpDevice>> ranked;

    public string ProviderName { get; }
    public Option<string> WireKey { get; }
    public Func<bool> HostGate { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> EpOptions { get; }
    public Func<ArtifactSite, FrozenDictionary<string, string>> LocationOptions { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> SessionKeys { get; }

    public Option<string> OrdinalKey { get; }

    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public OrtHardwareDeviceType HardwareAffinity { get; }
    public WarmForm Warm { get; }
    public Action<SessionOptions, Dictionary<string, string>> RegisterRow { get; }

    public bool Available => HostGate() && Snapshot.Loaded.Contains(ProviderName);

    public bool IsFloor => ReferenceEquals(this, Cpu);

    public static ExecutionProvider Floor => Cpu;

    public static ExecutionProvider Resolve(string key) =>
        TryGet(key, out ExecutionProvider? row) && row.Available ? row : Floor;

    public static ExecutionProvider FromWire(string wire) =>
        toSeq(Items)
            .Find(row => row.WireKey.Exists(key => StringComparer.Ordinal.Equals(wire)))
            .Match(Some: static row => Resolve(row.Key), None: static () => Floor);

    public string ReportKey => WireKey.IfNone(Key);

    public Seq<OrtEpDevice> Devices => Snapshot.For(ProviderName);

    public Seq<OrtEpDevice> AutoSelect => ranked.Value;

    public Seq<VetoRow> Vetoes => Snapshot.VetoesFor(ProviderName);

    static int Rank(OrtHardwareDeviceType device, OrtHardwareDeviceType affinity) =>
        (Affinity: device == affinity, Cpu: device == OrtHardwareDeviceType.CPU) switch {
            (true, _) => 2,
            (_, true) => 0,
            _ => 1,
        };

    public FrozenDictionary<string, string> OptionsFor(ModelPrecision precision) =>
        EpOptions(precision).Concat(SessionKeys(precision)).Concat(precision.QdqKeys)
            .ToFrozenDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal);

    public Fin<SessionOptions> Register(SessionOptions options, ArtifactSite artifacts, ModelPrecision precision, Seq<OrtEpDevice> devices) =>
        ModelPrecision.Rostered
            .Bind(_ => guard(
                Vetoes.IsEmpty || !Devices.IsEmpty,
                (Error)new ComputeFault.SubstrateUnavailable(
                    $"<ep-vetoed:{Key}:{string.Join(';', Vetoes.Map(static row => $"{row.Device}={row.Reason}:{row.Code}"))}>")).ToFin())
            .Bind(_ => Try.lift(() => {
                toSeq(SessionKeys(precision).Concat(precision.QdqKeys)).Iter(entry => options.AddSessionConfigEntry(entry.Key, entry.Value));
                Dictionary<string, string> registerOptions = Registered(precision, artifacts, devices);
                if (devices.IsEmpty) { RegisterRow(options, registerOptions); }
                else { options.AppendExecutionProvider(OrtEnv.Instance(), devices.ToList(), registerOptions); }
                return Fin.Succ(options);
            }).Run().Bind(static inner => inner));

    Dictionary<string, string> Registered(ModelPrecision precision, ArtifactSite artifacts, Seq<OrtEpDevice> devices) =>
        toHashMap(EpOptions(precision))
            .AddOrUpdateRange(LocationOptions(artifacts))
            .AddOrUpdateRange(OrdinalKey.Bind(key => Ordinal(devices)
                .Map(ordinal => (key, ordinal.ToString(CultureInfo.InvariantCulture))))
                .ToSeq())
            .ToDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal);

    Option<int> Ordinal(Seq<OrtEpDevice> devices) =>
        devices.Head.Bind(chosen => Devices
            .Map(static (device, index) => (Device: device, Index: index))
            .Find(row => ReferenceEquals(row.Device, chosen))
            .Map(static row => row.Index));

    public Fin<Option<OrtCompiledModelCompatibility>> Compatible(string compiledModelPath, Seq<OrtEpDevice> devices) =>
        devices.IsEmpty
            ? Fin.Succ(Option<OrtCompiledModelCompatibility>.None)
            : Try.lift(() => Some(OrtEnv.Instance().GetModelCompatibilityForEpDevices(
                devices.ToList(), OrtEnv.Instance().GetCompatibilityInfoFromModel(compiledModelPath, ProviderName)))).Run();

    public Fin<WarmVerdict> Warmth(string warmPath, Seq<OrtEpDevice> devices) =>
        !Warm.Present(warmPath)
            ? Fin.Succ<WarmVerdict>(new WarmVerdict.Absent(warmPath))
            : Warm.Switch(
                state: (Row: this, Path: warmPath, Devices: devices),
                epContext: static probe => probe.Row.Compatible(probe.Path, probe.Devices).Map(compat => compat.Match(
                    Some: held => held == OrtCompiledModelCompatibility.EP_SUPPORTED_OPTIMAL
                        ? (WarmVerdict)new WarmVerdict.Admissible(probe.Path, Some(held))
                        : new WarmVerdict.Recompile(probe.Path, held),
                    None: () => (WarmVerdict)new WarmVerdict.NotApplicable(probe.Path))),
                optimizedGraph: static probe => Fin.Succ<WarmVerdict>(new WarmVerdict.Admissible(probe.Path, None)),
                engineCache: static probe => Fin.Succ<WarmVerdict>(new WarmVerdict.Admissible(probe.Path, None)));

    public void BindWarm(SessionOptions options, WarmVerdict verdict) =>
        Warm.Switch(
            state: (Options: options, Verdict: verdict),
            epContext: static site => {
                site.Options.AddSessionConfigEntry("ep.context_enable", site.Verdict.Bound ? "1" : "0");
                site.Options.AddSessionConfigEntry("ep.context_file_path", site.Verdict.Path);
                site.Options.AddSessionConfigEntry("ep.share_ep_contexts", "1");
            },
            optimizedGraph: static site => {
                if (!site.Verdict.Bound) { site.Options.OptimizedModelFilePath = site.Verdict.Path; }
            },
            engineCache: static _ => { });

    public string ResultKey(string ortVersion, ModelPrecision precision, Option<OrtEpDevice> device) =>
        $"{Key}:{ortVersion}:{precision.Key}:{RosterFingerprint.Of(OptionsFor(precision)):x16}"
        + device.Match(Some: static held => $":{ProviderSnapshot.Fingerprint(held):x16}", None: static () => string.Empty);

    static ExecutionProvider Accelerator(
        string key,
        string providerName,
        OrtHardwareDeviceType affinity,
        Func<bool> hostGate,
        Func<ModelPrecision, FrozenDictionary<string, string>> epOptions,
        Action<SessionOptions, Dictionary<string, string>> register,
        Option<string> ordinalKey = default,
        Option<string> wireKey = default,
        Option<ExecutionProviderDevicePolicy> devicePolicy = default,
        Option<Func<ArtifactSite, FrozenDictionary<string, string>>> locationOptions = default) =>
        new(providerName,
            wireKey,
            hostGate,
            epOptions,
            locationOptions.IfNone(static _ => FrozenDictionary<string, string>.Empty),
            static _ => FrozenDictionary<string, string>.Empty,
            ordinalKey,
            devicePolicy,
            affinity,
            WarmForm.EpContext,
            register);
}
```
