# [SECURITY_WEBAUTHN]

Both halves of the passkey ceremony as two per-runtime subpath modules: the RP-side verifier over `@simplewebauthn/server` (node `./server`) mints ceremony options and verifies the signed response into a typed verdict, and the browser-safe invocation over `@simplewebauthn/browser` (`./browser`) wraps `navigator.credentials` into an `Effect` gated on a capability probe — the exports map keeps the node verifier physically unreachable from browser resolution. One options→verify pattern spans registration and authentication; the attestation-format dispatch is internal, parameterized by policy, never a hand switch. Ceremony position is type-witnessed data: `CeremonyPhase` is a `Schema.Class` carrying the intent (`enroll`/`assert`), the challenge, and its expiry, sealed into the `ChallengeStore` single-use port at start and consumed exactly once at finish — an intent mismatch, a stale phase, and a missing phase are each a typed `challenge` fault, so an enroll challenge can never complete an assert and the protocol order is enforced by data, not by convention. Policy is pinned, not defaulted: the COSE allow-list is the `[-8, -7]` (EdDSA, ES256) roster mirroring the `Jwt` `algorithms` pin, `authenticatorSelection` demands discoverable credentials and user verification as config rows, and the challenge mints through `crypt/sign`'s `Crypto.token` so the folder holds one RNG owner. Attestation trust is exploited end to end: `SettingsService` pins the root certificates, `MetadataService` loads the FIDO MDS blob, and `enrollFinish` projects `getStatement(aaguid)` onto the `Passkey` as the authenticator `model` — trust anchors that are read, not merely initialized; the trust anchors are process-wide simplewebauthn singletons, so one attestation policy governs a process and a divergent-policy tenant is a deployment split, never a Layer split. A non-increasing-counter check is the clone/replay defense and it is loud — the `clone` row lands on the folder reject stream and the error log lands before the `breached`-class fault surfaces — every consumed-challenge refusal lands the `ceremony` row beside it, and `assertFinish` runs under a per-subject store-backed `RateLimiter`. A successful assertion establishes a session through `authn/session`; the verdict is a discriminated rail, never a boolean-plus-throw.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [PUBLIC]                                                     | [RUNTIME]       |
| :-----: | :------------------ | :----------------------------------------------------------- | :-------------- |
|  [01]   | `ATTESTATION_TRUST` | `Passkey`, `WebAuthnFault`, `CeremonyPhase`, `WebAuthnTrust` | `./server` node |
|  [02]   | `RP_VERIFICATION`   | `WebAuthn`, `WebAuthnStore`, `ChallengeStore`                | `./server` node |
|  [03]   | `BROWSER_CEREMONY`  | `Passkeys`, `PasskeyFault`                                   | `./browser`     |

## [02]-[ATTESTATION_TRUST]

[ATTESTATION_TRUST]:
- Owner: `Passkey` is the stored credential (id, subject, public key, counter, transports, and the MDS-projected authenticator `model`), `WebAuthnFault` the folder fault shape with the guard pair closed, `CeremonyPhase` the type-witnessed protocol position, `WebAuthnTrust` the trust-anchor Layer that configures `SettingsService` root certificates, initializes `MetadataService` from the FIDO MDS, and carries the pinned ceremony policy rows. `WebAuthnStore` holds credentials, `ChallengeStore` the single-use phase.
- Law: attestation policy is a config row — `none` accepts any authenticator, `direct`/`enterprise` demand a validated cert chain; `WebAuthnTrust` sets the per-format root certs (`SettingsService.setRootCertificates`) and initializes MDS with a `strict`/`permissive` unregistered-AAGUID policy once at layer construction, so the format verifier validates provenance and the attestation type is a policy value the verify legs read, never a per-ceremony switch; the simplewebauthn trust services are process-global, so exactly one attestation policy exists per process — the folder law a multi-policy deployment answers with separate workloads.
- Law: `CeremonyPhase` is the transition payload — start seals `{ intent, challenge, expiresAt }` under the ceremony TTL, finish consumes it single-use and gates intent and freshness, so `*Finish` before `*Start`, cross-ceremony completion, and challenge replay are all unspellable at the store contract; the satisfying layer is a `Cache`/`PersistedCache` row over the `SingleUse` contract.
- Law: passkey material is public-key crypto — the credential and challenge are typed boundary values, not `Redacted` secrets; the fault rows carry the core `FaultClass` kind so status and blame derive from the branch table.
- Growth: a new authenticator vendor is one root-cert entry with its MDS metadata; a new attestation posture is one config row; a cross-restart multi-factor enrollment flow is an `@effect/experimental` `Machine.makeSerializable` actor whose snapshot rides the same single-use store.
- Boundary: `@simplewebauthn/server` dispatches the format verifier internally; the browser half collects the response; `authn/session` establishes the session; the trust anchors are config/fetch-sourced at boot.
- Packages: `@simplewebauthn/server` (`SettingsService`, `MetadataService`); `effect` (`Config`, `Context`, `Effect`, `Layer`, `Option`, `Schema`); `@rasm/ts/core` (`FaultClass`); `crypt/sign` (`SingleUse`); `authn/session` (`Subject`).

```typescript
import * as RateLimiter from "@effect/experimental/RateLimiter"
import {
  generateAuthenticationOptions, generateRegistrationOptions, MetadataService, SettingsService,
  verifyAuthenticationResponse, verifyRegistrationResponse,
  type AuthenticationResponseJSON, type AuthenticatorSelectionCriteria, type PublicKeyCredentialCreationOptionsJSON,
  type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON, type VerifiedRegistrationResponse, type WebAuthnCredential,
} from "@simplewebauthn/server"
import { FaultClass } from "@rasm/ts/core"
import { Config, Context, DateTime, Duration, Effect, Layer, Option, Redacted, Schema } from "effect"
import { Crypto, type SingleUse } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"
import { CredentialRef, type SessionFault, type Subject, Token, type TokenPair } from "./session.ts"

const _reasons = ["ceremony", "challenge", "verification", "counter", "attestation", "throttled"] as const
const _transports = ["ble", "cable", "hybrid", "internal", "nfc", "smart-card", "usb"] as const

const _faults = {
  ceremony: { class: "defect" },
  challenge: { class: "malformed" },
  verification: { class: "denied" },
  counter: { class: "breached" },
  attestation: { class: "denied" },
  throttled: { class: "exhausted" },
} as const

declare namespace WebAuthnFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class Passkey extends Schema.Class<Passkey>("Passkey")({
  id: Schema.NonEmptyString,
  subject: Schema.UUID,
  publicKey: Schema.Uint8ArrayFromBase64,
  counter: Schema.Number,
  aaguid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  model: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  transports: Schema.optionalWith(Schema.Array(Schema.Literal(..._transports)), { as: "Option" }),
}) {}

class CeremonyPhase extends Schema.Class<CeremonyPhase>("CeremonyPhase")({
  intent: Schema.Literal("enroll", "assert"),
  challenge: Schema.NonEmptyString,
  expiresAt: Schema.DateTimeUtc,
}) {}

class WebAuthnFault extends Schema.TaggedError<WebAuthnFault>()("WebAuthnFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<webauthn:${this.reason}> ${this.detail}`
  }
}

class WebAuthnStore extends Context.Tag("security/authn/WebAuthnStore")<WebAuthnStore, {
  readonly insert: (passkey: Passkey) => Effect.Effect<void, WebAuthnFault>
  readonly byId: (id: string) => Effect.Effect<Option.Option<Passkey>, WebAuthnFault>
  readonly bySubject: (subject: Subject["id"]) => Effect.Effect<ReadonlyArray<Passkey>, WebAuthnFault>
  readonly updateCounter: (id: string, counter: number) => Effect.Effect<void, WebAuthnFault>
}>() {}

class ChallengeStore extends Context.Tag("security/authn/ChallengeStore")<ChallengeStore, SingleUse<CeremonyPhase, WebAuthnFault>>() {}

class WebAuthnTrust extends Context.Tag("security/authn/WebAuthnTrust")<WebAuthnTrust, {
  readonly attestationType: "none" | "direct" | "enterprise"
  readonly selection: AuthenticatorSelectionCriteria
}>() {
  static readonly Live: Layer.Layer<WebAuthnTrust> = Layer.effect(
    WebAuthnTrust,
    Effect.gen(function* () {
      const attestationType = yield* Config.literal("none", "direct", "enterprise")("WEBAUTHN_ATTESTATION").pipe(Config.withDefault("none" as const))
      const roots = yield* Config.array(Config.string(), "WEBAUTHN_ROOT_CERTS").pipe(Config.withDefault([]))
      const mode = yield* Config.literal("strict", "permissive")("WEBAUTHN_MDS_MODE").pipe(Config.withDefault("permissive" as const))
      const residentKey = yield* Config.literal("required", "preferred", "discouraged")("WEBAUTHN_RESIDENT_KEY").pipe(Config.withDefault("required" as const))
      const userVerification = yield* Config.literal("required", "preferred", "discouraged")("WEBAUTHN_USER_VERIFICATION").pipe(Config.withDefault("required" as const))
      yield* attestationType === "none"
        ? Effect.void
        : Effect.gen(function* () {
            SettingsService.setRootCertificates({ identifier: "mds", certificates: [...roots] })
            yield* Effect.tryPromise({ try: () => MetadataService.initialize({ verificationMode: mode }), catch: (cause) => new WebAuthnFault({ reason: "attestation", detail: String(cause) }) }).pipe(Effect.orDie)
          })
      return { attestationType, selection: { residentKey, userVerification } }
    }),
  )
}
```

## [03]-[RP_VERIFICATION]

[RP_VERIFICATION]:
- Owner: `WebAuthn.enrollStart`/`enrollFinish` register a passkey, `WebAuthn.assertStart`/`assertFinish` authenticate one. Its `verified` discriminant is matched so the credential is extracted only on the true arm, `newCounter` is the replay defense, and `enrollFinish` enriches the stored `Passkey` with the MDS `getStatement` projection when an attestation policy is active.
- Law: the challenge is minted server-side through `Crypto.token` — one RNG owner across the folder — sealed as a `CeremonyPhase` under the ceremony TTL, and consumed single-use on the rail at finish with intent and freshness gated, every refusal landing `Reject.mark("ceremony")` so challenge replay is counted with the same weight as the oauth state replay; the response is `Schema`-decoded before verify; the resolved passkey belongs to the ceremony's subject — a cross-subject assertion is `verification`, so one subject's challenge can never complete against another subject's credential.
- Law: policy is pinned at the options mint — `supportedAlgorithmIDs` spreads the `_ALGORITHMS` `[-8, -7]` roster on both registration and verification so an algorithm-confusion downgrade is unspellable, and `authenticatorSelection` carries the trust row's discoverable-credential and user-verification demands; the caller never writes the format switch — attestation dispatches inside the verifier keyed by the decoded `fmt`, parameterized by `WebAuthnTrust.attestationType` and the pinned root certs.
- Law: a non-increasing counter is a cloned authenticator — `Reject.mark("clone")` lands on the folder reject stream and the error log lands with the passkey annotation before the `counter` fault (class `breached`) surfaces; a `newCounter` of zero from a fresh authenticator is admitted only when the stored counter is also zero; `assertFinish` runs under the per-subject token-bucket budget and an exhausted budget is `throttled`.
- Receipt: `Passkey` on registration, `TokenPair` on assertion — never a raw `VerifiedRegistrationResponse` past the seam.
- Growth: a new transport hint is one `_transports` entry; a new ceremony option is one options-bag field.
- Boundary: `WebAuthnTrust` supplies the attestation and selection policy; the browser half collects the response; `authn/session` `Token.establish` mints the session; the ports carry state; the `RateLimiter` store is data-wave-satisfied.
- Packages: `@simplewebauthn/server` (the 2×2 ceremony, `MetadataService.getStatement`); `@effect/experimental` (`RateLimiter`); `crypt/sign` (`Crypto.token`); `crypt/verify` (`Reject`); `authn/session` (`Token.establish`, `CredentialRef`).

```typescript
const _ALGORITHMS: ReadonlyArray<number> = [-8, -7]
const _CHALLENGE_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"
const _utf8 = new TextEncoder()

class WebAuthn extends Effect.Service<WebAuthn>()("security/authn/WebAuthn", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const store = yield* WebAuthnStore
    const challenges = yield* ChallengeStore
    const trust = yield* WebAuthnTrust
    const token = yield* Token
    const limit = yield* RateLimiter.makeWithRateLimiter
    const rpID = yield* Config.string("WEBAUTHN_RP_ID")
    const rpName = yield* Config.string("WEBAUTHN_RP_NAME")
    const origin = yield* Config.string("WEBAUTHN_ORIGIN")
    const ceremonyTtl = yield* Config.duration("WEBAUTHN_CEREMONY_TTL").pipe(Config.withDefault(Duration.minutes(5)))
    const window = yield* Config.duration("WEBAUTHN_RATE_WINDOW").pipe(Config.withDefault(Duration.minutes(5)))
    const budget = yield* Config.integer("WEBAUTHN_RATE_LIMIT").pipe(Config.withDefault(10))
    const _stash = (subject: string, intent: "enroll" | "assert", challenge: string): Effect.Effect<void, WebAuthnFault> =>
      Effect.flatMap(DateTime.now, (now) =>
        challenges.stash(subject, new CeremonyPhase({ intent, challenge, expiresAt: DateTime.addDuration(now, ceremonyTtl) }), ceremonyTtl))
    const _expected = (subject: string, intent: "enroll" | "assert"): Effect.Effect<string, WebAuthnFault> =>
      Effect.gen(function* () {
        const phase = yield* Effect.flatMap(challenges.consume(subject), Option.match({
          onNone: () => Effect.fail(new WebAuthnFault({ reason: "challenge", detail: subject })),
          onSome: Effect.succeed,
        }))
        const now = yield* DateTime.now
        return yield* Effect.succeed(phase).pipe(
          Effect.filterOrFail((held) => held.intent === intent, () => new WebAuthnFault({ reason: "challenge", detail: "intent mismatch" })),
          Effect.filterOrFail((held) => DateTime.lessThanOrEqualTo(now, held.expiresAt), () => new WebAuthnFault({ reason: "challenge", detail: "ceremony expired" })),
          Effect.map((held) => held.challenge),
        )
      }).pipe(Effect.tapError(() => Reject.mark("ceremony"))) // replay, intent mismatch, and staleness all count; the detail stays log material
    const _challenge = cipher.token(_CHALLENGE_ALPHABET, 43).pipe(
      Effect.mapBoth({
        onFailure: (cause) => new WebAuthnFault({ reason: "ceremony", detail: cause.detail }),
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
            supportedAlgorithmIDs: [..._ALGORITHMS],
            excludeCredentials: existing.map((passkey) => ({ id: passkey.id })),
          }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* _stash(subject, "enroll", options.challenge)
        return options
      }).pipe(Effect.withSpan("security.webauthn.enrollStart"))
    const enrollFinish = (subject: Subject["id"], response: RegistrationResponseJSON): Effect.Effect<Passkey, WebAuthnFault> =>
      Effect.gen(function* () {
        const expectedChallenge = yield* _expected(subject, "enroll")
        const verified = yield* Effect.tryPromise({
          try: () => verifyRegistrationResponse({
            response, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID,
            requireUserVerification: true, supportedAlgorithmIDs: [..._ALGORITHMS],
          }),
          catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
        }).pipe(Effect.filterOrFail(
          (outcome): outcome is Extract<VerifiedRegistrationResponse, { verified: true }> => outcome.verified,
          () => new WebAuthnFault({ reason: "verification", detail: "unverified registration" }),
        ))
        const statement = yield* trust.attestationType === "none"
          ? Effect.succeedNone
          : Effect.tryPromise({
              try: () => MetadataService.getStatement(verified.registrationInfo.aaguid),
              catch: (cause) => new WebAuthnFault({ reason: "attestation", detail: String(cause) }),
            }).pipe(Effect.orElseSucceed(() => undefined), Effect.map(Option.fromNullable))
        const passkey = new Passkey({
          id: verified.registrationInfo.credential.id, subject, publicKey: verified.registrationInfo.credential.publicKey,
          counter: verified.registrationInfo.credential.counter, aaguid: Option.some(verified.registrationInfo.aaguid),
          model: Option.flatMap(statement, (held) => Option.fromNullable(held.description)),
          transports: Option.fromNullable(verified.registrationInfo.credential.transports),
        })
        yield* store.insert(passkey)
        return passkey
      }).pipe(Effect.withSpan("security.webauthn.enrollFinish"))
    const assertStart = (subject: Subject["id"]): Effect.Effect<PublicKeyCredentialRequestOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const passkeys = yield* store.bySubject(subject)
        const challenge = yield* _challenge
        const options = yield* Effect.tryPromise({
          try: () => generateAuthenticationOptions({ rpID, challenge, allowCredentials: passkeys.map((passkey) => ({ id: passkey.id })), userVerification: "required" }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* _stash(subject, "assert", options.challenge)
        return options
      }).pipe(Effect.withSpan("security.webauthn.assertStart"))
    const assertFinish = (subject: Subject["id"], response: AuthenticationResponseJSON): Effect.Effect<TokenPair, WebAuthnFault | SessionFault> =>
      limit({ algorithm: "token-bucket", onExceeded: "fail", window, limit: budget, key: `webauthn:${subject}` })(
        Effect.gen(function* () {
          const passkey = yield* Effect.flatMap(store.byId(response.id), Option.match({
            onNone: () => Effect.fail(new WebAuthnFault({ reason: "verification", detail: response.id })),
            onSome: Effect.succeed,
          })).pipe(Effect.filterOrFail(
            (held) => held.subject === subject,
            () => new WebAuthnFault({ reason: "verification", detail: response.id }),
          ))
          const expectedChallenge = yield* _expected(subject, "assert")
          const credential: WebAuthnCredential = {
            id: passkey.id, publicKey: passkey.publicKey, counter: passkey.counter,
            ...(Option.isSome(passkey.transports) && { transports: [...passkey.transports.value] }),
          }
          const verified = yield* Effect.tryPromise({
            try: () => verifyAuthenticationResponse({ response, credential, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
            catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
          }).pipe(Effect.filterOrFail(
            (outcome) => outcome.verified,
            () => new WebAuthnFault({ reason: "verification", detail: "unverified assertion" }),
          ))
          const next = verified.authenticationInfo.newCounter
          yield* next > passkey.counter || (next === 0 && passkey.counter === 0)
            ? Effect.void
            : Reject.mark("clone").pipe(
                Effect.zipRight(Effect.logError("webauthn counter regression — cloned authenticator")),
                Effect.annotateLogs("passkey", passkey.id),
                Effect.zipRight(Effect.fail(new WebAuthnFault({ reason: "counter", detail: passkey.id }))),
              )
          yield* store.updateCounter(passkey.id, next)
          return yield* token.establish(new CredentialRef({ kind: "webauthn", key: passkey.id }), ["openid"], { tenant: Option.none(), verified: true })
        }),
      ).pipe(
        Effect.catchTags({
          RateLimitExceeded: () => Effect.fail(new WebAuthnFault({ reason: "throttled", detail: subject })),
          RateLimitStoreError: (error) => Effect.fail(new WebAuthnFault({ reason: "throttled", detail: String(error) })),
        }),
        Effect.withSpan("security.webauthn.assertFinish"),
      )
    return { enrollStart, enrollFinish, assertStart, assertFinish } as const
  }),
  dependencies: [Crypto.Default, Token.Default, WebAuthnTrust.Live],
  accessors: true,
}) {}
```

## [04]-[BROWSER_CEREMONY]

[BROWSER_CEREMONY]:
- Owner: `Passkeys.register`/`Passkeys.authenticate` — the `./browser` runtime module wrapping `navigator.credentials` into an `Effect` gated on a capability probe; `Passkeys.autofill` runs the conditional-UI assertion; `Passkeys.probe` reports platform-authenticator and autofill availability. `PasskeyFault` folds the pre-classified `WebAuthnError` `code`.
- Law: the ceremony is gated before the call — `browserSupportsWebAuthn` short-circuits an unsupported browser to a typed capability fault, and `autofill` demands `browserSupportsWebAuthnAutofill` as its second gate; a ceremony entry is never called without its probe.
- Law: `WebAuthnAbortService` enforces the single-live-ceremony law — each ceremony auto-arms a fresh `AbortSignal` and a new call cancels the prior, and `Passkeys.cancel` fires on a client-route change; the v13 `{ optionsJSON }` object form is the only call shape, never the pre-12 positional form; `register` carries the `useAutoRegister` conversion affordance so a just-signed-in password upgrades to a passkey without a second ceremony surface.
- Law: the browser never verifies — it invokes the authenticator and returns the response JSON; a `Schema` per JSON shape decodes both the inbound options and the outbound response at the fetch seam the ui folder owns; conditional-UI autofill (`useBrowserAutofill: true`) is a browser-only affordance the ui edge mounts on a login field.
- Receipt: the `RegistrationResponseJSON`/`AuthenticationResponseJSON` the caller POSTs back to `WebAuthn.*Finish`; the browser collects the signed response, never a verdict.
- Growth: a new probe (`platformAuthenticatorIsAvailable` variants) is one `probe` field; a new ceremony affordance is one options field.
- Boundary: this module is `runtime:browser` and imports no node code — the RP verification is the `./server` module; `@simplewebauthn/server` verifies.
- Packages: `@simplewebauthn/browser` (`startRegistration`/`startAuthentication`, `useAutoRegister`, the probes, `WebAuthnAbortService`, `WebAuthnError`).

```typescript
import {
  browserSupportsWebAuthn, browserSupportsWebAuthnAutofill, platformAuthenticatorIsAvailable, startAuthentication,
  startRegistration, WebAuthnAbortService, WebAuthnError,
  type AuthenticationResponseJSON, type PublicKeyCredentialCreationOptionsJSON, type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON,
} from "@simplewebauthn/browser"
import { Data, Effect } from "effect"

class PasskeyFault extends Data.TaggedError("PasskeyFault")<{ readonly code: string; readonly detail: string }> {}

const _lift = <A>(supported: boolean, run: () => Promise<A>): Effect.Effect<A, PasskeyFault> =>
  supported
    ? Effect.tryPromise({ try: run, catch: (cause) => cause instanceof WebAuthnError ? new PasskeyFault({ code: cause.code, detail: cause.message }) : new PasskeyFault({ code: "ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY", detail: String(cause) }) })
    : Effect.fail(new PasskeyFault({ code: "ERROR_UNSUPPORTED", detail: "webauthn unsupported" }))

const Passkeys = {
  register: (optionsJSON: PublicKeyCredentialCreationOptionsJSON, options?: { readonly autoRegister?: boolean }): Effect.Effect<RegistrationResponseJSON, PasskeyFault> =>
    _lift(browserSupportsWebAuthn(), () => startRegistration({ optionsJSON, ...(options?.autoRegister === true && { useAutoRegister: true }) })),
  authenticate: (optionsJSON: PublicKeyCredentialRequestOptionsJSON): Effect.Effect<AuthenticationResponseJSON, PasskeyFault> =>
    _lift(browserSupportsWebAuthn(), () => startAuthentication({ optionsJSON })),
  autofill: (optionsJSON: PublicKeyCredentialRequestOptionsJSON): Effect.Effect<AuthenticationResponseJSON, PasskeyFault> =>
    Effect.flatMap(
      Effect.promise(() => browserSupportsWebAuthnAutofill()),
      (ready) => _lift(browserSupportsWebAuthn() && ready, () => startAuthentication({ optionsJSON, useBrowserAutofill: true })),
    ),
  probe: (): Effect.Effect<{ readonly platform: boolean; readonly autofill: boolean }> =>
    Effect.all({
      platform: Effect.promise(() => platformAuthenticatorIsAvailable()),
      autofill: Effect.promise(() => browserSupportsWebAuthnAutofill()),
    }),
  cancel: (): Effect.Effect<void> => Effect.sync(() => WebAuthnAbortService.cancelCeremony()),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CeremonyPhase, ChallengeStore, Passkey, Passkeys, PasskeyFault, WebAuthn, WebAuthnFault, WebAuthnStore, WebAuthnTrust }
```
