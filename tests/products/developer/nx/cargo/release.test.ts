import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectConfiguration } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { Publish } from '@rasm/nx-cargo/executors/release-publish';
import { it, outputs, taskGraph, targetOptions } from '@rasm/testing/nx';
import { Array, Effect, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust/Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'rust/crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\ndescription = "Doubles"\nlicense = "MIT"\n',
    'rust/crates/kit/src/lib.rs': 'pub fn double(value: u32) -> u32 {\n    value * 2\n}\n',
    'rust/crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.2.0"\nedition = "2024"\ndescription = "Prints"\nlicense = "MIT"\n\n[dependencies]\nkit = { path = "../kit", version = "0.1.0" }\n',
    'rust/crates/app/src/main.rs': 'fn main() {\n    println!("{}", kit::double(1));\n}\n',
};
const RELEASED = ['pack--cargo', 'attach--gh', 'nx-release-publish'];

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('release targets', { timeout: 120_000 }, () => {
    it('tags a crate stating a version for release, packing and attaching it, and publishes only a crate its publish setting allows', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                'rust/crates/internal/Cargo.toml': '[package]\nname = "internal"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n',
                'rust/crates/internal/src/lib.rs': '',
                'rust/crates/draft/Cargo.toml': '[package]\nname = "draft"\nedition = "2024"\n',
                'rust/crates/draft/src/lib.rs': '',
            })
            .graph(cargo);

        const released = Record.map(graph.nodes, ({ data }) => ({ tags: data.tags, targets: Array.intersection(RELEASED, Object.keys(data.targets ?? {})), lock: data.release?.version?.versionActionsOptions?.['lock'] }));
        expect(released).toStrictEqual({
            kit: { tags: ['release'], targets: RELEASED, lock: 'rust/Cargo.lock' },
            app: { tags: ['release'], targets: RELEASED, lock: 'rust/Cargo.lock' },
            internal: { tags: ['release'], targets: ['pack--cargo', 'attach--gh'], lock: 'rust/Cargo.lock' },
            draft: { tags: [], targets: [], lock: undefined },
        });
    });

    it.for([
        { case: 'a registry of its own', publish: '["internal"]', registry: 'internal' },
        { case: 'crates.io, the default registry', publish: '["crates-io"]', registry: undefined },
    ])('publishes a crate whose publish setting names $case with that registry', async ({ publish, registry }, { workspace }) => {
        const graph = await workspace.write({ ...crates, 'rust/crates/kit/Cargo.toml': `[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\npublish = ${publish}\n` }).graph(cargo);

        const data: ProjectConfiguration = graph.nodes['kit']?.data ?? { root: 'rust/crates/kit' };
        const published = await Effect.runPromise(Schema.decodeUnknownEffect(Publish)(data.targets?.['nx-release-publish']?.options));
        expect([published.registry, data.release?.version?.versionActionsOptions?.['registry']]).toStrictEqual([registry, registry]);
    });

    it('publishes a crate after packing it and after publishing each workspace crate it depends on', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        const { tasks } = taskGraph(graph, ['app'], ['nx-release-publish']);

        expect(Object.keys(tasks).toSorted()).toStrictEqual(['app:nx-release-publish', 'app:pack--cargo', 'kit:nx-release-publish', 'kit:pack--cargo']);
    });
});

describe('pack--cargo', { timeout: 120_000 }, () => {
    it('writes the .crate archive it declares for a crate depending on another workspace crate', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: join(workspace.root, 'rust') });

        const { success } = await execute(await targetOptions(Cargo, graph, 'app', 'pack--cargo'), workspace.context(graph, 'app', 'pack--cargo'));

        expect(success).toBe(true);
        expect(outputs(graph, 'app', 'pack--cargo')).toStrictEqual(['rust/target/package/app-0.2.0.crate']);
        expect(existsSync(join(workspace.root, 'rust/target/package/app-0.2.0.crate'))).toBe(true);
    });
});
