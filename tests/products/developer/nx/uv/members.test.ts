import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { refused } from '@rasm/testing/errors';
import { it, unaggregated } from '@rasm/testing/nx';
import { Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUILT = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const MEMBERS = {
    'pyproject.toml': '[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n',
    'proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'proj/application/pyproject.toml': `[project]\nname = "application"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["library"]\n\n[project.scripts]\napplication = "application:main"\n\n[tool.uv.sources]\nlibrary = { workspace = true }\n${BUILT}`,
    'proj/internal/pyproject.toml': `[project]\nname = "internal"\nversion = "0.1.0"\nrequires-python = ">=3.13"\nclassifiers = ["Private :: Do Not Upload"]\n${BUILT}`,
    'proj/virtual/pyproject.toml': '[project]\nname = "virtual"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = (root: string, folder = '.') => execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(root, folder) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('uv workspace members', { timeout: 60_000 }, () => {
    it('gives each member the delivery targets its manifest allows', async ({ workspace }) => {
        locked(workspace.write(MEMBERS).root);

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(
            Record.map(graph.nodes, ({ data: { targets = {}, tags = [] } }) => ({
                released: tags.includes('release'),
                lock: 'check--uv' in targets,
                build: 'build--uv' in targets && 'attach--gh' in targets,
                install: 'install--uv' in targets,
                publish: 'nx-release-publish' in targets,
            })),
        ).toStrictEqual({
            'uv-tutorial': { released: false, lock: true, build: false, install: false, publish: false },
            library: { released: true, lock: false, build: true, install: false, publish: true },
            application: { released: true, lock: false, build: true, install: true, publish: true },
            internal: { released: true, lock: false, build: true, install: false, publish: false },
            virtual: { released: false, lock: false, build: false, install: false, publish: false },
        });
        expect(graph.nodes['library']?.data.targets?.['nx-release-publish']?.dependsOn).toContain('build--uv');
        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it.for([
        { case: 'locked manifests', edit: {}, success: true },
        { case: 'a dependency added after locking', edit: { 'nested/proj/proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["application"]\n${BUILT}` }, success: false },
    ])('checks the lock of a nested workspace from its own folder, given $case', async ({ edit, success }, { workspace }) => {
        locked(workspace.write(Record.mapKeys(MEMBERS, (file) => `nested/proj/${file}`)).root, 'nested/proj');
        const graph = await workspace.write(edit).graph({ createNodes, createDependencies });

        const result = await workspace.run(graph, 'uv-tutorial', 'check--uv');

        expect(result.success).toBe(success);
        expect(graph.nodes['library']?.data.release?.version?.versionActionsOptions).toStrictEqual({ lock: 'nested/proj/uv.lock' });
    });

    it.for([
        { case: 'a pixi.toml beside the workspace manifest', files: { 'pixi.toml': '[workspace]\nchannels = ["conda-forge"]\nplatforms = ["osx-arm64"]\n' }, pixi: true },
        { case: 'a tool.pixi table in the workspace manifest', files: { 'pyproject.toml': `${MEMBERS['pyproject.toml']}\n[tool.pixi.workspace]\nchannels = ["conda-forge"]\nplatforms = ["osx-arm64"]\n` }, pixi: true },
        { case: 'no Pixi manifest', files: {}, pixi: false },
    ])('upgrades the Pixi environments of the workspace root given $case', async ({ files, pixi }, { workspace }) => {
        locked(workspace.write({ ...MEMBERS, ...files }).root);

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect('upgrade--pixi' in { ...graph.nodes['uv-tutorial']?.data.targets }).toBe(pixi);
    });
});

describe('the nx table of a member manifest', { timeout: 60_000 }, () => {
    it('adds the implicit dependencies it names', async ({ workspace }) => {
        locked(workspace.write({ ...MEMBERS, 'proj/virtual/pyproject.toml': `${MEMBERS['proj/virtual/pyproject.toml']}\n[tool.nx]\nimplicitDependencies = ["internal"]\n` }).root);

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(graph.dependencies['virtual']).toContainEqual({ source: 'virtual', target: 'internal', type: 'implicit' });
    });

    it('refuses a key Nx does not read', async ({ workspace }) => {
        locked(workspace.write({ ...MEMBERS, 'proj/virtual/pyproject.toml': `${MEMBERS['proj/virtual/pyproject.toml']}\n[tool.nx]\nimplicitDeps = ["internal"]\n` }).root);

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^uv\.lock: proj\/virtual\/pyproject\.toml does not read as the nx table of tool/u));
    });
});
