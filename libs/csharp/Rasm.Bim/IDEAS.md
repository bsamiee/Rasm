# [RASM_BIM_IDEAS]

The forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

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

[UNIT_SCHEME_BIM_COUNTERPART]-[BLOCKED]: Carry the model-unit presentation scheme across the cross-runtime wire.
- Capability: a peer runtime decoding `HeaderWire` reads the model's declared display units without re-sniffing the IFC bytes.
- Shape: the seam `Graph/wire` `HeaderWire` gains the additive `Units` map field at the wire unfreeze; the Bim ingest (`UnitsOf` on `Projection/semantic`) and egress (the `EmitContext` declared-regime raise on `Projection/egress`) ends are landed and read the field with zero further edits.
- Unlocks: schedules and UI on the TypeScript peers render project units off one wire read.
- Anchors: the landed `Header.Units` `UnitScheme` policy both Bim ends compose; `api-geometrygym-ifc` catalogs `IfcUnitAssignment.ScaleSI`.
- Tension: the `HeaderWire` field is gated on the wire unfreeze window — the frozen wire never widens outside it.
- Ripple: `Rasm.Element` `[UNIT_SCHEME_BIM_COUNTERPART]`.

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[QUEUED]: Land the Bim lowering and re-materialization for the seam-landed `Connect.Interface` content key.
- Capability: `IfcConnectionGeometry` and `IfcRelSpaceBoundary2ndLevel` interface surfaces ride the graph as content-keyed typed geometry instead of stranding in `Generic` attributes.
- Shape: the `Projection/relations` projector hashes the interface surface into the blob store and stamps the key on the `Connect` edge (`IfcRelConnectsElements.ConnectionGeometry` and the 2nd-level space-boundary route off `Generic`); the `Projection/egress` re-materializes it; the seam `Graph/element` `GeometrySource` gains the typed decode leg (curve interface → `AxisCurve`, surface → `FootprintPolygon`).
- Unlocks: Compute reads connection-interface geometry one-hop by content key; re-exported analysis models keep their boundary surfaces.
- Anchors: `Connect.Interface` landed seam-side (presence-delimited canonical bytes, additive `ConnectWire` field); the egress eccentricity path already reconstitutes `IfcConnectionGeometry` STEP fragments from the ctor-held profiles store — the same lane.
- Ripple: `Rasm.Element` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Round-trip the `IfcPhysicalComplexQuantity` `Discrimination`/`Quality`/`Usage` grouping identity once the seam `QuantityBag` carries a group axis.
- Capability: complex-quantity grouping identity survives the dot-path flatten — a formwork quantity grouped by `Discrimination` re-emits with its grouping strings instead of prefix-only reconstruction.
- Shape: the `Projection/semantic` flatten stamps the group-axis rows the seam carrier adds; the `Projection/egress` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children from them.
- Unlocks: value-lossless AND identity-lossless complex-quantity round-trip; QTO consumers select by grouping axis.
- Anchors: `Projection/semantic` `FlattenQuantities` already recurses dot-path keys value-lossless and names the residual; the decompile-verified `Discrimination`/`Quality`/`Usage` public strings.
- Tension: the seam column ripples the counted-bag canonical-bytes injectivity law, the frozen wire, and the `Bake` merge — the seam owner lands first.
- Ripple: `Rasm.Element` `[QUANTITY_BAG_GROUP_AXIS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TEMPLATE_AUDIT_VALIDATION_TIER]-[COMPLETE]: Ruled — `Review/validation` widened to the two-tier QA owner: `ModelHealth`/`ModelFinding` compose the `TemplateFinding` stream as the baseline tier beneath authored IDS, the case the tier discriminant, one verdict surface for `Rasm.AppUi` and the review pipeline.
