# [RASM_COMPUTE_API_HYPERJET]

`HyperJet` is the forward-mode hyper-dual automatic-differentiation substrate: first- and second-order derivatives (gradient + Hessian) of smooth scalar functions carried on the value itself, so a function written once in generic math evaluates its own exact `∂f`/`∂²f` with no finite-difference stencil. It ships three computational models — source-generated compile-time structs `DDScalar1`…`DDScalar15` (stack-allocated, zero-GC, integrated with the .NET generic-math `IFloatingPoint<T>` system for up to 15 variables), the ref-struct `DDScalarSpan` (zero-allocation, runtime-dynamic variable count over a `stackalloc` buffer), and the dynamic heap `DDScalar` (arbitrary runtime size/order) — all exporting the gradient vector and Hessian matrix through `GetGradient()`/`GetHessian()` for downstream MathNet consumption, with SIMD kernels dispatching `Vector512`/`Vector256` on x64 and `Vector128` (Neon/AdvSIMD) on Apple silicon. It is the `[V10]` scalar-AD leg of the ONE `Sensitivity`/`Chain` family — `Tensor/dispatch` owns the geometry/DEC-adjoint tape, `Symbolic/lowering` the symbolic tape, HyperJet the hyperdual leg for general smooth scalar objectives; a fourth parallel gradient mechanism (the finite-difference fall it deletes) is the rejected form.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HyperJet`
- package: `HyperJet`
- version: `0.2.0` (pre-1.0 — the surface is a recorded watch item; the live owner is `oberbichler/HyperJet`, the nuspec repo pointer is dead)
- license: ISC (OSI permissive, MIT-equivalent)
- assembly: `HyperJet`
- namespace: `HyperJet` (`DDScalar`/`DDScalar1..15`/`DDScalarSpan`/`Vector3D<T>`/`Kernel`), `static HyperJet.HyperJetMath` (the transcendental function surface `Sin`/`Cos`/`Exp`/… over the DD types)
- asset: single managed AnyCPU IL assembly, **net10.0-only TFM** (zero-dependency; requires the .NET 10 generic-math + hardware-intrinsics surface, so no netstandard/net8 downlevel bind exists) — a net10 consumer is mandatory
- rail: autodiff

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hyper-dual scalar models
- namespace: `HyperJet`
- rail: autodiff

| [INDEX] | [SYMBOL]                | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :---------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `DDScalar1` … `DDScalar15` | autodiff | source-generated compile-time hyper-dual structs for a KNOWN variable count (1–15); 100% stack-allocated (zero heap, zero GC), integrated with `IFloatingPoint<T>` so a `DDScalarN<T>` composes standard operators/transcendentals; the generic `DDScalarN<double>` form mixes constants and active variables through implicit `double`→`DDScalarN` conversion. Carry `Value`, `G(int i)` (first derivative ∂f/∂xᵢ), `H(int i, int j)` (second derivative ∂²f/∂xᵢ∂xⱼ), and `GetGradient()`/`GetHessian()` exports |
|  [02]   | `DDScalarSpan`          | autodiff | a `ref struct` hyper-dual over a caller `stackalloc double[]` buffer — RUNTIME-dynamic variable count with ZERO heap allocation; carries `Value`/`G(int)`, and zero-alloc instance methods writing into a result span (`x.Sin(result)`, `Cos`, `Exp`, `Sinh`, …). The form when the variable count is runtime-determined but allocation must stay off the heap |
|  [03]   | `DDScalar`              | autodiff | the dynamic heap-allocated hyper-dual — arbitrary runtime size AND order (1st or 2nd), maximum flexibility for dynamic expression evaluation; `GetGradient()`/`GetHessian()` export the derivative structures. The generality terminal when neither the compile-time structs nor the span form fit |
|  [04]   | `Kernel`                | autodiff | the low-level math kernel; `static int GetDataLength(int size, int order)` sizes the flat buffer a `DDScalarSpan` needs for `size` variables at derivative `order` |
|  [05]   | `Vector3D<T>`           | autodiff | generic 3D vector `where T : IFloatingPoint<T>, IRootFunctions<T>` — addition, scaling, `Dot`, `Cross`, `Length`, `Normalize`, with implicit `double`/`float`→`T` conversion; written once, it runs with plain `double` for evaluation or `DDScalarN` for differentiated geometry (a torque `r × F` sensitivity in one type parameter) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: seed variables and read derivatives
- namespace: `HyperJet`
- rail: autodiff

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `DDScalarN.Variables`           | `static (DDScalarN, …) Variables(double v0, …)` (tuple-deconstructable up to 15)          | seed the active variables of a compile-time model; each returned scalar carries its unit seed derivative |
|  [02]   | `DDScalar.Variables`            | `static DDScalar[] Variables(double[] values)` (deconstructable)                          | seed a dynamic active-variable vector                        |
|  [03]   | `DDScalarSpan.Variable`         | `static DDScalarSpan Variable(Span<double> buffer, int index, double value, int size, int order)` | seed ONE active variable of a dynamic span model into its buffer |
|  [04]   | `new DDScalarSpan(buffer, size, order)` | ctor over a `stackalloc double[Kernel.GetDataLength(size, order)]`                | the result accumulator a span computation writes into        |
|  [05]   | `f.Value` / `f.G(i)` / `f.H(i, j)` | field/method reads on the result scalar                                               | the function value, gradient component, and Hessian component |
|  [06]   | `f.GetGradient()` / `f.GetHessian()` | `→ MathNet.Numerics.LinearAlgebra` vector / matrix                                   | export the full ∇f and ∇²f as MathNet structures — the bridge into the numeric-spine consumers |

## [04]-[IMPLEMENTATION_LAW]

[MODEL_SELECTION]:
- KNOWN small variable count (≤15) on a hot inner loop → `DDScalarN` (compile-time, zero-alloc, the fastest and GC-free path); RUNTIME-dynamic count with no heap budget → `DDScalarSpan` over a `stackalloc` buffer sized by `Kernel.GetDataLength(size, order)`; arbitrary/large dynamic size → `DDScalar` (heap). Second-order needs are declared through `order: 2`; a gradient-only need sets `order: 1` and skips the Hessian storage.
- the generic-math integration is load-bearing: a differentiated routine is written ONCE over `T : IFloatingPoint<T>` (or `Vector3D<T>`) and instantiated with `double` for the plain evaluation and `DDScalarN`/`DDScalar` for the sensitivity — never a hand-forked derivative body.

[STACKING]:
- `Tensor/dispatch#SENSITIVITY` (the ONE `Sensitivity`/`Chain` family): HyperJet is the hyperdual LEG beside the geometry-adjoint tape (`Sensitivity.Operator` over the kernel `MeshAdjointSnapshot`/`OperatorRow`) and the `Symbolic/lowering` symbolic tape — all under the one `Chain` contract. A general smooth scalar objective routes here; a fourth parallel gradient mechanism is the deleted form.
- `Stats/estimator` (the three `FiniteJacobian` sites — ARMA-MLE, Holt, the recurrence arm, "the autodiff-tape-absent fall"): the exact `GetGradient()`/`GetHessian()` replaces the finite-difference stencil, so the MLE Newton step reads a machine-exact Jacobian/Hessian rather than a differenced approximation.
- `Solver/uncertainty` (FORM/SORM limit-state gradient): the exact-AD row joins BESIDE the finite-difference fallback — a caller-supplied black-box limit-state oracle keeps FD honestly, an owned smooth limit-state function takes the exact HyperJet gradient.
- `Tensor/blas#LEVENBERG_MARQUARDT`: HyperJet is documented as the canonical Jacobian provider for the owned LM solver — the residual vector's `GetGradient()` per row assembles the exact Jacobian the normal-equation step consumes.
- `MathNet.Numerics` (`api-mathnet-numerics.md`): `GetGradient()`/`GetHessian()` export directly to MathNet vectors/matrices, so the AD output threads into the numeric-spine factorization/solve owners with no manual marshalling.

[RAIL_LAW]:
- Package: `HyperJet` `0.2.0` (ISC, net10.0-only, zero-dep, pre-1.0 watch item)
- Owns: forward-mode hyper-dual first/second-order automatic differentiation of smooth scalar functions — the compile-time `DDScalar1..15`, the zero-alloc `DDScalarSpan`, the dynamic `DDScalar`, the generic `Vector3D<T>`, and the `GetGradient()`/`GetHessian()` MathNet export
- Accept: an exact gradient/Hessian of a smooth scalar objective or residual written in generic math — the scalar-AD leg of the `Sensitivity`/`Chain` family, the estimator MLE Jacobians, the FORM/SORM limit-state gradient, the LM Jacobian
- Reject: a fourth parallel gradient mechanism beside the geometry/symbolic/hyperdual legs; a finite-difference stencil where the smooth objective admits exact AD; reverse-mode/large-scale tape adjoints (the geometry-DEC adjoint tape owns those); a net8/netstandard downlevel bind (the package is net10-only)
