# [PY_COMPUTE_API_PYTHON_FLINT]

`python-flint` (module `flint`) binds the FLINT and Arb C libraries into one exact-and-certified numeric rail: GMP-backed integer and rational arithmetic, word and multiprecision modular and finite-field algebra, univariate and multivariate polynomial factorization and linear algebra, and arbitrary-precision `arb`/`acb` ball arithmetic carrying certified error bounds across the full Arb special-function catalogue. Every ball result is its own error certificate, and exact types round only at the boundary. Feeds the compute exact-arithmetic and ball-arithmetic rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-flint`
- package: `python-flint` (MIT)
- module: `flint`
- owner: `compute`
- asset: Cython/C extension wrapping the FLINT and Arb libraries
- rail: exact-arithmetic, ball-arithmetic
- namespace: `flint`; every type imports directly, and `flint.ctx` is the precision/threading singleton

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exact integer and rational types

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

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]           | [CAPABILITY]                                                                   |
| :-----: | :-------------------- | :---------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `nmod`                | word-sized modular int  | arithmetic mod a machine-word modulus                                          |
|  [02]   | `nmod_mat`            | modular matrix          | dense matrix over `nmod`                                                       |
|  [03]   | `nmod_poly`           | modular polynomial      | univariate polynomial over `nmod`                                              |
|  [04]   | `nmod_series`         | modular series          | power series over `nmod`                                                       |
|  [05]   | `fmpz_mod`            | multiprecision mod int  | arithmetic mod a large-integer modulus                                         |
|  [06]   | `fmpz_mod_ctx`        | modulus context         | context object for `fmpz_mod` operations                                       |
|  [07]   | `fmpz_mod_mat`        | modular matrix          | dense matrix over `fmpz_mod`                                                   |
|  [08]   | `fmpz_mod_poly`       | modular polynomial      | univariate polynomial over `fmpz_mod`                                          |
|  [09]   | `fq_default`          | finite-field element    | element in GF(p^n) with Frobenius/norm                                         |
|  [10]   | `fq_default_ctx`      | finite-field context    | context for GF(p^n) carrying `characteristic`/`degree`/`gen`/`modulus`/`order` |
|  [11]   | `fq_default_poly`     | finite-field polynomial | polynomial over `fq_default`                                                   |
|  [12]   | `fq_default_poly_ctx` | finite-field poly ctx   | polynomial-ring context bound to a `fq_default_ctx`                            |
|  [13]   | `fmpz_mod_poly_ctx`   | modular poly context    | polynomial-ring context bound to a `fmpz_mod_ctx`                              |

[PUBLIC_TYPE_SCOPE]: multivariate polynomial types

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                                             |
| :-----: | :------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `fmpz_mpoly`         | integer mpoly  | multivariate poly over `fmpz`                                            |
|  [02]   | `fmpz_mpoly_ctx`     | mpoly context  | variable count, names, and `Ordering` (lex/deglex/degrevlex) for `fmpz`  |
|  [03]   | `fmpz_mpoly_vec`     | mpoly vector   | vector of `fmpz_mpoly` (ideal generator basis, Gröbner input)            |
|  [04]   | `fmpq_mpoly`         | rational mpoly | multivariate poly over `fmpq`                                            |
|  [05]   | `fmpq_mpoly_ctx`     | mpoly context  | variable count, names, and `Ordering` for `fmpq`                         |
|  [06]   | `fmpq_mpoly_vec`     | mpoly vector   | vector of `fmpq_mpoly`                                                   |
|  [07]   | `nmod_mpoly`         | modular mpoly  | multivariate poly over `nmod`                                            |
|  [08]   | `nmod_mpoly_ctx`     | mpoly context  | context for `nmod_mpoly`                                                 |
|  [09]   | `nmod_mpoly_vec`     | mpoly vector   | vector of `nmod_mpoly`                                                   |
|  [10]   | `fmpz_mod_mpoly`     | modular mpoly  | multivariate poly over `fmpz_mod`                                        |
|  [11]   | `fmpz_mod_mpoly_ctx` | mpoly context  | context for `fmpz_mod_mpoly`                                             |
|  [12]   | `fmpz_mod_mpoly_vec` | mpoly vector   | vector of `fmpz_mod_mpoly`                                               |
|  [13]   | `Ordering`           | monomial order | enum (`lex`/`deglex`/`degrevlex`) selecting the mpoly-context term order |

[PUBLIC_TYPE_SCOPE]: ball arithmetic types

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

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :---------------- | :------------------ | :----------------------------------------- |
|  [01]   | `dirichlet_char`  | Dirichlet character | character evaluation and properties        |
|  [02]   | `dirichlet_group` | character group     | Dirichlet group with character enumeration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `fmpz` exact integer operations

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `fmpz(n)` constructor                                                                         | construction   |
|  [02]   | `is_prime()` / `is_probable_prime()`                                                          | predicate      |
|  [03]   | `factor()` / `factor_smooth()`                                                                | factorization  |
|  [04]   | `gcd(other)` / `lcm(other)`                                                                   | arithmetic     |
|  [05]   | `sqrt()` / `isqrt()` / `root(n)`                                                              | roots          |
|  [06]   | `fac_ui(n)` / `fib_ui(n)` / `bell_number(n)` / `partitions_p(n)` / `primorial_ui(n)`          | combinatorics  |
|  [07]   | `euler_phi(n)` / `moebius_mu(n)` / `jacobi(a, n)` / `divisor_sigma(n, k)` / `euler_number(n)` | number theory  |
|  [08]   | `rising(x, n)` / `stirling_s1(n, k)` / `stirling_s2(n, k)` / `bin_uiui(n, k)`                 | combinatorics  |
|  [09]   | `is_perfect_power()` / `is_square()` / `sqrtmod(p)` / `sqrtrem()`                             | structure      |

[ENTRYPOINT_SCOPE]: `fmpq` exact rational analytic constants
- `fmpq` returns exact closed-form sequence values that feed exact-coefficient solver kernels with no float rounding.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `fmpq.bernoulli(n)` / `fmpq.harmonic(n)`                                | sequences      | exact Bernoulli and harmonic numbers    |
|  [02]   | `fmpq.dedekind_sum(h, k)`                                               | number theory  | exact Dedekind sum                      |
|  [03]   | `numer()`/`denom()`/`floor()`/`ceil()`/`round()`/`next()`/`__float__()` | access         | numer/denom, rounding, next, float cast |

[ENTRYPOINT_SCOPE]: `fmpz_mat` and `fmpq_mat` matrix operations

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :--------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `det()` / `rank()` / `inv()`             | linear algebra | determinant, rank, inverse       |
|  [02]   | `solve(b)` / `rref()` / `nullspace()`    | linear algebra | exact linear solve, RREF         |
|  [03]   | `charpoly()` / `minpoly()`               | polynomial     | characteristic and minimal poly  |
|  [04]   | `hnf()` / `snf()` / `lll()`              | lattice        | Hermite/Smith normal form, LLL   |
|  [05]   | `fflu()` / `hadamard()`                  | algorithms     | fraction-free LU, Hadamard bound |
|  [06]   | `transpose()` / `entries()` / `tolist()` | access         | transpose and data extraction    |

[ENTRYPOINT_SCOPE]: `fmpz_poly` and `fmpq_poly` polynomial operations
- `degree()` returns `-1` for the zero polynomial.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `factor()` / `factor_squarefree()`                        | factorization   | full and squarefree factorization               |
|  [02]   | `gcd(other)` / `resultant(other)` / `discriminant()`      | algebra         | GCD, resultant, discriminant                    |
|  [03]   | `roots()` / `real_roots()` / `complex_roots()`            | root finding    | exact, real, and complex roots                  |
|  [04]   | `derivative()` / `integral()` (fmpq_poly)                 | calculus        | formal derivative and integral                  |
|  [05]   | `cyclotomic(n)` / `chebyshev_t(n)` / `swinnerton_dyer(n)` | named poly      | cyclotomic, Chebyshev, Swinnerton-Dyer          |
|  [06]   | `is_cyclotomic()` / `deflate()` / `inflate(n)`            | structure       | cyclotomic test, deflate/inflate                |
|  [07]   | `mul_low(other, n)` / `pow_trunc(e, n)`                   | truncated-arith | low-degree product, power truncated to degree n |
|  [08]   | `degree()`                                                | access          | highest-term exponent                           |

[ENTRYPOINT_SCOPE]: `arb` and `acb` ball arithmetic operations
- `sinh_cosh()` fuses hyperbolic sine and cosine; `airy_ai_zero(n)` / `airy_bi_zero(n)` return the nth real Airy zeros.
- `acb_mat.theta_jets(tau, z, ord)` returns Riemann theta values together with their partial derivatives (the jet) up to total order `ord`; `theta(tau, z)` is the `ord=0` value alone.

| [INDEX] | [SURFACE]                                                                                                         | [ENTRY_FAMILY]   |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `arb(x)` / `arb(x, r)` constructor                                                                                | construction     |
|  [02]   | `arb.pi()` / `const_e()` / `const_euler()` / `const_catalan()` / `const_glaisher()` / `const_khinchin()`          | constants        |
|  [03]   | `exp()` / `log()` / `log1p()` / `expm1()` / `sin()` / `cos()` / `sin_cos()` / `tan()` / `sqrt()` / `rsqrt()`      | elementary       |
|  [04]   | `root(n)`                                                                                                         | elementary       |
|  [05]   | `gamma()` / `lgamma()` / `rgamma()` / `digamma()` / `gamma_lower()` / `gamma_upper()` / `beta_lower()`            | special-gamma    |
|  [06]   | `zeta()` / `polylog()` / `lerch_phi()` (acb) / `barnes_g()` (acb) / `stieltjes()` (acb)                           | special-zeta     |
|  [07]   | `erf()` / `erfc()` / `erfi()` / `erfinv()` / `erfcinv()` / `fresnel_s()` / `fresnel_c()`                          | special-error    |
|  [08]   | `ei()` / `li()` / `si()` / `ci()` / `shi()` / `chi()` / `expint()`                                                | special-integral |
|  [09]   | `bessel_j(n,z)` / `bessel_y(n,z)` / `bessel_i(n,z)` / `bessel_k(n,z)` / `airy()` / `airy_ai()` / `airy_bi()`      | special-bessel   |
|  [10]   | `coulomb()`                                                                                                       | special-bessel   |
|  [11]   | `hypgeom(a,b,z)` / `hypgeom_0f1(a,z)` / `hypgeom_1f1(a,b,z)` / `hypgeom_2f1(a,b,c,z)` / `hypgeom_u(a,b,z)`        | special-hypgeom  |
|  [12]   | `legendre_p()` / `legendre_q()` / `chebyshev_t()` / `chebyshev_u()` / `hermite_h()` / `laguerre_l()`              | orthogonal-poly  |
|  [13]   | `gegenbauer_c()` / `jacobi_p()`                                                                                   | orthogonal-poly  |
|  [14]   | `lambertw()` / `agm()` / `sinc()` / `sgn()` / `lambertw()`                                                        | special-misc     |
|  [15]   | `contains(other)` / `overlaps(other)` / `union(other)` / `intersection(other)` / `mid()` / `rad()` / `is_exact()` | ball-ops         |
|  [16]   | `is_finite()` / `rel_accuracy_bits()`                                                                             | ball-ops         |
|  [17]   | `acb(re, im)` / `real()` / `imag()` / `arg()` / `conjugate()` / `csgn()`                                          | complex-ops      |
|  [18]   | `acb.elliptic_k()` / `elliptic_e()` / `elliptic_p()` / `elliptic_zeta()` / `elliptic_sigma()`                     | elliptic         |
|  [19]   | `elliptic_rf/rg/rj/rc/rd()`                                                                                       | elliptic         |
|  [20]   | `acb.modular_j()` / `modular_eta()` / `modular_lambda()` / `modular_delta()` / `modular_theta()` / `dedekind_eta` | modular          |
|  [21]   | `acb.dirichlet_l(s, char)` / `acb.zeta_zero(n)` / `acb.zeta_zeros(n, count)` / `arb.zeta_nzeros(t)`               | analytic         |
|  [22]   | `arb_mat.solve(b)` / `arb_mat.inv()` / `arb_mat.det()` / `arb_mat.eig()` / `arb_mat.exp()` / `arb_mat.charpoly()` | real-matrix      |
|  [23]   | `arb_mat.dct()`                                                                                                   | real-matrix      |
|  [24]   | `acb_mat.eig()` / `acb_mat.solve(b)` / `acb_mat.exp()` / `acb_mat.dft()`                                          | complex-matrix   |
|  [25]   | `acb_mat.theta(tau, z)` / `acb_mat.theta_jets(tau, z, ord)`                                                       | complex-matrix   |

[ENTRYPOINT_SCOPE]: context and precision control

| [INDEX] | [SURFACE]                                                                                             | [ENTRY_FAMILY] |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `flint.ctx.prec` (get/set)                                                                            | precision      |
|  [02]   | `flint.ctx.dps` (get/set)                                                                             | precision      |
|  [03]   | `flint.ctx.threads` (get/set)                                                                         | threading      |
|  [04]   | `flint.ctx.pretty` (get/set)                                                                          | display        |
|  [05]   | `flint.ctx.unicode` (get/set)                                                                         | display        |
|  [06]   | `flint.ctx.cap` (get/set)                                                                             | series         |
|  [07]   | `flint.ctx.extraprec(n)` / `flint.ctx.extradps(n)` / `flint.ctx.workprec(n)` / `flint.ctx.workdps(n)` | precision      |
|  [08]   | `flint.ctx.default()` / `flint.ctx.cleanup()`                                                         | lifecycle      |

[ENTRYPOINT_SCOPE]: power-series (truncated analytic) operations
- `arb_series`/`acb_series` carry the scalar ball special-function catalogue evaluated as truncated power series at `ctx.cap`; `fmpz_series`/`fmpq_series`/`nmod_series` are the exact and modular truncated series. These own certified series composition, reversion, and root finding no hand-rolled Taylor truncation certifies.

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY]  |
| :-----: | :--------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `arb_series.exp()` / `log()` / `sin_cos()` / `gamma()` / `zeta()` / `erf()` / `bessel`/`airy` family | series-special  |
|  [02]   | `arb_series.derivative()` / `integral()` / `inv()` / `reversion()`                                   | series-calculus |
|  [03]   | `arb_series.find_roots()`                                                                            | series-roots    |
|  [04]   | `acb_series.dirichlet_l()` / `polylog()` / `modular_theta()` / `elliptic_p()`                        | series-analytic |
|  [05]   | `coeffs()` / `length()` / `valuation()` / `prec`                                                     | series-access   |

[ENTRYPOINT_SCOPE]: certified evaluation dispatch and analytic helpers
- Top-level helpers adaptively raise working precision until a target accuracy holds, decoupling the consumer from manual `ctx.prec` retry loops.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `flint.good(func, prec=, maxprec=)`                                        | adaptive-eval    | rising-precision certified re-evaluation |
|  [02]   | `flint.showgood(func, dps=, maxprec=)`                                     | adaptive-display | `good` plus certified-digit printing     |
|  [03]   | `dirichlet_char.l(s)` / `dirichlet_char.hardy_z(t)` / `dirichlet_group(q)` | analytic         | Dirichlet L, Hardy Z, group enumeration  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `fmpz` and `fmpq` carry the full Python arithmetic operators; results are exact with no rounding.
- `arb` and `acb` carry an interval `[mid ± rad]`; every operation propagates the radius, the result is certified while `rad` stays finite (`is_finite()`), and `rel_accuracy_bits()` reports the pinned significant bits. Only an exact ball hashes; an inexact `arb`/`acb` raises `ValueError` on `hash()`, barring it as a dict key or set member.
- `factor()` / `roots()` and the mpoly factorizations return terms in a deterministic sorted order, so a factor list is comparable across runs without a caller-side sort.
- `ctx.prec` sets session bit-precision for all `arb`/`acb` work and `ctx.cap` the default series truncation order; `ctx.extraprec(n)`/`ctx.workprec(n)` are block-scoped managers that bump precision without leaking to the session.
- `flint.good(func)` re-runs `func` at escalating `ctx.prec` until the ball is accurate to `ctx.dps`, replacing a hand-written precision-retry loop.
- Matrix constructors accept nested lists or flat data with shape; `eig()` on `arb_mat`/`acb_mat` returns certified eigenvalue balls, never float estimates.
- Modular and finite-field types construct only from an explicit context (`fmpz_mod_ctx`, `fq_default_ctx`, `*_mpoly_ctx`, `*_poly_ctx`) carrying the modulus, field parameters, or monomial `Ordering`.

[STACKING]:
- `mpmath`(`.api/mpmath.md`): `mpmath` is the unrigorous fast floor; re-evaluate its closed form through `arb`/`acb` under `flint.good` to promote a heuristic special-function value to a certified bound, `rad()` the error the `mpmath` value lacks.
- `sympy`(`.api/sympy.md`): lower a symbolic polynomial or matrix to `fmpz_poly`/`fmpq_poly`/`fmpz_mat` for the FLINT exact kernels (factorization, `resultant`, `roots`, HNF/SNF/LLL, `charpoly`), then lift the exact result back; `fmpz_mpoly_vec` and the mpoly context own Gröbner and lattice work.
- `uncertainties`(`.api/uncertainties.md`): dual error models — `uncertainties` for first-order Gaussian propagation, `arb` for worst-case certified enclosure; `arb` owns any requirement for a guaranteed enclosure over a `1σ` estimate.
- within-lib: an `arb`/`acb` result is its own receipt — `mid()` the value, `rad()` the certified error, `rel_accuracy_bits()` the precision evidence; the algorithm receipt captures `ctx.prec`/`ctx.dps` and the final `rad()` over an external tolerance.

[LOCAL_ADMISSION]:
- Integer and rational constants in solver kernels stay `fmpz`/`fmpq`, converting to float only at the boundary.
- Ball precision sets once at session scope via `flint.ctx.prec` before any `arb`/`acb` computation.
- Polynomial factorization and root isolation route through `fmpz_poly.roots()` or `arb_poly.real_roots()` for certified intervals.
- An `arb` result carries its own error bound; external tolerance tracking never rides alongside `rad`.

[RAIL_LAW]:
- Package: `python-flint`
- Owns: exact integer/rational/modular/finite-field arithmetic, univariate and multivariate polynomial algebra (factorization, GCD, resultant, root isolation, `*_mpoly_vec` Gröbner input), exact matrix linear algebra (det/rank/inv/solve/rref/nullspace/charpoly/minpoly, HNF/SNF/LLL/fflu), certified `arb`/`acb` ball arithmetic across the full Arb special-function catalogue, certified ball-matrix eigenvalues/solve/exp/DFT/DCT, rigorous truncated power series with reversion and root finding, adaptive-precision `flint.good`/`showgood`, and Dirichlet character/L-function evaluation
- Accept: `fmpz`/`fmpq` for exact integer/rational results; `nmod`/`fmpz_mod`/`fq_default` with their context for modular and finite-field results; `arb`/`acb` for certified results carrying `mid`/`rad`; `flint.good` for adaptive-precision evaluation; the mpoly context with `*_mpoly_vec` for exact multivariate algebra
- Reject: float/complex conversion before the boundary where exact or certified results are required; a hand-rolled precision-retry loop where `flint.good` adapts; heuristic mpmath special functions where a certified bound is required; symbolic re-derivation of factorization, resultant, or lattice reduction the FLINT exact kernels own
