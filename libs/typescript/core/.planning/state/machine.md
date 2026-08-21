# [CORE_MACHINE]

The statechart owner: a closed transition system is data — one `Transition.Spec` whose `nodes` table declares the state tree (atomic, compound, parallel, final, history — declaration order IS document order and document order is the determinism law) and whose `rows` carry guarded, internally-or-externally-domained, ordered-emit transitions — and one accumulated admission at `Transition.spec` that refuses invalid topology before precomputing the tree algebra (ancestor chains, entry completion, LCCA, the final-state census), deriving the configuration schema from the node vocabulary, and minting the serializable `@effect/experimental` `Machine` exactly once, so `boot` and `restore` only run an admitted actor and never recompile. The same compiled value drives three altitudes: the pure macrostep fold (`step` drains eventless and raised internal signals under a bounded-microstep fuel row, returns an `Either` whose `Spent` left rail carries exhaustion, and accounts every unrouted signal on the macro's own `refused` column), the published legality read (`legal` names the signals a configuration admits and `admits` answers one signal with the rows it takes or a typed refusal separating an unaddressed signal from a closed guard, both projections of the selection the macrostep already folds), the batch driver (`drive` folds the same rail without advancing past a left), the stream driver (`trace` lifts that rail through `Stream.mapAccumEffect`), and the booted actor — one state on one fiber, phase-keyed watchdogs and node-scoped invoke fibers armed by the entered/exited wave with completions folded through the node's `finalize` row before the done signal fires, history carried inside machine state so `snapshot`/`restore` transports it durably for free, the actor's own `Subscribable` state binding view atoms, and a fact stream published from the macrostep's own receipt on the send path — the inspection hook a consumer taps while the machine forks nothing of its own. The flat Mealy table is the degenerate case — a depth-one tree with a singleton configuration. THE ALTITUDE RULING: `Machine` is the in-process serializable actor, and serializability is forced rather than asserted — `snapshot`/`restore` and the `sendUnknown` wire admission exist only on the schema-carried `Machine.serializable` list, because the schemaless `Machine.procedures` altitude forfeits exactly the durability these laws demand; a machine whose steps demand durable-execution replay, activity memoization, compensation, or cross-process sharding is the runtime branch's workflow altitude, and promoting a transition system there re-homes the spec, never re-shapes it. The module is `core/src/state/machine.ts`; a new state is a node row, a new transition is a table row, a new deadline is a watch row, a new child activity is an invoke row.

## [01]-[INDEX]

- [02]-[STATECHART_TABLE]: node/row/config vocabulary, one-shot compile, static tree facts; `Transition.Spec`, `Transition.spec`.
- [03]-[MACROSTEP_FOLD]: selection, conflict removal, exit/entry algebra, fuel-bounded macrostep, published legality; `Transition.drive`, `Transition.trace`.
- [04]-[ACTOR]: boot, restore, wire admission, arming, subscribable state, fact stream; compiled `boot`/`restore`.

## [02]-[STATECHART_TABLE]

[STATECHART_TABLE]:

```typescript signature
import { Machine } from "@effect/experimental"
import {
  Array, Cause, Context, Data, type Duration, Effect, Either, Exit, HashMap, HashSet, Option, Order, type ParseResult,
  pipe, PubSub, Ref, type Schedule, Schema, type Scope, Stream, Subscribable, type Tracer,
} from "effect"
import { Fault } from "../value/fault.ts"
import { Shape } from "../value/schema.ts"

declare namespace Transition {
  type Spent = _Spent
  type DefinitionFault = InstanceType<typeof _DefinitionFault>
  type Depth = "shallow" | "deep"
  type Internal<Id extends string> = `done.state.${Id}` | `done.invoke.${Id}`
  type Signal<Id extends string, S extends string> = S | Internal<Id>
  type Watch<Id extends string, S extends string> = {
    readonly key: string
    readonly after: Duration.DurationInput
    readonly signal: Signal<Id, S>
  }
  type InvokeSpec<Id extends string, S extends string, X, A, E> = {
    readonly key: string
    readonly run: (extended: X) => Effect.Effect<A, E>
    readonly success?: Signal<Id, S>
    readonly failure: Signal<Id, S>
    readonly finalize?: (extended: X, result: A) => X
  }
  type Invoke<Id extends string, S extends string, X> = _Invoke<Id, S, X>
  type Face<V extends string> = { readonly entry?: ReadonlyArray<V>; readonly exit?: ReadonlyArray<V> }
  type Service<Id extends string, S extends string, X> = {
    readonly watches?: ReadonlyArray<Watch<Id, S>>
    readonly invokes?: ReadonlyArray<Invoke<Id, S, X>>
  }
  type Node<Id extends string, S extends string, V extends string, X> =
    | (Face<V> & Service<Id, S, X> & { readonly id: Id; readonly kind: "atomic"; readonly parent?: Id })
    | (Face<V> & Service<Id, S, X> & { readonly id: Id; readonly kind: "compound"; readonly parent?: Id; readonly initial: Id })
    | (Face<V> & Service<Id, S, X> & { readonly id: Id; readonly kind: "parallel"; readonly parent?: Id })
    | (Face<V> & { readonly id: Id; readonly kind: "final"; readonly parent: Id })
    | { readonly id: Id; readonly kind: "history"; readonly parent: Id; readonly depth: Depth; readonly fallback: Id }
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
  type Refusal<Id extends string, S extends string> =
    | { readonly _tag: "Unrouted"; readonly signal: Signal<Id, S> }
    | { readonly _tag: "Guarded"; readonly signal: Signal<Id, S>; readonly rows: Array.NonEmptyReadonlyArray<number> }
  type Macro<Id extends string, S extends string, V extends string> = {
    readonly program: ReadonlyArray<V>
    readonly entered: ReadonlyArray<Id>
    readonly exited: ReadonlyArray<Id>
    readonly refused: ReadonlyArray<Refusal<Id, S>>
  }
  type Spec<Id extends string, S extends string, V extends string, X> = {
    readonly name: string
    readonly nodes: Array.NonEmptyReadonlyArray<Node<Id, S, V, X>>
    readonly rows: ReadonlyArray<Row<Id, S, V, X>>
    readonly signal: Schema.Schema<S, S>
    readonly verdict: Schema.Schema<V, V>
    readonly extended: Schema.Schema<X>
    readonly seed: X
    readonly fuel: number
    readonly lag: number
    readonly traced: boolean
    readonly recover: <M extends Machine.Any>() => Schedule.Schedule<unknown, Machine.InitError<M> | Machine.MachineDefect>
  }
  type Frozen = readonly [unknown, unknown]
  type Fact<Id extends string, S extends string, V extends string, X> = {
    readonly config: Config<Id, X>
    readonly macro: Macro<Id, S, V>
    readonly faults: ReadonlyArray<ActivityFault<Id>>
  }
  type ActivityFault<Id extends string> = { readonly id: Id; readonly key: string; readonly cause: Cause.Cause<unknown> }
  type Actor<Id extends string, S extends string, V extends string, X> = {
    readonly initial: Macro<Id, S, V>
    readonly feed: (signal: Signal<Id, S>) => Effect.Effect<Fact<Id, S, V, X>, _Spent>
    readonly feedUnknown: (frame: unknown) => Effect.Effect<Schema.ExitEncoded<unknown, unknown, unknown>, ParseResult.ParseError>
    readonly config: Effect.Effect<Config<Id, X>>
    readonly state: Subscribable.Subscribable<Config<Id, X>>
    readonly facts: Stream.Stream<Fact<Id, S, V, X>>
    readonly freeze: Effect.Effect<Frozen, ParseResult.ParseError>
  }
  type Compiled<Id extends string, S extends string, V extends string, X> = Spec<Id, S, V, X> & {
    readonly origin: Config<Id, X>
    readonly legal: (config: Config<Id, X>) => HashSet.HashSet<Signal<Id, S>>
    readonly admits: (
      config: Config<Id, X>,
      signal: Signal<Id, S>,
    ) => Either.Either<Array.NonEmptyReadonlyArray<Row<Id, S, V, X>>, Refusal<Id, S>>
    readonly step: (config: Config<Id, X>, signal: Signal<Id, S>) => Either.Either<readonly [Config<Id, X>, Macro<Id, S, V>], _Spent>
    readonly boot: Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError | _Spent, Scope.Scope>
    readonly restore: (frozen: Frozen) => Effect.Effect<Actor<Id, S, V, X>, ParseResult.ParseError, Scope.Scope>
  }
  type Shape = {
    readonly DefinitionFault: typeof _DefinitionFault
    readonly Spent: typeof _Spent
    readonly invoke: <Id extends string, S extends string, X, A, E>(
      spec: InvokeSpec<Id, S, X, A, E>,
    ) => Invoke<Id, S, X>
    readonly spec: <Id extends string, S extends string, V extends string, X>(
      spec: Spec<Id, S, V, X>,
    ) => Either.Either<Compiled<Id, S, V, X>, DefinitionFault>
    readonly drive: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => (
      origin: Config<Id, X>,
      signals: ReadonlyArray<Signal<Id, S>>,
    ) => Either.Either<readonly [Config<Id, X>, ReadonlyArray<Macro<Id, S, V>>], _Spent>
    readonly trace: <Id extends string, S extends string, V extends string, X>(
      compiled: Compiled<Id, S, V, X>,
    ) => <E, R>(signals: Stream.Stream<Signal<Id, S>, E, R>) => Stream.Stream<Macro<Id, S, V>, E | _Spent, R>
  }
  type _Facts<Id extends string, S extends string, V extends string, X> = {
    readonly ids: ReadonlyArray<Id>
    readonly node: (id: Id) => Node<Id, S, V, X>
    readonly byOrder: Order.Order<Id>
    readonly ancestors: (id: Id) => ReadonlyArray<Id>
    readonly children: (id: Id) => ReadonlyArray<Id>
    readonly leaves: (id: Id, history: Readonly<Record<string, ReadonlyArray<Id>>>) => ReadonlyArray<Id>
    readonly closure: (active: ReadonlyArray<Id>) => ReadonlyArray<Id>
    readonly lcca: (members: Array.NonEmptyReadonlyArray<Id>) => Option.Option<Id>
    readonly finalized: (id: Id, active: ReadonlyArray<Id>) => boolean
  }
}

type _Completion<X> = Data.TaggedEnum<{
  Success: { readonly apply: (extended: X) => X }
  Failure: { readonly cause: Cause.Cause<unknown> }
}>
const _Completion = {
  Success: <X>(apply: (extended: X) => X): _Completion<X> => ({ _tag: "Success", apply }),
  Failure: <X>(cause: Cause.Cause<unknown>): _Completion<X> => ({ _tag: "Failure", cause }),
} as const

type _Invoke<Id extends string, S extends string, X> = {
  readonly key: string
  readonly run: (extended: X) => Effect.Effect<_Completion<X>>
  readonly success?: Transition.Signal<Id, S>
  readonly failure: Transition.Signal<Id, S>
}

const _invoke = <Id extends string, S extends string, X, A, E>(
  spec: Transition.InvokeSpec<Id, S, X, A, E>,
): Transition.Invoke<Id, S, X> => ({
  key: spec.key,
  success: spec.success,
  failure: spec.failure,
  run: (extended) => Effect.map(Effect.exit(spec.run(extended)), (exit) =>
    Exit.match(exit, {
      onFailure: (cause) => _Completion.Failure<X>(cause),
      onSuccess: (result) => _Completion.Success<X>(
        spec.finalize === undefined ? (state) => state : (state) => spec.finalize!(state, result),
      ),
    })),
})

// A spent microstep budget is bound exhaustion, and bound exhaustion is ONE family estate-wide: `Shape.Bound` already
// closes the unit roster and mints the evidence row, so this owner composes `Fault.Class.spent` rather than declaring
// a private `fuel` reason a second budget would then have to keep aligned with it. The SIGNAL rides beside the case
// because it is this owner's own coordinate — which signal the table was folding when the budget ran out — and no
// bound vocabulary can carry it. Topology refusals stay this owner's own mint below: caller-authored material the
// compile quarantines into a repair report, with retryability, blame, and quarantine read off the core row table.
class _Spent extends Schema.TaggedError<_Spent>()("Transition.Spent", {
  case: Fault.Class.spent.payload,
  signal: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return Fault.Class.spent.classOf(this.case.reason)
  }
  override get message(): string {
    return `${Fault.Class.spent.render(this.case)} @ ${this.signal}`
  }
}

// Every topology refusal is caller-authored material the compile quarantines into its repair report, so all ten rows
// grade `invalid` and retryability, blame, and quarantine read off the core row table with no local column beside
// `class`. The SUBJECT is what varies: six reasons refuse ABOUT a declared node while four refuse about the whole
// table or a budget scalar, so the `node: Option<string>` column only the first six ever filled collapses into
// per-reason detail and a row carrying no node can no longer spell one. Each spec-level row carries the MEASURE its
// own check read — the root count, the collision count, the offered scalar — which is the fact a repair acts on and
// the fact a bare reason word withheld. `leg` partitions the plane that decides: the declaration tables, or the two
// scalars bounding a macrostep.
const _At = Schema.Struct({ node: Schema.NonEmptyString })
const _Offered = Schema.Struct({ offered: Schema.Number })
const _definition = Fault.Class.family(
  ["root", "duplicate", "reference", "cycle", "initial", "history", "parallel", "service", "fuel", "lag"] as const,
  {
    cycle: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} reaches itself through its own parent chain`,
    }),
    duplicate: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: Schema.Struct({ repeated: Schema.Int.pipe(Schema.positive()) }),
      render: ({ repeated }) => `${repeated} node ids repeat in the declared table`,
    }),
    fuel: Fault.Class.row({
      class: "invalid",
      leg: "budget",
      detail: _Offered,
      render: ({ offered }) => `microstep budget ${offered} is not a positive integer`,
    }),
    history: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} falls back to a node outside its own parent's subtree`,
    }),
    initial: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} names an initial state that is not its own child`,
    }),
    lag: Fault.Class.row({
      class: "invalid",
      leg: "budget",
      detail: _Offered,
      render: ({ offered }) => `deadline lag ${offered} is not a positive integer`,
    }),
    parallel: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} declares parallel regions and holds no non-history child`,
    }),
    reference: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} names an id the node table does not declare`,
    }),
    root: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: Schema.Struct({ roots: Schema.Int.pipe(Schema.nonNegative()) }),
      render: ({ roots }) => `the node table declares ${roots} roots where exactly one is admitted`,
    }),
    service: Fault.Class.row({
      class: "invalid",
      leg: "tree",
      detail: _At,
      render: ({ node }) => `${node} repeats a watch or invoke key inside its own service row`,
    }),
  },
)

// Topology columns are admitted INDEPENDENTLY — a cyclic parent chain and a bad initial child decide nothing about
// each other — so the carrier is the family's OWN census and an author reads every offence of a spec in one pass.
// Re-declaring `{ issues, class, message }` here forked one taxonomy into two, and the dominance election, the leg,
// and the joined message all derive from the roster the rows above already close.
const _DefinitionFault = _definition.census("Transition.DefinitionFault")

const _validated = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
): Either.Either<Transition.Spec<Id, S, V, X>, Transition.DefinitionFault> => {
  const ids = Array.map(spec.nodes, (node) => node.id)
  const lookup = (id: Id): Option.Option<Transition.Node<Id, S, V, X>> =>
    Array.findFirst(spec.nodes, (node) => node.id === id)
  const roots = Array.filter(spec.nodes, (node) => node.parent === undefined)
  const cyclic = (id: Id, trail: HashSet.HashSet<Id>): boolean =>
    HashSet.has(trail, id)
      || Option.match(Option.flatMap(lookup(id), (node) => Option.fromNullable(node.parent)), {
        onNone: () => false,
        onSome: (parent) => cyclic(parent, HashSet.add(trail, id)),
      })
  const below = (id: Id, ancestor: Id, trail: HashSet.HashSet<Id>): boolean =>
    HashSet.has(trail, id)
      ? false
      : Option.match(Option.flatMap(lookup(id), (node) => Option.fromNullable(node.parent)), {
        onNone: () => false,
        onSome: (parent) => parent === ancestor || below(parent, ancestor, HashSet.add(trail, id)),
      })
  const referenced = (node: Transition.Node<Id, S, V, X>): ReadonlyArray<Id> => [
    ...Option.toArray(Option.fromNullable(node.parent)),
    ...(node.kind === "compound" ? [node.initial] : []),
    ...(node.kind === "history" ? [node.fallback] : []),
  ]
  const issues = [
    ...(roots.length === 1 ? [] : [{ reason: "root", roots: roots.length }] as const),
    ...(Array.dedupe(ids).length === ids.length
      ? []
      : [{ reason: "duplicate", repeated: ids.length - Array.dedupe(ids).length }] as const),
    ...Array.flatMap(spec.nodes, (node) => {
      const service = _service<Id, S, V, X>(node)
      const duplicateServiceKey = Option.exists(service, (held) => {
        const keys = [
          ...Array.map(held.watches ?? [], (watch) => watch.key),
          ...Array.map(held.invokes ?? [], (invoke) => invoke.key),
        ]
        return Array.dedupe(keys).length !== keys.length
      })
      return [
        ...(Array.some(referenced(node), (id) => Option.isNone(lookup(id)))
          ? [{ reason: "reference", node: node.id }] as const
          : []),
        ...(cyclic(node.id, HashSet.empty()) ? [{ reason: "cycle", node: node.id }] as const : []),
        ...(node.kind === "compound" && !Option.exists(lookup(node.initial), (initial) => initial.parent === node.id)
          ? [{ reason: "initial", node: node.id }] as const
          : []),
        ...(node.kind === "history" && !below(node.fallback, node.parent, HashSet.empty())
          ? [{ reason: "history", node: node.id }] as const
          : []),
        ...(node.kind === "parallel"
          && Array.every(spec.nodes, (child) => child.parent !== node.id || child.kind === "history")
          ? [{ reason: "parallel", node: node.id }] as const
          : []),
        ...(duplicateServiceKey ? [{ reason: "service", node: node.id }] as const : []),
      ]
    }),
    ...Array.flatMap(spec.rows, (row) =>
      Array.every([row.source, ...(row.to ?? [])], (id) => Option.isSome(lookup(id)))
        ? []
        : [{ reason: "reference", node: row.source }] as const),
    ...(Number.isInteger(spec.fuel) && spec.fuel > 0 ? [] : [{ reason: "fuel", offered: spec.fuel }] as const),
    ...(Number.isInteger(spec.lag) && spec.lag > 0 ? [] : [{ reason: "lag", offered: spec.lag }] as const),
  ]
  return Array.isNonEmptyReadonlyArray(issues)
    ? Either.left(new _DefinitionFault({ issues }))
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
  nodes: Array.NonEmptyReadonlyArray<Transition.Node<Id, S, V, X>>,
): Transition._Facts<Id, S, V, X> => {
  const ids = Array.map(nodes, (node) => node.id)
  const indexed = HashMap.fromIterable(Array.map(nodes, (node) => [node.id, node] as const))
  const node = (id: Id): Transition.Node<Id, S, V, X> => Option.getOrThrow(HashMap.get(indexed, id))
  const byOrder = Order.mapInput(Order.number, (id: Id) => ids.indexOf(id))
  const ancestors = (id: Id): ReadonlyArray<Id> =>
    Option.match(Option.fromNullable(node(id).parent), {
      onNone: (): ReadonlyArray<Id> => [],
      onSome: (held) => [held, ...ancestors(held)],
    })
  const children = (id: Id): ReadonlyArray<Id> =>
    Array.map(Array.filter(nodes, (child) => child.parent === id && child.kind !== "history"), (child) => child.id)
  const leaves = (id: Id, history: Readonly<Record<string, ReadonlyArray<Id>>>): ReadonlyArray<Id> => {
    const held = node(id)
    return held.kind === "compound"
      ? leaves(held.initial, history)
      : held.kind === "parallel"
        ? Array.flatMap(children(id), (child) => leaves(child, history))
        : held.kind === "history"
          ? Option.match(Option.fromNullable(history[id]), {
              onNone: () => leaves(held.fallback, history),
              onSome: (stored) => Array.flatMap(stored, (held) => leaves(held, history)),
            })
          : [id]
  }
  const finalized = (id: Id, active: ReadonlyArray<Id>): boolean => {
    const held = node(id)
    return held.kind === "parallel"
      ? Array.every(children(id), (child) => finalized(child, active))
      : held.kind === "compound"
        ? Array.some(children(id), (child) => node(child).kind === "final" && Array.contains(active, child))
        : held.kind === "final" && Array.contains(active, id)
  }
  return {
    ids,
    node,
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
        Array.filter(ancestors(Array.headNonEmpty(members)), (candidate) => node(candidate).kind === "compound"),
        (candidate) => Array.every(members, (member) => Array.contains(ancestors(member), candidate)),
      ),
  }
}
```

## [03]-[MACROSTEP_FOLD]

[MACROSTEP_FOLD]:
- Law: legality is DERIVED from the row table, never hand-kept — `_addressed` is the one predicate answering whether a row claims a signal from a source, `_selected` adds the guard and conflict removal to reach the rows a microstep takes, and `_refused` re-reads the same predicate to name why nothing was taken; `legal` and `admits` publish those two folds on `Compiled`, so a consumer asks the owner instead of manufacturing a mirror row, a stage-local enabled flag, or a second guard evaluation over the same extended state.
- Law: the refusal carries its discriminant — `Guarded` names the row indexes that addressed the signal and whose `when` answered false, `Unrouted` names a signal no active leaf or ancestor claims at all, and the two are not one absence: a guarded press is a policy the extended state can satisfy while an unrouted one is a spelling the table never carried. Spelling this position as a boolean, an empty array, or a bare `Option.none` forces every consumer to fabricate the fact the fold already held.
- Law: refusal work prices on the refusing arm alone — `_selected` computes no guard census and `_refused` runs only where the chosen set came back empty, so a routed microstep pays zero scan; a selection eagerly returning its guard evidence beside its rows charges every passing signal for a report only failures read.
- Law: the macrostep ACCOUNTS the signals it drops — a signal with no chosen row burns one fuel unit and lands its `Refusal` on `Macro.refused`, so a `Macro` with an empty program is distinguishable from one that silently swallowed a press; the fold never fails on an unrouted signal, because discarding an unclaimed event is the transition system's own semantics and only the evidence was missing.
- Growth: a pure read over the compiled tree (reachability census, terminal-configuration detection) is one member composing the same facts; a new transition is one `rows` entry and its legality derives with it.

```typescript signature
const _exits = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id, S, V, X>,
) =>
(active: ReadonlyArray<Id>, row: Transition.Row<Id, S, V, X>): ReadonlyArray<Id> =>
  row.to === undefined
    ? []
    : pipe(
        row.internal === true && facts.node(row.source).kind === "compound"
          && Array.every(row.to, (target) => Array.contains(facts.ancestors(target), row.source))
          ? Option.some(row.source)
          : Option.orElse(facts.lcca([row.source, ...row.to]), () =>
              Array.every(row.to, (target) => target === row.source || Array.contains(facts.ancestors(target), row.source))
                ? Option.some(row.source)
                : Option.none()),
        (domain) =>
          Array.filter(facts.closure(active), (id) =>
            Option.match(domain, {
              onNone: () => true,
              onSome: (root) =>
                Array.contains(facts.ancestors(id), root)
                || (row.internal !== true && root === row.source && id === root),
            })),
      )

// `_addressed` states the one claim test both halves of legality read — source identity beside the signal domain,
// with the guard deliberately OUTSIDE it: `_selected` adds `when` to reach the taken rows, `_refused` subtracts it
// to name the rows that claimed the signal and were closed, and a second inline spelling forks the two answers
const _addressed = <Id extends string, S extends string, V extends string, X>(
  row: Transition.Row<Id, S, V, X>,
  source: Id,
  signal: Option.Option<Transition.Signal<Id, S>>,
): boolean =>
  row.source === source
  && Option.match(signal, { onNone: () => row.on === undefined, onSome: (held) => row.on === held })

// minted on the refusing arm alone: a routed signal never pays this scan
const _refused = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id, S, V, X>,
  config: Transition.Config<Id, X>,
  signal: Transition.Signal<Id, S>,
): Transition.Refusal<Id, S> => {
  const held = Option.some(signal)
  const standing = facts.closure(config.active)
  // this fold runs only where `_selected` came back empty, so every row the standing closure addresses necessarily
  // answered false on its own `when` — an addressed row carrying no guard would have been chosen — and the index
  // set is therefore exactly the guards that closed the door, with an empty one proving the table never claimed it
  const rows = Array.filterMap(spec.rows, (row, index) =>
    Array.some(standing, (id) => _addressed(row, id, held)) ? Option.some(index) : Option.none())
  return Array.isNonEmptyReadonlyArray(rows) ? { _tag: "Guarded", signal, rows } : { _tag: "Unrouted", signal }
}

const _selected = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id, S, V, X>,
  config: Transition.Config<Id, X>,
  signal: Option.Option<Transition.Signal<Id, S>>,
): ReadonlyArray<Transition.Row<Id, S, V, X>> => {
  const matched = (source: Id): Option.Option<Transition.Row<Id, S, V, X>> =>
    Array.findFirst(spec.rows, (row) =>
      _addressed(row, source, signal) && (row.when === undefined || row.when(config.extended)))
  const exitSet = (row: Transition.Row<Id, S, V, X>): ReadonlyArray<Id> => _exits(spec, facts)(config.active, row)
  type Candidate = {
    readonly row: Transition.Row<Id, S, V, X>
    readonly rowIndex: number
    readonly depth: number
    readonly exits: ReadonlyArray<Id>
  }
  const byCandidate = Order.combine(
    Order.mapInput(Order.reverse(Order.number), (candidate: Candidate) => candidate.depth),
    Order.mapInput(Order.number, (candidate: Candidate) => candidate.rowIndex),
  )
  return pipe(
    Array.sort(config.active, facts.byOrder),
    Array.filterMap((leaf) => Array.head(Array.filterMap([leaf, ...facts.ancestors(leaf)], matched))),
    Array.dedupe,
    Array.map((row): Candidate => ({
      row,
      rowIndex: spec.rows.indexOf(row),
      depth: facts.ancestors(row.source).length,
      exits: exitSet(row),
    })),
    Array.sort(byCandidate),
    Array.reduce(
      [] as ReadonlyArray<Candidate>,
      (kept, candidate) =>
        Array.some(kept, (prior) => Array.intersection(prior.exits, candidate.exits).length > 0)
          ? kept
          : Array.append(kept, candidate),
    ),
    Array.map((candidate) => candidate.row),
  )
}

const _macro = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
  facts: Transition._Facts<Id, S, V, X>,
) =>
(
  config: Transition.Config<Id, X>,
  signal: Option.Option<Transition.Signal<Id, S>>,
): Either.Either<readonly [Transition.Config<Id, X>, Transition.Macro<Id, S, V>], _Spent> => {
  type Acc = {
    readonly config: Transition.Config<Id, X>
    readonly queue: ReadonlyArray<Transition.Signal<Id, S>>
    readonly program: ReadonlyArray<V>
    readonly entered: ReadonlyArray<Id>
    readonly exited: ReadonlyArray<Id>
    readonly refused: ReadonlyArray<Transition.Refusal<Id, S>>
    readonly remaining: number
  }
  const completed = (entered: ReadonlyArray<Id>, active: ReadonlyArray<Id>): ReadonlyArray<Transition.Signal<Id, S>> =>
    pipe(
      entered,
      Array.flatMap((id) => facts.node(id).kind === "final"
        ? Array.filter(facts.ancestors(id), (ancestor) => facts.finalized(ancestor, active))
        : []),
      Array.dedupe,
      Array.map((id) => `done.state.${id}` as const),
    )
  const advance = (
    acc: Acc,
    chosen: ReadonlyArray<Transition.Row<Id, S, V, X>>,
    driving: Option.Option<Transition.Signal<Id, S>>,
    tail: ReadonlyArray<Transition.Signal<Id, S>>,
  ): Acc => {
    const exited = pipe(
      Array.flatMap(chosen, (row) => _exits(spec, facts)(acc.config.active, row)),
      Array.dedupe,
      Array.sort(Order.reverse(facts.byOrder)),
    )
    const recorded = Array.reduce(exited, acc.config.history, (history, id) =>
      Array.reduce(
        Array.filter(facts.ids, (slot) => {
          const node = facts.node(slot)
          return node.kind === "history" && node.parent === id
        }),
        history,
        (held, slot) => {
          const node = facts.node(slot)
          const stored = node.kind === "history" && node.depth === "deep"
            ? Array.filter(acc.config.active, (leaf) => Array.contains(facts.ancestors(leaf), id))
            : Array.filter(facts.children(id), (child) =>
                Array.some(acc.config.active, (leaf) => leaf === child || Array.contains(facts.ancestors(leaf), child)))
          return { ...held, [slot]: stored }
        },
      ))
    const arrived = Array.flatMap(chosen, (row) => Array.flatMap(row.to ?? [], (target) => facts.leaves(target, recorded)))
    const survivors = Array.filter(acc.config.active, (leaf) => !Array.contains(exited, leaf))
    const active = Array.sort(Array.dedupe([...survivors, ...arrived]), facts.byOrder)
    const entered = Array.filter(facts.closure(active), (id) => !Array.contains(facts.closure(survivors), id))
    const extended = Array.reduce(chosen, acc.config.extended, (state, row) =>
      Option.match(Option.fromNullable(row.assign), {
        onNone: () => state,
        onSome: (assign) => assign(state, driving),
      }))
    return {
      config: { active, history: recorded, extended },
      queue: [...tail, ...completed(entered, active)],
      program: [
        ...acc.program,
        ...Array.flatMap(exited, (id) => _face(facts.node(id), "exit")),
        ...Array.flatMap(chosen, (row) => row.emit ?? []),
        ...Array.flatMap(entered, (id) => _face(facts.node(id), "entry")),
      ],
      entered: [...acc.entered, ...entered],
      exited: [...acc.exited, ...exited],
      refused: acc.refused,
      remaining: acc.remaining - 1,
    }
  }
  const drain = (acc: Acc): Either.Either<readonly [Transition.Config<Id, X>, Transition.Macro<Id, S, V>], _Spent> => {
    const eventless = _selected(spec, facts, acc.config, Option.none())
    const dequeued = Array.match(acc.queue, {
      onEmpty: () => Option.none<readonly [Transition.Signal<Id, S>, ReadonlyArray<Transition.Signal<Id, S>>]>(),
      onNonEmpty: (held) => Option.some([Array.headNonEmpty(held), Array.tailNonEmpty(held)] as const),
    })
    if (Array.isEmptyReadonlyArray(eventless) && Option.isNone(dequeued)) {
      return Either.right([
        acc.config,
        { program: acc.program, entered: acc.entered, exited: acc.exited, refused: acc.refused },
      ] as const)
    }
    if (acc.remaining === 0) {
      // The budget admitted as a positive integer at `spec`, so the branded ceiling mints HERE and nowhere else —
      // on the refusing arm alone, which is the one path that reads the evidence row.
      return Either.left(new _Spent({
        case: { reason: "fuel", ceiling: Shape.Bound.bounded("fuel", spec.fuel).ceiling, reached: spec.fuel },
        signal: Option.match(signal, { onNone: () => "<eventless>", onSome: (held) => held }),
      }))
    }
    const driving = Array.isNonEmptyReadonlyArray(eventless)
      ? Option.none<Transition.Signal<Id, S>>()
      : Option.map(dequeued, ([next]) => next)
    const tail = Array.isNonEmptyReadonlyArray(eventless)
      ? acc.queue
      : Option.match(dequeued, { onNone: () => [], onSome: ([, held]) => held })
    const chosen = Array.isNonEmptyReadonlyArray(eventless)
      ? eventless
      : Option.match(driving, {
          onNone: (): ReadonlyArray<Transition.Row<Id, S, V, X>> => [],
          onSome: (next) => _selected(spec, facts, acc.config, Option.some(next)),
        })
    // an unchosen signal is DROPPED by the transition system's own semantics and RECORDED by this one, so a caller
    // reads which press the table never carried instead of an empty program indistinguishable from a routed no-emit
    return drain(Array.isEmptyReadonlyArray(chosen)
      ? {
        ...acc,
        queue: tail,
        refused: [
          ...acc.refused,
          ...Option.toArray(Option.map(driving, (next) => _refused(spec, facts, acc.config, next))),
        ],
        remaining: acc.remaining - 1,
      }
      : advance(acc, chosen, driving, tail))
  }
  return drain({
    config,
    queue: Option.toArray(signal),
    program: [],
    entered: [],
    exited: [],
    refused: [],
    remaining: spec.fuel,
  })
}
```

## [04]-[ACTOR]

[ACTOR]:

```typescript signature
const _compile = <Id extends string, S extends string, V extends string, X>(
  spec: Transition.Spec<Id, S, V, X>,
): Transition.Compiled<Id, S, V, X> => {
  const facts = _facts(spec.nodes)
  const macro = _macro(spec, facts)
  const step = (config: Transition.Config<Id, X>, signal: Transition.Signal<Id, S>) =>
    macro(config, Option.some(signal))
  const admits = (
    config: Transition.Config<Id, X>,
    signal: Transition.Signal<Id, S>,
  ): Either.Either<Array.NonEmptyReadonlyArray<Transition.Row<Id, S, V, X>>, Transition.Refusal<Id, S>> => {
    const chosen = _selected(spec, facts, config, Option.some(signal))
    return Array.isNonEmptyReadonlyArray(chosen)
      ? Either.right(chosen)
      : Either.left(_refused(spec, facts, config, signal))
  }
  // `legal` draws its signal domain from the row table's own `on` column, so the admitted set grows with a
  // transition row and no consumer spells a second vocabulary beside the table that drifts from it
  const legal = (config: Transition.Config<Id, X>): HashSet.HashSet<Transition.Signal<Id, S>> =>
    pipe(
      Array.filterMap(spec.rows, (row) => Option.fromNullable(row.on)),
      Array.dedupe,
      Array.filter((on) => Either.isRight(admits(config, on))),
      HashSet.fromIterable,
    )
  const roots = Array.filter(facts.ids, (id) => facts.node(id).parent === undefined)
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
  const Leaf = Id.pipe(Schema.filter((id) => {
    const node = facts.node(id)
    return node.kind === "atomic" || node.kind === "final"
  }))
  const validConfig = (config: {
    readonly active: ReadonlyArray<Id>
    readonly history: Readonly<Record<string, ReadonlyArray<Id>>>
    readonly extended: X
  }): boolean => {
    const activeUnder = (id: Id): boolean =>
      Array.some(config.active, (leaf) => leaf === id || Array.contains(facts.ancestors(leaf), id))
    const legalCoverage = Array.every(facts.closure(config.active), (id) => {
      const node = facts.node(id)
      const covered = Array.filter(facts.children(id), activeUnder)
      return node.kind === "compound"
        ? covered.length === 1
        : node.kind === "parallel"
          ? covered.length === facts.children(id).length
          : true
    })
    const legalHistory = Record.every(config.history, (stored, key) => {
      if (!Array.contains(facts.ids, key as Id)) return false
      const node = facts.node(key as Id)
      return node.kind === "history"
        && Array.isNonEmptyReadonlyArray(stored)
        && Array.dedupe(stored).length === stored.length
        && (node.depth === "deep"
          ? Array.every(stored, (id) => {
              const held = facts.node(id)
              return (held.kind === "atomic" || held.kind === "final")
                && Array.contains(facts.ancestors(id), node.parent)
            })
          : Array.every(stored, (id) => {
              const held = facts.node(id)
              return held.kind !== "history" && held.parent === node.parent
            }))
    })
    return Array.dedupe(config.active).length === config.active.length && legalCoverage && legalHistory
  }
  const Config = Schema.Struct({
    active: Schema.Array(Leaf).pipe(Schema.filter(Array.isNonEmptyReadonlyArray)),
    history: Shape.Record(Id, Schema.Array(Id)),
    extended: spec.extended,
  }).pipe(Schema.filter(validConfig, { identifier: "Transition.Config" }))
  const Refusal = Schema.Union(
    Schema.TaggedStruct("Unrouted", { signal: Plane }),
    Schema.TaggedStruct("Guarded", { signal: Plane, rows: Schema.NonEmptyArray(Schema.Int.pipe(Schema.nonNegative())) }),
  )
  const Macro = Schema.Struct({
    program: Schema.Array(spec.verdict),
    entered: Schema.Array(Id),
    exited: Schema.Array(Id),
    refused: Schema.Array(Refusal),
  })
  const ActivityFault = Schema.Struct({
    id: Id,
    key: Schema.String,
    cause: Schema.Cause({ error: Schema.Unknown, defect: Schema.Unknown }),
  })
  const Fact = Schema.Struct({ config: Config, macro: Macro, faults: Schema.Array(ActivityFault) })
  class Feed extends Schema.TaggedRequest<Feed>()("Feed", {
    failure: _Spent,
    success: Fact,
    payload: { signal: Plane },
  }) {}
  class Poll extends Schema.TaggedRequest<Poll>()("Poll", {
    failure: Schema.Never,
    success: Config,
    payload: {},
  }) {}
  class Complete extends Schema.TaggedRequest<Complete>()("Complete", {
    failure: Schema.Never,
    success: Schema.Void,
    payload: {
      id: Id,
      key: Schema.String,
      generation: Schema.Int.pipe(Schema.nonNegative()),
      kind: Schema.Literal("watch", "invoke"),
    },
  }) {}
  class FactHub extends Context.Tag(`Transition/${spec.name}/FactHub`)<
    FactHub,
    PubSub.PubSub<Transition.Fact<Id, S, V, X>>
  >() {}
  const defined = Machine.makeSerializable({ state: Config, input: Config }, (boot, previous) =>
    Effect.gen(function* () {
      const context = yield* Machine.MachineContext
      const hub = yield* FactHub
      const current = previous ?? boot
      const tokens = yield* Ref.make(HashMap.empty<readonly [Id, string], number>())
      const outcomes = yield* Ref.make(HashMap.empty<readonly [Id, string], readonly [number, _Completion<X>]>() )
      const slot = (id: Id, key: string) => Data.tuple(id, key)
      const bump = (id: Id, key: string): Effect.Effect<number> => Ref.modify(tokens, (held) => {
        const coordinate = slot(id, key)
        const next = Option.getOrElse(HashMap.get(held, coordinate), () => 0) + 1
        return [next, HashMap.set(held, coordinate, next)] as const
      })
      const disarm = (exited: ReadonlyArray<Id>): Effect.Effect<void> =>
        Effect.forEach(exited, (id) =>
          Option.match(_service<Id, S, V, X>(facts.node(id)), {
            onNone: () => Effect.void,
            onSome: (service) => Effect.forEach([
              ...Array.map(service.watches ?? [], (watch) => ["watch", watch.key] as const),
              ...Array.map(service.invokes ?? [], (invoke) => ["invoke", invoke.key] as const),
            ], ([kind, key]) => Effect.zipRight(
              bump(id, key),
              context.forkReplace(Effect.void, `${kind}:${id}:${key}`),
            ), { concurrency: "inherit", discard: true }),
          }), { concurrency: "inherit", discard: true })
      const arm = (config: Transition.Config<Id, X>, entered: ReadonlyArray<Id>): Effect.Effect<void> =>
        Effect.forEach(entered, (id) =>
          Option.match(_service<Id, S, V, X>(facts.node(id)), {
            onNone: () => Effect.void,
            onSome: (service) => Effect.all([
              Effect.forEach(service.watches ?? [], (watch) => Effect.flatMap(bump(id, watch.key), (generation) =>
                context.forkReplace(
                  Effect.delay(context.unsafeSend(new Complete({ id, key: watch.key, generation, kind: "watch" })), watch.after),
                  `watch:${id}:${watch.key}`,
                )), { concurrency: "inherit", discard: true }),
              Effect.forEach(service.invokes ?? [], (invoke) => Effect.flatMap(bump(id, invoke.key), (generation) =>
                context.forkReplace(
                  Effect.flatMap(invoke.run(config.extended), (completion) => Effect.zipRight(
                    Ref.update(outcomes, HashMap.set(slot(id, invoke.key), [generation, completion] as const)),
                    context.unsafeSend(new Complete({ id, key: invoke.key, generation, kind: "invoke" })),
                  )),
                  `invoke:${id}:${invoke.key}`,
                )), { concurrency: "inherit", discard: true }),
            ], { concurrency: "inherit", discard: true }),
          }), { concurrency: "inherit", discard: true })
      const advance = (
        state: Transition.Config<Id, X>,
        signal: Transition.Signal<Id, S>,
        faults: ReadonlyArray<Transition.ActivityFault<Id>>,
      ): Effect.Effect<readonly [Transition.Fact<Id, S, V, X>, Transition.Config<Id, X>], _Spent> =>
        Either.match(step(state, signal), {
          onLeft: Effect.fail,
          onRight: ([next, settled]) => Effect.gen(function* () {
            yield* disarm(settled.exited)
            yield* arm(next, settled.entered)
            const fact = { config: next, macro: settled, faults }
            yield* PubSub.publish(hub, fact)
            return [fact, next] as const
          }),
        })
      yield* arm(current, facts.closure(current.active))
      return Machine.serializable.make(current).pipe(
        Machine.serializable.add(Feed, ({ request, state }) => advance(state, request.signal, [])),
        Machine.serializable.add(Poll, ({ state }) => Effect.succeed([state, state] as const)),
        Machine.serializable.addPrivate(Complete, ({ request, state }) => Effect.flatMap(Ref.get(tokens), (held) => {
          if (!Option.contains(HashMap.get(held, slot(request.id, request.key)), request.generation)) {
            return Effect.succeed([void 0, state] as const)
          }
          const service = _service<Id, S, V, X>(facts.node(request.id))
          if (request.kind === "watch") {
            return Option.match(Option.flatMap(service, (rows) =>
              Array.findFirst(rows.watches ?? [], (row) => row.key === request.key)), {
              onNone: () => Effect.succeed([void 0, state] as const),
              onSome: (watch) => Effect.map(advance(state, watch.signal, []), ([, next]) => [void 0, next] as const),
            })
          }
          return Effect.flatMap(Ref.modify(outcomes, (rows) => {
            const found = HashMap.get(rows, slot(request.id, request.key))
            return [found, HashMap.remove(rows, slot(request.id, request.key))] as const
          }), (found) => Option.match(found, {
            onNone: () => Effect.succeed([void 0, state] as const),
            onSome: ([generation, completion]) => generation !== request.generation
              ? Effect.succeed([void 0, state] as const)
              : Option.match(Option.flatMap(service, (rows) =>
                  Array.findFirst(rows.invokes ?? [], (row) => row.key === request.key)), {
                onNone: () => Effect.succeed([void 0, state] as const),
                onSome: (invoke) => completion._tag === "Success"
                  ? Effect.map(
                    advance(
                      { ...state, extended: completion.apply(state.extended) },
                      invoke.success ?? `done.invoke.${request.id}`,
                      [],
                    ),
                    ([, next]) => [void 0, next] as const,
                  )
                  : Effect.map(
                    advance(state, invoke.failure, [{ id: request.id, key: request.key, cause: completion.cause }]),
                    ([, next]) => [void 0, next] as const,
                  ),
              }),
          }))
        })),
      )
    }))
  const machine = Machine.retry(defined, spec.recover<typeof defined>())
  const surfaced = (
    actor: Machine.SerializableActor<typeof machine>,
    hub: PubSub.PubSub<Transition.Fact<Id, S, V, X>>,
    span: Tracer.Span,
    initial: Transition.Macro<Id, S, V>,
  ): Transition.Actor<Id, S, V, X> => {
    return {
      initial,
      feed: (signal) => Effect.withParentSpan(actor.send(new Feed({ signal })), span),
      feedUnknown: (frame) => Effect.withParentSpan(actor.sendUnknown(frame), span),
      config: actor.get,
      state: actor,
      facts: Stream.fromPubSub(hub),
      freeze: Machine.snapshot(actor),
    }
  }
  const risen = (
    live: Effect.Effect<Machine.SerializableActor<typeof machine>, ParseResult.ParseError, Scope.Scope | FactHub>,
    initial: Transition.Macro<Id, S, V>,
    lane: "boot" | "restore",
  ) =>
    Effect.flatMap(Effect.makeSpanScoped(`machine/${spec.name}`, { attributes: { "machine.lane": lane } }), (span) =>
      Effect.flatMap(PubSub.sliding<Transition.Fact<Id, S, V, X>>(spec.lag), (hub) =>
        Effect.map(
          Effect.provideService(Effect.withParentSpan(live, span), FactHub, hub),
          (actor) => surfaced(actor, hub, span, initial),
        )))
  const entered = facts.closure(origin.active)
  const initial: Transition.Macro<Id, S, V> = {
    program: Array.flatMap(entered, (id) => _face(facts.node(id), "entry")),
    entered,
    exited: [],
    refused: [],
  }
  const resumed: Transition.Macro<Id, S, V> = { program: [], entered: [], exited: [], refused: [] }
  const fresh = Either.match(macro(origin, Option.none()), {
    onLeft: Effect.fail,
    onRight: ([current, settled]) => risen(
      Machine.boot(machine, current).pipe(Machine.withTracingEnabled(spec.traced)),
      {
        program: [...initial.program, ...settled.program],
        entered: [...initial.entered, ...settled.entered],
        exited: settled.exited,
        refused: settled.refused,
      },
      "boot",
    ),
  })
  const restored = (frozen: Transition.Frozen) => Machine.restore(machine, frozen)
  return {
    ...spec,
    origin,
    legal,
    admits,
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
): Either.Either<readonly [Transition.Config<Id, X>, ReadonlyArray<Transition.Macro<Id, S, V>>], _Spent> => {
  const seeded: Either.Either<
    readonly [Transition.Config<Id, X>, ReadonlyArray<Transition.Macro<Id, S, V>>],
    _Spent
  > = Either.right([origin, []])
  return Array.reduce(signals, seeded, (rail, signal) =>
    Either.flatMap(rail, ([config, macros]) =>
      Either.map(compiled.step(config, signal), ([next, macro]) => [next, Array.append(macros, macro)] as const)))
}

const Transition: Transition.Shape = {
  DefinitionFault: _DefinitionFault,
  Spent: _Spent,
  invoke: _invoke,
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

export { Transition }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
