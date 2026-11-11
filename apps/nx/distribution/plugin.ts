import { matchesGlob } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { Array, Effect, FileSystem, Function, Option, Path, pipe, Result, Schema, UndefinedOr } from 'effect';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly manifest: string;
    readonly root: string;
    readonly fields: Fields;
}
interface Discovered extends Declared {
    readonly host: Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE = 'release';
const FILES = Array.dedupe(HOSTS.map(({ manifest }) => manifest.split('/').at(-1) ?? manifest));

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ engines: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) }).annotate({ description: 'engines: Unreal Engine roots by association, read before the install registry' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const projects = (declared: readonly Declared[], _: Unit, { path, plugin, named, placed, options: { engines } }: Request<typeof Options.Type>): readonly Project[] => {
    const discovered = Array.filterMap(declared, (item) =>
        Result.map(
            Result.fromOption(
                Array.findFirst(HOSTS, (host) => host.manifest === item.manifest),
                Function.constVoid,
            ),
            (host): Discovered => ({ ...item, host }),
        ),
    );
    const holds = (other: Discovered, item: Discovered) => (other.fields.folders ?? []).some((folder) => !path.relative(path.join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Discovered) => (item.fields.folders === undefined ? discovered.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Discovered) => discovered.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    const builds = (item: Discovered) =>
        Option.map(Option.fromUndefinedOr(item.host.build), ({ executor, command, variables, pack }): TargetConfiguration => {
            const archived = pack === undefined ? { folder: item.fields.name, file: UndefinedOr.match(item.fields.version, { onUndefined: () => `${item.fields.name}-*`, onDefined: (version) => `${item.fields.name}-${version}*` }) } : { folder: '{projectName}', file: '*' };
            const destination = {
                outputPath: Option.getOrElse(placed(item.host.technology, archived.folder), () => path.join('dist', item.root)),
                ...(item.host.standalone === true && { descriptor: item.file, hosts: hosts(item).map((other) => other.file), dependencies: uses(item).map((other) => other.file), ...(engines !== undefined && { engines }) }),
            };
            return {
                executor: `${plugin.name}:${executor}`,
                ...(pack !== undefined && { dependsOn: ['build--*'] }),
                ...UndefinedOr.match(variables, { onUndefined: () => ({ options: destination }), onDefined: () => ({ options: {}, configurations: { [RELEASE]: destination } }) }),
                cache: item.host.archive !== undefined,
                inputs: [
                    '{projectRoot}/**/*',
                    `^${named(item.root)}`,
                    ...(pack === undefined ? [] : [{ dependentTasksOutputFiles: '**/*' }]),
                    ...UndefinedOr.match(variables, { onUndefined: Array.empty, onDefined: Array.map((env) => ({ env })) }),
                    ...hosts(item)
                        .filter((other) => other !== item)
                        .map((other) => `{workspaceRoot}/${other.file}`),
                    { externalDependencies: [] },
                    ...plugin.inputs,
                ],
                ...(item.host.archive !== undefined && { outputs: [path.join('{options.outputPath}', `${archived.file}${item.host.archive}`)] }),
                metadata: { description: `Builds the ${item.host.technology} project its ${item.host.manifest} declares`, help: { command: `${command} --help`, example: { options: { outputPath: 'dist' } } } },
            };
        });
    const installs = (item: Discovered): Option.Option<TargetConfiguration> =>
        Option.firstSomeOf([
            Option.map(
                Option.fromUndefinedOr(item.host.install),
                ({ command }): TargetConfiguration => ({
                    executor: `${plugin.name}:install`,
                    dependsOn: ['build'],
                    metadata: { description: `Installs the ${item.host.technology} packages build wrote in the run's configuration, through ${command}`, help: { command: `${command} --help`, example: { options: { dryRun: true } } } },
                }),
            ),
            Option.map(
                Option.fromUndefinedOr(item.host.place),
                (): TargetConfiguration => ({
                    executor: `${plugin.name}:place`,
                    dependsOn: [`build--!(${item.host.technology})`],
                    ...UndefinedOr.match(item.host.build?.variables, {
                        onUndefined: () => ({ metadata: { description: `Places the unpacked ${item.host.technology} plug-in the project's builds wrote into its host's per-user folder, keeping the plug-in's data unless removeData is set` } }),
                        onDefined: (variables) => ({
                            configurations: { [RELEASE]: { variables } },
                            metadata: { description: `Signs the unpacked ${item.host.technology} plug-in the project's builds wrote and places it into its host's per-user folder, with the certificate ${variables[0]} holds in ${RELEASE} and this machine's self-signed certificate otherwise` },
                        }),
                    }),
                }),
            ),
            Option.liftPredicate(
                {
                    executor: 'nx:noop',
                    dependsOn: [{ projects: hosts(item).map((other) => other.fields.name), target: 'build' }],
                    metadata: {
                        description: `Builds ${hosts(item)
                            .map((other) => other.fields.name)
                            .join(', ')}, the host the result loads in`,
                    },
                },
                () => hosts(item).length > 0,
            ),
        ]);
    return discovered.map((item): Project => {
        const build = builds(item);
        const { technology } = item.host;
        const standalone = Option.filter(build, () => item.host.standalone === true);
        const linked = Array.dedupe([...discovered.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)]);
        const versioned = Option.isSome(build) && item.host.write !== undefined;
        return {
            file: item.file,
            root: item.root,
            technologies: [technology],
            clean: technology,
            ...Option.getOrUndefined(Option.map(standalone, () => ({ name: item.fields.name }))),
            members: {
                build: Option.match(build, { onNone: () => ({}), onSome: (target) => ({ [technology]: target }) }),
                check: Option.match(standalone, { onNone: () => ({}), onSome: () => ({ [technology]: { executor: 'nx:noop', dependsOn: [`build--${technology}`], metadata: { description: 'Compiles the project as its check' } } }) }),
                install: Option.match(installs(item), { onNone: () => ({}), onSome: (target) => ({ [item.host.install?.tool ?? technology]: target }) }),
            },
            targets:
                item.host.archive === undefined
                    ? {}
                    : {
                          'nx-release-publish': {
                              executor: `${plugin.name}:release-publish`,
                              dependsOn: ['^nx-release-publish'],
                              inputs: plugin.inputs,
                              defaultConfiguration: RELEASE,
                              configurations: { [RELEASE]: {} },
                              metadata: {
                                  description: UndefinedOr.match(item.host.publish, {
                                      onUndefined: () => `Attaches the ${technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release`,
                                      onDefined: ({ command }) => `Attaches the ${technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release and publishes them through ${command}`,
                                  }),
                              },
                          },
                      },
            kinds: { host: item.host.build !== undefined && item.host.build.pack === undefined ? [technology] : [], release: versioned ? ['github'] : [] },
            configuration: {
                ...(Option.isSome(standalone) && linked.length > 0 && { implicitDependencies: linked.map((other) => other.fields.name) }),
                ...(versioned && { release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest: path.relative(item.root, item.file) } } } }),
            },
        };
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Options,
    units: ({ files }: Request<typeof Options.Type>) => Effect.succeed(Array.take(files, 1).map((file): Unit => ({ key: 'hosts', file, salt: files.join('\n') }))),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Unit>, { files, context }: Request<typeof Options.Type>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const matched = Array.getSomes(
            files.map((file) =>
                pipe(
                    Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)),
                    Option.map((host) => ({ file, host, root: path.join(file, path.relative(host.manifest, '.')) })),
                ),
            ),
        );
        const [read, failures] = yield* Effect.partition(
            matched,
            ({ file, host, root }) =>
                fs.readFileString(path.join(context.workspaceRoot, file)).pipe(
                    Effect.flatMap((text) => host.read(text, file)),
                    Effect.mapBoth({ onFailure: (error): readonly [string, Error] => [file, error], onSuccess: Option.map((fields): Declared => ({ file, manifest: host.manifest, root, fields })) }),
                ),
            { concurrency: 'unbounded' },
        );
        const report = Array.getSomes(read).filter(({ fields }) => fields.installed !== true);
        return missing.map(() => Result.succeed({ report, failures }));
    }),
    sources: () => ({ tracked: FILES.map((file) => `**/${file}`) }),
    projects,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
