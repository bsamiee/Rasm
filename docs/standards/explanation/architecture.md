# [ARCHITECTURE_STANDARDS]

An architecture document explains the current structure of a scope: its boundaries, building blocks, invariants, runtime and deployment shape, and the proof that keeps the explanation matched to repository truth. It is an explanation artifact. It states what the system is now and which shapes are forbidden; it does not record why a durable decision was made, sequence unbuilt work, drive an incident response, or catalog an API surface.

## [1][USE_WHEN]

Use an architecture document when a reader must understand current structure rather than act on a task. Reach for it when the reader needs:

- system, package, owner, host, or runtime boundaries and what each one owns;
- building blocks, their relationships, and the codemap that grounds them;
- invariants, constraints, quality trade-offs, and the shapes the system rejects;
- current topology, runtime flow, or deployment layout that explains an invariant.

Route decision rationale to the decision-record topic, build sequence and exit criteria to the roadmap topic, operational recovery to the runbook topic, and generated endpoint or symbol surfaces to the API topic. When a draft mixes current structure with any of those, split it and keep this file to structure and invariants. The chooser that decides which type fits lives in the standards index; this section routes by topic, not by link.

## [2][EXTERNAL_BASIS]

Use external architecture standards for semantics, and keep this file's profiles as named subsets that preserve their meaning rather than redefining it. The profiles below are an arc42-lite subset rendered with C4 abstraction levels; an author who knows arc42 and C4 already knows this standard's shape.

- arc42 owns the architecture-explanation category set and section names. The canonical 12 sections are Introduction and goals, Constraints, Context and scope, Solution strategy, Building-block view, Runtime view, Deployment view, Crosscutting concepts, Architecture decisions, Quality requirements, Risks and technical debt, and Glossary. This file's `Landscape` order is the reading-optimized subset of that set; the omitted sections are optional context, not redefined semantics.
- C4 owns the abstraction levels (Context, Container, Component, Code), the diagram-level semantics, the supporting diagram set (System landscape, Dynamic, Deployment), and relationship direction.
- Structurizr DSL owns authored-model consistency when current repository tooling or an existing architecture corpus already uses it.
- Mermaid owns Markdown-friendly rendering syntax only, never the model.

A local subset must preserve the external standard's meaning for every section it keeps. When this file's compact profile names fewer sections than arc42, the omitted sections are optional context, not redefined semantics.

`Source of truth:` arc42 template (`arc42.org`, 12-section structure) and C4 model (`c4model.com`, four abstraction levels plus the landscape, dynamic, and deployment supporting diagrams). `Last verified:` 2026-06-04. `Review trigger:` arc42 or C4 publishes a structural change to its section set or abstraction levels.

## [3][SYSTEM_SCALE_PROFILES]

Select one profile by the scale the document governs, then meet that profile's required views and section cardinality. A profile is the unit of selection; do not blend two profiles in one file, and do not keep both a landscape file and an owner-contract file in the same directory.

Each row fixes the scope, the canonical file name, the minimum C4 views, and the floor on required body sections. "Required views" is the diagram floor; a profile may add a view when an invariant depends on it.

| [INDEX] | [PROFILE]      | [SCOPE]                         | [FILE_NAME]            | [REQUIRED_C4_VIEWS]    | [REQUIRED_SECTIONS] |
| :-----: | :------------- | :------------------------------ | :--------------------- | :--------------------- | ------------------: |
|   [1]   | Landscape      | System or tool boundary         | `ARCHITECTURE.md`      | Context + Container    |                   7 |
|   [2]   | Owner-contract | Package or sub-concern boundary | `_ARCHITECTURE.md`     | Container or Component |                   5 |
|   [3]   | Embedded       | Single directory, one entry     | section in `README.md` | None (codemap only)    |                   3 |

Profile selection rules:

- Use `Landscape` when the scope crosses two or more owners, hosts, or runtimes.
- Use `Owner-contract` when the scope is one package or one sub-concern with a public contract its consumers depend on.
- Use `Embedded` only when the directory holds one entry point, exposes no cross-owner contract, and the structure is readable from a single codemap without a Context view. Promote `Embedded` to `Landscape` or `Owner-contract` the moment a second owner or a cross-package contract appears.

## [4][REQUIRED_STRUCTURE]

Use the template for the selected profile as the literal heading set. Each heading carries a cardinality: `required` headings must be present and non-empty; `conditional` headings become required when their trigger holds and are omitted otherwise rather than filled with placeholder prose; `repeatable` records may appear more than once inside their section. The body sections below detail the content each heading must carry.

Landscape template (copy verbatim, then fill):

```markdown template
# [SCOPE_ARCHITECTURE]

## [1][SCOPE_GOALS_CONSTRAINTS]

## [2][CONTEXT_SCOPE_REQUIRED]

## [3][SOLUTION_STRATEGY_REQUIRED]

## [4][BUILDING_BLOCK_VIEW]

## [5][INVARIANTS_REJECTED_SHAPES]

## [6][RISKS_GLOSSARY_REQUIRED]

## [7][RUNTIME_SCENARIOS_CONDITIONAL]

## [8][DEPLOYMENT_VIEW_CONDITIONAL]

## [9][PROOF_REQUIRED]

```

Owner-contract template (copy verbatim, then fill):

```markdown template
# [PACKAGE_ARCHITECTURE]

## [1][PURPOSE_BOUNDARY_REQUIRED]

## [2][BUILD_SUPPORT_STATUS]

## [3][MATURITY_STATUS_CONDITIONAL]

## [4][PUBLIC_CONTRACTS_OWNED]

## [5][INVARIANTS_REJECTED_SHAPES]

## [6][CAPABILITY_CATALOG_CONDITIONAL]

## [7][PROOF_EVIDENCE_REQUIRED]

```

Embedded template (section inside `README.md`):

```markdown template
## [1][ARCHITECTURE_REQUIRED_HEADING]

### [1.1][PURPOSE_BOUNDARY_REQUIRED]

### [1.2][CODEMAP_REQUIRED_TEXT]

### [1.3][INVARIANTS_REQUIRED_OBSERVABLE]

```

The required floor for `Landscape` is the six required sections plus `Proof`; promote a conditional section to required the moment its trigger holds. The required floor for `Owner-contract` is its five required sections. The required floor for `Embedded` is the three sections above.

## [5][LANDSCAPE_SECTIONS]

Use the arc42-lite section order below. Each entry states its cardinality and the concrete content it must carry.

1. Scope, goals, and constraints — required. State the system boundary in one block, the driving quality goals as a named list (each goal is a falsifiable quality attribute, not a slogan), and the hard constraints in one block. A constraint that no reader can violate is not a constraint; name the bound and the consequence of crossing it.
2. Context and scope — required. Name external actors and systems and the data that crosses the boundary; this is the C4 Context subject. State each actor, its direction relative to the system, and the data or contract that crosses. Use a record table when three or more actors share these fields.
3. Solution strategy — required. State the top architectural choices and the quality goal each one serves, one sentence per choice. Bind each choice to a goal named in section 1; a strategy that serves no stated goal is decoration.
4. Building-block view and codemap — required. Decompose into containers and bind each to real repository paths; building-block entries are repeatable. Each block carries its owned concern, its repository path, and its relationship to sibling blocks. The C4 Container view renders the same set the codemap lists; the two must not disagree.
5. Invariants and rejected shapes — required. State each invariant as an observable, falsifiable rule and each rejected shape with the reason it is forbidden and the check that catches it; both are repeatable. An invariant names the rule, the boundary it governs, and the analyzer, test, or review that proves it. A rejected shape names the forbidden construction and the failure mode it prevents.
6. Risks and glossary — required. Record open architectural risks as status-tagged records. Define terms a first-time reader cannot infer as a co-located glossary. Each risk carries its `Status`, the `Exit` condition that retires it, the `Owner` accountable, and the `Proof` or mitigation in flight.
7. Runtime scenarios — conditional. Required when a current invariant depends on the order of a cross-container flow; one C4 Dynamic view per scenario. State the trigger, the ordered steps across containers, and the invariant the order proves.
8. Deployment view — conditional. Required when deployed topology, host placement, or a runtime boundary explains a current invariant. State each deployment node, what runs on it, and the boundary the placement enforces.

A `Proof` section closes the `Landscape` profile and is required; see `Proof` below for its content.

## [6][OWNER_CONTRACT_SECTIONS]

Use the compact profile for a package or sub-concern boundary. Same cardinality convention applies, and each section states its concrete content.

1. Purpose and boundary — required. State what the package owns and the line its consumers must not cross. Name the one concern the package owns and the adjacent concerns it explicitly does not.
2. Build and support status — required. State the current build state and support posture, with claim-level evidence. Carry the build status as a marker (`[PASS]`, `[FAIL]`, `[PARTIAL]`) and the support posture as a labeled fact, each beside its proof command or status check.
3. Public contracts and owned blocks — required. List the exported contracts and the internal blocks that back them; contract entries are repeatable. Each contract record carries its name, its signature or schema reference, the consumers that depend on it, and the internal block that implements it.
4. Invariants and rejected shapes — required. Same rule and content as the landscape profile; both are repeatable.
5. Proof and evidence — required. Name how the codemap and any diagram were refreshed against current paths; see `Proof` below.

Decision-record and roadmap pointers are optional in this profile: include one only when a consumer needs the rationale or the sequence to use the contract safely. An owner-contract file is a compact arc42 subset; it still carries scope, contracts, invariants, and proof at full strength.

Two conditional sections extend the profile when a package carries more than its contract. Add each only when its trigger holds, give it claim-level evidence, and keep it from competing with the required contract sections:

- Maturity status — conditional; add when the package promotes building blocks through tiers or tracks open contracts as future work. Place it immediately after `Build and support status`. Carry a promotion-axis tier table and a dependency-ordered set of open contracts rendered as status-tagged records, each stamped with a freshness field.
- Capability catalog — conditional; add when consumers need the package's downstream use cases. Place it after `Invariants and rejected shapes` and before `Proof and evidence`, mark it explicitly as non-contract context, and stamp it with a `Review trigger:` so the catalog has a drift anchor.

## [7][REQUIRED_STRUCTURES_CONTENT]

These structures are mandatory wherever the matching content appears; flat prose for any of them is a defect. Each maps a content kind to the container the form standard requires.

| [INDEX] | [CONTENT_KIND]                 | [REQUIRED_STRUCTURE]                      | [WHERE_IT_APPLIES]                |
| :-----: | :----------------------------- | :---------------------------------------- | :-------------------------------- |
|   [1]   | Open architectural risks       | Status-tagged records                     | Risks section, both profiles      |
|   [2]   | Glossary terms                 | Definition block, one term per entry      | Risks section, both profiles      |
|   [3]   | Maturity tiers and open work   | Tier table plus status-tagged records     | Maturity status, owner-contract   |
|   [4]   | External actors and data       | Record table or grouped definition blocks | Context and scope                 |
|   [5]   | Building blocks and owners     | Record table plus text codemap            | Building-block view               |
|   [6]   | Diagram caption facts          | Definition block, one fact per line       | Beside every diagram              |
|   [7]   | Invariants and rejected shapes | One observable rule per record            | Invariants section, both profiles |
|   [8]   | Diagram-type selection         | Decision table by reader question         | C4 view selection                 |
|   [9]   | Review gates                   | Verification checklist                    | Review checklist                  |

Render open architectural risks with the closed `Status` vocabulary the form standard defines (`PLANNED`, `IN-PROGRESS`, `BLOCKED`, `DONE`, `DROPPED`) and the recurring record fields. A risk that lacks an `Exit` condition is unfalsifiable architectural worry, not a tracked risk:

```markdown template
### [N.M][BRIDGE_RE_ENTRANCY]

Status: IN-PROGRESS
Exit: the bridge serializes solves; a concurrent-solve fixture passes.
Owner: Runtime maintainers
Proof: tests/csharp/.../BridgeConcurrencyTests.cs run output.
```

Escalate a risk record table to per-item record blocks when any item has more than five fields or any field needs a list or code block, per the form standard.

Render the glossary co-located in the Risks section as a definition block, one term per entry, so a first-time reader resolves a term inline rather than chasing it elsewhere. Define only terms a reader cannot infer from common domain vocabulary:

```markdown conceptual
Bridge: the in-process boundary in `Bridge/` through which every cross-owner call enters; nothing crosses owners outside it.
Solve: one spectral computation pass owned by `Solver/`; serialized so concurrent solves cannot interleave checkpoints.
```

Show the actor record table the `Context and scope` section requires when three or more external actors cross the boundary; each row names the actor, its direction relative to the system, and the data or contract that crosses:

```markdown conceptual
| [INDEX] | [ACTOR]          | [DIRECTION] | [DATA_CONTRACT_THAT]                             |
| :-----: | :--------------- | :---------- | :----------------------------------------------- |
|   [1]   | Rhino host       | inbound     | geometry document handle; in-process bridge call |
|   [2]   | Mesh export sink | outbound    | serialized mesh payload; versioned schema        |
```

Show the maturity tier table the `Maturity status` section requires when a package promotes building blocks through tiers; each row names the tier, the promotion bar that admits a block to it, and the freshness anchor that keeps the row honest:

```markdown conceptual
| [INDEX] | [TIER]      | [PROMOTION_BAR]                                    | [FRESHNESS]    |
| :-----: | :---------- | :------------------------------------------------- | :------------- |
|   [1]   | Stable      | public contract frozen; consumers depend on it     | Last verified  |
|   [2]   | Provisional | shape published, signature may break before freeze | Review trigger |
```

Render the open contracts that accompany the tier table as the same status-tagged record blocks the Risks section uses, each stamped with a freshness field.

## [8][C4_VIEWS_DIAGRAM]

C4 owns diagram semantics; the rendering tool does not. Select the diagram type from the question the view answers, and cap each top-level view at 7 boxes so it stays reviewable in source. Split a view that needs more than 7 boxes into a parent view and a drill-down rather than crowding one canvas. Keep level consistency: every element inside a Component view belongs to the container it drills into, never to the broader system.

| [INDEX] | [READER_QUESTION]                     | [C4_VIEW]  | [WHEN_IT_IS]                                   |
| :-----: | :------------------------------------ | :--------- | :--------------------------------------------- |
|   [1]   | Who and what does the system talk to? | Context    | `Landscape` profile, always                    |
|   [2]   | What are the deployable units inside? | Container  | `Landscape` always; `Owner-contract` floor     |
|   [3]   | How do parts of one container fit?    | Component  | When an internal boundary outlives one release |
|   [4]   | In what order does a flow execute?    | Dynamic    | When flow order proves a current invariant     |
|   [5]   | Where do units run and connect?       | Deployment | When deployed topology proves an invariant     |

Code-level diagrams stay out of architecture documents: when class or call structure is the subject, the codemap and generated symbol reference carry it. C4 itself recommends generating code-level diagrams rather than hand-drawing them, because a hand-drawn code diagram goes stale on the next commit; this standard resolves that by routing code structure to the generated symbol reference entirely.

Caption every diagram with five facts so a maintainer can re-verify it and a reader has a visible text equivalent. Use a definition block, one fact per line, beside the diagram:

```markdown template
Title: Container diagram — Rasm solver bridge
View: C4 Container
Authored from: docs/architecture/model.dsl
Rendered by: structurizr-cli export --format mermaid
Source of truth: repository paths in the building-block view
```

Give every rendered diagram a title that names its C4 view type and scope (for example, `Container diagram — Rasm solver bridge`), because C4 requires an unambiguous title and key on every diagram. The caption block above carries maintainer-facing provenance and the visible reader-facing text equivalent.

Use a checked-in architecture model when repository tooling, a manifest, or the existing architecture corpus configures one. When no model tool is configured, authored diagram source is acceptable only when the caption names the source, the renderer, the source of truth, and how the diagram was verified against current paths. When that verification is a human review rather than a tool gate, record a named `Proof gap:` beside the caption; a stated gap is acceptable evidence for an inline sketch, not a missing gate. Introduce Structurizr only when current repository tooling or the architecture owner makes it the modeling surface.

Treat generated SVG, PNG, PlantUML, exported Mermaid, and static-site output as generated artifacts: edit the model, not the export.

Use Mermaid in two roles only, and label the block so a reader knows which:

- a single inline `flowchart` that follows C4 terminology, as a lightweight sketch when no model tool is configured;
- Mermaid exported from a checked-in architecture model.

Do not treat Mermaid source as the architecture model.

## [9][CODEMAP]

Derive the codemap from the repository tree, never from intent. Include real paths at two or three directory levels, state the owner boundary each path holds, and omit a leaf file unless it is a public contract or a central algorithm. Use a monospace text tree so the structure stays readable in raw Markdown and diffable in review:

```text conceptual
libs/Rasm/
  Geometry/        # owns mesh and curve primitives; public contract
  Solver/          # owns the spectral solve; central algorithm
  Bridge/          # owns the in-process Rhino bridge boundary
```

A codemap entry that no longer resolves to a current path is a defect, not stale prose: fix the path or remove the entry in the same change. The codemap and the C4 Container view describe the same decomposition; when they disagree, one is stale and the change is incomplete.

## [10][PROOF]

Architecture proof is a claim-level obligation: name how each drift-prone fact was refreshed against repository truth, beside the fact. State, per document:

- the source paths or manifests the codemap was derived from;
- the authored diagram-model path, when a model exists;
- the renderer or export command, when a diagram is generated;
- the verification that model elements and codemap match current repository paths.

Do not claim a diagram reflects the system unless its elements were checked against current paths during the change. When that check is a human review rather than a tool gate, state the gap. Use the freshness fields the evidence standard defines — `Evidence:`, `Last verified:`, `Review trigger:`, `Generated from:`, `Source of truth:` — and place them beside the claim, not in a page footer.

## [11][EXAMPLES]

Show the boundary that authors miss most: an invariant stated as an observable rule versus an invariant stated as intent.

Accepted — observable and falsifiable:

```markdown conceptual
Invariant: every cross-owner call enters through `Bridge/`; a direct
`Geometry/ -> Solver/` reference fails the architecture analyzer.
```

Rejected — unfalsifiable intent:

```markdown rejected
Invariant: the system is cleanly layered and well decoupled.
```

The accepted form names the rule, the boundary, and the check that proves it. The rejected form states a feeling no maintainer can verify or refresh.

Show the second boundary authors miss: a tracked risk as a status-tagged record versus a risk as a worried sentence.

Accepted — tracked and falsifiable:

```markdown conceptual
### [N.M][SOLVER_CHECKPOINT_FORMAT]

Status: PLANNED
Exit: checkpoint headers carry a version byte; the loader rejects unknown versions.
Owner: Solver maintainers
Proof: Solver/Checkpoint.cs version field + loader fixture.
```

Rejected — untracked worry:

```markdown rejected
Risk: the checkpoint format might cause problems later.
```

The accepted record carries the state, the exit condition, the owner, and the proof a maintainer filters on. The rejected sentence carries none of those and cannot be tracked, retired, or refreshed.

## [12][BOUNDARIES]

- [adr.md](adr.md) owns why a durable architectural choice was made.
- [roadmap.md](roadmap.md) owns build sequence, milestones, and exit criteria.
- [runbook.md](../task/runbook.md) owns operational recovery and incident response.
- [api.md](../reference/api.md) owns generated endpoint and symbol surfaces.
- [readme.md](../reference/readme.md) owns the entry point and adoption links.
- [README.md](../README.md) owns document-type routing, placement, and the lifecycle of this corpus.

## [13][REVIEW_CHECKLIST]

- [ ] Exactly one system-scale profile is named, and its file name matches.
- [ ] No directory holds both `ARCHITECTURE.md` and `_ARCHITECTURE.md`.
- [ ] The document's headings match the required-structure template for the chosen profile.
- [ ] Every required section for the chosen profile is present and non-empty.
- [ ] Conditional runtime and deployment sections are present when their trigger holds and absent otherwise.
- [ ] Quality goals are named and falsifiable, and each solution-strategy choice binds to a stated goal.
- [ ] Open architectural risks are status-tagged records with `Status`, `Exit`, and `Owner`, never flat prose.
- [ ] Glossary terms a first-time reader cannot infer are rendered as a co-located definition block, one term per entry.
- [ ] Context actors, building blocks, and maturity tiers use the record structures their content kind requires.
- [ ] Required C4 views exist for the profile, and no top-level view exceeds 7 boxes.
- [ ] Each diagram carries view type, authored source, renderer, source of truth, and a scoped title.
- [ ] Mermaid appears only as a renderer or a labeled lightweight sketch.
- [ ] Every codemap path resolves to a current repository path, and the codemap matches the Container view.
- [ ] Each invariant is stated as an observable, falsifiable rule with the check that proves it.
- [ ] Drift-prone facts carry claim-level evidence and a freshness field, and human-reviewed checks state the proof gap.
