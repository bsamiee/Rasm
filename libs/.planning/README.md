# [PLANNING_STANDARD]

The authoring standard for the `libs/` planning corpus. It owns the doc-set per tier, the four index-doc templates, and the design-page grammar. Form — containers, tables, markers, prose — follows `docs/standards/information-structure.md`, `formatting.md`, and `style-guide.md`; the monorepo topology follows `architecture.md`. Neither is restated here.

## [01]-[DOC_SET]

The planning corpus widens by scope: a folder owns one package's planning, a branch aggregates its language, and the cross-`libs/` core binds the three languages. Only the cross-`libs/` core names another language; a branch or folder consumes a peer only through the wire contracts.

- Cross-`libs/` core (`libs/.planning/`): `architecture.md` (the topology law), `campaign-method.md` (the planning loop), `README.md` (this standard), `planning-targets.md` (the target index — every planning surface the campaign edits), `IDEAS.md`, `TASKLOG.md`.
- Branch (`libs/<lang>/.planning/`): `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`.
- Folder (`<pkg>/`): the four index docs at the package root — `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md` — plus design pages under one `<pkg>/.planning/<sub-domain>/<page>.md` and a `<pkg>/.api/` of generated package catalogues for folder-specific external libraries and folder-specific overlays. No `FEATURES.md`, no `.planning/README.md`.
- Branch `.api/` (`libs/<lang>/.api/`): one catalogue per language-wide substrate package. A folder consuming a substrate package reads the branch catalogue and lists the package in its README `## [3]-[SUBSTRATE_PACKAGES]` section. Folder `.api/` overlays for the same package exist only when a folder design page records additional seam facts.

The `.planning/` lifecycle — the transient greenfield scaffold, the single-`.planning/`-per-package nesting rule, and the dissolution once a folder lands code — is owned by `architecture.md`; the cross-`libs/` core and each branch `<lang>/.planning/` are the standing exceptions that persist for the campaign duration. This standard owns only the doc-set, the templates, and the page grammar those folders carry.

## [02]-[INDEX_DOCS]

Each index doc opens with one or two declarative lines stating its own organization, then its content. Keep each load-bearing: a file that only restates another doc is deleted, not maintained.

[README] — the folder's file router and package registry.
- Router: a short index of the design pages under `.planning/`, so a reader navigates the folder without scanning the tree.
- Domain packages: the folder-specific external libraries it uses, planned or implemented, grouped into `[CONCERN]` cards under `## [2]-[DOMAIN_PACKAGES]`. No version pin (versions are centralized in the one owning manifest) and no per-package link into `.api/` (the catalogue is expected at the folder's own `.api/<package>.md`; coupling the README to it is fragile). New admissions land here from the folder's ideas and tasks.
- Package-card row form: a package row is `- ` plus the backticked package id, optionally followed by one concise dash-led line of prose — never parentheses — and the whole row stays within 160 columns. Depth beyond the one line belongs to the package's `.api/` catalogue, never the card.
- Substrate packages (all languages): a `## [3]-[SUBSTRATE_PACKAGES]` section after the domain list names the language substrate the folder consumes — the typing/rails, concurrency, observability, identity, time, numeric, wire-codegen, and test tiers — pointing to the branch registry and branch `libs/<lang>/.api/` catalogue.
- Centralization is absolute: no per-package manifest exists (no per-folder `pyproject.toml`, `package.json`, or `*.props`); every package and version lives in the one language manifest.

[ARCHITECTURE] — the folder's domain map and seam record.
- Section order: a standardized intro paragraph, a constant reading-contract block, `[1]-[DOMAIN_MAP]` (the codemap tree), `[2]-[SEAMS]`, then any folder-specific sections.
- Codemap law: the codemap names the full sub-domain structure, including target sub-domains without design pages, each by its real domain concept — never a `rail`/`axis`/`lane` file-naming scheme — with a one-line charter.
- Each codemap node renders the eventual source file in the language's own folder and file casing; the `.planning/` scaffold is never shown, and no prose sits between the `[1]` heading and the tree fence.
- `[2]-[SEAMS]` is the curated, file-level record of every cross-folder and cross-language alignment, in one ` ```text seams ` fence with aligned columns.
- Row form: `<SourceFile> <glyph> <lang:pkg/subdomain> # [<KIND>]: <shared shape>`. Glyphs: `→` produces/projects, `←` consumes/reads, `⇄` shared shape.
- The `[KIND]` vocabulary is closed: `[WIRE]` `[SHAPE]` `[PROJECTION]` `[PORT]` `[BOUNDARY]` `[RECEIPT]` `[CONTENT_KEY]` `[TRANSPORT]` `[TESSELLATION]` `[GRADUATION]` `[FAULT]`. A row outside the list retags to the nearest canonical kind; a new kind is an amendment to this standard, never a fence-local mint.
- A codemap or seams fence line never exceeds 160 columns; the `#` comment carries the high-signal what/why an agent needs, never code-body detail.
- Every edge appears on both endpoint folders with mirrored glyph and the identical `[KIND]`; an in-package relation stays in the codemap and is never a seam.
- The map fuels ideas and tasks: a planned-but-empty sub-domain is a visible gap. Dependency direction is stated once in the branch `ARCHITECTURE.md` and never restated per folder; there is no owner-state registry. Transient build-order detail stays on the task that builds a seam; the settled alignment is recorded in `[2]-[SEAMS]`.

[IDEAS] — the folder's forward pool of higher-order concepts.
- Two sections, `[1]-[OPEN]` and `[2]-[CLOSED]`. Each idea is a card: a bracketed slug leader plus a few bullets — the capability, what it unlocks, and the gap or modern technique it draws on. A folder idea is a bigger concept (a new file, sub-domain, or capability) grounded in the folder's domain and the monorepo purpose, never speculation.
- An idea drives one or more tasks. A resolved idea moves to `[CLOSED]` with a one-line disposition, so the same idea is never re-litigated.

[TASKLOG] — the folder's open and closed work, distilled from its ideas.
- Two sections, `[1]-[OPEN]` and `[2]-[CLOSED]`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries/wires (internal to the folder or aligned to a sibling or branch, never coupled), and the key considerations.
- A task is scoped guidance — not a full spec and not vague. One idea spawns one or more tasks across one or more files; each task names the exact file or sub-domain it lands in.

## [03]-[DESIGN_PAGES]

Design pages live at `<pkg>/.planning/<sub-domain>/<page>.md`, one sub-domain folder per eventual source sub-tree, one page per eventual source file. A page is a decision-complete blueprint an implementation agent transcribes, not a narrative or research log.

- H1 `# [<PKG>_<PAGE>]`; one declarative lead paragraph; sections `## [k]-[TOKEN]` numbered from 1, section [1] the index of the page's clusters.
- A cluster carries a card (owner, packages, growth, and the boundary/receipt/entry lines it earns), then transcription-complete signature fences, then at most one Mermaid diagram.
- Signature fences are transcription-complete: every generated-owner knob, closed-family key, union case with its payload, and entrypoint signature is written so an agent copies it verbatim; bodies are written where the body is the law. Fences carry zero comments; invariants live on the card. Every literal traces to an axis on the page or an earlier page, or becomes a RESEARCH item.
- An external member is written only after the folder's `.api/` catalogue verifies its spelling; an unverified member is a RESEARCH item, never prose.

## [04]-[NOTATION]

One integration-point notation, scope-qualified by distance: `page#CLUSTER` inside a folder, `pkg/page#CLUSTER` across folders in a language, `lang:pkg/page#CLUSTER` across languages (cross-`libs/` only). A type name recurs across packages only when the concepts are genuinely distinct in distinct namespaces; a recurring wire-projection name is disambiguated at the source, never carried twice.

## [05]-[LANGUAGE]

- READ: `docs/standards/style-guide.md`, `docs/standards/information-structure.md`, `docs/standards/formatting.md`
- Agent-directed declarative present tense; the doc states law as fact. No reader address, narration, process, or provenance — no links, URLs, versions, dates, or session context on a design page.
- The gate-owned hedge roster is binding; open work uses a card state and a research marker, never soft posture.
- Vocabulary, owners, and policy values from earlier pages and the route's code doctrine arrive settled and are never re-taught. A fact owned by a sibling is composed inside a fence, never re-explained.

## [06]-[REVIEW]

Review is judgment against this standard and the route-owned code doctrine, not checklist pedantry. A reviewing agent reads the standard, the language's `docs/stacks/<lang>/` doctrine, and the three form standards, then grades cold: doc-set placement, card and page shape, signature truthfulness against the `.api/` catalogues, language and zero-provenance discipline, the absence of fragile duplication or an owner-state ledger, and integration points carried on tasks rather than a drift-prone cross-reference map. Findings repair in the same pass; a doc finalizes when a cold read surfaces nothing.

A signature fence transcribes an external member as settled fence code only when that member is verified against the folder's `.api/` catalogue; an external member outside the `.api/` catalogue stays a marked RESEARCH item and never appears as settled fence code, and a cold grade fails any fence whose external member contradicts a sibling RESEARCH item that declares it unverified.

## [07]-[CROSS_CUTTING_PACKAGES]

The cross-`libs/` core registers only the packages that are genuinely project-level — shared tooling spanning languages, or the dependencies of an admin/meta `libs/<x>` surface that is not bound to the AEC/Rhino pipelines. The registry names the package and its language scope, never a version or a link. There is no project-level `.api/`.

A package that is language-wide substrate — the typing, concurrency, observability, numeric, and wire-codegen tiers a branch's `.planning/` `[2]-[SUBSTRATE_PACKAGES]` registry names — carries one catalogue at the branch `libs/<lang>/.api/`, and each consuming folder lists it in its README `## [3]-[SUBSTRATE_PACKAGES]` section. A package shared across folders only as a domain seam keeps a per-folder catalogue in each consuming folder because the seam resource is folder-local. Per-language packages that are not cross-cutting stay folder-local in the folder README and `.api/`.
