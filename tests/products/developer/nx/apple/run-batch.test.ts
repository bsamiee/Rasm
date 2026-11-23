import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { batch, execute } from '@rasm/nx-apple/executors/run';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const task = (project: string, target: string) => ({ id: `${project}:${target}`, target: { project, target }, outputs: [], overrides: {}, projectRoot: project, parallelism: true, cache: true });
const logged = (name: string, code: number) => ({ runs: [['sh', '-c', `echo start-${name} >> order.log; sleep 0.2; echo end-${name} >> order.log; exit ${code}`]] });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('run batch', { timeout: 60_000 }, () => {
    it('runs each batched task alone in dependency order and skips a task whose dependency failed', async ({ workspace }) => {
        const context: ExecutorContext = { root: workspace.root, cwd: workspace.root, isVerbose: false, projectGraph: { nodes: {}, dependencies: {} }, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {} };
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
        expect(readFileSync(join(workspace.root, 'order.log'), 'utf-8')).toBe(['start-tool-build', 'end-tool-build', 'start-tool-test', 'end-tool-test', 'start-kit-build', 'end-kit-build', ''].join('\n'));
    });
});

describe('run coverage report', { timeout: 60_000 }, () => {
    it.for([
        { stated: 'no result bundle pattern', runs: [['true']], report: { profiles: ['*.profdata'], objects: ['Suite.xctest'], file: 'lcov.info' }, refusal: /^No profile matches \*\.profdata after the invocations$/u },
        { stated: 'no instrumented binary', runs: [['touch', 'default.profdata']], report: { profiles: ['*.profdata'], results: ['*.xcresult'], file: 'lcov.info' }, refusal: /^No instrumented binary is stated or reported by a result bundle matching \*\.xcresult after the invocations$/u },
    ])('reads a report with $stated as one stating none, and names what the invocations left unwritten', async ({ runs, report, refusal }, { workspace }) => {
        const context: ExecutorContext = { root: workspace.root, cwd: workspace.root, isVerbose: false, projectGraph: { nodes: {}, dependencies: {} }, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {} };

        await expect(execute({ runs, report }, context)).rejects.toThrow(refusal);
    });
});
