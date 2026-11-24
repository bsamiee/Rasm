import { execFileSync, execSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { refused } from '@rasm/testing/errors';
import { environment, it, pathed, shimmed } from '@rasm/testing/nx';
import { Array, Option, Predicate } from 'effect';
import { describe, expect, onTestFinished, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIBRARY = '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n</Project>\n';

// --- [OPERATIONS] ----------------------------------------------------------------------

const scratch = (prefix: string) => {
    const folder = mkdtempSync(join(tmpdir(), prefix));
    onTestFinished(() => rmSync(folder, { recursive: true, force: true }));
    return folder;
};

const counted = () => {
    const [folder, dotnet] = [scratch('dotnet-counted-'), execFileSync('sh', ['-c', 'command -v dotnet'], { encoding: 'utf-8' }).trim()];
    mkdirSync(join(folder, 'sdk', '10.0.100'), { recursive: true });
    shimmed(folder, { dotnet: `case "$1" in *MsbuildAnalyzer.dll) echo analyzed >> ${folder}/analyses.log ;; esac\nexec ${dotnet} "$@"` });
    return { folder, analyses: () => (existsSync(join(folder, 'analyses.log')) ? readFileSync(join(folder, 'analyses.log'), 'utf-8').trim().split('\n').length : 0) };
};

const hashed = (root: string, command: string) => execSync(command, { cwd: root, env: environment(root), encoding: 'utf-8' });

const referencing = (dll: string) => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <Reference Include="Host" HintPath="${dll}" />\n  </ItemGroup>\n</Project>\n`;

const runtime = (inputs: readonly unknown[] = []) =>
    Array.findLast(
        inputs.flatMap((input) => (Predicate.hasProperty(input, 'runtime') && Predicate.isString(input.runtime) ? [input.runtime] : [])),
        (command) => command.startsWith('git '),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('analysis cache', { timeout: 240_000 }, () => {
    it('reuses the analysis while no file the evaluation read changes, and analyzes again once an SDK lands beside the dotnet on PATH', async ({ workspace }) => {
        const shim = counted();
        const read = async () => {
            const { nodes } = await workspace.write({ 'apps/App/App.csproj': LIBRARY }).graph({ createNodes, createDependencies });
            return Object.keys(nodes);
        };
        const reads = [await read(), await read()];
        const reused = shim.analyses();

        mkdirSync(join(shim.folder, 'sdk', '11.0.100'), { recursive: true });
        reads.push(await read());

        expect([reads, reused, shim.analyses()]).toStrictEqual([[['App'], ['App'], ['App']], 1, 2]);
    });

    it.for([
        { kind: 'a Directory.Build.props MSBuild probes for', added: 'apps/Directory.Build.props', text: '<Project />\n' },
        { kind: 'an .editorconfig no evaluation probes for', added: 'apps/.editorconfig', text: 'root = false\n' },
    ])('analyzes again once $kind lands in a folder above the project, and hashes it in build--dotnet', async ({ added, text }, { workspace }) => {
        const shim = counted();
        const read = async () => {
            const { nodes } = await workspace.write({ 'apps/App/App.csproj': LIBRARY }).graph({ createNodes, createDependencies });
            return nodes['App']?.data.targets?.['build--dotnet']?.inputs ?? [];
        };
        const [before] = [await read(), await read()];

        workspace.write({ [added]: text });
        const after = await read();

        expect([before.includes(`{workspaceRoot}/${added}`), after.includes(`{workspaceRoot}/${added}`), shim.analyses()]).toStrictEqual([false, true, 2]);
    });

    it('states in the graph an edit to a file the project imports, read on the next graph', async ({ workspace }) => {
        const importing = '<Project Sdk="Microsoft.NET.Sdk">\n  <Import Project="../../build/Common.msbuild" />\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n</Project>\n';
        const before = await workspace.write({ 'apps/App/App.csproj': importing, 'build/Common.msbuild': '<Project />\n' }).graph({ createNodes, createDependencies });

        const after = await workspace.write({ 'build/Common.msbuild': '<Project>\n  <ItemGroup>\n    <NxTag Include="edited" />\n  </ItemGroup>\n</Project>\n' }).graph({ createNodes, createDependencies });

        expect([before.nodes['App']?.data.tags ?? [], after.nodes['App']?.data.tags ?? []]).toStrictEqual([[], ['edited']]);
    });

    it('keeps no analysis MSBuild failed to evaluate, refusing the graph with its error on every read', async ({ workspace }) => {
        const shim = counted();
        workspace.write({ 'apps/App/App.csproj': LIBRARY, 'apps/Bad/Bad.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n' });

        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^The project file could not be loaded\. /u));
        await expect(workspace.graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/^The project file could not be loaded\. /u));

        expect(shim.analyses()).toBe(2);
    });
});

describe('files Nx hands the plugin', () => {
    it('are the project and solution files alone, so an edit to a file MSBuild, NuGet, or the SDK finds beside them is no project configuration change', async ({ workspace }) => {
        const markers = ['Workspace.slnx', 'apps/App/App.csproj', 'apps/Fs/Fs.fsproj', 'apps/Vb/Vb.vbproj', 'tools/Tools.sln'];
        const sides = ['.config/.globalconfig', '.editorconfig', 'Directory.Build.props', 'Directory.Build.rsp', 'Directory.Build.targets', 'Directory.Packages.props', 'apps/NuGet.config', 'build/Common.targets', 'global.json', 'libs/Directory.Build.props', 'libs/NuGet.Config', 'nuget.config'];
        workspace.write(Object.fromEntries([...markers, ...sides].map((at) => [at, ''])));

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        expect(matched.toSorted()).toStrictEqual(markers);
    });
});

describe('files outside the workspace', { timeout: 240_000 }, () => {
    it('hashes a referenced file outside the workspace by its content, naming no path outside the workspace', async ({ workspace }) => {
        const dll = join(scratch('dotnet-outside-'), 'Host.dll');
        writeFileSync(dll, 'first');
        const graph = await workspace.write({ 'apps/App/App.csproj': referencing(dll) }).graph({ createNodes, createDependencies });
        const command = Option.getOrThrow(runtime(graph.nodes['App']?.data.targets?.['build--dotnet']?.inputs));
        const first = hashed(workspace.root, command);

        writeFileSync(dll, 'second');

        expect([first === hashed(workspace.root, command), command.includes(dll)]).toStrictEqual([false, false]);
    });

    it('writes the listing its hash reads again on a reused analysis once the listing is gone', async ({ workspace }) => {
        const dll = join(scratch('dotnet-outside-'), 'Host.dll');
        writeFileSync(dll, 'first');
        const shim = counted();
        const graph = await workspace.write({ 'apps/App/App.csproj': referencing(dll) }).graph({ createNodes, createDependencies });
        const command = Option.getOrThrow(runtime(graph.nodes['App']?.data.targets?.['build--dotnet']?.inputs));
        const first = hashed(workspace.root, command);
        rmSync(join(workspace.root, 'tmp/project-graph-cache/dotnet-external'), { recursive: true });

        await workspace.graph({ createNodes, createDependencies });

        expect([hashed(workspace.root, command), shim.analyses()]).toStrictEqual([first, 1]);
    });
});

describe('analyzer process', () => {
    it('infers every project of another ecosystem and no MSBuild project where no folder of PATH holds dotnet, warning once', async ({ workspace }) => {
        pathed(scratch('dotnet-absent-'), {});
        const logged = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onTestFinished(() => logged.mockRestore());

        const graph = await workspace.write({ 'package.json': JSON.stringify({ name: '@fixture/root', workspaces: ['tools/*'] }), 'tools/web/package.json': JSON.stringify({ name: '@fixture/web' }), 'apps/App/App.csproj': LIBRARY, 'libs/Core/Core.csproj': LIBRARY }).graph({ createNodes, createDependencies });

        expect([Object.keys(graph.nodes).toSorted(), logged.mock.calls.filter((call) => call.join(' ').includes('No MSBuild project of the workspace enters the graph')).length]).toStrictEqual([['@fixture/web'], 1]);
    });

    it('refuses an analyzer running past NX_DOTNET_PROJECT_GRAPH_TIMEOUT seconds, naming the variable', async ({ workspace }) => {
        vi.stubEnv('NX_DOTNET_PROJECT_GRAPH_TIMEOUT', '1');
        pathed(scratch('dotnet-hanging-'), { dotnet: 'exec /bin/sleep 20' });

        await expect(workspace.write({ 'apps/App/App.csproj': LIBRARY }).graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/ran past 1 seconds, set NX_DOTNET_PROJECT_GRAPH_TIMEOUT to a higher count of seconds$/u));
    });

    it('refuses a report it cannot decode, naming the command that builds the analyzer', async ({ workspace }) => {
        pathed(scratch('dotnet-garbled-'), { dotnet: 'echo Unhandled exception. System.IO.FileNotFoundException' });

        await expect(workspace.write({ 'apps/App/App.csproj': LIBRARY }).graph({ createNodes, createDependencies })).rejects.toSatisfy(refused(/printed a report this plugin cannot read, .* dotnet publish \S+\/analyzer\/MsbuildAnalyzer\.csproj --output \S+\/analyzer\/dist builds it: /u));
    });
});
