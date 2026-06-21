export const meta = {
  name: 'scout',
  description: 'Read-only blueprint producer for a planning session: ground the truthful scope, research the real domain + external libraries, synthesize a decision-complete plan, then adversarially critique and harden it. Writes NOTHING to the repo — its return value IS the plan, which the orchestrator presents and (on approval) hands to the execute workflows. args = { scope, mode }: mode "ideate" plans the IDEAS/TASKS to author, mode "implement" plans realizing open cards into deep fences; scope is a target path (e.g. "libs/python/geometry") or empty for all of libs. Scope per folder or per language to avoid one fragile long run.',
  phases: [
    { title: 'Ground', detail: 'read-only truthful scope map — realized capability, open cards + ripples, package surface; no fake depth' },
    { title: 'Research', detail: 'read-only real-domain floor + external-library maximization, web-sourced' },
    { title: 'Synthesize', detail: 'build the decision-complete blueprint from ground + research' },
    { title: 'Critique', detail: 'adversarial critic + redteam on the plan: gaps, scope, counterfactuals, direction' },
    { title: 'Harden', detail: 'merge the critique into the final blueprint' },
  ],
}

const GROUND_SCHEMA = { type: 'object', additionalProperties: false, required: ['findings'], properties: { findings: { type: 'array', items: { type: 'string' } }, open_cards: { type: 'array', items: { type: 'string' } }, ripples: { type: 'array', items: { type: 'string' } }, packages: { type: 'array', items: { type: 'string' } }, notes: { type: 'string' } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['findings'], properties: { findings: { type: 'array', items: { type: 'string' } }, domain_floor: { type: 'array', items: { type: 'string' } }, libraries: { type: 'array', items: { type: 'string' } }, sources: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['title'], properties: { title: { type: 'string' }, url: { type: 'string' } } } }, notes: { type: 'string' } } }
const ITEM = { type: 'object', additionalProperties: false, required: ['kind', 'thesis'], properties: { kind: { type: 'string', enum: ['idea', 'task', 'realize', 'page'] }, slug: { type: 'string' }, thesis: { type: 'string' }, shape: { type: 'string' }, anchors: { type: 'string' }, ripple: { type: 'string' } } }
const BLUEPRINT_SCHEMA = { type: 'object', additionalProperties: false, required: ['scope', 'mode', 'items', 'workflow_plan'], properties: { scope: { type: 'string' }, mode: { type: 'string' }, topology: { type: 'string' }, items: { type: 'array', items: ITEM }, ripples: { type: 'array', items: { type: 'string' } }, workflow_plan: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['step'], properties: { step: { type: 'string' }, detail: { type: 'string' } } } }, assumptions: { type: 'array', items: { type: 'string' } }, open_questions: { type: 'array', items: { type: 'string' } } } }
const CRITIQUE_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict'], properties: { verdict: { type: 'string', enum: ['sound', 'revise'] }, refinements: { type: 'array', items: { type: 'string' } }, gaps: { type: 'array', items: { type: 'string' } }, counterfactuals: { type: 'array', items: { type: 'string' } } } }

// --- [INPUT] -- args = { scope, mode } | "scope" string; mode defaults to ideate ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const SCOPE = (input && typeof input === 'object' && input.scope) ? String(input.scope).trim() : (typeof input === 'string' && input.trim() && input.trim() !== 'ALL') ? input.trim() : 'libs'
const MODE = (input && typeof input === 'object' && input.mode === 'implement') ? 'implement' : 'ideate'

const MODES = {
  ideate: {
    groundFocus: 'the current IDEAS/TASKS pool and which capabilities are ALREADY realized in the design pages versus genuinely deferred',
    researchFocus: 'what the REAL domain needs as genuinely-deferred IDEAS (conceptual/multi-step/higher-order) and TASKS (concrete/focused) BEYOND what is realized — hold the most advanced professional suite in this domain as the FLOOR; maximize external-library leverage (replacements, additions, fuller integration) by quality not quantity',
    blueprintNoun: 'the IDEAS + TASKS to author: each a card thesis with shape/anchors and a Ripple for any cross-folder pair. Enforce the idea-vs-task split (an idea is a sub-domain / larger feature-set / higher-order abstraction; a task is concrete focused work including adding capability to existing files). NEVER plan a test/meta/unblock/create-file card — those are fixed directly on plan-leave',
    execHint: 'on plan-leave, the execute step is the `ideate` workflow scoped per folder (and `rebuild-*` for any page the plan rebuilds), one scope at a time',
  },
  implement: {
    groundFocus: 'the open IDEAS/TASKS to realize and exactly which design pages and code fences each one touches',
    researchFocus: 'how to realize each open task/idea into DEEP code fences at full admitted-package capability, which pages must be rebuilt or deepened, and every cross-folder Ripple the realization triggers (the ripple landings are in scope)',
    blueprintNoun: 'the realize-into-fences execution plan: the open cards to realize, the pages to rebuild/deepen, the ripple landings to author, the phase sequence (bottom-up: per-folder then per-language then cross-libs), and which execute-workflow runs each step',
    execHint: 'on plan-leave, the execute steps are `rebuild-api` then `rebuild-<lang>` per folder, then `align-cards` and `hygiene-sweep` — scoped per folder/language, one at a time, never one long run',
  },
}
const M = MODES[MODE]

const LAW = [
  'Rasm monorepo, planning-stage. CLAUDE.md manifest law governs. READ the governing standards before judging: docs/stacks/csharp/ is the universal density FLOOR for every language; docs/stacks/<lang>/ is the route-owned doctrine for the scope language; libs/.planning/campaign-method.md is the card/page convention; libs/.planning/{architecture,planning-targets,README}.md is the topology. Treat admitted external packages as FIRST-CLASS, preferred over stdlib, mined to full capability via each target .api/ catalog + the central manifest.',
  'READ-ONLY MANDATE: this is a PLANNING pass — you produce a plan, you do NOT edit any repo file. Read, research, map, and reason; return your findings as structured output. The plan is the product; no design page, card, manifest, or catalog is touched here.',
  'IDEA vs TASK: an IDEA is conceptual/multi-step/higher-order (a new sub-domain, a larger feature-set, a higher-order abstraction); a TASK is concrete/focused (adding capability to an existing file, a scoped refactor, admitting + cataloguing a package). A card is ONLY genuinely-deferred work; NEVER a test/meta/unblock/create-file card. Every cross-folder dependency is a Ripple pairing two cards. Quality over quantity; advanced means detailed and high-impact, not merely complex.',
].join('\n')

const groundPrompt = (focus, task) => [LAW, '', 'SCOPE: ' + SCOPE + ' (mode: ' + MODE + '). Establish TRUTHFUL grounding — the targets are shallower than they appear; do not perceive fake depth from a tidy signature. ' + task + ' Focus: ' + focus + '. Read-only; return the structured map.'].join('\n')
const researchPrompt = (task) => [LAW, '', 'SCOPE: ' + SCOPE + ' (mode: ' + MODE + '). ' + task + ' Use WebSearch + WebFetch for current professional-standard and library material (cite real sources). Read-only; return the structured findings.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Ground')
const ground = (await parallel([
  () => agent(groundPrompt('the realized capability of every design page under ' + SCOPE + ' (what already exists, honestly — flag surface-level pages)', 'Map what the scope DELIVERS today by reading its .planning pages + ARCHITECTURE.md + README.md.'), { label: 'ground:realized', phase: 'Ground', model: 'sonnet', schema: GROUND_SCHEMA, effort: 'high' }),
  () => agent(groundPrompt(M.groundFocus, 'Read the IDEAS.md/TASKLOG.md across the corpus and map ' + SCOPE + ' open cards AND every cross-folder Ripple touching the scope (in and out).'), { label: 'ground:cards', phase: 'Ground', model: 'sonnet', schema: GROUND_SCHEMA, effort: 'high' }),
  () => agent(groundPrompt('underutilized packages, missing capability, and replacement/addition candidates', 'Map the package surface of ' + SCOPE + ' by listing its .api/ catalogs and reading the central manifest; identify where admitted packages are under-mined.'), { label: 'ground:packages', phase: 'Ground', model: 'sonnet', schema: GROUND_SCHEMA, effort: 'high' }),
])).filter(Boolean)
log('Ground: ' + ground.length + ' maps')

phase('Research')
const research = (await parallel([
  () => agent(researchPrompt('Research the REAL domain of ' + SCOPE + ': ' + M.researchFocus + '. Hold the most advanced professional software in this domain as the FLOOR, not the ceiling.'), { label: 'research:domain', phase: 'Research', agentType: 'general-purpose', schema: RESEARCH_SCHEMA, effort: 'high' }),
  () => agent(researchPrompt('Research external libraries for ' + SCOPE + ': validate current admissions, find stronger/replacement packages, and identify new admissions that unlock or deepen capability — maximize integration by QUALITY. Cross-check members against the relevant .api/ + manifest.'), { label: 'research:libs', phase: 'Research', agentType: 'general-purpose', schema: RESEARCH_SCHEMA, effort: 'high' }),
])).filter(Boolean)
log('Research: ' + research.length + ' digests')

phase('Synthesize')
const blueprint = await agent([LAW, '', 'TASK: SYNTHESIZE the decision-complete blueprint for ' + SCOPE + ' (mode: ' + MODE + ') from the grounding + research below. The blueprint enumerates ' + M.blueprintNoun + '. Make it exhaustive and unambiguous: every item is decision-complete (no research deferred to execution), each lands at its right target, nothing is truncated. The workflow_plan names the ordered execute steps — ' + M.execHint + '. Surface open_questions the orchestrator must ask the user, and assumptions that change execution.\n\nGROUND:\n' + JSON.stringify(ground, null, 1) + '\n\nRESEARCH:\n' + JSON.stringify(research, null, 1)].join('\n'), { label: 'synthesize', phase: 'Synthesize', schema: BLUEPRINT_SCHEMA, effort: 'max' })
log('Synthesize: ' + ((blueprint && blueprint.items) || []).length + ' items')

phase('Critique')
const critiques = (await parallel([
  () => agent([LAW, '', 'TASK: CONSTRUCTIVE CRITIQUE of this plan — content quality, full scope, ripples identified, external libs not left out, parameterization, no item underscoped. Return refinements (read-only; do not edit).\n\nBLUEPRINT:\n' + JSON.stringify(blueprint, null, 1)].join('\n'), { label: 'critique:content', phase: 'Critique', schema: CRITIQUE_SCHEMA, effort: 'xhigh' }),
  () => agent([LAW, '', 'TASK: ADVERSARIAL RED-TEAM of this plan from every angle — fundamental direction/concept, COUNTERFACTUALS (is there a better approach: models over loose JSON, durable code over generation, full IaC over yaml?), gaps/oversights, naive-vs-advanced framing, pigeonholed parameterization, long-tail and foresight. Attack the direction, not just the details. Return gaps + counterfactuals + refinements (read-only; do not edit).\n\nBLUEPRINT:\n' + JSON.stringify(blueprint, null, 1)].join('\n'), { label: 'critique:redteam', phase: 'Critique', schema: CRITIQUE_SCHEMA, effort: 'xhigh' }),
])).filter(Boolean)
log('Critique: ' + critiques.length + ' passes')

phase('Harden')
const final = await agent([LAW, '', 'TASK: HARDEN the blueprint by merging the critique + red-team findings below into it — absorb every refinement, close every gap, adopt the stronger counterfactual where it wins, drop nothing of value. Return the final decision-complete blueprint in the same shape.\n\nBLUEPRINT:\n' + JSON.stringify(blueprint, null, 1) + '\n\nCRITIQUE:\n' + JSON.stringify(critiques, null, 1)].join('\n'), { label: 'harden', phase: 'Harden', schema: BLUEPRINT_SCHEMA, effort: 'max' })
log('Harden: ' + ((final && final.items) || []).length + ' items; ' + ((final && final.open_questions) || []).length + ' open questions')

return { scope: SCOPE, mode: MODE, blueprint: final, open_questions: (final && final.open_questions) || [] }
