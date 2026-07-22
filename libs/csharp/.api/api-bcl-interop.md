# [RASM_API_BCL_INTEROP]

`System.Runtime.InteropServices` is the shared-framework inbox for every managed-to-native boundary the branch crosses: portable POSIX signal registration, vendored native-module resolution, source-generated P/Invoke marshalling, native handle custody, and zero-copy reinterpretation of managed storage. One owner holds each concern — the registration object, the resolver delegate, the `SafeHandle` subclass, the marshal projector — so a native seam binds, frees, and re-types through this surface instead of its own kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Runtime.InteropServices`
- package: `System.Runtime.InteropServices` (MIT, Microsoft)
- assembly: `System.Runtime.InteropServices.dll`, `System.Runtime.dll`, `System.Collections.Immutable.dll` (shared framework)
- namespace: `System.Runtime.InteropServices`, `Microsoft.Win32.SafeHandles`
- rail: native boundary — signal traps, module binding, handle lifetime, span aliasing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: portable POSIX signal seam

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [CAPABILITY]                        |
| :-----: | :------------------------ | :---------------- | :---------------------------------- |
|  [01]   | `PosixSignalRegistration` | sealed disposable | one live handler attachment         |
|  [02]   | `PosixSignal`             | enum              | portable signal vocabulary          |
|  [03]   | `PosixSignalContext`      | sealed class      | per-delivery signal and cancel flag |

[PosixSignal]: `SIGHUP` `SIGINT` `SIGQUIT` `SIGTERM` `SIGCHLD` `SIGCONT` `SIGWINCH` `SIGTTIN` `SIGTTOU` `SIGTSTP`

[PUBLIC_TYPE_SCOPE]: native-module binding and generated marshalling

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `NativeLibrary`                 | static facade | module load and export lookup     |
|  [02]   | `DllImportResolver`             | delegate      | per-assembly resolution override  |
|  [03]   | `DllImportSearchPath`           | flags enum    | probe-path policy on load         |
|  [04]   | `LibraryImportAttribute`        | attribute     | source-generated P/Invoke stub    |
|  [05]   | `StringMarshalling`             | enum          | generated-stub transcoding mode   |
|  [06]   | `UnmanagedCallersOnlyAttribute` | attribute     | managed method as native callback |
|  [07]   | `SuppressGCTransitionAttribute` | attribute     | drops the GC transition on a leaf |

[PUBLIC_TYPE_SCOPE]: handle custody and zero-copy projection

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]  | [CAPABILITY]                         |
| :-----: | :---------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `SafeHandle`                        | abstract class | ref-counted finalizer-safe handle    |
|  [02]   | `SafeHandleZeroOrMinusOneIsInvalid` | abstract class | handle base with `0`/`-1` sentinels  |
|  [03]   | `GCHandle`                          | struct         | pins or roots a managed object       |
|  [04]   | `Marshal`                           | static facade  | unmanaged memory and string interop  |
|  [05]   | `MemoryMarshal`                     | static facade  | span reinterpretation and byte codec |
|  [06]   | `CollectionsMarshal`                | static facade  | refs into BCL collection storage     |
|  [07]   | `ImmutableCollectionsMarshal`       | static facade  | `ImmutableArray<T>` array aliasing   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: signal registration and delivery

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `PosixSignalRegistration.Create(PosixSignal, Action<PosixSignalContext>)` | static   | attaches one handler to the signal |
|  [02]   | `PosixSignalRegistration.Dispose()`                                       | instance | detaches the handler               |
|  [03]   | `PosixSignalContext.Signal -> PosixSignal`                                | property | the delivered signal               |
|  [04]   | `PosixSignalContext.Cancel -> bool`                                       | property | suppresses the default action      |
|  [05]   | `new PosixSignalContext(PosixSignal)`                                     | ctor     | mints a delivery payload           |

- `PosixSignalRegistration.Create`: throws `PlatformNotSupportedException` on a refused signal and `IOException` on a failed install; the handler runs on a thread-pool thread rather than the OS signal context, so it may block and allocate, and every registration for one signal fires.
- `PosixSignalContext.Cancel`: a delivery left `false` runs the runtime's default action once every handler returns.

[ENTRYPOINT_SCOPE]: native-module resolution

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------- |
|  [01]   | `NativeLibrary.SetDllImportResolver(Assembly, DllImportResolver)`         | static  | installs a per-assembly resolver |
|  [02]   | `NativeLibrary.TryLoad(string, out nint) -> bool`                         | static  | probe-loads a module by path     |
|  [03]   | `NativeLibrary.TryLoad(string, Assembly, DllImportSearchPath?, out nint)` | static  | probe-loads by name under policy |
|  [04]   | `NativeLibrary.Load(string, Assembly, DllImportSearchPath?) -> nint`      | static  | loads by name or throws          |
|  [05]   | `NativeLibrary.TryGetExport(nint, string, out nint) -> bool`              | static  | resolves one export address      |
|  [06]   | `NativeLibrary.GetMainProgramHandle() -> nint`                            | static  | handle to the process image      |
|  [07]   | `NativeLibrary.Free(nint)`                                                | static  | releases a loaded module         |

- `DllImportResolver(string, Assembly, DllImportSearchPath?) -> nint`: returning `IntPtr.Zero` falls through to the default probe order, so one resolver overrides a single module name and delegates the rest.

[ENTRYPOINT_SCOPE]: handle custody and native memory

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `SafeHandle.DangerousAddRef(ref bool)`                    | instance | pins the handle across a raw call |
|  [02]   | `SafeHandle.DangerousRelease()`                           | instance | drops one reference               |
|  [03]   | `SafeHandle.DangerousGetHandle() -> nint`                 | instance | reads the raw handle              |
|  [04]   | `SafeHandle.ReleaseHandle() -> bool`                      | instance | subclass-owned native release     |
|  [05]   | `SafeHandle.SetHandleAsInvalid()`                         | instance | marks the handle closed           |
|  [06]   | `GCHandle.Alloc(object?, GCHandleType) -> GCHandle`       | static   | roots or pins a managed object    |
|  [07]   | `GCHandle.AddrOfPinnedObject() -> nint`                   | instance | address of the pinned payload     |
|  [08]   | `GCHandle.Free()`                                         | instance | releases the root                 |
|  [09]   | `Marshal.SizeOf<T>() -> int`                              | static   | unmanaged size of a struct        |
|  [10]   | `Marshal.StringToCoTaskMemUTF8(string?) -> nint`          | static   | allocates a UTF-8 native string   |
|  [11]   | `Marshal.PtrToStringUTF8(nint) -> string?`                | static   | reads a native UTF-8 string       |
|  [12]   | `Marshal.FreeCoTaskMem(nint)`                             | static   | releases a task-allocator block   |
|  [13]   | `Marshal.GetLastPInvokeError() -> int`                    | static   | last native error from a stub     |
|  [14]   | `Marshal.GetDelegateForFunctionPointer<TDelegate>(nint)`  | static   | binds a native function pointer   |
|  [15]   | `Marshal.GetFunctionPointerForDelegate(Delegate) -> nint` | static   | exposes a managed callback        |

- `SafeHandle.DangerousAddRef`: pairs with `DangerousRelease` in one `finally`; the `ref bool` reports whether the count was taken.
- `GCHandle.Free`: an unfreed handle roots its target for process life.

[ENTRYPOINT_SCOPE]: zero-copy projection

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `MemoryMarshal.Cast<TFrom, TTo>(Span<TFrom>) -> Span<TTo>`                     | static  | re-types a span element        |
|  [02]   | `MemoryMarshal.AsBytes<T>(Span<T>) -> Span<byte>`                              | static  | views a span as raw bytes      |
|  [03]   | `MemoryMarshal.CreateSpan<T>(ref T, int) -> Span<T>`                           | static  | spans a bare reference         |
|  [04]   | `MemoryMarshal.GetReference<T>(Span<T>) -> ref T`                              | static  | first-element reference        |
|  [05]   | `MemoryMarshal.GetArrayDataReference<T>(T[]) -> ref T`                         | static  | array data reference           |
|  [06]   | `MemoryMarshal.TryRead<T>(ReadOnlySpan<byte>, out T) -> bool`                  | static  | decodes one struct from bytes  |
|  [07]   | `MemoryMarshal.TryWrite<T>(Span<byte>, in T) -> bool`                          | static  | encodes one struct to bytes    |
|  [08]   | `MemoryMarshal.TryGetArray<T>(ReadOnlyMemory<T>, out ArraySegment<T>) -> bool` | static  | recovers the backing array     |
|  [09]   | `MemoryMarshal.CreateFromPinnedArray<T>(T[]?, int, int) -> Memory<T>`          | static  | memory over a pinned array     |
|  [10]   | `CollectionsMarshal.AsSpan<T>(List<T>?) -> Span<T>`                            | static  | spans list backing storage     |
|  [11]   | `CollectionsMarshal.SetCount<T>(List<T>, int)`                                 | static  | resizes without element writes |
|  [12]   | `CollectionsMarshal.GetValueRefOrAddDefault(Dictionary, TKey, out bool)`       | static  | one-probe insert-or-update ref |
|  [13]   | `CollectionsMarshal.GetValueRefOrNullRef(Dictionary, TKey) -> ref TValue`      | static  | one-probe lookup ref           |
|  [14]   | `ImmutableCollectionsMarshal.AsImmutableArray<T>(T[]?) -> ImmutableArray<T>`   | static  | wraps an array without copying |
|  [15]   | `ImmutableCollectionsMarshal.AsArray<T>(ImmutableArray<T>) -> T[]?`            | static  | unwraps to the backing array   |

- `CollectionsMarshal.AsSpan` and `GetValueRefOrAddDefault`: the span and the ref die on the next structural edit of their collection.
- `MemoryMarshal.GetReference`: an empty span yields a reference that is compared, never dereferenced.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every managed signal handler attaches through `PosixSignalRegistration.Create` and detaches through the returned registration's `Dispose`.
- `PosixSignal` members carry abstract values the runtime maps to each platform's real signal number, so a handler never spells a numeric constant; a member marked `[UnsupportedOSPlatform("windows")]` registers on Unix hosts alone while `SIGHUP`/`SIGINT`/`SIGQUIT`/`SIGTERM` register everywhere.
- One `DllImportResolver` per assembly owns native-module resolution; `LibraryImportAttribute` generates the stub and `StringMarshalling` fixes its transcoding.
- A native resource lives in a `SafeHandle` subclass whose `ReleaseHandle` is its only free path, and the reference count guards the handle across every raw call.
- Reinterpretation aliases rather than copies, so the storage owner outlives every span, memory, and ref this surface projects from it.

[STACKING]:
- `api-languageext`(`.api/api-languageext.md`): `Create`, `Load`, and `GetExport` throw, so each enters `Try`/`Eff` and lands on `Fin<A>`; the `IDisposable` registrations collect in an `Atom<Seq<IDisposable>>` cell disposed in reverse-registration order.
- `api-tensors`(`.api/api-tensors.md`): `MemoryMarshal.Cast`/`AsBytes` re-type a coordinate buffer in place and `TensorMarshal` lifts the same reference into `TensorSpan<T>`, so a tensor view is never re-declared here.
- `api-highperformance`(`.api/api-highperformance.md`): `MemoryOwner<T>` and `ArrayPoolBufferWriter<T>` own the rented buffer while `MemoryMarshal.TryGetArray`/`CreateSpan` project it, keeping pooling and reinterpretation on separate owners.
- `api-hosting-lifetimes`(`Rasm.AppHost/.api/api-hosting-lifetimes.md`): `SystemdLifetime` owns sd_notify state and the watchdog keep-alive, so this surface carries the signal trap alone.
- `Rasm.AppHost` `Runtime/lifecycle`: the trap set is one `Create` per trapped signal folded into a LIFO detacher composite whose `Dispose` unwinds at drain, and the delivered `PosixSignal` rides the fault union's signalled case onto the wire.
- within-lib: one resolver folds the whole native seam — `SetDllImportResolver` intercepts every `LibraryImport` stub in the assembly, `TryLoad` under an explicit `DllImportSearchPath` resolves the vendored module, `TryGetExport` binds each entry point, and the returned `nint` enters a `SafeHandleZeroOrMinusOneIsInvalid` subclass whose `ReleaseHandle` calls `NativeLibrary.Free`.

[LOCAL_ADMISSION]:
- Target-framework resolution alone admits every member here; the branch owns no manifest row for this surface.
- A native entry point declares `LibraryImportAttribute` with an explicit `StringMarshalling`, so the source generator owns the stub body.
- A native resource is held by a `SafeHandle` subclass; a bare `nint` lives only inside the call that produced it.
- A managed callback handed across the boundary carries `[UnmanagedCallersOnly]` and stays rooted for the callback's whole lifetime.
- A span or ref taken through a marshal projector is consumed before its owner is structurally edited.

[RAIL_LAW]:
- Package: `System.Runtime.InteropServices`
- Owns: portable signal registration, native-module resolution and export binding, generated P/Invoke marshalling, native handle custody, and zero-copy reinterpretation of managed storage.
- Accept: one registration per trapped signal, one resolver per assembly, one `SafeHandle` subclass per native resource kind, and span aliasing whose owner outlives the view.
- Reject: a hand-rolled `sigaction` P/Invoke, a hand-written `DllImport` stub, a raw `nint` field held past its call, or a defensive buffer copy where an alias carries the same bytes.
