import { basename, matchesGlob } from 'node:path';
import { platform } from 'node:process';
import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { present } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Function, Option, Path, Result, Schema, Struct, UndefinedOr } from 'effect';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly manifest: string;
    readonly root: string;
    readonly fields: Fields;
    readonly lock?: string;
    readonly withheld: readonly ('archive' | 'build' | 'install' | 'publish')[];
}
interface Discovered extends Declared {
    readonly host: Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = Array.dedupe(HOSTS.map(({ manifest }) => basename(manifest)));
const LOCKS = Array.dedupe(HOSTS.flatMap(({ build }) => Array.fromNullishOr(build?.lock)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const projects = (declared: readonly Declared[], _: Unit, { path, plugin, placed }: Request<unknown>): readonly Project[] => {
    const discovered = Array.filterMap(declared, (item) =>
        Result.map(
            Result.fromOption(
                Array.findFirst(HOSTS, (host) => host.manifest === item.manifest),
                Function.constVoid,
            ),
            (host): Discovered => ({ ...item, host: Struct.omit(host, item.withheld) }),
        ),
    );
    const holds = (other: Discovered, item: Discovered) => (other.fields.folders ?? []).some((folder) => !path.relative(path.join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Discovered) => (item.fields.folders === undefined ? discovered.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Discovered) => discovered.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    return discovered.map((item): Project => {
        const { technology, archive, platforms, standalone, build, install, publish, write, manifest } = item.host;
        const linked = Array.dedupe([...discovered.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)]);
        const outputPath = Option.getOrElse(placed(technology), () => path.join('dist', item.root));
        const builds: Record<string, TargetConfiguration> =
            build === undefined
                ? {}
                : {
                      [technology]: {
                          executor: `${plugin.name}:${build.executor}`,
                          ...(build.pack !== undefined && { dependsOn: ['build--*'] }),
                          options: { outputPath },
                          ...(standalone === true && { options: { outputPath, descriptor: item.file, hosts: hosts(item).map((other) => other.file), dependencies: uses(item).map((other) => other.file) }, parallelism: false }),
                          cache: archive !== undefined,
                          inputs: [
                              '{projectRoot}/**/*',
                              '^default',
                              ...(build.pack === undefined ? [] : [{ dependentTasksOutputFiles: '**/*' }]),
                              ...Array.fromNullishOr(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
                              ...(build.runtime ?? []).map((runtime) => ({ runtime })),
                              ...hosts(item)
                                  .filter((other) => other !== item)
                                  .map((other) => `{workspaceRoot}/${other.file}`),
                          ],
                          ...(archive !== undefined && { outputs: [`{options.outputPath}/*${archive}`] }),
                          metadata: { description: `Builds the ${technology} project its ${manifest} declares through ${build.command}` },
                      },
                  };
        const installer = Option.filter(Option.fromUndefinedOr(install), ({ platforms: held = [platform] }) => held.includes(platform));
        const installs: Record<string, TargetConfiguration> = Option.match(installer, {
            onSome: ({ command, unpacked }) => ({ [technology]: { executor: `${plugin.name}:install`, dependsOn: [unpacked === true ? `build--!(${technology})` : 'build'], metadata: { description: `Installs what build wrote in the run's configuration where its host loads it, through ${command}` } } }),
            onNone: () =>
                Array.match(
                    hosts(item).filter((other) => other.host.build !== undefined),
                    {
                        onEmpty: () => ({}),
                        onNonEmpty: (holders) => ({ [technology]: { executor: 'nx:noop', dependsOn: [{ projects: holders.map((other) => other.fields.name), target: 'build' }], metadata: { description: `Builds ${holders.map((other) => other.fields.name).join(', ')}, the host the result loads in` } } }),
                    },
                ),
        });
        return {
            file: item.file,
            root: item.root,
            name: item.fields.name,
            technologies: [technology],
            clean: technology,
            members: {
                build: builds,
                check: standalone === true && build !== undefined ? { [technology]: { executor: 'nx:noop', dependsOn: [`build--${technology}`], metadata: { description: 'Compiles the project as its check' } } } : {},
                install: installs,
            },
            ...(archive !== undefined && {
                publish: {
                    executor: `${plugin.name}:release-publish`,
                    dependsOn: [build === undefined ? 'build' : `build--${technology}`],
                    metadata: {
                        description: UndefinedOr.match(publish, {
                            onUndefined: () => `Attaches the ${archive} packages build wrote to the project's GitHub release through gh release upload`,
                            onDefined: ({ store, command }) => `Attaches the ${archive} packages build wrote to the project's GitHub release through gh release upload, then publishes them to ${store} through ${command}`,
                        }),
                    },
                },
            }),
            ...(write !== undefined && { release: { manifest: path.relative(item.root, item.file) } }),
            configuration: {
                ...(platforms !== undefined && { tags: platforms.map((os) => `os:${os}`) }),
                ...(standalone === true && linked.length > 0 && { implicitDependencies: linked.map((other) => other.fields.name) }),
            },
        };
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Schema.Struct({}),
    units: ({ files }: Request<unknown>) => Effect.succeed(Array.take(files, 1).map((file): Unit => ({ key: 'hosts', file, salt: files.join('\n') }))),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Unit>, { files, context }: Request<unknown>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const matched = Array.filterMap(files, (file) =>
            Result.map(
                Result.fromOption(
                    Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)),
                    Function.constVoid,
                ),
                (host) => ({ file, host, root: path.join(file, path.relative(host.manifest, '.')) }),
            ),
        );
        const local = Array.dedupe(matched.map(({ host }) => host)).filter(({ platforms = [platform] }) => platforms.includes(platform));
        const held = yield* Effect.filter(local, ({ install }) => (install?.probe === undefined ? Effect.succeed(true) : Effect.map(present(Array.headNonEmpty(install.probe), Array.tailNonEmpty(install.probe)), Option.isSome)));
        const locked = yield* Effect.forEach(matched, ({ host, root }) =>
            Effect.map(
                Effect.findFirst(Array.cartesianWith(ancestors(path, root), Array.fromNullishOr(host.build?.lock), path.join), (lock) => fs.exists(path.join(context.workspaceRoot, lock))),
                Option.match({ onNone: () => ({}), onSome: (lock) => ({ lock }) }),
            ),
        );
        const [read, failures] = yield* Effect.partition(
            Array.zip(matched, locked),
            ([{ file, host, root }, lock]) =>
                fs.readFileString(path.join(context.workspaceRoot, file)).pipe(
                    Effect.flatMap((text) => host.read(text, file)),
                    Effect.mapBoth({
                        onFailure: (error): readonly [string, Error] => [file, error],
                        onSuccess: Option.map((fields): Declared => ({ file, manifest: host.manifest, root, fields, ...lock, withheld: [...(local.includes(host) ? [] : (['archive', 'build', 'publish'] as const)), ...(held.includes(host) ? [] : (['install'] as const))] })),
                    }),
                ),
            { concurrency: 'unbounded' },
        );
        const report = Array.getSomes(read).filter(({ fields }) => fields.installed !== true);
        return missing.map(() => Result.succeed({ report, failures, partial: held.length < local.length }));
    }),
    sources: () => ({ tracked: [...FILES, ...LOCKS].map((file) => `**/${file}`) }),
    projects,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
