# [BIM_FEATURES]

The realized capability list for the host-neutral BIM-and-exchange engine. Every feature is a row, a record, a union arm, or a keyed-axis row on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[EXCHANGE_CONCEPTS]

The universal IFC/glTF/STEP exchange capabilities this package uniquely owns — the semantic model, the codec, the frame normalization, and the companion bridge.

| [INDEX] | [FEATURE]                                                                | [PAGE#CLUSTER]                       |
| :-----: | :----------------------------------------------------------------------- | :----------------------------------- |
|   [1]   | One format/codec axis discriminating import from export over row data    | Exchange/interchange#FORMAT_AXIS     |
|   [2]   | Per-importer frame/handedness/up-axis normalization at ingest            | Exchange/interchange#FORMAT_AXIS     |
|   [3]   | glTF KHR/EXT extension axis: thirteen rows over decompile-verified schema types | Exchange/interchange#FORMAT_AXIS |
|   [4]   | STEP AP203/AP214/AP242 protocols on one codec discriminated by a column  | Exchange/interchange#FORMAT_AXIS     |
|   [5]   | glTF/GLB managed mesh-and-scene import and export                        | Exchange/interchange#IMPORT_RAIL     |
|   [6]   | In-process IFC semantic-graph ingest, never tessellated BRep             | Exchange/interchange#IMPORT_RAIL     |
|   [7]   | IFC/IFC5 STEP/XML/JSON managed model serialization                       | Exchange/interchange#EXPORT_RAIL     |
|   [8]   | Idempotent IFC export under stable HashGlobalID and the content-key       | Exchange/interchange#EXPORT_RAIL     |
|   [9]   | Native Revit/Navisworks/DWG CAD-STEP ingest through the companion codec  | Exchange/interchange#FORMAT_AXIS     |
|  [10]   | Verify-before-admit candidate formats (USD/FBX/COLLADA), enumerable and detectable | Exchange/interchange#FORMAT_AXIS |
|  [11]   | IFC/AP242-to-geometry companion tessellation bridge, content-keyed reuse | Exchange/interchange#TESSELLATION_BRIDGE |

## [2]-[MODEL_CONCEPTS]

The host-neutral BIM object-model capabilities — the element vocabulary, the query algebra, the classification axis, and the assembly tree.

| [INDEX] | [FEATURE]                                                                | [PAGE#CLUSTER]                    |
| :-----: | :----------------------------------------------------------------------- | :------------------------------- |
|   [1]   | Host-neutral `BimElement` projected from the IFC semantic graph          | Model/object-model#ELEMENT_MODEL |
|   [2]   | One element record discriminated by an `IfcClass` row, never a per-type class | Model/object-model#ELEMENT_MODEL |
|   [3]   | Kernel-geometry handle by reference, never re-tessellated, never host-bound | Model/object-model#ELEMENT_MODEL |
|   [4]   | Set-algebraic element query over a closed predicate union                 | Model/object-model#ELEMENT_SET   |
|   [5]   | One polymorphic query surface, never a `Get<Dimension>` family            | Model/object-model#ELEMENT_SET   |
|   [6]   | Standard classification systems (Uniclass/OmniClass/MasterFormat/Uniformat) on one keyed axis | Model/object-model#CLASSIFICATION |
|   [7]   | Classification round-trip through `IfcRelAssociatesClassification`         | Model/object-model#CLASSIFICATION |
|   [8]   | Host-neutral spatial-structure assembly tree over the IFC hierarchy        | Model/object-model#ASSEMBLY      |
|   [9]   | Closed decomposition-relationship union mirroring the IFC `IfcRel*` family | Model/object-model#ASSEMBLY      |

## [3]-[CONCEPT_SEEDS]

Higher-order isolation concepts this folder uniquely enables, each riding an existing owner — no new surface.

- One content-keyed IFC artifact, two projections: the in-process semantic graph (`IMPORT_RAIL`) and the companion tessellated GLB (`TESSELLATION_BRIDGE`) join by the same Compute `XxHash128` content-key, so a coarse and a fine tessellation key distinctly and a re-import keys identically — deflection and tolerance partition the key, never a cross-setting hit (Exchange/interchange#IMPORT_RAIL · Exchange/interchange#TESSELLATION_BRIDGE).
- Universal-vs-Rhino coexistence as a contract boundary: the host-neutral IFC/exchange semantic model and the Rhino-native Make2D/sheet/native-file capture meet only at the universal contract, so neither is gutted to feed the other and a non-Rhino consumer reaches the universal owner while a Rhino consumer keeps the rich native surface (Exchange/interchange#FORMAT_AXIS · Model/object-model#ASSEMBLY).
- Element model as a projection, not a store: `BimElement` projects from the `IfcSemanticModel` graph and binds the kernel geometry by reference, so the object model is a host-neutral view over the content-keyed IFC artifact rather than a second geometry or property store (Model/object-model#ELEMENT_MODEL · Exchange/interchange#IMPORT_RAIL).
- Query and assembly collapse onto one model: the set-algebraic `ElementSet` query and the `BimAssembly` spatial tree both fold over the one `BimModel`, so a by-classification selection and a spatial-container traversal share the projected element graph rather than parallel query surfaces (Model/object-model#ELEMENT_SET · Model/object-model#ASSEMBLY).
- STEP protocol as a data column, not a type triad: AP203/AP214/AP242 ride one `step-iso10303` codec discriminated by the `StepProtocol` column, so a STEP file resolves to one codec row and the protocol is a switch the reader makes, never three sibling importer types (Exchange/interchange#FORMAT_AXIS).
