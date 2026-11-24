import { hash } from 'node:crypto';
import { constants, openAsBlob } from 'node:fs';
import { homedir } from 'node:os';
import { basename, dirname, join, matchesGlob, posix, resolve } from 'node:path';
import { arch, argv, platform } from 'node:process';
import { patch } from '@decimalturn/toml-patch';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext, quoteShellArg } from '@nx/devkit/internal';
import { HOSTS as UXP } from '@rasm/uxp';
import { Array, Cause, Config, Console, Duration, Effect, FileSystem, Function, Option, Path, pipe, Record, Result, Schedule, Schema, SemVer, Stream, String, Struct, UndefinedOr } from 'effect';
import { Ini, Toml, Yaml, Zip } from 'effect/encoding';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import type { ChildProcessSpawner } from 'effect/process';
import { applyEdits, modify, parse } from 'jsonc-parser';
import { parseDocument, Scalar } from 'yaml';
import { packaged } from '#/executor';
import { above, PLATFORM, spawned } from '#/inference';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner;
type Step<A = void> = Effect.Effect<A, Error, Services>;
type Stored<A = void> = Effect.Effect<A, Error, Services | HttpClient.HttpClient>;

interface Fields {
    readonly name: string;
    readonly version?: string;
    readonly foreign?: boolean;
    readonly platforms?: readonly string[];
    readonly folders?: readonly string[];
    readonly references?: readonly { readonly name: string; readonly enabled: boolean }[];
    readonly installed?: boolean;
    readonly packages?: NonNullable<Plugin['nx']>['packages'];
}

interface Found {
    readonly host: Host;
    readonly at: string;
    readonly name: string;
    readonly version: string;
    readonly foreign: boolean;
    readonly platforms: readonly string[];
    readonly text: string;
    readonly names: readonly string[];
}

interface Host {
    readonly manifest: string;
    readonly technology: string;
    readonly archive?: string;
    readonly platforms?: readonly string[];
    readonly standalone?: true;
    readonly hosted?: true;
    readonly configurations?: readonly string[];
    readonly engine?: { readonly fields: readonly string[] };
    readonly operations?: Partial<Record<'check' | 'test' | 'pack' | 'install', { readonly command: string; readonly options: Readonly<Record<string, unknown>> }>>;
    readonly output: string;
    readonly read: (text: string, file: string) => Effect.Effect<Option.Option<Fields>, Cause.UnknownError>;
    readonly write?: (text: string, version: string, file: string) => Effect.Effect<string, Cause.UnknownError>;
    readonly build?: {
        readonly executor: string;
        readonly command: string;
        readonly help?: string;
        readonly lock?: string;
        readonly runtime?: readonly string[];
        readonly outputs?: (fields: Fields) => readonly string[];
        readonly pack?: (folder: Found, file: string, words: readonly string[]) => Step;
    };
    readonly install?: { readonly command: string; readonly help?: string; readonly platforms?: readonly string[]; readonly unpacked?: true; readonly run: (found: Found, words: readonly string[]) => Step };
    readonly publish?: { readonly store: string; readonly command: string; readonly credential: string; readonly held: (found: Found, store: string, token: string) => Stored<boolean>; readonly push: (found: Found, store: string, token: string, words: readonly string[]) => Stored<Option.Option<string>> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSION = 'blender_manifest.toml';
const PYTHON = ['run', '--no-project', 'python', '-c'] as const;
const LOCATED = "shutil.which('blender_ext.py', os.F_OK)";
const BUILDER = [...PYTHON, `import os, runpy, shutil, sys; sys.argv[0] = ${LOCATED}; runpy.run_path(sys.argv[0], run_name='__main__')`] as const;
const BLENDER: Partial<Record<NodeJS.Platform, string>> = { darwin: 'macos', linux: 'linux', win32: 'windows' };
const UNREAL: Partial<Record<NodeJS.Platform, { readonly name: string; readonly settings: readonly string[] }>> = {
    darwin: { name: 'Mac', settings: ['Library', 'Application Support', 'Epic'] },
    linux: { name: 'Linux', settings: ['.config', 'Epic'] },
};
const ENGINE_PATH = /[/\\]/u;
const ANALYSIS = ['-StaticAnalyzer=Default', '-StaticAnalyzerProjectOnly', '-DisableUnity', '-NoPCH'] as const;
const FOLDERS = { build: 'package', test: 'report', pack: 'archive' } as const;
const CREDENTIALS = { blender: 'BLENDER_EXTENSIONS_TOKEN', yak: 'YAK_TOKEN', chrome: 'CHROME_WEB_STORE_TOKEN' } as const;
const WEBSTORE = { api: 'https://chromewebstore.googleapis.com', publisher: 'CHROME_WEB_STORE_PUBLISHER', poll: Duration.seconds(2), upload: Duration.minutes(5) } as const;
const ADOBE: Partial<Record<NodeJS.Platform, Config.Config<string>>> = {
    darwin: Config.succeed(join(homedir(), 'Library', 'Application Support', 'Adobe')),
    win32: Config.map(Config.String('APPDATA'), (roaming) => join(roaming, 'Adobe')),
};

// --- [MODELS] --------------------------------------------------------------------------

const Platforms = Schema.optionalKey(Schema.Array(Schema.String));
const Extension = Schema.Struct({ schema_version: Schema.String, id: Schema.String, version: Schema.String, platforms: Platforms, build: Schema.optionalKey(Schema.Struct({ generated: Schema.optionalKey(Schema.Struct({ platforms: Platforms })) })) });
const Listed = Schema.Struct({ data: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String, platforms: Platforms })) });
const Yak = Schema.Struct({ name: Schema.String, version: Schema.String, authors: Schema.Array(Schema.String), description: Schema.String });
const Pushed = Schema.Struct({ distributions: Schema.Array(Schema.Struct({ filename: Schema.String })) });
const Uxp = Schema.fromJsonString(Schema.Struct({ manifestVersion: Schema.Finite, id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.Struct({ app: Schema.Literals(Struct.keys(UXP)) }) }));
const Chrome = Schema.fromJsonString(Schema.Struct({ manifest_version: Schema.Literal(3), version: Schema.String, key: Schema.optionalKey(Schema.Uint8ArrayFromBase64) }));
const Revision = Schema.optionalKey(Schema.Struct({ distributionChannels: Schema.optionalKey(Schema.Array(Schema.Struct({ crxVersion: Schema.String }))) }));
const Upload = Schema.optionalKey(Schema.Literals(['UPLOAD_STATE_UNSPECIFIED', 'SUCCEEDED', 'IN_PROGRESS', 'FAILED', 'NOT_FOUND']));
const Status = Schema.Struct({ publishedItemRevisionStatus: Revision, submittedItemRevisionStatus: Revision, lastAsyncUploadState: Upload });
const Uploaded = Schema.Struct({ uploadState: Upload });
const Registry = Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: Schema.String, path: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)])) });
const References = Schema.optionalKey(Schema.Array(Schema.Struct({ Name: Schema.String, Enabled: Schema.optionalKey(Schema.Boolean) })));
const Project = Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Plugins: References, AdditionalPluginDirectories: Schema.optionalKey(Schema.Array(Schema.String)) }));
const Linked = Schema.Record(Schema.String, Schema.Struct({ project: Schema.String, target: Schema.String, configuration: Schema.optionalKey(Schema.String) })).annotate({
    description: 'Folders of a .uplugin, each mapped to the Nx target that installs a package under it in a folder named for the engine platform',
});
class Plugin extends Schema.Opaque<Plugin, unknown>()(
    Schema.fromJsonString(Schema.Struct({ FileVersion: Schema.Int, Version: Schema.optionalKey(Schema.Int), VersionName: Schema.optionalKey(Schema.String), Installed: Schema.optionalKey(Schema.Boolean), Plugins: References, nx: Schema.optionalKey(Schema.Struct({ packages: Linked })) })),
) {}
const Association = Schema.Struct({ EngineAssociation: Schema.optionalKey(Schema.String) });
const Engined = Schema.fromJsonString(Association);
const Launcher = Schema.fromJsonString(Schema.Struct({ InstallationList: Schema.Array(Schema.Struct({ AppName: Schema.String, InstallLocation: Schema.String })) }));
const Registered = Schema.Struct({ Installations: Schema.Record(Schema.String, Schema.String) });
const Built = Schema.fromJsonString(Schema.Struct({ MajorVersion: Schema.Int, MinorVersion: Schema.Int }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const exact = (subject: string, version: string) =>
    Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${subject} takes a major.minor.patch version alone, which ${version} is not`)),
    );

const webstore = Effect.fnUntraced(function* ({ at, text }: Found, token: string) {
    const [{ key }, publisher, client] = yield* Effect.all([
        Effect.mapError(Schema.decodeEffect(Chrome)(text), (cause) => new Cause.UnknownError(cause, `${at} does not decode`)),
        Effect.mapError(Config.NonEmptyString(WEBSTORE.publisher), (cause) => new Cause.UnknownError(cause, `${WEBSTORE.publisher} names no Chrome Web Store publisher`)),
        HttpClient.HttpClient,
    ]);
    const authorized = HttpClient.filterStatusOk(HttpClient.mapRequest(client, HttpClientRequest.bearerToken(token)));
    return Option.map(Option.fromUndefinedOr(key), (bytes) => {
        const item = `publishers/${publisher}/items/${hash('sha256', bytes, 'hex')
            .slice(0, 32)
            .replaceAll(/./gu, (digit) => (Number.parseInt(digit, 16) + 10).toString(36))}`;
        const status = Effect.flatMap(authorized.get(`${WEBSTORE.api}/v2/${item}:fetchStatus`), HttpClientResponse.schemaBodyJson(Status));
        return {
            status,
            upload: HttpClientRequest.bodyFile(HttpClientRequest.post(`${WEBSTORE.api}/upload/v2/${item}:upload`), at, { contentType: 'application/zip' }).pipe(
                Effect.flatMap((request) => authorized.execute(request)),
                Effect.flatMap(HttpClientResponse.schemaBodyJson(Uploaded)),
            ),
            settled: Effect.map(Effect.repeat(status, { schedule: Schedule.spaced(WEBSTORE.poll), until: ({ lastAsyncUploadState }) => lastAsyncUploadState !== 'IN_PROGRESS' }), Struct.get('lastAsyncUploadState')),
            publish: authorized.post(`${WEBSTORE.api}/v2/${item}:publish`),
        };
    });
});

const routed = (file: string, text: string) =>
    Effect.findFirstFilter(
        HOSTS.filter(({ manifest }) => posix.matchesGlob(file, `**/${manifest}`)),
        (host) => host.read(text, file).pipe(Effect.map(Option.map((fields) => ({ file, host, fields }))), Effect.map(Result.fromOption(Function.constVoid))),
    );

const zipped = Effect.fnUntraced(function* (folder: string, names: readonly string[], file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = yield* Effect.forEach(names.toSorted(), (name) => Effect.map(fs.stat(path.join(folder, name)), (info) => ({ name, info })));
    yield* Stream.fromIterable(entries.filter(({ info }) => info.type === 'File')).pipe(
        Stream.map(({ name, info }) => Zip.fileStream(name.split(path.sep).join('/'), fs.stream(path.join(folder, name)), { mode: info.mode & (constants.S_IRWXU + constants.S_IRWXG + constants.S_IRWXO) })),
        Stream.pipeThroughChannel(Zip.encode()),
        Stream.run(fs.sink(file)),
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${file} does not write`)),
    );
});

const entry = (file: string, name: string) =>
    Effect.flatMap(FileSystem.FileSystem, (fs) =>
        fs.stream(file).pipe(
            Stream.pipeThroughChannel(Zip.decode()),
            Stream.mapEffect((member) =>
                matchesGlob(member.header.path, name)
                    ? pipe(
                          Stream.decodeText(member.body),
                          Stream.mkString,
                          Effect.map((text) => ({ at: member.header.path, text })),
                          Effect.asSome,
                      )
                    : Effect.succeedNone,
            ),
            Stream.filter(Option.isSome),
            Stream.runHead,
            Effect.mapBoth({ onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not read as a zip archive`), onSuccess: Option.flatten }),
        ),
    );

const engine = Effect.fnUntraced(function* (descriptor: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const host = yield* Effect.mapError(Effect.fromNullishOr(UNREAL[platform]), (cause) => new Cause.UnknownError(cause, `${platform} is no supported Unreal Engine host; supported: ${Object.keys(UNREAL).join(', ')}`));
    const settings = path.join(homedir(), ...host.settings);
    const declared = yield* Effect.flatMap(fs.readFileString(descriptor), Schema.decodeEffect(Engined));
    const named = Option.filter(Option.fromUndefinedOr(declared.EngineAssociation), String.isNonEmpty);
    const read = (at: readonly string[]) => Effect.option(fs.readFileString(path.join(settings, ...at)));
    const [registered, launched] = yield* Effect.all([read(['UnrealEngine', 'Install.ini']), read(['UnrealEngineLauncher', 'LauncherInstalled.dat'])]);
    const native = yield* Effect.filter(above(path.dirname(descriptor)), (at) => fs.exists(path.join(at, 'Engine', 'Build', 'Build.version')));
    const root = yield* Effect.mapError(
        Effect.fromOption(
            Option.firstSomeOf([
                Option.flatMap(named, (key) => Option.liftPredicate(path.resolve(path.dirname(descriptor), key), () => ENGINE_PATH.test(key))),
                Option.match(named, { onNone: () => Array.head(native), onSome: () => Option.none() }),
                Option.flatMap(Option.all([named, Option.flatMap(registered, (text) => Schema.decodeUnknownOption(Registered)(Ini.parse(text)))]), ([key, { Installations }]) => Option.orElse(Record.get(Installations, key), () => Record.get(Installations, `UE_${key}`))),
                Option.flatMap(Option.all([named, Option.flatMap(launched, (text) => Schema.decodeOption(Launcher)(text))]), ([key, { InstallationList }]) =>
                    Option.map(
                        Array.findFirst(InstallationList, ({ AppName }) => AppName === `UE_${key}`),
                        ({ InstallLocation }) => InstallLocation,
                    ),
                ),
            ]),
        ),
        (cause) => new Cause.UnknownError(cause, Option.match(named, { onNone: () => `${descriptor} names no engine, and no engine tree encloses it`, onSome: (key) => `${descriptor} names engine "${key}", which the engine tree around it, Install.ini, and LauncherInstalled.dat under ${settings} do not list` })),
    );
    const stamp = yield* Effect.mapError(fs.readFileString(path.join(root, 'Engine', 'Build', 'Build.version')), (cause) => new Cause.UnknownError(cause, `${descriptor} associates the engine at ${root}, which holds no Engine/Build/Build.version`));
    const { MajorVersion, MinorVersion } = yield* Schema.decodeEffect(Built)(stamp);
    return { root, stamp, platform: host.name, version: `${MajorVersion}.${MinorVersion}`, binaries: path.join(root, 'Engine', 'Binaries', host.name), scripts: path.join(root, 'Engine', 'Build', 'BatchFiles') };
});

const stated = (host: Host, at: string, text: string, file: string, names: readonly string[]) =>
    Effect.map(
        host.read(text, file),
        Option.flatMap(({ name, version, foreign = false, platforms = [] }) => Option.map(Option.fromUndefinedOr(version), (known): Found => ({ host, at, name, version: known, foreign, platforms, text, names }))),
    );

const located = Effect.fnUntraced(function* (context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { project, target, outputs, files: listed } = yield* packaged(context);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const files = listed.map((file) => path.join(context.root, file));
    const rows = HOSTS.filter(({ technology }) => node.data.metadata?.technologies?.includes(technology) === true);
    const sources = Array.isReadonlyArrayNonEmpty(outputs) ? files : (yield* Effect.promise(() => globWithWorkspaceContext(context.root, [posix.join(node.data.root, '**')]))).map((file) => path.join(context.root, file));
    const packages = yield* Effect.forEach(
        files.flatMap((file) => rows.filter(({ archive }) => archive !== undefined && file.endsWith(archive)).map((host) => ({ file, host }))),
        ({ file, host }) => Effect.flatMap(entry(file, `{,*/}${host.manifest}`), Option.match({ onNone: () => Effect.succeedNone, onSome: ({ at, text }) => stated(host, file, text, path.join(file, at), []) })),
    );
    const under = (folder: string) => sources.filter((other) => other.startsWith(`${folder}${path.sep}`)).map((other) => path.relative(folder, other));
    const folders = yield* Effect.forEach(
        sources.flatMap((file) => rows.filter(({ build, install }) => build?.pack !== undefined || install?.unpacked === true).map((host) => ({ host, file, folder: path.join(file, path.relative(host.manifest, '.')) }))).filter(({ host, file, folder }) => matchesGlob(path.relative(folder, file), host.manifest)),
        ({ host, file, folder }) => Effect.flatMap(fs.readFileString(file), (text) => stated(host, folder, text, file, under(folder))),
    );
    return { project, target, outputs, packages: Array.getSomes(packages), folders: Array.getSomes(folders) };
});

// --- [HOSTS] ---------------------------------------------------------------------------

const HOSTS: readonly Host[] = [
    {
        manifest: EXTENSION,
        technology: 'blender',
        archive: '.zip',
        output: '.',
        read: (text, file) =>
            Effect.fromResult(Toml.parse(text)).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Extension)),
                Effect.mapBoth({
                    onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not decode`),
                    onSuccess: ({ id, version, platforms, build }) => Option.some({ name: id, version, ...UndefinedOr.match(build?.generated?.platforms ?? platforms, { onUndefined: () => ({}), onDefined: (built) => ({ platforms: built, foreign: !built.includes(`${BLENDER[platform]}-${arch}`) }) }) }),
                }),
            ),
        write: (text, version, file) => Effect.mapBoth(Effect.fromResult(Toml.parse(text)), { onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not parse as TOML`), onSuccess: (document) => patch(text, { ...document, version }) }),
        build: {
            executor: 'extension-build',
            command: 'blender_ext.py build',
            help: ['uv', ...BUILDER, 'build', '--help'].map(quoteShellArg).join(' '),
            lock: 'uv.lock',
            runtime: [['uv', ...PYTHON, `import hashlib, os, shutil; print(hashlib.file_digest(open(${LOCATED}, 'rb'), 'sha256').hexdigest())`].map(quoteShellArg).join(' ')],
        },
        operations: { check: { command: 'blender_ext.py validate', options: { operation: 'check' } } },
        install: {
            command: 'blender --command extension install-file',
            help: 'blender --command extension install-file --help',
            run: ({ at, name }, words) =>
                Effect.flatMap(spawned(['blender', '--command', 'extension', 'install-file', '--repo', 'user_default', '--enable', at, ...words], { stdout: 'pipe' }), (stdout) =>
                    new RegExp(`^STATUS (Installed|Reinstalled) "${RegExp.escape(name)}"$`, 'mu').test(stdout) ? Effect.void : Effect.fail(new Cause.UnknownError(stdout, `${at}: blender exited 0 without installing it: ${stdout.trim()}`)),
                ),
        },
        publish: {
            store: 'https://extensions.blender.org',
            command: 'POST /api/v1/extensions/<id>/versions/upload/',
            credential: CREDENTIALS.blender,
            held: ({ name, version, platforms }, store) =>
                HttpClient.get(`${store}/api/v1/extensions/`).pipe(
                    Effect.flatMap(HttpClientResponse.filterStatusOk),
                    Effect.flatMap(HttpClientResponse.schemaBodyJson(Listed)),
                    Effect.map(({ data }) => data.some((listed) => listed.id === name && listed.version === version && Array.difference(platforms, listed.platforms ?? []).length === 0)),
                ),
            push: Effect.fnUntraced(function* ({ at, name }, store, token) {
                const archive = yield* Effect.tryPromise({ try: () => openAsBlob(at), catch: (cause) => new Cause.UnknownError(cause, `${at} does not open`) });
                const request = HttpClientRequest.post(`${store}/api/v1/extensions/${name}/versions/upload/`).pipe(HttpClientRequest.bearerToken(token), HttpClientRequest.bodyFormDataRecord({ version_file: new File([archive], basename(at)) }));
                return yield* Effect.flatMap(
                    HttpClient.execute(request),
                    HttpClientResponse.matchStatus({ 404: () => Effect.succeedSome(`${store} holds no extension ${name}: its first version uploads by hand at ${store}/submit/, and each later version publishes here`), orElse: (response) => Effect.as(HttpClientResponse.filterStatusOk(response), Option.none()) }),
                );
            }),
        },
    },
    {
        manifest: 'manifest.yml',
        technology: 'yak',
        archive: '.yak',
        configurations: ['debug'],
        output: '.',
        read: (text) => Effect.succeed(Option.map(Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Yak)), Struct.pick(['name', 'version']))),
        write: (text, version, file) =>
            pipe(
                Option.liftPredicate(parseDocument(text).get('version'), (node) => node instanceof Scalar),
                Option.flatMap(({ range }) => Option.fromNullishOr(range)),
                Effect.fromOption,
                Effect.mapBoth({ onFailure: (cause) => new Cause.UnknownError(cause, `${file} states no version`), onSuccess: ([start, end]) => `${text.slice(0, start)}${version}${text.slice(end)}` }),
            ),
        build: {
            executor: 'package',
            command: 'yak build',
            help: 'yak build --help',
            pack: Effect.fnUntraced(function* ({ at }, file, words) {
                const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const stage = yield* fs.makeTempDirectoryScoped();
                yield* fs.copy(at, stage);
                yield* spawned(['yak', 'build', ...words], { cwd: stage });
                yield* Effect.forEach(
                    (yield* fs.readDirectory(stage)).filter((name) => path.extname(name) === '.yak'),
                    (name) => fs.copyFile(path.join(stage, name), path.join(path.dirname(file), name)),
                    { discard: true },
                );
            }, Effect.scoped),
        },
        install: { command: 'yak install', help: 'yak install --help', platforms: ['darwin', 'win32'], run: ({ at, name }, words) => Effect.asVoid(Effect.andThen(spawned(['yak', 'uninstall', name]), spawned(['yak', 'install', at, ...words]))) },
        publish: {
            store: 'https://yak.rhino3d.com',
            command: 'yak push',
            credential: CREDENTIALS.yak,
            held: ({ at, name, version }, store) =>
                Effect.flatMap(
                    HttpClient.get(`${store}/versions/${name}/${version}`),
                    HttpClientResponse.matchStatus({
                        404: () => Effect.succeed(false),
                        orElse: (response) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Pushed)), ({ distributions }) => distributions.some(({ filename }) => filename.toLowerCase() === basename(at).toLowerCase())),
                    }),
                ),
            push: ({ at }, store, token, words) => Effect.as(spawned(['yak', 'push', at, '--source', store, ...words], { env: { [CREDENTIALS.yak]: token } }), Option.none()),
        },
    },
    {
        manifest: 'manifest.json',
        technology: 'uxp',
        archive: '.ccx',
        output: '.',
        read: (text) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ id, version }) => ({ name: id, version }))),
        write: (text, version, file) => Effect.as(exact(file, version), applyEdits(text, modify(text, ['version'], version, {}))),
        build: { executor: 'package', command: 'a zip archive named .ccx', pack: ({ at, names }, file) => zipped(at, names, file) },
        install: {
            command: "a copy into the per-user UXP plug-in folder in place of its earlier folders and CEP extension, and the plug-in's row in the PluginsInfo registry of a host that reads one",
            platforms: Object.keys(ADOBE),
            unpacked: true,
            run: Effect.fnUntraced(function* (found) {
                const [fs, path, root] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.mapError(Effect.flatten(Effect.fromNullishOr(ADOBE[platform])), (cause) => new Cause.UnknownError(cause, `${platform} holds no per-user Adobe folder`))]);
                const { id, name, version, host } = yield* Effect.mapError(Schema.decodeEffect(Uxp)(found.text), (cause) => new Cause.UnknownError(cause, `${found.at} holds no UXP manifest`));
                const [registry, external, placed] = [path.join(root, 'UXP', 'PluginsInfo', 'v1', `${host.app}.json`), path.join(root, 'UXP', 'Plugins', 'External'), `$localPlugins/External/${id}_${version}`];
                const local = (at: string) => at.replace('$localPlugins', path.dirname(external));
                const before = yield* fs.readFileString(registry).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed('{"plugins":[]}')));
                const folders = yield* fs.readDirectory(external).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
                const rows = (yield* Effect.mapError(Schema.decodeUnknownEffect(Registry)(parse(before)), (cause) => new Cause.UnknownError(cause, `${registry} holds no plugins array`))).plugins;
                const held = rows.findIndex(({ pluginId }) => pluginId === id);
                const row = { ...rows[held], hostMinVersion: SemVer.format(UXP[host.app].since), name, path: placed, pluginId: id, status: 'enabled', type: 'uxp', versionString: version };
                const earlier = [...folders.filter((folder) => folder.startsWith(`${id}_`)).map((folder) => path.join(external, folder)), ...Array.fromNullishOr(rows[held]?.path).map(local), path.join(root, 'CEP', 'extensions', id)];
                yield* Effect.forEach(Array.dedupe(earlier), (at) => fs.remove(at, { recursive: true, force: true }), { discard: true });
                yield* Effect.forEach(
                    found.names.map((file) => [path.join(found.at, file), path.join(local(placed), file)] as const),
                    ([from, to]) => Effect.andThen(fs.makeDirectory(path.dirname(to), { recursive: true }), fs.copyFile(from, to)),
                    { discard: true },
                );
                yield* UXP[host.app].registry ? Effect.andThen(fs.makeDirectory(path.dirname(registry), { recursive: true }), fs.writeFileString(registry, applyEdits(before, modify(before, ['plugins', held === -1 ? rows.length : held], row, {})))) : Effect.void;
            }),
        },
    },
    {
        manifest: 'manifest.json',
        technology: 'chrome',
        archive: '.zip',
        output: '.',
        read: (text, file) => Effect.succeed(Option.map(Schema.decodeOption(Chrome)(text), ({ version }) => ({ name: basename(dirname(file), posix.extname(dirname(file))), version }))),
        write: (text, version, file) => Effect.as(exact(file, version), applyEdits(text, modify(text, ['version'], version, {}))),
        build: { executor: 'package', command: 'a zip archive of the extension folder', pack: ({ at, names }, file) => zipped(at, names, file) },
        publish: {
            store: 'https://chrome.google.com/webstore/devconsole',
            command: 'the Chrome Web Store API upload, then publish',
            credential: CREDENTIALS.chrome,
            held: (found, _store, token) =>
                Effect.flatMap(
                    webstore(found, token),
                    Option.match({
                        onNone: () => Effect.succeed(false),
                        onSome: ({ status }) => Effect.map(status, ({ publishedItemRevisionStatus, submittedItemRevisionStatus }) => [publishedItemRevisionStatus, submittedItemRevisionStatus].flatMap((revision) => revision?.distributionChannels ?? []).some(({ crxVersion }) => crxVersion === found.version)),
                    }),
                ),
            push: (found, store, token) =>
                Effect.flatMap(
                    webstore(found, token),
                    Option.match({
                        onNone: () => Effect.succeedSome(`${found.name} states no key in its manifest.json: its first version uploads by hand at ${store}, then the public key the dashboard shows goes in as key, and each later version publishes here`),
                        onSome: ({ upload, settled, publish }) =>
                            upload.pipe(
                                Effect.flatMap(({ uploadState }) => (uploadState === 'IN_PROGRESS' ? settled : Effect.succeed(uploadState))),
                                Effect.timeout(WEBSTORE.upload),
                                Effect.filterOrFail(
                                    (state) => state === 'SUCCEEDED',
                                    (state) => new Cause.UnknownError(state, `${found.at} uploads to ${store} in state ${state}, so nothing is submitted for review`),
                                ),
                                Effect.andThen(publish),
                                Effect.as(Option.none()),
                            ),
                    }),
                ),
        },
    },
    {
        manifest: '*.uproject',
        technology: 'unreal',
        output: 'Saved',
        platforms: Object.keys(UNREAL),
        standalone: true,
        hosted: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Project)(text), {
                onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not decode`),
                onSuccess: ({ Plugins, AdditionalPluginDirectories }) => Option.some({ name: basename(file, '.uproject'), folders: ['Plugins', ...(AdditionalPluginDirectories ?? [])], references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })) }),
            }),
        engine: { fields: Struct.keys(Association.fields) },
        build: { executor: 'unreal-build', command: 'UnrealBuildTool', outputs: ({ folders = [] }) => ['Binaries', ...folders.map((folder) => posix.join(folder, '**', 'Binaries'))] },
        operations: { check: { command: ['UnrealBuildTool', ...ANALYSIS].join(' '), options: { operation: 'check' } } },
    },
    {
        manifest: '*.uplugin',
        technology: 'unreal',
        archive: '.zip',
        output: 'Saved',
        platforms: Object.keys(UNREAL),
        standalone: true,
        hosted: true,
        read: (text, file) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not decode`),
                onSuccess: ({ VersionName, Installed, Plugins, nx }) =>
                    Option.some({
                        name: basename(file, '.uplugin'),
                        ...(VersionName !== undefined && { version: VersionName }),
                        ...(nx !== undefined && { packages: nx.packages }),
                        installed: Installed === true,
                        references: (Plugins ?? []).map(({ Name, Enabled }) => ({ name: Name, enabled: Enabled === true })),
                    }),
            }),
        write: (text, version, file) =>
            Effect.mapBoth(Schema.decodeEffect(Plugin)(text), {
                onFailure: (cause) => new Cause.UnknownError(cause, `${file} does not decode`),
                onSuccess: ({ Version }) => applyEdits(text, [...modify(text, ['VersionName'], version, {}), ...modify(text, ['Version'], (Version ?? 0) + 1, {})]),
            }),
        build: { executor: 'unreal-build', command: 'AutomationTool BuildPlugin -StrictIncludes', runtime: [PLATFORM.runtime] },
        operations: {
            test: { command: 'UnrealEditor-Cmd -ExecCmds="Automation RunTests ^<plug-in name>.;Quit" -nullrhi -unattended in each project loading the plug-in, then a read of each test state from the exported report', options: { operation: 'test' } },
            pack: { command: "the Fab store checks, then one zip per engine of the tracked sources with each installed package and that engine's EngineVersion", options: { operation: 'pack' } },
            install: { command: 'a copy of the BuildPlugin package into the Plugins folder of each project enabling the plug-in outside its folders', options: { operation: 'install' } },
        },
    },
];

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(Effect.match(engine(resolve(argv[2] ?? '')), { onFailure: () => 'absent', onSuccess: Struct.get('stamp') }).pipe(Effect.flatMap(Console.log), Effect.provide(layer)));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ANALYSIS, BUILDER, EXTENSION, engine, type Fields, FOLDERS, HOSTS, type Host, Linked, located, routed, zipped };
