import { mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { refused } from '@rasm/testing/errors';
import { pathed } from '@rasm/testing/nx';
import { describe, expect, onTestFinished, vi } from 'vitest';
import { it, node, report, STUB } from '#/analyzer';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = '<Project Sdk="Microsoft.NET.Sdk" />\n';
const ANALYZER = join(dirname(fileURLToPath(import.meta.resolve('@rasm/nx-dotnet/package.json'))), 'analyzer', 'dist', 'MsbuildAnalyzer.dll');

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyses = (calls: string) => calls.split('\n').filter((call) => call.startsWith('--roll-forward ')).length;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('launch', () => {
    it('runs the built analyzer under the newest runtime over the workspace root, the MSBuild files it matched on stdin', async ({ workspace, analyzer }) => {
        await workspace.write({ 'apps/App/App.csproj': PROJECT, 'Directory.Build.props': '<Project />\n', 'global.json': '{}\n', 'README.md': '# fixture\n', ...report([node('apps/App/App.csproj')]) }).graph({ createNodes, createDependencies });

        expect([analyzer.logged('calls').trim().split('\n').toSorted(), analyzer.logged('stdin').split('\n').toSorted()]).toStrictEqual([
            [`--roll-forward LatestMajor ${ANALYZER} ${workspace.root}`, '--version'],
            ['Directory.Build.props', 'apps/App/App.csproj', 'global.json'],
        ]);
    });

    it('gives the analyzer alone prerelease roll forward and crash dumps under .artifacts/dotnet/dumps', async ({ workspace, analyzer }) => {
        await workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj')]) }).graph({ createNodes, createDependencies });

        expect([analyzer.logged('env'), env['DOTNET_ROLL_FORWARD_TO_PRERELEASE']]).toStrictEqual([`1|${join(workspace.root, '.artifacts/dotnet/dumps/%e-%p.dmp')}|1\n`, undefined]);
    });

    it('starts no dotnet where the workspace holds no MSBuild file, so a machine without it reads the graph', async ({ workspace }) => {
        const folder = mkdtempSync(join(tmpdir(), 'dotnet-absent-'));
        onTestFinished(() => rmSync(folder, { recursive: true }));
        pathed(folder, {});

        const graph = await workspace.write({ 'README.md': '# fixture\n' }).graph({ createNodes, createDependencies });

        expect(graph.nodes).toStrictEqual({});
    });
});

describe('cache', () => {
    it('reuses the analysis while no file it read changes', async ({ workspace, analyzer }) => {
        await workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj')], { evaluated: ['apps/App/App.csproj'] }) }).graph({ createNodes, createDependencies });

        await workspace.graph({ createNodes, createDependencies });

        expect(analyses(analyzer.logged('calls'))).toBe(1);
    });

    it.for([
        { kind: 'a project file', stated: {}, edited: 'apps/App/App.csproj' },
        { kind: 'a file the evaluation imported', stated: {}, edited: 'build/Common.msbuild' },
        { kind: 'the assets file a restore rewrote', stated: { restored: ['apps/App/obj/project.assets.json'] }, edited: 'apps/App/obj/project.assets.json' },
        { kind: 'a lock file', stated: { locks: ['apps/App/packages.lock.json'] }, edited: 'apps/App/packages.lock.json' },
        { kind: 'the SDK version global.json selects', stated: {}, edited: STUB.sdk },
    ])('analyzes again once $kind changes', async ({ stated, edited }, { workspace, analyzer }) => {
        await workspace
            .write({ 'apps/App/App.csproj': PROJECT, 'build/Common.msbuild': '<Project />\n', 'apps/App/obj/project.assets.json': '{}\n', 'apps/App/packages.lock.json': '{}\n', ...report([node('apps/App/App.csproj', stated)], { evaluated: ['apps/App/App.csproj', 'build/Common.msbuild'] }) })
            .graph({ createNodes, createDependencies });

        await workspace.write({ [edited]: '<Project>\n  <!-- edited -->\n</Project>\n' }).graph({ createNodes, createDependencies });

        expect(analyses(analyzer.logged('calls'))).toBe(2);
    });

    it('analyzes again once a file the build reads outside the workspace changes', async ({ workspace, analyzer }) => {
        const outside = mkdtempSync(join(tmpdir(), 'dotnet-outside-'));
        onTestFinished(() => rmSync(outside, { recursive: true }));
        writeFileSync(join(outside, 'Host.dll'), 'first');
        await workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj', { external: [join(outside, 'Host.dll')] })], { evaluated: ['apps/App/App.csproj'] }) }).graph({ createNodes, createDependencies });

        writeFileSync(join(outside, 'Host.dll'), 'second');
        await workspace.graph({ createNodes, createDependencies });

        expect(analyses(analyzer.logged('calls'))).toBe(2);
    });

    it('reuses the analysis of a workspace holding solutions alone', async ({ workspace, analyzer }) => {
        await workspace.write({ 'Workspace.slnx': '<Solution />\n', ...report([]) }).graph({ createNodes, createDependencies });

        await workspace.graph({ createNodes, createDependencies });

        expect(analyses(analyzer.logged('calls'))).toBe(1);
    });

    it('keeps no analysis that reported a failure', async ({ workspace, analyzer }) => {
        workspace.write({ 'apps/App/App.csproj': PROJECT, ...report([node('apps/App/App.csproj')], { failures: [{ file: 'apps/App/App.csproj', message: 'MSB4025: The project file could not be loaded.' }] }) });
        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^MSB4025: /u));

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^MSB4025: /u));

        expect(analyses(analyzer.logged('calls'))).toBe(2);
    });
});
