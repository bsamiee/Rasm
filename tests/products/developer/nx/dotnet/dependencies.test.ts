import { env } from 'node:process';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { it, taskGraph } from '@rasm/testing/nx';
import { Option } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (references: readonly string[]) => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n${references.map((reference) => `    <ProjectReference Include="${reference}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

const locked = (version: string, contentHash: string) => JSON.stringify({ version: 2, dependencies: { 'net10.0': { Serilog: { type: 'Direct', requested: `[${version}, )`, resolved: version, contentHash } } } });

const catalog = (rows: readonly (readonly [id: string, version: string])[]) => `<Project>\n  <ItemGroup>\n${rows.map(([id, version]) => `    <PackageVersion Include="${id}" Version="${version}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

const hashed = async (root: string, graph: ProjectGraph, id: string) => {
    const [owner = '', target = ''] = id.split(':');
    const tasks = taskGraph(graph, [owner], [target]);
    const task = Option.getOrThrow(Option.fromUndefinedOr(tasks.tasks[id]));
    const { value } = await createTaskHasher(graph, readNxJson(root)).hashTask(task, tasks, env);
    return value;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('project references', { timeout: 180_000 }, () => {
    it('records a reference the project file states as static on that file, and one an import states as implicit', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'Directory.Build.props': '<Project>\n  <ItemGroup Condition="\'$(MSBuildProjectName)\' == \'App\'">\n    <ProjectReference Include="$(MSBuildThisFileDirectory)libs/Shared/Shared.csproj" />\n  </ItemGroup>\n</Project>\n',
                'apps/App/App.csproj': project(['../../libs/Core/Core.csproj']),
                'libs/Core/Core.csproj': project([]),
                'libs/Shared/Shared.csproj': project([]),
            })
            .graph({ createNodes, createDependencies });

        expect(new Set(graph.dependencies['App'])).toStrictEqual(
            new Set([
                { source: 'App', target: 'Core', type: 'static' },
                { source: 'App', target: 'Shared', type: 'implicit' },
            ]),
        );
    });

    it('infers both projects of a ProjectReference cycle with an edge each way', async ({ workspace }) => {
        const graph = await workspace.write({ 'a/a.csproj': project(['../b/b.csproj']), 'b/b.csproj': project(['../a/a.csproj']) }).graph({ createNodes, createDependencies });

        expect([graph.dependencies['a'], graph.dependencies['b']]).toStrictEqual([[{ source: 'a', target: 'b', type: 'static' }], [{ source: 'b', target: 'a', type: 'static' }]]);
    });
});

describe('locked packages', { timeout: 180_000 }, () => {
    it('adds each package of the lock file as an external node, linked from the project for a direct one and from each package naming it as a dependency', async ({ workspace }) => {
        const lock = {
            version: 2,
            dependencies: {
                'net10.0': {
                    'Serilog.Sinks.File': { type: 'Direct', requested: '[7.0.0, )', resolved: '7.0.0', contentHash: 'c2luaw==', dependencies: { Serilog: '4.3.0' } },
                    'Serilog.Sinks.Console': { type: 'Direct', requested: '[6.1.1, )', resolved: '6.1.1', contentHash: 'Y29uc29sZQ==', dependencies: { Serilog: '4.3.0', 'Serilog.Expressions': '5.0.0' } },
                    'Serilog.Expressions': { type: 'Transitive', resolved: '5.0.0', contentHash: 'ZXhwcmVzc2lvbnM=' },
                    Serilog: { type: 'Transitive', resolved: '4.3.0', contentHash: 'c2VyaWxvZw==' },
                },
            },
        };
        const graph = await workspace.write({ 'apps/App/App.csproj': project([]), 'apps/App/packages.lock.json': JSON.stringify(lock) }).graph({ createNodes, createDependencies });

        expect({
            serilog: graph.externalNodes?.['nuget:Serilog@4.3.0'],
            app: new Set(graph.dependencies['App']),
            file: graph.dependencies['nuget:Serilog.Sinks.File@7.0.0'],
            console: new Set(graph.dependencies['nuget:Serilog.Sinks.Console@6.1.1']),
            leaf: graph.dependencies['nuget:Serilog@4.3.0'] ?? [],
        }).toStrictEqual({
            serilog: { type: 'nuget', name: 'nuget:Serilog@4.3.0', data: { version: '4.3.0', packageName: 'Serilog', hash: 'c2VyaWxvZw==' } },
            app: new Set([
                { source: 'App', target: 'nuget:Serilog.Sinks.File@7.0.0', type: 'implicit' },
                { source: 'App', target: 'nuget:Serilog.Sinks.Console@6.1.1', type: 'implicit' },
            ]),
            file: [{ source: 'nuget:Serilog.Sinks.File@7.0.0', target: 'nuget:Serilog@4.3.0', type: 'static' }],
            console: new Set([
                { source: 'nuget:Serilog.Sinks.Console@6.1.1', target: 'nuget:Serilog@4.3.0', type: 'static' },
                { source: 'nuget:Serilog.Sinks.Console@6.1.1', target: 'nuget:Serilog.Expressions@5.0.0', type: 'static' },
            ]),
            leaf: [],
        });
    });
});

describe('central package versions', { timeout: 180_000 }, () => {
    it.for([
        {
            edited: {
                'Directory.Packages.props': catalog([
                    ['Serilog', '4.3.0'],
                    ['Unused.Package', '1.0.0'],
                ]),
            },
            build: false,
            restore: true,
        },
        { edited: { 'apps/App/packages.lock.json': locked('4.3.1', 'bmV4dA==') }, build: true, restore: true },
    ])('reaches a version change through the lock file alone and the central file through the locked restore check, so editing $edited reruns the build ($build) and the check ($restore)', async ({ edited, build, restore }, { workspace }) => {
        const before = await workspace
            .write({
                'Directory.Build.props': '<Project>\n  <PropertyGroup>\n    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n  </PropertyGroup>\n</Project>\n',
                'Directory.Packages.props': catalog([['Serilog', '4.3.0']]),
                'Workspace.slnx': '<Solution>\n  <Project Path="apps/App/App.csproj" />\n</Solution>\n',
                'apps/App/App.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include="Serilog" />\n  </ItemGroup>\n</Project>\n',
                'apps/App/packages.lock.json': locked('4.3.0', 'c2VyaWxvZw=='),
            })
            .graph({ createNodes, createDependencies });
        const [built, checked] = [await hashed(workspace.root, before, 'App:build--dotnet'), await hashed(workspace.root, before, 'Workspace:check--dotnet-restore')];

        const after = await workspace.write(edited).graph({ createNodes, createDependencies });

        expect([built !== (await hashed(workspace.root, after, 'App:build--dotnet')), checked !== (await hashed(workspace.root, after, 'Workspace:check--dotnet-restore'))]).toStrictEqual([build, restore]);
    });
});
