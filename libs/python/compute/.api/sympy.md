# [PY_COMPUTE_API_SYMPY]

`sympy` supplies the symbolic algebra surface for the compute symbolic-derivation rail: expression construction, calculus, equation solving, simplification, matrix algebra, exact-to-numeric evaluation, geometry primitives, and the multi-language code-generation path that backs the C# graduation receipt. The package owner composes these surfaces into the `SymbolicDerivation` owner and the geometry-handoff graduation case; it never re-implements an algebra primitive sympy already owns. The rail STACKS its siblings: `mpmath` is sympy's own numeric backend — `evalf`/`N` lift an exact expression to arbitrary `dps` precision through the same `mpmath` context the exact-arithmetic rail uses as a precision oracle; `lambdify(..., modules='numpy')` compiles an expression to a `numpy`-vectorized callable (and `modules='jax'` to a `jax`-traceable one) so a symbolic derivation feeds the numeric study rail directly; and `ccode`/`codegen` emit C99 source that becomes the `Rasm.Compute` C# numeric-owner graduation candidate, with `cse` factoring shared subexpressions before emission.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sympy`
- package: `sympy`
- version: `1.14.0`
- license: BSD-3-Clause
- import: `sympy` (lint alias `sym`)
- owner: `compute`
- rail: symbolic
- asset: pure Python; `mpmath` is the bundled numeric backend (the `evalf`/`N` precision engine); `gmpy2` accelerates the ground-domain integer arithmetic when present
- capability: pure-Python computer algebra system; exact symbolic computation, calculus, solving, simplification, polynomial/matrix algebra, set/assumption logic, geometry, arbitrary-precision numeric evaluation, and multi-language code generation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core expression vocabulary
- rail: symbolic

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]     | [CAPABILITY]                                                            |
| :-----: | :---------------------------- | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `Basic`                       | expression root    | base of every symbolic object                                           |
|  [02]   | `Expr`                        | algebraic root     | base of arithmetic-bearing expressions                                  |
|  [03]   | `Symbol`                      | free variable      | named symbolic variable                                                 |
|  [04]   | `Dummy`                       | unique variable    | collision-free temporary symbol                                         |
|  [05]   | `Wild`                        | pattern variable   | match-and-replace placeholder                                           |
|  [06]   | `Function`                    | function head      | declarable/undeclared function constructor                              |
|  [07]   | `Lambda`                      | anonymous map      | symbolic callable expression                                            |
|  [08]   | `Number`                      | numeric root       | base of exact numeric atoms                                             |
|  [09]   | `Integer`                     | exact integer      | arbitrary-precision integer atom                                        |
|  [10]   | `Rational`                    | exact rational     | exact fraction atom                                                     |
|  [11]   | `Float`                       | mpmath float       | arbitrary-precision float atom                                          |
|  [12]   | `S`                           | singleton registry | `S.Zero`, `S.One`, `S.Half`, `S.Infinity`, `S.NaN`, `S.ComplexInfinity` |
|  [13]   | `Eq` `Ne` `Lt` `Le` `Gt` `Ge` | relational atoms   | symbolic equation and inequality nodes                                  |

[PUBLIC_TYPE_SCOPE]: calculus and aggregate nodes
- rail: symbolic

| [INDEX] | [SYMBOL]     | [PACKAGE_ROLE]         | [CAPABILITY]                    |
| :-----: | :----------- | :--------------------- | :------------------------------ |
|  [01]   | `Derivative` | unevaluated derivative | symbolic differentiation node   |
|  [02]   | `Integral`   | unevaluated integral   | symbolic integration node       |
|  [03]   | `Sum`        | unevaluated sum        | symbolic summation node         |
|  [04]   | `Product`    | unevaluated product    | symbolic product node           |
|  [05]   | `Order`      | asymptotic term        | big-O remainder term for series |

[PUBLIC_TYPE_SCOPE]: polynomial, matrix, and geometry roots
- rail: symbolic
- Geometry roots live under `sympy.geometry.*`; `Poly` and the matrix roots are top-level `sympy`.

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]      | [CAPABILITY]                                               |
| :-----: | :------------------------------------ | :------------------ | :--------------------------------------------------------- |
|  [01]   | `Poly`                                | polynomial owner    | dense polynomial with `all_coeffs`, `degree`, roots access |
|  [02]   | `Matrix`                              | mutable matrix      | dense symbolic matrix                                      |
|  [03]   | `ImmutableMatrix`                     | hashable matrix     | immutable dense matrix for expression embedding            |
|  [04]   | `MatrixSymbol`                        | symbolic matrix     | named matrix variable                                      |
|  [05]   | `Point2D` `Point3D`                   | geometry atom       | exact coordinate point, `Point3D(*args, _nocheck=False)`   |
|  [06]   | `Line` `Line3D` `Segment` `Ray`       | geometry curve      | exact line/segment/ray                                     |
|  [07]   | `Circle` `Ellipse`                    | geometry conic      | exact conic section                                        |
|  [08]   | `Plane`                               | geometry surface    | exact plane, ctor `Plane(p1, a=None, b=None)`              |
|  [09]   | `Polygon` `Triangle` `RegularPolygon` | geometry region     | exact polygon/triangle/regular polygon                     |
|  [10]   | `Curve` `Parabola`                    | geometry parametric | parametric curve over a range; parabola conic              |

[PUBLIC_TYPE_SCOPE]: set, logic, and matrix-expression roots
- rail: symbolic
- `Identity`, `ZeroMatrix`, `BlockMatrix`, `HadamardProduct`, and `KroneckerProduct` are the unevaluated matrix-expression nodes over `MatrixSymbol`.

| [INDEX] | [SYMBOL]                                            | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Interval` `FiniteSet` `ConditionSet` `ImageSet`    | set atom       | the set-valued domains `solveset` returns               |
|  [02]   | `Union` `Intersection` `Complement`                 | set algebra    | combinators over sets                                   |
|  [03]   | `Reals` `Complexes` `Integers` `Naturals` `S.Reals` | named domain   | the `domain=` argument to `solveset`                    |
|  [04]   | `And` `Or` `Not` `Q` `Piecewise` `Rel`              | logic node     | boolean algebra, `Q` assumption predicates, branch expr |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion
- rail: symbolic

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                   | [CAPABILITY]                                                         |
| :-----: | :------------- | :--------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `symbols`      | `symbols('x y z', real=True)`                  | batch symbol construction with assumptions                           |
|  [02]   | `sympify`      | `sympify(obj)`                                 | coerces Python/str to a sympy expression                             |
|  [03]   | `Symbol`       | `Symbol('x', positive=True)`                   | single symbol with assumption keywords                               |
|  [04]   | `Function`     | `Function('f')`                                | undeclared function head                                             |
|  [05]   | `Expr.subs`    | `expr.subs({x: a, y: b})`                      | simultaneous structural substitution                                 |
|  [06]   | `Expr.replace` | `expr.replace(Wild('w'), f)`                   | `Wild`-pattern match-and-rewrite                                     |
|  [07]   | `Expr.rewrite` | `expr.rewrite(exp)`                            | rewrite into an equivalent functional basis                          |
|  [08]   | `srepr`        | `srepr(expr, *, order=None, perm_cyclic=True)` | round-trip repr, `eval(srepr(expr)) == expr`; content-keys an `Expr` |

[ENTRYPOINT_SCOPE]: calculus operations
- rail: symbolic

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                                                                | [CAPABILITY]                 |
| :-----: | :---------- | :------------------------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `diff`      | `diff(f, *symbols, **kwargs)`                                                               | n-th order derivative        |
|  [02]   | `integrate` | `integrate(*args, meijerg=None, conds='piecewise', risch=None, heurisch=None, manual=None)` | definite/indefinite integral |
|  [03]   | `limit`     | `limit(e, z, z0, dir='+')`                                                                  | directional limit            |
|  [04]   | `series`    | `series(expr, x=None, x0=0, n=6, dir='+')`                                                  | Taylor/Laurent expansion     |
|  [05]   | `summation` | `summation(f, (i, a, b))`                                                                   | closed-form summation        |

[ENTRYPOINT_SCOPE]: equation and system solving
- rail: symbolic

| [INDEX] | [SURFACE]     | [CALL_SHAPE]                                      | [CAPABILITY]                                       |
| :-----: | :------------ | :------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `solve`       | `solve(f, *symbols, **flags)`                     | general algebraic solver, legacy heuristic surface |
|  [02]   | `solveset`    | `solveset(f, symbol=None, domain=Complexes)`      | set-valued solver over a declared domain           |
|  [03]   | `linsolve`    | `linsolve(system, *symbols)`                      | linear-system solver returning a solution set      |
|  [04]   | `nonlinsolve` | `nonlinsolve(system, *symbols)`                   | nonlinear-system set solver                        |
|  [05]   | `nsolve`      | `nsolve(*args, dict=False)`                       | numeric root-find from a symbolic system           |
|  [06]   | `dsolve`      | `dsolve(eq, func=None, hint='default', ics=None)` | ODE solver                                         |
|  [07]   | `pdsolve`     | `pdsolve(eq, func=None)`                          | PDE solver                                         |
|  [08]   | `roots`       | `roots(poly)`                                     | polynomial-root multiset                           |

[ENTRYPOINT_SCOPE]: simplification and rewriting
- rail: symbolic

| [INDEX] | [SURFACE]    | [CALL_SHAPE]                | [CAPABILITY]                   |
| :-----: | :----------- | :-------------------------- | :----------------------------- |
|  [01]   | `simplify`   | `simplify(expr, **options)` | heuristic canonicalization     |
|  [02]   | `expand`     | `expand(expr)`              | product and power distribution |
|  [03]   | `factor`     | `factor(expr)`              | polynomial factorization       |
|  [04]   | `collect`    | `collect(expr, syms)`       | symbolic term grouping         |
|  [05]   | `cancel`     | `cancel(expr)`              | rational reduction             |
|  [06]   | `apart`      | `apart(expr)`               | partial-fraction split         |
|  [07]   | `together`   | `together(expr)`            | common-denominator join        |
|  [08]   | `trigsimp`   | `trigsimp(expr)`            | trigonometric rewriting        |
|  [09]   | `radsimp`    | `radsimp(expr)`             | radical rewriting              |
|  [10]   | `ratsimp`    | `ratsimp(expr)`             | rational rewriting             |
|  [11]   | `powsimp`    | `powsimp(expr)`             | power rewriting                |
|  [12]   | `logcombine` | `logcombine(expr)`          | logarithm rewriting            |
|  [13]   | `nsimplify`  | `nsimplify(expr)`           | float-to-exact reconstruction  |

[ENTRYPOINT_SCOPE]: matrix algebra
- rail: symbolic
- Rows [03]-[10] are `Matrix` instance methods, called `m.<method>(...)`; `Matrix` and `eye`/`zeros`/`ones`/`diag` build the matrix.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                 | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------- | :--------------------------- | :-------------------------------------------------- |
|  [01]   | `Matrix`                                       | `Matrix(rows, cols, data)`   | dense matrix construction                           |
|  [02]   | `eye` `zeros` `ones` `diag`                    | `eye(n)`                     | standard matrix builders                            |
|  [03]   | `det`                                          | `m.det()`                    | exact determinant                                   |
|  [04]   | `inv`                                          | `m.inv()`                    | exact inverse                                       |
|  [05]   | `eigenvals` `eigenvects`                       | `m.eigenvals()`              | exact spectrum (value->multiplicity / value->basis) |
|  [06]   | `rref` `charpoly` `rank`                       | `m.rref()` / `m.charpoly(x)` | RREF, characteristic polynomial, exact rank         |
|  [07]   | `nullspace` `columnspace`                      | `m.nullspace()`              | subspace bases                                      |
|  [08]   | `LUdecomposition` `QRdecomposition` `cholesky` | `m.LUdecomposition()`        | exact factorizations                                |
|  [09]   | `diagonalize` `jordan_form` `singular_values`  | `m.diagonalize()`            | canonical-form decompositions                       |
|  [10]   | `exp` `pinv` `norm`                            | `m.exp()`                    | matrix exponential, pseudo-inverse, exact norm      |

[ENTRYPOINT_SCOPE]: assumption logic, sets, and number theory
- rail: symbolic
- `resultant(p, p.diff(x))` is the squarefree-discriminant metric `|res(p, p')|`.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                               | [CAPABILITY]                                 |
| :-----: | :---------------------------------- | :----------------------------------------- | :------------------------------------------- |
|  [01]   | `refine`                            | `refine(expr, assumptions)`                | simplify under a `Q` assumption context      |
|  [02]   | `ask`                               | `ask(Q.positive(x), Q.real(x))`            | query a predicate against an assumption set  |
|  [03]   | `Q`                                 | `Q.real(x)` / `Q.integer(x)`               | assumption predicate constructors            |
|  [04]   | `Intersection` `Union` `Complement` | `Intersection(a, b)`                       | symbolic set algebra over `solveset` results |
|  [05]   | `factorint` `primerange` `isprime`  | `factorint(n)`                             | integer factorization, prime generation/test |
|  [06]   | `gcd` `lcm`                         | `gcd(a, b)`                                | polynomial/integer gcd and lcm               |
|  [07]   | `resultant`                         | `resultant(f, g, *gens, includePRS=False)` | resultant of two polynomials (elimination)   |

[ENTRYPOINT_SCOPE]: numeric evaluation
- rail: symbolic

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]     | [CAPABILITY]                                                                   |
| :-----: | :---------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `N`                           | `N(x, n=15)`     | exact-to-decimal evaluation at precision `n`                                   |
|  [02]   | `Expr.evalf`                  | `expr.evalf(n)`  | arbitrary-precision numeric evaluation                                         |
|  [03]   | `printing.numpy.NumPyPrinter` | `NumPyPrinter()` | backs `lambdify` NumPy mode; `modules='jax'` selects the JAX traceable printer |

[ENTRYPOINT_SCOPE]: Poly coefficient and root extraction
- rail: symbolic
- Every surface is a method on `Poly(expr, x)`; `resultant(p.as_expr(), p.diff(x).as_expr(), x)` is the squarefree-discriminant metric.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]       | [CAPABILITY]                      |
| :-----: | :------------------ | :----------------- | :-------------------------------- |
|  [01]   | `Poly.all_coeffs`   | `.all_coeffs()`    | ordered coefficient vector        |
|  [02]   | `Poly.nroots`       | `.nroots(n=15)`    | numeric root isolation            |
|  [03]   | `Poly.real_roots`   | `.real_roots()`    | real-root isolation               |
|  [04]   | `Poly.factor_list`  | `.factor_list()`   | exact factorization               |
|  [05]   | `Poly.discriminant` | `.discriminant()`  | discriminant                      |
|  [06]   | `Poly.LC`           | `.LC()`            | lead coefficient                  |
|  [07]   | `Poly.monic`        | `.monic()`         | monic form                        |
|  [08]   | `Poly.as_expr`      | `.as_expr(*gens)`  | recover the `Expr` from a `Poly`  |
|  [09]   | `Poly.diff`         | `.diff(x, *specs)` | polynomial derivative as a `Poly` |

[ENTRYPOINT_SCOPE]: source code emission
- rail: symbolic
- printer family: `cxxcode`/`fcode`/`rust_code`/`julia_code`/`octave_code`/`jscode`/`pycode`/`glsl_code` are one polymorphic surface, each `<printer>(expr, standard=...)` targeting its language (`cxxcode(expr, standard='c++17')`, `fcode(expr, standard=95)`).

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                              | [CAPABILITY]                                                   |
| :-----: | :-------- | :-------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `ccode`   | `ccode(expr, assign_to=None, standard='c99', **settings)` | emit C99 source for the C# numeric owner                       |
|  [02]   | `cxxcode` | `cxxcode(expr, standard='c++17')`                         | the source-printer family, per-language via the printer choice |

[ENTRYPOINT_SCOPE]: expression compilation and code-module emission
- rail: symbolic
- call: `lambdify(args, expr, modules=None, printer=None, use_imps=True, dummify=False, cse=False, docstring_limit=1000)`
- call: `cse(exprs, symbols=None, optimizations=None, postprocess=None, order='canonical', ignore=(), list=True)`
- call: `utilities.codegen.codegen(name_expr, language=None, prefix=None, project='project', to_files=False, header=True, empty=True, argument_sequence=None, global_vars=None, standard=None, code_gen=None, printer=None)`
- call: `utilities.autowrap.autowrap(expr, language=None, backend='f2py', tempdir=None, args=None, flags=None, helpers=None, code_gen=None, **kwargs)`
- call: `utilities.autowrap.ufuncify(args, expr, language=None, backend='numpy', tempdir=None, flags=None, helpers=None, **kwargs)`

| [INDEX] | [SURFACE]                     | [CAPABILITY]                                                     |
| :-----: | :---------------------------- | :--------------------------------------------------------------- |
|  [01]   | `lambdify`                    | compile an expression to a Python/NumPy callable                 |
|  [02]   | `cse`                         | common-subexpression elimination for codegen                     |
|  [03]   | `utilities.codegen.codegen`   | emit a named, header-wrapped code module (C/Fortran/Octave/Rust) |
|  [04]   | `utilities.autowrap.autowrap` | compile + bind an expression to a native extension callable      |
|  [05]   | `utilities.autowrap.ufuncify` | emit a broadcasting NumPy ufunc from an expression               |

## [04]-[IMPLEMENTATION_LAW]

[SYMBOLIC_DERIVATION]:
- import: `import sympy as sym` at boundary scope only; module-level import is banned by the manifest import policy.
- construction root: `symbols` / `sympify` produce the free-variable vocabulary; assumptions (`real`, `positive`, `integer`) are derivation inputs, not post-hoc filters.
- transform set: `diff`, `integrate`, `limit`, `series`, `summation` own calculus; `solve`/`solveset`/`linsolve`/`nonlinsolve`/`dsolve` own solving; `simplify` and the rewriting passes own canonicalization.
- evidence: each derivation captures the input expression, the transform route, the simplification path, and the closed-form or set-valued result as a derivation receipt.
- boundary: sympy results are offline evidence; product algebra lives in C# owners after graduation.

[GRADUATION_PATH]:
- numeric bridge: `N`/`evalf` lift exact expressions to fixed-precision decimals through the bundled `mpmath` context (arbitrary `dps`); `Poly.all_coeffs`/`nroots`/`real_roots` extract ordered coefficient vectors and isolated roots.
- callable bridge: `lambdify(args, expr, modules='numpy', cse=True)` produces a NumPy-vectorized callable for study evaluation; `modules='jax'` produces a traceable callable for the JAX numeric rail; `ufuncify` emits a compiled broadcasting ufunc when the hot path warrants native code.
- C# bridge: `ccode` (or `cxxcode` for C++17) and `utilities.codegen.codegen` emit source that becomes the `Rasm.Compute` C# numeric-owner graduation candidate; one printer-family surface targets C/C++/Fortran/Rust/Julia/Octave via the printer choice rather than parallel emitters. `cse` precedes codegen to dedupe shared subexpressions.
- receipt: the graduation receipt carries the symbolic source, the `cse` factoring, the emitted source, the target language, and the precision claim from the `mpmath`-backed `evalf`.

[GEOMETRY_HANDOFF]:
- primitives: `geometry.Point3D`, `geometry.Line3D`, `geometry.Plane`, `geometry.Curve`, `geometry.Polygon`, `geometry.Parabola` model exact analytic geometry offline.
- operations: `geometry.intersection`, `Plane.intersection`/`projection`/`perpendicular_line`/`angle_between`, `geometry.convex_hull`, `geometry.centroid`, `geometry.idiff` (implicit differentiation), and distance methods produce exact relations.
- handoff: exact geometry relations graduate to the C# geometry owner as the geometry-handoff graduation case; the Python side never mutates a Rhino/GH document.

[INTEGRATION_STACK]:
- mpmath: `evalf`/`N` are sympy's own arbitrary-precision evaluation through the bundled `mpmath` `mp` context — the same precision oracle the `mpmath` exact-arithmetic rail uses to certify a fast-path numeric result against a guaranteed-correct high-`dps` value.
- numpy/jax: `lambdify` is the symbolic->numeric seam; `modules='numpy'` (the `NumPyPrinter`) feeds the dense array rail and `modules='jax'` feeds a traceable/differentiable callable for the JAX study rail, so a symbolic derivation never needs a hand-written numeric kernel.
- python-flint: where an exact polynomial/number-theory operation is hot, `python-flint` is the FLINT-backed accelerator beside sympy's pure-Python `Poly`; sympy owns the symbolic algebra, `python-flint` owns the fast exact ground-domain arithmetic.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `sympy`
- Owns: offline symbolic algebra, calculus, solving, simplification, polynomial/matrix algebra, set/assumption logic, geometry primitives, `mpmath`-backed exact-to-numeric evaluation, and the multi-language code-generation graduation path
- Accept: exact symbolic derivation that produces a graduation candidate or numeric study callable
- Reject: wrapper-renames of `diff`/`solve`/`simplify`; numeric kernels SciPy or NumPy owns; production algebra that belongs to a C# owner after graduation
