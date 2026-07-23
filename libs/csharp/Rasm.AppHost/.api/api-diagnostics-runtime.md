# [RASM_APPHOST_API_DIAGNOSTICS_RUNTIME]

`Microsoft.Diagnostics.Runtime` (ClrMD) owns post-capture managed triage: it reads a captured minidump back into a live CLR view and walks the GC heap, managed threads, stack traces, and in-flight exceptions. It is the categorical owner of dump heap/thread/stack analysis, admitting a `Microsoft.Diagnostics.NETCore.Client` dump artifact and projecting it into structured support-bundle receipts, never a hand-rolled minidump parser.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.Runtime`
- package: `Microsoft.Diagnostics.Runtime` (MIT)
- assembly: `Microsoft.Diagnostics.Runtime`
- namespace: `Microsoft.Diagnostics.Runtime`
- asset: managed runtime library; the matching DAC resolves through `DataTargetOptions.SymbolPaths`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: target and runtime roots

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :----------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `DataTarget` | class         | one opened dump or attached process; enumerates `ClrVersions`, modules |
|  [02]   | `ClrInfo`    | class         | one detected CLR; `Version`/`Flavor` and `CreateRuntime()`             |
|  [03]   | `ClrRuntime` | class         | one live CLR view over `Heap`, `Threads`, `AppDomains`, modules        |
|  [04]   | `ModuleInfo` | class         | one loaded image; base, size, build id, version                        |

[PUBLIC_TYPE_SCOPE]: heap and object surfaces

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [CAPABILITY]                                                               |
| :-----: | :----------- | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `ClrHeap`    | class           | GC heap walker; `EnumerateObjects`/`Segments`/`EnumerateRoots`, corruption |
|  [02]   | `ClrSegment` | class           | one GC segment; `ObjectRange`, `Generation0/1/2`, committed/reserved       |
|  [03]   | `ClrObject`  | readonly struct | one managed object; `Address`/`Type`/`Size`, field reads, reference walk   |
|  [04]   | `ClrType`    | class           | one method-table type; `Name`, `Heap`, `IsFree`/`IsArray`                  |
|  [05]   | `ClrRoot`    | class           | one GC root; `Object`, `RootKind`, `IsInterior`/`IsPinned`                 |

[PUBLIC_TYPE_SCOPE]: thread, stack, and fault surfaces

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :-------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `ClrThread`     | class         | `OSThreadId`/`ManagedThreadId`, `GCMode`, `State`, stack-trace walk |
|  [02]   | `ClrStackFrame` | class         | `InstructionPointer`/`StackPointer`, `Method`, `Kind`, `FrameName`  |
|  [03]   | `ClrException`  | class         | `Message`/`HResult`/`Type`/`Inner`, `StackTrace`, throwing `Thread` |
|  [04]   | `ClrMethod`     | class         | one method behind a frame; `Name`, `MethodDesc`, native code range  |

[PUBLIC_TYPE_SCOPE]: policy and bounded vocabulary

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `DataTargetOptions` | class         | symbol paths, DAC verification, and the `Limits` frame/thread caps             |
|  [02]   | `ClrFlavor`         | enum          | `Desktop=0`/`Core=3`/`NativeAOT=4` — which CLR the DAC targets                 |
|  [03]   | `GCMode`            | enum          | `Cooperative`/`Preemptive` — a thread's GC mode at capture                     |
|  [04]   | `ClrStackFrameKind` | enum          | `Unknown`/`ManagedMethod`/`Runtime` — frame classification                     |
|  [05]   | `ClrRootKind`       | enum          | stack, handle, and finalizer-queue root provenance                             |
|  [06]   | `GCSegmentKind`     | enum          | `Generation0/1/2`/`Large`/`Pinned`/`Frozen`/`Ephemeral` segment classification |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: target construction — each surface returns a `DataTarget` root

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `DataTarget.LoadDump(string, DataTargetOptions?)`     | static  | open a captured minidump read-only           |
|  [02]   | `DataTarget.LoadDump(string, Stream, bool, options?)` | static  | open a dump streamed under a leave-open flag |
|  [03]   | `DataTarget.AttachToProcess(int, bool, options?)`     | static  | attach a live process, suspend optional      |
|  [04]   | `DataTarget.CreateSnapshotAndAttach(int, options?)`   | static  | snapshot a live process and attach           |

[ENTRYPOINT_SCOPE]: runtime materialization

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `DataTarget.ClrVersions -> ImmutableArray<ClrInfo>`        | property | every CLR the DAC recognizes               |
|  [02]   | `ClrInfo.CreateRuntime() -> ClrRuntime`                    | instance | bind the matching DAC, symbol-resolved     |
|  [03]   | `ClrInfo.CreateRuntime(string, bool) -> ClrRuntime`        | instance | bind an explicit DAC path, ignore mismatch |
|  [04]   | `DataTarget.EnumerateModules() -> IEnumerable<ModuleInfo>` | instance | every loaded native image                  |

[ENTRYPOINT_SCOPE]: heap triage

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `ClrRuntime.Heap -> ClrHeap`                                      | property | the managed GC heap walker     |
|  [02]   | `ClrHeap.Segments -> ImmutableArray<ClrSegment>`                  | property | heap partitioned by generation |
|  [03]   | `ClrHeap.EnumerateObjects(bool) -> IEnumerable<ClrObject>`        | instance | stream every managed object    |
|  [04]   | `ClrHeap.EnumerateRoots() -> IEnumerable<ClrRoot>`                | instance | the live-root set              |
|  [05]   | `ClrHeap.EnumerateFinalizableObjects() -> IEnumerable<ClrObject>` | instance | the finalizer queue            |
|  [06]   | `ClrHeap.GetObject(ulong) -> ClrObject`                           | instance | one object by address          |
|  [07]   | `ClrHeap.IsObjectCorrupted(ulong, out ObjectCorruption?) -> bool` | instance | heap-corruption verification   |

[ENTRYPOINT_SCOPE]: thread and stack triage

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `ClrRuntime.Threads -> ImmutableArray<ClrThread>`                        | property | the managed-thread set            |
|  [02]   | `ClrThread.EnumerateStackTrace(bool) -> IEnumerable<ClrStackFrame>`      | instance | ordered frames, default frame cap |
|  [03]   | `ClrThread.EnumerateStackTrace(bool, int) -> IEnumerable<ClrStackFrame>` | instance | frames bounded by `maxFrames`     |
|  [04]   | `ClrThread.CurrentException -> ClrException?`                            | property | the in-flight fault               |
|  [05]   | `ClrException.StackTrace -> ImmutableArray<ClrStackFrame>`               | property | the fault's own frames            |
|  [06]   | `ClrObject.AsException() -> ClrException?`                               | instance | reinterpret an object as a fault  |

[STACK_WALK_LAW]: `ClrThread.EnumerateStackTrace` bounds the walk by `maxFrames`, defaulting to `DataTargetOptions.Limits.MaxStackFrames`; `ClrStackFrame.Kind` discriminates a managed frame (carrying a `ClrMethod`) from a runtime frame (carrying only a `FrameName`), so the projection reads `Method` only on a managed `Kind`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A triage pass opens the minidump read-only through `DataTarget.LoadDump`, then `ClrInfo.CreateRuntime()` binds the matching DAC to a live `ClrRuntime` view.
- A heap census streams through `ClrHeap.EnumerateObjects`: `ClrObject` is a `readonly struct` over an address and its `ClrType`, so the walk allocates nothing per object.
- `ClrRuntime.Threads` with `EnumerateStackTrace` and `CurrentException` is the crash-summary spine — ordered stack frames and the in-flight `ClrException` per thread.
- `DataTarget`, `ClrRuntime`, and the DAC service graph are `IDisposable`: a pass materializes, drains the enumerations, and disposes, so the dump handle never spans a capture window.

[STACKING]:
- `Microsoft.Diagnostics.NETCore.Client`(`.api/api-diagnostics-client.md`): `DiagnosticsClient.WriteDump(DumpType, string)` writes the dump and `DataTarget.LoadDump(string)` reads it back — the sole capture→triage hand-off, composed as one `dump` + `dump-triage` receipt pair.
- support-bundle projection fan (`bundles.md`): a `dump-triage` artifact folds `DataTarget.LoadDump -> ClrRuntime` into a bounded `heap-summary` (type histogram by `ClrType.Name`, segment sizes, root counts) and a `thread-summary` (per-`ClrThread` top-N frames, any `ClrException`) as `SupportReceipt` rows; DAC and corruption faults fold to the typed `SupportFault`.

[LOCAL_ADMISSION]:
- Triage depth is policy DATA on the artifact row, not a call-site literal: frame count rides `EnumerateStackTrace(bool, int)` `maxFrames`, histogram breadth rides the enumeration cap, and a full `EnumerateObjects` census runs only under an explicit escalation trigger, so a multi-gigabyte dump never fully walks on a routine window.
- DAC-resolution and corruption faults (`ClrDiagnosticsException`, a missing or mismatched DAC, `ClrHeap.CanWalkHeap == false`, `IsObjectCorrupted`) fold to the typed `SupportFault` and `SupportReceipt.Partial` for the failed summary, never a thrown exception aborting the bundle.
- Triage binds captured artifacts and the host/companion process tree the AppHost owns, self-attaching through `AttachToProcess`/`CreateSnapshotAndAttach` only for live escalation, never a general remote-process attach, and rides `DeadlineClass` bounds so a slow symbol fetch degrades one summary row.
- Every triage summary is a `SupportReceipt` row carrying a `Rasm.Domain.ContentHash.Of` hash over the payload; the source dump path is recorded, never the dump bytes crossing the wire.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.Runtime`
- Owns: post-capture heap/thread/stack/exception triage over a captured dump or an escalation self-attach on the host/companion process tree
- Accept: `DataTarget.LoadDump` over a `NETCore.Client` dump, `CreateRuntime()` DAC binding, bounded heap/thread projection as row policy, and disposal inside the window
- Reject: a hand-rolled minidump parser, an unbounded full-heap census on a routine window, a general remote-process attach, or a thrown DAC/corruption fault crossing the bundle pipeline
