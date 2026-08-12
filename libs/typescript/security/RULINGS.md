# [TS_SECURITY_RULINGS]

`typescript/security` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `openid-client` is the sole OAuth custodian — browser ceremony and machine grants share its `Configuration`s; `arctic` retired, no successor.
- `otplib` omits `getRemainingTime`/`getTimeStepUsed` — a `@otplib/totp` admission prices a package for two modulo lines, so `_PERIOD` owns the leg.
- `@oslojs/crypto` retired on publisher deprecation — `@noble/hashes` owns sync symmetric, WebCrypto async verify, `@oslojs/encoding` the wire codec.

## [02]-[SHAPE]

- `Intake` holds the octets an inbound signature verifies — re-encoding respells floats, key order, and escapes, authenticating what no peer sent.
- Credential storage rides the material's entropy class — guessable material earns argon2, random mints the SHA-256 compare; no table walks them.

## [03]-[COLLAPSE]

- Auth throttling is ONE posture its wirings share, seated folder-local — folding into the branch's three erases the `Reject` counting it publishes.
- Breach and admit arms tap inline under ONE kind — the arm's correction leads, `Witness` stays off `Reject.mark`, and `Reject.measured` joins them.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
