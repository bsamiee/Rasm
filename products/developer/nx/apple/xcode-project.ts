import { basename, dirname, extname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { EXECUTORS, type Verbs } from '@rasm/nx';
import { Array, Option, Record, Schema } from 'effect';
import type { Run } from '#/executors/run/executor';
import { RESOLVED } from '#/swift-package';
import { APPCAST, ATTACH, INPUTS, NOTARY, type Options, type Part, PLUGIN, sequenced, tokened } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            containers: Schema.Array(Schema.String),
            plans: Schema.Array(Schema.String),
            tests: Schema.Boolean,
            launch: Schema.optionalKey(Schema.String),
            archive: Schema.optionalKey(Schema.Struct({ configuration: Schema.String, bundles: Schema.NonEmptyArray(Schema.String) })),
        }),
    ),
    configurations: Schema.Array(Schema.String),
    inputs: Schema.Array(Schema.String),
    projects: Schema.Array(Schema.String),
    products: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    buildFiles: Schema.Array(Schema.String),
});
class Analyzed extends Schema.Opaque<Analyzed, unknown>()(Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Record(Schema.String, Schema.String) }))) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const xcodeParts = (options: Options, file: string, container: Analyzed['containers'][number], name: string, inputs: readonly string[], built: string, packaged: (at: string) => Option.Option<string>, data: string): Part => {
    const workspace = extname(built) === '.xcworkspace';
    const lock = join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved');
    const flags = [workspace ? '-workspace' : '-project', built, '-skipMacroValidation', '-skipPackagePluginValidation'];
    const cloned = join(data, 'xcode-packages', container.path);
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-onlyUsePackageVersionsFromResolvedFile', '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0);
    const listing = extname(container.path) === '.xcworkspace';
    const root = listing ? container.path : dirname(container.path);
    const locals = Array.dedupe(Array.getSomes(container.projects.map(packaged)));
    const seed = Array.head(Array.getSomes(container.projects.map((at) => Option.as(packaged(at), join(at, RESOLVED)))));
    const archives = Array.getSomes(schemes.map(({ name: scheme, archive }) => Option.map(Option.fromUndefinedOr(archive), (held) => ({ scheme, ...held }))));
    const archived = Array.getSomes(
        archives.map(({ scheme, configuration }) =>
            Option.map(Option.fromUndefinedOr(options.outputPath), (at) => ({ folder: join(at, scheme), archive: join(at, scheme, `${scheme}.xcarchive`), run: invoke('archive', scheme, ['-archivePath', join(at, scheme, `${scheme}.xcarchive`), '-configuration', configuration]) })),
        ),
    );
    const referenced = container.buildFiles.filter((at) => relative(root, at).startsWith('..')).flatMap((at) => tokened(at).flatMap((input) => [input, join(input, '**/*'), ...[at, join(at, '**/*')].map((output) => ({ dependentTasksOutputFiles: output }))]));
    const read = Array.dedupe(['{projectRoot}/**/*', ...inputs, ...referenced, ...[lock, ...Option.toArray(seed)].flatMap(tokened), ...(workspace ? tokened(join(built, 'contents.xcworkspacedata')) : []), { runtime: 'xcodebuild -version' }, ...INPUTS]);
    const released = extname(dirname(file)) === '.xcodeproj' && Array.isReadonlyArrayNonEmpty(archived);
    const runs = schemes.filter(({ tests }) => tests).flatMap((scheme) => (scheme.plans.length === 0 ? [[]] : scheme.plans.map((plan) => ['-testPlan', plan])).map((planned) => invoke('test', scheme.name, planned)));
    const derived = Option.orElse(Option.fromUndefinedOr(options.derivedDataPath), () => Option.map(Option.fromUndefinedOr(options.outputPath), (at) => join(at, 'DerivedData')));
    const report = Option.all({ file: Option.map(Option.fromUndefinedOr(options.coveragePath), (folder) => join(folder, 'lcov.info')), folder: derived }).pipe(
        Option.map(({ file: at, folder }) => ({ profiles: [join(folder, 'Build/ProfileData/*/Coverage.profdata')], results: [join(folder, 'Logs/Test/*.xcresult')], file: at })),
    );
    const covered = Option.toArray(Option.map(report, ({ file: at }) => at));
    const builds: Run['runs'] = schemes.map((scheme) => invoke('build', scheme.name, []));
    const installs = archives.map(({ scheme, configuration }) => invoke('install', scheme, ['-configuration', configuration, 'DSTROOT=$(LOCAL_APPS_DIR)', 'INSTALL_PATH=/']));
    const launches = Array.dedupe(Array.getSomes(schemes.map(({ launch }) => Option.fromUndefinedOr(launch))));
    const modes = (planned: Run['runs']): Partial<TargetConfiguration> => ({
        configurations: Record.fromIterableWith(container.configurations, (configuration) => [configuration.toLowerCase(), { runs: planned.map((run) => [...run, '-configuration', configuration]) }]),
        ...(launches.length === 1 ? Record.fromIterableWith(launches, (launch) => ['defaultConfiguration', launch.toLowerCase()] as const) : {}),
    });
    const testing = modes(runs);
    const profiled = {
        runs: runs.map((run) => [...run, '-enableCodeCoverage', 'YES', ...Option.toArray(derived).flatMap((folder) => ['-derivedDataPath', folder])]),
        outputs: covered.map((at) => join('{workspaceRoot}', at)),
        ...Option.getOrUndefined(Option.map(report, (held) => ({ report: held }))),
    };
    const tools: Verbs = {
        ...(schemes.length > 0 && {
            build: {
                xcodebuild: sequenced(
                    builds,
                    {
                        technologies: ['xcode'],
                        description: 'Builds each shared scheme with xcodebuild build for the destination xcodebuild picks on this Mac, as app builds do, so a package scheme reuses the modules app builds compiled, pack--ditto archiving the schemes with installed products',
                        help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                    },
                    { cache: true, parallelism: false, ...(!listing && { dependsOn: ['^build--*'] }), inputs: read, ...modes(builds) },
                ),
            },
            check: { xcodebuild: { dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Type-checks each shared scheme with xcodebuild build', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } } } },
            upgrade: {
                xcodebuild: sequenced(
                    [
                        ['rm', '-rf', join(cloned, 'workspace-state.json'), join(cloned, 'checkouts'), ...(Option.isNone(seed) ? [lock] : [])],
                        ...Option.toArray(Option.map(seed, (from) => ['ln', '-sf', relative(dirname(lock), from), lock] as const)),
                        ...schemes.slice(0, 1).map((scheme) => ['xcodebuild', '-resolvePackageDependencies', ...flags, '-clonedSourcePackagesDirPath', cloned, '-scheme', scheme.name] as const),
                    ],
                    {
                        technologies: ['xcode'],
                        description: `Removes the checkouts and state of ${name}'s upgrade package folder, keeping its git mirrors, links ${name}'s Package.resolved to the one of the local package it names, which upgrade--swift-package of that package moves, or removes it where ${name} names none, then resolves its package references with xcodebuild -resolvePackageDependencies, which keeps each pin their requirements allow and so resolves the versions the local package resolved, else the newest releases their requirements allow`,
                        help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                    },
                    { inputs: read, ...(locals.length > 0 && { dependsOn: [{ projects: locals, target: 'upgrade--swift-package', params: 'forward' }] }) },
                ),
            },
        }),
        ...(Array.isReadonlyArrayNonEmpty(archived) && {
            pack: {
                ditto: {
                    executor: `${PLUGIN}:package`,
                    cache: true,
                    parallelism: false,
                    dependsOn: ['^build--*'],
                    inputs: [...read, { env: NOTARY.keyId }],
                    options: { exports: archived.map(({ archive, folder, run }) => ({ archive, folder, run })) },
                    outputs: archived.flatMap(({ folder, archive }) => ['*.dmg', '*.zip', basename(archive)].map((entry) => (relative(root, folder).startsWith('..') ? join('{workspaceRoot}', folder, entry) : join('{projectRoot}', relative(root, folder), entry)))),
                    metadata: {
                        technologies: ['xcode'],
                        description: `Archives each app with installed products with xcodebuild archive, then zips it with ditto, or where ${NOTARY.key}, ${NOTARY.keyId}, and ${NOTARY.issuer} are set exports it under the Developer ID Application identity into a disk image that is signed, notarized, and stapled`,
                        help: { command: 'man ditto', example: { args: ['--norsrc'] } },
                    },
                },
            },
        }),
        ...(runs.length > 0 && {
            test: {
                xcodebuild: sequenced(
                    runs,
                    {
                        technologies: ['xcode'],
                        description: "Tests each test plan of a shared scheme with xcodebuild, under the coverage configuration with -enableCodeCoverage YES, exporting lcov from Xcode's Coverage.profdata with xcrun llvm-cov export where coveragePath is set",
                        help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } },
                    },
                    {
                        cache: true,
                        parallelism: false,
                        dependsOn: ['build--xcodebuild'],
                        inputs: read,
                        outputs: covered.map((_, index) => `{options.outputs.${index}}`),
                        ...testing,
                        configurations: { ...testing.configurations, coverage: profiled },
                    },
                ),
            },
        }),
        ...(installs.length > 0 && {
            install: {
                xcodebuild: sequenced(
                    installs,
                    {
                        technologies: ['xcode'],
                        description:
                            "Builds each shared scheme archiving an application that installs, under -configuration set to the scheme's archive configuration, and installs its applications into the LOCAL_APPS_DIR xcodebuild evaluates on this Mac with its install action, that folder as DSTROOT since the build system refuses / as one",
                        help: { command: 'xcodebuild -help', example: { args: ['-jobs', '4'] } },
                    },
                    { parallelism: false, dependsOn: ['^build--*'], inputs: read, options: { applications: Array.dedupe(archives.flatMap(({ bundles }) => bundles)) } },
                ),
            },
        }),
        ...(released && { attach: { gh: ATTACH } }),
    };
    return {
        root,
        name,
        file,
        technologies: ['xcode'],
        tools: {
            ...tools,
            ...(schemes.length > 0 && {
                clean: {
                    xcodebuild: {
                        executor: EXECUTORS.clean,
                        parallelism: false,
                        inputs: read,
                        options: {
                            commands: [...schemes.map((scheme) => invoke('clean', scheme.name, [])), ...[...archived.map(({ archive }) => archive), ...(runs.length > 0 ? Option.toArray(derived) : [])].map((at) => ['rm', '-rf', at])],
                            cwd: '.',
                            targets: Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat(),
                        },
                        configurations: {
                            cache: {
                                commands: Array.take(
                                    Array.filter(schemes, ({ containers }) => containers.some((at) => Option.isNone(packaged(at)))),
                                    1,
                                ).map((scheme) => ['sh', '-c', 'set -eo pipefail; root=$("$@" | jq -er ".[0].buildSettings.BUILD_ROOT"); rm -rf "$(dirname "$(dirname "$root")")"', 'sh', ...invoke('-showBuildSettings', scheme.name, ['-json'])]),
                            },
                        },
                        metadata: {
                            technologies: ['xcode'],
                            description: `Removes the build products of each shared scheme with xcodebuild clean, then each archive and the DerivedData folder of the coverage configuration, and under configuration cache removes the per-user DerivedData folder of ${name}, two folders above the BUILD_ROOT xcodebuild -showBuildSettings -json prints`,
                            help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                        },
                    },
                },
            }),
        },
        ...(released && {
            published: {
                executor: `${PLUGIN}:release-publish`,
                dependsOn: ['pack--ditto', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                options: { exports: archived.map(({ archive }) => ({ archive })) },
                metadata: {
                    technologies: ['xcode'],
                    description: `Signs the Sparkle feed of each disk image pack--ditto wrote with generate_appcast under ${APPCAST.key} and uploads it to the R2 bucket ${APPCAST.bucket} names with cf r2 objects put, skipping where either is unset or no disk image exists`,
                    help: { command: 'generate_appcast --help', example: { options: { dryRun: true } } },
                },
            },
            release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(root, file) } } },
        }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, xcodeParts };
