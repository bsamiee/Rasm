# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_AWS]

`@opentelemetry/resource-detector-aws` mints one `ResourceDetector` instance per AWS compute service, each reading the host metadata endpoint and stamping its `cloud.*`/`aws.*` facts onto the OTLP `Resource`. A host composes exactly the one arm row matching its deploy target; a library-altitude compose double-detects and is refused.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-aws`
- package: `@opentelemetry/resource-detector-aws` (Apache-2.0)
- rail: observability/resource/detect
- base: each detector implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource`
- runtime: node only — reads EC2 IMDSv2, ECS/EKS metadata endpoints, the Beanstalk config file, and the Lambda environment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the AWS detector family
- Detectors are ONE parameterized family — every instance implements `ResourceDetector.detect(): DetectedResource`, so a compute arm is a row selection, never a new interface.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]     | [CAPABILITY]                                |
| :-----: | :------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `awsEc2Detector: AwsEc2Detector`             | detector instance | EC2 IMDSv2 instance-identity-document facts |
|  [02]   | `awsEcsDetector: AwsEcsDetector`             | detector instance | ECS metadata-v4 and cgroup container facts  |
|  [03]   | `awsEksDetector: AwsEksDetector`             | detector instance | EKS cluster-info and container facts        |
|  [04]   | `awsBeanstalkDetector: AwsBeanstalkDetector` | detector instance | Beanstalk config-file environment facts     |
|  [05]   | `awsLambdaDetector: AwsLambdaDetector`       | detector instance | Lambda runtime-environment facts            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- Each instance folds into the node lane's `ResourceDetector[]` via `detectResources({ detectors })`; a host carries exactly one AWS arm row.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each instance folds into the `otel/emit` node lane's `ResourceDetector[]` beside the `@opentelemetry/resources` detectors; a library-altitude compose double-detects the host.
- Deploy-target selection governs the row — a host composes its matching compute-service arm, and one host stacks exactly one AWS arm row.

[STACKING]:
- `opentelemetry-resources`(`.api/opentelemetry-resources.md`): `detectResources({ detectors })` runs the instance in the ordered set and `merge`s its output onto the base `Resource`; `waitForAsyncAttributes` gates first export until the metadata read resolves.
- `otel/emit` node row: a chosen instance enters the node lane's detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its `cloud.*`/`aws.*` attributes merge onto the `AppIdentity` base `Resource`.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; an arm's row lives only in that arm's boot graph, never a browser or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-aws`
- Owns: AWS compute-service resource detection across EC2, ECS, EKS, Beanstalk, and Lambda
- Accept: one deploy-arm detector row in the `otel/emit` node roster matching the host's compute service
- Reject: library-altitude composition, multiple AWS arm rows on one host, a hand-rolled IMDS reader where a detector belongs
