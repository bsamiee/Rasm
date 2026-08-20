# [TS_SECURITY_RULINGS]

`typescript/security` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `openid-client` is the sole OAuth custodian — browser ceremony and machine grants share its `Configuration`s; `arctic` retired, no successor.
- `otplib` omits `getRemainingTime`/`getTimeStepUsed` — a `@otplib/totp` admission prices a package for two modulo lines, so `_PERIOD` owns the leg.
- `@oslojs/crypto` retired — `@noble/hashes` and WebCrypto already own its whole surface; `@oslojs/encoding` stays the wire codec.

## [02]-[SHAPE]

- `Intake` holds the octets an inbound signature verifies — re-encoding respells floats, key order, and escapes, authenticating what no peer sent.
- Credential storage rides the material's entropy class — guessable material earns argon2, random mints the SHA-256 compare; no table walks them.
- Every credential-verify surface is throttled and telemetered structurally — an unthrottled surface publishes no `Reject` denominator.
- Rejected credentials are verdict arms, never faults — a fault channel hides the rejection count the throttle spends.
- KDF cost claims leave as core `Board.Claim` receipts — the `BenchmarkClaimWire` landing gated by `Board.Claim.matches` host admission.

## [03]-[COLLAPSE]

- Growth is one table row per provider, dialect, surface, or role — a sibling owner beside its table forks the throttle and telemetry it inherits.
- Auth throttling is ONE posture its wirings share, seated folder-local — folding into the branch's three erases the `Reject` counting it publishes.
- Breach and admit arms tap inline under ONE kind — a split kind severs the join `Reject.measured` makes between correction and count.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
