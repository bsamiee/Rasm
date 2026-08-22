# [TS_CONTRACTS_API_RASM_TS_CONTRACTS]

`@rasm/ts/contracts` owns the generated TypeScript bindings of every corpus `.proto`: one `_pb.ts` module per source carrying its `GenFile`, one `<Name>`/`<Name>Schema` pair per message, and one `GenService` per service, reached by module path and consumed through the `@bufbuild/protobuf` schema-first codec.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@rasm/ts/contracts`
- package: `@rasm/ts/contracts`
- module: ESM `.ts` under the `@rasm/ts` `exports` wildcard `./contracts/*` → `./contracts/gen/*.ts`; specifier `@rasm/ts/contracts/<proto path>_pb`
- runtime: universal — every module imports `@bufbuild/protobuf/codegenv2` and `@bufbuild/protobuf/wkt` alone
- depends: `@bufbuild/protoc-gen-es` ↔ `@bufbuild/protobuf` as one pinned lineage
- rail: generated contract bindings — the descriptors every decode, encode, parity walk, and Connect client binds

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generator symbol grammar — how `rasm.contracts.<family>.v1.<Name>` spells in every emitted module
- `<Name>` is the decoded VALUE type and `<Name>Schema` the descriptor every operation takes first; a consumer imports the pair from one module.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :---------------------------- | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `<Name>`                      | message type       | `Message<"<package>.<Name>"> & { fields }` — the `create`/`fromBinary` value       |
|  [02]   | `<Name>Schema`                | `GenMessage<Name>` | the descriptor `create`, `fromBinary`, `toBinary`, `equals`, `reflect` take        |
|  [03]   | `file_<proto path, _-joined>` | `GenFile`          | the file descriptor `messageDesc(file, n)` indexes every schema off                |
|  [04]   | `<Service>`                   | `GenService`       | the service descriptor `createClient` consumes; keys are camelCase `localName`     |
|  [05]   | `<Parent>_<Child>`            | nested pair        | a message declared inside another spells its parent chain with `_`                 |
|  [06]   | oneof `<group>`               | tagged union       | `{ case: "<camelField>"; value: T } \| { case: undefined; value?: undefined }`     |
|  [07]   | scalar leaves                 | field types        | int64 kinds as `bigint`, `bytes` as `Uint8Array`, maps as `{ [key: string]: T }`   |
|  [08]   | `Timestamp` / `Empty` / `Any` | `wkt` fields       | `@bufbuild/protobuf/wkt` imports, never re-emitted; `Struct` lands as `JsonObject` |
|  [09]   | `<Enum>` / `<Enum>Schema`     | object enum        | `erasable_syntax=true` emits an `as const` object with its derived type            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `rasm.contracts.channels.v1` — `@rasm/ts/contracts/rasm/contracts/channels/v1/channels_pb`

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `file_rasm_contracts_channels_v1_channels` | file    | `rasm.contracts.channels.v1` file descriptor  |
|  [02]   | `WireProvenanceSchema`                     | message | `rasm.contracts.channels.v1.WireProvenance`   |
|  [03]   | `PlaneRefWireSchema`                       | message | `rasm.contracts.channels.v1.PlaneRefWire`     |
|  [04]   | `PlaneRefSchema`                           | message | `rasm.contracts.channels.v1.PlaneRef`         |
|  [05]   | `ChannelWireSchema`                        | message | `rasm.contracts.channels.v1.ChannelWire`      |
|  [06]   | `PackWireSchema`                           | message | `rasm.contracts.channels.v1.PackWire`         |
|  [07]   | `PressReceiptWireSchema`                   | message | `rasm.contracts.channels.v1.PressReceiptWire` |
|  [08]   | `TextureSetWireSchema`                     | message | `rasm.contracts.channels.v1.TextureSetWire`   |
|  [09]   | `MapEntrySchema`                           | message | `rasm.contracts.channels.v1.MapEntry`         |
|  [10]   | `PackEntrySchema`                          | message | `rasm.contracts.channels.v1.PackEntry`        |
|  [11]   | `IblEntrySchema`                           | message | `rasm.contracts.channels.v1.IblEntry`         |
|  [12]   | `AssetSetManifestSchema`                   | message | `rasm.contracts.channels.v1.AssetSetManifest` |

[ENTRYPOINT_SCOPE]: `rasm.contracts.compute.v1` — `@rasm/ts/contracts/rasm/contracts/compute/v1/compute_pb`

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                                                       |
| :-----: | :--------------------------------------- | :------ | :--------------------------------------------------------------------------------- |
|  [01]   | `file_rasm_contracts_compute_v1_compute` | file    | `rasm.contracts.compute.v1` file descriptor                                        |
|  [02]   | `FaultRecoverySchema`                    | message | `rasm.contracts.compute.v1.FaultRecovery`                                          |
|  [03]   | `FaultDetailSchema`                      | message | `rasm.contracts.compute.v1.FaultDetail`                                            |
|  [04]   | `SymbolicDimSchema`                      | message | `rasm.contracts.compute.v1.SymbolicDim`                                            |
|  [05]   | `PointCloudTensorSchema`                 | message | `rasm.contracts.compute.v1.PointCloudTensor`                                       |
|  [06]   | `MeshTensorSchema`                       | message | `rasm.contracts.compute.v1.MeshTensor`                                             |
|  [07]   | `VoxelTensorSchema`                      | message | `rasm.contracts.compute.v1.VoxelTensor`                                            |
|  [08]   | `GeometryPayloadSchema`                  | message | `rasm.contracts.compute.v1.GeometryPayload`                                        |
|  [09]   | `GaussianSplatScanSchema`                | message | `rasm.contracts.compute.v1.GaussianSplatScan`                                      |
|  [10]   | `TransactionRequestSchema`               | message | `rasm.contracts.compute.v1.TransactionRequest`                                     |
|  [11]   | `TransactionReceiptSchema`               | message | `rasm.contracts.compute.v1.TransactionReceipt`                                     |
|  [12]   | `QueryRequestSchema`                     | message | `rasm.contracts.compute.v1.QueryRequest`                                           |
|  [13]   | `QueryResponseSchema`                    | message | `rasm.contracts.compute.v1.QueryResponse`                                          |
|  [14]   | `InferRequestSchema`                     | message | `rasm.contracts.compute.v1.InferRequest`                                           |
|  [15]   | `InferResponseSchema`                    | message | `rasm.contracts.compute.v1.InferResponse`                                          |
|  [16]   | `SolveRequestSchema`                     | message | `rasm.contracts.compute.v1.SolveRequest`                                           |
|  [17]   | `SolveResponseSchema`                    | message | `rasm.contracts.compute.v1.SolveResponse`                                          |
|  [18]   | `GenerateRequestSchema`                  | message | `rasm.contracts.compute.v1.GenerateRequest`                                        |
|  [19]   | `TokenChunkSchema`                       | message | `rasm.contracts.compute.v1.TokenChunk`                                             |
|  [20]   | `GraphDiffRequestSchema`                 | message | `rasm.contracts.compute.v1.GraphDiffRequest`                                       |
|  [21]   | `GraphDiffResponseSchema`                | message | `rasm.contracts.compute.v1.GraphDiffResponse`                                      |
|  [22]   | `SubtreeFetchRequestSchema`              | message | `rasm.contracts.compute.v1.SubtreeFetchRequest`                                    |
|  [23]   | `GraphChunkSchema`                       | message | `rasm.contracts.compute.v1.GraphChunk`                                             |
|  [24]   | `TessellationRequestSchema`              | message | `rasm.contracts.compute.v1.TessellationRequest`                                    |
|  [25]   | `TessellationReceiptSchema`              | message | `rasm.contracts.compute.v1.TessellationReceipt`                                    |
|  [26]   | `DispatchToolRequestSchema`              | message | `rasm.contracts.compute.v1.DispatchToolRequest`                                    |
|  [27]   | `DispatchReceiptSchema`                  | message | `rasm.contracts.compute.v1.DispatchReceipt`                                        |
|  [28]   | `CommandReplySchema`                     | message | `rasm.contracts.compute.v1.CommandReply`                                           |
|  [29]   | `DispatchPatchRequestSchema`             | message | `rasm.contracts.compute.v1.DispatchPatchRequest`                                   |
|  [30]   | `ReloadReplySchema`                      | message | `rasm.contracts.compute.v1.ReloadReply`                                            |
|  [31]   | `SetDegradationRequestSchema`            | message | `rasm.contracts.compute.v1.SetDegradationRequest`                                  |
|  [32]   | `DegradationReplySchema`                 | message | `rasm.contracts.compute.v1.DegradationReply`                                       |
|  [33]   | `DrainRuntimeRequestSchema`              | message | `rasm.contracts.compute.v1.DrainRuntimeRequest`                                    |
|  [34]   | `DrainStepRowSchema`                     | message | `rasm.contracts.compute.v1.DrainStepRow`                                           |
|  [35]   | `DrainReplySchema`                       | message | `rasm.contracts.compute.v1.DrainReply`                                             |
|  [36]   | `SupportBundleRequestSchema`             | message | `rasm.contracts.compute.v1.SupportBundleRequest`                                   |
|  [37]   | `SupportBundleReplySchema`               | message | `rasm.contracts.compute.v1.SupportBundleReply`                                     |
|  [38]   | `ArtifactFrameSchema`                    | message | `rasm.contracts.compute.v1.ArtifactFrame`                                          |
|  [39]   | `ComputeService`                         | service | `infer`, `solve`, `generate`, `graphDiff`, `subtreeFetch`, `tessellate`            |
|  [40]   | `DocumentService`                        | service | `executeTransaction`, `query`                                                      |
|  [41]   | `ControlService`                         | service | `reloadOptions`, `dispatchTool`, `dispatchPatch`, `setDegradation`, `drainRuntime` |
|  [42]   | `DiagnosticService`                      | service | `captureBundle`                                                                    |
|  [43]   | `ArtifactSyncService`                    | service | `sync`                                                                             |

- `ComputeService`: `generate` is server-streaming; `subtreeFetch` is server-streaming
- `ArtifactSyncService`: `sync` is bidi-streaming

[ENTRYPOINT_SCOPE]: `rasm.contracts.element.v1` — `@rasm/ts/contracts/rasm/contracts/element/v1/element_pb`

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :--------------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `file_rasm_contracts_element_v1_element` | file    | `rasm.contracts.element.v1` file descriptor             |
|  [02]   | `ElementGraphWireSchema`                 | message | `rasm.contracts.element.v1.ElementGraphWire`            |
|  [03]   | `RedactionManifestWireSchema`            | message | `rasm.contracts.element.v1.RedactionManifestWire`       |
|  [04]   | `GraphDeltaWireSchema`                   | message | `rasm.contracts.element.v1.GraphDeltaWire`              |
|  [05]   | `NodeRevisionWireSchema`                 | message | `rasm.contracts.element.v1.NodeRevisionWire`            |
|  [06]   | `HeaderWireSchema`                       | message | `rasm.contracts.element.v1.HeaderWire`                  |
|  [07]   | `UnitAxisWireSchema`                     | message | `rasm.contracts.element.v1.UnitAxisWire`                |
|  [08]   | `NodeWireSchema`                         | message | `rasm.contracts.element.v1.NodeWire`                    |
|  [09]   | `ObjectWireSchema`                       | message | `rasm.contracts.element.v1.ObjectWire`                  |
|  [10]   | `PlacementWireSchema`                    | message | `rasm.contracts.element.v1.PlacementWire`               |
|  [11]   | `ClassificationWireSchema`               | message | `rasm.contracts.element.v1.ClassificationWire`          |
|  [12]   | `StepHeaderWireSchema`                   | message | `rasm.contracts.element.v1.StepHeaderWire`              |
|  [13]   | `OwnerHistoryWireSchema`                 | message | `rasm.contracts.element.v1.OwnerHistoryWire`            |
|  [14]   | `SchemaSpanWireSchema`                   | message | `rasm.contracts.element.v1.SchemaSpanWire`              |
|  [15]   | `AppearanceWireSchema`                   | message | `rasm.contracts.element.v1.AppearanceWire`              |
|  [16]   | `PropertyValueWireSchema`                | message | `rasm.contracts.element.v1.PropertyValueWire`           |
|  [17]   | `LogicalWireSchema`                      | message | `rasm.contracts.element.v1.LogicalWire`                 |
|  [18]   | `EnumeratedWireSchema`                   | message | `rasm.contracts.element.v1.EnumeratedWire`              |
|  [19]   | `TemporalWireSchema`                     | message | `rasm.contracts.element.v1.TemporalWire`                |
|  [20]   | `ReferenceWireSchema`                    | message | `rasm.contracts.element.v1.ReferenceWire`               |
|  [21]   | `BoundedWireSchema`                      | message | `rasm.contracts.element.v1.BoundedWire`                 |
|  [22]   | `ListWireSchema`                         | message | `rasm.contracts.element.v1.ListWire`                    |
|  [23]   | `TableWireSchema`                        | message | `rasm.contracts.element.v1.TableWire`                   |
|  [24]   | `TableRowWireSchema`                     | message | `rasm.contracts.element.v1.TableRowWire`                |
|  [25]   | `ComplexWireSchema`                      | message | `rasm.contracts.element.v1.ComplexWire`                 |
|  [26]   | `MeasureValueWireSchema`                 | message | `rasm.contracts.element.v1.MeasureValueWire`            |
|  [27]   | `MeasureBandWireSchema`                  | message | `rasm.contracts.element.v1.MeasureBandWire`             |
|  [28]   | `PropertySetWireSchema`                  | message | `rasm.contracts.element.v1.PropertySetWire`             |
|  [29]   | `QuantitySetWireSchema`                  | message | `rasm.contracts.element.v1.QuantitySetWire`             |
|  [30]   | `GroupIdentityWireSchema`                | message | `rasm.contracts.element.v1.GroupIdentityWire`           |
|  [31]   | `RelationshipWireSchema`                 | message | `rasm.contracts.element.v1.RelationshipWire`            |
|  [32]   | `ComposeWireSchema`                      | message | `rasm.contracts.element.v1.ComposeWire`                 |
|  [33]   | `AssignWireSchema`                       | message | `rasm.contracts.element.v1.AssignWire`                  |
|  [34]   | `AssociateWireSchema`                    | message | `rasm.contracts.element.v1.AssociateWire`               |
|  [35]   | `ConnectWireSchema`                      | message | `rasm.contracts.element.v1.ConnectWire`                 |
|  [36]   | `VoidWireSchema`                         | message | `rasm.contracts.element.v1.VoidWire`                    |
|  [37]   | `GenericWireSchema`                      | message | `rasm.contracts.element.v1.GenericWire`                 |
|  [38]   | `RelationshipParticipantWireSchema`      | message | `rasm.contracts.element.v1.RelationshipParticipantWire` |
|  [39]   | `MaterialUsageWireSchema`                | message | `rasm.contracts.element.v1.MaterialUsageWire`           |
|  [40]   | `LayerSetUsageWireSchema`                | message | `rasm.contracts.element.v1.LayerSetUsageWire`           |
|  [41]   | `ProfileSetUsageWireSchema`              | message | `rasm.contracts.element.v1.ProfileSetUsageWire`         |
|  [42]   | `MaterialWireSchema`                     | message | `rasm.contracts.element.v1.MaterialWire`                |
|  [43]   | `MaterialCompositionWireSchema`          | message | `rasm.contracts.element.v1.MaterialCompositionWire`     |
|  [44]   | `SingleWireSchema`                       | message | `rasm.contracts.element.v1.SingleWire`                  |
|  [45]   | `LayerSetWireSchema`                     | message | `rasm.contracts.element.v1.LayerSetWire`                |
|  [46]   | `MaterialLayerWireSchema`                | message | `rasm.contracts.element.v1.MaterialLayerWire`           |
|  [47]   | `ProfileSetWireSchema`                   | message | `rasm.contracts.element.v1.ProfileSetWire`              |
|  [48]   | `MaterialProfileWireSchema`              | message | `rasm.contracts.element.v1.MaterialProfileWire`         |
|  [49]   | `ProfileRefWireSchema`                   | message | `rasm.contracts.element.v1.ProfileRefWire`              |
|  [50]   | `ConstituentSetWireSchema`               | message | `rasm.contracts.element.v1.ConstituentSetWire`          |
|  [51]   | `MaterialConstituentWireSchema`          | message | `rasm.contracts.element.v1.MaterialConstituentWire`     |
|  [52]   | `SectionPropertiesWireSchema`            | message | `rasm.contracts.element.v1.SectionPropertiesWire`       |
|  [53]   | `MaterialPropertySetWireSchema`          | message | `rasm.contracts.element.v1.MaterialPropertySetWire`     |
|  [54]   | `PropertyEvidenceWireSchema`             | message | `rasm.contracts.element.v1.PropertyEvidenceWire`        |
|  [55]   | `AttestationWireSchema`                  | message | `rasm.contracts.element.v1.AttestationWire`             |
|  [56]   | `MechanicalWireSchema`                   | message | `rasm.contracts.element.v1.MechanicalWire`              |
|  [57]   | `OrthotropicWireSchema`                  | message | `rasm.contracts.element.v1.OrthotropicWire`             |
|  [58]   | `ThermalWireSchema`                      | message | `rasm.contracts.element.v1.ThermalWire`                 |
|  [59]   | `AcousticWireSchema`                     | message | `rasm.contracts.element.v1.AcousticWire`                |
|  [60]   | `FireWireSchema`                         | message | `rasm.contracts.element.v1.FireWire`                    |
|  [61]   | `FireResistanceWireSchema`               | message | `rasm.contracts.element.v1.FireResistanceWire`          |
|  [62]   | `EnvironmentalWireSchema`                | message | `rasm.contracts.element.v1.EnvironmentalWire`           |
|  [63]   | `CostWireSchema`                         | message | `rasm.contracts.element.v1.CostWire`                    |
|  [64]   | `DampingWireSchema`                      | message | `rasm.contracts.element.v1.DampingWire`                 |
|  [65]   | `RayleighWireSchema`                     | message | `rasm.contracts.element.v1.RayleighWire`                |
|  [66]   | `HygrothermalWireSchema`                 | message | `rasm.contracts.element.v1.HygrothermalWire`            |
|  [67]   | `SampledCurveWireSchema`                 | message | `rasm.contracts.element.v1.SampledCurveWire`            |
|  [68]   | `DurabilityWireSchema`                   | message | `rasm.contracts.element.v1.DurabilityWire`              |
|  [69]   | `OpticalWireSchema`                      | message | `rasm.contracts.element.v1.OpticalWire`                 |
|  [70]   | `ElectricalWireSchema`                   | message | `rasm.contracts.element.v1.ElectricalWire`              |
|  [71]   | `AssessmentWireSchema`                   | message | `rasm.contracts.element.v1.AssessmentWire`              |
|  [72]   | `DiagnosticWireSchema`                   | message | `rasm.contracts.element.v1.DiagnosticWire`              |
|  [73]   | `ProvenanceWireSchema`                   | message | `rasm.contracts.element.v1.ProvenanceWire`              |
|  [74]   | `ObservationWireSchema`                  | message | `rasm.contracts.element.v1.ObservationWire`             |
|  [75]   | `ObservationChunkWireSchema`             | message | `rasm.contracts.element.v1.ObservationChunkWire`        |
|  [76]   | `SensorProvenanceWireSchema`             | message | `rasm.contracts.element.v1.SensorProvenanceWire`        |
|  [77]   | `SeriesStatisticsWireSchema`             | message | `rasm.contracts.element.v1.SeriesStatisticsWire`        |
|  [78]   | `CoverageWireSchema`                     | message | `rasm.contracts.element.v1.CoverageWire`                |
|  [79]   | `CellLatticeWireSchema`                  | message | `rasm.contracts.element.v1.CellLatticeWire`             |
|  [80]   | `CoverageBandWireSchema`                 | message | `rasm.contracts.element.v1.CoverageBandWire`            |
|  [81]   | `ColorBinWireSchema`                     | message | `rasm.contracts.element.v1.ColorBinWire`                |
|  [82]   | `OverviewLevelWireSchema`                | message | `rasm.contracts.element.v1.OverviewLevelWire`           |
|  [83]   | `GeoReferenceWireSchema`                 | message | `rasm.contracts.element.v1.GeoReferenceWire`            |
|  [84]   | `ProjectedCrsWireSchema`                 | message | `rasm.contracts.element.v1.ProjectedCrsWire`            |

[ENTRYPOINT_SCOPE]: `rasm.contracts.organization.v1` — `@rasm/ts/contracts/rasm/contracts/organization/v1/organization_pb`

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------- | :------ | :------------------------------------------------ |
|  [01]   | `file_rasm_contracts_organization_v1_organization` | file    | `rasm.contracts.organization.v1` file descriptor  |
|  [02]   | `EntityWireSchema`                                 | message | `rasm.contracts.organization.v1.EntityWire`       |
|  [03]   | `ContainmentWireSchema`                            | message | `rasm.contracts.organization.v1.ContainmentWire`  |
|  [04]   | `ViewOverrideWireSchema`                           | message | `rasm.contracts.organization.v1.ViewOverrideWire` |
|  [05]   | `OrganizationWireSchema`                           | message | `rasm.contracts.organization.v1.OrganizationWire` |

[ENTRYPOINT_SCOPE]: `rasm.contracts.scene.v1` — `@rasm/ts/contracts/rasm/contracts/scene/v1/scene_pb`

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `file_rasm_contracts_scene_v1_scene` | file    | `rasm.contracts.scene.v1` file descriptor      |
|  [02]   | `SceneVectorSchema`                  | message | `rasm.contracts.scene.v1.SceneVector`          |
|  [03]   | `SceneSpectrumSchema`                | message | `rasm.contracts.scene.v1.SceneSpectrum`        |
|  [04]   | `SolarFrameSchema`                   | message | `rasm.contracts.scene.v1.SolarFrame`           |
|  [05]   | `SolarAnglesSchema`                  | message | `rasm.contracts.scene.v1.SolarAngles`          |
|  [06]   | `SitedSunSchema`                     | message | `rasm.contracts.scene.v1.SitedSun`             |
|  [07]   | `AuthoredSunSchema`                  | message | `rasm.contracts.scene.v1.AuthoredSun`          |
|  [08]   | `SceneSunSchema`                     | message | `rasm.contracts.scene.v1.SceneSun`             |
|  [09]   | `PhotometricWebRefSchema`            | message | `rasm.contracts.scene.v1.PhotometricWebRef`    |
|  [10]   | `PhotometricPowerSchema`             | message | `rasm.contracts.scene.v1.PhotometricPower`     |
|  [11]   | `SpotConeSchema`                     | message | `rasm.contracts.scene.v1.SpotCone`             |
|  [12]   | `AreaExtentSchema`                   | message | `rasm.contracts.scene.v1.AreaExtent`           |
|  [13]   | `ScenePhotometrySchema`              | message | `rasm.contracts.scene.v1.ScenePhotometry`      |
|  [14]   | `TessellationFidelitySchema`         | message | `rasm.contracts.scene.v1.TessellationFidelity` |
|  [15]   | `ShadingArtifactSchema`              | message | `rasm.contracts.scene.v1.ShadingArtifact`      |
|  [16]   | `SceneDescriptorSchema`              | message | `rasm.contracts.scene.v1.SceneDescriptor`      |

[ENTRYPOINT_SCOPE]: `io.cloudevents.v1` — `@rasm/ts/contracts/io/cloudevents/v1/cloudevents_pb`

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :------------------------------------------ | :------ | :------------------------------------------------------ |
|  [01]   | `file_io_cloudevents_v1_cloudevents`        | file    | `io.cloudevents.v1` file descriptor                     |
|  [02]   | `CloudEventSchema`                          | message | `io.cloudevents.v1.CloudEvent`                          |
|  [03]   | `CloudEvent_CloudEventAttributeValueSchema` | nested  | `io.cloudevents.v1.CloudEvent.CloudEventAttributeValue` |
|  [04]   | `CloudEventBatchSchema`                     | message | `io.cloudevents.v1.CloudEventBatch`                     |

[ENTRYPOINT_SCOPE]: `grpc.health.v1` — `@rasm/ts/contracts/grpc/health/v1/health_pb`

| [INDEX] | [SURFACE]                                 | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `file_grpc_health_v1_health`              | file    | `grpc.health.v1` file descriptor                               |
|  [02]   | `HealthCheckRequestSchema`                | message | `grpc.health.v1.HealthCheckRequest`                            |
|  [03]   | `HealthCheckResponseSchema`               | message | `grpc.health.v1.HealthCheckResponse`                           |
|  [04]   | `HealthCheckResponse_ServingStatusSchema` | enum    | `grpc.health.v1.HealthCheckResponse.ServingStatus`             |
|  [05]   | `Health`                                  | service | `grpc.health.v1.Health` — `check` unary, `watch` server stream |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Bindings record their own field types, optionality, and cardinality as generated — a consumer reads the emitted shape, never the `.proto` source's.
- `Schema.decode` crosses a decoded message into branded vocabulary at the consuming page; no binding is a domain model.
- Every module is reached by its proto path alone, so a moved source moves its specifier and `buf breaking` FILE refuses the move.

[STACKING]:
- `@bufbuild/protobuf`(`core/.api/bufbuild-protobuf.md`): `<Name>Schema` is the first argument of `create`, `fromBinary`, `toBinary`, and `equals`.
- `@connectrpc/connect`(`core/.api/connectrpc-connect.md`): `createClient(<Service>, transport)` derives every member from the `GenService` roster.
- `core/interchange/format`: `_suite` binds `<Name>Schema` per declared message name from its module; `Proto.registry` admits estate families alone.
- `core/interchange/format`: `Format.event`'s `Core` seat takes the vendored `CloudEventSchema` as an argument the registry never admits.

[LOCAL_ADMISSION]:
- Import by module path from `@rasm/ts/contracts/<proto path>_pb`; a barrel, a per-family `exports` row, or a hand-declared twin is the deleted form.

[RAIL_LAW]:
- Package: `@rasm/ts/contracts`
- Owns: the generated `GenFile`, `GenMessage`, and `GenService` descriptors of every corpus source and their decoded value types
- Accept: `@bufbuild/protobuf` codecs over `<Name>Schema`, `createClient` over `<Service>`, `Schema.decode` at the page boundary
- Reject: a hand-authored proto shape, a decoded message reused as a domain model, a descriptor minted outside the emission, a hand edit
