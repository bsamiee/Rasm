import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { batch, Cargo } from '@rasm/nx-cargo/executors/cargo';
import { batch as suites } from '@rasm/nx-cargo/executors/nextest';
import { it, targetOptions, taskGraph } from '@rasm/testing/nx';
import { Effect, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    '.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
    'crates/two/Cargo.toml': '[package]\nname = "two"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n\n#[test]\nfn two_is_two() {\n    assert_eq!(two(), 2);\n}\n',
    'crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ntwo = { path = "../two", version = "0.1.0" }\n',
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", two::two());\n}\n\n#[test]\nfn reads_two() {\n    assert_eq!(two::two(), 2);\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = async (graph: ProjectGraph, target: string) => {
    const scheduled = taskGraph(graph, ['app', 'two'], [target]);
    const inputs = await Effect.runPromise(Effect.all(Record.map(scheduled.tasks, ({ target: { project } }) => Effect.promise(() => targetOptions(Cargo, graph, project, target)))));
    return { scheduled, inputs };
};

const settled = async (results: AsyncIterable<{ readonly task: string; readonly result: { readonly success: boolean; readonly status?: string; readonly terminalOutput?: string } }>) => Record.fromIterableWith(await Array.fromAsync(results), ({ task, result }) => [task, result] as const);

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
        const { scheduled, inputs } = await planned(graph, 'build--cargo');

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        expect(Record.map(results, ({ success, status }) => ({ success, status }))).toStrictEqual({ 'two:build--cargo': { success: false, status: undefined }, 'app:build--cargo': { success: false, status: 'skipped' } });
        expect([results['two:build--cargo']?.terminalOutput?.includes('absent_function'), existsSync(join(workspace.root, 'target/debug/app'))]).toStrictEqual([true, true]);
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
});
