# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_AWS]

`@opentelemetry/resource-detector-aws` mints five `ResourceDetector` instances — `awsEc2Detector`, `awsEcsDetector`, `awsEksDetector`, `awsBeanstalkDetector`, `awsLambdaDetector` — each stamping the AWS compute service it names onto the OTLP `Resource`. They land as detector rows in the `otel/emit` node roster beside `envDetector`/`hostDetector`/`osDetector`/`processDetector`/`serviceInstanceIdDetector`, never composed in a library. Detector selection is a deploy-target fact — every arm composes only its own service row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-aws`
- package: `@opentelemetry/resource-detector-aws` (Apache-2.0)
- base: each detector implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource`
- consumed-by: `otel/emit` node detector roster; folds beside the `@opentelemetry/resources` `env`/`host`/`os`/`process`/`serviceInstanceId` detectors
- runtime: node only — reads EC2 IMDSv2, ECS and EKS metadata endpoints, Beanstalk config file, and Lambda environment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instances + contract
- rail: observability/resource/detect
- Detectors are ONE parameterized family — every instance implements `ResourceDetector.detect(): DetectedResource`, so a compute arm is a row selection, never a new interface.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                 |
| :-----: | :------------------------------------------------ | :---------------- | :-------------------------------------------------- |
|  [01]   | `awsEc2Detector: AwsEc2Detector`                  | detector instance | EC2 IMDSv2 instance-identity-document facts         |
|  [02]   | `awsEcsDetector: AwsEcsDetector`                  | detector instance | ECS metadata-v4 and cgroup container facts          |
|  [03]   | `awsEksDetector: AwsEksDetector`                  | detector instance | EKS cluster-info and container facts                |
|  [04]   | `awsBeanstalkDetector: AwsBeanstalkDetector`      | detector instance | Beanstalk config-file environment facts             |
|  [05]   | `awsLambdaDetector: AwsLambdaDetector`            | detector instance | Lambda runtime-environment facts                    |
|  [06]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds            |
|  [07]   | `DetectedResource { attributes? }`                | detector output   | `cloud.*`/`aws.*` attribute map each detect returns |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect
- One instance per compute arm folds into the node lane's `ResourceDetector[]`; a host carries exactly one AWS arm row.

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                             |
| :-----: | :--------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `awsEc2Detector`       | detector row   | EC2 arm row in `detectResources({ detectors })` |
|  [02]   | `awsEcsDetector`       | detector row   | ECS arm row in the node lane roster             |
|  [03]   | `awsEksDetector`       | detector row   | EKS arm row in the node lane roster             |
|  [04]   | `awsBeanstalkDetector` | detector row   | Beanstalk arm row in the node lane roster       |
|  [05]   | `awsLambdaDetector`    | detector row   | Lambda arm row in the node lane roster          |

## [04]-[IMPLEMENTATION_LAW]

[AWS_DETECTOR_TOPOLOGY]:
- node-roster rows only — each instance folds into the `otel/emit` node lane's `ResourceDetector[]` beside the `@opentelemetry/resources` detectors; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — an EC2 host composes `awsEc2Detector`, an ECS task `awsEcsDetector`, an EKS pod `awsEksDetector`, a Beanstalk arm `awsBeanstalkDetector`, a Lambda function `awsLambdaDetector`; one host stacks one AWS arm row.

[INTEGRATION_LAW]:
- Stack with `otel/emit` node row: a chosen instance enters the node lane's detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its `cloud.*`/`aws.*` attributes merge onto the `AppIdentity` base `Resource`.
- Stack with `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs the instance in the ordered set and `merge`s its output; the async-attribute barrier gates first export until the metadata read resolves.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; one arm's row lives only in that arm's boot graph, never a browser or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-aws`
- Owns: EC2, ECS, EKS, Beanstalk, and Lambda compute-service resource detection
- Accept: one deploy-arm detector row in the `otel/emit` node roster matching the host's compute service
- Reject: library-altitude composition, stacking multiple AWS arm rows on one host, a hand-rolled IMDS reader where a detector belongs
