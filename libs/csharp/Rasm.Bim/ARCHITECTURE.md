# [RASM_BIM_ARCHITECTURE]

The professional domain folder structure of `Rasm.Bim`, each sub-domain with a one-line charter. Mechanics live on the `.planning/` design pages and forward work on `IDEAS.md` and `TASKLOG.md`.

## [1]-[DOMAIN_MAP]

The sub-domains mirror the eventual source tree, each carrying a `.planning/` design page. Every sub-domain composes the one `BimModel` the `exchange` import rail produces rather than minting a parallel model surface, lowers its rejection onto the one `faults#FAULT_BAND` `BimFault` band, and consumes the `query/element-set#ELEMENT_SET` `ElementPredicate` algebra and the `classification/systems#CLASSIFICATION_AXIS` axis as settled vocabulary rather than a parallel selection or classification surface.

```text codemap
Rasm.Bim/
├── faults/           # BimFault closed [Union] band-2600 (ModelRejected/UnmappedClass/DanglingReference/CodecReject/CapabilityMiss) every entrypoint lowers onto the Fin<T> rail, composing the kernel GeometryFault for shared degenerate-geometry.
├── model/            # Host-neutral BIM object model: BimElement record discriminated by an IfcClass row, BimModel collection, the Project fold.
├── query/            # ElementSet set-algebraic query over a closed ElementPredicate union folded by Union/Intersect/Except/Where.
├── classification/   # bSDD-bound standard-systems classification axis: Classification vocabulary, ClassificationCode, ClassificationRef, plus the BsddResolution live dictionary resolution degrading to the row's local code-shape policy.
├── assembly/         # Host-neutral spatial-structure tree plus the closed AssemblyRel decomposition union over the IFC IfcRel* relationships.
├── properties/       # First-class Pset/Qto owner: typed PropertySet/QuantitySet over the standard Pset_*/Qto_* sets, type-vs-occurrence inheritance, round-tripped through IfcRelDefinesByProperties.
├── validation/       # IDS v1.0 model-validation owner folding the six IdsFacet arms onto the ElementPredicate algebra over BimModel, routing cross-tool audit to the ifctester companion.
├── coordination/     # BCF 3.0 issue exchange (BcfTopic/BcfComment/BcfViewpoint, the .bcfzip codec, the BcfApi REST projection) and the GlobalId-plus-content-key ModelDiff federation change-set.
├── georeferencing/   # IFC4.3 coordinate-reference owner projecting IfcMapConversion/IfcProjectedCRS onto a host-neutral GeoReference record reconciled by the FrameNormalization.Georeference CRS overload, plus the ProjNET datum-to-datum reprojection bridging the rigid map-conversion offset onto a true geodetic transform.
├── material/         # Host-neutral construction-material composition: BimMaterial owning the BimMaterialComposition [Union] (IfcMaterialLayerSet thickness-keyed layer build-up, IfcMaterialProfileSet linear-member section material, IfcMaterialConstituentSet composite constituent fractions) and the BimAppearance IfcSurfaceStyleRendering PBR record reconciled with Rasm.Materials at the content-key seam — never a single material-name string.
├── systems/          # IFC distribution-system connectivity graph: DistributionSystem carrying its DistributionSystemKind [SmartEnum] over IfcDistributionSystemEnum, its member element set, and the PortConnection [Union] over IfcRelConnectsPortToElement/IfcRelConnectsPorts the SystemTrace graph fold traverses.
├── zoning/           # Cross-cutting spatial-zone/program grouping overlay: BimZone carrying its BimZoneKind [SmartEnum], a many-to-many element/space assignment distinct from the single-parent containment tree, and the ZoneAssignment [Union] over IfcRelAssignsToGroup/IfcRelReferencedInSpatialStructure a fire-compartment/thermal-zone/program-area query folds through the ByZone arm.
├── cost/             # 5D cost-and-resource network: CostItem joining an IfcCostValue rate to the QuantitySet quantity by element, the ConstructionResource [Union] (labor/material/equipment) joined to the sequencing ConstructionTask, and the CostSchedule.Rollup fold the persistence cost catalog and the AppUi estimate report consume.
├── analysis/         # IFC structural-analysis-domain projection: the AnalysisModel host-neutral analysis-element/load/support graph (AnalysisMember [Union] over IfcStructuralCurveMember/IfcStructuralSurfaceMember/IfcStructuralPointConnection plus LoadGroup/Support) the Compute solver reads by (GeometryHash, PropertyHash), the idealized member binding the physical BimElement by GlobalId.
├── sequencing/       # IFC4.3 4D construction-scheduling network: ConstructionTask carrying its IfcTaskTime as a NodaTime Interval, the SequenceRel [Union] dependency lag as a Period, and its IfcRelAssignsToProcess element assignment, so ConstructionState.At(Instant) reads the task-interval-bounded element set.
├── reconstruction/   # Scan-to-BIM primitive fitting: a ReconstructionPrimitive [Union] (plane/cylinder/torus/freeform) folding the kernel-registered segmented cloud into BimElement rows with an ElementClassifier primitive->IfcClass table, a confidence band on the Pset, and a ReconstructionLineage source-cloud key — registration is the kernel's, the splat payload is Compute's, the BIM semantics are Bim's.
└── exchange/         # Universal interchange codec across the format/codec/extension axis plus FrameNormalization, the BimIo import fold, the BimExport emit fold with the per-tile TileMetadata EXT_structural_metadata author, the TessellationRequest companion bridge, and the host-free BimWire projection the Python and TypeScript peers decode.
```
