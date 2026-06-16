# [PYTHON_LIBRARIES]

`libs/python` is the campaign root for Python library planning, external-package API evidence, and package-local transcription plans. The root `pyproject.toml` is the only Python manifest; package folders do not carry package-local manifests.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                 | [OWNER]                 |
| :-----: | :----------------------------------------------------- | :---------------------- |
|   [1]   | [campaign method](campaign-method.md)                  | campaign cadence        |
|   [2]   | [architecture](architecture.md)                        | package topology        |
|   [3]   | [API catalogues](api-catalogues.md)                    | evidence protocol       |
|   [4]   | [features](FEATURES.md)                                | capability atlas        |
|   [5]   | [tasklog](TASKLOG.md)                                  | open work               |
|   [6]   | [runtime planning](../runtime/.planning/README.md)     | runtime package plan    |
|   [7]   | [data planning](../data/.planning/README.md)           | data package plan       |
|   [8]   | [compute planning](../compute/.planning/README.md)     | compute package plan    |
|   [9]   | [artifacts planning](../artifacts/.planning/README.md) | artifacts package plan  |
|  [10]   | [page regions](region-map/page-regions.md)             | planning regions        |
|  [11]   | [owner symbols](region-map/owner-symbols.md)           | symbol ledger           |
|  [12]   | [seam splits](region-map/seam-splits.md)               | cross-owner seams       |
|  [13]   | [API owners](region-map/api-owners.md)                 | package evidence owners |

## [2]-[OWNER_RECORDS]

[RUNTIME]:
- Package: `libs/python/runtime`
- Owns: context admission, boundary rails, resources, local receipts, API evidence reading, and Python-local concurrency lanes.
- Boundary: host lifecycle, global health, product telemetry export, support capture, and service-root composition stay in `Rasm.AppHost`.
- Planning: [runtime planning](../runtime/.planning/README.md).

[DATA]:
- Package: `libs/python/data`
- Owns: portable datasets, columnar scans, query plans, schema claims, geospatial claims, graph payloads, and AEC/file exchange bundles.
- Boundary: durable stores, schema migrations, product repositories, query rails, and Rhino/GH document mutation stay outside Python libraries.
- Planning: [data planning](../data/.planning/README.md).

[COMPUTE]:
- Package: `libs/python/compute`
- Owns: offline array admission, solver comparison, symbolic derivation, units, uncertainty, study data, model assets, and C# graduation evidence.
- Boundary: production compute runtime, benchmark authority, substrate selection, tensor sessions, and product receipts stay in `Rasm.Compute`.
- Planning: [compute planning](../compute/.planning/README.md).

[ARTIFACTS]:
- Package: `libs/python/artifacts`
- Owns: document, PDF, image, Office, visualization, compression, content identity, and export bundles.
- Boundary: live UI controls, dashboard runtime, browser state, product artifact stores, and AppUi evidence timelines stay outside this package.
- Planning: [artifacts planning](../artifacts/.planning/README.md).

## [3]-[BUILD_SEQUENCE]

1. Stabilize the central campaign pages and package-local planning pages.
2. Admit dependencies only through the root `pyproject.toml`.
3. Capture package-owned external API evidence under `libs/python/<package>/.api/api-<distribution>.md`.
4. Refine package-local design pages until every planned symbol has one owner, one boundary, and one API evidence route.
5. Start source transcription only after the package-local planning page names the owner cluster and its `.api` evidence route.

## [4]-[FILE_PROCESS]

[ROOT_PLANNING]:
- Path: `libs/python/.planning`
- Role: suite-level charter, topology, API-evidence protocol, region maps, and cross-owner law.

[PACKAGE_PLANNING]:
- Path: `libs/python/<package>/.planning`
- Role: package-local owner pages modeled after the C# package planning pattern.

[API_EVIDENCE]:
- Path: `libs/python/<package>/.api/api-<distribution>.md`
- Role: external package capability evidence for the package owner.

[PACKAGE_ROOT]:
- Path: `libs/python/<package>`
- Role: package README, `.api`, `.planning`, and future source root. No package-local `pyproject.toml` exists.

[MANIFEST]:
- Path: `pyproject.toml`
- Role: the only Python dependency and tool manifest for this library campaign.

## [5]-[PROOF_GATES]

- Layout proof: confirm `libs/python` has no Python source files, no package-local manifests, and no nested API folders.
- Whitespace proof: run `git diff --check -- libs/python pyproject.toml`.
- Manifest proof: run `uv lock --check` or refresh the lock once after dependency-group edits.

## [6]-[PROHIBITIONS]

- Package-local `pyproject.toml` files.
- nested source-root layout.
- Python source files before the package-local planning and API-evidence pass admits them.
- Empty `__init__.py` marker files.
- Barrel exports, wildcard imports, facade-only modules, and one-hop re-export files.
- Product host lifecycle, durable stores, Rhino/GH mutation, live UI, browser dashboard runtime, production compute receipts, or C# wire ownership inside Python packages.
- Release columns, universal state columns, provenance tails, prompt narration, checklist history, and dependency facts copied away from the root manifest or `.api` evidence.

## [7]-[REFINEMENT_HORIZON]

Future passes deepen package-local plans from `.api` evidence into source. Splits such as `visuals`, `exchange`, and `studies` require owner pressure, API evidence volume, and a named consumer before they become packages.
