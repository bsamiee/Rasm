# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package owning the universal BIM object model and the universal IFC/glTF/STEP exchange and validation semantics. It owns the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec, the per-importer frame normalization, the `BimElement` element vocabulary, the `ElementSet` query algebra, the bSDD-bound classification axis, and the host-neutral assembly tree; it composes the kernel `Rasm` geometry, consumes the `Rasm.Compute` content-identity and companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. The professional domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/` mirror the eventual source tree, one page per source file.

- [faults/faults](.planning/faults/faults.md): the `BimFault` closed `[Union]` band-2600 (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`) every Bim entrypoint lowers onto the `Fin<T>` rail through `.ToError()`.
- [model/elements](.planning/model/elements.md): `BimElement` element record, the `IfcClass`/`IfcDomain` entity-class vocabulary, the `PredefinedType` sub-class discriminant, the `BimType` type-occurrence factoring, `BimModel` collection, and the `Project` fold from the IFC semantic graph.
- [query/element-set](.planning/query/element-set.md): the set-algebraic `ElementSet` query over a closed `ElementPredicate` union with the `ByDomain`/`ByPredefinedType`/`ByZone` discrimination arms.
- [classification/systems](.planning/classification/systems.md): the bSDD-bound standard-systems classification axis, the local code-shape policy, the `BsddResolution` live dictionary resolution, and the `IfcRelAssociatesClassification` round-trip.
- [material/composition](.planning/material/composition.md): the host-neutral `BimMaterial` construction-material composition — the `BimMaterialComposition` `[Union]` over the IFC layered/profiled/constituent material sets and the thickness-keyed `MaterialLayer`.
- [material/appearance](.planning/material/appearance.md): the `BimAppearance` host-neutral PBR record projected from `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`, reconciled with the `Rasm.Materials` OpenPBR owner at the content-key seam.
- [assembly/spatial-structure](.planning/assembly/spatial-structure.md): the host-neutral spatial-structure tree and the closed `AssemblyRel` decomposition algebra.
- [zoning/grouping](.planning/zoning/grouping.md): the cross-cutting `BimZone` many-to-many grouping overlay over `IfcZone`/`IfcGroup`/`IfcSystem` distinct from the single-parent containment tree, and the `ByZone` query arm.
- [properties/property-sets](.planning/properties/property-sets.md): the typed `PropertySet`/`QuantitySet` keyed vocabulary, the type-vs-occurrence inheritance fold, and the `IfcRelDefinesByProperties` round-trip.
- [validation/ids](.planning/validation/ids.md): the IDS v1.0 `IdsSpecification`/`IdsFacet` owner folding the six facets onto the `ElementPredicate` algebra, the XSD parse, and the `IdsAudit` receipt.
- [coordination/issue-exchange](.planning/coordination/issue-exchange.md): the BCF 3.0 `BcfTopic`/`BcfComment`/`BcfViewpoint` record family, the `.bcfzip` codec, and the `BcfApi` REST projection.
- [coordination/model-diff](.planning/coordination/model-diff.md): the `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms joined by GlobalId plus content-key.
- [analysis/structural-model](.planning/analysis/structural-model.md): the host-neutral `AnalysisModel` structural-analysis graph projecting `IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/load/support, the idealized member binding the physical `BimElement` by GlobalId for the Compute solver.
- [sequencing/schedule](.planning/sequencing/schedule.md): the 4D `ConstructionTask` activity network carrying `IfcTaskTime` as a NodaTime `Interval`, the `SequenceRel` `[Union]` dependency lag as a `Period`, and the `ConstructionState.At(Instant)` element-set snapshot.
- [cost/estimate](.planning/cost/estimate.md): the 5D `CostItem` cost-and-resource network joining an `IfcCostValue` rate to the `QuantitySet` quantity by GlobalId, the `ConstructionResource` `[Union]`, and the `CostSchedule.Rollup` fold.
- [systems/connectivity](.planning/systems/connectivity.md): the `DistributionSystem` MEP connectivity graph carrying its `DistributionSystemKind` `[SmartEnum]`, the `PortConnection` `[Union]` over `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts`, and the `SystemTrace` graph fold.
- [reconstruction/primitives](.planning/reconstruction/primitives.md): the scan-to-BIM `ReconstructionPrimitive` `[Union]` (plane/cylinder/torus/freeform) folding the kernel-registered segmented cloud into `BimElement` rows with an `ElementPredicate`-classified `IfcClass` and a source-cloud lineage key.
- [georeferencing/coordinate-reference](.planning/georeferencing/coordinate-reference.md): the host-neutral `GeoReference` record projected from `IfcMapConversion`/`IfcProjectedCRS`, the `FrameNormalization.Georeference` CRS overload, and the `GeoReference.Reproject` `ProjNET` datum-to-datum geodetic leg.
- [exchange/format-axis](.planning/exchange/format-axis.md): the `InterchangeFormat`/`InterchangeCodec`/`KhrExtension` format-codec-extension table, the `FrameNormalization` per-importer frame coercion, and the `Detect` row resolution.
- [exchange/import-rail](.planning/exchange/import-rail.md): the `BimIo` foreign-bytes ingest fold — managed glTF/mesh decode, the in-process semantic IFC/IFC5/STEP `IfcSemanticModel` graph, and the Speckle `Base` object-graph seam onto the canonical carriers.
- [exchange/export-rail](.planning/exchange/export-rail.md): the `BimExport` artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization, the per-tile `EXT_structural_metadata` `TileMetadata` author, and the `InterchangePolicy`/`ExportArtifact` carriers.
- [exchange/tessellation-bridge](.planning/exchange/tessellation-bridge.md): the `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [exchange/wire](.planning/exchange/wire.md): the host-free `BimWire` JSON projection of the generated owners through the Thinktecture converters, the source-generated `BimWireContext`, and the content-keyed `BimModel` snapshot the Python and TypeScript peers decode.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one C# manifest.

- GeometryGymIFC_Core
- SharpGLTF.Core
- SharpGLTF.Toolkit
- SharpGLTF.Runtime
- SharpGLTF.Ext.3DTiles (per-tile `EXT_structural_metadata` BIM-feature metadata, `exchange/export-rail#TILE_METADATA`)
- Openize.Drako
- Alimer.Bindings.MeshOptimizer
- Speckle.Sdk
- Speckle.Objects
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- LanguageExt.Core
- NodaTime
- System.IO.Hashing
- ProjNET (geodetic datum/projection reprojection for the `georeferencing/coordinate-reference#GEODETIC_TRANSFORM` `GeoReference.Reproject` datum-bridging leg, `.api/api-projnet`)
