# [COMPUTE_INFERENCE]

`RunOps` folds admitted `OrtValue` runs through one shared session, bracketed native ownership, and vectorized reductions. `BatchGate` coalesces compatible rows, `TilePlan` executes fixed-shape reflective tiles with feathered overlap-add, `StageRun` executes the photo-to-PBR wire, and `CacheOps` projects deterministic results onto `ComputeReceipt.Cache`.

`BoundFlow` allocates through the shared arena and reports per-bucket warm evidence. `ExecutionProvider` resolves the loaded provider, `RunInput.Strings` completes through extension ops, and Persistence owns result custody. `StageRequest` and `StageResult` transcribe the Materials↔Compute wire; opaque keys and content addresses cross injected ports without a strata reference.

## [01]-[INDEX]

- [02]-[INFERENCE_MODES]: every `OrtValue`-only run mode folded over the shared session, one polymorphic input admission feeding the vectorized reductions, the rail-shaped two-leg native bracket, the trace-reading warm pulse that measures a graph's partition census, and the cross-request batching gate.
- [03]-[TILED_INFERENCE]: fixed-bucket tiling with row-owned pad, blend, and layout kernels over one gather-run-scatter fold that binds every product to its own tensor lane, overlap-adds each field of one forward pass into its own plane, reads each grade off its lane, and proves its own coverage.
- [04]-[STAGE_EXECUTION]: photo-to-PBR request and result records bound as LOWERED PRIMITIVES over a grant gate, a per-row producer-extent gate at resolution, a layout-owned channel stack for the full input roster, a single-construction tile plan, a roster-owned provider, precision, and licence projection, lease-side artefact, layout, and latent gates, an executor-synthesized seeded latent draw, and a horizon-bounded capacity-capped decision-keyed floor-provider residual memo that survives a restart through its artifact port and demotes a breach against the card's live residual band.
- [05]-[RESULT_CACHE]: version-stamped deterministic keys and column-driven policy rows — behaviour and per-call suppression alike — over an echo-validated single-flight read-through with model-scoped drift invalidation.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` folds every run mode over the shared session; `RunInput` admits one operand polymorphically on carrier shape through the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; `PlannedRun` owns the `RunOptions` + `Terminate`-latch registration pair; a `BoundFlow` capsule composes the shared-arena device-resident hot path; `BatchGate` the bounded-window cross-request coalescer over one shared session.
- Cases: `Infer` single run; `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip arm; `BoundFlow` the arena-allocated device-resident steady state; `Chunked` streaming windows over a `RecyclableMemoryStream.GetReadOnlySequence` view; `Embed` mean/CLS/last/max-pool text-to-vector; `Classify` softmax top-`k` over the interchange `PointScan` encoding; `ClashScore` scalar clash false-positive scoring over a `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge; `WarmPulse` the profile-and-read fold answering a bucket's partition census; `BatchGate.Submit` the coalesced single-row shape; `InferTiled` the fixed-bucket mosaic over a plane exceeding every admitted shape (`[03]-[TILED_INFERENCE]`). Native async `RunAsync` is the rejected spelling — it demands pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so the lane seam owns the thread hop.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket, and the bracket CONSUMES the admitted inputs: one run is one deterministic release for both native legs, so a repeated same-shape loop rides `BoundFlow`, never re-admitted one-shot inputs.
- Auto: `Plan` folds the `RunConfig` row table into `AddRunConfigEntry` and returns a `PlannedRun` capsule pairing the `RunOptions` with the `Terminate`-latch `CancellationTokenRegistration` off the linked `CancelScope` — the registration disposes with the capsule, so a latch firing into a disposed `RunOptions` is structurally impossible and a bare `Token.Register` whose registration nobody owns is the named use-after-free; `ModelSessions.Faulted` is the single fault oracle for the whole model rail — it classifies by scope provenance into `DeadlineExpired`/`Cancelled` and lifts a non-cancellation native fault to `ModelRejected`, never a raw `Error.New` leaking an unclassified native exception, and it is the capsule's because the load latch it also serves sits below this page; a run-side copy lets one expiring deadline report two faults depending on whether it landed on the lease or the run. Shape reads bind the tensor's own `GetTensorTypeAndShape()` columns — `Shape` for an axis, `ElementCount` for a total — never re-multiplied dimensions. `RunInput` composes the `TensorBridge.Ingress` overloads (the sole dense `OrtValue` C-data factory owner) over the open unmanaged `T`; ONNX-owned strings and preconstructed sparse `OrtValue`s ride distinct cases, with sparse ownership transferring only after `IsSparseTensor` proves the carrier. `Pooling` carries each reduction as its delegate-backed row, and `Embed` admits only an unbatched hidden-state tensor, so a multi-row output never collapses unrelated embeddings into one vector. `Classify` selects each row's top-`k` through a class-stable bounded `PriorityQueue` min-heap (`O(width·log k)`, never a full-taxonomy `Array.Sort`) and materializes per row through an explicit span walk — no `ReadOnlySpan<float>` captured into a lambda, the named kernel exemption. `BatchGate` snapshots each admitted row, caps queued mass at `BatchPolicy.MaxPending`, and packs rows arriving within `MaxDelay` up to `MaxRows` into one bound execution using the injected `TimeProvider` — the bound input stays shaped `[MaxRows, rowWidth]`, a partial window zero-pads its tail rows, and fan-out returns only the submitted rows, because rows are independent under a feed-forward per-row model and a variable-shape rebind per window is the rejected form; per-call ORT dispatch overhead dominates small-tensor inference, so the screening loops the charter names ride one packed run per window instead of thousands of singletons.
- Law: the `Terminate` latch lands at ORT node boundaries, so the cancellation grain is the largest SCHEDULED unit — measured at the pin: a 224-node CPU graph at ~2.9 s wall answers a mid-run latch in 7-10 ms (`OnnxRuntimeException [ErrorCode:Fail] Exiting due to terminate flag being set to true`), while the SAME graph fused into one CoreML MLProgram partition runs to completion through a latch set at 60% of its ~420 ms wall, because a fused partition exposes no interior boundary the latch can land on. Deadline enforcement on an accelerator row therefore budgets the largest fused partition's wall time — the `partitionCount` assertion is what keeps that budget bounded — and a deadline poll finer than node latency buys nothing on the floor row.
- Receipt: `ModelRun` carries model checksum, EP, run mode, batch, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `GetTensorMemoryInfo` allocator name as `ArenaAllocator`, and the optional `Runtime/receipts#BENCHMARK_CLAIMS` `ProfileArtifact.ChromeTrace` profile evidence — content-keyed by the admitted `ArtifactIndexRow`'s `ContentAddress` and stamped with the `InferenceSession.ProfilingStartTimeNs` epoch, never a loose path string; profiling artifacts land as `ArtifactKind.OnnxProfile` rows. Every `BatchGate` window emits ONE `ModelRun` whose `BatchSize` is the window's submitted row count (zero-padded tail rows never count) — per-submitter receipt fan-out is the rejected form.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, System.Text.Json, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload case; a new measured warm fact is one column on the session capsule's `WarmEvidence` filled by one more read inside `WarmPulse`, never a second pulse surface; a new run-config posture is one `RunConfig` row with its `AddRunConfigEntry` pairs and `OrtAllocatorType` arena column; a new pooling shape is one delegate-backed `Pooling` row; a wider classifier candidate set is the `Classify` `top` arity; a batching posture is one `BatchPolicy` value, never a second coalescer; a `ZipMap` sequence-of-maps classifier reads through the `Model/extension#EXTENSION_OPS` `Egress`→`OpOutput.Sequence` reader, never a parallel arm on `Classify` (whose softmax owns the numeric-logit shape only); a BIM point-cloud→element classifier, symbol recognizer, or clash scorer is one more `Classify`/`ClashScore` run over the shared session — consuming the interchange `PointScan` encoding and the `Solver/clash#CLASH_AND_TWIN` `ClashPair` vector — never a BIM-specific service; a tensor-lane handoff already holding a `Tensor<T>` is one `InferTensor` run with zero managed copy; an input plane larger than the session's admitted shape is one `[03]-[TILED_INFERENCE]` `TilePlan` over this same `BoundFlow`, never a free-dimension override that re-plans memory on every extent.
- Boundary: `RunOps` extends `Model/sessions#SESSION_CAPSULE` with bracketed native disposal. `CreateTensorValueFromMemory` binds rented staging without a copy; input ownership transfers at the run, and `Bracket` disposes every admitted input beside the result collection. The bracket's ACQUIRE leg is rail-shaped: the `Tensor/residency#ORT_BRIDGE` bound flow already answers `Fin`, so its refusal rides straight in while direct session calls lift through `Fin.Succ` and the catch arms classify their throws exactly as before — a bare thunk beside a rail-shaped source would force a typed refusal back through an exception just to reach the classifier that would then re-derive it. `InferBound` calls the `OrtValue`-only `RunWithBoundResults` member directly; its named arm zips `GetOutputNames()` against that same collection and never materializes `DisposableNamedOnnxValue`. Every projection proves a nonempty output collection before `First()`. `BoundFlow` binds input and sink from `ModelSessions.SharedAllocator`, and `Pulse` writes through the mutable native span without staging. `Chunked` copies each `ReadOnlySequence<byte>` window into the bound value and emits one terminal `StreamSegment`. `Embed` derives its final axis from output shape and L2-normalizes the pooled vector; `Classify` derives class width, proves row divisibility, and uses bounded top-`k`; `BatchGate` proves packed output cardinality before fan-out. `Profile` admits its artifact through `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)`, grouping the trace under the profiled model checksum, and mints the typed `ProfileArtifact.ChromeTrace` evidence from the admitted row's `ContentAddress` and the `ProfilingStartTimeNs` epoch in the same pass — the index row is custody, the union case is receipt evidence, one identity joining both — while handing the trace BYTES out beside them so a reader wanting the events back takes what admission already read instead of re-reading a file whose path it would need separately; retention derives from `ArtifactKind.Retention`; the admitted trace is a BARE JSON event array — never a `traceEvents`-wrapped object — whose measured node-event key roster the `Profile` member states as fence law, and the `Model/sessions#SESSION_CAPSULE` options fold sets `ProfileOutputPathPrefix` BEFORE `EnableProfiling` because the setter reads the prefix at flip time and discards a later assignment silently. `WarmPulse` composes that pair into the session capsule's injected warm-pulse shape: the caller supplies the bound run for its own bucket, because only the surface that built the flow knows this model's input roster, and the fold owns closing profiling, admitting the trace, and reading the census — `JsonDocument.Parse` over caller-owned memory, `EnumerateArray` over the events, `TryGetProperty` by UTF-8 key on every read, and the document disposed inside the fold so no `JsonElement` view outlives its pooled rental.

```csharp signature
public sealed record RunConfig(FrozenDictionary<string, string> Entries, OrtAllocatorType Arena) {
    public static readonly RunConfig Steady = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.ArenaAllocator);
    public static RunConfig Bulk(string arenaShrinkDevice) => new(new Dictionary<string, string>(StringComparer.Ordinal) {
        ["memory.enable_memory_arena_shrinkage"] = arenaShrinkDevice,
    }.ToFrozenDictionary(StringComparer.Ordinal), OrtAllocatorType.ArenaAllocator);
    public static readonly RunConfig Device = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.DeviceAllocator);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Pooling {
    public static readonly Pooling Mean = new("mean", static (states, hidden) => {
        float[] pooled = new float[hidden];
        int tokens = states.Length / hidden;
        for (int token = 0; token < tokens; token++) { TensorPrimitives.Add(pooled, states.Slice(token * hidden, hidden), pooled); }
        TensorPrimitives.Divide(pooled, tokens, pooled);
        return pooled;
    });
    public static readonly Pooling Cls = new("cls", static (states, hidden) => states[..hidden].ToArray());
    public static readonly Pooling Last = new("last", static (states, hidden) => states[^hidden..].ToArray());
    public static readonly Pooling Max = new("max", static (states, hidden) => {
        float[] pooled = states[..hidden].ToArray();
        for (int token = 1; token < states.Length / hidden; token++) { TensorPrimitives.MaxNumber(pooled, states.Slice(token * hidden, hidden), pooled); }
        return pooled;
    });

    [UseDelegateFromConstructor]
    public partial float[] Apply(ReadOnlySpan<float> states, int hidden);
}

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
            : Fin.Fail<(string Name, OrtValue Value)>(new ComputeFault.ModelRejected($"<sparse-input:{Name}>"));
    }

    public abstract Fin<(string Name, OrtValue Value)> Admit();
}

public sealed record PlannedRun(RunOptions Options, CancellationTokenRegistration Latch) : IDisposable {
    public void Dispose() {
        Latch.Dispose();
        Options.Dispose();
    }
}

[ComplexValueObject]
public sealed partial class BatchPolicy {
    public int MaxRows { get; }

    public int MaxPending { get; }

    public Duration MaxDelay { get; }

    public static readonly BatchPolicy Canonical = Create(maxRows: 16, maxPending: 64, maxDelay: Duration.FromMilliseconds(4));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int maxRows, ref int maxPending, ref Duration maxDelay) =>
        validationError = maxRows > 0 && maxPending >= maxRows && maxDelay >= Duration.Zero
            ? null
            : new ValidationError(message: $"<batch-policy:{maxRows}:{maxPending}:{maxDelay}>");
}

public static partial class RunOps {
    public static PlannedRun Plan(CancelScope scope, RunConfig config, Option<OrtLoraAdapter> lora = default) {
        RunOptions options = new();
        lora.Iter(options.AddActiveLoraAdapter);
        config.Entries.Iter(entry => options.AddRunConfigEntry(entry.Key, entry.Value));
        return new PlannedRun(options, scope.Source.Token.Register(() => options.Terminate = true));
    }

    public static Fin<Seq<(string Name, OrtValue Value)>> Bind(params ReadOnlySpan<RunInput> inputs) {
        Seq<(string Name, OrtValue Value)> admitted = Seq<(string Name, OrtValue Value)>();
        foreach (RunInput input in inputs) {
            Fin<(string Name, OrtValue Value)> next = input.Admit();
            if (next.Case is (string Name, OrtValue Value) row) {
                admitted = admitted.Add(row);
                continue;
            }
            admitted.Iter(static row => row.Value.Dispose());
            return next.Map(_ => admitted);
        }
        return Fin.Succ(admitted);
    }

    extension(InferenceSession session) {
        public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            Bracket(scope, inputs, project, () => Fin.Succ(session.Run(options, inputs.Map(static row => row.Name), inputs.Map(static row => row.Value), outputs)));

        public Fin<T> InferBound<T>(RunOptions options, CancelScope scope, OrtIoBinding binding, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Option<Func<Seq<(string Name, OrtValue Value)>, Fin<T>>> named = default) =>
            Bracket(
                scope,
                Seq<(string Name, OrtValue Value)>(),
                results => named.Case is Func<Seq<(string Name, OrtValue Value)>, Fin<T>> zip
                    ? binding.GetOutputNames() is string[] names && names.Length == results.Count
                        ? zip(toSeq(names).Zip(toSeq(results), static (name, value) => (Name: name, Value: value)))
                        : Fin.Fail<T>(new ComputeFault.ModelRejected("<bound-output-cardinality>"))
                    : project(results),
                () => {
                    binding.SynchronizeBoundInputs();
                    IDisposableReadOnlyCollection<OrtValue> results = session.RunWithBoundResults(options, binding);
                    try {
                        binding.SynchronizeBoundOutputs();
                        return Fin.Succ(results);
                    }
                    catch {
                        results.Dispose();
                        throw;
                    }
                });

        // TRACE SCHEMA (measured at the pin): the emitted profile is a BARE JSON ARRAY — no traceEvents wrapper —
        // one object per line, every event carrying exactly {cat, pid, tid, dur, ts, ph, name, args} with ph "X"
        // and ts/dur in microseconds relative to profiling start. cat is "Session" (args always {}; names
        // model_loading_uri, session_initialization, SequentialExecutor::Execute, model_run) or "Node"; a node
        // event's name is <node>_kernel_time (no _fence_before/_fence_after exists) and its args bag spells
        // op_name, provider, node_index (a STRING decimal), activation_size/parameter_size/output_size (STRING
        // byte counts), input_type_shape/output_type_shape (arrays of one-key {type: [dims]} objects), and
        // thread_scheduling_stats (main_thread + sub_threads keyed by hex thread handle — parse as a map). A
        // graph_index key does not exist. Array order is COMPLETION order (node events precede their enclosing
        // Session spans) — join on ts/dur, never position; read args by key, never position. provider spells the
        // full EP class name (the GetAvailableProviders vocabulary, 1:1 onto the ExecutionProvider rows); a
        // non-CPU EP fuses its subgraph, so op_name becomes the synthesized fusion name (<hash>_CoreML_<hash>_<n>,
        // opaque to per-op-type aggregation) and node_index indexes the POST-PARTITION graph — never a stable
        // model-authoring identity across EP sets. Strip the trailing _kernel_time to recover node identity.
        // The trace BYTES ride out beside the row and the evidence: admission already read them whole, so a reader
        // that wants the events back — the warm pulse's partition census is the one — takes what this member holds
        // instead of re-reading a file whose path it would have to be handed separately.
        public Fin<(ArtifactIndexRow Row, ProfileArtifact Artifact, ReadOnlyMemory<byte> Trace)> Profile(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, Instant at) =>
            !policy.Profiling
                ? Fin.Fail<(ArtifactIndexRow, ProfileArtifact, ReadOnlyMemory<byte>)>(new ComputeFault.ModelRejected("profiling-disabled"))
                : Try.lift(() => session.EndProfiling() is string path
                        ? File.ReadAllBytes(path) switch {
                            var trace => Fin.Succ((
                                Row: ArtifactIndexRow.Admit(ArtifactKind.OnnxProfile, path, trace, classification, at, Some(sourceKey)),
                                Trace: (ReadOnlyMemory<byte>)trace)),
                        }
                        : Fin.Fail<(ArtifactIndexRow Row, ReadOnlyMemory<byte> Trace)>(new ComputeFault.ModelRejected("profiling-path-missing")))
                    .Run()
                    .MapFail(error => new ComputeFault.ModelRejected(error.Message))
                    .Bind(identity)
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
        public Fin<ModelSessions.WarmEvidence> WarmPulse(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, Instant at, Func<Fin<Unit>> run) =>
            run()
                .Bind(_ => session.Profile(policy, sourceKey, classification, at))
                .Bind(static profiled => Try.lift(() => Partitions(profiled.Trace)).Run()
                    .MapFail(static error => new ComputeFault.ModelRejected($"<warm-trace:{error.Message}>")))
                .Map(static partitions => new ModelSessions.WarmEvidence(Some(partitions)));

        // Partition count is the DISTINCT provider set over node events PLUS one per fused node: a non-CPU EP
        // collapses each claimed subgraph into a single synthesized `<hash>_<EP>_<hash>_<n>` op, so counting
        // distinct providers alone reports one partition for a graph an EP cut into several — which is exactly the
        // number the stage's own partition cap gates on. `cat` discriminates node events from the `Session` spans
        // sharing the array, every read goes BY KEY because the array is completion-ordered and the args bag carries
        // no fixed member order, and the document disposes inside this fold so no view outlives its pooled rental.
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

        public Fin<Seq<T>> Chunked<T>(RunOptions options, CancelScope scope, BoundFlow loop, ReadOnlySequence<byte> windows, int windowFloats, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            long frameBytes = (long)windowFloats * sizeof(float);
            long quotient = frameBytes > 0L ? windows.Length / frameBytes : 0L;
            if (frameBytes <= 0L || windows.Length % frameBytes is not 0L || quotient > int.MaxValue) {
                return Fin.Fail<Seq<T>>(new ComputeFault.ModelRejected($"<chunk-shape:{windows.Length}:{windowFloats}>"));
            }
            int frames = (int)quotient;
            return toSeq(Enumerable.Range(0, frames))
                .TraverseM(index => loop.Pulse(options, scope, windows.Slice(index * frameBytes, frameBytes), project)).As();
        }

        public Fin<float[]> Embed(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Pooling pooling) =>
            session.Infer(options, scope, inputs, Seq(output), results => {
                if (results.Count is 0) { return Fin.Fail<float[]>(new ComputeFault.ModelRejected("<embed-output-missing>")); }
                OrtValue value = results.First();
                long[] shape = value.GetTensorTypeAndShape().Shape;
                int hidden = shape.Length > 0 && shape[^1] is > 0 and <= int.MaxValue ? (int)shape[^1] : 0;
                ReadOnlySpan<float> states = value.GetTensorDataAsSpan<float>();
                bool unbatched = shape.Length is 2 || shape.Length is 3 && shape[0] is 1;
                if (!unbatched || hidden is 0 || states.IsEmpty || states.Length % hidden is not 0 || !TensorPrimitives.IsFiniteAll(states)) {
                    return Fin.Fail<float[]>(new ComputeFault.ModelRejected("<embed-shape>"));
                }
                float[] pooled = pooling.Apply(states, hidden);
                float norm = TensorPrimitives.Norm<float>(pooled);
                if (!float.IsFinite(norm) || norm <= 0f) { return Fin.Fail<float[]>(new ComputeFault.ModelRejected("<embed-norm>")); }
                TensorPrimitives.Divide(pooled, norm, pooled);
                return Fin.Succ(pooled);
            });

        public Fin<TResult> InferTensor<T, TResult>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Func<ReadOnlyTensorSpan<T>, Fin<TResult>> project) where T : unmanaged =>
            session.Infer(options, scope, inputs, Seq(output), results =>
                results.Count is 0
                    ? Fin.Fail<TResult>(new ComputeFault.ModelRejected("<tensor-output-missing>"))
                    : project(results.First().GetTensorDataAsTensorSpan<T>()));

        public Fin<Seq<Seq<(int Class, float Probability)>>> Classify(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string logits, int top = 1) =>
            session.Infer(options, scope, inputs, Seq(logits), results => {
                if (results.Count is 0) { return Fin.Fail<Seq<Seq<(int Class, float Probability)>>>(new ComputeFault.ModelRejected("<classify-output-missing>")); }
                OrtValue value = results.First();
                long[] shape = value.GetTensorTypeAndShape().Shape;
                ReadOnlySpan<float> scores = value.GetTensorDataAsSpan<float>();
                int width = shape.Length > 0 && shape[^1] is > 0 and <= int.MaxValue ? (int)shape[^1] : 0;
                if (width is 0 || scores.Length % width is not 0 || top is < 1 || top > width || !TensorPrimitives.IsFiniteAll(scores)) {
                    return Fin.Fail<Seq<Seq<(int Class, float Probability)>>>(new ComputeFault.ModelRejected("<classify-shape>"));
                }
                int rows = scores.Length / width;
                float[] probabilities = new float[rows * width];
                (int Class, float Probability)[][] ranked = new (int Class, float Probability)[rows][];
                for (int row = 0; row < rows; row++) {
                    Span<float> probability = probabilities.AsSpan(row * width, width);
                    TensorPrimitives.SoftMax(scores.Slice(row * width, width), probability);
                    ranked[row] = TopK(probability, top);
                }
                return Fin.Succ(toSeq(ranked).Map(static row => toSeq(row)));
            });

        public Fin<float> ClashScore(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> features, string output) =>
            session.Infer(options, scope, features, Seq(output), static results => {
                if (results.Count is 0) { return Fin.Fail<float>(new ComputeFault.ModelRejected("<clash-output-missing>")); }
                ReadOnlySpan<float> scores = results.First().GetTensorDataAsSpan<float>();
                return scores.Length is not 1 || !float.IsFinite(scores[0])
                    ? Fin.Fail<float>(new ComputeFault.ModelRejected("<clash-score>"))
                    : Fin.Succ(scores[0]);
            });

        public ComputeReceipt.ModelRun RunReceipt(ModelIdentity model, ExecutionProvider ep, string mode, int batch, OrtValue output, CorrelationId correlation, Option<ProfileArtifact> profile, Duration elapsed) =>
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()), output.GetTensorMemoryInfo().Name, profile.ValueUnsafe()) {
                Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
            };

        public Fin<ComputeReceipt.StreamSegment> StreamReceipt(ReadOnlySequence<byte> windows, int windowFloats, string artifactId, CorrelationId correlation, Duration elapsed) {
            long frameBytes = (long)windowFloats * sizeof(float);
            long frames = frameBytes > 0L ? windows.Length / frameBytes : 0L;
            return frameBytes <= 0L || windows.Length % frameBytes is not 0L || frames > int.MaxValue
                ? Fin.Fail<ComputeReceipt.StreamSegment>(new ComputeFault.ModelRejected($"<stream-receipt-shape:{windows.Length}:{windowFloats}>"))
                : Fin.Succ(new ComputeReceipt.StreamSegment(artifactId, (int)frames, windows.Length) {
                    Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
                });
        }
    }

    static (int Class, float Probability)[] TopK(ReadOnlySpan<float> probability, int top) {
        PriorityQueue<int, (float Probability, int ReverseClass)> heap = new(top);
        for (int index = 0; index < probability.Length; index++) {
            (float Probability, int ReverseClass) candidate = (probability[index], -index);
            if (heap.Count < top) { heap.Enqueue(index, candidate); }
            else if (heap.TryPeek(out _, out (float Probability, int ReverseClass) worst) && candidate.CompareTo(worst) > 0) { heap.EnqueueDequeue(index, candidate); }
        }
        int kept = heap.Count;
        (int Class, float Probability)[] ranked = new (int Class, float Probability)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { int cls = heap.Dequeue(); ranked[slot] = (cls, probability[cls]); }
        return ranked;
    }

    // Ownership transfers at the run: the bracket's completion is the ONE deterministic release for admitted inputs
    // and produced results alike. The acquire leg is RAIL-SHAPED because the bound-flow run already answers `Fin` —
    // a bare thunk beside it would force that typed refusal back through an exception to reach this bracket, and the
    // classifier below would then re-derive a fault the flow had already named. Direct session calls throw, so they
    // lift with `Fin.Succ` and the catch arms classify them exactly as before; the results binding seats INSIDE the
    // rail so the `finally` disposes whatever the acquire produced on either exit.
    static Fin<T> Bracket<T>(CancelScope scope, Seq<(string Name, OrtValue Value)> owned, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Func<Fin<IDisposableReadOnlyCollection<OrtValue>>> run) {
        IDisposableReadOnlyCollection<OrtValue>? results = null;
        try {
            return run().Bind(produced => {
                results = produced;
                return project(produced);
            });
        }
        // ONE classifier for both halves of a cancellation: `Model/sessions#SESSION_CAPSULE` owns it because the load
        // latch it also serves sits below this page, so a deadline that expires across a lease and its run cannot
        // report two different faults.
        catch (OnnxRuntimeException error) {
            return Fin.Fail<T>(ModelSessions.Faulted(scope, error));
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or OverflowException) {
            return Fin.Fail<T>(new ComputeFault.ModelRejected(error.Message));
        }
        finally {
            results?.Dispose();
            owned.Iter(static row => row.Value.Dispose());
        }
    }

    extension(BoundFlow flow) {
        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySpan<float> payload, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            return flow.Write(payload).Bind(_ => Bracket(scope, Seq<(string Name, OrtValue Value)>(), project, () => flow.Run(options)));
        }

        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySequence<byte> window, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            return flow.Write(window).Bind(_ => Bracket(scope, Seq<(string Name, OrtValue Value)>(), project, () => flow.Run(options)));
        }
    }
}

public sealed class BatchGate : IAsyncDisposable {
    readonly record struct Pending(float[] Row, TaskCompletionSource<Fin<float[]>> Reply);

    readonly Channel<Pending> queue;
    readonly Task pump;
    readonly CancellationTokenSource stop = new();
    readonly int rowWidth;

    private BatchGate(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, TimeProvider time) {
        this.rowWidth = rowWidth;
        queue = Channel.CreateBounded<Pending>(new BoundedChannelOptions(policy.MaxPending) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });
        pump = Task.Run(() => Pump(flow, options, scope, rowWidth, policy, time), stop.Token);
    }

    public static Fin<BatchGate> Admit(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, TimeProvider time) =>
        guard(rowWidth > 0, new ComputeFault.ModelRejected($"<batch-row-width:{rowWidth}>"))
            .ToFin()
            .Map(_ => new BatchGate(flow, options, scope, rowWidth, policy, time));

    public async ValueTask<Fin<float[]>> Submit(float[] row) {
        if (row.Length != rowWidth || !TensorPrimitives.IsFiniteAll(row)) { return Fin.Fail<float[]>(new ComputeFault.ModelRejected($"<batch-row:{row.Length}:{rowWidth}>")); }
        TaskCompletionSource<Fin<float[]>> reply = new(TaskCreationOptions.RunContinuationsAsynchronously);
        try {
            await queue.Writer.WriteAsync(new Pending(row.ToArray(), reply), stop.Token).ConfigureAwait(false);
            return await reply.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) {
            return Fin.Fail<float[]>(new ComputeFault.Cancelled("<batch-gate-stopped>"));
        }
        catch (ChannelClosedException) {
            return Fin.Fail<float[]>(new ComputeFault.Cancelled("<batch-gate-stopped>"));
        }
    }

    async Task Pump(BoundFlow flow, RunOptions options, CancelScope scope, int rowWidth, BatchPolicy policy, TimeProvider time) {
        List<Pending> window = new(policy.MaxRows);
        try {
            while (await queue.Reader.WaitToReadAsync(stop.Token).ConfigureAwait(false)) {
                window.Clear();
                while (window.Count < policy.MaxRows && queue.Reader.TryRead(out Pending head)) { window.Add(head); }
                if (window.Count < policy.MaxRows) {
                    await Task.Delay(policy.MaxDelay.ToTimeSpan(), time, stop.Token).ConfigureAwait(false);
                    while (window.Count < policy.MaxRows && queue.Reader.TryRead(out Pending tail)) { window.Add(tail); }
                }
                if (window.Count == 0) { continue; }
                // Bind shapes the bound input [MaxRows, rowWidth] once: a partial window zero-pads the tail rows and fans back only the submitted ones.
                float[] packed = new float[policy.MaxRows * rowWidth];
                for (int row = 0; row < window.Count; row++) { window[row].Row.CopyTo(packed, row * rowWidth); }
                Fin<float[][]> outcome = flow.Pulse(options, scope, packed, results => {
                    if (results.Count is 0) { return Fin.Fail<float[][]>(new ComputeFault.ModelRejected("<batch-output-missing>")); }
                    ReadOnlySpan<float> scores = results.First().GetTensorDataAsSpan<float>();
                    if (scores.IsEmpty || scores.Length % policy.MaxRows is not 0 || !TensorPrimitives.IsFiniteAll(scores)) {
                        return Fin.Fail<float[][]>(new ComputeFault.ModelRejected($"<batch-output:{scores.Length}:{policy.MaxRows}>"));
                    }
                    float[][] sliced = new float[window.Count][];
                    int stride = scores.Length / policy.MaxRows;
                    for (int row = 0; row < window.Count; row++) { sliced[row] = scores.Slice(row * stride, stride).ToArray(); }
                    return Fin.Succ(sliced);
                });
                for (int row = 0; row < window.Count; row++) {
                    TaskCompletionSource<Fin<float[]>> reply = window[row].Reply;
                    int index = row;
                    reply.TrySetResult(outcome.Map(rows => rows[index]));
                }
                window.Clear();
            }
        }
        finally {
            queue.Writer.TryComplete();
            Fin<float[]> cancelled = Fin.Fail<float[]>(new ComputeFault.Cancelled("<batch-gate-stopped>"));
            window.ForEach(pending => pending.Reply.TrySetResult(cancelled));
            while (queue.Reader.TryRead(out Pending pending)) { pending.Reply.TrySetResult(cancelled); }
        }
    }

    public async ValueTask DisposeAsync() {
        queue.Writer.TryComplete();
        stop.Cancel();
        try { await pump.ConfigureAwait(false); }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        stop.Dispose();
    }
}
```

## [03]-[TILED_INFERENCE]

- Owner: `TilePlan` `[ComplexValueObject]` owns the whole tiling — source extent, source channels, the ordered `TileProduct` roster binding each product to its graph tensor and lane, bucket edge, overlap, scale, and the three row families that generate the space: `PadMode` folds an out-of-range index back onto the plane, `TileBlend` shapes the overlap taper, and `TileLayout` carries the gather, scatter, and normalize kernel triple for one tensor layout; `TilePlan` itself owns the layout-free weight accumulation every field shares and the `TileTensor` derivation that folds the roster into the session's own output cardinality. `RunOps.InferTiled` is the fold; `TileMosaic` is the assembled field set and grade set with its measured coverage floor.
- Cases: `TileProduct.Plane` a field across the tile and `TileProduct.Measure` a grade over it; `PadMode.Reflect`; `TileBlend` rows `Hann`, `Linear`, `Smoothstep`; `TileLayout` rows `Planar` (`NCHW`) and `Interleaved` (`NHWC`).
- Law: tiles are FIXED-SHAPE. Dynamic input extents re-partition the graph and defeat memory-pattern reuse on every call, so the bound input holds one bucket shape for the whole mosaic and the plane adapts to the bucket rather than the session adapting to the plane. Grids count the first tile whole and step the remainder by the stride, so an extent equal to its bucket is exactly one tile; stepping the whole extent against the stride emits a trailing tile carrying no new texels.
- Law: ONE grid carries EVERY product. Appearance estimators emit base colour, normal, and roughness from a single forward pass, so `TilePlan.Products` scatters all three out of the same tile run and a mosaic costs the grid rather than the grid times the plane count — folding per output re-infers the whole image once for each plane the model already produced.
- Law: a product binds to a TENSOR and a LANE, never to a position. A PACKED export names one tensor for several products, so `TilePlan.Tensors` folds the roster into the distinct tensors in first-appearance order — the order `InferenceSession.OutputNames` carries — and each row takes the channel offset its earlier lanes on that tensor leave. The run then resolves a TENSOR by position and slices each lane out of it, so a graph emitting roughness beside metalness in one `material` tensor lands two planes where a one-tensor-one-product assumption lands the first tensor's bytes twice. Result cardinality proves against the TENSOR count rather than the roster width, and a lane whose element count disagrees with its tensor's declared channel sum refuses.
- Law: a MEASURE is rank-0 and grades ONE tile. A grade owns no arena, no taper, and no accumulation — the lane read IS the value — so a graded tensor's element count is its lane count rather than the tile area, and every lane on one tensor shares one modality. Aggregating N tile grades into one number mints a statistic no model measured and no score row declares a direction for, so a roster carrying a measure admits exactly one window and a source extent past its bucket refuses at the plan, where the specifying end's own bucket roster is what declares a scorer's admissible extent.
- Law: reassembly is OVERLAP-ADD, never last-writer-wins. Each produced tile scatters through its taper weights into its product's accumulation plane, one shared weight plane accumulates the taper mass, and one divide per product closes the mosaic — so an overlap band carries the weighted mean of both estimates rather than a hard seam, and a blend row whose profile does not sum to unity still reconstructs exactly because the divide normalizes what accumulated instead of trusting the profile. `TilePlan.Accumulate` owns that weight plane as pure geometry — one taper mass per texel, free of layout and product — so it runs once per window whatever the roster's width.
- Law: taper applies only where a tile MEETS a neighbour. Four per-axis ramps index by the window's edge mask, so a tile touching the plane border keeps unit weight there; tapering against that border divides the outermost texels by a weight no neighbour ever completes and fades the plane's own edge.
- Entry: `public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source)` on `BoundFlow` — one entry for the whole mosaic, because a per-tile entrypoint pushes the grid, the padding, the taper, and the coverage proof onto every caller; `source` is `ReadOnlyMemory` rather than a span so the scatter closure the run bracket invokes holds the arenas it writes.
- Auto: `TilePlan` derives grid, stride, output extent, bucket key, and the bound input and per-tensor output shapes from its own columns, so a caller states extents and a bucket and never a coordinate — and the binder that seats the flow reads the same shapes the fold runs. Admission rejects a nonpositive extent or channel count, an empty roster, a duplicated role key or duplicated `(Tensor, Lane)` pair, a tensor mixing field lanes with grade lanes, a multi-window grid under a graded roster, an overlap at or past half the shorter bucket edge, and any field whose output element count passes `Array.MaxLength`. It does NOT restate which pad row is legal: the row family is the general tiling vocabulary and the frozen wire pins `reflect` at `StageRequest.Admit`, the boundary that carries it. Gathering stages each tile through the pad row, taking the contiguous row copy whenever the row lies wholly inside the plane and folding per texel only at an edge. Scattering accumulates through `TensorPrimitives.Multiply`/`Add`/`MultiplyAdd` over the per-row weight vector, so reassembly vectorizes rather than walking texels. Coverage proves from the MEASURED weight floor — `TensorPrimitives.Min` over the weight plane — and a floor at or below zero refuses rather than dividing a texel no tile reached. Closing the mosaic partitions by ITEM: `ParallelHelper.ForEach` over an `IRefAction<TilePlane>` hands each worker its own plane where the corpus's index-partitioned `For` rows hand it a slot number, and the products are independent by construction — each divides its own arena by the one shared weight plane and reads nothing another writes.
- Receipt: a mosaic reports as one `ComputeReceipt.ModelRun` whose mode is the tiled key and whose `BatchSize` is `TileMosaic.Tiles`, the count inferred; per-tile and per-product receipt fan-out are the rejected forms for the same reason a `BatchGate` window emits one — the grid ran once.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new seam profile is one `TileBlend` row with its ramp; a new tensor layout is one `TileLayout` row carrying its own gather, scatter, and normalize kernels — never a layout flag branching inside the fold; a model emitting another plane is one more `TileProduct.Plane` row naming its own tensor at lane zero, a model PACKING another plane into a tensor it already names is one more row at that tensor's next lane, and a model grading its input is one `TileProduct.Measure` row — no surface moves for any of the three; a stage that up-samples is the `Scale` column, which threads every field grid without a caller recomputing anything; a pad posture beyond reflection is one `PadMode` row whose `Fold` may answer a negative index for a texel no source covers, which the gather rows already clear.
- Boundary: `InferTiled` composes the `Model/sessions#SESSION_CAPSULE` shared-arena `BoundFlow` and NEVER opens a session — the flow's bound input is the bucket and its bound outputs are the tensor roster, so a mosaic and its session warm-up name the same shapes by construction. Tiles run sequentially through the one bound input because the binding holds a single device-resident staging value; intra-tile parallelism belongs to the session's own thread pool, and the only fold this page partitions itself is the per-product normalize, which touches no binding at all. Every arena is a pooled `MemoryOwner<float>` released on the fold's exit, and the mosaic transfers one accumulation rental per product to the caller, so a failed pulse disposes every plane before the fault leaves.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// One tile's placement: grid coordinate, SOURCE read origin — which may sit past the plane edge, where the pad row
// folds the index back — and the per-axis taper mask, bit 0 for the leading edge and bit 1 for the trailing edge.
public readonly record struct TileWindow(int Column, int Row, int SourceX, int SourceY, int TaperX, int TaperY);

// What ONE roster row binds: the graph's own output tensor, the component LANE inside it, and the opaque role key the
// product publishes under. Tensor and role are DISTINCT — a model names its outputs whatever its author chose — and
// the lane exists because a PACKED export names one tensor for several products, so a graph emitting roughness beside
// metalness in one `material` tensor carries two rows against it and the lane is what separates their bytes. The two
// cases are genuine MODALITIES rather than a plane wearing a small extent: a field is sampled downstream and a grade
// is read, so only the field owns an arena, a taper, and a place in the mosaic. Binding columns live on the BASE and
// the cases pass through — the record-inheritance form, where a case's positional column binds the inherited property
// instead of shadowing it, and where a Switch-derived base member would instead recur through its own accessor — with
// the private constructor reachable from the nested cases alone, which is what seals the family.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileProduct {
    private TileProduct(string tensor, int lane, string role, int channels) =>
        (Tensor, Lane, Role, Channels) = (tensor, lane, role, channels);

    public sealed record Plane(string Tensor, int Lane, string Role, int Channels)
        : TileProduct(Tensor, Lane, Role, Channels);

    // A grade occupies exactly ONE component of its tensor, stated at the arm that knows it rather than as a column a
    // row author could fill wrong — which is what lets the lane-offset derivation fold both modalities through one
    // arithmetic instead of a second roster whose offsets would have to agree with this one.
    public sealed record Measure(string Tensor, int Lane, string Role)
        : TileProduct(Tensor, Lane, Role, channels: 1);

    public string Tensor { get; }

    public int Lane { get; }

    public string Role { get; }

    public int Channels { get; }
}

// One roster row's read window inside its own tensor: the row, its first channel within the packed tensor, and the
// slot it lands in — a plane index for a field, a grade index for a measure. Both counters derive from the roster in
// one pass, so nothing indexes a modality-local array by a roster-wide position.
public readonly record struct TileSlice(TileProduct Product, int Offset, int Slot);

// One produced tensor and every roster row reading it. The distinct-tensor order IS the session's output order, so a
// tensor resolves by position within one session's own results and each lane by its own offset — never by matching a
// declared role against a model's naming.
public readonly record struct TileTensor(string Name, int Channels, Seq<TileSlice> Slices) {
    // One tensor is all fields or all grades: their element counts differ by the whole tile area, so a mixed tensor
    // has no single expected length and the plan refuses it at admission rather than the fold guessing per lane.
    public bool Graded => Slices.Exists(static slice => slice.Product is TileProduct.Measure);

    public long Expected(long area) => Graded ? Channels : area * Channels;
}

// Layout rows own this kernel triple. Every carrier stays a span view, so a custom delegate is the only shape that
// holds them; `row` is scratch the scatter fills with one output row of taper weights. Scatter takes the LANE's own
// channel count, its channel offset inside the produced tensor, and that tensor's whole width, because a mosaic
// writes every field of every packed export through this one kernel.
public delegate void TileGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window);

public delegate void TileScatter(
    ReadOnlySpan<float> tile, Span<float> plane,
    ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
    TilePlan plan, TileWindow window, int channels, int offset, int total);

public delegate void TileNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels);

// Channel-axis stacking for a multi-input stage: one source plane lands at its channel offset inside the one bound
// tensor, in the layout's own placement — a second bound value per input would drift from the warmed bucket shape.
public delegate void TileStack(ReadOnlySpan<float> plane, Span<float> stacked, int channels, int offset, int total, int texels);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PadMode {
    // Reflect mirrors WITHOUT repeating the edge sample — the frozen wire value and the ONNX `reflect` spelling —
    // so a border tile carries no duplicated row, which a convolution would read as a false ridge.
    public static readonly PadMode Reflect = new("reflect", static (index, extent) => {
        if (extent is 1) { return 0; }
        int period = 2 * (extent - 1);
        int folded = Math.Abs(index) % period;
        return folded < extent ? folded : period - folded;
    });
    // Every row answers an in-range index or a NEGATIVE sentinel for a texel no source covers; the gather kernels
    // clear on the sentinel, so a bordering or constant-fill row lands without touching either kernel.
    [UseDelegateFromConstructor]
    public partial int Fold(int index, int extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileBlend {
    // Every row is COMPLEMENTARY — w(t) + w(1-t) = 1 — because the two tapers meeting over one overlap band read
    // mirrored ramp coordinates, so their weights already sum to unity before the normalizing divide runs.
    public static readonly TileBlend Hann = new("hann", static t => 0.5f * (1f - MathF.Cos(MathF.PI * t)));
    public static readonly TileBlend Linear = new("linear", static t => t);
    public static readonly TileBlend Smoothstep = new("smoothstep", static t => t * t * (3f - 2f * t));
    [UseDelegateFromConstructor]
    public partial float Weight(float t);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileLayout {
    public static readonly TileLayout Planar = new(
        "nchw", static (channels, height, width) => [1L, channels, height, width],
        PlanarGather, PlanarScatter, PlanarNormalize, PlanarStack);
    public static readonly TileLayout Interleaved = new(
        "nhwc", static (channels, height, width) => [1L, height, width, channels],
        InterleavedGather, InterleavedScatter, InterleavedNormalize, InterleavedStack);

    // Estate SmartEnum posture: non-key columns seat through the DECLARED private constructor chaining the
    // generated key ctor, never an implicit shape the generator happens to accept.
    private TileLayout(
        string key, Func<int, int, int, long[]> shape,
        TileGather gather, TileScatter scatter, TileNormalize normalize, TileStack stack) : this(key) =>
        (Shape, Gather, Scatter, Normalize, Stack) = (shape, gather, scatter, normalize, stack);

    public Func<int, int, int, long[]> Shape { get; }
    public TileGather Gather { get; }
    public TileScatter Scatter { get; }
    public TileNormalize Normalize { get; }
    public TileStack Stack { get; }

    static void PlanarGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window) {
        bool interior = window.SourceX >= 0 && window.SourceX + plan.TileWidth <= plan.SourceWidth;
        for (int y = 0; y < plan.TileHeight; y++) {
            int sourceY = plan.Pad.Fold(window.SourceY + y, plan.SourceHeight);
            for (int channel = 0; channel < plan.Channels; channel++) {
                Span<float> row = tile.Slice((channel * plan.TileHeight + y) * plan.TileWidth, plan.TileWidth);
                if (sourceY < 0) { row.Clear(); continue; }
                ReadOnlySpan<float> plane = source.Slice(
                    (channel * plan.SourceHeight + sourceY) * plan.SourceWidth, plan.SourceWidth);
                if (interior) { plane.Slice(window.SourceX, plan.TileWidth).CopyTo(row); continue; }
                for (int x = 0; x < plan.TileWidth; x++) {
                    int sourceX = plan.Pad.Fold(window.SourceX + x, plan.SourceWidth);
                    row[x] = sourceX < 0 ? 0f : plane[sourceX];
                }
            }
        }
    }

    // Contiguity along x holds in both the produced tile and the accumulation plane, so one weight vector per row
    // drives a vectorized fused multiply-add per channel. Weight accumulation is NOT here — it is geometry every
    // field shares, so the plan owns it and it runs once per window rather than once per field per window. Channel-
    // major storage addresses a packed lane by its ABSOLUTE channel, so `total` never enters the arithmetic here —
    // the same read the interleaved row's unused scratch `row` takes, one triple over two contiguous axes.
    static void PlanarScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels, int offset, int total) {
        int tileWidth = plan.TileWidth * plan.Scale;
        int tileHeight = plan.TileHeight * plan.Scale;
        int originX = window.SourceX * plan.Scale;
        int originY = window.SourceY * plan.Scale;
        int span = Math.Min(tileWidth, plan.OutputWidth - originX);
        if (span <= 0) { return; }
        for (int y = 0; y < tileHeight; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)plan.OutputHeight) { continue; }
            Span<float> weights = row[..span];
            TensorPrimitives.Multiply(rampX[..span], rampY[y], weights);
            for (int channel = 0; channel < channels; channel++) {
                ReadOnlySpan<float> produced = tile.Slice(((offset + channel) * tileHeight + y) * tileWidth, span);
                Span<float> target = plane.Slice(
                    (channel * plan.OutputHeight + planeY) * plan.OutputWidth + originX, span);
                TensorPrimitives.MultiplyAdd(produced, weights, target, target);
            }
        }
    }

    static void PlanarNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels) {
        for (int channel = 0; channel < channels; channel++) {
            Span<float> band = plane.Slice(channel * weight.Length, weight.Length);
            TensorPrimitives.Divide(band, weight, band);
        }
    }

    static void InterleavedGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window) {
        for (int y = 0; y < plan.TileHeight; y++) {
            int sourceY = plan.Pad.Fold(window.SourceY + y, plan.SourceHeight);
            for (int x = 0; x < plan.TileWidth; x++) {
                int sourceX = plan.Pad.Fold(window.SourceX + x, plan.SourceWidth);
                Span<float> texel = tile.Slice((y * plan.TileWidth + x) * plan.Channels, plan.Channels);
                if (sourceY < 0 || sourceX < 0) { texel.Clear(); continue; }
                source.Slice((sourceY * plan.SourceWidth + sourceX) * plan.Channels, plan.Channels).CopyTo(texel);
            }
        }
    }

    // Channel-interleaved storage strides the x-run, so the fused multiply-add runs across the CHANNEL vector at one
    // texel and the scratch row stays unread — the same triple, a different contiguous axis. A packed lane is a
    // sub-run of that texel's stride, which is why this row reads `total` where the planar row reads neither.
    static void InterleavedScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels, int offset, int total) {
        int tileWidth = plan.TileWidth * plan.Scale;
        int tileHeight = plan.TileHeight * plan.Scale;
        int originX = window.SourceX * plan.Scale;
        int originY = window.SourceY * plan.Scale;
        for (int y = 0; y < tileHeight; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)plan.OutputHeight) { continue; }
            for (int x = 0; x < tileWidth; x++) {
                int planeX = originX + x;
                if ((uint)planeX >= (uint)plan.OutputWidth) { continue; }
                ReadOnlySpan<float> produced = tile.Slice((y * tileWidth + x) * total + offset, channels);
                Span<float> target = plane.Slice((planeY * plan.OutputWidth + planeX) * channels, channels);
                TensorPrimitives.MultiplyAdd(produced, rampX[x] * rampY[y], target, target);
            }
        }
    }

    static void InterleavedNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels) {
        for (int texel = 0; texel < weight.Length; texel++) {
            Span<float> band = plane.Slice(texel * channels, channels);
            TensorPrimitives.Divide(band, weight[texel], band);
        }
    }

    // Planar stacking is a whole-plane copy at the channel offset; the source plane is already channel-major.
    static void PlanarStack(ReadOnlySpan<float> plane, Span<float> stacked, int channels, int offset, int total, int texels) =>
        plane.CopyTo(stacked.Slice(offset * texels, channels * texels));

    // Interleaved stacking seats each texel's channel run at its offset inside the widened texel stride.
    static void InterleavedStack(ReadOnlySpan<float> plane, Span<float> stacked, int channels, int offset, int total, int texels) {
        for (int texel = 0; texel < texels; texel++) {
            plane.Slice(texel * channels, channels).CopyTo(stacked.Slice((texel * total) + offset, channels));
        }
    }
}

// --- [MODELS] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class TilePlan {
    public int SourceWidth { get; }

    public int SourceHeight { get; }

    public int Channels { get; }

    // The lease's own binding roster, ordered by the card's declaration; its DISTINCT tensors are the session's output
    // order and the run's expected result cardinality, which is why a model emitting more planes, packing more planes
    // into a tensor it already names, or grading its input moves no surface here.
    public Seq<TileProduct> Products { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public int Overlap { get; }

    public int Scale { get; }

    // Carried from the leased card, proved once at admission, and never re-read by the fold: the plan is the one
    // place the card's shape authority and the grid geometry meet.
    public TileAdmission Admission { get; }

    public PadMode Pad { get; }

    public TileBlend Blend { get; }

    public TileLayout Layout { get; }

    // Warm-up keys ARE the bucket edge, so a plan and the session shape it needs never spell apart.
    public string Bucket => $"{TileWidth}x{TileHeight}";

    public int StrideX => TileWidth - Overlap;

    public int StrideY => TileHeight - Overlap;

    public int Columns => Steps(SourceWidth, TileWidth, Overlap);

    public int Rows => Steps(SourceHeight, TileHeight, Overlap);

    public int OutputWidth => SourceWidth * Scale;

    public int OutputHeight => SourceHeight * Scale;

    public long[] InputShape => Layout.Shape(Channels, TileHeight, TileWidth);

    // ONE derivation folds the roster into the tensors the session emits: a row joins the tensor it names at the next
    // free channel or opens a new one at offset zero, and its slot counts within its OWN modality. A packed export
    // and a one-product export therefore take exactly one path, and the offsets can never disagree with the roster
    // that produced them.
    public Seq<TileTensor> Tensors =>
        Products.Fold(
            (Roster: Seq<TileTensor>(), Fields: 0, Grades: 0),
            static (state, product) => (
                Roster: Seated(state.Roster, product, product is TileProduct.Measure ? state.Grades : state.Fields),
                Fields: state.Fields + (product is TileProduct.Plane ? 1 : 0),
                Grades: state.Grades + (product is TileProduct.Measure ? 1 : 0)))
            .Roster;

    // The mosaic's two collections size and order off these — one accumulation plane per field, one slot per grade —
    // so the arena roster and the slot roster are one derivation each rather than a count spelled beside a filter.
    public Seq<TileProduct.Plane> Fields =>
        Products.Choose(static product => product is TileProduct.Plane field ? Some(field) : Option<TileProduct.Plane>.None);

    public Seq<TileProduct.Measure> Scorers =>
        Products.Choose(static product => product is TileProduct.Measure grade ? Some(grade) : Option<TileProduct.Measure>.None);

    // Binders seat every bound value from the plan, so shapes the flow holds and shapes the fold writes cannot
    // drift: any bound output sized elsewhere is a second derivation of one grid. A GRADED tensor has no geometry to
    // derive — its element count is its lane count and its declared shape is the graph's own — so the plan answers
    // absence and the binder seats what `InferenceSession.OutputMetadata` declares rather than a rank this end invented.
    public Option<long[]> OutputShape(TileTensor tensor) =>
        tensor.Graded ? None : Some(Layout.Shape(tensor.Channels, TileHeight * Scale, TileWidth * Scale));

    public Seq<TileWindow> Windows {
        get {
            int columns = Columns;
            int rows = Rows;
            return toSeq(Enumerable.Range(0, rows).SelectMany(row => Enumerable.Range(0, columns).Select(column =>
                new TileWindow(
                    column, row, column * StrideX, row * StrideY,
                    TaperX: (column > 0 ? 1 : 0) | (column < columns - 1 ? 2 : 0),
                    TaperY: (row > 0 ? 1 : 0) | (row < rows - 1 ? 2 : 0)))));
        }
    }

    // Four taper variants per axis indexed by the window's own edge mask; index 0 is the all-unit ramp a single-tile
    // axis and both plane borders read.
    public float[][] Ramps(int span, int taper) {
        float[][] table = new float[4][];
        for (int mask = 0; mask < 4; mask++) {
            float[] ramp = new float[span];
            Array.Fill(ramp, 1f);
            for (int index = 0; taper > 0 && index < taper; index++) {
                float weight = Blend.Weight((index + 0.5f) / taper);
                if ((mask & 1) is not 0) { ramp[index] = weight; }
                if ((mask & 2) is not 0) { ramp[span - 1 - index] = weight; }
            }
            table[mask] = ramp;
        }
        return table;
    }

    // Taper mass is geometry, not content: one plane serves every product, so it accumulates once per window
    // however many planes the model emits, and one weight read then normalizes them all identically.
    public void Accumulate(
        Span<float> weight, ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row, TileWindow window) {
        int originX = window.SourceX * Scale;
        int originY = window.SourceY * Scale;
        int span = Math.Min(TileWidth * Scale, OutputWidth - originX);
        if (span <= 0) { return; }
        for (int y = 0; y < TileHeight * Scale; y++) {
            int planeY = originY + y;
            if ((uint)planeY >= (uint)OutputHeight) { continue; }
            Span<float> weights = row[..span];
            TensorPrimitives.Multiply(rampX[..span], rampY[y], weights);
            Span<float> covered = weight.Slice(planeY * OutputWidth + originX, span);
            TensorPrimitives.Add(covered, weights, covered);
        }
    }

    static int Steps(int extent, int tile, int overlap) =>
        extent <= tile ? 1 : 1 + (int)Math.Ceiling((double)(extent - tile) / (tile - overlap));

    static Seq<TileTensor> Seated(Seq<TileTensor> roster, TileProduct product, int slot) =>
        roster.Exists(tensor => StringComparer.Ordinal.Equals(tensor.Name, product.Tensor))
            ? roster.Map(tensor => StringComparer.Ordinal.Equals(tensor.Name, product.Tensor)
                ? tensor with {
                    Channels = tensor.Channels + product.Channels,
                    Slices = tensor.Slices.Add(new TileSlice(product, tensor.Channels, slot)),
                }
                : tensor)
            : roster.Add(new TileTensor(product.Tensor, product.Channels, Seq(new TileSlice(product, 0, slot))));

    // Which pad row is legal is NOT settled here: this owner is the general tiling vocabulary, and the frozen stage
    // wire pins `reflect` at `StageRequest.Admit`, so restating the pin would make one law answerable twice. The
    // BUCKET roster and the OVERLAP band follow the same law and for the same reason: a Materials model card
    // declaring a 1024 bucket, or a wider seam its estimator's receptive field needs, is a row at the specifying
    // end — mirroring either here turns every admitted model into a Compute edit, the exact defect this folder's
    // own no-mirrored-roster ruling names. `Admission` carries them from the leased card and this validator proves
    // only what a plan can prove alone: positivity, divisibility, seam containment, and the addressable ceiling.
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int sourceWidth, ref int sourceHeight, ref int channels, ref Seq<TileProduct> products,
        ref int tileWidth, ref int tileHeight, ref int overlap, ref int scale,
        ref TileAdmission admission, ref PadMode pad, ref TileBlend blend, ref TileLayout layout) {
        // Roster predicates read LOCAL copies: the validation seam takes every argument by `ref` for normalization
        // and a lambda cannot close over a `ref` parameter, so a per-product fold reaches its bounds only through
        // locals lifted first.
        int slots = products.Count;
        Seq<TileProduct> roster = products;
        int extentX = sourceWidth;
        int extentY = sourceHeight;
        int factor = scale;
        TileAdmission admitted = admission;
        validationError = sourceWidth > 0 && sourceHeight > 0 && channels > 0
            && slots > 0
            && roster.ForAll(static product => product.Tensor.Length > 0 && product.Role.Length > 0
                && product.Lane >= 0 && product.Channels > 0)
            // ROLE keys name what the fold publishes and (Tensor, Lane) pairs name what it reads, so both spaces are
            // injective or one product's bytes land under another's name.
            && roster.Map(static product => product.Role).ToFrozenSet(StringComparer.Ordinal).Count == slots
            && roster.Map(static product => $"{product.Tensor}#{product.Lane}").ToFrozenSet(StringComparer.Ordinal).Count == slots
            // One tensor is all fields or all grades: their element counts differ by the whole tile area, so a mixed
            // tensor carries no single expected length the fold could prove a run against.
            && roster.Map(static product => product.Tensor).ToFrozenSet(StringComparer.Ordinal).All(tensor =>
                roster.Filter(product => StringComparer.Ordinal.Equals(product.Tensor, tensor)) is var lanes
                && (lanes.ForAll(static lane => lane is TileProduct.Plane) || lanes.ForAll(static lane => lane is TileProduct.Measure)))
            // A GRADE is a property of one tile, and no score row declares a direction an aggregate could reduce
            // along, so a graded roster admits exactly one window and a wider extent refuses where the plan is built.
            && (!roster.Exists(static product => product is TileProduct.Measure)
                || (Steps(extentX, tileWidth, overlap) is 1 && Steps(extentY, tileHeight, overlap) is 1))
            // Bucket edges and the seam band come off the leased CARD's own admission; the axes decouple, so a
            // 256x512 request is legal wherever the card lists both edges.
            && admitted.Admits(tileWidth, tileHeight, overlap) && scale > 0
            && overlap * 2 < Math.Min(tileWidth, tileHeight)
            && roster.ForAll(product =>
                (long)extentX * extentY * factor * factor * product.Channels <= Array.MaxLength)
            ? null
            : new ValidationError(
                message: $"<tile-plan:{sourceWidth}x{sourceHeight}:{tileWidth}x{tileHeight}:{overlap}:{scale}:{slots}>");
    }
}

// Admitted bucket edges and the seam band a leased model card publishes carry onto the plan rather than being
// mirrored as literals. Cards admitting a 1024 bucket or a 64-texel seam move NO surface here; an empty roster
// admits every positive edge, which is the honest read for a plan built outside a card's authority.
public readonly record struct TileAdmission(Seq<int> Edges, int MinOverlap, int MaxOverlap) {
    public static readonly TileAdmission Unbounded = new(Seq<int>(), 1, int.MaxValue);

    public bool Admits(int tileWidth, int tileHeight, int overlap) =>
        tileWidth > 0 && tileHeight > 0
        && (Edges.IsEmpty || (Edges.Contains(tileWidth) && Edges.Contains(tileHeight)))
        && overlap >= MinOverlap && overlap <= MaxOverlap;
}

// One assembled field plane and the roster row that placed it — role and component count read off the row rather
// than copied beside it, so the binding the fold scattered under is the binding the writer publishes under.
public sealed record TilePlane(TileProduct.Plane Product, MemoryOwner<float> Plane);

// One graded product. A grade owns no arena, so it travels by value and its release is nothing.
public readonly record struct TileGrade(string Role, float Value);

// Assembled field set beside the grade set; owning every rental makes the mosaic the one release point, so a caller
// that encodes and drops it returns each arena and a faulted fold disposes them all before the fault leaves.
public sealed class TileMosaic : IDisposable {
    internal TileMosaic(Seq<TilePlane> planes, Seq<TileGrade> grades, TilePlan plan, int tiles, float coverage) =>
        (Planes, Grades, Plan, Tiles, Coverage) = (planes, grades, plan, tiles, coverage);

    public Seq<TilePlane> Planes { get; }
    public Seq<TileGrade> Grades { get; }
    public TilePlan Plan { get; }
    public int Tiles { get; }
    public float Coverage { get; }

    public void Dispose() => Planes.Iter(static produced => produced.Plane.Dispose());
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// Mosaic closing is this page's ONE item-partitioned fold. Every other parallel row in the corpus partitions an
// INDEX range over an `IAction`, right for a coordinate fold; products partition by ITEM instead — each plane
// divides its own arena by the one shared weight plane and reads nothing another plane writes — so `ForEach` hands
// each worker its own `ref TilePlane` where an index fold would hand it a slot number into a captured array. Both
// rentals outlive the fold, so the action holds `ReadOnlyMemory` and slices inside the worker rather than closing
// over a span it could not carry. One action is one whole-plane divide, so the per-thread floor is one item.
readonly struct NormalizeProduct(TileLayout layout, ReadOnlyMemory<float> weight) : IRefAction<TilePlane> {
    public void Invoke(ref TilePlane produced) =>
        layout.Normalize(produced.Plane.Span, weight.Span, produced.Product.Channels);
}

public static partial class RunOps {
    extension(BoundFlow flow) {
        public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source) {
            if (source.Length != (long)plan.Channels * plan.SourceWidth * plan.SourceHeight) {
                return Fin.Fail<TileMosaic>(new ComputeFault.ModelRejected($"<tile-source:{source.Length}>"));
            }
            int texels = plan.OutputWidth * plan.OutputHeight;
            // Materialized once: the scatter closure indexes by slice slot, and an array indexer is the only
            // positional read a lambda can hold without forcing a span the closure cannot capture. Grades ride the
            // same shape one rank down — one slot each, no arena, filled by a lane read rather than a scatter.
            TilePlane[] planes = plan.Fields
                .Map(field => new TilePlane(field, MemoryOwner<float>.Allocate(texels * field.Channels, AllocationMode.Clear)))
                .ToArray();
            Seq<TileProduct.Measure> scorers = plan.Scorers;
            float[] grades = new float[scorers.Count];
            Seq<TileTensor> tensors = plan.Tensors;
            using MemoryOwner<float> weight = MemoryOwner<float>.Allocate(texels, AllocationMode.Clear);
            using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(plan.Channels * plan.TileHeight * plan.TileWidth);
            using MemoryOwner<float> row = MemoryOwner<float>.Allocate(plan.TileWidth * plan.Scale);
            float[][] rampX = plan.Ramps(plan.TileWidth * plan.Scale, plan.Overlap * plan.Scale);
            float[][] rampY = plan.Ramps(plan.TileHeight * plan.Scale, plan.Overlap * plan.Scale);
            long area = (long)plan.TileHeight * plan.Scale * plan.TileWidth * plan.Scale;
            int emitted = 0;
            foreach (TileWindow window in plan.Windows) {
                plan.Layout.Gather(source.Span, tile.Span, plan, window);
                // ONE run per window feeds EVERY product: results arrive in the session's own output order, matching
                // the DISTINCT-tensor order, so a tensor resolves by position inside one session's results and each
                // lane by the offset the roster derived — never by matching a model's tensor name against a role
                // some other end declared, and never by assuming one tensor carries one product.
                Fin<Unit> pulsed = flow.Pulse(options, scope, tile.Span, results => {
                    if (results.Count != tensors.Count) {
                        return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<tile-tensors:{results.Count}:{tensors.Count}>"));
                    }
                    int slot = 0;
                    foreach (OrtValue value in results) {
                        TileTensor tensor = tensors[slot];
                        ReadOnlySpan<float> produced = value.GetTensorDataAsSpan<float>();
                        if (produced.Length != tensor.Expected(area) || !TensorPrimitives.IsFiniteAll(produced)) {
                            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<tile-output:{tensor.Name}:{produced.Length}>"));
                        }
                        foreach (TileSlice slice in tensor.Slices) {
                            if (slice.Product is TileProduct.Measure) { grades[slice.Slot] = produced[slice.Offset]; continue; }
                            plan.Layout.Scatter(
                                produced, planes[slice.Slot].Plane.Span,
                                rampX[window.TaperX], rampY[window.TaperY], row.Span, plan, window,
                                slice.Product.Channels, slice.Offset, tensor.Channels);
                        }
                        slot++;
                    }
                    return Fin.Succ(unit);
                });
                if (pulsed.Case is Error fault) { return Strand<TileMosaic>(planes, fault); }
                plan.Accumulate(weight.Span, rampX[window.TaperX], rampY[window.TaperY], row.Span, window);
                emitted++;
            }
            float coverage = TensorPrimitives.Min<float>(weight.Span);
            if (coverage <= 0f) {
                return Strand<TileMosaic>(planes, new ComputeFault.ModelRejected($"<tile-coverage:{coverage}>"));
            }
            ParallelHelper.ForEach<TilePlane, NormalizeProduct>(
                planes.AsMemory(), new NormalizeProduct(plan.Layout, weight.Memory), minimumActionsPerThread: 1);
            return Fin.Succ(new TileMosaic(
                toSeq(planes), scorers.Map((grade, index) => new TileGrade(grade.Role, grades[index])),
                plan, emitted, coverage));
        }

        // FIRST window's raw output on the FIRST tensor, the deterministic canary two providers compare on: one
        // tile bounds the parity cost at two runs whatever the mosaic's tile count, and one tensor bounds it
        // whatever the roster's width — a residual hides in no plane a shared graph produced in the same pass, and a
        // graded tensor's one-element array makes the residual fold's max magnitude the scalar absolute difference.
        public Fin<float[]> Canary(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source) {
            using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(plan.Channels * plan.TileHeight * plan.TileWidth);
            // Origin window, no taper: the canary compares raw model output and scatters nowhere.
            plan.Layout.Gather(source.Span, tile.Span, plan, new TileWindow(0, 0, 0, 0, 0, 0));
            return flow.Pulse(options, scope, tile.Span, static results =>
                results.Count is 0
                    ? Fin.Fail<float[]>(new ComputeFault.ModelRejected("<canary-output-missing>"))
                    : Fin.Succ(results.First().GetTensorDataAsSpan<float>().ToArray()));
        }
    }

    // Partly-built mosaics never escape: every plane already allocated returns to the pool before the fault leaves,
    // so an abandoned grid strands no arena.
    static Fin<T> Strand<T>(TilePlane[] planes, Error fault) {
        foreach (TilePlane held in planes) { held.Plane.Dispose(); }
        return Fin.Fail<T>(fault);
    }
}
```

## [04]-[STAGE_EXECUTION]

- Owner: `StageRun` folds a dependency-ordered request sequence into results; `StageRequest`/`StageInput`/`StageOutput`/`StageScore`/`StageResult` transcribe the frozen wire records and `StageProduct` carries one grid's own field set beside its grade set; `LicenseClass` enforces the grant vocabulary; `ResidualBand` carries the card's parity envelope and `LatentInput`/`LatentDraw` its declared and synthesized seed tensor; `PlaneStack` folds the channel sum once and memoizes the stacked bound tensor per layout row across every lease one stage takes; `ParityVerdict` carries one measured residual beside the instant it was measured at; `StagePorts` carries the plane read, plane write, output-description, session-open, and parity-custody legs the app root binds.
- Law: EVERY input row binds or the request refuses. Frozen-wire records carry one `StageInput` row per consumed product in the card's own binding order, and the executor resolves them ALL — a chained row against its producer's held output, an empty-stage row against its blob key — then STACKS the planes along the channel axis in that order into the one bound tensor, the session's own `InputMetadata` channel width proving the sum. Head-taking that silently drops `inputs[1..]` runs the `svbrdf` card without the photograph its estimator consumes, and nothing rails.
- Law: a chained input RE-ENTERS through the port and never bypasses it. Producer planes leave through `StagePorts.Write` at the transfer and format `Describe` chose, and only the host knows whether that crossing is lossless, so binding a retained float plane in place of the bytes the wire published runs one plan two numeric ways depending on whether the fold still holds the producer's rental — same request, same seed, different answer. Device-resident handoff is unreachable here for a second and independent reason: `InferTiled` overlap-adds every field into pooled HOST planes and `TileMosaic` owns those rentals, so no producer `OrtValue` survives a grid for a consumer to bind, and this fold reaches no `SessionPlacement` readback to compare residency with. Device-to-device copies belong where a bound output stays resident — one `Tensor/residency#ORT_BRIDGE` relay over a `BoundFlow` pair — never on this fold.
- Law: Materials SPECIFIES and Compute EXECUTES. Stage, model-card, and role identities cross as OPAQUE KEYS and this end dispatches on none of them, so admitting a model, a stage, or an intermediate at the specifying end moves no surface here; a mirrored stage roster makes every new model a Compute edit and breaks the row-growth law the wire exists to hold.
- Law: the ARTEFACT pins at the lease, not at the far end. `StageRequest.Artefact` carries the weight digest the model card declared and `StageSession.Artefact` the digest the lease loaded, so the ONE seam every lease crosses proves them equal before a grid runs — comparing only where the result lands pays a whole mosaic, and worse, grades a parity residual against weights nobody asked for and seats that verdict in the memo. `StageResult.Artefact` then reports the MEASURED value rather than echoing the request, which is what lets the specifying end's card gate prove an observation instead of trusting a round trip. No decode-time predicate restates the pin: a request naming no digest, or a digest no session holds, refuses at that one gate, and a second spelling of one law is the defect this folder's own compare-never ruling names.
- Law: this end binds LOWERED PRIMITIVES alone. Specifying ends author the wire in their own types; the strata forbid naming one of them here, so every column lands as the value the codec wrote — an enum as its roster string resolved through the roster THIS package owns, a content address as its hex32 string, a correlation key as a string echoed verbatim, an extent as the `int` every tile derivation and span index downstream already runs in (the wire's `uint32` widens losslessly and a negative never crosses). Opaque-key erasure is the deliberate consumer shape: a resolution that fails REFUSES rather than degrading, so a licence spelling this roster cannot honour never runs under a typo, and re-minting a rich value from a key is the drift a second vocabulary opens.
- Law: the EXECUTOR synthesizes the latent; nothing upstream produces it. `StageSession.Latent` carries the card's own declaration — the graph's second input tensor, the channel depth of the draw, and the factor its extent divides the tile by — and this end mints the standard-normal tensor from the request's `Seed` at session bind, because a diffusion export cannot bake its latent into an initializer without freezing every pass to one draw. The draw is a pure function of `(seed, index)` over a self-contained mixer: a framework generator's stream is a runtime implementation detail, so a receipt claiming replay against one holds only until the runtime re-tunes it.
- Law: SEED and LATENT are one joint discriminant, and both mismatches REFUSE. The specifying end zeroes the seed on a deterministic card, so a latent-declaring card arriving at the zero sentinel and a nonzero seed arriving at a card declaring no latent are the two halves of one contradiction — a request whose draw nothing synthesizes and a replay column the executor silently drops. One pattern over the pair refuses both; a graph that binds no latent runs at any seed it was never handed.
- Law: the LEASE binds tensor lanes to ROLE keys. Requests carry no output roster; the leased session reports one `TileProduct` row per card binding — the graph's own output tensor, the component lane inside it, and the opaque role key the product publishes under — so a PACKED export naming one tensor for several products lands each lane under its own role, and the executor never reads a role off a tensor's name. Ordinal correspondence between model outputs and declared roles is what that binding replaces: it bound two independently versioned rosters by position and had no spelling at all for a tensor carrying two products.
- Law: a tiling plan has ONE construction. `StageRequest.Plan` folds the request's own extent, bucket, overlap, pad, and derived scale columns into `TilePlan`, whose validator is the only place the fixed-bucket law is spelled, and that same value then seats the bound flow and drives the fold — so a plan built once cannot disagree with itself, and neither a request-side predicate restating the law nor a compare catching two spellings drift apart has anything left to do.
- Law: `Scale` DERIVES from the extents, never a column. Wire records thread both extents while a stage publishes `inputWidth × scale`, so a carried scale only ever contradicts them; `StageRequest.Scale` answers `None` for a fractional or anisotropic ratio and admission refuses there rather than at a bind reporting a shape mismatch it cannot explain.
- Law: providers are a PREFERENCE and the floor is guaranteed; PRECISION is neither. `ExecutionProvider.FromWire` degrades an unrostered or unloaded spelling to `Floor` and `ProviderUsed` reports what ran, so the substitution is visible in the result. Precision has no such report column, so `ModelPrecision.FromWire` refuses an unrostered spelling and the request never admits — an fp16 request silently running fp32 is the substitution the `CoreMl` `ModelFormat` pin exists to foreclose. `StagePorts.Lease` then TAKES the resolved precision: admitting a precision column and leasing without it is the same defect wearing an argument.
- Law: parity measures the CANDIDATE against a FULL-precision floor, ONCE per acceleration decision, BEFORE the grid runs. `Assured` runs the canary tile on the requested provider and on `ExecutionProvider.Floor` at `ModelPrecision.Full` — `cpu` is the floor row's REPORT key, the spelling a reader discriminates on, never the lease selector — so the residual grades the whole decision — provider and precision together — rather than comparing two runs that already agreed to lower precision. Residuals are a property of `(card, provider, precision, runtime, host)`: the canary grades the graph an EP compiled at a precision on one machine, so the first request measures and every later one reads that verdict beside the card's own band. Measuring per request prices two extra leases, two extra flows, and two extra runs on every stage of every plan for a verdict that cannot move between them — and a verdict living only in process memory re-prices exactly that on every cold app root, so the memo's durable half rides `ParityPort` under the same key and a restart READS what the last process measured. Both probes run at the REQUEST'S seed — comparing two stochastic draws grades noise, not the provider.
- Law: the residual GATES, never merely reports, and the BAND rides the lease rather than the memo. The card's `ResidualBand` reaches `StageSession` at every lease, the run's own lease grades the measured delta against the band's `Upper`, and a breach DEMOTES to the floor at full precision — one demotion, `ProviderUsed` reporting the substitution, `GoldenDelta` keeping the measured breach — so an accelerated run outside its card's envelope never publishes as if it were inside. The band's `Lower` is the DECLARED not-a-point state and never a gate term: a deterministic card diverges on the provider axis alone and its floor is its ceiling, while a stochastic card's band spans a seed sweep this end never runs, so an absent floor states that the sweep has not measured rather than weakening the comparison. Freezing the band into the verdict at measurement time is the rejected form: a card that widens its envelope would keep demoting against the old one until something re-measured a residual that had not changed. On the floor at full precision the delta is 0 by IDENTITY, not by measurement: `providerUsed == cpu` at `fp32` names the discrimination — every accelerated run carries a memoized measurement, the floor carries the identity, and no zero reads as an unmeasured observation.
- Law: a parity verdict EXPIRES. `ParityPort.Horizon` bounds every hit at both tiers: the key names the card, the provider result key, and the host fingerprint, but not the driver or firmware stack the silicon runs under — and a driver revision moves a residual without moving one term of that key. Age is therefore the only honest retirement, a verdict past the horizon reads as ABSENT and re-measures, and the canonical thirty days is the composing root's value rather than a constant this fold reads.
- Law: a GRADE leaves as a value, never as a blob. A `Measure` product is rank-0 — no content address, no plane write, no mosaic arena — so it crosses on `StageResult.Scores` beside the plane outputs and never enters the produced-output map, because nothing downstream samples a grade and a stage binding one would be binding a number as a tensor. Writing four bytes through the plane port to hand back one float is the rejected form: it mints a blob the specifying end must fetch to read a value the result already holds.
- Law: every input row of one stage shares ONE extent. The request declares `InputWidth`/`InputHeight` and every consumed plane matches it — a chained row proves against its producer's published extent at RESOLUTION, before the blob read and before the lease, and a source row proves against its own bytes at read-back — because the channel stack lays every plane into one bound tensor over one texel count, so a second extent has nowhere to go. Both refusals carry `<stage-extent-mismatch>` or its chained sibling and name both extents, where a bound session's shape fault three ports later names neither stage that disagreed.
- Law: the LAYOUT column crosschecks at the lease. The wire carries the producer graph's dimension-order row key so a binder can seat tensors before a lease returns, and the lease reports the layout the model actually emits, so the two prove equal at the one seam that holds both — a column the wire carries and nothing compares is a claim rather than a contract, and the disagreement it hides surfaces as a shape fault naming a port instead of an end.
- Law: evidence publishes MEASURED or refuses. `PartitionCount` reads the per-bucket warm evidence the session capsule measured once, never a zero standing in for an unmeasured run; a request whose bucket carries no partition measurement refuses rather than minting a result whose evidence column reads as observed. Registration seats that bucket with an ABSENT count and only the trace-reading `RunOps.WarmPulse` fills it, so the two surfaces divide cleanly: the composition registers the shapes it will run, and the pulse measures how the graph partitioned for each.
- Entry: `public static Fin<Seq<StageResult>> Fold(Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, IClock clock, TimeProvider time)` — one entry for the whole plan, because per-request entry pushes producer-output resolution onto the caller and re-opens the chained-stage defect where every stage reads the source photograph.
- Auto: `Fold` threads a produced-OUTPUT map so a binding naming a producer resolves against results already held, and every chained input's EXTENT proves against its producer's published output extent AT RESOLUTION — before the blob read and before the lease — because a chained disagreement is a PLAN defect the frozen wire refuses at admit, never a shape mismatch a bound session reports three ports later. Only an empty producer key reads a source plane, whose declared extent the read-back guard then proves against the bytes. `Admit` refuses an unrostered or ungranted licence, an unrostered precision, a bucket spelling disagreeing with its own tile columns, a pad key off the frozen `reflect` pin, and a non-integral scale. `Leased` proves the session's own artefact digest against the request's before any plan builds, so every lease the stage takes — production run and both parity probes — crosses one pin. `Execute` resolves the provider once against the frozen census, resolves and reads EVERY input row, runs `Assured` (the horizon-gated memoized parity measurement), then leases at that decision, proves the artefact digest and the layout row, proves the stacked channel sum against the session's own declared input width, grades the measured delta against the card's LIVE band, builds the plan against the session's binding roster, refuses a seed and a latent that contradict each other, synthesizes the draw from that seed, opens ONE bound flow at that plan and that draw, runs the grid once, writes every produced plane through the port, carries every grade out by value, and folds elapsed time from the injected `TimeProvider`. A breach answers `None` inside that lease and the run re-leases once at the floor, so the demotion costs exactly one extra lease and only on the runs that earned it. The warm pulse injects measured partitions; the run asserts that count against the model-card cap before publication.
- Receipt: each executed stage emits one `ComputeReceipt.ModelRun` with the tiled mode key and the mosaic's tile count as `BatchSize` — one grid ran, so one receipt mints whatever the roster's width; the stage-level evidence rides `StageResult` across the wire, never a second receipt case, because the specifying end owns the admission that reads it.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new stage column is one record field here landing in the same change as its `Runtime/wire#CONTRACT_EVOLUTION` `StageCrossing` slot row, whose arity probe then forces the pair; a new grant posture is one `LicenseClass` row; a further parity axis is one term folded into `ParityKey`, which re-keys both the process memo and the durable row in one edit because they read one derivation; a new wire column is one record field transcribed from the frozen roster at both ends in one change; a further execution backend is one `Model/providers#EP_AXIS` row declaring one `WireKey`, never a translation table here and never a second stage owner; a stage emitting more products is more `TileProduct` rows the lease reports, a stage PACKING two products into one tensor is one more row at that tensor's next lane, a stage GRADING its input is one `TileProduct.Measure` row landing on `StageResult.Scores`, and a stage CONSUMING more products is one more wire input row widening the channel stack — no surface move on any of them.
- Boundary: `StagePorts` is the ONLY route to a plane and the only route to durable parity custody. Compute holds no blob store, no artifact index, no codec, and no channel vocabulary — it reads and writes float planes and parity verdicts through injected legs the app root binds against the Persistence object and artifact lanes, exactly as `Model/sessions#SESSION_CAPSULE` binds its warm-artifact leg; the parity legs carry no rail outward because the root that owns the artifact write also owns the evidence cell its refusal parks on, and a read answering nothing degrades to the cold measurement the process memo already prices. Provider and precision spellings resolve at `Model/providers#EP_AXIS`, whose rows carry their own wire keys, so this record holds no translation table and a roster landing there crosses without an edit here. `StageSession` carries the model-derived facts a request cannot know — the tensor-and-lane binding roster, the tensor layout, the blend profile, the warm evidence, the bound input's own channel width, the card's parity band, and the latent the graph declares — and its `Flow` leg takes the built plan and the synthesized draw, so the bound shapes, the bound draw, and the fold's shapes have one source and the root binds bytes rather than re-deriving a distribution. This wire mints no `tests/contracts/MANIFEST.md` entry — it never leaves the C# runtime.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// This roster and the Materials registry's licence table are BOTH transcriptions of the frozen [07.1][03] five-key
// vocabulary — cross-branch equality tests the wire key, the strata forbid sharing the type, and each end carries
// only the columns its own dispatch reads (a grant verdict here, an admission rank there), and a merged shape is
// exactly the strata reference this wire exists to avoid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicenseClass {
    public static readonly LicenseClass Permissive = new("permissive", grants: true);
    public static readonly LicenseClass Copyleft = new("copyleft", grants: true);
    public static readonly LicenseClass OpenRail = new("openRail", grants: true);
    public static readonly LicenseClass Research = new("research", grants: true);
    // Silent-licence models reach the registry as this row and carry no grant to run; the executing end re-checks
    // it because a grant enforced only where the request is built trusts the caller's word.
    public static readonly LicenseClass Blocked = new("blocked", grants: false);

    private LicenseClass(string key, bool grants) : this(key) => Grants = grants;

    public bool Grants { get; }

    // Wire records carry the roster STRING and this roster owns the resolution, so a spelling no row claims REFUSES
    // rather than degrading — unlike a provider, a grant has no report column a caller could read a substitution
    // off, so a defaulted licence would run an unknown model under a typo.
    public static Option<LicenseClass> FromWire(string wire) => TryGet(wire, out LicenseClass? row) ? Some(row!) : None;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One consumed product. An empty Stage names the intent's own plane and carries the blob key; a named Stage names
// its producer and role, whose already-held output the fold resolves.
public readonly record struct StageInput(string Stage, string Role, string Key);

// One produced plane, named by the ROLE key the lease's binding roster carried — the graph's tensor name is the
// executor's business and never reaches the wire, because a packed tensor names several of these rows at once.
public readonly record struct StageOutput(string Role, string BlobKey, int Width, int Height, string Transfer, string Format);

// One MEASURED scalar. A grade carries its value and its role key alone — no blob, no extent, no transfer band —
// because nothing downstream samples it and a consumer asking for a grade reads the number the result already holds
// instead of fetching four bytes out of a store.
public readonly record struct StageScore(string Role, double Value);

// Every column is the LOWERED primitive the codec wrote: the producer's own types name none of this record, the
// vocabulary keys resolve through the rosters THIS package owns, and `Op` is a correlation string echoed verbatim
// onto the result rather than a value re-minted from a key. Extents land `int` because every grid derivation and
// span index downstream runs in that domain and the wire's `uint32` widens losslessly.
public sealed record StageRequest(
    string Stage, string ModelCardId, string License, Seq<StageInput> Inputs,
    int InputWidth, int InputHeight, int OutputWidth, int OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string Pad, string Bucket,
    string Layout, // the producer graph's dimension-order token (NCHW/NHWC) — the Materials wire widened; the binder seats tensors by it
    string Provider, string Precision, ulong Seed, string Op, string Artefact) {

    // Wire spellings resolve at the ROSTER that owns the rows, so this record holds no translation table and a
    // provider, precision, or licence landing there crosses without an edit here. The asymmetry is deliberate: a
    // substituted provider is reported on `ProviderUsed`, a substituted precision or grant is reported nowhere, so
    // one degrades and the other two refuse. The interior column shortens to `License` because a `LicenseClass`
    // member would SHADOW the `LicenseClass` type inside this record exactly as `PadMode` would — the same
    // interior-versus-wire split every other flat column takes, and the wire projection restores `licenseClass`.
    // PROVIDER resolution is deliberately NOT a property here: it answers what this host can run, which is a
    // property of the runtime rather than of the request, so the executor resolves it once against the frozen
    // provider census and threads that answer — a per-read property invites two reads of one decision.
    public Option<ModelPrecision> SelectedPrecision => ModelPrecision.FromWire(Precision);

    public Option<LicenseClass> SelectedLicense => LicenseClass.FromWire(License);

    // Scale is DERIVED from the extents the wire already threads: a stage publishes `inputWidth × scale`, so a
    // carried column could only ever contradict them, and a fractional or anisotropic ratio is a grid nothing builds.
    public Option<int> Scale =>
        InputWidth > 0 && InputHeight > 0
        && OutputWidth % InputWidth is 0 && OutputHeight % InputHeight is 0
        && OutputWidth / InputWidth == OutputHeight / InputHeight
            ? Some(OutputWidth / InputWidth)
            : None;

    // ONE plan construction lives here. Extent, bucket, overlap, pad, and scale come off this record; channels,
    // roster, layout, and blend come off the leased session, the only surface knowing the model. `TilePlan`'s own
    // validator then owns the fixed-bucket law, so no predicate here restates it and no later compare exists to
    // catch two spellings drifting. The wire's `padMode` field lands as the `Pad` column: a same-named `PadMode`
    // property SHADOWS the `PadMode` type inside this record (simple-name lookup binds the string member and
    // `string.TryGet` is CS1061; the static gate below hits CS0120), so the interior spelling shortens and the
    // wire projection restores `padMode` — the same interior-versus-wire split every other flat tile column takes.
    public Fin<TilePlan> Plan(int sourceChannels, Seq<TileProduct> products, TileAdmission admission, TileBlend blend, TileLayout layout) =>
        Scale.Case is not int scale
            ? Fin.Fail<TilePlan>(new ComputeFault.ModelRejected(
                $"<stage-scale:{InputWidth}x{InputHeight}:{OutputWidth}x{OutputHeight}>"))
            : !PadMode.TryGet(Pad, out PadMode? pad)
                ? Fin.Fail<TilePlan>(new ComputeFault.ModelRejected($"<stage-pad:{Pad}>"))
                : TilePlan.Validate(
                        sourceWidth: InputWidth, sourceHeight: InputHeight, channels: sourceChannels, products: products,
                        tileWidth: TileWidth, tileHeight: TileHeight, overlap: Overlap, scale: scale,
                        admission: admission, pad: pad!, blend: blend, layout: layout, out TilePlan? built) is { } fault
                    ? Fin.Fail<TilePlan>(fault)
                    : Fin.Succ(built!);

    // DECODE gate: everything provable WITHOUT a model. Extent, bucket, and pad legality prove here so a malformed
    // request never reaches a session lease; the plan itself builds after the lease, because only the model names
    // its own products. `StageRun` re-proves the grant alone, the one column an executing end never takes on trust.
    public static Fin<StageRequest> Admit(StageRequest request) =>
        request.SelectedLicense.Case is not LicenseClass licensed
            ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-license:{request.License}>"))
            : !licensed.Grants
                ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-blocked:{request.ModelCardId}>"))
                : request.SelectedPrecision.IsNone
                    ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-precision:{request.Precision}>"))
                    // Frozen wire records PIN `padMode` at `reflect`; the PadMode family stays the general tiling
                    // vocabulary, and this boundary — the one carrying the wire — is where the pin enforces.
                    : !StringComparer.Ordinal.Equals(request.Pad, PadMode.Reflect.Key)
                        ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-pad-pinned:{request.Pad}>"))
                        : request.Scale.IsNone
                            || !StringComparer.Ordinal.Equals(request.Bucket, $"{request.TileWidth}x{request.TileHeight}")
                            ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-shape:{request.Bucket}>"))
                            : Fin.Succ(request);
}

// `ParityFresh` and `Coverage` are the two columns that make the other measured columns readable. Decorators at the
// consuming end see every result identically and cannot know a memo answered it, so a residual histogram keyed on
// `GoldenDelta` alone reads N observations where ONE measurement was taken — the discriminant therefore
// rides the RESULT, set true only by the arm that actually leased a floor session and ran both probes, false on a
// memo hit and false on the floor identity where the zero is a definition rather than an observation. `Coverage`
// carries the measured overlap-add weight floor: the mosaic gates on it once and a reassembly at 0.001 publishes
// as healthy without it, so the one reader-visible proof the divide was well-conditioned crosses on the result.
// `Artefact` is the third: the digest of the weight bytes the leased session actually LOADED, measured at this end
// because only the lease observes which bytes reached the runtime, so the specifying end's card gate proves an
// observation rather than trusting a request round-tripped. Its slot ordinal sits past `Op` on the wire while the
// column reads beside `ModelCardId` here — `Runtime/wire#CONTRACT_EVOLUTION` `StageCrossing` folds slot ordinals and
// wire names alone, so declaration order is not contract identity.
public sealed record StageResult(
    string Stage, string ModelCardId, string Artefact, Seq<StageOutput> Outputs, Seq<StageScore> Scores,
    string ProviderUsed, int PartitionCount, double ElapsedMs, double GoldenDelta, bool ParityFresh, float Coverage,
    int TilesEmitted, string Op);

// What ONE executed grid produced, measured columns and the lease's own observations together. Fields and grades
// ride separate collections because they are separate MODALITIES, not one collection with a small extent: a field
// carries a blob key and an output extent, a grade carries a number. The demotion arm and the production arm answer
// this same shape, which is what lets a breach re-run at the floor and return through the same seam instead of
// forking the result construction.
public readonly record struct StageProduct(
    Seq<StageOutput> Products, Seq<StageScore> Scores, int Partitions, float Coverage, int Tiles, string Artefact);

// The card's parity envelope, mirrored at the shape the specifying end declares it in. `Upper` is the gate every
// comparison reads; `Lower` is the DECLARED not-a-point state — a deterministic card diverges on the provider axis
// alone and its floor is its ceiling, while a stochastic card's band spans a seed sweep no single run performs, so
// absence there states an unmeasured floor rather than a weakened gate. `Admits` folds finiteness into the same
// read, so a non-finite residual can never pass as within envelope.
public readonly record struct ResidualBand(Option<double> Lower, double Upper) {
    public static ResidualBand Point(double ceiling) => new(Some(ceiling), ceiling);

    public static ResidualBand Ceiling(double upper) => new(Option<double>.None, upper);

    public bool Admits(double delta) => double.IsFinite(delta) && delta <= Upper;
}

// The seed-driven graph input NO upstream stage emits: the graph's own tensor, the channel depth of the draw, and the
// factor the tile extent divides by. It rides the lease rather than an input binding because nothing produces it —
// the executor mints it — and a card declaring none answers absence, at which point no draw exists to bind.
public readonly record struct LatentInput(string Tensor, int Channels, int Downscale) {
    // Extent divides by the declared downscale; the specifying end gates that at its own declaration, and this fold
    // proves it once more rather than packing a fractional grid the session would reject as a shape fault.
    public Fin<LatentDraw> Draw(TilePlan plan, ulong seed) =>
        Channels > 0 && Downscale > 0 && plan.TileWidth % Downscale is 0 && plan.TileHeight % Downscale is 0
            ? Fin.Succ(new LatentDraw(
                Tensor,
                plan.Layout.Shape(Channels, plan.TileHeight / Downscale, plan.TileWidth / Downscale),
                Normal(seed, Channels * (plan.TileHeight / Downscale) * (plan.TileWidth / Downscale))))
            : Fin.Fail<LatentDraw>(new ComputeFault.ModelRejected(
                $"<stage-latent-grid:{Tensor}:{Downscale}:{plan.TileWidth}x{plan.TileHeight}>"));

    // A standard-normal draw the SEED alone determines. `Random` is the rejected source: its stream is a runtime
    // implementation detail, so a replay column the receipt publishes would hold only until a runtime re-tuned it.
    // SplitMix64 mixes the seed into a uniform stream and one Box-Muller polar pair fills two texels per rotation —
    // no rejection loop, so the draw at a given index is a pure function of the seed on every host that runs it.
    static float[] Normal(ulong seed, int count) {
        float[] draw = new float[count];
        ulong state = seed;
        for (int index = 0; index < count; index += 2) {
            double radius = Math.Sqrt(-2d * Math.Log(Uniform(ref state)));
            double angle = 2d * Math.PI * Uniform(ref state);
            draw[index] = (float)(radius * Math.Cos(angle));
            if (index + 1 < count) { draw[index + 1] = (float)(radius * Math.Sin(angle)); }
        }
        return draw;
    }

    // The mantissa lands in [1,2) and one subtraction shifts it, then the complement moves the half-open end: the
    // logarithm above is undefined at zero, and a rejection loop would make the stream position depend on the values
    // it drew — the one thing a replayable draw cannot afford.
    static double Uniform(ref ulong state) {
        ulong mixed = state += 0x9E3779B97F4A7C15UL;
        mixed = (mixed ^ (mixed >> 30)) * 0xBF58476D1CE4E5B9UL;
        mixed = (mixed ^ (mixed >> 27)) * 0x94D049BB133111EBUL;
        mixed ^= mixed >> 31;
        return 2d - BitConverter.UInt64BitsToDouble((mixed >> 12) | 0x3FF0000000000000UL);
    }
}

// The synthesized latent: the graph's own input tensor, its bound shape in the session's layout, and the draw filling
// it. The executor hands the ROOT bytes rather than a seed, so the distribution has exactly one implementation and a
// binder cannot re-derive a second one that replays differently.
public sealed record LatentDraw(string Tensor, long[] Shape, ReadOnlyMemory<float> Values);

// Multi-input stages bind ONE tensor: the wire's row order IS the channel-stack order, and the session's layout row
// owns the placement — planar appends whole channel planes, interleaved interleaves each texel's channel run — so a
// single-input stage crosses untouched and no second bound value exists to drift from the warm shape. The carrier
// exists because a stage holds up to three leases (the candidate probe, the floor probe, the production run) whose
// layouts may differ: the channel sum folds ONCE at the mint and each distinct layout stacks at most once, where a
// per-lease stack re-traversed the whole plane sequence four times for one stage. The memo keys on the layout row
// itself because layout is the only property of a lease the placement reads.
public sealed class PlaneStack {
    private readonly Seq<(ReadOnlyMemory<float> Plane, int Width, int Height, int Channels)> planes;
    private readonly Dictionary<TileLayout, ReadOnlyMemory<float>> stacked = new();
    private readonly int texels;

    private PlaneStack(Seq<(ReadOnlyMemory<float> Plane, int Width, int Height, int Channels)> planes, int channels, int texels) =>
        (this.planes, Channels, this.texels) = (planes, channels, texels);

    public int Channels { get; }

    public static PlaneStack Of(Seq<(ReadOnlyMemory<float> Plane, int Width, int Height, int Channels)> planes, StageRequest request) =>
        new(planes, planes.Sum(static plane => plane.Channels), request.InputWidth * request.InputHeight);

    // Single-plane stages ARE their own bound tensor, so the one-row case allocates nothing and skips the memo.
    public ReadOnlyMemory<float> For(TileLayout layout) {
        if (planes.Count is 1) { return planes[0].Plane; }
        if (stacked.TryGetValue(layout, out ReadOnlyMemory<float> held)) { return held; }
        float[] buffer = new float[(long)Channels * texels];
        int offset = 0;
        foreach ((ReadOnlyMemory<float> plane, int _, int __, int channels) in planes) {
            layout.Stack(plane.Span, buffer, channels, offset, Channels, texels);
            offset += channels;
        }

        stacked[layout] = buffer;
        return buffer;
    }
}

// --- [SERVICES] ----------------------------------------------------------------------------
// Everything about a run that only the LEASE knows: the card's binding roster naming each product's graph tensor,
// component lane, role key, and width, the tensor layout its graph emits, the taper profile its class of estimator
// wants, the partition count that bucket's warm-up measured, the bound input's own CHANNEL width (`InputMetadata`
// again — the gate the stacked input sum proves against), the card's parity `ResidualBand`, the digest of the weight
// bytes it loaded, and the LATENT its card declares — absent on a deterministic graph, which is what makes a seed
// nothing can bind refusable rather than silently dropped. `Flow` takes the BUILT plan and the SYNTHESIZED draw and
// binds every value from them, so shapes the flow holds, the draw it runs, and shapes the fold writes have one
// source. Holding the lease as this record's own disposable keeps its session alive across the whole grid.
public sealed record StageSession(
    Seq<TileProduct> Products, TileLayout Layout, TileBlend Blend, TileAdmission Admission, Option<int> Partitions, int PartitionCap,
    int InputChannels, ResidualBand Residual, string Artefact,
    Option<LatentInput> Latent, Func<TilePlan, Option<LatentDraw>, Fin<BoundFlow>> Flow, IDisposable Hold);

// Parity verdicts travel as VALUES and carry only what was MEASURED. Residuals are a property of
// `(card, provider, precision, runtime, host)` rather than of a process, so one shape keys the in-process memo and
// the durable row and neither tier can key differently. `MeasuredAt` is the second measured column and the reason
// the first stays trustworthy: the key names the silicon but not the driver or firmware stack running on it, and a
// driver revision changes a residual without changing one term of that key — so age, not a key term, is what
// retires a verdict. The BAND is deliberately absent: it belongs to the model card, rides the lease, and is read
// live at every gate, where freezing it here would keep demoting against an envelope the card has since widened.
public readonly record struct ParityVerdict(double Delta, Instant MeasuredAt) {
    // The floor at full precision answers itself, so the delta is zero by DEFINITION rather than by measurement and
    // the instant is the epoch nothing observed — `ParityFresh` on the result is what tells a reader which it was.
    public static readonly ParityVerdict Identity = new(0d, Instant.MinValue);
}

// Parity's DURABLE half injects exactly as every plane crosses. Compute holds no artifact store: the app root
// binds these legs against the Persistence artifact lane and supplies the running `HostFingerprint`, so the
// durable key and the process key are ONE derivation. `Read` answering `None` is a miss rather than a fault — an
// unbound lane costs exactly the two leases and two probes the memo would have saved — and `Write` returns `Unit`
// by contract because the composing root parks its own write refusal on its own evidence cell, so a failed artifact
// write never fails an inference whose measurement succeeded and this end never holds a rail it cannot carry out.
// `Horizon` rides here because this is the record the composing root builds: a verdict older than it reads as
// ABSENT and re-measures, which is what bounds the driver-and-firmware drift the parity key cannot name.
public sealed record ParityPort(
    HostFingerprint Host,
    Duration Horizon,
    Func<string, Option<ParityVerdict>> Read,
    Func<string, ParityVerdict, Unit> Write) {
    // Thirty days bounds a verdict against the cadence a host's driver stack actually moves at, and it is the
    // composition's value rather than a constant this fold reads — a root serving volatile silicon shortens it.
    public static readonly Duration CanonicalHorizon = Duration.FromDays(30);
}

// Every plane crosses as a content address the host resolves; Compute holds no store, no codec, and no vocabulary.
// `Lease` takes the resolved precision because a posture admitted at the wire and dropped before the session is a
// column the receipt then reports without anything having executed it. `Describe` takes the ROSTER ROW rather than a
// role string: the specifying end declares transfer and format per `(tensor, lane)` binding, so a packed export's two
// products describe apart and a role alone could not tell the port which binding it was answering for.
public sealed record StagePorts(
    Func<string, Fin<(ReadOnlyMemory<float> Plane, int Width, int Height, int Channels)>> Read,
    Func<ReadOnlyMemory<float>, int, int, int, Fin<string>> Write,
    Func<StageRequest, TileProduct, Fin<(string Transfer, string Format)>> Describe,
    Func<StageRequest, ExecutionProvider, ModelPrecision, Fin<StageSession>> Lease,
    ParityPort Parity);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StageRun {
    // Parity's PROCESS half. Residuals are a property of the decision, never of a plane: the canary grades the graph
    // an EP compiled at a precision on one machine, and the key names exactly that. The gate mirrors the
    // `Model/sessions#SESSION_CAPSULE` idiom — an immutable map behind one `Lock`, never a retry-capable swap over a
    // fold that already ran native effects — and the map is bounded by the key product, so nothing evicts. The
    // durable half rides `ParityPort`, so a restart reads a measured verdict instead of re-paying two leases, two
    // flows, and two inferences per triple — the memo alone dying at exit is what made every cold app root re-measure
    // what the last one already knew.
    // Memo capacity is a POLICY VALUE and eviction is least-recently-read. The key product is
    // `(card, provider ResultKey, host)`, and "bounded by the key product" only bounds anything if the product does
    // — a long-lived root serving a growing model registry grows it without ceiling. Eviction costs one durable
    // read on the next request, never a re-measurement, because `ParityPort` holds the same verdict.
    const int ParityMemoCap = 512;

    readonly record struct ParityMemo(ParityVerdict Verdict, long Read);

    static readonly Lock ParityGate = new();
    static HashMap<string, ParityMemo> Parity = HashMap<string, ParityMemo>();
    static long ParityTick;

    // ONE key across both tiers: the card names the model at its checksum, the provider's own `ResultKey` folds the
    // runtime version, the precision, and every behavior option that shaped the compiled graph, and the host
    // fingerprint pins the machine whose silicon produced the residual — a verdict measured on other silicon is
    // another host's verdict, and reading it here would gate an acceleration nothing on this machine graded.
    static string ParityKey(StageRequest request, ExecutionProvider provider, ModelPrecision precision, ParityPort parity) =>
        $"{request.ModelCardId}:{provider.ResultKey(OrtEnv.Instance().GetVersionString(), precision)}:{parity.Host}";

    public static Fin<Seq<StageResult>> Fold(
        Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, IClock clock, TimeProvider time) =>
        plan.Fold(
            Fin.Succ((Results: Seq<StageResult>(), Produced: HashMap<(string Stage, string Role), StageOutput>())),
            (state, request) => state.Bind(carried =>
                from admitted in StageRequest.Admit(request)
                from result in Execute(admitted, carried.Produced, ports, options, scope, clock, time)
                select (
                    Results: carried.Results.Add(result),
                    Produced: result.Outputs.Fold(
                        carried.Produced,
                        (map, output) => map.AddOrUpdate((request.Stage, output.Role), output)))))
            .Map(static carried => carried.Results);

    static Fin<StageResult> Execute(
        StageRequest request, HashMap<(string Stage, string Role), StageOutput> produced, StagePorts ports,
        RunOptions options, CancelScope scope, IClock clock, TimeProvider time) {
        long mark = time.GetTimestamp();
        // Grants re-check HERE even when Admit already ran at decode: any host edge reaches this end, and a grant
        // enforced only where the request is built trusts the caller's word — including the caller's SPELLING, so
        // roster resolution runs again rather than a resolved row riding in on the record.
        return from licensed in request.SelectedLicense
                       .ToFin(new ComputeFault.ModelRejected($"<stage-license:{request.License}>"))
               from _ in guard(
                       licensed.Grants,
                       (Error)new ComputeFault.ModelRejected($"<stage-blocked:{request.ModelCardId}>")).ToFin()
               from precision in request.SelectedPrecision
                       .ToFin(new ComputeFault.ModelRejected($"<stage-precision:{request.Precision}>"))
               from keys in Sources(request, produced)
               from planes in keys
                       .Traverse(key => ports.Read(key).ToValidation())
                       .As()
                       .ToFin()
               // Chained rows already proved their extents against their PRODUCERS at resolution; this guard
               // proves every plane's BYTES against the request's declaration — the whole gate for a source plane
               // no stage made, and the congruence every stacked row must share.
               from __ in planes
                       .Traverse(plane => guard(
                               plane.Width == request.InputWidth && plane.Height == request.InputHeight,
                               (Error)new ComputeFault.ModelRejected(
                                   $"<stage-extent-mismatch:{plane.Width}x{plane.Height}x{plane.Channels}:{request.InputWidth}x{request.InputHeight}>"))
                           .ToFin().ToValidation())
                       .As()
                       .ToFin()
               // One stack per stage: the parity probes and the production run share it, so the plane sequence is
               // traversed once for the sum and once per DISTINCT layout rather than once per lease.
               let stack = PlaneStack.Of(planes, request)
               // Provider resolution answers what this HOST can run, so it resolves once against the frozen census
               // and threads forward; precision already refused an unrostered spelling above.
               let requested = ExecutionProvider.FromWire(request.Provider)
               // Parity measures BEFORE the grid and decides nothing about the envelope: the card's live band
               // rides the lease the run itself takes, so a breach demotes there and the result reports the
               // demotion on ProviderUsed while GoldenDelta keeps the measured breach.
               from verdict in Assured(request, ports, stack, requested, precision, options, scope, clock)
               from outputs in Run(request, ports, stack, verdict.Verdict, requested, precision, options, scope)
               select new StageResult(
                   request.Stage, request.ModelCardId, outputs.Product.Artefact, outputs.Product.Products,
                   outputs.Product.Scores, outputs.Provider.ReportKey, outputs.Product.Partitions,
                   time.GetElapsedTime(mark).TotalMilliseconds, verdict.Verdict.Delta, verdict.Fresh,
                   outputs.Product.Coverage, outputs.Product.Tiles, request.Op);
    }

    // Chained stages NEVER carry the source plane: a binding naming a producer resolves against results already
    // held, so a pipeline whose links never touch is unrepresentable rather than merely discouraged. EVERY row
    // resolves — the frozen wire carries one row per consumed product in the card's binding order, and that order
    // is the channel-stack order the bound tensor takes. Each producer's PUBLISHED extent proves against this
    // request's declared input extent HERE — before the blob read and before the lease — because the frozen wire
    // refuses that disagreement at admit, and a plan defect caught by a bound session's shape mismatch names a
    // port rather than the two stages that disagree.
    static Fin<Seq<string>> Sources(StageRequest request, HashMap<(string Stage, string Role), StageOutput> produced) =>
        request.Inputs.IsEmpty
            ? Fin.Fail<Seq<string>>(new ComputeFault.ModelRejected($"<stage-no-input:{request.Stage}>"))
            : request.Inputs
                .Traverse(binding => (binding.Stage.Length is 0
                        ? Fin.Succ(binding.Key)
                        : produced.Find((binding.Stage, binding.Role))
                            .ToFin(new ComputeFault.ModelRejected($"<stage-unresolved:{binding.Stage}:{binding.Role}>"))
                            .Bind(upstream =>
                                upstream.Width == request.InputWidth && upstream.Height == request.InputHeight
                                    ? Fin.Succ(upstream.BlobKey)
                                    : Fin.Fail<string>(new ComputeFault.ModelRejected(
                                        $"<stage-extent-chain:{binding.Stage}:{binding.Role}:{upstream.Width}x{upstream.Height}:{request.InputWidth}x{request.InputHeight}>"))))
                    .ToValidation())
                .As()
                .ToFin();

    // ONE lease, ONE plan, ONE grid, and the band gate INSIDE that lease. Leasing reports the model's binding roster
    // and the card's live `ResidualBand`, the request folds the roster into a plan, that plan seats the flow,
    // and the grid runs once for every plane the model emits — a fold per output re-infers the whole image once per
    // plane a single forward pass already produced. `None` is the DEMOTION signal and nothing else: the lease that
    // reads the card's authority is the same lease that would have run the grid, so the decision happens where the
    // authority lives, the floor re-lease is paid only by a run that was about to publish outside its envelope, and
    // a widened band re-grades an already-measured residual on the very next request with nothing re-measured.
    // REGISTRATION IS NOT MEASUREMENT: the composition registers each bucket through `ModelSessions.Warm`, which
    // seats `WarmEvidence` with an absent partition count, and only the trace-reading `RunOps.WarmPulse` fills it —
    // so an unmeasured bucket names a pulse the composition has not injected, never a caller error.
    // The demoted PRECISION needs no column of its own: the floor row at full precision is the one demotion this
    // fold performs, so `providerUsed == cpu` already names it and a second reported column would restate one fact.
    static Fin<(StageProduct Product, ExecutionProvider Provider)> Run(
        StageRequest request, StagePorts ports, PlaneStack stack, ParityVerdict verdict,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Attempt(request, ports, stack, verdict, selected, precision, options, scope)
            .Bind(attempted => attempted.Case is StageProduct produced
                ? Fin.Succ((produced, selected))
                : Attempt(request, ports, stack, ParityVerdict.Identity, ExecutionProvider.Floor, ModelPrecision.Full, options, scope)
                    .Bind(floored => floored.Case is StageProduct onFloor
                        ? Fin.Succ((onFloor, ExecutionProvider.Floor))
                        // The floor arm grades the IDENTITY delta against the same band, so only a card whose `Upper`
                        // is itself negative or non-finite reaches here — a demotion loop is unrepresentable rather
                        // than merely unlikely.
                        : Fin.Fail<(StageProduct, ExecutionProvider)>(new ComputeFault.ModelRejected(
                            $"<stage-band:{request.ModelCardId}:{verdict.Delta}>"))));

    static Fin<Option<StageProduct>> Attempt(
        StageRequest request, StagePorts ports, PlaneStack stack, ParityVerdict verdict,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Leased(request, ports, selected, precision, stack.Channels, (session, plan) =>
            !session.Residual.Admits(verdict.Delta)
                ? Fin.Succ(Option<StageProduct>.None)
                // Stacked channel sums prove against the width the MODEL's own InputMetadata declares — the one
                // surface knowing how wide the bound tensor is — so a plan missing an input row or carrying a stray
                // one refuses by arithmetic before any texel moves.
                : from _ in guard(
                        plan.Channels == session.InputChannels,
                        (Error)new ComputeFault.ModelRejected(
                            $"<stage-input-channels:{plan.Channels}:{session.InputChannels}>")).ToFin()
                  from partitions in session.Partitions
                      .ToFin(new ComputeFault.ModelRejected($"<stage-partitions-unmeasured:{request.Bucket}>"))
                  from __ in guard(
                          partitions <= session.PartitionCap,
                          (Error)new ComputeFault.ModelRejected(
                              $"<stage-partitions:{partitions}:{session.PartitionCap}>")).ToFin()
                  // Only the leased session knows whether the graph takes a draw at all, so the seed-and-latent gate
                  // sits here — ONE pattern over the pair, because the two refusals are the two halves of one
                  // contradiction: a latent nothing seeds and a seed nothing binds.
                  from ___ in (request.Seed, session.Latent.Case) switch {
                      (0UL, LatentInput declared) => Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                          $"<stage-latent-unseeded:{request.Stage}:{declared.Tensor}>")),
                      (not 0UL, not LatentInput) => Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                          $"<stage-seed-unbindable:{request.Stage}:{request.Seed}>")),
                      _ => Fin.Succ(unit),
                  }
                  from emitted in Emit(request, ports, session, plan, stack.For(session.Layout), options, scope)
                  select Some(new StageProduct(
                      emitted.Products, emitted.Scores, partitions, emitted.Coverage, emitted.Tiles, session.Artefact)));

    // Lease and plan form ONE bracket: the hold releases inside the bind that took it, so a plan refusing never
    // strands a session and a fault never leaves a resident held.
    static Fin<T> Leased<T>(
        StageRequest request, StagePorts ports, ExecutionProvider provider, ModelPrecision precision,
        int sourceChannels, Func<StageSession, TilePlan, Fin<T>> use) =>
        ports.Lease(request, provider, precision).Bind(session => {
            using (session.Hold) {
                // Artefact pins HERE, at the one seam every lease crosses — the production run and both parity
                // probes — so a session holding weights the request never named refuses before a grid runs rather
                // than after the specifying end rejects a whole mosaic, and a residual graded on the wrong weights
                // never enters the memo. Comparing at the far end alone pays the entire inference to learn it.
                // LAYOUT crosschecks at the same seam and for the same reason the artefact does: the wire carries
                // the producer graph's dimension-order row key so the binder can seat tensors before the lease
                // returns, and the lease then reports the layout the model actually emits — a column the wire
                // carries and nothing compares is a claim, and a disagreement surfaces as a shape fault three
                // ports later naming neither end.
                return guard(
                        StringComparer.Ordinal.Equals(session.Artefact, request.Artefact),
                        (Error)new ComputeFault.ModelRejected(
                            $"<stage-artefact:{request.Artefact}:{session.Artefact}>")).ToFin()
                    .Bind(_ => guard(
                            TileLayout.TryGet(request.Layout, out TileLayout? declared)
                            && ReferenceEquals(declared, session.Layout),
                            (Error)new ComputeFault.ModelRejected(
                                $"<stage-layout-mismatch:{request.Layout}:{session.Layout.Key}>")).ToFin())
                    .Bind(_ => request.Plan(sourceChannels, session.Products, session.Admission, session.Blend, session.Layout))
                    .Bind(plan => use(session, plan));
            }
        });

    // GRADES bypass both ports: no `Describe` because a number carries no transfer band, no `Write` because a blob
    // the specifying end must fetch to read one float is a store round trip for a value the result already holds.
    static Fin<(Seq<StageOutput> Products, Seq<StageScore> Scores, float Coverage, int Tiles)> Emit(
        StageRequest request, StagePorts ports, StageSession session, TilePlan plan, ReadOnlyMemory<float> source,
        RunOptions options, CancelScope scope) =>
        Drawn(session, plan, request.Seed).Bind(latent => session.Flow(plan, latent)).Bind(flow => {
            using (flow) {
                return flow.InferTiled(options, scope, plan, source).Bind(assembled => {
                    using (assembled) {
                        return assembled.Planes
                            .Traverse(produced =>
                                (from shape in ports.Describe(request, produced.Product)
                                 from key in ports.Write(
                                     produced.Plane.Memory, plan.OutputWidth, plan.OutputHeight, produced.Product.Channels)
                                 select new StageOutput(
                                     produced.Product.Role, key, plan.OutputWidth, plan.OutputHeight,
                                     shape.Transfer, shape.Format)).ToValidation())
                            .As()
                            .ToFin()
                            .Map(products => (
                                Products: products,
                                Scores: assembled.Grades.Map(static grade => new StageScore(grade.Role, grade.Value)),
                                assembled.Coverage, assembled.Tiles));
                    }
                });
            }
        });

    // ONE synthesis site for the production grid and both parity probes. The draw is a pure function of the request's
    // seed and the plan's own tile extent, so the canary and the grid bind the SAME latent and the residual grades the
    // provider rather than two independent draws — the very comparison the seed threading exists to make meaningful.
    static Fin<Option<LatentDraw>> Drawn(StageSession session, TilePlan plan, ulong seed) =>
        session.Latent.Match(
            Some: declared => declared.Draw(plan, seed).Map(Some),
            None: static () => Fin.Succ(Option<LatentDraw>.None));

    // Parity is measured on ONE canary tile against a FULL-PRECISION floor run — two runs whatever the mosaic's tile
    // count, and the residual then grades the whole acceleration decision rather than comparing two runs that had
    // already agreed to lower precision. Floor-at-full answering itself is an IDENTITY rather than an unmeasured
    // zero (`providerUsed == cpu` at `fp32` is the discriminant a reader needs), so it reports without a second
    // lease. Beyond that, the decision is `(card, provider, precision)` and the plane is not part of it, so the
    // memo answers every later request in the process — delta AND band together, because a memo hit must still
    // gate — and the two extra leases a per-request measurement would take never happen. A delta outside the card's
    // band DEMOTES the run to the floor at full precision, keeping the measured breach on the result. Each lease
    // releases inside the bind that took it, and BOTH probes run at the request's own seed — two stochastic draws
    // compared would grade noise.
    static Fin<(ParityVerdict Verdict, bool Fresh)> Assured(
        StageRequest request, StagePorts ports, PlaneStack stack,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope, IClock clock) =>
        selected.IsFloor && ReferenceEquals(precision, ModelPrecision.Full)
            // Floor-at-full answering itself is an IDENTITY, so nothing was observed and `Fresh` is false — the
            // discriminant a reader needs is `providerUsed == cpu` at `fp32`, never a zero posing as a measurement.
            ? Fin.Succ((ParityVerdict.Identity, false))
            : Measured(request, ports, stack, selected, precision, options, scope, clock);

    static Fin<(ParityVerdict Verdict, bool Fresh)> Measured(
        StageRequest request, StagePorts ports, PlaneStack stack,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope, IClock clock) =>
        (Key: ParityKey(request, selected, precision, ports.Parity), Now: clock.GetCurrentInstant()) switch {
            // A hit measures nothing, so `Fresh` is false — which is what keeps a residual histogram counting
            // observations rather than requests; a per-inference tap cannot see this branch.
            var at => Remembered(at.Key, ports.Parity, at.Now).Case is ParityVerdict held
                ? Fin.Succ((held, false))
                : Leased(request, ports, selected, precision, stack.Channels, (candidate, plan) =>
                    Leased(request, ports, ExecutionProvider.Floor, ModelPrecision.Full, stack.Channels, (reference, truthPlan) =>
                        from fast in Probe(candidate, plan, request.Seed, stack.For(candidate.Layout), options, scope)
                        from truth in Probe(reference, truthPlan, request.Seed, stack.For(reference.Layout), options, scope)
                        from delta in Residual(fast, truth)
                        select (Remember(at.Key, new ParityVerdict(delta, at.Now), ports.Parity), true))),
        };

    // Process memo answers first and the durable row second, and a durable hit SEATS the memo so a cold start pays
    // one artifact read per decision rather than one per request. Both tiers answer through ONE staleness gate: a
    // verdict past the port's horizon reads as ABSENT and re-measures, because the parity key names the silicon but
    // not the driver stack on top of it, and no term of that key moves when a driver revision moves a residual.
    static Option<ParityVerdict> Remembered(string at, ParityPort parity, Instant now) {
        lock (ParityGate) {
            if (Parity.Find(at).Case is ParityMemo seated && Fresh(seated.Verdict, parity, now)) {
                Parity = Parity.SetItem(at, seated with { Read = ++ParityTick });
                return Some(seated.Verdict);
            }
        }
        return parity.Read(at).Filter(durable => Fresh(durable, parity, now)).Map(durable => Seat(at, durable));
    }

    static bool Fresh(ParityVerdict verdict, ParityPort parity, Instant now) =>
        now - verdict.MeasuredAt <= parity.Horizon;

    static ParityVerdict Remember(string at, ParityVerdict verdict, ParityPort parity) {
        parity.Write(at, verdict);
        return Seat(at, verdict);
    }

    static ParityVerdict Seat(string at, ParityVerdict verdict) {
        lock (ParityGate) {
            Parity = Parity.AddOrUpdate(at, new ParityMemo(verdict, ++ParityTick));
            if (Parity.Count > ParityMemoCap) {
                Parity = toSeq(Parity.AsIterable().OrderBy(static row => row.Value.Read))
                    .Head
                    .Match(Some: oldest => Parity.Remove(oldest.Key), None: () => Parity);
            }
        }
        return verdict;
    }

    static Fin<float[]> Probe(
        StageSession session, TilePlan plan, ulong seed, ReadOnlyMemory<float> source, RunOptions options, CancelScope scope) =>
        Drawn(session, plan, seed).Bind(latent => session.Flow(plan, latent)).Bind(flow => {
            using (flow) { return flow.Canary(options, scope, plan, source); }
        });

    // One fold serves both modalities: a GRADED canary's arrays hold exactly one element, so the max-magnitude read IS
    // the scalar absolute difference and a second residual arm would restate one arithmetic under a modality name.
    static Fin<double> Residual(float[] candidate, float[] reference) {
        if (candidate.Length != reference.Length) {
            return Fin.Fail<double>(new ComputeFault.ModelRejected($"<golden-shape:{candidate.Length}:{reference.Length}>"));
        }
        float[] difference = new float[candidate.Length];
        TensorPrimitives.Subtract(candidate, reference, difference);
        float residual = TensorPrimitives.MaxMagnitude<float>(difference);
        return float.IsFinite(residual)
            ? Fin.Succ((double)Math.Abs(residual))
            : Fin.Fail<double>(new ComputeFault.ModelRejected("<golden-nonfinite>"));
    }
}

```

## [05]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` — four behaviour columns (`Serves`/`CutsFirst`/`StoresPositive`/`StoresNegative`) drive every posture through the derived `ReadThroughStore` predicate and the derived `Flags` suppression set; `CacheOps` owns key derivation, echo-validated read-through, the precision-TTL negative probe, the one entry-options mint, the two eviction scopes, and the `Cached<T>` envelope. Cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact — a second fact stream is rejected, `ComputeReceipt` being the package's only measured-fact vocabulary.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches rather than re-running every call, `precision` sizes the negative TTL, and present drift evidence is an input-shape discriminant rather than an independently reconstructed monitor. `public static Fin<Option<DriftVerdict>> Sentinel(Option<GraduationEnvelope> envelope, Seq<FeatureSample> serving, DriftPolicy policy)` is the ONE fold that fills that slot: the run flow hands it the model's own envelope and its serving window, and the report's headline verdict crosses while the per-feature verdicts and uncovered roster stay with the evidence reader.
- Auto: `Key` stamps model checksum, input digest, EP key, ORT version, and option-table hash so cross-version drift never serves a stale hit; content-addressed dedup coalesces byte-identical-input/identical-EP runs to one stored payload. `Through` first consumes a `DriftVerdict.Breached` by PURGING the whole model's tag group and returning `ComputeFault.EquivalenceMiss`, then dispatches on the `ReadThroughStore` predicate — the read-through path delegates to `CacheLane.ModelResult` `Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the lane TTL), and every other row falls to `Fresh`, which cuts both keys when `CutsFirst`, serves a cached negative through an `Option<Cached<Fin<T>>>` `DisableUnderlyingData` cache-only probe, produces once, clears stale negative evidence before a positive write, then stores the success under the result key or the failure under the `neg:` key at `ModelPrecision.NegativeTtl` — every column reaches a live branch, no posture a twin of another. Every one of those writes and probes mints its options through the one `Options` fold, so the row's derived read- and write-suppression rides each call rather than a hand-picked flag set per site, and every write stamps `CacheLane.Tag(checksum)` beside the bare lane key so the purge above and the read-through's own framing cut one tag space.
- Receipt: outcome projects onto `ComputeReceipt.Cache(Outcome, Key, Bytes)` (`Outcome` ∈ `hit`/`miss`/`store`/`evict`) at the sink edge; `CacheLane.ReportTagMetrics` meters live hit/miss/evict by lane tag; `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated echo mismatches `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row with its four columns, its suppression flags and options following by derivation; a pre-computed result populates through the `WriteThrough` posture with a constant factory, never a second store entry; a richer outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner; a graduated-model validity axis is the `Model/identity#MODEL_IDENTITY` drift sentinel consumed here, never a sibling monitor.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect. Cached payloads ride the `Cached<Fin<T>>` envelope whose `Echo` is `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 corruption the content key alone cannot; a value stored without the echo is rejected. `ReadThrough` caches success and failure under one lane-TTL entry while `Negative` caches only the failure at `ModelPrecision.NegativeTtl` and re-produces every success — behaviourally distinct rows, so an identical-column twin of `ReadThrough` is the named defect. Content-addressed dedup folds the input digest into the stored key so identical-input runs across callers coalesce; a second dedup owner is rejected. Cross-process result-reuse recency horizons read by reference from the Persistence `ModelResultIndex` owner — a second `Duration horizon` parameter beside the policy rows is the named defect. Every `DriftVerdict.Breached` from the identity drift sentinel is consumed as reuse invalidation — the lane purges the model's whole tag group and the run faults `ComputeFault.EquivalenceMiss` — so a graduated model whose serving population leaves its evidence envelope never keeps serving cached verdicts under ANY input digest, provider row, or precision; cutting the requested key alone was the invalidation that left the rest of the model serving, and a drift monitor beside the identity sentinel is the rejected sibling. Tags reach the cache only through `CacheLane.Tag`: a bare checksum stamped at a write is a tag no lane minted, which is exactly what makes a tag-scoped purge unable to find it. Hit/miss/evict are HybridCache `ReportTagMetrics` consequences, never a second fact stream.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CachePolicy {
    public static readonly CachePolicy Bypass = new("bypass", serves: false, cutsFirst: false, storesPositive: false, storesNegative: false);
    public static readonly CachePolicy ReadThrough = new("read-through", serves: true, cutsFirst: false, storesPositive: true, storesNegative: true);
    public static readonly CachePolicy WriteThrough = new("write-through", serves: false, cutsFirst: false, storesPositive: true, storesNegative: false);
    public static readonly CachePolicy Refresh = new("refresh", serves: false, cutsFirst: true, storesPositive: true, storesNegative: true);
    public static readonly CachePolicy Negative = new("negative", serves: true, cutsFirst: false, storesPositive: false, storesNegative: true);

    public bool Serves { get; }
    public bool CutsFirst { get; }
    public bool StoresPositive { get; }
    public bool StoresNegative { get; }

    public bool ReadThroughStore => Serves && StoresPositive && StoresNegative && !CutsFirst;

    // Four per-call suppression flags DERIVE from the columns that already decide the posture: a row that serves
    // nothing closes both read legs, a row that stores nothing closes both write legs. Deriving is what keeps a
    // posture and the flags it sends from ever disagreeing — a fifth enumerated column is one more place a new row
    // gets filled in wrong — and `ReadThrough`'s empty derivation is exactly why that one row can ride the lane's own
    // entry options untouched while every other row mints its options through the derivation.
    public HybridCacheEntryFlags Flags =>
        (Serves
            ? HybridCacheEntryFlags.None
            : HybridCacheEntryFlags.DisableLocalCacheRead | HybridCacheEntryFlags.DisableDistributedCacheRead)
        | (StoresPositive || StoresNegative
            ? HybridCacheEntryFlags.None
            : HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite);
}

public readonly record struct Cached<T>(string Echo, T Value);

public static class CacheOps {
    public static ModelResultKey Key(ModelIdentity model, UInt128 inputDigest, ExecutionProvider ep, ModelPrecision precision) =>
        new(model.Key, inputDigest, ep.ResultKey(OrtEnv.Instance().GetVersionString(), precision));

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

    extension(HybridCache cache) {
        public async ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default) {
            if (drift.Case is DriftVerdict.Breached breached) {
                await Purge(cache, key, token);
                return Fin.Fail<T>(new ComputeFault.EquivalenceMiss($"drift:{breached.EvidenceKey}:{breached.Feature}:{breached.Psi}:{breached.SampleCount}"));
            }
            return policy.ReadThroughStore
                ? await ServeStore(cache, key, state, produce, token)
                : await Fresh(cache, policy, key, precision, state, produce, token);
        }
    }

    // OWNER keys cross the lane seam, never tags: `CacheSurface.Read` frames the checksum through `CacheLane.Tag`
    // and adds the bare lane key itself, which is the same framing every write below stamps by hand — a raw checksum
    // on either leg is a tag no lane ever minted, so no cut could ever reach it.
    static async ValueTask<Fin<T>> ServeStore<T, TState>(HybridCache cache, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) =>
        Validated(key, await cache.Read(
            CacheLane.ModelResult, key.ToString(),
            (Key: key, State: state, Produce: produce),
            static async (s, ct) => new Cached<Fin<T>>(s.Key.ModelChecksum, await s.Produce(s.State, ct)),
            Seq(key.ModelChecksum), token));

    static async ValueTask<Fin<T>> Fresh<T, TState>(HybridCache cache, CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) {
        if (policy.CutsFirst) { await Cut(cache, key, token); }
        if (policy.Serves) {
            Option<Cached<Fin<T>>> probed = await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"neg:{key}"),
                static _ => new ValueTask<Option<Cached<Fin<T>>>>(Option<Cached<Fin<T>>>.None),
                Options(policy, CacheLane.ModelResult.Entry.Expiration, HybridCacheEntryFlags.DisableUnderlyingData),
                cancellationToken: token);
            if (probed.Case is Cached<Fin<T>> cached) { return Validated(key, cached); }
        }
        Fin<T> value = await produce(state, token);
        if (value.IsSucc && policy.StoresPositive) {
            await cache.Remove(CacheLane.ModelResult, $"neg:{key}", token);
            await cache.SetAsync(
                CacheLane.ModelResult.Scoped(key.ToString()), new Cached<Fin<T>>(key.ModelChecksum, value),
                Options(policy, CacheLane.ModelResult.Entry.Expiration),
                [CacheLane.ModelResult.Tag(key.ModelChecksum), CacheLane.ModelResult.Key], token);
        }
        else if (value.IsFail && policy.StoresNegative) {
            await cache.SetAsync(
                CacheLane.ModelResult.Scoped($"neg:{key}"), new Cached<Fin<T>>(key.ModelChecksum, value),
                Options(policy, precision.NegativeTtl.ToTimeSpan()),
                [CacheLane.ModelResult.Tag(key.ModelChecksum), CacheLane.ModelResult.Key], token);
        }
        return value;
    }

    // ONE options mint for every write and probe this page makes: the lane's own lifetimes and hard flags with the
    // row's derived suppression folded in, so a posture can never store through a leg it declared closed and a probe
    // adds only the cache-only bit on top of what its row already says.
    static HybridCacheEntryOptions Options(CachePolicy policy, TimeSpan? expiration, HybridCacheEntryFlags probe = HybridCacheEntryFlags.None) =>
        new() {
            Expiration = expiration,
            LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
            Flags = CacheLane.ModelResult.Entry.Flags | policy.Flags | probe,
        };

    // Drift breaches invalidate the whole MODEL — every input digest, every provider row, every precision — because
    // an evidence envelope a graduated model left is a property of the MODEL, so cutting the requested key alone
    // leaves every other cached verdict of that same model still serving. The lane's own `Invalidate` frames the
    // checksum into the tag space every write above stamps and shadows the group by timestamp at constant cost,
    // where a key-by-key sweep would need an index the cache does not keep.
    static ValueTask Purge(HybridCache cache, ModelResultKey key, CancellationToken token) =>
        cache.Invalidate(CacheLane.ModelResult, Seq(key.ModelChecksum), token);

    // `CutsFirst` postures retire exactly the key pair they are about to rewrite.
    static async ValueTask Cut(HybridCache cache, ModelResultKey key, CancellationToken token) {
        await cache.Remove(CacheLane.ModelResult, key.ToString(), token);
        await cache.Remove(CacheLane.ModelResult, $"neg:{key}", token);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
