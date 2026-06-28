# [PY_COMPUTE_API_PYTHON_FLINT]

`python-flint` (module `flint`) wraps the FLINT and Arb C libraries, supplying exact integer/rational arithmetic (`fmpz`, `fmpq`), modular arithmetic (`nmod`, `fmpz_mod`, `fq_default`), multivariate polynomials, and arbitrary-precision ball arithmetic (`arb`, `acb`) with certified error bounds for the compute exact-arithmetic and special-function rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-flint`
- package: `python-flint`
- module: `flint`
- owner: `compute`
- asset: runtime library (Cython/C extension wrapping FLINT + Arb)
- rail: exact-arithmetic, ball-arithmetic
- namespace: `flint` (all types/functions imported directly from `flint`; `flint.ctx` is the precision/threading singleton)
- installed: `0.8.0`
- capability: GMP-backed exact integer/rational arithmetic, word/multiprecision modular and finite-field arithmetic, univariate and multivariate polynomial algebra (factorization, GCD, resultant, root isolation), exact matrix linear algebra with HNF/SNF/LLL, certified real/complex ball arithmetic with the full Arb special-function catalogue (gamma/zeta/Bessel/hypergeometric/elliptic/modular), certified eigenvalues/solve/DFT on ball matrices, rigorous power-series arithmetic, and Dirichlet character/L-function evaluation

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
|  [09]   | `fq_default`          | finite-field element    | element in GF(p^n) with Frobenius/norm                |
|  [10]   | `fq_default_ctx`      | finite-field context    | context for GF(p^n) carrying `characteristic`/`degree`/`gen`/`modulus`/`order` |
|  [11]   | `fq_default_poly`     | finite-field polynomial | polynomial over `fq_default`                          |
|  [12]   | `fq_default_poly_ctx` | finite-field poly ctx   | polynomial-ring context bound to a `fq_default_ctx`   |
|  [13]   | `fmpz_mod_poly_ctx`   | modular poly context    | polynomial-ring context bound to a `fmpz_mod_ctx`     |

[PUBLIC_TYPE_SCOPE]: multivariate polynomial types
- rail: exact-arithmetic

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :--------------- | :------------- | :------------------------------------- |
|  [01]   | `fmpz_mpoly`         | integer mpoly  | multivariate poly over `fmpz`                                          |
|  [02]   | `fmpz_mpoly_ctx`     | mpoly context  | variable count, names, and `Ordering` (lex/deglex/degrevlex) for `fmpz` |
|  [03]   | `fmpz_mpoly_vec`     | mpoly vector   | vector of `fmpz_mpoly` (ideal generator basis, Gröbner input)          |
|  [04]   | `fmpq_mpoly`         | rational mpoly | multivariate poly over `fmpq`                                          |
|  [05]   | `fmpq_mpoly_ctx`     | mpoly context  | variable count, names, and `Ordering` for `fmpq`                       |
|  [06]   | `fmpq_mpoly_vec`     | mpoly vector   | vector of `fmpq_mpoly`                                                  |
|  [07]   | `nmod_mpoly`         | modular mpoly  | multivariate poly over `nmod`                                          |
|  [08]   | `nmod_mpoly_ctx`     | mpoly context  | context for `nmod_mpoly`                                               |
|  [09]   | `nmod_mpoly_vec`     | mpoly vector   | vector of `nmod_mpoly`                                                  |
|  [10]   | `fmpz_mod_mpoly`     | modular mpoly  | multivariate poly over `fmpz_mod`                                      |
|  [11]   | `fmpz_mod_mpoly_ctx` | mpoly context  | context for `fmpz_mod_mpoly`                                           |
|  [12]   | `fmpz_mod_mpoly_vec` | mpoly vector   | vector of `fmpz_mod_mpoly`                                             |
|  [13]   | `Ordering`           | monomial order | enum (`lex`/`deglex`/`degrevlex`) selecting the mpoly-context term order |

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
|  [06]   | `fac_ui(n)` / `fib_ui(n)` / `bell_number(n)` / `partitions_p(n)` / `primorial_ui(n)` | combinatorics  | factorial, Fibonacci, Bell, partition count, primorial |
|  [07]   | `euler_phi(n)` / `moebius_mu(n)` / `jacobi(a, n)` / `divisor_sigma(n, k)` / `euler_number(n)` | number theory  | Euler totient, Möbius, Jacobi, divisor sum, Euler number |
|  [08]   | `rising(x, n)` / `stirling_s1(n, k)` / `stirling_s2(n, k)` / `bin_uiui(n, k)` | combinatorics  | rising factorial, Stirling numbers, binomial coefficient |
|  [09]   | `is_perfect_power()` / `is_square()` / `sqrtmod(p)` / `sqrtrem()` | structure      | perfect-power/square tests, modular and remainder roots |

[ENTRYPOINT_SCOPE]: `fmpq` exact rational analytic constants
- rail: exact-arithmetic
- `fmpq` exposes exact closed-form sequence values returned as exact rationals; these feed exact-coefficient solver kernels without float rounding.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `fmpq.bernoulli(n)` / `fmpq.harmonic(n)`        | sequences      | exact Bernoulli and harmonic numbers         |
|  [02]   | `fmpq.dedekind_sum(h, k)`                       | number theory  | exact Dedekind sum                           |
|  [03]   | `numer()` / `denom()` / `floor()` / `ceil()` / `round()` / `next()` | access | exact numerator/denominator, rounding, Stern-Brocot successor |

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

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY]   | [RAIL]                                                       |
| :-----: | :------------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `arb(x)` / `arb(x, r)` constructor                                                     | construction     | midpoint or midpoint+radius ball                             |
|  [02]   | `arb.pi()` / `const_e()` / `const_euler()` / `const_catalan()` / `const_glaisher()` / `const_khinchin()` | constants | certified mathematical constants (static methods on `arb`) |
|  [03]   | `exp()` / `log()` / `log1p()` / `expm1()` / `sin()` / `cos()` / `sin_cos()` / `tan()` / `sqrt()` / `rsqrt()` / `root(n)` | elementary | certified elementary functions and fused `sin_cos`/`sinh_cosh` |
|  [04]   | `gamma()` / `lgamma()` / `rgamma()` / `digamma()` / `gamma_lower()` / `gamma_upper()` / `beta_lower()` | special-gamma | gamma family and incomplete gamma/beta with certified bounds |
|  [05]   | `zeta()` / `polylog()` / `lerch_phi()` (acb) / `barnes_g()` (acb) / `stieltjes()` (acb) | special-zeta     | Riemann/Hurwitz zeta, polylog, Lerch transcendent, Barnes G  |
|  [06]   | `erf()` / `erfc()` / `erfi()` / `erfinv()` / `erfcinv()` / `fresnel_s()` / `fresnel_c()` | special-error   | error functions, inverse error, Fresnel integrals            |
|  [07]   | `ei()` / `li()` / `si()` / `ci()` / `shi()` / `chi()` / `expint()`                     | special-integral | exponential/logarithmic/trig/hyperbolic integrals            |
|  [08]   | `bessel_j(n,z)` / `bessel_y(n,z)` / `bessel_i(n,z)` / `bessel_k(n,z)` / `airy()` / `airy_ai()` / `airy_bi()` / `coulomb()` | special-bessel | Bessel, Airy (with `airy_ai_zero`/`airy_bi_zero`), Coulomb wave |
|  [09]   | `hypgeom(a,b,z)` / `hypgeom_0f1(a,z)` / `hypgeom_1f1(a,b,z)` / `hypgeom_2f1(a,b,c,z)` / `hypgeom_u(a,b,z)` | special-hypgeom | confluent/Gauss/Tricomi hypergeometric                       |
|  [10]   | `legendre_p()` / `legendre_q()` / `chebyshev_t()` / `chebyshev_u()` / `hermite_h()` / `laguerre_l()` / `gegenbauer_c()` / `jacobi_p()` | orthogonal-poly | orthogonal-polynomial evaluation at a ball argument          |
|  [11]   | `lambertw()` / `agm()` / `sinc()` / `sgn()` / `lambertw()`                             | special-misc     | Lambert W, arithmetic-geometric mean, sinc                   |
|  [12]   | `contains(other)` / `overlaps(other)` / `union(other)` / `intersection(other)` / `mid()` / `rad()` / `is_exact()` / `is_finite()` / `rel_accuracy_bits()` | ball-ops | containment, hull, intersection, ball metadata, accuracy bits |
|  [13]   | `acb(re, im)` / `real()` / `imag()` / `arg()` / `conjugate()` / `csgn()`               | complex-ops      | complex ball construction, parts, argument, sign             |
|  [14]   | `acb.elliptic_k()` / `elliptic_e()` / `elliptic_p()` / `elliptic_zeta()` / `elliptic_sigma()` / `elliptic_rf/rg/rj/rc/rd()` | elliptic | complete/incomplete elliptic integrals and Weierstrass functions |
|  [15]   | `acb.modular_j()` / `modular_eta()` / `modular_lambda()` / `modular_delta()` / `modular_theta()` / `dedekind_eta` | modular | modular forms and Jacobi theta on the complex ball           |
|  [16]   | `acb.dirichlet_l(s, char)` / `acb.zeta_zero(n)` / `acb.zeta_zeros(n, count)` / `arb.zeta_nzeros(t)` | analytic | Dirichlet L-function, individual and batched Riemann zeta zeros |
|  [17]   | `arb_mat.solve(b)` / `arb_mat.inv()` / `arb_mat.det()` / `arb_mat.eig()` / `arb_mat.exp()` / `arb_mat.charpoly()` / `arb_mat.dct()` | real-matrix | certified solve, inverse, eigenvalues, matrix exponential, DCT |
|  [18]   | `acb_mat.eig()` / `acb_mat.solve(b)` / `acb_mat.exp()` / `acb_mat.dft()` / `acb_mat.theta()` | complex-matrix   | certified complex eigenvalues, matrix DFT, Riemann theta on a matrix |

[ENTRYPOINT_SCOPE]: context and precision control
- rail: ball-arithmetic

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `flint.ctx.prec` (get/set)                    | precision      | working bit-precision       |
|  [02]   | `flint.ctx.dps` (get/set)                     | precision      | decimal digits of precision |
|  [03]   | `flint.ctx.threads` (get/set)                 | threading      | FLINT thread count          |
|  [04]   | `flint.ctx.pretty` (get/set)                  | display        | human-readable output mode  |
|  [05]   | `flint.ctx.unicode` (get/set)                 | display        | Unicode glyphs in `str()` output |
|  [06]   | `flint.ctx.cap` (get/set)                     | series         | default truncation order (`prec`) for power-series types |
|  [07]   | `flint.ctx.extraprec(n)` / `flint.ctx.extradps(n)` / `flint.ctx.workprec(n)` / `flint.ctx.workdps(n)` | precision | context-manager scopes that bump bit/decimal precision for a block |
|  [08]   | `flint.ctx.default()` / `flint.ctx.cleanup()` | lifecycle      | reset to defaults / FLINT thread-local cleanup |

[ENTRYPOINT_SCOPE]: power-series (truncated analytic) operations
- rail: ball-arithmetic
- `arb_series` / `acb_series` carry the same special-function catalogue as the scalar ball types but evaluated as truncated power series at precision `ctx.cap`; `fmpz_series`/`fmpq_series`/`nmod_series` are the exact/modular truncated series. These own rigorous series composition, reversion, and root finding that a hand-rolled Taylor truncation cannot certify.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `arb_series.exp()` / `log()` / `sin_cos()` / `gamma()` / `zeta()` / `erf()` / `bessel`/`airy` family | series-special | certified truncated power-series special functions |
|  [02]   | `arb_series.derivative()` / `integral()` / `inv()` / `reversion()`  | series-calculus | formal derivative/integral, series inverse, compositional reversion |
|  [03]   | `arb_series.find_roots()`                                           | series-roots   | rigorous root isolation from a truncated series         |
|  [04]   | `acb_series.dirichlet_l()` / `polylog()` / `modular_theta()` / `elliptic_p()` | series-analytic | analytic functions as truncated complex power series |
|  [05]   | `coeffs()` / `length()` / `valuation()` / `prec`                    | series-access  | coefficient list, length, valuation, truncation order   |

[ENTRYPOINT_SCOPE]: certified evaluation dispatch and analytic helpers
- rail: ball-arithmetic
- top-level helpers that adaptively raise working precision until a target accuracy is reached, decoupling the consumer from manual `ctx.prec` retry loops.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                                                 |
| :-----: | :--------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `flint.good(func, prec=, maxprec=)` | adaptive-eval  | re-evaluate `func` at rising precision until the result ball is accurate to `ctx.dps`, returning the certified value |
|  [02]   | `flint.showgood(func, dps=, maxprec=)` | adaptive-display | `good` plus pretty-printing of the certified digits                    |
|  [03]   | `dirichlet_char.l(s)` / `dirichlet_char.hardy_z(t)` / `dirichlet_group(q)` | analytic | Dirichlet L-function and Hardy Z-function from a character, character-group enumeration |

## [04]-[IMPLEMENTATION_LAW]

[ARITHMETIC_TOPOLOGY]:
- namespace: `flint`; all types imported directly from `flint`
- `fmpz` and `fmpq` support full Python arithmetic operators; results are exact with no rounding
- `arb` and `acb` carry an interval `[mid ± rad]`; all operations propagate the radius; output is certified when `rad` is finite (`is_finite()`); `rel_accuracy_bits()` reports how many significant bits the ball actually pins down
- `ctx.prec` sets the working precision in bits for all `arb`/`acb` computation in the session; `ctx.cap` sets the default power-series truncation order; `ctx.extraprec(n)`/`ctx.workprec(n)` are block-scoped context managers that bump precision without leaking the change to the session
- `flint.good(func)` is the adaptive-precision driver: it re-runs `func` at escalating `ctx.prec` until the ball is accurate to `ctx.dps`, replacing a hand-written precision-retry loop around an `arb`/`acb` computation
- matrix constructors accept nested lists or flat data with shape; `nrows()`/`ncols()` query dimensions; `eig()` on `arb_mat`/`acb_mat` returns certified eigenvalue balls, never floating-point estimates
- modular and finite-field types require an explicit context object (`fmpz_mod_ctx`, `fq_default_ctx`, `*_mpoly_ctx`, `*_poly_ctx`) that carries the modulus, field parameters, or monomial `Ordering`; the element type is constructed only from its context

[ARITHMETIC_STACKING]:
- mpmath ↔ flint: `mpmath` is the unrigorous arbitrary-precision floor (fast heuristic special functions, no error bound); `python-flint` is the rigorous ceiling (certified `arb`/`acb` balls). Promote an mpmath special-function result to a certified bound by re-evaluating the same closed form through `arb`/`acb` under `flint.good`; the ball `rad()` is the certified error the heuristic mpmath value lacks.
- sympy ↔ flint: lower a symbolic `sympy` polynomial/matrix to an exact `fmpz_poly`/`fmpq_poly`/`fmpz_mat` for the heavy exact kernels FLINT owns (factorization, `resultant`, `roots`, HNF/SNF/LLL, `charpoly`), then lift the exact result back; never re-implement Gröbner/lattice reduction symbolically when `fmpz_mpoly_vec` + the FLINT mpoly context own it.
- uncertainties ↔ flint: the `uncertainties` linear error-propagation owner and the `arb` interval owner are dual error models — `uncertainties` for first-order Gaussian propagation, `arb` for worst-case certified enclosure; the certified-bounds rail picks `arb` when a guaranteed enclosure (not a 1σ estimate) is the requirement.
- receipt rail: an `arb`/`acb` result is its own receipt — `mid()` is the value, `rad()` is the certified error bound, `rel_accuracy_bits()` is the precision evidence; the algorithm receipt captures `ctx.prec`/`ctx.dps` and the final `rad()` rather than re-deriving an external tolerance.

[LOCAL_ADMISSION]:
- Exact arithmetic uses `fmpz`/`fmpq` for integer and rational constants in solver kernels; never converts to float until the boundary.
- Ball arithmetic precision is set once at session scope via `flint.ctx.prec` before any `arb`/`acb` computation.
- Polynomial factorization and root isolation use `fmpz_poly.roots()` or `arb_poly.real_roots()` to produce certified intervals.
- `arb` results carry their own error bound; do not add external tolerance tracking when `rad` is already the certified bound.

[RAIL_LAW]:
- Package: `python-flint`
- Owns: exact integer/rational/word-modular/multiprecision-modular/finite-field arithmetic, univariate and multivariate polynomial algebra (factorization, GCD, resultant, root isolation, Gröbner-input mpoly vectors), exact matrix linear algebra (det/rank/inv/solve/rref/nullspace/charpoly/minpoly + HNF/SNF/LLL/fflu), certified real/complex ball arithmetic with the full Arb special-function catalogue (gamma/zeta/Bessel/Airy/Coulomb/hypergeometric/orthogonal-polynomial/elliptic/modular), certified ball-matrix eigenvalues/solve/exp/DFT/DCT, rigorous truncated power series (`arb_series`/`acb_series` with reversion and root finding), adaptive-precision evaluation (`flint.good`/`showgood`), and Dirichlet character/L-function evaluation
- Accept: `fmpz`/`fmpq` for exact integer/rational results; `nmod`/`fmpz_mod`/`fq_default` (each with its context) for modular/finite-field results; `arb`/`acb` for certified floating-point results carrying `mid`/`rad`; `flint.good` for adaptive-precision certified evaluation; the mpoly context + `*_mpoly_vec` for exact multivariate algebra
- Reject: `float`/`complex` conversions before the boundary when exact or certified results are required; a hand-rolled precision-retry loop where `flint.good` adapts; heuristic mpmath special functions where a certified `arb`/`acb` bound is required; symbolic re-derivation of factorization/resultant/lattice reduction that the FLINT exact kernels own
