import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { env } from 'node:process';
import { type ProjectGraph, workspaceRoot } from '@nx/devkit';
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
});
