import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/cli/src/main.rs': 'fn main() {\n    println!("installed");\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('install--cargo', { timeout: 120_000 }, () => {
    it('exists only for a crate with a binary target', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        expect(Object.keys(graph.nodes).filter((name) => 'install--cargo' in (graph.nodes[name]?.data.targets ?? {}))).toStrictEqual(['cli']);
    });

    it('places the release binary in bin of the Cargo install root', async ({ workspace, onTestFinished }) => {
        vi.stubEnv('CARGO_INSTALL_ROOT', join(workspace.root, '.install'));
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });

        const { success } = await execute(await targetOptions(Cargo, graph, 'cli', 'install--cargo'), workspace.context(graph, 'cli', 'install--cargo'));

        expect(success).toBe(true);
        expect(existsSync(join(workspace.root, '.install/bin/cli'))).toBe(true);
    });
});
