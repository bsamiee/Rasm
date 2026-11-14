import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Path, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'dotnet msbuild <project> -getProperty -getItem:ProjectReference -getItem:Compile';
const [APP, CORE, TESTS, PROPS] = ['src/App', 'src/Core', 'tests/Core.Tests', 'Directory.Build.props'] as const;
const SOURCES = { core: 'src/Core/Numbers.cs', tests: 'tests/Core.Tests/NumbersTests.cs' } as const;
const PROPERTIES = ['MSBuildProjectName', 'Version', 'PackageId', 'IsPackable', 'IsTestProject', 'IsTestingPlatformApplication', 'TargetPath'] as const;
const VERSION = '        <Version>0.1.0</Version>\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'global.json': '{ "test": { "runner": "Microsoft.Testing.Platform" } }\n',
    'Directory.Build.props': '<Project>\n    <PropertyGroup>\n        <TargetFramework>net10.0</TargetFramework>\n        <Nullable>enable</Nullable>\n    </PropertyGroup>\n</Project>\n',
    'Directory.Packages.props': '<Project>\n    <PropertyGroup>\n        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n    </PropertyGroup>\n    <ItemGroup>\n        <PackageVersion Include="xunit.v3.mtp-v2" Version="4.0.3" />\n    </ItemGroup>\n</Project>\n',
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
const Found = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String })) })) })).annotate({
    description: 'JSON dotnet package search --format json prints: per source the packages matching the id, oldest version first',
});
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
    released: stated.IsPackable === 'true',
    verbs: ['build', 'clean', ...(stated.IsTestProject === 'true' || stated.IsTestingPlatformApplication === 'true' ? ['test'] : []), ...(stated.IsPackable === 'true' ? ['pack'] : [])],
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

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));

const undeclared = (created: readonly string[], declared: readonly string[]) => created.filter((file) => !declared.some((output) => file === output || file.startsWith(`${output}/`)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const bound = Duration.toMillis('8 minutes');
const options = { timeout: bound, excludeTestServices: true } as const;

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    Layer.effectDiscard(Effect.flatMap(Consumer, ({ called }) => called('dotnet', ['restore', 'tests/Core.Tests/Fixture.Core.Tests.csproj']))).pipe(
        Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name], release: { projects: ['tag:release'] } }, ignored: ['bin', 'obj', 'TestResults', 'artifacts'], files: { ...files, ...stated } })),
        Layer.provideMerge(Packed.layer(plugin.name)),
        Layer.provideMerge(layer),
    );

conforms({
    workspace,
    members,
    built: ({ target }) => Effect.succeed([target]),
    report: REPORT,
    tools: ['dotnet'],
    verbs: ['build', 'clean', 'test', 'pack'],
    shared: [PROPS, 'Directory.Packages.props'],
    unlocked: [],
    scratch: [],
    added: Consumer.use(({ wrote }) =>
        wrote({
            'src/Extra/Fixture.Extra.csproj': files['src/Core/Fixture.Core.csproj'],
            'src/Extra/Zero.cs': 'namespace Fixture.Extra;\n\npublic static class Zero\n{\n    public static int Value() => 0;\n}\n',
            'src/App/Fixture.App.csproj': files['src/App/Fixture.App.csproj'].replace('</ItemGroup>', '    <ProjectReference Include="../Extra/Fixture.Extra.csproj" />\n    </ItemGroup>'),
        }),
    ),
    published: true,
    refused: { 'src/Broken/Fixture.Broken.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n' },
    manifests: (next) => ({
        'src/Core/Fixture.Core.csproj': files['src/Core/Fixture.Core.csproj'].replaceAll('0.1.0', next),
        'src/Middle/Fixture.Middle.csproj': files['src/Middle/Fixture.Middle.csproj'].replaceAll('0.1.0', next),
    }),
    edits: {
        verbs: ['build'],
        dependent: APP,
        upstream: (mark) => ({ [SOURCES.core]: files[SOURCES.core].replace('One() => 1;', `One() => 1;\n\n    public static int Probe_${mark}() => 7;`) }),
        outside: (mark) => ({ [SOURCES.tests]: `${files[SOURCES.tests]}// ${mark}\n`, 'src/App/notes.md': `${mark}\n` }),
        tested: TESTS,
        failing: { [SOURCES.tests]: files[SOURCES.tests].replace('Assert.Equal(1,', 'Assert.Equal(2,') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'gives a warm graph the targets a shared props edit changes for every project',
        Effect.fnUntraced(function* () {
            const { read, wrote } = yield* Consumer;
            yield* read;
            yield* wrote({ [PROPS]: files[PROPS].replace('<Nullable>enable</Nullable>', '<Nullable>enable</Nullable>\n        <IsPackable>false</IsPackable>') });
            const [{ nodes }, stated] = yield* Effect.all([read, members]);
            yield* wrote({ [PROPS]: files[PROPS] });
            assert.isEmpty(
                stated.filter(({ released }) => released),
                'dotnet msbuild <project> -getProperty:IsPackable',
            );
            assert.deepStrictEqual(
                Object.values(nodes).filter(({ data }) => 'pack' in (data.targets ?? {})),
                [],
                'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should rerun the analyzer when an evaluation input changed"',
            );
        }),
        bound,
    );

    methods.effect(
        'declares as an output of a build every file the builds of all projects write',
        Effect.fnUntraced(function* () {
            const { nx, cleaned, planned, written, outputs } = yield* Consumer;
            yield* cleaned;
            const { tasks } = yield* planned('run-many', '--targets=build');
            const [run, created] = yield* written(nx('run-many', '--targets=build', '--skip-nx-cache', '--output-style=static'));
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            const declared = yield* Effect.forEach(
                Object.keys(tasks).filter((id) => id.includes(':build--')),
                outputs,
            );
            assert.deepStrictEqual(
                undeclared(
                    created,
                    declared.flatMap(({ outputPaths }) => outputPaths),
                ),
                [],
                'nrwl/nx packages/dotnet/analyzer/Utilities/TargetBuilder.Build.cs: intermediate and output folders are declared outputs; README.md PRINCIPLES "restores an intermediate folder only with the outputs built from it"',
            );
        }),
        bound,
    );

    methods.effect.each(['--batch', '--no-batch'])(
        'builds every project with %s',
        Effect.fnUntraced(function* (flag) {
            const [{ nx, cleaned, absent }, stated] = yield* Effect.all([Consumer, members]);
            yield* cleaned;
            const run = yield* nx('run-many', '--targets=build', '--skip-nx-cache', flag, '--output-style=static');
            assert.strictEqual(run.code, 0, `nrwl/nx e2e/gradle/src/gradle.test.ts batch runs: ${run.stdout}${run.stderr}`);
            assert.deepStrictEqual(yield* absent(stated.map(({ target }) => target)), [], 'dotnet msbuild <project> -getProperty:TargetPath');
        }),
        bound,
    );

    methods.effect(
        'fails in one batch only the project that does not compile, and prints the compiler error',
        Effect.fnUntraced(function* () {
            const [{ nx, wrote }, broken, sound] = yield* Effect.all([Consumer, member(TESTS), member(APP)]);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests].replace('Fixture.Core.Numbers.One()', 'AbsentSymbol.One()') });
            const run = yield* nx('run-many', '--targets=build', '--skip-nx-cache', '--batch', '--nx-bail=false', '--output-style=static');
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests] });
            const [printed, source] = [`${run.stdout}${run.stderr}`, 'nrwl/nx packages/gradle/src/executors/gradle/gradle-batch.impl.spec.ts: a batch reports one result per task'];
            const failed = printed.slice(printed.indexOf('Failed tasks:'));
            assert.notStrictEqual(run.code, 0, source);
            assert.include(printed, 'AbsentSymbol', source);
            assert.include(failed, `${broken.name}:build`, source);
            assert.notInclude(failed, `${sound.name}:build`, source);
        }),
        bound,
    );

    methods.effect(
        'packs a packable project into the package file NuGet names from its id and version',
        Effect.fnUntraced(function* () {
            const [{ nx, written }, { name, packageId, version }] = yield* Effect.all([Consumer, member(CORE)]);
            const [packed, created] = yield* written(nx('pack', name, '--skip-nx-cache', '--output-style=static'));
            assert.strictEqual(packed.code, 0, `${packed.stdout}${packed.stderr}`);
            assert.isNotEmpty(
                created.filter((file) => file.endsWith(`/${packageId}.${version}.nupkg`)),
                `nrwl/nx e2e/dotnet/src/dotnet.test.ts "should create NuGet packages for libraries"; dotnet msbuild <project> -getProperty:PackageId -getProperty:Version: ${created.join(', ')}`,
            );
        }),
        bound,
    );
});

it.layer(
    Layer.provideMerge(
        Consumer.layer({
            resolved: import.meta.url,
            settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
            ignored: ['bin', 'obj'],
            files: {
                'src/Json/Newtonsoft.Json.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <TargetFramework>net10.0</TargetFramework>\n        <Version>0.0.1</Version>\n    </PropertyGroup>\n</Project>\n',
                'src/Json/Empty.cs': 'namespace Newtonsoft.Json;\n',
            },
        }),
        Layer.provideMerge(Packed.layer(plugin.name), layer),
    ),
    options,
)((methods) => {
    methods.effect(
        'reads the current version of a package from the source dotnet searches',
        Effect.fnUntraced(function* () {
            const { nx, called } = yield* Consumer;
            const found = yield* called('dotnet', ['package', 'search', 'Newtonsoft.Json', '--exact-match', '--format', 'json']);
            const { searchResult } = yield* Effect.orDie(Schema.decodeEffect(Found)(found.stdout));
            const stable = searchResult.flatMap(({ packages }) => packages).filter(({ version }) => !version.includes('-'));
            const latest = yield* Effect.orDie(Effect.fromOption(Array.last(stable)));
            const shown = yield* nx('release', 'version', 'patch', '--dry-run');
            assert.include(`${shown.stdout}${shown.stderr}`, `Resolved the current version as ${latest.version} from the remote registry`, 'dotnet package search <id> --exact-match --format json; nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts "registry"');
        }),
        bound,
    );
});
