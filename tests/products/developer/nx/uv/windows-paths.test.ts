import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-uv';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const uv = { createNodes };
const BUILT = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const PYTHON = {
    'python/pyproject.toml': '[project]\nname = "root"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ["pytest>=8", "pytest-cov"]\n\n[tool.uv.workspace]\nmembers = ["libs/app", "libs/middle", "libs/deep/core"]\n',
    'python/libs/app/pyproject.toml': `[project]\nname = "app"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["middle"]\n\n[tool.uv.sources]\nmiddle = { workspace = true }\n${BUILT}`,
    'python/libs/app/tests/test_app.py': 'def test_app() -> None:\n    assert True\n',
    'python/libs/middle/pyproject.toml': `[project]\nname = "middle"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["core"]\n\n[tool.uv.sources]\ncore = { workspace = true }\n${BUILT}`,
    'python/libs/deep/core/pyproject.toml': `[project]\nname = "core"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'other/pyproject.toml': '[tool.uv.workspace]\nmembers = ["cli", "sdk"]\n',
    'other/cli/pyproject.toml': '[project]\nname = "cli"\nversion = "0.1.0"\ndependencies = ["sdk"]\n\n[tool.uv.sources]\nsdk = { workspace = true }\n',
    'other/sdk/pyproject.toml': '[project]\nname = "sdk"\nversion = "0.1.0"\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('@rasm/nx-uv under Windows path separators', { timeout: 120_000 }, () => {
    it('hands Nx posix roots, inputs, outputs, and working folders for nested members and a parent-relative output folder', async ({ workspace }) => {
        ['python', 'other'].forEach((folder) => {
            execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.write(PYTHON).root, folder) });
        });

        const graph = await workspace.graph(uv, { outputPath: '.artifacts/uv/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data }) => [name, data.root]))).toStrictEqual({ app: 'python/libs/app', cli: 'other/cli', core: 'python/libs/deep/core', middle: 'python/libs/middle', other: 'other', root: 'python', sdk: 'other/sdk' });
        expect(unportable(graph)).toStrictEqual([]);
    });

    it('links each member of every uv workspace to the members uv resolves it to depend on directly', async ({ workspace }) => {
        ['python', 'other'].forEach((folder) => {
            execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.write(PYTHON).root, folder) });
        });

        const graph = await workspace.graph(uv);

        expect(
            Object.values(graph.dependencies)
                .flat()
                .filter(({ target }) => !target.startsWith('npm:'))
                .map(({ source, target }) => `${source} -> ${target}`)
                .toSorted(),
        ).toStrictEqual(['app -> middle', 'cli -> sdk', 'middle -> core']);
    });
});
