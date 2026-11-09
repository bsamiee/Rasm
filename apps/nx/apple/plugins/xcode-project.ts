import { isCI } from '@nx/devkit/internal';
import { Effect, FileSystem, Option, Path, Schema } from 'effect';
import { AppleError, type Options, type Part, spawned, TOOLCHAIN, target, tokened } from './targets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(Schema.Struct({ name: Schema.String, containers: Schema.Array(Schema.String), plans: Schema.Array(Schema.String), tests: Schema.Boolean })),
    inputs: Schema.Array(Schema.String),
    members: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    projects: Schema.Array(Schema.String),
    unresolved: Schema.Array(Schema.Struct({ target: Schema.String, setting: Schema.String, value: Schema.String })),
});
const Analyzed = Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Record(Schema.String, Schema.String) }));
const Evaluated = Schema.fromJsonString(Schema.Array(Schema.Struct({ target: Schema.String, buildSettings: Schema.Record(Schema.String, Schema.String) })));
const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ platform: Schema.String }) }));
const Request = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, container: typeof Container.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const settings = (yield* Effect.forEach(
        container.unresolved.length === 0 ? [] : container.schemes.filter(({ containers }) => containers.length > 0),
        (scheme) => Effect.flatMap(spawned('xcodebuild', ['-showBuildSettings', '-json', '-project', container.path, '-scheme', scheme.name], { cwd: workspaceRoot }), Schema.decodeEffect(Evaluated)),
        { concurrency: 'unbounded' },
    )).flat();
    const real = yield* fs.realPath(workspaceRoot);
    const [resolved, missing] = yield* Effect.partition(container.unresolved.filter((setting) => settings.some((entry) => entry.target === setting.target)), (setting) =>
        Option.match(Option.fromUndefinedOr(settings.find((entry) => entry.target === setting.target)?.buildSettings[setting.setting]), {
            onNone: () => Effect.fail(setting),
            onSome: (value) => Effect.mapBoth(fs.realPath(path.resolve(workspaceRoot, path.dirname(container.path), value)), { onFailure: () => setting, onSuccess: (at) => path.relative(real, at) }),
        }),
    );
    return missing.length === 0
        ? [...container.inputs, ...resolved]
        : yield* AppleError.make({ subject: container.path, detail: missing.map((setting) => `target ${setting.target} ${setting.setting}: ${setting.value} does not exist`).join('\n  ') });
});

const xcodeParts = (
    path: Path.Path,
    options: Options,
    file: string,
    container: typeof Container.Type,
    inputs: readonly string[],
    through: Option.Option<string>,
    destinations: (roots: readonly string[]) => readonly string[],
    packaged: (at: string) => boolean,
): Part => {
    const root = path.dirname(container.path);
    const built = Option.getOrElse(through, () => container.path);
    const workspace = path.extname(built) === '.xcworkspace';
    const lock = path.join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved');
    const shared = [...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : []), ...(Boolean(isCI()) ? ['-disableAutomaticPackageResolution'] : [])];
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, workspace ? '-workspace' : '-project', built, '-scheme', scheme, ...shared, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0).map((scheme) => ({ ...scheme, packages: scheme.containers.every(packaged) }));
    const read = [path.join('{projectRoot}', '**/*'), ...inputs, ...tokened(path, lock), ...TOOLCHAIN];
    const runs = schemes
        .filter(({ tests, packages }) => tests && !packages)
        .flatMap((scheme) => (scheme.plans.length === 0 ? [Option.none<string>()] : scheme.plans.map(Option.some)).map((plan) => invoke('test', scheme.name, Option.toArray(plan).flatMap((name) => ['-testPlan', name]))));
    return {
        root,
        name: path.basename(container.path, path.extname(container.path)),
        file,
        technologies: ['xcode'],
        targets: {
            build: target(
                schemes.flatMap((scheme) =>
                    (scheme.packages ? destinations(scheme.containers).map((platform) => ['-destination', `generic/platform=${platform}`]) : [[]]).map((destination) =>
                        invoke('build', scheme.name, destination),
                    ),
                ),
                { technologies: ['xcode'], description: 'Builds each shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-configuration', 'Release'] } } },
                { inputs: read },
            ),
            'check--xcodebuild': { executor: 'nx:noop', dependsOn: ['build'], inputs: read, metadata: { technologies: ['xcode'], description: 'Compiles each shared scheme as its type check' } },
            ...(runs.length === 0
                ? {}
                : {
                      test: target(
                          runs,
                          { technologies: ['xcode'], description: 'Tests each test plan of a shared scheme with xcodebuild', help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } } },
                          { cache: true, inputs: read },
                      ),
                  }),
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, Host, Request, xcodeParts };
