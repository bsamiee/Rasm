# inference-lane — bedrock

## OrtValue-only law

- `OrtValue` is the sole value carrier at the model boundary; every input, output, initializer override, and
  chained intermediate is an `OrtValue`.
- The legacy carriers — named values with per-call dictionary marshal, their disposable output twins, and
  fixed-buffer wrappers — are rejected rows: every capability they carry is subsumed by `OrtValue` factories
  plus the name-array run overloads, and they multiply the result-ownership shapes a consumer must handle.
- The managed dense-tensor type that ships beside the session API is also a rejected carrier for the loop: it is
  a copy path; the tensor lane's owner enters through the dedicated factory instead.
- The admitted run family: `Run(RunOptions, IReadOnlyCollection<string> inputNames,
  IReadOnlyCollection<OrtValue> inputValues, IReadOnlyCollection<string> outputNames)` returning a disposable
  `OrtValue` collection; the dictionary form `Run(RunOptions, IReadOnlyDictionary<string, OrtValue>,
  outputNames)`; the fully-bound void form with caller-owned `outputValues`; and `RunAsync(RunOptions,
  inputNames, inputValues, outputNames, outputValues)` for the awaited row.
- Zero-copy ingress has two gates: `OrtValue.CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[]
  shape)` (`T : unmanaged`) wraps managed memory, pinned for the value's lifetime;
  `CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)` admits the tensor lane's owner directly.
- A third raw gate exists for native interop: `CreateTensorValueWithData(OrtMemoryInfo, TensorElementType,
  long[] shape, nint dataBufferPtr, long bufferLengthInBytes)` — pointer-rooted memory enters under the same
  lifetime obligation.
- The lifetime-inversion trap: the backing memory or tensor must outlive the `OrtValue` and every run that binds
  it. The value wraps, it does not copy — freeing or repooling the buffer under a live value is a use-after-free
  expressed in managed code.
- The staging composition: a pooled owner's return must sequence after the wrapping value's dispose, so the
  value's dispose IS the owner's release point — an owner-transfer edge, not a borrow.
- Zero-copy egress is projection-shaped: `GetTensorDataAsSpan<T>` / `GetTensorMutableDataAsSpan<T>` and the
  tensor-span twins `GetTensorDataAsTensorSpan<T>` / `GetTensorMutableDataAsTensorSpan<T>` view the native
  buffer in place.
- Result evidence is queryable per value: `GetTensorTypeAndShape()` (element type, shape),
  `GetTensorMemoryInfo()` (residency), `GetTensorSizeInBytes()` (budget), `OnnxType` / `IsTensor` /
  `IsSparseTensor` (kind probes).
- `OrtTensorTypeAndShapeInfo` carries `ElementDataType`, `ElementCount`, `Shape` (`long[]`), and the `IsString`
  probe — output projection sizes its destination from `ElementCount`, never from re-multiplied dimensions.
- Half-width element carriers ship with the session package (the brain-float and half structs) so
  reduced-precision models bind without a float-up conversion at the wire; the compute interior still widens at
  the tensor lane's admission seam before span kernels touch the data.
- Copies are legal at exactly two named points — string tensors and explicit edge export; every other movement
  is a view.
- String tensors are the one always-copy family: `CreateFromStringTensor(Tensor<string>)` in;
  `CreateTensorWithEmptyStrings(OrtAllocator, long[] shape)` plus `StringTensorSetElementAt(ReadOnlySpan<char> |
  ReadOnlySpan<byte>, index)` for output slots; `GetStringElement(int)` / `GetStringElementAsSpan` /
  `GetStringTensorAsArray()` out.
- UTF-8 marshal cost on string tensors is structural; string I/O is budgeted as boundary cost and never sits on
  a steady-state hot path.
- Steady-state inference allocates nothing per call: inputs are long-lived memory-backed values refreshed by
  writing their spans; outputs are pre-created via `CreateAllocatedTensorValue(allocator, elementType, shape)`
  or memory-backed values consumed through the void run row.
- The allocate-per-call spelling — fresh values each run, results re-projected to arrays — is the rejected form
  that turns inference into a GC workload.
- Result collections are deterministic-dispose native material: one dispose call releases every element. The
  owned-capsule grammar is settled root law composed here; a leaked result collection is a native-memory leak
  invisible to GC heap heuristics.
- Sequence and map values (`CreateSequence`, `CreateMap`, `CreateMapWithStringKeys` /
  `CreateMapWithStringValues`, `GetValueCount` / `GetValue`, visitor-driven `ProcessSequence` / `ProcessMap`)
  cover the non-tensor model interfaces; they are boundary-only shapes — interior code sees projected typed
  rows, never visitor traversal.

## session lifecycle

- One session per model identity, process-wide. Construction cost — graph load, optimization, provider compile —
  makes the session a cached singleton per identity.
- The cache keys on the session fingerprint: model-bytes hash ⊕ every behavior-bearing option column — provider
  rows, optimization level, free-dimension overrides, registered custom-op and extension assets, initializer
  overrides, adapter set, config entries.
- Two sessions over one model with different fingerprints are different identities; a cache keyed on model path
  alone aliases them and serves wrong behavior.
- Model custody is a constructor axis: the byte-array constructors admit models from artifact stores and
  encrypted custody without disk residence; the path constructors let the runtime own file mapping — custody
  class is chosen at admission and recorded in the admission receipt.
- Per-run diagnostics are run-policy columns, not session rebuilds: `RunOptions.LogSeverityLevel` /
  `LogVerbosityLevel` / `LogId` raise verbosity for one investigated run while the session and environment
  posture stay fixed.
- The metadata surface is the admission contract: `InputMetadata` / `OutputMetadata` /
  `OverridableInitializerMetadata` dictionaries of `NodeMetadata` — `ElementDataType` (`TensorElementType`),
  fixed `Dimensions` (`int[]`), `SymbolicDimensions` (`string[]` naming the free axes).
- `ModelMetadata` carries `Version` (long), `GraphName`, `ProducerName`, `Domain`, and `CustomMetadataMap` — the
  map is the model's self-description channel: expected preprocessing, semantic version, golden-input identity
  travel inside the artifact, so admission reads the contract from the model instead of from convention.
- Input shape and dtype validation happens once at session admission against `NodeMetadata`; per-call
  re-validation re-derives what admission settled.
- Symbolic dimensions bind at build, not at run: `AddFreeDimensionOverrideByName(dimName, value)` pins dynamic
  axes, which is what makes memory-pattern planning effective.
- `EnableMemoryPattern` reuses run allocation plans for fixed shapes; under genuinely varying shapes pattern
  reuse thrashes — the shape-class decision (pin-and-pattern, or vary-and-disable) is a declared session column,
  never an accident.
- Warmup is a lifecycle phase with three yields in one act: the first run pays allocation and pattern cost,
  doubles as the liveness probe, and with a golden input yields the equivalence receipt certifying the session
  against its reference output.
- Admission completes when the warmup receipt exists; the first user call is then steady-state by construction.
- Threading is suite policy, not per-session drift: per-session pools (`IntraOpNumThreads` /
  `InterOpNumThreads`) are the default; a multi-session suite calls `DisablePerSessionThreads()` and routes onto
  the global pool.
- The global pool is declared once via `EnvironmentCreationOptions.threadOptions` (`OrtThreadingOptions`:
  `GlobalIntraOpNumThreads`, `GlobalInterOpNumThreads`, `GlobalSpinControl`) at
  `OrtEnv.CreateInstanceWithOptions(ref options)`; thread counts derive from the settled budget record's
  arithmetic.
- Environment boot is once-per-process and precedes all session work; `OrtEnv.Instance()` afterward returns the
  singleton and `IsCreated` probes it — configuring the environment after first session construction is
  unreachable, so boot order is a composition-root concern.
- Boot evidence rows: `GetVersionString()` (native runtime identity for receipts), `EnvLogLevel`, telemetry
  disablement — environment posture is declared at the same boot site.
- `ExecutionMode` (sequential versus parallel graph-node execution) and `EnableCpuMemArena` are
  session-fingerprint columns with thread-pool interactions: parallel node execution multiplies pool pressure
  and is only meaningful for graphs with parallel branches.
- `PrePackedWeightsContainer` shares packed weights across sessions over the same model — option-variant
  sessions pay weight memory once; without it, N variants cost N weight copies.
- Cancellation has two distinct latches: `RunOptions.Terminate` is a one-way latch aborting every in-flight run
  sharing that `RunOptions` instance — per-run cancellation requires per-run `RunOptions`, and a shared instance
  silently makes cancellation suite-wide.
- `SetLoadCancellationFlag` aborts session load — the boot-time twin. Neither latch is a reusable token; scope
  equals the instance's sharing scope.
- Adapter variation is run-policy data, not session multiplication: `OrtLoraAdapter.Create(path | bytes,
  OrtAllocator)` plus `RunOptions.AddActiveLoraAdapter` vary behavior per run over one base session; the active
  adapter set joins the run receipt and the cache key where caching is admitted.
- Eviction is drain-gated deterministic disposal: LRU capacity derives from the memory budget arithmetic
  (resident weight bytes × session count ≤ budget share); eviction disposes after in-flight runs drain through
  the session's own drain gate — a global pause to evict one session is the rejected form.

## execution-provider policy rows

- Provider selection is declared policy data in priority order: the uniform row shape is
  `AppendExecutionProvider(string providerName, Dictionary<string, string> options)`; first capable provider
  wins per graph node; the CPU provider is the implicit terminal row.
- Provider-specific members (`AppendExecutionProvider_CoreML` / `_CUDA` / `_DML` and the
  `MakeSessionOptionWith*Provider` factories) exist but fragment the row shape; the string+dictionary row keeps
  the provider axis a table.
- Capability is probed, never assumed: `OrtEnv.GetAvailableProviders()` vetoes absent rows;
  `GetHardwareDevices()` / `GetNumHardwareDevices()` enumerate the device plane.
- Compiled-model compatibility is checkable before load: `GetModelCompatibilityForEpDevices(epDevices,
  compatibilityInfo)` and `GetCompatibilityInfoFromModel(Bytes)`; incompatibility detail rides
  `GetHardwareDeviceEpIncompatibilityDetails` — typed reasons, not load exceptions.
- A vetoed row degrades to the next row and the veto reason enters the receipt — provider fallback is data, not
  log noise.
- The declarative device tier sits above named rows: `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` with
  the closed row set `DEFAULT` / `PREFER_CPU` / `PREFER_NPU` / `PREFER_GPU` / `MAX_PERFORMANCE` /
  `MAX_EFFICIENCY` / `MIN_OVERALL_POWER`.
- The deepest tier is `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` — a custom fold over
  `(IReadOnlyList<OrtEpDevice>, model metadata, runtime metadata, maxSelections)` — where a suite's own
  selection arithmetic plugs in without abandoning the policy-row shape.
- Provider libraries register and unregister at the environment (`RegisterExecutionProviderLibrary` /
  `UnregisterExecutionProviderLibrary`) — plugin providers are environment material, not session material.
- Further session-fingerprint columns: `GraphOptimizationLevel` (full optimization is the steady-state row),
  arbitrary `AddSessionConfigEntry(key, value)` pairs, and `OptimizedModelFilePath` — persisting the optimized
  graph converts repeat-boot optimization cost into a load.
- Profiling is a diagnostics-row column: `EnableProfiling` + `ProfileOutputPathPrefix`, closed by
  `EndProfiling()` returning the trace path — a trace artifact for evidence, never a production-row default.
- Allocator policy is environment-scoped where sharing matters: `CreateAndRegisterAllocator(OrtMemoryInfo,
  OrtArenaCfg)` and the shared-allocator family (`CreateSharedAllocator` / `GetSharedAllocator` /
  `ReleaseSharedAllocator` over device and memory-type rows) centralize arena behavior across sessions;
  per-session ad-hoc arena tuning fragments one process-level memory policy.

## IOBinding

- `OrtIoBinding` (from `session.CreateIoBinding()`) pins the I/O graph once: `BindInput(name, OrtValue)` /
  `BindOutput(name, OrtValue)`; `RunWithBinding(runOptions, ioBinding)` executes with zero per-call marshal.
- The binding accepts native-allocation rows beside value rows — `BindInput(name, elementType, shape,
  allocation)` and the external-allocation overloads — so device-owned buffers bind without ever surfacing as
  managed values; the value row remains the default and the allocation rows are the device-residency
  specialization.
- `RunWithBoundResults` returns the bound outputs as a disposable value collection; `GetOutputNames()` /
  `GetOutputValues()` read the binding state without running.
- Binding is the steady-state row for repeated same-shape inference — the name-array run rows re-marshal name
  collections per call; the binding does not.
- Device residency is a binding column: `BindOutputToDevice(name, OrtMemoryInfo)` keeps outputs on-device;
  chained model stages pass device-resident values with no host round-trip.
- `OrtMemoryInfo` rows — allocator name, device type, device id, memory type — describe residency; the default
  instance is the CPU row. The host-copy-per-stage pipeline is the rejected form a device-resident chain
  deletes.
- `SynchronizeBoundInputs()` / `SynchronizeBoundOutputs()` are the device-stream fences bracketing runs when
  device providers are bound; on CPU-only bindings they are no-ops — the fence calls are unconditional in the
  loop and cost nothing where unneeded, so conditioning them on provider class is complexity without yield.
- Shape staleness is the binding's trap: bound values carry shape; an input whose shape class changes without
  rebinding throws or mis-executes.
- The rebind protocol: rebind on shape-class transition; `ClearBoundInputs()` / `ClearBoundOutputs()` reset
  without recreating the binding object.
- The combined zero-allocation law: binding + pinned shapes + memory pattern + pre-allocated outputs = the
  steady-state loop; any one missing reintroduces per-call cost, and the four columns are declared together as
  one loop posture.

## extensions and result caching

- Custom operators enter through exactly one declared seam: `SessionOptions.RegisterOrtExtensions()` registers
  the extension operator bundle — tokenizers, pre- and post-processing ops shipped as native assets.
- Other native op libraries enter via `RegisterCustomOpLibrary(path)` or `RegisterCustomOpLibraryV2(path, out
  handle)` — the V2 handle is the lifetime hook for unload discipline.
- Native asset presence is model evidence checked before execution is admitted: a model referencing extension
  ops without the registered asset fails at session build, and admission surfaces that as a typed asset-absence
  rejection rather than a load exception.
- Tokenization and pre/post-processing are session-graph material, not a service family: composing them as graph
  nodes keeps one execution plan, one provider policy, and one receipt; a separate preprocessing service
  re-derives scheduling and loses operator fusion across the boundary.
- Registered assets are fingerprint columns: a session with custom ops is not identified by model bytes alone —
  asset hashes join the session fingerprint, or two behaviorally different sessions alias under one key.
- Initializer overrides are session columns, not inputs: `AddInitializer(name, OrtValue)` binds per-deployment
  constants at build (the overridable set is published by `OverridableInitializerMetadata`); threading
  deployment constants through per-run inputs re-pays marshal for values that never change.
- Result caching composes the settled cache seam — storage, expiry, and stampede mechanics are runtime law; this
  lane owns only key identity and admission.
- Key identity: (session fingerprint, input content hash). Only deterministic graphs are cacheable, and
  determinism is declared once at session admission — a session carrying nondeterministic ops is marked
  uncacheable at admission, never discovered per call.
- Model swap invalidates by tag = session fingerprint; hit/miss class joins the run receipt as route evidence.
- Negative caching is admitted with its own key class: schema-invalid inputs cache their typed rejection keyed
  by input hash — repeated malformed payloads cost one validation, and the cached rejection carries the same
  evidence the live rejection would.

## divergent — ortvalue-iobinding

- The maximal form is one residency lattice: managed span → memory-backed value → device residency via binding →
  output value → span view — with copies legal at exactly the two named points.
- Every accelerator the process will ever meet lands as residency rows plus device-binding columns; the
  inference loop itself never changes. The growth proof: adding an accelerator is zero code in the loop, one row
  in the residency table.
- Trap taxonomy at the value boundary: lifetime inversion (backing owner repooled under a live value — the
  value's dispose is the owner's release point); shape staleness (rebind protocol); latch sharing (`Terminate`
  scope = instance sharing scope); result-collection leak (native memory invisible to GC pressure heuristics —
  disposal is deterministic or it is a leak).
- Cross-lane composition stated once: a tensor-lane owner and a staging-lane pooled plane are the same row to
  the inference loop — the lattice is gate-blind, which is what lets the staging axis evolve without touching
  inference code.
- The mutable egress projection (`GetTensorMutableDataAsSpan`) enables in-place post-processing on output
  buffers before any copy — normalization or thresholding of results costs zero allocations when it rides the
  output view, and the tensor-lane in-place law (same-start aliasing only) governs the kernel applied to it.

## divergent — session-lifecycle-ep

- Session identity is a value, and everything keys on it: the cache, the receipts, the result-cache tags, the
  warmup ledger. The fingerprint record is the one place identity is computed; call sites composing their own
  keys from fragments is the aliasing defect that produces wrong-model-served incidents.
- The admission choreography is one receipted sequence: probe providers → veto and degrade rows → build session
  (assets, overrides, pinned dims) → warmup golden run → equivalence check → publish to cache. Each step emits
  into one admission receipt.
- A session reachable by consumers without a complete admission receipt is the structural defect: every later
  mystery — why slow, why wrong, why here — resolves from the receipt instead of reproduction.
- Failure rows are typed and exhaustive: asset absent (refuse with asset evidence); metadata mismatch (typed
  schema rejection naming the slot); provider compile failure (degrade row + receipt); device memory cap breach
  (fall to CPU row + receipt); warmup equivalence breach (session refused — a fast wrong model is the worst
  admitted object).
- The CPU terminal row makes all-rows-vetoed unreachable for selection but not for admission: equivalence
  failure refuses even the terminal row, because correctness gates admission while capability only gates
  routing.
- The provider fold is the inference instantiation of the dispatch lane's generic substrate fold: this lane owns
  the provider-specific columns — availability veto, compatibility veto, device-policy cost, memory cap — and
  the row vocabulary; the fold grammar itself is the sibling's law, composed.
- The custom-selection delegate is where measured evidence enters provider choice: a suite that has measured
  per-device latency feeds its claim table into the delegate's fold, making provider selection benchmark-gated
  by the same discipline as kernel routing — one gating philosophy across both.

## divergent — extensions-caching

- Cache admission is itself a small fold over (determinism, payload size class, expected hit rate): cache row,
  bypass row, or negative-only row — decided at session admission, recorded in the admission receipt.
- Caching jumbo outputs evicts everything else for one entry; caching low-repeat workloads pays hash cost for
  nothing; both are vetoed by declaration, not discovered by production regression.
- Input content hashing is a staging concern composed here: the hash rides the already-flattened input spans —
  one pass over memory the loop already touches, before the run; hashing a re-serialized projection of the input
  is the double-work spelling.
- The pre/post-processing-in-graph law has a cache corollary: when tokenization lives in the graph, the cache
  key hashes raw boundary input (text bytes), making the cache useful across callers that would tokenize
  identically; external preprocessing moves the key downstream of code that may drift, silently splitting cache
  populations per caller version.
- Adapter-aware keys: with run-time adapters active, the adapter set is a key column — caching base-model
  results against adapter-modified runs is the aliasing defect; an empty adapter set is the base row, so one key
  shape serves both.
