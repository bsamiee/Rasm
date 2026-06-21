# Workflow Patterns

Copy-paste orchestration shapes. Each says when to use it, then gives runnable
code. Match the pattern to the Step 2 answers in `SKILL.md`: known list vs
unknown count, one pass vs staged, barrier needed or not.

JSON Schemas are shown abbreviated as `SCHEMA`; define real ones — see the bottom
of this file.

---

## 1. Fan-out then synthesize

**When:** a known list of independent questions/items, one pass each, and you
need one combined answer at the end. The synthesis genuinely needs every result,
so the barrier (`parallel`) is correct here.

```js
export const meta = {
  name: 'research-fanout',
  description: 'Research independent questions in parallel, synthesize one report',
  phases: [{ title: 'Research' }, { title: 'Synthesize' }],
}

// The runtime serializes `args` to a string (a JSON array arrives as text), so parse it.
const input = typeof args === 'string'
  ? (() => { try { return JSON.parse(args) } catch { return args } })()
  : args
const questions = Array.isArray(input) && input.length ? input : ['demo question']

phase('Research')
const findings = await parallel(
  questions.map((q, i) => () =>
    agent(`Research and report verified facts:\n\n${q}`,
          { label: `q${i + 1}`, schema: RESEARCH_SCHEMA }))
)
const clean = findings
  .map((f, i) => (f ? { question: questions[i], ...f } : null))
  .filter(Boolean)

phase('Synthesize')
const report = await agent(
  'Combine the research below into one cohesive briefing; call out disagreements.\n\n'
  + JSON.stringify(clean, null, 2))

return { questionCount: clean.length, report }
```

---

## 2. Pipeline: review then verify (the default multi-stage shape)

**When:** items flow through ordered stages and each item should advance the
moment *it* is ready — no waiting for the slowest sibling. This is the default;
prefer it over two `parallel()` calls with a barrier between them.

```js
export const meta = {
  name: 'review-and-verify',
  description: 'Review each dimension, verify each finding as soon as its review lands',
  phases: [{ title: 'Review' }, { title: 'Verify' }],
}

const DIMENSIONS = [
  { key: 'bugs', prompt: 'Find logic bugs in the changed files.' },
  { key: 'perf', prompt: 'Find performance regressions in the changed files.' },
]

const results = await pipeline(
  DIMENSIONS,
  d => agent(d.prompt, { label: `review:${d.key}`, phase: 'Review', schema: FINDINGS_SCHEMA }),
  review => parallel((review?.findings ?? []).map(f => () =>
    agent(`Adversarially verify: ${f.title}`,
          { label: `verify:${f.file}`, phase: 'Verify', schema: VERDICT_SCHEMA })
      .then(v => ({ ...f, verdict: v }))))
)

return { confirmed: results.flat().filter(Boolean).filter(f => f.verdict?.isReal) }
```

Dimension `bugs` verifies its findings while `perf` is still being reviewed.

---

## 3. Barrier when you must dedup first

**When:** the next stage needs the *entire* previous result set in hand — to
dedup, merge, or early-exit on a count. This is the legitimate use of `parallel`
as a barrier.

```js
const all = await parallel(
  DIMENSIONS.map(d => () => agent(d.prompt, { schema: FINDINGS_SCHEMA })))

const deduped = dedupeByFileAndLine(
  all.filter(Boolean).flatMap(r => r.findings))   // genuinely needs ALL at once

if (deduped.length === 0) return { confirmed: [], note: 'nothing to verify' }

const verified = await parallel(
  deduped.map(f => () => agent(verifyPrompt(f), { schema: VERDICT_SCHEMA })))
return { confirmed: verified.filter(Boolean).filter(v => v.isReal) }
```

---

## 4. Loop until a target count

**When:** discovery with a fixed goal — "find 10 bugs". A plain counter is fine.

```js
const bugs = []
while (bugs.length < 10) {
  const r = await agent('Find bugs not already listed below.\n\n'
    + JSON.stringify(bugs.map(b => b.title)), { schema: BUGS_SCHEMA })
  bugs.push(...r.bugs)
  log(`${bugs.length}/10 found`)
}
return { bugs: bugs.slice(0, 10) }
```

---

## 5. Loop until the budget runs low

**When:** you want depth to scale to the user's token target. The
`budget.total &&` guard is essential — without a target, `remaining()` is
`Infinity` and the loop runs to the 1000-agent cap.

```js
const issues = []
while (budget.total && budget.remaining() > 50_000) {
  const r = await agent('Find one more issue in this codebase.', { schema: ISSUE_SCHEMA })
  issues.push(...r.issues)
  log(`${issues.length} found · ${Math.round(budget.remaining() / 1000)}k tokens left`)
}
return { issues }
```

---

## 6. Adversarial verification (skeptic vote)

**When:** a finding will be acted on and a plausible-but-wrong one is costly.
Spawn N independent skeptics, each told to *refute*; keep the finding only on a
majority. Stops confident hallucinations from surviving.

```js
async function survives(claim) {
  const votes = await parallel(Array.from({ length: 3 }, (_, i) => () =>
    agent(`Try hard to REFUTE this claim. Default to refuted=true if uncertain.\n\n${claim}`,
          { label: `skeptic:${i + 1}`, schema: VERDICT_SCHEMA })))
  return votes.filter(Boolean).filter(v => !v.refuted).length >= 2
}

const real = []
for (const f of candidateFindings) {
  if (await survives(f.title)) real.push(f)
}
return { real }
```

---

## 7. Judge panel (N attempts, score, synthesize)

**When:** the solution space is wide and one-attempt-iterated is weak. Generate
independent attempts from different angles, score them with parallel judges,
synthesize from the winner while grafting the runners-up's best ideas.

```js
const ANGLES = ['MVP-first', 'risk-first', 'user-first', 'cost-first']

// The runtime serializes `args` to a string; parse it (the guard keeps plain text intact).
const idea = typeof args === 'string'
  ? (() => { try { return JSON.parse(args) } catch { return args } })()
  : args

phase('Draft')
const drafts = await parallel(ANGLES.map(a => () =>
  agent(`Produce a plan for: ${idea}. Take a strictly ${a} approach.`, { label: a })))

phase('Judge')
const scored = await parallel(drafts.filter(Boolean).map((d, i) => () =>
  agent(`Score this plan 1-10 for feasibility and impact. Return {score, why}.\n\n${d}`,
        { label: `judge:${i + 1}`, schema: SCORE_SCHEMA }).then(s => ({ draft: d, ...s }))))

const ranked = scored.filter(Boolean).sort((a, b) => b.score - a.score)

phase('Synthesize')
const final = await agent(
  'Write the definitive plan. Base it on the WINNER, grafting the best ideas '
  + 'from the runners-up.\n\nWINNER:\n' + ranked[0].draft
  + '\n\nRUNNERS-UP:\n' + ranked.slice(1).map(r => r.draft).join('\n---\n'))
return { final }
```

---

## 8. Loop until dry (unknown-size discovery)

**When:** you do not know how much there is to find. Keep spawning finders until
K consecutive rounds turn up nothing new. Catches the long tail a fixed counter
misses.

```js
const seen = new Set()
const found = []
let dryRounds = 0

while (dryRounds < 2 && found.length < 100) {
  const r = await agent('Find issues NOT in this list:\n' + [...seen].join('\n'),
                        { schema: ISSUE_SCHEMA })
  const fresh = (r.issues ?? []).filter(x => !seen.has(x.id))
  fresh.forEach(x => { seen.add(x.id); found.push(x) })
  dryRounds = fresh.length === 0 ? dryRounds + 1 : 0
  log(`+${fresh.length} new · ${found.length} total · dry streak ${dryRounds}`)
}
return { found }
```

---

## 9. Nested workflow

**When:** a big workflow has a self-contained sub-job that is itself a workflow.
`workflow()` runs it inline and returns its result. Nesting is one level deep —
a workflow called this way cannot itself call `workflow()`.

```js
phase('Gather')
const research = await workflow('research-fanout', ['question one', 'question two'])

phase('Write')
const article = await agent('Write an article from this research:\n'
  + JSON.stringify(research))
return { article }
```

---

## 10. Fan-out then reconcile deferrals

**When:** a fan-out where each worker fixes what it can alone but DEFERS work that
spans items it does not own — a cross-file seam, a type two siblings must share, a
dangling cross-reference. A plain fan-out collects those deferrals and drops them:
the exact class of fix that needs an owner is surfaced and abandoned. The remedy is
a terminal reconcile stage that consumes them.

The deferral must be DATA whose resource slot is a LIST, so a deferral spanning two
files names BOTH — that is the only thing that lets you cluster by shared resource.

```js
// per-worker schema: a residual carries a FILE LIST, not a free string
const FIX = { type: 'object', required: ['file'], properties: {
  file: { type: 'string' },
  residual: { type: 'array', items: { type: 'object', required: ['files', 'claim'],
    properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

const done = (await parallel(items.map(it => () => agent(workPrompt(it), { schema: FIX })))).filter(Boolean)

// BARRIER (pure JS, zero tokens): collect, dedup, cluster by connected file-set (union-find).
const all = done.flatMap(d => d.residual ?? [])
const uniq = [...new Map(all.map(r => [r.files.join(',') + '|' + r.claim, r])).values()]
const clusters = unionFindBySharedFile(uniq)   // residuals sharing any file land in one cluster

let hard = []
if (clusters.length) {                          // count-barrier early-exit (pattern #3)
  const out = await pipeline(                    // each disjoint cluster verifies the moment ITS fix lands
    clusters,
    cl       => agent('Fix these cross-file deferrals in place; read every listed file.\n' + JSON.stringify(cl), { schema: FIXED }),
    (fix, cl) => agent('Adversarially verify each claim is ACTUALLY resolved; read the files from disk. One verdict per claim.\n' + JSON.stringify(cl), { schema: VERIFY }).then(v => ({ cl, v })))
  hard = out.filter(Boolean).flatMap(o => (o.v?.claims ?? []).filter(c => !c.resolved).map(c => c.claim))
}
return { hard }                                  // only genuinely-unresolvable deferrals reach the human
```

Why each choice: disjoint clusters write non-overlapping files, so the per-cluster
fixers run concurrently with no collision — `isolation:'worktree'` is unnecessary.
The verifier is a SEPARATE agent (a single self-reviewing fixer is the
self-correction anti-pattern) handed the claims as a checklist, and the
one-verdict-per-claim schema is what proves completeness — a dropped claim cannot
validate. Distinct from #1 (synthesize into one report) and #6 (skeptic vote on one
claim): this is cluster-by-shared-resource, then fix-and-verify each cluster. The
full worked file is `assets/examples/rebuild-and-reconcile.js`.

---

## 11. Steady worker pool for a large list of long chains

**When:** the list is large (hundreds) and each item is a long multi-stage chain.
`parallel(thunks)` enqueues all N at once and leans on the limiter to dequeue ~cap;
a steady worker pool holds a true steady state of ≤cap long chains, which is what
you want when each chain runs for minutes.

```js
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async () => { while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, run))
  return out
}
const done = (await pool(pages, 14, p => processPage(p))).filter(Boolean)
```

`parallel()` is still correct for a small fixed fan-out that needs a barrier; reach
for the pool only for the large-corpus case `parallel()` does not serve as well.

---

## Defining schemas

A schema is a plain JSON Schema object. Keep them small and `required`-tight so
the subagent returns exactly what you need.

```js
const FINDINGS_SCHEMA = {
  type: 'object',
  required: ['findings'],
  properties: {
    findings: {
      type: 'array',
      items: {
        type: 'object',
        required: ['title', 'file'],
        properties: {
          title: { type: 'string' },
          file:  { type: 'string' },
          line:  { type: 'number' },
        },
      },
    },
  },
}

const VERDICT_SCHEMA = {
  type: 'object',
  required: ['isReal'],
  properties: {
    isReal:   { type: 'boolean' },
    refuted:  { type: 'boolean' },
    reason:   { type: 'string' },
  },
}
```

Define schemas in the body (after `meta`), as `const`s — never inside `meta`.
