# [PY_COMPUTE_API_SYMPY]

`sympy` supplies the symbolic algebra surface for the compute symbolic-derivation rail: expression construction, calculus, equation solving, simplification, matrix algebra, exact-to-numeric evaluation, geometry primitives, and the C/NumPy code-generation path that backs the C# graduation receipt. The package owner composes these surfaces into the `SymbolicDerivation` owner and the geometry-handoff graduation case; it never re-implements an algebra primitive sympy already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sympy`
- package: `sympy`
- import: `sympy` (lint alias `sym`)
- owner: `compute`
- rail: symbolic
- capability: pure-Python computer algebra system; exact symbolic computation, calculus, solving, simplification, matrix algebra, geometry, numeric evaluation, and code generation

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

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]      | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------------------ | :------------------ | :--------------------------------------------------------- |
|  [01]   | `Poly`                                                              | polynomial owner    | dense polynomial with `all_coeffs`, `degree`, roots access |
|  [02]   | `Matrix`                                                            | mutable matrix      | dense symbolic matrix                                      |
|  [03]   | `ImmutableMatrix`                                                   | hashable matrix     | immutable dense matrix for expression embedding            |
|  [04]   | `MatrixSymbol`                                                      | symbolic matrix     | named matrix variable                                      |
|  [05]   | `geometry.Point2D` `geometry.Point3D`                               | geometry atom       | exact coordinate point, `Point3D(*args, _nocheck=False)`   |
|  [06]   | `geometry.Line` `geometry.Line3D` `geometry.Segment` `geometry.Ray` | geometry curve      | exact line/segment/ray                                     |
|  [07]   | `geometry.Circle` `geometry.Ellipse`                                | geometry conic      | exact conic section                                        |
|  [08]   | `geometry.Plane`                                                    | geometry surface    | exact plane, ctor `Plane(p1, a=None, b=None)`              |
|  [09]   | `geometry.Polygon` `geometry.Triangle`                              | geometry region     | exact polygon/triangle                                     |
|  [10]   | `geometry.Curve`                                                    | geometry parametric | parametric curve over a parameter range                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion
- rail: symbolic

| [INDEX] | [SURFACE]  | [CALL_SHAPE]                  | [CAPABILITY]                               |
| :-----: | :--------- | :---------------------------- | :----------------------------------------- |
|  [01]   | `symbols`  | `symbols('x y z', real=True)` | batch symbol construction with assumptions |
|  [02]   | `sympify`  | `sympify(obj)`                | coerces Python/str to a sympy expression   |
|  [03]   | `Symbol`   | `Symbol('x', positive=True)`  | single symbol with assumption keywords     |
|  [04]   | `Function` | `Function('f')`               | undeclared function head                   |

[ENTRYPOINT_SCOPE]: calculus operations
- rail: symbolic

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                                                                | [CAPABILITY]                     |
| :-----: | :---------- | :------------------------------------------------------------------------------------------ | :------------------------------- |
|  [01]   | `diff`      | `diff(f, *symbols, **kwargs)`                                                               | differentiate to arbitrary order |
|  [02]   | `integrate` | `integrate(*args, meijerg=None, conds='piecewise', risch=None, heurisch=None, manual=None)` | definite/indefinite integration  |
|  [03]   | `limit`     | `limit(e, z, z0, dir='+')`                                                                  | directional limit                |
|  [04]   | `series`    | `series(expr, x=None, x0=0, n=6, dir='+')`                                                  | Taylor/Laurent series expansion  |
|  [05]   | `summation` | `summation(f, (i, a, b))`                                                                   | closed-form finite/infinite sum  |

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

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]               | [CAPABILITY]              |
| :-----: | :-------------------------------------- | :------------------------- | :------------------------ |
|  [01]   | `Matrix`                                | `Matrix(rows, cols, data)` | dense matrix construction |
|  [02]   | `eye` `zeros` `ones` `diag`             | `eye(n)`                   | standard matrix builders  |
|  [03]   | `Matrix.det`                            | `m.det()`                  | exact determinant         |
|  [04]   | `Matrix.inv`                            | `m.inv()`                  | exact inverse             |
|  [05]   | `Matrix.eigenvals` `Matrix.eigenvects`  | `m.eigenvals()`            | exact spectrum            |
|  [06]   | `Matrix.rref`                           | `m.rref()`                 | reduced row-echelon form  |
|  [07]   | `Matrix.nullspace` `Matrix.columnspace` | `m.nullspace()`            | subspace bases            |

[ENTRYPOINT_SCOPE]: numeric evaluation and code generation (C# graduation path)
- rail: symbolic

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                                                                                                                       | [CAPABILITY]                                     |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `N`                           | `N(x, n=15)`                                                                                                                                                                                       | exact-to-decimal evaluation at precision `n`     |
|  [02]   | `Expr.evalf`                  | `expr.evalf(n)`                                                                                                                                                                                    | arbitrary-precision numeric evaluation           |
|  [03]   | `lambdify`                    | `lambdify(args, expr, modules=None, printer=None, use_imps=True, dummify=False, cse=False, docstring_limit=1000)`                                                                                  | compile expression to a Python/NumPy callable    |
|  [04]   | `cse`                         | `cse(exprs, symbols=None, optimizations=None, postprocess=None, order='canonical', ignore=(), list=True)`                                                                                          | common-subexpression elimination for codegen     |
|  [05]   | `Poly.all_coeffs`             | `Poly(expr, x).all_coeffs()`                                                                                                                                                                       | ordered coefficient vector for numeric handoff   |
|  [06]   | `printing.c.ccode`            | `ccode(expr, assign_to=None, standard='c99', **settings)`                                                                                                                                          | emit C99 source for the C# numeric owner         |
|  [07]   | `utilities.codegen.codegen`   | `codegen(name_expr, language=None, prefix=None, project='project', to_files=False, header=True, empty=True, argument_sequence=None, global_vars=None, standard=None, code_gen=None, printer=None)` | emit a named code module (C/Fortran/Octave)      |
|  [08]   | `printing.numpy.NumPyPrinter` | `NumPyPrinter()`                                                                                                                                                                                   | printer backing `lambdify(..., modules='numpy')` |

## [04]-[IMPLEMENTATION_LAW]

[SYMBOLIC_DERIVATION]:
- import: `import sympy as sym` at boundary scope only; module-level import is banned by the manifest import policy.
- construction root: `symbols` / `sympify` produce the free-variable vocabulary; assumptions (`real`, `positive`, `integer`) are derivation inputs, not post-hoc filters.
- transform set: `diff`, `integrate`, `limit`, `series`, `summation` own calculus; `solve`/`solveset`/`linsolve`/`nonlinsolve`/`dsolve` own solving; `simplify` and the rewriting passes own canonicalization.
- evidence: each derivation captures the input expression, the transform route, the simplification path, and the closed-form or set-valued result as a derivation receipt.
- boundary: sympy results are offline evidence; product algebra lives in C# owners after graduation.

[GRADUATION_PATH]:
- numeric bridge: `N`/`evalf` lift exact expressions to fixed-precision decimals; `Poly.all_coeffs` extracts ordered coefficient vectors.
- callable bridge: `lambdify(..., modules='numpy', cse=True)` produces a NumPy-vectorized callable for study evaluation.
- C# bridge: `ccode` and `utilities.codegen.codegen` emit C99 source that becomes the C# numeric-owner graduation candidate; `cse` precedes codegen to dedupe shared subexpressions.
- receipt: the graduation receipt carries the symbolic source, the `cse` factoring, the emitted C source, and the precision claim from `evalf`.

[GEOMETRY_HANDOFF]:
- primitives: `geometry.Point3D`, `geometry.Line3D`, `geometry.Plane`, `geometry.Curve`, `geometry.Polygon` model exact analytic geometry offline.
- operations: `geometry.intersection`, `Plane.intersection`, distance and projection methods produce exact relations.
- handoff: exact geometry relations graduate to the C# geometry owner as the geometry-handoff graduation case; the Python side never mutates a Rhino/GH document.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `sympy`
- Owns: offline symbolic algebra, calculus, solving, simplification, matrix algebra, geometry primitives, exact-to-numeric evaluation, and the C/NumPy code-generation graduation path
- Accept: exact symbolic derivation that produces a graduation candidate or numeric study callable
- Reject: wrapper-renames of `diff`/`solve`/`simplify`; numeric kernels SciPy or NumPy owns; production algebra that belongs to a C# owner after graduation
