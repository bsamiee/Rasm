import { existsSync, readFileSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import { batch, execute, Test } from '@rasm/nx-vitest/executors/test';
import { spanned, targetOptions, taskGraph, type Workspace } from '@rasm/testing/nx';
import { Record } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"repo","type":"module","workspaces":["libs/*","packages/*"]}' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const executed = async (workspace: Workspace, graph: ProjectGraph, project: string) => execute(await targetOptions(Test, graph, project, 'test--vitest'), workspace.context(graph, project, 'test--vitest'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('test--vitest runs', () => {
    const passing = "import { expect, test } from 'vitest';\n\ntest('holds', () => expect(1 + 1).toBe(2));\n";

    it.for([
        { case: 'no plugin options', options: undefined },
        { case: 'outputPath', options: { outputPath: '.artifacts/vitest/{projectName}' } },
    ])('writes every file of a run under the declared outputs, with $case', async ({ options: settings }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'libs/a/package.json': '{"name":"a","type":"module"}', 'libs/a/vitest.config.ts': 'export default {};\n', 'libs/a/sum.test.ts': passing }).graph({ createNodes }, settings);
        const before = workspace.files();

        const result = await executed(workspace, graph, 'a');

        const written = workspace.files().difference(before);
        const caches = [...written].filter((file) => basename(file) === 'CACHEDIR.TAG').map(dirname);
        const undeclared = workspace
            .files(...caches)
            .difference(before)
            .difference(workspace.cached(graph, 'a', 'test--vitest'));
        expect(result).toStrictEqual({ success: true });
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

    it('fails a run whose import duration limit fails while every test passes, leaving the exit code of the process as it was', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...ROOT,
                'libs/a/package.json': '{"name":"a","type":"module"}',
                'libs/a/vitest.config.ts': 'export default { test: { experimental: { importDurations: { failOnDanger: true, thresholds: { danger: 0 } } } } };\n',
                'libs/a/math.ts': 'export const double = (value: number) => value * 2;\n',
                'libs/a/math.test.ts': "import { expect, test } from 'vitest';\nimport { double } from './math.ts';\n\ntest('doubles', () => expect(double(2)).toBe(4));\n",
            })
            .graph({ createNodes });
        const before = process.exitCode;

        const result = await executed(workspace, graph, 'a');

        expect(result).toStrictEqual({ success: false });
        expect(process.exitCode).toBe(before);
    });
});

describe('batched test--vitest tasks', () => {
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
        const tasks = taskGraph(graph, ['a', 'b'], ['test--vitest']);

        const results = await Array.fromAsync(batch(tasks, { 'a:test--vitest': await targetOptions(Test, graph, 'a', 'test--vitest'), 'b:test--vitest': await targetOptions(Test, graph, 'b', 'test--vitest') }, {}, workspace.context(graph, 'a', 'test--vitest')));

        expect(results.map(({ task, result: { success } }) => [task, success])).toStrictEqual([
            ['a:test--vitest', true],
            ['b:test--vitest', false],
        ]);
        expect([results[0]?.result.terminalOutput.includes('stray'), results[1]?.result.terminalOutput.includes('FAIL b b.test.ts > breaks'), results[1]?.result.terminalOutput.includes('unhandled error')]).toStrictEqual([false, true, true]);
        expect(readFileSync(join(workspace.root, 'runs.log'), 'utf-8')).toBe('run\n');
        expect([...workspace.files()].flatMap((file) => (file.startsWith('.artifacts/') ? [dirname(file)] : []))).toStrictEqual(['.artifacts/vitest/a/attachments']);
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
        const tasks = taskGraph(graph, ['a', 'b'], ['test--vitest']);
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
