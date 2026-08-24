# [CONTRACTS_API_DOTNET]

`Rasm.Contracts` owns the committed generated C# symbol plane: messages, enums, reflection descriptors, service bases, and clients share one assembly consumed by project in the workspace and by versioned NuGet package outside it. Corpus generation authors every public member.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rasm.Contracts`
- package: `Rasm.Contracts`
- version: MinVer derivation from the repository's `v*` tags, never a hand-kept literal
- assembly: `Rasm.Contracts.dll`
- namespace: `Rasm.Contracts.<Family>`
- depends: `Celly.Protovalidate`, `Google.Api.CommonProtos`, `Google.Protobuf`, `Grpc.Core.Api`
- role: Generated-only assembly with publisher-owned package types omitted from its public roots
- rail: Project-referenced workspace symbols and package-referenced external symbols over the same embedded descriptors

## [02]-[SYMBOL_GRAMMAR]

[SYMBOL_GRAMMAR_SCOPE]: One proto declaration's generated C# correspondence

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Rasm.Contracts.<F>.<Msg>`           | class         | generated `IMessage<T>` value                       |
|  [02]   | `<Msg>.Parser`                       | property      | typed binary and stream parser                      |
|  [03]   | `<Msg>.Descriptor`                   | property      | generated message descriptor                        |
|  [04]   | `<File>Reflection.Descriptor`        | property      | generated file descriptor and imported option graph |
|  [05]   | `Rasm.Contracts.<F>.<Enum>`          | enum          | generated closed numeric vocabulary                 |
|  [06]   | `<Msg>.<Oneof>OneofCase`             | enum          | generated oneof discriminant                        |
|  [07]   | `<Msg>.Has<Field>`/`.Clear<Field>()` | property pair | optional scalar and enum presence access            |
|  [08]   | `<Msg>.Types.<Nested>`               | class         | nested generated declaration                        |
|  [09]   | `<Svc>.<Svc>Base`                    | class         | generated server override surface                   |
|  [10]   | `<Svc>.<Svc>Client`                  | class         | generated unary and streaming client surface        |

[SYMBOL_TRAPS]:
- `capability.AvailableCapability` emits `string descriptor = 1` as `Descriptor_` — protoc mangles a field colliding with the static `Descriptor`.
- Fences bind the GENERATED spelling — a hand-written `Descriptor` binds the static message descriptor, never the field value.

## [03]-[ROSTER]

<!-- roster:begin -->
[ROSTER_SCOPE]: `rasm.contracts.artifact` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [SYMBOL]                |
| :-----: | :---------------------- | :------ | :-------------- | :---------------------- |
|  [01]   | `ArtifactRef`           | message | support-closure | `ArtifactRef`           |
|  [02]   | `ArtifactFrame`         | message | support-closure | `ArtifactFrame`         |
|  [03]   | `FetchRequest`          | message | public-root     | `FetchRequest`          |
|  [04]   | `FetchResponse`         | message | public-root     | `FetchResponse`         |
|  [05]   | `PutRequest`            | message | public-root     | `PutRequest`            |
|  [06]   | `PutResponse`           | message | public-root     | `PutResponse`           |
|  [07]   | `ArtifactService`       | service | support-closure | `ArtifactService`       |
|  [08]   | `ArtifactService.Fetch` | method  | public-root     | `ArtifactService.Fetch` |
|  [09]   | `ArtifactService.Put`   | method  | public-root     | `ArtifactService.Put`   |

[ROSTER_SCOPE]: `rasm.contracts.appearance` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                            | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `Provenance`                                      | message | support-closure | `Provenance`                          |
|  [02]   | `Provenance.Types.Capture`                        | message | support-closure | `Provenance.Capture`                  |
|  [03]   | `Provenance.Types.Fit`                            | message | support-closure | `Provenance.Fit`                      |
|  [04]   | `Provenance.Types.Inference`                      | message | support-closure | `Provenance.Inference`                |
|  [05]   | `Provenance.Types.Chromaticity`                   | message | support-closure | `Provenance.Chromaticity`             |
|  [06]   | `Provenance.Types.Chromaticity.Types.Dominance`   | message | support-closure | `Provenance.Chromaticity.Dominance`   |
|  [07]   | `Provenance.Types.Chromaticity.Types.Temperature` | message | support-closure | `Provenance.Chromaticity.Temperature` |
|  [08]   | `Provenance.Types.Card`                           | message | support-closure | `Provenance.Card`                     |
|  [09]   | `Provenance.Types.Ingest`                         | message | support-closure | `Provenance.Ingest`                   |
|  [10]   | `Press`                                           | message | support-closure | `Press`                               |
|  [11]   | `SurfaceSet`                                      | message | support-closure | `SurfaceSet`                          |
|  [12]   | `BakedSet`                                        | message | support-closure | `BakedSet`                            |
|  [13]   | `EnvironmentSet`                                  | message | support-closure | `EnvironmentSet`                      |
|  [14]   | `Set`                                             | message | public-root     | `Set`                                 |
|  [15]   | `Color`                                           | message | support-closure | `Color`                               |
|  [16]   | `OpenPbr`                                         | message | support-closure | `OpenPbr`                             |
|  [17]   | `EmissionReadout`                                 | message | support-closure | `EmissionReadout`                     |
|  [18]   | `Emission`                                        | message | support-closure | `Emission`                            |
|  [19]   | `Material`                                        | message | public-root     | `Material`                            |

[ROSTER_SCOPE]: `rasm.contracts.compute` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                          | [KIND]  | [ORIGIN]        | [SYMBOL]                        |
| :-----: | :------------------------------ | :------ | :-------------- | :------------------------------ |
|  [01]   | `SetDegradationRequest`         | message | public-root     | `SetDegradationRequest`         |
|  [02]   | `SetDegradationResponse`        | message | public-root     | `SetDegradationResponse`        |
|  [03]   | `DrainRuntimeRequest`           | message | public-root     | `DrainRuntimeRequest`           |
|  [04]   | `DrainStep`                     | message | support-closure | `DrainStep`                     |
|  [05]   | `DrainRuntimeResponse`          | message | public-root     | `DrainRuntimeResponse`          |
|  [06]   | `DegradationLevel`              | enum    | support-closure | `DegradationLevel`              |
|  [07]   | `DrainBand`                     | enum    | support-closure | `DrainBand`                     |
|  [08]   | `DeadlineOutcome`               | enum    | support-closure | `DeadlineOutcome`               |
|  [09]   | `RuntimePhase`                  | enum    | support-closure | `RuntimePhase`                  |
|  [10]   | `ElementScope`                  | message | support-closure | `ElementScope`                  |
|  [11]   | `EntityScope`                   | message | support-closure | `EntityScope`                   |
|  [12]   | `TessellationScope`             | message | support-closure | `TessellationScope`             |
|  [13]   | `TessellateRequest`             | message | public-root     | `TessellateRequest`             |
|  [14]   | `Semantic`                      | message | support-closure | `Semantic`                      |
|  [15]   | `TessellateResponse`            | message | public-root     | `TessellateResponse`            |
|  [16]   | `Spill`                         | enum    | support-closure | `Spill`                         |
|  [17]   | `GeomSetting`                   | enum    | support-closure | `GeomSetting`                   |
|  [18]   | `Dimensionality`                | enum    | support-closure | `Dimensionality`                |
|  [19]   | `ControlService`                | service | support-closure | `ControlService`                |
|  [20]   | `ControlService.SetDegradation` | method  | public-root     | `ControlService.SetDegradation` |
|  [21]   | `ControlService.DrainRuntime`   | method  | public-root     | `ControlService.DrainRuntime`   |
|  [22]   | `ComputeService`                | service | support-closure | `ComputeService`                |
|  [23]   | `ComputeService.Tessellate`     | method  | public-root     | `ComputeService.Tessellate`     |

[ROSTER_SCOPE]: `rasm.contracts.availability` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                      |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------- |
|  [01]   | `CommandVerdictWire`                | message | support-closure | `CommandVerdictWire`          |
|  [02]   | `CommandVerdictWire.Types.Gated`    | message | support-closure | `CommandVerdictWire.Gated`    |
|  [03]   | `CommandVerdictWire.Types.Withheld` | message | support-closure | `CommandVerdictWire.Withheld` |
|  [04]   | `CommandAvailability`               | message | public-root     | `CommandAvailability`         |

[ROSTER_SCOPE]: `rasm.contracts.bcf` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `BcfCameraWire`     | message | support-closure | `BcfCameraWire`     |
|  [02]   | `BcfColoringWire`   | message | support-closure | `BcfColoringWire`   |
|  [03]   | `BcfLineWire`       | message | support-closure | `BcfLineWire`       |
|  [04]   | `BcfClippingWire`   | message | support-closure | `BcfClippingWire`   |
|  [05]   | `BcfBitmapWire`     | message | support-closure | `BcfBitmapWire`     |
|  [06]   | `BcfSnippetWire`    | message | support-closure | `BcfSnippetWire`    |
|  [07]   | `BcfDocumentWire`   | message | support-closure | `BcfDocumentWire`   |
|  [08]   | `BcfFileWire`       | message | support-closure | `BcfFileWire`       |
|  [09]   | `BcfHintsWire`      | message | support-closure | `BcfHintsWire`      |
|  [10]   | `BcfShowingWire`    | message | support-closure | `BcfShowingWire`    |
|  [11]   | `BcfHidingWire`     | message | support-closure | `BcfHidingWire`     |
|  [12]   | `BcfVisibilityWire` | message | support-closure | `BcfVisibilityWire` |
|  [13]   | `BcfViewpointWire`  | message | public-root     | `BcfViewpointWire`  |
|  [14]   | `BcfCommentWire`    | message | support-closure | `BcfCommentWire`    |
|  [15]   | `BcfTopicWire`      | message | public-root     | `BcfTopicWire`      |
|  [16]   | `BcfStatus`         | enum    | support-closure | `BcfStatus`         |
|  [17]   | `BcfBitmapFormat`   | enum    | support-closure | `BcfBitmapFormat`   |

[ROSTER_SCOPE]: `rasm.contracts.benchmark` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                | [KIND]  | [ORIGIN]        | [SYMBOL]              |
| :-----: | :-------------------- | :------ | :-------------- | :-------------------- |
|  [01]   | `BenchInputWire`      | message | support-closure | `BenchInputWire`      |
|  [02]   | `ChromeTraceWire`     | message | support-closure | `ChromeTraceWire`     |
|  [03]   | `BenchmarkExportWire` | message | support-closure | `BenchmarkExportWire` |
|  [04]   | `EpContextWire`       | message | support-closure | `EpContextWire`       |
|  [05]   | `ProfileArtifactWire` | message | support-closure | `ProfileArtifactWire` |
|  [06]   | `BenchKernelWire`     | message | support-closure | `BenchKernelWire`     |
|  [07]   | `BenchAggregate`      | message | support-closure | `BenchAggregate`      |
|  [08]   | `RungCell`            | message | support-closure | `RungCell`            |
|  [09]   | `BenchBandWire`       | message | support-closure | `BenchBandWire`       |
|  [10]   | `BenchMetric`         | message | support-closure | `BenchMetric`         |
|  [11]   | `BenchmarkClaimWire`  | message | public-root     | `BenchmarkClaimWire`  |
|  [12]   | `BenchModality`       | enum    | support-closure | `BenchModality`       |
|  [13]   | `BenchPolarity`       | enum    | support-closure | `BenchPolarity`       |
|  [14]   | `PayloadBand`         | enum    | support-closure | `PayloadBand`         |
|  [15]   | `BenchRung`           | enum    | support-closure | `BenchRung`           |

[ROSTER_SCOPE]: `rasm.contracts.declaration` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `ImpactCell`        | message | support-closure | `ImpactCell`        |
|  [02]   | `Source`            | message | support-closure | `Source`            |
|  [03]   | `DeclarationRecord` | message | public-root     | `DeclarationRecord` |
|  [04]   | `Registry`          | enum    | support-closure | `Registry`          |
|  [05]   | `DeclaredUnit`      | enum    | support-closure | `DeclaredUnit`      |
|  [06]   | `Standard`          | enum    | support-closure | `Standard`          |
|  [07]   | `Subtype`           | enum    | support-closure | `Subtype`           |
|  [08]   | `ImpactCategory`    | enum    | support-closure | `ImpactCategory`    |
|  [09]   | `Module`            | enum    | support-closure | `Module`            |

[ROSTER_SCOPE]: `rasm.contracts.element` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]             |
| :-----: | :------------------- | :------ | :-------------- | :------------------- |
|  [01]   | `ClassificationWire` | message | support-closure | `ClassificationWire` |
|  [02]   | `OwnerHistoryWire`   | message | support-closure | `OwnerHistoryWire`   |
|  [03]   | `SchemaSpanWire`     | message | support-closure | `SchemaSpanWire`     |
|  [04]   | `PlacementWire`      | message | support-closure | `PlacementWire`      |
|  [05]   | `RepresentationWire` | message | support-closure | `RepresentationWire` |
|  [06]   | `ObjectWire`         | message | support-closure | `ObjectWire`         |
|  [07]   | `PropertySetWire`    | message | support-closure | `PropertySetWire`    |
|  [08]   | `GroupIdentityWire`  | message | support-closure | `GroupIdentityWire`  |
|  [09]   | `GroupWire`          | message | support-closure | `GroupWire`          |
|  [10]   | `QuantitySetWire`    | message | support-closure | `QuantitySetWire`    |
|  [11]   | `AppearanceWire`     | message | support-closure | `AppearanceWire`     |
|  [12]   | `NodeWire`           | message | public-root     | `NodeWire`           |
|  [13]   | `ObjectKind`         | enum    | support-closure | `ObjectKind`         |
|  [14]   | `ReleaseVersion`     | enum    | support-closure | `ReleaseVersion`     |
|  [15]   | `ChangeAction`       | enum    | support-closure | `ChangeAction`       |
|  [16]   | `ObjectState`        | enum    | support-closure | `ObjectState`        |
|  [17]   | `RepresentationKind` | enum    | support-closure | `RepresentationKind` |
|  [18]   | `EditTombstone`      | message | support-closure | `EditTombstone`      |
|  [19]   | `EditMembers`        | message | support-closure | `EditMembers`        |
|  [20]   | `EntityEditWire`     | message | public-root     | `EntityEditWire`     |

[ROSTER_SCOPE]: `rasm.contracts.bim` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `DeltaValueWire`    | message | support-closure | `DeltaValueWire`    |
|  [02]   | `AspectDeltaWire`   | message | support-closure | `AspectDeltaWire`   |
|  [03]   | `DiffEndWire`       | message | support-closure | `DiffEndWire`       |
|  [04]   | `DiffModifiedWire`  | message | support-closure | `DiffModifiedWire`  |
|  [05]   | `DiffMovedWire`     | message | support-closure | `DiffMovedWire`     |
|  [06]   | `DiffRegroupWire`   | message | support-closure | `DiffRegroupWire`   |
|  [07]   | `ElementChangeWire` | message | support-closure | `ElementChangeWire` |
|  [08]   | `ModelDiffWire`     | message | public-root     | `ModelDiffWire`     |
|  [09]   | `DeltaShape`        | enum    | support-closure | `DeltaShape`        |

[ROSTER_SCOPE]: `rasm.contracts.binding` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                      |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------- |
|  [01]   | `BindingStatus`                     | message | public-root     | `BindingStatus`               |
|  [02]   | `ExternalTransport`                 | enum    | support-closure | `ExternalTransport`           |
|  [03]   | `BindingState`                      | enum    | support-closure | `BindingState`                |
|  [04]   | `BindingDirection`                  | enum    | support-closure | `BindingDirection`            |
|  [05]   | `CoercedValueWire`                  | message | public-root     | `CoercedValueWire`            |
|  [06]   | `WriteBackWire`                     | message | support-closure | `WriteBackWire`               |
|  [07]   | `WriteBackWire.Types.Acknowledged`  | message | support-closure | `WriteBackWire.Acknowledged`  |
|  [08]   | `WriteBackWire.Types.Rejected`      | message | support-closure | `WriteBackWire.Rejected`      |
|  [09]   | `WriteBackWire.Types.RolledBack`    | message | support-closure | `WriteBackWire.RolledBack`    |
|  [10]   | `WriteBackWire.Types.Indeterminate` | message | support-closure | `WriteBackWire.Indeterminate` |
|  [11]   | `WriteReceiptWire`                  | message | public-root     | `WriteReceiptWire`            |
|  [12]   | `EchoClass`                         | enum    | support-closure | `EchoClass`                   |

[ROSTER_SCOPE]: `rasm.contracts.clock` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME] | [KIND]  | [ORIGIN]    | [SYMBOL] |
| :-----: | :----- | :------ | :---------- | :------- |
|  [01]   | `Hlc`  | message | public-root | `Hlc`    |

[ROSTER_SCOPE]: `rasm.contracts.fault` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [ORIGIN]        | [SYMBOL]           |
| :-----: | :----------------- | :------ | :-------------- | :----------------- |
|  [01]   | `FaultRecovery`    | message | support-closure | `FaultRecovery`    |
|  [02]   | `FaultObservation` | message | support-closure | `FaultObservation` |
|  [03]   | `FaultDetail`      | message | public-root     | `FaultDetail`      |

[ROSTER_SCOPE]: `rasm.contracts.capability` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `DescriptorPinWire`                   | message | public-root     | `DescriptorPinWire`                   |
|  [02]   | `CapabilityEstimate`                  | message | support-closure | `CapabilityEstimate`                  |
|  [03]   | `AvailableCapability`                 | message | support-closure | `AvailableCapability`                 |
|  [04]   | `DiscoverRequest`                     | message | public-root     | `DiscoverRequest`                     |
|  [05]   | `DiscoverResponse`                    | message | public-root     | `DiscoverResponse`                    |
|  [06]   | `CostUnit`                            | enum    | support-closure | `CostUnit`                            |
|  [07]   | `CapabilityDiscoveryService`          | service | support-closure | `CapabilityDiscoveryService`          |
|  [08]   | `CapabilityDiscoveryService.Discover` | method  | public-root     | `CapabilityDiscoveryService.Discover` |

[ROSTER_SCOPE]: `rasm.contracts.crdt` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]          | [KIND]  | [ORIGIN]        | [SYMBOL]        |
| :-----: | :-------------- | :------ | :-------------- | :-------------- |
|  [01]   | `ElementId`     | message | support-closure | `ElementId`     |
|  [02]   | `VectorSlot`    | message | support-closure | `VectorSlot`    |
|  [03]   | `SetOp`         | message | support-closure | `SetOp`         |
|  [04]   | `WriteOp`       | message | support-closure | `WriteOp`       |
|  [05]   | `AddOp`         | message | support-closure | `AddOp`         |
|  [06]   | `RemoveOp`      | message | support-closure | `RemoveOp`      |
|  [07]   | `IncrementOp`   | message | support-closure | `IncrementOp`   |
|  [08]   | `InsertAfterOp` | message | support-closure | `InsertAfterOp` |
|  [09]   | `DeleteOp`      | message | support-closure | `DeleteOp`      |
|  [10]   | `MaintainOp`    | message | support-closure | `MaintainOp`    |
|  [11]   | `BeatOp`        | message | support-closure | `BeatOp`        |
|  [12]   | `LeaveOp`       | message | support-closure | `LeaveOp`       |
|  [13]   | `CrdtOpWire`    | message | public-root     | `CrdtOpWire`    |

[ROSTER_SCOPE]: `rasm.contracts.credential` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                 | [KIND]  | [ORIGIN]        | [SYMBOL]               |
| :-----: | :--------------------- | :------ | :-------------- | :--------------------- |
|  [01]   | `CertificateChain`     | message | support-closure | `CertificateChain`     |
|  [02]   | `CredentialPublicWire` | message | public-root     | `CredentialPublicWire` |

[ROSTER_SCOPE]: `rasm.contracts.event` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]       | [KIND]  | [ORIGIN]    | [SYMBOL]     |
| :-----: | :----------- | :------ | :---------- | :----------- |
|  [01]   | `Extensions` | message | public-root | `Extensions` |

[ROSTER_SCOPE]: `rasm.contracts.fabrication` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]           | [KIND]  | [ORIGIN]        | [SYMBOL]         |
| :-----: | :--------------- | :------ | :-------------- | :--------------- |
|  [01]   | `SourceKey`      | message | support-closure | `SourceKey`      |
|  [02]   | `Datum`          | message | support-closure | `Datum`          |
|  [03]   | `Segment`        | message | support-closure | `Segment`        |
|  [04]   | `FeatureControl` | message | public-root     | `FeatureControl` |
|  [05]   | `Characteristic` | enum    | support-closure | `Characteristic` |
|  [06]   | `Scope`          | enum    | support-closure | `Scope`          |
|  [07]   | `ZoneKind`       | enum    | support-closure | `ZoneKind`       |
|  [08]   | `Modifier`       | enum    | support-closure | `Modifier`       |
|  [09]   | `Material`       | enum    | support-closure | `Material`       |
|  [10]   | `Egress`         | enum    | support-closure | `Egress`         |

[ROSTER_SCOPE]: `rasm.contracts.feature` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]            | [KIND]  | [ORIGIN]        | [SYMBOL]          |
| :-----: | :---------------- | :------ | :-------------- | :---------------- |
|  [01]   | `FlagVerdictWire` | message | public-root     | `FlagVerdictWire` |
|  [02]   | `FlagReason`      | enum    | support-closure | `FlagReason`      |

[ROSTER_SCOPE]: `rasm.contracts.organization` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------- | :------ | :-------------- | :------------- |
|  [01]   | `ViewOverride` | message | support-closure | `ViewOverride` |
|  [02]   | `Entity`       | message | support-closure | `Entity`       |
|  [03]   | `EntityPath`   | message | support-closure | `EntityPath`   |
|  [04]   | `Organization` | message | public-root     | `Organization` |

[ROSTER_SCOPE]: `rasm.contracts.parity` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------- | :------ | :-------------- | :------------- |
|  [01]   | `Artifact`     | message | support-closure | `Artifact`     |
|  [02]   | `Capability`   | message | support-closure | `Capability`   |
|  [03]   | `Backend`      | message | public-root     | `Backend`      |
|  [04]   | `ArtifactRole` | enum    | support-closure | `ArtifactRole` |
|  [05]   | `Provider`     | enum    | support-closure | `Provider`     |
|  [06]   | `FailureRank`  | enum    | support-closure | `FailureRank`  |
|  [07]   | `RestartClass` | enum    | support-closure | `RestartClass` |

[ROSTER_SCOPE]: `rasm.contracts.render` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `SphereWire`        | message | support-closure | `SphereWire`        |
|  [02]   | `MeshoptStream`     | message | support-closure | `MeshoptStream`     |
|  [03]   | `Meshlet`           | message | support-closure | `Meshlet`           |
|  [04]   | `ResidencyTileWire` | message | support-closure | `ResidencyTileWire` |
|  [05]   | `GeometryResidency` | message | public-root     | `GeometryResidency` |
|  [06]   | `ResidencyKind`     | enum    | support-closure | `ResidencyKind`     |
|  [07]   | `StreamMode`        | enum    | support-closure | `StreamMode`        |
|  [08]   | `StreamFilter`      | enum    | support-closure | `StreamFilter`      |

[ROSTER_SCOPE]: `rasm.contracts.scan` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `GaussianSplatScan` | message | public-root     | `GaussianSplatScan` |
|  [02]   | `SplatFormat`       | enum    | support-closure | `SplatFormat`       |

[ROSTER_SCOPE]: `rasm.contracts.scene` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                    | [KIND]  | [ORIGIN]        | [SYMBOL]                  |
| :-----: | :------------------------ | :------ | :-------------- | :------------------------ |
|  [01]   | `Spectrum`                | message | support-closure | `Spectrum`                |
|  [02]   | `SolarFrame`              | message | support-closure | `SolarFrame`              |
|  [03]   | `SolarAngles`             | message | support-closure | `SolarAngles`             |
|  [04]   | `SitedSun`                | message | support-closure | `SitedSun`                |
|  [05]   | `SceneSun`                | message | support-closure | `SceneSun`                |
|  [06]   | `WebRef`                  | message | support-closure | `WebRef`                  |
|  [07]   | `Power`                   | message | support-closure | `Power`                   |
|  [08]   | `Cone`                    | message | support-closure | `Cone`                    |
|  [09]   | `Extent`                  | message | support-closure | `Extent`                  |
|  [10]   | `AttenuationCoefficients` | message | support-closure | `AttenuationCoefficients` |
|  [11]   | `Photometry`              | message | support-closure | `Photometry`              |
|  [12]   | `Shading`                 | message | support-closure | `Shading`                 |
|  [13]   | `SceneDescriptor`         | message | public-root     | `SceneDescriptor`         |
|  [14]   | `LightKind`               | enum    | support-closure | `LightKind`               |
|  [15]   | `Falloff`                 | enum    | support-closure | `Falloff`                 |
|  [16]   | `WebDialect`              | enum    | support-closure | `WebDialect`              |

[ROSTER_SCOPE]: `rasm.contracts.ui` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                          | [KIND]  | [ORIGIN]        | [SYMBOL]                            |
| :-----: | :---------------------------------------------- | :------ | :-------------- | :---------------------------------- |
|  [01]   | `CommandGateWire`                               | message | public-root     | `CommandGateWire`                   |
|  [02]   | `CommandOutcomeWire`                            | message | support-closure | `CommandOutcomeWire`                |
|  [03]   | `DeckReceiptWire`                               | message | support-closure | `DeckReceiptWire`                   |
|  [04]   | `PixelIdentityWire`                             | message | support-closure | `PixelIdentityWire`                 |
|  [05]   | `NativeAssetFactWire`                           | message | support-closure | `NativeAssetFactWire`               |
|  [06]   | `EvidenceReceiptWire`                           | message | public-root     | `EvidenceReceiptWire`               |
|  [07]   | `EvidenceReceiptWire.Types.Surface`             | message | support-closure | `EvidenceReceiptWire.Surface`       |
|  [08]   | `EvidenceReceiptWire.Types.Focus`               | message | support-closure | `EvidenceReceiptWire.Focus`         |
|  [09]   | `EvidenceReceiptWire.Types.Render`              | message | support-closure | `EvidenceReceiptWire.Render`        |
|  [10]   | `EvidenceReceiptWire.Types.Disposal`            | message | support-closure | `EvidenceReceiptWire.Disposal`      |
|  [11]   | `EvidenceReceiptWire.Types.Edit`                | message | support-closure | `EvidenceReceiptWire.Edit`          |
|  [12]   | `EvidenceReceiptWire.Types.Theme`               | message | support-closure | `EvidenceReceiptWire.Theme`         |
|  [13]   | `EvidenceReceiptWire.Types.Motion`              | message | support-closure | `EvidenceReceiptWire.Motion`        |
|  [14]   | `EvidenceReceiptWire.Types.Effect`              | message | support-closure | `EvidenceReceiptWire.Effect`        |
|  [15]   | `EvidenceReceiptWire.Types.Effect.Types.Extent` | message | support-closure | `EvidenceReceiptWire.Effect.Extent` |
|  [16]   | `EvidenceReceiptWire.Types.Asset`               | message | support-closure | `EvidenceReceiptWire.Asset`         |
|  [17]   | `EvidenceReceiptWire.Types.LiveData`            | message | support-closure | `EvidenceReceiptWire.LiveData`      |
|  [18]   | `EvidenceReceiptWire.Types.CollabSync`          | message | support-closure | `EvidenceReceiptWire.CollabSync`    |
|  [19]   | `EvidenceReceiptWire.Types.CollabRevert`        | message | support-closure | `EvidenceReceiptWire.CollabRevert`  |
|  [20]   | `EvidenceReceiptWire.Types.Media`               | message | support-closure | `EvidenceReceiptWire.Media`         |
|  [21]   | `EvidenceReceiptWire.Types.Quality`             | message | support-closure | `EvidenceReceiptWire.Quality`       |
|  [22]   | `EvidenceReceiptWire.Types.GpuFrame`            | message | support-closure | `EvidenceReceiptWire.GpuFrame`      |
|  [23]   | `EvidenceReceiptWire.Types.Layout`              | message | support-closure | `EvidenceReceiptWire.Layout`        |
|  [24]   | `EvidenceReceiptWire.Types.DispatcherLag`       | message | support-closure | `EvidenceReceiptWire.DispatcherLag` |
|  [25]   | `EvidenceReceiptWire.Types.PreCommit`           | message | support-closure | `EvidenceReceiptWire.PreCommit`     |
|  [26]   | `SkewBandWire`                                  | message | support-closure | `SkewBandWire`                      |
|  [27]   | `EvidenceRowWire`                               | message | support-closure | `EvidenceRowWire`                   |
|  [28]   | `EvidenceTimelineWire`                          | message | public-root     | `EvidenceTimelineWire`              |
|  [29]   | `PixelLayout`                                   | enum    | support-closure | `PixelLayout`                       |
|  [30]   | `MediaOutcome`                                  | enum    | support-closure | `MediaOutcome`                      |
|  [31]   | `AppUiSurfaceProgram`                           | message | public-root     | `AppUiSurfaceProgram`               |

<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `libs/contracts/RULINGS.md` `[04]-[STRUCTURE]` owns the sweep boundary, the roster-marker derivation, and the generated-only compile.
- `gen/dotnet/<Family>/` is the out root: `base_namespace=Rasm.Contracts` strips the managed namespace prefix and the tail names the directory.
- Embedded descriptors carry corpus validation options into `Celly.Protovalidate.Validator`.

[STACKING]:
- `Google.Protobuf`(`../../dotnet/.api/api-protobuf.md`): `<Msg>.Parser` and `<Msg>.Descriptor` drive bounded decode, encode, JSON, and registries.
- `Grpc.Core.Api`(`../../dotnet/.api/api-grpc-core-api.md`): `<Svc>.<Svc>Base` binds servers and `<Svc>.<Svc>Client` binds a `CallInvoker`.
- `Google.Api.CommonProtos`(`../../dotnet/.api/api-commonprotos.md`): imported `google.rpc` and `google.type` descriptors resolve generated fields.
- `Celly.Protovalidate`(`../../dotnet/.api/api-celly-protovalidate.md`): `Validator.Validate(IMessage)` evaluates embedded rules at admission.
- Consumer admission owners convert generated values into domain values after bounded parsing and validation.

[LOCAL_ADMISSION]:
- Workspace consumers import through one `ProjectReference`; unrelated applications import the same assembly through one versioned `PackageReference`.
- Corpus and generator changes regenerate the entire tree; package code adds no partials, helpers, or copied descriptors.

[RAIL_LAW]:
- Package: `Rasm.Contracts`
- Owns: Generated C# messages, enums, descriptors, service bases, and clients for selected corpus packages
- Accept: Project- or package-referenced generated symbols validated and admitted at their consuming boundary
- Reject: Consumer-local generation, hand-authored bindings, partial extensions, descriptor mirrors, and hand-kept roster rows
