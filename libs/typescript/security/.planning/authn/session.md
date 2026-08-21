# [SECURITY_SESSION]

Identity plane's session owner: the `Subject`/`Session`/`CredentialRef`/`TokenPair` vocabulary, rotation as a data-carried statechart with reuse detection, the `SessionStore`/`IdentityJournal` ports, the `BearerGuard` middleware seam, and the cookie framing and CSRF double-submit law as the session's egress projection — one owner because token minting and cookie framing are two faces of one session lifecycle. `Token` mints an access JWT through `crypt/sign`'s `Jwt` and an opaque high-entropy refresh through `Crypto`, storing only the refresh's SHA-256 fingerprint — argon2 is for low-entropy credentials; a random refresh needs a fast constant-time compare, never a per-check KDF. Rotation is the security-critical fold and it is a transition family, not an imperative flow: `_step` folds a live session, the current instant, and the constant-time match verdict into the `RotationStep` case — `Rotate`, `Expired`, `Reused` — and `$match` dispatches each arm, so the guard set is data and a new lifecycle rule is a case. `Reused` is the breach arm: it lands the `reuse` row on the folder reject stream, logs at error, and revokes every session of the subject family before the fault surfaces — a replayed rotated token never fails silently, and a refused presented bearer lands the `bearer` row at the guard seam, so the folder's highest-traffic rejection path is counted like every sibling surface. Every `refresh` runs under the `Curb` `refresh` row keyed by `sid`, session and subject ids mint through the `Crypto` entropy port so tests inject determinism, and the `${sid}.${secret}` wire frame decodes through one `Schema.TemplateLiteralParser` owner, never a hand split. `BearerGuard` and `SessionGuard` are the two declarative faces of one verified-claims provision — the bearer header for API callers, the access cookie with the CSRF double-submit gate for browser apps — so a cookie-authenticated app rides the same middleware seam and the same `bearer` ledger row as every token caller. `Cookie` frames the `TokenPair` through the `@effect/platform` `Cookies` codec — the attribute policy stays in the `CookieSpec` table, serialization and the `__Host-`/`__Secure-` prefix semantics belong to the platform, and the edge folds the returned collection through `Cookies.toSetCookieHeaders` and owns the log path, where `Headers.redact` masks the `cookie`/`set-cookie` pair on the bags it logs — this page frames `Cookies.Cookie` values and composes no header bag of its own. Two `Context.Tag` ports carry identity state out of the folder so a zero-durable-state browser app composes `security` without the data wave; the app root satisfies both with the data journal. Tenancy is the core `Identity.Tenant.Key`; `SessionFault`/`CsrfFault` instantiate the folder fault shape over the core `Fault.Class.family` seam.

## [01]-[INDEX]

- [02]-[IDENTITY_VOCAB]: `Subject`, `Session`, `CredentialRef`, `TokenPair`, `SessionFault`, `SessionStore`, `IdentityJournal`.
- [03]-[ROTATION_LAW]: `Token`, `RotationStep`, `BearerGuard`, `CurrentClaims`.
- [04]-[COOKIE_EGRESS]: `Cookie`, `CookieSpec`, `CsrfFault`, `SessionGuard`.

## [02]-[IDENTITY_VOCAB]

[IDENTITY_VOCAB]:
- Owner: `Subject` is the durable identity (branded `id`, tenant key, verification), `Session` the live session (branded `id`, subject, tenant, scope, window, refresh fingerprint, rotation generation — the tenant rides the session so a rotated access token keeps its tenancy claim), `CredentialRef` the `{ kind, key }` an authn ceremony resolves through, `TokenPair` the mint receipt; `SessionFault` is the folder fault shape; `SessionStore`/`IdentityJournal` are the identity-state ports.
- Law: a port exists exactly because the wave ledger forbids `security → data` — the data journal satisfies both at the app root (the `security/authn/session → data` [PORT] seam), so a Tag minted to dodge a legal edge is the defect.
- Growth: a new credential kind is one `CredentialRef.kind` literal; a new session facet is one `Session` field the store persists.
- Boundary: the authn ceremonies resolve a `CredentialRef` and call `Token.establish`; `access/claim` keys claims by `Subject["id"]`; `crypt/sign` supplies the JWT and crypto primitives.
- Packages: `effect` (`Schema`, `Context`); `@rasm/ts/core` (`Identity.Tenant`, `Fault.Class`).

```typescript
import { Cookies, Headers, HttpApiMiddleware, HttpApiSecurity, HttpServerRequest } from "@effect/platform"
import { Fault, Identity } from "@rasm/ts/core"
import { Array, Config, Context, Data, DateTime, Duration, Effect, Layer, Option, Redacted, Schema, Struct } from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"
import { AccessClaims, Alphabet, Crypto, Jwt, Probe, type SignFault } from "../crypt/sign.ts"
import { Curb, Reject } from "../crypt/verify.ts"

const _SubjectId = Schema.UUID.pipe(Schema.brand("SubjectId"))
const _SessionId = Schema.UUID.pipe(Schema.brand("SessionId"))

// Six reasons share one subject because one respell mints them all: a `crypt/sign` fault carries a cause and the
// COORDINATE the presentation was about — the session id where the frame already resolved to one, the ceremony
// itself where it never did. `reuse` alone carries its own subject, because a replayed rotation is this page's own
// statechart verdict over a stored generation and its evidence is the session, the subject, and the generation the
// replay tried to spend — facts no primitive fault can supply and no shared cause string can hold.
const _family = Fault.Class.family(["expired", "notFound", "reuse", "mismatch", "denied", "throttled", "store"] as const, {
  expired: Fault.Class.row({
    class: "expired",
    leg: "rotation",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `session ${coordinate} is past its window: ${cause}`,
  }),
  notFound: Fault.Class.row({
    class: "absent",
    leg: "rotation",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `no live session at ${coordinate}: ${cause}`,
  }),
  reuse: Fault.Class.row({
    class: "breached",
    leg: "rotation",
    detail: Schema.Struct({ sid: Schema.String, subject: Schema.String, generation: Schema.Int }),
    render: ({ generation, sid }) => `refresh replay on session ${sid} against generation ${generation}`,
  }),
  mismatch: Fault.Class.row({
    class: "malformed",
    leg: "presentation",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `presentation at ${coordinate} is unreadable: ${cause}`,
  }),
  denied: Fault.Class.row({
    class: "denied",
    leg: "presentation",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `presentation at ${coordinate} was refused: ${cause}`,
  }),
  throttled: Fault.Class.row({
    class: "exhausted",
    leg: "throttle",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `session budget spent on ${coordinate}: ${cause}`,
  }),
  store: Fault.Class.row({
    class: "unavailable",
    leg: "store",
    detail: Schema.Struct({ coordinate: Schema.String, cause: Schema.String }),
    render: ({ cause, coordinate }) => `session store unreachable for ${coordinate}: ${cause}`,
  }),
})

declare namespace SessionFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
  // A primitive fault never reports a REPLAY: reuse is this page's own statechart verdict over a stored generation,
  // so the respell table's value type forecloses it and no future core class can land a breach word on an outage.
  type Respelled = Exclude<Reason, "reuse">
}

class Subject extends Schema.Class<Subject>("Subject")({
  id: _SubjectId,
  tenant: Schema.optionalWith(Identity.Tenant.fields.tenant, { as: "Option" }),
  verified: Schema.Boolean,
}) {}

class CredentialRef extends Schema.Class<CredentialRef>("CredentialRef")({
  kind: Schema.Literal("oauth", "webauthn", "otp", "apikey", "password"),
  key: Schema.NonEmptyString,
}) {}

class Session extends Schema.Class<Session>("Session")({
  id: _SessionId,
  subject: _SubjectId,
  tenant: Schema.optionalWith(Identity.Tenant.fields.tenant, { as: "Option" }),
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
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
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
- Owner: `Token` — `establish` resolves-or-enrolls a `CredentialRef` into a `Subject` and mints the first pair, `refresh` rotates through the `RotationStep` statechart with reuse detection, `revoke` ends a session. `RotationStep` is the transition family — `Rotate` (live session, fingerprint matched), `Expired` (window elapsed), `Reused` (live session, fingerprint rejected — a replayed rotated token) — folded by the pure `_step` and dispatched by `$match`, so protocol position is a case value and a new lifecycle rule (idle timeout, device binding) is a new tagged arm, never another inline guard. `CurrentClaims`/`BearerGuard` are the declarative auth seam: the middleware Tag carries `HttpApiSecurity.bearer`, its implementation folds `Jwt.verify` over the decoded credential, the serve plane's admission lift composes the same provision, and a consumer composing security without it mounts the Tag so a bearer-protected endpoint receives `AccessClaims` through the requirement channel.
- Law: the access token is a `crypt/sign` `AccessClaims` JWT; the refresh is a `Crypto` opaque token minted over the shared `Alphabet.base62` row — no human types it, so a weaker lowercase-alnum dialect buys no legibility and only narrows the space — and its SHA-256 fingerprint alone is stored; the wire form is `${sid}.${secret}` decoded through the `_RefreshWire` `Schema.TemplateLiteralParser` owner so `refresh` reads the session before touching the secret and a malformed frame is one typed `mismatch`.
- Law: the `Reused` arm is the breach arm and it is loud — `revokeSubject` settles before observable taps; its failure is retained while `Reject.mark("reuse")`, the durable `Reuse` fact, and subject-redacted error logs publish before the invariant generic `reuse` verdict surfaces. Revoke failure routes through the same breached-fact remediation lane and never erases the replay verdict; every `refresh` runs under the `Curb` `refresh` row keyed by `sid` and an exhausted budget is `throttled`, so an offline brute-force of a stolen `sid` is bounded by the store-backed limiter across every app sharing the library.
- Law: the bearer seam and the rotation seam both carry their denominator — `BearerGuard` taps `Reject.mark("bearer")` before re-spelling the verify fault through `_SPELL` and composes `Reject.measured("bearer")` so every admitted presentation lands its `bearer`-kinded admission and wall span, and `refresh` composes `Reject.measured("refresh")` under its own admission kind while the breach arm marks `reuse` absolutely; the folder's two highest-traffic ceremonies therefore publish a ratio rather than a bare refusal rate, and a credential-stuffing spike separates from a traffic spike on the same key set.
- Law: rotation is mandatory per `refresh` — a fresh secret, a bumped `generation`, a replaced session; ids mint through `Crypto.uuid` over the folder entropy port, never the ambient global, so a seeded reader makes the whole lifecycle deterministic under test. Access and refresh TTLs are the `_policy` record's two described rows, resolved at the boot line.
- Law: a `crypt/sign` fault re-spells onto ONE session family and the re-spell keeps its BLAME — `_SPELL` is the total `Fault.Class.Kind` row table both guards and every `Token` leg fold through, so a caller-blamed class lands a caller-blamed session reason and a system-blamed class lands `store`; a lapsed credential stays 401, an exhausted budget stays 429, and only a genuine outage reads 503. Folding every class onto `store` billed a denied claim and a refused algorithm — the caller's own token — to the session store's availability series, so the one signal telling an operator the deployment is sick answered on user typing.
- Law: `Jwt` rides the requirement channel — `Jwt.Default` is a Layer factory over a `Keyset`, so the composition root satisfies it with a `Rotation` pair and the `Jwt` authority swaps its live ring in place, never a static dependency row here.
- Law: the `scope` array `establish` carries is the session's delegation bound the `access/claim` ceiling reads — a `rasm:`-namespaced entry caps the session's authority to that bundle's union, an array carrying none is a first-party session at the subject's whole role grant, and the vocabulary is `access/claim`'s `Scope`, so a narrowed session mints by naming its `rasm:` scopes at `establish` and no per-mint delegation table forks the one `ScopeGrant` owns.
- Receipt: `TokenPair` — access and refresh both `Redacted`, the `Session` embedded so the caller frames it or audits it without a second read.
- Boundary: `authn/*` resolves a `CredentialRef` and calls `establish`; `Cookie` frames the pair; the ports carry state; `crypt/verify`'s `Curb` owns the refresh budget row; the runtime serve admission lift realizes the bearer provision; a direct HttpApi consumer mounts `BearerGuard`.
- Packages: `crypt/sign` (`Jwt.mint`/`verify`, `Crypto.token`/`.uuid`/`.fingerprint`/`.matches`, `Alphabet.base62`, `SignFault`); `crypt/verify` (`Reject`, `Curb`); `access/audit` (`Witness`, `SecurityFact`); `@rasm/ts/core` (`Fault.Class`); `@effect/platform` (`HttpApiMiddleware`, `HttpApiSecurity`); `effect` (`Config`, `DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
type RotationStep = Data.TaggedEnum<{
  Rotate: { readonly session: Session }
  Expired: { readonly session: Session }
  Reused: { readonly session: Session }
}>

const _RotationStep = Data.taggedEnum<RotationStep>()

const _RefreshWire = Schema.TemplateLiteralParser(_SessionId, ".", Schema.String)

// Re-spelling to ONE family is this seam's law; flattening the BLAME axis never was. Each row keeps the class the
// core lattice already grades, so a caller-blamed `SignFault` (a denied claim, a refused algorithm, a malformed
// key) answers on the caller's side while a system-blamed one answers on the deployment's, and the table is total
// over `Fault.Class.Kind` so a new core class breaks here at compile time instead of defaulting to an outage.
const _SPELL: { readonly [K in Fault.Class.Kind]: SessionFault.Respelled } = {
  absent: "notFound",
  conflicted: "mismatch",
  invalid: "mismatch",
  malformed: "mismatch",
  denied: "denied",
  expired: "expired",
  exhausted: "throttled",
  unavailable: "store",
  breached: "store",
  defect: "store",
}

// The one SESSION_ decode site: both lifetime rows resolve at the boot line as one described record.
const _policy = Config.unwrap({
  accessTtl: Config.duration("SESSION_ACCESS_TTL").pipe(
    Config.withDefault(Duration.minutes(15)),
    Config.withDescription("access JWT lifetime the mint stamps"),
  ),
  refreshTtl: Config.duration("SESSION_REFRESH_TTL").pipe(
    Config.withDefault(Duration.days(30)),
    Config.withDescription("refresh window a rotation renews; the session expiry and the cookie maxAge both read it"),
  ),
})

const _step = (session: Session, now: DateTime.Utc, matched: boolean): RotationStep =>
  DateTime.greaterThan(now, session.expiresAt) ? _RotationStep.Expired({ session })
    : matched ? _RotationStep.Rotate({ session })
    : _RotationStep.Reused({ session })

// The coordinate is the respell's one parameter, never a second respell member: a refresh leg already holds the
// session id the refusal is about, a bearer guard holds only the ceremony, and both mint through this one site.
const _spell = (coordinate: string) => (fault: SignFault): SessionFault =>
  new SessionFault({ case: { reason: _SPELL[fault.class], coordinate, cause: fault.message } })

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
        jwt.verify(token).pipe(
          Effect.tapError(() => Reject.mark("bearer")),
          Reject.measured("bearer"), // the guard is the plane's highest-traffic ceremony: its admissions are the denominator every reject ratio divides by
          Effect.mapError(_spell("bearer")),
        ),
    })),
  )
}

class Token extends Effect.Service<Token>()("security/authn/Token", {
  effect: Effect.gen(function* () {
    const jwt = yield* Jwt
    const cipher = yield* Crypto
    const store = yield* SessionStore
    const journal = yield* IdentityJournal
    const curb = yield* Curb
    const { accessTtl, refreshTtl } = yield* _policy
    const _secret = cipher.token(Alphabet.base62, 48).pipe(Effect.mapError(_spell))
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
          Effect.mapError(() => new SessionFault({ case: { reason: "mismatch", coordinate: "refresh", cause: "frame is not <sid>.<secret>" } })))
        return yield* curb.guard("refresh", sid, (cause: string) => new SessionFault({ case: { reason: "throttled", coordinate: sid, cause } }))(
          Effect.gen(function* () {
            const session = yield* Effect.flatMap(store.read(sid), Option.match({
              onNone: () => Effect.fail(new SessionFault({ case: { reason: "notFound", coordinate: sid, cause: "no session under this id" } })),
              onSome: Effect.succeed,
            }))
            const now = yield* DateTime.now
            const matched = yield* cipher.matches(Probe.Digest({ opaque: Redacted.make(secret), stored: session.refreshHash })).pipe(Effect.mapError(_spell(sid)))
            return yield* _RotationStep.$match(_step(session, now, matched), {
              Expired: ({ session: held }) =>
                Effect.fail(new SessionFault({ case: { reason: "expired", coordinate: held.id, cause: "refresh window closed" } })),
              Reused: ({ session: held }) =>
                Effect.gen(function* () {
                  const revocation = yield* store.revokeSubject(held.subject).pipe(
                    Effect.as(Option.none<SessionFault>()),
                    Effect.catchAll((fault) => Effect.succeed(Option.some(fault))),
                  )
                  yield* Reject.mark("reuse")
                  yield* Witness.publish(SecurityFact.Reuse({ subject: held.subject, sid: held.id, tenant: held.tenant }))
                  yield* Option.match(revocation, {
                    onNone: () => Effect.void,
                    onSome: (fault) => Effect.logError("subject revocation requires remediation", fault),
                  })
                  yield* Effect.logError("refresh reuse detected")
                  return yield* Effect.fail(new SessionFault({
                    case: { reason: "reuse", sid: held.id, subject: held.subject, generation: held.generation },
                  }))
                }).pipe(Effect.annotateLogs("subject", Redacted.make(held.subject))),
              Rotate: ({ session: held }) => _rotate(held, now),
            })
          }),
        )
      }).pipe(Reject.measured("refresh"), Effect.withSpan("security.token.refresh"))
    const revoke = (id: Session["id"]): Effect.Effect<void, SessionFault> => store.revoke(id)
    return { establish, refresh, revoke } as const
  }),
  dependencies: [Crypto.Default, Curb.Default],
  accessors: true,
}) {}
```

## [04]-[COOKIE_EGRESS]

[COOKIE_EGRESS]:
- Owner: `Cookie` — the session's egress projection over the `@effect/platform` `Cookies` codec: `frame` maps a `TokenPair` onto the access and refresh specs with `maxAge` derived from the session window, `clear` folds every `CookieSpec` role into an expiry, `csrf` mints the readable CSRF cookie, `verify` compares it constant-time to the presented header. `CookieSpec` is the attribute policy table — one row per role carrying `{ name, options }`, the `csrf` row alone widened with `header`, the double-submit header spelling — and the platform `Cookies.Cookie` is the framed value, so serialization, attribute rendering, and the `__Host-`/`__Secure-` prefix semantics are the platform's, never a hand-rolled string; `CsrfFault` is the folder fault shape at 403 exposure. `SessionGuard` is the framing's server half — the cookie-scheme middleware providing `CurrentClaims` to a browser app.
- Law: every cookie attribute is a policy row, not a call-site literal — `httpOnly`, `secure`, `sameSite`, `path` live in the table, so a new role or a `sameSite` change reframes every write with zero handler edit; the refresh cookie is path-scoped to the refresh route so it never rides an ordinary request; a static policy row that refuses `Cookies.makeCookie` is a defect, so the lift is `Effect.orDie`.
- Law: the token unwraps exactly at the framing seam — the returned `Cookies.Cookie` values are the egress set the edge writes immediately (a `HttpServerResponse.setCookie` fold or `Cookies.fromIterable` → `Cookies.toSetCookieHeaders`), and this page composes no header bag at all, so masking the `cookie`/`set-cookie` pair belongs to the WRITING edge, which folds `Headers.redact` over any bag it logs; log-side and OTLP-side scrubbing beyond that is `runtime/otel/emit#REDACTION`'s policy.
- Law: CSRF is constant-time double-submit — `csrf` mints a high-entropy token through `Crypto.token`, `verify` routes the `Text` probe, a mismatch or an absent pair is `CsrfFault`, and every rejection lands `Reject.mark("csrf")` beside the `Reject.measured("csrf")` admission the passing pair lands, so the gate's own pass rate is queryable; a timing oracle and a stripped header both fail closed. The double-submit pair is ONE row read twice: the runtime serve gate reads `CookieSpec.csrf.header` for the echoed header and the browser dial stamps the same field, so neither end mints a route literal and neither reuses the cookie name — the fork the branch ruling forecloses.
- Law: `SessionGuard` is the cookie-scheme face of the claims provision — the middleware Tag declares `HttpApiSecurity.apiKey`'s cookie variant over the access role, its implementation folds `Jwt.verify` and then the CSRF double-submit gate on every state-changing method, refusals land the `bearer` ledger row before re-spelling through `_SPELL`, and admissions ride `Reject.measured("bearer")` — so browser and API callers publish one denominator; safe methods (`GET`/`HEAD`/`OPTIONS`) skip the CSRF gate, and the `csrf` ledger row is already counted inside `Cookie.verify`.
- Law: the CSRF refusal rides its OWN class out — the Tag's `failure` is the `SessionFault`/`CsrfFault` union, so a stripped or forged double-submit pair reaches the problem ladder as `denied` and answers 403 while a refused bearer answers on the class `_SPELL` gave it, and both `absent` and `mismatch` survive as reasons rather than as one string. Re-spelling the gate's verdict onto a session `mismatch` erased exactly the status distinction the two-arm family exists to carry: an authenticated browser missing its header read identically to an unverifiable token.
- Receipt: `ReadonlyArray<Cookies.Cookie>` on frame/clear, `Cookies.Cookie` on csrf; `verify` returns `void` or `CsrfFault`, never a boolean the caller can forget.
- Growth: a new cookie role is one `CookieSpec` row; a new CSRF posture is one `verify` policy value.
- Boundary: `Token` owns the token values; `crypt/sign` mints and compares the CSRF token and owns the `Alphabet` row the CSRF mint composes; the edge applies `Cookies.toSetCookieHeaders` and lifts the request cookie/header; the runtime serve admission lift's cookie arm realizes the provision; a direct HttpApi consumer mounts `SessionGuard`.
- Packages: `@effect/platform` (`Cookies.makeCookie`, `Headers.get`, `HttpServerRequest`); `crypt/sign` (`Crypto.token`/`.matches`, `Alphabet`); `crypt/verify` (`Reject`); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Redacted`, `Schema`).

```typescript
const CookieSpec = {
  access: { name: "__Host-access", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/" } },
  refresh: { name: "__Secure-refresh", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/auth/refresh" } },
  csrf: { name: "__Host-csrf", header: "x-csrf-token", options: { httpOnly: false, secure: true, sameSite: "strict", path: "/" } },
} as const

const _EMPTY_VALUE = Redacted.make("")

// The pair leg names WHICH half arrived, because a missing header and a missing cookie point at different bugs in
// the calling client. The compare leg carries no subject and says so: both operands are the tokens themselves, and
// a renderer that named either would publish the secret the double-submit check exists to keep opaque.
const _csrfFamily = Fault.Class.family(["absent", "mismatch"] as const, {
  absent: Fault.Class.row({
    class: "denied",
    leg: "pair",
    detail: Schema.Struct({ cookie: Schema.Boolean, header: Schema.Boolean }),
    render: ({ cookie, header }) => `csrf pair incomplete: cookie ${cookie ? "present" : "absent"}, header ${header ? "present" : "absent"}`,
  }),
  mismatch: Fault.Class.row({
    class: "denied",
    leg: "compare",
    detail: Schema.Struct({}),
    render: () => "csrf cookie and header disagree",
  }),
})

declare namespace CookieSpec {
  type Role = keyof typeof CookieSpec
  type _Rows<T extends Record<string, { readonly name: string; readonly options: Cookies.Cookie["options"] }> = typeof CookieSpec> = T
}

declare namespace CsrfFault {
  type Case = typeof _csrfFamily.payload.Type
  type Reason = (typeof _csrfFamily.kinds)[number]
}

class CsrfFault extends Schema.TaggedError<CsrfFault>()("CsrfFault", {
  case: _csrfFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _csrfFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _csrfFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _csrfFamily.render(this.case)
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
      Effect.forEach(Struct.keys(CookieSpec), (role) => _framed(role, _EMPTY_VALUE, 0))
    const csrf = (): Effect.Effect<Cookies.Cookie> =>
      cipher.token(Alphabet.base62, 32).pipe(Effect.orDie, Effect.flatMap((token) => _framed("csrf", token)))
    const verify = (cookieToken: Option.Option<string>, headerToken: Option.Option<string>): Effect.Effect<void, CsrfFault> =>
      Option.match(Option.zipWith(cookieToken, headerToken, (held, presented) => ({ held, presented })), {
        onNone: () =>
          Effect.fail(new CsrfFault({
            case: { reason: "absent", cookie: Option.isSome(cookieToken), header: Option.isSome(headerToken) },
          })),
        onSome: ({ held, presented }) =>
          cipher.matches(Probe.Text({ held: Redacted.make(held), presented })).pipe(
            Effect.orDie,
            Effect.filterOrFail((matched) => matched, () => new CsrfFault({ case: { reason: "mismatch" } })),
            Effect.asVoid,
          ),
      }).pipe(Effect.tapError((fault) => Reject.mark("csrf", { reason: fault.case.reason })), Reject.measured("csrf"))
    return { frame, clear, csrf, verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

const _SAFE: ReadonlyArray<string> = ["GET", "HEAD", "OPTIONS"]

class SessionGuard extends HttpApiMiddleware.Tag<SessionGuard>()("security/authn/SessionGuard", {
  provides: CurrentClaims,
  // Two refusals, two classes: the bearer leg answers on the class `_SPELL` derived, the CSRF gate answers `denied`
  // under its own two-arm family. One `failure` schema forced the gate's verdict into a session `detail` string,
  // which handed the problem ladder a `malformed` for a refusal the whole double-submit design exists to make a 403.
  failure: Schema.Union(SessionFault, CsrfFault),
  security: { cookie: HttpApiSecurity.apiKey({ in: "cookie", key: CookieSpec.access.name }) },
}) {
  static readonly Live: Layer.Layer<SessionGuard, never, Jwt | Cookie> = Layer.effect(
    SessionGuard,
    Effect.map(Effect.all({ cookie: Cookie, jwt: Jwt }), ({ cookie, jwt }) => ({
      cookie: (token: Redacted.Redacted<string>) =>
        Effect.gen(function* () {
          const claims = yield* jwt.verify(token).pipe(
            Effect.tapError(() => Reject.mark("bearer")),
            Effect.mapError(_spell),
          )
          // CSRF gates the state-changing methods alone: the double-submit pair reads CookieSpec.csrf's ONE row,
          // cookie by name and echoed header by field, and Cookie.verify already lands the csrf ledger row, so
          // this seam adds nothing to the refusal and re-spells nothing away from it.
          const request = yield* HttpServerRequest.HttpServerRequest
          yield* Array.contains(_SAFE, request.method)
            ? Effect.void
            : cookie.verify(
                Option.fromNullable(request.cookies[CookieSpec.csrf.name]),
                Headers.get(request.headers, CookieSpec.csrf.header),
              )
          return claims
        }).pipe(Reject.measured("bearer")), // one denominator for the claims provision: browser and API presentations divide the same reject ratio
    })),
  )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { BearerGuard, Cookie, CookieSpec, CredentialRef, CsrfFault, CurrentClaims, IdentityJournal, Session, SessionFault, SessionGuard, SessionStore, Subject, Token, TokenPair }
export type { RotationStep }
```

## [05]-[RESEARCH]

(none)
