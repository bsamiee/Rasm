# [RASM_PERSISTENCE_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies classifications, redactors, providers, and support-export redaction policies.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- asset: runtime library
- rail: redaction

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: redaction family
- rail: redaction

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :------------------------ | :---------------- | :------------------------ |
|   [1]   | `ErasingRedactor`         | runtime redactor  | redacts sensitive value   |
|   [2]   | `HmacRedactor`            | runtime redactor  | redacts sensitive value   |
|   [3]   | `HmacRedactorOptions`     | policy object     | carries policy input      |
|   [4]   | `RedactionBuilder`        | builder surface   | admits redaction policy   |
|   [5]   | `RedactionExtensions`     | extension surface | admits configured surface |
|   [6]   | `RedactorProviderOptions` | provider options  | binds redactor map        |

[ABSTRACTION_CONTRACTS]:
- rail: redaction

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]         | [CAPABILITY]                |
| :-----: | :---------------------- | :--------------------- | :-------------------------- |
|   [1]   | `DataClassification`    | classification value   | anchors redaction contract  |
|   [2]   | `DataClassificationSet` | classification set     | anchors redaction contract  |
|   [3]   | `IRedactionBuilder`     | builder contract       | admits redaction policy     |
|   [4]   | `IRedactorProvider`     | provider contract      | resolves redactor           |
|   [5]   | `NullRedactor`          | null redactor contract | defines null redaction path |
|   [6]   | `Redactor`              | redactor base contract | anchors redaction contract  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: redaction operations
- rail: redaction

| [INDEX] | [SURFACE]                | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :----------------------- | :---------------- | :------------------------ |
|   [1]   | `AddRedaction`           | DI extension      | admits configured surface |
|   [2]   | `SetHmacRedactor`        | builder extension | admits redaction policy   |
|   [3]   | `SetRedactor<T>`         | builder extension | admits redaction policy   |
|   [4]   | `SetFallbackRedactor<T>` | builder extension | admits fallback policy    |
|   [5]   | `GetRedactor`            | lookup call       | resolves redactor         |
|   [6]   | `Redact`                 | redaction method  | redacts sensitive value   |
|   [7]   | `TryRedact`              | redaction method  | redacts sensitive value   |
|   [8]   | `GetRedactedLength`      | length query      | sizes redacted output     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: support export redaction
- Accept: redaction class is profile data
- Reject: manual string masking
