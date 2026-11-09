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
    readonly root: string;
}

interface Publish {
    readonly dryRun: boolean;
    readonly registry: Option.Option<string>;
}

interface Host {
    readonly manifest: string;
    readonly archive: string;
    readonly technology: string;
    readonly platforms: Partial<Record<NodeJS.Platform, string>>;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>>;
    readonly install: { readonly command: string; readonly run: (held: Package) => Effect.Effect<void, DistributionError, ChildProcessSpawner.ChildProcessSpawner> };
    readonly publish?: { readonly command: string; readonly run: (held: Package, options: Publish) => Effect.Effect<void, DistributionError, Path.Path | ChildProcessSpawner.ChildProcessSpawner> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE = 'release';
const UPIA = '/Library/Application Support/Adobe/Adobe Desktop Common/RemoteComponents/UPI/UnifiedPluginInstallerAgent/UnifiedPluginInstallerAgent.app/Contents/MacOS/UnifiedPluginInstallerAgent';
const YAK = 'https://yak.rhino3d.com';
const YAK_EXECUTABLE = '_YakExecutable';
const REMOVED = /^(?:Removal Successful.*|.*status = -406)$/mu;
const INSTALLED = /^Installation Successful.*$/mu;
const PUBLISHED = /^Successfully published.*$/mu;
const REPOSITORY = /^Repository: /mu;
const REPOSITORY_ID = /\(id=(?<id>[^)]+)\)/u;
const DISTRIBUTION = /-(?<platform>\w+)\.yak$/u;
const UPSTREAM = /^\^/u;
const TOKEN = /\{(?<token>[^}]+)\}/gu;
const ASSETS = 'query($owner: String!, $name: String!, $tag: String!) { repository(owner: $owner, name: $name) { release(tagName: $tag) { releaseAssets(first: 100) { nodes { name } } } } }';

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
const Release = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ releaseAssets: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ name: Schema.String })) }) })) }) }) }));

// --- [ERRORS] --------------------------------------------------------------------------

class DistributionError extends Schema.TaggedError<DistributionError, { readonly brand: unique symbol }>()('DistributionError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[], cwd?: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, { extendEnv: true, stdin: 'ignore', ...(cwd === undefined ? {} : { cwd }) });
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

const yak = (held: Package) =>
    Effect.flatMap(spawned('dotnet', ['msbuild', `-getProperty:${YAK_EXECUTABLE}`], held.root), (stdout) =>
        Option.match(
            Option.liftPredicate(stdout.trim(), (found) => found.length > 0),
            {
                onSome: Effect.succeed,
                onNone: () => DistributionError.make({ subject: held.root, detail: `evaluates no ${YAK_EXECUTABLE}, which MSBuild sets from the Rhino install it resolves; install Rhino or set RhinoInstallPath` }),
            },
        ),
    );

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

const built = Effect.fnUntraced(function* (context: ExecutorContext, configuration: Option.Option<string>) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const targets = node.data.targets ?? {};
    const running = Record.fromEntries(
        Record.values(context.taskGraph?.tasks ?? {})
            .filter(({ target }) => target.project === project)
            .map(({ target }) => [target.target, target.configuration] as const),
    );
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
        const chosen = Option.getOrUndefined(Option.orElse(configuration, () => Option.fromUndefinedOr(running[target] ?? targets[target]?.defaultConfiguration)));
        return getOutputsForTargetAndConfiguration({ project, target, ...(chosen === undefined ? {} : { configuration: chosen }) }, {}, node);
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
            return Option.map(fields, (held): Package => ({ ...held, host, file, root: path.join(context.root, node.data.root) }));
        }),
    );
    return { project, outputs, configurations: Array.dedupe(selected.flatMap((target) => Record.keys(targets[target]?.configurations ?? {}))), packages: Array.getSomes(packages) };
});

const installable = (held: Package) => held.platforms.length === 0 || Option.exists(Option.fromUndefinedOr(held.host.platforms[process.platform]), (os) => held.platforms.some((platform) => platform === os || platform === `${os}-${process.arch}`));

const tagged = (context: ExecutorContext, project: string, version: string) =>
    Effect.flatMap(Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern).pipe(Effect.mapError(() => DistributionError.make({ subject: 'nx.json', detail: 'states no release.releaseTag.pattern, which names the GitHub release each package attaches to' }))), (pattern) => {
        const values: Readonly<Record<string, string>> = { projectName: project, version };
        return Array.match(
            [...pattern.matchAll(TOKEN)].flatMap(({ groups }) => Option.toArray(Option.filter(Option.fromUndefinedOr(groups?.token), (token) => !(token in values)))),
            {
                onEmpty: () => Effect.succeed(pattern.replaceAll(TOKEN, (_, token: string) => values[token] ?? token)),
                onNonEmpty: (tokens) => DistributionError.make({ subject: 'nx.json', detail: `release.releaseTag.pattern ${pattern} names ${tokens.join(', ')}, which an independent project release does not fill` }),
            },
        );
    });

const attached = Effect.fnUntraced(function* (held: Package, tag: string, workspace: string, options: Publish) {
    const path = yield* Path.Path;
    const asset = path.basename(held.file);
    const { data } = yield* Effect.flatMap(spawned('gh', ['api', 'graphql', '-F', 'owner={owner}', '-F', 'name={repo}', '-f', `tag=${tag}`, '-f', `query=${ASSETS}`], workspace), (stdout) =>
        Schema.decodeEffect(Release)(stdout).pipe(Effect.mapError((error) => DistributionError.make({ subject: `gh api graphql release ${tag}`, detail: error.message }))),
    );
    const release = Option.fromNullishOr(data.repository.release);
    return yield* Match.value({ held: Option.exists(release, ({ releaseAssets }) => releaseAssets.nodes.some(({ name }) => name === asset)), dry: options.dryRun, open: Option.isSome(release) }).pipe(
        Match.when({ held: true }, () => Effect.logInfo(`Skipped ${asset}, GitHub release ${tag} already holds it`)),
        Match.when({ dry: true }, ({ open }) => Effect.logInfo(`Would run gh release upload ${tag} ${held.file}; GitHub release ${tag} ${open ? 'exists' : 'is created by nx release before publishing'}`)),
        Match.when({ open: false }, () => DistributionError.make({ subject: tag, detail: `has no GitHub release to attach ${asset} to; nx release creates it when a changelog sets createRelease` })),
        Match.orElse(() => Effect.andThen(spawned('gh', ['release', 'upload', tag, held.file], workspace), Effect.logInfo(`Attached ${asset} to GitHub release ${tag}`))),
    );
});

const released = Effect.fnUntraced(function* (context: ExecutorContext, held: Package, options: Publish) {
    const project = yield* Effect.fromNullishOr(context.projectName);
    yield* attached(held, yield* tagged(context, project, held.version), context.root, options);
    yield* Option.match(Option.fromUndefinedOr(held.host.publish), { onNone: () => Effect.void, onSome: (channel) => channel.run(held, options) });
});

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
        install: { command: 'yak install', run: (held) => Effect.flatMap(yak(held), (executable) => reported(executable, ['install', held.file], new RegExp(`^Successfully installed ${RegExp.escape(held.name)}`, 'mu'))) },
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
                        Match.orElse(() => Effect.flatMap(yak(held), (executable) => reported(executable, args, PUBLISHED))),
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

export { built, DistributionError, HOSTS, type Host, installable, type Package, RELEASE, released };
