# [PYTHON_STACK_ARCHITECTURE]

This planning architecture explains `docs/stacks/python/` as the Python stack decision atlas. The codemap records current and planned docs paths; the roadmap owns sequence, and concept pages own durable Python coding decisions when promoted.

## [1]-[CODEMAP]

```text conceptual
docs/stacks/python/                              # Python stack decision atlas
├── AGENTS.md                                    # leaf overlay for Python stack docs maintenance
├── README.md                                    # chooser by Python stack decision
├── language.md                                  # active Python >=3.15 language surface
└── .planning/                                   # scope-local planning state
    ├── ARCHITECTURE.md                          # planned topology and promotion targets
    └── ROADMAP.md                               # active stack-doc buildout sequence
```

Planned concept pages are not shown as current paths until created. Their promotion targets are listed in `Planned view`.

## [2]-[SCOPE_BOUNDARY]

Included: `docs/stacks/python/`, its README chooser, leaf overlay, language decision page, and scope-local planning files.

Excluded: Python source implementation, tool command syntax, exact package version tables, generated outputs, and runtime validation logs.

Adjacent routes: `pyproject.toml` carries current Python package and tool graph truth; `docs/stacks/AGENTS.md` carries stack-parent topology; `docs/standards/` carries documentation form.

Reader rule: edits under `docs/stacks/python/` must keep the README chooser, leaf overlay, roadmap, and planned architecture aligned.

## [3]-[PROJECT_IDENTITY]

Project file: `pyproject.toml`

Package or export surface: not applicable; this scope is documentation, not a Python package.

Build target: none owned here.

Generated outputs: none owned here.

Proof route: documentation review, link and anchor checks, and `git diff --check` for changed docs.

## [4]-[CONTRACTS_GENERATED_TRUTH]

This scope owns no generated contract. If a future Python stack page represents generated output or parser-owned Markdown, route generated-truth fields to the relevant concept page and keep generator mechanics out of this planning architecture.

## [5]-[ENTRYPOINTS_AND_FLOWS]

Flow starts at [README](../README.md), selects the concept page for the coding decision, and falls back to [ROADMAP](ROADMAP.md) only when the concept page is planned but not yet promoted.

The [AGENTS](../AGENTS.md) overlay changes maintenance behavior for this folder. It does not carry the language rules, package graph state, roadmap task bodies, or architecture codemap.

## [6]-[DEPENDENCY_DIRECTION]

Stack concept pages may consume `pyproject.toml`, source, generated contracts, maintained Python documentation, and trusted instruction overlays as proof. Current graph facts flow from manifests into platform or concept owners; target capability stays in the concept page that owns the decision.

## [7]-[INVARIANTS]

- Concept pages are named by domain, category, or concept, not external libraries.
- Package-backed capability is written into the concept page that owns the decision.
- `language.md` is not a version changelog.
- `.planning/` contains sequence and planned topology only while it changes agent action.
- README chooser rows link to current pages when they exist and use future labels only for planned pages.

## [8]-[STATUS_AND_ROADMAP]

Roadmap: [ROADMAP](ROADMAP.md).

Current status: seed files are the active buildout surface. Planned concept pages become current only when created and linked from the README chooser.

## [9]-[PLANNED_VIEW]

[CONCEPT_PAGES]:
- Planned structure: `domain-shapes.md`, `surfaces-and-dispatch.md`, `rails-and-effects.md`, `boundaries-and-codecs.md`, `runtime-and-concurrency.md`, `algorithms-and-data.md`, `platform/build-and-packages.md`, and `testing/README.md`.
- Current anchor: [README](../README.md) future chooser rows.
- Source: [ROADMAP](ROADMAP.md) P-0020.
- Use now: reserve decision-based names and prevent operational, package-shaped, or skill-shaped docs.
- Promotion target: files under `docs/stacks/python/`.
- Promote when: the concept page exists, README links it, and the page owns at least one durable coding decision.
- Remove when: all planned concept pages are current and no planned route changes agent action.

## [10]-[PROOF]

Proof for this planning architecture is path review against `docs/stacks/python/`, `pyproject.toml`, the README chooser, and the roadmap.

Proof gap: planned concept pages do not exist yet; the roadmap owns their execution sequence.

## [11]-[BOUNDARIES]

Architecture carries current and planned topology. Roadmap carries task order and progress. README carries reader orientation. AGENTS carries local maintenance behavior. Concept pages carry coding decisions.

Do not preserve this planning architecture as current structure after the planned concept pages become ordinary docs.

## [12]-[VALIDATION]

- [ ] Codemap lists only current paths.
- [ ] Planned pages appear only in `Planned view`.
- [ ] Every planned path has a promotion target and removal trigger.
- [ ] README chooser, roadmap, and planned architecture agree.
- [ ] No planned page is named after an external library.
