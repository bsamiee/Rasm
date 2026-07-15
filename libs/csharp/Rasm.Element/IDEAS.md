# [RASM_ELEMENT_IDEAS]

The forward pool of higher-order concepts for the lowest AEC-DOMAIN element seam. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[UNIT_SCHEME_BIM_COUNTERPART]-[QUEUED]: Close the model-unit round-trip — the Bim ends of the landed `UnitScheme` header seam.
- Capability: an IFC model's declared display units (the `IfcUnitAssignment`) survive ingest, wire, and egress as presentation policy while the interior stays SI-canonical.
- Shape: the seam half is LANDED — `Graph/element` `Header.Units` (trailing `UnitScheme = default`, `CanonicalBytes`-excluded as presentation, the `StepHeader`-exclusion mirror) over the `Properties/quantity` `UnitScheme` (`QuantityType` token → `UnitInfo.Name`, `Render` composing `MeasureValue.In` through the `Registry` index). The open counterparts: the Bim ingress lowers `IfcUnitAssignment` onto `Header.Units` (quantity-type token → declared unit token beside the existing numeric `UnitScale` coercion), the egress re-emits the declared units instead of forcing SI, and `Graph/wire` `HeaderWire` gains the additive map field at the wire unfreeze.
- Unlocks: a mm-declared Revit export renders and re-exports in its own units off one policy read; schedules and UI drop per-call-site unit picks.
- Anchors: `Header.Units` trailing default (existing construction sites compile unchanged); `UnitScale` already walks the assignment per base axis; `api-geometrygym-ifc` catalogs `IfcUnitAssignment.ScaleSI`.
- Ripple: `Rasm.Bim` `[UNIT_SCHEME_BIM_COUNTERPART]` (the `Projection/semantic` + `Projection/egress` ends); `Graph/wire` additive `HeaderWire` field.

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[QUEUED]: Close the connection-interface round-trip — the Bim lowering and the typed decode leg for the landed `Connect.Interface` key.
- Capability: `IfcConnectionGeometry` and `IfcRelSpaceBoundary2ndLevel` interface surfaces ride the graph as content-keyed typed geometry instead of stranding in `Generic` attributes.
- Shape: the seam half is LANDED — `Relations/relation` `Connect` carries `Option<UInt128> Interface` (presence-delimited in `CanonicalBytes`, additive `ConnectWire` bytes field, a content key never a `NodeId`, `Members` unchanged). The open counterparts: the Bim projector hashes the interface surface into the blob store and stamps the key on the `Connect` edge (`IfcRelConnectsElements.ConnectionGeometry` and the 2nd-level space-boundary route off `Generic`), the egress re-materializes it, and a `Graph/element` `GeometrySource` typed leg decodes it (curve interface → `AxisCurve`, surface → `FootprintPolygon`).
- Unlocks: Compute reads connection-interface geometry one-hop by content key; re-exported analysis models keep their boundary surfaces.
- Anchors: `Connect.Interface` landed; the egress eccentricity path already reconstitutes `IfcConnectionGeometry` STEP fragments from the ctor-held profiles store — the same lane.
- Ripple: `Rasm.Bim` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]` (the `Projection/relations` lowering + `Projection/egress` re-materialization); `Graph/element` `GeometrySource` decode leg.

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Carry the complex-quantity grouping identity on `QuantityBag` — the group axis the IFC `IfcPhysicalComplexQuantity` round-trip needs.
- Capability: a quantity row can belong to a named group with `Discrimination`/`Quality`/`Usage` identity strings, so grouped takeoffs survive the graph identity-lossless, not merely value-lossless under dot-path prefixes.
- Shape: one group-axis carrier on the `Properties/property#PROPERTY_BAG` `QuantityBag` (a group row or per-row group column), threaded through `ToCanonicalBytes` and the seam `Bake` merge.
- Unlocks: the Bim projector stamps grouping identity at ingest and the egress rebuilds nested complex quantities; QTO consumers select by group.
- Anchors: `Rasm.Bim` `Projection/semantic` `FlattenQuantities` already recurses value-lossless and names this as its one residual row; the bag's 4-column ValueBag shape admits an additive axis.
- Tension: the column ripples the counted-bag canonical-bytes injectivity law, the frozen wire, and the `Bake` merge — a seam-owner design addition, never a consumer-side patch.
- Ripple: `Rasm.Bim` `[QUANTITY_BAG_GROUP_AXIS]` (the ingest/egress ends).

[FEDERATION_AND_PARTIAL_EXCHANGE]-[BLOCKED]: `Federate(models)` disjoint-id union and `Extract(roots)` Members-closed reachable-subgraph extraction on `ElementGraph`.
- Capability: multi-model federation and partial-model exchange as first-class graph operations — a coordination model unions discipline models without id collision, and a scoped deliverable extracts a closed subgraph.
- Shape: `Federate` the disjoint-id union over per-source graphs; `Extract` the reachable closure over `Members`-completeness so no edge dangles out of the slice.
- Unlocks: federated coordination review and partial IFC deliverables over the one graph spine.
- Anchors: `ElementGraph` frozen snapshot + incidence index; `NodeId` regime keeps rooted ids globally unique (Guid-v7 / content-hash).
- Tension: blocked on the singular-`Header` reconciliation ruling (per-source schema/tolerance/georeference) — the ruling shapes the design and wants an interview, not a guess.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
