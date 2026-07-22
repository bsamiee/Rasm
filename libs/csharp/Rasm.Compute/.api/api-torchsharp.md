# [RASM_COMPUTE_API_TORCHSHARP]

`TorchSharp` binds PyTorch's LibTorch C++ ATen runtime through a lower-cased `torch.*` mirror over one native `Tensor` handle. Compute admits two `[CLASSICAL_ML_BLAS]` legs: `torch.linalg` supplies osx-arm64 dense factorization over Apple Accelerate BLAS/LAPACK, while `torch.optim` and `torch.autograd` minimize the lasso L1 and canonical-link GLM-deviance `EstimatorKind` losses. Genuine kernels own k-means, EM, NMF, DBSCAN, and linkage; `Tensor/blas` `LevenbergMarquardt` plus HyperJet own ARMA, Holt, and state-space fitting. Native runtime facts remain in `api-libtorch-cpu.md`; this catalog owns the managed surface.

DETERMINISTIC NATIVE-MEMORY LIFETIME governs the binding. Each `Tensor` owns a native ATen allocation through `IDisposable`; finalization owns nothing. `TorchSharp.DisposeScope`, entered through `torch.NewDisposeScope()`, disposes intermediates and promotes surviving results through `scope.MoveToOuter(t)`, `scope.Detach(t)`, or `tensor.MoveToOuter()`. Compute wraps each `torch` computation in `using var scope = torch.NewDisposeScope();`, then returns one promoted result through `Fin`; `torch.inference_mode(true)` brackets forward-only BLAS, and `torch.autograd` activates only inside estimator fitting.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TorchSharp`
- package: `TorchSharp`
- license: `MIT`
- assembly: `TorchSharp` (`lib/net8.0/TorchSharp.dll` binds for the `net10.0` consumer; `lib/netstandard2.0/TorchSharp.dll` is the fallback asset) — the surface below is decompile-verified against the consumer-bound `net8.0` assembly
- namespace: `TorchSharp` (the `torch` static class and `nn` live as `TorchSharp.torch` / `TorchSharp.torch.nn`; element types `BFloat16`/`Float16` and the public lifetime types `DisposeScope`/`DisposeScopeManager` sit at the `TorchSharp` root — the no-grad scope is the `torch.inference_mode()`/`no_grad()` static factory, NOT the `internal` `TorchSharp.InferenceMode` guard it hands back, which a consumer assembly cannot reference)
- asset: managed P/Invoke shim assembly + a per-RID native shim (`libLibTorchSharp.{dylib,so,dll}`) that bridges to the LibTorch native floor; on osx-arm64 the shim is `libLibTorchSharp.dylib`
- dependencies: the build-time `buildTransitive/net8.0/TorchSharp.{props,targets}` import pair (no managed package deps in the lib graph); the heavy native runtime is the separately-pinned `libtorch-cpu` meta-package
- native-floor: `libtorch-cpu 2.10.0` — `api-libtorch-cpu.md` owns the per-RID native ABI matrix, the `libomp` OpenMP threading floor, and the `CheckOneTorchSharpRuntime` CPU-vs-CUDA guard; a `torch.*` call P/Invokes `libLibTorchSharp` -> `libtorch_cpu`/`libtorch`/`libc10` at native init, faulting if the RID payload is absent
- consumer-bind note: multi-targets `net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`. Public signatures match across both TFMs, so `[API_TFM_RESOLUTION]` is benign
- rail: compute (`[CLASSICAL_ML_BLAS]` — `Tensor/blas` native substrate + `Stats/estimator` iterative engine)
- training/nn surface: `torch.nn.Module`, the layer/loss zoo, `torch.utils.data.DataLoader`/`Dataset`, `torch.jit` TorchScript load, `torch.utils.tensorboard` — PRESENT but REJECTED by `[RAIL_LAW]`. Compute uses `torch` for tensors, autograd, classical-estimator optimization, and dense LA; deep-learning training and ONNX inference never enter this owner

## [02]-[LIFETIME_AND_RUNTIME]

[PUBLIC_TYPE_SCOPE]: the deterministic native-memory lifetime owner (the FIRST law every rail composes)
- rail: compute

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]               | [CAPABILITY]                                              |
| :-----: | :--------------------------------------- | :-------------------------- | :-------------------------------------------------------- |
|  [01]   | `DisposeScope`                           | `IDisposable` scope         | scope owner — every `Tensor` born inside is freed on exit |
|  [02]   | `DisposeScopeManager`                    | static thread registry      | `Statistics` → `ThreadDisposeScopeStatistics`             |
|  [03]   | `ThreadDisposeScopeStatistics`           | stats carrier               | per-thread live/created/disposed counts                   |
|  [04]   | `torch.inference_mode(bool mode = true)` | `public static IDisposable` | no-grad, no-version-counter forward scope                 |
|  [05]   | `torch.Device`                           | device handle               | device handle; osx-arm64 floor is `DeviceType.CPU`        |
|  [06]   | `torch.ScalarType`                       | element enum (`sbyte`)      | the dtype enum (values below)                             |
|  [07]   | `DeviceType`                             | device-kind enum            | the device-kind enum (values below)                       |
|  [08]   | `BFloat16` / `Float16`                   | readonly structs            | CLR bf16/fp16 carriers; bridge to Tensors/ONNX            |
|  [09]   | `Scalar` / `ScalarExtensionMethods`      | value carrier               | boxed scalar for mixed tensor-scalar ops; `42.ToScalar()` |

- [01]-[DISPOSESCOPE]: `torch.NewDisposeScope()` enters it; `T MoveToOuter<T>(T)` and tuple overloads (up to 3) promote a result to the enclosing scope, `Detach<T>(T)`/`Attach(IDisposable)` re-parent, `MoveToOther(DisposeScope, …)` retargets, and `DisposeEverything()`/`DisposablesView`/`DisposablesCount` inspect — the single owner of intermediate-tensor reclamation, never leaking native ATen memory to the GC.
- [02]-[MANAGER]: `Statistics` → `ThreadDisposeScopeStatistics` (live/created/disposed counts per thread) is the leak-detection probe a `BlasReceipt`/`EstimatorModel` stamps to prove zero outstanding intermediates after a computation.
- [03]-[STATS]: per-thread `CreatedOutsideScopeCount`/`DisposedOutsideScopeCount`/`ThreadTotalLiveCount` — the receipt-grade memory-discipline evidence.
- [04]-[INFERENCEMODE]: `using var _ = torch.inference_mode(true);` brackets a no-grad, no-version-counter forward region; `torch.no_grad()`/`torch.enable_grad(bool enabled = true)` are grad-toggle twins, and `torch.is_inference_mode_enabled()`/`torch.is_grad_enabled()` read state. Returned `internal` `TorchSharp.InferenceMode : IDisposable` guards are never constructed by consumers; public `torch.*` factories are the sole entry.
- [05]-[DEVICE]: `new Device(DeviceType, int index)`/`Device("cpu")` (implicit `string`→`Device`), `type` (`DeviceType`); `torch.set_default_device(Device)` pins the floor.
- [06]-[SCALARTYPE]: `Byte`/`Int8`/`Int16`/`Int32`/`Int64`/`Float16`/`Float32`(=6)/`Float64`(=7)/`ComplexFloat32`/`ComplexFloat64`/`Bool`/`QInt8`/`QUInt8`/`QInt32`/`BFloat16` — the dtype axis of every factory and `to_type`; estimator math uses `Float64` for IRLS/MLE stability.
- [07]-[DEVICETYPE]: `CPU`=0, `CUDA`=1, `MKLDNN`, `OPENGL`, `OPENCL`, `IDEEP`, `HIP`, `FPGA`, `MSNPU`, `XLA`, … — the osx-arm64 rail is `CPU`-only (the native floor ships no GPU payload).
- [08]-[HALF]: bridge to/from `System.Numerics.Tensors` and the ONNX `BFloat16`/`Float16` carriers at the wire.
- [09]-[SCALAR]: the boxed scalar ATen accepts for mixed tensor-scalar ops — folds a CLR primitive into a `Tensor` operand without a 0-d tensor allocation.

[ENTRYPOINT_SCOPE]: runtime configuration and the ATen thread-pool knobs
- rail: compute
- note: ATen CPU parallelism rides the bundled OpenMP pool (`api-libtorch-cpu.md` `[OPENMP_THREADING_FLOOR]`), NOT the .NET thread pool; these are the in-process knobs that drive it.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE] | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :----------- | :------------------------------------------- |
|  [01]   | `torch.set_num_threads(int)` / `get_num_threads()`                     | static call  | ATen intra-op (OpenMP) thread count          |
|  [02]   | `torch.set_num_interop_threads(int)`                                   | static call  | inter-op parallelism count                   |
|  [03]   | `torch.manual_seed(long)` / `manual_seed_all(long)` / `initial_seed()` | static call  | seeds the global `Generator`                 |
|  [04]   | `torch.set_default_dtype(ScalarType)` / `get_default_dtype()`          | static call  | default factory dtype                        |
|  [05]   | `torch.set_default_device(Device)` / `set_default_device(string)`      | static call  | the default tensor device (osx-arm64: `cpu`) |
|  [06]   | `torch.cuda_is_available()` / `torch.InitializeDeviceType(DeviceType)` | static call  | device-availability probe + native init      |
|  [07]   | `torch.NewDisposeScope()`                                              | static call  | enters a `DisposeScope`                      |

- [01]-[THREADS]: the `Tensor/blas` throughput knob the lane sets once at boot.
- [03]-[SEED]: the `Sampling`/GMM-init/k-means++ determinism anchor; returns a `Generator` for a scoped RNG.
- [06]-[DEVICEPROBE]: `cuda_is_available()` is always `false` on the CPU floor; `InitializeDeviceType` is the explicit native device-type init.
- [07]-[OPENER]: the rail's standard `using var scope = torch.NewDisposeScope();` opener.

## [03]-[TENSOR_AND_INGRESS]

[PUBLIC_TYPE_SCOPE]: the `Tensor` value and the CLR<->tensor ingress/egress boundary
- rail: compute
- note: `Tensor` is `TorchSharp.torch.Tensor`, `IDisposable`. `Tensor/layout` and `Tensor/residency` fold the CLR-array boundary through these factories and reads; `from_array`/`ToTensor` ingress and `ReadCpu*`/`data<T>` egress form the `System.Numerics.Tensors` and gRPC residency seam.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :--------------------------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `torch.Tensor`                           | native handle     | the single tensor type; ops in [01] below       |
|  [02]   | `torch.from_array`                       | factory           | wrap a managed `Array` (any rank) as a `Tensor` |
|  [03]   | `torch.tensor`                           | factory           | typed-overload ingress; a shaped `Tensor`       |
|  [04]   | `ToTensor<T>`                            | extension ingress | fluent array→tensor mirror; 0-d scalar overload |
|  [05]   | `Tensor.ReadCpu*`                        | egress read       | element-wise CPU read by flat index             |
|  [06]   | `Tensor.data<T>()` / `Tensor.ToArray()`  | egress bulk       | bulk CPU buffer / managed-array egress          |
|  [07]   | `PinnedArray<T>` / `AllocatePinnedArray` | interop pin       | pins a managed array for zero-copy handoff      |

- [01]-[TENSOR]: `torch.Tensor` is `IDisposable`; `NumberOfElements`/`numel()`, `ElementSize`/`element_size()`, `requires_grad`/`requires_grad_(bool)`/`with_requires_grad`, `grad`, `backward(…)`, `cpu()`/`cuda(…)`, `to(Device/ScalarType/DeviceType/string, …)`/`to_type(ScalarType, copy, disposeAfter, non_blocking)`, `clone()`/`detach()`, `DetachFromDisposeScope()`, and the shape ops `reshape(params long[] shape)`/`t()` (2-D transpose) chain off `from_array(…)`.
- [02]-[FROMARRAY]: `from_array(Array, ScalarType?, Device?, requires_grad, names)` — the zero-fuss CLR-array ingress for a design matrix / response vector; the 2-arg `from_array(Array, Device?)` infers dtype.
- [03]-[TENSORFACTORY]: `tensor(T[]/T[,]/IList<T>/Memory<T>, dims?, ScalarType?, Device?, …)` — one overload family per element type incl. `BFloat16`/`bool`/`Memory<T>`, with an explicit shape.
- [04]-[TOTENSOR]: `TensorExtensionMethods.ToTensor<T>(this T[] raw, long[] dims, doCopy, requires_grad)`; `ToTensor<T>(this T scalar, Device?, requires_grad)` for a 0-d tensor.
- [05]-[READCPU]: `ReadCpuDouble/Single/Int32/Int64/Byte/Bool/Float16/BFloat16(long i)` — the scalar egress for a fitted coefficient / metric.
- [06]-[BULK]: egress of the result tensor back across the boundary into a `BlasReceipt` payload.
- [07]-[PIN]: the zero-copy native handoff on the residency hot path.

[ENTRYPOINT_SCOPE]: `torch` creation and core dense ops (the `Tensor/blas` GEMM surface)
- rail: compute
- note: the `zeros/ones/empty/eye/arange/linspace` factories share the tail `(…, ScalarType?, Device?, requires_grad, names)` and admit rank-1..4 + `long[]`/`ReadOnlySpan<long>` overloads.

| [INDEX] | [SURFACE]                                                           | [CALL_SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :----------- | :---------------------------------------------- |
|  [01]   | `torch.matmul(Tensor, Tensor)` / `torch.mm(Tensor, Tensor)`         | static op    | general / 2-D matrix product — native ATen GEMM |
|  [02]   | `torch.bmm/mv/dot(Tensor, Tensor)`                                  | static op    | batched matmul / matrix-vector / vector dot     |
|  [03]   | `torch.einsum(string equation, params Tensor[])`                    | static op    | Einstein-summation contraction                  |
|  [04]   | `torch.zeros/ones/empty/eye/arange/linspace(…)`                     | factory      | shaped allocation families                      |
|  [05]   | `torch.randn/rand/randint(…, Generator?, …)`                        | factory      | RNG-backed allocation (seeded by `manual_seed`) |
|  [06]   | `torch.cat(IList<Tensor>, dim)` / `torch.stack(IList<Tensor>, dim)` | static op    | concatenation / new-axis stacking               |

- [01]-[GEMM]: the native ATen GEMM the `Tensor/blas` dense lane dispatches to (Accelerate BLAS on osx-arm64).
- [03]-[EINSUM]: the general tensor-contraction the `quadrature`/tensor-network paths compose without hand-rolling index loops.
- [04]-[FACTORY]: the working-buffer factories inside a `DisposeScope`.
- [05]-[RNG]: the `Sampling` lane's tensor source and the estimator init (GMM means, NMF factors).
- [06]-[ASSEMBLE]: design-matrix assembly from feature columns.

## [04]-[LINEAR_ALGEBRA]

[ENTRYPOINT_SCOPE]: `torch.linalg` — the native ATen factorization suite (the dense-LA substrate `Tensor/blas#FACTOR` dispatches to)
- rail: compute
- note: tuple returns such as `(Tensor U, Tensor S, Tensor Vh)` carry native handles the rail promotes or disposes. `_ex` variants return status through `info`; non-zero status maps to typed `FactorFault` without exception control flow.

| [INDEX] | [SURFACE]                                                    | [FACTOR_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|   [01]  | `cholesky(Tensor)`                                           | SPD factor      | Cholesky of an SPD matrix                        |
|   [02]  | `cholesky_ex(Tensor, check_errors) -> (L, info)`             | SPD factor      | + per-batch `info` status                        |
|   [03]  | `eig(Tensor) -> (vals, vecs)`                                | spectral        | general eigendecomposition                       |
|   [04]  | `eigh(Tensor, UPLO) -> (vals, vecs)`                         | spectral        | Hermitian eigendecomposition                     |
|   [05]  | `eigvals(Tensor)` / `eigvalsh(Tensor)`                       | spectral        | eigenvalues only                                 |
|   [06]  | `svd(Tensor, fullMatrices) -> (U, S, Vh)`                    | SVD             | full/thin SVD                                    |
|   [07]  | `svdvals(Tensor)`                                            | SVD             | singular values only                             |
|   [08]  | `qr(Tensor, QRMode) -> (Q, R)`                               | orthogonal      | QR (`Reduced`/`Complete`/`R`)                    |
|   [09]  | `lu(Tensor, pivot) -> (P, L, U)`                             | LU              | pivoted LU factor                                |
|   [10]  | `lu_factor(Tensor, pivot) -> (LU, Pivots?)`                  | LU              | LU factor + pivots                               |
|   [11]  | `lu_solve(LU, pivots, B, left, adjoint)`                     | LU              | LU solve                                         |
|   [12]  | `ldl_factor(Tensor, hermitian)`                              | LDL             | LDL factor                                       |
|   [13]  | `ldl_factor_ex(…) -> (LU, Pivots?, Info?)`                   | LDL             | LDL factor + `info`                              |
|   [14]  | `ldl_solve(LD, pivots, B, hermitian)`                        | LDL             | LDL solve                                        |
|   [15]  | `solve(A, B, left)`                                          | solve           | dense linear solve                               |
|   [16]  | `solve_ex(A, B, left, check_errors) -> (result, info)`       | solve           | dense solve + `info`                             |
|   [17]  | `solve_triangular(A, B, upper, left, unitriangular, out)`    | solve           | triangular solve                                 |
|   [18]  | `lstsq(A, B) -> (Solution, Residuals, Rank, SingularValues)` | least-squares   | OLS/GLM least-squares                            |
|   [19]  | `lstsq(A, B, rcond)`                                         | least-squares   | with rcond cutoff                                |
|   [20]  | `lstsq` CPU driver (`gelsy`)                                 | least-squares   | `Rank` valid; `SingularValues`/`Residuals` empty |
|   [21]  | `pinv(Tensor, atol?, rtol?, hermitian)`                      | inverse         | Moore-Penrose pseudoinverse                      |
|   [22]  | `inv(Tensor)`                                                | inverse         | inverse                                          |
|   [23]  | `inv_ex(Tensor, check_errors) -> (L, info)`                  | inverse         | inverse + `info`                                 |
|   [24]  | `det(Tensor)`                                                | determinant     | determinant                                      |
|   [25]  | `slogdet(Tensor) -> (sign, logabsdet)`                       | determinant     | stable sign+log-abs-det                          |
|   [26]  | `matrix_rank(Tensor, atol?, rtol?, hermitian)`               | numeric         | numerical rank                                   |
|   [27]  | `cond(Tensor, p)`                                            | numeric         | condition number                                 |
|   [28]  | `multi_dot(IList<Tensor>)`                                   | numeric         | optimal multi-product chaining                   |
|   [29]  | `matrix_exp(Tensor)`                                         | numeric         | matrix exponential                               |
|   [30]  | `cross(input, other, dim)`                                   | vector/util     | cross product                                    |
|   [31]  | `vecdot(x, y, dim, out)`                                     | vector/util     | batched vector dot                               |
|   [32]  | `householder_product(A, tau)`                                | vector/util     | Householder reconstruction                       |
|   [33]  | `vander(input, N)`                                           | vector/util     | Vandermonde                                      |
|   [34]  | `tensorinv(input, ind)`                                      | vector/util     | tensor inverse                                   |

- [SPD]: kernel-SVM and GP covariance.
- [SPECTRAL]: PCA, spectral clustering, and GMM covariance.
- [SVD]: NMF initialization, low-rank reduction, and `matrix_rank`.
- [ORTHOGONAL]: orthogonalization and least-squares conditioning.
- [LU]: general dense linear systems.
- [SOLVE]: IRLS normal equations and ridge solves.
- [LEAST_SQUARES]: OLS/GLM rank, residual, and singular-value receipts; `lstsq` [18]/[19] on the osx-arm64 CPU floor runs the driverless `gelsy` path [20] — `Rank` populates while `SingularValues`/`Residuals` return empty tensors, so the fit gates on `Rank` and applies the sigma floor only when `SingularValues` is present.
- [INVERSE]: damped pseudoinverse and Jacobian routes.
- [DETERMINANT]: Gaussian log-likelihood and MLE terms.

[ENTRYPOINT_SCOPE]: `torch.fft` and `torch.special` (the `Stats/signal` spectral arm and special-function library)
- rail: compute
- note: the `fft.*` transforms take `(input, n, dim, FFTNormType)` with `FFTNormType.Backward`/`Forward`/`Ortho`; `special.softmax` takes `(input, dim, dtype?)`.

| [INDEX] | [SURFACE]                                                          | [FAMILY]   | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------- | :--------- | :---------------------------------------- |
|  [01]   | `torch.fft.fft_/ifft/rfft/irfft/hfft/ihfft`                        | FFT        | 1-D fwd/inv/real/Hermitian FFTs           |
|  [02]   | `torch.fft.fftfreq/rfftfreq(n, d, dtype?, device?, requires_grad)` | FFT freq   | sample-frequency axis                     |
|  [03]   | `torch.special.erf/erfc/erfcx/erfinv/expit/entr(Tensor)`           | special fn | error fns, logistic sigmoid, entropy term |
|  [04]   | `torch.special.softmax` / `gammaln`/`digamma`/`i0`/`ndtr`          | special fn | stable softmax + gamma/Bessel/normal-CDF  |

- [01]-[FFT]: native ATen FFT family for torch-interior spectral terms; the `Stats/signal` spectral axis rides the admitted MathNet `Fourier` owner, never this leg.
- [03]-[SPECIAL]: GLM link functions and information-theoretic estimator terms.
- [04]-[SPECIAL2]: the gamma/Bessel/normal-CDF family the distribution + GLM math composes.

## [05]-[OPTIMIZATION_AND_AUTOGRAD]

[ENTRYPOINT_SCOPE]: `torch.autograd` — reverse-mode differentiation (the engine the estimator fit loop runs on)
- rail: compute

| [INDEX] | [SURFACE]                                                             | [CALL_SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `Tensor.backward(grad_tensors?, retain_graph, create_graph, inputs?)` | reverse-AD    | accumulate gradients into `.grad`         |
|  [02]   | `torch.autograd.grad(outputs, inputs, …)`                             | reverse-AD    | functional gradient (no `.grad` mutation) |
|  [03]   | `torch.autograd.functional.jacobian(Func<Tensor,Tensor>, inputs)`     | jacobian      | autodiff Jacobian                         |
|  [04]   | `torch.autograd.Function<T>`                                          | custom op     | custom forward/backward op                |
|  [05]   | `torch.autograd.detect_anomaly` / `set_detect_anomaly`                | `AnomalyMode` | NaN/inf-trapping debug scope              |

- [01]-[BACKWARD]: accumulates into `.grad` of the leaf parameters — the per-iteration gradient step of an iterative estimator.
- [02]-[GRAD]: `torch.autograd.grad(outputs, inputs, grad_outputs?, retain_graph, create_graph, allow_unused)` — Hessian-vector / second-order terms when `create_graph=true`.
- [03]-[JACOBIAN]: scalar/vector/multi-input overloads — the analytic Jacobian feeding Gauss-Newton / IRLS without finite differences.
- [04]-[CUSTOMOP]: `SingleTensorFunction<T>`/`MultiTensorFunction<T>` variants — `forward(ctx, …)`/`backward(ctx, grad)` with `AutogradContext.save_for_backward`, a bespoke estimator loss with a hand-written gradient.
- [05]-[ANOMALY]: the `AnomalyMode : IDisposable` scope around an unstable fit surfaces the offending op for a typed `EstimatorFault`; the public ctor is `new AnomalyMode(bool enabled, bool check_nan = true)` at the `TorchSharp` root, restoring the prior mode on dispose.

[ENTRYPOINT_SCOPE]: `torch.optim` — the optimizer family (the `EstimatorKind` solver rows)
- rail: compute
- note: each factory accepts `IEnumerable<Parameter>`, `IEnumerable<(string name, Parameter)>`, or `IEnumerable<ParamGroup>`. Optimizers own parameter tensors; fitting calls `opt.zero_grad(); loss.backward(); opt.step();`, while `LBFGS.step` takes a line-search closure.
- prefix: rows [01]-[04] and the `lr_scheduler` row are `torch.optim.*`; [07] is `torch.nn.utils.*`.

| [INDEX] | [SURFACE]                                                          | [OPTIMIZER_FAMILY]   | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------------------- | :-------------------------------------- |
|  [01]   | `LBFGS(params, lr, max_iter, …)`                                   | quasi-Newton         | limited-memory BFGS + line search       |
|  [02]   | `SGD(params, lr, momentum, …)`                                     | first-order          | SGD with momentum/Nesterov/weight-decay |
|  [03]   | `Adam/AdamW(params, lr, …)`                                        | adaptive             | Adam / decoupled-weight-decay AdamW     |
|  [04]   | `RMSProp/Adagrad/Adadelta/Adamax/NAdam/RAdam/ASGD/Rprop`           | adaptive/specialized | the full adaptive+specialized roster    |
|  [05]   | `Optimizer.step()` / `zero_grad()` / `add_param_group(ParamGroup)` | optimizer call       | universal step/clear-grad/group-add     |
|  [06]   | `lr_scheduler.{…}(optimizer, …)`                                   | LR schedule          | learning-rate schedule family           |
|  [07]   | `nn.utils.clip_grad_norm_` / `clip_grad_value_`                    | grad clip            | gradient-norm / value clipping          |
|  [08]   | `new Parameter(Tensor data, bool requires_grad = true)`            | parameter carrier    | the trainable-parameter carrier         |

- [01]-[LBFGS]: `LBFGS(params, lr, max_iter, max_eval?, tolerance_grad, tolerance_change, history_size)` — the second-order driver for the canonical-link GLM deviance losses; `step(Func<Tensor>)` re-evaluates the loss closure per line-search probe.
- [02]-[SGD]: `SGD(params, learningRate, momentum, dampening, weight_decay, nesterov, maximize)` — the large-batch first-order driver.
- [03]-[ADAM]: `Adam/AdamW(params, lr, beta1, beta2, eps, weight_decay, amsgrad, maximize)` — the robust default for non-smooth convex objectives (the lasso L1 row).
- [04]-[ROSTER]: `Adagrad`/`Adadelta` (sparse-feature), `Rprop` (full-batch sign), `ASGD` (averaged), `NAdam`/`RAdam` (rectified/Nesterov-Adam).
- [05]-[OPTIMIZER]: the universal step/clear-grad/group-add surface every optimizer inherits.
- [06]-[SCHEDULE]: `{StepLR, MultiStepLR, ExponentialLR, CosineAnnealingLR, OneCycleLR, CyclicLR, LinearLR, ConstantLR, PolynomialLR, LambdaLR, MultiplicativeLR, SequentialLR, ChainedLR, ReduceLROnPlateau}` — `ReduceLROnPlateau` for convergence-stalled fits, `OneCycleLR`/`CosineAnnealingLR` for scheduled annealing; each is an `LRScheduler` stepped per epoch.
- [07]-[CLIP]: gradient-norm / value clipping for stability of an ill-conditioned IRLS/SVM step before `opt.step()`.
- [08]-[PARAMETER]: `TorchSharp.Modules.Parameter : torch.Tensor` — the trainable-parameter the `torch.optim` factories take as `IEnumerable<Parameter>`; the ctor moves the source tensor's native handle (invalidating it) and re-parents it to the parameter's `DisposeScope`, so build it from a fresh `torch.zeros(…)`/clone (the `Stats/estimator` `new Parameter(torch.zeros(…), requires_grad: true)` coefficient vector), never a tensor reused after the wrap.

## [06]-[INTEGRATION]

[STACK_LAW]:
- `FactorKind` dispatch routes osx-arm64 dense rows to `torch.linalg.*`, managed cold starts to `MathNet.Numerics`, and sparse factors to `CSparse`.
- `_ex` `info` tensors map non-zero numerical status to typed `FactorFault`; native exceptions never control solver flow.
- `Stats/estimator` minimizes lasso L1 under `Adam` and canonical-link GLM deviances under `LBFGS`; genuine kernels own k-means, EM, NMF, DBSCAN, and linkage, while `LevenbergMarquardt` plus HyperJet own ARMA, Holt, and state-space fits.
- Surviving coefficient tensors cross `ReadCpu*`/`ToArray()` into typed `EstimatorModel` carriers with route, iteration, final-loss, and `DisposeScopeManager.Statistics` evidence.
- Tensor ingress/egress crosses to `System.Numerics.Tensors` (`libs/csharp/.api/api-tensors.md`) and the gRPC residency payload via `from_array`/`tensor` (in) and `data<T>`/`GetTensorDataAsSpan` mirrors (out); a `Tensor` never escapes the Compute boundary — only its CLR-array projection rides the wire. `BFloat16`/`Float16` carriers bridge directly to the ONNX (`api-onnxruntime.md`) `BFloat16`/`Float16` element types when an estimator feeds a downstream inference graph.
- Boot sets `torch.set_num_threads`, `torch.manual_seed`, and `set_default_dtype(Float64)` once for ATen concurrency, deterministic initialization, and estimator precision.
- Forward-only BLAS runs inside `using var _ = torch.inference_mode(true);`; estimator fitting owns the only grad-enabled region.

[RAIL_LAW]:
- Package: `TorchSharp` (assembly `TorchSharp`; native floor `libtorch-cpu`)
- Owns: the managed `torch.*` mirror — `Tensor` + `DisposeScope` deterministic native-memory lifetime, `torch.linalg` native dense factorization (the osx-arm64 `Tensor/blas` substrate), `torch.optim` + `torch.autograd` iterative optimization (the `Stats/estimator` torch-loss engine), `torch.fft`/`torch.special` (torch-interior spectral + GLM math), and the ATen thread/seed/dtype runtime knobs
- Accept: the `[CLASSICAL_ML_BLAS]` dual leg — native dense LA dispatched from `Tensor/blas#FACTOR` with `_ex` status-tensor fault mapping, and the smooth-convex `EstimatorKind` rows (lasso, the canonical-link GLM deviances) folded to a `Tensor` loss minimized under a `DisposeScope` and egressed to a typed `EstimatorModel`; every intermediate reclaimed by scope exit and proven by `DisposeScopeManager.Statistics`
- Reject: the `torch.nn.Module` deep-learning training stack, `torch.utils.data` pipelines, `torch.jit` TorchScript, and `torch.utils.tensorboard` (present but never entering a Compute owner — ONNX owns inference, MathNet/CSparse own the managed/sparse LA terminals); a `Tensor` escaping the Compute boundary onto the wire (only the CLR-array projection crosses); any reliance on GC finalization for native-tensor reclamation (the `DisposeScope` rail is mandatory); and any restatement of the `libtorch-cpu` native RID/ABI/OpenMP/CUDA-guard facts (deferred to `api-libtorch-cpu.md`)
