import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('@rasm/nx-uv under Windows path separators', { timeout: 60_000 }, () => {
    it('hands Nx posix roots, inputs, outputs, and edges for nested members and a parent-relative output folder', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], {
            cwd: join(
                workspace.write({
                    'python/pyproject.toml': '[project]\nname = "root"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ["pytest>=8", "pytest-cov"]\n\n[tool.uv.workspace]\nmembers = ["libs/app", "libs/deep/core"]\n',
                    'python/libs/app/pyproject.toml': '[project]\nname = "app"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["core"]\n\n[tool.uv.sources]\ncore = { workspace = true }\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
                    'python/libs/app/tests/test_app.py': 'def test_app() -> None:\n    assert True\n',
                    'python/libs/deep/core/pyproject.toml': '[project]\nname = "core"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
                }).root,
                'python',
            ),
        });

        const graph = await workspace.graph({ createNodes, createDependencies }, { outputPath: '.artifacts/uv/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.fromEntries(Object.values(graph.nodes).map(({ name, data }) => [name, data.root]))).toStrictEqual({ app: 'python/libs/app', core: 'python/libs/deep/core', root: 'python' });
        expect(graph.dependencies['app']).toStrictEqual([{ source: 'app', target: 'core', type: 'static' }]);
        expect(unportable(graph)).toStrictEqual([]);
    });
});
