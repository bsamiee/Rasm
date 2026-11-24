import { execFileSync } from 'node:child_process';
import { posix, relative } from 'node:path';
import { platform } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Build } from '@rasm/nx-dotnet/executors/build';
import { it as base, outputs, unaggregated, unportable } from '@rasm/testing/nx';
import { Option, Record, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECTS = ['libs/Core/Core.csproj', 'apps/Tool/Tool.csproj', 'tests/Core.Tests/Core.Tests.csproj', 'tests/Win.Tests/Win.Tests.csproj'] as const;
const FILES = {
    'Directory.Build.props': '<Project>\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>\n  </PropertyGroup>\n</Project>\n',
    'Workspace.slnx': '<Solution>\n  <Project Path="libs/Core/Core.csproj" />\n  <Project Path="apps/Tool/Tool.csproj" />\n  <Project Path="tests/Core.Tests/Core.Tests.csproj" />\n</Solution>\n',
    'libs/Core/Core.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <PackageId>Fixture.Core</PackageId>\n    <Version>1.2.0</Version>\n  </PropertyGroup>\n</Project>\n',
    'apps/Tool/Tool.csproj':
        '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <IsPackable>false</IsPackable>\n    <IsPublishable>true</IsPublishable>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n',
    'tests/Core.Tests/Core.Tests.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <IsTestProject>true</IsTestProject>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include="../../libs/Core/Core.csproj" />\n  </ItemGroup>\n</Project>\n',
    'tests/Win.Tests/Win.Tests.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0-windows</TargetFramework>\n    <IsPublishable>false</IsPublishable>\n    <IsTestProject>true</IsTestProject>\n    <IsPackable>false</IsPackable>\n  </PropertyGroup>\n</Project>\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Evaluated = Schema.fromJsonString(
    Schema.Struct({
        Properties: Schema.Struct({ MSBuildProjectName: Schema.String, PackageId: Schema.String, IsPackable: Schema.String, IsPublishable: Schema.String, IsTestProject: Schema.String, TargetPlatformIdentifier: Schema.String, OutputPath: Schema.String, PublishDir: Schema.String }),
        Items: Schema.Struct({ ProjectReference: Schema.optionalKey(Schema.Array(Schema.Struct({ FullPath: Schema.String }))) }),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluated = (root: string, project: string, configuration: string) =>
    Option.getOrThrow(
        Schema.decodeOption(Evaluated)(execFileSync('dotnet', ['msbuild', project, `-p:Configuration=${configuration}`, '-getProperty:MSBuildProjectName,PackageId,IsPackable,IsPublishable,IsTestProject,TargetPlatformIdentifier,OutputPath,PublishDir', '-getItem:ProjectReference'], { cwd: root, encoding: 'utf-8' })),
    );

const roles = (root: string, graph: ProjectGraph) =>
    PROJECTS.map((project) => {
        const { Properties: stated, Items: items } = evaluated(root, project, 'Debug');
        const name = stated.IsPackable === 'true' ? stated.PackageId : stated.MSBuildProjectName;
        const targets = graph.nodes[name]?.data.targets ?? {};
        return {
            expected: {
                name,
                pack: stated.IsPackable === 'true',
                test: stated.IsTestProject === 'true' && (stated.TargetPlatformIdentifier !== 'Windows' || platform === 'win32'),
                verb: stated.IsPublishable === 'true' ? 'publish' : 'build',
                windows: stated.TargetPlatformIdentifier === 'Windows',
                references: new Set((items.ProjectReference ?? []).map(({ FullPath }) => posix.dirname(relative(root, FullPath)))),
            },
            inferred: {
                name: graph.nodes[name]?.data.name,
                pack: 'pack--dotnet' in targets,
                test: 'test--dotnet' in targets,
                verb: Option.getOrUndefined(Option.map(Schema.decodeUnknownOption(Build)(targets['build--dotnet']?.options), ({ command: [verb] }) => verb)),
                windows: (graph.nodes[name]?.data.tags ?? []).includes('os:win32'),
                references: new Set((graph.dependencies[name] ?? []).flatMap(({ target }) => [graph.nodes[target]?.data.root ?? target])),
            },
        };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = base.extend('analyzed', { scope: 'file' }, async ({ fileWorkspace }) => ({ root: fileWorkspace.root, graph: await fileWorkspace.write(FILES).graph({ createNodes, createDependencies }) }));

describe('MsbuildAnalyzer evaluation of a workspace', { timeout: 300_000 }, () => {
    it('gives each project the name, targets, tags, and edges MSBuild evaluates it to', ({ analyzed: { root, graph } }) => {
        const compared = roles(root, graph);

        expect(compared.map(({ inferred }) => inferred)).toStrictEqual(compared.map(({ expected }) => expected));
    });

    it('declares as build--dotnet outputs the output and publish folders MSBuild evaluates for each configuration, leaving the intermediate folder to MSBuild as machine state', ({ analyzed: { root, graph } }) => {
        const folders = (configuration: string) => {
            const { Properties: stated } = evaluated(root, 'apps/Tool/Tool.csproj', configuration);
            return [stated.OutputPath, stated.PublishDir].map((folder) => posix.join('apps/Tool', folder.replaceAll('\\', '/')).replace(/\/$/u, ''));
        };

        expect([new Set(outputs(graph, 'Tool', 'build--dotnet')), new Set(outputs(graph, 'Tool', 'build--dotnet', 'release'))]).toStrictEqual([new Set(folders('Debug')), new Set(folders('Release'))]);
    });

    it('runs every tool target through its verb and states no machine path', ({ analyzed: { graph, root } }) => {
        expect([unaggregated(graph), unportable(graph), JSON.stringify(graph).includes(root)]).toStrictEqual([[], [], false]);
    });
});

describe('a change to an imported file', { timeout: 300_000 }, () => {
    it('gives each project the roles MSBuild evaluates after the change', async ({ workspace }) => {
        const before = roles(workspace.root, await workspace.write(FILES).graph({ createNodes, createDependencies }));

        const graph = await workspace.write({ 'Directory.Build.props': FILES['Directory.Build.props'].replace('</PropertyGroup>', '  <IsTestProject>true</IsTestProject>\n  </PropertyGroup>') }).graph({ createNodes, createDependencies });

        const compared = roles(workspace.root, graph);
        const tested = (held: typeof compared) => held.filter(({ expected }) => expected.test).length;
        expect([compared.map(({ inferred }) => inferred), tested(compared) > tested(before)]).toStrictEqual([compared.map(({ expected }) => expected), true]);
    });
});

describe('solution folders', { timeout: 300_000 }, () => {
    it.for([
        { beside: 'the root package.json', files: { 'Workspace.slnx': '<Solution />\n' }, named: '@fixture/root', root: '.' },
        { beside: 'no package.json', files: { 'tools/Tools.sln': '\n' }, named: 'Tools', root: 'tools' },
    ])('gives a solution folder with $beside upgrade--dotnet, named as Nx names the project at its root', async ({ files, named, root }, { workspace }) => {
        const graph = await workspace.write({ 'package.json': JSON.stringify({ name: '@fixture/root', workspaces: ['tools'] }), ...files }).graph({ createNodes, createDependencies });

        expect(
            Record.map(
                Record.filter(graph.nodes, (node) => 'upgrade--dotnet' in (node.data.targets ?? {})),
                ({ data }) => data.root,
            ),
        ).toStrictEqual({ [named]: root });
    });
});
