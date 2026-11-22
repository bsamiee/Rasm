import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Build } from '@rasm/nx-dotnet/executors/build';
import { outputs, unaggregated, unportable } from '@rasm/testing/nx';
import { Array, flow, Option, Order, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it, mode, node, report } from '#/analyzer.ts';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [MODELS] --------------------------------------------------------------------------

const Cleaned = Schema.Struct({ targets: Schema.Array(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = flow(Schema.decodeUnknownOption(Build), Option.getOrThrow);

const cleaned = flow(Schema.decodeUnknownOption(Cleaned), Option.getOrThrow);

const optional = (graph: ProjectGraph, project: string) => Array.intersection(Record.keys({ ...graph.nodes[project]?.data.targets }), ['pack--dotnet', 'attach--gh', 'nx-release-publish', 'test--dotnet']);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('build--dotnet', () => {
    it('resolves its outputs to the folders, documents, and reports the selected configuration writes', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/Api/Api.csproj': PROJECT,
                ...report([
                    node('apps/Api/Api.csproj', {
                        evaluated: mode('Debug', { outputs: ['apps/Api/bin/Debug/net10.0', 'apps/Api/obj/Debug/net10.0'] }),
                        declared: [mode('Release', { outputs: ['apps/Api/bin/Release/net10.0', 'apps/Api/obj/Release/net10.0'], published: ['apps/Api/bin/Release/net10.0/publish'], reports: ['apps/Api/obj/Release/net10.0/Api.sarif'], documents: ['apps/Api/openapi/Api.json'] })],
                    }),
                ]),
            })
            .graph({ createNodes, createDependencies });

        expect([outputs(graph, 'Api', 'build--dotnet'), outputs(graph, 'Api', 'build--dotnet', 'release')]).toStrictEqual([
            ['apps/Api/bin/Debug/net10.0', 'apps/Api/obj/Debug/net10.0'],
            ['apps/Api/bin/Release/net10.0', 'apps/Api/obj/Release/net10.0', 'apps/Api/bin/Release/net10.0/publish', 'apps/Api/obj/Release/net10.0/Api.sarif', 'apps/Api/openapi/Api.json'],
        ]);
    });

    it('runs dotnet publish for a publishable project and dotnet build for any other, in the evaluated configuration', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/Tool/Tool.csproj': PROJECT, 'libs/Core/Core.csproj': PROJECT, ...report([node('apps/Tool/Tool.csproj', { publishable: true, evaluated: mode('Staging') }), node('libs/Core/Core.csproj')]) }).graph({ createNodes, createDependencies });

        expect([built(graph.nodes['Tool']?.data.targets?.['build--dotnet']?.options).command, built(graph.nodes['Core']?.data.targets?.['build--dotnet']?.options).command]).toStrictEqual([
            ['publish', '--configuration', 'Staging'],
            ['build', '--configuration', 'Debug'],
        ]);
    });

    it('maps each project the build reaches to its own default where it lacks the selected configuration, and maps nothing where all declare it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/App/App.csproj': PROJECT,
                'apps/Kit/Kit.csproj': PROJECT,
                'libs/Core/Core.csproj': PROJECT,
                ...report([
                    node('apps/App/App.csproj', { declared: [mode('Interface')], references: [{ root: 'libs/Core', declared: 'apps/App/App.csproj' }] }),
                    node('apps/Kit/Kit.csproj', { declared: [mode('Release')], references: [{ root: 'libs/Core', declared: 'apps/Kit/Kit.csproj' }] }),
                    node('libs/Core/Core.csproj', { platform: 'x64', declared: [mode('Release')] }),
                ]),
            })
            .graph({ createNodes, createDependencies });

        expect([built(graph.nodes['App']?.data.targets?.['build--dotnet']?.configurations?.['interface']).solution, built(graph.nodes['Kit']?.data.targets?.['build--dotnet']?.configurations?.['release']).solution, built(graph.nodes['App']?.data.targets?.['build--dotnet']?.options).solution]).toStrictEqual([
            { 'apps/App/App.csproj': 'Interface|AnyCPU', 'libs/Core/Core.csproj': 'Debug|x64' },
            undefined,
            undefined,
        ]);
    });
});

describe('optional targets', () => {
    it.for([
        { kind: 'a library', stated: {}, targets: [] },
        { kind: 'a packable project with no default push source', stated: { packable: true }, targets: ['pack--dotnet', 'attach--gh'] },
        { kind: 'a packable project with a default push source', stated: { packable: true, pushed: true }, targets: ['pack--dotnet', 'attach--gh', 'nx-release-publish'] },
        { kind: 'a project with a default push source it cannot pack for', stated: { pushed: true }, targets: [] },
        { kind: 'a test project', stated: { test: true }, targets: ['test--dotnet'] },
    ])('gives $kind exactly the targets its kind runs', async ({ stated, targets }, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', stated)]) }).graph({ createNodes, createDependencies });

        expect(Array.sort(optional(graph, 'Lib'), Order.String)).toStrictEqual(Array.sort(targets, Order.String));
    });

    it('resolves pack--dotnet to the packages in the package folder of the selected configuration', async ({ workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, evaluated: mode('Debug', { packed: 'libs/Lib/bin/Debug' }), declared: [mode('Release', { packed: 'artifacts/package/release' })] })]) }).graph({ createNodes, createDependencies });

        expect([outputs(graph, 'Lib', 'pack--dotnet'), outputs(graph, 'Lib', 'pack--dotnet', 'release')]).toStrictEqual([['libs/Lib/bin/Debug/*.nupkg'], ['artifacts/package/release/*.nupkg']]);
    });

    it('cleans the outputs of every target of the project that declares outputs', async ({ workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', { packable: true, test: true, evaluated: mode('Debug', { outputs: ['libs/Lib/bin/Debug'], packed: 'libs/Lib/bin/Debug' }) })]) }).graph({ createNodes, createDependencies });

        expect(Array.sort(cleaned(graph.nodes['Lib']?.data.targets?.['clean--dotnet']?.options).targets, Order.String)).toStrictEqual(['build--dotnet', 'pack--dotnet', 'test--dotnet']);
    });
});

describe('graph laws', () => {
    it('runs every tool target through its verb and states no machine path', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'Workspace.slnx': '<Solution />\n',
                'Directory.Build.props': '<Project />\n',
                'apps/Api/Api.csproj': PROJECT,
                'apps/Api.Tests/Api.Tests.csproj': PROJECT,
                ...report([
                    node('apps/Api/Api.csproj', {
                        packable: true,
                        pushed: true,
                        publishable: true,
                        versioned: true,
                        inputs: ['Directory.Build.props'],
                        bases: ['apps/Api/bin', 'apps/Api/obj'],
                        evaluated: mode('Debug', { outputs: ['apps/Api/bin/Debug'], packed: 'apps/Api/bin/Debug' }),
                        declared: [mode('Release', { outputs: ['apps/Api/bin/Release'] })],
                    }),
                    node('apps/Api.Tests/Api.Tests.csproj', { test: true, referenced: ['coverlet.MTP'], results: ['apps/Api.Tests/TestResults'], references: [{ root: 'apps/Api', declared: 'apps/Api.Tests/Api.Tests.csproj' }] }),
                ]),
            })
            .graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });

        expect([unaggregated(graph), unportable(graph), JSON.stringify(graph).includes(workspace.root)]).toStrictEqual([[], [], false]);
    });
});
