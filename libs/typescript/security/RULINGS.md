# [TS_SECURITY_RULINGS]

`typescript/security` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `arctic` and `openid-client` are two admitted OAuth custodians neither supersedes — `arctic` owns the browser authorization-code ceremony and its provider redirect rows, `openid-client` the certified machine-grant surface (client-credentials, token exchange, DPoP, introspection) no browser row reaches; a supersession sweep retiring either is the refuted move — the capability planes are disjoint; reopens only when an `authn/workload.md` realization proves one client drives the other's ceremony without loss.

## [02]-[SHAPE]

- Inbound-signature verification computes over the exact request octets held at the edge before any body parse — a re-encoded body respells floats, key order, and escapes, authenticating a document the peer never sent; `Intake` is the held-octets seam the runtime serve wave mounts, and decoding the body ahead of verify is the foreclosed move.

## [03]-[COLLAPSE]

- Auth throttling is ONE posture across its five wirings — `session` refresh, `credential` otp and api-key, `webauthn` assert-finish, `verify` — and its policy-row surface stays folder-local beside the `credential._throttled` fold: merging up into the branch three-posture limiter erases the auth posture's reject-stream counting, and a survey reading the five as irreducible hand-wirings re-litigates the collapse.
- Breach arms spell state-correction → `Reject.mark` → `Witness.publish` → error-log → typed-fail inline at each site, never extracted into a shared breach helper: the correction preceding the evidence tap is arm-specific (`store.revokeSubject` collapses the reused family; the clone arm's correction is its counter guard), and `Reject` (lossy metric stream) and `Witness` (durable fact rail) are decoupled channels a merged helper entangles into one call.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
