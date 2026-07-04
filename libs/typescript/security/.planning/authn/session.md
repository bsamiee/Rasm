# [SECURITY_SESSION]

The identity plane's session owner: the `Subject`/`Session`/`CredentialRef`/`TokenPair` vocabulary, rotation with reuse-detection, the `SessionStore`/`IdentityJournal` ports, and the cookie framing plus CSRF double-submit law as the session's egress projection — one owner because token minting and cookie framing are two faces of one session lifecycle. `Token` mints an access JWT through `crypt/sign`'s `Jwt` and an opaque high-entropy refresh through `Crypto`, storing only the refresh's SHA-256 fingerprint — argon2 is for low-entropy credentials; a random refresh needs a fast constant-time compare, never a per-check KDF. Rotation is the security-critical fold: every `refresh` mints a fresh secret, bumps a generation counter, and replaces the session, and a presented refresh that targets a live session but fails the current fingerprint is a replayed rotated token — `SessionFault.reuse` — which revokes every session of the subject family. `Cookie` frames the `TokenPair` into `FramedCookie` values the edge writes — the access token as a `__Host-` session cookie, the refresh path-scoped to the refresh route, the CSRF token a readable `__Host-` cookie the client echoes into a header. The two `Context.Tag` ports carry identity state out of the folder so a zero-durable-state browser app composes `security` without the data wave; the app root satisfies both with the data journal. Tenancy is the core `TenantContext.Key`; `SessionFault`/`CsrfFault` instantiate the folder fault shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                              | [PUBLIC]                                            |
| :-----: | :------------------ | :------------------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `IDENTITY_VOCAB`    | the identity/session shapes, the fault, the two identity ports      | `Subject`, `Session`, `CredentialRef`, `TokenPair`, `SessionFault`, `SessionStore`, `IdentityJournal` |
|  [02]   | `ROTATION_LAW`      | mint, rotate, reuse-detect, revoke over the ports                   | `Token`                                             |
|  [03]   | `COOKIE_EGRESS`     | the cookie-attribute table, frame/clear, CSRF double-submit         | `Cookie`, `CookieSpec`, `CsrfFault`, `FramedCookie` |

## [2]-[IDENTITY_VOCAB]

[IDENTITY_VOCAB]:
- Owner: `Subject` is the durable identity (branded `id`, tenant key, verification), `Session` the live session (branded `id`, subject, tenant, scope, window, refresh fingerprint, rotation generation — the tenant rides the session so a rotated access token keeps its tenancy claim), `CredentialRef` the `{ kind, key }` an authn ceremony resolves through, `TokenPair` the mint receipt; `SessionFault` is the folder fault shape; `SessionStore`/`IdentityJournal` are the identity-state ports.
- Law: each id brand is a field refinement so `Subject["id"]` is the only `SubjectId` spelling; the tenant is the core `TenantContext.Key` brand, never a re-declaration; the ports declare their fault on the `E` channel.
- Law: a port exists exactly because the wave ledger forbids `security → data` — the data journal satisfies both at the app root (the `security/authn/session → data` [PORT] seam), so a Tag minted to dodge a legal edge is the defect.
- Growth: a new credential kind is one `CredentialRef.kind` literal; a new session facet is one `Session` field the store persists.
- Boundary: the authn ceremonies resolve a `CredentialRef` and call `Token.establish`; `access/claim` keys claims by `Subject["id"]`; `crypt/sign` supplies the JWT and crypto primitives.
- Packages: `effect` (`Schema`, `Context`); `@rasm/ts/core` (`TenantContext`, `FaultClass`).

```typescript
import { FaultClass, TenantContext } from "@rasm/ts/core"
import { Config, Context, DateTime, Duration, Effect, Option, Redacted, Schema } from "effect"
import { AccessClaims, Crypto, Jwt } from "../crypt/sign.ts"

const _SubjectId = Schema.UUID.pipe(Schema.brand("SubjectId"))
const _SessionId = Schema.UUID.pipe(Schema.brand("SessionId"))

const _reasons = ["expired", "notFound", "reuse", "mismatch", "store"] as const

const _faults = {
  expired: { class: "expired" },
  notFound: { class: "absent" },
  reuse: { class: "breached" },
  mismatch: { class: "malformed" },
  store: { class: "unavailable" },
} as const

declare namespace SessionFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
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

## [3]-[ROTATION_LAW]

[ROTATION_LAW]:
- Owner: `Token` — `establish` resolves-or-enrolls a `CredentialRef` into a `Subject` and mints the first pair, `refresh` rotates with reuse detection, `revoke` ends a session. The access token is a `crypt/sign` `AccessClaims` JWT; the refresh is a `Crypto` opaque token whose SHA-256 fingerprint alone is stored, the wire form `${sid}.${secret}` so `refresh` reads the session before touching the secret.
- Law: rotation is mandatory per `refresh` — a fresh secret, a bumped `generation`, a replaced session; a live session whose current fingerprint rejects the presented secret is a replayed rotated token, so `reuse` fires and `revokeSubject` collapses the whole family; an expired window is `expired`, a missing/revoked session is `notFound`. The access and refresh TTLs are `Config` `Duration` policy values.
- Law: a `Jwt` mint fault re-spells to `store` at this seam — the caller sees one session fault family, and a JWT failure is an infrastructure fault, not a session-logic branch; `Jwt` rides the requirement channel — `Jwt.Default` is a Layer factory over a `Keyset`, so the composition root satisfies it with the `Reloadable`-wrapped ring layer, never a static dependency row here.
- Receipt: `TokenPair` — access and refresh both `Redacted`, the `Session` embedded so the caller frames it or audits it without a second read.
- Growth: a new session policy (idle timeout, device binding) is one `Session` field plus one fold arm.
- Boundary: `authn/*` resolves a `CredentialRef` and calls `establish`; `Cookie` frames the pair; the ports carry state.
- Packages: `crypt/sign` (`Jwt.mint`, `Crypto.token`/`.fingerprint`/`.matches`); `effect` (`Config`, `DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
const _ALPHABET = "abcdefghijklmnopqrstuvwxyz0123456789"

class Token extends Effect.Service<Token>()("security/authn/Token", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const cipher = yield* Crypto
    const store = yield* SessionStore
    const journal = yield* IdentityJournal
    const accessTtl = yield* Config.duration("SESSION_ACCESS_TTL").pipe(Config.withDefault(Duration.minutes(15)))
    const refreshTtl = yield* Config.duration("SESSION_REFRESH_TTL").pipe(Config.withDefault(Duration.days(30)))
    const _secret = cipher.token(_ALPHABET, 48).pipe(Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.detail })))
    const _access = (subject: Subject["id"], id: Session["id"], scope: ReadonlyArray<string>, tenant: Subject["tenant"]): Effect.Effect<Redacted.Redacted<string>, SessionFault> =>
      jwt.mint(new AccessClaims({ sub: subject, sid: id, scope, tid: tenant }), accessTtl).pipe(
        Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.detail })))
    const _mint = (subject: Subject, scope: ReadonlyArray<string>): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const id = yield* Schema.decode(_SessionId)(crypto.randomUUID()).pipe(Effect.orDie)
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
            const id = yield* Schema.decode(_SubjectId)(crypto.randomUUID()).pipe(Effect.orDie)
            const subject = new Subject({ id, tenant: seed.tenant, verified: seed.verified })
            yield* journal.enroll(subject, ref)
            return yield* _mint(subject, scope)
          }),
      }))
    const refresh = (presented: Redacted.Redacted<string>): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const [sid, secret] = Redacted.value(presented).split(".")
        const id = yield* Schema.decode(_SessionId)(sid ?? "").pipe(Effect.mapError(() => new SessionFault({ reason: "mismatch", detail: "malformed refresh" })))
        const session = yield* Effect.flatMap(store.read(id), Option.match({ onNone: () => Effect.fail(new SessionFault({ reason: "notFound", detail: sid ?? "" })), onSome: Effect.succeed }))
        const now = yield* DateTime.now
        yield* DateTime.greaterThan(now, session.expiresAt) ? Effect.fail(new SessionFault({ reason: "expired", detail: sid ?? "" })) : Effect.void
        yield* cipher.matches(Redacted.make(secret ?? ""), session.refreshHash)
          ? Effect.void
          : Effect.zipRight(store.revokeSubject(session.subject), Effect.fail(new SessionFault({ reason: "reuse", detail: session.subject })))
        const next = yield* _secret
        const rotated = new Session({ ...session, refreshHash: cipher.fingerprint(next), generation: session.generation + 1, issuedAt: now, expiresAt: DateTime.addDuration(now, refreshTtl) })
        yield* store.replace(rotated)
        const access = yield* _access(session.subject, session.id, session.scope, session.tenant)
        return new TokenPair({ access, refresh: Redacted.make(`${session.id}.${Redacted.value(next)}`), session: rotated })
      })
    const revoke = (id: Session["id"]): Effect.Effect<void, SessionFault> => store.revoke(id)
    return { establish, refresh, revoke } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}
```

## [4]-[COOKIE_EGRESS]

[COOKIE_EGRESS]:
- Owner: `Cookie` — the session's egress projection: `frame` maps a `TokenPair` onto the access and refresh specs with `maxAge` derived from the session window, `clear` expires every role on logout, `csrf` mints the readable CSRF cookie, `verify` compares it constant-time to the presented header. `CookieSpec` is the attribute policy table — one row per role carrying `{ name, options }` — and `FramedCookie` is the value the edge writes; `CsrfFault` is the folder fault shape at 403 exposure.
- Law: every cookie attribute is a policy row, not a call-site literal — `httpOnly`, `secure`, `sameSite`, `path` live in the table, so a new role or a `sameSite` change reframes every write with zero handler edit; the refresh cookie is path-scoped to the refresh route so it never rides an ordinary request.
- Law: CSRF is constant-time double-submit — `csrf` mints a high-entropy token through `Crypto.token`, `verify` routes `Crypto.same`, and a mismatch or an absent pair is `CsrfFault`; a timing oracle and a stripped header both fail closed.
- Law: this cluster holds no HTTP transport — it produces `FramedCookie` values and reads lifted `Option` header/cookie values, so the edge owns the response write and the request read and `security` stays runtime-portable.
- Receipt: `FramedCookie[]` — every value `Redacted`, so the framed set never leaks a token; `verify` returns `void` or `CsrfFault`, never a boolean the caller can forget.
- Growth: a new cookie role is one `CookieSpec` row; a new CSRF posture is one `verify` policy value.
- Boundary: `Token` owns the token values; `crypt/sign` mints and compares the CSRF token; the edge applies each `FramedCookie` and lifts the request cookie/header.
- Packages: `crypt/sign` (`Crypto.token`/`.same`); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
type CookieOptions = {
  readonly httpOnly: boolean
  readonly secure: boolean
  readonly sameSite: "strict" | "lax" | "none"
  readonly path: string
  readonly maxAge?: number
}

type FramedCookie = { readonly name: string; readonly value: Redacted.Redacted<string>; readonly options: CookieOptions }

const CookieSpec = {
  access: { name: "__Host-access", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/" } },
  refresh: { name: "__Secure-refresh", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/auth/refresh" } },
  csrf: { name: "__Host-csrf", options: { httpOnly: false, secure: true, sameSite: "strict", path: "/" } },
} as const satisfies Record<string, { readonly name: string; readonly options: CookieOptions }>

const _CSRF_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

const _csrfReasons = ["absent", "mismatch"] as const
const _csrfFaults = { absent: { class: "denied" }, mismatch: { class: "denied" } } as const

declare namespace CookieSpec {
  type Role = keyof typeof CookieSpec
}

declare namespace CsrfFault {
  type Reason = keyof typeof _csrfFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _csrfFaults> = T
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
    const _framed = (role: CookieSpec.Role, value: Redacted.Redacted<string>, maxAge?: number): FramedCookie =>
      ({ name: CookieSpec[role].name, value, options: maxAge === undefined ? CookieSpec[role].options : { ...CookieSpec[role].options, maxAge } })
    const frame = (pair: TokenPair): Effect.Effect<ReadonlyArray<FramedCookie>> =>
      Effect.map(DateTime.now, (now) => {
        const remaining = Math.max(0, Math.floor(Duration.toSeconds(DateTime.distanceDuration(now, pair.session.expiresAt))))
        return [_framed("access", pair.access), _framed("refresh", pair.refresh, remaining)]
      })
    const clear = (): ReadonlyArray<FramedCookie> =>
      [_framed("access", Redacted.make(""), 0), _framed("refresh", Redacted.make(""), 0), _framed("csrf", Redacted.make(""), 0)]
    const csrf = (): Effect.Effect<FramedCookie> =>
      cipher.token(_CSRF_ALPHABET, 32).pipe(Effect.orDie, Effect.map((token) => _framed("csrf", token)))
    const verify = (cookieToken: Option.Option<string>, headerToken: Option.Option<string>): Effect.Effect<void, CsrfFault> =>
      Option.match(Option.zipWith(cookieToken, headerToken, (held, presented) => cipher.same(Redacted.make(held), presented)), {
        onNone: () => Effect.fail(new CsrfFault({ reason: "absent" })),
        onSome: (matched) => (matched ? Effect.void : Effect.fail(new CsrfFault({ reason: "mismatch" }))),
      })
    return { frame, clear, csrf, verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cookie, CookieSpec, CredentialRef, CsrfFault, IdentityJournal, Session, SessionFault, SessionStore, Subject, Token, TokenPair }
export type { CookieOptions, FramedCookie }
```
