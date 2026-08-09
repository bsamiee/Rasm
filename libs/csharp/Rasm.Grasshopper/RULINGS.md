# [RASM_GRASSHOPPER_RULINGS]

`Rasm.Grasshopper` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- `Components` admits no `EtoDispatch` or `GhSession` edge — GH2 solves component bodies off-thread, so a UI-thread edge inside one is a crash.
- Contributor-port instruments DECLARE, never bind — `GhInstruments` mints on the per-ALC meter, so specs ride `Published` and `Instruments` is empty.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
