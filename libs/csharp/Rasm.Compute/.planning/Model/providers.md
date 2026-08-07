# [COMPUTE_PROVIDERS]

`ExecutionProvider` rows select ONNX Runtime registration through host-gated device discovery and deterministic affinity over one frozen runtime snapshot. `ModelPrecision` owns execution posture, `WarmForm` owns which warm-start mechanism a row reaches, and compiled-context compatibility admits an EP-context warm start only on `EP_SUPPORTED_OPTIMAL`.

`ExecutionProvider`, `ModelPrecision`, and `WarmForm` own discovery, registration, compatibility, resolution, warm-start form, and result identity. ONNX Runtime supplies provider members, `ModelFingerprint` keys behavior, and NodaTime owns negative-cache duration.

## [01]-[INDEX]

- [02]-[EP_AXIS]: execution-provider rows over one frozen runtime snapshot and autoEP `OrtEpDevice` discovery, one polymorphic register threading the selected device ordinal, a typed two-step compatibility probe, precision-folded quantization knobs, and the three-row warm-start form axis every row selects one of.

## [02]-[EP_AXIS]

- Owner: `ExecutionProvider` `[SmartEnum<string>]` rows (provider name, wire spelling, host gate, precision-keyed EP options, artifact-site location options, session keys, device-ordinal option key, device policy, hardware affinity, warm form, register delegate); `ModelPrecision` `[SmartEnum<string>]` rows (wire spelling, low-precision accumulation, BF16 fast math, `session.qdq_matmulnbits_accuracy_level`, negative TTL); `WarmForm` `[SmartEnum<string>]` rows (artifact suffix, runtime-keyed identity, presence probe); `ProviderSnapshot` the once-materialized loaded-provider and published-device census; `ArtifactSite` the directory-and-warm-path pair a row's location options read; the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`BindWarm`/`Resolve`/`FromWire`/`ResultKey` fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `Cuda`, `DirectMl`, `TensorRt`, `CoreMl`, `WebGpu`, `OpenVino`, `MiGraphX`, `Nnapi`, `Dnnl`; `ModelPrecision` rows `Full`, `Fp16`, `Bf16`, `Int8`, `Int4`; `WarmForm` rows `EpContext`, `OptimizedGraph`, `EngineCache`.
- Law: `Cpu` is the GUARANTEED FLOOR and every other row a POLICY THAT MAY REFUSE. `CPUExecutionProvider` ships with every runtime under a host gate that never closes, so `Resolve` degrades a key naming no row — or a row whose native provider the host never loaded — onto the floor rather than faulting a caller holding no alternative. `IsFloor` derives that identity instead of a column, so exactly one row answers it and no construction forks the answer.
- Law: each row carries its OWN wire spelling. `WireKey` is a row column and `FromWire` the one projection, so a cross-boundary record naming a provider or a precision holds no translation table of its own and an accelerator landing later crosses every wire by declaring one string. Rows spelling `None` never cross, which keeps a posture demanding settled pre-quantized bytes unreachable from a wire carrying only an execution preference.
- Law: an unresolvable PROVIDER degrades and an unresolvable PRECISION refuses. `ExecutionProvider.FromWire` answers `Floor` for a spelling this roster cannot honour because the consuming record reports what ran and a caller reads the substitution off that column; `ModelPrecision.FromWire` answers `None` because precision carries no such report, so the consumer refuses — an fp16 request silently executing fp32 is the exact substitution the `CoreMl` row's `ModelFormat` pin exists to foreclose, and a default there reopens it one layer up.
- Law: EVERY row carries a warm-start form and the floor row is not exempt. `WarmForm.EpContext` is the compiled-partition mechanism — a blob reloaded through the `ep.context_*` keys whose admissibility its own embedded compat info answers; `WarmForm.OptimizedGraph` is the managed mechanism `Cpu` and `Dnnl` take, where ORT writes the post-optimization graph through `OptimizedModelFilePath` and a later cold open loads THAT graph at `ORT_DISABLE_ALL`; `WarmForm.EngineCache` is `TensorRt`'s, whose warm start is the EP's OWN engine cache — `trt_engine_cache_enable` plus `trt_engine_cache_path` arm it at registration and the EP writes and reads that directory itself, so the artifact is a directory rather than a file and its presence probe reads one. Without the managed row the guaranteed floor — the row every degrade lands on and every parity probe runs — re-pays `ORT_ENABLE_ALL` from source on every cold open forever, and `Compatible` answers `EP_NOT_APPLICABLE` for it by construction, so no compat verdict opens that door. `Suffix`, `RuntimeKeyed`, and `Present` keep all three forms inside the ONE artifact-key derivation: the compat-info-bearing blob needs no runtime column, the managed graph and the engine cache carry none and take one, so an artifact another runtime wrote misses its key rather than loading.
- Law: a row is admitted by EXECUTION, never by inventory. `ROCMExecutionProvider` carries no row here — the EP left the runtime's source tree, and its managed surface (both append overloads, the options capsule, the one-call shorthand) survives only as ABI-stable stubs whose native entry points unconditionally answer that the provider is not enabled in this build. A row over them would publish an accelerated route that executes nothing while `Available` answered false forever, which is precisely the inventory-versus-execution defect this folder already settled; AMD hardware reaches this roster through `MiGraphX`, the migration the runtime itself names.
- Law: the generic string append is a CLOSED native roster, and it decides each row's registration family. `AppendExecutionProvider(name, options)` marshals the name straight to native, where a fixed table admits a short or canonical spelling per EP and refuses everything else with `ORT_INVALID_ARGUMENT` — `CoreMLExecutionProvider`, `WebGPU`, `OpenVINO`, `DML`, `MIGraphX`, and `CPU` are on it; `CUDAExecutionProvider` and `TensorrtExecutionProvider` are NOT. Rows therefore split three ways and the split is measured, not stylistic: option-bag rows take the string append, `Cuda` and `TensorRt` take their `OrtCUDAProviderOptions`/`OrtTensorRTProviderOptions` `UpdateOptions` capsule and the typed append (their only dictionary path), and `Cpu`, `Nnapi`, and `Dnnl` take the typed scalar member their EP publishes. Spelling a name the table refuses fails at the append rather than at the run, so a row is admitted to the string family by its NAME being on that table and by nothing else.
- Law: accelerated rows answer to the floor by MEASUREMENT, and the residual band is an OBSERVATION. Every non-floor row produces a result whose residual against a floor-provider run over the same input is measured at the run and reported outward, because a provider drawing its speed from lower internal precision degrades silently and by construction raises nothing. The band a model family reaches is therefore the `Model/inference#STAGE_EXECUTION` canary's own record at that family's first execution on this host — carried on the parity artifact beside the verdict it gated — never a constant this axis asserts: a spec-side tolerance grades every family against whichever one happened to be measured when it was written. This axis declares the obligation; the consumer owning admission performs the comparison and owns the tolerance.
- Auto: `ProviderSnapshot` freezes the loaded-provider set and the published `OrtEpDevice` census ONCE behind a lazy accessor, and `Available`/`Devices`/`Resolve`/`FromWire` read that frozen census — `Available` still short-circuits on `HostGate` first. `FromWire` scans the row roster rather than caching a frozen inverse, because a static table folded from `Items` beside the row fields reads an empty roster whenever its initializer wins the ordering race — and a ten-row ordinal scan disappears beside the session lease it precedes. `AutoSelect` ranks the snapshot's devices by row-owned `HardwareAffinity`, then CPU last, then provider/vendor/device identity for deterministic ties. One selected-device snapshot passes through `Register`, `Compatible`, and `WarmStartAdmissible`. `Register` folds session keys and precision `QdqKeys`, composes row-owned EP and `ArtifactSite` location option tables, writes the SELECTED device's ordinal under the row's own `OrdinalKey`, then uses direct autoEP registration when the snapshot is non-empty or the row's verified fallback registration otherwise — measured at the pin: `CoreMl` publishes NO `OrtEpDevice`, so its snapshot is empty BY CONSTRUCTION and the fallback arm is its only reachable path, while `WebGpu` is the macOS row that publishes a GPU device and takes the autoEP arm; `EpOptions` is EMPTY on every published device, so no row inherits discovery defaults and the row-owned option table is the sole source. Location options are row-owned and disjoint: `CoreMl` alone contributes `ModelCacheDirectory` and `TensorRt` alone `trt_engine_cache_path`. `Compatible` runs the two-step probe over the same snapshot against the compiled artifact's embedded compat info. `WarmStartAdmissible` proves the artifact present through the FORM's own probe — a file for the blob and managed rows, a directory for the engine cache — then dispatches on that form: the EP-context arm requires exactly `EP_SUPPORTED_OPTIMAL` read from the artifact — `EP_UNSUPPORTED`, `EP_SUPPORTED_PREFER_RECOMPILATION`, and `EP_NOT_APPLICABLE` compile fresh — while the managed and engine-cache arms admit on presence, their runtime and behavior columns having keyed the artifact name. `BindWarm` writes each form's own session state under one call: the `ep.context_*` triple for the blob rows, `OptimizedModelFilePath` for the managed rows on a MISS alone (an admissible graph is the open's own load source), and NOTHING for the engine cache, whose arming already rode the EP option bag. `Veto` folds incompatibility reason, notes, and code per hardware device. `Resolve` reads the row roster by key, proves the native provider loaded through `Available`, and answers `Floor` otherwise. `ResultKey` stamps provider, runtime, precision, and the shared behavior-option fingerprint.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a built-in accelerator is one `ExecutionProvider` row minted through `Accelerator` with its provider name, OS gate, `HardwareAffinity`, EP-option and device-ordinal columns, and register delegate, its EP-context warm form implied by the factory; a row whose EP compiles no context partition declares through the constructor instead and states its own `WarmForm` — including an accelerator whose native provider this platform's runtime may not carry, because `Available` and `Resolve` already answer absence and no caller-facing surface moves. That row reaches every wire the moment it declares a `WireKey`; before it declares one, `FromWire` degrades the spelling it answers to onto the floor exactly as it degrades an unloaded provider. An OUT-OF-TREE accelerator is one row on this same roster and nothing else: its library registers once at composition through `OrtEnv.RegisterExecutionProviderLibrary`, its devices then publish into the snapshot like any built-in EP's, and its register delegate takes the autoEP device arm — the string-append name table is closed native vocabulary no out-of-tree name enters, and a row minted at RUNTIME never joins `Items`, so `TryGet`, `Resolve`, and `FromWire` could never answer it and every wire and receipt naming it read the floor instead. Its library bytes still reach identity: `Model/sessions#SESSION_CAPSULE` `DeviceFingerprint` folds the device's own `EpMetadata` and `EpOptions` into the resident and warm-artifact keys, so a library version change re-keys both. Each quantization posture is one `ModelPrecision` row folded into the same registration and fingerprint rails; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on `AutoSelect`; a fourth warm-start mechanism is one `WarmForm` row with its suffix, runtime-keying column, presence probe, and the arms `WarmStartAdmissible` and `BindWarm` then demand — the artifact key, the blob lane, and the retention row all follow it with no edit.
- Boundary: each row owns one provider-specific fallback registration and one autoEP device registration path selected by a caller-held device snapshot, so one lease appends one provider. `CoreMl` and `WebGpu` use the generic `AppendExecutionProvider(name, options)` spelling for one measured reason: neither carries a dedicated managed method nor an exported C append entry (the pinned dylib exports exactly `_CPU` and `_CoreML` beside `OrtGetApiBase`, and no `_WebGPU`), so the string append is their only path — `CoreMl` because its row owns `ModelFormat`, compute units, specialization, cache directory, and precision beyond the flags overload, `WebGpu` because the EP has no other spelling at all; the `CoreMl` flags overload never runs beside its row. `OpenVino`, `DirectMl`, and `MiGraphX` take that same spelling because it is their ONLY managed dictionary path — none publishes an options capsule, and each typed member takes a bare scalar that cannot carry an option bag at all. The DEVICE ORDINAL rides that bag as the row's own `OrdinalKey` rather than a registration literal: `AutoSelect` ranks by affinity, so binding device zero would run the graph on whichever adapter the driver enumerated first while every receipt named the device the rank chose; the ordinal is the device's index within THIS row's frozen census, which is why the census must be frozen — a re-enumeration hands back fresh instances a selected device can never be located in again. `WireKey` stays out of `OptionsFor` and out of `ResultKey`: it names the row for a boundary record and reaches nothing the built session does, so a rename changing no execution re-keys no cached result. Location options affect native artifact placement but stay out of result identity, while EP/session/precision options enter `OptionsFor`. `HostGate` expresses row-specific OS capability while the frozen loaded-provider set proves the native provider. `Warm` stays out of `OptionsFor` and out of `ResultKey`: it selects which artifact a cold open reads, never what the built graph computes, so a form change re-keys the warm artifact through `Suffix` and leaves every cached result standing. Rows reference `WarmForm` fields across a type boundary, so the roster-race the `FromWire` scan guards against — a same-class static folded from `Items` beside the row initializers — cannot arise here. `Full` leaves `mlas.enable_gemm_fastmath_arm64_bfloat16` disabled; `Bf16` alone sets it. Precision also reaches CoreML low-precision accumulation and MatMulNBits accuracy, and every behavior option participates in `ModelFingerprint.Of`. Compatibility consumes `OrtCompiledModelCompatibility` directly and admits EP-context reuse only for an existing `EP_SUPPORTED_OPTIMAL` artifact.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelPrecision {
    // Precision rows carry EXECUTION POSTURE, never a graph transform: Int8/Int4 demand settled pre-quantized
    // model bytes (the quantized graph is its own checksum identity) and select the MatMulNBits accuracy floor
    // with the accumulation posture; `QuantizedGraph` is the admission evidence session `Options` gates on.
    // Each row's wire spelling is its own column, and rows demanding settled quantized bytes carry NONE — nothing a
    // caller states as a preference selects a posture that is a property of the model file it did not supply.
    public static readonly ModelPrecision Full = new("full", wireKey: "fp32", lowPrecisionAccumulation: false, bfloat16FastMath: false, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(15));
    public static readonly ModelPrecision Fp16 = new("fp16", wireKey: "fp16", lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Bf16 = new("bf16", wireKey: Option<string>.None, lowPrecisionAccumulation: true, bfloat16FastMath: true, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Int8 = new("int8", wireKey: Option<string>.None, lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: 4, quantizedGraph: true, negativeTtl: Duration.FromMinutes(5));
    public static readonly ModelPrecision Int4 = new("int4", wireKey: Option<string>.None, lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: 4, quantizedGraph: true, negativeTtl: Duration.FromMinutes(2));

    private ModelPrecision(
        string key, Option<string> wireKey, bool lowPrecisionAccumulation, bool bfloat16FastMath,
        Option<int> accuracyLevel, bool quantizedGraph, Duration negativeTtl) : this(key) =>
        (WireKey, LowPrecisionAccumulation, Bfloat16FastMath, AccuracyLevel, QuantizedGraph, NegativeTtl) =
        (wireKey, lowPrecisionAccumulation, bfloat16FastMath, accuracyLevel, quantizedGraph, negativeTtl);

    public Option<string> WireKey { get; }
    public bool LowPrecisionAccumulation { get; }
    public bool Bfloat16FastMath { get; }
    public Option<int> AccuracyLevel { get; }
    public bool QuantizedGraph { get; }
    public Duration NegativeTtl { get; }

    // NONE refuses at the consumer. A precision the roster cannot honour has no report column on any result, so
    // substituting a default here would run one posture while the receipt names another.
    public static Option<ModelPrecision> FromWire(string wire) =>
        toSeq(Items).Find(row => row.WireKey.Case is string key && StringComparer.Ordinal.Equals(key, wire));

    public FrozenDictionary<string, string> QdqKeys =>
        AccuracyLevel.Match(
            Some: static level => new Dictionary<string, string>(StringComparer.Ordinal) {
                ["session.qdq_matmulnbits_accuracy_level"] = level.ToString(CultureInfo.InvariantCulture),
            }.ToFrozenDictionary(StringComparer.Ordinal),
            None: static () => FrozenDictionary<string, string>.Empty);
}

// THREE warm-start forms exist and every provider row takes exactly one. The EP-context blob is the compiled-
// partition form — a subgraph ORT reloads through the `ep.context_*` keys, carrying the compat info its own
// compilation embedded. The managed optimized graph is the only form the floor row and every EP-context-unaware row
// can reach at all: ORT writes the post-optimization graph through `OptimizedModelFilePath` and a later cold open
// LOADS that graph at `ORT_DISABLE_ALL`, which is the whole cost the form removes — pointing `OptimizedModelFilePath`
// at a file no open ever reads back is a warm start in name alone. The engine cache is TensorRT's, and it is the EP's
// OWN mechanism: the provider builds a plan per graph, precision, and GPU, writes it under `trt_engine_cache_path`,
// and reads it back itself, so the artifact is a DIRECTORY of EP-private engine files and this axis arms it through
// the option bag rather than through any session key.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WarmForm {
    public static readonly WarmForm EpContext = new("ep-context", "ctx.onnx", false, File.Exists);
    public static readonly WarmForm OptimizedGraph = new("optimized-graph", "opt.onnx", true, File.Exists);
    public static readonly WarmForm EngineCache = new("engine-cache", "trt.cache", true, Directory.Exists);

    // Suffix rides the ONE artifact-key derivation, so every form shares one identity, one blob lane, and one
    // retention row rather than a cache and a filename scheme per mechanism.
    public string Suffix { get; }

    // EP-context blobs answer their own admissibility through the compat info compiled into them; the managed
    // optimized graph embeds none and a TensorRT plan is valid only for the runtime that built it, so the RUNTIME
    // VERSION enters both their keys and an artifact another ORT wrote MISSES rather than loading under a runtime
    // that never produced it.
    public bool RuntimeKeyed { get; }

    // Presence is FORM-SHAPED: a blob and a managed graph are files, an engine cache is a directory the EP fills
    // itself, and a file probe over that directory answers absent forever — which is a warm start that never warms.
    [UseDelegateFromConstructor]
    public partial bool Present(string path);
}

// Provider availability and the published EP-device census are properties of the LOADED RUNTIME, and the runtime
// boots once. Re-reading `GetAvailableProviders` and `GetEpDevices` on every `Available`, `Devices`, `Resolve`, and
// `FromWire` pays a native call per row per lease for a set that cannot change after `OrtEnv` creation — and hands
// back FRESH `OrtEpDevice` instances each time, so a device `AutoSelect` ranked could never be located again by
// identity, which is exactly what the ordinal projection needs.
public sealed record ProviderSnapshot(FrozenSet<string> Loaded, FrozenDictionary<string, Seq<OrtEpDevice>> Published) {
    public static ProviderSnapshot Of(OrtEnv env) => new(
        env.GetAvailableProviders().ToFrozenSet(StringComparer.Ordinal),
        toSeq(env.GetEpDevices())
            .GroupBy(static device => device.EpName, StringComparer.Ordinal)
            .ToFrozenDictionary(static group => group.Key, static group => toSeq(group), StringComparer.Ordinal));

    public Seq<OrtEpDevice> For(string providerName) =>
        Published.TryGetValue(providerName, out Seq<OrtEpDevice> devices) ? devices : Seq<OrtEpDevice>();
}

// Where a row's native artifacts live: the shared artifact DIRECTORY any row may write beneath, and the
// form-derived WARM PATH this open's own artifact key names. Each row reads whichever its EP spells — CoreML wants a
// directory it manages itself, TensorRT wants the engine-cache directory this key derives — and neither reaches for
// the other's, so one value carries both facts instead of two parameters every row must ignore one of.
public readonly record struct ArtifactSite(string Directory, string WarmPath);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExecutionProvider {
    // ONE census per process, materialized at the accessor rather than an eager static: the read edge is the first
    // provider question any lease asks, which is necessarily after `Model/sessions#SESSION_CAPSULE` `Boot` created
    // the environment, while an eager initializer would race the row fields it sits beside.
    static readonly Lazy<ProviderSnapshot> Frozen =
        new(static () => ProviderSnapshot.Of(OrtEnv.Instance()), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ProviderSnapshot Snapshot => Frozen.Value;

    // ModelFormat is PINNED to MLProgram: the NeuralNetwork format the row would otherwise default to executes an
    // fp32 graph at fp16 on the ANE without saying so, so the pin is what keeps precision a declared posture rather
    // than a silent host decision — and it is why AllowLowPrecisionAccumulationOnGPU tracks the precision row
    // instead of standing open. Every claim this row makes about numeric agreement is answerable to a floor-provider
    // residual measured at the run. Measured floor at the pin: an fp32 MLProgram conv graph at MLComputeUnits ALL
    // reproduces the Cpu row within 2e-7 max absolute — float-ulp class; per-model-family bands stay the run's to record.
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
            ["mlas.enable_gemm_fastmath_arm64_bfloat16"] = precision.Bfloat16FastMath ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static FrozenDictionary<string, string> CoreMlOptions(ModelPrecision precision) =>
        new Dictionary<string, string>(CoreMlRows, StringComparer.Ordinal) {
            ["AllowLowPrecisionAccumulationOnGPU"] = precision.LowPrecisionAccumulation ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // The engine cache is armed on the OPTION BAG, and only the enable flag reaches result identity: the plan a
    // TensorRT engine encodes is the same graph at the same precision whichever directory holds it, so the path is
    // a location column and the flag is a behavior one.
    static readonly FrozenDictionary<string, string> TensorRtRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["trt_engine_cache_enable"] = "1",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    // OpenVINO's device selection is `device_type`, whose vocabulary is bare `CPU`/`GPU`/`NPU` or a mode prefix
    // (`AUTO`, `HETERO`, `MULTI`) over a comma-separated priority list; the parser refuses anything else outright.
    // Bare `AUTO` is the row's posture because it returns without a device list and lets the plugin rank the
    // hardware it actually found, where a pinned device names silicon this roster cannot know is present. The
    // legacy fused spellings (`GPU_FP16` and its train) are gone, and the old `precision`/`num_of_threads`/
    // `cache_dir` knobs are deprecated in favour of `load_config` — none of which this row spells.
    static readonly FrozenDictionary<string, string> OpenVinoRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["device_type"] = "AUTO",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static readonly ExecutionProvider Cpu = new(
        "cpu", providerName: "CPUExecutionProvider", wireKey: "cpu", hostGate: static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, locationOptions: static _ => FrozenDictionary<string, string>.Empty,
        sessionKeys: CpuSessionKeys, ordinalKey: Option<string>.None,
        devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        warm: WarmForm.OptimizedGraph,
        registerRow: static (options, _) => options.AppendExecutionProvider_CPU(1));

    // CUDA and TensorRT are the two rows the generic string append REFUSES, so each folds its bag through its own
    // provider-options capsule and takes the typed append — the capsule's `UpdateOptions` is the same marshaling the
    // string overload performs, reached through the only door native leaves open for these two EPs.
    public static readonly ExecutionProvider Cuda = Accelerator(
        "cuda", "CUDAExecutionProvider", OrtHardwareDeviceType.GPU, static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => {
            using OrtCUDAProviderOptions cuda = new();
            cuda.UpdateOptions(new Dictionary<string, string>(rows, StringComparer.Ordinal));
            options.AppendExecutionProvider_CUDA(cuda);
        });

    public static readonly ExecutionProvider DirectMl = Accelerator(
        "directml", "DmlExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsWindows,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => options.AppendExecutionProvider(
            "DML", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

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
            tensorRt.UpdateOptions(new Dictionary<string, string>(rows, StringComparer.Ordinal));
            options.AppendExecutionProvider_Tensorrt(tensorRt);
        });

    public static readonly ExecutionProvider CoreMl = new(
        "coreml", providerName: "CoreMLExecutionProvider", wireKey: "coreMl",
        hostGate: static () => OperatingSystem.IsMacOSVersionAtLeast(12),
        epOptions: CoreMlOptions,
        locationOptions: static site => new Dictionary<string, string>(StringComparer.Ordinal) { ["ModelCacheDirectory"] = site.Directory }.ToFrozenDictionary(StringComparer.Ordinal),
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: Option<string>.None,
        devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU), hardwareAffinity: OrtHardwareDeviceType.NPU,
        warm: WarmForm.EpContext,
        registerRow: static (options, rows) => options.AppendExecutionProvider(
            "CoreMLExecutionProvider", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

    // WebGPU is macOS-arm64-EXCLUSIVE at the pin: only the osx-arm64 dylib carries the Dawn/Metal EP body and
    // its ep.webgpuexecutionprovider.* option keys — every other RID holds the name literal with zero payload,
    // so HostGate closes there and Available answers false before GetAvailableProviders is consulted. On this
    // host GetAvailableProviders answers CoreML, WebGpu, CPU; the row publishes a GPU OrtEpDevice (vendor Apple,
    // EpOptions empty) and takes the autoEP arm, the generic string append its fallback. The row exposes NO
    // dawnBackendType and no native-handle knob (webgpuInstance/webgpuDevice/dawnProcTable/enablePIXCapture):
    // a dawnBackendType naming an unavailable backend aborts the PROCESS from a native callback frame the CLR
    // never unwinds, so the key is structurally unspellable here. Admitted vocabulary (bare keys on the string
    // append; values case-sensitive and exact — `disabled`/`bucket`/`1`, never `Disabled`/`Bucket`/`true`):
    // powerPreference (high-performance | low-power), preferredLayout (NCHW | NHWC), validationMode
    // (disabled | wgpuOnly | basic | full), storageBufferCacheMode/uniformBufferCacheMode/
    // queryResolveBufferCacheMode/defaultBufferCacheMode (disabled | lazyRelease | simple | bucket),
    // enableGraphCapture (0|1), enableInt64 (0|1 — a capability toggle, never a precision posture),
    // preserveDevice (0|1), maxStorageBufferBindingSize (bytes). ModelPrecision reaches WebGPU through NO
    // EP-specific knob — the row contributes only the shared session/precision rails, and the default option
    // table stays EMPTY so ORT's own defaults govern until a measured policy pins a key.
    public static readonly ExecutionProvider WebGpu = new(
        "webgpu", providerName: "WebGpuExecutionProvider", wireKey: "webGpu",
        hostGate: static () => OperatingSystem.IsMacOS(),
        epOptions: static _ => FrozenDictionary<string, string>.Empty,
        locationOptions: static _ => FrozenDictionary<string, string>.Empty,
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: Option<string>.None,
        devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_GPU), hardwareAffinity: OrtHardwareDeviceType.GPU,
        warm: WarmForm.EpContext,
        registerRow: static (options, rows) => options.AppendExecutionProvider(
            "WebGPU", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

    // `AppendExecutionProvider_OpenVINO(string)` takes a bare device string and nothing else — no options overload
    // and no managed provider-options capsule exists for this EP — so the generic string append IS the only path
    // that carries `device_type`, and the legacy scalar member is the deleted form rather than a fallback.
    public static readonly ExecutionProvider OpenVino = Accelerator(
        "openvino", "OpenVINOExecutionProvider", OrtHardwareDeviceType.NPU, static () => true,
        epOptions: static _ => OpenVinoRows,
        register: static (options, rows) => options.AppendExecutionProvider(
            "OpenVINO", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

    public static readonly ExecutionProvider MiGraphX = Accelerator(
        "migraphx", "MIGraphXExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsLinux,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, ordinalKey: "device_id",
        register: static (options, rows) => options.AppendExecutionProvider(
            "MIGraphX", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

    // NNAPI carries no option bag at all — its knob set is the `NnapiFlags` enum its typed member takes — so the row
    // declares no ordinal key and the bag it receives stays empty by construction.
    public static readonly ExecutionProvider Nnapi = Accelerator(
        "nnapi", "NnapiExecutionProvider", OrtHardwareDeviceType.NPU, OperatingSystem.IsAndroid,
        epOptions: static _ => FrozenDictionary<string, string>.Empty,
        register: static (options, _) => options.AppendExecutionProvider_Nnapi(NnapiFlags.NNAPI_FLAG_USE_NONE));

    // Dnnl declares through the full constructor rather than `Accelerator` because it is CPU-affinity and EP-context
    // UNAWARE — `Compatible` answers `EP_NOT_APPLICABLE` for it — so without the managed warm form this row re-runs
    // `ORT_ENABLE_ALL` over the source graph on every cold open forever, exactly as the floor row would.
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
        Action<SessionOptions, IReadOnlyDictionary<string, string>> registerRow) : this(key) =>
        (ProviderName, WireKey, HostGate, EpOptions, LocationOptions, SessionKeys, OrdinalKey, DevicePolicy, HardwareAffinity, Warm, RegisterRow) =
        (providerName, wireKey, hostGate, epOptions, locationOptions, sessionKeys, ordinalKey, devicePolicy, hardwareAffinity, warm, registerRow);

    public string ProviderName { get; }
    public Option<string> WireKey { get; }
    public Func<bool> HostGate { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> EpOptions { get; }
    public Func<ArtifactSite, FrozenDictionary<string, string>> LocationOptions { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> SessionKeys { get; }

    // The option key THIS EP spells the selected device's ordinal under, `None` where the EP has no such key at all
    // — a row that names one gets the ranked device and a row that does not never receives a fabricated slot.
    public Option<string> OrdinalKey { get; }

    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public OrtHardwareDeviceType HardwareAffinity { get; }
    public WarmForm Warm { get; }
    public Action<SessionOptions, IReadOnlyDictionary<string, string>> RegisterRow { get; }

    public bool Available => HostGate() && Snapshot.Loaded.Contains(ProviderName);

    // Floor identity is DERIVED, never a column: exactly one row can answer true and no construction can fork it.
    public bool IsFloor => ReferenceEquals(this, Cpu);

    public static ExecutionProvider Floor => Cpu;

    // Degradation happens HERE and nowhere else: a caller states a preference, this answer decides, and the run
    // reports it back as evidence — so an unshipped or host-refused provider never becomes a caller-facing fault.
    public static ExecutionProvider Resolve(string key) =>
        TryGet(key, out ExecutionProvider? row) && row.Available ? row : Floor;

    // Wire values name a PREFERENCE and this answer names what runs. Spellings no row claims and rows whose native
    // provider never loaded reach the floor by one route, because from this end they are one fact: a provider this
    // host cannot offer. Consuming records report the answer, so the substitution is never silent. Resolution scans
    // rather than folding a cached inverse — static tables built from `Items` beside the row initializers race them
    // and can freeze an empty roster.
    public static ExecutionProvider FromWire(string wire) =>
        toSeq(Items)
            .Find(row => row.WireKey.Case is string key && StringComparer.Ordinal.Equals(key, wire))
            .Match(Some: static row => Resolve(row.Key), None: static () => Floor);

    // Results report this spelling; rows with no wire column reach a result only as the floor's own answer.
    public string ReportKey => WireKey.IfNone(Key);

    public Seq<OrtEpDevice> Devices => Snapshot.For(ProviderName);

    public Seq<OrtEpDevice> AutoSelect =>
        toSeq(Devices.OrderByDescending(device =>
            device.HardwareDevice.Type == HardwareAffinity ? 2
            : device.HardwareDevice.Type == OrtHardwareDeviceType.CPU ? 0
            : 1)
        .ThenBy(static device => device.EpName, StringComparer.Ordinal)
        .ThenBy(static device => device.EpVendor, StringComparer.Ordinal)
        .ThenBy(static device => device.HardwareDevice.VendorId)
        .ThenBy(static device => device.HardwareDevice.DeviceId));

    public Seq<(OrtHardwareDeviceType Device, OrtDeviceEpIncompatibilityReason Reason, string Notes, int Code)> Veto =>
        toSeq(OrtEnv.Instance().GetHardwareDevices()).Map(device => {
            using OrtDeviceEpIncompatibilityDetails details = OrtEnv.Instance().GetHardwareDeviceEpIncompatibilityDetails(ProviderName, device);
            return (Device: device.Type, Reason: details.ReasonsBitmask, Notes: details.Notes, Code: details.ErrorCode);
        }).Filter(static row => row.Reason != OrtDeviceEpIncompatibilityReason.None);

    public FrozenDictionary<string, string> OptionsFor(ModelPrecision precision) =>
        EpOptions(precision).Concat(SessionKeys(precision)).Concat(precision.QdqKeys)
            .ToFrozenDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal);

    public void Register(SessionOptions options, ArtifactSite artifacts, ModelPrecision precision, Seq<OrtEpDevice> devices) {
        toSeq(SessionKeys(precision).Concat(precision.QdqKeys)).Iter(entry => options.AddSessionConfigEntry(entry.Key, entry.Value));
        Dictionary<string, string> registerOptions = new(EpOptions(precision), StringComparer.Ordinal);
        toSeq(LocationOptions(artifacts)).Iter(entry => registerOptions[entry.Key] = entry.Value);
        // The RANKED device's ordinal is one more option row: `AutoSelect` ordered by hardware affinity, so a
        // registration binding a literal zero runs the graph on whichever adapter the driver enumerated first while
        // every receipt names the device the rank chose — the two disagree silently on any multi-GPU host.
        OrdinalKey.Iter(key => Ordinal(devices).Iter(ordinal =>
            registerOptions[key] = ordinal.ToString(CultureInfo.InvariantCulture)));
        if (devices.IsEmpty) { RegisterRow(options, registerOptions); }
        else {
            options.AppendExecutionProvider(OrtEnv.Instance(), devices.ToList(), registerOptions);
        }
    }

    // The ordinal an EP option row spells is the device's INDEX within this row's own census — `GetEpDevices`
    // enumerates per EP — never the hardware vendor or device id, which name silicon rather than an EP device slot.
    // Locating it by reference is exactly why the census freezes: a fresh enumeration returns fresh instances and
    // the selected device is then findable in none of them.
    Option<int> Ordinal(Seq<OrtEpDevice> devices) =>
        devices.Head.Bind(chosen => Devices
            .Map(static (device, index) => (Device: device, Index: index))
            .Find(row => ReferenceEquals(row.Device, chosen))
            .Map(static row => row.Index));

    // Compat info is embedded at compile time: the probe reads the COMPILED EP-context artifact, never the uncompiled source model.
    public Option<OrtCompiledModelCompatibility> Compatible(string compiledModelPath, Seq<OrtEpDevice> devices) =>
        devices.IsEmpty
            ? None
            : Some(OrtEnv.Instance().GetModelCompatibilityForEpDevices(
                devices.ToList(), OrtEnv.Instance().GetCompatibilityInfoFromModel(compiledModelPath, ProviderName)));

    // Admissibility dispatches on the row's own warm FORM, and PRESENCE is that form's own probe — a blob and a
    // managed graph are files, an engine cache is a directory the EP fills itself. An EP-context blob answers
    // through the two-step compat probe embedded in it; the managed graph and the engine cache embed no compat info
    // at all, and their runtime version and every construction-behavior column already key the artifact name, so
    // PRESENCE is admissibility there and a stale artifact is a key MISS rather than a verdict.
    public bool WarmStartAdmissible(string warmPath, Seq<OrtEpDevice> devices) =>
        Warm.Present(warmPath)
        && Warm.Switch(
            state: (Row: this, Path: warmPath, Devices: devices),
            epContext: static probe => probe.Row.Compatible(probe.Path, probe.Devices).Case is OrtCompiledModelCompatibility.EP_SUPPORTED_OPTIMAL,
            optimizedGraph: static _ => true,
            engineCache: static _ => true);

    // ONE warm bind per open: EP-context rows write the `ep.context_*` triple and managed rows point
    // `OptimizedModelFilePath` at the same artifact key, so the session fold carries no per-provider branch and a row
    // landing later reaches every warm mechanism by declaring one column. The managed write arms on a MISS alone —
    // an admissible artifact is the open's own LOAD source, and re-serializing a graph just loaded from it pays the
    // whole optimization write for a file byte-identical to the one on disk. The engine cache writes NOTHING here:
    // `trt_engine_cache_enable` and `trt_engine_cache_path` are EP provider options the registration already folded,
    // so a session key beside them would be a second arming of one mechanism, and the one ORT never reads.
    public void BindWarm(SessionOptions options, string warmPath, bool admissible) =>
        Warm.Switch(
            state: (Options: options, Path: warmPath, Admissible: admissible),
            epContext: static site => {
                site.Options.AddSessionConfigEntry("ep.context_enable", site.Admissible ? "1" : "0");
                site.Options.AddSessionConfigEntry("ep.context_file_path", site.Path);
                site.Options.AddSessionConfigEntry("ep.share_ep_contexts", "1");
            },
            optimizedGraph: static site => {
                if (!site.Admissible) { site.Options.OptimizedModelFilePath = site.Path; }
            },
            engineCache: static _ => { });

    public string ResultKey(string ortVersion, ModelPrecision precision) =>
        $"{Key}:{ortVersion}:{precision.Key}:{ModelFingerprint.Of(OptionsFor(precision)):x16}";

    // Built-in accelerators carry NO wire spelling until a boundary record names one: rows exist so `Available`
    // answers for the host, and a spelling nobody sends puts an unreachable key on every wire reading this roster.
    // `wireKey` defaults to `None`, so admitting a row to a wire is one argument. Every row this factory mints is
    // EP-context capable by definition — a row whose EP warms through another mechanism declares through the
    // constructor beside `Cpu`, `TensorRt`, and `Dnnl`, which is what keeps the warm form a stated column rather
    // than an assumed default. The register delegate takes the OPTION BAG because that bag now carries the ranked
    // device ordinal, so a delegate ignoring it would discard the selection the rank just made.
    static ExecutionProvider Accelerator(
        string key,
        string providerName,
        OrtHardwareDeviceType affinity,
        Func<bool> hostGate,
        Func<ModelPrecision, FrozenDictionary<string, string>> epOptions,
        Action<SessionOptions, IReadOnlyDictionary<string, string>> register,
        Option<string> ordinalKey = default,
        Option<string> wireKey = default) =>
        new(
            key,
            providerName,
            wireKey,
            hostGate,
            epOptions,
            static _ => FrozenDictionary<string, string>.Empty,
            static _ => FrozenDictionary<string, string>.Empty,
            ordinalKey,
            Option<ExecutionProviderDevicePolicy>.None,
            affinity,
            WarmForm.EpContext,
            register);
}
```

`OrtEpDevice`, enumerated through `OrtEnv.GetEpDevices()` as an `IReadOnlyList<OrtEpDevice>`, carries the columns the `Devices`/`AutoSelect` fold reads:

| [INDEX] | [MEMBER]                                                | [CARRIES]                                                          |
| :-----: | :------------------------------------------------------ | :----------------------------------------------------------------- |
|  [01]   | `EpName`                                                | provider name keyed against the `ExecutionProvider` row            |
|  [02]   | `EpVendor`                                              | EP vendor string                                                   |
|  [03]   | `HardwareDevice`                                        | `OrtHardwareDevice`                                                |
|  [04]   | `EpMetadata`                                            | `OrtKeyValuePairs` EP self-description                             |
|  [05]   | `EpOptions`                                             | `OrtKeyValuePairs` default EP option set                           |
|  [06]   | `GetMemoryInfo(OrtDeviceMemoryType)`                    | `OrtMemoryInfo` for the device allocation at the named memory type |
|  [07]   | `CreateSyncStream(IReadOnlyDictionary<string, string>)` | `OrtSyncStream` tying a device-stream lifetime to the device       |

`OrtHardwareDevice`, reached through `HardwareDevice`, carries the device identity columns:

| [INDEX] | [COLUMN]   | [CARRIES]                                        |
| :-----: | :--------- | :----------------------------------------------- |
|  [01]   | `Type`     | `OrtHardwareDeviceType` — `CPU`, `GPU`, or `NPU` |
|  [02]   | `VendorId` | `uint` vendor id                                 |
|  [03]   | `DeviceId` | `uint` device id                                 |
|  [04]   | `Vendor`   | vendor string                                    |
|  [05]   | `Metadata` | `OrtKeyValuePairs` device self-description       |

`Veto` binds the `OrtDeviceEpIncompatibilityReason` `[Flags]` enum (`UInt32`) that `OrtDeviceEpIncompatibilityDetails.ReasonsBitmask` carries when an EP cannot claim a hardware device:

| [INDEX] | [REASON]             | [VALUE]    |
| :-----: | :------------------- | :--------- |
|  [01]   | `None`               | 0          |
|  [02]   | `DriverIncompatible` | 1          |
|  [03]   | `DeviceIncompatible` | 2          |
|  [04]   | `MissingDependency`  | 4          |
|  [05]   | `Unknown`            | 0x80000000 |

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
