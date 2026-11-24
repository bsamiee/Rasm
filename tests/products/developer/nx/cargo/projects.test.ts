import { execFileSync, spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { env } from 'node:process';
import { type ProjectGraph, workspaceRoot } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { refused } from '@rasm/testing/errors';
import { it, outputs, pathed, taskGraph, unaggregated, unportable, type Workspace } from '@rasm/testing/nx';
import { Effect, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'Cargo.toml': '[package]\nname = "root-app"\nversion = "0.1.0"\nedition = "2024"\n\n[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'src/main.rs': 'fn main() {}\n',
    'crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", core_kit::one());\n}\n',
    'crates/cli/notes/plan.md': '# plan\n',
};
const shared = {
    'rust-toolchain.toml': crates['rust-toolchain.toml'],
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/common/Cargo.toml': '[package]\nname = "common"\nversion = "0.1.0"\nedition = "2024"\n\n[features]\nextra = []\n',
    'crates/common/src/lib.rs': '#[cfg(feature = "extra")]\ncompile_error!("extra is on");\n\npub fn one() -> u32 {\n    1\n}\n',
    'crates/a/Cargo.toml': '[package]\nname = "a"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncommon = { path = "../common", version = "0.1.0" }\n',
    'crates/a/src/lib.rs': 'pub fn a() -> u32 {\n    common::one()\n}\n',
    'crates/b/Cargo.toml': '[package]\nname = "b"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncommon = { path = "../common", version = "0.1.0", features = ["extra"] }\n',
    'crates/b/src/lib.rs': 'pub fn b() -> u32 {\n    common::one()\n}\n',
};
const Listed = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ manifest_path: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, task: string) => {
    const [project = '', target = ''] = task.split(':');
    const tasks = taskGraph(graph, [project], [target]);
    const found = tasks.tasks[task];
    assert(found !== undefined, `${task} is a task of the graph`);
    return createTaskHasher(graph, readNxJson(workspace.root)).hashTask(found, tasks, env);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('inferred projects', { timeout: 120_000 }, () => {
    it('infers one project per package cargo metadata lists, rooted at the folder of its manifest', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        const { packages } = await Effect.runPromise(Schema.decodeEffect(Listed)(execFileSync('cargo', ['metadata', '--no-deps', '--format-version', '1'], { cwd: workspace.root, encoding: 'utf-8' })));
        expect(
            Object.values(graph.nodes)
                .map(({ data }) => data.root)
                .toSorted(),
        ).toStrictEqual(packages.map(({ manifest_path }) => relative(workspace.root, dirname(manifest_path)) || '.').toSorted());
    });

    it('refuses a member manifest cargo cannot parse, naming its file and line', async ({ workspace }) => {
        const pending = workspace.write({ ...crates, 'crates/broken/Cargo.toml': '[package\nname = ' }).graph(cargo);

        await expect(pending).rejects.toSatisfy(refused(/crates\/broken\/Cargo\.toml:1:\d+/u));
    });

    it('states each tool input in the verb running it, and every root, input, output, and folder as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                'package.json': '{ "name": "repo", "private": true, "workspaces": ["crates/*"] }',
                '.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
                'deny.toml': '[bans]\nmultiple-versions = "deny"\n',
                'crates/cli/package.json': '{ "name": "cli", "nx": { "targets": { "check--clippy": { "inputs": ["default", "{projectRoot}/lints/**/*"] } } } }',
            })
            .graph(cargo, { coveragePath: '.artifacts/coverage/{projectName}' });

        expect(graph.nodes['cli']?.data.targets?.['check--clippy']?.inputs).toContain('{projectRoot}/lints/**/*');
        expect([unaggregated(graph), unportable(graph)]).toStrictEqual([[], []]);
    });
});

describe('task hashes', { timeout: 120_000 }, () => {
    it.for([
        { case: 'the source of a crate it depends on', edited: { 'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    2 - 1\n}\n' }, changes: true },
        { case: 'a note in its folder rustc never reads', edited: { 'crates/cli/notes/plan.md': '# plan, revised\n' }, changes: false },
    ])('changes the build hash of a crate ($changes) after an edit to $case', async ({ edited, changes }, { workspace }) => {
        const before = await hashed(workspace, await workspace.write(crates).graph(cargo), 'cli:build--cargo');

        const after = await hashed(workspace, await workspace.write(edited).graph(cargo), 'cli:build--cargo');

        expect(before.value === after.value).toBe(!changes);
    });

    it.for([
        { case: 'workspace unification under its unstable gate', written: { '.cargo/config.toml': '[unstable]\nfeature-unification = true\n\n[resolver]\nfeature-unification = "workspace"\n' }, stated: {}, unified: true },
        { case: 'workspace unification from the environment over a selected row', written: { '.cargo/config.toml': '[unstable]\nfeature-unification = true\n\n[resolver]\nfeature-unification = "selected"\n' }, stated: { CARGO_RESOLVER_FEATURE_UNIFICATION: 'workspace' }, unified: true },
        { case: 'a workspace row cargo ignores without its unstable gate', written: { '.cargo/config.toml': '[resolver]\nfeature-unification = "workspace"\n' }, stated: {}, unified: false },
        { case: 'the selected unification cargo defaults to', written: {}, stated: {}, unified: false },
    ])('changes the hash of a crate ($unified) when a member it never reaches drops a feature of their shared dependency, under $case', async ({ written, stated, unified }, { workspace, onTestFinished }) => {
        Object.entries(stated).forEach(([name, value]) => {
            vi.stubEnv(name, value);
        });
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const compiles = () => spawnSync('cargo', ['check', '--package', 'a', '--quiet'], { cwd: workspace.root }).status === 0;
        workspace.write({ ...shared, ...written });
        const compiled = compiles();
        const before = await hashed(workspace, await workspace.graph(cargo), 'a:check--clippy');
        workspace.write({ 'crates/b/Cargo.toml': '[package]\nname = "b"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncommon = { path = "../common", version = "0.1.0" }\n' });
        const recompiled = compiles();

        const after = await hashed(workspace, await workspace.graph(cargo), 'a:check--clippy');

        expect([compiled !== recompiled, before.value !== after.value]).toStrictEqual([unified, unified]);
    });

    it.for([
        { case: 'the lock cargo deny reads', edited: { 'Cargo.lock': '# This file is automatically @generated by Cargo.\n# It is not intended for manual editing.\nversion = 3\n' }, changes: true },
        { case: 'a Cargo configuration patching the graph cargo metadata reports', edited: { '.cargo/config.toml': '[patch.crates-io]\ncore-kit = { path = "crates/core-kit" }\n' }, changes: true },
        { case: 'the source of a crate', edited: { 'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    2 - 1\n}\n' }, changes: false },
    ])('caches the advisories check and changes its hash ($changes) after an edit to $case', async ({ edited, changes }, { workspace }) => {
        const audited = { ...crates, 'deny.toml': '[advisories]\n\n[bans]\nmultiple-versions = "deny"\n', '.cargo/config.toml': '[build]\nfingerprint = "content"\n', 'Cargo.lock': '# This file is automatically @generated by Cargo.\n# It is not intended for manual editing.\nversion = 4\n' };
        const graph = await workspace.write(audited).graph(cargo);
        const before = await hashed(workspace, graph, 'root_app:check--cargo-deny-advisories');

        const after = await hashed(workspace, await workspace.write(edited).graph(cargo), 'root_app:check--cargo-deny-advisories');

        expect([graph.nodes['root_app']?.data.targets?.['check--cargo-deny-advisories']?.cache, before.value === after.value]).toStrictEqual([true, !changes]);
    });
});

describe('files Nx hands the plugin', () => {
    it('are the manifests cargo lists alone, so a Cargo, toolchain, clippy, nextest, or deny configuration edit is no project configuration change', async ({ workspace }) => {
        workspace.write({ ...crates, '.cargo/config.toml': '[build]\nfingerprint = "content"\n', '.config/clippy.toml': 'msrv = "1.90"\n', '.config/nextest.toml': '[profile.default]\nfail-fast = false\n', 'deny.toml': '[bans]\nmultiple-versions = "deny"\n' });

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        const { packages } = await Effect.runPromise(Schema.decodeEffect(Listed)(execFileSync('cargo', ['metadata', '--no-deps', '--format-version', '1'], { cwd: workspace.root, encoding: 'utf-8' })));
        expect(matched.toSorted()).toStrictEqual(packages.map(({ manifest_path }) => relative(workspace.root, manifest_path)).toSorted());
    });
});

describe('stored readings', { timeout: 120_000 }, () => {
    it('serves the stored reading while nothing cargo reads changes, with cargo gone from PATH', async ({ workspace }) => {
        await workspace.write(crates).graph(cargo);
        pathed(join(workspace.root, '.bin'), {});

        const graph = await workspace.graph(cargo);

        expect(graph.nodes['cli']?.data.targets).toHaveProperty('build--cargo');
    });

    it.for([
        { case: 'a binary source joins its layout', edited: { 'crates/core-kit/src/bin/report.rs': 'fn main() {\n    println!("{}", core_kit::one());\n}\n' }, target: 'install--cargo', held: [false, true] },
        { case: 'its manifest changes', edited: { 'crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n' }, target: 'nx-release-publish', held: [true, false] },
    ])('reads the crate again once $case', async ({ edited, target, held }, { workspace }) => {
        const before = await workspace.write(crates).graph(cargo);
        workspace.write(edited);

        const after = await workspace.graph(cargo);

        expect([before, after].map(({ nodes }) => [Object.hasOwn(nodes, 'core_kit'), target in (nodes['core_kit']?.data.targets ?? {})])).toStrictEqual(held.map((holds) => [true, holds]));
    });

    it('reads the workspace again once CARGO_TARGET_DIR names another target folder, no file changing', async ({ workspace, onTestFinished }) => {
        await workspace.write(crates).graph(cargo);
        vi.stubEnv('CARGO_TARGET_DIR', join(workspace.root, 'elsewhere'));
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });

        const graph = await workspace.graph(cargo);

        expect(outputs(graph, 'cli', 'build--cargo')).toStrictEqual(['elsewhere/debug/cli']);
    });

    it('reads the workspace again once a Cargo configuration names a build target, no manifest changing', async ({ workspace }) => {
        await workspace.write(crates).graph(cargo);
        const host = execFileSync('rustc', ['--print', 'host-tuple'], { cwd: workspace.root, encoding: 'utf-8' }).trim();
        workspace.write({ '.cargo/config.toml': `[build]\ntarget = "${host}"\n` });

        const graph = await workspace.graph(cargo);

        expect(outputs(graph, 'cli', 'build--cargo')).toStrictEqual([`target/${host}/debug/cli`]);
    });
});
