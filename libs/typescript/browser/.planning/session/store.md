# [BROWSER_STORE]

`session/store.ts` is the browser session/token storage law: credential material lives only where its class row permits, and one owner — `Vault` — holds the live `SessionStatus` cell, the silent-refresh arm, the cross-tab session channel, and the CSRF echo every mutating request stamps. The residency law is absolute: the access session travels as an `HttpOnly` cookie the page can never read (`security/session/cookie` frames it, the edge writes it — the browser holds zero token bytes); the CSRF token is the one deliberately readable cookie, echoed into a request header per the double-submit law; the pending ceremony record rides `persist/kv`'s `flow` domain; and session STATUS — subject, expiry, phase — is memory-only state in one `SubscriptionRef`, reconstructed on every boot, never written to Web Storage. A token string in `localStorage`, a readable session cookie, or a second status cell beside this one is the named residency defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              | [PUBLIC]                 |
| :-----: | :--------------- | :------------------------------------------------------------------ | :----------------------- |
|  [01]   | `STATUS_FAMILY`  | the session phase vocabulary and the cross-tab signal wire          | `SessionStatus`          |
|  [02]   | `VAULT_OWNER`    | the cell, transitions, refresh arm, channel fold, and request rows  | `Vault`                  |

## [2]-[STATUS_FAMILY]

[STATUS_FAMILY]:
- Owner: `SessionStatus`, one process-local `Data.taggedEnum` — `Anonymous`, `Authenticating`, `Authenticated { subject, expiresAt }`, `Expired` — constructed only through its generated constructors so every guard and affordance dispatch rides `$match`/`$is` and a hand-listed `_tag` union cannot exist; `_Signal` is the interior cross-tab wire — a `Schema.parseJson` union of `Established`/`Cleared` — decoded once at the channel seam, so a foreign tab's message is admitted material, never trusted shape.
- Law: `Authenticated` carries evidence, not secrets — `subject` is a display-grade identity string and `expiresAt` the refresh watermark; the credential itself never exists in this vocabulary because the cookie residency law keeps it out of script reach entirely.
- Law: the wire and the cell are distinct planes — `_Signal` exists only to synchronize tabs and carries the minimal transition facts; `Expired` and `Authenticating` are local phases that never cross the channel, because a foreign tab observing them would act on another tab's transient.
- Growth: a new phase is one case on the enum plus its `$match` arms breaking loudly; a new cross-tab fact is one `_Signal` member.
- Boundary: `security/session/token` owns the server-side `Session`/`TokenPair` truth; `route/guard` folds this cell into admission verdicts; `session/ceremony` drives the transitions.
- Packages: `effect` (`Data`, `Schema`, `DateTime`).

## [3]-[VAULT_OWNER]

[VAULT_OWNER]:
- Owner: `Vault`, one scoped `Effect.Service` built through the parameterized `Vault.Default(spec)` factory — `spec` carries the CSRF cookie name (fed from `security/session/cookie`'s spec at the app root, never respelled here), the refresh lead `Duration`, and the app-supplied `refresh` effect (the edge round-trip that re-establishes the cookie session, yielding `Option` of the fresh subject/expiry). Members: `status` (the one cell, published `Subscribable` — the transitions and the owned folds are its only writers), `established`/`authenticating`/`cleared` (the transitions — local transitions publish to the channel, foreign folds never re-publish, so the channel cannot echo), `csrf` (the header pair read from the readable cookie, `Option`-carried), and `posture` (`"include"` — the credentials row `transport/fetch` stamps on every dial).
- Law: the refresh arm is supersede-keyed — one `FiberHandle` holds at most one sleeper; each `Authenticated` transition re-arms it to wake at `expiresAt` minus the lead, run `spec.refresh`, and fold the outcome (`some` re-establishes, `none` expires); any other phase replaces the sleeper with `Effect.void`, so a signed-out tab holds no timer and two sleepers cannot race; a refresh success publishes the fresh expiry to the channel, so sibling tabs fold forward and re-arm to the later watermark — the first refresher wins and the others move their timers instead of re-dialing.
- Law: cross-tab truth is one `BroadcastChannel` held `Effect.acquireRelease` — a decoded `Established` folds the foreign fact into the local cell, `Cleared` signs every tab out at once, and an undecodable message is dropped (`Effect.ignore`) because a foreign tab's garbage is not this rail's fault; the channel name is the module's one wire constant.
- Law: the CSRF read is the one sanctioned `document.cookie` touch in the branch — the cookie scan is expression-shaped over the split rows, the value decodes through `Encoding.decodeUriComponent` (an `Either`, never a thrown `URIError`), and absence is `Option.none` the caller folds — a mutating request without the echo is refused server-side, so no browser-side guard re-checks it.
- Receipt: `csrf` yields the ready `[name, value]` header pair; `posture` is the literal fetch credentials row — both consumed by `transport/fetch` decoration, so request stamping is recoverable from this declaration.
- Boundary: cookie attribute policy is `security/session/cookie`'s table; the refresh endpoint is app data the composition root supplies; this owner never dials.
- Packages: `effect` (`Effect`, `SubscriptionRef`, `Subscribable`, `FiberHandle`, `Stream`, `Schema`, `DateTime`, `Duration`, `Order`, `Option`, `Array`, `Encoding`, `Data`).

```typescript
import { Array, Data, DateTime, Duration, Effect, Encoding, FiberHandle, Option, Order, Schema, Stream, Subscribable, SubscriptionRef } from "effect"

const _CHANNEL = "rasm-session"

const _Signal = Schema.parseJson(Schema.Union(
  Schema.TaggedStruct("Established", { subject: Schema.NonEmptyString, expiresAt: Schema.DateTimeUtc }),
  Schema.TaggedStruct("Cleared", {}),
))

type SessionStatus = Data.TaggedEnum<{
  Anonymous: {}
  Authenticating: {}
  Authenticated: { readonly subject: string; readonly expiresAt: DateTime.Utc }
  Expired: {}
}>
const SessionStatus: Data.TaggedEnum.Constructor<SessionStatus> = Data.taggedEnum<SessionStatus>()

declare namespace Vault {
  type Fresh = { readonly subject: string; readonly expiresAt: DateTime.Utc }
  type Spec = {
    readonly csrf: string
    readonly lead: Duration.DurationInput
    readonly refresh: Effect.Effect<Option.Option<Fresh>>
  }
}

const _cookie = (name: string): Option.Option<string> =>
  Array.findFirst(globalThis.document.cookie.split("; "), (row) => row.startsWith(`${name}=`)).pipe(
    Option.flatMap((row) => Option.getRight(Encoding.decodeUriComponent(row.slice(name.length + 1)))),
  )

class Vault extends Effect.Service<Vault>()("browser/session/Vault", {
  scoped: (spec: Vault.Spec) =>
    Effect.gen(function* () {
      const _status = yield* SubscriptionRef.make<SessionStatus>(SessionStatus.Anonymous())
      const sleeper = yield* FiberHandle.make()
      const channel = yield* Effect.acquireRelease(
        Effect.sync(() => new globalThis.BroadcastChannel(_CHANNEL)),
        (held) => Effect.sync(() => held.close()),
      )
      const _publish = (signal: typeof _Signal.Type): Effect.Effect<void> =>
        Effect.sync(() => channel.postMessage(Schema.encodeSync(_Signal)(signal)))
      const _armed = (held: SessionStatus): Effect.Effect<void> =>
        SessionStatus.$match(held, {
          Anonymous: () => FiberHandle.run(sleeper, Effect.void),
          Authenticating: () => FiberHandle.run(sleeper, Effect.void),
          Expired: () => FiberHandle.run(sleeper, Effect.void),
          Authenticated: ({ expiresAt }) =>
            FiberHandle.run(
              sleeper,
              Effect.gen(function* () {
                const now = yield* DateTime.now
                const wait = Order.max(Duration.Order)(
                  Duration.subtract(DateTime.distanceDuration(now, expiresAt), Duration.decode(spec.lead)),
                  Duration.zero,
                )
                yield* Effect.sleep(wait)
                const fresh = yield* spec.refresh
                yield* Option.match(fresh, {
                  onNone: () => SubscriptionRef.set(_status, SessionStatus.Expired()),
                  onSome: ({ expiresAt, subject }) =>
                    SubscriptionRef.set(_status, SessionStatus.Authenticated({ subject, expiresAt })).pipe(
                      Effect.zipRight(_publish({ _tag: "Established", subject, expiresAt })),
                    ),
                })
              }),
            ),
        })
      yield* Stream.fromEventListener<MessageEvent>(channel, "message").pipe(
        Stream.runForEach((event) =>
          Schema.decodeUnknown(_Signal)(event.data).pipe(
            Effect.flatMap((signal) =>
              SubscriptionRef.set(
                _status,
                signal._tag === "Established"
                  ? SessionStatus.Authenticated({ subject: signal.subject, expiresAt: signal.expiresAt })
                  : SessionStatus.Anonymous(),
              ),
            ),
            Effect.ignore,
          ),
        ),
        Effect.forkScoped,
      )
      yield* _status.changes.pipe(Stream.runForEach(_armed), Effect.forkScoped)
      const status: Subscribable.Subscribable<SessionStatus> = _status
      return {
        status,
        posture: "include" as const,
        established: (fresh: Vault.Fresh) =>
          SubscriptionRef.set(_status, SessionStatus.Authenticated(fresh)).pipe(
            Effect.zipRight(_publish({ _tag: "Established", subject: fresh.subject, expiresAt: fresh.expiresAt })),
          ),
        authenticating: SubscriptionRef.set(_status, SessionStatus.Authenticating()),
        cleared: SubscriptionRef.set(_status, SessionStatus.Anonymous()).pipe(
          Effect.zipRight(_publish({ _tag: "Cleared" })),
        ),
        csrf: Effect.sync(() => Option.map(_cookie(spec.csrf), (token) => [spec.csrf, token] as const)),
      }
    }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { SessionStatus, Vault }
```
