# [RASM_PERSISTENCE_API_SCHEMAREGISTRY]

`Confluent.SchemaRegistry` supplies the registry control-plane the three `Confluent.SchemaRegistry.Serdes.*` data-plane serdes share: the `ISchemaRegistryClient` REST contract and its `CachedSchemaRegistryClient` implementation (subject register/lookup, schema-id and GUID resolution, compatibility level read/update, latest-by-metadata lookup), the `Schema`/`RegisteredSchema` schema model with `References`/`Metadata`/`RuleSet`, the `SchemaId` wire-prefix struct plus the `ISchemaIdEncoder`/`ISchemaIdDecoder` framing family (Confluent magic-byte prefix vs. Kafka-header id placement), the `SubjectNameStrategy`/`ReferenceSubjectNameStrategy` naming axes, the `Compatibility` evolution enum, and the data-contract rule engine (`RuleSet`/`Rule`/`RuleMode`/`RulePhase`, `IRuleExecutor`/`IRuleAction`, `RuleRegistry`) that backs client-side field-level encryption and migration. It is the registry leg of the `Version/egress#EGRESS_SINK` rail: the serde codec is built once against one `ISchemaRegistryClient`, the registry governs schema evolution out-of-band, and only the registered schema id (not the schema) rides each Kafka payload.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry`
- package: `Confluent.SchemaRegistry`
- version: `2.14.2`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry`
- namespace: `Confluent.SchemaRegistry`
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0` (the Confluent 2.14.2 line tops out at `net8.0`, never `net10.0`)
- managed: pure-managed AnyCPU, no native asset; REST/JSON client only
- transitive: `Newtonsoft.Json` (the `Schema`/`Rule`/`Compatibility` DataContracts carry `Newtonsoft.Json` `[JsonConverter]`/`StringEnumConverter` attributes — the registry wire model is Newtonsoft, not System.Text.Json); `System.Net.Http` for the `RestService`
- rail: schema-registry control-plane (governs `cdc-egress`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client contract family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :----------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `ISchemaRegistryClient`        | client contract  | register/lookup/compat/latest/cache-clear/association, `IDisposable` |
|  [02]   | `CachedSchemaRegistryClient`   | client impl      | the only shipped client; bounded LRU schema cache over the REST service |
|  [03]   | `RestService`                  | transport        | `public class : IRestService` — the low-level REST surface behind the cached client (`IRestService` itself is `internal`) |
|  [04]   | `SchemaRegistryConfig`         | client config    | `: IEnumerable<KeyValuePair<string,string>>`; `Url`, retries, SSL, auth, cache TTL |
|  [05]   | `IWebProxy` (BCL)              | proxy            | optional outbound proxy on the client ctor      |

[PUBLIC_TYPE_SCOPE]: schema model family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                             |
| :-----: | :------------------ | :-------------- | :------------------------------------------------- |
|  [01]   | `Schema`            | unregistered    | `SchemaString`/`SchemaType`/`References`/`Metadata`/`RuleSet` (carries `Subject`/`Version`/`Id` once registered) |
|  [02]   | `RegisteredSchema`  | registered      | `: Schema`; adds the `Guid` (the GUID-keyed lookup id) — `Id`/`Subject`/`Version` live on the base `Schema` |
|  [03]   | `SchemaReference`   | reference       | named cross-schema dependency (`Name`/`Subject`/`Version`) |
|  [04]   | `Metadata`          | schema metadata | `Tags`/`Properties`/`Sensitive` (the sensitive-field set the CSFLE rule reads) |
|  [05]   | `SchemaType`        | type enum       | `Avro` / `Protobuf` / `Json`                       |
|  [06]   | `Compatibility`     | evolution enum  | `None`/`Forward`/`Backward`/`Full` (+`*Transitive`) |
|  [07]   | `ServerConfig`      | subject config  | server-side compatibility/rule defaults            |

`SchemaString` is the public `string` property on `Schema` (the server schema-string), not a public type — the `SchemaString` type itself is `internal`; `CompatibilityCheck` is likewise `internal`, never a consumer-facing result, so `IsCompatibleAsync` returns a plain `bool`.

[PUBLIC_TYPE_SCOPE]: wire-id framing family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [RAIL]                                             |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `SchemaId`                     | wire id struct  | `SchemaType`/`Id`/`Guid`/`MessageIndexes`; `VALUE_SCHEMA_ID_HEADER`/`KEY_SCHEMA_ID_HEADER`/`MAGIC_BYTE_V0`/`MAGIC_BYTE_V1` consts |
|  [02]   | `ISchemaIdEncoder`             | encode contract | `Encode(Span<byte>, ref SerializationContext, ref SchemaId)` + `CalculateSize(ref SchemaId)` |
|  [03]   | `ISchemaIdDecoder`             | decode contract | `Decode(ReadOnlyMemory<byte>, SerializationContext, ref SchemaId)` |
|  [04]   | `SchemaIdSerializerStrategy`   | encode enum     | `Header` / `Prefix` — the PUBLIC encoder selector  |
|  [05]   | `SchemaIdDeserializerStrategy` | decode enum     | `Dual` / `Prefix` — the PUBLIC decoder selector    |

The concrete encoders/decoders are `internal`, never `new`-able by a consumer: `PrefixSchemaIdEncoder` (the magic-byte `[0x00][int32 id]` prefix, the serde default) and `HeaderSchemaIdEncoder` (writes the id GUID into the `__value_schema_id`/`__key_schema_id` Kafka header) are selected by `SchemaIdSerializerStrategy`; `PrefixSchemaIdDecoder` and `DualSchemaIdDecoder` (header-or-prefix, the deserializer default) are selected by `SchemaIdDeserializerStrategy`. A serde sets the strategy on its config; it never references the encoder/decoder class. `ISchemaIdEncoder`/`ISchemaIdDecoder` are the public extension contracts for a bespoke framing.

[PUBLIC_TYPE_SCOPE]: subject naming family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :---------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `SubjectNameStrategy`               | naming enum      | `Topic`/`Record`/`TopicRecord`/`Associated`/`None` |
|  [02]   | `ReferenceSubjectNameStrategy`      | ref-naming enum  | `ReferenceName`/`Qualified`/`Custom`            |
|  [03]   | `SubjectNameStrategyExtensions`     | resolver         | `ConstructKeySubjectName`/`ConstructValueSubjectName(topic, recordType?)` + `ToDelegate`/`ToAsyncDelegate` — how the enum becomes a subject |
|  [04]   | `ReferenceSubjectNameStrategyExtensions` | ref resolver | `ToDelegate(strategy, custom?)` + `GetQualifiedSubjectName`/`GetReferenceNameSubjectName(context, referenceName)` |
|  [05]   | `ICustomReferenceSubjectNameStrategy` | ref hook       | custom reference-subject resolver (`Custom` mode) |
|  [06]   | `SubjectNameStrategyDelegate`       | delegate         | `(context, recordType) -> subject`              |
|  [07]   | `AsyncSubjectNameStrategyDelegate`  | async delegate   | async subject resolution (the form `AsyncSerde.subjectNameStrategy` holds) |
|  [08]   | `ReferenceSubjectNameStrategyDelegate` | delegate      | reference-subject resolution                    |
|  [09]   | `AssociatedNameStrategy`            | strategy impl    | registry-resolved subject (`Associated` mode, async-only) |

[PUBLIC_TYPE_SCOPE]: data-contract rule family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                             |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `RuleSet`           | rule bundle      | `MigrationRules`/`DomainRules`/`EncodingRules`     |
|  [02]   | `Rule`              | one rule         | name, kind, mode, type, expr, tags, params         |
|  [03]   | `RuleKind`          | rule-kind enum   | `Transform` / `Condition`                          |
|  [04]   | `RuleMode`          | rule-mode enum   | `Upgrade`/`Downgrade`/`UpDown`/`Read`/`Write`/`WriteRead` |
|  [05]   | `RulePhase`         | phase enum       | `Migration` / `Domain` / `Encoding`                |
|  [06]   | `IRuleExecutor`     | executor contract| applies a transform/condition rule                 |
|  [07]   | `IRuleAction`       | action contract  | on-success / on-failure rule action                |
|  [08]   | `FieldRuleExecutor` | field executor   | `abstract : IRuleExecutor`; `NewTransform(ctx)` + `Type()` — the CSFLE field-transform base |
|  [09]   | `IFieldTransform`   | field transform  | `: IDisposable`; per-field transform hook the executor builds |
|  [10]   | `FieldTransformer`  | transform delegate | the `RuleContext.FieldTransformer` delegate the executor invokes to walk fields |
|  [11]   | `RuleContext`       | rule context     | `Source`/`Target`/`Subject`/`Topic`/`RuleMode`/`FieldTransformer`; nested `RuleContext.FieldContext` (`FullName`/`Name`/`Tags`); `CurrentField()`/`EnterField(...)` |
|  [12]   | `IRuleBase`         | rule base contract | `: IDisposable`; `Configure(config, client?)`/`Type()` shared by executor + action |
|  [13]   | `RuleOverride`      | rule override    | per-rule type/action override registered via `RuleRegistry.RegisterOverride` |
|  [14]   | `RuleRegistry`      | rule registry    | `RegisterExecutor`/`RegisterAction`/`RegisterOverride` (+ static `RegisterRule*`), `TryGet*`, `GlobalInstance` |
|  [15]   | `RuleException` / `RuleConditionException` | rule fault | `RuleException : Exception` (rule failure); `RuleConditionException : RuleException` (condition-rule violation) |

[PUBLIC_TYPE_SCOPE]: authentication and error family
- rail: schema-registry control-plane

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [RAIL]                                          |
| :-----: | :-------------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `IAuthenticationHeaderValueProvider`    | auth contract   | per-request authorization header                |
|  [02]   | `IAuthenticationBearerHeaderValueProvider` | bearer contract | OAuth bearer header provider                  |
|  [03]   | `BasicAuthenticationHeaderValueProvider` | basic auth     | `BasicAuthUserInfo` HTTP basic                  |
|  [04]   | `BearerAuthenticationHeaderValueProvider` | bearer auth    | OAuth client-credentials token fetch            |
|  [05]   | `StaticBearerAuthenticationHeaderValueProvider` | static token | preminted bearer token                        |
|  [06]   | `AzureIMDSBearerAuthenticationHeaderValueProvider` | Azure IMDS | managed-identity token from Azure IMDS        |
|  [07]   | `AuthCredentialsSource`                 | basic-auth enum | basic-auth credential source                    |
|  [08]   | `BearerAuthCredentialsSource`           | bearer enum     | bearer-auth credential source                   |
|  [09]   | `SchemaRegistryException`               | client fault    | REST failure with `ErrorCode`/`Status`          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: schema-registry control-plane

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new CachedSchemaRegistryClient(config)`                     | ctor           | builds from `SchemaRegistryConfig` K/V pairs    |
|  [02]   | `new CachedSchemaRegistryClient(config, authProvider, proxy?)` | ctor         | explicit auth provider plus optional `IWebProxy` |
|  [03]   | `new SchemaRegistryConfig { Url = ... }`                     | object init    | bootstrap URL, retries, SSL, auth, subject strategy |
|  [04]   | `client.Dispose()`                                           | lifecycle      | `ISchemaRegistryClient : IDisposable`; one client shared across serdes |

[ENTRYPOINT_SCOPE]: register and lookup
- rail: schema-registry control-plane

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `RegisterSchemaAsync(subject, schema, normalize?)`     | register       | registers; returns the assigned schema id       |
|  [02]   | `RegisterSchemaWithResponseAsync(subject, schema, normalize?)` | register | returns the full `RegisteredSchema` (id+guid+version) |
|  [03]   | `GetSchemaIdAsync(subject, schema, normalize?)`        | lookup         | resolves an existing id without registering     |
|  [04]   | `LookupSchemaAsync(subject, schema, ignoreDeleted, normalize?)` | lookup | returns the matching `RegisteredSchema`         |
|  [05]   | `GetSchemaAsync(id, format?)`                          | fetch          | fetches a `Schema` by global id                 |
|  [06]   | `GetSchemaBySubjectAndIdAsync(subject, id, format?)`   | fetch          | id scoped to a subject                          |
|  [07]   | `GetSchemaByGuidAsync(guid, format?)`                  | fetch          | fetches by schema GUID                          |
|  [08]   | `GetRegisteredSchemaAsync(subject, version, ignoreDeleted?)` | fetch    | fetches a `RegisteredSchema` by version         |
|  [09]   | `GetLatestSchemaAsync(subject)`                        | fetch          | the latest registered version                   |
|  [10]   | `GetLatestWithMetadataAsync(subject, metadata, ignoreDeleted)` | fetch  | latest version whose `Metadata` matches the K/V (data-contract pin) |

[ENTRYPOINT_SCOPE]: compatibility and inventory
- rail: schema-registry control-plane

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `IsCompatibleAsync(subject, schema)`            | compat probe   | tests a candidate schema against the subject (returns `bool`) |
|  [02]   | `GetCompatibilityAsync(subject?)`               | compat read    | reads the level (subject or global default)     |
|  [03]   | `UpdateCompatibilityAsync(compatibility, subject?)` | compat write | sets the level for a subject or globally        |
|  [04]   | `GetAllSubjectsAsync()`                         | inventory      | every registered subject name                   |
|  [05]   | `GetSubjectVersionsAsync(subject)`              | inventory      | version list for a subject                      |
|  [06]   | `ClearCaches()` / `ClearLatestCaches()`         | cache control  | drop the in-process schema / latest-version caches on the long-lived shared client without disposing it |
|  [07]   | `CreateAssociationAsync(request)` / `GetAssociationsByResourceNameAsync(...)` / `DeleteAssociationsAsync(...)` | governance | the resource-association (data-governance lineage) surface, distinct from schema register/lookup |

[ENTRYPOINT_SCOPE]: schema, id, and rule construction
- rail: schema-registry control-plane

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new Schema(schemaString, schemaType)`                     | ctor           | unregistered schema, empty references           |
|  [02]   | `new Schema(schemaString, references, schemaType, metadata, ruleSet)` | ctor | full schema with references, metadata, rules    |
|  [03]   | `new SchemaReference(name, subject, version)`              | ctor           | a cross-schema dependency                       |
|  [04]   | `new SchemaId(schemaType, id, guid)`                       | ctor           | wire-id descriptor (int id and/or GUID)         |
|  [05]   | `schemaId.FromBytes(payload)`                              | decode         | strips the id framing, returns the value slice  |
|  [06]   | `new RuleSet(migrationRules, domainRules, encodingRules)`  | ctor           | the three rule phases bundled                   |
|  [07]   | `ruleSet.GetRules(RulePhase.Domain)`                       | accessor       | the rules for one phase                         |
|  [08]   | `RuleRegistry.RegisterRuleExecutor(executor)`              | static         | registers a global `IRuleExecutor` (e.g. CSFLE) |
|  [09]   | `RuleRegistry.RegisterRuleAction(action)`                  | static         | registers a global on-failure `IRuleAction`     |
|  [10]   | `ruleRegistry.TryGetExecutor(name, out executor)`          | instance       | resolves a per-serde executor by name           |

## [04]-[IMPLEMENTATION_LAW]

[REGISTRY_TOPOLOGY]:
- single namespace: `Confluent.SchemaRegistry` — client, schema model, wire-id framing, naming strategy, rule engine, auth, errors.
- the wire model is `Newtonsoft.Json`: `Schema`, `Rule`, `Compatibility`, `RuleMode`, `RuleKind` carry `[DataContract]` plus `[JsonConverter(StringEnumConverter)]`; the registry REST body is never System.Text.Json, so a System.Text.Json `JsonSerializerOptions` never touches the registry leg (it touches only the application payload codec).
- `CachedSchemaRegistryClient` is the only shipped `ISchemaRegistryClient` (ctors: `(config)`, `(config, proxy)`, `(config, authProvider, proxy?)`); it owns a bounded LRU schema cache (`DefaultMaxCachedSchemas = 1000`) plus a latest-version TTL (`DefaultLatestCacheTtlSecs = -1` = no expiry) so steady-state register/lookup is in-process after warmup. Default REST policy: `DefaultTimeout = 30000`, `DefaultMaxRetries = 3`, `DefaultRetriesWaitMs = 1000`, `DefaultRetriesMaxWaitMs = 20000`, `DefaultMaxConnectionsPerServer = 20`. `ClearCaches()`/`ClearLatestCaches()` drop the caches on the live shared client (a controlled re-pin after an out-of-band registry mutation) without disposing it.
- `SchemaRegistryConfig.Url` accepts a comma-separated failover list; the `RestService` round-robins instances under retry, distinct from the librdkafka broker list on the `Confluent.Kafka` data plane. Bearer auth is the full `BearerAuth*` config block (`BearerAuthClientId`/`ClientSecret`/`Scope`/`TokenEndpointUrl`/`LogicalCluster`/`IdentityPoolId`), basic auth is `BasicAuthUserInfo`.
- the wire id is framed by an `ISchemaIdEncoder` chosen with the `SchemaIdSerializerStrategy` enum on the serde config — never by referencing an encoder class (the concrete encoders are `internal`): `Prefix` selects the magic-byte `[0x00][int32 schema id]` prefix (`AsyncSerializer.schemaIdEncoder` defaults to it); `Header` moves the id GUID into the `__value_schema_id`/`__key_schema_id` Kafka header, leaving the value payload prefix-free. The deserializer picks the decoder with `SchemaIdDeserializerStrategy`: `Dual` (the base default) reads header-or-prefix so a topic migrates prefix->header without a flag day; `Prefix` is prefix-only. `SchemaId` carries `Id`/`Guid`/`MessageIndexes` (the Protobuf message-index list); `CalculateIdSize`/`CalculateGuidSize` size the framing and `SchemaId.FromBytes` reverses it.
- the rule engine is the data-contract surface: `RuleSet` carries `MigrationRules` (cross-version `Upgrade`/`Downgrade`), `DomainRules` (`Write`/`Read` field transforms — the CSFLE encrypt/decrypt seam), and `EncodingRules`. `RuleRegistry.GlobalInstance` is the default executor/action lookup every serde reads unless a per-serde `RuleRegistry` is passed.

[LOCAL_ADMISSION]:
- The egress rail builds exactly one `CachedSchemaRegistryClient` per registry endpoint from one `SchemaRegistryConfig`, shared by every `Confluent.SchemaRegistry.Serdes.*` codec on that cluster; the client is disposed once at rail teardown, never per message and never per topic.
- `SubjectNameStrategy` is fixed at serde-config time, not per call: `Topic` (`<topic>-value`) for single-type topics; `TopicRecord`/`Record` for the multi-event-type op-log topic so a `BimCommitted` and a `GeometryRebaked` event coexist on one topic each governed by its own subject. The strategy is the same axis the `api-redaction` `DataClassification` taxonomy reads when choosing the subject for a redacted op payload.
- Production register policy disables `AutoRegisterSchemas` on the serde and registers schemas out-of-band through `RegisterSchemaWithResponseAsync` under the governed `Compatibility` level (`Backward`/`FullTransitive` for the durable changefeed), so an incompatible producer schema is rejected at deploy time, never silently auto-registered. The consumer pins reader behaviour through `GetLatestWithMetadataAsync` against a `Metadata` tag rather than trusting the writer schema id blindly.
- Field-level encryption (CSFLE) routes through the rule engine, not a bespoke crypto pass: a `DomainRule` of `RuleMode.WriteRead` referencing a field-encryption `IRuleExecutor` (registered once via `RuleRegistry.RegisterRuleExecutor`) wraps/unwraps per-field DEKs against the admitted `AWSSDK.KeyManagementService`/`Azure.Security.KeyVault.Keys`/`Google.Cloud.Kms.V1` KMS clients, reusing the same `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` seam (the `KmsProvider` axis owned at `Element/identity#AUTHORITY`). The `Metadata` sensitive-field set marks which fields the rule encrypts; the rule executor is the single load-bearing seam binding registry-governed encryption to the KMS authority.
- `SchemaIdSerializerStrategy.Header` is the chosen framing when the value payload must stay schema-id-free for a downstream non-Confluent consumer (the id rides a header instead), pairing with the `CloudNative.CloudEvents.Kafka` header binding on the same `Message<TKey, TValue>` so registry id, CloudEvents attributes, and trace context all ride `Headers`.
- Bearer auth (`BearerAuthCredentialsSource`/`BearerAuthenticationHeaderValueProvider`) mints the registry token from the same runtime token authority the Kafka SASL/OAUTHBEARER refresh uses; `AzureIMDSBearerAuthenticationHeaderValueProvider` is the managed-identity path when the registry is Azure-hosted. Registry auth is configured once on the client, never per request.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry`
- Owns: the registry REST control-plane — subject register/lookup, schema-id/GUID resolution, compatibility governance, latest-by-metadata data-contract pinning, wire-id framing, subject naming strategy, and the client-side data-contract rule engine (migration + CSFLE).
- Accept: one shared `CachedSchemaRegistryClient` per endpoint, fixed `SubjectNameStrategy`, out-of-band `RegisterSchemaWithResponseAsync` under a governed `Compatibility` level, rule-engine CSFLE through `RuleRegistry`, and `DualSchemaIdDecoder` for prefix->header migration windows.
- Reject: a per-message or per-topic client, `AutoRegisterSchemas` on the durable changefeed producer, hand-rolled magic-byte framing in place of `ISchemaIdEncoder`, a bespoke field-encryption pass outside the rule engine, and treating the Newtonsoft registry wire model as a System.Text.Json surface.
