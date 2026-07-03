# [BROWSER_GUARD]

`route/guard.ts` is the navigation admission and confirm plane: one policy VALUE shape the route table's rows carry, one total fold that resolves a transition against the live verdict cells — `session/store`'s `SessionStatus`, `host`'s flag verdicts, `state`'s command availability — and one departure ceremony over held dirty scopes. The guard produces `route/navigate`'s own `Router.Admission` directly (`Proceed`/`Divert`/`Refuse`) — a second verdict family beside the commit vocabulary is the parallel-shape defect this page refuses — and the app hands `Guard.resolve` into the router's admission slot at composition, so navigate never imports guard and the seam stays one function value. Dirtiness is structural: a screen holds `Guard.hold` for exactly as long as unsaved work exists and the scope's release clears it, so a leaked dirty flag is unspellable; the native `beforeunload` arm covers tab close while the confirm port covers in-app departure, one registry beneath both.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                             | [PUBLIC]           |
| :-----: | :-------------- | :------------------------------------------------------------------ | :----------------- |
|  [01]   | `POLICY_VALUE`  | the per-route policy shape and its open constructor                 | `Guard` (types)    |
|  [02]   | `ADMISSION_FOLD`| the resolve chain, the settle wait, the dirty registry, the confirm | `Guard`, `Confirm` |

## [2]-[POLICY_VALUE]

[POLICY_VALUE]:
- Owner: `Guard.Policy`, one composable record — `session: Option<{ to }>` (the route demands an authenticated session, diverting to the login href otherwise), `flag: Option<{ name, to }>` (the route gates on a boolean flag verdict, diverting when it holds false), `command: Option<Availability.Command>` (the route gates on command availability evidence), `leave: Option<Guard.Prompt>` (departure from this route confirms while dirty work is held) — with `Guard.open` as the all-`none` row and `Guard.policy(overrides)` the spread constructor, so a route states only the gates it earns.
- Law: policy is data on the route row — `route/navigate`'s `Row.policy` carries this value, the app's admission wiring reads the departing and arriving rows, and no parallel policy registry exists; a new gate axis is one field here plus one fold arm in `[3]`, every table inheriting it untouched.
- Law: gate targets are hrefs minted by the router — `to` fields carry `Router` `href` output, so a divert target is typed at its mint site and the guard never assembles URLs.
- Growth: a tenant gate, a capability gate, or a quota gate is one `Option` field plus one chain arm.
- Boundary: what the flags mean is `host/flag`'s rollout law; what availability means is `state/evidence/availability`'s lattice; this page owns only the fold order.
- Packages: `effect` (`Option`); `@rasm/ts/state` (`Availability`).

## [3]-[ADMISSION_FOLD]

[ADMISSION_FOLD]:
- Owner: `Guard`, one scoped `Effect.Service` built through `Guard.Default(spec)` — `spec` carries the flag-targeting `subject` read, the app-composed `availability` snapshot read (`Option`-carried: no evidence feed composed means no command gating), and the `settle` budget an in-flight authentication may spend before the fold treats it as expired. Members: `resolve(departing, arriving)` — the one admission fold; `hold(token)` — the scoped dirty marker; `dirty` — the registry read. `Confirm` is the port Tag `ui` satisfies at composition: one prompt-to-boolean ceremony.
- Law: the chain is ordered and first-refusal-wins — departure confirm (only when the departing row carries `leave` AND dirty work is held), then session, then flag, then command; each arm yields `Proceed` to fall through, and the fold is total — every path lands an `Admission`, no throw, no absent arm.
- Law: `Authenticating` is waited out, never guessed — the session arm suspends on the status cell's change feed until a settled phase arrives or the `settle` budget lapses, and a lapse folds as `Expired` (divert), so a slow ceremony renders the router's pending affordance instead of a wrong verdict.
- Law: absence admits — a missing `Confirm` port proceeds (the native `beforeunload` arm still covers tab close), absent availability evidence admits, and only a `Withheld` verdict refuses (`Gated` proceeds — page-level affordances own gated rendering); the guard degrades open on missing evidence and shut on explicit refusal.
- Law: the `beforeunload` arm and its synchronous registry read are the module's platform-forced statement seam — the native handler must decide within its dispatch, so it reads the dirty cell through the captured runtime's sync run (the sanctioned callback-seam spelling) and prevents default only while work is held; the implementer carries the `// BOUNDARY ADAPTER` mark on `_departures`' first line.
- Receipt: `resolve`'s annotation states the whole read surface — `Router.Admission` out, no fault channel, requirement-free by construction.
- Entry: the app's composition maps the router's endpoint pair onto its rows — `(from, to) => guard.resolve(Option.some(rows[from.key].policy), rows[to.key].policy)` — one line in `main.ts`, zero lib coupling between the two pages.
- Boundary: the router consumes the fold through its admission slot; `ui` renders the confirm ceremony behind the `Confirm` Tag; the dirty affordance (disabled save, exit warning) reads `dirty` through the app-composed atom bridge.
- Packages: `effect` (`Effect`, `Option`, `HashSet`, `SubscriptionRef`, `Stream`, `Duration`, `Context`, `Runtime`); `@rasm/ts/host` (`Flags`, type `Rollout`); `@rasm/ts/state` (`Availability`); `../session/store.ts` (`Vault`, `SessionStatus`); `./navigate.ts` (`Router`).

```typescript
import { Flags, type Rollout } from "@rasm/ts/host"
import { Availability } from "@rasm/ts/state"
import { Context, type Duration, Effect, HashSet, Option, Runtime, type Scope, Stream, SubscriptionRef } from "effect"
import { SessionStatus, Vault } from "../session/store.ts"
import { Router } from "./navigate.ts"

declare namespace Guard {
  type Prompt = { readonly title: string; readonly detail: string }
  type Policy = {
    readonly session: Option.Option<{ readonly to: string }>
    readonly flag: Option.Option<{ readonly name: string; readonly to: string }>
    readonly command: Option.Option<Availability.Command>
    readonly leave: Option.Option<Prompt>
  }
  type Spec = {
    readonly subject: Effect.Effect<Rollout.Subject>
    readonly availability: Effect.Effect<Option.Option<Availability>>
    readonly settle: Duration.DurationInput
  }
}

const _OPEN: Guard.Policy = {
  session: Option.none(),
  flag: Option.none(),
  command: Option.none(),
  leave: Option.none(),
}

class Confirm extends Context.Tag("browser/route/Confirm")<Confirm, (prompt: Guard.Prompt) => Effect.Effect<boolean>>() {}

class Guard extends Effect.Service<Guard>()("browser/route/Guard", {
  scoped: (spec: Guard.Spec) =>
    Effect.gen(function* () {
      const vault = yield* Vault
      const flags = yield* Flags
      const confirm = yield* Effect.serviceOption(Confirm)
      const runtime = yield* Effect.runtime<never>()
      const dirty = yield* SubscriptionRef.make(HashSet.empty<string>())
      yield* Effect.acquireRelease(
        Effect.sync(() => {
          const handler = (event: BeforeUnloadEvent) => {
            const held = Runtime.runSync(runtime)(SubscriptionRef.get(dirty))
            if (HashSet.size(held) > 0) event.preventDefault()
          }
          globalThis.addEventListener("beforeunload", handler)
          return handler
        }),
        (handler) => Effect.sync(() => globalThis.removeEventListener("beforeunload", handler)),
      )
      const _settled: Effect.Effect<SessionStatus> = Effect.flatMap(SubscriptionRef.get(vault.status), (held) =>
        SessionStatus.$is("Authenticating")(held)
          ? vault.status.changes.pipe(
              Stream.filter((status) => !SessionStatus.$is("Authenticating")(status)),
              Stream.runHead,
              Effect.timeoutOption(spec.settle),
              Effect.map((outcome) => Option.getOrElse(Option.flatten(outcome), () => SessionStatus.Expired())),
            )
          : Effect.succeed(held),
      )
      const _asked = (prompt: Guard.Prompt): Effect.Effect<boolean> =>
        Option.match(confirm, { onNone: () => Effect.succeed(true), onSome: (ask) => ask(prompt) })
      const _leaveArm = (departing: Option.Option<Guard.Policy>): Effect.Effect<Router.Admission> =>
        Effect.flatMap(SubscriptionRef.get(dirty), (held) =>
          Option.match(Option.flatMap(departing, (policy) => policy.leave), {
            onNone: () => Effect.succeed(Router.Admission.Proceed()),
            onSome: (prompt) =>
              HashSet.size(held) === 0
                ? Effect.succeed(Router.Admission.Proceed())
                : Effect.map(_asked(prompt), (leave) => (leave ? Router.Admission.Proceed() : Router.Admission.Refuse())),
          }),
        )
      const _sessionArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(policy.session, {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: ({ to }) =>
            Effect.map(_settled, (status) =>
              SessionStatus.$is("Authenticated")(status) ? Router.Admission.Proceed() : Router.Admission.Divert({ to }),
            ),
        })
      const _flagArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(policy.flag, {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: ({ name, to }) =>
            Effect.flatMap(spec.subject, (subject) =>
              Effect.map(flags.evaluate(name, subject, false), (verdict) =>
                verdict.value === true ? Router.Admission.Proceed() : Router.Admission.Divert({ to }),
              ),
            ),
        })
      const _commandArm = (policy: Guard.Policy): Effect.Effect<Router.Admission> =>
        Option.match(policy.command, {
          onNone: () => Effect.succeed(Router.Admission.Proceed()),
          onSome: (command) =>
            Effect.map(spec.availability, (evidence) =>
              Option.match(evidence, {
                onNone: () => Router.Admission.Proceed(),
                onSome: (snapshot) =>
                  Availability.admits(snapshot, command)._tag === "Withheld"
                    ? Router.Admission.Refuse()
                    : Router.Admission.Proceed(),
              }),
            ),
        })
      const resolve = (departing: Option.Option<Guard.Policy>, arriving: Guard.Policy): Effect.Effect<Router.Admission> =>
        Effect.gen(function* () {
          const left = yield* _leaveArm(departing)
          if (left._tag !== "Proceed") return left
          const session = yield* _sessionArm(arriving)
          if (session._tag !== "Proceed") return session
          const flagged = yield* _flagArm(arriving)
          if (flagged._tag !== "Proceed") return flagged
          return yield* _commandArm(arriving)
        })
      const hold = (token: string): Effect.Effect<void, never, Scope.Scope> =>
        Effect.acquireRelease(
          SubscriptionRef.update(dirty, HashSet.add(token)),
          () => SubscriptionRef.update(dirty, HashSet.remove(token)),
        )
      return { resolve, hold, dirty }
    }),
  accessors: true,
}) {
  static readonly open: Guard.Policy = _OPEN
  static readonly policy = (overrides: Partial<Guard.Policy>): Guard.Policy => ({ ..._OPEN, ...overrides })
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Confirm, Guard }
```
