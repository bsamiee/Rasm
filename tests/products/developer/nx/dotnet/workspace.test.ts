import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Build } from '@rasm/nx-dotnet/executors/build';
import { it as nx, outputs, unaggregated, unportable } from '@rasm/testing/nx';
import { Array, flow, Option, Predicate, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = {
    'Directory.Build.props': '<Project>\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n</Project>\n',
    'nuget.config': '<configuration>\n  <config>\n    <add key="defaultPushSource" value="https://api.nuget.org/v3/index.json" />\n  </config>\n</configuration>\n',
    'Workspace.slnx': '<Solution>\n  <Project Path="libs/Core/Core.csproj" />\n  <Project Path="apps/Tool/Tool.csproj" />\n  <Project Path="tests/Core.Tests/Core.Tests.csproj" />\n</Solution>\n',
    'libs/Core/Core.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <Version>1.2.0</Version>\n  </PropertyGroup>\n</Project>\n',
    'apps/Tool/Tool.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n',
    'tests/Core.Tests/Core.Tests.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <IsTestProject>true</IsTestProject>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = flow(Schema.decodeUnknownOption(Build), Option.getOrThrow);

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('analyzed', { scope: 'file' }, async ({ fileWorkspace }) => ({ root: fileWorkspace.root, graph: await fileWorkspace.write(FILES).graph({ createNodes, createDependencies }) }));

describe('MsbuildAnalyzer evaluation of a workspace', { timeout: 120_000 }, () => {
    it('infers a project per project file, named by its package where it packs, and one per solution folder', ({ analyzed: { graph } }) => {
        expect(Record.collect(graph.nodes, (name, { data: { root } }) => `${name}@${root}`).toSorted()).toStrictEqual(['Core.Tests@tests/Core.Tests', 'Core@libs/Core', 'Tool@apps/Tool', 'Workspace@.']);
    });

    it('gives each project the targets of what MSBuild evaluates it to pack, publish, release, and test', ({ analyzed: { graph } }) => {
        const held = (project: string) => Array.intersection(Record.keys({ ...graph.nodes[project]?.data.targets }), ['pack--dotnet', 'nx-release-publish', 'test--dotnet', 'upgrade--dotnet']);

        expect([held('Core'), graph.nodes['Core']?.data.tags, held('Tool'), built(graph.nodes['Tool']?.data.targets?.['build--dotnet']?.options).command[0], held('Core.Tests'), held('Workspace')]).toStrictEqual([['pack--dotnet', 'nx-release-publish'], ['release'], [], 'publish', ['test--dotnet'], ['upgrade--dotnet']]);
    });

    it('resolves build outputs to the SDK default output, intermediate, and publish folders of each configuration', ({ analyzed: { graph } }) => {
        expect([outputs(graph, 'Core', 'build--dotnet'), outputs(graph, 'Core', 'build--dotnet', 'release')]).toStrictEqual([
            ['libs/Core/bin/Debug/net10.0', 'libs/Core/obj/Debug/net10.0', 'libs/Core/bin/Debug/net10.0/publish'],
            ['libs/Core/bin/Release/net10.0', 'libs/Core/obj/Release/net10.0', 'libs/Core/bin/Release/net10.0/publish'],
        ]);
    });

    it('hashes the imported and NuGet configuration files of the workspace in build--dotnet', ({ analyzed: { graph } }) => {
        expect(graph.nodes['Tool']?.data.targets?.['build--dotnet']?.inputs?.filter(Predicate.isString)).toStrictEqual(['default', '^default', '!{projectRoot}/bin/**', '!{projectRoot}/obj/**', '{workspaceRoot}/Directory.Build.props', '{workspaceRoot}/nuget.config']);
    });

    it('records each ProjectReference as a static edge from the project file stating it', ({ analyzed: { graph } }) => {
        expect([graph.dependencies['Tool'], graph.dependencies['Core.Tests']]).toStrictEqual([[{ source: 'Tool', target: 'Core', type: 'static' }], [{ source: 'Core.Tests', target: 'Core', type: 'static' }]]);
    });

    it('runs every tool target through its verb and states no machine path', ({ analyzed: { graph, root } }) => {
        expect([unaggregated(graph), unportable(graph), JSON.stringify(graph).includes(root)]).toStrictEqual([[], [], false]);
    });
});
