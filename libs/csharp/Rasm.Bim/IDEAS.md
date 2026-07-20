# [RASM_BIM_IDEAS]

Forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

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

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[QUEUED]: Land the Bim lowering and re-materialization for the seam-landed `Connect.Interface` content key.
- Capability: `IfcConnectionGeometry` and `IfcRelSpaceBoundary2ndLevel` interface surfaces ride the graph as content-keyed typed geometry instead of stranding in `Generic` attributes.
- Shape: the `Projection/relations` projector hashes the interface surface into the blob store and stamps the key on the `Connect` edge (`IfcRelConnectsElements.ConnectionGeometry` and the 2nd-level space-boundary route off `Generic`); the `Projection/egress` re-materializes it; the landed seam decodes the interface key through the one `GeometrySource.ResolveFootprint` leg — never a third port.
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

[BCF_API_RESPONSE_ADMISSION]-[QUEUED]: Close the BCF-API round-trip with the response half of the REST projection.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, and the response-body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint` — so a CDE round-trip reads one owner.
- Shape: `Review/issues` gains the response peer of `BcfResource` → `BcfApiRequest`: one response carrier discriminated by resource, the snake-case body admission reusing `BcfApiContext`, the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`, server-authored viewpoints landing beside `.bcfzip` imports, conflict-aware `ReviseTopic` round-trips.
- Anchors: `BcfResource`/`BcfApiRequest`/`BcfApiContext` landed; `BcfWireMapper` owns the archive↔wire correspondence the response admission reuses.
- Tension: the transport port and retry/auth policy are Compute's — the response admission consumes returned bytes and never mints a second transport owner.

[BRICK_SYSTEMS_OPERATIONS_OVERLAY]-[BLOCKED]: Compose the admitted `BrickSchema.Net` ontology as the building-systems-operations overlay on `Model/systems`, once the app-platform live-binding leg lands.
- Capability: a Brick `Point`/`Equipment`/`Location` graph overlays the static `SystemTrace` connectivity, mapping IFC `MonitoringSystem` occurrences onto Brick `Fedby`/`PointOf`/`PartOf`/`LocationOf` relations so operations consumers read one semantic systems model.
- Shape: `Model/systems` gains a Brick projector lowering the `DistributionSystem` reach set onto Brick classes; the live-point binding (`BACnetReference`/`BACnetDevice`/`ModbusDevice`) stays a reference the app-platform resolves, never a transport minted here.
- Unlocks: BMS-aware clash and coordination, live-versus-design systems reconciliation, operations-phase handover beyond COBie.
- Anchors: `BrickSchema.Net` admitted (README `[DOMAIN_VOCABULARY]`, `.api/api-brickschema-net.md`); `Model/systems` owns the static `SystemTrace` connectivity the overlay reads.
- Tension: the live-binding transport (OPC UA / MQTT / Modbus) is app-platform by strata law — Bim owns only the static ontology projection, and the overlay reads `SystemTrace`, never re-minting a second connectivity store beside it.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TEMPLATE_AUDIT_VALIDATION_TIER]-[COMPLETE]: Ruled — `Review/validation` widened to the two-tier QA owner: `ModelHealth`/`ModelFinding` compose the `TemplateFinding` stream as the baseline tier beneath authored IDS, the case the tier discriminant, one verdict surface for `Rasm.AppUi` and the review pipeline.

[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `HeaderWire.unit_scheme = 7` is landed on the seam `Graph/wire` with both Mapper legs, and the Bim ingest (`UnitsOf`) and egress (`DeclareUnits`) ends read the field — the `Rasm.Element` counterpart card closed with it.
