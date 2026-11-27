import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { FsTree, flushChanges } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import DotnetVersionActions from '@rasm/nx-dotnet/release/version-actions';
import type { ReleaseGroupWithName } from 'nx/src/command-line/release/config/filter-release-groups';
import type { FinalConfigForProject } from 'nx/src/command-line/release/utils/release-graph';
import { describe, expect, vi } from 'vitest';
import { it } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const GROUP: ReleaseGroupWithName = { name: 'dotnet', projects: ['Lib'], projectsRelationship: 'independent', resolvedVersionPlans: false };
const TRACKED = '<ItemGroup>\n    <AdditionalFiles Include="AnalyzerReleases.Shipped.md;AnalyzerReleases.Unshipped.md" />\n  </ItemGroup>';
const HEADER = '; Unshipped analyzer release\n; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md\n';
const UNSHIPPED = `${HEADER}\n### New Rules\n\nRule ID | Category | Severity | Notes\n--------|----------|----------|-------\n`;
const FILES = {
    'libs/Lib/Lib.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <Version>1.2.0</Version>\n  </PropertyGroup>\n  ${TRACKED}\n  <ItemGroup>\n    <ProjectReference Include="../Rules/Rules.csproj" />\n  </ItemGroup>\n</Project>\n`,
    'libs/Lib/AnalyzerReleases.Shipped.md': '; Shipped analyzer releases\n',
    'libs/Lib/AnalyzerReleases.Unshipped.md': `${UNSHIPPED}LIB0001 | Usage | Warning | Lib rule\n`,
    'libs/Rules/Rules.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  ${TRACKED}\n</Project>\n`,
    'libs/Rules/AnalyzerReleases.Shipped.md': '; Shipped analyzer releases\n',
    'libs/Rules/AnalyzerReleases.Unshipped.md': `${UNSHIPPED}RUL0001 | Design | Error | Rules rule\n`,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const actions = async (root: string, graph: ProjectGraph) => {
    const node = graph.nodes['Lib'];
    const settled: FinalConfigForProject = {
        specifierSource: 'prompt',
        currentVersionResolver: 'disk',
        currentVersionResolverMetadata: {},
        fallbackCurrentVersionResolver: 'disk',
        versionPrefix: 'auto',
        preserveLocalDependencyProtocols: false,
        preserveMatchingDependencyRanges: false,
        adjustSemverBumpsForZeroMajorVersion: false,
        applyPreidToDependents: false,
        versionActionsOptions: node?.data.release?.version?.versionActionsOptions ?? {},
        manifestRootsToUpdate: [],
        dockerOptions: {},
    };
    const versioned = new DotnetVersionActions(GROUP, node ?? { name: 'Lib', type: 'lib', data: { root: 'libs/Lib' } }, settled);
    const tree = new FsTree(root, false);
    await versioned.init(tree);
    return { versioned, tree };
};

const evaluated = (root: string) => execFileSync('dotnet', ['msbuild', 'libs/Lib/Lib.csproj', '-getProperty:Version'], { cwd: root, encoding: 'utf-8' }).trim();

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('release of a packable project stating its Version', () => {
    it('writes the version MSBuild then evaluates, and moves the unshipped rules of the project and its unreleased reference under the release', async ({ workspace }) => {
        const graph = await workspace.write(FILES).graph({ createNodes, createDependencies });
        const { versioned, tree } = await actions(workspace.root, graph);

        await versioned.updateProjectVersion(tree, '1.3.0');
        flushChanges(workspace.root, tree.listChanges());

        const read = (file: string) => readFileSync(join(workspace.root, file), 'utf-8');
        expect({
            tags: graph.nodes['Lib']?.data.tags,
            version: evaluated(workspace.root),
            shipped: [read('libs/Lib/AnalyzerReleases.Shipped.md'), read('libs/Rules/AnalyzerReleases.Shipped.md')],
            unshipped: [read('libs/Lib/AnalyzerReleases.Unshipped.md'), read('libs/Rules/AnalyzerReleases.Unshipped.md')],
        }).toStrictEqual({
            tags: ['release'],
            version: '1.3.0',
            shipped: [
                '; Shipped analyzer releases\n\n## Release 1.3.0\n\n### New Rules\n\nRule ID | Category | Severity | Notes\n--------|----------|----------|-------\nLIB0001 | Usage | Warning | Lib rule\n',
                '; Shipped analyzer releases\n\n## Release 1.3.0\n\n### New Rules\n\nRule ID | Category | Severity | Notes\n--------|----------|----------|-------\nRUL0001 | Design | Error | Rules rule\n',
            ],
            unshipped: [HEADER, HEADER],
        });
    });

    it('writes a prerelease version and leaves the unshipped rules in place', async ({ workspace }) => {
        const graph = await workspace.write(FILES).graph({ createNodes, createDependencies });
        const { versioned, tree } = await actions(workspace.root, graph);

        await versioned.updateProjectVersion(tree, '1.3.0-alpha.1');
        flushChanges(workspace.root, tree.listChanges());

        const read = (file: string) => readFileSync(join(workspace.root, file), 'utf-8');
        expect([evaluated(workspace.root), read('libs/Lib/AnalyzerReleases.Shipped.md'), read('libs/Lib/AnalyzerReleases.Unshipped.md')]).toStrictEqual(['1.3.0-alpha.1', FILES['libs/Lib/AnalyzerReleases.Shipped.md'], FILES['libs/Lib/AnalyzerReleases.Unshipped.md']]);
    });

    it('bumps a prerelease version by its prerelease number where a dependency was bumped', async ({ workspace }) => {
        const graph = await workspace.write(FILES).graph({ createNodes, createDependencies });
        const { versioned } = await actions(workspace.root, graph);

        const { newVersion } = await versioned.calculateNewVersion('1.3.0-alpha.1', 'patch', 'DEPENDENCY_WAS_BUMPED', {}, 'alpha');

        expect(newVersion).toBe('1.3.0-alpha.2');
    });
});
