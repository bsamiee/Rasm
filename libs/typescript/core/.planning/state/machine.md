# [CORE_MACHINE]

The statechart owner: a closed transition system is data — one `Transition.Spec` whose `nodes` table declares the state tree (atomic, compound, parallel, final, history — declaration order IS document order and document order is the determinism law) and whose `rows` carry guarded, internally-or-externally-domained, ordered-emit transitions — and one compile at `Transition.spec` precomputes the tree algebra (ancestor chains, entry completion, LCCA, the final-state census), derives the configuration schema from the node vocabulary, and mints the serializable `@effect/experimental` `Machine` exactly once, so `boot` and `restore` only run the actor and never recompile. The same compiled value drives three altitudes: the pure macrostep fold (`step` drains eventless and raised internal signals to stability under a bounded-microstep fuel row), the batch and stream drivers (`drive` through `Array.mapAccum`, `trace` through `Stream.mapAccum`), and the booted actor — one state on one fiber, phase-keyed watchdogs and node-scoped invoke fibers armed by the entered/exited wave, history carried inside machine state so `snapshot`/`restore` transports it durably for free, the actor's own `Subscribable` state binding view atoms, and a derived fact stream as the inspection hook a consumer taps without the machine forking anything. The flat Mealy table is the degenerate case — a depth-one tree with a singleton configuration. THE ALTITUDE RULING: `Machine` is the in-process serializable actor; a machine whose steps demand durable-execution replay, activity memoization, compensation, or cross-process sharding is the runtime branch's workflow altitude, and promoting a transition system there re-homes the spec, never re-shapes it. The module is `core/src/state/machine.ts`; a new state is a node row, a new transition is a table row, a new deadline is a watch row, a new child activity is an invoke row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                          | [PUBLIC]                          |
| :-----: | :----------------- | :------------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `STATECHART_TABLE` | the node/row/config vocabulary and the one-shot compile with static tree facts   | `Transition.Spec`, `Transition.spec` |
|  [02]   | `MACROSTEP_FOLD`   | selection, conflict removal, exit/entry algebra, the fuel-bounded macrostep      | `Transition.drive`, `Transition.trace` |
|  [03]   | `ACTOR`            | boot, restore, wire admission, arming, the subscribable state and fact stream    | compiled `boot`/`restore`          |

## [2]-[STATECHART_TABLE]

[STATECHART_TABLE]:
- Owner: `Transition.Spec<Id, S, V, X>` — the machine as one value: `name`, `nodes` (the kind-discriminated state tree; declaration order is SCXML document order), `rows` (the transition matrix in document order), the `signal`/`verdict` literal schemas, the `extended` schema plus `seed` (guard-readable extended state, snapshot-serializable), `fuel` (the bounded-microstep row that makes every macrostep terminate), `traced` (actor span emission as a definition fact), `recover` (the defect re-initialization `Schedule`). `Transition.spec` compiles it once into `Transition.Compiled` — static tree facts, the derived configuration schema, the origin configuration, the pure `step`, and the pre-minted machine with its request classes — so booting then restoring one spec never re-mints request-class identities.
- Law: `nodes` is a closed tagged family — `compound` demands `initial`, `history` demands `depth` and `fallback`, `final` demands `parent` — so an ill-formed tree is a compile error at the spec value, never a runtime walk; the configuration is the active atomic-leaf set plus the recorded `history` values plus the extended state, and its schema derives from the node vocabulary (`Schema.Literal` over the id roster), so a wire-carried or snapshot-carried configuration decodes against exactly the declared tree.
- Law: guards are pure reads of extended state — `when: (extended) => boolean`, SCXML side-effect-free `cond` — preserving the Mealy character and snapshot determinism; `assign` is the only extended-state writer and it is a pure function on the row.
- Law: internal signals derive from the id vocabulary — `done.state.${Id}` and `done.invoke.${Id}` are `Schema.TemplateLiteral` members of the signal plane, minted by the fold and the invoke completion, matchable by any row's `on`; a hand-written done-signal literal beside the template is the drift defect.
- Law: the internal-versus-external distinction is one row flag — `internal: true` shrinks the transition domain from the LCCA to the source (SCXML `type="internal"`), the whole semantic difference; external is the default posture.
- Law: `Machine.serializable.add` is the pipeable dual — data-last `(schema, handler) => (self) => self` — and `Machine.procedures.add` is a differently-shaped curried type application; the two namespaces never substitute.
- Growth: a new state, transition, deadline, or child activity is one row in the owning table; a new machine is one spec value; dynamic child registries beyond node-scoped invoke ride the context's id-addressed `forkOne` under the same arming law.
- Packages: `@effect/experimental` (`Machine`); `effect` (`Array`, `Duration`, `Effect`, `HashSet`, `Option`, `Order`, `ParseResult`, `Schedule`, `Schema`, `Scope`, `Stream`, `Struct`, `Subscribable`).

```typescript
import { Machine } from "@effect/experimental"
import {
  Array, type Duration, Effect, HashSet, Option, Order, type ParseResult, pipe, type Schedule, Schema, type Scope,
  Stream, Struct, Subscribable,
} from "effect"

declare namespace Transition {
  type Depth = "shallow" | "deep"
  type Internal<Id extends string> = `done.state.${Id}` | `done.invoke.${Id}`
  type Signal<Id extends string, S extends string> = S | Internal<Id>
  type Watch<Id extends string, S extends string> = {
    readonly after: Duration.DurationInput
    readonly signal: Signal<Id, S>
  }
  type Face<V extends string> = { readonly entry?: ReadonlyArray<V>; readonly exit?: ReadonlyArray<V> }
  type Service<Id extends string, S extends string, X> = {
    readonly watch?: Watch<Id, S>
    readonly invoke?: (extended: X) => Effect.Effect<unknown>
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
    readonly assign?: (extended: X, signal: Signal<Id, S>) => X
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
    readonly traced: boolean
    readonly recover: Schedule.Schedule<unknown>
  }
  type Frozen = readonly [unknown, unknown]
  type Fact<Id extends string, X> = {
    readonly config: Config<Id, X>
    readonly entered: ReadonlyArray<Id>
    readonly exited: ReadonlyArray<Id>
  }
  type Actor<Id extends string, S extends string, V extends string, X> = {
    readonly feed: (signal: Signal<Id, S>) => Effect.Effect<ReadonlyArray<V>>
    readonly feedUnknown: (frame: unknown) => Effect.Effect<unknown, ParseResult.ParseError>
    readonly config: Effect.Effect<Config<Id, X>>
    readonly state: Subscribable.Subscribable<Config<Id, X>>
    readonly facts: Stream.Stream<Fact<Id, X>>
    readonly freeze: Effect.Effect<Frozen, ParseResult.ParseError>
  }
  type Compiled<Id extends string, S extends string, V extends string, X> = Spec<Id, S, V, X> & {
    readonly origin: Config<Id, X>
    readonly step: (config: Config<Id, X>, signal: Signal<Id, S>) => readonly [Config<Id, X>, Macro<Id, V>]
    readonly boot: Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError, Scope.Scope>
    readonly restore: (frozen: Frozen) => Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError, Scope.Scope>
  }
  type Shape = {
    readonly spec: <Id extends string, S extends string, V extends string, X>(
      spec: Spec<Id, S, V, X>,
    ) => Compiled<Id, S, V, X>
    readonly drive: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => (
      origin: Config<Id, X>,
      signals: ReadonlyArray<Signal<Id, S>>,
    ) => [Config<Id, X>, ReadonlyArray<Macro<Id, V>>]
    readonly trace: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => <E, R>(signals: Stream.Stream<Signal<Id, S>, E, R>) => Stream.Stream<Macro<Id, V>, E, R>
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

## [3]-[MACROSTEP_FOLD]

[MACROSTEP_FOLD]:
- Owner: the macrostep algebra — selection walks each active leaf in document order through itself then its ancestors and takes the first row whose `on` and guard match (inner-first preemption, document-order priority); conflict removal keeps the earlier-selected transition when exit sets intersect (SCXML optimal transition set); the exit-set domain is one `_exits` computation selection and the microstep both read; the microstep exits innermost-first, records history before exit actions run, applies `assign`, emits the transition program, and enters outermost-first with compound defaults, parallel expansion, and history dereference; entering a `final` node raises `done.state.${parent}` onto the internal queue, and when that completion puts every region of a parallel grandparent into a final state — the `finalized` tree fact, SCXML `isInFinalState` — `done.state.${parallel}` follows in the same microstep.
- Law: one external signal is one macrostep — the fold drains eventless rows first, then the raised internal queue, to stability or fuel exhaustion, so `raise`-style cascades and completion events resolve inside the step and a consumer never observes a half-drained configuration; the fuel row is the termination guarantee SCXML leaves informative. `assign` reads the signal that selected its row — the dequeued internal signal on an internal microstep, the external signal otherwise — so an extended-state writer never observes a trigger its row did not match.
- Law: the emit channel is an ordered action program — exit emits in reverse document order, transition emits, entry emits in document order — pure Mealy output the Effect interpreter at the consumer executes; the fold performs no effect.
- Law: history is pure data — the exit fold records the deep atomic-descendant set or the shallow child set into `config.history` keyed by the history node, so `Machine.snapshot`/`restore` carries re-entry targets across process restarts with zero extra machinery.
- Law: `drive` and `trace` are the same step lifted — `Array.mapAccum` over a batch, `Stream.mapAccum` over a signal stream seeded at the compiled origin — and the actor's handler runs the identical `step`, so a machine proven at the pure altitude behaves identically booted.
- Exemption: `_macro` is the page's one measured statement kernel — the run-to-completion drain mutates only its local queue, fuel counter, and accumulators, all dying at the return; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a pure read over the compiled tree (reachability census, terminal-configuration detection) is one member composing the same facts.

```typescript
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
  signal: Transition.Signal<Id, S>,
): readonly [Transition.Config<Id, X>, Transition.Macro<Id, V>] => {
  let held = config
  let fuel = spec.fuel
  const queue: Array<Transition.Signal<Id, S>> = [signal]
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
    const driving = Option.getOrElse(dequeued, () => signal)
    for (const row of chosen) {
      const exited = Array.sort(_exits(spec, facts)(held.active, row), Order.reverse(facts.byOrder))
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
      const targets = row.to ?? []
      const arrived = Array.flatMap(targets, (target) => facts.leaves(target, recorded))
      const survivors = Array.filter(held.active, (leaf) => !Array.contains(exited, leaf))
      const active = Array.sort(Array.dedupe([...survivors, ...arrived]), facts.byOrder)
      const settled = facts.closure(survivors)
      const entered = Array.filter(facts.closure(active), (id) => !Array.contains(settled, id))
      for (const id of exited) program.push(..._face(spec.nodes[id], "exit"))
      program.push(...(row.emit ?? []))
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
      held = {
        active,
        history: recorded,
        extended: Option.match(Option.fromNullable(row.assign), {
          onNone: () => held.extended,
          onSome: (assign) => assign(held.extended, driving),
        }),
      }
    }
    fuel -= 1
  }
  return [held, { program, entered: enteredAll, exited: exitedAll }] as const
}
```

## [4]-[ACTOR]

[ACTOR]:
- Owner: the compiled `boot`/`restore` — `Machine.makeSerializable({ state, input }, initialize)` over the derived configuration schema, one `Feed` request carrying a signal-plane member and one `Poll` request reading the configuration, minted ONCE inside `Transition.spec`; the actor surface exposes `feed` (typed), `feedUnknown` (the wire-arriving lane — a socket-decoded frame admits through the machine's own schemas via `sendUnknown` and a forged request fails as `ParseError`), `config` (the rail-side read), `state` (the actor IS a `Subscribable` of configuration — view atoms bind it directly), `facts` (the inspection stream), and `freeze`.
- Law: the entered/exited wave is the arming law — every macrostep disarms the watch and invoke fibers of exited nodes and arms entered nodes: a watch row `forkReplace`s a keyed delayed self-`Feed` (at most one watcher per node, stacked signals cannot stack timers), an invoke row `forkReplace`s the child activity whose completion `unsafeSend`s `done.invoke.${id}` back onto the request plane — entry-start, exit-stop, exactly the SCXML invoke lifecycle on fiber primitives, with `Schedule`/`TestClock` beating hand timers on testability.
- Law: the fact stream is a derived hook, never a fork — `Stream.zipWithPrevious` over the actor's own subscribable changes yields configuration, entered, and exited per macrostep; a consumer taps it for inspection, metrics, or replay capture and the machine runs zero telemetry fibers of its own; actor span tracing is the `traced` policy row applied through `Machine.withTracingEnabled` at boot.
- Law: recovery and durability are definition facts — `Machine.retry(spec.recover)` re-initializes through the initialize slot carrying the last live configuration (`previous ?? origin`), `freeze` is `Machine.snapshot` (the schema-encoded pair carried opaque, history values inside), and `restore` re-admits through the machine's own schemas so a forged snapshot fails as `ParseError`, never a corrupted boot; the origin configuration enters silently — entry programs are observable only through macrosteps.
- Law: the request surface is closed at feed/poll — signals ARE the protocol; a request demanding its own payload and reply contract is evidence the concern outgrew the transition altitude and belongs to the runtime branch's workflow plane.
- Boundary: the `Persistence`-backed snapshot store, durable-execution replay, and cluster sharding are runtime-branch concerns; this owner fixes the in-process actor and its vocabulary.
- Growth: a phase-entry side effect beyond emit vocabulary is a consumer tap on the verdict program or the fact stream, never a fourth handler concern.

```typescript
const _compile = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
): Transition.Compiled<Id, S, V, X> => {
  const facts = _facts(spec.nodes)
  const step = _macro(spec, facts)
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
  class Feed extends Schema.TaggedRequest<Feed>()("Feed", {
    failure: Schema.Never,
    success: Schema.Array(spec.verdict),
    payload: { signal: Plane },
  }) {}
  class Poll extends Schema.TaggedRequest<Poll>()("Poll", {
    failure: Schema.Never,
    success: Config,
    payload: {},
  }) {}
  const machine = Machine.makeSerializable({ state: Config, input: Config }, (boot, previous) =>
    Machine.serializable.make(previous ?? boot).pipe(
      Machine.serializable.add(Feed, ({ forkReplace, request, send, state, unsafeSend }) =>
        Effect.gen(function* () {
          const [next, macro] = step(state, request.signal)
          yield* Effect.forEach(macro.exited, (id) =>
            Option.match(_service(spec.nodes[id]), {
              onNone: () => Effect.void,
              onSome: () =>
                Effect.zipRight(forkReplace(Effect.void, `arm:${id}`), forkReplace(Effect.void, `invoke:${id}`)),
            }), { discard: true })
          yield* Effect.forEach(macro.entered, (id) =>
            Option.match(_service(spec.nodes[id]), {
              onNone: () => Effect.void,
              onSome: (service) =>
                Effect.zipRight(
                  service.watch === undefined
                    ? Effect.void
                    : forkReplace(
                        Effect.delay(send(new Feed({ signal: service.watch.signal })), service.watch.after),
                        `arm:${id}`,
                      ),
                  service.invoke === undefined
                    ? Effect.void
                    : forkReplace(
                        Effect.zipRight(
                          service.invoke(next.extended),
                          unsafeSend(new Feed({ signal: `done.invoke.${id}` })),
                        ),
                        `invoke:${id}`,
                      ),
                ),
            }), { discard: true })
          return [macro.program, next] as const
        })),
      Machine.serializable.add(Poll, ({ state }) => Effect.succeed([state, state] as const)),
    )).pipe(Machine.retry(spec.recover))
  const surfaced = (actor: Machine.SerializableActor<typeof machine>): Transition.Actor<Id, S, V, X> => ({
    feed: (signal) => actor.send(new Feed({ signal })),
    feedUnknown: (frame) => actor.sendUnknown(frame),
    config: actor.get,
    state: actor,
    facts: pipe(
      actor.changes,
      Stream.zipWithPrevious,
      Stream.map(([previous, next]) => ({
        config: next,
        entered: Option.match(previous, {
          onNone: () => next.active,
          onSome: (prior) => Array.difference(next.active, prior.active),
        }),
        exited: Option.match(previous, {
          onNone: (): ReadonlyArray<Id> => [],
          onSome: (prior) => Array.difference(prior.active, next.active),
        }),
      })),
    ),
    freeze: Machine.snapshot(actor),
  })
  return {
    ...spec,
    origin,
    step,
    boot: Effect.map(
      Machine.boot(machine, origin).pipe(Machine.withTracingEnabled(spec.traced)),
      surfaced,
    ),
    restore: (frozen) =>
      Effect.map(
        Machine.restore(machine, frozen).pipe(Machine.withTracingEnabled(spec.traced)),
        surfaced,
      ),
  }
}

const Transition: Transition.Shape = {
  spec: _compile,
  drive: (compiled) => (origin, signals) => Array.mapAccum(signals, origin, compiled.step),
  trace: (compiled) => (signals) => Stream.mapAccum(signals, compiled.origin, compiled.step),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Transition }
```
