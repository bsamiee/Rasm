# [UI_LAYOUT]

`viewer/panel/layout.ts` re-solves the C#-authored layout program (AU:83) to IDENTICAL positions: the ordered `LayoutProgram` arrives decoded through `wire/vocab` — variables implied by term order, constraints as `(terms, relation, strength, constant)` rows, edit names enumerated — and this module folds it into one `@lume/kiwi` `Solver` in EXACTLY the wire order, reads `Variable.value()` back as the panel geometry, and models live drag as edit-variable suggestions over the frozen program. The seam contract is bit-identical geometry, not a plausible layout: same constraint set, same insertion order, same strengths, same edit sequence — the four determinism axes — and any observed drift is a program-construction defect surfaced loud, never papered over with a re-solve-until-close loop.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                       |
| :-----: | :------------- | :------------------------------------------------------------------------------ |
|   [1]   | `PROGRAM_FOLD` | the wire-order fold — program → solver → positions, with the unsatisfiable fault |
|   [2]   | `DRAG_EDIT`    | edit-variable suggestions per frame over the frozen program                      |
|   [3]   | `DETERMINISM`  | the four-axes law and the drift-evidence policy                                  |

## [2]-[PROGRAM_FOLD]

- Owner: `PanelLayout.solve(program)` — the one fold: walk `program.constraints` in received order, minting each `Variable` at FIRST APPEARANCE (an interior name→`Variable` ledger — first-appearance order is the wire's variable order by construction), fold each constraint's `terms` into an `Expression` (`Σ coeff·var + constant`), map the closed `relation` vocabulary (`le`/`ge`/`eq`) onto `Operator` and the closed `strength` vocabulary (`required`/`strong`/`medium`/`weak`) onto the `Strength` constants, `addConstraint` in order, register `program.edits` as edit variables at `Strength.strong` (sub-required by kiwi's own law), run `updateVariables()`, and read every variable's `value()` into the positions map.
- Packages: `@lume/kiwi` (`Variable`, `Expression`, `Operator`, `Constraint`, `Strength`, `Solver` — zero-dependency Cassowary, CPU-only), `#vocab` (`LayoutProgram` — `surface`, `edits`, ordered `constraints`), `effect` (`Effect.try`, `HashMap`).
- Fault: `PanelLayout.Fault` — an unsatisfiable required set throws inside kiwi; the fold catches it into the one tagged fault carrying the surface name and the offending constraint's rank — a program-construction defect surfaced to the operator as evidence, never retried.
- Law: the solver is the imperative foreign resource — the fold's statements (the name ledger, the ordered walk) live inside its `Effect.try` seam, the interior `Map` is the kernel's draft holding kiwi cells only, and the sole escape is the immutable `HashMap` positions read; this card carries the statement-seam exemption.
- Law: the fold inserts, never authors — no constraint is synthesized, reordered, re-strengthened, or dropped; TS-side layout intelligence is the `CROSS_LANGUAGE_WIRE` drift defect this page's whole existence guards against.
- Law: `maxIterations` stays at kiwi's default — a pathological program fails loud through the iteration cap; tuning it to make a bad program pass hides the upstream defect.
- Growth: a new constraint kind, variable class, or strength tier is a C# solver change mirrored at `wire` — this fold's vocabulary maps grow a row each, nothing else moves.

```typescript
import { Constraint, Expression, Operator, Solver, Strength, Variable } from "@lume/kiwi"
import type { LayoutProgram } from "#vocab"
import { Data, Effect, HashMap, Iterable } from "effect"

const _relations = { le: Operator.Le, ge: Operator.Ge, eq: Operator.Eq } as const

const _strengths = {
  required: Strength.required,
  strong: Strength.strong,
  medium: Strength.medium,
  weak: Strength.weak,
} as const

class SolveFault extends Data.TaggedError("SolveFault")<{
  readonly surface: string
  readonly rank: number
  readonly detail: string
}> {}

declare namespace PanelLayout {
  type Positions = HashMap.HashMap<string, number>
  type Solved = {
    readonly positions: Positions
    readonly suggest: (edit: string, value: number) => Effect.Effect<Positions, SolveFault>
  }
}

const _solve = (program: LayoutProgram): Effect.Effect<PanelLayout.Solved, SolveFault> =>
  Effect.gen(function* () {
    const solver = new Solver()
    const cells = new Map<string, Variable>()
    const named = (name: string): Variable => {
      const held = cells.get(name) ?? new Variable(name)
      cells.set(name, held)
      return held
    }
    yield* Effect.try({
      try: () => {
        program.constraints.forEach((row) => {
          const terms = row.terms.map((term): [number, Variable] => [term.coefficient, named(term.variable)])
          const lhs = new Expression(...terms, row.constant)
          solver.addConstraint(new Constraint(lhs, _relations[row.relation], undefined, _strengths[row.strength]))
        })
        program.edits.forEach((edit) => solver.addEditVariable(named(edit), Strength.strong))
        solver.updateVariables()
      },
      catch: (defect) => new SolveFault({ surface: program.surface, rank: solver.getConstraints().length, detail: String(defect) }),
    })
    const read = (): PanelLayout.Positions =>
      HashMap.fromIterable(Iterable.map(cells, ([name, cell]) => [name, cell.value()] as const))
    return {
      positions: read(),
      suggest: (edit, value) =>
        Effect.try({
          try: () => {
            solver.suggestValue(named(edit), value)
            solver.updateVariables()
            return read()
          },
          catch: (defect) => new SolveFault({ surface: program.surface, rank: 0, detail: String(defect) }),
        }),
    }
  })
```

## [3]-[DRAG_EDIT]

- Law: drag is suggestion, never structure — a pointer drag on a panel edge feeds `suggest(edit, value)` per frame (the gesture source is `act/gesture`'s continuous owner; the target value arrives already in panel coordinates through its `transform`); the frozen program re-optimizes incrementally and every dependent position updates in the same read — no constraint is added or removed during interaction.
- Law: only wire-enumerated edits are suggestible — `program.edits` is the closed set; a suggestion against a non-edit variable is a construction error kiwi rejects, surfaced through the same fault.
- Law: positions flow to render as one atom write per settle — the returned map replaces the positions atom (`Atom.batch` coalesces multi-panel updates), and panel components read their own cell through a selector; per-panel subscriptions keep a 60fps drag from re-rendering the board.

## [4]-[DETERMINISM]

- Law: the four axes are fixed by construction — identical constraint SET (the fold inserts every row), identical insertion ORDER (the walk is the wire order), identical STRENGTHS (the closed map), identical EDIT sequence (suggestions replay in gesture order) — so the TS tableau converges to the C# tableau; equal-strength competition resolves identically because insertion order is preserved.
- Law: drift is evidence, not tolerance — a position mismatch against a C#-provided expectation is reported (the probe plane consumes it) with the variable name and both values; a fuzzy-match re-solve loop is the named defect.
- Boundary: the ordered program's decode and its golden fixtures are `wire/codec/layout`'s; drag gestures are `act/gesture`'s; position display is the panel shell's.

```typescript
const PanelLayout: {
  readonly Fault: typeof SolveFault
  readonly relations: typeof _relations
  readonly strengths: typeof _strengths
  readonly solve: typeof _solve
} = {
  Fault: SolveFault,
  relations: _relations,
  strengths: _strengths,
  solve: _solve,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { PanelLayout }
```
