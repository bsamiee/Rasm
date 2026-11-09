import { glob } from 'node:fs/promises';
import process from 'node:process';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { findMatchingTargetNames } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Match, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { Toml, Xml, Yaml, Zip } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields {
    readonly name: string;
    readonly version: string;
    readonly platforms: readonly string[];
}

interface Package extends Fields {
    readonly host: Host;
    readonly file: string;
}

interface Host {
    readonly manifest: string;
    readonly archive: string;
    readonly technology: string;
    readonly platforms: Partial<Record<NodeJS.Platform, string>>;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>>;
    readonly install: { readonly command: string; readonly run: (held: Package) => Effect.Effect<void, DistributionError, ChildProcessSpawner.ChildProcessSpawner> };
    readonly publish?: {
        readonly command: string;
        readonly run: (held: Package, options: { readonly dryRun: boolean; readonly registry: Option.Option<string> }) => Effect.Effect<void, DistributionError, Path.Path | ChildProcessSpawner.ChildProcessSpawner>;
    };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UPIA = '/Library/Application Support/Adobe/Adobe Desktop Common/RemoteComponents/UPI/UnifiedPluginInstallerAgent/UnifiedPluginInstallerAgent.app/Contents/MacOS/UnifiedPluginInstallerAgent';
const YAK = 'https://yak.rhino3d.com';
const REMOVED = /^(?:Removal Successful.*|.*status = -406)$/mu;
const INSTALLED = /^Installation Successful.*$/mu;
const PUBLISHED = /^Successfully published.*$/mu;
const REPOSITORY = /^Repository: /mu;
const REPOSITORY_ID = /\(id=(?<id>[^)]+)\)/u;
const DISTRIBUTION = /-(?<platform>\w+)\.yak$/u;
const UPSTREAM = /^\^/u;

// --- [MODELS] --------------------------------------------------------------------------

const Extension = Schema.Struct({
    schema_version: Schema.String,
    id: Schema.String,
    version: Schema.String,
    platforms: Schema.optionalKey(Schema.Array(Schema.String)),
    build: Schema.optionalKey(Schema.Struct({ generated: Schema.optionalKey(Schema.Struct({ platforms: Schema.Array(Schema.String) })) })),
});
const Yak = Schema.Struct({ name: Schema.String, version: Schema.String, authors: Schema.Array(Schema.String), description: Schema.String });
const Uxp = Schema.fromJsonString(Schema.Struct({ manifestVersion: Schema.Finite, id: Schema.String, name: Schema.String, version: Schema.String }));
const Cep = Schema.Struct({ ExtensionBundleId: Schema.String, ExtensionBundleName: Schema.String, ExtensionBundleVersion: Schema.String });
const Pushed = Schema.Struct({ distributions: Schema.Array(Schema.Struct({ filename: Schema.String })) });

// --- [ERRORS] --------------------------------------------------------------------------

class DistributionError extends Schema.TaggedError<DistributionError, { readonly brand: unique symbol }>()('DistributionError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[]) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, { extendEnv: true, stdin: 'ignore' });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* DistributionError.make({ subject: [command, ...args].join(' '), detail: `exited ${code}: ${[stdout, stderr].join('\n').trim()}` });
        }),
    ).pipe(Effect.catchTag('PlatformError', (error) => DistributionError.make({ subject: [command, ...args].join(' '), detail: error.message })));

const reported = (command: string, args: readonly string[], signal: RegExp) =>
    Effect.flatMap(spawned(command, args), (stdout) =>
        Option.match(Option.fromNullishOr(signal.exec(stdout)), {
            onSome: ([line]) => Effect.logInfo(line),
            onNone: () => DistributionError.make({ subject: [command, ...args].join(' '), detail: `printed no line matching ${signal.source}: ${stdout.trim()}` }),
        }),
    );

const upia = (held: Package) => Effect.andThen(reported(UPIA, ['--remove', held.name], REMOVED), reported(UPIA, ['--install', held.file], INSTALLED));

const entry = (file: string, name: string) =>
    Effect.flatMap(FileSystem.FileSystem, (fs) =>
        fs.stream(file).pipe(
            Stream.pipeThroughChannel(Zip.decode()),
            Stream.mapEffect((found) => (found.header.path === name ? Effect.asSome(Stream.mkString(Stream.decodeText(found.body))) : Effect.succeedNone)),
            Stream.filter(Option.isSome),
            Stream.runHead,
            Effect.map(Option.flatten),
        ),
    );

const built = Effect.fnUntraced(function* (context: ExecutorContext) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const targets = node.data.targets ?? {};
    const tasks = Record.values(context.taskGraph?.tasks ?? {}).filter(({ target }) => target.project === project);
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([
            ...names,
            ...[...names]
                .flatMap((name) => targets[name]?.dependsOn ?? [])
                .map((dependency) => (typeof dependency === 'string' ? { target: dependency.replace(UPSTREAM, ''), dependencies: UPSTREAM.test(dependency) } : dependency))
                .filter((dependency) => !('projects' in dependency && dependency.projects !== undefined) && dependency.dependencies !== true)
                .flatMap(({ target }) => Array.match(findMatchingTargetNames(target, Record.keys(targets)), { onEmpty: () => [target], onNonEmpty: (found) => found })),
        ]);
        return next.size === names.size ? names : reached(next);
    };
    const selected = [...reached(new Set(['build']))].filter((target) => target in targets);
    const declared = selected.filter((target) => targets[target]?.outputs !== undefined);
    const outputs = declared.flatMap((target) => {
        const configuration = tasks.find((task) => task.target.target === target)?.target.configuration ?? targets[target]?.defaultConfiguration;
        return getOutputsForTargetAndConfiguration({ project, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node);
    });
    const written = yield* Stream.runCollect(
        Stream.fromAsyncIterable(
            glob(
                outputs.filter((output) => !output.startsWith('!')).flatMap((output) => [output, path.join(output, '**')]),
                { cwd: context.root, exclude: outputs.filter((output) => output.startsWith('!')).map((output) => output.slice(1)) },
            ),
            (cause) => DistributionError.make({ subject: project, detail: `cannot list its build outputs: ${String(cause)}` }),
        ),
    );
    const archives = Array.dedupe(written).flatMap((file) =>
        Option.toArray(
            Option.map(
                Array.findFirst(Record.values(HOSTS), (host) => file.endsWith(host.archive)),
                (host) => ({ file: path.join(context.root, file), host }),
            ),
        ),
    );
    const packages = yield* Effect.forEach(
        archives,
        Effect.fnUntraced(function* ({ file, host }) {
            const text = yield* entry(file, host.manifest);
            const fields = Option.isSome(text) ? yield* host.read(text.value, file) : Option.none<Fields>();
            return Option.map(fields, (held): Package => ({ ...held, host, file }));
        }),
    );
    return { project, outputs, configurations: Array.dedupe(selected.flatMap((target) => Record.keys(targets[target]?.configurations ?? {}))), packages: Array.getSomes(packages) };
});

const installable = (held: Package) => held.platforms.length === 0 || Option.exists(Option.fromUndefinedOr(held.host.platforms[process.platform]), (os) => held.platforms.some((platform) => platform === os || platform === `${os}-${process.arch}`));

// --- [HOSTS] ---------------------------------------------------------------------------

const HOSTS: Readonly<Record<'blender' | 'yak' | 'uxp' | 'cep', Host>> = {
    blender: {
        manifest: 'blender_manifest.toml',
        archive: '.zip',
        technology: 'blender',
        platforms: { darwin: 'macos', win32: 'windows', linux: 'linux' },
        read: (text) => Effect.succeed(Option.map(Option.flatMap(Result.getSuccess(Toml.parse(text)), Schema.decodeUnknownOption(Extension)), ({ id, version, platforms, build }) => ({ name: id, version, platforms: build?.generated?.platforms ?? platforms ?? [] }))),
        install: {
            command: 'blender --command extension install-file',
            run: (held) =>
                Effect.flatMap(spawned('blender', ['--command', 'extension', 'list']), (listing) =>
                    reported(
                        'blender',
                        [
                            '--command',
                            'extension',
                            'install-file',
                            '--repo',
                            Option.getOrElse(
                                Option.flatMap(
                                    Array.findFirst(listing.split(REPOSITORY), (section) => section.includes(`\n  ${held.name} [installed]`)),
                                    (section) => Option.fromNullishOr(REPOSITORY_ID.exec(section)?.groups?.id),
                                ),
                                () => 'user_default',
                            ),
                            '--enable',
                            held.file,
                        ],
                        new RegExp(`^STATUS (Installed|Reinstalled) "${RegExp.escape(held.name)}"$`, 'mu'),
                    ),
                ),
        },
    },
    yak: {
        manifest: 'manifest.yml',
        archive: '.yak',
        technology: 'rhino',
        platforms: { darwin: 'mac', win32: 'win' },
        read: (text, file) =>
            Effect.succeed(
                Option.map(Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Yak)), ({ name, version }) => ({
                    name,
                    version,
                    platforms: Array.filter(Option.toArray(Option.fromUndefinedOr(DISTRIBUTION.exec(file)?.groups?.platform)), (platform) => platform !== 'any'),
                })),
            ),
        install: { command: 'yak install', run: (held) => reported('yak', ['install', held.file], new RegExp(`^Successfully installed ${RegExp.escape(held.name)}`, 'mu')) },
        publish: {
            command: 'yak push',
            run: (held, options) =>
                Effect.gen(function* () {
                    const path = yield* Path.Path;
                    const registry = Option.getOrElse(options.registry, () => YAK);
                    const pushed = yield* Effect.flatMap(
                        HttpClient.get(`${registry}/versions/${held.name}/${held.version}`),
                        HttpClientResponse.matchStatus({
                            404: () => Effect.succeed(Array.empty<string>()),
                            orElse: (response) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Pushed)), ({ distributions }) => distributions.map(({ filename }) => filename)),
                        }),
                    );
                    const args = ['push', held.file, ...Option.toArray(options.registry).flatMap((source) => ['--source', source])];
                    return yield* Match.value({ held: pushed.includes(path.basename(held.file)), dry: options.dryRun }).pipe(
                        Match.when({ held: true }, () => Effect.logInfo(`Skipped ${path.basename(held.file)}, ${registry} already holds it`)),
                        Match.when({ dry: true }, () => Effect.logInfo(`Would run yak ${args.join(' ')}; ${registry} holds ${pushed.join(', ') || 'no distribution'} of ${held.name} ${held.version}`)),
                        Match.orElse(() => reported('yak', args, PUBLISHED)),
                    );
                }).pipe(
                    Effect.catchTags({
                        HttpClientError: (error) => DistributionError.make({ subject: held.file, detail: error.message }),
                        SchemaError: (error) => DistributionError.make({ subject: held.file, detail: error.message }),
                    }),
                    Effect.provide(FetchHttpClient.layer),
                ),
        },
    },
    uxp: {
        manifest: 'manifest.json',
        archive: '.ccx',
        technology: 'uxp',
        platforms: {},
        read: (text) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ name, version }) => ({ name, version, platforms: [] }))),
        install: { command: 'UnifiedPluginInstallerAgent --install', run: upia },
    },
    cep: {
        manifest: 'CSXS/manifest.xml',
        archive: '.zxp',
        technology: 'cep',
        platforms: {},
        read: (text) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.map(Option.flatMap(({ attributes }) => Schema.decodeUnknownOption(Cep)(attributes))),
                Effect.map(Option.map(({ ExtensionBundleName, ExtensionBundleVersion }) => ({ name: ExtensionBundleName, version: ExtensionBundleVersion, platforms: [] }))),
                Effect.orElseSucceed(Option.none),
            ),
        install: { command: 'UnifiedPluginInstallerAgent --install', run: upia },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { built, DistributionError, HOSTS, type Host, installable, type Package };
