# [TS_CORE_RULINGS]

`typescript/core` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- Instrument-to-`Metric` materialization homes at the `Convention` owner, not the consumer sites — one factory maps each instrument row's kind, unit, and boundary spec to its live handle; a per-site constructor pick re-spells the boundary vectors at every consumer and drifts the contract the row already declares.

## [03]-[COLLAPSE]

- `FaultClass`, `Budget`, and `Degrade` stay three row families under the one fault module — a shared declaration-time table generator is assembly machinery each family reuses, never a merge; folding them into one parameterized owner erases the class/retry/degrade policy split and mints a god-vocabulary, denser on paper and weaker at every policy seam.
- `Degrade` and `Availability` stay two owners — `Degrade` compiles local connection-silence policy into probe cadence, `Availability` lands the peer-minted degradation snapshot the serving gate types against; folding them writes a local liveness policy into peer-minted wire evidence and breaks the single-writer law the snapshot decodes under.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
