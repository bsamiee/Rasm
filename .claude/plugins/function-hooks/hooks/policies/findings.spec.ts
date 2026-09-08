import { describe, expect, it } from 'vitest';
import type { Entry, Finding } from '../host/store.ts';
import { batch, batchPrompt, DUE_MS, due, expiredFindings, finding, landed, questions, summarize } from './findings.ts';

const _NOW = 1_700_000_000_000;
const _RANDOM = 'random-uuid-tail';
const _INTENT_LINE = 2;

const _row = (kind: Finding['kind'], status: Finding['status'], change = 'drop the line'): Finding => ({
    session: 's',
    file: 'CLAUDE.md',
    section: '[01]',
    evidence: 'a:1 b:2',
    change,
    kind,
    status,
    proof: '',
    ts: _NOW,
});

const _ENTRIES: readonly Entry[] = [
    { key: 'findings/a', value: _row('stale', 'open') },
    { key: 'findings/b', value: _row('stale', 'open') },
    { key: 'findings/c', value: _row('coined', 'open') },
    { key: 'findings/d', value: _row('wrong', 'open-question', 'keep the row?') },
    { key: 'findings/e', value: { ..._row('wrong', 'landed'), ts: _NOW - DUE_MS - 1 } },
    { key: 'findings/f', value: 'row' },
];

describe('findings', () => {
    it('writes a row under a stamped key, open or open-question by the change', () => {
        const keyed = finding({
            session: 's',
            kind: 'stale',
            fields: { file: 'f', section: 's', evidence: 'e', change: 'keep?' },
            now: 0,
            random: _RANDOM,
        });
        expect(keyed.key).toBe('findings/1970-01-01T00:00:00.000Z-random-u');
        expect(keyed.row.status).toBe('open-question');
    });

    it('summarizes the open count and the question lines into the block', () => {
        const summary = summarize([_row('stale', 'open'), _row('wrong', 'open-question', 'keep the row?'), _row('wrong', 'landed')]);
        expect(summary).toStrictEqual({ open: 1, questions: ['- CLAUDE.md [01]: keep the row?'] });
        expect(questions(summary)?.text).toBe('Open questions from earlier findings, answer the ones you can:\n- CLAUDE.md [01]: keep the row?');
        expect(questions({ open: 1, questions: [] })).toBeUndefined();
    });

    it('expires landed rows past the window and reads due parts from absent, null, unparsed, or old stamps', () => {
        expect(expiredFindings(_ENTRIES, _NOW)).toStrictEqual(['findings/e']);
        const fresh = new Date(_NOW).toISOString();
        expect(due({ memory: fresh, rules: null, skills: 'x', agents: new Date(_NOW - DUE_MS - 1).toISOString() }, _NOW)).toStrictEqual([
            'rules',
            'skills',
            'agents',
            'hooks',
            'prose',
        ]);
    });

    it('batches the most frequent open kind, else the first due part, else nothing', () => {
        const kind = batch(_ENTRIES, {}, _NOW, _RANDOM);
        expect(kind?.dispatch).toStrictEqual({ spawnedAt: _NOW, rows: ['findings/a', 'findings/b'], kind: 'stale' });
        const part = batch([], {}, _NOW, _RANDOM);
        expect(part?.dispatch).toStrictEqual({ spawnedAt: _NOW, rows: [], part: 'memory' });
        const stamped = Object.fromEntries(
            ['memory', 'rules', 'skills', 'agents', 'hooks', 'prose'].map((name) => [name, new Date(_NOW).toISOString()]),
        );
        expect(batch([], stamped, _NOW, _RANDOM)).toBeUndefined();
    });

    it('prompts the orchestrator with the scope, the intent, and the rows, then lands the rows with the proof', () => {
        const named = batch(_ENTRIES, {}, _NOW, _RANDOM);
        const prompt = named === undefined ? '' : batchPrompt(named, _ENTRIES);
        expect(prompt.split('\n').slice(1, _INTENT_LINE + 1)).toStrictEqual([
            'scope: CLAUDE.md',
            'intent: land each finding below, a weakness of kind stale (Held once, and the tool, documentation, or repository moved on) whose move is Correct, verified by a run, a page, or history before the change',
        ]);
        expect(prompt.split('\n').at(-1)).toBe('- b CLAUDE.md [01]: a:1 b:2 | drop the line');
        const writes = named === undefined ? [] : landed(named, _ENTRIES, 'done', _NOW + 1);
        expect(writes.map((write) => write.key)).toStrictEqual(['findings/a', 'findings/b', 'summary']);
        expect(writes[0]?.value).toStrictEqual({ ..._row('stale', 'landed'), proof: 'done', ts: _NOW + 1 });
        expect(writes[2]?.value).toStrictEqual({ open: 1, questions: ['- CLAUDE.md [01]: keep the row?'] });
        const part = batch([], {}, _NOW, _RANDOM);
        expect(part === undefined ? '' : batchPrompt(part, [])).toContain('scope: the memory directory of this project');
    });
});
