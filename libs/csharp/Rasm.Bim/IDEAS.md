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

[UNIT_SCHEME_BIM_COUNTERPART]-[QUEUED]: Land the Bim ends of the model-unit round-trip over the seam-landed `UnitScheme` header policy.
- Capability: an IFC model's declared display units (`IfcUnitAssignment`) survive ingest and egress as presentation policy while the interior stays SI-canonical.
- Shape: the `Projection/semantic` ingress lowers `IfcUnitAssignment` onto `Header.Units` (quantity-type token → declared unit token, beside the existing numeric `UnitScale` coercion); the `Projection/egress` re-emits the declared units instead of forcing SI; the seam `Graph/wire` `HeaderWire` gains the additive map field at the wire unfreeze.
- Unlocks: a mm-declared Revit export renders and re-exports in its own units off one policy read; schedules and UI drop per-call-site unit picks.
- Anchors: `UnitScale` already walks the assignment per base axis; `api-geometrygym-ifc` catalogs `IfcUnitAssignment.ScaleSI`; the seam `Header.Units` trailing default keeps existing construction sites compiling.
- Tension: the `HeaderWire` field is gated on the wire unfreeze — the ingest/egress ends land first, the wire field with the next unfreeze window.
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

[TEMPLATE_AUDIT_VALIDATION_TIER]-[BLOCKED]: Rule where the spec-free `TemplateAudit` baseline surfaces beside the buildingSMART IDS v1.0 lane.
- Capability: one model-health entry answering both the zero-configuration template audit and the authored-IDS verdict without forking report consumers.
- Shape: either `Review/validation` absorbs the `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` stream as its baseline tier beneath any authored IDS, or model-health stays the properties page's own surface and `Rasm.AppUi` composes the two reports.
- Unlocks: a single QA verdict surface for `Rasm.AppUi` and the review pipeline.
- Anchors: `TemplateAudit.Run` landed with the axis-named `TemplateVerdict` vocabulary; the IDS owner's facet lane narrows the SAME constraint family into `ValueConstraint`.
- Tension: blocked on the validation-owner charter ruling — the IDS owner's charter is strictly buildingSMART IDS v1.0, so absorbing a spec-free tier widens it; the ruling wants an interview, not a guess.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
