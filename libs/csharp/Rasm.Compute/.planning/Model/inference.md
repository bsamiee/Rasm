# [COMPUTE_INFERENCE]

Rasm.Compute model inference: `RunOps` folds every `OrtValue`-only run mode over the one shared session — single, bound-batch, windowed, embedding, classification, clash-scoring, tensor-bridge — with `RunInput` admitting the operand polymorphically on carrier shape and the vectorized `TensorPrimitives` reductions serving softmax top-k, mean/CLS pooling, and L2-normalize. `CacheOps` reads a version-stamped deterministic result cache through over each produced run. `RunConfig`/`RunInput`/`Pooling` own the run vocabulary; `CachePolicy`/`CacheOps` own the cache, projecting its outcome onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact.

Run surfaces ride `Microsoft.ML.OnnxRuntime`, reductions `System.Numerics.Tensors`, the cache `Microsoft.Extensions.Caching.Hybrid`. `BoundFlow` bound input and sink allocate from the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` arena; `ModelIdentity` (`Model/identity#MODEL_IDENTITY`), `ExecutionProvider`/`ModelPrecision` (`Model/providers#EP_AXIS`), the `RunInput.Strings` egress completion (`Model/extension#EXTENSION_OPS`), AppHost `CancelScope`/`ClockPolicy`, and the Persistence `ModelResultKey`/`ModelResultIndex`/`ArtifactIndexRow`/vector-lane owners arrive settled. `RunInput.Strings` is the ingress the extension-ops string egress completes; `Embed` feeds the Persistence vector lane by reference.

## [01]-[INDEX]

- [01]-[INFERENCE_MODES]: every `OrtValue`-only run mode folded over the shared session, one polymorphic input admission feeding the vectorized reductions.
- [02]-[RESULT_CACHE]: version-stamped deterministic keys and column-driven policy rows over an echo-validated single-flight read-through.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` folds every run mode over the shared session; `RunInput` admits one operand polymorphically on carrier shape through the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; a `BoundFlow` capsule composes the shared-arena device-resident hot path.
- Cases: `Infer` single run; `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip arm; `BoundFlow` the arena-allocated device-resident steady state; `Chunked` streaming windows over a `RecyclableMemoryStream.GetReadOnlySequence` view; `Embed` mean-pool/CLS text-to-vector; `Classify` softmax top-`k` over the interchange `PointScan` encoding; `ClashScore` scalar clash false-positive scoring over a `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge. Native async `RunAsync` is the rejected spelling — it demands pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so the lane seam owns the thread hop.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket.
- Auto: `Plan` folds the `RunConfig` row table into `AddRunConfigEntry` and registers the `Terminate` latch off the linked `CancelScope`, so a posture change selects a row; `Faulted` is the single fault oracle — it classifies by scope provenance into `DeadlineExpired`/`Cancelled` and lifts a non-cancellation native fault to `ModelRejected`, never a raw `Error.New` leaking an unclassified native exception. Output buffers size from `GetTensorTypeAndShape().ElementCount`, never re-multiplied dimensions. `RunInput` composes the `TensorBridge.Ingress` overloads (the sole `OrtValue` C-data factory owner) over the open unmanaged `T` — every `TensorDtype` carrier, not a closed float/long/int arm that throws on `double`/`byte`; the ONNX-owned `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` and the `System.Numerics.Tensors.Tensor<T>` are distinct types that never unify, so each rides its own case. `Classify` selects each row's top-`k` through a bounded `PriorityQueue` min-heap (`O(width·log k)`, never a full-taxonomy `Array.Sort`) and materializes per row through an explicit span walk — no `ReadOnlySpan<float>` captured into a lambda, the named kernel exemption.
- Receipt: `ModelRun` carries model checksum, EP, run mode, batch, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `GetTensorMemoryInfo` allocator name as `ArenaAllocator`, and the optional `EndProfiling` chrome-trace path; profiling artifacts land as `ArtifactKind.OnnxProfile` rows.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload case; a new run-config posture is one `RunConfig` row with its `AddRunConfigEntry` pairs and `OrtAllocatorType` arena column; a new pooling shape is one `Pooling` row folded into `Pool`; a wider classifier candidate set is the `Classify` `top` arity; a `ZipMap` sequence-of-maps classifier reads through the `Model/extension#EXTENSION_OPS` `Egress`→`OpOutput.Batched` reader, never a parallel arm on `Classify` (whose softmax owns the numeric-logit shape only); a BIM point-cloud→element classifier, symbol recognizer, or clash scorer is one more `Classify`/`ClashScore` run over the shared session — consuming the interchange `PointScan` encoding and the `Solver/clash#CLASH_AND_TWIN` `ClashPair` vector — never a BIM-specific service; a tensor-lane handoff already holding a `Tensor<T>` is one `InferTensor` run with zero managed copy.
- Boundary: `RunOps` extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` capsule with bracketed native disposal. OrtValue-only law: `NamedOnnxValue`, `DisposableNamedOnnxValue`, and `FixedBufferOnnxValue` are superseded and never appear. `CreateTensorValueFromMemory` binds rented staging without a copy — the backing must outlive the value and every run binding it, and the value's dispose is the owner's release point. `CreateTensorValueFromSystemNumericsTensorObject<T>` admits a tensor-lane `Tensor<T>` directly with no managed round-trip, and `GetTensorDataAsTensorSpan<T>` reduces egress to a DETACHED `TResult` inside the native bracket (`boundaries.md` REF_SAFE_PROJECTION); the `ReadOnlyTensorSpan<T>` ref struct never crosses the `Fin` boundary because `shapes.md` UNIONS rejects a ref-struct generic argument. `InferBound`'s optional named arm pairs `GetOutputNames()` against `RunWithBindingAndNames(RunOptions, OrtIoBinding, string[])` without materializing the forbidden `IDisposableReadOnlyCollection<DisposableNamedOnnxValue>` that member returns — one `InferBound`, never a named sibling. `BoundFlow` binds input and sink from the `ModelSessions.SharedAllocator` arena through `CreateAllocatedTensorValue` (never `OrtAllocator.DefaultInstance`, never a managed staging plane); `Pulse` writes through `GetTensorMutableDataAsSpan<float>()` with no staging copy, and a shape-class transition rebinds through `RebindDevice` (`BindInput(string, TensorElementType, long[], OrtMemoryAllocation)` plus `BindOutputToDevice`) or `RebindDevicePointer` (`CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)`). `Chunked` copies each sliced window straight into the bound value through `ReadOnlySequence<byte>.CopyTo(GetTensorMutableRawData())` — the same single copy the float write performs, never an intermediate `MemoryOwner<float>` or `.ToArray()` — advances the `Runtime/progress#PROGRESS_CELL` cell to `Streaming`, and projects one `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.StreamSegment` at the sink edge, never one receipt per window. `Embed` L2-normalizes the pooled vector to a `float[]` feeding the Persistence vector lane by reference, keyed on the content-hash and model-id reuse identity the Persistence owner holds — a raw last-token passthrough and a `Pooling`-blind hardcoded mean-pool are the rejected forms, since the `Model/embedding#EMBEDDING` consumer reads both pooling shapes. `Profile` guards on `policy.Profiling` and returns `Fin<ArtifactIndexRow>`, so a no-profile session never reads a nonexistent trace. Output projection scopes native memory inside `project`; a sentinel or NaN value projects to `Option` at the boundary, never inward.

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

- Owner: `CachePolicy` `[SmartEnum<string>]` — four behaviour columns (`Serves`/`CutsFirst`/`StoresPositive`/`StoresNegative`) drive every posture through the derived `ReadThroughStore` predicate; `CacheOps` owns key derivation, echo-validated read-through, the precision-TTL negative probe, and the `Cached<T>` envelope. Cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact — a second fact stream is rejected, `ComputeReceipt` being the package's only measured-fact vocabulary.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches rather than re-running every call, and `precision` sizes the negative TTL.
- Auto: `Key` stamps model checksum, input digest, EP key, ORT version, and option-table hash so cross-version drift never serves a stale hit; content-addressed dedup coalesces byte-identical-input/identical-EP runs to one stored payload. `Through` dispatches on the `ReadThroughStore` predicate — the read-through path delegates to `CacheLane.ModelResult` `Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the lane TTL), and every other row falls to `Fresh`, which evicts first when `CutsFirst`, serves a cached negative through a `DisableUnderlyingData` cache-only probe when `Serves`, produces once, then stores the success under the result key or the failure under the `neg:` key at `ModelPrecision.NegativeTtl` — every column reaches a live branch, no posture a twin of another.
- Receipt: outcome projects onto `ComputeReceipt.Cache(Outcome, Key, Bytes)` (`Outcome` ∈ `hit`/`miss`/`store`/`evict`) at the sink edge; `CacheLane.ReportTagMetrics` meters live hit/miss/evict by lane tag; `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated echo mismatches `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row with its four columns; a richer outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect. Cached payloads ride the `Cached<Fin<T>>` envelope whose `Echo` is `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 corruption the content key alone cannot; a value stored without the echo is rejected. `ReadThrough` caches success and failure under one lane-TTL entry while `Negative` caches only the failure at `ModelPrecision.NegativeTtl` and re-produces every success — behaviourally distinct rows, so an identical-column twin of `ReadThrough` is the named defect. Content-addressed dedup folds the input digest into the stored key so identical-input runs across callers coalesce; a second dedup owner is rejected. A cross-process result-reuse recency horizon reads by reference from the Persistence `ModelResultIndex` owner — a second `Duration horizon` parameter beside the policy rows is the named defect. Hit/miss/evict are HybridCache `ReportTagMetrics` consequences, never a second fact stream.

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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [CANCELLATION]-[OPEN]: `RunOptions.Terminate=true` aborts `Run`/`RunWithBinding` by throwing the native `OnnxRuntimeException [ErrorCode:Fail] Exiting due to terminate flag being set to true` that `Faulted` reclassifies by scope provenance — what is the latch-propagation latency and safe deadline-poll cadence for the `CoreMl`/`Cpu` rows inside the live plugin ALC; measure against a running `InferenceSession` under the ORT host.
