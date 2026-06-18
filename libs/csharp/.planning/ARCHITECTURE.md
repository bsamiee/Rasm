# [CSHARP_BRANCH_ARCHITECTURE]

The C# branch domain map: the strata-ordered package roster and the one project-reference graph. Each package is a genuine higher-order domain; the per-package sub-domain structure, owners, and seams live in each folder `ARCHITECTURE.md` and are never restated here. This node is the only place the C# dependency direction is stated; cross-folder wires live on the folder task cards, never in a branch seam ledger.

## [1]-[PACKAGE_ROSTER]

The branch is one Rhino9(WIP)/GH2-aware AEC platform organized into strict strata. The kernel carries the durable geometry/numeric capability; the AEC-domain packages are host-neutral; the app-platform packages are generic application capability; the host boundaries are self-contained and admitted only at the future app roots.

```text codemap
libs/csharp/
├── Rasm/                # KERNEL — RhinoCommon-aware geometry/numeric kernel: Vectors/Analysis/Domain + the Geometry/ robust-core (predicates, spatial index, topology, healing, constraints, tessellation)
├── Rasm.Materials/      # AEC-DOMAIN — host-neutral profiles + appearance + construction: ProfileFamily cross-sections, the measured BSDF/spectral/photometric engine, the element→assembly→layout model
├── Rasm.Bim/            # AEC-DOMAIN — host-neutral BIM object model + IFC/glTF/STEP exchange codec + element-set/classification/assembly + the planned properties/validation/coordination/georeferencing owners
├── Rasm.Fabrication/    # AEC-DOMAIN — host-neutral portable fabrication frontier: HLR/hidden-line, CAM toolpath + serial-chain kinematics, 2D true-shape nesting, the portable cut-program emitter over a Clipper2 polygon floor
├── Rasm.AppHost/        # APP-PLATFORM — runtime spine: host profiles, lifecycle/drain, time, configuration, resources, observability, outbound, the seven ports, provisioning, capability/sandbox/determinism
├── Rasm.Compute/        # APP-PLATFORM — measured execution: the tensor vocabulary/residency/layout/dispatch sub-domains, the numeric algebra, the model identity/sessions/providers/extension-ops/inference/generative sub-domains, the suite wire vocabulary, the field/geometry codecs + tessellation bridge, the solver discretization/solve-contract/optimizer/sweep/clash sub-domains, the units boundary, the receipt union
├── Rasm.Persistence/    # APP-PLATFORM — durable stores: store profiles (with the in-stores classification + 5D cost catalog), object-store/server tiers, schema/query rails, snapshots, cache indexes, sync, version-control, federation, provenance, annotation, schedule
├── Rasm.AppUi/          # APP-PLATFORM — Avalonia product UI: surface hosts, shell, screens, commands, charts, visuals, viewport, drafting, notebook, animation, accessibility, theme/typography/motion vocabularies
├── Rasm.Rhino/          # HOST-BOUNDARY (out-of-scope-durable) — RhinoCommon host APIs: capture, events, commands, exchange, drafting; references only Rasm, admitted only at the future app roots
└── Rasm.Grasshopper/    # HOST-BOUNDARY (out-of-scope-durable) — GH2 host APIs: components and UI; references only Rasm, admitted only at the future app roots
```

The eight planning-scoped packages carry a `.planning/` scaffold with the four index docs and design pages; `Rasm` keeps its scaffold only in the greenfield `Geometry/` robust-core because its `Vectors`/`Analysis`/`Domain` siblings are mature source. `Rasm.Rhino` and `Rasm.Grasshopper` are durable host-bound source with no `.planning/`; their open work the future app root composes rides this branch `TASKLOG.md`.

## [2]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata; the graph is acyclic with the kernel at the base, `Rasm.AppHost` as the app-platform root, and the app shells at the future leaf. This is the project-reference graph the eight folders consume as settled vocabulary.

- `Rasm` references no sibling and is referenced by every C# stratum above it.
- The AEC-domain packages (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) reference only `Rasm` and, minimally, each other. They are host-neutral and consume an app-platform capability only at a content-keyed wire seam (reproduced bit-identically), never through an upward project reference.
- `Rasm.AppHost` references nothing and is referenced by its app-platform siblings; `Rasm.Persistence` references `Rasm.AppHost`; `Rasm.Compute` references `Rasm`, `Rasm.AppHost`, and `Rasm.Persistence`; `Rasm.AppUi` references `Rasm`, `Rasm.AppHost`, `Rasm.Compute`, and `Rasm.Persistence`. `Rasm.Compute` references `Rasm.Persistence` for the cache/artifact/benchmark indexes; `Rasm.Persistence` never references `Rasm.Compute`.
- No host-neutral package (KERNEL, AEC-DOMAIN, APP-PLATFORM) references a HOST-BOUNDARY package. `Rasm.AppUi` is the app-platform consuming leaf and references no host boundary. `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and enter at the future app root that composes a live host.
- `Rasm.Bim` consumes the Compute content-identity seed and the tessellation rail as settled wire vocabulary, never through a `Rasm.Compute` project reference, so the AEC-domain never depends upward on the app-platform.
