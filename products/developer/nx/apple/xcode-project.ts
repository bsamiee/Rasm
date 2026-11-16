import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, identity, Option, Record, Result, Schema, Struct } from 'effect';
import { APPCAST, ATTACH, INPUTS, NOTARY, type Options, type Part, PLUGIN, sequenced, spawned, tokened, type Verbs } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Runs = readonly (readonly [string, ...string[]])[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCATED = ['INFOPLIST_FILE', 'CODE_SIGN_ENTITLEMENTS'] as const;
const LINKED: ReadonlySet<string | undefined> = new Set(['mh_execute', 'mh_bundle', 'mh_dylib']);

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(Schema.Struct({ name: Schema.String, containers: Schema.Array(Schema.String), plans: Schema.Array(Schema.String), tests: Schema.Boolean, launch: Schema.optionalKey(Schema.String) })),
    configurations: Schema.Array(Schema.String),
    inputs: Schema.Array(Schema.String),
    projects: Schema.Array(Schema.String),
    buildFiles: Schema.Array(Schema.String),
});
const Analyzed = Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Record(Schema.String, Schema.String) }));
const Evaluated = Schema.fromJsonString(
    Schema.Array(
        Schema.Struct({
            action: Schema.Literals(['build', 'archive', 'test']),
            target: Schema.String,
            buildSettings: Schema.Struct({
                CONFIGURATION: Schema.String,
                SRCROOT: Schema.String,
                BUILD_ROOT: Schema.String,
                TARGET_BUILD_DIR: Schema.String,
                EXECUTABLE_PATH: Schema.optionalKey(Schema.String),
                MACH_O_TYPE: Schema.optionalKey(Schema.String),
                SKIP_INSTALL: Schema.Literals(['NO', 'YES']).transform([false, true]),
                INSTALL_PATH: Schema.optionalKey(Schema.String),
                WRAPPER_NAME: Schema.OptionFromOptionalKey(Schema.String),
                INFOPLIST_FILE: Schema.optionalKey(Schema.String),
                CODE_SIGN_ENTITLEMENTS: Schema.optionalKey(Schema.String),
            }),
        }),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = (options: typeof Options.Type, built: string) => {
    const workspace = extname(built) === '.xcworkspace';
    return {
        lock: join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved'),
        flags: [workspace ? '-workspace' : '-project', built, ...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : [])],
    };
};

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, options: typeof Options.Type, container: typeof Container.Type, built: string) {
    const fs = yield* FileSystem.FileSystem;
    const { flags } = invocation(options, built);
    const read = ({ name, tests }: (typeof container.schemes)[number], selected: readonly string[]) =>
        Effect.flatMap(spawned(['xcodebuild', 'build', 'archive', ...(tests ? ['test'] : []), '-showBuildSettings', '-json', '-IDEPackageEnablePrebuilts=NO', ...flags, '-scheme', name, ...selected], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated));
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0);
    const archived = Array.zip(schemes, yield* Effect.forEach(schemes, (scheme) => read(scheme, [])));
    const others = yield* Effect.forEach(
        archived.flatMap(([scheme, listed]) =>
            Array.difference(
                container.configurations,
                listed.map(({ buildSettings }) => buildSettings.CONFIGURATION),
            ).map((configuration) => [scheme, configuration] as const),
        ),
        ([scheme, configuration]) => read(scheme, ['-configuration', configuration]),
    );
    const settings = [...archived.flatMap(([, listed]) => listed), ...others.flat()];
    const real = yield* fs.realPath(workspaceRoot);
    const [resolved, missing] = yield* Effect.partition(
        Array.getSomes(
            settings
                .flatMap(({ target: owner, buildSettings }) => LOCATED.map((setting) => ({ owner, setting, root: buildSettings.SRCROOT, value: Option.fromUndefinedOr(buildSettings[setting]) })))
                .map(({ value, ...located }) => Option.map(value, (held) => ({ ...located, value: held, at: resolve(located.root, held) }))),
        ),
        (setting) => Effect.mapBoth(fs.realPath(setting.at), { onFailure: (cause) => new Cause.UnknownError(cause, `target ${setting.owner} ${setting.setting}: ${setting.value} does not exist`), onSuccess: (at) => relative(real, at) }),
    );
    const entries = archived.flatMap(([{ name: scheme }, listed]) => listed.map(({ action, buildSettings }) => ({ scheme, action, ...buildSettings })));
    const installed = Array.getSomes(
        entries.map(({ INSTALL_PATH, ...entry }) =>
            Option.map(
                Option.filter(Option.fromUndefinedOr(INSTALL_PATH), () => entry.action === 'archive' && !entry.SKIP_INSTALL),
                (place) => ({ ...entry, place }),
            ),
        ),
    );
    const archives = Record.collect(Array.groupBy(installed, Struct.get('scheme')), (scheme, group) => ({
        scheme,
        configuration: Array.headNonEmpty(group).CONFIGURATION,
        places: Array.dedupe(Array.map(group, Struct.get('place'))),
        bundles: Array.getSomes(Array.map(group, Struct.get('WRAPPER_NAME'))),
    }));
    const failures = [...missing.map(({ message }) => message), ...archives.filter(({ places }) => places.length > 1).map(({ scheme, places }) => `scheme ${scheme} installs into ${places.join(' and ')}, and one xcodebuild install takes one DSTROOT`)];
    const instrumented = entries.filter(({ action, MACH_O_TYPE }) => action === 'test' && LINKED.has(MACH_O_TYPE));
    const placed = Array.getSomes(
        instrumented.map(({ BUILD_ROOT, TARGET_BUILD_DIR, EXECUTABLE_PATH }) => Option.map(Option.fromUndefinedOr(EXECUTABLE_PATH), (at) => ({ derived: dirname(dirname(BUILD_ROOT)), profile: join(dirname(BUILD_ROOT), 'ProfileData', '*', 'Coverage.profdata'), object: join(TARGET_BUILD_DIR, at) }))),
    );
    const coverage = {
        profiles: Array.dedupe(placed.map(({ derived, profile }) => relative(derived, profile))),
        objects: Array.dedupe(placed.map(({ derived, object }) => relative(derived, object))),
    };
    return failures.length === 0 ? { inputs: Array.dedupe([...container.inputs, ...resolved]), archives, coverage } : yield* new Cause.UnknownError(failures, [container.path, ...Array.dedupe(failures)].join('\n  '));
});

const xcodeParts = (
    options: typeof Options.Type,
    file: string,
    container: typeof Container.Type,
    inputs: readonly string[],
    { archives, coverage }: Pick<Effect.Success<ReturnType<typeof evaluated>>, 'archives' | 'coverage'>,
    built: string,
    destinations: (roots: readonly string[]) => readonly string[],
    packaged: (at: string) => boolean,
): Part => {
    const { lock, flags } = invocation(options, built);
    const name = basename(container.path, extname(container.path));
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-onlyUsePackageVersionsFromResolvedFile', '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map((scheme) => ({ ...scheme, packages: scheme.containers.every(packaged) }));
    const output = (scheme: string) => Option.map(Option.fromUndefinedOr(options.outputPath), (at) => join(at, scheme));
    const archived = Array.getSomes(
        archives.map(({ scheme, configuration }) => Option.map(output(scheme), (folder) => ({ configuration, folder, archive: join(folder, `${scheme}.xcarchive`), run: [...invoke('archive', scheme, ['-archivePath', join(folder, `${scheme}.xcarchive`)]), '-configuration', configuration] }))),
    );
    const referenced = container.buildFiles
        .filter((at) => relative(dirname(container.path), at).startsWith('..'))
        .flatMap((at) => tokened(at))
        .flatMap((input) => [input, join(input, '**/*')]);
    const read = Array.dedupe([join('{projectRoot}', '**/*'), ...inputs, ...referenced, ...tokened(lock), ...INPUTS]);
    const released = extname(dirname(file)) === '.xcodeproj' && Array.isReadonlyArrayNonEmpty(archived);
    const runs = schemes
        .filter(({ tests, packages }) => tests && !packages)
        .flatMap((scheme) =>
            (scheme.plans.length === 0 ? [Option.none<string>()] : scheme.plans.map(Option.some)).map((plan) =>
                invoke(
                    'test',
                    scheme.name,
                    Option.toArray(plan).flatMap((testPlan) => ['-testPlan', testPlan]),
                ),
            ),
        );
    const derived = Option.orElse(Option.fromUndefinedOr(options.derivedDataPath), () => Option.map(Option.fromUndefinedOr(options.outputPath), (at) => join(at, 'DerivedData')));
    const report = Option.all({
        profiles: Option.liftPredicate(coverage.profiles, Array.isReadonlyArrayNonEmpty),
        objects: Option.liftPredicate(coverage.objects, Array.isReadonlyArrayNonEmpty),
        file: Option.map(Option.fromUndefinedOr(options.coveragePath), (folder) => join(folder, 'lcov.info')),
        folder: derived,
    }).pipe(Option.map((held) => ({ profiles: Array.map(held.profiles, (at) => join(held.folder, at)), objects: Array.map(held.objects, (at) => join(held.folder, at)), file: held.file })));
    const covered = Option.toArray(Option.map(report, ({ file: at }) => at));
    const builds = (configuration: Option.Option<string>): Runs => {
        const [archiving, building] = Array.partition(
            schemes,
            Result.liftPredicate((scheme: (typeof schemes)[number]) => archives.some((archive) => archive.scheme === scheme.name && Option.contains(configuration, archive.configuration)), identity),
        );
        return [
            ...archiving.map((scheme) => invoke('archive', scheme.name, Option.match(output(scheme.name), { onNone: () => [], onSome: (at) => ['-archivePath', join(at, `${scheme.name}.xcarchive`)] }))),
            ...Array.flatMap(building, (scheme) => (scheme.packages ? destinations(scheme.containers).map((platform) => ['-destination', `generic/platform=${platform}`]) : [[]]).map((destination) => invoke('build', scheme.name, destination))),
        ];
    };
    const installs = archives.map(({ scheme, configuration, places }) => invoke('install', scheme, ['-configuration', configuration, `DSTROOT=${Array.headNonEmpty(places)}`, 'INSTALL_PATH=/']));
    const launches = Array.dedupe(Array.getSomes(schemes.map(({ launch }) => Option.fromUndefinedOr(launch))));
    const modes = (planned: (configuration: Option.Option<string>) => Runs): Partial<TargetConfiguration> => ({
        configurations: Record.fromIterableWith(container.configurations, (configuration) => [configuration.toLowerCase(), { runs: planned(Option.some(configuration)).map((run) => [...run, '-configuration', configuration]) }]),
        ...(launches.length === 1 ? Record.fromIterableWith(launches, (launch) => ['defaultConfiguration', launch.toLowerCase()] as const) : {}),
    });
    const testing = modes(() => runs);
    const profiled = {
        runs: runs.map((run) => [...run, '-enableCodeCoverage', 'YES', ...Option.toArray(derived).flatMap((folder) => ['-derivedDataPath', folder])]),
        outputs: covered.map((at) => join('{workspaceRoot}', at)),
        ...Option.getOrUndefined(Option.map(report, (held) => ({ report: held }))),
    };
    const tools: Verbs = {
        ...(schemes.length > 0 && {
            build: {
                xcodebuild: sequenced(
                    builds(Option.none()),
                    { technologies: ['xcode'], description: 'Builds each shared scheme with xcodebuild, archiving each scheme with installed products in its archive configuration', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } },
                    { cache: true, dependsOn: ['^build'], inputs: read, ...modes(builds) },
                ),
            },
            check: { xcodebuild: { dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Type-checks each shared scheme with xcodebuild build', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } } } },
        }),
        ...(Array.isReadonlyArrayNonEmpty(archived) && {
            pack: {
                ditto: {
                    executor: `${PLUGIN}:package`,
                    cache: true,
                    inputs: [...read, { env: NOTARY.keyId }],
                    options: { exports: archived.map(({ archive, folder, run }) => ({ archive, folder, run })) },
                    outputs: archived.flatMap(({ folder }) => ['.dmg', '.zip'].map((extension) => (relative(dirname(container.path), folder).startsWith('..') ? join('{workspaceRoot}', folder, `*${extension}`) : join('{projectRoot}', relative(dirname(container.path), folder), `*${extension}`)))),
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
                        description: "Builds each shared scheme whose targets install, under -configuration set to the scheme's archive configuration, and installs the products at INSTALL_PATH on this Mac with xcodebuild's install action",
                        help: { command: 'xcodebuild -help', example: { args: ['-jobs', '4'] } },
                    },
                    { dependsOn: ['^build'], inputs: read, options: { applications: archives.flatMap(({ places, bundles }) => bundles.map((bundle) => join(Array.headNonEmpty(places), bundle))) } },
                ),
            },
        }),
        ...(released && { attach: { gh: ATTACH } }),
    };
    return {
        root: dirname(container.path),
        name,
        file,
        technologies: ['xcode'],
        tools: {
            ...tools,
            ...(schemes.length > 0 && {
                clean: {
                    xcodebuild: {
                        executor: `${distribution.name}:clean`,
                        inputs: read,
                        options: {
                            commands: [...schemes.map((scheme) => invoke('clean', scheme.name, [])), ...[...archived.map(({ archive }) => archive), ...(runs.length > 0 ? Option.toArray(derived) : [])].map((at) => ['rm', '-rf', at])],
                            cwd: '.',
                            targets: Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat(),
                        },
                        metadata: { technologies: ['xcode'], description: 'Removes the build products of each shared scheme with xcodebuild clean, then each archive and the DerivedData folder of the coverage configuration', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } },
                    },
                },
            }),
        },
        ...(released && {
            published: {
                executor: `${PLUGIN}:release-publish`,
                dependsOn: ['pack--ditto', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*.dmg' }, { externalDependencies: [] }],
                options: { exports: archived.map(({ archive, folder }) => ({ archive, folder })) },
                metadata: {
                    technologies: ['xcode'],
                    description: `Signs the Sparkle feed of each disk image pack--ditto wrote with generate_appcast under ${APPCAST.key} and uploads it to the R2 bucket ${APPCAST.bucket} names with cf r2 objects put, skipping where either is unset or no disk image exists`,
                    help: { command: 'generate_appcast --help', example: { options: { dryRun: true } } },
                },
            },
            release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(dirname(dirname(file)), file) } } },
        }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, xcodeParts };
