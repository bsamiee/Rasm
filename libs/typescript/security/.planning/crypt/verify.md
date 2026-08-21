# [SECURITY_VERIFY]

External-signature ingress and the folder's admission and throttle planes: one closed dialect table carries every inbound authenticity convention — symmetric HMAC webhooks and asymmetric ECDSA/RSA partner and attestation signatures in both PKIX-DER and IEEE-P1363 wire forms — and one verify fold runs any dialect over the HELD request octets, so a provider integration is a table row, never a bespoke verifier. Byte-identity law governs the whole page: verification computes over the exact bytes admitted at the edge before any parse, because a re-encoded body respells floats, key order, and escapes and signs a document the provider never sent, and the octets travel onward untouched. HMAC dialects route `crypt/sign`'s `Crypto.matches` `Mac` probe; the asymmetric dialects route WebCrypto `subtle.verify`, the `PublicKey` tagged family carrying the SEC1/PKIX key-encoding axis and the dialect row the `sigForm` signature-encoding axis, so a partner signing raw `r‖s` P1363 (the JWS ES256 wire form) and a partner shipping SPKI-DER keys both land as rows — a PKIX-DER signature normalizes to P1363 and a PKCS1 key wraps to SPKI at the seam WebCrypto's own import demands, both folder-owned ASN.1 rewrites over `@oslojs/encoding` decode. Both folder-wide planes live here: `Curb` is the store-backed brute-force budget every credential-verify ceremony draws its policy row from, and `Reject` is the folder-wide authenticity ledger over ONE closed `kind` discriminant and bounded dialect/surface/reason facets — `mark` counts a refusal, `admit` counts the same kind's success, and `measured` is the ceremony aspect that times the wall span and admits on the success arm — so refusals, their denominator, and their latency are three `Convention`-named series joined on one key, every ratio the plane exists to answer is queryable, and a per-page counter name has no spelling anywhere in the folder. One ceremony anchor feeds both planes as stated projections, and each kind's `breach` column decides whether it earns a denominator at all. `VerifyFault` instantiates the folder fault shape over one core `Fault.Class.family` mint, folding a `crypt/sign` primitive fault to a caller-caused `malformed` at this seam so a bad presented signature is never a 500. Timestamp participation, candidate rotation, and the signed prefix are row grammar; tolerance, keys, and freshness are fold parameters a row cannot weaken, so admitting a dialect is review-free on the security axis. `Intake` is the typed `HttpApiMiddleware` face of the held-octets seam, reading its dialect from the `IntakeRoute` binding its mounting layer declares because two header spellings each carry two rows — the runtime serve route owner realizes the same fold over `Verify`, and a direct HttpApi consumer mounts both.

## [01]-[INDEX]

- [02]-[VERIFY_FAULT]: the folder fault shape and the `crypt/sign` re-spell; `VerifyFault`.
- [03]-[DIALECT_TABLE]: the signing-convention rows and their header parse folds; `Verify`.
- [04]-[VERIFY_FOLD]: the throttled constant-time verify pipeline, `Verified` receipt, key registry, intake middleware and its route binding; `Verify`, `Intake`, `IntakeRoute`, `CurrentVerified`.
- [05]-[ADMISSION_LEDGER]: the folder-wide authenticity ledger: refusal counter, admission twin, ceremony histogram, kind discriminant with its breach column, the ceremony anchor both planes project, bounded facets; `Reject`.
- [06]-[THROTTLE]: the folder-wide auth-throttle owner: per-surface budget rows, the store-backed token-bucket guard; `Curb`.

## [02]-[VERIFY_FAULT]

[VERIFY_FAULT]:
- Law: a crypto-primitive fault is re-spelled at this seam — a `SignFault` from a malformed presented signature folds to `malformed` (caller-caused), never escapes as a `defect`; a genuine key or algorithm defect on Rasm's side stays a fold-internal `defect`.
- Law: verification is result-typed — a valid signature lands the `Verified` receipt, a failed one a typed fault; there is no boolean-plus-throw and a `false` compare is `mismatch`, never a thrown value.
- Growth: a new failure mode is one family row carrying its core kind, its leg, the subject a raise must supply, and that subject's renderer.
- Law: legs partition the fold — header, freshness, key, compare, throttle — so a refusal names which stage of one verify refused before its subject is read.
- Packages: `effect` (`Schema`, `Duration`); `@rasm/ts/core` (`Fault.Class`); `crypt/sign` (`SignFault`).

```typescript
import { RateLimiter } from "@effect/experimental"
import { HttpApiMiddleware } from "@effect/platform"
import { decodeBase64, decodeHex, encodeHexLowerCase } from "@oslojs/encoding"
import { Convention, Fault } from "@rasm/ts/core"
import { Array, Config, Context, Data, DateTime, Duration, Effect, Either, Layer, Metric, Number, Option, Predicate, Record, Redacted, Schema, pipe } from "effect"
import { Crypto, Probe, SignFault } from "./sign.ts"

// Five legs partition the fold and each reason renders its OWN subject, because the operator questions differ per
// leg: a header refusal names the header the dialect never sent, a compare refusal names how many candidates were
// tried and failed, a freshness refusal names the stamp against the tolerance it missed, and a key refusal names
// the `kid` whose registry entry no verifier could be built from. One free `detail` string answered all five with
// prose an operator had to re-parse, and the mismatch arm in particular said nothing about candidate rotation.
const _family = Fault.Class.family(["missing", "malformed", "mismatch", "stale", "unknownKey", "throttled"] as const, {
  missing: Fault.Class.row({
    class: "malformed",
    leg: "header",
    detail: Schema.Struct({ dialect: Schema.String, header: Schema.String }),
    render: ({ dialect, header }) => `${dialect} presented no ${header} header`,
  }),
  malformed: Fault.Class.row({
    class: "malformed",
    leg: "header",
    detail: Schema.Struct({ dialect: Schema.String, cause: Schema.String }),
    render: ({ cause, dialect }) => `${dialect} signature material is unreadable: ${cause}`,
  }),
  mismatch: Fault.Class.row({
    class: "denied",
    leg: "compare",
    detail: Schema.Struct({ dialect: Schema.String, candidates: Schema.Int }),
    render: ({ candidates, dialect }) => `${dialect} offered ${candidates} candidate signatures and none verified`,
  }),
  stale: Fault.Class.row({
    class: "expired",
    leg: "freshness",
    detail: Schema.Struct({ epoch: Schema.Int, tolerance: Schema.DurationFromSelf }),
    render: ({ epoch, tolerance }) => `stamp ${epoch} falls outside the ${Duration.toMillis(tolerance)}ms tolerance`,
  }),
  unknownKey: Fault.Class.row({
    class: "denied",
    leg: "key",
    detail: Schema.Struct({ kid: Schema.String, cause: Schema.String }),
    render: ({ cause, kid }) => `key ${kid} yields no usable verifier: ${cause}`,
  }),
  throttled: Fault.Class.row({
    class: "exhausted",
    leg: "throttle",
    detail: Schema.Struct({ scope: Schema.String, cause: Schema.String }),
    render: ({ cause, scope }) => `verify budget spent on ${scope}: ${cause}`,
  }),
})

declare namespace VerifyFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class VerifyFault extends Schema.TaggedError<VerifyFault>()("VerifyFault", {
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

// The re-spell keeps the dialect the primitive fault never carried, so a partner's unreadable material lands on the
// same subject every other header refusal on that dialect lands on.
const _respell = (dialect: Verify.Dialect) => (fault: SignFault): VerifyFault =>
  new VerifyFault({ case: { reason: "malformed", dialect, cause: fault.message } })
```

## [03]-[DIALECT_TABLE]

[DIALECT_TABLE]:
- Owner: `_dialects` — one row per inbound signing convention, each carrying `header` (the signature header, lowercase), `scheme` (`"hmac"` symmetric or `"ecdsa"`/`"rsa-pkcs1"`/`"rsa-pss"` asymmetric), `sigForm` on the ECDSA rows (`"pkix"` DER or `"p1363"` raw `r‖s`), `digest` on every asymmetric row (the `_DIGESTS` key selecting the one WebCrypto hash name the ECDSA verify, the RSA import, and the PSS message and MGF1 all read), `saltLen` on the PSS row, `parse` (header value to the candidate signature set with the optional epoch-second stamp — `Option`-total, so any grammar refusal is one `malformed`), and `prefix` (the bytes prepended to the payload before signing — the `${t}.` stripe frame, empty elsewhere). Its rows: `github` (`sha256=<hex>`, HMAC), `stripe` (`t=<epoch>,v1=<hex>` rotation candidates, HMAC), `hmacHex`/`hmacBase64` (bare digests, HMAC), `ecdsaPkix`/`ecdsaP1363` (`kid=<id>,sig=<base64>` ECDSA in either signature encoding), `rsaPss`/`rsaPkcs1` (`kid=<id>,sig=<base64>` RSA), `attestation` (raw base64 signature over the attestation object, ECDSA PKIX).
- Law: the hash and the PSS salt length are row grammar like every other axis — a partner signing SHA-512 is one `digest` value and a partner on a non-32-byte salt is one `saltLen`, because a hash pinned inside the fold makes the page's own "rows are grammar" law false for the two axes a provider integration most often moves.
- Law: the candidate set is non-empty by parse — a row returning zero marks is a parse refusal, so the verify fold never runs an empty compare loop and "no signature" is `missing`/`malformed`, never a vacuous pass; a base64 decode refusal is `Option.none`, never an empty-array sentinel, so decode failure and an empty candidate set never conflate; stripe's every `v1` candidate is tried, so key-rotation windows verify.
- Law: rows are grammar, never trust policy — tolerance, secrets, and keys are verify-fold parameters a row cannot weaken; an asymmetric row resolves its registry key by the presented `kid`, or by the dialect name when the row carries none (`attestation`), and the scheme and `sigForm` select the WebCrypto import format and verify algorithm.
- Law: the `_kinds` tuple anchors the key set — the `Verified.dialect` wire literal spreads it, and the guard pair closes tuple and table against each other in both directions, so a row without its tuple entry (or the converse) fails at the declaration.
- Growth: a new provider is one row and its tuple entry; a provider changing grammar is a row edit every intake inherits; a new asymmetric suite (Ed25519 when a partner signs with it) is one row over the existing key-registry resolution.
- Packages: WebCrypto `SubtleCrypto` (`importKey`/`verify` over ECDSA and RSA); the DER→P1363 signature and PKCS1→SPKI key normalizers are folder-owned; `@oslojs/encoding` (base64/hex decode).

```typescript
const _kinds = ["github", "stripe", "hmacHex", "hmacBase64", "ecdsaPkix", "ecdsaP1363", "rsaPss", "rsaPkcs1", "attestation"] as const

const _utf8 = new TextEncoder()
const _EMPTY = new Uint8Array(0)

// One entry per hash a partner can sign under, holding the WebCrypto algorithm name every asymmetric arm reads —
// the ECDSA verify hash, the RSA import hash, and the PSS message-and-MGF1 hash are one spelling per row, so a
// SHA-512 partner is a row rather than the three scattered SHA-256 literals a hardcoded fold once carried.
const _DIGESTS = {
  sha256: { subtle: "SHA-256" },
  sha512: { subtle: "SHA-512" },
} as const satisfies Record<string, { readonly subtle: "SHA-256" | "SHA-512" }>

declare namespace Verify {
  type Dialect = keyof typeof _dialects
  type Digest = keyof typeof _DIGESTS
  type Scheme = "hmac" | "ecdsa" | "rsa-pkcs1" | "rsa-pss"
  type SigForm = "pkix" | "p1363"
  type Parsed = { readonly marks: Array.NonEmptyReadonlyArray<string>; readonly kid: Option.Option<string>; readonly stamp: Option.Option<number> }
  type _Keys<K extends Dialect = (typeof _kinds)[number]> = K
  type _Kinds<K extends (typeof _kinds)[number] = Dialect> = K
}

const _pairs = (value: string): ReadonlyArray<readonly [string, string]> =>
  Array.filterMap(value.split(","), (part) => {
    const at = part.indexOf("=")
    return at <= 0 ? Option.none() : Option.some([part.slice(0, at).trim(), part.slice(at + 1)] as const)
  })

const _marked = (marks: ReadonlyArray<string>, kid: Option.Option<string>, stamp: Option.Option<number>): Option.Option<Verify.Parsed> =>
  Array.isNonEmptyReadonlyArray(marks) ? Option.some({ marks, kid, stamp }) : Option.none()

const _base64Hex = (value: string): Option.Option<string> =>
  Either.match(Either.try(() => decodeBase64(value)), {
    onLeft: () => Option.none(),
    onRight: (bytes) => Option.some(encodeHexLowerCase(bytes)),
  })

const _keyed = (value: string): Option.Option<Verify.Parsed> => {
  const pairs = _pairs(value)
  const kid = pipe(Array.findFirst(pairs, ([key]) => key === "kid"), Option.map(([, held]) => held))
  const marks = pipe(
    Array.findFirst(pairs, ([key]) => key === "sig"),
    Option.flatMap(([, held]) => _base64Hex(held)),
    Option.toArray,
  )
  return _marked(marks, kid, Option.none())
}

const _dialects = {
  github: {
    header: "x-hub-signature-256", scheme: "hmac",
    parse: (value: string) => _marked(value.startsWith("sha256=") ? [value.slice(7)] : [], Option.none(), Option.none()),
    prefix: () => _EMPTY,
  },
  stripe: {
    header: "stripe-signature", scheme: "hmac",
    parse: (value: string) => {
      const pairs = _pairs(value)
      const stamp = pipe(Array.findFirst(pairs, ([key]) => key === "t"), Option.flatMap(([, held]) => Number.parse(held)))
      return Option.isNone(stamp) ? Option.none()
        : _marked(Array.filterMap(pairs, ([key, held]) => (key === "v1" ? Option.some(held) : Option.none())), Option.none(), stamp)
    },
    prefix: (stamp: Option.Option<number>) => _utf8.encode(`${Option.getOrElse(stamp, () => 0)}.`),
  },
  hmacHex: { header: "x-signature", scheme: "hmac", parse: (value: string) => _marked([value], Option.none(), Option.none()), prefix: () => _EMPTY },
  hmacBase64: {
    header: "x-signature", scheme: "hmac",
    parse: (value: string) => Option.flatMap(_base64Hex(value), (hex) => _marked([hex], Option.none(), Option.none())),
    prefix: () => _EMPTY,
  },
  ecdsaPkix: { header: "x-signature-ecdsa", scheme: "ecdsa", sigForm: "pkix", digest: "sha256", parse: _keyed, prefix: () => _EMPTY },
  ecdsaP1363: { header: "x-signature-ecdsa-p1363", scheme: "ecdsa", sigForm: "p1363", digest: "sha256", parse: _keyed, prefix: () => _EMPTY },
  rsaPss: { header: "x-signature-rsa", scheme: "rsa-pss", digest: "sha256", saltLen: 32, parse: _keyed, prefix: () => _EMPTY },
  rsaPkcs1: { header: "x-signature-rsa", scheme: "rsa-pkcs1", digest: "sha256", parse: _keyed, prefix: () => _EMPTY },
  attestation: {
    header: "x-attestation-signature", scheme: "ecdsa", sigForm: "pkix", digest: "sha256",
    parse: (value: string) => Option.flatMap(_base64Hex(value), (hex) => _marked([hex], Option.none(), Option.none())),
    prefix: () => _EMPTY,
  },
} as const satisfies Record<string, {
  readonly header: string
  readonly scheme: Verify.Scheme
  readonly sigForm?: Verify.SigForm
  readonly digest?: Verify.Digest
  readonly saltLen?: number
  readonly parse: (value: string) => Option.Option<Verify.Parsed>
  readonly prefix: (stamp: Option.Option<number>) => Uint8Array
}>
```

## [04]-[VERIFY_FOLD]

[VERIFY_FOLD]:
- Owner: `Verify` — the assembled owner: `verify` runs a dialect over held octets against a resolved key into a `Verified` receipt under the per-key rate budget, and `PublicKeyStore` is the `Context.Tag` registry the asymmetric dialects resolve a partner or attestation public key from by `kid`. `PublicKey` is the tagged key family — `Ecdsa` carries `bytes`, the pinned `curve`, and the `encoding` axis (`sec1` raw point or `pkix` SPKI-DER), `Rsa` carries `bytes` and its `pkcs1`/`pkix` encoding — and `$match` drives the asymmetric dispatch and the `_FAMILY` row states which key family each scheme demands, so a scheme/key mismatch is a named configuration refusal rather than an if-ladder or a silent false. `Intake` is the `HttpApiMiddleware` face over the raw request octets before any body parse, `IntakeRoute` the per-group binding it reads its dialect from, and `CurrentVerified` the receipt Tag it hands the handler; the runtime serve route owner realizes the same held-octets fold through `Verify`.
- Law: the dialect is declared by the route, never inferred from the header — two rows share `x-signature` and two share `x-signature-rsa`, so header-to-row is one-to-many by design and a sniffing selector guesses on precisely the collisions a caller controls. `Verify.verify` takes the dialect as an argument and `Intake` reads it from the `IntakeRoute` binding its mounting layer supplies, so an ingress group either names its grammar or fails to compose.
- Law: the compare runs over the exact admitted bytes and never over a copy of them — the payload is the held request octets, the row's signed prefix rides beside them as its own value into the streaming HMAC and the streaming digest alike, and freshness is checked before the signature (a stale stamp short-circuits to `stale` under the caller's tolerance `Duration`), so a replay outside the window never reaches the compare and no verify allocates a second image of the request body.
- Law: every verify is throttled — the fold body runs under the `Curb` `verify` row keyed `<dialect>:<kid|dialect>`, an exhausted budget folds to `throttled` at the guard, and the store-backed limiter holds the budget across every app sharing the library; every fault lands `Reject.mark("verify", { dialect, reason })` and every admitted signature lands its `verify`-kinded twin and wall span through `Reject.measured`, so the dialect's reject ratio is queryable rather than inferred from traffic.
- Law: every asymmetric candidate resolves its key first — the registry key is the presented `kid` or the dialect name for a kid-less row, and a miss is `unknownKey`, never a silent skip; the ECDSA arm imports a `sec1` uncompressed point (`raw`) or a `pkix` SPKI key over the `P-256`/`P-384`/`P-521` roster and normalizes a PKIX-DER signature to the IEEE-P1363 form `subtle.verify` takes; the RSA arm imports an SPKI key — wrapping a `pkcs1` key first — and verifies RSASSA-PKCS1-v1_5 or PSS with the row's hash and salt length as `subtle` parameters. A `sec1` point in the compressed form WebCrypto's raw import rejects is a config `unknownKey` naming its fix: ship the key `pkix`.
- Law: a DECODE fault and a verify VERDICT are two answers, and the fold refuses to spell them the same — the key decodes ONCE ahead of the candidate loop, and a family mismatch, a curve that disagrees with the stored point, or a corrupt encoding lands `unknownKey` with its own reason facet on the ledger and an error log an operator can act on; only the per-candidate signature decode stays inside the loop, where a structurally garbage mark verifies `false` and a non-empty set matching none is `mismatch`. Collapsing every decode throw into `false` graded a misconfigured pinned curve and a genuine forgery identically, so the one signal telling an operator to fix a key never left the process.
- Law: the HMAC fold tries every candidate under one `Crypto.matches` `Mac` probe carrying the prefix and the octets as separate fields, and folds a primitive throw to `malformed`.
- Receipt: `Verified` — the `dialect`, the resolved `kid` when asymmetric, and the verified octet length, so the admitting edge enqueues exactly what was verified; `verify` returns `Verified` or a `VerifyFault`, never a boolean, and `Intake` hands the same receipt to its handler through `CurrentVerified`.
- Growth: a new scheme is one `$match` arm with its `_FAMILY` row; a new key roster is a registry row; a new hash is one `_DIGESTS` entry; the HMAC path never changes when an asymmetric row lands.
- Boundary: the edge holds the octets and lifts the header/signature into the fold; `crypt/sign` owns the streaming HMAC compare; the registry is satisfied by a config-sourced or fetched key set the composition root wires; `Curb` owns the budget row; the runtime serve route owner realizes the held-octets fold on ingress routes; a direct consumer mounts `Intake` over an `IntakeRoute.of` binding.
- Packages: WebCrypto `SubtleCrypto` (`importKey`/`verify`); the DER→P1363 and PKCS1→SPKI normalizers are folder-owned; `crypt/sign` (`Crypto.matches`); `@effect/platform` (`HttpApiMiddleware`); `effect` (`Context`, `Layer`, `Schema`, `Effect`, `Metric`, `Either`).

```typescript
type MacKey = Redacted.Redacted<Uint8Array>

type PublicKey = Data.TaggedEnum<{
  Ecdsa: { readonly bytes: Uint8Array; readonly curve: "p256" | "p384" | "p521"; readonly encoding: "sec1" | "pkix" }
  Rsa: { readonly bytes: Uint8Array; readonly encoding: "pkcs1" | "pkix" }
}>

const PublicKey = Data.taggedEnum<PublicKey>()

declare namespace PublicKey {
  type Curve = "p256" | "p384" | "p521"
}

class Verified extends Schema.Class<Verified>("Verified")({
  dialect: Schema.Literal(..._kinds),
  kid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  length: Schema.NonNegativeInt,
}) {}

class PublicKeyStore extends Context.Tag("security/crypt/PublicKeyStore")<PublicKeyStore, {
  readonly byKid: (kid: string) => Effect.Effect<Option.Option<PublicKey>, VerifyFault>
}>() {}

class CurrentVerified extends Context.Tag("security/crypt/CurrentVerified")<CurrentVerified, Verified>() {}

// Route declaration, never header sniffing: `hmacHex`/`hmacBase64` both arrive on `x-signature` and
// `rsaPss`/`rsaPkcs1` both on `x-signature-rsa`, so the header cannot key the table — a middleware inferring the
// row from the header picks between two candidates on exactly the collisions a caller controls. The mounting
// layer names the row, its tolerance, and its HMAC key for one ingress group, so an unbound group refuses at
// composition instead of defaulting into another provider's grammar.
class IntakeRoute extends Context.Tag("security/crypt/IntakeRoute")<IntakeRoute, {
  readonly dialect: Verify.Dialect
  readonly tolerance: Duration.Duration
  readonly mac: Option.Option<MacKey>
}>() {
  static readonly of = (row: Context.Tag.Service<IntakeRoute>): Layer.Layer<IntakeRoute> => Layer.succeed(IntakeRoute, row)
}

class Intake extends HttpApiMiddleware.Tag<Intake>()("security/crypt/Intake", {
  failure: VerifyFault,
  provides: CurrentVerified,
}) {}

// WebCrypto names the curve on import and pins the coordinate width the P1363 signature pads to: a P-256 `r`/`s`
// each ride 32 bytes, P-384 48, P-521 66. The width is the row's, never the signature's — a DER integer strips its
// sign byte and shrinks, so reading length off the mark would let a short-`r` signature verify against a padding
// nothing checked.
const _CURVES = { p256: "P-256", p384: "P-384", p521: "P-521" } as const satisfies Record<PublicKey.Curve, string>
const _COORD = { p256: 32, p384: 48, p521: 66 } as const satisfies Record<PublicKey.Curve, number>

// DER length prefix in the short/one-byte/two-byte forms an RSA public key reaches; a modulus past 65535 bytes is
// unreachable, so no four-byte form exists here.
const _derLen = (length: number): ReadonlyArray<number> =>
  length < 0x80 ? [length] : length < 0x100 ? [0x81, length] : [0x82, length >>> 8, length & 0xff]

// WebCrypto verifies only the IEEE-P1363 `r‖s` fixed-width form, so a partner's PKIX-DER ECDSA signature — the
// ASN.1 `SEQUENCE { INTEGER r, INTEGER s }` — reads each INTEGER here, strips the ASN.1 sign-pad byte, and left-pads
// to the curve's coordinate width. A `p1363` row already carries that form and never reaches here.
const _derToP1363 = (der: Uint8Array, size: number): Uint8Array => {
  let offset = (der[1] & 0x80) === 0 ? 2 : 2 + (der[1] & 0x7f)
  const readInt = (position: number): readonly [Uint8Array, number] => {
    if (der[position] !== 0x02) throw new Error("ecdsa der: expected INTEGER")
    const length = der[position + 1]
    let start = position + 2
    let remaining = length
    while (remaining > 0 && der[start] === 0x00) { start += 1; remaining -= 1 }
    if (remaining > size) throw new Error("ecdsa der: integer exceeds coordinate width")
    const out = new Uint8Array(size)
    out.set(der.subarray(start, start + remaining), size - remaining)
    return [out, position + 2 + length]
  }
  const [r, afterR] = readInt(offset)
  const [s] = readInt(afterR)
  const p1363 = new Uint8Array(size * 2)
  p1363.set(r, 0)
  p1363.set(s, size)
  return p1363
}

// WebCrypto imports an RSA public key only as SubjectPublicKeyInfo; a `pkcs1` partner ships the bare
// `RSAPublicKey`, so this wraps it in the fixed rsaEncryption AlgorithmIdentifier and the BIT STRING SPKI spells,
// byte-identical to what a standards library exports. A `pkix` row already carries SPKI and skips the wrap.
const _RSA_ALG_ID = [0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00] as const
const _pkcs1ToSpki = (pkcs1: Uint8Array): Uint8Array => {
  const bitString = [0x03, ..._derLen(pkcs1.length + 1), 0x00, ...pkcs1]
  const body = [..._RSA_ALG_ID, ...bitString]
  return new Uint8Array([0x30, ..._derLen(body.length), ...body])
}

const _sigForm = (row: (typeof _dialects)[Verify.Dialect]): Verify.SigForm =>
  Predicate.hasProperty(row, "sigForm") ? row.sigForm : "pkix"

const _digest = (row: (typeof _dialects)[Verify.Dialect]): Verify.Digest =>
  Predicate.hasProperty(row, "digest") ? row.digest : "sha256"

const _saltLen = (row: (typeof _dialects)[Verify.Dialect]): number =>
  Predicate.hasProperty(row, "saltLen") ? row.saltLen : 32

const _fresh = (stamp: Option.Option<number>, tolerance: Duration.Duration): Effect.Effect<void, VerifyFault> =>
  Option.match(stamp, {
    onNone: () => Effect.void,
    onSome: (epoch) =>
      Effect.flatMap(DateTime.now, (now) =>
        Duration.lessThanOrEqualTo(Duration.millis(Math.abs(DateTime.toEpochMillis(now) - epoch * 1000)), tolerance)
          ? Effect.void
          : Effect.fail(new VerifyFault({ case: { reason: "stale", epoch, tolerance } }))),
  })

// Which key family each asymmetric scheme demands, so a registry entry holding an RSA key under a `kid` an ECDSA
// route resolves reads as the configuration refusal it is rather than as a signature that merely never matches.
const _FAMILY = { ecdsa: "Ecdsa", "rsa-pkcs1": "Rsa", "rsa-pss": "Rsa" } as const satisfies Record<Exclude<Verify.Scheme, "hmac">, PublicKey["_tag"]>

// Row grammar projected into exactly the parameters a WebCrypto verify reads, so the fold reads no row field
// directly and a new axis lands as one more projection here.
type _Asym = {
  readonly scheme: Exclude<Verify.Scheme, "hmac">
  readonly form: Verify.SigForm
  readonly spec: (typeof _DIGESTS)[Verify.Digest]
  readonly saltLen: number
}

const _asym = (row: (typeof _dialects)[Verify.Dialect]): Option.Option<_Asym> =>
  row.scheme === "hmac"
    ? Option.none()
    : Option.some({ scheme: row.scheme, form: _sigForm(row), spec: _DIGESTS[_digest(row)], saltLen: _saltLen(row) })

// One imported key backs a verifier closure: `subtle.verify` hashes the raw message itself, so no pre-digest exists and,
// every asymmetric row carrying an empty prefix, the held octets cross untouched with no joined body copy. Import
// lands ONCE ahead of the candidate loop and its failure is Rasm's own configuration — a key family the route
// never resolves, a `sec1` point WebCrypto's raw import refuses because it is compressed rather than the `0x04`
// uncompressed form, or a corrupt encoding — so it answers `unknownKey`, while a garbage presented signature stays
// the ordinary per-candidate `mismatch`. Folding both to `false` once published one verdict for a misconfigured
// partner and a genuine forgery, leaving the dialect's reject ratio no facet to separate them.
type _Verifier = (sig: Uint8Array, message: Uint8Array) => Promise<boolean>

const _imported = (asym: _Asym, key: PublicKey, kid: string): Effect.Effect<_Verifier, VerifyFault> =>
  _FAMILY[asym.scheme] !== key._tag
    ? Effect.fail(new VerifyFault({ case: { reason: "unknownKey", kid, cause: `${key._tag} key offered to a ${asym.scheme} route` } }))
    : Effect.tryPromise({
        try: () =>
          PublicKey.$match(key, {
            Ecdsa: async ({ bytes, curve, encoding }): Promise<_Verifier> => {
              const held = await crypto.subtle.importKey(encoding === "sec1" ? "raw" : "spki", bytes, { name: "ECDSA", namedCurve: _CURVES[curve] }, false, ["verify"])
              return (sig, message) =>
                crypto.subtle.verify({ name: "ECDSA", hash: asym.spec.subtle }, held, asym.form === "pkix" ? _derToP1363(sig, _COORD[curve]) : sig, message)
            },
            Rsa: async ({ bytes, encoding }): Promise<_Verifier> => {
              const name = asym.scheme === "rsa-pss" ? "RSA-PSS" : "RSASSA-PKCS1-v1_5"
              const held = await crypto.subtle.importKey("spki", encoding === "pkcs1" ? _pkcs1ToSpki(bytes) : bytes, { name, hash: asym.spec.subtle }, false, ["verify"])
              return (sig, message) =>
                crypto.subtle.verify(asym.scheme === "rsa-pss" ? { name, saltLength: asym.saltLen } : { name }, held, sig, message)
            },
          }),
        catch: (cause) => new VerifyFault({ case: { reason: "unknownKey", kid, cause: String(cause) } }),
      })

class Verify extends Effect.Service<Verify>()("security/crypt/Verify", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const keys = yield* PublicKeyStore
    const curb = yield* Curb
    const verify = (
      dialect: Verify.Dialect,
      octets: Uint8Array,
      header: Option.Option<string>,
      mac: Option.Option<MacKey>,
      tolerance: Duration.Duration,
    ): Effect.Effect<Verified, VerifyFault> =>
      Effect.gen(function* () {
        const row = _dialects[dialect]
        const raw = yield* Option.match(header, {
          onNone: () => Effect.fail(new VerifyFault({ case: { reason: "missing", dialect, header: row.header } })),
          onSome: Effect.succeed,
        })
        const parsed = yield* Option.match(row.parse(raw), {
          onNone: () => Effect.fail(new VerifyFault({ case: { reason: "malformed", dialect, cause: `unparsable ${row.header} value` } })),
          onSome: Effect.succeed,
        })
        yield* _fresh(parsed.stamp, tolerance)
        const keyId = Option.getOrElse(parsed.kid, () => dialect)
        // Signed frame travels as its own value into both arms. Joining it onto the body materializes an
        // intermediate array of the whole request per verify — at the `verify` row's 60-per-minute budget that is
        // a memory amplifier any caller who can post reaches, and the octets must stay untouched regardless.
        const prefix = row.prefix(parsed.stamp)
        const matched = yield* curb.guard("verify", `${dialect}:${keyId}`, (cause) => new VerifyFault({ case: { reason: "throttled", scope: `${dialect}:${keyId}`, cause } }))(
          row.scheme === "hmac"
            ? Effect.flatMap(
                Option.match(mac, {
                  onNone: () => Effect.fail(new VerifyFault({ case: { reason: "malformed", dialect, cause: "no hmac key bound for this dialect" } })),
                  onSome: Effect.succeed,
                }),
                (key) => Effect.map(
                  Effect.forEach(parsed.marks, (mark) => cipher.matches(Probe.Mac({ key, prefix, body: octets, signature: mark })).pipe(Effect.mapError(_respell(dialect)))),
                  (results) => Array.contains(results, true),
                ),
              )
            : Effect.gen(function* () {
                const asym = yield* Option.match(_asym(row), {
                  onNone: () => Effect.fail(new VerifyFault({ case: { reason: "malformed", dialect, cause: "row carries no asymmetric grammar" } })),
                  onSome: Effect.succeed,
                })
                const key = yield* Effect.flatMap(keys.byKid(keyId), Option.match({
                  onNone: () => Effect.fail(new VerifyFault({ case: { reason: "unknownKey", kid: keyId, cause: "no registry entry" } })),
                  onSome: Effect.succeed,
                }))
                // Key import lands once and loudly: its refusal is an operator's to fix, so it logs at error and
                // carries its own reason facet, while each candidate's signature decode stays inside the loop where
                // a structurally garbage mark verifies false and the set answers `mismatch`. The signed prefix is
                // empty on every asymmetric row, so the held octets are the verified message with no joined copy.
                const check = yield* Effect.matchEffect(_imported(asym, key, keyId), {
                  onFailure: (fault) => Effect.zipRight(Effect.logError("verify key import refused", fault), Effect.fail(fault)),
                  onSuccess: Effect.succeed,
                })
                return yield* Effect.reduce(parsed.marks, false, (found, mark) =>
                  found ? Effect.succeed(true) : Effect.tryPromise(() => check(decodeHex(mark), octets)).pipe(Effect.orElseSucceed(() => false)))
              }),
        )
        return matched
          ? new Verified({ dialect, kid: parsed.kid, length: octets.byteLength })
          : yield* Effect.fail(new VerifyFault({ case: { reason: "mismatch", dialect, candidates: parsed.marks.length } }))
      }).pipe(
        Effect.tapError((fault) => Reject.mark("verify", { dialect, reason: fault.case.reason })),
        Reject.measured("verify", { dialect }), // the same kind carries the refusal and its denominator, so the ratio is a same-key join
        Effect.withSpan("security.verify", { attributes: { dialect } }),
      )
    return { verify } as const
  }),
  dependencies: [Crypto.Default, Curb.Default],
  accessors: true,
}) {}
```

## [05]-[ADMISSION_LEDGER]

[ADMISSION_LEDGER]:
- Owner: `Reject` — the folder's one authenticity ledger over the closed `_REJECTS` kind table (`bearer` a presented bearer, `ceremony` a webauthn challenge, `clone` a webauthn counter regression, `credential` an otp/recovery/apikey/workload presentation, `csrf` a double-submit pair, `refresh` a session rotation, `reuse` a presented rotated refresh, `state` an oauth ceremony state, `verify` an external signature), each row carrying its `breach` column, and three folds over it: `mark(kind, facet?)` increments the `securityRejects` refusal counter, `admit(kind, facet?)` its `securityAdmitted` twin, and `measured(kind, facet?)` is the ceremony aspect timing the wall span onto the `securityCeremony` distribution and admitting on the success arm — so one composed line at a ceremony entrypoint yields the refusal, the denominator, and the latency under one `Convention.rasm.securityKind` key.
- Law: admission rides the SAME kind its refusal rides, so every rate, burn, and ratio is a same-key join — a credential-stuffing spike separates from a traffic spike because both series move under one tag set; a surface with no refusal row therefore has no admission row.
- Law: a breach kind is read ABSOLUTELY and the type says so — `clone` and `reuse` carry `breach: true`, `Denominated` projects them out of `admit` and `measured`, and their denominator is the enclosing ceremony's kind: `ceremony` for a cloned authenticator, `refresh` for a replayed rotated token. Diluting a breach count by its own ceremony's admissions turns the one series an operator pages on into a ratio that a traffic increase flattens, which is why the rotation ceremony owns a kind separate from its replay arm rather than sharing one.
- Law: instrument metadata derives from the Convention row — name, description, dimension roster, and the UCUM unit column have one spelling on the core vocabulary spine, so the two admission rows reuse this table's kind vocabulary verbatim and a page-local counter name is the parallel this owner deletes.
- Law: ONE ceremony anchor feeds both folder planes — `_CEREMONIES` is the space, `Surface` projects the rows the `credential` kind's facet admits and `Curbed` the rows holding a throttle budget, so the two rosters cannot silently diverge and every divergence that stands has its reason written in the row rather than inferred from two tuples' difference. `_Ceremonies` closes the anchor against the union of its own projections exactly as `_Keys` closes the kind table.
- Law: facets are bounded tag values and two of the three axes carry that bound in their own types — `dialect` is the `[03]` dialect key space and `surface` the credential projection of the ceremony anchor, so a mistyped facet is a compile error rather than a second series; `reason` stays a string because every family that marks one declares its `Reason` above this stratum, and its bound is the caller passing that closed literal, never a member assembling text. An identifier-grade value (a subject, a kid, a session id) rides log annotations and span attributes, never a facet; one `_tagged` fold applies the kind and every present facet, so the three members cannot drift in their tagging.
- Law: ledger writes are evidence, never control flow — every member is total and composes through `Effect.tapError`, `Effect.tap`, or the pipeline seam beside the verdict it witnesses, so a metric failure can never alter a security verdict and `measured` never widens the fault channel it wraps.
- Entry: `Reject.measured("verify", { dialect })` in this page's fold; the authn pages compose one `measured` line per ceremony entrypoint — `bearer` at the guard, `refresh` at rotation, `csrf` at the double-submit check, `credential` at otp/recovery/api-key/workload resolve, `state` at the oauth callback, `ceremony` at both passkey finishes — beside the `mark` line their refusal arms already carry, with `reuse` and `clone` marking alone.
- Growth: a new ceremony kind is one `_REJECTS` row reaching the folds its `breach` column admits; a new throttled or credential surface is one `_CEREMONIES` row with its two plane flags; a new facet axis is one `_FACETS` row with its Convention tag key and its `Facet` field, the `_Facets` guard closing the two against each other.
- Packages: `effect` (`Metric`, `Record`, `Array`, `Duration`); `@rasm/ts/core` (`Convention`).

```typescript
// `breach` is the column the ledger's own law demanded and never held: a breach kind is read ABSOLUTELY, its
// denominator carried by the enclosing ceremony's kind, so it takes a `mark` and admits no twin. Spelling it here
// makes `admit`/`measured` reject it at the type level instead of trusting six call sites to remember. `refresh`
// is the rotation ceremony's own admission kind, so session refresh measures under a denominator of its own and
// its `reuse` replay arm keeps marking absolutely.
const _REJECTS = {
  bearer: { breach: false },
  ceremony: { breach: false },
  clone: { breach: true },
  credential: { breach: false },
  csrf: { breach: false },
  refresh: { breach: false },
  reuse: { breach: true },
  state: { breach: false },
  verify: { breach: false },
} as const

// ONE ceremony space anchors both planes the folder runs over it, each a stated projection rather than a second
// hand-kept roster: `credential` marks a surface whose refusals tag the `credential` kind's `surface` facet,
// `curbed` marks one holding a token-bucket budget row. The divergence is legible where it is decided —
// `workload` presents a credential the ledger tags but throttles at its own issuer, while `refresh`, `verify`,
// and `webauthn` carry their own ledger kinds and so never tag the credential facet.
const _CEREMONIES = {
  apikey: { credential: true, curbed: true },
  otp: { credential: true, curbed: true },
  recovery: { credential: true, curbed: true },
  refresh: { credential: false, curbed: true },
  verify: { credential: false, curbed: true },
  webauthn: { credential: false, curbed: true },
  workload: { credential: true, curbed: false },
} as const

const _FACETS = {
  dialect: Convention.rasm.securityDialect,
  reason: Convention.rasm.securityReason,
  surface: Convention.rasm.securitySurface,
} as const

declare namespace Reject {
  type Kind = keyof typeof _REJECTS
  // Every kind whose refusals earn a denominator: the breach rows drop out, so `Reject.admit("clone")` and
  // `Reject.measured("reuse")` are compile errors rather than a silently diluted breach rate.
  type Denominated = { [K in Kind]: (typeof _REJECTS)[K]["breach"] extends true ? never : K }[Kind]
  type Ceremony = keyof typeof _CEREMONIES
  type Surface = { [K in Ceremony]: (typeof _CEREMONIES)[K]["credential"] extends true ? K : never }[Ceremony]
  type Curbed = { [K in Ceremony]: (typeof _CEREMONIES)[K]["curbed"] extends true ? K : never }[Ceremony]
  // Two of the three axes close at their own anchors; `reason` cannot, because every folder fault family that
  // marks one sits at or above this stratum and no union reaches down here — its boundedness is the caller
  // passing its own closed `Reason` literal, which is why no member ever builds a reason string.
  type Facet = {
    readonly dialect?: Verify.Dialect
    readonly reason?: string
    readonly surface?: Surface
  }
  type _Keys<K extends Kind = keyof typeof _REJECTS> = K
  type _Ceremonies<K extends Ceremony = Surface | Curbed> = K
  type _Facets<K extends keyof typeof _FACETS = keyof Facet> = K
}

const _rejects = Convention.mount(Convention.metric.securityRejects)
const _admitted = Convention.mount(Convention.metric.securityAdmitted)
const _ceremony = Convention.mount(Convention.metric.securityCeremony)

// One tagging fold serves refusal, admission, and latency, so the three series carry byte-identical key sets and a
// join across them can never silently miss on a facet one member applied and another skipped.
const _tagged = <Type, In, Out>(
  metric: Metric.Metric<Type, In, Out>,
  kind: Reject.Kind,
  facet: Reject.Facet,
): Metric.Metric<Type, In, Out> =>
  Array.reduce(
    Record.toEntries(_FACETS),
    Metric.tagged(metric, Convention.rasm.securityKind, kind),
    (tagged, [key, tag]) =>
      Option.match(Option.fromNullable(facet[key]), { onNone: () => tagged, onSome: (held) => Metric.tagged(tagged, tag, held) }),
  )

const Reject = {
  kinds: _REJECTS,
  ceremonies: _CEREMONIES,
  mark: (kind: Reject.Kind, facet: Reject.Facet = {}): Effect.Effect<void> =>
    Metric.increment(_tagged(_rejects, kind, facet)),
  admit: (kind: Reject.Denominated, facet: Reject.Facet = {}): Effect.Effect<void> =>
    Metric.increment(_tagged(_admitted, kind, facet)),
  // One line at an entrypoint buys the wall span AND the denominator, so no arm lands the latency without its
  // admission or the admission without its latency; the parameter is `Denominated`, so a breach kind cannot be
  // handed a denominator that would read its absolute count as a rate.
  measured:
    (kind: Reject.Denominated, facet: Reject.Facet = {}) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      self.pipe(
        Metric.trackDuration(_tagged(_ceremony, kind, facet)),
        Effect.tap(() => Reject.admit(kind, facet)),
      ),
} as const
```

## [06]-[THROTTLE]

[THROTTLE]:
- Owner: `Curb` — the folder's one auth-throttle posture: `Surface` projects the ledger's `curbed` ceremony rows, `_CURB` is the per-surface budget table resolved as one described record at the boot line, and `guard(surface, key, exhausted)` runs a ceremony body under that surface's store-backed token bucket, folding the limiter's `RateLimiterError` onto the caller's own fault family exactly once. Session refresh, OTP verify, recovery redeem, api-key resolve, webauthn assert-finish, and this page's signature verify each key one row — a new throttled ceremony is a row and a `guard` line, never a sixth hand-wiring.
- Law: auth throttling REFUSES — `onExceeded: "fail"` is pinned at the single guard composition every row travels through, so no row carries the field and none can spell a delaying posture; an exhausted budget is a typed `throttled` verdict the caller's family classes `exhausted`; the branch's delaying postures stay distinct owners, and each ceremony keeps its own `Reject` mark and `measured` denominator beside the guard, so the collapse erases no counting.
- Law: the store is a named requirement — `RateLimiter.layer` rides `dependencies`, so `Curb.Default` publishes `RateLimiter.RateLimiterStore` in its requirement channel and every consumer inherits it; an unbound store fails composition rather than the first guarded ceremony.
- Law: the budget key is `<surface>:<caller key>` — the caller supplies the amortizing index its own page law names (a subject, a prefix, a sid, a dialect-kid pair), so one store-backed limiter bounds a guessing campaign across every app sharing the library under one key grammar.
- Growth: a new throttled ceremony is one `_CEREMONIES` row flagged `curbed` with its `_budget` row and one `guard` composition at its entrypoint.
- Boundary: the `RateLimiterStore` Layer is data-wave-satisfied and app-root-bound; each ceremony supplies its own `throttled` constructor, so no fault family crosses this seam.
- Packages: `@effect/experimental` (`RateLimiter.makeWithRateLimiter`, `RateLimiter.layer`, `RateLimiterStore`); `effect` (`Config`, `Duration`).

```typescript
declare namespace Curb {
  // Projected off the ledger's ceremony anchor, never re-listed: a throttled ceremony and its budget row cannot
  // drift apart, and the `_CURB` guard below closes the projection against the table in the other direction.
  type Surface = Reject.Curbed
}

const _budget = (surface: string, window: Duration.DurationInput, limit: number) =>
  Config.all({
    window: Config.duration(`CURB_${surface}_WINDOW`).pipe(
      Config.withDefault(Duration.decode(window)),
      Config.withDescription(`token-bucket window bounding the ${surface.toLowerCase()} surface`),
    ),
    limit: Config.integer(`CURB_${surface}_LIMIT`).pipe(
      Config.withDefault(limit),
      Config.withDescription(`presentations admitted per window on the ${surface.toLowerCase()} surface`),
    ),
  })

// One described record per namespace: every budget row resolves at this boot line, so an optional surface never
// defers its proof and a malformed environment fails the layer, not the first guarded ceremony.
const _CURB = Config.unwrap({
  apikey: _budget("APIKEY", "1 minute", 30),
  otp: _budget("OTP", "5 minutes", 5),
  recovery: _budget("RECOVERY", "5 minutes", 5),
  refresh: _budget("REFRESH", "1 minute", 10),
  verify: _budget("VERIFY", "1 minute", 60),
  webauthn: _budget("WEBAUTHN", "5 minutes", 10),
} satisfies Record<Curb.Surface, ReturnType<typeof _budget>>)

class Curb extends Effect.Service<Curb>()("security/crypt/Curb", {
  effect: Effect.gen(function* () {
    const rows = yield* _CURB
    const limit = yield* RateLimiter.makeWithRateLimiter
    // Refusal is pinned at the one call site every row travels, so `onExceeded` has no per-row spelling to get
    // wrong. Both experimental faults share the `"RateLimiterError"` tag and discriminate on `reason`, so the
    // fold catches one tag and reads that field — a two-tag catch matches neither and lets a store outage escape
    // this guard as an untyped defect. Each ceremony hands its own `throttled` constructor, so budget refusal and
    // store fault collapse onto the caller's family without a per-page pair.
    const guard = <E>(surface: Curb.Surface, key: string, exhausted: (detail: string) => E) =>
      <A, R>(body: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
        limit({ algorithm: "token-bucket", onExceeded: "fail", window: rows[surface].window, limit: rows[surface].limit, key: `${surface}:${key}` })(body).pipe(
          Effect.catchTag("RateLimiterError", (error) => Effect.fail(exhausted(error.reason === "Exceeded" ? key : error.message))))
    return { guard } as const
  }),
  // Store custody rides the type, not a prose promise: `RateLimiter.layer` satisfies the limiter this service
  // reads and leaves `RateLimiterStore` standing in `Curb.Default`'s requirement channel, so every consumer
  // inherits it and the app root binds `layerStoreMemory` on one node or the data wave's store on a fleet.
  dependencies: [RateLimiter.layer],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Curb, CurrentVerified, Intake, IntakeRoute, PublicKey, PublicKeyStore, Reject, Verified, Verify, VerifyFault }
export type { MacKey }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
