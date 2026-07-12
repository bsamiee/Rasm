# [RASM_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies redactors, providers, builders, HMAC options, erasing redaction, and dependency-injection registration that bind each `DataClassification` to its redactor at every exporter seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions` (`Redactor`, `IRedactionBuilder`, `IRedactorProvider`, `NullRedactor`, `DataClassification*`)
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- namespace classification: `Microsoft.Extensions.Compliance.Classification`
- asset: runtime library
- rail: redaction
- ruled gate: `EXTEXP0002` on `RedactionExtensions.SetHmacRedactor` HMAC configuration

## [02]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor and provider surfaces
- rail: redaction

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                  |
| :-----: | :-------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `Redactor`            | redactor contract | redacts values into spans                     |
|  [02]   | `IRedactorProvider`   | provider contract | resolves redactors                            |
|  [03]   | `IRedactionBuilder`   | builder contract  | configures redaction                          |
|  [04]   | `ErasingRedactor`     | redactor          | erases sensitive values                       |
|  [05]   | `HmacRedactor`        | redactor          | hashes sensitive values                       |
|  [06]   | `HmacRedactorOptions` | redactor options  | HMAC key id and discriminator                 |
|  [07]   | `NullRedactor`        | redactor          | passes values unchanged; `Instance` singleton |
|  [08]   | `RedactionExtensions` | builder extension | `SetHmacRedactor` registration                |

`Redactor` (abstract, contract assembly) — overrides supply `Redact(ReadOnlySpan<char>, Span<char>) → int` and `GetRedactedLength(ReadOnlySpan<char>) → int`; the base implements `string Redact(ReadOnlySpan<char>)`, `string Redact(string?)`, `int Redact(string?, Span<char>)`, the generic `string Redact<T>(T value, string? format = null, IFormatProvider? provider = null)`, `int Redact<T>(T value, Span<char> destination, string? format, IFormatProvider?)`, `bool TryRedact<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider?)`, and `int GetRedactedLength(string?)`. A typed classified value redacts at its value (not stringified first) on the span path. `ErasingRedactor`/`NullRedactor` are the two settled terminal redactors reachable as static `Instance` outside DI.

[CLASSIFICATION_TYPES]: classification keys (contract assembly)
- rail: redaction

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]     | [CAPABILITY]                  |
| :-----: | :---------------------------- | :----------------- | :---------------------------- |
|  [01]   | `DataClassification`          | classification key | category-plus-name value      |
|  [02]   | `DataClassificationSet`       | classification set | keys redactor selection       |
|  [03]   | `DataClassificationAttribute` | annotation base    | annotates classified members  |
|  [04]   | `NoDataClassification`        | classification key | absence-of-classification key |
|  [05]   | `UnknownDataClassification`   | classification key | unmapped-classification key   |

`DataClassificationSet` constructs from a single `DataClassification`, an `IEnumerable<DataClassification>`, or `params DataClassification[]`; `implicit operator DataClassificationSet(DataClassification)` lifts a bare key, and `Union(DataClassificationSet)` composes sets — so a multi-class field keys one redactor lookup.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and policy operations
- rail: redaction

Registration calls target `IServiceCollection` or `IRedactionBuilder`; redactor calls use span inputs and return the redacted length or resolved `Redactor`.

| [INDEX] | [SURFACE]                                            | [KIND]       | [CAPABILITY]                |
| :-----: | :--------------------------------------------------- | :----------- | :-------------------------- |
|  [01]   | `AddRedaction`                                       | registration | registers redaction         |
|  [02]   | `AddRedaction`                                       | configured   | registers configured policy |
|  [03]   | `SetRedactor<T>`                                     | mapping      | maps classes to redactors   |
|  [04]   | `SetHmacRedactor`                                    | options      | maps HMAC by action         |
|  [05]   | `SetHmacRedactor`                                    | section      | maps HMAC by section        |
|  [06]   | `SetFallbackRedactor<T>`                             | fallback     | sets fail-closed redaction  |
|  [07]   | `GetRedactor`                                        | provider     | resolves classified sets    |
|  [08]   | `Redact<T>`                                          | typed value  | redacts formatted values    |
|  [09]   | `TryRedact<T>`                                       | typed value  | writes redaction into spans |
|  [10]   | `Redact`                                             | span         | redacts character spans     |
|  [11]   | `GetRedactedLength`                                  | sizing       | sizes the destination       |
|  [12]   | `ErasingRedactor.Instance` / `NullRedactor.Instance` | singleton    | selects terminal redaction  |

[ENTRYPOINT_SCOPE]: HMAC options
- rail: redaction
- call shape: `HmacRedactorOptions` property

| [INDEX] | [SURFACE]                   | [CAPABILITY]                      |
| :-----: | :-------------------------- | :-------------------------------- |
|  [01]   | `HmacRedactorOptions.KeyId` | key-version discriminator integer |
|  [02]   | `HmacRedactorOptions.Key`   | base64 HMAC key material          |

## [04]-[IMPLEMENTATION_LAW]

[REDACTION_POLICY]:
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- provider root: `IRedactorProvider` resolves the redactor for a `DataClassificationSet`
- builder root: `IRedactionBuilder` maps each classification set to its redactor via `SetRedactor<T>`; `SetHmacRedactor` rides `RedactionExtensions`
- class root: `DataClassificationAttribute` subclasses annotate classified members at definition time
- redactor root: erasing, HMAC, and null redactors with a fail-closed fallback; `GetRedactedLength` sizes the buffer, then `Redact`/`TryRedact` writes the redacted span

[LOCAL_ADMISSION]:
- The `DataClassification` axis drives redactor selection through its `RedactorKind` column; `RedactionRegistration.Bind` folds each row to `SetRedactor` or `SetHmacRedactor`.
- `SetFallbackRedactor<ErasingRedactor>()` is the fail-closed default for unmapped classifications, so an unclassified value erases rather than leaks.
- `SetHmacRedactor` carries `EXTEXP0002`; the fold registers it without a suppression because the HMAC row is a declared policy value, not an ruled opt-in at the call site.
- HMAC redaction pseudonymizes while preserving cross-event correlation; erase destroys the value; credential and secret material never persists in any signal.
- `GetRedactor` is the provider read seam that resolves a `Redactor` from a `DataClassificationSet` at every exporter/bundle egress; `DataClassification` crosses to Persistence as VALUE fields on landed rows (`Element/codec` `SnapshotCatalogRow.Classification`, `Element/identity`), never a guard symbol — the once-cited Persistence `ClassificationGuard` is a phantom absent from the landed corpus; it never re-registers a second redaction builder.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: redaction provider and redactor surfaces
- Accept: declared redaction classes and HMAC key identity
- Reject: ad hoc string masking, unredacted classified values at any exporter seam
