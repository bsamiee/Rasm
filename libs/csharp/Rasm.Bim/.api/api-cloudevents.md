# [RASM_BIM_API_CLOUDEVENTS]

`Rasm.Persistence` owns the CloudEvents envelope algebra and the `System.Text.Json` codec for this branch at `libs/csharp/Rasm.Persistence/.api/api-cloudevents.md`, so Bim registers that surface rather than re-tabling it. This partition holds the Bim EMIT lacing alone: the `BimEvent` lowering onto one `CloudEvent`, the source-generated `JsonSerializerContext` payload projection, the NodaTime `time` seal, and the W3C trace-continuity extension rows — Persistence owns every transport binding and Bim mints none.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: Bim emit partition of the CloudEvents distribution
- packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson` (both Apache-2.0, direct `PackageReference`); the Kafka, MQTT, and AMQP bindings are absent from this folder's closure
- assembly/namespace: as catalogued at the Persistence owner; Bim reaches `CloudNative.CloudEvents` and `.Extensions` alone
- rail: event envelope

- Registers the CloudEvents envelope and codec core(`libs/csharp/Rasm.Persistence/.api/api-cloudevents.md`): `CloudEvent` and its typed attribute map, `CloudEventAttribute`/`CloudEventAttributeType`, `CloudEventsSpecVersion` with its per-attribute singletons and rosters, `ContentMode`, the abstract `CloudEventFormatter`, the `Partitioning`/`Sampling`/`Sequence` standard extensions, `MimeUtilities`/`BinaryDataUtilities`/`Validation`, and the `JsonEventFormatter`/`JsonEventFormatter<T>` structured, binary, and batch codec all resolve there — a member verified against that catalogue is verified for this partition, and re-tabling one here forks the branch's envelope truth.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The extension roster handed to the ctor and to every decode is the wire contract, so Bim declares `traceparent` and `tracestate` as two `CloudEventAttribute.CreateExtension` String rows once and passes that same roster at both ends; an undeclared extension decodes as a string-typed surprise row.

[STACKING]:
- `NodaTime`(`libs/csharp/.api/api-nodatime.md`): the `time` attribute (`CloudEventAttributeType.Timestamp`, RFC 3339 `DateTimeOffset`) takes a NodaTime `Instant` through `Instant.ToDateTimeOffset()` at the seal, never a formatted string.
- `JsonEventFormatter` composes `System.Text.Json`: a source-generated `JsonSerializerContext` projects the domain payload to a `JsonElement` set as `Data`, and the structured-mode body carries it verbatim with zero reflection metadata; on decode `data` lands back as a `JsonElement` the same context deserializes, and `JsonEventFormatter<T>` serializes `Data` as `T` directly through supplied `JsonSerializerOptions`.
- Distributed-tracing continuity stamps the two declared rows from `Activity.Current.Id`/`.TraceStateString` under W3C id format — the declared-attribute pattern the SDK's own `Partitioning`/`Sampling`/`Sequence` helpers hold, extended in place rather than forked per transport.

[LOCAL_ADMISSION]:
- Envelope shape and JSON serialization enter through the registered core; the transport bindings consume the same `CloudEventFormatter` instance at the app tier and never enter this folder, and the formatter is minted once as a `static readonly` codec identity every transport shares.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the `BimEvent`-to-`CloudEvent` emit lacing — the declared extension roster, the source-generated `Data` projection, the `Instant` `time` seal, and the W3C trace rows
- Accept: a `BimEvent` lowered onto a `CloudEvent` with that roster, `Data` carried as a source-generated `JsonElement`, and JSON emit or decode through the shared `JsonEventFormatter`
- Reject: a member roster for either package here, a hand-built JSON envelope where `JsonEventFormatter` owns structured mode, a formatted timestamp string where `Instant.ToDateTimeOffset()` seals the `time` attribute, a per-transport formatter instance where one `static readonly` identity serves all, and any transport binding the Persistence owner holds
