# [PY_COMPUTE_API_PYTHON_FLINT]

`python-flint` (module `flint`) wraps the FLINT and Arb C libraries, supplying exact integer/rational arithmetic (`fmpz`, `fmpq`), modular arithmetic (`nmod`, `fmpz_mod`, `fq_default`), multivariate polynomials, and arbitrary-precision ball arithmetic (`arb`, `acb`) with certified error bounds for the compute exact-arithmetic and special-function rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-flint`
- package: `python-flint`
- module: `flint`
- asset: runtime library (Cython/C extension wrapping FLINT + Arb)
- rail: exact-arithmetic, ball-arithmetic

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exact integer and rational types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]         | [CAPABILITY]                                     |
| :-----: | :------------ | :-------------------- | :----------------------------------------------- |
|   [1]   | `fmpz`        | exact integer         | GMP-backed arbitrary-precision integer           |
|   [2]   | `fmpq`        | exact rational        | exact rational over `fmpz` numerator/denominator |
|   [3]   | `fmpz_mat`    | exact integer matrix  | dense exact integer matrix with HNF/SNF/LLL      |
|   [4]   | `fmpq_mat`    | exact rational matrix | dense exact rational matrix                      |
|   [5]   | `fmpz_poly`   | exact integer poly    | univariate polynomial over `fmpz`                |
|   [6]   | `fmpq_poly`   | exact rational poly   | univariate polynomial over `fmpq`                |
|   [7]   | `fmpz_series` | power series          | power series over `fmpz`                         |
|   [8]   | `fmpq_series` | power series          | power series over `fmpq`                         |
|   [9]   | `fmpz_vec`    | integer vector        | dense vector of `fmpz`                           |
|  [10]   | `fmpq_vec`    | rational vector       | dense vector of `fmpq`                           |

[PUBLIC_TYPE_SCOPE]: modular and finite-field types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]           | [CAPABILITY]                             |
| :-----: | :---------------- | :---------------------- | :--------------------------------------- |
|   [1]   | `nmod`            | word-sized modular int  | arithmetic mod a machine-word modulus    |
|   [2]   | `nmod_mat`        | modular matrix          | dense matrix over `nmod`                 |
|   [3]   | `nmod_poly`       | modular polynomial      | univariate polynomial over `nmod`        |
|   [4]   | `nmod_series`     | modular series          | power series over `nmod`                 |
|   [5]   | `fmpz_mod`        | multiprecision mod int  | arithmetic mod a large-integer modulus   |
|   [6]   | `fmpz_mod_ctx`    | modulus context         | context object for `fmpz_mod` operations |
|   [7]   | `fmpz_mod_mat`    | modular matrix          | dense matrix over `fmpz_mod`             |
|   [8]   | `fmpz_mod_poly`   | modular polynomial      | univariate polynomial over `fmpz_mod`    |
|   [9]   | `fq_default`      | finite-field element    | element in GF(p^n) with Frobenius/norm   |
|  [10]   | `fq_default_ctx`  | finite-field context    | context for GF(p^n) with characteristic  |
|  [11]   | `fq_default_poly` | finite-field polynomial | polynomial over `fq_default`             |

[PUBLIC_TYPE_SCOPE]: multivariate polynomial types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :--------------- | :------------- | :------------------------------------- |
|   [1]   | `fmpz_mpoly`     | integer mpoly  | multivariate poly over `fmpz`          |
|   [2]   | `fmpz_mpoly_ctx` | mpoly context  | variable ordering and count for `fmpz` |
|   [3]   | `fmpq_mpoly`     | rational mpoly | multivariate poly over `fmpq`          |
|   [4]   | `fmpq_mpoly_ctx` | mpoly context  | variable ordering and count for `fmpq` |
|   [5]   | `nmod_mpoly`     | modular mpoly  | multivariate poly over `nmod`          |
|   [6]   | `fmpz_mod_mpoly` | modular mpoly  | multivariate poly over `fmpz_mod`      |

[PUBLIC_TYPE_SCOPE]: ball arithmetic types
- rail: ball-arithmetic

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]             | [CAPABILITY]                                      |
| :-----: | :----------- | :------------------------ | :------------------------------------------------ |
|   [1]   | `arb`        | real ball                 | real interval `[mid ± rad]` with certified bounds |
|   [2]   | `arb_mat`    | real ball matrix          | matrix of `arb` with certified eigenvalues, solve |
|   [3]   | `arb_poly`   | real ball polynomial      | polynomial with certified roots                   |
|   [4]   | `arb_series` | real ball series          | power series with rigorous truncation             |
|   [5]   | `acb`        | complex ball              | complex ball `[re ± r] + i[im ± r]`               |
|   [6]   | `acb_mat`    | complex ball matrix       | matrix of `acb` with DFT, solve, eig              |
|   [7]   | `acb_poly`   | complex ball polynomial   | polynomial with certified roots                   |
|   [8]   | `acb_series` | complex ball series       | complex power series                              |
|   [9]   | `arf`        | arbitrary-precision float | directed-rounding float mantissa/exponent         |

[PUBLIC_TYPE_SCOPE]: analytic/number-theory types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :---------------- | :------------------ | :----------------------------------------- |
|   [1]   | `dirichlet_char`  | Dirichlet character | character evaluation and properties        |
|   [2]   | `dirichlet_group` | character group     | Dirichlet group with character enumeration |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `fmpz` exact integer operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `fmpz(n)` constructor                                      | construction   | from int or string             |
|   [2]   | `is_prime()` / `is_probable_prime()`                       | predicate      | primality test                 |
|   [3]   | `factor()` / `factor_smooth()`                             | factorization  | prime factorization            |
|   [4]   | `gcd(other)` / `lcm(other)`                                | arithmetic     | GCD and LCM                    |
|   [5]   | `sqrt()` / `isqrt()` / `root(n)`                           | roots          | exact and integer square roots |
|   [6]   | `fac_ui(n)` / `fib_ui(n)` / `bell_number(n)`               | combinatorics  | factorial, Fibonacci, Bell     |
|   [7]   | `euler_phi(n)` / `moebius_mu(n)` / `jacobi(a, n)`          | number theory  | Euler totient, Möbius, Jacobi  |
|   [8]   | `rising(x, n)` / `stirling_s1(n, k)` / `stirling_s2(n, k)` | combinatorics  | rising factorial, Stirling     |

[ENTRYPOINT_SCOPE]: `fmpz_mat` and `fmpq_mat` matrix operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `det()` / `rank()` / `inv()`             | linear algebra | determinant, rank, inverse       |
|   [2]   | `solve(b)` / `rref()` / `nullspace()`    | linear algebra | exact linear solve, RREF         |
|   [3]   | `charpoly()` / `minpoly()`               | polynomial     | characteristic and minimal poly  |
|   [4]   | `hnf()` / `snf()` / `lll()`              | lattice        | Hermite/Smith normal form, LLL   |
|   [5]   | `fflu()` / `hadamard()`                  | algorithms     | fraction-free LU, Hadamard bound |
|   [6]   | `transpose()` / `entries()` / `tolist()` | access         | transpose and data extraction    |

[ENTRYPOINT_SCOPE]: `fmpz_poly` and `fmpq_poly` polynomial operations
- rail: exact-arithmetic

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `factor()` / `factor_squarefree()`                        | factorization  | polynomial factorization        |
|   [2]   | `gcd(other)` / `resultant(other)`                         | algebra        | GCD and resultant               |
|   [3]   | `roots()` / `real_roots()` / `complex_roots()`            | root finding   | exact and numerical roots       |
|   [4]   | `derivative()` / `integral()` (fmpq_poly)                 | calculus       | formal derivative/integral      |
|   [5]   | `cyclotomic(n)` / `chebyshev_t(n)` / `swinnerton_dyer(n)` | named poly     | cyclotomic, Chebyshev, SD       |
|   [6]   | `is_cyclotomic()` / `deflate()` / `inflate(n)`            | structure      | cyclotomic test and compression |

[ENTRYPOINT_SCOPE]: `arb` and `acb` ball arithmetic operations
- rail: ball-arithmetic

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `arb(x)` / `arb(x, r)` constructor                                      | construction   | midpoint or midpoint+radius ball       |
|   [2]   | `arb.pi()` / `arb.const_e()` / `arb.const_euler()`                      | constants      | certified mathematical constants       |
|   [3]   | `exp()` / `log()` / `sin()` / `cos()` / `sqrt()`                        | elementary     | certified elementary functions         |
|   [4]   | `gamma()` / `lgamma()` / `digamma()` / `rgamma()`                       | special        | gamma family with certified bounds     |
|   [5]   | `zeta()` / `polylog()` / `ei()` / `li()`                                | special        | Riemann zeta, polylogarithm, integrals |
|   [6]   | `erf()` / `erfc()` / `erfi()`                                           | special        | error functions                        |
|   [7]   | `bessel_j(n, z)` / `bessel_y(n, z)` / `bessel_i(n, z)`                  | special        | Bessel functions                       |
|   [8]   | `hypgeom(a, b, z)` / `hypgeom_1f1(a, b, z)` / `hypgeom_2f1(a, b, c, z)` | special        | hypergeometric                         |
|   [9]   | `contains(other)` / `overlaps(other)` / `mid()` / `rad()`               | ball ops       | containment and ball metadata          |
|  [10]   | `acb(re, im)` / `real()` / `imag()` / `conjugate()`                     | complex ops    | complex ball construction and parts    |
|  [11]   | `acb.dirichlet_l(s, char)` / `acb.zeta_zero(n)`                         | analytic       | Dirichlet L-function, zeta zeros       |
|  [12]   | `arb_mat.solve(b)` / `arb_mat.eig()` / `acb_mat.eig()`                  | matrix ops     | certified linear solve and eigenvalues |

[ENTRYPOINT_SCOPE]: context and precision control
- rail: ball-arithmetic

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------- |
|   [1]   | `flint.ctx.prec` (get/set)                    | precision      | working bit-precision       |
|   [2]   | `flint.ctx.dps` (get/set)                     | precision      | decimal digits of precision |
|   [3]   | `flint.ctx.threads` (get/set)                 | threading      | FLINT thread count          |
|   [4]   | `flint.ctx.pretty` (get/set)                  | display        | human-readable output mode  |
|   [5]   | `flint.ctx.default()` / `flint.ctx.cleanup()` | lifecycle      | reset to defaults / cleanup |

## [4]-[IMPLEMENTATION_LAW]

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
