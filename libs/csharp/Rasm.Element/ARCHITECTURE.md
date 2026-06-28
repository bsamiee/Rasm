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
│ └── Acoustic.cs # AcousticBand + banded Acoustic carrier + Nrc/Saa/StcWeighted pure folds + shared StcContourFit kernel
├── Assessment/ # The generic analysis receipt
│ └── Assessment.cs # generic AssessmentPayload (Discipline+route+InputKey) + AssessmentOutcome lifecycle + Provenance + content-keyed ResultBlob
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
Graph/element → csharp:Rasm.Compute # [SHAPE]: reads the concrete ElementGraph directly (above the seam, no interface); writes Assessment.Result nodes content-keyed on (InputKey, Route)
Composition/acoustic → csharp:Rasm.Compute # [SHAPE]: StcContourFit shared for the ISO 12354 layered-STC; the multi-ply AssemblyAggregator folds MaterialComposition plies in Compute
Composition/material → csharp:Rasm.Materials/Profiles # [PROJECTION]: ProfileRef resolved one-hop to the VividOrange section-property catalog
Graph/delta ⇄ csharp:Rasm.Persistence/Version # [CONTENT_KEY]: GraphDelta is the Marten event body; inline projection folds GraphDelta → ElementGraph; AggregateSnapshot bounds replay
Graph/element ⇄ csharp:Rasm.Persistence/Element # [CONTENT_KEY]: ElementGraph store-load roundtrip; Generator.Equals Inequalities feeds the 3-way StructuralMerge
Projection/address ⇄ csharp:Rasm # [CONTENT_KEY]: composes the kernel seed-zero XxHash128 content-hash entry; the ONE hasher, no second hasher
Projection/address → csharp:Rasm.Persistence # [CONTENT_KEY]: the ContentAddress + the Object RepresentationContentHash representation content hashes the Store/blobstore keys geometry/raster blobs on; the Element/codec NodeHash composes the one canonical-byte projection shared by the NodeId hash + the diff
Projection ← csharp:Rasm.AppHost # [PORT]: ProjectionContext neutral primitives (clock instant / CorrelationId / TenantId), supplied at the app composition root, never an AppHost type on the seam
* ⇄ python:geometry/ifc # [WIRE]: the content-key (one XxHash128 seed) + the typed Material/Property/Assessment/Classification wire vocabulary the companion decodes, never re-mints
* ⇄ typescript:interchange # [WIRE]: the ElementGraph/Node/Relationship content-keyed wire the TypeScript peer decodes
```

The `[PROJECTION]` rows are the INVERSION OF CONTROL: GeometryGym, VividOrange, and every provider stay in the AEC peers, which implement `IElementProjection` and lower their foreign source onto a `GraphDelta` — no provider edge points down into the seam, no second IFC or section-property stack. The acyclic strata is preserved: every AEC peer references `{Rasm, Rasm.Element}` (a shared LOWER stratum, the same shape as depending on the kernel), and peers never reference each other; the live element assembly (registering the `Seq<IElementProjection>`, binding the tessellation adapter, running `Assemble` against a live source) is an APP / HOST-BOUNDARY composition-root concern, the seam owning the `Assemble` capability and the apps the wiring.

[CONTENT_KEY_IDIOM]:
- every page that joins the persistence, projection, assessment-cache, or diff lane derives a typed `UInt128` through the `Projection/address#CONTENT_ADDRESS` `XxHash128.HashToUInt128` (seed zero) over the one `CanonicalWriter` projection, never a second identity scheme; the `RepresentationContentHash` (geometry by content hash), the `Coverage.RasterKey`, the `Assessment.InputKey`, and the graph snapshot `ContentAddress.OfGraph` all address one content space shared with the kernel `GeometryHash` and the Python/TypeScript peers.
- a second hasher, a non-zero seed, a per-page hash function, or a `Guid`-keyed content join is the named cross-folder drift defect — the seam composes the kernel's one `XxHash128` seed-zero entry and the cross-runtime parity corpus (a float-bearing `IfcMaterialLayer`-shaped node) anchors byte-for-byte agreement.
