# [COMPUTE_SESSIONS]

`ModelSessions` shares one `InferenceSession` per policy-complete `ResidentKey`, one device-keyed warm artifact, and one allocator lease map. A fleet roster keyed on the profile-stripped identity owns the capped registered shape buckets and their measured warm evidence; the capsule serializes lifecycle, binds every cold load to a cancellation latch, and admits every warm artifact through `AdmitContext`.

`SessionPolicy`, `ResidentKey`, and `ModelSessions` own boot, lease, open, allocation, warmup, eviction, drain, and compilation. ONNX Runtime supplies session and allocator surfaces; `CpuBudget`, `CancelScope`, receipt ports, identity fingerprints, provider policy with its `WarmForm` axis, and `BoundFlow` arrive settled.

## [01]-[INDEX]

- [02]-[SESSION_CAPSULE]: one `Gate`-serialized shared session per policy-complete `ResidentKey` with lifecycle, warmup, and drain rows, a profile-stripped fleet bucket roster, a shared-device-allocator lease, a deadline-bound cold load with its mapped-graph fallback, and a fleet-shared device-keyed warm artifact under one `ContextKey` derivation spanning every warm-start form.

## [02]-[SESSION_CAPSULE]

- Owner: `SessionPolicy` lifecycle record with its `SessionRows` fingerprint projection; `ResidentKey` the policy-complete resident identity (`Checksum` + the `ModelFingerprint.Of` fold over every construction-behavior column) beside its profile-stripped `Warm` projection; `PlannedLoad` the options-plus-latch load capsule; `WarmSite` one cold open's warm decision (artifact key, path, admissibility, optimization level); `ModelSessions` capsule owning the `Gate`-serialized OrtEnv boot with its token-gated blob leg, the resident-session map (each `Resident` carries `ExecutionProvider`, its warm key, and warm-start `Option<ArtifactIndexRow>`), the fleet `WarmRoster` map keyed on that warm key, the shared-device-allocator lease map, the selected `SessionPlacement` readback, the single form-aware `ContextKey` warm identity, the single `AdmitContext` warm-artifact owner, the single `Faulted` native-fault classifier, and the warmup, idle-eviction, drain, and sweep rows.
- Law: warm-up is PER BUCKET. ORT plans its memory pattern per executed shape, so one warm pulse at one representative shape leaves every other shape a cold first run, and a caller that tiles against fixed buckets runs exactly the shapes it declared. Consumers REGISTER each bucket they will run, capped by `SessionPolicy.WarmBuckets` so a per-request extent never grows an unbounded warm set, and the sweep then pulses every registered bucket on the resident's own session.
- Law: every WARM identity strips profiling and session identity does not. `SessionRows` keeps the `profiling` column because the flag changes the session ORT builds, so two residents cannot alias; but the bucket roster, the partition census, and the warm artifact are properties of the OPTIMIZED GRAPH and its EP set, which profiling moves not at all. `ResidentKey.Warm` therefore re-derives the fingerprint over a profile-cleared policy, and the roster map, `Warm`, `Partitions`, `ContextKey`, and the fleet `Compile` all key there — so the one profiling lease that reads a graph's partition census publishes evidence every production resident reads, and one compiled context serves both. Keying them to the full identity strands each measurement inside the lease that took it, leaves the production resident reading an absent count forever, and re-compiles a context the fleet already owns.
- Law: rosters OUTLIVE residents. A partition census belongs to the graph rather than to a session, so eviction drops the session and keeps the roster: a re-leased resident reads back the measurement the fleet already paid for, and the map bounds at the model-and-policy product the fleet actually opens rather than at the residency cap. `Drain` clears it whole because that is process teardown, not eviction.
- Law: each bucket carries the evidence its own pulse MEASURED — graph partition count, pulse duration, warm instant — and nothing else. `WarmEvidence` replaces `Unit` on the pulse return because the caller that ran the shape is the only surface observing how the graph partitioned for it; an unmeasured column stays `None` and a consumer needing it refuses rather than reading a zero as an observation. Buckets only ever seat a shape someone will RUN: seeding admits a fully static model signature and nothing else, and a registration re-binding a seated key to a different shape refuses instead of keeping the first shape under the second consumer's name.
- Law: COLD opens build native state outside `Gate`. Session construction runs graph optimization and provider compilation, so serializing it behind the one lock stalls every lease of every other model for the duration; two threads racing one cold key both build, `Publish` seats the first, and the loser disposes its own build and leases the seated resident — one redundant build instead of a serialized fleet, and never two live sessions forking one warm roster.
- Law: that same cold build is CANCELLABLE. `SetLoadCancellationFlag` registered off the lease's own `CancelScope` is the load-time counterpart to the run-side `Terminate` latch, so a deadline bounds the load it precedes rather than only the run that follows it, a shutting-down host aborts the cold opens a `SweepRow` stacked instead of paying every one, and a cancelled first pass stops re-paying the full compile. `Faulted` then classifies by provenance — a fired deadline is `DeadlineExpired`, another cancellation is `Cancelled` — because a latch-aborted load raises the same `OnnxRuntimeException` a genuine rejection does, and reporting a cancelled load as `ModelRejected` sends a retry policy after a model that was never wrong.
- Law: the warm form decides what a cold open LOADS, and the managed hit loads BY PATH. `WarmForm.EpContext` leaves the source bytes and the policy's optimization level alone and reads its blob through the bound `ep.context_*` keys; `WarmForm.OptimizedGraph` opens the artifact ITSELF at `ORT_DISABLE_ALL` through the path-taking `InferenceSession(string, SessionOptions, PrePackedWeightsContainer)` constructor, so ORT memory-maps a graph that runs to hundreds of megabytes and the process pays no managed copy of it — reading that file into an array and handing the array over costs one full read AND one full copy of bytes ORT maps anyway, which is most of the cost the form exists to remove. Model bytes materialize to a `byte[]` exactly ONCE per open or compile, at the single native hand-off that demands an array. A mapped graph that will not construct answers MISS by re-siting at the policy's own level, so a file deleted between the existence probe and the open, or truncated by a crashed writer, costs one cold optimization rather than a refused lease.
- Entry: `public static Fin<SessionLease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock)` aborts on rejected admission; every lease carries the resident's own stored warm row — a hit reports the artifact its resident opened against rather than reporting none, because warm-start evidence is a property of the SESSION and a consumer that leased a warmed resident otherwise reads a cold one — and `SessionLease.Dispose` stamps release time and decrements the resident hold exactly once. `WarmStartAdmissible` dispatches on the provider row's own `WarmForm` — the EP-context arm reads the compiled artifact's embedded compat info while the managed and engine-cache arms read presence against a runtime-keyed artifact name — so an incompatible, stale, or absent warm artifact degrades to a fresh session without one. `SessionLease` CARRIES both identities, so `public static Fin<Unit> Warm(ResidentKey warm, string bucket, long[] shape)` and `public static Option<int> Partitions(ResidentKey warm, string bucket)` reach the fleet roster off the handle a consumer already holds rather than re-deriving it from model, provider, and policy.
- Auto: `Boot` binds the blob leg under `Gate` TOKEN-GATED — a second boot passing the same leg is idempotent and one passing a different leg refuses typed, because every admitted context row names an artifact this leg wrote and a silent rebind re-homes every later write while every earlier row keeps pointing at the first store. `CustomOpLibrary.Admit` content-keys each native asset before it enters `SessionPolicy`; `Admit` re-hashes the model bytes and every custom-op asset, rejects nonpositive capacity/durations, invalid or duplicate free dimensions and initializers, zero initializer content identities, duplicate custom-op paths, and every initializer that misses the model's exact tensor schema before `Lease` or `Compile` reaches native state. `Options` then folds the site's optimization level, free dimensions, initializers, execution, memory, profiling, device policy, provider registration, the row's own `BindWarm` warm-state write, custom ops, and finally the load-cancellation registration once for both open and fleet compile, returning the `PlannedLoad` capsule that owns both halves. `ResidentKey.Of(model, ep, policy)` joins checksum with the `ModelFingerprint.Of` fold over every construction-behavior column, including each initializer and custom-op content key, so equal paths or names carrying different bytes cannot alias one resident or compiled context, while `ResidentKey.Warm` re-folds that same projection over a profile-cleared policy for every roster and artifact identity. `DeviceFingerprint` folds the EP, hardware, and provider metadata tables into context and allocator identity too. `Placement` zips ordered input names with `GetEpDeviceForInputs` and `GetMemoryInfosForInputs`, zips output names with `GetMemoryInfosForOutputs`, and rejects any native cardinality mismatch before returning provider/memory evidence. `Lease` increments `Resident.Leases` under `Gate`; `SessionLease.Dispose` decrements once through `Interlocked.Exchange`; `Unload` removes only zero-lease residents older than its threshold; `Drain` releases shared allocators only after no resident remains. `Warmup` acquires temporary leases over its snapshot, expands each resident against its own bucket roster, and releases every lease in `finally`, so a sweep cannot dispose a pulsed session; the bucket fold ACCUMULATES rather than short-circuits, so one unwarmable shape refuses alone instead of leaving every later bucket and every later resident cold on a schedule refusing at the same point each sweep; each pulse's measured evidence folds back onto its bucket row under `Gate` before the receipt mints, so evidence and receipt cannot disagree. `Site` derives the warm decision once — the form's own artifact key, its admissibility, and the bytes and level the session takes — and `Open` then admits through the single `AdmitContext` owner under the same `ContextKey(ResidentKey, device, WarmForm)` the fleet `Compile` writes, publishing on the side of the miss its form sits on: an EP-context open indexes the blob it bound, a managed open indexes the graph its construction wrote. `Compile` refuses a row whose warm form compiles no partition and forces its own site to a miss, so a fleet sweep never reads the file it is about to overwrite. `StaticShape` reads the first `SlotShape.Tensor` and answers only when every dimension is fixed, so `Seed` lands a first bucket for a model whose signature already names one shape and lands NOTHING for a model carrying a free dimension — a dynamic dim collapsed to `1` mints a shape no consumer runs and a downsampling graph refuses outright.
- Receipt: `Warmup` returns `Fin<Seq<(ComputeReceipt.Warmup, Option<ArtifactIndexRow>)>>`, preserving any pulse fault and carrying one checksum, provider, BUCKET KEY, and warm-start row per warmed bucket — the receipt's shape column carries the bucket spelling the registering consumer used, so a fleet reading warm facts and a stage reading its own bucket name the same string and no receipt case widens; `DrainRow` emits one `ComputeReceipt.Drain(Drained, 0, 0)` on `DrainBand.Compute`, where `Drained` is the unloaded-session count and the capsule owns no admission queue. Both carry `ReceiptScope.Execution(Substrate.Onnx, WorkLane.Background, AllocationClass.NativeOrt)` and drain emission crosses the sink-bound `ReceiptSurface` under one `CorrelationId`.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a lifecycle change is one `SessionPolicy` value; a new construction-behavior column is one `SessionRows` row that automatically re-keys residency and — unless it is trace-only, which `ResidentKey.Warm` clears — the warm artifacts too; a further warm-start mechanism is one `Model/providers#EP_AXIS` `WarmForm` row whose suffix and runtime-keying columns this derivation already reads, never a second load path here; a further load-time bound is one more registration on the existing `PlannedLoad`, never a second cancellation owner; the warm-start and the fleet compile both admit through the single `AdmitContext` owner over the single `ContextKey` derivation, never a second cache, artifact owner, or filename scheme; the fleet-shared context is one `Compile` member publishing a `ContextKey(ResidentKey, device, WarmForm)`-keyed `ArtifactIndexRow` through the same blob-lane owner, never a second EP-cache; a warmup or drain fact is one existing `ComputeReceipt.Warmup`/`Drain` case through the one `ReceiptSurface`, never a parallel receipt owner; a new warm strategy is the injected `pulse`, never a second warm surface, and a further shape to warm is one `Warm` registration on the existing roster; a further measured warm fact is one `WarmEvidence` column folded onto the bucket row, never a second evidence owner; a quantized session is `SessionPolicy.Precision` set to `Int8`/`Int4` OVER settled pre-quantized model bytes — the row is execution posture (`QuantizedGraph` evidence, MatMulNBits accuracy floor, accumulation), never a graph transform, and the quantized graph carries its own checksum identity — flowing through the existing `Options` rail with residency and context reuse re-keyed by the same `SessionRows` fold, never a quantization-specific owner; a sequential-versus-parallel posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner.
- Boundary: `ModelSessions` is the `CAPSULE_OWNER`. ORT sessions are thread-safe for concurrent `Run`, so all lanes share one `InferenceSession` per `ResidentKey`; `SessionLease` is the only lifetime handed to a run. `Gate` serializes boot — including the token-gated blob-leg bind, a static every context admission reads — beside resident acquire/publish/release/eviction, every warm-roster registration and observation, and shared allocator create/release; native session and context CONSTRUCTION stays outside it, which is the whole reason a cold open can lose a race and dispose its own build. Immutable maps replace retry-capable `Atom.Swap` mutation, so no capture or native effect can replay. `PlannedLoad` is transient and disposes after `InferenceSession` or `OrtModelCompilationOptions` consumes its options, releasing the load latch first so the token can never reach freed native state; `PrePackedWeightsContainer` alone spans sessions. `DisablePerSessionThreads` binds every session to the global pool `Boot` derives from `CpuBudget`. Warm artifacts — compiled `ep.context_*` blobs and managed optimized graphs alike — and profiles land WRITE-BLOB-FIRST: the Boot-bound Persistence object-store leg persists the bytes, and only durable residence publishes the `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)` row — an unbound leg or a failed write publishes no row, so a dangling index cannot name unavailable content; retention derives from `ArtifactKind.Retention`, and each warm artifact projects under its model checksum. `ContextKey(ResidentKey, device, WarmForm)` is the sole warm identity for lookup, compilation, admission, and transport, and it takes the PROFILE-STRIPPED key: it cannot alias sessions whose provider options or construction policy differ, it deliberately DOES alias a profiling session with the production one that shares its graph, the form's suffix keeps the two mechanisms' artifacts distinct inside one derivation, and the runtime version enters only for the form carrying no compat info of its own — so a managed graph another ORT wrote misses its key instead of loading under a runtime that never produced it. `Placement` closes autoEP selection and I/O memory residency with post-construction evidence. Shared allocators release only after all resident leases drain; `Unload` never disposes a session under an active run.

```csharp signature
public sealed record SessionPolicy(
    int ResidentSessions, Duration IdleUnload, Duration WarmupSweep,
    GraphOptimizationLevel Optimization, ExecutionMode Execution, bool MemoryPattern, bool Profiling,
    bool OrtExtensions, Seq<SessionPolicy.CustomOpLibrary> CustomOpLibraries, Seq<(string Dim, long Value)> FreeDims,
    Seq<SessionPolicy.Initializer> Initializers,
    ModelPrecision Precision,
    int WarmBuckets,
    DataClassification WarmStartClassification) {
    public static readonly SessionPolicy Canonical = new(
        ResidentSessions: 4, IdleUnload: Duration.FromMinutes(10), WarmupSweep: Duration.FromMinutes(5),
        Optimization: GraphOptimizationLevel.ORT_ENABLE_ALL, Execution: ExecutionMode.ORT_SEQUENTIAL,
        MemoryPattern: true, Profiling: false,
        OrtExtensions: false, CustomOpLibraries: Seq<CustomOpLibrary>(), FreeDims: Seq<(string Dim, long Value)>(),
        Initializers: Seq<Initializer>(),
        Precision: ModelPrecision.Full,
        WarmBuckets: 8,
        WarmStartClassification: DataClassification.Operational);

    public Fin<Unit> Conforms() =>
        guard(
            ResidentSessions > 0
            && WarmBuckets > 0
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
    // WarmBuckets is deliberately ABSENT: it caps a roster the sweep reads and changes no built session, so folding it
    // would re-key every resident and every compiled context on a cap edit that alters nothing native.
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

    // Profiling changes the TRACE a session emits, never the graph it optimized or the partitions an EP claimed, so
    // every artifact and every measurement keyed to the optimized graph keys HERE instead: one profiling lease
    // measures the partition census and writes the warm artifact that every production resident then reads. The
    // full identity keeps `profiling` because two residents must not alias; stranding the census inside the
    // profiling lease is what left the production resident reading an absent count forever.
    public static ResidentKey Warm(ModelIdentity model, ExecutionProvider ep, SessionPolicy policy) =>
        Of(model, ep, policy with { Profiling = false });
}

// Load-side twin of the `Model/inference#INFERENCE_MODES` `PlannedRun` capsule: built options plus the
// registration that arms them, released latch-first so a token firing into disposed native options is structurally
// impossible. Both the cold open and the fleet compile hold it in the one `using` bracket they already had.
public sealed record PlannedLoad(SessionOptions Options, CancellationTokenRegistration Latch) : IDisposable {
    public void Dispose() {
        Latch.Dispose();
        Options.Dispose();
    }
}

// One cold open's warm decision: the artifact key this provider's own `WarmForm` names, where that artifact sits,
// whether it admits, and the optimization level the session therefore takes. The site carries NO byte column — the
// managed hit opens its artifact by PATH so ORT maps the file, every other arm opens the caller's own source bytes,
// and a site holding a whole optimized graph in managed memory pays the copy the mapped form exists to delete.
public readonly record struct WarmSite(string Key, string Path, bool Admissible, GraphOptimizationLevel Level);

public static class ModelSessions {
    public sealed record SessionPlacement(
        Seq<(string Name, Option<string> Provider, string Memory)> Inputs,
        Seq<(string Name, string Memory)> Outputs);

    // Leases CARRY both identities: `Key` releases the session it holds, `Warm` reaches the fleet roster a bucket
    // registration and a partition read both key on. A holder unable to name either re-derives it from model,
    // provider, and policy — a second derivation of an identity this capsule already computed, and the one place
    // the two silently diverge.
    public sealed class SessionLease : IDisposable {
        readonly IClock clock;
        int released;

        internal SessionLease(ResidentKey key, ResidentKey warm, InferenceSession session, Option<ArtifactIndexRow> warmStart, IClock clock) {
            this.clock = clock;
            Key = key;
            Warm = warm;
            Session = session;
            WarmStart = warmStart;
        }

        public ResidentKey Key { get; }
        public ResidentKey Warm { get; }
        public InferenceSession Session { get; }
        public Option<ArtifactIndexRow> WarmStart { get; }

        public void Dispose() {
            if (Interlocked.Exchange(ref released, 1) is 0) { Release(Key, clock.GetCurrentInstant()); }
        }
    }

    // What one warm pulse OBSERVED. The runner owns the observation because only the surface that ran the shape can
    // read how the graph partitioned for it; a column nobody measured stays None rather than reading as zero.
    public readonly record struct WarmEvidence(Option<int> Partitions);

    // One registered shape and everything measured about it. Key is the CONSUMER's spelling — a tile bucket edge, a
    // sequence length — never a re-derivation of the tensor dims, so the surface that registers and the surface that
    // reads back name one string.
    public readonly record struct WarmBucket(
        string Key, long[] Shape, Option<int> Partitions, Option<Duration> Elapsed, Option<Instant> WarmedAt);

    sealed record Resident(
        InferenceSession Session, ExecutionProvider Ep, ResidentKey Warm,
        Option<ArtifactIndexRow> WarmStart, Instant LastUsed, int Leases);

    // Bucket rosters seat OFF the resident record and key on the profile-stripped identity, so the profiling lease
    // that measures a partition census and the production resident that reads it name ONE roster; a roster carried
    // on `Resident` binds every measurement to the session that took it and dies with the first eviction.
    sealed record WarmRoster(HashMap<string, WarmBucket> Buckets, int Cap);

    sealed record DeviceArena(OrtEpDevice Device, OrtDeviceMemoryType Memory, OrtAllocator Allocator);

    static HashMap<ResidentKey, Resident> Residents = HashMap<ResidentKey, Resident>();
    static HashMap<ResidentKey, WarmRoster> WarmRosters = HashMap<ResidentKey, WarmRoster>();
    static HashMap<(ulong Device, OrtDeviceMemoryType Memory), DeviceArena> SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
    static readonly PrePackedWeightsContainer PrePacked = new();
    static readonly Lock Gate = new();

    // Blob-lane write leg binds once at Boot from the Persistence object-store composition; AdmitContext writes
    // context bytes durable-first through it and publishes the index row only after residence, so an unbound
    // leg publishes nothing rather than a dangling index.
    static Option<Func<ReadOnlyMemory<byte>, Fin<UInt128>>> BlobStore = Option<Func<ReadOnlyMemory<byte>, Fin<UInt128>>>.None;

    // Blob-leg binding rides the SAME gate every other static here answers to: unlocked writes to a field the
    // context admitter reads are the one race this capsule would otherwise keep, priced at an uncontended lock.
    // Binding is TOKEN-GATED rather than last-writer-wins — every admitted context row names an artifact this leg
    // wrote, so a second boot handing a different store silently re-homes every later write while every row already
    // published keeps pointing at content the new store does not hold; the same leg re-boots idempotently.
    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget, Func<ReadOnlyMemory<byte>, Fin<UInt128>> blobStore) {
        lock (Gate) {
            if (BlobStore.Case is Func<ReadOnlyMemory<byte>, Fin<UInt128>> seated && !ReferenceEquals(seated, blobStore)) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected("<session-boot-blob-rebind>"));
            }
            BlobStore = Some(blobStore);
            if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
            using OrtThreadingOptions pool = new() { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
            EnvironmentCreationOptions creation = new() { logId = logId, logLevel = severity, threadOptions = pool };
            OrtEnv.CreateInstanceWithOptions(ref creation);
            OrtEnv.Instance().DisableTelemetryEvents();
            return Fin.Succ(unit);
        }
    }

    // `IClock` alone for time: leasing stamps residency instants and measures no interval, so the elapsed-time
    // provider the warm sweep and the run bracket carry has no reader on this path. `CancelScope` rides beside it
    // because a COLD lease is the folder's slowest native call — the deadline that bounds a run has to bound the
    // load that precedes it or the bound is fiction.
    public static Fin<SessionLease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock) =>
        Admit(model, bytes, policy).Bind(_ => LeaseAdmitted(model, bytes, ep, policy, artifactDir, scope, clock));

    // COLD opens build native state outside the gate. Constructing an `InferenceSession` runs graph optimization
    // and provider compilation — seconds for a UNet — and holding the one lock across it stalls every lease of
    // every other model behind it, which is the whole fleet during a pipeline's first pass. Two threads racing one
    // cold key both build and `Publish` seats the first, so the race costs one redundant build instead of a
    // serialized fleet, and a warm hit never touches native state at all.
    static Fin<SessionLease> LeaseAdmitted(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock) {
        ResidentKey key = ResidentKey.Of(model, ep, policy);
        ResidentKey warm = ResidentKey.Warm(model, ep, policy);
        if (Acquire(key, clock).Case is SessionLease held) { return Fin.Succ(held); }
        return Open(warm, bytes, ep, policy, artifactDir, scope, clock, ep.AutoSelect)
            .Map(opened => Publish(key, warm, model, ep, policy, opened.Session, opened.WarmStart, clock));
    }

    // A hit reports the resident's OWN warm row: warm-start evidence is a property of the session, so answering
    // `None` told every consumer after the first that it had leased a cold session and left a fleet reading warm
    // facts off the one lease that happened to open the artifact.
    static Option<SessionLease> Acquire(ResidentKey key, IClock clock) {
        lock (Gate) {
            if (Residents.Find(key).Case is not Resident resident) { return None; }
            Residents = Residents.SetItem(key, resident with { LastUsed = clock.GetCurrentInstant(), Leases = resident.Leases + 1 });
            return Some(new SessionLease(key, resident.Warm, resident.Session, resident.WarmStart, clock));
        }
    }

    // Race LOSERS dispose their own build and lease the seated resident: two live sessions under one key double the
    // native arena for a model the fleet counts once.
    static SessionLease Publish(ResidentKey key, ResidentKey warm, ModelIdentity model, ExecutionProvider ep, SessionPolicy policy, InferenceSession session, Option<ArtifactIndexRow> warmStart, IClock clock) {
        Instant now = clock.GetCurrentInstant();
        lock (Gate) {
            if (Residents.Find(key).Case is Resident raced) {
                Residents = Residents.SetItem(key, raced with { LastUsed = now, Leases = raced.Leases + 1 });
                session.Dispose();
                return new SessionLease(key, raced.Warm, raced.Session, raced.WarmStart, clock);
            }
            // Roster seeding is FIRST-WRITER: a roster already standing carries measurements this open must not
            // reset, and it legitimately outlives the resident that seeded it.
            WarmRosters = WarmRosters.Find(warm).IsSome
                ? WarmRosters
                : WarmRosters.Add(warm, new WarmRoster(Seed(model), policy.WarmBuckets));
            Resident fresh = new(session, ep, warm, warmStart, now, Leases: 1);
            HashMap<ResidentKey, Resident> next = Residents.Add(key, fresh);
            Seq<(ResidentKey Key, Resident Held)> evicted = toSeq(next.AsIterable()
                .Filter(pair => pair.Key != key && pair.Value.Leases is 0)
                .OrderBy(static pair => pair.Value.LastUsed)
                .Take(Math.Max(next.Count - policy.ResidentSessions, 0)));
            Residents = evicted.Fold(next, static (state, row) => state.Remove(row.Key));
            evicted.Iter(static row => row.Held.Session.Dispose());
            return new SessionLease(key, warm, session, warmStart, clock);
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

    // Registration is IDEMPOTENT under the SAME shape and REFUSES a different one: a repeat must not reset the
    // evidence its last pulse measured, and a second consumer binding a different tensor shape to a key already
    // taken would silently keep the first shape and then read the first shape's partition count back as its own.
    // Capping refuses a shape growing the roster past the policy rather than evicting one a caller still runs.
    public static Fin<Unit> Warm(ResidentKey warm, string bucket, long[] shape) {
        lock (Gate) {
            if (WarmRosters.Find(warm).Case is not WarmRoster roster) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<warm-roster:{warm.Checksum:x32}>"));
            }
            if (bucket.Length is 0 || shape.Length is 0 || shape.Any(static dim => dim <= 0L)) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<warm-bucket-shape:{bucket}>"));
            }
            if (roster.Buckets.Find(bucket).Case is WarmBucket seated) {
                return seated.Shape.AsSpan().SequenceEqual(shape)
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<warm-bucket-conflict:{bucket}>"));
            }
            if (roster.Buckets.Count >= roster.Cap) {
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<warm-bucket-cap:{roster.Buckets.Count}:{roster.Cap}>"));
            }
            WarmRosters = WarmRosters.SetItem(warm, roster with {
                Buckets = roster.Buckets.Add(bucket, new WarmBucket(bucket, shape, None, None, None)),
            });
            return Fin.Succ(unit);
        }
    }

    public static Option<int> Partitions(ResidentKey warm, string bucket) {
        lock (Gate) {
            return WarmRosters.Find(warm).Bind(roster => roster.Buckets.Find(bucket)).Bind(static row => row.Partitions);
        }
    }

    public static Fin<Seq<(ComputeReceipt.Warmup Receipt, Option<ArtifactIndexRow> WarmStart)>> Warmup(
        Func<InferenceSession, long[], Fin<WarmEvidence>> pulse,
        CorrelationId correlation,
        IClock clock, TimeProvider time) {
        Seq<(ResidentKey Key, Resident Held, SessionLease Lease, Seq<WarmBucket> Buckets)> held;
        lock (Gate) {
            held = Residents.AsIterable().Map(pair => (
                Key: pair.Key,
                Held: pair.Value,
                Lease: new SessionLease(pair.Key, pair.Value.Warm, pair.Value.Session, pair.Value.WarmStart, clock),
                Buckets: WarmRosters.Find(pair.Value.Warm)
                    .Map(static roster => roster.Buckets.Values.ToSeq())
                    .IfNone(Seq<WarmBucket>()))).ToSeq();
            Residents = held.Fold(Residents, static (state, row) =>
                state.SetItem(row.Key, row.Held with { Leases = row.Held.Leases + 1 }));
        }
        try {
            // One lease per resident spans every bucket that resident registered: the roster expands INSIDE the
            // held snapshot, so a sweep never re-leases per shape and never pulses a session it has released.
            // Buckets ACCUMULATE rather than short-circuit: a monadic traverse stops at the first refusing shape and
            // leaves every later bucket — and every later resident — cold on a schedule that would refuse there again
            // each sweep, so one unwarmable shape would permanently starve the fleet. The applicative fold attempts
            // every bucket and carries every fault, and a partly-failed sweep still leaves the reachable shapes warm.
            return held
                .Bind(row => row.Buckets.Map(bucket => (row.Held, row.Lease, Bucket: bucket)))
                .Traverse(row => {
                    long mark = time.GetTimestamp();
                    return pulse(row.Lease.Session, row.Bucket.Shape).Map(evidence => {
                        Duration elapsed = time.GetElapsedTime(mark);
                        Observe(row.Lease.Warm, row.Bucket.Key, evidence, elapsed, clock.GetCurrentInstant());
                        return (
                            new ComputeReceipt.Warmup($"{row.Lease.Key.Checksum:x32}", row.Held.Ep, row.Bucket.Key) {
                                Scope = new ReceiptScope.Execution(
                                    correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
                            },
                            row.Held.WarmStart);
                    }).ToValidation();
                }).As().ToFin();
        }
        finally { held.Iter(static row => row.Lease.Dispose()); }
    }

    // Absence never overwrites: a pulse measuring nothing KEEPS the prior observation, while timing and the warm
    // instant always land because the sweep itself observed both.
    static void Observe(ResidentKey warm, string bucket, WarmEvidence evidence, Duration elapsed, Instant at) {
        lock (Gate) {
            if (WarmRosters.Find(warm).Case is WarmRoster roster && roster.Buckets.Find(bucket).Case is WarmBucket row) {
                WarmRosters = WarmRosters.SetItem(warm, roster with {
                    Buckets = roster.Buckets.SetItem(bucket, row with {
                        Partitions = evidence.Partitions.IsSome ? evidence.Partitions : row.Partitions,
                        Elapsed = Some(elapsed),
                        WarmedAt = Some(at),
                    }),
                });
            }
        }
    }

    public static Seq<ResidentKey> Unload(Instant idleBefore) {
        Seq<(ResidentKey Key, Resident Held)> evicted;
        lock (Gate) {
            evicted = Residents.AsIterable()
                .Filter(pair => pair.Value.Leases is 0 && pair.Value.LastUsed < idleBefore)
                .ToSeq();
            Residents = evicted.Fold(Residents, static (state, row) => state.Remove(row.Key));
        }
        evicted.Iter(static pair => pair.Held.Session.Dispose());
        return evicted.Map(static pair => pair.Key);
    }

    // Rosters clear HERE and never at eviction: a partition census belongs to the graph, so a re-leased resident
    // reads back what the fleet already measured, while a drain IS process teardown and leaves nothing standing.
    public static int Drain() {
        int drained = Unload(Instant.MaxValue).Count;
        lock (Gate) {
            if (!Residents.IsEmpty) { return drained; }
            Seq<DeviceArena> arenas = toSeq(SharedAllocators.Values);
            SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
            WarmRosters = HashMap<ResidentKey, WarmRoster>();
            arenas.Iter(static arena => OrtEnv.Instance().ReleaseSharedAllocator(arena.Device, arena.Memory));
        }
        return drained;
    }

    public static DrainParticipantPort DrainRow(ReceiptSurface receipts, CorrelationId correlation, TimeProvider time) =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, _ =>
            from mark in IO.lift(time.GetTimestamp)
            from drained in IO.lift(Drain)
            from sent in receipts.Emit(new ComputeReceipt.Drain(drained, 0, 0) {
                Scope = new ReceiptScope.Execution(
                    correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, time.GetElapsedTime(mark)),
            })
            select unit);

    public static ScheduleEntry SweepRow(SessionPolicy policy, IClock clock, Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(policy.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None,
            () => IO.lift(() => Unload(clock.GetCurrentInstant() - policy.IdleUnload)).Bind(_ => warm()));

    // ONE warm-artifact identity across both forms: the model checksum, the PROFILE-STRIPPED construction-behavior
    // fingerprint, the device fingerprint when a device selected the build, the RUNTIME version for a form whose
    // artifact embeds no compat info of its own, and the form's own suffix. Lookup, compilation, admission, and
    // transport read this one derivation, so no second filename scheme exists and no form can alias another's
    // artifact — while a profiling session and the production session sharing its graph deliberately DO share one,
    // because tracing moves no optimization the artifact holds.
    public static string ContextKey(ResidentKey resident, Option<OrtEpDevice> device, WarmForm form) =>
        $"{resident.Checksum:x32}:{resident.Options:x16}"
        + device.Match(Some: static held => $":{DeviceFingerprint(held):x16}", None: static () => string.Empty)
        + (form.RuntimeKeyed ? $":{RuntimeFingerprint():x16}" : string.Empty)
        + $".{form.Suffix}";

    static ulong RuntimeFingerprint() =>
        ModelFingerprint.Of(Seq(new KeyValuePair<string, string>("ort", OrtEnv.Instance().GetVersionString())));

    // Fleet compile is the EP-CONTEXT form's own verb: `OrtModelCompilationOptions` partitions subgraphs into a
    // context blob, and a row warming through any other mechanism has no partition to compile at all — the managed
    // graph is a serialization and an engine cache is the EP's own private product — so `ERROR_IF_NO_NODES_COMPILED`
    // would fault a whole fleet sweep over a row that never had one.
    public static Fin<ArtifactIndexRow> Compile(ReadOnlyMemory<byte> bytes, OrtEpDevice device, ModelIdentity model, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, Instant at) =>
        Admit(model, bytes, policy)
            .Bind(_ => guard(
                ReferenceEquals(ep.Warm, WarmForm.EpContext),
                (Error)new ComputeFault.ModelRejected($"<compile-warm-form:{ep.Key}:{ep.Warm.Key}>")).ToFin())
            .Bind(_ => Compiled(bytes, device, ResidentKey.Warm(model, ep, policy), ep, policy, artifactDir, scope, at));

    // Fleet compile always PRODUCES, so its site reports no admissible artifact: a site answering true arms
    // `ep.context_enable=1` on the very options writing that path, and the compile would read the file it is about
    // to overwrite. Row admissibility governs a session OPEN; the compile's product IS the artifact.
    static Fin<ArtifactIndexRow> Compiled(ReadOnlyMemory<byte> bytes, OrtEpDevice device, ResidentKey resident, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, Instant at) =>
        Site(resident, ep, policy, artifactDir, Seq(device)) with { Admissible = false, Level = policy.Optimization } switch {
            var site => Options(ep, policy, artifactDir, site, scope, Seq(device))
                .Bind(load => CompileAdmitted(resident, bytes, policy, site, scope, at, load)),
        };

    static Fin<ArtifactIndexRow> CompileAdmitted(ResidentKey resident, ReadOnlyMemory<byte> bytes, SessionPolicy policy, WarmSite site, CancelScope scope, Instant at, PlannedLoad load) {
        using (load) {
            try {
                using OrtModelCompilationOptions compile = new(load.Options);
                // ONE materialization of the model bytes on this leg — the compile API takes an array, so the copy
                // is forced exactly here and nowhere earlier in the chain.
                compile.SetInputModelFromBuffer(bytes.ToArray());
                compile.SetOutputModelPath(site.Path);
                compile.SetEpContextEmbedMode(true);
                compile.SetGraphOptimizationLevel(policy.Optimization);
                compile.SetFlags(OrtCompileApiFlags.ERROR_IF_NO_NODES_COMPILED);
                compile.CompileModel();
                return AdmitContext(site, resident, policy, at).Case is ArtifactIndexRow row
                    ? Fin.Succ(row)
                    : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected($"<ep-context-compile-empty:{resident.Checksum:x32}>"));
            }
            catch (OnnxRuntimeException error) { return Fault<ArtifactIndexRow>(scope, error); }
            catch (IOException error) { return Fault<ArtifactIndexRow>(scope, error); }
            catch (UnauthorizedAccessException error) { return Fault<ArtifactIndexRow>(scope, error); }
            catch (ArgumentException error) { return Fault<ArtifactIndexRow>(scope, error); }
        }
    }

    // The site names WHERE the artifact sits and at what level the session opens; it carries no bytes, because the
    // managed hit reads its graph by path and every other arm reads the caller's own source memory.
    static WarmSite Site(ResidentKey key, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Seq<OrtEpDevice> devices) =>
        ContextKey(key, devices.Head, ep.Warm) switch {
            var artifactKey => Path.Combine(artifactDir, artifactKey) switch {
                var location => ep.WarmStartAdmissible(location, devices) switch {
                    var admissible => new WarmSite(
                        artifactKey,
                        location,
                        admissible,
                        admissible && ReferenceEquals(ep.Warm, WarmForm.OptimizedGraph)
                            ? GraphOptimizationLevel.ORT_DISABLE_ALL
                            : policy.Optimization),
                },
            },
        };

    // Native construction ONLY: no resident map, no lease, no eviction. Everything this returns is unpublished, so a
    // fault here strands nothing and the caller alone decides whether the built session is seated or disposed. A
    // MAPPED graph that will not construct re-sites at a miss and opens the source bytes at the policy's own level,
    // so a file deleted between the existence probe and the open costs one cold optimization instead of the lease —
    // and the retry arms on a rejection alone, because re-running a compile a fired deadline just aborted is the
    // one thing a deadline exists to prevent.
    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Open(ResidentKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock, Seq<OrtEpDevice> devices) =>
        Site(warm, ep, policy, artifactDir, devices) switch {
            var site => Opened(warm, bytes, ep, policy, site, scope, clock, devices, artifactDir)
                .BindFail(fault => site.Admissible && ReferenceEquals(ep.Warm, WarmForm.OptimizedGraph) && fault is ComputeFault.ModelRejected
                    ? Opened(
                        warm, bytes, ep, policy,
                        site with { Admissible = false, Level = policy.Optimization },
                        scope, clock, devices, artifactDir)
                    : Fin.Fail<(InferenceSession, Option<ArtifactIndexRow>)>(fault)),
        };

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Opened(ResidentKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, WarmSite site, CancelScope scope, IClock clock, Seq<OrtEpDevice> devices, string artifactDir) =>
        Options(ep, policy, artifactDir, site, scope, devices)
            .Bind(load => OpenAdmitted(warm, bytes, ep, policy, site, scope, clock, load));

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> OpenAdmitted(ResidentKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, WarmSite site, CancelScope scope, IClock clock, PlannedLoad load) {
        using (load) {
            try {
                // A managed warm HIT opens BY PATH so ORT memory-maps the optimized graph; every other arm hands
                // native the source array. That array is the ONE materialization of the model on this leg, forced
                // by the constructor's own signature and paid nowhere earlier in the chain. The engine-cache arm
                // opens the SOURCE too: its EP reads its own cache directory during construction, so the model this
                // session loads is the same graph either way.
                InferenceSession session = ep.Warm.Switch(
                    state: (Site: site, Bytes: bytes, Options: load.Options),
                    epContext: static at => new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked),
                    optimizedGraph: static at => at.Site.Admissible
                        ? new InferenceSession(at.Site.Path, at.Options, PrePacked)
                        : new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked),
                    engineCache: static at => new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked));
                // Publication follows which side of the miss this open sat on: an EP-context open BINDS an existing
                // compatible blob, so it publishes the artifact it read; a managed open WRITES its graph during
                // construction, so it publishes on the miss and finds a hit already indexed by the open that wrote
                // it. An engine cache publishes NOTHING: it is a directory of EP-private plans valid only for the
                // GPU, driver, and runtime that built them, so the single-payload blob lane cannot address it and a
                // fleet that could would be shipping engines no other machine can load.
                return Fin.Succ((session, ep.Warm.Switch(
                    state: (Site: site, Key: warm, Policy: policy, At: clock.GetCurrentInstant()),
                    epContext: static held => held.Site.Admissible ? AdmitContext(held.Site, held.Key, held.Policy, held.At) : Option<ArtifactIndexRow>.None,
                    optimizedGraph: static held => held.Site.Admissible ? Option<ArtifactIndexRow>.None : AdmitContext(held.Site, held.Key, held.Policy, held.At),
                    engineCache: static _ => Option<ArtifactIndexRow>.None)));
            }
            catch (OnnxRuntimeException error) { return Fault<(InferenceSession, Option<ArtifactIndexRow>)>(scope, error); }
            catch (IOException error) { return Fault<(InferenceSession, Option<ArtifactIndexRow>)>(scope, error); }
            catch (UnauthorizedAccessException error) { return Fault<(InferenceSession, Option<ArtifactIndexRow>)>(scope, error); }
            catch (ArgumentException error) { return Fault<(InferenceSession, Option<ArtifactIndexRow>)>(scope, error); }
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

    // Load latch registers LAST and releases FIRST. Constructing an `InferenceSession` runs graph optimization
    // and provider compilation — seconds for a UNet, the folder's slowest native call — and `SetLoadCancellationFlag`
    // is the ONLY surface that aborts it, the load-time counterpart of the run-side `RunOptions.Terminate` latch;
    // without it a shutting-down host still pays every cold open the warm sweep stacked, and a deadline that bounds
    // a run but not the load it waits on bounds nothing. Registration before the fold fires against
    // half-configured options, and owning the registration on the returned capsule is what keeps a latch firing into
    // disposed options unrepresentable.
    static Fin<PlannedLoad> Options(ExecutionProvider ep, SessionPolicy policy, string artifactDir, WarmSite site, CancelScope scope, Seq<OrtEpDevice> devices) {
        SessionOptions options = new();
        try {
            options.GraphOptimizationLevel = site.Level;
            options.ExecutionMode = policy.Execution;
            options.EnableMemoryPattern = policy.MemoryPattern;
            // ORDER IS LOAD-BEARING (measured at the pin): the EnableProfiling setter READS the prefix at the
            // moment it flips true — a prefix assigned after it is silently discarded and the trace lands at the
            // default onnxruntime_profile__<timestamp>.json in the process CWD, so the prefix sets FIRST.
            options.ProfileOutputPathPrefix = Path.Combine(artifactDir, "onnx-profile");
            options.EnableProfiling = policy.Profiling;
            options.DisablePerSessionThreads();
            ep.BindWarm(options, site.Path, site.Admissible);
            policy.FreeDims.Iter(dim => options.AddFreeDimensionOverrideByName(dim.Dim, dim.Value));
            policy.Initializers.Iter(slot => options.AddInitializer(slot.Name, slot.Value));
            ep.DevicePolicy.Iter(options.SetEpSelectionPolicy);
            ep.Register(options, new ArtifactSite(artifactDir, site.Path), policy.Precision, devices);
            return CustomOps.Register(options, policy)
                .MapFail(fault => { options.Dispose(); return fault; })
                .Map(admitted => new PlannedLoad(admitted, scope.Source.Token.Register(() => admitted.SetLoadCancellationFlag(true))));
        }
        catch (OnnxRuntimeException error) { options.Dispose(); return Fault<PlannedLoad>(scope, error); }
        catch (IOException error) { options.Dispose(); return Fault<PlannedLoad>(scope, error); }
        catch (UnauthorizedAccessException error) { options.Dispose(); return Fault<PlannedLoad>(scope, error); }
        catch (ArgumentException error) { options.Dispose(); return Fault<PlannedLoad>(scope, error); }
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

    // ONE native-fault classifier for the whole model rail. A latch-aborted load and a latch-aborted run both surface
    // as an `OnnxRuntimeException` indistinguishable from a genuine rejection, so PROVENANCE decides: a fired
    // deadline is `DeadlineExpired`, any other cancellation is `Cancelled`, and everything else stays
    // `ModelRejected`. `Model/inference#INFERENCE_MODES` composes this classifier rather than re-spelling it on the
    // run side, so the load and run halves of one cancellation cannot report two different faults.
    public static Error Faulted(CancelScope scope, Exception error) =>
        scope.Source.Token.IsCancellationRequested
            ? scope.Deadline is { IsSome: true, Case: CancellationTokenSource expired } && expired.IsCancellationRequested
                ? new ComputeFault.DeadlineExpired(scope.Provenance)
                : new ComputeFault.Cancelled(scope.Provenance)
            : new ComputeFault.ModelRejected(error.Message);

    static Fin<T> Fault<T>(CancelScope scope, Exception error) => Fin.Fail<T>(Faulted(scope, error));

    // The artifact read is BRACKETED rather than fenced behind an existence probe: a `let` binding running
    // `File.ReadAllBytes` inside the query throws straight through the `Option` rail on a file deleted, truncated, or
    // permission-refused between the probe and the read, and an existence test beside it is a second spelling of one
    // law that still loses that race. Absence, unreadability, and a refused blob write are ONE answer here — MISS —
    // because a warm artifact nothing could index is exactly a cold open, never a lease refusal.
    static Option<ArtifactIndexRow> AdmitContext(WarmSite site, ResidentKey resident, SessionPolicy policy, Instant at) =>
        from store in BlobStore
        from bytes in Try.lift(() => (ReadOnlyMemory<byte>)File.ReadAllBytes(site.Path)).Run().ToOption()
        from _ in store(bytes).ToOption()
        select ArtifactIndexRow.Admit(
            ArtifactKind.EpContext,
            site.Key,
            bytes,
            policy.WarmStartClassification,
            at,
            Some(resident.Checksum));

    // Seeding lands ONLY a shape the model itself fixes. A dynamic dimension has no representative value, and
    // collapsing one to 1 mints a shape no consumer runs: the sweep then warms a memory pattern nobody reads, and a
    // graph with a downsampling stack REFUSES a unit extent outright — a fabricated seed becomes a bucket failing
    // every sweep forever. Models carrying any free dimension seed NOTHING and warm once a consumer registers the
    // bucket it will actually run; fully static models need no registration and warm from their own signature.
    // Seeded keys spell their own dims because no consumer has named them; lookup is by explicit key, so a
    // consumer's later spelling for the same shape is a distinct row rather than a collision.
    static HashMap<string, WarmBucket> Seed(ModelIdentity model) =>
        StaticShape(model).Match(
            Some: static shape => HashMap((string.Join('x', shape), new WarmBucket(string.Join('x', shape), shape, None, None, None))),
            None: static () => HashMap<string, WarmBucket>());

    static Option<long[]> StaticShape(ModelIdentity model) =>
        model.Inputs.Head.Bind(static slot =>
            slot.Shape is SlotShape.Tensor tensor && tensor.Dims.ForAll(static dim => dim > 0L)
                ? Some(tensor.Dims.ToArray())
                : None);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
