# [PY_RUNTIME_CONTEXT_SETTINGS]

Context admission turns caller-owned host facts into a Python-local immutable record. It never discovers the host, starts services, owns lifecycle, or derives product roots.

## [1]-[CONTEXT_OWNER]

[RUNTIME_CONTEXT]:
- Owns: profile key, correlation id, resource root, deadline, data classification, and metadata rows.
- Input: caller-provided facts from AppHost, Assay, bridge, sidecar, or a package-local tool.
- Output: one admitted context record plus one local receipt.
- Packages: `pydantic`, `pydantic-settings`, `msgspec`.
- API evidence: `.api/api-pydantic.md`, `.api/api-pydantic-settings.md`, `.api/api-msgspec.md`.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context.

[PROFILE]:
- Owns: Python-local runtime profile vocabulary for `tool`, `sidecar`, `package`, and `test`.
- Input: one literal profile key.
- Output: profile row with import, filesystem, observability, and concurrency policy columns.
- Boundary: C# `HostProfile` remains AppHost-owned and is never mirrored row-for-row.

[CORRELATION]:
- Owns: local correlation id and optional trace-context carrier projection.
- Input: caller-provided correlation or local mint at a Python boundary.
- Output: normalized correlation record.
- Boundary: AppHost remains the suite-wide correlation and distributed trace owner.

## [2]-[SETTINGS_OWNER]

[SETTINGS_ADMISSION]:
- Owns: local settings source order for Python-only tools and package internals.
- Input: explicit mapping, environment-backed settings object, or caller-supplied config payload.
- Output: frozen settings record with rejected unknown fields.
- Packages: `pydantic-settings`, `pydantic`.
- Boundary: no package reads process environment directly after context admission.

[DEADLINE]:
- Owns: Python-local deadline value and elapsed measurement receipt fields.
- Input: caller-provided deadline or bounded package-local default from profile policy.
- Output: deadline record that AnyIO scopes can consume later.
- Boundary: global clock, fleet deadlines, and host deadline taxonomy remain AppHost-owned.

## [3]-[RED_TEAM]

- Collapse every new context field into `RuntimeContext` before adding sibling records.
- Reject settings classes that read environment variables outside the admission owner.
- Reject profile rows that duplicate AppHost host profiles.
- Reject package-level globals that cache current context.
