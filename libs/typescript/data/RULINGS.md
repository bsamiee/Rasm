# [TS_DATA_RULINGS]

Per-folder decision registry for the durable-persistence plane — the settled rulings agents re-litigate for lack of a home. Append-mostly; a row earns its seat while its why stays homeless, and dies only when fact and why both live at one durable surface.

## [01]-[PACKAGES]

- `@aws-sdk/client-s3`, `@aws-sdk/lib-storage`, and `@aws-sdk/s3-request-presigner` HOLD at one matched pin — the SDK's near-daily release cadence is noise deliberately not chased, so a routine bump-to-newest is the refuted move; the trio moves together or not at all, and the hold reopens only when a composed member demands a newer line.
- `@effect/sql-mysql2` and `@effect/sql-mssql` stay read-only interop ingress, never the journal/tenant/capability statement set — no statement page spells a `mysql`/`mssql` arm, and the neutral statements' `orElse` arm carries sqlite file-per-app semantics with no tenancy GUC, so a foreign-relational client on the write path silently drops isolation; the posture reopens only when a statement page realizes the arms with an explicit tenancy answer.
- Object-engine conformance pins `If-None-Match: *` conditional put as the admission bar — the key IS the content, so atomic create-if-absent keeps concurrent writers of one `ContentKey` idempotent; a plain-put engine races writers into silent overwrite beneath dedup, reference counting, and sweep, so its refusal is a guarantee, never a gap.
