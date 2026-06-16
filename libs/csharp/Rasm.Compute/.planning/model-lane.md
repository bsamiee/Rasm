# [COMPUTE_MODEL_LANE]

Rasm.Compute model lane: ONNX model identity and provenance, the one shared session capsule with its EP-context warm-start route, the EP-parameterized execution-provider axis across CPU and the Apple-silicon CoreML row, custom-operator admission, the OrtValue-only run-mode fold with its `BoundLoop` zero-allocation hot path, the ORT-GenAI token-streaming generative run owner, and the version-stamped deterministic result cache. The page owns the `ModelSource`/`ModelIdentity` vocabulary, the `SessionPolicy` lifecycle rows, the `ExecutionProvider` axis, the extension-op admission fold, the `RunConfig`/`RunOps` inference fold, the `GenerationPolicy`/`GenerativeRun` token-streaming owner over Microsoft.ML.OnnxRuntimeGenAI, and the `CachePolicy`/`CacheOps` read-through over Microsoft.ML.OnnxRuntime. The lane composes AppHost clocks, deadlines, drain, schedule, and cache ports plus Persistence index, blob, and `ModelResultKey` rows as settled vocabulary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                               |
| :-----: | :-------------- | :------------------------------------------------------------------- |
|   [1]   | MODEL_IDENTITY  | Checksum identity; acquisition union; schema snapshot; admission law |
|   [2]   | SESSION_CAPSULE | One shared session per model; lifecycle, warmup, drain rows          |
|   [3]   | EP_AXIS         | Execution-provider rows with probe, OS gate, option table            |
|   [4]   | EXTENSION_OPS   | Extension and custom-op registration with asset evidence             |
|   [5]   | INFERENCE_MODES | OrtValue-only run modes; cancellation edge; profiling artifacts      |
|   [6]   | GENERATIVE_RUN  | ORT-GenAI token-streaming owner; search-option table; guidance       |
|   [7]   | RESULT_CACHE    | Version-stamped deterministic keys; cache-policy rows                |

## [2]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` schema rows; `ModelSource` `[Union]` four acquisition cases collapsing to one byte admission.
- Cases: `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`.
- Entry: `public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — pure value; identity derives from the bytes, never from the caller.
- Auto: `Snapshot` stamps the XxHash128 identity checksum, graph version, and input/output slot rows in one call; `Accepts` runs once at load and faults `ModelRejected` with mismatch evidence.
- Receipt: the ModelLoad receipt case carries checksum, source case, slot counts, and elapsed; emission rides the sink port at the composition edge.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project)
- Growth: a new acquisition route is one case on `ModelSource`; zero new surface.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum` — path-keyed or filename-keyed model identity is the deleted form; `Slot.FreeDims` rows drive the free-dimension overrides at session build, with symbolic-dim values arriving from the geometry-encoding rows as settled vocabulary; schema admission happens exactly once at load.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModelSource {
    private ModelSource() { }

    public sealed record LocalFile(string Path) : ModelSource;

    public sealed record EmbeddedResource(Assembly Assembly, string Name) : ModelSource;

    public sealed record PersistenceBlob(ArtifactIndexRow Row) : ModelSource;

    public sealed record RemoteFetch(string ArtifactId) : ModelSource;
}

public sealed record ModelIdentity(
    UInt128 Checksum,
    long GraphVersion,
    Seq<ModelIdentity.Slot> Inputs,
    Seq<ModelIdentity.Slot> Outputs,
    ModelSource Source,
    Instant AcquiredAt) {
    public sealed record Slot(string Name, TensorElementType Dtype, Seq<int> Dims, Seq<string> FreeDims);

    public string Key => $"{Checksum:x32}";

    public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at) =>
        new(
            XxHash128.HashToUInt128(bytes),
            session.ModelMetadata.Version,
            Slots(session.InputMetadata),
            Slots(session.OutputMetadata),
            source,
            at);

    public Fin<Unit> Accepts(Seq<(string Name, TensorElementType Dtype, int Rank)> binding) =>
        binding.Filter(slot => !Inputs.Exists(own =>
            StringComparer.Ordinal.Equals(own.Name, slot.Name)
            && own.Dtype == slot.Dtype
            && own.Dims.Count == slot.Rank)).IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(Key));

    static Seq<Slot> Slots(IReadOnlyDictionary<string, NodeMetadata> nodes) =>
        toSeq(nodes).Map(static pair => new Slot(
            pair.Key,
            pair.Value.ElementDataType,
            toSeq(pair.Value.Dimensions),
            toSeq(pair.Value.SymbolicDimensions)));
}
```

## [3]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle policy record; `ModelSessions` boundary capsule owning the OrtEnv boot gate, the resident-session map, and the drain and warmup rows.
- Entry: `public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, ClockPolicy clocks)` — `Fin` aborts on rejected admission; a hit shares the resident session with `None` warm-start evidence and a first open carries the compiled EP-context row.
- Auto: the admission fold runs options, EP-context keys, free-dim overrides, device policy, EP registration, custom ops, and resident admission as one rail; every lease touches `LastUsed`; eviction past `ResidentSessions` captures the least-recently-used residents inside the swap and disposes them only after the map commits; on first open the compiled EP-context blob is read back from the artifact directory through `WarmStart` inside the success arm and the resulting `ArtifactIndexRow` — content-addressed by the model checksum under the `WarmStartClassification`/`WarmStartRetention` policy columns — rides out of `Open` for the composition edge to route to the Persistence blob lane, so a cold companion warms from the same blob the host wrote.
- Receipt: the Warmup receipt rides the representative-shape first run on the sweep row and carries the warm-start `ArtifactIndexRow` checksum and byte size from the `Lease` evidence when the compiled context lands; the Drain receipt counts unloaded sessions on the band-200 row.
- Packages: Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a lifecycle change is one policy value on `SessionPolicy`; the EP-context warm-start route is one artifact column on the open fold, never a second cache or artifact owner; a quantized session is the `SessionPolicy.Precision` column set to `ModelPrecision.Int8`/`Int4` so the precision flows through the existing `ep.Register(options, artifactDir, policy.Precision)` rail and the resident map keys on the model checksum unchanged — a quantization-specific session owner is the rejected form; zero new surface.
- Boundary: `ModelSessions` is the page's boundary capsule and its fence carries language-owned statement forms; ORT sessions are thread-safe for concurrent `Run`, so all lanes share ONE `InferenceSession` per checksum — a session pool is the rejected form; `DisablePerSessionThreads` puts every session on the global pool `Boot` constructs from the `CpuBudget` row — `OrtThreadingOptions.GlobalIntraOpNumThreads` and `GlobalInterOpNumThreads` take the budget's `OrtIntraOp` and `OrtInterOp` and `GlobalSpinControl` takes its `SpinControl` latency-versus-CPU posture, so a thread count or spin flag set outside this one boot fence is the named defect; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals; the sweep entry folds idle eviction before re-warm on the registered `compute-model-warmup` row; the compiled `ep.context_*` artifact and profile outputs land under the blob-lane artifact directory through `ArtifactIndexRow.Admit`, never as stray temp files, and the warm-start blob is content-addressed by the session fingerprint the capsule already computes — a managed copy of the context bytes is the rejected form.

```csharp signature
public sealed record SessionPolicy(
    int ResidentSessions, Duration IdleUnload, Duration WarmupSweep,
    GraphOptimizationLevel Optimization, bool MemoryPattern, bool Profiling,
    bool OrtExtensions, Seq<string> CustomOpLibraries, Seq<(string Dim, long Value)> FreeDims,
    ModelPrecision Precision,
    DataClassification WarmStartClassification, string WarmStartRetention) {
    public static readonly SessionPolicy Canonical = new(
        ResidentSessions: 4, IdleUnload: Duration.FromMinutes(10), WarmupSweep: Duration.FromMinutes(5),
        Optimization: GraphOptimizationLevel.ORT_ENABLE_ALL, MemoryPattern: true, Profiling: false,
        OrtExtensions: false, CustomOpLibraries: Seq<string>(), FreeDims: Seq<(string Dim, long Value)>(),
        Precision: ModelPrecision.Full,
        WarmStartClassification: DataClassification.Operational, WarmStartRetention: "blob-index");
}

public static class ModelSessions {
    sealed record Resident(InferenceSession Session, Instant LastUsed);

    static readonly Atom<HashMap<UInt128, Resident>> Residents = Atom(HashMap<UInt128, Resident>());
    static readonly PrePackedWeightsContainer PrePacked = new();

    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget) {
        if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
        var pool = new OrtThreadingOptions { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
        var creation = new EnvironmentCreationOptions { logId = logId, logLevel = severity, threadOptions = pool };
        OrtEnv.CreateInstanceWithOptions(ref creation);
        OrtEnv.Instance().DisableTelemetryEvents();
        return Fin.Succ(unit);
    }

    public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, ClockPolicy clocks) {
        var now = clocks.Now;
        if (Residents.Value.Find(model.Checksum).Case is Resident resident) {
            Residents.Swap(map => map.SetItem(model.Checksum, resident with { LastUsed = now }));
            return Fin.Succ((resident.Session, Option<ArtifactIndexRow>.None));
        }
        return Open(model, bytes, ep, policy, artifactDir, now);
    }

    public static Seq<UInt128> Unload(Instant idleBefore) {
        Seq<(UInt128, Resident)> evicted = default;
        Residents.Swap(map => (evicted = toSeq(map.ToSeq().Filter(pair => pair.Item2.LastUsed < idleBefore))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)));
        evicted.Iter(static pair => pair.Item2.Session.Dispose());
        return evicted.Map(static pair => pair.Item1);
    }

    public static DrainParticipantPort DrainRow =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, static _ => IO.lift(() => Unload(Instant.MaxValue)).Map(static _ => unit));

    public static ScheduleEntry SweepRow(Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(SessionPolicy.Canonical.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None, warm);

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Open(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Instant now) {
        var options = new SessionOptions();
        try {
            options.GraphOptimizationLevel = policy.Optimization;
            options.EnableMemoryPattern = policy.MemoryPattern;
            options.EnableProfiling = policy.Profiling;
            options.ProfileOutputPathPrefix = Path.Combine(artifactDir, "onnx-profile");
            options.DisablePerSessionThreads();
            options.AddSessionConfigEntry("ep.context_enable", "1");
            options.AddSessionConfigEntry("ep.context_file_path", Path.Combine(artifactDir, $"{model.Checksum:x32}.ctx.onnx"));
            options.AddSessionConfigEntry("ep.share_ep_contexts", "1");
            policy.FreeDims.Iter(dim => options.AddFreeDimensionOverrideByName(dim.Dim, dim.Value));
            ep.DevicePolicy.Iter(options.SetEpSelectionPolicy);
            ep.Register(options, artifactDir, policy.Precision);
            return CustomOps.Register(options, policy)
                .MapFail(fault => { options.Dispose(); return fault; })
                .Map(ready => {
                    var session = new InferenceSession(bytes.ToArray(), ready, PrePacked);
                    Seq<(UInt128, Resident)> evicted = default;
                    Residents.Swap(map => (evicted = toSeq(map.ToSeq().OrderBy(static pair => pair.Item2.LastUsed).Take(Math.Max(map.Count - policy.ResidentSessions + 1, 0)))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)).Add(model.Checksum, new Resident(session, now)));
                    evicted.Iter(static pair => pair.Item2.Session.Dispose());
                    return (session, WarmStart(model, artifactDir, policy.WarmStartClassification, policy.WarmStartRetention, now));
                });
        }
        catch (Exception error) {
            options.Dispose();
            return Fin.Fail<(InferenceSession, Option<ArtifactIndexRow>)>(new ComputeFault.ModelRejected(error.Message));
        }
    }

    static Option<ArtifactIndexRow> WarmStart(ModelIdentity model, string artifactDir, DataClassification classification, string retentionClass, Instant at) {
        var contextPath = Path.Combine(artifactDir, $"{model.Checksum:x32}.ctx.onnx");
        return File.Exists(contextPath)
            ? Some(ArtifactIndexRow.Admit(ArtifactIndexRow.EpContext, contextPath, File.ReadAllBytes(contextPath), classification, retentionClass, at))
            : None;
    }
}
```

## [4]-[EP_AXIS]

- Owner: `ModelKeyPolicy` ordinal accessor; `ExecutionProvider` `[SmartEnum<string>]` rows with probe name, OS gate, `ModelPrecision` quantization posture, frozen option table, device policy, and register delegate columns; `ModelPrecision` `[SmartEnum<string>]` int8/int4 quantization rows.
- Cases: `ExecutionProvider` rows `Cpu`, `CoreMl`; `ModelPrecision` rows full · int8 · int4.
- Auto: `Available` reads the `GetAvailableProviders` probe plus the macOS 12 gate riding the `ModelFormat` row value; `ResultKey(ortVersion, precision)` stamps EP key, ORT version, the `ModelPrecision` key, and the precision-folded option-table hash for the deterministic cache key with zero call-site hashing; the `Register(options, artifactDir, precision)` rail folds the int8/int4 quantization posture into the CoreML option table through `CoreMlRowsFor(precision)` — the row writes `AllowLowPrecisionAccumulationOnGPU` from the `ModelPrecision.LowPrecisionAccumulation` flag and the precision XORs into the cache-key option hash so a quantized session keys distinctly from a full-precision one with zero call-site ceremony.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new accelerator is one `ExecutionProvider` row with its probe name, OS gate, and device policy columns — the GPU `Cuda`/`DirectMl` registration member spelling stays the design record on the `[EP_EXECUTION]` RESEARCH row (member shape `AppendExecutionProvider_CUDA(0)`/`_DML(0)` FINALIZED for win/linux-x64) and re-enters as one row only on a host whose RID carries the GPU asset; a new model-quantization posture is one `ModelPrecision` row plus its option-table contribution folded into the same registration rail, never a second session-options owner; the generative token-streaming successor lands as the `GENERATIVE_RUN` run-mode cluster composing this EP axis, never a chat-client surface; zero new surface.
- Boundary: `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags)` is the canonical typed registration carrying the `CoreMlFlag` flag column, and `AppendExecutionProvider("CoreMLExecutionProvider", options)` is the proved option-rich fallback for the string-keyed `ModelCacheDirectory`/`MLComputeUnits`/`SpecializationStrategy` keys — a bare `"CoreML"` provider name faults `InvalidArgument` and is the deleted spelling; the axis is the two live osx-arm64 rows `Cpu` and `CoreMl` because no GPU asset ships on this single-RID host — the `Cuda`/`DirectMl` GPU rows are dropped from the live axis and their grounded `AppendExecutionProvider_CUDA(0)`/`AppendExecutionProvider_DML(0)` member spelling is the FINALIZED design record on `[EP_EXECUTION]`, re-entering as a row only behind a GPU-carrying RID; the macOS 12 gate is per `ModelFormat` value because the legacy NeuralNetwork format alone reaches back to macOS 10.15; the CoreML option keys and their value domains are catalogued and the `MLComputeUnits` value domain (`ALL`/`CPUAndGPU`/`CPUAndNeuralEngine`/`CPUOnly`) and the `SpecializationStrategy` value domain (`Default`/`FastPrediction`) ride the option-table rows; the default CoreML flag is `COREML_FLAG_USE_NONE` (proved working) and `COREML_FLAG_CREATE_MLPROGRAM` is the MLProgram-backend column matching the `ModelFormat=MLProgram` option while `COREML_FLAG_USE_CPU_AND_GPU` is the `UInt32` 32 flag opening the CPU-and-GPU compute path; `ModelCacheDirectory` binds at registration to the blob-lane artifact directory so compiled CoreML caches are catalogued inventory; the `ModelPrecision` column carries the int8/int4 model-quantization posture — `Full` leaves accumulation full-precision, `Int8`/`Int4` flip `AllowLowPrecisionAccumulationOnGPU=1` on the CoreML option table (the only catalogued ORT low-precision session knob) so a quantized weight layout accumulates in low precision on the GPU compute path while the int4/int8 weight quantization itself is the packaged ONNX model's own graph property (ORT executes the quantized operators the exported graph carries, never a runtime re-quantization pass), and the precision row folds into `OptionsHash` so a quantized run keys distinctly in the result cache — a managed re-quantization kernel or a second session-options owner is the rejected form; a vetoed row degrades to the next with its reason in the receipt and `Cpu` is the implicit terminal; dylib-presence heuristics are the deleted probe form.

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
    public static readonly ModelPrecision Full = new("full", lowPrecisionAccumulation: false);
    public static readonly ModelPrecision Int8 = new("int8", lowPrecisionAccumulation: true);
    public static readonly ModelPrecision Int4 = new("int4", lowPrecisionAccumulation: true);

    public bool LowPrecisionAccumulation { get; }
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

    public static readonly ExecutionProvider Cpu = new("cpu", providerName: "CPUExecutionProvider", minMacOsMajor: 0, optionsHash: 0UL, options: FrozenDictionary<string, string>.Empty, coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Option<ExecutionProviderDevicePolicy>.None, register: static (sessionOptions, cacheDir, precision) => { });

    public static readonly ExecutionProvider CoreMl = new(
        "coreml", providerName: "CoreMLExecutionProvider", minMacOsMajor: 12, optionsHash: Hash(CoreMlRows),
        options: CoreMlRows, coreMlFlag: CoreMLFlags.COREML_FLAG_USE_NONE, devicePolicy: Some(ExecutionProviderDevicePolicy.PREFER_NPU),
        register: static (sessionOptions, cacheDir, precision) => {
            sessionOptions.AppendExecutionProvider_CoreML(CoreMLFlags.COREML_FLAG_USE_NONE);
            sessionOptions.AppendExecutionProvider("CoreMLExecutionProvider", new Dictionary<string, string>(CoreMlRowsFor(precision), StringComparer.Ordinal) { ["ModelCacheDirectory"] = cacheDir });
        });

    public string ProviderName { get; }
    public int MinMacOsMajor { get; }
    public ulong OptionsHash { get; }
    public FrozenDictionary<string, string> Options { get; }
    public CoreMLFlags CoreMlFlag { get; }
    public Option<ExecutionProviderDevicePolicy> DevicePolicy { get; }
    public Action<SessionOptions, string, ModelPrecision> Register { get; }

    public bool Available =>
        OrtEnv.Instance().GetAvailableProviders().Contains(ProviderName, StringComparer.Ordinal)
        && (MinMacOsMajor is 0 || OperatingSystem.IsMacOSVersionAtLeast(MinMacOsMajor));

    public string ResultKey(string ortVersion, ModelPrecision precision) =>
        $"{Key}:{ortVersion}:{precision.Key}:{Hash(CoreMlRowsFor(precision)) ^ OptionsHash:x16}";

    static ulong Hash(FrozenDictionary<string, string> rows) =>
        XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(string.Join(';',
            rows.OrderBy(static row => row.Key, StringComparer.Ordinal).Select(static row => $"{row.Key}={row.Value}"))));
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

## [5]-[EXTENSION_OPS]

- Owner: `CustomOps` — one registration fold over the extensions bundle and the custom-op library rows, plus the string-tensor boundary constructors.
- Cases: `RegisterOrtExtensions` bundle row; `RegisterCustomOpLibraryV2` per-path rows; `CreateFromStringTensor` and `CreateTensorWithEmptyStrings` string-tensor rows.
- Entry: `public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy)` — `Fin` aborts with `ExtensionAssetMissing` naming every absent native asset before any registration runs.
- Receipt: native-asset evidence rides the ModelLoad receipt; the missing-path set is the fault payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one path row on `SessionPolicy.CustomOpLibraries`; zero new surface.
- Boundary: registration extends the `ModelSessions` boundary capsule and this fence carries language-owned statement forms — guard admission before registration and the out-parameter custom-op handle; tokenizer and pre/post operators stay session assets — a preprocessing or tokenizer service family is the rejected form; the `String` dtype is a model-boundary-only row entering through `DenseTensor<string>` and never the interior tensor vocabulary.

```csharp signature
public static class CustomOps {
    public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy) {
        var missing = policy.CustomOpLibraries.Filter(static path => !File.Exists(path));
        if (!missing.IsEmpty) {
            return Fin.Fail<SessionOptions>(new ComputeFault.ExtensionAssetMissing(string.Join(';', missing)));
        }
        if (policy.OrtExtensions) {
            options.RegisterOrtExtensions();
        }
        policy.CustomOpLibraries.Iter(path => options.RegisterCustomOpLibraryV2(path, out _));
        return Fin.Succ(options);
    }

    public static (string Name, OrtValue Value) StringInput(string name, DenseTensor<string> tokens) =>
        (name, OrtValue.CreateFromStringTensor(tokens));
    public static OrtValue StringSlots(OrtAllocator allocator, long[] shape) =>
        OrtValue.CreateTensorWithEmptyStrings(allocator, shape);
}
```

## [6]-[INFERENCE_MODES]

- Owner: `RunOps` — the run-mode fold over the shared session: single, lane-enqueued, bound-batch, and windowed runs discriminated by intent payload shape.
- Cases: single `Run`; lane-enqueued async (the lane seam owns the thread hop — the native `RunAsync` requires pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so it is the rejected spelling); `InferBound` bound batch over a populated `OrtIoBinding`; `InferBoundNamed` the name-keyed steady-state bound run pairing `GetOutputNames` with `GetOutputValues`; the `BoundLoop` steady-state hot path; `Chunked` streaming windows over chunked inputs through `RecyclableMemoryStream.GetReadOnlySequence`; `Embed` text-to-vector projection over an embedding model; `Classify` argmax-over-logits run for BIM point-cloud→element classification and symbol recognition over the interchange `PointScan` encoding; `ClashScore` scalar-output run for clash false-positive scoring over a candidate `ClashPair` feature vector.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket.
- Auto: `Plan` wires deadline expiry into the `Terminate` one-way latch from the linked `CancelScope`, attaches LoRA adapters, and folds the `RunConfig` row table into `AddRunConfigEntry` calls so a posture change selects a row rather than editing the fence; one conversion arm classifies failures into `DeadlineExpired`/`Cancelled` by scope provenance.
- Receipt: the ModelRun receipt carries route, elapsed, allocation class, and `OrtMemoryInfo` allocator evidence slots; profiling chrome-trace artifacts land as `ArtifactIndexRow.OnnxProfile` rows with the artifact path in the receipt.
- Packages: Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload-shape case on the intent family; a new run-config posture is one `RunConfig` row carrying its `AddRunConfigEntry` key-value pairs and its `OrtAllocatorType` arena column; an embedding model is one more `Embed` run over the same session capsule and a BIM classifier, symbol recognizer, or clash false-positive scorer is one more `Classify`/`ClashScore` run over the shared session reusing the inference engine for the non-AI in-scope BIM pipelines (point-cloud→element classification consumes the interchange `PointScan` encoding, clash scoring consumes the `solver-and-optimization#CLASH_AND_TWIN` `ClashPair` feature vector), never a new model lane or a BIM-specific service; zero new surface.
- Boundary: `RunOps` extends the `ModelSessions` boundary capsule and this fence carries bracketed statement forms with deterministic native disposal; OrtValue-only law — `NamedOnnxValue`, `DisposableNamedOnnxValue`, and `FixedBufferOnnxValue` are superseded spellings that never appear; `CreateTensorValueFromMemory` binds rented staging arrays without copies; the `Terminate` latch is the single cancellation propagation path and the deadline-poll cadence binds from the CANCELLATION research row; `InferBound` runs `RunWithBinding` over a populated `OrtIoBinding`, bracketing the run between `SynchronizeBoundInputs` and `SynchronizeBoundOutputs` and projecting `GetOutputValues`; `InferBoundNamed` is the name-keyed steady-state bound run — the same `RunWithBinding` bracket projecting the output `OrtValue`s zipped against `GetOutputNames()` to a `Seq<(string Name, OrtValue Value)>`, delivering the `RunWithBindingAndNames(RunOptions, OrtIoBinding, string[])` named-output convenience without materializing the forbidden `IDisposableReadOnlyCollection<DisposableNamedOnnxValue>` that member returns; the `BoundLoop` capsule is the zero-allocation steady-state posture for repeated same-shape inference — `CreateIoBinding`, `BindInput`/`BindOutput` once, a `MemoryOwner<float>` input plane refreshed by span copy, a `CreateAllocatedTensorValue` output sink, and `RunWithBinding` per `Pulse` with no per-call marshal — and a shape-class transition rebinds through `ClearBoundInputs`/`ClearBoundOutputs` with `BindOutputToDevice` routing device outputs; `Chunked` reads the chunked input through `RecyclableMemoryStream.GetReadOnlySequence` (zero-copy) and drives one `BoundLoop.Pulse` per window emitting the `streaming` `ProgressPhase` and a `StreamSegment` receipt per chunk, never a hand-rolled contiguous frame; `Embed` runs one inference over an embedding model and projects the last-hidden-state mean-pool or CLS-token slice to a `float[]`/`Half[]` vector feeding the Persistence vector lane by reference, keying on the content-hash and model-id reuse identity the Persistence owner holds; the `RunConfig.Arena` column classifies the run allocator through `OrtAllocatorType` (`ArenaAllocator` steady, `DeviceAllocator` device-resident); output projection scopes native memory inside `project` and sentinel or NaN values project to `Option` at the boundary, never inward.

```csharp signature
public sealed record RunConfig(FrozenDictionary<string, string> Entries, OrtAllocatorType Arena) {
    public static readonly RunConfig Steady = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.ArenaAllocator);
    public static RunConfig Bulk(string arenaShrinkDevice) => new(new Dictionary<string, string>(StringComparer.Ordinal) {
        ["memory.enable_memory_arena_shrinkage"] = arenaShrinkDevice,
    }.ToFrozenDictionary(StringComparer.Ordinal), OrtAllocatorType.ArenaAllocator);
    public static readonly RunConfig Device = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.DeviceAllocator);
}

public static class RunOps {
    public static RunOptions Plan(CancelScope scope, RunConfig config, Option<OrtLoraAdapter> lora = default) {
        var options = new RunOptions();
        lora.Iter(options.AddActiveLoraAdapter);
        config.Entries.Iter(entry => options.AddRunConfigEntry(entry.Key, entry.Value));
        scope.Source.Token.Register(() => options.Terminate = true);
        return options;
    }

    public static (string Name, OrtValue Value) Input<T>(string name, T[] data, long[] shape) where T : unmanaged =>
        (name, OrtValue.CreateTensorValueFromMemory(data, shape));

    extension(InferenceSession session) {
        public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            Bracket(scope, project, () => session.Run(options, inputs.Map(static row => row.Name), inputs.Map(static row => row.Value), outputs));

        public Fin<T> InferBound<T>(RunOptions options, CancelScope scope, OrtIoBinding binding, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            Bracket(scope, project, () => { binding.SynchronizeBoundInputs(); session.RunWithBinding(options, binding); binding.SynchronizeBoundOutputs(); return binding.GetOutputValues(); });

        public Fin<T> InferBoundNamed<T>(RunOptions options, CancelScope scope, OrtIoBinding binding, Func<Seq<(string Name, OrtValue Value)>, Fin<T>> project) =>
            Bracket(
                scope,
                results => project(toSeq(binding.GetOutputNames()).Zip(toSeq(results), static (name, value) => (Name: name, Value: value))),
                () => { binding.SynchronizeBoundInputs(); session.RunWithBinding(options, binding); binding.SynchronizeBoundOutputs(); return binding.GetOutputValues(); });

        public ArtifactIndexRow Profile(DataClassification classification, string retentionClass, Instant at) {
            var path = session.EndProfiling();
            return ArtifactIndexRow.Admit(ArtifactIndexRow.OnnxProfile, path, File.ReadAllBytes(path), classification, retentionClass, at);
        }

        public Fin<Seq<T>> Chunked<T>(RunOptions options, CancelScope scope, BoundLoop loop, ReadOnlySequence<byte> windows, int windowFloats, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            toSeq(Frames(windows, windowFloats)).TraverseM(window => loop.Pulse(options, scope, window.Span, project)).As();

        public Fin<float[]> Embed(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output) =>
            session.Infer(options, scope, inputs, Seq(output), static results => Fin.Succ(results.First().GetTensorDataAsSpan<float>().ToArray()));

        public Fin<Seq<(int Class, float Score)>> Classify(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string logits, int classes) =>
            session.Infer(options, scope, inputs, Seq(logits), results => {
                var scores = results.First().GetTensorDataAsSpan<float>();
                return Fin.Succ(toSeq(Enumerable.Range(0, scores.Length / Math.Max(1, classes)))
                    .Map(row => { int slice = row * classes; int arg = ArgMax(scores.Slice(slice, classes)); return (arg, scores[slice + arg]); }));
            });

        public Fin<float> ClashScore(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> features, string output) =>
            session.Infer(options, scope, features, Seq(output), static results => Fin.Succ(results.First().GetTensorDataAsSpan<float>()[0]));
    }

    static int ArgMax(ReadOnlySpan<float> scores) {
        int best = 0;
        for (int index = 1; index < scores.Length; index++) {
            if (scores[index] > scores[best]) { best = index; }
        }
        return best;
    }

    static IEnumerable<ReadOnlyMemory<float>> Frames(ReadOnlySequence<byte> windows, int windowFloats) =>
        toSeq(Enumerable.Range(0, checked((int)(windows.Length / (windowFloats * sizeof(float))))))
            .Map(index => {
                var slice = windows.Slice((long)index * windowFloats * sizeof(float), windowFloats * sizeof(float)).ToArray();
                return new ReadOnlyMemory<float>(MemoryMarshal.Cast<byte, float>(slice).ToArray());
            });

    static Fin<T> Bracket<T>(CancelScope scope, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Func<IDisposableReadOnlyCollection<OrtValue>> run) {
        IDisposableReadOnlyCollection<OrtValue>? results = null;
        try {
            results = run();
            return project(results);
        }
        catch (Exception error) {
            return Fin.Fail<T>(Faulted(scope, error));
        }
        finally {
            results?.Dispose();
        }
    }

    static Error Faulted(CancelScope scope, Exception error) =>
        scope.Source.Token.IsCancellationRequested
            ? scope.Deadline is { IsSome: true, Case: CancellationTokenSource expired } && expired.IsCancellationRequested
                ? new ComputeFault.DeadlineExpired(scope.Provenance)
                : new ComputeFault.Cancelled(scope.Provenance)
            : Error.New(error);

    public sealed class BoundLoop : IDisposable {
        readonly InferenceSession session;
        readonly OrtIoBinding binding;
        readonly MemoryOwner<float> plane;
        readonly OrtValue bound, sink;

        public BoundLoop(InferenceSession session, string input, string output, long[] shape) {
            this.session = session;
            plane = MemoryOwner<float>.Allocate(checked((int)shape.Aggregate(1L, static (acc, extent) => acc * extent)));
            bound = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, plane.Memory, shape);
            sink = OrtValue.CreateAllocatedTensorValue(OrtAllocator.DefaultInstance, TensorElementType.Float, shape);
            binding = session.CreateIoBinding();
            binding.BindInput(input, bound);
            binding.BindOutput(output, sink);
        }

        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySpan<float> payload, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            payload.CopyTo(plane.Span);
            return Bracket(scope, project, () => { binding.SynchronizeBoundInputs(); session.RunWithBinding(options, binding); binding.SynchronizeBoundOutputs(); return binding.GetOutputValues(); });
        }

        public void Rebind(string input, string output, long[] shape) {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(input, OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, plane.Memory, shape));
            binding.BindOutputToDevice(output, OrtMemoryInfo.DefaultInstance);
        }

        public void Dispose() { binding.Dispose(); bound.Dispose(); sink.Dispose(); plane.Dispose(); }
    }
}
```

## [7]-[GENERATIVE_RUN]

- Owner: `GenerationPolicy` search-option and prompt-assembly record carrying the `SearchKey` `[SmartEnum<string>]` recognized-key axis and the `GenerationPolicy.SearchRows` value table; `GuidanceKind` `[SmartEnum<string>]` structured-output constraint rows; `RunMode` `[SmartEnum<string>]` text/multimodal generative-shape rows; `Adapters`-backed `AdapterSet` LoRA hot-swap registry; `GenerativeRun` boundary capsule owning the ORT-GenAI handle chain, the token-stream loop, the LoRA-activation arm, and the tool-call arm over Microsoft.ML.OnnxRuntimeGenAI.
- Cases: `GuidanceKind` rows none · json-schema · regex · lark-grammar · choice; `SearchKey` rows num_beams · length_penalty · repetition_penalty · top_k · top_p · temperature · do_sample · max_length · min_length · early_stopping; `RunMode` rows text · multimodal.
- Entry: `public static async IAsyncEnumerable<string> Stream(ModelIdentity model, string modelDir, GenerationPolicy policy, string prompt, ExecutionProvider ep, Option<AdapterSet> adapters, [EnumeratorCancellation] CancellationToken token)` — the stream yields incremental decoded pieces; a `OnnxRuntimeGenAIException` lifts into `ComputeFault.ModelRejected` at the boundary so domain code never sees the native exception.
- Auto: the search-option fold folds the `GenerationPolicy.SearchRows` value table over the `SearchKey` axis through `GeneratorParams.SetSearchOption(string, double)` for every numeric row and `SetSearchOption(string, bool)` for every flag row — no string-valued overload exists, the recognized key strings (`num_beams`/`length_penalty`/`repetition_penalty`/`top_k`/`top_p`/`temperature`/`do_sample`/`max_length`/`min_length`/`early_stopping`) live as `SearchKey.Key` not as call-site literals, and `Echo` reads any row back through `GetSearchNumber(string)`/`GetSearchBool(string)` for the receipt; `Guidance` writes through `GeneratorParams.SetGuidance(string type, string data, bool enableFFTokens)` before `Generator` construction so a constrained run can only emit syntactically valid tokens; the LoRA arm loads each named adapter once through `Adapters.LoadAdapter(path, name)` and activates the policy's adapter mid-generation through `Generator.SetActiveAdapter(Adapters, string)` so a fine-tune swap is a name on the policy, never a second model load; the prompt assembles through `Tokenizer.ApplyChatTemplate(template, messages, tools, add_generation_prompt)` then `Encode`, so system prompt, history, retrieved context, and tool schemas serialize through the native template primitive, never a hand-rolled string concat; the loop decodes the newest token per step through `TokenizerStream.Decode(int)` over `GetSequence(0UL)[^1]`.
- Receipt: the `Generate` `ComputeReceipt` case carries model checksum, EP (whose `Precision.Key` rides the `ExecutionProvider` key so a quantized run is receipt-distinct), model-type from `Model.GetModelType()`, token count read back through `Generator.TokenCount()`, tokens-per-second, the `GuidanceKind` dimension, the constrained-token count, and the tool-call count — the catalogued 8-field `Generate` case at `receipts-and-benchmarks#RECEIPT_UNION`; the `RunMode` and active-adapter dimensions ride the `rasm.compute.generate.tokens` instrument tags (`run.mode`, `lora.adapter`) rather than minting receipt fields, so a `RunMode`/adapter receipt column is a `receipts-and-benchmarks` owner change, never invented here; the `streaming` `ProgressPhase` and a `StreamSegment` receipt emit per chunk.
- Packages: Microsoft.ML.OnnxRuntimeGenAI, Microsoft.Extensions.AI.Abstractions, Microsoft.ML.OnnxRuntime, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), BCL inbox
- Growth: a new search option is one `SearchKey` row plus one `SearchRows` value entry folded into the `SetSearchOption` rail — never a new fence statement; a new output constraint is one `GuidanceKind` row; a new generative shape is one `RunMode` row; a new fine-tune is one named adapter on `AdapterSet`; the built-in `OnnxRuntimeGenAIChatClient : IChatClient` composes the same handle chain when the M.E.AI streaming abstraction is the consumer; zero new surface.
- Boundary: token-streaming is a run mode on this owned model lane composing the session and EP spine and is HOST-LOCAL — the cluster carries no TS_PROJECTION; a remote generative run crosses solely through the EXISTING `remote-lane#PROTO_VOCABULARY` `Generate` rpc (`GenerateRequest` → `TokenChunk` server-stream) projected once as the `ComputeServiceShape.generate` `MethodShape` at `remote-lane`, never re-projected here, and the decoded token pieces and `IAsyncEnumerable<string>` stream are interior types that never sit between wire and rail; a `GenerativeService`, `ChatClient`, `Conversation`, or `PromptService` is the rejected form, and the `Tokenizer`/`TokenizerStream` are session assets, never a tokenizer service family; every ORT-GenAI type is `IDisposable` wrapping a native handle and the `using` order is LIFO with `OgaHandle` outermost (process-global init/teardown); `Adapters : SafeHandle` is created per `Model` and lives for the adapter set's lifetime, released through `ReleaseHandle`→`OgaDestroyAdapters` at the `SafeHandle` GC boundary, so `AdapterSet` holds it for the model's resident lifetime and never re-creates per generation; the recognized `SetSearchOption` key strings are not validated at the managed boundary and an unrecognized key faults `OnnxRuntimeGenAIException` from native — the `SearchKey` axis is the managed registry the binary itself does not carry, so a literal key string at a call site is the named defect; spans returned by `GetSequence`/`GetNextTokens` are views over native memory owned by the live `Generator` — the newest token copies out before the next iteration and never retains past the loop, and `GetNextTokens()` (most-recently-generated span) is distinct from `GetSequence(ulong)` (full-sequence view); `TokenizerStream` is obtained ONLY through `Tokenizer.CreateStream()` (no public constructor) and `GetSequence`/`RewindTo`/`TokenCount` take/return `ulong` (`0UL`); the sole error rail is `OnnxRuntimeGenAIException` lifted to `ComputeFault.ModelRejected` at the boundary, never a per-call catch; numeric search options pass as `double` and flags as `bool`; provider selection rides `Config.AppendProvider`/`SetProviderOption` before `new Model(config)` as a policy column, never per-generation, and int8/int4 model quantization is a packaged genai-format model property (the `genai_config.json` the `Config(modelDir)` opens already carries the quantized weight layout) never a managed quantization knob here — the EP-side quantization knobs live on `EP_AXIS`, never a second quantization owner on this cluster; the tool-call arm surfaces a decoded tool request to the AppHost control dispatch and re-feeds the typed result through `Generator.AppendTokens(ReadOnlySpan<int>)`/`AppendTokenSequences(Sequences)` or rewinds a partial turn through `RewindTo(ulong)`, with the conversation and turn state owned by the caller, never here; grammar-constrained structured output is enforced at generation through `SetGuidance` and a managed JSON-schema validator over the output is the rejected form; the `RunMode.Multimodal` image/audio-token row is a live `Stream` dispatch arm on the same handle chain — `MultiModalProcessor(Model)`, `StreamingProcessor`, `Images`/`Audios`, `NamedTensors`, `Tensor`, and `ElementType` are all public in the 0.14.1 managed assembly, so the multimodal arm stages `Images.Load(paths)`/`Audios.Load(paths)` through `MultiModalProcessor.ProcessImagesAndAudios(prompt, images, audios)` into a `NamedTensors` batch fed to `Generator.SetInputs(NamedTensors)` (a single named tensor injects through `SetModelInput(string, Tensor)` over a `Tensor(IntPtr, long[], ElementType)`) and decodes through the processor's own `CreateStream()`/`Decode(ReadOnlySpan<int>)`, never the text-mode `Tokenizer` seam; a second multimodal owner beside this `Stream` arm or a hand-rolled image-preprocessing kernel is the rejected form, and the only remaining `[GENAI_MULTIMODAL]` residual is a live vision-language genai-format asset for runtime validation.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class GuidanceKind {
    public static readonly GuidanceKind None = new("none", type: "");
    public static readonly GuidanceKind JsonSchema = new("json-schema", type: "json_schema");
    public static readonly GuidanceKind Regex = new("regex", type: "regex");
    public static readonly GuidanceKind LarkGrammar = new("lark-grammar", type: "lark_grammar");
    public static readonly GuidanceKind Choice = new("choice", type: "choice");

    public string Type { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class SearchKey {
    public static readonly SearchKey NumBeams = new("num_beams", flag: false);
    public static readonly SearchKey LengthPenalty = new("length_penalty", flag: false);
    public static readonly SearchKey RepetitionPenalty = new("repetition_penalty", flag: false);
    public static readonly SearchKey TopK = new("top_k", flag: false);
    public static readonly SearchKey TopP = new("top_p", flag: false);
    public static readonly SearchKey Temperature = new("temperature", flag: false);
    public static readonly SearchKey DoSample = new("do_sample", flag: true);
    public static readonly SearchKey MaxLength = new("max_length", flag: false);
    public static readonly SearchKey MinLength = new("min_length", flag: false);
    public static readonly SearchKey EarlyStopping = new("early_stopping", flag: true);

    public bool Flag { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class RunMode {
    public static readonly RunMode Text = new("text", multimodal: false);
    public static readonly RunMode Multimodal = new("multimodal", multimodal: true);

    public bool Multimodal { get; }
}

public sealed record MultiModalAssets(Seq<string> ImagePaths, Seq<string> AudioPaths) {
    public static readonly MultiModalAssets None = new(Seq<string>(), Seq<string>());
}

public sealed record GenerationPolicy(
    FrozenDictionary<SearchKey, double> SearchRows,
    RunMode Mode,
    GuidanceKind Guidance,
    string GuidanceData,
    bool FastForwardTokens,
    Option<string> Adapter,
    string SystemPrompt,
    string ChatTemplate,
    Seq<(string Role, string Content)> History,
    Seq<string> RetrievedContext,
    string Tools,
    MultiModalAssets Assets) {
    public static readonly GenerationPolicy Canonical = new(
        SearchRows: new Dictionary<SearchKey, double> {
            [SearchKey.MaxLength] = 512.0, [SearchKey.MinLength] = 0.0, [SearchKey.Temperature] = 0.7,
            [SearchKey.TopP] = 0.9, [SearchKey.TopK] = 50.0, [SearchKey.RepetitionPenalty] = 1.0,
            [SearchKey.DoSample] = 1.0, [SearchKey.NumBeams] = 1.0, [SearchKey.LengthPenalty] = 1.0,
            [SearchKey.EarlyStopping] = 0.0,
        }.ToFrozenDictionary(),
        Mode: RunMode.Text, Guidance: GuidanceKind.None, GuidanceData: "", FastForwardTokens: false, Adapter: None,
        SystemPrompt: "", ChatTemplate: "", History: Seq<(string, string)>(), RetrievedContext: Seq<string>(), Tools: "",
        Assets: MultiModalAssets.None);

    public static GenerationPolicy Beam(int beams, double lengthPenalty = 1.0) =>
        Canonical with {
            SearchRows = new Dictionary<SearchKey, double>(Canonical.SearchRows) {
                [SearchKey.NumBeams] = beams, [SearchKey.DoSample] = 0.0,
                [SearchKey.LengthPenalty] = lengthPenalty, [SearchKey.EarlyStopping] = 1.0,
            }.ToFrozenDictionary(),
        };

    public void Apply(GeneratorParams generatorParams) {
        SearchRows.Iter(row => {
            if (row.Key.Flag) { generatorParams.SetSearchOption(row.Key.Key, row.Value != 0.0); }
            else { generatorParams.SetSearchOption(row.Key.Key, row.Value); }
        });
        if (Guidance != GuidanceKind.None) {
            generatorParams.SetGuidance(Guidance.Type, GuidanceData, FastForwardTokens);
        }
    }

    public FrozenDictionary<SearchKey, double> Echo(GeneratorParams generatorParams) =>
        SearchRows.Keys.ToFrozenDictionary(
            static key => key,
            key => key.Flag ? (generatorParams.GetSearchBool(key.Key) ? 1.0 : 0.0) : generatorParams.GetSearchNumber(key.Key));

    public string Messages(string prompt) =>
        JsonSerializer.Serialize(
            (Seq(("system", SystemPrompt)) + History + Seq(("user", $"{string.Join('\n', RetrievedContext)}\n{prompt}")))
                .Map(static turn => new { role = turn.Item1, content = turn.Item2 }));
}

public sealed class AdapterSet : IDisposable {
    readonly Adapters adapters;
    Set<string> loaded = Set<string>();

    public AdapterSet(Model model) => adapters = new Adapters(model);

    public Fin<AdapterSet> Load(string name, string adapterPath) {
        if (loaded.Contains(name)) { return Fin.Succ(this); }
        if (!File.Exists(adapterPath)) { return Fin.Fail<AdapterSet>(new ComputeFault.ExtensionAssetMissing(adapterPath)); }
        adapters.LoadAdapter(adapterPath, name);
        loaded = loaded.Add(name);
        return Fin.Succ(this);
    }

    public Fin<Unit> Unload(string name) {
        if (!loaded.Contains(name)) { return Fin.Succ(unit); }
        adapters.UnloadAdapter(name);
        loaded = loaded.Remove(name);
        return Fin.Succ(unit);
    }

    public void Activate(Generator generator, string name) => generator.SetActiveAdapter(adapters, name);

    public void Dispose() => adapters.Dispose();
}

public static class GenerativeRun {
    public static async IAsyncEnumerable<string> Stream(ModelIdentity model, string modelDir, GenerationPolicy policy, string prompt, ExecutionProvider ep, Option<AdapterSet> adapters, [EnumeratorCancellation] CancellationToken token) {
        using var oga = new OgaHandle();
        using var config = new Config(modelDir);
        if (ep != ExecutionProvider.Cpu) {
            config.AppendProvider(ep.Key);
            ep.Options.Iter(option => config.SetProviderOption(ep.Key, option.Key, option.Value));
        }
        using var session = new Model(config);
        using var generatorParams = new GeneratorParams(session);
        policy.Apply(generatorParams);
        using var generator = new Generator(session, generatorParams);
        adapters.Iter(set => policy.Adapter.Iter(name => set.Activate(generator, name)));
        if (policy.Mode.Multimodal) {
            using var processor = new MultiModalProcessor(session);
            using var stream = processor.CreateStream();
            using var images = Images.Load(policy.Assets.ImagePaths.ToArray());
            using var audios = Audios.Load(policy.Assets.AudioPaths.ToArray());
            using var staged = processor.ProcessImagesAndAudios(policy.Messages(prompt), images, audios);
            generator.SetInputs(staged);
            while (!generator.IsDone()) {
                token.ThrowIfCancellationRequested();
                generator.GenerateNextToken();
                var sequence = generator.GetSequence(0UL);
                yield return stream.Decode(sequence[sequence.Length - 1]);
            }
        } else {
            using var tokenizer = new Tokenizer(session);
            using var stream = tokenizer.CreateStream();
            using var encoded = tokenizer.Encode(tokenizer.ApplyChatTemplate(policy.ChatTemplate, policy.Messages(prompt), policy.Tools, true));
            generator.AppendTokenSequences(encoded);
            while (!generator.IsDone()) {
                token.ThrowIfCancellationRequested();
                generator.GenerateNextToken();
                var sequence = generator.GetSequence(0UL);
                yield return stream.Decode(sequence[sequence.Length - 1]);
            }
        }
    }

    public static ComputeReceipt.Generate Receipt(ModelIdentity model, ExecutionProvider ep, string modelType, GenerationPolicy policy, CorrelationId correlation, ulong tokenCount, Duration elapsed) =>
        new(model.Key, ep, modelType, checked((int)tokenCount),
            elapsed.TotalSeconds > 0.0 ? tokenCount / elapsed.TotalSeconds : 0.0,
            policy.Guidance.Key, 0, 0) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.Onnx, AllocationClass = AllocationClass.NativeOrt, Elapsed = elapsed,
        };

    public static async Task<Fin<Seq<string>>> Collect(ModelIdentity model, string modelDir, GenerationPolicy policy, string prompt, ExecutionProvider ep, Option<AdapterSet> adapters, CancellationToken token) {
        var pieces = Seq<string>();
        try {
            await foreach (var piece in Stream(model, modelDir, policy, prompt, ep, adapters, token)) {
                pieces = pieces.Add(piece);
            }
            return Fin.Succ(pieces);
        }
        catch (OnnxRuntimeGenAIException error) {
            return Fin.Fail<Seq<string>>(new ComputeFault.ModelRejected(error.Message));
        }
    }
}
```

```mermaid
stateDiagram-v2
    [*] --> OgaHandle
    OgaHandle --> Config
    Config --> Model
    Model --> Tokenizer
    Tokenizer --> Generator
    Generator --> Streaming
    Streaming --> Streaming : GenerateNextToken
    Streaming --> Done : IsDone
    Done --> [*]
```

## [8]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` serve/store/cut rows; `CacheOps` key derivation, checksum-echo validation, and the policy-dispatched read-through.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`.
- Entry: `public ValueTask<T> Through<T, TState>(CachePolicy policy, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token = default)` — the cache-policy row is an intent field, never a boolean flag.
- Auto: `Key` stamps model checksum, intent input digest, EP key, ORT version, and option-table hash in one call so cross-version numerical drift never serves as a deterministic hit; stampede single-flight, lane tags, and hit/miss facts ride the composed index surface with zero call-site ceremony.
- Receipt: `CacheIndexFact` hit/miss/evict facts with byte sizes; `Validated` faults `CacheCorrupt` on checksum-echo mismatch at read.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row; zero new surface.
- Boundary: `CacheOps` extends the cache boundary capsule and this fence carries the async statement forms of the fresh path; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single cache owner and hand-rolled memoization beside it is the named defect; cached payloads carry the checksum echo that `Validated` checks before projection; the cross-process result-reuse recency horizon is read by reference from the Persistence `ModelResultKey` index owner — the single horizon owner — never minted here, so a second `Duration horizon` parameter beside the policy rows is the named defect.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class CachePolicy {
    public static readonly CachePolicy Bypass = new("bypass", serveHits: false, stores: false, cutsFirst: false);
    public static readonly CachePolicy ReadThrough = new("read-through", serveHits: true, stores: true, cutsFirst: false);
    public static readonly CachePolicy WriteThrough = new("write-through", serveHits: false, stores: true, cutsFirst: false);
    public static readonly CachePolicy Refresh = new("refresh", serveHits: false, stores: true, cutsFirst: true);

    public bool ServeHits { get; }
    public bool Stores { get; }
    public bool CutsFirst { get; }
}

public static class CacheOps {
    public static ModelResultKey Key(ModelIdentity model, UInt128 inputDigest, ExecutionProvider ep, ModelPrecision precision) =>
        new(model.Key, inputDigest, ep.ResultKey(OrtEnv.Instance().GetVersionString(), precision));

    public static Fin<T> Validated<T>(ModelResultKey key, string echo, T value) =>
        StringComparer.Ordinal.Equals(echo, key.ModelChecksum)
            ? Fin.Succ(value)
            : Fin.Fail<T>(new ComputeFault.CacheCorrupt(key.ToString()));

    extension(HybridCache cache) {
        public ValueTask<T> Through<T, TState>(CachePolicy policy, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token = default) =>
            policy.ServeHits
                ? cache.Result(key, state, produce, token)
                : Fresh(cache, policy, key, state, produce, token);
    }

    static async ValueTask<T> Fresh<T, TState>(HybridCache cache, CachePolicy policy, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token) {
        if (policy.CutsFirst) {
            await cache.Remove(CacheLane.ModelResult, key.ToString(), token);
        }
        var value = await produce(state, token);
        if (policy.Stores) {
            await cache.SetAsync(CacheLane.ModelResult.Scoped(key.ToString()), value, CacheLane.ModelResult.Entry, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        return value;
    }
}
```

## [9]-[RESEARCH]

- [EP_EXECUTION]: the `Cuda` and `DirectMl` GPU registration members `AppendExecutionProvider_CUDA(int)` and `AppendExecutionProvider_DML(int)` are member-shape FINALIZED and compile, with device id `0`, kept as the win/linux-x64 design record; the rows are dropped from the live osx-arm64 `ExecutionProvider` axis (no NVIDIA-Linux/Windows RID, no GPU asset on this single-RID host) and re-enter as one row each only on a host whose RID carries the GPU EP asset, with the registration spelling transcribing verbatim from this record.
- [GENAI_LIVE_STREAM]: `OgaHandle` native dylib load (`libonnxruntime-genai.dylib`) is confirmed on osx-arm64; `OnnxRuntimeGenAIException` is the sole fault rail at `Model`/`Config` construction (`Config(modelDir)` opens `{modelDir}/genai_config.json`); the streaming member shape is FINALIZED against v0.14.1, including the `SearchKey` recognized-key fold over `SetSearchOption(string,double)`/`(string,bool)` with `GetSearchNumber`/`GetSearchBool` readback, the beam-search/stopping-criteria rows (`num_beams`/`length_penalty`/`early_stopping`/`min_length`), and the LoRA adapter-activation arm (`Adapters(Model)`/`LoadAdapter(path,name)`/`UnloadAdapter(name)` + `Generator.SetActiveAdapter(Adapters,string)`, `Adapters : SafeHandle`); the full multi-token loop and the LoRA hot-swap are gated on an operator-provisioned genai-format model asset (`genai_config.json` + ONNX weights + tokenizer + optional adapter `.onnx_adapter` files) — int8/int4 quantization is a packaged property of that asset's exported graph, not a managed pass.
- [GENAI_MULTIMODAL]: the `RunMode.Multimodal` arm is FINALIZED against the 0.14.1 managed surface — `MultiModalProcessor(Model)`, `StreamingProcessor(Model)`, `Images.Load`/`Audios.Load`, `NamedTensors`, `Tensor(IntPtr, long[], ElementType)`, and the `ElementType` enum (`float32`/`float16`/`int64`/… 14 cases) are all public, and the `Stream` multimodal dispatch stages `ProcessImagesAndAudios → NamedTensors → Generator.SetInputs` with decode through `MultiModalProcessor.CreateStream()`/`Decode(ReadOnlySpan<int>)`; the sole residual is a tier-3 live-asset probe — a vision-language genai-format asset (image/audio processor config + ONNX weights) to runtime-validate the staged-tensor shapes against the exported graph and to confirm whether image/audio token counts force a `receipts-and-benchmarks#RECEIPT_UNION` measured column beyond the current `rasm.compute.generate.tokens` `run.mode` tag.
- [CANCELLATION]: `RunOptions.Terminate` is a one-way `get;set;` latch — a latched `RunOptions` aborts both `Run` and `RunWithBinding` with the native `OnnxRuntimeException` `[ErrorCode:Fail] Exiting due to terminate flag being set to true`, which the `Bracket` arm classifies by scope provenance; the residual probe is the latch propagation latency and the deadline poll cadence on the CoreML and CPU rows inside the live plugin ALC.
