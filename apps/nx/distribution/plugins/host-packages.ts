import { glob } from 'node:fs/promises';
import { basename, matchesGlob } from 'node:path';
import { patch } from '@decimalturn/toml-patch';
import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { getDependencyConfigs } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Formatter, Match, Option, Path, Record, Schema, Stream, Struct } from 'effect';
import { Toml, Xml, Yaml, Zip } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify } from 'jsonc-parser';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields {
    readonly name: string;
    readonly version?: string;
    readonly platforms: readonly string[];
    readonly folders?: readonly string[];
    readonly references?: readonly { readonly name: string; readonly enabled: boolean }[];
    readonly installed?: boolean;
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
    readonly write?: (text: string, version: string) => Effect.Effect<string, DistributionError>;
    readonly build?: { readonly executor: string; readonly command: string };
    readonly install?: { readonly command: string; readonly run: (bundle: Package) => Effect.Effect<void, DistributionError, ChildProcessSpawner.ChildProcessSpawner> };
    readonly publish?: { readonly command: string; readonly run: (bundle: Package, options: Publish) => Effect.Effect<void, DistributionError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> };
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
const Printed = Schema.Trim.check(Schema.isNonEmpty());
const RepositoryGroups = Schema.Struct({ id: Schema.String });
const DistributionGroups = Schema.Struct({ platform: Schema.String });
const Plugin = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Version: Schema.optionalKey(Schema.Int), VersionName: Schema.String, Installed: Schema.optionalKey(Schema.Boolean), Plugins: References }));

// --- [ERRORS] --------------------------------------------------------------------------

class DistributionError extends Schema.TaggedError<DistributionError, { readonly brand: unique symbol }>()('DistributionError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause: unknown) => DistributionError.make({ subject, detail, cause });

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[], cwd?: string, echo?: { readonly stdout: NodeJS.WritableStream; readonly stderr: NodeJS.WritableStream }) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, { extendEnv: true, stdin: 'ignore', ...(cwd === undefined ? {} : { cwd }) });
            const sinks = Option.fromUndefinedOr(echo);
            const read = (stream: typeof handle.stdout, sink: Option.Option<NodeJS.WritableStream>) => Stream.mkString(Stream.tap(Stream.decodeText(stream), (chunk) => Option.match(sink, { onNone: () => Effect.void, onSome: (target) => Effect.sync(() => target.write(chunk)) })));
            const [stdout, stderr, code] = yield* Effect.all([read(handle.stdout, Option.map(sinks, Struct.get('stdout'))), read(handle.stderr, Option.map(sinks, Struct.get('stderr'))), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* DistributionError.make({ subject: [command, ...args].join(' '), detail: Option.isSome(sinks) ? `exited ${code}` : `exited ${code}: ${[stdout, stderr].join('\n').trim()}` });
        }),
    ).pipe(Effect.catchTag('PlatformError', DistributionError.of([command, ...args].join(' '), 'does not start')));

const reported = (command: string, args: readonly string[], signal: RegExp) =>
    Effect.flatMap(spawned(command, args), (stdout) =>
        Option.match(Option.fromNullishOr(signal.exec(stdout)), {
            onSome: ([line]) => Effect.logInfo(line),
            onNone: () => DistributionError.make({ subject: [command, ...args].join(' '), detail: `printed no line matching ${signal.source}: ${stdout.trim()}` }),
        }),
    );

const upia = (bundle: Package) => Effect.andThen(reported(UPIA, ['--remove', bundle.name], REMOVED), reported(UPIA, ['--install', bundle.file], INSTALLED));

const yak = (bundle: Package) =>
    Effect.flatMap(spawned('dotnet', ['msbuild', `-getProperty:${YAK_EXECUTABLE}`], bundle.root), (stdout) =>
        Option.match(Schema.decodeOption(Printed)(stdout), {
            onSome: Effect.succeed,
            onNone: () => DistributionError.make({ subject: bundle.root, detail: `evaluates no ${YAK_EXECUTABLE}, which MSBuild sets from the Rhino install it resolves; install Rhino or set RhinoInstallPath` }),
        }),
    );

const entry = (file: string, name: string) =>
    Effect.flatMap(FileSystem.FileSystem, (fs) =>
        fs.stream(file).pipe(
            Stream.pipeThroughChannel(Zip.decode()),
            Stream.mapEffect((member) => (matchesGlob(member.header.path, name) ? Effect.asSome(Effect.map(Stream.mkString(Stream.decodeText(member.body)), (text) => ({ at: member.header.path, text }))) : Effect.succeedNone)),
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
                DistributionError.of(project, 'cannot list its build outputs'),
            ),
        ),
    );
    const packages = yield* Effect.forEach(
        Array.dedupe(written.flat()).flatMap((file) => HOSTS.filter((host) => host.archive !== undefined && file.endsWith(host.archive)).map((host) => ({ file: path.join(context.root, file), host }))),
        Effect.fnUntraced(function* ({ file, host }) {
            const manifest = yield* entry(file, host.manifest);
            const fields = Option.isSome(manifest) ? yield* host.read(manifest.value.text, path.join(file, manifest.value.at)) : Option.none<Fields>();
            const version = Option.flatMap(fields, (read) => Option.fromUndefinedOr(read.version));
            return Option.map(Option.all({ read: fields, version }), ({ read, version: known }): Package => ({ ...read, version: known, host, file, root: path.join(context.root, node.data.root) }));
        }),
    );
    return { project, outputs: outputs.flatMap(({ paths }) => paths), configurations: Array.dedupe(selected.flatMap((target) => Record.keys(targets[target]?.configurations ?? {}))), packages: Array.getSomes(packages) };
});

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
                Effect.mapBoth({ onFailure: DistributionError.of(file, 'does not decode'), onSuccess: ({ id, version, platforms, build }) => Option.some({ name: id, version, platforms: build?.generated?.platforms ?? platforms ?? [] }) }),
            ),
        write: (text, version) => Effect.mapBoth(Effect.fromResult(Toml.parse(text)), { onFailure: DistributionError.of(EXTENSION, 'does not parse as TOML'), onSuccess: (document) => patch(text, { ...document, version }) }),
        build: { executor: 'extension-build', command: 'blender --command extension build' },
        install: {
            command: 'blender --command extension install-file',
            run: (bundle) =>
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
                                    Array.findFirst(listing.split(REPOSITORY), (section) => section.includes(`\n  ${bundle.name} [installed]`)),
                                    (section) => Option.map(Schema.decodeUnknownOption(RepositoryGroups)(REPOSITORY_ID.exec(section)?.groups), ({ id }) => id),
                                ),
                                () => 'user_default',
                            ),
                            '--enable',
                            bundle.file,
                        ],
                        new RegExp(`^STATUS (Installed|Reinstalled) "${RegExp.escape(bundle.name)}"$`, 'mu'),
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
                    platforms: Array.filter(Option.toArray(Option.map(Schema.decodeUnknownOption(DistributionGroups)(DISTRIBUTION.exec(file)?.groups), ({ platform }) => platform)), (platform) => platform !== 'any'),
                })),
            ),
        install: { command: 'yak install', run: (bundle) => Effect.flatMap(yak(bundle), (executable) => reported(executable, ['install', bundle.file], new RegExp(`^Successfully installed ${RegExp.escape(bundle.name)}`, 'mu'))) },
        publish: {
            command: 'yak push',
            run: (bundle, options) =>
                Effect.gen(function* () {
                    const path = yield* Path.Path;
                    const registry = Option.getOrElse(options.registry, () => YAK);
                    const pushed = yield* Effect.flatMap(
                        HttpClient.get(`${registry}/versions/${bundle.name}/${bundle.version}`),
                        HttpClientResponse.matchStatus({
                            404: () => Effect.succeed(Array.empty<string>()),
                            orElse: (response) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Pushed)), ({ distributions }) => distributions.map(({ filename }) => filename)),
                        }),
                    );
                    const args = ['push', bundle.file, ...Option.toArray(options.registry).flatMap((source) => ['--source', source])];
                    return yield* Match.value({ present: pushed.includes(path.basename(bundle.file)), dry: options.dryRun }).pipe(
                        Match.when({ present: true }, () => Effect.logInfo(`Skipped ${path.basename(bundle.file)}, ${registry} already holds it`)),
                        Match.when({ dry: true }, () => Effect.logInfo(`Would run yak ${args.join(' ')}; ${registry} holds ${Array.match(pushed, { onEmpty: () => 'no distribution', onNonEmpty: (names) => names.join(', ') })} of ${bundle.name} ${bundle.version}`)),
                        Match.orElse(() => Effect.flatMap(yak(bundle), (executable) => reported(executable, args, PUBLISHED))),
                    );
                }).pipe(
                    Effect.catchTags({
                        HttpClientError: DistributionError.of(bundle.file, 'does not reach the Yak server'),
                        SchemaError: DistributionError.of(bundle.file, 'reads an unexpected Yak server reply'),
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
                onFailure: DistributionError.of(file, 'does not decode'),
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
                onFailure: DistributionError.of(file, 'does not decode'),
                onSuccess: ({ VersionName, Installed, Plugins }) => Option.some({ name: basename(file, '.uplugin'), version: VersionName, platforms: [], installed: Installed === true, references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        write: (text, version) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: DistributionError.of('uplugin', 'does not decode'),
                onSuccess: ({ Version }) => applyEdits(text, [...modify(text, ['VersionName'], version, {}), ...modify(text, ['Version'], (Version ?? 0) + 1, {})]),
            }),
        build: { executor: 'unreal-build', command: 'RunUAT.sh BuildPlugin' },
    },
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { built, DistributionError, EXTENSION, type Fields, HOSTS, type Host, type Package, type Publish, RELEASE, spawned, UNREAL };
