# [COMPUTE_SESSIONS]

Rasm.Compute model session capsule: one shared `InferenceSession` per model checksum, its EP-context warm-start generalized into a device-keyed fleet-shared compiled context, a shared-device-allocator lease map, and the lifecycle, warmup, and drain rows that materialize as `ComputeReceipt.Warmup`/`Drain` facts at the sink edge. `ModelSessions` serializes the OrtEnv boot behind one `Gate`, holds every `Resident` with its `ExecutionProvider`, representative warm shape, and warm-start `ArtifactIndexRow` so the sweep re-warms and reports without re-opening, and admits every EP-context and compiled-context blob through the single `AdmitContext` owner into the Persistence blob lane.

`SessionPolicy` and the `ModelSessions` capsule own the `Boot`/`Lease`/`Open`/`SharedAllocator`/`Warmup`/`Unload`/`DrainRow`/`SweepRow`/`Compile` fold. Session and allocator surfaces ride `Microsoft.ML.OnnxRuntime`, the `Boot` thread pool the AppHost `CpuBudget` row, the drain and warmup rows the AppHost `DrainParticipantPort`/`ScheduleEntry`/`ReceiptSinkPort`/`CorrelationId`/`ClockPolicy` surfaces, the fleet fingerprint `System.IO.Hashing` `XxHash3`; `ModelIdentity` with its `Slot` input dims (`Model/identity#MODEL_IDENTITY`), `ExecutionProvider`/`ModelPrecision` (`Model/providers#EP_AXIS`), `CustomOps.Register` (`Model/extension#EXTENSION_OPS`), the `ComputeReceipt`/`WorkLane`/`Substrate`/`AllocationClass` rail (`Runtime/receipts#RECEIPT_UNION`), and `NodaTime` `Instant`/`Duration` arrive settled. A shared-arena lease is the arena the `Tensor/residency#ORT_BRIDGE` `BoundFlow` (via `TensorBridge.Bind`) threads into `CreateAllocatedTensorValue`/`RebindDevice`, and that same loop is the injected `pulse` `Warmup` drives for the representative-shape first run.

## [01]-[INDEX]

- [01]-[SESSION_CAPSULE]: one `Gate`-serialized shared session per model with lifecycle, warmup, and drain rows, a shared-device-allocator lease, and a fleet-shared device-keyed compiled context.

## [02]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle record; `ModelSessions` capsule owning the `Gate`-serialized OrtEnv boot, the resident-session map (each `Resident` carries `ExecutionProvider`, representative `WarmShape`, warm-start `Option<ArtifactIndexRow>`), the shared-device-allocator lease map, the single `AdmitContext` EP-context blob owner, and the warmup, drain, and sweep rows.
- Entry: `public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string modelPath, string artifactDir, ClockPolicy clocks)` — `Fin` aborts on rejected admission; a hit shares the resident with `None` warm-start evidence, a first open carries the compiled EP-context row, and `modelPath` feeds the compatibility probe so an incompatible warm-start blob degrades to a fresh compile.
- Auto: the admission fold runs options, EP-context keys, free-dim overrides, initializers, execution mode, device policy, warm-start, EP registration, custom ops, and resident admission as one rail. Every lease touches `LastUsed`; eviction past `ResidentSessions` captures the least-recent residents by `LastUsed` inside the swap and disposes them only after the map commits. `Open` writes `ep.context_enable=0` on an `EP_UNSUPPORTED`/`EP_SUPPORTED_PREFER_RECOMPILATION` device so it recompiles fresh rather than faulting a load against a stale context, `=1` on `EP_SUPPORTED_OPTIMAL`; on the success arm the compiled blob admits through the single `AdmitContext` owner into an `ArtifactIndexRow` keyed by model checksum (and by the `(checksum, OrtEpDevice fingerprint)` `FleetContextKey` for a fleet context) yet content-addressed over the bytes through the seam `ContentAddress.Of(bytes)` the Persistence `ArtifactIndexRow.Admit` mints (seed-zero `XxHash128`), stamped on the `Resident` and ridden out of `Open` so a cold companion warms from the same blob. `RepresentativeShape` derives the batch-1 warm shape from the model's first input `Slot` (dynamic dims → 1) once at open. `Warmup` folds the injected `pulse` over every `Resident` and contributes no fact for a failed pulse; `SweepRow` folds idle eviction (`Unload(now - IdleUnload)`) before the injected re-warm on the `compute-model-warmup` cadence.
- Receipt: `Warmup` returns one `(ComputeReceipt.Warmup, Option<ArtifactIndexRow>)` per warmed resident — `Warmup` carries model checksum, `Resident.Ep`, and the `LxWxH` representative shape, paired with that resident's warm-start `ArtifactIndexRow` provenance so the warm-start identity rides every warm; `DrainRow` emits one `ComputeReceipt.Drain(Drained, 0, 0)` on the `DrainBand.Compute` (band-200) row whose `Drained` is the unloaded-session count, `Faulted`/`Refused` zero because session disposal is total and the capsule holds no admission queue. Both ride `Substrate.Onnx`/`WorkLane.Background`/`AllocationClass.NativeOrt` at the `ReceiptSinkPort` edge under one `CorrelationId`.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a lifecycle change is one `SessionPolicy` value; the warm-start and the fleet compile both admit through the single `AdmitContext` owner, never a second cache or artifact owner; the fleet-shared context is one `Compile` member publishing a `(checksum, OrtEpDevice fingerprint)`-keyed `ArtifactIndexRow` through the same blob-lane owner, never a second EP-cache; a warmup or drain fact is one existing `ComputeReceipt.Warmup`/`Drain` case through the one `ReceiptSinkPort`, never a parallel receipt owner; a new warm strategy is the injected `pulse` shape, never a second warm surface; a quantized session is `SessionPolicy.Precision` set to `Int8`/`Int4`, flowing through the existing `ep.Register(options, artifactDir, policy.Precision)` rail with the resident map keyed on checksum unchanged, never a quantization-specific owner; a sequential-versus-parallel posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner.
- Boundary: `ModelSessions` is the boundary capsule whose fence carries language-owned statement forms (`boundaries.md` CAPSULE_OWNER). ORT sessions are thread-safe for concurrent `Run`, so all lanes share ONE `InferenceSession` per checksum — a session pool is the rejected form. `Boot` and the shared-allocator create/release run under one `Gate` `System.Threading.Lock` so the env and each `(device, memory-type)` allocator are created exactly once, double-checked against `OrtEnv.IsCreated` and the lease map — a native `CreateInstanceWithOptions`/`CreateSharedAllocator` inside an `Atom.Swap` retry re-runs and leaks and is the named defect, while the `Residents` map stays lock-free because its swaps are pure and disposal follows the commit. A per-open `SessionOptions` is a transient build handle the `InferenceSession` copies its config from at construction — a `SessionOptions` surviving past `Open` is the named native-handle leak, and the ONLY handle deliberately shared across opens is the process-wide `PrePackedWeightsContainer` weights cache (created once, never disposed per session). `DisablePerSessionThreads` puts every session on the global pool `Boot` constructs from `CpuBudget` — `OrtThreadingOptions.GlobalIntraOpNumThreads`/`GlobalInterOpNumThreads` take `OrtIntraOp`/`OrtInterOp` and `GlobalSpinControl` takes `SpinControl`, so a thread count or spin flag set outside this one boot fence is the named defect; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals. `Warmup` runs the injected `pulse` — the `Model/inference#INFERENCE_MODES` `RunOps`-composed `BoundFlow` first run, kept above this capsule so the sessions owner never references the inference owner. Compiled `ep.context_*` artifacts and profile outputs land under the blob-lane artifact directory through the single `AdmitContext` → `ArtifactIndexRow.Admit` owner, never stray temp files; the warm-start blob is keyed by the session checksum yet content-addressed over its bytes through the seam `ContentAddress` — a managed copy of the context bytes, and a second `AdmitContext`-bypassing admission, are the rejected forms. `SetEpContextEmbedMode(true)` makes the side-file `SetEpContextBinaryInformation` redundant, so the external-binary call is the rejected contradictory form; a cold companion or farm node on matching hardware fetches the `FleetContextKey` `ArtifactKind.EpContext` blob over the `Runtime/transport#ARTIFACT_FRAMES` leg and warms instead of recompiling. A warm-affinity column on `Runtime/admission#SUBSTRATE_AXIS` reorders the eligible chain toward the node holding the matching blob; the device fingerprint folds `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type` through `XxHash3` so a mismatched fingerprint addresses a distinct row and falls back to a fresh compile, and a second EP-cache beside the one blob-lane owner is the rejected form. Warm-start reads `ep.WarmStartAdmissible` — providers owns the two-step `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` read whose `OrtCompiledModelCompatibility` enum is CONSUMED here, and a `verdict.ToString().Contains("Incompatible")` test is the named defect because no enum value carries that token. A shared-device allocator leases once per `(device, memory-type)` with an empty `IReadOnlyDictionary<string,string>` (`FrozenDictionary<string,string>.Empty`, never an `OrtKeyValuePairs` — that `SafeHandle` is not the option type `CreateSharedAllocator` accepts) and releases at drain through `OrtEnv.ReleaseSharedAllocator` captured out of the swap under `Gate`; a per-session device allocation beside the shared arena is the rejected form, and `OrtDeviceMemoryType.HOST_ACCESSIBLE` is the zero-copy host-pinned class versus `DEFAULT` device-local. `DrainRow` emits its `ComputeReceipt.Drain` through the `ReceiptSinkPort` (emission bypassing the sink port is the rejected form), the `DrainBand.Compute` band-200 rank ordering its step.

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
                    // Session copies its options at construction: this per-open handle releases here exactly as the catch/MapFail arms do.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
