# [PY_ARTIFACTS_API_KIWISOLVER]

`kiwisolver` owns incremental Cassowary linear-constraint solving for the artifacts layout plane: a mutable `Solver` over an operator-built `Variable`/`Term`/`Expression` algebra, `Constraint`s carrying a four-band `strength` priority, and edit-variable re-solve. It never re-implements the simplex, owns graph topology or placement (`rustworkx`/`grandalf`), or emits SVG (`visualization/diagram/draw#DRAW`); it feeds the constraint-layout arm of `visualization/diagram/layout#LAYOUT` and the `composition/sheet#SHEET` alignment math, every solver fault crossing `rasm.runtime.faults`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `kiwisolver`
- package: `kiwisolver` (BSD-3-Clause, Nucleic Development Team)
- module: `kiwisolver`
- rail: figure
- abi: native C extension (`kiwisolver._cext`), `py.typed`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, free symbol, and linear-form value objects

`Solver` owns the constraint system over a `Variable` free symbol, the immutable `Term`/`Expression` linear-combination value objects built by operator overloading, and a `Constraint` (an `Expression`, a relational op, and a `strength`); `strength` is the priority singleton.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Solver`     | class         | the mutable constraint-system owner                           |
|  [02]   | `Variable`   | class         | a named free unknown the solver writes                        |
|  [03]   | `Term`       | value-object  | an immutable `coefficient * Variable` product                 |
|  [04]   | `Expression` | value-object  | an immutable sum of `Term`s and a constant                    |
|  [05]   | `Constraint` | value-object  | an `Expression` + relational op + `strength`                  |
|  [06]   | `Strength`   | class         | the `strength` singleton; `weak`/`medium`/`strong`/`required` |

[PUBLIC_TYPE_SCOPE]: typed solver-fault family

Every solver-state failure is a distinct exception carrying the offending object on `__slots__`, so the `rasm.runtime.faults` boundary discriminates the fault kind on type and carried object, never on a message string.

| [INDEX] | [SYMBOL]                  | [SLOT]          | [RAISED_BY]                                                             |
| :-----: | :------------------------ | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `UnsatisfiableConstraint` | `constraint`    | `addConstraint` when a new required constraint conflicts                |
|  [02]   | `DuplicateConstraint`     | `constraint`    | `addConstraint` for an already-added constraint                         |
|  [03]   | `UnknownConstraint`       | `constraint`    | `removeConstraint`/`hasConstraint` for a never-added constraint         |
|  [04]   | `BadRequiredStrength`     | (none)          | `addEditVariable` with `required` strength (edits must be non-required) |
|  [05]   | `DuplicateEditVariable`   | `edit_variable` | `addEditVariable` for an already-added edit variable                    |
|  [06]   | `UnknownEditVariable`     | `edit_variable` | `suggestValue`/`removeEditVariable` for a non-edit variable             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Variable` / `Term` / `Expression` construction and linear algebra

`Variable("x")` is the free symbol; the linear form is built by operator overloading, never a constructor call — `2 * x` is a `Term`, `2 * x + y - 30` an `Expression`, and a relational operator turns an expression into a `Constraint` whose rhs is a `float`/`Variable`/`Term`/`Expression`. `__eq__`/`__le__`/`__ge__` produce a `Constraint`; `__ne__`/`__gt__`/`__lt__` are `NoReturn`, since Cassowary models `==`/`<=`/`>=` only.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------------- |
|  [01]   | `Variable(name="", context=None)`                             | ctor     | named free unknown; `context` holds an opaque payload    |
|  [02]   | `Variable.value()`                                            | instance | solved value after `updateVariables()` (`0.0` pre-solve) |
|  [03]   | `Variable.name()` / `setName(name)`                           | instance | get/set the name surfaced in `dumps()` provenance        |
|  [04]   | `Variable.context()` / `setContext(obj)`                      | instance | get/set the opaque context object threaded through       |
|  [05]   | `var * c` / `c * var` -> `Term`                               | operator | scale a variable to a coefficient `Term`                 |
|  [06]   | `var + other` / `var - other` -> `Expression`                 | operator | sum variables/terms/floats into an `Expression`          |
|  [07]   | `Term(variable, coefficient=1.0)`                             | ctor     | explicit `Term`; `coefficient()`/`variable()`/`value()`  |
|  [08]   | `Expression(terms, constant=0.0)`                             | ctor     | explicit `Expression`; `terms()`/`constant()`/`value()`  |
|  [09]   | `expr == rhs` / `expr <= rhs` / `expr >= rhs` -> `Constraint` | operator | build a `Constraint` from an expression + relational op  |

[ENTRYPOINT_SCOPE]: `Constraint` and `strength` priority

A `Constraint` defaults to `required` strength; `constraint | strength` or the constructor `strength=` lowers it to a band or numeric priority. `strength.create(a, b, c, weight=1.0)` blends the strong/medium/weak slots (×10^6, ×10^3, ×1, each clamped to its band and scaled by `weight`) for a custom priority between the canonical bands. `required` constraints must be satisfiable or `addConstraint` raises `UnsatisfiableConstraint`; non-required constraints minimize as soft objectives.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `Constraint(expression, op, strength="required")`  | ctor     | explicit build; `op` `"=="`/`"<="`/`">="`, `strength` float/band |
|  [02]   | `constraint \| strength`                           | operator | lower a constraint's strength (`(x==y) \| "weak"`)               |
|  [03]   | `Constraint.expression()` / `op()` / `strength()`  | instance | the constraint's expression, relational op, numeric strength     |
|  [04]   | `Constraint.violated()`                            | instance | unsatisfied in the current solver state (post-solve QA)          |
|  [05]   | `strength.weak` / `medium` / `strong` / `required` | property | the four priority floats (`1.0`/`1e3`/`1e6`/`1001001000.0`)      |
|  [06]   | `strength.create(a, b, c, weight=1.0)`             | factory  | symbolic priority between bands (`create(0,1,0,2)` = 2×medium)   |

[ENTRYPOINT_SCOPE]: `Solver` lifecycle and edit-variable re-solve

Solving is two-phase: register constraints and edit variables, then `updateVariables()` writes the solved `value()` into every `Variable`. Edit variables (`addEditVariable` + `suggestValue` + `updateVariables`) are the interactive re-solve path — suggest a new value for a dragged handle or resized sheet and re-solve incrementally without rebuilding the system. `dump()`/`dumps()` serialize the full solver internals (objective, tableau, variable bindings, edit variables, constraints) for a layout-debug receipt.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `Solver()`                                   | ctor     | a fresh empty constraint system                                       |
|  [02]   | `Solver.addConstraint(constraint)`           | instance | add a constraint; raises `Duplicate`/`Unsatisfiable`                  |
|  [03]   | `Solver.removeConstraint(constraint)`        | instance | remove a constraint; raises `UnknownConstraint`                       |
|  [04]   | `Solver.hasConstraint(constraint)`           | instance | whether the solver contains the constraint                            |
|  [05]   | `Solver.addEditVariable(variable, strength)` | instance | register a non-`required` edit variable; raises `BadRequiredStrength` |
|  [06]   | `Solver.removeEditVariable(variable)`        | instance | drop an edit variable; raises `UnknownEditVariable`                   |
|  [07]   | `Solver.hasEditVariable(variable)`           | instance | whether the solver contains the edit variable                         |
|  [08]   | `Solver.suggestValue(variable, value)`       | instance | suggest a desired value; raises `UnknownEditVariable`                 |
|  [09]   | `Solver.updateVariables()`                   | instance | write the solved values into every `Variable`                         |
|  [10]   | `Solver.reset()`                             | instance | clear all constraints and edit variables to empty                     |
|  [11]   | `Solver.dump()` / `dumps()`                  | instance | print / return the full solver-internals text                         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Solver` is the single mutable constraint-system owner: `addConstraint`/`removeConstraint`/`addEditVariable` mutate in place and return `None`, and `updateVariables()` is the one solve commit that writes `Variable.value()` — never a per-constraint-kind solver method family, never a fluent chain. One `Solver` per laid-out diagram or sheet.
- Operator overloading builds the linear form (`c * var` -> `Term`, `+`/`-` -> `Expression`, `==`/`<=`/`>=` -> `Constraint`); an alignment or separation rule is one expression-operator line, never a hand-built `Term`/`Expression` tuple or tableau row.
- Hard layout rules take `required` (the operator default); aesthetic preferences take `weak`/`medium`/`strong` via `constraint | "weak"` or `strength.create`, minimized as soft objectives, so an over-constrained layout resolves through the strength hierarchy, never conditional add/remove branching per case.
- Interactive re-layout registers the moved coordinate as a non-`required` edit variable and re-solves via `suggestValue` + `updateVariables` incrementally.
- `Variable`/`Term`/`Expression` are unhashable (`__hash__ = None`); key a per-node `Variable` map on the stable `rustworkx` integer node index (the key `visualization/diagram/layout#LAYOUT` and `visualization/diagram/glyphset#GLYPHSET` share), reading `var.value()` back into the `LayoutMap`.
- `Solver` build and `updateVariables()` solve are synchronous native work run inside the `visualization/diagram/layout#LAYOUT` `_render` kernel offloaded onto `anyio.to_thread.run_sync` under its `CapacityLimiter`; the solve never runs on the loop thread, and a `Solver` never crosses an await boundary or is shared across threads.
- Every constraint-layout op surfaces `__kiwi_version__` (the C++ Kiwi algorithm provenance, distinct from the wrapper `__version__`) on its receipt, never assuming the two match.

[STACKING]:
- `visualization/diagram/layout#LAYOUT` — `Constrained(rules)` is the constraint-layout arm of the `LayoutPolicy` `expression.tagged_union` (sibling to `Force`/`Radial`/`Layered`/`Projected`): it builds one `Solver`, maps each stable `rustworkx` node index to an `x`/`y` `Variable` pair, folds per-kind alignment/distribution/separation rules (`required` grid snapping, `weak`/`medium` centering and equal-gap) into `addConstraint`, calls `updateVariables()`, and reads `var.value()` into the `LayoutMap` — one more `_position` match arm keyed on the shared node index, never a parallel layout owner.
- `rasm.runtime.faults` (shared rail) — `async_boundary` (wrapping the layout `_compute`) catches the typed solver-fault family and maps each fault with its carried `constraint`/`edit_variable` onto a `RuntimeRail` case, so an over-constrained diagram returns a typed `Result` fault rather than crashing the render.
- `composition/sheet#SHEET` — viewport/grid alignment math runs the same Cassowary system: sheet extents and viewport bounds become edit variables, frame and title-block field positions become `required`/`weak` constraints, and `suggestValue` re-solves the field layout on a sheet-size change — one `Solver` shared with the diagram pattern, not a second alignment engine.
- `composition/compose#COMPOSE` — `arranged(extents, region, rules)` folds the `Rule` vocabulary (`align`/`chain`/`pin`/`center`/`inside`) onto one `Solver` (containment `required`, top-left settling `weak`, centering `medium`); `FigureOp.Arrange` consumes it over SVG extents and `composition/sheet#SHEET` `FigurePlacement.arranged` over PDF extents, so figure auto-layout is one Cassowary owner.
- `libs/python/.api/numpy` (shared rail) — solved `var.value()` coordinates collect into a `numpy` `float64` `(N, 2)` point matrix consumed by `visualization/diagram/glyphset#GLYPHSET` marks and `data/tabular#GRAPH` route geometry, the same numeric substrate the force/radial/layered policies produce, never a parallel coordinate representation.
- `libs/python/.api/msgspec` + `libs/python/.api/structlog` (shared rails) — layout-evidence receipt (`msgspec.Struct`: variable/constraint/edit-variable counts, `__kiwi_version__`, `violated()` tally) is the corpus receipt shape, and the `violated()` soft-constraint count with the `dumps()` tableau ride a `structlog` debug event under the layout span, contributing the value `"cassowary"` to the `visualization/diagram/layout#LAYOUT` `ArtifactReceipt.Diagram` `algorithm` slot.

[LOCAL_ADMISSION]:
- kiwisolver is the sole constraint-solve owner behind the `visualization/diagram/layout#LAYOUT` `Constrained` policy arm and the `composition/sheet#SHEET`/`composition/compose#COMPOSE` alignment math, every solver fault crossing `rasm.runtime.faults` and the solve offloaded onto the layout `CapacityLimiter` thread.

[RAIL_LAW]:
- Package: `kiwisolver`
- Owns: incremental Cassowary linear-constraint solving (add/remove/edit-variable re-solve), the operator-built `Variable`/`Term`/`Expression` algebra, `Constraint` construction with the four-band `strength` hierarchy and `strength.create` symbolic blending, constraint-violation introspection, and full solver-internals serialization.
- Accept: the constraint-layout policy arm of `visualization/diagram/layout#LAYOUT` (alignment/distribution/anchoring/minimum-separation/symmetry/aspect over the stable `rustworkx` node index), the `composition/sheet#SHEET`/`composition/compose#COMPOSE` alignment math, and any interactive re-layout that re-solves on a suggested value.
- Reject: a hand-rolled simplex/LP where `Solver` exists; graph topology or force/radial/hierarchical placement (`rustworkx`/`grandalf` behind `visualization/diagram/layout#LAYOUT`); SVG emission (`visualization/diagram/draw#DRAW`); a `Variable`-keyed dict where the stable node index is the key; a from-scratch `Solver` rebuild per interactive frame; a bare escaping solver exception; a synchronous solve left on the event loop.
