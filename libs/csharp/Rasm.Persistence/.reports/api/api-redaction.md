# [RASM_PERSISTENCE_API_REDACTION]

Redaction APIs supply classification, redactor registration, support-bundle export policy, and redacted artifact receipts.

## [1]-[SURFACES]

This table is a lookup by redaction package.

| [INDEX] | [PACKAGE]                                  | [ASSEMBLY]                                  | [LOCAL_RAIL] |
| :-----: | :----------------------------------------- | :------------------------------------------ | :----------- |
|   [1]   | `Microsoft.Extensions.Compliance.Redaction` | `Microsoft.Extensions.Compliance.Redaction` | redaction    |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                                  | [NAMESPACE]                          | [USING]                               | [API_LOCATOR] |
| :-----: | :------------------------------------------ | :----------------------------------- | :------------------------------------ | :------------ |
|   [1]   | `Microsoft.Extensions.Compliance.Redaction` | `Microsoft.Extensions.Compliance.Redaction` | `Microsoft.Extensions.Compliance.Redaction` | `.cache/nuget/packages/microsoft.extensions.compliance.redaction/` |
|   [2]   | `Microsoft.Extensions.Compliance.Redaction` | `Microsoft.Extensions.Compliance.Classification` | `Microsoft.Extensions.Compliance.Classification` | `.cache/nuget/packages/microsoft.extensions.compliance.redaction/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]       | [ENTRY_SURFACE]            | [LOCAL_RAIL] |
| :-----: | :------------------ | :------------------------- | :----------- |
|   [1]   | classification APIs | data category vocabulary   | redaction    |
|   [2]   | redactor providers  | redaction service binding  | redaction    |
|   [3]   | redaction delegates | artifact field transform   | redaction    |
|   [4]   | support receipts    | redacted output evidence   | support      |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]                 | [LOCAL_RAIL] | [REASON]                |
| :-----: | :----------------------- | :----------- | :---------------------- |
|   [1]   | ASP.NET DataProtection   | redaction    | file protection is profile policy |
|   [2]   | raw support payload export | support    | redaction owns export   |
