# Orchestration Patterns

Copy-paste orchestration shapes. Each names when it wins, the primitive it rests on, and the failure mode it guards — then gives runnable code. Match the pattern to the shape answers in SKILL.md: known list or unknown count, one pass or staged, barrier needed or not. JSON Schemas appear abbreviated as `SCHEMA`; define real ones per the closing section. Concurrency mechanics (pools, slots, packers) live in the throughput reference; runtime signatures in the api reference.

## [01]-[MAP]

The whole space is built from five primitives: `pipeline` (streaming stages, no barrier), `parallel` (a barrier — waits for every thunk), a plain-JS loop, a bounded worker pool (throughput reference), and the `agent()` leaf itself. Every shape below is a canonical topology or a hardened specialization:

| [INDEX] | [TOPOLOGY]           | [PRIMITIVES]                        | [FITS]                             | [GUARDS]                            |
| :-----: | :------------------- | :---------------------------------- | :--------------------------------- | :---------------------------------- |
|  [01]   | Prompt chaining      | `pipeline` or `agent()` chain       | Fixed ordered subtasks; chained    | One overloaded call losing accuracy |
|  [02]   | Sectioning           | `parallel` barrier or fan-out       | Independent subtasks, then combine | Sequential cost; diluted focus      |
|  [03]   | Voting               | `parallel`, N identical inputs      | One judgement wanting agreement    | A confident-wrong answer surviving  |
|  [04]   | Tournament           | JS bracket, blind pairwise judges   | Wide space, relative quality only  | Absolute-score drift; source bias   |
|  [05]   | Debate               | position → rebuttal → judge         | High-stakes ambiguous judgment     | Lone view missing the counter-catch |
|  [06]   | Routing              | JS discriminant → one `agent()`     | Classes want distinct specialists  | Generic prompt mediocre everywhere  |
|  [07]   | Escalation           | tiered residual re-dispatch         | Bulk on a cheap tier; hard residue | Top model doing floor-model work    |
|  [08]   | Orchestrator-workers | planner `agent()` → fan-out         | Subtask list unknown up front      | Hardcoding an absent worklist       |
|  [09]   | Evaluator-optimizer  | loop: generate → evaluate `agent()` | Clear pass bar; one draft too few  | Shipping first pass; self-grading   |
|  [10]   | Self-repair          | loop: check → fix agent             | Machine-checkable target in rounds | Exit on the claim, not the check    |

- Debate: disagreement exposes blind spots a lone view misses.
- Self-repair: machine-checkable means types, tests, or lint.

Selection rules that sit on top of the map: sectioning defaults to `pipeline`, never `parallel` — reach for the barrier only when a stage needs the ENTIRE previous result set at once. The evaluator is always a separate `agent()` from the generator — self-grading finds nothing. A verdict that a command measures belongs to self-repair or an eval gate, never to a model judge. The discrimination order runs as one dispatch — answer in order, land at the owning section:

1. Deliverable is transformed items over a KNOWN list: a later stage needing ALL results at once takes the barrier [05]; everything else takes the pipeline [04].
2. Deliverable is transformed items over an UNKNOWN count: a command-green stop is self-repair [09]; a count, token, or dry-streak stop is the loop family [16].
3. Deliverable is one contested verdict: command-checkable routes to the skeptic vote [11]; judgment in a shallow field to the panel [12], a wide or close field to the tournament [13], ambiguous high-stakes interpretation to the debate [14].

The delegation contract — objective, territory, exclusions, output contract, success criteria — is the agent-dispatch skill's prompting law and rides every `agent()` prompt here unchanged. The workflow-specific residue: the output contract is a `schema` wherever a later line reads a field, and mid-run clarification does not exist — a subagent left needing to ask was dispatched vaguely. A write-station's prompt also pins authorship: the writing is the agent's own — a nested delegate may fetch information, never author the deliverable, so the model tier the orchestrator paid for is the tier that writes.

Anti-shapes the catalog refuses, each priced by the failure it manufactures:

- Raw transcript as the coordination substrate: workers exchange receipts, typed facts, and product files ([21], [23]) — a full-history handoff bloats every downstream context and couples lanes to each other's reasoning.
- Debate or vote as VERIFICATION: a command-checkable claim takes self-repair or a refutation vote ([09], [11]); debate ([14]) earns its spend only on genuinely contested interpretation — agreement pressure suppresses independent correction, so unanimity is never the proof.
- Unbounded delegation: every fan carries a cap, every loop a stop, every worker a bounded charter; a worker that widens its own scope mid-run files the excess as data ([22]).
- The ceremonial Plan phase: an agent that returns a bare roster the next stage re-derives spends a context to produce nothing — the plan-phase law in SKILL.md, with the discovery bar at [19].
- Verdict-last ordering: an audit stage placed AFTER the run's last writer returns findings nothing consumes — the finder fan precedes the terminal fixer, every findings product names its consuming stage, and an audit whose guarantee is completeness reads its full evidence set, never a sample.

## [02]-[CHAIN]

Canonical: prompt chaining. Primitive: `pipeline`. Guards: the accuracy loss of one overloaded call, by giving each subtask its own clean-context agent. Cost: stages × items agent calls; wall-clock is the slowest item's full chain. When: the work splits into a fixed sequence where each stage consumes the previous stage's output — outline → draft → tighten; extract → normalize → validate.

```js conceptual
const sections = ['geometry kernel', 'mesh pipeline', 'IFC export'];

const out = await pipeline(
    sections,
    (s) => agent(`Outline the doc section for: ${s}. Return headings only.`, { label: `outline:${s}`, phase: 'Outline', schema: OUTLINE_SCHEMA }),
    (outline, s) => agent(`Draft "${s}" from this outline:\n${JSON.stringify(outline)}`, { label: `draft:${s}`, phase: 'Draft' }),
    (draft, s) => agent(`Tighten this "${s}" draft — cut redundancy, keep every claim:\n${draft}`, { label: `tighten:${s}`, phase: 'Tighten' }),
);

return { sections: out.filter(Boolean) };
```

A gate variant adds a JS check between stages — an outline that fails a cheap structural test drops (`return null`) before paying for the draft; the heavier machine-checked form is the eval gate at [10].

## [03]-[FANOUT]

Canonical: sectioning. Primitive: `parallel` barrier. Guards: sequential wall-clock, and the quality dilution of one call covering every angle. Cost: N + 1 calls; wall-clock is the slowest lane plus the synthesis. The synthesis genuinely needs every result, so the barrier is correct here.

```js conceptual
// `args` arrives as structured data — an array stays an array. Default the no-args run.
const questions = Array.isArray(args) && args.length ? args : ['demo question'];

phase('Research');
const findings = await parallel(
    questions.map(
        (q, i) => () => agent(`Research and report verified facts:\n\n${q}`, { label: `q${i + 1}`, phase: 'Research', schema: RESEARCH_SCHEMA }),
    ),
);
const clean = findings.map((f, i) => (f ? { question: questions[i], ...f } : null)).filter(Boolean);

phase('Synthesize');
const report = await agent('Combine the research below into one cohesive briefing; call out disagreements.\n\n' + JSON.stringify(clean, null, 2));

return { questionCount: clean.length, report };
```

Inline synthesis is small-output only: past ~50 rows of collected product, the `JSON.stringify` handoff spends the synthesizer's context before its work starts — route heavy products through the report-file topology at [21]. The synthesis is also where individually-valid lane results conflict, duplicate, or miss coverage — map-reduce-refine closes it: a SEPARATE auditor checks the merged artifact against a coverage-and-conflict contract, and a refiner repairs merge-level defects only, never re-doing lane work. One audit round is the spend; a merge that fails a second audit is a decomposition defect, not a refine target.

## [04]-[PIPELINE]

Canonical: chaining ⋈ sectioning — staged work where each item also fans out within a stage. Primitive: `pipeline` with a nested `parallel` inside one stage. Guards: the idle time a barrier inflicts; each item advances the moment IT is ready. Cost: the same calls as the barriered form, but wall-clock is the slowest single item's chain, never the sum of stage maxima. This is the default multi-stage shape; it displaces barriered `parallel()` pairs.

```js conceptual
const DIMENSIONS = [
    { key: 'bugs', prompt: 'Find logic bugs in the changed files.' },
    { key: 'perf', prompt: 'Find performance regressions in the changed files.' },
];

const results = await pipeline(
    DIMENSIONS,
    (d) => agent(d.prompt, { label: `review:${d.key}`, phase: 'Review', schema: FINDINGS_SCHEMA }),
    (review) =>
        parallel(
            (review?.findings ?? []).map(
                (f) => () =>
                    agent(`Adversarially verify: ${f.title}`, { label: `verify:${f.file}`, phase: 'Verify', schema: VERDICT_SCHEMA })
                        .then((v) => ({ ...f, verdict: v })),
            ),
        ),
);
return { confirmed: results.flat().filter(Boolean).filter((f) => f.verdict?.isReal) };
```

Dimension `bugs` verifies its findings while `perf` is still under review. The full worked file is `assets/examples/review-branch.js`.

## [05]-[BARRIER]

Canonical: sectioning. Primitive: `parallel` as a true barrier. Guards: double work and wasted spend — the next stage needs the ENTIRE previous result set in hand to dedup, merge, or early-exit on a count. Cost: wall-clock is each stage's slowest lane summed across stages — paid only because the dedup or early-exit genuinely needs the whole set. This is the legitimate use of `parallel` over a `pipeline`.

```js conceptual
const all = await parallel(DIMENSIONS.map((d) => () => agent(d.prompt, { schema: FINDINGS_SCHEMA })));

const deduped = dedupeByFileAndLine(all.filter(Boolean).flatMap((r) => r.findings)); // genuinely needs ALL at once
if (deduped.length === 0) return { confirmed: [], note: 'nothing to verify' };

const verified = await parallel(deduped.map((f) => () => agent(verifyPrompt(f), { schema: VERDICT_SCHEMA })));
return { confirmed: verified.filter(Boolean).filter((v) => v.isReal) };
```

## [06]-[ROUTE]

Canonical: routing. Primitive: a plain-JS discriminant choosing one `agent()` from a table. Guards: the mediocrity of one generic prompt forced to cover every input class, and cross-class interference where tuning for one class hurts another. Cost: one call per item, plus one classifier call only when the class is not a simple key; a new class is one table row at zero orchestration cost. The classifier is itself an `agent()` when the class is not a simple key.

```js conceptual
// One row per class: the discriminant value, the specialist prompt, and its model.
const ROUTES = {
    cs: { prompt: (f) => `Refactor ${f} to LanguageExt ROP; collapse parallel types into a [Union].`, model: 'inherit' },
    sql: { prompt: (f) => `Rewrite ${f} set-algebraically; push filters into the query.`, model: 'sonnet' },
};
const classify = (f) => (f.endsWith('.cs') ? 'cs' : f.endsWith('.sql') ? 'sql' : null);

const fixed = await parallel(
    changedFiles.map((f) => () => {
        const route = ROUTES[classify(f)];
        return route ? agent(route.prompt(f), { label: `fix:${f}`, model: route.model }) : null;
    }),
);

return { fixed: fixed.filter(Boolean) };
```

The dispatch table is the pattern: adding a class is one row, never a new branch threaded through the body. An unroutable input returns `null` and falls out at the `.filter(Boolean)` rather than hitting a wrong specialist. The full worked file is `assets/examples/route-and-refactor.js`.

## [07]-[PLAN_WORK]

Canonical: orchestrator-workers. Primitive: a planner `agent()` whose structured output becomes the items a `parallel`/`pipeline` fans out over. Guards: hardcoding a fixed worklist when the subtasks cannot be known up front — the files a migration touches, the modules a feature spans, the questions a topic raises. Cost: planner + N workers + integrator; wall-clock is the plan, then the slowest worker, then the integration.

```js conceptual
phase('Plan');
const plan = await agent(
    `Decompose this migration into independent units. One task per file or module that can be changed on its own.\n\n${task}`,
    { label: 'orchestrator', schema: PLAN_SCHEMA },
); // → { tasks: [{ id, file, instruction }] }

phase('Work');
const done = (
    await parallel(
        (plan?.tasks ?? []).map((t) => () =>
            agent(`${t.instruction}\n\nFile: ${t.file}`, { label: `work:${t.id}`, schema: WORK_SCHEMA }).then((r) => ({ ...t, ...r }))),
    )
).filter(Boolean);

phase('Integrate');
const integrated = await agent('Integrate these independently-completed units; resolve any seam between them.\n\n' + JSON.stringify(done));
return { integrated, unitCount: done.length };
```

The planner returns DATA (a typed task list via `schema`), never prose — the JS fans out over it. The integrator is the orchestrator's other half: workers cannot see each other's context, so a terminal stage owns whatever spans them. The planner earns its agent because decomposition is judgment; a "planner" that merely enumerates files is the plan-phase defect (SKILL.md) — fold that enumeration into a discovery stage that also analyzes ([19]).

Re-planning variant — the worklist emerges from evidence. When execution feedback reshapes the plan (a repository-wide change, open-ended research, a dependency graph discovered mid-run), the planner loops: workers execute only the issued tasks, their receipts feed a re-planner that emits the next FULL typed plan or `done`, and a revision cap plus the budget guard bound the loop. The re-planner receives receipts as data, never worker transcripts, and every revision is a complete plan — a diff-shaped patch drifts from the state it patches. The full worked file is `assets/examples/orchestrate-workers.js`.

## [08]-[REFINE]

Canonical: evaluator-optimizer. Primitive: a plain-JS loop wrapping a generate `agent()` and a SEPARATE evaluate `agent()`. Guards: shipping a weak first pass, and the self-correction anti-pattern — a generator grading its own work rubber-stamps it. Cost: two calls per round under the cap; the verdict-driven exit usually lands well below it. The pass verdict drives the loop; a hard round cap stops an unsatisfiable bar from looping forever. When the bar is machine-checkable, use self-repair at [09] instead — a command verdict beats a model verdict wherever one exists.

```js conceptual
let draft = null;
let feedback = '';
for (let round = 1; round <= MAX_ROUNDS; round++) {
    draft = await agent(`Implement the task. Address this feedback from the last attempt:\n${feedback || '(first attempt)'}\n\nTask: ${task}`, {
        label: `attempt:${round}`,
    });
    // SEPARATE evaluator — never self-grade → { passed, feedback }
    const review = await agent(`Evaluate this attempt against the acceptance criteria. Be strict.\n\n${draft}`, {
        label: `evaluate:${round}`,
        schema: REVIEW_SCHEMA,
    });
    if (review?.passed) return { draft, rounds: round };
    feedback = review?.feedback ?? '';
}
return { draft, rounds: MAX_ROUNDS, note: 'hit round cap without passing' };
```

The full worked file is `assets/examples/implement-and-review.js`.

## [09]-[SELF_REPAIR]

Canonical: evaluator-optimizer with a deterministic evaluator. Primitive: a loop alternating a read-only check agent (running a command) and a fixer agent. Guards: the exit firing on the worker's claim of done instead of an externally measurable verdict — a fixer that believes it finished while the suite is red, or a loop that burns rounds re-verifying fixes that changed nothing. Cost: a cheap low-effort check plus a fixer per round; check-first means an already-green target exits after one floor-model call. The stop condition is a machine fact: a passing suite, zero diagnostics, an empty queue.

```js conceptual
const MAX_ROUNDS = 6;
const CHECK = {
    type: 'object',
    additionalProperties: false,
    required: ['passed', 'failures', 'sample'],
    properties: { passed: { type: 'boolean' }, failures: { type: 'integer' }, sample: { type: 'string' } },
};

let last = Infinity;
let stalled = 0;
for (let round = 1; round <= MAX_ROUNDS; round++) {
    const check = await agent(
        // read-only verdict agent — edits NOTHING
        'Run exactly `npx tsc --noEmit`. Report passed, the failure count, and the first error verbatim. Do not edit any file.',
        { label: `check:${round}`, schema: CHECK, model: 'sonnet', effort: 'low' },
    );
    if (check?.passed) return { passed: true, rounds: round };

    stalled = (check?.failures ?? Infinity) >= last ? stalled + 1 : 0;
    if (stalled >= 2) return { passed: false, failures: check?.failures ?? -1, note: 'no progress across rounds' };
    last = check?.failures ?? Infinity;

    await agent(
        // fresh-context fixer per round — no degradation
        'Fix the errors `npx tsc --noEmit` reports. Start from this one and re-run the command as you go:\n' + (check?.sample ?? ''),
        { label: `fix:${round}` },
    );
}
return { passed: false, failures: last, note: 'hit round cap' };
```

Why each choice: check-first exits an already-green target at zero cost; the checker is a separate read-only agent, so the fixer never grades itself; the failure count is the progress gate — a non-decreasing count across consecutive rounds means the loop stopped converging, and the cap is only the runaway backstop; each round's fixer is a fresh context, so quality never degrades with accumulated transcript. An unattended long-horizon variant adds a separate verifier re-reviewing the changed files each round with its own lens — an independent check, never a re-run of the fixer's claim.

## [10]-[GATE]

Canonical: chaining hardened with an admission check. Primitive: a grader between stages that decides whether an item earns the next (expensive) stage. Guards: spending the heavy stage on output the cheap grader already rejects, and one-sided optimization — a gate probed only with must-pass cases drifts permissive, so the fixture set carries both admit and reject probes. Cost: one cheap call per item; the gate pays whenever rejected items × the expensive stage's price exceeds its own spend.

The grader ladder, strongest first: a deterministic grader (a command with a machine verdict — build, schema check, linter) wherever one exists; a model grader only for what no command measures, rubric-scoped to one dimension per call; a human only at the terminal artifact. Grade what the stage produced, never the path it took — a tool-call-sequence check breaks on every valid approach the design did not anticipate.

```js conceptual
const GATE = {
    type: 'object',
    additionalProperties: false,
    required: ['passed', 'reason'],
    properties: { passed: { type: 'boolean' }, reason: { type: 'string' } },
};

const out = await pipeline(
    items,
    (it) => agent(draftPrompt(it), { label: `draft:${it.id}`, phase: 'Draft' }),
    async (draft, it) => {
        const gate = await agent(
            // deterministic verdict, cheap lane
            'Run `' + it.check + '` against the draft at ' + it.path + '. Return the machine verdict; do not repair anything.',
            { label: `gate:${it.id}`, phase: 'Gate', schema: GATE, model: 'sonnet', effort: 'low' },
        );
        return gate?.passed ? draft : null; // rejected items drop before the expensive stage
    },
    (draft, it) => draft && agent(publishPrompt(it, draft), { label: `publish:${it.id}`, phase: 'Publish', effort: 'high' }),
);
return { published: out.filter(Boolean).length };
```

Trial law for a stochastic gate: when a stage must pass CONSISTENTLY, run the trial k times and demand every one passes (`parallel` over k thunks, then `.every`); a single-trial pass is enough only when a later verifier catches the false positives. Repeated gate mechanics are a staged script, never prose the grader re-derives — the throughput reference carries that law.

## [11]-[SKEPTICS]

Canonical: voting. Primitive: `parallel` over N identical-input thunks. Guards: a single confident hallucination surviving — a plausible-but-wrong finding one verifier waves through. Cost: N votes per claim — reserve it for findings that survive cheaper screens, never the raw candidate flood. Spawn N independent skeptics, each told to REFUTE; keep the finding only on a majority.

```js conceptual
async function survives(claim) {
    const votes = await parallel(
        Array.from({ length: 3 }, (_, i) => () =>
            agent(`Try hard to REFUTE this claim. Default to refuted=true if uncertain.\n\n${claim}`, { label: `skeptic:${i + 1}`, schema: VERDICT_SCHEMA })),
    );
    return votes.filter(Boolean).filter((v) => !v.refuted).length >= 2;
}

const real = [];
for (const f of candidateFindings) {
    if (await survives(f.title)) real.push(f);
}
return { real };
```

## [12]-[PANEL]

Canonical: voting feeding sectioning. Primitive: `parallel` to draft, `parallel` to score, one `agent()` to synthesize. Guards: the weakness of one-attempt-then-iterate when the solution space is wide — independent attempts from different angles, scored by parallel judges, then synthesized from the winner while grafting the runners-up's best ideas. Cost: 2N + 1 calls in three barriers. For a deeper field where absolute scores drift, use the tournament at [13].

```js conceptual
const ANGLES = ['MVP-first', 'risk-first', 'user-first', 'cost-first'];

const drafts = await parallel(ANGLES.map((a) => () => agent(`Produce a plan for: ${idea}. Take a strictly ${a} approach.`, { label: a, phase: 'Draft' })));
const scored = await parallel(
    drafts.filter(Boolean).map(
        (d, i) => () =>
            agent(`Score this plan 1-10 for feasibility and impact. Return {score, why}.\n\n${d}`, {
                label: `judge:${i + 1}`, phase: 'Judge', schema: SCORE_SCHEMA,
            }).then((s) => ({ draft: d, ...s })),
    ),
);
const ranked = scored.filter(Boolean).sort((a, b) => b.score - a.score);
const final = await agent(
    'Write the definitive plan. Base it on the WINNER, grafting the best ideas from the runners-up.\n\nWINNER:\n' +
        ranked[0].draft + '\n\nRUNNERS-UP:\n' + ranked.slice(1).map((r) => r.draft).join('\n---\n'),
    { label: 'synthesize', phase: 'Synthesize' },
);
return { final };
```

Portfolio pruning — spend the expensive stage on survivors only. When drafts are cheap and the terminal pass is dear, a deterministic or bounded scorer (a command gate, a rubric-scoped floor-model screen) ranks the field FIRST and only the top k reach the judge or synthesis: diversity happens before convergence, and the prune is an explicit budgeted transition, never a judge reading the whole flood. The panel above is the k-equals-all case; shrink k as the draft count grows, and let deliberately different strategies — not reworded prompts — supply the diversity the prune selects over.

## [13]-[TOURNAMENT]

Canonical: voting hardened for wide fields. Primitive: a plain-JS single-elimination bracket over blind pairwise judges. Guards: absolute-score drift — parallel judges scoring 1-10 calibrate differently, so ranks reshuffle run to run — and source bias, where a judge that knows which agent or angle produced a draft ratifies pedigree instead of quality. A comparative pick between two concrete artifacts is stable where an absolute scale is not.

```js conceptual
const PICK = {
    type: 'object',
    additionalProperties: false,
    required: ['winner', 'why'],
    properties: { winner: { type: 'string', enum: ['A', 'B'] }, why: { type: 'string' } },
};
// Single-elimination over the surviving drafts: pair the pool, judge each pair blind, winners + bye advance.
while (pool.length > 1) {
    const winners = await parallel(
        pairs.map(([a, b], i) => () => {
            const [first, second] = i % 2 ? [b, a] : [a, b]; // alternate order — cancels position bias
            return agent(
                'Pick the stronger submission strictly on the criteria. The labels are arbitrary and carry no meaning.\n\nCRITERIA:\n' +
                    CRITERIA + '\n\nA:\n' + first.body + '\n\nB:\n' + second.body,
                { label: `judge:${round}:${i}`, phase: 'Bracket', schema: PICK, effort: 'high' },
            ).then((v) => (v?.winner === 'A' ? first : second));
        }),
    );
    pool = winners.filter(Boolean).concat(bye);
}
// Graft: one agent improves the winner with the runners-up's strongest members — provenance unblinds only here.
```

The full worked file — draft fan, bracket loop, bye handling, graft — is `assets/examples/design-tournament.js`. Laws that ride the bracket: the judge sees only the artifacts and the criteria — never provenance, never prior verdicts; presentation order alternates by pair index, because a fixed A-first ordering leaks a measurable position preference; the graft stage is the only place provenance unblinds, after every comparison is done. Cost is N−1 judge calls for N drafts — cheaper than N absolute scorings once N passes a handful, and strictly better calibrated. Distinct from [12]: a panel scores in one shot and suits a shallow field; a bracket compares pairwise and suits a wide or close field.

## [14]-[DEBATE]

Canonical: adversarial sectioning. Primitive: independent positions → one anonymized rebuttal round → a separate ruling agent. Guards: a lone perspective missing what a counter-position catches, and its inverse — consensus pressure, where positions converge because agreement is comfortable, not because the argument won. Cost: 2N + 1 calls; the single rebuttal round is the whole spend — further rounds buy convergence pressure, not signal. Use it for ambiguous, high-stakes judgment (an architecture choice, a contested diagnosis); an objectively checkable claim routes to the skeptic vote at [11] instead.

```js conceptual
const LENSES = ['operational risk', 'long-term maintainability', 'raw performance'];

const open = (
    await parallel(
        LENSES.map((l, i) => () =>
            agent(`Argue the strongest position on the question below strictly from the lens of ${l}.\n\n${QUESTION}`, {
                label: `pos:${i}`, phase: 'Position',
            })),
    )
).filter(Boolean);
const closed = (
    await parallel(
        open.map(
            (p, i) => () =>
                agent(
                    'Revise the OWN position after weighing the counter-positions. Concede a point only to a stronger argument, never to numbers — ' +
                        'majority is not evidence. Keep what survives.\n\nOWN:\n' + p + '\n\nCOUNTER:\n' + open.filter((_, j) => j !== i).join('\n---\n'),
                    { label: `rebut:${i}`, phase: 'Rebut' },
                ),
        ),
    )
).filter(Boolean);
const ruling = await agent(
    'Adjudicate the dispute below point by point: where the positions agree after rebuttal, that is settled; where they still disagree, rule ' +
        'with reasons. Return the decision and the residual open questions.\n\n' + closed.join('\n===\n'),
    { label: 'rule', phase: 'Rule', schema: RULING_SCHEMA, effort: 'xhigh' },
);
return { ruling };
```

Laws: the diversity of the lenses does the work, not debate length — one rebuttal round captures the gain, and further rounds mostly manufacture convergence; positions travel anonymized (no author labels), so rebuttals attack arguments, never reputations; the judge is a separate agent that never held a position; post-rebuttal agreement is signal, but unanimity without reasons is treated as pressure, not proof.

Evidence-graph hardening — adjudicate structure, never transcript. For the highest-stakes contested claim, each position returns ATOMIC claims with evidence plus explicit support/attack edges under a typed schema; a zero-token JS merge builds the graph, and the judge rules per edge on evidence coverage. A ruling over conversational prose cannot be audited, and free-form exchange converges on comfort — the graph makes every concession traceable to the evidence that forced it.

## [15]-[ESCALATE]

Canonical: routing by outcome. Primitive: a tier table and a residual loop — every item enters the cheapest tier, and only items that FAIL a tier's check re-dispatch to the next. Guards: paying the strongest model for volume a floor model finishes; the bulk resolves cheap, and the expensive tiers see only what defeated the tier below. Cost: expected spend per item is the tier ladder weighted by residual rates — the top tier prices only the hard residue.

```js conceptual
const TIERS = [
    { model: 'sonnet', effort: 'low' },
    { model: 'inherit', effort: 'high' },
    { model: 'fable', effort: 'high' },
];
const OUT = {
    type: 'object',
    additionalProperties: false,
    required: ['passed', 'nav'],
    properties: { passed: { type: 'boolean' }, nav: { type: 'array', items: { type: 'string' } } },
};

let pending = items.map((it) => ({ it, tier: 0, nav: [] }));
const done = [];
while (pending.length) {
    const wave = (
        await parallel(
            pending.map(({ it, tier, nav }) => () =>
                agent(workPrompt(it) + (nav.length ? '\nTouched so far (locations only): ' + nav.join(', ') : ''), {
                    label: `t${tier}:${it.id}`, model: TIERS[tier].model, effort: TIERS[tier].effort, schema: OUT,
                }).then((r) => ({ it, tier, r }))),
        )
    ).filter(Boolean);
    pending = [];
    for (const w of wave) {
        if (w.r?.passed) done.push({ id: w.it.id, tier: w.tier });
        else if (w.tier + 1 < TIERS.length) pending.push({ it: w.it, tier: w.tier + 1, nav: w.r?.nav ?? [] });
        else done.push({ id: w.it.id, tier: w.tier, failed: true });
    }
}
return { done };
```

Laws: the tier verdict comes from a check (the schema's `passed` backed by a command or a separate verifier), never the worker's optimism; the escalated prompt carries the failed attempt's NAVIGATION only — files and locations touched, never its rationale or self-assessment, per the anchoring law at [20]; the top tier's failures return as data, never silently dropped. The same ladder tiers a review: a floor-model screen passes only its uncertain findings to the expensive judge.

## [16]-[LOOPS]

Canonical: orchestrator-workers with an unknown count — the loop IS the orchestrator. Primitive: a plain-JS `while` whose guard composes the stop table below. Guards: both halves of the unknown-size trap — a fixed counter stops short of the long tail, an open loop never terminates — plus the ungoverned spend a token target closes. Cost: rounds scale inversely with per-round yield; the composed guard is the only ceiling. One shape carries every stop; a run keeps the guards its work needs and always keeps a hard cap.

| [INDEX] | [STOP]       | [GUARD]                                  | [FITS]                                     |
| :-----: | :----------- | :--------------------------------------- | :----------------------------------------- |
|  [01]   | Count target | `found.length < N`                       | discovery with a fixed goal                |
|  [02]   | Token target | `budget.total && budget.remaining() > K` | depth scaled to the user's token directive |
|  [03]   | Dry streak   | `dry < K` reset on any fresh yield       | unknown, unbounded corpus                  |
|  [04]   | Progress     | file-changing progress per round ([17])  | fix-verify drive-to-zero                   |

```js conceptual
const seen = new Set();
const found = [];
let dry = 0;
while (dry < 2 && found.length < 100 && (!budget.total || budget.remaining() > 50_000)) {
    const r = await agent('Find issues NOT in this list:\n' + [...seen].join('\n'), { schema: ISSUE_SCHEMA });
    const fresh = (r?.issues ?? []).filter((x) => !seen.has(x.id));
    fresh.forEach((x) => {
        seen.add(x.id);
        found.push(x);
    });
    dry = fresh.length === 0 ? dry + 1 : 0;
    log(`+${fresh.length} new · ${found.length} total · dry streak ${dry}`);
}
return { found };
```

- The `budget.total &&` spelling is load-bearing on a budget-driven stop: with no target, `remaining()` is `Infinity` and the loop runs to the 1000-agent cap; the `!budget.total ||` form above lets a budget-less run fall through to its other stops.
- The cumulative `seen` set is the dedup spine — each round's prompt excludes everything found, so yield measures genuinely new work and a dry streak means the corpus is exhausted, never that the finder repeated itself. The exclusion paste grows per round and is small-output-only; a heavy accumulator moves to a report file ([21]).

## [17]-[RECONCILE]

Canonical: sectioning with a terminal orchestrator stage. Primitive: `parallel` to fan out, a pure-JS barrier to cluster, `pipeline` to fix-and-verify each cluster. Cost: clustering is zero-token JS; the reconcile adds one fix-verify pair per cluster on top of the fan. Guards: the silent loss of cross-item work — each worker fixes what it owns alone but DEFERS work that spans items it does not own (a cross-file seam, a type siblings must share, a dangling reference). A plain fan-out collects those deferrals and drops them. The remedy is a terminal reconcile stage that consumes them.

The deferral must be DATA whose resource slot is a LIST, so a deferral spanning files names them ALL — that is what permits clustering by shared resource:

```js conceptual
// per-worker schema: a residual carries a FILE LIST plus a claim, never a free string; `residual` is required-but-possibly-empty
const FIX = { …, residual: { type: 'array', items: { …, required: ['files', 'claim'] } } };

const done = (await parallel(items.map((it) => () => agent(workPrompt(it), { schema: FIX })))).filter(Boolean);

// BARRIER (pure JS, zero tokens): collect, dedup, cluster by connected file-set (union-find).
const all = done.flatMap((d) => d.residual ?? []);
const uniq = [...new Map(all.map((r) => [r.files.join(',') + '|' + r.claim, r])).values()];
const clusters = unionFindBySharedFile(uniq); // residuals sharing any file land in one cluster
// disjoint clusters fix-and-verify concurrently (pipeline: fix, then adversarial per-claim verify);
// only claims a verify leaves unresolved reach the return — logged LOUDLY, never dropped
```

Why each choice: disjoint clusters write non-overlapping files, so the per-cluster fixers run concurrently with no collision — `isolation:'worktree'` is unnecessary. The verifier is a SEPARATE agent handed the claims as a checklist, and the one-verdict-per-claim schema is what proves completeness — a dropped claim cannot validate. Distinct from [03] (synthesize one report) and [11] (skeptic vote on one claim): this is cluster-by-shared-resource, then fix-and-verify each cluster. The full worked file is `assets/examples/rebuild-and-reconcile.js`. When clusters must consolidate into a bounded agent count, the work-weight packer and the fair-share atomicity budget in the throughput reference own the balancing — a count-balanced or cluster-atomic packer recreates a 2x-plus long pole.

Iterating to drive-to-zero — the progress gate. The shape above fixes each cluster ONCE. When the reconcile instead ITERATES — re-queue the residuals a verify left `open`, re-cluster, fix again, round after round — every round MUST gate on file-changing PROGRESS, or it spends rounds verifying fixes that changed nothing:

```js conceptual
const seen = new Set();
let pending = uniq;
let round = 0;
while (pending.length && round++ < MAX_ROUNDS) {
    let changed = false;
    const next = [];
    for (const cl of unionFindBySharedFile(pending)) {
        const fix = await agent(fixPrompt(cl), { schema: FIXED });
        if (!(fix?.files ?? []).filter(inRepo).length || fix?.verdict === 'clean') continue; // (1) no change -> NO verify
        changed = true;
        const v = await agent(verifyPrompt(cl, fix.files), { schema: VERIFY });
        for (const c of v?.claims ?? [])
            if (!c.resolved && !seen.has(key(c))) {
                seen.add(key(c));
                next.push(c);
            } // (2) only NEW
    }
    if (!changed) break; // (3) a round that changed no file never will -> stop
    pending = next;
}
return { hard: pending }; // still-open: log LOUDLY + return, never drop
```

(1) a fix that touched no file (or returned `clean`) has nothing to verify — skip the verify and drop the cluster; (2) the cumulative `seen` set (key `sorted-files|claim`) stops a fixer that re-surfaces the same residual from feeding the loop forever; (3) a round that changes no file never will, so break — `MAX_ROUNDS` is a runaway backstop, never the exit. The no-defer guarantee holds: a genuinely-open residual is still surfaced, never dropped.

## [18]-[NEST]

Canonical: composition — a topology as a worker inside a larger one. Primitive: `workflow()`. Guards: re-inlining a self-contained sub-job by hand. Cost: the child spends from this run's shared caps and budget — nesting isolates state, never spend. `workflow()` runs a saved workflow inline and returns its result; nesting is one level deep.

```js conceptual
phase('Gather');
const research = await workflow('research-fanout', ['question one', 'question two']);

phase('Write');
const article = await agent('Write an article from this research:\n' + JSON.stringify(research));
return { article };
```

Call the child once with the whole work-set, never once per item in a loop. Each `workflow()` invocation owns its internal state — caches, `seen`-sets, dedup closures. A `for (const item of items) await workflow('child', item)` re-runs the child's shared discovery per item, and when the child's reachability overlaps it both redoes the overlapping work and mis-classifies an item that is primary in one call yet secondary in another. Make the child accept its scope as a single value OR an array, and pass the full set in one call; the child's dedup, closure, and classification guarantees hold only WITHIN one invocation. Thread cross-cutting run flags (a dry-run toggle, a model override) into the child's `args` too, so the whole tree honors them.

## [19]-[SCOPE]

Canonical: orchestrator-workers where the planner is a discovery agent resolving caller targets into the worklist. Primitive: one discovery `agent()` (the orchestrator has no filesystem) emitting structured page sets, then plain-JS filtering. Cost: one cheap discovery call, then spend proportional to the targeted subset, never the corpus. Guards: the fragility of a workflow that accepts only one coarse scope. A granular workflow takes a TARGET that is a single file, a sub-folder at ANY nesting depth, a unit root, or several of these at once, and acts on exactly that subset — while keeping a folder-wide terminal concern over the whole owning unit.

What makes it robust:

- Read `args` as `string | string[] | {targets}`, default the no-op — an empty run is a no-op, never a full-corpus sweep.
- Expand targets inside an agent, never in JS — a `find` over arbitrary nesting belongs in the discovery agent, which returns the targeted subset (the cost lever) and the folder-wide set (the blast radius) kept separate, so a one-file target stays a one-file rebuild.
- Derive the owning unit by splitting on a STRUCTURAL SENTINEL, never a fixed depth: splitting `unit/.planning/a/b/c.md` on `'/.planning/'` homes it correctly; a fixed-depth `split('/')[2]` breaks the moment a sub-folder nests deeper.
- A sub-domain whose governing root is an ANCESTOR needs an explicit homing branch named in the discovery prompt.

```js conceptual
// A target, an array of targets, or {targets:[...]}; empty = no-op
const ROOT = 'libs/python';
const normTarget = (t) => {
    const s = String(t).trim().replace(/\/+$/, '');
    return s === ROOT || s.indexOf(ROOT + '/') === 0 ? s : ROOT + '/' + s.replace(/^\/+/, '');
};
const rawTargets = Array.isArray(args) ? args : Array.isArray(args?.targets) ? args.targets : typeof args === 'string' && args.trim() ? [args] : [];
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))];

// One row per owning unit: targetPages = the granular subset to act on; folderPages = every page under the unit (the blast radius)
const DISCOVERY = { …, units: { items: { …, required: ['unitRoot', 'targetPages', 'folderPages'] } } };

const inv = await agent(
    'Resolve these TARGETS into owning units + page sets. A target is a UNIT root (expands to every page under it), a SUB-FOLDER at ANY ' +
        'depth (every page under it), or a FILE (itself); the owning unit is the path BEFORE the structural sentinel "/.planning/" (or the ' +
        'target itself when it has none). Return one row per distinct owning unit: unitRoot, targetPages (the deduplicated union every ' +
        'target resolves to inside it), folderPages (every page under it, regardless of targeting). Use find; do not cd.\n' + JSON.stringify(TARGETS),
    { label: 'discover', schema: DISCOVERY, model: 'sonnet', effort: 'low' },
);

const units = (inv?.units ?? []).filter((u) => (u.targetPages ?? []).length);
if (!units.length) {
    log('no targets — pass a file, sub-folder, or unit path');
    return { targets: TARGETS, total: 0 };
}

// Per-unit chain under one agent-level slot cap (throughput reference): each unit's terminal seam check
// fires the moment ITS OWN pages land — a global page barrier before the seam fan holds every unit's
// terminal hostage to the slowest sibling unit. The seam stage runs ONLY where untargeted siblings exist,
// scoped to the TARGETED units, never the corpus.
const slot = makeSlots(CAP);
const done = await Promise.all(
    units.map(async (u) => {
        const pages = (await Promise.all(u.targetPages.map((p) => slot(() => processPage(p))))).filter(Boolean);
        const seam =
            u.targetPages.length < u.folderPages.length
                ? await slot(() => agent(seamPrompt(u), { schema: SEAM_SCHEMA, effort: 'xhigh' }))
                : null;
        return { unit: u.unitRoot, pages: pages.length, seamed: !!seam };
    }),
);
```

The `string | array | unit-root | sub-folder | file` target space collapses to one discovery agent plus pure-JS filtering — accepting many targets is the array branch in `rawTargets`, never a second entrypoint. A "many"-granularity sibling (one agent per whole unit instead of per file) is the same shape with a coarser work unit and the terminal stage promoted to a cross-unit align; size the unit to the coherence boundary the stages need.

## [20]-[HANDOFF]

Canonical: prompt chaining hardened for review integrity. Primitive: sequential `agent()` stages passing a facts-only JS projection. Cost: the projection is free JS — what it buys is a reviewer whose verdict is independent enough to be worth its spend. Guards: reviewer anchoring — a reviewer that reads the producer's rationale, self-assessment, or confidence ratifies instead of attacking; verdicts flip on authority cues alone, "ignore the prior verdict" instructions do not remove the bias, and a reviewer handed the producer's framing scores below one reading the artifact cold. Withholding is the only mitigation that works.

The rules that make a writer → critic → red-team chain fast AND independent:

- Pass navigation, withhold assessment. The payload carries only verifiable location facts — touched files, symbol deltas as data (`{symbol, change}`), seam/ripple pointers, decline-ledger rows (refutation targets the reviewer re-verifies, never settled verdicts) — never the producer's summary, verdict, confidence, or rationale; scope ("look here first") is legitimate, assessment ("this is complete") is the anchor. The same law binds SCHEMAS: a self-grade field (`verdict: fixed|clean`) in a traveling product is the leak in data form — no downstream stage consumes it, and it rides the disk product into every later reviewer; facts, deltas, and dispositions are the record. Build the projection in JS from schema fields so adjectives cannot leak:

    ```js conceptual
    const navOf = (fix) => ({ files: fix.files, deltas: fix.deltas, seams: fix.seamsTouched, declinedIdeas: fix.declinedIdeas });
    const crit = await agent(criticPrompt(pages, navOf(fix)), { schema: REVIEW }); // fix.summary never travels
    ```

- Own verdict first. Order the reviewer's work explicitly: derive an independent defect list from the artifact on disk FIRST, then use the navigation to reach touched territory fast. Never place the prior stage's output last in the prompt — trailing content reads as the conclusion; end on the task and output contract instead.
- Third-party framing. Present the artifact as another author's submission under review — models catch errors in others' text that they cannot see in work they believe is their own.
- Claims only to the terminal skeptic, as refutation targets. A later red-team stage MAY receive the critic's fix-log — explicitly framed as unverified claims to refute against the current artifact, placed mid-prompt, never as a settled record.

Give sequential review stages genuinely different objectives (a clause-by-clause conformance audit against a pre-mortem/counterfactual attack) and license "clean after a failed attack" as a first-class verdict — a second identical review manufactures findings the artifact cannot supply, and a reviewer forced to edit invents defects. Distinct from [11] (independent skeptic votes on one claim): this hardens a SEQUENTIAL chain where every stage also writes.

A review stage carries the WRITER'S full authority: scope rows bound where the reviewer looks FIRST, never what it may fix — every defect it finds is fixed at its root in the same pass regardless of scope, and deferral is reserved for territory a LIVE sibling owns, never for "outside my unit". Residuals drain at the nearest subsequent stage as they surface; a run that accumulates them toward one terminal mega-reconcile has mispriced every stage before it. The same anchoring law binds the orchestrator's own prompt authorship: a judgment stage handed a pre-ruled example outcome (a named split, a sample verdict, a worked disposition) inherits the example as a ruling — state the criteria and the pressures on both sides, never an example resolution.

## [21]-[REPORTS]

Canonical: sectioning hardened for heavy products. Primitive: a fan of producing LANES, each writing its complete product to run scratch and returning a thin receipt; one terminal reader consuming the files. Cost: receipts hold the wire at a constant few fields per lane regardless of product size; the terminal reader pays for full products once, from disk. Guards: relay loss — every hop a heavy product takes through an intermediate agent's structured output is a truncation/paraphrase risk on the weakest model in the chain, and a full product `JSON.stringify`-ed into a downstream prompt spends the reader's context before its work starts. This is the dataflow contract, not a topology of its own — it composes with any concurrency shape.

Vocabulary, one meaning each: a LANE is one concurrent worker in a fan — its own scope, product, and receipt; a STAGE is one position in a pipeline; a PRODUCT is a lane's complete output; a RECEIPT is the thin `{ok, report, entries, headline, failure, thread}` envelope that stands in for it on the wire — `thread` is the external-lane session join key and dead-lane resume handle (external-lanes reference), required in the schema and returned empty by native lanes. This block is the ONE canonical receipt shape; every template, example, and lane law cites it rather than restating its fields. Sourcing rides product size, never mixed ad hoc:

| [INDEX] | [TIER]                   | [WHEN]                                           | [MECHANISM]                                     |
| :-----: | :----------------------- | :----------------------------------------------- | :---------------------------------------------- |
|  [01]   | Inline structured output | Small structural outputs; default below ~50 rows | Schema-tight `agent()` structured output        |
|  [02]   | Scratch product file     | Any heavy product                                | Lane writes product to scratch, returns receipt |
|  [03]   | `journal.jsonl`          | Recovery only                                    | Never a designed data path                      |

- Inline structured output: the default is plans, slices, verdicts, counts, paths.
- Scratch product file: heavy product means maps, findings, dossiers, reports; consumers READ THE FILE IN FULL.

The scratch convention — one layout, no exceptions:

- Run scratch is `.claude/scratch/<workflow-name>-<slug>-<hash>` (repo-relative, gitignored, ephemeral — deletable once the run's campaign closes): ONE FLAT directory per INSTANCE, never per workflow. A per-workflow constant dir mixes the products of concurrent and successive runs — a sibling run's leftover report reads as THIS run's product to any consumer handed the path, and a second launch clobbers a paused run's dossiers.
- The script mints the path deterministically from its NORMALIZED args, after args normalization and never in a constants block above it — a clock or randomness breaks resume, since a resume re-executes the script and must rehydrate the identical directory.
- `<slug>` is the human-readable scope (target basenames joined, lowercased, `[^a-z0-9.-]` runs collapsed to `-`, bounded ~60 chars); `<hash>` is a short FNV-1a of the normalized args JSON, so equal slugs over distinct arg sets still fork (`libs/python/data` and `libs/typescript/data` share a basename, never a directory). Never a tool-segregated dir, never a bespoke sibling, never extra nesting — scope beyond the instance rides the FILENAME.

```js conceptual
// Instance mint — sits AFTER args normalization; deterministic, so a resume recomputes the identical path.
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/' +
    ('<workflow-name>-' + TARGETS.map((t) => t.split('/').pop().toLowerCase()).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));
```

- Gitignore consequence: `rg`/`fd`/Grep skip ignored dirs by default, so consumers are handed EXPLICIT paths (the roster) and read them directly — never asked to discover products by search; an agent that must hunt inside scratch passes `--no-ignore` (rg) or `-I` (fd).
- File grammar: `<scope>-<lane>-<artifact>.<ext>` — lane is a 1-2 word semantic slug (`s0`, `gov`, `rip-python`), artifact names the role (`report`, `dossier`, `map`); no agent names, timestamps, or run IDs in filenames. A codex lane owns exactly one artifact — its report, written by the wrapper from the tool result; no task, schema, events, or stderr files exist on the MCP path.
- A lane's first act deletes its own prior report (`rm -f`) — a leftover file reads as THIS run's product to any consumer handed the path.
- Run scratch (the lanes' data plane) is distinct from the SESSION SCRATCHPAD (the harness temp dir outside the repo) — orchestrator-only artifacts like the run ledger live in the session scratchpad, never in run scratch.

Dual schema: the PRODUCT schema types the on-disk file; the RECEIPT types the wire. Both strict — every object `additionalProperties: false` with every property required — so one shape serves AJV lanes and codex `--output-schema` alike.

ONE REPRESENTATION PER FACT: a lane authors each fact once. A content lane whose product is a prose dossier returns a thin INDEX receipt (per-scope-key pointers into the dossier's sections plus coverage) — a wire product restating the dossier's content is double-authoring the consumer never reads, and transcript evidence shows consumers pick one twin and orphan the other. The same law binds hand-authored disk twins: a coordination ledger row and its wire `seamsTouched` fold are ONE authored row in two transports, never two derivations. A streaming coordination surface appends per event with an ordering prefix (`seq | origin | stage | TYPE | payload`) — a file written once at the end has failed its coordination purpose, and a consumer of a columnless concurrent file cannot order or trust its rows.

```js conceptual
// One anchor = one fact at one coordinate; interpretation never lives in an anchor row. `note` is the shortest literal witness under 20 words,
// or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not found, BOUNDED by what was actually
// read — full-file coverage grounds a file-wide absence, a partial read grounds only the range read stated as that range (`<file>:1-NNN searched`).
const ANCHOR = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

// Defect-shaped PRODUCT. A recon/inventory product swaps the defect fields for prose `info`
// facts plus verified `members`, framed inventory-never-instruction, and swaps the anchor
// role `defect` for `state`; anchors and coverage stay.
const PRODUCT = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'coverage', 'summary'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claimKey', 'target', 'files', 'class', 'severity', 'claim', 'anchors', 'mechanism', 'owner', 'reject', 'acceptance'],
                properties: {
                    // <class>|<owner>|<primary symbol or absence route> — schema-enforced; free-text keys fracture per lane
                    claimKey: { type: 'string', pattern: '^[a-z0-9_-]+(\\|[a-z0-9_-]+){2}$' },
                    target: { type: 'string' }, // short display label
                    files: { type: 'array', items: { type: 'string' } }, // What the reader must open or edit first
                    class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // The observed defect as fact
                    anchors: { type: 'array', items: ANCHOR },
                    mechanism: { type: 'string' }, // WHY it fails — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: { type: 'string' } }, // signals proving resolution
                },
            },
        },
        coverage: {
            type: 'object',
            additionalProperties: false,
            required: ['requested', 'read', 'skipped', 'unverified'],
            properties: {
                requested: { type: 'array', items: { type: 'string' } },
                read: { type: 'array', items: { type: 'string' } },
                skipped: { type: 'array', items: { type: 'string' } },
                unverified: { type: 'array', items: { type: 'string' } },
            },
        },
        summary: { type: 'string' },
    },
};

// Thin wire receipt — the canonical shape; the lane's PRODUCT stays on disk at `report`.
// `thread` carries the external-lane session id (external-lanes reference); native lanes return ''.
const RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure', 'thread'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
        thread: { type: 'string' },
    },
};

// Dispatch helper: codex wrapper when CODEX, native lane otherwise (the native prompt names the report path,
// the PRODUCT schema, and "return ONLY the receipt … thread empty"). The helper is the ONE composition point
// for every cross-cutting rider — watchdog race, quota fallback, telemetry bracket, scope attachment — so a
// rider lands once, never per call site. The `.then()` attaches the ORCHESTRATOR-ASSIGNED scope: a lane that
// dies before writing still names its territory. The blocking tool call is the wrapper's legal wait.
const lane = (task, o) =>
    (CODEX
        ? agent(codexLane(o.label, task, PRODUCT, !!o.writes), {
              label: 'terra:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL,
          })
        : agent(nativeLane(task, o.label, PRODUCT), { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [], // orchestrator-assigned, never the lane's self-report
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
        thread: (r && r.thread) || '',
    }));

const roster = (await parallel(slices.map((s, i) => () => lane(producePrompt(s), { label: 's' + i, phase: 'Produce', scope: s })))).filter(Boolean);
const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })));
// Terminal reader: UNMAPPED as the direct-hunt queue + the roster's report paths; its FIXLOG carries
// {files, resolved[], beyond[], rejected[], summary} — `beyond` attests the reader's own hunt ran.
```

Laws that ride the shape:

Receipts are thin and mechanical. `report` is the product path; `entries` is jq-counted from the product's primary array; `headline` is jq-built (per-class tallies, top file) — never the lane's own judgment or a lifted summary sentence, so the terminal reader meets every product cold. A failed lane returns `{ok: false, report: '', entries: 0, headline: '', failure: <stderr tail, one line>, thread: <id if any, else ''>}` — failure lives in the envelope, never as sentinel values inside data rows; downstream filters on `ok`, never string-matches magic values.

The producer prompt carries the evidence law for defect-shaped products: the lane delivers TRUTH, never an implementation — `claim` states the observed defect and `mechanism` states why it fails as fact, with add/replace/implement/promote/delete never written as instruction; the reader owns the design, the lane owns the constraint boundary (`owner`, `reject`, `acceptance`). A cross-lane dedupe key is schema-enforced with a `pattern`, never prose-specified alone — free-text keys fracture into incompatible per-lane formats, and corroboration of one defect across lanes then never collides into one key. Output bounds: an ordinary scope yields 3-8 retained findings; 0 only after a mandatory second-pass self-verify (re-open every cited anchor, delete what fails re-confirmation) returns empty, with `summary` naming the probes that produced nothing. `coverage` is part of the product — an honest skip beats a silent one, and a lane whose PRIMARY verification route is unavailable verifies through its named fallback and records the substitution in `coverage.unverified`, so the consumer reads coverage before weighting members. Rail honesty extends to the row grain: a member confirmed only against catalog prose is graded `catalog-anchored`, never `verified` — the verification claim names the rail actually run — and a section that CAN carry rows but carries none states the checks that earned the emptiness, because a silent empty is indistinguishable from an unexamined one. A consumer obligated to land upstream rows dispositions each BY ID with a closed verdict vocabulary — landed, refuted with the disk fact, deferred to its named owner — so no row goes silent.

Anchors must survive the run's own mutations: an anchor into a file a LATER stage deletes, moves, or wholesale-rewrites (a source swap, a scaffold teardown) is valid only when the consumer instruction names the recovery route beside the re-open mandate — `git show <pre-swap-hash>:<path>` for a swapped source, the move target for a relocation; a bare "re-open every anchor" over vanished files silently voids the verification law.

The terminal reader's consumption protocol, baked into its prompt, in order: (a) UNMAPPED is the direct-hunt queue — a failed lane's territory gets the reader's own cold read FIRST; (b) every ok report read IN FULL from disk, shared-surface/governance lanes before per-item lanes, grouped by `claimKey` while reading — the same key across lanes is ONE defect with corroborating evidence, never several priorities; (c) every entry is a SIGNAL, not law — anchors behind an edit re-verify MANDATORY, navigation-only entries in untouched groups re-verify only when touched; (d) a finding whose anchors do not re-confirm is rejected with reason, and the reader hunts PAST the signal list on its own authority.

Fix-to-root is the executor's standing law, not a per-pattern nicety: the terminal reader and every write-station resolve each confirmed defect at its cause, and a defect surfaced beyond the mapped scope — the `beyond[]` attestation, a lane's UNMAPPED territory, a quirk caught mid-edit — is fixed in the same pass, never handed to a later one. A defect genuinely out of reach lands as an explicit unreachable naming its owner, the [22] escalation seam a recurring pass files as data, never a silent residual a successor re-discovers.

Recovery synergy: product files make every stage re-enterable at zero cache dependence — receipts are exactly what a continuation script rebuilds (recovery reference).

Distinct from [03] (inline synthesis — correct below ~50 rows) and [17] (deferral reconcile — which rides this contract when its residual sets grow heavy). The full worked file — codex lanes, watchdog race, quota fallback, batched probes, terminal reader — is `assets/examples/codex-lane-batch.js`. A report specimen under another workflow's scratch path is read-only evidence of a retired run, never a copy target — every new lane writes under its own workflow's run scratch.

## [22]-[RECUR]

Canonical: composition with the harness — a workflow as the deterministic inner pass of an outer loop, or a chain of workflows with the conversation as the checkpoint. Primitive: a saved workflow plus a harness trigger (`/loop`, a schedule, a goal loop) or a per-stage `Workflow` launch. Cost: priced per tick, so the idempotency law IS the economics — an empty-queue tick exits at the no-op guard for near zero. Guards: two failure modes the runtime imposes — a workflow accepts no mid-run user input, and a recurring job hand-driven every time never becomes durable.

- Checkpoint chains. A stage that needs human sign-off becomes its OWN workflow: stage one runs, its product lands on disk, the operator rules in conversation, stage two launches with the ruling in `args`. Each stage writes its product to a durable path and returns a `{path, summary}` receipt, so the next stage enters cold with zero cache dependence — the property the recovery reference exploits.
- Recurring runs. A queue triage, a dependency sweep, a nightly audit is a saved workflow under an outer time- or goal-based loop; the workflow owns the deterministic inner pass, the outer loop owns recurrence and the stop. The stop is externally measurable — an empty queue, a merged PR, zero diagnostics, a score threshold; a vibes-based stop exits early or never.
- Idempotency law. The inner pass defaults to a no-op on an empty queue (`if (!work.length) return { skipped: true, reason: 'queue empty' }`) so a schedule tick costs nothing, and dedupes against durable state on disk — each run starts with a fresh cache, so anything persisting across ticks lives in a file an agent reads, never in script variables.
- Escalation seam. A recurring pass that discovers work beyond its charter files it as data for the operator (a triage list on disk), never expands its own scope mid-run.

## [23]-[BLACKBOARD]

Canonical: sectioning coordinated over evolving shared facts. Primitive: a typed, append-only board of fact files in run scratch — one namespaced write file per lane, declared read slices, a deterministic reducer stage. Cost: each lane pays one extra read of its declared slices; one reducer replaces pairwise peer messaging. Guards: the two failure modes of cross-lane coordination — broadcasting raw transcript (context bloat, accidental coupling to a sibling's reasoning) and writing one shared file (silent edit collisions). Use it when concurrent lanes must compose each other's LANDED facts mid-run — seam endpoints, frozen wire names, claimed territory — where the report-file contract's one-way fan ([21]) cannot carry the exchange.

- One board directory per run under run scratch; each lane appends to exactly ONE file it owns (`<scope>-<lane>-facts.json`), so writes never collide, and every sibling file is read-only to it.
- Rows are typed facts with anchors — a claim, a decision, an open question — landed only when a sibling must compose them; status narration never earns a row.
- A lane re-reads its declared slices immediately before acting on shared territory, never once at spawn — landed sibling facts compose as found; conflicts resolve in one reducer stage (evidence-first, a separate agent, its ruling written back to the board), never by lanes negotiating with each other.

A seam ledger is this contract's minimal form: typed fact rows per territory, read by area before any cross-territory edit. Distinct from [21]: a report file is a lane's terminal product for one downstream reader; a board row is a mid-run fact every live sibling may compose.

## [24]-[SCHEMAS]

A schema is a plain JSON Schema object. Keep them small, strict, and `required`-tight so the subagent returns exactly what the next line needs. Default to the strict profile — `additionalProperties: false` on every object, every property listed in `required`, conditional fields required-but-empty — so the same shape is copyable into a codex `--output-schema` lane without edits (validator split: api reference).

```js conceptual
const FINDINGS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['findings'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['title', 'file', 'line'],
                properties: { title: { type: 'string' }, file: { type: 'string' }, line: { type: 'integer' } },
            },
        },
    },
};
```

Define schemas in the body (after `meta`), as `const`s — never inside `meta`. The canonical PRODUCT, ANCHOR, and RECEIPT shapes at [21] are the worked deep instances of this profile.
