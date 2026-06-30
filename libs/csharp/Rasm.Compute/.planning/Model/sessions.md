# [COMPUTE_SESSIONS]

Rasm.Compute model session capsule: the one shared `InferenceSession` per model checksum with its EP-context warm-start route generalized into a device-keyed fleet-shared compiled context, the shared-device-allocator lease map, and the lifecycle/warmup/drain rows that materialize as `ComputeReceipt.Warmup`/`ComputeReceipt.Drain` facts at the sink edge. The page owns the `SessionPolicy` lifecycle record and the `ModelSessions` boundary capsule with its `Gate`-serialized OrtEnv boot, the resident-session map whose every `Resident` remembers its `ExecutionProvider`, representative warm shape, and warm-start `ArtifactIndexRow` so the sweep re-warms and reports without re-opening, the shared-device-allocator lease map, the single `AdmitContext` EP-context blob owner, and the compatibility-gated `Boot`/`Lease`/`Open`/`SharedAllocator`/`Warmup`/`Unload`/`DrainRow`/`SweepRow`/`Compile` fold; the session and allocator surfaces ride `Microsoft.ML.OnnxRuntime`, the `Boot` thread pool reads the AppHost `CpuBudget` row, the drain and warmup rows ride the AppHost `DrainParticipantPort`/`ScheduleEntry`/`ReceiptSinkPort`/`CorrelationId`/`ClockPolicy` surfaces, the fleet fingerprint folds `System.IO.Hashing` `XxHash3`, the warm-start and compiled-context blobs cross to the Persistence blob lane as an `ArtifactIndexRow`, and the `ModelIdentity` identity (with its `Slot` input dims) from `Model/identity#MODEL_IDENTITY`, the `ExecutionProvider`/`ModelPrecision` axis from `Model/providers#EP_AXIS`, the `CustomOps.Register` fold from `Model/extension#EXTENSION_OPS`, the `ComputeReceipt`/`WorkLane`/`Substrate`/`AllocationClass` rail from `Runtime/receipts#RECEIPT_UNION`, and `NodaTime` `Instant`/`Duration` arrive settled. The shared-arena lease is the arena the `Tensor/residency#ORT_BRIDGE` `BoundFlow` (via `TensorBridge.Bind`) threads into `CreateAllocatedTensorValue`/`RebindDevice`, and that same loop is the injected `pulse` `Warmup` drives for the representative-shape first run.

## [01]-[INDEX]

- [01]-[SESSION_CAPSULE]: one shared session per model; `Gate`-serialized boot; lifecycle, warmup (representative-shape pulse → `Warmup` receipt), drain (unloaded count → `Drain` receipt) rows; shared-device-allocator lease; compatibility-gated warm-start; fleet-shared device-keyed compiled context.

## [02]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle policy record; `ModelSessions` boundary capsule owning the `Gate`-serialized OrtEnv boot, the resident-session map (each `Resident` carries `ExecutionProvider`, representative `WarmShape`, and warm-start `Option<ArtifactIndexRow>`), the shared-device-allocator lease map, the single `AdmitContext` EP-context blob owner, the `Warmup` representative-shape re-warm, and the drain and warmup/sweep rows.
- Entry: `public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string modelPath, string artifactDir, ClockPolicy clocks)` — `Fin` aborts on rejected admission; a hit shares the resident session with `None` warm-start evidence and a first open carries the compiled EP-context row; `modelPath` feeds the autoEP compatibility probe so an incompatible warm-start blob degrades to a fresh compile.
- Auto: the admission fold runs options, EP-context keys, free-dim overrides, deployment-constant initializers, execution mode, device policy, the compatibility-gated warm-start decision, EP registration, custom ops, and resident admission as one rail; every lease touches `LastUsed`; eviction past `ResidentSessions` captures the least-recently-used residents inside the swap and disposes them only after the map commits; `Open` reads `ep.WarmStartAdmissible(modelPath, contextPath)` (providers owns the `OrtCompiledModelCompatibility` enum verdict consumption) and writes `ep.context_enable=0` on an `EP_UNSUPPORTED`/`EP_SUPPORTED_PREFER_RECOMPILATION` device so it recompiles fresh rather than faulting a load against a stale context, `ep.context_enable=1` on an `EP_SUPPORTED_OPTIMAL` verdict to keep the warm-start read — then on the success arm the compiled EP-context blob is admitted through the single `AdmitContext` owner and the resulting `ArtifactIndexRow` — keyed by the model checksum (and the `(checksum, OrtEpDevice fingerprint)` `FleetContextKey` for a fleet-published context) yet content-addressed over the blob bytes through the seam `ContentAddress` the Persistence `ArtifactIndexRow.Admit` owner mints (`ContentAddress.Of(bytes)`, the kernel seed-zero `XxHash128`), under the `WarmStartClassification`/`WarmStartRetention` policy columns — is both stamped on the `Resident` and ridden out of `Open` for the composition edge to route to the Persistence blob lane, so a cold companion warms from the same blob the host wrote, and `Compile` is the first-compile publish member driving `OrtModelCompilationOptions` (`SetEpContextEmbedMode(true)` for ONE self-contained portable blob) to emit the device-keyed context for matching-hardware peers; `RepresentativeShape` derives the batch-1 warm shape from the model's first input `Slot` (dynamic dims → 1) once at open; `Warmup` folds the injected representative-shape `pulse` over every `Resident`, emits one `ComputeReceipt.Warmup` per successfully-warmed session paired with that resident's warm-start `ArtifactIndexRow`, and contributes no fact for a failed pulse; `SweepRow` is the `compute-model-warmup` cadence whose `Work` folds idle eviction (`Unload(now - IdleUnload)`) before the injected re-warm, and `DrainRow` is the `DrainBand.Compute` participant whose `Drain` unloads every resident at `Instant.MaxValue` and emits one `ComputeReceipt.Drain` counting the unloaded sessions through the `ReceiptSinkPort`; `SharedAllocator` leases one process-shared `OrtAllocator` per `(OrtEpDevice, OrtDeviceMemoryType)` through `OrtEnv.CreateSharedAllocator(device, memory, OrtAllocatorType.ArenaAllocator, FrozenDictionary<string, string>.Empty)` under `Gate` double-checked locking so the native allocator is created exactly once (a side-effecting create inside an `Atom.Swap` retry is the rejected form), and the lease is the arena the `Tensor/residency#ORT_BRIDGE` `BoundFlow` threads into `CreateAllocatedTensorValue`/`RebindDevice`.
- Receipt: `Warmup` returns one `(ComputeReceipt.Warmup, Option<ArtifactIndexRow>)` per warmed resident — the `ComputeReceipt.Warmup` carries model checksum, `Resident.Ep`, and the `LxWxH` representative shape, and the paired warm-start `ArtifactIndexRow` (itself the checksum + byte-size carrier) is the compiled-context provenance from that resident's `Lease` evidence, so the warm-start identity rides every warm; `DrainRow` emits one `ComputeReceipt.Drain(Drained, 0, 0)` on the `DrainBand.Compute` (band-200) row whose `Drained` is the unloaded-session count, `Faulted`/`Refused` zero because session disposal is total and the capsule holds no admission queue to refuse. Both ride `Substrate.Onnx`/`WorkLane.Background`/`AllocationClass.NativeOrt` and materialize at the `ReceiptSinkPort` edge under one sweep/drain `CorrelationId`.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a lifecycle change is one policy value on `SessionPolicy`; the EP-context warm-start and the fleet compile both admit through the single `AdmitContext` owner, never a second cache or artifact owner; the fleet-shared compiled context is one `Compile` member publishing a `(checksum, OrtEpDevice fingerprint)`-keyed `ArtifactIndexRow` through the same blob-lane owner the warm-start writes, never a second EP-cache; a warmup or drain fact is one existing `ComputeReceipt.Warmup`/`Drain` case emitted through the one `ReceiptSinkPort`, never a parallel receipt owner; a new warm strategy is the injected `pulse` shape, never a second warm surface; a quantized session is the `SessionPolicy.Precision` column set to `ModelPrecision.Int8`/`Int4` so the precision flows through the existing `ep.Register(options, artifactDir, policy.Precision)` rail and the resident map keys on the model checksum unchanged — a quantization-specific session owner is the rejected form; a sequential-versus-parallel execution posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner; zero new surface.
- Boundary: `ModelSessions` is the page's boundary capsule and its fence carries language-owned statement forms (the named boundary-capsule statement exemption per `boundaries.md` CAPSULE_OWNER); ORT sessions are thread-safe for concurrent `Run`, so all lanes share ONE `InferenceSession` per checksum — a session pool is the rejected form; `Boot` and the shared-allocator create/release run under one `Gate` `System.Threading.Lock` so the env is created exactly once and each `(device, memory-type)` allocator exactly once, double-checked against `OrtEnv.IsCreated` and the lease map — a native `CreateInstanceWithOptions`/`CreateSharedAllocator` inside an `Atom.Swap` retry (which re-runs and leaks) is the named defect, and the `Residents` map stays lock-free because its swaps are pure and disposal happens after the commit; the per-open `SessionOptions` is a transient build handle the `InferenceSession` copies its config from at construction, so `Open` disposes it on the success arm exactly as the catch/`MapFail` arms dispose it on failure — a `SessionOptions` surviving past `Open` is the named native-handle leak, and the ONLY handle deliberately shared across opens is the process-wide `PrePackedWeightsContainer` weights cache (created once, never disposed per session); `DisablePerSessionThreads` puts every session on the global pool `Boot` constructs from the `CpuBudget` row — `OrtThreadingOptions.GlobalIntraOpNumThreads` and `GlobalInterOpNumThreads` take the budget's `OrtIntraOp` and `OrtInterOp` and `GlobalSpinControl` takes its `SpinControl` latency-versus-CPU posture, so a thread count or spin flag set outside this one boot fence is the named defect; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals; `SweepRow`'s `Work` folds idle eviction (`Unload(now - IdleUnload)`) before the injected re-warm on the registered `compute-model-warmup` row, and `Warmup` runs the injected representative-shape `pulse` (the `Model/inference#INFERENCE_MODES` `RunOps`-composed `BoundFlow` first run, kept above this capsule so the sessions owner never references the inference owner) over each `Resident`; the compiled `ep.context_*` artifact and profile outputs land under the blob-lane artifact directory through the single `AdmitContext` → `ArtifactIndexRow.Admit` owner, never as stray temp files, and the warm-start blob's `ArtifactIndexRow` is keyed by the session checksum the capsule already computes yet content-addressed over its bytes through the seam `ContentAddress` (never a second hasher) — a managed copy of the context bytes, and a second `AdmitContext`-bypassing admission, are the rejected forms; the fleet-shared compiled context generalizes the warm-start into a device-keyed wire artifact — `Compile` drives `OrtModelCompilationOptions` (`SetInputModelFromBuffer`/`SetOutputModelPath`/`SetEpContextEmbedMode(true)`/`SetGraphOptimizationLevel`/`SetFlags(ERROR_IF_NO_NODES_COMPILED)`/`CompileModel`) on first compile to emit ONE self-contained portable blob and publishes the `(checksum, OrtEpDevice fingerprint)`-keyed `FleetContextKey` `ArtifactKind.EpContext` row through the same blob-lane owner, so a cold companion or farm node on matching hardware fetches that device-keyed blob into its artifact directory over the `Runtime/channels#ARTIFACT_FRAMES` frame leg and warms from it instead of recompiling — `SetEpContextEmbedMode(true)` makes the side-file `SetEpContextBinaryInformation` redundant, so the external-binary call is the rejected (contradictory) form; the `Runtime/admission#SUBSTRATE_AXIS` warm-affinity column reorders the eligible chain toward the node holding the matching context blob, the device fingerprint folds `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type` through `XxHash3` so a mismatched fingerprint addresses a distinct row and cleanly falls back to a fresh compile, and a second EP-cache beside the one blob-lane owner is the rejected form; the warm-start decision is `ep.WarmStartAdmissible(modelPath, contextPath)` (providers owns the two-step `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` read whose `OrtCompiledModelCompatibility` enum is CONSUMED to choose fresh-compile-versus-warm-start), and a `verdict.ToString().Contains("Incompatible")` substring test is the named defect because no enum value carries that token; the shared-device allocator is leased once per `(device, memory-type)` with an empty `IReadOnlyDictionary<string,string>` option set (`FrozenDictionary<string,string>.Empty`, never an `OrtKeyValuePairs` — that `SafeHandle` is not the option type `CreateSharedAllocator` accepts) and released at drain (`Unload(Instant.MaxValue)`) through `OrtEnv.ReleaseSharedAllocator` captured out of the swap and run under `Gate`, and the lease is threaded into `BoundFlow` so a device-resident loop allocates its sink from the shared arena — a per-session device allocation beside the shared arena is the rejected form, and `OrtDeviceMemoryType.HOST_ACCESSIBLE` is the zero-copy host-pinned class versus `DEFAULT` device-local; the `DrainRow` participant emits its `ComputeReceipt.Drain` through the `ReceiptSinkPort` (emission bypassing the sink port is the rejected form), the `DrainBand.Compute` band-200 rank ordering its step among the lane drains.

```csharp signature
public sealed record SessionPolicy(
    int ResidentSessions, Duration IdleUnload, Duration WarmupSweep,
    GraphOptimizationLevel Optimization, ExecutionMode Execution, bool MemoryPattern, bool Profiling,
    bool OrtExtensions, Seq<string> CustomOpLibraries, Seq<(string Dim, long Value)> FreeDims,
    Seq<(string Name, OrtValue Value)> Initializers,
    ModelPrecision Precision,
    DataClassification WarmStartClassification, string WarmStartRetention) {
    public static readonly SessionPolicy Canonical = new(
        ResidentSessions: 4, IdleUnload: Duration.FromMinutes(10), WarmupSweep: Duration.FromMinutes(5),
        Optimization: GraphOptimizationLevel.ORT_ENABLE_ALL, Execution: ExecutionMode.ORT_SEQUENTIAL,
        MemoryPattern: true, Profiling: false,
        OrtExtensions: false, CustomOpLibraries: Seq<string>(), FreeDims: Seq<(string Dim, long Value)>(),
        Initializers: Seq<(string Name, OrtValue Value)>(),
        Precision: ModelPrecision.Full,
        WarmStartClassification: DataClassification.Operational, WarmStartRetention: "blob-index");
}

public static class ModelSessions {
    sealed record Resident(InferenceSession Session, ExecutionProvider Ep, long[] WarmShape, Option<ArtifactIndexRow> WarmStart, Instant LastUsed);

    sealed record DeviceArena(OrtEpDevice Device, OrtDeviceMemoryType Memory, OrtAllocator Allocator);

    static readonly Atom<HashMap<UInt128, Resident>> Residents = Atom(HashMap<UInt128, Resident>());
    static readonly Atom<HashMap<string, DeviceArena>> SharedAllocators = Atom(HashMap<string, DeviceArena>());
    static readonly PrePackedWeightsContainer PrePacked = new();
    static readonly Lock Gate = new();

    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget) {
        if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
        lock (Gate) {
            if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
            var pool = new OrtThreadingOptions { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
            var creation = new EnvironmentCreationOptions { logId = logId, logLevel = severity, threadOptions = pool };
            OrtEnv.CreateInstanceWithOptions(ref creation);
            OrtEnv.Instance().DisableTelemetryEvents();
            return Fin.Succ(unit);
        }
    }

    public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string modelPath, string artifactDir, ClockPolicy clocks) {
        var now = clocks.Now;
        if (Residents.Value.Find(model.Checksum).Case is Resident resident) {
            Residents.Swap(map => map.SetItem(model.Checksum, resident with { LastUsed = now }));
            return Fin.Succ((resident.Session, Option<ArtifactIndexRow>.None));
        }
        return Open(model, bytes, ep, policy, modelPath, artifactDir, now);
    }

    public static OrtAllocator SharedAllocator(OrtEpDevice device, OrtDeviceMemoryType memory) {
        var key = $"{device.EpName}:{device.HardwareDevice.DeviceId}:{(int)memory}";
        if (SharedAllocators.Value.Find(key).Case is DeviceArena held) { return held.Allocator; }
        lock (Gate) {
            if (SharedAllocators.Value.Find(key).Case is DeviceArena raced) { return raced.Allocator; }
            var arena = new DeviceArena(device, memory, OrtEnv.Instance().CreateSharedAllocator(device, memory, OrtAllocatorType.ArenaAllocator, FrozenDictionary<string, string>.Empty));
            SharedAllocators.Swap(map => map.Add(key, arena));
            return arena.Allocator;
        }
    }

    public static Seq<(ComputeReceipt.Warmup Receipt, Option<ArtifactIndexRow> WarmStart)> Warmup(Func<InferenceSession, long[], Fin<Unit>> pulse, CorrelationId correlation, ClockPolicy clocks) =>
        Residents.Value.ToSeq().Bind(pair => {
            var mark = clocks.Mark();
            return pulse(pair.Item2.Session, pair.Item2.WarmShape).IsSucc
                ? Seq((new ComputeReceipt.Warmup($"{pair.Item1:x32}", pair.Item2.Ep, string.Join('x', pair.Item2.WarmShape)) {
                    Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.Onnx,
                    AllocationClass = AllocationClass.NativeOrt, Elapsed = clocks.Elapsed(mark),
                }, pair.Item2.WarmStart))
                : Seq<(ComputeReceipt.Warmup, Option<ArtifactIndexRow>)>();
        });

    public static Seq<UInt128> Unload(Instant idleBefore) {
        Seq<(UInt128, Resident)> evicted = default;
        Residents.Swap(map => (evicted = toSeq(map.ToSeq().Filter(pair => pair.Item2.LastUsed < idleBefore))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)));
        evicted.Iter(static pair => pair.Item2.Session.Dispose());
        if (idleBefore == Instant.MaxValue) {
            lock (Gate) {
                Seq<DeviceArena> arenas = default;
                SharedAllocators.Swap(map => { arenas = toSeq(map.Values); return HashMap<string, DeviceArena>(); });
                arenas.Iter(static arena => OrtEnv.Instance().ReleaseSharedAllocator(arena.Device, arena.Memory));
            }
        }
        return evicted.Map(static pair => pair.Item1);
    }

    public static DrainParticipantPort DrainRow(ReceiptSinkPort sink, JsonSerializerOptions wire, CorrelationId correlation, ClockPolicy clocks) =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, _ =>
            from mark in IO.lift(clocks.Mark)
            from drained in IO.lift(() => Unload(Instant.MaxValue))
            from sent in new ComputeReceipt.Drain(drained.Count, 0, 0) {
                Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.Onnx,
                AllocationClass = AllocationClass.NativeOrt, Elapsed = clocks.Elapsed(mark),
            }.Emit(sink, wire)
            select unit);

    public static ScheduleEntry SweepRow(SessionPolicy policy, ClockPolicy clocks, Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(policy.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None,
            () => IO.lift(() => Unload(clocks.Now - policy.IdleUnload)).Bind(_ => warm()));

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Open(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string modelPath, string artifactDir, Instant now) {
        var options = new SessionOptions();
        try {
            var contextPath = Path.Combine(artifactDir, $"{model.Checksum:x32}.ctx.onnx");
            var warmCompatible = ep.WarmStartAdmissible(modelPath, contextPath);
            options.GraphOptimizationLevel = policy.Optimization;
            options.ExecutionMode = policy.Execution;
            options.EnableMemoryPattern = policy.MemoryPattern;
            options.EnableProfiling = policy.Profiling;
            options.ProfileOutputPathPrefix = Path.Combine(artifactDir, "onnx-profile");
            options.DisablePerSessionThreads();
            options.AddSessionConfigEntry("ep.context_enable", warmCompatible ? "1" : "0");
            options.AddSessionConfigEntry("ep.context_file_path", contextPath);
            options.AddSessionConfigEntry("ep.share_ep_contexts", "1");
            policy.FreeDims.Iter(dim => options.AddFreeDimensionOverrideByName(dim.Dim, dim.Value));
            policy.Initializers.Iter(slot => options.AddInitializer(slot.Name, slot.Value));
            ep.DevicePolicy.Iter(options.SetEpSelectionPolicy);
            ep.Register(options, artifactDir, policy.Precision);
            return CustomOps.Register(options, policy)
                .MapFail(fault => { options.Dispose(); return fault; })
                .Map(ready => {
                    var session = new InferenceSession(bytes.ToArray(), ready, PrePacked);
                    var warm = warmCompatible ? AdmitContext(Path.GetFileNameWithoutExtension(contextPath), contextPath, policy, now) : Option<ArtifactIndexRow>.None;
                    var resident = new Resident(session, ep, RepresentativeShape(model), warm, now);
                    Seq<(UInt128, Resident)> evicted = default;
                    Residents.Swap(map => (evicted = toSeq(map.ToSeq().OrderBy(static pair => pair.Item2.LastUsed).Take(Math.Max(map.Count - policy.ResidentSessions + 1, 0)))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)).Add(model.Checksum, resident));
                    evicted.Iter(static pair => pair.Item2.Session.Dispose());
                    // The session copies its options at construction, so the per-open SessionOptions native handle
                    // releases here exactly as the catch/MapFail arms release it on failure — the only handle held
                    // across opens is the process-shared `PrePacked`, never the per-open options.
                    ready.Dispose();
                    return (session, warm);
                });
        }
        catch (Exception error) {
            options.Dispose();
            return Fin.Fail<(InferenceSession, Option<ArtifactIndexRow>)>(new ComputeFault.ModelRejected(error.Message));
        }
    }

    static Option<ArtifactIndexRow> AdmitContext(string key, string path, SessionPolicy policy, Instant at) =>
        File.Exists(path)
            ? Some(ArtifactIndexRow.Admit(ArtifactKind.EpContext, key, File.ReadAllBytes(path), policy.WarmStartClassification, policy.WarmStartRetention, at))
            : None;

    static long[] RepresentativeShape(ModelIdentity model) =>
        model.Inputs.Head.Map(static slot => slot.Dims.Map(static d => d <= 0 ? 1L : d).ToArray()).IfNone([1L]);

    public static string FleetContextKey(UInt128 checksum, OrtEpDevice device) {
        Span<byte> seed = stackalloc byte[64];
        int written = Encoding.ASCII.GetBytes($"{device.EpName}:{device.HardwareDevice.VendorId}:{device.HardwareDevice.DeviceId}:{(int)device.HardwareDevice.Type}", seed);
        ulong fingerprint = XxHash3.HashToUInt64(seed[..written]);
        return $"{checksum:x32}:{fingerprint:x16}.ctx.onnx";
    }

    public static Fin<ArtifactIndexRow> Compile(ReadOnlyMemory<byte> bytes, OrtEpDevice device, UInt128 checksum, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Instant at) {
        var options = new SessionOptions();
        try {
            ep.Register(options, artifactDir, policy.Precision);
            options.GraphOptimizationLevel = policy.Optimization;
            var fleetKey = FleetContextKey(checksum, device);
            var outputPath = Path.Combine(artifactDir, fleetKey);
            using var compile = new OrtModelCompilationOptions(options);
            compile.SetInputModelFromBuffer(bytes.ToArray());
            compile.SetOutputModelPath(outputPath);
            compile.SetEpContextEmbedMode(true);
            compile.SetGraphOptimizationLevel(policy.Optimization);
            compile.SetFlags(OrtCompileApiFlags.ERROR_IF_NO_NODES_COMPILED);
            compile.CompileModel();
            options.Dispose();
            return AdmitContext(fleetKey, outputPath, policy, at).Case is ArtifactIndexRow row
                ? Fin.Succ(row)
                : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected($"<ep-context-compile-empty:{checksum:x32}>"));
        }
        catch (Exception error) {
            options.Dispose();
            return Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected(error.Message));
        }
    }
}
```
