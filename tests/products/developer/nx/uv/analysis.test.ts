import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { env } from 'node:process';
import { createNodes } from '@rasm/nx-uv';
import { refused } from '@rasm/testing/errors';
import { it, shimmed, type Workspace } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const NESTED = {
    'nested/proj/pyproject.toml': '[tool.uv.workspace]\nmembers = ["library"]\n',
    'nested/proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\n',
};
const ADDED = {
    'nested/proj/pyproject.toml': '[tool.uv.workspace]\nmembers = ["library", "added"]\n',
    'nested/proj/added/pyproject.toml': '[project]\nname = "added"\nversion = "0.1.0"\n',
};
const SCRIPTED = { 'nested/proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\n\n[project.scripts]\nlibrary = "library:main"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n' };
const ROOTED = {
    'pyproject.toml': '[project]\nname = "root"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n',
    'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
};
const UNLOCKED = { 'proj/added/pyproject.toml': '[project]\nname = "added"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n' };
const ASKED = ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--output-version', 'preview', '--quiet'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = (workspace: Workspace, folder: string) => execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.root, folder) });

const counted = (workspace: Workspace, name: string) => {
    const log = join(workspace.root, 'tmp', `${name}.log`);
    shimmed(join(workspace.root, 'tmp', name), { uv: `echo "$UV_PROJECT_ENVIRONMENT|$*" >> "${log}"\nPATH="\${PATH#*:}" exec uv "$@"` });
    return () => (existsSync(log) ? readFileSync(log, 'utf-8').trim().split('\n') : []).filter((call) => call.includes('workspace metadata'));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('uv workspace metadata', { timeout: 60_000 }, () => {
    it.for([
        { file: 'uv.toml', content: 'compile-bytecode = false\n' },
        { file: '.python-version', content: '3.13\n' },
        { file: '.python-versions', content: '3.13\n' },
    ])('asks uv once for a lock, and not again once $file appears at the uv root', async ({ file, content }, { workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        const calls = counted(workspace, 'uv');
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ [`nested/proj/${file}`]: content }).graph({ createNodes });

        expect([graph.nodes['library']?.data.root, calls().length]).toStrictEqual(['nested/proj/library', 1]);
    });

    it('asks uv again through another uv binary on PATH', async ({ workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        counted(workspace, 'first');
        await workspace.graph({ createNodes });
        const calls = counted(workspace, 'second');

        await workspace.graph({ createNodes });

        expect(calls()).toHaveLength(1);
    });

    it('asks uv with a project environment that does not exist, which uv answers without the environment it reports by default', async ({ workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        const [calls, cwd] = [counted(workspace, 'uv'), join(workspace.root, 'nested/proj')];
        execFileSync('uv', ['sync', '--offline', '--quiet'], { cwd });
        await workspace.graph({ createNodes });

        const [environment = ''] = calls()[0]?.split('|') ?? [];
        const answered = [{}, { UV_PROJECT_ENVIRONMENT: environment }].map((set): unknown => JSON.parse(execFileSync('uv', ASKED, { cwd, encoding: 'utf-8', env: { ...env, ...set } })));

        expect([environment === '', existsSync(resolve(cwd, environment)), ...answered.map(Predicate.hasProperty('environment'))]).toStrictEqual([false, false, true, false]);
    });

    it('reads the workspace again once a lock below the Nx workspace root changes', async ({ workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        await workspace.graph({ createNodes });
        locked(workspace.write(ADDED), 'nested/proj');

        const graph = await workspace.graph({ createNodes });

        expect(graph.nodes['added']?.data.root).toBe('nested/proj/added');
    });

    it('reads the workspace again once a member manifest declares scripts', async ({ workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        const before = await workspace.graph({ createNodes });
        locked(workspace.write(SCRIPTED), 'nested/proj');

        const after = await workspace.graph({ createNodes });

        expect([before, after].map((graph) => 'install--uv' in (graph.nodes['library']?.data.targets ?? {}))).toStrictEqual([false, true]);
    });

    it('keeps every member a project, warns once, and leaves the lock check failing once a member is added without uv lock', async ({ workspace, onTestFinished }) => {
        locked(workspace.write(ROOTED), '.');
        const warned = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onTestFinished(() => warned.mockRestore());

        const graph = await workspace.write(UNLOCKED).graph({ createNodes });
        const checked = await workspace.run(graph, 'root', 'check--uv');
        locked(workspace, '.');
        const relocked = await workspace.graph({ createNodes });

        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['added', 'library', 'root']);
        expect([checked.success, 'build--uv' in (graph.nodes['library']?.data.targets ?? {}), 'build--uv' in (relocked.nodes['library']?.data.targets ?? {})]).toStrictEqual([false, false, true]);
        expect(warned.mock.calls).toStrictEqual([[expect.stringMatching(/^uv\.lock .*`uv lock`/u)]]);
    });

    it('keeps the projects of every other lock while one lock fails', async ({ workspace }) => {
        locked(workspace.write(NESTED), 'nested/proj');
        workspace.write({ 'broken/pyproject.toml': '[project\nname = ', 'broken/uv.lock': 'version = 1\n' });

        const graphed = workspace.graph({ createNodes });

        await expect(graphed).rejects.toSatisfy(refused(/^uv workspace metadata .* exits 2: error: Failed to parse: `?pyproject\.toml/u));
        await expect(graphed).rejects.toHaveProperty(['partialProjectConfigurationsResult', 'projects', 'nested/proj/library', 'name'], 'library');
    });
});
