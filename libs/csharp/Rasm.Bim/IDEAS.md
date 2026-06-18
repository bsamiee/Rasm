# [RASM_BIM_IDEAS]

The forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

## [1]-[OPEN]

[IDS_VALIDATION_OWNER]: a `validation` sub-domain owning the buildingSMART IDS v1.0 XSD-bound model-validation contract.
- An `InformationDelivery` specification record parsed from and authored to the IDS XSD, its facets (Entity, Attribute, Property, Classification, Material, PartOf) folded into one closed `IdsFacet` `[Union]` that reuses the `query/element-set` `ElementPredicate` algebra over `BimModel`, with execution routed to the IfcOpenShell ifctester companion for deterministic cross-tool audit.
- Unlocks model-quality gating, requirement-driven exchange acceptance, and a deterministic `IdsAudit` receipt that composes the existing predicate fold — the validation predicate IS the query predicate, never a new selection surface.
- Draws on IDS v1.0 (June 2024), the openBIM requirements-checking standard with a published audit test-suite that guarantees identical results across checkers; the C# folder owns no validation concept while the Python companion already does.

[BCF_COORDINATION_GRAPH]: a `coordination` sub-domain owning BCF 3.0 issue exchange and GlobalId-stable federation diff.
- A closed `BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds, a self-owned `.bcfzip` archive codec and the BCF-API REST projection living inside `coordination` (BCF is an issue/coordination container, never a geometry-or-model interchange row, so it is NOT a row on the `InterchangeFormat` geometry axis), plus a `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms.
- Unlocks issue round-trip with any CDE or viewer, clash and coordination workflows, and incremental federation diff that reuses the Compute content-key to dedup unchanged elements — the diff joins by GlobalId plus content-key, no second identity scheme.
- Draws on BCF 3.0, the openBIM issue/coordination wire (file plus REST, IFC-GUID-anchored viewpoints), and federated-model diff as the multi-discipline coordination primitive; coordination turns a single semantic model into a multi-party federation; the issue payload aligns to `model/elements` GlobalIds only, never to the geometry codec axis.

[BSDD_DICTIONARY_BINDING]: re-found the `classification` axis on the live buildingSMART Data Dictionary service.
- `Classification` rows resolve dictionary URIs (Uniclass/OmniClass/IFC and 300+ dictionaries), `ClassificationCode` validates against the dictionary's published class shape, and bSDD property definitions drive the Pset mapping rather than a hardcoded code-shape regex per system.
- Unlocks authoritative classification and property-definition lookup shared by `classification`, `properties`, and `validation`; a new standard becomes a dictionary-URI row, not a new code-shape table, and the bSDD class-to-property mapping feeds the Pset owner directly.
- Draws on bSDD (ISO 12006-3/23386/23387, 300+ dictionaries, free, RESTful and GraphQL), the canonical live source that also drives IDS classification facets and bSDD-referenced properties; hardcoding code shapes drifts from the authoritative dictionary.

[PROPERTY_QUANTITY_OWNER]: a `properties` sub-domain promoting Pset/Qto from raw rows to a first-class owner.
- A `PropertySet`/`QuantitySet` keyed vocabulary over the standard `Pset_*`/`Qto_*` definitions, occurrence- vs type-driven quantity semantics, base-quantity derivation, and round-trip through `IfcRelDefinesByProperties`, composed by the `query/element-set` `ByProperty` predicate and the IDS Property facet.
- Unlocks quantity takeoff, standard-Pset round-trip fidelity, type-vs-occurrence property inheritance, and a single property model the query predicate, the IDS property facet, and the bSDD binding all compose — never a second property store.
- Draws on the standard buildingSMART Pset/Qto definitions as the dominant BIM data payload and the basis of quantity takeoff and IDS property checks; today the data exists only as flat `PropertyRow`/`QuantityRow` projections with no domain owner.

[GEOREFERENCED_FEDERATION]: a `georeferencing` sub-domain owning the IFC4.3 coordinate-reference surface.
- Project `IfcMapConversion`/`IfcProjectedCRS`/`IfcCoordinateOperation` onto a host-neutral `GeoReference` record (eastings, northings, orthogonal height, X-axis rotation, scale, EPSG/CRS name), reconciled into the canonical kernel frame by `FrameNormalization` at ingest so every federated model shares one georeferenced origin.
- Unlocks real-world-coordinate federation, IFC4.3 infrastructure georeferenced placement, and correct cross-model clash detection; the `GeoReference` record extends `FrameNormalization` with a translation/rotation/scale leg rather than a new transform surface.
- Draws on IFC4.3 georeferencing as mandatory for openBIM federation and infrastructure (roads/bridges/rail); the folder normalizes only up-axis and handedness today and carries no CRS or map-conversion concept, so federated models cannot share a real-world frame. GeometryGym exposes the full `IfcMapConversion`/`IfcProjectedCRS` surface.

[UNIVERSAL_WIRE_PROJECTION]: a host-free wire projection of the BIM semantic model that the Python and TypeScript peers decode.
- A JSON wire surface over the generated `BimElement`/`IfcClass`/`ElementPredicate`/`AssemblyRel`/`InterchangeFormat` owners through the Thinktecture JSON converters, so the closed families serialize by their key or case discriminant rather than a hand-authored DTO mirror, and a `BimModel` snapshot crosses the wire as one content-keyed payload joined to the Compute `InterchangeIdentity`.
- Unlocks the cross-runtime contract the architecture names: the host-free Python `ifcopenshell` companion and the TypeScript web consumer decode the same `BimModel` vocabulary the C# branch mints, never re-minting a parallel BIM shape, and the BCF coordination diff and the IDS audit ride the same wire payload.
- Draws on the monorepo wire law (one canonical concept, one owner, decoded at the boundary) and the Thinktecture `Thinktecture.Runtime.Extensions.Json` generated-owner serialization; today the generated owners carry no wire converter, so a peer runtime cannot consume the semantic model without a second mint.

[BIM_FAULT_OWNER]: a `faults` sub-domain owning the `BimFault` band as a first-class closed `[Union]`.
- `BimFault.ModelRejected` is constructed on every design page (`model/elements`, `query/element-set`, `classification/systems`, `assembly/spatial-structure`, `exchange/*`) yet no `faults/` sub-domain, no `BimFault` owner page, and no `ARCHITECTURE.md` sub-domain charter declares it — the fault type is an implied owner with no banded home, unlike the sibling `Rasm.Fabrication/faults#FAULT_BAND` `FabricationFault` and `Rasm.Materials` `ProfileFault`/`ConstructionFault`/`MaterialFault` bands.
- Unlocks a single typed BIM fault band with its own code range, an `Expected`-derived `IValidationError<BimFault>` lift into the `Fin<T>` rail every Bim entrypoint returns, and a closed case family (model-rejected, unmapped-class, dangling-reference, codec-reject, capability-miss) the pages already imply through their `ModelRejected` detail strings, rather than one catch-all case carrying a stringly-typed discriminant.
- Draws on the band-ownership pattern the AEC-domain siblings already realize (Fabrication band-2500, Materials bands 2300/2350/2400); the Bim band slots into the same scheme so a Bim fault lowers onto the rail identically to a kernel `GeometryFault` and the case is recoverable by code, never by message matching.

## [2]-[CLOSED]

None.
