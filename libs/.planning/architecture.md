# [MONOREPO_ARCHITECTURE]

The canonical monorepo hierarchy law — the strata, the dependency direction, the universal-vs-Rhino-capture rule, the geometry/mesh/IFC flow, the `.planning/` lifecycle, the per-language roles, and the cross-language wire seams. This Tier-0 law owns the cross-cutting topology that no single branch or package can own; it is distinct from the authoring standard (`README.md`, which owns the doc-set tiers and page grammar) and the campaign method (`campaign-method.md`, which owns the planning loop). Per-package detail — owner registries, source trees, intra-package seams — lives in each folder `ARCHITECTURE.md` and is never restated here. Tier-0 is the only tier that names a language; branches and folders consume this topology as settled vocabulary.

## [1]-[STRATA]

The repository is one tri-language AEC platform organized into strict strata. Each stratum depends only upward; each package is a genuine higher-order domain, never a weak or mini sibling. The C# branch carries the durable host-bound source and the geometry/AEC capability; Python and TypeScript are host-free peer runtimes that consume the wire.

[KERNEL]
- Folder(s): `Rasm`
- The RhinoCommon-aware geometry/numeric kernel, with sub-domains Vectors, Analysis, Domain, and the robust-core Geometry. The branch base: referenced by every higher stratum, references none.

[AEC-DOMAIN]
- Folder(s): `Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`
- Host-neutral AEC capability: profiles/appearance/construction, the BIM object-model + IFC/glTF/STEP exchange, and portable fabrication (HLR/CAM/nesting). References the kernel and, minimally, its AEC-domain peers.

[APP-PLATFORM]
- Folder(s): `Rasm.AppHost`, `Rasm.Compute`, `Rasm.Persistence`, `Rasm.AppUi`
- Generic application capability: runtime spine, measured execution, durable stores, product UI. Composes the kernel and AEC-domain and owns no geometry, BIM semantics, or fabrication algorithms — it consumes them.

[HOST-BOUNDARY]
- Folder(s): `Rasm.Rhino`, `Rasm.Grasshopper`
- Self-contained, host-bound RhinoCommon/GH2 boundaries. Reference only the kernel; admitted only at the app roots, never as an interior dependency of a host-neutral package.

[APP]
- Folder(s): (future apps/plugins/services) `apps` by concern `apps/rhino`, `apps/grasshopper`, `apps/<X>`
- Compose APP-PLATFORM + HOST-BOUNDARY into product shells that declare intent, bind host edges, and emit output.

## [2]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata; the graph is acyclic with the kernel at the base and the app shells at the leaf.

- `Rasm` references no sibling and is referenced by every C# stratum above it.
- AEC-domain and app-platform reference the kernel; cross-references between AEC-domain and app-platform are minimal and one-directional (app-platform consumes AEC-domain capability, never the reverse).
- No host-neutral package (KERNEL, AEC-DOMAIN, APP-PLATFORM) references a HOST-BOUNDARY package. `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and are composed at the app roots.
- `Rasm.AppUi` is the app-platform consuming leaf; it is NOT the app composition root. The product composition that binds host boundaries (Rhino/GH2) lives at the future APP stratum, never inside app-platform. A scaffold `.csproj` that references `Rasm.Rhino`/`Rasm.Grasshopper` from `Rasm.AppUi` is redrawn to the strata: AppUi references the kernel, AppHost, Compute, and Persistence, and the host boundaries enter at the app root that composes a live host.

## [3]-[UNIVERSAL_VS_CAPTURE]

"Universal" never means host-free C#. The entire C# branch is RhinoCommon-aware, the kernel included. Universal means the portable cross-runtime contracts the host-free runtimes consume: content-identity, geometry payload/GLB, IFC/BIM semantics, material-profile-as-data, and the typed receipts. The host-free peers (Python, TypeScript) consume those contracts; they never consume RhinoCommon.

- Build a host-neutral owner ONLY when a genuinely non-Rhino consumer needs the contract. Otherwise capture the Rhino-native surface as a rich host-specific feature. Zero current consumers never lowers the capability bar, but a contract with no cross-runtime consumer is a Rhino feature, not a universal owner.
- `Rasm.Rhino/Exchange` and `Rasm.Rhino` drafting stay rich Rhino features and are NOT thinned. Rhino owns Make2D, sheet layout, and native file I/O; `Rasm.Bim` independently owns the universal IFC/exchange semantic model. The two coexist — Rhino-native capture and host-neutral semantics — and neither is gutted to feed the other.
- A host-neutral AEC-domain or app-platform owner expresses the universal contract; the host boundary expresses the native surface; they meet only at the contract, never by one re-implementing the other.

## [4]-[GEOMETRY_FLOW]

Geometry, meshing, and IFC each have exactly one owner per runtime; the runtimes meet only at the wire. No concern is owned twice within a runtime, and no runtime re-implements a peer's geometry.

- C# geometry source-of-truth is `Rasm` (the kernel). `Rasm.Compute` (tensor-encode/tessellate/solve), `Rasm.Persistence` (content-hash identity/federation), and `Rasm.Bim` (BIM model/IFC) CONSUME `Rasm` geometry; they never own or re-implement it.
- Python owns its OWN host-free geometry (`open3d`/`ifcopenshell`/`trimesh`) for offline scan/IFC work — an independent peer producer, not a `Rasm` consumer.
- `Rasm` and Python geometry meet only at the wire: content-identity plus the GLB/GeometryPayload tessellation rail. TypeScript consumes that wire for web render and owns no geometry.
- Meshing has one owner per runtime: Rasm-DEC (C#), Py-scan (Python), TS-render (TypeScript). IFC has one semantic owner per runtime: `Rasm.Bim` (C# semantics) and Python `ifcopenshell` (offline evaluation); they meet at the wire, never by duplication.

## [5]-[PLANNING_LIFECYCLE]

`.planning/` is a transient greenfield scaffold, not a permanent fixture. It exists to bring an under-developed folder to the decision-complete bar and dissolves as source lands; the eventual source tree is authored only when code is written.

- A greenfield package keeps its design pages inside one `.planning/` at the package root, organized into sub-domain sub-folders that mirror the eventual source tree (`<pkg>/.planning/<sub-domain>/<page>.md`). The package root carries the four index docs — `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md` — and nothing else.
- All planning lives under the single `.planning/`: never a `.planning/` inside a real source sub-folder. The package `ARCHITECTURE.md` maps the full folder structure (including planned sub-domains that have no page yet), so the map fuels ideas and tasks without scattering planning across the tree.
- Mature folders with real code at the bar carry NO `.planning/`, neither at the package root nor inside a sub-folder. Their co-located source architecture note is the only design surface (the `Vectors/_ARCHITECTURE.md` pattern); their open split/cleanup/re-architect work lives as task cards in the branch `TASKLOG.md`.
- The one exception is a genuinely-new unbuilt sub-domain inside an otherwise-mature package: it keeps its scaffold in that sub-domain folder (the robust-geometry `Rasm/Geometry/.planning/` is the canonical instance), because a package-root `.planning/` would wrongly imply the mature siblings (`Vectors/`, `Analysis/`, `Domain/`) are also in planning.
- `Rasm.Rhino` and `Rasm.Grasshopper` are out of scope: self-contained, durable, host-bound code with no `.planning/`.

## [6]-[PER_LANGUAGE_ROLES]

The three branches are first-class peers, each a complete library adoptable in any monorepo, coupled only at the wire. Their roles are distinct in kind, not a name-mirror of one another.

- C# is the Rhino9(WIP)/GH2-aware AEC platform: the geometry kernel, the AEC-domain (materials/BIM/fabrication), the app-platform, and the host boundaries. It is the producer of the wire vocabulary and the capability descriptors every peer consumes.
- Python is the host-free science/compute/data/geometry/IFC companion: mature-library leverage (numpy/scipy/open3d/ifcopenshell/pyarrow/polars) for offline analysis, tessellation, data interchange, and artifact/visualization production. It integrates only through the wire and the companion/offline seams.
- TypeScript is the host-free web/edge platform: wire-interchange, the browser SPA + UI, and durable node services. It consumes the C# wire only and owns no geometry.

Within each language the same organization principle holds: real higher-order domain folders (no weak or mini sibling — a small isolated concern folds into the bigger concept it belongs to), source-mirroring sub-domain organization, OOP capsules at boundaries and FP-ROP internals. A branch re-derives its topology from the finalized owner set, never from a stale layout.

## [7]-[CROSS_LANGUAGE_WIRE]

The branches couple ONLY through the wire contracts and the companion/offline seams; coupling beyond them is a defect. This section states the wire law; the concrete integration point for any one seam lives as a boundary/wire consideration on the task that builds it, not in a standalone cross-reference ledger that drifts.

- Each shared canonical concept carries ONE name and ONE owner across the three branches, consumed at the boundary and never re-minted. The shared owners are content-address identity (the `XxHash128` seed), the proto wire vocabulary, the suite wire law, the op-log CRDT payload, the capability descriptor + SDK source, tenant/causal identity, the geometry/IFC tessellation rail (GLB), and graduation evidence.
- C# owns the wire vocabulary; Python and TypeScript decode it and never mint a parallel. Python owns the geometry/IFC evaluation companion (the `IFC → IfcOpenShell → GLB` two-hop); C# requests and re-imports; TypeScript consumes the GLB for web render.
- Content-identity is reproduced bit-identically in all three runtimes from the one C#-owned seed, so an artifact computed by any runtime is reusable by the others.
- A second mint of a shared concept in any branch is the named cross-language drift defect. The cross-`libs/` `IDEAS.md` and `TASKLOG.md` carry the cross-language concert and the seams it spans; each branch states only its own owner of a shared concept and consumes the peer's at the boundary.
