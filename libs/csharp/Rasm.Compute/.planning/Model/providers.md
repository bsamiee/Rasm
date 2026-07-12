# [COMPUTE_PROVIDERS]

Rasm.Compute model execution-provider axis: the EP-parameterized provider rows across CPU and the Apple-silicon CoreML row, the autoEP `OrtEpDevice` hardware-device discovery with policy-delegate selection, the `ModelPrecision` int8/int4 quantization posture folded into BOTH the CoreML `AllowLowPrecisionAccumulationOnGPU` EP flag AND the EP-agnostic `session.qdq_matmulnbits_accuracy_level` MatMulNBits compute knob, the `mlas.enable_gemm_fastmath_arm64_bfloat16` Apple-silicon CPU throughput knob, and the two-step typed `OrtCompiledModelCompatibility` warm-start probe with its `OrtDeviceEpIncompatibilityReason` veto receipt. The page owns the `ComparerAccessors.StringOrdinal` accessor, the `ExecutionProvider` `[SmartEnum<string>]` rows with their probe/OS-gate/EP-option/session-key/device-policy/register columns, the `ModelPrecision` rows with their low-precision-accumulation, accuracy-level, and negative-TTL columns, and the `Devices`/`AutoSelect`/`Veto`/`OptionsFor`/`Register`/`Compatible`/`WarmStartAdmissible`/`ResultKey` autoEP discovery and one-polymorphic-register fold; the provider, device, and compatibility surfaces ride `Microsoft.ML.OnnxRuntime`, the option fingerprint rides the one `Model/identity#MODEL_IDENTITY` `ModelFingerprint.Of` ordinal-keyvalue projection, the negative-TTL posture rides `NodaTime` `Duration`, and the GPU `Cuda`/`DirectMl` member spellings stay the design record. The `ExecutionProvider`/`ModelPrecision` axis crosses to `Model/sessions#SESSION_CAPSULE`, `Model/inference#RESULT_CACHE`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary.

## [01]-[INDEX]

- [01]-[EP_AXIS]: execution-provider rows with probe, OS gate, EP-option and session-key tables; autoEP `OrtEpDevice` discovery, policy-delegate selection, the typed two-step compatibility probe and its veto receipt; precision-folded quantization knobs; one polymorphic register.

## [02]-[EP_AXIS]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `ExecutionProvider` `[SmartEnum<string>]` rows with provider name, macOS gate, precision-keyed EP-option projection, row session-key table, CoreML flag, device policy, hardware-device-type affinity, and register-delegate columns; `ModelPrecision` `[SmartEnum<string>]` int8/int4 rows with low-precision-accumulation, `session.qdq_matmulnbits_accuracy_level`, and negative-TTL columns; the `Devices`/`AutoSelect`/`Veto`/`Register` autoEP discovery + one-polymorphic-register fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `CoreMl`; `ModelPrecision` rows `Full` · `Int8` · `Int4`.
- Auto: `Available` reads the `GetAvailableProviders` probe plus the macOS-12 gate riding `MinMacOsMajor`; `Devices` folds `OrtEnv.GetEpDevices()` into the `OrtEpDevice` rows the host enumerates, `AutoSelect` ranks them by proximity to the row's OWN `HardwareAffinity` — the affinity device type ranks first, `CPU` is the terminal rank, every other accelerator sits between — so the `CoreMl` row prefers an `NPU` and a future GPU-affinity row prefers a `GPU` with zero rank re-coding (a hardcoded universal `NPU≻GPU≻CPU` switch that ignores the row's `HardwareAffinity` would misrank the GPU EP and is the deleted form), and `Register(options, cacheDir, precision)` is ONE polymorphic registration — it first folds the row session-key table and the precision `QdqKeys` through `AddSessionConfigEntry`, then discriminates on `AutoSelect.IsEmpty`: a non-empty device list registers through the device-list `AppendExecutionProvider(env, devices, EpOptions(precision))` overload, an empty list drives the row's `RegisterRow` delegate over the row's own `CoreMlFlag`/`ProviderName`/`EpOptions` columns — never two parallel register surfaces; the model-compatibility decision is the verified two-step contract — `Compatible(modelPath)` reads `GetCompatibilityInfoFromModel(modelPath, ProviderName)` for the per-EP compat-info string then `GetModelCompatibilityForEpDevices(AutoSelect, info)` for the typed `OrtCompiledModelCompatibility` verdict, and `WarmStartAdmissible(modelPath, contextPath)` keeps the EP-context path enabled unless an existing blob is `EP_UNSUPPORTED`/`EP_SUPPORTED_PREFER_RECOMPILATION` for the current devices — only those degrade to a fresh compile, while a missing blob (the first compile, which generates it), `EP_SUPPORTED_OPTIMAL` (read it), and `EP_NOT_APPLICABLE` (context moot) keep it enabled rather than forcing a needless recompile; `Veto` folds `GetHardwareDeviceEpIncompatibilityDetails` into the typed `OrtDeviceEpIncompatibilityReason`+notes degradation receipt for each hardware device the row cannot claim; `ResultKey(ortVersion, precision)` stamps EP key, ORT version, the `ModelPrecision` key, and `ModelFingerprint.Of(OptionsFor(precision))` — the precision-complete fingerprint over EP options, row session keys, and the qdq accuracy knob — for the deterministic cache key with zero call-site hashing; the precision posture folds into the registration rail at TWO sinks — the CoreML EP-option `AllowLowPrecisionAccumulationOnGPU` flips from `ModelPrecision.LowPrecisionAccumulation`, and the EP-agnostic `session.qdq_matmulnbits_accuracy_level` carries `ModelPrecision.AccuracyLevel` so the int4/int8 MatMulNBits compute floor is set once — and both ride `OptionsFor` so a quantized session keys distinctly from a full-precision one with zero call-site ceremony.
- Packages: Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new accelerator is one `ExecutionProvider` row with its provider name, OS gate, `HardwareAffinity`, EP-option/session-key projections, and register delegate — the GPU `Cuda`/`DirectMl` registration member spelling (`AppendExecutionProvider_CUDA(0)`/`AppendExecutionProvider_DML(0)`) stays the win/linux-x64 design record on the `[EP_EXECUTION]` RESEARCH row and re-enters as one row only on a host whose RID carries the GPU asset; a new model-quantization posture is one `ModelPrecision` row with its accumulation flag, accuracy level, and negative TTL folded into the same registration and fingerprint rails, never a second session-options owner; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on the `AutoSelect` fold, never a second selection owner; the generative token-streaming successor lands as the `Model/generative#GENERATIVE_RUN` run-mode cluster composing this EP axis, never a chat-client surface; zero new surface.
- Boundary: `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags)` is the canonical typed registration carrying the `CoreMlFlag` column, and `AppendExecutionProvider("CoreMLExecutionProvider", options)` is the option-rich fallback for the string-keyed `ModelCacheDirectory`/`MLComputeUnits`/`SpecializationStrategy` keys — a bare `"CoreML"` provider name faults `InvalidArgument` and is the deleted spelling; the autoEP device-list overload `AppendExecutionProvider(OrtEnv, IReadOnlyList<OrtEpDevice>, IReadOnlyDictionary<string,string>)` is the policy-free direct-device registration the `Register` fold drives once `AutoSelect` is non-empty after the host enumerates the device through `GetEpDevices()`, and `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` is the managed device-rank callback overriding the enum `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` when the row needs a custom ranking — the delegate receives `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint)` and returns the ranked `List<OrtEpDevice>`; the live axis is the two rows `Cpu` and `CoreMl` on a host with no GPU asset — the `Cuda`/`DirectMl` GPU rows carry their `AppendExecutionProvider_CUDA(0)`/`AppendExecutionProvider_DML(0)` member spelling as the design record on `[EP_EXECUTION]`, re-entering as a row only behind a GPU-carrying RID; the macOS-12 gate rides `MinMacOsMajor` because the legacy NeuralNetwork format alone reaches back to macOS 10.15; the `MLComputeUnits` value domain (`ALL`/`CPUAndGPU`/`CPUAndNeuralEngine`/`CPUOnly`) and the `SpecializationStrategy` value domain (`Default`/`FastPrediction`) ride the EP-option table, the default CoreML flag is `COREML_FLAG_USE_NONE`, `COREML_FLAG_CREATE_MLPROGRAM` is the MLProgram-backend column matching `ModelFormat=MLProgram`, and `COREML_FLAG_USE_CPU_AND_GPU` (the `UInt32` 32 flag) opens the CPU-and-GPU compute path; `ModelCacheDirectory` binds at registration to the blob-lane artifact directory so compiled CoreML caches are catalogued inventory; the `ModelPrecision` posture folds at TWO sinks, never one — `AllowLowPrecisionAccumulationOnGPU` is the CoreML-EP GPU-accumulation flag and `session.qdq_matmulnbits_accuracy_level` is the EP-agnostic MatMulNBits compute floor (`Int8`/`Int4` carry `4`, the runtime default), while the int4/int8 WEIGHT quantization itself is the packaged ONNX graph's own property (ORT executes the quantized operators the exported graph carries, never a runtime re-quantization pass) — a managed re-quantization kernel, a single-knob claim, or a second session-options owner is the rejected form; `mlas.enable_gemm_fastmath_arm64_bfloat16` is the `Cpu` row's Apple-silicon fp32-GEMM bf16 throughput knob carried in its session-key table; the compatibility verdict is the typed `OrtCompiledModelCompatibility` enum read through the two-step `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` contract — a `verdict.ToString().Contains("Incompatible")` substring match (no enum name carries that substring), a model path passed where the compat-info string is required, and an `Option<string>` verdict are the named defects; the option fingerprint rides `ModelFingerprint.Of(OptionsFor(precision))`, the one precision-complete projection — a re-derived `XxHash3` keyvalue body, or a precision-independent `OptionsHash` XORed with a per-precision overlay, is the deleted form; a vetoed row surfaces its `OrtDeviceEpIncompatibilityReason`+notes through `Veto` for the degradation receipt and `Cpu` (`MinMacOsMajor` 0, always `Available`) is the implicit terminal of the composition root's degrade chain; the per-row `Available` probe is what a composition root AGGREGATES into the single `onnx` substrate-capability key on `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Providers` — the `onnx` key is present iff at least one `ExecutionProvider.Available` holds (the ORT runtime reports an execution provider), so the populator emits the one substrate key the `Substrate.Onnx` `!Providers.Contains(Key)` gate reads and a populator pushing the raw ORT EP names (`CPUExecutionProvider`/`CoreMLExecutionProvider`) into `Providers`, or inferring `onnx` from a `Providers` emptiness test rather than the substrate key, is the deleted contribution form; dylib-presence heuristics are the deleted probe form.

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

The `CoreMlFlag` column binds the package `Microsoft.ML.OnnxRuntime.CoreMLFlags` `[Flags]` enum (`UInt32`) values:

| [INDEX] | [FLAG]                                       | [VALUE] |
| :-----: | :------------------------------------------- | :-----: |
|  [01]   | `COREML_FLAG_USE_NONE`                       |    0    |
|  [02]   | `COREML_FLAG_USE_CPU_ONLY`                   |    1    |
|  [03]   | `COREML_FLAG_ENABLE_ON_SUBGRAPH`             |    2    |
|  [04]   | `COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE`    |    4    |
|  [05]   | `COREML_FLAG_ONLY_ALLOW_STATIC_INPUT_SHAPES` |    8    |
|  [06]   | `COREML_FLAG_CREATE_MLPROGRAM`               |   16    |
|  [07]   | `COREML_FLAG_USE_CPU_AND_GPU`                |   32    |

The `OrtEpDevice` autoEP descriptor (enumerated through `OrtEnv.GetEpDevices()`) carries the columns the `Devices`/`AutoSelect` fold reads:

| [INDEX] | [MEMBER]                       | [CARRIES]                                                                                      |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `OrtEpDevice.EpName`           | provider name keyed against the `ExecutionProvider` row                                        |
|  [02]   | `OrtEpDevice.EpVendor`         | EP vendor string                                                                               |
|  [03]   | `OrtEpDevice.HardwareDevice`   | `OrtHardwareDevice` — `Type` (`CPU`/`GPU`/`NPU`), `VendorId`, `DeviceId`, `Vendor`, `Metadata` |
|  [04]   | `OrtEpDevice.EpMetadata`       | `OrtKeyValuePairs` EP self-description                                                         |
|  [05]   | `OrtEpDevice.EpOptions`        | `OrtKeyValuePairs` default EP option set                                                       |
|  [06]   | `OrtEpDevice.GetMemoryInfo`    | `OrtMemoryInfo` for the device's default allocation                                            |
|  [07]   | `OrtEpDevice.CreateSyncStream` | `OrtSyncStream` tying a device-stream lifetime to the device                                   |

The `Veto` receipt binds the `OrtDeviceEpIncompatibilityReason` `[Flags]` enum (`UInt32`) the `OrtDeviceEpIncompatibilityDetails.ReasonsBitmask` carries when an EP cannot claim a hardware device:

| [INDEX] | [REASON]             | [VALUE]    |
| :-----: | :------------------- | :--------- |
|  [01]   | `None`               | 0          |
|  [02]   | `DriverIncompatible` | 1          |
|  [03]   | `DeviceIncompatible` | 2          |
|  [04]   | `MissingDependency`  | 4          |
|  [05]   | `Unknown`            | 0x80000000 |

## [03]-[RESEARCH]

- [EP_EXECUTION]: the `Cuda`/`DirectMl` GPU registration members `AppendExecutionProvider_CUDA(int)`/`AppendExecutionProvider_DML(int)` (device id `0`) stay the win/linux-x64 design record, re-entering as one `ExecutionProvider` row each only on a host whose RID carries the GPU EP asset; the live axis is the `Cpu` and `CoreMl` rows enumerated through `OrtEnv.GetEpDevices()` as `OrtEpDevice` rows. The open leaf is the GPU device registration and the `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` warm-start verdict against live GPU hardware.
