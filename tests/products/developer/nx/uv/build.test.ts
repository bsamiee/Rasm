import { execFileSync } from 'node:child_process';
import { join, matchesGlob } from 'node:path';
import { createNodes } from '@rasm/nx-uv';
import { it } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const MEMBER = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["proj/*"]\n',
    'proj/my-lib/pyproject.toml': '[project]\nname = "my-lib"\nversion = "0.1.0"\nrequires-python = ">=3.13"\nreadme = "README.md"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
    'proj/my-lib/README.md': '# my-lib\n',
    'proj/my-lib/src/my_lib/__init__.py': 'def one() -> int:\n    return 1\n',
    'proj/my-lib/src/my_lib/__pycache__/stale.cpython-313.pyc': '',
    'proj/my-lib/notes.txt': 'not packed\n',
};

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

    it('hashes every source file uv packs into the sdist, and no file it leaves out', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(MEMBER).root });
        const graph = await workspace.graph({ createNodes });
        const sources = workspace.files();
        await workspace.run(graph, 'my-lib', 'build--uv');

        const packed = execFileSync('tar', ['-tzf', join(workspace.root, 'dist/my_lib-0.1.0.tar.gz')], { encoding: 'utf-8' })
            .split('\n')
            .map((entry) => entry.replace('my_lib-0.1.0/', 'proj/my-lib/'))
            .filter((file) => sources.has(file));
        const globs = (graph.nodes['my-lib']?.data.targets?.['build--uv']?.inputs ?? []).filter(Predicate.isString).map((glob) => glob.replace('{projectRoot}', 'proj/my-lib').replace('{workspaceRoot}/', ''));
        const hashed = (file: string) => globs.some((glob) => !glob.startsWith('!') && matchesGlob(file, glob)) && !globs.some((glob) => glob.startsWith('!') && matchesGlob(file, glob.slice(1)));
        expect(packed.toSorted()).toStrictEqual(['proj/my-lib/README.md', 'proj/my-lib/pyproject.toml', 'proj/my-lib/src/my_lib/__init__.py']);
        expect(packed.filter((file) => !hashed(file))).toStrictEqual([]);
        expect(['proj/my-lib/notes.txt', 'proj/my-lib/src/my_lib/__pycache__/stale.cpython-313.pyc'].filter(hashed)).toStrictEqual([]);
    });
});
