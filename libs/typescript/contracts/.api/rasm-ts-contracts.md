# [TS_CONTRACTS_API_RASM_TS_CONTRACTS]

`@rasm/contracts/*` publishes compiled TypeScript descriptors and exact publisher-asset projections. One wildcard exposes every module by contract path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@rasm/contracts`
- package: `@rasm/contracts`
- module: ESM JavaScript and declarations through `./*` to `./dist/*`
- runtime: universal; direct `@bufbuild/protobuf` dependency
- role: contract-path subpath family; descriptor modules retain the `<proto path>_pb` grammar
- rail: generated schemas, validated value types, descriptors, services, and frozen publisher-asset projections

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one proto declaration's generated TypeScript correspondence

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]                                            |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `<Name>`                          | message type        | decoded `Message<"<package>.<Name>">` value             |
|  [02]   | `<Name>Valid`                     | valid message type  | protovalidate-required fields refined as present        |
|  [03]   | `<Name>Schema`                    | `GenMessage`        | schema carrying runtime and valid-type parameters       |
|  [04]   | `MessageValidType<typeof Schema>` | derived type        | validated value inferred from the generated descriptor  |
|  [05]   | `file_<proto path>`               | `GenFile`           | generated file descriptor and dependency graph          |
|  [06]   | `<Service>`                       | `GenService`        | generated service and method descriptors                |
|  [07]   | `<Parent>_<Child>`                | nested declaration  | generated nested message or enum                        |
|  [08]   | oneof `<group>`                   | tagged union        | generated case and value discriminant                   |
|  [09]   | `<Enum>` / `<Enum>Schema`         | object enum         | erasable generated enum vocabulary and descriptor       |
|  [10]   | `CloudEventsAvro`                 | readonly JSON value | exact frozen CloudEvents AVSC for consumer-owned codecs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated public roots and reachable support closure, grouped by descriptor package

<!-- roster:begin -->
[ROSTER_SCOPE]: `io.cloudevents.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                      | [KIND]  | [ORIGIN]        | [FQN]                                                   |
| :-----: | :------------------------------------------ | :------ | :-------------- | :------------------------------------------------------ |
|  [01]   | `CloudEventSchema`                          | message | public-root     | `io.cloudevents.v1.CloudEvent`                          |
|  [02]   | `CloudEvent_CloudEventAttributeValueSchema` | message | support-closure | `io.cloudevents.v1.CloudEvent.CloudEventAttributeValue` |
|  [03]   | `CloudEventBatchSchema`                     | message | public-root     | `io.cloudevents.v1.CloudEventBatch`                     |

[ROSTER_SCOPE]: `buf.validate` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND]  | [ORIGIN]        | [FQN]                           |
| :-----: | :----------------------- | :------ | :-------------- | :------------------------------ |
|  [01]   | `RuleSchema`             | message | support-closure | `buf.validate.Rule`             |
|  [02]   | `MessageRulesSchema`     | message | support-closure | `buf.validate.MessageRules`     |
|  [03]   | `MessageOneofRuleSchema` | message | support-closure | `buf.validate.MessageOneofRule` |
|  [04]   | `OneofRulesSchema`       | message | support-closure | `buf.validate.OneofRules`       |
|  [05]   | `FieldRulesSchema`       | message | support-closure | `buf.validate.FieldRules`       |
|  [06]   | `PredefinedRulesSchema`  | message | support-closure | `buf.validate.PredefinedRules`  |
|  [07]   | `FloatRulesSchema`       | message | support-closure | `buf.validate.FloatRules`       |
|  [08]   | `DoubleRulesSchema`      | message | support-closure | `buf.validate.DoubleRules`      |
|  [09]   | `Int32RulesSchema`       | message | support-closure | `buf.validate.Int32Rules`       |
|  [10]   | `Int64RulesSchema`       | message | support-closure | `buf.validate.Int64Rules`       |
|  [11]   | `UInt32RulesSchema`      | message | support-closure | `buf.validate.UInt32Rules`      |
|  [12]   | `UInt64RulesSchema`      | message | support-closure | `buf.validate.UInt64Rules`      |
|  [13]   | `SInt32RulesSchema`      | message | support-closure | `buf.validate.SInt32Rules`      |
|  [14]   | `SInt64RulesSchema`      | message | support-closure | `buf.validate.SInt64Rules`      |
|  [15]   | `Fixed32RulesSchema`     | message | support-closure | `buf.validate.Fixed32Rules`     |
|  [16]   | `Fixed64RulesSchema`     | message | support-closure | `buf.validate.Fixed64Rules`     |
|  [17]   | `SFixed32RulesSchema`    | message | support-closure | `buf.validate.SFixed32Rules`    |
|  [18]   | `SFixed64RulesSchema`    | message | support-closure | `buf.validate.SFixed64Rules`    |
|  [19]   | `BoolRulesSchema`        | message | support-closure | `buf.validate.BoolRules`        |
|  [20]   | `StringRulesSchema`      | message | support-closure | `buf.validate.StringRules`      |
|  [21]   | `BytesRulesSchema`       | message | support-closure | `buf.validate.BytesRules`       |
|  [22]   | `EnumRulesSchema`        | message | support-closure | `buf.validate.EnumRules`        |
|  [23]   | `RepeatedRulesSchema`    | message | support-closure | `buf.validate.RepeatedRules`    |
|  [24]   | `MapRulesSchema`         | message | support-closure | `buf.validate.MapRules`         |
|  [25]   | `AnyRulesSchema`         | message | support-closure | `buf.validate.AnyRules`         |
|  [26]   | `DurationRulesSchema`    | message | support-closure | `buf.validate.DurationRules`    |
|  [27]   | `FieldMaskRulesSchema`   | message | support-closure | `buf.validate.FieldMaskRules`   |
|  [28]   | `TimestampRulesSchema`   | message | support-closure | `buf.validate.TimestampRules`   |
|  [29]   | `IgnoreSchema`           | enum    | support-closure | `buf.validate.Ignore`           |
|  [30]   | `KnownRegexSchema`       | enum    | support-closure | `buf.validate.KnownRegex`       |

[ROSTER_SCOPE]: `rasm.contracts.artifact.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                    |
| :-----: | :------------------ | :------ | :-------------- | :--------------------------------------- |
|  [01]   | `ArtifactRefSchema` | message | support-closure | `rasm.contracts.artifact.v1.ArtifactRef` |

[ROSTER_SCOPE]: `rasm.contracts.appearance.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                      | [KIND]  | [ORIGIN]        | [FQN]                                                              |
| :-----: | :------------------------------------------ | :------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `PlaneRefSchema`                            | message | support-closure | `rasm.contracts.appearance.v1.PlaneRef`                            |
|  [02]   | `PlaneSchema`                               | message | support-closure | `rasm.contracts.appearance.v1.Plane`                               |
|  [03]   | `PackRowSchema`                             | message | support-closure | `rasm.contracts.appearance.v1.PackRow`                             |
|  [04]   | `RoleSchema`                                | enum    | support-closure | `rasm.contracts.appearance.v1.Role`                                |
|  [05]   | `TransferSchema`                            | enum    | support-closure | `rasm.contracts.appearance.v1.Transfer`                            |
|  [06]   | `NormalConventionSchema`                    | enum    | support-closure | `rasm.contracts.appearance.v1.NormalConvention`                    |
|  [07]   | `AlphaModeSchema`                           | enum    | support-closure | `rasm.contracts.appearance.v1.AlphaMode`                           |
|  [08]   | `ContainerSchema`                           | enum    | support-closure | `rasm.contracts.appearance.v1.Container`                           |
|  [09]   | `PackSchema`                                | enum    | support-closure | `rasm.contracts.appearance.v1.Pack`                                |
|  [10]   | `PlaneFormatSchema`                         | enum    | support-closure | `rasm.contracts.appearance.v1.PlaneFormat`                         |
|  [11]   | `MipPolicySchema`                           | enum    | support-closure | `rasm.contracts.appearance.v1.MipPolicy`                           |
|  [12]   | `KtxPayloadSchema`                          | enum    | support-closure | `rasm.contracts.appearance.v1.KtxPayload`                          |
|  [13]   | `BlockFormatSchema`                         | enum    | support-closure | `rasm.contracts.appearance.v1.BlockFormat`                         |
|  [14]   | `LayerLawSchema`                            | enum    | support-closure | `rasm.contracts.appearance.v1.LayerLaw`                            |
|  [15]   | `LicenseClassSchema`                        | enum    | support-closure | `rasm.contracts.appearance.v1.LicenseClass`                        |
|  [16]   | `UdimSchema`                                | enum    | support-closure | `rasm.contracts.appearance.v1.Udim`                                |
|  [17]   | `PrimariesSchema`                           | enum    | support-closure | `rasm.contracts.appearance.v1.Primaries`                           |
|  [18]   | `DepthSchema`                               | enum    | support-closure | `rasm.contracts.appearance.v1.Depth`                               |
|  [19]   | `ToolSchema`                                | enum    | support-closure | `rasm.contracts.appearance.v1.Tool`                                |
|  [20]   | `EnvironmentPlaneSchema`                    | message | support-closure | `rasm.contracts.appearance.v1.EnvironmentPlane`                    |
|  [21]   | `EnvironmentSourceSchema`                   | message | support-closure | `rasm.contracts.appearance.v1.EnvironmentSource`                   |
|  [22]   | `HdriSchema`                                | message | support-closure | `rasm.contracts.appearance.v1.Hdri`                                |
|  [23]   | `IblSchema`                                 | message | support-closure | `rasm.contracts.appearance.v1.Ibl`                                 |
|  [24]   | `ProvenanceSchema`                          | message | support-closure | `rasm.contracts.appearance.v1.Provenance`                          |
|  [25]   | `Provenance_CaptureSchema`                  | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Capture`                  |
|  [26]   | `Provenance_FitSchema`                      | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Fit`                      |
|  [27]   | `Provenance_InferenceSchema`                | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Inference`                |
|  [28]   | `Provenance_ChromaticitySchema`             | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity`             |
|  [29]   | `Provenance_Chromaticity_DominanceSchema`   | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity.Dominance`   |
|  [30]   | `Provenance_Chromaticity_TemperatureSchema` | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity.Temperature` |
|  [31]   | `Provenance_CardSchema`                     | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Card`                     |
|  [32]   | `Provenance_IngestSchema`                   | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Ingest`                   |
|  [33]   | `PressSchema`                               | message | support-closure | `rasm.contracts.appearance.v1.Press`                               |
|  [34]   | `SurfaceSetSchema`                          | message | support-closure | `rasm.contracts.appearance.v1.SurfaceSet`                          |
|  [35]   | `BakedSetSchema`                            | message | support-closure | `rasm.contracts.appearance.v1.BakedSet`                            |
|  [36]   | `EnvironmentSetSchema`                      | message | support-closure | `rasm.contracts.appearance.v1.EnvironmentSet`                      |
|  [37]   | `SetSchema`                                 | message | public-root     | `rasm.contracts.appearance.v1.Set`                                 |
|  [38]   | `ColorSchema`                               | message | support-closure | `rasm.contracts.appearance.v1.Color`                               |
|  [39]   | `OpenPbrSchema`                             | message | support-closure | `rasm.contracts.appearance.v1.OpenPbr`                             |
|  [40]   | `EmissionReadoutSchema`                     | message | support-closure | `rasm.contracts.appearance.v1.EmissionReadout`                     |
|  [41]   | `EmissionSchema`                            | message | support-closure | `rasm.contracts.appearance.v1.Emission`                            |
|  [42]   | `MaterialSchema`                            | message | public-root     | `rasm.contracts.appearance.v1.Material`                            |

[ROSTER_SCOPE]: `rasm.contracts.compute.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND] | [ORIGIN]        | [FQN]                                        |
| :-----: | :----------------------- | :----- | :-------------- | :------------------------------------------- |
|  [01]   | `DegradationLevelSchema` | enum   | support-closure | `rasm.contracts.compute.v1.DegradationLevel` |

[ROSTER_SCOPE]: `rasm.contracts.availability.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [FQN]                                                        |
| :-----: | :---------------------------------- | :------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `CommandVerdictWireSchema`          | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire`          |
|  [02]   | `CommandVerdictWire_GatedSchema`    | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire.Gated`    |
|  [03]   | `CommandVerdictWire_WithheldSchema` | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire.Withheld` |
|  [04]   | `CommandAvailabilitySchema`         | message | public-root     | `rasm.contracts.availability.v1.CommandAvailability`         |

[ROSTER_SCOPE]: `rasm.contracts.spatial.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND]  | [ORIGIN]        | [FQN]                                      |
| :-----: | :--------------------- | :------ | :-------------- | :----------------------------------------- |
|  [01]   | `Point3Schema`         | message | support-closure | `rasm.contracts.spatial.v1.Point3`         |
|  [02]   | `UnitDirection3Schema` | message | support-closure | `rasm.contracts.spatial.v1.UnitDirection3` |

[ROSTER_SCOPE]: `rasm.contracts.bcf.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [FQN]                                     |
| :-----: | :------------------------ | :------ | :-------------- | :---------------------------------------- |
|  [01]   | `BcfCameraWireSchema`     | message | support-closure | `rasm.contracts.bcf.v1.BcfCameraWire`     |
|  [02]   | `BcfColoringWireSchema`   | message | support-closure | `rasm.contracts.bcf.v1.BcfColoringWire`   |
|  [03]   | `BcfLineWireSchema`       | message | support-closure | `rasm.contracts.bcf.v1.BcfLineWire`       |
|  [04]   | `BcfClippingWireSchema`   | message | support-closure | `rasm.contracts.bcf.v1.BcfClippingWire`   |
|  [05]   | `BcfBitmapWireSchema`     | message | support-closure | `rasm.contracts.bcf.v1.BcfBitmapWire`     |
|  [06]   | `BcfSnippetWireSchema`    | message | support-closure | `rasm.contracts.bcf.v1.BcfSnippetWire`    |
|  [07]   | `BcfDocumentWireSchema`   | message | support-closure | `rasm.contracts.bcf.v1.BcfDocumentWire`   |
|  [08]   | `BcfFileWireSchema`       | message | support-closure | `rasm.contracts.bcf.v1.BcfFileWire`       |
|  [09]   | `BcfHintsWireSchema`      | message | support-closure | `rasm.contracts.bcf.v1.BcfHintsWire`      |
|  [10]   | `BcfShowingWireSchema`    | message | support-closure | `rasm.contracts.bcf.v1.BcfShowingWire`    |
|  [11]   | `BcfHidingWireSchema`     | message | support-closure | `rasm.contracts.bcf.v1.BcfHidingWire`     |
|  [12]   | `BcfVisibilityWireSchema` | message | support-closure | `rasm.contracts.bcf.v1.BcfVisibilityWire` |
|  [13]   | `BcfViewpointWireSchema`  | message | public-root     | `rasm.contracts.bcf.v1.BcfViewpointWire`  |
|  [14]   | `BcfCommentWireSchema`    | message | support-closure | `rasm.contracts.bcf.v1.BcfCommentWire`    |
|  [15]   | `BcfTopicWireSchema`      | message | public-root     | `rasm.contracts.bcf.v1.BcfTopicWire`      |
|  [16]   | `BcfStatusSchema`         | enum    | support-closure | `rasm.contracts.bcf.v1.BcfStatus`         |
|  [17]   | `BcfBitmapFormatSchema`   | enum    | support-closure | `rasm.contracts.bcf.v1.BcfBitmapFormat`   |

[ROSTER_SCOPE]: `rasm.contracts.benchmark.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                      | [KIND]  | [ORIGIN]        | [FQN]                                             |
| :-----: | :-------------------------- | :------ | :-------------- | :------------------------------------------------ |
|  [01]   | `LabelPairSchema`           | message | support-closure | `rasm.contracts.benchmark.v1.LabelPair`           |
|  [02]   | `HostFingerprintWireSchema` | message | support-closure | `rasm.contracts.benchmark.v1.HostFingerprintWire` |
|  [03]   | `BenchInputWireSchema`      | message | support-closure | `rasm.contracts.benchmark.v1.BenchInputWire`      |
|  [04]   | `ChromeTraceWireSchema`     | message | support-closure | `rasm.contracts.benchmark.v1.ChromeTraceWire`     |
|  [05]   | `BenchmarkExportWireSchema` | message | support-closure | `rasm.contracts.benchmark.v1.BenchmarkExportWire` |
|  [06]   | `EpContextWireSchema`       | message | support-closure | `rasm.contracts.benchmark.v1.EpContextWire`       |
|  [07]   | `ProfileArtifactWireSchema` | message | support-closure | `rasm.contracts.benchmark.v1.ProfileArtifactWire` |
|  [08]   | `BenchKernelWireSchema`     | message | support-closure | `rasm.contracts.benchmark.v1.BenchKernelWire`     |
|  [09]   | `BenchAggregateSchema`      | message | support-closure | `rasm.contracts.benchmark.v1.BenchAggregate`      |
|  [10]   | `RungCellSchema`            | message | support-closure | `rasm.contracts.benchmark.v1.RungCell`            |
|  [11]   | `BenchBandWireSchema`       | message | support-closure | `rasm.contracts.benchmark.v1.BenchBandWire`       |
|  [12]   | `BenchMetricSchema`         | message | support-closure | `rasm.contracts.benchmark.v1.BenchMetric`         |
|  [13]   | `BenchmarkClaimWireSchema`  | message | public-root     | `rasm.contracts.benchmark.v1.BenchmarkClaimWire`  |
|  [14]   | `BenchModalitySchema`       | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchModality`       |
|  [15]   | `BenchPolaritySchema`       | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchPolarity`       |
|  [16]   | `PayloadBandSchema`         | enum    | support-closure | `rasm.contracts.benchmark.v1.PayloadBand`         |
|  [17]   | `BenchRungSchema`           | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchRung`           |

[ROSTER_SCOPE]: `google.type` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]  | [ORIGIN]        | [FQN]                   |
| :-----: | :---------------- | :------ | :-------------- | :---------------------- |
|  [01]   | `DateSchema`      | message | support-closure | `google.type.Date`      |
|  [02]   | `DateTimeSchema`  | message | support-closure | `google.type.DateTime`  |
|  [03]   | `TimeZoneSchema`  | message | support-closure | `google.type.TimeZone`  |
|  [04]   | `TimeOfDaySchema` | message | support-closure | `google.type.TimeOfDay` |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [ORIGIN]        | [FQN]                                               |
| :-----: | :------------------------------ | :------ | :-------------- | :-------------------------------------------------- |
|  [01]   | `VectorWireSchema`              | message | support-closure | `rasm.contracts.element.v1.VectorWire`              |
|  [02]   | `DimensionWireSchema`           | message | support-closure | `rasm.contracts.element.v1.DimensionWire`           |
|  [03]   | `MeasureBandWireSchema`         | message | support-closure | `rasm.contracts.element.v1.MeasureBandWire`         |
|  [04]   | `MeasureValueWireSchema`        | message | support-closure | `rasm.contracts.element.v1.MeasureValueWire`        |
|  [05]   | `NamedMeasureWireSchema`        | message | support-closure | `rasm.contracts.element.v1.NamedMeasureWire`        |
|  [06]   | `CurvePointWireSchema`          | message | support-closure | `rasm.contracts.element.v1.CurvePointWire`          |
|  [07]   | `SampledCurveWireSchema`        | message | support-closure | `rasm.contracts.element.v1.SampledCurveWire`        |
|  [08]   | `PropertyValueWireSchema`       | message | support-closure | `rasm.contracts.element.v1.PropertyValueWire`       |
|  [09]   | `LogicalWireSchema`             | message | support-closure | `rasm.contracts.element.v1.LogicalWire`             |
|  [10]   | `EnumeratedWireSchema`          | message | support-closure | `rasm.contracts.element.v1.EnumeratedWire`          |
|  [11]   | `TemporalWireSchema`            | message | support-closure | `rasm.contracts.element.v1.TemporalWire`            |
|  [12]   | `ReferenceWireSchema`           | message | support-closure | `rasm.contracts.element.v1.ReferenceWire`           |
|  [13]   | `BoundedWireSchema`             | message | support-closure | `rasm.contracts.element.v1.BoundedWire`             |
|  [14]   | `ListWireSchema`                | message | support-closure | `rasm.contracts.element.v1.ListWire`                |
|  [15]   | `TableRowWireSchema`            | message | support-closure | `rasm.contracts.element.v1.TableRowWire`            |
|  [16]   | `TableWireSchema`               | message | support-closure | `rasm.contracts.element.v1.TableWire`               |
|  [17]   | `NamedValueWireSchema`          | message | support-closure | `rasm.contracts.element.v1.NamedValueWire`          |
|  [18]   | `ComplexWireSchema`             | message | support-closure | `rasm.contracts.element.v1.ComplexWire`             |
|  [19]   | `UncertaintyKindSchema`         | enum    | support-closure | `rasm.contracts.element.v1.UncertaintyKind`         |
|  [20]   | `InterpolationSchema`           | enum    | support-closure | `rasm.contracts.element.v1.Interpolation`           |
|  [21]   | `InheritanceModeSchema`         | enum    | support-closure | `rasm.contracts.element.v1.InheritanceMode`         |
|  [22]   | `EvidenceGradeSchema`           | enum    | support-closure | `rasm.contracts.element.v1.EvidenceGrade`           |
|  [23]   | `DiagnosticWireSchema`          | message | support-closure | `rasm.contracts.element.v1.DiagnosticWire`          |
|  [24]   | `ProvenanceWireSchema`          | message | support-closure | `rasm.contracts.element.v1.ProvenanceWire`          |
|  [25]   | `AssessmentWireSchema`          | message | support-closure | `rasm.contracts.element.v1.AssessmentWire`          |
|  [26]   | `ObservationChunkWireSchema`    | message | support-closure | `rasm.contracts.element.v1.ObservationChunkWire`    |
|  [27]   | `SensorProvenanceWireSchema`    | message | support-closure | `rasm.contracts.element.v1.SensorProvenanceWire`    |
|  [28]   | `GradeCountWireSchema`          | message | support-closure | `rasm.contracts.element.v1.GradeCountWire`          |
|  [29]   | `MomentsWireSchema`             | message | support-closure | `rasm.contracts.element.v1.MomentsWire`             |
|  [30]   | `SeriesStatisticsWireSchema`    | message | support-closure | `rasm.contracts.element.v1.SeriesStatisticsWire`    |
|  [31]   | `ObservationWireSchema`         | message | support-closure | `rasm.contracts.element.v1.ObservationWire`         |
|  [32]   | `ProjectedCrsWireSchema`        | message | support-closure | `rasm.contracts.element.v1.ProjectedCrsWire`        |
|  [33]   | `GeoReferenceWireSchema`        | message | support-closure | `rasm.contracts.element.v1.GeoReferenceWire`        |
|  [34]   | `CellLatticeWireSchema`         | message | support-closure | `rasm.contracts.element.v1.CellLatticeWire`         |
|  [35]   | `ColorBinWireSchema`            | message | support-closure | `rasm.contracts.element.v1.ColorBinWire`            |
|  [36]   | `CoverageBandWireSchema`        | message | support-closure | `rasm.contracts.element.v1.CoverageBandWire`        |
|  [37]   | `OverviewLevelWireSchema`       | message | support-closure | `rasm.contracts.element.v1.OverviewLevelWire`       |
|  [38]   | `CoverageWireSchema`            | message | support-closure | `rasm.contracts.element.v1.CoverageWire`            |
|  [39]   | `DisciplineSchema`              | enum    | support-closure | `rasm.contracts.element.v1.Discipline`              |
|  [40]   | `AssessmentOutcomeSchema`       | enum    | support-closure | `rasm.contracts.element.v1.AssessmentOutcome`       |
|  [41]   | `SolvePhaseSchema`              | enum    | support-closure | `rasm.contracts.element.v1.SolvePhase`              |
|  [42]   | `FailureKindSchema`             | enum    | support-closure | `rasm.contracts.element.v1.FailureKind`             |
|  [43]   | `SamplingKindSchema`            | enum    | support-closure | `rasm.contracts.element.v1.SamplingKind`            |
|  [44]   | `ObservationGradeSchema`        | enum    | support-closure | `rasm.contracts.element.v1.ObservationGrade`        |
|  [45]   | `CoverageKindSchema`            | enum    | support-closure | `rasm.contracts.element.v1.CoverageKind`            |
|  [46]   | `ChannelDtypeSchema`            | enum    | support-closure | `rasm.contracts.element.v1.ChannelDtype`            |
|  [47]   | `BandRoleSchema`                | enum    | support-closure | `rasm.contracts.element.v1.BandRole`                |
|  [48]   | `CrsResolutionSchema`           | enum    | support-closure | `rasm.contracts.element.v1.CrsResolution`           |
|  [49]   | `MaterialLayerWireSchema`       | message | support-closure | `rasm.contracts.element.v1.MaterialLayerWire`       |
|  [50]   | `LayerSetWireSchema`            | message | support-closure | `rasm.contracts.element.v1.LayerSetWire`            |
|  [51]   | `ProfileRefWireSchema`          | message | support-closure | `rasm.contracts.element.v1.ProfileRefWire`          |
|  [52]   | `MaterialProfileWireSchema`     | message | support-closure | `rasm.contracts.element.v1.MaterialProfileWire`     |
|  [53]   | `SectionPropertiesWireSchema`   | message | support-closure | `rasm.contracts.element.v1.SectionPropertiesWire`   |
|  [54]   | `ProfileSetWireSchema`          | message | support-closure | `rasm.contracts.element.v1.ProfileSetWire`          |
|  [55]   | `MaterialConstituentWireSchema` | message | support-closure | `rasm.contracts.element.v1.MaterialConstituentWire` |
|  [56]   | `ConstituentSetWireSchema`      | message | support-closure | `rasm.contracts.element.v1.ConstituentSetWire`      |
|  [57]   | `MaterialCompositionWireSchema` | message | support-closure | `rasm.contracts.element.v1.MaterialCompositionWire` |
|  [58]   | `AttestationWireSchema`         | message | support-closure | `rasm.contracts.element.v1.AttestationWire`         |
|  [59]   | `PropertyEvidenceWireSchema`    | message | support-closure | `rasm.contracts.element.v1.PropertyEvidenceWire`    |
|  [60]   | `MechanicalWireSchema`          | message | support-closure | `rasm.contracts.element.v1.MechanicalWire`          |
|  [61]   | `OrthotropicWireSchema`         | message | support-closure | `rasm.contracts.element.v1.OrthotropicWire`         |
|  [62]   | `ThermalWireSchema`             | message | support-closure | `rasm.contracts.element.v1.ThermalWire`             |
|  [63]   | `BandValueWireSchema`           | message | support-closure | `rasm.contracts.element.v1.BandValueWire`           |
|  [64]   | `AcousticWireSchema`            | message | support-closure | `rasm.contracts.element.v1.AcousticWire`            |
|  [65]   | `FireResistanceWireSchema`      | message | support-closure | `rasm.contracts.element.v1.FireResistanceWire`      |
|  [66]   | `FireWireSchema`                | message | support-closure | `rasm.contracts.element.v1.FireWire`                |
|  [67]   | `BandCellWireSchema`            | message | support-closure | `rasm.contracts.element.v1.BandCellWire`            |
|  [68]   | `EnvironmentalWireSchema`       | message | support-closure | `rasm.contracts.element.v1.EnvironmentalWire`       |
|  [69]   | `CostWireSchema`                | message | support-closure | `rasm.contracts.element.v1.CostWire`                |
|  [70]   | `RayleighWireSchema`            | message | support-closure | `rasm.contracts.element.v1.RayleighWire`            |
|  [71]   | `DampingWireSchema`             | message | support-closure | `rasm.contracts.element.v1.DampingWire`             |
|  [72]   | `HygrothermalWireSchema`        | message | support-closure | `rasm.contracts.element.v1.HygrothermalWire`        |
|  [73]   | `DurabilityWireSchema`          | message | support-closure | `rasm.contracts.element.v1.DurabilityWire`          |
|  [74]   | `OpticalWireSchema`             | message | support-closure | `rasm.contracts.element.v1.OpticalWire`             |
|  [75]   | `ElectricalWireSchema`          | message | support-closure | `rasm.contracts.element.v1.ElectricalWire`          |
|  [76]   | `MaterialPropertySetWireSchema` | message | support-closure | `rasm.contracts.element.v1.MaterialPropertySetWire` |
|  [77]   | `MaterialWireSchema`            | message | support-closure | `rasm.contracts.element.v1.MaterialWire`            |
|  [78]   | `AttestationRoleSchema`         | enum    | support-closure | `rasm.contracts.element.v1.AttestationRole`         |
|  [79]   | `FireRatingSchema`              | enum    | support-closure | `rasm.contracts.element.v1.FireRating`              |
|  [80]   | `SmokeClassSchema`              | enum    | support-closure | `rasm.contracts.element.v1.SmokeClass`              |
|  [81]   | `DropletClassSchema`            | enum    | support-closure | `rasm.contracts.element.v1.DropletClass`            |
|  [82]   | `MeasurementBasisSchema`        | enum    | support-closure | `rasm.contracts.element.v1.MeasurementBasis`        |
|  [83]   | `LifecycleBandSchema`           | enum    | support-closure | `rasm.contracts.element.v1.LifecycleBand`           |
|  [84]   | `AcousticBandSchema`            | enum    | support-closure | `rasm.contracts.element.v1.AcousticBand`            |
|  [85]   | `ClassificationWireSchema`      | message | support-closure | `rasm.contracts.element.v1.ClassificationWire`      |
|  [86]   | `OwnerHistoryWireSchema`        | message | support-closure | `rasm.contracts.element.v1.OwnerHistoryWire`        |
|  [87]   | `SchemaSpanWireSchema`          | message | support-closure | `rasm.contracts.element.v1.SchemaSpanWire`          |
|  [88]   | `PlacementWireSchema`           | message | support-closure | `rasm.contracts.element.v1.PlacementWire`           |
|  [89]   | `RepresentationWireSchema`      | message | support-closure | `rasm.contracts.element.v1.RepresentationWire`      |
|  [90]   | `ObjectWireSchema`              | message | support-closure | `rasm.contracts.element.v1.ObjectWire`              |
|  [91]   | `PropertySetWireSchema`         | message | support-closure | `rasm.contracts.element.v1.PropertySetWire`         |
|  [92]   | `GroupIdentityWireSchema`       | message | support-closure | `rasm.contracts.element.v1.GroupIdentityWire`       |
|  [93]   | `GroupWireSchema`               | message | support-closure | `rasm.contracts.element.v1.GroupWire`               |
|  [94]   | `QuantitySetWireSchema`         | message | support-closure | `rasm.contracts.element.v1.QuantitySetWire`         |
|  [95]   | `AppearanceWireSchema`          | message | support-closure | `rasm.contracts.element.v1.AppearanceWire`          |
|  [96]   | `NodeWireSchema`                | message | public-root     | `rasm.contracts.element.v1.NodeWire`                |
|  [97]   | `ObjectKindSchema`              | enum    | support-closure | `rasm.contracts.element.v1.ObjectKind`              |
|  [98]   | `ReleaseVersionSchema`          | enum    | support-closure | `rasm.contracts.element.v1.ReleaseVersion`          |
|  [99]   | `ChangeActionSchema`            | enum    | support-closure | `rasm.contracts.element.v1.ChangeAction`            |
|  [100]  | `ObjectStateSchema`             | enum    | support-closure | `rasm.contracts.element.v1.ObjectState`             |
|  [101]  | `RepresentationKindSchema`      | enum    | support-closure | `rasm.contracts.element.v1.RepresentationKind`      |
|  [102]  | `EditTombstoneSchema`           | message | support-closure | `rasm.contracts.element.v1.EditTombstone`           |
|  [103]  | `EditMembersSchema`             | message | support-closure | `rasm.contracts.element.v1.EditMembers`             |
|  [104]  | `EntityEditWireSchema`          | message | public-root     | `rasm.contracts.element.v1.EntityEditWire`          |

[ROSTER_SCOPE]: `rasm.contracts.declaration.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND] | [ORIGIN]        | [FQN]                                          |
| :-----: | :--------------------- | :----- | :-------------- | :--------------------------------------------- |
|  [01]   | `ImpactCategorySchema` | enum   | support-closure | `rasm.contracts.declaration.v1.ImpactCategory` |

[ROSTER_SCOPE]: `rasm.contracts.bim.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [FQN]                                     |
| :-----: | :------------------------ | :------ | :-------------- | :---------------------------------------- |
|  [01]   | `DeltaValueWireSchema`    | message | support-closure | `rasm.contracts.bim.v1.DeltaValueWire`    |
|  [02]   | `AspectDeltaWireSchema`   | message | support-closure | `rasm.contracts.bim.v1.AspectDeltaWire`   |
|  [03]   | `DiffEndWireSchema`       | message | support-closure | `rasm.contracts.bim.v1.DiffEndWire`       |
|  [04]   | `DiffModifiedWireSchema`  | message | support-closure | `rasm.contracts.bim.v1.DiffModifiedWire`  |
|  [05]   | `DiffMovedWireSchema`     | message | support-closure | `rasm.contracts.bim.v1.DiffMovedWire`     |
|  [06]   | `DiffRegroupWireSchema`   | message | support-closure | `rasm.contracts.bim.v1.DiffRegroupWire`   |
|  [07]   | `ElementChangeWireSchema` | message | support-closure | `rasm.contracts.bim.v1.ElementChangeWire` |
|  [08]   | `ModelDiffWireSchema`     | message | public-root     | `rasm.contracts.bim.v1.ModelDiffWire`     |
|  [09]   | `DeltaShapeSchema`        | enum    | support-closure | `rasm.contracts.bim.v1.DeltaShape`        |

[ROSTER_SCOPE]: `rasm.contracts.binding.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [FQN]                                                   |
| :-----: | :---------------------------------- | :------ | :-------------- | :------------------------------------------------------ |
|  [01]   | `BindingStatusSchema`               | message | public-root     | `rasm.contracts.binding.v1.BindingStatus`               |
|  [02]   | `ExternalTransportSchema`           | enum    | support-closure | `rasm.contracts.binding.v1.ExternalTransport`           |
|  [03]   | `BindingStateSchema`                | enum    | support-closure | `rasm.contracts.binding.v1.BindingState`                |
|  [04]   | `BindingDirectionSchema`            | enum    | support-closure | `rasm.contracts.binding.v1.BindingDirection`            |
|  [05]   | `CoercedValueWireSchema`            | message | public-root     | `rasm.contracts.binding.v1.CoercedValueWire`            |
|  [06]   | `WriteBackWireSchema`               | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire`               |
|  [07]   | `WriteBackWire_AcknowledgedSchema`  | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Acknowledged`  |
|  [08]   | `WriteBackWire_RejectedSchema`      | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Rejected`      |
|  [09]   | `WriteBackWire_RolledBackSchema`    | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.RolledBack`    |
|  [10]   | `WriteBackWire_IndeterminateSchema` | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Indeterminate` |
|  [11]   | `WriteReceiptWireSchema`            | message | public-root     | `rasm.contracts.binding.v1.WriteReceiptWire`            |
|  [12]   | `EchoClassSchema`                   | enum    | support-closure | `rasm.contracts.binding.v1.EchoClass`                   |

[ROSTER_SCOPE]: `google.rpc` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                            | [KIND]  | [ORIGIN]        | [FQN]                                  |
| :-----: | :-------------------------------- | :------ | :-------------- | :------------------------------------- |
|  [01]   | `RetryInfoSchema`                 | message | support-closure | `google.rpc.RetryInfo`                 |
|  [02]   | `BadRequestSchema`                | message | support-closure | `google.rpc.BadRequest`                |
|  [03]   | `BadRequest_FieldViolationSchema` | message | support-closure | `google.rpc.BadRequest.FieldViolation` |
|  [04]   | `LocalizedMessageSchema`          | message | support-closure | `google.rpc.LocalizedMessage`          |

[ROSTER_SCOPE]: `rasm.contracts.clock.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]      | [KIND]  | [ORIGIN]    | [FQN]                         |
| :-----: | :---------- | :------ | :---------- | :---------------------------- |
|  [01]   | `HlcSchema` | message | public-root | `rasm.contracts.clock.v1.Hlc` |

[ROSTER_SCOPE]: `rasm.contracts.fault.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND]  | [ORIGIN]        | [FQN]                                      |
| :-----: | :----------------------- | :------ | :-------------- | :----------------------------------------- |
|  [01]   | `FaultRecoverySchema`    | message | support-closure | `rasm.contracts.fault.v1.FaultRecovery`    |
|  [02]   | `FaultObservationSchema` | message | support-closure | `rasm.contracts.fault.v1.FaultObservation` |
|  [03]   | `FaultDetailSchema`      | message | public-root     | `rasm.contracts.fault.v1.FaultDetail`      |

[ROSTER_SCOPE]: `rasm.contracts.capability.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]    | [FQN]                                            |
| :-----: | :------------------------ | :------ | :---------- | :----------------------------------------------- |
|  [01]   | `DescriptorPinWireSchema` | message | public-root | `rasm.contracts.capability.v1.DescriptorPinWire` |

[ROSTER_SCOPE]: `rasm.contracts.crdt.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [ORIGIN]        | [FQN]                                  |
| :-----: | :-------------------- | :------ | :-------------- | :------------------------------------- |
|  [01]   | `ElementIdSchema`     | message | support-closure | `rasm.contracts.crdt.v1.ElementId`     |
|  [02]   | `VectorSlotSchema`    | message | support-closure | `rasm.contracts.crdt.v1.VectorSlot`    |
|  [03]   | `SetOpSchema`         | message | support-closure | `rasm.contracts.crdt.v1.SetOp`         |
|  [04]   | `WriteOpSchema`       | message | support-closure | `rasm.contracts.crdt.v1.WriteOp`       |
|  [05]   | `AddOpSchema`         | message | support-closure | `rasm.contracts.crdt.v1.AddOp`         |
|  [06]   | `RemoveOpSchema`      | message | support-closure | `rasm.contracts.crdt.v1.RemoveOp`      |
|  [07]   | `IncrementOpSchema`   | message | support-closure | `rasm.contracts.crdt.v1.IncrementOp`   |
|  [08]   | `InsertAfterOpSchema` | message | support-closure | `rasm.contracts.crdt.v1.InsertAfterOp` |
|  [09]   | `DeleteOpSchema`      | message | support-closure | `rasm.contracts.crdt.v1.DeleteOp`      |
|  [10]   | `MaintainOpSchema`    | message | support-closure | `rasm.contracts.crdt.v1.MaintainOp`    |
|  [11]   | `BeatOpSchema`        | message | support-closure | `rasm.contracts.crdt.v1.BeatOp`        |
|  [12]   | `LeaveOpSchema`       | message | support-closure | `rasm.contracts.crdt.v1.LeaveOp`       |
|  [13]   | `CrdtOpWireSchema`    | message | public-root     | `rasm.contracts.crdt.v1.CrdtOpWire`    |

[ROSTER_SCOPE]: `rasm.contracts.credential.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                       | [KIND]  | [ORIGIN]        | [FQN]                                               |
| :-----: | :--------------------------- | :------ | :-------------- | :-------------------------------------------------- |
|  [01]   | `CertificateChainSchema`     | message | support-closure | `rasm.contracts.credential.v1.CertificateChain`     |
|  [02]   | `CredentialPublicWireSchema` | message | public-root     | `rasm.contracts.credential.v1.CredentialPublicWire` |

[ROSTER_SCOPE]: `rasm.contracts.patch.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [FQN]                                  |
| :-----: | :------------------- | :------ | :-------------- | :------------------------------------- |
|  [01]   | `PatchAddSchema`     | message | support-closure | `rasm.contracts.patch.v1.PatchAdd`     |
|  [02]   | `PatchRemoveSchema`  | message | support-closure | `rasm.contracts.patch.v1.PatchRemove`  |
|  [03]   | `PatchReplaceSchema` | message | support-closure | `rasm.contracts.patch.v1.PatchReplace` |
|  [04]   | `PatchMoveSchema`    | message | support-closure | `rasm.contracts.patch.v1.PatchMove`    |
|  [05]   | `PatchCopySchema`    | message | support-closure | `rasm.contracts.patch.v1.PatchCopy`    |
|  [06]   | `PatchTestSchema`    | message | support-closure | `rasm.contracts.patch.v1.PatchTest`    |
|  [07]   | `PatchOpSchema`      | message | support-closure | `rasm.contracts.patch.v1.PatchOp`      |

[ROSTER_SCOPE]: `rasm.contracts.event.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [ORIGIN]    | [FQN]                                |
| :-----: | :----------------- | :------ | :---------- | :----------------------------------- |
|  [01]   | `ExtensionsSchema` | message | public-root | `rasm.contracts.event.v1.Extensions` |

[ROSTER_SCOPE]: `rasm.contracts.feature.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [FQN]                                       |
| :-----: | :---------------------- | :------ | :-------------- | :------------------------------------------ |
|  [01]   | `FlagVerdictWireSchema` | message | public-root     | `rasm.contracts.feature.v1.FlagVerdictWire` |
|  [02]   | `FlagReasonSchema`      | enum    | support-closure | `rasm.contracts.feature.v1.FlagReason`      |

[ROSTER_SCOPE]: `rasm.contracts.organization.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [FQN]                                         |
| :-----: | :------------------- | :------ | :-------------- | :-------------------------------------------- |
|  [01]   | `ViewOverrideSchema` | message | support-closure | `rasm.contracts.organization.v1.ViewOverride` |
|  [02]   | `EntitySchema`       | message | support-closure | `rasm.contracts.organization.v1.Entity`       |
|  [03]   | `EntityPathSchema`   | message | support-closure | `rasm.contracts.organization.v1.EntityPath`   |
|  [04]   | `OrganizationSchema` | message | public-root     | `rasm.contracts.organization.v1.Organization` |

[ROSTER_SCOPE]: `rasm.contracts.parity.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [FQN]                                   |
| :-----: | :------------------- | :------ | :-------------- | :-------------------------------------- |
|  [01]   | `ArtifactSchema`     | message | support-closure | `rasm.contracts.parity.v1.Artifact`     |
|  [02]   | `CapabilitySchema`   | message | support-closure | `rasm.contracts.parity.v1.Capability`   |
|  [03]   | `BackendSchema`      | message | public-root     | `rasm.contracts.parity.v1.Backend`      |
|  [04]   | `ArtifactRoleSchema` | enum    | support-closure | `rasm.contracts.parity.v1.ArtifactRole` |
|  [05]   | `ProviderSchema`     | enum    | support-closure | `rasm.contracts.parity.v1.Provider`     |
|  [06]   | `FailureRankSchema`  | enum    | support-closure | `rasm.contracts.parity.v1.FailureRank`  |
|  [07]   | `RestartClassSchema` | enum    | support-closure | `rasm.contracts.parity.v1.RestartClass` |

[ROSTER_SCOPE]: `rasm.contracts.receipt.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [FQN]                                         |
| :-----: | :------------------------ | :------ | :-------------- | :-------------------------------------------- |
|  [01]   | `TenantContextWireSchema` | message | support-closure | `rasm.contracts.receipt.v1.TenantContextWire` |
|  [02]   | `ReceiptHeaderWireSchema` | message | support-closure | `rasm.contracts.receipt.v1.ReceiptHeaderWire` |
|  [03]   | `PackageSchema`           | enum    | support-closure | `rasm.contracts.receipt.v1.Package`           |

[ROSTER_SCOPE]: `rasm.contracts.render.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [FQN]                                                  |
| :-----: | :---------------------------------- | :------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `ViewCameraWireSchema`              | message | support-closure | `rasm.contracts.render.v1.ViewCameraWire`              |
|  [02]   | `ViewCameraWire_FrameSchema`        | message | support-closure | `rasm.contracts.render.v1.ViewCameraWire.Frame`        |
|  [03]   | `ViewCameraWire_PerspectiveSchema`  | message | support-closure | `rasm.contracts.render.v1.ViewCameraWire.Perspective`  |
|  [04]   | `ViewCameraWire_OrthographicSchema` | message | support-closure | `rasm.contracts.render.v1.ViewCameraWire.Orthographic` |
|  [05]   | `ViewCameraWire_AsymmetricSchema`   | message | support-closure | `rasm.contracts.render.v1.ViewCameraWire.Asymmetric`   |
|  [06]   | `SectionBoxWireSchema`              | message | support-closure | `rasm.contracts.render.v1.SectionBoxWire`              |
|  [07]   | `VisibilityOverrideWireSchema`      | message | support-closure | `rasm.contracts.render.v1.VisibilityOverrideWire`      |
|  [08]   | `ViewMeasurementPointWireSchema`    | message | support-closure | `rasm.contracts.render.v1.ViewMeasurementPointWire`    |
|  [09]   | `ViewMeasurementWireSchema`         | message | support-closure | `rasm.contracts.render.v1.ViewMeasurementWire`         |
|  [10]   | `ViewpointWireSchema`               | message | support-closure | `rasm.contracts.render.v1.ViewpointWire`               |
|  [11]   | `SphereWireSchema`                  | message | support-closure | `rasm.contracts.render.v1.SphereWire`                  |
|  [12]   | `MeshoptStreamSchema`               | message | support-closure | `rasm.contracts.render.v1.MeshoptStream`               |
|  [13]   | `MeshletSchema`                     | message | support-closure | `rasm.contracts.render.v1.Meshlet`                     |
|  [14]   | `ResidencyTileWireSchema`           | message | support-closure | `rasm.contracts.render.v1.ResidencyTileWire`           |
|  [15]   | `GeometryResidencySchema`           | message | public-root     | `rasm.contracts.render.v1.GeometryResidency`           |
|  [16]   | `ResidencyKindSchema`               | enum    | support-closure | `rasm.contracts.render.v1.ResidencyKind`               |
|  [17]   | `StreamModeSchema`                  | enum    | support-closure | `rasm.contracts.render.v1.StreamMode`                  |
|  [18]   | `StreamFilterSchema`                | enum    | support-closure | `rasm.contracts.render.v1.StreamFilter`                |

[ROSTER_SCOPE]: `rasm.contracts.ui.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                    | [KIND]  | [ORIGIN]        | [FQN]                                                    |
| :-----: | :---------------------------------------- | :------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `CommandGateWireSchema`                   | message | public-root     | `rasm.contracts.ui.v1.CommandGateWire`                   |
|  [02]   | `CommandPayloadWireSchema`                | message | support-closure | `rasm.contracts.ui.v1.CommandPayloadWire`                |
|  [03]   | `CommandPayloadWire_ManySchema`           | message | support-closure | `rasm.contracts.ui.v1.CommandPayloadWire.Many`           |
|  [04]   | `CommandInvocationSchema`                 | message | public-root     | `rasm.contracts.ui.v1.CommandInvocation`                 |
|  [05]   | `CommandOutcomeWireSchema`                | message | support-closure | `rasm.contracts.ui.v1.CommandOutcomeWire`                |
|  [06]   | `DeckReceiptWireSchema`                   | message | support-closure | `rasm.contracts.ui.v1.DeckReceiptWire`                   |
|  [07]   | `IconSlotWireSchema`                      | message | support-closure | `rasm.contracts.ui.v1.IconSlotWire`                      |
|  [08]   | `HintRowWireSchema`                       | message | support-closure | `rasm.contracts.ui.v1.HintRowWire`                       |
|  [09]   | `IntentBindingWireSchema`                 | message | support-closure | `rasm.contracts.ui.v1.IntentBindingWire`                 |
|  [10]   | `OptionRowWireSchema`                     | message | support-closure | `rasm.contracts.ui.v1.OptionRowWire`                     |
|  [11]   | `OptionSourceWireSchema`                  | message | support-closure | `rasm.contracts.ui.v1.OptionSourceWire`                  |
|  [12]   | `OptionSourceWire_InlineSchema`           | message | support-closure | `rasm.contracts.ui.v1.OptionSourceWire.Inline`           |
|  [13]   | `CrumbRowWireSchema`                      | message | support-closure | `rasm.contracts.ui.v1.CrumbRowWire`                      |
|  [14]   | `AvatarRowWireSchema`                     | message | support-closure | `rasm.contracts.ui.v1.AvatarRowWire`                     |
|  [15]   | `FileFilterRowWireSchema`                 | message | support-closure | `rasm.contracts.ui.v1.FileFilterRowWire`                 |
|  [16]   | `MenuRowWireSchema`                       | message | support-closure | `rasm.contracts.ui.v1.MenuRowWire`                       |
|  [17]   | `ToolbarRowWireSchema`                    | message | support-closure | `rasm.contracts.ui.v1.ToolbarRowWire`                    |
|  [18]   | `SectionWireSchema`                       | message | support-closure | `rasm.contracts.ui.v1.SectionWire`                       |
|  [19]   | `ExtentWireSchema`                        | message | support-closure | `rasm.contracts.ui.v1.ExtentWire`                        |
|  [20]   | `ColumnRowWireSchema`                     | message | support-closure | `rasm.contracts.ui.v1.ColumnRowWire`                     |
|  [21]   | `WindowWireSchema`                        | message | support-closure | `rasm.contracts.ui.v1.WindowWire`                        |
|  [22]   | `NumericRangeWireSchema`                  | message | support-closure | `rasm.contracts.ui.v1.NumericRangeWire`                  |
|  [23]   | `NumericRangeWire_IntegralSchema`         | message | support-closure | `rasm.contracts.ui.v1.NumericRangeWire.Integral`         |
|  [24]   | `NumericRangeWire_UnsignedSchema`         | message | support-closure | `rasm.contracts.ui.v1.NumericRangeWire.Unsigned`         |
|  [25]   | `NumericRangeWire_RealSchema`             | message | support-closure | `rasm.contracts.ui.v1.NumericRangeWire.Real`             |
|  [26]   | `NumericRangeWire_PreciseSchema`          | message | support-closure | `rasm.contracts.ui.v1.NumericRangeWire.Precise`          |
|  [27]   | `ControlIntentWireSchema`                 | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire`                 |
|  [28]   | `ControlIntentWire_ButtonSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Button`          |
|  [29]   | `ControlIntentWire_LabelSchema`           | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Label`           |
|  [30]   | `ControlIntentWire_TextInputSchema`       | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.TextInput`       |
|  [31]   | `ControlIntentWire_NumberInputSchema`     | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.NumberInput`     |
|  [32]   | `ControlIntentWire_DateInputSchema`       | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.DateInput`       |
|  [33]   | `ControlIntentWire_PathInputSchema`       | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.PathInput`       |
|  [34]   | `ControlIntentWire_ColorInputSchema`      | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.ColorInput`      |
|  [35]   | `ControlIntentWire_SelectSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Select`          |
|  [36]   | `ControlIntentWire_MultiSelectSchema`     | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.MultiSelect`     |
|  [37]   | `ControlIntentWire_SliderSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Slider`          |
|  [38]   | `ControlIntentWire_RangeSchema`           | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Range`           |
|  [39]   | `ControlIntentWire_ToggleSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Toggle`          |
|  [40]   | `ControlIntentWire_RadioSchema`           | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Radio`           |
|  [41]   | `ControlIntentWire_SegmentedSchema`       | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Segmented`       |
|  [42]   | `ControlIntentWire_ChipSchema`            | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Chip`            |
|  [43]   | `ControlIntentWire_ProgressSchema`        | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Progress`        |
|  [44]   | `ControlIntentWire_AvatarSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Avatar`          |
|  [45]   | `ControlIntentWire_BreadcrumbSchema`      | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Breadcrumb`      |
|  [46]   | `ControlIntentWire_TooltipSchema`         | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Tooltip`         |
|  [47]   | `ControlIntentWire_BannerSchema`          | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Banner`          |
|  [48]   | `ControlIntentWire_EmptyStateSchema`      | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.EmptyState`      |
|  [49]   | `ControlIntentWire_GridSchema`            | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Grid`            |
|  [50]   | `ControlIntentWire_TreeSchema`            | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Tree`            |
|  [51]   | `ControlIntentWire_OverviewSchema`        | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Overview`        |
|  [52]   | `ControlIntentWire_MenuSchema`            | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Menu`            |
|  [53]   | `ControlIntentWire_ToolbarSchema`         | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Toolbar`         |
|  [54]   | `ControlIntentWire_TabSchema`             | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Tab`             |
|  [55]   | `ControlIntentWire_AccordionSchema`       | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Accordion`       |
|  [56]   | `ControlIntentWire_PanelSchema`           | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Panel`           |
|  [57]   | `ControlIntentWire_DockSchema`            | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Dock`            |
|  [58]   | `ControlIntentWire_SplitterSchema`        | message | support-closure | `rasm.contracts.ui.v1.ControlIntentWire.Splitter`        |
|  [59]   | `ControlEmphasisSchema`                   | enum    | support-closure | `rasm.contracts.ui.v1.ControlEmphasis`                   |
|  [60]   | `ControlTriggerSchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.ControlTrigger`                    |
|  [61]   | `IconPlacementSchema`                     | enum    | support-closure | `rasm.contracts.ui.v1.IconPlacement`                     |
|  [62]   | `NumericKindSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.NumericKind`                       |
|  [63]   | `TemporalKindSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.TemporalKind`                      |
|  [64]   | `PickerModeSchema`                        | enum    | support-closure | `rasm.contracts.ui.v1.PickerMode`                        |
|  [65]   | `ColorPostureSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.ColorPosture`                      |
|  [66]   | `SelectPostureSchema`                     | enum    | support-closure | `rasm.contracts.ui.v1.SelectPosture`                     |
|  [67]   | `MultiPostureSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.MultiPosture`                      |
|  [68]   | `SegmentPostureSchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.SegmentPosture`                    |
|  [69]   | `ChipPostureSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.ChipPosture`                       |
|  [70]   | `ProgressFormSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.ProgressForm`                      |
|  [71]   | `MenuPostureSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.MenuPosture`                       |
|  [72]   | `OverflowModeSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.OverflowMode`                      |
|  [73]   | `OrientationSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.Orientation`                       |
|  [74]   | `ExtentModeSchema`                        | enum    | support-closure | `rasm.contracts.ui.v1.ExtentMode`                        |
|  [75]   | `ExtentUnitSchema`                        | enum    | support-closure | `rasm.contracts.ui.v1.ExtentUnit`                        |
|  [76]   | `ColumnAlignSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.ColumnAlign`                       |
|  [77]   | `BannerSeveritySchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.BannerSeverity`                    |
|  [78]   | `BannerPlacementSchema`                   | enum    | support-closure | `rasm.contracts.ui.v1.BannerPlacement`                   |
|  [79]   | `OverviewAxisSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.OverviewAxis`                      |
|  [80]   | `TypographyRoleSchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.TypographyRole`                    |
|  [81]   | `PixelIdentityWireSchema`                 | message | support-closure | `rasm.contracts.ui.v1.PixelIdentityWire`                 |
|  [82]   | `NativeAssetFactWireSchema`               | message | support-closure | `rasm.contracts.ui.v1.NativeAssetFactWire`               |
|  [83]   | `EvidenceReceiptWireSchema`               | message | public-root     | `rasm.contracts.ui.v1.EvidenceReceiptWire`               |
|  [84]   | `EvidenceReceiptWire_SurfaceSchema`       | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Surface`       |
|  [85]   | `EvidenceReceiptWire_FocusSchema`         | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Focus`         |
|  [86]   | `EvidenceReceiptWire_RenderSchema`        | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Render`        |
|  [87]   | `EvidenceReceiptWire_DisposalSchema`      | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Disposal`      |
|  [88]   | `EvidenceReceiptWire_EditSchema`          | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Edit`          |
|  [89]   | `EvidenceReceiptWire_ThemeSchema`         | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Theme`         |
|  [90]   | `EvidenceReceiptWire_MotionSchema`        | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Motion`        |
|  [91]   | `EvidenceReceiptWire_EffectSchema`        | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Effect`        |
|  [92]   | `EvidenceReceiptWire_Effect_ExtentSchema` | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Effect.Extent` |
|  [93]   | `EvidenceReceiptWire_AssetSchema`         | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Asset`         |
|  [94]   | `EvidenceReceiptWire_LiveDataSchema`      | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.LiveData`      |
|  [95]   | `EvidenceReceiptWire_CollabSyncSchema`    | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.CollabSync`    |
|  [96]   | `EvidenceReceiptWire_CollabRevertSchema`  | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.CollabRevert`  |
|  [97]   | `EvidenceReceiptWire_MediaSchema`         | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Media`         |
|  [98]   | `EvidenceReceiptWire_QualitySchema`       | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Quality`       |
|  [99]   | `EvidenceReceiptWire_GpuFrameSchema`      | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.GpuFrame`      |
|  [100]  | `EvidenceReceiptWire_LayoutSchema`        | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Layout`        |
|  [101]  | `EvidenceReceiptWire_DispatcherLagSchema` | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.DispatcherLag` |
|  [102]  | `EvidenceReceiptWire_PreCommitSchema`     | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.PreCommit`     |
|  [103]  | `SkewBandWireSchema`                      | message | support-closure | `rasm.contracts.ui.v1.SkewBandWire`                      |
|  [104]  | `EvidenceRowWireSchema`                   | message | support-closure | `rasm.contracts.ui.v1.EvidenceRowWire`                   |
|  [105]  | `EvidenceTimelineWireSchema`              | message | public-root     | `rasm.contracts.ui.v1.EvidenceTimelineWire`              |
|  [106]  | `PixelLayoutSchema`                       | enum    | support-closure | `rasm.contracts.ui.v1.PixelLayout`                       |
|  [107]  | `MediaOutcomeSchema`                      | enum    | support-closure | `rasm.contracts.ui.v1.MediaOutcome`                      |
|  [108]  | `LayoutVarWireSchema`                     | message | support-closure | `rasm.contracts.ui.v1.LayoutVarWire`                     |
|  [109]  | `LayoutTermWireSchema`                    | message | support-closure | `rasm.contracts.ui.v1.LayoutTermWire`                    |
|  [110]  | `LayoutExprWireSchema`                    | message | support-closure | `rasm.contracts.ui.v1.LayoutExprWire`                    |
|  [111]  | `LayoutConstraintWireSchema`              | message | support-closure | `rasm.contracts.ui.v1.LayoutConstraintWire`              |
|  [112]  | `LayoutEditSchema`                        | message | support-closure | `rasm.contracts.ui.v1.LayoutEdit`                        |
|  [113]  | `LayoutValueSchema`                       | message | support-closure | `rasm.contracts.ui.v1.LayoutValue`                       |
|  [114]  | `LayoutProgramSchema`                     | message | support-closure | `rasm.contracts.ui.v1.LayoutProgram`                     |
|  [115]  | `LayoutRelationSchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.LayoutRelation`                    |
|  [116]  | `LayoutStrengthSchema`                    | enum    | support-closure | `rasm.contracts.ui.v1.LayoutStrength`                    |
|  [117]  | `AppUiSurfaceProgramSchema`               | message | public-root     | `rasm.contracts.ui.v1.AppUiSurfaceProgram`               |

[ASSET_SCOPE]: exact publisher projections emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]        | [PATH]                                                                |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `CloudEventsAvro` | readonly-json | `libs/typescript/contracts/gen/io/cloudevents/v1/cloudevents_avro.ts` |
<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root generation emits selected packages and reachable support descriptors into one clean module tree.
- `valid_types=protovalidate_required` carries corpus presence rules into each generated valid type.
- Package self-reference resolves every generated module through the proto-path wildcard.
- Assay restores manifest-distributed publisher assets after Buf's clean sweep.
- Publisher descriptors and assets remain direct codec inputs; estate registries retain estate descriptors alone.

[STACKING]:
- `@bufbuild/protobuf`(`../../.api/bufbuild-protobuf.md`): `<Name>Schema` drives create, decode, encode, equality, reflection, and validation typing.
- `@connectrpc/connect`(`core/.api/connectrpc-connect.md`): `createClient(<Service>, transport)` derives the client surface from `GenService`.
- `core/interchange/format`: estate schemas enter the registry by generated descriptor; `CloudEventSchema` and `CloudEventBatchSchema` bind directly.
- `CloudEventsAvro`: exact publisher AVSC input to a consumer-owned Avro codec, without a second schema or package dependency.
- Consumer schema transforms convert validated generated values into domain values after codec admission.

[LOCAL_ADMISSION]:
- Consumers import `@rasm\/contracts/<proto path>_pb` and derive value types from the exported schema.
- Avro consumers import `CloudEventsAvro` from `@rasm\/contracts/io/cloudevents/v1/cloudevents_avro`.
- Corpus and generator changes regenerate the module tree; package code adds no barrel, copied descriptor, or handwritten message shape.

[RAIL_LAW]:
- Package: `@rasm/ts` contracts subpath
- Owns: generated descriptor/value declarations and exact publisher-asset projections for selected contracts
- Accept: schema-first codecs, validation, Connect derivation, and consumer-owned domain conversion
- Reject: handwritten proto shapes, copied publisher assets, descriptor mirrors, per-family exports, barrels, and hand edits under `gen`
