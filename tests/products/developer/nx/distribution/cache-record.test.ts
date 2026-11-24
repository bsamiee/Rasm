import { existsSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import type { PostTasksExecutionContext, TaskResult } from '@nx/devkit';
import { postTasksExecution } from '@rasm/nx-distribution';
import { it as nx } from '@rasm/testing/nx';
import { Array, Option, Record, Schema } from 'effect';
import { assert as holds, asyncProperty, boolean, constantFrom, nat, tuple, uniqueArray } from 'fast-check';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const RECORD = 'tmp/project-graph-cache/task-cache.jsonl';
const STATUSES = ['success', 'failure', 'skipped', 'stopped', 'local-cache-kept-existing', 'local-cache', 'remote-cache'] as const;
const run = [
    ['a:build--tsc', 'local-cache', true, 0.2],
    ['b:build--tsc', 'remote-cache', true, 0.4],
    ['c:check--tsc', 'local-cache-kept-existing', true, 0.1],
    ['d:build--tsc', 'success', true, 5],
    ['e:test--vitest', 'failure', true, 9],
    ['f:serve--vite', 'success', false, 30],
    ['g:test--vitest', 'skipped', true, 0],
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Line = Schema.fromJsonString(
    Schema.Struct({
        id: Schema.String,
        local: Schema.Int,
        remote: Schema.Int,
        missed: Schema.Int,
        targets: Schema.Record(Schema.String, Schema.Struct({ hits: Schema.Int, misses: Schema.Int })),
        failed: Schema.Array(Schema.Struct({ task: Schema.String })),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const finished = (id: string, rows: readonly (readonly [task: string, status: TaskResult['status'], cache: boolean, seconds: number])[]): PostTasksExecutionContext => ({
    id,
    workspaceRoot: '.',
    nxJsonConfiguration: {},
    argv: ['node', 'nx', 'run-many', '-t', 'build,test'],
    startTime: 1000,
    endTime: 61_000,
    taskResults: Object.fromEntries(
        rows.map(([task, status, cache, seconds]) => {
            const [project = '', target = ''] = task.split(':');
            return [task, { status, code: status === 'failure' ? 1 : 0, task: { id: task, target: { project, target }, overrides: {}, outputs: [], cache, startTime: 1000, endTime: 1000 + seconds * 1000 } }];
        }),
    ),
});

const lines = (root: string) => (existsSync(join(root, RECORD)) ? readFileSync(join(root, RECORD), 'utf-8').trim().split('\n') : []);

const last = (root: string) => Option.flatMap(Option.fromUndefinedOr(lines(root).at(-1)), Schema.decodeUnknownOption(Line));

const KIND: Readonly<Record<TaskResult['status'], 'local' | 'remote' | 'misses' | 'none'>> = { 'local-cache': 'local', 'local-cache-kept-existing': 'local', 'remote-cache': 'remote', success: 'misses', failure: 'misses', skipped: 'none', stopped: 'none' };

const reference = (rows: readonly (readonly [task: string, status: TaskResult['status'], cache: boolean, seconds: number])[]) => {
    const counted = rows.filter(([, status, cache]) => cache && KIND[status] !== 'none');
    const kinds = Array.groupBy(counted, ([, status]) => KIND[status]);
    const targets = Record.map(
        Array.groupBy(counted, ([task]) => task.slice(task.indexOf(':') + 1)),
        (held) => ({ hits: held.filter(([, status]) => KIND[status] !== 'misses').length, misses: held.filter(([, status]) => KIND[status] === 'misses').length }),
    );
    return { local: kinds['local']?.length ?? 0, remote: kinds['remote']?.length ?? 0, misses: kinds['misses']?.length ?? 0, targets, failed: rows.filter(([, status]) => status === 'failure' || status === 'stopped').map(([task]) => task) };
};

const timed = (id: string, cache: boolean, times: { readonly startTime?: number; readonly endTime: number }) => ({ id, target: { project: id.slice(0, id.indexOf(':')), target: id.slice(id.indexOf(':') + 1) }, overrides: {}, outputs: [], cache, ...times });

const tallied = (root: string) => Option.map(last(root), ({ id, local, remote, missed, targets, failed }) => ({ id, local, remote, misses: missed, targets, failed: failed.map(({ task }) => task) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('unannotated', { auto: true }, ({}, { onCleanup }) => {
    vi.stubEnv('GITHUB_ACTIONS', 'false');
    onCleanup(() => {
        vi.unstubAllEnvs();
    });
});

describe('task cache record after a run', () => {
    it('counts local and remote hits against misses of cacheable tasks alone, by target, with the slowest misses first', async ({ workspace }) => {
        await postTasksExecution({}, finished('run-1', run));

        expect(JSON.parse(lines(workspace.root).at(-1) ?? 'null')).toMatchObject({
            id: 'run-1',
            seconds: 60,
            local: 2,
            remote: 1,
            missed: 2,
            rate: 0.6,
            targets: { 'build--tsc': { hits: 2, misses: 1 }, 'check--tsc': { hits: 1, misses: 0 }, 'test--vitest': { hits: 0, misses: 1 } },
            slowest: [
                { task: 'e:test--vitest', status: 'failure', seconds: 9 },
                { task: 'd:build--tsc', status: 'success', seconds: 5 },
            ],
        });
    });

    it('agrees with an independent count over any run, each target summing to the run', async ({ workspace }) => {
        const row = tuple(constantFrom('a:build--tsc', 'b:build--tsc', 'c:build--tsc', 'a:test--vitest', 'b:test--vitest', 'a:check--tsc'), constantFrom(...STATUSES), boolean(), nat(90));
        await holds(
            asyncProperty(uniqueArray(row, { minLength: 1, selector: ([task]) => task }), async (rows) => {
                const before = lines(workspace.root).length;
                const expected = reference(rows);

                await postTasksExecution({}, finished(`run-${before}`, rows));

                expect(lines(workspace.root).length > before ? tallied(workspace.root) : Option.none()).toStrictEqual(expected.local + expected.remote + expected.misses + expected.failed.length === 0 ? Option.none() : Option.some({ id: `run-${before}`, ...expected }));
            }),
            { numRuns: 60 },
        );
    });

    it('names every failed or stopped task with its status and exit code, stating a duration only for a task holding both times', async ({ workspace }) => {
        await postTasksExecution(
            {},
            {
                ...finished('run-failed', []),
                taskResults: {
                    'a:build--dotnet': { status: 'failure', code: 1, task: timed('a:build--dotnet', true, { endTime: 9000 }) },
                    'b:serve--vite': { status: 'stopped', code: 130, task: timed('b:serve--vite', false, { startTime: 1000, endTime: 4000 }) },
                    'c:test--vitest': { status: 'failure', code: 2, task: timed('c:test--vitest', true, { startTime: 1000, endTime: 10_000 }) },
                    'd:build--tsc': { status: 'success', code: 0, task: timed('d:build--tsc', true, { endTime: 9000 }) },
                },
            },
        );

        const line: unknown = JSON.parse(lines(workspace.root).at(-1) ?? 'null');
        expect(line).toStrictEqual(
            expect.objectContaining({
                failed: [
                    { task: 'a:build--dotnet', status: 'failure', code: 1 },
                    { task: 'b:serve--vite', status: 'stopped', code: 130, seconds: 3 },
                    { task: 'c:test--vitest', status: 'failure', code: 2, seconds: 9 },
                ],
                slowest: [{ task: 'c:test--vitest', status: 'failure', seconds: 9 }],
            }),
        );
    });

    it.for([
        { on: 'true', printed: ['::error title=e%3Atest--vitest::e:test--vitest ended in failure with exit code 1'] },
        { on: 'false', printed: [] },
    ])('prints an error workflow command per failed task where GITHUB_ACTIONS is $on', async ({ on, printed }, { workspace, onTestFinished }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onTestFinished(() => {
            log.mockRestore();
        });
        vi.stubEnv('GITHUB_ACTIONS', on);

        await postTasksExecution({}, finished('run-annotated', run));

        expect([log.mock.calls.map((call) => String(call.at(-1))), lines(workspace.root).length]).toStrictEqual([printed, 1]);
    });

    it('writes nothing for a run holding no cacheable task', async ({ workspace }) => {
        await postTasksExecution({}, finished('run-idle', [['f:serve--vite', 'success', false, 30]]));

        expect(lines(workspace.root)).toStrictEqual([]);
    });

    it('adds one line to the job summary GITHUB_STEP_SUMMARY names', async ({ workspace }) => {
        const summary = join(workspace.root, 'summary.md');
        writeFileSync(summary, '# Earlier step\n');
        vi.stubEnv('GITHUB_STEP_SUMMARY', summary);

        await postTasksExecution({}, finished('run-ci', run));

        expect(readFileSync(summary, 'utf-8')).toMatch(/^# Earlier step\n[^\n]*\b3 of 5 cacheable tasks\b[^\n]*\b60\s?%[^\n]*e:test--vitest[^\n]*\n$/u);
    });

    it('keeps the newest runs and drops the oldest once the record outgrows its bound', async ({ workspace }) => {
        const old = Array.makeBy(40_000, (index) => JSON.stringify({ id: `old-${index}`, pad: 'x'.repeat(60) }));
        workspace.write({ [RECORD]: `${old.join('\n')}\n` });

        await postTasksExecution({}, finished('run-new', run));

        const kept = lines(workspace.root);
        expect([statSync(join(workspace.root, RECORD)).size <= 1_048_576, kept.length > 1, Option.map(last(workspace.root), ({ id }) => id), kept.slice(0, -1)]).toStrictEqual([true, true, Option.some('run-new'), old.slice(old.length - (kept.length - 1))]);
    });
});
