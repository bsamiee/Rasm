import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-dotnet';
import { TempFs } from '@rasm/testing/temp-fs';
import { minimatch } from 'minimatch';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const project = (properties = '') => `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    ${properties}\n  </PropertyGroup>\n</Project>\n`;

const files = {
    'Directory.Build.props': '<Project />\n',
    'apps/Directory.Build.targets': '<Project />\n',
    'apps/MyLib/MyLib.csproj': project(),
    'apps/MyLib.Tests/MyLib.Tests.csproj': project('<IsTestProject>true</IsTestProject>'),
    'Workspace.slnx': '<Solution>\n  <Project Path="apps/MyLib/MyLib.csproj" />\n  <Project Path="apps/MyLib.Tests/MyLib.Tests.csproj" />\n</Solution>\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('dotnet-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-dotnet under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx posix inputs, outputs, and project files for every target', async ({ context }) => {
        const results = await createNodes[1](
            Object.keys(files).filter((file) => minimatch(file, createNodes[0], { dot: true })),
            undefined,
            context,
        );

        const projects = Object.fromEntries(results.flatMap(([, { projects: held = {} }]) => Object.entries(held)));
        const named = Object.values(projects)
            .flatMap(({ targets = {} }) => Object.values(targets))
            .flatMap(({ inputs = [], outputs = [] }) => [...inputs, ...outputs])
            .filter((at): at is string => typeof at === 'string');
        expect([Object.keys(projects).toSorted(), named.filter((at) => at.includes('\\'))]).toStrictEqual([['.', 'apps/MyLib', 'apps/MyLib.Tests'], []]);
        expect(projects['apps/MyLib']?.targets?.['build--dotnet']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/Directory.Build.props', '{workspaceRoot}/apps/Directory.Build.targets']));
        expect(projects['apps/MyLib']?.targets?.['pack--dotnet']?.options).toMatchObject({ project: '{projectRoot}/MyLib.csproj', outputs: ['apps/MyLib/bin/Debug/*.nupkg'] });
        expect(projects['apps/MyLib.Tests']?.targets?.['test--dotnet']?.outputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/TestResults', '{projectRoot}/{options.results-directory}']));
        expect(projects['.']?.targets?.['upgrade--dotnet']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/Workspace.slnx', '{workspaceRoot}/Directory.Build.props']));
    });
});
