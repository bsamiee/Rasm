# [RASM_COMPUTE_API_LIBTORCH_CPU]

`libtorch-cpu` is a pure MSBuild meta-package — it ships NO managed assembly and NO public type surface. Its `lib/netstandard2.0/_._/empty.txt` is a deliberate empty lib folder; the entire payload is the `buildTransitive/netstandard2.0/libtorch-cpu.{props,targets}` import pair plus a per-RID native `dependencies` fan-out. It is the LibTorch (PyTorch C++ ATen) 2.10.0 native CPU runtime floor that the `TorchSharp` managed binding P/Invokes through `libLibTorchSharp.dylib`; `api-torchsharp.md` owns the managed `torch.*` surface and declares its native floor as THIS package, restating no native facts. The two are ONE logical admission: `TorchSharp 0.107.0` (managed) + `libtorch-cpu 2.10.0` (native). With no native floor resident, every `torch.*` call faults at native init (`THSTorch_*` entry-point load), so this owner pins the RID/ABI/license/mutual-exclusion contract the `Tensor/blas` and `Stats/estimator` rails build on.

The package fans out to exactly three RID sub-packages — `libtorch-cpu-linux-x64`, `libtorch-cpu-osx-arm64`, `libtorch-cpu-win-x64` (each `2.10.0`, `exclude="Build,Analyzers"`). There is NO `linux-arm64`, NO `win-arm64`, and NO `osx-x64` native CPU payload on this line, so the build's native floor exists only for those three RIDs; the workspace `osx-arm64` target resolves `libtorch-cpu-osx-arm64`. The `libtorch-cpu.targets` import injects a `CheckOneTorchSharpRuntime` MSBuild target (`AfterTargets=ResolveReferences`, `BeforeTargets=PrepareForBuild`) that emits a hard `<Error>` when both `$(TorchSharpCpuPackage)` and `$(TorchSharpCudaPackage)` are set — `libtorch-cpu` and `libtorch-cuda` are mutually exclusive in one project, so the Compute folder admits the CPU floor alone and never co-pins a CUDA runtime beside it. The nuspec sets `requireLicenseAcceptance=true`; the redistributed LibTorch binaries are MIT (the PyTorch + TorchSharp license expression), so the floor carries no copyleft burden into the managed closure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `libtorch-cpu`
- package: `libtorch-cpu`
- version: `2.10.0` (the LibTorch / PyTorch ATen native version it redistributes)
- license: `MIT` (license-expression in nuspec; `requireLicenseAcceptance=true` — explicit acceptance is part of the contract)
- assembly: NONE — `lib/netstandard2.0/_._/empty.txt` is an empty lib placeholder; this package contributes zero managed IL and zero public types
- namespace: none (no managed surface)
- asset: MSBuild props/targets import pair (`buildTransitive/netstandard2.0/libtorch-cpu.{props,targets}`) plus a per-RID native `dependencies` group that pulls the matching `libtorch-cpu-<rid>` native sub-package
- dependencies: `libtorch-cpu-linux-x64 2.10.0`, `libtorch-cpu-osx-arm64 2.10.0`, `libtorch-cpu-win-x64 2.10.0` (all `.NETStandard2.0` group, `exclude="Build,Analyzers"`) — the native dylib/so/dll carriers; ONLY these three RIDs ship a CPU payload
- consumer-bind note: a meta-package, not a TFM-bound assembly — the `[API_TFM_RESOLUTION]` hazard does NOT apply (there is no `lib/<tfm>` asset to misresolve). The native floor binds per-RID through the transitive sub-package, not through a managed `lib` asset
- mutual-exclusion: the `CheckOneTorchSharpRuntime` target HARD-ERRORS if `libtorch-cuda` is co-referenced; this folder pins the CPU floor exclusively
- rail: compute (`[CLASSICAL_ML_BLAS]` native floor for `Tensor/blas` + `Stats/estimator`)

## [02]-[NATIVE_ABI_FLOOR]

[PACKAGE_ASSET_SCOPE]: per-RID native CPU payload matrix (the OWNING native floor for the whole TorchSharp/LibTorch stack)
- rail: compute
- note: this is the canonical per-RID native-payload owner; `api-torchsharp.md` `native-floor` restates NOTHING — it declares its managed pin and the `libLibTorchSharp` shim, then defers the LibTorch ABI facts HERE. The `TorchSharp` package ships only the thin P/Invoke shim (`libLibTorchSharp.{dylib,so,dll}`); the heavy ATen/c10 native runtime is THIS floor. A `torch.*` call resolves `libLibTorchSharp` (TorchSharp's shim) -> `libtorch_cpu` + `libtorch` + `libc10` (this floor) at native init; a missing RID payload faults the first native entry-point load, never silently degrading.

| [INDEX] | [RID]         | [NATIVE_CPU_PAYLOAD]                                                                                                  | [TORCHSHARP_SHIM]              | [STATUS]                                  |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------------------------ | :----------------------------- | :---------------------------------------- |
|  [01]   | `osx-arm64`   | `libtorch.dylib`, `libtorch_cpu.dylib`, `libc10.dylib`, `libomp.dylib`, `libtorch_global_deps.dylib`, `libshm.dylib` | `libLibTorchSharp.dylib`       | the workspace target — VERIFIED host asset |
|  [02]   | `linux-x64`   | `libtorch.so` + `libtorch_cpu.so` + `libc10.so` + OpenMP runtime (`libtorch-cpu-linux-x64`)                          | `libLibTorchSharp.so`          | shipped sub-package                       |
|  [03]   | `win-x64`     | `torch.dll` + `torch_cpu.dll` + `c10.dll` (`libtorch-cpu-win-x64`)                                                   | `LibTorchSharp.dll`            | shipped sub-package                       |
|  [04]   | `linux-arm64` | NONE — no CPU payload on this line                                                                                  | `libLibTorchSharp.so` (ships)  | NO native floor — `torch.*` faults at init |
|  [05]   | `win-arm64`   | NONE — no CPU payload on this line                                                                                  | `LibTorchSharp.dll` (ships)    | NO native floor — `torch.*` faults at init |
|  [06]   | `osx-x64`     | NONE — no CPU payload on this line                                                                                  | `libLibTorchSharp.dylib` (ships)| NO native floor — `torch.*` faults at init |

[OPENMP_THREADING_FLOOR]:
- The osx-arm64 payload bundles `libomp.dylib` — the LLVM OpenMP runtime LibTorch's ATen ops parallelize over. ATen CPU intra-op parallelism rides this OpenMP pool, NOT the .NET thread pool, so the `Tensor/blas` lane's thread-count knob is `TorchSharp.torch.set_num_threads(int)` / `get_num_threads()` (the ATen/OMP intra-op count) and `set_num_interop_threads(int)`, each documented in `api-torchsharp.md`. The `OMP_NUM_THREADS` / `MKL_NUM_THREADS` environment variables read by `libomp` at native init are the only out-of-band override; the managed `set_num_threads` is the in-process rail the design uses.
- LibTorch ATen GEMM/factorization on Apple silicon dispatches through the macOS Accelerate BLAS/LAPACK backend compiled into `libtorch_cpu.dylib`; there is no separate MKL native asset on `osx-arm64` (contrast `MathNet.Numerics.Providers.MKL`, the x64-only managed BLAS provider). This is why TorchSharp is the chosen native dense-LA substrate for the osx-arm64 `Tensor/blas` lane.

## [03]-[BUILD_INTEGRATION]

[MSBUILD_IMPORT_LAW]:
- `buildTransitive/netstandard2.0/libtorch-cpu.props` / `.targets` flow transitively to the consuming project (the `buildTransitive` folder, not `build`), so a `<PackageReference>` on a referenced project propagates the native-copy behavior upward without the leaf project re-declaring it. The targets set `$(TorchSharpCpuPackage)` so the `CheckOneTorchSharpRuntime` guard can detect a co-pinned `libtorch-cuda`.
- The native sub-packages stage their `runtimes/<rid>/native/*` payload into the build output via the standard NuGet RID-asset copy; the consuming `Rasm.Compute` build resolves `osx-arm64` and copies the six `*.dylib` files beside the managed output, where `libLibTorchSharp.dylib` (from `TorchSharp`) P/Invokes them. No `<NativeReference>` or manual dylib copy is authored — the meta-package + sub-package targets own asset placement.
- A native sub-package is large (the LibTorch CPU runtime is hundreds of MB per RID); the central manifest pins ONLY `libtorch-cpu 2.10.0` (the meta-package), and the three RID sub-packages resolve transitively, so the manifest carries one row, not four. The osx-arm64 `.cache/nuget/packages/libtorch-cpu-osx-arm64/2.10.0/runtimes/osx-arm64/native/` folder is the verified payload location.

[RAIL_LAW]:
- Package: `libtorch-cpu` (native meta-package; assembly NONE)
- Owns: the LibTorch 2.10.0 native CPU ATen/c10 runtime floor, per-RID for `osx-arm64`/`linux-x64`/`win-x64`, plus the OpenMP threading runtime and the `CheckOneTorchSharpRuntime` CPU-vs-CUDA mutual-exclusion guard; the native substrate every `TorchSharp.torch.*` call P/Invokes through `libLibTorchSharp`
- Accept: a single CPU-runtime pin (`libtorch-cpu 2.10.0`) co-admitted with `TorchSharp 0.107.0` for the osx-arm64 `[CLASSICAL_ML_BLAS]` dual leg — native dense-LA for `Tensor/blas` and the iterative-estimator engine for `Stats/estimator`; thread-count tuning through the managed `torch.set_num_threads`/`set_num_interop_threads` knobs that drive the bundled `libomp`
- Reject: any direct managed reference to this package (it has no surface — code references `TorchSharp` only); a co-pin of `libtorch-cuda` (the `CheckOneTorchSharpRuntime` target hard-errors the build); a `torch.*` call on `linux-arm64`/`win-arm64`/`osx-x64` where no CPU payload ships (faults at native init); and any native-fact restatement in `api-torchsharp.md` (the managed catalog defers the RID/ABI/OpenMP/license facts to THIS owner)
