# [PY_RUNTIME]

`runtime` owns Python-local context, resource roots, API evidence, rails, lane policy, and receipts for `libs/python`. This package currently contains planning and API placeholders only; future source lands directly in this folder after the planning and API pages are filled.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: context admission, rails, resources, lanes, observability, and API evidence planning.

[API]:
- Path: `.api/README.md`
- Owns: flat `api-*.md` placeholders for runtime dependencies.

[BOUNDARY]:
- Runtime receives host facts from app and tool owners.
- `Rasm.AppHost` remains the owner for host lifecycle, global clock, health, support capture, and product runtime composition.
