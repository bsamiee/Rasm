# [PY_RUNTIME_API_CATALOGUE]

`runtime` API catalogue pages capture decompile-grounded external package surface for the Python-local execution foundation. Each `api-<distribution>.md` page records the package surface, public types, entrypoints, and local admission law before runtime source names that API. Versions live only in the root `pyproject.toml`; these pages carry the reflected import name and version as evidence.

## [1]-[PACKAGE_PAGES]

[RAILS]:
- rail: rails
- pages:
  - [api-expression.md](api-expression.md)

[VALIDATION]:
- rail: validation
- pages:
  - [api-pydantic.md](api-pydantic.md)
  - [api-pydantic-settings.md](api-pydantic-settings.md)
  - [api-beartype.md](api-beartype.md)

[SERIALIZATION]:
- rail: serialization
- pages:
  - [api-msgspec.md](api-msgspec.md)

[CONCURRENCY]:
- rail: concurrency
- pages:
  - [api-anyio.md](api-anyio.md)

[RESILIENCE]:
- rail: resilience
- pages:
  - [api-stamina.md](api-stamina.md)

[OBSERVABILITY]:
- rail: observability
- pages:
  - [api-structlog.md](api-structlog.md)
  - [api-opentelemetry-api.md](api-opentelemetry-api.md)
  - [api-opentelemetry-sdk.md](api-opentelemetry-sdk.md)
  - [api-opentelemetry-exporter-otlp-proto-http.md](api-opentelemetry-exporter-otlp-proto-http.md)
  - [api-psutil.md](api-psutil.md)

[RESOURCES]:
- rail: resources
- pages:
  - [api-fsspec.md](api-fsspec.md)
  - [api-s3fs.md](api-s3fs.md)
  - [api-gcsfs.md](api-gcsfs.md)
  - [api-universal-pathlib.md](api-universal-pathlib.md)

[TRANSPORT]:
- rail: transport
- pages:
  - [api-httpx.md](api-httpx.md)
  - [api-asyncssh.md](api-asyncssh.md)
  - [api-specklepy.md](api-specklepy.md)
  - [api-grpcio.md](api-grpcio.md)
  - [api-grpcio-tools.md](api-grpcio-tools.md)
  - [api-protobuf.md](api-protobuf.md)

[AUTOMATION]:
- rail: automation
- pages:
  - [api-watchfiles.md](api-watchfiles.md)
  - [api-aiocron.md](api-aiocron.md)

[PARSING]:
- rail: parsing
- pages:
  - [api-tree-sitter.md](api-tree-sitter.md)
  - [api-tree-sitter-python.md](api-tree-sitter-python.md)
  - [api-tree-sitter-typescript.md](api-tree-sitter-typescript.md)

[ENTRY]:
- rail: entry
- pages:
  - [api-cyclopts.md](api-cyclopts.md)

## [2]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- API pages carry decompile-grounded external package API facts and package-rail admission records.
- Planning pages carry owner boundaries and source-transcription law.
- README pages route catalogues without duplicating member tables.

[EVIDENCE_ROUTE]:
- Surface is captured through `uv run python -m tools.assay api query --key <distribution> --sources pydist --full`, with `--symbol` for member-level detail; the `--sources pydist` scope forces the Python distribution over any same-named host artifact.
- A distribution absent from the active environment preserves its page with un-reflected members marked in a TASKLOG_GAP section, never invented.
