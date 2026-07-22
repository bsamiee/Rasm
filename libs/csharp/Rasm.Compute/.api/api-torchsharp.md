# [RASM_COMPUTE_API_TORCHSHARP]

`TorchSharp` mirrors PyTorch's LibTorch C++ ATen runtime as a lower-cased `torch.*` surface over one `Tensor` handle, owning the managed `[CLASSICAL_ML_BLAS]` leg: `torch.linalg` dense factorization over Apple Accelerate BLAS/LAPACK, and `torch.optim`/`torch.autograd` minimizing the lasso-L1 and canonical-link GLM-deviance `EstimatorKind` losses. Every `torch` op folds through a `DisposeScope`, freeing intermediate ATen allocations at scope exit, never on GC finalization. `torch.nn` training never enters a Compute owner; native RID/ABI facts defer to the `libtorch-cpu` floor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TorchSharp`
- package: `TorchSharp` (`MIT`, PyTorch/dotnet)
- assembly: `TorchSharp` (`lib/net8.0/TorchSharp.dll` binds the `net10.0` consumer, `lib/netstandard2.0` the fallback; signatures match across both TFMs)
- namespace: `TorchSharp` — the `torch` static class hosts `nn`/`linalg`/`optim`/`autograd`/`fft`/`special` as `TorchSharp.torch.*`; element types `BFloat16`/`Float16` and lifetime types `DisposeScope`/`DisposeScopeManager` sit at the root; the no-grad scope enters through the `torch.inference_mode()`/`no_grad()` factory, never the `internal` `InferenceMode` guard it returns
- asset: managed P/Invoke shim with a per-RID native shim (`libLibTorchSharp.{dylib,so,dll}`) bridging to the LibTorch native floor; osx-arm64 loads `libLibTorchSharp.dylib`
- depends: build-time `buildTransitive/net8.0/TorchSharp.{props,targets}` (no managed package deps); the native runtime floor is `libtorch-cpu`, whose per-RID ABI/OpenMP/CUDA-guard facts `api-libtorch-cpu.md` owns
- rail: compute — `Tensor/blas` native substrate with the `Stats/estimator` iterative engine

## [02]-[LIFETIME_AND_RUNTIME]

[PUBLIC_TYPE_SCOPE]: the deterministic native-memory lifetime owner — the first law every rail composes

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

- [01]-[DISPOSESCOPE]: `torch.NewDisposeScope()` enters it; `T MoveToOuter<T>(T)` and tuple overloads (up to 3) promote a result to the enclosing scope, `Detach<T>(T)`/`Attach(IDisposable)` re-parent, `MoveToOther(DisposeScope, …)` retargets, and `DisposeEverything()`/`DisposablesView`/`DisposablesCount` inspect — the single owner of intermediate-tensor reclamation.
- [02]-[MANAGER]: `Statistics` → `ThreadDisposeScopeStatistics` is the leak-detection probe a `BlasReceipt`/`EstimatorModel` stamps to prove zero outstanding intermediates.
- [03]-[STATS]: `CreatedOutsideScopeCount`/`DisposedOutsideScopeCount`/`ThreadTotalLiveCount` — the receipt-grade memory-discipline evidence.
- [04]-[INFERENCEMODE]: `using var _ = torch.inference_mode(true);` brackets a no-grad forward region; `torch.no_grad()`/`torch.enable_grad(bool enabled = true)` toggle grad, and `torch.is_inference_mode_enabled()`/`torch.is_grad_enabled()` read state. A consumer never constructs the returned `internal InferenceMode : IDisposable` guard; the public `torch.*` factory is the sole entry.
- [05]-[DEVICE]: `new Device(DeviceType, int index)`/`Device("cpu")` (implicit `string`→`Device`); `torch.set_default_device(Device)` pins the floor.
- [06]-[SCALARTYPE]: `Byte`/`Int8`/`Int16`/`Int32`/`Int64`/`Float16`/`Float32`(=6)/`Float64`(=7)/`ComplexFloat32`/`ComplexFloat64`/`Bool`/`QInt8`/`QUInt8`/`QInt32`/`BFloat16` — the dtype axis of every factory and `to_type`; estimator math uses `Float64` for IRLS/MLE stability.
- [07]-[DEVICETYPE]: `CPU`=0, `CUDA`=1, `MKLDNN`, `OPENGL`, `OPENCL`, `IDEEP`, `HIP`, `FPGA`, `MSNPU`, `XLA`, … — the osx-arm64 rail is `CPU`-only.
- [08]-[HALF]: bridge to/from `System.Numerics.Tensors` and the ONNX `BFloat16`/`Float16` carriers at the wire.
- [09]-[SCALAR]: `Scalar` folds a CLR primitive into a `Tensor` operand for mixed tensor-scalar ops without a 0-d tensor allocation.

[ENTRYPOINT_SCOPE]: runtime configuration and the ATen thread-pool knobs — ATen CPU parallelism rides the bundled OpenMP pool (`api-libtorch-cpu.md`), not the .NET thread pool, and these knobs drive it

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `torch.set_num_threads(int)` / `get_num_threads()`                     | static  | ATen intra-op (OpenMP) thread count          |
|  [02]   | `torch.set_num_interop_threads(int)`                                   | static  | inter-op parallelism count                   |
|  [03]   | `torch.manual_seed(long)` / `manual_seed_all(long)` / `initial_seed()` | static  | seeds the global `Generator`                 |
|  [04]   | `torch.set_default_dtype(ScalarType)` / `get_default_dtype()`          | static  | default factory dtype                        |
|  [05]   | `torch.set_default_device(Device)` / `set_default_device(string)`      | static  | the default tensor device (osx-arm64: `cpu`) |
|  [06]   | `torch.cuda_is_available()` / `torch.InitializeDeviceType(DeviceType)` | static  | device-availability probe + native init      |
|  [07]   | `torch.NewDisposeScope()`                                              | static  | enters a `DisposeScope`                      |

- [03]-[SEED]: `manual_seed` anchors `Sampling`/GMM-init/k-means++ determinism and returns a `Generator` for a scoped RNG.
- [06]-[DEVICEPROBE]: `cuda_is_available()` is always `false` on the CPU floor; `InitializeDeviceType` is the explicit native device-type init.
- [07]-[OPENER]: `NewDisposeScope()` opens the rail's standard `using var scope = torch.NewDisposeScope();`.

## [03]-[TENSOR_AND_INGRESS]

[PUBLIC_TYPE_SCOPE]: the `Tensor` value and the CLR↔tensor ingress/egress boundary — `from_array`/`ToTensor` ingress and `ReadCpu*`/`data<T>` egress form the `System.Numerics.Tensors` and gRPC residency seam

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :--------------------------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `torch.Tensor`                           | native handle     | the single tensor type; ops in [01] below       |
|  [02]   | `torch.from_array`                       | factory           | wrap a managed `Array` (any rank) as a `Tensor` |
|  [03]   | `torch.tensor`                           | factory           | typed-overload ingress; a shaped `Tensor`       |
|  [04]   | `ToTensor<T>`                            | extension ingress | fluent array→tensor mirror; 0-d scalar overload |
|  [05]   | `Tensor.ReadCpu*`                        | egress read       | element-wise CPU read by flat index             |
|  [06]   | `Tensor.data<T>()` / `Tensor.ToArray()`  | egress bulk       | bulk CPU buffer / managed-array egress          |
|  [07]   | `PinnedArray<T>` / `AllocatePinnedArray` | interop pin       | pins a managed array for zero-copy handoff      |

- [01]-[TENSOR]: `torch.Tensor` is `IDisposable`; `NumberOfElements`/`numel()`, `ElementSize`/`element_size()`, `requires_grad`/`requires_grad_(bool)`/`with_requires_grad`, `grad`, `backward(…)`, `cpu()`/`cuda(…)`, `to(Device/ScalarType/DeviceType/string, …)`/`to_type(ScalarType, copy, disposeAfter, non_blocking)`, `clone()`/`detach()`, `DetachFromDisposeScope()`, and the shape ops `reshape(params long[] shape)`/`t()` chain off `from_array(…)`.
- [02]-[FROMARRAY]: `from_array(Array, ScalarType?, Device?, requires_grad, names)` — the zero-fuss CLR-array ingress for a design matrix / response vector; the 2-arg `from_array(Array, Device?)` infers dtype.
- [03]-[TENSORFACTORY]: `tensor(T[]/T[,]/IList<T>/Memory<T>, dims?, ScalarType?, Device?, …)` — one overload family per element type including `BFloat16`/`bool`/`Memory<T>`, with an explicit shape.
- [04]-[TOTENSOR]: `TensorExtensionMethods.ToTensor<T>(this T[] raw, long[] dims, doCopy, requires_grad)`; `ToTensor<T>(this T scalar, Device?, requires_grad)` for a 0-d tensor.
- [05]-[READCPU]: `ReadCpuDouble/Single/Int32/Int64/Byte/Bool/Float16/BFloat16(long i)` — the scalar egress for a fitted coefficient / metric.
- [07]-[PIN]: `PinnedArray<T>` holds the zero-copy native handoff on the residency hot path.

[ENTRYPOINT_SCOPE]: `torch` creation and core dense ops — the `Tensor/blas` GEMM surface; the `zeros/ones/empty/eye/arange/linspace` factories share the tail `(…, ScalarType?, Device?, requires_grad, names)` and admit rank-1..4 with `long[]`/`ReadOnlySpan<long>` overloads

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :------ | :---------------------------------------------- |
|  [01]   | `torch.matmul(Tensor, Tensor)` / `torch.mm(Tensor, Tensor)`         | static  | general / 2-D matrix product — native ATen GEMM |
|  [02]   | `torch.bmm/mv/dot(Tensor, Tensor)`                                  | static  | batched matmul / matrix-vector / vector dot     |
|  [03]   | `torch.einsum(string equation, params Tensor[])`                    | static  | Einstein-summation contraction                  |
|  [04]   | `torch.zeros/ones/empty/eye/arange/linspace(…)`                     | factory | shaped allocation families                      |
|  [05]   | `torch.randn/rand/randint(…, Generator?, …)`                        | factory | RNG-backed allocation (seeded by `manual_seed`) |
|  [06]   | `torch.cat(IList<Tensor>, dim)` / `torch.stack(IList<Tensor>, dim)` | static  | concatenation / new-axis stacking               |

- [01]-[GEMM]: `matmul`/`mm` are the native ATen GEMM the `Tensor/blas` dense lane dispatches to (Accelerate BLAS on osx-arm64).
- [03]-[EINSUM]: `einsum` contracts the `quadrature`/tensor-network paths without hand-rolling index loops.
- [05]-[RNG]: `randn`/`rand`/`randint` source the `Sampling` lane and the estimator init (GMM means, NMF factors).

## [04]-[LINEAR_ALGEBRA]

[ENTRYPOINT_SCOPE]: `torch.linalg` — the native ATen factorization suite the dense-LA `Tensor/blas` substrate dispatches to; tuple returns such as `(Tensor U, Tensor S, Tensor Vh)` carry native handles the rail promotes or disposes, and `_ex` variants return status through `info` mapping non-zero to a typed `FactorFault`

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `cholesky(Tensor)`                                           | static  | SPD Cholesky factor             |
|  [02]   | `cholesky_ex(Tensor, check_errors) -> (L, info)`             | static  | SPD Cholesky + per-batch `info` |
|  [03]   | `eig(Tensor) -> (vals, vecs)`                                | static  | general eigendecomposition      |
|  [04]   | `eigh(Tensor, UPLO) -> (vals, vecs)`                         | static  | Hermitian eigendecomposition    |
|  [05]   | `eigvals(Tensor)` / `eigvalsh(Tensor)`                       | static  | eigenvalues only                |
|  [06]   | `svd(Tensor, fullMatrices) -> (U, S, Vh)`                    | static  | full/thin SVD                   |
|  [07]   | `svdvals(Tensor)`                                            | static  | singular values only            |
|  [08]   | `qr(Tensor, QRMode) -> (Q, R)`                               | static  | QR (`Reduced`/`Complete`/`R`)   |
|  [09]   | `lu(Tensor, pivot) -> (P, L, U)`                             | static  | pivoted LU factor               |
|  [10]   | `lu_factor(Tensor, pivot) -> (LU, Pivots?)`                  | static  | LU factor + pivots              |
|  [11]   | `lu_solve(LU, pivots, B, left, adjoint)`                     | static  | LU solve                        |
|  [12]   | `ldl_factor(Tensor, hermitian)`                              | static  | LDL factor                      |
|  [13]   | `ldl_factor_ex(…) -> (LU, Pivots?, Info?)`                   | static  | LDL factor + `info`             |
|  [14]   | `ldl_solve(LD, pivots, B, hermitian)`                        | static  | LDL solve                       |
|  [15]   | `solve(A, B, left)`                                          | static  | dense linear solve              |
|  [16]   | `solve_ex(A, B, left, check_errors) -> (result, info)`       | static  | dense solve + `info`            |
|  [17]   | `solve_triangular(A, B, upper, left, unitriangular, out)`    | static  | triangular solve                |
|  [18]   | `lstsq(A, B) -> (Solution, Residuals, Rank, SingularValues)` | static  | OLS/GLM least-squares           |
|  [19]   | `lstsq(A, B, rcond)`                                         | static  | least-squares + rcond cutoff    |
|  [20]   | `pinv(Tensor, atol?, rtol?, hermitian)`                      | static  | Moore-Penrose pseudoinverse     |
|  [21]   | `inv(Tensor)`                                                | static  | inverse                         |
|  [22]   | `inv_ex(Tensor, check_errors) -> (L, info)`                  | static  | inverse + `info`                |
|  [23]   | `det(Tensor)`                                                | static  | determinant                     |
|  [24]   | `slogdet(Tensor) -> (sign, logabsdet)`                       | static  | stable sign + log-abs-det       |
|  [25]   | `matrix_rank(Tensor, atol?, rtol?, hermitian)`               | static  | numerical rank                  |
|  [26]   | `cond(Tensor, p)`                                            | static  | condition number                |
|  [27]   | `multi_dot(IList<Tensor>)`                                   | static  | optimal multi-product chaining  |
|  [28]   | `matrix_exp(Tensor)`                                         | static  | matrix exponential              |
|  [29]   | `cross(input, other, dim)`                                   | static  | cross product                   |
|  [30]   | `vecdot(x, y, dim, out)`                                     | static  | batched vector dot              |
|  [31]   | `householder_product(A, tau)`                                | static  | Householder reconstruction      |
|  [32]   | `vander(input, N)`                                           | static  | Vandermonde                     |
|  [33]   | `tensorinv(input, ind)`                                      | static  | tensor inverse                  |

- `torch.linalg.lstsq`: its osx-arm64 CPU driver `gelsy` populates `Rank` while `SingularValues`/`Residuals` return empty tensors, so the fit gates on `Rank` and applies the sigma floor only when `SingularValues` is present.
- [SPD]: kernel-SVM and GP covariance.
- [SPECTRAL]: PCA, spectral clustering, GMM covariance.
- [SVD]: NMF initialization, low-rank reduction, `matrix_rank`.
- [ORTHOGONAL]: orthogonalization and least-squares conditioning.
- [LU]: general dense linear systems.
- [SOLVE]: IRLS normal equations and ridge solves.
- [INVERSE]: damped pseudoinverse and Jacobian routes.
- [DETERMINANT]: Gaussian log-likelihood and MLE terms.

[ENTRYPOINT_SCOPE]: `torch.fft` and `torch.special` — the `Stats/signal` spectral arm and special-function library; `fft.*` transforms take `(input, n, dim, FFTNormType)` with `FFTNormType.Backward`/`Forward`/`Ortho`

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `torch.fft.fft_/ifft/rfft/irfft/hfft/ihfft`                        | static  | 1-D fwd/inv/real/Hermitian FFTs           |
|  [02]   | `torch.fft.fftfreq/rfftfreq(n, d, dtype?, device?, requires_grad)` | static  | sample-frequency axis                     |
|  [03]   | `torch.special.erf/erfc/erfcx/erfinv/expit/entr(Tensor)`           | static  | error fns, logistic sigmoid, entropy term |
|  [04]   | `torch.special.softmax` / `gammaln`/`digamma`/`i0`/`ndtr`          | static  | stable softmax + gamma/Bessel/normal-CDF  |

- [01]-[FFT]: `fft.*` supplies the native ATen FFT family for torch-interior spectral terms; the `Stats/signal` spectral axis rides the admitted MathNet `Fourier` owner, never this leg.
- [03]-[SPECIAL]: GLM link functions and information-theoretic estimator terms — the gamma/Bessel/normal-CDF family the distribution and GLM math composes.

## [05]-[OPTIMIZATION_AND_AUTOGRAD]

[ENTRYPOINT_SCOPE]: `torch.autograd` — reverse-mode differentiation, the engine the estimator fit loop runs on

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Tensor.backward(grad_tensors?, retain_graph, create_graph, inputs?)` | instance | accumulate gradients into `.grad`         |
|  [02]   | `torch.autograd.grad(outputs, inputs, …)`                             | static   | functional gradient (no `.grad` mutation) |
|  [03]   | `torch.autograd.functional.jacobian(Func<Tensor,Tensor>, inputs)`     | static   | autodiff Jacobian                         |
|  [04]   | `torch.autograd.Function<T>`                                          | ctor     | custom forward/backward op                |
|  [05]   | `torch.autograd.detect_anomaly` / `set_detect_anomaly`                | static   | NaN/inf-trapping debug scope              |

- [01]-[BACKWARD]: accumulates into `.grad` of the leaf parameters — the per-iteration gradient step of an iterative estimator.
- [02]-[GRAD]: `grad(outputs, inputs, grad_outputs?, retain_graph, create_graph, allow_unused)` — Hessian-vector / second-order terms when `create_graph=true`.
- [03]-[JACOBIAN]: scalar/vector/multi-input overloads — the analytic Jacobian feeding Gauss-Newton / IRLS without finite differences.
- [04]-[CUSTOMOP]: `SingleTensorFunction<T>`/`MultiTensorFunction<T>` — `forward(ctx, …)`/`backward(ctx, grad)` with `AutogradContext.save_for_backward`, a bespoke estimator loss carrying a hand-written gradient.
- [05]-[ANOMALY]: `new AnomalyMode(bool enabled, bool check_nan = true)` at the `TorchSharp` root wraps an unstable fit and surfaces the offending op for a typed `EstimatorFault`, restoring the prior mode on dispose.

[ENTRYPOINT_SCOPE]: `torch.optim` — the optimizer family carrying the `EstimatorKind` solver rows; each factory accepts `IEnumerable<Parameter>`, `IEnumerable<(string name, Parameter)>`, or `IEnumerable<ParamGroup>`, and fitting calls `opt.zero_grad(); loss.backward(); opt.step();` while `LBFGS.step` takes a line-search closure. Rows [01]-[06] are `torch.optim.*`; [07] is `torch.nn.utils.*`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `LBFGS(params, lr, max_iter, …)`                                   | factory  | quasi-Newton limited-memory BFGS + line search    |
|  [02]   | `SGD(params, lr, momentum, …)`                                     | factory  | first-order SGD w/ momentum/Nesterov/weight-decay |
|  [03]   | `Adam/AdamW(params, lr, …)`                                        | factory  | adaptive Adam / decoupled-weight-decay AdamW      |
|  [04]   | `RMSProp/Adagrad/Adadelta/Adamax/NAdam/RAdam/ASGD/Rprop`           | factory  | the adaptive + specialized roster                 |
|  [05]   | `Optimizer.step()` / `zero_grad()` / `add_param_group(ParamGroup)` | instance | universal step/clear-grad/group-add               |
|  [06]   | `lr_scheduler.{…}(optimizer, …)`                                   | factory  | learning-rate schedule family                     |
|  [07]   | `nn.utils.clip_grad_norm_` / `clip_grad_value_`                    | static   | gradient-norm / value clipping                    |
|  [08]   | `new Parameter(Tensor data, bool requires_grad = true)`            | ctor     | the trainable-parameter carrier                   |

- [01]-[LBFGS]: `LBFGS(params, lr, max_iter, max_eval?, tolerance_grad, tolerance_change, history_size)` — the second-order driver for the canonical-link GLM deviance losses; `step(Func<Tensor>)` re-evaluates the loss closure per line-search probe.
- [03]-[ADAM]: `Adam/AdamW(params, lr, beta1, beta2, eps, weight_decay, amsgrad, maximize)` — the robust default for non-smooth convex objectives (the lasso L1 row).
- [04]-[ROSTER]: `Adagrad`/`Adadelta` (sparse-feature), `Rprop` (full-batch sign), `ASGD` (averaged), `NAdam`/`RAdam` (rectified/Nesterov-Adam).
- [06]-[SCHEDULE]: `{StepLR, MultiStepLR, ExponentialLR, CosineAnnealingLR, OneCycleLR, CyclicLR, LinearLR, ConstantLR, PolynomialLR, LambdaLR, MultiplicativeLR, SequentialLR, ChainedLR, ReduceLROnPlateau}`, each an `LRScheduler` stepped per epoch — `ReduceLROnPlateau` for stalled fits, `OneCycleLR`/`CosineAnnealingLR` for scheduled annealing.
- [07]-[CLIP]: gradient-norm / value clipping for an ill-conditioned IRLS/SVM step before `opt.step()`.
- [08]-[PARAMETER]: `TorchSharp.Modules.Parameter : torch.Tensor` — the `torch.optim` factories take `IEnumerable<Parameter>`; the ctor moves the source tensor's native handle (invalidating it) and re-parents it to the parameter's `DisposeScope`, so build it from a fresh `torch.zeros(…)`/clone, never a tensor reused after the wrap.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every intermediate tensor is reclaimed at `DisposeScope` exit and proven by `DisposeScopeManager.Statistics`; a `Tensor` never escapes the Compute boundary — only its CLR-array projection crosses onto the wire.
- `_ex` `info` status tensors map non-zero numerical status to a typed `FactorFault`; native exceptions never control solver flow.
- Boot pins `torch.set_num_threads`, `torch.manual_seed`, and `set_default_dtype(Float64)` once for ATen concurrency, deterministic init, and estimator precision; forward-only BLAS runs inside `torch.inference_mode(true)` and estimator fitting owns the only grad-enabled region.
- `Stats/estimator` minimizes lasso L1 under `Adam` and canonical-link GLM deviances under `LBFGS`; genuine kernels own k-means, EM, NMF, DBSCAN, and linkage, while `LevenbergMarquardt` with HyperJet owns ARMA, Holt, and state-space fits.

[STACKING]:
- `Tensor/blas` dispatches osx-arm64 dense factor rows to `torch.linalg.*` and folds a torch loss under a `DisposeScope`, egressing surviving coefficient tensors through `ReadCpu*`/`ToArray()` into a typed `EstimatorModel` carrying route, iteration, final-loss, and `Statistics` evidence.
- `System.Numerics.Tensors`(`libs/csharp/.api/api-tensors.md`): `from_array`/`tensor` ingress and `data<T>`/`GetTensorDataAsSpan` egress form the CLR↔tensor residency seam.
- `onnxruntime`(`api-onnxruntime.md`): `BFloat16`/`Float16` carriers bridge directly to the ONNX element types when an estimator feeds a downstream inference graph.
- `MathNet.Numerics`(`libs/csharp/.api/api-mathnet-numerics.md`) and `CSparse`(`libs/csharp/.api/api-csparse.md`): `FactorKind` routes managed cold starts to MathNet and sparse factors to CSparse.
- `HyperJet`(`api-hyperjet.md`): dual-number Jacobians feed the `LevenbergMarquardt` time-series fits.
- `libtorch-cpu`(`api-libtorch-cpu.md`): every `torch.*` call P/Invokes the native floor through `libLibTorchSharp`.

[LOCAL_ADMISSION]:
- A `torch` computation enters Compute only wrapped in `using var scope = torch.NewDisposeScope();`, returning one promoted result through `Fin`; estimator math runs in `Float64` for IRLS/MLE stability.

[RAIL_LAW]:
- Package: `TorchSharp` (assembly `TorchSharp`; native floor `libtorch-cpu`)
- Owns: the managed `torch.*` mirror — `Tensor` + `DisposeScope` deterministic native-memory lifetime, `torch.linalg` native dense factorization (the osx-arm64 `Tensor/blas` substrate), `torch.optim` + `torch.autograd` iterative optimization (the `Stats/estimator` torch-loss engine), `torch.fft`/`torch.special` torch-interior spectral and GLM math, and the ATen thread/seed/dtype runtime knobs
- Accept: the `[CLASSICAL_ML_BLAS]` dual leg — native dense LA dispatched from `Tensor/blas` with `_ex` status-tensor fault mapping, and the smooth-convex `EstimatorKind` rows folded to a `Tensor` loss minimized under a `DisposeScope` and egressed to a typed `EstimatorModel`, every intermediate reclaimed by scope exit
- Reject: the `torch.nn.Module` deep-learning training stack, `torch.utils.data` pipelines, `torch.jit` TorchScript, and `torch.utils.tensorboard` (ONNX owns inference, MathNet/CSparse own the managed/sparse LA terminals); a `Tensor` crossing the Compute boundary onto the wire; GC finalization for native-tensor reclamation; and any `libtorch-cpu` native RID/ABI/OpenMP/CUDA-guard restatement
