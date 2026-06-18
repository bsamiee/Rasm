# [PY_COMPUTE_API_MPMATH]

`mpmath` supplies arbitrary-precision floating-point arithmetic, complex numbers, interval arithmetic, and a 360-function library of special functions, calculus routines, linear algebra, and number theory for the compute exact-arithmetic and symbolic-validation rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mpmath`
- package: `mpmath`
- import: `mpmath`
- owner: `compute`
- rail: exact-arithmetic
- capability: arbitrary-precision real, complex, and interval arithmetic with a 360-function library covering special functions, numerical calculus, linear algebra, and number theory

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and numeric types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [ROLE]                                          |
| :-----: | :------------------ | :------------ | :---------------------------------------------- |
|   [1]   | `MPContext`         | context class | arbitrary-precision real/complex context (`mp`) |
|   [2]   | `FPContext`         | context class | double-precision fallback context (`fp`)        |
|   [3]   | `MPIntervalContext` | context class | interval arithmetic context (`iv`)              |
|   [4]   | `mpf`               | numeric type  | arbitrary-precision real float                  |
|   [5]   | `mpc`               | numeric type  | arbitrary-precision complex float               |
|   [6]   | `matrix`            | numeric type  | arbitrary-precision matrix                      |

[PUBLIC_TYPE_SCOPE]: mpf members
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]    | [KIND]   | [ROLE]            |
| :-----: | :---------- | :------- | :---------------- |
|   [1]   | `real`      | property | real part         |
|   [2]   | `imag`      | property | imaginary part    |
|   [3]   | `conjugate` | method   | complex conjugate |
|   [4]   | `context`   | property | owning context    |

[PUBLIC_TYPE_SCOPE]: matrix members
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]    | [KIND]   | [ROLE]                         |
| :-----: | :---------- | :------- | :----------------------------- |
|   [1]   | `rows`      | property | row count                      |
|   [2]   | `cols`      | property | column count                   |
|   [3]   | `T`         | property | transpose                      |
|   [4]   | `H`         | property | conjugate transpose            |
|   [5]   | `apply`     | method   | element-wise function apply    |
|   [6]   | `tolist`    | method   | convert to nested Python lists |
|   [7]   | `transpose` | method   | explicit transpose             |
|   [8]   | `conjugate` | method   | element-wise conjugate         |
|   [9]   | `copy`      | method   | deep copy                      |
|  [10]   | `convert`   | method   | convert element types          |

[PUBLIC_TYPE_SCOPE]: context attributes
- rail: exact-arithmetic
- context: `mp` (default working context)

| [INDEX] | [SYMBOL]  | [KIND]   | [ROLE]                       |
| :-----: | :-------- | :------- | :--------------------------- |
|   [1]   | `mp.prec` | int attr | working precision in bits    |
|   [2]   | `mp.dps`  | int attr | decimal places (sets `prec`) |
|   [3]   | `mp.inf`  | constant | positive infinity as `mpf`   |
|   [4]   | `mp.j`    | constant | imaginary unit as `mpc`      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: precision management
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------- | :------------- | :---------------------------- |
|   [1]   | `mp.prec = n`                  | context set    | set working precision in bits |
|   [2]   | `mp.dps = n`                   | context set    | set decimal places            |
|   [3]   | `workprec(n)`                  | context mgr    | temporary bit precision       |
|   [4]   | `workdps(n)`                   | context mgr    | temporary decimal precision   |
|   [5]   | `extraprec(n)`                 | context mgr    | add extra bits temporarily    |
|   [6]   | `extradps(n)`                  | context mgr    | add extra decimal places      |
|   [7]   | `autoprec(f, *args, **kwargs)` | adaptive       | auto-calibrate precision      |

[ENTRYPOINT_SCOPE]: elementary and trigonometric functions
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `sqrt`, `cbrt`, `nthroot`, `power`, `exp`, `expm1`, `log`, `log10`, `log1p` | elementary      | standard elementary ops       |
|   [2]   | `sin`, `cos`, `tan`, `sinpi`, `cospi`                                       | trig            | circular trig                 |
|   [3]   | `asin`, `acos`, `atan`, `atan2`                                             | trig            | inverse trig                  |
|   [4]   | `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh`                           | hyperbolic      | hyperbolic trig               |
|   [5]   | `sec`, `csc`, `cot`, `sech`, `csch`, `coth`                                 | reciprocal trig | reciprocal functions          |
|   [6]   | `expj`, `expjpi`, `cplot`, `splot`                                          | complex         | complex exponential and plots |
|   [7]   | `fabs`, `sign`, `re`, `im`, `arg`, `phase`, `polar`, `rect`                 | complex utils   | modulus, argument, polar form |

[ENTRYPOINT_SCOPE]: special functions
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY]    | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :---------------- | :-------------------------- |
|   [1]   | `gamma`, `loggamma`, `rgamma`, `digamma`, `polygamma`, `psi`                                               | gamma family      | gamma and digamma           |
|   [2]   | `factorial`, `fac`, `fac2`, `hyperfac`, `superfac`                                                         | factorials        | factorial variants          |
|   [3]   | `beta`, `betainc`, `gammainc`, `gammaprod`                                                                 | beta/gamma integ. | incomplete/combined         |
|   [4]   | `erf`, `erfc`, `erfi`, `erfinv`, `ncdf`, `npdf`                                                            | error functions   | error and normal            |
|   [5]   | `besselj`, `bessely`, `besseli`, `besselk`, `hankel1`, `hankel2`                                           | Bessel            | Bessel functions            |
|   [6]   | `besseljzero`, `besselyzero`                                                                               | Bessel zeros      | zero finding                |
|   [7]   | `hyp0f1`, `hyp1f1`, `hyp1f2`, `hyp2f0`, `hyp2f1`, `hyp2f2`, `hyp2f3`, `hyp3f2`, `hyper`                    | hypergeometric    | generalized hypergeometric  |
|   [8]   | `ellipk`, `ellipe`, `ellipf`, `ellippi`, `elliprc`, `elliprd`, `elliprf`, `elliprg`, `elliprj`, `ellipfun` | elliptic          | elliptic integrals          |
|   [9]   | `zeta`, `hurwitz`, `eta`, `altzeta`, `stieltjes`, `secondzeta`                                             | zeta functions    | Riemann and generalizations |
|  [10]   | `zetazero`, `siegelz`, `siegeltheta`, `backlunds`, `grampoint`, `nzeros`                                   | zeta zeros        | Riemann zero analysis       |
|  [11]   | `airyai`, `airyaizero`, `airybi`, `airybizero`                                                             | Airy              | Airy functions and zeros    |
|  [12]   | `legendre`, `legenp`, `legenq`, `chebyt`, `chebyu`, `hermite`, `laguerre`, `jacobi`, `gegenbauer`          | orthogonal        | orthogonal polynomials      |

[ENTRYPOINT_SCOPE]: calculus and integration
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :------------------------------------- | :-------------- | :-------------------------- |
|   [1]   | `quad(f, *points, **kwargs)`           | integration     | adaptive quadrature         |
|   [2]   | `quadts(f, *points, **kwargs)`         | integration     | tanh-sinh quadrature        |
|   [3]   | `quadgl(f, *points, **kwargs)`         | integration     | Gauss-Legendre quadrature   |
|   [4]   | `quadosc(f, *points, **kwargs)`        | integration     | oscillatory quadrature      |
|   [5]   | `diff(f, x, n=1, **kwargs)`            | differentiation | numerical derivative        |
|   [6]   | `diffs(f, x, n=None, **kwargs)`        | differentiation | sequence of derivatives     |
|   [7]   | `diffun(f, n=1, **kwargs)`             | differentiation | derivative function builder |
|   [8]   | `differint(f, x, q, **kwargs)`         | fractional calc | fractional derivative       |
|   [9]   | `nsum(f, *intervals, **kwargs)`        | summation       | numerical series sum        |
|  [10]   | `nprod(f, *intervals, **kwargs)`       | product         | numerical series product    |
|  [11]   | `odefun(f, x0, y0, **kwargs)`          | ODE             | ODE integrator              |
|  [12]   | `invertlaplace(f, t, **kwargs)`        | Laplace         | inverse Laplace transform   |
|  [13]   | `limit(f, x, direction='+', **kwargs)` | limits          | numerical limit             |
|  [14]   | `taylor(f, x, n, **kwargs)`            | series          | Taylor series coefficients  |
|  [15]   | `pade(a, m, n)`                        | approximation   | Padé approximant            |

[ENTRYPOINT_SCOPE]: linear algebra and root-finding
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `lu(A)`, `lu_solve(A, b)`                                | linear algebra | LU factorization and solve   |
|   [2]   | `qr(A)`, `qr_solve(A, b)`                                | linear algebra | QR factorization and solve   |
|   [3]   | `cholesky(A)`, `cholesky_solve(A, b)`                    | linear algebra | Cholesky and solve           |
|   [4]   | `svd(A)`, `svd_r(A)`, `svd_c(A)`                         | linear algebra | SVD (real/complex)           |
|   [5]   | `eig(A)`, `eigh(A)`, `eig_sort(E, v)`                    | eigenvalues    | general and symmetric eigen  |
|   [6]   | `det(A)`, `inverse(A)`, `norm(A)`, `mnorm(A)`, `cond(A)` | matrix ops     | determinant, norm, condition |
|   [7]   | `findroot(f, x0, **kwargs)`                              | root finding   | nonlinear root               |
|   [8]   | `polyroots(coeffs, **kwargs)`                            | polynomials    | polynomial root finding      |
|   [9]   | `jacobian(f, x, **kwargs)`                               | derivatives    | Jacobian matrix              |
|  [10]   | `identify(x, tol=None, **kwargs)`                        | recognition    | constant identification      |
|  [11]   | `pslq(x, tol=None, **kwargs)`                            | recognition    | PSLQ integer relation        |

## [4]-[IMPLEMENTATION_LAW]

[MPMATH_TOPOLOGY]:
- global context: `mp` (mutable), `fp` (double-precision fixed), `iv` (interval)
- precision set via `mp.dps` (decimal) or `mp.prec` (bits); 1 dps ≈ 3.32 bits
- context managers `workdps`/`workprec`/`extradps`/`extraprec` restore precision on exit
- all `mp.*` functions also available at module top-level (e.g. `mpmath.quad`)
- 605 introspected types across 12 namespaces including calculus, functions, matrices, libmp

[LOCAL_ADMISSION]:
- Set `mp.dps` before calling any function; do not rely on the default 15-digit precision for high-accuracy work.
- Use `workdps(n)` context managers in study receipts to avoid precision state leaking across evaluations.
- Interval arithmetic (`iv.*`) bounds absolute error; use it to verify that a result is within tolerance.
- `findpoly`/`identify`/`pslq` are recognition tools for study; never use in production numeric paths.

[RAIL_LAW]:
- Package: `mpmath`
- Owns: arbitrary-precision real, complex, interval arithmetic and special function evaluation
- Accept: `mp.dps`-scoped evaluation via context managers; structured receipts capturing precision, method, and residual
- Reject: fixed-precision reimplementations of functions mpmath owns; production paths using mpmath for performance-critical evaluation
