import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join, posix } from 'node:path';
import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-uv';
import { it, targetOptions, taskGraph, type Workspace } from '@rasm/testing/nx';
import { Array, Record, Schema } from 'effect';
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
const VERSIONED = 'from packaging.version import Version\n\n\ndef parsed(text: str) -> Version:\n    return Version(text)\n';
const DECLARING = {
    'proj/declaring/pyproject.toml': `[project]\nname = "declaring"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["packaging"]\n${BUILT}`,
    'proj/declaring/src/declaring/__init__.py': VERSIONED,
};
const IMPORTING = {
    'proj/importing/pyproject.toml': `[project]\nname = "importing"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${BUILT}`,
    'proj/importing/src/importing/__init__.py': VERSIONED,
};
const BRANCHED = 'def one(flag: bool = True) -> int:\n    if flag:\n        return 1\n    return 0\n';
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

// --- [MODELS] --------------------------------------------------------------------------

const Commanded = Schema.Struct({ command: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspaced = (dev: readonly string[], sourced: readonly string[] = [], test: readonly string[] = []) => ({
    'pyproject.toml': `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ${JSON.stringify(dev)}\ntest = ${JSON.stringify(test)}\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n\n[tool.uv.sources]\n${sourced.map((name) => `${name} = { workspace = true }\n`).join('')}`,
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

describe('checker targets', () => {
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

    it.for([
        { case: 'at the Nx workspace root', folder: '.' },
        { case: 'in a nested folder, below the pytest configuration', folder: 'nested' },
    ])('runs the mypy check and the tests of a member of a uv root $case in one environment per dependency group under environmentPath, each tool cache beside them, and reuses each', async ({ folder }, { workspace, onTestFinished }) => {
        workspace.write({ ...Record.mapKeys({ ...workspaced(['mypy'], [], ['pytest>=8']), ...TESTED }, (file) => posix.join(folder, file)), 'pytest.toml': '[pytest]\n' });
        execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.root, folder) });
        vi.stubEnv('NX_WORKSPACE_ROOT', workspace.root);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const runs = [await workspace.run(graph, 'library', 'check--mypy'), await workspace.run(graph, 'library', 'test--pytest'), await workspace.run(graph, 'library', 'check--mypy'), await workspace.run(graph, 'library', 'test--pytest')];

        const kept = workspace.files('.venv');
        expect(runs.map(({ success }) => success)).toStrictEqual([true, true, true, true]);
        expect(['dev/pyvenv.cfg', 'test/pyvenv.cfg', '.mypy_cache/CACHEDIR.TAG', '.pytest_cache/CACHEDIR.TAG'].filter((file) => !kept.has(`tmp/environments/library/${file}`))).toStrictEqual([]);
    });

    it.for([
        { case: 'environments kept under environmentPath', options: { environmentPath: 'tmp/environments/{projectName}' } },
        { case: 'a temporary environment per run', options: {} },
    ])('fails the mypy check of a member importing a package only another member declares, after that member passed its own, in $case', async ({ options }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy']), ...DECLARING, ...IMPORTING });
        const graph = await workspace.graph(uv, options);

        const [declaring, importing] = [await workspace.run(graph, 'declaring', 'check--mypy'), await workspace.run(graph, 'importing', 'check--mypy')];

        expect([declaring.success, importing.success]).toStrictEqual([true, false]);
    });

    it('checks a uv root that is its only member with mypy and ty in one environment holding every package', async ({ workspace }) => {
        workspace.write({ 'solo/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[dependency-groups]\ndev = ["mypy", "ty"]\n`, 'solo/src/library/__init__.py': LIBRARY['proj/library/src/library/__init__.py'] });
        execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.root, 'solo') });
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const runs = [await workspace.run(graph, 'library', 'check--mypy'), await workspace.run(graph, 'library', 'check--ty')];

        expect([...runs.map(({ success }) => success), workspace.files('.venv').has('tmp/environments/library/dev/pyvenv.cfg')]).toStrictEqual([true, true, true]);
    });

    it('runs the mypy check of a member whose optional dependencies alone hold mypy, as uv run --all-extras installs every extra of the member', async ({ workspace }) => {
        locked(workspace, { ...workspaced([]), 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[project.optional-dependencies]\nlint = ["mypy"]\n` });
        const graph = await workspace.graph(uv);

        const result = await workspace.run(graph, 'library', 'check--mypy');

        expect(result.success).toBe(true);
    });

    it('holds the members of a uv root and the group stating ty in the environment of its ty check, and no other group the root installs by default', async ({ workspace }) => {
        const files = workspaced(['ty'], [], ['iniconfig']);
        locked(workspace, { ...files, 'pyproject.toml': `${files['pyproject.toml']}\n[tool.uv]\ndefault-groups = "all"\n` });
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const result = await workspace.run(graph, 'uv-tutorial', 'check--ty');

        const installed = new Set([...workspace.files('.venv')].filter((file) => file.startsWith('tmp/environments/uv-tutorial/dev/') && file.endsWith('.dist-info/METADATA')).map((file) => posix.basename(posix.dirname(file)).replace(/-[^-]+\.dist-info$/u, '')));
        expect([result.success, installed.has('ty'), installed.has('iniconfig')]).toStrictEqual([true, true, false]);
    });

    it('gives a uv root one ty check over every member, and no member its own', async ({ workspace }) => {
        locked(workspace, { ...workspaced(['ty']), ...APPLICATION });

        const graph = await workspace.graph(uv);
        const result = await workspace.run(graph, 'uv-tutorial', 'check--ty');

        expect([result.success, ...['uv-tutorial', 'library', 'application'].map((project) => 'check--ty' in (graph.nodes[project]?.data.targets ?? {}))]).toStrictEqual([true, true, false, false]);
    });

    it.for([
        { file: 'uv.toml', content: 'compile-bytecode = false\n' },
        { file: '.python-version', content: '3.13\n' },
        { file: '.python-versions', content: '3.13\n' },
    ])('changes the hash of a checker, of the lock check, and of a build after $file appears at the uv root', async ({ file, content }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy']), ...APPLICATION });
        const tasks = ['application:check--mypy', 'uv-tutorial:check--uv', 'application:build--uv'];
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
        { case: 'a mypy.ini in the member root', project: 'library', target: 'check--mypy', source: 'def one():\n    return 1\n', edited: { 'proj/library/mypy.ini': '[mypy]\ndisallow_untyped_defs = True\n' } },
        { case: 'a mypy table in the member manifest', project: 'library', target: 'check--mypy', source: 'def one():\n    return 1\n', edited: { 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[tool.mypy]\ndisallow_untyped_defs = true\n` } },
        { case: 'a ty.toml at the uv root', project: 'uv-tutorial', target: 'check--ty', source: 'def one() -> float:\n    return 1 / 0\n', edited: { 'ty.toml': '[rules]\ndivision-by-zero = "error"\n' } },
    ])('changes the $target hash and result of $project in the graph built before $case', async ({ project, target, source, edited }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy', 'ty']), 'proj/library/src/library/__init__.py': source });
        const graph = await workspace.graph(uv);
        const [before, passed] = [await hashed(workspace, graph, `${project}:${target}`), await workspace.run(graph, project, target)];

        await workspace.write(edited).graph(uv);

        const [after, failed] = [await hashed(workspace, graph, `${project}:${target}`), await workspace.run(graph, project, target)];
        expect([passed.success, failed.success, after.value === before.value]).toStrictEqual([true, false, false]);
    });

    it.for([
        { case: 'a mypy.ini appears in the folder refurb builds mypy from', task: 'library:check--refurb', files: workspaced(['refurb']), edited: { 'proj/library/mypy.ini': '[mypy]\ndisallow_untyped_defs = True\n' } },
        { case: 'an edit to a member manifest, which ty reads through uv and no createNodes pattern matches', task: 'uv-tutorial:check--ty', files: workspaced(['ty']), edited: { 'proj/library/pyproject.toml': `${LIBRARY['proj/library/pyproject.toml']}\n[project.urls]\nSource = "https://example.invalid"\n` } },
        { case: 'an edit to a dependency of a workspace member its dependency group installs', task: 'library:test--pytest', files: { ...workspaced(['pytest>=8', 'helper'], ['helper']), ...WORDS, ...HELPER, ...TESTED }, edited: { 'proj/words/src/words/__init__.py': 'WORD = "two"\n' } },
    ])('changes the hash of $task after $case', async ({ task, files, edited }, { workspace }) => {
        locked(workspace, files);
        const before = await hashed(workspace, await workspace.graph(uv), task);

        const after = await hashed(workspace, await workspace.write(edited).graph(uv), task);

        expect(after.value).not.toBe(before.value);
    });

    it.for([
        { case: 'the source of a refurb plugin', edited: { 'proj/checks/src/checks/__init__.py': 'NAME = "renamed"\n' }, changed: { 'library:check--refurb': true, 'library:check--mypy': false, 'uv-tutorial:check--ty': true } },
        { case: 'a stubs package', edited: { 'proj/words-stubs/src/words-stubs/__init__.pyi': 'WORD: int\n' }, changed: { 'library:check--refurb': true, 'library:check--mypy': true, 'uv-tutorial:check--ty': true } },
    ])('changes the checker hashes as $changed after an edit to $case the dependency group of a member installs', async ({ edited, changed }, { workspace }) => {
        locked(workspace, { ...workspaced(['mypy', 'refurb', 'ty', 'checks', 'words-stubs'], ['checks', 'words-stubs']), ...CHECKS, ...STUBS });
        const tasks = Object.keys(changed);
        const graph = await workspace.graph(uv);
        const before = await Promise.all(tasks.map((task) => hashed(workspace, graph, task)));

        const edits = await workspace.write(edited).graph(uv);

        const after = await Promise.all(tasks.map((task) => hashed(workspace, edits, task)));
        expect(Object.fromEntries(tasks.map((task, index) => [task, after[index]?.value !== before[index]?.value]))).toStrictEqual(changed);
    });

    it('runs refurb without a mypy cache, as the mypy build refurb runs reads and writes none', async ({ workspace }) => {
        locked(workspace, workspaced(['refurb']));
        const graph = await workspace.graph(uv, { environmentPath: 'tmp/environments/{projectName}' });

        const result = await workspace.run(graph, 'library', 'check--refurb');

        expect(result.success).toBe(true);
        expect([...workspace.files('.venv')].filter((file) => file.includes('.mypy_cache/'))).toStrictEqual([]);
    });

    it('passes mypy the lowest Python minor the requires-python range of each member admits, as packaging.specifiers works it', async ({ workspace }) => {
        const ranges = { floored: '>=3.13', wildcard: '==3.13.*', compatible: '~=3.13.1', above: '>3.12', bounded: '>=3.13,<3.16', excluding: '!=3.14,>=3.13' };
        const members = Record.toEntries(ranges).flatMap(([name, range]): readonly (readonly [string, string])[] => [
            [`proj/${name}/pyproject.toml`, `[project]\nname = "${name}"\nversion = "0.1.0"\nrequires-python = "${range}"\n${BUILT}`],
            [`proj/${name}/src/${name}/__init__.py`, ''],
        ]);
        locked(workspace, { ...workspaced(['mypy']), ...Record.fromEntries(members) });

        const graph = await workspace.graph(uv);

        const options = await Promise.all(Record.keys(ranges).map((name) => targetOptions(Commanded, graph, name, 'check--mypy')));
        const floors = Array.zipWith(Record.keys(ranges), options, (name, { command }) => [name, /--python-version (?<minor>\S+)/u.exec(command)?.groups?.['minor']] as const);
        expect(Record.fromEntries(floors)).toStrictEqual({ floored: '3.13', wildcard: '3.13', compatible: '3.13', above: '3.12', bounded: '3.13', excluding: '3.13' });
    });

    it.for([
        { variable: 'PYTEST_ADDOPTS', value: '-k smoke', dev: ['pytest>=8'], tasks: ['library:test--pytest'] },
        { variable: 'COVERAGE_RCFILE', value: 'other.ini', dev: ['pytest>=8'], tasks: ['library:test--pytest'] },
        { variable: 'MYPYPATH', value: 'stubs', dev: ['mypy', 'refurb'], tasks: ['library:check--mypy', 'library:check--refurb'] },
        { variable: 'TY_CONFIG_FILE', value: 'other.toml', dev: ['ty'], tasks: ['uv-tutorial:check--ty'] },
    ])('changes the hash of $tasks once $variable is set', async ({ variable, value, dev, tasks }, { workspace, onTestFinished }) => {
        locked(workspace, { ...workspaced(dev), ...TESTED });
        const graph = await workspace.graph(uv);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const before = await Promise.all(tasks.map((task) => hashed(workspace, graph, task)));
        vi.stubEnv(variable, value);

        const after = await Promise.all(tasks.map((task) => hashed(workspace, graph, task)));

        expect(after.map(({ value: hash }, index) => hash === before[index]?.value)).toStrictEqual(tasks.map(() => false));
    });

    it.for([
        { case: 'a conftest.py in a folder above it', file: 'proj/conftest.py', content: '' },
        { case: 'a .coveragerc at the uv root', file: '.coveragerc', content: '[run]\nbranch = True\n' },
    ])('changes the pytest hash of a member in the graph built before $case', async ({ file, content }, { workspace }) => {
        locked(workspace, { ...workspaced(['pytest>=8']), ...TESTED });
        const graph = await workspace.graph(uv);
        const before = await hashed(workspace, graph, 'library:test--pytest');

        await workspace.write({ [file]: content }).graph(uv);

        const after = await hashed(workspace, graph, 'library:test--pytest');
        expect(after.value).not.toBe(before.value);
    });

    it('measures branches in the lcov report of a suite in a nested uv root, as the coverage table of a manifest above it states', async ({ workspace }) => {
        workspace.write({ 'pyproject.toml': '[tool.coverage.run]\nbranch = true\n', ...Record.mapKeys({ ...workspaced(['pytest>=8', 'pytest-cov']), ...TESTED, 'proj/library/src/library/__init__.py': BRANCHED }, (file) => posix.join('nested', file)) });
        execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.root, 'nested') });
        const graph = await workspace.graph(uv, { coveragePath: '.artifacts/coverage/{projectName}' });

        const result = await workspace.run(graph, 'library', 'test--pytest', 'coverage');

        const report = readFileSync(join(workspace.root, '.artifacts/coverage/library/lcov.info'), 'utf-8').split('\n');
        expect([result.success, report.filter((row) => row.startsWith('SF:')), report.some((row) => row.startsWith('BRDA:'))]).toStrictEqual([true, ['SF:nested/proj/library/src/library/__init__.py', 'SF:nested/proj/library/tests/test_library.py'], true]);
    });

    it('measures a member outside the folder of its uv root under the coverage table of that root, each source of the report relative to the Nx workspace root', async ({ workspace }) => {
        const { 'pyproject.toml': manifest, ...library } = Record.mapKeys({ ...workspaced(['pytest>=8', 'pytest-cov']), ...TESTED, 'proj/library/src/library/__init__.py': BRANCHED }, (file) => file.replace('proj/', 'apart/'));
        workspace.write({ ...library, 'nested/pyproject.toml': `${manifest?.replace('proj/*', '../apart/*')}\n[tool.coverage.run]\nbranch = true\n` });
        execFileSync('uv', ['lock', '--quiet'], { cwd: join(workspace.root, 'nested') });
        const graph = await workspace.graph(uv, { coveragePath: '.artifacts/coverage/{projectName}' });

        const result = await workspace.run(graph, 'library', 'test--pytest', 'coverage');

        const report = readFileSync(join(workspace.root, '.artifacts/coverage/library/lcov.info'), 'utf-8').split('\n');
        expect([result.success, report.filter((row) => row.startsWith('SF:')), report.some((row) => row.startsWith('BRDA:'))]).toStrictEqual([true, ['SF:apart/library/src/library/__init__.py', 'SF:apart/library/tests/test_library.py'], true]);
    });
});
