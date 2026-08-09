# [PLANNING_STANDARD]

Authoring standard for the `libs/` corpus, it owns the doc-set per tier, index-docs, and spec-sheet grammar, for Forms and Topology:
- Form: Containers, tables, markers, prose — follows `docs/standards/information-structure.md`, `formatting.md`, and `style-guide.md`
- Topology: Follow `ARCHITECTURE.md`, also owns the `.planning/` lifecycle for planning.

## [01]-[DOC_SET]

This planning corpus widens by scope: a folder owns one package, a branch aggregates one independently adoptable language estate, and the cross-`libs/` core owns polyglot contracts. Peer languages appear in a branch or folder doc only as seam-registry counterpart nodes; every capability, ownership, and dependency claim spanning languages stays at the cross-`libs/` core.

- Branch (`libs/<lang>/.planning/`): `README.md`, `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md`.
- Folder (`<pkg>/`): branch index docs at root; design pages in `.planning/` use `<sub-domain>/<page>.md`, or `<page>.md` for a single-page concept.
- Cross-`libs/` core (`libs/.planning/`): the branch doc-set beside `campaign-method.md` and `planning-targets.md`.

[API_TIERS] — this section owns the two-tier catalogue law; every other surface points here:
- Catalogues document external distributions and host SDK assemblies alone; a corpus package declares its members on its own design pages.
- Cross-folder member use verifies at the owning design page under `docs/laws/topology.md` `[FENCE_SEAM]`.
- Branch `.api/` (`libs/<lang>/.api/`): one catalogue per language-wide substrate package.
- Folder `.api/`: exists in two tiers, Domain and Substrate, the former being specific to a package, and the latter being language branch wide.
- Every folder consuming a substrate package reads the branch catalogue and lists the package in its README `## [03]-[SUBSTRATE_PACKAGES]` section.
- Centralization is absolute: no per-package manifest exists, and every package and version lives in the one language manifest at monorepo root.

## [02]-[INDEX_DOCS]

Each index doc opens on its own charter law, then its content. Keep each load-bearing.

[README] — the folder's file router and package registry:
- Router: the design-page index under `.planning/`.
- Domain packages: the folder-specific libraries it uses, planned or implemented, grouped into `[CONCERN]` cards under `## [02]-[DOMAIN_PACKAGES]`.
- Cards carry no version pin and no `.api/` link; versions centralize in the owning manifest, and coupling the README to the catalogue is fragile.
- New admissions land here from the folder's ideas and tasks.
- License gate: any license granting an OSS project full free use admits, copyleft included — the estate is fully OSS with zero commercial intent.
- Payment-required or paid-tier-gated capability rejects; no deeper license analysis runs.
- Package-card row form: `- ` with the backticked package id, optionally one concise dash-led line of prose — never parentheses — within 150 columns.
- Depth beyond the one line belongs to the package's `.api/` catalogue, never the card.
- Substrate packages: the `## [03]-[SUBSTRATE_PACKAGES]` section names the branch substrate the folder consumes, under `[01]-[DOC_SET]` `[API_TIERS]`.

[ARCHITECTURE] — the folder's topology:
- Codemaps name the complete eventual source structure under the real domain concept each node owns, planned-but-empty sub-domain stays visible.
- Branch architecture owns dependency direction and the folder roster with one-line charters; folder architecture composes that direction.
- Every seam appears at both endpoint folders with identical kind and direction; a new kind amends this standard before use.
- Settled architecture contains no owner-state registry or transient build order; task cards own construction order.
- `[<KIND>]: <shape>`; `KIND = WIRE|CONTRACT|SHAPE|PROJECTION|PORT|BOUNDARY|RECEIPT|CONTENT_KEY|TRANSPORT|TESSELLATION|GRADUATION|FAULT`.
- Codemap edges carry `IMPORT` — a sub-domain composing a sibling owner — and `COUNTER` — the strata counter-edge — under the same label grammar.

[RULINGS] — the folder's permanent decision registry:
- `RULINGS.md` are leaf specific source of truth, prevents re-litigation; with README and ARCHITECTURE it forms the folder's core.
- Schema, admission law, row anatomy, and tier scope: `.claude/skills/docgen/templates/rulings.template.md`; sections are a closed vocabulary.

[IDEAS] — the folder's forward pool of higher-order concepts.
- Two sections, `[01]-[OPEN]` and `[02]-[CLOSED]`; each idea is a card — a bracketed semantic UPPERCASE_SNAKE slug leader (never numeric) and bullets.
- Card bullets carry the capability, what it unlocks, and the gap or technique it draws on.
- Folder ideas are bigger concepts — a new file, sub-domain, or capability — grounded in the folder's domain, never speculation.
- Each idea drives one or more tasks; resolved ideas move to `[CLOSED]` with a one-line disposition, so the same idea is never re-litigated.

[TASKLOG] — the folder's open and closed work, distilled from its ideas:
- Two sections, `[01]-[OPEN]` and `[02]-[CLOSED]`; each task card's leader carries a status marker.
- Open markers: `[QUEUED]`, `[ACTIVE]`, `[BLOCKED]`; closed: `[COMPLETE]`, `[DROPPED]`.
- Task cards carry three to four bullets: the capability or file to build, the packages to integrate, the boundaries or wires, and the considerations.
- Integration points align internal to the folder or to a sibling or branch, never coupled.
- Tasks are scoped guidance — not a full spec and not vague; one idea spawns one or more tasks, each naming the exact file or sub-domain it lands in.

## [03]-[DESIGN_PAGES]

Design pages live at `<pkg>/.planning/<sub-domain>/<page>.md`, one sub-domain folder per eventual source sub-tree, one page per eventual source file. Each page is a decision-complete blueprint an implementation agent transcribes, never a narrative or research log.

- H1 `# [<PKG>_<PAGE>]`; sections `## [k]-[TOKEN]` numbered from 1, section [1] the index of the page's clusters.
- Leads run two paragraphs: the charter paragraph — the capability the owner owns, its piece in the folder's system, the boundary it holds.
- Its composition paragraph carries the settled facts a rebuild composes without re-derivation, present only when the page carries them.
- Composition facts: reused axes with their owning pages, seam obligations and frozen wire names, admission and receipt rails, policy rows.
- `[RESEARCH]` is the terminal section: each row is `- [TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>`.
- Research rows record epistemic debt in place of a guessed spelling; `(none)` marks the empty section, and a resolved row is deleted whole.
- Settled facts never ride a research section — each folds into its owning line at write, and a bullet restating settled law is removed on sight.
- SPIKE markers name a design element whose convergence only live-host evidence finalizes; the page ships its deterministic floor beside it.
- Each cluster carries a card, then transcription-complete signature fences, then at most one Mermaid diagram.
- Card fields are a closed ordered vocabulary: `Owner` `Cases` `Law` `Exemption` `Entry` `Auto` `Output` `Receipt` `Packages` `Growth` `Boundary`.
- Each card field is earned: a field that decides nothing for the cluster is omitted.
- Card bullets carry only what the fence cannot show — the decision, invariant, boundary, ownership ruling, trap, or rejection-with-reason.
- `[01]-[INDEX]` indexes the page's clusters in section order, one entry per cluster and never an owner roster or a card restatement.
- Each index entry leads `[NN]-[CLUSTER]` restating its cluster's header number — a leader list, never a table seating the ordinal in `[INDEX]`.
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

- Path segments name the page and every folder above it in its package — no `.md` suffix, `.planning/`/`.api/` segment, or hyphen standing in for `/`.
- `#CLUSTER` names a `## [NN]-[CLUSTER]` header on that page — never a code symbol, fence-local binding, or table row index the table renumbers away.
- References into a live table name the row's stable token, so growth in the table never silently re-points them.

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

This cross-`libs/` core registers only the packages that are genuinely project-level — shared tooling spanning languages, or the dependencies of an admin/meta `libs/<x>` surface bound to no branch domain. Its registry names the package and its language scope, never a version or a link. There is no project-level `.api/`.

Catalogue homing follows `[01]-[DOC_SET]` `[API_TIERS]`: a package shared across folders only as a domain seam keeps a per-folder catalogue in each consuming folder, because the seam resource is folder-local, and a per-language package that is not cross-cutting stays folder-local in the folder README and `.api/`.
