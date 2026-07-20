# [RASM_APPHOST_API_DIAGNOSTICS_RUNTIME]

`Microsoft.Diagnostics.Runtime` (ClrMD, dotnet/diagnostics) is the managed dump-analysis engine: `DataTarget.LoadDump(path)` opens a captured minidump, `ClrInfo.CreateRuntime()` materializes a `ClrRuntime`, and that runtime walks the GC heap (`ClrHeap.EnumerateObjects`/`Segments`/`EnumerateRoots`), the managed threads (`ClrThread.EnumerateStackTrace`), and the in-flight exceptions (`ClrThread.CurrentException`). ClrMD is the categorical owner of POST-capture heap/thread/stack triage.

Capture (`api-diagnostics-client.md`) writes the dump; this catalog reads it back, turning a `Microsoft.Diagnostics.NETCore.Client` dump artifact into structured support-bundle receipts rather than a hand-rolled minidump parser.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.Runtime`
- package: `Microsoft.Diagnostics.Runtime`
- license: `MIT`
- assembly: `Microsoft.Diagnostics.Runtime`
- namespace: `Microsoft.Diagnostics.Runtime`
- dependency: managed collection and memory primitives
- asset: managed runtime library; matching DAC resolves through symbol paths at runtime
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: target and runtime roots
- rail: observability

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]             | [RAIL]                                                                 |
| :-----: | :----------- | :------------------------ | :--------------------------------------------------------------------- |
|  [01]   | `DataTarget` | sealed `IDisposable` root | one opened dump/attached process — enumerates `ClrVersions`, `Modules` |
|  [02]   | `ClrInfo`    | CLR descriptor            | one detected CLR in the target — `Version`/`Flavor`, `CreateRuntime()` |
|  [03]   | `ClrRuntime` | sealed `IDisposable`      | one live CLR view — `Heap`, `Threads`, `AppDomains`, module walk       |
|  [04]   | `ModuleInfo` | native module descriptor  | one loaded image — base, size, build id, version                       |

[PUBLIC_TYPE_SCOPE]: heap and object surfaces
- rail: observability

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [RAIL]                                                                    |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `ClrHeap`    | GC heap walker    | `EnumerateObjects`/`Segments`/`EnumerateRoots`, corruption verification   |
|  [02]   | `ClrSegment` | heap segment      | one GC segment — `ObjectRange`, `Generation0/1/2`, committed/reserved     |
|  [03]   | `ClrObject`  | `readonly struct` | one managed object — `Address`/`Type`/`Size`, field reads, reference walk |
|  [04]   | `ClrType`    | type descriptor   | one method-table type — `Name`, `Heap`, `IsFree`/`IsArray`                |
|  [05]   | `ClrRoot`    | GC root           | one root — `Object`, `RootKind`, `IsInterior`/`IsPinned`                  |

[PUBLIC_TYPE_SCOPE]: thread, stack, and fault surfaces
- rail: observability

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [RAIL]                                                              |
| :-----: | :-------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `ClrThread`     | managed thread | `OSThreadId`/`ManagedThreadId`, `GCMode`, `State`, stack-trace walk |
|  [02]   | `ClrStackFrame` | stack frame    | `InstructionPointer`/`StackPointer`, `Method`, `Kind`, `FrameName`  |
|  [03]   | `ClrException`  | managed fault  | `Message`/`HResult`/`Type`/`Inner`, `StackTrace`, throwing `Thread` |
|  [04]   | `ClrMethod`     | jitted method  | one method behind a frame — name, method-table, native code range   |

[PUBLIC_TYPE_SCOPE]: policy and bounded vocabulary
- rail: observability

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                                                                         |
| :-----: | :------------------ | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `DataTargetOptions` | options record | symbol paths, DAC verification, complete-enumeration, lock-free map reader     |
|  [02]   | `ClrFlavor`         | enum           | `Desktop=0`/`Core=3`/`NativeAOT=4` — which CLR the DAC targets                 |
|  [03]   | `GCMode`            | enum           | `Cooperative`/`Preemptive` — a thread's GC mode at capture                     |
|  [04]   | `ClrStackFrameKind` | enum           | `Unknown`/`ManagedMethod`/`Runtime` — frame classification                     |
|  [05]   | `ClrRootKind`       | enum           | stack/handle/finalizer-queue root provenance                                   |
|  [06]   | `GCSegmentKind`     | enum           | `Generation0/1/2`/`Large`/`Pinned`/`Frozen`/`Ephemeral` segment classification |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: target construction
- rail: observability

| [INDEX] | [MEMBER]                                                                             | [KIND]      | [RETURN]     |
| :-----: | :----------------------------------------------------------------------------------- | :---------- | :----------- |
|  [01]   | `DataTarget.LoadDump(string filePath, DataTargetOptions? options = null)`            | static call | `DataTarget` |
|  [02]   | `DataTarget.LoadDump(string displayName, Stream stream, bool leaveOpen, options?)`   | static call | `DataTarget` |
|  [03]   | `DataTarget.AttachToProcess(int processId, bool suspend, DataTargetOptions? = null)` | static call | `DataTarget` |
|  [04]   | `DataTarget.CreateSnapshotAndAttach(int processId, DataTargetOptions? = null)`       | static call | `DataTarget` |

[ENTRYPOINT_SCOPE]: runtime materialization
- rail: observability

| [INDEX] | [MEMBER]                                                     | [KIND]   | [RETURN]                  |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `DataTarget.ClrVersions`                                     | property | `ImmutableArray<ClrInfo>` |
|  [02]   | `ClrInfo.CreateRuntime()`                                    | call     | `ClrRuntime`              |
|  [03]   | `ClrInfo.CreateRuntime(string dacPath, bool ignoreMismatch)` | call     | `ClrRuntime`              |
|  [04]   | `DataTarget.EnumerateModules()`                              | call     | `IEnumerable<ModuleInfo>` |

[ENTRYPOINT_SCOPE]: heap triage
- rail: observability

| [INDEX] | [MEMBER]                                                  | [RETURN]                     |
| :-----: | :-------------------------------------------------------- | :--------------------------- |
|  [01]   | `ClrRuntime.Heap`                                         | `ClrHeap`                    |
|  [02]   | `ClrHeap.Segments`                                        | `ImmutableArray<ClrSegment>` |
|  [03]   | `ClrHeap.EnumerateObjects(bool carefully = false)`        | `IEnumerable<ClrObject>`     |
|  [04]   | `ClrHeap.EnumerateRoots()`                                | `IEnumerable<ClrRoot>`       |
|  [05]   | `ClrHeap.EnumerateFinalizableObjects()`                   | `IEnumerable<ClrObject>`     |
|  [06]   | `ClrHeap.GetObject(ulong objRef)`                         | `ClrObject`                  |
|  [07]   | `ClrHeap.IsObjectCorrupted(ulong, out ObjectCorruption?)` | `bool`                       |

[ENTRYPOINT_SCOPE]: thread and stack triage
- rail: observability

| [INDEX] | [MEMBER]                                                            | [RETURN]                        |
| :-----: | :------------------------------------------------------------------ | :------------------------------ |
|  [01]   | `ClrRuntime.Threads`                                                | `ImmutableArray<ClrThread>`     |
|  [02]   | `ClrThread.EnumerateStackTrace(bool includeContext = false)`        | `IEnumerable<ClrStackFrame>`    |
|  [03]   | `ClrThread.EnumerateStackTrace(bool includeContext, int maxFrames)` | `IEnumerable<ClrStackFrame>`    |
|  [04]   | `ClrThread.CurrentException`                                        | `ClrException?`                 |
|  [05]   | `ClrException.StackTrace`                                           | `ImmutableArray<ClrStackFrame>` |
|  [06]   | `ClrObject.AsException()`                                           | `ClrException?`                 |

[STACK_WALK_LAW]: `ClrThread.EnumerateStackTrace(includeContext, maxFrames)` bounds the walk by `maxFrames` (defaulting to `DataTargetOptions.Limits.MaxStackFrames`); `ClrStackFrame.Kind` (`ClrStackFrameKind`) discriminates managed frames (carrying a `ClrMethod`) from runtime frames (carrying only a `FrameName`), and `InstructionPointer`/`StackPointer` anchor each frame — the projection reads `Method` only when `Kind` is managed.

## [04]-[IMPLEMENTATION_LAW]

[TRIAGE_TOPOLOGY]:
- `DataTarget.LoadDump(filePath)` opens a captured minidump read-only; `ClrVersions` enumerates every CLR the DAC recognizes in the image, and `ClrInfo.CreateRuntime()` binds the matching DAC (resolved through `DataTargetOptions.SymbolPaths`, defaulting to the public symbol server) to produce a `ClrRuntime`. A dump captured by `Microsoft.Diagnostics.NETCore.Client` (`api-diagnostics-client.md` `WriteDump`) is the exact input; the two catalogs compose one capture→triage seam, never two dump paths.
- `ClrRuntime.Heap` walks the managed heap: `Segments` partitions by generation (`Generation0/1/2`) and `GCSegmentKind`, `EnumerateObjects` yields every `ClrObject` (with `Type`, `Size`, reference edges), and `EnumerateRoots` yields the live-root set. `ClrObject` is a `readonly struct` over an address and its `ClrType`, so a heap census streams without per-object allocation.
- `ClrRuntime.Threads` is the managed-thread set; each `ClrThread.EnumerateStackTrace` yields ordered `ClrStackFrame` rows, and `CurrentException` projects the in-flight `ClrException` (`Message`, `HResult`, `Inner` chain, `StackTrace`). Thread and exception triage is the crash-summary spine of a support bundle.
- `DataTarget`, `ClrRuntime`, and the DAC service graph are `IDisposable`; a triage pass opens the target, materializes the runtime, drains the enumerations, and disposes — the dump handle is never held across a capture window.

[LOCAL_ADMISSION]:
- Post-capture triage fills the support-bundle projection fan: a `dump-triage` artifact adapts `DataTarget.LoadDump` → `ClrRuntime` and projects a bounded `heap-summary` (type histogram by `ClrType.Name`, segment sizes, root counts) and a `thread-summary` (per-`ClrThread` top-N frames and any `ClrException`) into `SupportReceipt` manifest rows — never the raw dump re-streamed into telemetry.
- Triage depth is policy DATA on the artifact row, not a call-site literal: frame count rides `ClrThread.EnumerateStackTrace(includeContext, maxFrames)`, object-histogram breadth rides the enumeration cap, and a full `EnumerateObjects` heap census runs only under an explicit escalation trigger so a multi-gigabyte dump never fully walks on a routine window.
- DAC-resolution and corruption faults (`ClrDiagnosticsException`, a missing/mismatched DAC, `ClrHeap.CanWalkHeap == false`, `IsObjectCorrupted`) map to the projection fan's typed `SupportFault` (registry band 4810), folded into `SupportReceipt.Partial` for the failed summary — never a thrown exception aborting the bundle, and never a bare `Error.New`.
- Triage is bounded to captured artifacts and the host/companion process tree the AppHost owns (self-attach via `AttachToProcess`/`CreateSnapshotAndAttach` only for live escalation); it is never a general remote-process attach surface, and triage rides `DeadlineClass` bounds so a slow symbol fetch or a wedged DAC degrades one summary row rather than the whole pipeline.

[STACK]:
- capture → triage seam: `DiagnosticsClient.WriteDump(dumpType, path)` (`api-diagnostics-client.md`) writes the dump; `DataTarget.LoadDump(path)` here reads it back — the single verified hand-off between dump capture and dump analysis, composed as one `dump` + `dump-triage` receipt pair.
- heap projection: `ClrHeap.EnumerateObjects` + `ClrObject.Type.Name` fold into a type histogram row on the `SupportReceipt`; segment sizing reads `ClrSegment.CommittedMemory`/`Generation*` ranges — the heap summary is a projection, never the raw object graph.
- fault band: triage failures land as the typed `SupportFault` case in registry band 4810, sharing the capture fan's fault band so capture and triage faults reconcile on one receipt.
- manifest receipt: every triage summary is a `SupportReceipt` row carrying a `Rasm.Domain.ContentHash.Of` content hash over the summary payload; the source dump path is recorded, never the dump bytes crossing the wire.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.Runtime`
- Owns: post-capture heap/thread/stack/exception triage over a captured dump or an escalation self-attach for the host/companion process tree
- Accept: `DataTarget.LoadDump` over a `NETCore.Client` dump, `CreateRuntime()` DAC binding, bounded heap/thread projection as row policy, and disposal inside the window
- Reject: a hand-rolled minidump parser, an unbounded full-heap census on a routine window, a general remote-process attach, or a thrown DAC/corruption fault crossing the bundle pipeline
