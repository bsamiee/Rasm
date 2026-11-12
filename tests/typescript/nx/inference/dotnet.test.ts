import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-dotnet';
import manifest from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const project = (items: string) => `<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <TargetFramework>net11.0</TargetFramework>\n    </PropertyGroup>\n${items}</Project>\n`;

const reference = (name: string) => `    <ItemGroup>\n        <ProjectReference Include="../${name}/${name}.csproj" />\n    </ItemGroup>\n`;

const MULTI = (items: string) => project(items).replace('<TargetFramework>net11.0</TargetFramework>', '<TargetFrameworks>net11.0;net10.0</TargetFrameworks>');

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['dotnet'],
    state: ['obj'],
    typed: true,
    runs: ['build'],
    compiled: ['Core/Sheet.cs', 'build--dotnet'],
    changed: ['App/App.csproj', project('')],
    files: {
        'package.json': '{ "name": "root" }\n',
        'Core/Core.csproj': project(''),
        'Core/Sheet.cs': 'namespace Core;\n\npublic sealed record Sheet(string Name);\n',
        'App/App.csproj': project('    <ItemGroup>\n        <ProjectReference Include="../Core/Core.csproj" />\n    </ItemGroup>\n'),
        'App/Program.cs': 'namespace App;\n\npublic static class Program\n{\n    public static string Name => new Core.Sheet("plan").Name;\n}\n',
        'Sheet.Report/Sheet.Report.csproj': project('    <ItemGroup>\n        <NxImplicitDependency Include="Core" />\n    </ItemGroup>\n'),
    },
    broken: ['App/App.csproj', '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('links projects that target more than one framework once', () =>
        inferred({ ...subject, files: { ...subject.files, 'Core/Core.csproj': MULTI(''), 'App/App.csproj': MULTI('    <ItemGroup>\n        <ProjectReference Include="../Core/Core.csproj" />\n    </ItemGroup>\n') } }).pipe(
            Effect.map(edges),
            Effect.map(Array.filter(({ source }) => source === 'App')),
            Effect.map((linked) => assert.deepStrictEqual(linked, [{ source: 'App', target: 'Core', type: 'static' }], 'nrwl/nx@2a424d3 e2e/dotnet/src/dotnet-dependency-graph.test.ts "Multi-targeting projects"')),
        ),
    );

    it.effect('links each project to the projects it references directly and to none it reaches through them', () =>
        inferred({
            ...subject,
            files: {
                'package.json': '{ "name": "root" }\n',
                'LeafLib/LeafLib.csproj': project(''),
                'MiddleLib/MiddleLib.csproj': project(reference('LeafLib')),
                'AppProject/AppProject.csproj': project(reference('MiddleLib')),
            },
        }).pipe(
            Effect.map(edges),
            Effect.map(Array.map(({ source, target }) => `${source} ${target}`)),
            Effect.map((linked) => assert.deepStrictEqual(linked, ['AppProject MiddleLib', 'MiddleLib LeafLib'], 'nrwl/nx@2a424d3 e2e/dotnet/src/dotnet-dependency-graph.test.ts "should only show direct dependencies, not transitive ones"')),
        ),
    );

    it.effect('links both paths of a diamond of project references', () =>
        inferred({
            ...subject,
            files: {
                'package.json': '{ "name": "root" }\n',
                'Shared/Shared.csproj': project(''),
                'UserService/UserService.csproj': project(reference('Shared')),
                'OrderService/OrderService.csproj': project(reference('Shared')),
                'ApiGateway/ApiGateway.csproj': project(`${reference('UserService')}${reference('OrderService')}`),
            },
        }).pipe(
            Effect.map(edges),
            Effect.map(Array.map(({ source, target, type }) => `${source} ${target} ${type}`)),
            Effect.map((linked) => assert.deepStrictEqual(linked, ['ApiGateway OrderService static', 'ApiGateway UserService static', 'OrderService Shared static', 'UserService Shared static'], 'nrwl/nx@2a424d3 e2e/dotnet/src/dotnet-advanced-msbuild.test.ts "should detect all project dependencies"')),
        ),
    );

    it.effect('names each project file by its project name, links a ProjectReference from the project file that states it, and links an implicit dependency its NxImplicitDependency item states', () =>
        Effect.map(inferred(subject), (graph) =>
            assert.deepOwnInclude(
                { ...roots(graph), edges: edges(graph) },
                {
                    Core: 'Core',
                    App: 'App',
                    'Sheet.Report': 'Sheet.Report',
                    edges: [
                        { source: 'App', target: 'Core', type: 'static' },
                        { source: 'Sheet.Report', target: 'Core', type: 'implicit' },
                    ],
                },
            ),
        ),
    );
});
