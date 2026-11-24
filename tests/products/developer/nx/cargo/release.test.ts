import { execFileSync, spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { workspaceRoot } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, outputs, taskGraph, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'rust/Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'rust/crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\ndescription = "Doubles"\nlicense = "MIT"\n',
    'rust/crates/kit/src/lib.rs': 'pub fn double(value: u32) -> u32 {\n    value * 2\n}\n',
    'rust/crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.2.0"\nedition = "2024"\ndescription = "Prints"\nlicense = "MIT"\n\n[dependencies]\nkit = { path = "../kit", version = "0.1.0" }\n',
    'rust/crates/app/src/main.rs': 'fn main() {\n    println!("{}", kit::double(1));\n}\n',
};
const sealed = {
    ...crates,
    'rust/crates/sealed/Cargo.toml': '[package]\nname = "nx-fixture-sealed"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n',
    'rust/crates/sealed/src/lib.rs': 'pub fn three() -> u32 {\n    3\n}\n',
    'rust/crates/adder/Cargo.toml': '[package]\nname = "nx-fixture-adder"\nversion = "0.1.0"\nedition = "2024"\ndescription = "Adds"\nlicense = "MIT"\n\n[dependencies]\nnx-fixture-sealed = { path = "../sealed", version = "0.1.0" }\n',
    'rust/crates/adder/src/main.rs': 'fn main() {\n    println!("{}", nx_fixture_sealed::three());\n}\n',
};

const CLOSURES = [
    ['app', ['--package', 'app', '--package', 'kit']],
    ['kit', ['--package', 'kit']],
    ['nx_fixture_adder', ['--package', 'nx-fixture-adder', '--package', 'nx-fixture-sealed']],
    ['nx_fixture_sealed', ['--package', 'nx-fixture-sealed']],
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('pack--cargo', { timeout: 120_000 }, () => {
    it('packs a crate depending on another workspace crate into the archive its outputs name, holding its manifest and sources', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: join(workspace.root, 'rust') });

        const { success } = await execute(await targetOptions(Cargo, graph, 'app', 'pack--cargo'), workspace.context(graph, 'app', 'pack--cargo'));

        const [archive = ''] = outputs(graph, 'app', 'pack--cargo');
        expect(success).toBe(true);
        expect(
            execFileSync('tar', ['-tzf', join(workspace.root, archive)], { encoding: 'utf-8' })
                .split('\n')
                .filter((entry) => ['app-0.2.0/Cargo.toml', 'app-0.2.0/src/main.rs', 'app-0.2.0/Cargo.lock'].includes(entry))
                .toSorted(),
        ).toStrictEqual(['app-0.2.0/Cargo.lock', 'app-0.2.0/Cargo.toml', 'app-0.2.0/src/main.rs']);
    });

    it('infers a pack for exactly the versioned crates cargo package accepts, none for a crate whose path dependency publishes nowhere', async ({ workspace }) => {
        const graph = await workspace.write(sealed).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: join(workspace.root, 'rust') });

        const accepted = CLOSURES.filter(([, selected]) => spawnSync('cargo', ['package', ...selected, '--no-verify', '--locked'], { cwd: join(workspace.root, 'rust') }).status === 0).map(([project]) => project);
        expect(
            Object.keys(graph.nodes)
                .filter((project) => 'pack--cargo' in (graph.nodes[project]?.data.targets ?? {}))
                .toSorted(),
        ).toStrictEqual(accepted);
        expect(accepted).toStrictEqual(['app', 'kit', 'nx_fixture_sealed']);
    });
});

describe('nx-release-publish', { timeout: 120_000 }, () => {
    it('publishes a crate after each workspace crate it depends on, running nothing else first', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        const { tasks } = taskGraph(graph, ['app'], ['nx-release-publish']);

        expect(Object.keys(tasks).toSorted()).toStrictEqual(['app:nx-release-publish', 'kit:nx-release-publish']);
    });
});
