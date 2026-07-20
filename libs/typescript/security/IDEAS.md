# [TS_SECURITY_IDEAS]

Forward pool of higher-order folder concepts grounded in the identity-and-custody domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[0003]-[ACTIVE]: security fact rail — one typed `SecurityFact` domain-fact family behind an app-scoped hook registry with a durable `AuditJournal` port, so breach evidence is receipt-truth and metrics stay the lossy dashboard channel.
- Capability: every loud arm the folder already fires (refresh reuse, webauthn clone, secret rotation, policy deny, ceremony refusal, key admission, shred-open reject) publishes a typed fact through `rasm.security.<domain>.<point>` registry points with observe/veto modalities and subscriber-fault isolation; `AuditJournal` is the append-only port the data wave satisfies; a pseudonymized projection folds `Crypto.fingerprint` over subject identifiers so audit facts leave to analytics stores without PII.
- Shape: new page `libs/typescript/security/.planning/access/audit.md` owning the fact vocabulary, the registry, the journal port, a `Metric.snapshot` support-bundle projection, and the egress projection; existing pages publish facts beside their `Reject.mark`/log arms with zero verdict-path change — marking stays evidence, never control flow.
- Unlocks: security-event alerting, compliance export, session-forensics queries, and the `[0005]` board pack all ride one fact plane; two apps composing the library subscribe under disjoint registry scopes with no shared mutable state.
- Anchors: `Reject` stream (`.planning/crypt/verify.md`), `Reused`/`clone`/`_publish`/`Deny` arms, `Convention.instrument` rows, `effect` `PubSub`/`Stream`/`Metric.snapshot` (`libs/typescript/.api/effect.md`), branch hook-rail law.

[0004]-[QUEUED]: workload identity — machine-to-machine grants (client-credentials, token exchange) and DPoP sender-constrained proofs complete the authn plane for service, sidecar, and headless app shapes.
- Capability: a machine principal mints and refreshes its own access token through grant rows sharing the oauth row grammar; DPoP proofs mint and verify through `Jwt` with the `cnf.jkt` binding `Material.thumbprintUri` already supplies; the resolved machine principal projects into the per-call transport credential (gRPC metadata, NATS auth callout header) the runtime wave mounts.
- Shape: new page `libs/typescript/security/.planning/authn/workload.md` — grant rows, proof mint/verify, principal projection; `authn/oauth.md` keeps the browser authorization-code ceremony untouched.
- Unlocks: service-to-service auth over every transport axis at this folder's altitude; a fleet worker authenticates without a browser ceremony or a hand-carried static token.
- Anchors: `AccessClaims.cnf`, `Material.thumbprintUri`, `IssuerRef` remote verify, `arctic` ceremony rows, `openid-client` (admission-lane candidate for the certified grant/DPoP client).
- Tension: `openid-client` admission must survive the supersession review against `arctic` — split custody rules arctic the browser code ceremony and openid-client the machine grants and DPoP.

[0005]-[QUEUED]: security board and alert pack as data — panel and burn-rate alert rows over the folder instrument set, compiled and deployed by the iac observe leg.
- Capability: reject-by-kind, policy-deny-by-reason, KDF latency, JWKS resolve/miss/quarantine, secret-rotation age, and session-reuse panels declared as typed rows over the core dashboard model; burn-rate alert rows fire on `breached`-class fact rates (reuse, clone, shred-open) with tenant as the governed grouping dimension.
- Shape: board and alert row section on `libs/typescript/security/.planning/access/audit.md`; the iac observe page compiles the rows through its Foundation-SDK leg — declaration here, compilation there.
- Unlocks: every app composing security inherits the same attack-visibility board with zero per-app dashboard authoring; alert thresholds are policy rows, never hand-tuned JSON.
- Anchors: `Convention.instrument.security*` rows, `Reject` kind facets, `securityPolicyDeny` reason tags, `Convention.rasm.tenant` cardinality governor, iac `observe.md` Foundation-SDK compile leg.

[0006]-[QUEUED]: KDF cost calibration — `CryptoCost` rows become bench-derived facts admitted per host class, never hand-copied OWASP folklore.
- Capability: a bench leg measures argon2 latency per cost row on the executing host, emits the core benchmark-receipt wire with host-fingerprint admission, and selects the strongest row satisfying the per-class latency target; `Jwt` mint/verify and the verify fold ride the same receipt family so crypto throughput claims are evidence.
- Shape: calibration law and bench receipt rows on `libs/typescript/security/.planning/crypt/sign.md` beside `CryptoCost`; receipts compose the core `BenchmarkClaimWire` family — no new bench engine.
- Unlocks: a cost bump is a receipt-backed row edit the rehash fold propagates; a slow host class degrades to a measured row instead of timing out login storms.
- Anchors: `CryptoCost` rows, `Convention.instrument.securityKdf` timer boundaries, semaphore bulkhead permits, core `BenchmarkClaimWire` host-fingerprint admission.

[0007]-[QUEUED]: admitted-surface completion — every verified-unexploited folder and branch catalog member lands as a row on its owning page, closing the census gap between catalogs and pages.
- Capability: `getTimeStepUsed` replaces the hand-rolled TOTP window math; `encodeBase64urlNoPadding` gives opaque tokens a base64url wire row; `preferredAuthenticatorType` and `verifyBrowserAutofillInput` complete the webauthn policy and autofill surfaces; `secrets.list` backs the partial-refresh planner with the full-object census.
- Shape: one row per member on its owner — `libs/typescript/security/.planning/authn/credential.md`, `libs/typescript/security/.planning/crypt/sign.md`, `libs/typescript/security/.planning/authn/webauthn.md`, `libs/typescript/security/.planning/crypt/secret.md`.
- Unlocks: admitted capability the estate already pays for stops idling; hand-rolled parallels to library members are deleted.
- Anchors: `.api/otplib.md`, `.api/oslojs-encoding.md`, `.api/simplewebauthn-server.md`, `.api/simplewebauthn-browser.md`, `.api/dopplerhq-node-sdk.md` — each member re-verified at its catalog anchor.

[0008]-[QUEUED]: cookie-session guard — the guard family completes with the browser cookie scheme, so a cookie-authenticated browser app rides the same declarative middleware seam as bearer and api-key callers.
- Capability: a `SessionGuard` middleware decodes the access cookie through the platform security-decode seam, folds `Jwt.verify` and the CSRF double-submit check into one requirement provision, and lands refused presentations on the `bearer` reject row; cookie framing, CSRF mint, and guard verdict stay one lifecycle.
- Shape: guard row on `libs/typescript/security/.planning/authn/session.md` beside `BearerGuard` — same `HttpApiMiddleware.Tag` grammar, `provides: CurrentClaims`.
- Unlocks: a zero-JS-token browser app composes full session auth declaratively; the runtime serve wave mounts one more guard row, never a hand parser.
- Anchors: `HttpApiBuilder.securityDecode` (`libs/typescript/.api/effect-platform.md`), `HttpApiSecurity.apiKey` cookie variant, `Cookie.verify`, `CookieSpec` rows, `Reject.mark("bearer")`.

[0009]-[BLOCKED]: per-app webauthn trust isolation — the process-global `SettingsService`/`MetadataService` singletons collide with the app-neutrality law when two apps with divergent attestation policies share one process.
- Capability: either instance-scoped trust configuration (root certs and MDS policy per `WebAuthnTrust` layer) or a ruled composition law making the single-policy constraint explicit and collision-proof at the layer seam.
- Shape: trust-isolation ruling and any instance-scoped rewrite land on `libs/typescript/security/.planning/authn/webauthn.md` `[02]-[ATTESTATION_TRUST]`.
- Unlocks: a multi-app host (plugin runtime, sidecar pair) composes two webauthn verifiers without silent policy bleed.
- Anchors: `WebAuthnTrust` layer, `.api/simplewebauthn-server.md` trust-service members, mandate app-neutrality law.
- Tension: blocked on one answerable question — does `@simplewebauthn/server` expose instance-scoped `SettingsService`/`MetadataService` construction, or only module-global mutation? Route: `.api/simplewebauthn-server.md` trust section, then the installed package source under `node_modules/@simplewebauthn/server`.

[0010]-[QUEUED]: LeaseSpec deploy-realization contract — the encoded lease shape publishes as the boundary value the deploy plane decodes into custody cells.
- Capability: `LeaseSpec` — scope, keys, TTL, renewal posture — gains its one encoded owner on the crypt plane, so lease semantics have a single spelling and the iac workload estate decodes each lease into a config-scoped Doppler `ServiceToken` and namespace-`Secret` custody cell as pure data; renewal posture and epoch-keyed rotation read from the spec, never from deploy-side convention.
- Shape: encoded `LeaseSpec` schema and boundary row on `libs/typescript/security/.planning/crypt/secret.md` beside the `_ACCESS` custody law; the `ARCHITECTURE.md` `[BOUNDARY]: LeaseSpec` seam keeps its spelling with the schema now real.
- Unlocks: a leased credential's blast radius is the lease by construction; per-app lease custody composes without collision; the deploy plane consumes lease semantics with zero security knowledge.
- Anchors: `crypt/secret.md` custody and rotation rows; `ARCHITECTURE.md` Crypt-to-Iac `[BOUNDARY]: LeaseSpec` edge; iac `kube/workload.md` lease realization (carded).
- Tension: iac realizes custody cells — this folder owns only the encoded shape and its semantics; a deploy concern leaking into the spec is a boundary breach.
- Ripple: `iac` `[0009]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: one authenticity-reject fact stream — realized as the `Reject` owner on the crypt verify page; dialect/surface/reason are bounded facets, never divergent metric names.
- [0002]-[COMPLETE]: Convention-owned instrument plane — every folder instrument (kdf timer, jwks quarantine/resolve/miss, shred reject, secret rotation, policy deny) mints from a core `Convention.instrument` row, and the tenant/reason tag keys read `Convention.rasm.*`.
