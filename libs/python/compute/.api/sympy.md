# [PY_COMPUTE_API_SYMPY]

`sympy` owns the compute branch's offline computer-algebra rail: exact calculus, equation solving, simplification, polynomial and matrix algebra, set and assumption logic, geometry primitives, `mpmath`-backed exact-to-numeric evaluation, and multi-language code generation. Every result is offline evidence a C# owner graduates as emitted source or a numeric callable, never production algebra the Python side runs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sympy`
- package: `sympy` (BSD-3-Clause)
- module: `sympy` (alias `sym`)
- namespaces: `sympy`, `sympy.geometry`, `sympy.utilities.codegen`, `sympy.utilities.autowrap`
- rail: symbolic
- asset: pure Python; bundled `mpmath` backs `evalf`/`N`, and `gmpy2` accelerates ground-domain integer arithmetic when present

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core expression vocabulary

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [CAPABILITY]                                                            |
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
|  [13]   | `Eq` `Ne` `Lt` `Le` `Gt` `Ge` | relational atom    | symbolic equation and inequality nodes                                  |

[PUBLIC_TYPE_SCOPE]: calculus and aggregate nodes

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]          | [CAPABILITY]                    |
| :-----: | :----------- | :--------------------- | :------------------------------ |
|  [01]   | `Derivative` | unevaluated derivative | symbolic differentiation node   |
|  [02]   | `Integral`   | unevaluated integral   | symbolic integration node       |
|  [03]   | `Sum`        | unevaluated sum        | symbolic summation node         |
|  [04]   | `Product`    | unevaluated product    | symbolic product node           |
|  [05]   | `Order`      | asymptotic term        | big-O remainder term for series |

[PUBLIC_TYPE_SCOPE]: polynomial, matrix, and geometry roots — geometry roots live under `sympy.geometry.*`, `Poly` and the matrix roots at top-level `sympy`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]       | [CAPABILITY]                                               |
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

[PUBLIC_TYPE_SCOPE]: set, logic, and matrix-expression roots — `Identity`, `ZeroMatrix`, `BlockMatrix`, `HadamardProduct`, `KroneckerProduct` are the unevaluated matrix-expression nodes over `MatrixSymbol`

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `Interval` `FiniteSet` `ConditionSet` `ImageSet`    | set atom      | the set-valued domains `solveset` returns               |
|  [02]   | `Union` `Intersection` `Complement`                 | set algebra   | combinators over sets                                   |
|  [03]   | `Reals` `Complexes` `Integers` `Naturals` `S.Reals` | named domain  | the `domain=` argument to `solveset`                    |
|  [04]   | `And` `Or` `Not` `Q` `Piecewise` `Rel`              | logic node    | boolean algebra, `Q` assumption predicates, branch expr |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `symbols(names, **assumptions)`    | function | batch symbol construction with assumptions          |
|  [02]   | `sympify(a, *, strict=False)`      | function | coerce Python/str to a sympy expression             |
|  [03]   | `Symbol(name, **assumptions)`      | ctor     | single symbol with assumption keywords              |
|  [04]   | `Function(name)`                   | ctor     | undeclared function head                            |
|  [05]   | `Expr.subs(map)`                   | method   | simultaneous structural substitution                |
|  [06]   | `Expr.replace(query, value)`       | method   | `Wild`-pattern match-and-rewrite                    |
|  [07]   | `Expr.rewrite(*targets)`           | method   | rewrite into an equivalent functional basis         |
|  [08]   | `srepr(expr, *, perm_cyclic=True)` | function | round-trip repr `eval(srepr(e)) == e`; content-keys |

[ENTRYPOINT_SCOPE]: calculus operations

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `diff(f, *symbols)`                          | function | n-th order derivative        |
|  [02]   | `integrate(*args, meijerg=None, risch=None)` | function | definite/indefinite integral |
|  [03]   | `limit(e, z, z0, dir='+')`                   | function | directional limit            |
|  [04]   | `series(expr, x=None, x0=0, n=6, dir='+')`   | function | Taylor/Laurent expansion     |
|  [05]   | `summation(f, *symbols)`                     | function | closed-form summation        |

[ENTRYPOINT_SCOPE]: equation and system solving

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `solve(f, *symbols, **flags)`                     | function | general algebraic solver, heuristic surface   |
|  [02]   | `solveset(f, symbol=None, domain=Complexes)`      | function | set-valued solver over a declared domain      |
|  [03]   | `linsolve(system, *symbols)`                      | function | linear-system solver returning a solution set |
|  [04]   | `nonlinsolve(system, *symbols)`                   | function | nonlinear-system set solver                   |
|  [05]   | `nsolve(*args, dict=False)`                       | function | numeric root-find from a symbolic system      |
|  [06]   | `dsolve(eq, func=None, hint='default', ics=None)` | function | ODE solver                                    |
|  [07]   | `pdsolve(eq, func=None, hint='default')`          | function | PDE solver                                    |
|  [08]   | `roots(f, *gens, multiple=False)`                 | function | polynomial-root multiset                      |

[ENTRYPOINT_SCOPE]: simplification and rewriting

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `simplify(expr, ratio=1.7)`                    | function | heuristic canonicalization     |
|  [02]   | `expand(e, deep=True)`                         | function | product and power distribution |
|  [03]   | `factor(f, *gens, deep=False)`                 | function | polynomial factorization       |
|  [04]   | `collect(expr, syms)`                          | function | symbolic term grouping         |
|  [05]   | `cancel(f, *gens)`                             | function | rational reduction             |
|  [06]   | `apart(f, x=None, full=False)`                 | function | partial-fraction split         |
|  [07]   | `together(expr, deep=False)`                   | function | common-denominator join        |
|  [08]   | `trigsimp(expr, inverse=False)`                | function | trigonometric rewriting        |
|  [09]   | `radsimp(expr, max_terms=4)`                   | function | radical rewriting              |
|  [10]   | `ratsimp(expr)`                                | function | rational rewriting             |
|  [11]   | `powsimp(expr, combine='all')`                 | function | power rewriting                |
|  [12]   | `logcombine(expr, force=False)`                | function | logarithm rewriting            |
|  [13]   | `nsimplify(expr, constants=(), rational=None)` | function | float-to-exact reconstruction  |

[ENTRYPOINT_SCOPE]: matrix algebra

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------ | :------ | :------------------------------------------------ |
|  [01]   | `Matrix(rows, cols, data)`                                          | ctor    | dense matrix construction                         |
|  [02]   | `eye` `zeros` `ones` `diag`                                         | factory | standard matrix builders                          |
|  [03]   | `Matrix.det`                                                        | method  | exact determinant                                 |
|  [04]   | `Matrix.inv`                                                        | method  | exact inverse                                     |
|  [05]   | `Matrix.eigenvals` `Matrix.eigenvects`                              | method  | exact spectrum (value→multiplicity / value→basis) |
|  [06]   | `Matrix.rref` `Matrix.charpoly` `Matrix.rank`                       | method  | RREF, characteristic polynomial, exact rank       |
|  [07]   | `Matrix.nullspace` `Matrix.columnspace`                             | method  | subspace bases                                    |
|  [08]   | `Matrix.LUdecomposition` `Matrix.QRdecomposition` `Matrix.cholesky` | method  | exact factorizations                              |
|  [09]   | `Matrix.diagonalize` `Matrix.jordan_form` `Matrix.singular_values`  | method  | canonical-form decompositions                     |
|  [10]   | `Matrix.exp` `Matrix.pinv` `Matrix.norm`                            | method  | matrix exponential, pseudo-inverse, exact norm    |

[ENTRYPOINT_SCOPE]: assumption logic, sets, and number theory

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `refine(expr, assumptions)`                     | function | simplify under a `Q` assumption context      |
|  [02]   | `ask(proposition, assumptions=True)`            | function | query a predicate against an assumption set  |
|  [03]   | `Q.real` `Q.integer` `Q.positive`               | property | assumption predicate constructors            |
|  [04]   | `Intersection` `Union` `Complement`             | ctor     | symbolic set algebra over `solveset` results |
|  [05]   | `factorint(n, multiple=False)`                  | function | integer factorization                        |
|  [06]   | `primerange(a, b=None)` `isprime(n)`            | function | prime generation and test                    |
|  [07]   | `gcd(f, g=None, *gens)` `lcm(f, g=None, *gens)` | function | polynomial/integer gcd and lcm               |
|  [08]   | `resultant(f, g, *gens, includePRS=False)`      | function | resultant of two polynomials (elimination)   |

[ENTRYPOINT_SCOPE]: geometry operations — exact analytic relations over the `sympy.geometry.*` roots

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `geometry.intersection(*entities)`               | function | exact intersection of geometric entities     |
|  [02]   | `geometry.convex_hull(*points)`                  | function | exact convex hull of a point set             |
|  [03]   | `geometry.centroid(*entities)`                   | function | exact centroid                               |
|  [04]   | `geometry.idiff(eq, y, x, n=1)`                  | function | implicit differentiation of a relation       |
|  [05]   | `Plane.intersection` `Plane.projection`          | method   | plane intersection and orthogonal projection |
|  [06]   | `Plane.perpendicular_line` `Plane.angle_between` | method   | perpendicular line and dihedral angle        |

[ENTRYPOINT_SCOPE]: numeric evaluation

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                                                         |
| :-----: | :---------------------------- | :------- | :------------------------------------------------------------------- |
|  [01]   | `N(x, n=15)`                  | function | exact-to-decimal evaluation at precision `n`                         |
|  [02]   | `Expr.evalf(n)`               | method   | arbitrary-precision numeric evaluation                               |
|  [03]   | `printing.numpy.NumPyPrinter` | class    | backs `lambdify` NumPy mode; `modules='jax'` selects the JAX printer |

[ENTRYPOINT_SCOPE]: `Poly` coefficient and root extraction — every surface a method on `Poly(expr, x)`

| [INDEX] | [SURFACE]              | [SHAPE] | [CAPABILITY]                      |
| :-----: | :--------------------- | :------ | :-------------------------------- |
|  [01]   | `Poly.all_coeffs()`    | method  | ordered coefficient vector        |
|  [02]   | `Poly.nroots(n=15)`    | method  | numeric root isolation            |
|  [03]   | `Poly.real_roots()`    | method  | real-root isolation               |
|  [04]   | `Poly.factor_list()`   | method  | exact factorization               |
|  [05]   | `Poly.discriminant()`  | method  | discriminant                      |
|  [06]   | `Poly.LC()`            | method  | lead coefficient                  |
|  [07]   | `Poly.monic()`         | method  | monic form                        |
|  [08]   | `Poly.as_expr(*gens)`  | method  | recover the `Expr` from a `Poly`  |
|  [09]   | `Poly.diff(x, *specs)` | method  | polynomial derivative as a `Poly` |

[ENTRYPOINT_SCOPE]: source emission and compilation — `cxxcode`/`fcode`/`rust_code`/`julia_code`/`octave_code`/`jscode`/`pycode`/`glsl_code` are one printer-family surface, each `<printer>(expr, standard=...)` targeting its language

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `ccode(expr, assign_to=None, standard='c99')`       | function | emit C99 source for the C# numeric owner                       |
|  [02]   | `cxxcode(expr, standard='c++17')`                   | function | the source-printer family, per-language via the printer choice |
|  [03]   | `lambdify(args, expr, modules=None, cse=False)`     | function | compile an expression to a Python/NumPy/JAX callable           |
|  [04]   | `cse(exprs, order='canonical')`                     | function | common-subexpression elimination for codegen                   |
|  [05]   | `codegen(name_expr, language=None, to_files=False)` | function | emit a named, header-wrapped C/Fortran/Octave/Rust module      |
|  [06]   | `autowrap(expr, language=None, backend='f2py')`     | function | compile + bind an expression to a native extension callable    |
|  [07]   | `ufuncify(args, expr, backend='numpy')`             | function | emit a broadcasting NumPy ufunc from an expression             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- assumptions (`real`, `positive`, `integer`) declared on `symbols`/`sympify` are derivation inputs, not post-hoc filters, so a derivation constrains its domain at construction.
- exact symbolic results are offline evidence; production algebra lives in the C# owner after graduation, and the Python side never mutates a Rhino/GH document.
- graduation emits one artifact per candidate — a numeric callable (`lambdify`/`ufuncify`) or C/C++/Fortran/Rust source (`ccode`/`codegen`) through one printer-family surface, never parallel emitters — and `cse` factors shared subexpressions before emission.
- each derivation folds one receipt: input expression, transform route, `cse` factoring, emitted source or callable, target language, and the `mpmath`-backed precision claim.

[STACKING]:
- `mpmath`(`.api/mpmath.md`): `evalf`/`N` evaluate through the bundled `mp` context, lifting an exact closed form to arbitrary `dps` as the precision oracle a fast-path result certifies against.
- `numpy`(`libs/python/.api/numpy.md`) / `jax`(`.api/jax.md`): `lambdify(..., modules='numpy')` compiles a vectorized callable and `modules='jax'` a traceable/differentiable one, so a symbolic derivation feeds the numeric study rail with no hand-written kernel.
- `python-flint`(`.api/python-flint.md`): a hot exact polynomial or number-theory step promotes off the pure-Python `Poly` onto FLINT-backed ground-domain arithmetic; sympy owns the symbolic algebra, flint the fast exact ground domain.
- within-lib: `SymbolicDerivation` lowers a derivation to a numpy or C handoff artifact, and `HandoffAxis` graduates exact `geometry.*` relations to the C# geometry owner as the geometry-handoff case.

[LOCAL_ADMISSION]:
- `import sympy as sym` at boundary scope produces the free-variable vocabulary through `symbols`/`sympify`; `cse` precedes any codegen emission.
- `resultant(p.as_expr(), p.diff(x).as_expr(), x)` is the squarefree-discriminant metric `|res(p, p')|`, recognized as study evidence, never a production numeric path.

[RAIL_LAW]:
- Package: `sympy`
- Owns: offline symbolic algebra, calculus, solving, simplification, polynomial/matrix algebra, set/assumption logic, geometry primitives, `mpmath`-backed exact-to-numeric evaluation, and the multi-language code-generation graduation path
- Accept: exact symbolic derivation producing a graduation candidate or a numeric study callable
- Reject: wrapper-renames of `diff`/`solve`/`simplify`; numeric kernels SciPy or NumPy owns; production algebra a C# owner holds after graduation
