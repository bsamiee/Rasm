# [PY_RUNTIME_PLANNING]

`runtime` is the Python-local foundation package. It admits caller-provided context, fault rails, resources, deadlines, local receipts, observability contributors, and API evidence without becoming `Rasm.AppHost`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] |
| :-----: | :----- | :----- |
|   [1]   | [context settings](context-settings.md) | profile, settings, correlation, deadline, and context admission |
|   [2]   | [rails resources](rails-resources.md) | Result/Option rails, resource roots, filesystem adapters, and drain lanes |
|   [3]   | [observability API](observability-api.md) | local receipts, structlog/OTel contribution, and API evidence records |

## [2]-[OWNER_CLUSTERS]

[CONTEXT]:
- Owner symbols: `RuntimeContext`, `RuntimeProfile`, `Correlation`, `Deadline`.
- Entry surface: one admission function receiving caller-owned host facts.
- API routes: `.api/api-pydantic.md`, `.api/api-pydantic-settings.md`, `.api/api-msgspec.md`.
- Boundary: no host profile resolver, host lifecycle, global clock, or service root.

[RAILS]:
- Owner symbols: `BoundaryFault`, `RuntimeRail`.
- Entry surface: one boundary conversion function that turns exceptions into rail failures only at ingress/egress.
- API routes: `.api/api-expression.md`, `.api/api-beartype.md`, `.api/api-stamina.md`.
- Boundary: no broad exception taxonomy copied from C# owners.

[RESOURCES]:
- Owner symbols: `ResourceRoot`, `ResourceRef`.
- Entry surface: one resource-root admission and resolution surface.
- API routes: `.api/api-fsspec.md`, `.api/api-s3fs.md`, `.api/api-gcsfs.md`, `.api/api-universal-pathlib.md`, `.api/api-httpx.md`, `.api/api-asyncssh.md`.
- Boundary: no product store roots, support-bundle roots, bridge staging roots, or scratch defaults.

[LANES]:
- Owner symbols: `LanePolicy`, `DrainReceipt`.
- Entry surface: one structured-concurrency lane surface.
- API routes: `.api/api-anyio.md`, `.api/api-watchfiles.md`, `.api/api-aiocron.md`.
- Boundary: no job framework, scheduler service, retry law, or background service lifecycle.

[OBSERVABILITY]:
- Owner symbols: `Receipt`, `ReceiptContributor`, `ApiEvidence`.
- Entry surface: one local receipt contribution surface and one API-evidence reader surface.
- API routes: `.api/api-structlog.md`, `.api/api-opentelemetry-api.md`, `.api/api-opentelemetry-sdk.md`, `.api/api-opentelemetry-exporter-otlp-proto-http.md`, `.api/api-opentelemetry-instrumentation-logging.md`.
- Boundary: no exporter ownership, health state, support capture, or product telemetry pipeline.

## [3]-[TRANSCRIPTION_LAW]

- Future source lands directly under `libs/python/runtime`.
- Public shapes stay in the file that owns the implementation.
- External package members used by source must appear in the matching `.api/api-<distribution>.md` record first.
- Runtime is the only first-wave package with no first-wave package import.
