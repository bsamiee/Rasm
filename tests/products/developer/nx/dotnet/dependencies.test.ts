import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (references: readonly string[]) => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n${references.map((reference) => `    <ProjectReference Include="${reference}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

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
