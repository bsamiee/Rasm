# [RASM_CONTRACTS_API_RASM_CONTRACTS]

`Rasm.Contracts` owns the committed generated C# symbol plane: messages, enums, reflection descriptors, service bases, and clients share one assembly consumed by project in the workspace and by versioned NuGet package outside it. Corpus generation authors every public member.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rasm.Contracts`
- package: `Rasm.Contracts`
- version: `0.1.0`
- assembly: `Rasm.Contracts.dll`
- namespace: `Rasm.Contracts.<Family>.V1`
- depends: `Celly.Protovalidate`, `Google.Api.CommonProtos`, `Google.Protobuf`, `Grpc.Core.Api`
- role: Generated-only assembly with publisher-owned package types omitted from its public roots
- rail: Project-referenced workspace symbols and package-referenced external symbols over the same embedded descriptors

## [02]-[SYMBOL_GRAMMAR]

[SYMBOL_GRAMMAR_SCOPE]: One proto declaration's generated C# correspondence

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Rasm.Contracts.<F>.V1.<Msg>`        | class         | generated `IMessage<T>` value                       |
|  [02]   | `<Msg>.Parser`                       | property      | typed binary and stream parser                      |
|  [03]   | `<Msg>.Descriptor`                   | property      | generated message descriptor                        |
|  [04]   | `<File>Reflection.Descriptor`        | property      | generated file descriptor and imported option graph |
|  [05]   | `Rasm.Contracts.<F>.V1.<Enum>`       | enum          | generated closed numeric vocabulary                 |
|  [06]   | `<Msg>.<Oneof>OneofCase`             | enum          | generated oneof discriminant                        |
|  [07]   | `<Msg>.Has<Field>`/`.Clear<Field>()` | property pair | optional scalar and enum presence access            |
|  [08]   | `<Msg>.Types.<Nested>`               | class         | nested generated declaration                        |
|  [09]   | `<Svc>.<Svc>Base`                    | class         | generated server override surface                   |
|  [10]   | `<Svc>.<Svc>Client`                  | class         | generated unary and streaming client surface        |

## [03]-[ROSTER]

<!-- roster:begin -->
[ROSTER_SCOPE]: `rasm.contracts.artifact.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [FQN]                                              |
| :-----: | :---------------------- | :------ | :-------------- | :------------------------------------------------- |
|  [01]   | `ArtifactRef`           | message | support-closure | `rasm.contracts.artifact.v1.ArtifactRef`           |
|  [02]   | `ArtifactFrame`         | message | support-closure | `rasm.contracts.artifact.v1.ArtifactFrame`         |
|  [03]   | `FetchRequest`          | message | public-root     | `rasm.contracts.artifact.v1.FetchRequest`          |
|  [04]   | `FetchResponse`         | message | public-root     | `rasm.contracts.artifact.v1.FetchResponse`         |
|  [05]   | `PutRequest`            | message | public-root     | `rasm.contracts.artifact.v1.PutRequest`            |
|  [06]   | `PutResponse`           | message | public-root     | `rasm.contracts.artifact.v1.PutResponse`           |
|  [07]   | `ArtifactService`       | service | support-closure | `rasm.contracts.artifact.v1.ArtifactService`       |
|  [08]   | `ArtifactService.Fetch` | method  | public-root     | `rasm.contracts.artifact.v1.ArtifactService.Fetch` |
|  [09]   | `ArtifactService.Put`   | method  | public-root     | `rasm.contracts.artifact.v1.ArtifactService.Put`   |

[ROSTER_SCOPE]: `rasm.contracts.appearance.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                            | [KIND]  | [ORIGIN]        | [FQN]                                                              |
| :-----: | :------------------------------------------------ | :------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `Provenance`                                      | message | support-closure | `rasm.contracts.appearance.v1.Provenance`                          |
|  [02]   | `Provenance.Types.Capture`                        | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Capture`                  |
|  [03]   | `Provenance.Types.Fit`                            | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Fit`                      |
|  [04]   | `Provenance.Types.Inference`                      | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Inference`                |
|  [05]   | `Provenance.Types.Chromaticity`                   | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity`             |
|  [06]   | `Provenance.Types.Chromaticity.Types.Dominance`   | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity.Dominance`   |
|  [07]   | `Provenance.Types.Chromaticity.Types.Temperature` | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Chromaticity.Temperature` |
|  [08]   | `Provenance.Types.Card`                           | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Card`                     |
|  [09]   | `Provenance.Types.Ingest`                         | message | support-closure | `rasm.contracts.appearance.v1.Provenance.Ingest`                   |
|  [10]   | `Press`                                           | message | support-closure | `rasm.contracts.appearance.v1.Press`                               |
|  [11]   | `SurfaceSet`                                      | message | support-closure | `rasm.contracts.appearance.v1.SurfaceSet`                          |
|  [12]   | `BakedSet`                                        | message | support-closure | `rasm.contracts.appearance.v1.BakedSet`                            |
|  [13]   | `EnvironmentSet`                                  | message | support-closure | `rasm.contracts.appearance.v1.EnvironmentSet`                      |
|  [14]   | `Set`                                             | message | public-root     | `rasm.contracts.appearance.v1.Set`                                 |
|  [15]   | `Color`                                           | message | support-closure | `rasm.contracts.appearance.v1.Color`                               |
|  [16]   | `OpenPbr`                                         | message | support-closure | `rasm.contracts.appearance.v1.OpenPbr`                             |
|  [17]   | `EmissionReadout`                                 | message | support-closure | `rasm.contracts.appearance.v1.EmissionReadout`                     |
|  [18]   | `Emission`                                        | message | support-closure | `rasm.contracts.appearance.v1.Emission`                            |
|  [19]   | `Material`                                        | message | public-root     | `rasm.contracts.appearance.v1.Material`                            |

[ROSTER_SCOPE]: `rasm.contracts.compute.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [ORIGIN]        | [FQN]                                                     |
| :-----: | :------------------------------ | :------ | :-------------- | :-------------------------------------------------------- |
|  [01]   | `SetDegradationRequest`         | message | public-root     | `rasm.contracts.compute.v1.SetDegradationRequest`         |
|  [02]   | `SetDegradationResponse`        | message | public-root     | `rasm.contracts.compute.v1.SetDegradationResponse`        |
|  [03]   | `DrainRuntimeRequest`           | message | public-root     | `rasm.contracts.compute.v1.DrainRuntimeRequest`           |
|  [04]   | `DrainStep`                     | message | support-closure | `rasm.contracts.compute.v1.DrainStep`                     |
|  [05]   | `DrainRuntimeResponse`          | message | public-root     | `rasm.contracts.compute.v1.DrainRuntimeResponse`          |
|  [06]   | `DegradationLevel`              | enum    | support-closure | `rasm.contracts.compute.v1.DegradationLevel`              |
|  [07]   | `DrainBand`                     | enum    | support-closure | `rasm.contracts.compute.v1.DrainBand`                     |
|  [08]   | `DeadlineOutcome`               | enum    | support-closure | `rasm.contracts.compute.v1.DeadlineOutcome`               |
|  [09]   | `RuntimePhase`                  | enum    | support-closure | `rasm.contracts.compute.v1.RuntimePhase`                  |
|  [10]   | `ElementScope`                  | message | support-closure | `rasm.contracts.compute.v1.ElementScope`                  |
|  [11]   | `EntityScope`                   | message | support-closure | `rasm.contracts.compute.v1.EntityScope`                   |
|  [12]   | `TessellationScope`             | message | support-closure | `rasm.contracts.compute.v1.TessellationScope`             |
|  [13]   | `TessellateRequest`             | message | public-root     | `rasm.contracts.compute.v1.TessellateRequest`             |
|  [14]   | `Semantic`                      | message | support-closure | `rasm.contracts.compute.v1.Semantic`                      |
|  [15]   | `TessellateResponse`            | message | public-root     | `rasm.contracts.compute.v1.TessellateResponse`            |
|  [16]   | `Spill`                         | enum    | support-closure | `rasm.contracts.compute.v1.Spill`                         |
|  [17]   | `GeomSetting`                   | enum    | support-closure | `rasm.contracts.compute.v1.GeomSetting`                   |
|  [18]   | `Dimensionality`                | enum    | support-closure | `rasm.contracts.compute.v1.Dimensionality`                |
|  [19]   | `ControlService`                | service | support-closure | `rasm.contracts.compute.v1.ControlService`                |
|  [20]   | `ControlService.SetDegradation` | method  | public-root     | `rasm.contracts.compute.v1.ControlService.SetDegradation` |
|  [21]   | `ControlService.DrainRuntime`   | method  | public-root     | `rasm.contracts.compute.v1.ControlService.DrainRuntime`   |
|  [22]   | `ComputeService`                | service | support-closure | `rasm.contracts.compute.v1.ComputeService`                |
|  [23]   | `ComputeService.Tessellate`     | method  | public-root     | `rasm.contracts.compute.v1.ComputeService.Tessellate`     |

[ROSTER_SCOPE]: `rasm.contracts.availability.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [FQN]                                                        |
| :-----: | :---------------------------------- | :------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `CommandVerdictWire`                | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire`          |
|  [02]   | `CommandVerdictWire.Types.Gated`    | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire.Gated`    |
|  [03]   | `CommandVerdictWire.Types.Withheld` | message | support-closure | `rasm.contracts.availability.v1.CommandVerdictWire.Withheld` |
|  [04]   | `CommandAvailability`               | message | public-root     | `rasm.contracts.availability.v1.CommandAvailability`         |

[ROSTER_SCOPE]: `rasm.contracts.bcf.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                     |
| :-----: | :------------------ | :------ | :-------------- | :---------------------------------------- |
|  [01]   | `BcfCameraWire`     | message | support-closure | `rasm.contracts.bcf.v1.BcfCameraWire`     |
|  [02]   | `BcfColoringWire`   | message | support-closure | `rasm.contracts.bcf.v1.BcfColoringWire`   |
|  [03]   | `BcfLineWire`       | message | support-closure | `rasm.contracts.bcf.v1.BcfLineWire`       |
|  [04]   | `BcfClippingWire`   | message | support-closure | `rasm.contracts.bcf.v1.BcfClippingWire`   |
|  [05]   | `BcfBitmapWire`     | message | support-closure | `rasm.contracts.bcf.v1.BcfBitmapWire`     |
|  [06]   | `BcfSnippetWire`    | message | support-closure | `rasm.contracts.bcf.v1.BcfSnippetWire`    |
|  [07]   | `BcfDocumentWire`   | message | support-closure | `rasm.contracts.bcf.v1.BcfDocumentWire`   |
|  [08]   | `BcfFileWire`       | message | support-closure | `rasm.contracts.bcf.v1.BcfFileWire`       |
|  [09]   | `BcfHintsWire`      | message | support-closure | `rasm.contracts.bcf.v1.BcfHintsWire`      |
|  [10]   | `BcfShowingWire`    | message | support-closure | `rasm.contracts.bcf.v1.BcfShowingWire`    |
|  [11]   | `BcfHidingWire`     | message | support-closure | `rasm.contracts.bcf.v1.BcfHidingWire`     |
|  [12]   | `BcfVisibilityWire` | message | support-closure | `rasm.contracts.bcf.v1.BcfVisibilityWire` |
|  [13]   | `BcfViewpointWire`  | message | public-root     | `rasm.contracts.bcf.v1.BcfViewpointWire`  |
|  [14]   | `BcfCommentWire`    | message | support-closure | `rasm.contracts.bcf.v1.BcfCommentWire`    |
|  [15]   | `BcfTopicWire`      | message | public-root     | `rasm.contracts.bcf.v1.BcfTopicWire`      |
|  [16]   | `BcfStatus`         | enum    | support-closure | `rasm.contracts.bcf.v1.BcfStatus`         |
|  [17]   | `BcfBitmapFormat`   | enum    | support-closure | `rasm.contracts.bcf.v1.BcfBitmapFormat`   |

[ROSTER_SCOPE]: `rasm.contracts.benchmark.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [ORIGIN]        | [FQN]                                             |
| :-----: | :-------------------- | :------ | :-------------- | :------------------------------------------------ |
|  [01]   | `BenchInputWire`      | message | support-closure | `rasm.contracts.benchmark.v1.BenchInputWire`      |
|  [02]   | `ChromeTraceWire`     | message | support-closure | `rasm.contracts.benchmark.v1.ChromeTraceWire`     |
|  [03]   | `BenchmarkExportWire` | message | support-closure | `rasm.contracts.benchmark.v1.BenchmarkExportWire` |
|  [04]   | `EpContextWire`       | message | support-closure | `rasm.contracts.benchmark.v1.EpContextWire`       |
|  [05]   | `ProfileArtifactWire` | message | support-closure | `rasm.contracts.benchmark.v1.ProfileArtifactWire` |
|  [06]   | `BenchKernelWire`     | message | support-closure | `rasm.contracts.benchmark.v1.BenchKernelWire`     |
|  [07]   | `BenchAggregate`      | message | support-closure | `rasm.contracts.benchmark.v1.BenchAggregate`      |
|  [08]   | `RungCell`            | message | support-closure | `rasm.contracts.benchmark.v1.RungCell`            |
|  [09]   | `BenchBandWire`       | message | support-closure | `rasm.contracts.benchmark.v1.BenchBandWire`       |
|  [10]   | `BenchMetric`         | message | support-closure | `rasm.contracts.benchmark.v1.BenchMetric`         |
|  [11]   | `BenchmarkClaimWire`  | message | public-root     | `rasm.contracts.benchmark.v1.BenchmarkClaimWire`  |
|  [12]   | `BenchModality`       | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchModality`       |
|  [13]   | `BenchPolarity`       | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchPolarity`       |
|  [14]   | `PayloadBand`         | enum    | support-closure | `rasm.contracts.benchmark.v1.PayloadBand`         |
|  [15]   | `BenchRung`           | enum    | support-closure | `rasm.contracts.benchmark.v1.BenchRung`           |

[ROSTER_SCOPE]: `rasm.contracts.declaration.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                             |
| :-----: | :------------------ | :------ | :-------------- | :------------------------------------------------ |
|  [01]   | `ImpactCell`        | message | support-closure | `rasm.contracts.declaration.v1.ImpactCell`        |
|  [02]   | `Source`            | message | support-closure | `rasm.contracts.declaration.v1.Source`            |
|  [03]   | `DeclarationRecord` | message | public-root     | `rasm.contracts.declaration.v1.DeclarationRecord` |
|  [04]   | `Registry`          | enum    | support-closure | `rasm.contracts.declaration.v1.Registry`          |
|  [05]   | `DeclaredUnit`      | enum    | support-closure | `rasm.contracts.declaration.v1.DeclaredUnit`      |
|  [06]   | `Standard`          | enum    | support-closure | `rasm.contracts.declaration.v1.Standard`          |
|  [07]   | `Subtype`           | enum    | support-closure | `rasm.contracts.declaration.v1.Subtype`           |
|  [08]   | `ImpactCategory`    | enum    | support-closure | `rasm.contracts.declaration.v1.ImpactCategory`    |
|  [09]   | `Module`            | enum    | support-closure | `rasm.contracts.declaration.v1.Module`            |

[ROSTER_SCOPE]: `rasm.contracts.element.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [FQN]                                          |
| :-----: | :------------------- | :------ | :-------------- | :--------------------------------------------- |
|  [01]   | `ClassificationWire` | message | support-closure | `rasm.contracts.element.v1.ClassificationWire` |
|  [02]   | `OwnerHistoryWire`   | message | support-closure | `rasm.contracts.element.v1.OwnerHistoryWire`   |
|  [03]   | `SchemaSpanWire`     | message | support-closure | `rasm.contracts.element.v1.SchemaSpanWire`     |
|  [04]   | `PlacementWire`      | message | support-closure | `rasm.contracts.element.v1.PlacementWire`      |
|  [05]   | `RepresentationWire` | message | support-closure | `rasm.contracts.element.v1.RepresentationWire` |
|  [06]   | `ObjectWire`         | message | support-closure | `rasm.contracts.element.v1.ObjectWire`         |
|  [07]   | `PropertySetWire`    | message | support-closure | `rasm.contracts.element.v1.PropertySetWire`    |
|  [08]   | `GroupIdentityWire`  | message | support-closure | `rasm.contracts.element.v1.GroupIdentityWire`  |
|  [09]   | `GroupWire`          | message | support-closure | `rasm.contracts.element.v1.GroupWire`          |
|  [10]   | `QuantitySetWire`    | message | support-closure | `rasm.contracts.element.v1.QuantitySetWire`    |
|  [11]   | `AppearanceWire`     | message | support-closure | `rasm.contracts.element.v1.AppearanceWire`     |
|  [12]   | `NodeWire`           | message | public-root     | `rasm.contracts.element.v1.NodeWire`           |
|  [13]   | `ObjectKind`         | enum    | support-closure | `rasm.contracts.element.v1.ObjectKind`         |
|  [14]   | `ReleaseVersion`     | enum    | support-closure | `rasm.contracts.element.v1.ReleaseVersion`     |
|  [15]   | `ChangeAction`       | enum    | support-closure | `rasm.contracts.element.v1.ChangeAction`       |
|  [16]   | `ObjectState`        | enum    | support-closure | `rasm.contracts.element.v1.ObjectState`        |
|  [17]   | `RepresentationKind` | enum    | support-closure | `rasm.contracts.element.v1.RepresentationKind` |
|  [18]   | `EditTombstone`      | message | support-closure | `rasm.contracts.element.v1.EditTombstone`      |
|  [19]   | `EditMembers`        | message | support-closure | `rasm.contracts.element.v1.EditMembers`        |
|  [20]   | `EntityEditWire`     | message | public-root     | `rasm.contracts.element.v1.EntityEditWire`     |

[ROSTER_SCOPE]: `rasm.contracts.bim.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                     |
| :-----: | :------------------ | :------ | :-------------- | :---------------------------------------- |
|  [01]   | `DeltaValueWire`    | message | support-closure | `rasm.contracts.bim.v1.DeltaValueWire`    |
|  [02]   | `AspectDeltaWire`   | message | support-closure | `rasm.contracts.bim.v1.AspectDeltaWire`   |
|  [03]   | `DiffEndWire`       | message | support-closure | `rasm.contracts.bim.v1.DiffEndWire`       |
|  [04]   | `DiffModifiedWire`  | message | support-closure | `rasm.contracts.bim.v1.DiffModifiedWire`  |
|  [05]   | `DiffMovedWire`     | message | support-closure | `rasm.contracts.bim.v1.DiffMovedWire`     |
|  [06]   | `DiffRegroupWire`   | message | support-closure | `rasm.contracts.bim.v1.DiffRegroupWire`   |
|  [07]   | `ElementChangeWire` | message | support-closure | `rasm.contracts.bim.v1.ElementChangeWire` |
|  [08]   | `ModelDiffWire`     | message | public-root     | `rasm.contracts.bim.v1.ModelDiffWire`     |
|  [09]   | `DeltaShape`        | enum    | support-closure | `rasm.contracts.bim.v1.DeltaShape`        |

[ROSTER_SCOPE]: `rasm.contracts.binding.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [FQN]                                                   |
| :-----: | :---------------------------------- | :------ | :-------------- | :------------------------------------------------------ |
|  [01]   | `BindingStatus`                     | message | public-root     | `rasm.contracts.binding.v1.BindingStatus`               |
|  [02]   | `ExternalTransport`                 | enum    | support-closure | `rasm.contracts.binding.v1.ExternalTransport`           |
|  [03]   | `BindingState`                      | enum    | support-closure | `rasm.contracts.binding.v1.BindingState`                |
|  [04]   | `BindingDirection`                  | enum    | support-closure | `rasm.contracts.binding.v1.BindingDirection`            |
|  [05]   | `CoercedValueWire`                  | message | public-root     | `rasm.contracts.binding.v1.CoercedValueWire`            |
|  [06]   | `WriteBackWire`                     | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire`               |
|  [07]   | `WriteBackWire.Types.Acknowledged`  | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Acknowledged`  |
|  [08]   | `WriteBackWire.Types.Rejected`      | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Rejected`      |
|  [09]   | `WriteBackWire.Types.RolledBack`    | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.RolledBack`    |
|  [10]   | `WriteBackWire.Types.Indeterminate` | message | support-closure | `rasm.contracts.binding.v1.WriteBackWire.Indeterminate` |
|  [11]   | `WriteReceiptWire`                  | message | public-root     | `rasm.contracts.binding.v1.WriteReceiptWire`            |
|  [12]   | `EchoClass`                         | enum    | support-closure | `rasm.contracts.binding.v1.EchoClass`                   |

[ROSTER_SCOPE]: `rasm.contracts.clock.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME] | [KIND]  | [ORIGIN]    | [FQN]                         |
| :-----: | :----- | :------ | :---------- | :---------------------------- |
|  [01]   | `Hlc`  | message | public-root | `rasm.contracts.clock.v1.Hlc` |

[ROSTER_SCOPE]: `rasm.contracts.fault.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [ORIGIN]        | [FQN]                                      |
| :-----: | :----------------- | :------ | :-------------- | :----------------------------------------- |
|  [01]   | `FaultRecovery`    | message | support-closure | `rasm.contracts.fault.v1.FaultRecovery`    |
|  [02]   | `FaultObservation` | message | support-closure | `rasm.contracts.fault.v1.FaultObservation` |
|  [03]   | `FaultDetail`      | message | public-root     | `rasm.contracts.fault.v1.FaultDetail`      |

[ROSTER_SCOPE]: `rasm.contracts.capability.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                | [KIND]  | [ORIGIN]        | [FQN]                                                              |
| :-----: | :------------------------------------ | :------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `DescriptorPinWire`                   | message | public-root     | `rasm.contracts.capability.v1.DescriptorPinWire`                   |
|  [02]   | `CapabilityEstimate`                  | message | support-closure | `rasm.contracts.capability.v1.CapabilityEstimate`                  |
|  [03]   | `AvailableCapability`                 | message | support-closure | `rasm.contracts.capability.v1.AvailableCapability`                 |
|  [04]   | `DiscoverRequest`                     | message | public-root     | `rasm.contracts.capability.v1.DiscoverRequest`                     |
|  [05]   | `DiscoverResponse`                    | message | public-root     | `rasm.contracts.capability.v1.DiscoverResponse`                    |
|  [06]   | `CostUnit`                            | enum    | support-closure | `rasm.contracts.capability.v1.CostUnit`                            |
|  [07]   | `CapabilityDiscoveryService`          | service | support-closure | `rasm.contracts.capability.v1.CapabilityDiscoveryService`          |
|  [08]   | `CapabilityDiscoveryService.Discover` | method  | public-root     | `rasm.contracts.capability.v1.CapabilityDiscoveryService.Discover` |

[ROSTER_SCOPE]: `rasm.contracts.crdt.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]          | [KIND]  | [ORIGIN]        | [FQN]                                  |
| :-----: | :-------------- | :------ | :-------------- | :------------------------------------- |
|  [01]   | `ElementId`     | message | support-closure | `rasm.contracts.crdt.v1.ElementId`     |
|  [02]   | `VectorSlot`    | message | support-closure | `rasm.contracts.crdt.v1.VectorSlot`    |
|  [03]   | `SetOp`         | message | support-closure | `rasm.contracts.crdt.v1.SetOp`         |
|  [04]   | `WriteOp`       | message | support-closure | `rasm.contracts.crdt.v1.WriteOp`       |
|  [05]   | `AddOp`         | message | support-closure | `rasm.contracts.crdt.v1.AddOp`         |
|  [06]   | `RemoveOp`      | message | support-closure | `rasm.contracts.crdt.v1.RemoveOp`      |
|  [07]   | `IncrementOp`   | message | support-closure | `rasm.contracts.crdt.v1.IncrementOp`   |
|  [08]   | `InsertAfterOp` | message | support-closure | `rasm.contracts.crdt.v1.InsertAfterOp` |
|  [09]   | `DeleteOp`      | message | support-closure | `rasm.contracts.crdt.v1.DeleteOp`      |
|  [10]   | `MaintainOp`    | message | support-closure | `rasm.contracts.crdt.v1.MaintainOp`    |
|  [11]   | `BeatOp`        | message | support-closure | `rasm.contracts.crdt.v1.BeatOp`        |
|  [12]   | `LeaveOp`       | message | support-closure | `rasm.contracts.crdt.v1.LeaveOp`       |
|  [13]   | `CrdtOpWire`    | message | public-root     | `rasm.contracts.crdt.v1.CrdtOpWire`    |

[ROSTER_SCOPE]: `rasm.contracts.credential.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND]  | [ORIGIN]        | [FQN]                                               |
| :-----: | :--------------------- | :------ | :-------------- | :-------------------------------------------------- |
|  [01]   | `CertificateChain`     | message | support-closure | `rasm.contracts.credential.v1.CertificateChain`     |
|  [02]   | `CredentialPublicWire` | message | public-root     | `rasm.contracts.credential.v1.CredentialPublicWire` |

[ROSTER_SCOPE]: `rasm.contracts.event.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]       | [KIND]  | [ORIGIN]    | [FQN]                                |
| :-----: | :----------- | :------ | :---------- | :----------------------------------- |
|  [01]   | `Extensions` | message | public-root | `rasm.contracts.event.v1.Extensions` |

[ROSTER_SCOPE]: `rasm.contracts.fabrication.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]           | [KIND]  | [ORIGIN]        | [FQN]                                          |
| :-----: | :--------------- | :------ | :-------------- | :--------------------------------------------- |
|  [01]   | `SourceKey`      | message | support-closure | `rasm.contracts.fabrication.v1.SourceKey`      |
|  [02]   | `Datum`          | message | support-closure | `rasm.contracts.fabrication.v1.Datum`          |
|  [03]   | `Segment`        | message | support-closure | `rasm.contracts.fabrication.v1.Segment`        |
|  [04]   | `FeatureControl` | message | public-root     | `rasm.contracts.fabrication.v1.FeatureControl` |
|  [05]   | `Characteristic` | enum    | support-closure | `rasm.contracts.fabrication.v1.Characteristic` |
|  [06]   | `Scope`          | enum    | support-closure | `rasm.contracts.fabrication.v1.Scope`          |
|  [07]   | `ZoneKind`       | enum    | support-closure | `rasm.contracts.fabrication.v1.ZoneKind`       |
|  [08]   | `Modifier`       | enum    | support-closure | `rasm.contracts.fabrication.v1.Modifier`       |
|  [09]   | `Material`       | enum    | support-closure | `rasm.contracts.fabrication.v1.Material`       |
|  [10]   | `Egress`         | enum    | support-closure | `rasm.contracts.fabrication.v1.Egress`         |

[ROSTER_SCOPE]: `rasm.contracts.feature.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]  | [ORIGIN]        | [FQN]                                       |
| :-----: | :---------------- | :------ | :-------------- | :------------------------------------------ |
|  [01]   | `FlagVerdictWire` | message | public-root     | `rasm.contracts.feature.v1.FlagVerdictWire` |
|  [02]   | `FlagReason`      | enum    | support-closure | `rasm.contracts.feature.v1.FlagReason`      |

[ROSTER_SCOPE]: `rasm.contracts.organization.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [FQN]                                         |
| :-----: | :------------- | :------ | :-------------- | :-------------------------------------------- |
|  [01]   | `ViewOverride` | message | support-closure | `rasm.contracts.organization.v1.ViewOverride` |
|  [02]   | `Entity`       | message | support-closure | `rasm.contracts.organization.v1.Entity`       |
|  [03]   | `EntityPath`   | message | support-closure | `rasm.contracts.organization.v1.EntityPath`   |
|  [04]   | `Organization` | message | public-root     | `rasm.contracts.organization.v1.Organization` |

[ROSTER_SCOPE]: `rasm.contracts.parity.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [FQN]                                   |
| :-----: | :------------- | :------ | :-------------- | :-------------------------------------- |
|  [01]   | `Artifact`     | message | support-closure | `rasm.contracts.parity.v1.Artifact`     |
|  [02]   | `Capability`   | message | support-closure | `rasm.contracts.parity.v1.Capability`   |
|  [03]   | `Backend`      | message | public-root     | `rasm.contracts.parity.v1.Backend`      |
|  [04]   | `ArtifactRole` | enum    | support-closure | `rasm.contracts.parity.v1.ArtifactRole` |
|  [05]   | `Provider`     | enum    | support-closure | `rasm.contracts.parity.v1.Provider`     |
|  [06]   | `FailureRank`  | enum    | support-closure | `rasm.contracts.parity.v1.FailureRank`  |
|  [07]   | `RestartClass` | enum    | support-closure | `rasm.contracts.parity.v1.RestartClass` |

[ROSTER_SCOPE]: `rasm.contracts.render.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                        |
| :-----: | :------------------ | :------ | :-------------- | :------------------------------------------- |
|  [01]   | `SphereWire`        | message | support-closure | `rasm.contracts.render.v1.SphereWire`        |
|  [02]   | `MeshoptStream`     | message | support-closure | `rasm.contracts.render.v1.MeshoptStream`     |
|  [03]   | `Meshlet`           | message | support-closure | `rasm.contracts.render.v1.Meshlet`           |
|  [04]   | `ResidencyTileWire` | message | support-closure | `rasm.contracts.render.v1.ResidencyTileWire` |
|  [05]   | `GeometryResidency` | message | public-root     | `rasm.contracts.render.v1.GeometryResidency` |
|  [06]   | `ResidencyKind`     | enum    | support-closure | `rasm.contracts.render.v1.ResidencyKind`     |
|  [07]   | `StreamMode`        | enum    | support-closure | `rasm.contracts.render.v1.StreamMode`        |
|  [08]   | `StreamFilter`      | enum    | support-closure | `rasm.contracts.render.v1.StreamFilter`      |

[ROSTER_SCOPE]: `rasm.contracts.scan.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [FQN]                                      |
| :-----: | :------------------ | :------ | :-------------- | :----------------------------------------- |
|  [01]   | `GaussianSplatScan` | message | public-root     | `rasm.contracts.scan.v1.GaussianSplatScan` |
|  [02]   | `SplatFormat`       | enum    | support-closure | `rasm.contracts.scan.v1.SplatFormat`       |

[ROSTER_SCOPE]: `rasm.contracts.scene.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [FQN]                                             |
| :-----: | :------------------------ | :------ | :-------------- | :------------------------------------------------ |
|  [01]   | `Spectrum`                | message | support-closure | `rasm.contracts.scene.v1.Spectrum`                |
|  [02]   | `SolarFrame`              | message | support-closure | `rasm.contracts.scene.v1.SolarFrame`              |
|  [03]   | `SolarAngles`             | message | support-closure | `rasm.contracts.scene.v1.SolarAngles`             |
|  [04]   | `SitedSun`                | message | support-closure | `rasm.contracts.scene.v1.SitedSun`                |
|  [05]   | `SceneSun`                | message | support-closure | `rasm.contracts.scene.v1.SceneSun`                |
|  [06]   | `WebRef`                  | message | support-closure | `rasm.contracts.scene.v1.WebRef`                  |
|  [07]   | `Power`                   | message | support-closure | `rasm.contracts.scene.v1.Power`                   |
|  [08]   | `Cone`                    | message | support-closure | `rasm.contracts.scene.v1.Cone`                    |
|  [09]   | `Extent`                  | message | support-closure | `rasm.contracts.scene.v1.Extent`                  |
|  [10]   | `AttenuationCoefficients` | message | support-closure | `rasm.contracts.scene.v1.AttenuationCoefficients` |
|  [11]   | `Photometry`              | message | support-closure | `rasm.contracts.scene.v1.Photometry`              |
|  [12]   | `Shading`                 | message | support-closure | `rasm.contracts.scene.v1.Shading`                 |
|  [13]   | `SceneDescriptor`         | message | public-root     | `rasm.contracts.scene.v1.SceneDescriptor`         |
|  [14]   | `LightKind`               | enum    | support-closure | `rasm.contracts.scene.v1.LightKind`               |
|  [15]   | `Falloff`                 | enum    | support-closure | `rasm.contracts.scene.v1.Falloff`                 |
|  [16]   | `WebDialect`              | enum    | support-closure | `rasm.contracts.scene.v1.WebDialect`              |

[ROSTER_SCOPE]: `rasm.contracts.ui.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                          | [KIND]  | [ORIGIN]        | [FQN]                                                    |
| :-----: | :---------------------------------------------- | :------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `CommandGateWire`                               | message | public-root     | `rasm.contracts.ui.v1.CommandGateWire`                   |
|  [02]   | `CommandOutcomeWire`                            | message | support-closure | `rasm.contracts.ui.v1.CommandOutcomeWire`                |
|  [03]   | `DeckReceiptWire`                               | message | support-closure | `rasm.contracts.ui.v1.DeckReceiptWire`                   |
|  [04]   | `PixelIdentityWire`                             | message | support-closure | `rasm.contracts.ui.v1.PixelIdentityWire`                 |
|  [05]   | `NativeAssetFactWire`                           | message | support-closure | `rasm.contracts.ui.v1.NativeAssetFactWire`               |
|  [06]   | `EvidenceReceiptWire`                           | message | public-root     | `rasm.contracts.ui.v1.EvidenceReceiptWire`               |
|  [07]   | `EvidenceReceiptWire.Types.Surface`             | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Surface`       |
|  [08]   | `EvidenceReceiptWire.Types.Focus`               | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Focus`         |
|  [09]   | `EvidenceReceiptWire.Types.Render`              | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Render`        |
|  [10]   | `EvidenceReceiptWire.Types.Disposal`            | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Disposal`      |
|  [11]   | `EvidenceReceiptWire.Types.Edit`                | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Edit`          |
|  [12]   | `EvidenceReceiptWire.Types.Theme`               | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Theme`         |
|  [13]   | `EvidenceReceiptWire.Types.Motion`              | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Motion`        |
|  [14]   | `EvidenceReceiptWire.Types.Effect`              | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Effect`        |
|  [15]   | `EvidenceReceiptWire.Types.Effect.Types.Extent` | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Effect.Extent` |
|  [16]   | `EvidenceReceiptWire.Types.Asset`               | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Asset`         |
|  [17]   | `EvidenceReceiptWire.Types.LiveData`            | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.LiveData`      |
|  [18]   | `EvidenceReceiptWire.Types.CollabSync`          | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.CollabSync`    |
|  [19]   | `EvidenceReceiptWire.Types.CollabRevert`        | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.CollabRevert`  |
|  [20]   | `EvidenceReceiptWire.Types.Media`               | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Media`         |
|  [21]   | `EvidenceReceiptWire.Types.Quality`             | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Quality`       |
|  [22]   | `EvidenceReceiptWire.Types.GpuFrame`            | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.GpuFrame`      |
|  [23]   | `EvidenceReceiptWire.Types.Layout`              | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.Layout`        |
|  [24]   | `EvidenceReceiptWire.Types.DispatcherLag`       | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.DispatcherLag` |
|  [25]   | `EvidenceReceiptWire.Types.PreCommit`           | message | support-closure | `rasm.contracts.ui.v1.EvidenceReceiptWire.PreCommit`     |
|  [26]   | `SkewBandWire`                                  | message | support-closure | `rasm.contracts.ui.v1.SkewBandWire`                      |
|  [27]   | `EvidenceRowWire`                               | message | support-closure | `rasm.contracts.ui.v1.EvidenceRowWire`                   |
|  [28]   | `EvidenceTimelineWire`                          | message | public-root     | `rasm.contracts.ui.v1.EvidenceTimelineWire`              |
|  [29]   | `PixelLayout`                                   | enum    | support-closure | `rasm.contracts.ui.v1.PixelLayout`                       |
|  [30]   | `MediaOutcome`                                  | enum    | support-closure | `rasm.contracts.ui.v1.MediaOutcome`                      |
|  [31]   | `AppUiSurfaceProgram`                           | message | public-root     | `rasm.contracts.ui.v1.AppUiSurfaceProgram`               |
<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root generation rewrites the generated tree and roster markers from one descriptor image.
- Managed package identity derives `Rasm.Contracts.<Family>.V1`; selective roots omit publisher-owned packages from both C# plugins.
- `Directory.Build.props` isolates the assembly from workspace references and analyzers while central package versions remain active.
- Embedded descriptors carry corpus validation options into `Celly.Protovalidate.Validator`.

[STACKING]:
- `Google.Protobuf`(`../../.api/api-protobuf.md`): `<Msg>.Parser` and `<Msg>.Descriptor` drive bounded decode, encode, JSON, and registry operations.
- `Grpc.Core.Api`(`../../.api/api-grpc-core-api.md`): `<Svc>.<Svc>Base` binds servers and `<Svc>.<Svc>Client` binds a `CallInvoker`.
- `Google.Api.CommonProtos`(`../../.api/api-commonprotos.md`): imported `google.rpc` and `google.type` descriptors resolve generated field types.
- `Celly.Protovalidate`(`../../.api/api-celly-protovalidate.md`): `Validator.Validate(IMessage)` evaluates embedded rules at consumer admission.
- Consumer admission owners convert generated values into domain values after bounded parsing and validation.

[LOCAL_ADMISSION]:
- Workspace consumers import through one `ProjectReference`; unrelated applications import the same assembly through one versioned `PackageReference`.
- Corpus and generator changes regenerate the entire tree; package code adds no partials, helpers, or copied descriptors.

[RAIL_LAW]:
- Package: `Rasm.Contracts`
- Owns: Generated C# messages, enums, descriptors, service bases, and clients for selected corpus packages
- Accept: Project- or package-referenced generated symbols validated and admitted at their consuming boundary
- Reject: Consumer-local generation, hand-authored bindings, partial extensions, descriptor mirrors, and hand-kept roster rows
