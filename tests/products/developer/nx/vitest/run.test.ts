import { existsSync, readFileSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';
import type { ProjectGraph, TaskGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import { batch, execute, Test } from '@rasm/nx-vitest/executors/test';
import { spanned, targetOptions, type Workspace } from '@rasm/testing/nx';
import { Record } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"repo","type":"module","workspaces":["libs/*","packages/*"]}' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const awaited = (mine: string, theirs: string) =>
    `import { existsSync, writeFileSync } from 'node:fs';\nimport { expect, test, vi } from 'vitest';\nimport { double, half } from './math.ts';\n\ntest('meets the other run', { timeout: 30_000 }, async () => {\n    writeFileSync('${mine}', '');\n    await vi.waitFor(() => expect(existsSync('${theirs}')).toBe(true), { timeout: 30_000, interval: 50 });\n    expect(${mine === 'a-started' ? 'double(half(2))' : 'double(1)'}).toBe(2);\n});\n`;

const executed = async (workspace: Workspace, graph: ProjectGraph, project: string, configuration?: string) => execute(await targetOptions(Test, graph, project, 'test--vitest', configuration), workspace.context(graph, project, 'test--vitest', configuration));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('test--vitest runs', { timeout: 60_000 }, () => {
    const passing = "import { expect, test } from 'vitest';\n\ntest('holds', () => expect(1 + 1).toBe(2));\n";

    it.for([
        { case: 'no plugin options', options: undefined, lcov: 'libs/a/coverage/lcov.info' },
        { case: 'outputPath and coveragePath', options: { outputPath: '.artifacts/vitest/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' }, lcov: '.artifacts/coverage/a/lcov.info' },
    ])('writes every file of a plain and a coverage run under the declared outputs, with $case', async ({ options: settings, lcov }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a","type":"module"}', 'libs/a/vitest.config.ts': 'export default {};\n', 'libs/a/sum.test.ts': passing }).graph({ createNodes }, settings);
        const before = workspace.files();

        const results = [await executed(workspace, graph, 'a'), await executed(workspace, graph, 'a', 'coverage')];

        const written = workspace.files().difference(before);
        const caches = [...written].filter((file) => basename(file) === 'CACHEDIR.TAG').map(dirname);
        const undeclared = workspace
            .files(...caches)
            .difference(before)
            .difference(workspace.cached(graph, 'a', 'test--vitest'))
            .difference(workspace.cached(graph, 'a', 'test--vitest', 'coverage'));
        expect(results).toStrictEqual([{ success: true }, { success: true }]);
        expect([...written]).toContain(lcov);
        expect([...undeclared]).toStrictEqual([]);
    });

    it('runs only the Vitest project of the package its task names', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n",
                'packages/a/package.json': '{"name":"a","type":"module"}',
                'packages/a/a.test.ts': passing,
                'packages/b/package.json': '{"name":"b","type":"module"}',
                'packages/b/b.test.ts': "import { writeFileSync } from 'node:fs';\nimport { test } from 'vitest';\n\ntest('records its run', () => writeFileSync('b-ran', ''));\n",
            })
            .graph({ createNodes });

        const result = await executed(workspace, graph, 'a');

        expect(result).toStrictEqual({ success: true });
        expect(existsSync(join(workspace.root, 'b-ran'))).toBe(false);
    });

    it.for([
        { case: 'false', test: '{ passWithNoTests: false }', success: false },
        { case: 'unset', test: '{}', success: false },
        { case: 'true', test: '{ passWithNoTests: true }', success: true },
    ])('decides a run finding no test file from passWithNoTests $case', async ({ test, success }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a","type":"module"}', 'libs/a/vitest.config.ts': `export default { test: ${test} };\n` }).graph({ createNodes });

        await expect(executed(workspace, graph, 'a')).resolves.toStrictEqual({ success });
    });

    it('fails a coverage run whose threshold fails while every test passes, leaving the exit code of the process as it was', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'libs/a/package.json': '{"name":"a","type":"module"}',
                'libs/a/vitest.config.ts': "export default { test: { coverage: { include: ['math.ts'], thresholds: { functions: 100 } } } };\n",
                'libs/a/math.ts': 'export const double = (value: number) => value * 2;\n\nexport const half = (value: number) => value / 2;\n',
                'libs/a/math.test.ts': "import { expect, test } from 'vitest';\nimport { double } from './math.ts';\n\ntest('doubles', () => expect(double(2)).toBe(4));\n",
            })
            .graph({ createNodes });
        const before = process.exitCode;

        const result = await executed(workspace, graph, 'a', 'coverage');

        expect(result).toStrictEqual({ success: false });
        expect(process.exitCode).toBe(before);
    });
});

describe('batched test--vitest tasks', { timeout: 60_000 }, () => {
    it('runs every task of one configuration in one Vitest run, deciding each from its own projects and writing its attachments to its own folder', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: ['packages/*'], reporters: ['default', './counted.ts'] } };\n",
                'counted.ts': "import { appendFileSync } from 'node:fs';\n\nexport default class { onInit() { appendFileSync('runs.log', 'run\\n'); } }\n",
                'packages/a/package.json': '{"name":"a","type":"module"}',
                'packages/a/note.txt': 'a',
                'packages/a/a.test.ts': "import { expect, test } from 'vitest';\n\ntest('holds', async ({ annotate }) => { await annotate('kept', { path: 'note.txt' }); expect(1).toBe(1); });\n",
                'packages/b/package.json': '{"name":"b","type":"module"}',
                'packages/b/b.test.ts': "import { expect, test } from 'vitest';\n\ntest('breaks', () => { void Promise.reject(new Error('stray')); expect(1).toBe(2); });\n",
            })
            .graph({ createNodes }, { outputPath: '.artifacts/vitest/{projectName}' });
        const names = ['a', 'b'];
        const tasks: TaskGraph = {
            tasks: Record.fromIterableWith(names, (name) => [`${name}:test--vitest`, { id: `${name}:test--vitest`, target: { project: name, target: 'test--vitest' }, outputs: [], overrides: {}, projectRoot: `packages/${name}`, parallelism: true, cache: true }]),
            dependencies: Record.fromIterableWith(names, (name) => [`${name}:test--vitest`, []]),
            continuousDependencies: {},
            roots: names.map((name) => `${name}:test--vitest`),
        };

        const results = await Array.fromAsync(batch(tasks, { 'a:test--vitest': await targetOptions(Test, graph, 'a', 'test--vitest'), 'b:test--vitest': await targetOptions(Test, graph, 'b', 'test--vitest') }, {}, workspace.context(graph, 'a', 'test--vitest')));

        expect(results.map(({ task, result: { success } }) => [task, success])).toStrictEqual([
            ['a:test--vitest', true],
            ['b:test--vitest', false],
        ]);
        expect([results[0]?.result.terminalOutput.includes('stray'), results[1]?.result.terminalOutput.includes('FAIL b b.test.ts > breaks'), results[1]?.result.terminalOutput.includes('unhandled error')]).toStrictEqual([false, true, true]);
        expect(readFileSync(join(workspace.root, 'runs.log'), 'utf-8')).toBe('run\n');
        expect([...workspace.files()].flatMap((file) => (file.startsWith('.artifacts/') ? [dirname(file)] : []))).toStrictEqual(['.artifacts/vitest/a/attachments']);
    });

    it('runs each task enabling coverage in a Vitest run of its own beside the others, deciding each from the coverage of its own projects', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: ['packages/*'], coverage: { thresholds: { functions: 75 } } } };\n",
                'packages/a/package.json': '{"name":"a","type":"module"}',
                'packages/a/math.ts': 'export const double = (value: number) => value * 2;\n\nexport const half = (value: number) => value / 2;\n',
                'packages/a/a.test.ts': awaited('a-started', 'b-started'),
                'packages/b/package.json': '{"name":"b","type":"module"}',
                'packages/b/math.ts': 'export const double = (value: number) => value * 2;\n\nexport const half = (value: number) => value / 2;\n',
                'packages/b/b.test.ts': awaited('b-started', 'a-started'),
            })
            .graph({ createNodes }, { coveragePath: '.artifacts/coverage/{projectName}' });
        const names = ['a', 'b'];
        const tasks: TaskGraph = {
            tasks: Record.fromIterableWith(names, (name) => [`${name}:test--vitest`, { id: `${name}:test--vitest`, target: { project: name, target: 'test--vitest' }, outputs: [], overrides: {}, projectRoot: `packages/${name}`, parallelism: true, cache: true }]),
            dependencies: Record.fromIterableWith(names, (name) => [`${name}:test--vitest`, []]),
            continuousDependencies: {},
            roots: names.map((name) => `${name}:test--vitest`),
        };
        const covered = async (name: string) => ({ ...(await targetOptions(Test, graph, name, 'test--vitest')), __unparsed__: ['--coverage'] });

        const results = await Array.fromAsync(batch(tasks, { 'a:test--vitest': await covered('a'), 'b:test--vitest': await covered('b') }, {}, workspace.context(graph, 'a', 'test--vitest')));

        expect(Object.fromEntries(results.map(({ task, result: { success } }) => [task, success]))).toStrictEqual({ 'a:test--vitest': true, 'b:test--vitest': false });
        expect(names.map((name) => existsSync(join(workspace.root, `.artifacts/coverage/${name}/coverage-final.json`)))).toStrictEqual([true, true]);
    });

    it('stamps each task with the span Vitest states for its own test modules, inside the run, a task with no module taking the span of the run', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n",
                'packages/a/package.json': '{"name":"a","type":"module"}',
                'packages/a/a.test.ts': "import { expect, test } from 'vitest';\n\ntest('waits', async () => { await new Promise((resolve) => setTimeout(resolve, 300)); expect(1).toBe(1); });\n",
                'packages/b/package.json': '{"name":"b","type":"module"}',
            })
            .graph({ createNodes });
        const tasks: TaskGraph = {
            tasks: Record.fromIterableWith(['a', 'b'], (name) => [`${name}:test--vitest`, { id: `${name}:test--vitest`, target: { project: name, target: 'test--vitest' }, outputs: [], overrides: {}, projectRoot: `packages/${name}`, parallelism: true, cache: true }]),
            dependencies: { 'a:test--vitest': [], 'b:test--vitest': [] },
            continuousDependencies: {},
            roots: ['a:test--vitest', 'b:test--vitest'],
        };
        const before = vi.getRealSystemTime();

        const results = Record.fromIterableWith(
            await Array.fromAsync(batch(tasks, { 'a:test--vitest': await targetOptions(Test, graph, 'a', 'test--vitest'), 'b:test--vitest': await targetOptions(Test, graph, 'b', 'test--vitest') }, {}, workspace.context(graph, 'a', 'test--vitest'))),
            ({ task, result }) => [task, result] as const,
        );

        const after = vi.getRealSystemTime();
        expect(results).toStrictEqual({ 'a:test--vitest': spanned(before, after), 'b:test--vitest': spanned(before, after) });
        expect([(results['a:test--vitest']?.endTime ?? 0) - (results['a:test--vitest']?.startTime ?? 0) >= 300, (results['a:test--vitest']?.startTime ?? 0) > (results['b:test--vitest']?.startTime ?? Infinity)]).toStrictEqual([true, true]);
    });
});
