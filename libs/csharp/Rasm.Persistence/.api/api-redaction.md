# [RASM_PERSISTENCE_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies redactors, redactor
providers, redaction builders, HMAC options, erasing redaction, and dependency
injection registration for support bundles and retained snapshot projections.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions`
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- asset: runtime library
- rail: redaction

## [2]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor and provider surfaces
- rail: redaction

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------- | :----------------- | :----------------------- |
|   [1]   | `Redactor`              | redactor contract  | redacts values           |
|   [2]   | `IRedactorProvider`     | provider contract  | resolves redactors       |
|   [3]   | `IRedactionBuilder`     | builder contract   | configures redaction     |
|   [4]   | `DataClassificationSet` | classification set | keys redactor selection  |
|   [5]   | `ErasingRedactor`       | redactor           | erases sensitive values  |
|   [6]   | `HmacRedactor`          | redactor           | hashes sensitive values  |
|   [7]   | `HmacRedactorOptions`   | redactor options   | configures HMAC redactor |
|   [8]   | `NullRedactor`          | redactor           | passes values unchanged  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and policy operations
- rail: redaction

| [INDEX] | [SURFACE]                | [CALL_SHAPE]      | [CAPABILITY]             |
| :-----: | :----------------------- | :---------------- | :----------------------- |
|   [1]   | `AddRedaction`           | service extension | registers redaction      |
|   [2]   | `SetRedactor<T>`         | builder call      | maps redactor            |
|   [3]   | `SetHmacRedactor`        | builder call      | maps HMAC redactor       |
|   [4]   | `SetFallbackRedactor<T>` | builder call      | sets fallback redactor   |
|   [5]   | `GetRedactor`            | provider call     | resolves redactor        |
|   [6]   | `Redact`                 | redactor call     | redacts value            |
|   [7]   | `GetRedactedLength`      | redactor call     | measures redacted output |

## [4]-[IMPLEMENTATION_LAW]

[REDACTION_POLICY]:
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- provider root: `IRedactorProvider`
- builder root: `IRedactionBuilder`
- class root: data classification mapping
- redactor root: erasing and HMAC redactors

[LOCAL_ADMISSION]:
- Redaction class is part of snapshot, support bundle, log, and receipt projection.
- Redactor selection is policy data and cannot hide in serializers.
- HMAC redaction requires declared key identity and output length policy in local receipts.
- Unclassified persisted data is rejected before it reaches a support bundle.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: redaction provider and redactor surfaces
- Accept: declared redaction classes
- Reject: ad hoc string masking
