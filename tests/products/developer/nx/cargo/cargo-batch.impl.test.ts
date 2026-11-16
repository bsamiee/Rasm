import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import { batch, type Cargo } from '@rasm/nx-cargo/executors/cargo';
import { TempFs } from '@rasm/testing/temp-fs';
import { ChildProcess } from 'effect/process';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('effect/process'), { spy: true });

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUILD = ['build', '--keep-going', '--message-format', 'json-diagnostic-rendered-ansi'] as const;
const NEXTEST = ['nextest', 'run', '--locked', '--cargo-message-format', 'json-diagnostic-rendered-ansi', '--message-format', 'libtest-json-plus', '--message-format-version', '0.1'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const task = (project: string, target: string) => ({ id: `${project}:${target}`, target: { project, target }, outputs: [], overrides: {}, projectRoot: project, parallelism: false, cache: false });
const batchTaskGraph: TaskGraph = {
    tasks: {
        'app2:build': task('app2', 'build'),
        'app1:build': task('app1', 'build'),
    },
    dependencies: {
        'app2:build': [],
        'app1:build': [{ id: 'app2:build' }],
    },
    continuousDependencies: {},
    roots: ['app2:build'],
};
const suites = (project: string): typeof Cargo.Type => ({ cwd: '.', package: project, commands: [{ args: NEXTEST, env: { NEXTEST_EXPERIMENTAL_LIBTEST_JSON: '1' } }] });
const inputsFor = (command: (typeof Cargo.Type)['commands'][0]): Readonly<Record<string, typeof Cargo.Type>> => ({
    'app2:build': { cwd: '.', package: 'app2', commands: [command] },
    'app1:build': { cwd: '.', package: 'app1', commands: [command] },
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const tempFs = new TempFs('cargo-batch');
        onCleanup(() => tempFs.cleanup());
        await tempFs.createFiles({
            'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["app1", "app2"]\n',
            'app1/Cargo.toml': '[package]\nname = "app1"\nedition = "2024"\n\n[dependencies]\napp2 = { path = "../app2" }\n',
            'app1/src/main.rs': 'fn main() {\n    println!("{}", app2::two());\n}\n\n#[test]\nfn reads_two() {\n    assert_eq!(app2::two(), 2);\n}\n',
            'app2/Cargo.toml': '[package]\nname = "app2"\nedition = "2024"\n',
            'app2/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n\n#[test]\nfn two_is_two() {\n    assert_eq!(two(), 2);\n}\n',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): ExecutorContext => ({
        root: tempFs.tempDir,
        cwd: tempFs.tempDir,
        isVerbose: false,
        projectGraph: {
            nodes: {
                app1: { name: 'app1', type: 'app', data: { root: 'app1' } },
                app2: { name: 'app2', type: 'lib', data: { root: 'app2' } },
            },
            dependencies: { app1: [{ source: 'app1', target: 'app2', type: 'static' }], app2: [] },
            externalNodes: {},
        },
        projectsConfigurations: { version: 2, projects: { app1: { root: 'app1' }, app2: { root: 'app2' } } },
        nxJsonConfiguration: {},
        taskGraph: batchTaskGraph,
    }))
    .extend('spawn', () => vi.mocked(ChildProcess.make).mockClear());

describe('cargoBatch (async generator)', { timeout: 120_000 }, () => {
    it('relays skipped status for a task whose dependency failed to compile in the same run', async ({ tempFs, context }) => {
        tempFs.writeFile('app2/src/lib.rs', 'pub fn two() -> u32 {\n    absent_function()\n}\n');

        const results = await Array.fromAsync(batch(batchTaskGraph, inputsFor({ args: BUILD }), { __overrides_unparsed__: [] }, context));

        expect(results.map(({ task: id, result: { success, status } }) => [id, success, status])).toStrictEqual([
            ['app2:build', false, undefined],
            ['app1:build', false, 'skipped'],
        ]);
        expect(results[0]?.result.terminalOutput).toContain('absent_function');
        expect(results[1]?.result.terminalOutput).toBe('');
    });

    it('backfills unreported tasks as failure when cargo cannot run', async ({ tempFs, context }) => {
        const results = await Array.fromAsync(batch(batchTaskGraph, inputsFor({ args: BUILD, env: { PATH: join(tempFs.tempDir, 'empty') } }), { __overrides_unparsed__: [] }, context));

        expect(results).toStrictEqual([
            { task: 'app2:build', result: { success: false, terminalOutput: 'cargo is not on PATH, so no Cargo task can run\n' } },
            { task: 'app1:build', result: { success: false, terminalOutput: 'cargo is not on PATH, so no Cargo task can run\n' } },
        ]);
    });

    it('passes through successful results without modification', async ({ context }) => {
        const results = await Array.fromAsync(batch(batchTaskGraph, inputsFor({ args: BUILD }), { __overrides_unparsed__: [] }, context));

        expect(results).toStrictEqual([
            { task: 'app2:build', result: { success: true, terminalOutput: '' } },
            { task: 'app1:build', result: { success: true, terminalOutput: '' } },
        ]);
    });

    it('should group tasks with same targets', async ({ tempFs, context, spawn }) => {
        await Array.fromAsync(batch(batchTaskGraph, inputsFor({ args: BUILD }), { __overrides_unparsed__: [] }, context));

        expect(spawn).toHaveBeenCalledExactlyOnceWith('cargo', [...BUILD, '--package', `${pathToFileURL(join(tempFs.tempDir, 'app2')).href}#app2`, '--package', `${pathToFileURL(join(tempFs.tempDir, 'app1')).href}#app1`], expect.objectContaining({ cwd: tempFs.tempDir }));
    });

    it('should handle multiple task groups', async ({ tempFs, context, spawn }) => {
        const graph: TaskGraph = {
            ...batchTaskGraph,
            tasks: { ...batchTaskGraph.tasks, 'app2:test': task('app2', 'test') },
            dependencies: { ...batchTaskGraph.dependencies, 'app2:test': [] },
            roots: ['app2:build', 'app2:test'],
        };

        const results = await Array.fromAsync(batch(graph, { ...inputsFor({ args: BUILD }), 'app2:test': { cwd: '.', package: 'app2', commands: [{ args: ['test', '--locked'] }] } }, { __overrides_unparsed__: [] }, context));

        expect(results.map(({ task: id, result: { success } }) => [id, success])).toStrictEqual([
            ['app2:build', true],
            ['app1:build', true],
            ['app2:test', true],
        ]);
        expect(spawn).toHaveBeenCalledTimes(2);
        expect(spawn).toHaveBeenNthCalledWith(2, 'cargo', ['test', '--locked', '--package', `${pathToFileURL(join(tempFs.tempDir, 'app2')).href}#app2`], expect.objectContaining({ cwd: tempFs.tempDir }));
    });

    it('passes through successful nextest results', async ({ tempFs, context }) => {
        execFileSync('cargo', ['generate-lockfile'], { cwd: tempFs.tempDir });
        const nextest = await import('@rasm/nx-cargo/executors/nextest');

        const results = await Array.fromAsync(nextest.batch(batchTaskGraph, { 'app2:build': suites('app2'), 'app1:build': suites('app1') }, { __overrides_unparsed__: [] }, context));

        expect(results.map(({ task: id, result: { success } }) => [id, success])).toStrictEqual([
            ['app2:build', true],
            ['app1:build', true],
        ]);
    });
});
