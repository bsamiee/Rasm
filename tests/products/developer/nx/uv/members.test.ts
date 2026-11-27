import { execFileSync } from 'node:child_process';
import { basename, join, posix } from 'node:path';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-uv';
import { unaggregated } from '@rasm/testing/nx';
import { Array, Record, Struct } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { LoadedNxPlugin, retrieveProjectConfigurations } from 'nx/src/devkit-internals';
import { buildProjectGraphUsingProjectFileMap } from 'nx/src/project-graph/build-project-graph';
import type { FileMapCache } from 'nx/src/project-graph/nx-deps-cache';
import { retrieveWorkspaceFiles } from 'nx/src/project-graph/utils/retrieve-workspace-files';
import { resetWorkspaceContext } from 'nx/src/utils/workspace-context';
import { describe, expect, vi } from 'vitest';
import { it } from '#/fixtures';

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
const VIRTUAL = { 'pyproject.toml': '[tool.uv.workspace]\nmembers = ["library"]\n', 'library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n' };
const WORDS = {
    'proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["terms", "words"]\n\n[tool.uv.sources]\nterms = { workspace = true }\n${BUILT}`,
    'proj/terms/pyproject.toml': `[project]\nname = "terms"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'vendor/words/pyproject.toml': `[project]\nname = "words"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[tool.uv.workspace]\nmembers = []\n${BUILT}`,
    'other/words/pyproject.toml': `[project]\nname = "words"\nversion = "0.2.0"\nrequires-python = ">=3.13"\n${BUILT}`,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = (root: string, folder = '.') => execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(root, folder) });

const sourced = (folder: string) => ({ 'pyproject.toml': `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n\n[tool.uv.sources]\nwords = { path = "${folder}" }\n` });

const built = async (root: string, cache: FileMapCache | null) => {
    resetWorkspaceContext();
    const plugin = new LoadedNxPlugin({ name: 'plugin-under-test', createNodes }, { plugin: 'plugin-under-test' });
    const read = await retrieveProjectConfigurations({ specifiedPlugins: [plugin], defaultPlugins: [] }, root, readNxJson(root));
    const { fileMap, rustReferences } = await retrieveWorkspaceFiles(root, read.projectRootMap);
    return buildProjectGraphUsingProjectFileMap(read.projects, read.externalNodes, fileMap, rustReferences, cache, [plugin], read.sourceMaps);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('files Nx hands the plugin', () => {
    it('hold the locks alone, as a matched file edit is a project configuration change', async ({ workspace }) => {
        const read = { 'uv.toml': '', '.python-version': '3.13\n', '.python-versions': '3.13\n', 'mypy.ini': '', '.mypy.ini': '', 'setup.cfg': '', 'ty.toml': '', 'pytest.toml': '', '.pytest.toml': '', 'pytest.ini': '', '.pytest.ini': '', 'tox.ini': '' };
        const placed = Array.cartesianWith(['', 'proj/library/'], Record.toEntries(read), (folder, [file, content]) => [`${folder}${file}`, content] as const);
        locked(workspace.write({ ...MEMBERS, ...Record.fromEntries(placed) }).root);

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        expect(matched).toStrictEqual(['uv.lock']);
    });
});

describe('uv roots without a project table', () => {
    it.for([
        { case: 'a nested folder, by the folder', folder: 'nested/proj', files: {}, name: 'proj' },
        { case: 'the Nx workspace root, by the project.json beside it', folder: '.', files: { 'project.json': '{ "name": "named" }' }, name: 'named' },
        { case: 'the Nx workspace root, by the package.json beside it', folder: '.', files: { 'package.json': '{ "name": "packaged" }' }, name: 'packaged' },
    ])('are a project owning the lock check and upgrade, named in $case', async ({ folder, files, name }, { workspace }) => {
        locked(workspace.write({ ...Record.mapKeys(VIRTUAL, (file) => posix.join(folder, file)), ...files }).root, folder);

        const graph = await workspace.graph({ createNodes });
        const result = await workspace.run(graph, name, 'check--uv');

        expect([graph.nodes[name]?.data.root, result.success, 'upgrade--uv' in (graph.nodes[name]?.data.targets ?? {})]).toStrictEqual([folder, true, true]);
    });

    it('are a project named by the folder at an Nx workspace root no other plugin names', async ({ workspace }) => {
        locked(workspace.write(VIRTUAL).root);

        const graph = await workspace.graph({ createNodes });

        expect(Record.map(graph.nodes, ({ data: { root, targets = {} } }) => [root, 'check--uv' in targets])).toStrictEqual({ library: ['library', false], [basename(workspace.root)]: ['.', true] });
    });
});

describe('uv workspace members', () => {
    it('gives each member the delivery targets its manifest allows', async ({ workspace }) => {
        locked(workspace.write(MEMBERS).root);

        const graph = await workspace.graph({ createNodes });

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
        { case: 'a dependency added after locking', edit: { 'nested/proj/proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["internal"]\n\n[tool.uv.sources]\ninternal = { workspace = true }\n${BUILT}` }, success: false },
    ])('checks the lock of a nested workspace from its own folder, given $case', async ({ edit, success }, { workspace }) => {
        locked(workspace.write(Record.mapKeys(MEMBERS, (file) => `nested/proj/${file}`)).root, 'nested/proj');
        const graph = await workspace.write(edit).graph({ createNodes });

        const result = await workspace.run(graph, 'uv-tutorial', 'check--uv');

        expect(result.success).toBe(success);
        expect(graph.nodes['library']?.data.release?.version?.versionActionsOptions).toStrictEqual({ lock: 'nested/proj/uv.lock' });
    });

    it('gives a member a dependency on each member its manifest declares and on none its dependency groups alone install', async ({ workspace }) => {
        locked(
            workspace.write({
                ...MEMBERS,
                'pyproject.toml': '[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["library"]\n\n[dependency-groups]\ndev = ["internal"]\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n\n[tool.uv.sources]\nlibrary = { workspace = true }\ninternal = { workspace = true }\n',
            }).root,
        );

        const graph = await workspace.graph({ createNodes });

        expect(graph.dependencies['uv-tutorial']?.map(({ target }) => target)).toStrictEqual(['library']);
    });

    it('links a member to a dependency a project.json renames', async ({ workspace }) => {
        locked(workspace.write({ ...MEMBERS, 'proj/library/project.json': '{ "name": "renamed" }' }).root);

        const graph = await workspace.graph({ createNodes });

        expect(graph.dependencies['application']?.map(({ target }) => target)).toStrictEqual(['renamed']);
    });

    it.for([
        { case: 'gains', before: 'other/words', after: 'vendor/words', edges: [['terms'], ['terms', 'words']] },
        { case: 'loses', before: 'vendor/words', after: 'other/words', edges: [['terms', 'words'], ['terms']] },
    ])('rebuilds over the cached graph the dependency of a member that the source mapping of the root manifest $case', async ({ before, after, edges }, { workspace }) => {
        locked(workspace.write({ ...WORDS, ...sourced(before) }).root, 'vendor/words');
        locked(workspace.root);
        const first = await built(workspace.root, null);

        locked(workspace.write(sourced(after)).root);
        const second = await built(workspace.root, first.projectFileMapCache);

        expect([first, second].map(({ projectGraph }) => (projectGraph.dependencies['library'] ?? []).map(Struct.get('target')).toSorted())).toStrictEqual(edges);
    });
});
