# [PERSISTENCE_RULINGS]

`Rasm.Persistence` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `age` (Apache AGE) admits `ExtensionAdmission.Standalone`, never `Preload` — `CREATE EXTENSION age` succeeds with no `shared_preload_libraries` and the per-session `LOAD 'age'` is a runtime connection concern, not an install gate; re-adding `Preload("age")` — the sweep move reading AGE's `_PG_init` planner/parse hooks as a preload requirement — makes the verification fold emit a spurious `MissingPreload` `shared_preload_libraries` diff for a correctly per-session-`LOAD` deployment, and re-litigation opens only when AGE upstream hard-errors on `CREATE EXTENSION` without preload.

## [02]-[SHAPE]

- `ProjectionContext` is the one Persistence time frame — an AppHost `ClockPolicy` parameter on a Persistence signature is the named strata inversion, sinking an app-platform policy type into the store's public surface where the injected frame already carries the sampled instant; re-litigation opens only if `ProjectionContext` stops carrying that instant.
- `Store/Schema` composes contract artifacts only from owners this package holds — a sibling AEC peer contributing an artifact row reverses the strata the seam fixes, so a peer's durable shape enters as an `ElementGraph` projection this package then declares, never as a foreign artifact the contract admits directly.
- `Crdt.Apply` and `GraphDelta.Apply` are the only materializers — projection, live merge, and AS-OF reconstruction each fold the one delta, and a second materializer beside them forks replay from live state so the two disagree silently at the first conflicting op.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- `ElementSet` membership widening re-cuts the frozen `elementset` parity vector in the same pass — the length-framed content-address and its `ContentParityCorpus.Contribute(ParitySlot.ElementSet, …)` freeze are one atomic contract; a membership change landed without the re-freeze diverges one selection's hash across the three runtimes.
