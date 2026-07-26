# [TS_DATA_RULINGS]

`typescript/data` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@aws-sdk/client-s3`, `@aws-sdk/lib-storage`, and `@aws-sdk/s3-request-presigner` HOLD at one matched pin — the SDK's near-daily release cadence is noise deliberately not chased, so a routine bump-to-newest is the refuted move; the trio moves together or not at all, and the hold reopens only when a composed member demands a newer line.
- `@effect/sql-mysql2` and `@effect/sql-mssql` stay read-only interop ingress, never the journal/tenant/capability statement set — no statement page spells a `mysql`/`mssql` arm, and the neutral statements' `orElse` arm carries sqlite file-per-app semantics with no tenancy GUC, so a foreign-relational client on the write path silently drops isolation; the posture reopens only when a statement page realizes the arms with an explicit tenancy answer.
- Object-engine conformance pins `If-None-Match: *` conditional put as the admission bar — the key IS the content, so atomic create-if-absent keeps concurrent writers of one `ContentKey` idempotent; a plain-put engine races writers into silent overwrite beneath dedup, reference counting, and sweep, so its refusal is a guarantee, never a gap.

## [02]-[SHAPE]

- (none)

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
