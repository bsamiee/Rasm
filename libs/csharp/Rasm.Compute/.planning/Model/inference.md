# [COMPUTE_INFERENCE]

Rasm.Compute model inference: the OrtValue-only run-mode fold over the shared session with its `BoundLoop` shared-arena zero-allocation hot path and device-resident chaining, one polymorphic `RunInput` admission keyed on carrier shape, the vectorized `TensorPrimitives` reduction kernels (argmax, mean-pool, L2-normalize), the `System.Numerics.Tensors` carrier bridge, and the version-stamped deterministic result cache read-through over the produced run. The page owns the `RunConfig`/`RunInput` run vocabulary, the `RunOps` run-mode fold and its `BoundLoop` capsule, the `CachePolicy`/`CacheIndexFact` cache rows, and the `CacheOps` key-derivation and policy-dispatched read-through; the run surfaces ride `Microsoft.ML.OnnxRuntime` and the reductions `System.Numerics.Tensors`, the cache rides `Microsoft.Extensions.Caching.Hybrid`, the `BoundLoop` sink allocates from the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` arena, and the `ModelIdentity` from `Model/identity#MODEL_IDENTITY`, the `ExecutionProvider`/`ModelPrecision` from `Model/providers#EP_AXIS`, the `RunInput.Strings` egress completion from `Model/extension#EXTENSION_OPS`, the AppHost `CancelScope`/`ClockPolicy`, and the Persistence `ModelResultKey`/`ArtifactIndexRow`/vector-lane owners arrive settled. The `RunInput.Strings` ingress is the catalogued ingress the extension-ops string egress completes, and the `Embed` vector feeds the Persistence vector lane by reference.

## [01]-[INDEX]

- [01]-[INFERENCE_MODES]: `OrtValue`-only run modes; one polymorphic input admission; vectorized reductions; cancellation rail; profiling artifacts; `ModelRun` receipt; device-resident shared-arena chaining; `System.Numerics.Tensors` bridge.
- [02]-[RESULT_CACHE]: version-stamped deterministic keys; cache-policy rows; negative-result, content-addressed dedup, TTL-by-precision, and stampede facts.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` — the run-mode fold over the shared session: single, bound-batch, named bound, windowed, embedding, classification, clash-scoring, and S.N.Tensors-bridge runs discriminated by intent payload shape; `RunInput` one polymorphic input admission keyed on carrier shape; `BoundLoop` the shared-arena device-resident hot path.
- Cases: single `Run`; lane-enqueued async (the lane seam owns the thread hop — the native `RunAsync` requires pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so it is the rejected spelling); `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip projection arm; the `BoundLoop` steady-state hot path with its shared-arena `CreateAllocatedTensorValue` sink and device-resident `ClearBound*`/`BindOutputToDevice`/`CreateTensorValueWithData` rebind; `Chunked` streaming windows over chunked inputs through `RecyclableMemoryStream.GetReadOnlySequence`; `Embed` mean-pool/CLS-slice + L2-normalized text-to-vector projection over an embedding model; `Classify` `TensorPrimitives.IndexOfMax`-over-logits run for BIM point-cloud→element classification and symbol recognition over the interchange `PointScan` encoding; `ClashScore` scalar-output run for clash false-positive scoring over a candidate `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge over `CreateTensorValueFromSystemNumericsTensorObject` and `GetTensorDataAsTensorSpan<T>` materializing to a detached `TResult` inside the native bracket.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket.
- Auto: `Plan` wires deadline expiry into the `Terminate` one-way latch from the linked `CancelScope`, attaches LoRA adapters, and folds the `RunConfig` row table into `AddRunConfigEntry` calls so a posture change selects a row rather than editing the fence; one conversion arm (`Faulted`) classifies failures into `DeadlineExpired`/`Cancelled` by scope provenance — the single cancellation oracle every run shape rides; output buffers size from `GetTensorTypeAndShape().ElementCount`, never re-multiplied dimensions; `RunInput` admits the carrier polymorphically — a managed `T[]+shape` binds through the generic `CreateTensorValueFromMemory<T>`, a `System.Numerics.Tensors.Tensor<T>` binds through the generic `CreateTensorValueFromSystemNumericsTensorObject<T>`, and a `Tensor<string>` binds through `CreateFromStringTensor` — one `[Union]` admission whose per-case `Admit` override dispatches by carrier shape over the open unmanaged `T` (every `TensorDtype` carrier, not a closed float/long/int arm set that degenerate-throws on `double`/`byte`), never the three sibling `StringInput`/`Input<T>`/`TensorInput<T>` factories; `Classify` drives `TensorPrimitives.IndexOfMax` per logit slice (no hand-rolled argmax loop) and `Embed` mean-pools or CLS-slices then L2-normalizes via `TensorPrimitives.Norm` + `Divide`; the `ModelRun` receipt factory stamps route, batch, the `GetTensorSizeInBytes` peak footprint, the `GetTensorMemoryInfo` arena name, and the optional profile-artifact path; a sentinel/NaN egress projects to `Option` at the boundary.
- Receipt: the `ModelRun` receipt carries model checksum, EP, run mode, batch size, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `OrtMemoryInfo` allocator name from `GetTensorMemoryInfo` as `ArenaAllocator`, and the optional `EndProfiling` chrome-trace artifact path; profiling chrome-trace artifacts land as `ArtifactIndexRow.OnnxProfile` rows with the artifact path in the receipt.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload-shape case on the intent family; a new run-config posture is one `RunConfig` row carrying its `AddRunConfigEntry` key-value pairs and its `OrtAllocatorType` arena column; an embedding model is one more `Embed` run over the same session capsule and a BIM classifier, symbol recognizer, or clash false-positive scorer is one more `Classify`/`ClashScore` run over the shared session reusing the inference engine for the non-AI in-scope BIM pipelines (point-cloud→element classification consumes the interchange `PointScan` encoding, clash scoring consumes the `Solver/clash#CLASH_AND_TWIN` `ClashPair` feature vector), never a new model lane or a BIM-specific service; a tensor-lane handoff that already holds a `System.Numerics.Tensors.Tensor<T>` is one `InferTensor` run binding the carrier directly with zero managed copy and projecting to a detached value; zero new surface.
- Boundary: `RunOps` extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` boundary capsule and this fence carries bracketed statement forms with deterministic native disposal; OrtValue-only law — `NamedOnnxValue`, `DisposableNamedOnnxValue`, and `FixedBufferOnnxValue` are superseded spellings that never appear; `CreateTensorValueFromMemory` binds rented staging arrays without copies and the backing must outlive the value and every run binding it — the value's dispose IS the owner's release point; `CreateTensorValueFromSystemNumericsTensorObject<T>` admits the tensor-lane `Tensor<T>` owner directly so a tensor already in the compute interior never round-trips through a managed array, and `GetTensorDataAsTensorSpan<T>` reads egress as a `ReadOnlyTensorSpan<T>` consumed and reduced to a DETACHED `TResult` inside the native bracket (per `boundaries.md` REF_SAFE_PROJECTION) — the `ReadOnlyTensorSpan<T>` ref struct never crosses the `Fin` boundary because `shapes.md` UNIONS rejects a ref-struct generic type argument; the `Terminate` latch is the single cancellation propagation path and the deadline-poll cadence binds from the CANCELLATION research row; `InferBound` runs `RunWithBinding` over a populated `OrtIoBinding`, bracketing the run between `SynchronizeBoundInputs` and `SynchronizeBoundOutputs` and projecting `GetOutputValues`, with the optional `names` zip arm delivering the `RunWithBindingAndNames(RunOptions, OrtIoBinding, string[])` named-output convenience by pairing `GetOutputNames()` against the result without materializing the forbidden `IDisposableReadOnlyCollection<DisposableNamedOnnxValue>` that member returns — one `InferBound`, not a separate named sibling; the `BoundLoop` capsule is the zero-allocation steady-state posture for repeated same-shape inference — `CreateIoBinding`, `BindInput`/`BindOutput` once over the bound input and output `OrtValue`s both allocated from the SHARED arena `ModelSessions.SharedAllocator` leases (not `OrtAllocator.DefaultInstance` and not a managed staging plane), the steady-state write is `payload.CopyTo(bound.GetTensorMutableDataAsSpan<float>())` directly into the bound value per `Pulse` (the catalogued IO-binding mutable-span write, no managed `MemoryOwner<float>` staging copy), and `RunWithBinding` per `Pulse` with no per-call marshal — and a shape-class transition rebinds through `ClearBoundInputs`/`ClearBoundOutputs` with `BindOutputToDevice` routing device outputs, the device-input `BindInput(string, TensorElementType, long[], OrtMemoryAllocation)` overload binding a shared-arena device buffer, and the raw-device-pointer input path `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` admitting a device pointer directly; `Chunked` reads the chunked input through `RecyclableMemoryStream.GetReadOnlySequence` (zero-copy) and drives one `BoundLoop.Pulse` per window over a scoped span sliced from the sequence with no re-materialization, emitting the `streaming` `ProgressPhase` and a `StreamSegment` receipt per chunk, never a hand-rolled contiguous frame and never a double `.ToArray()`; `Embed` runs one inference over an embedding model and mean-pools the last-hidden-state (or CLS-slices) then L2-normalizes via `TensorPrimitives.Norm`+`Divide` to a `float[]` vector feeding the Persistence vector lane by reference, keying on the content-hash and model-id reuse identity the Persistence owner holds — a raw last-token passthrough is the rejected form; the `RunConfig.Arena` column classifies the run allocator through `OrtAllocatorType` (`ArenaAllocator` steady, `DeviceAllocator` device-resident); `Profile` is guarded on `policy.Profiling` and returns `Fin<ArtifactIndexRow>` so a no-profile session never reads a nonexistent trace path; output projection scopes native memory inside `project` and sentinel or NaN values project to `Option` at the boundary, never inward.

```csharp signature
public sealed record RunConfig(FrozenDictionary<string, string> Entries, OrtAllocatorType Arena) {
    public static readonly RunConfig Steady = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.ArenaAllocator);
    public static RunConfig Bulk(string arenaShrinkDevice) => new(new Dictionary<string, string>(StringComparer.Ordinal) {
        ["memory.enable_memory_arena_shrinkage"] = arenaShrinkDevice,
    }.ToFrozenDictionary(StringComparer.Ordinal), OrtAllocatorType.ArenaAllocator);
    public static readonly RunConfig Device = new(FrozenDictionary<string, string>.Empty, OrtAllocatorType.DeviceAllocator);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RunInput {
    private RunInput() { }
    public sealed record Managed<T>(string Name, T[] Data, long[] Shape) : RunInput where T : unmanaged {
        public override (string Name, OrtValue Value) Admit() => (Name, OrtValue.CreateTensorValueFromMemory(Data, Shape));
    }
    public sealed record Carrier<T>(string Name, Tensor<T> Tensor) : RunInput where T : unmanaged {
        public override (string Name, OrtValue Value) Admit() => (Name, OrtValue.CreateTensorValueFromSystemNumericsTensorObject(Tensor));
    }
    public sealed record Strings(string Name, Tensor<string> Tokens) : RunInput {
        public override (string Name, OrtValue Value) Admit() => (Name, OrtValue.CreateFromStringTensor(Tokens));
    }

    public abstract (string Name, OrtValue Value) Admit();
}

public static class RunOps {
    public static RunOptions Plan(CancelScope scope, RunConfig config, Option<OrtLoraAdapter> lora = default) {
        var options = new RunOptions();
        lora.Iter(options.AddActiveLoraAdapter);
        config.Entries.Iter(entry => options.AddRunConfigEntry(entry.Key, entry.Value));
        scope.Source.Token.Register(() => options.Terminate = true);
        return options;
    }

    public static Seq<(string Name, OrtValue Value)> Bind(params ReadOnlySpan<RunInput> inputs) =>
        toSeq(inputs.ToArray()).Map(static input => input.Admit());

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
                ? Fin.Succ(ArtifactIndexRow.Admit(ArtifactIndexRow.OnnxProfile, path, File.ReadAllBytes(path), classification, retentionClass, at))
                : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected("profiling-disabled"));

        public Fin<Seq<T>> Chunked<T>(RunOptions options, CancelScope scope, BoundLoop loop, ReadOnlySequence<byte> windows, int windowFloats, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) =>
            toSeq(Frames(windows, windowFloats)).TraverseM(window => loop.Pulse(options, scope, window.Span, project)).As();

        public Fin<float[]> Embed(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, int hidden) =>
            session.Infer(options, scope, inputs, Seq(output), results => {
                var hiddenStates = results.First().GetTensorDataAsSpan<float>();
                var pooled = MeanPool(hiddenStates, hidden);
                var norm = TensorPrimitives.Norm<float>(pooled);
                if (norm <= 0f) { return Fin.Fail<float[]>(new ComputeFault.ModelRejected("embed-zero-norm")); }
                TensorPrimitives.Divide(pooled, norm, pooled);
                return Fin.Succ(pooled);
            });

        public Fin<TResult> InferTensor<T, TResult>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string output, Func<ReadOnlyTensorSpan<T>, Fin<TResult>> project) where T : unmanaged =>
            session.Infer(options, scope, inputs, Seq(output), results => project(results.First().GetTensorDataAsTensorSpan<T>()));

        public Fin<Seq<(int Class, float Score)>> Classify(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, string logits, int classes) =>
            session.Infer(options, scope, inputs, Seq(logits), results => {
                var scores = results.First().GetTensorDataAsSpan<float>();
                int rows = scores.Length / Math.Max(1, classes);
                return Fin.Succ(toSeq(Enumerable.Range(0, rows)).Map(row => {
                    var slice = scores.Slice(row * classes, classes);
                    int arg = TensorPrimitives.IndexOfMax(slice);
                    return (arg, slice[arg]);
                }));
            });

        public Fin<float> ClashScore(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> features, string output) =>
            session.Infer(options, scope, features, Seq(output), static results => {
                var scores = results.First().GetTensorDataAsSpan<float>();
                return float.IsNaN(scores[0]) ? Fin.Fail<float>(new ComputeFault.ModelRejected("clash-nan")) : Fin.Succ(scores[0]);
            });

        public ComputeReceipt.ModelRun RunReceipt(ModelIdentity model, ExecutionProvider ep, string mode, int batch, OrtValue output, CorrelationId correlation, Substrate substrate, Option<string> profile, Duration elapsed) =>
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()), output.GetTensorMemoryInfo().Name, profile.IfNone((string?)null)) {
                Correlation = correlation, Lane = WorkLane.Background, Substrate = substrate, AllocationClass = AllocationClass.NativeOrt, Elapsed = elapsed,
            };
    }

    static float[] MeanPool(ReadOnlySpan<float> hiddenStates, int hidden) {
        var pooled = new float[hidden];
        int tokens = hiddenStates.Length / Math.Max(1, hidden);
        for (int token = 0; token < tokens; token++) {
            TensorPrimitives.Add(pooled, hiddenStates.Slice(token * hidden, hidden), pooled);
        }
        if (tokens > 0) { TensorPrimitives.Divide(pooled, tokens, pooled); }
        return pooled;
    }

    static IEnumerable<ReadOnlyMemory<float>> Frames(ReadOnlySequence<byte> windows, int windowFloats) {
        int frameBytes = windowFloats * sizeof(float);
        long frames = windows.Length / frameBytes;
        for (long index = 0; index < frames; index++) {
            var segment = windows.Slice(index * frameBytes, frameBytes);
            var owner = MemoryOwner<float>.Allocate(windowFloats);
            segment.CopyTo(MemoryMarshal.AsBytes(owner.Span));
            yield return owner.Memory;
        }
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
            : Error.New(error);

    public sealed class BoundLoop : IDisposable {
        readonly InferenceSession session;
        readonly OrtIoBinding binding;
        readonly OrtAllocator arena;
        readonly OrtValue bound, sink;

        public BoundLoop(InferenceSession session, OrtAllocator arena, string input, string output, long[] shape) {
            this.session = session;
            this.arena = arena;
            bound = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
            sink = OrtValue.CreateAllocatedTensorValue(arena, TensorElementType.Float, shape);
            binding = session.CreateIoBinding();
            binding.BindInput(input, bound);
            binding.BindOutput(output, sink);
        }

        public Fin<T> Pulse<T>(RunOptions options, CancelScope scope, ReadOnlySpan<float> payload, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project) {
            payload.CopyTo(bound.GetTensorMutableDataAsSpan<float>());
            return Bracket(scope, project, () => { binding.SynchronizeBoundInputs(); session.RunWithBinding(options, binding); binding.SynchronizeBoundOutputs(); return binding.GetOutputValues(); });
        }

        public void RebindDevice(string input, string output, TensorElementType dtype, long[] shape, OrtMemoryAllocation device, OrtMemoryInfo deviceInfo) {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(input, dtype, shape, device);
            binding.BindOutputToDevice(output, deviceInfo);
        }

        public void RebindDevicePointer(string input, string output, TensorElementType dtype, long[] shape, OrtMemoryInfo deviceInfo, nint pointer, long bytes) {
            binding.ClearBoundInputs();
            binding.ClearBoundOutputs();
            binding.BindInput(input, OrtValue.CreateTensorValueWithData(deviceInfo, dtype, shape, pointer, bytes));
            binding.BindOutputToDevice(output, deviceInfo);
        }

        public void Dispose() { binding.Dispose(); bound.Dispose(); sink.Dispose(); }
    }
}
```

## [03]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` serve/store/cut/negative rows; `CacheOps` key derivation, checksum-echo validation, content-addressed dedup, and the policy-dispatched read-through; `CacheIndexFact` the one fact stream carrying hit/miss/evict/negative/dedup/stampede slots.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<T> Through<T, TState>(CachePolicy policy, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the cache-policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches under the `Negative` row rather than re-running every call.
- Auto: `Key` stamps model checksum, intent input digest, EP key, ORT version, and option-table hash in one call so cross-version numerical drift never serves as a deterministic hit; content-addressed dedup keys identical-input/identical-EP runs to one stored payload so two callers with byte-identical inputs share the produced value; stampede single-flight, negative-result caching under a `NegativeTtl` derived from `ModelPrecision` (a quantized run's failure expires faster than a full-precision one's), lane tags, and hit/miss/negative/dedup/stampede facts ride the composed index surface with zero call-site ceremony.
- Receipt: `CacheIndexFact` hit/miss/evict/negative/dedup/stampede facts with byte sizes; `Validated` faults `CacheCorrupt` on checksum-echo mismatch at read.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row; a new fact slot is one `CacheIndexFact` kind, never a parallel fact owner; zero new surface.
- Boundary: `CacheOps` extends the cache boundary capsule and this fence carries the async statement forms of the fresh path; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single cache owner and hand-rolled memoization beside it is the named defect; cached payloads carry the checksum echo that `Validated` checks before projection; negative results store under the `Negative` row keyed identically so a deterministic re-fault serves the cached failure rather than re-running, with TTL-by-precision derived from `ModelPrecision.NegativeTtl` and never a second negative-cache owner; content-addressed dedup folds the input digest into the stored key so identical-input runs across distinct callers coalesce — a second dedup owner is the named defect; the cross-process result-reuse recency horizon is read by reference from the Persistence `ModelResultKey` index owner — the single horizon owner — never minted here, so a second `Duration horizon` parameter beside the policy rows is the named defect.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class CachePolicy {
    public static readonly CachePolicy Bypass = new("bypass", serveHits: false, stores: false, cutsFirst: false, cachesNegative: false);
    public static readonly CachePolicy ReadThrough = new("read-through", serveHits: true, stores: true, cutsFirst: false, cachesNegative: true);
    public static readonly CachePolicy WriteThrough = new("write-through", serveHits: false, stores: true, cutsFirst: false, cachesNegative: false);
    public static readonly CachePolicy Refresh = new("refresh", serveHits: false, stores: true, cutsFirst: true, cachesNegative: false);
    public static readonly CachePolicy Negative = new("negative", serveHits: true, stores: true, cutsFirst: false, cachesNegative: true);

    public bool ServeHits { get; }
    public bool Stores { get; }
    public bool CutsFirst { get; }
    public bool CachesNegative { get; }
}

public static class CacheOps {
    public static ModelResultKey Key(ModelIdentity model, UInt128 inputDigest, ExecutionProvider ep, ModelPrecision precision) =>
        new(model.Key, inputDigest, ep.ResultKey(OrtEnv.Instance().GetVersionString(), precision));

    public static Fin<T> Validated<T>(ModelResultKey key, string echo, T value) =>
        StringComparer.Ordinal.Equals(echo, key.ModelChecksum)
            ? Fin.Succ(value)
            : Fin.Fail<T>(new ComputeFault.CacheCorrupt(key.ToString()));

    extension(HybridCache cache) {
        public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default) =>
            policy.ServeHits
                ? cache.Result(key, state, produce, token)
                : Fresh(cache, policy, key, precision, state, produce, token);
    }

    static async ValueTask<Fin<T>> Fresh<T, TState>(HybridCache cache, CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) {
        if (policy.CutsFirst) {
            await cache.Remove(CacheLane.ModelResult, key.ToString(), token);
        }
        var value = await produce(state, token);
        if (value.IsSucc && policy.Stores) {
            await cache.SetAsync(CacheLane.ModelResult.Scoped(key.ToString()), value, CacheLane.ModelResult.Entry, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        else if (value.IsFail && policy.CachesNegative) {
            await cache.SetAsync(CacheLane.ModelResult.Scoped($"neg:{key}"), value, CacheLane.ModelResult.Entry with { Expiration = precision.NegativeTtl.ToTimeSpan() }, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        return value;
    }
}
```

## [04]-[RESEARCH]

- [CANCELLATION]: the `RunOptions.Terminate` one-way latch aborts `Run`/`RunWithBinding` with the native `OnnxRuntimeException` `[ErrorCode:Fail] Exiting due to terminate flag being set to true`, which the `RunOps.Faulted` arm classifies by scope provenance into `ComputeFault.Cancelled`/`DeadlineExpired`; the open leaf is the latch-propagation latency and the deadline-poll cadence on the CoreML and CPU rows inside the live plugin ALC.
