import { execFileSync } from 'node:child_process';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const uv = { createNodes, createDependencies };
const BUILT = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const LIBRARY = {
    'proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
};
const APPLICATION = {
    'proj/application/pyproject.toml': `[project]\nname = "application"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["library"]\n\n[tool.uv.sources]\nlibrary = { workspace = true }\n${BUILT}`,
    'proj/application/src/application/__init__.py': 'from library import one\n\n\ndef two() -> int:\n    return one() + 1\n',
};
const TESTED = { 'proj/library/tests/test_library.py': 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspaced = (dev: readonly string[]) => ({ 'pyproject.toml': `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ${JSON.stringify(dev)}\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n`, ...LIBRARY });

const locked = (workspace: Workspace, files: Readonly<Record<string, string>>) => execFileSync('uv', ['lock', '--quiet'], { cwd: workspace.write(files).root });

const hashed = (workspace: Workspace, graph: ProjectGraph, task: string) => {
    const [project = '', target = ''] = task.split(':');
    const tasks = taskGraph(graph, [project], [target]);
    const found = tasks.tasks[task];
    assert(found !== undefined, `${task} is a task of the graph`);
    return createTaskHasher(graph, readNxJsonFromDisk(workspace.root)).hashTask(found, tasks, env);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('checker targets', { timeout: 120_000 }, () => {
    it('writes the coverage of a pytest run with pytest-cov locked to the outputs its coverage configuration declares under coveragePath', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8', 'pytest-cov']), ...TESTED });
        const graph = await workspace.graph(uv, { coveragePath: '.artifacts/coverage/{projectName}' });
        const before = workspace.files('.venv', 'tmp');

        const result = await workspace.run(graph, 'library', 'test--pytest', 'coverage');

        const written = [...workspace.files('.venv', 'tmp').difference(before)].filter((file) => !/(?:^|\/)(?:__pycache__|\.pytest_cache)\//u.test(file));
        expect(result.success).toBe(true);
        expect([...new Set(written).difference(workspace.cached(graph, 'library', 'test--pytest', 'coverage'))]).toStrictEqual([]);
        expect(written.toSorted()).toStrictEqual(['.artifacts/coverage/library/.coverage', '.artifacts/coverage/library/lcov.info']);
    });

    it('runs pytest in the environment environmentPath names, which a second run reuses at the locked selection', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8']), ...TESTED });
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const [first, second] = [await workspace.run(graph, 'library', 'test--pytest'), await workspace.run(graph, 'library', 'test--pytest')];

        expect([first.success, second.success, workspace.files('.venv').has('tmp/environments/library/pyvenv.cfg')]).toStrictEqual([true, true, true]);
    });

    it.for([
        { case: 'its own source', edited: { 'proj/application/src/application/__init__.py': 'from library import one\n\n\ndef three() -> int:\n    return one() + 2\n' }, changes: true },
        { case: 'a dependency source vulture never reads', edited: { 'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 2 - 1\n' }, changes: false },
    ])('changes the vulture hash of a member ($changes) after an edit to $case', async ({ edited, changes }, { workspace }) => {
        locked(workspace, { ...workspaced(['vulture']), ...APPLICATION });
        const before = await hashed(workspace, await workspace.graph(uv), 'application:check--vulture');

        const after = await hashed(workspace, await workspace.write(edited).graph(uv), 'application:check--vulture');

        expect(before.value === after.value).toBe(!changes);
    });

    it('changes the pytest hash of a member once PYTEST_ADDOPTS selects other tests', async ({ workspace, onTestFinished }) => {
        locked(workspace, { ...workspaced(['pytest>=8']), ...TESTED });
        const graph = await workspace.graph(uv);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const before = await hashed(workspace, graph, 'library:test--pytest');
        vi.stubEnv('PYTEST_ADDOPTS', '-k smoke');

        const after = await hashed(workspace, graph, 'library:test--pytest');

        expect(after.value).not.toBe(before.value);
    });
});
