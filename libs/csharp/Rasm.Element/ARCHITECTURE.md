# [RASM_ELEMENT_ARCHITECTURE]

The domain map of `Rasm.Element` — the lowest AEC-DOMAIN seam between the KERNEL (`Rasm`) and the AEC peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`). The `Graph`, `Relations`, `Classification`, `Properties`, `Composition`, `Assessment`, `Geospatial`, and `Projection` sub-domains, each composing the one `ElementGraph` and lowering onto the one `ElementFault` band, the peers depending UP on the `IElementProjection`/`IGraphConstraint` contracts and aligning by the content-keyed graph rather than by referencing each other.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Element/             # refs ../Rasm ONLY; no GeometryGym; no host geometry (geometry by content hash)
├── Graph/                # The authoritative property graph and its mutation algebra
│   ├── Element.cs        # The property-graph spine — dual-seeded NodeId (Occurrence Guid-v7 / rooted Type, geometry excluded), the Node [Union], the Bake fold
│   ├── Delta.cs          # The mutation algebra — GraphMutation [Union] + total Switch over the HAMT WorkingGraph, structural edge law, persistable GraphDelta
│   ├── Wire.cs           # The content-key-preserving crossing — Fin-railed Encode/Decode under decode-budget gate, Mapperly WireCodec family; keys verbatim
│   └── element.proto     # The language-neutral rasm.element.v1 oneof contract — Grpc.Tools compiles it (GrpcServices=None); TS/Python compile the SAME proto
├── Relations/            # The neutral objectified-edge algebra
│   └── Relation.cs       # The neutral edge algebra — Relationship [Union] (Compose/Assign/Associate/Connect/Void + Generic), never the seventeen IfcRel* cases
├── Classification/       # The neutral cross-cutting axes
│   └── Classification.cs # The cross-cutting axes — the generic Classification [ComplexValueObject] system+code, the one Discipline [SmartEnum] analysis axis
├── Properties/           # The typed property/quantity value vocabulary
│   ├── Property.cs       # The typed value vocabulary — PropertyValue [Union] IFC-value family over PropertyName, PropertySource rank, InheritanceMode merge
│   └── Quantity.cs       # The physical-quantity carrier — the 7-SI-exponent Dimension, MeasureValue SI coercion over UnitsNet, and MeasureBand bounds
├── Composition/          # The material composition and intrinsic acoustic folds
│   ├── Material.cs       # The material composition — MaterialId, MaterialComposition [Union] (+ProfileRef), PropertyEvidence, MaterialPropertySet [Union]
│   └── Acoustic.cs       # The intrinsic acoustic folds — the one-third-octave Acoustic carrier, the Nrc/Saa/StcWeighted/Rw folds, the shared RatingContour.Fit
├── Assessment/           # The generic analysis receipt
│   └── Assessment.cs     # The generic analysis receipt — AssessmentPayload keyed by Discipline+AnalysisRoute+InputKey, AssessmentOutcome lifecycle, Results
├── Geospatial/           # The georeferenced coverage and CRS
│   ├── Coverage.cs       # The georeferenced coverage — the by-ref CoverageGrid raster/field, the CoverageBand schema, the affine GridDescriptor; bytes by key
│   └── Reference.cs      # The map-conversion-and-CRS record — the three-state ProjectedCrs [ComplexValueObject] over EPSG/WKT/projection, CrsResolution, Admit
└── Projection/           # The cross-stratum contracts, the content codec, and the fault band
    ├── Projection.cs     # The cross-stratum contracts — IElementProjection projector floor, IGraphConstraint legality floor, ProjectionContext, Assemble
    ├── Address.cs        # The content codec — the CanonicalWriter byte projection and ContentAddress [ValueObject<UInt128>] over kernel seed-zero XxHash128
    └── Fault.cs          # The fault band — the FaultBand [SmartEnum<int>] allocation registry and ElementFault [Union] band 2500 every entrypoint lowers onto
```

The `Graph` sub-domain is the spine: `Element.cs` declares the `Node`/`NodeId`/`Header`/`ElementGraph` and the `Bake` fold; `Delta.cs` the mutation algebra and the persistable `GraphDelta`; `Wire.cs` + `element.proto` the one content-key-preserving crossing every peer runtime decodes, keys verbatim and admission through the seam gates. Every other sub-domain owns a `Node` case payload or a cross-cutting value the spine composes — `Relations` the edges, `Classification`/`Properties`/`Composition`/`Assessment`/`Geospatial` the typed payloads, `Projection` the contracts, the content codec, and the fault band. The `Composition/Material` page composes `Properties/Quantity` (the `MeasureValue` columns) and `Composition/Acoustic` (the banded carrier); the `Graph/Element` `Bake` composes every payload owner and applies both the named type→occurrence inheritance (single fields occurrence-overrides-type, `Seq` fields union + dedup-by-key) and the `Properties/Property` `InheritanceMode` bag merge; the `Projection/Address` codec is the one canonical projection the `NodeId.Content` mint, the `ContentAddress` diff, and the `Generator.Equals` content comparison all share. The seam re-mints nothing the kernel owns: the content-identity seed is the kernel `XxHash128` seed-zero entry, the op-key the kernel `Op`, the fault base the kernel `Expected`.

## [02]-[SEAMS]

```text seams
Graph/element         ← csharp:Rasm.Bim/Projection         # [PROJECTION]: SemanticProjector lowers GeometryGym → GraphDelta; Emit stays Bim-internal
Graph/element         ← csharp:Rasm.Materials/Projection   # [PROJECTION]: mints the deterministic-rooted Type Object, binds occurrences, authors the subgraph
Graph/element         ← csharp:Rasm.Fabrication/Projection # [PROJECTION]: a future third IElementProjection, one registration row
Graph/element         ⇄ csharp:Rasm.Materials/Component    # [SHAPE]: owner-mints-its-identity law + named Bake type→occurrence inheritance + TypeId on Element
Projection            ← csharp:Rasm.Bim                    # [PORT]: IGraphConstraint IFC legality (containment/void/type-aggregation), after the structural law
Graph/element         → csharp:Rasm.Compute                # [SHAPE]: reads the concrete graph; geometry one-hop via GeometrySource; writes Assessment.Result
Composition/acoustic  → csharp:Rasm.Compute                # [SHAPE]: RatingContour.Fit shared for ISO 12354 layered-STC; the AssemblyAggregator runs in Compute
Assessment/assessment ← csharp:Rasm.Compute                # [SHAPE]: Compute runs the route, writes Computed Node.Assessment keyed (Discipline,Route,InputKey)
Composition/material  → csharp:Rasm.Compute                # [SHAPE]: composition plies feed the AssemblyAggregator; SectionProperties feed design-code routes
Composition/material  → csharp:Rasm.Materials/Component    # [PROJECTION]: ProfileRef one-hop to the Component-owned ComputedSection (VividOrange catalog)
Composition/material  → csharp:Rasm.Bim                    # [SHAPE]: Cost per-unit doubles + ISO-4217 Currency meet the Bim NodaMoney algebra at the 5D join
Geospatial/reference  ← csharp:Rasm.Bim/Semantics          # [PROJECTION]: GeoReferenceProjector folds IFC CRS → GeoReference; seam owns Admit+fault, Bim fills
Geospatial/coverage   ← csharp:Rasm.Bim/Semantics          # [PROJECTION]: GeoModel lowers vector/raster → Object/Coverage; NTS/GDAL in Bim, metadata on seam
Properties/property   ← csharp:Rasm.Bim/Semantics          # [PROJECTION]: Pset roster + bSDD + IFC round-trip in Bim; seam owns value shape + InheritanceMode
Properties/property   ⇄ csharp:Rasm.Materials/Component    # [SHAPE]: DetailSchema.Realization/.Product rows the ComponentProjector authors, no Pset_* literal
Properties/property   ⇄ csharp:Rasm.Bim/Semantics          # [SHAPE]: the ONE neutral DetailSchema + PropertyName Materials authors, Bim egresses to Pset
Properties/property   ⇄ csharp:Rasm.Bim/Semantics          # [SHAPE]: PropertySource ranks the value bags after InheritanceMode selects type-vs-occurrence
Properties/quantity   → csharp:Rasm.Bim/Semantics          # [SHAPE]: MeasureValue carries the Bim QuantityDerivation.Derive base-quantity yield (derived-wins)
Composition/material  ⇄ csharp:Rasm.Materials/Properties   # [SHAPE]: PropertyEvidence rides each MaterialPropertySet row; only MeasureValue/MeasureBand cross
Composition/material  ⇄ csharp:Rasm.Bim/Semantics          # [PROJECTION]: IFC material sets ↔ MaterialComposition/ProfileRef; profiles → IIfcProfileStore
Composition/material  ⇄ csharp:Rasm.Bim/Semantics          # [CONTENT_KEY]: AppearanceSummary ↔ Materials Appearance at content key, no direct reference
Composition/material  → csharp:Rasm.Materials/Projection   # [SHAPE]: seam MaterialComposition/MaterialUsage cases — the absorbed CompositionAuthor
Graph/element         ← csharp:Rasm.Materials/Appearance   # [CONTENT_KEY]: a library row lowered to the content-keyed AppearanceSummary at full precision
Graph/element         ⇄ csharp:Rasm.Compute/Runtime        # [CONTENT_KEY]: RepresentationContentHash shares the kernel seed-zero; InterchangeIdentity DISTINCT
Properties/property   ⇄ csharp:Rasm.Compute/Symbolic       # [SHAPE]: seam Dimension ℤ⁷ measure ↔ DimensionMonomial ℚ⁷ proof, both from UnitsNet BaseDimensions
Projection            → csharp:Rasm.Materials/Projection   # [SHAPE]: IElementProjection contract set + IGraphConstraint validate gate the projector composes
Graph/delta           ⇄ csharp:Rasm.Persistence/Element    # [SHAPE]: GraphDelta is GraphEvent body; GraphProjection replays → ElementGraph, snapshot-bounded
Graph/element         ⇄ csharp:Rasm.Persistence/Element    # [SHAPE]: ElementGraph store-load roundtrip; Generator.Equals diff feeds the 3-way StructuralMerge
Projection/address    ⇄ csharp:Rasm                        # [CONTENT_KEY]: composes the kernel seed-zero XxHash128 entry; the ONE hasher, no second hasher
Projection/address    → csharp:Rasm.Persistence            # [CONTENT_KEY]: ContentAddress keys geometry/raster blobs; Persistence composes seam CanonicalWriter
Projection            ← csharp:Rasm.AppHost                # [PORT]: ProjectionContext neutral primitives (clock/CorrelationId/TenantId) at app composition root
Graph/element         ⇄ python:geometry/mesh               # [WIRE]: imported-IFC GLB seed-zero XxHash128 == RepresentationContentHash; companion never re-mints
Graph/element         ← csharp:Rasm.Persistence/Ingest     # [WIRE]: tabular row shape only; the per-app composition root maps rows onto ElementGraph nodes
Graph/element         ← csharp:Rasm.Persistence/Ingest     # [WIRE]: geospatial feature row shape, mirroring the tabular law; app root owns the geo→element map
Projection/address    ⇄ python:runtime/evidence            # [CONTENT_KEY]: seed-zero ContentAddress parity — MATERIAL_LAYER_GOLDEN pins CanonicalWriter.Measure
Graph/wire            ⇄ typescript:core/interchange/codec  # [WIRE]: rasm.element.v1 proto the codec decodes under core/interchange/contract ContractDrift gate
Graph/wire            ⇄ python:geometry/ifc                # [WIRE]: the SAME rasm.element.v1 contract the companion decodes via grpcio-tools; keys verbatim
```

The `[PROJECTION]` rows are the INVERSION OF CONTROL: GeometryGym, VividOrange, and every provider stay in the AEC peers, which implement `IElementProjection` and lower their foreign source onto a `GraphDelta` — no provider edge points down into the seam, no second IFC or section-property stack. Each provider OWNS its concept and MINTS its own `Object` identity (the owner-mints-its-identity law): the `Rasm.Materials` `Component` projection mints the deterministic-rooted Type `Object` AND stamps its `Classification`/`PredefinedType`, a model author mints Occurrence `Object`s, and `Rasm.Bim` ingests `IfcElementType`→the SAME Type `Object` and `IfcElement`→Occurrence, so a minter never stamps a foreign projector's egress and the one type representation is authored and ingested unified. The acyclic strata is preserved: every AEC peer references `{Rasm, Rasm.Element}` (a shared LOWER stratum, the same shape as depending on the kernel), and peers never reference each other; the live element assembly (registering the `Seq<IElementProjection>`, binding the tessellation adapter, running `Assemble` against a live source) is an APP / HOST-BOUNDARY composition-root concern, the seam owning the `Assemble` capability and the apps the wiring.

[CONTENT_KEY_IDIOM]:
- every page that joins the persistence, projection, assessment-cache, or diff lane derives a typed `UInt128` through the `Projection/address#CONTENT_ADDRESS` `XxHash128.HashToUInt128` (seed zero) over the one `CanonicalWriter` projection, never a second identity scheme; the `RepresentationContentHash` (geometry by content hash), the `Coverage.RasterKey`, the `Assessment.InputKey`, and the graph snapshot `ContentAddress.OfGraph` all address one content space shared with the kernel `GeometryHash` and the Python/TypeScript peers.
- a second hasher, a non-zero seed, a per-page hash function, or a `Guid`-keyed content join is the named cross-folder drift defect — the seam composes the kernel's one `XxHash128` seed-zero entry and the cross-runtime parity corpus (a float-bearing `IfcMaterialLayer`-shaped node) anchors byte-for-byte agreement; the `Graph/wire` crossing carries every content key VERBATIM (X32 `NodeId` strings, 16-byte big-endian `UInt128` `bytes` fields), so the codec never re-derives an identity on either side of the wire.
- a non-rooted node id is the self-hash of the node's OWN `ToCanonicalBytes` via `Graph/element#NODE_MODEL` `NodeId.Content` (the form the `Projection/address#CONTENT_ADDRESS` `Verify` dual recomputes and the parity corpus pins); `NodeId.OfContent(addr)` is valid ONLY when `addr == ContentAddress.Of(node.ToCanonicalBytes())` (it skips the re-hash), so feeding it a FOREIGN precomputed key — an `Assessment` `InputKey` over (route, targets, policy) rather than the node's self-hash — stores an id `Verify` cannot reproduce and is the named drift; the canonical `Assessment` node id is `NodeId.Content(ToCanonicalBytes(Discipline, Route, InputKey))`, the `InputKey` a payload field the triple folds.
- the `Object` node's rooted `NodeId` carries ONE regime, TWO seedings keyed by `Graph/element#NODE_MODEL` `ObjectKind`: an Occurrence's id is its unique placement identity (a Guid-v7), a Type's id is DETERMINISTICALLY derived through the SAME kernel seed-zero `XxHash128` over the `Object`'s canonical bytes with the volatile `Representations` EXCLUDED from the Type seed (`Rasm.Materials/Component` mints it from the Component content), so a later geometry attach never re-keys the Type, identical Components dedup to ONE Type, and the IFC `IfcElementType` round-trip is stable — never a second identity owner beside the one `NodeId` regime.
