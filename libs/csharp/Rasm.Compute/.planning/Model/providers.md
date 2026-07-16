# [COMPUTE_PROVIDERS]

Rasm.Compute model execution-provider axis: `ExecutionProvider` rows span every admitted ONNX Runtime registration family, autoEP `OrtEpDevice` discovery ranks hardware deterministically by each row's affinity, and row-owned `HostGate` predicates compose provider discovery with OS capability. `ProviderLibrary` brackets and content-keys an out-of-tree EP registration, and `External` mints its row on the same axis. `ModelPrecision` folds fp16/bf16/int8/int4 policy into provider and EP-agnostic session options. A two-step typed `OrtCompiledModelCompatibility` probe reads the compatibility info a COMPILED EP-context artifact embeds against the same device snapshot registration uses, and gates warm-start only when that artifact exists and the selected device reports `EP_SUPPORTED_OPTIMAL` — probing the uncompiled source model carries no embedded compat info and answers `EP_NOT_APPLICABLE`, the dead-warm-start defect.

`ExecutionProvider` rows, `ModelPrecision` rows, and the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`ResultKey` fold are the owned surface; the Thinktecture `ComparerAccessors.StringOrdinal` key accessor arrives settled. Provider, device, and compatibility members ride `Microsoft.ML.OnnxRuntime`; option identity composes `ModelFingerprint.Of`, and negative-cache posture uses `NodaTime.Duration`. `ExecutionProvider` and `ModelPrecision` cross to sessions, inference, and generative owners as settled vocabulary.

## [01]-[INDEX]

- [01]-[EP_AXIS]: execution-provider rows and autoEP `OrtEpDevice` discovery over one polymorphic register, a typed two-step compatibility probe, and precision-folded quantization knobs.

## [02]-[EP_AXIS]

- Owner: `ExecutionProvider` `[SmartEnum<string>]` rows (provider name, host gate, precision-keyed EP options, location options, session keys, device policy, hardware affinity, register delegate); `ModelPrecision` `[SmartEnum<string>]` rows (low-precision accumulation, BF16 fast math, `session.qdq_matmulnbits_accuracy_level`, negative TTL); the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`ResultKey` fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `Cuda`, `DirectMl`, `TensorRt`, `Rocm`, `CoreMl`, `OpenVino`, `MiGraphX`, `Nnapi`, `Dnnl`; `ModelPrecision` rows `Full`, `Fp16`, `Bf16`, `Int8`, `Int4`.
- Auto: `Available` short-circuits on `HostGate` before `GetAvailableProviders`; `AutoSelect` ranks devices by row-owned `HardwareAffinity`, then CPU last, then provider/vendor/device identity for deterministic ties. One selected-device snapshot passes through `Register`, `Compatible`, and `WarmStartAdmissible`. `Register` folds session keys and precision `QdqKeys`, composes row-owned EP and location option tables, then uses direct autoEP registration when the snapshot is non-empty or the row's verified fallback registration otherwise. Only `CoreMl.LocationOptions` contributes `ModelCacheDirectory`; no foreign provider receives that key. `Compatible` runs the two-step probe over the same snapshot against the compiled artifact's embedded compat info. `WarmStartAdmissible` requires an existing context artifact and exactly `EP_SUPPORTED_OPTIMAL` read from that artifact; `EP_UNSUPPORTED`, `EP_SUPPORTED_PREFER_RECOMPILATION`, and `EP_NOT_APPLICABLE` compile fresh. `Veto` folds incompatibility reason, notes, and code per hardware device. `ResultKey` stamps provider, runtime, precision, and the shared behavior-option fingerprint; external-library bytes participate through `ProviderLibrary.ContentKey`.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Domain.ContentHash`), BCL inbox
- Growth: a built-in accelerator is one `ExecutionProvider` row with its provider name, OS gate, `HardwareAffinity`, EP-option/session-key projections, and register delegate. An out-of-tree accelerator enters through `External`, which brackets `RegisterExecutionProviderLibrary`/`UnregisterExecutionProviderLibrary` in one `ProviderLibrary` and returns a row using the same generic registration path. A quantization posture is one `ModelPrecision` row folded into the same registration and fingerprint rails; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on `AutoSelect`.
- Boundary: each row owns one provider-specific fallback registration and one autoEP device registration path selected by a caller-held device snapshot, so one lease appends one provider. `CoreMl` alone uses the generic `AppendExecutionProvider("CoreMLExecutionProvider", options)` spelling because its row owns `ModelFormat`, compute units, specialization, cache directory, and precision; the flags overload never runs beside it. Location options affect native artifact placement but stay out of result identity, while EP/session/precision options enter `OptionsFor`. `ProviderLibrary` rejects blank identities or an absent asset, hashes the registered bytes, unregisters once through `Interlocked.Exchange`, and threads its content identity into the dynamic row's behavior options. `HostGate` expresses row-specific OS capability while `GetAvailableProviders` proves the loaded native provider. `Full` leaves `mlas.enable_gemm_fastmath_arm64_bfloat16` disabled; `Bf16` alone sets it. Precision also reaches CoreML low-precision accumulation and MatMulNBits accuracy, and every behavior option participates in `ModelFingerprint.Of`. Compatibility consumes `OrtCompiledModelCompatibility` directly and admits reuse only for an existing `EP_SUPPORTED_OPTIMAL` artifact.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelPrecision {
    // A precision row is EXECUTION POSTURE, never a graph transform: Int8/Int4 demand settled pre-quantized model
    // bytes (the quantized graph is its own checksum identity) and select the MatMulNBits accuracy floor plus the
    // accumulation posture; `QuantizedGraph` is the admission evidence session `Options` gates on.
    public static readonly ModelPrecision Full = new("full", lowPrecisionAccumulation: false, bfloat16FastMath: false, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(15));
    public static readonly ModelPrecision Fp16 = new("fp16", lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Bf16 = new("bf16", lowPrecisionAccumulation: true, bfloat16FastMath: true, accuracyLevel: Option<int>.None, quantizedGraph: false, negativeTtl: Duration.FromMinutes(10));
    public static readonly ModelPrecision Int8 = new("int8", lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: 4, quantizedGraph: true, negativeTtl: Duration.FromMinutes(5));
    public static readonly ModelPrecision Int4 = new("int4", lowPrecisionAccumulation: true, bfloat16FastMath: false, accuracyLevel: 4, quantizedGraph: true, negativeTtl: Duration.FromMinutes(2));

    public bool LowPrecisionAccumulation { get; }
    public bool Bfloat16FastMath { get; }
    public Option<int> AccuracyLevel { get; }
    public bool QuantizedGraph { get; }
    public Duration NegativeTtl { get; }

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
        "cpu", providerName: "CPUExecutionProvider", hostGate: static () => true,
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
        "coreml", providerName: "CoreMLExecutionProvider", hostGate: static () => OperatingSystem.IsMacOSVersionAtLeast(12),
        epOptions: CoreMlOptions,
        locationOptions: static cacheDir => new Dictionary<string, string>(StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir }.ToFrozenDictionary(StringComparer.Ordinal),
        sessionKeys: static _ => FrozenDictionary<string, string>.Empty,
        devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU), hardwareAffinity: OrtHardwareDeviceType.NPU,
        registerRow: static (options, rows) => options.AppendExecutionProvider(
            "CoreMLExecutionProvider", new Dictionary<string, string>(rows, StringComparer.Ordinal)));

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

    public string ProviderName { get; }
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

    static ExecutionProvider Accelerator(
        string key,
        string providerName,
        OrtHardwareDeviceType affinity,
        Func<bool> hostGate,
        Action<SessionOptions> register) =>
        new(
            key,
            providerName,
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
|  [06]   | `OrtEpDevice.GetMemoryInfo`    | `OrtMemoryInfo` for the device's default allocation                                            |
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

(none)
