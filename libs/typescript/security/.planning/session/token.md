# [SECURITY_TOKEN] — the session/refresh vocabulary, rotation-with-reuse-detection, and the identity ports

`session/token` is the identity plane's vocabulary owner: the `Subject` identity and `Session` live-session shapes, the `CredentialRef` every authn ceremony resolves through, the `TokenPair` mint receipt, and the two `Context.Tag` ports — `SessionStore` and `IdentityJournal` — that carry identity state out of the folder so a zero-durable-state browser app composes `security` without pulling the SQL folder; the app root satisfies both with `store` journal `Layer`s. The `Token` service mints an access JWT through `sign/jwt` and an opaque high-entropy refresh through `sign/crypto`, storing only the refresh's SHA-256 fingerprint (argon2 is for low-entropy credentials; a random refresh needs a fast constant-time compare, never a per-check KDF). Rotation is the security-critical fold: every `refresh` mints a fresh secret, bumps a generation counter, and replaces the session, and a presented refresh that targets a live session but fails the current fingerprint is a replayed rotated token — `SessionFault.reuse` — which revokes every session of the subject family. `authn/*` resolves a `CredentialRef` and calls `Token.establish`; `session/cookie` frames the pair; `authz` keys claims by `Subject["id"]`.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                          | [OWNER]                                  | [PACKAGES]                       | [REJECTED_FORM]                              |
| :-----: | :--------------------------------- | :--------------------------------------- | :------------------------------- | :------------------------------------------- |
|  [01]   | identity + session shapes          | `Subject` / `Session` / `TokenPair`      | `effect` `Schema`                | a DTO beside the model, a raw `sub` string   |
|  [02]   | identity state boundary            | `SessionStore` / `IdentityJournal` ports | `effect` `Context.Tag`           | a `store` import, a module-level session map |
|  [03]   | mint, rotate, reuse-detect, revoke | `Token.establish`/`.refresh`/`.revoke`   | `sign/jwt`, `sign/crypto`        | argon2 on a refresh, a rotated token accepted |

## [2]-[IDENTITY_VOCABULARY]

[SHAPES_AND_PORTS]:
- Owner: `Subject` is the durable identity (branded `id`, tenant, verification), `Session` the live session (branded `id`, subject, scope, window, refresh fingerprint, rotation generation), `CredentialRef` the `{ kind, key }` an authn ceremony resolves through. `SessionStore` owns the live-session lifecycle, `IdentityJournal` the credential↔subject resolution and enrollment; both are `Context.Tag` ports the app root satisfies with `store`.
- Packages: `effect` — one `Schema.Class` per shape carries the id brand as a field refinement so `Subject["id"]` is the only `SubjectId` spelling; the ports declare their fault as `SessionFault` on the `E` channel.
- Boundary: `store/journal` satisfies both ports (the `session/token ← store/journal [PORT]` seam); a port exists exactly because the wave ledger forbids `security → store`, so a Tag minted to dodge a legal edge would be the defect.
- Growth: a new credential kind is one `CredentialRef.kind` literal; a new session facet is one `Session` field the store persists.

```typescript
import { Config, Context, DateTime, Duration, Effect, Option, Redacted, Schema } from "effect"
import { AccessClaims, Jwt } from "../sign/jwt.ts"
import { Crypto } from "../sign/crypto.ts"

// --- [TYPES] ----------------------------------------------------------------------------

const _SubjectId = Schema.UUID.pipe(Schema.brand("SubjectId"))
const _SessionId = Schema.UUID.pipe(Schema.brand("SessionId"))
const _TenantId = Schema.NonEmptyString.pipe(Schema.brand("TenantId"))

const _reasons = ["expired", "notFound", "reuse", "mismatch", "store"] as const

const SessionFaultPolicy = {
  expired: { rank: 2, retry: false, status: 401 },
  notFound: { rank: 3, retry: false, status: 401 },
  reuse: { rank: 5, retry: false, status: 401 },
  mismatch: { rank: 3, retry: false, status: 400 },
  store: { rank: 4, retry: true, status: 503 },
} as const

declare namespace SessionFault {
  type Reason = keyof typeof SessionFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof SessionFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class Subject extends Schema.Class<Subject>("Subject")({
  id: _SubjectId,
  tenant: Schema.optionalWith(_TenantId, { as: "Option" }),
  verified: Schema.Boolean,
}) {}

class CredentialRef extends Schema.Class<CredentialRef>("CredentialRef")({
  kind: Schema.Literal("oauth", "webauthn", "otp", "apikey", "password"),
  key: Schema.NonEmptyString,
}) {}

class Session extends Schema.Class<Session>("Session")({
  id: _SessionId,
  subject: _SubjectId,
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

// --- [ERRORS] ---------------------------------------------------------------------------

class SessionFault extends Schema.TaggedError<SessionFault>()("SessionFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): SessionFault.Row {
    return SessionFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<session:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class SessionStore extends Context.Tag("security/session/SessionStore")<SessionStore, {
  readonly create: (session: Session) => Effect.Effect<void, SessionFault>
  readonly read: (id: Session["id"]) => Effect.Effect<Option.Option<Session>, SessionFault>
  readonly replace: (session: Session) => Effect.Effect<void, SessionFault>
  readonly revoke: (id: Session["id"]) => Effect.Effect<void, SessionFault>
  readonly revokeSubject: (subject: Subject["id"]) => Effect.Effect<void, SessionFault>
}>() {}

class IdentityJournal extends Context.Tag("security/session/IdentityJournal")<IdentityJournal, {
  readonly resolve: (ref: CredentialRef) => Effect.Effect<Option.Option<Subject>, SessionFault>
  readonly enroll: (subject: Subject, ref: CredentialRef) => Effect.Effect<void, SessionFault>
  readonly credentials: (subject: Subject["id"]) => Effect.Effect<ReadonlyArray<CredentialRef>, SessionFault>
  readonly revokeCredential: (ref: CredentialRef) => Effect.Effect<void, SessionFault>
}>() {}
```

## [3]-[ROTATION_LAW]

[TOKEN]:
- Owner: `Token` — `establish` resolves-or-enrolls a `CredentialRef` into a `Subject` and mints the first pair, `refresh` rotates with reuse detection, `revoke` ends a session. The access token is a `sign/jwt` `AccessClaims` JWT; the refresh is a `sign/crypto` opaque token whose SHA-256 fingerprint alone is stored, the wire form `${sid}.${secret}` so `refresh` reads the session before touching the secret.
- Packages: `sign/jwt` `Jwt.mint`, `sign/crypto` `Crypto.token`/`.fingerprint`/`.matches`; the `SessionStore`/`IdentityJournal` ports carry state.
- Law: rotation is mandatory per `refresh` — a fresh secret, a bumped `generation`, a replaced session; a live session whose current fingerprint rejects the presented secret is a replayed rotated token, so `reuse` fires and `revokeSubject` collapses the whole family; an expired window is `expired`, a missing/revoked session is `notFound`. The access and refresh TTLs are `Config` `Duration` policy values.
- Receipt: `TokenPair` — the access and refresh both `Redacted`, the `Session` embedded so the caller frames it (cookie) or audits it without a second read.

```typescript
// --- [CONSTANTS] ------------------------------------------------------------------------

const _ALPHABET = "abcdefghijklmnopqrstuvwxyz0123456789"

// --- [SERVICES] -------------------------------------------------------------------------

class Token extends Effect.Service<Token>()("security/session/Token", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const cipher = yield* Crypto
    const store = yield* SessionStore
    const journal = yield* IdentityJournal
    const accessTtl = yield* Config.duration("SESSION_ACCESS_TTL").pipe(Config.withDefault(Duration.minutes(15)))
    const refreshTtl = yield* Config.duration("SESSION_REFRESH_TTL").pipe(Config.withDefault(Duration.days(30)))
    const _mint = (subject: Subject, scope: ReadonlyArray<string>): Effect.Effect<TokenPair, SessionFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const id = yield* Schema.decode(_SessionId)(crypto.randomUUID()).pipe(Effect.orDie)
        const secret = yield* cipher.token(_ALPHABET, 48).pipe(Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.message })))
        const session = new Session({ id, subject: subject.id, scope, issuedAt: now, expiresAt: DateTime.addDuration(now, refreshTtl), refreshHash: cipher.fingerprint(secret), generation: 0 })
        yield* store.create(session)
        const claims = new AccessClaims({ sub: subject.id, sid: id, scope, tid: subject.tenant })
        const access = yield* jwt.mint(claims, accessTtl).pipe(Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.message })))
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
        if (DateTime.greaterThan(now, session.expiresAt)) return yield* Effect.fail(new SessionFault({ reason: "expired", detail: sid ?? "" }))
        if (!cipher.matches(Redacted.make(secret ?? ""), session.refreshHash)) {
          yield* store.revokeSubject(session.subject)
          return yield* Effect.fail(new SessionFault({ reason: "reuse", detail: session.subject }))
        }
        const next = yield* cipher.token(_ALPHABET, 48).pipe(Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.message })))
        const rotated = new Session({ ...session, refreshHash: cipher.fingerprint(next), generation: session.generation + 1, issuedAt: now, expiresAt: DateTime.addDuration(now, refreshTtl) })
        yield* store.replace(rotated)
        const claims = new AccessClaims({ sub: session.subject, sid: session.id, scope: session.scope, tid: Option.none() })
        const access = yield* jwt.mint(claims, accessTtl).pipe(Effect.mapError((cause) => new SessionFault({ reason: "store", detail: cause.message })))
        return new TokenPair({ access, refresh: Redacted.make(`${session.id}.${Redacted.value(next)}`), session: rotated })
      })
    const revoke = (id: Session["id"]): Effect.Effect<void, SessionFault> => store.revoke(id)
    return { establish, refresh, revoke } as const
  }),
  dependencies: [Jwt.Default, Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { CredentialRef, IdentityJournal, Session, SessionFault, SessionStore, Subject, Token, TokenPair }
```
