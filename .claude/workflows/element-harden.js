export const meta = {
  name: 'element-harden',
  whenToUse: 'After element-architect-build; adversarial hardening of newly-built Rasm.Element design pages, one folder (or explicit page set) at a time.',
  description: 'Rebuild ALL target pages per-file FIRST, then BATCHED critique -> redteam -> sweep (4 pages/agent; sweep adversarially CONFIRMS the prior passes were 100% done and runs the full-libs/ seam ripple), then a TERMINAL lib-wide RESOLVE that fixes EVERY residual any phase surfaced via an adversarial fix->verify loop until dry — no leftovers, no deferral, no scope cap. Stacks BOTH the shared libs/csharp/.api tier AND each folder .api, preserves the section-4-RT architecture invariants. Per ELEMENT-REBUILD-PLAN.md. args = a folder name (Bim), an array of folders, or explicit .planning page paths; empty = all five element folders.',
  phases: [
    { title: 'Discover' },
    { title: 'Rebuild' },
    { title: 'Critique' },
    { title: 'Redteam' },
    { title: 'Sweep' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const BATCH = 4
const STAGGER_MS = 1500
const PLAN = 'ELEMENT-REBUILD-PLAN.md'
const ELEMENT_FOLDERS = ['libs/csharp/Rasm.Element', 'libs/csharp/Rasm.Bim', 'libs/csharp/Rasm.Materials', 'libs/csharp/Rasm.Persistence', 'libs/csharp/Rasm.Compute']

// --- [INPUTS] ----------------------------------------------------------------------------

const parsedArgs = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawArgs = Array.isArray(parsedArgs) ? parsedArgs : (typeof parsedArgs === 'string' ? [parsedArgs] : [])
const cleaned = rawArgs.map((x) => String(x).trim()).filter((x) => x && x.toUpperCase() !== 'ALL')
const isPagePath = (x) => x.indexOf('/.planning/') !== -1 && x.endsWith('.md')
const EXPLICIT_PAGES = cleaned.filter(isPagePath)
const folderNames = cleaned.filter((x) => !isPagePath(x))
const ownerOf = (p) => ELEMENT_FOLDERS.find((r) => p.indexOf(r + '/') === 0)
const FOLDERS = folderNames.length ? ELEMENT_FOLDERS.filter((r) => folderNames.some((w) => r.endsWith(w) || r.endsWith('Rasm.' + w) || r === w))
  : EXPLICIT_PAGES.length ? [...new Set(EXPLICIT_PAGES.map(ownerOf).filter(Boolean))]
  : ELEMENT_FOLDERS

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages', 'folders'],
  properties: { pages: { type: 'array', items: { type: 'string' } }, folders: { type: 'array', items: { type: 'string' } } } }
const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FILE_FIXLOG = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'],
  properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const BATCH_FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'],
  properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['hardened', 'refined', 'clean'] }, extended: { type: 'string' }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm.Element rebuild. READ ' + PLAN + ' (repo root) in FULL FIRST — section 4-RT (RED-TEAM REVISIONS) is AUTHORITATIVE and OVERRIDES section ' +
    '4A-H on conflict. These are DESIGN-PAGE specs (.planning markdown). The NEW architecture (the Rasm.Element seam, the IElementProjection + IGraphConstraint ' +
    'contracts, the NEUTRAL edge algebra, the typed value vocabulary, Marten-as-append-substrate-beneath-the-CRDT-engine) is CANONICAL — harden TO it, never ' +
    'regress it. CLAUDE.md WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; the amended strata ' +
    'adds Rasm.Element as the lowest-AEC sub-stratum).',
  'MANDATORY STANDARDS: docs/stacks/csharp/** is the FLOOR (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, ' +
    'system-apis + the relevant domain/ shard) — meet it then PUSH PAST to the strongest form. EVERY stage (rebuild, critique, redteam, sweep) ACTUALLY READS the ' +
    'core docs/stacks/csharp pages + the relevant domain/ shard(s) for each page it touches and conforms UNIVERSALLY: this is a GENERAL doctrinal harden to the ' +
    'strongest form the doctrine admits (EXPRESSION_SPINE / BOUNDARY_ADMISSION / SHAPE_BUDGET / MODAL_ARITY / OWNER_CHOOSER / RAILS / two-weave ASPECTS / ' +
    'COLLAPSE_SCAN / DERIVED_LOGIC), NOT merely a seam-alignment pass — seam + 4-RT fidelity is necessary but NOT sufficient, the page must independently satisfy ' +
    'the whole doctrine. Cite only members verified via `uv run python -m tools.assay api`.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write; the returned fix-log REPORTS edits already made, never a to-do. Leave nothing ' +
    'behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, redteam, sweep) is HOSTILE: assume the fence is NAIVE/SHALLOW/ILLUSORY until it survives an aggressive attack; ' +
    'the burden of proof is ON THE PAGE. "Mature"/"good enough"/a prior clean verdict are REJECTED. A no-edit verdict is earned ONLY after a genuine attack finds ' +
    'nothing — never a first-read concession, never to avoid work.',
  'ILLUSORY/FAKE CODE is the PRIMARY target — code that USES the doctrine vocabulary ([Union]/[SmartEnum]/[ValueObject]/Fold/the rails) and reads dense yet is ' +
    'HOLLOW: a name/signature/prose promising capability the body lacks; a thin slice of a rich concept (a 2-case union for a 20-case domain); a placeholder/stub ' +
    'dressed as finished; a cited .api/host member never verified (a phantom). Treat dense confident pages with MORE suspicion. Earn every clean verdict.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named laws of docs/stacks/csharp/README.md held as fact: EXPRESSION_SPINE + BOUNDARY_ADMISSION; SHAPE_BUDGET (one concept owns ONE ' +
    'type, variants are cases) + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE; POLICY_VALUES + DERIVED_LOGIC/TYPES + SEMANTIC_NAMING; LIBRARY_DEPTH + ' +
    'DEFINITION_TIME_ASPECTS; ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION; INTERFACE_SEAM.',
  'COLLAPSE MANDATE: collapse >=3 parallel types / sibling factories / repeated switch arms / single-call helpers into ONE polymorphic owner IN THE SAME FILE via ' +
    '[Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case families/Fold algebra/frozen tables — never extract a new file to ' +
    'reduce LOC, never delete capability.',
  'STACK CAPABILITY (CORRECTED — there IS a central tier now): FIRST mine BOTH the SHARED tier `libs/csharp/.api/**` AND the target folder`s own `.api/**` to full ' +
    'depth, layered with the universal Thinktecture/LanguageExt rails. Compose EVERY relevant host API + admitted NuGet member into single dense owners (generated ' +
    'owners, Fold algebra, data tables), not flat per-API uses; use the DEEPEST surface each package reaches; reject thin wrappers + BCL-first reflexes; verify ' +
    'novel members via `uv run python -m tools.assay api`. MAX-STACK BOTH TIERS TO EXHAUSTION: mine the ENTIRE folder `.api/` catalog set — for a large `.api` folder ' +
    '(e.g. Rasm.Persistence carries ~80 catalogs: the event/store/columnar/index/wire/codec/compression/vector/geo/messaging/kms families — Marten, Npgsql/EF, ' +
    'DuckDB, Apache AGE, Arrow/Parquet, DeltaLake, the object-store/MinIO, NodaTime, MessagePack/CBOR/Avro, zstd/lz4/FastCDC, pgvector/Qdrant, H3/pgRouting, ' +
    'TimescaleDB, the schema-registry/messaging set, the KMS set, etc.) READ EVERY relevant catalog, NEVER a token Marten/DuckDB subset — AND the full shared ' +
    '`libs/csharp/.api/` tier. A composed owner that leaves an admitted `.api` capability the page concern ADMITS unexploited is a CAPABILITY-INCOMPLETENESS defect ' +
    'to close in place. (The legacy cs-rebuild workflows wrongly said C# has no central .api tier — it does: libs/csharp/.api/.)',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam): structural collapse and .api-stacking are NECESSARY but NOT SUFFICIENT — a fully-collapsed owner ' +
    'can still model a NAIVE slice of its concept. Close the gap by GROWING the existing owner (a case in the closed family, a row/richer data on the smart-enum, a ' +
    'field/composed value-object, an operation, a policy value) per ROOT_REBUILD + COMPOSED_IMPLEMENTATION — never a parallel surface or a new file. Every extension ' +
    'cites exactly one source: a PACKAGE member, a DOMAIN attribute, or a CONSUMER contract. If the concept is genuinely complete, prove it by adding nothing.',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + .api surface is the real measure, and structural completeness ' +
    'and CAPABILITY completeness are ORTHOGONAL. A SMALL page modeling a rich concept is almost always UNDER-built (give it the deepest sweep); a LARGE, ' +
    'well-collapsed, confident-LOOKING page is the PRIME suspect for a thin slice. Assess each owner against its concept INDEPENDENTLY of size. Concrete gap shapes ' +
    'the corpus repeatedly hides: an element/zone owning a flat id-set where the concept owns geometry, quantities, space boundaries, per-kind attributes (a fire ' +
    'compartment a rating, a thermal zone a setpoint, an MEP system its medium/flow/pressure), adjacency/nesting topology, and coverage/aggregation operations; a ' +
    'profile owning width/height where the concept owns section properties, grade, fabrication + code-check inputs; a durable store owning naive columns where the ' +
    'concept owns constraints, indexes, partitions, RLS, migration, lifecycle. Close each IN PLACE by growing the owner; a "looks finished" verdict without this ' +
    'domain+package+consumer sweep is the rejected concession.',
].join('\n')
const SEAM = [
  'SECTION-4-RT INVARIANTS (the new architecture — preserve, never regress): Relationship is a NEUTRAL edge algebra (Compose|Assign|Associate|Connect|Void + typed ' +
    'payload + Generic passthrough), NOT 17 typed IfcRel cases — the IFC relationship schema lives in Bim`s projector [C5]; PredefinedType is a typed Object field ' +
    'with a Bim egress gate [C6]; the Associate edge carries LayerSet/ProfileSet usage [C7]; MeasureValue uses a Dimension value-object + UnitsNet QuantityType [H2]; ' +
    'ElementGraph carries the incidence index + memoized Bake [H3] with a HAMT working graph vs Frozen read snapshot [H4]; rooted NodeId is a neutral kernel id, IFC ' +
    'GlobalId a Bim attribute [H6]; ONE canonical codec ToCanonicalBytes() on the Node union shared by NodeId-hash + diff [H7]; GeoReference (full tuple) only on ' +
    'Header/Coverage [M1]; RepresentationContentHash keyed map [M2]; TWO seam interfaces — IElementProjection + IGraphConstraint [M3]; Marten is the append substrate ' +
    'BENEATH the preserved op-log/CRDT/time-travel/StructuralMerge engine [H11]; topology is synchronous, AGE/DuckDB are async analytical lanes [C2/H5]. A page that ' +
    'contradicts a 4-RT invariant is a DEFECT to fix toward 4-RT.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely (no weak/erased types); no exception control flow in domain logic (LanguageExt rails/ROP); no imperative branching ' +
    'where a bounded vocabulary/frozen table/generated Switch/Fold owns the variation; no mutable accumulation. Total generated Switch with compile-time ' +
    'exhaustiveness (no runtime-silent _ arm). Typed receipts (no generic IReceipt). Fault = a closed [Union] deriving from Expected.',
  'Latest C# 14 on net10 to the metal (Nullable enable; primary constructors; collection expressions + spread; params collections incl ReadOnlySpan; list/slice/' +
    'relational/logical patterns; required members; file-scoped types; field accessors; extension blocks/operators; generic math / static abstract+virtual interface ' +
    'members; with-expressions; System.Threading.Lock; raw + u8 literals where they fit). Apply the file-organization + section-order law.',
  'BOUNDARY/STRATA: each owner stays on its stratum, depends strictly upward; geometry/mesh/IFC meet at the wire with one owner per runtime; no host-type leak into a ' +
    'host-neutral owner; SEMANTIC_NAMING (one bounded-context term per concept, one word default; no Get/GetMany/GetBy/List/Search families). Each libs/csharp package ' +
    'usable in ISOLATION yet ALIGNED-not-coupled (peers never reference each other; alignment via the Rasm.Element contracts).',
].join('\n')
const PROSE = [
  'PROSE: high-signal design-SPEC prose only. Lead each section with the controlling rule; one idea per paragraph; close on the consequence/boundary. Cut provenance, ' +
    'process narration, freshness disclaimers, report framing, empty hedges. Prose that ASSERTS capability the fence lacks is a defect.',
  'DESIGN-DOC OUTPUT LAW: REAL transcription-complete code fences — ZERO placeholder/stub/TODO, NO page/length cap; densify in place. BACKTICK every symbol/type/' +
    'member/path/package. ("Page craft" is a docs/stacks/csharp doctrine concept only; it does NOT govern these design docs.) COMMENT HYGIENE: keep the canonical ' +
    'section-divider headers; otherwise agent-facing comments only where intent is not obvious — default ZERO; no task/process/history narration.',
].join('\n')
const DOCTRINE = [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', SEAM, '', PATLAW, '', PROSE].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------

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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const base = (p) => p.split('/').pop()
const folderOf = (p) => { for (const r of ELEMENT_FOLDERS) { if (p.indexOf(r + '/') === 0) return r.split('/').pop() } return 'root' }
const inLibs = (p) => typeof p === 'string' && p.startsWith('libs/')
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const batchLabel = (stage, b) => stage + ':' + folderOf(b[0]) + ':' + base(b[0]) + (b.length > 1 ? '+' + (b.length - 1) : '')
const rebuildPrompt = (page) => [DOCTRINE, '', 'TASK: HOSTILE GROUND-UP REBUILD of ' + page + ' to the ULTRA-ADVANCED bar AND domain-complete capability. DISBELIEVE ' +
  'the page — rebuild to the strongest form the doctrine + the section-4-RT invariants admit. Read the page, its sibling pages, ' + PLAN + ' (sections 4/4-RT/5/6), ' +
  'docs/stacks/csharp + the relevant domain/ shard, and BOTH the SHARED libs/csharp/.api/ AND the folder .api/ catalogs it composes; verify every cited member via ' +
  '`uv run python -m tools.assay api`. Collapse parallel shapes into one owner IN THE SAME FILE; close the concept capability gaps in place (each addition citing a ' +
  'package/domain/consumer source); modern C#14/net10, all-backticked high-signal prose, real full code fences. Fix THIS page in place. Report collapsed (before->after) + ' +
  'extended (additions + cited sources); residual_high = each {files,claim} for a CROSS-FILE item only.'].join('\n')
const critiquePrompt = (files) => [DOCTRINE, '', 'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + CAPABILITY-COMPLETENESS + FIX IN PLACE of these ' + files.length + ' already-rebuilt pages:\n' +
  files.map((f) => '- ' + f).join('\n') + '\nYou are an ULTRA-HARSH, UNAGREEABLE auditor AND an IMPLEMENTER — you do NOT merely verify, you REPAIR / COLLAPSE / EXTEND / REBUILD ' +
  'in place exactly as the rebuild pass does. Process EACH page INDEPENDENTLY at FULL per-page depth (batching is for dispersal, never a license to skim — every page gets the ' +
  'complete attack). Assume a violation exists in EVERY fence until you prove otherwise; trust NOTHING the prose or a prior pass claims; "good enough"/"mature"/a prior clean ' +
  'verdict are REJECTED. For each page read it, its sibling pages, the operative docs/stacks/csharp pages + the relevant domain/ shard, ' + PLAN + ' (sections 4/4-RT), and BOTH the ' +
  'SHARED libs/csharp/.api/ tier AND the folder .api/ catalogs + the universal Thinktecture/LanguageExt rails. Run these MECHANICAL checklists line-by-line on EACH page and REPAIR ' +
  'every hit in place (a fix, never a ledger note):',
  '(1) COLLAPSE_SCAN — apply the move for any signal (3+ makes it mandatory): sibling prefix/suffix names -> one modality-polymorphic entrypoint; same return rail differing only by ' +
    'arity -> input-shape discrimination; functions differing only by a literal -> a POLICY_VALUE; a bool/mode/batch param selecting two bodies -> one derived body or policy value; a ' +
    'method calling exactly one other -> delete the hop (ONE_HOP_RESOLUTION); parallel dispatch arms repeating structure -> a Fold algebra or frozen table; several types sharing ' +
    'fields for one concept -> one closed family; a Get/GetMany/GetBy/List/Search family -> one input-keyed polymorphic operation; a wrapper renaming a package API -> the package ' +
    'surface directly; 3+ parallel types / sibling factories / repeated switch arms / single-call helpers -> ONE [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/' +
    'source-generated case family IN THE SAME page.',
  '(2) OWNER_CHOOSER — re-derive EVERY shape from the 5 discriminants (admission, identity regime, variant arity, payload timing, openness), most-specific wins: invariant-bearing ' +
    'scalar -> [ValueObject<TKey>]; N-field one-concept product, no discriminator -> [ComplexValueObject]; wire-keyed vocabulary -> [SmartEnum<TKey>]; process-local-behavior ' +
    'vocabulary -> [SmartEnum] keyless; closed alternatives w/ per-occurrence payload -> [Union]; one value over 2-5 unrelated types -> [Union<T1,...>] ad-hoc; interior product, no ' +
    'invariant -> record/readonly record struct; combinable capability set -> a frozen set; cross-product/external policy key -> a frozen table; foreign wire enum/ABI bits/kernel ' +
    'ordinal -> a language enum AT THE SEAM ONLY. Kill every parallel DTO, one-field wrapper, field-rename shape, nullable-as-failure, struct-default ghost.',
  '(3) KNOB_TEST — delete each parameter; if the value reconstructs what it carried it was a knob -> collapse a bool/mode/strict/batch flag into a policy value or input-shape ' +
    'discriminant; a nullable flag tail -> one Option<ContextRecord>; the single optional form is Option<T> x = default consumed via IfNone(canonical); move every ' +
    'timeout/retry/deadline/CancellationToken OFF the signature onto the carrier or a composition-time effect aspect.',
  '(4) ASPECTS — two-weave: definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via attribute-directed SOURCE GENERATION in the fixed ' +
    'generator-owned order; composition-time concerns attach as effect transformers — retry as Schedule-driven IO.Retry/Prelude.retry, recovery as named catch combinators ' +
    '(@catch/catchOf/CatchM via |), resource lifetime as Bracket/BracketIO/Finally; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. 2-4 co-occurring wrappers ' +
    'collapse into one aspect; an aspect NEVER raises into domain flow; inline-repeated concerns + sibling helper methods are defects.',
  '(5) RAILS — RAIL_CHOOSER, narrowest carrier chosen ONCE at admission (Option absence, Fin sync fallibility, Validation independent accumulated faults, Eff runtime capability, IO ' +
    'deferred boundary work, Schedule retry, Seq/Arr/HashMap immutable traversal); the fault type is a CLOSED [Union] deriving from Expected (a bare exception or generic untyped ' +
    'Error is a defect; recovery via Is/HasCode/IsType<E>, never ==); accumulate-vs-abort correct (Apply/&/.Traverse for independents, Bind/.TraverseM/query for dependents); total ' +
    'generated Switch with compile-time exhaustiveness (NO silent _ arm); .Fold/.Traverse/.Choose with the mandatory .As() re-anchor; NO exception control flow in domain logic, NO ' +
    'mutable accumulation.',
  '(6) STRATA/MEMBERS/MODERN — depend strictly upward (NO downward dependency, NO host-type leak into a host-neutral owner; geometry/mesh/IFC meet at one wire owner per runtime); ' +
    'cite ONLY host/NuGet members verified via `uv run python -m tools.assay api` (a member you cannot verify is a phantom — delete it); latest modern C# 14 on net10; FULL ' +
    'docs/stacks/csharp + the relevant domain/ shard conformance; BOTH the SHARED libs/csharp/.api/ tier AND the folder .api/ maximized to full depth, never a surface-level subset.',
  '(7) CAPABILITY-COMPLETENESS + ILLUSION — structural collapse and capability completeness are ORTHOGONAL: a fully-collapsed owner can STILL model a NAIVE, thin slice (a 2-case ' +
    'union for a 20-case domain; the obvious 3 fields where the concept carries fifteen; a flat id-set where the concept owns geometry/metrics/per-kind-attributes/topology/' +
    'operations). DISBELIEVE the page about its own richness — verify the body actually implements what the names/prose promise. Any capability the .api surface / the real domain ' +
    'concept / a consumer contract admits that the owner OMITS is a DEFECT — close it NOW by GROWING the existing owner (a case/row/field/operation/policy-value, citing its one ' +
    'source), never a parallel surface or a new file. Conversely delete speculative/padding fields, decorative ceremony, and prose asserting capability the fence lacks.',
  '(8) SECTION-4-RT INVARIANTS — every page upholds every 4-RT invariant relevant to it (NEUTRAL edge algebra, not 17 IfcRel cases; PredefinedType typed field + Bim egress gate; ' +
    'MeasureValue = Dimension value-object + UnitsNet QuantityType; incidence index + memoized Bake + HAMT working / Frozen read split; neutral kernel NodeId, IFC GlobalId a Bim ' +
    'attribute; ONE ToCanonicalBytes codec shared by NodeId-hash + diff; TWO seam interfaces IElementProjection + IGraphConstraint; Marten as the append substrate BENEATH the ' +
    'preserved CRDT/time-travel engine; synchronous topology vs async AGE/DuckDB analytical lanes). A page contradicting a 4-RT invariant is a DEFECT to fix toward 4-RT.',
  'Also enforce the file-organization + section-order law, cross-page convention consistency, and prose + comment hygiene. EDIT each page to fix every hit; a no-edit verdict on a ' +
  'page is EARNED only after running ALL 8 checklists + the real domain/package sweep finds nothing — never a first-read concession, never to avoid work. Edit ONLY these ' +
  files.length + ' pages — a fix spanning a file OUTSIDE this set goes to residual_high {files,claim} for the terminal Resolve. Report extended + verdict + aligned + residual_high.'].join('\n')
const redteamPrompt = (files) => [DOCTRINE, '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of these ' + files.length + ' pages — the LAST and MOST AGGRESSIVE pass; an ' +
  'IMPLEMENTER, not a verifier (you REBUILD / COLLAPSE / EXTEND in place):\n' + files.map((f) => '- ' + f).join('\n') + '\nProcess EACH page INDEPENDENTLY at FULL depth (batching ' +
  'never licenses a skim). Assume the author AND critique missed things and the chosen design is naive or illusory until PROVEN the strongest, the burden of proof ON THE DESIGN, ' +
  'never on you; trust nothing the prior passes or the prose claimed. For each page open BOTH the SHARED libs/csharp/.api/ tier AND the folder .api/ + the universal ' +
  'Thinktecture/LanguageExt rails, the sibling pages, docs/stacks/csharp + the relevant domain/ shard, and ' + PLAN + ' (4/4-RT). Attack from every direction and REPAIR every defect ' +
  'in place — no soft-pedalling, no could/should, a fix never a ledger:',
  'PRIMARY LENS: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra (Fold/generated Switch/data table), and the dispatch form categorically the strongest the ' +
    'doctrine admits, or does a denser owner ([Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case family), a data table, or a DEEPER ' +
    'admitted-package primitive (LanguageExt/Thinktecture/MathNet/the .api surface) collapse the whole fence? If a fundamentally stronger design exists, REBUILD to it — never defend ' +
    'the incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next case/dimension/knob/modality/provider arrives, does it land as ONE ' +
    'case/row/policy value with every consumer untouched or broken LOUDLY at compile time (total generated Switch, no silent _)? If it would touch multiple sites, reshape so the ' +
    'growth axis is a case/row/policy value/carrier swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure mode (empty, singular, plural, stream, ' +
    'malformed, concurrent, cancelled, partial-failure, version-skew); is accumulate-vs-abort correct for the REAL boundary; are BOTH ingress AND egress parameterized so the owner ' +
    'sources and sinks across hundreds of consumers without interior edits? (D) STRATA + BOUNDARY-INTEGRITY — a downward dependency, a host-type leak into a host-neutral owner, a ' +
    'concern owned twice in a runtime, geometry/mesh/IFC not meeting at ONE wire owner per runtime, or any coupling to a sibling owner INTERIOR (vs its seam/wire) is a defect: fix ' +
    'it, or record it as a cross-file residual. (E) SURFACE-SPRAWL-IN-TIME + PHANTOMS — an admitted package whose .api or the universal rails expose capability the fence re-derives ' +
    'by hand, flat code below the operator depth the packages reach, a phantom .api/host member (cited but unverifiable — delete it), or a thin wrapper: collapse to package depth ' +
    'and verify the member exists (via assay api). (F) CAPABILITY-COMPLETENESS + ILLUSION — counterfactually attack the owner for DOMAIN-COMPLETENESS independently of how collapsed ' +
    'or confident it looks: does the .api surface, the real-world concept, or a consumer contract admit a capability this owner still OMITS (a flat membership/id set where the ' +
    'concept owns geometry/metrics/per-kind-attributes/topology/operations; a 2-category vocabulary where the domain has twenty; a name/prose promising capability the body lacks)? ' +
    'Name it with a cite and EXTEND THE OWNER IN PLACE (a case/row/field/operation) — a structurally-perfect but capability-sparse or illusory owner is a DEFECT, not a finished ' +
    'page; conversely REJECT any extension that is flat spam, speculative, or a parallel surface. (G) SECTION-4-RT FIDELITY — verify the page upholds every 4-RT invariant relevant ' +
    'to it (neutral edge algebra; PredefinedType + Bim egress gate; MeasureValue Dimension/UnitsNet; incidence index + Bake memo + HAMT/Frozen split; neutral kernel NodeId; one ' +
    'ToCanonicalBytes codec; IElementProjection + IGraphConstraint; Marten beneath the CRDT engine; sync topology vs async lanes); fix toward 4-RT.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (run this EVERY time, NOT only when an architectural restructure is warranted): re-attack EVERY conformance dimension with fresh hostile ' +
    'eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER per shape, the KNOB_TEST per param, the two-weave ASPECT taxonomy, rail + ' +
    'closed-Expected-fault discipline, capability-completeness + illusion per owner, strata correctness, modern C# 14 typing, docs/stacks/csharp + domain-shard conformance, BOTH ' +
    '.api tiers + Thinktecture/LanguageExt maximization, the 4-RT invariants, and prose/comment hygiene — and fix every defect. Even absent a structural rebuild, each page MUST end ' +
    'objectively DENSER, MORE CAPABLE, more correct, and more powerful than the critique left it; if the strongest form is genuinely already present, prove it by finding nothing — ' +
    'but EARN it through the full attack, never invent churn to look busy. Edit ONLY these ' + files.length + ' pages — cross-file items go to residual_high {files,claim} for the ' +
    'terminal Resolve. Report extended + verdict + aligned + residual_high.'].join('\n')
const sweepPrompt = (files) => [DOCTRINE, '', 'TASK: ADVERSARIAL SWEEP + CONFIRM + FULL-LIBS RIPPLE of these ' + files.length + ' pages:\n' + files.map((f) => '- ' + f).join('\n') +
  '\n(1) ADVERSARIAL CONFIRM (this is the LAST per-page gate — a hostile critique that the rebuild/critique/redteam work is 100% DONE): re-read EACH page and attack every fence as if ' +
  'NAIVE/SHALLOW/ILLUSORY, burden of proof ON THE PAGE; FIX in place any hollow/partial/low-quality content, any half-applied collapse, any naive slice of a rich concept (a 2-case ' +
  'union for a 20-case domain), any unverified phantom member, any 4-RT invariant regression. A page is NOT done until it survives this attack — never a first-read concession. ' +
  '(2) DRIFT SWEEP — fix any cross-page incoherence the prior passes left (dangling refs, a renamed owner a sibling still cites, a half-applied rename/collapse). ' +
  '(3) FULL-LIBS RIPPLE — for EVERY cross-folder/cross-stack seam these pages expose, align THIS side of the seam in these pages against the shared shape the ENTIRE libs/ stack ' +
  'uses (the other C# folders: libs/csharp/Rasm kernel, Rasm.AppHost, Rasm.AppUi, Rasm.Fabrication, Rasm.Rhino, Rasm.Grasshopper, the AEC peers; AND the libs/python wire — ' +
  'decode-not-remint): IElementProjection/IGraphConstraint <-> projectors; the GeometryRef/RepresentationContentHash/ContentAddress content-key <-> Persistence <-> Rasm kernel; the ' +
  'typed Material/Property/Assessment/Classification wire vocabulary <-> libs/python decoders. READ the counterpart endpoints to learn the correct shared shape, then fix THESE ' +
  'pages` side + mirror the seam in THESE pages` ARCHITECTURE [02]-[SEAMS]. EDIT ONLY these ' + files.length + ' pages (+ their folder ARCHITECTURE/README) — every counterpart-side ' +
  'edit in another folder goes to residual_high {files,claim} for the TERMINAL RESOLVE to apply lib-wide (a handoff, NEVER a leftover). Return files + verdict + aligned + residual_high.'].join('\n')
const reconcileFix = (cl) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', SEAM, '', PATLAW, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals; NO ' +
  'severity, NO leftovers, NO deferral, NO scope cap (this is the last no-defer pass — nothing leaves unfixed). Blast radius is LIB-WIDE: read EVERY listed file across libs/ (csharp ' +
  '+ py) and FIX the real cross-file defect in place to the STRONGEST doctrine + 4-RT form (unify the shared type/seam/contract, repair strata/boundary, GROW a shared owner spanning ' +
  'files), preserving all capability — a token/rename patch that leaves the seam misaligned is NOT a fix. If a residual is FACTUALLY WRONG, leave it and say why (verify will mark it ' +
  'invalid). If your fix SURFACES a new cross-file need, report it in residual_high {files,claim} so the next round resolves it. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', ADVERSARIAL, '', SEAM, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and, for EACH ' +
  'claim, CONFIRM the fix was ACTUALLY made AND is COMPLETE + HIGH-QUALITY + doctrine/4-RT-conformant, NOT a token or naive patch. ATTACK the fix: is it shallow, partial, a rename ' +
  'that left the defect, or does it leave the cross-file seam still misaligned? Classify each: "fixed" (confirmed real, complete, non-naive), "invalid" (the claim is factually wrong ' +
  '— cite why), or "open" (NOT fixed, OR fixed naively/incompletely/low-quality — must be redone). Default to "open" on ANY doubt; a confident-looking edit that does not truly ' +
  'resolve the cross-file defect is "open", never "fixed". Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Discover')
const EXCLUDED_PAGE_NAMES = new Set(['README.md', 'ARCHITECTURE.md', 'IDEAS.md', 'TASKLOG.md'])
const isAllowedFolderFile = (p) => typeof p === 'string' && FOLDERS.some((root) => p === root || p.startsWith(root + '/'))
const isDesignPage = (p) => isAllowedFolderFile(p) && p.includes('/.planning/') && p.endsWith('.md') && !EXCLUDED_PAGE_NAMES.has(base(p))
const inv = EXPLICIT_PAGES.length ? null : await agent('Resolve the rebuilt Rasm.Element-campaign design pages to harden. For EACH folder root below, find every design page (repo-relative *.md) ' +
  'under `<root>/.planning/**` at ANY depth; EXCLUDE README.md/ARCHITECTURE.md/IDEAS.md/TASKLOG.md. Use find; do not cd; do not edit. Roots:\n' + JSON.stringify(FOLDERS, null, 1) +
  '\nReturn pages (the union of all design pages) + folders (the roots that had pages).', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const pages = [...new Set((EXPLICIT_PAGES.length ? EXPLICIT_PAGES : (inv && inv.pages || [])).filter(isDesignPage))]
log('Discover: ' + (EXPLICIT_PAGES.length ? 'explicit page targeting' : 'folder discovery') + ' -> ' + pages.length + ' page(s) across ' + FOLDERS.length + ' folder(s); CAP=' + CAP + ' BATCH=' + BATCH)
if (!pages.length) { log('No pages — run element-architect-build first, or pass a folder/page subset'); return { folders: FOLDERS, total: 0 } }

// --- [REBUILD]

phase('Rebuild')
const built = (await pool(pages, CAP, (p) => agent(rebuildPrompt(p), { label: 'rebuild:' + folderOf(p) + ':' + base(p), phase: 'Rebuild', schema: FILE_FIXLOG, effort: 'max', stallMs: 300000 }))).filter(Boolean)
log('Rebuild: ' + built.length + '/' + pages.length + ' pages rebuilt')

// --- [CRITIQUE]

phase('Critique')
const batches = chunk(pages, BATCH)
const crit = (await pool(batches, CAP, (b) => agent(critiquePrompt(b), { label: batchLabel('critique', b), phase: 'Critique', schema: BATCH_FIXLOG, effort: 'xhigh', stallMs: 420000 }))).filter(Boolean)

// --- [REDTEAM]

phase('Redteam')
const red = (await pool(batches, CAP, (b) => agent(redteamPrompt(b), { label: batchLabel('redteam', b), phase: 'Redteam', schema: BATCH_FIXLOG, effort: 'max', stallMs: 420000 }))).filter(Boolean)

// --- [SWEEP]

phase('Sweep')
const swept = (await pool(batches, CAP, (b) => agent(sweepPrompt(b), { label: batchLabel('sweep', b), phase: 'Sweep', schema: BATCH_FIXLOG, effort: 'max', stallMs: 420000 }))).filter(Boolean)

// --- [RESOLVE]
// Terminal lib-wide no-defer reconcile: EVERY residual any phase surfaced is fixed + ADVERSARIALLY verified, looped until dry. Nothing dropped.

const norm = (x, page) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [page], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const residualsOf = (rows, keyOf) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, keyOf(r) || pages[0])))
let pending = dedup([
  ...residualsOf(built, (r) => r.file),
  ...residualsOf(crit, (r) => r.files && r.files[0]),
  ...residualsOf(red, (r) => r.files && r.files[0]),
  ...residualsOf(swept, (r) => r.files && r.files[0]),
])
const MAX_ROUNDS = 6
let invalid = []
let round = 0
if (pending.length) {
  phase('Resolve')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide, no-defer, adversarial verify)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: 420000 })
      if (!fix) return { open: cl, invalid: [], surfaced: [] }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: 420000 })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return {
        open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)),
        invalid: cl.filter((r) => bad.has(r.claim)),
        surfaced: (fix.residual_high || []).map((x) => norm(x, (fix.files && fix.files[0]) || pages[0])),
      }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    pending = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim))
  }
  if (pending.length) {
    log('Resolve: ' + pending.length + ' residual(s) open after ' + MAX_ROUNDS + ' rounds -> FINAL FORCE-CLOSE (one lib-wide max pass over ALL remaining, no exceptions)')
    const cl = pending
    const fix = await agent(reconcileFix(cl), { label: 'resolve-force-close', phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: 420000 })
    const verify = fix ? await agent(reconcileVerify(cl, fix.files), { label: 'resolve-force-verify', phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: 420000 }) : null
    const claims = (verify && verify.claims) || []
    const okF = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
    const badF = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
    invalid = dedup([...invalid, ...cl.filter((r) => badF.has(r.claim))])
    pending = cl.filter((r) => !okF.has(r.claim) && !badF.has(r.claim))
  }
  if (pending.length) log('Resolve: ' + pending.length + ' residual(s) STILL OPEN after force-close — HARD BLOCKER (likely architectural), reported LOUDLY, never silently dropped')
  else log('Resolve: ALL residuals fixed + adversarially verified (' + round + ' rounds + force-close)')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  folders: FOLDERS, pages: pages.length, rebuilt: built.length,
  critiqueBatches: crit.length, redteamBatches: red.length, sweepBatches: swept.length,
  resolveRounds: round,
  invalidClaims: invalid.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: pending.map((x) => ({ files: x.files, claim: x.claim })),
}
