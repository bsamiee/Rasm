# [RASM_COMPUTE_API_LIBTORCH_CPU]

`libtorch-cpu` is the LibTorch native CPU runtime floor: the ATen/c10 dense-compute engine every `TorchSharp.torch.*` call P/Invokes through `libLibTorchSharp` at native init. It ships no managed assembly and no public type — an MSBuild import pair and a per-RID native `dependencies` fan-out are the whole payload — so `api-torchsharp.md` owns the managed `torch.*` surface while this owner pins the per-RID, ABI, OpenMP, and CPU-vs-CUDA contract the osx-arm64 `Tensor/blas` and `Stats/families` rails build on.

## [01]-[NATIVE_ABI_FLOOR]

[PACKAGE_ASSET_SCOPE]: per-RID native CPU payload — the owning floor for the whole TorchSharp/LibTorch stack
- [OSX_ARM64]: `libtorch.dylib` `libtorch_cpu.dylib` `libc10.dylib` `libomp.dylib` `libtorch_global_deps.dylib` `libshm.dylib`
- [LINUX_X64]: `libtorch.so` `libtorch_cpu.so` `libc10.so` + OpenMP runtime
- [WIN_X64]: `torch.dll` `torch_cpu.dll` `c10.dll`
- [NO_CPU_PAYLOAD]: `linux-arm64` `win-arm64` `osx-x64` — the TorchSharp shim ships, this floor is absent

[OPENMP_THREADING_FLOOR]:
- osx-arm64 bundles `libomp.dylib`, and ATen CPU intra-op parallelism rides that OpenMP pool, never the .NET thread pool; `api-torchsharp.md` owns the `torch.set_num_threads`/`set_num_interop_threads` knobs that drive it, with `OMP_NUM_THREADS`/`MKL_NUM_THREADS` the only out-of-band override.
- osx-arm64 `libtorch_cpu.dylib` links that OpenMP runtime by the ABSOLUTE install name `/opt/homebrew/opt/libomp/lib/libomp.dylib` (`otool -L`), a Homebrew path a Nix-managed machine does not carry, so the bundled sibling `libomp.dylib` satisfies the dependency ONLY while the platform dylib search path includes the consolidated payload directory. With the payload staged and both packages referenced, TorchSharp's own two-step loader fails (`Failed to load native component .../cpu/libLibTorchSharp.dylib`) and `torch` type-init throws; a direct `libtorch-cpu-osx-arm64` `PackageReference` does NOT change that outcome; the same binary loads once `DYLD_LIBRARY_PATH` names the payload directory. Every composition root seats the payload directory on the loader path before the first `torch` touch, and the ATen residency gate is a real LOAD probe — a file-presence or RID check publishes a route that throws at first tensor.
- Explicitly loading BOTH `libtorch_cpu.dylib` and `libtorch.dylib` aborts the process with `Key already registered with the same priority: C10`; `libtorch.dylib` already carries the CPU library, so the rejected shortcut is preloading the pair rather than fixing the search path.
- ATen GEMM and factorization on Apple silicon dispatch through the macOS Accelerate BLAS/LAPACK backend compiled into `libtorch_cpu.dylib`, so osx-arm64 carries no separate MKL native asset — the reason TorchSharp is the native dense-LA substrate for the osx-arm64 `Tensor/blas` lane.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `torch.*` op resolves `libLibTorchSharp` → `libtorch_cpu`/`libtorch`/`libc10` at native init; a missing RID payload faults the first entry-point load, never silently degrading. A PRESENT payload faults the same way when the OpenMP install-name dependency is unresolvable, so the fault names a load failure and never proves the payload absent — `[OPENMP_THREADING_FLOOR]` carries the measured contract.
- `libtorch-cpu.targets` injects `CheckOneTorchSharpRuntime` (`AfterTargets=ResolveReferences`, `BeforeTargets=PrepareForBuild`), sets `$(TorchSharpCpuPackage)`, and emits a hard `<Error>` when `$(TorchSharpCudaPackage)` is also set — one project binds exactly one runtime.
- `buildTransitive` props/targets flow to any downstream project, so a referenced `<PackageReference>` propagates the native-copy behavior without the leaf re-declaring it; the RID sub-packages stage `runtimes/<rid>/native/*` beside the managed output through standard NuGet RID-asset copy, authoring no `<NativeReference>` or manual dylib copy.

[STACKING]:
- `api-torchsharp.md`(`.api/api-torchsharp.md`): its `libLibTorchSharp` shim is the P/Invoke bridge into this floor; the managed catalog declares its native floor as this package and defers the per-RID, ABI, OpenMP, and CUDA-guard facts here.
- central manifest: the C# manifest pins the `libtorch-cpu` meta-package alone, and the three RID sub-packages resolve transitively into one manifest row.

[LOCAL_ADMISSION]:
- Compute pins the CPU floor as the osx-arm64 dense-LA and estimator substrate; a source file references `TorchSharp`, never this meta-package.
