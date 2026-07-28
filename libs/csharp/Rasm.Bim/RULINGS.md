# [RASM_BIM_RULINGS]

`Rasm.Bim` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `GeometryGymIFC_Core` sole-IFC-model-surface KEEP — do NOT consolidate the IFC model surface onto the admitted xBIM leaves: none carries the full IFC4.3 entity vocabulary with schema-versioned STEP/ifcXML/ifcJSON write the egress re-author rides, so consolidation trades the one `DatabaseIfc` authority for a capability hole the leaf packages cannot fill; reopens only on an xBIM release owning that full write surface.

## [02]-[SHAPE]

- Ingested `IfcClass`/`PredefinedType` tokens admit BARE at `SemanticProjector` ingress, validity deferring to the `Emit` egress gate [PREDEFINED_TOKEN_RULING] — ingress validation aborts a whole import on one unknown entity and forks the token vocabulary between ingress and egress.
- Every `IfcRel*` name, directionality, and inverse-attribute pair lives on `IfcRelKind` rows lowering onto the neutral `Relationship` edge, the typed case carrying only `SubKind` and `Generic` alone carrying wire-name and attribute bag [NEUTRAL_EDGE_RULING] — a typed `IfcRel*` seam case leaks GeometryGym below the seam and forks the neutral edge algebra.
- Content keys ride ONE kernel seed-zero hasher across every federation, solver, cache, and diff edge — a per-page hash, a second scheme, or a `Guid`-keyed join forks the content space Compute's content-addressing lane shares, and a downward `InterchangeIdentity` reference from Bim inverts the strata.
- Model identity is SPAN-grade, never a metric dimension [MODEL_SLOT_RULING] — models mint unbounded, so `rasm.bim.model` multiplies every instrument by the live model count while a sampler-thinned span carries it free; the slot carries the package namespace a sibling also needs, `BimTelemetry.Traced` stamps it from its OWN required argument — a slot left to caller discipline is the slot no caller stamps — and "just the active model" re-mints that cardinality behind a bounded-sounding qualifier.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
