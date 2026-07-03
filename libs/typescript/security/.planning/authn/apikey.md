# [SECURITY_APIKEY] — machine credentials: mint, argon2 digest-at-rest, rotate/revoke, prefix-indexed resolve

`authn/apikey` owns the machine-credential family: minting a `rk_<prefix>.<secret>` key shown exactly once, storing only its argon2 digest keyed by the public prefix, resolving a presented key by prefix-indexed candidate lookup then constant-time argon2 verify, and rotating or revoking with a timestamp. Hashing delegates `sign/crypto` — argon2 fits here because the resolve budget is amortized over a prefix index (a full-table scan would be the naive alternative), and the digest-at-rest is the PHC string the `sign/crypto` cost row governs. Storage is the `ApiKeyStore` port the app root satisfies with `store`; the resolved `ApiKeyRecord` carries the `Subject["id"]` and scopes the edge lifts into a principal, so this page never mints a session itself — it authenticates a machine and hands the subject on. The plaintext key exists only in the `MintReceipt` returned once; every stored field is the digest or public metadata, and `ApiKeyFault` is the folder's 401 fault shape.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                             | [PACKAGES]                    | [REJECTED_FORM]                            |
| :-----: | :------------------------------ | :--------------------------------- | :---------------------------- | :----------------------------------------- |
|  [01]   | credential record + fault       | `ApiKeyRecord` / `ApiKeyFault`     | `effect` `Schema`             | a plaintext key column, a boolean verdict  |
|  [02]   | storage boundary                | `ApiKeyStore` port                 | `effect` `Context.Tag`        | a `store` import, a full-table scan resolve |
|  [03]   | mint / resolve / rotate / revoke| `ApiKey.mint`/`.resolve`/`.rotate` | `sign/crypto`                 | a `getByKey`/`verifyKey` twin, bcrypt      |

## [2]-[CREDENTIAL_VOCABULARY]

[RECORD_AND_FAULT]:
- Owner: `ApiKeyRecord` is the stored credential — a UUID id, the public `prefix` index, the `Subject["id"]`, the argon2 `digest`, and lifecycle timestamps; `MintReceipt` carries the one-time plaintext; `ApiKeyFault` is the 401 reason-family. `ApiKeyStore` declares the prefix-indexed storage.
- Packages: `effect` — one `Schema.Class` for the record; `sign/crypto` for the digest, `session/token` for the `Subject["id"]` reference.
- Boundary: `store` satisfies `ApiKeyStore` at the app root; the edge lifts the resolved record's subject and scopes into a request principal; `sign/crypto` owns the hash, so no folder re-imports argon2.
- Growth: a new credential facet (a description, an IP allowlist) is one `ApiKeyRecord` field the store persists; a new failure mode is one `ApiKeyFault` reason.

```typescript
import { Effect, Context, DateTime, type Duration, Option, Redacted, Schema } from "effect"
import { Crypto } from "../sign/crypto.ts"
import type { Subject } from "../session/token.ts"

// --- [CONSTANTS] ------------------------------------------------------------------------

const _ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

const _reasons = ["malformed", "notFound", "revoked", "expired"] as const

const ApiKeyFaultPolicy = {
  malformed: { rank: 3, retry: false, status: 400 },
  notFound: { rank: 4, retry: false, status: 401 },
  revoked: { rank: 4, retry: false, status: 401 },
  expired: { rank: 2, retry: false, status: 401 },
} as const

declare namespace ApiKeyFault {
  type Reason = keyof typeof ApiKeyFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof ApiKeyFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class ApiKeyRecord extends Schema.Class<ApiKeyRecord>("ApiKeyRecord")({
  id: Schema.UUID,
  prefix: Schema.NonEmptyString,
  subject: Schema.UUID,
  digest: Schema.Redacted(Schema.String),
  name: Schema.NonEmptyString,
  scopes: Schema.Array(Schema.NonEmptyString),
  createdAt: Schema.DateTimeUtc,
  expiresAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  revokedAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  lastUsedAt: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
}) {}

class MintReceipt extends Schema.Class<MintReceipt>("MintReceipt")({
  record: ApiKeyRecord,
  secret: Schema.Redacted(Schema.String),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class ApiKeyFault extends Schema.TaggedError<ApiKeyFault>()("ApiKeyFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): ApiKeyFault.Row {
    return ApiKeyFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<apikey:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class ApiKeyStore extends Context.Tag("security/authn/ApiKeyStore")<ApiKeyStore, {
  readonly insert: (record: ApiKeyRecord) => Effect.Effect<void, ApiKeyFault>
  readonly byPrefix: (prefix: string) => Effect.Effect<ReadonlyArray<ApiKeyRecord>, ApiKeyFault>
  readonly touch: (id: string, at: DateTime.Utc) => Effect.Effect<void, ApiKeyFault>
  readonly revoke: (id: string, at: DateTime.Utc) => Effect.Effect<void, ApiKeyFault>
}>() {}
```

## [3]-[MINT_AND_RESOLVE]

[APIKEY]:
- Owner: `ApiKey.mint` issues `rk_<prefix>.<secret>` and stores its argon2 digest; `ApiKey.resolve` splits the prefix, loads candidates, argon2-verifies each, checks lifecycle, and touches `lastUsedAt`; `ApiKey.rotate` revokes and re-mints for the same subject; `ApiKey.revoke` timestamps. One polymorphic resolve dispatches on the presented value, never a `getByKey`/`verifyKey` twin.
- Packages: `sign/crypto` `Crypto.token` mints the prefix and secret, `Crypto.digest("apiKey", ...)`/`Crypto.verify` own the at-rest hash; `Effect.findFirst` picks the matching candidate under the amortized prefix index.
- Law: the plaintext leaves only through `MintReceipt`; the digest is the PHC the cost row governs; a stale-parameter match on resolve is a rehash signal the store persists; a revoked or expired record is a typed fault, never a silent accept.
- Receipt: `MintReceipt` on mint/rotate, `ApiKeyRecord` on resolve — the subject and scopes the edge lifts, never a bare boolean.

```typescript
// --- [OPERATIONS] -----------------------------------------------------------------------

const _prefixOf = (presented: string): Option.Option<string> =>
  Option.map(Option.filter(Option.some(presented.indexOf(".")), (dot) => dot > 0), (dot) => presented.slice(0, dot))

// --- [SERVICES] -------------------------------------------------------------------------

class ApiKey extends Effect.Service<ApiKey>()("security/authn/ApiKey", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* ApiKeyStore
    const mint = (subject: Subject["id"], name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, ApiKeyFault> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const prefixBody = yield* cipher.token(_ALPHABET, 8).pipe(Effect.orDie)
        const secret = yield* cipher.token(_ALPHABET, 40).pipe(Effect.orDie)
        const prefix = `rk_${Redacted.value(prefixBody)}`
        const presented = Redacted.make(`${prefix}.${Redacted.value(secret)}`)
        const digest = yield* cipher.digest("apiKey", presented).pipe(Effect.orDie)
        const record = new ApiKeyRecord({
          id: crypto.randomUUID(), prefix, subject, digest, name, scopes, createdAt: now,
          expiresAt: Option.map(ttl, (input) => DateTime.addDuration(now, input)), revokedAt: Option.none(), lastUsedAt: Option.none(),
        })
        yield* store.insert(record)
        return new MintReceipt({ record, secret: presented })
      })
    const resolve = (presented: Redacted.Redacted<string>): Effect.Effect<ApiKeyRecord, ApiKeyFault> =>
      Effect.gen(function* () {
        const prefix = yield* Option.match(_prefixOf(Redacted.value(presented)), { onNone: () => Effect.fail(new ApiKeyFault({ reason: "malformed", detail: "no prefix" })), onSome: Effect.succeed })
        const candidates = yield* store.byPrefix(prefix)
        const hit = yield* Effect.findFirst(candidates, (record) => cipher.verify("apiKey", record.digest, presented).pipe(Effect.map((verdict) => verdict._tag === "Matched"), Effect.orDie))
        const record = yield* Option.match(hit, { onNone: () => Effect.fail(new ApiKeyFault({ reason: "notFound", detail: prefix })), onSome: Effect.succeed })
        yield* Option.isSome(record.revokedAt) ? Effect.fail(new ApiKeyFault({ reason: "revoked", detail: record.id })) : Effect.void
        const now = yield* DateTime.now
        yield* Option.match(record.expiresAt, { onNone: () => false, onSome: (exp) => DateTime.greaterThan(now, exp) })
          ? Effect.fail(new ApiKeyFault({ reason: "expired", detail: record.id }))
          : Effect.void
        yield* store.touch(record.id, now)
        return record
      })
    const rotate = (id: string, subject: Subject["id"], name: string, scopes: ReadonlyArray<string>, ttl: Option.Option<Duration.DurationInput>): Effect.Effect<MintReceipt, ApiKeyFault> =>
      Effect.flatMap(DateTime.now, (now) => Effect.zipRight(store.revoke(id, now), mint(subject, name, scopes, ttl)))
    const revoke = (id: string): Effect.Effect<void, ApiKeyFault> => Effect.flatMap(DateTime.now, (now) => store.revoke(id, now))
    return { mint, resolve, rotate, revoke } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ApiKey, ApiKeyFault, ApiKeyRecord, ApiKeyStore, MintReceipt }
```
