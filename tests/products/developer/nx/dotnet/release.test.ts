import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { describe, expect, vi } from 'vitest';
import { it, node, report } from '#/analyzer.ts';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('release projects', () => {
    it.for([
        { kind: 'a packable project stating its Version', stated: { versioned: true, packable: true }, tags: ['release'], released: true },
        { kind: 'a publishable project stating its Version', stated: { versioned: true, publishable: true }, tags: ['release'], released: true },
        { kind: 'a packable project with no Version of its own', stated: { packable: true }, tags: [], released: false },
        { kind: 'a project stating its Version it neither packs nor publishes', stated: { versioned: true }, tags: [], released: false },
    ])('tags $kind and gives it version actions by whether nx release can version a package of it', async ({ stated, tags, released }, { workspace }) => {
        const graph = await workspace.write({ 'libs/Lib/Lib.csproj': PROJECT, ...report([node('libs/Lib/Lib.csproj', stated)]) }).graph({ createNodes, createDependencies });

        expect([graph.nodes['Lib']?.data.root, graph.nodes['Lib']?.data.tags ?? [], graph.nodes['Lib']?.data.release !== undefined]).toStrictEqual(['libs/Lib', tags, released]);
    });

    it('versions the project file and moves the analyzer release notes of the project and of each reference released with it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'libs/Lib/Lib.csproj': PROJECT,
                'libs/Rules/Rules.csproj': PROJECT,
                'libs/Other/Other.csproj': PROJECT,
                ...report([
                    node('libs/Lib/Lib.csproj', {
                        versioned: true,
                        packable: true,
                        releases: { shipped: 'libs/Lib/AnalyzerReleases.Shipped.md', unshipped: 'libs/Lib/AnalyzerReleases.Unshipped.md' },
                        references: [
                            { root: 'libs/Rules', declared: 'libs/Lib/Lib.csproj' },
                            { root: 'libs/Other', declared: 'libs/Lib/Lib.csproj' },
                        ],
                    }),
                    node('libs/Rules/Rules.csproj', { releases: { shipped: 'libs/Rules/AnalyzerReleases.Shipped.md', unshipped: 'libs/Rules/AnalyzerReleases.Unshipped.md' } }),
                    node('libs/Other/Other.csproj', { versioned: true, packable: true, releases: { shipped: 'libs/Other/AnalyzerReleases.Shipped.md', unshipped: 'libs/Other/AnalyzerReleases.Unshipped.md' } }),
                ]),
            })
            .graph({ createNodes, createDependencies });

        expect(graph.nodes['Lib']?.data.release).toStrictEqual({
            version: {
                versionActions: '@rasm/nx-dotnet/release/version-actions',
                versionActionsOptions: {
                    manifest: 'Lib.csproj',
                    releases: [
                        { shipped: 'libs/Lib/AnalyzerReleases.Shipped.md', unshipped: 'libs/Lib/AnalyzerReleases.Unshipped.md' },
                        { shipped: 'libs/Rules/AnalyzerReleases.Shipped.md', unshipped: 'libs/Rules/AnalyzerReleases.Unshipped.md' },
                    ],
                },
            },
        });
    });
});

describe('stated relations', () => {
    it('gives the project the NxTag items as tags and the NxImplicitDependency items as implicit dependencies', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/Plugin/Plugin.csproj': PROJECT, 'apps/Host/Host.csproj': PROJECT, ...report([node('apps/Plugin/Plugin.csproj', { tags: ['host:rhino'], implicit: ['Host'] }), node('apps/Host/Host.csproj')]) }).graph({ createNodes, createDependencies });

        expect([graph.nodes['Plugin']?.data.tags, graph.nodes['Plugin']?.data.implicitDependencies, graph.dependencies['Plugin']]).toStrictEqual([['host:rhino'], ['Host'], [{ source: 'Plugin', target: 'Host', type: 'implicit' }]]);
    });
});
