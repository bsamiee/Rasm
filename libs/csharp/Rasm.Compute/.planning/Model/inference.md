# [COMPUTE_INFERENCE]

`RunOps` folds admitted `OrtValue` runs through one shared session, bracketed native ownership, and vectorized reductions. `BatchGate` coalesces compatible rows, `TilePlan` executes fixed-shape reflective tiles with feathered overlap-add, `StageRun` executes the photo-to-PBR wire, and `CacheOps` projects deterministic results onto `ComputeReceipt.Cache`.

`BoundFlow` allocates through the shared arena and reports per-bucket warm evidence. `ExecutionProvider` resolves the loaded provider, `RunInput.Strings` completes through extension ops, and Persistence owns result custody. `StageRequest` and `StageResult` transcribe the Materials↔Compute wire; opaque keys and content addresses cross injected ports without a strata reference.

## [01]-[INDEX]

- [02]-[INFERENCE_MODES]: every `OrtValue`-only run mode folded over the shared session, one polymorphic input admission feeding the vectorized reductions, the two-leg native bracket, and the cross-request batching gate.
- [03]-[TILED_INFERENCE]: fixed-bucket tiling with row-owned pad, blend, and layout kernels over one gather-run-scatter fold that overlap-adds every product of one forward pass into its own plane and proves its own coverage.
- [04]-[STAGE_EXECUTION]: transcribed photo-to-PBR request and result records over a grant gate, a single-construction tile plan, a roster-owned provider and precision projection, and floor-provider residual measurement.
- [05]-[RESULT_CACHE]: version-stamped deterministic keys and column-driven policy rows over an echo-validated single-flight read-through with drift-gated invalidation.

## [02]-[INFERENCE_MODES]

- Owner: `RunOps` folds every run mode over the shared session; `RunInput` admits one operand polymorphically on carrier shape through the `Tensor/residency#ORT_BRIDGE` `TensorBridge.Ingress` carriers; `PlannedRun` owns the `RunOptions` + `Terminate`-latch registration pair; a `BoundFlow` capsule composes the shared-arena device-resident hot path; `BatchGate` the bounded-window cross-request coalescer over one shared session.
- Cases: `Infer` single run; `InferBound` bound batch over a populated `OrtIoBinding` with an optional name-zip arm; `BoundFlow` the arena-allocated device-resident steady state; `Chunked` streaming windows over a `RecyclableMemoryStream.GetReadOnlySequence` view; `Embed` mean/CLS/last/max-pool text-to-vector; `Classify` softmax top-`k` over the interchange `PointScan` encoding; `ClashScore` scalar clash false-positive scoring over a `ClashPair` feature vector; `InferTensor` the `System.Numerics.Tensors` carrier bridge; `BatchGate.Submit` the coalesced single-row shape; `InferTiled` the fixed-bucket mosaic over a plane exceeding every admitted shape (`[03]-[TILED_INFERENCE]`). Native async `RunAsync` is the rejected spelling — it demands pre-allocated output `OrtValue`s and completes on a native callback outside the lane scope, so the lane seam owns the thread hop.
- Entry: `public Fin<T> Infer<T>(RunOptions options, CancelScope scope, Seq<(string Name, OrtValue Value)> inputs, Seq<string> outputs, Func<IDisposableReadOnlyCollection<OrtValue>, Fin<T>> project)` — the projection runs inside the native-result bracket, and the bracket CONSUMES the admitted inputs: one run is one deterministic release for both native legs, so a repeated same-shape loop rides `BoundFlow`, never re-admitted one-shot inputs.
- Auto: `Plan` folds the `RunConfig` row table into `AddRunConfigEntry` and returns a `PlannedRun` capsule pairing the `RunOptions` with the `Terminate`-latch `CancellationTokenRegistration` off the linked `CancelScope` — the registration disposes with the capsule, so a latch firing into a disposed `RunOptions` is structurally impossible and a bare `Token.Register` whose registration nobody owns is the named use-after-free; `Faulted` is the single fault oracle — it classifies by scope provenance into `DeadlineExpired`/`Cancelled` and lifts a non-cancellation native fault to `ModelRejected`, never a raw `Error.New` leaking an unclassified native exception. Output buffers size from `GetTensorTypeAndShape().ElementCount`, never re-multiplied dimensions. `RunInput` composes the `TensorBridge.Ingress` overloads (the sole dense `OrtValue` C-data factory owner) over the open unmanaged `T`; ONNX-owned strings and preconstructed sparse `OrtValue`s ride distinct cases, with sparse ownership transferring only after `IsSparseTensor` proves the carrier. `Pooling` carries each reduction as its delegate-backed row, and `Embed` admits only an unbatched hidden-state tensor, so a multi-row output never collapses unrelated embeddings into one vector. `Classify` selects each row's top-`k` through a class-stable bounded `PriorityQueue` min-heap (`O(width·log k)`, never a full-taxonomy `Array.Sort`) and materializes per row through an explicit span walk — no `ReadOnlySpan<float>` captured into a lambda, the named kernel exemption. `BatchGate` snapshots each admitted row, caps queued mass at `BatchPolicy.MaxPending`, and packs rows arriving within `MaxDelay` up to `MaxRows` into one bound execution using the injected `TimeProvider` — the bound input stays shaped `[MaxRows, rowWidth]`, a partial window zero-pads its tail rows, and fan-out returns only the submitted rows, because rows are independent under a feed-forward per-row model and a variable-shape rebind per window is the rejected form; per-call ORT dispatch overhead dominates small-tensor inference, so the screening loops the charter names ride one packed run per window instead of thousands of singletons.
- Receipt: `ModelRun` carries model checksum, EP, run mode, batch, the `OrtValue.GetTensorSizeInBytes` output footprint as `PeakBytes`, the `GetTensorMemoryInfo` allocator name as `ArenaAllocator`, and the optional `Runtime/receipts#BENCHMARK_CLAIMS` `ProfileArtifact.ChromeTrace` profile evidence — content-keyed by the admitted `ArtifactIndexRow`'s `ContentAddress` and stamped with the `InferenceSession.ProfilingStartTimeNs` epoch, never a loose path string; profiling artifacts land as `ArtifactKind.OnnxProfile` rows. Every `BatchGate` window emits ONE `ModelRun` whose `BatchSize` is the window's submitted row count (zero-padded tail rows never count) — per-submitter receipt fan-out is the rejected form.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new run shape is one payload case; a new run-config posture is one `RunConfig` row with its `AddRunConfigEntry` pairs and `OrtAllocatorType` arena column; a new pooling shape is one delegate-backed `Pooling` row; a wider classifier candidate set is the `Classify` `top` arity; a batching posture is one `BatchPolicy` value, never a second coalescer; a `ZipMap` sequence-of-maps classifier reads through the `Model/extension#EXTENSION_OPS` `Egress`→`OpOutput.Batched` reader, never a parallel arm on `Classify` (whose softmax owns the numeric-logit shape only); a BIM point-cloud→element classifier, symbol recognizer, or clash scorer is one more `Classify`/`ClashScore` run over the shared session — consuming the interchange `PointScan` encoding and the `Solver/clash#CLASH_AND_TWIN` `ClashPair` vector — never a BIM-specific service; a tensor-lane handoff already holding a `Tensor<T>` is one `InferTensor` run with zero managed copy; an input plane larger than the session's admitted shape is one `[03]-[TILED_INFERENCE]` `TilePlan` over this same `BoundFlow`, never a free-dimension override that re-plans memory on every extent.
- Boundary: `RunOps` extends `Model/sessions#SESSION_CAPSULE` with bracketed native disposal. `CreateTensorValueFromMemory` binds rented staging without a copy; input ownership transfers at the run, and `Bracket` disposes every admitted input beside the result collection. `InferBound` calls the `OrtValue`-only `RunWithBoundResults` member directly; its named arm zips `GetOutputNames()` against that same collection and never materializes `DisposableNamedOnnxValue`. Every projection proves a nonempty output collection before `First()`. `BoundFlow` binds input and sink from `ModelSessions.SharedAllocator`, and `Pulse` writes through the mutable native span without staging. `Chunked` copies each `ReadOnlySequence<byte>` window into the bound value and emits one terminal `StreamSegment`. `Embed` derives its final axis from output shape and L2-normalizes the pooled vector; `Classify` derives class width, proves row divisibility, and uses bounded top-`k`; `BatchGate` proves packed output cardinality before fan-out. `Profile` admits its artifact through `ArtifactIndexRow.Admit(kind, key, bytes, classification, at, sourceKey)`, grouping the trace under the profiled model checksum, and mints the typed `ProfileArtifact.ChromeTrace` evidence from the admitted row's `ContentAddress` and the `ProfilingStartTimeNs` epoch in the same pass — the index row is custody, the union case is receipt evidence, one identity joining both; retention derives from `ArtifactKind.Retention`.

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

        public Fin<(ArtifactIndexRow Row, ProfileArtifact Artifact)> Profile(SessionPolicy policy, UInt128 sourceKey, DataClassification classification, Instant at) =>
            !policy.Profiling
                ? Fin.Fail<(ArtifactIndexRow, ProfileArtifact)>(new ComputeFault.ModelRejected("profiling-disabled"))
                : Try.lift(() => session.EndProfiling() is string path
                        ? Fin.Succ(ArtifactIndexRow.Admit(ArtifactKind.OnnxProfile, path, File.ReadAllBytes(path), classification, at, Some(sourceKey)))
                        : Fin.Fail<ArtifactIndexRow>(new ComputeFault.ModelRejected("profiling-path-missing")))
                    .Run()
                    .MapFail(error => new ComputeFault.ModelRejected(error.Message))
                    .Bind(identity)
                    .Map(row => (row, (ProfileArtifact)new ProfileArtifact.ChromeTrace(row.Content, session.ProfilingStartTimeNs)));

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
            new(model.Key, ep, mode, batch, checked((long)output.GetTensorSizeInBytes()), output.GetTensorMemoryInfo().Name, profile.IfNoneUnsafe((ProfileArtifact?)null)) {
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

- Owner: `TilePlan` `[ComplexValueObject]` owns the whole tiling — source extent, source channels, the ordered `TileProduct` roster the model's own output names key, bucket edge, overlap, scale, and the three row families that generate the space: `PadMode` folds an out-of-range index back onto the plane, `TileBlend` shapes the overlap taper, and `TileLayout` carries the gather, scatter, and normalize kernel triple for one tensor layout; `TilePlan` itself owns the layout-free weight accumulation every product shares. `RunOps.InferTiled` is the fold; `TileMosaic` is the assembled product set with its measured coverage floor.
- Cases: `PadMode.Reflect`; `TileBlend` rows `Hann`, `Linear`, `Smoothstep`; `TileLayout` rows `Planar` (`NCHW`) and `Interleaved` (`NHWC`).
- Law: tiles are FIXED-SHAPE. Dynamic input extents re-partition the graph and defeat memory-pattern reuse on every call, so the bound input holds one bucket shape for the whole mosaic and the plane adapts to the bucket rather than the session adapting to the plane. Grids count the first tile whole and step the remainder by the stride, so an extent equal to its bucket is exactly one tile; stepping the whole extent against the stride emits a trailing tile carrying no new texels.
- Law: ONE grid carries EVERY product. Appearance estimators emit base colour, normal, and roughness from a single forward pass, so `TilePlan.Products` scatters all three out of the same tile run and a mosaic costs the grid rather than the grid times the plane count — folding per output re-infers the whole image once for each plane the model already produced. `TilePlan.Products` orders exactly as `InferenceSession.OutputNames`, and a run whose result cardinality disagrees with that roster refuses rather than mapping planes by guess.
- Law: reassembly is OVERLAP-ADD, never last-writer-wins. Each produced tile scatters through its taper weights into its product's accumulation plane, one shared weight plane accumulates the taper mass, and one divide per product closes the mosaic — so an overlap band carries the weighted mean of both estimates rather than a hard seam, and a blend row whose profile does not sum to unity still reconstructs exactly because the divide normalizes what accumulated instead of trusting the profile. `TilePlan.Accumulate` owns that weight plane as pure geometry — one taper mass per texel, free of layout and product — so it runs once per window whatever the roster's width.
- Law: taper applies only where a tile MEETS a neighbour. Four per-axis ramps index by the window's edge mask, so a tile touching the plane border keeps unit weight there; tapering against that border divides the outermost texels by a weight no neighbour ever completes and fades the plane's own edge.
- Entry: `public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source)` on `BoundFlow` — one entry for the whole mosaic, because a per-tile entrypoint pushes the grid, the padding, the taper, and the coverage proof onto every caller; `source` is `ReadOnlyMemory` rather than a span so the scatter closure the run bracket invokes holds the arenas it writes.
- Auto: `TilePlan` derives grid, stride, output extent, bucket key, and the bound input and per-product output shapes from its own columns, so a caller states extents and a bucket and never a coordinate — and the binder that seats the flow reads the same shapes the fold runs. Admission rejects a nonpositive extent or channel count, an empty or name-duplicated product roster, an overlap at or past half the shorter bucket edge, and any product whose output element count passes `Array.MaxLength`. It does NOT restate which pad row is legal: the row family is the general tiling vocabulary and the frozen wire pins `reflect` at the boundary that carries it. Gathering stages each tile through the pad row, taking the contiguous row copy whenever the row lies wholly inside the plane and folding per texel only at an edge. Scattering accumulates through `TensorPrimitives.Multiply`/`Add`/`MultiplyAdd` over the per-row weight vector, so reassembly vectorizes rather than walking texels. Coverage proves from the MEASURED weight floor — `TensorPrimitives.Min` over the weight plane — and a floor at or below zero refuses rather than dividing a texel no tile reached.
- Receipt: a mosaic reports as one `ComputeReceipt.ModelRun` whose mode is the tiled key and whose `BatchSize` is `TileMosaic.Tiles`, the count inferred; per-tile and per-product receipt fan-out are the rejected forms for the same reason a `BatchGate` window emits one — the grid ran once.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new seam profile is one `TileBlend` row with its ramp; a new tensor layout is one `TileLayout` row carrying its own gather, scatter, and normalize kernels — never a layout flag branching inside the fold; a model emitting another plane is one more `TileProduct` row read off `InferenceSession.OutputNames`, and no surface moves at all; a stage that up-samples is the `Scale` column, which threads every product grid without a caller recomputing anything; a pad posture beyond reflection is one `PadMode` row whose `Fold` may answer a negative index for a texel no source covers, which the gather rows already clear.
- Boundary: `InferTiled` composes the `Model/sessions#SESSION_CAPSULE` shared-arena `BoundFlow` and NEVER opens a session — the flow's bound input is the bucket and its bound outputs are the product roster, so a mosaic and its session warm-up name the same shapes by construction. Tiles run sequentially through the one bound input because the binding holds a single device-resident staging value; intra-tile parallelism belongs to the session's own thread pool. Every arena is a pooled `MemoryOwner<float>` released on the fold's exit, and the mosaic transfers one accumulation rental per product to the caller, so a failed pulse disposes every plane before the fault leaves.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// One tile's placement: grid coordinate, SOURCE read origin — which may sit past the plane edge, where the pad row
// folds the index back — and the per-axis taper mask, bit 0 for the leading edge and bit 1 for the trailing edge.
public readonly record struct TileWindow(int Column, int Row, int SourceX, int SourceY, int TaperX, int TaperY);

// One produced plane: the model's own output tensor name and the component count that tensor carries. The roster
// order IS the session's output order, so a product resolves by position within one session's own results and never
// by matching a declared role against a model's naming.
public readonly record struct TileProduct(string Name, int Channels);

// Layout rows own this kernel triple. Every carrier stays a span view, so a custom delegate is the only shape that
// holds them; `row` is scratch the scatter fills with one output row of taper weights. Scatter takes the product's
// channel count rather than reading one off the plan, because a mosaic writes every product through this one kernel.
public delegate void TileGather(ReadOnlySpan<float> source, Span<float> tile, TilePlan plan, TileWindow window);

public delegate void TileScatter(
    ReadOnlySpan<float> tile, Span<float> plane,
    ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
    TilePlan plan, TileWindow window, int channels);

public delegate void TileNormalize(Span<float> plane, ReadOnlySpan<float> weight, int channels);

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
        PlanarGather, PlanarScatter, PlanarNormalize);
    public static readonly TileLayout Interleaved = new(
        "nhwc", static (channels, height, width) => [1L, height, width, channels],
        InterleavedGather, InterleavedScatter, InterleavedNormalize);

    public Func<int, int, int, long[]> Shape { get; }
    public TileGather Gather { get; }
    public TileScatter Scatter { get; }
    public TileNormalize Normalize { get; }

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
    // product shares, so the plan owns it and it runs once per window rather than once per product per window.
    static void PlanarScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels) {
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
                ReadOnlySpan<float> produced = tile.Slice((channel * tileHeight + y) * tileWidth, span);
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
    // texel and the scratch row stays unread — the same triple, a different contiguous axis.
    static void InterleavedScatter(
        ReadOnlySpan<float> tile, Span<float> plane,
        ReadOnlySpan<float> rampX, ReadOnlySpan<float> rampY, Span<float> row,
        TilePlan plan, TileWindow window, int channels) {
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
                ReadOnlySpan<float> produced = tile.Slice((y * tileWidth + x) * channels, channels);
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
}

// --- [MODELS] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class TilePlan {
    public int SourceWidth { get; }

    public int SourceHeight { get; }

    public int Channels { get; }

    // Ordered exactly as the session's own output roster; the count is the mosaic's plane count and the run's
    // expected result cardinality, which is why a model emitting more planes moves no surface here.
    public Seq<TileProduct> Products { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public int Overlap { get; }

    public int Scale { get; }

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

    // Binders seat every bound value from the plan, so shapes the flow holds and shapes the fold writes cannot
    // drift: any bound output sized elsewhere is a second derivation of one grid.
    public long[] OutputShape(TileProduct product) =>
        Layout.Shape(product.Channels, TileHeight * Scale, TileWidth * Scale);

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

    // Which pad row is legal is NOT settled here: this owner is the general tiling vocabulary, and the frozen stage
    // wire pins `reflect` at the boundary carrying it, so restating the pin would make one law answerable twice.
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int sourceWidth, ref int sourceHeight, ref int channels, ref Seq<TileProduct> products,
        ref int tileWidth, ref int tileHeight, ref int overlap, ref int scale,
        ref PadMode pad, ref TileBlend blend, ref TileLayout layout) {
        // Roster predicates read LOCAL copies: the validation seam takes every argument by `ref` for normalization
        // and a lambda cannot close over a `ref` parameter, so a per-product fold reaches its bounds only through
        // locals lifted first.
        int slots = products.Count;
        Seq<TileProduct> roster = products;
        int extentX = sourceWidth;
        int extentY = sourceHeight;
        int factor = scale;
        validationError = sourceWidth > 0 && sourceHeight > 0 && channels > 0
            && slots > 0
            && roster.ForAll(static product => product.Name.Length > 0 && product.Channels > 0)
            && roster.Map(static product => product.Name).ToFrozenSet(StringComparer.Ordinal).Count == slots
            && (tileWidth is 256 or 512) && tileHeight == tileWidth && scale > 0
            && overlap is >= 8 and <= 32 && overlap * 2 < tileWidth
            && roster.ForAll(product =>
                (long)extentX * extentY * factor * factor * product.Channels <= Array.MaxLength)
            ? null
            : new ValidationError(
                message: $"<tile-plan:{sourceWidth}x{sourceHeight}:{tileWidth}x{tileHeight}:{overlap}:{scale}:{slots}>");
    }
}

// One assembled product plane and the component count it carries.
public sealed record TilePlane(string Role, MemoryOwner<float> Plane, int Channels);

// Assembled product set; owning every rental makes the mosaic the one release point, so a caller that encodes and
// drops it returns each arena and a faulted fold disposes them all before the fault leaves.
public sealed class TileMosaic : IDisposable {
    internal TileMosaic(Seq<TilePlane> planes, TilePlan plan, int tiles, float coverage) =>
        (Planes, Plan, Tiles, Coverage) = (planes, plan, tiles, coverage);

    public Seq<TilePlane> Planes { get; }
    public TilePlan Plan { get; }
    public int Tiles { get; }
    public float Coverage { get; }

    public void Dispose() => Planes.Iter(static produced => produced.Plane.Dispose());
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class RunOps {
    extension(BoundFlow flow) {
        public Fin<TileMosaic> InferTiled(RunOptions options, CancelScope scope, TilePlan plan, ReadOnlyMemory<float> source) {
            if (source.Length != (long)plan.Channels * plan.SourceWidth * plan.SourceHeight) {
                return Fin.Fail<TileMosaic>(new ComputeFault.ModelRejected($"<tile-source:{source.Length}>"));
            }
            int texels = plan.OutputWidth * plan.OutputHeight;
            // Materialized once: the scatter closure indexes by result position, and an array indexer is the only
            // positional read a lambda can hold without forcing a span the closure cannot capture.
            TilePlane[] planes = plan.Products.Map(product => new TilePlane(
                product.Name, MemoryOwner<float>.Allocate(texels * product.Channels, AllocationMode.Clear), product.Channels))
                .ToArray();
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
                // roster order, so each plane resolves by position inside one session's results — never by matching
                // a model's tensor name against a role some other end declared.
                Fin<Unit> pulsed = flow.Pulse(options, scope, tile.Span, results => {
                    if (results.Count != planes.Length) {
                        return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<tile-products:{results.Count}:{planes.Length}>"));
                    }
                    int slot = 0;
                    foreach (OrtValue value in results) {
                        TilePlane target = planes[slot];
                        ReadOnlySpan<float> produced = value.GetTensorDataAsSpan<float>();
                        if (produced.Length != area * target.Channels || !TensorPrimitives.IsFiniteAll(produced)) {
                            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<tile-output:{target.Role}:{produced.Length}>"));
                        }
                        plan.Layout.Scatter(
                            produced, target.Plane.Span,
                            rampX[window.TaperX], rampY[window.TaperY], row.Span, plan, window, target.Channels);
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
            foreach (TilePlane held in planes) { plan.Layout.Normalize(held.Plane.Span, weight.Span, held.Channels); }
            return Fin.Succ(new TileMosaic(toSeq(planes), plan, emitted, coverage));
        }

        // FIRST window's raw output on the FIRST product, the deterministic canary two providers compare on: one
        // tile bounds the parity cost at two runs whatever the mosaic's tile count, and one product bounds it
        // whatever the roster's width — a residual hides in no plane a shared graph produced in the same pass.
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

- Owner: `StageRun` folds a dependency-ordered request sequence into results; `StageRequest`/`StageInput`/`StageOutput`/`StageResult` transcribe the frozen wire records; `LicenseClass` enforces the grant vocabulary; `StagePorts` carries the plane read, plane write, output-description, and session-open legs the app root binds.
- Law: Materials SPECIFIES and Compute EXECUTES. Stage, model-card, and role identities cross as OPAQUE KEYS and this end dispatches on none of them, so admitting a model, a stage, or an intermediate at the specifying end moves no surface here; a mirrored stage roster makes every new model a Compute edit and breaks the row-growth law the wire exists to hold.
- Law: ONNX output tensor names ARE the role keys. Requests carry no output roster, so the executor emits one product per `InferenceSession.OutputNames` entry under that name and the specifying end resolves it against its own channel and prior vocabularies — ordinal correspondence between model outputs and declared roles binds two independently versioned rosters by position.
- Law: a tiling plan has ONE construction. `StageRequest.Plan` folds the request's own extent, bucket, overlap, pad, and derived scale columns into `TilePlan`, whose validator is the only place the fixed-bucket law is spelled, and that same value then seats the bound flow and drives the fold — so a plan built once cannot disagree with itself, and neither a request-side predicate restating the law nor a compare catching two spellings drift apart has anything left to do.
- Law: `Scale` DERIVES from the extents, never a column. Wire records thread both extents while a stage publishes `inputWidth × scale`, so a carried scale only ever contradicts them; `StageRequest.Scale` answers `None` for a fractional or anisotropic ratio and admission refuses there rather than at a bind reporting a shape mismatch it cannot explain.
- Law: providers are a PREFERENCE and the floor is guaranteed; PRECISION is neither. `ExecutionProvider.FromWire` degrades an unrostered or unloaded spelling to `Floor` and `ProviderUsed` reports what ran, so the substitution is visible in the result. Precision has no such report column, so `ModelPrecision.FromWire` refuses an unrostered spelling and the request never admits — an fp16 request silently running fp32 is the substitution the `CoreMl` `ModelFormat` pin exists to foreclose. `StagePorts.Lease` then TAKES the resolved precision: admitting a precision column and leasing without it is the same defect wearing an argument.
- Law: parity measures the CANDIDATE against a FULL-precision floor. `GoldenDelta` runs the canary tile on the requested provider and on `Cpu` at `ModelPrecision.Full`, so the residual grades the whole acceleration decision — provider and precision together — rather than comparing two runs that already agreed to lower precision.
- Law: evidence publishes MEASURED or refuses. `PartitionCount` reads the per-bucket warm evidence the session capsule measured once, never a zero standing in for an unmeasured run; a request whose bucket carries no partition measurement refuses rather than minting a result whose evidence column reads as observed.
- Entry: `public static Fin<Seq<StageResult>> Fold(Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, TimeProvider time)` — one entry for the whole plan, because per-request entry pushes producer-output resolution onto the caller and re-opens the chained-stage defect where every stage reads the source photograph.
- Auto: `Fold` threads a produced-plane map so a binding naming a producer resolves against results already held and only an empty producer key reads the intent's own plane. `Admit` refuses an ungranted license, an unrostered precision, a bucket spelling disagreeing with its own tile columns, a pad key no row claims, and a non-integral scale. `Execute` resolves the provider, leases once, builds the plan against the session's product roster, opens ONE bound flow at that plan, runs the grid once, writes every produced plane through the port, and folds elapsed time from the injected `TimeProvider`. `Warmup` injects measured partitions; the run asserts that count against the model-card cap before publication.
- Receipt: each executed stage emits one `ComputeReceipt.ModelRun` with the tiled mode key and the mosaic's tile count as `BatchSize` — one grid ran, so one receipt mints whatever the roster's width; the stage-level evidence rides `StageResult` across the wire, never a second receipt case, because the specifying end owns the admission that reads it.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new grant posture is one `LicenseClass` row; a new wire column is one record field transcribed from the frozen roster at both ends in one change; a further execution backend is one `Model/providers#EP_AXIS` row declaring one `WireKey`, never a translation table here and never a second stage owner; a stage emitting more products is more `InferenceSession.OutputNames` entries the lease reports as `TileProduct` rows, with no surface move at all.
- Boundary: `StagePorts` is the ONLY route to a plane. Compute holds no blob store, no codec, and no channel vocabulary — it reads and writes float planes through injected legs the app root binds against the Persistence object lane, exactly as `Model/sessions#SESSION_CAPSULE` binds its context-blob leg. Provider and precision spellings resolve at `Model/providers#EP_AXIS`, whose rows carry their own wire keys, so this record holds no translation table and a roster landing there crosses without an edit here. `StageSession` carries the model-derived facts a request cannot know — the product roster, the tensor layout, the blend profile, the warm evidence — and its `Flow` leg takes the built plan, so the bound shapes and the fold's shapes have one source. This wire mints no `tests/contracts/MANIFEST.md` entry — it never leaves the C# runtime.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
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

    public bool Grants { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// One consumed product. An empty Stage names the intent's own plane and carries the blob key; a named Stage names
// its producer and role, whose already-held output the fold resolves.
public readonly record struct StageInput(string Stage, string Role, string Key);

// One produced plane, named by the model's own output tensor.
public readonly record struct StageOutput(string Role, string BlobKey, int Width, int Height, string Transfer, string Format);

public sealed record StageRequest(
    string Stage, string ModelCardId, LicenseClass LicenseClass, Seq<StageInput> Inputs,
    int InputWidth, int InputHeight, int OutputWidth, int OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string Pad, string Bucket,
    string Provider, string Precision, ulong Seed, Op Op) {

    // Wire spellings resolve at the ROSTER that owns the rows, so this record holds no translation table and a
    // provider or precision landing there crosses without an edit here. The asymmetry is deliberate: a substituted
    // provider is reported on `ProviderUsed`, a substituted precision is reported nowhere, so one degrades and the
    // other refuses.
    public ExecutionProvider SelectedProvider => ExecutionProvider.FromWire(Provider);

    public Option<ModelPrecision> SelectedPrecision => ModelPrecision.FromWire(Precision);

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
    public Fin<TilePlan> Plan(int sourceChannels, Seq<TileProduct> products, TileBlend blend, TileLayout layout) =>
        Scale.Case is not int scale
            ? Fin.Fail<TilePlan>(new ComputeFault.ModelRejected(
                $"<stage-scale:{InputWidth}x{InputHeight}:{OutputWidth}x{OutputHeight}>"))
            : !PadMode.TryGet(Pad, out PadMode? pad)
                ? Fin.Fail<TilePlan>(new ComputeFault.ModelRejected($"<stage-pad:{Pad}>"))
                : TilePlan.Validate(
                        sourceWidth: InputWidth, sourceHeight: InputHeight, channels: sourceChannels, products: products,
                        tileWidth: TileWidth, tileHeight: TileHeight, overlap: Overlap, scale: scale,
                        pad: pad!, blend: blend, layout: layout, out TilePlan? built) is { } fault
                    ? Fin.Fail<TilePlan>(fault)
                    : Fin.Succ(built!);

    // DECODE gate: everything provable WITHOUT a model. Extent, bucket, and pad legality prove here so a malformed
    // request never reaches a session lease; the plan itself builds after the lease, because only the model names
    // its own products. `StageRun` re-proves the grant alone, the one column an executing end never takes on trust.
    public static Fin<StageRequest> Admit(StageRequest request) =>
        !request.LicenseClass.Grants
            ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-blocked:{request.ModelCardId}>"))
            : request.SelectedPrecision.IsNone
                ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-precision:{request.Precision}>"))
                : request.Scale.IsNone
                    || !StringComparer.Ordinal.Equals(request.Bucket, $"{request.TileWidth}x{request.TileHeight}")
                    || !PadMode.TryGet(request.Pad, out PadMode? _)
                    ? Fin.Fail<StageRequest>(new ComputeFault.ModelRejected($"<stage-shape:{request.Bucket}>"))
                    : Fin.Succ(request);
}

public sealed record StageResult(
    string Stage, string ModelCardId, Seq<StageOutput> Outputs, string ProviderUsed,
    int PartitionCount, double ElapsedMs, double GoldenDelta, int TilesEmitted, Op Op);

// --- [SERVICES] ----------------------------------------------------------------------------
// Everything about a run that only the MODEL knows: its output roster with each product's component count, the
// tensor layout its graph emits, the taper profile its class of estimator wants, and the partition count that
// bucket's warm-up measured. `Flow` takes the BUILT plan and binds every value from it, so shapes the flow holds
// and shapes the fold writes have one source. Holding the lease as this record's own disposable keeps its session
// alive across the whole grid.
public sealed record StageSession(
    Seq<TileProduct> Products, TileLayout Layout, TileBlend Blend, Option<int> Partitions, int PartitionCap,
    Func<TilePlan, Fin<BoundFlow>> Flow, IDisposable Hold);

// Every plane crosses as a content address the host resolves; Compute holds no store, no codec, and no vocabulary.
// `Lease` takes the resolved precision because a posture admitted at the wire and dropped before the session is a
// column the receipt then reports without anything having executed it.
public sealed record StagePorts(
    Func<string, Fin<(ReadOnlyMemory<float> Plane, int Width, int Height, int Channels)>> Read,
    Func<ReadOnlyMemory<float>, int, int, int, Fin<string>> Write,
    Func<StageRequest, string, Fin<(string Transfer, string Format)>> Describe,
    Func<StageRequest, ExecutionProvider, ModelPrecision, Fin<StageSession>> Lease);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StageRun {
    public static Fin<Seq<StageResult>> Fold(
        Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, TimeProvider time) =>
        plan.Fold(
            Fin.Succ((Results: Seq<StageResult>(), Produced: HashMap<(string Stage, string Role), string>())),
            (state, request) => state.Bind(carried =>
                from admitted in StageRequest.Admit(request)
                from result in Execute(admitted, carried.Produced, ports, options, scope, time)
                select (
                    Results: carried.Results.Add(result),
                    Produced: result.Outputs.Fold(
                        carried.Produced,
                        (map, output) => map.AddOrUpdate((request.Stage, output.Role), output.BlobKey)))))
            .Map(static carried => carried.Results);

    static Fin<StageResult> Execute(
        StageRequest request, HashMap<(string Stage, string Role), string> produced, StagePorts ports,
        RunOptions options, CancelScope scope, TimeProvider time) {
        long mark = time.GetTimestamp();
        ExecutionProvider selected = request.SelectedProvider;
        // Grants re-check HERE even when Admit already ran at decode: any host edge reaches this end, and a grant
        // enforced only where the request is built trusts the caller's word.
        return from _ in guard(
                       request.LicenseClass.Grants,
                       (Error)new ComputeFault.ModelRejected($"<stage-blocked:{request.ModelCardId}>")).ToFin()
               from precision in request.SelectedPrecision
                       .ToFin(new ComputeFault.ModelRejected($"<stage-precision:{request.Precision}>"))
               from key in Source(request, produced)
               from source in ports.Read(key)
               from __ in guard(
                       source.Width == request.InputWidth
                       && source.Height == request.InputHeight,
                       (Error)new ComputeFault.ModelRejected(
                           $"<stage-source-extent:{source.Width}x{source.Height}x{source.Channels}>")).ToFin()
               from outputs in Run(request, ports, source, selected, precision, options, scope)
               from golden in Golden(request, ports, source, selected, precision, options, scope)
               select new StageResult(
                   request.Stage, request.ModelCardId, outputs.Products, selected.ReportKey,
                   outputs.Partitions, time.GetElapsedTime(mark).TotalMilliseconds, golden, outputs.Tiles,
                   request.Op);
    }

    // Chained stages NEVER carry the source plane: a binding naming a producer resolves against results already
    // held, so a pipeline whose links never touch is unrepresentable rather than merely discouraged.
    static Fin<string> Source(StageRequest request, HashMap<(string Stage, string Role), string> produced) =>
        request.Inputs.Head
            .ToFin(new ComputeFault.ModelRejected($"<stage-no-input:{request.Stage}>"))
            .Bind(binding => binding.Stage.Length is 0
                ? Fin.Succ(binding.Key)
                : produced.Find((binding.Stage, binding.Role))
                    .ToFin(new ComputeFault.ModelRejected($"<stage-unresolved:{binding.Stage}:{binding.Role}>")));

    // ONE lease, ONE plan, ONE grid. Leasing reports the model's products, the request folds them into a plan, that
    // plan seats the flow, and the grid runs once for every plane the model emits — a fold per output re-infers the
    // whole image once per plane a single forward pass already produced. The partitions read below is the
    // sessions#MODEL_SESSIONS `Warm` counterpart obligation: the APP ROOT registers each request's bucket through
    // `ModelSessions.Warm(key, request.Bucket, shape)` before the first run, so an unmeasured bucket here names a
    // composition that skipped its registration, never a missing surface — the refusal is the seam's proof.
    static Fin<(Seq<StageOutput> Products, int Partitions, int Tiles)> Run(
        StageRequest request, StagePorts ports, (ReadOnlyMemory<float> Plane, int Width, int Height, int Channels) source,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Leased(request, ports, selected, precision, source.Channels, (session, plan) =>
            from partitions in session.Partitions
                .ToFin(new ComputeFault.ModelRejected($"<stage-partitions-unmeasured:{request.Bucket}>"))
            from _ in guard(
                    partitions <= session.PartitionCap,
                    (Error)new ComputeFault.ModelRejected(
                        $"<stage-partitions:{partitions}:{session.PartitionCap}>")).ToFin()
            from emitted in Emit(request, ports, session, plan, source.Plane, options, scope)
            select (emitted.Products, Partitions: partitions, emitted.Tiles));

    // Lease and plan form ONE bracket: the hold releases inside the bind that took it, so a plan refusing never
    // strands a session and a fault never leaves a resident held.
    static Fin<T> Leased<T>(
        StageRequest request, StagePorts ports, ExecutionProvider provider, ModelPrecision precision,
        int sourceChannels, Func<StageSession, TilePlan, Fin<T>> use) =>
        ports.Lease(request, provider, precision).Bind(session => {
            using (session.Hold) {
                return request.Plan(sourceChannels, session.Products, session.Blend, session.Layout)
                    .Bind(plan => use(session, plan));
            }
        });

    static Fin<(Seq<StageOutput> Products, int Tiles)> Emit(
        StageRequest request, StagePorts ports, StageSession session, TilePlan plan, ReadOnlyMemory<float> source,
        RunOptions options, CancelScope scope) =>
        session.Flow(plan).Bind(flow => {
            using (flow) {
                return flow.InferTiled(options, scope, plan, source).Bind(assembled => {
                    using (assembled) {
                        return assembled.Planes
                            .Traverse(produced =>
                                (from shape in ports.Describe(request, produced.Role)
                                 from key in ports.Write(
                                     produced.Plane.Memory, plan.OutputWidth, plan.OutputHeight, produced.Channels)
                                 select new StageOutput(
                                     produced.Role, key, plan.OutputWidth, plan.OutputHeight,
                                     shape.Transfer, shape.Format)).ToValidation())
                            .As()
                            .ToFin()
                            .Map(products => (Products: products, assembled.Tiles));
                    }
                });
            }
        });

    // Parity is measured on ONE canary tile against a FULL-PRECISION floor run — two runs whatever the mosaic's tile
    // count, and the residual then grades the whole acceleration decision rather than comparing two runs that had
    // already agreed to lower precision. Floor answering itself is an identity rather than an unmeasured zero, so it
    // reports without a second lease. Each lease releases inside the bind that took it.
    static Fin<double> Golden(
        StageRequest request, StagePorts ports, (ReadOnlyMemory<float> Plane, int Width, int Height, int Channels) source,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        selected.IsFloor && ReferenceEquals(precision, ModelPrecision.Full)
            ? Fin.Succ(0d)
            : Leased(request, ports, selected, precision, source.Channels, (candidate, plan) =>
                Leased(request, ports, ExecutionProvider.Floor, ModelPrecision.Full, source.Channels, (reference, truthPlan) =>
                    from fast in Probe(candidate, plan, source.Plane, options, scope)
                    from truth in Probe(reference, truthPlan, source.Plane, options, scope)
                    from delta in Residual(fast, truth)
                    select delta));

    static Fin<float[]> Probe(
        StageSession session, TilePlan plan, ReadOnlyMemory<float> source, RunOptions options, CancelScope scope) =>
        session.Flow(plan).Bind(flow => {
            using (flow) { return flow.Canary(options, scope, plan, source); }
        });

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

- Owner: `CachePolicy` `[SmartEnum<string>]` — four behaviour columns (`Serves`/`CutsFirst`/`StoresPositive`/`StoresNegative`) drive every posture through the derived `ReadThroughStore` predicate; `CacheOps` owns key derivation, echo-validated read-through, the precision-TTL negative probe, and the `Cached<T>` envelope. Cache outcome projects onto the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.Cache` fact — a second fact stream is rejected, `ComputeReceipt` being the package's only measured-fact vocabulary.
- Cases: `Bypass`, `ReadThrough`, `WriteThrough`, `Refresh`, `Negative`.
- Entry: `public ValueTask<Fin<T>> Through<T, TState>(CachePolicy policy, ModelResultKey key, ModelPrecision precision, Option<DriftVerdict> drift, TState state, Func<TState, CancellationToken, ValueTask<Fin<T>>> produce, CancellationToken token = default)` — the policy row is an intent field, never a boolean flag; `produce` returns `Fin<T>` so a faulted run negative-caches rather than re-running every call, `precision` sizes the negative TTL, and present drift evidence is an input-shape discriminant rather than an independently reconstructed monitor.
- Auto: `Key` stamps model checksum, input digest, EP key, ORT version, and option-table hash so cross-version drift never serves a stale hit; content-addressed dedup coalesces byte-identical-input/identical-EP runs to one stored payload. `Through` first consumes a `DriftVerdict.Breached` by evicting both result and negative keys and returning `ComputeFault.EquivalenceMiss`, then dispatches on the `ReadThroughStore` predicate — the read-through path delegates to `CacheLane.ModelResult` `Read` (the `HybridCache.GetOrCreateAsync` single-flight that collapses a stampede and caches the whole `Cached<Fin<T>>`, success and deterministic failure alike, under the lane TTL), and every other row falls to `Fresh`, which evicts both keys when `CutsFirst`, serves a cached negative through an `Option<Cached<Fin<T>>>` `DisableUnderlyingData` cache-only probe, produces once, clears stale negative evidence before a positive write, then stores the success under the result key or the failure under the `neg:` key at `ModelPrecision.NegativeTtl` — every column reaches a live branch, no posture a twin of another.
- Receipt: outcome projects onto `ComputeReceipt.Cache(Outcome, Key, Bytes)` (`Outcome` ∈ `hit`/`miss`/`store`/`evict`) at the sink edge; `CacheLane.ReportTagMetrics` meters live hit/miss/evict by lane tag; `Validated` faults `ComputeFault.CacheCorrupt` when a rehydrated echo mismatches `key.ModelChecksum`.
- Packages: Microsoft.Extensions.Caching.Hybrid, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new cache posture is one `CachePolicy` row with its four columns; a richer outcome is one `ComputeReceipt.Cache.Outcome` value at the receipts owner, never a parallel fact owner; a graduated-model validity axis is the `Model/identity#MODEL_IDENTITY` drift sentinel consumed here, never a sibling monitor.
- Boundary: `CacheOps` extends the `Rasm.AppHost` cache boundary; Compute owns keys and policy rows, never a cache instance — `CacheSurface` over `CacheLane.ModelResult` is the single owner and a hand-rolled `ConcurrentDictionary` memoization beside it is the named defect. Cached payloads ride the `Cached<Fin<T>>` envelope whose `Echo` is `key.ModelChecksum`, so `Validated` catches a cross-checksum L2 corruption the content key alone cannot; a value stored without the echo is rejected. `ReadThrough` caches success and failure under one lane-TTL entry while `Negative` caches only the failure at `ModelPrecision.NegativeTtl` and re-produces every success — behaviourally distinct rows, so an identical-column twin of `ReadThrough` is the named defect. Content-addressed dedup folds the input digest into the stored key so identical-input runs across callers coalesce; a second dedup owner is rejected. Cross-process result-reuse recency horizons read by reference from the Persistence `ModelResultIndex` owner — a second `Duration horizon` parameter beside the policy rows is the named defect. Every `DriftVerdict.Breached` from the identity drift sentinel is consumed as reuse invalidation — the lane cuts through the `Refresh` posture and the run faults `ComputeFault.EquivalenceMiss` — so a graduated model whose serving population leaves its evidence envelope never keeps serving cached verdicts; a drift monitor beside the identity sentinel is the rejected sibling. Hit/miss/evict are HybridCache `ReportTagMetrics` consequences, never a second fact stream.

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

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [COMPLEX_VALUE_FACTORY]-[OPEN]: what signature does the Thinktecture generator emit for a `[ComplexValueObject]` admission returning its error — `Validate(<members>, out T?)` as `[04]-[STAGE_EXECUTION]` `StageRequest.Plan` spells it, or a form threading `IFormatProvider`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md` spells the KEYED arity alone, so compile one multi-member declaration under `uv run python -m tools.assay static` and read the generated factory off the output.
- [TRACE_SCHEMA]-[OPEN]: which keys does the ONNX Runtime chrome trace spell for a node event's category, argument bag, and assigned execution provider, and is the emitted document a bare event array or a `traceEvents`-wrapped object; run one profiled `InferenceSession` under the ORT host through `tools.assay bridge`, `EndProfiling`, and read the emitted file's first node event verbatim.
- [CANCELLATION]-[OPEN]: `RunOptions.Terminate=true` aborts `Run`/`RunWithBinding` by throwing the native `OnnxRuntimeException [ErrorCode:Fail] Exiting due to terminate flag being set to true` that `Faulted` reclassifies by scope provenance — what is the latch-propagation latency and safe deadline-poll cadence for the `CoreMl`/`Cpu` rows inside the live plugin ALC; measure against a running `InferenceSession` under the ORT host.
