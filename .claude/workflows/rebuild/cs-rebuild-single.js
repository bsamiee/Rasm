export const meta = {
  name: 'cs-rebuild-single',
  whenToUse: 'Hostile ground-up rebuild of one C# design-page folder to the csharp doctrine bar.',
  description: 'Hostile ground-up rebuild of libs/csharp design pages to world-class modern C# (C#14/net10, strata-correct, the 16 named docs/stacks/csharp laws, [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] ADT collapse, LanguageExt Fin/Validation/Option/Eff rails, two-weave AOP, source-generated owners) AND justified IN-PLACE capability extension. Per design page, 1 agent per file in a 3-step ADVERSARIAL pipeline — rebuild(max) -> critique(xhigh) -> redteam(max), every stage hostile: assume the fence is naive/junior/illusory until it survives attack, never accept "mature", hunt the fake/decorative code that reads advanced but is hollow, collapse + maximize the .api, AND close the concept capability gaps by growing the existing owner in place. Then a cross-file reconcile. Untied to any idea/task: it improves every page objectively. args = optional package scope (e.g. "Rasm.Bim"); empty/"ALL" = all of libs/csharp.',
  phases: [
    { title: 'Discover', detail: 'list every design page under the target (recursive .planning specs)' },
    { title: 'Rebuild', detail: 'per page (1 agent/file): rebuild(max) -> critique(xhigh, 6-checklist + capability-completeness) -> redteam(max, counterfactual + cold re-review), every stage ADVERSARIAL (naive/illusory-by-default), pooled at CAP=11' },
    { title: 'Reconcile', detail: 'consume cross-file residuals: union-find cluster by shared file -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals' },
    { title: 'Final-Collapse', detail: 'one series of agents over the whole scope (collapse(max) -> critique(xhigh) -> redteam(max)): collapse + unify across files into one rail/polymorphism/shape system, break illusory differentiation, reduce chaff without removing capability' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const ROOT = 'libs/csharp'

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SCOPE = (!rawScope || rawScope === 'ALL') ? '' : rawScope
const SWEEP = !SCOPE ? ROOT : (SCOPE === ROOT || SCOPE.indexOf(ROOT + '/') === 0) ? SCOPE : ROOT + '/' + SCOPE

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages'], properties: { pages: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
// --- [FINAL-COLLAPSE] -- wider-scope rebuild over the WHOLE in-scope corpus (one series) ---
const COLLAPSE_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['collapsed', 'clean'] }, collapsed: { type: 'array', items: { type: 'string' } }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; a host-neutral owner only where a non-Rhino runtime ' +
    'consumes the contract). C# PLANNING-HOMING: under `libs/csharp/Rasm` the active planning effort is `Rasm/Geometry` — its design pages live at ' +
    '`libs/csharp/Rasm/Geometry/.planning/**` while its governing `ARCHITECTURE.md`/`IDEAS.md`/`TASKLOG.md`/`README.md` and `.api/` catalogs live ' +
    'at the `libs/csharp/Rasm/` package ROOT (one level UP from `Geometry/`); read those Rasm-root docs + `.api/` as the governing context and ' +
    'shared capability tier for the Geometry pages, and never trample the mature siblings `Analysis`/`Domain`/`Vectors` (not planning targets).',
  'MANDATORY STANDARDS — docs/stacks/csharp/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/csharp/ (README, language, shapes, ' +
    'surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the specialized docs/stacks/csharp/domain/ shard(s) ' +
    'relevant to the page concern (compute, concurrency, data-interchange, diagnostics, durability, interaction, persistence, postgres, ' +
    'resilience, runtime, transport, validation, visuals), then PUSH PAST it to the objectively strongest form the doctrine admits. READ the ' +
    'relevant shard(s) and conform exactly — this is a hard gate enforced by the `tools/cs-analyzer` compiled-doctrine gate (a true positive is ' +
    'architecture pressure, fix the shape; a false positive is rule pressure, never a suppression). Cite only host/NuGet members confirmed via `uv ' +
    'run python -m tools.assay api`; back bridge claims with EvidenceCertificate + reviewed ReferenceEvidence.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE, not a polish pass — and it is UNTIED to any idea/task: it improves the ' +
    'page objectively (collapse surfaces/types/objects, deepen bleeding-edge spellings, maximize admitted-library capability, AND close the ' +
    'concept capability gaps) wherever the doctrine admits a stronger form.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items ' +
    '(report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY ' +
    'until it survives an aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", ' +
    '"polished", and a prior `clean` verdict are REJECTED self-assessments and prior-author claims — MOST of this corpus is naive, surface-level ' +
    'code dressed in the right vocabulary, and it is NOT tolerable. Default to "this fence is naive and must be rebuilt to the strongest form the ' +
    'doctrine admits" and MAKE that rebuild; a no-edit (`clean`/`refined`) verdict is reached ONLY after a genuinely aggressive attack on the real ' +
    'domain + the verified package surface finds nothing — never as a first-read concession, never to avoid work. Reject "good enough" categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine vocabulary ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, ' +
    'confident-looking fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against ' +
    'the real domain and the catalogued package surface. HUNT: a name/signature/prose that PROMISES capability the body does not implement; a ' +
    '"rich" owner that is a thin slice of its concept (a 2-case union for a 20-case domain; the obvious 3 fields where the concept carries ' +
    'fifteen); decorative density, ceremony, and vocabulary carrying no real capability; a placeholder/stub/sketch/`TODO`-in-spirit dressed as a ' +
    'finished design; prose that ASSERTS richness or completeness the fence does not contain; a structurally-correct collapse that is semantically ' +
    'empty; a `.api`/host member cited but never verified (a phantom). Every such illusion is a DEFECT to rebuild, not a feature to preserve. ' +
    'Where you genuinely cannot break the fence, say so by finding nothing — but earn it; never invent churn to look busy either.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the 16 named laws of docs/stacks/csharp/README.md, held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
    'expression-shaped; dependent steps `Bind` monadically, independent ones accumulate applicatively; the carrier, never a flag, selects the ' +
    'algebra; statements survive only in measured `ref struct`/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted EXACTLY ' +
    'ONCE into an evidence-carrying owner; interior never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET (one concept ' +
    'owns ONE type; variants are cases in one closed family) + DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on ' +
    'input shape) + ANTICIPATORY_COLLAPSE (shape the owner for the family it will absorb). [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + ' +
    'DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave ' +
    'new capability into the owner as if always present; no shims/aliases/[Obsolete]/migration layers) + ONE_HOP_RESOLUTION + ' +
    'COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated switch arms / single-call private helpers ' +
    'into ONE polymorphic owner IN THE SAME FILE via `[Union]` / `[Union<T1,...>]` ad-hoc / `[SmartEnum<TKey>]` / `[SmartEnum]` keyless / ' +
    '`[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case families / `Fold` algebra / frozen data tables — never extract a new file ' +
    'to reduce LOC, never delete capability.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE (generated factory + validation partial admits/rejects; one rail ' +
    'bridge lifts the generated outcome into `Fin<T>` / `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the owning ' +
    'boundary only) -> canonical owner -> unified rail -> projection -> egress. Interior code never re-validates, never sees `null`-as-failure, ' +
    'sentinels, or provider shapes; parameterize BOTH ingress AND egress so the same owner sources and sinks across many consumers without ' +
    'interior edits.',
  'STACK CAPABILITY: FIRST mine the package `.api/*.md` catalogs (the curated, integration-shaped capability surface at ' +
    '`libs/csharp/<Package>/.api/`; for the `Rasm/Geometry` effort they live at `libs/csharp/Rasm/.api/`) AND the universal Thinktecture / ' +
    'LanguageExt rails — C# has NO central `.api/` tier, so the universals are Thinktecture (generated domain shape) / LanguageExt (rails, ' +
    'effects, schedules, immutable collections) plus full docs/stacks/csharp doctrine, with MathNet / CSparse owning numeric algorithms. There is ' +
    'NO fixed package count: compose EVERY relevant host API + admitted NuGet package + catalog member into single dense owners woven as ONE rail ' +
    '(source-generated owners, `Fold` algebra, data tables), ALWAYS layering the universal Thinktecture/LanguageExt rails onto the domain ' +
    'packages, NOT flat one-shot per-API uses. Use the DEEPEST operator/combinator/generated surface each package itself reaches (LIBRARY_DEPTH); ' +
    'reject surface-level subsets, BCL-first reflexes, and thin rename wrappers; verify novel members with `uv run python -m tools.assay api`.',
  'PRESERVE all capability (densify, never delete functionality). Where a fence is already dense, deepen; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary/strata law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse and `.api`-stacking are NECESSARY but NOT SUFFICIENT. A page ' +
    'can be fully collapsed into one polymorphic owner and STILL be capability-thin: modeling a NAIVE, LIMITED slice of its domain concept — a ' +
    'flat membership/id set where the concept owns geometry, metrics, per-kind attributes, topology, and operations; a 2-category vocabulary where ' +
    'the domain has twenty; a record with the obvious 3 fields where the concept carries fifteen. Structural completeness and CAPABILITY ' +
    'completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS DOMAIN CONCEPT COMPLETELY. Per ' +
    'COMPOSED_IMPLEMENTATION + the DOCTRINE growth law (capability grows sublinearly; growth lands as cases/rows/policy-values INSIDE existing ' +
    'owners, never new surfaces beside them), every real missing concern lands as a CASE in the existing closed family, a ROW or richer data on ' +
    'the existing smart-enum, a FIELD or a composed `[ValueObject]`/`[ComplexValueObject]` on the existing record, an OPERATION on the existing ' +
    'surface, or a POLICY_VALUE on the existing vocabulary — reshaping the owner per ROOT_REBUILD as if it had always carried it; NEVER a parallel ' +
    'type, a new file, a sibling shape, or flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one — justified, never speculative): (a) PACKAGE — a member the admitted package/host surface ' +
    'exposes that the concept ADMITS but the page IGNORES is a missing case in the owner law (LIBRARY_DEPTH: e.g. an IFC schema gives a zone its ' +
    'quantities, space boundaries, and properties the page never reads — stacking that full surface IS new functionality woven into the owner, not ' +
    'a denser spelling of the same call; verify the member via `uv run python -m tools.assay api`). (b) DOMAIN — an attribute, metric, sub-kind, ' +
    'relationship, state, or operation the REAL concept demands but the page omits (a BIM zone owns its boundary/area/volume, per-kind attributes ' +
    '— a fire compartment a rating, a thermal zone a setpoint, a load group its combinations, an MEP system its medium/flow/pressure — ' +
    'adjacency/nesting topology, and coverage/aggregation/spatial-query operations, not a flat member-id set alone; a profile owns section ' +
    'properties, grade, fabrication + code-check inputs, not width/height; a durable store owns its constraints, indexes, partitions, RLS, ' +
    'migration, and lifecycle, not naive columns). (c) CONSUMER — a contract a sibling or downstream owner will require that has no composed ' +
    'spelling here yet (a need with no spelling marks a missing case: the law extends first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + package surface is the real measure. A SMALL ' +
    'page modeling a rich concept is almost always under-built (give it the DEEPEST sweep), AND a LARGE, well-collapsed page can still be ' +
    'capability-SPARSE (an owner that indexes membership but models none of the concept geometry, metrics, per-kind attributes, topology, or ' +
    'analytics). Assess each owner against its domain independently of size and EXTEND every owner the concept under-realizes IN PLACE — ' +
    'integrated and unified into the one owner at full operator depth, every new field/case/operation composing the existing rails — never a new ' +
    'flat surface beside it.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — never ' +
    'invent capability to look busy or pad with flat fields. Every added case/row/field/operation is load-bearing, cites a package member / domain ' +
    'attribute / consumer contract, and composes the existing rails; preserve ALL existing capability — extension only deepens, never regresses.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types where the language can express the domain; NEVER exception ' +
    'control flow in domain logic (use the LanguageExt typed rails / ROP and the route recovery patterns); NEVER imperative branching where a ' +
    'bounded vocabulary, frozen table, generated `Switch`, match, or `Fold` owns the variation; NEVER mutable accumulation for domain transforms ' +
    '(use immutable folds, projections, collection combinators). Total generated `Switch` with compile-time exhaustiveness (a new case breaks ' +
    'every dispatch site — NEVER a runtime-silent `_` arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when fields carry ' +
    'route/status/sampling/solver/spectral/mesh/extraction/benchmark/host evidence. The fault type is a CLOSED `[Union]` family deriving from ' +
    '`Expected` (a bare exception or a generic untyped `Error` for a multi-cause domain is a defect).',
  'Latest stable C# 14 on `net10.0` to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with spread, ' +
    '`params` collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical pattern matching, switch expressions, `required` ' +
    'members, `file`-scoped types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic math / static ' +
    'abstract+virtual interface members, `with` expressions, `nameof` with unbound generics, `System.Threading.Lock`, raw string + `u8` literals ' +
    'where they fit. Treat analyzer diagnostics as architecture pressure (fix true positives, refine false positives, no ceremony suppressions). ' +
    'Apply the docs/stacks/csharp file-organization and section-order law (`[Union]`/`[SmartEnum]`/`[ValueObject]` and generated case families ' +
    'stay inside the declaring owner block; canonical section order TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> ' +
    'COMPOSITION -> EXPORTS).',
  'Keep conventions IDENTICAL across every package; place each package on its canonical stratum and depend strictly upward; geometry/mesh/IFC meet ' +
    'at the wire with one owner per runtime; never leak a host type into a host-neutral owner. SEMANTIC_NAMING: one canonical bounded-context term ' +
    'per concept (one word default, three the ceiling); arity/filter/provider/modality live in request shape, case, or policy row, never parallel ' +
    '`Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` names; ONE_HOP_RESOLUTION (no alias chains, forwarding helpers, or util shells).',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one ' +
  'owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while ' +
  'densifying; never introduce a downward dependency or leak a host type into a host-neutral owner.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence does not implement is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, method, operator, package ID, path, command, flag, and literal value in backticks. Name the ' +
    'exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no XML-doc bloat. Densify names and types so comments are rarely needed; cut every ' +
  'low-value comment.'
const WIDE = [
  'WIDE-SCOPE REBUILD — this is the FINAL pass over the WHOLE in-scope corpus at once, AFTER every page ran its own per-page rebuild -> critique ' +
    '-> redteam and the cross-file residual reconcile. The per-page passes each saw ONE page; the reconcile fixed cross-file residuals point by ' +
    'point. THIS pass holds EVERY in-scope design page under `' + SWEEP + '` in view SIMULTANEOUSLY and REBUILDS with the bigger view — it is NOT ' +
    'alignment (seam/wire/port bookkeeping is a different concern), it is a wider-scope ground-up rebuild that ONLY the whole-corpus view makes ' +
    'possible. Same hostile, illusory-by-default stance as the per-page passes: assume the corpus is naive, fragmented, or illusory ACROSS files ' +
    'until it survives an aggressive cross-file attack; the burden of proof is ON THE CODE.',
  'COLLAPSE OBJECTIVES (fix every one in place across the spanned pages, PRESERVING ALL capability): (1) LOGIC-HOMING — ALL LOGIC LIVES IN THE ' +
    'RIGHT PLACE: no concern needlessly split across files, no differentiation that should be a SINGLE owner; pull a concern scattered across ' +
    'pages back to its ONE rightful owner per SHAPE_BUDGET + DEEP_SURFACES + ONE_HOP_RESOLUTION. (2) UNIFY EVERYTHING across files — ONE rail ' +
    'family (`Fin`/`Validation`/`Option`/`Eff`), ONE polymorphism approach, ONE canonical set of shapes/owner forms ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case families); collapse parallel / near-duplicate ' +
    'shapes, pipelines, and logic flows that live in DIFFERENT files into ONE unified owner / pipeline / logic flow per ROOT_REBUILD + ' +
    'COMPOSED_IMPLEMENTATION. (3) BREAK ILLUSORY PATTERNS cross-file — be HARSH on bad / illusory differentiation, decorative complexity, and ' +
    'fake-advanced code applied ACROSS files; a dense, confident-looking fence is the PRIME suspect, now hunted across page boundaries, and ' +
    'disbelieved until verified against the real domain + catalogued package surface. (4) REDUCE FOOTPRINT by removing chaff and illusory ' +
    'differentiation — NEVER by removing functionality; preserve every capability, densify it INTO the unified owner. (5) KEEP IMPROVING QUALITY ' +
    'per ALL of docs/stacks/csharp/ — the 16 named laws, COLLAPSE_SCAN, the rail/shape/AOP doctrine, and the relevant domain/ shard(s) — push the ' +
    'whole corpus past the floor to the strongest UNIFIED form the doctrine admits.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()
const authorPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ' +
  'HOSTILE GROUND-UP REBUILD of ' + page + ' to the ULTRA-ADVANCED bar AND to domain-complete capability. DISBELIEVE the page — assume every fence ' +
  'is naive, junior, or illusory until proven world-class; do NOT polish what is there, REBUILD it to the strongest form the doctrine admits, and ' +
  'treat dense confident-looking code as a prime suspect for hollow/decorative complexity. Read the page, its sibling pages (cross-page ' +
  'unification), the docs/stacks/csharp standards PLUS the relevant domain/ shard(s) for this page concern, the package `.api/*.md` catalogs it ' +
  'composes (MINE + STACK them with the universal Thinktecture/LanguageExt rails; for `Rasm/Geometry` pages the catalogs + governing docs are at ' +
  'the `libs/csharp/Rasm/` root), and VERIFY every cited host/NuGet member via `uv run python -m tools.assay api` (a member you cannot verify is a ' +
  'phantom — delete it). Construct in LIFECYCLE order (BOUNDARY_ADMISSION): admit raw EXACTLY ONCE through a generated factory + validation ' +
  'partial -> lift the generated outcome into the canonical owner chosen by the OWNER_CHOOSER discriminants -> weave every cross-cutting concern ' +
  'as a definition-time source-generated aspect or a composition-time effect transformer over a thin pure core -> compose the domain transform ' +
  'through ONE unified rail (`Fin`/`Validation`/`Option`/`Eff`) with total generated `Switch` -> project + egress, with BOTH ingress and egress ' +
  'parameterized. Collapse parallel shapes into one `[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case ' +
  'family IN THE SAME FILE; drive cases with a `Fold` algebra or a frozen data table; one polymorphic entrypoint per modality. BEYOND collapse + ' +
  '`.api` maximization, CLOSE THE CONCEPT CAPABILITY GAPS so the page OWNS ITS DOMAIN CONCEPT COMPLETELY: run your OWN aggressive domain + package ' +
  'sweep, find where the owner models a NAIVE/thin slice of its concept, and extend the EXISTING owner in place (a ' +
  'case/row/field/operation/policy-value per ANTICIPATORY_COLLAPSE + COMPOSED_IMPLEMENTATION + ROOT_REBUILD), each addition citing a package ' +
  'member / domain attribute / consumer contract — never a parallel surface, a new file, or flat spam. Latest modern C# 14 on net10. High-signal ' +
  'prose all-backticked. Fix-in-place (read-then-rebuild, preserve capability). Report what you collapsed (count before->after) in `collapsed` and ' +
  'what capability you extended (each addition + its cited source) in `extended`; verdict is `rebuilt` unless the fence genuinely survived the ' +
  'hostile rebuild untouched. Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} ' +
  'object for any CROSS-FILE item you surface but cannot fix from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const critiquePrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + CAPABILITY-COMPLETENESS + FIX IN PLACE of ' + page + '. You are an ULTRA-HARSH, UNAGREEABLE ' +
    'auditor: assume a violation exists in EVERY fence until you prove otherwise, trust NOTHING the author or the prose claims, and "good ' +
    'enough"/"mature" is rejected outright. Read the page, its sibling pages, the operative docs/stacks/csharp/ pages AND the relevant ' +
    'docs/stacks/csharp/domain/ shard(s) for this page concern, and BOTH the package `.api/*.md` catalogs and the universal ' +
    'Thinktecture/LanguageExt rails. Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note):',
  '(1) COLLAPSE_SCAN — apply the move for any signal (3+ instances makes it mandatory): sibling prefix/suffix names -> one modality-polymorphic ' +
    'entrypoint (MODAL_ARITY); same return rail differing only by arity -> input-shape discrimination; functions differing only by a literal -> ' +
    'parameterize the literal as a POLICY_VALUE; a `bool`/`mode`/`batch` parameter selecting two bodies -> one derived body or one policy value; a ' +
    'method calling exactly one other -> delete the hop (ONE_HOP_RESOLUTION); parallel dispatch arms repeating structure -> a `Fold` algebra or a ' +
    'frozen data table (DERIVED_LOGIC); several types sharing fields for one concept -> one closed family; a ' +
    '`Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` family -> one input-keyed polymorphic operation; a wrapper renaming a package API -> use the ' +
    'package surface directly (LIBRARY_DEPTH); the same 2-4 wrappers recurring -> one parameterized aspect; 3+ parallel types / sibling factories ' +
    '/ repeated switch arms / single-call helpers -> ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / `[ComplexValueObject]` / ' +
    'source-generated case family IN THE SAME FILE.',
  '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, payload timing, ' +
    'openness) and select the OWNER_CHOOSER row, most-specific wins: invariant-bearing scalar -> `[ValueObject<TKey>]`; N-field one-concept ' +
    'product, no discriminator -> `[ComplexValueObject]`; bounded vocabulary with wire-keyed identity -> `[SmartEnum<TKey>]`; bounded vocabulary ' +
    'with process-local behavior -> `[SmartEnum]` keyless; closed alternatives with per-occurrence payload -> `[Union]`; one value over 2-5 ' +
    'unrelated types -> `[Union<T1,...>]` ad-hoc; interior product, no invariant/admission -> `record`/`readonly record struct`; combinable ' +
    'capability set -> a frozen set; cross-product or external policy key -> a frozen table; foreign wire enum / ABI bits / kernel ordinal -> a ' +
    'language `enum` AT THE SEAM ONLY (re-closed at conversion). Kill every parallel DTO, one-field wrapper, field-rename shape, nullable payload ' +
    'bag, enum-dictionary pair, protocol shadow, nullable-as-failure, and struct-`default` ghost.',
  '(3) KNOB_TEST — removal: delete each parameter; if the value reconstructs what it carried, it was a knob -> collapse a ' +
    '`bool`/`mode`/`strict`/`batch` flag into a policy value or input-shape discriminant; a nullable flag tail (`T? a = null, ..., bool x = ' +
    'false`) -> one `Option<ContextRecord>`; the single optional form is `Option<T> x = default` consumed via `IfNone(canonical)`; move every ' +
    '`timeout`/`retry`/`deadline`/`CancellationToken` OFF the signature onto the carrier or a composition-time effect aspect.',
  '(4) ASPECTS — the two-weave taxonomy: definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via ' +
    'attribute-directed SOURCE GENERATION in the fixed generator-owned order; composition-time concerns attach as effect transformers in author ' +
    'order — retry as `Schedule`-driven `IO<T>.Retry(Schedule)`/`Prelude.retry`, recovery as named catch combinators (`@catch`/`catchOf`/`CatchM` ' +
    'composed via `|`), resource lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. ' +
    '2-4 co-occurring wrappers collapse into one aspect; an aspect NEVER raises into domain flow; deterministic stacking order verified. ' +
    'Inline-repeated concerns and sibling helper methods are defects.',
  '(5) RAILS — RAIL_CHOOSER, the narrowest carrier chosen ONCE at admission: `Option<T>` absence, `Fin<T>` synchronous fallibility, ' +
    '`Validation<Error,T>` independent accumulated faults, `Eff<RT,T>` runtime capability, `IO<T>` deferred boundary work, `Schedule` retry ' +
    'policy, `Seq<T>`/`Arr<T>`/`HashMap<K,V>` immutable traversal/lookup; the fault type is a CLOSED `[Union]` family deriving from `Expected` (a ' +
    'bare exception or a generic untyped `Error` for a multi-cause domain is a defect; recovery identity via `Is`/`HasCode`/`IsType<E>`, never ' +
    '`==`); accumulate-vs-abort disposition correct (`Apply`/`&`/`.Traverse` for independents, `Bind`/`.TraverseM`/query expressions for ' +
    'dependents); total generated `Switch` with compile-time exhaustiveness (NO `_` arm hiding a case); `.Fold`/`.Traverse`/`.Choose` traversal ' +
    'with the mandatory `.As()` re-anchor; NO exception control flow in domain logic, NO mutable accumulation.',
  '(6) STRATA/MEMBERS/MODERN — strata correctness (depend strictly upward; NO downward dependency, NO host-type leak into a host-neutral owner; ' +
    'geometry/mesh/IFC meet at the wire with one owner per runtime); cite ONLY host/NuGet members confirmed in the package `.api/` catalog (no ' +
    'phantom member; verify novel members via `uv run python -m tools.assay api`); latest modern C# 14 on net10; FULL docs/stacks/csharp + the ' +
    'relevant domain/ shard conformance (READ the shard); BOTH the package `.api/` catalogs AND the universal Thinktecture/LanguageExt rails ' +
    'maximized, not a surface-level subset; the `tools/cs-analyzer` compiled-doctrine gate clean.',
  '(7) CAPABILITY-COMPLETENESS + ILLUSION — structural collapse and capability completeness are ORTHOGONAL: a fully-collapsed owner can still ' +
    'model a NAIVE, LIMITED slice of its concept, and dense confident code is the prime suspect for hollowness. DISBELIEVE the page about its own ' +
    'richness: verify the body actually implements what the names/prose promise; any capability the admitted-package surface / the real domain ' +
    'concept / a consumer contract admits that the owner OMITS (a flat id/member set where the concept owns ' +
    'geometry/metrics/per-kind-attributes/topology/operations; a 2-category vocabulary where the domain has twenty; the obvious 3 fields where the ' +
    'concept carries fifteen) is a DEFECT — close it in place now by growing the EXISTING owner (case/row/field/operation), citing its source. ' +
    'Reject the inverse: a speculative/padding field, decorative ceremony, or prose asserting capability the fence lacks is deleted.',
  'Also enforce the docs/stacks/csharp file-organization + section-order law, cross-package convention consistency, and prose + comment hygiene. ' +
    'EDIT the page to fix every hit; OVERRIDE any earlier residual you can now resolve. Report what you extended in `extended`. Return the fix-log ' +
    '+ residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} object for any CROSS-FILE item you cannot fix ' +
    'from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const redteamPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + '. You are the LAST and MOST AGGRESSIVE pass: assume the author and critique ' +
    'missed things and that the chosen design is naive or illusory until PROVEN the strongest, with the burden of proof ON THE DESIGN, never on ' +
    'you; trust nothing the prior passes or the prose claimed. Open BOTH the package `.api/` catalogs and the universal Thinktecture/LanguageExt ' +
    'rails, the sibling pages, the operative docs/stacks/csharp/ pages, and the relevant docs/stacks/csharp/domain/ shard. Attack from every ' +
    'direction and REPAIR every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra (`Fold`/generated ' +
    '`Switch`/data table), and the dispatch form categorically the strongest the doctrine admits, or does a denser owner ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family), a data table, or a DEEPER ' +
    'admitted-package primitive (LanguageExt/Thinktecture/MathNet/CSparse) collapse the whole fence? If a fundamentally stronger design exists, ' +
    'rebuild to it — never defend the incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next ' +
    'case/dimension/knob/modality/provider arrives, does it land as ONE case/row/policy value with every consumer untouched or broken LOUDLY at ' +
    'compile time (total generated `Switch`, no silent `_`)? If it would touch multiple sites, reshape so the growth axis is a case, row, policy ' +
    'value, or carrier swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure mode (empty, singular, plural, stream, ' +
    'malformed, concurrent, cancelled, partial-failure, version-skew); is the accumulate-vs-abort disposition correct for the REAL boundary; are ' +
    'BOTH ingress AND egress parameterized so this owner sources and sinks across hundreds of consumers without interior edits? (D) STRATA + ' +
    'BOUNDARY-INTEGRITY — a downward dependency, a host-type leak into a host-neutral owner, a concern owned twice in a runtime, a folder mixing ' +
    'concerns, geometry/mesh/IFC not meeting at ONE wire owner per runtime, or any coupling to a sibling owner\'s INTERIOR (vs its seam/wire) is a ' +
    'defect: fix it, or record it as a cross-file residual. (E) SURFACE-SPRAWL-IN-TIME + PHANTOMS — an admitted package whose `.api` or the ' +
    'universal rails expose capability the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom `.api`/host ' +
    'member (cited but unverifiable — delete it), or a thin wrapper: collapse to package depth and verify the member exists (via `assay api`). (F) ' +
    'CAPABILITY-COMPLETENESS + ILLUSION — counterfactually attack the owner for DOMAIN-COMPLETENESS independently of how collapsed or confident it ' +
    'looks: does the admitted-package surface, the real-world concept, or a consumer contract admit a capability this owner still OMITS (a flat ' +
    'membership/id set where the concept owns geometry/metrics/per-kind-attributes/topology/operations; a 2-category vocabulary where the domain ' +
    'has twenty; the obvious 3 fields where the concept carries fifteen; a name/prose promising capability the body lacks)? Name it with a cite ' +
    'and EXTEND THE OWNER IN PLACE (a case/row/field/operation, per ANTICIPATORY_COLLAPSE) — a structurally-perfect but capability-sparse or ' +
    'illusory owner is a DEFECT, not a finished page. Conversely, REJECT any extension that is flat spam, speculative, or a parallel surface.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (run this every time, NOT only when an architectural restructure is warranted): re-attack every ' +
    'conformance dimension with fresh hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER ' +
    'correctness per shape, the KNOB_TEST per param, the two-weave ASPECT taxonomy, rail + closed-`Expected`-fault discipline, ' +
    'capability-completeness + illusion per owner, strata correctness, modern-C# 14 typing, docs/stacks/csharp + domain-shard conformance, `.api` ' +
    '+ Thinktecture/LanguageExt maximization, the `tools/cs-analyzer` doctrine-gate, and prose/comment hygiene — and fix every defect. Even absent ' +
    'a structural rebuild, the fence must end objectively denser, MORE CAPABLE, more correct, and more powerful than the critique left it; if the ' +
    'strongest form is genuinely already present, prove it by finding nothing — never invent churn.',
  'Hold the highest bar of any stage; reject "good enough"; every defect you raise you REPAIR in place. Report what you extended in `extended`. ' +
    'Return the fix-log + residual_high — each item a {files: [every repo-relative path the cross-file fix spans], claim} object for a CROSS-FILE ' +
    'item you cannot fix from one file (NO severity — every finding counts equally and the reconcile phase addresses all; every single-file fix ' +
    'you make yourself).'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (w, tag) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(w.page), { label: st.key + ':' + folderOf(w.page) + ':' + subOf(w.page), phase: tag + folderOf(w.page), schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 300000 })
    if (r === null) break
    logs[st.key] = r
  }
  return { page: w.page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const finalCollapsePrompt = () => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', WIDE, '',
  'TASK: WIDE-SCOPE COLLAPSE — read EVERY in-scope design page under `' + SWEEP + '` AT ONCE and REBUILD with the whole-corpus view. Hunt the ' +
    'differentiation no per-page pass could see from inside one page: a concern needlessly split across files; parallel / near-duplicate shapes, ' +
    'pipelines, or logic flows living in DIFFERENT files; more than one rail family, polymorphism approach, or canonical owner-shape set surviving ' +
    'across the corpus; and any illusory cross-file differentiation dressed in the doctrine vocabulary. PULL every misplaced concern to its ONE ' +
    'rightful owner; UNIFY the corpus onto ONE rail family, ONE polymorphism approach, ONE canonical set of owner forms; COLLAPSE the cross-file ' +
    'duplicates into ONE unified owner / pipeline / logic flow per ROOT_REBUILD + COMPOSED_IMPLEMENTATION; REDUCE footprint by deleting chaff and ' +
    'illusory differentiation while PRESERVING ALL capability (densify, never delete functionality); and keep PUSHING quality per ALL of ' +
    'docs/stacks/csharp/. Read the spanned pages + the docs/stacks/csharp core + the relevant domain/ shard(s) + the package `.api/` catalogs and ' +
    'the universal Thinktecture/LanguageExt rails; verify any cited host/NuGet member via `uv run python -m tools.assay api`. FIX EVERY defect in ' +
    'place across the spanned pages, regressing none. Return verdict + collapsed (each cross-file collapse/unification made, naming the files + ' +
    'the resulting owner/pipeline) + residual (each {files, claim} for anything you could not fully resolve) + summary.'].join('\n')
const finalCritiquePrompt = () => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', WIDE, '',
  'TASK: HARSHER ADVERSARIAL AUDIT of the wide-scope collapse + FIX IN PLACE. Assume a cross-file defect REMAINS until you prove otherwise; trust ' +
    'NOTHING the collapse pass or the prose claimed. Re-sweep the WHOLE in-scope corpus under `' + SWEEP + '`: a concern still split across files, ' +
    'a shape / pipeline / logic flow still duplicated across pages, more than one rail family or polymorphism approach or canonical owner form ' +
    'still surviving, illusory cross-file differentiation still standing, decorative complexity still passing for capability, or chaff still ' +
    'padding the footprint. REPAIR every hit in place across the spanned pages — unify onto the ONE owner / rail / polymorphism / shape system, ' +
    'preserving ALL capability and regressing no page. Return verdict + collapsed + residual + summary.'].join('\n')
const finalRedteamPrompt = () => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', WIDE, '',
  'TASK: ADVERSARIAL RED-TEAM of the wide-scope collapse — the LAST and MOST AGGRESSIVE whole-corpus pass. Trust nothing the collapse / critique ' +
    'claimed. COUNTERFACTUALLY attack the cross-file ownership: is each shared concept homed on its ONE rightful owner; is the corpus genuinely ' +
    'unified onto ONE rail family, ONE polymorphism approach, ONE canonical shape set; does any PAIR of pages still carry parallel / ' +
    'near-duplicate shapes, pipelines, or logic flows that should be ONE; is any cross-file differentiation illusory / decorative; is any ' +
    'footprint still chaff a denser unified owner would erase without losing capability; will the next cross-file growth axis land as ONE ' +
    'case/row/policy value with every page untouched or broken LOUDLY at compile time? FIX every defect in place across the spanned pages, ' +
    'densifying and preserving ALL capability; if the corpus is genuinely unified, minimal, and capability-complete, prove it by finding nothing — ' +
    'never invent churn. Return verdict + collapsed + residual + summary.'].join('\n')
const COLLAPSE_STAGES = [
  { key: 'collapse', build: finalCollapsePrompt, effort: 'max' },
  { key: 'critique', build: finalCritiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: finalRedteamPrompt, effort: 'max' },
]

// --- [COMPOSITION] -----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}

phase('Discover')
const inv = await agent('List every design page under ' + SWEEP + ' — markdown specs at paths matching */.planning/**/*.md. Return each as a ' +
  'repo-relative path (e.g. ' + ROOT + '/<Package>/.planning/<sub>/<page>.md). Exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md. Use find; do ' +
  'not cd.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const pending = ((inv && inv.pages) || []).filter(Boolean).map((p) => ({ page: p }))
const total = pending.length
log('Discover under ' + SWEEP + ': ' + total + ' design pages; pooling at CAP=' + CAP)

// --- [REBUILD]
phase('Rebuild')
const done = (await pool(pending, CAP, (w) => processPage(w, 'Rebuild-'))).filter(Boolean)

const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Rebuild: ' + done.length + '/' + total + ' pages; reconcile ' + uniq.length + ' residuals (crit+redteam, deduped) -> ' + clusters.length + ' ' +
  'clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', 'TASK: RECONCILE these cross-FILE ' +
      'residuals the critique AND red-team passes deferred. There is NO severity — treat EVERY residual as must-address. Read EVERY listed file. ' +
      'For each: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, repair the strata/boundary issue, or extend ' +
      'the shared owner in place to close a capability gap that spans files), preserving all capability and regressing no file; if a residual is ' +
      'FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real one to avoid work. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent([LAW, '', BOUNDARIES, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and ' +
      'classify each residual: status "fixed" (real defect, now genuinely resolved), "invalid" (the claim is factually wrong / not a real defect — ' +
      'cite why), or "open" (real defect still NOT resolved). Default to "open" on any doubt for a real-looking defect; mark "invalid" ONLY when ' +
      'you can show the claim is wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual -> resolve-residuals), ' + dropped.length + ' ' +
  'dropped as invalid')

// --- [FINAL_COLLAPSE]
phase('Final-Collapse')
const collapseLogs = {}
for (const st of COLLAPSE_STAGES) {
  const r = await agent(st.build(), { label: 'collapse-' + st.key, phase: 'Final-Collapse', schema: COLLAPSE_SCHEMA, effort: st.effort, stallMs: 900000 })
  if (r === null) break
  collapseLogs[st.key] = r
}
const collapsedAll = Object.values(collapseLogs).flatMap((l) => (l && l.collapsed) || [])
const finalResidual = Object.values(collapseLogs).flatMap((l) => (l && l.residual) || [])
log('Final-Collapse: ' + Object.keys(collapseLogs).length + '/3 whole-scope passes; ' + collapsedAll.length + ' collapses, ' + finalResidual.length + ' ' +
  'residual')

return { root: ROOT, scope: SCOPE || 'ALL', complete: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: total, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped, finalCollapsed: collapsedAll, finalResidual: finalResidual }
