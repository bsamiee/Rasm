import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import { it } from '@rasm/testing/nx';
import type { Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHAIN = {
    'pyproject.toml': '[tool.uv.workspace]\nmembers = ["app", "middle", "core/lib"]\n',
    'app/pyproject.toml': '[project]\nname = "app"\nversion = "0.1.0"\ndependencies = ["middle"]\n\n[tool.uv.sources]\nmiddle = { workspace = true }\n',
    'middle/pyproject.toml': '[project]\nname = "middle"\nversion = "0.1.0"\ndependencies = ["core"]\n\n[tool.uv.sources]\ncore = { workspace = true }\n',
    'core/lib/pyproject.toml': '[project]\nname = "core"\nversion = "0.1.0"\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const edges = (dependencies: Record.ReadonlyRecord<string, readonly { readonly source: string; readonly target: string; readonly type: string }[]>) =>
    Object.values(dependencies)
        .flat()
        .filter(({ target }) => !target.startsWith('npm:'))
        .map(({ source, target, type }) => `${source} -> ${target} (${type})`)
        .toSorted();

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('dependency edges', { timeout: 60_000 }, () => {
    it('links each member to the members uv resolves it to depend on directly', async ({ workspace }) => {
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: workspace.write(CHAIN).root });

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(edges(graph.dependencies)).toStrictEqual(['app -> middle (static)', 'middle -> core (static)']);
    });

    it('links the members of every uv workspace in one Nx workspace', async ({ workspace }) => {
        workspace.write({
            ...CHAIN,
            'other/pyproject.toml': '[tool.uv.workspace]\nmembers = ["cli", "sdk"]\n',
            'other/cli/pyproject.toml': '[project]\nname = "cli"\nversion = "0.1.0"\ndependencies = ["sdk"]\n\n[tool.uv.sources]\nsdk = { workspace = true }\n',
            'other/sdk/pyproject.toml': '[project]\nname = "sdk"\nversion = "0.1.0"\n',
        });
        ['.', 'other'].forEach((folder) => {
            execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: join(workspace.root, folder) });
        });

        const graph = await workspace.graph({ createNodes, createDependencies });

        expect(edges(graph.dependencies)).toStrictEqual(['app -> middle (static)', 'cli -> sdk (static)', 'middle -> core (static)']);
    });
});
