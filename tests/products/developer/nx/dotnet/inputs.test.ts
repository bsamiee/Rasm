import { writeFileSync } from 'node:fs';
import { delimiter, join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { environment, taskGraph } from '@rasm/testing/nx';
import { Option } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';
import { it, mode, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';
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
