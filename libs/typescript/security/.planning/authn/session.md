# [SECURITY_SESSION]

The identity plane's session owner: the `Subject`/`Session`/`CredentialRef`/`TokenPair` vocabulary, rotation as a data-carried statechart with reuse detection, the `SessionStore`/`IdentityJournal` ports, the `BearerGuard` middleware seam, and the cookie framing plus CSRF double-submit law as the session's egress projection — one owner because token minting and cookie framing are two faces of one session lifecycle. `Token` mints an access JWT through `crypt/sign`'s `Jwt` and an opaque high-entropy refresh through `Crypto`, storing only the refresh's SHA-256 fingerprint — argon2 is for low-entropy credentials; a random refresh needs a fast constant-time compare, never a per-check KDF. Rotation is the security-critical fold and it is a transition family, not an imperative flow: `_step` folds a live session, the current instant, and the constant-time match verdict into the `RotationStep` case — `Rotate`, `Expired`, `Reused` — and `$match` dispatches each arm, so the guard set is data and a new lifecycle rule is a case. The `Reused` arm is the breach arm: it increments `security_token_reuse`, logs at error, and revokes every session of the subject family before the fault surfaces — a replayed rotated token never fails silently. Every `refresh` runs under a store-backed per-`sid` `RateLimiter`, session and subject ids mint through the `Crypto` entropy port so tests inject determinism, and the `${sid}.${secret}` wire frame decodes through one `Schema.TemplateLiteralParser` owner, never a hand split. `Cookie` frames the `TokenPair` through the `@effect/platform` `Cookies` codec — the attribute policy stays in the `CookieSpec` table, serialization and the `__Host-`/`__Secure-` prefix semantics belong to the platform, and the edge folds the returned collection through `Cookies.toSetCookieHeaders` with `Headers.redact` covering the log path. The two `Context.Tag` ports carry identity state out of the folder so a zero-durable-state browser app composes `security` without the data wave; the app root satisfies both with the data journal. Tenancy is the core `TenantContext.Key`; `SessionFault`/`CsrfFault` instantiate the folder fault shape with the guard pair closed in both directions.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [PUBLIC]                                                                                              |
| :-----: | :--------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `IDENTITY_VOCAB` | `Subject`, `Session`, `CredentialRef`, `TokenPair`, `SessionFault`, `SessionStore`, `IdentityJournal` |
|  [02]   | `ROTATION_LAW`   | `Token`, `RotationStep`, `BearerGuard`, `CurrentClaims`                                               |
|  [03]   | `COOKIE_EGRESS`  | `Cookie`, `CookieSpec`, `CsrfFault`                                                                   |

## [02]-[IDENTITY_VOCAB]

[IDENTITY_VOCAB]:
- Owner: `Subject` is the durable identity (branded `id`, tenant key, verification), `Session` the live session (branded `id`, subject, tenant, scope, window, refresh fingerprint, rotation generation — the tenant rides the session so a rotated access token keeps its tenancy claim), `CredentialRef` the `{ kind, key }` an authn ceremony resolves through, `TokenPair` the mint receipt; `SessionFault` is the folder fault shape; `SessionStore`/`IdentityJournal` are the identity-state ports.
- Law: each id brand is a field refinement so `Subject["id"]` is the only `SubjectId` spelling; the tenant is the core `TenantContext.Key` brand, never a re-declaration; the ports declare their fault on the `E` channel.
- Law: a port exists exactly because the wave ledger forbids `security → data` — the data journal satisfies both at the app root (the `security/authn/session → data` [PORT] seam), so a Tag minted to dodge a legal edge is the defect.
- Growth: a new credential kind is one `CredentialRef.kind` literal; a new session facet is one `Session` field the store persists.
- Boundary: the authn ceremonies resolve a `CredentialRef` and call `Token.establish`; `access/claim` keys claims by `Subject["id"]`; `crypt/sign` supplies the JWT and crypto primitives.
- Packages: `effect` (`Schema`, `Context`); `@rasm/ts/core` (`TenantContext`, `FaultClass`).

```typescript
import * as RateLimiter from "@effect/experimental/RateLimiter"
import { Cookies, HttpApiMiddleware, HttpApiSecurity } from "@effect/platform"
import { FaultClass, TenantContext } from "@rasm/ts/core"
import { Config, Context, Data, DateTime, Duration, Effect, Layer, Metric, Option, Redacted, Schema } from "effect"
import { AccessClaims, Crypto, Jwt, Probe } from "../crypt/sign.ts"

const _SubjectId = Schema.UUID.pipe(Schema.brand("SubjectId"))
const _SessionId = Schema.UUID.pipe(Schema.brand("SessionId"))

const _reasons = ["expired", "notFound", "reuse", "mismatch", "throttled", "store"] as const

const _faults = {
  expired: { class: "expired" },
  notFound: { class: "absent" },
  reuse: { class: "breached" },
  mismatch: { class: "malformed" },
  throttled: { class: "exhausted" },
  store: { class: "unavailable" },
} as const

declare namespace SessionFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class Subject extends Schema.Class<Subject>("Subject")({
  id: _SubjectId,
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
  verified: Schema.Boolean,
}) {}

class CredentialRef extends Schema.Class<CredentialRef>("CredentialRef")({
  kind: Schema.Literal("oauth", "webauthn", "otp", "apikey", "password"),
  key: Schema.NonEmptyString,
}) {}

class Session extends Schema.Class<Session>("Session")({
  id: _SessionId,
  subject: _SubjectId,
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
  scope: Schema.Array(Schema.NonEmptyString),
  issuedAt: Schema.DateTimeUtc,
  expiresAt: Schema.DateTimeUtc,
  refreshHash: Schema.String,
  generation: Schema.Int,
}) {}

class TokenPair extends Schema.Class<TokenPair>("TokenPair")({
  access: Schema.Redacted(Schema.String),
  refresh: Schema.Redacted(Schema.String),
  session: Session,
}) {}

class SessionFault extends Schema.TaggedError<SessionFault>()("SessionFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<session:${this.reason}> ${this.detail}`
  }
}

class SessionStore extends Context.Tag("security/authn/SessionStore")<SessionStore, {
  readonly create: (session: Session) => Effect.Effect<void, SessionFault>
  readonly read: (id: Session["id"]) => Effect.Effect<Option.Option<Session>, SessionFault>
  readonly replace: (session: Session) => Effect.Effect<void, SessionFault>
  readonly revoke: (id: Session["id"]) => Effect.Effect<void, SessionFault>
  readonly revokeSubject: (subject: Subject["id"]) => Effect.Effect<void, SessionFault>
}>() {}

class IdentityJournal extends Context.Tag("security/authn/IdentityJournal")<IdentityJournal, {
  readonly resolve: (ref: CredentialRef) => Effect.Effect<Option.Option<Subject>, SessionFault>
  readonly enroll: (subject: Subject, ref: CredentialRef) => Effect.Effect<void, SessionFault>
  readonly credentials: (subject: Subject["id"]) => Effect.Effect<ReadonlyArray<CredentialRef>, SessionFault>
  readonly revokeCredential: (ref: CredentialRef) => Effect.Effect<void, SessionFault>
}>() {}
```

## [03]-[ROTATION_LAW]

[ROTATION_LAW]:
- Owner: `Token` — `establish` resolves-or-enrolls a `CredentialRef` into a `Subject` and mints the first pair, `refresh` rotates through the `RotationStep` statechart with reuse detection, `revoke` ends a session. `RotationStep` is the transition family — `Rotate` (live session, fingerprint matched), `Expired` (window elapsed), `Reused` (live session, fingerprint rejected — a replayed rotated token) — folded by the pure `_step` and dispatched by `$match`, so protocol position is a case value and a new lifecycle rule (idle timeout, device binding) is a new tagged arm, never another inline guard. `CurrentClaims`/`BearerGuard` are the declarative auth seam: the middleware Tag carries `HttpApiSecurity.bearer`, its implementation folds `Jwt.verify` over the decoded credential, and the runtime serve wave mounts it so every bearer-protected endpoint receives `AccessClaims` through the requirement channel.
- Law: the access token is a `crypt/sign` `AccessClaims` JWT; the refresh is a `Crypto` opaque token whose SHA-256 fingerprint alone is stored; the wire form is `${sid}.${secret}` decoded through the `_RefreshWire` `Schema.TemplateLiteralParser` owner so `refresh` reads the session before touching the secret and a malformed frame is one typed `mismatch`.
- Law: the `Reused` arm is the breach arm and it is loud — `security_token_reuse` increments, the error log lands with the subject annotation, `revokeSubject` collapses the whole family, and only then does `reuse` surface; every `refresh` runs under the per-`sid` token-bucket budget and an exhausted budget is `throttled`, so an offline brute-force of a stolen `sid` is bounded by the store-backed limiter across every app sharing the library.
- Law: rotation is mandatory per `refresh` — a fresh secret, a bumped `generation`, a replaced session; ids mint through `Crypto.uuid` over the folder entropy port, never the ambient global, so a seeded reader makes the whole lifecycle deterministic under test. The access and refresh TTLs are `Config` `Duration` policy values.
- Law: a `Jwt` mint fault re-spells to `store` at this seam — the caller sees one session fault family; `Jwt` rides the requirement channel — `Jwt.Default` is a Layer factory over a `Keyset`, so the composition root satisfies it with the `Reloadable`-wrapped ring layer, never a static dependency row here.
- Receipt: `TokenPair` — access and refresh both `Redacted`, the `Session` embedded so the caller frames it or audits it without a second read.
- Boundary: `authn/*` resolves a `CredentialRef` and calls `establish`; `Cookie` frames the pair; the ports carry state; the `RateLimiter` store is a data-wave-satisfied Layer; the runtime serve wave mounts `BearerGuard`.
- Packages: `crypt/sign` (`Jwt.mint`/`verify`, `Crypto.token`/`.uuid`/`.fingerprint`/`.matches`); `@effect/experimental` (`RateLimiter`); `@effect/platform` (`HttpApiMiddleware`, `HttpApiSecurity`); `effect` (`Config`, `DateTime`, `Duration`, `Effect`, `Metric`, `Option`, `Redacted`, `Schema`).

```typescript
type RotationStep = Data.TaggedEnum<{
  Rotate: { readonly session: Session }
  Expired: { readonly session: Session }
  Reused: { readonly session: Session }
}>

const _RotationStep = Data.taggedEnum<RotationStep>()

const _RefreshWire = Schema.TemplateLiteralParser(_SessionId, ".", Schema.String)

const _ALPHABET = "abcdefghijklmnopqrstuvwxyz0123456789"

const _reuse = Metric.counter("security_token_reuse")

const _step = (session: Session, now: DateTime.Utc, matched: boolean): RotationStep =>
  DateTime.greaterThan(now, session.expiresAt) ? _RotationStep.Expired({ session })
    : matched ? _RotationStep.Rotate({ session })
    : _RotationStep.Reused({ session })

class CurrentClaims extends Context.Tag("security/authn/CurrentClaims")<CurrentClaims, AccessClaims>() {}

class BearerGuard extends HttpApiMiddleware.Tag<BearerGuard>()("security/authn/BearerGuard", {
  provides: CurrentClaims,
  failure: SessionFault,
  security: { bearer: HttpApiSecurity.bearer },
}) {
  static readonly Live: Layer.Layer<BearerGuard, never, Jwt> = Layer.effect(
    BearerGuard,
    Effect.map(Jwt, (jwt) => ({
      bearer: (token: Redacted.Redacted<string>) =>
        jwt.verify(token).pipe(Effect.mapError((fault) => new SessionFault({ reason: "mismatch", detail: fault.detail }))),
    })),
  )
}

class Token extends Effect.Service<Token>()("security/authn/Token", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const cipher = yield* Crypto
    const store = yield* SessionStore
    const journal = yield* IdentityJournal
    const limit = yield* RateLimiter.makeWithRateLimiter
    const accessTtl = yield* Config.duration("SESSION_ACCESS_TTL").pipe(Config.withDefault(Duration.minutes(15)))
    const refreshTtl = yield* Config.duration("SESSION_REFRESH_TTL").pipe(Config.withDefault(Duration.days(30)))
    const refreshWindow = yield* Config.duration("SESSION_REFRESH_WINDOW").pipe(Config.withDefault(Duration.minutes(1)))
    const refreshBudget = yield* Config.integer("SESSION_REFRESH_LIMIT").pipe(Config.withDefault(10))
    const _spell = (fault: { readonly detail: string }): SessionFault => new SessionFault({ reason: "store", detail: fault.detail })
    const _secret = cipher.token(_ALPHABET, 48).pipe(Effect.mapError(_spell))
    const _id = <B extends string>(brand: Schema.Schema<B, string>): Effect.Effect<B, SessionFault> =>
      cipher.uuid().pipe(Effect.mapError(_spell), Effect.flatMap((raw) => Schema.decode(brand)(raw).pipe(Effect.orDie)))
    const _access = (subject: Subject["id"], id: Session["id"], scope: ReadonlyArray<string>, tenant: Subject["tenant"]): Effect.Effect<Redacted.Redacted<string>, SessionFault> =>
      jwt.mint(new AccessClaims({ sub: subject, sid: id, scope, tid: tenant, cnf: Option.none() }), accessTtl).pipe(Effect.mapError(_spell))
    const _mint = (subject: Subject, scope: ReadonlyArray<string>): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const id = yield* _id(_SessionId)
        const secret = yield* _secret
        const session = new Session({
          id, subject: subject.id, tenant: subject.tenant, scope, issuedAt: now,
          expiresAt: DateTime.addDuration(now, refreshTtl), refreshHash: cipher.fingerprint(secret), generation: 0,
        })
        yield* store.create(session)
        const access = yield* _access(subject.id, id, scope, subject.tenant)
        return new TokenPair({ access, refresh: Redacted.make(`${id}.${Redacted.value(secret)}`), session })
      })
    const establish = (ref: CredentialRef, scope: ReadonlyArray<string>, seed: { readonly tenant: Subject["tenant"]; readonly verified: boolean }): Effect.Effect<TokenPair, SessionFault> =>
      Effect.flatMap(journal.resolve(ref), Option.match({
        onSome: (subject) => _mint(subject, scope),
        onNone: () =>
          Effect.gen(function* () {
            const id = yield* _id(_SubjectId)
            const subject = new Subject({ id, tenant: seed.tenant, verified: seed.verified })
            yield* journal.enroll(subject, ref)
            return yield* _mint(subject, scope)
          }),
      })).pipe(Effect.withSpan("security.token.establish"))
    const _rotate = (session: Session, now: DateTime.Utc): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const next = yield* _secret
        const rotated = new Session({
          ...session, refreshHash: cipher.fingerprint(next), generation: session.generation + 1,
          issuedAt: now, expiresAt: DateTime.addDuration(now, refreshTtl),
        })
        yield* store.replace(rotated)
        const access = yield* _access(session.subject, session.id, session.scope, session.tenant)
        return new TokenPair({ access, refresh: Redacted.make(`${session.id}.${Redacted.value(next)}`), session: rotated })
      })
    const refresh = (presented: Redacted.Redacted<string>): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const [sid, , secret] = yield* Schema.decode(_RefreshWire)(Redacted.value(presented)).pipe(
          Effect.mapError(() => new SessionFault({ reason: "mismatch", detail: "malformed refresh frame" })))
        return yield* limit({ algorithm: "token-bucket", onExceeded: "fail", window: refreshWindow, limit: refreshBudget, key: `refresh:${sid}` })(
          Effect.gen(function* () {
            const session = yield* Effect.flatMap(store.read(sid), Option.match({
              onNone: () => Effect.fail(new SessionFault({ reason: "notFound", detail: sid })),
              onSome: Effect.succeed,
            }))
            const now = yield* DateTime.now
            const matched = yield* cipher.matches(Probe.Digest({ opaque: Redacted.make(secret), stored: session.refreshHash })).pipe(Effect.mapError(_spell))
            return yield* _RotationStep.$match(_step(session, now, matched), {
              Expired: ({ session: held }) => Effect.fail(new SessionFault({ reason: "expired", detail: held.id })),
              Reused: ({ session: held }) =>
                Metric.increment(_reuse).pipe(
                  Effect.zipRight(Effect.logError("refresh reuse detected")),
                  Effect.annotateLogs("subject", held.subject),
                  Effect.zipRight(store.revokeSubject(held.subject)),
                  Effect.zipRight(Effect.fail(new SessionFault({ reason: "reuse", detail: held.subject }))),
                ),
              Rotate: ({ session: held }) => _rotate(held, now),
            })
          }),
        ).pipe(Effect.catchTags({
          RateLimitExceeded: () => Effect.fail(new SessionFault({ reason: "throttled", detail: sid })),
          RateLimitStoreError: (error) => Effect.fail(new SessionFault({ reason: "throttled", detail: String(error) })),
        }))
      }).pipe(Effect.withSpan("security.token.refresh"))
    const revoke = (id: Session["id"]): Effect.Effect<void, SessionFault> => store.revoke(id)
    return { establish, refresh, revoke } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [04]-[COOKIE_EGRESS]

[COOKIE_EGRESS]:
- Owner: `Cookie` — the session's egress projection over the `@effect/platform` `Cookies` codec: `frame` maps a `TokenPair` onto the access and refresh specs with `maxAge` derived from the session window, `clear` expires every role on logout, `csrf` mints the readable CSRF cookie, `verify` compares it constant-time to the presented header. `CookieSpec` is the attribute policy table — one row per role carrying `{ name, options }` — and the platform `Cookies.Cookie` is the framed value, so serialization, attribute rendering, and the `__Host-`/`__Secure-` prefix semantics are the platform's, never a hand-rolled string; `CsrfFault` is the folder fault shape at 403 exposure.
- Law: every cookie attribute is a policy row, not a call-site literal — `httpOnly`, `secure`, `sameSite`, `path` live in the table, so a new role or a `sameSite` change reframes every write with zero handler edit; the refresh cookie is path-scoped to the refresh route so it never rides an ordinary request; a static policy row that refuses `Cookies.makeCookie` is a defect, so the lift is `Effect.orDie`.
- Law: the token unwraps exactly at the framing seam — the returned `Cookies.Cookie` values are the egress set the edge writes immediately (a `HttpServerResponse.setCookie` fold or `Cookies.fromIterable` → `Cookies.toSetCookieHeaders`), and `Headers.redact` masks the `cookie`/`set-cookie` pair on every logged header bag, so no token reaches a log.
- Law: CSRF is constant-time double-submit — `csrf` mints a high-entropy token through `Crypto.token`, `verify` routes the `Text` probe, a mismatch or an absent pair is `CsrfFault`, and every rejection increments `security_csrf_mismatch`; a timing oracle and a stripped header both fail closed.
- Receipt: `ReadonlyArray<Cookies.Cookie>` on frame/clear, `Cookies.Cookie` on csrf; `verify` returns `void` or `CsrfFault`, never a boolean the caller can forget.
- Growth: a new cookie role is one `CookieSpec` row; a new CSRF posture is one `verify` policy value.
- Boundary: `Token` owns the token values; `crypt/sign` mints and compares the CSRF token; the edge applies `Cookies.toSetCookieHeaders` and lifts the request cookie/header.
- Packages: `@effect/platform` (`Cookies.makeCookie`, `Headers.redact`); `crypt/sign` (`Crypto.token`/`.matches`); `effect` (`DateTime`, `Duration`, `Effect`, `Metric`, `Option`, `Redacted`, `Schema`).

```typescript
const CookieSpec = {
  access: { name: "__Host-access", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/" } },
  refresh: { name: "__Secure-refresh", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/auth/refresh" } },
  csrf: { name: "__Host-csrf", options: { httpOnly: false, secure: true, sameSite: "strict", path: "/" } },
} as const

const _CSRF_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

const _EMPTY_VALUE = Redacted.make("")

const _csrfMiss = Metric.counter("security_csrf_mismatch")

const _csrfReasons = ["absent", "mismatch"] as const
const _csrfFaults = { absent: { class: "denied" }, mismatch: { class: "denied" } } as const

declare namespace CookieSpec {
  type Role = keyof typeof CookieSpec
  type _Rows<T extends Record<string, { readonly name: string; readonly options: Cookies.Cookie["options"] }> = typeof CookieSpec> = T
}

declare namespace CsrfFault {
  type Reason = (typeof _csrfReasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _csrfFaults> = T
  type _Closed<K extends Reason = keyof typeof _csrfFaults> = K
}

class CsrfFault extends Schema.TaggedError<CsrfFault>()("CsrfFault", {
  reason: Schema.Literal(..._csrfReasons),
}) {
  get class(): FaultClass.Kind {
    return _csrfFaults[this.reason].class
  }
  override get message(): string {
    return `<csrf:${this.reason}>`
  }
}

class Cookie extends Effect.Service<Cookie>()("security/authn/Cookie", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const _framed = (role: CookieSpec.Role, value: Redacted.Redacted<string>, maxAge?: Duration.DurationInput): Effect.Effect<Cookies.Cookie> =>
      Effect.orDie(Cookies.makeCookie(
        CookieSpec[role].name,
        Redacted.value(value),
        maxAge === undefined ? CookieSpec[role].options : { ...CookieSpec[role].options, maxAge },
      ))
    const frame = (pair: TokenPair): Effect.Effect<ReadonlyArray<Cookies.Cookie>> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const remaining = Duration.max(Duration.zero, DateTime.distanceDuration(now, pair.session.expiresAt))
        const access = yield* _framed("access", pair.access)
        const refresh = yield* _framed("refresh", pair.refresh, remaining)
        return [access, refresh]
      })
    const clear = (): Effect.Effect<ReadonlyArray<Cookies.Cookie>> =>
      Effect.all([_framed("access", _EMPTY_VALUE, 0), _framed("refresh", _EMPTY_VALUE, 0), _framed("csrf", _EMPTY_VALUE, 0)])
    const csrf = (): Effect.Effect<Cookies.Cookie> =>
      cipher.token(_CSRF_ALPHABET, 32).pipe(Effect.orDie, Effect.flatMap((token) => _framed("csrf", token)))
    const verify = (cookieToken: Option.Option<string>, headerToken: Option.Option<string>): Effect.Effect<void, CsrfFault> =>
      Option.match(Option.zipWith(cookieToken, headerToken, (held, presented) => ({ held, presented })), {
        onNone: () => Effect.fail(new CsrfFault({ reason: "absent" })),
        onSome: ({ held, presented }) =>
          cipher.matches(Probe.Text({ held: Redacted.make(held), presented })).pipe(
            Effect.orDie,
            Effect.filterOrFail((matched) => matched, () => new CsrfFault({ reason: "mismatch" })),
            Effect.asVoid,
          ),
      }).pipe(Effect.tapError(() => Metric.increment(_csrfMiss)))
    return { frame, clear, csrf, verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { BearerGuard, Cookie, CookieSpec, CredentialRef, CsrfFault, CurrentClaims, IdentityJournal, Session, SessionFault, SessionStore, Subject, Token, TokenPair }
export type { RotationStep }
```
