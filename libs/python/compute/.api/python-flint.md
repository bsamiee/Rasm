# [PY_COMPUTE_API_PYTHON_FLINT]

`python-flint` (module `flint`) wraps the FLINT and Arb C libraries, supplying exact integer/rational arithmetic (`fmpz`, `fmpq`), modular arithmetic (`nmod`, `fmpz_mod`, `fq_default`), multivariate polynomials, and arbitrary-precision ball arithmetic (`arb`, `acb`) with certified error bounds for the compute exact-arithmetic and special-function rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-flint`
- package: `python-flint`
- module: `flint`
- asset: runtime library (Cython/C extension wrapping FLINT + Arb)
- rail: exact-arithmetic, ball-arithmetic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exact integer and rational types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]         | [CAPABILITY]                                     |
| :-----: | :------------ | :-------------------- | :----------------------------------------------- |
|  [01]   | `fmpz`        | exact integer         | GMP-backed arbitrary-precision integer           |
|  [02]   | `fmpq`        | exact rational        | exact rational over `fmpz` numerator/denominator |
|  [03]   | `fmpz_mat`    | exact integer matrix  | dense exact integer matrix with HNF/SNF/LLL      |
|  [04]   | `fmpq_mat`    | exact rational matrix | dense exact rational matrix                      |
|  [05]   | `fmpz_poly`   | exact integer poly    | univariate polynomial over `fmpz`                |
|  [06]   | `fmpq_poly`   | exact rational poly   | univariate polynomial over `fmpq`                |
|  [07]   | `fmpz_series` | power series          | power series over `fmpz`                         |
|  [08]   | `fmpq_series` | power series          | power series over `fmpq`                         |
|  [09]   | `fmpz_vec`    | integer vector        | dense vector of `fmpz`                           |
|  [10]   | `fmpq_vec`    | rational vector       | dense vector of `fmpq`                           |

[PUBLIC_TYPE_SCOPE]: modular and finite-field types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]           | [CAPABILITY]                             |
| :-----: | :---------------- | :---------------------- | :--------------------------------------- |
|  [01]   | `nmod`            | word-sized modular int  | arithmetic mod a machine-word modulus    |
|  [02]   | `nmod_mat`        | modular matrix          | dense matrix over `nmod`                 |
|  [03]   | `nmod_poly`       | modular polynomial      | univariate polynomial over `nmod`        |
|  [04]   | `nmod_series`     | modular series          | power series over `nmod`                 |
|  [05]   | `fmpz_mod`        | multiprecision mod int  | arithmetic mod a large-integer modulus   |
|  [06]   | `fmpz_mod_ctx`    | modulus context         | context object for `fmpz_mod` operations |
|  [07]   | `fmpz_mod_mat`    | modular matrix          | dense matrix over `fmpz_mod`             |
|  [08]   | `fmpz_mod_poly`   | modular polynomial      | univariate polynomial over `fmpz_mod`    |
|  [09]   | `fq_default`      | finite-field element    | element in GF(p^n) with Frobenius/norm   |
|  [10]   | `fq_default_ctx`  | finite-field context    | context for GF(p^n) with characteristic  |
|  [11]   | `fq_default_poly` | finite-field polynomial | polynomial over `fq_default`             |

[PUBLIC_TYPE_SCOPE]: multivariate polynomial types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :--------------- | :------------- | :------------------------------------- |
|  [01]   | `fmpz_mpoly`     | integer mpoly  | multivariate poly over `fmpz`          |
|  [02]   | `fmpz_mpoly_ctx` | mpoly context  | variable ordering and count for `fmpz` |
|  [03]   | `fmpq_mpoly`     | rational mpoly | multivariate poly over `fmpq`          |
|  [04]   | `fmpq_mpoly_ctx` | mpoly context  | variable ordering and count for `fmpq` |
|  [05]   | `nmod_mpoly`     | modular mpoly  | multivariate poly over `nmod`          |
|  [06]   | `fmpz_mod_mpoly` | modular mpoly  | multivariate poly over `fmpz_mod`      |

[PUBLIC_TYPE_SCOPE]: ball arithmetic types
- rail: ball-arithmetic

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]             | [CAPABILITY]                                      |
| :-----: | :----------- | :------------------------ | :------------------------------------------------ |
|  [01]   | `arb`        | real ball                 | real interval `[mid ± rad]` with certified bounds |
|  [02]   | `arb_mat`    | real ball matrix          | matrix of `arb` with certified eigenvalues, solve |
|  [03]   | `arb_poly`   | real ball polynomial      | polynomial with certified roots                   |
|  [04]   | `arb_series` | real ball series          | power series with rigorous truncation             |
|  [05]   | `acb`        | complex ball              | complex ball `[re ± r] + i[im ± r]`               |
|  [06]   | `acb_mat`    | complex ball matrix       | matrix of `acb` with DFT, solve, eig              |
|  [07]   | `acb_poly`   | complex ball polynomial   | polynomial with certified roots                   |
|  [08]   | `acb_series` | complex ball series       | complex power series                              |
|  [09]   | `arf`        | arbitrary-precision float | directed-rounding float mantissa/exponent         |

[PUBLIC_TYPE_SCOPE]: analytic/number-theory types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :---------------- | :------------------ | :----------------------------------------- |
|  [01]   | `dirichlet_char`  | Dirichlet character | character evaluation and properties        |
|  [02]   | `dirichlet_group` | character group     | Dirichlet group with character enumeration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `fmpz` exact integer operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `fmpz(n)` constructor                                      | construction   | from int or string             |
|  [02]   | `is_prime()` / `is_probable_prime()`                       | predicate      | primality test                 |
|  [03]   | `factor()` / `factor_smooth()`                             | factorization  | prime factorization            |
|  [04]   | `gcd(other)` / `lcm(other)`                                | arithmetic     | GCD and LCM                    |
|  [05]   | `sqrt()` / `isqrt()` / `root(n)`                           | roots          | exact and integer square roots |
|  [06]   | `fac_ui(n)` / `fib_ui(n)` / `bell_number(n)`               | combinatorics  | factorial, Fibonacci, Bell     |
|  [07]   | `euler_phi(n)` / `moebius_mu(n)` / `jacobi(a, n)`          | number theory  | Euler totient, Möbius, Jacobi  |
|  [08]   | `rising(x, n)` / `stirling_s1(n, k)` / `stirling_s2(n, k)` | combinatorics  | rising factorial, Stirling     |

[ENTRYPOINT_SCOPE]: `fmpz_mat` and `fmpq_mat` matrix operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `det()` / `rank()` / `inv()`             | linear algebra | determinant, rank, inverse       |
|  [02]   | `solve(b)` / `rref()` / `nullspace()`    | linear algebra | exact linear solve, RREF         |
|  [03]   | `charpoly()` / `minpoly()`               | polynomial     | characteristic and minimal poly  |
|  [04]   | `hnf()` / `snf()` / `lll()`              | lattice        | Hermite/Smith normal form, LLL   |
|  [05]   | `fflu()` / `hadamard()`                  | algorithms     | fraction-free LU, Hadamard bound |
|  [06]   | `transpose()` / `entries()` / `tolist()` | access         | transpose and data extraction    |

[ENTRYPOINT_SCOPE]: `fmpz_poly` and `fmpq_poly` polynomial operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `factor()` / `factor_squarefree()`                        | factorization  | polynomial factorization        |
|  [02]   | `gcd(other)` / `resultant(other)`                         | algebra        | GCD and resultant               |
|  [03]   | `roots()` / `real_roots()` / `complex_roots()`            | root finding   | exact and numerical roots       |
|  [04]   | `derivative()` / `integral()` (fmpq_poly)                 | calculus       | formal derivative/integral      |
|  [05]   | `cyclotomic(n)` / `chebyshev_t(n)` / `swinnerton_dyer(n)` | named poly     | cyclotomic, Chebyshev, SD       |
|  [06]   | `is_cyclotomic()` / `deflate()` / `inflate(n)`            | structure      | cyclotomic test and compression |

[ENTRYPOINT_SCOPE]: `arb` and `acb` ball arithmetic operations
- rail: ball-arithmetic

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `arb(x)` / `arb(x, r)` constructor                                      | construction   | midpoint or midpoint+radius ball       |
|  [02]   | `arb.pi()` / `arb.const_e()` / `arb.const_euler()`                      | constants      | certified mathematical constants       |
|  [03]   | `exp()` / `log()` / `sin()` / `cos()` / `sqrt()`                        | elementary     | certified elementary functions         |
|  [04]   | `gamma()` / `lgamma()` / `digamma()` / `rgamma()`                       | special        | gamma family with certified bounds     |
|  [05]   | `zeta()` / `polylog()` / `ei()` / `li()`                                | special        | Riemann zeta, polylogarithm, integrals |
|  [06]   | `erf()` / `erfc()` / `erfi()`                                           | special        | error functions                        |
|  [07]   | `bessel_j(n, z)` / `bessel_y(n, z)` / `bessel_i(n, z)`                  | special        | Bessel functions                       |
|  [08]   | `hypgeom(a, b, z)` / `hypgeom_1f1(a, b, z)` / `hypgeom_2f1(a, b, c, z)` | special        | hypergeometric                         |
|  [09]   | `contains(other)` / `overlaps(other)` / `mid()` / `rad()`               | ball ops       | containment and ball metadata          |
|  [10]   | `acb(re, im)` / `real()` / `imag()` / `conjugate()`                     | complex ops    | complex ball construction and parts    |
|  [11]   | `acb.dirichlet_l(s, char)` / `acb.zeta_zero(n)`                         | analytic       | Dirichlet L-function, zeta zeros       |
|  [12]   | `arb_mat.solve(b)` / `arb_mat.eig()` / `acb_mat.eig()`                  | matrix ops     | certified linear solve and eigenvalues |

[ENTRYPOINT_SCOPE]: context and precision control
- rail: ball-arithmetic

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `flint.ctx.prec` (get/set)                    | precision      | working bit-precision       |
|  [02]   | `flint.ctx.dps` (get/set)                     | precision      | decimal digits of precision |
|  [03]   | `flint.ctx.threads` (get/set)                 | threading      | FLINT thread count          |
|  [04]   | `flint.ctx.pretty` (get/set)                  | display        | human-readable output mode  |
|  [05]   | `flint.ctx.default()` / `flint.ctx.cleanup()` | lifecycle      | reset to defaults / cleanup |

## [04]-[IMPLEMENTATION_LAW]

[ARITHMETIC_TOPOLOGY]:
- namespace: `flint`; all types imported directly from `flint`
- `fmpz` and `fmpq` support full Python arithmetic operators; results are exact with no rounding
- `arb` and `acb` carry an interval `[mid ± rad]`; all operations propagate the radius; output is certified when `rad` is finite
- `ctx.prec` sets the working precision in bits for all `arb`/`acb` computation in the session
- matrix constructors accept nested lists or flat data with shape; `nrows()`/`ncols()` query dimensions
- modular types require an explicit context object (`fmpz_mod_ctx`, `fmpq_mpoly_ctx`, `fq_default_ctx`) that carries the modulus or field parameters

[LOCAL_ADMISSION]:
- Exact arithmetic uses `fmpz`/`fmpq` for integer and rational constants in solver kernels; never converts to float until the boundary.
- Ball arithmetic precision is set once at session scope via `flint.ctx.prec` before any `arb`/`acb` computation.
- Polynomial factorization and root isolation use `fmpz_poly.roots()` or `arb_poly.real_roots()` to produce certified intervals.
- `arb` results carry their own error bound; do not add external tolerance tracking when `rad` is already the certified bound.

[RAIL_LAW]:
- Package: `python-flint`
- Owns: exact integer/rational/modular arithmetic, multivariate polynomial algebra, certified ball arithmetic, and special-function evaluation with rigorous error bounds
- Accept: `fmpz`/`fmpq` for exact results; `arb`/`acb` for certified floating-point results
- Reject: `float` or `complex` conversions before the boundary when exact or certified results are required
