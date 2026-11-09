import { glob } from 'node:fs/promises';
import { basename, dirname, matchesGlob } from 'node:path';
import { patch } from '@decimalturn/toml-patch';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { getDependencyConfigs } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Match, Option, Path, Record, Schema, Stream } from 'effect';
import { Toml, Xml, Yaml, Zip } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify } from 'jsonc-parser';
import { parse } from 'plist';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields {
    readonly name: string;
    readonly version?: string;
    readonly platforms: readonly string[];
    readonly folders?: readonly string[];
    readonly references?: readonly { readonly name: string; readonly enabled: boolean }[];
    readonly installed?: boolean;
    readonly feed?: string;
}

interface Package extends Fields {
    readonly version: string;
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
    readonly archive?: string;
    readonly technology: string;
    readonly platforms: Partial<Record<NodeJS.Platform, string>>;
    readonly standalone?: true;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>, DistributionError>;
    readonly packaged?: { readonly manifest: string; readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>, DistributionError> };
    readonly write?: (text: string, version: string) => Effect.Effect<string, DistributionError>;
    readonly build?: { readonly executor: string; readonly command: string };
    readonly install?: { readonly command: string; readonly run: (held: Package) => Effect.Effect<void, DistributionError, ChildProcessSpawner.ChildProcessSpawner> };
    readonly publish?: { readonly command: string; readonly run: (held: Package, options: Publish) => Effect.Effect<void, DistributionError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE = 'release';
const EXTENSION = 'blender_manifest.toml';
const UPIA = '/Library/Application Support/Adobe/Adobe Desktop Common/RemoteComponents/UPI/UnifiedPluginInstallerAgent/UnifiedPluginInstallerAgent.app/Contents/MacOS/UnifiedPluginInstallerAgent';
const YAK = 'https://yak.rhino3d.com';
const YAK_EXECUTABLE = 'YakExecutable';
const REMOVED = /^(?:Removal Successful.*|.*status = -406)$/mu;
const INSTALLED = /^Installation Successful.*$/mu;
const PUBLISHED = /^Successfully published.*$/mu;
const REPOSITORY = /^Repository: /mu;
const REPOSITORY_ID = /\(id=(?<id>[^)]+)\)/u;
const DISTRIBUTION = /-(?<platform>\w+)\.yak(?=\/|$)/u;
const UNREAL: Partial<Record<NodeJS.Platform, string>> = { darwin: 'Mac', linux: 'Linux' };
const STAPLED = '*.app/Contents/CodeResources';
const FEED = /^https:\/\/github\.com\/(?<owner>[^/]+)\/(?<repo>[^/]+)\/releases\/download\/(?<tag>[^/]+)\/(?<asset>[^/]+)$/u;
const FEED_RELEASE = 'query($owner: String!, $name: String!, $tag: String!, $asset: String!) { repository(owner: $owner, name: $name) { release(tagName: $tag) { releaseAssets(first: 1, name: $asset) { nodes { name } } } } }';

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
const References = Schema.optionalKey(Schema.Array(Schema.Struct({ Name: Schema.String, Enabled: Schema.optionalKey(Schema.Boolean) })));
const Project = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Plugins: References, AdditionalPluginDirectories: Schema.optionalKey(Schema.Array(Schema.String)) }));
const Plugin = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Version: Schema.optionalKey(Schema.Int), VersionName: Schema.String, Installed: Schema.optionalKey(Schema.Boolean), Plugins: References }));
const Bundle = Schema.Struct({ CFBundleName: Schema.String, CFBundleShortVersionString: Schema.String, SUFeedURL: Schema.optionalKey(Schema.String) });
const Feed = Schema.Struct({ owner: Schema.String, repo: Schema.String, tag: Schema.String, asset: Schema.String });
const Published = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ releaseAssets: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ name: Schema.String })) }) })) }) }) }));

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
            Stream.mapEffect((found) => (matchesGlob(found.header.path, name) ? Effect.asSome(Effect.map(Stream.mkString(Stream.decodeText(found.body)), (text) => ({ at: found.header.path, text }))) : Effect.succeedNone)),
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
                .flatMap((name) => getDependencyConfigs({ project, target: name }, {}, context.projectGraph, Record.keys(targets)) ?? [])
                .filter(({ projects }) => projects?.includes(project) === true)
                .map(({ target }) => target),
        ]);
        return next.size === names.size ? names : reached(next);
    };
    const selected = [...reached(new Set(['build']))].filter((target) => target in targets);
    const declared = selected.filter((target) => targets[target]?.outputs !== undefined);
    const outputs = declared.map((target) => {
        const chosen = Option.getOrUndefined(Option.orElse(configuration, () => Option.fromUndefinedOr(running[target] ?? targets[target]?.defaultConfiguration)));
        return { target, paths: getOutputsForTargetAndConfiguration({ project, target, ...(chosen === undefined ? {} : { configuration: chosen }) }, {}, node) };
    });
    const written = yield* Effect.forEach(outputs, ({ paths }) =>
        Stream.runCollect(
            Stream.fromAsyncIterable(
                glob(
                    paths.filter((output) => !output.startsWith('!')).flatMap((output) => [output, path.join(output, '**')]),
                    { cwd: context.root, exclude: paths.filter((output) => output.startsWith('!')).map((output) => output.slice(1)) },
                ),
                (cause) => DistributionError.make({ subject: project, detail: `cannot list its build outputs: ${String(cause)}` }),
            ),
        ),
    );
    const packages = yield* Effect.forEach(
        Array.dedupe(written.flat()).flatMap((file) => HOSTS.filter((host) => host.archive !== undefined && file.endsWith(host.archive)).map((host) => ({ file: path.join(context.root, file), host }))),
        Effect.fnUntraced(function* ({ file, host }) {
            const reader = host.packaged ?? host;
            const found = yield* entry(file, reader.manifest);
            const fields = Option.isSome(found) ? yield* reader.read(found.value.text, path.join(file, found.value.at)) : Option.none<Fields>();
            const version = Option.flatMap(fields, (held) => Option.fromUndefinedOr(held.version));
            return Option.map(Option.all({ held: fields, version }), ({ held, version: known }): Package => ({ ...held, version: known, host, file, root: path.join(context.root, node.data.root) }));
        }),
    );
    return { project, outputs: outputs.flatMap(({ paths }) => paths), configurations: Array.dedupe(selected.flatMap((target) => Record.keys(targets[target]?.configurations ?? {}))), packages: Array.getSomes(packages) };
});

const sparkle = Effect.fnUntraced(function* (held: Package, options: Publish) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { owner, repo, tag, asset } = yield* Schema.decodeUnknownEffect(Feed)(FEED.exec(held.feed ?? '')?.groups).pipe(
        Effect.mapError(() => DistributionError.make({ subject: held.file, detail: `names SUFeedURL ${held.feed ?? '(none)'}, and the Sparkle feed publishes to a GitHub release asset (https://github.com/<owner>/<repo>/releases/download/<tag>/<file>)` })),
    );
    const repository = `${owner}/${repo}`;
    const release = yield* Effect.flatMap(spawned('gh', ['api', 'graphql', '-f', `owner=${owner}`, '-f', `name=${repo}`, '-f', `tag=${tag}`, '-f', `asset=${asset}`, '-f', `query=${FEED_RELEASE}`]), (stdout) =>
        Schema.decodeEffect(Published)(stdout).pipe(Effect.mapBoth({ onFailure: (error) => DistributionError.make({ subject: `gh api graphql release ${tag}`, detail: error.message }), onSuccess: ({ data }) => Option.fromNullishOr(data.repository.release) })),
    );
    const [staging, resolved] = yield* Effect.mapError(Effect.all([fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped()]), (error) => DistributionError.make({ subject: held.file, detail: error.message }));
    yield* spawned('xcodebuild', ['-resolvePackageDependencies', '-clonedSourcePackagesDirPath', resolved], held.root);
    const generator = path.join(resolved, 'artifacts', 'sparkle', 'Sparkle', 'bin', 'generate_appcast');
    const steps: readonly (readonly [string, ...string[]])[] = [
        ...(Option.isNone(release) ? [['gh', 'release', 'create', tag, '--repo', repository, '--prerelease', '--title', tag, '--notes', `Sparkle feed for ${held.name}`] as const] : []),
        ...(Option.exists(release, ({ releaseAssets }) => releaseAssets.nodes.length > 0) ? [['gh', 'release', 'download', tag, '--repo', repository, '--pattern', asset, '--dir', staging] as const] : []),
    ];
    return yield* options.dryRun
        ? Effect.forEach([...steps, ['cp', held.file, staging], [generator, staging], ['gh', 'release', 'upload', tag, '--repo', repository, '--clobber', `${staging}/*`]], (step) => Effect.logInfo(`Would run ${step.join(' ')}`), { discard: true })
        : Effect.gen(function* () {
              yield* Effect.forEach(steps, ([command, ...args]) => spawned(command, args), { discard: true });
              yield* fs.copyFile(held.file, path.join(staging, path.basename(held.file)));
              yield* Effect.flatMap(spawned(generator, [staging]), (stdout) => Effect.logInfo(stdout.trim()));
              const listed = yield* fs.readDirectory(staging);
              const kinds = yield* Effect.forEach(listed, (name) => fs.stat(path.join(staging, name)));
              const written = Array.zip(listed, kinds).flatMap(([name, { type }]) => (type === 'File' ? [path.join(staging, name)] : []));
              yield* spawned('gh', ['release', 'upload', tag, '--repo', repository, '--clobber', ...written]);
              return yield* Effect.logInfo(`Published ${written.map((file) => path.basename(file)).join(', ')} to the Sparkle feed ${held.feed ?? ''}`);
          }).pipe(Effect.catchTag('PlatformError', (error) => DistributionError.make({ subject: staging, detail: error.message })));
}, Effect.scoped);

// --- [HOSTS] ---------------------------------------------------------------------------

const HOSTS: readonly Host[] = [
    {
        manifest: EXTENSION,
        archive: '.zip',
        technology: 'blender',
        platforms: { darwin: 'macos', win32: 'windows', linux: 'linux' },
        read: (text, file) =>
            Effect.fromResult(Toml.parse(text)).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Extension)),
                Effect.mapBoth({ onFailure: (error) => DistributionError.make({ subject: file, detail: error.message }), onSuccess: ({ id, version, platforms, build }) => Option.some({ name: id, version, platforms: build?.generated?.platforms ?? platforms ?? [] }) }),
            ),
        write: (text, version) => Effect.mapBoth(Effect.fromResult(Toml.parse(text)), { onFailure: (error) => DistributionError.make({ subject: EXTENSION, detail: error.message }), onSuccess: (held) => patch(text, { ...held, version }) }),
        build: { executor: 'extension-build', command: 'blender --command extension build' },
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
    {
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
    {
        manifest: 'manifest.json',
        archive: '.ccx',
        technology: 'uxp',
        platforms: {},
        read: (text) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ name, version }) => ({ name, version, platforms: [] }))),
        install: { command: 'UnifiedPluginInstallerAgent --install', run: upia },
    },
    {
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
    {
        manifest: '*.uproject',
        technology: 'unreal',
        platforms: UNREAL,
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Project)(text), {
                onFailure: (error) => DistributionError.make({ subject: file, detail: error.message }),
                onSuccess: ({ Plugins, AdditionalPluginDirectories }) => Option.some({ name: basename(file, '.uproject'), platforms: [], folders: ['Plugins', ...(AdditionalPluginDirectories ?? [])], references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        build: { executor: 'unreal-build', command: 'RunUBT.sh -TargetType=Editor' },
    },
    {
        manifest: '*.uplugin',
        archive: '.zip',
        technology: 'unreal',
        platforms: UNREAL,
        standalone: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: (error) => DistributionError.make({ subject: file, detail: error.message }),
                onSuccess: ({ VersionName, Installed, Plugins }) => Option.some({ name: basename(file, '.uplugin'), version: VersionName, platforms: [], installed: Installed === true, references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        write: (text, version) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: (error) => DistributionError.make({ subject: 'uplugin', detail: error.message }),
                onSuccess: ({ Version }) => applyEdits(text, [...modify(text, ['VersionName'], version, {}), ...modify(text, ['Version'], (Version ?? 0) + 1, {})]),
            }),
        build: { executor: 'unreal-build', command: 'RunUAT.sh BuildPlugin' },
    },
    {
        manifest: '*.xcodeproj/project.xcproj',
        archive: '.zip',
        technology: 'macos',
        platforms: {},
        read: (_, file) => Effect.succeedSome({ name: basename(dirname(file), '.xcodeproj'), platforms: [] }),
        packaged: {
            manifest: '*.app/Contents/Info.plist',
            read: (text) =>
                Effect.succeed(
                    Option.map(Option.flatMap(Option.liftThrowable(parse)(text), Schema.decodeUnknownOption(Bundle)), ({ CFBundleName, CFBundleShortVersionString, SUFeedURL }) => ({
                        name: CFBundleName,
                        version: CFBundleShortVersionString,
                        platforms: [],
                        ...(SUFeedURL === undefined ? {} : { feed: SUFeedURL }),
                    })),
                ),
        },
        publish: {
            command: 'generate_appcast',
            run: (held, options) =>
                Effect.flatMap(
                    Effect.mapError(entry(held.file, STAPLED), (error) => DistributionError.make({ subject: held.file, detail: error.message })),
                    (ticket) => (Option.isSome(ticket) && held.feed !== undefined ? sparkle(held, options) : Effect.logInfo(`Skipped the Sparkle feed for ${basename(held.file)}, which holds no stapled notarization ticket or names no SUFeedURL`)),
                ),
        },
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { built, DistributionError, EXTENSION, type Fields, HOSTS, type Host, type Package, type Publish, RELEASE, spawned, UNREAL };
