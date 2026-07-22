# [RASM_API_OTEL_INSTRUMENTATION_AWS]

`OpenTelemetry.Instrumentation.AWS` binds the AWS SDK for .NET's own telemetry seat: admission seats an `AWSSDK.*`-scoped tracer and meter provider on the SDK telemetry provider and hooks a tracing customizer into every service client's runtime pipeline, so the SDK mints one `Client`-kind span per operation and this package enriches it with aws-semconv attributes. Emission is per-service — one `ActivitySource` and one `Meter` per `AWSSDK.<Service>` scope, never one package-wide source.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.AWS`
- package: `OpenTelemetry.Instrumentation.AWS`
- assembly: `OpenTelemetry.Instrumentation.AWS`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Instrumentation.AWS`
- rail: cloud-sdk instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options carrier and the convention pin it holds

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `AWSClientInstrumentationOptions` | class         | suppression and attribute-vocabulary seat |
|  [02]   | `SemanticConventionVersion`       | enum          | emitted aws-semconv attribute vocabulary  |

[SemanticConventionVersion]: `Latest` `V1_28_0` (default) `V1_29_0` `V1_40_0`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder admission over the trace and metric legs

| [INDEX] | [SURFACE]                                                                              | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `TracerProviderBuilder.AddAWSInstrumentation()`                                        | static  | unconfigured trace admission   |
|  [02]   | `TracerProviderBuilder.AddAWSInstrumentation(Action<AWSClientInstrumentationOptions>)` | static  | options-seated trace admission |
|  [03]   | `MeterProviderBuilder.AddAWSInstrumentation()`                                         | static  | metric admission               |

- `MeterProviderBuilder.AddAWSInstrumentation`: pins `SemanticConventionVersion.V1_28_0`, so the metric leg never follows the tracer's convention selection.
- `AWSClientInstrumentationOptions.SuppressDownstreamInstrumentation`: opens the ambient suppression scope around the whole SDK invocation, so every nested instrumentation drops its span, never the HTTP leg alone.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Admission writes two process-global seats: `RuntimePipelineCustomizerRegistry.Instance` takes the tracing customizer and ignores a later same-name registration, while `AWSConfigs.TelemetryProvider` takes the convention-bound provider and lets a later call overwrite it — a second admission runs the newer convention pin against the first call's attribute options.
- Customizers apply where `AmazonServiceClient` builds its pipeline, so admission precedes every SDK client the process constructs.
- Every outbound AWS request carries X-Ray-format context: the propagator handler injects through `AWSXRayPropagator` wherever `Activity.Current` rides an `AWSSDK` source, ignoring the process default propagator; SQS and SNS calls carry that context on message attributes.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `AddAWSInstrumentation` is the whole verb — a bare `AddSource("AWSSDK.*")` subscribes a source no unregistered customizer enriches, and the root's `Sdk.SetDefaultTextMapPropagator` composite governs every egress except the AWS leg this package propagates itself.
- `OpenTelemetry.Instrumentation.Http`(`api-otel-instrumentation-http.md`): `SuppressDownstreamInstrumentation` drops the `HttpClient` span the SDK transport nests inside each AWS operation span, leaving one span carrying the region, service, and operation attributes.
- `Rasm.Persistence`: one root admission spans both held SDK clients — `IAmazonS3` object transfers and `IAmazonKeyManagementService` custody calls — off the `AWSSDK.Core` pipeline they share, never a per-client bind.

[LOCAL_ADMISSION]:
- Composition-root-only: both registries are process-global, so the first admitted options govern every app co-hosted in the process.
- `SuppressDownstreamInstrumentation` sets wherever the root also admits HTTP instrumentation.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AWS`
- Owns: aws-semconv enrichment and `AWSSDK.*` source and meter admission for every SDK client in the process
- Accept: `AddAWSInstrumentation` on the tracer and meter builders at the composition root, ahead of client construction
- Reject: per-client or per-app customizer registration; hand-rolled aws-semconv tagging over SDK operations; a bare `AddSource`/`AddMeter` standing in for admission
