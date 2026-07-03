export const meta = {
  name: 'brief-track-review',
  description: 'Terminal cross-corpus review of one language track of campaign briefs: 3 sequential writing passes (initial holistic, critique, red-team cold read) over ALL the track briefs together. Each pass improves every brief in place — per-doc corrections plus COLLECTIVE work: predecessor-enabled opportunities (capability an earlier folder lands that unlocks a feature in a later folder), seam alignment massaged on both sides (fit, never coupling), gaps/mistakes/silences on shared concerns closed — so the rebuild workflows inherit the cleanest possible guidance. 3 agents, sequential. args = {track, briefs: [...]} — briefs in dependency order; empty = no-op. Run once per track when every brief in it has landed.',
  whenToUse: 'After ALL briefs of a language track exist (py: runtime/data/geometry/compute PLUS artifacts — the consumer plane rides the py track so its rebuild inherits every lower-folder capability; cs: geometry/persistence/compute/apphost/appui/fabrication). Ephemeral: delete after both tracks land.',
  phases: [
    { title: 'Initial', detail: '1 holistic writing pass: read every brief in order, fix per-doc, add predecessor-enabled opportunities, align every shared seam both directions' },
    { title: 'Critique', detail: '1 writing pass: verify the initial edits on disk, mechanical closure checklists per brief, cross-brief consistency floor' },
    { title: 'Redteam', detail: '1 cold re-read of the track as ONE program: adversarial lenses, final verdict + residual risks' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const TRACK = (argsIn && typeof argsIn === 'object' && typeof argsIn.track === 'string' && argsIn.track.trim()) ? argsIn.track.trim() : ''
const BRIEFS = (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.briefs)) ? argsIn.briefs.filter((b) => typeof b === 'string' && b.trim()) : []

// --- [MODELS] ----------------------------------------------------------------------------
const PASS_SCHEMA = { type: 'object', additionalProperties: false, required: ['edits', 'opportunities', 'alignments'], properties: {
  edits: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['brief', 'what'], properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  opportunities: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['enabler', 'consumer', 'what'], properties: { enabler: { type: 'string' }, consumer: { type: 'string' }, what: { type: 'string' } } } },
  alignments: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['seam', 'fix'], properties: { seam: { type: 'string' }, fix: { type: 'string' } } } },
  final_verdict: { type: 'string' }, residual_risks: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo. Track = ' + TRACK + '. THE BRIEFS, in dependency order (earlier = upstream foundation, later = consumer): ' +
  JSON.stringify(BRIEFS) + ' — each a finalized campaign brief a rebuild workflow will execute against its folder. Read EVERY brief COMPLETELY ' +
  'and in order before any edit. You WRITE: improvements land in the briefs in place as owner extensions (a verdict clause, an evidence row, ' +
  'an escalation delta, a package row, a leg obligation — never a parallel section, never a rewrite of settled law). LAWS: (1) FIT, NEVER ' +
  'COUPLED — a later brief composes an earlier folder at declared seams and never re-plans it; an earlier brief never reaches forward except ' +
  'as recorded consumer pressure; (2) ALIGNMENT is bidirectional — a seam one brief names and the counterpart is silent on is a defect fixed ' +
  'on BOTH sides; conflicting spellings/shapes for one seam resolve to one; (3) OPPORTUNITY law — where an earlier brief lands a capability ' +
  'that makes a stronger feature possible in a later folder, the later brief gains the row (with the enabling brief named as the seam), and ' +
  'where a later brief needs something no earlier brief provides, the earlier brief gains the consumer-pressure extension; (4) every claim you ' +
  'add is disk-verified or brief-anchored — no vapor; (5) each brief must remain internally closed after your edits (dispositions complete, ' +
  'partitions acyclic, hedges criteria-bearing); (6) briefs differ in campaign SHAPE — some single-phase (the rebuild engine consumes them ' +
  'directly), some two-phase (an ephemeral design pass consumes them to emit a DECISION; the artifacts brief is the standing example) — every ' +
  'edit preserves the brief\'s own [01] phase contract and numbered verdict/evidence grammar, EXTENDING the numbering (a new Vn/En row) rather ' +
  'than restructuring it; (7) INTEGRATION-FIRST roster audit — re-judge every REMOVE/strike/prune row across the track: zero consumers is an ' +
  'integration mandate, never a removal reason — a removal survives only on proven redundancy (stronger admitted owner named), feed-verified ' +
  'abandonment, or charter/license conflict, and one failing that bar rewrites to a realization row on its owning axis; record cross-folder ' +
  'package enablement (upstream owner extension via the ripple authority, downstream forward-obligation row) so an addition lands ' +
  'consistently across the chain. The goal: the rebuild workflows inherit guidance with zero gaps, zero contradictions, and ' +
  'every cross-folder possibility named.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const passPrompts = [
  [PRE, 'TASK: PASS 1 of 3 — INITIAL HOLISTIC. First full read of the track as one program. Per brief: fix mistakes, close gaps, kill silences ' +
    'on concerns a sibling brief treats as load-bearing. Collectively: walk the dependency chain both directions — enumerate every ' +
    'predecessor-enabled opportunity (what does runtime/geometry landing make possible downstream that no brief names yet?) and every ' +
    'consumer-demand hole (what does a later brief assume upstream that upstream never promises?). Apply every finding in place.'].join('\n'),
  [PRE, 'TASK: PASS 2 of 3 — CRITIQUE. Verify every pass-1 edit against disk and the briefs (an unanchored or wrong edit is repaired, never ' +
    'tolerated). Then the mechanical floor per brief: cross-reference closure (verdict <-> evidence <-> escalation <-> package <-> leg), ' +
    'disposition completeness, seam ledger consistency ACROSS briefs (one seam, one spelling, both sides), upstream-brief references still ' +
    'valid after pass-1 edits. Fix everything you find in place.'].join('\n'),
  [PRE, 'TASK: PASS 3 of 3 — RED-TEAM COLD READ, the last hands on this track. Read the whole track twice as its future implementing agents ' +
    'will: hostile, fresh, lens-by-lens — counterfactual (would faithful execution actually produce world-class folders, or is a brief ' +
    'load-bearing on an unstated assumption?), long-tail (rare-but-real cases no brief covers), boundary (every cross-track and cross-language ' +
    'seam honest?), sprawl (do two briefs quietly plan the same owner twice?), completeness (is any [03] target unreachable from the verdicts ' +
    'as written?). Fix in place; return the track verdict + residual risks.'].join('\n'),
]

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TRACK || !BRIEFS.length) { log('brief-track-review: pass {track, briefs: [...]} (dependency order). No-op.'); return { track: TRACK, briefs: BRIEFS.length, total: 0 } }

const PASS_LABELS = ['initial', 'critique', 'redteam']
const PHASES = ['Initial', 'Critique', 'Redteam']
const passes = []
for (let i = 0; i < passPrompts.length; i++) {
  phase(PHASES[i])
  const p = await agent(passPrompts[i], { label: 'pass:' + PASS_LABELS[i], phase: PHASES[i], effort: i === 0 ? 'xhigh' : 'max', schema: PASS_SCHEMA, stallMs: STALL })
  passes.push(p)
  log('Pass ' + (i + 1) + '/3 (' + PASS_LABELS[i] + '): ' + (p ? (p.edits || []).length + ' edit(s), ' + (p.opportunities || []).length +
    ' opportunit(ies), ' + (p.alignments || []).length + ' alignment(s)' : 'NO RESULT — rerun via resume'))
}
const final = passes[2]

return {
  track: TRACK, briefs: BRIEFS,
  pass_summary: passes.map((p, i) => ({ pass: PASS_LABELS[i], edits: p ? (p.edits || []).length : -1 })),
  opportunities: passes.filter(Boolean).flatMap((p) => p.opportunities || []),
  alignments: passes.filter(Boolean).flatMap((p) => p.alignments || []),
  final_verdict: (final && final.final_verdict) || 'PASS 3 MISSING — rerun',
  residual_risks: (final && final.residual_risks) || [],
}
