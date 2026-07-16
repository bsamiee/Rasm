# [RASM_ELEMENT]

`Rasm.Element` owns the canonical property-graph element model at the lowest AEC-DOMAIN seam — the federation's neutral thing-model. One authoritative `ElementGraph` folds header, neutral node-and-relationship vocabulary, and a built-once incidence index; the consumer element is a memoized `Bake` over the reachable subgraph rather than a second stored record; and the typed payload vocabularies — property, quantity, material, acoustic, assessment, coverage, classification, georeference — are the one currency every discipline's data lands in. Its bar is federation-grade neutrality: every AEC peer projects its foreign source onto this graph through `IElementProjection`, Persistence persists it as the system of record, and the peer runtimes decode its wire bit-identically, so a new discipline, payload, or relationship lands as a case on the neutral vocabulary, never a provider type.

AEC peers project onto this graph and never reference each other — alignment travels through the cross-stratum contracts and the content-keyed wire. This seam references the kernel alone, carries no host geometry and no IFC stack, and re-mints no identity the kernel owns.

## [01]-[ROUTER]

[GRAPH]:
- [01]-[GRAPH](.planning/Graph/element.md): Property-graph spine — the frozen graph and the memoized `Bake` every consumer reads flat.
- [02]-[DELTA](.planning/Graph/delta.md): Mutation algebra — the live working graph, the edge law, and the persistable `GraphDelta` body.
- [03]-[WIRE](.planning/Graph/wire.md): `rasm.element.v1` content-key-preserving crossing every peer runtime decodes under the seam admission gate.

[RELATIONS]:
- [04]-[RELATION](.planning/Relations/relation.md): Neutral objectified-edge algebra and the sub-kind vocabularies the graph spine composes.

[CLASSIFICATION]:
- [05]-[CLASSIFICATION](.planning/Classification/classification.md): Generic `Classification` system-and-code pair and the shared discipline axis.

[PROPERTIES]:
- [06]-[PROPERTY](.planning/Properties/property.md): Typed IFC-value vocabulary and the `InheritanceMode` type→occurrence precedence fold.
- [07]-[QUANTITY](.planning/Properties/quantity.md): Seven-SI-exponent signature and the `MeasureValue` carrier with neutral uncertainty bounds.

[COMPOSITION]:
- [08]-[MATERIAL](.planning/Composition/material.md): `MaterialComposition` family and the discipline-keyed engineering-property rows.
- [09]-[ACOUSTIC](.planning/Composition/acoustic.md): Banded acoustic carrier and the shared `RatingContour` single-number contour-fit kernel.

[ASSESSMENT]:
- [10]-[ASSESSMENT](.planning/Assessment/assessment.md): Generic `AssessmentPayload` analysis receipt keyed by discipline, route, and input.

[GEOSPATIAL]:
- [11]-[COVERAGE](.planning/Geospatial/coverage.md): By-ref `CoverageGrid` raster and field descriptor over a band schema and affine grid.
- [12]-[REFERENCE](.planning/Geospatial/reference.md): Map-conversion-and-CRS `GeoReference` record over the three-state projected-CRS identity.

[PROJECTION]:
- [13]-[PROJECTION](.planning/Projection/projection.md): `IElementProjection` and graph-constraint floors and the assemble composition apps wire.
- [14]-[ADDRESS](.planning/Projection/address.md): `ContentAddress` codec and order-independent graph addressing over the kernel seed-zero hash.
- [15]-[FAULTS](.planning/Projection/fault.md): Cross-federation band registry and the `ElementFault` union lowered onto the result rail.

## [02]-[DOMAIN_PACKAGES]

No domain package lands here — the host-neutral, provider-free seam. GeometryGym, VividOrange, NetTopologySuite, and the IFC and geospatial codecs all live in the AEC peers that project onto this graph.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[QUANTITY_GRAPH]:
- `UnitsNet` — quantity-type registry and SI-coercion boundary.
- `QuikGraph` — built-once incidence and topology view over the graph.

[IDENTITY_TIME]:
- `System.IO.Hashing` — kernel seed-zero content-hash seed.
- `NodaTime` — instant stamps on assessments, provenance, and headers.
- `NodaTime.Serialization.Protobuf` — `Instant` wire crossing on the `Graph/wire` headers.

[WIRE_TRANSCRIPTION]:
- `Google.Protobuf` — `rasm.element.v1` message flow and payload-limit gate.
- `Grpc.Tools` — build-only proto codegen; never a runtime surface.
- `Riok.Mapperly` — source-generated seam↔wire case transcription.
- `Generator.Equals` — structural equality and member diff feeding the 3-way merge.
- `Thinktecture.Runtime.Extensions.Json` — JSON boundary transcription for smart-enum and value-object types.
