import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-uv';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { Array, Effect } from 'effect';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const MEMBER = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["proj/*"]\n',
    'proj/my-lib/pyproject.toml': '[project]\nname = "my-lib"\nversion = "0.1.0"\nrequires-python = ">=3.13"\nreadme = "README.md"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
    'proj/my-lib/README.md': '# my-lib\n',
    'proj/my-lib/src/my_lib/__init__.py': 'def one() -> int:\n    return 1\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph) => {
    const tasks = taskGraph(graph, ['my-lib'], ['build--uv']);
    const found = tasks.tasks['my-lib:build--uv'];
    assert(found !== undefined, 'my-lib:build--uv is a task of the graph');
    return createTaskHasher(graph, readNxJsonFromDisk(workspace.root)).hashTask(found, tasks, env);
};

const appended = (workspace: Workspace, file: string) => Effect.promise(async () => hashed(workspace, await workspace.write({ [file]: `${readFileSync(join(workspace.root, file), 'utf-8')}\n` }).graph({ createNodes })));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('build--uv', { timeout: 60_000 }, () => {
    it.for([
        { case: 'no plugin options', options: undefined, folder: 'dist/' },
        { case: 'outputPath', options: { outputPath: '.artifacts/uv/{projectName}' }, folder: '.artifacts/uv/my-lib/' },
    ])('writes the sdist and wheel of a member only to the outputs it declares, with $case', async ({ options, folder }, { workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(MEMBER).root });
        const graph = await workspace.graph({ createNodes }, options);
        const before = workspace.files('tmp');

        const result = await workspace.run(graph, 'my-lib', 'build--uv');

        const written = workspace.files('tmp').difference(before);
        expect(result.success).toBe(true);
        expect(
            [...written]
                .filter((file) => file.endsWith('.tar.gz') || file.endsWith('.whl'))
                .map((file) => file.replace(folder, ''))
                .toSorted(),
        ).toStrictEqual(['my_lib-0.1.0-py3-none-any.whl', 'my_lib-0.1.0.tar.gz']);
        expect([...written.difference(workspace.cached(graph, 'my-lib', 'build--uv'))]).toStrictEqual([]);
    });

    it('changes the build hash of a member after an edit to each source file uv packs into its sdist', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(MEMBER).root });
        const graph = await workspace.graph({ createNodes });
        const sources = workspace.files();
        await workspace.run(graph, 'my-lib', 'build--uv');
        const packed = execFileSync('tar', ['-tzf', join(workspace.root, 'dist/my_lib-0.1.0.tar.gz')], { encoding: 'utf-8' })
            .split('\n')
            .map((entry) => entry.replace('my_lib-0.1.0/', 'proj/my-lib/'))
            .filter((file) => sources.has(file))
            .toSorted();
        const built = await hashed(workspace, graph);

        const edited = await Effect.runPromise(Effect.forEach(packed, (file) => appended(workspace, file)));

        expect(packed).toStrictEqual(['proj/my-lib/README.md', 'proj/my-lib/pyproject.toml', 'proj/my-lib/src/my_lib/__init__.py']);
        expect(Array.window([built, ...edited], 2).map(([before, after]) => before.value !== after.value)).toStrictEqual([true, true, true]);
    });
});
