import { layer } from '@effect/platform-node/NodeServices';
import plugin from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Effect, Layer, Path, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MSBUILD = {
    evaluated: 'dotnet msbuild <project> -getProperty -getItem',
    name: 'dotnet msbuild <project> -getProperty:MSBuildProjectName',
    kinds: 'dotnet msbuild <project> -getProperty:IsTestingPlatformApplication -getProperty:IsTestProject -getProperty:IsPackable',
    compiled: 'dotnet msbuild <project> -getItem:Compile',
    references: 'dotnet msbuild <project> -getItem:ProjectReference',
    target: 'dotnet msbuild <project> -getProperty:TargetPath',
    version: 'dotnet msbuild <project> -getProperty:Version',
    packageFile: 'dotnet msbuild <project> -getProperty:PackageId -getProperty:Version',
    search: 'dotnet package search <id> --exact-match --format json',
} as const;
const PROPERTIES = ['MSBuildProjectName', 'Version', 'PackageId', 'IsPackable', 'IsTestProject', 'IsTestingPlatformApplication', 'TargetPath'] as const;
const VERSION = '        <Version>0.1.0</Version>\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'global.json': '{ "test": { "runner": "Microsoft.Testing.Platform" } }\n',
    'Directory.Build.props': '<Project>\n    <PropertyGroup>\n        <TargetFramework>net10.0</TargetFramework>\n        <Nullable>enable</Nullable>\n    </PropertyGroup>\n</Project>\n',
    'Directory.Packages.props':
        '<Project>\n    <PropertyGroup>\n        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n    </PropertyGroup>\n    <ItemGroup>\n        <PackageVersion Include="xunit.v3.mtp-v2" Version="4.0.3" />\n    </ItemGroup>\n</Project>\n',
    'src/Core/Fixture.Core.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n${VERSION}    </PropertyGroup>\n</Project>\n`,
    'src/Core/Numbers.cs': 'namespace Fixture.Core;\n\npublic static class Numbers\n{\n    public static int One() => 1;\n}\n',
    'src/Middle/Fixture.Middle.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n    <!-- Middle of the chain -->\n    <PropertyGroup>\n${VERSION}        <Description>Fixture middle</Description>\n    </PropertyGroup>\n\n    <ItemGroup Label="Workspace Projects">\n        <ProjectReference Include="../Core/Fixture.Core.csproj" />\n    </ItemGroup>\n</Project>\n`,
    'src/Middle/Sums.cs': 'namespace Fixture.Middle;\n\npublic static class Sums\n{\n    public static int Two() => Fixture.Core.Numbers.One() + 1;\n}\n',
    'src/App/Fixture.App.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <OutputType>Exe</OutputType>\n        <IsPackable>false</IsPackable>\n${VERSION}    </PropertyGroup>\n    <ItemGroup>\n        <ProjectReference Include="../Middle/Fixture.Middle.csproj" />\n    </ItemGroup>\n</Project>\n`,
    'src/App/Program.cs': 'System.Console.WriteLine(Fixture.Middle.Sums.Two());\n',
    'tests/Core.Tests/Fixture.Core.Tests.csproj': `<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <OutputType>Exe</OutputType>\n        <IsPackable>false</IsPackable>\n${VERSION}    </PropertyGroup>\n    <ItemGroup>\n        <PackageReference Include="xunit.v3.mtp-v2" />\n        <ProjectReference Include="../../src/Core/Fixture.Core.csproj" />\n    </ItemGroup>\n</Project>\n`,
    'tests/Core.Tests/NumbersTests.cs': 'public class NumbersTests\n{\n    [Xunit.Fact]\n    public void OneIsOne() => Xunit.Assert.Equal(1, Fixture.Core.Numbers.One());\n}\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Item = Schema.Struct({ FullPath: Schema.String });
const Evaluated = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Record(Schema.Literals(PROPERTIES), Schema.String), Items: Schema.Struct({ ProjectReference: Schema.Array(Item), Compile: Schema.Array(Item) }) })).annotate({
    description: 'JSON dotnet msbuild <project> -getProperty -getItem prints: the properties and items MSBuild evaluates for one project',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (path: Path.Path, root: string, manifest: string, { Properties: stated, Items: { ProjectReference, Compile } }: typeof Evaluated.Type) => ({
    name: stated.MSBuildProjectName,
    version: stated.Version,
    packageId: stated.PackageId,
    root: path.dirname(manifest),
    manifest,
    packable: stated.IsPackable === 'true',
    tested: stated.IsTestProject === 'true' || stated.IsTestingPlatformApplication === 'true',
    target: path.relative(root, stated.TargetPath),
    sources: [manifest, ...Compile.map(({ FullPath }) => path.relative(root, FullPath)).filter((file) => !file.split(path.sep).includes('obj'))],
    dependencies: ProjectReference.map(({ FullPath }) => path.relative(root, path.dirname(FullPath))),
});

const evaluated = Effect.fnUntraced(function* (manifest: string) {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { code, stdout } = yield* called('dotnet', ['msbuild', manifest, ...PROPERTIES.map((property) => `-getProperty:${property}`), '-getItem:ProjectReference', '-getItem:Compile']);
    return Number(code) === 0 ? [described(path, root, manifest, yield* Schema.decodeEffect(Evaluated)(stdout))] : [];
}, Effect.orDie);

const members = Effect.gen(function* () {
    const { called } = yield* Consumer;
    const listed = yield* called('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', '*.csproj']);
    const projects = yield* Effect.forEach(
        listed.stdout.split('\n').filter((manifest) => manifest !== ''),
        evaluated,
        { concurrency: 'unbounded' },
    );
    return projects.flat();
});

const member = Effect.fnUntraced(function* (root: string) {
    const stated = yield* members;
    return yield* Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root)));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const restored = Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('dotnet', ['restore', 'tests/Core.Tests/Fixture.Core.Tests.csproj'])));

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    restored.pipe(
        Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name], release: { projects: ['tag:release'] } }, ignored: ['bin', 'obj', 'TestResults', 'artifacts'], files: { ...files, ...stated } })),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { files, MSBUILD, member, members, workspace };
