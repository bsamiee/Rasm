# [CONTRACTS_API_PYTHON]

`rasm.contracts` owns the generated Python bindings, Connect stubs, and exact publisher resources of the contract corpus under one import root spelled package and proto path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasm.contracts`
- package: `rasm.contracts`
- module: `rasm.contracts.rasm.contracts.<family>.<source>_pb`, `<source>_connect`, and `rasm.contracts.{buf,google,grpc,io}.<proto path>`
- namespaces: package and proto path from one root; `rasm` and `rasm.contracts` are PEP 420 portions
- abi: pure Python wheel `uv_build` builds from `gen/python`; `py.typed` gate-projected beneath the swept root
- depends: `protobuf-py`, `connectrpc`
- role: the `rasm-contracts` distribution, wholly emission
- rail: generated messages, descriptors, service protocols, applications, clients, and publisher resources

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one proto declaration's generated Python correspondence

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `rasm.contracts.rasm.contracts.<f>.<s>_pb.<Msg>`      | class         | typed `Message[Literal[fields]]` value         |
|  [02]   | `<s>_pb.<Outer>.<Inner>`                              | class         | nested generated message                       |
|  [03]   | `<s>_pb.<Enum>`                                       | class         | generated enumeration and exact wire numbers   |
|  [04]   | `<s>_pb.desc()`                                       | static        | generated `DescFile` and dependency graph      |
|  [05]   | `rasm.contracts.rasm.contracts.<f>.<s>_connect.<Svc>` | protocol      | asynchronous handler surface                   |
|  [06]   | `<s>_connect.<Svc>ASGIApplication`                    | class         | generated service application and endpoint set |
|  [07]   | `<s>_connect.<Svc>Client`                             | class         | generated typed asynchronous client            |
|  [08]   | `rasm.contracts.{buf,google}.<path>.<s>_pb.<Msg>`     | class         | reachable support declaration                  |
|  [09]   | `rasm.contracts.{grpc,io}.<path>.<s>_pb.<Msg>`        | class         | publisher-owned generated declaration          |
|  [10]   | `files("rasm.contracts").joinpath(<path>)`            | resource      | exact gate-projected publisher bytes           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated public roots and reachable support closure, grouped by descriptor package

<!-- roster:begin -->
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

[ROSTER_SCOPE]: `rasm.contracts.spatial` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `google.rpc` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                      | [KIND]  | [ORIGIN]        | [SYMBOL]                    |
| :-----: | :-------------------------- | :------ | :-------------- | :-------------------------- |
|  [01]   | `RetryInfo`                 | message | support-closure | `RetryInfo`                 |
|  [02]   | `BadRequest`                | message | support-closure | `BadRequest`                |
|  [03]   | `BadRequest.FieldViolation` | message | support-closure | `BadRequest.FieldViolation` |
|  [04]   | `LocalizedMessage`          | message | support-closure | `LocalizedMessage`          |

[ROSTER_SCOPE]: `rasm.contracts.clock` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME] | [KIND]  | [ORIGIN]    | [SYMBOL] |
| :-----: | :----- | :------ | :---------- | :------- |
|  [01]   | `Hlc`  | message | public-root | `Hlc`    |

[ROSTER_SCOPE]: `rasm.contracts.fault` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]          | [KIND]  | [ORIGIN]        | [SYMBOL]        |
| :-----: | :-------------- | :------ | :-------------- | :-------------- |
|  [01]   | `FaultRecovery` | message | support-closure | `FaultRecovery` |
|  [02]   | `FaultDetail`   | message | public-root     | `FaultDetail`   |

[ROSTER_SCOPE]: `rasm.contracts.cad` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]                  | [KIND]  | [ORIGIN]        | [SYMBOL]                |
| :-----: | :---------------------- | :------ | :-------------- | :---------------------- |
|  [01]   | `SealedBody`            | message | support-closure | `SealedBody`            |
|  [02]   | `Point2`                | message | support-closure | `Point2`                |
|  [03]   | `LineSpan`              | message | support-closure | `LineSpan`              |
|  [04]   | `ArcSpan`               | message | support-closure | `ArcSpan`               |
|  [05]   | `SplineSpan`            | message | support-closure | `SplineSpan`            |
|  [06]   | `ProfileKnot`           | message | support-closure | `ProfileKnot`           |
|  [07]   | `PiecewiseLoop`         | message | support-closure | `PiecewiseLoop`         |
|  [08]   | `PeriodicSplineLoop`    | message | support-closure | `PeriodicSplineLoop`    |
|  [09]   | `ProfileLoop`           | message | support-closure | `ProfileLoop`           |
|  [10]   | `ProfileRegion`         | message | support-closure | `ProfileRegion`         |
|  [11]   | `Profile`               | message | support-closure | `Profile`               |
|  [12]   | `Indices`               | message | support-closure | `Indices`               |
|  [13]   | `Selection`             | message | support-closure | `Selection`             |
|  [14]   | `Image`                 | message | support-closure | `Image`                 |
|  [15]   | `Trace`                 | message | support-closure | `Trace`                 |
|  [16]   | `Correspondence`        | message | support-closure | `Correspondence`        |
|  [17]   | `TopologyCensus`        | message | support-closure | `TopologyCensus`        |
|  [18]   | `Principal`             | message | support-closure | `Principal`             |
|  [19]   | `Inertia`               | message | support-closure | `Inertia`               |
|  [20]   | `BrepMeasure`           | message | support-closure | `BrepMeasure`           |
|  [21]   | `Healing`               | message | support-closure | `Healing`               |
|  [22]   | `Healing.Step`          | message | support-closure | `Healing.Step`          |
|  [23]   | `PartIdentity`          | message | support-closure | `PartIdentity`          |
|  [24]   | `TessellateResponse`    | message | public-root     | `TessellateResponse`    |
|  [25]   | `StepProtocol`          | enum    | support-closure | `StepProtocol`          |
|  [26]   | `Emission`              | enum    | support-closure | `Emission`              |
|  [27]   | `Grain`                 | enum    | support-closure | `Grain`                 |
|  [28]   | `Relation`              | enum    | support-closure | `Relation`              |
|  [29]   | `HealStepKind`          | enum    | support-closure | `HealStepKind`          |
|  [30]   | `BoxOp`                 | message | support-closure | `BoxOp`                 |
|  [31]   | `WedgeWindow`           | message | support-closure | `WedgeWindow`           |
|  [32]   | `WedgeOp`               | message | support-closure | `WedgeOp`               |
|  [33]   | `SphereBounds`          | message | support-closure | `SphereBounds`          |
|  [34]   | `SphereOp`              | message | support-closure | `SphereOp`              |
|  [35]   | `CylinderOp`            | message | support-closure | `CylinderOp`            |
|  [36]   | `ConeOp`                | message | support-closure | `ConeOp`                |
|  [37]   | `TorusBounds`           | message | support-closure | `TorusBounds`           |
|  [38]   | `TorusOp`               | message | support-closure | `TorusOp`               |
|  [39]   | `BooleanInputs`         | message | support-closure | `BooleanInputs`         |
|  [40]   | `ProfileOffset`         | message | support-closure | `ProfileOffset`         |
|  [41]   | `PlacedProfile`         | message | support-closure | `PlacedProfile`         |
|  [42]   | `ExtrudeOp`             | message | support-closure | `ExtrudeOp`             |
|  [43]   | `RevolveOp`             | message | support-closure | `RevolveOp`             |
|  [44]   | `LoftSection`           | message | support-closure | `LoftSection`           |
|  [45]   | `LoftTrack`             | message | support-closure | `LoftTrack`             |
|  [46]   | `LoftOp`                | message | support-closure | `LoftOp`                |
|  [47]   | `ThickOp`               | message | support-closure | `ThickOp`               |
|  [48]   | `SweepOp`               | message | support-closure | `SweepOp`               |
|  [49]   | `TransformOp`           | message | support-closure | `TransformOp`           |
|  [50]   | `RadiusRun`             | message | support-closure | `RadiusRun`             |
|  [51]   | `RadiusKnot`            | message | support-closure | `RadiusKnot`            |
|  [52]   | `RadiusLaw`             | message | support-closure | `RadiusLaw`             |
|  [53]   | `FilletOp`              | message | support-closure | `FilletOp`              |
|  [54]   | `ChamferSkew`           | message | support-closure | `ChamferSkew`           |
|  [55]   | `ChamferAngle`          | message | support-closure | `ChamferAngle`          |
|  [56]   | `ChamferOp`             | message | support-closure | `ChamferOp`             |
|  [57]   | `ShellOp`               | message | support-closure | `ShellOp`               |
|  [58]   | `DraftOp`               | message | support-closure | `DraftOp`               |
|  [59]   | `OffsetOp`              | message | support-closure | `OffsetOp`              |
|  [60]   | `DefeatureOp`           | message | support-closure | `DefeatureOp`           |
|  [61]   | `SewStep`               | message | support-closure | `SewStep`               |
|  [62]   | `FixStep`               | message | support-closure | `FixStep`               |
|  [63]   | `SmallEdgesStep`        | message | support-closure | `SmallEdgesStep`        |
|  [64]   | `HealStep`              | message | support-closure | `HealStep`              |
|  [65]   | `HealOp`                | message | support-closure | `HealOp`                |
|  [66]   | `ExecuteRequest`        | message | public-root     | `ExecuteRequest`        |
|  [67]   | `ExecuteResponse`       | message | public-root     | `ExecuteResponse`       |
|  [68]   | `OffsetJoin`            | enum    | support-closure | `OffsetJoin`            |
|  [69]   | `LoftStyle`             | enum    | support-closure | `LoftStyle`             |
|  [70]   | `DraftPropagation`      | enum    | support-closure | `DraftPropagation`      |
|  [71]   | `TessellateRequest`     | message | public-root     | `TessellateRequest`     |
|  [72]   | `CadService`            | service | support-closure | `CadService`            |
|  [73]   | `CadService.Execute`    | method  | public-root     | `CadService.Execute`    |
|  [74]   | `CadService.Tessellate` | method  | public-root     | `CadService.Tessellate` |

[ROSTER_SCOPE]: `rasm.contracts.geometry` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]               | [KIND]  | [ORIGIN]        | [SYMBOL]             |
| :-----: | :------------------- | :------ | :-------------- | :------------------- |
|  [01]   | `TessellationPolicy` | message | support-closure | `TessellationPolicy` |

[ROSTER_SCOPE]: `rasm.contracts.capability` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ROSTER_SCOPE]: `rasm.contracts.compute` — public roots and reachable support closure emitted by `assay contracts generate`; hand edits are overwritten

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

[ASSET_SCOPE]: exact publisher projections emitted by `assay contracts generate`; hand edits are overwritten

| [INDEX] | [NAME]             | [KIND]           | [PATH]                                                                        |
| :-----: | :----------------- | :--------------- | :---------------------------------------------------------------------------- |
|  [01]   | `cloudevents.avsc` | package-resource | `libs/contracts/gen/python/rasm/contracts/io/cloudevents/v1/cloudevents.avsc` |

<!-- roster:end -->

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `libs/contracts/RULINGS.md` `[02]-[SHAPE]` owns the one-root import grammar, the PEP 420 portions, and the projected `py.typed`.
- `gen/python/rasm/contracts/` is the one out root for both Python plugin rows; Connect stubs land beside each selected service's `_pb` module.
- Generated modules import their runtimes and no sibling library.

[STACKING]:
- `protobuf-py`(`../../python/.api/protobuf-py.md`): generated `Message` classes own binary, ProtoJSON, oneof, WKT, and descriptor operations.
- `connectrpc`(`../../python/.api/connectrpc.md`): applications and clients bind `Endpoint`, `MethodInfo`, `RequestContext`, and `ConnectClient`.
- `buf.validate` descriptors: `DescField.proto.options[ext_field]` yields the corpus rule set a consumer projects, so no bound is respelled anywhere.

[LOCAL_ADMISSION]:
- Consumers import every class at package and proto path — estate `rasm.contracts.rasm.contracts.<f>`, publisher `rasm.contracts.io.cloudevents.v1`.
- Publisher resources resolve below `files("rasm.contracts")`; no runtime path reaches into `libs/contracts/vendor`.
- Generated classes remain wire values; `python:runtime/transport/body` validates the crossing and consumers project admitted values into domain.
- Corpus and generator changes rewrite the whole root; nothing authored lives beneath it.

[RAIL_LAW]:
- Package: `rasm.contracts` module
- Owns: generated Python bindings, Connect stubs, and exact publisher resources for selected packages
- Accept: installed imports at package and proto path from the one root
- Reject: handwritten message twins, field-rule mirrors, respelled `buf.validate` bounds, copied publisher assets, and import-path mutation
