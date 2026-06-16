# [PY_RUNTIME_OBSERVABILITY_API]

Runtime observability is local evidence production. It contributes receipts and structured facts for callers to collect; it does not own product telemetry export or health.

## [1]-[RECEIPT_OWNER]

[RECEIPT]:
- Owns: Python-local evidence rows for admitted, planned, emitted, rejected, and drained operations.
- Entry: one receipt-construction surface fed by owner, subject, facts, and correlation.
- Packages: `msgspec`, `structlog`.
- Output: immutable receipt record.
- Boundary: no AppHost envelope, no health status, no support-bundle capture, and no C# receipt minting.

[REDACTION]:
- Owns: Python-local field classification before logging or receipt emission.
- Entry: one classification policy consumed by receipt contributors.
- Packages: `structlog`, OpenTelemetry packages.
- Output: redaction-safe log and trace attributes.
- Boundary: AppHost remains the suite classification taxonomy owner.

## [2]-[API_EVIDENCE_OWNER]

[API_PACKAGE]:
- Owns: distribution key, import name, owner, capability, entry points, and surfaces for one external package.
- Entry: one evidence record per distribution under `.api/api-<distribution>.md`.
- Packages: `importlib.metadata` from the standard library.
- Output: generated API evidence read by package planning.
- Boundary: no package version tables in planning pages and no guessed environment status in owner plans.

[API_MEMBER]:
- Owns: official API surface rows that package-local source may later name.
- Entry: one surface list per external package capability.
- Output: admitted member names and owner notes.
- Boundary: source cannot name members absent from `.api` evidence or official docs captured by the package owner.

## [3]-[RED_TEAM]

- Remove stdlib logging calls from planned source unless they sit inside the structlog/OTel bridge owner.
- Remove receipt fields that duplicate AppHost envelope ownership.
- Remove API facts from planning pages when the `.api` folder already owns them.
- Reject source that imports an external package only to expose a weaker local rename.
