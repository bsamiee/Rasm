import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { unportable } from '@rasm/testing/nx';
import { expect, vi } from 'vitest';
import { it, mode, node, report } from '#/analyzer';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

it('states posix roots, inputs, outputs, and edges for nested and parent-relative projects under Windows path separators', async ({ workspace }) => {
    const graph = await workspace
        .write({
            'Workspace.slnx': '<Solution />\n',
            'build/Shared.props': '<Project />\n',
            'src/apps/App/App.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n',
            'src/apps/App.Tests/App.Tests.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n',
            'libs/Core/Core.csproj': '<Project Sdk="Microsoft.NET.Sdk" />\n',
            ...report([
                node('src/apps/App/App.csproj', {
                    packable: true,
                    inputs: ['build/Shared.props'],
                    bases: ['src/apps/App/bin', 'src/apps/App/obj'],
                    references: [{ root: 'libs/Core', declared: 'src/apps/App/App.csproj' }],
                    evaluated: mode('Debug', { outputs: ['src/apps/App/bin/Debug', 'artifacts/obj/App'], packed: 'artifacts/package/debug' }),
                }),
                node('src/apps/App.Tests/App.Tests.csproj', { test: true, referenced: ['coverlet.MTP'], results: ['src/apps/App.Tests/TestResults', 'artifacts/test/App.Tests'], references: [{ root: 'src/apps/App', declared: 'src/apps/App.Tests/App.Tests.csproj' }] }),
                node('libs/Core/Core.csproj', { releases: { shipped: 'libs/Core/AnalyzerReleases.Shipped.md', unshipped: 'libs/Core/AnalyzerReleases.Unshipped.md' } }),
            ]),
        })
        .graph({ createNodes, createDependencies }, { coveragePath: 'coverage/{projectName}' });

    expect([unportable(graph), graph.dependencies['App'], graph.dependencies['App.Tests']]).toStrictEqual([[], [{ source: 'App', target: 'Core', type: 'static' }], [{ source: 'App.Tests', target: 'App', type: 'static' }]]);
});
