import { execFileSync } from 'node:child_process';
import { existsSync, globSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { batch, Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { batch as suites } from '@rasm/nx-cargo/executors/nextest';
import { it, pathed, shimmed, targetOptions, taskGraph } from '@rasm/testing/nx';
import { Effect, Record, Struct } from 'effect';
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

const planned = async (graph: ProjectGraph, targets: readonly string[]) => {
    const scheduled = taskGraph(graph, ['app', 'two'], [...targets]);
    const inputs = await Effect.runPromise(Effect.all(Record.map(scheduled.tasks, ({ target: { project, target } }) => Effect.promise(() => targetOptions(Cargo, graph, project, target)))));
    return { scheduled, inputs };
};

const settled = async (results: AsyncIterable<{ readonly task: string; readonly result: { readonly success: boolean; readonly status?: string; readonly terminalOutput?: string } }>) => Record.fromIterableWith(await Array.fromAsync(results), ({ task, result }) => [task, result] as const);

describe('cargo batch', { timeout: 120_000 }, () => {
    it('runs one cargo process per command for the tasks sharing it, selecting each package by its path package ID', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        const { scheduled, inputs } = await planned(graph, ['build--cargo']);
        shimmed(join(workspace.root, '.shim'), { cargo: `printf '%s\\n' "$*" >> ${join(workspace.root, 'cargo.log')}\nPATH="\${PATH#*:}"\nexec cargo "$@"` });

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        const builds = readFileSync(join(workspace.root, 'cargo.log'), 'utf-8')
            .split('\n')
            .filter((call) => call.startsWith('build '));
        expect(Record.map(results, ({ success }) => success)).toStrictEqual({ 'app:build--cargo': true, 'two:build--cargo': true });
        expect(builds.map((call) => call.split(' '))).toStrictEqual([expect.arrayContaining(['--package', `${pathToFileURL(join(workspace.root, 'crates/app')).href}#app`, `${pathToFileURL(join(workspace.root, 'crates/two')).href}#two`])]);
    });

    it('passes the task of a crate the command emits nothing for where the command succeeds beside a crate it emits for', async ({ workspace }) => {
        const graph = await workspace.write({ ...Struct.omit(crates, ['crates/app/src/main.rs']), 'crates/app/tests/reads.rs': '#[test]\nfn reads_two() {\n    assert_eq!(two::two(), 2);\n}\n' }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, ['check--rustdoc']);

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'check--rustdoc')));

        expect(Record.map(results, ({ success }) => success)).toStrictEqual({ 'app:check--rustdoc': true, 'two:check--rustdoc': true });
    });

    it('fails the task of the crate that does not compile and skips the task of a crate depending on it', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, 'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    absent_function()\n}\n' }).graph(cargo);
        const { scheduled, inputs } = await planned(graph, ['build--cargo']);

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        expect(Record.map(results, ({ success, status }) => ({ success, status }))).toStrictEqual({ 'two:build--cargo': { success: false, status: undefined }, 'app:build--cargo': { success: false, status: 'skipped' } });
        expect(results['two:build--cargo']?.terminalOutput).toContain('absent_function');
    });

    it('fails every task with the reason where cargo is not on PATH', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        const { scheduled, inputs } = await planned(graph, ['build--cargo']);
        pathed(join(workspace.root, '.bin'), {});

        const results = await settled(batch(scheduled, inputs, {}, workspace.context(graph, 'app', 'build--cargo')));

        expect(Object.values(results)).toStrictEqual([
            { success: false, terminalOutput: 'cargo is not on PATH, so no Cargo task can run\n' },
            { success: false, terminalOutput: 'cargo is not on PATH, so no Cargo task can run\n' },
        ]);
    });
});

describe('cargo execute', { timeout: 120_000 }, () => {
    it('stops a task at its first failing command', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        const { success } = await execute({ cwd: '.', package: 'two', commands: [{ args: ['check', '--features', 'absent'] }, { args: ['build', '--message-format', 'json-diagnostic-rendered-ansi'] }] }, workspace.context(graph, 'two', 'check--clippy'));

        expect([success, globSync('target/**/*.rlib', { cwd: workspace.root })]).toStrictEqual([false, []]);
    });

    it('appends the words after -- of a run to the command', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        const { success } = await execute({ ...(await targetOptions(Cargo, graph, 'app', 'build--cargo')), __unparsed__: ['--release'] }, workspace.context(graph, 'app', 'build--cargo'));

        expect(success).toBe(true);
        expect(existsSync(join(workspace.root, 'target/release/app'))).toBe(true);
    });
});

describe('nextest batch', { timeout: 120_000 }, () => {
    it('fails the crate whose test fails and passes the crate whose tests pass', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, 'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n\n#[test]\nfn two_is_three() {\n    assert_eq!(two(), 3);\n}\n' }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, ['test--nextest']);

        const results = await settled(suites(scheduled, inputs, {}, workspace.context(graph, 'app', 'test--nextest')));

        expect(Record.map(results, ({ success }) => success)).toStrictEqual({ 'two:test--nextest': false, 'app:test--nextest': true });
        expect(results['two:test--nextest']?.terminalOutput).toContain('two_is_three');
    });

    it('fails a crate that builds but holds no test, naming it', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, 'crates/two/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n' }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });
        const { scheduled, inputs } = await planned(graph, ['test--nextest']);

        const results = await settled(suites(scheduled, inputs, {}, workspace.context(graph, 'app', 'test--nextest')));

        expect(results).toMatchObject({ 'two:test--nextest': { success: false, terminalOutput: 'two: no tests to run\n' }, 'app:test--nextest': { success: true } });
    });
});
