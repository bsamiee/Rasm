import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust/Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'rust/.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
    'rust/crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\nreadme = "../../docs/core-kit.md"\n',
    'rust/crates/core-kit/src/lib.rs': '',
    'rust/docs/core-kit.md': '# core-kit\n',
    'rust/crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'rust/crates/cli/src/main.rs': 'fn main() {}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('Windows path separators', { timeout: 120_000 }, () => {
    it('states roots, inputs, outputs, working folders, and the lock as workspace-relative posix paths', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo, { coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['cli', 'core_kit']);
        expect(unportable(graph)).toStrictEqual([]);
        expect(graph.nodes['cli']?.data.release?.version?.versionActionsOptions).toStrictEqual({ lock: 'rust/Cargo.lock' });
    });

    it('keeps the static edge of a path dependency', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        expect(graph.dependencies['cli']).toStrictEqual([{ source: 'cli', target: 'core_kit', type: 'static' }]);
    });
});
