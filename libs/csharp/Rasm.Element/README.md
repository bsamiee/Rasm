# [RASM_ELEMENT]

`Rasm.Element` is the lowest AEC-DOMAIN seam package — inserted between the KERNEL (`Rasm`) and the AEC peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) — owning the canonical property-graph element model both Materials and Bim project onto WITHOUT referencing each other. The authoritative thing is the `ElementGraph` (a `Header` + a `FrozenDictionary<NodeId, Node>` + an `ImmutableArray<Relationship>` + a built-once incidence index), and the consumer-facing `Element` is a derived fold `Bake(objectNode)` over the reachable subgraph — never a second stored record, so "has it all" is one flat read. The `Object` node carries an `ObjectKind`: a Component projects a deterministic-rooted Type `Object` (the `NodeId` derived from the Component's canonical content, geometry excluded so a later attach never re-keys it), a model author mints an Occurrence, and `Bake` folds an occurrence together with the Component data it inherits through the `Assign.TypeDefinition` edge — the named type→occurrence precedence — into the one flat `Element`, surfacing the `TypeId` a downstream generator reads to recover which Component a piece realizes. The full data lives once on the Type and the baked occurrence has it all; an Element is placement and overrides, never a re-described Component. It owns the `Node` `[Union]` property-graph vocabulary (`Object`/`Material`/`PropertySet`/`QuantitySet`/`Assessment`/`Appearance`/`Coverage`), the NEUTRAL `Relationship` edge algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void` plus a `Generic` passthrough — never the seventeen `IfcRel*` cases), the typed value vocabulary (`PropertyValue`/`MeasureValue` over a 7-SI-dimension `Dimension`/`PropertyName`/`PropertyBag`/`QuantityBag` with an `InheritanceMode` precedence fold), the generic `Classification` and the one `Discipline` axis, the `Material` composition and property family plus the intrinsic acoustic folds, the generic `Assessment` receipt, the geospatial `Coverage` and the map-conversion-and-CRS `GeoReference` (a three-state `ProjectedCrs` over EPSG/WKT/projection), the `NodeId`/`ContentAddress` content-identity over one canonical codec, the `rasm.element.v1` proto wire and its `WireCodec`/`ElementWire` codec, and the `IElementProjection`/`IGraphConstraint` cross-stratum contracts. `MeasureValue` carries SI value identity, optional neutral uncertainty bounds, and optional distribution metadata; `MaterialPropertySet` carries row evidence; `ValueBag` carries source precedence. Package-specific uncertainty libraries stay above the seam. It is the ALIGNMENT mechanism: peers NEVER reference each other; alignment travels through these contracts and the content-keyed graph, each package usable in ISOLATION (Materials projects a material subgraph, Bim the IFC projection, Persistence persists any `ElementGraph`). It references the `Rasm` kernel ONLY — references NO GeometryGym, carries NO host geometry (geometry is held by content hash), and is DISTINCT from `Rasm.Fabrication` (manufacturing/CAM). The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/Projection/fault.md): the `FaultBand` `[SmartEnum<int>]` cross-federation band-allocation registry (one row per band with the `Owner` telemetry column, duplicate integers failing at type initialization, every peer band's `Code` rebinding to a row) and `ElementFault` closed `[Union]` band 2500 (`NodeAbsent`/`RelationshipInvalid`/`DeltaConflict`/`ValueRejected`/`ProjectionFailed`/`AddressUnstable`), `Expected`-derived so every seam entrypoint lowers a bare typed case onto the `Fin<T>` rail.
- [02]-[GRAPH](.planning/Graph/element.md): the `NodeId` `[ValueObject<string>]` identity owner (rooted Guid-v7 / non-rooted `XxHash128` content hash), the `Node` `[Union]` seven-case property-graph vocabulary, the `ToCanonicalBytes` shared canonical projection, the `RepresentationContentHash` keyed geometry map plus the `AxisCurve`/`FootprintPolygon` analytical-geometry shapes and the `GeometrySource` resolution port (the content key → analytical shape contract an above-seam runner resolves one-hop), the `Header`, the frozen `ElementGraph` with the built-once incidence index and `QuikGraph` topology view, and the memoized `Bake` derived fold.
- [03]-[DELTA](.planning/Graph/delta.md): the `GraphMutation` `[Union]` request + total `Switch`, the HAMT `WorkingGraph` live form, the structural edge law, the `Apply` fold, the `GraphDelta` monoid event body, and the `ReplayOnto` persistence fold.
- [04]-[WIRE](.planning/Graph/wire.md): the `rasm.element.v1` `Graph/element.proto` `oneof` wire contract, the `WireCodec` Mapperly per-case transcription family with verbatim key codecs, and the `ElementWire` `Fin`-railed `Encode`/`DecodeGraph`/`DecodeDelta` boundary under the `CodedInputStream.CreateWithLimits` depth/size gate — keys crossing verbatim, peers reproducing and never re-minting.
- [05]-[RELATION](.planning/Relations/relation.md): the `Relationship` `[Union]` neutral edge algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void` + `Generic` passthrough), the neutral sub-kind vocabularies, and the `MaterialUsage` occurrence payload the `Associate` edge carries.
- [06]-[CLASSIFICATION](.planning/Classification/classification.md): the generic `Classification` `[ComplexValueObject]` system-and-code pair an `Object` node carries, and the one `Discipline` `[SmartEnum<string>]` analysis axis the property, assessment, and analysis route share.
- [07]-[PROPERTY](.planning/Properties/property.md): the `PropertyValue` `[Union]` typed IFC-value family, the `PropertyName` key, the `PropertyBag`/`QuantityBag` named bags, the `PropertySource` source-rank stamp, and the `InheritanceMode` type→occurrence `Merge` precedence fold.
- [08]-[QUANTITY](.planning/Properties/quantity.md): the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature, the `QuantityType` `[ValueObject<string>]` discriminator (the `UnitsNet` `QuantityInfo.Name`, since the 7-vector is not injective over quantity types), and the `MeasureValue` SI-coerced scalar carrier with optional neutral `MeasureBand` uncertainty bounds, optional distribution metadata, and type-checked QTO accessors over the `UnitsNet` registry.
- [09]-[MATERIAL](.planning/Composition/material.md): the `MaterialId` key, the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) with the neutral `ProfileRef`, the `PropertyEvidence` row-evidence stamp, and the `MaterialPropertySet` `[Union]` typed engineering-property family keyed to `Discipline`.
- [10]-[ACOUSTIC](.planning/Composition/acoustic.md): the `AcousticBand` one-third-octave-centre vocabulary, the banded `Acoustic` carrier, the `Nrc`/`Saa`/`StcWeighted`/`Rw` pure projection folds, and the shared `RatingContour` `Fit` single-number contour-fit kernel (`Stc`/`Rw` rows differing by data alone).
- [11]-[ASSESSMENT](.planning/Assessment/assessment.md): the generic `AssessmentPayload` analysis receipt keyed by `Discipline`+`AnalysisRoute`+`InputKey`, the `AnalysisRoute` opaque route token, the `AssessmentOutcome` column-driven lifecycle (`Usable`/`Terminal`/`Dispatchable` rows with the `Next()` flip adjacency one `Advance` validates), the typed `Results` bag and typed failure `Diagnostic` (`SolvePhase`/`FailureKind`), and the content-keyed `ResultBlob`/`Provenance` (solver `Elapsed`/`Window`/`Correlation`).
- [12]-[COVERAGE](.planning/Geospatial/coverage.md): the `CoverageGrid` by-ref raster/field descriptor, the `CoverageKind` discriminant, the `CoverageBand` per-band schema, and the affine `GridDescriptor`.
- [13]-[REFERENCE](.planning/Geospatial/reference.md): the `GeoReference` map-conversion-and-CRS record, the three-state `ProjectedCrs` `[ComplexValueObject]` CRS identity (authority `Name`+parsed `Epsg`, inline `Wkt`, `MapProjection`/`MapZone`), the `CrsResolution` `[SmartEnum<string>]` resolution-mode column, and the `Admit` fault-on-fully-unresolvable factory.
- [14]-[PROJECTION](.planning/Projection/projection.md): the `IElementProjection` projector floor and its one `Project`, the `IGraphConstraint` IFC-semantic legality floor and its one `Validate`, the `ProjectionContext`, and the `Assemble` composition capability the apps wire.
- [15]-[ADDRESS](.planning/Projection/address.md): the `CanonicalWriter` deterministic byte codec and the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, with order-independent graph addressing.

## [02]-[DOMAIN_PACKAGES]

The folder admits NO domain packages — it is the host-neutral, provider-free seam; GeometryGym, VividOrange, NetTopologySuite, and the IFC/geospatial codecs all live in the AEC peers that project onto this graph.

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder composes; full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.Protobuf` — `Instant`↔`google.protobuf.Timestamp`/`Duration` crossings on the `Graph/wire` headers; interior code stays NodaTime.
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[GRAPH_ALGORITHM]:
- `QuikGraph`

[BOUNDARY_TRANSCRIPTION]:
- `Riok.Mapperly` — seam↔wire transcription only: `WireCodec` `[Mapper]` case mappings + `[UserMapping]` key codecs on the generated `Switch`/`PayloadCase`.
- `Google.Protobuf` — `rasm.element.v1` wire messages: `IMessage<T>`/`MessageParser<T>` flow, `CreateWithLimits` payload gate, `ByteString` content-key carrier.
- `Grpc.Tools` — build-only `<Protobuf>` message codegen for `Graph/element.proto` (`GrpcServices=None`, `PrivateAssets=all`); never a runtime surface.
- `Generator.Equals` — structural equality + `Inequalities` member diff, feeding the 3-way `StructuralMerge`; never replaces the `XxHash128` rail.
