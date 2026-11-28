import { execFileSync } from 'node:child_process';
import { existsSync, globSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { workspaceRoot } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, outputs, targetOptions } from '@rasm/testing/nx';
import { Array } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", core_kit::one());\n}\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('build--cargo', () => {
    it.for([
        { layout: 'the host layout', written: () => ({}), folder: () => 'target/debug' },
        { layout: 'the folder of the target build.target names', written: (host: string) => ({ '.cargo/config.toml': `[build]\ntarget = "${host}"\n` }), folder: (host: string) => `target/${host}/debug` },
    ])('declares the binary cargo uplifts in $layout as its one output, which the build writes, writing nothing outside the target folder', async ({ written, folder }, { workspace }) => {
        const host = execFileSync(...workspace.write(crates).spawned('rustc', ['--print', 'host-tuple'], { encoding: 'utf-8' })).trim();
        const graph = await workspace.write(written(host)).graph(cargo);
        execFileSync(...workspace.spawned('cargo', ['generate-lockfile']));
        const before = globSync('**', { cwd: workspace.root });

        const { success } = await execute(await targetOptions(Cargo, graph, 'cli', 'build--cargo'), workspace.context(graph, 'cli', 'build--cargo'));

        expect(success).toBe(true);
        expect(outputs(graph, 'cli', 'build--cargo')).toStrictEqual([`${folder(host)}/cli`]);
        expect(existsSync(join(workspace.root, folder(host), 'cli'))).toBe(true);
        expect(Array.difference(globSync('**', { cwd: workspace.root }), before).filter((file) => !file.startsWith('target'))).toStrictEqual([]);
    });
});
