# [RASM_COMPUTE_API_LIBTORCH_CPU]

`libtorch-cpu` is the LibTorch native CPU runtime floor: the ATen/c10 dense-compute engine every `TorchSharp.torch.*` call P/Invokes through `libLibTorchSharp` at native init. It ships no managed assembly and no public type — an MSBuild import pair and a per-RID native `dependencies` fan-out are the whole payload — so `api-torchsharp.md` owns the managed `torch.*` surface while this owner pins the per-RID, ABI, OpenMP, and CPU-vs-CUDA contract the osx-arm64 `Tensor/blas` and `Stats/estimator` rails build on.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `libtorch-cpu`
- package: `libtorch-cpu` (MIT)
- assembly: NONE — `lib/netstandard2.0/_._/empty.txt` empty placeholder carries zero managed IL, zero public type, and no `lib/<tfm>` asset, so the `[API_TFM_RESOLUTION]` hazard cannot apply
- asset: `buildTransitive/netstandard2.0/libtorch-cpu.{props,targets}` import pair with a per-RID native `dependencies` group pulling the matching `libtorch-cpu-<rid>` sub-package
- depends: `libtorch-cpu-linux-x64`, `libtorch-cpu-osx-arm64`, `libtorch-cpu-win-x64` — the native dylib/so/dll carriers; only these three RIDs ship a CPU payload
- rail: compute — `[CLASSICAL_ML_BLAS]` native floor for `Tensor/blas` and `Stats/estimator`

## [02]-[NATIVE_ABI_FLOOR]

[PACKAGE_ASSET_SCOPE]: per-RID native CPU payload — the owning floor for the whole TorchSharp/LibTorch stack
- [OSX_ARM64]: `libtorch.dylib` `libtorch_cpu.dylib` `libc10.dylib` `libomp.dylib` `libtorch_global_deps.dylib` `libshm.dylib`
- [LINUX_X64]: `libtorch.so` `libtorch_cpu.so` `libc10.so` + OpenMP runtime
- [WIN_X64]: `torch.dll` `torch_cpu.dll` `c10.dll`
- [NO_CPU_PAYLOAD]: `linux-arm64` `win-arm64` `osx-x64` — the TorchSharp shim ships, this floor is absent

[OPENMP_THREADING_FLOOR]:
- osx-arm64 bundles `libomp.dylib`, and ATen CPU intra-op parallelism rides that OpenMP pool, never the .NET thread pool; `api-torchsharp.md` owns the `torch.set_num_threads`/`set_num_interop_threads` knobs that drive it, with `OMP_NUM_THREADS`/`MKL_NUM_THREADS` the only out-of-band override.
- ATen GEMM and factorization on Apple silicon dispatch through the macOS Accelerate BLAS/LAPACK backend compiled into `libtorch_cpu.dylib`, so osx-arm64 carries no separate MKL native asset — the reason TorchSharp is the native dense-LA substrate for the osx-arm64 `Tensor/blas` lane.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `torch.*` op resolves `libLibTorchSharp` → `libtorch_cpu`/`libtorch`/`libc10` at native init; a missing RID payload faults the first entry-point load, never silently degrading.
- `libtorch-cpu.targets` injects `CheckOneTorchSharpRuntime` (`AfterTargets=ResolveReferences`, `BeforeTargets=PrepareForBuild`), sets `$(TorchSharpCpuPackage)`, and emits a hard `<Error>` when `$(TorchSharpCudaPackage)` is also set — one project binds exactly one runtime.
- `buildTransitive` props/targets flow to any downstream project, so a referenced `<PackageReference>` propagates the native-copy behavior without the leaf re-declaring it; the RID sub-packages stage `runtimes/<rid>/native/*` beside the managed output through standard NuGet RID-asset copy, authoring no `<NativeReference>` or manual dylib copy.

[STACKING]:
- `api-torchsharp.md`(`.api/api-torchsharp.md`): its `libLibTorchSharp` shim is the P/Invoke bridge into this floor; the managed catalog declares its native floor as this package and defers the per-RID, ABI, OpenMP, and CUDA-guard facts here.
- central manifest: the C# manifest pins the `libtorch-cpu` meta-package alone, and the three RID sub-packages resolve transitively into one manifest row.

[LOCAL_ADMISSION]:
- Compute pins the CPU floor as the osx-arm64 dense-LA and estimator substrate; a source file references `TorchSharp`, never this meta-package.

[RAIL_LAW]:
- Package: `libtorch-cpu` (native meta-package; assembly NONE)
- Owns: the LibTorch native CPU ATen/c10 runtime floor per-RID for `osx-arm64`/`linux-x64`/`win-x64`, the bundled `libomp` OpenMP pool, and the `CheckOneTorchSharpRuntime` CPU-vs-CUDA guard — the substrate every `TorchSharp.torch.*` call P/Invokes through `libLibTorchSharp`
- Accept: a single CPU-runtime pin co-admitted with `TorchSharp` for the osx-arm64 `[CLASSICAL_ML_BLAS]` dual leg — native dense LA for `Tensor/blas` and the iterative-estimator engine for `Stats/estimator` — thread count tuned through the managed OpenMP knobs `api-torchsharp.md` owns
- Reject: any direct managed reference to this package; a `libtorch-cuda` co-pin; a `torch.*` call on `linux-arm64`/`win-arm64`/`osx-x64` where no CPU payload ships; the managed `torch.*` surface `api-torchsharp.md` owns
