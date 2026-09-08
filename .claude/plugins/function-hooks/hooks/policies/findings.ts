// Pure decisions over the findings rows and the cleaned stamps for the classifier, the open-question block, the band, and the dispatch

// --- [IMPORTS] -------------------------------------------------------------------------

import type { InvalidatableEventName } from 'claude-code';
import {
    type Cleaned,
    type Dispatch,
    type Entry,
    type Finding,
    findings,
    id,
    type Kind,
    key,
    PART_NAMES,
    type Part,
    type Summary,
    suffix,
} from '../host/store.ts';
import { type Fields, KINDS } from './kinds.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface FindingInput {
    readonly session: string;
    readonly kind: Kind;
    readonly fields: Fields;
    readonly now: number;
    readonly random: string;
}

// A batch the session start dispatches, its id and the dispatch row under dispatch/<batchId>
interface Batch {
    readonly batchId: string;
    readonly dispatch: Dispatch;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MS_PER_DAY = 86_400_000;
const _DUE_DAYS = 30;
// The window a landed row stays in the store, a cleaned stamp holds, and a dispatch row holds the next start
const DUE_MS = _DUE_DAYS * MS_PER_DAY;
const _QUESTION_HEADER = 'Open questions from earlier findings, answer the ones you can:';
// The agent one dispatch spawns over a batch, its prompt holds the scope, the intent, and the rows
const ORCHESTRATOR = 'system-orchestrator';
// The scope a cleaning batch names per part
const _PART_SCOPE: Readonly<Record<Part, string>> = {
    memory: 'the memory directory of this project',
    rules: '.claude/rules',
    skills: '.claude/skills and .claude/plugins/*/skills',
    agents: '.claude/agents and .claude/plugins/*/agents',
    hooks: '.claude/plugins/function-hooks',
    prose: 'CLAUDE.md, README.md, and docs',
};

// The cached answers a finding change invalidates, the band and the open-question block both read the summary row
const FINDING_VIEWS: readonly InvalidatableEventName[] = ['ui.render', 'prompt.context'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isOpen = (row: Finding): boolean => row.status === 'open' || row.status === 'open-question';

// One findings/<id> row from the classifier's fields, a change that ends in a question mark waits for the user's answer
const finding = (input: FindingInput): Entry & { readonly row: Finding } => {
    const row: Finding = {
        session: input.session,
        file: input.fields.file,
        section: input.fields.section,
        evidence: input.fields.evidence,
        change: input.fields.change,
        kind: input.kind,
        status: input.fields.change.endsWith('?') ? 'open-question' : 'open',
        proof: '',
        ts: input.now,
    };
    return { key: key('findings', id(input.now, input.random)), value: row, row };
};

// The summary row over every findings row, the snapshot the band and the open-question block read in place of a scan
const summarize = (rows: readonly Finding[]): Summary => ({
    open: rows.filter((row) => row.status === 'open').length,
    questions: rows.filter((row) => row.status === 'open-question').map((row) => `- ${row.file} ${row.section}: ${row.change}`),
});

// The prompt.context block over the summary's question lines, undefined when there is none
const questions = (summary: Summary): { readonly name: string; readonly text: string } | undefined =>
    summary.questions.length > 0 ? { name: 'openQuestions', text: [_QUESTION_HEADER, ...summary.questions].join('\n') } : undefined;

// The findings/ keys of landed or closed rows older than the due window, the prune that keeps one window of closed history
const expiredFindings = (entries: readonly Entry[], now: number): readonly string[] =>
    findings(entries)
        .filter((entry) => !_isOpen(entry.row) && now - entry.row.ts > DUE_MS)
        .map((entry) => entry.key);

// Parts are due when their stamp is absent, null, unparsed, or older than the due window
const due = (cleaned: Cleaned, now: number): readonly Part[] =>
    PART_NAMES.filter((part) => {
        const stamp = cleaned[part];
        return stamp === undefined || stamp === null || !(now - Date.parse(stamp) <= DUE_MS);
    });

// The batch a start dispatches: the open rows of the most frequent kind, else the first due part, undefined with neither
const batch = (entries: readonly Entry[], cleaned: Cleaned, now: number, random: string): Batch | undefined => {
    const open = findings(entries).filter((entry) => entry.row.status === 'open');
    const kinds = [...new Set(open.map((entry) => entry.row.kind))];
    const count = (name: Kind): number => open.filter((entry) => entry.row.kind === name).length;
    const [kind] = kinds.toSorted((left, right) => count(right) - count(left));
    const [part] = due(cleaned, now);
    if (kind !== undefined) {
        return {
            batchId: id(now, random),
            dispatch: { spawnedAt: now, rows: open.filter((entry) => entry.row.kind === kind).map((entry) => entry.key), kind },
        };
    }
    return part === undefined ? undefined : { batchId: id(now, random), dispatch: { spawnedAt: now, rows: [], part } };
};

// The orchestrator's prompt over a batch, the scope and intent its contract names, and one line per row of a kind batch
const batchPrompt = (named: Batch, entries: readonly Entry[]): string => {
    if ('part' in named.dispatch) {
        return [
            `Batch ${named.batchId}`,
            `scope: ${_PART_SCOPE[named.dispatch.part]}`,
            `intent: clean the ${named.dispatch.part} guidance under the clean-prose skill, every fact kept once in its owning file`,
        ].join('\n');
    }
    const keyed: readonly string[] = named.dispatch.rows;
    const rows = findings(entries).filter((entry) => keyed.includes(entry.key));
    const row = KINDS[named.dispatch.kind];
    return [
        `Batch ${named.batchId}`,
        `scope: ${[...new Set(rows.map((entry) => entry.row.file))].join(', ')}`,
        `intent: land each finding below, a weakness of kind ${named.dispatch.kind} (${row.criterion}) whose move is ${row.move}, verified by a run, a page, or history before the change`,
        ...rows.map(
            (entry) => `- ${suffix('findings')(entry.key)} ${entry.row.file} ${entry.row.section}: ${entry.row.evidence} | ${entry.row.change}`,
        ),
    ].join('\n');
};

// The rows of a settled batch marked landed with the report's first line as proof, and the summary over every row after them
const landed = (named: Batch, entries: readonly Entry[], proof: string, now: number): readonly Entry[] => {
    const keyed: readonly string[] = named.dispatch.rows;
    const rows = findings(entries).map((entry) =>
        keyed.includes(entry.key) ? { key: entry.key, row: { ...entry.row, status: 'landed' as const, proof, ts: now } } : entry,
    );
    return [
        ...rows.flatMap((entry): readonly Entry[] => (keyed.includes(entry.key) ? [{ key: entry.key, value: entry.row }] : [])),
        { key: key('summary'), value: summarize(rows.map((entry) => entry.row)) },
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Batch, FindingInput };
export { batch, batchPrompt, DUE_MS, due, expiredFindings, FINDING_VIEWS, finding, landed, MS_PER_DAY, ORCHESTRATOR, questions, summarize };
