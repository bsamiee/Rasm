# [COMPUTE_SESSIONS]

Rasm.Compute model session capsule: one shared `InferenceSession` per policy-complete `ResidentKey`, its EP-context warm-start generalized into a device-keyed fleet-shared compiled context under ONE `ContextKey` derivation, a shared-device-allocator lease map, and the lifecycle, warmup, and drain rows that materialize as `ComputeReceipt.Warmup`/`Drain` facts at the sink edge. `ModelSessions` serializes the OrtEnv boot behind one `Gate`, holds every `Resident` with its `ExecutionProvider`, representative warm shape, and warm-start `ArtifactIndexRow` so the sweep re-warms and reports without re-opening, and admits every EP-context and compiled-context blob through the single `AdmitContext` owner into the Persistence blob lane.

`SessionPolicy`, `ResidentKey`, and the `ModelSessions` capsule own the `Boot`/`Lease`/`Open`/`SharedAllocator`/`Warmup`/`Unload`/`Drain`/`DrainRow`/`SweepRow`/`Compile` fold. Session and allocator surfaces ride `Microsoft.ML.OnnxRuntime`, the `Boot` thread pool the `Runtime/scheduling#CPU_BUDGET` `CpuBudget` record, the drain and warmup rows the AppHost `DrainParticipantPort`/`ScheduleEntry`/`CorrelationId` surfaces with NodaTime `IClock` + BCL `TimeProvider` threaded neutral (the App-owned `ClockPolicy` stays at composition), the resident and context fingerprints the `Model/identity#MODEL_IDENTITY` `ModelFingerprint.Of` projection; `ModelIdentity` with its `Slot` input dims (`Model/identity#MODEL_IDENTITY`), `ExecutionProvider`/`ModelPrecision` (`Model/providers#EP_AXIS`), `CustomOps.Register` (`Model/extension#EXTENSION_OPS`), the `ComputeReceipt`/`ReceiptScope`/`ReceiptSurface` rail (`Runtime/receipts#RECEIPT_UNION`), and `NodaTime` `Instant`/`Duration` arrive settled. A shared-arena lease is the arena the `Tensor/residency#ORT_BRIDGE` `BoundFlow` (via `TensorBridge.Bind`) threads into `CreateAllocatedTensorValue`/`RebindDevice`, and that same loop is the injected `pulse` `Warmup` drives for the representative-shape first run.

## [01]-[INDEX]

- [01]-[SESSION_CAPSULE]: one `Gate`-serialized shared session per policy-complete `ResidentKey` with lifecycle, warmup, and drain rows, a shared-device-allocator lease, and a fleet-shared device-keyed compiled context under one `ContextKey` derivation.

## [02]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle record with its `SessionRows` fingerprint projection; `ResidentKey` the policy-complete resident identity (`Checksum` + the `ModelFingerprint.Of` fold over every construction-behavior column); `ModelSessions` capsule owning the `Gate`-serialized OrtEnv boot, the resident-session map (each `Resident` carries `ExecutionProvider`, representative `WarmShape`, warm-start `Option<ArtifactIndexRow>`), the shared-device-allocator lease map, the selected `SessionPlacement` readback, the single `ContextKey` EP-context identity, the single `AdmitContext` EP-context blob owner, and the warmup, idle-eviction, drain, and sweep rows.
- Entry: `public static Fin<SessionLease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, IClock clock, TimeProvider time)` aborts on rejected admission; a hit shares the resident with `None` warm-start evidence, an open beside an existing compatible context carries that EP-context row, and `SessionLease.Dispose` stamps release time and decrements the resident hold exactly once. The compatibility probe reads the existing context artifact itself — compat info is embedded at compile — so an incompatible or absent warm-start blob degrades to a fresh session without one.
- Auto: `CustomOpLibrary.Admit` content-keys each native asset before it enters `SessionPolicy`; `Admit` re-hashes the model bytes and every custom-op asset, rejects nonpositive capacity/durations, invalid or duplicate free dimensions and initializers, zero initializer content identities, duplicate custom-op paths, and every initializer that misses the model's exact tensor schema before `Lease` or `Compile` reaches native state. `Options` then folds free dimensions, initializers, execution, memory, profiling, device policy, provider registration, and custom ops once for both open and fleet compile. `ResidentKey.Of(model, ep, policy)` joins checksum with the `ModelFingerprint.Of` fold over every construction-behavior column, including each initializer and custom-op content key, so equal paths or names carrying different bytes cannot alias one resident or compiled context. `DeviceFingerprint` additionally folds the EP, hardware, and provider metadata tables into context and allocator identity. `Placement` zips ordered input names with `GetEpDeviceForInputs` and `GetMemoryInfosForInputs`, zips output names with `GetMemoryInfosForOutputs`, and rejects any native cardinality mismatch before returning provider/memory evidence. `Lease` increments `Resident.Leases` under `Gate`; `SessionLease.Dispose` decrements once through `Interlocked.Exchange`; `Unload` removes only zero-lease residents older than its threshold; `Drain` releases shared allocators only after no resident remains. `Warmup` acquires temporary leases over its snapshot and releases them in `finally`, so a sweep cannot dispose a pulsed session. `Open` consumes the provider compatibility enum into `ep.context_enable` and admits a compiled blob through the single `AdmitContext` owner under the same `ContextKey(ResidentKey, device)` the fleet `Compile` writes. `RepresentativeShape` reads the first dense `SlotShape.Tensor` and maps dynamic dims to `1`; a non-tensor first input falls back to `[1L]`.
- Receipt: `Warmup` returns `Fin<Seq<(ComputeReceipt.Warmup, Option<ArtifactIndexRow>)>>`, preserving any pulse fault and carrying one checksum, provider, representative shape, and warm-start row per success; `DrainRow` emits one `ComputeReceipt.Drain(Drained, 0, 0)` on `DrainBand.Compute`, where `Drained` is the unloaded-session count and the capsule owns no admission queue. Both carry `ReceiptScope.Execution(Substrate.Onnx, WorkLane.Background, AllocationClass.NativeOrt)` and drain emission crosses the sink-bound `ReceiptSurface` under one `CorrelationId`.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a lifecycle change is one `SessionPolicy` value; a new construction-behavior column is one `SessionRows` row that automatically re-keys residency and compiled contexts; the warm-start and the fleet compile both admit through the single `AdmitContext` owner over the single `ContextKey` derivation, never a second cache, artifact owner, or filename scheme; the fleet-shared context is one `Compile` member publishing a `ContextKey(ResidentKey, device)`-keyed `ArtifactIndexRow` through the same blob-lane owner, never a second EP-cache; a warmup or drain fact is one existing `ComputeReceipt.Warmup`/`Drain` case through the one `ReceiptSurface`, never a parallel receipt owner; a new warm strategy is the injected `pulse` shape, never a second warm surface; a quantized session is `SessionPolicy.Precision` set to `Int8`/`Int4` OVER settled pre-quantized model bytes — the row is execution posture (`QuantizedGraph` evidence, MatMulNBits accuracy floor, accumulation), never a graph transform, and the quantized graph carries its own checksum identity — flowing through the existing `Options` rail with residency and context reuse re-keyed by the same `SessionRows` fold, never a quantization-specific owner; a sequential-versus-parallel posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner.
- Boundary: `ModelSessions` is the `CAPSULE_OWNER`. ORT sessions are thread-safe for concurrent `Run`, so all lanes share one `InferenceSession` per `ResidentKey`; `SessionLease` is the only lifetime handed to a run. `Gate` serializes boot, resident acquire/release/eviction, and shared allocator create/release; immutable maps replace retry-capable `Atom.Swap` mutation, so no capture or native effect can replay. `SessionOptions` is transient and disposes after `InferenceSession` or `OrtModelCompilationOptions` consumes it; `PrePackedWeightsContainer` alone spans sessions. `DisablePerSessionThreads` binds every session to the global pool `Boot` derives from `CpuBudget`. Compiled `ep.context_*` artifacts and profiles land WRITE-BLOB-FIRST: the Boot-bound Persistence object-store leg persists the bytes, and only durable residence publishes the `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)` row — an unbound leg or a failed write publishes no row, so a dangling index cannot name unavailable content; retention derives from `ArtifactKind.Retention`, and each EP context projects under its model checksum. `ContextKey(ResidentKey, device)` is the sole context identity for lookup, compilation, admission, and transport, and it cannot alias sessions whose provider options or construction policy differ. `Placement` closes autoEP selection and I/O memory residency with post-construction evidence. Shared allocators release only after all resident leases drain; `Unload` never disposes a session under an active run.

```csharp signature
public sealed record SessionPolicy(
    int ResidentSessions, Duration IdleUnload, Duration WarmupSweep,
    GraphOptimizationLevel Optimization, ExecutionMode Execution, bool MemoryPattern, bool Profiling,
    bool OrtExtensions, Seq<SessionPolicy.CustomOpLibrary> CustomOpLibraries, Seq<(string Dim, long Value)> FreeDims,
    Seq<SessionPolicy.Initializer> Initializers,
    ModelPrecision Precision,
    DataClassification WarmStartClassification) {
    public static readonly SessionPolicy Canonical = new(
        ResidentSessions: 4, IdleUnload: Duration.FromMinutes(10), WarmupSweep: Duration.FromMinutes(5),
        Optimization: GraphOptimizationLevel.ORT_ENABLE_ALL, Execution: ExecutionMode.ORT_SEQUENTIAL,
        MemoryPattern: true, Profiling: false,
        OrtExtensions: false, CustomOpLibraries: Seq<CustomOpLibrary>(), FreeDims: Seq<(string Dim, long Value)>(),
        Initializers: Seq<Initializer>(),
        Precision: ModelPrecision.Full,
        WarmStartClassification: DataClassification.Operational);

    public Fin<Unit> Conforms() =>
        guard(
            ResidentSessions > 0
            && IdleUnload > Duration.Zero
            && WarmupSweep > Duration.Zero
            && FreeDims.ForAll(static dim => dim.Dim.Length > 0 && dim.Value > 0)
            && FreeDims.Map(static dim => dim.Dim).ToFrozenSet(StringComparer.Ordinal).Count == FreeDims.Count
            && Initializers.ForAll(static slot => slot.Name.Length > 0 && slot.ContentKey != UInt128.Zero)
            && Initializers.Map(static slot => slot.Name).ToFrozenSet(StringComparer.Ordinal).Count == Initializers.Count
            && CustomOpLibraries.Map(static library => library.Path).ToFrozenSet(StringComparer.Ordinal).Count == CustomOpLibraries.Count,
            new ComputeFault.ModelRejected("<session-policy>"))
        .ToFin();

    // Every construction-behavior column lands here; a column that changes the built session yet stays out of this fold re-opens the resident-aliasing defect.
    public Seq<KeyValuePair<string, string>> SessionRows(ExecutionProvider ep) => Seq(
        new KeyValuePair<string, string>("ep", ep.Key),
        new("ep-options", ModelFingerprint.Of(ep.OptionsFor(Precision)).ToString("x16", CultureInfo.InvariantCulture)),
        new("precision", Precision.Key),
        new("optimization", ((int)Optimization).ToString(CultureInfo.InvariantCulture)),
        new("execution", ((int)Execution).ToString(CultureInfo.InvariantCulture)),
        new("memory-pattern", MemoryPattern ? "1" : "0"),
        new("profiling", Profiling ? "1" : "0"),
        new("extensions", OrtExtensions ? "1" : "0"),
        new("custom-ops", string.Join(';', CustomOpLibraries.Map(static library => $"{library.Path}={library.ContentKey:x32}"))),
        new("free-dims", string.Join(';', FreeDims.OrderBy(static dim => dim.Dim, StringComparer.Ordinal).Select(static dim => $"{dim.Dim}={dim.Value}"))),
        new("initializers", string.Join(';', Initializers.OrderBy(static slot => slot.Name, StringComparer.Ordinal).Select(static slot => $"{slot.Name}={slot.ContentKey:x32}"))));

    public sealed record Initializer(string Name, OrtValue Value, UInt128 ContentKey);

    public sealed record CustomOpLibrary {
        CustomOpLibrary(string path, UInt128 contentKey) => (Path, ContentKey) = (path, contentKey);

        public string Path { get; }
        public UInt128 ContentKey { get; }

        public static Fin<CustomOpLibrary> Admit(string path) =>
            Try.lift(() => File.Exists(path)
                    ? Fin.Succ(new CustomOpLibrary(path, ContentHash.Of(File.ReadAllBytes(path))))
                    : Fin.Fail<CustomOpLibrary>(new ComputeFault.ExtensionAssetMissing(path)))
                .Run()
                .MapFail(error => new ComputeFault.ExtensionAssetMissing(error.Message))
                .Bind(identity);

        public Fin<Unit> Verify() =>
            Try.lift(() => File.Exists(Path) && ContentHash.Of(File.ReadAllBytes(Path)) == ContentKey
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new ComputeFault.ExtensionAssetMissing(Path)))
                .Run()
                .MapFail(error => new ComputeFault.ExtensionAssetMissing(error.Message))
                .Bind(identity);
    }
}

public readonly record struct ResidentKey(UInt128 Checksum, ulong Options) {
    public static ResidentKey Of(ModelIdentity model, ExecutionProvider ep, SessionPolicy policy) =>
        new(model.Checksum, ModelFingerprint.Of(policy.SessionRows(ep)));
}

public static class ModelSessions {
    public sealed record SessionPlacement(
        Seq<(string Name, Option<string> Provider, string Memory)> Inputs,
        Seq<(string Name, string Memory)> Outputs);

    public sealed class SessionLease : IDisposable {
        readonly ResidentKey key;
        readonly IClock clock;
        int released;

        internal SessionLease(ResidentKey key, InferenceSession session, Option<ArtifactIndexRow> warmStart, IClock clock) {
            this.key = key;
            this.clock = clock;
            Session = session;
            WarmStart = warmStart;
        }

        public InferenceSession Session { get; }
        public Option<ArtifactIndexRow> WarmStart { get; }

        public void Dispose() {
            if (Interlocked.Exchange(ref released, 1) is 0) { Release(key, clock.GetCurrentInstant()); }
        }
    }

    sealed record Resident(InferenceSession Session, ExecutionProvider Ep, long[] WarmShape, Option<ArtifactIndexRow> WarmStart, Instant LastUsed, int Leases);

    sealed record DeviceArena(OrtEpDevice Device, OrtDeviceMemoryType Memory, OrtAllocator Allocator);

    static HashMap<ResidentKey, Resident> Residents = HashMap<ResidentKey, Resident>();
    static HashMap<(ulong Device, OrtDeviceMemoryType Memory), DeviceArena> SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
    static readonly PrePackedWeightsContainer PrePacked = new();
    static readonly Lock Gate = new();

    // Blob-lane write leg bound once at Boot from the Persistence object-store composition; AdmitContext writes
    // the context bytes durable-first through it and publishes the index row only after residence — an unbound
    // leg publishes nothing, never a dangling index.
    static Option<Func<ReadOnlyMemory<byte>, Fin<UInt128>>> BlobStore = Option<Func<ReadOnlyMemory<byte>, Fin<UInt128>>>.None;

    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget, Func<ReadOnlyMemory<byte>, Fin<UInt128>> blobStore) {
        BlobStore = Some(blobStore);
        if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
        lock (Gate) {
            if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
            using OrtThreadingOptions pool = new() { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
            EnvironmentCreationOptions creation = new() { logId = logId, logLevel = severity, threadOptions = pool };
            OrtEnv.CreateInstanceWithOptions(ref creation);
            OrtEnv.Instance().DisableTelemetryEvents();
            return Fin.Succ(unit);
        }
    }

    public static Fin<SessionLease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, IClock clock, TimeProvider time) =>
        Admit(model, bytes, policy).Bind(_ => LeaseAdmitted(model, bytes, ep, policy, artifactDir, clock, time));

    static Fin<SessionLease> LeaseAdmitted(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, IClock clock, TimeProvider time) {
        Instant now = clock.GetCurrentInstant();
        ResidentKey key = ResidentKey.Of(model, ep, policy);
        Seq<OrtEpDevice> devices = ep.AutoSelect;
        lock (Gate) {
            if (Residents.Find(key).Case is Resident resident) {
                Residents = Residents.SetItem(key, resident with { LastUsed = now, Leases = resident.Leases + 1 });
                return Fin.Succ(new SessionLease(key, resident.Session, Option<ArtifactIndexRow>.None, clock));
            }
            return Open(key, model, bytes, ep, policy, artifactDir, clock, time, devices);
        }
    }

    public static OrtAllocator SharedAllocator(OrtEpDevice device, OrtDeviceMemoryType memory) {
        (ulong Device, OrtDeviceMemoryType Memory) key = (DeviceFingerprint(device), memory);
        lock (Gate) {
            if (SharedAllocators.Find(key).Case is DeviceArena raced) { return raced.Allocator; }
            DeviceArena arena = new(device, memory, OrtEnv.Instance().CreateSharedAllocator(device, memory, OrtAllocatorType.ArenaAllocator, FrozenDictionary<string, string>.Empty));
            SharedAllocators = SharedAllocators.Add(key, arena);
            return arena.Allocator;
        }
    }

    public static Fin<SessionPlacement> Placement(InferenceSession session) {
        IReadOnlyList<OrtEpDevice> devices = session.GetEpDeviceForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> inputMemory = session.GetMemoryInfosForInputs();
        using IDisposableReadOnlyCollection<OrtMemoryInfo> outputMemory = session.GetMemoryInfosForOutputs();
        bool cardinality = session.InputNames.Count == devices.Count
            && session.InputNames.Count == inputMemory.Count
            && session.OutputNames.Count == outputMemory.Count;
        return cardinality
            ? Fin.Succ(new SessionPlacement(
                toSeq(session.InputNames.Select((name, index) => (
                    name,
                    Optional(devices[index]).Map(static device => device.EpName),
                    inputMemory[index].Name))),
                toSeq(session.OutputNames.Select((name, index) => (name, outputMemory[index].Name)))))
            : Fin.Fail<SessionPlacement>(new ComputeFault.ModelRejected("<session-placement-cardinality>"));
    }

    public static Fin<Seq<(ComputeReceipt.Warmup Receipt, Option<ArtifactIndexRow> WarmStart)>> Warmup(
        Func<InferenceSession, long[], Fin<Unit>> pulse,
        CorrelationId correlation,
        IClock clock, TimeProvider time) {
        Seq<(ResidentKey Key, Resident Held, SessionLease Lease)> held;
        lock (Gate) {
            held = Residents.ToSeq().Map(pair => (
                pair.Item1,
                pair.Item2,
                new SessionLease(pair.Item1, pair.Item2.Session, pair.Item2.WarmStart, clock)));
            Residents = held.Fold(Residents, static (state, row) =>
                state.SetItem(row.Key, row.Held with { Leases = row.Held.Leases + 1 }));
        }
        try {
            return held.TraverseM(row => {
                long mark = time.GetTimestamp();
                return pulse(row.Lease.Session, row.Held.WarmShape).Map(_ => (
                    new ComputeReceipt.Warmup($"{row.Key.Checksum:x32}", row.Held.Ep, string.Join('x', row.Held.WarmShape)) {
                        Scope = new ReceiptScope.Execution(
                            correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, time.GetElapsedTime(mark)),
                    },
                    row.Held.WarmStart));
            }).As();
        }
        finally { held.Iter(static row => row.Lease.Dispose()); }
    }

    public static Seq<ResidentKey> Unload(Instant idleBefore) {
        Seq<(ResidentKey Key, Resident Held)> evicted;
        lock (Gate) {
            evicted = toSeq(Residents.ToSeq()
                .Filter(pair => pair.Item2.Leases is 0 && pair.Item2.LastUsed < idleBefore)
                .Map(static pair => (pair.Item1, pair.Item2)));
            Residents = evicted.Fold(Residents, static (state, row) => state.Remove(row.Key));
        }
        evicted.Iter(static pair => pair.Held.Session.Dispose());
        return evicted.Map(static pair => pair.Key);
    }

    public static int Drain() {
        int drained = Unload(Instant.MaxValue).Count;
        lock (Gate) {
            if (!Residents.IsEmpty) { return drained; }
            Seq<DeviceArena> arenas = toSeq(SharedAllocators.Values);
            SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
            arenas.Iter(static arena => OrtEnv.Instance().ReleaseSharedAllocator(arena.Device, arena.Memory));
        }
        return drained;
    }

    public static DrainParticipantPort DrainRow(ReceiptSurface receipts, CorrelationId correlation, IClock clock, TimeProvider time) =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, _ =>
            from mark in IO.lift(time.GetTimestamp)
            from drained in IO.lift(Drain)
            from sent in receipts.Emit(new ComputeReceipt.Drain(drained, 0, 0) {
                Scope = new ReceiptScope.Execution(
                    correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, time.GetElapsedTime(mark)),
            })
            select unit);

    public static ScheduleEntry SweepRow(SessionPolicy policy, IClock clock, TimeProvider time, Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(policy.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None,
            () => IO.lift(() => Unload(clock.GetCurrentInstant() - policy.IdleUnload)).Bind(_ => warm()));

    public static string ContextKey(ResidentKey resident, Option<OrtEpDevice> device) =>
        device.Map(DeviceFingerprint).Match(
            Some: fingerprint => $"{resident.Checksum:x32}:{resident.Options:x16}:{fingerprint:x16}.ctx.onnx",
            None: () => $"{resident.Checksum:x32}:{resident.Options:x16}.ctx.onnx");

    public static Fin<ArtifactIndexRow> Compile(ReadOnlyMemory<byte> bytes, OrtEpDevice device, ModelIdentity model, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Instant at) =>
        Admit(model, bytes, policy)
            .Bind(_ => Options(ep, policy, artifactDir, Seq(device)))
            .Bind(options => CompileAdmitted(bytes, device, ResidentKey.Of(model, ep, policy), policy, artifactDir, at, options));

    static Fin<ArtifactIndexRow> CompileAdmitted(ReadOnlyMemory<byte> bytes, OrtEpDevice device, ResidentKey resident, SessionPolicy policy, string artifactDir, Instant at, SessionOptions options) {
        using (options) {
            try {
                string contextKey = ContextKey(resident, Some(device));
                string outputPath = Path.Combine(artifactDir, contextKey);
                using OrtModelCompilationOptions compile = new(options);
                compile.SetInputModelFromBuffer(bytes.ToArray());
                compile.SetOutputModelPath(outputPath);
                compile.SetEpContextEmbedMode(true);
                compile.SetGraphOptimizationLevel(policy.Optimization);
                compile.SetFlags(OrtCompileApiFlags.ERROR_IF_NO_NODES_COMPILED);
                compile.CompileModel();
                return AdmitContext(resident, Some(device), outputPath, policy, at).Case is ArtifactIndexRow row
                    ? Fin.Succ(row)
                    : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected($"<ep-context-compile-empty:{resident.Checksum:x32}>"));
            }
            catch (OnnxRuntimeException error) { return Fault<ArtifactIndexRow>(error); }
            catch (IOException error) { return Fault<ArtifactIndexRow>(error); }
            catch (UnauthorizedAccessException error) { return Fault<ArtifactIndexRow>(error); }
            catch (ArgumentException error) { return Fault<ArtifactIndexRow>(error); }
        }
    }

    static Fin<SessionLease> Open(ResidentKey key, ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, IClock clock, TimeProvider time, Seq<OrtEpDevice> devices) =>
        Options(ep, policy, artifactDir, devices).Bind(options => OpenAdmitted(key, model, bytes, ep, policy, artifactDir, clock, time, devices, options));

    static Fin<SessionLease> OpenAdmitted(ResidentKey key, ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, IClock clock, TimeProvider time, Seq<OrtEpDevice> devices, SessionOptions options) {
        using (options) {
            try {
                Instant now = clock.GetCurrentInstant();
                string contextKey = ContextKey(key, devices.Head);
                string contextPath = Path.Combine(artifactDir, contextKey);
                bool warmCompatible = ep.WarmStartAdmissible(contextPath, devices);
                options.AddSessionConfigEntry("ep.context_enable", warmCompatible ? "1" : "0");
                options.AddSessionConfigEntry("ep.context_file_path", contextPath);
                options.AddSessionConfigEntry("ep.share_ep_contexts", "1");
                Option<ArtifactIndexRow> warm = warmCompatible ? AdmitContext(key, devices.Head, contextPath, policy, now) : Option<ArtifactIndexRow>.None;
                InferenceSession session = new(bytes.ToArray(), options, PrePacked);
                Resident fresh = new(session, ep, RepresentativeShape(model), warm, now, Leases: 1);
                HashMap<ResidentKey, Resident> next = Residents.Add(key, fresh);
                Seq<(ResidentKey Key, Resident Held)> evicted = toSeq(next.ToSeq()
                    .Filter(pair => pair.Item1 != key && pair.Item2.Leases is 0)
                    .OrderBy(static pair => pair.Item2.LastUsed)
                    .Take(Math.Max(next.Count - policy.ResidentSessions, 0))
                    .Map(static pair => (pair.Item1, pair.Item2)));
                Residents = evicted.Fold(next, static (state, row) => state.Remove(row.Key));
                evicted.Iter(static row => row.Held.Session.Dispose());
                return Fin.Succ(new SessionLease(key, session, warm, clock));
            }
            catch (OnnxRuntimeException error) { return Fault<SessionLease>(error); }
            catch (IOException error) { return Fault<SessionLease>(error); }
            catch (UnauthorizedAccessException error) { return Fault<SessionLease>(error); }
            catch (ArgumentException error) { return Fault<SessionLease>(error); }
        }
    }

    static void Release(ResidentKey key, Instant at) {
        lock (Gate) {
            if (Residents.Find(key).Case is Resident held) {
                Residents = Residents.SetItem(key, held with { LastUsed = at, Leases = Math.Max(held.Leases - 1, 0) });
            }
        }
    }

    static Fin<Unit> Admit(ModelIdentity model, ReadOnlyMemory<byte> bytes, SessionPolicy policy) =>
        guard(
            ContentHash.Of(bytes.Span) == model.Checksum,
            new ComputeFault.ModelRejected($"<model-content:{model.Checksum:x32}>"))
        .ToFin()
        .Bind(_ => policy.Conforms())
        .Bind(_ => policy.CustomOpLibraries
            .Traverse(library => library.Verify().ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit))
        .Bind(_ => policy.Initializers
            .Traverse(slot => model.Initializer(slot.Name, slot.Value).Map(static _ => unit).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit));

    static Fin<SessionOptions> Options(ExecutionProvider ep, SessionPolicy policy, string artifactDir, Seq<OrtEpDevice> devices) {
        SessionOptions options = new();
        try {
            options.GraphOptimizationLevel = policy.Optimization;
            options.ExecutionMode = policy.Execution;
            options.EnableMemoryPattern = policy.MemoryPattern;
            options.EnableProfiling = policy.Profiling;
            options.ProfileOutputPathPrefix = Path.Combine(artifactDir, "onnx-profile");
            options.DisablePerSessionThreads();
            policy.FreeDims.Iter(dim => options.AddFreeDimensionOverrideByName(dim.Dim, dim.Value));
            policy.Initializers.Iter(slot => options.AddInitializer(slot.Name, slot.Value));
            ep.DevicePolicy.Iter(options.SetEpSelectionPolicy);
            ep.Register(options, artifactDir, policy.Precision, devices);
            return CustomOps.Register(options, policy).MapFail(fault => { options.Dispose(); return fault; });
        }
        catch (OnnxRuntimeException error) { options.Dispose(); return Fault<SessionOptions>(error); }
        catch (IOException error) { options.Dispose(); return Fault<SessionOptions>(error); }
        catch (UnauthorizedAccessException error) { options.Dispose(); return Fault<SessionOptions>(error); }
        catch (ArgumentException error) { options.Dispose(); return Fault<SessionOptions>(error); }
    }

    static ulong DeviceFingerprint(OrtEpDevice device) => ModelFingerprint.Of(
        new KeyValuePair<string, string>[] {
            new("ep", device.EpName),
            new("ep-vendor", device.EpVendor),
            new("hardware-vendor-id", device.HardwareDevice.VendorId.ToString(CultureInfo.InvariantCulture)),
            new("hardware-vendor", device.HardwareDevice.Vendor),
            new("hardware-device", device.HardwareDevice.DeviceId.ToString(CultureInfo.InvariantCulture)),
            new("hardware-type", ((int)device.HardwareDevice.Type).ToString(CultureInfo.InvariantCulture)),
        }
        .Concat(device.EpMetadata.Entries.Select(static row => new KeyValuePair<string, string>($"ep-meta:{row.Key}", row.Value)))
        .Concat(device.EpOptions.Entries.Select(static row => new KeyValuePair<string, string>($"ep-option:{row.Key}", row.Value)))
        .Concat(device.HardwareDevice.Metadata.Entries.Select(static row => new KeyValuePair<string, string>($"hardware-meta:{row.Key}", row.Value))));

    static Fin<T> Fault<T>(Exception error) =>
        Fin.Fail<T>(new ComputeFault.ModelRejected(error.Message));

    static Option<ArtifactIndexRow> AdmitContext(ResidentKey resident, Option<OrtEpDevice> device, string path, SessionPolicy policy, Instant at) =>
        File.Exists(path)
            ? from store in BlobStore
              let bytes = (ReadOnlyMemory<byte>)File.ReadAllBytes(path)
              from _ in store(bytes).ToOption()
              select ArtifactIndexRow.Admit(
                  ArtifactKind.EpContext,
                  ContextKey(resident, device),
                  bytes,
                  policy.WarmStartClassification,
                  at,
                  Some(resident.Checksum))
            : None;

    static long[] RepresentativeShape(ModelIdentity model) =>
        model.Inputs.Head
            .Bind(static slot => slot.Shape is SlotShape.Tensor tensor ? Some(tensor.Dims.Map(static dim => dim <= 0 ? 1L : dim).ToArray()) : None)
            .IfNone([1L]);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
