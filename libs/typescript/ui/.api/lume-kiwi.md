# [TS_UI_API_LUME_KIWI]

`@lume/kiwi` is the zero-dependency Cassowary (incremental simplex) solver the `viewer/panel/layout` row runs to re-solve the ordered `LayoutConstraintWire` program authored by `csharp:Rasm.AppUi/Shell/solver`. The seam contract is bit-identical geometry, not a plausible layout: the panel decodes the wire program once at `wire` (through the `#vocab` subpath), folds its ordered variables/terms/constraints into one `Solver`, and reads `Variable.value()` back — the TS replay MUST land on the same positions the C# solver produced, so the folder re-solves for local interaction latency while the C# side stays authoritative. A peer re-mint of a *different* constraint program (reordered, re-strengthed, or algebra-rebuilt in TS) is the `CROSS_LANGUAGE_WIRE` drift defect, mirroring the appearance-seam law. Pure CPU (no DOM, no GL), `scope:viewer` project-local, compile-time excluded from the core `ui`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@lume/kiwi`
- package: `@lume/kiwi`
- license: `BSD-3-Clause`
- deps: none — self-contained ESM Cassowary port (`kiwi.d.ts` barrel re-exports the module set); no peer, no runtime
- catalog-verdict: KEEP
- runtime: `scope:viewer` project-local, CPU-only — admitted by the `ui/viewer` Nx project alone; no browser Web API, GL context, or React binding of its own
- modules: `Variable`, `Expression`, `Operator`, `Constraint`, `Strength`, `Solver` (barrelled through `kiwi.d.ts`); `IMap`/`createMap` are the internal id-keyed associative map

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: constraint algebra + tableau solver
- rail: viewer/panel/layout
- The value algebra is built bottom-up: a `Variable` is a named cell; `plus`/`minus`/`multiply`/`divide` fold `Variable`s and constants into an `Expression` (linear terms + constant, RHS implicitly zero); a `Constraint` pairs an `Expression` with an `Operator` and a `Strength`; the `Solver` holds the tableau and re-solves incrementally. Strength is a numeric algebra, not a fixed enum: `Strength.create(strong, medium, weak, weight?)` produces any symbolic strength and the four named constants are seed rows of that algebra.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `Variable(name?)` — `name`/`setName`, `value`, `plus`/`minus`/`multiply`/`divide` | value cell | one wire layout variable; `value()` is the solved position read back |
| [02] | `Expression(...args)` — args `number \| Variable \| Expression \| [coeff, Variable\|Expression]` | linear form | wire term-list folded to `Σ coeff·var + const`; RHS implicitly zero |
| [03] | `Operator` — `Le = 0` / `Ge = 1` / `Eq = 2` | relation enum | the wire constraint's relational operator (`<=` / `>=` / `==`) |
| [04] | `Constraint(expr, op, rhs?, strength?)` — `expression`/`op`/`strength` | equation | one decoded wire constraint row; `strength` defaults to `Strength.required` |
| [05] | `Strength.create(a, b, c, w?)` / `required` / `strong` / `medium` / `weak` | strength algebra | symbolic strength as a `(strong, medium, weak, weight)` fold; named values are seed rows |
| [06] | `Solver` — `maxIterations` (default `10000`), the constraint + edit surface | tableau solver | the one re-solve engine; `maxIterations` bounds pathological programs |
| [07] | `IMap<T,U>` / `createMap` (id-keyed) | internal map | solver-internal id-indexed associative store; not a consumer surface |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fold the wire program into a solver, re-solve, read positions
- rail: viewer/panel/layout
- The panel builds the algebra in the wire program's order, then drives the solver. Two constraint entrypoints exist and are equivalent: `new Constraint(...)` + `solver.addConstraint(c)`, or `solver.createConstraint(lhs, op, rhs, strength?)` which constructs and adds in one call. Edit variables model live drag: register with a sub-required strength, `suggestValue` per frame, `updateVariables` to re-solve.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------- |:-------------- |:---------------------------------------------------------------- |
| [01] | `new Variable(name)` per wire variable, in wire order | build vars | one `Variable` per decoded wire cell; names mirror the C# program |
| [02] | `v.multiply(coeff).plus(other)…` / `new Expression([coeff, v], …, const)` | build expr | fold the wire term-list into the constraint LHS |
| [03] | `Strength.create(a, b, c, w?)` / `Strength.required\|strong\|medium\|weak` | resolve strength | map the wire strength field to the exact numeric strength |
| [04] | `solver.createConstraint(lhs, op, rhs, strength?)` → `Constraint` | add constraint | construct-and-add in wire order; equivalent to `new Constraint`+`addConstraint` |
| [05] | `solver.addConstraint(c)` / `removeConstraint(c)` / `hasConstraint(c)` / `getConstraints()` | mutate set | incremental add/remove; `hasConstraint` guards idempotent replay |
| [06] | `solver.addEditVariable(v, strength)` / `removeEditVariable(v)` / `hasEditVariable(v)` | edit register | live-drag variables; `strength` MUST be `< Strength.required` |
| [07] | `solver.suggestValue(v, value)` then `solver.updateVariables()` | suggest + solve | per-frame drag target then incremental re-solve of the tableau |
| [08] | `v.value()` after `updateVariables()` | read position | the solved coordinate the panel binds; the seam-verified output |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- one solver, incremental: `Solver` holds a simplex tableau; `addConstraint`/`removeConstraint`/`suggestValue` mutate it and `updateVariables()` re-optimizes without a full rebuild. `maxIterations` (default `10000`) caps iteration to fail loud on a pathological program rather than hang.
- strength is numeric, not categorical: `required` is a hard constraint (the solver guarantees it or throws on an unsatisfiable required set); `strong`/`medium`/`weak` and any `create(a,b,c,w)` value are soft — the solver minimizes weighted violation. Edit variables reject `required` strength; a required edit is a construction error.
- RHS-implicit-zero: a `Constraint` normalizes to `expr op 0`; `createConstraint(lhs, op, rhs, …)` folds `lhs − rhs` into that form. `suggestValue` only applies to a registered edit variable.

[DETERMINISM_LAW]:
- identical positions require identical inputs in identical order: Cassowary's solution is unique for a fully-determined system, but under equal-strength competition the result depends on constraint INSERTION ORDER and edit/stay weights. The seam therefore fixes all The axes — identical constraint set, identical insertion order, identical strengths, identical edit-suggestion sequence — and the wire program is *ordered* precisely so the TS replay reproduces the C# tableau.
- the wire carries the program, `ui` never authors it: the panel folds decoded rows in the received order and MUST NOT insert, reorder, re-strengthen, or synthesize constraints locally; interactive drag adds only edit-variable `suggestValue` calls over the frozen program, never new structural constraints.
- floating-point parity is the seam's tolerance contract, not kiwi's concern: both sides run the same Cassowary algorithm, so equal-order equal-strength programs converge to the same tableau; any observed drift is a program-construction defect (wrong order/strength/algebra), surfaced at the panel, never papered over with a re-solve-until-close loop.

[INTEGRATION_LAW]:
- Stack with `wire#vocab`: the `LayoutConstraintWire` is decoded once at `wire` (Schema-typed, `.api/effect.md`), and `ui` types the decoded ordered program through the `wire` `#vocab` subpath — the codec interior is unexported, so the panel physically cannot re-parse or re-mint the program. The fold consumes decoded values only.
- Stack with `@effect-atom` (`.api/effect-atom-atom.md`): the solved `Variable.value()` set binds through the one panel atom (`ONE_FOLD_ONE_BINDING`); `updateVariables()` runs inside the atom's write fold so a suggested value and its re-solve land as one state transition the panel view reads.
- Stack with the gesture rows (`act/gesture`, react-aria / `.api/use-gesture-react.md`): a drag gesture is the only writer into `suggestValue`; the gesture delta targets an edit variable, `updateVariables()` re-solves, and the atom re-renders — one gesture → one suggest → one solve → one bind.
- Stack with `csharp:Rasm.AppUi/Shell/solver`: C# owns the authoritative solve and serializes the ordered program; TS re-solves for local latency. The two are the same Cassowary algorithm over the same ordered program — a TS-side algebra rebuild that diverges from the wire order is the `CROSS_LANGUAGE_WIRE` drift defect.

[LOCAL_ADMISSION]:
- `scope:viewer` project-local; the core `ui` never imports it. No DOM/GL — pure numeric solve.
- the wire program is the single source of constraints; `ui` adds only edit-variable suggestions, never structural constraints.
- one `Solver` per layout panel; strengths are resolved through `Strength.create`/the named constants, never hardcoded magic numbers.

[RAIL_LAW]:
- Package: `@lume/kiwi`
- Owns: the incremental Cassowary re-solve of the ordered wire `LayoutConstraintWire` program, the `Variable`/`Expression`/`Constraint`/`Strength` algebra, and the edit-variable drag protocol
- Accept: one `Solver` folding the decoded program in wire order, `Strength.create`/named constants for every strength, `addEditVariable`+`suggestValue`+`updateVariables` for live drag, `Variable.value()` as the seam-verified position read
- Reject: authoring/reordering/re-strengthening constraints in `ui`, a TS-side algebra re-mint that diverges from the wire order, `required`-strength edit variables, a re-solve-until-close loop hiding a construction defect, importing it into the core `ui`, hand-rolling a linear solver
