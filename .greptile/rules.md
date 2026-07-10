# Review context — Rasm

C#/.NET-primary monorepo with TypeScript and Python lanes. Language doctrine is codified under docs/stacks/{csharp,typescript,python}; design law under docs/standards/. Review against those documents, not generic community convention. Where a finding and the doctrine disagree, the doctrine wins.

## Machine-law boundary

Formatters and gates own mechanics — never restate their law as findings; flag only suppressions and bypasses. A true positive from any gate is architecture pressure (fix the shape); a false positive is rule pressure (refine the rule); a suppression (`#pragma`, `[SuppressMessage]`, `noqa`, `type: ignore`, `biome-ignore`, `@ts-expect-error`, a bare `as` cast) is neither and demands the ownership justification in the diff. Two scope facts govern deference:

- The C# custom analyzer catalog is empty — every C# doctrine demand below is live review duty, not machine-enforced.
- The TypeScript GritQL plugins cover only libs/typescript non-spec sources — the identical defect classes in tools/, workflow scripts, config TS, and every spec file are review duty.

## Universal discipline

- Anticipate 10x growth: surfaces absorb new modalities as rows, cases, or dispatch arms — never as new files, flags, or knobs. Two shapes collapse the moment they share an identity regime, an admission path, a payload timing, or a consumer; a sibling survives only on a genuinely distinct discriminant named at the site that keeps it.
- The proof of a shape is the next feature's diff: one declaration inside the owner, consumers untouched or loudly compile-broken. An owner sized for the current instance — `parse(input)` hardcoding one format instead of a format vocabulary row the parse dispatches on — is the defect.
- Abort-vs-accumulate is a correctness decision fixed once at the boundary: independent operands compose applicatively and accumulate evidence; a bind chain reporting only the first failure over independent field validations is the defect; abort is reserved for dependent steps.
- One entrypoint owns singular, plural, batch, and stream, discriminated on the input value itself. `Solve(x)` beside `SolveMany(xs)`, or `run(x, batch: true)`, smuggles the knob back in; forward and inverse of one correspondence ride one owner, never direction-named siblings.
- Naming grammar: one semantic word default; two when owner plus action, result, axis, or boundary is load-bearing; three the ceiling absent a generated contract or external API. Operations are action verbs, values and receipts are result nouns, policies and vocabularies are stable noun rows; renaming for variety, tense drift, abbreviations, and prefix/suffix families are collapse debt.
- An admitted package capability left unmined is the same defect as hand-rolling it: a hand loop re-deriving a combinator the package ships is flat code below admitted operator depth.
- A name resolves to semantics in one hop: no alias-to-constant-to-enum chains, no forwarding helpers, no helper or util shells. Compat shims, `[Obsolete]` aliases, and deprecated-name re-exports are the deleted form — the sanctioned path is the aggressive break with every call site updated in the same change. Destructuring is projection, not a rename — never flag it.
- Configuration enters as one policy value carrying its own behavior — a smart-enum row with delegates, a union case whose body is the policy, a frozen table, an as-const row — never flag sets whose combinations the implementation re-derives.
- One primary correspondence is declared; every secondary map, type, schema, or handler derives from it. Enum-value-as-driver is the house idiom (the enum value IS the backend string or resolved callable name); a parallel identity table is twin truth in code.
- Expected domain outcomes (non-convergence, absence, degraded verify) ride the success receipt as typed status; unexpected host faults ride the fault rail. Raising on max-iterations instead of returning a status slot on the receipt conflates the two.
- Every table-, law-, or row-driven surface refuses empty input loudly; a fold that silently succeeds on zero rows is a vacuous fold.
- Comments are rare, 1-2 lines, and state the design law, invariant, or trap the code cannot show — never the what, never paths or session narration; every pass touching a file prunes stale comments in the same pass.
- Fix-to-root completeness: a change that patches a symptom while its root cause stands, leaves a known defect unfixed because it sits outside the diff's scope, or defers a residual for a later pass is a defect — the root fix belongs in the same change, and a genuinely blocked item is an explicit unreachable naming its owner, never a silent residual.

## C#

- Raw material admits once at the boundary: `Optional(x).ToFin(...)` for presence, `Catch`/`Try` lifts for native failures, `Option<T>` for non-failing absence; the interior never re-validates and never sees nulls, sentinels, or provider shapes.
- Rails are semantic: `Validation<Fault,T>` accumulates over independent operands, `Fin<T>` aborts over dependent steps, `IO<T>` effects. `Fin` where accumulation is owed is a carrier misselection. Receipts use NodaTime.
- Domain shape lives in generated owners (`[Union]`, `[SmartEnum]`, `[ValueObject]`, `[ComplexValueObject]`); behavior brackets through generated state-threaded `Switch` with static lambdas. A hand enum with an external switch, or an is-chain over a union, is the deleted form.
- Strata: `Rasm` references nothing; `Rasm.Element` references only `Rasm` and no provider package; the AEC peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) depend up on `{Rasm, Rasm.Element}` and never on each other — alignment rides projection and constraint seams and the content-keyed wire; Rhino and Grasshopper assemblies are admitted only at app roots; `Rasm.AppUi` is a consuming leaf, never the composition root. A csproj reference breaching these edges is a strata defect.
- Universal does not mean host-free: the C# branch is RhinoCommon-aware, kernel included; a host-neutral owner exists only for a genuine non-Rhino consumer contract. Never flag Rhino-rich capture as "should be portable"; never accept a consumerless universal owner.
- UI-bound host work routes through one dispatch owner; a batch takes one dispatch under one policy bracket; native object access returns `Fin`/`Option`, never leaked nulls; consumers never sequence an owner's internals.

## Python

- Lifecycle: raw admits once into a canonical owner, then rail, projection, egress. `Option` for non-failing absence, `Result` for fallibility, exceptions convert at the owning boundary; None-as-failure and sentinel flow are defects.
- Dispatch totality is review-critical because no compiler backs it. Closed dispatch is a tagged-union case, `StrEnum` member, `frozendict` row, or `match` with `assert_never`; string dispatch and if/elif ladders are open dispatch — never a free `solve(intent)` beside a free `_dispatch`.
- The doctrine names the review residue the gates cannot catch: class/type/string/constant spam, one-method classes, get/get_many families, private-dunder enum probes, thin rename wrappers, scattered importlib, sys.modules mutation.
- The absorbing spelling is class-free: a parallel dataclass, per-mode model, or mini hierarchy minted beside an owner is the deleted form; the absorbed case is a family case, vocabulary row, or fold; one-public-method classes collapse to module functions.
- Import placement is judgment the gates cannot see: eager-hot at module scope, cold/heavy/native deferred via `lazy import` at module scope — never flag `lazy import` as misplaced or unused.
- Cross-cutting capability is one signature-preserving, rail-preserving decorator with deterministic stack order; a recurring wrapper stack collapses into one parameterized aspect factory at the second occurrence.
- Concurrency is AnyIO-structured; cancellation is never railed into `Result`; blocking work lane-routes with explicit capacity; domain packages mint zero CapacityLimiters — capacity is runtime-owned.
- Numeric routes admit operands once into one operand owner with total projections; structure, tuning, and mode enter as policy values; backend termination codes fold into one typed status vocabulary; naive routes — matrix inversion, determinant gates, raised non-convergence — are defects.

## TypeScript

- Declarations are authored unexported; the file closes at one terminal EXPORTS block that declares the surface, never widens it; no `export` keyword on body declarations; no barrels; modules expose one or two exports with underscore-prefixed interiors.
- The Schema owner is the single shape authority: static types derive from it, wire twins via transform plus field encodings, variants via pick/omit/extend. A hand interface, alias, or DTO beside a Schema owner is a parallel-shape defect on sight.
- One contract-checked as-const vocabulary anchors each keyed domain, rows carrying behavior, the union derived from the keys; a `Match` or `switch` enumerating a domain a vocabulary row already keys is a keyed-switch restatement — keyed domains dispatch through lookup, `Match` owns structural and predicate dispatch only.
- `pipe` owns the linear chain where each step consumes only its immediate predecessor; `Effect.gen` takes over at the second live binding; a combinator body opening another combinator is the deleted pyramid; conditional arms are terminal.
- Retry, timeout, span, and annotation policy attaches at the `Effect.fn` declaration tail, recoverable from the declaration; policy pushed into the body, or admission hoisted above the decode seam, is buried policy.
- Every dependency is a Tag satisfied by the Layer graph; the service IS one `Effect.Service` class owner — never flag that class as OO drift; the defects are ad hoc class hierarchies and hand interface-plus-Tag-plus-Layer triples beside it. Substitution is Layer provision, never mock frameworks or module patches; requirement elimination to `never` at the composition root is the wiring proof; components consume the graph only through the sanctioned bridge, never running effects or owning Layers.
- `catchTag`/`catchTags` over blanket `catchAll`; discard only as a ruled outcome. TS2589/2590 are architecture pressure on the derivation — repair is value-level; a suppression directive adding ceremony without correctness is itself the defect.
- Branch topology is wave-ordered acyclic: core, then security, data, runtime, ui; iac imports only core and data; port satisfaction happens at app composition, never via upward imports. C# owns every wire shape — TS decodes verbatim through the interchange codec and owns no geometry or IFC semantics; re-minting a wire shape TS-side is a defect.

## Planning corpus (libs/\*\* and .planning/)

- The fence is the work product; the measure of a planning item is the diff to a code fence, never the prose around it.
- An external member appears as settled fence code only when the folder's `.api/` catalogue verifies its spelling; otherwise it is a marked RESEARCH item, never prose and never fence code; a fence contradicting a sibling RESEARCH item fails.
- Packages lines enumerate exact verified spellings including load-bearing arguments and name deprecated spellings as deleted; Growth lines pair every next-capability with its one-declaration landing spot and its named rejected form — either half missing is a hand-waved contract.
- Seams rows follow `<SourceFile> <glyph> <lang:pkg/subdomain> # [<KIND>]: <shared shape>` with glyphs →/←/⇄ and the closed kind vocabulary owned by libs/.planning/README.md; a new kind is an amendment to that standard, never a fence-local mint; every edge mirrors on both endpoint folders with identical kind; in-package relations are never seams; seams fence lines cap at 160 columns.
- `.planning/` is transient greenfield scaffold: one per package root, sub-domains mirroring the eventual tree; mature folders carry none — the sole exception is a genuinely new sub-domain inside a mature package. A `.planning/` added inside a source sub-folder, or at a mature package root, is misplaced scaffold.
- Codemap nodes name real domain concepts; rail, axis, and lane doctrine jargon never surfaces as an ARCHITECTURE sub-domain name.
- Fence comments obey comment law — rare, 1-2 lines, stating the in-situ invariant the fence cannot show; flag narrative fence comments, never law-bearing ones. This ruling governs where the planning standard's zero-comment line disagrees.
- A planning task naming source-file creation is out of scope and must be removed; implement means deepening a fence plus manifest, `.api/`, and tooling work. Integration points use `page#CLUSTER` notation; standalone cross-reference ledgers are drift channels.

## Tests

- A test falsifies production behavior with an independent oracle: grade A independent prediction, B metamorphic or model relation, C durable failure-category rail, D shape-only inspection — D stands alone nowhere. Banned shapes: existence tests, mirror tests (asserting a constructed value's own fields, re-implementing the algorithm as its own oracle, same-body snapshots), speculative-state tests, per-function spam.
- Every law family registers a refuting witness the law must fail on; a surviving witness, or one trivially outside the domain, exposes a tautology and the registration is itself the failure.
- The integration lane is reserved for real process/IO boundaries; in-process with doubles is unit regardless of owner span — inflating the lane hides the missing boundary proof.
- One kit per language; suites mirror production trees; TS unit specs colocate beside source and tests/typescript never hosts them; ad hoc helpers beside specs are kit bypass — new kit capability is a row or fold on the existing kit owner, never a sibling API.
- Substitution rides Layer provision or kit doubles; mock frameworks and module patches are kit bypass. A spec asserting only that its own doubles were called proves the wiring it wrote — a mirror test.
- Contracts corpus: C# is the sole producer; Python and TS round-trip read-only; a failed round-trip is a seam defect, never a reason to fork a language-local variant; a manifest entry precedes every directory; DESIGN-PIN rows carry Blocker and no Expectation, REAL rows the inverse; no reserved directories, no placeholder assets.
- Every tools/ operator owns a suite under tests/<lang>/tools/<tool>; a rail change without its spec change in the same diff is an incomplete change.
- A failing law is evidence: investigate the owner before weakening the test. Snapshot mismatches identify producer drift, never reflexive acceptance.

## Tools

- A GritQL pattern encodes one doctrine defect with trigger, predicate, and exemption in comments plus paired FIRES/CLEAN exemplar lines carrying near-miss guards; a pattern without the pair, or overlapping a sibling's territory, is a defect.
- Analyzer rules promote only doctrine-breaking shapes nothing mechanical rejects; rules describe semantic shape, never paths or one-off symbols; the inventory is the code — a prose catalog of rules anywhere is a defect.
- Assay: the stdout Envelope is the sole result contract; Completed (with defects as completed failed runs) stays distinct from Fault (routing, spawn, lease, timeout); every command routes through the registry; the envelope derives exit codes — hand-authored exit semantics are a bypass; remote env stays allowlisted; retry stays narrow, transport and spawn only.
- Bridge wire evolution is additive-only with camelCase and skip-unmapped for compat; vocabulary owners — ranked status enums, fact-key prefixes — never scatter as literals; scenario code uses the SDK vocabulary, never load directives, absolute paths, or direct MCP.

## Manifests

- Central custody: Directory.Packages.props hand-edited, never dotnet-add drift; pyproject.toml with uv; per-package package.json with the committed pnpm lockfile and workspace catalogs. Version movement is forward-only; a downgrade or stale floor without a recorded reason in the diff is a defect.
- A sanctioned pin carries its mechanism inline — a comment stating exactly what breaks without it. A pin without the mechanism comment is a defect; a pin carrying one is never flagged.
- A package claimed in a planning README registry but absent from the owning manifest, or present but unregistered, is registry drift; new admissions owe an `.api/<package>.md` catalogue.

## Durable prose

The review vocabulary is docs/standards/style-guide.md §04; findings cite the defect name and the line:

- [ENUMERATION_ANCHOR] — prose listing or counting enumerable content or the doc's own members; legal quantities are rule thresholds, domain values, or counts whose members are enumerated in the same clause.
- [STALE_MIRROR] — a hand-maintained copy of truth whose system of record is disk, a manifest, or another doc.
- [MECHANISM_LEAK] — index-tier prose carrying spec-tier mechanism; repair is demotion, never deletion.
- [META_FRAME] — the artifact describing itself, its audience, its siblings, or its process; routing surfaces exempt.
- [TWIN_TRUTH] — one fact worded in two tiers; one owner per fact, everything else composes or points.
- [HEDGE] — modal and hedge vocabulary; contract qualifiers (optional, if present, where supported, unless) survive. TODO/FIXME are banned in durable markdown yet sanctioned as source comment markers — never cross-apply.
- [REPORT_FRAME] — provenance, freshness, or verification narration in durable law.
- [CAPABILITY_GATE] — capability adjudicated against consumer presence in either polarity; consumer count is never a design axis.
- [LEGACY_COMPAT] — shapes preserved for predecessors; coexisting arms are peer rows selected by a present environment fact.
- [IMPORTED_POSTURE] — upstream maturity labels, defaults, or fallback posture adopted over the corpus ruling.
- [VERSION_ANCHOR] — pins, version-conditional bands, dates, newest/latest/currently; genuine wire pins stated once at the codec owner.
- [SET_IN_STONE] — sealed/frozen chants; a real contract is stated once at its declaration.
- [WEAK_VERBS] — supports/allows/enables/provides/offers where an owning verb states law.
- [PROCESS_LEDGER] — ship-status markers, decision tags, wave and gate stamps in durable law.
- [ASSERTED_IMPOSSIBILITY] — impossibility claims not naming the enforcing structure in-clause.
- [DELETED_FORM_LITANY] — law stated as an inventory of forbidden alternatives with the positive rule buried; one non-obvious trap survives as one tight positive invariant.
- [COUPLING] — links or citations outside declared routing surfaces; cross-reference and boundaries sections are banned outright.

Structural law riding the same standards: apply the regeneration test — a fact a fresh agent regenerates from disk plus stated invariants (restated topology, member rosters, tool inventories) is a mirror, delete it; facts land at the lowest owning tier and repair is demote-then-collapse, dropped payload is a defect. Tables enumerate, cards legislate: one prose column disqualifies a table; cells stay atomic; bullets cap at two sentences. Table repair preserves the grid — hoist repeats into headers, relieve clause tails into the lead or a note, split overloaded rows; conversion to cards or lists is earned only by rows sharing no comparison question or a type-standard-owned shape, never by cell width. Marker families are closed — heading grammar, token sets, and glyphs per formatting.md; invocation markers only in instruction files; one concept never carries both an invocation marker and a prose modal. Sibling files of one kind (a bundle's references, templates, doc pages) share one structural schema — section vocabulary, card field sets, marker tokens; consistency across the kind outranks local optimization.

Skill bundles (.claude/skills/\*\*) additionally carry skill-craft law: first- or second-person frontmatter descriptions — quoted user-utterance trigger phrases are not voice; over-broad or keyword-stuffed trigger descriptions; SKILL.md over 500 lines or carrying reference banks inline; references that only route to other references; deterministic multi-step procedures narrated in prose where a bundled script belongs; instructed network fetches or global installs inside skill bodies, except an owned install surface naming exact source, scope, and verification.

## Comment discipline

A comment exists only for the in-situ constraint the code cannot show — the why, the invariant, the trap. One line is the target; a short comment inlines onto its statement; two lines is the usual ceiling, and three-plus survives only when truly irreplaceable. Flag: what-comments restating the adjacent code, narration and process residue, comments coupling to paths, sessions, or sibling docs, and multi-line blocks whose payload compresses to one line. Every pass that touches a file prunes its stale or drifted comments in the same pass — comment hygiene is a standing obligation, not a separate cleanup.

## Review priorities

1. Doctrine regressions (rails, dispatch, package custody, topology) outrank style and naming.
2. New public surfaces demand justification against extending an existing owner.
3. Generated or lock content is never review substrate.

## Load-bearing exceptions

Code that violates generic best practice on purpose — do not flag:

- Aggressive API breaks with every call site updated in the same change are the sanctioned rename path, not regressions.
- Dense single-expression bodies and heavy polymorphic dispatch are the bar, not obfuscation.
- Absent defensive guards inside domain logic reflect admission-once boundaries, not missing error handling.
- Sparse 1-2 line agent-facing comments are compliance with comment law, not missing documentation.
- Fences in .planning pages are implementation, not documentation examples — hold them to source standards, never suggest simplifying them into sketches.
- A large file that owns one full concern is sanctioned; file-size budgets do not exist here — never recommend splitting by size, and a statement or branch cap never justifies splitting a closed union or extracting its arms.
- Explicit match and dispatch arms over merged ternaries or combined branches are deliberate; match/case owns dispatch.
- Named intermediates in flow pipelines and explicit error branches keep the ROP rails visible; never suggest inlining them.
- Inline error messages in frozen error constructors are the house shape.
- Bracketed uppercase section dividers (`# --- [LABEL] ---`, `// --- [TYPES] ---`) are source structure, not commented-out code.
- Semantic, non-alphabetical `__all__` and `__slots__` order is deliberate.
- TODO/FIXME as source comment markers are sanctioned; the ban applies only to durable markdown.
- Raising inside `@safe` ROP boundary guards is the admission mechanism, not exception-driven domain flow.
- Specs importing the private-by-design testkit and magic values in specs are sanctioned.
- Rhino-rich host capture coexisting with host-neutral semantics is architecture, never duplication.
- `lazy import` at module scope is the doctrine's deferred lane, never a misplaced or unused import.
- Statement-shaped code inside named kernel exemptions — ref struct folds, span loops, platform-forced seams — is sanctioned when the site names the exemption.
- Planning-corpus closed markers — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` task status and marked RESEARCH items — are that standard's vocabulary, never process-ledger stamps or hedges.
