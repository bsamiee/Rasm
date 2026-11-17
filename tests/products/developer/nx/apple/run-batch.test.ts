import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { batch, type Run } from '@rasm/nx-apple/executors/run';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const task = (project: string, target: string) => ({ id: `${project}:${target}`, target: { project, target }, outputs: [], overrides: {}, projectRoot: project, parallelism: true, cache: true });
const logged = (name: string, code: number): typeof Run.Type => ({ runs: [['sh', '-c', `echo start-${name} >> order.log; sleep 0.2; echo end-${name} >> order.log; exit ${code}`]] });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('apple-run-batch');
        onCleanup(() => tempFs.cleanup());
        return tempFs;
    })
    .extend('context', ({ tempFs }): ExecutorContext => ({ root: tempFs.tempDir, cwd: tempFs.tempDir, isVerbose: false, projectGraph: { nodes: {}, dependencies: {} }, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {} }));

describe('run batch', { timeout: 60_000 }, () => {
    it('runs each batched task alone in dependency order and skips a task whose dependency failed', async ({ tempFs, context }) => {
        const graph: TaskGraph = {
            tasks: { 'Tool:test--swift': task('Tool', 'test--swift'), 'Tool:build--swift-build': task('Tool', 'build--swift-build'), 'Kit:build--swift-build': task('Kit', 'build--swift-build'), 'Kit:test--swift': task('Kit', 'test--swift') },
            dependencies: { 'Tool:test--swift': [{ id: 'Tool:build--swift-build' }], 'Tool:build--swift-build': [], 'Kit:build--swift-build': [], 'Kit:test--swift': [{ id: 'Kit:build--swift-build' }] },
            continuousDependencies: {},
            roots: ['Tool:build--swift-build', 'Kit:build--swift-build'],
        };
        const inputs = { 'Tool:test--swift': logged('tool-test', 0), 'Tool:build--swift-build': logged('tool-build', 0), 'Kit:build--swift-build': logged('kit-build', 1), 'Kit:test--swift': logged('kit-test', 0) };

        const results = await Array.fromAsync(batch(graph, inputs, {}, context));

        expect(results.map(({ task: id, result: { success, status } }) => [id, success, status])).toStrictEqual([
            ['Tool:build--swift-build', true, undefined],
            ['Tool:test--swift', true, undefined],
            ['Kit:build--swift-build', false, undefined],
            ['Kit:test--swift', false, 'skipped'],
        ]);
        await expect(tempFs.readFile('order.log')).resolves.toBe(['start-tool-build', 'end-tool-build', 'start-tool-test', 'end-tool-test', 'start-kit-build', 'end-kit-build', ''].join('\n'));
    });
});
