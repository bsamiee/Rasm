# [RASM_RHINO_RULINGS]

`Rasm.Rhino` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- (none)

## [03]-[COLLAPSE]

- `RhinoInstrumentPartition` and `RhinoInstruments` stay separate — the partition is kind-keyed receipt-to-instrument projection data declared at the boundary and executed at the app root, while `RhinoInstruments` is the contributed-meter-row port under one custody; folding them into one instrument owner erases the declared-projection-versus-contributed-mechanism split, denser on paper and weaker in both guarantees.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
