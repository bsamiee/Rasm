import { mkdtempSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import type { TaskGraph } from '@nx/devkit';
import { batch, execute } from '@rasm/nx-apple/executors/run';
import { it, shimmed, spanned, type Workspace } from '@rasm/testing/nx';
import { Array, Record } from 'effect';
import { assert, asyncProperty, integer, record, subarray, tuple } from 'fast-check';
import { describe, expect, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EMPTY = { nodes: {}, dependencies: {} };
const plan = integer({ min: 1, max: 5 }).chain((count) =>
    record({
        needs: tuple(...Array.makeBy(count, (index) => subarray(Array.take(Array.range(0, count), index)))),
        failing: subarray(Array.take(Array.range(0, count), count)),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const id = (index: number) => `P${index}:build--swift-build`;

const scheduled = async (workspace: Workspace, { needs, failing }: { readonly needs: readonly (readonly number[])[]; readonly failing: readonly number[] }) => {
    const trace = join(mkdtempSync(join(workspace.root, 'trace-')), 'trace.log');
    const blocked = (index: number): boolean => needs[index]?.some((need) => failing.includes(need) || blocked(need)) ?? false;
    const indices = needs.keys().toArray();
    const graph: TaskGraph = {
        tasks: Record.fromIterableWith(indices, (index) => [id(index), { id: id(index), target: { project: `P${index}`, target: 'build--swift-build' }, outputs: [], overrides: {}, projectRoot: `P${index}`, parallelism: true, cache: true }]),
        dependencies: Record.fromIterableWith(indices, (index) => [id(index), (needs[index] ?? []).map((need) => ({ id: id(need) }))]),
        continuousDependencies: {},
        roots: indices.filter((index) => needs[index]?.length === 0).map(id),
    };
    const inputs = Record.fromIterableWith(indices.toReversed(), (index) => [id(index), { runs: [['sh', '-c', `echo start-${index} >> "$0"; echo end-${index} >> "$0"; exit ${failing.includes(index) ? 1 : 0}`, trace]] }]);

    const results = await globalThis.Array.fromAsync(batch(graph, inputs, {}, workspace.context(EMPTY, 'P0', 'build--swift-build')));

    const lines = readFileSync(trace, 'utf-8').split('\n');
    const ran = indices.filter((index) => !blocked(index));
    const early = ran.flatMap((index) => (needs[index] ?? []).filter((need) => !lines.includes(`end-${need}`) || lines.indexOf(`end-${need}`) > lines.indexOf(`start-${index}`)).map((need) => `${index} started before ${need} ended`));
    expect({ results: Record.fromEntries(results.map(({ task, result: { success, status } }) => [task, [success, status ?? 'ran']])), started: lines.filter((line) => line.startsWith('start-')).toSorted(), early }).toStrictEqual({
        results: Record.fromIterableWith(indices, (index) => [id(index), blocked(index) ? [false, 'skipped'] : [!failing.includes(index), 'ran']]),
        started: ran.map((index) => `start-${index}`).toSorted(),
        early: [],
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('run batch', { timeout: 120_000 }, () => {
    it('runs each task after every task it depends on, and skips each task a failure reaches through its dependencies', async ({ workspace }) => {
        await assert(
            asyncProperty(plan, (planned) => scheduled(workspace, planned)),
            { numRuns: 25 },
        );
    });

    it('stamps each task that ran with the span of its own invocations inside the batch, and no span on a task it skips', async ({ workspace }) => {
        const graph: TaskGraph = {
            tasks: Record.fromIterableWith([0, 1, 2, 3], (index) => [id(index), { id: id(index), target: { project: `P${index}`, target: 'build--swift-build' }, outputs: [], overrides: {}, projectRoot: `P${index}`, parallelism: true, cache: true }]),
            dependencies: { [id(0)]: [], [id(1)]: [{ id: id(0) }], [id(2)]: [], [id(3)]: [{ id: id(2) }] },
            continuousDependencies: {},
            roots: [id(0), id(2)],
        };
        const before = vi.getRealSystemTime();

        const results = Record.fromIterableWith(
            await globalThis.Array.fromAsync(batch(graph, { [id(0)]: { runs: [['sleep', '0.2']] }, [id(1)]: { runs: [['sleep', '0.2']] }, [id(2)]: { runs: [['false']] }, [id(3)]: { runs: [['true']] } }, {}, workspace.context(EMPTY, 'P0', 'build--swift-build'))),
            ({ task, result }) => [task, result] as const,
        );

        const after = vi.getRealSystemTime();
        expect(results).toStrictEqual({ [id(0)]: spanned(before, after), [id(1)]: spanned(results[id(0)]?.endTime ?? after, after), [id(2)]: spanned(before, after), [id(3)]: { success: false, status: 'skipped', terminalOutput: '' } });
    });
});

describe('run coverage report', { timeout: 60_000 }, () => {
    it.for([
        { stated: 'no result bundle pattern', runs: [['true']], report: { profiles: ['*.profdata'], objects: ['Suite.xctest'], file: 'lcov.info' }, refusal: /^No profile matches \*\.profdata after the invocations$/u },
        { stated: 'no instrumented binary', runs: [['touch', 'default.profdata']], report: { profiles: ['*.profdata'], results: ['*.xcresult'], file: 'lcov.info' }, refusal: /^No instrumented binary is stated or reported by a result bundle matching \*\.xcresult after the invocations$/u },
    ])('reads a report with $stated as one stating none, and names what the invocations left unwritten', async ({ runs, report, refusal }, { workspace }) => {
        await expect(execute({ runs, report }, workspace.context(EMPTY, 'Viewer', 'test--xcodebuild'))).rejects.toThrow(refusal);
    });
});

describe('run install notice', { timeout: 60_000 }, () => {
    it.for([
        { case: 'runs from inside its bundle', running: '/Applications/Viewer.app/Contents/MacOS/Viewer', notices: ['Viewer runs the build it launched from, relaunch it to run this one'] },
        { case: 'runs from a bundle whose name ends in its name', running: '/Applications/MyViewer.app/Contents/MacOS/Viewer', notices: [] },
    ])('asks for a relaunch after an install only where the installed application $case', async ({ running, notices }, { workspace, onTestFinished }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        onTestFinished(() => {
            log.mockRestore();
        });
        shimmed(join(workspace.root, 'bin'), { ps: `printf '%s\\n' /sbin/launchd '${running}'` });

        const { success } = await execute({ runs: [['true']], applications: ['Viewer.app'] }, workspace.context(EMPTY, 'Viewer', 'install--xcodebuild'));

        expect({ success, notices: log.mock.calls.map((call) => String(call.at(-1))) }).toStrictEqual({ success: true, notices });
    });
});
