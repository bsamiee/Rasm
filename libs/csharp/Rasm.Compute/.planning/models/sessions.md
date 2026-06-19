# [COMPUTE_SESSIONS]

Rasm.Compute model session capsule: the one shared `InferenceSession` per model checksum with its EP-context warm-start route generalized into a device-keyed fleet-shared compiled context, the shared-device-allocator lease map, and the lifecycle/warmup/drain rows. The page owns the `SessionPolicy` lifecycle record and the `ModelSessions` boundary capsule with its OrtEnv boot gate, resident-session map, shared-device-allocator lease, and the compatibility-gated `Open`/`Lease`/`Unload`/`Boot`/`SharedAllocator`/`Compile` fold; the session and allocator surfaces ride `Microsoft.ML.OnnxRuntime`, the `Boot` thread pool reads the AppHost `CpuBudget` row, the drain and warmup rows ride the AppHost `DrainParticipantPort`/`ScheduleEntry` surfaces, the warm-start blob crosses to the Persistence blob lane as an `ArtifactIndexRow`, and the `ModelIdentity` identity from `identity#MODEL_IDENTITY`, the `ExecutionProvider`/`ModelPrecision` axis from `providers#EP_AXIS`, the `CustomOps.Register` fold from `extension-ops#EXTENSION_OPS`, and `NodaTime` `Instant`/`Duration` arrive settled. The shared-arena lease is the arena `inference#INFERENCE_MODES` `RunOps.BoundLoop` threads into `CreateAllocatedTensorValue`/`RebindDevice`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                                                       |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------------- |
|   [1]   | SESSION_CAPSULE | One shared session per model; lifecycle, warmup, drain rows; shared-device-allocator lease; compatibility-gated warm-start |

## [2]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle policy record; `ModelSessions` boundary capsule owning the OrtEnv boot gate, the resident-session map, the shared-device-allocator lease map, and the drain and warmup rows.
- Entry: `public static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string modelPath, string artifactDir, ClockPolicy clocks)` — `Fin` aborts on rejected admission; a hit shares the resident session with `None` warm-start evidence and a first open carries the compiled EP-context row; `modelPath` feeds the autoEP compatibility probe so an incompatible warm-start blob degrades to a fresh compile.
- Auto: the admission fold runs options, EP-context keys, free-dim overrides, deployment-constant initializers, execution mode, device policy, the compatibility-gated warm-start decision, EP registration, custom ops, and resident admission as one rail; every lease touches `LastUsed`; eviction past `ResidentSessions` captures the least-recently-used residents inside the swap and disposes them only after the map commits; `Open` calls `ep.Compatible(modelPath)` over the enumerated `OrtEpDevice` rows and branches on the verdict — `Incompatible` clears the `ep.context_*` keys so the device recompiles fresh rather than faulting a load against a stale context, `Compatible`/`Unknown` keep the warm-start read — then the compiled EP-context blob is read back from the artifact directory through `WarmStart` inside the success arm and the resulting `ArtifactIndexRow` — content-addressed by the model checksum (and the `(checksum, OrtEpDevice fingerprint)` `FleetContextKey` for a fleet-published context) under the `WarmStartClassification`/`WarmStartRetention` policy columns — rides out of `Open` for the composition edge to route to the Persistence blob lane, so a cold companion warms from the same blob the host wrote, and `Compile` is the first-compile publish member driving `OrtModelCompilationOptions` to emit the device-keyed context for matching-hardware peers; `SharedAllocator` leases one process-shared `OrtAllocator` per `(OrtEpDevice, OrtDeviceMemoryType)` through `OrtEnv.CreateSharedAllocator` so device-resident bound loops on the same hardware share one arena rather than minting a per-session allocation, and the lease is the arena `RunOps.BoundLoop` threads into `CreateAllocatedTensorValue`/`RebindDevice`.
- Receipt: the Warmup receipt rides the representative-shape first run on the sweep row and carries the warm-start `ArtifactIndexRow` checksum and byte size from the `Lease` evidence when the compiled context lands; the Drain receipt counts unloaded sessions on the band-200 row.
- Packages: Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a lifecycle change is one policy value on `SessionPolicy`; the EP-context warm-start route is one artifact column on the open fold, never a second cache or artifact owner; the fleet-shared compiled context is one `Compile` member publishing a `(checksum, OrtEpDevice fingerprint)`-keyed `ArtifactIndexRow` through the same blob-lane owner the warm-start writes, never a second EP-cache; a quantized session is the `SessionPolicy.Precision` column set to `ModelPrecision.Int8`/`Int4` so the precision flows through the existing `ep.Register(options, artifactDir, policy.Precision)` rail and the resident map keys on the model checksum unchanged — a quantization-specific session owner is the rejected form; a sequential-versus-parallel execution posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner; zero new surface.
- Boundary: `ModelSessions` is the page's boundary capsule and its fence carries language-owned statement forms (the named boundary-capsule statement exemption per `boundaries.md` CAPSULE_OWNER); ORT sessions are thread-safe for concurrent `Run`, so all lanes share ONE `InferenceSession` per checksum — a session pool is the rejected form; `DisablePerSessionThreads` puts every session on the global pool `Boot` constructs from the `CpuBudget` row — `OrtThreadingOptions.GlobalIntraOpNumThreads` and `GlobalInterOpNumThreads` take the budget's `OrtIntraOp` and `OrtInterOp` and `GlobalSpinControl` takes its `SpinControl` latency-versus-CPU posture, so a thread count or spin flag set outside this one boot fence is the named defect; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals; the sweep entry folds idle eviction before re-warm on the registered `compute-model-warmup` row; the compiled `ep.context_*` artifact and profile outputs land under the blob-lane artifact directory through `ArtifactIndexRow.Admit`, never as stray temp files, and the warm-start blob is content-addressed by the session fingerprint the capsule already computes — a managed copy of the context bytes is the rejected form; the fleet-shared compiled context generalizes the warm-start into a device-keyed wire artifact — `Compile` drives `OrtModelCompilationOptions` (`SetInputModelFromBuffer`/`SetOutputModelPath`/`SetEpContextEmbedMode`/`SetEpContextBinaryInformation`/`SetGraphOptimizationLevel`/`SetFlags(ERROR_IF_NO_NODES_COMPILED)`/`CompileModel`) on first compile and publishes the `(checksum, OrtEpDevice fingerprint)`-keyed `FleetContextKey` `ArtifactIndexRow.EpContext` row through the same blob-lane owner, so a cold companion or farm node on matching hardware fetches that device-keyed blob into its artifact directory over the `remote#ARTIFACT_FRAMES` frame leg and warms from it instead of recompiling — the `intent#SUBSTRATE_AXIS` warm-affinity column reorders the eligible chain toward the node holding the matching context blob, the device fingerprint folds `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type` through `XxHash3` so a mismatched fingerprint addresses a distinct row and cleanly falls back to a fresh compile, and a second EP-cache beside the one blob-lane owner is the rejected form; the compatibility verdict from `OrtEnv.GetModelCompatibilityForEpDevices` is read once at open and CONSUMED to choose fresh-compile-versus-warm-start — a computed-but-unread verdict is the named defect; the shared-device allocator is leased once per `(device, memory-type)` and released in the drain sweep through `OrtEnv.ReleaseSharedAllocator`, and the lease is threaded into `BoundLoop` so a device-resident loop allocates its sink from the shared arena — a per-session device allocation beside the shared arena is the rejected form, and `OrtDeviceMemoryType.HOST_ACCESSIBLE` is the zero-copy host-pinned class versus `DEFAULT` device-local.

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
    sealed record Resident(InferenceSession Session, Instant LastUsed);

    sealed record DeviceArena(OrtEpDevice Device, OrtDeviceMemoryType Memory, OrtAllocator Allocator);

    static readonly Atom<HashMap<UInt128, Resident>> Residents = Atom(HashMap<UInt128, Resident>());
    static readonly Atom<HashMap<string, DeviceArena>> SharedAllocators = Atom(HashMap<string, DeviceArena>());
    static readonly PrePackedWeightsContainer PrePacked = new();

    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget) {
        if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
        var pool = new OrtThreadingOptions { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
        var creation = new EnvironmentCreationOptions { logId = logId, logLevel = severity, threadOptions = pool };
        OrtEnv.CreateInstanceWithOptions(ref creation);
        OrtEnv.Instance().DisableTelemetryEvents();
        return Fin.Succ(unit);
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
        return SharedAllocators.Value.Find(key).Case is DeviceArena held
            ? held.Allocator
            : SharedAllocators.Swap(map => map.ContainsKey(key)
                ? map
                : map.Add(key, new DeviceArena(device, memory, OrtEnv.Instance().CreateSharedAllocator(device, memory, OrtAllocatorType.ArenaAllocator, new OrtKeyValuePairs(new Dictionary<string, string>())))))
                .Find(key).Map(static arena => arena.Allocator).IfNone(OrtAllocator.DefaultInstance);
    }

    public static Seq<UInt128> Unload(Instant idleBefore) {
        Seq<(UInt128, Resident)> evicted = default;
        Residents.Swap(map => (evicted = toSeq(map.ToSeq().Filter(pair => pair.Item2.LastUsed < idleBefore))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)));
        evicted.Iter(static pair => pair.Item2.Session.Dispose());
        if (idleBefore == Instant.MaxValue) {
            SharedAllocators.Swap(static map => map.Fold(map, static (acc, pair) => { OrtEnv.Instance().ReleaseSharedAllocator(pair.Value.Device, pair.Value.Memory); return acc.Remove(pair.Key); }));
        }
        return evicted.Map(static pair => pair.Item1);
    }

    public static DrainParticipantPort DrainRow =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, static _ => IO.lift(() => Unload(Instant.MaxValue)).Map(static _ => unit));

    public static ScheduleEntry SweepRow(Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(SessionPolicy.Canonical.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None, warm);

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
                    Seq<(UInt128, Resident)> evicted = default;
                    Residents.Swap(map => (evicted = toSeq(map.ToSeq().OrderBy(static pair => pair.Item2.LastUsed).Take(Math.Max(map.Count - policy.ResidentSessions + 1, 0)))).Fold(map, static (acc, pair) => acc.Remove(pair.Item1)).Add(model.Checksum, new Resident(session, now)));
                    evicted.Iter(static pair => pair.Item2.Session.Dispose());
                    return (session, warmCompatible ? WarmStart(contextPath, policy.WarmStartClassification, policy.WarmStartRetention, now) : Option<ArtifactIndexRow>.None);
                });
        }
        catch (Exception error) {
            options.Dispose();
            return Fin.Fail<(InferenceSession, Option<ArtifactIndexRow>)>(new ComputeFault.ModelRejected(error.Message));
        }
    }

    static Option<ArtifactIndexRow> WarmStart(string contextPath, DataClassification classification, string retentionClass, Instant at) =>
        File.Exists(contextPath)
            ? Some(ArtifactIndexRow.Admit(ArtifactIndexRow.EpContext, DeviceContextKey(contextPath), File.ReadAllBytes(contextPath), classification, retentionClass, at))
            : None;

    static string DeviceContextKey(string contextPath) => Path.GetFileNameWithoutExtension(contextPath);

    public static string FleetContextKey(UInt128 checksum, OrtEpDevice device) {
        Span<byte> seed = stackalloc byte[64];
        int written = Encoding.ASCII.GetBytes($"{device.EpName}:{device.HardwareDevice.VendorId}:{device.HardwareDevice.DeviceId}:{(int)device.HardwareDevice.Type}", seed);
        ulong fingerprint = XxHash3.HashToUInt64(seed[..written]);
        return $"{checksum:x32}:{fingerprint:x16}.ctx.onnx";
    }

    public static Fin<ArtifactIndexRow> Compile(ReadOnlyMemory<byte> bytes, string modelPath, OrtEpDevice device, UInt128 checksum, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Instant at) {
        var options = new SessionOptions();
        try {
            ep.Register(options, artifactDir, policy.Precision);
            options.GraphOptimizationLevel = policy.Optimization;
            using var compile = new OrtModelCompilationOptions(options);
            string outputPath = Path.Combine(artifactDir, FleetContextKey(checksum, device));
            compile.SetInputModelFromBuffer(bytes.ToArray());
            compile.SetOutputModelPath(outputPath);
            compile.SetEpContextEmbedMode(true);
            compile.SetEpContextBinaryInformation(artifactDir, Path.GetFileName(outputPath));
            compile.SetGraphOptimizationLevel(policy.Optimization);
            compile.SetFlags(OrtCompileApiFlags.ERROR_IF_NO_NODES_COMPILED);
            compile.CompileModel();
            options.Dispose();
            return File.Exists(outputPath)
                ? Fin.Succ(ArtifactIndexRow.Admit(ArtifactIndexRow.EpContext, FleetContextKey(checksum, device), File.ReadAllBytes(outputPath), policy.WarmStartClassification, policy.WarmStartRetention, at))
                : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected($"<ep-context-compile-empty:{checksum:x32}>"));
        }
        catch (Exception error) {
            options.Dispose();
            return Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected(error.Message));
        }
    }
}
```
