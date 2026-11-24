import { readFileSync, writeFileSync } from 'node:fs';
import { delimiter, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { environment, taskGraph } from '@rasm/testing/nx';
import { Option } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, onTestFinished, vi } from 'vitest';
import { it, mode, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';
const TARGETS = fileURLToPath(new URL('targets.ts', import.meta.resolve('@rasm/nx-dotnet/package.json')));
const PLUGIN = 'products/developer/nx/dotnet';
const SUITE = 'tests/products/developer/nx/dotnet';
const TESTED = {
    'libs/Lib/Lib.csproj': PROJECT,
    'libs/Lib/Code.cs': 'class Code;\n',
    'tests/Tests/Tests.csproj': PROJECT,
    'tests/Tests/Case.cs': 'class Case;\n',
    'Directory.Build.props': '<Project />\n',
    '.globalconfig': 'is_global = true\n',
    'out/Lib/Lib.dll': 'lib',
    'out/Tests/Tests.dll': 'tests',
    'obj/Tests/Tests.sarif': '{ "runs": [] }',
    ...report([
        node('libs/Lib/Lib.csproj', { inputs: ['.globalconfig', 'Directory.Build.props'], loaded: ['Directory.Build.props'], evaluated: mode('Debug', { outputs: ['out/Lib'] }) }),
        node('tests/Tests/Tests.csproj', {
            test: true,
            inputs: ['.globalconfig', 'Directory.Build.props'],
            loaded: ['Directory.Build.props'],
            references: [{ root: 'libs/Lib', declared: 'tests/Tests/Tests.csproj' }],
            evaluated: mode('Debug', { outputs: ['out/Tests'], reports: ['obj/Tests/Tests.sarif'] }),
        }),
    ]),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const hasher = (root: string, graph: ProjectGraph, target: string, project = 'Lib') => {
    const tasks = taskGraph(graph, [project], [target]);
    const task = Option.getOrThrow(Option.fromUndefinedOr(tasks.tasks[`${project}:${target}`]));
    return async (set: Readonly<Record<string, string>>) => {
        const { value } = await createTaskHasher(graph, readNxJson(root)).hashTask(task, tasks, environment(root, set));
        return value;
    };
};

const stated = (folder: string, target: string) => ({ [`${folder}/package.json`]: JSON.stringify({ ...JSON.parse(readFileSync(join(import.meta.dirname, '../../../../..', folder, 'package.json'), 'utf-8')), scripts: { [target]: 'true' } }) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task hashes', () => {
    it.for([
        { target: 'build--dotnet', split: true },
        { target: 'pack--dotnet', split: true },
        { target: 'test--dotnet', split: true },
        { target: 'check--dotnet-build', split: true },
        { target: 'check--dotnet-format', split: false },
        { target: 'fix--dotnet-format', split: false },
    ])('hashes in $target the CI variable MSBuild reads as the property $(CI) only where the target reads what the compile it changes wrote', async ({ target, split }, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true })]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);

        const [local, ci, again] = [await hashed({ CI: 'false' }), await hashed({ CI: 'true' }), await hashed({ CI: 'false' })];

        expect([local === ci, local === again]).toStrictEqual([!split, true]);
    });

    it.for(['build--dotnet', 'test--dotnet'])('hashes in %s each variable an NxEnvironmentInput item names, and no other', async (target, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { test: true, environment: ['HOST_INSTALLER'] })]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);

        const [first, next, unnamed, again] = [await hashed({ HOST_INSTALLER: 'host_1.exe' }), await hashed({ HOST_INSTALLER: 'host_2.exe' }), await hashed({ HOST_INSTALLER: 'host_1.exe', OTHER_INSTALLER: 'other.exe' }), await hashed({ HOST_INSTALLER: 'host_1.exe' })];

        expect([first === next, first === unnamed, first === again]).toStrictEqual([false, true, true]);
    });

    it.for(['build--dotnet', 'check--dotnet-format'])('hashes in %s the version of the SDK dotnet resolves', async (target, { workspace, analyzer }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj')]) }).graph({ createNodes, createDependencies });
        const hashed = hasher(workspace.root, graph, target);
        const resolving = (version: string) => {
            writeFileSync(join(analyzer.folder, 'version'), version);
            return hashed({ PATH: [analyzer.folder, environment(workspace.root).PATH].join(delimiter) });
        };

        const [first, next, again] = [await resolving('10.0.100'), await resolving('11.0.100'), await resolving('10.0.100')];

        expect([first === next, first === again]).toStrictEqual([false, true]);
    });

    it.for([
        { edited: '.globalconfig', text: 'is_global = true\ndotnet_diagnostic.CA1000.severity = none\n', build: true, test: false },
        { edited: 'Directory.Build.props', text: '<Project><PropertyGroup><Deterministic>true</Deterministic></PropertyGroup></Project>\n', build: true, test: true },
        { edited: 'tests/Tests/Case.cs', text: 'class Case { }\n', build: true, test: true },
        { edited: 'libs/Lib/Code.cs', text: 'class Code { }\n', build: true, test: false },
        { edited: 'out/Lib/Lib.dll', text: 'lib rebuilt', build: false, test: true },
        { edited: 'out/Tests/Tests.dll', text: 'tests rebuilt', build: false, test: true },
        { edited: 'obj/Tests/Tests.sarif', text: '{ "runs": [{ "results": [] }] }', build: false, test: false },
    ])('hashes in test--dotnet the binaries the builds wrote and the files loading the test project reads, so editing $edited reruns the build ($build) and the tests ($test)', async ({ edited, text, build, test }, { workspace }) => {
        const before = await workspace.write(TESTED).graph({ createNodes, createDependencies });
        const [built, tested] = [await hasher(workspace.root, before, 'build--dotnet', 'Tests')({}), await hasher(workspace.root, before, 'test--dotnet', 'Tests')({})];

        const after = await workspace.write({ [edited]: text }).graph({ createNodes, createDependencies });

        expect([built !== (await hasher(workspace.root, after, 'build--dotnet', 'Tests')({})), tested !== (await hasher(workspace.root, after, 'test--dotnet', 'Tests')({}))]).toStrictEqual([build, test]);
    });
});

describe('analyzer binary', () => {
    it('adds no edge from the plugin package to the project publishing into the analyzer folder, and no build of that project before upgrade--dotnet', async ({ workspace }) => {
        vi.resetModules();
        vi.doMock(TARGETS, async (actual) => ({ ...(await actual<Record<string, unknown>>()), ANALYZER: join(workspace.root, 'plugin/analyzer/dist/MsbuildAnalyzer.dll') }));
        onTestFinished(() => {
            vi.doUnmock(TARGETS);
        });
        const loaded = await import('@rasm/nx-dotnet');

        const graph = await workspace
            .write({
                'package.json': JSON.stringify({ name: '@fixture/root', workspaces: ['plugin'] }),
                'Workspace.slnx': '<Solution />\n',
                'plugin/package.json': JSON.stringify({ name: '@fixture/plugin' }),
                'plugin/analyzer/Analyzer.csproj': PROJECT,
                'plugin/analyzer/dist/MsbuildAnalyzer.dll': 'built',
                ...report([node('plugin/analyzer/Analyzer.csproj', { evaluated: mode('Release', { published: ['plugin/analyzer/dist'] }) })]),
            })
            .graph(loaded);

        expect([Object.keys(graph.nodes).toSorted(), graph.dependencies['@fixture/plugin'] ?? [], graph.nodes['@fixture/root']?.data.targets?.['upgrade--dotnet']?.dependsOn]).toStrictEqual([['@fixture/plugin', '@fixture/root', 'Analyzer'], [], undefined]);
    });

    it.for([
        { edited: `${PLUGIN}/analyzer/dist/MsbuildAnalyzer.dll`, text: 'rebuilt', changed: true },
        { edited: `${PLUGIN}/analyzer/Program.cs`, text: 'return 1;\n', changed: false },
    ])('hashes the analyzer mise publishes into the package pack and the plugin suite, so editing $edited reruns both ($changed)', async ({ edited, text, changed }, { workspace }) => {
        const tasks = [
            ['@rasm/nx-dotnet', 'pack--pnpm'],
            ['@rasm/test-nx-dotnet', 'test--vitest'],
        ] as const;
        const before = await workspace
            .write({
                '.gitignore': 'dist/\n',
                'package.json': JSON.stringify({ name: '@fixture/root', workspaces: [PLUGIN, SUITE] }),
                ...stated(PLUGIN, 'pack--pnpm'),
                ...stated(SUITE, 'test--vitest'),
                [`${PLUGIN}/analyzer/MsbuildAnalyzer.csproj`]: PROJECT,
                [`${PLUGIN}/analyzer/Program.cs`]: 'return 0;\n',
                [`${PLUGIN}/analyzer/dist/MsbuildAnalyzer.dll`]: 'built',
                ...report([node(`${PLUGIN}/analyzer/MsbuildAnalyzer.csproj`)]),
            })
            .graph({ createNodes, createDependencies });
        const first = await Promise.all(tasks.map(([project, target]) => hasher(workspace.root, before, target, project)({})));

        const after = await workspace.write({ [edited]: text }).graph({ createNodes, createDependencies });

        await expect(Promise.all(tasks.map(async ([project, target], index) => (await hasher(workspace.root, after, target, project)({})) !== first[index]))).resolves.toStrictEqual([changed, changed]);
    });
});
