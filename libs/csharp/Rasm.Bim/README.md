# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package owning the universal BIM object model and IFC/glTF/STEP exchange and validation semantics. It owns the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec, per-importer frame normalization, the `BimElement` vocabulary, the `ElementSet` query algebra, the bSDD-bound classification axis, and the host-neutral assembly tree. It composes the kernel `Rasm` geometry, consumes the `Rasm.Compute` content-identity and companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/Model/faults.md): `BimFault` closed `[Union]` (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`) every Bim entrypoint lowers onto the `Fin<T>` rail through `.ToError()`.
- [02]-[ELEMENTS](.planning/Model/elements.md): `BimElement` record, the `IfcClass`/`IfcDomain` entity-class vocabulary, the `PredefinedType` sub-class discriminant, `BimType` type-occurrence factoring, `BimModel` collection, and the `Project` fold from the IFC semantic graph.
- [03]-[QUERY](.planning/Model/query.md): set-algebraic `ElementSet` query over a closed `ElementPredicate` union with `ByDomain`/`ByPredefinedType`/`ByZone` discrimination arms.
- [04]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD-bound standard-systems classification axis, local code-shape policy, `BsddResolution` live dictionary resolution, and the `IfcRelAssociatesClassification` round-trip.
- [05]-[COMPOSITION](.planning/Semantics/composition.md): host-neutral `BimMaterial` construction-material composition — `BimMaterialComposition` `[Union]` over the IFC layered/profiled/constituent material sets and the thickness-keyed `MaterialLayer`.
- [06]-[APPEARANCE](.planning/Semantics/appearance.md): `BimAppearance` host-neutral PBR record projected from `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`, reconciled with the `Rasm.Materials` OpenPBR owner at the content-key seam.
- [07]-[STRUCTURE](.planning/Model/structure.md): host-neutral spatial-structure tree and the closed `AssemblyRel` decomposition algebra.
- [08]-[ZONES](.planning/Model/zones.md): cross-cutting `BimZone` many-to-many grouping overlay over `IfcZone`/`IfcGroup`/`IfcSystem`, distinct from the single-parent containment tree, with the `ByZone` query arm.
- [09]-[PROPERTIES](.planning/Semantics/properties.md): typed `PropertySet`/`QuantitySet` keyed vocabulary, type-vs-occurrence inheritance fold, and the `IfcRelDefinesByProperties` round-trip.
- [10]-[VALIDATION](.planning/Review/validation.md): IDS v1.0 `IdsSpecification`/`IdsFacet` owner folding the six facets onto the `ElementPredicate` algebra, the XSD parse, and the `IdsAudit` receipt.
- [11]-[ISSUES](.planning/Review/issues.md): BCF 3.0 `BcfTopic`/`BcfComment`/`BcfViewpoint` record family, the `.bcfzip` codec, and the `BcfApi` REST projection.
- [12]-[DIFF](.planning/Review/diff.md): `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms joined by GlobalId plus content-key.
- [13]-[STRUCTURAL](.planning/Model/structural.md): host-neutral `AnalysisModel` structural-analysis graph projecting `IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/load/support, binding the physical `BimElement` by GlobalId for the Compute solver.
- [14]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` activity network carrying `IfcTaskTime` as a NodaTime `Interval`, the `SequenceRel` `[Union]` dependency lag as a `Period`, and the `ConstructionState.At(Instant)` element-set snapshot.
- [15]-[COST](.planning/Planning/cost.md): 5D `CostItem` cost-and-resource network joining an `IfcCostValue` rate to the `QuantitySet` quantity by GlobalId, the `ConstructionResource` `[Union]`, and the `CostSchedule.Rollup` fold.
- [16]-[SYSTEMS](.planning/Model/systems.md): `DistributionSystem` MEP connectivity graph with `DistributionSystemKind` `[SmartEnum]`, the `PortConnection` `[Union]` over `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts`, and the `SystemTrace` graph fold.
- [17]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): scan-to-BIM `ReconstructionPrimitive` `[Union]` (plane/cylinder/torus/freeform) folding the kernel-registered segmented cloud into `BimElement` rows with an `ElementPredicate`-classified `IfcClass` and source-cloud lineage key.
- [18]-[GEOREFERENCE](.planning/Semantics/georeference.md): host-neutral `GeoReference` record projected from `IfcMapConversion`/`IfcProjectedCRS`, the `FrameNormalization.Georeference` CRS overload, and the `GeoReference.Reproject` `ProjNET` datum-to-datum geodetic leg.
- [19]-[FORMAT](.planning/Exchange/format.md): `InterchangeFormat`/`InterchangeCodec`/`KhrExtension` format-codec-extension table, `FrameNormalization` per-importer frame coercion, and the `Detect` row resolution.
- [20]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold — managed glTF/mesh decode, the in-process semantic IFC/IFC5/STEP `IfcSemanticModel` graph, and the Speckle `Base` object-graph seam onto the canonical carriers.
- [21]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization, per-tile `EXT_structural_metadata` `TileMetadata` author, and the `InterchangePolicy`/`ExportArtifact` carriers.
- [22]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [23]-[WIRE](.planning/Exchange/wire.md): host-free `BimWire` JSON projection of the generated owners through the Thinktecture converters, the source-generated `BimWireContext`, and the content-keyed `BimModel` snapshot the Python and TypeScript peers decode.

## [02]-[DOMAIN_PACKAGES]

The IFC/glTF/STEP interchange and geodetic domain packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[IFC_SEMANTIC]:
- `GeometryGymIFC_Core`

[GLTF_CODEC]:
- `SharpGLTF.Core`
- `SharpGLTF.Toolkit`
- `SharpGLTF.Runtime`
- `SharpGLTF.Ext.3DTiles`

[CAD_INTERCHANGE]:
- `ACadSharp`

[MESH_COMPRESSION]:
- `Openize.Drako`
- `Alimer.Bindings.MeshOptimizer`

[MESH_TEXT]:
- `geometry3Sharp`

[SPECKLE_SYNC]:
- `Speckle.Sdk`
- `Speckle.Objects`

[GEODETIC]:
- `ProjNET`

[QUANTITY_UNIT]:
- `UnitsNet`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; full registry and version ownership live in `libs/csharp/.planning/README.md`, with decompile evidence in the folder `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `SharpFuzz`
- `Verify.XunitV3`
