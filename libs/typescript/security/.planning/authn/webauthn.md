# [SECURITY_WEBAUTHN]

Both halves of the passkey ceremony as two per-runtime subpath modules: the RP-side verifier over `@simplewebauthn/server` (node `./server`) mints ceremony options and verifies the signed response into a typed verdict, and the browser-safe invocation over `@simplewebauthn/browser` (`./browser`) wraps `navigator.credentials` into an `Effect` gated on a capability probe — the exports map keeps the node verifier physically unreachable from browser resolution. One options→verify pattern spans registration and authentication; the attestation-format dispatch is internal, parameterized by policy, never a hand switch. Ceremony position is type-witnessed data: `CeremonyPhase` is a `Schema.Class` carrying the intent (`enroll`/`assert`), the subject, the challenge, and its expiry, sealed into the `ChallengeStore` single-use port under the CHALLENGE as its key and consumed INSIDE verification through `expectedChallenge` — a subject, intent, freshness, or presence miss is a typed `challenge` fault, so an enroll challenge never completes an assert, a finish naming no live challenge spends nothing and evicts no one, and protocol order is enforced by data, not convention. Policy is pinned, not defaulted: the COSE allow-list is the `[-8, -7]` (EdDSA, ES256) pair narrowing the `Jwt` four-algorithm pin to what platform authenticators mint, `authenticatorSelection` demands discoverable credentials and user verification as config rows, and the challenge mints through `crypt/sign`'s `Crypto.token` byte overload, so one RNG owner and one base64url spelling serve the folder. Attestation trust pins per FORMAT because the verifier reads it per format: `SettingsService` holds one anchor row per chain-bearing format and a separate `mds` row for the blob signer, and `enrollFinish` refuses a chain-demanding posture no anchor covers before projecting `getStatement(aaguid)` onto the `Passkey` as the authenticator `model` — anchors read on the verify path, not merely initialized beside it; those anchors are process-wide simplewebauthn singletons, so one attestation policy governs a process and a divergent-policy tenant is a deployment split, never a Layer split. A non-increasing-counter check is the clone/replay defense and it is loud — the `clone` row lands on the folder reject stream and the error log lands before the `breached`-class fault surfaces — every consumed-challenge refusal lands the `ceremony` row beside it, and `assertFinish` runs under the folder `Curb` `webauthn` row keyed by subject. A successful assertion establishes a session through `authn/session`; the verdict is a discriminated rail, never a boolean-plus-throw.

## [01]-[INDEX]

- [02]-[ATTESTATION_TRUST]: `Passkey`, `WebAuthnFault`, `CeremonyPhase`, `WebAuthnTrust`; `./server` node.
- [03]-[RP_VERIFICATION]: `WebAuthn`, `WebAuthnStore`, `ChallengeStore`; `./server` node.
- [04]-[BROWSER_CEREMONY]: `Passkeys`, `PasskeyFault`; `./browser`.

## [02]-[ATTESTATION_TRUST]

[ATTESTATION_TRUST]:
- Owner: `Passkey` is the stored credential (id, subject, public key, counter, transports, and the MDS-projected authenticator `model`), `WebAuthnFault` the folder fault shape closed at the core family seam, `CeremonyPhase` the type-witnessed protocol position, `WebAuthnTrust` the folder's one `WEBAUTHN_` decode site — a described record resolving the per-format trust anchors, RP identity, the authenticator-type preference, and the ceremony TTL at layer construction — writing one `SettingsService` anchor row per stated format and initializing `MetadataService` from the FIDO MDS. `WebAuthnStore` holds credentials, `ChallengeStore` the single-use phase.
- Law: a trust anchor is keyed by the FORMAT whose chain walks it, because that is how the verifier reads it — `SettingsService.getRootCertificates({ identifier: fmt })` runs off the DECODED attestation format, so the `mds` row pins the metadata BLOB signer and pins nothing any attestation chain validates against; `_ROOTS` states one env row per chain-bearing format and closes itself against the package's own `RootCertIdentifier`, an empty row is an unstated anchor rather than an instruction to clear the anchors the package ships, and `enrollFinish` reads the verified `fmt` back against the same registry so a chain-demanding posture over an unpinned format refuses as `attestation`. Pinning nowhere and demanding a chain anyway is the shape this law deletes: the path validator treats an empty anchor set as a skip, so the refusal has to be spelled here.
- Law: attestation policy is a config row — `none` accepts any authenticator while `direct`/`enterprise` demand a validated chain, and MDS initializes with a `strict`/`permissive` unregistered-AAGUID policy once at layer construction, so the attestation type is a policy value the verify legs read, never a per-ceremony switch; the simplewebauthn trust services are process-global, so exactly one attestation policy exists per process — the folder law a multi-policy deployment answers with separate workloads.
- Law: `CeremonyPhase` is the transition payload and the CHALLENGE is its store key — start seals `{ intent, subject, challenge, expiresAt }` under the ceremony TTL against the challenge it just minted, and finish consumes that key single-use, so `*Finish` before `*Start`, cross-subject completion, cross-ceremony completion, and challenge replay are all unspellable at the store contract. Keying by subject instead let a caller who knew only a subject name spend that subject's live phase, so the phase carries its subject as a field the finish gate reads and the key stays the one value an attacker cannot guess; the satisfying layer is `crypt/sign`'s `SingleUse.persisted` row over this port's Tag.
- Law: one alphabet spans the whole WebAuthn wire — the verified `WebAuthnCredential.publicKey` bytes render base64url-noPadding exactly as the credential id, the challenge, and every response coordinate do, so `Passkey.publicKey` admits through `Schema.Uint8ArrayFromBase64Url` and a journalled row round-trips back into verification under one encoding; a standard-base64 field here is the folder's lone exception to its own stated base64url law and decodes a `+`/`/` payload the authenticator never emitted.
- Growth: a new authenticator vendor is one `_ROOTS` value under the format its chain already uses; a format the package adds is one `_ROOTS` row the `RootCertIdentifier` guard demands; a new attestation posture is one config row; a cross-restart multi-factor enrollment flow is an `@effect/experimental` `Machine.makeSerializable` actor whose snapshot rides the same single-use store.
- Boundary: `@simplewebauthn/server` dispatches the format verifier internally; the browser half collects the response; `authn/session` establishes the session; the trust anchors are config/fetch-sourced at boot.

```typescript
import {
  generateAuthenticationOptions, generateRegistrationOptions, MetadataService, SettingsService,
  verifyAuthenticationResponse, verifyRegistrationResponse,
  type AttestationFormat, type AuthenticationResponseJSON, type AuthenticatorSelectionCriteria, type AuthenticatorTransportFuture,
  type PublicKeyCredentialCreationOptionsJSON, type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON,
  type RootCertIdentifier, type VerifiedRegistrationResponse, type WebAuthnCredential,
} from "@simplewebauthn/server"
import { Fault } from "@rasm/core"
import { Array, Config, Context, DateTime, Duration, Effect, Layer, Option, Record, Redacted, Ref, Runtime, Schema } from "effect"
import { SecurityFact, Witness } from "../access/audit.ts"
import { Crypto, type SingleUse } from "../crypt/sign.ts"
import { Curb, Reject } from "../crypt/verify.ts"
import { CredentialRef, type SessionFault, type Subject, Token, type TokenPair } from "./session.ts"

// `Schema.Literal` takes values, so the transport roster is spelled once as a tuple — and the guard pair closes it
// against the package's own `AuthenticatorTransportFuture` in BOTH directions, so a minor line adding a transport
// fails this declaration instead of refusing a legitimate enrollment at the `Passkey` schema.
const _transports = ["ble", "cable", "hybrid", "internal", "nfc", "smart-card", "usb"] as const

type _Transports<T extends AuthenticatorTransportFuture = (typeof _transports)[number]> = T
type _Future<T extends (typeof _transports)[number] = AuthenticatorTransportFuture> = T

// Anchors resolve by DECODED attestation format, so `mds` — pinning only the metadata BLOB signer — sits as one
// row beside the chain-bearing formats rather than standing in for them. `none` is absent because its statement
// carries no chain, and `RootCertIdentifier` bounds the roster against the package in both directions. The tuple
// anchors the key set so the attestation refusal names a pinned identifier rather than a free string.
const _anchors = ["android-key", "android-safetynet", "apple", "fido-u2f", "mds", "packed", "tpm"] as const

type _Anchors<T extends Exclude<RootCertIdentifier, "none"> = (typeof _anchors)[number]> = T
type _Pinned<T extends (typeof _anchors)[number] = Exclude<RootCertIdentifier, "none">> = T

const _ROOTS = {
  "android-key": "WEBAUTHN_ROOTS_ANDROID_KEY",
  "android-safetynet": "WEBAUTHN_ROOTS_ANDROID_SAFETYNET",
  apple: "WEBAUTHN_ROOTS_APPLE",
  "fido-u2f": "WEBAUTHN_ROOTS_FIDO_U2F",
  mds: "WEBAUTHN_ROOTS_MDS",
  packed: "WEBAUTHN_ROOTS_PACKED",
  tpm: "WEBAUTHN_ROOTS_TPM",
} as const satisfies Record.ReadonlyRecord<(typeof _anchors)[number], string>

const _Intent = Schema.Literal("enroll", "assert")

// Ceremony refusal is a CLOSED verdict, never a sentence: the gate asks four questions of the held phase and the
// first unmet one IS the answer, so an operator reads which check fired instead of re-parsing prose.
const _refusals = ["absent", "subject", "intent", "expired"] as const

// Six legs partition the server ceremony and each reason renders its OWN subject: a mint refusal names the stage
// that broke, a gate refusal names the check that fired, a verify refusal names the intent and the credential when
// one resolved, a replay refusal names both counters, and an anchor refusal names the identifier whose registry
// answered nothing. One free `detail` string answered all six, and the counter arm in particular carried the passkey
// id while saying nothing about the regression that condemned it.
const _family = Fault.Class.family(["ceremony", "challenge", "verification", "counter", "attestation", "throttled"] as const, {
  ceremony: Fault.Class.row({
    class: "defect",
    leg: "mint",
    detail: Schema.Struct({ stage: Schema.Literal("challenge", "options"), cause: Schema.String }),
    render: ({ cause, stage }) => `ceremony ${stage} could not be built: ${cause}`,
  }),
  challenge: Fault.Class.row({
    class: "malformed",
    leg: "gate",
    detail: Schema.Struct({ subject: Schema.UUID, intent: _Intent, refusal: Schema.Literal(..._refusals) }),
    render: ({ intent, refusal, subject }) => `${intent} ceremony for ${subject} refused: ${refusal}`,
  }),
  verification: Fault.Class.row({
    class: "denied",
    leg: "verify",
    detail: Schema.Struct({
      intent: _Intent,
      // Registration reaches its refusals before any credential resolves, so the column is absence-shaped rather
      // than a placeholder id no authenticator ever presented.
      credential: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
      cause: Schema.String,
    }),
    render: ({ cause, credential, intent }) =>
      `${intent} response refused${Option.getOrElse(Option.map(credential, (id) => ` on credential ${id}`), () => "")}: ${cause}`,
  }),
  counter: Fault.Class.row({
    class: "breached",
    leg: "replay",
    detail: Schema.Struct({
      subject: Schema.UUID,
      passkey: Schema.NonEmptyString,
      held: Schema.Number,
      presented: Schema.Number,
    }),
    render: ({ held, passkey, presented, subject }) =>
      `passkey ${passkey} for ${subject} presented counter ${presented} against held ${held}`,
  }),
  attestation: Fault.Class.row({
    class: "denied",
    leg: "anchor",
    detail: Schema.Struct({ identifier: Schema.Literal(..._anchors), cause: Schema.String }),
    render: ({ cause, identifier }) => `${identifier} attestation refused: ${cause}`,
  }),
  throttled: Fault.Class.row({
    class: "exhausted",
    leg: "throttle",
    detail: Schema.Struct({ subject: Schema.UUID, cause: Schema.String }),
    render: ({ cause, subject }) => `webauthn budget spent for ${subject}: ${cause}`,
  }),
})

declare namespace WebAuthnFault {
  type Case = typeof _family.payload.Type
  type Intent = typeof _Intent.Type
  type Reason = (typeof _family.kinds)[number]
  type Refusal = (typeof _refusals)[number]
}

class Passkey extends Schema.Class<Passkey>("Passkey")({
  id: Schema.NonEmptyString,
  subject: Schema.UUID,
  publicKey: Schema.Uint8ArrayFromBase64Url,
  counter: Schema.Number,
  aaguid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  model: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  transports: Schema.optionalWith(Schema.Array(Schema.Literal(..._transports)), { as: "Option" }),
}) {}

// Phase carries its own subject because the store key is the CHALLENGE, not the subject: no stranger evicts a
// slot only the holder of a live challenge can name, and subject then rides the value, where the finish gate
// reads it back against the caller claiming it.
class CeremonyPhase extends Schema.Class<CeremonyPhase>("CeremonyPhase")({
  intent: _Intent,
  subject: Schema.UUID,
  challenge: Schema.NonEmptyString,
  expiresAt: Schema.DateTimeUtc,
}) {}

class WebAuthnFault extends Schema.TaggedError<WebAuthnFault>()("WebAuthnFault", {
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

class WebAuthnStore extends Context.Tag("security/authn/WebAuthnStore")<WebAuthnStore, {
  readonly insert: (passkey: Passkey) => Effect.Effect<void, WebAuthnFault>
  readonly byId: (id: string) => Effect.Effect<Option.Option<Passkey>, WebAuthnFault>
  readonly bySubject: (subject: Subject["id"]) => Effect.Effect<ReadonlyArray<Passkey>, WebAuthnFault>
  readonly updateCounter: (id: string, counter: number) => Effect.Effect<void, WebAuthnFault>
}>() {}

class ChallengeStore extends Context.Tag("security/authn/ChallengeStore")<ChallengeStore, SingleUse<CeremonyPhase, WebAuthnFault>>() {}

// The folder's one WEBAUTHN_ decode site: every namespace row resolves in this record at layer construction, so a
// malformed environment fails the boot line and no second decode site can fork the namespace.
const _setting = Config.unwrap({
  attestationType: Config.literal("none", "direct", "enterprise")("WEBAUTHN_ATTESTATION").pipe(
    Config.withDefault("none" as const),
    Config.withDescription("attestation posture; direct/enterprise arm MDS and demand a validated cert chain"),
  ),
  // One PEM list per anchor row, each under its own name, because the verifier looks anchors up by format: a
  // single flat list would land under one identifier and leave every other format's chain walking nothing.
  roots: Config.all(Record.map(_ROOTS, (name) =>
    Config.array(Config.string(), name).pipe(
      Config.withDefault(Array.empty<string>()),
      Config.withDescription("PEM trust anchors this format's certificate path validates against"),
    ))),
  mode: Config.literal("strict", "permissive")("WEBAUTHN_MDS_MODE").pipe(
    Config.withDefault("permissive" as const),
    Config.withDescription("unregistered-AAGUID policy the MDS initialization pins"),
  ),
  residentKey: Config.literal("required", "preferred", "discouraged")("WEBAUTHN_RESIDENT_KEY").pipe(
    Config.withDefault("required" as const),
    Config.withDescription("discoverable-credential demand on the options mint"),
  ),
  userVerification: Config.literal("required", "preferred", "discouraged")("WEBAUTHN_USER_VERIFICATION").pipe(
    Config.withDefault("required" as const),
    Config.withDescription("user-verification demand on the options mint"),
  ),
  preferred: Config.option(Config.literal("securityKey", "localDevice", "remoteDevice")("WEBAUTHN_PREFERRED_AUTHENTICATOR").pipe(
    Config.withDescription("authenticator-type hint the registration options carry; absent states no preference"),
  )),
  rpID: Config.string("WEBAUTHN_RP_ID").pipe(Config.withDescription("relying-party id every ceremony verifies against")),
  rpName: Config.string("WEBAUTHN_RP_NAME").pipe(Config.withDescription("relying-party display name on the registration options")),
  origin: Config.string("WEBAUTHN_ORIGIN").pipe(Config.withDescription("expected web origin every response verifies against")),
  ceremonyTtl: Config.duration("WEBAUTHN_CEREMONY_TTL").pipe(
    Config.withDefault(Duration.minutes(5)),
    Config.withDescription("one window for the authenticator dialog and the stashed phase"),
  ),
})

class WebAuthnTrust extends Context.Tag("security/authn/WebAuthnTrust")<WebAuthnTrust, {
  readonly attestationType: "none" | "direct" | "enterprise"
  readonly selection: AuthenticatorSelectionCriteria
  readonly preferred: Option.Option<"securityKey" | "localDevice" | "remoteDevice">
  readonly rpID: string
  readonly rpName: string
  readonly origin: string
  readonly ceremonyTtl: Duration.Duration
}>() {
  static readonly Live: Layer.Layer<WebAuthnTrust> = Layer.effect(
    WebAuthnTrust,
    Effect.gen(function* () {
      const setting = yield* _setting
      // Each stated row REPLACES that format's anchor list, so only a non-empty one is written: handing `[]` to
      // `setRootCertificates` deletes the anchors the package ships for `apple`, `android-key`, and
      // `android-safetynet`, and the path validator reads an empty set as permission to skip validation.
      yield* Effect.forEach(
        Array.filter(Record.toEntries(setting.roots), ([, certificates]) => Array.isNonEmptyReadonlyArray(certificates)),
        ([identifier, certificates]) => Effect.sync(() => SettingsService.setRootCertificates({ identifier, certificates })),
        { discard: true },
      )
      yield* Effect.when(
        Effect.tryPromise({
          try: () => MetadataService.initialize({ verificationMode: setting.mode }),
          catch: (cause) => new WebAuthnFault({ case: { reason: "attestation", identifier: "mds", cause: String(cause) } }),
        }).pipe(Effect.orDie),
        () => setting.attestationType !== "none",
      )
      return {
        attestationType: setting.attestationType,
        selection: { residentKey: setting.residentKey, userVerification: setting.userVerification },
        preferred: setting.preferred,
        rpID: setting.rpID, rpName: setting.rpName, origin: setting.origin,
        ceremonyTtl: setting.ceremonyTtl,
      }
    }),
  )
}
```

## [03]-[RP_VERIFICATION]

[RP_VERIFICATION]:
- Owner: `WebAuthn.enrollStart`/`enrollFinish` register a passkey, `WebAuthn.assertStart`/`assertFinish` authenticate one. Its `verified` discriminant is matched so the credential is extracted only on the true arm, `newCounter` is the replay defense, and `enrollFinish` gates the verified `fmt` against its pinned anchor before enriching the stored `Passkey` with the MDS `getStatement` projection.
- Law: the challenge is minted server-side through `Crypto.token`'s byte overload — one RNG owner and one base64url renderer across the folder, 32 bytes over the spec's 16-byte floor — and sealed as a `CeremonyPhase` under the ceremony TTL. It is consumed INSIDE verification, never ahead of it: `expectedChallenge` takes a resolver the package calls only once the response has parsed and its clientDataJSON has yielded a challenge, and the challenge that response carried IS the store key, so consumption and verification settle together and a finish that names no live challenge spends nothing. Consuming a subject-keyed slot ahead of the parse made an unsigned, hand-built finish a denial of service on the user it named — that response carried no credential at all, only the victim's subject. Every ceremony refusal — a missing phase, a subject mismatch, an intent mismatch, a stale phase — lands `Reject.mark("ceremony")` beside a `Ceremony` fact through `Witness`, so challenge replay counts with the same weight as the oauth state replay and rides the audit rail as receipt-truth; the resolved passkey belongs to the ceremony's subject, so one subject's challenge can never complete against another subject's credential.
- Law: policy is pinned at the options mint — `supportedAlgorithmIDs` spreads the `_ALGORITHMS` `[-8, -7]` pair on both registration and verification so an algorithm-confusion downgrade is unspellable, `authenticatorSelection` carries the trust row's discoverable-credential and user-verification demands, and the `preferred` trust row spreads `preferredAuthenticatorType` onto the registration mint only when a deployment states one — absent is no preference, never a defaulted steer; the caller never writes the format switch — attestation dispatches inside the verifier keyed by the decoded `fmt`, parameterized by `WebAuthnTrust.attestationType` and the format's own pinned anchors.
- Law: the passkey COSE pair is a deliberate narrowing of the folder's JWT pin, not a copy of it — `Jwt` admits four algorithms because it verifies tokens from issuers it does not control, while this plane pins only the two a platform authenticator mints, so `[-8, -7]` (EdDSA, ES256) is a strict subset and `RS256`/`ES384` are refused at the mint rather than tolerated at the verify. Spreading the JWT roster here admits the RSA key an authenticator falls back to when it holds nothing better, which is the downgrade this pin exists to refuse; the package's own default is wider still and never applies.
- Law: a chain-demanding posture reads its anchor before it trusts a chain — the path validator returns TRUE on an empty anchor list, so `enrollFinish` reads `SettingsService.getRootCertificates` for the verified `fmt` and refuses an unpinned chain-bearing format as `attestation`; a `none` statement carries no chain and is judged by the posture itself.
- Law: one TTL governs the whole ceremony window — both option bags carry `timeout: Duration.toMillis(ceremonyTtl)` off the same resolved config value the `CeremonyPhase` expiry reads, so the authenticator dialog and the stashed server phase close together; leaving the library's 60-second default in place moves one half of one window on a config change and fails a ninety-second user at the authenticator while a live phase still sits in the store.
- Law: both finishes carry the ceremony denominator — `enrollFinish` and `assertFinish` compose `Reject.measured("ceremony")` under the same kind their challenge refusals mark, so the passkey plane publishes an assertion success rate and a ceremony wall span beside its refusals; the `clone` breach row deliberately has no admission twin, because a counter regression is read absolutely and its denominator is the enclosing assertion's own kind.
- Law: a non-increasing counter is a cloned authenticator — `Reject.mark("clone")` lands on the folder reject stream, the `breached`-class `Clone` fact publishes through `Witness`, and the error log lands with the passkey annotation before the `counter` fault (class `breached`) surfaces; a `newCounter` of zero from a fresh authenticator is admitted only when the stored counter is also zero; `assertFinish` runs under the `Curb` `webauthn` row keyed by subject and an exhausted budget folds to `throttled` at the guard.
- Receipt: `Passkey` on registration, `TokenPair` on assertion — never a raw `VerifiedRegistrationResponse` past the seam.
- Growth: a new transport hint arrives as a package union widening and lands as one `_transports` entry the guard pair already demands; a new ceremony option is one options-bag field.
- Boundary: `WebAuthnTrust` supplies the resolved `WEBAUTHN_` policy record; the browser half collects the response; `authn/session` `Token.establish` mints the session; the ports carry state; `crypt/verify`'s `Curb` owns the assertion budget row.
- Packages: `@simplewebauthn/server` (the 2×2 ceremony, `MetadataService.getStatement`, `preferredAuthenticatorType`); `crypt/sign` (`Crypto.token`); `crypt/verify` (`Reject`, `Curb`); `access/audit` (`Witness`, `SecurityFact`); `authn/session` (`Token.establish`, `CredentialRef`).

```typescript
// Strict subset of the `Jwt` four-algorithm pin: two COSE identifiers a platform authenticator mints, so an RSA
// fallback key never enters the ceremony.
const _ALGORITHMS: ReadonlyArray<number> = [-8, -7]

// WebAuthn wants at least sixteen random bytes behind a challenge; thirty-two is the deployed norm and renders
// forty-three base64url characters, matching every other value on this wire.
const _CHALLENGE_BYTES = 32
const _utf8 = new TextEncoder()

class WebAuthn extends Effect.Service<WebAuthn>()("security/authn/WebAuthn", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* WebAuthnStore
    const challenges = yield* ChallengeStore
    const trust = yield* WebAuthnTrust
    const token = yield* Token
    const curb = yield* Curb
    const { ceremonyTtl, origin, rpID, rpName } = trust
    // Package's challenge check is a callback wanting a `Promise`, so the gate below needs the CURRENT fiber's
    // runtime rather than the default one — a test clock, a stubbed store, and every fiber ref this ceremony
    // runs under reach inside the resolver instead of stopping at its edge.
    const runPromise = Runtime.runPromise(yield* Effect.runtime<never>())
    // Stash key is the CHALLENGE itself. Keying by subject let anyone naming a subject spend that subject's live
    // phase, which turned a well-formed forgery — an id, a type, and a hand-built clientDataJSON, none of it
    // signed — into a denial of service on the ceremony its victim was mid-way through. No stranger reaches a
    // slot only the holder of the minted challenge can spell.
    const _stash = (subject: string, intent: WebAuthnFault.Intent, challenge: string): Effect.Effect<void, WebAuthnFault> =>
      Effect.flatMap(DateTime.now, (now) =>
        challenges.stash(challenge, new CeremonyPhase({ intent, subject, challenge, expiresAt: DateTime.addDuration(now, ceremonyTtl) }), ceremonyTtl))
    // The gate's four questions answer in ONE word and the issue mints once around whichever fired, so the refusal
    // an operator reads is a rostered verdict rather than four sentences no consumer can discriminate on.
    const _refused = (now: DateTime.Utc, subject: string, intent: WebAuthnFault.Intent) =>
      (held: Option.Option<CeremonyPhase>): Option.Option<WebAuthnFault.Case> =>
        Option.map(
          Option.match(held, {
            onNone: () => Option.some<WebAuthnFault.Refusal>("absent"),
            onSome: (phase) =>
              phase.subject !== subject
                ? Option.some<WebAuthnFault.Refusal>("subject")
                : phase.intent !== intent
                  ? Option.some<WebAuthnFault.Refusal>("intent")
                  : DateTime.lessThan(phase.expiresAt, now)
                    ? Option.some<WebAuthnFault.Refusal>("expired")
                    : Option.none<WebAuthnFault.Refusal>(),
          }),
          (refusal): WebAuthnFault.Case => ({ reason: "challenge", subject, intent, refusal }),
        )
    // ONE ceremony gate for both finishes, and it consumes the phase INSIDE verification: `expectedChallenge`
    // takes a resolver the package calls only after the response's id, type, and clientDataJSON have parsed, and
    // it hands over the challenge THAT response carried — which is the key, so a finish naming no live challenge
    // consumes nothing and the ceremony its victim is still walking survives it. The resolver's own contract is a
    // bare boolean, so the refusal REASON rides a cell `settled` reads back: a phase the resolver refused settles
    // `challenge` with that reason, a response that never reached the resolver settles `verification`, and the
    // ceremony mark and its `Ceremony` fact land on the first arm alone. The cell carries the ISSUE, not a sentence,
    // so a store refusal on consume arrives under its own reason instead of being relabelled as a gate verdict.
    const _gate = (subject: string, intent: WebAuthnFault.Intent) =>
      Effect.map(Ref.make(Option.none<WebAuthnFault.Case>()), (cell) => ({
        expectedChallenge: (presented: string): Promise<boolean> =>
          runPromise(
            Effect.flatMap(
              Effect.all([DateTime.now, challenges.consume(presented)]).pipe(
                Effect.map(([now, held]) => _refused(now, subject, intent)(held)),
                Effect.catchAll((fault: WebAuthnFault) => Effect.succeed(Option.some(fault.case))),
              ),
              (refusal) => Effect.as(Ref.set(cell, refusal), Option.isNone(refusal)),
            )),
        settled: <A>(self: Effect.Effect<A, WebAuthnFault>): Effect.Effect<A, WebAuthnFault> =>
          Effect.catchAll(self, (fault) =>
            Effect.flatMap(Ref.get(cell), Option.match({
              onNone: () => Effect.fail(fault),
              onSome: (held) =>
                Reject.mark("ceremony").pipe(
                  Effect.zipRight(Witness.publish(SecurityFact.Ceremony({ subject, intent }))),
                  Effect.zipRight(Effect.fail(new WebAuthnFault({ case: held }))),
                ),
            }))),
      }))
    // `validateCertificatePath` returns TRUE on an empty anchor list — no anchors, so no path to validate — which
    // makes a `direct` posture over an unpinned format a chain checked against nothing. The verified format is
    // read back against the very registry the verifier consulted, so the refusal is spelled where the gap is.
    const _anchored = (fmt: AttestationFormat): Effect.Effect<void, WebAuthnFault> =>
      trust.attestationType === "none" || fmt === "none"
        || Array.isNonEmptyReadonlyArray(SettingsService.getRootCertificates({ identifier: fmt }))
        ? Effect.void
        : Effect.fail(new WebAuthnFault({ case: { reason: "attestation", identifier: fmt, cause: "chain has no pinned root" } }))
    const _challenge = cipher.token(_CHALLENGE_BYTES).pipe(
      Effect.mapBoth({
        onFailure: (cause) => new WebAuthnFault({ case: { reason: "ceremony", stage: "challenge", cause: cause.message } }),
        onSuccess: Redacted.value,
      }))
    const enrollStart = (subject: Subject["id"], userName: string): Effect.Effect<PublicKeyCredentialCreationOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const existing = yield* store.bySubject(subject)
        const challenge = yield* _challenge
        const options = yield* Effect.tryPromise({
          try: () => generateRegistrationOptions({
            rpName, rpID, userName, challenge, userID: _utf8.encode(subject),
            attestationType: trust.attestationType, authenticatorSelection: trust.selection,
            ...(Option.isSome(trust.preferred) && { preferredAuthenticatorType: trust.preferred.value }),
            supportedAlgorithmIDs: [..._ALGORITHMS],
            timeout: Duration.toMillis(ceremonyTtl), // one config value drives both halves of the window; the library default would expire the dialog four minutes before the phase
            excludeCredentials: existing.map((passkey) => ({ id: passkey.id })),
          }),
          catch: (cause) => new WebAuthnFault({ case: { reason: "ceremony", stage: "options", cause: String(cause) } }),
        })
        yield* _stash(subject, "enroll", options.challenge)
        return options
      }).pipe(Effect.withSpan("security.webauthn.enrollStart"))
    const enrollFinish = (subject: Subject["id"], response: RegistrationResponseJSON): Effect.Effect<Passkey, WebAuthnFault> =>
      Effect.gen(function* () {
        const gate = yield* _gate(subject, "enroll")
        const verified = yield* Effect.tryPromise({
          try: () => verifyRegistrationResponse({
            response, expectedChallenge: gate.expectedChallenge, expectedOrigin: origin, expectedRPID: rpID,
            requireUserVerification: true, supportedAlgorithmIDs: [..._ALGORITHMS],
          }),
          catch: (cause) =>
            new WebAuthnFault({ case: { reason: "verification", intent: "enroll", credential: Option.none(), cause: String(cause) } }),
        }).pipe(Effect.filterOrFail(
          (outcome): outcome is Extract<VerifiedRegistrationResponse, { verified: true }> => outcome.verified,
          () =>
            new WebAuthnFault({
              case: { reason: "verification", intent: "enroll", credential: Option.none(), cause: "attestation statement did not verify" },
            }),
        ), gate.settled)
        yield* _anchored(verified.registrationInfo.fmt)
        const statement = yield* trust.attestationType === "none"
          ? Effect.succeedNone
          : Effect.tryPromise({
              try: () => MetadataService.getStatement(verified.registrationInfo.aaguid),
              catch: (cause) => new WebAuthnFault({ case: { reason: "attestation", identifier: "mds", cause: String(cause) } }),
            }).pipe(Effect.orElseSucceed(() => undefined), Effect.map(Option.fromNullable))
        const passkey = new Passkey({
          id: verified.registrationInfo.credential.id, subject, publicKey: verified.registrationInfo.credential.publicKey,
          counter: verified.registrationInfo.credential.counter, aaguid: Option.some(verified.registrationInfo.aaguid),
          model: Option.flatMap(statement, (held) => Option.fromNullable(held.description)),
          transports: Option.fromNullable(verified.registrationInfo.credential.transports),
        })
        yield* store.insert(passkey)
        return passkey
      }).pipe(Reject.measured("ceremony"), Effect.withSpan("security.webauthn.enrollFinish"))
    const assertStart = (subject: Subject["id"]): Effect.Effect<PublicKeyCredentialRequestOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const passkeys = yield* store.bySubject(subject)
        const challenge = yield* _challenge
        const options = yield* Effect.tryPromise({
          try: () => generateAuthenticationOptions({
            rpID, challenge, allowCredentials: passkeys.map((passkey) => ({ id: passkey.id })),
            userVerification: "required", timeout: Duration.toMillis(ceremonyTtl),
          }),
          catch: (cause) => new WebAuthnFault({ case: { reason: "ceremony", stage: "options", cause: String(cause) } }),
        })
        yield* _stash(subject, "assert", options.challenge)
        return options
      }).pipe(Effect.withSpan("security.webauthn.assertStart"))
    const assertFinish = (subject: Subject["id"], response: AuthenticationResponseJSON): Effect.Effect<TokenPair, WebAuthnFault | SessionFault> =>
      curb.guard("webauthn", subject, (cause: string): WebAuthnFault | SessionFault => new WebAuthnFault({ case: { reason: "throttled", subject, cause } }))(
        Effect.gen(function* () {
          const passkey = yield* Effect.flatMap(store.byId(response.id), Option.match({
            onNone: () =>
              Effect.fail(
                new WebAuthnFault({
                  case: { reason: "verification", intent: "assert", credential: Option.some(response.id), cause: "no passkey holds this credential" },
                }),
              ),
            onSome: Effect.succeed,
          })).pipe(Effect.filterOrFail(
            (held) => held.subject === subject,
            () =>
              new WebAuthnFault({
                case: { reason: "verification", intent: "assert", credential: Option.some(response.id), cause: "credential is enrolled to another subject" },
              }),
          ))
          const gate = yield* _gate(subject, "assert")
          const credential: WebAuthnCredential = {
            id: passkey.id, publicKey: passkey.publicKey, counter: passkey.counter,
            ...(Option.isSome(passkey.transports) && { transports: [...passkey.transports.value] }),
          }
          const verified = yield* Effect.tryPromise({
            try: () => verifyAuthenticationResponse({ response, credential, expectedChallenge: gate.expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
            catch: (cause) =>
              new WebAuthnFault({
                case: { reason: "verification", intent: "assert", credential: Option.some(passkey.id), cause: String(cause) },
              }),
          }).pipe(Effect.filterOrFail(
            (outcome) => outcome.verified,
            () =>
              new WebAuthnFault({
                case: { reason: "verification", intent: "assert", credential: Option.some(passkey.id), cause: "assertion signature did not verify" },
              }),
          ), gate.settled)
          const next = verified.authenticationInfo.newCounter
          yield* next > passkey.counter || (next === 0 && passkey.counter === 0)
            ? Effect.void
            : Reject.mark("clone").pipe(
                Effect.zipRight(Witness.publish(SecurityFact.Clone({ subject, passkey: passkey.id }))),
                Effect.zipRight(Effect.logError("webauthn counter regression — cloned authenticator")),
                Effect.annotateLogs("passkey", passkey.id),
                Effect.zipRight(
                  Effect.fail(
                    new WebAuthnFault({
                      case: { reason: "counter", subject, passkey: passkey.id, held: passkey.counter, presented: next },
                    }),
                  ),
                ),
              )
          yield* store.updateCounter(passkey.id, next)
          return yield* token.establish(new CredentialRef({ kind: "webauthn", key: passkey.id }), ["openid"], { tenant: Option.none(), verified: true })
        }),
      ).pipe(
        Reject.measured("ceremony"),
        Effect.withSpan("security.webauthn.assertFinish"),
      )
    return { enrollStart, enrollFinish, assertStart, assertFinish } as const
  }),
  dependencies: [Crypto.Default, Curb.Default, Token.Default, WebAuthnTrust.Live],
  accessors: true,
}) {}
```

## [04]-[BROWSER_CEREMONY]

[BROWSER_CEREMONY]:
- Law: the reason vocabulary is closed and the package's own codes map onto it, never through it — `_CODES` is a `Record<WebAuthnErrorCode, PasskeyFault.Reason>` whose stated annotation demands the package's whole twelve-code union, so a minor line adding a code breaks the mapping at compile time; the codes collapse to what a consumer can act on (`aborted` a user cancel or superseded ceremony, `origin` an RP-id or domain misconfiguration, `options` an RP-built options defect, `authenticator` a device refusal, `enrolled` a credential this authenticator already holds, `passthrough` the package deferring to `cause`) beside the two folder-local rows the capability gates raise before any call — `unsupported` for a browser or a conditional-UI mode that is not there, `anchorless` for a page that armed autofill without the field it prompts into. A free-string `code` field carries no class, forces every consumer to re-parse the package's spelling, and is the shape this family deletes.
- Law: every gate a ceremony depends on is read HERE, as a stated row, and the first unmet row is the refusal — the package raises its own pre-flight checks as a bare `Error` rather than a `WebAuthnError`, so routing them through the code map lands a browser gap and a page-authoring mistake alike as an unactionable `defect`. `browserSupportsWebAuthn` gates every entry, `autofill` adds the conditional-UI probe and the `<input[autocomplete$='webauthn']>` anchor the package itself matches on, and `verifyBrowserAutofillInput` still pins TRUE behind them as the second line, so a library default flip cannot drop the check either.
- Law: a capability probe is a QUESTION, and a browser that throws while answering it has answered no — every probe lifts through `Effect.tryPromise` and folds its rejection to `false`, so an absent capability is a value the gate reads rather than a defect that escapes the ceremony's own fault channel; a bare `Effect.promise` here turns a rejecting `isConditionalMediationAvailable` into an unhandled defect on a page whose every other call is typed.
- Law: `WebAuthnAbortService` enforces the single-live-ceremony law — each ceremony auto-arms a fresh `AbortSignal` and a new call cancels the prior, and `Passkeys.cancel` fires on a client-route change; the v13 `{ optionsJSON }` object form is the only call shape, never the pre-12 positional form; `register` carries the `useAutoRegister` conversion affordance so a just-signed-in password upgrades to a passkey without a second ceremony surface.
- Law: the browser never verifies — it invokes the authenticator and returns the response JSON; a `Schema` per JSON shape decodes both the inbound options and the outbound response at the fetch seam the ui folder owns; conditional-UI autofill (`useBrowserAutofill: true`) is a browser-only affordance the ui edge mounts on a login field.
- Receipt: the `RegistrationResponseJSON`/`AuthenticationResponseJSON` the caller POSTs back to `WebAuthn.*Finish`; the browser collects the signed response, never a verdict.
- Growth: a new probe (`platformAuthenticatorIsAvailable` variants) is one `_probed` row; a new ceremony affordance is one options field; a new gate is one `_Gate` value in the entry's own list; a new package code is one `_CODES` cell, and a genuinely new refusal meaning is one family row.

```typescript
import {
  browserSupportsWebAuthn, browserSupportsWebAuthnAutofill, platformAuthenticatorIsAvailable, startAuthentication,
  startRegistration, WebAuthnAbortService, WebAuthnError,
  type AuthenticationResponseJSON, type PublicKeyCredentialCreationOptionsJSON, type PublicKeyCredentialRequestOptionsJSON,
  type RegistrationResponseJSON, type WebAuthnErrorCode,
} from "@simplewebauthn/browser"
import { Fault } from "@rasm/core"
import { Array, Effect, Option, Schema } from "effect"

// Two legs partition this half and each reason renders its OWN subject: `ceremony` reasons carry the package's own
// message for the call that refused, and `capability` reasons carry the closed thing that was missing — the browser
// feature this page probed for, or the selector it prompts into. A shared free-string `detail` spelled both as prose
// a caller had to read rather than as a value it could branch on.
const _CAPABILITIES = ["webauthn", "conditional-ui"] as const

const _family = Fault.Class.family(
  ["aborted", "origin", "options", "authenticator", "enrolled", "passthrough", "unsupported", "anchorless"] as const,
  {
    aborted: Fault.Class.row({
      class: "denied",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `ceremony aborted: ${cause}`,
    }),
    origin: Fault.Class.row({
      class: "defect",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `rp id or domain rejected the ceremony: ${cause}`,
    }),
    options: Fault.Class.row({
      class: "defect",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `rp built unusable ceremony options: ${cause}`,
    }),
    authenticator: Fault.Class.row({
      class: "denied",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `authenticator refused the ceremony: ${cause}`,
    }),
    enrolled: Fault.Class.row({
      class: "conflicted",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `authenticator already holds a credential for this account: ${cause}`,
    }),
    passthrough: Fault.Class.row({
      class: "defect",
      leg: "ceremony",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `ceremony failed below the package: ${cause}`,
    }),
    unsupported: Fault.Class.row({
      class: "absent",
      leg: "capability",
      detail: Schema.Struct({ capability: Schema.Literal(..._CAPABILITIES) }),
      render: ({ capability }) => `this browser has no ${capability}`,
    }),
    // Conditional-UI prompt has nowhere to land: this page armed autofill without the field it anchors to, an
    // absent affordance a caller fixes by mounting one, never a device or a browser refusal.
    anchorless: Fault.Class.row({
      class: "absent",
      leg: "capability",
      detail: Schema.Struct({ selector: Schema.NonEmptyString }),
      render: ({ selector }) => `autofill armed with no ${selector} on the page`,
    }),
  },
)

// The package's own error-code union governs this record, so a minor line adding a code breaks the mapping loudly at
// compile time rather than landing as an unmapped free string on a fault a caller must parse.
const _CODES: Record<WebAuthnErrorCode, PasskeyFault.Reason> = {
  ERROR_CEREMONY_ABORTED: "aborted",
  ERROR_INVALID_DOMAIN: "origin",
  ERROR_INVALID_RP_ID: "origin",
  ERROR_INVALID_USER_ID_LENGTH: "options",
  ERROR_MALFORMED_PUBKEYCREDPARAMS: "options",
  ERROR_AUTHENTICATOR_NO_SUPPORTED_PUBKEYCREDPARAMS_ALG: "options",
  ERROR_AUTHENTICATOR_GENERAL_ERROR: "authenticator",
  ERROR_AUTHENTICATOR_MISSING_DISCOVERABLE_CREDENTIAL_SUPPORT: "authenticator",
  ERROR_AUTHENTICATOR_MISSING_USER_VERIFICATION_SUPPORT: "authenticator",
  ERROR_AUTO_REGISTER_USER_VERIFICATION_FAILURE: "authenticator",
  ERROR_AUTHENTICATOR_PREVIOUSLY_REGISTERED: "enrolled",
  ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY: "passthrough",
}

declare namespace PasskeyFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class PasskeyFault extends Schema.TaggedError<PasskeyFault>()("PasskeyFault", {
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

// Probes answer a capability question, so a rejection IS the negative answer: `Effect.merge` folds the refusal
// arm back into the value and each gate reads a boolean, where `Effect.promise` escalates that same rejection
// into a defect on a page whose every other call carries a typed reason.
const _probed = (ask: () => Promise<boolean>): Effect.Effect<boolean> =>
  Effect.merge(Effect.tryPromise({ try: ask, catch: (): false => false }))

// Selector the package itself matches before arming conditional UI; reading it here keeps the refusal typed
// rather than letting the package raise a bare `Error` the code map holds no cell for.
const _ANCHOR = "input[autocomplete$='webauthn']"

type _Gate = { readonly met: boolean; readonly case: PasskeyFault.Case }

const _supported = (): _Gate => ({ met: browserSupportsWebAuthn(), case: { reason: "unsupported", capability: "webauthn" } })

// Gates are stated per entry and the FIRST unmet row is the refusal, so an unsupported browser, a browser without
// conditional UI, and a page missing its autofill field answer as three acts a caller can take.
const _lift = <A>(gates: ReadonlyArray<_Gate>, run: () => Promise<A>): Effect.Effect<A, PasskeyFault> =>
  Option.match(Array.findFirst(gates, (gate) => !gate.met), {
    onNone: () =>
      Effect.tryPromise({
        try: run,
        catch: (cause) =>
          cause instanceof WebAuthnError
            ? new PasskeyFault({ case: { reason: _CODES[cause.code], cause: cause.message } })
            : new PasskeyFault({ case: { reason: "passthrough", cause: String(cause) } }),
      }),
    onSome: ({ case: held }) => Effect.fail(new PasskeyFault({ case: held })),
  })

const Passkeys = {
  register: (optionsJSON: PublicKeyCredentialCreationOptionsJSON, options?: { readonly autoRegister?: boolean }): Effect.Effect<RegistrationResponseJSON, PasskeyFault> =>
    _lift([_supported()], () => startRegistration({ optionsJSON, ...(options?.autoRegister === true && { useAutoRegister: true }) })),
  authenticate: (optionsJSON: PublicKeyCredentialRequestOptionsJSON): Effect.Effect<AuthenticationResponseJSON, PasskeyFault> =>
    _lift([_supported()], () => startAuthentication({ optionsJSON })),
  autofill: (optionsJSON: PublicKeyCredentialRequestOptionsJSON): Effect.Effect<AuthenticationResponseJSON, PasskeyFault> =>
    Effect.flatMap(_probed(browserSupportsWebAuthnAutofill), (ready) =>
      _lift(
        [
          _supported(),
          { met: ready, case: { reason: "unsupported", capability: "conditional-ui" } },
          { met: document.querySelectorAll(_ANCHOR).length > 0, case: { reason: "anchorless", selector: _ANCHOR } },
        ],
        // `verifyBrowserAutofillInput` stays pinned TRUE behind the anchor gate, so a library default flip drops
        // neither check and the prompt never arms over a field that is not there.
        () => startAuthentication({ optionsJSON, useBrowserAutofill: true, verifyBrowserAutofillInput: true }),
      )),
  probe: (): Effect.Effect<{ readonly platform: boolean; readonly autofill: boolean }> =>
    Effect.all({ platform: _probed(platformAuthenticatorIsAvailable), autofill: _probed(browserSupportsWebAuthnAutofill) }),
  cancel: (): Effect.Effect<void> => Effect.sync(() => WebAuthnAbortService.cancelCeremony()),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CeremonyPhase, ChallengeStore, Passkey, Passkeys, PasskeyFault, WebAuthn, WebAuthnFault, WebAuthnStore, WebAuthnTrust }
```

## [05]-[RESEARCH]

(none)
