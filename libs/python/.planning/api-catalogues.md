# [PYTHON_API_CATALOGUES]

`.api` folders prevent underuse of external packages. A planning page or future source module names an external API member only after the package owner captures evidence under `libs/python/<package>/.api/api-<distribution>.md`.

## [1]-[EVIDENCE_RECORD]

Each distribution record carries these fields as a definition record or compact field list:
- Distribution: root manifest package key.
- Import name: module path the owner expects to import.
- Owner: one package owner from `runtime`, `data`, `compute`, or `artifacts`.
- Status: `admitted`, `pending`, or `rejected`.
- Installed: distribution metadata signal when the active environment contains it.
- Entry points: installed console or plugin entry point names when present.
- Surfaces: primary classes, functions, builders, codecs, engines, exporters, or protocols the owner intends to use.
- Docs: official API reference links or local generated API evidence routes.
- Boundary: owner-specific rule preventing wrapper-renames and cross-package leakage.

## [2]-[PACKAGE_FOLDERS]

- runtime: `libs/python/runtime/.api/api-<distribution>.md`
- data: `libs/python/data/.api/api-<distribution>.md`
- compute: `libs/python/compute/.api/api-<distribution>.md`
- artifacts: `libs/python/artifacts/.api/api-<distribution>.md`

## [3]-[CAPTURE_ORDER]

1. Admit or hold the dependency in the root `pyproject.toml`.
2. Capture distribution metadata from the active environment when installed.
3. Record import names without moving import policy into planning prose.
4. Capture official API surfaces and local generated API evidence.
5. Bind the distribution to one package owner in [API owners](region-map/api-owners.md).
6. Update the package-local planning page with only the owner consequence.

## [4]-[UNDERUSE_SCAN]

Reject any local function that renames a package API without adding owner policy, receipt projection, boundary admission, or cross-package translation. Reject any package claim that names a broad library but uses only the weakest stdlib-shaped subset.
