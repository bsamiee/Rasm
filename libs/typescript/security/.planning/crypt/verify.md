# [SECURITY_VERIFY]

External-signature ingress and the folder's admission and throttle planes: one closed dialect table carries every inbound authenticity convention — symmetric HMAC webhooks and asymmetric ECDSA/RSA partner and attestation signatures in both PKIX-DER and IEEE-P1363 wire forms — and one verify fold runs any dialect over the HELD request octets, so a provider integration is a table row, never a bespoke verifier. Byte-identity law governs the whole page: verification computes over the exact bytes admitted at the edge before any parse, because a re-encoded body respells floats, key order, and escapes and signs a document the provider never sent, and the octets travel onward untouched. HMAC dialects route `crypt/sign`'s `Crypto.matches` `Mac` probe; the asymmetric dialects route `@oslojs/crypto`'s verify-only public-key surface, with the `PublicKey` tagged family carrying the SEC1/PKIX key-encoding axis and the dialect row carrying the `sigForm` signature-encoding axis, so a partner signing raw `r‖s` P1363 (the JWS ES256 wire form) and a partner shipping SPKI-DER keys both land as rows. Every verify runs under the folder's one auth-throttle owner keyed by dialect and presented key, every reject lands on the folder's one reject stream, and the fold rides its span — inbound-attack telemetry is structural, not optional. Both folder-wide planes live here: `Curb` is the store-backed brute-force budget every credential-verify ceremony draws its policy row from, and `Reject` is the folder-wide authenticity ledger over ONE closed `kind` discriminant and bounded dialect/surface/reason facets — `mark` counts a refusal, `admit` counts the same kind's success, and `measured` is the ceremony aspect that times the wall span and admits on the success arm — so refusals, their denominator, and their latency are three `Convention`-named series joined on one key, every ratio the plane exists to answer is queryable, and a per-page counter name has no spelling anywhere in the folder. `VerifyFault` instantiates the folder fault shape over one core `Fault.Class.family` mint, folding a `crypt/sign` primitive fault to a caller-caused `malformed` at this seam so a bad presented signature is never a 500. Timestamp participation, candidate rotation, and the signed prefix are row grammar; tolerance, keys, and freshness are fold parameters a row cannot weaken, so admitting a dialect is review-free on the security axis. `Intake` is the typed `HttpApiMiddleware` spelling of the held-octets seam the runtime serve wave mounts.

## [01]-[INDEX]

- [02]-[VERIFY_FAULT]: the folder fault shape and the `crypt/sign` re-spell; `VerifyFault`.
- [03]-[DIALECT_TABLE]: the signing-convention rows and their header parse folds; `Verify`.
- [04]-[VERIFY_FOLD]: the throttled constant-time verify pipeline, `Verified` receipt, key registry, intake middleware; `Verify`, `Intake`.
- [05]-[ADMISSION_LEDGER]: the folder-wide authenticity ledger: refusal counter, admission twin, ceremony histogram, kind discriminant, bounded facets; `Reject`.
- [06]-[THROTTLE]: the folder-wide auth-throttle owner: per-surface budget rows, the store-backed token-bucket guard; `Curb`.

## [02]-[VERIFY_FAULT]

[VERIFY_FAULT]:
- Law: a crypto-primitive fault is re-spelled at this seam — a `SignFault` from a malformed presented signature folds to `malformed` (caller-caused), never escapes as a `defect`; a genuine key or algorithm defect on Rasm's side stays a fold-internal `defect`.
- Law: verification is result-typed — a valid signature lands the `Verified` receipt, a failed one a typed fault; there is no boolean-plus-throw and a `false` compare is `mismatch`, never a thrown value.
- Growth: a new failure mode is one family row carrying its core kind.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`Fault.Class`); `crypt/sign` (`SignFault`).

```typescript
import { RateLimiter } from "@effect/experimental"
import { HttpApiMiddleware } from "@effect/platform"
import {
  decodeIEEEP1363ECDSASignature, decodePKIXECDSAPublicKey, decodePKIXECDSASignature, decodeSEC1PublicKey,
  p256, p384, p521, verifyECDSASignature,
} from "@oslojs/crypto/ecdsa"
import { decodePKCS1RSAPublicKey, decodePKIXRSAPublicKey, sha256ObjectIdentifier, verifyRSASSAPKCS1v15Signature, verifyRSASSAPSSSignature } from "@oslojs/crypto/rsa"
import { SHA256, sha256 } from "@oslojs/crypto/sha2"
import { decodeBase64, decodeHex, encodeHexLowerCase } from "@oslojs/encoding"
import { Convention, Fault } from "@rasm/ts/core"
import { Array, Config, Context, Data, DateTime, Duration, Effect, Either, Metric, Number, Option, Predicate, Record, Redacted, Schema, pipe } from "effect"
import { Crypto, Probe, SignFault } from "./sign.ts"

const _family = Fault.Class.family(["missing", "malformed", "mismatch", "stale", "unknownKey", "throttled"] as const, {
  missing: { class: "malformed" },
  malformed: { class: "malformed" },
  mismatch: { class: "denied" },
  stale: { class: "expired" },
  unknownKey: { class: "denied" },
  throttled: { class: "exhausted" },
})

declare namespace VerifyFault {
  type Reason = (typeof _family.reasons)[number]
}

class VerifyFault extends Schema.TaggedError<VerifyFault>()("VerifyFault", {
  reason: _family.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<verify:${this.reason}> ${this.detail}`
  }
}

const _respell = (fault: SignFault): VerifyFault => new VerifyFault({ reason: "malformed", detail: fault.detail })
```

## [03]-[DIALECT_TABLE]

[DIALECT_TABLE]:
- Owner: `_dialects` — one row per inbound signing convention, each carrying `header` (the signature header, lowercase), `scheme` (`"hmac"` symmetric or `"ecdsa"`/`"rsa-pkcs1"`/`"rsa-pss"` asymmetric), `sigForm` on the ECDSA rows (`"pkix"` DER or `"p1363"` raw `r‖s`), `parse` (header value to the candidate signature set with the optional epoch-second stamp — `Option`-total, so any grammar refusal is one `malformed`), and `prefix` (the bytes prepended to the payload before signing — the `${t}.` stripe frame, empty elsewhere). Its rows: `github` (`sha256=<hex>`, HMAC), `stripe` (`t=<epoch>,v1=<hex>` rotation candidates, HMAC), `hmacHex`/`hmacBase64` (bare digests, HMAC), `ecdsaPkix`/`ecdsaP1363` (`kid=<id>,sig=<base64>` ECDSA in either signature encoding), `rsaPss`/`rsaPkcs1` (`kid=<id>,sig=<base64>` RSA), `attestation` (raw base64 signature over the attestation object, ECDSA PKIX).
- Law: the candidate set is non-empty by parse — a row returning zero marks is a parse refusal, so the verify fold never runs an empty compare loop and "no signature" is `missing`/`malformed`, never a vacuous pass; a base64 decode refusal is `Option.none`, never an empty-array sentinel, so decode failure and an empty candidate set never conflate; stripe's every `v1` candidate is tried, so key-rotation windows verify.
- Law: rows are grammar, never trust policy — tolerance, secrets, and keys are verify-fold parameters a row cannot weaken; an asymmetric row resolves its registry key by the presented `kid`, or by the dialect name when the row carries none (`attestation`), and the scheme and `sigForm` select the oslo decode and verify primitives.
- Law: the `_kinds` tuple anchors the key set — the `Verified.dialect` wire literal spreads it, and the guard pair closes tuple and table against each other in both directions, so a row without its tuple entry (or the converse) fails at the declaration.
- Growth: a new provider is one row and its tuple entry; a provider changing grammar is a row edit every intake inherits; a new asymmetric suite (Ed25519 when a partner signs with it) is one row over the existing key-registry resolution.
- Packages: `@oslojs/crypto` (`decodeSEC1PublicKey`/`decodePKIXECDSAPublicKey`, `decodePKIXECDSASignature`/`decodeIEEEP1363ECDSASignature`, `decodePKCS1RSAPublicKey`/`decodePKIXRSAPublicKey`, curve/OID rows); `@oslojs/encoding` (base64/hex decode).

```typescript
const _kinds = ["github", "stripe", "hmacHex", "hmacBase64", "ecdsaPkix", "ecdsaP1363", "rsaPss", "rsaPkcs1", "attestation"] as const

const _utf8 = new TextEncoder()
const _EMPTY = new Uint8Array(0)

declare namespace Verify {
  type Dialect = keyof typeof _dialects
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
  ecdsaPkix: { header: "x-signature-ecdsa", scheme: "ecdsa", sigForm: "pkix", parse: _keyed, prefix: () => _EMPTY },
  ecdsaP1363: { header: "x-signature-ecdsa-p1363", scheme: "ecdsa", sigForm: "p1363", parse: _keyed, prefix: () => _EMPTY },
  rsaPss: { header: "x-signature-rsa", scheme: "rsa-pss", parse: _keyed, prefix: () => _EMPTY },
  rsaPkcs1: { header: "x-signature-rsa", scheme: "rsa-pkcs1", parse: _keyed, prefix: () => _EMPTY },
  attestation: {
    header: "x-attestation-signature", scheme: "ecdsa", sigForm: "pkix",
    parse: (value: string) => Option.flatMap(_base64Hex(value), (hex) => _marked([hex], Option.none(), Option.none())),
    prefix: () => _EMPTY,
  },
} as const satisfies Record<string, {
  readonly header: string
  readonly scheme: Verify.Scheme
  readonly sigForm?: Verify.SigForm
  readonly parse: (value: string) => Option.Option<Verify.Parsed>
  readonly prefix: (stamp: Option.Option<number>) => Uint8Array
}>
```

## [04]-[VERIFY_FOLD]

[VERIFY_FOLD]:
- Owner: `Verify` — the assembled owner: `verify` runs a dialect over held octets against a resolved key into a `Verified` receipt under the per-key rate budget, and `PublicKeyStore` is the `Context.Tag` registry the asymmetric dialects resolve a partner or attestation public key from by `kid`. `PublicKey` is the tagged key family — `Ecdsa` carries `bytes`, the pinned `curve`, and the `encoding` axis (`sec1` raw point or `pkix` SPKI-DER), `Rsa` carries `bytes` and its `pkcs1`/`pkix` encoding — and `$match` drives the asymmetric dispatch, so a scheme/key family mismatch is the residue arm, never an if-ladder. `Intake` is the `HttpApiMiddleware` Tag the runtime serve wave implements over the raw request octets before any body parse.
- Law: the compare runs over the exact admitted bytes — the payload is the held request octets, the prefix rides the row, and freshness is checked before the signature (a stale stamp short-circuits to `stale` under the caller's tolerance `Duration`), so a replay outside the window never reaches the compare.
- Law: every verify is throttled — the fold body runs under the `Curb` `verify` row keyed `<dialect>:<kid|dialect>`, an exhausted budget folds to `throttled` at the guard, and the store-backed limiter holds the budget across every app sharing the library; every fault lands `Reject.mark("verify", { dialect, reason })` and every admitted signature lands its `verify`-kinded twin and wall span through `Reject.measured`, so the dialect's reject ratio is queryable rather than inferred from traffic.
- Law: every asymmetric candidate resolves its key first — the registry key is the presented `kid` or the dialect name for a kid-less row, and a miss is `unknownKey`, never a silent skip; the ECDSA arm decodes SEC1 or PKIX keys over the `p256`/`p384`/`p521` roster the registry pins per key and PKIX-DER or IEEE-P1363 signatures per the row's `sigForm`; the RSA arm decodes PKCS1/PKIX keys and checks RSASSA-PKCS1-v1_5 or PSS with the SHA-256 OID; the oslo decoders throw on malformed DER, so the per-candidate verify runs inside `Either.try` and a candidate whose signature refuses to decode verifies `false` — a structurally garbage presented signature is `mismatch`, never a defect.
- Law: the HMAC fold tries every candidate under one `Crypto.matches` `Mac` probe and folds a primitive throw to `malformed`; a non-empty candidate set that matches none is `mismatch`.
- Receipt: `Verified` — the `dialect`, the resolved `kid` when asymmetric, and the verified octet length, so the admitting edge enqueues exactly what was verified; `verify` returns `Verified` or a `VerifyFault`, never a boolean.
- Growth: a new scheme is one `$match` arm; a new key roster is a registry row; the HMAC path never changes when an asymmetric row lands.
- Boundary: the edge holds the octets and lifts the header/signature into the fold; `crypt/sign` owns the HMAC compare and the SHA-256 primitive; the registry is satisfied by a config-sourced or fetched key set the composition root wires; `Curb` owns the budget row; the runtime serve wave implements `Intake` and mounts it on ingress routes.
- Packages: `@oslojs/crypto` (verify + decode primitives, curve roster); `crypt/sign` (`Crypto.matches`); `@effect/platform` (`HttpApiMiddleware`); `effect` (`Context`, `Schema`, `Effect`, `Metric`).

```typescript
type MacKey = Redacted.Redacted<Uint8Array>

type PublicKey = Data.TaggedEnum<{
  Ecdsa: { readonly bytes: Uint8Array; readonly curve: "p256" | "p384" | "p521"; readonly encoding: "sec1" | "pkix" }
  Rsa: { readonly bytes: Uint8Array; readonly encoding: "pkcs1" | "pkix" }
}>

const PublicKey = Data.taggedEnum<PublicKey>()

class Verified extends Schema.Class<Verified>("Verified")({
  dialect: Schema.Literal(..._kinds),
  kid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  length: Schema.NonNegativeInt,
}) {}

class PublicKeyStore extends Context.Tag("security/crypt/PublicKeyStore")<PublicKeyStore, {
  readonly byKid: (kid: string) => Effect.Effect<Option.Option<PublicKey>, VerifyFault>
}>() {}

class Intake extends HttpApiMiddleware.Tag<Intake>()("security/crypt/Intake", {
  failure: VerifyFault,
}) {}

const _CURVES = { p256, p384, p521 } as const

const _sigForm = (row: (typeof _dialects)[Verify.Dialect]): Verify.SigForm =>
  Predicate.hasProperty(row, "sigForm") ? row.sigForm : "pkix"

const _fresh = (stamp: Option.Option<number>, tolerance: Duration.Duration): Effect.Effect<void, VerifyFault> =>
  Option.match(stamp, {
    onNone: () => Effect.void,
    onSome: (epoch) =>
      Effect.flatMap(DateTime.now, (now) =>
        Duration.lessThanOrEqualTo(Duration.millis(Math.abs(DateTime.toEpochMillis(now) - epoch * 1000)), tolerance)
          ? Effect.void
          : Effect.fail(new VerifyFault({ reason: "stale", detail: String(epoch) }))),
  })

const _verifyAsym = (scheme: Verify.Scheme, sigForm: Verify.SigForm, key: PublicKey, digest: Uint8Array, mark: string): boolean =>
  Either.getOrElse(
    Either.try(() => {
      const sig = decodeHex(mark)
      return PublicKey.$match(key, {
        Ecdsa: ({ bytes, curve, encoding }) =>
          scheme === "ecdsa"
          && verifyECDSASignature(
            encoding === "sec1" ? decodeSEC1PublicKey(_CURVES[curve], bytes) : decodePKIXECDSAPublicKey(bytes, [_CURVES[curve]]),
            digest,
            sigForm === "p1363" ? decodeIEEEP1363ECDSASignature(_CURVES[curve], sig) : decodePKIXECDSASignature(sig),
          ),
        Rsa: ({ bytes, encoding }) =>
          (scheme === "rsa-pss" || scheme === "rsa-pkcs1")
          && (scheme === "rsa-pss"
            ? verifyRSASSAPSSSignature(encoding === "pkcs1" ? decodePKCS1RSAPublicKey(bytes) : decodePKIXRSAPublicKey(bytes), SHA256, SHA256, 32, digest, sig)
            : verifyRSASSAPKCS1v15Signature(encoding === "pkcs1" ? decodePKCS1RSAPublicKey(bytes) : decodePKIXRSAPublicKey(bytes), sha256ObjectIdentifier, digest, sig)),
      })
    }),
    () => false,
  )

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
          onNone: () => Effect.fail(new VerifyFault({ reason: "missing", detail: row.header })),
          onSome: Effect.succeed,
        })
        const parsed = yield* Option.match(row.parse(raw), {
          onNone: () => Effect.fail(new VerifyFault({ reason: "malformed", detail: dialect })),
          onSome: Effect.succeed,
        })
        yield* _fresh(parsed.stamp, tolerance)
        const keyId = Option.getOrElse(parsed.kid, () => dialect)
        const payload = new Uint8Array([...row.prefix(parsed.stamp), ...octets])
        const matched = yield* curb.guard("verify", `${dialect}:${keyId}`, (detail) => new VerifyFault({ reason: "throttled", detail }))(
          row.scheme === "hmac"
            ? Effect.flatMap(
                Option.match(mac, { onNone: () => Effect.fail(new VerifyFault({ reason: "malformed", detail: "hmac key absent" })), onSome: Effect.succeed }),
                (key) => Effect.map(
                  Effect.forEach(parsed.marks, (mark) => cipher.matches(Probe.Mac({ key, body: payload, signature: mark })).pipe(Effect.mapError(_respell))),
                  (results) => Array.contains(results, true),
                ),
              )
            : Effect.gen(function* () {
                const key = yield* Effect.flatMap(keys.byKid(keyId), Option.match({
                  onNone: () => Effect.fail(new VerifyFault({ reason: "unknownKey", detail: keyId })),
                  onSome: Effect.succeed,
                }))
                const digest = sha256(payload)
                return Array.some(parsed.marks, (mark) => _verifyAsym(row.scheme, _sigForm(row), key, digest, mark))
              }),
        )
        return matched
          ? new Verified({ dialect, kid: parsed.kid, length: octets.byteLength })
          : yield* Effect.fail(new VerifyFault({ reason: "mismatch", detail: dialect }))
      }).pipe(
        Effect.tapError((fault) => Reject.mark("verify", { dialect, reason: fault.reason })),
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
- Owner: `Reject` — the folder's one authenticity ledger over the closed `_REJECTS` kind table (`bearer` a presented bearer, `ceremony` a webauthn challenge, `clone` a webauthn counter regression, `credential` an otp/recovery/apikey presentation, `csrf` a double-submit pair, `reuse` a presented rotated refresh, `state` an oauth ceremony state, `verify` an external signature) and three folds over it: `mark(kind, facet?)` increments the `securityRejects` refusal counter, `admit(kind, facet?)` its `securityAdmitted` twin, and `measured(kind, facet?)` is the ceremony aspect timing the wall span onto the `securityCeremony` distribution and admitting on the success arm — so one composed line at a ceremony entrypoint yields the refusal, the denominator, and the latency under one `Convention.rasm.securityKind` key.
- Law: admission rides the SAME kind its refusal rides, so every rate, burn, and ratio is a same-key join — a credential-stuffing spike separates from a traffic spike because both series move under one tag set; a surface with no refusal row therefore has no admission row, and a breach-class kind (`clone`, `reuse` on a replay) is read absolutely, its enclosing ceremony's kind carrying the denominator.
- Law: instrument metadata derives from the Convention row — name, description, dimension roster, and the UCUM unit column have one spelling on the core vocabulary spine, so the two admission rows reuse this table's kind vocabulary verbatim and a page-local counter name is the parallel this owner deletes.
- Law: facets are bounded tag values and two of the three axes carry that bound in their own types — `dialect` is the `[03]` dialect key space and `surface` the `_SURFACES` credential roster this owner anchors, so a mistyped facet is a compile error rather than a second series; `reason` stays a string because every family that marks one declares its `Reason` above this stratum, and its bound is the caller passing that closed literal, never a member assembling text. An identifier-grade value (a subject, a kid, a session id) rides log annotations and span attributes, never a facet; one `_tagged` fold applies the kind and every present facet, so the three members cannot drift in their tagging.
- Law: ledger writes are evidence, never control flow — every member is total and composes through `Effect.tapError`, `Effect.tap`, or the pipeline seam beside the verdict it witnesses, so a metric failure can never alter a security verdict and `measured` never widens the fault channel it wraps.
- Entry: `Reject.measured("verify", { dialect })` in this page's fold; the authn pages compose one `measured` line per ceremony entrypoint — `bearer` at the guard, `reuse` at refresh, `csrf` at the double-submit check, `credential` at otp/recovery/api-key resolve, `state` at the oauth callback, `ceremony` at both passkey finishes — beside the `mark` line their refusal arms already carry.
- Growth: a new ceremony surface is one `_REJECTS` row reaching all three folds at once; a new credential surface is one `_SURFACES` entry; a new facet axis is one `_FACETS` row with its Convention tag key and its `Facet` field, the `_Facets` guard closing the two against each other.
- Packages: `effect` (`Metric`, `Record`, `Array`, `Duration`); `@rasm/ts/core` (`Convention`).

```typescript
const _REJECTS = {
  bearer: {},
  ceremony: {},
  clone: {},
  credential: {},
  csrf: {},
  reuse: {},
  state: {},
  verify: {},
} as const

// The credential-surface roster lives beside the kind table because the ledger is what bounds it: every
// `credential`-kinded mark and admission across the folder tags one of these, so an unrostered spelling is a
// compile error at the call site rather than a silent second series under a mistyped name.
const _SURFACES = ["apikey", "otp", "recovery", "workload"] as const

const _FACETS = {
  dialect: Convention.rasm.securityDialect,
  reason: Convention.rasm.securityReason,
  surface: Convention.rasm.securitySurface,
} as const

declare namespace Reject {
  type Kind = keyof typeof _REJECTS
  type Surface = (typeof _SURFACES)[number]
  // Two of the three axes close at their own anchors; `reason` cannot, because every folder fault family that
  // marks one sits at or above this stratum and no union reaches down here — its boundedness is the caller
  // passing its own closed `Reason` literal, which is why no member ever builds a reason string.
  type Facet = {
    readonly dialect?: Verify.Dialect
    readonly reason?: string
    readonly surface?: Surface
  }
  type _Keys<K extends Kind = keyof typeof _REJECTS> = K
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
  mark: (kind: Reject.Kind, facet: Reject.Facet = {}): Effect.Effect<void> =>
    Metric.increment(_tagged(_rejects, kind, facet)),
  admit: (kind: Reject.Kind, facet: Reject.Facet = {}): Effect.Effect<void> =>
    Metric.increment(_tagged(_admitted, kind, facet)),
  // The ceremony aspect: one line at an entrypoint buys the wall span AND the denominator, so no arm can land the
  // latency without its admission or the admission without its latency.
  measured:
    (kind: Reject.Kind, facet: Reject.Facet = {}) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      self.pipe(
        Metric.trackDuration(_tagged(_ceremony, kind, facet)),
        Effect.tap(() => Reject.admit(kind, facet)),
      ),
} as const
```

## [06]-[THROTTLE]

[THROTTLE]:
- Owner: `Curb` — the folder's one auth-throttle posture: `_CURB` is the per-surface budget table resolved as one described record at the boot line, and `guard(surface, key, exhausted)` runs a ceremony body under that surface's store-backed token bucket, folding `RateLimitExceeded` and `RateLimitStoreError` onto the caller's own fault family exactly once. Session refresh, OTP verify, recovery redeem, api-key resolve, webauthn assert-finish, and this page's signature verify each key one row — a new throttled ceremony is a row and a `guard` line, never a sixth hand-wiring.
- Law: auth throttling REFUSES — `onExceeded: "fail"` on every row, so an exhausted budget is a typed `throttled` verdict the caller's family classes `exhausted`; the branch's delaying postures stay distinct owners, and each ceremony keeps its own `Reject` mark and `measured` denominator beside the guard, so the collapse erases no counting.
- Law: the budget key is `<surface>:<caller key>` — the caller supplies the amortizing index its own page law names (a subject, a prefix, a sid, a dialect-kid pair), so one store-backed limiter bounds a guessing campaign across every app sharing the library under one key grammar.
- Growth: a new throttled ceremony is one `_curbed` entry with its `_budget` row and one `guard` composition at its entrypoint.
- Boundary: the `RateLimiter` store is a data-wave-satisfied Layer; each ceremony supplies its own `throttled` constructor, so no fault family crosses this seam.
- Packages: `@effect/experimental` (`RateLimiter.makeWithRateLimiter`); `effect` (`Config`, `Duration`).

```typescript
const _curbed = ["apikey", "otp", "recovery", "refresh", "verify", "webauthn"] as const

declare namespace Curb {
  type Surface = (typeof _curbed)[number]
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
    // The exhausted fold happens ONCE here: each ceremony hands its own `throttled` constructor, so the store
    // fault and the budget refusal collapse onto the caller's fault family without a per-page catchTags pair.
    const guard = <E>(surface: Curb.Surface, key: string, exhausted: (detail: string) => E) =>
      <A, R>(body: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
        limit({ algorithm: "token-bucket", onExceeded: "fail", window: rows[surface].window, limit: rows[surface].limit, key: `${surface}:${key}` })(body).pipe(
          Effect.catchTags({
            RateLimitExceeded: () => Effect.fail(exhausted(key)),
            RateLimitStoreError: (error) => Effect.fail(exhausted(String(error))),
          }))
    return { guard } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Curb, Intake, PublicKey, PublicKeyStore, Reject, Verified, Verify, VerifyFault }
export type { MacKey }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
