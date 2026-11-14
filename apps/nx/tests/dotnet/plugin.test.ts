import { dirname, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { createProjectGraphAsync, workspaceRoot } from '@nx/devkit';
import plugin from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, exited, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, FileSystem, Layer, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'dotnet msbuild <project> -getProperty -getItem:ProjectReference -getItem:Compile -getItem:None, each path in the Configuration the project evaluates with none named';
const SINGLE = 'README.md PRINCIPLES ".NET targets of a project run in the one `Configuration` MSBuild evaluates for it, and test and pack read what `build--dotnet` wrote (`--no-build`)"';
const [APP, CORE, TESTS, PROPS] = ['src/App', 'src/Core', 'tests/Core.Tests', 'Directory.Build.props'] as const;
const SOURCES = { core: 'src/Core/Numbers.cs', tests: 'tests/Core.Tests/NumbersTests.cs' } as const;
const PROPERTIES = ['MSBuildProjectName', 'Version', 'PackageId', 'IsPackable', 'IsTestProject', 'IsTestingPlatformApplication', 'TargetPath', 'Configuration', 'Configurations', 'PackageOutputPath'] as const;
const ANALYZER = relative(workspaceRoot, resolve(dirname(fileURLToPath(import.meta.resolve(`${plugin.name}/package.json`))), dirname(plugin.imports['#analyzer/project'])));
const VERSION = '        <Version>0.1.0</Version>\n';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'global.json': '{ "test": { "runner": "Microsoft.Testing.Platform" } }\n',
    'NuGet.config': '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n    <config>\n        <add key="defaultPushSource" value="https://api.nuget.org/v3/index.json" />\n    </config>\n</configuration>\n',
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
const Evaluated = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Record(Schema.Literals(PROPERTIES), Schema.String), Items: Schema.Struct({ ProjectReference: Schema.Array(Item), Compile: Schema.Array(Item), None: Schema.Array(Item) }) })).annotate({
    description: 'JSON dotnet msbuild <project> -getProperty -getItem prints: the properties and items MSBuild evaluates for one project',
});

const Hashed = Schema.fromJsonString(Schema.Struct({ files: Schema.Array(Schema.String) })).annotate({ description: 'JSON nx show target inputs prints: the workspace files Nx hashes for one task' });
const Manifest = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct({ imports: Schema.Record(Schema.String, Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)]), { space: 4 }).annotate({
    description: 'package.json of the installed plugin: the names its imports map resolves, and every other field as it stands',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (path: Path.Path, root: string, manifest: string, { Properties: stated, Items: { ProjectReference, Compile, None } }: typeof Evaluated.Type) => ({
    name: stated.MSBuildProjectName,
    version: stated.Version,
    packageId: stated.PackageId,
    root: path.dirname(manifest),
    manifest,
    released: stated.IsPackable === 'true',
    verbs: ['build', 'clean', ...(stated.IsTestProject === 'true' || stated.IsTestingPlatformApplication === 'true' ? ['test'] : []), ...(stated.IsPackable === 'true' ? ['pack'] : [])],
    target: path.relative(root, stated.TargetPath),
    configuration: stated.Configuration,
    others: stated.Configurations.split(';').filter((configuration) => configuration !== stated.Configuration),
    packed: path.join(path.dirname(manifest), stated.PackageOutputPath.replaceAll('\\', '/'), `${stated.PackageId}.${stated.Version}.nupkg`),
    sources: [manifest, ...[...Compile, ...None].map(({ FullPath }) => path.relative(root, FullPath)).filter((file) => !file.split(path.sep).includes('obj'))],
    dependencies: ProjectReference.map(({ FullPath }) => path.relative(root, path.dirname(FullPath))),
});

const evaluated = Effect.fnUntraced(function* (manifest: string) {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const read = (...selected: readonly string[]) => called('dotnet', ['msbuild', manifest, ...selected]);
    const { code, stdout } = yield* read(...PROPERTIES.map((property) => `-getProperty:${property}`), '-getItem:ProjectReference', '-getItem:Compile', '-getItem:None');
    if (Number(code) !== 0) {
        return [];
    }
    return [described(path, root, manifest, yield* Schema.decodeEffect(Evaluated)(stdout))];
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

const sources = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { nodes } = yield* Effect.promise(() => createProjectGraphAsync());
    const analyzer = yield* Effect.fromOption(Array.findFirst(Object.values(nodes), ({ data }) => data.root === ANALYZER));
    const { checked } = yield* exited(ChildProcess.make('nx', ['show', 'target', 'inputs', `${analyzer.name}:build`, '--json'], { cwd: workspaceRoot, stdin: 'ignore', extendEnv: true, env: { NX_TUI: 'false' } }));
    const { files: hashed } = yield* Effect.flatMap(checked, Schema.decodeEffect(Hashed));
    const texts = yield* Effect.forEach(hashed, (file) => fs.readFileString(path.join(workspaceRoot, file)), { concurrency: 'unbounded' });
    return Record.fromIterableWith(Array.zip(hashed, texts), ([file, text]) => [file, text]);
}).pipe(Effect.orDie);

const languages = Effect.map(
    Consumer.use(({ read }) => read),
    ({ nodes }) => Array.difference(Array.dedupe(Object.values(nodes).flatMap(({ data }) => data.metadata?.technologies ?? [])), ['dotnet']),
);

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
        outside: (mark) => ({ [SOURCES.tests]: `${files[SOURCES.tests]}// ${mark}\n`, 'src/notes.md': `${mark}\n` }),
        tested: TESTS,
        failing: { [SOURCES.tests]: files[SOURCES.tests].replace('Assert.Equal(1,', 'Assert.Equal(2,') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    const unpacked = 'learn.microsoft.com dotnet pack --no-build "Doesn\'t build the project before packing. It also implicitly sets the --no-restore flag"; dotnet test --no-build "Doesn\'t build the test project before running it"';

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
        'plans one build of each project for check, test, build, and pack, which every check, test, and pack task of the project waits on',
        Effect.fnUntraced(function* () {
            const [{ planned, read }, stated] = yield* Effect.all([Consumer, members]);
            const [{ tasks, dependencies }, { nodes }] = yield* Effect.all([planned('run-many', '--targets=check,test,build,pack'), read]);
            const waits = Record.map(dependencies, Array.map(Struct.get('id')));
            const reading = Record.filter(tasks, ({ target }) => ['check--dotnet-build', 'test--dotnet', 'pack--dotnet'].includes(target.target));
            const commands = Object.values(nodes).flatMap(({ data }) => [data.targets?.['test--dotnet']?.options.commands?.at(-1), data.targets?.['pack--dotnet']?.options.command?.join(' ')]);
            assert.sameMembers(
                Record.keys(tasks).filter((id) => id.includes(':build--')),
                stated.map(({ name }) => `${name}:build--dotnet`),
                SINGLE,
            );
            assert.isNotEmpty(Record.keys(reading), SINGLE);
            assert.deepStrictEqual(Record.keys(Record.filter(reading, ({ target }, id) => !(waits[id] ?? []).includes(`${target.project}:build--dotnet`))), [], SINGLE);
            assert.isNotEmpty(commands.filter(Predicate.isString), unpacked);
            assert.deepStrictEqual(
                commands.filter(Predicate.isString).filter((command) => !command.includes('--no-build')),
                [],
                unpacked,
            );
        }),
        bound,
    );

    methods.effect(
        'refuses to pack a project nothing built, and packs a built one into the file NuGet names from its id and version without writing an assembly',
        Effect.fnUntraced(function* () {
            const [{ nx, called, cleaned, wrote, written }, { name, packed }] = yield* Effect.all([Consumer, member(CORE)]);
            const pack = nx('run', `${name}:pack--dotnet`, '--skip-nx-cache', '--excludeTaskDependencies', '--output-style=static');
            yield* cleaned;
            const refused = yield* pack;
            assert.notStrictEqual(refused.code, 0, unpacked);
            assert.include(`${refused.stdout}${refused.stderr}`, 'NU5026', 'learn.microsoft.com nuget/reference/errors-and-warnings/nu5026 "The file to be packed was not found on disk"');
            const build = yield* nx('run', `${name}:build--dotnet`, '--output-style=static');
            assert.strictEqual(build.code, 0, `${build.stdout}${build.stderr}`);
            yield* wrote({ '.nx/marker': '' });
            const [run, created] = yield* written(pack);
            const rewritten = yield* called('find', ['.', '-name', '*.dll', '-newer', '.nx/marker', '-not', '-path', './node_modules/*']);
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            assert.include(created, packed, `nrwl/nx e2e/dotnet/src/dotnet.test.ts "should create NuGet packages for libraries"; dotnet msbuild <project> -getProperty:PackageOutputPath -getProperty:PackageId -getProperty:Version: ${created.join(', ')}`);
            assert.deepStrictEqual(rewritten.stdout.split('\n').filter(String.isNonEmpty), [], unpacked);
        }),
        bound,
    );

    methods.effect(
        'runs check, test, and pack of every project over the builds one build run wrote, compiling no assembly again',
        Effect.fnUntraced(function* () {
            const { nx, called, wrote } = yield* Consumer;
            const build = yield* nx('run-many', '--targets=build', '--output-style=static');
            assert.strictEqual(build.code, 0, `${build.stdout}${build.stderr}`);
            yield* wrote({ '.nx/marker': '' });
            const run = yield* nx('run-many', '--targets=check,test,pack', '--skip-nx-cache', '--output-style=static');
            const rewritten = yield* called('find', ['.', '-name', '*.dll', '-newer', '.nx/marker', '-not', '-path', './node_modules/*']);
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            assert.include(`${run.stdout}${run.stderr}`, 'All projects are up-to-date for restore.', 'NuGet restore no-op: a restore of a project whose packages the machine holds fetches nothing');
            assert.deepStrictEqual(rewritten.stdout.split('\n').filter(String.isNonEmpty), [], `${SINGLE}; learn.microsoft.com visualstudio/msbuild/incremental-builds: a target with outputs newer than its inputs is skipped`);
        }),
        bound,
    );

    methods.effect(
        'runs every .NET task of a project declaring more configurations in the one a run names, each reading the build of that configuration',
        Effect.fnUntraced(function* () {
            const [{ planned, nx, absent }, stated] = yield* Effect.all([Consumer, members]);
            const core = yield* member(CORE);
            const { tasks } = yield* planned('run-many', '--targets=check,test,build,pack', '--configuration=release');
            const run = yield* nx('run', `${core.name}:pack--dotnet:release`, '--output-style=static');
            assert.sameMembers(Array.dedupe(stated.flatMap(({ others }) => others)), ['Release'], 'Microsoft.NET.Sdk.props: Configurations defaults to Debug;Release and Configuration to Debug');
            assert.deepStrictEqual(
                Record.keys(Record.filter(tasks, ({ target }) => target.target.includes('--dotnet') && !target.target.includes('format'))).filter((id) => !id.endsWith(':release')),
                [],
                'nrwl/nx packages/nx/src/tasks-runner/create-task-graph.ts resolveConfiguration: a task runs in the configuration the command names where its target declares it',
            );
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            assert.deepStrictEqual(yield* absent([core.packed.replace(`/${core.configuration}/`, '/Release/'), core.target.replace(`/${core.configuration}/`, '/Release/')]), [], 'dotnet pack --no-build --configuration Release packs bin/Release of the build dotnet build --configuration Release wrote');
        }),
        bound,
    );
});

it.layer(
    workspace({ 'NuGet.config': '<?xml version="1.0" encoding="utf-8"?>\n<configuration />\n', [PROPS]: files[PROPS].replace('<Nullable>enable</Nullable>', '<Nullable>enable</Nullable>\n        <Configuration>Release</Configuration>\n        <Configurations>Release</Configurations>') }),
    options,
)((methods) => {
    methods.effect(
        'keeps the release tag, pack, and attach of a packable project and gives it no publish target where NuGet resolves no default push source',
        Effect.fnUntraced(function* () {
            const [{ read }, { name }] = yield* Effect.all([Consumer, member(CORE)]);
            const { tags = [], targets = {} } = (yield* read).nodes[name]?.data ?? {};
            assert.include(tags, 'release', 'README.md PRINCIPLES "Release projects are versioned, packed, and attached, and the switch their registry reads alone gives one a publish target"');
            assert.containsAllKeys(targets, ['pack--dotnet', 'attach--gh'], 'README.md PRINCIPLES "Release projects are versioned, packed, and attached"');
            assert.notProperty(targets, 'nx-release-publish', 'learn.microsoft.com dotnet nuget push --source "This option is required unless the DefaultPushSource config value is set in the NuGet config file"');
        }),
        bound,
    );

    methods.effect(
        'plans the same tasks with and without -c release for projects declaring one configuration, and builds each in it',
        Effect.fnUntraced(function* () {
            const [{ planned, nx, absent }, stated] = yield* Effect.all([Consumer, members]);
            const [plain, named] = yield* Effect.all([planned('run-many', '--targets=check,test,build,pack'), planned('run-many', '--targets=check,test,build,pack', '--configuration=release')]);
            const run = yield* nx('run-many', '--targets=build,pack', '--configuration=release', '--output-style=static');
            assert.deepStrictEqual(Array.dedupe(stated.map(({ configuration, others }) => [configuration, ...others].join(';'))), ['Release'], 'dotnet msbuild <project> -getProperty:Configuration -getProperty:Configurations under a Directory.Build.props stating both as Release');
            assert.deepStrictEqual(
                [...Array.difference(Record.keys(plain.tasks), Record.keys(named.tasks)), ...Array.difference(Record.keys(named.tasks), Record.keys(plain.tasks))],
                [],
                `${SINGLE}; nrwl/nx packages/nx/src/tasks-runner/create-task-graph.ts resolveConfiguration: a target declaring no such configuration runs without one`,
            );
            assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
            assert.deepStrictEqual(yield* absent(stated.flatMap(({ target, packed, released }) => [target, ...(released ? [packed] : [])])), [], REPORT);
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

it.layer(
    Layer.unwrap(Effect.map(sources, (read) => Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, ignored: ['.artifacts', '.cache', 'dist'], files: read }))).pipe(Layer.provideMerge(Packed.layer(plugin.name)), Layer.provideMerge(layer)),
    options,
)((methods) => {
    methods.effect(
        'builds its analyzer from the project its source manifest names, and reads the graph a changed analyzer source reports',
        Effect.fnUntraced(function* () {
            const { root, read, nx, called, text, wrote } = yield* Consumer;
            const [installed, source] = [`node_modules/${plugin.name}`, `${ANALYZER}/Workspace.cs`];
            const manifest = yield* Effect.orDie(Effect.flatMap(text(`${installed}/package.json`), Schema.decodeEffect(Manifest)));
            yield* wrote({ [`${installed}/package.json`]: yield* Effect.orDie(Schema.encodeEffect(Manifest)({ ...manifest, imports: { ...manifest.imports, '#analyzer/project': plugin.imports['#analyzer/project'] } })) });
            yield* called('rm', ['-rf', `${installed}/analyzer`]);
            yield* called('ln', ['-s', `${root}/${ANALYZER}`, `${installed}/analyzer`]);
            assert.deepStrictEqual((yield* read).failures, [], 'learn.microsoft.com dotnet run "The dotnet run command builds the project before running it"');
            assert.deepStrictEqual(yield* languages, ['C#'], 'dotnet msbuild <project> -getProperty:Language');
            yield* wrote({ [source]: (yield* text(source)).replace('outer.GetPropertyValue("Language")', '"Probe"') });
            const listed = yield* Effect.all([nx('show', 'projects', '--json'), nx('show', 'projects', '--json')], { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                [listed.map(({ code }) => code), yield* languages],
                [[0, 0], ['Probe']],
                'learn.microsoft.com dotnet run "The dotnet run command builds the project before running it"; nrwl/nx packages/nx/src/project-graph/project-graph.ts: with the daemon off a FileLock on project-graph.lock makes a second process wait for the graph construction of the first, so two reads never build the analyzer at once',
            );
        }),
        bound,
    );

    methods.effect(
        'fails a graph read with the compiler error of an analyzer source that does not compile, and reads the graph again once it does',
        Effect.fnUntraced(function* () {
            const { read, text, wrote } = yield* Consumer;
            const [source, sound] = [`${ANALYZER}/Program.cs`, yield* text(`${ANALYZER}/Program.cs`)];
            yield* wrote({ [source]: `${sound}}\n` });
            const { failures } = yield* read;
            yield* wrote({ [source]: sound });
            const said = failures.flatMap(Struct.get('files')).map(Struct.get('message'));
            assert.isNotEmpty(
                said.filter((message) => message.includes('error CS1022')),
                `learn.microsoft.com dotnet/csharp/language-reference/compiler-messages CS1022 "Type or namespace definition, or end-of-file expected"; nodejs.org child_process: a child that exits before reading its stdin raises EPIPE on the stream, which no reader of the exit code needs: ${said.join('\n')}`,
            );
            assert.deepStrictEqual((yield* read).failures, [], 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should not cache a cancelled run": no failed read is stored');
        }),
        bound,
    );

    methods.effect(
        'reads the same graph after clean removed the build of its analyzer',
        Effect.fnUntraced(function* () {
            const { read, nx, absent } = yield* Consumer;
            const before = yield* read;
            const named = Object.values(before.nodes).flatMap(({ name, data }) => (data.root === ANALYZER ? [name] : []));
            const cleaned = yield* nx('run-many', '--targets=clean', `--projects=${named.join(',')}`, '--output-style=static');
            assert.strictEqual(cleaned.code, 0, `${cleaned.stdout}${cleaned.stderr}`);
            assert.deepStrictEqual(yield* absent([`${ANALYZER}/dist`]), [`${ANALYZER}/dist`], 'README.md TASKS "clean removes the outputs a project\'s targets declare and runs its tool\'s clean"');
            const after = yield* read;
            assert.deepStrictEqual(after.failures, [], 'dotnet run --project <project> builds the project before running it');
            assert.deepStrictEqual(
                Array.dedupe([...Record.keys(before.nodes), ...Record.keys(after.nodes)]).filter((project) => JSON.stringify(before.nodes[project]) !== JSON.stringify(after.nodes[project])),
                [],
                'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesFunction: a plugin gives one result for one set of configuration files',
            );
        }),
        bound,
    );
});
