# [EDGE_PRESENCE]

`live/presence.ts` is the admission gate of the realtime family: who may subscribe to which channel, whether presence is served there, how many live subscriptions one principal may hold, and which lease policy governs a channel's liveness verdicts — all data in one channel-rule table the app constructs, matched by longest prefix so channel families are one row, not one row per channel. `state` owns the presence FOLD (`Presence.plan`, `Presence.roster`); this page owns everything around it at the edge: the scope-checked subscription grant, the stamp guard that pins a decoded presence op to the authenticated principal before it may reach the fold, the roster read against a caller-held horizon, and the per-connection subscription registry whose fan cap sheds structurally. Heartbeat cadence and socket lifecycle are edge policy here; liveness itself stays a `state` read-time verdict, so no timer fiber sweeps anything.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                             | [PUBLIC]    |
| :-----: | :---------------- | :-------------------------------------------------------------------- | :---------- |
|  [01]   | [CHANNEL_RULES]   | the rule shape, the prefix-matched rule table, the subscription grant  | `Admission` |
|  [02]   | [PRESENCE_GUARD]  | the op stamp guard and the horizon-parameterized roster read           | `Admission` |
|  [03]   | [FAN_REGISTRY]    | the per-connection subscription registry and its shed law              | `Admission` |

## [2]-[CHANNEL_RULES]

[CHANNEL_RULES]:
- Owner: `Admission.make(rules)` — one constructor over the app's channel-rule rows: each row keys a channel PREFIX and carries `scope` (the `Principal` scope a subscriber must hold, `Option` for public channels), `presence` (whether the channel serves a roster), `fan` (the per-principal live-subscription cap), and `lease` (the `state` `Presence.Lease` liveness windows) — held in a `Trie` so `Trie.longestPrefixOf` resolves any concrete channel to its most specific family row in one read, and a channel family (`board:` and everything under it) is one row.
- Law: admission is a two-gate fold — the channel must resolve to a rule (an unmatched channel is `denied`, never a default-open), and the rule's scope, when present, must pass `Principal.allows` — producing a `Grant` that carries the resolved rule, so every later decision (roster serving, fan cap, lease) reads the grant's own row and no downstream re-looks anything up.
- Law: the rule table is app data under a lib shape — the lib ships the vocabulary and the fold; which channels exist is composition material, so two apps with different channel maps share every line of this module.
- Growth: a new admission axis (a rate row, a payload ceiling) is one `Rule` field read at its gate; a new channel family is one app-side row.
- Packages: `effect` (`Trie`, `Option`, `Schema`, `Effect`); `api/middleware` (`Principal`); `live/socket` (`LiveFault`).

```typescript
import { Hlc } from "@rasm/ts/kernel"
import { Presence } from "@rasm/ts/state"
import { type DateTime, Duration, Effect, FiberMap, HashMap, Option, Schema, type Scope, Stream, type Subscribable, Trie } from "effect"
import { Principal } from "../api/middleware.ts"
import { LiveFault } from "./socket.ts"

const _Channel = Schema.NonEmptyString.pipe(Schema.maxLength(128), Schema.pattern(/^[a-z0-9][a-z0-9:_-]*$/), Schema.brand("Channel"))

const _LEASE = { idle: Duration.seconds(30), gone: Duration.minutes(5) } as const

declare namespace Admission {
  type Channel = typeof _Channel.Type
  type Rule = {
    readonly scope: Option.Option<string>
    readonly presence: boolean
    readonly fan: number
    readonly lease: Presence.Lease
  }
  type Grant = { readonly channel: Channel; readonly rule: Rule }
  type Gates = {
    readonly admit: (principal: Principal.Shape, channel: Channel) => Effect.Effect<Grant, LiveFault>
    readonly guard: typeof _guard
    readonly roster: typeof _roster
    readonly subscribe: <E>(grant: Grant, run: Stream.Stream<void, E>) => Effect.Effect<void, LiveFault>
  }
}

const _admit = (rules: Trie.Trie<Admission.Rule>) =>
  (principal: Principal.Shape, channel: Admission.Channel): Effect.Effect<Admission.Grant, LiveFault> =>
    Option.match(Trie.longestPrefixOf(rules, channel), {
      onNone: () => Effect.fail(new LiveFault({ reason: "denied", detail: channel })),
      onSome: ([, rule]) =>
        Option.match(rule.scope, {
          onNone: () => Effect.succeed({ channel, rule }),
          onSome: (scope) =>
            Principal.allows(principal, scope)
              ? Effect.succeed({ channel, rule })
              : Effect.fail(new LiveFault({ reason: "denied", detail: scope })),
        }),
    })
```

## [3]-[PRESENCE_GUARD]

[PRESENCE_GUARD]:
- Law: the stamp guard pins identity — a decoded `Presence.Op` may reach the fold only when its `actor` equals the authenticated principal's subject and the grant's channel serves presence; a mismatched actor is `denied` with the op discarded, so presence forgery is refused at the edge and the `state` fold never carries an authorization concern.
- Law: forwarding is a supplied sink — `guard(grant, principal, forward)` composes the app-wired `(op) => Effect<void>` that feeds the channel's replay fold; the edge never reaches into `state`'s graph, so where the fold runs (in-process handle, durable node) is composition, not edge law.
- Law: the roster read is horizon-parameterized — `roster(grant, view, horizon)` projects the channel's folded table through `Presence.roster` under the grant's lease at the horizon the socket loop minted (`Hlc.tick(Hlc.genesis, Hlc.physicalOf(now))`), so liveness is a pure read-time verdict and a sweep is just a re-read with a fresh horizon — no timer mutates presence, the `state` law inherited verbatim.
- Law: heartbeat cadence is edge policy — the socket row's `ping` cadence doubles as the client beat interval, and `_LEASE` defaults size the liveness windows against it (idle at one missed beat, gone at ten) so the two policies cannot drift apart silently; a channel overrides both through its rule row.
- Boundary: `Presence.Op` decode happens at the socket frame seam (`live/socket.ts` inbound schema); status semantics, the op family, and the merge product are `state/query/live`'s.

```typescript
const _guard = (
  grant: Admission.Grant,
  principal: Principal.Shape,
  forward: (op: Presence.Op) => Effect.Effect<void>,
) =>
  (op: Presence.Op): Effect.Effect<void, LiveFault> =>
    grant.rule.presence && op.actor === principal.subject
      ? forward(op)
      : Effect.fail(new LiveFault({ reason: "denied", detail: op.actor }))

const _roster = (
  grant: Admission.Grant,
  view: Subscribable.Subscribable<HashMap.HashMap<Presence.Actor, Presence.State>>,
  now: DateTime.Utc,
): Effect.Effect<HashMap.HashMap<Presence.Actor, Presence.Status>, LiveFault> =>
  grant.rule.presence
    ? Effect.map(view.get, (table) => Presence.roster(table, Hlc.tick(Hlc.genesis, Hlc.physicalOf(now)), grant.rule.lease))
    : Effect.fail(new LiveFault({ reason: "denied", detail: grant.channel }))
```

## [4]-[FAN_REGISTRY]

[FAN_REGISTRY]:
- Owner: the per-connection registry — one scoped `FiberMap` keyed by channel: `subscribe(grant, run)` refuses over the grant's fan cap with `shed`, then `FiberMap.run` supersedes any prior subscription to the same channel (a re-subscribe replaces, never duplicates), and closing the connection's scope interrupts every member — so a leaked subscription fiber is structurally impossible and per-connection teardown is scope closure, not bookkeeping.
- Law: the cap reads live size — `FiberMap.size` against the grant's `fan` row at admission time; refusal is immediate (`shed` carries no retry window — the client must drop a subscription first), and the cap is per-connection per-principal because the registry is per-connection by construction.
- Law: the assembled owner is the page's one export shape — `Admission.make(rules)` yields `{ admit, guard, roster, subscribe }` closed over the rule table and the registry, so a socket session composes one value and the four gates cannot be half-applied.
- Packages: `effect` (`FiberMap`, `Scope`, `Stream`).

```typescript
const _registry = (): Effect.Effect<
  <E>(grant: Admission.Grant, run: Stream.Stream<void, E>) => Effect.Effect<void, LiveFault>,
  never,
  Scope.Scope
> =>
  Effect.map(FiberMap.make<Admission.Channel, void, unknown>(), (members) =>
    (grant, run) =>
      Effect.flatMap(FiberMap.size(members), (held) =>
        held >= grant.rule.fan
          ? Effect.fail(new LiveFault({ reason: "shed", detail: grant.channel }))
          : Effect.asVoid(FiberMap.run(members, grant.channel, Stream.runDrain(run)))))

const Admission: {
  readonly Channel: typeof _Channel
  readonly lease: typeof _LEASE
  readonly make: (rules: Iterable<readonly [string, Admission.Rule]>) => Effect.Effect<Admission.Gates, never, Scope.Scope>
} = {
  Channel: _Channel,
  lease: _LEASE,
  make: (rules) =>
    Effect.map(_registry(), (subscribe) => ({
      admit: _admit(Trie.fromIterable(rules)),
      guard: _guard,
      roster: _roster,
      subscribe,
    })),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Admission }
```
