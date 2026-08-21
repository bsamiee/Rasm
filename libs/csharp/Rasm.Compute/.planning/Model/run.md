# [COMPUTE_RUN_MODES]

`RunOps` folds every admitted `OrtValue` run over one shared session through a single rail-shaped native bracket: `RunInput` admits one operand polymorphically on carrier shape, `PlannedRun` pairs the `RunOptions` with its `Terminate` latch, and every mode — pooled embedding, softmax or `ZipMap` classification, scalar clash score, tensor-carrier bridge, lazily streamed chunk windows, and the trace-reading warm pulse — projects inside that bracket. `BatchGate` coalesces compatible rows into one packed bound run under a declared lane bound, and `CacheOps` projects deterministic results onto `ComputeReceipt.Cache` under a version-and-device-stamped key.

`BoundFlow` allocates through the shared arena and reports per-bucket warm evidence; `ExecutionProvider` resolves the loaded provider and owns the result key; `ModelSessions.Faulted` is the one native-fault classifier every leg funnels through; `CustomOps.Egress` reads the non-tensor model boundary a `ZipMap` head produces; Persistence owns result custody and the artifact index. The `Model/tiling#TILE_PLAN` mosaic and the `Model/stage#STAGE_FOLD` executor compose this owner's bracket rather than re-declaring one.

## [01]-[INDEX]

- [02]-[RUN_MODES]: every `OrtValue`-only run mode folded over the shared session, one polymorphic input admission feeding the vectorized reductions, the rail-shaped native bracket whose acquire leg carries no classifier of its own, one classification entrypoint spanning the numeric-logit and `ZipMap` modalities, one lazily streamed chunk source over both window providers, and the trace-reading warm pulse that measures a graph's partition census.
- [03]-[BATCH_GATE]: the cross-request coalescer over one shared session — a declared parked lane bound, a schedule-expressed fill window, a CAS-batched window cell, a forked pump under effect custody, and measured park evidence per admitted row.
- [04]-[RESULT_CACHE]: version-and-device-stamped deterministic keys and one set-membership policy column — behaviour and per-call suppression alike — over an echo-validated single-flight read-through on the `CacheRuntime` surface, with model-scoped drift invalidation keyed on severity.
- [05]-[RESEARCH]: open questions.

## [02]-[RUN_MODES]

- Owner: `RunOps` folds every run mode over the shared session; `RunInput` admits one operand polymorphically on carrier shape through the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; `PlannedRun` owns the `RunOptions` + `Terminate`-latch registration pair; `FlowPayload` carries the one write-admissible shape family a `BoundFlow` pulse takes; `ChunkSource` carries the two window providers under one streaming entry; `ClassHead` carries the two classification modalities under one entry; `RunRefusal` names this owner's shared contract refusals without a string-key roster.
- Cases: `RunInput` cases `Managed<T>`, `Carrier<T>`, `Strings`, `Sparse`; `FlowPayload` cases `Floats`, `Bytes`; `ChunkSource` cases `Sequence` (a `RecyclableMemoryStream.GetReadOnlySequence` view) and `Filler` (an index-keyed `WindowFill` span filler — an HDF5 response corpus fills each window with one hyperslab read, and the delegate is the seam that keeps PureHDF off every Compute signature); `ClassHead` cases `Logits` (softmax top-`k` over a numeric logit tensor) and `Sequenced` (the `ZipMap` sequence-of-maps head read through `Model/extension#EXTENSION_OPS`); run modes `Infer`, `InferBound`, `Chunked`, `Embed`, `InferTensor`, `Classify`, `ClashScore`, `WarmPulse`, plus `BatchGate.Submit`, the `Model/tiling#TILE_FOLD` mosaic, and the `Model/stage#STAGE_FOLD` executor above them. Native async `RunAsync` is the rejected spelling — it demands pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so the lane seam owns the thread hop.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket, and the bracket CONSUMES the admitted inputs: one run is one deterministic release for both native legs, so a repeated same-shape loop rides `BoundFlow`, never re-admitted one-shot inputs.
- Auto: `Plan` folds the `RunConfig` row table into `AddRunConfigEntry` and returns a `PlannedRun` capsule pairing the `RunOptions` with the `Terminate`-latch `CancellationTokenRegistration` off the linked `CancelScope` — the registration disposes with the capsule, so a latch firing into a disposed `RunOptions` is structurally impossible and a bare `Token.Register` whose registration nobody owns is the named use-after-free. `Bind` traverses the operand span monadically and rides `Custody.Rollback` on the failure arm, so the first refusal releases every already-admitted value and leaves with its own identity rather than a re-mapped one. Shape reads bind the tensor's own `GetTensorTypeAndShape()` columns — `Shape` for an axis, `ElementCount` for a total — never re-multiplied dimensions. `RunInput` composes the `TensorBridge.Ingress` overloads (the sole dense `OrtValue` C-data factory owner) over the open unmanaged `T`; ONNX-owned strings and preconstructed sparse `OrtValue`s ride distinct cases, with sparse ownership transferring only after `IsSparseTensor` proves the carrier. `Pooling` carries each reduction as its delegate-backed row over a `Span2D<float>` token plane, and `Embed` admits only an unbatched hidden-state tensor, so a multi-row output never collapses unrelated embeddings into one vector. `Classify` ranks each row through the kernel `Ranked` bounded-selection cell at `O(width·log k)` offers — never a full-taxonomy sort, and never a folder-local heap — and materializes per row through an explicit span walk, no `ReadOnlySpan<float>` captured into a lambda, the named kernel exemption.
- Law: `Op.Catch` admits each native leg and `ModelSessions.Faulted` classifies the documented model cases; unknown errors retain their original identity.
- Law: the `Terminate` latch lands at ORT node boundaries, so the cancellation grain is the largest SCHEDULED unit — measured at the pin: a 224-node CPU graph at ~2.9 s wall answers a mid-run latch in 7-10 ms (`OnnxRuntimeException [ErrorCode:Fail] Exiting due to terminate flag being set to true`), while the SAME graph fused into one CoreML MLProgram partition runs to completion through a latch set at 60% of its ~420 ms wall, because a fused partition exposes no interior boundary the latch can land on. Deadline enforcement on an accelerator row therefore budgets the largest fused partition's wall time — the `partitionCount` assertion is what keeps that budget bounded — and a deadline poll finer than node latency buys nothing on the floor row.
- Law: classification is ONE entrypoint over two MODALITIES. A numeric logit head needs a softmax this page owns; a `ZipMap` head arrives already normalized as a sequence of label→score maps the numeric tensor egress cannot carry at all. Both answer ranked `(class, probability)` rows, so the class key is `OpOutput.MapKey` — an integer ordinal for a logit head, the model's own `String` or `Int64` label for a `ZipMap` head — and a caller reading ordinals recovers them from the `Int64` case. A parallel `Classify` sibling for the structured head would fork one ranked-output concept across two names whose `top` arities then drift.
- Law: streaming is LAZY and ORDERED. `Chunked` admits its frame arithmetic ONCE on the outer rail and then yields one window at a time, so a screening corpus of N windows costs one window of results rather than N, and the emission order is the cursor's own. The drain is a SYNCHRONOUS native cursor over one bound staging value — there is nothing to await between windows, and both providers (a materialized sequence view and a hyperslab filler) read synchronously — so `IEnumerable<Fin<T>>` is the honest shape where the sibling `Runtime/transport#…` stream genuinely awaits a socket and takes `IAsyncEnumerable`. Ordered emission is structural in a single cursor, which is what satisfies the branch ordered-await ruling without a second writer.
- Receipt: `ModelRun` carries model checksum, EP, run mode, batch, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `GetTensorMemoryInfo` allocator name as `ArenaAllocator`, and the optional `Runtime/claims#PROFILE_EVIDENCE` `ProfileArtifact.ChromeTrace` profile evidence — content-keyed by the admitted `ArtifactIndexRow`'s `ContentAddress` and stamped with the `InferenceSession.ProfilingStartTimeNs` epoch, never a loose path string; profiling artifacts land as `ArtifactKind.OnnxProfile` rows. The WORK LANE is a parameter, never a constant: a warm-sweep cold open and an interactive lease run on different lanes, and hardwiring either publishes one call site's context on every run — the same law `Model/identity#MODEL_IDENTITY` `LoadReceipt` holds for the load half. `AllocationClass.NativeOrt` stays fixed because the arena a run allocates from is a property of the runtime doing the running.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, System.Text.Json, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Domain.Ranked`/`Domain.ExtremumDirection`), Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox (`System.Runtime.InteropServices.MemoryMarshal` — the provider staging view)
- Growth: a new run shape is one payload case; a new measured warm fact is one column on the session capsule's `WarmEvidence` filled by one more read inside `WarmPulse`, never a second pulse surface; a new run-config posture is one `RunConfig` row with its `AddRunConfigEntry` pairs and `OrtAllocatorType` arena column; a new pooling shape is one delegate-backed `Pooling` row; a new window provider is one `ChunkSource` case; a new classification modality is one `ClassHead` case, never a sibling entrypoint; a new refusal is one named `RunRefusal` over the shared contract vocabulary, never a free slug; a BIM point-cloud→element classifier, symbol recognizer, or clash scorer is one more `Classify`/`ClashScore` run over the shared session — consuming the interchange `PointScan` encoding and the `Solver/clash#CLASH_AND_TWIN` `ClashPair` vector — never a BIM-specific service; a tensor-lane handoff already holding a `Tensor<T>` is one `InferTensor` run with zero managed copy; an input plane larger than the session's admitted shape is one `Model/tiling#TILE_PLAN` mosaic over this same `BoundFlow`, never a free-dimension override that re-plans memory on every extent.
- Boundary: `RunOps` extends `Model/sessions#SESSION_CAPSULE` with bracketed native disposal. `CreateTensorValueFromMemory` binds rented staging without a copy; input ownership transfers at the run, and `Bracket` disposes every admitted input beside the result collection. The bracket's ACQUIRE leg is rail-shaped and carries NO custody of its own: it hands back exactly what the run produced, the bracket seats it in its custody cell before the projection runs, and every post-run native read — `SynchronizeBoundOutputs` among them — happens inside the projection where the cell already covers it. The one `try`/`finally` here is the boundary-capsule statement exemption `rails-and-effects.md` `[RESOURCE_BOUNDARY]` names; it never appears in domain flow, and it is unconditional release where `Custody.Rollback` is failure-release. `InferBound` calls the `OrtValue`-only `RunWithBoundResults` member directly; its named arm zips `GetOutputNames()` against that same collection and never materializes `DisposableNamedOnnxValue`. Every projection proves a nonempty output collection before `First()`. `BoundFlow` binds input and sink from `ModelSessions.SharedAllocator`, and `Pulse` writes through the mutable native span without staging, taking ONE `FlowPayload` whose case owns its own `Write` arm rather than re-spelling the overload `BoundFlow.Write` already carries. `Chunked` yields one `StreamSegment` per completed run through `StreamReceipt`. `Embed` derives its final axis from output shape and L2-normalizes the pooled vector; `Classify` derives class width, proves row divisibility for the logit head and delegates the sequenced head to `CustomOps.Egress` under the slot the identity snapshot declared. `Profile` admits its artifact through `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)`, grouping the trace under the profiled model checksum, and mints the typed `ProfileArtifact.ChromeTrace` evidence from the admitted row's `ContentAddress` and the `ProfilingStartTimeNs` epoch in the same pass — the index row is custody, the union case is receipt evidence, one identity joining both — while handing the trace BYTES out beside them so a reader wanting the events back takes what admission already read instead of re-reading a file whose path it would need separately; retention derives from `ArtifactKind.Retention`; the `Model/sessions#SESSION_CAPSULE` options fold sets `ProfileOutputPathPrefix` BEFORE `EnableProfiling` because the setter reads the prefix at flip time and discards a later assignment silently. `WarmPulse` composes that pair into the session capsule's injected warm-pulse shape: the caller supplies the bound run for its own bucket, because only the surface that built the flow knows this model's input roster, and the fold owns closing profiling, admitting the trace, and reading the census — `JsonDocument.Parse` over caller-owned memory, `EnumerateArray` over the events, `TryGetProperty` by UTF-8 key on every read, and the document disposed inside the fold so no `JsonElement` view outlives its pooled rental. Bounded selection composes the kernel `Rasm/Domain/stats#ORDER_STATISTICS` `Ranked` cell at both classification arms, so this page declares no heap of its own and the direction is stated rather than encoded as a negated key.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Pooling {
    // Token-major reduction over a `[tokens, hidden]` plane: the row projection is the `Span2D` view's, so the
    // `token * hidden` arithmetic no arm ever spelled differently is gone from every arm at once.
    public static readonly Pooling Mean = new("mean", static (states, hidden) => {
        float[] pooled = new float[hidden];
        ReadOnlySpan2D<float> plane = ReadOnlySpan2D<float>.DangerousCreate(in states[0], states.Length / hidden, hidden, pitch: 0);
        for (int token = 0; token < plane.Height; token++) { TensorPrimitives.Add(pooled, plane.GetRowSpan(token), pooled); }
        TensorPrimitives.Divide(pooled, plane.Height, pooled);
        return pooled;
    });
    public static readonly Pooling Cls = new("cls", static (states, hidden) => states[..hidden].ToArray());
    public static readonly Pooling Last = new("last", static (states, hidden) => states[^hidden..].ToArray());
    public static readonly Pooling Max = new("max", static (states, hidden) => {
        float[] pooled = states[..hidden].ToArray();
        ReadOnlySpan2D<float> plane = ReadOnlySpan2D<float>.DangerousCreate(in states[0], states.Length / hidden, hidden, pitch: 0);
        for (int token = 1; token < plane.Height; token++) { TensorPrimitives.MaxNumber(pooled, plane.GetRowSpan(token), pooled); }
        return pooled;
    });

    [UseDelegateFromConstructor]
    public partial float[] Apply(ReadOnlySpan<float> states, int hidden);
}

// The admitted operand family. `Admit` stays an ABSTRACT member rather than a generated `Switch` fold because two
// cases carry their OWN type parameter: a generated arm is a non-generic delegate and cannot bind `T`, so the
// dispatch the generator would emit is unspellable for this family. That is a language constraint, not the
// manual-hierarchy form reserved for foreign extension — nothing outside this package derives here.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunInput {
    private RunInput() { }
    public sealed record Managed<T>(string Name, T[] Data, long[] Shape) : RunInput where T : unmanaged {
        public override Fin<(string Name, OrtValue Value)> Admit() => TensorBridge.Ingress(Data, Shape).Map(value => (Name, value));
    }
    public sealed record Carrier<T>(string Name, Tensor<T> Tensor) : RunInput where T : unmanaged {
        public override Fin<(string Name, OrtValue Value)> Admit() => TensorBridge.Ingress(Tensor).Map(value => (Name, value));
    }
    public sealed record Strings(string Name, Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> Tokens) : RunInput {
        public override Fin<(string Name, OrtValue Value)> Admit() => TensorBridge.Ingress(Tokens).Map(value => (Name, value));
    }
    public sealed record Sparse(string Name, OrtValue Value) : RunInput {
        public override Fin<(string Name, OrtValue Value)> Admit() => Value.IsSparseTensor
            ? Fin.Succ((Name, Value))
            : RunRefusal.SparseInput.Fault();
    }

    public abstract Fin<(string Name, OrtValue Value)> Admit();
}

// The write-admissible payload family a bound pulse takes. `BoundFlow.Write` already overloads on payload, so a
// `Pulse` overload pair re-spelled that decision at a second surface and made the discriminant unrecoverable from
// the value; each case carries its own write arm instead. `ReadOnlyMemory<float>` rather than a span because a
// closed family holds no ref struct — every producer here already owns a `MemoryOwner<float>` or a `float[]`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FlowPayload {
    private FlowPayload() { }
    public sealed record Floats(ReadOnlyMemory<float> Values) : FlowPayload {
        public override Fin<Unit> Write(BoundFlow flow) => flow.Write(Values.Span);
    }
    public sealed record Bytes(ReadOnlySequence<byte> Window) : FlowPayload {
        public override Fin<Unit> Write(BoundFlow flow) => flow.Write(Window);
    }

    public abstract Fin<Unit> Write(BoundFlow flow);
}

// Index-keyed span filler — the provider seam for corpus-backed streaming: `fill` lands window `index` into the
// reused staging span, an HDF5 response corpus filling it with one hyperslab read, so PureHDF appears on no
// Compute signature because the delegate IS the seam.
public delegate Fin<Unit> WindowFill(int index, Span<float> window);

// Two window providers, one streaming entry. Both answer a frame count and a per-index window, so the frame
// arithmetic that both overloads spelled separately folds onto the family and the discriminant rides the value.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChunkSource {
    private ChunkSource() { }

    public sealed record Sequence(ReadOnlySequence<byte> Windows) : ChunkSource;

    public sealed record Filler(int Frames, WindowFill Fill) : ChunkSource;

    // ONE frame-count admission for both providers: a nonpositive width, a ragged byte length, a window past the
    // addressable ceiling, and a nonpositive frame count all refuse here, before any window is read.
    public Fin<int> Admit(int windowFloats) {
        long frameBytes = (long)windowFloats * sizeof(float);
        return windowFloats <= 0 || frameBytes > int.MaxValue
            ? RunRefusal.ChunkShape.Fault()
            : Switch(
                sequence: source => frameBytes > 0L && source.Windows.Length % frameBytes is 0L
                    && source.Windows.Length / frameBytes <= int.MaxValue
                    ? Fin.Succ((int)(source.Windows.Length / frameBytes))
                    : RunRefusal.ChunkShape.Fault(),
                filler: source => source.Frames > 0
                    ? Fin.Succ(source.Frames)
                    : RunRefusal.ChunkShape.Fault());
    }
}

// Classification carries two MODALITIES under one entry: a numeric logit tensor this page softmaxes, and the
// `ZipMap` sequence-of-maps head the numeric egress cannot carry at all. Both rank `(class, probability)` rows,
// so a sibling entrypoint would fork one ranked-output concept across two names whose `top` arities then drift.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClassHead {
    private ClassHead() { }

    public sealed record Logits(string Tensor, int Top = 1) : ClassHead;

    // The declared slot rides in because `CustomOps.Egress` proves the value against it — a `seq(map(int64,float))`
    // head admitted by the identity snapshot and read with no slot is exactly the coverage asymmetry that owner closes.
    public sealed record Sequenced(string Tensor, SlotShape Declared, int Top = 1) : ClassHead;

    public string Tensor => Switch(logits: static head => head.Tensor, sequenced: static head => head.Tensor);

    public int Top => Switch(logits: static head => head.Top, sequenced: static head => head.Top);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record RunConfig(FrozenDictionary<string, string> Entries, OrtAllocatorType Arena) {
    public static readonly RunConfig Steady = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.ArenaAllocator);
    public static RunConfig Bulk(string arenaShrinkDevice) => new(new Dictionary<string, string>(StringComparer.Ordinal) {
        ["memory.enable_memory_arena_shrinkage"] = arenaShrinkDevice,
    }.ToFrozenDictionary(StringComparer.Ordinal), OrtAllocatorType.ArenaAllocator);
    public static readonly RunConfig Device = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.DeviceAllocator);
}

public sealed record PlannedRun(RunOptions Options, CancellationTokenRegistration Latch) : IDisposable {
    public void Dispose() {
        Latch.Dispose();
        Options.Dispose();
    }
}

// --- [ERRORS] ------------------------------------------------------------------------------
// Named sites select bounded contracts directly; no string-key roster survives beneath the shared violation.
public static class RunRefusal {
    public static readonly ContractRefusal SparseInput = new(ComputeArea.Model, ComputeContract.Supported);
    public static readonly ContractRefusal BoundOutputCardinality = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal ProfilingDisabled = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ProfilingPathMissing = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal ChunkShape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal OutputMissing = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal EmbedShape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal EmbedNorm = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ClassifyShape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal ClassifyHead = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ClashScore = new(ComputeArea.Model, ComputeContract.Valid);

}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class RunOps {
    public static PlannedRun Plan(CancelScope scope, RunConfig config, Option<OrtLoraAdapter> lora = default) {
        RunOptions options = new();
        lora.Iter(options.AddActiveLoraAdapter);
        config.Entries.Iter(entry => options.AddRunConfigEntry(entry.Key, entry.Value));
        return new PlannedRun(options, scope.Source.Token.Register(() => options.Terminate = true));
    }

    // Abort-first traversal with failure-release custody on the rail: the first refusal releases every value
    // already admitted and leaves carrying its OWN error, where the hand fold re-mapped the failed rail through
    // `Map` to change its type and discarded the refusal's identity in the process.
    public static Fin<Seq<(string Name, OrtValue Value)>> Bind(params ReadOnlySpan<RunInput> inputs) {
        Seq<RunInput> roster = toSeq(inputs.ToArray());
        Seq<OrtValue> admitted = Seq<OrtValue>();
        return roster
            .TraverseM(input => input.Admit().Map(row => { admitted = admitted.Add(row.Value); return row; }))
            .As()
            .Rollback([.. admitted]);
    }

    extension(InferenceSession session) {
        public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            Bracket(scope, inputs, project, () => session.Run(options, inputs.Map(static row => row.Name), inputs.Map(static row => row.Value), outputs));

        // `SynchronizeBoundOutputs` moved INTO the projection: it is a post-run native read, so it belongs where
        // the bracket's custody cell already covers the produced collection. In the acquire leg it needed a bare
        // `catch { results.Dispose(); throw; }` to cover the window between production and seating — a window that
        // does not exist once the leg hands back exactly what the run produced.
        public Fin<T> InferBound<T>(RunOptions options, CancelScope scope, OrtIoBinding binding, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Option<Func<Seq<(string Name, OrtValue Value)>, Fin<T>>> named = default) {
            binding.SynchronizeBoundInputs();
            return Bracket(
                scope,
                Seq<(string Name, OrtValue Value)>(),
                results => {
                    binding.SynchronizeBoundOutputs();
                    return named.Case is Func<Seq<(string Name, OrtValue Value)>, Fin<T>> zip
                        ? binding.GetOutputNames() is string[] names && names.Length == results.Count
                            ? zip(toSeq(names).Zip(toSeq(results), static (name, value) => (Name: name, Value: value)))
                            : RunRefusal.BoundOutputCardinality.Fault<T>()
                        : project(results);
                },
                () => session.RunWithBoundResults(options, binding));
        }

        // TRACE SCHEMA (measured at the pin, and only the facts no signature can show): the emitted profile is a
        // BARE JSON ARRAY — no `traceEvents` wrapper. Array order is COMPLETION order, so node events precede the
        // `Session` spans enclosing them: join on `ts`/`dur`, never on position, and read `args` BY KEY because the
        // bag carries no fixed member order. `provider` spells the full EP class name (the `GetAvailableProviders`
        // vocabulary, 1:1 onto the `ExecutionProvider` rows); a non-CPU EP fuses its subgraph, so `op_name` becomes
        // the synthesized fusion name `<hash>_<EP>_<hash>_<n>` — opaque to per-op-type aggregation — and
        // `node_index` indexes the POST-PARTITION graph rather than any stable model-authoring identity.
        // The trace BYTES ride out beside the row and the evidence: admission already read them whole, so a reader
        // that wants the events back — the warm pulse's partition census is the one — takes what this member holds
        // instead of re-reading a file whose path it would have to be handed separately.
        public Fin<(ArtifactIndexRow Row, ProfileArtifact Artifact, ReadOnlyMemory<byte> Trace)> Profile(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, CancelScope scope, Instant at) =>
            !policy.Profiling
                ? RunRefusal.ProfilingDisabled.Fault<(ArtifactIndexRow, ProfileArtifact, ReadOnlyMemory<byte>)>()
                : Op.Of(name: "model.profile-close").Catch(() => session.EndProfiling() is string path
                        ? File.ReadAllBytes(path) switch {
                            var trace => Fin.Succ((
                                Row: ArtifactIndexRow.Admit(ArtifactKind.OnnxProfile, path, trace, classification, at, Some(sourceKey)),
                                Trace: (ReadOnlyMemory<byte>)trace)),
                        }
                        : RunRefusal.ProfilingPathMissing.Fault<(ArtifactIndexRow Row, ReadOnlyMemory<byte> Trace)>(), scope.Source.Token)
                    .MapFail(error => ModelSessions.Faulted(scope, error))
                    .Map(admitted => (
                        admitted.Row,
                        (ProfileArtifact)new ProfileArtifact.ChromeTrace(admitted.Row.Content, session.ProfilingStartTimeNs),
                        admitted.Trace));

        // The warm PULSE the composition injects into `Model/sessions#SESSION_CAPSULE` `Warmup`. The caller supplies
        // the one bound run for its own bucket shape — only the surface that built the flow knows this model's input
        // roster and dtypes — and this fold owns everything after it: close profiling, admit the trace, read the
        // partition census out of it, because NO managed session member exposes that census at all. Evidence lands
        // as `Some` only on a trace that actually parsed, so a `None` partition column stays an unmeasured column
        // rather than a zero a consumer would read as an observation.
        public Fin<ModelSessions.WarmEvidence> WarmPulse(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, CancelScope scope, Instant at, Func<Fin<Unit>> run) =>
            run()
                .Bind(_ => session.Profile(policy, sourceKey, classification, scope, at))
                .Bind(profiled => Op.Of(name: "model.warm-trace").Catch(() => Fin.Succ(Partitions(profiled.Trace)))
                .Map(static partitions => new ModelSessions.WarmEvidence(Some(partitions)));

        // Partition count is the DISTINCT provider set over node events PLUS one per fused node: a non-CPU EP
        // collapses each claimed subgraph into a single synthesized `<hash>_<EP>_<hash>_<n>` op, so counting
        // distinct providers alone reports one partition for a graph an EP cut into several — which is exactly the
        // number the stage's own partition cap gates on. `cat` discriminates node events from the `Session` spans
        // sharing the array, and the document disposes inside this fold so no view outlives its pooled rental.
        static int Partitions(ReadOnlyMemory<byte> trace) {
            using JsonDocument document = JsonDocument.Parse(trace, default);
            HashSet<string> providers = new(StringComparer.Ordinal);
            int fused = 0;
            foreach (JsonElement node in document.RootElement.EnumerateArray()) {
                if (!node.TryGetProperty("cat"u8, out JsonElement category)
                    || !category.ValueEquals("Node"u8)
                    || !node.TryGetProperty("args"u8, out JsonElement args)
                    || !args.TryGetProperty("provider"u8, out JsonElement provider)
                    || provider.GetString() is not string claimed) {
                    continue;
                }
                if (args.TryGetProperty("op_name"u8, out JsonElement op) && Fused(op.GetString(), claimed)) { fused++; }
                else { providers.Add(claimed); }
            }
            return providers.Count + fused;
        }

        // A fused op names its own EP INSIDE the synthesized identity, which is the one signal separating a claimed
        // subgraph from an ordinary node the same EP happened to run under its own operator name.
        static bool Fused(string? opName, string provider) =>
            opName is not null && opName.Contains($"_{provider}_", StringComparison.Ordinal);

        public Fin<float[]> Embed(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Pooling pooling) =>
            session.Infer(options, scope, inputs, Seq(output), results => {
                if (results.Count is 0) { return RunRefusal.OutputMissing.Fault<float[]>(); }
                OrtValue value = results.First();
                long[] shape = value.GetTensorTypeAndShape().Shape;
                int hidden = shape.Length > 0 && shape[^1] is > 0 and <= int.MaxValue ? (int)shape[^1] : 0;
                ReadOnlySpan<float> states = value.GetTensorDataAsSpan<float>();
                bool unbatched = shape.Length is 2 || shape.Length is 3 && shape[0] is 1;
                if (!unbatched || hidden is 0 || states.IsEmpty || states.Length % hidden is not 0 || !TensorPrimitives.IsFiniteAll(states)) {
                    return RunRefusal.EmbedShape.Fault<float[]>();
                }
                float[] pooled = pooling.Apply(states, hidden);
                float norm = TensorPrimitives.Norm<float>(pooled);
                if (!float.IsFinite(norm) || norm <= 0f) { return RunRefusal.EmbedNorm.Fault<float[]>(); }
                TensorPrimitives.Divide(pooled, norm, pooled);
                return Fin.Succ(pooled);
            });

        public Fin<TResult> InferTensor<T, TResult>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Func<ReadOnlyTensorSpan<T>, Fin<TResult>> project) where T : unmanaged =>
            session.Infer(options, scope, inputs, Seq(output), results =>
                results.Count is 0
                    ? RunRefusal.OutputMissing.Fault<TResult>()
                    : project(results.First().GetTensorDataAsTensorSpan<T>()));

        // ONE ranked-output entry over both classification modalities. The logit arm softmaxes and ranks integer
        // ordinals; the sequenced arm hands the value to `CustomOps.Egress` under the declared slot and reads the
        // `OpOutput.Sequence` of `Mapping` rows the `ZipMap` head produced — the reader `Model/extension` owns and
        // this page composes rather than re-deriving a second structured extraction.
        public Fin<Seq<Seq<(OpOutput.MapKey Class, double Probability)>>> Classify(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, ClassHead head) =>
            session.Infer(options, scope, inputs, Seq(head.Tensor), results =>
                results.Count is 0
                    ? RunRefusal.OutputMissing.Fault<Seq<Seq<(OpOutput.MapKey, double)>>>()
                    : head.Switch(
                        state: (Value: results.First(), Head: head),
                        logits: static (s, _) => Logits(s.Value, s.Head.Top),
                        sequenced: static (s, arm) => Sequenced(s.Value, arm.Declared, s.Head.Top)));

        public Fin<float> ClashScore(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> features, string output) =>
            session.Infer(options, scope, features, Seq(output), static results => {
                if (results.Count is 0) { return RunRefusal.OutputMissing.Fault<float>(); }
                ReadOnlySpan<float> scores = results.First().GetTensorDataAsSpan<float>();
                return scores.Length is not 1 || !float.IsFinite(scores[0])
                    ? RunRefusal.ClashScore.Fault<float>()
                    : Fin.Succ(scores[0]);
            });

        // The lane is a PARAMETER: a warm-sweep cold open and an interactive lease run on different lanes, and a
        // hardwired row publishes one call site's context on every run — the law `Model/identity` already holds for
        // the load half. The profile column and the allocator name cross as the `Option`s the receipt case
        // declares, so nothing collapses a carrier at this edge and no unsafe extraction re-enters the domain.
        public ComputeReceipt.ModelRun RunReceipt(ModelIdentity model, ExecutionProvider ep, string mode, int batch, OrtValue output, CorrelationId correlation, WorkLane lane, Option<ProfileArtifact> profile, Duration elapsed) =>
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()),
                Optional(output.GetTensorMemoryInfo().Name), profile) {
                Scope = new ReceiptScope.Execution(correlation, lane, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
            };

        public Fin<ComputeReceipt.StreamSegment> StreamReceipt(ChunkSource source, int windowFloats, string artifactId, CorrelationId correlation, WorkLane lane, Duration elapsed) =>
            source.Admit(windowFloats)
                .Map(frames => new ComputeReceipt.StreamSegment(
                        artifactId, frames, (long)frames * windowFloats * sizeof(float), Census: None) {
                    Scope = new ReceiptScope.Execution(correlation, lane, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
                });
    }

    extension(BoundFlow flow) {
        // ONE pulse over the payload family; the case owns its write arm, so no overload pair re-states a decision
        // `BoundFlow.Write` already made.
        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, FlowPayload payload, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            payload.Write(flow).Bind(_ => Bracket(scope, Seq<(string Name, OrtValue Value)>(), project, () => flow.Run(options)));

        // LAZY and ORDERED: the frame arithmetic admits once on the outer rail — a malformed request refuses before
        // any run — and each window then yields as it completes, so a corpus of N windows costs one window of
        // results. The cursor is one bound staging value and every read is synchronous, so there is nothing between
        // windows to await; ordering is the cursor's own rather than a second writer's index law.
        public Fin<IEnumerable<Fin<T>>> Chunked<T>(RunOptions options, CancelScope scope, ChunkSource source, int windowFloats, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            source.Admit(windowFloats).Map(frames => Windows(flow, options, scope, source, windowFloats, frames, project));

        static IEnumerable<Fin<T>> Windows<T>(BoundFlow flow, RunOptions options, CancelScope scope, ChunkSource source, int windowFloats, int frames, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            long frameBytes = (long)windowFloats * sizeof(float);
            using SpanOwner<byte> staging = SpanOwner<byte>.Allocate(windowFloats * sizeof(float));
            for (int index = 0; index < frames; index++) {
                yield return source.Switch(
                    state: (Flow: flow, Options: options, Scope: scope, Index: index, Bytes: frameBytes, Project: project),
                    sequence: static (s, window) => s.Flow.Pulse(
                        s.Options, s.Scope, new FlowPayload.Bytes(window.Windows.Slice(s.Index * s.Bytes, s.Bytes)), s.Project),
                    filler: (s, window) => window.Fill(s.Index, MemoryMarshal.Cast<byte, float>(staging.Span))
                        .Bind(_ => s.Flow.Pulse(
                            s.Options, s.Scope, new FlowPayload.Bytes(new ReadOnlySequence<byte>(staging.Memory)), s.Project)));
            }
        }
    }

    static Fin<Seq<Seq<(OpOutput.MapKey Class, double Probability)>>> Logits(OrtValue value, int top) {
        long[] shape = value.GetTensorTypeAndShape().Shape;
        ReadOnlySpan<float> scores = value.GetTensorDataAsSpan<float>();
        int width = shape.Length > 0 && shape[^1] is > 0 and <= int.MaxValue ? (int)shape[^1] : 0;
        if (width is 0 || scores.Length % width is not 0 || top is < 1 || top > width || !TensorPrimitives.IsFiniteAll(scores)) {
            return RunRefusal.ClassifyShape.Fault<Seq<Seq<(OpOutput.MapKey, double)>>>();
        }
        int rows = scores.Length / width;
        using SpanOwner<float> probabilities = SpanOwner<float>.Allocate(rows * width);
        Seq<(OpOutput.MapKey, double)>[] ranked = new Seq<(OpOutput.MapKey, double)>[rows];
        Span2D<float> plane = probabilities.Span.AsSpan2D(rows, width);
        for (int row = 0; row < rows; row++) {
            Span<float> probability = plane.GetRowSpan(row);
            TensorPrimitives.SoftMax(scores.Slice(row * width, width), probability);
            ranked[row] = Best(probability, top);
        }
        return Fin.Succ(toSeq(ranked));
    }

    // The structured head reads through the ONE non-tensor reader: `Egress` projects the value onto `OpOutput` and
    // proves it against the slot the identity snapshot declared, so a `seq(map(int64,float))` head that admission
    // accepted has a reader here rather than an unexplained refusal, and a head shaped otherwise names its slot.
    static Fin<Seq<Seq<(OpOutput.MapKey Class, double Probability)>>> Sequenced(OrtValue value, SlotShape declared, int top) =>
        value.Egress(declared).Bind(output => output is OpOutput.Sequence rows
            ? rows.Elements
                .Traverse(element => element is OpOutput.Mapping mapping
                    ? Fin.Succ(Best(mapping.Pairs, top)).ToValidation()
                    : RunRefusal.ClassifyHead.Fault<Seq<(OpOutput.MapKey, double)>>().ToValidation())
                .As()
                .ToFin()
            : RunRefusal.ClassifyHead.Fault<Seq<Seq<(OpOutput.MapKey, double)>>>());

    // Bounded top-`k` composes the kernel `Ranked` cell; no heap declares here. The STREAMING arm takes the dense
    // probability row, because offering per index never materializes a `(class, probability)` pair per class of a
    // wide taxonomy, and the ONE-SHOT fold takes the `ZipMap` roster, which arrives as a `Seq` already. Direction
    // is STATED at both sites. NAMED LOSS on the collapse: the local heap's `(Probability, -index)` composite key
    // deletes — a negated ordinal is not an ordering, and the only thing it bought was the class stability the
    // cell's strictly-better admission gives by keeping the first arrival among bound-ties.
    // Both members spell `Best` rather than `Ranked`: a local named for the kernel owner would SHADOW it under
    // simple-name lookup and make `Ranked.Top` bind a method group — the same interior-versus-owner split the
    // stage wire takes for `Pad`.
    static Seq<(OpOutput.MapKey Class, double Probability)> Best(ReadOnlySpan<float> probability, int top) {
        Ranked<int, double> cell = new(keep: top, direction: ExtremumDirection.Maximum);
        for (int index = 0; index < probability.Length; index++) { cell.Offer(index, probability[index]); }
        return cell.Drain().Map(cls => ((OpOutput.MapKey)new OpOutput.MapKey.Int64(cls), (double)probability[cls]));
    }

    static Seq<(OpOutput.MapKey Class, double Probability)> Best(Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)> pairs, int top) =>
        Ranked.Top(
            source: pairs.Map(static pair => (pair.Key, Probability: pair.Value.Switch(
                real: static value => value.Value,
                whole: static value => (double)value.Value))),
            keep: top,
            key: static row => row.Probability,
            direction: ExtremumDirection.Maximum);

    // Ownership transfers at the run: the bracket's completion is the ONE deterministic release for admitted inputs
    // and produced results alike. The acquire leg carries NO custody of its own — it hands back exactly what the run
    // produced and the cell seats it before the projection runs — so every post-run native read happens where the
    // cell already covers it and the nested `catch { results.Dispose(); throw; }` has no window left to guard.
    // `ModelSessions.Faulted` classifies EVERY throw, so cancellation, artifact unreadability, and a native
    // rejection each land on their own fault and no fourth throw class escapes the way a `when` roster let it.
    // The `finally` is the boundary-capsule statement exemption: release here is unconditional, where
    // `Custody.Rollback` is failure-release and cannot express it.
    // Custody discriminant, stated: `owned` inputs ride the kernel UNCONDITIONAL span arm (spent whether the run
    // succeeds or not), the native result collection rides the ACQUIRE arm (produced inside the window) — the
    // kernel resource rail owns both LIFO disposals and their fault aggregation, and `ModelSessions.Faulted`
    // stays the ONE classifier on the way out.
    static Fin<T> Bracket<T>(CancelScope scope, Seq<(string Name, OrtValue Value)> owned, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Func<IDisposableReadOnlyCollection<OrtValue>> run) =>
        Custody.Bracket(
                () => Custody.Bracket(run, project, Op.Of(nameof(RunOps))),
                [.. owned.Map(static row => (IDisposable?)row.Value)])
            .MapFail(error => ModelSessions.Faulted(scope, error));
}
```

## [03]-[BATCH_GATE]

- Owner: `BatchGate` the bounded-window cross-request coalescer over one shared `BoundFlow`; `BatchPolicy` `[ComplexValueObject]` carries the packed row cap, the fill delay, and the queue's own `LaneBound.Parked` bound — the landed `Runtime/scheduling#WORK_LANES` vocabulary rather than a call-site option literal.
- Cases: one gate per `(flow, rowWidth)` pair; the queue bound is a `Parked` case by construction.
- Entry: `public static Fin<BatchGate> Admit(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, MonotonicTimeline timeline, Action<LanePressure> pressure)` and `public ValueTask<Fin<float[]>> Submit(float[] row)`.
- Law: the queue bound is PARKED and nothing else. A shedding bound drops an admitted row, and a dropped row's `Submit` awaits a reply promise nothing will ever complete; a ranked bound is unbounded and refuses at a ceiling the submitter cannot retry against a native cursor it does not own. Parking is therefore the only bound a request/reply gate admits, and carrying the case type as the column is what makes the other two unspellable rather than merely discouraged.
- Law: the fill window is a SCHEDULE, never a hand delay. One drain step repeats spaced by `MaxDelay` and bounded by `MaxDelay`, stopping the moment the window reaches `MaxRows`, so the latency ceiling and the batching cadence are one declared policy value rather than an `await Task.Delay` a reader has to reconstruct the bound from.
- Law: a parked write is MEASURED evidence. `WaitToWriteAsync` returns when capacity frees, so the park times against the kernel `MonotonicTimeline` across the WAIT alone — timing the write conflates the park with the write and leaves cancellation no seam — and the gate publishes `LanePressure.Parked` through its injected sink. A `Wait` lane with no park evidence is unreceipted backpressure exactly as a drop lane with no `itemDropped` is unreceipted loss.
- Law: the pump is a FORKED effect with custody, never a fire-and-forget task. `IO<A>.Fork` hands back a `ForkIO<A>` whose `Await` the gate's disposal harvests and whose `Cancel` it uses to stop, so a pump that fails names its failure at disposal — where a bare `Task` field faulting outside its own `try` parked every later `Submit` forever with nothing to observe.
- Law: the reply promise is a `TaskCompletionSource` and stays one. `ForkIO<A>.Await` awaits work the awaiter itself started; here the PUMP computes and the SUBMITTER waits, so no fork models the handoff and a hand promise is the primitive that does. The reader loop takes the DEFAULT token — the gate owns the only reader, completion is its shutdown signal, and a cancelable token forfeits the pooled parked-operation fast path on every wait.
- Auto: `Submit` snapshots each admitted row, proves width and finiteness, polls `WaitToWriteAsync` and folds `false` onto its own stop signal so the writer never throws on shutdown, and awaits its own reply. The pump drains up to `MaxRows` through one `Atom<Seq<Pending>>` CAS batch, packs the window into a `[MaxRows, rowWidth]` bound input with zero-padded tail rows, runs once, and fans results back to the submitted rows alone — rows are independent under a feed-forward per-row model and a variable-shape rebind per window is the rejected form; per-call ORT dispatch overhead dominates small-tensor inference, so a screening loop rides one packed run per window instead of thousands of singletons.
- Receipt: every window emits ONE `ComputeReceipt.ModelRun` whose `BatchSize` is the window's submitted row count (zero-padded tail rows never count) — per-submitter receipt fan-out is the rejected form; park pressure rides the injected `LanePressure` sink, never a second receipt case.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Parametric.MonotonicTimeline`), Rasm.AppHost (project)
- Growth: a batching posture is one `BatchPolicy` value, never a second coalescer; a further queue bound is unrepresentable by design (see the parked law); a further measured pressure fact is one `LanePressure` case at the scheduling owner.
- Boundary: `BatchGate` composes the shared `BoundFlow` and opens no session. The channel mint here is the ONE place this page spells `BoundedChannelOptions`, and it reads its capacity, full-mode, and reader arity off the declared `LaneBound.Parked` row rather than off literals; the lane owner's `LaneProfile.Open` is `WorkItem`-typed, so the two mints stay separate until that member widens to carry an element type. Disposal completes the writer, cancels the fork, awaits its harvest, and answers every still-parked and still-queued row with the stop fault, so no submitter is left holding a promise nothing completes.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class BatchPolicy {
    public int MaxRows { get; }

    // The queue's bound as the LANDED closed family's parked case, not a loose capacity int: the case type is the
    // column, so a shedding or ranked bound is unspellable here rather than refused at admission.
    public LaneBound.Parked Queue { get; }

    public Duration MaxDelay { get; }

    public static readonly BatchPolicy Canonical = Create(maxRows: 16, queue: new LaneBound.Parked(64), maxDelay: Duration.FromMilliseconds(4));

    public int MaxPending => Queue.Capacity;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int maxRows, ref LaneBound.Parked queue, ref Duration maxDelay) =>
        validationError = maxRows > 0 && queue.Capacity >= maxRows && maxDelay >= Duration.Zero
            ? null
            : new ValidationError(message: $"<batch-policy:{maxRows}:{queue.Capacity}:{maxDelay}>");
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class BatchGate : IAsyncDisposable {
    readonly record struct Pending(float[] Row, TaskCompletionSource<Fin<float[]>> Reply);

    readonly Channel<Pending> queue;
    readonly ForkIO<Unit> pump;
    readonly MonotonicTimeline timeline;
    readonly Action<LanePressure> pressure;
    readonly int rowWidth;

    private BatchGate(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, MonotonicTimeline timeline, Action<LanePressure> pressure, ForkIO<Unit> pump, Channel<Pending> queue) =>
        (this.rowWidth, this.timeline, this.pressure, this.pump, this.queue) = (rowWidth, timeline, pressure, pump, queue);

    public static Fin<BatchGate> Admit(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, MonotonicTimeline timeline, Action<LanePressure> pressure) =>
        from _ in guard(rowWidth > 0, (Error)RunRefusal.ChunkShape.Fault()).ToFin()
        let queue = Channel.CreateBounded<Pending>(new BoundedChannelOptions(policy.Queue.Capacity) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        })
        // The pump is FORKED, so its failure is a value disposal harvests rather than a silently faulted task
        // field that leaves every later `Submit` parked on a promise no reader will ever complete.
        from forked in Pump(queue, flow, options, scope, rowWidth, policy).Fork().Run()
        select new BatchGate(flow, options, scope, rowWidth, policy, timeline, pressure, forked, queue);

    public async ValueTask<Fin<float[]>> Submit(float[] row) {
        if (row.Length != rowWidth || !TensorPrimitives.IsFiniteAll(row)) {
            return RunRefusal.ChunkShape.Fault<float[]>();
        }
        TaskCompletionSource<Fin<float[]>> reply = new(TaskCreationOptions.RunContinuationsAsynchronously);
        // The writer POLLS rather than throwing: `WaitToWriteAsync` answers `false` on a completed channel, which
        // folds onto this gate's own stop fault, so shutdown needs no `OperationCanceledException` arm and no
        // `ChannelClosedException` arm. The park it measures is the wait alone.
        return await Parked()
            .Bind(admitted => admitted
                ? Fin.Succ(unit)
                : RunRefusal.ChunkShape.Fault<Unit>())
            .Match(
                Succ: async _ => {
                    await queue.Writer.WriteAsync(new Pending(row.ToArray(), reply)).ConfigureAwait(false);
                    return await reply.Task.ConfigureAwait(false);
                },
                Fail: static stopped => new ValueTask<Fin<float[]>>(Fin.Fail<float[]>(stopped)))
            .ConfigureAwait(false);

        async ValueTask<Fin<bool>> Parked() {
            Fin<MonotonicStamp> start = timeline.Capture();
            bool admitted = await queue.Writer.WaitToWriteAsync().ConfigureAwait(false);
            return start
                .Bind(from => timeline.Capture().Bind(to => timeline.Elapsed(from, to)))
                .Map(waited => {
                    pressure(new LanePressure.Parked(Duration.FromTimeSpan(waited)));
                    return admitted;
                });
        }
    }

    // The window is a CAS-batched cell, so a drain step lands every read row in one swap rather than mutating a
    // `List` a reader could observe half-filled; the fill REPEATS on a schedule spaced by the policy's own delay
    // and bounded by it, so the batching cadence and the latency ceiling are one declared value. The reader takes
    // the DEFAULT token: this gate owns the only reader and completion is its shutdown signal, where a cancelable
    // token would forfeit the pooled parked-operation fast path on every wait.
    static IO<Unit> Pump(Channel<Pending> queue, BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy) =>
        IO.liftAsync(async _ => await queue.Reader.WaitToReadAsync().ConfigureAwait(false))
            .Bind(open => open
                ? Fill(queue.Reader, policy).Bind(window => Emit(window, flow, options, scope, rowWidth, policy))
                : IO.pure(unit))
            .RepeatWhile(static _ => true)
            .Finally(IO.lift(() => {
                queue.Writer.TryComplete();
                Fin<float[]> cancelled = RunRefusal.ChunkShape.Fault<float[]>();
                while (queue.Reader.TryRead(out Pending pending)) { pending.Reply.TrySetResult(cancelled); }
                return unit;
            }));

    static IO<Seq<Pending>> Fill(ChannelReader<Pending> reader, BatchPolicy policy) {
        Atom<Seq<Pending>> window = Atom(Seq<Pending>());
        return IO.lift(() => {
                Seq<Pending> read = Seq<Pending>();
                while (window.Value.Count + read.Count < policy.MaxRows && reader.TryRead(out Pending head)) { read = read.Add(head); }
                return window.Swap(held => held + read);
            })
            .RepeatWhile(
                Schedule.spaced(policy.MaxDelay) & Schedule.upto(policy.MaxDelay),
                held => held.Count < policy.MaxRows);
    }

    // Bind shapes the bound input `[MaxRows, rowWidth]` ONCE: a partial window zero-pads the tail rows and fans
    // back only the submitted ones, so a variable-shape rebind per window never re-plans the arena.
    static IO<Unit> Emit(Seq<Pending> window, BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy) =>
        IO.lift(() => {
            if (window.IsEmpty) { return unit; }
            using MemoryOwner<float> packed = MemoryOwner<float>.Allocate(policy.MaxRows * rowWidth, AllocationMode.Clear);
            Span2D<float> rows = packed.Span.AsSpan2D(policy.MaxRows, rowWidth);
            window.Iter((pending, row) => pending.Row.CopyTo(rows.GetRowSpan(row)));
            Fin<Seq<float[]>> outcome = flow.Pulse(options, scope, new FlowPayload.Floats(packed.Memory), results => {
                if (results.Count is 0) { return RunRefusal.OutputMissing.Fault<Seq<float[]>>(); }
                ReadOnlySpan<float> scores = results.First().GetTensorDataAsSpan<float>();
                if (scores.IsEmpty || scores.Length % policy.MaxRows is not 0 || !TensorPrimitives.IsFiniteAll(scores)) {
                    return RunRefusal.ClassifyShape.Fault<Seq<float[]>>();
                }
                int stride = scores.Length / policy.MaxRows;
                Span2D<float> fanned = scores.AsSpan2D(policy.MaxRows, stride);
                float[][] sliced = new float[window.Count][];
                for (int row = 0; row < window.Count; row++) { sliced[row] = fanned.GetRowSpan(row).ToArray(); }
                return Fin.Succ(toSeq(sliced));
            });
            window.Iter((pending, row) => pending.Reply.TrySetResult(outcome.Map(fanned => fanned[row])));
            return unit;
        });

    public async ValueTask DisposeAsync() {
        queue.Writer.TryComplete();
        await pump.Cancel.RunAsync().ConfigureAwait(false);
        // The harvest is where a pump failure becomes visible: awaiting the fork answers its own `Fin`, so a
        // faulted drain names itself at disposal instead of dying inside an unobserved task.
        _ = await pump.Await.RunAsync().ConfigureAwait(false);
    }
}
```

## [04]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` — one `CacheReach` set-membership column drives every posture through the derived `ReadThroughStore` predicate and the derived `Flags` suppression set; `CacheOps` owns key derivation, echo-validated read-through, the precision-TTL negative probe, the one entry-options mint, the two eviction scopes, and the `Cached<T>` typed envelope. Cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact — a second fact stream is rejected, `ComputeReceipt` being the package's only measured-fact vocabulary.
- Cases: `CachePolicy` rows `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`; `CacheReach` members `Serve`, `StorePositive`, `StoreNegative`, `CutFirst`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` on `CacheRuntime` — the policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches rather than re-running every call, `precision` sizes the negative TTL, and present drift evidence is an input-shape discriminant rather than an independently reconstructed monitor. `public static Fin<Option<DriftVerdict>> Sentinel(Option<GraduationEnvelope> envelope, Seq<FeatureSample> serving, DriftPolicy policy)` is the ONE fold that fills that slot: the run flow hands it the model's own `GraduationEnvelope` and its serving window, and the report's headline verdict crosses while the per-feature verdicts and uncovered roster stay with the evidence reader.
- Law: the RECEIVER is `CacheRuntime`, never a bare `HybridCache`. `Read`, `Write`, `Invalidate`, and `Remove` are `Rasm.AppHost` `Runtime/resources#CACHE_SURFACE` extensions on the composed runtime, which resolves the lane's own service and — decisively — swaps `lane.Capsuled` for `lane.Entry` under an in-host capsule topology. A consumer reading `lane.Entry` directly writes to a distributed leg the deployment disarmed, and one that spells `Scoped`/`Tag`/`Key` by hand re-states framing the lane already internalizes and can drift from it at any one of those sites.
- Law: reach is ONE membership set, and it has FOUR members rather than five. Splitting the serve leg into local and distributed is refuted at the lane owner: `CacheLane.Capsuled` gates both legs together because `DisableDistributedCacheWrite` alone leaves every miss probing a permanently empty L2, so a per-leg column here would spell a posture the composed runtime overrides. `ReadThroughStore` and `Flags` derive off the set, which is what keeps a posture and the flags it sends from ever disagreeing.
- Law: drift invalidation keys on SEVERITY, never on a case. `DriftSeverity` carries its own `Invalidates` posture, so a new band is one row at the identity owner and no consumer dispatch moves; reading a `Breached` case here would break the moment a fourth band declared itself invalidating.
- Auto: `Key` stamps model checksum, input digest, and the provider's own `ResultKey` — ORT version, precision, option-table hash, and the SELECTED DEVICE fingerprint — so a residual produced on one adapter of a dual-GPU host never serves a request ranked onto the other, and cross-version drift never serves a stale hit; content-addressed dedup coalesces byte-identical-input/identical-EP runs to one stored payload. `Through` first consumes a verdict whose severity invalidates by PURGING the whole model's tag group and returning `ComputeFault.EquivalenceMiss`, then dispatches on the `ReadThroughStore` predicate — the read-through path delegates to `CacheSurface.Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the runtime's own entry options), and every other row falls to `Produced`, which cuts both keys when the reach carries `CutFirst`, serves a cached negative through a cache-only probe, produces once, clears stale negative evidence before a positive write, then stores the success under the result key or the failure under the `neg:` key at `ModelPrecision.NegativeTtl` — every member reaches a live branch, no posture a twin of another.
- Receipt: outcome projects onto `ComputeReceipt.Cache` at the sink edge, whose `Outcome` is the typed `CacheOutcome` row set (`Hit`/`Miss`/`Store`/`Evict`) rather than a free string a meter would fan on, and whose `Residual` and `Delta` columns answer ABSENCE here — a result cache stores a whole payload and codes nothing, so a zero ratio would grade an encode that never ran; live hit/miss/evict metering by lane tag is the `HybridCacheOptions.ReportTagMetrics` composition flag's consequence — a property the app root sets on the cache builder, never a member of the lane row — and `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated echo mismatches `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row naming its reach set, its suppression flags and options following by derivation; a new reach member is one `CacheReach` row plus the arm that reads it; a pre-computed result populates through the `WriteThrough` posture with a constant factory, never a second store entry; a richer outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner; a graduated-model validity axis is the `Model/identity#MODEL_IDENTITY` drift sentinel consumed here, never a sibling monitor.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect. Cached payloads ride the `Cached<Fin<T>>` typed envelope whose `Echo` is `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 corruption the content key alone cannot; a value stored without the echo is rejected. `ReadThrough` caches success and failure under one lane-TTL entry while `Negative` caches only the failure at `ModelPrecision.NegativeTtl` and re-produces every success — behaviourally distinct rows, so an identical-column twin of `ReadThrough` is the named defect. Content-addressed dedup folds the input digest into the stored key so identical-input runs across callers coalesce; a second dedup owner is rejected. Cross-process result-reuse recency horizons read by reference from the Persistence `ModelResultIndex` owner — a second `Duration horizon` parameter beside the policy rows is the named defect. Every verdict whose severity invalidates is consumed as reuse invalidation — the lane purges the model's whole tag group and the run faults `ComputeFault.EquivalenceMiss` — so a graduated model whose serving population leaves its `GraduationEnvelope` never keeps serving cached verdicts under ANY input digest, provider row, or precision; cutting the requested key alone was the invalidation that left the rest of the model serving, and a drift monitor beside the identity sentinel is the rejected sibling. Owner keys cross the lane seam and tags mint at `CacheLane.Tag` alone; ZERO hand-framed keys remain — `CacheSurface.Read` now carries the entry-options tail its `Write` sibling always had, so the negative probe rides the surface and a `CacheLane.Scoped` spelling on this page is the deleted form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Reach is combinable capability as vocabulary, not four parallel bools spanning sixteen corners of which five
// are inhabited. The serve leg has NO local/distributed split: `CacheLane.Capsuled` gates both legs together at
// the composed runtime, so a per-leg member here would declare a posture the topology overrides.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheReach {
    public static readonly CacheReach Serve = new("serve");
    public static readonly CacheReach StorePositive = new("store-positive");
    public static readonly CacheReach StoreNegative = new("store-negative");
    public static readonly CacheReach CutFirst = new("cut-first");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CachePolicy {
    public static readonly CachePolicy Bypass = new("bypass", []);
    public static readonly CachePolicy ReadThrough = new("read-through", [CacheReach.Serve, CacheReach.StorePositive, CacheReach.StoreNegative]);
    public static readonly CachePolicy WriteThrough = new("write-through", [CacheReach.StorePositive]);
    public static readonly CachePolicy Refresh = new("refresh", [CacheReach.CutFirst, CacheReach.StorePositive, CacheReach.StoreNegative]);
    public static readonly CachePolicy Negative = new("negative", [CacheReach.Serve, CacheReach.StoreNegative]);

    private CachePolicy(string key, params ReadOnlySpan<CacheReach> reach) : this(key) => Reach = reach.ToFrozenSet();

    public FrozenSet<CacheReach> Reach { get; }

    public bool Reaches(CacheReach member) => Reach.Contains(member);

    // The one posture that rides the runtime's own entry options untouched: it serves both legs, stores both
    // outcomes, and cuts nothing, so its derived suppression set is empty by construction rather than by a row
    // author remembering to leave four flags alone.
    public bool ReadThroughStore =>
        Reaches(CacheReach.Serve) && Reaches(CacheReach.StorePositive)
        && Reaches(CacheReach.StoreNegative) && !Reaches(CacheReach.CutFirst);

    // Four per-call suppression flags DERIVE from the membership that already decides the posture: a row that
    // serves nothing closes both read legs, a row that stores nothing closes both write legs. Deriving is what
    // keeps a posture and the flags it sends from ever disagreeing — an enumerated column is one more place a new
    // row gets filled in wrong.
    public HybridCacheEntryFlags Flags =>
        (Reaches(CacheReach.Serve)
            ? HybridCacheEntryFlags.None
            : HybridCacheEntryFlags.DisableLocalCacheRead | HybridCacheEntryFlags.DisableDistributedCacheRead)
        | (Reaches(CacheReach.StorePositive) || Reaches(CacheReach.StoreNegative)
            ? HybridCacheEntryFlags.None
            : HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct Cached<T>(string Echo, T Value);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class CacheOps {
    // The SELECTED device joins result identity: `AutoSelect` ranks by affinity, so one provider key spans two
    // adapters on a dual-GPU host and a result produced on one is not a result the other would have produced.
    public static ModelResultKey Key(ModelIdentity model, UInt128 inputDigest, ExecutionProvider ep, ModelPrecision precision, Option<OrtEpDevice> device) =>
        new(model.Key, inputDigest, ep.ResultKey(OrtEnv.Instance().GetVersionString(), precision, device));

    // The one fold that SUPPLIES `Through`'s drift slot. A graduated model arrives with its own envelope and the
    // caller's serving window is the population that model is now seeing, so the headline verdict — the worst
    // covered feature — is what gates reuse; the per-feature verdicts and the uncovered roster stay on the report
    // for the surface that reads evidence rather than the one that invalidates. A model carrying NO envelope was
    // never graduated, so it has no population to leave and the slot answers `None` rather than a fabricated
    // `Stable` a purge would then read as evidence of health.
    public static Fin<Option<DriftVerdict>> Sentinel(
        Option<GraduationEnvelope> envelope, Seq<FeatureSample> serving, DriftPolicy policy) =>
        envelope.Match(
            Some: held => held.Drift(serving, policy).Map(static report => Some(report.Worst)),
            None: static () => Fin.Succ(Option<DriftVerdict>.None));

    static Fin<T> Validated<T>(ModelResultKey key, Cached<Fin<T>> cached) =>
        StringComparer.Ordinal.Equals(cached.Echo, key.ModelChecksum)
            ? cached.Value
            : Fin.Fail<T>(new ComputeFault.CacheCorrupt(key.ToString()));

    extension(CacheRuntime runtime) {
        public async ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default) {
            // Severity carries its own invalidation posture, so a fourth band declaring itself invalidating lands
            // at the identity owner and this gate never moves — where a `Breached` case read would silently keep
            // serving it.
            if (drift.Case is DriftVerdict verdict && verdict.Severity.Invalidates) {
                await runtime.Invalidate(CacheLane.ModelResult, Seq(key.ModelChecksum), token);
                return Fin.Fail<T>(new ComputeFault.EquivalenceMiss(
                    $"drift:{verdict.EvidenceKey:x32}:{verdict.Feature}:{verdict.Statistic.Key}:{verdict.Score}:{verdict.SampleCount}"));
            }
            return policy.ReadThroughStore
                ? await ServeStore(runtime, key, state, produce, token)
                : await Produced(runtime, policy, key, precision, state, produce, token);
        }
    }

    // OWNER keys cross the lane seam, never tags: the surface frames the checksum through `CacheLane.Tag`, adds
    // the bare lane key, and resolves the entry options through `CacheRuntime.Entry` — which the capsule topology
    // swaps for `Capsuled`, the one read a call site could not reproduce by naming `lane.Entry` itself.
    static async ValueTask<Fin<T>> ServeStore<T, TState>(CacheRuntime runtime, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) =>
        Validated(key, await runtime.Read(
            CacheLane.ModelResult, key.ToString(),
            (Key: key, State: state, Produce: produce),
            static async (s, ct) => new Cached<Fin<T>>(s.Key.ModelChecksum, await s.Produce(s.State, ct)),
            Seq(key.ModelChecksum), token));

    static async ValueTask<Fin<T>> Produced<T, TState>(CacheRuntime runtime, CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) {
        if (policy.Reaches(CacheReach.CutFirst)) {
            await runtime.Remove(CacheLane.ModelResult, key.ToString(), token);
            await runtime.Remove(CacheLane.ModelResult, $"neg:{key}", token);
        }
        if (policy.Reaches(CacheReach.Serve)) {
            // Cache-only probe: `DisableUnderlyingData` suppresses the factory, so the None arm writes nothing
            // and the empty owner set never mints a tag — the whole read rides the AppHost surface now that
            // `Read` takes the entry-options tail its `Write` sibling always carried.
            Option<Cached<Fin<T>>> probed = await runtime.Read(
                CacheLane.ModelResult, $"neg:{key}", unit,
                static (_, _) => new ValueTask<Option<Cached<Fin<T>>>>(Option<Cached<Fin<T>>>.None),
                Seq<string>(), Some(Options(runtime, policy, HybridCacheEntryFlags.DisableUnderlyingData)), token);
            if (probed.Case is Cached<Fin<T>> cached) { return Validated(key, cached); }
        }
        Fin<T> value = await produce(state, token);
        if (value.IsSucc && policy.Reaches(CacheReach.StorePositive)) {
            await runtime.Remove(CacheLane.ModelResult, $"neg:{key}", token);
            await runtime.Write(
                CacheLane.ModelResult, key.ToString(), new Cached<Fin<T>>(key.ModelChecksum, value),
                Seq(key.ModelChecksum), Some(Options(runtime, policy)), token);
        }
        else if (value.IsFail && policy.Reaches(CacheReach.StoreNegative)) {
            await runtime.Write(
                CacheLane.ModelResult, $"neg:{key}", new Cached<Fin<T>>(key.ModelChecksum, value),
                Seq(key.ModelChecksum), Some(Options(runtime, policy, expiration: precision.NegativeTtl.ToTimeSpan())), token);
        }
        return value;
    }

    // ONE options mint for every write and probe this page makes: the runtime's OWN entry options — capsule-aware
    // by construction — with the row's derived suppression folded in, so a posture can never store through a leg
    // it declared closed and a probe adds only the cache-only bit on top of what its row already says.
    static HybridCacheEntryOptions Options(CacheRuntime runtime, CachePolicy policy, HybridCacheEntryFlags probe = HybridCacheEntryFlags.None, TimeSpan? expiration = null) =>
        runtime.Entry(CacheLane.ModelResult) switch {
            var seated => new HybridCacheEntryOptions {
                Expiration = expiration ?? seated.Expiration,
                LocalCacheExpiration = seated.LocalCacheExpiration,
                Flags = (seated.Flags ?? HybridCacheEntryFlags.None) | policy.Flags | probe,
            },
        };
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
