// The brief block appended to a spawned agent's prompt by its type, the dispatch row, and the offer rows

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, type Rule, rewrite } from '../composition/decision.ts';
import { flatMap, fromNullable, fromPredicate, map, type Option, toArray } from '../composition/option.ts';
import { type Dispatch, isKindDispatch, isPartDispatch, type KindDispatch, type PartDispatch } from '../host/store.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface AgentRow {
    readonly subagentType: string;
    readonly brief: readonly string[];
    readonly dispatch?: true;
}

// The field is AgentOfferResult's own name, the row is the answer
interface OfferRow {
    readonly agent: string;
    readonly isOffered: false;
}

interface Spawn {
    readonly subagentType: string;
    readonly prompt: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

// Guidance agents sit under the plugin's agents/ and spawn by their plugin-scoped names
const EDITOR = 'function-hooks:guidance-editor';
const REVIEWER = 'function-hooks:guidance-reviewer';

// The lines every brief of the type shares, one sentence per line and one row per agent type, the prompt holds the task alone
const AGENTS = [
    {
        subagentType: 'fork',
        brief: [
            'Never rewrite a whole file in one move: read the file whole, write one scoped change, read the result.',
            'Read the skill the step names and the reference it touches before the first edit, apply each change as an exact-string replacement that asserts one match, and land any defect you meet in the same file in the same step.',
            'Write in clean-prose, the 150 width read with leeway per entry.',
            'Run the check the task names after the edit.',
            'Report the lines changed and any fact the task names that you failed to place, in at most 12 lines.',
        ],
    },
    {
        subagentType: 'general-purpose',
        brief: [
            'Never rewrite a whole file in one move: read, one scoped change, read.',
            'Correct only what you can justify, and report each correction as file, line, finding, and reason, in at most 20 lines.',
        ],
    },
    {
        subagentType: EDITOR,
        dispatch: true,
        brief: [
            "Read the rows the prompt lists, or through the close tool's list mode when that tool is listed.",
            "For each row, verify by a run, a page, or history, classify by the kinds table, decide the owner by the parts table, land one change in the owner's form under the five moves, and prove it with pnpm exec nx run function-hooks:test.",
            'For a cleaning batch, land each hit of the cleaning reference over the part named.',
            `Dispatch ${REVIEWER} over every delete, reframe, or move.`,
            'Set the verdict to open: <question> when the user must decide.',
            'End with your final message as one JSON object alone, the close input { batchId, rows: [{ id, verdict, landedFile, lines, proof }], cleaned? }, with cleaned stamping the part a cleaning batch names, or call the close tool with it when that tool is listed.',
        ],
    },
    {
        subagentType: REVIEWER,
        brief: [
            'Read the diff paths and the closed rows the editor names.',
            'Dispute each hunk that lost a fact, states a false one, or bound a category to an instance, with the evidence the editor needs to act.',
            'Change nothing.',
        ],
    },
] as const satisfies readonly AgentRow[];

// Agent types withheld from the model, one row per type
const OFFERS = [] as const satisfies readonly OfferRow[];

// --- [OPERATIONS] ----------------------------------------------------------------------

// The batch the timer dispatched, its id and the dispatch row, a kind batch over rows or a part the editor stamps
interface Batch {
    readonly batchId: string;
    readonly dispatch: Dispatch;
}

const _batchLines = (row: AgentRow, batch: Option<Batch>): readonly string[] =>
    toArray(flatMap(() => batch)(fromNullable(row.dispatch))).flatMap((named) => [
        `batchId: ${named.batchId}`,
        ...toArray(map((kind: KindDispatch) => `kind: ${kind.kind}`)(fromPredicate(isKindDispatch)(named.dispatch))),
        ...toArray(map((part: PartDispatch) => `part: ${part.part}`)(fromPredicate(isPartDispatch)(named.dispatch))),
    ]);

// Appends the brief of the spawned type and the batch line on a dispatch row, and an unknown type passes unchanged
const spawnRule =
    <E extends Spawn>(batch: Option<Batch>): Rule<E, never, never> =>
    (e: E): Decision<E, never, never> =>
        fromNullable(AGENTS.find((row) => row.subagentType === e.subagentType)).match<Decision<E, never, never>>({
            some: (row) =>
                rewrite({ ...e, prompt: [e.prompt, '', ...row.brief, ..._batchLines(row, batch)].join('\n') }, [
                    `Appended the ${row.subagentType} brief`,
                ]),
            none: () => rewrite(e, []),
        });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { AgentRow, Batch, OfferRow, Spawn };
export { AGENTS, EDITOR, OFFERS, REVIEWER, spawnRule };
