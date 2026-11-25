import { execFileSync } from 'node:child_process';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import { Build, execute } from '@rasm/nx-dotnet/executors/build';
import { it, outputs, targetOptions } from '@rasm/testing/nx';
import { Array, Result } from 'effect';
import { expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (stated: string, references: readonly string[] = []) =>
    `<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n${stated}  </PropertyGroup>\n  <ItemGroup>\n${references.map((reference) => `    <ProjectReference Include="${reference.replaceAll('&', '&amp;').replaceAll("'", '&apos;')}" />\n`).join('')}  </ItemGroup>\n</Project>\n`;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.for([
    { kind: 'a plain folder', folder: 'libs/Core' },
    { kind: 'a folder holding XML punctuation', folder: "libs/Core & Kit's" },
])('builds a project in a configuration its reference in $kind lacks, writing the build within the declared outputs of both, the intermediate folders alone outside them, and the reference in its own default', async ({ folder }, { workspace }) => {
    const graph = await workspace
        .write({
            [`${folder}/Core.csproj`]: project(''),
            [`${folder}/Shapes.cs`]: 'namespace Core;\n\npublic static class Shapes;\n',
            'apps/App/App.csproj': project('    <Configurations>Debug;Release;Interface</Configurations>\n', [`../../${folder}/Core.csproj`]),
            'apps/App/Views.cs': 'namespace App;\n\npublic static class Views;\n',
        })
        .graph({ createNodes, createDependencies });
    const declared = [...outputs(graph, 'App', 'build--dotnet', 'interface'), ...outputs(graph, 'Core', 'build--dotnet')];
    execFileSync('dotnet', ['restore', 'apps/App/App.csproj'], { cwd: workspace.root, stdio: 'ignore' });
    const before = workspace.files();

    const { success } = await execute(await targetOptions(Build, graph, 'App', 'build--dotnet', 'interface'), workspace.context(graph, 'App', 'build--dotnet', 'interface'));

    const written = [...workspace.files().difference(before)];
    const [inside, outside] = Array.partition(written, (file) => (declared.some((output) => file.startsWith(`${output}/`)) ? Result.succeed(file) : Result.fail(file)));
    expect({ success, inside: new Set(inside.map((file) => file.split('/').slice(0, 4).join('/'))), outside: new Set(outside.map((file) => file.split('/').slice(0, 4).join('/'))) }).toStrictEqual({
        success: true,
        inside: new Set(['apps/App/bin/Interface', `${folder}/bin/Debug`]),
        outside: new Set(['apps/App/obj/Interface', `${folder}/obj/Debug`]),
    });
});

it.for([
    { verb: 'publish', stated: '    <OutputType>Exe</OutputType>\n    <IsPublishable>true</IsPublishable>\n    <PublishDir>$(MSBuildProjectDirectory)/dist/</PublishDir>\n', source: 'return 0;\n', target: 'build--dotnet', written: /^apps\/Tool\/dist\/Tool\.dll$/u },
    { verb: 'pack', stated: '    <IsPackable>true</IsPackable>\n    <Version>1.2.0</Version>\n', source: 'namespace Tool;\n\npublic static class Marker;\n', target: 'pack--dotnet', written: /^apps\/Tool\/bin\/Debug\/Tool\.1\.2\.0\.nupkg$/u },
])('writes what dotnet $verb produces within the outputs $target declares', async ({ stated, source, target, written }, { workspace }) => {
    const graph = await workspace.write({ 'apps/Tool/Tool.csproj': project(stated), 'apps/Tool/Tool.cs': source }).graph({ createNodes, createDependencies });
    execFileSync('dotnet', ['build', 'apps/Tool/Tool.csproj', '--configuration', 'Debug'], { cwd: workspace.root, stdio: 'ignore' });
    const before = workspace.files();

    const { success } = await execute(await targetOptions(Build, graph, 'Tool', target), workspace.context(graph, 'Tool', target));

    const produced = [...workspace.files().difference(before)].filter((file) => written.test(file));
    expect({ success, produced: produced.length, cached: produced.filter((file) => workspace.cached(graph, 'Tool', target).has(file)).length }).toStrictEqual({ success: true, produced: 1, cached: 1 });
});
