# [PYTHON_BRANCH]

The Python branch file router and the cross-cutting package registry: a first-class host-free science/compute/data/geometry/IFC library of five peer packages, held to the cross-language density bar in idiomatic modern Python. The single root `pyproject.toml` owns the Python `>=3.15` project and dependency groups; the Forge companion lane owns the `<'3.13'` native geometry/IFC cores and the `grpcio-tools` codegen compiler outside that manifest, while the `grpc.aio` runtime (`grpcio`/`protobuf`) is already present transitively on the core through `specklepy`. This file routes the branch index docs and registers the packages shared across folders; `ARCHITECTURE.md` carries the package map and dependency direction, `IDEAS.md` the cross-package concert, and `TASKLOG.md` the cross-package work.

## [1]-[ROUTER]

- `ARCHITECTURE.md` — the five-package domain map, the dependency direction stated once (runtime mints the shared shapes; the four consumers compose them and never re-mint), and the interpreter floor.
- `IDEAS.md` — the cross-package Python concert: ideas coupling the packages to each other, distilled from the folder ideas.
- `TASKLOG.md` — the cross-package open work, including the companion-environment floor gate.
- Per-package planning: each of `runtime/`, `compute/`, `data/`, `geometry/`, and `artifacts/` carries its own `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`, and design pages under `<package>/.planning/<sub-domain>/<page>.md`.

## [2]-[CROSS_CUTTING_PACKAGES]

The packages shared across two or more folders, registered once here and trimmed from the per-folder registries. Per-folder-only packages stay on the owning folder `README.md`. Root-compatible package versions live in the root manifest; companion-floor rows carry no pin here.

The typing/modelling, concurrency, observability, numeric-core, and companion-wire tiers below each carry one catalogue at the branch `libs/python/.api/<dist>.md`, authored once and never duplicated into a folder `.api/`; a consuming folder names them in its README `[CROSS_CUTTING]` section. The companion native-geometry, mesh-file-interchange, and object-store tiers are domain seams that keep a per-folder catalogue in each consuming folder, not a branch catalogue.

- Typing and modelling: `msgspec`, `pydantic`, `beartype`, `expression`.
- Concurrency and process: `anyio`.
- Observability: `structlog`, `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, `psutil`.
- Numeric core: `numpy`.
- Companion wire: `grpcio-tools` (the `protoc` codegen compiler) is companion-lane-only on the Forge `<'3.13'` interpreter (`forge-companion-env`, python312); `grpcio` (the `grpc.aio` runtime) and `protobuf` resolve transitively on the cp315 core through `specklepy`, so a page needing only the runtime leg sits on the core and only proto codegen assumes the companion interpreter.
- Companion native geometry: `ifcopenshell`, `open3d`, `small-gicp`, `topologicpy` — provided by the Forge companion lane (`<'3.13'`), not cp315 core dependencies.
- Mesh-file interchange (data emits, geometry consumes at the mesh seam): `meshio`, `trimesh`, `rhino3dm`.
- Object-store transport (runtime owns; data egress composes): `obstore`.
