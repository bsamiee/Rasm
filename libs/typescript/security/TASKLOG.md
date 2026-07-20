# [TS_SECURITY_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0003]-[ACTIVE]: author `libs/typescript/security/.planning/access/audit.md` — the `SecurityFact` vocabulary, app-scoped hook registry, `AuditJournal` port, and pseudonymized egress projection (idea `IDEAS.md` `[0003]`).
- Capability: closed fact family mirroring the loud arms (reuse, clone, rotation, deny, ceremony, admission, shred-open), registry points `rasm.security.<domain>.<point>` with observe/veto modalities and subscriber-fault isolation, append-only journal port on the `E`-channel fault law, `Crypto.fingerprint` subject pseudonymizer for analytics egress.
- Shape: one new page under `access/`; port satisfaction stays the data wave's per the existing `SessionStore` seam grammar; registry scoping keys off the core app identity so two apps never share a mutable registry.
- Anchors: `Reject` kinds (`crypt/verify.md`), `effect` `PubSub`/`Stream`, `Convention` tag keys, `docgen` spec template for the page spine.

[0004]-[QUEUED]: publish `SecurityFact` beside every existing loud arm — `session.md` `Reused`, `webauthn.md` clone and ceremony refusals, `secret.md` `_publish` rotation, `claim.md` `Deny` tap, `sign.md` shred-open reject (idea `IDEAS.md` `[0003]`).
- Capability: each arm gains one fact-publish line composed through `Effect.tap*` beside its `Reject.mark`/log call; verdict paths stay byte-identical.
- Shape: five page edits, each one arm-local line plus the fact row import; no new owner, no new metric.
- Anchors: exact arms pinned — `session.md` `[03]` `Reused` dispatch, `webauthn.md` `[03]` counter law, `secret.md` `[03]` `_publish`, `claim.md` `[05]` deny tap, `sign.md` `[05]` `_openReject`.

[0005]-[QUEUED]: land the `Metric.snapshot` support-bundle projection on `libs/typescript/security/.planning/access/audit.md` — the local board snapshot an app dumps without an exporter (ideas `IDEAS.md` `[0003]`/`[0005]`).
- Capability: one projection folding `Metric.snapshot` over the folder instrument set into a typed snapshot receipt for support bundles and tests.
- Anchors: `Metric.snapshot` (`libs/typescript/.api/effect.md`), `Convention.instrument.security*` rows.
- Atomic: one projection row.

[0006]-[QUEUED]: author `libs/typescript/security/.planning/authn/workload.md` — client-credentials and token-exchange grant rows, DPoP proof mint/verify, machine-principal projection (idea `IDEAS.md` `[0004]`).
- Capability: grant rows share the oauth row grammar (`Config`-bundled credentials, closed `_kinds` tuple, guard pair); DPoP proofs ride `Jwt` with `cnf.jkt` from `Material.thumbprintUri`; the resolved principal projects into per-call transport credentials the runtime wave mounts.
- Shape: one new page under `authn/`; `oauth.md` untouched; `openid-client` composed as the certified grant/DPoP client under the admission-lane assumption.
- Anchors: `IssuerRef` remote verify, `AccessClaims.cnf`, `arctic` row grammar as the shape precedent, `.api/jose.md` JWS members for proof signing.

[0007]-[QUEUED]: land the board and alert row section on `libs/typescript/security/.planning/access/audit.md` — panels per instrument, burn-rate alerts on `breached`-class fact rates (idea `IDEAS.md` `[0005]`).
- Capability: typed rows over the core dashboard model — reject-by-kind, policy-deny-by-reason, KDF latency, JWKS health, rotation age, reuse rate — with alert thresholds as policy values and tenant as the governed grouping dimension.
- Shape: declaration-only section; compilation and deploy ride the iac observe Foundation-SDK leg through the cross-folder counterpart.
- Anchors: `Convention.instrument.security*` rows, `Reject` facets, `Convention.rasm.tenant` governor, iac `observe.md`.

[0008]-[QUEUED]: land the KDF calibration law and bench receipt rows on `libs/typescript/security/.planning/crypt/sign.md` beside `CryptoCost` (idea `IDEAS.md` `[0006]`).
- Capability: per-host-class bench legs over the cost rows emitting core benchmark-receipt wires with host-fingerprint admission; row selection against the per-class latency target; `Jwt` and verify-fold throughput receipts in the same family.
- Anchors: `CryptoCost`, `_argonMs` boundaries, semaphore permits, core `BenchmarkClaimWire`.

[0009]-[QUEUED]: replace the hand-rolled TOTP window math in `libs/typescript/security/.planning/authn/credential.md` with otplib's `getTimeStepUsed` projection (idea `IDEAS.md` `[0007]`).
- Capability: `Otp.remaining` derives from the library's step projection; the `_PERIOD` modulo arithmetic is deleted.
- Anchors: `.api/otplib.md` `getTimeStepUsed` member anchor.
- Atomic: one member swap.

[0010]-[QUEUED]: add the base64url opaque-token wire row to `libs/typescript/security/.planning/crypt/sign.md` via `encodeBase64urlNoPadding` (idea `IDEAS.md` `[0007]`).
- Capability: `Crypto.token` gains the URL-safe wire form for tokens riding paths and fragments; hex stays the fingerprint form.
- Anchors: `.api/oslojs-encoding.md` `encodeBase64urlNoPadding` member anchor.
- Atomic: one codec row.

[0011]-[QUEUED]: complete the webauthn policy and autofill surfaces in `libs/typescript/security/.planning/authn/webauthn.md` — `preferredAuthenticatorType` registration hint row, `verifyBrowserAutofillInput` gate on the autofill entry (idea `IDEAS.md` `[0007]`).
- Capability: authenticator-type preference becomes a trust-row policy value at options mint; conditional-UI input verification gates `Passkeys.autofill` beside the existing capability probe.
- Anchors: `.api/simplewebauthn-server.md` and `.api/simplewebauthn-browser.md` member anchors.
- Atomic: two option rows.

[0012]-[QUEUED]: back the partial-refresh planner in `libs/typescript/security/.planning/crypt/secret.md` with `secrets.list` (idea `IDEAS.md` `[0007]`).
- Capability: the planner diffs the full-object census against the held set so a metadata-only change never forces a value refetch; the admitted-surface roster in the page lead gains the row.
- Anchors: `.api/dopplerhq-node-sdk.md` `secrets.list` member anchor.
- Atomic: one admitted surface row.

[0013]-[QUEUED]: land the `SessionGuard` cookie-scheme row on `libs/typescript/security/.planning/authn/session.md` beside `BearerGuard` (idea `IDEAS.md` `[0008]`).
- Capability: platform security-decode over the access cookie, `Jwt.verify` fold, CSRF double-submit composed into one requirement provision, refusals on the `bearer` reject row.
- Shape: same `HttpApiMiddleware.Tag` grammar, `provides: CurrentClaims`; the runtime serve wave mounts it as one more guard row.
- Anchors: `HttpApiBuilder.securityDecode` (`libs/typescript/.api/effect-platform.md`), `CookieSpec`, `Cookie.verify`.

[0014]-[BLOCKED]: prove or refute instance-scoped `SettingsService`/`MetadataService` construction in `@simplewebauthn/server`, then land the trust-isolation ruling on `libs/typescript/security/.planning/authn/webauthn.md` (idea `IDEAS.md` `[0009]`).
- Capability: on proof, `WebAuthnTrust` becomes a per-layer configuration with zero process-global mutation; on refute, the page states the single-policy-per-process law as a composition constraint with the deployment-split remedy.
- Anchors: `.api/simplewebauthn-server.md` trust section, installed source under `node_modules/@simplewebauthn/server`.
- Atomic: one probe plus one ruling edit.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: reject-telemetry collapse — `crypt/verify.md` minted `Reject`, the one Convention-named reject stream with a kind discriminant; session/oauth/webauthn/credential re-keyed their reject arms onto it, `BearerGuard` gained the `bearer` row, and webauthn challenge refusals gained the `ceremony` row.
- [0002]-[COMPLETE]: wire-law instrument fold — seven page-local `security_*` mints (sign x5, secret, claim) converted to `Convention.instrument.*` rows; free-string `tenant`/`reason` tag keys re-keyed to `Convention.rasm.tenant`/`Convention.rasm.securityReason`; the tenant-cardinality obligation recorded as the `access -> runtime` `[PROJECTION]: rasm.tenant` seam.
