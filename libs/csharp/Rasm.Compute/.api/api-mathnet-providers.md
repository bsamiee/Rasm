# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics.Providers.MKL` and `MathNet.Numerics.Providers.OpenBLAS` are the native adapters behind the branch numeric plane's provider selection: each carries a control class minting an `ILinearAlgebraProvider` off a native payload, a loader reporting availability without throwing, and its own diagnostic surface. Neither owns algebra, and neither ships an osx-arm64 asset — so the Compute numeric lane resolves the managed provider here and the adapters arm only where an x64 payload lands.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.MKL`
- package: `MathNet.Numerics.Providers.MKL` (MIT)
- assembly: `MathNet.Numerics.Providers.MKL`
- namespace: `MathNet.Numerics.Providers.MKL`, `.MKL.LinearAlgebra`, `.MKL.SparseSolver`, `.MKL.FourierTransform`, `.Common`
- asset: managed adapter; native binaries ship in the `MathNet.Numerics.MKL.Win-x64` and `.Linux-x64` payload packages, no osx-arm64 asset
- rail: numeric-provider

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS` (MIT)
- assembly: `MathNet.Numerics.Providers.OpenBLAS`
- namespace: `MathNet.Numerics.Providers.OpenBLAS`, `.OpenBLAS.LinearAlgebra`, `.Common`
- asset: managed adapter; native binaries ship in the x64 OpenBLAS payload packages, no osx-arm64 asset
- rail: numeric-provider

- Registers `MathNet.Numerics`(`libs/csharp/.api/api-mathnet-numerics.md`): `Control`, `LinearAlgebraControl`, `ILinearAlgebraProvider`, the dense factorization and sparse ingestion families, and the Krylov solvers all resolve there and are never re-tabled here.
- Registers `CSparse`(`libs/csharp/.api/api-csparse.md`): the direct sparse Cholesky, LU, and QR lane the Compute solver selects against the Krylov peer.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: adapter control classes and the payload-tuning vocabulary

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `MklProvider`                  | static class  | MKL payload load, memory pool, and diagnostic surface   |
|  [02]   | `MklLinearAlgebraControl`      | class         | mints and selects the MKL `ILinearAlgebraProvider`      |
|  [03]   | `MklSparseSolverControl`       | class         | mints the MKL `ISparseSolverProvider`                   |
|  [04]   | `MklFourierTransformControl`   | class         | mints the MKL `IFourierTransformProvider`               |
|  [05]   | `MklConsistency`               | enum          | run-to-run reproducibility floor across CPU generations |
|  [06]   | `MklPrecision`                 | enum          | internal working precision                              |
|  [07]   | `MklAccuracy`                  | enum          | vector-math accuracy tier                               |
|  [08]   | `OpenBlasProvider`             | static class  | OpenBLAS payload load and diagnostic surface            |
|  [09]   | `OpenBlasLinearAlgebraControl` | class         | mints and selects the OpenBLAS `ILinearAlgebraProvider` |

- `MklConsistency`: `Auto=2` `Compatible=3` `SSE2=4` `SSE4_2=8` `AVX=9` `AVX2=10` — `Auto` is same-CPU-only reproducibility at maximum speed and `Compatible` the SSE2 floor across Intel-compatible parts.
- `MklPrecision`: `Single=0x10` `Double=0x20`. `MklAccuracy`: `Low=1` `High=2`.
- Every control class implements `IProviderCreator<T>` over its provider interface, so `CreateProvider()` is the uniform mint the branch `LinearAlgebraControl.TryUse` admits.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: payload availability and load — every member static, `IsAvailable` probing without loading and `Load` returning the native revision

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `MklProvider.IsAvailable(string hintPath = null) -> bool`               | static  | probe the payload without binding it           |
|  [02]   | `MklProvider.Load(string hintPath = null) -> int`                       | static  | load at the default consistency triple         |
|  [03]   | `MklProvider.Load(hintPath, MklConsistency, MklPrecision, MklAccuracy)` | static  | load under an explicit tuning triple           |
|  [04]   | `MklProvider.Describe() -> string`                                      | static  | active native revision and tuning receipt line |
|  [05]   | `OpenBlasProvider.IsAvailable(string hintPath = null) -> bool`          | static  | probe the payload without binding it           |
|  [06]   | `OpenBlasProvider.Load(string hintPath = null) -> int`                  | static  | load the payload                               |
|  [07]   | `OpenBlasProvider.Describe() -> string`                                 | static  | active native revision receipt line            |

- Both `Load` overloads default `hintPath` to `null`, falling back to the branch `Control.NativeProviderPath` probe root and then the platform default paths.
- `Load`'s tuning triple defaults to `(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High)`.

[ENTRYPOINT_SCOPE]: provider mint and selection — the `Use*` form throws on a missing payload and its `Try*` twin returns the verdict

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `MklLinearAlgebraControl.CreateNativeMKL(consistency, precision, accuracy)` | static   | mint the provider without selecting it       |
|  [02]   | `MklLinearAlgebraControl.UseNativeMKL(consistency, precision, accuracy)`    | static   | mint and select, throwing on absence         |
|  [03]   | `MklLinearAlgebraControl.TryUseNativeMKL(consistency, precision, accuracy)` | static   | the same selection returning `bool`          |
|  [04]   | `OpenBlasLinearAlgebraControl.CreateNativeOpenBLAS()`                       | static   | mint the provider without selecting it       |
|  [05]   | `OpenBlasLinearAlgebraControl.UseNativeOpenBLAS()`                          | static   | mint and select, throwing on absence         |
|  [06]   | `OpenBlasLinearAlgebraControl.TryUseNativeOpenBLAS()`                       | static   | the same selection returning `bool`          |
|  [07]   | `IProviderCreator<T>.CreateProvider() -> T`                                 | instance | the uniform mint an instance control exposes |

- Every MKL selection argument carries the same tuning-triple default as `MklProvider.Load`, so a call naming none binds `Auto`/`Double`/`High`.

[ENTRYPOINT_SCOPE]: MKL native memory custody and telemetry — no OpenBLAS counterpart exists

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `MklProvider.FreeResources()`                         | static  | release the loaded payload whole               |
|  [02]   | `MklProvider.FreeBuffers()`                           | static  | drop every pooled buffer across threads        |
|  [03]   | `MklProvider.ThreadFreeBuffers()`                     | static  | drop the calling thread's pooled buffers       |
|  [04]   | `MklProvider.DisableMemoryPool()`                     | static  | run allocation-transparent, no pooling         |
|  [05]   | `MklProvider.MemoryStatistics(out int) -> long`       | static  | live allocated bytes with the buffer count     |
|  [06]   | `MklProvider.EnablePeakMemoryStatistics()`            | static  | begin peak tracking                            |
|  [07]   | `MklProvider.DisablePeakMemoryStatistics()`           | static  | end peak tracking                              |
|  [08]   | `MklProvider.PeakMemoryStatistics(bool reset = true)` | static  | peak bytes, resetting the watermark by default |
|  [09]   | `OpenBlasProvider.FreeResources()`                    | static  | release the loaded payload whole               |

- `PeakMemoryStatistics` resets the watermark unless the caller passes `false`, so a sampling loop reading it every interval measures per-interval peaks rather than a running maximum.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Selection is a two-step the branch owns end to end: the adapter mints a provider through its control class and the branch `LinearAlgebraControl` admits it, so an adapter never installs itself behind the branch's back and a caller-built provider enters through `LinearAlgebraControl.TryUse` on the same seam.
- Branch selection probes each adapter by assembly-qualified type name, so an absent adapter assembly degrades to a `false` verdict rather than a load fault and both packages stay optional at every consumer.
- `IsAvailable` answers off the probe path alone while `Load` binds and returns the native revision, so admission reads availability, records the revision on its receipt, and never treats a load return as a boolean.
- Native memory is MKL-only custody: the pool survives `FreeBuffers` and dies with `FreeResources`, and `DisableMemoryPool` trades throughput for allocation transparency under a leak hunt.
- Tuning is reproducibility policy, not performance tuning: `MklConsistency` fixes the instruction floor every run reproduces against, so a cross-machine bit-identical claim binds `Compatible` or a named ISA row and `Auto` forfeits it.

[STACKING]:
- `MathNet.Numerics`(`libs/csharp/.api/api-mathnet-numerics.md`): the whole numeric plane — provider control, dense factorization, sparse ingestion, Krylov solve, distributions, statistics, and transforms — resolves at the branch catalogue; these packages add the native mints alone.
- `CSparse`(`libs/csharp/.api/api-csparse.md`): a residual or stiffness operator assembled as `CompressedColumnStorage<double>` factors on the direct sparse `SparseCholesky`/`SparseLU`/`SparseQR` lane and solves in place, while the branch Krylov solvers under an `Iterator<T>` control cover the matrix-free peer; matrix density and factor reuse select among the three.
- Tensor/Stats/Solver lane: numeric composition selects the provider once through `LinearProvider.Select()`, folds dense and sparse solves onto one `Factorization` `ComputeReceipt` carrying the resolved provider name and its native revision, and claim-gates provider rank through `BenchmarkRow.Claim`.
- uncertainty lane: each `RandomVariable` case lowers onto one `IContinuousDistribution` (`Normal`/`LogNormal`/`ContinuousUniform`/`Weibull`/`Beta`); Monte-Carlo and LHS draw through `Sample`/`Samples` seeded from the owned `LowDiscrepancy` sequence, PCE fits its coefficients through the thin-QR least-squares route, response moments fold through `Statistics.Mean`/`Variance`/`Quantile`, and the reliability index β is `Normal.InvCDF(1 − pf)` over the limit-state CDF.
- inference lane: each `StatisticalTest` reads its p-value from the matching CDF — `StudentT.CDF` for `t`/`welch-t`, `FisherSnedecor.CDF` for `anova`, `ChiSquared.CDF` for `chi-square`, `Normal.CDF` for the `mann-whitney` large-sample tail; the GLM IRLS loop reads variance and deviance from the response family (`Poisson.Probability`/`ProbabilityLn` for count, `Gamma.Density`/`DensityLn` for gamma), and `naive-bayes` fits per-class `Normal`/`Poisson` moments through `Statistics`.
- signal lane: the real `Tensor<float>` marshals into `Complex[]` once through the dispatch-lane Complex kernels under one consistent `FourierOptions` scaling; MathNet ships no DWT/wavelet and no analog-prototype IIR design, so the `dwt` QMF cascade and the Butterworth, Chebyshev, and elliptic bilinear design ground in-fence at the signal-lane gate. `MklFourierTransformControl` mints an accelerated transform provider where an x64 payload lands, and the managed transform is the platform floor.

[LOCAL_ADMISSION]:
- Numeric composition selects the provider once through `LinearProvider.Select()`, which probes `IsAvailable` per adapter, admits the winner through the branch control, and records provider name and native revision on the solve receipt; a per-call-site `UseNativeMKL()` is the named defect.
- osx-arm64 resolves no adapter payload and rides the managed provider, so a benchmark claim asserting native rank on this platform fails its gate rather than degrading silently.
- Cross-machine reproducibility claims bind an explicit `MklConsistency` row; `Auto` is the throughput default and carries no such claim.

[RAIL_LAW]:
- Package: `MathNet.Numerics.Providers.MKL`, `MathNet.Numerics.Providers.OpenBLAS`
- Owns: native linear-algebra, sparse-solver, and Fourier provider mints, their payload probe and load, the MKL tuning triple, and MKL native memory custody and telemetry
- Accept: one composition-time probe-and-select through `LinearProvider.Select()`, an explicit tuning triple where reproducibility is claimed, and native memory reads on the MKL diagnostic surface
- Reject: a per-call-site provider switch, a re-tabling of the branch numeric plane, a native-rank benchmark claim on a platform carrying no payload, and treating a `Load` return code as a success boolean
