# [STACKS_PYTHON_AGENTS]

Scope: `docs/stacks/python/**`. Parent instructions and `docs/stacks/AGENTS.md` own stack topology; this file adds the local rules for building and maintaining the Python stack decision atlas.

## [1]-[READ_BEHAVIOR]

- When changing Python syntax, typing spellings, annotation APIs, import/startup form, template syntax, or standard-library primitive selection, read `language.md` and extend the canonical rule, replacement, or rejection.
- When changing Python shape, surface, rail, boundary, runtime, algorithm, platform, or proof policy, read `README.md` and the owning concept or planned route before editing `language.md`.
- When changing package-backed capability guidance, read `pyproject.toml`, the owning Python concept page, and `.planning/ROADMAP.md` when the concept page is still planned.
- When changing Python stack folder shape, update `README.md` and `.planning/ARCHITECTURE.md` so the chooser, planned topology, and current files agree.
- When changing Python stack sequence, update `.planning/ROADMAP.md`; do not put task state in `README.md`, `AGENTS.md`, or concept pages.

## [2]-[TARGETS]

Python `>=3.15` is the target language surface. This folder owns durable Python stack guidance for the repository.

Do not weaken target guidance with compatibility prose, version anxiety, partial-adoption caveats, package-shaped facades, or source-history narration. State the target rule, route current graph facts to `pyproject.toml` or the owning concept page, or mark a proof gap.

## [3]-[OWNER_CONTRACT]

Python concept pages own Python decisions. Extend the owning page by adding or correcting a rule, row, replacement, boundary record, or rejection.

Current package and tool graph truth stays in `pyproject.toml` until `platform/build-and-packages.md` exists. Missing package admission is recorded in the owning concept or active planning route.

## [4]-[PLANNING_ROUTE]

`.planning/ROADMAP.md` owns the Python stack buildout sequence. `.planning/ARCHITECTURE.md` owns planned topology and promotion targets until the planned pages become ordinary current files.

Delete planned-view records when the corresponding concept page exists and the README chooser reaches it. Do not copy roadmap task bodies, progress, or terminal work into this overlay.

## [5]-[REPORT_COMPANION]

When a Python roadmap task requires deep research before a concept page is created, use the scope-local report companion for reusable source scans, critique, synthesis, and candidate wording. Promote the final task-ready guidance into the matching `.planning/SPEC.<slug>.md`; keep reports as source material only.

Read `.reports/AGENTS.md` before creating or using Python stack reports. Do not copy report session naming, track mechanics, sub-agent notes, source inventories, or report outcomes into README, roadmap, architecture, or concept pages.

## [6]-[ROUTE_AWAY]

Route README orientation to `README.md`, current package/tool graph truth to `pyproject.toml` until `platform/build-and-packages.md` exists, package-backed coding policy to concept pages, implementation sequence to `.planning/ROADMAP.md`, planned structure to `.planning/ARCHITECTURE.md`, command syntax to tool owners, and documentation form to `docs/standards/`.

## [7]-[REJECTIONS]

- No Python package-specific files, folders, helper taxonomies, or upstream API catalogs.
- No standard-library fallback rule when the stronger package adoption gap belongs in `pyproject.toml`, a Python concept page, or active planning route.
- No version-by-version feature catalogs, copied provider manuals, package API inventories, command ladders, or validation ceremonies.
- No compatibility aliases, deprecation windows, wrapper facades, partial-adoption caveats, or current-baseline wording.
- No source/provenance sections; source material promotes only as durable rules, proof gaps, and rejection replacements.
