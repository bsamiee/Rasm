# [PY_COMPUTE_API_MPMATH]

`mpmath` supplies arbitrary-precision floating-point arithmetic, complex numbers, interval arithmetic, and a large library of special functions, numerical calculus, linear algebra, and number theory for the compute exact-arithmetic and symbolic-validation rail; it is the precision oracle that certifies a fast-path JAX/numba result against a guaranteed-correct high-`dps` evaluation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mpmath`
- package: `mpmath`
- version: `1.3.0`
- license: BSD-3-Clause
- import: `mpmath`
- owner: `compute`
- rail: exact-arithmetic
- asset: pure-Python; optional `gmpy2` backend accelerates the `mp` context when present
- capability: arbitrary-precision real, complex, and interval arithmetic with a large library covering special functions, numerical calculus, linear algebra, and number theory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and numeric types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                          |
| :-----: | :------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `MPContext`         | context class | arbitrary-precision real/complex context (`mp`) |
|  [02]   | `FPContext`         | context class | double-precision fallback context (`fp`)        |
|  [03]   | `MPIntervalContext` | context class | interval arithmetic context (`iv`)              |
|  [04]   | `mpf`               | numeric type  | arbitrary-precision real float                  |
|  [05]   | `mpc`               | numeric type  | arbitrary-precision complex float               |
|  [06]   | `mpi`               | numeric type  | real interval `[a, b]` (constructed via the `iv` context); rigorous error bounds |
|  [07]   | `matrix`            | numeric type  | arbitrary-precision dense matrix                |

[PUBLIC_TYPE_SCOPE]: mpf members
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]    | [KIND]   | [ROLE]            |
| :-----: | :---------- | :------- | :---------------- |
|  [01]   | `real`      | property | real part         |
|  [02]   | `imag`      | property | imaginary part    |
|  [03]   | `conjugate` | method   | complex conjugate |
|  [04]   | `context`   | property | owning context    |

[PUBLIC_TYPE_SCOPE]: matrix members
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]    | [KIND]   | [ROLE]                         |
| :-----: | :---------- | :------- | :----------------------------- |
|  [01]   | `rows`      | property | row count                      |
|  [02]   | `cols`      | property | column count                   |
|  [03]   | `T`         | property | transpose                      |
|  [04]   | `H`         | property | conjugate transpose            |
|  [05]   | `apply`     | method   | element-wise function apply    |
|  [06]   | `tolist`    | method   | convert to nested Python lists |
|  [07]   | `transpose` | method   | explicit transpose             |
|  [08]   | `conjugate` | method   | element-wise conjugate         |
|  [09]   | `copy`      | method   | deep copy                      |
|  [10]   | `convert`   | method   | convert element types          |

[PUBLIC_TYPE_SCOPE]: context attributes
- rail: exact-arithmetic
- context: `mp` (default working context)

| [INDEX] | [SYMBOL]  | [KIND]   | [ROLE]                       |
| :-----: | :-------- | :------- | :--------------------------- |
|  [01]   | `mp.prec` | int attr | working precision in bits    |
|  [02]   | `mp.dps`  | int attr | decimal places (sets `prec`) |
|  [03]   | `mp.inf`  | constant | positive infinity as `mpf`   |
|  [04]   | `mp.j`    | constant | imaginary unit as `mpc`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: precision management
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------- | :------------- | :---------------------------- |
|  [01]   | `mp.prec = n`                  | context set    | set working precision in bits |
|  [02]   | `mp.dps = n`                   | context set    | set decimal places            |
|  [03]   | `workprec(n)`                  | context mgr    | temporary bit precision       |
|  [04]   | `workdps(n)`                   | context mgr    | temporary decimal precision   |
|  [05]   | `extraprec(n)`                 | context mgr    | add extra bits temporarily    |
|  [06]   | `extradps(n)`                  | context mgr    | add extra decimal places      |
|  [07]   | `autoprec(f, *args, **kwargs)` | adaptive       | auto-calibrate precision      |

[ENTRYPOINT_SCOPE]: extended-precision arithmetic, conversion, and boundary
- rail: exact-arithmetic — the precision-safe accumulation and Python/numpy edge primitives

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [RAIL]                                                       |
| :-----: | :---------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `fsum(terms, absolute=False, squared=False)`    | accumulation    | correctly-rounded sum of an iterable (no catastrophic cancellation) |
|  [02]   | `fprod(factors)`                                | accumulation    | correctly-rounded product of an iterable                     |
|  [03]   | `fdot(A, B, conjugate=False)`                   | accumulation    | correctly-rounded dot product of two iterables               |
|  [04]   | `fadd`, `fsub`, `fmul`, `fdiv`, `fneg`          | fused op        | single-rounding binary ops with an explicit `prec`/`rounding` |
|  [05]   | `mpmathify(x)`                                  | conversion      | coerce str/int/float/Fraction/Decimal into the active context number |
|  [06]   | `nstr(x, n=6, **kwargs)` / `nint(x)` / `frac(x)`| formatting      | decimal-string rendering and integer/fractional split        |
|  [07]   | `chop(x, tol=None)`                             | cleanup         | zero out negligible real/imag parts below `tol`              |
|  [08]   | `almosteq(s, t, rel_eps=None, abs_eps=None)`    | comparison      | tolerance-aware equality for floating results                |
|  [09]   | `isnan`, `isinf`, `isfinite`, `isint`           | predicate       | numeric-class predicates over `mpf`/`mpc`                     |
|  [10]   | `ldexp(x, n)` / `frexp(x)` / `mag(x)`           | bit ops         | mantissa/exponent decomposition and binary magnitude          |
|  [11]   | `arange(*args)` / `linspace(a, b, n, **kw)`     | sampling        | arbitrary-precision ranges for series/quadrature grids        |

[ENTRYPOINT_SCOPE]: mathematical constants (each is a lazy `mpf` re-evaluated at the active `dps`)
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `pi`, `e`, `phi`, `degree`, `eps`                                              | core           | π, e, golden ratio, deg→rad, machine eps |
|  [02]   | `ln2`, `ln10`, `catalan`, `euler`, `apery`, `khinchin`, `glaisher`, `mertens`, `twinprime` | named   | named mathematical constants     |

[ENTRYPOINT_SCOPE]: elementary and trigonometric functions
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `sqrt`, `cbrt`, `nthroot`, `power`, `exp`, `expm1`, `log`, `log10`, `log1p` | elementary      | standard elementary ops       |
|  [02]   | `sin`, `cos`, `tan`, `sinpi`, `cospi`                                       | trig            | circular trig                 |
|  [03]   | `asin`, `acos`, `atan`, `atan2`                                             | trig            | inverse trig                  |
|  [04]   | `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh`                           | hyperbolic      | hyperbolic trig               |
|  [05]   | `sec`, `csc`, `cot`, `sech`, `csch`, `coth`                                 | reciprocal trig | reciprocal functions          |
|  [06]   | `expj`, `expjpi`, `cplot`, `splot`                                          | complex         | complex exponential and plots |
|  [07]   | `fabs`, `sign`, `re`, `im`, `arg`, `phase`, `polar`, `rect`                 | complex utils   | modulus, argument, polar form |

[ENTRYPOINT_SCOPE]: special functions
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY]    | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :---------------- | :-------------------------- |
|  [01]   | `gamma`, `loggamma`, `rgamma`, `digamma`, `polygamma`, `psi`                                               | gamma family      | gamma and digamma           |
|  [02]   | `factorial`, `fac`, `fac2`, `hyperfac`, `superfac`                                                         | factorials        | factorial variants          |
|  [03]   | `beta`, `betainc`, `gammainc`, `gammaprod`                                                                 | beta/gamma integ. | incomplete/combined         |
|  [04]   | `erf`, `erfc`, `erfi`, `erfinv`, `ncdf`, `npdf`                                                            | error functions   | error and normal            |
|  [05]   | `besselj`, `bessely`, `besseli`, `besselk`, `hankel1`, `hankel2`                                           | Bessel            | Bessel functions            |
|  [06]   | `besseljzero`, `besselyzero`                                                                               | Bessel zeros      | zero finding                |
|  [07]   | `hyp0f1`, `hyp1f1`, `hyp1f2`, `hyp2f0`, `hyp2f1`, `hyp2f2`, `hyp2f3`, `hyp3f2`, `hyper`                    | hypergeometric    | generalized hypergeometric  |
|  [08]   | `ellipk`, `ellipe`, `ellipf`, `ellippi`, `elliprc`, `elliprd`, `elliprf`, `elliprg`, `elliprj`, `ellipfun` | elliptic          | elliptic integrals          |
|  [09]   | `zeta`, `hurwitz`, `eta`, `altzeta`, `stieltjes`, `secondzeta`                                             | zeta functions    | Riemann and generalizations |
|  [10]   | `zetazero`, `siegelz`, `siegeltheta`, `backlunds`, `grampoint`, `nzeros`                                   | zeta zeros        | Riemann zero analysis       |
|  [11]   | `airyai`, `airyaizero`, `airybi`, `airybizero`                                                             | Airy              | Airy functions and zeros    |
|  [12]   | `legendre`, `legenp`, `legenq`, `chebyt`, `chebyu`, `hermite`, `laguerre`, `jacobi`, `gegenbauer`          | orthogonal        | orthogonal polynomials      |
|  [13]   | `ei`, `e1`, `expint`, `li`, `ci`, `si`, `chi`, `shi`, `fresnels`, `fresnelc`                               | exp/trig integrals | exponential & sine/cosine integrals |
|  [14]   | `meijerg`, `hyperu`, `appellf1`, `appellf2`, `appellf3`, `appellf4`                                        | advanced hypergeom | Meijer-G, confluent-U, Appell series |
|  [15]   | `polylog`, `clsin`, `clcos`, `barnesg`, `harmonic`, `bernoulli`, `bernfrac`                                | special/number     | polylogarithm, Clausen, Barnes-G, Bernoulli |
|  [16]   | `coulombf`, `coulombg`, `whitm`, `whitw`, `pcfd`, `pcfu`, `pcfv`, `pcfw`                                    | mathematical physics | Coulomb wave, Whittaker, parabolic cylinder |
|  [17]   | `agm`, `primepi`, `riemannr`                                                                               | misc               | arithmetic-geometric mean, prime-count |

[ENTRYPOINT_SCOPE]: calculus and integration
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `quad(f, *points, **kwargs)`           | integration     | adaptive quadrature         |
|  [02]   | `quadts(f, *points, **kwargs)`         | integration     | tanh-sinh quadrature        |
|  [03]   | `quadgl(f, *points, **kwargs)`         | integration     | Gauss-Legendre quadrature   |
|  [04]   | `quadosc(f, *points, **kwargs)`        | integration     | oscillatory quadrature      |
|  [05]   | `diff(f, x, n=1, **kwargs)`            | differentiation | numerical derivative        |
|  [06]   | `diffs(f, x, n=None, **kwargs)`        | differentiation | sequence of derivatives     |
|  [07]   | `diffun(f, n=1, **kwargs)`             | differentiation | derivative function builder |
|  [08]   | `differint(f, x, q, **kwargs)`         | fractional calc | fractional derivative       |
|  [09]   | `nsum(f, *intervals, **kwargs)`        | summation       | numerical series sum        |
|  [10]   | `nprod(f, *intervals, **kwargs)`       | product         | numerical series product    |
|  [11]   | `odefun(f, x0, y0, **kwargs)`          | ODE             | ODE integrator              |
|  [12]   | `invertlaplace(f, t, **kwargs)`        | Laplace         | inverse Laplace transform   |
|  [13]   | `limit(f, x, direction='+', **kwargs)` | limits          | numerical limit             |
|  [14]   | `taylor(f, x, n, **kwargs)`            | series          | Taylor series coefficients  |
|  [15]   | `pade(a, m, n)`                        | approximation   | Padé approximant            |
|  [16]   | `chebyfit(f, interval, N, **kwargs)`   | approximation   | Chebyshev polynomial fit on an interval |
|  [17]   | `fourier(f, interval, N)` / `fourierval(series, interval, x)` | series | Fourier series fit and evaluation |
|  [18]   | `richardson(seq)` / `shanks(seq)`      | acceleration    | Richardson extrapolation / Shanks transform of a partial-sum sequence |
|  [19]   | `sumem(f, interval, ...)` / `sumap(f, interval, ...)` | acceleration | Euler-Maclaurin / Abel-Plana summation |
|  [20]   | `polyval(coeffs, x, derivative=False)` | polynomial      | Horner evaluation of a polynomial          |

[ENTRYPOINT_SCOPE]: linear algebra and root-finding
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `lu(A)`, `lu_solve(A, b)`                                | linear algebra | LU factorization and solve   |
|  [02]   | `qr(A)`, `qr_solve(A, b)`                                | linear algebra | QR factorization and solve   |
|  [03]   | `cholesky(A)`, `cholesky_solve(A, b)`                    | linear algebra | Cholesky and solve           |
|  [04]   | `svd(A)`, `svd_r(A)`, `svd_c(A)`                         | linear algebra | SVD (real/complex)           |
|  [05]   | `eig(A)`, `eigh(A)`, `eig_sort(E, v)`                    | eigenvalues    | general and symmetric eigen  |
|  [06]   | `det(A)`, `inverse(A)`, `norm(A)`, `mnorm(A)`, `cond(A)` | matrix ops     | determinant, norm, condition |
|  [07]   | `findroot(f, x0, **kwargs)`                              | root finding   | nonlinear root               |
|  [08]   | `polyroots(coeffs, **kwargs)`                            | polynomials    | polynomial root finding      |
|  [09]   | `jacobian(f, x, **kwargs)`                               | derivatives    | Jacobian matrix              |
|  [10]   | `identify(x, tol=None, **kwargs)`                        | recognition    | constant identification      |
|  [11]   | `pslq(x, tol=None, **kwargs)`                            | recognition    | PSLQ integer relation        |

## [04]-[IMPLEMENTATION_LAW]

[MPMATH_TOPOLOGY]:
- global contexts: `mp` (mutable arbitrary-precision via `MPContext`), `fp` (double-precision fixed via `FPContext`), `iv` (interval via `MPIntervalContext`); every elementary/special function is a bound method on each context with identical naming.
- precision set via `mp.dps` (decimal) or `mp.prec` (bits); 1 dps ≈ 3.32 bits. Setting one updates the other.
- context managers `workdps`/`workprec`/`extradps`/`extraprec` restore the prior precision on exit; nest them to bracket a high-precision intermediate without leaking global state.
- all `mp.*` functions are re-exported at module top-level (e.g. `mpmath.quad` == `mpmath.mp.quad`); the `fp` and `iv` contexts expose the same names for double-precision and interval evaluation respectively.
- the `gmpy2` backend, when installed, accelerates the `mp` context transparently; no API change.

[LOCAL_ADMISSION]:
- Set `mp.dps` before calling any function; do not rely on the default 15-digit precision for high-accuracy work.
- Use `workdps(n)`/`extradps(n)` context managers in study receipts to avoid precision state leaking across evaluations, and `autoprec(f, ...)` when the needed precision is not known a priori.
- Use `fsum`/`fdot`/`fprod` for accumulation rather than Python `sum`/`@` to avoid catastrophic cancellation at any precision.
- Interval arithmetic (`iv.mpf`, `mpi`) bounds absolute error rigorously; use it to certify that a fast-path result lies inside a proven enclosure.
- `findpoly`/`identify`/`pslq` are constant-recognition tools for study; never use in production numeric paths.

[INTEGRATION_TOPOLOGY]:

| [INDEX] | [SIBLING]       | [SEAM]                                                                                                            |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `sympy`         | `sympy.Expr.evalf(n)` is backed by mpmath; lift a symbolic closed form to `mpmath.mpmathify(sympy_expr)` or hand `lambdify(..., 'mpmath')` to validate a derived expression at arbitrary precision |
|  [02]   | `jax`/`numba`   | the precision oracle: evaluate the same kernel at `mp.dps=50`, then `almosteq(fast_result, mp_result, rel_eps=1e-12)` to certify the JAX/numba fast path's accuracy as study evidence |
|  [03]   | `python-flint`  | flint owns exact rationals/`arb` balls for speed; mpmath owns the broad special-function library — recognise a constant with `identify`/`pslq`, then carry it as an exact flint number |
|  [04]   | `uncertainties` | propagate an mpmath-evaluated nominal value plus a high-precision derivative (`diff`) into an `uncertainties` linear error model when the function has no closed-form gradient |
|  [05]   | `findiff`/`scipy` | seed a finite-difference or quadrature scheme's reference value with `mpmath.quad`/`diff` at high `dps` to measure the discretisation error of the fast numerical method |

[RAIL_LAW]:
- Package: `mpmath`
- Owns: arbitrary-precision real, complex, interval arithmetic and a broad special-function/calculus library; the precision-oracle role for fast-path validation
- Accept: `mp.dps`-scoped evaluation via context managers; `fsum`/`fdot`/`fprod` accumulation; structured receipts capturing precision, method, and residual
- Reject: fixed-precision reimplementations of functions mpmath owns; production paths using mpmath for performance-critical evaluation; Python `sum`/dot accumulation where cancellation matters
