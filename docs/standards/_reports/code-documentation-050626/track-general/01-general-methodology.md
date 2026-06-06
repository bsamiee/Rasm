# [GENERAL_01_CODE_DOCUMENTATION_METHODOLOGY]

This report studies universal source-comment decision methodology for `docs/standards/reference/code-documentation.md`. It does not propose edits to active standards in-place; it records candidate changes, removals, no-change confirmations, and source evidence for a later standards pass.

## [1][SCOPE]

Assigned focus: `GENERAL 1`, universal code-documentation theory and comment-decision method beyond language-specific syntax.

Assigned output: `docs/standards/_reports/code-documentation-050626/track-general/01-general-methodology.md`.

Active standards read fully:
- `docs/standards/reference/code-documentation.md`
- `docs/standards/style-guide.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`

Repo instruction context checked:
- `CLAUDE.md`
- root `AGENTS.md`
- `~/.codex/memories/MEMORY.md` entries for the June 4-5 `docs/standards` rebuild, reference rewrite, metadata cleanup, and `_reports/` handling.

Current workspace note: `docs/standards/reference/code-documentation.md` was already modified in the worktree before this report. This report leaves that active file untouched.

## [2][CURRENT_STANDARD_SNAPSHOT]

`code-documentation.md` already has a strong universal spine:
- It leads with a high-value rule: comments exist only for caller-visible semantics that declarations cannot express.
- It makes public visibility a review question instead of an automatic comment requirement.
- It separates machine shape, semantic contract, generated reference, and routed documentation.
- It defines surface profiles for pure, rail, throwing, script, and catalog surfaces.
- It rejects type echoes, carrier echoes, name echoes, hidden side effects, line narration, and generated/lifecycle preservation.

The main weakness is not missing language syntax. The universal methodology is present but diluted by long language capsules and volatile version/tool details. A later edit should sharpen the decision method before readers reach language mechanics, then move repeated capsule detail behind that method or compress it.

## [3][SOURCES_CHECKED]

[LOCAL_REPO]:
- `docs/standards/reference/code-documentation.md`: current target standard and active source-comment doctrine.
- `docs/standards/style-guide.md`: prose shape, noise removal, examples, code-safe Markdown, final proofing pass.
- `docs/standards/agentic-documentation.md`: salience, evidence-before-synthesis, artifact separation, provider claims, retrieval and generated mirrors.
- `docs/standards/README.md`: reader-need routing, placement, split/link, lifecycle, shared standard ownership.
- `docs/standards/AGENTS.md`: standards-folder read scope, rule owners, forbidden metadata/process patterns, close checks.
- `CLAUDE.md` and root `AGENTS.md`: project-level Markdown/docs routing, source precedence, no active-code validation rails for docs-only report work.

[PRIMARY_OR_STABLE_EXTERNAL]:
- Google C++ Style Guide, `Comments`: self-documenting code first; declaration comments describe use when non-obvious; definition comments explain tricky implementation choices; data-member comments cover invariants, special values, relationships, and lifetime requirements. Source: <https://google.github.io/styleguide/cppguide.html>.
- Google Python Style Guide, `Comments and Docstrings`: right comment style by surface; mathematical notation comments cite the source of naming conventions; public API names should remain descriptive out of context. Source: <https://google.github.io/styleguide/pyguide.html>.
- PEP 257, `Docstring Conventions`: docstrings are runtime-accessible object documentation; one-line docstrings must not repeat signatures available by introspection; function docstrings summarize behavior and document arguments, returns, side effects, exceptions, and call restrictions when applicable. Source: <https://peps.python.org/pep-0257/>.
- Go Doc Comments: extracted source comments feed `go doc` and `pkg.go.dev`; type comments should explain what an instance represents, concurrency guarantees stronger than the default, zero-value meaning when non-obvious, and exported field meaning. Source: <https://go.dev/doc/comment>.
- Rust API Guidelines, `Documentation`: examples should often show why to use an item, not mechanically how to call it; function docs include error, panic, and safety considerations; rustdoc should include what users need and hide irrelevant implementation details. Source: <https://rust-lang.github.io/api-guidelines/documentation.html>.
- Swift API Design Guidelines: clarity at point of use is the primary API goal; documentation writing can reveal a wrong API design; summaries are the most important part of many comments. Source: <https://www.swift.org/documentation/api-design-guidelines/>.
- Microsoft Learn C# XML documentation tags: `<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`, and `cref` are parsed and surfaced by tooling, with compiler checks for parameter and `cref` validity. Source: <https://learn.microsoft.com/en-au/dotnet/csharp/language-reference/xmldoc/recommended-tags>.
- TSDoc `Approach` and `Tag kinds`: common tags need consistent behavior across tools; unsupported custom tags should not interfere with parsing; content before the first block tag is the summary section; modifier tags are parsed as special API qualities. Sources: <https://tsdoc.org/pages/intro/approach/> and <https://tsdoc.org/pages/spec/tag_kinds/>.
- Diátaxis: documentation types follow user needs; reference is distinct from tutorial, how-to, and explanation. Source: <https://diataxis.fr/>.
- PostgreSQL 18 `COMMENT`: `COMMENT ON` stores one comment per object; comments are visible through `psql` and description functions; there is no security mechanism for viewing comments. Source: <https://www.postgresql.org/docs/18/sql-comment.html>.
- GNU Bash 5.3 Reference Manual, command substitution: Bash 5.3 current-shell substitution preserves side effects in the current environment. Source: <https://www.gnu.org/s/bash/manual/html_node/Command-Substitution.html>.
- Python 3.15.0b2 release page: Python 3.15 was still a beta preview on 2026-06-02 and not recommended for production environments. Source: <https://www.python.org/downloads/release/python-3150b2/>.
- TypeScript 7.0 beta announcement: TypeScript 7.0 was beta as of 2026-04-21, with the stable release still planned for later and the stable programmatic API not expected until at least 7.1. Source: <https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/>.
- PostgreSQL 18.4 release notes: PostgreSQL 18.4 was released on 2026-05-14. Source: <https://www.postgresql.org/docs/release/18.4/>.

[RESEARCH_CONTEXT]:
- Rani et al., `A decade of code comment quality assessment`: comment quality lacks one universal definition; research commonly studies consistency, completeness, readability, and language/tool-specific comment types; much evidence is Java-heavy and not automatically generalizable. Source: <https://www.sciencedirect.com/science/article/pii/S0164121222001911>.
- IBM Research, `Markdown Mayhem`: agent-facing Markdown proliferation can create ambiguity, redundancy, and erosion of authoritative truth. Source: <https://research.ibm.com/publications/markdown-mayhem-taming-the-agentic-documentation-explosion>.
- `Documentation-Guided Agentic Codebase Migration from C to Rust`: recent agentic migration work treats repository documentation as architecture/API/design-rationale blueprint and compares output documentation against source documentation for repair signals. Source: <https://arxiv.org/abs/2605.14634>.
- Fantino et al., `Beyond syntax: enhancing automated documentation with data differences`: code comments often need signals beyond static syntax; execution-derived effects can reduce unnecessary or speculative detail in neutral cases but remain sensitive to comment-code alignment. Source: <https://link.springer.com/article/10.1007/s10515-026-00623-y>.

## [4][FINDINGS]

### [4.1][COMMENT_DECISION_IS_A_DELTA_TEST]

The best universal formulation is a delta test: compare what the declaration, type system, schema, shell syntax, SQL catalog, or generator already says against what a caller still must know to use the surface correctly. `code-documentation.md` already states this, but the decision record can be made more operational.

Candidate stronger rule:
- `Machine shape`: what tooling can extract without prose.
- `Semantic delta`: what remains caller-visible after machine shape is removed.
- `Risk class`: what goes wrong if the delta is absent.
- `Comment route`: source comment, generated reference, README/reference/architecture/support/runbook/how-to/tutorial, or omit.

Why this is not table stakes: most public-comment rules say "document public APIs." The stronger method asks the reviewer to subtract machine truth first, then document only residual caller risk. This supports Rasm's current anti-spam doctrine and makes the omit path as first-class as the write path.

Confidence: high. It is supported by the current standard, PEP 257's no-signature-repetition rule, Go's exported-field and zero-value guidance, Google's self-documenting-code rule, and Rust's user-needed-only rustdoc guidance.

### [4.2][COMMENT_REVIEW_SHOULD_CLASSIFY_RISK_BEFORE_TAGS]

The current standard classifies surfaces as pure, rail, throwing, script, and catalog. That is useful, but the universal section could benefit from a risk taxonomy that cuts across all languages:
- Misuse risk: caller passes semantically valid but wrong input.
- Outcome risk: caller misreads success, absence, failure, or status.
- Boundary risk: side effect, IO, mutation, transaction, lock, resource, cancellation, trap, or terminal runner behavior is hidden.
- Exposure risk: security, tenant scope, public catalog visibility, trace/log data, or secrets risk is hidden.
- Lifecycle risk: external caller relies on a surface that is preview, deprecated, removed, or migration-bound.
- Tooling risk: generated reference, resolvable link, compiler/parser contract, or catalog extraction changes.

This risk pass would give agents a decision ladder before they enter language capsules. It also makes the standard easier to apply to future languages without adding another long capsule.

Confidence: high. The categories are already scattered through the current `Use when`, `Decision router`, `Surface model`, and language capsules; the improvement is consolidation, not new doctrine.

### [4.3][EXAMPLES_SHOULD_BE_SEMANTIC_PROBES]

Rust's API guideline has a useful non-obvious distinction: an example is often better when it shows why the item matters, not just how to invoke it. Rasm's standard already says examples are conditional and should prevent misuse, but it could state a sharper test:
- Add an example when it reveals a semantic distinction a caller cannot infer from the signature or ordinary syntax.
- Remove an example when it only proves that the call compiles or mirrors the declaration.
- Prefer one misuse-preventing example over a generic positive sample when the risk is hidden failure, resource ownership, security, or lifecycle.

This would improve `code-documentation.md` without turning it into a tutorial standard. It preserves the reference route because the example is a semantic probe, not a task path.

Confidence: high. It aligns with Rust's `why`, the local examples rule in `style-guide.md`, and `README.md` route-away rules for tutorials/how-to guides.

### [4.4][COMMENT_CREATION_CAN_SIGNAL_API_DESIGN_FAILURE]

Swift's API design guidance says difficulty explaining an API in simple terms can reveal a design problem. This is valuable for Rasm because the repo already prefers refactoring and polymorphic consolidation over comment compensation.

Candidate addition:
- If the semantic comment must explain parallel names, unclear carrier meaning, hidden mode switches, repeated branches, or caller-visible state that could be encoded in a type, value object, algebra, schema, or command surface, treat the comment as a design-smell signal before accepting it.
- Keep the comment only when the remaining obligation cannot move into the declaration or canonical owner without making the code less correct.

This bridges code-documentation theory with the repo's engineering contract. It prevents comments from becoming a compatibility shim for bad API shape.

Confidence: medium-high. It is strongly aligned with repo policy and Swift/Google API clarity guidance. It needs careful wording so authors do not delete necessary semantic comments just because a refactor would be ideal but out of scope.

### [4.5][GENERATED_OUTPUT_IS_A CONSUMER, NOT A SECOND SOURCE]

The standard already says generated references mirror source truth. It can be made more explicit that generated API docs, compiler XML, TSDoc extraction, rustdoc, `go doc`, pydoc, PostgreSQL catalog output, and shell/script catalogs are consumers of source comments, not co-equal authoring locations.

Candidate stronger rule:
- A source comment change that alters generated text creates a generated-output check.
- A generated page that is wrong because of source comments is fixed at the source comment.
- A generated page that is wrong because of generator configuration is fixed at the generator or API standard route.
- A generated page must not receive hand-written patch prose that forks from the source comment.

Confidence: high. This is supported by `agentic-documentation.md` generated mirror rules, Microsoft XML docs, TSDoc parser guarantees, Go doc extraction, PostgreSQL `COMMENT` extraction, and the current `api.md` route.

### [4.6][SECURITY_AND_VISIBILITY_NEED_A_COMMENT_PRIVACY_TEST]

PostgreSQL makes this concrete: object comments can be viewed by any connected user, so sensitive information does not belong there. The universal method should generalize this beyond SQL:
- Before adding a source comment, ask who can read the generated mirror, IDE hover, package docs, catalog output, logs, or public repository.
- Reject secrets, privileged assumptions, exploit detail, tenant IDs, personal data, host-local private paths, and nonpublic runtime details unless a controlled internal route is the explicit consumer.
- If the comment must mention a security boundary, state the boundary without publishing sensitive operational material.

The current PostgreSQL capsule contains this rule, but the privacy test is universal. Source comments often leak through generated docs and search indexes even when authors think they are "inside code."

Confidence: high. Supported by PostgreSQL docs, local `AGENTS.md` forbidden-pattern rules, and `agentic-documentation.md` generated mirror/access boundary rules.

### [4.7][LIFECYCLE_TAGS_NEED_EXTERNAL_CONSUMER PROOF]

The current standard already says lifecycle tags preserve only external support contracts. That is worth keeping and possibly tightening with a universal proof line:
- A lifecycle tag is allowed only when a named external consumer changes behavior because of the tag.
- Internal greenfield surfaces should be deleted, renamed, or replaced rather than marked deprecated for preservation.
- A tag without replacement path, behavioral delta, removal/migration condition, and review trigger is noise.

Confidence: high. The current standard already states this and it aligns with repo greenfield policy.

### [4.8][QUALITY_EVIDENCE_SHOULD_REJECT_COMMENT-CODE CONSISTENCY THEATER]

The research literature emphasizes consistency and completeness, but also shows that comment quality is multi-dimensional and not reducible to a generic linter. For Rasm, the useful takeaway is not to add broad missing-comment gates; it is to validate only the changed comment's source truth, generated mirror, and route effects.

Candidate validation language:
- Do not measure quality by comment count or public-surface coverage alone.
- Validate a source-comment change against the machine shape it claims to supplement, the generated consumer it feeds, and the caller risk it exists to prevent.
- Prefer stale-term and route-away scans over missing-comment churn for docs-only standards work.

Confidence: medium-high. The SLR supports caution around simplistic quality metrics, but it is older and Java-heavy. The recommendation is mostly grounded in repo policy plus source-specific tooling docs.

### [4.9][AGENTIC_DOCS_NEED LESS MARKDOWN, MORE AUTHORITY ROUTING]

Recent agentic documentation work reinforces the local standard's existing direction: many Markdown surfaces can erode authoritative truth. `code-documentation.md` should not grow a new agent-specific subsection unless it changes source-comment decisions. The useful universal rule is shorter:
- Source comments are not state artifacts, task prompts, or agent memory.
- Comments should not carry prompt-era rationale, generated critique, task stage labels, or proof transcripts.
- If an agent needs repository context, route it to README, architecture, reference, generated API docs, or state artifacts instead of copying context into source comments.

Confidence: high for the local recommendation; medium for external generality because the IBM item is a position paper. It matches current `agentic-documentation.md` and `AGENTS.md`.

### [4.10][VOLATILE VERSION MICRODETAILS ARE DILUTING THE UNIVERSAL METHOD]

Current `code-documentation.md` names TypeScript 7, Python 3.15, Bash 5.3+, and PostgreSQL 18.4 in a required language capsule list. Some are valid current or preview facts on 2026-06-05, but they create drift and pull the file toward language-release tracking.

Verified on 2026-06-05:
- PostgreSQL 18.4 is current stable, released 2026-05-14.
- Bash 5.3 is current in the GNU Bash manual.
- Python 3.15 is still beta preview, not recommended for production environments.
- TypeScript 7.0 is beta; the stable `typescript` package release and stable programmatic API are not yet final, with API stability expected no earlier than 7.1.

Recommendation: keep exact language baselines only where the repo manifest or language skill owns them. The universal `code-documentation.md` should describe how to verify toolchain syntax and generated consumers, then route volatile release facts to language skills or generated/tooling docs.

Confidence: high for removing or compressing volatile microdetail from the universal layer. Exact baselines may still be intentional repo policy, but the report scope is methodology beyond language specifics.

## [5][ADD_RECOMMENDATIONS]

Add a compact universal `COMMENT_DECISION_LADDER` before `Produced shape` or inside `Decision router`:

```text
1. Identify the public surface and generated consumers.
2. Subtract machine shape already carried by declarations, schemas, shell syntax, SQL objects, annotations, and catalog metadata.
3. Classify the remaining caller risk: misuse, outcome, boundary, exposure, lifecycle, or tooling.
4. Choose the owner: source comment, generated API route, reference, README, architecture, support, runbook, how-to, tutorial, or omit.
5. Validate only the changed contract, generated consumer, and adjacent route affected by the chosen owner.
```

Add a universal `COMMENT_AS_DESIGN_SIGNAL` rule:
- If a comment explains a shape that should be encoded in names, types, schemas, value objects, discriminants, operation algebras, or source-owned tables, consider a source refactor before accepting the comment.
- If the semantic obligation cannot be encoded cleanly, keep the comment as caller contract.

Add a universal `EXAMPLE_DECISION` rule:
- Use an example only when it prevents a likely semantic misuse.
- Reject examples that merely show invocability, syntax, or a happy path obvious from the declaration.

Add a universal `COMMENT_PRIVACY` rule:
- Treat comments as potentially surfaced through generated docs, hovers, package pages, catalogs, repository search, and retrieval.
- Do not put sensitive operational detail, secrets, tenant identifiers, exploit details, private host paths, or unsupported privileged assumptions in source comments.

Add a universal `GENERATED_CONSUMER` rule:
- Generated references consume source comments and must not become a second hand-maintained truth source.
- Fix wrong generated text at the source comment or generator owner, not by patching the generated page body.

## [6][CHANGE_RECOMMENDATIONS]

Change `Surface model` from profile-first prose to profile plus risk:
- Keep pure, rail, throwing, script, and catalog.
- Add risk classes that are reusable across profiles.
- Use the risk classes to decide which fields are allowed in the comment.

Change `Produced shape` so the review record has fewer fields and a clearer omit path:
- Replace `Profile` with `Profile/Risk`.
- Replace `Semantic comment fields` with `Semantic delta`.
- Replace `Omit when` with `Omit proof`, so an agent must name why no comment is needed when reviewing a public surface.

Change language capsules from detailed doctrine to syntax adapters:
- Keep each capsule focused on parsed syntax, generated consumer, and language-only hazards.
- Move repeated result/effect/resource theory to the universal ladder.
- Keep language examples as syntax cues only.

Change lifecycle guidance to require a consumer:
- Require `Consumer`, `Replacement`, `Behavior delta`, `Removal or migration condition`, and `Review trigger`.
- Reject lifecycle markers that exist only to preserve stale internal surfaces.

## [7][REMOVE_OR_COMPRESS_RECOMMENDATIONS]

Remove or compress drift-prone version microclaims from the universal standard:
- `TypeScript 7` as a baseline in a universal required capsule is premature while TypeScript 7.0 remains beta and its stable programmatic API is not yet available.
- `Python 3.15` feature lists are not stable production guidance on 2026-06-05 because Python 3.15 is still beta preview.
- Bash 5.3 and PostgreSQL 18.4 are currently verifiable, but their exact feature/version claims still belong in language skills or toolchain docs unless this standard must own them.

Remove repeated rail/resource lists that duplicate the universal model:
- The C#, TypeScript, and Python capsules all restate success/failure/resource/cancellation semantics in language-specific vocabulary.
- Keep the language-specific carriers, but move the semantic obligations to the universal surface model.

Remove proof-field duplication where `proof.md` already owns the vocabulary:
- `Generated from`, `Source of truth`, `Last verified`, `Review trigger`, and `Evidence` should appear only when a local relation record needs them.
- Otherwise route to `proof.md` and avoid another metadata-like schema.

Remove examples that teach only syntax:
- Keep a syntax cue only if the language capsule needs it.
- Replace generic examples with one accepted/rejected cross-language contrast when the universal method needs reinforcement.

Remove broad "document public surface" implications:
- Where external guides require all exported/public items to have comments, retain as a toolchain or language-capsule fact.
- Keep the universal Rasm rule: public visibility creates a review question, not a comment requirement.

## [8][NO_CHANGE_CONFIRMATIONS]

Keep the opening doctrine. It is the strongest part of the standard and matches external guidance: comments carry semantic contract that source cannot express, not type or name echoes.

Keep the route-away section early. It prevents source comments from absorbing generated API pages, reference catalogs, README entry maps, architecture, support, tasks, tutorials, and runbooks.

Keep omission as an explicit valid outcome. Most external guidance starts from "document public APIs"; Rasm's stronger rule is better for a codebase that treats names, types, schemas, and generated contracts as primary truth.

Keep the anti-pattern list. The cross-language anti-patterns are precise and actionable: type-restating parameter, throw tag for typed rail, carrier echo, hidden side effect or cancellation, name-echo summary, profile leakage, line narration, and generated/lifecycle preservation.

Keep lifecycle externality. Lifecycle comments must serve external support contracts and should not preserve greenfield stale surfaces.

Keep source/generated separation. Generated references are consumers and mirrors, not authoring locations.

Keep docs-only proof boundaries. A research transcript/report does not justify running C#, TypeScript, Python, Bash, SQL, static, test, bridge, or generated-reference rails.

## [9][CONFIDENCE]

Overall confidence: high.

High-confidence recommendations:
- Add the comment-decision ladder.
- Add risk classes before language mechanics.
- Treat examples as semantic probes.
- Promote comment privacy to universal guidance.
- Keep generated references as consumers, not second sources.
- Compress version/tool microdetail out of the universal layer.

Medium-confidence recommendations:
- Add comment-as-design-smell language. It fits Rasm policy, but later wording should avoid implying that every necessary comment demands a refactor.
- Add an `Omit proof` field. It could improve reviews, but it may feel like metadata if applied too mechanically. Use it as an authoring review field, not as source-comment text.

Low-confidence or deferred:
- Adding a separate agentic-source-comment subsection. The current `agentic-documentation.md` already owns artifact separation and provider behavior; `code-documentation.md` should only include agentic concerns when they affect source-comment ownership.

## [10][NEXT_PASS_PACKET]

Suggested later edit scope:
- Touch only `docs/standards/reference/code-documentation.md`.
- Do not alter active language baselines unless the maintainer wants the universal standard to stop naming preview versions.
- Preserve all current route-away links.
- Fold repeated capsule resource/failure theory into the universal ladder.
- Keep one compact language capsule per language as a syntax/generator adapter.
- Run only docs-only checks after active-standard edits: `git diff --check -- docs/standards`, plus local Markdown link/anchor checks if headings or links change.
