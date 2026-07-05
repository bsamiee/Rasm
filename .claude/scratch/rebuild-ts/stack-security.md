# [STACK_SECURITY] — ultra-stacking dossier for libs/typescript/security

Findings-only. The security corpus is the folder the census called "unusually complete" — every router entry resolves, every admitted package is imported in its owning page, no phantoms. That completeness is a trap: the pages are *correct* but *flat*. They mint, verify, and rotate with zero internalized intelligence — no telemetry on a single security-critical event, no rate limiting on any credential-verify surface, bare-`fetch` egress on the JWKS remote resolver, hand-rolled cookie framing beside `@effect/platform`'s `Cookies` codec, and abstract ports where a TTL cache or durable actor is the obvious owner. The whole folder violates campaign-law points 2 (single deep owners with automatic intelligence), 3 (per-tenant telemetry/diagnostics hooks), and 6 (resilience rides every egress internally). This dossier is the buildout that pulls it to 13/10.

The spelling law: every symbol below is verified against the on-disk catalog or the branch `.api` tier. No phantom.

---

## [A]-[UNDERUTILIZED MEMBERS PER CATALOG]

Exact spelling · owning page · what it unlocks.

### jose (`.api/jose.md`) → `crypt/sign.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `resolver.reload()` / `.fresh` / `.coolingDown` / `.reloading` / `.jwks()` | sign · TOKEN_AUTHORITY | `verifyExternal` builds `createRemoteJWKSet` but only ever refetches *reactively* on a `kid` miss under `cooldownDuration`. The catalog's own law: "a `Schedule` drives `reload`, `jwks()` snapshots the current set." A `Schedule`-driven proactive `reload()` keeps the cache warm across a provider key roll before the first miss. Currently a rotation window is a cold-miss latency spike per stateless invocation. |
| `generateKeyPair(alg, { extractable })` → `GenerateKeyPairResult` | sign · KEY_MATERIAL | `Material.admit` only ever *imports* a fetched/wire PEM. There is no self-minting path — a KMS-less dev/test/bootstrap ring must round-trip through Doppler. `generateKeyPair("ES256", { extractable: false })` is the row that mints an ephemeral non-extractable signing keypair for the composition root's default ring. Growth row on `Material`, not a fork. |
| `calculateJwkThumbprintUri(key)` | sign · KEY_MATERIAL | `Material.thumbprint` mints the bare RFC 7638 hash; the `urn:ietf:params:oauth:jwk-thumbprint:sha-256:<b64>` URI form is the stable `sub` identity for a key-bound principal (DPoP / mTLS-bound tokens). One projection row. |
| `CompactSign`/`FlattenedSign`/`GeneralSign` + `compactVerify`/`generalVerify` | sign · (growth) | The raw JWS serialization axis is entirely unused. `GeneralSign` (multi-signature) is the row for a detached-signature artifact or a co-signed document — a real AEC-domain need (dual-signed drawings/BIM exports). Not required now; a growth row on a future `crypt/sign` detached-signature surface, flagged so the improver does not re-invent it. |

### arctic (`.api/arctic.md`) → `authn/oauth.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `ArcticFetchError` retry | oauth · CEREMONY/GRANT_LIFECYCLE | The fault row classes `transport` as `unavailable` (retryable per `FaultClass`) but the ceremony legs are bare `Effect.tryPromise` — the retryable classification is *declared and never acted on*. `Effect.retry(Schedule.exponential(...).pipe(Schedule.jittered))` on the `ArcticFetchError` arm (the only retryable one; `OAuth2RequestError` is terminal) is the missing resilience. |
| `Effect.timeout` on the three fetch legs | oauth · CEREMONY | arctic owns its own `fetch` with no timeout; a hung provider token endpoint hangs the ceremony fiber indefinitely. `Effect.timeout(Duration)` bounds it. |
| `createS256CodeChallenge(verifier)` | oauth · (already covered) | Used transitively inside `createAuthorizationURLWithPKCE`; no direct call needed — noted so the improver does not add a redundant one. |

### @dopplerhq/node-sdk (`.api/dopplerhq-node-sdk.md`) → `crypt/secret.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `Cache` de-dup of `(project, config)` refetch | secret · LEASED_CUSTODY | Catalog STACKS_WITH: "`Cache` de-dupes concurrent refetches of the same `(project, config)`." The design's refresh loop + `probe` can double-fetch under concurrency. An effect `Cache.make({ capacity, timeToLive, lookup })` around the download collapses concurrent refetches to one in-flight request. |
| `Schedule.exponential`/`.jittered`/`.intersect` on the refresh loop | secret · LEASED_CUSTODY | The loop is `Schedule.spaced` only. Catalog: "`Schedule.fixed`/`Schedule.exponential` drives the sub-lease refresh." A transient `download` fault should back off jittered-exponential and re-drive under the fault-class budget, not silently `Effect.ignore` and wait a full flat interval. `Schedule.spaced(window).pipe(Schedule.intersect(Schedule.recurWhile(retryable)))` is the adaptive form. |
| `sdk.secrets.list(project, config, {...})` | secret · (alternative) | Design uses `download` (name→value map). `list` returns per-secret metadata incl. dynamic-lease detail — relevant only if per-secret lease introspection is needed; noted as the road not taken, not a defect. |

### otplib (`.api/otplib.md`) → `authn/credential.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `TOTP.getRemainingTime()` / `.getTimeStepUsed()` | credential · SECOND_FACTOR | The seconds-left-in-window value the `ui` edge renders on a TOTP prompt. `Otp` exposes no remaining-time projection; a `remaining()` member over the `TOTP` class (or a functional derivation over `period`/`epoch`) is the UI affordance. Growth projection. |
| `OTPHooks { encodeToken; validateToken; truncateDigest }` | credential · SECOND_FACTOR | Design growth prose names it ("a Steam-Guard-style alphabet is one otplib `hooks` value") but no `hooks?` field is threaded through `verify_`/`enroll`. Wire the optional `hooks` option so a non-numeric variant is a value, per the stated growth law. |

### @simplewebauthn/server (`.api/simplewebauthn-server.md`) → `authn/webauthn.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `MetadataService.getStatement(aaguid)` | webauthn · RP_VERIFICATION | The design *initializes* MDS (so the internal verifier validates the cert chain) but never *reads* it. The whole value of MDS is enriching the credential with authenticator model + certification level. `getStatement(verified.registrationInfo.aaguid)` projects the `MetadataStatement` onto the `Passkey` at enrollment (authenticator name, FIDO certification, allowed-algorithm list). Real capability left on the table. |
| `authenticatorSelection: AuthenticatorSelectionCriteria` | webauthn · RP_VERIFICATION | `generateRegistrationOptions` accepts a `residentKey`/`userVerification`/`authenticatorAttachment` policy; the design sets none, so it does not pin discoverable-credential (`residentKey: "required"`) — the modern passkey default. A `WebAuthnTrust`-row policy value threaded into `enrollStart`. |
| `supportedAlgorithmIDs` / `supportedCOSEAlgorithmIdentifiers` | webauthn · RP_VERIFICATION | The COSE algorithm allow-list is unpinned (accepts the library default roster). A pinned `[-8 (EdDSA), -7 (ES256)]` policy row is the algorithm-confusion floor, mirroring the `Jwt` `algorithms` pin. |
| `expectedChallenge` resolver-closure overload `((challenge)=>boolean\|Promise<boolean>)` | webauthn · RP_VERIFICATION | Design consumes the challenge from `ChallengeStore` *before* `verify*` and passes the plain string. The documented idiom passes the store-lookup closure so consumption is single-use *on match* inside the verifier. Defensible either way; the closure form is the catalog's canonical shape and avoids a challenge being burned when verify throws for an unrelated reason. |
| `opts.challenge` / `opts.userID` from `Crypto.token` | webauthn · RP_VERIFICATION | Catalog STACK note: supply these to "keep one RNG owner." The design lets the package mint the challenge via its own WebCrypto; delegating to `sign/crypto`'s `RandomReader`-backed `Crypto.token` unifies entropy sourcing (and makes it test-injectable). |

### @simplewebauthn/browser (`.api/simplewebauthn-browser.md`) → `authn/webauthn.md` (`./browser`)

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `startRegistration({ optionsJSON, useAutoRegister })` | webauthn · BROWSER_CEREMONY | `useAutoRegister` silently upgrades a just-signed-in password to a passkey — a real conversion-UX capability. `Passkeys.register` omits the option. One field. |

### @oslojs/crypto (`.api/oslojs-crypto.md`) → `crypt/verify.md`

| [SYMBOL] | [PAGE] | [GAP] |
| :--- | :--- | :--- |
| `decodeIEEEP1363ECDSASignature(curve, raw)` | verify · DIALECT_TABLE | `_verifyAsym` decodes ECDSA signatures **only** via `decodePKIXECDSASignature` (DER). Many providers (and the JWS ES256 wire form) sign raw `r‖s` IEEE-P1363, not PKIX-DER. A partner signing P1363 cannot be verified today. A dialect-row `sigForm: "pkix" \| "p1363"` axis + this decoder closes the inbound-signature surface. Real gap. |
| `decodePKIXECDSAPublicKey(der, curves)` | verify · VERIFY_FOLD | The `PublicKey` ecdsa variant carries only `sec1` bytes (`decodeSEC1ECDSAPublicKey`). Partner key configs routinely ship SPKI/PKIX-DER EC public keys. A `{ scheme: "ecdsa"; encoding: "sec1" \| "pkix" }` axis + this decoder handles both. Real gap. |
| streaming `new HMAC(Algorithm, key)` → `.update` → `.digest()` | verify · (n/a) | One-shot `hmac` is used; the streaming class matters only for un-buffered large webhook bodies. Verify holds octets in memory anyway — noted as not-needed, not a defect. |

### @oslojs/encoding (`.api/oslojs-encoding.md`) → no gap

Every used codec is case-explicit (no deprecated `encodeBase32` alias). The `IgnorePadding` lenient-decode rows are available if a padding-stripping transport lands; currently correct.

---

## [B]-[NEVER-USED ADMITTED CAPABILITY THE FOLDER CONCEPT DEMANDS]

These are branch-substrate capabilities *already admitted* (in `effect`, `@effect/platform`, `@effect/experimental`, `@effect/opentelemetry` catalogs) that the security domain structurally requires and the folder uses **nowhere**. This is the buildout core.

### B1 — Telemetry spine: ZERO across the entire folder (campaign law §3)

Not one `Metric`, `Effect.withSpan`, or `Effect.log` exists in nine security pages. Security is the folder that most *needs* audit/observability, and every security-critical event fires silently today:

| [EVENT] | [PAGE] | [SIGNAL] (effect-native; exported by runtime's OTLP lane) |
| :--- | :--- | :--- |
| token-reuse breach → `revokeSubject` | session | `Metric.counter("security_token_reuse")` + `Effect.logError` + `Effect.withSpan` — a **breach** that currently vanishes |
| counter-replay clone → `WebAuthnFault.counter` | webauthn | `Metric.counter("security_webauthn_clone")` + span — class `breached`, silent |
| CSRF double-submit mismatch | session | `Metric.counter("security_csrf_mismatch")` |
| OAuth `state` mismatch (CSRF/replay) | oauth | `Metric.counter("security_oauth_state_reject")` |
| credential-verify reject (brute-force telemetry) | credential/verify | `Metric.counter` tagged by surface + subject-hash |
| policy `Deny` (by reason) | claim | `Metric.counter("security_policy_deny").tagged("reason", …)` |
| secret rotation observed on `changes` | secret | `Metric.counter("security_secret_rotation")` + `Effect.log` audit |
| JWKS remote reload / cold miss | sign | `Metric.histogram` on remote-resolve latency |
| argon2 digest/verify latency | sign | `Metric.timerWithBoundaries("security_argon2_ms")` — cost-tuning + DoS signal |

The mechanism (campaign §3, per-tenant sound): every instrument is `(app, tenant)`-tagged via the `TenantScope` reference the folder already owns (`Metric.tagged` reading the bound `Principal`). Diagnostics grow *consumer hooks* — the tap is the `Effect.withSpan`/`withMetric` seam that a downstream app plugs an exporter into without forking. The security owners emit through effect-native `Metric`/`Tracer`/`Logger`; the runtime wave's `@effect/opentelemetry` `Otlp.layer` exports them with **no call-site change** (that is the whole point of the `Layer.setTracer`/OTLP swap). Security imports nothing from `@opentelemetry/*` — it emits through the substrate.

### B2 — Brute-force RateLimiter: NONE (campaign law §6; `@effect/experimental` RateLimiter)

`RateLimiter.makeWithRateLimiter({ algorithm, onExceeded, window, limit, key, tokens? })` and `RateLimiter.RateLimiterStore` (store-backed for distributed multi-app) are admitted and used nowhere. Every credential-verify surface in the folder is an un-throttled brute-force target:

| [SURFACE] | [PAGE] | [KEY] |
| :--- | :--- | :--- |
| `ApiKey.resolve` | credential | `prefix` (the public index) |
| `Otp.verify` / `Otp.redeem` | credential | `subject` |
| `Token.refresh` | session | `sid` |
| `Verify.verify` (inbound signature) | verify | `dialect + source` |
| `WebAuthn.assertFinish` | webauthn | `subject` |
| `OAuth.callback` | oauth | `state`-source |

The idiom: `RateLimiter.makeWithRateLimiter({ algorithm: "token-bucket", onExceeded: "fail", window, limit, key })(effect)` wrapping the verify body; the `RateLimiter.RateLimitExceeded` fault folds into each page's fault family as a new `throttled` reason (class `exhausted`, retryable-with-backoff via `Retry-After`). Store-backed (`RateLimiterStore`, data-wave-satisfied) so limits hold across the many apps sharing the library (campaign §3 — nothing tangles when many apps share). This is a **never-used capability the folder concept demands**, not an optimization.

### B3 — HttpClient resilience on the JWKS egress (`@effect/platform` HttpClient)

`Jwt`'s `JwksTransport` defaults to bare `globalThis.fetch` (`crypt/sign.md` line ~413). The jose catalog's own STACKS_WITH law: `customFetch` must route through `HttpClient.retryTransient({ schedule })` "so rotation inherits the shared net policy and W3C trace propagation." The remote-JWKS fetch is an *egress with no retry, no timeout, no circuit breaker, no trace propagation* — the exact resilience gap campaign §6 forbids. `HttpClient.retryTransient({ schedule }).pipe(HttpClient.withTracerPropagation)` is the correct `JwksTransport` default (runtime provides the client Layer; security declares the Tag).

### B4 — `@effect/platform` `Cookies` / `Headers.redact` (session hand-rolls the cookie codec)

`Cookie.frame` mints hand-rolled `type FramedCookie = { name; value; options }` and leaves serialization to the edge. `@effect/platform` owns cookie serialization: `Cookies.toCookieHeader`, `Cookies.setCookie`, the `__Host-`/`__Secure-` prefix semantics, and `Headers.redact(headers, keys)` (the platform catalog names `Headers.redact` explicitly for "`security` header redaction"). The design hand-rolls a lower-level reimplementation of an admitted higher-level primitive — a direct doctrine violation (leverage the ecosystem's native abstraction, never re-implement the lower level).

### B5 — `effect` `Cache` / `@effect/experimental` `PersistedCache` for the single-use/TTL stores

`OAuthStateStore`, `JwksLedger`, `ChallengeStore` are abstract `Context.Tag` ports whose natural satisfying owner is a TTL cache — `Cache.make({ capacity, timeToLive, lookup })` (in-process) or `PersistedCache.make({ storeId, lookup, timeToLive })` / `Persistence.layerResultKeyValueStore` (durable, cross-process, multi-app). The design leaves them as bare ports; the improver should declare the TTL contract on the port shape (single-use consume + expiry) so the data-wave layer is a cache, not a hand-rolled map.

### B6 — `effect` `Reloadable.auto` for keyset/JWKS rotation (mentioned, not sealed)

`sign.md` prose says "the composition root wraps `Jwt.Default(keyset)` in its `Reloadable.auto` row." The seam is real (`Reloadable.auto(Tag, { layer, schedule })`) but lives only in prose. It should be an explicit composition contract the page states, paired with `Secret.changes` (the `SubscriptionRef` rotation feed) driving the reload — closing the rotation loop from Doppler → `Material.ring` → `Jwt` without a graph teardown.

---

## [C]-[CROSS-STACKING PLAYS] (package × package the corpus never attempts)

| [PLAY] | [COMPOSITION] | [PAGES] |
| :--- | :--- | :--- |
| **CS1 — Audit spine** | `TenantScope` (folder) → `Metric.tagged("tenant", scope)` + `Effect.withSpan` + `Effect.annotateLogs` on every fault-emitting fold; exported by runtime `@effect/opentelemetry` `Otlp.layer` unchanged | ALL |
| **CS2 — Throttled crypto** | `@effect/experimental` `RateLimiter.makeWithRateLimiter` × each page's `Crypto.verify`/oslo-verify/argon2 body; `RateLimitExceeded` → folder-fault `throttled` row | credential, session, verify, webauthn, oauth |
| **CS3 — Resilient JWKS** | `jose` `createRemoteJWKSet[customFetch]` × `@effect/platform` `HttpClient.retryTransient().withTracerPropagation` × `effect` `Schedule.exponential.jittered` driving `resolver.reload()` | sign |
| **CS4 — Declarative auth seam** | `@effect/platform` `HttpApiSecurity.Bearer`/`.ApiKey` + `HttpApiMiddleware.TagClass.BaseSecurity` × `Jwt.verify` (bearer) / `ApiKey.resolve` (api-key) / `Verify.verify` (the held-octets intake as `HttpApiMiddleware`) — one middleware Tag per scheme the runtime/serve wave binds | sign, session, credential, verify |
| **CS5 — Cookie codec** | `@effect/platform` `Cookies.toCookieHeader`/`.setCookie` + `Headers.redact` × `Cookie.frame`/`.clear`/`.csrf` — retire hand-rolled `FramedCookie` serialization | session |
| **CS6 — TTL-cached ports** | `effect` `Cache` / `@effect/experimental` `PersistedCache`+`Persistence.layerResultKeyValueStore` × `OAuthStateStore`/`JwksLedger`/`ChallengeStore` single-use+expiry contract | oauth, sign, webauthn |
| **CS7 — Rotation loop** | `effect` `Reloadable.auto` × `Secret.changes` (`SubscriptionRef`) × `Material.ring` × `Jwt.Default(keyset)` — sealed rotation without teardown | secret, sign |
| **CS8 — Durable ceremony (candidate)** | `@effect/experimental` `Machine.makeSerializable` + `boot`/`snapshot`/`restore` × the OAuth authorize→callback two-leg ceremony and webauthn enroll/assert — `Subscribable` state for UI, snapshot across process restart (campaign §4) | oauth, webauthn |
| **CS9 — E2E encryption downstream seam** | `@effect/experimental` `EventLogEncryption.layerSubtle`/`makeEncryptionSubtle(crypto)` consumes `Shredder`/`Crypto` key material — the experimental catalog states "`security/secret` composes keys" for the zero-knowledge EventLog server. Security is the *provider*; declare the key-handoff contract so the data/runtime wave binds it | sign, secret |

---

## [D]-[GAP CAPABILITIES] (package + integration shape — improver weighs each)

| [GAP] | [CLOSER] | [SHAPE] |
| :--- | :--- | :--- |
| **D1 — IEEE-P1363 ECDSA + PKIX-EC public keys** | `@oslojs/crypto` (already admitted) `decodeIEEEP1363ECDSASignature`, `decodePKIXECDSAPublicKey` | `verify.md`: `PublicKey` ecdsa variant gains `encoding: "sec1"\|"pkix"`; dialect row gains `sigForm: "pkix"\|"p1363"`. Closes the inbound-signature surface for JWS-style and P1363 partners. **Strong — a real capability hole.** |
| **D2 — MDS metadata projection** | `@simplewebauthn/server` (admitted) `MetadataService.getStatement` | `webauthn.md`: `Passkey` gains `model`/`certification` fields projected from the statement at `enrollFinish`. **Strong — the whole point of initializing MDS.** |
| **D3 — Distributed brute-force limiting** | `@effect/experimental` `RateLimiter` + `RateLimiterStore` (admitted) | B2 above. **Strong — the folder concept demands it.** |
| **D4 — Ephemeral self-minted signing ring** | `jose` (admitted) `generateKeyPair` | `sign.md`: `Material.mint(alg)` growth row for KMS-less bootstrap/test rings. Medium. |
| **D5 — Durable multi-tenant onboarding ceremonies** | `@effect/experimental` `Machine` (admitted) | CS8. Weigh: OAuth is short-lived (may be overkill), but a device-onboarding / multi-factor-enrollment flow across process restarts is the genuine Machine case. Medium — improver decides per §4 ambition. |
| **D6 — DPoP / key-bound tokens** | `jose` `calculateJwkThumbprintUri` (admitted) + `AccessClaims` `cnf` claim | `sign.md`: sender-constrained tokens (RFC 9449). A `cnf: { jkt }` field on `AccessClaims` + thumbprint-URI binding. Speculative-but-real for a bleeding-edge auth surface (campaign "imagine the most complex apps"). Low-medium. |
| **D7 — WebAuthn conditional-create / auto-register UX** | `@simplewebauthn/browser` `useAutoRegister` (admitted) | `webauthn.md` `./browser`: `Passkeys.register` gains the option. Low — one field, real UX. |

No new package admission is required for D1–D3 (the strong gaps) — all three close with *already-admitted* members. D4–D7 are also admission-free. The folder's package roster is complete; the gap is entirely *composition*, which is the campaign's exact thesis.

---

## [E]-[PER-PAGE INTEGRATION MAP] (what the improver executes)

Ordered by dependency (sign is the root every page consumes).

### crypt/sign.md — the crypto authority (the most under-built for its centrality)
1. **CS3**: `JwksTransport` default → `HttpClient.retryTransient({ schedule }).pipe(HttpClient.withTracerPropagation)`, not `globalThis.fetch`. Drive `resolver.reload()` on a `Schedule.exponential.jittered` inside the `_remote` cached resolver.
2. **CS1**: `Crypto.digest`/`.verify` gain `Metric.timerWithBoundaries("security_argon2_ms")` + `Effect.withSpan`; `Jwt.verifyExternal` gains a JWKS-resolve latency histogram and a cold-miss counter.
3. **CS2**: argon2 `digest`/`verify` bodies wrap in a `RateLimiter` (per-subject) — the KDF is the DoS surface.
4. **D4**: `Material.mint(alg)` via `jose.generateKeyPair` — ephemeral non-extractable ring for bootstrap.
5. **CS7**: state the `Reloadable.auto` + `Secret.changes` rotation contract explicitly (B6).
6. **D6** (weigh): `AccessClaims.cnf` + `calculateJwkThumbprintUri` for DPoP.
7. Shape: `Keyset`/`Ring` module-`type`s — weigh promoting `Keyset` to a `Schema.Class` carrying `issuer`/`audience`/`ring`/`seal` with derived projections (CLASS-FIRST §1); defensible as internal config carriers, low priority.

### crypt/verify.md — inbound signature ingress
1. **D1** (strong): add `decodeIEEEP1363ECDSASignature` + `decodePKIXECDSAPublicKey`; `PublicKey` ecdsa variant → `Data.taggedEnum` keyed by `scheme`, gaining `encoding` axis; dialect rows gain `sigForm`.
2. **CS2**: `Verify.verify` body under a `RateLimiter` keyed `dialect+source` (signature-verify DoS).
3. **CS1**: `mismatch`/`stale`/`unknownKey` faults emit `Metric.counter` tagged by dialect — inbound-attack telemetry.
4. **CS4**: expose the held-octets intake as an `HttpApiMiddleware` Tag the runtime/serve wave mounts (the `crypt/verify → runtime/serve` seam made a typed middleware).
5. Shape: `PublicKey` union → `Data.taggedEnum` (ADT, §1).

### crypt/secret.md — leased custody
1. **CS complement of CS3**: `Cache.make({ timeToLive, lookup })` de-dup around `download` (the catalog's named play).
2. Refresh loop → `Schedule.spaced.intersect(Schedule.exponential.jittered)` under the fault-class retryable predicate, not flat `spaced` + `Effect.ignore`.
3. **CS1**: `changes` rotation feed emits `Metric.counter("security_secret_rotation")` + `Effect.log` audit.
4. **CS7**: `Secret.changes` is the driver for `sign`'s `Reloadable.auto` — state the seam.

### authn/session.md — identity spine (the audit gap is worst here)
1. **CS1 (critical)**: `SessionFault.reuse` → `revokeSubject` MUST emit `Metric.counter("security_token_reuse")` + `Effect.logError` + span. A breach vanishing silently is the single most severe finding in the folder.
2. **CS2**: `Token.refresh` under a per-`sid` `RateLimiter`.
3. **CS5**: `Cookie.frame`/`.clear`/`.csrf` compose `@effect/platform` `Cookies.toCookieHeader`/`.setCookie` + `Headers.redact`; retire hand-rolled `FramedCookie` serialization (keep the `CookieSpec` policy table).
4. **CS4**: session bearer decode → `HttpApiSecurity.Bearer` + a `HttpApiMiddleware.TagClass.BaseSecurity` Tag.
5. **CS1**: CSRF `verify` mismatch → `Metric.counter`.

### authn/credential.md — digest-at-rest + second factor
1. **CS2 (strong)**: `ApiKey.resolve` (per-`prefix`), `Otp.verify`/`.redeem` (per-`subject`) under `RateLimiter` — the classic credential brute-force. New `throttled` fault row.
2. **CS1**: verify-reject counters tagged by surface (api-key vs otp vs recovery).
3. **D-otplib**: thread the optional `hooks` field (variant support) and a `remaining()` TOTP-window projection for the ui edge.
4. **CS4**: `ApiKey` → `HttpApiSecurity.ApiKey` scheme.

### authn/oauth.md — authorization-code ceremony
1. **CS-arctic resilience**: `Effect.retry(Schedule.exponential.jittered)` on the `ArcticFetchError` transport arm (the classified-retryable-but-never-retried gap) + `Effect.timeout` on the three fetch legs.
2. **CS1**: `OAuthFault.state` (CSRF/replay) → `Metric.counter("security_oauth_state_reject")` + span on authorize/callback.
3. **CS6**: `OAuthStateStore` port gains a TTL-single-use contract satisfied by `Cache`/`PersistedCache`.
4. **CS8** (weigh): the two-leg ceremony as a `Machine` actor (snapshot/restore) — improver decides per §4.
5. Shape: `Creds` 8-field option-bag → weigh `Schema.Class`.

### authn/webauthn.md — passkey ceremony
1. **D2 (strong)**: `MetadataService.getStatement(aaguid)` → project `model`/`certification` onto `Passkey` at `enrollFinish`.
2. **CS1 (critical)**: `WebAuthnFault.counter` (clone/replay, class `breached`) → `Metric.counter("security_webauthn_clone")` + `logError` + span. Same silent-breach severity as session reuse.
3. **Server policy**: pin `supportedAlgorithmIDs` (`[-8, -7]`) + set `authenticatorSelection: { residentKey: "required", userVerification: "required" }` (discoverable-credential default); supply `opts.challenge` from `Crypto.token` (one RNG owner).
4. **CS2**: `assertFinish` under per-`subject` `RateLimiter`.
5. **`./browser`**: `useAutoRegister` option (D7).
6. Weigh the `expectedChallenge` resolver-closure form (catalog-canonical single-use-on-match).

### access/claim.md — authorization fold
1. **CS1**: `Policy.check` `Deny` → `Metric.counter("security_policy_deny").tagged("reason", …)` — access-denial dashboards.
2. Efficiency: precompute the `_expand` role closure into a static `HashMap` at module load (the closure is immutable data; recomputing per `_granted` call is waste).
3. No package gap — this page is the best-built in the folder.

### access/tenant.md — tenancy contract
1. No capability change — the contract is correctly thin. `Principal` type is a defensible single-consumer field block; the telemetry `(app, tenant)` tag reads `TenantScope.scopeOf` (CS1's tenant-tagging source across the whole folder flows through this owner).

---

## [SUMMARY OF SEVERITY]

- **Critical (silent security breaches)**: session token-reuse and webauthn counter-clone emit nothing — CS1 on session.md + webauthn.md is non-negotiable.
- **Strong (folder concept demands, admission-free)**: B2/CS2 rate limiting (all verify surfaces); D1 verify.md P1363/PKIX-EC signature gap; D2 webauthn MDS projection; B3/CS3 JWKS-egress resilience; B4/CS5 platform Cookies codec.
- **Buildout (campaign §3/§6 conformance)**: B1/CS1 telemetry spine folder-wide; secret.md adaptive-schedule + Cache; CS7 rotation loop; CS4 HttpApiSecurity seam; CS6 TTL-cached ports.
- **Weigh (§4 ambition, §1 shape)**: CS8 durable ceremonies; D6 DPoP; ADT promotions (`PublicKey`, `Creds`/`Keyset` classes).

Every strong finding closes with an **already-admitted** member — the roster is complete, the composition is the work.
