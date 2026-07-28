# [RASM_GRASSHOPPER_RULINGS]

`Rasm.Grasshopper` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- `Components` island admits no `EtoDispatch`/`GhSession` edge — GH2 solves component bodies off-thread under parallel iteration, so a UI-thread edge inside component logic is a threading crash, never a convenience; presentation stays with the UI-thread owners.
- Plugin identity is single-typed — `HookScope` is the one process-global plugin key, and `PlatformTelemetry.Open`'s plugin discriminator admits through it rather than a raw string parameter; a second ad-hoc string key on any per-plugin surface forks the identity space the `(point, scope)` hook registry and the `TelemetryDomain.Host.Measure("plugin")` resource attribute must share — the fork the Rhino boundary forecloses by typing `PluginKey`.
- App-root contributor ports for this folder carry a scope coordinate and no `InstrumentSpec` row, because `GhInstruments` mints every instrument on the per-ALC meter the capsule factory hands it; a parallel roster minted at the app root forks name, unit, and bucket advice against the meter holding them, so the port earns rows only once this folder declares specs rather than creates.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
