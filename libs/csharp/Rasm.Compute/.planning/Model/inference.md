# [COMPUTE_INFERENCE]

Rasm.Compute model inference: `RunOps` folds every `OrtValue`-only run mode over the one shared session — single, bound-batch, windowed, embedding, classification, clash-scoring, tensor-bridge — with `RunInput` admitting the operand polymorphically on carrier shape, the run bracket owning both native legs (admitted inputs and produced results), and the vectorized `TensorPrimitives` reductions serving softmax top-k, mean/CLS/last/max pooling, and L2-normalize. `BatchGate` coalesces concurrent single-row runs on one shared session into one batched execution and fans results back per caller. `CacheOps` reads a version-stamped deterministic result cache around each produced run. `RunConfig`/`RunInput`/`Pooling`/`PlannedRun`/`BatchPolicy` own the run vocabulary; `CachePolicy`/`CacheOps` own the cache, projecting its outcome onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact.

Run surfaces ride `Microsoft.ML.OnnxRuntime`, reductions `System.Numerics.Tensors`, the cache `Microsoft.Extensions.Caching.Hybrid`. `BoundFlow` bound input and sink allocate from the `Model/sessions#SESSION_CAPSULE` `SharedAllocator` arena; `ModelIdentity`/`GraduationEnvelope`/`DriftVerdict` (`Model/identity#MODEL_IDENTITY`), `ExecutionProvider`/`ModelPrecision` (`Model/providers#EP_AXIS`), the `RunInput.Strings` egress completion (`Model/extension#EXTENSION_OPS`), AppHost `CancelScope`/`ClockPolicy`, and the Persistence `ModelResultKey`/`ModelResultIndex`/`ArtifactIndexRow`/vector-lane owners arrive settled. `RunInput.Strings` is the ingress the extension-ops string egress completes; `Embed` feeds the Persistence vector lane by reference.

## [01]-[INDEX]

- [01]-[INFERENCE_MODES]: every `OrtValue`-only run mode folded over the shared session, one polymorphic input admission feeding the vectorized reductions, the two-leg native bracket, and the cross-request batching gate.
- [02]-[RESULT_CACHE]: version-stamped deterministic keys and column-driven policy rows over an echo-validated single-flight read-through with drift-gated invalidation.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` folds every run mode over the shared session; `RunInput` admits one operand polymorphically on carrier shape through the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; `PlannedRun` owns the `RunOptions` + `Terminate`-latch registration pair; a `BoundFlow` capsule composes the shared-arena device-resident hot path; `BatchGate` the bounded-window cross-request coalescer over one shared session.
- Cases: `Infer` single run; `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip arm; `BoundFlow` the arena-allocated device-resident steady state; `Chunked` streaming windows over a `RecyclableMemoryStream.GetReadOnlySequence` view; `Embed` mean/CLS/last/max-pool text-to-vector; `Classify` softmax top-`k` over the interchange `PointScan` encoding; `ClashScore` scalar clash false-positive scoring over a `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge; `BatchGate.Submit` the coalesced single-row shape. Native async `RunAsync` is the rejected spelling — it demands pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so the lane seam owns the thread hop.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket, and the bracket CONSUMES the admitted inputs: one run is one deterministic release for both native legs, so a repeated same-shape loop rides `BoundFlow`, never re-admitted one-shot inputs.
- Auto: `Plan` folds the `RunConfig` row table into `AddRunConfigEntry` and returns a `PlannedRun` capsule pairing the `RunOptions` with the `Terminate`-latch `CancellationTokenRegistration` off the linked `CancelScope` — the registration disposes with the capsule, so a latch firing into a disposed `RunOptions` is structurally impossible and a bare `Token.Register` whose registration nobody owns is the named use-after-free; `Faulted` is the single fault oracle — it classifies by scope provenance into `DeadlineExpired`/`Cancelled` and lifts a non-cancellation native fault to `ModelRejected`, never a raw `Error.New` leaking an unclassified native exception. Output buffers size from `GetTensorTypeAndShape().ElementCount`, never re-multiplied dimensions. `RunInput` composes the `TensorBridge.Ingress` overloads (the sole dense `OrtValue` C-data factory owner) over the open unmanaged `T`; ONNX-owned strings and preconstructed sparse `OrtValue`s ride distinct cases, with sparse ownership transferring only after `IsSparseTensor` proves the carrier. `Pooling` carries each reduction as its delegate-backed row, and `Embed` admits only an unbatched hidden-state tensor, so a multi-row output never collapses unrelated embeddings into one vector. `Classify` selects each row's top-`k` through a class-stable bounded `PriorityQueue` min-heap (`O(width·log k)`, never a full-taxonomy `Array.Sort`) and materializes per row through an explicit span walk — no `ReadOnlySpan<float>` captured into a lambda, the named kernel exemption. `BatchGate` snapshots each admitted row, caps queued mass at `BatchPolicy.MaxPending`, and packs rows arriving within `MaxDelay` up to `MaxRows` into one bound execution using the injected `TimeProvider` — the bound input stays shaped `[MaxRows, rowWidth]`, a partial window zero-pads its tail rows, and fan-out returns only the submitted rows, because rows are independent under a feed-forward per-row model and a variable-shape rebind per window is the rejected form; per-call ORT dispatch overhead dominates small-tensor inference, so the screening loops the charter names ride one packed run per window instead of thousands of singletons.
- Receipt: `ModelRun` carries model checksum, EP, run mode, batch, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `GetTensorMemoryInfo` allocator name as `ArenaAllocator`, and the optional `EndProfiling` chrome-trace path; profiling artifacts land as `ArtifactKind.OnnxProfile` rows. A `BatchGate` window emits ONE `ModelRun` whose `BatchSize` is the window's submitted row count (zero-padded tail rows never count) — per-submitter receipt fan-out is the rejected form.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload case; a new run-config posture is one `RunConfig` row with its `AddRunConfigEntry` pairs and `OrtAllocatorType` arena column; a new pooling shape is one delegate-backed `Pooling` row; a wider classifier candidate set is the `Classify` `top` arity; a batching posture is one `BatchPolicy` value, never a second coalescer; a `ZipMap` sequence-of-maps classifier reads through the `Model/extension#EXTENSION_OPS` `Egress`→`OpOutput.Batched` reader, never a parallel arm on `Classify` (whose softmax owns the numeric-logit shape only); a BIM point-cloud→element classifier, symbol recognizer, or clash scorer is one more `Classify`/`ClashScore` run over the shared session — consuming the interchange `PointScan` encoding and the `Solver/clash#CLASH_AND_TWIN` `ClashPair` vector — never a BIM-specific service; a tensor-lane handoff already holding a `Tensor<T>` is one `InferTensor` run with zero managed copy.
- Boundary: `RunOps` extends `Model/sessions#SESSION_CAPSULE` with bracketed native disposal. `CreateTensorValueFromMemory` binds rented staging without a copy; input ownership transfers at the run, and `Bracket` disposes every admitted input beside the result collection. `InferBound` calls the `OrtValue`-only `RunWithBoundResults` member directly; its named arm zips `GetOutputNames()` against that same collection and never materializes `DisposableNamedOnnxValue`. Every projection proves a nonempty output collection before `First()`. `BoundFlow` binds input and sink from `ModelSessions.SharedAllocator`, and `Pulse` writes through the mutable native span without staging. `Chunked` copies each `ReadOnlySequence<byte>` window into the bound value and emits one terminal `StreamSegment`. `Embed` derives its final axis from output shape and L2-normalizes the pooled vector; `Classify` derives class width, proves row divisibility, and uses bounded top-`k`; `BatchGate` proves packed output cardinality before fan-out. `Profile` admits its artifact through `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)`, grouping the trace under the profiled model checksum; retention derives from `ArtifactKind.Retention`.

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

public static class RunOps {
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
            Bracket(scope, inputs, project, () => session.Run(options, inputs.Map(static row => row.Name), inputs.Map(static row => row.Value), outputs));

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
                        return results;
                    }
                    catch {
                        results.Dispose();
                        throw;
                    }
                });

        public Fin<ArtifactIndexRow> Profile(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, Instant at) =>
            !policy.Profiling
                ? Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected("profiling-disabled"))
                : Try.lift(() => session.EndProfiling() is string path
                        ? Fin.Succ(ArtifactIndexRow.Admit(ArtifactKind.OnnxProfile, path, File.ReadAllBytes(path), classification, at, Some(sourceKey)))
                        : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected("profiling-path-missing")))
                    .Run()
                    .MapFail(error => new ComputeFault.ModelRejected(error.Message))
                    .Bind(identity);

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

        public ComputeReceipt.ModelRun RunReceipt(ModelIdentity model, ExecutionProvider ep, string mode, int batch, OrtValue output, CorrelationId correlation, Option<string> profile, Duration elapsed) =>
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()), output.GetTensorMemoryInfo().Name, profile.IfNoneUnsafe((string?)null)) {
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

    // Ownership transfers at the run: the bracket's completion is the ONE deterministic release for admitted inputs and produced results alike.
    static Fin<T> Bracket<T>(CancelScope scope, Seq<(string Name, OrtValue Value)> owned, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project, Func<IDisposableReadOnlyCollection<OrtValue>> run) {
        IDisposableReadOnlyCollection<OrtValue>? results = null;
        try {
            results = run();
            return project(results);
        }
        catch (OnnxRuntimeException error) {
            return Fin.Fail<T>(Faulted(scope, error));
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or OverflowException) {
            return Fin.Fail<T>(new ComputeFault.ModelRejected(error.Message));
        }
        finally {
            results?.Dispose();
            owned.Iter(static row => row.Value.Dispose());
        }
    }

    static Error Faulted(CancelScope scope, OnnxRuntimeException error) =>
        scope.Source.Token.IsCancellationRequested
            ? scope.Deadline is { IsSome: true, Case: CancellationTokenSource expired } && expired.IsCancellationRequested
                ? new ComputeFault.DeadlineExpired(scope.Provenance)
                : new ComputeFault.Cancelled(scope.Provenance)
            : new ComputeFault.ModelRejected(error.Message);

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
                // The bound input is shaped [MaxRows, rowWidth] once at Bind: a partial window zero-pads the tail rows and fans back only the submitted ones.
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

## [03]-[RESULT_CACHE]

- Owner: `CachePolicy` `[SmartEnum<string>]` — four behaviour columns (`Serves`/`CutsFirst`/`StoresPositive`/`StoresNegative`) drive every posture through the derived `ReadThroughStore` predicate; `CacheOps` owns key derivation, echo-validated read-through, the precision-TTL negative probe, and the `Cached<T>` envelope. Cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact — a second fact stream is rejected, `ComputeReceipt` being the package's only measured-fact vocabulary.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches rather than re-running every call, `precision` sizes the negative TTL, and present drift evidence is an input-shape discriminant rather than an independently reconstructed monitor.
- Auto: `Key` stamps model checksum, input digest, EP key, ORT version, and option-table hash so cross-version drift never serves a stale hit; content-addressed dedup coalesces byte-identical-input/identical-EP runs to one stored payload. `Through` first consumes a `DriftVerdict.Breached` by evicting both result and negative keys and returning `ComputeFault.EquivalenceMiss`, then dispatches on the `ReadThroughStore` predicate — the read-through path delegates to `CacheLane.ModelResult` `Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the lane TTL), and every other row falls to `Fresh`, which evicts both keys when `CutsFirst`, serves a cached negative through an `Option<Cached<Fin<T>>>` `DisableUnderlyingData` cache-only probe, produces once, clears stale negative evidence before a positive write, then stores the success under the result key or the failure under the `neg:` key at `ModelPrecision.NegativeTtl` — every column reaches a live branch, no posture a twin of another.
- Receipt: outcome projects onto `ComputeReceipt.Cache(Outcome, Key, Bytes)` (`Outcome` ∈ `hit`/`miss`/`store`/`evict`) at the sink edge; `CacheLane.ReportTagMetrics` meters live hit/miss/evict by lane tag; `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated echo mismatches `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row with its four columns; a richer outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner; a graduated-model validity axis is the `Model/identity#MODEL_IDENTITY` drift sentinel consumed here, never a sibling monitor.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect. Cached payloads ride the `Cached<Fin<T>>` envelope whose `Echo` is `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 corruption the content key alone cannot; a value stored without the echo is rejected. `ReadThrough` caches success and failure under one lane-TTL entry while `Negative` caches only the failure at `ModelPrecision.NegativeTtl` and re-produces every success — behaviourally distinct rows, so an identical-column twin of `ReadThrough` is the named defect. Content-addressed dedup folds the input digest into the stored key so identical-input runs across callers coalesce; a second dedup owner is rejected. A cross-process result-reuse recency horizon reads by reference from the Persistence `ModelResultIndex` owner — a second `Duration horizon` parameter beside the policy rows is the named defect. A `DriftVerdict.Breached` from the identity drift sentinel is consumed as reuse invalidation — the lane cuts through the `Refresh` posture and the run faults `ComputeFault.EquivalenceMiss` — so a graduated model whose serving population leaves its evidence envelope never keeps serving cached verdicts; a drift monitor beside the identity sentinel is the rejected sibling. Hit/miss/evict are HybridCache `ReportTagMetrics` consequences, never a second fact stream.

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
        public async ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default) {
            if (drift.Case is DriftVerdict.Breached breached) {
                await RemoveBoth(cache, key, token);
                return Fin.Fail<T>(new ComputeFault.EquivalenceMiss($"drift:{breached.EvidenceKey}:{breached.Feature}:{breached.Psi}:{breached.SampleCount}"));
            }
            return policy.ReadThroughStore
                ? await ServeStore(cache, key, state, produce, token)
                : await Fresh(cache, policy, key, precision, state, produce, token);
        }
    }

    static async ValueTask<Fin<T>> ServeStore<T, TState>(HybridCache cache, ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) =>
        Validated(key, await cache.Read(
            CacheLane.ModelResult, key.ToString(),
            (Key: key, State: state, Produce: produce),
            static async (s, ct) => new Cached<Fin<T>>(s.Key.ModelChecksum, await s.Produce(s.State, ct)),
            Some(Seq(key.ModelChecksum)), token));

    static async ValueTask<Fin<T>> Fresh<T, TState>(HybridCache cache, CachePolicy policy, ModelResultKey key, ModelPrecision precision, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token) {
        if (policy.CutsFirst) { await RemoveBoth(cache, key, token); }
        if (policy.Serves) {
            HybridCacheEntryOptions probe = new() {
                Expiration = CacheLane.ModelResult.Entry.Expiration,
                LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
                Flags = HybridCacheEntryFlags.DisableUnderlyingData,
            };
            Option<Cached<Fin<T>>> probed = await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"neg:{key}"),
                static _ => new ValueTask<Option<Cached<Fin<T>>>>(Option<Cached<Fin<T>>>.None),
                probe, cancellationToken: token);
            if (probed.Case is Cached<Fin<T>> cached) { return Validated(key, cached); }
        }
        Fin<T> value = await produce(state, token);
        if (value.IsSucc && policy.StoresPositive) {
            await cache.Remove(CacheLane.ModelResult, $"neg:{key}", token);
            await cache.SetAsync(CacheLane.ModelResult.Scoped(key.ToString()), new Cached<Fin<T>>(key.ModelChecksum, value), CacheLane.ModelResult.Entry, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        else if (value.IsFail && policy.StoresNegative) {
            HybridCacheEntryOptions negative = new() {
                Expiration = precision.NegativeTtl.ToTimeSpan(),
                LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
                Flags = CacheLane.ModelResult.Entry.Flags,
            };
            await cache.SetAsync(CacheLane.ModelResult.Scoped($"neg:{key}"), new Cached<Fin<T>>(key.ModelChecksum, value), negative, [CacheLane.ModelResult.Key, key.ModelChecksum], token);
        }
        return value;
    }

    static async ValueTask RemoveBoth(HybridCache cache, ModelResultKey key, CancellationToken token) {
        await cache.Remove(CacheLane.ModelResult, key.ToString(), token);
        await cache.Remove(CacheLane.ModelResult, $"neg:{key}", token);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [CANCELLATION]-[OPEN]: `RunOptions.Terminate=true` aborts `Run`/`RunWithBinding` by throwing the native `OnnxRuntimeException [ErrorCode:Fail] Exiting due to terminate flag being set to true` that `Faulted` reclassifies by scope provenance — what is the latch-propagation latency and safe deadline-poll cadence for the `CoreMl`/`Cpu` rows inside the live plugin ALC; measure against a running `InferenceSession` under the ORT host.
