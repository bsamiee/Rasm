# [RASM_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` owns classification-keyed redactor resolution: a `DataClassificationSet` selects one `Redactor` at every egress seam, and a set no row claims falls to the erasing fallback. Redaction writes into a caller-sized span, so a classified value crosses the seam with no intermediate string on the sized path. Its contract assembly carries the taxonomy, the redactor base, and the provider and builder contracts an instrumented library binds without the registration fold.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction` (MIT, Microsoft)
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions`
- namespace: `Microsoft.Extensions.Compliance.Redaction`, `Microsoft.Extensions.Compliance.Classification`, `Microsoft.Extensions.DependencyInjection`, `System.Text`
- asset: runtime library
- rail: redaction
- ruled gate: `EXTEXP0002` on `DataClassificationTypeConverter`

## [02]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor, provider, and builder surfaces

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `Redactor`                             | abstract class | span-write redaction base, two abstract overrides |
|  [02]   | `IRedactorProvider`                    | interface      | resolves one redactor per classification set      |
|  [03]   | `IRedactionBuilder`                    | interface      | classification-to-redactor mapping seam           |
|  [04]   | `ErasingRedactor`                      | sealed class   | erases to zero length; `Instance` singleton       |
|  [05]   | `HmacRedactor`                         | sealed class   | keyed HMAC-SHA256 pseudonym at constant width     |
|  [06]   | `HmacRedactorOptions`                  | options        | key identity, validated at start                  |
|  [07]   | `NullRedactor`                         | sealed class   | copies through unchanged; `Instance` singleton    |
|  [08]   | `NullRedactorProvider`                 | sealed class   | pass-through provider; `Instance` singleton       |
|  [09]   | `RedactionServiceCollectionExtensions` | static class   | `AddRedaction` registration                       |
|  [10]   | `RedactionExtensions`                  | static class   | HMAC mapping rows on the builder                  |
|  [11]   | `RedactionStringBuilderExtensions`     | static class   | `StringBuilder` redaction egress                  |

- `HmacRedactor`: every non-empty input redacts to a constant width — an optional key-id prefix ahead of 24 base64 characters over 16 hash bytes.

[CLASSIFICATION_TYPES]: classification taxonomy (contract assembly)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :----------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `DataClassification`                 | readonly struct | taxonomy-scoped key; `None`/`Unknown` statics      |
|  [02]   | `DataClassificationSet`              | sealed class    | equatable key set the redactor lookup binds        |
|  [03]   | `DataClassificationAttribute`        | attribute       | repeatable definition-time key; `Notes` audit text |
|  [04]   | `NoDataClassificationAttribute`      | attribute       | seals a member `None`                              |
|  [05]   | `UnknownDataClassificationAttribute` | attribute       | seals a member `Unknown`                           |
|  [06]   | `DataClassificationTypeConverter`    | type converter  | string round-trip for configuration binding        |

- `DataClassification`: carries its own `[TypeConverter]`, so a configuration-bound value round-trips `Taxonomy:Value` with no converter registration.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration on `IServiceCollection`, mapping on `IRedactionBuilder`, resolution on `IRedactorProvider`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `AddRedaction()`                                                        | static   | seats the provider with the fallback alone     |
|  [02]   | `AddRedaction(Action<IRedactionBuilder>)`                               | static   | seats the provider and folds every mapping row |
|  [03]   | `SetRedactor<T>(DataClassificationSet[])`                               | instance | binds one redactor type per exact set          |
|  [04]   | `SetFallbackRedactor<T>()`                                              | instance | replaces the unmapped-set redactor             |
|  [05]   | `SetHmacRedactor(Action<HmacRedactorOptions>, DataClassificationSet[])` | static   | HMAC rows over inline options                  |
|  [06]   | `SetHmacRedactor(IConfigurationSection, DataClassificationSet[])`       | static   | HMAC rows bound from configuration             |
|  [07]   | `Services`                                                              | property | collection each mapped redactor registers into |
|  [08]   | `GetRedactor(DataClassificationSet) -> Redactor`                        | instance | resolves the egress redactor                   |

- `SetHmacRedactor`: both rows delegate to `SetRedactor<HmacRedactor>` and validate the key at start, so malformed key material fails boot rather than the first redaction.

[ENTRYPOINT_SCOPE]: `Redactor` redaction, sizing, and `StringBuilder` egress

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Redact(ReadOnlySpan<char>, Span<char>) -> int`                              | abstract | the write every redactor supplies   |
|  [02]   | `GetRedactedLength(ReadOnlySpan<char>) -> int`                               | abstract | the sizer every redactor supplies   |
|  [03]   | `Redact(ReadOnlySpan<char>) -> string`                                       | instance | sizes, then materializes one string |
|  [04]   | `Redact(string?) -> string`                                                  | virtual  | string arm a redactor specializes   |
|  [05]   | `Redact(string?, Span<char>) -> int`                                         | instance | string source into a caller span    |
|  [06]   | `Redact<T>(T, string?, IFormatProvider?) -> string`                          | instance | formats a typed value, then redacts |
|  [07]   | `Redact<T>(T, Span<char>, string?, IFormatProvider?) -> int`                 | instance | typed value into a caller span      |
|  [08]   | `TryRedact<T>(T, Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` | instance | non-throwing sized write            |
|  [09]   | `GetRedactedLength(string?) -> int`                                          | instance | string-source sizer                 |
|  [10]   | `AppendRedacted(Redactor, ReadOnlySpan<char>) -> StringBuilder`              | static   | appends off a rented buffer         |
|  [11]   | `AppendRedacted(Redactor, string?) -> StringBuilder`                         | static   | string arm of the same append       |

- `Redact<T>`: an `ISpanFormattable` value formats into a 256-char stack buffer and redacts at its rendered value; a longer rendering falls to the string path.
- `TryRedact<T>`: returns `false` with `charsWritten` at zero when the destination is shorter than the redacted length.

[ENTRYPOINT_SCOPE]: classification keys and set composition

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `DataClassification(string, string)`                               | ctor     | mints a taxonomy-scoped key     |
|  [02]   | `DataClassificationSet(DataClassification)`                        | ctor     | one-key set                     |
|  [03]   | `DataClassificationSet(IEnumerable<DataClassification>)`           | ctor     | sequence-built set              |
|  [04]   | `DataClassificationSet(DataClassification[])`                      | ctor     | params-built set                |
|  [05]   | `DataClassificationSet.FromDataClassification(DataClassification)` | factory  | lifts one key to a set          |
|  [06]   | `implicit operator DataClassificationSet(DataClassification)`      | operator | lifts a bare key at a call site |
|  [07]   | `DataClassificationSet.Union(DataClassificationSet)`               | instance | mints a composed set from two   |

[ENTRYPOINT_SCOPE]: `HmacRedactorOptions` policy

| [INDEX] | [SURFACE] | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------- | :------- | :-------------------------------------------------- |
|  [01]   | `KeyId`   | property | `int?` key version prefixed to every redacted value |
|  [02]   | `Key`     | property | base64 key material, 44-character floor             |

- `KeyId`: values hashed under different key ids never correlate, so a key rotation cuts correlation at the rotation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IRedactorProvider` resolves one `Redactor` per set from a frozen map built at registration, so an egress read costs a hash lookup.
- Lookup keys on whole-set equality: a set composed through `Union` resolves against a row registered for that same composite, never against its member classifications.
- `DataClassification.None` binds `NullRedactor` unless a row claims it, and `ErasingRedactor` is the shipped fallback for every other unclaimed set.
- Sizing precedes writing on every span path: `GetRedactedLength` bounds the destination, then `Redact` writes and returns the count.
- Provider construction throws when the fallback type resolves to no registered instance, so a fallback row and its redactor registration land together.

[STACKING]:
- `Microsoft.Extensions.Telemetry`(`api-extensions-telemetry.md`): `EnableRedaction` binds this provider onto the `ILogger` seam, and `LoggerRedactionOptions.ApplyDiscriminator` folds the tag name into the value before redaction, so one raw value yields a distinct token per tag name.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging-abstractions.md`): `[LogProperties]` and `[TagProvider]` generated methods carry each expanded member's `DataClassificationSet` to the provider before a sink observes the tag.
- `OpenTelemetry.Instrumentation.Http`(`api-otel-instrumentation-http.md`): the rosters partition at the value boundary — URL-query redaction rides that package's own environment row, annotated values ride this provider.
- `Rasm.AppHost` `Observability/telemetry#REDACTION_TAXONOMY`: `RedactionRegistration.Bind` folds each taxonomy row's `RedactorKind` onto `SetHmacRedactor` or `SetRedactor<ErasingRedactor>` and closes on `SetFallbackRedactor<ErasingRedactor>()`; `DataClassification.Marker` mints the compliance key.
- `Rasm.Persistence` `Element/codec#SNAPSHOT_SPINE`: a classification lands as a value field on the snapshot catalog row and the `Element/identity` ceiling, so retention policy ranks the stamp without resolving a redactor.

[LOCAL_ADMISSION]:
- `GetRedactor` is the read seam at every exporter and bundle egress; a classified value reaches a sink through the redactor it resolves.
- `DataClassificationTypeConverter` binds under `EXTEXP0002` acknowledged as a declared policy value at the owning project row.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: classification-keyed redactor resolution and the span-write redaction contract
- Accept: declared classification sets mapped at one registration fold, HMAC key identity bound from configuration
- Reject: ad hoc string masking at a call site; a second redaction builder beside the composition-root fold
