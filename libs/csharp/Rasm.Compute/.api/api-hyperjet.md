# [RASM_COMPUTE_API_HYPERJET]

`HyperJet` is the forward-mode hyper-dual automatic-differentiation substrate: first- and second-order derivatives (gradient + Hessian) of smooth scalar functions carried on the value itself, so a function written once in generic math evaluates its own exact `∂f`/`∂²f` with no finite-difference stencil. It ships three computational models — source-generated compile-time structs `DDScalar1`…`DDScalar15` (stack-allocated, zero-GC, integrated with the .NET generic-math `IFloatingPoint<T>` system for up to 15 variables), the ref-struct `DDScalarSpan` (zero-allocation, runtime-dynamic variable count over a `stackalloc` buffer), and the dynamic heap `DDScalar` (arbitrary runtime size/order) — all exporting the gradient vector and Hessian matrix through `GetGradient()`/`GetHessian()` for downstream MathNet consumption, with SIMD kernels dispatching `Vector512`/`Vector256` on x64 and `Vector128` (Neon/AdvSIMD) on Apple silicon. It is the scalar-AD leg of the ONE `Sensitivity`/`Chain` family — `Tensor/dispatch` owns the geometry/DEC-adjoint tape, `Symbolic/lowering` the symbolic tape, HyperJet the hyperdual leg for general smooth scalar objectives; a fourth parallel gradient mechanism (the finite-difference fall it deletes) is the rejected form.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HyperJet`
- package: `HyperJet`
- license: ISC (OSI permissive, MIT-equivalent)
- assembly: `HyperJet`
- namespace: `HyperJet` (`DDScalar`/`DDScalar1..15`/`DDScalarSpan`/`Vector3D<T>`/`Kernel`), `static HyperJet.HyperJetMath` (the transcendental function surface `Sin`/`Cos`/`Exp`/… over the DD types)
- asset: single managed AnyCPU IL assembly, net10.0-only TFM (zero-dependency; requires the .NET 10 generic-math + hardware-intrinsics surface, so no netstandard/net8 downlevel bind exists) — a net10 consumer is mandatory
- rail: autodiff

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hyper-dual scalar models
- namespace: `HyperJet`
- rail: autodiff
- shared: each scalar carries `Value`, `G(int i)` (first derivative), `H(int i, int j)` (second derivative) and exports `GetGradient()`/`GetHessian()` to MathNet; seed/read call shapes live in [03]

| [INDEX] | [SYMBOL]       | [CAPABILITY]                                                                                                       |
| :-----: | :------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DDScalarN`    | `DDScalar1`..`DDScalar15` compile-time source-gen structs, stack-allocated zero-GC, `IFloatingPoint<T>`-integrated |
|  [02]   | `DDScalarSpan` | `ref struct` over a caller `stackalloc double[]`: runtime-dynamic count, zero heap, zero-alloc result spans        |
|  [03]   | `DDScalar`     | dynamic heap hyper-dual — arbitrary runtime size AND order (1st/2nd), the generality terminal                      |
|  [04]   | `Kernel`       | `static int GetDataLength(int size, int order)` sizes the flat `DDScalarSpan` buffer                               |
|  [05]   | `Vector3D<T>`  | generic 3D vector `where T : IFloatingPoint<T>, IRootFunctions<T>` — `Dot`/`Cross`/`Length`/`Normalize`            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: seed variables and read derivatives
- namespace: `HyperJet`
- rail: autodiff

| [INDEX] | [SURFACE]                            | [CALL]                                                                                 |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `DDScalarN.Variables`                | `Variables(double v0, …)`; tuple-deconstructable up to `15`                            |
|  [02]   | `DDScalar.Variables`                 | `Variables(double[] values, int order = 2)`; `order: 1` omits Hessians                  |
|  [03]   | `DDScalar.Constant`                  | `Constant(double value, int size, int order = 2)`                                      |
|  [04]   | `DDScalarSpan.Variable`              | `Variable(Span<double> buffer, int index, double value, int size, int order)`           |
|  [05]   | `DDScalarSpan.ctor`                  | `new DDScalarSpan(Span<double> buffer, int size, int order)`                            |
|  [06]   | `f.Value` / `f.G(i)` / `f.H(i, j)`   | result value, gradient component, and Hessian component                                |
|  [07]   | `f.GetGradient()` / `f.GetHessian()` | `MathNet.Numerics.LinearAlgebra` vector and matrix exports                              |

- [01]-[SEED_COMPILE]: seed a compile-time model's active variables; each returned scalar carries its unit seed derivative.
- [02]-[SEED_DYNAMIC]: seed a dynamic active-variable vector.
- [03]-[SEED_CONSTANT]: seed a zero-derivative data read — the post-fit re-evaluation seeding an order-compatible constant array.
- [04]-[SEED_SPAN]: seed ONE active variable of a dynamic span model into its buffer.
- [05]-[SPAN_CTOR]: the result accumulator a span computation writes into, over a `stackalloc double[Kernel.GetDataLength(size, order)]`.
- [06]-[READS]: the function value, gradient component, and Hessian component.
- [07]-[EXPORT]: export the full gradient and Hessian as MathNet structures — the bridge into the numeric-spine consumers.

## [04]-[IMPLEMENTATION_LAW]

[MODEL_SELECTION]:
- KNOWN small variable count (≤15) on a hot inner loop → `DDScalarN` (compile-time, zero-alloc, the fastest and GC-free path); RUNTIME-dynamic count with no heap budget → `DDScalarSpan` over a `stackalloc` buffer sized by `Kernel.GetDataLength(size, order)`; arbitrary/large dynamic size → `DDScalar` (heap). Second-order needs are declared through `order: 2`; a gradient-only need sets `order: 1` and skips the Hessian storage.
- Generic-math integration is load-bearing: one differentiated routine over `T : IFloatingPoint<T>` or `Vector3D<T>` instantiates with `double` for plain evaluation and `DDScalarN`/`DDScalar` for sensitivity; a hand-forked derivative body is rejected.

[STACKING]:
- `Tensor/dispatch#SENSITIVITY` (the ONE `Sensitivity`/`Chain` family): HyperJet is the hyperdual LEG beside the geometry-adjoint tape (`Sensitivity.Operator` over the kernel `MeshAdjointSnapshot`/`OperatorRow`) and the `Symbolic/lowering` symbolic tape — all under the one `Chain` contract. A general smooth scalar objective routes here; a fourth parallel gradient mechanism is the deleted form.
- `Stats/estimator` (ARMA-MLE residual recursion, `HoltFilter`, `StateSpaceFilter`): the likelihood/smoothing recursions are authored ONCE over `DDScalar` (`Constants()` seeding the data reads, `Variables(theta, order: 1)` seeding the parameters), so each fit calls `LevenbergMarquardt.Minimize` with a machine-exact gradient — the finite-difference stencil is deleted, not fallen back to.
- `Solver/uncertainty` (FORM/SORM limit-state gradient): the exact-AD row joins BESIDE the finite-difference fallback — a caller-supplied black-box limit-state oracle keeps FD honestly, an owned smooth limit-state function takes the exact HyperJet gradient.
- `Tensor/blas#LEVENBERG_MARQUARDT`: HyperJet is documented as the canonical Jacobian provider for the owned LM solver — the residual vector's `GetGradient()` per row assembles the exact Jacobian the normal-equation step consumes.
- `MathNet.Numerics` (`api-mathnet-numerics.md`): `GetGradient()`/`GetHessian()` export directly to MathNet vectors/matrices, so the AD output threads into the numeric-spine factorization/solve owners with no manual marshalling.

[RAIL_LAW]:
- Package: `HyperJet` `0.2.0` (ISC, net10.0-only, zero-dep, pre-1.0 watch item)
- Owns: forward-mode hyper-dual first/second-order automatic differentiation of smooth scalar functions — the compile-time `DDScalar1..15`, the zero-alloc `DDScalarSpan`, the dynamic `DDScalar`, the generic `Vector3D<T>`, and the `GetGradient()`/`GetHessian()` MathNet export
- Accept: an exact gradient/Hessian of a smooth scalar objective or residual written in generic math — the scalar-AD leg of the `Sensitivity`/`Chain` family, the estimator MLE Jacobians, the FORM/SORM limit-state gradient, the LM Jacobian
- Reject: a fourth parallel gradient mechanism beside the geometry/symbolic/hyperdual legs; a finite-difference stencil where the smooth objective admits exact AD; reverse-mode/large-scale tape adjoints (the geometry-DEC adjoint tape owns those); a net8/netstandard downlevel bind (the package is net10-only)
