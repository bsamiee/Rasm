import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { refused } from '@rasm/testing/errors';
import { it, pathed } from '@rasm/testing/nx';
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

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('uv workspace metadata', { timeout: 60_000 }, () => {
    it('answers from the cache while no file uv reads changes', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.write(NESTED).root, 'nested/proj') });
        await workspace.graph({ createNodes, createDependencies });
        pathed(join(workspace.root, '.bin'), {});

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(graph.nodes['library']?.data.root).toBe('nested/proj/library');
    });

    it('reads the workspace again once a lock below the Nx workspace root changes', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.write(NESTED).root, 'nested/proj') });
        await workspace.graph({ createNodes, createDependencies });
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.write(ADDED).root, 'nested/proj') });

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(graph.nodes['added']?.data.root).toBe('nested/proj/added');
    });

    it('keeps the projects of every other lock while one lock fails', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.write(NESTED).root, 'nested/proj') });
        workspace.write({ 'broken/pyproject.toml': '[project\nname = ', 'broken/uv.lock': 'version = 1\n' });

        const graphed = workspace.graph({ createNodes, createDependencies });

        await expect(graphed).rejects.toSatisfy(refused(/^uv workspace metadata .* exits 2: error: Failed to parse: `?pyproject\.toml/u));
        await expect(graphed).rejects.toHaveProperty(['partialProjectConfigurationsResult', 'projects', 'nested/proj/library', 'name'], 'library');
    });
});
