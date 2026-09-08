// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { none, type Option, some } from '../composition/option.ts';
import { AGENTS, type Batch, EDITOR, REVIEWER, type Spawn, spawnRule } from './agents.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain = { readonly kind: 'rewrite'; readonly prompt: string; readonly context: readonly string[] } | { readonly kind: 'other' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TASK = 'Shorten the entry.';
const _BATCH_ID = '2026-09-06T00:00:00.000Z-random-u';
const _FORK_WIDTH_LINE = 'Write in clean-prose, the 150 width read with leeway per entry.';
const _NO_BATCH = none<never>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = (decision: Decision<Spawn, never, never>): Plain =>
    decision.match<Plain>({
        rewrite: (e, context) => ({ kind: 'rewrite', prompt: e.prompt, context }),
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
    it('appends the brief of the spawned type after the task and names it in the context', () => {
        expect(_plain(spawnRule<Spawn>(_NO_BATCH)({ subagentType: 'fork', prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            prompt: [_TASK, '', ..._brief('fork')].join('\n'),
            context: ['Appended the fork brief'],
        });
    });

    it('adds the batch and kind lines on the dispatch row alone', () => {
        const batch = some({ batchId: _BATCH_ID, dispatch: { rows: ['findings/a'], spawnedAt: 0, kind: 'stale' as const } });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: EDITOR, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            prompt: [_TASK, '', ..._brief(EDITOR), `batchId: ${_BATCH_ID}`, 'kind: stale'].join('\n'),
            context: [`Appended the ${EDITOR} brief`],
        });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: REVIEWER, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            prompt: [_TASK, '', ..._brief(REVIEWER)].join('\n'),
            context: [`Appended the ${REVIEWER} brief`],
        });
    });

    it('adds the batch and part lines on a part batch', () => {
        const batch: Option<Batch> = some({ batchId: _BATCH_ID, dispatch: { rows: [], spawnedAt: 0, part: 'memory' } });
        expect(_plain(spawnRule<Spawn>(batch)({ subagentType: EDITOR, prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            prompt: [_TASK, '', ..._brief(EDITOR), `batchId: ${_BATCH_ID}`, 'part: memory'].join('\n'),
            context: [`Appended the ${EDITOR} brief`],
        });
    });

    it('passes an unknown type unchanged', () => {
        expect(_plain(spawnRule<Spawn>(_NO_BATCH)({ subagentType: 'Explore', prompt: _TASK }))).toStrictEqual({
            kind: 'rewrite',
            prompt: _TASK,
            context: [],
        });
    });
});
