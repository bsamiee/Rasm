# [BROWSER_CEREMONY]

`session/ceremony.ts` runs the browser halves of the two authentication ceremonies over `security`'s runtime-neutral subpaths, and only the halves: the passkey ceremony invokes `navigator.credentials` through `security`'s `Passkeys` browser module and never verifies — the RP-side verify is `security/authn/webauthn`'s node subpath, reached over the wire by app-supplied exchange legs; the OAuth ceremony owns the redirect continuity a full-page IdP navigation destroys — the pending-flow record persisted into `persist/kv`'s `flow` domain before departure, consumed single-use at landing, with the code exchange again an app-supplied leg because the PKCE verifier and state stash live server-side in `security/authn/oauth`. Both ceremonies drive `session/store`'s `SessionStatus` transitions through one owner, so phase truth has one writer; node-only crypto stays out of the bundle by construction because this module imports only `security`'s browser subpath. A browser-side verifier, a second pending-flow stash, or a ceremony invoked outside this owner is the named defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             | [PUBLIC]                    |
| :-----: | :--------------- | :------------------------------------------------------------------ | :-------------------------- |
|  [01]   | `FAULT_FAMILY`   | the ceremony fault vocabulary and its policy rows                   | `CeremonyFault`             |
|  [02]   | `CEREMONY_OWNER` | the passkey round-trips and the OAuth depart/land continuity        | `Ceremony`                  |

## [2]-[FAULT_FAMILY]

[FAULT_FAMILY]:
- Owner: `CeremonyFault`, the folder's ceremony rail — `unsupported` (the host ships no WebAuthn), `refused` (the user dismissed the credential prompt), `ceremony` (every other `PasskeyFault` code, carried as detail), `replay` (a landing with no pending flow or a state echo mismatch), `lapsed` (a pending flow older than the grace window), `malformed` (a callback URL missing its code) — with the policy table carrying rank and retryability so recovery reads rows, never tags.
- Law: `PasskeyFault` folds at this seam — `security`'s browser module pre-classifies the `WebAuthnError` code, and the fold maps `ERROR_UNSUPPORTED` to `unsupported`, the user-abort code to `refused`, and the remainder to `ceremony` with the original code as evidence; the security fault never travels past this module, so consumers dispatch one family.
- Law: every reason is terminal for the attempt — no ceremony fault retries mechanically (`retry: false` across the table); a `refused` or `lapsed` outcome is a user-facing affordance decision, which is exactly why the rows carry rank for the caller's severity fold.
- Growth: a new refusal cause is one reason row plus one fold arm.
- Packages: `effect` (`Data`).

## [3]-[CEREMONY_OWNER]

[CEREMONY_OWNER]:
- Owner: `Ceremony`, one `Effect.Service` over `Vault` and `Kv` — `enroll(legs)` and `assert(legs)` run the passkey round-trips: `legs.start` fetches the ceremony options from the edge, the interior invokes `Passkeys.register`/`Passkeys.authenticate`, and `legs.finish` delivers the signed response back; the interior `_ceremony` fold is generic over the option and response shapes, so both entries inherit their exact types from `Passkeys`' own signatures and this module respells none of them. `depart(plan)` and `land(url, exchange)` run the OAuth continuity: depart persists the pending flow — the return target and the `Option`-carried state echo the plan mirrors from its authorize URL — then commits the full-page navigation; land re-reads the flow single-use, guards replay, lapse, and the echo, extracts the callback code, and hands `{ code, state }` to the exchange leg.
- Law: phase transitions are this owner's writes — every entry opens with `Vault.authenticating`; `assert` and `land` close by folding the exchange's `Vault.Fresh` into `Vault.established`; a failed leg leaves the cell for the caller's fold, never a half-authenticated phase.
- Law: the pending flow is single-use and time-bounded — `land` drops the record before acting on it, so a replayed callback finds nothing and folds to `replay`; a record older than the `grace` window folds to `lapsed`; the state echo, when the record carries one, must equal the callback's — defense in depth beside the server-side single-use stash `security/authn/oauth` owns.
- Law: the departure commit is the module's platform-forced statement seam — `location.assign` unloads the document, so nothing sequences after it; the implementer carries the `// BOUNDARY ADAPTER` mark on `_departed`'s first line.
- Law: the option and response types derive from `Passkeys`' own signatures onto this owner's merged namespace (`Ceremony.EnrollOptions` and siblings) — one derivation site, zero respelled `@simplewebauthn` names, and the derivation retires the moment the security owner carries the companions itself.
- Receipt: `land` yields the flow's `returnTo` beside the established session so `route/navigate` restores the interrupted destination; `enroll` yields the finish leg's own receipt untouched.
- Boundary: option minting, challenge stash, and verification are `security`'s server subpath behind the app's edge exchange legs; the callback URL arrives from `route/navigate`'s landing arm; cookie framing is the edge's.
- Packages: `@rasm/ts/security/browser` (`Passkeys`, `PasskeyFault`); `effect` (`Effect`, `Option`, `DateTime`, `Duration`, `Data`); `../persist/kv.ts` (`Kv`); `./store.ts` (`Vault`).

```typescript
import { Passkeys, type PasskeyFault } from "@rasm/ts/security/browser"
import { Data, DateTime, Duration, Effect, Option } from "effect"
import { Kv, type KvFault } from "../persist/kv.ts"
import { Vault } from "./store.ts"

const _FLOW_KEY = "pending"

const CeremonyFaultPolicy = {
  unsupported: { rank: 4, retry: false },
  refused: { rank: 2, retry: false },
  ceremony: { rank: 3, retry: false },
  replay: { rank: 5, retry: false },
  lapsed: { rank: 3, retry: false },
  malformed: { rank: 4, retry: false },
} as const

declare namespace CeremonyFault {
  type Reason = keyof typeof CeremonyFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof CeremonyFaultPolicy> = T
}

class CeremonyFault extends Data.TaggedError("CeremonyFault")<{
  readonly reason: CeremonyFault.Reason
  readonly detail: string
}> {
  get policy(): CeremonyFault.Row {
    return CeremonyFaultPolicy[this.reason]
  }
}

const _folded = (fault: PasskeyFault): CeremonyFault =>
  fault.code === "ERROR_UNSUPPORTED"
    ? new CeremonyFault({ reason: "unsupported", detail: fault.detail })
    : fault.code === "ERROR_CEREMONY_ABORTED"
      ? new CeremonyFault({ reason: "refused", detail: fault.detail })
      : new CeremonyFault({ reason: "ceremony", detail: fault.code })

declare namespace Ceremony {
  type Legs<O, R2, A, E> = {
    readonly start: Effect.Effect<O, E>
    readonly finish: (response: R2) => Effect.Effect<A, E>
  }
  type EnrollOptions = Parameters<typeof Passkeys.register>[0]
  type EnrollResponse = Effect.Effect.Success<ReturnType<typeof Passkeys.register>>
  type AssertOptions = Parameters<typeof Passkeys.authenticate>[0]
  type AssertResponse = Effect.Effect.Success<ReturnType<typeof Passkeys.authenticate>>
  type Plan = { readonly authorize: URL; readonly returnTo: string; readonly state: Option.Option<string> }
  type Landing = { readonly returnTo: string }
  type Exchange<E> = (payload: {
    readonly code: string
    readonly state: Option.Option<string>
  }) => Effect.Effect<Vault.Fresh, E>
}

class Ceremony extends Effect.Service<Ceremony>()("browser/session/Ceremony", {
  effect: (grace: Duration.DurationInput) =>
    Effect.gen(function* () {
      const kv = yield* Kv
      const vault = yield* Vault
      const _ceremony = <O, R2, A, E>(
        invoke: (options: O) => Effect.Effect<R2, PasskeyFault>,
        legs: Ceremony.Legs<O, R2, A, E>,
      ): Effect.Effect<A, CeremonyFault | E> =>
        vault.authenticating.pipe(
          Effect.zipRight(legs.start),
          Effect.flatMap((options) => Effect.mapError(invoke(options), _folded)),
          Effect.flatMap(legs.finish),
        )
      const _departed = (target: URL): Effect.Effect<never> =>
        Effect.zipRight(Effect.sync(() => globalThis.location.assign(target.toString())), Effect.never)
      const depart = (plan: Ceremony.Plan): Effect.Effect<never, KvFault> =>
        vault.authenticating.pipe(
          Effect.zipRight(
            Effect.flatMap(DateTime.now, (minted) =>
              kv.write("flow", _FLOW_KEY, { state: plan.state, returnTo: plan.returnTo, minted }),
            ),
          ),
          Effect.zipRight(_departed(plan.authorize)),
        )
      const land = <E>(
        callback: URL,
        exchange: Ceremony.Exchange<E>,
      ): Effect.Effect<Ceremony.Landing, CeremonyFault | KvFault | E> =>
        Effect.gen(function* () {
          const held = yield* kv.read("flow", _FLOW_KEY)
          yield* kv.drop("flow", _FLOW_KEY)
          const flow = yield* Option.match(held, {
            onNone: () => new CeremonyFault({ reason: "replay", detail: "<no-pending-flow>" }),
            onSome: Effect.succeed,
          })
          const now = yield* DateTime.now
          yield* Effect.when(
            new CeremonyFault({ reason: "lapsed", detail: "<flow-expired>" }),
            () => Duration.greaterThan(DateTime.distanceDuration(flow.minted, now), Duration.decode(grace)),
          )
          const state = Option.fromNullable(callback.searchParams.get("state"))
          yield* Effect.when(
            new CeremonyFault({ reason: "replay", detail: "<state-mismatch>" }),
            () =>
              Option.match(flow.state, {
                onNone: () => false,
                onSome: (expected) => !Option.contains(state, expected),
              }),
          )
          const code = yield* Option.match(Option.fromNullable(callback.searchParams.get("code")), {
            onNone: () => new CeremonyFault({ reason: "malformed", detail: "<no-code>" }),
            onSome: Effect.succeed,
          })
          const fresh = yield* exchange({ code, state })
          yield* vault.established(fresh)
          return { returnTo: flow.returnTo }
        })
      return {
        enroll: <A, E>(legs: Ceremony.Legs<Ceremony.EnrollOptions, Ceremony.EnrollResponse, A, E>) =>
          _ceremony(Passkeys.register, legs),
        assert: <E>(legs: Ceremony.Legs<Ceremony.AssertOptions, Ceremony.AssertResponse, Vault.Fresh, E>) =>
          Effect.tap(_ceremony(Passkeys.authenticate, legs), (fresh) => vault.established(fresh)),
        depart,
        land,
      }
    }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ceremony, CeremonyFault }
```
