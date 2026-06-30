# [RASM_ELEMENT]

`Rasm.Element` is the lowest AEC-DOMAIN seam package — inserted between the KERNEL (`Rasm`) and the AEC peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) — owning the canonical property-graph element model both Materials and Bim project onto WITHOUT referencing each other. The authoritative thing is the `ElementGraph` (a `Header` + a `FrozenDictionary<NodeId, Node>` + an `ImmutableArray<Relationship>` + a built-once incidence index), and the consumer-facing `Element` is a derived fold `Bake(objectNode)` over the reachable subgraph — never a second stored record, so "has it all" is one flat read. It owns the `Node` `[Union]` property-graph vocabulary (`Object`/`Material`/`PropertySet`/`QuantitySet`/`Assessment`/`Appearance`/`Coverage`), the NEUTRAL `Relationship` edge algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void` plus a `Generic` passthrough — never the seventeen `IfcRel*` cases), the typed value vocabulary (`PropertyValue`/`MeasureValue` over a 7-SI-dimension `Dimension`/`PropertyName`/`PropertyBag`/`QuantityBag` with an `InheritanceMode` precedence fold), the generic `Classification` and the one `Discipline` axis, the `Material` composition and property family plus the intrinsic acoustic folds, the generic `Assessment` receipt, the geospatial `Coverage` and the twelve-tuple `GeoReference`, the `NodeId`/`ContentAddress` content-identity over one canonical codec, and the `IElementProjection`/`IGraphConstraint` cross-stratum contracts. It is the ALIGNMENT mechanism: peers NEVER reference each other; alignment travels through these contracts and the content-keyed graph, each package usable in ISOLATION (Materials projects a material subgraph, Bim the IFC projection, Persistence persists any `ElementGraph`). It references the `Rasm` kernel ONLY — references NO GeometryGym, carries NO host geometry (geometry is held by content hash), and is DISTINCT from `Rasm.Fabrication` (manufacturing/CAM). The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/Projection/fault.md): `ElementFault` closed `[Union]` band 2500 (`NodeAbsent`/`RelationshipInvalid`/`DeltaConflict`/`ValueRejected`/`ProjectionFailed`/`AddressUnstable`), `Expected`-derived so every seam entrypoint lowers a bare typed case onto the `Fin<T>` rail.
- [02]-[GRAPH](.planning/Graph/element.md): the `NodeId` `[ValueObject<string>]` identity owner (rooted Guid-v7 / non-rooted `XxHash128` content hash), the `Node` `[Union]` seven-case property-graph vocabulary, the `ToCanonicalBytes` shared canonical projection, the `RepresentationContentHash` keyed geometry map plus the `AxisCurve`/`FootprintPolygon` analytical-geometry shapes and the `GeometrySource` resolution port (the content key → analytical shape contract an above-seam runner resolves one-hop), the `Header`, the frozen `ElementGraph` with the built-once incidence index and `QuikGraph` topology view, and the memoized `Bake` derived fold.
- [03]-[DELTA](.planning/Graph/delta.md): the `GraphMutation` `[Union]` request + total `Switch`, the HAMT `WorkingGraph` live form, the structural edge law, the `Apply` fold, the `GraphDelta` monoid event body, and the `ReplayOnto` persistence fold.
- [04]-[RELATION](.planning/Relations/relation.md): the `Relationship` `[Union]` neutral edge algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void` + `Generic` passthrough), the neutral sub-kind vocabularies, and the `MaterialUsage` occurrence payload the `Associate` edge carries.
- [05]-[CLASSIFICATION](.planning/Classification/classification.md): the generic `Classification` `[ComplexValueObject]` system-and-code pair an `Object` node carries, and the one `Discipline` `[SmartEnum<string>]` analysis axis the property, assessment, and analysis route share.
- [06]-[PROPERTY](.planning/Properties/property.md): the `PropertyValue` `[Union]` typed IFC-value family, the `PropertyName` key, the `PropertyBag`/`QuantityBag` named bags, and the `InheritanceMode` type→occurrence `Merge` precedence fold.
- [07]-[QUANTITY](.planning/Properties/quantity.md): the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature, the `QuantityType` `[ValueObject<string>]` discriminator (the `UnitsNet` `QuantityInfo.Name`, since the 7-vector is not injective over quantity types), and the `MeasureValue` SI-coerced scalar carrier with its type-checked QTO accessors over the `UnitsNet` registry.
- [08]-[MATERIAL](.planning/Composition/material.md): the `MaterialId` key, the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`) with the neutral `ProfileRef`, and the `MaterialPropertySet` `[Union]` typed engineering-property family keyed to `Discipline`.
- [09]-[ACOUSTIC](.planning/Composition/acoustic.md): the `AcousticBand` one-third-octave-centre vocabulary, the banded `Acoustic` carrier, the `Nrc`/`Saa`/`StcWeighted`/`Rw` pure projection folds, and the shared `RatingContour` `Fit` single-number contour-fit kernel (`Stc`/`Rw` rows differing by data alone).
- [10]-[ASSESSMENT](.planning/Assessment/assessment.md): the generic `AssessmentPayload` analysis receipt keyed by `Discipline`+`AnalysisRoute`+`InputKey`, the `AnalysisRoute` opaque route token, the `AssessmentOutcome` lifecycle, the typed `Results` bag and failure `Detail`, and the content-keyed `ResultBlob`/`Provenance`.
- [11]-[COVERAGE](.planning/Geospatial/coverage.md): the `CoverageGrid` by-ref raster/field descriptor, the `CoverageKind` discriminant, the `CoverageBand` per-band schema, and the affine `GridDescriptor`.
- [12]-[REFERENCE](.planning/Geospatial/reference.md): the `GeoReference` twelve-tuple coordinate-reference record, the `ProjectedCrs` `[ValueObject<string>]` EPSG parse, and the `Admit` fault-on-unresolvable factory.
- [13]-[PROJECTION](.planning/Projection/projection.md): the `IElementProjection` projector floor and its one `Project`, the `IGraphConstraint` IFC-semantic legality floor and its one `Validate`, the `ProjectionContext`, and the `Assemble` composition capability the apps wire.
- [14]-[ADDRESS](.planning/Projection/address.md): the `CanonicalWriter` deterministic byte codec and the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, with order-independent graph addressing.

## [02]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder composes; full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`. The folder admits NO domain packages — it is the host-neutral, provider-free seam; GeometryGym, VividOrange, NetTopologySuite, and the IFC/geospatial codecs all live in the AEC peers that project onto this graph.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[GRAPH_ALGORITHM]:
- `QuikGraph`

[BOUNDARY_TRANSCRIPTION]:
- `Riok.Mapperly` — compile-time `ElementGraph`↔DTO/proto/row mapping; `[MapDerivedType]` lowers a `Node`/`Relationship` `[Union]` to its wire case, the seam owning identity/hash/rail and Mapperly only the boundary transcription.
- `Generator.Equals` — compile-time structural equality and the `Inequalities` member-level diff over the `ElementGraph` snapshot, feeding the Persistence 3-way `StructuralMerge`; complements, never replaces, the kernel `XxHash128` content-addressing rail.

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
