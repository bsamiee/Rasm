# [PY_CONTRACTS_API_RASM_CONTRACTS]

`rasm.contracts` owns generated Python bindings, generic Connect body admission, verified artifact transfer, and exact resources across collision-safe estate and publisher roots.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasm.contracts`
- package: `rasm.contracts`
- module: `rasm.contracts.admission`, `rasm.contracts.artifact`, `rasm.contracts.gen`, `rasm.contracts.vendor`
- namespaces: estate `rasm.contracts.gen.rasm.contracts.<family>.v1`; publisher `rasm.contracts.vendor.<publisher package>`
- abi: pure Python wheel; PEP 420 `rasm` namespace with typed `rasm.contracts` package boundary
- depends: `anyio`, `protobuf-py`, `connectrpc`, `protovalidate`, `expression`
- role: module root the `rasm-contracts` workspace member builds and installs
- rail: generated messages, descriptors, service protocols, applications, clients, body admission, artifact transfer, and publisher resources

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one proto declaration's generated Python correspondence

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `gen.rasm.contracts.<f>.v1.<s>_pb.<Msg>`      | class         | typed `Message[Literal[fields]]` value           |
|  [02]   | `<s>_pb.<Outer>.<Inner>`                      | class         | nested generated message                         |
|  [03]   | `gen.rasm.contracts.<f>.v1.<s>_pb.<Enum>`     | class         | generated enumeration and exact wire numbers     |
|  [04]   | `<s>_pb.desc()`                               | static        | generated `DescFile` and dependency graph        |
|  [05]   | `gen.rasm.contracts.<f>.v1.<s>_connect.<Svc>` | protocol      | asynchronous handler surface                     |
|  [06]   | `<s>_connect.<Svc>ASGIApplication`            | class         | generated service application and endpoint set   |
|  [07]   | `<s>_connect.<Svc>Client`                     | class         | generated typed asynchronous client              |
|  [08]   | `gen.{buf,google}.<path>.<s>_pb.<Msg>`        | class         | reachable support declaration                    |
|  [09]   | `vendor.<publisher>.<s>_pb.<Msg>`             | class         | publisher-owned generated declaration            |
|  [10]   | `files("rasm.contracts").joinpath(<path>)`    | resource      | exact manifest-projected publisher bytes         |
|  [11]   | `AdmissionSide`                               | enum          | client or server trust-boundary posture          |
|  [12]   | `AdmissionPhase`                              | enum          | request or response refusal direction            |
|  [13]   | `AdmissionError`                              | exception     | client refusal with phase, cause, and violations |
|  [14]   | `BodyAdmission`                               | interceptor   | all four asynchronous Connect body shapes        |
|  [15]   | `AsyncClosable`                               | protocol      | streamed body releasing its source on early exit |
|  [16]   | `ArtifactLaw`                                 | policy        | descriptor-read frame, extent, identity bounds   |
|  [17]   | `ArtifactRefusal`                             | union         | closed artifact law, each case carrying evidence |
|  [18]   | `ArtifactError`                               | exception     | egress raise reconstructing one railed refusal   |
|  [19]   | `OwnedArtifact`                               | value         | verified reference and helper-owned path         |
|  [20]   | `ArtifactCustody`                             | union         | closed open-or-sealed spool lifecycle            |
|  [21]   | `ArtifactSink`                                | lifecycle     | single-use spool and its one folding seal        |
|  [22]   | `ArtifactStream`                              | stream        | envelope-parameterized sealed-artifact emission  |
|  [23]   | `ArtifactTransfer`                            | client        | generated Fetch and Put lifecycle composition    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: authored admission boundary, generated public roots, and reachable support closure

[ADMISSION_SCOPE]: generic descriptor-driven body admission

| [INDEX] | [SURFACE]                                     | [SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------------- | :---------- | :--------------------------------------------------------------- |
|  [01]   | `BodyAdmission(AdmissionSide.CLIENT)`         | interceptor | validates requests before encode and responses after decode      |
|  [02]   | `BodyAdmission(AdmissionSide.SERVER)`         | interceptor | validates requests after decode and responses before encode      |
|  [03]   | `AdmissionError.phase` / `.cause`             | evidence    | preserves client refusal direction and Protovalidate failure     |
|  [04]   | `AdmissionError.violations`                   | evidence    | returns typed constraint findings; engine defects return empty   |
|  [05]   | `ConnectError(INVALID_ARGUMENT, details=...)` | server rail | exposes request constraint findings as `buf.validate.Violations` |
|  [06]   | `ConnectError(INTERNAL)`                      | server rail | withholds response findings and validation-engine defects        |
|  [07]   | `AsyncClosable`                               | protocol    | admits any streamed body that can release its source early       |

[ARTIFACT_SCOPE]: generated-client transfer and stream-wide proof

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `ArtifactLaw` fields                | policy   | frame floor, frame ceiling, extent bounds, identity width      |
|  [02]   | `output(suffix="")`                 | context  | helper-owned writable path with one native format extension    |
|  [03]   | `ArtifactSink.seal(source, claim=)` | async    | one latch, one spool digest, and one stated claim per route    |
|  [04]   | `stage(source, claim=)`             | context  | copies bytes, paths, or asynchronous chunks into owned custody |
|  [05]   | `receive(frames, claim=)`           | context  | proves repeated ref, frame width, extent, and identity         |
|  [06]   | `frames(owned)`                     | stream   | bare frame emission at the descriptor-declared frame width     |
|  [07]   | `fetch_responses(owned)`            | stream   | Fetch response wrapping for service implementations            |
|  [08]   | `put_requests(owned)`               | stream   | Put request wrapping for the publishing client                 |
|  [09]   | `put_frames(requests)`              | stream   | generated Put request unwrap for service-side receipt proof    |
|  [10]   | `fetch_frames(responses)`           | stream   | generated Fetch response unwrap for client-side receipt proof  |
|  [11]   | `confirm(expected, actual)`         | function | extent-then-identity reference confirmation on the rail        |
|  [12]   | `references(message)`               | function | frontier-walked generated-message `ArtifactRef` discovery      |
|  [13]   | `rendered(refusal)`                 | function | total refusal projection carrying each case's own evidence     |
|  [14]   | `ArtifactTransfer.put(source)`      | async    | stage, generated Put wrappers, publish, and confirm            |
|  [15]   | `ArtifactTransfer.publish(owned)`   | async    | zero-copy helper-owned Put and confirm                         |
|  [16]   | `ArtifactTransfer.fetch(ref)`       | context  | generated Fetch unwrap, receive, proof, and cleanup            |

<!-- roster:begin -->
[ROSTER_SCOPE]: `buf.validate` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]  | [ORIGIN]        | [SYMBOL]           |
| :-----: | :----------------- | :------ | :-------------- | :----------------- |
|  [01]   | `Rule`             | message | support-closure | `Rule`             |
|  [02]   | `MessageRules`     | message | support-closure | `MessageRules`     |
|  [03]   | `MessageOneofRule` | message | support-closure | `MessageOneofRule` |
|  [04]   | `OneofRules`       | message | support-closure | `OneofRules`       |
|  [05]   | `FieldRules`       | message | support-closure | `FieldRules`       |
|  [06]   | `PredefinedRules`  | message | support-closure | `PredefinedRules`  |
|  [07]   | `FloatRules`       | message | support-closure | `FloatRules`       |
|  [08]   | `DoubleRules`      | message | support-closure | `DoubleRules`      |
|  [09]   | `Int32Rules`       | message | support-closure | `Int32Rules`       |
|  [10]   | `Int64Rules`       | message | support-closure | `Int64Rules`       |
|  [11]   | `UInt32Rules`      | message | support-closure | `UInt32Rules`      |
|  [12]   | `UInt64Rules`      | message | support-closure | `UInt64Rules`      |
|  [13]   | `SInt32Rules`      | message | support-closure | `SInt32Rules`      |
|  [14]   | `SInt64Rules`      | message | support-closure | `SInt64Rules`      |
|  [15]   | `Fixed32Rules`     | message | support-closure | `Fixed32Rules`     |
|  [16]   | `Fixed64Rules`     | message | support-closure | `Fixed64Rules`     |
|  [17]   | `SFixed32Rules`    | message | support-closure | `SFixed32Rules`    |
|  [18]   | `SFixed64Rules`    | message | support-closure | `SFixed64Rules`    |
|  [19]   | `BoolRules`        | message | support-closure | `BoolRules`        |
|  [20]   | `StringRules`      | message | support-closure | `StringRules`      |
|  [21]   | `BytesRules`       | message | support-closure | `BytesRules`       |
|  [22]   | `EnumRules`        | message | support-closure | `EnumRules`        |
|  [23]   | `RepeatedRules`    | message | support-closure | `RepeatedRules`    |
|  [24]   | `MapRules`         | message | support-closure | `MapRules`         |
|  [25]   | `AnyRules`         | message | support-closure | `AnyRules`         |
|  [26]   | `DurationRules`    | message | support-closure | `DurationRules`    |
|  [27]   | `FieldMaskRules`   | message | support-closure | `FieldMaskRules`   |
|  [28]   | `TimestampRules`   | message | support-closure | `TimestampRules`   |
|  [29]   | `Ignore`           | enum    | support-closure | `Ignore`           |
|  [30]   | `KnownRegex`       | enum    | support-closure | `KnownRegex`       |

[ROSTER_SCOPE]: `rasm.contracts.artifact.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `rasm.contracts.appearance.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `PlaneRef`                            | message | support-closure | `PlaneRef`                            |
|  [02]   | `Plane`                               | message | support-closure | `Plane`                               |
|  [03]   | `PackRow`                             | message | support-closure | `PackRow`                             |
|  [04]   | `Role`                                | enum    | support-closure | `Role`                                |
|  [05]   | `Transfer`                            | enum    | support-closure | `Transfer`                            |
|  [06]   | `NormalConvention`                    | enum    | support-closure | `NormalConvention`                    |
|  [07]   | `AlphaMode`                           | enum    | support-closure | `AlphaMode`                           |
|  [08]   | `Container`                           | enum    | support-closure | `Container`                           |
|  [09]   | `Pack`                                | enum    | support-closure | `Pack`                                |
|  [10]   | `PlaneFormat`                         | enum    | support-closure | `PlaneFormat`                         |
|  [11]   | `MipPolicy`                           | enum    | support-closure | `MipPolicy`                           |
|  [12]   | `KtxPayload`                          | enum    | support-closure | `KtxPayload`                          |
|  [13]   | `BlockFormat`                         | enum    | support-closure | `BlockFormat`                         |
|  [14]   | `LayerLaw`                            | enum    | support-closure | `LayerLaw`                            |
|  [15]   | `LicenseClass`                        | enum    | support-closure | `LicenseClass`                        |
|  [16]   | `Udim`                                | enum    | support-closure | `Udim`                                |
|  [17]   | `Primaries`                           | enum    | support-closure | `Primaries`                           |
|  [18]   | `Depth`                               | enum    | support-closure | `Depth`                               |
|  [19]   | `Tool`                                | enum    | support-closure | `Tool`                                |
|  [20]   | `EnvironmentPlane`                    | message | support-closure | `EnvironmentPlane`                    |
|  [21]   | `EnvironmentSource`                   | message | support-closure | `EnvironmentSource`                   |
|  [22]   | `Hdri`                                | message | support-closure | `Hdri`                                |
|  [23]   | `Ibl`                                 | message | support-closure | `Ibl`                                 |
|  [24]   | `Provenance`                          | message | support-closure | `Provenance`                          |
|  [25]   | `Provenance.Capture`                  | message | support-closure | `Provenance.Capture`                  |
|  [26]   | `Provenance.Fit`                      | message | support-closure | `Provenance.Fit`                      |
|  [27]   | `Provenance.Inference`                | message | support-closure | `Provenance.Inference`                |
|  [28]   | `Provenance.Chromaticity`             | message | support-closure | `Provenance.Chromaticity`             |
|  [29]   | `Provenance.Chromaticity.Dominance`   | message | support-closure | `Provenance.Chromaticity.Dominance`   |
|  [30]   | `Provenance.Chromaticity.Temperature` | message | support-closure | `Provenance.Chromaticity.Temperature` |
|  [31]   | `Provenance.Card`                     | message | support-closure | `Provenance.Card`                     |
|  [32]   | `Provenance.Ingest`                   | message | support-closure | `Provenance.Ingest`                   |
|  [33]   | `Press`                               | message | support-closure | `Press`                               |
|  [34]   | `SurfaceSet`                          | message | support-closure | `SurfaceSet`                          |
|  [35]   | `BakedSet`                            | message | support-closure | `BakedSet`                            |
|  [36]   | `EnvironmentSet`                      | message | support-closure | `EnvironmentSet`                      |
|  [37]   | `Set`                                 | message | public-root     | `Set`                                 |

[ROSTER_SCOPE]: `rasm.contracts.spatial.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]           | [KIND]  | [ORIGIN]        | [SYMBOL]         |
| :-----: | :--------------- | :------ | :-------------- | :--------------- |
|  [01]   | `Point3`         | message | support-closure | `Point3`         |
|  [02]   | `Displacement3`  | message | support-closure | `Displacement3`  |
|  [03]   | `UnitDirection3` | message | support-closure | `UnitDirection3` |
|  [04]   | `Axis3`          | message | support-closure | `Axis3`          |
|  [05]   | `Frame3`         | message | support-closure | `Frame3`         |
|  [06]   | `LineSegment3`   | message | support-closure | `LineSegment3`   |
|  [07]   | `ArcSegment3`    | message | support-closure | `ArcSegment3`    |
|  [08]   | `SplineSegment3` | message | support-closure | `SplineSegment3` |
|  [09]   | `CurveSegment3`  | message | support-closure | `CurveSegment3`  |
|  [10]   | `Curve3`         | message | support-closure | `Curve3`         |

[ROSTER_SCOPE]: `google.type` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME] | [KIND]  | [ORIGIN]        | [SYMBOL] |
| :-----: | :----- | :------ | :-------------- | :------- |
|  [01]   | `Date` | message | support-closure | `Date`   |

[ROSTER_SCOPE]: `rasm.contracts.declaration.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `google.rpc` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                      | [KIND]  | [ORIGIN]        | [SYMBOL]                    |
| :-----: | :-------------------------- | :------ | :-------------- | :-------------------------- |
|  [01]   | `RetryInfo`                 | message | support-closure | `RetryInfo`                 |
|  [02]   | `BadRequest`                | message | support-closure | `BadRequest`                |
|  [03]   | `BadRequest.FieldViolation` | message | support-closure | `BadRequest.FieldViolation` |
|  [04]   | `LocalizedMessage`          | message | support-closure | `LocalizedMessage`          |

[ROSTER_SCOPE]: `rasm.contracts.clock.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME] | [KIND]  | [ORIGIN]    | [SYMBOL] |
| :-----: | :----- | :------ | :---------- | :------- |
|  [01]   | `Hlc`  | message | public-root | `Hlc`    |

[ROSTER_SCOPE]: `rasm.contracts.fault.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]          | [KIND]  | [ORIGIN]        | [SYMBOL]        |
| :-----: | :-------------- | :------ | :-------------- | :-------------- |
|  [01]   | `FaultRecovery` | message | support-closure | `FaultRecovery` |
|  [02]   | `FaultDetail`   | message | public-root     | `FaultDetail`   |

[ROSTER_SCOPE]: `rasm.contracts.cad.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [SYMBOL]                |
| :-----: | :---------------------- | :------ | :-------------- | :---------------------- |
|  [01]   | `Point2`                | message | support-closure | `Point2`                |
|  [02]   | `LineSpan`              | message | support-closure | `LineSpan`              |
|  [03]   | `ArcSpan`               | message | support-closure | `ArcSpan`               |
|  [04]   | `SplineSpan`            | message | support-closure | `SplineSpan`            |
|  [05]   | `ProfileKnot`           | message | support-closure | `ProfileKnot`           |
|  [06]   | `PiecewiseLoop`         | message | support-closure | `PiecewiseLoop`         |
|  [07]   | `PeriodicSplineLoop`    | message | support-closure | `PeriodicSplineLoop`    |
|  [08]   | `ProfileLoop`           | message | support-closure | `ProfileLoop`           |
|  [09]   | `ProfileRegion`         | message | support-closure | `ProfileRegion`         |
|  [10]   | `Profile`               | message | support-closure | `Profile`               |
|  [11]   | `SealedStep`            | message | support-closure | `SealedStep`            |
|  [12]   | `TopologyCensus`        | message | support-closure | `TopologyCensus`        |
|  [13]   | `BooleanProvenance`     | message | support-closure | `BooleanProvenance`     |
|  [14]   | `BrepKernelReceipt`     | message | support-closure | `BrepKernelReceipt`     |
|  [15]   | `TessellateResponse`    | message | public-root     | `TessellateResponse`    |
|  [16]   | `StepProtocol`          | enum    | support-closure | `StepProtocol`          |
|  [17]   | `BoxOp`                 | message | support-closure | `BoxOp`                 |
|  [18]   | `SphereOp`              | message | support-closure | `SphereOp`              |
|  [19]   | `CylinderOp`            | message | support-closure | `CylinderOp`            |
|  [20]   | `ConeOp`                | message | support-closure | `ConeOp`                |
|  [21]   | `TorusOp`               | message | support-closure | `TorusOp`               |
|  [22]   | `BooleanInputs`         | message | support-closure | `BooleanInputs`         |
|  [23]   | `ProfileOffset`         | message | support-closure | `ProfileOffset`         |
|  [24]   | `PlacedProfile`         | message | support-closure | `PlacedProfile`         |
|  [25]   | `ExtrudeOp`             | message | support-closure | `ExtrudeOp`             |
|  [26]   | `RevolveOp`             | message | support-closure | `RevolveOp`             |
|  [27]   | `LoftSection`           | message | support-closure | `LoftSection`           |
|  [28]   | `LoftTrack`             | message | support-closure | `LoftTrack`             |
|  [29]   | `LoftOp`                | message | support-closure | `LoftOp`                |
|  [30]   | `ThickOp`               | message | support-closure | `ThickOp`               |
|  [31]   | `SweepOp`               | message | support-closure | `SweepOp`               |
|  [32]   | `TransformOp`           | message | support-closure | `TransformOp`           |
|  [33]   | `EdgeIndices`           | message | support-closure | `EdgeIndices`           |
|  [34]   | `EdgeSelection`         | message | support-closure | `EdgeSelection`         |
|  [35]   | `FilletOp`              | message | support-closure | `FilletOp`              |
|  [36]   | `ChamferOp`             | message | support-closure | `ChamferOp`             |
|  [37]   | `SewOp`                 | message | support-closure | `SewOp`                 |
|  [38]   | `ExecuteRequest`        | message | public-root     | `ExecuteRequest`        |
|  [39]   | `ExecuteResponse`       | message | public-root     | `ExecuteResponse`       |
|  [40]   | `LoftStyle`             | enum    | support-closure | `LoftStyle`             |
|  [41]   | `TessellateRequest`     | message | public-root     | `TessellateRequest`     |
|  [42]   | `CadService`            | service | support-closure | `CadService`            |
|  [43]   | `CadService.Execute`    | method  | public-root     | `CadService.Execute`    |
|  [44]   | `CadService.Tessellate` | method  | public-root     | `CadService.Tessellate` |

[ROSTER_SCOPE]: `rasm.contracts.geometry.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]             |
| :-----: | :------------------- | :------ | :-------------- | :------------------- |
|  [01]   | `TessellationPolicy` | message | support-closure | `TessellationPolicy` |

[ROSTER_SCOPE]: `rasm.contracts.capability.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `DescriptorPinWire`                   | message | support-closure | `DescriptorPinWire`                   |
|  [02]   | `CapabilityEstimate`                  | message | support-closure | `CapabilityEstimate`                  |
|  [03]   | `AvailableCapability`                 | message | support-closure | `AvailableCapability`                 |
|  [04]   | `DiscoverRequest`                     | message | public-root     | `DiscoverRequest`                     |
|  [05]   | `DiscoverResponse`                    | message | public-root     | `DiscoverResponse`                    |
|  [06]   | `CostUnit`                            | enum    | support-closure | `CostUnit`                            |
|  [07]   | `CapabilityDiscoveryService`          | service | support-closure | `CapabilityDiscoveryService`          |
|  [08]   | `CapabilityDiscoveryService.Discover` | method  | public-root     | `CapabilityDiscoveryService.Discover` |

[ROSTER_SCOPE]: `rasm.contracts.compute.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                      | [KIND]  | [ORIGIN]        | [SYMBOL]                    |
| :-----: | :-------------------------- | :------ | :-------------- | :-------------------------- |
|  [01]   | `ElementScope`              | message | support-closure | `ElementScope`              |
|  [02]   | `EntityScope`               | message | support-closure | `EntityScope`               |
|  [03]   | `TessellationScope`         | message | support-closure | `TessellationScope`         |
|  [04]   | `TessellateRequest`         | message | public-root     | `TessellateRequest`         |
|  [05]   | `Semantic`                  | message | support-closure | `Semantic`                  |
|  [06]   | `TessellateResponse`        | message | public-root     | `TessellateResponse`        |
|  [07]   | `Spill`                     | enum    | support-closure | `Spill`                     |
|  [08]   | `GeomSetting`               | enum    | support-closure | `GeomSetting`               |
|  [09]   | `Dimensionality`            | enum    | support-closure | `Dimensionality`            |
|  [10]   | `ComputeService`            | service | support-closure | `ComputeService`            |
|  [11]   | `ComputeService.Tessellate` | method  | public-root     | `ComputeService.Tessellate` |

[ROSTER_SCOPE]: `rasm.contracts.crdt.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `rasm.contracts.event.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]       | [KIND]  | [ORIGIN]    | [SYMBOL]     |
| :-----: | :----------- | :------ | :---------- | :----------- |
|  [01]   | `Extensions` | message | public-root | `Extensions` |

[ROSTER_SCOPE]: `rasm.contracts.fabrication.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `rasm.contracts.organization.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------- | :------ | :-------------- | :------------- |
|  [01]   | `ViewOverride` | message | support-closure | `ViewOverride` |
|  [02]   | `Entity`       | message | support-closure | `Entity`       |
|  [03]   | `EntityPath`   | message | support-closure | `EntityPath`   |
|  [04]   | `Organization` | message | public-root     | `Organization` |

[ROSTER_SCOPE]: `rasm.contracts.parity.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]         | [KIND]  | [ORIGIN]        | [SYMBOL]       |
| :-----: | :------------- | :------ | :-------------- | :------------- |
|  [01]   | `Artifact`     | message | support-closure | `Artifact`     |
|  [02]   | `Capability`   | message | support-closure | `Capability`   |
|  [03]   | `Backend`      | message | public-root     | `Backend`      |
|  [04]   | `ArtifactRole` | enum    | support-closure | `ArtifactRole` |
|  [05]   | `Provider`     | enum    | support-closure | `Provider`     |
|  [06]   | `FailureRank`  | enum    | support-closure | `FailureRank`  |
|  [07]   | `RestartClass` | enum    | support-closure | `RestartClass` |

[ROSTER_SCOPE]: `rasm.contracts.scan.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]              | [KIND]  | [ORIGIN]        | [SYMBOL]            |
| :-----: | :------------------ | :------ | :-------------- | :------------------ |
|  [01]   | `GaussianSplatScan` | message | public-root     | `GaussianSplatScan` |
|  [02]   | `SplatFormat`       | enum    | support-closure | `SplatFormat`       |

[ROSTER_SCOPE]: `rasm.contracts.scene.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `grpc.health.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                              | [KIND]  | [ORIGIN]        | [SYMBOL]                            |
| :-----: | :---------------------------------- | :------ | :-------------- | :---------------------------------- |
|  [01]   | `HealthCheckRequest`                | message | public-root     | `HealthCheckRequest`                |
|  [02]   | `HealthCheckResponse`               | message | public-root     | `HealthCheckResponse`               |
|  [03]   | `HealthCheckResponse.ServingStatus` | enum    | support-closure | `HealthCheckResponse.ServingStatus` |
|  [04]   | `Health`                            | service | support-closure | `Health`                            |
|  [05]   | `Health.Check`                      | method  | public-root     | `Health.Check`                      |

[ROSTER_SCOPE]: `io.cloudevents.v1` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                                | [KIND]  | [ORIGIN]        | [SYMBOL]                              |
| :-----: | :------------------------------------ | :------ | :-------------- | :------------------------------------ |
|  [01]   | `CloudEvent`                          | message | public-root     | `CloudEvent`                          |
|  [02]   | `CloudEvent.CloudEventAttributeValue` | message | support-closure | `CloudEvent.CloudEventAttributeValue` |
|  [03]   | `CloudEventBatch`                     | message | public-root     | `CloudEventBatch`                     |

[ASSET_SCOPE]: exact publisher projections emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]           | [PATH]                                                                           |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------------------------------- |
|  [01]   | `cloudevents.avsc` | package-resource | `libs/python/contracts/rasm/contracts/vendor/io/cloudevents/v1/cloudevents.avsc` |

<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root generation writes estate messages and reachable support under `rasm.contracts.gen`.
- Separate publisher generation writes CloudEvents and health modules under `rasm.contracts.vendor`.
- Assay restores exact manifest-distributed publisher resources after Buf's clean sweep.
- Connect generation writes asynchronous stubs beside each selected service package without duplicating message types.
- Root install seats `rasm.contracts` through its module root; generated modules import their runtimes and no sibling library.

[STACKING]:
- `protobuf-py`(`../../.api/protobuf-py.md`): generated `Message` classes own binary, ProtoJSON, oneof, WKT, and descriptor operations.
- `connectrpc`(`../../.api/connectrpc.md`): generated applications and clients bind `Endpoint`, `MethodInfo`, `RequestContext`, and `ConnectClient`.
- `protovalidate`(`../../.api/protovalidate.md`): generated descriptors feed direct standard and CEL validation with typed violations.
- `expression`(`../../.api/expression.md`): `Result` carries every artifact custody outcome until the generated-stream edge collapses it to a raise.
- `buf.validate` descriptors: `DescField.proto.options[ext_field]` yields the corpus rule set `ArtifactLaw` projects, so no bound is respelled here.
- `BodyAdmission`: one official-protocol composition validates every asynchronous Connect body element without buffering streams or mirroring rules.

[LOCAL_ADMISSION]:
- Consumers import estate classes through `rasm.contracts.gen.rasm.contracts` and publisher classes through `rasm.contracts.vendor`.
- Publisher resources resolve below `files("rasm.contracts")`; no runtime path reaches into `tests/contracts`.
- Generated classes remain wire values; `BodyAdmission` validates their transport crossing and consumers convert admitted values into domain values.
- Corpus and generator changes rewrite `gen` and `vendor`; authored boundary modules stay above both clean roots.
- Authored package content stays above the clean roots and owns only package identity, typing, and generic generated-message boundary composition.

[RAIL_LAW]:
- Package: `rasm.contracts` module
- Owns: generated Python bindings, generic Connect body admission, validation runtime closure, and exact publisher resources for selected packages
- Accept: installed imports from `rasm.contracts.gen` and `rasm.contracts.vendor`
- Reject: handwritten message twins, field-rule mirrors, respelled `buf.validate` bounds, handler validation prologues, copied publisher assets, hand-built services, and import-path mutation
