import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { workspaceRoot } from '@nx/devkit';
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
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'rust/Cargo.toml': '[package]\nname = "root"\nversion = "0.1.0"\nedition = "2024"\n\n[workspace]\nresolver = "3"\nmembers = ["crates/*", "api-checker/core"]\n\n[dependencies]\ncore-kit = { path = "crates/core-kit", version = "0.1.0" }\n',
    'rust/src/lib.rs': '',
    'rust/.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
    'rust/crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\nreadme = "../../docs/core-kit.md"\n',
    'rust/crates/core-kit/src/lib.rs': '',
    'rust/docs/core-kit.md': '# core-kit\n',
    'rust/crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'rust/crates/cli/src/main.rs': 'fn main() {}\n',
    'rust/api-checker/core/Cargo.toml': '[package]\nname = "api-checker-core"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../../crates/core-kit", version = "0.1.0" }\n',
    'rust/api-checker/core/src/lib.rs': '',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Windows path separators', () => {
    it('states roots, inputs, outputs, working folders, and the lock as workspace-relative posix paths', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['api_checker_core', 'cli', 'core_kit', 'root']);
        expect(unportable(graph)).toStrictEqual([]);
        expect(graph.nodes['cli']?.data.release?.version?.versionActionsOptions).toStrictEqual({ lock: 'rust/Cargo.lock' });
    });

    it('links each path dependency as a static edge from its manifest, the root crate and a parent-relative path included', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        expect(['root', 'cli', 'api_checker_core'].flatMap((project) => graph.dependencies[project] ?? [])).toStrictEqual([
            { source: 'root', target: 'core_kit', type: 'static' },
            { source: 'cli', target: 'core_kit', type: 'static' },
            { source: 'api_checker_core', target: 'core_kit', type: 'static' },
        ]);
    });
});
