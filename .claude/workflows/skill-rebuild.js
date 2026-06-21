export const meta = {
  name: 'skill-rebuild',
  description: 'Research-grounded 11/10 rebuild of the workflow-creator skill. 8 research agents (2x per category, <=4wk validated) feed a 3-stage Architect (build->critique->redteam) that decides the structure. Every durable item (each reference, template, script, example, and SKILL.md) is then built with its own author->critique->redteam, whole-skill-aware. A final whole-skill critique then redteam catch duplication/confusion/sizing, then reconcile + a validator gate. The skill is durable declarative law: NO meta/sourcing/citing/provenance ever enters it; SKILL.md frontmatter is never touched.',
  phases: [
    { title: 'Research' }, { title: 'Architect' }, { title: 'Foundation' }, { title: 'Examples' }, { title: 'Skill' }, { title: 'Whole-Critique' }, { title: 'Whole-Redteam' }, { title: 'Reconcile' }, { title: 'Verify' },
  ],
}

const DIGEST_SCHEMA = { type: 'object', additionalProperties: false, required: ['angle', 'findings'], properties: { angle: { type: 'string' }, findings: { type: 'array', items: { type: 'string' } }, sources: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['title'], properties: { title: { type: 'string' }, url: { type: 'string' }, date: { type: 'string' } } } }, notes: { type: 'string' } } }
const MANIFEST_SCHEMA = { type: 'object', additionalProperties: false, required: ['references', 'templates', 'examples', 'scripts'], properties: { references: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'role'], properties: { path: { type: 'string' }, role: { type: 'string' }, sections: { type: 'array', items: { type: 'string' } } } } }, templates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'shape'], properties: { path: { type: 'string' }, shape: { type: 'string' } } } }, examples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'scenario', 'patterns'], properties: { path: { type: 'string' }, scenario: { type: 'string' }, patterns: { type: 'array', items: { type: 'string' } } } } }, scripts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'action', 'why'], properties: { path: { type: 'string' }, action: { type: 'string', enum: ['keep', 'refine', 'add'] }, why: { type: 'string' } } } }, skill_md_changes: { type: 'array', items: { type: 'string' } }, rationale: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'refined', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pass'], properties: { pass: { type: 'boolean' }, failures: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [HARNESS] -- ramped bounded worker pool ---------------------------------------------
const STAGGER_MS = 1500
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async (slot) => {
    if (slot) await new Promise((res) => setTimeout(res, slot * STAGGER_MS))
    while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) }
  }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, (_, slot) => run(slot)))
  return out
}
const CAP = 10
const SKILL = '.claude/skills/workflow-creator'
const base = (p) => p.split('/').pop()

const LAW = [
  'TARGET: the `workflow-creator` SKILL at ' + SKILL + ' — a Claude Code skill that teaches Claude to author dynamic-Workflow-tool scripts (deterministic JS orchestrating fresh-context subagents). Build the BEST POSSIBLE skill at an 11/10 bar where MORE IS NOT MORE — density, accuracy, and a COMPLETE canonical set with zero bloat. It is a popular, high-rated artifact; every change earns its place.',
  'SKILL-BUILDING DISCIPLINE (not ad-hoc): build skill content the way a top-tier skill is built — SKILL.md is the lean entry that sequences which references to load and when; references carry the deep knowledge; assets/templates are minimal-but-complete skeletons of canonical shapes; assets/examples are full worked builds that stack patterns on the templates; scripts are gates. Every agent understands the WHOLE skill (the manifest below) and its one file role within it. Follow the validated skill-authoring research, not improvisation.',
  'ACCURACY (no guessing): every claim about the Workflow tool matches its REAL, CURRENT behavior, grounded in the official Anthropic dynamic-workflows docs and the validated research. `references/api-reference.md` is the tool MANUAL. Never invent a global, option, cap, or behavior.',
  'META-GUARD (absolute): the skill is durable, declarative LAW. NEVER let meta-commentary, research-origin, sourcing, citing, freshness/version-provenance, "we researched/found", or session/process narration enter ANY skill file. State every rule and shape as fact.',
  'STRUCTURE DISCIPLINE: conform to the Architect manifest exactly — do not invent or split files beyond it. No GOD file (one reference carrying everything) and no MINI-SILOS (many tiny fragments). Patterns are FULLY realized AND concise/appropriately sized — complete coverage, not padding.',
  'SKILL.md: NEVER touch the YAML frontmatter; changes are SURGICAL and highly considered — load/use sequencing so every reference is read in the right order and nothing is missed.',
  'VALIDATE: every template and example is a real, lint-clean workflow that PASSES `node ' + SKILL + '/scripts/validate-workflow.mjs <file>` — run it and fix until clean. EXAMPLES are grounded in THIS monorepo (read README.md + CLAUDE.md + AGENTS.md): code/doc audits, large migrations, multi-language refactors, AEC / Rhino / Grasshopper / geometry, agnostic backend/database/web/ui/ux (TypeScript), varied Python (artifacts, compute, data) — NEVER generic "business" scenarios (CRM, customer feedback, ticket triage). Examples build ON a template and MIX patterns.',
  'FIX-IN-PLACE + WRITE-FULLY: make every change NOW via Edit/Write; the fix-log REPORTS edits already made. Cross-FILE items you cannot fix from your file go in residual (each a {files, claim}); the reconcile phase fixes all. NO severity — every finding is must-address; drop only what is provably wrong.',
].join('\n')

// --- [RESEARCH] -- 2 agents per category, distinct emphasis ------------------------------
const RESEARCH = [
  { key: 'wf-cap-A', prompt: 'Research the CURRENT Claude Code dynamic-workflows tool from PRIMARY sources: WebFetch the official docs (code.claude.com/docs/en/workflows) and the "Introducing dynamic workflows" post (claude.com/blog). Capture the script globals (agent/parallel/pipeline/phase/log/budget/workflow), every agent() option, concurrency cap + burst/ramp, resume/same-session, args, model/effort, determinism limits, the agent caps. GATE the feature to the last 4 weeks; VALIDATE it is the real feature (Claude Code >= v2.1.154), not a pre-feature "workflow" post. Cite URLs + dates.' },
  { key: 'wf-cap-B', prompt: 'Research the CURRENT dynamic-workflows tool from SECONDARY/ADVANCED sources: recent changelog/release notes and advanced practitioner guides. Focus on the NUANCES and gotchas a naive skill omits — the ramp-vs-burst lesson, resume cache keying, args serialization, effort/model tiering, what trips the agent caps. GATE to the last 4 weeks; VALIDATE real-feature only. Cite URLs + dates.' },
  { key: 'oss-A', prompt: 'Find the highest-quality OSS Claude Code skills that author dynamic workflows (the NEW feature) on GitHub. Extract how the best ones structure SKILL.md + references + assets and how they teach pattern selection. GATE to the last 4 weeks; VALIDATE the project targets the real dynamic-workflows feature, not a pre-feature orchestration hack. Cite repos/URLs + recency.' },
  { key: 'oss-B', prompt: 'Find advanced real-world dynamic-workflow SCRIPTS and example libraries (the new feature) and extract concrete authoring techniques: schema design, pattern mixing, validation, and what their examples/templates look like. GATE to the last 4 weeks; VALIDATE real-feature. Cite repos/URLs + recency.' },
  { key: 'patterns-A', prompt: 'Catalog the CANONICAL workflow patterns as concrete shapes over the primitives (parallel=barrier, pipeline=streaming, plain-JS loop, bounded pool, agent): Anthropic "Building Effective Agents" five — prompt-chaining ~ pipeline, parallelization ~ parallel, routing, orchestrator-workers, evaluator-optimizer. For each: when it fits, the primitive(s) it uses, the failure mode it guards. Map the (older) taxonomy onto the CURRENT tool. Cite sources.' },
  { key: 'patterns-B', prompt: 'Catalog the field-standard patterns BEYOND Anthropic\'s five, as concrete shapes over the primitives: judge panel, skeptic-vote / adversarial verify, reflection, loop-until-dry, fan-out-then-reconcile, map-reduce-over-agents. For each: when it fits, primitives used, failure mode guarded, and how it differs from the closest Anthropic pattern. Cite sources.' },
  { key: 'skill-A', prompt: 'Research current Anthropic guidance on AUTHORING a Claude Code skill (skill-creator / agent-skills docs): how SKILL.md frontmatter + body should be structured, how to sequence instruction and reference loading, what makes a skill discover and trigger well. GATE current docs to the last 4 weeks where possible. Cite sources.' },
  { key: 'skill-B', prompt: 'Research skill-authoring STRUCTURE best practices from high-quality modern skills: WHEN to split a reference into multiple files vs keep one (the sizing judgment that avoids both a god file and mini-silos), how to organize assets (templates/examples/scripts), and how the strongest skills keep SKILL.md lean while references carry depth. Cite sources + recency.' },
]

// --- [PROMPT BUILDERS] -------------------------------------------------------------------
const architectBuild = (digests) => [LAW, '', 'RESEARCH DIGESTS:\n' + JSON.stringify(digests, null, 1), '', 'TASK (read-only — produce the PLAN, edit nothing): read the entire current skill (SKILL.md + references/* + assets/templates/* + assets/examples/* + scripts/*) under ' + SKILL + ', and read README.md + CLAUDE.md + AGENTS.md for the monorepo domains. DECIDE the structure as a manifest: (references) default `api-reference.md` (tool manual) + `primitives.md` (foundational combinators) + `patterns.md` (composed canonical approaches incl. the currently-MISSING routing / orchestrator-workers / evaluator-optimizer / reflection) — split one further ONLY to avoid a god file, never into mini-silos (give the sizing rationale); (templates) the full logical set of canonical workflow TYPES as minimal-but-complete skeletons; (examples) monorepo-relevant scenarios that each build on a template and MIX patterns, spanning the full pattern set, DROPPING business scenarios (the current customer-feedback example is out of scope); (scripts) keep `validate-workflow.mjs`, mark `refine`/`add` only with concrete 11/10 justification; (skill_md_changes) the SURGICAL SKILL.md sequencing edits, frontmatter NEVER touched. Return the manifest + rationale.'].join('\n')
const architectCrit = (manifest, digests) => [LAW, '', 'RESEARCH:\n' + JSON.stringify(digests, null, 1), '', 'DRAFT MANIFEST:\n' + JSON.stringify(manifest, null, 1), '', 'TASK (read-only): CONSTRUCTIVELY CRITIQUE this structural plan and return an IMPROVED manifest (same schema). Is the reference split optimal (no god file, no mini-silos, right sizing)? Is the canonical pattern set COMPLETE (routing, orchestrator-workers, evaluator-optimizer, reflection present) yet concise? Is the template set the full logical canonical set without mini/god shapes? Are the examples monorepo-grounded, pattern-mixing, business-free, and each tied to a template? Are scripts justified? Are the SKILL.md changes surgical/sequencing-only? Tighten and complete the manifest; return it.'].join('\n')
const architectRedteam = (manifest, digests) => [LAW, '', 'RESEARCH:\n' + JSON.stringify(digests, null, 1), '', 'CRITIQUED MANIFEST:\n' + JSON.stringify(manifest, null, 1), '', 'TASK (read-only): ADVERSARIAL RED-TEAM this plan and return the HARDENED final manifest (same schema). Hunt: a missing or half-realized canonical pattern; a god file or mini-silo; a business-flavored or template-less example; an unjustified script or a real gap a new script should close; any SKILL.md change that would touch frontmatter or is non-surgical; bloat or padding presented as completeness; any structure that invites meta-leakage. Fix every flaw in the manifest; return the final plan.'].join('\n')

const refAuthor = (item, M, digests) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'RESEARCH:\n' + JSON.stringify(digests, null, 1), '', 'TASK: author/refine the reference ' + item.path + ' to its role (' + item.role + ') at the 11/10 bar — accurate to the current tool, complete for its scope, dense, no meta. `primitives.md`: the foundational combinators (parallel=barrier vs pipeline=streaming, the ramped bounded pool, agent, plain-JS loop, phase=display-only, budget, nesting) — what each IS and when. `patterns.md`: the complete canonical approach set incl. routing / orchestrator-workers / evaluator-optimizer / reflection, each concise. `api-reference.md`: the tool manual with the real nuances (cap, burst-vs-ramp, resume, args, model/effort, determinism). Edit in place. Return the fix-log.'].join('\n')
const tplAuthor = (item, M) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'TASK: author the template ' + item.path + ' — a minimal-but-complete runnable skeleton of the canonical shape "' + item.shape + '". Structural bones only (TODOs for domain detail), embodies the references doctrine, passes the validator. Edit in place. Return the fix-log.'].join('\n')
const scrAuthor = (item, M) => [LAW, '', 'TASK: ' + item.action + ' the script ' + item.path + ' (' + item.why + '). Preserve `validate-workflow.mjs`\'s proven checks; refine/add ONLY with a concrete, justified 11/10 improvement (catch a real authoring mistake) — otherwise verdict=clean and change nothing. Edit in place. Return the fix-log.'].join('\n')
const exAuthor = (item, M) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'TASK: author the example ' + item.path + ' — a real, powerful, runnable workflow for the monorepo scenario "' + item.scenario + '" demonstrating [' + (item.patterns || []).join(', ') + '], built ON the matching template and mixing references concepts. Read README.md/CLAUDE.md/AGENTS.md to ground it in a real monorepo domain (NO business). RUN `node ' + SKILL + '/scripts/validate-workflow.mjs ' + item.path + '` and fix until it passes. Edit in place. Return the fix-log.'].join('\n')
const skillAuthor = (M) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'TASK: apply ONLY the surgical SKILL.md sequencing changes (' + (M.skill_md_changes || []).join('; ') + '). Touch NOTHING in the YAML frontmatter. Ensure every reference in the final set (' + (M.references || []).map((r) => r.path).join(', ') + ') is loaded in the right order at the right step and the template/example index is accurate. Minimal, highly considered, no rewrite of sound prose. Edit in place. Return the fix-log.'].join('\n')
const authorFor = (item, M, digests) => item.kind === 'reference' ? refAuthor(item, M, digests) : item.kind === 'template' ? tplAuthor(item, M) : item.kind === 'script' ? scrAuthor(item, M) : item.kind === 'example' ? exAuthor(item, M) : skillAuthor(M)
const authorEffort = (item) => item.kind === 'script' ? 'high' : item.kind === 'skill' ? 'xhigh' : 'max'
const KINDNOTE = { reference: 'a reference (deep knowledge, accurate, no meta)', template: 'a template (minimal-but-complete skeleton, passes the validator)', script: 'a script (proven checks preserved; changes justified)', example: 'an example (monorepo-grounded, pattern-mixing, passes the validator)', skill: 'SKILL.md (NEVER touch frontmatter; surgical sequencing only)' }
const itemCrit = (item, M) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'TASK: CONSTRUCTIVE CRITIQUE + FIX IN PLACE of ' + item.path + ' — ' + KINDNOTE[item.kind] + '. Aware of the WHOLE skill (manifest). Is it the densest, most accurate, most complete-yet-concise form at the 11/10 bar? Correct any drift from the real tool, fill a genuine gap, cut bloat. For a template/example run the validator. Report cross-file items in residual. Edit in place. Return the fix-log.'].join('\n')
const itemRedteam = (item, M) => [LAW, '', 'MANIFEST:\n' + JSON.stringify(M, null, 1), '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE of ' + item.path + ' — ' + KINDNOTE[item.kind] + '. HOSTILE hunt+repair: meta-leakage (sourcing/citing/provenance/process), any claim not matching the real current tool, bloat or a god/mini shape, a business example, a template/example that fails the validator (run it), touched SKILL.md frontmatter, a missing or half-realized pattern. Repair in place; cross-file items to residual. Return the fix-log.'].join('\n')

const buildItem = async (item, M, digests, ph) => {
  const logs = {}
  const stages = [
    { key: 'author', prompt: authorFor(item, M, digests), effort: authorEffort(item) },
    { key: 'crit', prompt: itemCrit(item, M), effort: 'xhigh' },
    { key: 'redteam', prompt: itemRedteam(item, M), effort: 'max' },
  ]
  for (const st of stages) {
    const r = await agent(st.prompt, { label: st.key + ':' + base(item.path), phase: ph, schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 300000 })
    if (r === null) break
    logs[st.key] = r
  }
  return { path: item.path, logs }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Research')
const digests = (await parallel(RESEARCH.map((r) => () => agent([LAW, '', r.prompt].join('\n'), { label: 'research:' + r.key, phase: 'Research', agentType: 'general-purpose', schema: DIGEST_SCHEMA, effort: 'high' })))).filter(Boolean)
log('Research: ' + digests.length + '/8 digests')

phase('Architect')
const m0 = await agent(architectBuild(digests), { label: 'architect:build', phase: 'Architect', schema: MANIFEST_SCHEMA, effort: 'max' })
const m1 = await agent(architectCrit(m0 || {}, digests), { label: 'architect:critique', phase: 'Architect', schema: MANIFEST_SCHEMA, effort: 'xhigh' })
const M = (await agent(architectRedteam(m1 || m0 || {}, digests), { label: 'architect:redteam', phase: 'Architect', schema: MANIFEST_SCHEMA, effort: 'max' })) || m1 || m0 || { references: [], templates: [], examples: [], scripts: [] }
log('Architect: ' + (M.references || []).length + ' refs, ' + (M.templates || []).length + ' templates, ' + (M.examples || []).length + ' examples, ' + (M.scripts || []).length + ' scripts')

const foundation = [...(M.references || []).map((x) => ({ ...x, kind: 'reference' })), ...(M.templates || []).map((x) => ({ ...x, kind: 'template' })), ...(M.scripts || []).map((x) => ({ ...x, kind: 'script' }))]
const examples = (M.examples || []).map((x) => ({ ...x, kind: 'example' }))
const skillItem = { path: SKILL + '/SKILL.md', kind: 'skill' }

phase('Foundation')
const fBuilt = (await pool(foundation, CAP, (it) => buildItem(it, M, digests, 'Foundation'))).filter(Boolean)
phase('Examples')
const eBuilt = (await pool(examples, CAP, (it) => buildItem(it, M, digests, 'Examples'))).filter(Boolean)
phase('Skill')
const sBuilt = await buildItem(skillItem, M, digests, 'Skill')

const built = [...fBuilt, ...eBuilt, sBuilt]
const FILES = [skillItem.path, ...(M.references || []).map((r) => r.path), ...(M.templates || []).map((t) => t.path), ...(M.examples || []).map((e) => e.path), ...(M.scripts || []).map((s) => s.path)]
const filesList = JSON.stringify(FILES)

phase('Whole-Critique')
const wc = await agent([LAW, '', 'TASK: WHOLE-SKILL CONSTRUCTIVE CRITIQUE + FIX IN PLACE. Read the ENTIRE skill (' + filesList + ') as one body. Fix: duplication or contradictory info across files, confusing or out-of-order instructions, a reference/template/example that is weak or off-doctrine, a canonical pattern that is missing OR over-grown (fully realized AND concise — no padding). Ensure SKILL.md sequences every reference and frontmatter is untouched. Edit in place; cross-file items to residual. Return the fix-log.'].join('\n'), { label: 'whole:critique', phase: 'Whole-Critique', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
phase('Whole-Redteam')
const wr = await agent([LAW, '', 'TASK: WHOLE-SKILL ADVERSARIAL RED-TEAM + FIX IN PLACE. Read the ENTIRE skill (' + filesList + ') as one body and attack it: any META-LEAKAGE anywhere; duplication or confusion between files; a poor/redundant template or example; a reference that is a god file or a mini-silo; a pattern half-realized or bloated; a template/example that fails the validator (run them); business scenarios; touched SKILL.md frontmatter. Repair every defect in place; only genuine cross-file items to residual. Return the fix-log.'].join('\n'), { label: 'whole:redteam', phase: 'Whole-Redteam', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })

// --- [RECONCILE] -------------------------------------------------------------------------
const RFIX = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RVER = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const allRes = []
for (const b of built) for (const st of ['author', 'crit', 'redteam']) { const l = b.logs && b.logs[st]; if (l && l.residual) for (const x of l.residual) allRes.push({ files: x.files && x.files.length ? x.files : [b.path], claim: x.claim }) }
for (const r of [wc, wr]) if (r && r.residual) for (const x of r.residual) allRes.push({ files: x.files && x.files.length ? x.files : FILES, claim: x.claim })
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map(); for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
let hard = []
if (clusters.length) {
  phase('Reconcile')
  const out = (await pipeline(clusters,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-file residuals across the skill. Read every listed file and FIX each in place (no severity; if a claim is provably wrong, leave it and say why). Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RFIX, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: VERIFY each residual per claim — read the files; status "fixed"/"invalid"/"open" (default open on doubt). Claims:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RVER, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ v })) : null,
  )).filter(Boolean)
  hard = out.flatMap((o) => ((o.v && o.v.claims) || []).filter((c) => c.status === 'open').map((c) => c.claim))
}

phase('Verify')
const verify = await agent([LAW, '', 'TASK (final gate): run `node ' + SKILL + '/scripts/validate-workflow.mjs <file>` on EVERY template and example under ' + SKILL + '/assets/, and re-read SKILL.md + every reference. Confirm: all templates/examples PASS; SKILL.md frontmatter unchanged and its sequencing names every reference; no meta/sourcing anywhere; no god file or mini-silo; no duplication; the canonical pattern set complete and concise. FIX any failure/violation you can in place; report what remains in failures. Return pass + failures.'].join('\n'), { label: 'verify-gate', phase: 'Verify', schema: VERIFY_SCHEMA, effort: 'high', stallMs: 300000 })
log('Verify: pass=' + (verify && verify.pass) + '; hard_residual=' + hard.length)

return { manifest: M, files: FILES.length, hard_residual: hard, verify }
