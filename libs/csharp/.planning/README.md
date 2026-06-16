# [SUITE_PLANNING_STANDARD]

This file is the binding authoring standard for the four app-package planning corpora under `libs/csharp/Rasm.{AppHost,Persistence,Compute,AppUi}/.planning/`. Every authoring agent loads this standard and the suite ledger `region-map/` before writing a page; every reviewing agent grades cold against the review law in section [5]. Binding precedence when sources conflict: locked campaign decisions, then adversarial-verifier and attack corrections, then synthesis closures, then design JSON. AppHost authors first — every sibling consumes its ports as settled vocabulary; the remaining packages author in parallel against the ledger's pinned seam contracts. Within a package, position equals the charter PAGE_INDEX order, the charter is authored before its pages, and symbol resolution is package-cumulative: a package's fences resolve against its own full final symbol set, while cross-package consumption resolves only against finalized owners.

## [1]-[CORPUS_SHAPE]

- Planning home: `libs/csharp/Rasm.<Pkg>/.planning/`, sibling to `.api/`. The suite home `libs/csharp/.planning/` holds this standard, the ledger `region-map/`, the campaign method (`campaign-method.md`), the open-work log (`TASKLOG.md`), and the concert feature atlas (`FEATURES.md`); the standard and the ledger are the only durable law, and campaign working material lives under `.artifacts/planning-briefs/`.
- Per package: one charter (`README.md`, the only linking file) plus self-contained domain pages. Pages are decision-complete blueprints an implementation agent transcribes; they are not narratives, reports, or research logs.
- Package-local planning docs are repo-concrete: they name Rasm projects, admitted packages, and decided policy values. Stack doctrine (`docs/stacks/csharp/`) is never restated; its vocabulary arrives settled and is composed as given.
- Page size is set by the concern, never by a line count: a page is exactly as large as its owned concern requires; a signature is never truncated and a capability is never dropped. A page splits into an axis-led sibling only when it owns two separable concerns, never to hit a number; logical consolidation into one page is preferred, and the planning page set models the implementation file set — one page maps to one transcription unit.

## [2]-[PAGE_GRAMMAR]

- File: `libs/csharp/Rasm.<Pkg>/.planning/<page>.md`. H1: `# [<PKG>_<PAGE>]` where PKG is APPHOST, PERSISTENCE, COMPUTE, or APPUI and PAGE is the file stem upper-snaked.
- Lead: exactly one declarative paragraph, at most 6 lines, between the H1 and section [1], stating the owned concern, the axes the page owns, and the package spine. No list, table, fence, or heading intervenes.
- Section headings: `## [k]-[TOKEN]`, k strictly sequential from 1, TOKEN uppercase-snake. Section [1] is INDEX. Section order: INDEX, then one section per cluster in index order, then optional TS_PROJECTION, then RESEARCH last when research items exist.
- Index table: 3 columns `[INDEX] | [CLUSTER] | [OWNS]`; one row per cluster; the CLUSTER cell equals the cluster's heading token verbatim; OWNS is a decision statement of at most 12 words; row order equals section order.
- Clusters per page: 2 to 6. Near-peer clusters merge until each owns a real decision cluster.
- Cluster internal order: card, then signature fence(s), then at most one Mermaid diagram. The card precedes the first fence; a snippet sits beside the rule it proves.
- Card: exactly one per cluster, field lines `- Owner:` `- Cases:` `- Entry:` `- Auto:` `- Receipt:` `- Packages:` `- Growth:` `- Boundary:` in that fixed order; Owner, Packages, and Growth are mandatory; the rest appear only when earned.
- Fence info strings: ```` ```csharp signature ````, ```` ```ts contract ````, ```` ```mermaid ````. Mermaid kinds: stateDiagram-v2 (lifecycles), flowchart (pipelines), sequenceDiagram (cross-boundary handshakes) only.
- Tables: at most 8 columns and 20 rows; beyond 20 rows decompose into axis-led sibling tables; rows atomic; no links inside cells; tables enumerate, cards legislate.
- RESEARCH section: grouped leader lines `- [<GROUP>]:`, each followed by its related unresolved items as compact clauses. An item is a question-free fact statement naming the unverified surface; near-duplicate items merge into one clause. No proof routes and no gate column: the charter PROOF_GATES carries the executable rails once, and the section already sits on the page that owns the decision. An item answerable trivially or owned elsewhere is deleted.
- Size: a page is as large as its owned concern requires; no line target and no cap — never drop a fence or a case to satisfy a number. A page splits into an axis-led sibling only when it owns two separable concerns, after consolidation is exhausted, because the page set models the eventual implementation file set.
- Anchors: cluster tokens are unique within a package's planning set (TS_PROJECTION and RESEARCH are exempt structural tokens); every owner type name is unique across the four-package suite, enforced through the ledger. Bounded-context carve-out: a name may recur across packages ONLY when the recurrences are genuinely distinct concepts in distinct namespaces AND the ledger records each occurrence with its disambiguating context note; the carve-out is closed and enumerated in the ledger (WireFault: external-transport edge vs gRPC fault projection; CommandReceipt: capability-algebra transaction vs UI command execution; FrameBudget: solver early-stop governor vs viewport frame budget). A wire-projection type (`*Wire`) is NEVER admitted to the carve-out — two `*Wire` types with the same name project into one polyglot SDK off one descriptor source and collide at codegen; a recurring wire name is force-disambiguated at the source (the AppHost capability-registry CommandReceiptWire is `CapabilityCommandReceiptWire`, distinct from the AppUi command-execution CommandReceiptWire).
- Disjointness: a sibling page's concern is neither re-shown nor pointed to; a fact owned elsewhere is composed as supporting material inside a fence, never re-taught in prose.

## [3]-[SIGNATURE_LAW]

- Completeness bar: a signature block is transcription-complete — an implementation agent copies it verbatim and adds nothing but file-organization scaffolding.
- Attribute configuration: every generated owner writes every load-bearing knob explicitly — conversion-operator generation, `SwitchMapMethodsGeneration`, key-member comparer accessors, operator-generation axes on value objects, validation factory hooks.
- SmartEnum keys: every `[SmartEnum<TKey>]` writes the key type; string-keyed vocabularies declare exactly one comparer accessor.
- Union cases: every case is a full `public sealed record Case(Type Field, ...) : Owner;` with payload field names and types. Fault unions follow the doctrine `Expected` shape with the dual-tier `Create` contract.
- Entrypoints: full signatures — generic parameters with semantic payload, constraints at minimal sufficiency including trait stacks and `allows ref struct`, `params ReadOnlySpan<T>` arities, `Option<T> = default` optionals, carrier return types.
- Bodies are written when the body is the law: total generated `Switch` dispatch with state-threaded static lambdas, fold algebras, applicative joins, bracket/retry aspect stacks.
- Policy, receipt, and port records spell every field with its exact type — NodaTime `Instant`/`Duration` for semantic time, `TimeProvider` for elapsed, correlation-id types, slot/kind metadata.
- Operation families attach as extension blocks dispatching totally over the owner; one rail per entrypoint, named in the return type and on the Entry line: `Validation<Error,T>` accumulates, `Fin<T>` aborts, `Eff<RT,T>`/`IO<T>` carries effects.
- Growth axes are marked structurally, never by comment: the closed family declaration with its complete current case list is the axis; the card Growth line states the unit cost.
- Comment law: signature and contract fences contain zero comment lines; invariants and boundary facts live on the card's Boundary and Growth lines.
- Literals: every policy literal (duration, capacity, rank, deadline) traces to an axis row on the page or an earlier page. An invented literal is the named highest-risk defect; an untraceable literal becomes a RESEARCH item, never prose.
- TS projection fences are type-only `ts contract` blocks — interfaces, type aliases, literal-discriminated unions — transcribing the wire shape the codec actually emits.

## [4]-[LANGUAGE]

- Voice: agent-directed declarative present tense; the page states law as fact. No reader address, no narration, no process context.
- Banned tokens (word-boundary, page-wide): should, could, would, might, maybe, may be, perhaps, likely, probably, propose, proposed, consider, recommended, ideally, TBD, TODO, FIXME, etc., we, our, you. Hedge-synonym forms are equally banned: is expected to, is intended to, can be, aims to, is designed to, in the future, eventually, as needed, if necessary.
- Future tense is legal only on card Growth lines and RESEARCH items. The bare word "future" outside those positions is a hedge hit.
- Zero provenance: no links, no URLs, no version narration, no dates, no verification or session context on domain pages. Package versions live only in `Directory.Packages.props`; the charter ADMISSIONS_RECORD owns the package→page→catalogue→status map, never the pin.
- An API member, knob, native behavior, or host interaction not traceable to a catalogue page or completed decompile probe is never written into prose or fences; it becomes a RESEARCH item.
- Gap closures are never recorded on domain pages; a gap is closed by page content. The closure bookkeeping lives in the charter GAP_LEDGER.
- Where a card or axis names a use, it names the spelling, wrapper, or local pattern it deletes.
- Prose budget: vocabulary, owners, rails, and policy values from earlier pages and the doctrine arrive settled and are never re-taught.

## [5]-[REVIEW_LAW]

Review is judgment against the established law, not checklist pedantry. A reviewing agent reads this standard's sections [1]-[4], the page-craft and code doctrine in `docs/stacks/csharp/`, and `docs/standards/style-guide.md` + `formatting.md` + `information-structure.md`, then grades the page cold against them: grammar and card shape, signature truthfulness against the catalogues and the ledger, language and zero-meta discipline, anchor and link integrity, doctrine-true fences, traceable literals. Findings are repaired in the same pass; a page is finalized when a cold read surfaces nothing.

## [6]-[LEDGER_PROTOCOL]

The suite ledger is `libs/csharp/.planning/region-map/`. Authors append provisional rows BEFORE writing a page; collisions resolve before authoring; the reviewing agent flips rows to FINAL.

- [1]-[PAGE_REGIONS]: per-package blocks; row shape `- <page>.md [PROVISIONAL|FINAL]: <one-sentence concern law>`. Cluster tokens reside once, on the page's SIGNATURE_REGIONS keys.
- [2]-[SIGNATURE_REGIONS]: one row per signature-fence region, keyed `<AH|PS|CP|AU>-<NN> <CLUSTER_TOKEN>:` followed by the owner symbols the fence declares and its spotlight.
- [3]-[OWNER_SYMBOLS]: the suite symbol registry — `- <TypeName> — <pkg>/<page>#<CLUSTER> [<kind>]`, append-ordered by authoring position. This registry backs cross-package symbol resolution and uniqueness review.
- [4]-[SEAM_SPLITS]: every two-package fact records its altitude split — `- <fact>: mechanics at <owner pkg/page#CLUSTER>; consequence at <consumer pkg/page#CLUSTER>.`
- Duplicate-region repair: route to the owner — the later/consumer page deletes its re-teaching and consumes the owner's symbol as settled vocabulary.

## [7]-[CHARTER_TEMPLATE]

Per-package `libs/csharp/Rasm.<Pkg>/.planning/README.md`, the only linking file in the package planning set. Authored before the package's pages, carrying the catalogue-pending list and the wire-page list at authoring time.

- H1 `# [<PKG>_PLANNING]` + scope/intent in at most 4 declarative sentences: zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed.
- [1]-[PAGE_INDEX]: `[INDEX] | [PAGE] | [OWNS] | [STATE]`, STATE in {planned, authored, finalized}; finalized is the one-way all-PASS gate.
- [2]-[WIRE_PAGES]: the package's wire-relevant page list; each named page carries exactly one TS_PROJECTION cluster.
- [3]-[CATALOGUE_PENDING]: app-root-only pins and transitive surfaces catalogued at app-root creation, each with its landing condition.
- [4]-[GAP_LEDGER]: `[INDEX] | [GAP] | [CLOSED_BY (page#cluster)]` — every adversarial-verifier gap finding for the package; a row is present only when closed by its page#cluster, so every row is CLOSED.
- [5]-[DENSITY_BAR]: implementation collapses to one owner per axis and one entrypoint family per rail — density is the absence of parallel rails, near-duplicate shapes, and re-derived logic, not a line count; the owner-count budget table `[INDEX] | [AXIS/CONCERN] | [OWNER] | [KIND] | [CASES]` with an optional trailing `[STATE]` feature-state column — a new feature is a row or case, never a new surface. The `[STATE]` column, when present, carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page RESEARCH cluster; a SPIKE owner is fully shaped now, never a deferred surface. The `[OWNER]` cell folds every extension block and mapping descriptor under its axis owner so the complete-owner claim holds. Only DENSITY_BAR carries `[STATE]`; the charter GAP_LEDGER carries none.
- [6]-[BUILD_ORDER]: dependency-ordered file plan `[INDEX] | [FILE] | [TRANSCRIBES] | [GATE]` — vocabulary owners first, then shapes, rails, dispatch surfaces, boundaries, composition; binding seam notes precede the table.
- [7]-[FILE_PROCESS]: the numbered per-file loop — read the charter and every page in the file's TRANSCRIBES cell end-to-end; transcribe signature fences verbatim; run the collapse scan per edit (parallel types, sibling factories, repeated arms, single-call helpers — each at 3 or more triggers in-place collapse); `uv run python -m tools.assay static fix` then `static build` on the touched closure; author specs per the `testing-cs` skill; bridge scenarios gate host seams.
- [8]-[PROOF_GATES]: `[GATE] | [COMMAND] | [EVIDENCE]` — restore proof, `assay api doctor`/`resolve` catalogue proof, `static plan`/`build` routing proof, `test run --target` spec proof, bridge scenarios for host seams, local Mermaid render proof.
- [9]-[PROHIBITIONS]: the closed NEVER list — no new public surfaces beside the budgeted owners; no wrappers, rename adapters, helpers, or utility files; no generic receipt abstractions; no sentinel propagation; no DateTime.UtcNow; no second cache/retry/correlation owner; CSP analyzer diagnostics are architecture pressure, not suppression targets.
- [10]-[ADMISSIONS_RECORD]: `[INDEX] | [PACKAGE] | [PAGE] | [CATALOGUE] | [STATUS]` — the executed admissions ledger; `[STATUS]` in {catalogue-pending, app-root-pending, tests-only, admitted}; versions live only in `Directory.Packages.props` and never appear here.
- [11]-[REFINEMENT_HORIZON]: the next deepening session's entry route and the folder-specific deepening targets beyond the closed corpus, closed by a bar statement.

## [8]-[TOOL_AND_RELOCK_LAW]

- `.config/dotnet-tools.json` is the sole CLI tool pin surface; agents never install global tools or invoke unpinned tool versions.
- Central version bump law: edit `Directory.Packages.props`, run `dotnet restore --force-evaluate`, and commit every regenerated `packages.lock.json` in the same change. A transitive-pin conflict gets a named Transitive Floors row, never a NoWarn.
- Mermaid render validation runs through the local `@mermaid-js/mermaid-cli` route (the MCP renderer is permission-blocked for planning content); a diagram that fails render is a finding.
- Decompile probes run through `uv run python -m tools.assay api query` with fuzzy package keys; probe results answer research items at authoring time when the surface is local.
