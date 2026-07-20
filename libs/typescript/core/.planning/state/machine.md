# [CORE_MACHINE]

The statechart owner: a closed transition system is data — one `Transition.Spec` whose `nodes` table declares the state tree (atomic, compound, parallel, final, history — declaration order IS document order and document order is the determinism law) and whose `rows` carry guarded, internally-or-externally-domained, ordered-emit transitions — and one accumulated admission at `Transition.spec` that refuses invalid topology before precomputing the tree algebra (ancestor chains, entry completion, LCCA, the final-state census), deriving the configuration schema from the node vocabulary, and minting the serializable `@effect/experimental` `Machine` exactly once, so `boot` and `restore` only run an admitted actor and never recompile. The same compiled value drives three altitudes: the pure macrostep fold (`step` drains eventless and raised internal signals under a bounded-microstep fuel row and returns an `Either` whose `Spent` left rail carries exhaustion), the batch driver (`drive` folds the same rail without advancing past a left), the stream driver (`trace` lifts that rail through `Stream.mapAccumEffect`), and the booted actor — one state on one fiber, phase-keyed watchdogs and node-scoped invoke fibers armed by the entered/exited wave with completions folded through the node's `finalize` row before the done signal fires, history carried inside machine state so `snapshot`/`restore` transports it durably for free, the actor's own `Subscribable` state binding view atoms, and a fact stream published from the macrostep's own receipt on the send path — the inspection hook a consumer taps while the machine forks nothing of its own. The flat Mealy table is the degenerate case — a depth-one tree with a singleton configuration. THE ALTITUDE RULING: `Machine` is the in-process serializable actor, and serializability is forced rather than asserted — `snapshot`/`restore` and the `sendUnknown` wire admission exist only on the schema-carried `Machine.serializable` list, because the schemaless `Machine.procedures` altitude forfeits exactly the durability these laws demand; a machine whose steps demand durable-execution replay, activity memoization, compensation, or cross-process sharding is the runtime branch's workflow altitude, and promoting a transition system there re-homes the spec, never re-shapes it. The module is `core/src/state/machine.ts`; a new state is a node row, a new transition is a table row, a new deadline is a watch row, a new child activity is an invoke row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                  | [PUBLIC]                               |
| :-----: | :----------------- | :---------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `STATECHART_TABLE` | node/row/config vocabulary, one-shot compile, static tree facts         | `Transition.Spec`, `Transition.spec`   |
|  [02]   | `MACROSTEP_FOLD`   | selection, conflict removal, exit/entry algebra, fuel-bounded macrostep | `Transition.drive`, `Transition.trace` |
|  [03]   | `ACTOR`            | boot, restore, wire admission, arming, subscribable state, fact stream  | compiled `boot`/`restore`              |

## [02]-[STATECHART_TABLE]

[STATECHART_TABLE]:
- Owner: `Transition.Spec<Id, S, V, X>` — the machine as one value: `name`, `nodes` (the kind-discriminated state tree; declaration order is SCXML document order), `rows` (the transition matrix in document order), the `signal`/`verdict` literal schemas, the `extended` schema plus `seed` (guard-readable extended state, snapshot-serializable), `fuel` (the bounded-microstep row that makes every macrostep terminate), `lag` (the fact-hub sliding capacity — a slow inspector sheds oldest facts, never backpressures the actor), `traced` (actor span emission as a definition fact), `recover` (the defect re-initialization `Schedule`). `Transition.spec` accumulates topology and policy issues into `DefinitionFault`, then compiles the admitted value once into `Transition.Compiled` — static tree facts, the derived configuration and macro schemas, the origin configuration, the pure `step`, and the pre-minted machine with its request classes — so booting then restoring one spec never re-mints request-class identities.
- Law: `nodes` is a closed tagged family — `compound` demands `initial`, `history` demands `depth` and `fallback`, `final` demands `parent` — and relational admission proves exactly one root, acyclic parent chains, direct-child compound initials, in-parent history fallbacks, non-empty parallel regions, and positive integral `fuel`/`lag` before `_facts` can recurse. Every issue is retained on the one `DefinitionFault` rail, so an invalid table never compiles partially and one repair pass sees the entire violated topology. The configuration is the active atomic-leaf set plus the recorded `history` values plus the extended state, and its schema derives from the admitted node vocabulary (`Schema.Literal` over the id roster), so a wire-carried or snapshot-carried configuration decodes against exactly the declared tree.
- Law: guards are pure reads of extended state — `when: (extended) => boolean`, SCXML side-effect-free `cond` — preserving the Mealy character and snapshot determinism; `assign` and each keyed invoke row's `finalize` are the only extended-state writers, each a pure function on its owning row. A node carries any number of keyed `watches` and `invokes`; each invoke declares its failure signal and optionally overrides the success signal, so child failure is statechart data rather than an untyped fiber defect.
- Law: internal signals derive from the id vocabulary — `done.state.${Id}` and `done.invoke.${Id}` are `Schema.TemplateLiteral` members of the signal plane, minted by the fold and the invoke completion, matchable by any row's `on`; a hand-written done-signal literal beside the template is the drift defect.
- Law: the internal-versus-external distinction is one row flag — `internal: true` shrinks the transition domain from the LCCA to the source (SCXML `type="internal"`), the whole semantic difference; external is the default posture.
- Law: `Machine.serializable.add` is the pipeable dual — data-last `(schema, handler) => (self) => self` — with `Machine.serializable.addPrivate` its interior twin whose request never reaches `sendUnknown`; `Machine.procedures.add` is a differently-shaped curried type application, and the two namespaces never substitute.
- Growth: a new state, transition, deadline, or child activity is one row in the owning table; a new machine is one spec value; dynamic child registries beyond node-scoped invoke ride the context's id-addressed `forkOne` — or its state-returning `forkOneWith` twin when the registration itself must advance state — under the same arming law.
- Packages: `@effect/experimental` (`Machine`); `effect` (`Array`, `Duration`, `Effect`, `Either`, `HashSet`, `Option`, `Order`, `ParseResult`, `PubSub`, `Schedule`, `Schema`, `Scope`, `Stream`, `Struct`, `Subscribable`, `Tracer`).

```typescript signature
import { Machine } from "@effect/experimental"
import {
  Array, type Duration, Effect, Either, HashSet, Option, Order, type ParseResult, pipe, PubSub, type Schedule, Schema,
  type Scope, Stream, Struct, Subscribable, type Tracer,
} from "effect"

declare namespace Transition {
  type Depth = "shallow" | "deep"
  type Internal<Id extends string> = `done.state.${Id}` | `done.invoke.${Id}`
  type Signal<Id extends string, S extends string> = S | Internal<Id>
  type Watch<Id extends string, S extends string> = {
    readonly key: string
    readonly after: Duration.DurationInput
    readonly signal: Signal<Id, S>
  }
  type Invoke<Id extends string, S extends string, X> = {
    readonly key: string
    readonly run: (extended: X) => Effect.Effect<unknown, unknown>
    readonly success?: Signal<Id, S>
    readonly failure: Signal<Id, S>
    readonly finalize?: (extended: X, result: unknown) => X
  }
  type Face<V extends string> = { readonly entry?: ReadonlyArray<V>; readonly exit?: ReadonlyArray<V> }
  type Service<Id extends string, S extends string, X> = {
    readonly watches?: ReadonlyArray<Watch<Id, S>>
    readonly invokes?: ReadonlyArray<Invoke<Id, S, X>>
  }
  type Node<Id extends string, S extends string, V extends string, X> =
    | (Face<V> & Service<Id, S, X> & { readonly kind: "atomic"; readonly parent?: Id })
    | (Face<V> & Service<Id, S, X> & { readonly kind: "compound"; readonly parent?: Id; readonly initial: Id })
    | (Face<V> & Service<Id, S, X> & { readonly kind: "parallel"; readonly parent?: Id })
    | (Face<V> & { readonly kind: "final"; readonly parent: Id })
    | { readonly kind: "history"; readonly parent: Id; readonly depth: Depth; readonly fallback: Id }
  type Row<Id extends string, S extends string, V extends string, X> = {
    readonly source: Id
    readonly on?: Signal<Id, S>
    readonly when?: (extended: X) => boolean
    readonly to?: Array.NonEmptyReadonlyArray<Id>
    readonly internal?: boolean
    readonly emit?: ReadonlyArray<V>
    readonly assign?: (extended: X, signal: Option.Option<Signal<Id, S>>) => X
  }
  type Config<Id extends string, X> = {
    readonly active: ReadonlyArray<Id>
    readonly history: Readonly<Record<string, ReadonlyArray<Id>>>
    readonly extended: X
  }
  type Macro<Id extends string, V extends string> = {
    readonly program: ReadonlyArray<V>
    readonly entered: ReadonlyArray<Id>
    readonly exited: ReadonlyArray<Id>
  }
  type Spec<Id extends string, S extends string, V extends string, X> = {
    readonly name: string
    readonly nodes: Readonly<Record<Id, Node<Id, S, V, X>>>
    readonly rows: ReadonlyArray<Row<Id, S, V, X>>
    readonly signal: Schema.Schema<S, S>
    readonly verdict: Schema.Schema<V, V>
    readonly extended: Schema.Schema<X>
    readonly seed: X
    readonly fuel: number
    readonly lag: number
    readonly traced: boolean
    readonly recover: Schedule.Schedule<unknown>
  }
  type Frozen = readonly [unknown, unknown]
  type Fact<Id extends string, V extends string, X> = {
    readonly config: Config<Id, X>
    readonly macro: Macro<Id, V>
  }
  type Actor<Id extends string, S extends string, V extends string, X> = {
    readonly initial: Macro<Id, V>
    readonly feed: (signal: Signal<Id, S>) => Effect.Effect<Macro<Id, V>, Spent>
    readonly feedUnknown: (frame: unknown) => Effect.Effect<Schema.ExitEncoded<unknown, unknown, unknown>, ParseResult.ParseError>
    readonly config: Effect.Effect<Config<Id, X>>
    readonly state: Subscribable.Subscribable<Config<Id, X>>
    readonly facts: Stream.Stream<Fact<Id, V, X>>
    readonly freeze: Effect.Effect<Frozen, ParseResult.ParseError>
  }
  type Compiled<Id extends string, S extends string, V extends string, X> = Spec<Id, S, V, X> & {
    readonly origin: Config<Id, X>
    readonly step: (config: Config<Id, X>, signal: Signal<Id, S>) => Either.Either<readonly [Config<Id, X>, Macro<Id, V>], Spent>
    readonly boot: Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError | Spent, Scope.Scope>
    readonly restore: (frozen: Frozen) => Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError, Scope.Scope>
  }
  type Shape = {
    readonly spec: <Id extends string, S extends string, V extends string, X>(
      spec: Spec<Id, S, V, X>,
    ) => Either.Either<Compiled<Id, S, V, X>, DefinitionFault>
    readonly drive: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => (
      origin: Config<Id, X>,
      signals: ReadonlyArray<Signal<Id, S>>,
    ) => Either.Either<readonly [Config<Id, X>, ReadonlyArray<Macro<Id, V>>], Spent>
    readonly trace: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => <E, R>(signals: Stream.Stream<Signal<Id, S>, E, R>) => Stream.Stream<Macro<Id, V>, E | Spent, R>
  }
  type _Facts<Id extends string> = {
    readonly ids: ReadonlyArray<Id>
    readonly byOrder: Order.Order<Id>
    readonly ancestors: (id: Id) => ReadonlyArray<Id>
    readonly children: (id: Id) => ReadonlyArray<Id>
    readonly leaves: (id: Id, history: Readonly<Record<string, ReadonlyArray<Id>>>) => ReadonlyArray<Id>
    readonly closure: (active: ReadonlyArray<Id>) => ReadonlyArray<Id>
    readonly lcca: (members: Array.NonEmptyReadonlyArray<Id>) => Option.Option<Id>
    readonly finalized: (id: Id, active: ReadonlyArray<Id>) => boolean
  }
}

class Spent extends Schema.TaggedError<Spent>()("Spent", {
  signal: Schema.String,
  fuel: Schema.Int.pipe(Schema.nonNegative()),
}) {
  override get message(): string {
    return `<spent@${this.fuel}> ${this.signal}`
  }
}

const _DEFINITION_ISSUES = ["root", "cycle", "initial", "history", "parallel", "service", "fuel", "lag"] as const
const _DefinitionIssue = Schema.Struct({
  reason: Schema.Literal(..._DEFINITION_ISSUES),
  node: Schema.optionalWith(Schema.String, { as: "Option" }),
})

class DefinitionFault extends Schema.TaggedError<DefinitionFault>()("DefinitionFault", {
  issues: Schema.NonEmptyArray(_DefinitionIssue),
}) {}

const _validated = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
): Either.Either<Transition.Spec<Id, S, V, X>, DefinitionFault> => {
  const ids = Struct.keys(spec.nodes)
  const roots = Array.filter(ids, (id) => spec.nodes[id].parent === undefined)
  const cyclic = (id: Id, trail: HashSet.HashSet<Id>): boolean =>
    HashSet.has(trail, id)
      || Option.match(Option.fromNullable(spec.nodes[id].parent), {
        onNone: () => false,
        onSome: (parent) => cyclic(parent, HashSet.add(trail, id)),
      })
  const below = (id: Id, ancestor: Id, trail: HashSet.HashSet<Id>): boolean =>
    HashSet.has(trail, id)
      ? false
      : Option.match(Option.fromNullable(spec.nodes[id].parent), {
        onNone: () => false,
        onSome: (parent) => parent === ancestor || below(parent, ancestor, HashSet.add(trail, id)),
      })
  const issues = [
    ...(roots.length === 1 ? [] : [{ reason: "root", node: Option.none<string>() }] as const),
    ...Array.flatMap(ids, (id) => {
      const node = spec.nodes[id]
      const service = _service<Id, S, V, X>(node)
      const duplicateServiceKey = Option.exists(service, (held) => {
        const watches = Array.map(held.watches ?? [], (watch) => watch.key)
        const invokes = Array.map(held.invokes ?? [], (invoke) => invoke.key)
        return Array.dedupe(watches).length !== watches.length || Array.dedupe(invokes).length !== invokes.length
      })
      return [
        ...(cyclic(id, HashSet.empty()) ? [{ reason: "cycle", node: Option.some(id) }] as const : []),
        ...(node.kind === "compound" && spec.nodes[node.initial].parent !== id
          ? [{ reason: "initial", node: Option.some(id) }] as const
          : []),
        ...(node.kind === "history" && !below(node.fallback, node.parent, HashSet.empty())
          ? [{ reason: "history", node: Option.some(id) }] as const
          : []),
        ...(node.kind === "parallel" && Array.every(ids, (child) => spec.nodes[child].parent !== id)
          ? [{ reason: "parallel", node: Option.some(id) }] as const
          : []),
        ...(duplicateServiceKey ? [{ reason: "service", node: Option.some(id) }] as const : []),
      ]
    }),
    ...(Number.isInteger(spec.fuel) && spec.fuel > 0 ? [] : [{ reason: "fuel", node: Option.none<string>() }] as const),
    ...(Number.isInteger(spec.lag) && spec.lag > 0 ? [] : [{ reason: "lag", node: Option.none<string>() }] as const),
  ]
  return Array.isNonEmptyReadonlyArray(issues)
    ? Either.left(new DefinitionFault({ issues }))
    : Either.right(spec)
}

const _service = <Id extends string, S extends string, V extends string, X>(
  node: Transition.Node<Id, S, V, X>,
): Option.Option<Transition.Service<Id, S, X>> =>
  node.kind === "final" || node.kind === "history" ? Option.none() : Option.some(node)

const _face = <Id extends string, S extends string, V extends string, X>(
  node: Transition.Node<Id, S, V, X>,
  side: keyof Transition.Face<V>,
): ReadonlyArray<V> => (node.kind === "history" ? [] : node[side] ?? [])

const _facts = <Id extends string, S extends string, V extends string, X>(
  nodes: Readonly<Record<Id, Transition.Node<Id, S, V, X>>>,
): Transition._Facts<Id> => {
  const ids = Struct.keys(nodes)
  const byOrder = Order.mapInput(Order.number, (id: Id) => ids.indexOf(id))
  const ancestors = (id: Id): ReadonlyArray<Id> =>
    Option.match(Option.fromNullable(nodes[id].parent), {
      onNone: (): ReadonlyArray<Id> => [],
      onSome: (held) => [held, ...ancestors(held)],
    })
  const children = (id: Id): ReadonlyArray<Id> => Array.filter(ids, (child) => nodes[child].parent === id)
  const leaves = (id: Id, history: Readonly<Record<string, ReadonlyArray<Id>>>): ReadonlyArray<Id> => {
    const node = nodes[id]
    return node.kind === "compound"
      ? leaves(node.initial, history)
      : node.kind === "parallel"
        ? Array.flatMap(children(id), (child) => leaves(child, history))
        : node.kind === "history"
          ? Option.match(Option.fromNullable(history[id]), {
              onNone: () => leaves(node.fallback, history),
              onSome: (stored) => Array.flatMap(stored, (held) => leaves(held, history)),
            })
          : [id]
  }
  const finalized = (id: Id, active: ReadonlyArray<Id>): boolean => {
    const node = nodes[id]
    return node.kind === "parallel"
      ? Array.every(children(id), (child) => finalized(child, active))
      : node.kind === "compound"
        ? Array.some(children(id), (child) => nodes[child].kind === "final" && Array.contains(active, child))
        : node.kind === "final" && Array.contains(active, id)
  }
  return {
    ids,
    byOrder,
    ancestors,
    children,
    leaves,
    finalized,
    closure: (active) =>
      pipe(
        HashSet.fromIterable(Array.flatMap(active, (id) => [id, ...ancestors(id)])),
        HashSet.toValues,
        Array.sort(byOrder),
      ),
    lcca: (members) =>
      Array.findFirst(
        Array.filter(ancestors(Array.headNonEmpty(members)), (candidate) => nodes[candidate].kind === "compound"),
        (candidate) => Array.every(members, (member) => Array.contains(ancestors(member), candidate)),
      ),
  }
}
```

## [03]-[MACROSTEP_FOLD]

[MACROSTEP_FOLD]:
- Owner: the macrostep algebra — selection walks each active leaf in document order through itself then its ancestors and takes the first row whose `on` and guard match (inner-first preemption, document-order priority); conflict removal keeps the earlier-selected transition when exit sets intersect (SCXML optimal transition set); the exit-set domain is one `_exits` computation selection and the microstep both read; one selected transition set becomes one aggregate microstep: the union exit set runs innermost-first, history records against the pre-step configuration, every `assign` folds in transition document order, transition programs emit in that same order, and the union entry set runs outermost-first with compound defaults, parallel expansion, and history dereference. Entering a `final` node raises `done.state.${parent}` onto the internal queue, and when that completion puts every region of a parallel grandparent into a final state — the `finalized` tree fact, SCXML `isInFinalState` — `done.state.${parallel}` follows in the same microstep.
- Law: one external signal is one macrostep — the fold drains eventless rows first, then the raised internal queue, so `raise`-style cascades and completion events resolve inside the step; the fuel row is the termination guarantee SCXML leaves informative, and its exhaustion is a typed left rail, never a silent partial: `_macro` returns the `[Config, Macro]` right only when a post-drain re-selection finds no live eventless row and an empty queue, otherwise it returns `Spent`, so the partial configuration and partial action program are unreachable from the success channel. `drive` short-circuits the first left without advancing its accumulator, `trace` raises that left on the stream error channel, and the actor request fails before arming. `assign` reads the `Option` trigger that selected its row — `Option.none` for an eventless microstep and the dequeued external or raised internal signal otherwise — so an extended-state writer never observes a trigger its row did not match.
- Law: the emit channel is an ordered action program — all selected exits emit in reverse document order before any selected transition program, every transition emits in document order before any entry, and all entries emit in document order — pure Mealy output the Effect interpreter at the consumer executes; parallel regions never interleave one region's entry ahead of another region's exit, and the fold performs no effect.
- Law: history is pure data — the exit fold records the deep atomic-descendant set or the shallow child set into `config.history` keyed by the history node, so `Machine.snapshot`/`restore` carries re-entry targets across process restarts with zero extra machinery.
- Law: `drive` and `trace` are the same typed step lifted — `drive` folds `Either` over a batch and aborts at the first `Spent`, `trace` uses `Stream.mapAccumEffect` to preserve the last settled accumulator and raise `Spent` on the stream rail, and the actor handler lifts the identical `Either` into its request failure, so a machine proven at the pure altitude behaves identically booted.
- Exemption: `_macro` is the page's one measured statement kernel — the run-to-completion drain mutates only its local queue, fuel counter, and accumulators, all dying at the return; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a pure read over the compiled tree (reachability census, terminal-configuration detection) is one member composing the same facts.

```typescript signature
const _exits = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id>,
) =>
(active: ReadonlyArray<Id>, row: Transition.Row<Id, S, V, X>): ReadonlyArray<Id> =>
  row.to === undefined
    ? []
    : pipe(
        row.internal === true && spec.nodes[row.source].kind === "compound"
          && Array.every(row.to, (target) => Array.contains(facts.ancestors(target), row.source))
          ? Option.some(row.source)
          : facts.lcca([row.source, ...row.to]),
        (domain) =>
          Array.filter(facts.closure(active), (id) =>
            Option.match(domain, {
              onNone: () => true,
              onSome: (root) => Array.contains(facts.ancestors(id), root),
            })),
      )

const _selected = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id>,
  config: Transition.Config<Id, X>,
  signal: Option.Option<Transition.Signal<Id, S>>,
): ReadonlyArray<Transition.Row<Id, S, V, X>> => {
  const matched = (source: Id): Option.Option<Transition.Row<Id, S, V, X>> =>
    Array.findFirst(spec.rows, (row) =>
      row.source === source
      && Option.match(signal, { onNone: () => row.on === undefined, onSome: (held) => row.on === held })
      && (row.when === undefined || row.when(config.extended)))
  const exitSet = (row: Transition.Row<Id, S, V, X>): ReadonlyArray<Id> => _exits(spec, facts)(config.active, row)
  return pipe(
    Array.sort(config.active, facts.byOrder),
    Array.filterMap((leaf) => Array.head(Array.filterMap([leaf, ...facts.ancestors(leaf)], matched))),
    Array.dedupe,
    Array.map((row) => [row, exitSet(row)] as const),
    Array.reduce(
      [] as ReadonlyArray<readonly [Transition.Row<Id, S, V, X>, ReadonlyArray<Id>]>,
      (kept, pair) =>
        Array.some(kept, ([, claimed]) => Array.intersection(claimed, pair[1]).length > 0)
          ? kept
          : Array.append(kept, pair),
    ),
    Array.map(([row]) => row),
  )
}

const _macro = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id>,
) =>
(
  config: Transition.Config<Id, X>,
  signal: Option.Option<Transition.Signal<Id, S>>,
): Either.Either<readonly [Transition.Config<Id, X>, Transition.Macro<Id, V>], Spent> => {
  // BOUNDARY ADAPTER: Bounded local mutation realizes run-to-completion without observable intermediate state.
  let held = config
  let fuel = spec.fuel
  const queue: Array<Transition.Signal<Id, S>> = Option.match(signal, { onNone: () => [], onSome: (held) => [held] })
  const program: Array<V> = []
  const enteredAll: Array<Id> = []
  const exitedAll: Array<Id> = []
  while (fuel > 0) {
    const eventless = _selected(spec, facts, held, Option.none())
    const dequeued = eventless.length > 0 ? Option.none<Transition.Signal<Id, S>>() : Option.fromNullable(queue.shift())
    const chosen = eventless.length > 0
      ? eventless
      : Option.match(dequeued, {
          onNone: (): ReadonlyArray<Transition.Row<Id, S, V, X>> => [],
          onSome: (next) => _selected(spec, facts, held, Option.some(next)),
        })
    if (eventless.length === 0 && Option.isNone(dequeued)) break
    const driving = dequeued
    if (chosen.length > 0) {
      const exited = pipe(
        Array.flatMap(chosen, (row) => _exits(spec, facts)(held.active, row)),
        Array.dedupe,
        Array.sort(Order.reverse(facts.byOrder)),
      )
      const recorded = Array.reduce(exited, held.history, (acc, id) =>
        Array.reduce(
          Array.filter(facts.ids, (child) => {
            const node = spec.nodes[child]
            return node.kind === "history" && node.parent === id
          }),
          acc,
          (inner, slot) => {
            const node = spec.nodes[slot]
            const stored = node.kind === "history" && node.depth === "deep"
              ? Array.filter(held.active, (leaf) => Array.contains(facts.ancestors(leaf), id))
              : Array.filter(facts.children(id), (child) =>
                  Array.some(held.active, (leaf) => leaf === child || Array.contains(facts.ancestors(leaf), child)))
            return { ...inner, [slot]: stored }
          },
        ))
      const arrived = Array.flatMap(chosen, (row) => Array.flatMap(row.to ?? [], (target) => facts.leaves(target, recorded)))
      const survivors = Array.filter(held.active, (leaf) => !Array.contains(exited, leaf))
      const active = Array.sort(Array.dedupe([...survivors, ...arrived]), facts.byOrder)
      const settled = facts.closure(survivors)
      const entered = Array.filter(facts.closure(active), (id) => !Array.contains(settled, id))
      const extended = Array.reduce(chosen, held.extended, (state, row) =>
        Option.match(Option.fromNullable(row.assign), {
          onNone: () => state,
          onSome: (assign) => assign(state, driving),
        }))
      for (const id of exited) program.push(..._face(spec.nodes[id], "exit"))
      for (const row of chosen) program.push(...(row.emit ?? []))
      for (const id of entered) program.push(..._face(spec.nodes[id], "entry"))
      for (const id of entered) {
        const node = spec.nodes[id]
        if (node.kind === "final") {
          queue.push(`done.state.${node.parent}`)
          const grand = spec.nodes[node.parent].parent
          if (
            grand !== undefined && spec.nodes[grand].kind === "parallel"
            && Array.every(facts.children(grand), (region) => facts.finalized(region, active))
          ) queue.push(`done.state.${grand}`)
        }
      }
      exitedAll.push(...exited)
      enteredAll.push(...entered)
      held = { active, history: recorded, extended }
    }
    fuel -= 1
  }
  const settled = _selected(spec, facts, held, Option.none()).length === 0 && queue.length === 0 // Post-drain re-selection proves stability.
  return settled
    ? Either.right([held, { program, entered: enteredAll, exited: exitedAll }] as const)
    : Either.left(new Spent({
        signal: Option.match(signal, { onNone: () => "<eventless>", onSome: (held) => held }),
        fuel: spec.fuel,
      }))
}
```

## [04]-[ACTOR]

[ACTOR]:
- Owner: the compiled `boot`/`restore` — `Machine.makeSerializable({ state, input }, initialize)` over the derived configuration schema, one `Feed` request whose success is the whole `Macro` receipt and whose failure is the typed `Spent` refusal, one `Poll` request reading the configuration, and one private `Finalize` request `Machine.serializable.addPrivate` seals off the wire — three procedures minted ONCE inside `Transition.spec`; the actor surface exposes `initial` (the boot-only origin entry receipt), `feed` (typed — the returned macro is the caller's verdict program AND the fact it just caused), `feedUnknown` (the wire-arriving lane — a socket-decoded frame admits through the machine's own schemas via `sendUnknown`, answers the schema-encoded `Schema.ExitEncoded` outcome a socket forwards verbatim, and a forged request fails as `ParseError`), `config` (the rail-side read), `state` (the actor IS a `Subscribable` of configuration — view atoms bind it directly), `facts` (the inspection stream), and `freeze`.
- Law: a spent macrostep never lands — `step` has no success tuple for exhaustion, and the `Feed` handler lifts its `Either` directly onto the request rail before any arming or state return, so the machine keeps its pre-signal configuration, no pure, stream, or actor consumer can observe a half-drained active set or partial action program as settled, and the authoring defect surfaces as `Spent` instead of a corrupted actor.
- Law: the entered/exited wave is the arming law — every macrostep disarms all keyed watch and invoke fibers of exited nodes and arms every row on entered nodes: each watch `forkReplace`s a keyed delayed self-`Feed`, and each invoke `forkReplace`s the child activity whose typed failure emits its declared failure signal while success `unsafeSend`s the private `Finalize` then its declared success signal or `done.invoke.${id}` — entry-start, exit-stop, exactly the SCXML invoke lifecycle on fiber primitives, with `Schedule`/`TestClock` beating hand timers on testability.
- Law: `finalize` is SCXML `<finalize>` on the request plane — the child result and invoke key ride the private `Finalize` request, its handler folds that invoke row's `finalize` function into extended state, and because one fiber serializes the queue the fold lands BEFORE the success signal is processed, so a done row's guard and `assign` read the already-folded result; the signal plane stays a literal vocabulary and never carries payloads, and a row without `finalize` discards the result by construction.
- Law: a fact is the macrostep's own receipt, never an inference — `boot` mints one `PubSub.sliding(spec.lag)` hub, `feed` publishes `{ config, macro }` after each settled send, and `feedUnknown` taps its encoded exit through the interior settled-macro decode so the wire lane publishes the identical fact; because the fact carries the macro's `entered`/`exited` directly, an external self-transition reports its exit-and-reentry wave exactly where an active-set diff reports nothing, and `facts` is `Stream.fromPubSub` over the hub — the machine still forks zero fibers, publication rides the send path, and a lagging inspector sheds oldest facts by the `lag` policy; actor span tracing is the `traced` policy row applied through `Machine.withTracingEnabled` at boot. Its hub is the branch's `rasm.core.state.macrostep` tap point — the `observe/tap` name row a subscription targets — and the machine imports nothing of the rail: publication stays the send path, dispatch stays the runtime executor's.
- Law: the actor lifetime is one scoped span — `boot` and `restore` each open `Effect.makeSpanScoped` named `machine/<name>` with the lane stamped (`boot` or `restore`), the span ends with the actor `Scope`, and every `feed`/`feedUnknown` send parents under it through `Effect.withParentSpan`, so `Machine.withTracingEnabled` request spans nest beneath one long-lived correlation anchor profile links and tap facts annotate; the name and lane spellings stay machine-local strings because state composes no observe vocabulary, and the runtime bridge re-stamps `convention` rows at export.
- Law: recovery and durability are definition facts — `Machine.retry(spec.recover)` re-initializes through the initialize slot carrying the last live configuration, `freeze` is `Machine.snapshot` (the schema-encoded pair carried opaque, history values inside), and `restore` re-admits through the machine's own schemas so a forged snapshot fails as `ParseError`, never a corrupted boot. A fresh boot drains the origin's eventless closure before actor creation, fails with `Spent` instead of exposing an unstable actor, arms every service in the settled active closure, and exposes the combined origin-entry and stabilization program through `actor.initial`; restore re-arms the restored active closure but returns the empty initial receipt, so process recovery cannot replay entry actions as new domain output.
- Law: the PUBLIC request surface is closed at feed/poll — signals ARE the wire protocol, and `sendUnknown` admits only the public list, so the private `Finalize` lane is unreachable from any frame; a public request demanding its own payload and reply contract is evidence the concern outgrew the transition altitude and belongs to the runtime branch's workflow plane.
- Boundary: the `Persistence`-backed snapshot store, durable-execution replay, and cluster sharding are runtime-branch concerns; this owner fixes the in-process actor and its vocabulary.
- Growth: a phase-entry side effect beyond emit vocabulary is a consumer tap on the verdict program or the fact stream, never a fourth handler concern.

```typescript signature
const _compile = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
): Transition.Compiled<Id, S, V, X> => {
  const facts = _facts(spec.nodes)
  const macro = _macro(spec, facts)
  const step = (config: Transition.Config<Id, X>, signal: Transition.Signal<Id, S>) =>
    macro(config, Option.some(signal))
  const roots = Array.filter(facts.ids, (id) => spec.nodes[id].parent === undefined)
  const origin: Transition.Config<Id, X> = {
    active: Option.match(Array.head(roots), {
      onNone: (): ReadonlyArray<Id> => [],
      onSome: (root) => facts.leaves(root, {}),
    }),
    history: {},
    extended: spec.seed,
  }
  const Id = Schema.Literal(...facts.ids)
  const Plane = Schema.Union(
    spec.signal,
    Schema.TemplateLiteral("done.state.", Id),
    Schema.TemplateLiteral("done.invoke.", Id),
  )
  const Config = Schema.Struct({
    active: Schema.Array(Id),
    history: Schema.Record({ key: Schema.String, value: Schema.Array(Id) }),
    extended: spec.extended,
  })
  const Macro = Schema.Struct({
    program: Schema.Array(spec.verdict),
    entered: Schema.Array(Id),
    exited: Schema.Array(Id),
  })
  const _settled = Schema.decodeUnknownOption(Schema.Struct({ _tag: Schema.Literal("Success"), value: Macro }))
  class Feed extends Schema.TaggedRequest<Feed>()("Feed", {
    failure: Spent,
    success: Macro,
    payload: { signal: Plane },
  }) {}
  class Poll extends Schema.TaggedRequest<Poll>()("Poll", {
    failure: Schema.Never,
    success: Config,
    payload: {},
  }) {}
  class Finalize extends Schema.TaggedRequest<Finalize>()("Finalize", {
    failure: Schema.Never,
    success: Schema.Void,
    payload: { id: Id, key: Schema.String, result: Schema.Unknown },
  }) {}
  const machine = Machine.makeSerializable({ state: Config, input: Config }, (boot, previous) =>
    Effect.gen(function* () {
      const context = yield* Machine.MachineContext
      const current = previous ?? boot
      const disarm = (exited: ReadonlyArray<Id>): Effect.Effect<void> =>
        Effect.forEach(exited, (id) =>
          Option.match(_service<Id, S, V, X>(spec.nodes[id]), {
            onNone: () => Effect.void,
            onSome: (service) => Effect.all([
              Effect.forEach(service.watches ?? [], (watch) => context.forkReplace(Effect.void, `arm:${id}:${watch.key}`), {
                discard: true,
              }),
              Effect.forEach(service.invokes ?? [], (invoke) => context.forkReplace(Effect.void, `invoke:${id}:${invoke.key}`), {
                discard: true,
              }),
            ], { concurrency: "unbounded", discard: true }),
          }), { discard: true })
      const arm = (config: Transition.Config<Id, X>, entered: ReadonlyArray<Id>): Effect.Effect<void> =>
        Effect.forEach(entered, (id) =>
          Option.match(_service<Id, S, V, X>(spec.nodes[id]), {
            onNone: () => Effect.void,
            onSome: (service) => Effect.all([
              Effect.forEach(service.watches ?? [], (watch) =>
                context.forkReplace(
                  Effect.delay(context.unsafeSend(new Feed({ signal: watch.signal })), watch.after),
                  `arm:${id}:${watch.key}`,
                ), { discard: true }),
              Effect.forEach(service.invokes ?? [], (invoke) =>
                context.forkReplace(
                  invoke.run(config.extended).pipe(Effect.matchEffect({
                    onFailure: () => context.unsafeSend(new Feed({ signal: invoke.failure })),
                    onSuccess: (result) => Effect.zipRight(
                      context.unsafeSend(new Finalize({ id, key: invoke.key, result })),
                      context.unsafeSend(new Feed({ signal: invoke.success ?? `done.invoke.${id}` })),
                    ),
                  })),
                  `invoke:${id}:${invoke.key}`,
                ), { discard: true }),
            ], { concurrency: "unbounded", discard: true }),
          }), { discard: true })
      yield* arm(current, facts.closure(current.active))
      return Machine.serializable.make(current).pipe(
        Machine.serializable.add(Feed, ({ request, state }) =>
          Effect.gen(function* () {
            const [next, macro] = yield* Either.match(step(state, request.signal), {
              onLeft: Effect.fail,
              onRight: Effect.succeed,
            })
            yield* disarm(macro.exited)
            yield* arm(next, macro.entered)
            return [macro, next] as const
          })),
        Machine.serializable.add(Poll, ({ state }) => Effect.succeed([state, state] as const)),
        Machine.serializable.addPrivate(Finalize, ({ request, state }) =>
          Effect.succeed([
            void 0,
            {
              ...state,
              extended: Option.match(
                Option.flatMap(
                  _service<Id, S, V, X>(spec.nodes[request.id]),
                  (service) => Option.flatMap(
                    Array.findFirst(service.invokes ?? [], (invoke) => invoke.key === request.key),
                    (invoke) => Option.fromNullable(invoke.finalize),
                  ),
                ),
                {
                  onNone: () => state.extended,
                  onSome: (finalize) => finalize(state.extended, request.result),
                },
              ),
            },
          ] as const)),
      )
    })).pipe(Machine.retry(spec.recover))
  const surfaced = (
    actor: Machine.SerializableActor<typeof machine>,
    hub: PubSub.PubSub<Transition.Fact<Id, V, X>>,
    span: Tracer.Span,
    initial: Transition.Macro<Id, V>,
  ): Transition.Actor<Id, S, V, X> => {
    const published = (macro: Transition.Macro<Id, V>): Effect.Effect<void> =>
      Effect.asVoid(Effect.flatMap(actor.get, (config) => PubSub.publish(hub, { config, macro })))
    return {
      initial,
      feed: (signal) => Effect.withParentSpan(Effect.tap(actor.send(new Feed({ signal })), published), span), // every send parents under the lifetime span: request spans nest beneath one correlation anchor
      feedUnknown: (frame) =>
        Effect.withParentSpan(
          Effect.tap(actor.sendUnknown(frame), (outcome) =>
            Option.match(_settled(outcome), {
              onNone: () => Effect.void,
              onSome: (exit) => published(exit.value),
            })),
          span,
        ),
      config: actor.get,
      state: actor,
      facts: Stream.fromPubSub(hub),
      freeze: Machine.snapshot(actor),
    }
  }
  const risen = (
    live: Effect.Effect<Machine.SerializableActor<typeof machine>, ParseResult.ParseError, Scope.Scope>,
    initial: Transition.Macro<Id, V>,
    lane: "boot" | "restore",
  ) =>
    Effect.flatMap(Effect.makeSpanScoped(`machine/${spec.name}`, { attributes: { "machine.lane": lane } }), (span) =>
      Effect.zipWith(Effect.withParentSpan(live, span), PubSub.sliding<Transition.Fact<Id, V, X>>(spec.lag), (actor, hub) =>
        surfaced(actor, hub, span, initial))) // the scoped span ends with the actor Scope: one lifetime anchor per boot or restore
  const entered = facts.closure(origin.active)
  const initial: Transition.Macro<Id, V> = {
    program: Array.flatMap(entered, (id) => _face(spec.nodes[id], "entry")),
    entered,
    exited: [],
  }
  const resumed: Transition.Macro<Id, V> = { program: [], entered: [], exited: [] }
  const fresh = Either.match(macro(origin, Option.none()), {
    onLeft: Effect.fail,
    onRight: ([current, settled]) => risen(
      Machine.boot(machine, current).pipe(Machine.withTracingEnabled(spec.traced)),
      {
        program: [...initial.program, ...settled.program],
        entered: [...initial.entered, ...settled.entered],
        exited: settled.exited,
      },
      "boot",
    ),
  })
  const restored = (frozen: Transition.Frozen) =>
    Effect.flatMap(
      Effect.all({
        input: Schema.decodeUnknown(machine.schemaInput)(frozen[0]),
        state: Schema.decodeUnknown(machine.schemaState)(frozen[1]),
      }),
      ({ input, state }) => Machine.boot(machine, input, { previousState: state }),
    )
  return {
    ...spec,
    origin,
    step,
    boot: fresh,
    restore: (frozen) => risen(restored(frozen).pipe(Machine.withTracingEnabled(spec.traced)), resumed, "restore"),
  }
}

const _drive = <Id extends string, S extends string, V extends string, X>(
  compiled: Transition.Compiled<Id, S, V, X>,
) => (
  origin: Transition.Config<Id, X>,
  signals: ReadonlyArray<Transition.Signal<Id, S>>,
): Either.Either<readonly [Transition.Config<Id, X>, ReadonlyArray<Transition.Macro<Id, V>>], Spent> => {
  const seeded: Either.Either<
    readonly [Transition.Config<Id, X>, ReadonlyArray<Transition.Macro<Id, V>>],
    Spent
  > = Either.right([origin, []])
  return Array.reduce(signals, seeded, (rail, signal) =>
    Either.flatMap(rail, ([config, macros]) =>
      Either.map(compiled.step(config, signal), ([next, macro]) => [next, Array.append(macros, macro)] as const)))
}

const Transition: Transition.Shape = {
  spec: (spec) => Either.map(_validated(spec), _compile),
  drive: _drive,
  trace: (compiled) => (signals) =>
    Stream.mapAccumEffect(signals, compiled.origin, (config, signal) =>
      Either.match(compiled.step(config, signal), {
        onLeft: Effect.fail,
        onRight: ([next, macro]) => Effect.succeed([next, macro] as const),
      })),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { DefinitionFault, Spent, Transition }
```
