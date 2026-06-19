# [COMPUTE_PROVIDERS]

Rasm.Compute model execution-provider axis: the EP-parameterized provider rows across CPU and the Apple-silicon CoreML row plus the autoEP `OrtEpDevice` hardware-device discovery and policy-delegate selection, the `ModelPrecision` int8/int4 quantization posture folded into the registration rail, and the model-compatibility warm-start probe. The page owns the `ModelKeyPolicy` ordinal accessor, the `ExecutionProvider` `[SmartEnum<string>]` rows with their probe/OS-gate/option-table/device-policy/register columns, the `ModelPrecision` quantization rows, and the `Devices`/`AutoSelect`/`Compatible`/`WarmStartAdmissible`/`Register`/`ResultKey` autoEP discovery and one-polymorphic-register fold; the provider and device surfaces ride `Microsoft.ML.OnnxRuntime`, the option-table hash rides the one `Model/identity#MODEL_IDENTITY` `ModelFingerprint.Of` ordinal-keyvalue projection, the deterministic-cache `ResultKey` reads `System.IO.Hashing`, and the GPU `Cuda`/`DirectMl` member spellings stay the design record. The `ExecutionProvider`/`ModelPrecision` axis crosses to `Model/sessions#SESSION_CAPSULE`, `Model/inference#RESULT_CACHE`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary.

## [1]-[INDEX]

- [1]-[EP_AXIS]: execution-provider rows with probe, OS gate, option table; autoEP `OrtEpDevice` discovery, policy-delegate selection, and model-compatibility probe; one polymorphic register.

## [2]-[EP_AXIS]

- Owner: `ModelKeyPolicy` ordinal accessor; `ExecutionProvider` `[SmartEnum<string>]` rows with probe name, OS gate, `ModelPrecision` quantization posture, frozen option table, device policy, hardware-device-type affinity, and register delegate columns; `ModelPrecision` `[SmartEnum<string>]` int8/int4 quantization rows; the `Devices`/`AutoSelect`/`Compatible`/`WarmStartAdmissible`/`Register` autoEP discovery + one-polymorphic-register fold over `OrtEpDevice`.
- Cases: `ExecutionProvider` rows `Cpu`, `CoreMl`; `ModelPrecision` rows full · int8 · int4.
- Auto: `Available` reads the `GetAvailableProviders` probe plus the macOS 12 gate riding the `ModelFormat` row value; `Devices` folds `OrtEnv.GetEpDevices()` into the `OrtEpDevice` rows the host enumerates, `AutoSelect` ranks them by the row's `HardwareAffinity` (`OrtHardwareDeviceType` NPU≻GPU≻CPU), and `Register(options, cacheDir, precision)` is ONE polymorphic registration discriminating on `AutoSelect.IsEmpty` — a non-empty device list registers through the device-list `AppendExecutionProvider(env, devices, options)` overload, an empty list falls to the string-keyed/typed row delegate — never two parallel register surfaces; `Compatible(modelPath)` reads the `OrtEnv.GetModelCompatibilityForEpDevices` verdict and `WarmStartAdmissible(modelPath, contextPath)` folds it into the session-capsule decision so a warm-start EP-context blob compiled for an incompatible device degrades to a fresh compile rather than a faulted load; `ResultKey(ortVersion, precision)` stamps EP key, ORT version, the `ModelPrecision` key, and the precision-folded option-table hash for the deterministic cache key with zero call-site hashing; the `Register` rail folds the int8/int4 quantization posture into the CoreML option table through `CoreMlRowsFor(precision)` — the row writes `AllowLowPrecisionAccumulationOnGPU` from the `ModelPrecision.LowPrecisionAccumulation` flag and the precision XORs into the cache-key option hash so a quantized session keys distinctly from a full-precision one with zero call-site ceremony.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new accelerator is one `ExecutionProvider` row with its probe name, OS gate, `HardwareAffinity` column, and device policy columns — the GPU `Cuda`/`DirectMl` registration member spelling (`AppendExecutionProvider_CUDA(0)`/`_DML(0)`) stays the win/linux-x64 design record on the `[EP_EXECUTION]` RESEARCH row and re-enters as one row only on a host whose RID carries the GPU asset; a new model-quantization posture is one `ModelPrecision` row plus its option-table contribution folded into the same registration rail, never a second session-options owner; a custom device-rank strategy is one `SetEpSelectionPolicyDelegate` arm on the `AutoSelect` fold, never a second selection owner; the generative token-streaming successor lands as the `Model/generative#GENERATIVE_RUN` run-mode cluster composing this EP axis, never a chat-client surface; zero new surface.
- Boundary: `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags)` is the canonical typed registration carrying the `CoreMlFlag` flag column, and `AppendExecutionProvider("CoreMLExecutionProvider", options)` is the option-rich fallback for the string-keyed `ModelCacheDirectory`/`MLComputeUnits`/`SpecializationStrategy` keys — a bare `"CoreML"` provider name faults `InvalidArgument` and is the deleted spelling; the autoEP device-list overload `AppendExecutionProvider(OrtEnv, IReadOnlyList<OrtEpDevice>, IReadOnlyDictionary<string,string>)` is the policy-free direct-device registration the `Register` fold drives once `AutoSelect` is non-empty after the host enumerates the device through `GetEpDevices()`, and `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` is the managed device-rank callback overriding the enum `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` when the row needs a custom ranking — the delegate receives `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint)` and returns the ranked `List<OrtEpDevice>`; the live axis is the two rows `Cpu` and `CoreMl` on a host with no GPU asset — the `Cuda`/`DirectMl` GPU rows carry their `AppendExecutionProvider_CUDA(0)`/`AppendExecutionProvider_DML(0)` member spelling as the design record on `[EP_EXECUTION]`, re-entering as a row only behind a GPU-carrying RID; the macOS 12 gate is per `ModelFormat` value because the legacy NeuralNetwork format alone reaches back to macOS 10.15; the CoreML option keys and their value domains are catalogued and the `MLComputeUnits` value domain (`ALL`/`CPUAndGPU`/`CPUAndNeuralEngine`/`CPUOnly`) and the `SpecializationStrategy` value domain (`Default`/`FastPrediction`) ride the option-table rows; the default CoreML flag is `COREML_FLAG_USE_NONE` and `COREML_FLAG_CREATE_MLPROGRAM` is the MLProgram-backend column matching the `ModelFormat=MLProgram` option while `COREML_FLAG_USE_CPU_AND_GPU` is the `UInt32` 32 flag opening the CPU-and-GPU compute path; `ModelCacheDirectory` binds at registration to the blob-lane artifact directory so compiled CoreML caches are catalogued inventory; the `ModelPrecision` column carries the int8/int4 model-quantization posture — `Full` leaves accumulation full-precision, `Int8`/`Int4` flip `AllowLowPrecisionAccumulationOnGPU=1` on the CoreML option table (the only catalogued ORT low-precision session knob) so a quantized weight layout accumulates in low precision on the GPU compute path while the int4/int8 weight quantization itself is the packaged ONNX model's own graph property (ORT executes the quantized operators the exported graph carries, never a runtime re-quantization pass), and the precision row folds into `OptionsHash` so a quantized run keys distinctly in the result cache — a managed re-quantization kernel or a second session-options owner is the rejected form; the option-table hash rides the one `Model/identity#MODEL_IDENTITY` `ModelFingerprint.Of` ordinal-keyvalue projection — a re-derived `XxHash3` keyvalue body here is the named defect; a vetoed row degrades to the next with its reason in the receipt and `Cpu` is the implicit terminal; dylib-presence heuristics are the deleted probe form.

```csharp signature
public sealed class ModelKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class ModelPrecision {
    public static readonly ModelPrecision Full = new("full", lowPrecisionAccumulation: false, negativeTtl: Duration.FromMinutes(15));
    public static readonly ModelPrecision Int8 = new("int8", lowPrecisionAccumulation: true, negativeTtl: Duration.FromMinutes(5));
    public static readonly ModelPrecision Int4 = new("int4", lowPrecisionAccumulation: true, negativeTtl: Duration.FromMinutes(2));

    public bool LowPrecisionAccumulation { get; }
    public Duration NegativeTtl { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
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

    static FrozenDictionary<string, string> CoreMlRowsFor(ModelPrecision precision) =>
        new Dictionary<string, string>(CoreMlRows, StringComparer.Ordinal) {
            ["AllowLowPrecisionAccumulationOnGPU"] = precision.LowPrecisionAccumulation ? "1" : "0",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static readonly ExecutionProvider Cpu = new(
        "cpu", providerName: "CPUExecutionProvider", minMacOsMajor: 0, optionsHash: 0UL, options: FrozenDictionary<string, string>.Empty,
        coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Option<ExecutionProviderDevicePolicy>.None, hardwareAffinity: OrtHardwareDeviceType.CPU,
        registerRow: static (sessionOptions, cacheDir, precision) => sessionOptions.AppendExecutionProvider_CPU(1));

    public static readonly ExecutionProvider CoreMl = new(
        "coreml", providerName: "CoreMLExecutionProvider", minMacOsMajor: 12, optionsHash: ModelFingerprint.Of(CoreMlRows),
        options: CoreMlRows, coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU), hardwareAffinity: OrtHardwareDeviceType.NPU,
        registerRow: static (sessionOptions, cacheDir, precision) => {
            sessionOptions.AppendExecutionProvider_CoreML(CoreMLFlags.COREML_FLAG_USE_NONE);
            sessionOptions.AppendExecutionProvider("CoreMLExecutionProvider", new Dictionary<string, string>(CoreMlRowsFor(precision), StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir });
        });

    public string ProviderName { get; }
    public int MinMacOsMajor { get; }
    public ulong OptionsHash { get; }
    public FrozenDictionary<string, string> Options { get; }
    public CoreMLFlags CoreMlFlag { get; }
    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public OrtHardwareDeviceType HardwareAffinity { get; }
    public Action<SessionOptions, string, ModelPrecision> RegisterRow { get; }

    public bool Available =>
        OrtEnv.Instance().GetAvailableProviders().Contains(ProviderName, StringComparer.Ordinal)
        && (MinMacOsMajor is 0 || OperatingSystem.IsMacOSVersionAtLeast(MinMacOsMajor));

    public Seq<OrtEpDevice> Devices =>
        toSeq(OrtEnv.Instance().GetEpDevices()).Filter(device => StringComparer.Ordinal.Equals(device.EpName, ProviderName));

    public Seq<OrtEpDevice> AutoSelect =>
        Devices.OrderByDescending(device => device.HardwareDevice.Type switch {
            OrtHardwareDeviceType.NPU => 2,
            OrtHardwareDeviceType.GPU => 1,
            _ => 0,
        }).ToSeq();

    public void Register(SessionOptions sessionOptions, string cacheDir, ModelPrecision precision) {
        if (AutoSelect.IsEmpty) { RegisterRow(sessionOptions, cacheDir, precision); }
        else {
            sessionOptions.AppendExecutionProvider(
                OrtEnv.Instance(), AutoSelect.ToList(),
                new Dictionary<string, string>(CoreMlRowsFor(precision), StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir });
        }
    }

    public Option<string> Compatible(string modelPath) =>
        AutoSelect.IsEmpty ? None : Some(OrtEnv.Instance().GetModelCompatibilityForEpDevices(AutoSelect.ToList(), modelPath).ToString());

    public bool WarmStartAdmissible(string modelPath, string contextPath) =>
        !File.Exists(contextPath)
        || Compatible(modelPath).Case is not string verdict
        || !verdict.Contains("Incompatible", StringComparison.OrdinalIgnoreCase);

    public string ResultKey(string ortVersion, ModelPrecision precision) =>
        $"{Key}:{ortVersion}:{precision.Key}:{ModelFingerprint.Of(CoreMlRowsFor(precision)) ^ OptionsHash:x16}";
}
```

The `CoreMlFlag` column binds the package `Microsoft.ML.OnnxRuntime.CoreMLFlags` `[Flags]` enum (`UInt32`) values:

| [INDEX] | [FLAG]                                       | [VALUE] |
| :-----: | :------------------------------------------- | :-----: |
|   [1]   | `COREML_FLAG_USE_NONE`                       |    0    |
|   [2]   | `COREML_FLAG_USE_CPU_ONLY`                   |    1    |
|   [3]   | `COREML_FLAG_ENABLE_ON_SUBGRAPH`             |    2    |
|   [4]   | `COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE`    |    4    |
|   [5]   | `COREML_FLAG_ONLY_ALLOW_STATIC_INPUT_SHAPES` |    8    |
|   [6]   | `COREML_FLAG_CREATE_MLPROGRAM`               |   16    |
|   [7]   | `COREML_FLAG_USE_CPU_AND_GPU`                |   32    |

The `OrtEpDevice` autoEP descriptor (enumerated through `OrtEnv.GetEpDevices()`) carries the columns the `Devices`/`AutoSelect` fold reads:

| [INDEX] | [MEMBER]                       | [CARRIES]                                                                                      |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | `OrtEpDevice.EpName`           | provider name keyed against the `ExecutionProvider` row                                        |
|   [2]   | `OrtEpDevice.EpVendor`         | EP vendor string                                                                               |
|   [3]   | `OrtEpDevice.HardwareDevice`   | `OrtHardwareDevice` — `Type` (`CPU`/`GPU`/`NPU`), `VendorId`, `DeviceId`, `Vendor`, `Metadata` |
|   [4]   | `OrtEpDevice.EpMetadata`       | `OrtKeyValuePairs` EP self-description                                                         |
|   [5]   | `OrtEpDevice.EpOptions`        | `OrtKeyValuePairs` default EP option set                                                       |
|   [6]   | `OrtEpDevice.GetMemoryInfo`    | `OrtMemoryInfo` for the device's default allocation                                            |
|   [7]   | `OrtEpDevice.CreateSyncStream` | `OrtSyncStream` tying a device-stream lifetime to the device                                   |

## [3]-[RESEARCH]

- [EP_EXECUTION]: the `Cuda`/`DirectMl` GPU registration members `AppendExecutionProvider_CUDA(int)`/`AppendExecutionProvider_DML(int)` (device id `0`) stay the win/linux-x64 design record, re-entering as one `ExecutionProvider` row each only on a host whose RID carries the GPU EP asset; the live axis is the `Cpu` and `CoreMl` rows enumerated through `OrtEnv.GetEpDevices()` as `OrtEpDevice` rows. The open leaf is the GPU device registration and the `GetModelCompatibilityForEpDevices` warm-start verdict against live GPU hardware.
