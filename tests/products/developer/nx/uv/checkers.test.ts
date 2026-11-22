import { execFileSync } from 'node:child_process';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { it } from '@rasm/testing/nx';
import { Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIBRARY = {
    'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
    'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
};
const TESTED = { 'proj/library/tests/test_library.py': 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspaced = (dev: readonly string[]) => ({ 'pyproject.toml': `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ${JSON.stringify(dev)}\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n`, ...LIBRARY });

const checkers = (targets: object) => Object.keys(targets).filter((target) => /^(?:check|test)--(?!uv)/u.test(target));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('checker targets', { timeout: 60_000 }, () => {
    it.for([
        { case: 'pytest locked beside test files', dev: ['pytest>=8'], files: TESTED, checks: ['test--pytest'] },
        { case: 'pytest locked with no test file', dev: ['pytest>=8'], files: {}, checks: [] },
        { case: 'ty locked', dev: ['ty'], files: {}, checks: ['check--ty'] },
        { case: 'no checker locked', dev: [], files: TESTED, checks: [] },
    ])('runs the checkers of a member the workspace locks, given $case', async ({ dev, files, checks }, { workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write({ ...workspaced(dev), ...files }).root });

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(Record.map(graph.nodes, ({ data }) => checkers(data.targets ?? {}))).toStrictEqual({ 'uv-tutorial': [], library: checks });
    });

    it('hashes the ty configuration of the workspace ty runs in and the lock, and no ty table of the member', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write({ ...workspaced(['ty']), 'ty.toml': '[rules]\nunresolved-import = "error"\n', 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[tool.ty.rules]\nunresolved-import = "ignore"\n` }).root });

        const graph = await workspace.graph({ createNodes, createDependencies });

        const inputs = graph.nodes['library']?.data.targets?.['check--ty']?.inputs;

        expect(inputs).toContain('{workspaceRoot}/ty.toml');
        expect(inputs).toContain('{workspaceRoot}/uv.lock');
        expect(inputs).not.toContain('{workspaceRoot}/proj/library/pyproject.toml');
    });

    it('writes the coverage of a pytest run with pytest-cov locked to the outputs its coverage configuration declares under coveragePath', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write({ ...workspaced(['pytest>=8', 'pytest-cov']), ...TESTED }).root });
        const graph = await workspace.graph({ createNodes, createDependencies }, { coveragePath: '.artifacts/coverage/{projectName}' });
        const before = workspace.files('.venv', 'tmp');

        const result = await workspace.run(graph, 'library', 'test--pytest', 'coverage');

        const written = [...workspace.files('.venv', 'tmp').difference(before)].filter((file) => !/(?:^|\/)(?:__pycache__|\.pytest_cache)\//u.test(file));
        expect(result.success).toBe(true);
        expect([...new Set(written).difference(workspace.cached(graph, 'library', 'test--pytest', 'coverage'))]).toStrictEqual([]);
        expect(written.toSorted()).toStrictEqual(['.artifacts/coverage/library/.coverage', '.artifacts/coverage/library/lcov.info']);
    });
});
