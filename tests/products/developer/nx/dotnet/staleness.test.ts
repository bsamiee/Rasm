import { execFileSync, spawnSync } from 'node:child_process';
import { appendFileSync, existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { refused } from '@rasm/testing/errors';
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
const PUBLISHED = {
    'Directory.Build.props': '<Project>\n  <PropertyGroup>\n    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>\n    <TargetFramework>net10.0</TargetFramework>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n  </PropertyGroup>\n</Project>\n',
    'Directory.Build.targets': `<Project>\n  <ItemGroup>\n    <PackageReference Remove="@(PackageReference)" />\n    <ProjectReference Include="$(MSBuildThisFileDirectory)roslyn/Rules/Rules.csproj" ReferenceOutputAssembly="false" Condition="'$(MSBuildProjectName)' != 'Rules'" />\n  </ItemGroup>\n</Project>\n`,
    'roslyn/Rules/Rules.csproj': LIBRARY,
    'roslyn/Rules/Shapes.cs': 'namespace Rules;\n\npublic static class Shapes;\n',
    'analyzer/MsbuildAnalyzer.csproj': readFileSync(fileURLToPath(new URL('analyzer/MsbuildAnalyzer.csproj', import.meta.resolve('@rasm/nx-dotnet/package.json'))), 'utf-8'),
    'analyzer/Program.cs': 'return 0;\n',
};

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
        readdirSync(analyzer).forEach((entry) => rmSync(join(analyzer, entry), { recursive: true, force: true }));
    });
    pathed(folder, { dotnet: `echo launched >> ${folder}/launches.log\nexec /bin/cat .stub/report.json` });
    return { logged, launches: () => (existsSync(join(folder, 'launches.log')) ? readFileSync(join(folder, 'launches.log'), 'utf-8').trim().split('\n').length : 0) };
};

const warnings = (logged: MockInstance<typeof console.warn>) => logged.mock.calls.map((call) => call.join(' ')).filter((line) => line.includes('MsbuildAnalyzer'));

const predates = (files: readonly string[]) => refused(new RegExp(`MsbuildAnalyzer\\.dll predates ${files.map((file) => `\\S*/${RegExp.escape(file)}`).join(', ')}, dotnet publish \\S+/analyzer/MsbuildAnalyzer\\.csproj --output \\S+/analyzer/dist builds it$`, 'u'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('analyzer build', () => {
    it('infers no MSBuild project where the analyzer is absent and launches nothing, warning once with the command that builds it', async ({ workspace }) => {
        const { logged, launches } = reporting();

        const graph = await workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        expect([graph.nodes, launches(), warnings(logged).filter((line) => /MsbuildAnalyzer\.dll is absent, dotnet publish \S+\/analyzer\/MsbuildAnalyzer\.csproj --output \S+\/analyzer\/dist builds it$/u.test(line)).length]).toStrictEqual([{}, 0, 1]);
    });

    it.for([
        { kind: 'a source changed after it was published', edited: { 'analyzer/Catalog.cs': 'namespace MsbuildAnalyzer;\n\ninternal static class Edited;\n' }, removed: [] },
        { kind: 'a source it was published with is gone', edited: {}, removed: ['analyzer/Catalog.cs'] },
        { kind: 'both sources changed after it was published', edited: { 'analyzer/Catalog.cs': 'namespace Edited;\n', 'analyzer/MsbuildAnalyzer.csproj': '<Project />\n' }, removed: [] },
    ])('refuses the graph naming exactly the sources shasum -c refuses and the command that rebuilds it, as $kind', async ({ edited, removed }, { workspace }) => {
        reporting();
        packaged({ 'analyzer/MsbuildAnalyzer.csproj': LIBRARY, 'analyzer/Catalog.cs': 'namespace MsbuildAnalyzer;\n', 'analyzer/dist/MsbuildAnalyzer.dll': 'built' });
        packaged({ 'analyzer/dist/MsbuildAnalyzer.sha256': shasum('MsbuildAnalyzer.csproj', 'Catalog.cs').stdout.replaceAll(/^\w+/gmu, (digest) => digest.toUpperCase()) });
        packaged(edited);
        removed.forEach((file) => rmSync(join(analyzer, file)));
        const failed = [...shasum('-c', 'dist/MsbuildAnalyzer.sha256').stdout.matchAll(/^(?<file>.+): FAILED/gmu)].map((match) => `analyzer/${match.groups?.['file']}`);

        const graph = workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        await expect(graph).rejects.toSatisfy(predates(failed));
    });

    it.for([
        { kind: 'no file its build read changed', appended: [] },
        { kind: 'root MSBuild files its evaluation imports and a source of the Roslyn project it references changed', appended: ['Directory.Build.props', 'Directory.Build.targets', 'roslyn/Rules/Shapes.cs'] },
    ])('infers the reported projects with no warning after a real dotnet publish of the analyzer, as $kind', { timeout: 300_000 }, async ({ appended }, { workspace }) => {
        packaged(PUBLISHED);
        execFileSync('dotnet', ['publish', 'analyzer/MsbuildAnalyzer.csproj', '--output', 'analyzer/dist', '--disable-build-servers'], { cwd: analyzer, stdio: 'ignore' });
        appended.forEach((file) => appendFileSync(join(analyzer, file), '\n'));
        const { logged } = reporting();

        const graph = await workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        expect([Object.keys(graph.nodes), warnings(logged)]).toStrictEqual([['App'], []]);
    });

    it.for([
        { kind: 'a source it compiles changed', appended: 'analyzer/Program.cs' },
        { kind: 'its project file changed', appended: 'analyzer/MsbuildAnalyzer.csproj' },
        { kind: 'its lock file changed', appended: 'analyzer/packages.lock.json' },
    ])('refuses the graph after a real dotnet publish of the analyzer, naming exactly the changed file and the command that rebuilds it, as $kind', { timeout: 300_000 }, async ({ appended }, { workspace }) => {
        packaged(PUBLISHED);
        execFileSync('dotnet', ['publish', 'analyzer/MsbuildAnalyzer.csproj', '--output', 'analyzer/dist', '--disable-build-servers'], { cwd: analyzer, stdio: 'ignore' });
        appendFileSync(join(analyzer, appended), '\n');
        reporting();

        const graph = workspace.write({ 'apps/App/App.csproj': LIBRARY, ...REPORT }).graph({ createNodes, createDependencies });

        await expect(graph).rejects.toSatisfy(predates([appended]));
    });
});
