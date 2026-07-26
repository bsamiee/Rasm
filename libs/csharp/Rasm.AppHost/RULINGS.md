# [RASM_APPHOST_RULINGS]

`Rasm.AppHost` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- (none)

## [03]-[COLLAPSE]

- `Idempotency` and `HopIdempotency` stay two typed owners, never one merged smart-enum — discriminant: the op row keys END-TO-END command repeat-safety while the hop row keys PER-HOP transport dedup, and the row sets do not interchange (`NonIdempotent` exists only op-side where no dedup key exists, `MethodDerived` only hop-side as HTTP-method-derived safety with no op-level meaning); a dedup sweep reading the capability page's one-repeat-safety-semantic clause as a merge license erases the two dedup scopes — that clause means the `Keyed` row carries one meaning on both layers, never one type serving both edges; reopens only on proof that a single dedup scope serves both the command edge and the transport hop.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
