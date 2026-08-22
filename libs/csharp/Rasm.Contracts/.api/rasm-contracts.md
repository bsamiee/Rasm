# [RASM_CONTRACTS_API_RASM_CONTRACTS]

`Rasm.Contracts` is the committed protoc and `grpc_csharp_plugin` emission of the `rasm.contracts.<family>.v1` corpus — every message, enum, file descriptor, service base, and client a C# wire consumer composes by project reference; it mints no authored member. Twelve families ride one assembly: `appearance`, `compute` (`compute.proto` + `control.proto`), `declaration`, `element` (`value`, `substance`, `evidence`, `graph`), `event`, `fabrication`, `fault`, `host` (`apphost`, `appui`, `bim`, `evidence`), `organization`, `parity`, `scene`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rasm.Contracts`
- package: `Rasm.Contracts`
- assembly: `Rasm.Contracts`
- namespace: `Rasm.Contracts.<Family>.V1` per family — `Appearance`, `Compute`, `Declaration`, `Element`, `Event`, `Fabrication`, `Fault`, `Host`, `Organization`, `Parity`, `Scene`
- depends: `protoc` `csharp` builtin + `grpc_csharp_plugin` on the machine PATH under root `buf.gen.yaml`; runtime rows `Google.Protobuf`, `Grpc.Core.Api`, `Google.Api.CommonProtos` (`google.rpc`, `google.type` imports), `ProtoValidate` (`buf.validate` descriptor)
- plane: out root `libs/csharp/Rasm.Contracts/Generated` — `<Family>/V1/<File>.cs` per source file, `<File>Grpc.cs` beside a service-bearing one
- rail: generated bindings — `assay contracts generate` writes, `assay contracts check` proves freshness and the roster below, a `ProjectReference` consumes

## [02]-[SYMBOL_GRAMMAR]

[SYMBOL_GRAMMAR_SCOPE]: how one proto declaration spells in this emission — the grammar the emitted roster instantiates

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Rasm.Contracts.<F>.V1.<Msg>`        | class         | `sealed partial` over `IMessage<T>`, `IBufferMessage`, `IDeepCloneable<T>`       |
|  [02]   | `<Msg>.Parser`/`.Descriptor`         | class         | static `MessageParser<T>` and `MessageDescriptor` per message                    |
|  [03]   | `<File>Reflection.Descriptor`        | class         | static `FileDescriptor` holder per SOURCE FILE (`GraphReflection`, `ControlReflection`) |
|  [04]   | `Rasm.Contracts.<F>.V1.<Enum>`       | enum          | one member per value, `<ENUM>_<VALUE>` lowered to `Value`; `_UNSPECIFIED` is `Unspecified` |
|  [05]   | `<Msg>.<Oneof>OneofCase`             | enum          | `None` + one member per arm; `<Msg>.<Oneof>Case` reads the set arm; any arm setter clears the others |
|  [06]   | `<Msg>.Has<Field>`/`.Clear<Field>()` | class         | presence pair on every `optional` scalar and enum; a message column is `null` unset |
|  [07]   | `<Msg>.Types.<Nested>`               | class         | nested message and enum holder                                                   |
|  [08]   | `<Svc>`                              | class         | `static partial` container: `Descriptor` and the two `BindService` overloads     |
|  [09]   | `<Svc>.<Svc>Base`                    | class         | `abstract partial` server base: `Task<TReply> Verb(TRequest, ServerCallContext)` |
|  [10]   | `<Svc>.<Svc>Client`                  | class         | `partial : ClientBase<T>`: per verb a blocking pair and a `VerbAsync` pair; a bidi verb one `AsyncDuplexStreamingCall` mint |

- `<Msg>` is a message, `<F>` a family, `<File>` a source file, `<Svc>` a service; `<Msg>.Types` is absent where nothing nests, `<File>Grpc.cs` where no service exists; two files named `evidence.proto` emit `Element.V1.EvidenceReflection` and `Host.V1.EvidenceReflection`, so a registry names both qualified.

## [03]-[ROSTER]

<!-- roster:begin -->
[ROSTER_SCOPE]: `rasm.contracts.appearance.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [FQN]                                                  |
| :-----: | :------------------------------ | :------ | :----------------------------------------------------- |
|  [01]   | `PlaneRef`                      | message | `rasm.contracts.appearance.v1.PlaneRef`                |
|  [02]   | `Plane`                         | message | `rasm.contracts.appearance.v1.Plane`                   |
|  [03]   | `PackRow`                       | message | `rasm.contracts.appearance.v1.PackRow`                 |
|  [04]   | `Ibl`                           | message | `rasm.contracts.appearance.v1.Ibl`                     |
|  [05]   | `Provenance`                    | message | `rasm.contracts.appearance.v1.Provenance`              |
|  [06]   | `Provenance.Types.Capture`      | message | `rasm.contracts.appearance.v1.Provenance.Capture`      |
|  [07]   | `Provenance.Types.Fit`          | message | `rasm.contracts.appearance.v1.Provenance.Fit`          |
|  [08]   | `Provenance.Types.Chromaticity` | message | `rasm.contracts.appearance.v1.Provenance.Chromaticity` |
|  [09]   | `Provenance.Types.Card`         | message | `rasm.contracts.appearance.v1.Provenance.Card`         |
|  [10]   | `Provenance.Types.Ingest`       | message | `rasm.contracts.appearance.v1.Provenance.Ingest`       |
|  [11]   | `Press`                         | message | `rasm.contracts.appearance.v1.Press`                   |
|  [12]   | `Set`                           | message | `rasm.contracts.appearance.v1.Set`                     |
|  [13]   | `Color`                         | message | `rasm.contracts.appearance.v1.Color`                   |
|  [14]   | `OpenPbr`                       | message | `rasm.contracts.appearance.v1.OpenPbr`                 |
|  [15]   | `EmissionReadout`               | message | `rasm.contracts.appearance.v1.EmissionReadout`         |
|  [16]   | `Emission`                      | message | `rasm.contracts.appearance.v1.Emission`                |
|  [17]   | `Material`                      | message | `rasm.contracts.appearance.v1.Material`                |
|  [18]   | `Kind`                          | enum    | `rasm.contracts.appearance.v1.Kind`                    |
|  [19]   | `Role`                          | enum    | `rasm.contracts.appearance.v1.Role`                    |
|  [20]   | `Transfer`                      | enum    | `rasm.contracts.appearance.v1.Transfer`                |
|  [21]   | `NormalConvention`              | enum    | `rasm.contracts.appearance.v1.NormalConvention`        |
|  [22]   | `AlphaMode`                     | enum    | `rasm.contracts.appearance.v1.AlphaMode`               |
|  [23]   | `Container`                     | enum    | `rasm.contracts.appearance.v1.Container`               |
|  [24]   | `Pack`                          | enum    | `rasm.contracts.appearance.v1.Pack`                    |
|  [25]   | `PlaneFormat`                   | enum    | `rasm.contracts.appearance.v1.PlaneFormat`             |
|  [26]   | `MipPolicy`                     | enum    | `rasm.contracts.appearance.v1.MipPolicy`               |
|  [27]   | `KtxPayload`                    | enum    | `rasm.contracts.appearance.v1.KtxPayload`              |
|  [28]   | `BlockFormat`                   | enum    | `rasm.contracts.appearance.v1.BlockFormat`             |
|  [29]   | `LayerLaw`                      | enum    | `rasm.contracts.appearance.v1.LayerLaw`                |
|  [30]   | `LicenseClass`                  | enum    | `rasm.contracts.appearance.v1.LicenseClass`            |
|  [31]   | `Udim`                          | enum    | `rasm.contracts.appearance.v1.Udim`                    |
|  [32]   | `Primaries`                     | enum    | `rasm.contracts.appearance.v1.Primaries`               |
|  [33]   | `Depth`                         | enum    | `rasm.contracts.appearance.v1.Depth`                   |
|  [34]   | `Tool`                          | enum    | `rasm.contracts.appearance.v1.Tool`                    |

[ROSTER_SCOPE]: `rasm.contracts.fault.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [FQN]                                      |
| :-----: | :----------------- | :------ | :----------------------------------------- |
|  [01]   | `Hlc`              | message | `rasm.contracts.fault.v1.Hlc`              |
|  [02]   | `FaultRecovery`    | message | `rasm.contracts.fault.v1.FaultRecovery`    |
|  [03]   | `FaultObservation` | message | `rasm.contracts.fault.v1.FaultObservation` |
|  [04]   | `FaultDetail`      | message | `rasm.contracts.fault.v1.FaultDetail`      |

[ROSTER_SCOPE]: `rasm.contracts.compute.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [FQN]                                           |
| :-----: | :-------------------- | :------ | :---------------------------------------------- |
|  [01]   | `PointCloud`          | message | `rasm.contracts.compute.v1.PointCloud`          |
|  [02]   | `Mesh`                | message | `rasm.contracts.compute.v1.Mesh`                |
|  [03]   | `GeometryPayload`     | message | `rasm.contracts.compute.v1.GeometryPayload`     |
|  [04]   | `GaussianSplatScan`   | message | `rasm.contracts.compute.v1.GaussianSplatScan`   |
|  [05]   | `TransactionRequest`  | message | `rasm.contracts.compute.v1.TransactionRequest`  |
|  [06]   | `TransactionReceipt`  | message | `rasm.contracts.compute.v1.TransactionReceipt`  |
|  [07]   | `TessellationRequest` | message | `rasm.contracts.compute.v1.TessellationRequest` |
|  [08]   | `Semantic`            | message | `rasm.contracts.compute.v1.Semantic`            |
|  [09]   | `TessellationReceipt` | message | `rasm.contracts.compute.v1.TessellationReceipt` |
|  [10]   | `ArtifactFrame`       | message | `rasm.contracts.compute.v1.ArtifactFrame`       |
|  [11]   | `Dtype`               | enum    | `rasm.contracts.compute.v1.Dtype`               |
|  [12]   | `SplatFormat`         | enum    | `rasm.contracts.compute.v1.SplatFormat`         |
|  [13]   | `Spill`               | enum    | `rasm.contracts.compute.v1.Spill`               |
|  [14]   | `Modality`            | enum    | `rasm.contracts.compute.v1.Modality`            |
|  [15]   | `ComputeService`      | service | `rasm.contracts.compute.v1.ComputeService`      |
|  [16]   | `DocumentService`     | service | `rasm.contracts.compute.v1.DocumentService`     |
|  [17]   | `ArtifactSyncService` | service | `rasm.contracts.compute.v1.ArtifactSyncService` |

[ROSTER_SCOPE]: `rasm.contracts.compute.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                           | [KIND]  | [FQN]                                                |
| :-----: | :------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `PatchAdd`                       | message | `rasm.contracts.compute.v1.PatchAdd`                 |
|  [02]   | `PatchRemove`                    | message | `rasm.contracts.compute.v1.PatchRemove`              |
|  [03]   | `PatchReplace`                   | message | `rasm.contracts.compute.v1.PatchReplace`             |
|  [04]   | `PatchMove`                      | message | `rasm.contracts.compute.v1.PatchMove`                |
|  [05]   | `PatchCopy`                      | message | `rasm.contracts.compute.v1.PatchCopy`                |
|  [06]   | `PatchTest`                      | message | `rasm.contracts.compute.v1.PatchTest`                |
|  [07]   | `PatchOp`                        | message | `rasm.contracts.compute.v1.PatchOp`                  |
|  [08]   | `ReloadOptionsRequest`           | message | `rasm.contracts.compute.v1.ReloadOptionsRequest`     |
|  [09]   | `DispatchPatchRequest`           | message | `rasm.contracts.compute.v1.DispatchPatchRequest`     |
|  [10]   | `ReloadReply`                    | message | `rasm.contracts.compute.v1.ReloadReply`              |
|  [11]   | `DispatchToolRequest`            | message | `rasm.contracts.compute.v1.DispatchToolRequest`      |
|  [12]   | `DispatchReceipt`                | message | `rasm.contracts.compute.v1.DispatchReceipt`          |
|  [13]   | `Meter`                          | message | `rasm.contracts.compute.v1.Meter`                    |
|  [14]   | `CommandReply`                   | message | `rasm.contracts.compute.v1.CommandReply`             |
|  [15]   | `CommandReply.Types.Compensated` | message | `rasm.contracts.compute.v1.CommandReply.Compensated` |
|  [16]   | `SetDegradationRequest`          | message | `rasm.contracts.compute.v1.SetDegradationRequest`    |
|  [17]   | `DegradationReply`               | message | `rasm.contracts.compute.v1.DegradationReply`         |
|  [18]   | `DrainRuntimeRequest`            | message | `rasm.contracts.compute.v1.DrainRuntimeRequest`      |
|  [19]   | `DrainStep`                      | message | `rasm.contracts.compute.v1.DrainStep`                |
|  [20]   | `DrainReply`                     | message | `rasm.contracts.compute.v1.DrainReply`               |
|  [21]   | `SupportBundleRequest`           | message | `rasm.contracts.compute.v1.SupportBundleRequest`     |
|  [22]   | `SupportBundleReply`             | message | `rasm.contracts.compute.v1.SupportBundleReply`       |
|  [23]   | `DegradationLevel`               | enum    | `rasm.contracts.compute.v1.DegradationLevel`         |
|  [24]   | `ReloadClass`                    | enum    | `rasm.contracts.compute.v1.ReloadClass`              |
|  [25]   | `CostUnit`                       | enum    | `rasm.contracts.compute.v1.CostUnit`                 |
|  [26]   | `DrainBand`                      | enum    | `rasm.contracts.compute.v1.DrainBand`                |
|  [27]   | `DeadlineOutcome`                | enum    | `rasm.contracts.compute.v1.DeadlineOutcome`          |
|  [28]   | `RuntimePhase`                   | enum    | `rasm.contracts.compute.v1.RuntimePhase`             |
|  [29]   | `ControlService`                 | service | `rasm.contracts.compute.v1.ControlService`           |
|  [30]   | `DiagnosticService`              | service | `rasm.contracts.compute.v1.DiagnosticService`        |

[ROSTER_SCOPE]: `rasm.contracts.declaration.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [FQN]                                             |
| :-----: | :------------------ | :------ | :------------------------------------------------ |
|  [01]   | `ImpactCell`        | message | `rasm.contracts.declaration.v1.ImpactCell`        |
|  [02]   | `Source`            | message | `rasm.contracts.declaration.v1.Source`            |
|  [03]   | `DeclarationRecord` | message | `rasm.contracts.declaration.v1.DeclarationRecord` |
|  [04]   | `Registry`          | enum    | `rasm.contracts.declaration.v1.Registry`          |
|  [05]   | `DeclaredUnit`      | enum    | `rasm.contracts.declaration.v1.DeclaredUnit`      |
|  [06]   | `Standard`          | enum    | `rasm.contracts.declaration.v1.Standard`          |
|  [07]   | `Subtype`           | enum    | `rasm.contracts.declaration.v1.Subtype`           |
|  [08]   | `ImpactCategory`    | enum    | `rasm.contracts.declaration.v1.ImpactCategory`    |
|  [09]   | `Module`            | enum    | `rasm.contracts.declaration.v1.Module`            |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [FQN]                                         |
| :-----: | :------------------ | :------ | :-------------------------------------------- |
|  [01]   | `VectorWire`        | message | `rasm.contracts.element.v1.VectorWire`        |
|  [02]   | `DimensionWire`     | message | `rasm.contracts.element.v1.DimensionWire`     |
|  [03]   | `MeasureBandWire`   | message | `rasm.contracts.element.v1.MeasureBandWire`   |
|  [04]   | `MeasureValueWire`  | message | `rasm.contracts.element.v1.MeasureValueWire`  |
|  [05]   | `NamedMeasureWire`  | message | `rasm.contracts.element.v1.NamedMeasureWire`  |
|  [06]   | `CurvePointWire`    | message | `rasm.contracts.element.v1.CurvePointWire`    |
|  [07]   | `SampledCurveWire`  | message | `rasm.contracts.element.v1.SampledCurveWire`  |
|  [08]   | `PropertyValueWire` | message | `rasm.contracts.element.v1.PropertyValueWire` |
|  [09]   | `LogicalWire`       | message | `rasm.contracts.element.v1.LogicalWire`       |
|  [10]   | `EnumeratedWire`    | message | `rasm.contracts.element.v1.EnumeratedWire`    |
|  [11]   | `TemporalWire`      | message | `rasm.contracts.element.v1.TemporalWire`      |
|  [12]   | `ReferenceWire`     | message | `rasm.contracts.element.v1.ReferenceWire`     |
|  [13]   | `BoundedWire`       | message | `rasm.contracts.element.v1.BoundedWire`       |
|  [14]   | `ListWire`          | message | `rasm.contracts.element.v1.ListWire`          |
|  [15]   | `TableRowWire`      | message | `rasm.contracts.element.v1.TableRowWire`      |
|  [16]   | `TableWire`         | message | `rasm.contracts.element.v1.TableWire`         |
|  [17]   | `NamedValueWire`    | message | `rasm.contracts.element.v1.NamedValueWire`    |
|  [18]   | `ComplexWire`       | message | `rasm.contracts.element.v1.ComplexWire`       |
|  [19]   | `UncertaintyKind`   | enum    | `rasm.contracts.element.v1.UncertaintyKind`   |
|  [20]   | `Interpolation`     | enum    | `rasm.contracts.element.v1.Interpolation`     |
|  [21]   | `InheritanceMode`   | enum    | `rasm.contracts.element.v1.InheritanceMode`   |
|  [22]   | `EvidenceGrade`     | enum    | `rasm.contracts.element.v1.EvidenceGrade`     |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND]  | [FQN]                                            |
| :-----: | :--------------------- | :------ | :----------------------------------------------- |
|  [01]   | `DiagnosticWire`       | message | `rasm.contracts.element.v1.DiagnosticWire`       |
|  [02]   | `ProvenanceWire`       | message | `rasm.contracts.element.v1.ProvenanceWire`       |
|  [03]   | `AssessmentWire`       | message | `rasm.contracts.element.v1.AssessmentWire`       |
|  [04]   | `ObservationChunkWire` | message | `rasm.contracts.element.v1.ObservationChunkWire` |
|  [05]   | `SensorProvenanceWire` | message | `rasm.contracts.element.v1.SensorProvenanceWire` |
|  [06]   | `GradeCountWire`       | message | `rasm.contracts.element.v1.GradeCountWire`       |
|  [07]   | `MomentsWire`          | message | `rasm.contracts.element.v1.MomentsWire`          |
|  [08]   | `SeriesStatisticsWire` | message | `rasm.contracts.element.v1.SeriesStatisticsWire` |
|  [09]   | `ObservationWire`      | message | `rasm.contracts.element.v1.ObservationWire`      |
|  [10]   | `ProjectedCrsWire`     | message | `rasm.contracts.element.v1.ProjectedCrsWire`     |
|  [11]   | `GeoReferenceWire`     | message | `rasm.contracts.element.v1.GeoReferenceWire`     |
|  [12]   | `CellLatticeWire`      | message | `rasm.contracts.element.v1.CellLatticeWire`      |
|  [13]   | `ColorBinWire`         | message | `rasm.contracts.element.v1.ColorBinWire`         |
|  [14]   | `CoverageBandWire`     | message | `rasm.contracts.element.v1.CoverageBandWire`     |
|  [15]   | `OverviewLevelWire`    | message | `rasm.contracts.element.v1.OverviewLevelWire`    |
|  [16]   | `CoverageWire`         | message | `rasm.contracts.element.v1.CoverageWire`         |
|  [17]   | `Discipline`           | enum    | `rasm.contracts.element.v1.Discipline`           |
|  [18]   | `AssessmentOutcome`    | enum    | `rasm.contracts.element.v1.AssessmentOutcome`    |
|  [19]   | `SolvePhase`           | enum    | `rasm.contracts.element.v1.SolvePhase`           |
|  [20]   | `FailureKind`          | enum    | `rasm.contracts.element.v1.FailureKind`          |
|  [21]   | `SamplingKind`         | enum    | `rasm.contracts.element.v1.SamplingKind`         |
|  [22]   | `ObservationGrade`     | enum    | `rasm.contracts.element.v1.ObservationGrade`     |
|  [23]   | `CoverageKind`         | enum    | `rasm.contracts.element.v1.CoverageKind`         |
|  [24]   | `ChannelDtype`         | enum    | `rasm.contracts.element.v1.ChannelDtype`         |
|  [25]   | `BandRole`             | enum    | `rasm.contracts.element.v1.BandRole`             |
|  [26]   | `CrsResolution`        | enum    | `rasm.contracts.element.v1.CrsResolution`        |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [FQN]                                               |
| :-----: | :------------------------ | :------ | :-------------------------------------------------- |
|  [01]   | `LayerSetUsageWire`       | message | `rasm.contracts.element.v1.LayerSetUsageWire`       |
|  [02]   | `ProfileSetUsageWire`     | message | `rasm.contracts.element.v1.ProfileSetUsageWire`     |
|  [03]   | `MaterialUsageWire`       | message | `rasm.contracts.element.v1.MaterialUsageWire`       |
|  [04]   | `MaterialLayerWire`       | message | `rasm.contracts.element.v1.MaterialLayerWire`       |
|  [05]   | `LayerSetWire`            | message | `rasm.contracts.element.v1.LayerSetWire`            |
|  [06]   | `ProfileRefWire`          | message | `rasm.contracts.element.v1.ProfileRefWire`          |
|  [07]   | `MaterialProfileWire`     | message | `rasm.contracts.element.v1.MaterialProfileWire`     |
|  [08]   | `SectionPropertiesWire`   | message | `rasm.contracts.element.v1.SectionPropertiesWire`   |
|  [09]   | `ProfileSetWire`          | message | `rasm.contracts.element.v1.ProfileSetWire`          |
|  [10]   | `MaterialConstituentWire` | message | `rasm.contracts.element.v1.MaterialConstituentWire` |
|  [11]   | `ConstituentSetWire`      | message | `rasm.contracts.element.v1.ConstituentSetWire`      |
|  [12]   | `MaterialCompositionWire` | message | `rasm.contracts.element.v1.MaterialCompositionWire` |
|  [13]   | `AttestationWire`         | message | `rasm.contracts.element.v1.AttestationWire`         |
|  [14]   | `PropertyEvidenceWire`    | message | `rasm.contracts.element.v1.PropertyEvidenceWire`    |
|  [15]   | `MechanicalWire`          | message | `rasm.contracts.element.v1.MechanicalWire`          |
|  [16]   | `OrthotropicWire`         | message | `rasm.contracts.element.v1.OrthotropicWire`         |
|  [17]   | `ThermalWire`             | message | `rasm.contracts.element.v1.ThermalWire`             |
|  [18]   | `BandValueWire`           | message | `rasm.contracts.element.v1.BandValueWire`           |
|  [19]   | `AcousticWire`            | message | `rasm.contracts.element.v1.AcousticWire`            |
|  [20]   | `FireResistanceWire`      | message | `rasm.contracts.element.v1.FireResistanceWire`      |
|  [21]   | `FireWire`                | message | `rasm.contracts.element.v1.FireWire`                |
|  [22]   | `BandCellWire`            | message | `rasm.contracts.element.v1.BandCellWire`            |
|  [23]   | `EnvironmentalWire`       | message | `rasm.contracts.element.v1.EnvironmentalWire`       |
|  [24]   | `CostWire`                | message | `rasm.contracts.element.v1.CostWire`                |
|  [25]   | `RayleighWire`            | message | `rasm.contracts.element.v1.RayleighWire`            |
|  [26]   | `DampingWire`             | message | `rasm.contracts.element.v1.DampingWire`             |
|  [27]   | `HygrothermalWire`        | message | `rasm.contracts.element.v1.HygrothermalWire`        |
|  [28]   | `DurabilityWire`          | message | `rasm.contracts.element.v1.DurabilityWire`          |
|  [29]   | `OpticalWire`             | message | `rasm.contracts.element.v1.OpticalWire`             |
|  [30]   | `ElectricalWire`          | message | `rasm.contracts.element.v1.ElectricalWire`          |
|  [31]   | `MaterialPropertySetWire` | message | `rasm.contracts.element.v1.MaterialPropertySetWire` |
|  [32]   | `MaterialWire`            | message | `rasm.contracts.element.v1.MaterialWire`            |
|  [33]   | `LayerSetDirection`       | enum    | `rasm.contracts.element.v1.LayerSetDirection`       |
|  [34]   | `DirectionSense`          | enum    | `rasm.contracts.element.v1.DirectionSense`          |
|  [35]   | `AttestationRole`         | enum    | `rasm.contracts.element.v1.AttestationRole`         |
|  [36]   | `FireRating`              | enum    | `rasm.contracts.element.v1.FireRating`              |
|  [37]   | `SmokeClass`              | enum    | `rasm.contracts.element.v1.SmokeClass`              |
|  [38]   | `DropletClass`            | enum    | `rasm.contracts.element.v1.DropletClass`            |
|  [39]   | `MeasurementBasis`        | enum    | `rasm.contracts.element.v1.MeasurementBasis`        |
|  [40]   | `LifecycleBand`           | enum    | `rasm.contracts.element.v1.LifecycleBand`           |
|  [41]   | `AcousticBand`            | enum    | `rasm.contracts.element.v1.AcousticBand`            |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [FQN]                                               |
| :-----: | :------------------------------ | :------ | :-------------------------------------------------- |
|  [01]   | `StepHeaderWire`                | message | `rasm.contracts.element.v1.StepHeaderWire`          |
|  [02]   | `UnitOverrideWire`              | message | `rasm.contracts.element.v1.UnitOverrideWire`        |
|  [03]   | `UnitAxisWire`                  | message | `rasm.contracts.element.v1.UnitAxisWire`            |
|  [04]   | `HeaderWire`                    | message | `rasm.contracts.element.v1.HeaderWire`              |
|  [05]   | `ClassificationWire`            | message | `rasm.contracts.element.v1.ClassificationWire`      |
|  [06]   | `OwnerHistoryWire`              | message | `rasm.contracts.element.v1.OwnerHistoryWire`        |
|  [07]   | `SchemaSpanWire`                | message | `rasm.contracts.element.v1.SchemaSpanWire`          |
|  [08]   | `PlacementWire`                 | message | `rasm.contracts.element.v1.PlacementWire`           |
|  [09]   | `RepresentationWire`            | message | `rasm.contracts.element.v1.RepresentationWire`      |
|  [10]   | `ObjectWire`                    | message | `rasm.contracts.element.v1.ObjectWire`              |
|  [11]   | `PropertySetWire`               | message | `rasm.contracts.element.v1.PropertySetWire`         |
|  [12]   | `GroupIdentityWire`             | message | `rasm.contracts.element.v1.GroupIdentityWire`       |
|  [13]   | `GroupWire`                     | message | `rasm.contracts.element.v1.GroupWire`               |
|  [14]   | `QuantitySetWire`               | message | `rasm.contracts.element.v1.QuantitySetWire`         |
|  [15]   | `AppearanceWire`                | message | `rasm.contracts.element.v1.AppearanceWire`          |
|  [16]   | `NodeWire`                      | message | `rasm.contracts.element.v1.NodeWire`                |
|  [17]   | `ComposeWire`                   | message | `rasm.contracts.element.v1.ComposeWire`             |
|  [18]   | `ConnectWire`                   | message | `rasm.contracts.element.v1.ConnectWire`             |
|  [19]   | `ParticipantWire`               | message | `rasm.contracts.element.v1.ParticipantWire`         |
|  [20]   | `GenericWire`                   | message | `rasm.contracts.element.v1.GenericWire`             |
|  [21]   | `RelationshipWire`              | message | `rasm.contracts.element.v1.RelationshipWire`        |
|  [22]   | `RedactionManifestWire`         | message | `rasm.contracts.element.v1.RedactionManifestWire`   |
|  [23]   | `ElementGraphWire`              | message | `rasm.contracts.element.v1.ElementGraphWire`        |
|  [24]   | `GraphDeltaWire`                | message | `rasm.contracts.element.v1.GraphDeltaWire`          |
|  [25]   | `GraphDeltaWire.Types.Revision` | message | `rasm.contracts.element.v1.GraphDeltaWire.Revision` |
|  [26]   | `ObjectKind`                    | enum    | `rasm.contracts.element.v1.ObjectKind`              |
|  [27]   | `ReleaseVersion`                | enum    | `rasm.contracts.element.v1.ReleaseVersion`          |
|  [28]   | `ModelView`                     | enum    | `rasm.contracts.element.v1.ModelView`               |
|  [29]   | `ChangeAction`                  | enum    | `rasm.contracts.element.v1.ChangeAction`            |
|  [30]   | `ObjectState`                   | enum    | `rasm.contracts.element.v1.ObjectState`             |
|  [31]   | `RepresentationKind`            | enum    | `rasm.contracts.element.v1.RepresentationKind`      |
|  [32]   | `DimensionAxis`                 | enum    | `rasm.contracts.element.v1.DimensionAxis`           |
|  [33]   | `RelKind`                       | enum    | `rasm.contracts.element.v1.RelKind`                 |
|  [34]   | `IfcRel`                        | enum    | `rasm.contracts.element.v1.IfcRel`                  |

[ROSTER_SCOPE]: `rasm.contracts.event.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]       | [KIND]  | [FQN]                                |
| :-----: | :----------- | :------ | :----------------------------------- |
|  [01]   | `Extensions` | message | `rasm.contracts.event.v1.Extensions` |
|  [02]   | `DataGrade`  | enum    | `rasm.contracts.event.v1.DataGrade`  |

[ROSTER_SCOPE]: `rasm.contracts.fabrication.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]           | [KIND]  | [FQN]                                          |
| :-----: | :--------------- | :------ | :--------------------------------------------- |
|  [01]   | `SourceKey`      | message | `rasm.contracts.fabrication.v1.SourceKey`      |
|  [02]   | `Datum`          | message | `rasm.contracts.fabrication.v1.Datum`          |
|  [03]   | `Segment`        | message | `rasm.contracts.fabrication.v1.Segment`        |
|  [04]   | `FeatureControl` | message | `rasm.contracts.fabrication.v1.FeatureControl` |
|  [05]   | `Characteristic` | enum    | `rasm.contracts.fabrication.v1.Characteristic` |
|  [06]   | `Scope`          | enum    | `rasm.contracts.fabrication.v1.Scope`          |
|  [07]   | `ZoneKind`       | enum    | `rasm.contracts.fabrication.v1.ZoneKind`       |
|  [08]   | `Modifier`       | enum    | `rasm.contracts.fabrication.v1.Modifier`       |
|  [09]   | `Material`       | enum    | `rasm.contracts.fabrication.v1.Material`       |
|  [10]   | `Egress`         | enum    | `rasm.contracts.fabrication.v1.Egress`         |

[ROSTER_SCOPE]: `rasm.contracts.host.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                                  | [KIND]  | [FQN]                                                    |
| :-----: | :-------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `Vec3Wire`                              | message | `rasm.contracts.host.v1.Vec3Wire`                        |
|  [02]   | `TenantContextWire`                     | message | `rasm.contracts.host.v1.TenantContextWire`               |
|  [03]   | `ReceiptEnvelopeWire`                   | message | `rasm.contracts.host.v1.ReceiptEnvelopeWire`             |
|  [04]   | `CommandVerdictWire`                    | message | `rasm.contracts.host.v1.CommandVerdictWire`              |
|  [05]   | `CommandVerdictWire.Types.Gated`        | message | `rasm.contracts.host.v1.CommandVerdictWire.Gated`        |
|  [06]   | `CommandVerdictWire.Types.Withheld`     | message | `rasm.contracts.host.v1.CommandVerdictWire.Withheld`     |
|  [07]   | `CommandAvailability`                   | message | `rasm.contracts.host.v1.CommandAvailability`             |
|  [08]   | `CredentialPemWire`                     | message | `rasm.contracts.host.v1.CredentialPemWire`               |
|  [09]   | `FlagVerdictWire`                       | message | `rasm.contracts.host.v1.FlagVerdictWire`                 |
|  [10]   | `BootMarkerWire`                        | message | `rasm.contracts.host.v1.BootMarkerWire`                  |
|  [11]   | `FaultRecordWire`                       | message | `rasm.contracts.host.v1.FaultRecordWire`                 |
|  [12]   | `FaultRecordWire.Types.Unhandled`       | message | `rasm.contracts.host.v1.FaultRecordWire.Unhandled`       |
|  [13]   | `FaultRecordWire.Types.UnobservedTask`  | message | `rasm.contracts.host.v1.FaultRecordWire.UnobservedTask`  |
|  [14]   | `FaultRecordWire.Types.Signalled`       | message | `rasm.contracts.host.v1.FaultRecordWire.Signalled`       |
|  [15]   | `FaultRecordWire.Types.HostCrashMarker` | message | `rasm.contracts.host.v1.FaultRecordWire.HostCrashMarker` |
|  [16]   | `FaultRecordWire.Types.MarkerDrifted`   | message | `rasm.contracts.host.v1.FaultRecordWire.MarkerDrifted`   |
|  [17]   | `SupportEntry`                          | message | `rasm.contracts.host.v1.SupportEntry`                    |
|  [18]   | `SupportCaptureWire`                    | message | `rasm.contracts.host.v1.SupportCaptureWire`              |
|  [19]   | `HopReceiptWire`                        | message | `rasm.contracts.host.v1.HopReceiptWire`                  |
|  [20]   | `DeliveryReceiptWire`                   | message | `rasm.contracts.host.v1.DeliveryReceiptWire`             |
|  [21]   | `DropReceiptWire`                       | message | `rasm.contracts.host.v1.DropReceiptWire`                 |
|  [22]   | `OutboxRowWire`                         | message | `rasm.contracts.host.v1.OutboxRowWire`                   |
|  [23]   | `DeadLetterRowWire`                     | message | `rasm.contracts.host.v1.DeadLetterRowWire`               |
|  [24]   | `ReplayTallyWire`                       | message | `rasm.contracts.host.v1.ReplayTallyWire`                 |
|  [25]   | `OutboxLane`                            | message | `rasm.contracts.host.v1.OutboxLane`                      |
|  [26]   | `OutboxSweep`                           | message | `rasm.contracts.host.v1.OutboxSweep`                     |
|  [27]   | `BindingStatus`                         | message | `rasm.contracts.host.v1.BindingStatus`                   |
|  [28]   | `CoercedValueWire`                      | message | `rasm.contracts.host.v1.CoercedValueWire`                |
|  [29]   | `WriteBackWire`                         | message | `rasm.contracts.host.v1.WriteBackWire`                   |
|  [30]   | `WriteBackWire.Types.Acknowledged`      | message | `rasm.contracts.host.v1.WriteBackWire.Acknowledged`      |
|  [31]   | `WriteBackWire.Types.Rejected`          | message | `rasm.contracts.host.v1.WriteBackWire.Rejected`          |
|  [32]   | `WriteBackWire.Types.RolledBack`        | message | `rasm.contracts.host.v1.WriteBackWire.RolledBack`        |
|  [33]   | `WriteBackWire.Types.Indeterminate`     | message | `rasm.contracts.host.v1.WriteBackWire.Indeterminate`     |
|  [34]   | `WriteReceiptWire`                      | message | `rasm.contracts.host.v1.WriteReceiptWire`                |
|  [35]   | `Package`                               | enum    | `rasm.contracts.host.v1.Package`                         |
|  [36]   | `TerminationKind`                       | enum    | `rasm.contracts.host.v1.TerminationKind`                 |
|  [37]   | `PemLabel`                              | enum    | `rasm.contracts.host.v1.PemLabel`                        |
|  [38]   | `FlagReason`                            | enum    | `rasm.contracts.host.v1.FlagReason`                      |
|  [39]   | `SupportTrigger`                        | enum    | `rasm.contracts.host.v1.SupportTrigger`                  |
|  [40]   | `DataClassification`                    | enum    | `rasm.contracts.host.v1.DataClassification`              |
|  [41]   | `Topic`                                 | enum    | `rasm.contracts.host.v1.Topic`                           |
|  [42]   | `HopOutcome`                            | enum    | `rasm.contracts.host.v1.HopOutcome`                      |
|  [43]   | `DeliveryDisposition`                   | enum    | `rasm.contracts.host.v1.DeliveryDisposition`             |
|  [44]   | `CircuitState`                          | enum    | `rasm.contracts.host.v1.CircuitState`                    |
|  [45]   | `DropClass`                             | enum    | `rasm.contracts.host.v1.DropClass`                       |
|  [46]   | `OutboxDisposition`                     | enum    | `rasm.contracts.host.v1.OutboxDisposition`               |
|  [47]   | `ExternalTransport`                     | enum    | `rasm.contracts.host.v1.ExternalTransport`               |
|  [48]   | `BindingState`                          | enum    | `rasm.contracts.host.v1.BindingState`                    |
|  [49]   | `BindingDirection`                      | enum    | `rasm.contracts.host.v1.BindingDirection`                |
|  [50]   | `EchoClass`                             | enum    | `rasm.contracts.host.v1.EchoClass`                       |

[ROSTER_SCOPE]: `rasm.contracts.host.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                                    | [KIND]  | [FQN]                                                      |
| :-----: | :---------------------------------------- | :------ | :--------------------------------------------------------- |
|  [01]   | `CommandRowWire`                          | message | `rasm.contracts.host.v1.CommandRowWire`                    |
|  [02]   | `CommandGateWire`                         | message | `rasm.contracts.host.v1.CommandGateWire`                   |
|  [03]   | `CommandPayloadWire`                      | message | `rasm.contracts.host.v1.CommandPayloadWire`                |
|  [04]   | `CommandPayloadWire.Types.Many`           | message | `rasm.contracts.host.v1.CommandPayloadWire.Many`           |
|  [05]   | `CommandInvocation`                       | message | `rasm.contracts.host.v1.CommandInvocation`                 |
|  [06]   | `CommandOutcomeWire`                      | message | `rasm.contracts.host.v1.CommandOutcomeWire`                |
|  [07]   | `DeckReceiptWire`                         | message | `rasm.contracts.host.v1.DeckReceiptWire`                   |
|  [08]   | `IconSlotWire`                            | message | `rasm.contracts.host.v1.IconSlotWire`                      |
|  [09]   | `HintRowWire`                             | message | `rasm.contracts.host.v1.HintRowWire`                       |
|  [10]   | `IntentBindingWire`                       | message | `rasm.contracts.host.v1.IntentBindingWire`                 |
|  [11]   | `OptionRowWire`                           | message | `rasm.contracts.host.v1.OptionRowWire`                     |
|  [12]   | `OptionSourceWire`                        | message | `rasm.contracts.host.v1.OptionSourceWire`                  |
|  [13]   | `OptionSourceWire.Types.Inline`           | message | `rasm.contracts.host.v1.OptionSourceWire.Inline`           |
|  [14]   | `CrumbRowWire`                            | message | `rasm.contracts.host.v1.CrumbRowWire`                      |
|  [15]   | `AvatarRowWire`                           | message | `rasm.contracts.host.v1.AvatarRowWire`                     |
|  [16]   | `FileFilterRowWire`                       | message | `rasm.contracts.host.v1.FileFilterRowWire`                 |
|  [17]   | `MenuRowWire`                             | message | `rasm.contracts.host.v1.MenuRowWire`                       |
|  [18]   | `ToolbarRowWire`                          | message | `rasm.contracts.host.v1.ToolbarRowWire`                    |
|  [19]   | `SectionWire`                             | message | `rasm.contracts.host.v1.SectionWire`                       |
|  [20]   | `ExtentWire`                              | message | `rasm.contracts.host.v1.ExtentWire`                        |
|  [21]   | `ColumnRowWire`                           | message | `rasm.contracts.host.v1.ColumnRowWire`                     |
|  [22]   | `WindowWire`                              | message | `rasm.contracts.host.v1.WindowWire`                        |
|  [23]   | `NumericRangeWire`                        | message | `rasm.contracts.host.v1.NumericRangeWire`                  |
|  [24]   | `NumericRangeWire.Types.Integral`         | message | `rasm.contracts.host.v1.NumericRangeWire.Integral`         |
|  [25]   | `NumericRangeWire.Types.Unsigned`         | message | `rasm.contracts.host.v1.NumericRangeWire.Unsigned`         |
|  [26]   | `NumericRangeWire.Types.Real`             | message | `rasm.contracts.host.v1.NumericRangeWire.Real`             |
|  [27]   | `NumericRangeWire.Types.Precise`          | message | `rasm.contracts.host.v1.NumericRangeWire.Precise`          |
|  [28]   | `ControlIntentWire`                       | message | `rasm.contracts.host.v1.ControlIntentWire`                 |
|  [29]   | `ControlIntentWire.Types.Button`          | message | `rasm.contracts.host.v1.ControlIntentWire.Button`          |
|  [30]   | `ControlIntentWire.Types.Label`           | message | `rasm.contracts.host.v1.ControlIntentWire.Label`           |
|  [31]   | `ControlIntentWire.Types.TextInput`       | message | `rasm.contracts.host.v1.ControlIntentWire.TextInput`       |
|  [32]   | `ControlIntentWire.Types.NumberInput`     | message | `rasm.contracts.host.v1.ControlIntentWire.NumberInput`     |
|  [33]   | `ControlIntentWire.Types.DateInput`       | message | `rasm.contracts.host.v1.ControlIntentWire.DateInput`       |
|  [34]   | `ControlIntentWire.Types.PathInput`       | message | `rasm.contracts.host.v1.ControlIntentWire.PathInput`       |
|  [35]   | `ControlIntentWire.Types.ColorInput`      | message | `rasm.contracts.host.v1.ControlIntentWire.ColorInput`      |
|  [36]   | `ControlIntentWire.Types.Select`          | message | `rasm.contracts.host.v1.ControlIntentWire.Select`          |
|  [37]   | `ControlIntentWire.Types.MultiSelect`     | message | `rasm.contracts.host.v1.ControlIntentWire.MultiSelect`     |
|  [38]   | `ControlIntentWire.Types.Slider`          | message | `rasm.contracts.host.v1.ControlIntentWire.Slider`          |
|  [39]   | `ControlIntentWire.Types.Range`           | message | `rasm.contracts.host.v1.ControlIntentWire.Range`           |
|  [40]   | `ControlIntentWire.Types.Toggle`          | message | `rasm.contracts.host.v1.ControlIntentWire.Toggle`          |
|  [41]   | `ControlIntentWire.Types.Radio`           | message | `rasm.contracts.host.v1.ControlIntentWire.Radio`           |
|  [42]   | `ControlIntentWire.Types.Segmented`       | message | `rasm.contracts.host.v1.ControlIntentWire.Segmented`       |
|  [43]   | `ControlIntentWire.Types.Chip`            | message | `rasm.contracts.host.v1.ControlIntentWire.Chip`            |
|  [44]   | `ControlIntentWire.Types.Progress`        | message | `rasm.contracts.host.v1.ControlIntentWire.Progress`        |
|  [45]   | `ControlIntentWire.Types.Avatar`          | message | `rasm.contracts.host.v1.ControlIntentWire.Avatar`          |
|  [46]   | `ControlIntentWire.Types.Breadcrumb`      | message | `rasm.contracts.host.v1.ControlIntentWire.Breadcrumb`      |
|  [47]   | `ControlIntentWire.Types.Tooltip`         | message | `rasm.contracts.host.v1.ControlIntentWire.Tooltip`         |
|  [48]   | `ControlIntentWire.Types.Banner`          | message | `rasm.contracts.host.v1.ControlIntentWire.Banner`          |
|  [49]   | `ControlIntentWire.Types.EmptyState`      | message | `rasm.contracts.host.v1.ControlIntentWire.EmptyState`      |
|  [50]   | `ControlIntentWire.Types.Grid`            | message | `rasm.contracts.host.v1.ControlIntentWire.Grid`            |
|  [51]   | `ControlIntentWire.Types.Tree`            | message | `rasm.contracts.host.v1.ControlIntentWire.Tree`            |
|  [52]   | `ControlIntentWire.Types.Overview`        | message | `rasm.contracts.host.v1.ControlIntentWire.Overview`        |
|  [53]   | `ControlIntentWire.Types.Menu`            | message | `rasm.contracts.host.v1.ControlIntentWire.Menu`            |
|  [54]   | `ControlIntentWire.Types.Toolbar`         | message | `rasm.contracts.host.v1.ControlIntentWire.Toolbar`         |
|  [55]   | `ControlIntentWire.Types.Tab`             | message | `rasm.contracts.host.v1.ControlIntentWire.Tab`             |
|  [56]   | `ControlIntentWire.Types.Accordion`       | message | `rasm.contracts.host.v1.ControlIntentWire.Accordion`       |
|  [57]   | `ControlIntentWire.Types.Panel`           | message | `rasm.contracts.host.v1.ControlIntentWire.Panel`           |
|  [58]   | `ControlIntentWire.Types.Dock`            | message | `rasm.contracts.host.v1.ControlIntentWire.Dock`            |
|  [59]   | `ControlIntentWire.Types.Splitter`        | message | `rasm.contracts.host.v1.ControlIntentWire.Splitter`        |
|  [60]   | `ControlReceiptWire`                      | message | `rasm.contracts.host.v1.ControlReceiptWire`                |
|  [61]   | `LayoutVarWire`                           | message | `rasm.contracts.host.v1.LayoutVarWire`                     |
|  [62]   | `LayoutTermWire`                          | message | `rasm.contracts.host.v1.LayoutTermWire`                    |
|  [63]   | `LayoutExprWire`                          | message | `rasm.contracts.host.v1.LayoutExprWire`                    |
|  [64]   | `LayoutConstraintWire`                    | message | `rasm.contracts.host.v1.LayoutConstraintWire`              |
|  [65]   | `LayoutEdit`                              | message | `rasm.contracts.host.v1.LayoutEdit`                        |
|  [66]   | `LayoutValue`                             | message | `rasm.contracts.host.v1.LayoutValue`                       |
|  [67]   | `LayoutProgram`                           | message | `rasm.contracts.host.v1.LayoutProgram`                     |
|  [68]   | `SphereWire`                              | message | `rasm.contracts.host.v1.SphereWire`                        |
|  [69]   | `ViewCameraWire`                          | message | `rasm.contracts.host.v1.ViewCameraWire`                    |
|  [70]   | `SectionBoxWire`                          | message | `rasm.contracts.host.v1.SectionBoxWire`                    |
|  [71]   | `VisibilityOverrideWire`                  | message | `rasm.contracts.host.v1.VisibilityOverrideWire`            |
|  [72]   | `ViewMeasurementPointWire`                | message | `rasm.contracts.host.v1.ViewMeasurementPointWire`          |
|  [73]   | `ViewMeasurementWire`                     | message | `rasm.contracts.host.v1.ViewMeasurementWire`               |
|  [74]   | `ViewpointWire`                           | message | `rasm.contracts.host.v1.ViewpointWire`                     |
|  [75]   | `MeshoptStream`                           | message | `rasm.contracts.host.v1.MeshoptStream`                     |
|  [76]   | `Meshlet`                                 | message | `rasm.contracts.host.v1.Meshlet`                           |
|  [77]   | `ResidencyTileWire`                       | message | `rasm.contracts.host.v1.ResidencyTileWire`                 |
|  [78]   | `GeometryResidency`                       | message | `rasm.contracts.host.v1.GeometryResidency`                 |
|  [79]   | `PixelIdentityWire`                       | message | `rasm.contracts.host.v1.PixelIdentityWire`                 |
|  [80]   | `NativeAssetFactWire`                     | message | `rasm.contracts.host.v1.NativeAssetFactWire`               |
|  [81]   | `EvidenceReceiptWire`                     | message | `rasm.contracts.host.v1.EvidenceReceiptWire`               |
|  [82]   | `EvidenceReceiptWire.Types.Surface`       | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Surface`       |
|  [83]   | `EvidenceReceiptWire.Types.Focus`         | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Focus`         |
|  [84]   | `EvidenceReceiptWire.Types.Render`        | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Render`        |
|  [85]   | `EvidenceReceiptWire.Types.Disposal`      | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Disposal`      |
|  [86]   | `EvidenceReceiptWire.Types.Edit`          | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Edit`          |
|  [87]   | `EvidenceReceiptWire.Types.Theme`         | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Theme`         |
|  [88]   | `EvidenceReceiptWire.Types.Motion`        | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Motion`        |
|  [89]   | `EvidenceReceiptWire.Types.Effect`        | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Effect`        |
|  [90]   | `EvidenceReceiptWire.Types.Asset`         | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Asset`         |
|  [91]   | `EvidenceReceiptWire.Types.LiveData`      | message | `rasm.contracts.host.v1.EvidenceReceiptWire.LiveData`      |
|  [92]   | `EvidenceReceiptWire.Types.CollabSync`    | message | `rasm.contracts.host.v1.EvidenceReceiptWire.CollabSync`    |
|  [93]   | `EvidenceReceiptWire.Types.CollabRevert`  | message | `rasm.contracts.host.v1.EvidenceReceiptWire.CollabRevert`  |
|  [94]   | `EvidenceReceiptWire.Types.Media`         | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Media`         |
|  [95]   | `EvidenceReceiptWire.Types.Quality`       | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Quality`       |
|  [96]   | `EvidenceReceiptWire.Types.GpuFrame`      | message | `rasm.contracts.host.v1.EvidenceReceiptWire.GpuFrame`      |
|  [97]   | `EvidenceReceiptWire.Types.Layout`        | message | `rasm.contracts.host.v1.EvidenceReceiptWire.Layout`        |
|  [98]   | `EvidenceReceiptWire.Types.DispatcherLag` | message | `rasm.contracts.host.v1.EvidenceReceiptWire.DispatcherLag` |
|  [99]   | `EvidenceReceiptWire.Types.PreCommit`     | message | `rasm.contracts.host.v1.EvidenceReceiptWire.PreCommit`     |
|  [100]  | `SkewBandWire`                            | message | `rasm.contracts.host.v1.SkewBandWire`                      |
|  [101]  | `EvidenceRowWire`                         | message | `rasm.contracts.host.v1.EvidenceRowWire`                   |
|  [102]  | `EvidenceTimelineWire`                    | message | `rasm.contracts.host.v1.EvidenceTimelineWire`              |
|  [103]  | `CommandScope`                            | enum    | `rasm.contracts.host.v1.CommandScope`                      |
|  [104]  | `ControlEmphasis`                         | enum    | `rasm.contracts.host.v1.ControlEmphasis`                   |
|  [105]  | `ControlTrigger`                          | enum    | `rasm.contracts.host.v1.ControlTrigger`                    |
|  [106]  | `IconPlacement`                           | enum    | `rasm.contracts.host.v1.IconPlacement`                     |
|  [107]  | `NumericKind`                             | enum    | `rasm.contracts.host.v1.NumericKind`                       |
|  [108]  | `TemporalKind`                            | enum    | `rasm.contracts.host.v1.TemporalKind`                      |
|  [109]  | `PickerMode`                              | enum    | `rasm.contracts.host.v1.PickerMode`                        |
|  [110]  | `ColorPosture`                            | enum    | `rasm.contracts.host.v1.ColorPosture`                      |
|  [111]  | `SelectPosture`                           | enum    | `rasm.contracts.host.v1.SelectPosture`                     |
|  [112]  | `MultiPosture`                            | enum    | `rasm.contracts.host.v1.MultiPosture`                      |
|  [113]  | `SegmentPosture`                          | enum    | `rasm.contracts.host.v1.SegmentPosture`                    |
|  [114]  | `ChipPosture`                             | enum    | `rasm.contracts.host.v1.ChipPosture`                       |
|  [115]  | `ProgressForm`                            | enum    | `rasm.contracts.host.v1.ProgressForm`                      |
|  [116]  | `MenuPosture`                             | enum    | `rasm.contracts.host.v1.MenuPosture`                       |
|  [117]  | `OverflowMode`                            | enum    | `rasm.contracts.host.v1.OverflowMode`                      |
|  [118]  | `Orientation`                             | enum    | `rasm.contracts.host.v1.Orientation`                       |
|  [119]  | `ExtentMode`                              | enum    | `rasm.contracts.host.v1.ExtentMode`                        |
|  [120]  | `ExtentUnit`                              | enum    | `rasm.contracts.host.v1.ExtentUnit`                        |
|  [121]  | `ColumnAlign`                             | enum    | `rasm.contracts.host.v1.ColumnAlign`                       |
|  [122]  | `BannerSeverity`                          | enum    | `rasm.contracts.host.v1.BannerSeverity`                    |
|  [123]  | `BannerPlacement`                         | enum    | `rasm.contracts.host.v1.BannerPlacement`                   |
|  [124]  | `OverviewAxis`                            | enum    | `rasm.contracts.host.v1.OverviewAxis`                      |
|  [125]  | `TypographyRole`                          | enum    | `rasm.contracts.host.v1.TypographyRole`                    |
|  [126]  | `LayoutRelation`                          | enum    | `rasm.contracts.host.v1.LayoutRelation`                    |
|  [127]  | `LayoutStrength`                          | enum    | `rasm.contracts.host.v1.LayoutStrength`                    |
|  [128]  | `CameraProjection`                        | enum    | `rasm.contracts.host.v1.CameraProjection`                  |
|  [129]  | `ResidencyKind`                           | enum    | `rasm.contracts.host.v1.ResidencyKind`                     |
|  [130]  | `StreamMode`                              | enum    | `rasm.contracts.host.v1.StreamMode`                        |
|  [131]  | `StreamFilter`                            | enum    | `rasm.contracts.host.v1.StreamFilter`                      |
|  [132]  | `PixelLayout`                             | enum    | `rasm.contracts.host.v1.PixelLayout`                       |
|  [133]  | `MediaOutcome`                            | enum    | `rasm.contracts.host.v1.MediaOutcome`                      |

[ROSTER_SCOPE]: `rasm.contracts.host.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                   | [KIND]  | [FQN]                                           |
| :-----: | :----------------------- | :------ | :---------------------------------------------- |
|  [01]   | `BcfCameraWire`          | message | `rasm.contracts.host.v1.BcfCameraWire`          |
|  [02]   | `BcfColoringWire`        | message | `rasm.contracts.host.v1.BcfColoringWire`        |
|  [03]   | `BcfLineWire`            | message | `rasm.contracts.host.v1.BcfLineWire`            |
|  [04]   | `BcfClippingWire`        | message | `rasm.contracts.host.v1.BcfClippingWire`        |
|  [05]   | `BcfBitmapWire`          | message | `rasm.contracts.host.v1.BcfBitmapWire`          |
|  [06]   | `BcfSnippetWire`         | message | `rasm.contracts.host.v1.BcfSnippetWire`         |
|  [07]   | `BcfDocumentWire`        | message | `rasm.contracts.host.v1.BcfDocumentWire`        |
|  [08]   | `BcfFileWire`            | message | `rasm.contracts.host.v1.BcfFileWire`            |
|  [09]   | `BcfHintsWire`           | message | `rasm.contracts.host.v1.BcfHintsWire`           |
|  [10]   | `BcfShowingWire`         | message | `rasm.contracts.host.v1.BcfShowingWire`         |
|  [11]   | `BcfHidingWire`          | message | `rasm.contracts.host.v1.BcfHidingWire`          |
|  [12]   | `BcfVisibilityWire`      | message | `rasm.contracts.host.v1.BcfVisibilityWire`      |
|  [13]   | `BcfViewpointWire`       | message | `rasm.contracts.host.v1.BcfViewpointWire`       |
|  [14]   | `BcfCommentWire`         | message | `rasm.contracts.host.v1.BcfCommentWire`         |
|  [15]   | `BcfTopicWire`           | message | `rasm.contracts.host.v1.BcfTopicWire`           |
|  [16]   | `DeltaValueWire`         | message | `rasm.contracts.host.v1.DeltaValueWire`         |
|  [17]   | `AspectDeltaWire`        | message | `rasm.contracts.host.v1.AspectDeltaWire`        |
|  [18]   | `DiffEndWire`            | message | `rasm.contracts.host.v1.DiffEndWire`            |
|  [19]   | `DiffModifiedWire`       | message | `rasm.contracts.host.v1.DiffModifiedWire`       |
|  [20]   | `DiffMovedWire`          | message | `rasm.contracts.host.v1.DiffMovedWire`          |
|  [21]   | `DiffRegroupWire`        | message | `rasm.contracts.host.v1.DiffRegroupWire`        |
|  [22]   | `ElementChangeWire`      | message | `rasm.contracts.host.v1.ElementChangeWire`      |
|  [23]   | `ModelDiffWire`          | message | `rasm.contracts.host.v1.ModelDiffWire`          |
|  [24]   | `NodeMatchWire`          | message | `rasm.contracts.host.v1.NodeMatchWire`          |
|  [25]   | `OneOfWire`              | message | `rasm.contracts.host.v1.OneOfWire`              |
|  [26]   | `LengthWire`             | message | `rasm.contracts.host.v1.LengthWire`             |
|  [27]   | `DigitsWire`             | message | `rasm.contracts.host.v1.DigitsWire`             |
|  [28]   | `BoundWire`              | message | `rasm.contracts.host.v1.BoundWire`              |
|  [29]   | `RangeWire`              | message | `rasm.contracts.host.v1.RangeWire`              |
|  [30]   | `ValueMatchWire`         | message | `rasm.contracts.host.v1.ValueMatchWire`         |
|  [31]   | `PredefinedWire`         | message | `rasm.contracts.host.v1.PredefinedWire`         |
|  [32]   | `ClassificationCodeWire` | message | `rasm.contracts.host.v1.ClassificationCodeWire` |
|  [33]   | `AttributeMatchWire`     | message | `rasm.contracts.host.v1.AttributeMatchWire`     |
|  [34]   | `PropertyMatchWire`      | message | `rasm.contracts.host.v1.PropertyMatchWire`      |
|  [35]   | `SpatialWire`            | message | `rasm.contracts.host.v1.SpatialWire`            |
|  [36]   | `ComposedWire`           | message | `rasm.contracts.host.v1.ComposedWire`           |
|  [37]   | `ConnectedWire`          | message | `rasm.contracts.host.v1.ConnectedWire`          |
|  [38]   | `VoidedWire`             | message | `rasm.contracts.host.v1.VoidedWire`             |
|  [39]   | `AssessmentMatchWire`    | message | `rasm.contracts.host.v1.AssessmentMatchWire`    |
|  [40]   | `GenericMatchWire`       | message | `rasm.contracts.host.v1.GenericMatchWire`       |
|  [41]   | `PredicateSetWire`       | message | `rasm.contracts.host.v1.PredicateSetWire`       |
|  [42]   | `PredicateWire`          | message | `rasm.contracts.host.v1.PredicateWire`          |
|  [43]   | `BcfStatus`              | enum    | `rasm.contracts.host.v1.BcfStatus`              |
|  [44]   | `BcfBitmapFormat`        | enum    | `rasm.contracts.host.v1.BcfBitmapFormat`        |
|  [45]   | `IfcDomain`              | enum    | `rasm.contracts.host.v1.IfcDomain`              |
|  [46]   | `SpatialReach`           | enum    | `rasm.contracts.host.v1.SpatialReach`           |
|  [47]   | `DeltaShape`             | enum    | `rasm.contracts.host.v1.DeltaShape`             |

[ROSTER_SCOPE]: `rasm.contracts.host.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [FQN]                                        |
| :-----: | :-------------------- | :------ | :------------------------------------------- |
|  [01]   | `LabelPair`           | message | `rasm.contracts.host.v1.LabelPair`           |
|  [02]   | `HostFingerprintWire` | message | `rasm.contracts.host.v1.HostFingerprintWire` |
|  [03]   | `BenchInputWire`      | message | `rasm.contracts.host.v1.BenchInputWire`      |
|  [04]   | `ChromeTraceWire`     | message | `rasm.contracts.host.v1.ChromeTraceWire`     |
|  [05]   | `BenchmarkExportWire` | message | `rasm.contracts.host.v1.BenchmarkExportWire` |
|  [06]   | `EpContextWire`       | message | `rasm.contracts.host.v1.EpContextWire`       |
|  [07]   | `ProfileArtifactWire` | message | `rasm.contracts.host.v1.ProfileArtifactWire` |
|  [08]   | `BenchKernelWire`     | message | `rasm.contracts.host.v1.BenchKernelWire`     |
|  [09]   | `BenchAggregate`      | message | `rasm.contracts.host.v1.BenchAggregate`      |
|  [10]   | `RungCell`            | message | `rasm.contracts.host.v1.RungCell`            |
|  [11]   | `BenchBandWire`       | message | `rasm.contracts.host.v1.BenchBandWire`       |
|  [12]   | `BenchMetric`         | message | `rasm.contracts.host.v1.BenchMetric`         |
|  [13]   | `BenchmarkClaimWire`  | message | `rasm.contracts.host.v1.BenchmarkClaimWire`  |
|  [14]   | `DescriptorPinWire`   | message | `rasm.contracts.host.v1.DescriptorPinWire`   |
|  [15]   | `EditTombstone`       | message | `rasm.contracts.host.v1.EditTombstone`       |
|  [16]   | `EditMembers`         | message | `rasm.contracts.host.v1.EditMembers`         |
|  [17]   | `EntityEditWire`      | message | `rasm.contracts.host.v1.EntityEditWire`      |
|  [18]   | `BenchModality`       | enum    | `rasm.contracts.host.v1.BenchModality`       |
|  [19]   | `BenchPolarity`       | enum    | `rasm.contracts.host.v1.BenchPolarity`       |
|  [20]   | `PayloadBand`         | enum    | `rasm.contracts.host.v1.PayloadBand`         |
|  [21]   | `BenchRung`           | enum    | `rasm.contracts.host.v1.BenchRung`           |

[ROSTER_SCOPE]: `rasm.contracts.organization.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [FQN]                                             |
| :-----: | :----------------- | :------ | :------------------------------------------------ |
|  [01]   | `EntityWire`       | message | `rasm.contracts.organization.v1.EntityWire`       |
|  [02]   | `ContainmentWire`  | message | `rasm.contracts.organization.v1.ContainmentWire`  |
|  [03]   | `ViewOverrideWire` | message | `rasm.contracts.organization.v1.ViewOverrideWire` |
|  [04]   | `OrganizationWire` | message | `rasm.contracts.organization.v1.OrganizationWire` |

[ROSTER_SCOPE]: `rasm.contracts.parity.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [FQN]                                   |
| :-----: | :------------------- | :------ | :-------------------------------------- |
|  [01]   | `Profile`            | message | `rasm.contracts.parity.v1.Profile`      |
|  [02]   | `Artifact`           | message | `rasm.contracts.parity.v1.Artifact`     |
|  [03]   | `Capability`         | message | `rasm.contracts.parity.v1.Capability`   |
|  [04]   | `Backend`            | message | `rasm.contracts.parity.v1.Backend`      |
|  [05]   | `Row`                | message | `rasm.contracts.parity.v1.Row`          |
|  [06]   | `Row.Types.Withheld` | message | `rasm.contracts.parity.v1.Row.Withheld` |
|  [07]   | `Conformance`        | message | `rasm.contracts.parity.v1.Conformance`  |
|  [08]   | `Tenancy`            | enum    | `rasm.contracts.parity.v1.Tenancy`      |
|  [09]   | `Topology`           | enum    | `rasm.contracts.parity.v1.Topology`     |
|  [10]   | `Lifecycle`          | enum    | `rasm.contracts.parity.v1.Lifecycle`    |
|  [11]   | `Isolation`          | enum    | `rasm.contracts.parity.v1.Isolation`    |
|  [12]   | `ArtifactRole`       | enum    | `rasm.contracts.parity.v1.ArtifactRole` |
|  [13]   | `Provider`           | enum    | `rasm.contracts.parity.v1.Provider`     |
|  [14]   | `FailureRank`        | enum    | `rasm.contracts.parity.v1.FailureRank`  |
|  [15]   | `RestartClass`       | enum    | `rasm.contracts.parity.v1.RestartClass` |
|  [16]   | `MinterRole`         | enum    | `rasm.contracts.parity.v1.MinterRole`   |

[ROSTER_SCOPE]: `rasm.contracts.scene.v1` — emitted by `assay contracts generate` from the built descriptor set; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]  | [FQN]                                     |
| :-----: | :---------------- | :------ | :---------------------------------------- |
|  [01]   | `Vector`          | message | `rasm.contracts.scene.v1.Vector`          |
|  [02]   | `Spectrum`        | message | `rasm.contracts.scene.v1.Spectrum`        |
|  [03]   | `SolarFrame`      | message | `rasm.contracts.scene.v1.SolarFrame`      |
|  [04]   | `SolarAngles`     | message | `rasm.contracts.scene.v1.SolarAngles`     |
|  [05]   | `SitedSun`        | message | `rasm.contracts.scene.v1.SitedSun`        |
|  [06]   | `SceneSun`        | message | `rasm.contracts.scene.v1.SceneSun`        |
|  [07]   | `WebRef`          | message | `rasm.contracts.scene.v1.WebRef`          |
|  [08]   | `Power`           | message | `rasm.contracts.scene.v1.Power`           |
|  [09]   | `Cone`            | message | `rasm.contracts.scene.v1.Cone`            |
|  [10]   | `Extent`          | message | `rasm.contracts.scene.v1.Extent`          |
|  [11]   | `Photometry`      | message | `rasm.contracts.scene.v1.Photometry`      |
|  [12]   | `Fidelity`        | message | `rasm.contracts.scene.v1.Fidelity`        |
|  [13]   | `Shading`         | message | `rasm.contracts.scene.v1.Shading`         |
|  [14]   | `SceneDescriptor` | message | `rasm.contracts.scene.v1.SceneDescriptor` |
|  [15]   | `LightKind`       | enum    | `rasm.contracts.scene.v1.LightKind`       |
|  [16]   | `Falloff`         | enum    | `rasm.contracts.scene.v1.Falloff`         |
|  [17]   | `WebDialect`      | enum    | `rasm.contracts.scene.v1.WebDialect`      |
<!-- roster:end -->

## [04]-[SERVICES]

[ENTRY_SCOPE]: server overrides on each `<Svc>.<Svc>Base` — `ServerCallContext` rides last, a stream override returns bare `Task`

| [INDEX] | [SURFACE]                                                                                             | [SHAPE]  | [CAPABILITY] |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------- | :----------- |
|  [01]   | `ComputeServiceBase.Tessellate(TessellationRequest) -> Task<TessellationReceipt>`                     | instance | unary        |
|  [02]   | `DocumentServiceBase.ExecuteTransaction(TransactionRequest) -> Task<TransactionReceipt>`              | instance | unary        |
|  [03]   | `ControlServiceBase.ReloadOptions(ReloadOptionsRequest) -> Task<ReloadReply>`                         | instance | unary        |
|  [04]   | `ControlServiceBase.DispatchTool(DispatchToolRequest) -> Task<CommandReply>`                          | instance | unary        |
|  [05]   | `ControlServiceBase.DispatchPatch(DispatchPatchRequest) -> Task<ReloadReply>`                         | instance | unary        |
|  [06]   | `ControlServiceBase.SetDegradation(SetDegradationRequest) -> Task<DegradationReply>`                  | instance | unary        |
|  [07]   | `ControlServiceBase.DrainRuntime(DrainRuntimeRequest) -> Task<DrainReply>`                            | instance | unary        |
|  [08]   | `DiagnosticServiceBase.CaptureBundle(SupportBundleRequest) -> Task<SupportBundleReply>`               | instance | unary        |
|  [09]   | `ArtifactSyncServiceBase.Sync(IAsyncStreamReader<ArtifactFrame>, IServerStreamWriter<ArtifactFrame>)` | instance | bidi         |

- `ComputeService` and `DocumentService` sit in `compute.proto` (`ComputeGrpc.cs`), `ControlService` and `DiagnosticService` in `control.proto` (`ControlGrpc.cs`), all four under `Rasm.Contracts.Compute.V1`; `ExecuteTransaction` declares `idempotency_level = IDEMPOTENT`.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Generation is the only author: `assay contracts generate` rewrites the whole out root under `clean` and fills `[03]-[ROSTER]` between its markers, `assay contracts check` regenerates into scratch and byte-diffs both, and a hand edit of either is the freshness failure that gate reports.
- One assembly serves client and server: `no_client`/`no_server` are not passed, app roots derive `<Svc>.<Svc>Base` and dial `<Svc>.<Svc>Client` off the same reference, and the unused half ships as IL.
- Emission stays nullable-oblivious under every project setting — protoc writes no `#nullable` and Roslyn holds an auto-generated file in the disabled context — so a message-typed column reads `null` until set and every consumer admits at its boundary.
- Every `buf.validate` rule rides the embedded descriptor, so a consumer's `ProtoValidate.Validator` reads the corpus constraints off `<Msg>.Descriptor` and no rule is restated in C#.

[STACKING]:
- `Google.Protobuf`(`../../.api/api-protobuf.md`): every message row is an `IMessage<T>` with its static `Parser` and `Descriptor`, so `MessageParser<T>.ParseFrom`, `CodedInputStream.CreateWithLimits`, `CodedOutputStream.Deterministic`, `MessageExtensions.WriteTo(IBufferWriter<byte>)`, `FieldMask.IsValid(Descriptor, mask)`, `Any.Is(Descriptor)`, and `TypeRegistry.FromFiles(<File>Reflection.Descriptor…)` all read off the generated members here.
- `Grpc.Core.Api`(`../../.api/api-grpc-core-api.md`): `<Svc>.BindService` folds a `<Svc>Base` onto one `ServerServiceDefinition` of `Method<TReq,TResp>` rows over `Marshallers.Create`, `<Svc>Client` binds a `CallInvoker` through `ClientBase<T>`, and `Sync` returns `AsyncDuplexStreamingCall<ArtifactFrame, ArtifactFrame>`.
- `Google.Api.CommonProtos`(`../../.api/api-commonprotos.md`): `fault.v1.FaultDetail.violations` is `repeated google.rpc.BadRequest.FieldViolation`, and `element.v1`/`host.v1` declare `google.type.Date`, `DateTime`, `TimeOfDay`; the `FaultDetail` rides `google.rpc.Status.details` as one `Any`.
- `Grpc.StatusProto`(`../../.api/api-grpc-statusproto.md`): carries that status on the trailer — `Rasm.AppHost/Runtime/ports#WIRE_LAW` `FaultWire.Raise`/`Decode`.
- `ProtoValidate`(`../../.api/api-protovalidate.md`): `Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` `ParseGuard` validates every admitted message against the rules this emission embeds.
- `Grpc.AspNetCore.Server`(`../../.api/api-grpc-aspnetcore.md`): `MapGrpcService<TImpl>` mounts an implementation deriving `<Svc>.<Svc>Base`, the `BindServiceMethodAttribute` on the base naming the registration.
- `Grpc.Net.Client`(`../../.api/api-grpc-client.md`): `new <Svc>.<Svc>Client(invoker)` binds the intercepted `CreateCallInvoker` so every verb quartet rides the channel's interceptor chain.
- Consumers: `Rasm.Compute` (`Runtime/wire#PROTO_VOCABULARY` `WireServices.Of` + `ParseGuard`, `Runtime/channels#ARTIFACT_FRAMES` `FrameEdge.Sync`, `Runtime/claims` benchmark families); `Rasm.AppHost` (`Wire/companion#CONTROL_SERVICE` `ControlService.ControlServiceBase`/`DiagnosticService.DiagnosticServiceBase`, `Runtime/ports#WIRE_LAW` `WireJson`/`FaultWire` over the fault and host families); `Rasm.Element` (`Graph/wire#WIRE_CODEC` element family); `Rasm.Rhino` (`Document/layers#ORGANIZATION_PROJECTION`, `Objects/lights#ASK_AND_COMMIT` scene family); `Rasm.Materials` (`Appearance/interchange#MATERIAL_WIRE`, `Raster/set#SET_INGEST` appearance family); `Rasm.Fabrication` (`Spec/tolerance#OWNER_FOLD` `FeatureControl`); `Rasm.Persistence` (`Version/merge` `EntityEditWire`); `Rasm.Bim` (`Review/issues#TS_PROJECTION`, `Model/query#PREDICATE_WIRE` host bim families); `Rasm.AppUi` (`Diagnostics/evidence`, `Shell/commands`, `Shell/controls` host appui families) — each transcribing through its `[Mapper]` owner.

[LOCAL_ADMISSION]:
- Consumers reach the bindings through `<ProjectReference Include="../Rasm.Contracts/Rasm.Contracts.csproj" />` alone; no consumer compiles a `.proto`, carries a `<Protobuf>` item, or copies a generated type.
- Bindings record their own field types, optionality, and collection cardinality as generated (`docs/laws/topology.md` `[FENCE_SEAM]`); a fence composing one binds the landed declaration, never the source schema's spelling.
- New families land as one corpus source, one regeneration, and one emitted roster table here; a new rpc lands as one `[04]-[SERVICES]` row.

[RAIL_LAW]:
- Package: `Rasm.Contracts`
- Owns: the C# spelling of every `rasm.contracts.<family>.v1` message, enum, file descriptor, service base, and client
- Accept: project-referenced consumption of the generated members under the generator's symbol grammar, regenerated only through the assay contracts writer
- Reject: a per-consumer `Grpc.Tools` emission, a hand-written or hand-patched binding, a second copy of a generated type, a hand roster row inside the markers, and a descriptor mirror beside `<File>Reflection.Descriptor`
