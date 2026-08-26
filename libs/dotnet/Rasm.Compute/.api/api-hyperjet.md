# [RASM_COMPUTE_API_HYPERJET]

`HyperJet` owns forward-mode hyper-dual automatic differentiation: a smooth scalar function written once in generic math carries its own exact first and second derivatives on the value, so one evaluation yields `f`, `∂f`, and `∂²f` with no finite-difference stencil. Three models span the size and allocation axis — compile-time `DDScalar1<T>`..`DDScalar15<T>` structs, the zero-alloc ref-struct `DDScalarSpan`, and the dynamic-heap `DDScalar` — each reading out a plain `T[]` gradient and `T[,]` Hessian for the branch `Sensitivity`/`Chain` autodiff pipeline.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hyper-dual scalar models; each carries `Value`, `G(i)` first derivative, and `H(i, j)` second derivative

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `DDScalar1<T>`..`DDScalar15<T>` | struct        | source-gen structs, `IFloatingPointIeee754<T>`-generic, `BufferN`-backed zero-GC |
|  [02]   | `DDScalarSpan`                  | ref-struct    | runtime-dynamic variable count over a caller `Span<double>`; zero heap           |
|  [03]   | `DDScalar`                      | struct        | dynamic-heap model, arbitrary runtime size and order — the generality terminal   |
|  [04]   | `Kernel`                        | class         | flat-buffer sizing: `GetDataLength`, `GetSizeFromDataLength`                     |
|  [05]   | `Vector3D<T>`                   | struct        | generic 3D vector; `Dot`/`Cross`/`Length`/`Normalize`, `+`/`-` operators         |
|  [06]   | `HyperJetMath`                  | class         | static transcendental surface over `DDScalar` and `DDScalar2<double>`            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: seed active variables, read derivatives, export the gradient and Hessian

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `DDScalarN.Variables(T, …) -> (DDScalarN<T> …)`                    | factory  | seed 1..15 compile-time actives; tuple-deconstructable   |
|  [02]   | `DDScalar.Variables(double[], int order) -> DDScalar[]`            | factory  | seed a dynamic active vector; `order: 1` drops Hessians  |
|  [03]   | `DDScalar.Constant(double, int, int order) -> DDScalar`            | factory  | seed a zero-derivative data read (post-fit re-eval)      |
|  [04]   | `DDScalarSpan.Variable(Span<double>, int, double, int, int order)` | factory  | seed one active variable into the span buffer            |
|  [05]   | `new DDScalarSpan(Span<double>, int, int order)`                   | ctor     | span-model result accumulator over a `stackalloc` buffer |
|  [06]   | `f.Value` / `f.G(int)` / `f.H(int, int)`                           | property | value, gradient component, Hessian component             |
|  [07]   | `f.GetGradient() -> T[]` / `f.GetHessian() -> T[,]`                | instance | full gradient and Hessian as plain .NET arrays           |

- `DDScalarSpan` sizes its buffer with `Kernel.GetDataLength(size, order)`.
- `f.GetGradient(Span<double>)` writes the gradient in place, zero-alloc.

## [03]-[OPERATOR_SURFACE]

[OPERATOR_SCOPE]: `DDScalar`'s own arithmetic — the dynamic model publishes a complete `in`-taking operator set, so a wrapper struct forwards rather than reconstructing the derivative algebra

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `-(in DDScalar)`                                                          | operator | unary negate                 |
|  [02]   | `+`/`-`/`*`/`/` `(in DDScalar, in DDScalar)`                              | operator | scalar-scalar arithmetic     |
|  [03]   | `+`/`-`/`*`/`/` `(in DDScalar, double)` and `(double, in DDScalar)`       | operator | `double / DDScalar` included |
|  [04]   | `==`/`!=`/`<`/`>`/`<=`/`>=` `(DDScalar, DDScalar)`                        | operator | value-plane ordering         |
|  [05]   | `==`/`!=`/`<`/`>`/`<=`/`>=` `(DDScalar, double)` and `(double, DDScalar)` | operator | mixed-operand ordering       |

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `Sin` `Cos` `Tan` `Asin` `Acos` `Atan` `Atan2(y, x)` | static  | trigonometric, both models                                     |
|  [02]   | `Exp` `Log` `Log10` `Pow(a, double b)`               | static  | exponential family, both models                                |
|  [03]   | `Sqrt(in DDScalar)`                                  | static  | exact-derivative root; the hand Newton-halving loop is deleted |
|  [04]   | `Hypot(a, b)` `Abs`                                  | static  | magnitude family, both models                                  |
|  [05]   | `Sinh` `Cosh` `Tanh` `Cbrt` `Log2`                   | static  | `DDScalar` ONLY — absent on `DDScalar2<double>`                |

- Every member takes its operand `in` and returns by value; a fixed-iteration hand root over these operators is a `FORGED_ZERO`-shaped claim, because it reports convergence without testing a residual while an exact member stands one call away.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HyperJet` is the hyperdual leg of the one `Sensitivity`/`Chain` family, beside the `Tensor/dispatch` geometry-adjoint tape and the `Symbolic/lowering` symbolic tape; a general smooth scalar objective folds here.
- One routine written over `T : IFloatingPoint<T>` or `Vector3D<T>` instantiates with `double` for plain evaluation and `DDScalarN<double>`/`DDScalar` for sensitivity, so one body yields value and exact derivatives.

[STACKING]:
- `MathNet.Numerics`(`.api/api-mathnet-numerics.md`): folder marshalling lifts HyperJet's `double[]` gradient and `double[,]` Hessian into MathNet `Vector`/`Matrix` for the numeric-spine factorization and solve owners.
- `Stats/families`: ARMA-MLE, `HoltFilter`, and `StateSpaceFilter` recursions author once over `DDScalar` (`Constant` data reads, `Variables(theta, order: 1)` parameters), so each `LevenbergMarquardt.Minimize` fit takes a machine-exact gradient.
- `Solver/uncertainty`: FORM/SORM limit-state gradient joins beside the finite-difference row — an owned smooth limit-state takes exact HyperJet AD, a caller-supplied black-box oracle stays on finite differences.
- `Tensor/blas#DENSE_ALGEBRA`: HyperJet is the canonical Jacobian provider — each residual row's `GetGradient()` assembles the exact Jacobian the normal-equation step consumes.

[LOCAL_ADMISSION]:
- Admit `HyperJet` for any exact gradient or Hessian of a smooth scalar objective or residual expressed in generic math; a caller-supplied black-box objective stays on the finite-difference row.
