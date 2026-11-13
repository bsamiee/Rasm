import { basename, dirname, extname, join, relative, resolve } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, identity, Option, Record, Result, Schema, Struct } from 'effect';
import { INPUTS, PACKAGES, type Part, PLUGIN, type Settings, target, tokened } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Runs = readonly (readonly [string, ...string[]])[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCATED = ['INFOPLIST_FILE', 'CODE_SIGN_ENTITLEMENTS'] as const;

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
            action: Schema.Literals(['build', 'archive']).transform([false, true]),
            target: Schema.String,
            buildSettings: Schema.Struct({
                CONFIGURATION: Schema.String,
                SRCROOT: Schema.String,
                SKIP_INSTALL: Schema.Literals(['NO', 'YES']).transform([false, true]),
                INSTALL_PATH: Schema.String,
                WRAPPER_NAME: Schema.OptionFromOptionalKey(Schema.String),
                INFOPLIST_FILE: Schema.optionalKey(Schema.String),
                CODE_SIGN_ENTITLEMENTS: Schema.optionalKey(Schema.String),
            }),
        }),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = (options: Settings, built: string) => {
    const workspace = extname(built) === '.xcworkspace';
    return {
        lock: join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved'),
        flags: [workspace ? '-workspace' : '-project', built, ...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : [])],
    };
};

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, options: Settings, container: typeof Container.Type, built: string) {
    const fs = yield* FileSystem.FileSystem;
    const { flags } = invocation(options, built);
    const read = (scheme: string, selected: readonly string[]) => Effect.flatMap(printed('xcodebuild', ['build', 'archive', '-showBuildSettings', '-json', '-IDEPackageEnablePrebuilts=NO', ...flags, '-scheme', scheme, ...selected], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated));
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map(({ name }) => name);
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

        (setting) => Effect.mapBoth(fs.realPath(setting.at), { onFailure: () => ToolError.make({ subject: `target ${setting.owner} ${setting.setting}`, detail: `${setting.value} does not exist` }), onSuccess: (at) => relative(real, at) }),
    );
    const archives = archived.flatMap(([scheme, listed]) =>
        Array.match(
            listed.filter(({ action: archiving, buildSettings }) => archiving && !buildSettings.SKIP_INSTALL).map(({ buildSettings }) => buildSettings),
            {
                onEmpty: () => [],
                onNonEmpty: (installed) => [
                    {
                        scheme,
                        configuration: Array.headNonEmpty(installed).CONFIGURATION,
                        places: Array.dedupe(Array.map(installed, Struct.get('INSTALL_PATH'))),
                        bundles: Array.getSomes(Array.map(installed, Struct.get('WRAPPER_NAME'))),
                    },
                ],
            },
        ),
    );
    const failures = [...missing.map(({ message }) => message), ...archives.filter(({ places }) => places.length > 1).map(({ scheme, places }) => `scheme ${scheme} installs into ${places.join(' and ')}, and one xcodebuild install takes one DSTROOT`)];
    return failures.length === 0 ? { inputs: Array.dedupe([...container.inputs, ...resolved]), archives } : yield* ToolError.make({ subject: container.path, detail: Array.dedupe(failures).join('\n  ') });
});

const xcodeParts = (options: Settings, file: string, container: typeof Container.Type, inputs: readonly string[], archives: Effect.Success<ReturnType<typeof evaluated>>['archives'], built: string, destinations: (roots: readonly string[]) => readonly string[], packaged: (at: string) => boolean): Part => {
    const { lock, flags } = invocation(options, built);
    const name = basename(container.path, extname(container.path));
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-onlyUsePackageVersionsFromResolvedFile', '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map((scheme) => ({ ...scheme, packages: scheme.containers.every(packaged) }));
    const output = (scheme: string) => Option.map(options.placed('xcode', name), (at) => join(at, scheme));
    const archived = Array.getSomes(
        archives.map(({ scheme, configuration }) => Option.map(output(scheme), (folder) => ({ configuration, folder, archive: join(folder, `${scheme}.xcarchive`), run: [...invoke('archive', scheme, ['-archivePath', join(folder, `${scheme}.xcarchive`)]), '-configuration', configuration] }))),
    );
    const referenced = container.buildFiles
        .filter((at) => relative(dirname(container.path), at).startsWith('..'))
        .flatMap((at) => tokened(at))
        .flatMap((input) => [input, join(input, '**/*')]);
    const read = Array.dedupe([join('{projectRoot}', '**/*'), ...inputs, ...referenced, ...tokened(lock), ...INPUTS]);
    const released: Pick<Part, 'release' | 'packaged' | 'publish'> =
        extname(dirname(file)) === '.xcodeproj' && Array.isReadonlyArrayNonEmpty(archived)
            ? {
                  release: { version: { versionActions: `${PLUGIN}/release/xcode-version-actions`, versionActionsOptions: { manifest: relative(dirname(dirname(file)), file) } } },
                  packaged: ['pack--ditto'],
                  publish: {
                      executor: `${PLUGIN}:release-publish`,
                      options: { exports: archived.map(({ archive, folder }) => ({ archive, folder })) },
                      metadata: { technologies: ['xcode'], description: 'Signs the Sparkle feed of each disk image pack--ditto wrote with generate_appcast and uploads it to the R2 bucket SPARKLE_FEED_BUCKET names', help: { command: 'generate_appcast --help', example: { options: { dryRun: true } } } },
                  },
              }
            : {};
    const releasing = Array.match(archived, {
        onEmpty: () => ({}),
        onNonEmpty: (exported) => ({
            ditto: {
                executor: `${PLUGIN}:package`,
                cache: true,
                inputs: [...read, { env: 'ASC_API_KEY_ID' }],
                options: { exports: exported.map(({ archive, folder, run }) => ({ archive, folder, run })) },
                outputs: exported.flatMap(({ folder }) => PACKAGES.map((extension) => join('{workspaceRoot}', folder, `*${extension}`))),
                metadata: {
                    technologies: ['xcode'],
                    description: 'Archives each app with installed products, then zips it with ditto, or where ASC_API_KEY_P8, ASC_API_KEY_ID, and ASC_API_ISSUER_ID are set exports it under the Developer ID Application identity into a disk image that is signed, notarized, and stapled',
                    help: { command: 'man ditto', example: { args: ['--norsrc'] } },
                },
            } satisfies TargetConfiguration,
        }),
    });
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
    return {
        root: dirname(container.path),
        name,
        file,
        technologies: ['xcode'],
        clean: 'xcodebuild',
        ...released,
        members: {
            build: {
                ...(schemes.length === 0
                    ? {}
                    : {
                          xcodebuild: target(
                              builds(Option.none()),
                              { technologies: ['xcode'], description: 'Builds each shared scheme with xcodebuild, archiving each scheme with installed products in its archive configuration', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } },
                              { dependsOn: ['^build'], inputs: read, ...modes(builds) },
                          ),
                      }),
            },
            pack: releasing,
            clean:
                schemes.length === 0
                    ? {}
                    : {
                          xcodebuild: {
                              options: { commands: schemes.map((scheme) => invoke('clean', scheme.name, [])), cwd: '.' },
                              metadata: { description: 'Removes the build products of each shared scheme with xcodebuild clean', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } },
                          },
                      },
            check: schemes.length === 0 ? {} : { xcodebuild: { executor: 'nx:noop', dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Compiles each shared scheme as its type check' } } },
            test:
                runs.length === 0
                    ? {}
                    : {
                          xcodebuild: target(
                              runs,
                              { technologies: ['xcode'], description: 'Tests each test plan of a shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } } },
                              { cache: true, dependsOn: ['build--xcodebuild'], inputs: read, ...modes(() => runs) },
                          ),
                      },
            install:
                installs.length === 0
                    ? {}
                    : {
                          xcodebuild: target(
                              installs,
                              {
                                  technologies: ['xcode'],
                                  description: "Builds each shared scheme whose targets install, under -configuration set to the scheme's archive configuration, and installs the products at INSTALL_PATH on this Mac with xcodebuild's install action",
                                  help: { command: 'xcodebuild -help', example: { args: ['-jobs', '4'] } },
                              },
                              { dependsOn: ['^build'], options: { applications: archives.flatMap(({ places, bundles }) => bundles.map((bundle) => join(Array.headNonEmpty(places), bundle))) } },
                          ),
                      },
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, xcodeParts };
