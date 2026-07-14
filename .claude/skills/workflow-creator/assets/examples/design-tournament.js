/**
 * design-tournament — draft one plan per strategy angle, rank the field through a blind
 * single-elimination bracket of pairwise judges, then graft the runners-up into the winner.
 *
 * Demonstrates the tournament shape: comparative picks between two concrete artifacts are
 * stable where absolute 1-10 scores drift; the judge sees only artifacts and criteria
 * (never provenance or prior verdicts); presentation order alternates by pair index so a
 * fixed A-first ordering cannot leak a position preference; provenance unblinds only at
 * the terminal graft, after every comparison is done.
 *
 * Workflow({ name: 'design-tournament',
 *            args: { question: 'How should the artifact cache invalidate?', criteria: 'correctness under concurrency; operational simplicity' } })
 */

export const meta = {
    name: 'design-tournament',
    description: 'Draft one plan per strategy angle, rank the drafts through a blind pairwise bracket, graft the runners-up into the winner',
    whenToUse: 'A wide or close design field where absolute scoring drifts and only relative quality is trustworthy',
    phases: [{ title: 'Draft', detail: 'one plan per angle' }, { title: 'Bracket', detail: 'blind pairwise judges' }, { title: 'Graft' }],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANGLES = ['simplest-correct', 'performance-first', 'operations-first', 'evolution-first'];

// --- [INPUTS] --------------------------------------------------------------------------

// Structured args — read fields directly; default the omitted case to a safe representative question.
const question = typeof args?.question === 'string' && args.question.trim() ? args.question : 'How should the artifact cache invalidate?';
const criteria = typeof args?.criteria === 'string' && args.criteria.trim() ? args.criteria : 'correctness; simplicity; failure behavior';

// --- [MODELS] --------------------------------------------------------------------------

// STRICT: additionalProperties:false + every property required; the enum forces a decision.
const PICK = {
    type: 'object',
    additionalProperties: false,
    required: ['winner', 'why'],
    properties: { winner: { type: 'string', enum: ['A', 'B'] }, why: { type: 'string' } },
};

// --- [COMPOSITION] ---------------------------------------------------------------------

phase('Draft');
const drafts = (
    await parallel(
        ANGLES.map(
            (a, i) => () =>
                agent(`Produce the strongest plan for the question below from a strictly ${a} stance.\n\nQUESTION: ${question}`, {
                    label: `draft:${i}`,
                    phase: 'Draft',
                }),
        ),
    )
).filter(Boolean);
if (drafts.length < 2) return { final: drafts[0] ?? null, note: 'fewer than two drafts survived — nothing to rank' };

// Single-elimination bracket: N-1 judge calls for N drafts, each a blind pairwise pick.
phase('Bracket');
let pool = drafts.map((body, id) => ({ id, body }));
let round = 0;
while (pool.length > 1) {
    round++;
    const pairs = [];
    for (let i = 0; i + 1 < pool.length; i += 2) pairs.push([pool[i], pool[i + 1]]);
    const bye = pool.length % 2 ? [pool[pool.length - 1]] : [];
    const winners = await parallel(
        pairs.map(([a, b], i) => () => {
            const [first, second] = i % 2 ? [b, a] : [a, b]; // alternate order — cancels position bias
            return agent(
                'Pick the stronger submission strictly on the criteria. The labels are arbitrary and carry no meaning.\n\nCRITERIA:\n' +
                    criteria +
                    '\n\nA:\n' +
                    first.body +
                    '\n\nB:\n' +
                    second.body,
                { label: `judge:${round}:${i}`, phase: 'Bracket', schema: PICK, effort: 'high' },
            ).then((v) => (v?.winner === 'A' ? first : second));
        }),
    );
    pool = winners.filter(Boolean).concat(bye);
    if (!pool.length) return { final: null, note: 'every judge in a round was skipped — bracket abandoned' };
}

// Provenance unblinds only here, after every comparison is done.
phase('Graft');
const final = await agent(
    'Improve the WINNER by grafting the strongest members of the runners-up; change nothing that already works.\n\nWINNER:\n' +
        pool[0].body +
        '\n\nRUNNERS-UP:\n' +
        drafts.filter((d) => d !== pool[0].body).join('\n---\n'),
    { label: 'graft', phase: 'Graft', effort: 'high' },
);
return { final, rounds: round, drafts: drafts.length };
