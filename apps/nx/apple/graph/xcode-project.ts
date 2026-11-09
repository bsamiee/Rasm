import { isCI } from '@nx/devkit/internal';
import { Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { AppleError, type FOLDERS, linted, type Options, type Part, spawned, target, tokened } from './targets.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(Schema.Struct({ name: Schema.String, entries: Schema.Array(Schema.Struct({ container: Schema.String })), plans: Schema.Array(Schema.String), tests: Schema.Boolean })),
    inputs: Schema.Array(Schema.String),
    members: Schema.Array(Schema.Struct({ product: Schema.String, packages: Schema.Array(Schema.String) })),
    projects: Schema.Array(Schema.String),
    unresolved: Schema.Array(Schema.Struct({ target: Schema.String, setting: Schema.String, value: Schema.String })),
});
const Analyzed = Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Array(Schema.Struct({ path: Schema.String, reason: Schema.String })) }));
const Evaluated = Schema.fromJsonString(Schema.Array(Schema.Struct({ target: Schema.String, buildSettings: Schema.Record(Schema.String, Schema.String) })));
const Host = Schema.fromJsonString(Schema.Struct({ target: Schema.Struct({ triple: Schema.String }) }));
const Request = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluated = Effect.fnUntraced(function* (workspaceRoot: string, container: typeof Container.Type, derived: Option.Option<string>) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const settings = (yield* Effect.forEach(
        container.unresolved.length === 0 ? [] : container.schemes.filter(({ entries }) => entries.length > 0),
        (scheme) =>
            Effect.flatMap(
                spawned('xcodebuild', ['-showBuildSettings', '-json', '-project', container.path, '-scheme', scheme.name, ...Option.toArray(derived).flatMap((at) => ['-derivedDataPath', at])], {
                    cwd: workspaceRoot,
                }),
                Schema.decodeEffect(Evaluated),
            ),
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
    placed: (root: string) => Option.Option<Readonly<Record<keyof typeof FOLDERS, string>>>,
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
    const folders = placed(path.dirname(built));
    const shared = [
        ...Option.toArray(folders).flatMap(({ derived }) => ['-derivedDataPath', derived]),
        ...(options.trustPackages === true ? ['-skipMacroValidation', '-skipPackagePluginValidation'] : []),
        ...(Boolean(isCI()) ? ['-disableAutomaticPackageResolution'] : []),
    ];
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, workspace ? '-workspace' : '-project', built, '-scheme', scheme, ...shared, ...extra] as const;
    const schemes = container.schemes.filter(({ entries }) => entries.length > 0).map((scheme) => ({ ...scheme, packages: scheme.entries.every(({ container: owner }) => packaged(owner)) }));
    const runs = schemes
        .filter(({ tests, packages }) => tests && !packages)
        .flatMap((scheme) => (scheme.plans.length === 0 ? [Option.none<string>()] : scheme.plans.map(Option.some)).map((plan) => ({ scheme: scheme.name, plan })))
        .map(({ scheme, plan }) => ({ scheme, plan, bundle: Option.map(Option.map(placed(root), ({ results }) => results), (at) => path.join(at, `${[scheme, ...Option.toArray(plan)].join('-')}.xcresult`)) }));
    return {
        root,
        name: path.basename(container.path, path.extname(container.path)),
        file,
        technologies: ['xcode'],
        targets: {
            build: target(
                schemes.flatMap((scheme) =>
                    (scheme.packages ? destinations(scheme.entries.map(({ container: owner }) => owner)).map((platform) => ['-destination', `generic/platform=${platform}`]) : [[]]).map((destination) => ({
                        command: invoke('build', scheme.name, destination),
                    })),
                ),
            ),
            ...(runs.length === 0
                ? {}
                : {
                      test: target(
                          runs.map(({ scheme, plan, bundle }) => ({
                              command: invoke('test', scheme, [...Option.toArray(plan).flatMap((name) => ['-testPlan', name]), ...Option.toArray(bundle).flatMap((at) => ['-resultBundlePath', at])]),
                              results: Option.toArray(bundle),
                          })),
                          {
                              cache: true,
                              inputs: [path.join('{projectRoot}', '**/*'), ...inputs, ...tokened(path, lock), { runtime: 'xcodebuild -version' }],
                              outputs: runs.flatMap(({ bundle }) => Option.toArray(bundle)).flatMap((at) => tokened(path, at)),
                          },
                      ),
                  }),
            ...linted(path, root, [root]),
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, evaluated, Host, Request, xcodeParts };
