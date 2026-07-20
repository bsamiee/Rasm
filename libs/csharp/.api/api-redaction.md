# [RASM_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies redactors, providers, builders, HMAC options, erasing redaction, the `DataClassification` taxonomy, and dependency-injection registration that binds each classification to its redactor at every exporter seam, with `StringBuilder` redaction for the logging pipeline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions` — redactor base, builder and provider contracts, the null redactor/provider pair, the `DataClassification` taxonomy, the `StringBuilder` redaction egress
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- namespace classification: `Microsoft.Extensions.Compliance.Classification`
- asset: runtime library
- rail: redaction
- ruled gate: `EXTEXP0002` on `DataClassificationTypeConverter` string round-tripping

## [02]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor and provider surfaces
- rail: redaction

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]    | [CAPABILITY]                                  |
| :-----: | :--------------------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `Redactor`                         | redactor contract | redacts values into spans                     |
|  [02]   | `IRedactorProvider`                | provider contract | resolves redactors                            |
|  [03]   | `IRedactionBuilder`                | builder contract  | configures redaction                          |
|  [04]   | `ErasingRedactor`                  | redactor          | erases sensitive values; `Instance` singleton |
|  [05]   | `HmacRedactor`                     | redactor          | hashes sensitive values                       |
|  [06]   | `HmacRedactorOptions`              | redactor options  | HMAC key id and base64 key                    |
|  [07]   | `NullRedactor`                     | redactor          | passes values unchanged; `Instance` singleton |
|  [08]   | `NullRedactorProvider`             | provider          | resolves `NullRedactor`; `Instance` singleton |
|  [09]   | `RedactionExtensions`              | builder extension | `SetHmacRedactor` registration                |
|  [10]   | `RedactionStringBuilderExtensions` | builder extension | `AppendRedacted` into a `StringBuilder`       |

`Redactor` (abstract, contract assembly) — overrides supply `Redact(ReadOnlySpan<char>, Span<char>) → int` and `GetRedactedLength(ReadOnlySpan<char>) → int`; the base implements `string Redact(ReadOnlySpan<char>)`, virtual `string Redact(string?)`, `int Redact(string?, Span<char>)`, the generic `string Redact<T>(T value, string? format = null, IFormatProvider? provider = null)`, `int Redact<T>(T value, Span<char> destination, string? format = null, IFormatProvider? provider = null)`, `bool TryRedact<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)`, and `int GetRedactedLength(string?)`. A typed classified value redacts at its value (not stringified first) on the span path. `ErasingRedactor`/`NullRedactor` are the two settled terminal redactors reachable as static `Instance` outside DI, and `NullRedactorProvider.Instance` is the matching pass-through provider.

[CLASSIFICATION_TYPES]: classification taxonomy (contract assembly)
- rail: redaction

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]     | [CAPABILITY]                                           |
| :-----: | :----------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `DataClassification`                 | classification key | taxonomy-plus-value key; static `None`/`Unknown`       |
|  [02]   | `DataClassificationSet`              | classification set | keys redactor selection                                |
|  [03]   | `DataClassificationAttribute`        | annotation base    | annotates classified members; `Notes` audit text       |
|  [04]   | `NoDataClassificationAttribute`      | annotation         | marks `None`-classified members                        |
|  [05]   | `UnknownDataClassificationAttribute` | annotation         | marks `Unknown`-classified members                     |
|  [06]   | `DataClassificationTypeConverter`    | converter          | string round-trips `DataClassification` (`EXTEXP0002`) |

`DataClassification` pairs a `TaxonomyName` with a `Value` (ctor `(taxonomyName, value)`), renders `Taxonomy:Value`, and exposes static `None`/`Unknown` keys; `DataClassificationTypeConverter` round-trips that string form for configuration binding under `EXTEXP0002`. `DataClassificationSet` constructs from a single `DataClassification`, an `IEnumerable<DataClassification>`, or `params DataClassification[]`; `implicit operator DataClassificationSet(DataClassification)` and `FromDataClassification` lift a bare key, and `Union(DataClassificationSet)` composes sets — so a multi-class field keys one redactor lookup.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and policy operations
- rail: redaction

Registration calls target `IServiceCollection` or `IRedactionBuilder`; redactor calls use span inputs and return the redacted length or resolved `Redactor`.

| [INDEX] | [SURFACE]                                            | [KIND]             | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------- | :----------------- | :------------------------------------------- |
|  [01]   | `AddRedaction`                                       | registration       | registers redaction                          |
|  [02]   | `AddRedaction`                                       | configured         | registers configured policy                  |
|  [03]   | `SetRedactor<T>`                                     | mapping            | maps classes to redactors                    |
|  [04]   | `SetHmacRedactor`                                    | options            | HMAC via `Action<HmacRedactorOptions>`       |
|  [05]   | `SetHmacRedactor`                                    | section            | HMAC via `IConfigurationSection`             |
|  [06]   | `SetFallbackRedactor<T>`                             | fallback           | sets fail-closed redaction                   |
|  [07]   | `GetRedactor`                                        | provider           | resolves classified sets                     |
|  [08]   | `Redact<T>`                                          | typed value        | redacts formatted values                     |
|  [09]   | `TryRedact<T>`                                       | typed value        | writes redaction into spans                  |
|  [10]   | `Redact`                                             | span               | redacts character spans                      |
|  [11]   | `GetRedactedLength`                                  | sizing             | sizes the destination                        |
|  [12]   | `AppendRedacted`                                     | string builder     | appends a redacted span to a `StringBuilder` |
|  [13]   | `ErasingRedactor.Instance` / `NullRedactor.Instance` | singleton          | selects terminal redaction                   |
|  [14]   | `NullRedactorProvider.Instance`                      | provider singleton | DI-free pass-through provider                |

[ENTRYPOINT_SCOPE]: HMAC options
- rail: redaction
- call shape: `HmacRedactorOptions` property

| [INDEX] | [SURFACE]                   | [CAPABILITY]                               |
| :-----: | :-------------------------- | :----------------------------------------- |
|  [01]   | `HmacRedactorOptions.KeyId` | optional key-version discriminator integer |
|  [02]   | `HmacRedactorOptions.Key`   | base64 HMAC key material (44-char minimum) |

## [04]-[IMPLEMENTATION_LAW]

[REDACTION_POLICY]:
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- provider root: `IRedactorProvider` resolves the redactor for a `DataClassificationSet`
- builder root: `IRedactionBuilder` maps each classification set to its redactor via `SetRedactor<T>`; `SetHmacRedactor` rides `RedactionExtensions`
- class root: `DataClassificationAttribute` subclasses annotate classified members at definition time
- redactor root: erasing, HMAC, and null redactors with a fail-closed fallback; `GetRedactedLength` sizes the buffer, then `Redact`/`TryRedact` writes the redacted span

[LOCAL_ADMISSION]:
- `DataClassification` drives redactor selection through its `RedactorKind` column; `RedactionRegistration.Bind` folds each row to `SetRedactor` or `SetHmacRedactor`.
- `SetFallbackRedactor<ErasingRedactor>()` is the fail-closed default for unmapped classifications, so an unclassified value erases rather than leaks.
- `DataClassificationTypeConverter` carries `EXTEXP0002`; a string-bound classification acknowledges it as a declared policy value, never a suppression at the call site, and `SetHmacRedactor` itself is ungated.
- HMAC redaction pseudonymizes while preserving cross-event correlation; erase destroys the value; credential and secret material never persists in any signal.
- `GetRedactor` is the provider read seam that resolves a `Redactor` from a `DataClassificationSet` at every exporter/bundle egress; `DataClassification` crosses to Persistence as VALUE fields on landed rows (`Element/codec` `SnapshotCatalogRow.Classification`, `Element/identity`), never a guard symbol, and never registers a second redaction builder.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: redaction provider and redactor surfaces
- Accept: declared redaction classes and HMAC key identity
- Reject: ad hoc string masking, unredacted classified values at any exporter seam
