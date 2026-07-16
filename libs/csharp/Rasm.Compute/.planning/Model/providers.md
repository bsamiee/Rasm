# [COMPUTE_PROVIDERS]

Rasm.Compute model execution-provider axis: `ExecutionProvider` rows span CPU and the Apple-silicon CoreML row, autoEP `OrtEpDevice` discovery ranks hardware by the row's own affinity under an optional policy delegate, and `ModelPrecision` folds int8/int4 quantization into both the CoreML `AllowLowPrecisionAccumulationOnGPU` flag and the EP-agnostic `session.qdq_matmulnbits_accuracy_level` MatMulNBits knob. A two-step typed `OrtCompiledModelCompatibility` probe gates the warm-start read and emits an `OrtDeviceEpIncompatibilityReason` veto receipt for each device a row cannot claim.

`ComparerAccessors.StringOrdinal`, the `ExecutionProvider` rows (probe, OS gate, EP-option, session-key, device-policy, hardware-affinity, register columns), the `ModelPrecision` rows (accumulation, accuracy-level, negative-TTL columns), and the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`ResultKey` fold are the owned surface. Provider, device, and compatibility surfaces ride `Microsoft.ML.OnnxRuntime`, the option fingerprint the `Model/identity#MODEL_IDENTITY` `ModelFingerprint.Of` projection, the negative-TTL posture `NodaTime` `Duration`; the GPU `Cuda`/`DirectMl` member spellings stay the design record. `ExecutionProvider`/`ModelPrecision` crosses to `Model/sessions#SESSION_CAPSULE`, `Model/inference#RESULT_CACHE`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary.

## [01]-[INDEX]

- [01]-[EP_AXIS]: execution-provider rows and autoEP `OrtEpDevice` discovery over one polymorphic register, a typed two-step compatibility probe, and precision-folded quantization knobs.

## [02]-[EP_AXIS]

- Owner: `ComparerAccessors.StringOrdinal`; `ExecutionProvider` `[SmartEnum<string>]` rows (provider name, macOS gate, precision-keyed EP-option projection, session-key table, CoreML flag, device policy, hardware-affinity, register delegate); `ModelPrecision` `[SmartEnum<string>]` int8/int4 rows (low-precision-accumulation, `session.qdq_matmulnbits_accuracy_level`, negative-TTL); the `Devices`/`AutoSelect`/`Veto`/`Register` discovery-and-register fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `CoreMl`; `ModelPrecision` rows `Full` · `Int8` · `Int4`.
- Auto: `Available` reads the `GetAvailableProviders` probe plus the macOS gate on `MinMacOsMajor`; `AutoSelect` ranks `Devices` by the row's OWN `HardwareAffinity` — affinity type first, `CPU` terminal, every other accelerator between — so `CoreMl` ranks an `NPU` first and a future GPU-affinity row a `GPU` with zero rank re-coding, and a hardcoded `NPU≻GPU≻CPU` switch ignoring `HardwareAffinity` misranks the GPU EP and is the deleted form. `Register(options, cacheDir, precision)` is ONE polymorphic registration — it folds the session-key table and precision `QdqKeys` through `AddSessionConfigEntry`, then on `AutoSelect.IsEmpty` either registers the non-empty device list through `AppendExecutionProvider(env, devices, EpOptions(precision))` or drives the row's `RegisterRow` delegate, never two parallel register surfaces. `Compatible(modelPath)` reads `GetCompatibilityInfoFromModel(modelPath, ProviderName)` then `GetModelCompatibilityForEpDevices(AutoSelect, info)` for the typed verdict, and `WarmStartAdmissible` keeps the EP-context path enabled unless an existing blob is `EP_UNSUPPORTED`/`EP_SUPPORTED_PREFER_RECOMPILATION` — only those recompile fresh, while a missing blob, `EP_SUPPORTED_OPTIMAL`, and `EP_NOT_APPLICABLE` keep it enabled. `Veto` folds `GetHardwareDeviceEpIncompatibilityDetails` into the typed reason+notes receipt per device. `ResultKey(ortVersion, precision)` stamps EP key, ORT version, the `ModelPrecision` key, and `ModelFingerprint.Of(OptionsFor(precision))` for the deterministic cache key. Precision folds at TWO sinks — the CoreML `AllowLowPrecisionAccumulationOnGPU` flips from `LowPrecisionAccumulation`, the EP-agnostic `session.qdq_matmulnbits_accuracy_level` carries `AccuracyLevel` — both through `OptionsFor` so a quantized session keys distinctly.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new accelerator is one `ExecutionProvider` row with its provider name, OS gate, `HardwareAffinity`, EP-option/session-key projections, and register delegate; a new quantization posture is one `ModelPrecision` row folded into the same registration and fingerprint rails, never a second session-options owner; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on `AutoSelect`, never a second selection owner; the generative token-streaming successor lands as the `Model/generative#GENERATIVE_RUN` cluster composing this axis, never a chat-client surface.
- Boundary: `AppendExecutionProvider_CoreML(CoreMLFlags)` is the typed registration carrying `CoreMlFlag`, and `AppendExecutionProvider("CoreMLExecutionProvider", options)` is the option-rich fallback for `ModelCacheDirectory`/`MLComputeUnits`/`SpecializationStrategy` — a bare `"CoreML"` name faults `InvalidArgument`. `AppendExecutionProvider(OrtEnv, IReadOnlyList<OrtEpDevice>, IReadOnlyDictionary<string,string>)` is the direct-device registration `Register` drives once `AutoSelect` is non-empty, and `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` overrides the enum `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` for a custom ranking — the delegate receives `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint)` and returns the ranked `List<OrtEpDevice>`. Live axis is `Cpu` and `CoreMl` on a no-GPU host; the `Cuda`/`DirectMl` rows carry `AppendExecutionProvider_CUDA(0)`/`AppendExecutionProvider_DML(0)` as the design record on `[EP_EXECUTION]`, re-entering only behind a GPU-carrying RID. macOS gate rides `MinMacOsMajor` because the legacy NeuralNetwork format reaches back to macOS 10.15. `MLComputeUnits` domain is `ALL`/`CPUAndGPU`/`CPUAndNeuralEngine`/`CPUOnly` and `SpecializationStrategy` is `Default`/`FastPrediction`; the default flag is `COREML_FLAG_USE_NONE`, `COREML_FLAG_CREATE_MLPROGRAM` matches `ModelFormat=MLProgram`, and `COREML_FLAG_USE_CPU_AND_GPU` (`UInt32` 32) opens the CPU-and-GPU path. `ModelCacheDirectory` binds at registration to the blob-lane artifact directory so compiled CoreML caches are catalogued. Precision folds at TWO sinks, never one — the int4/int8 WEIGHT quantization is the packaged ONNX graph's own property (ORT executes the exported quantized operators, never a runtime re-quantization pass) and `Int8`/`Int4` carry accuracy `4`, the runtime default; a managed re-quantization kernel, a single-knob claim, or a second session-options owner is the rejected form. `mlas.enable_gemm_fastmath_arm64_bfloat16` is the `Cpu` row's Apple-silicon fp32-GEMM bf16 knob. Compatibility is the typed `OrtCompiledModelCompatibility` enum read through the two-step contract — a `verdict.ToString().Contains("Incompatible")` substring match (no enum name carries that substring), a model path where the compat-info string is required, and an `Option<string>` verdict are the named defects. Option fingerprint rides `ModelFingerprint.Of(OptionsFor(precision))` — a re-derived `XxHash3` body, or a precision-independent hash XORed with a per-precision overlay, is the deleted form. `Cpu` (`MinMacOsMajor` 0, always `Available`) is the implicit terminal of the degrade chain. Per-row `Available` is what a composition root AGGREGATES into the single `onnx` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` — present iff at least one `ExecutionProvider.Available` holds; a populator pushing the raw ORT EP names (`CPUExecutionProvider`/`CoreMLExecutionProvider`) into `Providers`, or inferring `onnx` from a `Providers` emptiness test, is the deleted contribution form, and dylib-presence heuristics are the deleted probe form.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelPrecision {
    public static readonly ModelPrecision Full = new("full", lowPrecisionAccumulation: false, accuracyLevel: Option<int>.None, negativeTtl: Duration.FromMinutes(15));
    public static readonly ModelPrecision Int8 = new("int8", lowPrecisionAccumulation: true, accuracyLevel: 4, negativeTtl: Duration.FromMinutes(5));
    public static readonly ModelPrecision Int4 = new("int4", lowPrecisionAccumulation: true, accuracyLevel: 4, negativeTtl: Duration.FromMinutes(2));

    public bool LowPrecisionAccumulation { get; }
    public Option<int> AccuracyLevel { get; }
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
    static readonly FrozenDictionary<string, string> CoreMlRows = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["ModelFormat"] = "MLProgram",
        ["MLComputeUnits"] = "ALL",
        ["RequireStaticInputShapes"] = "0",
        ["EnableOnSubgraphs"] = "0",
        ["SpecializationStrategy"] = "Default",
        ["ProfileComputePlan"] = "0",
        ["AllowLowPrecisionAccumulationOnGPU"] = "0",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> CpuSessionKeys = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["mlas.enable_gemm_fastmath_arm64_bfloat16"] = "1",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static FrozenDictionary<string, string> CoreMlOptions(ModelPrecision precision) =>
        new Dictionary<string, string>(CoreMlRows, StringComparer.Ordinal) {
            ["AllowLowPrecisionAccumulationOnGPU"] = precision.LowPrecisionAccumulation ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static readonly ExecutionProvider Cpu = new(
        "cpu", providerName: "CPUExecutionProvider", minMacOsMajor: 0,
        epOptions: static _ => FrozenDictionary<string, string>.Empty, sessionKeys: static _ => CpuSessionKeys,
        coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        registerRow: static (_, options, _, _) => options.AppendExecutionProvider_CPU(1));

    public static readonly ExecutionProvider CoreMl = new(
        "coreml", providerName: "CoreMLExecutionProvider", minMacOsMajor: 12,
        epOptions: CoreMlOptions, sessionKeys: static _ => FrozenDictionary<string, string>.Empty,
        coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU), hardwareAffinity: OrtHardwareDeviceType.NPU,
        registerRow: static (self, options, cacheDir, precision) => {
            options.AppendExecutionProvider_CoreML(self.CoreMlFlag);
            options.AppendExecutionProvider(self.ProviderName,
                new Dictionary<string, string>(self.EpOptions(precision), StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir });
        });

    public string ProviderName { get; }
    public int MinMacOsMajor { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> EpOptions { get; }
    public Func<ModelPrecision, FrozenDictionary<string, string>> SessionKeys { get; }
    public CoreMLFlags CoreMlFlag { get; }
    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public OrtHardwareDeviceType HardwareAffinity { get; }
    public Action<ExecutionProvider, SessionOptions, string, ModelPrecision> RegisterRow { get; }

    public bool Available =>
        OrtEnv.Instance().GetAvailableProviders().Contains(ProviderName, StringComparer.Ordinal)
        && (MinMacOsMajor is 0 || OperatingSystem.IsMacOSVersionAtLeast(MinMacOsMajor));

    public Seq<OrtEpDevice> Devices =>
        toSeq(OrtEnv.Instance().GetEpDevices()).Filter(device => StringComparer.Ordinal.Equals(device.EpName, ProviderName));

    public Seq<OrtEpDevice> AutoSelect =>
        Devices.OrderByDescending(device =>
            device.HardwareDevice.Type == HardwareAffinity ? 2
            : device.HardwareDevice.Type == OrtHardwareDeviceType.CPU ? 0
            : 1).ToSeq();

    public Seq<(OrtHardwareDeviceType Device, OrtDeviceEpIncompatibilityReason Reason, string Notes, int Code)> Veto =>
        toSeq(OrtEnv.Instance().GetHardwareDevices()).Map(device => {
            using var details = OrtEnv.Instance().GetHardwareDeviceEpIncompatibilityDetails(ProviderName, device);
            return (Device: device.Type, Reason: details.ReasonsBitmask, Notes: details.Notes, Code: details.ErrorCode);
        }).Filter(static row => row.Reason != OrtDeviceEpIncompatibilityReason.None);

    public FrozenDictionary<string, string> OptionsFor(ModelPrecision precision) =>
        EpOptions(precision).Concat(SessionKeys(precision)).Concat(precision.QdqKeys)
            .ToFrozenDictionary(static row => row.Key, static row => row.Value, StringComparer.Ordinal);

    public void Register(SessionOptions options, string cacheDir, ModelPrecision precision) {
        toSeq(SessionKeys(precision).Concat(precision.QdqKeys)).Iter(entry => options.AddSessionConfigEntry(entry.Key, entry.Value));
        if (AutoSelect.IsEmpty) { RegisterRow(this, options, cacheDir, precision); }
        else {
            options.AppendExecutionProvider(
                OrtEnv.Instance(), AutoSelect.ToList(),
                new Dictionary<string, string>(EpOptions(precision), StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir });
        }
    }

    public Option<OrtCompiledModelCompatibility> Compatible(string modelPath) =>
        AutoSelect.IsEmpty
            ? None
            : Some(OrtEnv.Instance().GetModelCompatibilityForEpDevices(
                AutoSelect.ToList(), OrtEnv.Instance().GetCompatibilityInfoFromModel(modelPath, ProviderName)));

    public bool WarmStartAdmissible(string modelPath, string contextPath) =>
        !File.Exists(contextPath)
        || Compatible(modelPath).Case is not OrtCompiledModelCompatibility verdict
        || verdict is not (OrtCompiledModelCompatibility.EP_UNSUPPORTED or OrtCompiledModelCompatibility.EP_SUPPORTED_PREFER_RECOMPILATION);

    public string ResultKey(string ortVersion, ModelPrecision precision) =>
        $"{Key}:{ortVersion}:{precision.Key}:{ModelFingerprint.Of(OptionsFor(precision)):x16}";
}
```

`CoreMlFlag` binds the package `Microsoft.ML.OnnxRuntime.CoreMLFlags` `[Flags]` enum (`UInt32`) values:

| [INDEX] | [FLAG]                                       | [VALUE] |
| :-----: | :------------------------------------------- | :-----: |
|  [01]   | `COREML_FLAG_USE_NONE`                       |    0    |
|  [02]   | `COREML_FLAG_USE_CPU_ONLY`                   |    1    |
|  [03]   | `COREML_FLAG_ENABLE_ON_SUBGRAPH`             |    2    |
|  [04]   | `COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE`    |    4    |
|  [05]   | `COREML_FLAG_ONLY_ALLOW_STATIC_INPUT_SHAPES` |    8    |
|  [06]   | `COREML_FLAG_CREATE_MLPROGRAM`               |   16    |
|  [07]   | `COREML_FLAG_USE_CPU_AND_GPU`                |   32    |

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

- [EP_EXECUTION]-[BLOCKED]: the `Cuda`/`DirectMl` GPU rows register through `AppendExecutionProvider_CUDA(int)`/`AppendExecutionProvider_DML(int)` (device id `0`) and re-enter as one `ExecutionProvider` row each only behind a GPU-carrying RID — verify GPU device registration and the `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` warm-start verdict against live GPU hardware; blocked on a GPU-carrying host.
