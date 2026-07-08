# [PY_ARTIFACTS_API_KIWISOLVER]

`kiwisolver` supplies the incremental Cassowary constraint-solver substrate for the artifacts diagram-and-sheet layout plane: a `Solver` that owns a live constraint system, a `Variable` free-symbol whose `value()` the solver writes, the `Term`/`Expression` linear-algebra value objects built by operator overloading (`2 * x + y - 30`), a `Constraint` produced when an expression meets a relational operator (`x + gap <= y`), a `strength` priority singleton with `weak`/`medium`/`strong`/`required` bands plus `strength.create(a, b, c, weight)` symbolic-weight blending, edit-variable suggestion for interactive re-solve, and a typed exception family (`UnsatisfiableConstraint`, `DuplicateConstraint`, `BadRequiredStrength`, `UnknownEditVariable`, `UnknownConstraint`, `DuplicateEditVariable`) that is the failure boundary. The package owner composes `Solver`, the operator-built `Constraint` algebra, and the `strength` bands into the constraint-layout arm of `visualization/diagram/layout#LAYOUT` — alignment, equal-distribution, anchoring, minimum-separation, symmetry, and aspect constraints over the same stable `rustworkx` node index the force/radial/layered policies key on — and into the `composition/sheet#SHEET` viewport/grid alignment math; it never re-implements a linear-programming simplex (Cassowary's dual simplex is the native pipeline), never owns graph topology (that is `data/graph#GRAPH`/`rustworkx`), and never emits SVG (that is `visualization/diagram/draw#DRAW`). The synchronous `Solver` build and `updateVariables()` solve run inside the one `_render` kernel `visualization/diagram/layout#LAYOUT` already offloads onto `anyio.to_thread.run_sync` under its `CapacityLimiter`, so the native solve never blocks the event loop, and every solver fault crosses the `rasm.runtime.faults` rail rather than escaping as a bare exception.

## [01]-[PACKAGE_SURFACE]

- package: `kiwisolver`
- import: `kiwisolver`
- owner: `artifacts`
- rail: figure
- installed: `1.5.0` (wrapper); `__kiwi_version__` `1.4.2` (underlying C++ Kiwi library — distinct from the Python wrapper version, surface the C++ version when a solver-behavior receipt needs the algorithm provenance)
- license: BSD (Modified/3-Clause, Nucleic Development Team)
- target: cp315 native abi (`_cext.cpython-315-darwin.so`, `Root-Is-Purelib: false`, `py.typed`); the version-specific (NOT abi3) C extension has no published cp315 wheel — the resolved `.so` is a local source build, so the manifest row carries `; python_version<'3.15'` and the marker drops when a stable cp315/abi3 wheel lands. Promoted from a transitive `matplotlib` dependency to a direct admission per the campaign roster.
- entry points: none (library only)
- capability: incremental Cassowary linear-constraint solving with amortized-incremental add/remove and edit-variable re-solve; an operator-built `Variable`/`Term`/`Expression` linear-form algebra; `Constraint` construction by relational operator with a four-band priority hierarchy plus symbolic-weight blending; edit variables for interactive suggested-value re-solve (drag/resize); constraint-violation introspection; full solver-internals text dump (`Objective`/`Tableau`/`Variables`/`Edit Variables`/`Constraints`); and a typed exception family naming every solver-state failure with the offending `constraint`/`edit_variable` carried on the exception

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, free symbol, and linear-form value objects
- rail: figure

The constraint system is five types: a `Solver` mutable owner, a `Variable` free symbol, the `Term`/`Expression` linear-combination value objects (immutable — built by operator overloading, never mutated), and a `Constraint` (an `Expression op 0` plus a priority). `Variable`/`Term`/`Expression` are explicitly unhashable (`__hash__ = None`) — they are algebraic operands, never dict keys; key constraint layouts on the stable `rustworkx` node index, not on a `Variable`.

| [INDEX] | [TYPE] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Solver` | system owner | the mutable constraint system; `addConstraint`/`updateVariables`/`suggestValue`/`reset` |
| [02] | `Variable` | free symbol | a named unknown the solver writes; `value()` reads the solved coordinate |
| [03] | `Term` | linear value | a `coefficient * Variable` product (immutable); built by `coeff * var` |
| [04] | `Expression` | linear value | a sum of `Term`s plus a constant (immutable); built by `+`/`-` over variables/terms |
| [05] | `Constraint` | relation | an `Expression` + relational op (`==`/`<=`/`>=`) + a `strength`; built by `expr <= rhs` |
| [06] | `Strength` | priority | the `strength` singleton (`type_check_only` class); `weak`/`medium`/`strong`/`required` + `create` |

[PUBLIC_TYPE_SCOPE]: typed solver-fault family
- rail: figure

Every solver-state failure is a distinct exception carrying the offending object, so the `rasm.runtime.faults` boundary discriminates the fault kind without string-matching a message. `addConstraint`/`addEditVariable`/`suggestValue`/`removeConstraint` are the throwing surfaces; `__slots__` carries the offending `constraint` or `edit_variable`.

| [INDEX] | [TYPE] | [SLOT] | [RAISED_BY] |
| --- | --- | --- | --- |
| [01] | `UnsatisfiableConstraint` | `constraint` | `addConstraint` when a new required constraint conflicts |
| [02] | `DuplicateConstraint` | `constraint` | `addConstraint` for an already-added constraint |
| [03] | `UnknownConstraint` | `constraint` | `removeConstraint`/`hasConstraint` for a never-added constraint |
| [04] | `BadRequiredStrength` | (none) | `addEditVariable` with `required` strength (edits must be non-required) |
| [05] | `DuplicateEditVariable` | `edit_variable` | `addEditVariable` for an already-added edit variable |
| [06] | `UnknownEditVariable` | `edit_variable` | `suggestValue`/`removeEditVariable` for a non-edit variable |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Variable` / `Term` / `Expression` construct and linear algebra
- rail: figure

`Variable("x")` is the free symbol; the linear form is built by operator overloading, never by a constructor call — `2 * x` is a `Term`, `2 * x + y - 30` is an `Expression`, and a relational operator turns an expression into a `Constraint`. `__eq__`/`__ge__`/`__le__` produce a `Constraint`; `__ne__`/`__gt__`/`__lt__` are `NoReturn` (strict orderings and inequality are intentionally unsupported — Cassowary models `==`/`<=`/`>=` only). Build alignment/separation expressions directly from the operator overloads; never hand-construct a `Term`/`Expression` tuple.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Variable(name="", context=None)` | construct | a named free unknown; optional `context` carries an opaque payload |
| [02] | `Variable.value()` | read | the solved value after `updateVariables()` (`0.0` before solve) |
| [03] | `Variable.name()` / `setName(name)` | identity | get/set the variable name (used in `Solver.dumps()` provenance) |
| [04] | `Variable.context()` / `setContext(obj)` | payload | get/set the opaque context object the consumer threads through |
| [05] | `var * c` / `c * var` -> `Term` | algebra | scale a variable to a `Term` (coefficient product) |
| [06] | `var + other` / `var - other` -> `Expression` | algebra | sum variables/terms/floats into an `Expression` |
| [07] | `Term(variable, coefficient=1.0)` | construct | explicit `Term` (prefer `c * var`); `coefficient()`/`variable()`/`value()` |
| [08] | `Expression(terms, constant=0.0)` | construct | explicit `Expression`; `terms()` (a `tuple[Term, ...]`)/`constant()`/`value()` |
| [09] | `expr == rhs` / `expr <= rhs` / `expr >= rhs` -> `Constraint` | relation | build a `Constraint` (rhs is `float`/`Variable`/`Term`/`Expression`) |

[ENTRYPOINT_SCOPE]: `Constraint` and `strength` priority
- rail: figure

A `Constraint` carries an `Expression`, a relational op, and a `strength`. The relational operators default to `required` strength; `constraint | strength` (or the `strength=` arg of the explicit constructor) lowers it to `weak`/`medium`/`strong` or any numeric band. `strength.create(a, b, c, weight=1.0)` blends the three bands symbolically (`a` -> strong slot ×10^6, `b` -> medium ×10^3, `c` -> weak ×1, each clamped to its band, the whole scaled by `weight`) for a custom priority between the canonical bands. `required` (= `1001001000.0`) constraints must be satisfiable or the add raises `UnsatisfiableConstraint`; non-required constraints are minimized as soft objectives, so a layout expresses hard rules as `required` and aesthetic preferences (centering, equal spacing) as `weak`/`medium`.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Constraint(expression, op, strength="required")` | construct | explicit build; `op` is `"=="`/`"<="`/`">="`, `strength` float or band name |
| [02] | `constraint \| strength` | priority | lower a constraint's strength (`(x == y) \| "weak"`); band name or float |
| [03] | `Constraint.expression()` / `op()` / `strength()` | read | the constraint's expression, relational op, and numeric strength |
| [04] | `Constraint.violated()` | introspect | whether the constraint is unsatisfied in the current solver state (post-solve QA) |
| [05] | `strength.weak` / `medium` / `strong` / `required` | band | the four canonical priority floats (`1.0` / `1e3` / `1e6` / `1001001000.0`) |
| [06] | `strength.create(a, b, c, weight=1.0)` | blend | a symbolic priority between bands (e.g. `strength.create(0, 1, 0, 2)` = 2×medium) |

[ENTRYPOINT_SCOPE]: `Solver` system lifecycle and edit-variable re-solve
- rail: figure

`Solver` is the one mutable owner of the constraint system — `addConstraint`/`removeConstraint` mutate in place and return `None` (the solver is the stateful sink, never a fluent value object). Solving is two-phase: register constraints and edit variables, then `updateVariables()` writes the solved `value()` into every `Variable`. Edit variables (`addEditVariable` + `suggestValue` + `updateVariables`) are the interactive re-solve path — register an edit variable at a non-`required` strength, suggest a new value (a dragged handle, a resized sheet width), and re-solve incrementally without rebuilding the system, the amortized-incremental advantage over a from-scratch LP. `dump()`/`dumps()` serialize the full solver internals (objective, tableau, variable bindings, edit variables, constraints) for a layout-debug receipt.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Solver()` | construct | a fresh empty constraint system |
| [02] | `Solver.addConstraint(constraint)` | mutate | add a constraint (raises `Duplicate`/`Unsatisfiable`); returns `None` |
| [03] | `Solver.removeConstraint(constraint)` | mutate | remove a constraint (raises `UnknownConstraint`) |
| [04] | `Solver.hasConstraint(constraint)` | query | whether the solver contains the constraint |
| [05] | `Solver.addEditVariable(variable, strength)` | mutate | register an edit variable (non-`required`; raises `BadRequiredStrength`/`Duplicate`) |
| [06] | `Solver.removeEditVariable(variable)` | mutate | drop an edit variable (raises `UnknownEditVariable`) |
| [07] | `Solver.hasEditVariable(variable)` | query | whether the solver contains the edit variable |
| [08] | `Solver.suggestValue(variable, value)` | mutate | suggest a desired value for an edit variable (raises `UnknownEditVariable`) |
| [09] | `Solver.updateVariables()` | solve | write the solved values into every `Variable` (the solve commit) |
| [10] | `Solver.reset()` | mutate | clear all constraints/edit variables back to empty |
| [11] | `Solver.dump()` / `dumps()` | introspect | print / return the full solver-internals text (debug receipt) |

## [04]-[IMPLEMENTATION_LAW]

- import: `import kiwisolver` (or `from kiwisolver import Solver, Variable, strength`) at boundary scope only; the distribution and import name are both `kiwisolver`; the wrapper version is `kiwisolver.__version__` and the underlying C++ library version is `kiwisolver.__kiwi_version__` (surface the latter on a solver-behavior receipt, never assume they match).
- system axis: `Solver` is the single mutable constraint-system owner; `addConstraint`/`removeConstraint`/`addEditVariable` mutate in place and return `None`, and `updateVariables()` is the one solve commit that writes `Variable.value()`, never a per-constraint-kind solver method family and never a fluent return chain. One `Solver` per laid-out diagram/sheet, built and solved inside the `visualization/diagram/layout#LAYOUT` `_render` kernel.
- algebra axis: the linear form is built by operator overloading (`c * var` -> `Term`, `+`/`-` -> `Expression`, `==`/`<=`/`>=` -> `Constraint`), never by constructing `Term`/`Expression` tuples by hand; `__ne__`/`__gt__`/`__lt__` are `NoReturn` (Cassowary models equality and non-strict inequality only), so an alignment/separation rule is one expression-and-operator line, not a hand-built tableau row.
- priority axis: hard layout rules are `required` (the operator default) and aesthetic preferences are `weak`/`medium`/`strong` via `constraint | "weak"` or `strength.create(...)`; a soft constraint is minimized as an objective rather than enforced, so over-constrained-but-preferred layouts (center if possible, else left-align) are expressed as a strength hierarchy, never as conditional Python branching that adds/removes constraints per case.
- edit axis: interactive re-layout (drag a node, resize a sheet, re-flow on viewport change) registers the moved coordinate as an edit variable at a non-`required` strength, calls `suggestValue` + `updateVariables`, and re-solves incrementally — the amortized-incremental advantage — never rebuilding the `Solver` from scratch per frame.
- identity axis: `Variable`/`Term`/`Expression` are unhashable (`__hash__ = None`); key a per-node `Variable` map on the stable `rustworkx` integer node index (the same index `visualization/diagram/layout#LAYOUT` coordinates and `visualization/diagram/glyphset#GLYPHSET` marks key on), reading `var.value()` back into the `LayoutMap`, never keying a dict on a `Variable` itself.
- fault axis: the solver-state failures are a typed exception family carrying the offending `constraint`/`edit_variable` on `__slots__`; the boundary catches `UnsatisfiableConstraint`/`DuplicateConstraint`/`BadRequiredStrength`/`UnknownEditVariable`/`UnknownConstraint`/`DuplicateEditVariable` and maps each to a `rasm.runtime.faults` rail case, never letting a bare kiwisolver exception escape and never string-matching a message — the exception type and its carried object are the discriminant.
- concurrency axis: the `Solver` build + `updateVariables()` solve is synchronous native work; it runs inside the one `_render` kernel `visualization/diagram/layout#LAYOUT` offloads onto `anyio.to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` under the module `CapacityLimiter`, so the native solve never blocks the event loop and never holds a `Solver` across the await boundary — never solve on the loop thread, never share one `Solver` across threads.
- evidence axis: a constraint-layout op captures the variable count, constraint count, edit-variable count, the `__kiwi_version__` algorithm provenance, the number of `violated()` soft constraints after the solve, and (on a debug receipt) the `dumps()` tableau, contributing the `visualization/diagram/layout#LAYOUT` `ArtifactReceipt.Diagram` `algorithm` slot the value `"cassowary"` for the constraint policy.
- boundary: kiwisolver owns the linear-constraint solve and the priority hierarchy; graph topology and force/radial/hierarchical placement stay at `rustworkx`/`grandalf` behind `visualization/diagram/layout#LAYOUT`; SVG emission stays at `visualization/diagram/draw#DRAW`; the `numpy` coordinate arrays the laid-out points feed stay at the shared `libs/python/.api/numpy` rail; a hand-rolled simplex/LP, a per-constraint-kind solver method family, a `Variable`-keyed dict, a from-scratch re-solve per interactive frame, a bare escaping solver exception, and a synchronous solve left on the event loop are the deleted forms.

[STACKING]:
- `visualization/diagram/layout#LAYOUT` — kiwisolver is the constraint-layout arm of the `LayoutPolicy` `expression.tagged_union`: a `Constrained(rules)` case (sibling to `Force`/`Radial`/`Layered`/`Projected`) builds one `Solver`, maps each stable `rustworkx` node index to an `x`/`y` `Variable` pair, folds the per-kind alignment/distribution/separation rules (`required` for hard grid snapping, `weak`/`medium` for centering and equal-gap aesthetics) into `addConstraint` calls, `updateVariables()`, and reads `var.value()` back into the `LayoutMap` the glyph fold consumes — keyed on the same node index every other policy uses, so the constraint policy is one more `_position` match arm, never a parallel layout owner.
- `rasm.runtime.faults` (shared rail) — the `UnsatisfiableConstraint`/`DuplicateConstraint`/`BadRequiredStrength`/`Unknown*`/`Duplicate*` family is caught at the `async_boundary` the layout `_compute` already wraps, mapping each typed solver fault (with its carried `constraint`/`edit_variable`) onto a `RuntimeRail` case, so an over-constrained diagram returns a typed `Result` fault rather than crashing the render — the same `expression`-folded `Result`/`RuntimeRail` egress the rest of the artifacts corpus returns.
- `composition/sheet#SHEET` — the viewport/grid alignment math (`Viewport` scale-to-fit, ISO 5457 frame-field placement, multi-viewport equal-distribution on a sheet) is the same Cassowary system: sheet width/height and viewport extents become edit variables, the frame and title-block field positions become `required`/`weak` constraints, and `suggestValue` re-solves the field layout when the sheet size changes — one `Solver` shared with the diagram-layout pattern, not a second alignment engine.
- `libs/python/.api/numpy` (shared rail) — the solved `var.value()` coordinates collect into a `numpy` `float64` array (the `(N, 2)` point matrix the `visualization/diagram/glyphset#GLYPHSET` marks and the `data/tabular#GRAPH` route geometry consume), so the constraint solve feeds the same numeric coordinate substrate the force/radial/layered policies produce, never a parallel coordinate representation.
- `libs/python/.api/msgspec` + `libs/python/.api/structlog` (shared rails) — the layout-evidence receipt model (`msgspec.Struct`: variable/constraint/edit-variable counts, `__kiwi_version__`, `violated()` tally) is the same `msgspec`-encoded receipt shape the rest of the corpus uses, and the soft-constraint `violated()` count plus the `dumps()` tableau ride a `structlog`-bound debug event under the layout span, so a constraint-layout op's diagnostics are structured, not a printed dump.

## [05]-[LOCAL_ADMISSION]

- Package: `kiwisolver`
- Owns: incremental Cassowary linear-constraint solving (add/remove/edit-variable re-solve), the operator-built `Variable`/`Term`/`Expression` linear-form algebra, `Constraint` construction with the four-band `strength` priority hierarchy plus `strength.create` symbolic blending, edit-variable interactive re-solve, constraint-violation introspection, and full solver-internals serialization
- Accept: the constraint-layout policy arm of `visualization/diagram/layout#LAYOUT` (alignment/distribution/anchoring/minimum-separation/symmetry/aspect over the stable `rustworkx` node index), the `composition/sheet#SHEET` viewport/grid/title-block alignment math, and any interactive re-layout that re-solves on a suggested value, with every solver fault crossing the `rasm.runtime.faults` rail and the solve offloaded onto the layout `CapacityLimiter` thread
- Reject: a hand-rolled simplex/linear-programming solver where `Solver` exists; graph topology or force/radial/hierarchical placement (that is `rustworkx`/`grandalf` behind `visualization/diagram/layout#LAYOUT`); SVG emission (that is `visualization/diagram/draw#DRAW`); a `Variable`-keyed dict where the stable node index is the key; a from-scratch `Solver` rebuild per interactive frame where edit variables re-solve incrementally; a bare escaping solver exception where the typed family maps onto the runtime rail; a synchronous solve on the event loop where `anyio.to_thread.run_sync` offloads it
