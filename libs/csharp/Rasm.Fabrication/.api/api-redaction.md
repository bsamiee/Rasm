# [RASM_FABRICATION_API_REDACTION]

`Microsoft.Extensions.Compliance.Redaction` enters this folder for its contract assembly alone: the `DataClassification` key and `DataClassificationAttribute` base that `Process/telemetry#CLASSIFICATION` binds into the folder's sealed attribute rows. Redactor selection, HMAC options, and the `AddRedaction` registration fold stay at the AppHost composition root; no Fabrication page resolves a `Redactor`. Substrate canonical members live at `libs/csharp/.api/api-redaction.md`; this overlay carries only the Fabrication delta — the classified-member map and the value-federation law.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-redaction.md`
- redactor, provider, builder, HMAC, and classification type rosters with their call-shape tables live on the substrate catalog — this overlay never re-states them
- rail: redaction

## [02]-[FABRICATION_BINDINGS]

- `FabricationClassified` mints the folder's three `DataClassification` values as `(taxonomy, value)` string pairs — `"DataClassification"` with `personal`, `confidential`, `credential` — value federation to the suite taxonomy rows, never a type reference across the package boundary.
- Sealed attribute rows `PersonalDataAttribute`, `ConfidentialDataAttribute`, and `CredentialDataAttribute` subclass `DataClassificationAttribute` and annotate at definition time.
- Classified members: `AttestationPayload.Signer` and `TravelerAmendment.Actor` and `WelderQualification.Welder` carry personal; `HeatNumber` carries confidential; `AttestationPayload.Credential` carries credential.
- Sealed canonical artifact bytes never redact — classification governs the telemetry and log egress of these members, and the signed record keeps its attested content.
- `DataClassificationTypeConverter` stays under its `EXTEXP0002` gate as a declared policy value when a classification ever binds from configuration; no Fabrication surface binds one today.

## [03]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Compliance.Redaction`
- Owns: the classification attribute surface this folder consumes through the transitively arriving contract assembly
- Accept: definition-time attribute rows binding suite taxonomy keys; measures-only fact cases at `Process/telemetry#FACT_UNION`
- Reject: a folder-local taxonomy row, a `Redactor` resolution below the app root, classified identity on any instrument tag or fact field
