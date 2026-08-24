# [TS_CONTRACTS_API_RASM_TS_CONTRACTS]

`@rasm/contracts/*` publishes compiled TypeScript descriptors and exact publisher-asset projections. One wildcard exposes every module by contract path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@rasm/contracts`
- package: `@rasm/contracts`
- module: ESM JavaScript and declarations through `./*` to `./dist/*`
- runtime: universal; direct `@bufbuild/protobuf` dependency
- role: contract-path subpath family; descriptor modules retain the `<proto path>_pb` grammar
- rail: generated schemas, validated value types, descriptors, and frozen publisher-asset projections

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one proto declaration's generated TypeScript correspondence

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]                                            |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `<Name>`                          | message type        | decoded `Message<"<package>.<Name>">` value             |
|  [02]   | `<Name>Valid`                     | valid message type  | protovalidate-required fields refined as present        |
|  [03]   | `<Name>Schema`                    | `GenMessage`        | schema carrying runtime and valid-type parameters       |
|  [04]   | `MessageValidType<typeof Schema>` | derived type        | validated value inferred from the generated descriptor  |
|  [05]   | `file_<proto path>`               | `GenFile`           | generated file descriptor and dependency graph          |
|  [06]   | `<Parent>_<Child>`                | nested declaration  | generated nested message or enum                        |
|  [07]   | oneof `<group>`                   | tagged union        | generated case and value discriminant                   |
|  [08]   | `<Enum>` / `<Enum>Schema`         | object enum         | erasable generated enum vocabulary and descriptor       |
|  [09]   | `CloudEventsAvro`                 | readonly JSON value | exact frozen CloudEvents AVSC for consumer-owned codecs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated public roots and reachable support closure, grouped by descriptor package

<!-- roster:begin -->
[ROSTER_SCOPE]: `io.cloudevents.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                      | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `CloudEventSchema`                          | message | public-root     | `CloudEvent`                          |
|  [02]   | `CloudEvent_CloudEventAttributeValueSchema` | message | support-closure | `CloudEvent.CloudEventAttributeValue` |
|  [03]   | `CloudEventBatchSchema`                     | message | public-root     | `CloudEventBatch`                     |

[ROSTER_SCOPE]: `buf.validate` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND]  | [ORIGIN]        | [SYMBOL]           |
| :-----: | :----------------------- | :------ | :-------------- | :----------------- |
|  [01]   | `RuleSchema`             | message | support-closure | `Rule`             |
|  [02]   | `MessageRulesSchema`     | message | support-closure | `MessageRules`     |
|  [03]   | `MessageOneofRuleSchema` | message | support-closure | `MessageOneofRule` |
|  [04]   | `OneofRulesSchema`       | message | support-closure | `OneofRules`       |
|  [05]   | `FieldRulesSchema`       | message | support-closure | `FieldRules`       |
|  [06]   | `PredefinedRulesSchema`  | message | support-closure | `PredefinedRules`  |
|  [07]   | `FloatRulesSchema`       | message | support-closure | `FloatRules`       |
|  [08]   | `DoubleRulesSchema`      | message | support-closure | `DoubleRules`      |
|  [09]   | `Int32RulesSchema`       | message | support-closure | `Int32Rules`       |
|  [10]   | `Int64RulesSchema`       | message | support-closure | `Int64Rules`       |
|  [11]   | `UInt32RulesSchema`      | message | support-closure | `UInt32Rules`      |
|  [12]   | `UInt64RulesSchema`      | message | support-closure | `UInt64Rules`      |
|  [13]   | `SInt32RulesSchema`      | message | support-closure | `SInt32Rules`      |
|  [14]   | `SInt64RulesSchema`      | message | support-closure | `SInt64Rules`      |
|  [15]   | `Fixed32RulesSchema`     | message | support-closure | `Fixed32Rules`     |
|  [16]   | `Fixed64RulesSchema`     | message | support-closure | `Fixed64Rules`     |
|  [17]   | `SFixed32RulesSchema`    | message | support-closure | `SFixed32Rules`    |
|  [18]   | `SFixed64RulesSchema`    | message | support-closure | `SFixed64Rules`    |
|  [19]   | `BoolRulesSchema`        | message | support-closure | `BoolRules`        |
|  [20]   | `StringRulesSchema`      | message | support-closure | `StringRules`      |
|  [21]   | `BytesRulesSchema`       | message | support-closure | `BytesRules`       |
|  [22]   | `EnumRulesSchema`        | message | support-closure | `EnumRules`        |
|  [23]   | `RepeatedRulesSchema`    | message | support-closure | `RepeatedRules`    |
|  [24]   | `MapRulesSchema`         | message | support-closure | `MapRules`         |
|  [25]   | `AnyRulesSchema`         | message | support-closure | `AnyRules`         |
|  [26]   | `DurationRulesSchema`    | message | support-closure | `DurationRules`    |
|  [27]   | `FieldMaskRulesSchema`   | message | support-closure | `FieldMaskRules`   |
|  [28]   | `TimestampRulesSchema`   | message | support-closure | `TimestampRules`   |
|  [29]   | `IgnoreSchema`           | enum    | support-closure | `Ignore`           |
|  [30]   | `KnownRegexSchema`       | enum    | support-closure | `KnownRegex`       |

[ROSTER_SCOPE]: `rasm.contracts.artifact` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]      |
| :-----: | :------------------ | :------ | :-------------- | :------------ |
|  [01]   | `ArtifactRefSchema` | message | support-closure | `ArtifactRef` |

[ROSTER_SCOPE]: `rasm.contracts.appearance` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                      | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `PlaneRefSchema`                            | message | support-closure | `PlaneRef`                            |
|  [02]   | `PlaneSchema`                               | message | support-closure | `Plane`                               |
|  [03]   | `PackRowSchema`                             | message | support-closure | `PackRow`                             |
|  [04]   | `RoleSchema`                                | enum    | support-closure | `Role`                                |
|  [05]   | `TransferSchema`                            | enum    | support-closure | `Transfer`                            |
|  [06]   | `NormalConventionSchema`                    | enum    | support-closure | `NormalConvention`                    |
|  [07]   | `AlphaModeSchema`                           | enum    | support-closure | `AlphaMode`                           |
|  [08]   | `ContainerSchema`                           | enum    | support-closure | `Container`                           |
|  [09]   | `PackSchema`                                | enum    | support-closure | `Pack`                                |
|  [10]   | `PlaneFormatSchema`                         | enum    | support-closure | `PlaneFormat`                         |
|  [11]   | `MipPolicySchema`                           | enum    | support-closure | `MipPolicy`                           |
|  [12]   | `KtxPayloadSchema`                          | enum    | support-closure | `KtxPayload`                          |
|  [13]   | `BlockFormatSchema`                         | enum    | support-closure | `BlockFormat`                         |
|  [14]   | `LayerLawSchema`                            | enum    | support-closure | `LayerLaw`                            |
|  [15]   | `LicenseClassSchema`                        | enum    | support-closure | `LicenseClass`                        |
|  [16]   | `UdimSchema`                                | enum    | support-closure | `Udim`                                |
|  [17]   | `PrimariesSchema`                           | enum    | support-closure | `Primaries`                           |
|  [18]   | `DepthSchema`                               | enum    | support-closure | `Depth`                               |
|  [19]   | `ToolSchema`                                | enum    | support-closure | `Tool`                                |
|  [20]   | `EnvironmentPlaneSchema`                    | message | support-closure | `EnvironmentPlane`                    |
|  [21]   | `EnvironmentSourceSchema`                   | message | support-closure | `EnvironmentSource`                   |
|  [22]   | `HdriSchema`                                | message | support-closure | `Hdri`                                |
|  [23]   | `IblSchema`                                 | message | support-closure | `Ibl`                                 |
|  [24]   | `ProvenanceSchema`                          | message | support-closure | `Provenance`                          |
|  [25]   | `Provenance_CaptureSchema`                  | message | support-closure | `Provenance.Capture`                  |
|  [26]   | `Provenance_FitSchema`                      | message | support-closure | `Provenance.Fit`                      |
|  [27]   | `Provenance_InferenceSchema`                | message | support-closure | `Provenance.Inference`                |
|  [28]   | `Provenance_ChromaticitySchema`             | message | support-closure | `Provenance.Chromaticity`             |
|  [29]   | `Provenance_Chromaticity_DominanceSchema`   | message | support-closure | `Provenance.Chromaticity.Dominance`   |
|  [30]   | `Provenance_Chromaticity_TemperatureSchema` | message | support-closure | `Provenance.Chromaticity.Temperature` |
|  [31]   | `Provenance_CardSchema`                     | message | support-closure | `Provenance.Card`                     |
|  [32]   | `Provenance_IngestSchema`                   | message | support-closure | `Provenance.Ingest`                   |
|  [33]   | `PressSchema`                               | message | support-closure | `Press`                               |
|  [34]   | `SurfaceSetSchema`                          | message | support-closure | `SurfaceSet`                          |
|  [35]   | `BakedSetSchema`                            | message | support-closure | `BakedSet`                            |
|  [36]   | `EnvironmentSetSchema`                      | message | support-closure | `EnvironmentSet`                      |
|  [37]   | `SetSchema`                                 | message | public-root     | `Set`                                 |
|  [38]   | `ColorSchema`                               | message | support-closure | `Color`                               |
|  [39]   | `OpenPbrSchema`                             | message | support-closure | `OpenPbr`                             |
|  [40]   | `EmissionReadoutSchema`                     | message | support-closure | `EmissionReadout`                     |
|  [41]   | `EmissionSchema`                            | message | support-closure | `Emission`                            |
|  [42]   | `MaterialSchema`                            | message | public-root     | `Material`                            |

[ROSTER_SCOPE]: `rasm.contracts.compute` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND] | [ORIGIN]        | [SYMBOL]           |
| :-----: | :----------------------- | :----- | :-------------- | :----------------- |
|  [01]   | `DegradationLevelSchema` | enum   | support-closure | `DegradationLevel` |

[ROSTER_SCOPE]: `rasm.contracts.availability` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                      |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------- |
|  [01]   | `CommandVerdictWireSchema`          | message | support-closure | `CommandVerdictWire`          |
|  [02]   | `CommandVerdictWire_GatedSchema`    | message | support-closure | `CommandVerdictWire.Gated`    |
|  [03]   | `CommandVerdictWire_WithheldSchema` | message | support-closure | `CommandVerdictWire.Withheld` |
|  [04]   | `CommandAvailabilitySchema`         | message | public-root     | `CommandAvailability`         |

[ROSTER_SCOPE]: `rasm.contracts.spatial` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND]  | [ORIGIN]        | [SYMBOL]         |
| :-----: | :--------------------- | :------ | :-------------- | :--------------- |
|  [01]   | `Point3Schema`         | message | support-closure | `Point3`         |
|  [02]   | `UnitDirection3Schema` | message | support-closure | `UnitDirection3` |

[ROSTER_SCOPE]: `rasm.contracts.bcf` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `BcfCameraWireSchema`     | message | support-closure | `BcfCameraWire`     |
|  [02]   | `BcfColoringWireSchema`   | message | support-closure | `BcfColoringWire`   |
|  [03]   | `BcfLineWireSchema`       | message | support-closure | `BcfLineWire`       |
|  [04]   | `BcfClippingWireSchema`   | message | support-closure | `BcfClippingWire`   |
|  [05]   | `BcfBitmapWireSchema`     | message | support-closure | `BcfBitmapWire`     |
|  [06]   | `BcfSnippetWireSchema`    | message | support-closure | `BcfSnippetWire`    |
|  [07]   | `BcfDocumentWireSchema`   | message | support-closure | `BcfDocumentWire`   |
|  [08]   | `BcfFileWireSchema`       | message | support-closure | `BcfFileWire`       |
|  [09]   | `BcfHintsWireSchema`      | message | support-closure | `BcfHintsWire`      |
|  [10]   | `BcfShowingWireSchema`    | message | support-closure | `BcfShowingWire`    |
|  [11]   | `BcfHidingWireSchema`     | message | support-closure | `BcfHidingWire`     |
|  [12]   | `BcfVisibilityWireSchema` | message | support-closure | `BcfVisibilityWire` |
|  [13]   | `BcfViewpointWireSchema`  | message | public-root     | `BcfViewpointWire`  |
|  [14]   | `BcfCommentWireSchema`    | message | support-closure | `BcfCommentWire`    |
|  [15]   | `BcfTopicWireSchema`      | message | public-root     | `BcfTopicWire`      |
|  [16]   | `BcfStatusSchema`         | enum    | support-closure | `BcfStatus`         |
|  [17]   | `BcfBitmapFormatSchema`   | enum    | support-closure | `BcfBitmapFormat`   |

[ROSTER_SCOPE]: `rasm.contracts.benchmark` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                      | [KIND]  | [ORIGIN]        | [SYMBOL]              |
| :-----: | :-------------------------- | :------ | :-------------- | :-------------------- |
|  [01]   | `LabelPairSchema`           | message | support-closure | `LabelPair`           |
|  [02]   | `HostFingerprintWireSchema` | message | support-closure | `HostFingerprintWire` |
|  [03]   | `BenchInputWireSchema`      | message | support-closure | `BenchInputWire`      |
|  [04]   | `ChromeTraceWireSchema`     | message | support-closure | `ChromeTraceWire`     |
|  [05]   | `BenchmarkExportWireSchema` | message | support-closure | `BenchmarkExportWire` |
|  [06]   | `EpContextWireSchema`       | message | support-closure | `EpContextWire`       |
|  [07]   | `ProfileArtifactWireSchema` | message | support-closure | `ProfileArtifactWire` |
|  [08]   | `BenchKernelWireSchema`     | message | support-closure | `BenchKernelWire`     |
|  [09]   | `BenchAggregateSchema`      | message | support-closure | `BenchAggregate`      |
|  [10]   | `RungCellSchema`            | message | support-closure | `RungCell`            |
|  [11]   | `BenchBandWireSchema`       | message | support-closure | `BenchBandWire`       |
|  [12]   | `BenchMetricSchema`         | message | support-closure | `BenchMetric`         |
|  [13]   | `BenchmarkClaimWireSchema`  | message | public-root     | `BenchmarkClaimWire`  |
|  [14]   | `BenchModalitySchema`       | enum    | support-closure | `BenchModality`       |
|  [15]   | `BenchPolaritySchema`       | enum    | support-closure | `BenchPolarity`       |
|  [16]   | `PayloadBandSchema`         | enum    | support-closure | `PayloadBand`         |
|  [17]   | `BenchRungSchema`           | enum    | support-closure | `BenchRung`           |

[ROSTER_SCOPE]: `google.type` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]  | [ORIGIN]        | [SYMBOL]    |
| :-----: | :---------------- | :------ | :-------------- | :---------- |
|  [01]   | `DateSchema`      | message | support-closure | `Date`      |
|  [02]   | `DateTimeSchema`  | message | support-closure | `DateTime`  |
|  [03]   | `TimeZoneSchema`  | message | support-closure | `TimeZone`  |
|  [04]   | `TimeOfDaySchema` | message | support-closure | `TimeOfDay` |

[ROSTER_SCOPE]: `rasm.contracts.element` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [ORIGIN]        | [SYMBOL]                  |
| :-----: | :------------------------------ | :------ | :-------------- | :------------------------ |
|  [01]   | `VectorWireSchema`              | message | support-closure | `VectorWire`              |
|  [02]   | `DimensionWireSchema`           | message | support-closure | `DimensionWire`           |
|  [03]   | `MeasureBandWireSchema`         | message | support-closure | `MeasureBandWire`         |
|  [04]   | `MeasureValueWireSchema`        | message | support-closure | `MeasureValueWire`        |
|  [05]   | `NamedMeasureWireSchema`        | message | support-closure | `NamedMeasureWire`        |
|  [06]   | `CurvePointWireSchema`          | message | support-closure | `CurvePointWire`          |
|  [07]   | `SampledCurveWireSchema`        | message | support-closure | `SampledCurveWire`        |
|  [08]   | `PropertyValueWireSchema`       | message | support-closure | `PropertyValueWire`       |
|  [09]   | `LogicalWireSchema`             | message | support-closure | `LogicalWire`             |
|  [10]   | `EnumeratedWireSchema`          | message | support-closure | `EnumeratedWire`          |
|  [11]   | `TemporalWireSchema`            | message | support-closure | `TemporalWire`            |
|  [12]   | `ReferenceWireSchema`           | message | support-closure | `ReferenceWire`           |
|  [13]   | `BoundedWireSchema`             | message | support-closure | `BoundedWire`             |
|  [14]   | `ListWireSchema`                | message | support-closure | `ListWire`                |
|  [15]   | `TableRowWireSchema`            | message | support-closure | `TableRowWire`            |
|  [16]   | `TableWireSchema`               | message | support-closure | `TableWire`               |
|  [17]   | `NamedValueWireSchema`          | message | support-closure | `NamedValueWire`          |
|  [18]   | `ComplexWireSchema`             | message | support-closure | `ComplexWire`             |
|  [19]   | `UncertaintyKindSchema`         | enum    | support-closure | `UncertaintyKind`         |
|  [20]   | `InterpolationSchema`           | enum    | support-closure | `Interpolation`           |
|  [21]   | `InheritanceModeSchema`         | enum    | support-closure | `InheritanceMode`         |
|  [22]   | `EvidenceGradeSchema`           | enum    | support-closure | `EvidenceGrade`           |
|  [23]   | `DiagnosticWireSchema`          | message | support-closure | `DiagnosticWire`          |
|  [24]   | `ProvenanceWireSchema`          | message | support-closure | `ProvenanceWire`          |
|  [25]   | `AssessmentWireSchema`          | message | support-closure | `AssessmentWire`          |
|  [26]   | `ObservationChunkWireSchema`    | message | support-closure | `ObservationChunkWire`    |
|  [27]   | `SensorProvenanceWireSchema`    | message | support-closure | `SensorProvenanceWire`    |
|  [28]   | `GradeCountWireSchema`          | message | support-closure | `GradeCountWire`          |
|  [29]   | `MomentsWireSchema`             | message | support-closure | `MomentsWire`             |
|  [30]   | `SeriesStatisticsWireSchema`    | message | support-closure | `SeriesStatisticsWire`    |
|  [31]   | `ObservationWireSchema`         | message | support-closure | `ObservationWire`         |
|  [32]   | `ProjectedCrsWireSchema`        | message | support-closure | `ProjectedCrsWire`        |
|  [33]   | `GeoReferenceWireSchema`        | message | support-closure | `GeoReferenceWire`        |
|  [34]   | `CellLatticeWireSchema`         | message | support-closure | `CellLatticeWire`         |
|  [35]   | `ColorBinWireSchema`            | message | support-closure | `ColorBinWire`            |
|  [36]   | `CoverageBandWireSchema`        | message | support-closure | `CoverageBandWire`        |
|  [37]   | `OverviewLevelWireSchema`       | message | support-closure | `OverviewLevelWire`       |
|  [38]   | `CoverageWireSchema`            | message | support-closure | `CoverageWire`            |
|  [39]   | `DisciplineSchema`              | enum    | support-closure | `Discipline`              |
|  [40]   | `AssessmentOutcomeSchema`       | enum    | support-closure | `AssessmentOutcome`       |
|  [41]   | `SolvePhaseSchema`              | enum    | support-closure | `SolvePhase`              |
|  [42]   | `FailureKindSchema`             | enum    | support-closure | `FailureKind`             |
|  [43]   | `SamplingKindSchema`            | enum    | support-closure | `SamplingKind`            |
|  [44]   | `ObservationGradeSchema`        | enum    | support-closure | `ObservationGrade`        |
|  [45]   | `CoverageKindSchema`            | enum    | support-closure | `CoverageKind`            |
|  [46]   | `ChannelDtypeSchema`            | enum    | support-closure | `ChannelDtype`            |
|  [47]   | `BandRoleSchema`                | enum    | support-closure | `BandRole`                |
|  [48]   | `CrsResolutionSchema`           | enum    | support-closure | `CrsResolution`           |
|  [49]   | `MaterialLayerWireSchema`       | message | support-closure | `MaterialLayerWire`       |
|  [50]   | `LayerSetWireSchema`            | message | support-closure | `LayerSetWire`            |
|  [51]   | `ProfileRefWireSchema`          | message | support-closure | `ProfileRefWire`          |
|  [52]   | `MaterialProfileWireSchema`     | message | support-closure | `MaterialProfileWire`     |
|  [53]   | `SectionPropertiesWireSchema`   | message | support-closure | `SectionPropertiesWire`   |
|  [54]   | `ProfileSetWireSchema`          | message | support-closure | `ProfileSetWire`          |
|  [55]   | `MaterialConstituentWireSchema` | message | support-closure | `MaterialConstituentWire` |
|  [56]   | `ConstituentSetWireSchema`      | message | support-closure | `ConstituentSetWire`      |
|  [57]   | `MaterialCompositionWireSchema` | message | support-closure | `MaterialCompositionWire` |
|  [58]   | `AttestationWireSchema`         | message | support-closure | `AttestationWire`         |
|  [59]   | `PropertyEvidenceWireSchema`    | message | support-closure | `PropertyEvidenceWire`    |
|  [60]   | `MechanicalWireSchema`          | message | support-closure | `MechanicalWire`          |
|  [61]   | `OrthotropicWireSchema`         | message | support-closure | `OrthotropicWire`         |
|  [62]   | `ThermalWireSchema`             | message | support-closure | `ThermalWire`             |
|  [63]   | `BandValueWireSchema`           | message | support-closure | `BandValueWire`           |
|  [64]   | `AcousticWireSchema`            | message | support-closure | `AcousticWire`            |
|  [65]   | `FireResistanceWireSchema`      | message | support-closure | `FireResistanceWire`      |
|  [66]   | `FireWireSchema`                | message | support-closure | `FireWire`                |
|  [67]   | `BandCellWireSchema`            | message | support-closure | `BandCellWire`            |
|  [68]   | `EnvironmentalWireSchema`       | message | support-closure | `EnvironmentalWire`       |
|  [69]   | `CostWireSchema`                | message | support-closure | `CostWire`                |
|  [70]   | `RayleighWireSchema`            | message | support-closure | `RayleighWire`            |
|  [71]   | `DampingWireSchema`             | message | support-closure | `DampingWire`             |
|  [72]   | `HygrothermalWireSchema`        | message | support-closure | `HygrothermalWire`        |
|  [73]   | `DurabilityWireSchema`          | message | support-closure | `DurabilityWire`          |
|  [74]   | `OpticalWireSchema`             | message | support-closure | `OpticalWire`             |
|  [75]   | `ElectricalWireSchema`          | message | support-closure | `ElectricalWire`          |
|  [76]   | `MaterialPropertySetWireSchema` | message | support-closure | `MaterialPropertySetWire` |
|  [77]   | `MaterialWireSchema`            | message | support-closure | `MaterialWire`            |
|  [78]   | `AttestationRoleSchema`         | enum    | support-closure | `AttestationRole`         |
|  [79]   | `FireRatingSchema`              | enum    | support-closure | `FireRating`              |
|  [80]   | `SmokeClassSchema`              | enum    | support-closure | `SmokeClass`              |
|  [81]   | `DropletClassSchema`            | enum    | support-closure | `DropletClass`            |
|  [82]   | `MeasurementBasisSchema`        | enum    | support-closure | `MeasurementBasis`        |
|  [83]   | `LifecycleBandSchema`           | enum    | support-closure | `LifecycleBand`           |
|  [84]   | `AcousticBandSchema`            | enum    | support-closure | `AcousticBand`            |
|  [85]   | `ClassificationWireSchema`      | message | support-closure | `ClassificationWire`      |
|  [86]   | `OwnerHistoryWireSchema`        | message | support-closure | `OwnerHistoryWire`        |
|  [87]   | `SchemaSpanWireSchema`          | message | support-closure | `SchemaSpanWire`          |
|  [88]   | `PlacementWireSchema`           | message | support-closure | `PlacementWire`           |
|  [89]   | `RepresentationWireSchema`      | message | support-closure | `RepresentationWire`      |
|  [90]   | `ObjectWireSchema`              | message | support-closure | `ObjectWire`              |
|  [91]   | `PropertySetWireSchema`         | message | support-closure | `PropertySetWire`         |
|  [92]   | `GroupIdentityWireSchema`       | message | support-closure | `GroupIdentityWire`       |
|  [93]   | `GroupWireSchema`               | message | support-closure | `GroupWire`               |
|  [94]   | `QuantitySetWireSchema`         | message | support-closure | `QuantitySetWire`         |
|  [95]   | `AppearanceWireSchema`          | message | support-closure | `AppearanceWire`          |
|  [96]   | `NodeWireSchema`                | message | public-root     | `NodeWire`                |
|  [97]   | `ObjectKindSchema`              | enum    | support-closure | `ObjectKind`              |
|  [98]   | `ReleaseVersionSchema`          | enum    | support-closure | `ReleaseVersion`          |
|  [99]   | `ChangeActionSchema`            | enum    | support-closure | `ChangeAction`            |
|  [100]  | `ObjectStateSchema`             | enum    | support-closure | `ObjectState`             |
|  [101]  | `RepresentationKindSchema`      | enum    | support-closure | `RepresentationKind`      |
|  [102]  | `EditTombstoneSchema`           | message | support-closure | `EditTombstone`           |
|  [103]  | `EditMembersSchema`             | message | support-closure | `EditMembers`             |
|  [104]  | `EntityEditWireSchema`          | message | public-root     | `EntityEditWire`          |

[ROSTER_SCOPE]: `rasm.contracts.declaration` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND] | [ORIGIN]        | [SYMBOL]         |
| :-----: | :--------------------- | :----- | :-------------- | :--------------- |
|  [01]   | `ImpactCategorySchema` | enum   | support-closure | `ImpactCategory` |

[ROSTER_SCOPE]: `rasm.contracts.bim` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `DeltaValueWireSchema`    | message | support-closure | `DeltaValueWire`    |
|  [02]   | `AspectDeltaWireSchema`   | message | support-closure | `AspectDeltaWire`   |
|  [03]   | `DiffEndWireSchema`       | message | support-closure | `DiffEndWire`       |
|  [04]   | `DiffModifiedWireSchema`  | message | support-closure | `DiffModifiedWire`  |
|  [05]   | `DiffMovedWireSchema`     | message | support-closure | `DiffMovedWire`     |
|  [06]   | `DiffRegroupWireSchema`   | message | support-closure | `DiffRegroupWire`   |
|  [07]   | `ElementChangeWireSchema` | message | support-closure | `ElementChangeWire` |
|  [08]   | `ModelDiffWireSchema`     | message | public-root     | `ModelDiffWire`     |
|  [09]   | `DeltaShapeSchema`        | enum    | support-closure | `DeltaShape`        |

[ROSTER_SCOPE]: `rasm.contracts.binding` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                      |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------- |
|  [01]   | `BindingStatusSchema`               | message | public-root     | `BindingStatus`               |
|  [02]   | `ExternalTransportSchema`           | enum    | support-closure | `ExternalTransport`           |
|  [03]   | `BindingStateSchema`                | enum    | support-closure | `BindingState`                |
|  [04]   | `BindingDirectionSchema`            | enum    | support-closure | `BindingDirection`            |
|  [05]   | `CoercedValueWireSchema`            | message | public-root     | `CoercedValueWire`            |
|  [06]   | `WriteBackWireSchema`               | message | support-closure | `WriteBackWire`               |
|  [07]   | `WriteBackWire_AcknowledgedSchema`  | message | support-closure | `WriteBackWire.Acknowledged`  |
|  [08]   | `WriteBackWire_RejectedSchema`      | message | support-closure | `WriteBackWire.Rejected`      |
|  [09]   | `WriteBackWire_RolledBackSchema`    | message | support-closure | `WriteBackWire.RolledBack`    |
|  [10]   | `WriteBackWire_IndeterminateSchema` | message | support-closure | `WriteBackWire.Indeterminate` |
|  [11]   | `WriteReceiptWireSchema`            | message | public-root     | `WriteReceiptWire`            |
|  [12]   | `EchoClassSchema`                   | enum    | support-closure | `EchoClass`                   |

[ROSTER_SCOPE]: `google.rpc` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                            | [KIND]  | [ORIGIN]        | [SYMBOL]                    |
| :-----: | :-------------------------------- | :------ | :-------------- | :-------------------------- |
|  [01]   | `RetryInfoSchema`                 | message | support-closure | `RetryInfo`                 |
|  [02]   | `BadRequestSchema`                | message | support-closure | `BadRequest`                |
|  [03]   | `BadRequest_FieldViolationSchema` | message | support-closure | `BadRequest.FieldViolation` |
|  [04]   | `LocalizedMessageSchema`          | message | support-closure | `LocalizedMessage`          |

[ROSTER_SCOPE]: `rasm.contracts.clock` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]      | [KIND]  | [ORIGIN]    | [SYMBOL] |
| :-----: | :---------- | :------ | :---------- | :------- |
|  [01]   | `HlcSchema` | message | public-root | `Hlc`    |

[ROSTER_SCOPE]: `rasm.contracts.fault` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND]  | [ORIGIN]        | [SYMBOL]           |
| :-----: | :----------------------- | :------ | :-------------- | :----------------- |
|  [01]   | `FaultRecoverySchema`    | message | support-closure | `FaultRecovery`    |
|  [02]   | `FaultObservationSchema` | message | support-closure | `FaultObservation` |
|  [03]   | `FaultDetailSchema`      | message | public-root     | `FaultDetail`      |

[ROSTER_SCOPE]: `rasm.contracts.capability` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]    | [SYMBOL]            |
| :-----: | :------------------------ | :------ | :---------- | :------------------ |
|  [01]   | `DescriptorPinWireSchema` | message | public-root | `DescriptorPinWire` |

[ROSTER_SCOPE]: `rasm.contracts.crdt` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [ORIGIN]        | [SYMBOL]        |
| :-----: | :-------------------- | :------ | :-------------- | :-------------- |
|  [01]   | `ElementIdSchema`     | message | support-closure | `ElementId`     |
|  [02]   | `VectorSlotSchema`    | message | support-closure | `VectorSlot`    |
|  [03]   | `SetOpSchema`         | message | support-closure | `SetOp`         |
|  [04]   | `WriteOpSchema`       | message | support-closure | `WriteOp`       |
|  [05]   | `AddOpSchema`         | message | support-closure | `AddOp`         |
|  [06]   | `RemoveOpSchema`      | message | support-closure | `RemoveOp`      |
|  [07]   | `IncrementOpSchema`   | message | support-closure | `IncrementOp`   |
|  [08]   | `InsertAfterOpSchema` | message | support-closure | `InsertAfterOp` |
|  [09]   | `DeleteOpSchema`      | message | support-closure | `DeleteOp`      |
|  [10]   | `MaintainOpSchema`    | message | support-closure | `MaintainOp`    |
|  [11]   | `BeatOpSchema`        | message | support-closure | `BeatOp`        |
|  [12]   | `LeaveOpSchema`       | message | support-closure | `LeaveOp`       |
|  [13]   | `CrdtOpWireSchema`    | message | public-root     | `CrdtOpWire`    |

[ROSTER_SCOPE]: `rasm.contracts.credential` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                       | [KIND]  | [ORIGIN]        | [SYMBOL]               |
| :-----: | :--------------------------- | :------ | :-------------- | :--------------------- |
|  [01]   | `CertificateChainSchema`     | message | support-closure | `CertificateChain`     |
|  [02]   | `CredentialPublicWireSchema` | message | public-root     | `CredentialPublicWire` |

[ROSTER_SCOPE]: `rasm.contracts.patch` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------------- | :------ | :-------------- | :------------- |
|  [01]   | `PatchAddSchema`     | message | support-closure | `PatchAdd`     |
|  [02]   | `PatchRemoveSchema`  | message | support-closure | `PatchRemove`  |
|  [03]   | `PatchReplaceSchema` | message | support-closure | `PatchReplace` |
|  [04]   | `PatchMoveSchema`    | message | support-closure | `PatchMove`    |
|  [05]   | `PatchCopySchema`    | message | support-closure | `PatchCopy`    |
|  [06]   | `PatchTestSchema`    | message | support-closure | `PatchTest`    |
|  [07]   | `PatchOpSchema`      | message | support-closure | `PatchOp`      |

[ROSTER_SCOPE]: `rasm.contracts.event` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [ORIGIN]    | [SYMBOL]     |
| :-----: | :----------------- | :------ | :---------- | :----------- |
|  [01]   | `ExtensionsSchema` | message | public-root | `Extensions` |

[ROSTER_SCOPE]: `rasm.contracts.feature` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [SYMBOL]          |
| :-----: | :---------------------- | :------ | :-------------- | :---------------- |
|  [01]   | `FlagVerdictWireSchema` | message | public-root     | `FlagVerdictWire` |
|  [02]   | `FlagReasonSchema`      | enum    | support-closure | `FlagReason`      |

[ROSTER_SCOPE]: `rasm.contracts.organization` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------------- | :------ | :-------------- | :------------- |
|  [01]   | `ViewOverrideSchema` | message | support-closure | `ViewOverride` |
|  [02]   | `EntitySchema`       | message | support-closure | `Entity`       |
|  [03]   | `EntityPathSchema`   | message | support-closure | `EntityPath`   |
|  [04]   | `OrganizationSchema` | message | public-root     | `Organization` |

[ROSTER_SCOPE]: `rasm.contracts.parity` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------------- | :------ | :-------------- | :------------- |
|  [01]   | `ArtifactSchema`     | message | support-closure | `Artifact`     |
|  [02]   | `CapabilitySchema`   | message | support-closure | `Capability`   |
|  [03]   | `BackendSchema`      | message | public-root     | `Backend`      |
|  [04]   | `ArtifactRoleSchema` | enum    | support-closure | `ArtifactRole` |
|  [05]   | `ProviderSchema`     | enum    | support-closure | `Provider`     |
|  [06]   | `FailureRankSchema`  | enum    | support-closure | `FailureRank`  |
|  [07]   | `RestartClassSchema` | enum    | support-closure | `RestartClass` |

[ROSTER_SCOPE]: `rasm.contracts.receipt` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `TenantContextWireSchema` | message | support-closure | `TenantContextWire` |
|  [02]   | `ReceiptHeaderWireSchema` | message | support-closure | `ReceiptHeaderWire` |
|  [03]   | `PackageSchema`           | enum    | support-closure | `Package`           |

[ROSTER_SCOPE]: `rasm.contracts.render` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                      |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------- |
|  [01]   | `ViewCameraWireSchema`              | message | support-closure | `ViewCameraWire`              |
|  [02]   | `ViewCameraWire_FrameSchema`        | message | support-closure | `ViewCameraWire.Frame`        |
|  [03]   | `ViewCameraWire_PerspectiveSchema`  | message | support-closure | `ViewCameraWire.Perspective`  |
|  [04]   | `ViewCameraWire_OrthographicSchema` | message | support-closure | `ViewCameraWire.Orthographic` |
|  [05]   | `ViewCameraWire_AsymmetricSchema`   | message | support-closure | `ViewCameraWire.Asymmetric`   |
|  [06]   | `SectionBoxWireSchema`              | message | support-closure | `SectionBoxWire`              |
|  [07]   | `VisibilityOverrideWireSchema`      | message | support-closure | `VisibilityOverrideWire`      |
|  [08]   | `ViewMeasurementPointWireSchema`    | message | support-closure | `ViewMeasurementPointWire`    |
|  [09]   | `ViewMeasurementWireSchema`         | message | support-closure | `ViewMeasurementWire`         |
|  [10]   | `ViewpointWireSchema`               | message | support-closure | `ViewpointWire`               |
|  [11]   | `SphereWireSchema`                  | message | support-closure | `SphereWire`                  |
|  [12]   | `MeshoptStreamSchema`               | message | support-closure | `MeshoptStream`               |
|  [13]   | `MeshletSchema`                     | message | support-closure | `Meshlet`                     |
|  [14]   | `ResidencyTileWireSchema`           | message | support-closure | `ResidencyTileWire`           |
|  [15]   | `GeometryResidencySchema`           | message | public-root     | `GeometryResidency`           |
|  [16]   | `ResidencyKindSchema`               | enum    | support-closure | `ResidencyKind`               |
|  [17]   | `StreamModeSchema`                  | enum    | support-closure | `StreamMode`                  |
|  [18]   | `StreamFilterSchema`                | enum    | support-closure | `StreamFilter`                |

[ROSTER_SCOPE]: `rasm.contracts.ui` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                    | [KIND]  | [ORIGIN]        | [SYMBOL]                            |
| :-----: | :---------------------------------------- | :------ | :-------------- | :---------------------------------- |
|  [01]   | `CommandGateWireSchema`                   | message | public-root     | `CommandGateWire`                   |
|  [02]   | `CommandPayloadWireSchema`                | message | support-closure | `CommandPayloadWire`                |
|  [03]   | `CommandPayloadWire_ManySchema`           | message | support-closure | `CommandPayloadWire.Many`           |
|  [04]   | `CommandInvocationSchema`                 | message | public-root     | `CommandInvocation`                 |
|  [05]   | `CommandOutcomeWireSchema`                | message | support-closure | `CommandOutcomeWire`                |
|  [06]   | `DeckReceiptWireSchema`                   | message | support-closure | `DeckReceiptWire`                   |
|  [07]   | `IconSlotWireSchema`                      | message | support-closure | `IconSlotWire`                      |
|  [08]   | `HintRowWireSchema`                       | message | support-closure | `HintRowWire`                       |
|  [09]   | `IntentBindingWireSchema`                 | message | support-closure | `IntentBindingWire`                 |
|  [10]   | `OptionRowWireSchema`                     | message | support-closure | `OptionRowWire`                     |
|  [11]   | `OptionSourceWireSchema`                  | message | support-closure | `OptionSourceWire`                  |
|  [12]   | `OptionSourceWire_InlineSchema`           | message | support-closure | `OptionSourceWire.Inline`           |
|  [13]   | `CrumbRowWireSchema`                      | message | support-closure | `CrumbRowWire`                      |
|  [14]   | `AvatarRowWireSchema`                     | message | support-closure | `AvatarRowWire`                     |
|  [15]   | `FileFilterRowWireSchema`                 | message | support-closure | `FileFilterRowWire`                 |
|  [16]   | `MenuRowWireSchema`                       | message | support-closure | `MenuRowWire`                       |
|  [17]   | `ToolbarRowWireSchema`                    | message | support-closure | `ToolbarRowWire`                    |
|  [18]   | `SectionWireSchema`                       | message | support-closure | `SectionWire`                       |
|  [19]   | `ExtentWireSchema`                        | message | support-closure | `ExtentWire`                        |
|  [20]   | `ColumnRowWireSchema`                     | message | support-closure | `ColumnRowWire`                     |
|  [21]   | `WindowWireSchema`                        | message | support-closure | `WindowWire`                        |
|  [22]   | `NumericRangeWireSchema`                  | message | support-closure | `NumericRangeWire`                  |
|  [23]   | `NumericRangeWire_IntegralSchema`         | message | support-closure | `NumericRangeWire.Integral`         |
|  [24]   | `NumericRangeWire_UnsignedSchema`         | message | support-closure | `NumericRangeWire.Unsigned`         |
|  [25]   | `NumericRangeWire_RealSchema`             | message | support-closure | `NumericRangeWire.Real`             |
|  [26]   | `NumericRangeWire_PreciseSchema`          | message | support-closure | `NumericRangeWire.Precise`          |
|  [27]   | `ControlIntentWireSchema`                 | message | support-closure | `ControlIntentWire`                 |
|  [28]   | `ControlIntentWire_ButtonSchema`          | message | support-closure | `ControlIntentWire.Button`          |
|  [29]   | `ControlIntentWire_LabelSchema`           | message | support-closure | `ControlIntentWire.Label`           |
|  [30]   | `ControlIntentWire_TextInputSchema`       | message | support-closure | `ControlIntentWire.TextInput`       |
|  [31]   | `ControlIntentWire_NumberInputSchema`     | message | support-closure | `ControlIntentWire.NumberInput`     |
|  [32]   | `ControlIntentWire_DateInputSchema`       | message | support-closure | `ControlIntentWire.DateInput`       |
|  [33]   | `ControlIntentWire_PathInputSchema`       | message | support-closure | `ControlIntentWire.PathInput`       |
|  [34]   | `ControlIntentWire_ColorInputSchema`      | message | support-closure | `ControlIntentWire.ColorInput`      |
|  [35]   | `ControlIntentWire_SelectSchema`          | message | support-closure | `ControlIntentWire.Select`          |
|  [36]   | `ControlIntentWire_MultiSelectSchema`     | message | support-closure | `ControlIntentWire.MultiSelect`     |
|  [37]   | `ControlIntentWire_SliderSchema`          | message | support-closure | `ControlIntentWire.Slider`          |
|  [38]   | `ControlIntentWire_RangeSchema`           | message | support-closure | `ControlIntentWire.Range`           |
|  [39]   | `ControlIntentWire_ToggleSchema`          | message | support-closure | `ControlIntentWire.Toggle`          |
|  [40]   | `ControlIntentWire_RadioSchema`           | message | support-closure | `ControlIntentWire.Radio`           |
|  [41]   | `ControlIntentWire_SegmentedSchema`       | message | support-closure | `ControlIntentWire.Segmented`       |
|  [42]   | `ControlIntentWire_ChipSchema`            | message | support-closure | `ControlIntentWire.Chip`            |
|  [43]   | `ControlIntentWire_ProgressSchema`        | message | support-closure | `ControlIntentWire.Progress`        |
|  [44]   | `ControlIntentWire_AvatarSchema`          | message | support-closure | `ControlIntentWire.Avatar`          |
|  [45]   | `ControlIntentWire_BreadcrumbSchema`      | message | support-closure | `ControlIntentWire.Breadcrumb`      |
|  [46]   | `ControlIntentWire_TooltipSchema`         | message | support-closure | `ControlIntentWire.Tooltip`         |
|  [47]   | `ControlIntentWire_BannerSchema`          | message | support-closure | `ControlIntentWire.Banner`          |
|  [48]   | `ControlIntentWire_EmptyStateSchema`      | message | support-closure | `ControlIntentWire.EmptyState`      |
|  [49]   | `ControlIntentWire_GridSchema`            | message | support-closure | `ControlIntentWire.Grid`            |
|  [50]   | `ControlIntentWire_TreeSchema`            | message | support-closure | `ControlIntentWire.Tree`            |
|  [51]   | `ControlIntentWire_OverviewSchema`        | message | support-closure | `ControlIntentWire.Overview`        |
|  [52]   | `ControlIntentWire_MenuSchema`            | message | support-closure | `ControlIntentWire.Menu`            |
|  [53]   | `ControlIntentWire_ToolbarSchema`         | message | support-closure | `ControlIntentWire.Toolbar`         |
|  [54]   | `ControlIntentWire_TabSchema`             | message | support-closure | `ControlIntentWire.Tab`             |
|  [55]   | `ControlIntentWire_AccordionSchema`       | message | support-closure | `ControlIntentWire.Accordion`       |
|  [56]   | `ControlIntentWire_PanelSchema`           | message | support-closure | `ControlIntentWire.Panel`           |
|  [57]   | `ControlIntentWire_DockSchema`            | message | support-closure | `ControlIntentWire.Dock`            |
|  [58]   | `ControlIntentWire_SplitterSchema`        | message | support-closure | `ControlIntentWire.Splitter`        |
|  [59]   | `ControlEmphasisSchema`                   | enum    | support-closure | `ControlEmphasis`                   |
|  [60]   | `ControlTriggerSchema`                    | enum    | support-closure | `ControlTrigger`                    |
|  [61]   | `IconPlacementSchema`                     | enum    | support-closure | `IconPlacement`                     |
|  [62]   | `NumericKindSchema`                       | enum    | support-closure | `NumericKind`                       |
|  [63]   | `TemporalKindSchema`                      | enum    | support-closure | `TemporalKind`                      |
|  [64]   | `PickerModeSchema`                        | enum    | support-closure | `PickerMode`                        |
|  [65]   | `ColorPostureSchema`                      | enum    | support-closure | `ColorPosture`                      |
|  [66]   | `SelectPostureSchema`                     | enum    | support-closure | `SelectPosture`                     |
|  [67]   | `MultiPostureSchema`                      | enum    | support-closure | `MultiPosture`                      |
|  [68]   | `SegmentPostureSchema`                    | enum    | support-closure | `SegmentPosture`                    |
|  [69]   | `ChipPostureSchema`                       | enum    | support-closure | `ChipPosture`                       |
|  [70]   | `ProgressFormSchema`                      | enum    | support-closure | `ProgressForm`                      |
|  [71]   | `MenuPostureSchema`                       | enum    | support-closure | `MenuPosture`                       |
|  [72]   | `OverflowModeSchema`                      | enum    | support-closure | `OverflowMode`                      |
|  [73]   | `OrientationSchema`                       | enum    | support-closure | `Orientation`                       |
|  [74]   | `ExtentModeSchema`                        | enum    | support-closure | `ExtentMode`                        |
|  [75]   | `ExtentUnitSchema`                        | enum    | support-closure | `ExtentUnit`                        |
|  [76]   | `ColumnAlignSchema`                       | enum    | support-closure | `ColumnAlign`                       |
|  [77]   | `BannerSeveritySchema`                    | enum    | support-closure | `BannerSeverity`                    |
|  [78]   | `BannerPlacementSchema`                   | enum    | support-closure | `BannerPlacement`                   |
|  [79]   | `OverviewAxisSchema`                      | enum    | support-closure | `OverviewAxis`                      |
|  [80]   | `TypographyRoleSchema`                    | enum    | support-closure | `TypographyRole`                    |
|  [81]   | `PixelIdentityWireSchema`                 | message | support-closure | `PixelIdentityWire`                 |
|  [82]   | `NativeAssetFactWireSchema`               | message | support-closure | `NativeAssetFactWire`               |
|  [83]   | `EvidenceReceiptWireSchema`               | message | public-root     | `EvidenceReceiptWire`               |
|  [84]   | `EvidenceReceiptWire_SurfaceSchema`       | message | support-closure | `EvidenceReceiptWire.Surface`       |
|  [85]   | `EvidenceReceiptWire_FocusSchema`         | message | support-closure | `EvidenceReceiptWire.Focus`         |
|  [86]   | `EvidenceReceiptWire_RenderSchema`        | message | support-closure | `EvidenceReceiptWire.Render`        |
|  [87]   | `EvidenceReceiptWire_DisposalSchema`      | message | support-closure | `EvidenceReceiptWire.Disposal`      |
|  [88]   | `EvidenceReceiptWire_EditSchema`          | message | support-closure | `EvidenceReceiptWire.Edit`          |
|  [89]   | `EvidenceReceiptWire_ThemeSchema`         | message | support-closure | `EvidenceReceiptWire.Theme`         |
|  [90]   | `EvidenceReceiptWire_MotionSchema`        | message | support-closure | `EvidenceReceiptWire.Motion`        |
|  [91]   | `EvidenceReceiptWire_EffectSchema`        | message | support-closure | `EvidenceReceiptWire.Effect`        |
|  [92]   | `EvidenceReceiptWire_Effect_ExtentSchema` | message | support-closure | `EvidenceReceiptWire.Effect.Extent` |
|  [93]   | `EvidenceReceiptWire_AssetSchema`         | message | support-closure | `EvidenceReceiptWire.Asset`         |
|  [94]   | `EvidenceReceiptWire_LiveDataSchema`      | message | support-closure | `EvidenceReceiptWire.LiveData`      |
|  [95]   | `EvidenceReceiptWire_CollabSyncSchema`    | message | support-closure | `EvidenceReceiptWire.CollabSync`    |
|  [96]   | `EvidenceReceiptWire_CollabRevertSchema`  | message | support-closure | `EvidenceReceiptWire.CollabRevert`  |
|  [97]   | `EvidenceReceiptWire_MediaSchema`         | message | support-closure | `EvidenceReceiptWire.Media`         |
|  [98]   | `EvidenceReceiptWire_QualitySchema`       | message | support-closure | `EvidenceReceiptWire.Quality`       |
|  [99]   | `EvidenceReceiptWire_GpuFrameSchema`      | message | support-closure | `EvidenceReceiptWire.GpuFrame`      |
|  [100]  | `EvidenceReceiptWire_LayoutSchema`        | message | support-closure | `EvidenceReceiptWire.Layout`        |
|  [101]  | `EvidenceReceiptWire_DispatcherLagSchema` | message | support-closure | `EvidenceReceiptWire.DispatcherLag` |
|  [102]  | `EvidenceReceiptWire_PreCommitSchema`     | message | support-closure | `EvidenceReceiptWire.PreCommit`     |
|  [103]  | `SkewBandWireSchema`                      | message | support-closure | `SkewBandWire`                      |
|  [104]  | `EvidenceRowWireSchema`                   | message | support-closure | `EvidenceRowWire`                   |
|  [105]  | `EvidenceTimelineWireSchema`              | message | public-root     | `EvidenceTimelineWire`              |
|  [106]  | `PixelLayoutSchema`                       | enum    | support-closure | `PixelLayout`                       |
|  [107]  | `MediaOutcomeSchema`                      | enum    | support-closure | `MediaOutcome`                      |
|  [108]  | `LayoutVarWireSchema`                     | message | support-closure | `LayoutVarWire`                     |
|  [109]  | `LayoutTermWireSchema`                    | message | support-closure | `LayoutTermWire`                    |
|  [110]  | `LayoutExprWireSchema`                    | message | support-closure | `LayoutExprWire`                    |
|  [111]  | `LayoutConstraintWireSchema`              | message | support-closure | `LayoutConstraintWire`              |
|  [112]  | `LayoutEditSchema`                        | message | support-closure | `LayoutEdit`                        |
|  [113]  | `LayoutValueSchema`                       | message | support-closure | `LayoutValue`                       |
|  [114]  | `LayoutProgramSchema`                     | message | support-closure | `LayoutProgram`                     |
|  [115]  | `LayoutRelationSchema`                    | enum    | support-closure | `LayoutRelation`                    |
|  [116]  | `LayoutStrengthSchema`                    | enum    | support-closure | `LayoutStrength`                    |
|  [117]  | `AppUiSurfaceProgramSchema`               | message | public-root     | `AppUiSurfaceProgram`               |

[ASSET_SCOPE]: exact publisher projections emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]        | [PATH]                                                                |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `CloudEventsAvro` | readonly-json | `libs/typescript/contracts/gen/io/cloudevents/v1/cloudevents_avro.ts` |

<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root generation emits selected packages and reachable support descriptors into one clean module tree.
- `valid_types=protovalidate_required` carries corpus presence rules into each generated valid type.
- Generated modules cross-reference by relative specifier under `import_extension=js`, so the emitted tree resolves without the package name.
- Assay restores manifest-distributed publisher assets after Buf's clean sweep.
- Publisher descriptors and assets remain direct codec inputs; estate registries retain estate descriptors alone.

[STACKING]:
- `@bufbuild/protobuf`(`../../.api/bufbuild-protobuf.md`): `<Name>Schema` drives create, decode, encode, equality, reflection, and validation typing.
- `core/interchange/format`: estate schemas enter the registry by generated descriptor; `CloudEventSchema` and `CloudEventBatchSchema` bind directly.
- `CloudEventsAvro`: exact publisher AVSC input to a consumer-owned Avro codec, without a second schema or package dependency.
- Consumer schema transforms convert validated generated values into domain values after codec admission.

[LOCAL_ADMISSION]:
- Consumers import `@rasm\/contracts/<proto path>_pb` and derive value types from the exported schema.
- Avro consumers import `CloudEventsAvro` from `@rasm\/contracts/io/cloudevents/v1/cloudevents_avro`.
- Corpus and generator changes regenerate the module tree; package code adds no barrel, copied descriptor, or handwritten message shape.

[RAIL_LAW]:
- Package: `@rasm/contracts`
- Owns: generated descriptor/value declarations and exact publisher-asset projections for selected contracts
- Accept: schema-first codecs, validation, and consumer-owned domain conversion
- Reject: handwritten proto shapes, copied publisher assets, descriptor mirrors, per-family exports, barrels, and hand edits under `gen`
