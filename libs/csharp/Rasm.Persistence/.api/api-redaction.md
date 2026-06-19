# [RASM_PERSISTENCE_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies redactors, redactor providers,
redaction builders, HMAC options, erasing redaction, and dependency-injection
registration that bind the `DataClassification` taxonomy to its redactor column at every
log, trace, support-bundle, and HTTP route-parameter exporter seam.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions`
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- namespace classification: `Microsoft.Extensions.Compliance.Classification`
- asset: runtime library
- rail: redaction
- experimental gate: `EXTEXP0002` on `IRedactionBuilder.SetHmacRedactor` HMAC configuration

## [2]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor and provider surfaces
- rail: redaction

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                  |
| :-----: | :-------------------- | :---------------- | :---------------------------- |
|   [1]   | `Redactor`            | redactor contract | redacts values into spans     |
|   [2]   | `IRedactorProvider`   | provider contract | resolves redactors            |
|   [3]   | `IRedactionBuilder`   | builder contract  | configures redaction          |
|   [4]   | `ErasingRedactor`     | redactor          | erases sensitive values       |
|   [5]   | `HmacRedactor`        | redactor          | hashes sensitive values       |
|   [6]   | `HmacRedactorOptions` | redactor options  | HMAC key id and discriminator |
|   [7]   | `NullRedactor`        | redactor          | passes values unchanged       |
|   [8]   | `RedactionExtensions` | builder extension | HMAC builder registration     |

[CLASSIFICATION_TYPES]: classification keys (contract assembly)
- rail: redaction

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]     | [CAPABILITY]                  |
| :-----: | :---------------------------- | :----------------- | :---------------------------- |
|   [1]   | `DataClassification`          | classification key | category-plus-name value      |
|   [2]   | `DataClassificationSet`       | classification set | keys redactor selection       |
|   [3]   | `DataClassificationAttribute` | annotation base    | annotates classified members  |
|   [4]   | `NoDataClassification`        | classification key | absence-of-classification key |
|   [5]   | `UnknownDataClassification`   | classification key | unmapped-classification key   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and policy operations
- rail: redaction

Registration calls target `IServiceCollection` or `IRedactionBuilder`; redactor calls use span inputs and return the redacted length or resolved `Redactor`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]       | [CAPABILITY]                  |
| :-----: | :----------------------- | :----------------- | :---------------------------- |
|   [1]   | `AddRedaction`           | service extension  | registers redaction           |
|   [2]   | `AddRedaction`           | configured service | registers and configures      |
|   [3]   | `SetRedactor<T>`         | builder mapping    | maps redactor per class       |
|   [4]   | `SetHmacRedactor`        | options mapping    | maps HMAC redactor by action  |
|   [5]   | `SetHmacRedactor`        | section mapping    | maps HMAC redactor by section |
|   [6]   | `SetFallbackRedactor<T>` | builder fallback   | fail-closed default redactor  |
|   [7]   | `GetRedactor`            | provider lookup    | resolves redactor by set      |
|   [8]   | `Redact`                 | span redaction     | redacts value into buffer     |
|   [9]   | `GetRedactedLength`      | span measurement   | measures redacted output      |
|  [10]   | redactor singletons      | static accessor    | settled pass / erase redactor |

[ENTRYPOINT_SCOPE]: HMAC options
- rail: redaction

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------- | :------------ | :-------------------------------- |
|   [1]   | `HmacRedactorOptions.KeyId` | options value | key-version discriminator integer |
|   [2]   | `HmacRedactorOptions.Key`   | options value | base64 HMAC key material          |

## [4]-[IMPLEMENTATION_LAW]

[REDACTION_POLICY]:
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- provider root: `IRedactorProvider` resolves the redactor for a `DataClassificationSet`
- builder root: `IRedactionBuilder` maps each classification set to its redactor
- class root: `DataClassificationAttribute` subclasses annotate classified members at definition time
- redactor root: erasing, HMAC, and null redactors with a fail-closed fallback

[LOCAL_ADMISSION]:
- The `DataClassification` axis at Observability/telemetry#REDACTION_TAXONOMY drives redactor selection through its `RedactorKind` column; `RedactionRegistration.Bind` folds each row to `SetRedactor`/`SetHmacRedactor`.
- `SetFallbackRedactor<ErasingRedactor>()` is the fail-closed default for unmapped classifications, so an unclassified value erases rather than leaks.
- `SetHmacRedactor` carries `EXTEXP0002`; the fold registers it without a suppression because the HMAC row is a declared policy value, not an experimental opt-in at the call site.
- HMAC redaction pseudonymizes while preserving cross-event correlation; erase destroys the value; credential and secret material never persists in any signal.
- `GetRedactor` is the read seam Persistence `ClassificationGuard` consumes for store-side enforcement; it never re-registers a second redaction builder.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: redaction provider and redactor surfaces
- Accept: declared redaction classes and HMAC key identity
- Reject: ad hoc string masking, unredacted classified values at any exporter seam
