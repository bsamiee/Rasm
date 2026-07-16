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

[UNIT_SCHEME_BIM_COUNTERPART]-[BLOCKED]: Carry the model-unit presentation scheme across the cross-runtime wire.
- Capability: a peer runtime decoding `HeaderWire` reads the model's declared display units without re-sniffing the IFC bytes.
- Shape: the seam half is LANDED — `Graph/element` `Header.Units` (trailing `UnitScheme = default`, `CanonicalBytes`-excluded as presentation, the `StepHeader`-exclusion mirror) over the `Properties/quantity` `UnitScheme` (`QuantityType` token → `UnitInfo.Name`, `Render` composing `MeasureValue.In` through the `Registry` index) — and the Bim ingest (`UnitsOf` on `Projection/semantic`) and egress (the `EmitContext` declared-regime raise on `Projection/egress`) ends are landed; the open counterpart is the `Graph/wire` `HeaderWire` additive map field at the wire unfreeze.
- Unlocks: schedules and UI on the TypeScript peers render project units off one wire read.
- Anchors: the landed `Header.Units` both Bim ends compose; `api-geometrygym-ifc` catalogs `IfcUnitAssignment.ScaleSI`.
- Tension: the `HeaderWire` field is gated on the wire unfreeze window — the frozen wire never widens outside it.
- Ripple: `Rasm.Bim` `[UNIT_SCHEME_BIM_COUNTERPART]`.

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

[OBSERVATION_SERIES]-[BLOCKED]: A monitored/measured time-series evidence modality beside the computed assessment receipt — the operating-asset record the model keeps past handover.
- Capability: sensor telemetry (temperature, humidity, energy meters, structural-health strain, occupancy) attaches to the elements it observes as typed observation-series evidence, so measured data folds into the same `Bake` read, diffs under the same merge, crosses the same wire, and the commissioning comparison (declared U-value vs metered heat flux, predicted vs metered energy) is a graph query, never an external historian join.
- Shape: one new `Node` case (`Observation`) wrapping a series descriptor — observed `QuantityType` token, sampling cadence and observation `Interval` (NodaTime), sensor provenance, and a content-keyed series blob — attached through the existing `Assign` algebra as one `AssignKind` row, the descriptor's `CanonicalBytes` co-located on the payload per the `AssessmentPayload` discipline.
- Unlocks: the digital-twin/commissioning lane over the one graph spine; `Rasm.Compute` computed-vs-measured comparison routes reading both evidence kinds off one baked element.
- Anchors: the by-reference heavy-payload pattern is proven twice (`Geospatial/coverage` `RasterKey`, `Assessment/assessment` `ResultBlob`); NodaTime is admitted substrate; `LegalAssign` and `Bake` each grow by one row/arm.
- Tension: a new `Node` case is a new `NodeWire` oneof arm — the `rasm.element.v1` wire is campaign-frozen, so the case lands at the wire unfreeze beside the queued `NodeWire` column adds; a seam-only landing would strand the node at every crossing.

[REDACTION_SCOPED_EGRESS]-[BLOCKED]: Sensitivity-classed wire egress — share the model, withhold the commercial and personal columns.
- Capability: partner-scoped exchange as a first-class egress mode — one model, N lawful projections: unit costs and lifecycle rates (commercial secrets), `OwnerHistory`/`Provenance` authors (GDPR-class personal data), and supplier-confidential EPD references cross only to the peers a policy admits, the redaction typed and auditable instead of a per-deal hand-stripped copy.
- Shape: a sensitivity classification on the known columns (`CostWire`, `OwnerHistoryWire`, `ProvenanceWire`, the EPD evidence rows) plus a `WireLimits`-style redaction policy record parameterizing `ElementWire.Encode` — redacted fields unset through the proto3 optional/unset forms, zero wire-schema change — composing the admitted `libs/csharp/.api/api-redaction.md` substrate catalog.
- Unlocks: lawful federation-partner deliverables and discipline packages off one stored model; the redaction substrate catalog earns its Element consumer.
- Anchors: `Encode` is the one egress fold every crossing takes; proto3 presence semantics already model absence; the `Object` canonical bytes already exclude `OwnerHistory`, so that column redacts identity-inert.
- Tension: some classified columns FOLD into node content ids (`MaterialPropertySet.CaseBytes` writes the EPD `PropertyEvidence`; `AssessmentPayload` excludes `Provenance` but a `Material` node's id folds its property sets) — whether a redacted crossing preserves the source content keys (breaking the peer's `ContentAddress.Verify` re-hash on redacted nodes) or re-derives them (forking identity off the source model) is the unresolved ruling that shapes the design; the decode-side `AddressUnstable` posture and the cross-runtime parity corpus both hang on it.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
