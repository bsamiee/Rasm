import { basename, matchesGlob } from 'node:path';
import { platform } from 'node:process';
import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { present } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Function, Option, Path, Result, Schema, Struct } from 'effect';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly manifest: string;
    readonly root: string;
    readonly fields: Fields;
    readonly lock?: string;
    readonly withheld: readonly ('archive' | 'build' | 'install' | 'operations' | 'publish')[];
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
    const users = (item: Discovered) => discovered.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
    const engined = (item: Discovered, loading: readonly Discovered[]): Host => (item.fields.folders !== undefined || item.host.standalone !== true || loading.some((other) => other !== item && other.host.build !== undefined) ? item.host : Struct.omit(item.host, ['archive', 'build', 'operations', 'publish']));
    return discovered.map((item): Project => {
        const loading = Array.dedupe([...hosts(item), ...users(item)]);
        const offered = engined(item, loading);
        const { technology, archive, platforms, standalone, build, operations, install, publish, write, manifest } = offered;
        const linked = Array.dedupe([...[...discovered.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)].map((other) => other.fields.name), ...Object.values({ ...item.fields.packages }).map(({ project }) => project)]);
        const outputPath = Option.getOrElse(placed(technology), () => path.join('dist', item.root));
        const settings = standalone === true ? { outputPath, descriptor: item.file, hosts: loading.map((other) => other.file), dependencies: uses(item).map((other) => other.file), packages: { ...item.fields.packages } } : { outputPath };
        const inputs: NonNullable<TargetConfiguration['inputs']> = [
            '{projectRoot}/**/*',
            '^default',
            ...Array.fromNullishOr(build?.pack).map(() => ({ dependentTasksOutputFiles: '**/*' })),
            ...Array.fromNullishOr(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
            ...Array.fromNullishOr(build?.runtime).flatMap((runtimes) => runtimes.map((runtime) => ({ runtime }))),
            ...loading.filter((other) => other !== item).map((other) => `{workspaceRoot}/${other.file}`),
        ];
        const required = Array.match(
            uses(item).filter((other) => !hosts(other).includes(item)),
            { onEmpty: () => [], onNonEmpty: (loaded) => [{ projects: loaded.map((other) => other.fields.name), target: 'install' }] },
        );
        const written = operations?.pack === undefined ? Array.fromNullishOr(archive).map((extension) => `{options.outputPath}/*${extension}`) : ['{options.outputPath}/package'];
        const builder = build === undefined ? 'build' : `build--${technology}`;
        const packer = operations?.pack === undefined ? builder : `pack--${technology}`;
        const built = (target: (held: NonNullable<Host['build']>) => TargetConfiguration): Record<string, TargetConfiguration> => Object.fromEntries(Array.fromNullishOr(build).map((held) => [technology, target(held)] as const));
        const operated = (verb: keyof NonNullable<Host['operations']>, extra: Partial<TargetConfiguration> & { readonly options?: Readonly<Record<string, unknown>> }): Record<string, TargetConfiguration> =>
            Object.fromEntries(
                Array.fromNullishOr(operations?.[verb]).flatMap(({ command, options }) =>
                    Object.entries(built(({ executor }) => ({ executor: `${plugin.name}:${executor}`, parallelism: false, ...extra, options: { ...settings, ...options, ...extra.options }, metadata: { description: `Runs ${command} for the ${technology} project its ${manifest} declares` } }))),
                ),
            );
        const holding: Record<string, TargetConfiguration> = Array.match(
            hosts(item).filter((other) => other.host.build !== undefined),
            {
                onEmpty: () => ({}),
                onNonEmpty: (holders) => ({ [technology]: { executor: 'nx:noop', dependsOn: [{ projects: holders.map((other) => other.fields.name), target: 'build' }], metadata: { description: `Builds ${holders.map((other) => other.fields.name).join(', ')}, the host the result loads in` } } }),
            },
        );
        const copied = users(item).length === 0 ? {} : operated('install', { dependsOn: [builder, ...(holding[technology]?.dependsOn ?? [])], options: { users: users(item).map((other) => other.file) } });
        const installs: Record<string, TargetConfiguration> = Option.match(
            Option.filter(Option.fromUndefinedOr(install), ({ platforms: held = [platform] }) => held.includes(platform)),
            {
                onSome: ({ command, unpacked }) => ({ [technology]: { executor: `${plugin.name}:install`, dependsOn: [unpacked === true ? `build--!(${technology})` : 'build'], metadata: { description: `Installs what build wrote in the run's configuration where its host loads it, through ${command}` } } }),
                onNone: () => ({ ...holding, ...copied }),
            },
        );
        const compiled = standalone === true ? built(() => ({ executor: 'nx:noop', dependsOn: [builder], metadata: { description: 'Compiles the project as its check' } })) : {};
        return {
            file: item.file,
            root: item.root,
            name: item.fields.name,
            technologies: [technology],
            clean: technology,
            members: {
                build: built((held) => ({
                    executor: `${plugin.name}:${held.executor}`,
                    dependsOn: [...Array.fromNullishOr(held.pack).map(() => 'build--*'), ...required],
                    options: settings,
                    parallelism: standalone !== true,
                    cache: archive !== undefined,
                    inputs,
                    outputs: written,
                    metadata: { description: `Builds the ${technology} project its ${manifest} declares through ${held.command}` },
                })),
                check: { ...compiled, ...operated('check', { cache: true, inputs, dependsOn: required }) },
                test: loading.some((other) => other !== item) ? operated('test', { cache: true, inputs, dependsOn: [`install--${technology}`], outputs: ['{options.outputPath}/report'] }) : {},
                pack: operated('pack', { cache: true, inputs, dependsOn: [builder], outputs: ['{options.outputPath}/archive'] }),
                install: installs,
            },
            ...(archive !== undefined && { packaged: [packer] }),
            ...(publish !== undefined && { publish: { executor: `${plugin.name}:release-publish`, metadata: { description: `Publishes the ${archive} packages of the project to ${publish.store} through ${publish.command}` } } }),
            ...(write !== undefined && item.fields.version !== undefined && { release: { manifest: path.relative(item.root, item.file) } }),
            configuration: {
                ...(platforms !== undefined && { tags: platforms.map((os) => `os:${os}`) }),
                ...(standalone === true && linked.length > 0 && { implicitDependencies: linked }),
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
        const hosted = yield* Effect.forEach(matched, ({ file, host }) => (host.held === undefined || !local.includes(host) ? Effect.succeed(true) : host.held(path.join(context.workspaceRoot, file))));
        const [read, failures] = yield* Effect.partition(
            Iterator.zip([matched, locked, hosted]),
            ([{ file, host, root }, lock, engined]) =>
                fs.readFileString(path.join(context.workspaceRoot, file)).pipe(
                    Effect.flatMap((text) => host.read(text, file)),
                    Effect.mapBoth({
                        onFailure: (error): readonly [string, Error] => [file, error],
                        onSuccess: Option.map((fields): Declared => ({ file, manifest: host.manifest, root, fields, ...lock, withheld: [...(local.includes(host) && engined ? [] : (['archive', 'build', 'operations', 'publish'] as const)), ...(held.includes(host) ? [] : (['install'] as const))] })),
                    }),
                ),
            { concurrency: 'unbounded' },
        );
        const report = Array.getSomes(read).filter(({ fields }) => fields.installed !== true);
        return missing.map(() => Result.succeed({ report, failures, partial: held.length < local.length || hosted.includes(false) }));
    }),
    sources: () => ({ tracked: [...FILES, ...LOCKS].map((file) => `**/${file}`) }),
    projects,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
