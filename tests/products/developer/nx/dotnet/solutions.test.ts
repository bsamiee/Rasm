import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Option, Predicate, Schema } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it, mode, node, report } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANALYZER = join(dirname(fileURLToPath(import.meta.resolve('@rasm/nx-dotnet/package.json'))), 'analyzer', 'dist', 'MsbuildAnalyzer.dll');

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ apply: Schema.Array(Schema.String), preview: Schema.optionalKey(Schema.Array(Schema.String)), env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });
const Upgrade = Schema.Struct({ cwd: Schema.String, commands: Schema.Tuple([Command, Command, Command]) });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('upgrade--dotnet', () => {
    const project = '<Project Sdk="Microsoft.NET.Sdk" />\n';

    it.for([
        { beside: 'the root package.json', files: { 'Workspace.slnx': '<Solution />\n' }, workspaces: ['apps/*'], root: '.', named: '@fixture/root' },
        { beside: 'no package.json', files: { 'tools/Tools.sln': '\n' }, workspaces: ['apps/*'], root: 'tools', named: 'Tools' },
        { beside: 'a package.json outside the workspaces naming no package', files: { 'tools/Tools.sln': '\n', 'tools/package.json': JSON.stringify({ type: 'module' }) }, workspaces: ['apps/*'], root: 'tools', named: 'Tools' },
        { beside: 'a package.json outside the workspaces naming a package', files: { 'tools/Tools.sln': '\n', 'tools/package.json': JSON.stringify({ name: '@fixture/tools' }) }, workspaces: ['apps/*'], root: 'tools', named: '@fixture/tools' },
        { beside: 'a workspace member package.json', files: { 'tools/Tools.sln': '\n', 'tools/package.json': JSON.stringify({ name: '@fixture/tools' }) }, workspaces: ['tools'], root: 'tools', named: '@fixture/tools' },
    ])('gives a solution folder with $beside the target, named as Nx names a project from the package.json at its root, else by the solution', async ({ files, workspaces, root, named }, { workspace }) => {
        const graph = await workspace.write({ 'package.json': JSON.stringify({ name: '@fixture/root', workspaces }), ...files, ...report([]) }).graph({ createNodes, createDependencies });

        expect([graph.nodes[named]?.data.root, 'upgrade--dotnet' in (graph.nodes[named]?.data.targets ?? {})]).toStrictEqual([root, true]);
    });

    it('moves versions through the analyzer under prerelease roll forward from the solution folder, then restores locks with dotnet alone', async ({ workspace }) => {
        const graph = await workspace.write({ 'tools/Tools.sln': '\n', 'apps/App/App.csproj': project, ...report([node('apps/App/App.csproj')]) }).graph({ createNodes, createDependencies });
        const launch = ['dotnet', '--roll-forward', 'LatestMajor', relative(join(workspace.root, 'tools'), ANALYZER), 'upgrade', 'Tools.sln'];

        const {
            cwd,
            commands: [{ apply, preview, env }, ...restores],
        } = Option.getOrThrow(Schema.decodeUnknownOption(Upgrade)(graph.nodes['Tools']?.data.targets?.['upgrade--dotnet']?.options));

        expect([cwd, apply, preview, env?.['DOTNET_ROLL_FORWARD_TO_PRERELEASE'], restores]).toStrictEqual(['tools', launch, [...launch, '--dry-run'], '1', [{ apply: ['dotnet', 'nuget', 'locals', 'global-packages', '--clear'] }, { apply: ['dotnet', 'restore', 'Tools.sln', '--force-evaluate'] }]]);
    });

    it('hashes the solution and every file the analyzed projects read', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'Workspace.slnx': '<Solution />\n',
                'Directory.Packages.props': '<Project />\n',
                'apps/App/App.csproj': project,
                'apps/App/packages.lock.json': '{}\n',
                ...report([node('apps/App/App.csproj', { inputs: ['Directory.Packages.props'], locks: ['apps/App/packages.lock.json'] })]),
            })
            .graph({ createNodes, createDependencies });

        expect(graph.nodes['Workspace']?.data.targets?.['upgrade--dotnet']?.inputs?.filter(Predicate.isString).toSorted()).toStrictEqual(['{workspaceRoot}/Directory.Packages.props', '{workspaceRoot}/Workspace.slnx', '{workspaceRoot}/apps/App/App.csproj', '{workspaceRoot}/apps/App/packages.lock.json']);
    });

    it.for([
        { kind: 'after the build of the project publishing the analyzer the plugin launches', publishes: true, dependsOn: [{ projects: ['Analyzer'], target: 'build--dotnet' }] },
        { kind: 'at once where no project of the workspace builds the analyzer', publishes: false, dependsOn: undefined },
    ])('runs $kind', async ({ publishes, dependsOn }, { workspace }) => {
        const graph = await workspace
            .write({
                'Workspace.slnx': '<Solution />\n',
                'tools/Analyzer/Analyzer.csproj': project,
                'tools/Other/Other.csproj': project,
                ...report([node('tools/Analyzer/Analyzer.csproj', { evaluated: mode('Release', { published: [publishes ? relative(workspace.root, dirname(ANALYZER)) : 'tools/Analyzer/dist'] }) }), node('tools/Other/Other.csproj', { evaluated: mode('Release', { published: ['tools/Other/dist'] }) })]),
            })
            .graph({ createNodes, createDependencies });
        const target = graph.nodes['Workspace']?.data.targets?.['upgrade--dotnet'];

        expect([target?.executor, target?.dependsOn]).toStrictEqual(['@rasm/nx-distribution:upgrade', dependsOn]);
    });
});
