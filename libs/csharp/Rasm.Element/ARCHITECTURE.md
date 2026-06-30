# [RASM_ELEMENT_ARCHITECTURE]

The domain map of `Rasm.Element` — the lowest AEC-DOMAIN seam between the KERNEL (`Rasm`) and the AEC peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`). The `Graph`, `Relations`, `Classification`, `Properties`, `Composition`, `Assessment`, `Geospatial`, and `Projection` sub-domains, each composing the one `ElementGraph` and lowering onto the one `ElementFault` band, the peers depending UP on the `IElementProjection`/`IGraphConstraint` contracts and aligning by the content-keyed graph rather than by referencing each other.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Element/ # refs ../Rasm ONLY; no GeometryGym; no host geometry (geometry by content hash)
├── Graph/ # The authoritative property graph and its mutation algebra
│ ├── Element.cs # NodeId identity (rooted Guid-v7 / non-rooted XxHash128) + Node [Union] + Header + frozen ElementGraph + built-once incidence index + memoized Bake derived fold
│ └── Delta.cs # GraphMutation [Union] + total Switch + HAMT WorkingGraph + structural edge law + GraphDelta monoid event body + ReplayOnto persistence fold
├── Relations/ # The neutral objectified-edge algebra
│ └── Relation.cs # Relationship [Union] (Compose/Assign/Associate/Connect/Void + Generic passthrough) + neutral sub-kinds + MaterialUsage occurrence payload
├── Classification/ # The neutral cross-cutting axes
│ └── Classification.cs # generic Classification [ComplexValueObject] system+code + the one Discipline [SmartEnum] analysis axis
├── Properties/ # The typed property/quantity value vocabulary
│ ├── Property.cs # PropertyValue [Union] IFC-value family + PropertyName + PropertyBag/QuantityBag + InheritanceMode type→occurrence merge
│ └── Quantity.cs # Dimension [ComplexValueObject] 7-SI-exponent discriminator + MeasureValue UnitsNet SI coercion + QTO accessors
├── Composition/ # The material composition and intrinsic acoustic folds
│ ├── Material.cs # MaterialId + MaterialComposition [Union] (+ ProfileRef) + MaterialPropertySet [Union] keyed to Discipline
│ └── Acoustic.cs # AcousticBand one-third-octave + banded Acoustic carrier + Nrc/Saa/StcWeighted/Rw pure folds + shared RatingContour.Fit kernel
├── Assessment/ # The generic analysis receipt
│ └── Assessment.cs # generic AssessmentPayload (Discipline+AnalysisRoute+InputKey) + AssessmentOutcome lifecycle + typed Results bag + failure Detail + Provenance + content-keyed ResultBlob
├── Geospatial/ # The georeferenced coverage and CRS
│ ├── Coverage.cs # CoverageGrid by-ref raster/field + CoverageBand schema + affine GridDescriptor
│ └── Reference.cs # GeoReference twelve-tuple + ProjectedCrs EPSG parse + fault-on-unresolvable Admit
└── Projection/ # The cross-stratum contracts, the content codec, and the fault band
 ├── Projection.cs # IElementProjection projector floor + IGraphConstraint legality floor + ProjectionContext + Assemble capability
 ├── Address.cs # CanonicalWriter one canonical codec + ContentAddress [ValueObject<UInt128>] over the kernel seed-zero XxHash128
 └── Fault.cs # ElementFault [Union] band-2500 every entrypoint lowers onto
```

The `Graph` sub-domain is the spine: `Element.cs` declares the `Node`/`NodeId`/`Header`/`ElementGraph` and the `Bake` fold; `Delta.cs` the mutation algebra and the persistable `GraphDelta`. Every other sub-domain owns a `Node` case payload or a cross-cutting value the spine composes — `Relations` the edges, `Classification`/`Properties`/`Composition`/`Assessment`/`Geospatial` the typed payloads, `Projection` the contracts, the content codec, and the fault band. The `Composition/Material` page composes `Properties/Quantity` (the `MeasureValue` columns) and `Composition/Acoustic` (the banded carrier); the `Graph/Element` `Bake` composes every payload owner and applies the `Properties/Property` `InheritanceMode` merge; the `Projection/Address` codec is the one canonical projection the `NodeId.Content` mint, the `ContentAddress` diff, and the `Generator.Equals` content comparison all share. The seam re-mints nothing the kernel owns: the content-identity seed is the kernel `XxHash128` seed-zero entry, the op-key the kernel `Op`, the fault base the kernel `Expected`.

## [02]-[SEAMS]

```text seams
Graph/element ← csharp:Rasm.Bim/Projection # [PROJECTION]: SemanticProjector:IElementProjection lowers GeometryGym → GraphDelta; Emit Bim-internal, never a seam member
Graph/element ← csharp:Rasm.Materials/Projection # [PROJECTION]: MaterialProjector:IElementProjection projects the material subgraph onto Material nodes; the projector authors Associate when given an element-id set
Graph/element ← csharp:Rasm.Fabrication/Projection # [PROJECTION]: a future third IElementProjection, one registration row
Projection ← csharp:Rasm.Bim # [CONSTRAINT]: IGraphConstraint IFC-semantic legality (containment/void/type-aggregation), composed after the seam structural law
Graph/element → csharp:Rasm.Compute # [SHAPE]: reads the concrete ElementGraph directly (above the seam, no interface); resolves the analytical AxisCurve/FootprintPolygon one-hop by content key through the seam GeometrySource port (app-wired over the Persistence blob store, never a phantom node field); writes Assessment.Result nodes content-keyed on (InputKey, Route)
Composition/acoustic → csharp:Rasm.Compute # [SHAPE]: RatingContour.Fit (the seam contour-fit kernel, Stc/Rw rows by data alone) shared for the ISO 12354 layered-STC; the multi-ply AssemblyAggregator folds MaterialComposition plies in Compute
Assessment/assessment → csharp:Rasm.Compute # [SHAPE]: Compute reads the concrete graph, runs the discipline route, writes a Computed Node.Assessment back via the neutral Assign/AssignKind.Assessment edge content-keyed on (Discipline,Route,InputKey); the AnalysisRoute roster + the solver-tool/version re-key are Compute's, the seam carries only the opaque AnalysisRoute token
Composition/material → csharp:Rasm.Compute # [SHAPE]: the MaterialComposition/MaterialPropertySet plies feed the relocated AssemblyAggregator (series-U / mass-law-STC / rule-of-mixtures / EN 15978 GWP / cost); the baked SectionProperties feed the design-code routes via graph.SectionOf
Composition/material → csharp:Rasm.Materials/Profiles # [PROJECTION]: ProfileRef resolved one-hop to the VividOrange section-property catalog
Composition/material → csharp:Rasm.Bim # [SHAPE]: the Cost case's neutral per-unit doubles + opaque Currency ISO-4217 token meet the Bim NodaMoney money algebra at the 5D quantity×rate join; the MaterialComposition serializes to the IFC 4.3 material-definition family at Emit
Geospatial/reference ← csharp:Rasm.Bim/Semantics # [PROJECTION]: GeoReferenceProjector folds IfcMapConversion(Scaled)/IfcProjectedCRS (or the IfcSite WGS84 fallback) onto the seam GeoReference twelve-tuple on Header/Coverage; the ProjNET/OSR datum reprojection consumes it; the seam owns GeoReference.Admit + the EPSG parse + fault-on-unresolvable, Bim the IFC fill + the kernel-transform build
Geospatial/coverage ← csharp:Rasm.Bim/Semantics # [PROJECTION]: GeoModel.Project lowers a vector feature onto an Object node and GeoRaster.ToCoverage a raster onto a Coverage node via a GraphDelta; NTS/STRtree/GDAL-OGR + the vector/raster codecs live in Bim; raster bytes to the content-keyed blob store, only grid metadata + band schema on the seam
Properties/property ← csharp:Rasm.Bim/Semantics # [PROJECTION]: the Pset_* template roster, bSDD resolution, base-quantity derivation, and the IfcRelDefinesByProperties round-trip stay in Bim; Bim fills the seam PropertySet/QuantitySet bags with typed PropertyValue, the seam owning the value shape + the InheritanceMode merge
Properties/quantity → csharp:Rasm.Bim/Semantics # [SHAPE]: MeasureValue is the carrier the Bim QuantityDerivation.Derive base-quantity fold composes (Qto_*BaseQuantities, derived-wins, MeasureValue.OfSi/Sum), each Pset_* measure admitting with its real QuantityType/Dimension/unit
Graph/delta ⇄ csharp:Rasm.Persistence/Element # [CONTENT_KEY]: GraphDelta is the GraphEvent body; the inline GraphProjection folds GraphDelta → ElementGraph via ReplayOnto; AggregateSnapshot bounds replay; the Version/ledger op-log/CRDT engine projects FROM the committed delta events
Graph/element ⇄ csharp:Rasm.Persistence/Element # [CONTENT_KEY]: ElementGraph store-load roundtrip (GraphStore); the Generator.Equals Inequalities member diff feeds the Version/merge 3-way StructuralMerge
Projection/address ⇄ csharp:Rasm # [CONTENT_KEY]: composes the kernel seed-zero XxHash128 content-hash entry; the ONE hasher, no second hasher
Projection/address → csharp:Rasm.Persistence # [CONTENT_KEY]: the ContentAddress + the Object RepresentationContentHash representation content hashes the Store/blobstore keys geometry/raster blobs on; the Element/codec NodeHash composes the one canonical-byte projection shared by the NodeId hash + the diff
Projection ← csharp:Rasm.AppHost # [PORT]: ProjectionContext neutral primitives (clock instant / CorrelationId / TenantId), supplied at the app composition root, never an AppHost type on the seam
Graph/element ⇄ python:geometry/mesh # [WIRE]: the imported-IFC GLB seed-zero XxHash128 == the Object RepresentationContentHash entry; the companion decodes the seam representation key, never re-mints
Projection/address ⇄ python:runtime/evidence # [WIRE]: the one XxHash128 seed-zero ContentAddress parity — the MATERIAL_LAYER_GOLDEN IfcMaterialLayer golden vector pins byte-for-byte agreement; the typed Material/Property/Quantity(MeasureValue)/Assessment/Classification wire vocabulary the companion decodes (the {value, unit} MeasureValue shape via MeasureValue.Of(string) under the invariant culture), never re-mints
* ⇄ typescript:interchange # [WIRE]: the ElementGraph/Node/Relationship content-keyed wire the TypeScript peer decodes
```

The `[PROJECTION]` rows are the INVERSION OF CONTROL: GeometryGym, VividOrange, and every provider stay in the AEC peers, which implement `IElementProjection` and lower their foreign source onto a `GraphDelta` — no provider edge points down into the seam, no second IFC or section-property stack. The acyclic strata is preserved: every AEC peer references `{Rasm, Rasm.Element}` (a shared LOWER stratum, the same shape as depending on the kernel), and peers never reference each other; the live element assembly (registering the `Seq<IElementProjection>`, binding the tessellation adapter, running `Assemble` against a live source) is an APP / HOST-BOUNDARY composition-root concern, the seam owning the `Assemble` capability and the apps the wiring.

[CONTENT_KEY_IDIOM]:
- every page that joins the persistence, projection, assessment-cache, or diff lane derives a typed `UInt128` through the `Projection/address#CONTENT_ADDRESS` `XxHash128.HashToUInt128` (seed zero) over the one `CanonicalWriter` projection, never a second identity scheme; the `RepresentationContentHash` (geometry by content hash), the `Coverage.RasterKey`, the `Assessment.InputKey`, and the graph snapshot `ContentAddress.OfGraph` all address one content space shared with the kernel `GeometryHash` and the Python/TypeScript peers.
- a second hasher, a non-zero seed, a per-page hash function, or a `Guid`-keyed content join is the named cross-folder drift defect — the seam composes the kernel's one `XxHash128` seed-zero entry and the cross-runtime parity corpus (a float-bearing `IfcMaterialLayer`-shaped node) anchors byte-for-byte agreement.
- a non-rooted node id is the self-hash of the node's OWN `ToCanonicalBytes` via `Graph/element#NODE_MODEL` `NodeId.Content` (the form the `Projection/address#CONTENT_ADDRESS` `Verify` dual recomputes and the parity corpus pins); `NodeId.OfContent(addr)` is valid ONLY when `addr == ContentAddress.Of(node.ToCanonicalBytes())` (it skips the re-hash), so feeding it a FOREIGN precomputed key — an `Assessment` `InputKey` over (route, targets, policy) rather than the node's self-hash — stores an id `Verify` cannot reproduce and is the named drift; the canonical `Assessment` node id is `NodeId.Content(ToCanonicalBytes(Discipline, Route, InputKey))`, the `InputKey` a payload field the triple folds.
