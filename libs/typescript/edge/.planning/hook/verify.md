# [EDGE_VERIFY]

`hook/verify.ts` is inbound webhook authenticity: one closed dialect table carries every provider signing convention — header name, signature grammar, timestamp participation — and one verify fold runs any dialect over the HELD request octets through `security`'s constant-time HMAC compare, so a provider integration is a table row, never a bespoke verifier. The byte-identity law governs the whole page: verification computes over the exact bytes admitted at the endpoint before any parse (a re-encoded body respells floats, key order, and escapes, and signs a document the provider never sent), and the octets travel onward untouched so `hook/admit.ts` enqueues what was verified. `HookFault` declared here is the hook sub-domain's one fault family — `verify` mints the authenticity reasons, `admit` the admission reasons — each row carrying the kernel class the `problem` altitude folds.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          | [PUBLIC]              |
| :-----: | :-------------- | :------------------------------------------------------------------ | :-------------------- |
|  [01]   | [HOOK_FAULT]    | the hook sub-domain fault family                                     | `HookFault`           |
|  [02]   | [DIALECT_TABLE] | the provider signing-convention rows and their parse folds           | `Signature`           |
|  [03]   | [VERIFY_FOLD]   | the constant-time verify pipeline and the `Verified` receipt         | `Signature`           |

## [2]-[HOOK_FAULT]

[HOOK_FAULT]:
- Owner: `HookFault` — the one reason family for the hook door: `missing` (required signature header absent), `malformed` (header grammar or signature encoding refused), `mismatch` (constant-time compare failed every candidate), `stale` (timestamp outside tolerance), `replayed` (event identity already claimed — minted by `admit`), `quota` (intake window exhausted — minted by `admit`, carrying the verdict's window), `closed` (ingress retired) — rows carrying the kernel class so status, exposure, and type derive through the governed policy record with zero local status column.
- Law: `retryAfter` rides the fault as `Option` evidence exactly like the middleware family — inhabited only by `quota` from the gate's own verdict — so the problem fold's grace ladder reads the truthful window on a 429.
- Law: a crypto-primitive fault is re-spelled at this seam — `security`'s `CryptoFault` from a malformed presented signature folds to `malformed` (caller-caused), never escapes as a 500 — the point-of-knowledge re-spell the carrier law demands.
- Packages: `effect` (`Schema`, `Option`); `kernel/fault/classify` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/kernel"
import { Crypto } from "@rasm/ts/security"
import { Array, DateTime, Duration, Effect, Either, Encoding, Number, Option, type Redacted, Schema, pipe } from "effect"

const _reasons = ["missing", "malformed", "mismatch", "stale", "replayed", "quota", "closed"] as const

const _faults = {
  missing: { class: "malformed" },
  malformed: { class: "malformed" },
  mismatch: { class: "denied" },
  stale: { class: "expired" },
  replayed: { class: "conflicted" },
  quota: { class: "exhausted" },
  closed: { class: "unavailable" },
} as const

declare namespace HookFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
}

class HookFault extends Schema.TaggedError<HookFault>()("HookFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
  retryAfter: Schema.optionalWith(Schema.DurationFromSelf, { as: "Option" }),
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<hook:${this.reason}> ${this.detail}`
  }
}
```

## [3]-[DIALECT_TABLE]

[DIALECT_TABLE]:
- Owner: `_dialects` — one row per provider signing convention, each carrying `header` (the signature header, lowercase), `parse` (header value to the candidate signature set plus the optional epoch-second stamp — `Option`-total, so any grammar refusal is one `malformed`), and `prefix` (the bytes the convention prepends to the payload before signing — the stripe `${t}.` frame; empty everywhere else): four rows — `github` (`sha256=<hex>`), `stripe` (`t=<epoch>,v1=<hex>` with rotation candidates — every `v1` is tried, so key rotation windows verify), `hex` (bare hex), `base64` (base64 re-encoded to hex at the parse so the compare speaks one encoding).
- Law: the candidate set is non-empty by parse — a row returning zero marks is a parse refusal, so the verify fold never runs an empty compare loop and "no signature" is `missing`/`malformed`, never a vacuous pass.
- Law: rows are grammar, never trust policy — tolerance, secrets, and freshness are verify-fold parameters; a row cannot weaken them, so adding a provider row is review-free on the security axis.
- Growth: a new provider is one row; a provider changing its grammar is a row edit that every intake inherits.

```typescript
declare namespace Signature {
  type Dialect = keyof typeof _dialects
  type Parsed = { readonly marks: Array.NonEmptyReadonlyArray<string>; readonly stamp: Option.Option<number> }
}

const _pairs = (value: string): ReadonlyArray<readonly [string, string]> =>
  Array.filterMap(value.split(","), (part) => {
    const at = part.indexOf("=")
    return at <= 0 ? Option.none() : Option.some([part.slice(0, at).trim(), part.slice(at + 1)] as const)
  })

const _marked = (marks: ReadonlyArray<string>, stamp: Option.Option<number>): Option.Option<Signature.Parsed> =>
  Array.isNonEmptyReadonlyArray(marks) ? Option.some({ marks, stamp }) : Option.none()

const _EMPTY = new Uint8Array(0)

const _dialects = {
  github: {
    header: "x-hub-signature-256",
    parse: (value: string) => _marked(value.startsWith("sha256=") ? [value.slice(7)] : [], Option.none()),
    prefix: () => _EMPTY,
  },
  stripe: {
    header: "stripe-signature",
    parse: (value: string) => {
      const pairs = _pairs(value)
      const stamp = pipe(
        Array.findFirst(pairs, ([key]) => key === "t"),
        Option.flatMap(([, held]) => Number.parse(held)),
      )
      return Option.isNone(stamp) ? Option.none() : _marked(Array.filterMap(pairs, ([key, held]) => (key === "v1" ? Option.some(held) : Option.none())), stamp)
    },
    prefix: (stamp: Option.Option<number>) => new TextEncoder().encode(`${Option.getOrElse(stamp, () => 0)}.`),
  },
  hex: {
    header: "x-signature",
    parse: (value: string) => _marked([value], Option.none()),
    prefix: () => _EMPTY,
  },
  base64: {
    header: "x-signature",
    parse: (value: string) =>
      _marked(
        Either.match(Encoding.decodeBase64(value), { onLeft: () => [], onRight: (bytes) => [Encoding.encodeHex(bytes)] }),
        Option.none(),
      ),
    prefix: () => _EMPTY,
  },
} as const
```

## [4]-[VERIFY_FOLD]

[VERIFY_FOLD]:
- Owner: `Signature.verify` — the one pipeline over any dialect: read the row's header (absence is `missing`), parse (refusal is `malformed`), gate freshness when the row carries a stamp (drift beyond tolerance in either direction is `stale` — both directions, because a future-dated stamp is as forged as an old one), frame the message as the row's prefix over the held octets, then fold the candidate set through `security`'s constant-time compare — first hit wins, exhaustion is `mismatch`. The compare is `Crypto.compare` and nothing else: no local HMAC, no `===` over signature bytes.
- Law: `Verified` is the receipt — dialect, verification instant, and the provider stamp as `Option<DateTime.Utc>` — the evidence `hook/admit.ts` threads into the admitted hook, so downstream consumers can audit freshness without re-verifying.
- Law: the fold's requirement channel carries `Crypto` alone — the secret arrives as a `Redacted<Uint8Array>` parameter from the app's per-source configuration, never resolved here, so one verify serves every source and secret rotation is configuration.
- Boundary: HMAC mechanics, constant-time discipline, and key handling are `security/sign/crypto`'s; which octets are held and what happens after verification is `hook/admit.ts`'s; endpoint body-ceiling admission is the serve seam's `withMaxBodySize` posture.
- Packages: `effect` (`Effect`, `DateTime`, `Duration`, `Option`, `Array`); `security/sign/crypto` (`Crypto`).

```typescript
class Verified extends Schema.Class<Verified>("Verified")({
  dialect: Schema.Literal("github", "stripe", "hex", "base64"),
  at: Schema.DateTimeUtcFromSelf,
  stamp: Schema.optionalWith(Schema.DateTimeUtcFromSelf, { as: "Option" }),
}) {}

const _message = (prefix: Uint8Array, held: Uint8Array): Uint8Array => Uint8Array.from([...prefix, ...held])

const _fresh = (stamp: Option.Option<number>, now: DateTime.Utc, tolerance: Duration.Duration): boolean =>
  Option.match(stamp, {
    onNone: () => true,
    onSome: (seconds) => Math.abs(DateTime.toEpochMillis(now) - seconds * 1000) <= Duration.toMillis(tolerance),
  })

const _verify = (
  dialect: Signature.Dialect,
  secret: Redacted.Redacted<Uint8Array>,
  held: Uint8Array,
  headers: Readonly<Record<string, string | undefined>>,
  tolerance: Duration.Duration,
): Effect.Effect<Verified, HookFault, Crypto> =>
  Effect.gen(function* () {
    const row = _dialects[dialect]
    const raw = yield* Option.match(Option.fromNullable(headers[row.header]), {
      onNone: () => Effect.fail(new HookFault({ reason: "missing", detail: row.header, retryAfter: Option.none() })),
      onSome: Effect.succeed,
    })
    const parsed = yield* Option.match(row.parse(raw), {
      onNone: () => Effect.fail(new HookFault({ reason: "malformed", detail: row.header, retryAfter: Option.none() })),
      onSome: Effect.succeed,
    })
    const now = yield* DateTime.now
    yield* Effect.when(
      Effect.fail(new HookFault({ reason: "stale", detail: dialect, retryAfter: Option.none() })),
      () => !_fresh(parsed.stamp, now, tolerance),
    )
    const cipher = yield* Crypto
    const message = _message(row.prefix(parsed.stamp), held)
    const hit = yield* Effect.findFirst(parsed.marks, (mark) =>
      cipher.compare(secret, message, mark).pipe(
        Effect.mapError(() => new HookFault({ reason: "malformed", detail: "signature encoding", retryAfter: Option.none() })),
      ))
    return yield* Option.match(hit, {
      onNone: () => Effect.fail(new HookFault({ reason: "mismatch", detail: dialect, retryAfter: Option.none() })),
      onSome: () =>
        Effect.succeed(new Verified({
          dialect,
          at: now,
          stamp: Option.flatMap(parsed.stamp, (seconds) => DateTime.make(seconds * 1000)),
        })),
    })
  })

const Signature: {
  readonly dialects: typeof _dialects
  readonly verify: typeof _verify
} = {
  dialects: _dialects,
  verify: _verify,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { HookFault, Signature, Verified }
```
