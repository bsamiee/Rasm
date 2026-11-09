import type { TargetConfiguration } from '@nx/devkit';
import { isCI } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';
import type { Options } from '#/plugins/project-graph';
import { AppleError, type Part, spawned, TOOLCHAIN, target, tokened } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(Schema.Struct({ name: Schema.String, containers: Schema.Array(Schema.String), plans: Schema.Array(Schema.String), tests: Schema.Boolean, launch: Schema.optionalKey(Schema.String) })),
    configurations: Schema.Array(Schema.String),
    inputs: Schema.Array(Schema.String),
    members: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    projects: Schema.Array(Schema.String),
    references: Schema.Array(Schema.String),
});
const Analyzed = Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Record(Schema.String, Schema.String) }));
const Evaluated = Schema.fromJsonString(Schema.Array(Schema.Struct({ target: Schema.String, buildSettings: Schema.Struct({ SRCROOT: Schema.String, INFOPLIST_FILE: Schema.optionalKey(Schema.String), CODE_SIGN_ENTITLEMENTS: Schema.optionalKey(Schema.String) }) })));
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
    const settings = (yield* Effect.forEach(
        container.schemes.filter(({ containers }) => containers.length > 0).flatMap(({ name }) => container.configurations.map((configuration) => ['-scheme', name, '-configuration', configuration])),
        (selection) => Effect.flatMap(spawned('xcodebuild', ['-showBuildSettings', '-json', ...flags, ...selection], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated)),
    )).flat();
    const real = yield* fs.realPath(workspaceRoot);
    const [resolved, missing] = yield* Effect.partition(
        settings.flatMap(({ target: owner, buildSettings: { SRCROOT, ...files } }) => Object.entries(files).map(([setting, value]) => ({ owner, setting, value, at: path.resolve(SRCROOT, value) }))),
        (setting) => Effect.mapBoth(fs.realPath(setting.at), { onFailure: () => `target ${setting.owner} ${setting.setting}: ${setting.value} does not exist`, onSuccess: (at) => path.relative(real, at) }),
    );
    return missing.length === 0 ? Array.dedupe([...container.inputs, ...resolved]) : yield* AppleError.make({ subject: container.path, detail: Array.dedupe(missing).join('\n  ') });
});

const xcodeParts = (path: Path.Path, options: Options, plugin: string, file: string, container: typeof Container.Type, inputs: readonly string[], built: string, destinations: (roots: readonly string[]) => readonly string[], packaged: (at: string) => boolean): Part => {
    const { lock, flags } = invocation(path, options, built);
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map((scheme) => ({ ...scheme, packages: scheme.containers.every(packaged) }));
    const referenced = container.references.flatMap((at) => tokened(path, at)).flatMap((input) => [input, path.join(input, '**/*')]);
    const read = Array.dedupe([path.join('{projectRoot}', '**/*'), ...inputs, ...referenced, ...tokened(path, lock), ...TOOLCHAIN]);
    const runs = schemes
        .filter(({ tests, packages }) => tests && !packages)
        .flatMap((scheme) =>
            (scheme.plans.length === 0 ? [Option.none<string>()] : scheme.plans.map(Option.some)).map((plan) =>
                invoke(
                    'test',
                    scheme.name,
                    Option.toArray(plan).flatMap((name) => ['-testPlan', name]),
                ),
            ),
        );
    const builds = schemes.flatMap((scheme) => (scheme.packages ? destinations(scheme.containers).map((platform) => ['-destination', `generic/platform=${platform}`]) : [[]]).map((destination) => invoke('build', scheme.name, destination)));
    const launches = Array.dedupe(schemes.flatMap(({ launch }) => Option.toArray(Option.fromUndefinedOr(launch))));
    const modes = (held: readonly (readonly [string, ...string[]])[]): Partial<TargetConfiguration> => ({
        configurations: Record.fromIterableWith(container.configurations, (name) => [name.toLowerCase(), { runs: held.map((run) => [...run, '-configuration', name]) }]),
        ...Option.match(
            Option.filter(Array.head(launches), () => launches.length === 1),
            { onNone: () => ({}), onSome: (launch) => ({ defaultConfiguration: launch.toLowerCase() }) },
        ),
    });
    return {
        root: path.dirname(container.path),
        name: path.basename(container.path, path.extname(container.path)),
        file,
        technologies: ['xcode'],
        ...(path.extname(container.path) === '.xcodeproj' ? { release: { version: { versionActions: `${plugin}/release/version-actions`, versionActionsOptions: { manifest: path.relative(path.dirname(container.path), file) } } } } : {}),
        targets: {
            ...(schemes.length === 0
                ? {}
                : {
                      'build--xcodebuild': target(plugin, builds, { technologies: ['xcode'], description: 'Builds each shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } }, { dependsOn: ['^build'], inputs: read, ...modes(builds) }),
                      'check--xcodebuild': { executor: 'nx:noop', dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Compiles each shared scheme as its type check' } },
                  }),
            ...(runs.length === 0
                ? {}
                : {
                      test: target(plugin, runs, { technologies: ['xcode'], description: 'Tests each test plan of a shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } } }, { cache: true, inputs: read, ...modes(runs) }),
                  }),
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, Host, Request, xcodeParts };
