// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { fromBoolean, none, type Option, some } from '../composition/option.ts';
import { AGENTS, type Batch, EDITOR, REVIEWER, type Spawn, spawnRule } from './agents.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The rewritten prompt read as the task before its first blank line and the lines appended after it
type Plain =
    | { readonly kind: 'rewrite'; readonly task: string; readonly appended: readonly string[]; readonly context: readonly string[] }
    | { readonly kind: 'other' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TASK = 'Shorten the entry.';
const _BATCH_ID = '2026-09-06T00:00:00.000Z-random-u';
const _FORK_WIDTH_LINE = 'Write in clean-prose, the 150 width read with leeway per entry.';
const _NO_BATCH = none<never>();

// The lines a file-less worker's prompt gains, the one owner of its report shape
const _FORK_LINES = [
    'Never rewrite a whole file in one move: read the file whole, write one scoped change, read the result.',
    'Read the skill the step names and the reference it touches before the first edit, apply each change as an exact-string replacement that asserts one match, and land any defect you meet in the same file in the same step.',
    _FORK_WIDTH_LINE,
    'Report result: done, partial, or not started, changes: with file and line, open: with the evidence, gate: with each check the task names and its result line, and suggestions: on the brief or a preloaded skill, in at most 12 lines.',
];
const _GENERAL_PURPOSE_LINES = [
    'Never rewrite a whole file in one move: read, one scoped change, read.',
    'Correct only what you can justify, and report result: done, partial, or not started, changes: with file, line, and reason, open: with the evidence, gate: with each check the task names and its result line, and suggestions: on the brief or a preloaded skill, in at most 20 lines.',
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// The task before the first blank line and the lines after it, a prompt with no blank line is the task alone
const _split = (prompt: string): { readonly task: string; readonly appended: readonly string[] } => {
    const blank = prompt.indexOf('\n\n');
    return fromBoolean(blank >= 0).match<{ readonly task: string; readonly appended: readonly string[] }>({
        some: () => ({ task: prompt.slice(0, blank), appended: prompt.slice(blank + 2).split('\n') }),
        none: () => ({ task: prompt, appended: [] }),
    });
};

const _plain = (decision: Decision<Spawn, never, never>): Plain =>
    decision.match<Plain>({
        rewrite: (e, context) => ({ kind: 'rewrite', ..._split(e.prompt), context }),
        deny: () => ({ kind: 'other' }),
        answer: () => ({ kind: 'other' }),
    });

const _brief = (subagentType: string): readonly string[] => AGENTS.find((row) => row.subagentType === subagentType)?.brief ?? [];

// --- [TESTS] ---------------------------------------------------------------------------

describe('AGENTS', () => {
    it('holds the four briefs with the width line the guidance plan states', () => {
        expect(AGENTS.map((row) => row.subagentType)).toStrictEqual(['fork', 'general-purpose', EDITOR, REVIEWER]);
        expect(_brief('fork')).toContain(_FORK_WIDTH_LINE);
        expect(_brief(EDITOR).some((line) => line.includes(REVIEWER))).toBe(true);
    });
});

describe('spawnRule', () => {
    it('appends the fork lines after the task and names the brief in the context', () => {
        expect(_plain(spawnRule<Spawn>(_NO_BATCH)({ subagentType: 'fork', prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: _FORK_LINES,
            context: ['Appended the fork brief'],
        });
    });

    it('appends the general-purpose lines after the task', () => {
        expect(_plain(spawnRule<Spawn>(_NO_BATCH)({ subagentType: 'general-purpose', prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: _GENERAL_PURPOSE_LINES,
            context: ['Appended the general-purpose brief'],
        });
    });

    it('adds the batch and kind lines on the dispatch row alone', () => {
        const batch = some({ batchId: _BATCH_ID, dispatch: { rows: ['findings/a'], spawnedAt: 0, kind: 'stale' as const } });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: EDITOR, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: [..._brief(EDITOR), `batchId: ${_BATCH_ID}`, 'kind: stale'],
            context: [`Appended the ${EDITOR} brief`],
        });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: REVIEWER, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: _brief(REVIEWER),
            context: [`Appended the ${REVIEWER} brief`],
        });
    });

    it('adds the batch and part lines on a part batch', () => {
        const batch: Option<Batch> = some({ batchId: _BATCH_ID, dispatch: { rows: [], spawnedAt: 0, part: 'memory' } });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: EDITOR, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: [..._brief(EDITOR), `batchId: ${_BATCH_ID}`, 'part: memory'],
            context: [`Appended the ${EDITOR} brief`],
        });
    });

    it('passes an unknown type unchanged', () => {
        expect(_plain(spawnRule<Spawn>(_NO_BATCH)({ subagentType: 'Explore', prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            task: _TASK,
            appended: [],
            context: [],
        });
    });
});
