# [PLANNING_STANDARD]

Authoring standard for the `libs/` planning corpus: it owns the doc-set per tier, the index-doc templates, and the design-page grammar. Form — containers, tables, markers, prose — follows `docs/standards/information-structure.md`, `formatting.md`, and `style-guide.md`; the monorepo topology follows `ARCHITECTURE.md`. Neither is restated here.

## [01]-[DOC_SET]

This planning corpus widens by scope: a folder owns one package's planning, a branch aggregates its language, and the cross-`libs/` core binds the three languages. Only the cross-`libs/` core names another language; a branch or folder consumes a peer only through the wire contracts.

- Cross-`libs/` core (`libs/.planning/`): `ARCHITECTURE.md`, `campaign-method.md`, `README.md`, `planning-targets.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md`.
- Branch (`libs/<lang>/.planning/`): `README.md`, `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md`.
- Folder (`<pkg>/`): the branch index-doc set at the package root and design pages under one `<pkg>/.planning/<sub-domain>/<page>.md`.
- Each folder `.api/` carries generated catalogues for folder-specific libraries and overlays; no `FEATURES.md`, no `.planning/README.md`.
- Branch `.api/` (`libs/<lang>/.api/`): one catalogue per language-wide substrate package.
- Every folder consuming a substrate package reads the branch catalogue and lists it in its README `## [03]-[SUBSTRATE_PACKAGES]` section.
- Folder `.api/` overlays for the same package exist only when a folder design page records additional seam facts.

`ARCHITECTURE.md` owns the `.planning/` lifecycle — the transient greenfield scaffold, the single-`.planning/`-per-package nesting rule, and the dissolution once a folder lands code; the cross-`libs/` core and each branch `<lang>/.planning/` are the standing exceptions that persist for the campaign duration. This standard owns only the doc-set, the templates, and the page grammar those folders carry.

## [02]-[INDEX_DOCS]

Each index doc opens on its own charter law, then its content. Keep each load-bearing: a file that only restates another doc is deleted, not maintained.

[README] — the folder's file router and package registry.
- Router: the design-page index under `.planning/`.
- Domain packages: the folder-specific libraries it uses, planned or implemented, grouped into `[CONCERN]` cards under `## [02]-[DOMAIN_PACKAGES]`.
- Cards carry no version pin and no `.api/` link; versions centralize in the owning manifest, and coupling the README to the catalogue is fragile.
- New admissions land here from the folder's ideas and tasks.
- License gate: any license granting an OSS project full free use admits, copyleft included — the estate is fully OSS with zero commercial intent.
- Payment-required or paid-tier-gated capability rejects; no deeper license analysis runs.
- Package-card row form: `- ` with the backticked package id, optionally one concise dash-led line of prose — never parentheses — within 150 columns.
- Depth beyond the one line belongs to the package's `.api/` catalogue, never the card.
- Substrate packages: a `## [03]-[SUBSTRATE_PACKAGES]` section after the domain list names the language substrate the folder consumes.
- That section points to the branch registry and the branch `libs/<lang>/.api/` catalogue.
- Centralization is absolute: no per-package manifest exists; every package and version lives in the one language manifest.

[ARCHITECTURE] — the folder's domain map and seam record.
- Section order: a standardized intro paragraph, a constant reading-contract block, then the numbered sections opening on `[DOMAIN_MAP]`.
- Codemap law: the codemap names the full sub-domain structure, including target sub-domains without design pages, each with a one-line charter.
- Each codemap node takes its name from its real domain concept, never a generic file-naming scheme.
- Each codemap node renders the eventual source file in the language's own folder and file casing; the `.planning/` scaffold is never shown.
- No prose sits between the `[DOMAIN_MAP]` heading and the tree fence.
- `[SEAMS]` records every cross-folder and cross-language alignment as Mermaid seam-registry diagrams (`flowchart LR`).
- Seam nodes: sub-domain owners interior, counterpart packages exterior, one edge per contract family.
- Edge label form: `[<KIND>]: <shared shape>`; `-->` produces/projects toward the consumer, `<-->` a shared shape.
- `[KIND]` is a closed vocabulary; a row outside it retags to the nearest canonical kind, and a new kind amends this standard.
- Kinds: `[WIRE]` `[SHAPE]` `[PROJECTION]` `[PORT]` `[BOUNDARY]` `[RECEIPT]` `[CONTENT_KEY]` `[TRANSPORT]` `[TESSELLATION]` `[GRADUATION]` `[FAULT]`.
- Codemap fence lines never exceed 150 columns; each `#` comment carries the high-signal what/why an agent needs, never code-body detail.
- Every edge appears on both endpoint folders, mirrored and `[KIND]`-identical; an in-package relation stays in the codemap, never a seam.
- Domain maps fuel ideas and tasks: a planned-but-empty sub-domain is a visible gap.
- Dependency direction is stated once in the branch `ARCHITECTURE.md` and never restated per folder; there is no owner-state registry.
- Branch-tier `ARCHITECTURE.md` carries the strata map, dependency direction, branch-grain seam registry, and folder roster with one-line charters.
- Folder maps compose the branch map, never restate it.
- Transient build-order detail stays on the task that builds a seam; the settled alignment is recorded in `[SEAMS]`.

[RULINGS] — the folder's permanent decision registry and re-litigation guard; with README and ARCHITECTURE it forms the folder's core three.
- Schema, admission law, row anatomy, and tier scope: `.claude/skills/docgen/templates/rulings.template.md`; sections are a closed vocabulary.

[IDEAS] — the folder's forward pool of higher-order concepts.
- Two sections, `[01]-[OPEN]` and `[02]-[CLOSED]`; each idea is a card — a bracketed semantic UPPERCASE_SNAKE slug leader (never numeric) and bullets.
- Card bullets carry the capability, what it unlocks, and the gap or technique it draws on.
- Folder ideas are bigger concepts — a new file, sub-domain, or capability — grounded in the folder's domain, never speculation.
- Each idea drives one or more tasks; resolved ideas move to `[CLOSED]` with a one-line disposition, so the same idea is never re-litigated.

[TASKLOG] — the folder's open and closed work, distilled from its ideas.
- Two sections, `[01]-[OPEN]` and `[02]-[CLOSED]`; each task card's leader carries a status marker.
- Open markers: `[QUEUED]`, `[ACTIVE]`, `[BLOCKED]`; closed: `[COMPLETE]`, `[DROPPED]`.
- Task cards carry three to four bullets: the capability or file to build, the packages to integrate, the boundaries or wires, and the considerations.
- Integration points align internal to the folder or to a sibling or branch, never coupled.
- Tasks are scoped guidance — not a full spec and not vague; one idea spawns one or more tasks, each naming the exact file or sub-domain it lands in.

## [03]-[DESIGN_PAGES]

Design pages live at `<pkg>/.planning/<sub-domain>/<page>.md`, one sub-domain folder per eventual source sub-tree, one page per eventual source file. Each page is a decision-complete blueprint an implementation agent transcribes, never a narrative or research log.

- H1 `# [<PKG>_<PAGE>]`; sections `## [k]-[TOKEN]` numbered from 1, section [1] the index of the page's clusters.
- Leads run two paragraphs: the telos paragraph — the capability the owner owns, its piece in the folder's system, the boundary it holds.
- Its composition paragraph carries the settled facts a rebuild composes without re-derivation, present only when the page carries them.
- Composition facts: reused axes with their owning pages, seam obligations and frozen wire names, admission and receipt rails, policy rows.
- `[RESEARCH]` is the terminal section: each row is `- [TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>`.
- Research rows record epistemic debt in place of a guessed spelling; `(none)` marks the empty section, and a resolved row is deleted whole.
- SPIKE markers name a design element whose convergence only live-host evidence finalizes; the page ships its deterministic floor beside it.
- Each cluster carries a card, then transcription-complete signature fences, then at most one Mermaid diagram.
- Card fields are a closed vocabulary — `Owner`, `Cases`, `Entry`, `Auto`, `Output`, `Receipt`, `Packages`, `Growth`, `Boundary`, in that order.
- Each card field is earned: a field that decides nothing for the cluster is omitted.
- Card bullets carry only what the fence cannot show — the decision, invariant, boundary, ownership ruling, trap, or rejection-with-reason.
- `[01]-[INDEX]` is one line per cluster, never a card restatement.
- Signature fences are transcription-complete: every generated-owner knob, closed-family key, union case, and entrypoint signature copies verbatim.
- Fence bodies land only where the body is the law.
- Fence comments carry one in-situ constraint the code cannot show, never a duplicate of a card line; cluster invariants live on the card.
- Every literal traces to an axis on the page or an earlier page, or becomes a RESEARCH item.
- Every fact has one owner: a line trap in its fence comment, a cluster decision on the card, a page boundary on the lead — never two at once.
- External members land only after the folder's `.api/` catalogue verifies the spelling; an unverified member is a RESEARCH item, never prose.
- Forge service, server-extension, and host tool-surface facts verify through `tools.assay provision` evidence.
- Every unverified availability claim is a RESEARCH item carrying its verification route.

## [04]-[NOTATION]

One integration-point notation, scope-qualified by distance: `page#CLUSTER` inside a folder, `pkg/page#CLUSTER` across folders in a language, `lang:pkg/page#CLUSTER` across languages (cross-`libs/` only). Type names recur across packages only when the concepts are genuinely distinct in distinct namespaces; a recurring wire-projection name is disambiguated at the source, never carried twice.

## [05]-[LANGUAGE]

- Agent-directed declarative present tense; the doc states law as fact.
- No reader address, narration, process, or provenance — no links, URLs, versions, dates, or session context on a design page.
- Hedge vocabulary is gate-owned and binding; open work uses a card state and a research marker, never soft posture.
- Vocabulary, owners, and policy values from earlier pages and the route's code doctrine arrive settled and are never re-taught.
- Sibling-owned facts compose inside a fence, never re-explained.

## [06]-[REVIEW]

Review is judgment against this standard and the route-owned code doctrine. Each reviewing agent reads the standard, the language's `docs/stacks/<lang>/` doctrine, and the form standards, then grades cold: doc-set placement, card and page shape, signature truthfulness against the `.api/` catalogues, language and zero-provenance discipline, no fragile duplication or owner-state ledger, and integration points on tasks, never a drift-prone cross-reference map. Findings repair in the same pass; a doc finalizes when a cold read surfaces nothing.

Signature fences transcribe an external member as settled fence code only when that member is verified against the folder's `.api/` catalogue; an external member outside the `.api/` catalogue stays a marked RESEARCH item and never appears as settled fence code, and a cold grade fails any fence whose external member contradicts a sibling RESEARCH item that declares it unverified.

## [07]-[CROSS_CUTTING_PACKAGES]

This cross-`libs/` core registers only the packages that are genuinely project-level — shared tooling spanning languages, or the dependencies of an admin/meta `libs/<x>` surface that is not bound to the AEC/Rhino pipelines. Its registry names the package and its language scope, never a version or a link. There is no project-level `.api/`.

Language-wide substrate packages — the typing, concurrency, observability, numeric, and wire-codegen tiers a branch's `.planning/` `[2]-[SUBSTRATE_PACKAGES]` registry names — carry one catalogue at the branch `libs/<lang>/.api/`, and each consuming folder lists them in its README `## [3]-[SUBSTRATE_PACKAGES]` section. Packages shared across folders only as a domain seam keep a per-folder catalogue in each consuming folder because the seam resource is folder-local. Per-language packages that are not cross-cutting stay folder-local in the folder README and `.api/`.
