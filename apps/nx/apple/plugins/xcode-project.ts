import type { TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, Struct } from 'effect';
import type { Options } from '#/plugins/project-graph';
import { AppleError, type Part, type Plugin, published, spawned, target, tokened } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Runs = readonly (readonly [string, ...string[]])[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEVELOPER_ID = { identity: 'Developer ID Application', method: 'developer-id' } as const;

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
            target: Schema.String,
            buildSettings: Schema.Struct({
                CONFIGURATION: Schema.String,
                CODE_SIGN_IDENTITY: Schema.optionalKey(Schema.String),
                ENABLE_HARDENED_RUNTIME: Schema.optionalKey(Schema.Literals(['NO', 'YES']).transform([false, true])),
                SRCROOT: Schema.String,
                SKIP_INSTALL: Schema.Literals(['NO', 'YES']).transform([false, true]),
                INSTALL_PATH: Schema.String,
                INFOPLIST_FILE: Schema.optionalKey(Schema.String),
                CODE_SIGN_ENTITLEMENTS: Schema.optionalKey(Schema.String),
            }),
        }),
    ),
);
const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));
const Request = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = (path: Path.Path, options: Options, built: string) => {
    const workspace = path.extname(built) === '.xcworkspace';
    return {
        lock: path.join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved'),
        flags: [workspace ? '-workspace' : '-project', built, ...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : []), ...(Predicate.isTruthy(isCI()) ? ['-disableAutomaticPackageResolution'] : [])],
    };
};

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, options: Options, container: typeof Container.Type, built: string) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const { flags } = invocation(path, options, built);
    const read = (scheme: string, selected: readonly string[]) => Effect.flatMap(spawned('xcodebuild', ['archive', '-showBuildSettings', '-json', ...flags, '-scheme', scheme, ...selected], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated));
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
        settings.flatMap(({ target: owner, buildSettings }) => Object.entries(Struct.pick(buildSettings, ['INFOPLIST_FILE', 'CODE_SIGN_ENTITLEMENTS'])).map(([setting, value]) => ({ owner, setting, value, at: path.resolve(buildSettings.SRCROOT, value) }))),
        (setting) => Effect.mapBoth(fs.realPath(setting.at), { onFailure: () => `target ${setting.owner} ${setting.setting}: ${setting.value} does not exist`, onSuccess: (at) => path.relative(real, at) }),
    );
    const archives = archived.flatMap(([scheme, listed]) =>
        Array.match(
            listed.filter(({ buildSettings }) => !buildSettings.SKIP_INSTALL).map(({ buildSettings }) => buildSettings),
            {
                onEmpty: () => [],
                onNonEmpty: (installed) => [
                    {
                        scheme,
                        configuration: Array.headNonEmpty(installed).CONFIGURATION,
                        places: Array.dedupe(Array.map(installed, Struct.get('INSTALL_PATH'))),
                        notarized: installed.every((signing) => signing.ENABLE_HARDENED_RUNTIME === true && signing.CODE_SIGN_IDENTITY?.startsWith(DEVELOPER_ID.identity) === true),
                    },
                ],
            },
        ),
    );
    const failures = [...missing, ...archives.filter(({ places }) => places.length > 1).map(({ scheme, places }) => `scheme ${scheme} installs into ${places.join(' and ')}, and one xcodebuild install takes one DSTROOT`)];
    return failures.length === 0 ? { inputs: Array.dedupe([...container.inputs, ...resolved]), archives } : yield* AppleError.make({ subject: container.path, detail: Array.dedupe(failures).join('\n  ') });
});

const released = (path: Path.Path, plugin: Plugin, file: string): Pick<Part, 'release'> =>
    path.extname(path.dirname(file)) === '.xcodeproj' ? { release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest: path.relative(path.dirname(path.dirname(file)), file) } } } } : {};

const xcodeParts = (
    path: Path.Path,
    options: Options,
    plugin: Plugin,
    file: string,
    container: typeof Container.Type,
    inputs: readonly string[],
    archives: Effect.Success<ReturnType<typeof evaluated>>['archives'],
    built: string,
    destinations: (roots: readonly string[]) => readonly string[],
    packaged: (at: string) => boolean,
): Part => {
    const { lock, flags } = invocation(path, options, built);
    const name = path.basename(container.path, path.extname(container.path));
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map((scheme) => ({ ...scheme, packages: scheme.containers.every(packaged) }));
    const output = (scheme: string) => Option.map(Option.fromUndefinedOr(options.outputPath), (at) => path.join(at, 'xcode', name, scheme));
    const exported = archives.flatMap(({ scheme, configuration, notarized }) => Option.toArray(output(scheme)).map((folder) => ({ configuration, folder, notarized, entry: { archive: path.join(folder, `${scheme}.xcarchive`), folder, ...(notarized ? { method: DEVELOPER_ID.method } : {}) } })));
    const packaging = [
        { name: 'build--ditto', notarized: false, options: {}, description: 'Zips each archived app with ditto for a GitHub release', help: { command: 'man ditto', example: { args: ['--norsrc'] } } },
        {
            name: 'build--notarytool',
            notarized: true,
            options: options.notaryProfile === undefined ? {} : { profile: options.notaryProfile },
            description: 'Exports each Developer ID archive, notarizes it with notarytool, staples the ticket, and zips the app',
            help: { command: 'xcrun notarytool submit --help', example: { args: ['--timeout', '2h'] } },
        },
    ].flatMap((kind) =>
        Array.match(
            exported.filter(({ notarized }) => notarized === kind.notarized),
            { onEmpty: () => [], onNonEmpty: (exports) => [{ ...kind, exports }] },
        ),
    );
    const referenced = container.buildFiles
        .filter((at) => path.relative(path.dirname(container.path), at).startsWith('..'))
        .flatMap((at) => tokened(path, at))
        .flatMap((input) => [input, path.join(input, '**/*')]);
    const read = Array.dedupe([path.join('{projectRoot}', '**/*'), ...inputs, ...referenced, ...tokened(path, lock), ...plugin.inputs]);
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
    const builds = (configuration: Option.Option<string>): Runs =>
        schemes.flatMap((scheme) =>
            archives.some((archive) => archive.scheme === scheme.name && Option.contains(configuration, archive.configuration))
                ? [invoke('archive', scheme.name, Option.match(output(scheme.name), { onNone: () => [], onSome: (at) => ['-archivePath', path.join(at, `${scheme.name}.xcarchive`)] }))]
                : (scheme.packages ? destinations(scheme.containers).map((platform) => ['-destination', `generic/platform=${platform}`]) : [[]]).map((destination) => invoke('build', scheme.name, destination)),
        );
    const installs = archives.map(({ scheme, places }) => invoke('install', scheme, [`DSTROOT=${Array.headNonEmpty(places)}`, 'INSTALL_PATH=/']));
    const launches = Array.dedupe(schemes.flatMap(({ launch }) => Option.toArray(Option.fromUndefinedOr(launch))));
    const modes = (planned: (configuration: Option.Option<string>) => Runs): Partial<TargetConfiguration> => ({
        configurations: Record.fromIterableWith(container.configurations, (configuration) => [configuration.toLowerCase(), { runs: planned(Option.some(configuration)).map((run) => [...run, '-configuration', configuration]) }]),
        ...Option.match(
            Option.filter(Array.head(launches), () => launches.length === 1),
            { onNone: () => ({}), onSome: (launch) => ({ defaultConfiguration: launch.toLowerCase() }) },
        ),
    });
    return {
        root: path.dirname(container.path),
        name,
        file,
        technologies: ['xcode'],
        ...released(path, plugin, file),
        targets: {
            ...(schemes.length === 0
                ? {}
                : {
                      'build--xcodebuild': target(
                          plugin,
                          builds(Option.none()),
                          { technologies: ['xcode'], description: 'Builds each shared scheme with xcodebuild, archiving each scheme with installed products in its archive configuration', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } },
                          { dependsOn: ['^build'], inputs: read, ...modes(builds) },
                      ),
                      'check--xcodebuild': { executor: 'nx:noop', dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Compiles each shared scheme as its type check' } },
                  }),
            ...Record.fromIterableWith(packaging, ({ name: tool, exports, options: given, description, help }): readonly [string, TargetConfiguration] => [
                tool,
                {
                    executor: `${plugin.name}:zip`,
                    dependsOn: ['build--xcodebuild'],
                    inputs: plugin.inputs,
                    options: { exports: [], ...given },
                    configurations: Record.map(
                        Array.groupBy(exports, ({ configuration }) => configuration.toLowerCase()),
                        (group) => ({ exports: group.map(Struct.get('entry')) }),
                    ),
                    outputs: exports.map(({ folder }) => path.join('{workspaceRoot}', folder, '*.zip')),
                    metadata: { technologies: ['xcode'], description, help },
                },
            ]),
            ...(exported.length === 0
                ? {}
                : {
                      'nx-release-publish': published(
                          plugin,
                          'xcode',
                          exported.map(({ folder }) => folder),
                          exported.filter(({ notarized }) => notarized).map(({ folder }) => folder),
                      ),
                  }),
            ...(runs.length === 0
                ? {}
                : {
                      test: target(plugin, runs, { technologies: ['xcode'], description: 'Tests each test plan of a shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } } }, { cache: true, inputs: read, ...modes(() => runs) }),
                  }),
            ...(installs.length === 0
                ? {}
                : {
                      install: target(
                          plugin,
                          installs,
                          { technologies: ['xcode'], description: "Builds each shared scheme whose targets install in its archive configuration and installs the products at INSTALL_PATH on this Mac with xcodebuild's install action", help: { command: 'xcodebuild -help', example: { args: ['-jobs', '4'] } } },
                          { dependsOn: ['^build'], inputs: plugin.inputs, cache: false },
                      ),
                  }),
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, Host, invocation, Request, released, xcodeParts };
