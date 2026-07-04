# [SECURITY_WEBAUTHN]

Both halves of the passkey ceremony as two per-runtime subpath modules: the RP-side verifier over `@simplewebauthn/server` (node `./server`) mints ceremony options and verifies the signed response into a typed verdict, and the browser-safe invocation over `@simplewebauthn/browser` (`./browser`) wraps `navigator.credentials` into an `Effect` gated on a capability probe — the exports map keeps the node verifier physically unreachable from browser resolution. The whole surface is one options→verify pattern across registration and authentication; the attestation-format dispatch is internal, parameterized by policy, never a hand switch. Attestation trust is exploited: `SettingsService` pins the root certificates and `MetadataService` loads the FIDO MDS blob, so a policy of `direct`/`enterprise` attestation validates the authenticator's cert chain and AAGUID metadata rather than accepting `none` blind. The challenge is stateful across the two phases (minted at start, stashed in the `ChallengeStore` port, consumed single-use through the resolver-closure form at finish), and the verified `Passkey` is public-key crypto — a typed boundary value, not a `Redacted` secret — persisted through the `WebAuthnStore` port with its signature counter, the non-increasing-counter check the clone/replay defense. The browser half arms the `WebAuthnAbortService` single-live-ceremony law, gates on `browserSupportsWebAuthn`/`platformAuthenticatorIsAvailable`/`browserSupportsWebAuthnAutofill`, and exposes conditional-UI autofill through `useBrowserAutofill`. A successful assertion establishes a session through `authn/session`; the verdict is a discriminated rail, never a boolean-plus-throw.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                              | [PUBLIC]                                    | [RUNTIME]        |
| :-----: | :------------------ | :------------------------------------------------------------------ | :------------------------------------------ | :--------------- |
|  [01]   | `ATTESTATION_TRUST` | the credential/fault vocabulary, the MDS + root-cert trust anchors  | `Passkey`, `WebAuthnFault`, `WebAuthnTrust` | `./server` node  |
|  [02]   | `RP_VERIFICATION`   | RP-side options + verify, the counter replay defense                | `WebAuthn`, `WebAuthnStore`, `ChallengeStore` | `./server` node |
|  [03]   | `BROWSER_CEREMONY`  | the `navigator.credentials` invocation, autofill, abort, probes     | `Passkeys`, `PasskeyFault`                  | `./browser`      |

## [2]-[ATTESTATION_TRUST]

[ATTESTATION_TRUST]:
- Owner: `Passkey` is the stored credential (id, subject, public key, counter, transports), `WebAuthnFault` the folder fault shape, `WebAuthnTrust` the trust-anchor Layer that configures `SettingsService` root certificates and initializes `MetadataService` from the FIDO MDS. `WebAuthnStore` holds credentials, `ChallengeStore` the single-use challenge.
- Law: attestation policy is a config row — `none` accepts any authenticator, `direct`/`enterprise` demand a validated cert chain; `WebAuthnTrust` sets the per-format root certs (`SettingsService.setRootCertificates`) and initializes MDS with a `strict`/`permissive` unregistered-AAGUID policy once at layer construction, so the format verifier validates provenance and the attestation type is a policy value the verify legs read, never a per-ceremony switch.
- Law: passkey material is public-key crypto — the credential and challenge are typed boundary values, not `Redacted` secrets; the fault rows carry the core `FaultClass` kind so status and blame derive from the branch table.
- Growth: a new authenticator vendor is one root-cert entry plus its MDS metadata; a new attestation posture is one config row.
- Boundary: `@simplewebauthn/server` dispatches the format verifier internally; the browser half collects the response; `authn/session` establishes the session; the trust anchors are config/fetch-sourced at boot.
- Packages: `@simplewebauthn/server` (`SettingsService`, `MetadataService`); `effect` (`Config`, `Context`, `Effect`, `Layer`, `Option`, `Schema`); `@rasm/ts/core` (`FaultClass`); `authn/session` (`Subject`).

```typescript
import {
  generateAuthenticationOptions, generateRegistrationOptions, MetadataService, SettingsService,
  verifyAuthenticationResponse, verifyRegistrationResponse,
  type AuthenticationResponseJSON, type AttestationFormat, type PublicKeyCredentialCreationOptionsJSON,
  type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON, type VerifiedRegistrationResponse, type WebAuthnCredential,
} from "@simplewebauthn/server"
import { FaultClass } from "@rasm/ts/core"
import { Config, Context, Effect, Layer, Option, Schema } from "effect"
import { CredentialRef, type SessionFault, type Subject, Token, type TokenPair } from "./session.ts"

const _reasons = ["ceremony", "challenge", "verification", "counter", "attestation"] as const
const _transports = ["ble", "cable", "hybrid", "internal", "nfc", "smart-card", "usb"] as const

const _faults = {
  ceremony: { class: "defect" },
  challenge: { class: "malformed" },
  verification: { class: "denied" },
  counter: { class: "breached" },
  attestation: { class: "denied" },
} as const

declare namespace WebAuthnFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
}

class Passkey extends Schema.Class<Passkey>("Passkey")({
  id: Schema.NonEmptyString,
  subject: Schema.UUID,
  publicKey: Schema.Uint8ArrayFromBase64,
  counter: Schema.Number,
  aaguid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  transports: Schema.optionalWith(Schema.Array(Schema.Literal(..._transports)), { as: "Option" }),
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

class ChallengeStore extends Context.Tag("security/authn/ChallengeStore")<ChallengeStore, {
  readonly stash: (subject: string, challenge: string) => Effect.Effect<void, WebAuthnFault>
  readonly consume: (subject: string) => Effect.Effect<Option.Option<string>, WebAuthnFault>
}>() {}

class WebAuthnTrust extends Context.Tag("security/authn/WebAuthnTrust")<WebAuthnTrust, {
  readonly attestationType: "none" | "direct" | "enterprise"
}>() {
  static readonly Live: Layer.Layer<WebAuthnTrust> = Layer.effect(
    WebAuthnTrust,
    Effect.gen(function* () {
      const attestationType = yield* Config.literal("none", "direct", "enterprise")("WEBAUTHN_ATTESTATION").pipe(Config.withDefault("none" as const))
      const roots = yield* Config.array(Config.string("WEBAUTHN_ROOT_CERT")).pipe(Config.nested("MDS"), Config.withDefault([] as ReadonlyArray<string>))
      const mode = yield* Config.literal("strict", "permissive")("WEBAUTHN_MDS_MODE").pipe(Config.withDefault("permissive" as const))
      yield* attestationType === "none"
        ? Effect.void
        : Effect.gen(function* () {
            SettingsService.setRootCertificates({ identifier: "mds", certificates: [...roots] })
            yield* Effect.tryPromise({ try: () => MetadataService.initialize({ verificationMode: mode }), catch: (cause) => new WebAuthnFault({ reason: "attestation", detail: String(cause) }) }).pipe(Effect.orDie)
          })
      return { attestationType }
    }),
  )
}
```

## [3]-[RP_VERIFICATION]

[RP_VERIFICATION]:
- Owner: `WebAuthn.enrollStart`/`enrollFinish` register a passkey, `WebAuthn.assertStart`/`assertFinish` authenticate one. The `verified` discriminant is matched so the credential is extracted only on the true arm, and `newCounter` is the replay defense.
- Law: the challenge is minted server-side, stashed, and consumed single-use — the finish leg consumes the stashed challenge on the rail (a miss is `WebAuthnFault.challenge`) and hands `verify*` the exact expected value, so the challenge is never trusted from the client and the store owns single-use; the response is `Schema`-decoded before verify.
- Law: attestation dispatches inside the verifier keyed by the decoded `fmt`, parameterized by `WebAuthnTrust.attestationType` and the pinned root certs — a `direct`/`enterprise` policy validates the cert chain and records the `aaguid`, while `none` accepts any authenticator; the caller never writes the format switch.
- Law: a non-increasing counter is a cloned authenticator (`WebAuthnFault.counter`, class `breached`); a `newCounter` of zero from a fresh authenticator is admitted only when the stored counter is also zero.
- Receipt: `Passkey` on registration, `TokenPair` on assertion — never a raw `VerifiedRegistrationResponse` past the seam.
- Growth: a new transport hint is one `_transports` entry; a new ceremony option is one options-bag field.
- Boundary: `WebAuthnTrust` supplies the attestation policy; the browser half collects the response; `authn/session` `Token.establish` mints the session; the ports carry state.
- Packages: `@simplewebauthn/server` (the 2×2 ceremony); `authn/session` (`Token.establish`, `CredentialRef`).

```typescript
class WebAuthn extends Effect.Service<WebAuthn>()("security/authn/WebAuthn", {
  effect: Effect.gen(function* () {
    const store = yield* WebAuthnStore
    const challenges = yield* ChallengeStore
    const trust = yield* WebAuthnTrust
    const token = yield* Token
    const rpID = yield* Config.string("WEBAUTHN_RP_ID")
    const rpName = yield* Config.string("WEBAUTHN_RP_NAME")
    const origin = yield* Config.string("WEBAUTHN_ORIGIN")
    const _expected = (subject: string): Effect.Effect<string, WebAuthnFault> =>
      Effect.flatMap(challenges.consume(subject), Option.match({
        onNone: () => Effect.fail(new WebAuthnFault({ reason: "challenge", detail: subject })),
        onSome: Effect.succeed,
      }))
    const enrollStart = (subject: Subject["id"], userName: string): Effect.Effect<PublicKeyCredentialCreationOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const existing = yield* store.bySubject(subject)
        const options = yield* Effect.tryPromise({
          try: () => generateRegistrationOptions({ rpName, rpID, userName, userID: new TextEncoder().encode(subject), attestationType: trust.attestationType, excludeCredentials: existing.map((passkey) => ({ id: passkey.id })) }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* challenges.stash(subject, options.challenge)
        return options
      })
    const enrollFinish = (subject: Subject["id"], response: RegistrationResponseJSON): Effect.Effect<Passkey, WebAuthnFault> =>
      Effect.gen(function* () {
        const expectedChallenge = yield* _expected(subject)
        const verified = yield* Effect.tryPromise({
          try: () => verifyRegistrationResponse({ response, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
          catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
        }).pipe(Effect.filterOrFail(
          (outcome): outcome is Extract<VerifiedRegistrationResponse, { verified: true }> => outcome.verified,
          () => new WebAuthnFault({ reason: "verification", detail: "unverified registration" }),
        ))
        const passkey = new Passkey({
          id: verified.registrationInfo.credential.id, subject, publicKey: verified.registrationInfo.credential.publicKey,
          counter: verified.registrationInfo.credential.counter, aaguid: Option.some(verified.registrationInfo.aaguid),
          transports: Option.fromNullable(verified.registrationInfo.credential.transports),
        })
        yield* store.insert(passkey)
        return passkey
      })
    const assertStart = (subject: Subject["id"]): Effect.Effect<PublicKeyCredentialRequestOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const passkeys = yield* store.bySubject(subject)
        const options = yield* Effect.tryPromise({
          try: () => generateAuthenticationOptions({ rpID, allowCredentials: passkeys.map((passkey) => ({ id: passkey.id })), userVerification: "required" }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* challenges.stash(subject, options.challenge)
        return options
      })
    const assertFinish = (subject: Subject["id"], response: AuthenticationResponseJSON): Effect.Effect<TokenPair, WebAuthnFault | SessionFault> =>
      Effect.gen(function* () {
        const passkey = yield* Effect.flatMap(store.byId(response.id), Option.match({ onNone: () => Effect.fail(new WebAuthnFault({ reason: "verification", detail: response.id })), onSome: Effect.succeed }))
        const expectedChallenge = yield* _expected(subject)
        const credential: WebAuthnCredential = {
          id: passkey.id, publicKey: passkey.publicKey, counter: passkey.counter,
          ...(Option.isSome(passkey.transports) && { transports: [...passkey.transports.value] }),
        }
        const verified = yield* Effect.tryPromise({
          try: () => verifyAuthenticationResponse({ response, credential, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
          catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
        })
        yield* verified.verified ? Effect.void : Effect.fail(new WebAuthnFault({ reason: "verification", detail: "unverified assertion" }))
        const next = verified.authenticationInfo.newCounter
        yield* next > passkey.counter || (next === 0 && passkey.counter === 0)
          ? Effect.void
          : Effect.fail(new WebAuthnFault({ reason: "counter", detail: passkey.id }))
        yield* store.updateCounter(passkey.id, next)
        return yield* token.establish(new CredentialRef({ kind: "webauthn", key: passkey.id }), ["openid"], { tenant: Option.none(), verified: true })
      })
    return { enrollStart, enrollFinish, assertStart, assertFinish } as const
  }),
  dependencies: [Token.Default, WebAuthnTrust.Live],
  accessors: true,
}) {}
```

## [4]-[BROWSER_CEREMONY]

[BROWSER_CEREMONY]:
- Owner: `Passkeys.register`/`Passkeys.authenticate` — the `./browser` runtime module wrapping `navigator.credentials` into an `Effect` gated on a capability probe; `Passkeys.autofill` runs the conditional-UI assertion; `Passkeys.probe` reports platform-authenticator and autofill availability. `PasskeyFault` folds the pre-classified `WebAuthnError` `code`.
- Law: the ceremony is gated before the call — `browserSupportsWebAuthn` short-circuits an unsupported browser to a typed capability fault, and `autofill` additionally checks `browserSupportsWebAuthnAutofill`; a ceremony entry is never called without its probe.
- Law: `WebAuthnAbortService` enforces the single-live-ceremony law — each ceremony auto-arms a fresh `AbortSignal` and a new call cancels the prior, and `Passkeys.cancel` fires on a client-route change; the v13 `{ optionsJSON }` object form is the only call shape, never the pre-12 positional form.
- Law: the browser never verifies — it invokes the authenticator and returns the response JSON; a `Schema` per JSON shape decodes both the inbound options and the outbound response at the fetch seam the ui folder owns; conditional-UI autofill (`useBrowserAutofill: true`) is a browser-only affordance the ui edge offers on a login field.
- Receipt: the `RegistrationResponseJSON`/`AuthenticationResponseJSON` the caller POSTs back to `WebAuthn.*Finish`; the browser collects the signed response, never a verdict.
- Growth: a new probe (`platformAuthenticatorIsAvailable` variants) is one `probe` field; a new ceremony affordance is one options field.
- Boundary: this module is `runtime:browser` and imports no node code — the RP verification is the `./server` module; `@simplewebauthn/server` verifies.
- Packages: `@simplewebauthn/browser` (`startRegistration`/`startAuthentication`, the probes, `WebAuthnAbortService`, `WebAuthnError`).

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
  register: (optionsJSON: PublicKeyCredentialCreationOptionsJSON): Effect.Effect<RegistrationResponseJSON, PasskeyFault> =>
    _lift(browserSupportsWebAuthn(), () => startRegistration({ optionsJSON })),
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

export { ChallengeStore, Passkey, Passkeys, PasskeyFault, WebAuthn, WebAuthnFault, WebAuthnStore, WebAuthnTrust }
```
