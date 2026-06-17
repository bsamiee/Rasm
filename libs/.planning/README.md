# [PLANNING_STANDARD]

The authoring standard for the `libs/` planning corpus. It owns the doc-set per tier, the four index-doc templates, and the design-page grammar. Form — containers, tables, markers, prose — follows `docs/standards/information-structure.md`, `formatting.md`, and `style-guide.md`; the monorepo topology follows `architecture.md`. Neither is restated here.

## [1]-[DOC_SET]

Three tiers carry the same intent at widening scope: a folder owns one package's planning, a branch aggregates its language, and the cross-`libs/` core binds the three languages. Only the cross-`libs/` core names another language; a branch or folder consumes a peer only through the wire contracts.

- Cross-`libs/` core (`libs/.planning/`): `architecture.md` (the topology law), `campaign-method.md` (the planning loop), `README.md` (this standard), `IDEAS.md`, `TASKLOG.md`.
- Branch (`libs/<lang>/.planning/`): `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`.
- Folder (`<pkg>/`): the four index docs at the package root — `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md` — plus design pages under one `<pkg>/.planning/<sub-domain>/<page>.md` and a `<pkg>/.api/` of generated package catalogues, one per external library the folder uses. No `FEATURES.md`, no `.planning/README.md`. The `.api/` lives per folder, never consolidated to a branch or project-level home: it is the resource a planning pass reads to draw an external package's real API without guessing.

The `.planning/` lifecycle — the transient greenfield scaffold, the single-`.planning/`-per-package nesting rule, and the dissolution once a folder lands code — is owned by `architecture.md`; the cross-`libs/` core and each branch `<lang>/.planning/` are the standing exceptions that persist for the campaign duration. This standard owns only the doc-set, the templates, and the page grammar those folders carry.

## [2]-[INDEX_DOCS]

Each index doc opens with one or two declarative lines stating its own organization, then its content. Keep each load-bearing: a file that only restates another doc is deleted, not maintained.

[README] — the folder's file router and external-package registry.
- Router: a short index of the design pages under `.planning/`, so a reader navigates the folder without scanning the tree.
- Packages: every external library the folder uses, planned or implemented, as a flat list. No version pin (versions are centralized in the one owning manifest), and no link into `.api/` (the catalogue is expected at the folder's own `.api/<package>.md`; coupling the README to it is fragile). New admissions land here from the folder's ideas and tasks.
- Centralization is absolute: no per-package manifest exists (no per-folder `pyproject.toml`, `package.json`, or `*.props`); every package and version lives in the one language manifest.

[ARCHITECTURE] — the folder's professional domain map.
- The full sub-domain structure of the package, including planned sub-domains that hold no design page yet, each named by its real domain concept — never a `rail`/`axis`/`lane` file-naming scheme — with a one-line charter. A codemap tree is the carrier.
- The map fuels ideas and tasks: a planned-but-empty sub-domain is a visible gap. Dependency direction is stated once in the branch `ARCHITECTURE.md` and never restated per folder; there is no owner-state registry and no standalone seam ledger — boundaries and wires live on the tasks that build them.

[IDEAS] — the folder's forward pool of higher-order concepts.
- Two sections, `[1]-[OPEN]` and `[2]-[CLOSED]`. Each idea is a card: a bracketed slug leader plus a few bullets — the capability, what it unlocks, and the gap or modern technique it draws on. A folder idea is a bigger concept (a new file, sub-domain, or capability) grounded in the folder's domain and the monorepo purpose, never speculation.
- An idea drives one or more tasks. A finished or dropped idea moves to `[CLOSED]` with a one-line disposition, so the same idea is never re-litigated.

[TASKLOG] — the folder's open and closed work, distilled from its ideas.
- Two sections, `[1]-[OPEN]` and `[2]-[CLOSED]`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries/wires (internal to the folder or aligned to a sibling or branch, never coupled), and the key considerations.
- A task is scoped guidance — not a full spec and not vague. One idea spawns one or more tasks across one or more files; each task names the exact file or sub-domain it lands in.

## [3]-[DESIGN_PAGES]

Design pages live at `<pkg>/.planning/<sub-domain>/<page>.md`, one sub-domain folder per eventual source sub-tree, one page per eventual source file. A page is a decision-complete blueprint an implementation agent transcribes, not a narrative or research log.

- H1 `# [<PKG>_<PAGE>]`; one declarative lead paragraph; sections `## [k]-[TOKEN]` numbered from 1, section [1] the index of the page's clusters.
- A cluster carries a card (owner, packages, growth, and the boundary/receipt/entry lines it earns), then transcription-complete signature fences, then at most one Mermaid diagram.
- Signature fences are transcription-complete: every generated-owner knob, closed-family key, union case with its payload, and entrypoint signature is written so an agent copies it verbatim; bodies are written where the body is the law. Fences carry zero comments; invariants live on the card. Every literal traces to an axis on the page or an earlier page, or becomes a RESEARCH item.
- An external member is written only after the folder's `.api/` catalogue verifies its spelling; an unverified member is a RESEARCH item, never prose.

## [4]-[NOTATION]

One integration-point notation, scope-qualified by distance: `page#CLUSTER` inside a folder, `pkg/page#CLUSTER` across folders in a language, `lang:pkg/page#CLUSTER` across languages (cross-`libs/` only). A type name recurs across packages only when the concepts are genuinely distinct in distinct namespaces; a recurring wire-projection name is disambiguated at the source, never carried twice.

## [5]-[LANGUAGE]

- READ: `docs/standards/style-guide.md`, `docs/standards/information-structure.md`, `docs/standards/formatting.md`
- Agent-directed declarative present tense; the doc states law as fact. No reader address, narration, process, or provenance — no links, URLs, versions, dates, or session context on a design page.
- Banned hedges (word-boundary, page-wide): should, could, would, might, maybe, perhaps, likely, probably, propose, consider, recommended, ideally, TBD, TODO, FIXME, we, our, you, and the synonym forms — is expected to, can be, aims to, is designed to, in the future, eventually, as needed, if necessary. Future tense is legal only on a card growth line and a RESEARCH item.
- Vocabulary, owners, and policy values from earlier pages and the route's code doctrine arrive settled and are never re-taught. A fact owned by a sibling is composed inside a fence, never re-explained.

## [6]-[REVIEW]

Review is judgment against this standard and the route-owned code doctrine, not checklist pedantry. A reviewing agent reads the standard, the language's `docs/stacks/<lang>/` doctrine, and the three form standards, then grades cold: doc-set placement, card and page shape, signature truthfulness against the `.api/` catalogues, language and zero-provenance discipline, the absence of fragile duplication or an owner-state ledger, and integration points carried on tasks rather than a drift-prone cross-reference map. Findings repair in the same pass; a doc finalizes when a cold read surfaces nothing.

## [7]-[CROSS_CUTTING_PACKAGES]

The cross-`libs/` core registers only the packages that are genuinely project-level — shared tooling spanning languages, or the dependencies of a future admin/meta `libs/<x>` that is not bound to the AEC/Rhino pipelines. Per-language packages stay in their branch README and are never duplicated here. There is no project-level `.api/`; each such package's catalogue lives in the `.api/` of every folder that uses it. The registry names the package and its language scope, never a version or a link.
