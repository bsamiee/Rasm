# [COMPUTE_PROVIDERS]

`ExecutionProvider` rows select ONNX Runtime registration through host-gated device discovery and deterministic affinity. `ProviderLibrary` owns external EP lifetime, `ModelPrecision` owns execution posture, and compiled-context compatibility admits warm starts only on `EP_SUPPORTED_OPTIMAL`.

`ExecutionProvider` and `ModelPrecision` own discovery, registration, compatibility, resolution, and result identity. ONNX Runtime supplies provider members, `ModelFingerprint` keys behavior, and NodaTime owns negative-cache duration.

## [01]-[INDEX]

- [02]-[EP_AXIS]: execution-provider rows and autoEP `OrtEpDevice` discovery over one polymorphic register, a typed two-step compatibility probe, and precision-folded quantization knobs.

## [02]-[EP_AXIS]

- Owner: `ExecutionProvider` `[SmartEnum<string>]` rows (provider name, wire spelling, host gate, precision-keyed EP options, location options, session keys, device policy, hardware affinity, register delegate); `ModelPrecision` `[SmartEnum<string>]` rows (wire spelling, low-precision accumulation, BF16 fast math, `session.qdq_matmulnbits_accuracy_level`, negative TTL); the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`Resolve`/`FromWire`/`ResultKey` fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `Cuda`, `DirectMl`, `TensorRt`, `Rocm`, `CoreMl`, `WebGpu`, `OpenVino`, `MiGraphX`, `Nnapi`, `Dnnl`; `ModelPrecision` rows `Full`, `Fp16`, `Bf16`, `Int8`, `Int4`.
- Law: `Cpu` is the GUARANTEED FLOOR and every other row a POLICY THAT MAY REFUSE. `CPUExecutionProvider` ships with every runtime under a host gate that never closes, so `Resolve` degrades a key naming no row — or a row whose native provider the host never loaded — onto the floor rather than faulting a caller holding no alternative. `IsFloor` derives that identity instead of a column, so exactly one row answers it and no construction forks the answer.
- Law: each row carries its OWN wire spelling. `WireKey` is a row column and `FromWire` the one projection, so a cross-boundary record naming a provider or a precision holds no translation table of its own and an accelerator landing later crosses every wire by declaring one string. Rows spelling `None` never cross, which keeps a posture demanding settled pre-quantized bytes unreachable from a wire carrying only an execution preference.
- Law: an unresolvable PROVIDER degrades and an unresolvable PRECISION refuses. `ExecutionProvider.FromWire` answers `Floor` for a spelling this roster cannot honour because the consuming record reports what ran and a caller reads the substitution off that column; `ModelPrecision.FromWire` answers `None` because precision carries no such report, so the consumer refuses — an fp16 request silently executing fp32 is the exact substitution the `CoreMl` row's `ModelFormat` pin exists to foreclose, and a default there reopens it one layer up.
- Law: accelerated rows answer to the floor by MEASUREMENT. Every non-floor row produces a result whose residual against a floor-provider run over the same input is measured at the run and reported outward, because a provider drawing its speed from lower internal precision degrades silently and by construction raises nothing; this axis declares the obligation and `Model/inference#STAGE_EXECUTION` performs the comparison, so tolerance lives with the consumer owning admission rather than here.
- Auto: `Available` short-circuits on `HostGate` before `GetAvailableProviders`; `FromWire` scans the row roster rather than caching a frozen inverse, because a static table folded from `Items` beside the row fields reads an empty roster whenever its initializer wins the ordering race — and a ten-row ordinal scan disappears beside the session lease it precedes. `AutoSelect` ranks devices by row-owned `HardwareAffinity`, then CPU last, then provider/vendor/device identity for deterministic ties. One selected-device snapshot passes through `Register`, `Compatible`, and `WarmStartAdmissible`. `Register` folds session keys and precision `QdqKeys`, composes row-owned EP and location option tables, then uses direct autoEP registration when the snapshot is non-empty or the row's verified fallback registration otherwise — measured at the pin: `CoreMl` publishes NO `OrtEpDevice`, so its snapshot is empty BY CONSTRUCTION and the fallback arm is its only reachable path, while `WebGpu` is the macOS row that publishes a GPU device and takes the autoEP arm; `EpOptions` is EMPTY on every published device, so no row inherits discovery defaults and the row-owned option table is the sole source. Only `CoreMl.LocationOptions` contributes `ModelCacheDirectory`; no foreign provider receives that key. `Compatible` runs the two-step probe over the same snapshot against the compiled artifact's embedded compat info. `WarmStartAdmissible` requires an existing context artifact and exactly `EP_SUPPORTED_OPTIMAL` read from that artifact; `EP_UNSUPPORTED`, `EP_SUPPORTED_PREFER_RECOMPILATION`, and `EP_NOT_APPLICABLE` compile fresh. `Veto` folds incompatibility reason, notes, and code per hardware device. `Resolve` reads the row roster by key, proves the native provider loaded through `Available`, and answers `Floor` otherwise. `ResultKey` stamps provider, runtime, precision, and the shared behavior-option fingerprint; external-library bytes participate through `ProviderLibrary.ContentKey`.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Domain.ContentHash`), BCL inbox
- Growth: a built-in accelerator is one `ExecutionProvider` row with its provider name, OS gate, `HardwareAffinity`, EP-option/session-key projections, and register delegate — including an accelerator whose native provider this platform's runtime may not carry, because `Available` and `Resolve` already answer absence and no caller-facing surface moves. That row reaches every wire the moment it declares a `WireKey`; before it declares one, `FromWire` degrades the spelling it answers to onto the floor exactly as it degrades an unloaded provider. Out-of-tree accelerators enter through `External`, which brackets `RegisterExecutionProviderLibrary`/`UnregisterExecutionProviderLibrary` in one `ProviderLibrary` and returns a row using the same generic registration path. Each quantization posture is one `ModelPrecision` row folded into the same registration and fingerprint rails; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on `AutoSelect`.
- Boundary: each row owns one provider-specific fallback registration and one autoEP device registration path selected by a caller-held device snapshot, so one lease appends one provider. `CoreMl` and `WebGpu` use the generic `AppendExecutionProvider(name, options)` spelling for one measured reason: neither carries a dedicated managed method nor an exported C append entry (the pinned dylib exports exactly `_CPU` and `_CoreML` beside `OrtGetApiBase`, and no `_WebGPU`), so the string append is their only path — `CoreMl` because its row owns `ModelFormat`, compute units, specialization, cache directory, and precision beyond the flags overload, `WebGpu` because the EP has no other spelling at all; the `CoreMl` flags overload never runs beside its row. `WireKey` stays out of `OptionsFor` and out of `ResultKey`: it names the row for a boundary record and reaches nothing the built session does, so a rename changing no execution re-keys no cached result. Location options affect native artifact placement but stay out of result identity, while EP/session/precision options enter `OptionsFor`. `ProviderLibrary` rejects blank identities or an absent asset, hashes the registered bytes, unregisters once through `Interlocked.Exchange`, and threads its content identity into the dynamic row's behavior options. `HostGate` expresses row-specific OS capability while `GetAvailableProviders` proves the loaded native provider. `Full` leaves `mlas.enable_gemm_fastmath_arm64_bfloat16` disabled; `Bf16` alone sets it. Precision also reaches CoreML low-precision accumulation and MatMulNBits accuracy, and every behavior option participates in `ModelFingerprint.Of`. Compatibility consumes `OrtCompiledModelCompatibility` directly and admits reuse only for an existing `EP_SUPPORTED_OPTIMAL` artifact.

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExecutionProvider {
    public sealed class ProviderLibrary : IDisposable {
        readonly string registrationName;
        int released;

        ProviderLibrary(string registrationName, UInt128 contentKey) =>
            (this.registrationName, ContentKey) = (registrationName, contentKey);

        public UInt128 ContentKey { get; }

        public static Fin<ProviderLibrary> Admit(string registrationName, string libraryPath) =>
            string.IsNullOrWhiteSpace(registrationName) || string.IsNullOrWhiteSpace(libraryPath) || !File.Exists(libraryPath)
                ? Fin.Fail<ProviderLibrary>(new ComputeFault.ExtensionAssetMissing(libraryPath))
                : Try.lift(() => {
                    UInt128 contentKey = ContentHash.Of(File.ReadAllBytes(libraryPath));
                    OrtEnv.Instance().RegisterExecutionProviderLibrary(registrationName, libraryPath);
                    return new ProviderLibrary(registrationName, contentKey);
                }).Run().MapFail(error => new ComputeFault.ModelRejected(error.Message));

        public void Dispose() {
            if (Interlocked.Exchange(ref released, 1) is 0) {
                OrtEnv.Instance().UnregisterExecutionProviderLibrary(registrationName);
            }
        }
    }

    // ModelFormat is PINNED to MLProgram: the NeuralNetwork format the row would otherwise default to executes an
    // fp32 graph at fp16 on the ANE without saying so, so the pin is what keeps precision a declared posture rather
    // than a silent host decision — and it is why AllowLowPrecisionAccumulationOnGPU tracks the precision row
    // instead of standing open. Every claim this row makes about numeric agreement is answerable to a floor-provider
    // residual measured at the run.
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

    public static readonly ExecutionProvider Cpu = new(
        "cpu", providerName: "CPUExecutionProvider", wireKey: "cpu", hostGate: static () => true,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, locationOptions: static _ => FrozenDictionary<string, string>.Empty,
        sessionKeys: CpuSessionKeys,
        devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        registerRow: static (options, _) => options.AppendExecutionProvider_CPU(1));

    public static readonly ExecutionProvider Cuda = Accelerator(
        "cuda", "CUDAExecutionProvider", OrtHardwareDeviceType.GPU, static () => true,
        static options => options.AppendExecutionProvider_CUDA(0));

    public static readonly ExecutionProvider DirectMl = Accelerator(
        "directml", "DmlExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsWindows,
        static options => options.AppendExecutionProvider_DML(0));

    public static readonly ExecutionProvider TensorRt = Accelerator(
        "tensorrt", "TensorrtExecutionProvider", OrtHardwareDeviceType.GPU, static () => true,
        static options => options.AppendExecutionProvider_Tensorrt(0));

    public static readonly ExecutionProvider Rocm = Accelerator(
        "rocm", "ROCMExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsLinux,
        static options => options.AppendExecutionProvider_ROCm(0));

    public static readonly ExecutionProvider CoreMl = new(
        "coreml", providerName: "CoreMLExecutionProvider", wireKey: "coreMl",
        hostGate: static () => OperatingSystem.IsMacOSVersionAtLeast(12),
        epOptions: CoreMlOptions,
        locationOptions: static cacheDir => new Dictionary<string, string>(StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir }.ToFrozenDictionary(StringComparer.Ordinal),
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty,
        devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU), hardwareAffinity: OrtHardwareDeviceType.NPU,
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
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty,
        devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_GPU), hardwareAffinity: OrtHardwareDeviceType.GPU,
        registerRow: static (options, rows) => options.AppendExecutionProvider(
            "WebGPU", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

    public static readonly ExecutionProvider OpenVino = Accelerator(
        "openvino", "OpenVINOExecutionProvider", OrtHardwareDeviceType.NPU, static () => true,
        static options => options.AppendExecutionProvider_OpenVINO(string.Empty));

    public static readonly ExecutionProvider MiGraphX = Accelerator(
        "migraphx", "MIGraphXExecutionProvider", OrtHardwareDeviceType.GPU, OperatingSystem.IsLinux,
        static options => options.AppendExecutionProvider_MIGraphX(0));

    public static readonly ExecutionProvider Nnapi = Accelerator(
        "nnapi", "NnapiExecutionProvider", OrtHardwareDeviceType.NPU, OperatingSystem.IsAndroid,
        static options => options.AppendExecutionProvider_Nnapi(NnapiFlags.NNAPI_FLAG_USE_NONE));

    public static readonly ExecutionProvider Dnnl = Accelerator(
        "dnnl", "DnnlExecutionProvider", OrtHardwareDeviceType.CPU, static () => true,
        static options => options.AppendExecutionProvider_Dnnl(1));

    private ExecutionProvider(
        string key, string providerName, Option<string> wireKey, Func<bool> hostGate,
        Func<ModelPrecision, FrozenDictionary<string, string>> epOptions,
        Func<string, FrozenDictionary<string, string>> locationOptions,
        Func<ModelPrecision, FrozenDictionary<string, string>> sessionKeys,
        Option<ExecutionProviderDevicePolicy> devicePolicy, OrtHardwareDeviceType hardwareAffinity,
        Action<SessionOptions, IReadOnlyDictionary<string, string>> registerRow) : this(key) =>
        (ProviderName, WireKey, HostGate, EpOptions, LocationOptions, SessionKeys, DevicePolicy, HardwareAffinity, RegisterRow) =
        (providerName, wireKey, hostGate, epOptions, locationOptions, sessionKeys, devicePolicy, hardwareAffinity, registerRow);

    public string ProviderName { get; }
    public Option<string> WireKey { get; }
    public Func<bool> HostGate { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> EpOptions { get; }
    public Func<string, FrozenDictionary<string, string>> LocationOptions { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> SessionKeys { get; }
    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public OrtHardwareDeviceType HardwareAffinity { get; }
    public Action<SessionOptions, IReadOnlyDictionary<string, string>> RegisterRow { get; }

    public bool Available =>
        HostGate()
        && OrtEnv.Instance().GetAvailableProviders().Contains(ProviderName, StringComparer.Ordinal);

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

    public Seq<OrtEpDevice> Devices =>
        toSeq(OrtEnv.Instance().GetEpDevices()).Filter(device => StringComparer.Ordinal.Equals(device.EpName, ProviderName));

    public Seq<OrtEpDevice> AutoSelect =>
        Devices.OrderByDescending(device =>
            device.HardwareDevice.Type == HardwareAffinity ? 2
            : device.HardwareDevice.Type == OrtHardwareDeviceType.CPU ? 0
            : 1)
        .ThenBy(static device => device.EpName, StringComparer.Ordinal)
        .ThenBy(static device => device.EpVendor, StringComparer.Ordinal)
        .ThenBy(static device => device.HardwareDevice.VendorId)
        .ThenBy(static device => device.HardwareDevice.DeviceId)
        .ToSeq();

    public Seq<(OrtHardwareDeviceType Device, OrtDeviceEpIncompatibilityReason Reason, string Notes, int Code)> Veto =>
        toSeq(OrtEnv.Instance().GetHardwareDevices()).Map(device => {
            using OrtDeviceEpIncompatibilityDetails details = OrtEnv.Instance().GetHardwareDeviceEpIncompatibilityDetails(ProviderName, device);
            return (Device: device.Type, Reason: details.ReasonsBitmask, Notes: details.Notes, Code: details.ErrorCode);
        }).Filter(static row => row.Reason != OrtDeviceEpIncompatibilityReason.None);

    public FrozenDictionary<string, string> OptionsFor(ModelPrecision precision) =>
        EpOptions(precision).Concat(SessionKeys(precision)).Concat(precision.QdqKeys)
            .ToFrozenDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal);

    public void Register(SessionOptions options, string cacheDir, ModelPrecision precision, Seq<OrtEpDevice> devices) {
        toSeq(SessionKeys(precision).Concat(precision.QdqKeys)).Iter(entry => options.AddSessionConfigEntry(entry.Key, entry.Value));
        Dictionary<string, string> registerOptions = new(EpOptions(precision), StringComparer.Ordinal);
        toSeq(LocationOptions(cacheDir)).Iter(entry => registerOptions[entry.Key] = entry.Value);
        if (devices.IsEmpty) { RegisterRow(options, registerOptions); }
        else {
            options.AppendExecutionProvider(OrtEnv.Instance(), devices.ToList(), registerOptions);
        }
    }

    // Compat info is embedded at compile time: the probe reads the COMPILED EP-context artifact, never the uncompiled source model.
    public Option<OrtCompiledModelCompatibility> Compatible(string compiledModelPath, Seq<OrtEpDevice> devices) =>
        devices.IsEmpty
            ? None
            : Some(OrtEnv.Instance().GetModelCompatibilityForEpDevices(
                devices.ToList(), OrtEnv.Instance().GetCompatibilityInfoFromModel(compiledModelPath, ProviderName)));

    public bool WarmStartAdmissible(string contextPath, Seq<OrtEpDevice> devices) =>
        File.Exists(contextPath)
        && Compatible(contextPath, devices).Case is OrtCompiledModelCompatibility.EP_SUPPORTED_OPTIMAL;

    public string ResultKey(string ortVersion, ModelPrecision precision) =>
        $"{Key}:{ortVersion}:{precision.Key}:{ModelFingerprint.Of(OptionsFor(precision)):x16}";

    public static Fin<(ExecutionProvider Provider, ProviderLibrary Library)> External(
        string key,
        string providerName,
        string registrationName,
        string libraryPath,
        OrtHardwareDeviceType affinity) =>
        string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(providerName)
            ? Fin.Fail<(ExecutionProvider Provider, ProviderLibrary Library)>(new ComputeFault.ModelRejected("<external-provider-identity>"))
            : ProviderLibrary.Admit(registrationName, libraryPath).Map(library => (
                new ExecutionProvider(
                    key,
                    providerName,
                    Option<string>.None,
                    static () => true,
                    _ => new Dictionary<string, string>(StringComparer.Ordinal) {
                        ["external.library.content"] = library.ContentKey.ToString("x32", CultureInfo.InvariantCulture),
                    }.ToFrozenDictionary(StringComparer.Ordinal),
                    static _ => FrozenDictionary<string, string>.Empty,
                    static _ => FrozenDictionary<string, string>.Empty,
                    Option<ExecutionProviderDevicePolicy>.None,
                    affinity,
                    (options, rows) => options.AppendExecutionProvider(
                        providerName, new Dictionary<string, string>(rows, StringComparer.Ordinal))),
                library));

    // Built-in accelerators carry NO wire spelling until a boundary record names one: rows exist so `Available`
    // answers for the host, and a spelling nobody sends puts an unreachable key on every wire reading this roster.
    // `wireKey` defaults to `None`, so admitting a row to a wire is one argument.
    static ExecutionProvider Accelerator(
        string key,
        string providerName,
        OrtHardwareDeviceType affinity,
        Func<bool> hostGate,
        Action<SessionOptions> register,
        Option<string> wireKey = default) =>
        new(
            key,
            providerName,
            wireKey,
            hostGate,
            static _ => FrozenDictionary<string, string>.Empty,
            static _ => FrozenDictionary<string, string>.Empty,
            static _ => FrozenDictionary<string, string>.Empty,
            Option<ExecutionProviderDevicePolicy>.None,
            affinity,
            (options, _) => register(options));
}
```

`OrtEpDevice` (enumerated through `OrtEnv.GetEpDevices()`) carries the columns the `Devices`/`AutoSelect` fold reads:

| [INDEX] | [MEMBER]                       | [CARRIES]                                                                                      |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `OrtEpDevice.EpName`           | provider name keyed against the `ExecutionProvider` row                                        |
|  [02]   | `OrtEpDevice.EpVendor`         | EP vendor string                                                                               |
|  [03]   | `OrtEpDevice.HardwareDevice`   | `OrtHardwareDevice` — `Type` (`CPU`/`GPU`/`NPU`), `VendorId`, `DeviceId`, `Vendor`, `Metadata` |
|  [04]   | `OrtEpDevice.EpMetadata`       | `OrtKeyValuePairs` EP self-description                                                         |
|  [05]   | `OrtEpDevice.EpOptions`        | `OrtKeyValuePairs` default EP option set                                                       |
|  [06]   | `OrtEpDevice.GetMemoryInfo(OrtDeviceMemoryType)` | `OrtMemoryInfo` for the device allocation at the named memory type                |
|  [07]   | `OrtEpDevice.CreateSyncStream` | `OrtSyncStream` tying a device-stream lifetime to the device                                   |

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

- [COREML_PRECISION_RESIDUAL]-[OPEN]: what residual does the `CoreMl` row's MLProgram path reach against a `Cpu` run of the same graph at `ModelPrecision.Full`, per stage class; measure through the `Model/inference#STAGE_EXECUTION` canary comparison on the ORT host and record the observed band per model family rather than asserting a tolerance here.
