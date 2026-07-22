# [RASM_PERSISTENCE_API_SCHEMAREGISTRY]

`Confluent.SchemaRegistry` owns the registry REST control-plane every `Confluent.SchemaRegistry.Serdes.*` codec shares: subject register/lookup, schema-id and GUID resolution, compatibility governance, wire-id framing, subject naming, and the client-side data-contract rule engine backing field-level encryption and migration. It is the registry leg of the `Version/egress#EGRESS_SINK` rail — one codec builds once against one `ISchemaRegistryClient`, the registry governs evolution out-of-band, and only the registered schema id rides each Kafka payload.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry`
- package: `Confluent.SchemaRegistry`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry`
- namespace: `Confluent.SchemaRegistry`
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0`
- managed: pure-managed AnyCPU, no native asset; REST/JSON client only
- transitive: `Newtonsoft.Json` (registry wire model), `Microsoft.Extensions.Caching.Memory` (latest-cache TTL), BCL `HttpClient` (`RestService` transport)
- rail: schema-registry control-plane governing `cdc-egress`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client contract family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :--------------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `ISchemaRegistryClient`      | client contract | register/lookup/compat/latest/cache-clear/association, `IDisposable`                |
|  [02]   | `CachedSchemaRegistryClient` | client impl     | the only shipped client; bounded LRU schema cache over the REST service             |
|  [03]   | `RestService`                | transport       | `public class : IRestService`; low-level REST surface, `IRestService` is `internal` |
|  [04]   | `SchemaRegistryConfig`       | client config   | `: IEnumerable<KeyValuePair<string,string>>`; `Url`, retries, SSL, auth, cache TTL  |
|  [05]   | `IWebProxy` (BCL)            | proxy           | optional outbound proxy on the client ctor                                          |

[PUBLIC_TYPE_SCOPE]: schema model family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                                             |
| :-----: | :----------------- | :-------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Schema`           | unregistered    | `SchemaString`/`SchemaType`/`References`/`Metadata`/`RuleSet`                            |
|  [02]   | `RegisteredSchema` | registered      | `: Schema`; carries `Id`/`Subject`/`Version`/`Guid` as its own registered-identity props |
|  [03]   | `SchemaReference`  | reference       | named cross-schema dependency (`Name`/`Subject`/`Version`)                               |
|  [04]   | `Metadata`         | schema metadata | `Tags`/`Properties`/`Sensitive` (the sensitive-field set the CSFLE rule reads)           |
|  [05]   | `SchemaType`       | type enum       | `Avro` / `Protobuf` / `Json`                                                             |
|  [06]   | `Compatibility`    | evolution enum  | `None`/`Forward`/`Backward`/`Full` (+`*Transitive`)                                      |
|  [07]   | `ServerConfig`     | subject config  | server-side compatibility/rule defaults                                                  |

- `Schema.SchemaString`: a public `string` carries the server schema-string; the `SchemaString` and `CompatibilityCheck` types stay `internal`, so `IsCompatibleAsync` returns a plain `bool`.

[PUBLIC_TYPE_SCOPE]: wire-id framing family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :----------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `SchemaId`                     | wire id struct  | `SchemaType`/`Id`/`Guid`/`MessageIndexes`                          |
|  [02]   | `ISchemaIdEncoder`             | encode contract | `Encode(Span<byte>, ref SerializationContext, ref SchemaId)`       |
|  [03]   | `ISchemaIdDecoder`             | decode contract | `Decode(ReadOnlyMemory<byte>, SerializationContext, ref SchemaId)` |
|  [04]   | `SchemaIdSerializerStrategy`   | encode enum     | `Header` / `Prefix` — the public encoder selector                  |
|  [05]   | `SchemaIdDeserializerStrategy` | decode enum     | `Dual` / `Prefix` — the public decoder selector                    |
|  [06]   | `SchemaIdStrategyExtensions`   | strategy folder | `ToEncoder`/`ToDeserializer` fold a strategy enum to its codec     |

- `SchemaId`: exposes framing consts `VALUE_SCHEMA_ID_HEADER`/`KEY_SCHEMA_ID_HEADER` and `MAGIC_BYTE_V0`/`MAGIC_BYTE_V1`; every encoder/decoder method trails a `ref SchemaId`, sized by `ISchemaIdEncoder.CalculateSize(ref SchemaId)`. `ISchemaIdEncoder`/`ISchemaIdDecoder` are the public extension contracts for bespoke framing.

[PUBLIC_TYPE_SCOPE]: subject naming family

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]   | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `SubjectNameStrategy`                    | naming enum     | `Topic`/`Record`/`TopicRecord`/`Associated`/`None`                    |
|  [02]   | `ReferenceSubjectNameStrategy`           | ref-naming enum | `ReferenceName`/`Qualified`/`Custom`                                  |
|  [03]   | `SubjectNameStrategyExtensions`          | resolver        | `ToAsyncDelegate(strategy, client?, config?)`                         |
|  [04]   | `ReferenceSubjectNameStrategyExtensions` | ref resolver    | `ToDelegate`, `GetQualifiedSubjectName`/`GetReferenceNameSubjectName` |
|  [05]   | `ICustomReferenceSubjectNameStrategy`    | ref hook        | custom reference-subject resolver (`Custom` mode)                     |
|  [06]   | `SubjectNameStrategyDelegate`            | delegate        | `(context, recordType) -> subject`                                    |
|  [07]   | `AsyncSubjectNameStrategyDelegate`       | async delegate  | async form `AsyncSerde.subjectNameStrategy` holds                     |
|  [08]   | `ReferenceSubjectNameStrategyDelegate`   | delegate        | reference-subject resolution                                          |
|  [09]   | `AssociatedNameStrategy`                 | strategy impl   | registry-resolved subject (`Associated` mode, async-only)             |

- `SubjectNameStrategyExtensions.ToAsyncDelegate`: its async form resolves `SubjectNameStrategy.Associated` through the registry; `*Delegate` types are the resolved `(context, recordType) -> subject` closures.

[PUBLIC_TYPE_SCOPE]: data-contract rule family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :----------------------- | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `RuleSet`                | rule bundle        | `MigrationRules`/`DomainRules`/`EncodingRules`/`EnableAt`; `GetRules`/`HasRules`   |
|  [02]   | `Rule`                   | one rule           | one evolution rule record; fields in lead                                          |
|  [03]   | `Migration`              | migration descr    | `RuleMode`/`Source`/`Target` — one upgrade/downgrade hop between two schemas       |
|  [04]   | `RuleKind`               | rule-kind enum     | `Transform` / `Condition`                                                          |
|  [05]   | `RuleMode`               | rule-mode enum     | `Upgrade`/`Downgrade`/`UpDown`/`Read`/`Write`/`WriteRead`                          |
|  [06]   | `RulePhase`              | phase enum         | `Migration` / `Domain` / `Encoding`                                                |
|  [07]   | `IRuleExecutor`          | executor contract  | `: IRuleBase`; `Transform(ctx, message)` applies a transform/condition rule        |
|  [08]   | `IRuleAction`            | action contract    | `: IRuleBase`; `Run(ctx, message, exception?)` on-success / on-failure action      |
|  [09]   | `ErrorAction`            | fail action        | shipped `IRuleAction`, `Type() == "ERROR"`                                         |
|  [10]   | `NoneAction`             | no-op action       | shipped `IRuleAction`, `Type() == "NONE"`                                          |
|  [11]   | `FieldRuleExecutor`      | field executor     | `abstract : IRuleExecutor`; `NewTransform(ctx)`/`Type()` CSFLE base                |
|  [12]   | `IFieldTransform`        | field transform    | `: IDisposable`; `Init(ctx)`/`Transform(ctx, fieldCtx, fieldValue)` per-field hook |
|  [13]   | `FieldTransformer`       | transform delegate | `RuleContext.FieldTransformer` delegate walking fields                             |
|  [14]   | `RuleContext`            | rule context       | per-rule evaluation context; properties in lead                                    |
|  [15]   | `IRuleBase`              | rule base contract | `: IDisposable`; `Configure(config, client?)`/`Type()`                             |
|  [16]   | `RuleOverride`           | rule override      | per-rule type/action override via `RuleRegistry.RegisterOverride`                  |
|  [17]   | `RuleRegistry`           | rule registry      | executor/action/override registry; members in lead                                 |
|  [18]   | `RuleException`          | rule fault         | `: Exception`; a rule failed                                                       |
|  [19]   | `RuleConditionException` | condition fault    | `: RuleException`; a condition rule was violated                                   |

- `Rule` fields: `Name` `Doc` `Kind` `Mode` `Type` `Expr` `Tags` `Params` `OnSuccess` `OnFailure` `Disabled`.
- `RuleContext` props: `Source` `Target` `Subject` `Topic` `Headers` `IsKey` `RuleMode` `Rule` `Rules` `Index` `FieldTransformer` `CustomData`; walks the nested `FieldContext` (`FullName`/`Name`/`Type`/`Tags` + `IsPrimitive()`) via `CurrentField()`/`EnterField(...)`/`GetParameter(key)`/`GetTags(fullName)`.
- `RuleRegistry` members: static `RegisterRuleExecutor`/`RegisterRuleAction`/`RegisterRuleOverride`, instance `RegisterExecutor`/`RegisterAction`/`RegisterOverride`, `Get*`/`TryGet*`, and the `GlobalInstance` singleton.

[PUBLIC_TYPE_SCOPE]: authentication and error family

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [CAPABILITY]                           |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------- |
|  [01]   | `IAuthenticationHeaderValueProvider`               | auth contract   | per-request authorization header       |
|  [02]   | `IAuthenticationBearerHeaderValueProvider`         | bearer contract | OAuth bearer header provider           |
|  [03]   | `BasicAuthenticationHeaderValueProvider`           | basic auth      | `BasicAuthUserInfo` HTTP basic         |
|  [04]   | `BearerAuthenticationHeaderValueProvider`          | bearer auth     | OAuth client-credentials token fetch   |
|  [05]   | `StaticBearerAuthenticationHeaderValueProvider`    | static token    | preminted bearer token                 |
|  [06]   | `AzureIMDSBearerAuthenticationHeaderValueProvider` | Azure IMDS      | managed-identity token from Azure IMDS |
|  [07]   | `AuthCredentialsSource`                            | basic-auth enum | basic-auth credential source           |
|  [08]   | `BearerAuthCredentialsSource`                      | bearer enum     | bearer-auth credential source          |
|  [09]   | `SchemaRegistryException`                          | client fault    | REST failure with `ErrorCode`/`Status` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new CachedSchemaRegistryClient(config)`                       | ctor     | builds from `SchemaRegistryConfig` K/V pairs         |
|  [02]   | `new CachedSchemaRegistryClient(config, authProvider, proxy?)` | ctor     | explicit auth provider plus optional `IWebProxy`     |
|  [03]   | `new SchemaRegistryConfig { Url = ... }`                       | ctor     | bootstrap URL, retries, SSL, auth, subject strategy  |
|  [04]   | `client.Dispose()`                                             | instance | `ISchemaRegistryClient : IDisposable`, shared client |

[ENTRYPOINT_SCOPE]: register and lookup
- [SHAPE]: instance

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `RegisterSchemaAsync(subject, schema, normalize?)`              | registers; returns the assigned schema id             |
|  [02]   | `RegisterSchemaWithResponseAsync(subject, schema, normalize?)`  | returns the full `RegisteredSchema` (id+guid+version) |
|  [03]   | `GetSchemaIdAsync(subject, schema, normalize?)`                 | resolves an existing id without registering           |
|  [04]   | `LookupSchemaAsync(subject, schema, ignoreDeleted, normalize?)` | returns the matching `RegisteredSchema`               |
|  [05]   | `GetSchemaAsync(id, format?)`                                   | fetches a `Schema` by global id                       |
|  [06]   | `GetSchemaBySubjectAndIdAsync(subject, id, format?)`            | id scoped to a subject                                |
|  [07]   | `GetSchemaByGuidAsync(guid, format?)`                           | fetches by schema GUID                                |
|  [08]   | `GetRegisteredSchemaAsync(subject, version, ignoreDeleted?)`    | fetches a `RegisteredSchema` by version               |
|  [09]   | `GetLatestSchemaAsync(subject)`                                 | the latest registered version                         |
|  [10]   | `GetLatestWithMetadataAsync(subject, metadata, ignoreDeleted)`  | latest version matching a `Metadata` K/V pin          |

[ENTRYPOINT_SCOPE]: compatibility and inventory
- [SHAPE]: instance

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `IsCompatibleAsync(subject, schema)`                | tests a candidate schema against the subject (returns `bool`) |
|  [02]   | `GetCompatibilityAsync(subject?)`                   | reads the level (subject or global default)                   |
|  [03]   | `UpdateCompatibilityAsync(compatibility, subject?)` | sets the level for a subject or globally                      |
|  [04]   | `GetAllSubjectsAsync()`                             | every registered subject name                                 |
|  [05]   | `GetSubjectVersionsAsync(subject)`                  | version list for a subject                                    |
|  [06]   | `ClearCaches()` / `ClearLatestCaches()`             | drop the in-process schema/latest caches without disposing    |
|  [07]   | `CreateAssociationAsync(request)`                   | registers a resource association                              |
|  [08]   | `GetAssociationsByResourceNameAsync(...)`           | lists associations by resource name                           |
|  [09]   | `DeleteAssociationsAsync(...)`                      | deletes resource associations                                 |

- `CreateAssociationAsync`: takes an `AssociationCreateOrUpdateRequest` (`ResourceName`/`ResourceNamespace`/`ResourceId`/`ResourceType` + `AssociationCreateOrUpdateInfo` rows) and returns an `AssociationResponse`; the `*AssociationAsync` surface is the data-governance lineage rail, distinct from schema register/lookup.

[ENTRYPOINT_SCOPE]: schema, id, and rule construction

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new Schema(schemaString, schemaType)`                                | ctor     | unregistered schema, empty references          |
|  [02]   | `new Schema(schemaString, references, schemaType, metadata, ruleSet)` | ctor     | full schema with references, metadata, rules   |
|  [03]   | `new SchemaReference(name, subject, version)`                         | ctor     | a cross-schema dependency                      |
|  [04]   | `new SchemaId(schemaType, id, guid)`                                  | ctor     | wire-id descriptor (int id and/or GUID)        |
|  [05]   | `schemaId.FromBytes(payload)`                                         | instance | strips the id framing, returns the value slice |
|  [06]   | `new RuleSet(migrationRules, domainRules, encodingRules)`             | ctor     | the three rule phases bundled                  |
|  [07]   | `ruleSet.GetRules(RulePhase.Domain)`                                  | instance | the rules for one phase                        |
|  [08]   | `RuleRegistry.RegisterRuleExecutor(executor)`                         | static   | registers a global `IRuleExecutor`             |
|  [09]   | `RuleRegistry.RegisterRuleAction(action)`                             | static   | registers a global on-failure `IRuleAction`    |
|  [10]   | `RuleRegistry.RegisterRuleOverride(ruleOverride)`                     | static   | registers a global per-rule override           |
|  [11]   | `ruleRegistry.TryGetExecutor(name, out executor)`                     | instance | resolves a per-serde executor by name          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One namespace `Confluent.SchemaRegistry` carries client, schema model, wire-id framing, naming strategy, rule engine, auth, and errors.
- `Newtonsoft.Json` is the registry wire model: `Schema`, `Rule`, `Compatibility`, `RuleMode`, `RuleKind` carry `[DataContract]` and `[JsonConverter(StringEnumConverter)]`, so a System.Text.Json `JsonSerializerOptions` never touches the registry leg — only the application payload codec.
- `CachedSchemaRegistryClient` is the only shipped `ISchemaRegistryClient`; it owns a bounded LRU schema cache (`DefaultMaxCachedSchemas = 1000`) and a latest-version TTL (`DefaultLatestCacheTtlSecs = -1`, no expiry) so steady-state register/lookup runs in-process after warmup. REST policy: `DefaultTimeout = 30000`, `DefaultMaxRetries = 3`, `DefaultRetriesWaitMs = 1000`, `DefaultRetriesMaxWaitMs = 20000`, `DefaultMaxConnectionsPerServer = 20`. `ClearCaches()`/`ClearLatestCaches()` drop the caches on the live shared client without disposing it.
- `SchemaRegistryConfig.Url` accepts a comma-separated failover list the `RestService` round-robins under retry. Bearer auth is the `BearerAuth*` block (`BearerAuthClientId`/`ClientSecret`/`Scope`/`TokenEndpointUrl`/`TokenEndpointQuery`/`LogicalCluster`/`IdentityPoolId`, or a preminted `BearerAuthToken`); basic auth is `BasicAuthUserInfo`.
- `ISchemaIdEncoder` frames the wire id, selected with the `SchemaIdSerializerStrategy` enum on the serde config, never by referencing a concrete encoder class (the encoders are `internal`): `Prefix` writes the magic-byte `[0x00][int32 schema id]` prefix; `Header` moves the id GUID into the `__value_schema_id`/`__key_schema_id` Kafka header, leaving the value payload prefix-free.
- `SchemaIdDeserializerStrategy` picks the decoder: `Dual` reads header-or-prefix so a topic migrates prefix->header without a flag day, `Prefix` reads prefix-only; `SchemaId.FromBytes` reverses the framing.
- `RuleSet` is the data-contract surface: `MigrationRules` (cross-version `Upgrade`/`Downgrade`), `DomainRules` (`Write`/`Read` field transforms, the CSFLE encrypt/decrypt seam), and `EncodingRules`; `RuleRegistry.GlobalInstance` is the default executor/action lookup every serde reads unless a per-serde `RuleRegistry` is passed.

[STACKING]:
- `Confluent.SchemaRegistry.Serdes.Avro`/`.Json`/`.Protobuf`(`.api/api-schemaregistry-serdes-avro.md`, `api-schemaregistry-serdes-json.md`, `api-schemaregistry-serdes-protobuf.md`): each serde binds one shared `ISchemaRegistryClient`, frames every payload with `SchemaId`, and reads `RuleRegistry` for CSFLE and migration.
- `Confluent.Kafka`(`.api/api-kafka.md`): `SchemaIdSerializerStrategy.Header` moves the id into `Message<TKey,TValue>.Headers`, freeing the value payload for a non-Confluent downstream consumer.
- `CloudNative.CloudEvents.Kafka`(`.api/api-cloudevents.md`): the header-framed schema id rides the same `Headers` as CloudEvents attributes and trace context on one message.
- `AWSSDK.KeyManagementService`/`Azure.Security.KeyVault.Keys`/`Google.Cloud.Kms.V1`(`.api/api-aws-kms.md`, `api-azure-keyvault.md`, `api-google-kms.md`): a field-encryption `IRuleExecutor` wraps per-field DEKs against these KMS clients through the shared `RuleRegistry`.
- `Version/egress`: builds one `CachedSchemaRegistryClient` per registry endpoint from one `SchemaRegistryConfig`, shared by every serde on the cluster and disposed once at rail teardown.

[LOCAL_ADMISSION]:
- `SubjectNameStrategy` is fixed at serde-config time: `Topic` (`<topic>-value`) for single-type topics; `TopicRecord`/`Record` for a multi-event-type op-log topic so a `BimCommitted` and a `GeometryRebaked` event coexist on one topic, each governed by its own subject.
- A durable changefeed producer sets `AutoRegisterSchemas = false` and registers schemas out-of-band through `RegisterSchemaWithResponseAsync` under a governed `Compatibility` level (`Backward`/`FullTransitive`), so an incompatible producer schema is rejected at deploy; the consumer pins reader behaviour through `GetLatestWithMetadataAsync` against a `Metadata` tag.
- Field-level encryption routes through the rule engine: a `DomainRule` of `RuleMode.WriteRead` naming a field-encryption `IRuleExecutor` (registered once via `RuleRegistry.RegisterRuleExecutor`) wraps/unwraps per-field DEKs, the `Metadata` sensitive-field set marking which fields it encrypts. `Element/identity#KMS_CUSTODY` owns the `KmsProvider` axis binding registry-governed encryption to the KMS authority.
- Registry auth mints its token from the same runtime authority the Kafka SASL/OAUTHBEARER refresh uses; `AzureIMDSBearerAuthenticationHeaderValueProvider` is the managed-identity path for an Azure-hosted registry. Auth is configured once on the client, never per request.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry`
- Owns: the registry REST control-plane — subject register/lookup, schema-id/GUID resolution, compatibility governance, latest-by-metadata pinning, wire-id framing, subject naming, and the client-side data-contract rule engine (migration + CSFLE).
- Accept: one shared `CachedSchemaRegistryClient` per endpoint, fixed `SubjectNameStrategy`, out-of-band `RegisterSchemaWithResponseAsync` under a governed `Compatibility` level, rule-engine CSFLE through `RuleRegistry`, and `Dual` decoding for prefix->header migration windows.
- Reject: a per-message or per-topic client, `AutoRegisterSchemas` on the durable changefeed producer, hand-rolled magic-byte framing in place of `ISchemaIdEncoder`, a field-encryption pass outside the rule engine, and the Newtonsoft registry wire model treated as a System.Text.Json surface.
