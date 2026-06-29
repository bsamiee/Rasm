# [COMPUTE_INFERENCE]

Rasm.Compute model inference: the OrtValue-only run-mode fold over the shared session with the shared `Tensor/residency#ORT_BRIDGE` `BoundFlow` capsule composed for its shared-arena zero-allocation hot path and device-resident chaining, one polymorphic `RunInput` admission keyed on carrier shape composing `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress`, the vectorized `TensorPrimitives` reduction kernels (softmax top-k, mean/CLS pooling, L2-normalize), the `System.Numerics.Tensors` carrier bridge, and the version-stamped deterministic result cache read-through over the produced run. The page owns the `RunConfig`/`RunInput`/`Pooling` run vocabulary, the `RunOps` run-mode fold composing the one `Tensor/residency#ORT_BRIDGE` `BoundFlow` capsule, the `CachePolicy` cache rows projecting onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact, and the `CacheOps` key-derivation and policy-dispatched read-through; the run surfaces ride `Microsoft.ML.OnnxRuntime` and the reductions `System.Numerics.Tensors`, the cache rides `Microsoft.Extensions.Caching.Hybrid`, the `BoundFlow` bound input and sink allocate from the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` arena, and the `ModelIdentity` from `Model/identity#MODEL_IDENTITY`, the `ExecutionProvider`/`ModelPrecision` from `Model/providers#EP_AXIS`, the `RunInput.Strings` egress completion from `Model/extension#EXTENSION_OPS`, the AppHost `CancelScope`/`ClockPolicy`, and the Persistence `ModelResultKey`/`ModelResultIndex`/`ArtifactIndexRow`/vector-lane owners arrive settled. The `RunInput.Strings` ingress is the catalogued ingress the extension-ops string egress completes, and the `Embed` vector feeds the Persistence vector lane by reference.

## [01]-[INDEX]

- [01]-[INFERENCE_MODES]: `OrtValue`-only run modes; one polymorphic input admission; vectorized reductions; cancellation rail; profiling artifacts; `ModelRun` receipt; device-resident shared-arena chaining; `System.Numerics.Tensors` bridge.
- [02]-[RESULT_CACHE]: version-stamped deterministic keys; column-driven cache-policy rows; content-addressed dedup; echo-validated read-through; precision-TTL negative cache; stampede single-flight; outcome projected onto `ComputeReceipt.Cache`.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` — the run-mode fold over the shared session: single, bound-batch, named bound, windowed, embedding, classification, clash-scoring, and S.N.Tensors-bridge runs discriminated by intent payload shape; `RunInput` one polymorphic input admission keyed on carrier shape composing the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; the `Tensor/residency#ORT_BRIDGE` `BoundFlow` capsule composed for the shared-arena device-resident hot path.
- Cases: single `Run`; lane-enqueued async (the lane seam owns the thread hop — the native `RunAsync` requires pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so it is the rejected spelling); `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip projection arm; the `Tensor/residency#ORT_BRIDGE` `BoundFlow` steady-state hot path composed with its shared-arena `CreateAllocatedTensorValue` bound input and sink and device-resident `ClearBound*`/`BindOutputToDevice`/`CreateTensorValueWithData` rebind; `Chunked` streaming windows over chunked inputs through `RecyclableMemoryStream.GetReadOnlySequence`; `Embed` mean-pool/CLS-slice + L2-normalized text-to-vector projection over an embedding model; `Classify` `TensorPrimitives.SoftMax`-calibrated top-`k` run for BIM point-cloud→element classification and symbol recognition over the interchange `PointScan` encoding; `ClashScore` scalar-output run for clash false-positive scoring over a candidate `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge over `CreateTensorValueFromSystemNumericsTensorObject` and `GetTensorDataAsTensorSpan<T>` materializing to a detached `TResult` inside the native bracket.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket.
- Auto: `Plan` wires deadline expiry into the `Terminate` one-way latch from the linked `CancelScope`, attaches LoRA adapters, and folds the `RunConfig` row table into `AddRunConfigEntry` calls so a posture change selects a row rather than editing the fence; one conversion arm (`Faulted`) classifies failures by scope provenance into `DeadlineExpired`/`Cancelled` and lifts a non-cancellation native fault to `ModelRejected` carrying its message — the single fault oracle every run shape rides, never a raw `Error.New` leaking an unclassified native exception past the typed rail; output buffers size from `GetTensorTypeAndShape().ElementCount`, never re-multiplied dimensions; `RunInput` admits the carrier polymorphically by composing the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carrier overloads (the sole `OrtValue` C-data factory owner, never a re-spelled factory) — a managed `T[]+shape` rides `Ingress<T>(T[], shape)`, a `System.Numerics.Tensors.Tensor<T>` rides `Ingress<T>(Tensor<T>)`, and a `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` rides `Ingress(Tensor<string>)` (the ONNX-owned tensor type the model boundary requires — a distinct type from the `System.Numerics.Tensors` carrier the numeric cases ride, so the two `Tensor<...>` spellings never unify) — one `[Union]` admission whose per-case `Admit` override returns `Fin<(string, OrtValue)>` and dispatches by carrier shape over the open unmanaged `T` (every `TensorDtype` carrier, not a closed float/long/int arm set that degenerate-throws on `double`/`byte`), `Bind` traversing the cases into one `Fin<Seq<(string, OrtValue)>>` so a faulted ingress aborts the run, never the three sibling `StringInput`/`Input<T>`/`TensorInput<T>` factories; `Classify` softmaxes each logit slice through `TensorPrimitives.SoftMax` and emits the top-`k` calibrated `(class, probability)` candidates per row through a span-safe loop (no hand-rolled argmax accumulation and no `ReadOnlySpan<float>` captured into a lambda — the projection materializes per row through an explicit span walk, the named kernel exemption), the per-row top-`k` selected through a bounded `PriorityQueue` min-heap (`O(width·log k)`, never a full per-row `Array.Sort` over the whole class taxonomy) and `Embed` projects the last-hidden-state down the `Pooling` axis — `Mean` mean-pool or `Cls` first-token slice — then L2-normalizes via `TensorPrimitives.Norm` + `Divide`; the `ModelRun` receipt factory stamps route, batch, the `GetTensorSizeInBytes` peak footprint, the `GetTensorMemoryInfo` arena name, and the optional profile-artifact path; a sentinel/NaN egress projects to `Option` at the boundary.
- Receipt: the `ModelRun` receipt carries model checksum, EP, run mode, batch size, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `OrtMemoryInfo` allocator name from `GetTensorMemoryInfo` as `ArenaAllocator`, and the optional `EndProfiling` chrome-trace artifact path; profiling chrome-trace artifacts land as `ArtifactKind.OnnxProfile` rows with the artifact path in the receipt.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload-shape case on the intent family; a new run-config posture is one `RunConfig` row carrying its `AddRunConfigEntry` key-value pairs and its `OrtAllocatorType` arena column; an embedding model is one more `Embed` run over the same session capsule, a new embedding pooling shape is one `Pooling` row folded into the `Pool` dispatch (never a parallel pooling method), a wider classifier candidate set is the `Classify` `top` arity (never a parallel top-k surface), a classifier whose output is the `ZipMap` sequence-of-maps probability shape rather than a raw logit tensor is read through the `Model/extension#EXTENSION_OPS` `Egress`→`OpOutput.Batched` reader (never a parallel ZipMap arm on `Classify`, whose `TensorPrimitives.SoftMax` argmax owns the numeric-logit shape only), and a BIM classifier, symbol recognizer, or clash false-positive scorer is one more `Classify`/`ClashScore` run over the shared session reusing the inference engine for the non-AI in-scope BIM pipelines (point-cloud→element classification consumes the interchange `PointScan` encoding, clash scoring consumes the `Solver/clash#CLASH_AND_TWIN` `ClashPair` feature vector), never a new model lane or a BIM-specific service; a tensor-lane handoff that already holds a `System.Numerics.Tensors.Tensor<T>` is one `InferTensor` run binding the carrier directly with zero managed copy and projecting to a detached value; zero new surface.
- Boundary: `RunOps` extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` boundary capsule and this fence carries bracketed statement forms with deterministic native disposal; OrtValue-only law — `NamedOnnxValue`, `DisposableNamedOnnxValue`, and `FixedBufferOnnxValue` are superseded spellings that never appear; `CreateTensorValueFromMemory` binds rented staging arrays without copies and the backing must outlive the value and every run binding it — the value's dispose IS the owner's release point; `CreateTensorValueFromSystemNumericsTensorObject<T>` admits the tensor-lane `Tensor<T>` owner directly so a tensor already in the compute interior never round-trips through a managed array, and `GetTensorDataAsTensorSpan<T>` reads egress as a `ReadOnlyTensorSpan<T>` consumed and reduced to a DETACHED `TResult` inside the native bracket (per `boundaries.md` REF_SAFE_PROJECTION) — the `ReadOnlyTensorSpan<T>` ref struct never crosses the `Fin` boundary because `shapes.md` UNIONS rejects a ref-struct generic type argument; the `Terminate` latch is the single cancellation propagation path and the deadline-poll cadence binds from the CANCELLATION research row; `InferBound` runs `RunWithBinding` over a populated `OrtIoBinding`, bracketing the run between `SynchronizeBoundInputs` and `SynchronizeBoundOutputs` and projecting `GetOutputValues`, with the optional `names` zip arm delivering the `RunWithBindingAndNames(RunOptions, OrtIoBinding, string[])` named-output convenience by pairing `GetOutputNames()` against the result without materializing the forbidden `IDisposableReadOnlyCollection<DisposableNamedOnnxValue>` that member returns — one `InferBound`, not a separate named sibling; the `Tensor/residency#ORT_BRIDGE` `BoundFlow` capsule is the zero-allocation steady-state posture for repeated same-shape inference the run-mode fold composes (never a second IO-binding capsule) — `BoundFlow.Lease` over the model session and the `ModelSessions.SharedAllocator` arena binds the input and output `OrtValue`s both arena-allocated through `CreateAllocatedTensorValue` (not `OrtAllocator.DefaultInstance` and not a managed staging plane), `RunOps.Pulse` writes `payload` directly into the bound value through `BoundFlow.Write` → `GetTensorMutableDataAsSpan<float>()` (the catalogued IO-binding mutable-span write, no managed `MemoryOwner<float>` staging copy) and brackets `BoundFlow.Run`'s `RunWithBinding` under the `CancelScope` per `Pulse` with no per-call marshal — and a shape-class transition rebinds through `BoundFlow.RebindDevice` (the device-input `BindInput(string, TensorElementType, long[], OrtMemoryAllocation)` overload binding a shared-arena device buffer with `BindOutputToDevice` routing the output) and `BoundFlow.RebindDevicePointer` (the raw-device-pointer `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` input path); `Chunked` slices the caller's `ReadOnlySequence<byte>` (the `RecyclableMemoryStream.GetReadOnlySequence` fragmented zero-copy view) frame-by-frame via `ReadOnlySequence<byte>.Slice(long, long)` and drives one `BoundFlow.Pulse` per window whose `Write` copies the sliced sequence straight into the bound value through `ReadOnlySequence<byte>.CopyTo(bound.GetTensorMutableRawData())` — the SAME single copy the steady-state float write performs, never an intermediate `MemoryOwner<float>` staging buffer (the rejected double-copy) and never a `.ToArray()`; the run advances the `Runtime/progress#PROGRESS_CELL` cell to the `Streaming` `ProgressPhase` carrying the window count and projects one `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.StreamSegment(artifactId, windows, bytes)` through `StreamReceipt` at the sink edge, never one receipt per chunk; `Embed` runs one inference over an embedding model and projects the last-hidden-state down the `Pooling` axis — `Mean` mean-pools every token through `TensorPrimitives.Add`+`Divide`, `Cls` slices the first token's hidden vector — then L2-normalizes via `TensorPrimitives.Norm`+`Divide` to a `float[]` vector feeding the Persistence vector lane by reference, keying on the content-hash and model-id reuse identity the Persistence owner holds — a raw last-token passthrough is the rejected form and a `Pooling`-blind hardcoded mean-pool is the superseded form (the consumer `Model/embedding#EMBEDDING` contract reads both pooling shapes); the `RunConfig.Arena` column classifies the run allocator through `OrtAllocatorType` (`ArenaAllocator` steady, `DeviceAllocator` device-resident); `Profile` is guarded on `policy.Profiling` and returns `Fin<ArtifactIndexRow>` so a no-profile session never reads a nonexistent trace path; output projection scopes native memory inside `project` and sentinel or NaN values project to `Option` at the boundary, never inward.

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
    public static readonly Pooling Mean = new("mean", clsSlice: false);
    public static readonly Pooling Cls = new("cls", clsSlice: true);

    public bool ClsSlice { get; }
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

    public abstract Fin<(string Name, OrtValue Value)> Admit();
}

public static class RunOps {
    public static RunOptions Plan(CancelScope scope, RunConfig config, Option<OrtLoraAdapter> lora = default) {
        var options = new RunOptions();
        lora.Iter(options.AddActiveLoraAdapter);
        config.Entries.Iter(entry => options.AddRunConfigEntry(entry.Key, entry.Value));
        scope.Source.Token.Register(() => options.Terminate = true);
        return options;
    }

    public static Fin<Seq<(string Name, OrtValue Value)>> Bind(params ReadOnlySpan<RunInput> inputs) =>
        toSeq(inputs.ToArray()).TraverseM(static input => input.Admit()).As();

    extension(InferenceSession session) {
        public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            Bracket(scope, project, () => session.Run(options, inputs.Map(static row => row.Name), inputs.Map(static row => row.Value), outputs));

        public Fin<T> InferBound<T>(RunOptions options, CancelScope scope, OrtIoBinding binding, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Option<Func<Seq<(string Name, OrtValue Value)>, Fin<T>>> named = default) =>
            Bracket(
                scope,
                results => named.Case is Func<Seq<(string Name, OrtValue Value)>, Fin<T>> zip
                    ? zip(toSeq(binding.GetOutputNames()).Zip(toSeq(results), static (name, value) => (Name: name, Value: value)))
                    : project(results),
                () => { binding.SynchronizeBoundInputs(); session.RunWithBinding(options, binding); binding.SynchronizeBoundOutputs(); return binding.GetOutputValues(); });

        public Fin<ArtifactIndexRow> Profile(SessionPolicy policy, DataClassification classification, string retentionClass, Instant at) =>
            policy.Profiling && session.EndProfiling() is string path
                ? Fin.Succ(ArtifactIndexRow.Admit(ArtifactKind.OnnxProfile, path, File.ReadAllBytes(path), classification, retentionClass, at))
                : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected("profiling-disabled"));

        public Fin<Seq<T>> Chunked<T>(RunOptions options, CancelScope scope, BoundFlow loop, ReadOnlySequence<byte> windows, int windowFloats, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            long frameBytes = (long)windowFloats * sizeof(float);
            int frames = frameBytes > 0L ? checked((int)(windows.Length / frameBytes)) : 0;
            return toSeq(Enumerable.Range(0, frames))
                .TraverseM(index => loop.Pulse(options, scope, windows.Slice(index * frameBytes, frameBytes), project)).As();
        }

        public Fin<float[]> Embed(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, int hidden, Pooling pooling) =>
            session.Infer(options, scope, inputs, Seq(output), results => {
                var pooled = Pool(results.First().GetTensorDataAsSpan<float>(), hidden, pooling);
                var norm = TensorPrimitives.Norm<float>(pooled);
                if (norm <= 0f) { return Fin.Fail<float[]>(new ComputeFault.ModelRejected("embed-zero-norm")); }
                TensorPrimitives.Divide(pooled, norm, pooled);
                return Fin.Succ(pooled);
            });

        public Fin<TResult> InferTensor<T, TResult>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Func<ReadOnlyTensorSpan<T>, Fin<TResult>> project) where T : unmanaged =>
            session.Infer(options, scope, inputs, Seq(output), results => project(results.First().GetTensorDataAsTensorSpan<T>()));

        public Fin<Seq<Seq<(int Class, float Probability)>>> Classify(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string logits, int classes, int top = 1) =>
            session.Infer(options, scope, inputs, Seq(logits), results => {
                var scores = results.First().GetTensorDataAsSpan<float>();
                int width = Math.Max(1, classes);
                int rows = scores.Length / width;
                int rank = Math.Clamp(top, 1, width);
                var probabilities = new float[rows * width];
                var ranked = new (int Class, float Probability)[rows][];
                for (int row = 0; row < rows; row++) {
                    var probability = probabilities.AsSpan(row * width, width);
                    TensorPrimitives.SoftMax(scores.Slice(row * width, width), probability);
                    ranked[row] = TopK(probability, rank);
                }
                return Fin.Succ(toSeq(ranked).Map(static row => toSeq(row)));
            });

        public Fin<float> ClashScore(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> features, string output) =>
            session.Infer(options, scope, features, Seq(output), static results => {
                var scores = results.First().GetTensorDataAsSpan<float>();
                return float.IsNaN(scores[0]) ? Fin.Fail<float>(new ComputeFault.ModelRejected("clash-nan")) : Fin.Succ(scores[0]);
            });

        public ComputeReceipt.ModelRun RunReceipt(ModelIdentity model, ExecutionProvider ep, string mode, int batch, OrtValue output, CorrelationId correlation, Option<string> profile, Duration elapsed) =>
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()), output.GetTensorMemoryInfo().Name, profile.IfNone((string?)null)) {
                Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.Onnx, AllocationClass = AllocationClass.NativeOrt, Elapsed = elapsed,
            };

        public ComputeReceipt.StreamSegment StreamReceipt(ReadOnlySequence<byte> windows, int windowFloats, string artifactId, CorrelationId correlation, Duration elapsed) {
            long frameBytes = (long)windowFloats * sizeof(float);
            return new(artifactId, frameBytes > 0L ? checked((int)(windows.Length / frameBytes)) : 0, windows.Length) {
                Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.Onnx, AllocationClass = AllocationClass.NativeOrt, Elapsed = elapsed,
            };
        }
    }

    static float[] Pool(ReadOnlySpan<float> hiddenStates, int hidden, Pooling pooling) {
        if (pooling.ClsSlice) { return hiddenStates[..hidden].ToArray(); }
        var pooled = new float[hidden];
        int tokens = hiddenStates.Length / Math.Max(1, hidden);
        for (int token = 0; token < tokens; token++) {
            TensorPrimitives.Add(pooled, hiddenStates.Slice(token * hidden, hidden), pooled);
        }
        if (tokens > 0) { TensorPrimitives.Divide(pooled, tokens, pooled); }
        return pooled;
    }

    static (int Class, float Probability)[] TopK(ReadOnlySpan<float> probability, int top) {
        var heap = new PriorityQueue<int, float>(top);
        for (int index = 0; index < probability.Length; index++) {
            if (heap.Count < top) { heap.Enqueue(index, probability[index]); }
            else if (heap.TryPeek(out _, out float worst) && probability[index] > worst) { heap.EnqueueDequeue(index, probability[index]); }
        }
        int kept = heap.Count;
        var ranked = new (int Class, float Probability)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { int cls = heap.Dequeue(); ranked[slot] = (cls, probability[cls]); }
        return ranked;
    }

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
            : new ComputeFault.ModelRejected(error.Message);

    extension(BoundFlow flow) {
        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySpan<float> payload, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            flow.Write(payload);
            return Bracket(scope, project, () => flow.Run(options));
        }

        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySequence<byte> window, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            flow.Write(window);
            return Bracket(scope, project, () => flow.Run(options));
        }
    }
}
```

## [03]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` with the four behaviour columns `Serves`/`CutsFirst`/`StoresPositive`/`StoresNegative` driving every posture and the derived `ReadThroughStore` fast-path predicate; `CacheOps` key derivation, echo-validated read-through, the precision-TTL negative-probe fresh path, and the `Cached<T>` echo envelope; the cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact at the sink edge — a second `CacheIndexFact` fact stream is the rejected form because `ComputeReceipt` is the package's only measured-fact vocabulary.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the cache-policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches under the `ReadThrough`/`Negative` rows rather than re-running every call, and `precision` sizes the negative TTL so a quantized run's failure expires faster than a full-precision one's.
- Auto: `Key` stamps model checksum, intent input digest, EP key, ORT version, and option-table hash in one call so cross-version numerical drift never serves as a deterministic hit; content-addressed dedup keys identical-input/identical-EP runs to one stored payload so two callers with byte-identical inputs share the produced value; `Through` dispatches on the row through the `ReadThroughStore` predicate — the full read-through path delegates to the `CacheLane.ModelResult` `Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the lane TTL), and every other row falls to `Fresh`; `Fresh` evicts first when `CutsFirst`, serves a cached negative through a `DisableUnderlyingData` cache-only probe when `Serves`, produces once, then stores the success under the result key when `StoresPositive` or the failure under the `neg:` key at the `ModelPrecision.NegativeTtl` when `StoresNegative` — every column reaches a live branch and no posture is a behavioural twin of another.
- Receipt: the cache outcome projects onto `ComputeReceipt.Cache(Outcome, Key, Bytes)` (`Outcome` ∈ `hit`/`miss`/`store`/`evict`) at the sink edge and the `CacheLane` `ReportTagMetrics` meters live hit/miss/evict by lane tag; `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated `Cached<Fin<T>>` echo mismatches the requested `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row with its four behaviour columns; a richer cache outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner here; zero new surface.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary and this fence carries the async statement forms of the read-through and fresh paths; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single cache owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect; cached payloads ride the `Cached<Fin<T>>` envelope whose `Echo` is the `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 bucket corruption (`ComputeFault.CacheCorrupt`) the content key alone cannot — a cache value stored without the echo is the rejected form; the `ReadThrough` row caches success and failure under one lane-TTL entry while the `Negative` row caches only the failure under the `ModelPrecision.NegativeTtl` and re-produces every success, so the two rows are behaviourally distinct and the negative-store branch is reachable — an identical-column twin of `ReadThrough` is the named defect; content-addressed dedup folds the input digest into the stored key so identical-input runs across distinct callers coalesce — a second dedup owner is the named defect; the cross-process result-reuse recency horizon is read by reference from the Persistence `ModelResultIndex` index owner — the single horizon owner — never minted here, so a second `Duration horizon` parameter beside the policy rows is the named defect; hit/miss/evict are HybridCache `ReportTagMetrics` consequences, never a second fact stream this page emits.

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
}

public readonly record struct Cached<T>(string Echo, T Value);

public static class CacheOps {
    public static ModelResultKey Key(ModelIdentity model, UInt128 inputDigest, ExecutionProvider ep, ModelPrecision precision) =>
        new(model.Key, inputDigest, ep.ResultKey(OrtEnv.Instance().GetVersionString(), precision));

    static Fin<T> Validated<T>(ModelResultKey key, Cached<Fin<T>> cached) =>
        StringComparer.Ordinal.Equals(cached.Echo, key.ModelChecksum)
            ? cached.Value
            : Fin.Fail<T>(new ComputeFault.CacheCorrupt(key.ToString()));

    extension(HybridCache cache) {
        public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default) =>
            policy.ReadThroughStore
                ? ServeStore(cache, key, state, produce, token)
                : Fresh(cache, policy, key, precision, state, produce, token);
    }

    static async ValueTask<Fin<T>> ServeStore<T, TState>(HybridCache cache, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) =>
        Validated(key, await cache.Read(
            CacheLane.ModelResult, key.ToString(),
            (Key: key, State: state, Produce: produce),
            static async (s, ct) => new Cached<Fin<T>>(s.Key.ModelChecksum, await s.Produce(s.State, ct)),
            Some(Seq(key.ModelChecksum)), token));

    static async ValueTask<Fin<T>> Fresh<T, TState>(HybridCache cache, CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) {
        if (policy.CutsFirst) { await cache.Remove(CacheLane.ModelResult, key.ToString(), token); }
        if (policy.Serves) {
            var probed = await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"neg:{key}"),
                static _ => new ValueTask<Cached<Fin<T>>>(default(Cached<Fin<T>>)),
                CacheLane.ModelResult.Entry with { Flags = HybridCacheEntryFlags.DisableUnderlyingData }, cancellationToken: token);
            if (probed.Echo is not null) { return Validated(key, probed); }
        }
        var value = await produce(state, token);
        if (value.IsSucc && policy.StoresPositive) {
            await cache.SetAsync(CacheLane.ModelResult.Scoped(key.ToString()), new Cached<Fin<T>>(key.ModelChecksum, value), CacheLane.ModelResult.Entry, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        else if (value.IsFail && policy.StoresNegative) {
            await cache.SetAsync(CacheLane.ModelResult.Scoped($"neg:{key}"), new Cached<Fin<T>>(key.ModelChecksum, value), CacheLane.ModelResult.Entry with { Expiration = precision.NegativeTtl.ToTimeSpan() }, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        return value;
    }
}
```

## [04]-[RESEARCH]

- [CANCELLATION]: the `RunOptions.Terminate` one-way latch aborts `Run`/`RunWithBinding` with the native `OnnxRuntimeException` `[ErrorCode:Fail] Exiting due to terminate flag being set to true`, which the `RunOps.Faulted` arm classifies by scope provenance into `ComputeFault.Cancelled`/`DeadlineExpired`; the open leaf is the latch-propagation latency and the deadline-poll cadence on the CoreML and CPU rows inside the live plugin ALC.
