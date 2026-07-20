# [RASM_API_OTEL_INSTRUMENTATION_AWS]

`OpenTelemetry.Instrumentation.AWS` is the aws-semconv span and metric owner for the AWS SDK for .NET: it registers a runtime pipeline customizer wrapping every `AWSSDK.*` service client's execution pipeline, minting one `Client`-kind span per SDK operation off per-service `AWSSDK.*` `ActivitySource` names. Tracer and meter subscribe the same wildcard — `AddSource("AWSSDK.*")` and `AddMeter("AWSSDK.*")`. Its X-Ray id-generator/propagator half lives in the companion `OpenTelemetry.Extensions.AWS`, transitive and root-admitted only under an X-Ray backend.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.AWS`
- package: `OpenTelemetry.Instrumentation.AWS`
- assembly: `OpenTelemetry.Instrumentation.AWS`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Instrumentation.AWS`
- driver package: `AWSSDK.Core` — supplies the `RuntimePipelineCustomizerRegistry` the customizer hooks; every `AWSSDK.*` service client routes its execution through that shared pipeline
- companion: `OpenTelemetry.Extensions.AWS` — carries the `AWSXRayIdGenerator`/`AWSXRayPropagator` X-Ray trace-id half, transitive and root-admitted only under an X-Ray backend
- asset: runtime library
- rail: cloud-sdk instrumentation

## [02]-[PUBLIC_TYPES]

[INSTRUMENTATION_TYPES]: trace options carrier and semantic-convention policy
- rail: cloud-sdk instrumentation

| [INDEX] | [SYMBOL]                          | [KIND]          | [CAPABILITY]                                |
| :-----: | :-------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `AWSClientInstrumentationOptions` | options carrier | suppression and attribute-vocabulary policy |
|  [02]   | `SemanticConventionVersion`       | enum            | installed semantic-convention vocabulary    |

`AWSClientInstrumentationOptions` exposes `SuppressDownstreamInstrumentation` for collapsing the inner `HttpClient` span and `SemanticConventionVersion` for selecting the emitted attribute vocabulary. Derive selectable values from the installed `SemanticConventionVersion` enum surface; meter admission takes no options overload.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: source/meter subscription and pipeline-customizer registration
- rail: cloud-sdk instrumentation

| [INDEX] | [SURFACE]                        | [KIND]           | [CAPABILITY]                                      |
| :-----: | :------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `AddAWSInstrumentation` (tracer) | trace admission  | `AddSource("AWSSDK.*")` + customizer registration |
|  [02]   | `AddAWSInstrumentation` (meter)  | metric admission | `AddMeter("AWSSDK.*")` subscription, no options   |

`AddAWSInstrumentation(TracerProviderBuilder)` and its `AddAWSInstrumentation(TracerProviderBuilder, Action<AWSClientInstrumentationOptions>?)` twin subscribe `AddSource("AWSSDK.*")` and register `AWSTracingPipelineCustomizer` (UniqueName `"AWS Tracing Registration Customization"`) on the process-global `RuntimePipelineCustomizerRegistry.Instance`. `AddAWSInstrumentation(MeterProviderBuilder)` is the single meter overload — it subscribes `AddMeter("AWSSDK.*")` and takes no options.

## [04]-[IMPLEMENTATION_LAW]

[AWS_TOPOLOGY]:
- pipeline customizer: the tracer admission registers one `AWSTracingPipelineCustomizer` on the process-global `RuntimePipelineCustomizerRegistry.Instance`, so the hook covers every `AWSSDK.*` client the process constructs; later registrations with the same customizer name are ignored
- span shape: `Client`-kind span minted where the source name starts with `AWSSDK`, one per SDK operation, backed by the per-service `ActivitySource`
- source and meter names: `AWSSDK.*` wildcard — spans and metrics carry per-service source names, never one package-wide source

[STACKING]:
- `AWSSDK.Core`: instrumentation hooks the runtime pipeline every `AWSSDK.S3`, `AWSSDK.KeyManagementService`, and other service client shares — no per-client wiring, the customizer registration reaches them all.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddAWSInstrumentation` is the complete verb; a bare `AddSource("AWSSDK.*")` subscribes the source but never registers the customizer, so no span mints — never a bare `AddSource` shim.
- `OpenTelemetry.Extensions.AWS`: companion holding `AWSXRayIdGenerator`/`AWSXRayPropagator`; admitted at the root only when the trace backend is X-Ray, disjoint from the span-emission this package owns.

[LOCAL_ADMISSION]:
- Composition-root-only at the host root; the customizer registry is process-global, and the first same-name registration owns options for every app because later registrations are ignored.
- `SuppressDownstreamInstrumentation` trades the SDK's inner `HttpClient` span for a single service span — set where the HTTP instrumentation otherwise duplicates every AWS call.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AWS`
- Owns: `AWSSDK.*` client-call spans and metrics via the runtime pipeline customizer at the composition root
- Accept: `AddAWSInstrumentation` on the tracer and meter builders at the app root
- Reject: per-app customizer admission; hand-rolled aws-semconv spans over SDK operations; a bare `AddSource`/`AddMeter` without the customizer registration
