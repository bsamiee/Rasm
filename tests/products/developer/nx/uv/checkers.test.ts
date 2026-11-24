import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-uv';
import { it, taskGraph, type Workspace } from '@rasm/testing/nx';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const uv = { createNodes };
const BUILT = '\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n';
const LIBRARY = {
    'proj/library/pyproject.toml': `[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
};
const APPLICATION = {
    'proj/application/pyproject.toml': `[project]\nname = "application"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["library"]\n\n[tool.uv.sources]\nlibrary = { workspace = true }\n${BUILT}`,
    'proj/application/src/application/__init__.py': 'from library import one\n\n\ndef two() -> int:\n    return one() + 1\n',
};
const TESTED = { 'proj/library/tests/test_library.py': 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n' };
const WORDS = {
    'proj/words/pyproject.toml': `[project]\nname = "words"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'proj/words/src/words/__init__.py': 'WORD = "one"\n',
};
const HELPER = {
    'proj/helper/pyproject.toml': `[project]\nname = "helper"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["words"]\n\n[tool.uv.sources]\nwords = { workspace = true }\n${BUILT}`,
    'proj/helper/src/helper/__init__.py': 'from words import WORD\n',
};
const CHECKS = {
    'proj/checks/pyproject.toml': `[project]\nname = "checks"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[project.entry-points."refurb.plugins"]\nchecks = "checks"\n${BUILT}`,
    'proj/checks/src/checks/__init__.py': 'NAME = "checks"\n',
};
const STUBS = {
    'proj/words-stubs/pyproject.toml': `[project]\nname = "words-stubs"\nversion = "0.1.0"\nrequires-python = ">=3.13"\nclassifiers = ["Typing :: Stubs Only"]\n${BUILT}`,
    'proj/words-stubs/src/words-stubs/__init__.pyi': 'WORD: str\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspaced = (dev: readonly string[], sourced: readonly string[] = []) => ({
    'pyproject.toml': `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ${JSON.stringify(dev)}\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n\n[tool.uv.sources]\n${sourced.map((name) => `${name} = { workspace = true }\n`).join('')}`,
    ...LIBRARY,
});

const locked = (workspace: Workspace, files: Readonly<Record<string, string>>) => execFileSync('uv', ['lock', '--quiet'], { cwd: workspace.write(files).root });

const hashed = (workspace: Workspace, graph: ProjectGraph, task: string) => {
    const [project = '', target = ''] = task.split(':');
    const tasks = taskGraph(graph, [project], [target]);
    const found = tasks.tasks[task];
    assert(found !== undefined, `${task} is a task of the graph`);
    return createTaskHasher(graph, readNxJsonFromDisk(workspace.root)).hashTask(found, tasks, env);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('checker targets', { timeout: 120_000 }, () => {
    it('writes the coverage of a pytest run with pytest-cov locked to the outputs its coverage configuration declares under coveragePath', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8', 'pytest-cov']), ...TESTED });
        const graph = await workspace.graph(uv, { coveragePath: '.artifacts/coverage/{projectName}' });
        const before = workspace.files('.venv', 'tmp');

        const result = await workspace.run(graph, 'library', 'test--pytest', 'coverage');

        const written = [...workspace.files('.venv', 'tmp').difference(before)].filter((file) => !/(?:^|\/)(?:__pycache__|\.pytest_cache)\//u.test(file));
        expect(result.success).toBe(true);
        expect([...new Set(written).difference(workspace.cached(graph, 'library', 'test--pytest', 'coverage'))]).toStrictEqual([]);
        expect(written.toSorted()).toStrictEqual(['.artifacts/coverage/library/.coverage', '.artifacts/coverage/library/lcov.info']);
    });

    it('runs pytest in the environment environmentPath names, which a second run reuses at the locked selection', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8']), ...TESTED });
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const [first, second] = [await workspace.run(graph, 'library', 'test--pytest'), await workspace.run(graph, 'library', 'test--pytest')];

        expect([first.success, second.success, workspace.files('.venv').has('tmp/environments/library/pyvenv.cfg')]).toStrictEqual([true, true, true]);
    });

    it.for([
        { case: 'its own source', edited: { 'proj/application/src/application/__init__.py': 'from library import one\n\n\ndef three() -> int:\n    return one() + 2\n' }, changes: true },
        { case: 'a dependency source vulture never reads', edited: { 'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 2 - 1\n' }, changes: false },
    ])('changes the vulture hash of a member ($changes) after an edit to $case', async ({ edited, changes }, { workspace }) => {
        locked(workspace, { ...workspaced(['vulture']), ...APPLICATION });
        const before = await hashed(workspace, await workspace.graph(uv), 'application:check--vulture');

        const after = await hashed(workspace, await workspace.write(edited).graph(uv), 'application:check--vulture');

        expect(before.value === after.value).toBe(!changes);
    });

    it.for([
        { file: 'uv.toml', content: 'compile-bytecode = false\n' },
        { file: '.python-version', content: '3.13\n' },
        { file: '.python-versions', content: '3.13\n' },
    ])('changes the hash of a checker, of the lock check, and of a build after $file appears at the uv root', async ({ file, content }, { workspace }) => {
        locked(workspace, { ...workspaced(['vulture']), ...APPLICATION });
        const tasks = ['application:check--vulture', 'uv-tutorial:check--uv', 'application:build--uv'];
        const graph = await workspace.graph(uv);
        const before = await Promise.all(tasks.map((task) => hashed(workspace, graph, task)));

        const edited = await workspace.write({ [file]: content }).graph(uv);

        const after = await Promise.all(tasks.map((task) => hashed(workspace, edited, task)));
        expect(after.map(({ value }, index) => value === before[index]?.value)).toStrictEqual([false, false, false]);
    });

    it('keeps the mypy hash of a member after a .python-version in the member root, whose minor uv run never picks', async ({ workspace }) => {
        locked(workspace, workspaced(['mypy']));
        const picked = () => execFileSync('uv', ['run', '--no-active', '--quiet', 'python', '-c', 'import sys; print(sys.version_info[:2])'], { cwd: join(workspace.root, 'proj/library'), encoding: 'utf-8' });
        const [before, interpreter] = [await hashed(workspace, await workspace.graph(uv), 'library:check--mypy'), picked()];

        const after = await hashed(workspace, await workspace.write({ 'proj/library/.python-version': '3.14\n' }).graph(uv), 'library:check--mypy');

        expect([picked(), after.value]).toStrictEqual([interpreter, before.value]);
    });

    it.for([
        { case: 'a mypy.ini in the member root', target: 'check--mypy', source: 'def one():\n    return 1\n', edited: { 'proj/library/mypy.ini': '[mypy]\ndisallow_untyped_defs = True\n' } },
        { case: 'a mypy table in the member manifest', target: 'check--mypy', source: 'def one():\n    return 1\n', edited: { 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[tool.mypy]\ndisallow_untyped_defs = true\n` } },
        { case: 'a ty.toml at the uv root', target: 'check--ty', source: 'def one() -> float:\n    return 1 / 0\n', edited: { 'ty.toml': '[rules]\ndivision-by-zero = "error"\n' } },
    ])('changes the $target hash and result of a member in the graph built before $case', async ({ target, source, edited }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy', 'ty']), 'proj/library/src/library/__init__.py': source });
        const graph = await workspace.graph(uv);
        const [before, passed] = [await hashed(workspace, graph, `library:${target}`), await workspace.run(graph, 'library', target)];

        await workspace.write(edited).graph(uv);

        const [after, failed] = [await hashed(workspace, graph, `library:${target}`), await workspace.run(graph, 'library', target)];
        expect([passed.success, failed.success, after.value === before.value]).toStrictEqual([true, false, false]);
    });

    it('changes the refurb hash of a member after a mypy.ini appears in the folder refurb builds mypy from', async ({ workspace }) => {
        locked(workspace, workspaced(['refurb']));
        const before = await hashed(workspace, await workspace.graph(uv), 'library:check--refurb');

        const after = await hashed(workspace, await workspace.write({ 'proj/library/mypy.ini': '[mypy]\ndisallow_untyped_defs = True\n' }).graph(uv), 'library:check--refurb');

        expect(after.value).not.toBe(before.value);
    });

    it('changes the ty hash of a member after an edit to its manifest, which ty reads through uv and no createNodes pattern matches', async ({ workspace }) => {
        locked(workspace, workspaced(['ty']));
        const before = await hashed(workspace, await workspace.graph(uv), 'library:check--ty');

        const after = await hashed(workspace, await workspace.write({ 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[project.urls]\nSource = "https://example.invalid"\n` }).graph(uv), 'library:check--ty');

        expect(after.value).not.toBe(before.value);
    });

    it('changes the pytest hash of a member after an edit to a dependency of a workspace member its dependency group installs', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8', 'helper'], ['helper']), ...WORDS, ...HELPER, ...TESTED });
        const before = await hashed(workspace, await workspace.graph(uv), 'library:test--pytest');

        const after = await hashed(workspace, await workspace.write({ 'proj/words/src/words/__init__.py': 'WORD = "two"\n' }).graph(uv), 'library:test--pytest');

        expect(after.value).not.toBe(before.value);
    });

    it.for([
        { case: 'the source of a refurb plugin', edited: { 'proj/checks/src/checks/__init__.py': 'NAME = "renamed"\n' }, changed: { 'check--refurb': true, 'check--mypy': false, 'check--ty': false } },
        { case: 'a stubs package', edited: { 'proj/words-stubs/src/words-stubs/__init__.pyi': 'WORD: int\n' }, changed: { 'check--refurb': true, 'check--mypy': true, 'check--ty': true } },
    ])('changes the checker hashes of a member as $changed after an edit to $case its dependency group installs', async ({ edited, changed }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy', 'refurb', 'ty', 'checks', 'words-stubs'], ['checks', 'words-stubs']), ...CHECKS, ...STUBS });
        const tasks = Object.keys(changed).map((target) => `library:${target}`);
        const graph = await workspace.graph(uv);
        const before = await Promise.all(tasks.map((task) => hashed(workspace, graph, task)));

        const edits = await workspace.write(edited).graph(uv);

        const after = await Promise.all(tasks.map((task) => hashed(workspace, edits, task)));
        expect(Object.fromEntries(Object.keys(changed).map((target, index) => [target, after[index]?.value !== before[index]?.value]))).toStrictEqual(changed);
    });

    it('runs refurb with no mypy cache folder under the member, as the mypy build refurb runs reads and writes none', async ({ workspace }) => {
        locked(workspace, workspaced(['refurb']));
        const graph = await workspace.graph(uv);
        const before = workspace.files('.venv', 'tmp');

        const result = await workspace.run(graph, 'library', 'check--refurb');

        expect(result.success).toBe(true);
        expect([...workspace.files('.venv', 'tmp').difference(before)].filter((file) => file.startsWith('proj/library/.mypy_cache/proj/library/'))).toStrictEqual([]);
    });

    it('changes the pytest hash of a member once PYTEST_ADDOPTS selects other tests', async ({ workspace, onTestFinished }) => {
        locked(workspace, { ...workspaced(['pytest>=8']), ...TESTED });
        const graph = await workspace.graph(uv);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const before = await hashed(workspace, graph, 'library:test--pytest');
        vi.stubEnv('PYTEST_ADDOPTS', '-k smoke');

        const after = await hashed(workspace, graph, 'library:test--pytest');

        expect(after.value).not.toBe(before.value);
    });
});
