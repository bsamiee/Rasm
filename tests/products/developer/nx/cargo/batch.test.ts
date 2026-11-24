import { execFileSync, spawnSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { type ProjectGraph, workspaceRoot } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { batch, Cargo } from '@rasm/nx-cargo/executors/cargo';
import { batch as suites } from '@rasm/nx-cargo/executors/nextest';
import { it, spanned, targetOptions, taskGraph } from '@rasm/testing/nx';
import { Effect, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const UNIFIED = '[unstable]\nfeature-unification = true\n\n[resolver]\nfeature-unification = "workspace"\n';
const crates = {
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    '.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
    '.cargo/config.toml': UNIFIED,
    'crates/two/Cargo.toml': '[package]\nname = "two"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n\n#[test]\nfn two_is_two() {\n    assert_eq!(two(), 2);\n}\n',
    'crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ntwo = { path = "../two", version = "0.1.0" }\n',
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", two::two());\n}\n\n#[test]\nfn reads_two() {\n    assert_eq!(two::two(), 2);\n}\n',
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = async (graph: ProjectGraph, target: string, projects: readonly string[] = ['app', 'two']) => {
    const scheduled = taskGraph(graph, projects, [target]);
    const inputs = await Effect.runPromise(Effect.all(Record.map(scheduled.tasks, ({ target: { project } }) => Effect.promise(() => targetOptions(Cargo, graph, project, target)))));
    return { scheduled, inputs };
};

const settled = async (results: AsyncIterable<{ readonly task: string; readonly result: { readonly success: boolean; readonly status?: string; readonly terminalOutput?: string; readonly startTime?: number; readonly endTime?: number } }>) =>
    Record.fromIterableWith(await Array.fromAsync(results), ({ task, result }) => [task, result] as const);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('cargo batch', { timeout: 120_000 }, () => {
    it('fails the crate that does not compile with its own diagnostic, and skips the dependent whose own build succeeds beside it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                'crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dev-dependencies]\ntwo = { path = "../two", version = "0.1.0" }\n',
                'crates/app/src/main.rs': 'fn main() {}\n\n#[test]\nfn reads_two() {\n    assert_eq!(two::two(), 2);\n}\n',
                'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    absent_function()\n}\n',
            })
            .graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, 'build--cargo');

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        expect(Record.map(results, ({ success, status }) => ({ success, status }))).toStrictEqual({ 'two:build--cargo': { success: false, status: undefined }, 'app:build--cargo': { success: false, status: 'skipped' } });
        expect([results['two:build--cargo']?.terminalOutput?.includes('absent_function'), existsSync(join(workspace.root, 'target/debug/app'))]).toStrictEqual([true, true]);
    });

    it('stamps each crate from the start of its command to the last message of its own package, the dependency ending before its dependent', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, 'build--cargo');
        const before = vi.getRealSystemTime();

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        const after = vi.getRealSystemTime();
        expect(results).toStrictEqual({ 'two:build--cargo': spanned(before, after), 'app:build--cargo': spanned(before, after) });
        expect((results['app:build--cargo']?.endTime ?? 0) - (results['two:build--cargo']?.endTime ?? 0)).toBeGreaterThan(0);
    });

    it.for([
        { case: 'the selected unification cargo defaults to', written: {}, alone: { 'a:build--cargo': true, 'b:build--cargo': false } },
        { case: 'workspace unification', written: { '.cargo/config.toml': UNIFIED }, alone: { 'a:build--cargo': false, 'b:build--cargo': false } },
    ])('decides each crate as cargo builds it alone, where another selected crate turns on a feature of their shared dependency, under $case', async ({ written, alone }, { workspace }) => {
        const graph = await workspace.write({ ...shared, ...written }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const built = Record.fromIterableWith(['a', 'b'], (crate) => [`${crate}:build--cargo`, spawnSync('cargo', ['build', '--package', crate, '--quiet'], { cwd: workspace.root }).status === 0] as const);
        const { scheduled, inputs } = await planned(graph, 'build--cargo', ['a', 'b']);

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'a', 'build--cargo')));

        expect([built, Record.map(results, ({ success }) => success)]).toStrictEqual([alone, alone]);
    });
});

describe('nextest batch', { timeout: 120_000 }, () => {
    it.for([
        { case: 'its own failing test', source: 'pub fn two() -> u32 {\n    2\n}\n\n#[test]\nfn two_is_three() {\n    assert_eq!(two(), 3);\n}\n', said: 'two_is_three' },
        { case: 'no test at all, though it builds', source: 'pub fn two() -> u32 {\n    2\n}\n', said: 'two: no tests to run\n' },
    ])('fails the crate holding $case and passes the crate whose tests pass', async ({ source, said }, { workspace }) => {
        const graph = await workspace.write({ ...crates, 'crates/two/src/lib.rs': source }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, 'test--nextest');

        const results = await settled(suites(scheduled, inputs, {}, workspace.context(graph, 'app', 'test--nextest')));

        expect(Record.map(results, ({ success }) => success)).toStrictEqual({ 'two:test--nextest': false, 'app:test--nextest': true });
        expect([results['two:test--nextest']?.terminalOutput?.includes(said), results['app:test--nextest']?.terminalOutput?.includes(said)]).toStrictEqual([true, false]);
    });

    it('stamps each crate from the start of its command to the suite events of its own package, inside the batch', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, 'test--nextest');
        const before = vi.getRealSystemTime();

        const results = await settled(suites(scheduled, inputs, {}, workspace.context(graph, 'app', 'test--nextest')));

        const after = vi.getRealSystemTime();
        expect(results).toStrictEqual({ 'two:test--nextest': spanned(before, after), 'app:test--nextest': spanned(before, after) });
    });
});
