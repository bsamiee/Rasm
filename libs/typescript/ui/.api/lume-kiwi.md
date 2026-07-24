# [TS_UI_API_LUME_KIWI]

`@lume/kiwi` owns the incremental Cassowary simplex re-solve the `viewer/panel/layout` row runs over the ordered `LayoutConstraintWire` program: fold the decoded variables, terms, and constraints into one `Solver`, drive it, read `Variable.value()` back.

C# `Rasm.AppUi/Shell/solver` produces the authoritative solve; the TS replay reproduces its tableau bit-for-bit over the identical ordered program, so positional drift is a program-construction defect, never a re-solve tolerance.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@lume/kiwi`
- package: `@lume/kiwi` (BSD-3-Clause)
- module: `@lume/kiwi` ESM barrel (`kiwi.d.ts`), self-contained; no peer, no runtime dependency
- runtime: CPU-only, no DOM, GL context, or React binding; the `ui/viewer` Nx project admits it and the core `ui` never imports it
- rail: viewer/panel/layout

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: constraint algebra and incremental tableau solver

Bottom-up, `Variable` cells fold through `plus`/`minus`/`multiply`/`divide` into an `Expression`, a `Constraint` pairs that `Expression` with an `Operator` and a numeric `Strength`, and the `Solver` holds the tableau.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Variable(name?)`                       | value cell    | `plus`/`minus`/`multiply`/`divide` build an `Expression`                |
|  [02]   | `Expression(...args)`                   | linear form   | the constraint LHS from summed `number`/`Variable`/`[coeff, var]` terms |
|  [03]   | `Operator`                              | enum          | wire relational operator `Le`/`Ge`/`Eq` = `0`/`1`/`2`                   |
|  [04]   | `Constraint(expr, op, rhs?, strength?)` | class         | one decoded constraint row; `strength` defaults to `required`           |
|  [05]   | `Strength.create(a, b, c, w?)`          | class         | numeric strength algebra seeding `required`/`strong`/`medium`/`weak`    |
|  [06]   | `Solver`                                | class         | the incremental simplex re-solve engine                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fold the wire program into a solver, re-solve, read positions

`ui` folds the algebra in wire order, then drives the solver; members below sit on `Solver` unless prefixed. `new Constraint(...)`+`addConstraint(c)` and `createConstraint(lhs, op, rhs, strength?)` are equivalent, the latter constructing and adding in one call. Live drag registers an edit variable at sub-`required` strength, calls `suggestValue` per frame, and `updateVariables()` re-solves.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `new Variable(name)`                                     | ctor     | one `Variable` per decoded wire cell, in wire order     |
|  [02]   | `new Expression([coeff, v], …)`                          | ctor     | fold the wire term-list into the constraint LHS         |
|  [03]   | `Strength.create(a, b, c, w?)`                           | static   | map the wire strength field to a numeric strength       |
|  [04]   | `createConstraint(lhs, op, rhs, strength?)`              | instance | construct-and-add one constraint, in wire order         |
|  [05]   | `addConstraint(c)` / `removeConstraint(c)`               | instance | incremental add or remove of a constraint               |
|  [06]   | `hasConstraint(c)` / `getConstraints()`                  | instance | `hasConstraint` guards idempotent replay                |
|  [07]   | `addEditVariable(v, strength)` / `removeEditVariable(v)` | instance | register live-drag variables at `strength` `< required` |
|  [08]   | `hasEditVariable(v)`                                     | instance | test whether a variable is an edit variable             |
|  [09]   | `suggestValue(v, value)` then `updateVariables()`        | instance | per-frame drag target, then incremental re-solve        |
|  [10]   | `v.value()` after `updateVariables()`                    | instance | the solved coordinate the panel binds — the seam output |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one incremental solver: `Solver` holds a simplex tableau; `addConstraint`/`removeConstraint`/`suggestValue` mutate it and `updateVariables()` re-optimizes without a rebuild, while `maxIterations` (default `10000`) caps iteration to fail loud on a pathological program.
- strength is numeric, not categorical: `required` is hard and the solver satisfies it or throws on an unsatisfiable required set; `strong`/`medium`/`weak` and any `Strength.create(a, b, c, w)` value are soft, minimizing weighted violation, and an edit variable rejects `required` strength.
- `Constraint` normalizes to `expr op 0`; `createConstraint(lhs, op, rhs, …)` folds `lhs − rhs` into that form, and `suggestValue` applies only to a registered edit variable.
- identical ordered inputs yield identical positions: under equal-strength competition the tableau depends on constraint insertion order and edit weights, so the seam fixes every axis — constraint set, insertion order, strengths, edit-suggestion sequence — and both sides run the same Cassowary algorithm to one tableau; drift surfaces at the panel as a construction defect, never a re-solve-until-close loop.

[STACKING]:
- `wire`#vocab (`effect` Schema, `libs/typescript/.api/effect.md`): `ui` types the decoded ordered program through the `wire` `#vocab` subpath; the codec interior stays unexported, so the panel cannot re-parse or re-mint the program and the fold consumes decoded values only.
- `@effect-atom`(`.api/effect-atom-atom.md`): the solved `Variable.value()` set binds through one panel atom, and `updateVariables()` runs inside the atom's write fold so a suggested value and its re-solve land as one state transition.
- `use-gesture-react`(`.api/use-gesture-react.md`): a drag gesture is the sole writer into `suggestValue`, its delta targeting an edit variable so `updateVariables()` re-solves and the atom re-renders — one gesture, one suggest, one solve, one bind.
- `csharp:Rasm.AppUi/Shell/solver`: C# owns the authoritative solve and serializes the ordered program; TS re-solves the identical program for local latency, and a TS-side algebra rebuild that diverges from wire order is the `CROSS_LANGUAGE_WIRE` drift defect.
- within-lib: one `Solver` per layout panel folds the decoded `LayoutConstraintWire` in received order, and `ui` adds only edit-variable suggestions over the frozen program.

[LOCAL_ADMISSION]:
- `scope:viewer` project-local, pure numeric solve with no DOM or GL; the core `ui` never imports it.
- `ui` draws every constraint from the wire program and adds only edit-variable suggestions, never a structural constraint.
- strengths resolve through `Strength.create` or the named constants, never a hardcoded magic number.

[RAIL_LAW]:
- Package: `@lume/kiwi`
- Owns: the incremental Cassowary re-solve of the ordered `LayoutConstraintWire` program, the `Variable`/`Expression`/`Constraint`/`Strength` algebra, and the edit-variable drag protocol
- Accept: one `Solver` folding the decoded program in wire order, `Strength.create` or named constants for every strength, `addEditVariable`+`suggestValue`+`updateVariables` for live drag, `Variable.value()` as the seam-verified position read
- Reject: authoring/reordering/re-strengthening constraints in `ui`, a TS-side algebra re-mint diverging from wire order, `required`-strength edit variables, a re-solve-until-close loop hiding a construction defect, importing it into the core `ui`, hand-rolling a linear solver
