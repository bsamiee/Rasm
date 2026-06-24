# [RASM_PERSISTENCE_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` supplies redactors, redactor providers,
redaction builders, HMAC options, erasing/null redaction, and dependency-injection
registration that bind the `DataClassification` taxonomy to its redactor column at every
log, trace, support-bundle, and HTTP route-parameter exporter seam. The `Redactor`
contract is polymorphic: it redacts a `ReadOnlySpan<char>`, a `string`, or any `T value`
with a format/provider into a caller buffer, so a typed classified field is redacted at
its value (not stringified first) on the same span-based zero-alloc path the telemetry
exporters use.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Compliance.Redaction`
- package: `Microsoft.Extensions.Compliance.Redaction`
- version: `10.7.0`
- license: MIT
- assembly: `Microsoft.Extensions.Compliance.Redaction`
- contract assembly: `Microsoft.Extensions.Compliance.Abstractions` (`Redactor`, `IRedactionBuilder`, `IRedactorProvider`, `NullRedactor`, `DataClassification*`)
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- namespace classification: `Microsoft.Extensions.Compliance.Classification`
- bound asset: `lib/net10.0` (exact; both assemblies ship a net10.0 asset)
- asset: runtime library
- rail: redaction
- experimental gate: `EXTEXP0002` on `RedactionExtensions.SetHmacRedactor` HMAC configuration

## [02]-[PUBLIC_TYPES]

[REDACTION_TYPES]: redactor and provider surfaces
- rail: redaction

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                       |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `Redactor`            | redactor contract | abstract; redacts span/string/`T value` into spans |
|  [02]   | `IRedactorProvider`   | provider contract | `GetRedactor(DataClassificationSet) → Redactor`    |
|  [03]   | `IRedactionBuilder`   | builder contract  | `SetRedactor<T>` / `SetFallbackRedactor<T>`        |
|  [04]   | `ErasingRedactor`     | redactor          | erases to empty; `Instance` singleton              |
|  [05]   | `NullRedactor`        | redactor          | passes through unchanged; `Instance` singleton     |
|  [06]   | `HmacRedactor`        | redactor          | keyed-HMAC pseudonymizes (experimental)            |
|  [07]   | `HmacRedactorOptions` | redactor options  | `int? KeyId` + base64 `string Key`                 |
|  [08]   | `RedactionExtensions` | builder extension | `SetHmacRedactor` registration                     |

`Redactor` (abstract, contract assembly) — overrides supply `Redact(ReadOnlySpan<char>, Span<char>) → int` and `GetRedactedLength(ReadOnlySpan<char>) → int`; the base provides `string Redact(ReadOnlySpan<char>)`, `string Redact(string?)`, `int Redact(string?, Span<char>)`, the generic `string Redact<T>(T value, string? format = null, IFormatProvider? provider = null)`, `int Redact<T>(T value, Span<char> destination, string? format, IFormatProvider?)`, `bool TryRedact<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider?)`, and `int GetRedactedLength(string?)`. `ErasingRedactor`/`NullRedactor` are the two settled terminal redactors reachable as static `Instance` for use outside DI.

[CLASSIFICATION_TYPES]: classification keys (contract assembly)
- rail: redaction

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]     | [CAPABILITY]                                       |
| :-----: | :---------------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `DataClassification`          | classification key | `(category, name)` value; equality/`==`            |
|  [02]   | `DataClassificationSet`       | classification set | keys redactor selection; ctor + `Union` + implicit |
|  [03]   | `DataClassificationAttribute` | annotation base    | subclasses annotate classified members             |
|  [04]   | `NoDataClassification`        | classification key | absence-of-classification key                      |
|  [05]   | `UnknownDataClassification`   | classification key | unmapped-classification key                        |

`DataClassificationSet` constructs from a single `DataClassification`, an `IEnumerable<DataClassification>`, or `params DataClassification[]`; `implicit operator DataClassificationSet(DataClassification)` lifts a bare key, and `Union(DataClassificationSet)` composes sets — so a multi-class field keys one redactor lookup.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and policy operations
- rail: redaction

Registration targets `IServiceCollection` (`AddRedaction`) or `IRedactionBuilder` (`SetRedactor`/`SetHmacRedactor`/`SetFallbackRedactor`); redaction targets a resolved `Redactor` over span/string/`T value` inputs.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                    | [CAPABILITY]                              |
| :-----: | :----------------------- | :---------------------------------------------- | :---------------------------------------- |
|  [01]   | `AddRedaction`           | `IServiceCollection`                            | registers the redaction provider          |
|  [02]   | `AddRedaction`           | `(IServiceCollection, Action<IRedactionBuilder>)` | registers and configures                  |
|  [03]   | `SetRedactor<T>`         | `(params DataClassificationSet[])`              | maps redactor `T` per classification set  |
|  [04]   | `SetHmacRedactor`        | `(Action<HmacRedactorOptions>, params Set[])`   | maps HMAC redactor by configured action   |
|  [05]   | `SetHmacRedactor`        | `(IConfigurationSection, params Set[])`         | maps HMAC redactor by config section      |
|  [06]   | `SetFallbackRedactor<T>` | `()`                                            | fail-closed default for unmapped classes  |
|  [07]   | `GetRedactor`            | `(DataClassificationSet) → Redactor`            | resolves the redactor for a class set     |
|  [08]   | `Redact<T>`              | `(T value, string? format, IFormatProvider?)`   | redacts a typed value at its value        |
|  [09]   | `TryRedact<T>`           | `(T value, Span<char> dest, out int, format, provider)` | zero-alloc typed-value redaction  |
|  [10]   | `Redact`                 | `(ReadOnlySpan<char>, Span<char>) → int`        | redacts a span into a buffer              |
|  [11]   | `GetRedactedLength`      | `(ReadOnlySpan<char>) → int`                    | sizes the buffer before `Redact`          |
|  [12]   | `ErasingRedactor.Instance` / `NullRedactor.Instance` | static accessor              | settled erase / pass-through redactor     |

[ENTRYPOINT_SCOPE]: HMAC options
- rail: redaction

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------- | :------------ | :------------------------------------ |
|  [01]   | `HmacRedactorOptions.KeyId` | `int?`        | key-version discriminator (nullable)  |
|  [02]   | `HmacRedactorOptions.Key`   | `string`      | base64 HMAC key material              |

## [04]-[IMPLEMENTATION_LAW]

[REDACTION_POLICY]:
- namespace: `Microsoft.Extensions.Compliance.Redaction`
- provider root: `IRedactorProvider.GetRedactor(DataClassificationSet)` resolves the redactor for a set
- builder root: `IRedactionBuilder` maps each classification set to its redactor via `SetRedactor<T>`; `SetHmacRedactor` rides `RedactionExtensions`
- class root: `DataClassificationAttribute` subclasses annotate classified members at definition time
- redactor root: erasing, HMAC, and null redactors with a fail-closed fallback; `GetRedactedLength` sizes the buffer, then `Redact`/`TryRedact` writes the redacted span

[INTEGRATION_LAW]:
- The `DataClassification` axis is the SAME taxonomy `Store/encryption#ENCRYPTION_AXIS` keys on: its `DemandsEncryption` `FrozenSet<DataClassification>` (`Personal`/`Credential`/`Secret`) gates the at-rest encryption demand, and the redaction column gates the exporter masking — one classification value drives both the KMS-wrap demand and the redactor selection, so a `Secret`-classified field is both at-rest encrypted and signal-redacted from one annotation.
- `SetFallbackRedactor<ErasingRedactor>()` is the fail-closed default for unmapped classifications, so an unclassified-but-classified-shaped value erases rather than leaks.
- `SetHmacRedactor` carries `EXTEXP0002`; the policy fold registers it as a declared row value, not a per-call-site experimental opt-in, so no suppression is added at the call site.
- HMAC redaction pseudonymizes while preserving cross-event correlation (a `KeyId` rotation re-buckets the pseudonyms); `ErasingRedactor` destroys the value; credential and secret material never persists in any signal.
- `GetRedactor` is the read seam Persistence's `ClassificationGuard` consumes for store-side enforcement; it resolves the already-registered provider and never re-registers a second redaction builder. The generic `Redact<T>` lets the guard redact a typed value (e.g. a structured key id) without an intermediate `ToString()` that would itself transit the unredacted value.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: redaction provider, redactor surfaces, and the `DataClassification` selection taxonomy
- Accept: declared redaction classes, HMAC key identity, and typed-value redaction through the resolved `Redactor`
- Reject: ad hoc string masking, `ToString()`-then-mask paths, unredacted classified values at any exporter seam
