# [COMPUTE_SESSIONS]

`ModelSessions` shares one `InferenceSession` per policy-complete `ResidentKey`, one device-keyed warm artifact, and one allocator lease map. Its fleet roster, keyed on the profile-stripped identity, owns the capped registered shape buckets and their measured warm evidence; residency rides the generic `ResidentPool` seated here, every cold load binds to a cancellation latch, and every warm artifact admits through `AdmitContext`.

`SessionPolicy`, `ResidentKey`, and `ModelSessions` own boot, lease, open, allocation, warmup, eviction, drain, and compilation. ONNX Runtime supplies session and allocator surfaces; `CpuBudget`, `CancelScope`, result ports, identity fingerprints, provider policy with its `WarmForm` axis, and `BoundFlow` arrive settled.

## [01]-[INDEX]

- [02]-[SESSION_CAPSULE]: the one generic `ResidentPool` every native-resident family on this branch instantiates; one shared session per policy-complete `ResidentKey` with lifecycle, warmup, and drain rows, a profile-stripped fleet bucket roster, a shared-device-allocator lease, a deadline-bound cold load with its mapped-graph fallback, and a fleet-shared device-keyed warm artifact under one `ContextKey` derivation spanning every warm-start form.

## [02]-[SESSION_CAPSULE]

- Owner: `ResidentPool<TKey, THandle>` the ONE keyed native-resident pool — residency, lease refcount, cap and idle eviction, bracketed LIFO disposal, and native-throw classification — seated here and instantiated three times, twice on this branch's generative page; `SessionTrait` the kernel-`ICapability` construction-behavior vocabulary; `SessionPolicy` the lifecycle record with its `Posture` capability set and its `Fingerprint` fold over every construction-behavior column through the kernel `CanonicalWriter`; `ResidentKey` and `WarmKey` the two distinct identities — policy-complete and profile-stripped — that every lifecycle member takes as separate types; `PlannedLoad` the options-plus-latch load capsule; `WarmSite` the two-case cold-open decision pairing this capsule's artifact key with the provider's own `WarmVerdict` and the level it therefore takes; `ModelSessions` capsule owning the `Gate`-serialized OrtEnv boot with its token-gated `ContentBlobPort`, the pool-held `OrtResident` fleet (each carrying `ExecutionProvider`, its warm key, its warm-start `Option<ArtifactIndexRow>`, and its `SessionPlacement` evidence), the `Atom`-held fleet `WarmRoster` map keyed on that warm key, the shared-device-allocator lease map, the single form-aware `ContextKey` warm identity, the single `AdmitContext` warm-artifact owner, the single `Faulted` cancellation-and-artifact classifier, and the warmup, idle-eviction, drain, and sweep rows.
- Law: RESIDENCY IS ONE OWNER. Three fleets — this ORT session map, the generative `Config`/`Model`/`AdapterSet` map, and the conversation registry beside it — each spelled find-or-build, refcount, race-loser disposal, cap and idle eviction, and release by hand, so every fix landed on one and reached neither other. `ResidentPool` is that machinery once; a family contributes only its `THandle` payload and its cap. The collapse LOSES this capsule's blanket `Gate` over residency and GAINS three facts no copy had: eviction commits as a per-key CAS against a row still holding zero leases rather than removing a filtered snapshot an `Acquire` can slip through, disposal brackets LIFO so one throwing native `Dispose` cannot strand every handle behind it, and a conversation acquires a real hold so an idle sweep can no longer close a `Generator` mid-turn. A fourth native-resident family lands as one instantiation.
- Law: warm-up is PER BUCKET. ORT plans its memory pattern per executed shape, so one warm pulse at one representative shape leaves every other shape a cold first run, and a caller that tiles against fixed buckets runs exactly the shapes it declared. Consumers REGISTER each bucket they will run, capped by `SessionPolicy.WarmBuckets` so a per-request extent never grows an unbounded warm set, and the sweep then pulses every registered bucket on the resident's own session.
- Law: every WARM identity strips profiling and session identity does not. `ResidentKey` keeps the `Profiling` trait because the flag changes the session ORT builds, so two residents cannot alias; but the bucket roster, the partition census, and the warm artifact are properties of the OPTIMIZED GRAPH and its EP set, which profiling moves not at all. `WarmKey.Of` therefore re-derives the fingerprint over a profile-cleared posture, and the roster map, `Warm`, `Measured`, `ContextKey`, and the fleet `Compile` all key there — so the one profiling lease that reads a graph's partition census publishes evidence every production resident reads, and one compiled context serves both. Keying them to the full identity strands each measurement inside the lease that took it, leaves the production resident reading an absent count forever, and re-compiles a context the fleet already owns.
- Law: rosters OUTLIVE residents. Eviction drops the session and keeps the roster because the partition census belongs to the graph, never the session: a re-leased resident reads back the measurement the fleet already paid for, and the map bounds at the model-and-policy product the fleet opens rather than at the residency cap. `Drain` clears it whole because that is process teardown, not eviction.
- Law: each bucket carries the evidence its own pulse MEASURED — graph partition count, pulse duration, warm instant — and nothing else. `WarmEvidence` replaces `Unit` on the pulse return because the caller that ran the shape is the only surface observing how the graph partitioned for it; an unmeasured column stays `None` and a consumer needing it refuses rather than reading a zero as an observation. Buckets only ever seat a shape someone will RUN: seeding admits a fully static model signature and nothing else, and a registration re-binding a seated key to a different shape refuses instead of keeping the first shape under the second consumer's name.
- Law: COLD opens build native state outside every transition. Session construction runs graph optimization and provider compilation, so serializing it behind a residency transition stalls every lease of every other model for the duration; two threads racing one cold key both build, the pool's keyed CAS seats the first, and the loser disposes its own build and leases the seated resident — one redundant build instead of a serialized fleet, and never two live sessions forking one warm roster. A cold build whose `Placement` read refuses now ROLLS BACK the session it measured, because that handle is unpublished and nothing else can reach it.
- Law: that same cold build is CANCELLABLE. `SetLoadCancellationFlag` registered off the lease's own `CancelScope` is the load-time counterpart to the run-side `Terminate` latch, so a deadline bounds the load it precedes rather than only the run that follows it, a shutting-down host aborts the cold opens a `SweepRow` stacked instead of paying every one, and a cancelled first pass stops re-paying the full compile. `Faulted` then classifies by provenance — a fired deadline is `DeadlineExpired`, another cancellation is `Cancelled` — because a latch-aborted load raises the same `OnnxRuntimeException` as a genuine rejection, while an unclassified native failure remains the original exceptional Error.
- Law: the warm form decides what a cold open LOADS, and the managed hit loads BY PATH. `WarmForm.EpContext` leaves the source bytes and the policy's optimization level alone and reads its blob through the bound `ep.context_*` keys; `WarmForm.OptimizedGraph` opens the artifact ITSELF at `ORT_DISABLE_ALL` through the path-taking `InferenceSession(string, SessionOptions, PrePackedWeightsContainer)` constructor, so ORT memory-maps a graph that runs to hundreds of megabytes and the process pays no managed copy of it — reading that file into an array and handing the array over costs one full read AND one full copy of bytes ORT maps anyway, which is most of the cost the form exists to remove. Model bytes materialize to a `byte[]` exactly ONCE per open or compile, at the single native hand-off that demands an array. Construction failure over a mapped graph answers MISS by re-siting at the policy's own level, so a file deleted between the existence probe and the open, or truncated by a crashed writer, costs one cold optimization rather than a refused lease.
- Entry: `public static Fin<ResidentPool<ResidentKey, OrtResident>.Lease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock)` aborts on rejected admission; every lease carries the resident's own stored warm row — a hit reports the artifact its resident opened against rather than reporting none, because warm-start evidence is a property of the SESSION and a consumer that leased a warmed resident otherwise reads a cold one — and the pool lease's `Dispose` stamps release time and decrements the resident hold exactly once. `Warmth` dispatches on the provider row's own `WarmForm` — the EP-context arm reads the compiled artifact's embedded compat info while the managed and engine-cache arms read presence against a runtime-keyed artifact name — so an incompatible, stale, or absent warm artifact answers its own `WarmVerdict` case and degrades to a fresh session without one. The lease CARRIES its key beside the `OrtResident` payload holding both identities and the placement evidence its cold open measured, so `public static Fin<Unit> Warm(WarmKey warm, string bucket, long[] shape)` and `public static Option<WarmBucket> Measured(WarmKey warm, string bucket)` reach the fleet roster off the handle a consumer already holds rather than re-deriving it from model, provider, and policy. `SessionPolicy.Pack(HdfHandle, ModelIdentity)` loads the HDF5 initializer pack through `Runtime/archive#HDF_ARCHIVE` — the `/initializers` group's children ARE the roster, each child's dtype row admitting through `TensorVocabulary.Admit(IH5DataType)` and its extents through `Space.Dimensions`, the content key hashed over the staging span BEFORE `TensorBridge.Ingress`, and the minted value re-proving through `ModelIdentity.Initializer` before any session options carry it.
- Auto: `Boot` binds the Persistence `Store/blobstore#OBJECT_STORE` `ContentBlobPort` under `Gate` TOKEN-GATED — a second boot re-passing the composition root's own port value is idempotent and one passing a differently-minted port refuses typed, because every admitted context row names an artifact this port wrote and a silent rebind re-homes every later write while every earlier row keeps pointing at the first store. `CustomOpLibrary.Admit` content-keys each native ASSET before it enters `SessionPolicy` while the bundle row carries the runtime that ships it; `Admit` re-hashes the model bytes and every custom-op asset, rejects nonpositive capacity/durations, invalid or duplicate free dimensions and initializers, zero initializer content identities, duplicate custom-op identities, and every initializer that misses the model's exact tensor schema before `Lease` or `Compile` reaches native state. `Options` then folds the site's optimization level, free dimensions, initializers, execution, memory, profiling, device policy, provider registration, the row's own `BindWarm` warm-state write, custom ops, and finally the load-cancellation registration once for both open and fleet compile, returning the `PlannedLoad` capsule that owns both halves. `ResidentKey.Of(model, ep, policy)` joins checksum with the `SessionPolicy.Fingerprint` fold over every construction-behavior column, including each initializer and custom-op content key, so equal paths or names carrying different bytes cannot alias one resident or compiled context, while `WarmKey.Of` re-folds that same projection over a profile-cleared posture for every roster and artifact identity. `Model/providers#EP_AXIS` `ProviderSnapshot.Fingerprint` folds the EP, hardware, and provider metadata tables into context, allocator, and result identity alike. `Placement` runs ONCE per cold open — zipping ordered input names with `GetEpDeviceForInputs` and `GetMemoryInfosForInputs`, output names with `GetMemoryInfosForOutputs`, and rejecting any native cardinality mismatch — and rides the resident, so every lease off it reads the same evidence off the handle it already holds.
- Law: LIFECYCLE. `Lease` increments the pool row's hold inside its key-local transition; the lease's `Dispose` decrements once through `Interlocked.Exchange`; `Unload` removes only zero-lease residents older than its threshold and proves that zero AT the removal; `Drain` releases shared allocators only after the pool reports no resident remains, so a resident a disposer fault stranded keeps its arena rather than having the memory freed under it. `Warmup` acquires REAL pool holds over its snapshot — a row evicted between the snapshot and the acquire is skipped rather than pulsed — expands each resident against its own bucket roster, and releases every lease in `finally`, so a sweep cannot dispose a pulsed session; the bucket fold ACCUMULATES rather than short-circuits, so one unwarmable shape refuses alone instead of leaving every later bucket and every later resident cold on a schedule refusing at the same point each sweep; each pulse's measured evidence folds back onto its bucket row through one atom swap before the pulse returns.
- Law: WARM. `Site` derives the warm decision once — the form's own artifact key, the provider's `WarmVerdict`, and the level the session therefore takes — and `Open` then admits through the single `AdmitContext` owner under the same `ContextKey(WarmKey, device, WarmForm)` the fleet `Compile` writes, publishing on the side of the miss its form sits on: an EP-context open indexes the blob it bound, a managed open indexes the graph its construction wrote. `Compile` refuses a row whose warm form compiles no partition and forces its own site to a miss, so a fleet sweep never reads the file it is about to overwrite. `StaticShape` reads the first `SlotShape.Tensor` and answers only when every dimension is fixed, so `Seed` lands a first bucket for a model whose signature already names one shape and lands NOTHING for a model carrying a free dimension — a dynamic dim collapsed to `1` mints a shape no consumer runs and a downsampling graph refuses outright.
- Result: `Warmup` returns `Fin<Seq<Option<ArtifactIndexRow>>>`, preserving any pulse fault and one optional warm-start artifact per registered bucket. `DrainRow` returns the official `DrainParticipantPort`; its delegate drains directly and refuses when native disposal fails.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, PureHDF (`IH5Object.Name`/`Children`, `NativeDataset.Type`/`Space.Dimensions`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`, `HyperslabSelection`), Generator.Equals (`[Equatable]`, `[OrderedEquality]`, `[IgnoreEquality]`), LanguageExt.Core (`AtomHashMap.SwapKey`, `Atom.Swap`), NodaTime, Rasm (project, `Domain.ContentHash`/`CanonicalWriter`/`CapabilitySet`, `Domain.Custody.Rollback`/`Bracket`, `Parametric.MonotonicTimeline`), Rasm.AppHost (project), Rasm.Persistence (project, `ContentBlobPort`, `ArtifactIndexRow`, `ArtifactKind`), BCL inbox
- Growth: a lifecycle change is one `SessionPolicy` value; a new construction-behavior column is one `Fingerprint` field that automatically re-keys residency and — unless it is trace-only, which `WarmKey.Of` clears — the warm artifacts too; a further warm-start mechanism is one `Model/providers#EP_AXIS` `WarmForm` row whose suffix and runtime-keying columns this derivation already reads, never a second load path here; a further load-time bound is one more registration on the existing `PlannedLoad`, never a second cancellation owner; the warm-start and the fleet compile both admit through the single `AdmitContext` owner over the single `ContextKey` derivation, never a second cache, artifact owner, or filename scheme; the fleet-shared context is one `Compile` member publishing a `ContextKey(WarmKey, device, WarmForm)`-keyed `ArtifactIndexRow` through the same blob-lane owner, never a second EP-cache; a new warm strategy is the injected `pulse`, never a second warm surface, and a further shape to warm is one `Warm` registration on the existing roster; a further measured warm fact is one `WarmEvidence` column folded onto the bucket row, never a second evidence owner; a quantized session is `SessionPolicy.Precision` set to `Int8`/`Int4` OVER settled pre-quantized model bytes — the row is execution posture (`QuantizedGraph` evidence, MatMulNBits accuracy floor, accumulation), never a graph transform, and the quantized graph carries its own checksum identity — flowing through the existing `Options` pipeline with residency and context reuse re-keyed by the same `Fingerprint` fold, never a quantization-specific owner; a sequential-versus-parallel posture is the `SessionPolicy.Execution` column folded into `options.ExecutionMode`, never a second session owner; a further construction-behavior toggle is one `SessionTrait` row the posture set already carries; a further registration target — a bundle, a signed plugin — is one `CustomOpLibrary` case whose identity, probe, and fingerprint column all land with it.
- Boundary: `ModelSessions` is the `CAPSULE_OWNER`. ORT sessions are thread-safe for concurrent `Run`, so all lanes share one `InferenceSession` per `ResidentKey`; the pool lease is the only lifetime handed to a run. `Gate` now serializes exactly the two legs the residency owner cannot see: boot — including the token-gated `ContentBlobPort` bind, a static every context admission reads — and shared allocator create/release. Residency rides `ResidentPool`'s key-local transitions, warm-roster registration/observation/read-back carry no native effect and ride `Atom.Swap`, and native session and context CONSTRUCTION stays outside all three, which is the whole reason a cold open can lose a race and dispose its own build. `PlannedLoad` is transient and disposes after `InferenceSession` or `OrtModelCompilationOptions` consumes its options, releasing the load latch first so the token can never reach freed native state; `PrePackedWeightsContainer` alone spans sessions. `DisablePerSessionThreads` binds every session to the global pool `Boot` derives from `CpuBudget`. Warm artifacts — compiled `ep.context_*` blobs and managed optimized graphs alike — and profiles land WRITE-BLOB-FIRST: the Boot-bound `ContentBlobPort` persists the bytes AND mints their content address, and only durable storage publishes the `ArtifactIndexRow` naming that same address — an unbound port or a failed write publishes no row, so a dangling index cannot name unavailable content, and the row can never carry a second locally-hashed key the object plane did not agree to; retention derives from `ArtifactKind.Retention`, and each warm artifact projects under its model checksum. `ContextKey(WarmKey, device, WarmForm)` is the sole warm identity for lookup, compilation, admission, and transport, and it takes the PROFILE-STRIPPED key: it cannot alias sessions whose provider options or construction policy differ, it deliberately DOES alias a profiling session with the production one that shares its graph, the form's suffix keeps the two mechanisms' artifacts distinct inside one derivation, and the runtime version enters only for the form carrying no compat info of its own — so a managed graph another ORT wrote misses its key instead of loading under a runtime that never produced it. `Placement` closes autoEP selection and I/O memory residency with post-construction evidence, read once at the cold open and carried on every lease of that resident. Shared allocators release only after all resident leases drain; `Unload` never disposes a session under an active run.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SessionTrait : ICapability<SessionTrait> {
    public static readonly SessionTrait MemoryPattern = new("memory-pattern");
    public static readonly SessionTrait Profiling = new("profiling");
    public static CapabilityLaw<SessionTrait> Law => CapabilityLaw<SessionTrait>.Open;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WarmSite(string Key, WarmVerdict Verdict, GraphOptimizationLevel Level) {
    public sealed record Hit(string Key, WarmVerdict Verdict, GraphOptimizationLevel Level) : WarmSite(Key, Verdict, Level);

    public sealed record Miss(string Key, WarmVerdict Verdict, GraphOptimizationLevel Level) : WarmSite(Key, Verdict, Level);

    public string Path => Verdict.Path;
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record SessionPolicy(
    int ResidentSessions, Duration IdleUnload, Duration WarmupSweep,
    GraphOptimizationLevel Optimization, ExecutionMode Execution, CapabilitySet<SessionTrait> Posture,
    Seq<SessionPolicy.CustomOpLibrary> CustomOpLibraries, Seq<(string Dim, long Value)> FreeDims,
    Seq<SessionPolicy.Initializer> Initializers,
    ModelPrecision Precision,
    int WarmBuckets,
    DataClassification WarmStartClassification) {
    public static readonly SessionPolicy Canonical = new(
        ResidentSessions: 4, IdleUnload: Duration.FromMinutes(10), WarmupSweep: Duration.FromMinutes(5),
        Optimization: GraphOptimizationLevel.ORT_ENABLE_ALL, Execution: ExecutionMode.ORT_SEQUENTIAL,
        Posture: CapabilitySet<SessionTrait>.Of(SessionTrait.MemoryPattern),
        CustomOpLibraries: Seq<CustomOpLibrary>(), FreeDims: Seq<(string Dim, long Value)>(),
        Initializers: Seq<Initializer>(),
        Precision: ModelPrecision.Full,
        WarmBuckets: 8,
        WarmStartClassification: DataClassification.Operational);

    public bool Holds(SessionTrait trait) => Posture.Admits(trait);

    public Fin<Unit> Conforms() =>
        AdmissionSlots.Accumulate(Seq(
         Refusal.Unless(ResidentSessions > 0, ComputeArea.Model, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(ResidentSessions, 1L))),
         Refusal.Unless(WarmBuckets > 0, ComputeArea.Model, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(WarmBuckets, 1L))),
         Refusal.Unless(IdleUnload > Duration.Zero, ComputeArea.Model, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.DurationValue(IdleUnload))),
         Refusal.Unless(WarmupSweep > Duration.Zero, ComputeArea.Model, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.DurationValue(WarmupSweep))),
         Refusal.Unless(FreeDims.ForAll(static dim => dim.Dim.Length > 0 && dim.Value > 0), ComputeArea.Model,
             new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())),
         Refusal.Unless(FreeDims.Map(static dim => dim.Dim).ToFrozenSet(StringComparer.Ordinal).Count == FreeDims.Count, ComputeArea.Model,
             new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(
                 FreeDims.Map(static dim => dim.Dim).ToFrozenSet(StringComparer.Ordinal).Count, FreeDims.Count))),
         Refusal.Unless(Initializers.ForAll(static slot => slot.Name.Length > 0 && slot.ContentKey != UInt128.Zero), ComputeArea.Model,
             new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.None())),
         Refusal.Unless(Initializers.Map(static slot => slot.Name).ToFrozenSet(StringComparer.Ordinal).Count == Initializers.Count, ComputeArea.Model,
             new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(
                 Initializers.Map(static slot => slot.Name).ToFrozenSet(StringComparer.Ordinal).Count, Initializers.Count))),
         Refusal.Unless(CustomOpLibraries.Map(static library => library.Identity).ToFrozenSet(StringComparer.Ordinal).Count == CustomOpLibraries.Count, ComputeArea.Model,
             new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(
                 CustomOpLibraries.Map(static library => library.Identity).ToFrozenSet(StringComparer.Ordinal).Count, CustomOpLibraries.Count))),
         SessionTrait.Law.Admit(Posture).ToValidation().Map(static _ => unit)))
        .ToFin();

    public ulong Fingerprint(ExecutionProvider ep) => ContentHash.Halves(
        ContentHash.Of((Policy: this, Ep: ep), static (state, writer) => writer
            .String(state.Ep.Key)
            .I64(unchecked((long)RosterFingerprint.Of(state.Ep.OptionsFor(state.Policy.Precision))))
            .String(state.Policy.Precision.Key)
            .Ordinal((int)state.Policy.Optimization)
            .Ordinal((int)state.Policy.Execution)
            .String(state.Policy.Posture.Wire)
            .Sorted(state.Policy.CustomOpLibraries, static library => library.Identity, StringComparer.Ordinal,
                static (library, rows) => rows.String(library.Identity))
            .Sorted(state.Policy.FreeDims, static dim => dim.Dim, StringComparer.Ordinal,
                static (dim, rows) => rows.String(dim.Dim).I64(dim.Value))
            .Sorted(state.Policy.Initializers, static slot => slot.Name, StringComparer.Ordinal,
                static (slot, rows) => rows.String(slot.Name).U128(slot.ContentKey)))).Low;

    public sealed record Initializer(string Name, OrtValue Value, UInt128 ContentKey);

    static readonly FrozenDictionary<TensorElementType, Func<HdfHandle, NativeDataset, long[], Fin<(OrtValue Value, UInt128 Key)>>> PackDtypes =
        new Dictionary<TensorElementType, Func<HdfHandle, NativeDataset, long[], Fin<(OrtValue Value, UInt128 Key)>>> {
            [TensorElementType.Float] = Staged<float>,
            [TensorElementType.Double] = Staged<double>,
            [TensorElementType.Int32] = Staged<int>,
            [TensorElementType.Int64] = Staged<long>,
        }.ToFrozenDictionary();

    public static Fin<Seq<Initializer>> Pack(HdfHandle archive, ModelIdentity model) =>
        guard(archive.Exists("initializers"), (Error)new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.None())))
            .ToFin()
            .Bind(_ => Op.Of(name: "model.initializer-pack-roster").Catch(() => Fin.Succ(toSeq(archive.Group("initializers").Children()).Map(static child => child.Name))))
            .Bind(names => names.Traverse(name => Packed(archive, model, name).ToValidation()).As().ToFin());

    static Fin<Initializer> Packed(HdfHandle archive, ModelIdentity model, string name) =>
        Op.Of(name: "model.initializer-pack").Catch(() => {
                NativeDataset dataset = archive.Dataset($"initializers/{name}");
                long[] shape = [.. dataset.Space.Dimensions.Select(static dim => checked((long)dim))];
                return TensorVocabulary.Admit(dataset.Type).Bind(row =>
                    (PackDtypes.TryGetValue(row.Element, out Func<HdfHandle, NativeDataset, long[], Fin<(OrtValue Value, UInt128 Key)>>? stage)
                        ? stage(archive, dataset, shape)
                        : Fin.Fail<(OrtValue Value, UInt128 Key)>(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Key(row.Key)))))
                    .Bind(staged => model.Initializer(name, staged.Value)
                        .Map(gated => new Initializer(gated.Name, gated.Value, staged.Key))));
            });

    static Fin<(OrtValue Value, UInt128 Key)> Staged<T>(HdfHandle archive, NativeDataset dataset, long[] shape) where T : unmanaged {
        long count = shape.Aggregate(1L, static (acc, dim) => acc * dim);
        T[] staging = new T[checked((int)count)];
        ulong[] dims = [.. shape.Select(static dim => (ulong)dim)];
        dataset.Read<T>(archive.Access, staging.AsSpan(), new HyperslabSelection(shape.Length, new ulong[shape.Length], dims));
        UInt128 key = ContentHash.Of(MemoryMarshal.AsBytes<T>(staging.AsSpan()));
        return TensorBridge.Ingress(staging, shape).Map(value => (Value: value, Key: key));
    }

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record CustomOpLibrary {
        private CustomOpLibrary() { }

        public sealed record Bundled : CustomOpLibrary;

        public sealed record Asset : CustomOpLibrary {
            internal Asset(string path, UInt128 contentKey) => (Path, ContentKey) = (path, contentKey);

            public string Path { get; }
            public UInt128 ContentKey { get; }
        }

        public static Fin<CustomOpLibrary> Admit(string path) =>
            Op.Of(name: "model.custom-op-asset-admit").Catch(() => File.Exists(path)
                    ? Fin.Succ<CustomOpLibrary>(new Asset(path, ContentHash.Of(File.ReadAllBytes(path))))
                    : Fin.Fail<CustomOpLibrary>(new ComputeFault.ExtensionAssetMissing(path)));

        public Fin<Unit> Verify() => Switch(
            bundled: static _ => Fin.Succ(unit),
            asset: static library => Op.Of(name: "model.custom-op-asset-verify").Catch(() => File.Exists(library.Path) && ContentHash.Of(File.ReadAllBytes(library.Path)) == library.ContentKey
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new ComputeFault.ExtensionAssetMissing(library.Path))));

        public string Identity => Switch(
            bundled: static _ => $"bundle:{OrtEnv.Instance().GetVersionString()}",
            asset: static library => $"asset:{library.Path}={library.ContentKey:x32}");
    }
}

public readonly record struct ResidentKey(UInt128 Checksum, ulong Options) {
    public static ResidentKey Of(ModelIdentity model, ExecutionProvider ep, SessionPolicy policy) =>
        new(model.Checksum, policy.Fingerprint(ep));
}

public readonly record struct WarmKey(UInt128 Checksum, ulong Options) {
    public static WarmKey Of(ModelIdentity model, ExecutionProvider ep, SessionPolicy policy) {
        SessionPolicy stripped = policy with { Posture = policy.Posture.Without(SessionTrait.Profiling) };
        return new(model.Checksum, stripped.Fingerprint(ep));
    }
}

public sealed record PlannedLoad(SessionOptions Options, CancellationTokenRegistration Latch) : IDisposable {
    public void Dispose() {
        Latch.Dispose();
        Options.Dispose();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class ResidentPool<TKey, THandle>
    where TKey : notnull
    where THandle : IDisposable {
    public readonly record struct Row(THandle Held, Instant LastUsed, int Leases);

    public sealed class Lease : IDisposable {
        readonly ResidentPool<TKey, THandle> pool;
        readonly IClock clock;
        int released;

        internal Lease(ResidentPool<TKey, THandle> pool, TKey key, THandle held, IClock clock) {
            this.pool = pool;
            this.clock = clock;
            Key = key;
            Held = held;
        }

        public TKey Key { get; }

        public THandle Held { get; }

        public void Dispose() {
            if (Interlocked.Exchange(ref released, 1) is 0) { pool.Release(Key, clock.GetCurrentInstant()); }
        }
    }

    readonly AtomHashMap<TKey, Row> residents = AtomHashMap<TKey, Row>();

    public int Count => residents.Count;

    public Seq<(TKey Key, THandle Held)> Seated() =>
        residents.AsIterable().Map(static pair => (Key: pair.Key, Held: pair.Value.Held));

    public Fin<Lease> Hold(TKey key, Option<int> cap, Func<Fin<THandle>> build, IClock clock, CancelScope scope) =>
        Acquire(key, clock).Match(
            Some: Fin.Succ,
            None: () => Op.Of(name: "model.resident-build").Catch(build, scope.Source.Token)
                .MapFail(error => ModelSessions.Faulted(scope, error))
                .Bind(built => Publish(key, cap, built, clock)));

    public Option<Lease> Acquire(TKey key, IClock clock) {
        Instant now = clock.GetCurrentInstant();
        Option<THandle> taken = None;
        residents.SwapKey(key, seat => {
            taken = seat.Map(static row => row.Held);
            return seat.Map(row => row with { LastUsed = now, Leases = row.Leases + 1 });
        });
        return taken.Map(held => new Lease(this, key, held, clock));
    }

    Fin<Lease> Publish(TKey key, Option<int> cap, THandle built, IClock clock) {
        Instant now = clock.GetCurrentInstant();
        Option<THandle> seated = None;
        residents.SwapKey(key, seat => {
            seated = seat.Map(static row => row.Held);
            return seat.Match(
                Some: row => Some(row with { LastUsed = now, Leases = row.Leases + 1 }),
                None: () => Some(new Row(built, now, Leases: 1)));
        });
        return seated.Match(
            Some: raced => Custody.Bracket(() => Fin.Succ(new Lease(this, key, raced, clock)), built),
            None: () => Capped(cap).Map(_ => new Lease(this, key, built, clock)));
    }

    Fin<Unit> Capped(Option<int> cap) =>
        cap.Match(
            Some: bound => Released(Evict(Math.Max(residents.Count - bound, 0), static _ => true)),
            None: () => Fin.Succ(unit));

    Seq<(TKey Key, THandle Held)> Evict(int take, Func<Row, bool> admits) =>
        toSeq(residents.AsIterable()
                .Filter(pair => pair.Value.Leases is 0 && admits(pair.Value))
                .OrderBy(static pair => pair.Value.LastUsed)
                .Take(take))
            .Choose(pair => Taken(pair.Key, admits).Map(held => (Key: pair.Key, Held: held)));

    Option<THandle> Taken(TKey key, Func<Row, bool> admits) {
        Option<THandle> taken = None;
        residents.SwapKey(key, seat => {
            taken = seat.Filter(row => row.Leases is 0 && admits(row)).Map(static row => row.Held);
            return taken.IsSome ? Option<Row>.None : seat;
        });
        return taken;
    }

    Fin<Unit> Released(Seq<(TKey Key, THandle Held)> evicted) =>
        Custody.Bracket(static () => Fin.Succ(unit), [.. evicted.Map(static row => (IDisposable)row.Held)]);

    public Fin<Seq<TKey>> Unload(Instant idleBefore) {
        Seq<(TKey Key, THandle Held)> evicted = Evict(int.MaxValue, row => row.LastUsed < idleBefore);
        return Released(evicted).Map(_ => evicted.Map(static row => row.Key));
    }

    public Fin<int> Drain() => Unload(Instant.MaxValue).Map(static keys => keys.Count);

    void Release(TKey key, Instant at) =>
        residents.SwapKey(key, seat => seat.Map(row => row with { LastUsed = at, Leases = Math.Max(row.Leases - 1, 0) }));
}

public static class ModelSessions {
    public sealed record SessionPlacement(
        Seq<(string Name, Option<string> Provider, string Memory)> Inputs,
        Seq<(string Name, string Memory)> Outputs);

    public sealed record OrtResident(
        InferenceSession Session, ExecutionProvider Ep, WarmKey Warm,
        Option<ArtifactIndexRow> WarmStart, SessionPlacement Placement) : IDisposable {
        public void Dispose() => Session.Dispose();
    }

    public readonly record struct WarmEvidence(Option<int> Partitions);

    [Equatable]
    public readonly partial record struct WarmBucket(
        string Key, [property: OrderedEquality] long[] Shape,
        [property: IgnoreEquality] Option<int> Partitions,
        [property: IgnoreEquality] Option<Duration> Elapsed,
        [property: IgnoreEquality] Option<Instant> WarmedAt);

    sealed record WarmRoster(HashMap<string, WarmBucket> Buckets, int Cap);

    sealed record DeviceArena(OrtEpDevice Device, OrtDeviceMemoryType Memory, OrtAllocator Allocator);

    static readonly ResidentPool<ResidentKey, OrtResident> Fleet = new();
    static readonly Atom<HashMap<WarmKey, WarmRoster>> WarmRosters = Atom(HashMap<WarmKey, WarmRoster>());
    static HashMap<(ulong Device, OrtDeviceMemoryType Memory), DeviceArena> SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
    static readonly PrePackedWeightsContainer PrePacked = new();
    static readonly Lock Gate = new();

    static Option<ContentBlobPort> Blobs = Option<ContentBlobPort>.None;

    public static Fin<Unit> Boot(string logId, OrtLoggingLevel severity, CpuBudget budget, ContentBlobPort blobs) {
        lock (Gate) {
            if (Blobs.Case is ContentBlobPort seated && !seated.Equals(blobs)) {
                return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.None())));
            }
            Blobs = Some(blobs);
            if (OrtEnv.IsCreated) { return Fin.Succ(unit); }
            using OrtThreadingOptions pool = new() { GlobalIntraOpNumThreads = budget.OrtIntraOp, GlobalInterOpNumThreads = budget.OrtInterOp, GlobalSpinControl = budget.SpinControl };
            EnvironmentCreationOptions creation = new() { logId = logId, logLevel = severity, threadOptions = pool };
            OrtEnv.CreateInstanceWithOptions(ref creation);
            OrtEnv.Instance().DisableTelemetryEvents();
            return Fin.Succ(unit);
        }
    }

    public static Fin<ResidentPool<ResidentKey, OrtResident>.Lease> Lease(ModelIdentity model, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock) {
        ResidentKey key = ResidentKey.Of(model, ep, policy);
        WarmKey warm = WarmKey.Of(model, ep, policy);
        return Admit(model, bytes, policy)
            .Map(_ => WarmRosters.Swap(rosters => rosters.Find(warm).IsSome
                ? rosters
                : rosters.Add(warm, new WarmRoster(Seed(model), policy.WarmBuckets))))
            .Bind(_ => Fleet.Hold(
                key,
                Some(policy.ResidentSessions),
                () => Open(warm, bytes, ep, policy, artifactDir, scope, clock, ep.AutoSelect)
                    .Bind(opened => Placement(opened.Session)
                        .Map(placement => new OrtResident(opened.Session, ep, warm, opened.WarmStart, placement))
                        .Rollback(opened.Session)),
                clock,
                scope));
    }

    public static OrtAllocator SharedAllocator(OrtEpDevice device, OrtDeviceMemoryType memory) {
        (ulong Device, OrtDeviceMemoryType Memory) key = (ProviderSnapshot.Fingerprint(device), memory);
        lock (Gate) {
            if (SharedAllocators.Find(key).Case is DeviceArena raced) { return raced.Allocator; }
            DeviceArena arena = new(device, memory, OrtEnv.Instance().CreateSharedAllocator(device, memory, OrtAllocatorType.ArenaAllocator, FrozenDictionary<string, string>.Empty));
            SharedAllocators = SharedAllocators.Add(key, arena);
            return arena.Allocator;
        }
    }

    static Fin<SessionPlacement> Placement(InferenceSession session) {
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
            : Fin.Fail<SessionPlacement>(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.None())));
    }

    public static Fin<Unit> Warm(WarmKey warm, string bucket, long[] shape) {
        Fin<Unit> verdict = Fin.Succ(unit);
        WarmRosters.Swap(rosters => Seated(rosters, warm, bucket, shape).Match(
            Succ: next => { verdict = Fin.Succ(unit); return next; },
            Fail: fault => { verdict = Fin.Fail<Unit>(fault); return rosters; }));
        return verdict;
    }

    static Fin<HashMap<WarmKey, WarmRoster>> Seated(HashMap<WarmKey, WarmRoster> rosters, WarmKey warm, string bucket, long[] shape) =>
        rosters.Find(warm).ToFin(new ComputeFault.WarmBucketRefused($"<roster-absent:{warm.Checksum:x32}>"))
            .Bind(roster => guard(
                    bucket.Length > 0 && shape.Length > 0 && shape.All(static dim => dim > 0L),
                    (Error)new ComputeFault.WarmBucketRefused($"<shape:{bucket}>")).ToFin()
                .Bind(_ => roster.Buckets.Find(bucket).Case is WarmBucket seated
                    ? guard(
                        WarmBucket.EqualityComparer.Default.Equals(seated, new WarmBucket(bucket, shape, None, None, None)),
                        (Error)new ComputeFault.WarmBucketRefused($"<conflict:{bucket}>")).ToFin().Map(_ => rosters)
                    : guard(
                        roster.Buckets.Count < roster.Cap,
                        (Error)new ComputeFault.WarmBucketRefused($"<cap:{roster.Buckets.Count}:{roster.Cap}>")).ToFin()
                        .Map(_ => rosters.SetItem(warm, roster with {
                            Buckets = roster.Buckets.Add(bucket, new WarmBucket(bucket, shape, None, None, None)),
                        }))));

    public static Option<WarmBucket> Measured(WarmKey warm, string bucket) =>
        WarmRosters.Value.Find(warm).Bind(roster => roster.Buckets.Find(bucket));

    public static Fin<Seq<Option<ArtifactIndexRow>>> Warmup(
        Func<InferenceSession, long[], Fin<WarmEvidence>> pulse,
        IClock clock, MonotonicTimeline timeline) {
        HashMap<WarmKey, WarmRoster> rosters = WarmRosters.Value;
        Seq<(ResidentPool<ResidentKey, OrtResident>.Lease Lease, Seq<WarmBucket> Buckets)> held = Fleet.Seated()
            .Choose(row => Fleet.Acquire(row.Key, clock))
            .Map(lease => (
                Lease: lease,
                Buckets: rosters.Find(lease.Held.Warm)
                    .Map(static roster => roster.Buckets.Values.ToSeq())
                    .IfNone(Seq<WarmBucket>())));
        try {
            return held
                .Bind(row => row.Buckets.Map(bucket => (row.Lease, Bucket: bucket)))
                .Traverse(row =>
                    (from mark in timeline.Capture()
                     from evidence in pulse(row.Lease.Held.Session, row.Bucket.Shape)
                     from settled in timeline.Capture()
                     from span in timeline.Elapsed(mark, settled)
                     let elapsed = Duration.FromTimeSpan(span)
                     let at = clock.GetCurrentInstant()
                     let observed = Observe(row.Lease.Held.Warm, row.Bucket.Key, evidence, elapsed, at)
                     select row.Lease.Held.WarmStart).ToValidation())
                .As().ToFin();
        }
        finally { held.Iter(static row => row.Lease.Dispose()); }
    }

    static Unit Observe(WarmKey warm, string bucket, WarmEvidence evidence, Duration elapsed, Instant at) {
        WarmRosters.Swap(rosters =>
            rosters.Find(warm).Case is WarmRoster roster && roster.Buckets.Find(bucket).Case is WarmBucket row
                ? rosters.SetItem(warm, roster with {
                    Buckets = roster.Buckets.SetItem(bucket, row with {
                        Partitions = evidence.Partitions | row.Partitions,
                        Elapsed = Some(elapsed),
                        WarmedAt = Some(at),
                    }),
                })
                : rosters);
        return unit;
    }

    public static Fin<Seq<ResidentKey>> Unload(Instant idleBefore) => Fleet.Unload(idleBefore);

    public static Fin<int> Drain() =>
        Fleet.Drain().Map(drained => {
            lock (Gate) {
                if (Fleet.Count is not 0) { return drained; }
                Seq<DeviceArena> arenas = toSeq(SharedAllocators.Values);
                SharedAllocators = HashMap<(ulong, OrtDeviceMemoryType), DeviceArena>();
                arenas.Iter(static arena => OrtEnv.Instance().ReleaseSharedAllocator(arena.Device, arena.Memory));
            }
            WarmRosters.Swap(static _ => HashMap<WarmKey, WarmRoster>());
            return drained;
        });

    public static DrainParticipantPort DrainRow() =>
        new("compute-model-sessions", DrainBand.Compute, Rank: 10, _ =>
            IO.lift(Drain).Map(static _ => unit));

    public static ScheduleEntry SweepRow(SessionPolicy policy, IClock clock, Func<IO<Unit>> warm) =>
        new("compute-model-warmup", new OccurrenceSpec.Every(policy.WarmupSweep), DeadlineClass.Startup, Option<LeasePolicy>.None,
            RedrivePolicy.None,
            () => IO.lift(() => Unload(clock.GetCurrentInstant() - policy.IdleUnload)).Bind(_ => warm()));

    public static string ContextKey(WarmKey resident, Option<OrtEpDevice> device, WarmForm form) =>
        $"{resident.Checksum:x32}:{resident.Options:x16}"
        + device.Match(Some: static held => $":{ProviderSnapshot.Fingerprint(held):x16}", None: static () => string.Empty)
        + (form.RuntimeKeyed
            ? $":{RosterFingerprint.Of(Seq(new KeyValuePair<string, string>("ort", OrtEnv.Instance().GetVersionString()))):x16}"
            : string.Empty)
        + $".{form.Suffix}";

    public static Fin<ArtifactIndexRow> Compile(ReadOnlyMemory<byte> bytes, OrtEpDevice device, ModelIdentity model, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, Instant at) {
        WarmKey resident = WarmKey.Of(model, ep, policy);
        string artifactKey = ContextKey(resident, Some(device), ep.Warm);
        WarmSite site = new WarmSite.Miss(
            artifactKey, new WarmVerdict.Absent(Path.Combine(artifactDir, artifactKey)), policy.Optimization);
        return Admit(model, bytes, policy)
            .Bind(_ => guard(
                ReferenceEquals(ep.Warm, WarmForm.EpContext),
                (Error)new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Keys(ep.Key, ep.Warm.Key))).ToFin())
            .Bind(_ => Options(ep, policy, artifactDir, site, scope, Seq(device)))
            .Bind(load => CompileAdmitted(resident, bytes, policy, site, scope, at, load));
    }

    static Fin<ArtifactIndexRow> CompileAdmitted(WarmKey resident, ReadOnlyMemory<byte> bytes, SessionPolicy policy, WarmSite site, CancelScope scope, Instant at, PlannedLoad load) =>
        Op.Of(name: "model.compile").Catch(() => {
            using (load) {
                using OrtModelCompilationOptions compile = new(load.Options);
                compile.SetInputModelFromBuffer(bytes.ToArray());
                compile.SetOutputModelPath(site.Path);
                compile.SetEpContextEmbedMode(true);
                compile.SetGraphOptimizationLevel(policy.Optimization);
                compile.SetFlags(OrtCompileApiFlags.ERROR_IF_NO_NODES_COMPILED);
                compile.CompileModel();
                return AdmitContext(site, resident, policy, at)
                    .ToFin(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Required(ComputeSubject.Input)));
            }
        }, scope.Source.Token).MapFail(error => Faulted(scope, error));

    static Fin<WarmSite> Site(WarmKey key, ExecutionProvider ep, SessionPolicy policy, string artifactDir, Seq<OrtEpDevice> devices) {
        string artifactKey = ContextKey(key, devices.Head, ep.Warm);
        string location = Path.Combine(artifactDir, artifactKey);
        return ep.Warmth(location, devices).Map(verdict => (verdict.Bound, Mapped: ReferenceEquals(ep.Warm, WarmForm.OptimizedGraph)) switch {
            (true, true) => (WarmSite)new WarmSite.Hit(artifactKey, verdict, GraphOptimizationLevel.ORT_DISABLE_ALL),
            (true, false) => new WarmSite.Hit(artifactKey, verdict, policy.Optimization),
            _ => new WarmSite.Miss(artifactKey, verdict, policy.Optimization),
        });
    }

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Open(WarmKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, string artifactDir, CancelScope scope, IClock clock, Seq<OrtEpDevice> devices) =>
        Site(warm, ep, policy, artifactDir, devices).Bind(site =>
            Opened(warm, bytes, ep, policy, site, scope, clock, devices, artifactDir)
                .BindFail(fault => site is WarmSite.Hit && ReferenceEquals(ep.Warm, WarmForm.OptimizedGraph)
                    && fault is not ComputeFault.DeadlineExpired and not ComputeFault.Cancelled
                    ? Opened(
                        warm, bytes, ep, policy,
                        new WarmSite.Miss(site.Key, new WarmVerdict.Absent(site.Path), policy.Optimization),
                        scope, clock, devices, artifactDir)
                    : Fin.Fail<(InferenceSession, Option<ArtifactIndexRow>)>(fault)));

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> Opened(WarmKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, WarmSite site, CancelScope scope, IClock clock, Seq<OrtEpDevice> devices, string artifactDir) =>
        Options(ep, policy, artifactDir, site, scope, devices)
            .Bind(load => OpenAdmitted(warm, bytes, ep, policy, site, scope, clock, load));

    static Fin<(InferenceSession Session, Option<ArtifactIndexRow> WarmStart)> OpenAdmitted(WarmKey warm, ReadOnlyMemory<byte> bytes, ExecutionProvider ep, SessionPolicy policy, WarmSite site, CancelScope scope, IClock clock, PlannedLoad load) =>
        Op.Of(name: "model.open").Catch(() => {
            using (load) {
                return ep.Warm.Switch(
                    state: (Site: site, Bytes: bytes, Options: load.Options, Key: warm, Policy: policy, At: clock.GetCurrentInstant()),
                    epContext: static at => (
                        new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked),
                        at.Site is WarmSite.Hit ? AdmitContext(at.Site, at.Key, at.Policy, at.At) : Option<ArtifactIndexRow>.None),
                    optimizedGraph: static at => at.Site is WarmSite.Hit
                        ? (new InferenceSession(at.Site.Path, at.Options, PrePacked), Option<ArtifactIndexRow>.None)
                        : (new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked), AdmitContext(at.Site, at.Key, at.Policy, at.At)),
                    engineCache: static at => (
                        new InferenceSession(at.Bytes.ToArray(), at.Options, PrePacked), Option<ArtifactIndexRow>.None));
            }
        }, scope.Source.Token).MapFail(error => Faulted(scope, error));

    static Fin<Unit> Admit(ModelIdentity model, ReadOnlyMemory<byte> bytes, SessionPolicy policy) =>
        guard(
            ContentHash.Of(bytes.Span) == model.Checksum,
            new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Digest(model.Checksum))))
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

    static Fin<PlannedLoad> Options(ExecutionProvider ep, SessionPolicy policy, string artifactDir, WarmSite site, CancelScope scope, Seq<OrtEpDevice> devices) {
        SessionOptions options = new();
        return Op.Of(name: "model.options").Catch(() => {
                options.GraphOptimizationLevel = site.Level;
                options.ExecutionMode = policy.Execution;
                options.EnableMemoryPattern = policy.Holds(SessionTrait.MemoryPattern);
                options.ProfileOutputPathPrefix = Path.Combine(artifactDir, "onnx-profile");
                options.EnableProfiling = policy.Holds(SessionTrait.Profiling);
                options.DisablePerSessionThreads();
                ep.BindWarm(options, site.Verdict);
                policy.FreeDims.Iter(dim => options.AddFreeDimensionOverrideByName(dim.Dim, dim.Value));
                policy.Initializers.Iter(slot => options.AddInitializer(slot.Name, slot.Value));
                ep.DevicePolicy.Iter(options.SetEpSelectionPolicy);
                return Fin.Succ(options);
            }, scope.Source.Token)
            .Bind(built => ep.Register(built, new ArtifactSite(artifactDir, site.Path), policy.Precision, devices))
            .Bind(registered => CustomOps.Register(registered, policy))
            .MapFail(error => Faulted(scope, error))
            .Map(admitted => new PlannedLoad(admitted, scope.Source.Token.Register(() => admitted.SetLoadCancellationFlag(true))))
            .Rollback(options);
    }

    public static Error Faulted(CancelScope scope, Error error) =>
        scope.Source.Token.IsCancellationRequested
            ? scope.Deadline is { IsSome: true, Case: CancellationTokenSource expired } && expired.IsCancellationRequested
                ? new ComputeFault.DeadlineExpired(scope.Provenance)
                : new ComputeFault.Cancelled(scope.Provenance)
            : error.IsType<IOException>() || error.IsType<UnauthorizedAccessException>()
                ? new ComputeFault.ArtifactUnreadable(error)
                : error;

    static Option<ArtifactIndexRow> AdmitContext(WarmSite site, WarmKey resident, SessionPolicy policy, Instant at) =>
        from port in Blobs
        from bytes in Op.Of(name: "model.warm-artifact-read").Catch(() => Fin.Succ((ReadOnlyMemory<byte>)File.ReadAllBytes(site.Path))).ToOption()
        from address in port.Put(bytes).Try().Run().ToOption()
        select new ArtifactIndexRow(
            ArtifactKind.EpContext,
            site.Key,
            address,
            bytes.Length,
            policy.WarmStartClassification,
            Some(resident.Checksum),
            at);

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

// --- [ERRORS] --------------------------------------------------------------------------

public abstract partial record ComputeFault {
    [FaultCase(25)] public sealed partial record ArtifactUnreadable(Error Cause) : ComputeFault(Cause.Message), ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }

    [FaultCase(26)] public sealed partial record WarmBucketRefused(string Detail) : ComputeFault(Detail);
}
```
