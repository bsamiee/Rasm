import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { describe, expect, vi } from 'vitest';
import { it, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('project references', () => {
    it('records a reference the project file states as static on that file, and one an imported file states as implicit', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'Directory.Build.props': '<Project />\n',
                'apps/App/App.csproj': PROJECT,
                'libs/Core/Core.csproj': PROJECT,
                'libs/Shared/Shared.csproj': PROJECT,
                ...report([
                    node('apps/App/App.csproj', {
                        references: [
                            { root: 'libs/Core', declared: 'apps/App/App.csproj' },
                            { root: 'libs/Shared', declared: 'Directory.Build.props' },
                        ],
                    }),
                    node('libs/Core/Core.csproj'),
                    node('libs/Shared/Shared.csproj'),
                ]),
            })
            .graph({ createNodes, createDependencies });

        expect(graph.dependencies['App']?.toSorted((left, right) => left.target.localeCompare(right.target))).toStrictEqual([
            { source: 'App', target: 'Core', type: 'static' },
            { source: 'App', target: 'Shared', type: 'implicit' },
        ]);
    });
});

describe('packages', () => {
    it('adds each locked package as an external node, linked from the project it states directly and from the package depending on it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'apps/App/App.csproj': PROJECT,
                ...report([
                    node('apps/App/App.csproj', {
                        packages: [
                            { id: 'Serilog.Sinks.File', version: '7.0.0', hash: 'sha512-file', direct: true, dependencies: ['Serilog'] },
                            { id: 'Serilog', version: '4.3.0', hash: 'sha512-serilog', direct: false, dependencies: [] },
                        ],
                    }),
                ]),
            })
            .graph({ createNodes, createDependencies });

        expect([graph.externalNodes?.['nuget:Serilog@4.3.0'], graph.dependencies['App'], graph.dependencies['nuget:Serilog.Sinks.File@7.0.0']]).toStrictEqual([
            { type: 'nuget', name: 'nuget:Serilog@4.3.0', data: { version: '4.3.0', packageName: 'Serilog', hash: 'sha512-serilog' } },
            [{ source: 'App', target: 'nuget:Serilog.Sinks.File@7.0.0', type: 'implicit' }],
            [{ source: 'nuget:Serilog.Sinks.File@7.0.0', target: 'nuget:Serilog@4.3.0', type: 'static' }],
        ]);
    });
});
