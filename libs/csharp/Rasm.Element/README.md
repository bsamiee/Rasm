# [RASM_ELEMENT]

`Rasm.Element` owns the neutral thing-model: the canonical property-graph element model at the lowest AEC-DOMAIN seam. One authoritative `ElementGraph` folds header, neutral node-and-relationship vocabulary, and a built-once incidence index; the consumer element is a memoized `Bake` over the reachable subgraph, never a second stored record; and the typed payload vocabularies are the one currency every discipline's data lands in, so a new discipline, payload, or relationship lands as a case on the neutral vocabulary, never a provider type.

Every AEC peer projects its foreign source through `IElementProjection`, Persistence holds the system of record, and the peer runtimes decode the wire bit-identically.

## [01]-[ROUTER]

[GRAPH]:
- [01]-[GRAPH](.planning/Graph/element.md): Property-graph spine — the frozen graph and the memoized `Bake` every consumer reads flat.
- [02]-[DELTA](.planning/Graph/delta.md): Mutation algebra — the live working graph, the edge law, and the persistable `GraphDelta` body.
- [03]-[WIRE](.planning/Graph/wire.md): Proto-first `rasm.element.v1` graph crossing — valid values lower, hostile input re-admits on `Fin<T>`.
- [04]-[WIRE_PAYLOAD](.planning/Graph/wirepayload.md): Node and edge envelope folds beside the header and object payload transcription arms.
- [05]-[WIRE_VALUE](.planning/Graph/wirevalue.md): Recursive value, measure, bag, and evidence-envelope transcription arms.
- [06]-[WIRE_SUBSTANCE](.planning/Graph/wiresubstance.md): Material composition, usage, and engineering-property transcription arms.
- [07]-[WIRE_EVIDENCE](.planning/Graph/wireevidence.md): Assessment and observation evidence transcription arms.
- [08]-[WIRE_RASTER](.planning/Graph/wireraster.md): Coverage, lattice, and georeference transcription arms.
- [09]-[CORPUS](.planning/Graph/corpus.md): Deterministic synthetic models — `CorpusProfile` closes the graded roster benchmarks and parity share.
- [10]-[TABLE](.planning/Graph/table.md): Columnar row family projection of the frozen snapshot and its `AnalyticsSchema` wire handoff.

[QUERY]:
- [11]-[PREDICATE](.planning/Query/predicate.md): Boolean selection closure `Predicate<TLeaf>` and its projected `PredicateKey` content key.

[RELATIONS]:
- [12]-[RELATION](.planning/Relations/relation.md): Neutral objectified-edge algebra and the sub-kind vocabularies the graph spine composes.

[CLASSIFICATION]:
- [13]-[CLASSIFICATION](.planning/Classification/classification.md): Generic `Classification` system-and-code pair and the shared discipline axis.

[PROPERTIES]:
- [14]-[PROPERTY](.planning/Properties/property.md): Typed IFC-value vocabulary and the `InheritanceMode` type→occurrence precedence fold.
- [15]-[QUANTITY](.planning/Properties/quantity.md): Seven-SI-exponent signature and the `MeasureValue` carrier with neutral uncertainty bounds.

[COMPOSITION]:
- [16]-[MATERIAL](.planning/Composition/material.md): `MaterialComposition` family and the discipline-keyed engineering-property rows.
- [17]-[ACOUSTIC](.planning/Composition/acoustic.md): Banded acoustic carrier and the shared `RatingContour` single-number contour-fit kernel.

[ASSESSMENT]:
- [18]-[ASSESSMENT](.planning/Assessment/assessment.md): Generic `AssessmentPayload` analysis receipt keyed by discipline, route, and input.
- [19]-[OBSERVATION](.planning/Assessment/observation.md): `ObservationSeries` measured sensor evidence, the computed receipt's sibling modality.

[GEOSPATIAL]:
- [20]-[COVERAGE](.planning/Geospatial/coverage.md): `CoverageGrid` by-reference raster-and-field descriptor over the kernel `CellLattice` placement.
- [21]-[REFERENCE](.planning/Geospatial/reference.md): Map-conversion-and-CRS `GeoReference` record over the three-state projected-CRS identity.

[PROJECTION]:
- [22]-[PROJECTION](.planning/Projection/projection.md): Cross-stratum projector and constraint floors under one `Assemble` composition apps wire.
- [23]-[ADDRESS](.planning/Projection/address.md): `ContentAddress` codec and order-independent graph addressing over the kernel seed-zero hash.
- [24]-[FAULTS](.planning/Projection/fault.md): `ElementFault` codes on the kernel band registry beside the `AdmissionSlots` accumulating fold.
- [25]-[OBSERVE](.planning/Projection/observe.md): `ElementHooks` tap surface — typed graph facts onto the kernel rail, fanned into `GraphInstrument`.
- [26]-[AUDIT](.planning/Projection/audit.md): `ModelAudit` completeness grade folding coverage ratios and integrity sweeps into one typed receipt.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `Generator.Equals` — Structural equality and member diff feeding the 3-way merge.
- `JetBrains.Annotations`
- `LanguageExt.Core`
- `NodaTime` — Instant stamps on assessments, provenance, and headers.
- `QuikGraph` — Built-once incidence and topology view over the graph.
- `Riok.Mapperly` — Source-generated seam↔wire case transcription.
- `System.IO.Hashing` — Kernel seed-zero content-hash seed.
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json` — JSON boundary transcription for smart-enum and value-object types.
- `UnitsNet` — Quantity-type registry and SI-coercion boundary.

[OBSERVABILITY]:
- `Microsoft.Extensions.Compliance.Abstractions` — `rasm.element` taxonomy over the wire's classified columns; contract-only, no redactor resolves.

[WIRE_CODEGEN]:
- `Google.Protobuf` — `rasm.element.v1` message flow and payload-limit gate.
- `Grpc.Tools` — Build-only proto codegen; never a runtime surface.
- `NodaTime.Serialization.Protobuf` — `Instant` wire crossing on the `Graph/wire` headers.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — `CloudEvent` values behind the kernel message-envelope owner, minted per consuming binding.
