import { existsSync, globSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, taskGraph, targetOptions } from '@rasm/testing/nx';
import { Array } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", core_kit::one());\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('build--cargo', { timeout: 120_000 }, () => {
    it.for([
        { configuration: undefined, outputs: ['target/debug/cli', 'target/debug/cli.d'] },
        { configuration: 'release', outputs: ['target/release/cli', 'target/release/cli.d'] },
    ])('writes the binary and dep-info it declares under configuration $configuration, and nothing outside the target folder', async ({ configuration, outputs }, { workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        const before = globSync('**', { cwd: workspace.root });

        const { success } = await execute(await targetOptions(Cargo, graph, 'cli', 'build--cargo', configuration), workspace.context(graph, 'cli', 'build--cargo', configuration));

        expect(success).toBe(true);
        expect(Object.values(taskGraph(graph, ['cli'], ['build--cargo'], configuration).tasks).map((task) => task.outputs)).toStrictEqual([outputs]);
        expect(outputs.filter((output) => !existsSync(join(workspace.root, output)))).toStrictEqual([]);
        expect(Array.difference(globSync('**', { cwd: workspace.root }), before).filter((file) => !file.startsWith('target') && file !== 'Cargo.lock')).toStrictEqual([]);
    });

    it('declares no outputs and caches nothing where the target folder lies outside the workspace', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, '.cargo/config.toml': `[build]\ntarget-dir = "${workspace.root}-target"\n` }).graph(cargo);

        const { cache, outputs } = graph.nodes['cli']?.data.targets?.['build--cargo'] ?? {};
        expect({ cache, outputs }).toStrictEqual({ cache: undefined, outputs: [] });
    });
});
