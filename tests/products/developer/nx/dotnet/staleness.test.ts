import { execFileSync, spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, type MockInstance, onTestFinished, vi } from 'vitest';
import { node, report } from '#/analyzer';

const { analyzer, targets } = await vi.hoisted(async () => {
    const [fs, os, path, url] = await Promise.all([import('node:fs'), import('node:os'), import('node:path'), import('node:url')]);
    return { analyzer: fs.mkdtempSync(path.join(os.tmpdir(), 'dotnet-package-')), targets: url.fileURLToPath(new URL('targets.ts', import.meta.resolve('@rasm/nx-dotnet/package.json'))) };
});

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(targets, async (actual) => ({ ...(await actual<Record<string, unknown>>()), ANALYZER: join(analyzer, 'analyzer', 'dist', 'MsbuildAnalyzer.dll') }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIBRARY = '<Project Sdk="Microsoft.NET.Sdk" />\n';
const REPORT = report([node('apps/App/App.csproj')]);
const SHASUM = execFileSync('sh', ['-c', 'command -v shasum'], { encoding: 'utf-8' }).trim();

// --- [OPERATIONS] ----------------------------------------------------------------------

const packaged = (files: Readonly<Record<string, string>>) =>
    Object.entries(files).forEach(([file, text]) => {
        mkdirSync(dirname(join(analyzer, file)), { recursive: true });
        writeFileSync(join(analyzer, file), text);
    });

const shasum = (...flags: readonly string[]) => spawnSync(SHASUM, ['-a', '256', ...flags], { cwd: join(analyzer, 'analyzer'), encoding: 'utf-8' });

const reporting = () => {
    const folder = mkdtempSync(join(tmpdir(), 'dotnet-reporting-'));
    const logged = vi.spyOn(console, 'warn').mockReturnValue(undefined);
    onTestFinished(() => {
        logged.mockRestore();
        rmSync(folder, { recursive: true });
        rmSync(join(analyzer, 'analyzer'), { recursive: true, force: true });
    });
    pathed(folder, { dotnet: `echo launched >> ${folder}/launches.log\nexec /bin/cat .stub/report.json` });
    return { logged, launches: () => (existsSync(join(folder, 'launches.log')) ? readFileSync(join(folder, 'launches.log'), 'utf-8').trim().split('\n').length : 0) };
};

const warnings = (logged: MockInstance<typeof console.warn>) => logged.mock.calls.map((call) => call.join(' ')).filter((line) => line.includes('MsbuildAnalyzer'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('analyzer build', () => {
    it('infers no MSBuild project where the analyzer is absent and launches nothing, warning once with the command that builds it', async ({ workspace }) => {
        const { logged, launches } = reporting();

        const graph = await workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        expect([graph.nodes, launches(), warnings(logged).filter((line) => /MsbuildAnalyzer\.dll is absent, dotnet publish \S+\/analyzer\/MsbuildAnalyzer\.csproj builds it$/u.test(line)).length]).toStrictEqual([{}, 0, 1]);
    });

    it.for([
        { kind: 'its sources match the checksums it was published with', edited: {}, removed: [] },
        { kind: 'a source changed after it was published', edited: { 'analyzer/Catalog.cs': 'namespace MsbuildAnalyzer;\n\ninternal static class Edited;\n' }, removed: [] },
        { kind: 'a source it was published with is gone', edited: {}, removed: ['analyzer/Catalog.cs'] },
        { kind: 'both sources changed after it was published', edited: { 'analyzer/Catalog.cs': 'namespace Edited;\n', 'analyzer/MsbuildAnalyzer.csproj': '<Project />\n' }, removed: [] },
    ])('infers the reported projects only where shasum -c refuses no source, naming exactly the sources it refuses, as $kind', async ({ edited, removed }, { workspace }) => {
        const { logged } = reporting();
        packaged({ 'analyzer/MsbuildAnalyzer.csproj': LIBRARY, 'analyzer/Catalog.cs': 'namespace MsbuildAnalyzer;\n', 'analyzer/dist/MsbuildAnalyzer.dll': 'built' });
        packaged({ 'analyzer/dist/MsbuildAnalyzer.sha256': shasum('MsbuildAnalyzer.csproj', 'Catalog.cs').stdout.replaceAll(/^\w+/gmu, (digest) => digest.toUpperCase()) });
        packaged(edited);
        removed.forEach((file) => rmSync(join(analyzer, file)));
        const refused = [...shasum('-c', 'dist/MsbuildAnalyzer.sha256').stdout.matchAll(/^(?<file>.+): FAILED/gmu)].map((match) => match.groups?.['file']);

        const graph = await workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        const named = warnings(logged)
            .flatMap((line) => [...(/MsbuildAnalyzer\.dll predates (?<files>.+), dotnet publish /u.exec(line)?.groups?.['files']?.split(', ') ?? [])])
            .map((file) => file.slice(file.lastIndexOf('/analyzer/') + '/analyzer/'.length));
        expect([Object.keys(graph.nodes), new Set(named)]).toStrictEqual([refused.length === 0 ? ['App'] : [], new Set(refused)]);
    });
});
